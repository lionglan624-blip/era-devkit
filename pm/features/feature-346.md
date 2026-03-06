# Feature 346: ERB Parser

## Status: [DONE]

## Type: engine

## Created: 2026-01-05

---

## Summary

Implement ERB lexer/parser that converts ERB source files into an Abstract Syntax Tree (AST). This is the foundation component for Phase 1 migration, enabling structured analysis of ERB code.

---

## Background

### Philosophy (Mid-term Vision)

**Testable Minimum Unit**: Each feature should be the smallest unit that can be independently tested. This parser provides the atomic input for all downstream conversion tools (F347, F349) while remaining independently verifiable through structural validation.

### Problem (Current Issue)

F344 Phase 1 Task 1.1 identified ERB→YAML conversion as high complexity. The parser component must be isolated as an independent, testable unit to:
- Enable clear failure diagnosis (parsing vs conversion issues)
- Allow parallel development of schema generator (F348)
- Provide reusable AST for multiple consumers (TALENT extractor F347, DATALIST converter F349)

### Goal (What to Achieve)

Create a parser that converts ERB source files into a structured AST that:
1. Preserves all syntactic information (DATALIST blocks, control flow, PRINT commands)
2. Validates ERB syntax correctness
3. Outputs queryable structure for downstream tools

### Implementation Approach

Create migration tool project (`tools/ErbParser/`) that extracts and adapts parsing logic from existing `engine/Assets/Scripts/Emuera/GameProc/` (LogicalLineParser.cs, LexicalAnalyzer, and related classes) to produce queryable AST output. This is an extraction/adaptation approach, not writing from scratch.

**Implementation Details**:
1. New project `tools/ErbParser/` creates standalone parser (no Unity/engine runtime dependency)
2. Extract minimal subset: LexicalAnalyzer, TokenReader (stateless components)
3. New AST classes (DatalistNode, IfNode, PrintformNode, DataformNode) - not reusing LogicalLine which has runtime dependencies
4. No dependency on Process/EmueraConsole/runtime initialization
5. Test project: `tools/ErbParser.Tests/` with TestData/ folder for test fixtures
6. Config class references: stub with default values or remove config-dependent branches (parsing-only mode)

**Source References**:
- `engine/Assets/Scripts/Emuera/GameProc/LogicalLineParser.cs` - parsing logic
- `engine/Assets/Scripts/Emuera/GameProc/ErbLoader.cs` - DATALIST block assembly logic
- `engine/Assets/Scripts/Emuera/Sub/LexicalAnalyzer.cs` - tokenization
- `engine/Assets/Scripts/Emuera/Sub/TokenReader.cs` - token reading
- `engine/Assets/Scripts/Emuera/Sub/StringStream.cs` - string stream
- `engine/Assets/Scripts/Emuera/Sub/WordCollection.cs` - word collection
- `engine/Assets/Scripts/Emuera/Sub/Word.cs` - word types

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Parse simple DATALIST | test | dotnet test tools/ErbParser.Tests/ | succeeds | Parse basic DATALIST block | [x] |
| 2 | Detect syntax errors | test | dotnet test tools/ErbParser.Tests/ | succeeds | Invalid ERB source rejected | [x] |
| 3 | Extract structure | test | dotnet test tools/ErbParser.Tests/ | succeeds | AST contains DATALIST nodes | [x] |
| 4 | Invalid nested DATALIST rejected | test | dotnet test tools/ErbParser.Tests/ | succeeds | Nested DATALIST throws error | [x] |
| 5 | Empty file handled | test | dotnet test tools/ErbParser.Tests/ | succeeds | Empty input returns empty AST | [x] |

### AC Details

**AC#1 Test**: Parse a minimal ERB file with DATALIST block, verify AST structure
- Input: ERB file containing `DATALIST`/`DATAFORM`/`ENDLIST`
- Expected: AST with DatalistNode containing DataformNode children
- Test file: `tools/ErbParser.Tests/TestData/simple_datalist.erb`

**AC#2 Test**: Attempt to parse invalid ERB syntax, verify parser throws appropriate error
- Input: ERB file with unclosed DATALIST (missing ENDLIST)
- Expected: Parser throws ParseException with meaningful error message
- Test file: `tools/ErbParser.Tests/TestData/invalid_syntax.erb`

**AC#3 Test**: Parse sample kojo file, verify AST contains expected node types
- Input: Minimal kojo excerpt copied to `tools/ErbParser.Tests/TestData/sample_kojo.erb`
- Expected: AST contains IfNode, DatalistNode, PrintformNode instances
- Verify presence of expected node types in parsed AST

**AC#4 Test**: Invalid nested DATALIST rejected
- Input: ERB file with DATALIST inside DATALIST (invalid structure)
- Expected: Parser throws ParseException for nested DATALIST
- Test file: `tools/ErbParser.Tests/TestData/nested_datalist.erb`

**AC#5 Test**: Empty file handled gracefully
- Input: Empty ERB file or file with only whitespace/comments
- Expected: Parser returns empty AST (no nodes) without error
- Test file: `tools/ErbParser.Tests/TestData/empty.erb`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | - | Create tools/ErbParser/ and tools/ErbParser.Tests/ project structure | [x] |
| 1 | - | Define AST node types (DatalistNode, IfNode, PrintformNode, DataformNode) | [x] |
| 2 | - | Extract/adapt parsing from engine/GameProc (LogicalLineParser, LexicalAnalyzer) | [x] |
| 3 | 1 | Write unit test parsing simple DATALIST block and verify AST structure | [x] |
| 4 | 2 | Write unit test for invalid ERB syntax rejection | [x] |
| 5 | 3 | Write unit test parsing kojo file extracting DATALIST/IF/PRINTFORM nodes | [x] |
| 6 | 4 | Write unit test for nested DATALIST rejection | [x] |
| 7 | 5 | Write unit test for empty file handling | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F345 | Feature breakdown and design source |
| Parallel | F348 | Can be developed concurrently; both feed into F349 |
| Successor | F347 | TALENT Extractor consumes AST output |
| Successor | F349 | DATALIST Converter consumes AST output |

---

## Links

- [feature-345.md](feature-345.md) - Phase 1 Migration Feature Breakdown
- [feature-347.md](feature-347.md) - TALENT Branching Extractor (depends on this)
- [feature-349.md](feature-349.md) - DATALIST→YAML Converter (depends on this)
- [feature-344.md](feature-344.md) - Codebase Analysis (ERB patterns reference)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | - | Initial proposal from F345 breakdown | PROPOSED |
| 2026-01-05 08:31 | START | implementer | Task 0 | - |
| 2026-01-05 08:31 | END | implementer | Task 0 | SUCCESS |
| 2026-01-05 08:34 | START | implementer | Task 1-2 | - |
| 2026-01-05 08:34 | END | implementer | Task 1-2 | SUCCESS |
| 2026-01-05 08:35 | START | implementer | Task 3-7 | - |
| 2026-01-05 08:35 | END | implementer | Task 3-7 | SUCCESS |
| 2026-01-05 08:40 | VERIFY | finalizer | All ACs [x], All Tasks [x], Build passes | READY_TO_COMMIT |
