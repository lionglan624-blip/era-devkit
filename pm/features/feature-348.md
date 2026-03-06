# Feature 348: YAML Schema Generator

## Status: [DONE]

## Type: engine

## Created: 2026-01-05

---

## Summary

Generate JSON Schema for YAML dialogue files from VariableCode.cs definitions. Provides validation contract for YAML dialogue files and enables IDE autocomplete.

**Output**: `tools/YamlSchemaGen/dialogue-schema.json`

---

## Background

### Philosophy (Mid-term Vision)

**Testable Minimum Unit**: Each feature should be the smallest unit that can be independently tested. Schema generation is independent of parsing (F346) and rendering (F350), enabling parallel development while providing validation foundation.

### Problem (Current Issue)

F344 Phase 1 requires YAML schema for:
- Dialogue file validation (structural correctness)
- IDE support (autocomplete for conditions, variables)
- Documentation (contract between ERB and YAML)

Without schema, YAML files lack validation until runtime, delaying error detection.

### Goal (What to Achieve)

Generate JSON Schema that:
1. Defines common ERA variable types for dialogue conditions (TALENT, ABL, EXP, FLAG, CFLAG)
2. Validates YAML dialogue structure
3. Supports sample YAML validation test

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Schema generated | file | Glob | exists | tools/YamlSchemaGen/dialogue-schema.json | [x] |
| 2 | Variable types mapped | file | Grep | contains | "TALENT" in tools/YamlSchemaGen/dialogue-schema.json | [x] |
| 3 | Validate sample YAML | test | dotnet test | succeeds | - | [x] |

### AC Details

**AC#1 Test**: Run `dotnet run --project tools/YamlSchemaGen/`, verify output file exists at tools/YamlSchemaGen/dialogue-schema.json

**AC#2 Test**: Grep dialogue-schema.json for TALENT/ABL/EXP definitions. Schema must contain definitions for: TALENT, ABL, EXP (character-scoped integer arrays)

**AC#3 Test**: Run `dotnet test tools/YamlSchemaGen.Tests/` which validates Game/tests/sample-dialogue.yaml against dialogue-schema.json using NJsonSchema

**Sample YAML location**: `Game/tests/sample-dialogue.yaml` containing TALENT/ABL/EXP condition examples (created by Task#3 as test fixture)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create tools/YamlSchemaGen/ C# project that generates dialogue-schema.json with common ERA variable definitions (hardcoded for Phase 1) | [x] |
| 2 | 2 | Add TALENT/ABL/EXP/FLAG/CFLAG variable definitions to schema with proper type mappings | [x] |
| 3 | 3 | Create tools/YamlSchemaGen.Tests/ with NJsonSchema dependency, sample YAML fixture at Game/tests/sample-dialogue.yaml, and unit test that validates sample against schema | [x] |

**Scope Note**: Phase 1 focuses on core type flags (__INTEGER__, __STRING__, __ARRAY_1D__, __CHARACTER_DATA__). Extended flags (__ARRAY_2D__, __ARRAY_3D__, __CALC__, __LOCAL__, __GLOBAL__) are out of scope for this feature. Rationale: Phase 1 dialogue conditions only use CHARACTER_DATA variables (TALENT/ABL/EXP); extended array types are not used in TALENT branching conditions.

**Engine Access**: VariableCode.cs is accessible at `engine/Assets/Scripts/Emuera/GameData/Variable/VariableCode.cs` (engine submodule).

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F345 | Feature breakdown and design source |
| Parallel | F346 | Can be developed concurrently; uses VariableCode.cs directly, not ERB AST |
| Successor | F349 | DATALIST Converter validates output against schema |
| Successor | F350 | Provides JSON Schema for YAML validation |

---

## Links

- [feature-345.md](feature-345.md) - Phase 1 Migration Feature Breakdown
- [feature-346.md](feature-346.md) - ERB Parser (independent development, parsing functionality)
- [feature-349.md](feature-349.md) - DATALIST→YAML Converter (uses schema for validation)
- [feature-350.md](feature-350.md) - YAML Dialogue Renderer (uses schema for validation)
- [feature-344.md](feature-344.md) - Codebase Analysis (variable catalog reference)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | - | Initial proposal from F345 breakdown | PROPOSED |
| 2026-01-05 11:18 | START | implementer | Task 1-3 | - |
| 2026-01-05 11:28 | END | implementer | Task 1-3 | SUCCESS |
