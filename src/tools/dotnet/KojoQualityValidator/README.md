# KojoQualityValidator

Validates kojo YAML files against quality standards (minimum entries, minimum lines per entry).

## Usage

```bash
# Validate specific files
dotnet run --project tools/KojoQualityValidator/KojoQualityValidator.csproj -- --files "Game/YAML/Kojo/**/*.yaml"

# Validate changed files in git diff
dotnet run --project tools/KojoQualityValidator/KojoQualityValidator.csproj -- --diff HEAD~1

# Custom thresholds
dotnet run --project tools/KojoQualityValidator/KojoQualityValidator.csproj -- --files "*.yaml" --min-entries 4 --min-lines 4
```

## Purpose

Enforces quality standards for kojo dialogue files, ensuring sufficient content volume and variety. Used in CI/CD pipelines to prevent low-quality dialogue from being committed.

## Key Features

- File pattern matching with `--files`
- Git diff integration with `--diff`
- Configurable thresholds (`--min-entries`, `--min-lines`)
- Clear PASS/FAIL reporting

## Default Thresholds

- Minimum entries: 4
- Minimum lines per entry: 4

## Exit Codes

- 0: All files passed validation
- 1: One or more files failed or invalid arguments
