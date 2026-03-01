# Community Tools Reference

Tools designed for community contributors to create and validate kojo (口上) content without requiring a full development environment.

## Overview

Community tools prioritize accessibility for non-developer contributors. These tools feature:

- **No development environment required** - Standalone binaries with bundled dependencies
- **Japanese language support** - Localized error messages and documentation
- **User-friendly interfaces** - Drag-and-drop support, clear error messages
- **Rich guidance** - Usage examples (凡例), typo suggestions, troubleshooting tips

## Available Tools

### COM YAML Validator (com-validator)

**Purpose:** Validate COM (Command) YAML files with Japanese error messages

**Location:** `tools/com-validator/`

**Usage:**
```bash
# Command line
com-validator Game/data/coms/training/touch/caress.yaml

# Drag and drop (Windows)
# Drag YAML file onto validate.bat
```

**Key Features:**
- ✅ Single binary (no .NET runtime required)
- ✅ Japanese and English error messages
- ✅ Line number and column reporting
- ✅ Typo suggestions (もしかして / did you mean)
- ✅ Bundled COM schema (no external files)
- ✅ Usage examples with `--examples` flag
- ✅ Windows batch file for double-click execution

**When to use:**
- You're a community contributor writing COM files
- You use a text editor (Notepad, Sakura Editor, etc.)
- You want validation without installing .NET
- You need Japanese error messages and examples

**Documentation:** [tools/com-validator/README.md](../../../tools/com-validator/README.md)

**Related Feature:** [F611 - COM YAML Linter with Japanese Support](../feature-611.md)

---

## Developer Tools (For Comparison)

These tools require .NET development environment and are designed for developers working in CI/CD pipelines:

### YamlValidator

**Purpose:** Batch YAML validation for CI/CD and pre-commit hooks

**Location:** `tools/YamlValidator/`

**Usage:**
```bash
# Requires .NET runtime
dotnet run --project tools/YamlValidator/ -- \
  --schema tools/YamlSchemaGen/dialogue-schema.json \
  --yaml Game/YAML/Kojo/COM_K1_0.yaml
```

**Key Features:**
- ✅ .NET-based for build pipeline integration
- ✅ Batch validation of entire directories
- ✅ Git pre-commit hook support
- ✅ CI/CD exit codes for automation
- ❌ Requires .NET runtime
- ❌ English-only error messages
- ❌ Command-line only (no drag-and-drop)

**When to use:**
- You're a developer with .NET installed
- You need pre-commit or CI/CD integration
- You want to validate entire directories at once
- You're comfortable with command-line tools

**Documentation:** [tools/YamlValidator/README.md](../../../tools/YamlValidator/README.md)

**Related Feature:** [F590 - YAML Schema Validation Tools](../feature-590.md)

---

## Tool Selection Guide

| Scenario | Recommended Tool | Reason |
|----------|------------------|--------|
| Community kojo writer (no dev tools) | **com-validator** | No .NET required, Japanese errors, drag-and-drop |
| Developer in VS Code | **YamlValidator** | IDE integration, .NET tooling, git hooks |
| Pre-commit validation | **YamlValidator** | Git hook integration with build pipeline |
| Learning YAML syntax | **com-validator** | Rich examples and Japanese guidance |
| CI/CD pipeline | **YamlValidator** | Batch validation, exit codes, directory support |
| Quick validation of single file | **com-validator** | Faster startup (no .NET runtime overhead) |
| Batch validation of many files | **YamlValidator** | Directory recursion, summary reports |

---

## Getting Started (Community Contributors)

### Prerequisites

**For com-validator:**
- ✅ Windows, macOS, or Linux
- ✅ No development tools required
- ✅ Download pre-built binary or build from source (requires Go)

**For YamlValidator (developers only):**
- ❌ .NET 8.0 or later runtime
- ❌ Command-line experience
- ❌ Git (for pre-commit hooks)

### Quick Start

**Validating a COM file:**

1. Navigate to `tools/com-validator/` folder
2. Drag your YAML file onto `validate.bat` (Windows)
3. View validation results
4. Press any key to close

**Command line (all platforms):**

```bash
# From repository root
tools/com-validator/com-validator Game/data/coms/training/touch/caress.yaml
```

**Getting help:**

```bash
# Show usage
com-validator --help

# Show Japanese and English examples
com-validator --examples

# Show version
com-validator --version
```

---

## Understanding Validation Errors

### Error Message Format

com-validator displays errors in Japanese and English:

```
==================== 検証エラー / Validation Error ====================
位置 / Location: 12行 5列 (Line 12, Column 5)
フィールド / Field: character_id

エラー / Error:
  [JP] 型が一致しません。期待される型と異なる値が指定されています。
  [EN] Type mismatch. The value does not match the expected type.

詳細 / Details: Invalid type. Expected: string, given: integer

もしかして / Did you mean: character
======================================================================
```

### Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| 型が一致しません (Type mismatch) | String used where number expected | Check schema requirements for field type |
| 必須フィールドが不足 (Missing required field) | Required property not present | Add missing field to YAML |
| フォーマットエラー (Format error) | YAML syntax error (indentation, colons) | Fix YAML syntax at reported line number |
| 列挙型の値が無効 (Invalid enum) | Value not in allowed list | Use one of the allowed values shown in error |
| 未知のプロパティ (Unknown property) | Typo or unsupported field | Check field name spelling or schema documentation |

### Typo Suggestions

The validator suggests corrections for common mistakes:

- `charcter` → `character`
- `dialoge` → `dialogue`
- `affectoin` → `affection`
- `talant` → `talent`

See the "もしかして / Did you mean" line in error output.

---

## Workflow Integration

### Recommended Workflow (Community Contributors)

1. **Edit** - Create or modify COM YAML file in your text editor
2. **Validate** - Drag file onto `validate.bat` or run `com-validator`
3. **Fix** - Correct errors shown with line numbers and Japanese descriptions
4. **Repeat** - Validate again until successful
5. **Submit** - Share validated YAML file with development team

### Validation Before Submission

**Always validate before submitting kojo contributions:**

```bash
# Validate your file
com-validator Game/data/coms/your-new-com.yaml

# Expected output on success
✓ Validation successful! / 検証成功!
```

This ensures your contribution matches schema requirements and reduces review time.

---

## IDE Integration (Optional)

For contributors using VS Code or other IDEs, comprehensive setup instructions are available:

**See:** [IDE Integration Reference](./ide-integration.md)

The IDE integration guide covers:

- **VS Code** - YAML Language Support extension setup (automatic + manual)
- **IntelliJ IDEA** - JSON Schema Mappings configuration
- **Vim** - coc.nvim and ALE LSP client setup
- **Emacs** - yaml-mode, flycheck, and lsp-mode configuration
- **Sublime Text** - LSP-yaml package setup
- **Troubleshooting** - Common issues and solutions for all editors

**Benefits:**
- YAML schema autocomplete
- Real-time validation (as you type)
- Error highlighting with line numbers
- Schema-based snippets and documentation on hover

**Note:** IDE integration requires editor setup and schema configuration. Community contributors using simple text editors should use com-validator instead.

---

## Frequently Asked Questions

### Can I use com-validator without installing Go?

Yes. Download the pre-built `com-validator.exe` binary from the releases page. The binary includes all dependencies (including the COM schema) and requires no runtime.

### What's the difference between com-validator and YamlValidator?

- **com-validator** - Standalone binary for community contributors (Japanese errors, no .NET)
- **YamlValidator** - .NET tool for developers (CI/CD integration, batch validation)

See the **Tool Selection Guide** above for detailed comparison.

### Does com-validator work on macOS and Linux?

Yes. Build from source using Go:

```bash
cd tools/com-validator
go build -o com-validator  # Linux/macOS
```

Or download platform-specific binaries from releases.

### How do I update the schema?

Community contributors don't need to update the schema manually. When the development team updates `src/tools/schemas/com.schema.json`, a new com-validator binary will be built and released with the embedded schema.

If building from source, run `go build` after schema updates to embed the latest schema.

### What if I find a bug or have a suggestion?

Report issues through the project's issue tracker or contact the development team. Include:

- Error message (Japanese and English)
- YAML file that caused the error (if applicable)
- Expected behavior
- com-validator version (`--version`)

---

## Future Tools

Planned community tools (not yet implemented):

- **kojo-lint** - Dialogue content linter with NTR consistency checks
- **kojo-preview** - Preview dialogue flow and branching logic
- **kojo-template** - Template generator for common kojo patterns

See [content-roadmap.md](../content-roadmap.md) for community tool development plans.

---

## References

- [com-validator README](../../../tools/com-validator/README.md) - Detailed usage documentation
- [YamlValidator README](../../../tools/YamlValidator/README.md) - Developer tool documentation
- [Feature F611](../feature-611.md) - COM YAML Linter with Japanese Support
- [Feature F590](../feature-590.md) - YAML Schema Validation Tools
- [Feature F599](../feature-599.md) - IDE/Editor Integration for YAML Schema
- [COM Schema](../../schemas/com.schema.json) - Schema definition for COM files
- [IDE Integration](./ide-integration.md) - VS Code setup for kojo development
