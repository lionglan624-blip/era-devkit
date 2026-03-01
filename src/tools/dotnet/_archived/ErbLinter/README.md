# ERB Linter

Static analysis tool for ERB scripts used in ERA games.

## Features

- **Syntax checks**: Block matching (IF/ENDIF, FOR/NEXT, etc.), parenthesis balance
- **Variable checks**: Undefined FLAG/CFLAG indices
- **Function checks**: Undefined CALL targets, duplicate function definitions
- **Style checks**: Kojo naming convention validation
- **Dead code detection**: Find unused functions (DEAD001)

## Usage

```bash
cd tools/ErbLinter

# Basic usage - scan all ERB files
dotnet run -- ../../Game/ERB/

# With CSV definitions for variable validation
dotnet run -- -c ../../Game/CSV/ ../../Game/ERB/

# JSON output
dotnet run -- -f json -o report.json ../../Game/ERB/

# Filter by level
dotnet run -- -l error ../../Game/ERB/    # Errors only
dotnet run -- -l warning ../../Game/ERB/  # Errors + Warnings

# Dead code detection
dotnet run -- --dead-code ../../Game/ERB/

# With custom entry points file
dotnet run -- --dead-code --entry-points custom.txt ../../Game/ERB/
```

## Options

| Option | Description |
|--------|-------------|
| `-h, --help` | Show help |
| `-o, --output <file>` | Output to file |
| `-f, --format <fmt>` | Output format: `text` (default), `json` |
| `-l, --level <level>` | Minimum level: `error`, `warning`, `info` |
| `-c, --csv <path>` | CSV directory for variable validation |
| `--no-color` | Disable colored output |
| `--dead-code` | Enable dead code detection |
| `--entry-points <file>` | Custom entry points file for dead code detection |

## Error Codes

### Syntax Errors (ERB)

| Code | Level | Description |
|------|-------|-------------|
| ERB001 | Error | IF/ENDIF mismatch |
| ERB002 | Error | Other block mismatch (FOR/NEXT, WHILE/WEND, etc.) |
| ERB003 | Error | Parenthesis mismatch |

### Variable Warnings (VAR)

| Code | Level | Description |
|------|-------|-------------|
| VAR001 | Info | Undefined FLAG index |
| VAR002 | Info | Undefined CFLAG index |

### Function Warnings (FUNC)

| Code | Level | Description |
|------|-------|-------------|
| FUNC001 | Warning | CALL target not found (disabled by default) |
| FUNC002 | Info | TRYCALL target not found (disabled by default) |
| FUNC004 | Warning | Duplicate function definition |

### Style Warnings (STYLE)

| Code | Level | Description |
|------|-------|-------------|
| STYLE001 | Info | Kojo function in wrong character directory |
| STYLE002 | Info | Kojo function doesn't follow K{N} pattern |

### Dead Code (DEAD)

| Code | Level | Description |
|------|-------|-------------|
| DEAD001 | Info | Function is never called (potential dead code) |

## Output Example

### Text (Default)

```
Game/ERB/TEST.ERB:42:5: error ERB001: Unmatched IF without ENDIF
Game/ERB/TEST.ERB:100:12: warning FUNC004: Duplicate function @FUNC_NAME

Summary:
  Errors:   1
  Warnings: 1
  Info:     0
  Files:    1
```

### JSON

```json
{
  "summary": {
    "errors": 1,
    "warnings": 1,
    "info": 0,
    "files_scanned": 1
  },
  "issues": [
    {
      "file": "Game/ERB/TEST.ERB",
      "line": 42,
      "column": 5,
      "level": "error",
      "code": "ERB001",
      "message": "Unmatched IF without ENDIF"
    }
  ]
}
```

## Development

Built with .NET 8. Located in `tools/ErbLinter/`.

```bash
# Build
dotnet build

# Run tests
dotnet run -- tests/
```

## Links

- [Feature 003](../../pm/archive/feature-003.md) - Feature specification
- [WBS-003](../../pm/archive/WBS-003.md) - Work breakdown
- [erb-reference.md](../../pm/reference/erb-reference.md) - ERB language reference
