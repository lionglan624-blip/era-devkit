using ErbParser;
using ErbParser.Ast;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KojoComparer;

/// <summary>
/// Injects intro lines (PRINTFORM/PRINTFORMW before PRINTDATA) into existing YAML kojo files.
/// Reuses F748 intro line extraction logic from ErbToYaml.
/// </summary>
public class IntroLineInjector
{
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public IntroLineInjector()
    {
        _serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// Injects intro lines from ERB file into corresponding YAML file.
    /// </summary>
    /// <param name="erbFilePath">Path to source ERB file</param>
    /// <param name="yamlFilePath">Path to target YAML file</param>
    public async Task InjectAsync(string erbFilePath, string yamlFilePath)
    {
        if (!File.Exists(erbFilePath))
            throw new FileNotFoundException($"ERB file not found: {erbFilePath}");

        if (!File.Exists(yamlFilePath))
            throw new FileNotFoundException($"YAML file not found: {yamlFilePath}");

        // Parse ERB to extract intro lines per branch
        var introBranches = ExtractIntroBranchesFromErb(erbFilePath);

        if (introBranches.Count == 0)
        {
            Console.WriteLine($"No intro lines found in {erbFilePath}");
            return;
        }

        // Load YAML
        var yamlContent = await File.ReadAllTextAsync(yamlFilePath);
        var kojoData = _deserializer.Deserialize<KojoFileData>(yamlContent);

        if (kojoData?.Branches == null || kojoData.Branches.Count == 0)
            throw new InvalidDataException($"No branches found in YAML file: {yamlFilePath}");

        // Inject intro lines into matching branches (idempotent: skip if already injected)
        int injectedCount = 0;
        for (int i = 0; i < Math.Min(introBranches.Count, kojoData.Branches.Count); i++)
        {
            var introLines = introBranches[i];
            if (introLines.Count > 0)
            {
                var branch = kojoData.Branches[i];
                branch.Lines ??= new List<string>();

                // Skip if intro already exists (idempotency check)
                if (branch.Lines.Count > 0 && branch.Lines[0] == introLines[0])
                    continue;

                // Insert intro lines at position 0
                branch.Lines.InsertRange(0, introLines);
                injectedCount++;
            }
        }

        // Save modified YAML
        var modifiedYaml = _serializer.Serialize(kojoData);
        await File.WriteAllTextAsync(yamlFilePath, modifiedYaml);

        Console.WriteLine($"Injected intro lines into {injectedCount} branches in {yamlFilePath}");
    }

    /// <summary>
    /// Batch injects intro lines for all ERB-YAML pairs using FileDiscovery mapping.
    /// </summary>
    /// <param name="erbDirectory">Directory containing ERB files</param>
    /// <param name="yamlDirectory">Directory containing YAML files</param>
    /// <param name="mapFilePath">Path to com_file_map.json</param>
    public async Task BatchInjectAsync(string erbDirectory, string yamlDirectory, string mapFilePath)
    {
        if (!Directory.Exists(erbDirectory))
            throw new DirectoryNotFoundException($"ERB directory not found: {erbDirectory}");

        if (!Directory.Exists(yamlDirectory))
            throw new DirectoryNotFoundException($"YAML directory not found: {yamlDirectory}");

        // Use FileDiscovery to get ERB-YAML mappings
        var discovery = new FileDiscovery(erbDirectory, yamlDirectory, mapFilePath);
        var testCases = discovery.DiscoverTestCases();

        // Group by ERB file to avoid duplicate processing
        var erbToYamlMap = testCases
            .GroupBy(tc => tc.ErbFile)
            .ToDictionary(g => g.Key, g => g.Select(tc => tc.YamlFile).Distinct().ToList());

        int processedCount = 0;
        int errorCount = 0;

        foreach (var (erbFile, yamlFiles) in erbToYamlMap)
        {
            foreach (var yamlFile in yamlFiles)
            {
                try
                {
                    await InjectAsync(erbFile, yamlFile);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR processing {erbFile} -> {yamlFile}: {ex.Message}");
                    errorCount++;
                }
            }
        }

        Console.WriteLine($"\nBatch injection complete: {processedCount} processed, {errorCount} errors");
    }

    /// <summary>
    /// Extracts intro lines from ERB file using F748 logic.
    /// Returns list of intro line lists (one per branch).
    /// Preserves TALENT branch order: IF → ELSEIF(s) → ELSE.
    /// Pattern from FileConverter.cs lines 219-242, 272-349.
    /// Uses AstExtensions.OfTypeFlatten to traverse FunctionDefNode.Body (F764).
    /// </summary>
    private List<List<string>> ExtractIntroBranchesFromErb(string erbFilePath)
    {
        var parser = new ErbParser.ErbParser();
        var ast = parser.Parse(erbFilePath);

        var result = new List<List<string>>();

        // Find top-level IfNode that represents TALENT branching (not guard clauses)
        // TALENT branching IF should have:
        // 1. "TALENT:" in its condition
        // 2. PrintDataNode in its body (indicates main content, not just guard logic)
        // NOTE: Use OfTypeFlatten to traverse FunctionDefNode.Body (introduced in F764)
        var ifNode = ast.OfTypeFlatten<IfNode>().FirstOrDefault(node =>
            node.Condition.Contains("TALENT:") && ContainsPrintData(node.Body));

        if (ifNode == null)
        {
            // No TALENT branching - single branch case
            // Use flattened AST to search for intro lines (may be inside FunctionDefNode)
            var flattenedAst = ast.FlattenFunctionBodies().ToList();
            var introLines = ExtractIntroFromScope(flattenedAst);
            result.Add(introLines);
            return result;
        }

        // Process IF branch (index 0) → YAML branches[0]
        result.Add(ExtractIntroFromScope(ifNode.Body));

        // Process ELSEIF branches in order (index 1, 2, ...) → YAML branches[1], branches[2], ...
        foreach (var elseIfBranch in ifNode.ElseIfBranches)
        {
            result.Add(ExtractIntroFromScope(elseIfBranch.Body));
        }

        // Process ELSE branch (last index) → YAML branches[last] (empty condition)
        if (ifNode.ElseBranch != null)
        {
            result.Add(ExtractIntroFromScope(ifNode.ElseBranch.Body));
        }

        return result;
    }

    /// <summary>
    /// Checks if a scope contains PrintDataNode (indicating main content branching).
    /// </summary>
    private bool ContainsPrintData(List<AstNode> nodes)
    {
        return nodes.OfType<PrintDataNode>().Any();
    }

    /// <summary>
    /// Extracts intro lines from a scope (list of nodes).
    /// Intro lines are PRINTFORM/PRINTFORMW nodes BEFORE the first PrintDataNode or DatalistNode.
    /// Handles PRINTFORMW continuation (appends to previous line).
    /// Pattern from FileConverter.cs lines 272-349.
    /// </summary>
    private List<string> ExtractIntroFromScope(List<AstNode> nodes)
    {
        var result = new List<string>();

        // Scan nodes until we hit PRINTDATA or DATALIST
        foreach (var node in nodes)
        {
            if (node is PrintDataNode || node is DatalistNode)
                break; // Stop at first PRINTDATA/DATALIST

            if (node is PrintformNode printform)
            {
                if (result.Count > 0 && printform.Variant == "PRINTFORMW")
                {
                    // W suffix = continuation (no newline), append to last line
                    result[result.Count - 1] += printform.Content;
                }
                else
                {
                    // New intro line
                    result.Add(printform.Content);
                }
            }
        }

        return result;
    }
}
