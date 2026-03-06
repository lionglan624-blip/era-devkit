# Feature 349: DATALIST→YAML Converter

## Status: [DONE]

## Type: engine

## Created: 2026-01-05

---

## Summary

Convert DATALIST blocks from ERB AST into YAML dialogue files using extracted condition trees and JSON Schema validation. Integrates parser (F346), extractor (F347/F353), and schema (F348) components.

**Project**: `tools/ErbToYaml/` + `tools/ErbToYaml.Tests/`

---

## Background

### Philosophy (Mid-term Vision)

**Testable Minimum Unit**: Each feature should be the smallest unit that can be independently tested. The converter integrates tested components (AST from F346, conditions from F347) and validates against schema (F348), enabling isolated verification of conversion logic correctness.

### Problem (Current Issue)

F344 Phase 1 Task 1.1 identified ERB→YAML conversion as high complexity. F345 breakdown split components, but conversion logic must be independently testable:
- Input: AST (F346) + Condition trees (F347)
- Output: YAML dialogue files
- Validation: Against JSON Schema (F348)

Without isolated converter, testing would require full pipeline, delaying feedback.

### Goal (What to Achieve)

Build converter tool (`tools/ErbToYaml/`) that:
1. Processes DATALIST blocks from AST (F346)
2. Embeds condition trees from F347/F353 (TALENT + CFLAG/Function)
3. Generates schema-valid YAML dialogue files (validated against F348 schema)

**Dependencies**:
- `tools/ErbParser/` - AST generation (F346)
- `tools/YamlSchemaGen/` - Schema validation (F348)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Convert DATALIST block | test | dotnet test | succeeds | Simple DATALIST → YAML | [x] |
| 2 | Embed conditions | test | dotnet test | contains | YAML contains condition objects | [x] |
| 3 | Schema validation | test | dotnet test | succeeds | Output YAML passes schema validation | [x] |
| 4 | Invalid DATALIST rejected | test | dotnet test | succeeds | Exception thrown for malformed AST | [x] |
| 5 | Missing condition handled | test | dotnet test | succeeds | Graceful handling when condition missing | [x] |

### AC Details

**AC#1 Test**: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~ConvertSimpleDatalist"`
- Input: `tools/ErbToYaml.Tests/TestData/simple_datalist.erb` (DATALIST with 2 DATAFORM lines)
- Expected: YAML output: `branches: [{ lines: ["line1", "line2"] }]` matching dialogue-schema.json structure

**AC#2 Test**: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~EmbedConditions"`
- Input: `tools/ErbToYaml.Tests/TestData/datalist_with_conditions.erb` (ERB file parsed via ErbParser.Parse())
- Transformation: TalentRef.Name → CSV index (via Game/CSV/Talent.csv lookup)
- Example: `TalentRef(Name='恋慕')` → YAML `condition: { TALENT: { '3': { ne: 0 } } }`
- Expected: YAML output matches dialogue-schema.json `branches[].condition.TALENT` structure with numeric keys

**AC#3 Test**: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~SchemaValidation"`
- Input: Converter output
- Validation: Against `tools/YamlSchemaGen/dialogue-schema.json`

**AC#4 Test**: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~InvalidInput"`
- Input: Malformed/incomplete AST
- Expected: Appropriate exception with clear error message

**AC#5 Test**: `dotnet test tools/ErbToYaml.Tests/ --filter "FullyQualifiedName~MissingCondition"`
- Input: DATALIST referencing non-existent condition
- Test approach: Use StringWriter redirect for Console.Error verification
- Expected: (1) no exception thrown, (2) warning logged to Console.Error, (3) output YAML contains empty condition object `{}` (schema-valid)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | - | Create `tools/ErbToYaml/` and `tools/ErbToYaml.Tests/` project structure; add project reference to ErbParser, NJsonSchema, and YamlDotNet | [x] |
| 1 | 1 | Implement DATALIST→YAML converter core; extract DATAFORM content from DatalistNode as dialogue lines | [x] |
| 2 | 2 | Integrate TALENT conditions from F347; load Talent.csv for name→index mapping; transform TalentRef to dialogue-schema.json format | [x] |
| 3 | 3 | Add schema validation using NJsonSchema, loading dialogue-schema.json from tools/YamlSchemaGen/ | [x] |
| 4 | 4 | Implement input validation and error handling for malformed AST | [x] |
| 5 | 5 | Implement graceful handling for missing condition references | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F345 | Feature breakdown and design source |
| Predecessor | F346 | Requires AST input (DATALIST blocks) |
| Predecessor | F347 | Requires TALENT condition tree objects |
| Predecessor | F348 | Requires JSON Schema for validation |
| Predecessor | F353 | Requires CFLAG/Function condition extraction for complete conversion (optional - basic TALENT via F347) |
| Successor | F351 | Pilot Conversion uses this converter |

---

## Links

- [feature-345.md](feature-345.md) - Phase 1 Migration Feature Breakdown
- [feature-346.md](feature-346.md) - ERB Parser (AST source)
- [feature-347.md](feature-347.md) - TALENT Branching Extractor (condition source)
- [feature-348.md](feature-348.md) - YAML Schema Generator (validation source)
- [feature-351.md](feature-351.md) - Pilot Conversion (integration test)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | - | Initial proposal from F345 breakdown | PROPOSED |
| 2026-01-05 12:25 | START | implementer | Task 0 | - |
| 2026-01-05 12:25 | END | implementer | Task 0 | SUCCESS |
| 2026-01-05 12:32 | START | implementer | Task 1 | - |
| 2026-01-05 12:32 | END | implementer | Task 1 | SUCCESS |
| 2026-01-05 12:40 | START | implementer | Task 2 | - |
| 2026-01-05 12:40 | END | implementer | Task 2 | SUCCESS |
| 2026-01-05 13:15 | START | implementer | Task 3 | - |
| 2026-01-05 13:15 | END | implementer | Task 3 | SUCCESS |
| 2026-01-05 13:15 | START | implementer | Task 4 | - |
| 2026-01-05 13:15 | END | implementer | Task 4 | SUCCESS |
| 2026-01-05 13:15 | START | implementer | Task 5 | - |
| 2026-01-05 13:15 | END | implementer | Task 5 | SUCCESS |
| 2026-01-05 13:20 | VERIFY | feature-reviewer | All ACs [x], All Tasks [x], Build passes | READY_TO_COMMIT |
