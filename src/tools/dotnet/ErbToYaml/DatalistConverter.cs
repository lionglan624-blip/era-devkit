using System;
using System.Collections.Generic;
using System.Linq;
using ErbParser;
using ErbParser.Ast;
using Newtonsoft.Json;
using NJsonSchema;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ErbToYaml;

/// <summary>
/// Converts DATALIST blocks from ERB AST to YAML dialogue files
/// Feature 349 - AC#1, AC#2 implementation
/// Feature 361 - AC#1 schema validation
/// </summary>
public class DatalistConverter : IDatalistConverter
{
    private readonly TalentCsvLoader _talentLoader;
    private readonly ConditionExtractor _conditionExtractor;
    private readonly ISerializer _yamlSerializer;
    private readonly JsonSchema? _schema;
    private readonly IDimConstResolver? _dimConstResolver;
    private readonly IConditionSerializer _conditionSerializer;

    private readonly Dictionary<Type, string> _variableTypePrefixes = new()
    {
        { typeof(CflagRef), "CFLAG" },
        { typeof(TcvarRef), "TCVAR" },
        { typeof(EquipRef), "EQUIP" },
        { typeof(ItemRef), "ITEM" },
        { typeof(StainRef), "STAIN" },
        { typeof(MarkRef), "MARK" },
        { typeof(ExpRef), "EXP" },
        { typeof(NowexRef), "NOWEX" },
        { typeof(AblRef), "ABL" },
        { typeof(FlagRef), "FLAG" },
        { typeof(TflagRef), "TFLAG" },
        { typeof(TequipRef), "TEQUIP" },
        { typeof(PalamRef), "PALAM" },
    };

    public DatalistConverter(string talentCsvPath, IDimConstResolver? dimConstResolver = null)
    {
        if (talentCsvPath == null)
            throw new ArgumentNullException(nameof(talentCsvPath));

        _talentLoader = new TalentCsvLoader(talentCsvPath);
        _conditionExtractor = new ConditionExtractor();
        _dimConstResolver = dimConstResolver;
        _conditionSerializer = new ConditionSerializer(_talentLoader, _dimConstResolver, _variableTypePrefixes);

        _yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        _schema = null;
    }

    /// <summary>
    /// Constructor with schema validation support
    /// Feature 361 - AC#1
    /// </summary>
    /// <param name="talentCsvPath">Path to Talent.csv file</param>
    /// <param name="schemaPath">Path to dialogue-schema.json file</param>
    public DatalistConverter(string talentCsvPath, string schemaPath, IDimConstResolver? dimConstResolver = null)
    {
        if (talentCsvPath == null)
            throw new ArgumentNullException(nameof(talentCsvPath));
        if (schemaPath == null)
            throw new ArgumentNullException(nameof(schemaPath));

        _talentLoader = new TalentCsvLoader(talentCsvPath);
        _conditionExtractor = new ConditionExtractor();
        _dimConstResolver = dimConstResolver;
        _conditionSerializer = new ConditionSerializer(_talentLoader, _dimConstResolver, _variableTypePrefixes);

        _yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        // Load schema from file
        _schema = JsonSchema.FromFileAsync(schemaPath).Result;
    }

    /// <summary>
    /// Convert a DATALIST node to YAML dialogue format
    /// Handles both simple (DataForms only) and conditional (IF/ELSEIF/ELSE) DATALIST
    /// </summary>
    /// <param name="datalist">DATALIST AST node</param>
    /// <param name="character">Character identifier</param>
    /// <param name="situation">Situation code (e.g., K4, K100)</param>
    /// <returns>YAML string conforming to dialogue-schema.json</returns>
    public string Convert(DatalistNode datalist, string character, string situation)
    {
        if (datalist == null)
            throw new ArgumentNullException(nameof(datalist));

        List<object> branches;

        // Check if DATALIST has conditional branches (IF/ELSEIF/ELSE)
        if (datalist.ConditionalBranches != null && datalist.ConditionalBranches.Count > 0)
        {
            // Convert conditional branches
            branches = ConvertConditionalBranches(datalist.ConditionalBranches);
        }
        else
        {
            // Simple DATALIST with only DataForms
            branches = ConvertSimpleBranches(datalist.DataForms);
        }

        // Build YAML structure matching dialogue-schema.json
        var dialogueData = new Dictionary<string, object>
        {
            { "character", character },
            { "situation", situation },
            { "entries", BranchesToEntriesConverter.Convert(branches) }
        };

        return _yamlSerializer.Serialize(dialogueData);
    }

    /// <summary>
    /// Convert simple DATAFORM nodes to branches (no conditions)
    /// </summary>
    private List<object> ConvertSimpleBranches(List<DataformNode> dataforms)
    {
        var lines = new List<string>();

        foreach (var dataform in dataforms)
        {
            // Extract string content from DATAFORM arguments
            foreach (var arg in dataform.Arguments)
            {
                if (arg is string line)
                {
                    lines.Add(line);
                    break; // Only take the first string argument
                }
            }
        }

        return new List<object>
        {
            new Dictionary<string, object>
            {
                { "lines", lines }
            }
        };
    }

    /// <summary>
    /// Convert conditional branches (IF/ELSEIF/ELSE) to YAML branches with conditions
    /// AC#2 implementation
    /// </summary>
    private List<object> ConvertConditionalBranches(List<IfNode> ifNodes)
    {
        var branches = new List<object>();

        foreach (var ifNode in ifNodes)
        {
            // Process IF branch
            var ifBranch = ProcessBranch(ifNode.Condition, ifNode.Body);
            branches.Add(ifBranch);

            // Process ELSEIF branches
            foreach (var elseIfBranch in ifNode.ElseIfBranches)
            {
                var branch = ProcessBranch(elseIfBranch.Condition, elseIfBranch.Body);
                branches.Add(branch);
            }

            // Process ELSE branch (no condition)
            if (ifNode.ElseBranch != null)
            {
                var elseBranch = ProcessBranch(null, ifNode.ElseBranch.Body);
                branches.Add(elseBranch);
            }
        }

        return branches;
    }

    /// <summary>
    /// Process a single branch (IF, ELSEIF, or ELSE)
    /// </summary>
    private Dictionary<string, object> ProcessBranch(string? condition, List<AstNode> body)
    {
        var branch = new Dictionary<string, object>();

        // Extract dialogue lines from body
        var lines = new List<string>();
        foreach (var node in body)
        {
            if (node is DataformNode dataform)
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

        branch["lines"] = lines;

        // Parse and add condition if present
        if (!string.IsNullOrWhiteSpace(condition))
        {
            var conditionObj = ParseCondition(condition);
            if (conditionObj != null)
            {
                branch["condition"] = conditionObj;
            }
        }

        return branch;
    }

    /// <summary>
    /// Parse condition string and convert to dialogue-schema.json format
    /// AC#2: Transform TALENT:恋慕 → { "TALENT": { "3": { "ne": 0 } } }
    /// F755: Now supports compound conditions via ConditionExtractor
    /// F765: Delegates to IConditionSerializer (facade pattern)
    /// </summary>
    public Dictionary<string, object>? ParseCondition(string condition)
    {
        var parsedCondition = _conditionExtractor.Extract(condition);

        if (parsedCondition == null)
        {
            Console.Error.WriteLine($"Warning: Could not parse condition: {condition}");
            return new Dictionary<string, object>();
        }

        return _conditionSerializer.ConvertConditionToYaml(parsedCondition) ?? new Dictionary<string, object>();
    }


    /// <summary>
    /// Validate YAML content against loaded schema
    /// Feature 361 - AC#1
    /// </summary>
    /// <param name="yaml">YAML content to validate</param>
    /// <exception cref="SchemaValidationException">Thrown when validation fails</exception>
    public void ValidateYaml(string yaml)
    {
        if (_schema == null)
        {
            // No schema loaded - skip validation
            return;
        }

        if (string.IsNullOrWhiteSpace(yaml))
        {
            throw new SchemaValidationException("YAML content is empty");
        }

        try
        {
            // Convert YAML to JSON for schema validation
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yamlObject = deserializer.Deserialize(new System.IO.StringReader(yaml));
            var json = JsonConvert.SerializeObject(yamlObject);

            // Validate against schema
            var errors = _schema.Validate(json);

            if (errors.Count > 0)
            {
                var errorMessages = errors.Select(e => $"  - {e.Path}: {e.Kind}").ToList();
                throw new SchemaValidationException(
                    $"YAML validation failed:\n{string.Join("\n", errorMessages)}");
            }
        }
        catch (SchemaValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SchemaValidationException($"YAML validation error: {ex.Message}", ex);
        }
    }

}
