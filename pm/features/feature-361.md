# Feature 361: Schema Validator Integration

## Status: [DONE]

## Type: infra

## Created: 2026-01-05

---

## Summary

Integrate JSON Schema validation into YAML dialogue workflow. Uses F348 output (dialogue-schema.json) to validate YAML kojo files at build time and during conversion pipeline. Prevents structural errors from reaching runtime.

**Output**: Schema validation integrated into:
1. ErbToYaml conversion pipeline (fail on invalid YAML)
2. CI workflow (validate all YAML files)

---

## Background

### Philosophy (Mid-term Vision)

**Test-First Migration**: Before migrating game logic (Phase 3+), establish robust validation to catch structural errors early. Schema validation shifts error detection from runtime to build time, reducing debugging cost.

### Problem (Current Issue)

F358 Phase 2 Analysis identified validation gaps:
- F348 generated dialogue-schema.json (DONE)
- F350 YamlRenderer has runtime validation (AC#6)
- No build-time validation for converted YAML files
- Conversion pipeline (F349) does not validate output against schema
- CI has no YAML validation step

Without build-time validation:
- Invalid YAML structure detected only at runtime (late)
- Conversion bugs may produce invalid YAML silently
- No guarantee that batch-converted YAML files (Phase 12) conform to schema
- Developer experience poor (no IDE autocomplete/validation)

### Goal (What to Achieve)

Integrate schema validation into development workflow:
1. Add schema validation to ErbToYaml conversion pipeline (fail fast on invalid YAML)
2. Create CLI validator tool for standalone validation
3. Create CI workflow step to validate all YAML files against schema

**Out of scope** (implementation details, no AC):
- Runtime validation in Era.Core.KojoEngine (optional future enhancement)
- IDE schema integration (VS Code, Rider) - documented in Schema Integration Points section

**Schema Source**: `tools/YamlSchemaGen/dialogue-schema.json` (F348 output)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Schema validation in converter | test | dotnet test | succeeds | - | [x] |
| 2 | Invalid YAML rejected | test | dotnet test | succeeds | - | [x] |
| 3 | Valid YAML passes | test | dotnet test | succeeds | - | [x] |
| 4 | CLI validator tool created | file | Glob | exists | tools/YamlValidator/ | [x] |
| 5 | CLI validates file | output | dotnet run | contains | PASS: COM_K1_0.yaml is valid | [x] |
| 6 | CLI rejects invalid file | output | dotnet run | contains | FAIL: invalid-kojo.yaml | [x] |
| 7 | CI workflow file created | file | Glob | exists | .github/workflows/test.yml | [x] |

### AC Details

**AC#1 Test**: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~SchemaValidation"`
- Modify ErbToYaml converter to validate output YAML against schema before writing to file
- Verify: Test creates converter, validates output YAML, confirms no exception

**AC#2 Test**: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~InvalidYaml"`
- Input: Create invalid YAML fixture (missing required field `function_name`)
- Verify: SchemaValidationException thrown with descriptive message

**AC#3 Test**: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~ValidYaml"`
- Input: Use pilot YAML from F351 (美鈴 COM_0)
- Verify: Validation succeeds, no exception

**AC#4 Test**: Verify `tools/YamlValidator/` directory exists with CLI tool project.

**AC#5 Test**: Run CLI on valid YAML:
```bash
dotnet run --project tools/YamlValidator/ -- \
  --schema "tools/YamlSchemaGen/dialogue-schema.json" \
  --yaml "tools/ErbToYaml.Tests/TestOutput/COM_K1_0.yaml"
```
Expected output: `PASS: COM_K1_0.yaml is valid`

**AC#6 Test**: Run CLI on invalid YAML (malformed fixture):
```bash
dotnet run --project tools/YamlValidator/ -- \
  --schema "tools/YamlSchemaGen/dialogue-schema.json" \
  --yaml "tools/YamlValidator.Tests/TestData/invalid-kojo.yaml"
```
Expected output contains:
```
FAIL: invalid-kojo.yaml
Error at line 5: Missing required property 'function_name'
```

**AC#7 Test**: Verify `.github/workflows/test.yml` exists with YAML validation step:
```bash
dir .github\workflows\test.yml
```
Expected: File exists with `Validate YAML Dialogue Files` step.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add NJsonSchema validation to ErbToYaml converter (DatalistConverter.cs) | [x] |
| 2 | 2 | Add test for invalid YAML rejection (SchemaValidationException) | [x] |
| 3 | 3 | Add test for valid YAML passing validation | [x] |
| 4 | 4 | Create tools/YamlValidator/ CLI tool project | [x] |
| 5 | 5 | Implement CLI --schema/--yaml args with PASS output | [x] |
| 6 | 6 | Implement CLI FAIL output with error details | [x] |
| 7 | 7 | Create .github/workflows/test.yml with YAML validation step | [x] |

<!-- Note: Test fixtures and IDE documentation are implementation details, not AC-level deliverables -->

---

## Schema Integration Points

### 1. Conversion Pipeline (ErbToYaml)

**Location**: `tools/ErbToYaml/DatalistConverter.cs`

**Integration**:
```csharp
public class DatalistConverter
{
    private readonly NJsonSchema.JsonSchema _schema;

    public DatalistConverter(string schemaPath)
    {
        _schema = JsonSchema.FromFileAsync(schemaPath).Result;
    }

    public void ConvertToYaml(string erbPath, string yamlPath)
    {
        // ... existing conversion logic ...

        // Validate before writing
        var errors = _schema.Validate(yamlContent);
        if (errors.Count > 0)
        {
            throw new SchemaValidationException(
                $"YAML validation failed: {string.Join(", ", errors)}");
        }

        File.WriteAllText(yamlPath, yamlContent);
    }
}
```

**Benefit**: Fail fast if conversion produces invalid YAML, catching bugs early.

### 2. CLI Validator Tool

**Location**: `tools/YamlValidator/`

**Purpose**: Standalone tool for validating YAML files during development and CI.

**Usage**:
```bash
# Validate single file
dotnet run --project tools/YamlValidator/ -- \
  --schema "tools/YamlSchemaGen/dialogue-schema.json" \
  --yaml "Game/YAML/Kojo/COM_K1_0.yaml"

# Validate all YAML in directory (CI mode)
dotnet run --project tools/YamlValidator/ -- \
  --schema "tools/YamlSchemaGen/dialogue-schema.json" \
  --validate-all "Game/YAML/Kojo/"
```

**Exit Codes**:
- `0`: All files valid
- `1`: Validation errors found (CI fails build)

### 3. Runtime Validation (Era.Core.KojoEngine)

**Location**: `Era.Core/KojoEngine.cs`

**Integration** (optional, debug mode only):
```csharp
public class KojoEngine
{
    private readonly bool _validateSchema;

    public KojoEngine(bool validateSchema = false)
    {
        _validateSchema = validateSchema;
    }

    public string LoadYaml(string yamlPath)
    {
        var yaml = File.ReadAllText(yamlPath);

        if (_validateSchema)
        {
            // Validate at runtime (debug mode)
            ValidateAgainstSchema(yaml);
        }

        return ParseYaml(yaml);
    }
}
```

**Note**: Runtime validation disabled by default for performance. Enabled via environment variable `ERA_VALIDATE_SCHEMA=1` for debugging.

### 4. IDE Integration

**VS Code** (YAML extension):

Create `.vscode/settings.json`:
```json
{
  "yaml.schemas": {
    "tools/YamlSchemaGen/dialogue-schema.json": "Game/YAML/Kojo/*.yaml"
  }
}
```

**JetBrains Rider**: Automatic schema detection if `$schema` property in YAML:
```yaml
$schema: ../../tools/YamlSchemaGen/dialogue-schema.json
function_name: "@KOJO_MESSAGE_COM_K1_0"
# ... rest of YAML ...
```

**Benefit**: Autocomplete for condition keys (TALENT, ABL, EXP), validation errors highlighted in editor.

---

## CI Integration

### GitHub Actions Workflow

**File**: `.github/workflows/test.yml`

**New Step** (add after existing tests):
```yaml
jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
        with:
          submodules: recursive

      # ... existing test steps ...

      - name: Validate YAML Dialogue Files
        run: |
          dotnet run --project tools/YamlValidator/ -- \
            --schema "tools/YamlSchemaGen/dialogue-schema.json" \
            --validate-all "Game/YAML/Kojo/"
        shell: pwsh

      - name: Report Validation Results
        if: failure()
        run: echo "YAML validation failed. Check logs for details."
```

**Trigger**: Runs on all PRs and pushes to master.

**Effect**: Blocks merge if YAML files violate schema.

---

## Error Reporting

### Validation Error Format

**Example Invalid YAML**:
```yaml
function_name: "@KOJO_MESSAGE_COM_K1_0"
conditions:
  - TALENT: 0  # Invalid: should be "TALENT:0"
    dialogue: "text"
```

**Validator Output**:
```
FAIL: COM_K1_0.yaml
Error at line 3, column 5:
  Property 'TALENT' does not match schema pattern '^[A-Z]+:\d+$'
  Expected format: 'VARIABLE:INDEX' (e.g., 'TALENT:0')
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F348 | Requires dialogue-schema.json (DONE) |
| Predecessor | F349 | Integrates validation into DATALIST converter |
| Predecessor | F350 | Era.Core.KojoEngine may use optional runtime validation |
| Predecessor | F358 | Phase 2 planning, defines validation requirements |
| Parallel | F359 | Test structure provides validation test utilities |
| Parallel | F360 | KojoComparer may use schema validator for YAML input checking |
| Successor | - | CI workflow created within F361 scope (AC#7) |

---

## Links

- [feature-348.md](feature-348.md) - YAML Schema Generator (provides dialogue-schema.json)
- [feature-349.md](feature-349.md) - DATALIST→YAML Converter (integration point)
- [feature-350.md](feature-350.md) - YAML Dialogue Renderer (runtime validation)
- [feature-358.md](feature-358.md) - Phase 2 Planning (defines validation needs)
- [feature-359.md](feature-359.md) - Test Structure (shared validation utilities)
- [feature-360.md](feature-360.md) - KojoComparer Tool (uses validated YAML)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 2 Task 3 (line 821)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | implementer | Task 2 from F358 | PROPOSED |
| 2026-01-05 20:21 | START | implementer | Task 1 | - |
| 2026-01-05 20:21 | END | implementer | Task 1 | SUCCESS |
| 2026-01-05 20:27 | START | implementer | Task 4-6 | - |
| 2026-01-05 20:27 | END | implementer | Task 4-6 | SUCCESS |
| 2026-01-05 20:29 | START | implementer | Task 7 | - |
| 2026-01-05 20:29 | END | implementer | Task 7 | SUCCESS |
| 2026-01-05 20:35 | AC | ac-tester | AC#1-7 verification | PASS:7/7 |
