namespace EntriesFormatMigrator;

public class DiffReporter
{
    public void LogDryRunChange(string filePath, List<string> changes)
    {
        Console.WriteLine($"\n[DRY-RUN] Would modify: {filePath}");
        foreach (var change in changes)
        {
            Console.WriteLine($"  - {change}");
        }
    }

    public void LogSummary(MigrationSummary summary, bool dryRun)
    {
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine(dryRun ? "DRY-RUN SUMMARY" : "MIGRATION SUMMARY");
        Console.WriteLine(new string('=', 60));
        Console.WriteLine($"Modified: {summary.Modified}");
        Console.WriteLine($"Skipped:  {summary.Skipped}");
        Console.WriteLine($"Failed:   {summary.Failed}");

        if (summary.FailedFiles.Count > 0)
        {
            Console.WriteLine("\nFailed files:");
            foreach (var failure in summary.FailedFiles)
            {
                Console.WriteLine($"  - {failure}");
            }
        }

        Console.WriteLine(new string('=', 60));
    }
}
