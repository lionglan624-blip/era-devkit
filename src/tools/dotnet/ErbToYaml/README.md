# ErbToYaml

Converts ERB DATALIST dialogue files to YAML format with schema validation support.

## Usage

```bash
# Batch mode - convert entire directory
dotnet run --project tools/ErbToYaml/ErbToYaml.csproj -- --batch Game/ERB/口上/1_美鈴 [output-dir]

# Single file mode
dotnet run --project tools/ErbToYaml/ErbToYaml.csproj -- <input.erb> <output.yaml>

# Inject COM IDs
dotnet run --project tools/ErbToYaml/ErbToYaml.csproj -- --inject-com-id --batch <dir>
```

## Purpose

Automates the migration of ERB dialogue files (DATALIST format) to YAML, preserving talent conditions, patterns, and display modes. Supports batch conversion of entire character directories and COM ID injection for schema validation.

## Key Features

- Batch directory conversion with `--batch` flag
- Preserves TALENT branching logic
- Generates schema-compliant YAML
- COM ID injection for validation
- Exit code 0 on success, 1 on failure

## Dependencies

- ErbParser (AST parsing)
- YamlDotNet (YAML serialization)
