namespace EntriesFormatMigrator;

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: EntriesFormatMigrator <kojo-directory> [--dry-run]");
            Console.WriteLine("  <kojo-directory>: Path to Game/YAML/Kojo directory");
            Console.WriteLine("  --dry-run:        Preview changes without modifying files");
            return 1;
        }

        var kojoDirectory = args[0];
        var dryRun = args.Length > 1 && args[1] == "--dry-run";

        if (!Directory.Exists(kojoDirectory))
        {
            Console.Error.WriteLine($"ERROR: Directory not found: {kojoDirectory}");
            return 1;
        }

        Console.WriteLine($"Starting migration on: {kojoDirectory}");
        Console.WriteLine($"Mode: {(dryRun ? "DRY-RUN (no files will be modified)" : "LIVE")}");

        var fileSystem = new MigrationFileSystem();
        var patcher = new EntryPatcher();
        var reporter = new DiffReporter();
        var runner = new MigrationRunner(fileSystem, patcher, reporter);

        var summary = await runner.RunAsync(kojoDirectory, dryRun);

        return summary.Failed > 0 ? 1 : 0;
    }
}
