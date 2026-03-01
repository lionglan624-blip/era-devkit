using System.Text.RegularExpressions;

namespace EntriesFormatMigrator;

public class FileDiscovery(IMigrationFileSystem fileSystem)
{
    public IReadOnlyList<string> FindTargetFiles(string kojoDirectory)
    {
        var allYamlFiles = fileSystem.GetFiles(kojoDirectory, "*.yaml");
        var targetFiles = new List<string>();

        foreach (var file in allYamlFiles)
        {
            var content = fileSystem.ReadAllText(file);
            if (IsTargetFile(content) && ValidateFourEntryStructure(content))
            {
                targetFiles.Add(file);
            }
        }

        return targetFiles;
    }

    public static bool IsTargetFile(string content)
    {
        return content.Contains("talent_3_1");
    }

    public static bool ValidateFourEntryStructure(string content)
    {
        // Normalize line endings for consistent matching
        var normalizedContent = content.Replace("\r\n", "\n");

        // Check for exactly 4 entries with priorities 4, 3, 2, 1
        var priorityMatches = Regex.Matches(normalizedContent, @"^  priority:\s*(\d+)$", RegexOptions.Multiline);

        if (priorityMatches.Count != 4)
        {
            return false;
        }

        var priorities = priorityMatches.Select(m => int.Parse(m.Groups[1].Value)).ToList();
        var expectedPriorities = new[] { 4, 3, 2, 1 };

        return priorities.SequenceEqual(expectedPriorities);
    }
}
