using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KojoQualityValidator;

public record QualityRule(int MinEntries = 4, int MinLinesPerEntry = 4);

public class ValidationResult
{
    public string FilePath { get; init; } = string.Empty;
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    public int EntryCount { get; init; }
}

public class QualityValidator
{
    private class DialogueFile
    {
        public string Character { get; set; } = string.Empty;
        public string Situation { get; set; } = string.Empty;
        public List<DialogueEntry> Entries { get; set; } = new();
    }

    private class DialogueEntry
    {
        public string Id { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public ValidationResult Validate(string filePath, QualityRule rule)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {filePath}");
        }

        var yaml = File.ReadAllText(filePath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var dialogue = deserializer.Deserialize<DialogueFile>(yaml);
        var errors = new List<string>();

        // Validate entry count
        if (dialogue.Entries.Count < rule.MinEntries)
        {
            errors.Add($"Entry count {dialogue.Entries.Count} < {rule.MinEntries}");
        }

        // Validate lines per entry
        for (int i = 0; i < dialogue.Entries.Count; i++)
        {
            var entry = dialogue.Entries[i];
            // Count lines by splitting on newlines
            var lines = entry.Content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var lineCount = lines.Length;

            if (lineCount < rule.MinLinesPerEntry)
            {
                errors.Add($"Entry[{i}]: Lines {lineCount} < {rule.MinLinesPerEntry}");
            }
        }

        return new ValidationResult
        {
            FilePath = filePath,
            IsValid = errors.Count == 0,
            Errors = errors,
            EntryCount = dialogue.Entries.Count
        };
    }
}
