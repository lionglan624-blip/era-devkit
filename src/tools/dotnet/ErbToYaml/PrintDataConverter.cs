using ErbParser.Ast;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ErbToYaml;

/// <summary>
/// Converts PrintDataNode to YAML dialogue format
/// Feature 634 - AC#13
/// Uses PrintDataNode.GetDataForms() to extract DataformNode list
/// </summary>
public class PrintDataConverter : IPrintDataConverter
{
    private readonly ISerializer _yamlSerializer;

    public PrintDataConverter()
    {
        _yamlSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// Convert a PrintDataNode to YAML dialogue format
    /// Handles simple PRINTDATA blocks without conditionals
    /// Note: Conditionals wrapping PRINTDATA are handled by FileConverter
    /// </summary>
    /// <param name="printData">PrintData AST node</param>
    /// <param name="character">Character identifier</param>
    /// <param name="situation">Situation code</param>
    /// <returns>YAML string conforming to dialogue-schema.json</returns>
    public string Convert(PrintDataNode printData, string character, string situation)
    {
        if (printData == null)
            throw new ArgumentNullException(nameof(printData));

        // Extract all DataformNodes using GetDataForms() helper
        var dataforms = printData.GetDataForms().ToList();

        // Convert DataformNodes to dialogue lines
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

        // Build YAML structure matching dialogue-schema.json
        // Simple PRINTDATA (no conditionals) produces single entry with no condition
        // Create a temporary branches structure to convert to entries format
        var branch = new Dictionary<string, object>
        {
            { "lines", lines }
        };

        var displayMode = DisplayModeMapper.MapVariant(printData.Variant);
        if (displayMode != null)
        {
            branch["displayMode"] = displayMode;
        }

        var branches = new List<object> { branch };

        // Convert branches to entries using canonical format
        var entries = BranchesToEntriesConverter.Convert(branches);

        var dialogueData = new Dictionary<string, object>
        {
            { "character", character },
            { "situation", situation },
            { "entries", entries }
        };

        return _yamlSerializer.Serialize(dialogueData);
    }
}
