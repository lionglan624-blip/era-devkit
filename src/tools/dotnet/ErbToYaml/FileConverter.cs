using System.Text;
using System.Text.RegularExpressions;
using ErbParser;
using ErbParser.Ast;

namespace ErbToYaml;

/// <summary>
/// Orchestrates single ERB file conversion to YAML
/// Feature 634 - AC#4, AC#15, AC#16
/// Pipeline: Read ERB → Parse → Extract convertible nodes → Convert → Validate → Write YAML
/// </summary>
public class FileConverter : IFileConverter
{
    private readonly IPathAnalyzer _pathAnalyzer;
    private readonly IPrintDataConverter _printDataConverter;
    private readonly IDatalistConverter _datalistConverter;
    private readonly ISelectCaseConverter? _selectCaseConverter;
    private readonly ILocalGateResolver? _localGateResolver;

    public FileConverter(
        IPathAnalyzer pathAnalyzer,
        IPrintDataConverter printDataConverter,
        IDatalistConverter datalistConverter,
        ISelectCaseConverter? selectCaseConverter = null,
        ILocalGateResolver? localGateResolver = null)
    {
        _pathAnalyzer = pathAnalyzer ?? throw new ArgumentNullException(nameof(pathAnalyzer));
        _printDataConverter = printDataConverter ?? throw new ArgumentNullException(nameof(printDataConverter));
        _datalistConverter = datalistConverter ?? throw new ArgumentNullException(nameof(datalistConverter));
        _selectCaseConverter = selectCaseConverter;
        _localGateResolver = localGateResolver;
    }

    /// <summary>
    /// Constructor with explicit TalentCsvLoader for condition parsing
    /// </summary>
    public FileConverter(
        IPathAnalyzer pathAnalyzer,
        IPrintDataConverter printDataConverter,
        IDatalistConverter datalistConverter,
        TalentCsvLoader talentLoader,
        ISelectCaseConverter? selectCaseConverter = null,
        ILocalGateResolver? localGateResolver = null)
        : this(pathAnalyzer, printDataConverter, datalistConverter, selectCaseConverter, localGateResolver)
    {
    }

    /// <summary>
    /// Convert a single ERB file to YAML output(s)
    /// Handles IF-wrapped PRINTDATA blocks with conditional structure preservation (AC#15)
    /// Validates generated YAML against schema (AC#16)
    /// </summary>
    /// <param name="erbFilePath">Path to input ERB file</param>
    /// <param name="outputDirectory">Directory for output YAML files (pre-computed by BatchConverter)</param>
    /// <returns>List of conversion results (one per generated YAML file)</returns>
    public async Task<List<ConversionResult>> ConvertAsync(string erbFilePath, string outputDirectory)
    {
        var results = new List<ConversionResult>();

        try
        {
            // 1. Extract character/situation from file path
            var (character, situation) = _pathAnalyzer.Extract(erbFilePath);

            // 2. Read ERB file content
            var content = await File.ReadAllTextAsync(erbFilePath, Encoding.UTF8);

            // 3. Parse with ErbParser
            var parser = new ErbParser.ErbParser();
            var astNodes = parser.ParseString(content, erbFilePath);

            // F761: Apply LOCAL gate resolution preprocessing
            if (_localGateResolver != null)
            {
                astNodes = _localGateResolver.Resolve(astNodes);
            }

            // 4. Find all convertible top-level nodes
            // Track (node, comId) pairs; comId is null for top-level nodes without function context
            var convertibleNodes = new List<(AstNode Node, int? ComId)>();

            foreach (var node in astNodes)
            {
                if (node is DatalistNode || node is PrintDataNode)
                {
                    convertibleNodes.Add((node, null));
                }
                else if (node is SelectCaseNode)
                {
                    // F765: SELECTCASE is a directly-convertible top-level type
                    convertibleNodes.Add((node, null));
                }
                else if (node is IfNode ifNode)
                {
                    // Check if IF contains PRINTDATA/DATALIST (AC#15: conditional preservation)
                    if (ContainsConvertibleContent(ifNode))
                    {
                        convertibleNodes.Add((ifNode, null));
                    }
                }
                else if (node is FunctionDefNode functionNode)
                {
                    // F764: Process FunctionDefNode for both backward-compat and EVENT conversion
                    // Apply LOCAL envelope stripping first
                    var resolvedBody = _localGateResolver != null
                        ? _localGateResolver.Resolve(functionNode.Body)
                        : functionNode.Body;

                    // Extract COM ID from function name (pattern: COM_K{N}_{comId} or COM_K{N}_{comId}_{subIndex})
                    var comId = ExtractComId(functionNode.FunctionName);

                    // Add backward-compatible nodes (DATALIST, PRINTDATA, IF with convertible content, SELECTCASE)
                    foreach (var bodyNode in resolvedBody)
                    {
                        if (bodyNode is DatalistNode || bodyNode is PrintDataNode)
                        {
                            convertibleNodes.Add((bodyNode, comId));
                        }
                        else if (bodyNode is SelectCaseNode)
                        {
                            convertibleNodes.Add((bodyNode, comId));
                        }
                        else if (bodyNode is IfNode bodyIfNode && ContainsConvertibleContent(bodyIfNode))
                        {
                            convertibleNodes.Add((bodyIfNode, comId));
                        }
                    }

                    // Also attempt EVENT function conversion
                    var eventOutputs = ConvertEventFunction(functionNode, resolvedBody, character, situation);
                    foreach (var (yaml, filename) in eventOutputs)
                    {
                        // Validate before writing
                        try
                        {
                            _datalistConverter.ValidateYaml(yaml);
                        }
                        catch (SchemaValidationException ex)
                        {
                            results.Add(new ConversionResult(
                                Success: false,
                                FilePath: erbFilePath,
                                Error: $"Schema validation failed for {filename}: {ex.Message}"
                            ));
                            continue;
                        }

                        // Ensure output directory exists
                        Directory.CreateDirectory(outputDirectory);

                        var yamlPath = Path.Combine(outputDirectory, filename);
                        await File.WriteAllTextAsync(yamlPath, yaml, Encoding.UTF8);

                        results.Add(new ConversionResult(
                            Success: true,
                            FilePath: erbFilePath,
                            Error: null
                        ));
                    }
                }
            }

            // 5. Convert each node to YAML
            int nodeIndex = 0;
            foreach (var (node, comId) in convertibleNodes)
            {
                string yaml;
                string baseFilename = situation;

                // Determine YAML content based on node type
                if (node is DatalistNode datalistNode)
                {
                    yaml = _datalistConverter.Convert(datalistNode, character, situation);
                }
                else if (node is PrintDataNode printDataNode)
                {
                    // Check if PrintDataNode has any content
                    var dataforms = printDataNode.GetDataForms().ToList();
                    if (dataforms.Count == 0)
                    {
                        // Empty PRINTDATA - treat as validation failure
                        results.Add(new ConversionResult(
                            Success: false,
                            FilePath: erbFilePath,
                            Error: "Empty PRINTDATA block - no content to convert"
                        ));
                        continue;
                    }

                    yaml = _printDataConverter.Convert(printDataNode, character, situation);
                }
                else if (node is SelectCaseNode selectCaseNode && _selectCaseConverter != null)
                {
                    // F765: SELECTCASE to YAML conversion
                    yaml = _selectCaseConverter.Convert(selectCaseNode, character, situation);
                }
                else if (node is IfNode ifNode)
                {
                    // AC#15: Conditional preservation - IF wrapping PRINTDATA/DATALIST
                    yaml = ConvertConditionalNode(ifNode, character, situation);
                }
                else
                {
                    continue; // Skip unknown node types
                }

                // 6. Validate YAML against schema (AC#16)
                try
                {
                    _datalistConverter.ValidateYaml(yaml);
                }
                catch (SchemaValidationException ex)
                {
                    results.Add(new ConversionResult(
                        Success: false,
                        FilePath: erbFilePath,
                        Error: $"Schema validation failed: {ex.Message}"
                    ));
                    continue; // Skip writing invalid YAML
                }

                // 6.5. Inject com_id metadata into YAML after validation passes
                if (comId.HasValue)
                {
                    yaml = InjectComId(yaml, comId.Value);
                }

                // 7. Ensure output directory exists
                Directory.CreateDirectory(outputDirectory);

                // 8. Determine output filename (with index suffix if multiple nodes)
                string yamlFilename;
                if (convertibleNodes.Count > 1)
                {
                    yamlFilename = $"{baseFilename}_{nodeIndex}.yaml";
                }
                else
                {
                    yamlFilename = $"{baseFilename}.yaml";
                }

                var yamlPath = Path.Combine(outputDirectory, yamlFilename);

                // 9. Write YAML to file
                await File.WriteAllTextAsync(yamlPath, yaml, Encoding.UTF8);

                results.Add(new ConversionResult(
                    Success: true,
                    FilePath: erbFilePath,
                    Error: null
                ));

                nodeIndex++;
            }

            // If no convertible nodes found, treat as success with warning
            if (results.Count == 0)
            {
                results.Add(new ConversionResult(
                    Success: true,
                    FilePath: erbFilePath,
                    Error: null
                ));
            }

            return results;
        }
        catch (Exception ex)
        {
            results.Add(new ConversionResult(
                Success: false,
                FilePath: erbFilePath,
                Error: $"Conversion failed: {ex.Message}"
            ));
            return results;
        }
    }

    /// <summary>
    /// Check if IfNode contains PRINTDATA or DATALIST content in any branch (recursive)
    /// </summary>
    private bool ContainsConvertibleContent(IfNode ifNode)
    {
        return HasConvertibleContentRecursive(ifNode.Body)
            || ifNode.ElseIfBranches.Any(e => HasConvertibleContentRecursive(e.Body))
            || (ifNode.ElseBranch != null && HasConvertibleContentRecursive(ifNode.ElseBranch.Body));
    }

    private bool HasConvertibleContentRecursive(List<AstNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is PrintDataNode || node is DatalistNode)
                return true;

            if (node is IfNode nested && ContainsConvertibleContent(nested))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Convert IfNode wrapping PRINTDATA/DATALIST to YAML with conditional branches
    /// AC#15: Conditional structure preservation
    /// Pattern matches DatalistConverter.ConvertConditionalBranches
    /// </summary>
    private string ConvertConditionalNode(IfNode ifNode, string character, string situation)
    {
        var branches = new List<object>();

        // Process IF branch
        var ifBranch = ProcessConditionalBranch(ifNode.Condition, ifNode.Body, character, situation);
        if (ifBranch != null)
            branches.Add(ifBranch);

        // Process ELSEIF branches
        foreach (var elseIfBranch in ifNode.ElseIfBranches)
        {
            var branch = ProcessConditionalBranch(elseIfBranch.Condition, elseIfBranch.Body, character, situation);
            if (branch != null)
                branches.Add(branch);
        }

        // Process ELSE branch (no condition)
        if (ifNode.ElseBranch != null)
        {
            var elseBranch = ProcessConditionalBranch(null, ifNode.ElseBranch.Body, character, situation);
            if (elseBranch != null)
                branches.Add(elseBranch);
        }

        // Build YAML structure
        var dialogueData = new Dictionary<string, object>
        {
            { "character", character },
            { "situation", situation },
            { "entries", BranchesToEntriesConverter.Convert(branches) }
        };

        var serializer = new YamlDotNet.Serialization.SerializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();

        return serializer.Serialize(dialogueData);
    }

    /// <summary>
    /// Process a single conditional branch (IF, ELSEIF, or ELSE)
    /// Extracts dialogue lines from PRINTDATA/DATALIST nodes in branch body
    /// </summary>
    private Dictionary<string, object>? ProcessConditionalBranch(
        string? condition,
        List<AstNode> body,
        string character,
        string situation)
    {
        var lines = new List<string>();
        string? variantToUse = null;

        // Accumulate intro lines from PRINTFORM[WL] before PRINTDATA
        var introLines = new List<string>();
        bool hasEncounteredPrintData = false;

        // Extract lines from PRINTDATA or DATALIST nodes in body
        foreach (var node in body)
        {
            // Collect intro lines BEFORE first PRINTDATA/DATALIST
            if (!hasEncounteredPrintData && node is PrintformNode printform)
            {
                if (introLines.Count > 0 && printform.Variant == "PRINTFORMW")
                {
                    // W suffix = continuation (no newline), append to last line
                    introLines[introLines.Count - 1] += printform.Content;
                }
                else
                {
                    // New intro line
                    introLines.Add(printform.Content);
                }
                continue;
            }

            if (node is PrintDataNode printData)
            {
                hasEncounteredPrintData = true;

                if (variantToUse == null && printData.Variant != "PRINTDATA")
                {
                    variantToUse = printData.Variant;
                }

                // Extract DataformNodes from PrintDataNode
                var dataforms = printData.GetDataForms();
                foreach (var dataform in dataforms)
                {
                    foreach (var arg in dataform.Arguments)
                    {
                        if (arg is string line)
                        {
                            lines.Add(line);
                            break;
                        }
                    }
                }
            }
            else if (node is DatalistNode datalist)
            {
                hasEncounteredPrintData = true;

                // Extract DataformNodes from DatalistNode
                foreach (var dataform in datalist.DataForms)
                {
                    foreach (var arg in dataform.Arguments)
                    {
                        if (arg is string line)
                        {
                            lines.Add(line);
                            break;
                        }
                    }
                }
            }
        }

        // Prepend intro lines to dialogue lines[]
        if (introLines.Count > 0)
        {
            if (lines.Count > 0)
            {
                lines.InsertRange(0, introLines);
            }
            else
            {
                // Handle intro-only branches (no PRINTDATA)
                lines.AddRange(introLines);
            }
        }

        // If no lines found, return null (skip this branch)
        if (lines.Count == 0)
            return null;

        var branch = new Dictionary<string, object>
        {
            { "lines", lines }
        };

        if (variantToUse != null)
        {
            var displayMode = DisplayModeMapper.MapVariant(variantToUse);
            if (displayMode != null)
            {
                branch["displayMode"] = displayMode;
            }
        }

        // Add condition if present (mirrors DatalistConverter.ProcessBranch)
        if (!string.IsNullOrWhiteSpace(condition))
        {
            var conditionObj = _datalistConverter.ParseCondition(condition);
            if (conditionObj != null)
            {
                branch["condition"] = conditionObj;
            }
        }

        return branch;
    }

    /// <summary>
    /// Convert EVENT function to YAML output(s)
    /// Filters SELECTCASE branches, extracts ARG conditions, maps PRINTFORM displayMode
    /// F764 - Task 3
    /// </summary>
    /// <param name="function">Function definition node</param>
    /// <param name="resolvedBody">Function body with LOCAL envelope already stripped by caller</param>
    /// <param name="character">Character identifier</param>
    /// <param name="situation">Situation code</param>
    /// <returns>List of (YAML content, filename) pairs; empty if no convertible content</returns>
    private List<(string Yaml, string Filename)> ConvertEventFunction(
        FunctionDefNode function,
        List<AstNode> resolvedBody,
        string character,
        string situation)
    {
        // Step 1: Check for SELECTCASE-only content (early exit optimization)
        if (IsSelectCaseOnly(resolvedBody))
        {
            return new List<(string Yaml, string Filename)>();
        }

        // Step 2: Extract IF ARG branches
        var branches = new List<object>();
        foreach (var node in resolvedBody)
        {
            if (node is IfNode ifNode)
            {
                // Process IF body + all ELSEIF branches
                var allBranches = new List<(string Condition, List<AstNode> Body)>
                {
                    (ifNode.Condition, ifNode.Body)
                };
                if (ifNode.ElseIfBranches != null)
                {
                    foreach (var elseIf in ifNode.ElseIfBranches)
                        allBranches.Add((elseIf.Condition, elseIf.Body));
                }

                foreach (var (branchCondition, branchBody) in allBranches)
                {
                    // Parse condition
                    var condition = _datalistConverter.ParseCondition(branchCondition);
                    if (condition == null || !condition.ContainsKey("ARG"))
                        continue;

                    // Skip if branch contains SELECTCASE
                    if (ContainsSelectCase(branchBody))
                        continue;

                    // Extract PRINTFORM lines
                    var lines = new List<string>();
                    string? displayMode = null;
                    foreach (var bodyNode in branchBody)
                    {
                        if (bodyNode is PrintformNode printform)
                        {
                            lines.Add(printform.Content);
                            displayMode = DisplayModeMapper.MapVariant(printform.Variant);
                        }
                        if (bodyNode is ReturnNode)
                            break; // Stop at RETURN
                    }

                    if (lines.Count == 0)
                        continue; // No dialogue content

                    // Build branch
                    var branchDict = new Dictionary<string, object>
                    {
                        { "condition", condition },
                        { "lines", lines }
                    };
                    if (displayMode != null)
                    {
                        branchDict["displayMode"] = displayMode;
                    }
                    branches.Add(branchDict);
                }
            }
        }

        if (branches.Count == 0)
        {
            return new List<(string Yaml, string Filename)>();
        }

        // Step 3: Build YAML
        var dialogueData = new Dictionary<string, object>
        {
            { "character", character },
            { "situation", situation },
            { "entries", BranchesToEntriesConverter.Convert(branches) }
        };

        var serializer = new YamlDotNet.Serialization.SerializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
            .Build();

        var yaml = serializer.Serialize(dialogueData);

        // Step 4: Return per-function output
        // Extract short function name by removing "KOJO_EVENT_" prefix if present
        // KOJO_EVENT_K1_0 → K1_0
        var shortFunctionName = function.FunctionName;
        if (shortFunctionName.StartsWith("KOJO_EVENT_", StringComparison.OrdinalIgnoreCase))
        {
            shortFunctionName = shortFunctionName.Substring("KOJO_EVENT_".Length);
        }
        var yamlFilename = $"{situation}_{shortFunctionName}.yaml";
        return new List<(string Yaml, string Filename)> { (yaml, yamlFilename) };
    }

    /// <summary>
    /// Check if branch body contains SELECTCASE
    /// </summary>
    private bool ContainsSelectCase(List<AstNode> nodes)
    {
        return nodes.Any(n => n is SelectCaseNode);
    }

    /// <summary>
    /// Early-exit optimization: detects functions with no convertible branches
    /// Returns true when body has no IfNode (meaning no IF ARG branches to extract)
    /// </summary>
    private bool IsSelectCaseOnly(List<AstNode> body)
    {
        return body.All(n => n is SelectCaseNode || n is ReturnNode || n is AssignmentNode || n is PrintformNode);
    }

    /// <summary>
    /// Extract COM ID from function name.
    /// Delegates to ComIdInjector.ExtractComId for consistent pattern matching.
    /// </summary>
    private static int? ExtractComId(string functionName)
    {
        return ComIdInjector.ExtractComId(functionName);
    }

    /// <summary>
    /// Inject com_id field into YAML content after the situation line.
    /// Inserts "com_id: {value}" as a top-level field between situation and entries.
    /// </summary>
    private static string InjectComId(string yaml, int comId)
    {
        // Insert com_id after the "situation:" line
        // Use line-ending-agnostic approach: find "situation:" line and insert after it
        var lines = yaml.Split('\n');
        var result = new System.Text.StringBuilder();
        bool injected = false;

        foreach (var line in lines)
        {
            result.Append(line);
            result.Append('\n');

            if (!injected && line.TrimEnd('\r').StartsWith("situation:"))
            {
                result.Append($"com_id: {comId}\n");
                injected = true;
            }
        }

        return result.ToString();
    }
}
