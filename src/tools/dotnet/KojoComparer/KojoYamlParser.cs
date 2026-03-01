using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KojoComparer;

/// <summary>
/// Custom YAML parser for Kojo dialogue files.
/// Handles the branches-based format used by Kojo YAML files.
/// </summary>
public class KojoYamlParser
{
    private readonly IDeserializer _deserializer;

    public KojoYamlParser()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// Parses a Kojo YAML file and returns the dialogue lines as a list of strings.
    /// For now, returns lines from the first branch with an empty or matching condition.
    /// </summary>
    /// <param name="yamlFilePath">Path to the YAML file</param>
    /// <returns>List of dialogue lines</returns>
    public List<string> Parse(string yamlFilePath)
    {
        if (!File.Exists(yamlFilePath))
            throw new FileNotFoundException($"Kojo YAML file not found: {yamlFilePath}");

        var yaml = File.ReadAllText(yamlFilePath);
        var kojoData = _deserializer.Deserialize<KojoFileData>(yaml);

        if (kojoData?.Branches == null || kojoData.Branches.Count == 0)
            throw new InvalidDataException($"No branches found in Kojo YAML file: {yamlFilePath}");

        // For now, return the first branch
        // Condition evaluation is intentionally deferred to F709 (multi-state testing per COM)
        var firstBranch = kojoData.Branches[0];
        return firstBranch.Lines ?? new List<string>();
    }
}

/// <summary>
/// Data model for Kojo YAML files (branches format).
/// </summary>
internal class KojoFileData
{
    public string? Character { get; set; }
    public string? Situation { get; set; }
    public int? ComId { get; set; }
    public List<KojoBranch> Branches { get; set; } = new();
}

/// <summary>
/// Data model for a single branch in Kojo YAML.
/// </summary>
internal class KojoBranch
{
    public List<string>? Lines { get; set; }
    public Dictionary<string, object>? Condition { get; set; }
}
