# Feature 633: PRINTDATA Parser Extension

## Status: [DONE]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: engine

## Created: 2026-01-27

---

## Summary

Extend ErbParser to support PRINTDATA...ENDDATA block parsing, enabling automated extraction of DATALIST content for YAML conversion.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format

### Problem (Current Issue)

ErbParser currently cannot parse PRINTDATA...ENDDATA blocks, which contain the actual dialogue text in kojo files. F351 pilot used regex workaround. This blocks automated batch conversion.

### Goal (What to Achieve)

1. Add PRINTDATA...ENDDATA parsing to ErbParser
2. Extract structured DATALIST content
3. Enable downstream batch conversion (F634)

---

## Dependencies

| Type | ID | Description | Status |
|------|----|-------------|:------:|
| Predecessor | F555 | Phase 19 Planning | [DONE] |
| Successor | F634 | Batch Conversion Tool | [DRAFT] |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Automated migration of 117 ERB kojo files" | Parser must handle all PRINTDATA variants found in kojo files | AC#1, AC#2, AC#3 |
| "cannot parse PRINTDATA...ENDDATA blocks" | Add PrintDataNode AST class | AC#4, AC#5 |
| "Extract structured DATALIST content" | PrintDataNode must contain nested DATALIST blocks | AC#6 |
| "9 PRINTDATA variants must be supported" | Recognize all variants: PRINTDATA/L/W/D/DL/DW/K/KL/KW | AC#2 |
| "Conditional branches within PRINTDATA" | Support IF/ELSEIF/ELSE inside PRINTDATA | AC#7 |
| "Blocks automated batch conversion" | Parser must not throw on valid PRINTDATA structures | AC#8, AC#9 |
| "Enable downstream batch conversion (F634)" | PrintDataNode provides content extraction API | AC#10 |
| "F351 pilot used regex workaround" | Parser replaces regex workaround with proper AST | AC#11 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | PrintDataNode.cs exists | file | Glob(tools/ErbParser/Ast/PrintDataNode.cs) | exists | - | [x] |
| 2 | PrintDataNode has Variant property | code | Grep(tools/ErbParser/Ast/PrintDataNode.cs) | contains | "public string Variant" | [x] |
| 3 | PrintDataNode stores nested content | code | Grep(tools/ErbParser/Ast/PrintDataNode.cs) | contains | "List<AstNode>" | [x] |
| 4 | ErbParser recognizes PRINTDATA | code | Grep(tools/ErbParser/ErbParser.cs) | contains | "PRINTDATA" | [x] |
| 5 | ErbParser recognizes ENDDATA | code | Grep(tools/ErbParser/ErbParser.cs) | contains | "ENDDATA" | [x] |
| 6 | Parser nests DATALIST in PrintDataNode | exit_code | dotnet test tools/ErbParser.Tests --filter PrintDataNestedDatalist | succeeds | - | [x] |
| 7 | Parser handles IF inside PRINTDATA | exit_code | dotnet test tools/ErbParser.Tests --filter PrintDataConditional | succeeds | - | [x] |
| 8 | Parser succeeds on simple PRINTDATA | exit_code | dotnet test tools/ErbParser.Tests --filter ParseSimplePrintData | succeeds | - | [x] |
| 9 | Parser error on unclosed PRINTDATA | exit_code | dotnet test tools/ErbParser.Tests --filter UnclosedPrintData | succeeds | - | [x] |
| 10 | PrintDataNode.GetDataForms() extracts content | code | Grep(tools/ErbParser/Ast/PrintDataNode.cs) | contains | "GetDataForms" | [x] |
| 11 | PrintDataParseTests.cs exists | file | Glob(tools/ErbParser.Tests/PrintDataParseTests.cs) | exists | - | [x] |
| 12 | Zero technical debt | code | Grep(tools/ErbParser/**/*.cs) | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 13 | Build succeeds | build | dotnet build tools/ErbParser | succeeds | - | [x] |
| 14 | All parser tests pass | exit_code | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |

### AC Details

**AC#1: PrintDataNode.cs exists**
- Test: Glob pattern=`tools/ErbParser/Ast/PrintDataNode.cs`
- Expected: File exists
- Rationale: New AST node class for PRINTDATA...ENDDATA blocks, following DatalistNode pattern

**AC#2: PrintDataNode has Variant property**
- Test: Grep pattern=`public string Variant` path=`tools/ErbParser/Ast/PrintDataNode.cs`
- Expected: Property exists to store variant type (PRINTDATA, PRINTDATAL, PRINTDATAW, PRINTDATAD, PRINTDATADL, PRINTDATADW, PRINTDATAK, PRINTDATAKL, PRINTDATAKW)
- Rationale: F634 batch converter needs to know which variant was used for output format handling

**AC#3: PrintDataNode stores nested content**
- Test: Grep pattern=`List<AstNode>` path=`tools/ErbParser/Ast/PrintDataNode.cs`
- Expected: Collection property for nested content (DATAFORM, IF blocks, etc.)
- Rationale: PRINTDATA blocks contain DATALIST, conditionals, and other nested content

**AC#4: ErbParser recognizes PRINTDATA**
- Test: Grep pattern=`PRINTDATA` path=`tools/ErbParser/ErbParser.cs`
- Expected: Parser detects PRINTDATA variants
- Rationale: Entry point detection for PRINTDATA...ENDDATA blocks

**AC#5: ErbParser recognizes ENDDATA**
- Test: Grep pattern=`ENDDATA` path=`tools/ErbParser/ErbParser.cs`
- Expected: Parser detects block terminator
- Rationale: Required to close PRINTDATA blocks (different from ENDLIST)

**AC#6: Parser nests DATALIST in PrintDataNode**
- Test: `dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~PrintDataNestedDatalist`
- Expected: Test passes - PRINTDATA containing DATALIST...ENDLIST produces PrintDataNode with DatalistNode children
- Test scenario:
  ```erb
  PRINTDATAL
      DATALIST
          DATAFORM "hello"
      ENDLIST
  ENDDATA
  ```
- Rationale: Real kojo files have DATALIST inside PRINTDATA (legacy structure)

**AC#7: Parser handles IF inside PRINTDATA**
- Test: `dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~PrintDataConditional`
- Expected: Test passes - IF/ELSEIF/ELSE within PRINTDATA produces correct AST
- Test scenario: PRINTDATA with nested IF block containing DATAFORM
- Rationale: Kojo files have conditional branches inside PRINTDATA (see KOJO_K1_日常.ERB lines 50-78)

**AC#8: Parser succeeds on simple PRINTDATA**
- Test: `dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~ParseSimplePrintData`
- Expected: Test passes - Simple PRINTDATA...ENDDATA block parses successfully
- Test scenario:
  ```erb
  PRINTDATAL
      DATAFORM "line1"
      DATAFORM "line2"
  ENDDATA
  ```
- Rationale: Positive test for basic PRINTDATA parsing

**AC#9: Parser error on unclosed PRINTDATA**
- Test: `dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~UnclosedPrintData`
- Expected: Test passes - Missing ENDDATA throws ParseException
- Test scenario: PRINTDATA without ENDDATA
- Rationale: Negative test - parser must detect malformed input

**AC#10: PrintDataNode.GetDataForms() extracts content**
- Test: Grep pattern=`GetDataForms` path=`tools/ErbParser/Ast/PrintDataNode.cs`
- Expected: Helper method exists for downstream F634 integration
- Method signature: `public IEnumerable<DataformNode> GetDataForms()` - flattens nested content for YAML conversion
- Rationale: F634 needs simple API to extract dialogue content without traversing AST manually

**AC#11: PrintDataParseTests.cs exists**
- Test: Glob pattern=`tools/ErbParser.Tests/PrintDataParseTests.cs`
- Expected: Test file exists
- Rationale: Following DatalistParseTests.cs pattern for test organization

**AC#12: Zero technical debt**
- Test: Grep pattern=`TODO|FIXME|HACK` paths=`tools/ErbParser/**/*.cs`
- Expected: Pattern not found in any parser files (not_contains matcher)
- Rationale: No technical debt markers in new code

**AC#13: Build succeeds**
- Test: `dotnet build tools/ErbParser`
- Expected: Exit code 0
- Rationale: Code compiles without errors

**AC#14: All parser tests pass**
- Test: `dotnet test tools/ErbParser.Tests`
- Expected: All tests pass (including existing DATALIST tests)
- Rationale: New PRINTDATA parsing must not break existing functionality

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create PrintDataNode.cs AST class with properties | [x] |
| 2 | 2,3 | Add Variant and Content properties to PrintDataNode | [x] |
| 3 | 4,5 | Add PRINTDATA and ENDDATA detection to ErbParser.cs | [x] |
| 4 | 6 | Implement nested DATALIST parsing within PRINTDATA blocks | [x] |
| 5 | 7 | Implement IF conditional parsing within PRINTDATA blocks | [x] |
| 6 | 8,9 | Add basic PRINTDATA validation tests (positive/negative) | [x] |
| 7 | 10 | Implement GetDataForms() content extraction method | [x] |
| 8 | 11 | Create PrintDataParseTests.cs test file | [x] |
| 9 | 12 | Verify zero technical debt in implementation | [x] |
| 10 | 13 | Verify ErbParser build succeeds | [x] |
| 11 | 14 | Verify all parser tests pass (regression protection) | [x] |

<!-- AC:Task 1:1 Rule: Each AC maps to specific Tasks for implementation and verification -->

<!-- Batch task waiver: Task#2 combines AC#2,3 as both define properties in same PrintDataNode class (atomic edit operation) -->
<!-- Batch task waiver: Task#3 combines AC#4,5 as PRINTDATA/ENDDATA detection is single state machine extension (atomic edit operation) -->
<!-- Batch task waiver: Task#6 combines AC#8,9 as positive/negative test pair for same parsing behavior (symmetric test coverage per Issue 37) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T5,T7 | Technical Design specs | PrintDataNode.cs, ErbParser.cs modifications |
| 2 | implementer | sonnet | T6,T8 | AC Details test specs | PrintDataParseTests.cs |
| 3 | ac-tester | haiku | T9-T11 | AC verification commands | Test results |

**Constraints** (from Technical Design):

1. **Nesting Rules**: Disallow nested PRINTDATA blocks (throw ParseException)
2. **State Machine**: Add `inPrintData` and `currentPrintData` tracking variables parallel to existing `inDatalist` pattern
3. **Variant Detection**: Support all 9 PRINTDATA variants (PRINTDATA, PRINTDATAL, PRINTDATAW, PRINTDATAK, PRINTDATAKL, PRINTDATAKW, PRINTDATAD, PRINTDATADL, PRINTDATADW)
4. **Content Collection**: PrintDataNode.Content accepts DatalistNode, IfNode, DataformNode, PrintformNode
5. **GetDataForms() Implementation**: Recursive traversal with yield return for lazy evaluation

**Pre-conditions**:

- ErbParser project compiles without errors
- Existing DatalistNode and IfNode parsing works correctly
- ErbParser.Tests has passing baseline tests

**Success Criteria**:

- All 14 ACs pass verification
- PrintDataNode follows existing AST node patterns (DatalistNode, IfNode)
- Parser state machine handles PRINTDATA...ENDDATA block lifecycle
- GetDataForms() provides flat content extraction API for F634
- Zero technical debt (no TODO/FIXME/HACK markers)
- All existing parser tests still pass (regression protection)

**Test Naming Convention**: Test methods follow `{TestCase}{Variant}` format (e.g., `ParseSimplePrintData`, `PrintDataNestedDatalist`, `UnclosedPrintData`). This ensures AC filter patterns match correctly.

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback with error details
3. Create follow-up feature for fix with additional investigation of state machine interaction

---

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| - | - | No deferred items |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| | | |

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: F351 pilot conversion used regex workaround to extract PRINTDATA content
2. Why: ErbParser does not recognize PRINTDATA...ENDDATA as a block construct
3. Why: ErbParser was originally designed for DATALIST...ENDLIST parsing only
4. Why: Phase 1 scope (F346) focused on minimal viable parser for pilot conversion
5. Why: PRINTDATA is a wrapper construct containing DATALIST blocks, requiring separate handling

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| F351 tests use complex regex extraction for PRINTDATA content | ErbParser lacks native PRINTDATA...ENDDATA block parsing support |
| Batch conversion cannot process kojo files automatically | No AST node type exists for PRINTDATA blocks |

### Conclusion

The root cause is that ErbParser was designed for DATALIST parsing only. PRINTDATA...ENDDATA is a separate block construct (containing DATALIST blocks and other content) that requires:
1. A new `PrintDataNode` AST class
2. Parser logic to recognize PRINTDATA/PRINTDATAL/PRINTDATAW variants
3. Nested DATALIST parsing within PRINTDATA blocks
4. Content extraction API for downstream conversion (F634)

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F346 | [DONE] | Foundation | Original ERB Parser - DATALIST only |
| F349 | [DONE] | Predecessor | DATALIST→YAML Converter - depends on parser AST |
| F351 | [DONE] | Pilot | Identified PRINTDATA gap via regex workaround |
| F634 | [DRAFT] | Successor | Batch Conversion Tool - depends on F633 output |
| F354-F357 | - | Architecture definition | Defined in architecture.md but no feature files created |

### Pattern Analysis

This is not a recurring pattern - it's a known gap from Phase 1 pilot (F351) that was explicitly deferred. The workaround used in F351 (`FindPrintDataNode` and `FindAllDatalistNodes` methods using Regex) demonstrates the need for proper parser support.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Engine already handles PRINTDATA (9 variants in FunctionIdentifier.cs line 158-166); parser extension follows DATALIST pattern |
| Scope is realistic | YES | ~200 lines addition; follows existing ErbParser structure for DATALIST |
| No blocking constraints | YES | Engine PRINTDATA handling is reference-only; parser is independent |

**Verdict**: FEASIBLE

The ErbParser already has the structural patterns needed (DatalistNode, IfNode). Adding PrintDataNode follows the same AST pattern. The engine code in FunctionIdentifier.cs (lines 158-166, 413-421) provides reference for all 9 PRINTDATA variants.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F555 | [DONE] | Phase 19 Planning - created this feature |
| Related | F346 | [DONE] | Original ERB Parser foundation |
| Related | F349 | [DONE] | DATALIST Converter - integration point |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| ErbParser assembly | Build-time | Low | Self-contained C# project |
| ErbToYaml | Runtime | Low | Consumer of parser AST |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/ErbToYaml.Tests/PilotConversionTests.cs | HIGH | Will replace regex workaround with proper AST |
| F634 Batch Conversion Tool | HIGH | Depends on PrintDataNode for automated extraction |
| DatalistConverter.cs | MEDIUM | May need updates to accept PrintDataNode |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbParser/Ast/PrintDataNode.cs | Create | New AST node for PRINTDATA...ENDDATA blocks |
| tools/ErbParser/ErbParser.cs | Update | Add PRINTDATA/ENDDATA parsing logic |
| tools/ErbParser.Tests/PrintDataParseTests.cs | Create | Unit tests for PRINTDATA parsing |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| 9 PRINTDATA variants | engine/FunctionIdentifier.cs | Must support: PRINTDATA, PRINTDATAL, PRINTDATAW, PRINTDATAK, PRINTDATAKL, PRINTDATAKW, PRINTDATAD, PRINTDATADL, PRINTDATADW |
| Nested DATALIST | kojo file structure | PRINTDATA blocks contain DATALIST...ENDLIST blocks |
| Conditional branches | F349 pattern | IF/ELSEIF/ELSE within PRINTDATA (like current DATALIST handling) |
| Parser state machine | ErbParser.cs | Must track inPrintData state similar to inDatalist |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Complex nesting patterns | Low | Medium | Follow engine's nestCheck logic in ErbLoader.cs (line 895-955) |
| Variant-specific behavior | Low | Low | All variants end with ENDDATA; differences are output format only |
| Breaking existing tests | Low | Medium | Add tests first; don't modify existing DATALIST behavior |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

PRINTDATA...ENDDATA parsing follows the established DATALIST...ENDLIST pattern in ErbParser. The implementation consists of three components:

1. **PrintDataNode AST Class** (similar to DatalistNode):
   - Inherits from AstNode
   - Stores Variant property (PRINTDATA, PRINTDATAL, PRINTDATAW, PRINTDATAK, PRINTDATAKL, PRINTDATAKW, PRINTDATAD, PRINTDATADL, PRINTDATADW)
   - Contains `List<AstNode> Content` for nested elements (DatalistNode, IfNode, DataformNode, PrintformNode)
   - Provides `GetDataForms()` helper method that recursively extracts all DataformNode instances from nested content

2. **Parser State Machine Extension**:
   - Add `inPrintData` boolean flag (parallel to existing `inDatalist` flag)
   - Add `currentPrintData` tracking variable (parallel to `currentDatalist`)
   - Detect PRINTDATA variants at line start (9 variants: PRINTDATA/L/W/D/DL/DW/K/KL/KW)
   - Parse nested content within PRINTDATA...ENDDATA block:
     - DATALIST blocks (create DatalistNode, add to PrintDataNode.Content)
     - IF/ELSEIF/ELSE blocks (create IfNode, add to PrintDataNode.Content)
     - Standalone DATAFORM (add to PrintDataNode.Content)
     - Standalone PRINTFORM (add to PrintDataNode.Content)
   - Detect ENDDATA terminator and close block
   - Error handling: throw ParseException on unclosed PRINTDATA, nested PRINTDATA (not allowed)

3. **Test Suite**:
   - PrintDataParseTests.cs with test cases covering:
     - Simple PRINTDATA parsing (AC#8)
     - Nested DATALIST within PRINTDATA (AC#6)
     - IF conditionals within PRINTDATA (AC#7)
     - Unclosed PRINTDATA error (AC#9)
     - All 9 PRINTDATA variants
     - GetDataForms() content extraction (AC#10)

**Rationale**: This approach directly mirrors the existing DATALIST parsing logic, minimizing implementation risk and maintaining architectural consistency. The PrintDataNode acts as a container that preserves the AST structure while providing a convenience API (GetDataForms) for downstream F634 batch conversion.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `tools/ErbParser/Ast/PrintDataNode.cs` following DatalistNode.cs pattern |
| 2 | Add `public string Variant { get; set; } = string.Empty;` property to PrintDataNode |
| 3 | Add `public List<AstNode> Content { get; } = new();` property to PrintDataNode for nested elements |
| 4 | Add PRINTDATA detection in ErbParser.cs ParseString method: `if (line.StartsWith("PRINTDATA", StringComparison.OrdinalIgnoreCase))` with variant extraction |
| 5 | Add ENDDATA detection in ErbParser.cs: `if (line.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))` with state check |
| 6 | When parsing DATALIST inside `inPrintData` block, add DatalistNode to `currentPrintData.Content` instead of top-level nodes |
| 7 | When parsing IF inside `inPrintData` block, add IfNode to `currentPrintData.Content` (similar to current DATALIST conditional branch handling) |
| 8 | Create xUnit test `ParseSimplePrintData` in PrintDataParseTests.cs that parses simple PRINTDATA with DATAFORM lines and asserts node creation |
| 9 | Create xUnit test `UnclosedPrintData` that expects ParseException when ENDDATA is missing |
| 10 | Add `public IEnumerable<DataformNode> GetDataForms()` method that recursively traverses Content collection and yields all DataformNode instances |
| 11 | Create `tools/ErbParser.Tests/PrintDataParseTests.cs` with all PRINTDATA test cases |
| 12 | Write implementation without TODO/FIXME/HACK markers; review code before commit |
| 13 | Run `dotnet build tools/ErbParser` and verify exit code 0 |
| 14 | Run `dotnet test tools/ErbParser.Tests` and verify all tests pass including existing DATALIST tests |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| PrintDataNode structure | A) Simple container with List<AstNode>, B) Specialized properties for each child type (DatalistNodes, IfNodes, etc.), C) Flat List<DataformNode> only | A) List<AstNode> | Preserves full AST structure for future use cases beyond F634; provides flexibility for nested content; mirrors IfNode.Body pattern; GetDataForms() provides flat extraction for F634 without losing structural information |
| Variant storage | A) Enum PrintDataVariant, B) String property, C) Separate node classes per variant | B) String property | Engine uses string-based FunctionCode matching; simplifies parsing; no behavioral differences between variants (only output format); avoids 9 separate node classes |
| Content extraction API | A) No helper (F634 traverses AST manually), B) GetDataForms() iterator, C) ToFlatList() eager collection | B) GetDataForms() iterator | Lazy evaluation with yield return; memory-efficient for large kojo files; simple API for F634; follows C# best practices for collection enumeration |
| Nesting rules | A) Allow nested PRINTDATA, B) Disallow nested PRINTDATA, C) No validation | B) Disallow nested PRINTDATA | Matches engine behavior (funcMatch shows PRINTDATA→ENDDATA pairing); prevents ambiguous ENDDATA matching; follows DATALIST nesting rules (also disallowed) |
| DATALIST handling inside PRINTDATA | A) Parse as flat content, B) Parse as nested DatalistNode in Content collection | B) Nested DatalistNode | Preserves semantic structure; real kojo files have DATALIST...ENDLIST inside PRINTDATA; enables future analysis of dialogue organization; matches actual ERB file structure |
| IF block handling inside PRINTDATA | A) Add to ConditionalBranches property (like DATALIST), B) Add to Content collection | B) Add to Content | PrintDataNode can contain any nested content (DATALIST, IF, standalone DATAFORM); Content collection is more general-purpose; matches IfNode.Body pattern |

### Interfaces / Data Structures

**PrintDataNode.cs**:
```csharp
namespace ErbParser.Ast;

/// <summary>
/// Represents a PRINTDATA...ENDDATA block
/// Contains nested DATALIST blocks, IF conditionals, and standalone DATAFORM/PRINTFORM statements
/// </summary>
public class PrintDataNode : AstNode
{
    /// <summary>
    /// PRINTDATA variant type (PRINTDATA, PRINTDATAL, PRINTDATAW, PRINTDATAK, PRINTDATAKL, PRINTDATAKW, PRINTDATAD, PRINTDATADL, PRINTDATADW)
    /// </summary>
    public string Variant { get; set; } = string.Empty;

    /// <summary>
    /// Nested content within PRINTDATA block (DatalistNode, IfNode, DataformNode, PrintformNode)
    /// </summary>
    public List<AstNode> Content { get; } = new();

    /// <summary>
    /// Extract all DataformNode instances from nested content (recursive traversal)
    /// Used by F634 batch converter for flat dialogue content extraction
    /// </summary>
    /// <returns>Iterator of all DataformNode instances found in Content tree</returns>
    public IEnumerable<DataformNode> GetDataForms()
    {
        return ExtractDataFormsFromNodes(Content);
    }

    /// <summary>
    /// Recursively extracts DataformNode instances from a collection of AST nodes
    /// </summary>
    /// <param name="nodes">Collection of AST nodes to traverse</param>
    /// <returns>Iterator of all DataformNode instances found in the node tree</returns>
    private IEnumerable<DataformNode> ExtractDataFormsFromNodes(IEnumerable<AstNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is DataformNode dataform)
            {
                yield return dataform;
            }
            else if (node is DatalistNode datalist)
            {
                foreach (var df in datalist.DataForms)
                {
                    yield return df;
                }
            }
            else if (node is IfNode ifNode)
            {
                // Recursively extract from IF body
                foreach (var df in ExtractDataFormsFromNodes(ifNode.Body))
                {
                    yield return df;
                }

                // Extract from ELSEIF branches
                foreach (var elseIf in ifNode.ElseIfBranches)
                {
                    foreach (var df in ExtractDataFormsFromNodes(elseIf.Body))
                    {
                        yield return df;
                    }
                }

                // Extract from ELSE branch
                if (ifNode.ElseBranch != null)
                {
                    foreach (var df in ExtractDataFormsFromNodes(ifNode.ElseBranch.Body))
                    {
                        yield return df;
                    }
                }
            }
        }
    }
}
```

**Parser State Machine Extension (ErbParser.cs ParseString method)**:
```csharp
// Add to method variables (line ~33):
bool inPrintData = false;
PrintDataNode? currentPrintData = null;

// Add detection block after DATALIST/ENDLIST blocks:
// Check for PRINTDATA (9 variants)
if (line.StartsWith("PRINTDATA", StringComparison.OrdinalIgnoreCase))
{
    if (inPrintData)
    {
        throw new ParseException("Nested PRINTDATA is not allowed", fileName, lineNumber);
    }
    inPrintData = true;
    currentPrintData = new PrintDataNode
    {
        LineNumber = lineNumber,
        SourceFile = fileName,
        Variant = line.Split()[0] // Extract variant (PRINTDATA/PRINTDATAL/etc)
    };
    continue;
}

// Check for ENDDATA
if (line.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
{
    if (!inPrintData || currentPrintData == null)
    {
        throw new ParseException("ENDDATA without matching PRINTDATA", fileName, lineNumber);
    }
    nodes.Add(currentPrintData);
    inPrintData = false;
    currentPrintData = null;
    continue;
}

// Modify DATALIST block handling (line ~46-58):
// When DATALIST is detected inside inPrintData, parse it but add to currentPrintData.Content instead of top-level nodes

// Modify IF block handling (line ~88-104):
// When IF is detected inside inPrintData, add to currentPrintData.Content

// Add unclosed PRINTDATA check at end (line ~124):
if (inPrintData)
{
    throw new ParseException("PRINTDATA without matching ENDDATA", fileName, lineNumber);
}
```

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- [resolved] Phase1 iter2: AC table contains only placeholder template - /fc 633 generated 14 ACs
- [resolved] Phase1 iter2: Tasks table contains only placeholder template - /fc 633 generated 11 tasks
- [resolved] Phase1 iter2: AC Details section contains only placeholder - /fc 633 generated detailed test specs
- [resolved] Phase1 iter2: Goal #3 vague - GetDataForms() API specified in Technical Design
- [resolved] Phase1 iter3: Goal #3 vague - PrintDataNode.GetDataForms() method for F634 integration
- [resolved] Phase2 iter3: AC Definition Table placeholders - 14 concrete ACs with types/matchers
- [resolved] Phase2 iter3: Tasks Table placeholders - 11 tasks with AC:Task 1:1 alignment
- [resolved] Phase2 iter3: AC Details placeholders - Test commands and expected outputs documented
- [resolved] Phase2 iter3: Goal #3 vague API - GetDataForms() method specified
- [resolved] Phase2 iter3: Missing Implementation Contract - 3-phase contract with constraints
- [resolved] Phase3 iter3: AC table - 14 ACs with proper types/matchers
- [resolved] Phase3 iter3: AC Details - Test commands documented
- [resolved] Phase3 iter3: Tasks table - 11 tasks with batch waivers documented
- [resolved] Phase3 iter3: Missing Pos/Neg coverage - AC#8 (positive), AC#9 (negative)
- [resolved-applied] Phase3-ACValidation iter2: Invalid matcher 'not_matches' - not in valid matchers list

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

| 課題 | 理由 | 追跡先 | 追跡先ID | Creation Task |
|------|------|--------|----------|-------------|
| - | - | - | - | No handoffs required |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-27 | DEVIATION | implementer | dotnet test ErbParser.Tests | PrintDataGetDataForms_FromConditional FAIL: DATAFORM inside IF not parsed when inPrintData=true, inDatalist=false |
| 2026-01-27 | FIX | debugger | ErbParser.cs L142 | Changed ParseIfBlock call to pass `inDatalist \|\| inPrintData` - 72/72 tests pass |

---

## Links

- [feature-555.md](feature-555.md) - Phase 19 Planning
- [feature-634.md](feature-634.md) - Batch Conversion Tool (Successor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 19 section

### Implementation Artifacts (Created during /run)
- `tools/ErbParser/Ast/PrintDataNode.cs` - AST node for PRINTDATA blocks
- `tools/ErbParser.Tests/PrintDataParseTests.cs` - Unit tests for parser extension
