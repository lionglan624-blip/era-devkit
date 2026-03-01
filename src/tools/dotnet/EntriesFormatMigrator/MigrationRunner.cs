namespace EntriesFormatMigrator;

public class MigrationRunner(IMigrationFileSystem fileSystem, IEntryPatcher patcher, DiffReporter reporter)
{
    public async Task<MigrationSummary> RunAsync(string kojoDirectory, bool dryRun)
    {
        var discovery = new FileDiscovery(fileSystem);
        var targetFiles = discovery.FindTargetFiles(kojoDirectory);

        Console.WriteLine($"Found {targetFiles.Count} target files to process.");

        int modified = 0;
        int skipped = 0;
        int failed = 0;
        var failedFiles = new List<string>();

        foreach (var file in targetFiles)
        {
            try
            {
                var result = ProcessFile(file, dryRun);
                if (result.Modified)
                {
                    modified++;
                    if (dryRun)
                    {
                        reporter.LogDryRunChange(file, result.Changes);
                    }
                }
                else
                {
                    skipped++;
                }
            }
            catch (Exception ex)
            {
                failed++;
                failedFiles.Add($"{file}: {ex.Message}");
                Console.Error.WriteLine($"ERROR processing {file}: {ex.Message}");
            }
        }

        var summary = new MigrationSummary(modified, skipped, failed, failedFiles);
        reporter.LogSummary(summary, dryRun);

        return await Task.FromResult(summary);
    }

    public MigrationResult ProcessFile(string filePath, bool dryRun)
    {
        var originalContent = fileSystem.ReadAllText(filePath);
        var patchedContent = patcher.PatchEntries(originalContent);

        var modified = originalContent != patchedContent;
        var changes = new List<string>();

        if (modified)
        {
            // Count entries updated and log changes
            var originalLines = originalContent.Split('\n');
            var patchedLines = patchedContent.Split('\n');

            int entriesUpdated = 0;
            for (int i = 0; i < Math.Min(originalLines.Length, patchedLines.Length); i++)
            {
                if (originalLines[i] != patchedLines[i])
                {
                    if (originalLines[i].Contains("id: fallback") && patchedLines[i].Contains("id: talent_"))
                    {
                        changes.Add($"Renamed id: fallback -> {patchedLines[i].Trim()} at line {i + 1}");
                        entriesUpdated++;
                    }
                    else if (patchedLines[i].Contains("condition:"))
                    {
                        changes.Add($"Inserted condition block after line {i}");
                    }
                }
            }

            if (!dryRun)
            {
                fileSystem.WriteAllText(filePath, patchedContent);
            }
        }

        return new MigrationResult(modified, changes.Count(c => c.Contains("Renamed")), changes);
    }
}
