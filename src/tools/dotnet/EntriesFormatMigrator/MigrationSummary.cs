namespace EntriesFormatMigrator;

public record MigrationSummary(int Modified, int Skipped, int Failed, List<string> FailedFiles);
