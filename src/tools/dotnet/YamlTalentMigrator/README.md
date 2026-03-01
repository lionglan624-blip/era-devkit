# YAML Talent Migrator

One-time migration tool to add TALENT conditions to Kojo YAML files.

## Purpose

Migrates empty `condition: {}` entries in Kojo YAML branches to proper TALENT-based conditions:

- Branch 0: `TALENT: 16: ne: 0` (恋人 - Lover)
- Branch 1: `TALENT: 3: ne: 0` (恋慕 - Love)
- Branch 2: `TALENT: 17: ne: 0` (思慕 - Affection)
- Branch 3+: `{}` (ELSE - keep empty)

## Features

- Preserves existing non-empty conditions (won't overwrite)
- Skips files where all branches already have conditions
- Supports dry-run mode for testing
- Logs all changes to stdout
- Custom path support for testing

## Configuration

The TALENT index mappings are stored in `talent-mapping.json`:

```json
{
  "0": {
    "TALENT": {
      "16": { "ne": 0 }
    }
  },
  "1": {
    "TALENT": {
      "3": { "ne": 0 }
    }
  },
  "2": {
    "TALENT": {
      "17": { "ne": 0 }
    }
  }
}
```

### Schema

- Top-level keys: Branch indices ("0", "1", "2")
- Second level: Condition type ("TALENT")
- Third level: TALENT index (e.g., "16" = 恋人, "3" = 恋慕, "17" = 思慕)
- Fourth level: Operator mapping (e.g., {"ne": 0} means "not equal to 0")

### Custom Configuration

Use `--config` to specify a custom configuration file:

```bash
dotnet run -- --config custom-mapping.json --path /path/to/kojo
```

## Usage

### Dry Run (Recommended First)

```bash
dotnet run --project tools/YamlTalentMigrator/ -- --dry-run
```

### Live Migration

```bash
dotnet run --project tools/YamlTalentMigrator/
```

### Custom Path

```bash
dotnet run --project tools/YamlTalentMigrator/ -- --path "path/to/yaml/files"
```

## Verification

### Before Migration

```bash
grep -r "condition: {}" Game/YAML/Kojo/ | wc -l
# Expected: ~1316
```

### After Migration

```bash
grep -r "condition: {}" Game/YAML/Kojo/ | wc -l
# Expected: <50 (only branch 3+ should remain empty)
```

## Example

### Before

```yaml
branches:
- lines: [...]
  condition: {}
- lines: [...]
  condition: {}
- lines: [...]
  condition: {}
- lines: [...]
  condition:
```

### After

```yaml
branches:
- lines: [...]
  condition:
    TALENT:
      16:
        ne: 0
- lines: [...]
  condition:
    TALENT:
      3:
        ne: 0
- lines: [...]
  condition:
    TALENT:
      17:
        ne: 0
- lines: [...]
  condition: {}
```

## Statistics

From dry-run on 2026-02-05:

- Files scanned: 1118
- Files modified: 409
- Branches updated: 1488

## Dependencies

- .NET 8.0
- YamlDotNet 16.2.1
