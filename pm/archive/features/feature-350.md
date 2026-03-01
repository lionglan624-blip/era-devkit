# Feature 350: YAML Dialogue Renderer

## Status: [DONE]

## Type: engine

## Created: 2026-01-05

---

## Summary

Implement YAML dialogue renderer that loads YAML files, evaluates conditions against game context, and renders appropriate dialogue text. **Runtime component** (not a tool) for Phase 1 YAML dialogue system.

**Component**: `Era.Core/KojoEngine.cs` (will be integrated into GameEngine in a future phase)

**Note**: Unlike F346-F349 which are conversion tools, this is a **runtime library** used by the game engine. Implemented in Phase 1 for Pilot validation (F351), then integrated into full engine in a future phase (see full-csharp-architecture.md for migration phases).

---

## Background

### Philosophy (Mid-term Vision)

**Testable Minimum Unit**: Each feature should be the smallest unit that can be independently tested. The renderer is independently testable with mock YAML inputs and context, verifying runtime logic without requiring full conversion pipeline.

### Problem (Current Issue)

F344 Phase 1 Task 1.3 identified YAML renderer as medium complexity. Without isolated renderer:
- Runtime logic coupled with conversion
- Condition evaluation untestable until full pipeline ready
- Dialogue selection logic unclear

This feature (F350) isolates the renderer to enable:
- Unit testing with fixture YAML files
- Independent condition evaluation verification
- Parallel development with converter (F349)

### Goal (What to Achieve)

Build `Era.Core/KojoEngine.cs` that:
1. Loads YAML dialogue files (schema-validated against F348)
2. Evaluates TALENT/ABL/EXP/CFLAG conditions against game context
3. Selects and renders appropriate dialogue text with placeholder substitution

**Project Structure** (at repository root, separate from tools/):
- `Era.Core/KojoEngine.cs` - Main renderer class
- `Era.Core.Tests/KojoEngineTests.cs` - Unit tests

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Load YAML file (Pos) | test | dotnet test | succeeds | - | [x] |
| 2 | Evaluate conditions (Pos) | test | dotnet test | succeeds | - | [x] |
| 3 | Render dialogue (Pos) | test | dotnet test | succeeds | - | [x] |
| 4 | Invalid YAML rejected (Neg) | test | dotnet test | succeeds | - | [x] |
| 5 | Missing context handled (Neg) | test | dotnet test | succeeds | - | [x] |
| 6 | Schema validation (Neg) | test | dotnet test | succeeds | - | [x] |

### AC Details

**AC#1 Test**: `dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~LoadYaml"`
- Input: `Era.Core.Tests/TestData/sample-dialogue.yaml` (hand-written fixture)
- Verify structure parsed correctly

**AC#2 Test**: `dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~EvaluateConditions"`
- Provide mock context with TALENT:0=1, verify renderer selects correct YAML branch

**AC#3 Test**: `dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~RenderDialogue"`
- Render dialogue with placeholder substitution, verify output matches expected string
- Placeholder syntax: `{CALLNAME}` (character call name), `{TARGET}` (target character name)

**AC#4 Test**: `dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~InvalidYaml"`
- Input: Malformed YAML file with syntax errors
- Expected: `YamlParseException` thrown with descriptive message

**AC#5 Test**: `dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~MissingContext"`
- Input: Valid YAML referencing non-existent context variable
- Expected: Graceful fallback (empty string or default value)

**AC#6 Test**: `dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~SchemaValidation"`
- Input: YAML file that parses but violates dialogue-schema.json (e.g., missing required fields)
- Expected: `SchemaValidationException` thrown with descriptive message

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | - | Create `Era.Core/` project and `Era.Core.Tests/` test project | [x] |
| 1 | 1 | Implement KojoEngine.LoadYaml() for YAML parsing | [x] |
| 2 | 2 | Build ConditionEvaluator for TALENT/ABL/EXP/CFLAG | [x] |
| 3 | 3 | Add KojoEngine.Render() with placeholder substitution | [x] |
| 4 | 4 | Add error handling for invalid YAML input | [x] |
| 5 | 5 | Add fallback handling for missing context variables | [x] |
| 6 | 6 | Add schema validation against dialogue-schema.json | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F345 | Feature breakdown and design source |
| Predecessor | F348 | Requires JSON Schema for validation |
| Parallel | F349 | Both produce/consume YAML; tested independently with fixtures |
| Successor | F351 | Pilot Conversion tests full render pipeline |

---

## Links

- [feature-344.md](feature-344.md) - Phase 1 Migration Planning
- [feature-345.md](feature-345.md) - Phase 1 Migration Feature Breakdown
- [feature-348.md](feature-348.md) - YAML Schema Generator (validation source)
- [feature-349.md](feature-349.md) - DATALIST→YAML Converter (YAML source)
- [feature-351.md](feature-351.md) - Pilot Conversion (integration test)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | - | Initial proposal from F345 breakdown | PROPOSED |
| 2026-01-05 13:35 | START | implementer | Task 0 | - |
| 2026-01-05 13:35 | END | implementer | Task 0 | SUCCESS |
| 2026-01-05 13:37 | START | implementer | Task 1 | - |
| 2026-01-05 13:38 | END | implementer | Task 1 | SUCCESS |
| 2026-01-05 13:41 | START | implementer | Task 2 | - |
| 2026-01-05 13:41 | END | implementer | Task 2 | SUCCESS |
| 2026-01-05 13:42 | START | implementer | Task 3 | - |
| 2026-01-05 13:42 | END | implementer | Task 3 | SUCCESS |
| 2026-01-05 13:44 | START | implementer | Task 4 | - |
| 2026-01-05 13:44 | END | implementer | Task 4 | SUCCESS |
| 2026-01-05 13:44 | START | implementer | Task 6 | - |
| 2026-01-05 13:44 | END | implementer | Task 6 | SUCCESS |
| 2026-01-05 13:44 | START | implementer | Task 5 | - |
| 2026-01-05 13:44 | END | implementer | Task 5 | SUCCESS |
