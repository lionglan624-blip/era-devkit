# EntriesFormatMigrator

Migrates kojo YAML entries from old format to new schema-compliant format.

## Usage

```bash
# Live migration
dotnet run --project tools/EntriesFormatMigrator/EntriesFormatMigrator.csproj -- Game/YAML/Kojo

# Preview changes without modifying files
dotnet run --project tools/EntriesFormatMigrator/EntriesFormatMigrator.csproj -- Game/YAML/Kojo --dry-run
```

## Purpose

Automates the conversion of kojo YAML entry structures to match the latest schema requirements. Applies format patches (e.g., field renames, structure changes) across entire kojo directories while preserving content.

## Key Features

- Batch directory processing
- Dry-run mode for safe previewing
- Diff reporting for transparency
- Rollback-safe (creates backups)

## Dependencies

- Era.Core (YAML schema)

## Exit Codes

- 0: Migration succeeded (or dry-run completed)
- 1: Migration failed or directory not found
