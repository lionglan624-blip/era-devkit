namespace EntriesFormatMigrator;

public record MigrationResult(bool Modified, int EntriesUpdated, List<string> Changes);
