# COM YAML Validator

Community-friendly standalone YAML validator for ERA game COM (Command) files with Japanese error messages.

## Purpose

com-validator is a Go-based standalone validator designed for community kojo writers who need to validate COM YAML files without setting up a development environment. Unlike [YamlValidator](../YamlValidator/README.md) which requires .NET runtime and is designed for CI/CD integration, com-validator is a single-binary tool with:

- **Japanese error messages** - Localized errors with line numbers and suggestions
- **Bundled schema** - No external dependencies or schema files required
- **Drag-and-drop support** - Windows batch file for non-technical users
- **Typo suggestions** - "もしかして" (did you mean) corrections for common mistakes
- **Usage examples** - Built-in Japanese and English documentation

### Tool Selection Criteria

Choose the right validator for your use case:

| Use Case | Tool | Reason |
|----------|------|--------|
| Community contributor (text editor) | **com-validator** | No .NET required, Japanese errors, single binary |
| Developer (VS Code, CI/CD) | **YamlValidator** | .NET integration, batch validation, pre-commit hooks |
| Pre-commit validation | **YamlValidator** | Git hook integration with .NET build pipeline |
| Learning YAML syntax | **com-validator** | Rich examples and Japanese guidance |
| Automated testing | **YamlValidator** | CI/CD integration with exit codes |

**Rule of thumb:** If you need .NET runtime for development, use YamlValidator. If you're a community contributor without development tools, use com-validator.

## Installation

### Download Pre-built Binary

Download `com-validator.exe` from the releases page and place it in `src/tools/go/com-validator/`.

### Build from Source

Requires Go 1.21 or later:

```bash
cd tools/com-validator
go build -o com-validator.exe
```

The build automatically embeds `schemas/com.schema.json` using Go's embed feature.

## 使い方 (Usage)

### Basic Usage

Validate a single COM YAML file:

```bash
# Windows Command Prompt
com-validator Game\data\coms\training\touch\caress.yaml

# Git Bash / PowerShell
com-validator Game/data/coms/training/touch/caress.yaml
```

**Success output:**
```
Validating: Game/data/coms/training/touch/caress.yaml
✓ Validation successful! / 検証成功!
```

**Error output:**
```
Validating: Game/data/coms/training/touch/caress.yaml
==================== 検証エラー / Validation Error ====================
位置 / Location: 12行 5列 (Line 12, Column 5)
フィールド / Field: character_id

エラー / Error:
  [JP] 型が一致しません。期待される型と異なる値が指定されています。
  [EN] Type mismatch. The value does not match the expected type.

詳細 / Details: Invalid type. Expected: string, given: integer (期待される型 / Expected type: string)

もしかして / Did you mean: character

======================================================================
```

### ドラッグ&ドロップ使用 (Drag and Drop)

Windows users can double-click `validate.bat` or drag a YAML file onto it:

1. Open `src/tools/go/com-validator/` folder
2. Drag your `.yaml` file onto `validate.bat`
3. Validation results appear in a command window
4. Window stays open (press any key to close)

### コマンドオプション (Command Options)

```bash
# Show help
com-validator --help

# Show version
com-validator --version

# Show usage examples (凡例)
com-validator --examples
```

### 凡例 (Usage Examples)

Run `com-validator --examples` to see detailed Japanese and English examples including:

- Single file validation
- Drag and drop usage
- Error message interpretation
- Common error types and solutions
- Troubleshooting tips

## Error Messages

### Japanese Error Format

All errors are displayed in both Japanese and English:

```
==================== 検証エラー / Validation Error ====================
位置 / Location: 15行 3列 (Line 15, Column 3)
フィールド / Field: description

エラー / Error:
  [JP] 必須フィールドが不足しています。
  [EN] Required field is missing.

======================================================================
```

### Error Components

- **位置 (Location)** - Line and column number where the error occurred
- **フィールド (Field)** - YAML field name with the problem
- **エラー (Error)** - Localized description in Japanese and English
- **詳細 (Details)** - Additional context (expected types, allowed values)
- **もしかして (Did you mean)** - Typo suggestions for common mistakes

### Common Error Types

| Error Type | Japanese | English | Common Cause |
|------------|----------|---------|--------------|
| `type_mismatch` | 型が一致しません | Type mismatch | String specified where number is required |
| `required_field` | 必須フィールドが不足しています | Required field is missing | Missing required property in YAML |
| `invalid_value` | 無効な値が指定されています | Invalid value specified | Value outside allowed range |
| `format_error` | フォーマットエラー | Format error | Incorrect YAML syntax (indentation, colons) |
| `invalid_enum` | 列挙型の値が無効です | Invalid enum value | Value not in allowed list |
| `unknown_property` | 未知のプロパティが含まれています | Unknown property detected | Typo in property name or schema violation |

## Typo Suggestions

The validator suggests corrections for common typos:

```
もしかして / Did you mean: character
```

**Supported typo corrections:**

- General: `charcter` → `character`, `discription` → `description`, `conditon` → `condition`
- COM-specific: `dialoge` → `dialogue`, `affectoin` → `affection`, `talant` → `talent`

See `localization.go` for the complete typo mapping.

## Troubleshooting

### File Not Found

**Error:**
```
Error: File not found: Game/data/coms/test.yaml
```

**Solution:** Verify the file path is correct. Use forward slashes (`/`) or backslashes (`\`) depending on your shell.

### YAML Parse Error

**Error:**
```
==================== 検証エラー / Validation Error ====================
位置 / Location: 8行 1列 (Line 8, Column 1)

エラー / Error:
  [JP] YAMLの解析に失敗しました。
  [EN] Failed to parse YAML.

詳細 / Details: yaml: line 8: mapping values are not allowed in this context
======================================================================
```

**Solution:** Check YAML syntax at the specified line. Common issues:
- Missing colon after property name
- Incorrect indentation (use spaces, not tabs)
- Unclosed quotes or brackets

### Schema Validation Error

**Error:**
```
エラー / Error:
  [JP] スキーマ検証エラー。スキーマの制約に違反しています。
  [EN] Schema validation error. The data violates schema constraints.
```

**Solution:** The YAML structure doesn't match the COM schema requirements. Check:
- Required fields are present
- Field types match schema (string vs number vs array)
- Enum values are from the allowed list

### Schema Out of Sync

If `src/tools/schemas/com.schema.json` is updated but `src/tools/go/com-validator/schemas/com.schema.json` is not synchronized:

**Error:**
```
Validation failed unexpectedly with latest schema definitions
```

**Solution:** The pre-commit hook (`schema-sync-check`) automatically prevents commits with schema drift. If you encounter this error:

1. Copy the updated schema:
   ```bash
   cp src/tools/schemas/com.schema.json src/tools/go/com-validator/schemas/com.schema.json
   ```

2. Rebuild the validator:
   ```bash
   cd tools/com-validator
   go build -o com-validator.exe
   ```

The schema is embedded at build time using `//go:embed`, so rebuilding is required after schema updates.

## Schema Synchronization

The validator uses an embedded copy of `src/tools/schemas/com.schema.json` located at `src/tools/go/com-validator/schemas/com.schema.json`. These files must be kept synchronized:

- **Pre-commit hook** - `.githooks/schema-sync-check` verifies schema files are identical before commit
- **Manual verification** - Run `diff src/tools/schemas/com.schema.json src/tools/go/com-validator/schemas/com.schema.json`
- **Rebuild required** - After updating the schema, rebuild the validator to embed the new schema

## Development

### Project Structure

```
src/tools/go/com-validator/
├── main.go              # CLI entry point and argument parsing
├── validator.go         # Schema validation logic
├── localization.go      # Japanese error messages and typo suggestions
├── localization_test.go # Unit tests for localization
├── go.mod              # Go module definition
├── go.sum              # Dependency checksums
├── schemas/
│   └── com.schema.json # Embedded COM schema (synced from src/tools/schemas/)
├── validate.bat        # Windows drag-and-drop launcher
└── README.md           # This file
```

### Running Tests

```bash
cd tools/com-validator
go test -v
```

### Building for Distribution

```bash
# Build for Windows
go build -o com-validator.exe

# Build for Linux
GOOS=linux GOARCH=amd64 go build -o com-validator

# Build for macOS
GOOS=darwin GOARCH=amd64 go build -o com-validator
```

### Adding New Error Messages

Edit `localization.go`:

1. Add message type to `getErrorMessage()` map
2. Add typo mappings to `getSuggestion()` if applicable
3. Update `PrintUsageExamples()` for new error type documentation
4. Add test cases in `localization_test.go`

## References

- [YamlValidator](../YamlValidator/README.md) - .NET-based validator for CI/CD integration
- [Feature F611](../../pm/features/feature-611.md) - COM YAML Linter with Japanese Support
- [Feature F590](../../pm/features/feature-590.md) - YAML Schema Validation Tools
- [COM Schema](../../src/tools/schemas/com.schema.json) - Source schema definition
- [Community Tools](../../docs/reference/community-tools.md) - Community contribution tools overview
