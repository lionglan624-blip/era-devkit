# Feature 650: ErbParser ENDDATA Edge Case Fix

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

## Type: erb

## Created: 2026-01-28

---

## Summary

Add STRDATA...ENDDATA block recognition to ErbParser. NTR口上_お持ち帰り.ERB line 1574 uses STRDATA (not PRINTDATA), but both share ENDDATA terminator. Parser throws "ENDDATA without matching PRINTDATA" because STRDATA is unrecognized. Fix: Add STRDATA skip logic parallel to PRINTDATA handling.

---

## Background

### Philosophy (Mid-term Vision)

Phase 19: Kojo Conversion - All ERB files must be parseable for complete batch conversion.

### Problem (Current Issue)

`Game/ERB/口上/4_咲夜/NTR口上_お持ち帰り.ERB` line 1570-1574 contains a `STRDATA 悲鳴` block terminated by ENDDATA. ErbParser (F633) only recognizes PRINTDATA...ENDDATA, not STRDATA...ENDDATA. When the parser encounters the ENDDATA on line 1574, it throws "ENDDATA without matching PRINTDATA" because no PRINTDATA is open. This blocks F639 AC#3 (batch conversion succeeds) and affects NTR口上_お持ち帰り files in multiple character directories.

### Goal (What to Achieve)

1. Investigate the specific ERB structure at line 1574
2. Determine root cause: malformed ERB file or parser limitation
3. Fix parser edge case or ERB file structure
4. Verify NTR口上_お持ち帰り.ERB converts successfully after fix

---

## Links

- [feature-639.md](feature-639.md) - Sakuya Kojo Conversion (origin: AC#3 blocker)
- [feature-633.md](feature-633.md) - PRINTDATA Parser Extension (foundation)
- [feature-634.md](feature-634.md) - Kojo Conversion Context
- [feature-636.md](feature-636.md) - Meiling Kojo Conversion
- [feature-637.md](feature-637.md) - Flandre Kojo Conversion
- [feature-638.md](feature-638.md) - Remilia Kojo Conversion
- [feature-640.md](feature-640.md) - Youmu Kojo Conversion
- [feature-641.md](feature-641.md) - Reimu Kojo Conversion
- [feature-642.md](feature-642.md) - Marisa Kojo Conversion
- [feature-643.md](feature-643.md) - Alice Kojo Conversion
- [feature-349.md](feature-349.md) - DATALIST→YAML Converter
- [feature-633.md](feature-633.md) - PRINTDATA Parser Extension (parser source)
- [feature-634.md](feature-634.md) - Batch Conversion Tool

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: ErbParser throws "ENDDATA without matching PRINTDATA" at NTR口上_お持ち帰り.ERB line 1574
2. Why: The ENDDATA at line 1574 closes a STRDATA block (lines 1570-1574), not a PRINTDATA block
3. Why: ErbParser recognizes PRINTDATA...ENDDATA but does NOT recognize STRDATA...ENDDATA as a valid construct
4. Why: F633 (PRINTDATA Parser Extension) only implemented PRINTDATA variants; STRDATA was not considered during design
5. Why: STRDATA is a separate command type that also uses ENDDATA as its terminator (per `funcMatch[FunctionCode.STRDATA] = "ENDDATA"` in FunctionIdentifier.cs line 423), but has different semantics (string variable assignment vs display output)

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| ErbParser throws ParseException "ENDDATA without matching PRINTDATA" at line 1574 | ErbParser does not recognize STRDATA command - only PRINTDATA uses ENDDATA terminator in current parser |
| Batch conversion fails with exit code 1 on NTR口上_お持ち帰り.ERB | Parser limitation: STRDATA...ENDDATA blocks are valid Emuera constructs but unsupported by ErbParser |

### Conclusion

The root cause is a **parser limitation**, not a malformed ERB file. The ERB file structure at lines 1570-1574 is:

```erb
STRDATA 悲鳴
    DATA 「ああっ！中に、中にくるううっ！」
    DATA 「ああっ！あついっ！ あついのぉぉぉ！」
    DATA 「ああっ！出てるっ！ いっぱい出てるぅっ！」
ENDDATA
```

This is a valid Emuera construct: `STRDATA <variable>` selects a random DATA line and assigns it to the string variable (see `StrDataCommand.cs`). Both PRINTDATA and STRDATA use `ENDDATA` as their terminator (per `FunctionIdentifier.cs` line 413-423), but ErbParser only implemented PRINTDATA support in F633.

**Key Insight**: STRDATA is semantically different from PRINTDATA:
- **PRINTDATA**: Display output command (random selection for display)
- **STRDATA**: Variable assignment command (random selection stored in string variable)

For batch conversion purposes, STRDATA blocks contain dialogue text that should be converted to YAML. The parser must be extended to recognize STRDATA...ENDDATA.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F633 | [DONE] | Parser foundation | PRINTDATA Parser Extension - same ENDDATA terminator, missing STRDATA handling |
| F639 | [BLOCKED] | Origin/Blocker | Sakuya Kojo Conversion - AC#3 blocked by this parse error |
| F636 | [BLOCKED] | Affected | Meiling Kojo Conversion - 10_魔理沙/NTR口上_お持ち帰り.ERB has 3 STRDATA blocks |
| F637-F643 | [DRAFT]/[BLOCKED] | Affected | All character conversions may encounter STRDATA in NTR files |

### Pattern Analysis

This is **NOT a recurring pattern** - it's an initial oversight in F633 design. The Feature 633 Root Cause Analysis (line 292-296) stated "PRINTDATA...ENDDATA is a separate block construct" but did not account for STRDATA also using ENDDATA terminator. The engine handles both constructs identically at the block level (same `funcMatch` pattern), but ErbParser was designed for PRINTDATA-specific extraction.

**Scope of Impact**:
- 8 STRDATA occurrences across 3 files:
  - `Game/ERB/PREGNACY_S.ERB`: 2 occurrences (non-kojo file)
  - `Game/ERB/口上/10_魔理沙/NTR口上_お持ち帰り.ERB`: 3 occurrences
  - `Game/ERB/口上/4_咲夜/NTR口上_お持ち帰り.ERB`: 3 occurrences
- Only 2 kojo files are affected; both are NTR口上_お持ち帰り.ERB variants
- PREGNACY_S.ERB is not in kojo conversion scope

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Parser extension follows F633 pattern; STRDATA structure identical to PRINTDATA at block level |
| Scope is realistic | YES | Add STRDATA detection (~30 lines) parallel to existing PRINTDATA code (ErbParser.cs lines 87-115) |
| No blocking constraints | YES | Self-contained parser change; no external dependencies |

**Verdict**: FEASIBLE

The fix follows the established PRINTDATA pattern from F633:
1. Add `inStrData` and `currentStrData` state tracking (parallel to `inPrintData`)
2. Detect `STRDATA <variable>` at line start
3. Parse DATA/DATAFORM lines inside STRDATA block
4. Close block on ENDDATA (shared terminator with PRINTDATA)

**Design Decision Required**: Should STRDATA content be converted to YAML? STRDATA assigns dialogue text to a variable for later use (e.g., `%悲鳴%` expansion in PRINTFORML), unlike PRINTDATA which displays directly. For conversion purposes:
- **Option A**: Skip STRDATA blocks (variables used elsewhere, not standalone dialogue)
- **Option B**: Convert STRDATA to YAML with variable-name metadata (preserve for reference)
- **Recommendation**: Option A (skip) - STRDATA is control flow, not displayable content. The variable is referenced later in PRINTFORML statements which ARE converted.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F633 | [DONE] | PRINTDATA Parser Extension - provides code pattern to follow |
| Blocker-for | F639 | [BLOCKED] | Sakuya Kojo Conversion - AC#3 blocked by this parse error |
| Related | F636-F643 | Various | Character conversions - may encounter STRDATA in NTR files |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| ErbParser assembly | Build-time | Low | Self-contained C# project |
| engine/FunctionIdentifier.cs | Reference | None | Reference for STRDATA→ENDDATA mapping (read-only) |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/ErbToYaml/FileConverter.cs | LOW | Will skip STRDATA blocks (no YAML output needed for variable assignment) |
| F639 Sakuya Conversion | HIGH | Batch conversion will succeed after fix |
| F636-F643 Character Conversions | MEDIUM | NTR口上 files will parse successfully |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbParser/ErbParser.cs | Update | Add STRDATA...ENDDATA block detection and skip logic |
| tools/ErbParser.Tests/StrDataParseTests.cs | Create | Unit tests for STRDATA parsing |

**No new AST node required** - STRDATA blocks are skipped by the parser since their content is not convertible dialogue (it's variable assignment). The parser simply needs to recognize STRDATA to avoid the "ENDDATA without matching PRINTDATA" error.

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| STRDATA uses same ENDDATA terminator as PRINTDATA | engine/FunctionIdentifier.cs line 423 | MEDIUM - Must track separate state for STRDATA vs PRINTDATA |
| STRDATA takes variable argument (STRDATA <varname>) | engine/FunctionIdentifier.cs line 299 | LOW - Parser extracts but ignores variable name |
| DATA vs DATAFORM inside STRDATA | StrDataCommand.cs | LOW - Both are valid; STRDATA content skipped |
| No nested STRDATA allowed | engine/NestValidator.cs line 514-517 | LOW - Same constraint as PRINTDATA |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| STRDATA content should be converted (not skipped) | Low | Medium | Review STRDATA usage in kojo files; confirm variable is referenced in PRINTFORML (not standalone) |
| Other DATA-block commands exist (besides PRINTDATA, STRDATA) | Low | Low | Verified: only PRINTDATA (9 variants), STRDATA, DATALIST use ENDDATA/ENDLIST terminators |
| STRDATA inside IF blocks (nested context) | Medium | Medium | Follow PRINTDATA-inside-IF pattern from F633 (lines 360-441, 521-602, 670-751) |

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "All ERB files" | Every kojo ERB file in conversion scope must parse without error | AC#4, AC#5 |
| "must be parseable" | Parser must recognize STRDATA...ENDDATA as valid block construct | AC#1, AC#2, AC#3 |
| "complete batch conversion" | Batch conversion on affected files succeeds with exit code 0 | AC#6, AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | STRDATA state tracking exists | code | Grep(tools/ErbParser/ErbParser.cs) | contains | "inStrData" | [x] |
| 2 | STRDATA detection logic exists | code | Grep(tools/ErbParser/ErbParser.cs) | matches | "STRDATA.*OrdinalIgnoreCase" | [x] |
| 3 | Unit test file exists | file | Glob | exists | tools/ErbParser.Tests/StrDataParseTests.cs | [x] |
| 4 | Basic STRDATA parse test | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~StrDataParseTests.ParsesBasicStrDataBlock | succeeds | - | [x] |
| 5 | Nested STRDATA error test | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~StrDataParseTests.ThrowsOnNestedStrData | succeeds | - | [x] |
| 6 | Sakuya NTR file parses | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~StrDataParseTests.ParsesSakuyaNtrFile | succeeds | - | [x] |
| 7 | Marisa NTR file parses | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~StrDataParseTests.ParsesMarisaNtrFile | succeeds | - | [x] |
| 8 | PRINTDATA regression test | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~PrintDataParseTests | succeeds | - | [x] |
| 9 | Zero technical debt | code | Grep(tools/ErbParser/ErbParser.cs) | not_contains | "TODO" | [x] |
| 10 | ErbParser build succeeds | build | dotnet build tools/ErbParser | succeeds | - | [x] |
| 11 | Unclosed STRDATA error test | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~StrDataParseTests.ThrowsOnUnclosedStrData | succeeds | - | [x] |

### AC Details

**AC#1: STRDATA state tracking exists**
- Test: Grep pattern=`inStrData` path=`tools/ErbParser/ErbParser.cs`
- Expected: Match found indicating boolean state variable for STRDATA block tracking
- Rationale: Parallel to existing `inPrintData` state tracking (line 36)

**AC#2: STRDATA detection logic exists in all parsing contexts**
- Test: Grep pattern=`STRDATA.*OrdinalIgnoreCase` path=`tools/ErbParser/ErbParser.cs`
- Expected: Multiple matches found indicating case-insensitive STRDATA command detection in main loop, ParseIfBlock, ParseElseIfBranch, ParseElseBranch methods
- Rationale: STRDATA blocks appear in IF/ELSE context (ERB lines 1325-1335), requiring explicit handling in all parsing methods

**AC#3: Unit test file exists**
- Test: Glob pattern=`tools/ErbParser.Tests/StrDataParseTests.cs`
- Expected: File exists
- Rationale: New test file for STRDATA-specific tests, parallel to existing PrintDataParseTests

**AC#4: Basic STRDATA parse test**
- Test: Unit test verifying parser handles STRDATA...ENDDATA block without throwing
- Input: `STRDATA varname\n    DATA line1\n    DATA line2\nENDDATA`
- Expected: Parser completes successfully, no nodes added (STRDATA is skipped, not converted)
- Rationale: Core functionality - parser must recognize and skip STRDATA blocks

**AC#5: Nested STRDATA error test**
- Test: Unit test verifying parser throws on nested STRDATA (same constraint as PRINTDATA)
- Input: `STRDATA var1\n    STRDATA var2\nENDDATA`
- Expected: ParseException "Nested STRDATA is not allowed"
- Rationale: Validate error handling follows PRINTDATA pattern

**AC#6: Sakuya NTR file parses**
- Test: Integration test parsing `Game/ERB/口上/4_咲夜/NTR口上_お持ち帰り.ERB`
- Expected: Parser completes without throwing ParseException
- Rationale: The original failing file (line 1574) must now parse successfully

**AC#7: Marisa NTR file parses**
- Test: Integration test parsing `Game/ERB/口上/10_魔理沙/NTR口上_お持ち帰り.ERB`
- Expected: Parser completes without throwing ParseException
- Rationale: Second affected kojo file with 3 STRDATA occurrences

**AC#8: PRINTDATA regression test**
- Test: All existing PRINTDATA tests continue to pass
- Expected: No test failures in PrintDataParseTests
- Rationale: STRDATA addition must not break existing PRINTDATA functionality

**AC#9: Zero technical debt**
- Test: Grep pattern=`TODO` path=`tools/ErbParser/ErbParser.cs`
- Expected: 0 matches
- Rationale: Clean implementation without deferred work markers

**AC#10: ErbParser build succeeds**
- Test: `dotnet build tools/ErbParser`
- Expected: Exit code 0
- Rationale: Code compiles without errors

**AC#11: Unclosed STRDATA error test**
- Test: Unit test verifying parser throws on unclosed STRDATA at EOF
- Input: `STRDATA varname\n    DATA line1\n[EOF without ENDDATA]`
- Expected: ParseException "STRDATA without matching ENDDATA"
- Rationale: Error handling completeness - validates EOF state checking similar to PRINTDATA

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The implementation follows the established PRINTDATA pattern from F633 with three core changes:

1. **State tracking**: Add `inStrData` boolean and `currentStrData` StrDataNode variable (parallel to `inPrintData`/`currentPrintData` at lines 36-37)
2. **STRDATA detection**: Detect `STRDATA <variable>` command at line start (parallel to PRINTDATA detection at lines 87-102)
3. **Skip content**: Parse and discard STRDATA block content without adding AST nodes (STRDATA is variable assignment, not displayable dialogue)

**Key difference from PRINTDATA**: STRDATA content is NOT added to the AST. The parser recognizes the block structure to consume lines until ENDDATA, but does not create output nodes. This is because:
- STRDATA assigns a random DATA line to a string variable for later interpolation (e.g., `%悲鳴%` in PRINTFORML)
- The actual dialogue display happens in PRINTFORML statements, which ARE converted
- Converting STRDATA blocks would create duplicate YAML content (same text appears in both STRDATA and its usage site)

**No new AST node required**: Unlike PRINTDATA which creates PrintDataNode, STRDATA is recognized but skipped. The parser's role is to prevent "ENDDATA without matching PRINTDATA" errors, not to extract STRDATA content.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `private bool inStrData = false;` field at line 37 (after `inPrintData` declaration) |
| 2 | Add STRDATA detection at line 116 (after ENDDATA handler): `if (line.StartsWith("STRDATA", StringComparison.OrdinalIgnoreCase))` with nested error check `if (inStrData) throw ParseException("Nested STRDATA is not allowed")` |
| 3 | Create new test file `tools/ErbParser.Tests/StrDataParseTests.cs` with 4 test methods (basic, nested error, Sakuya file, Marisa file) |
| 4 | Write unit test `ParsesBasicStrDataBlock()` that parses `STRDATA var\n    DATA line1\nENDDATA` and verifies no nodes added (empty AST) |
| 5 | Write unit test `ThrowsOnNestedStrData()` that verifies nested STRDATA throws ParseException with message "Nested STRDATA is not allowed" |
| 6 | Write integration test `ParsesSakuyaNtrFile()` that parses full `Game/ERB/口上/4_咲夜/NTR口上_お持ち帰り.ERB` and verifies no ParseException thrown |
| 7 | Write integration test `ParsesMarisaNtrFile()` that parses full `Game/ERB/口上/10_魔理沙/NTR口上_お持ち帰り.ERB` and verifies no ParseException thrown |
| 8 | All existing PrintDataParseTests continue to pass (STRDATA logic is isolated in separate state tracking) |
| 9 | Implementation uses clean code without TODO/FIXME/HACK comments |
| 10 | Code compiles with `dotnet build tools/ErbParser` exit code 0 |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **STRDATA content handling** | A) Skip block (no AST nodes), B) Create StrDataNode with content | A (Skip) | STRDATA is variable assignment control flow, not displayable content. The dialogue text is referenced later in PRINTFORML statements which ARE converted. Converting STRDATA would create duplicate YAML entries. |
| **AST node creation** | A) No new node type, B) Create StrDataNode (parallel to PrintDataNode) | A (No new node) | Skipping STRDATA blocks requires only state tracking to consume lines until ENDDATA. No AST representation needed since content is not extracted. Simpler implementation with zero impact on FileConverter. |
| **Error message text** | A) "ENDDATA without matching PRINTDATA or STRDATA", B) Keep existing message, handle via state | B (Keep existing) | Line 109 error message remains unchanged. STRDATA state prevents reaching that error path. Cleaner separation of concerns - PRINTDATA error is still accurate when inPrintData=false && inStrData=false. |
| **State variable placement** | A) Add after inPrintData (line 37), B) Add before inPrintData | A (After) | Maintains chronological order (DATALIST added first in F349, PRINTDATA in F633, STRDATA now). Easier to locate related state variables. |
| **STRDATA variant detection** | A) Exact match "STRDATA", B) StartsWith "STRDATA" | B (StartsWith) | Consistent with PRINTDATA pattern (line 88). STRDATA has only one variant (no STRDATAL/STRDATAW), but StartsWith is more defensive if engine adds variants. |

### Implementation Details

#### Code Structure

**Location**: `tools/ErbParser/ErbParser.cs`

**State tracking (after line 37)**:
```csharp
bool inStrData = false;
```

**STRDATA detection (after line 115, before DATAFORM check)**:
```csharp
// Check for STRDATA
if (line.StartsWith("STRDATA", StringComparison.OrdinalIgnoreCase))
{
    if (inStrData)
    {
        throw new ParseException("Nested STRDATA is not allowed", fileName, lineNumber);
    }
    inStrData = true;
    continue;
}
```

**ENDDATA handler update (replace lines 104-115)**:
```csharp
// Check for ENDDATA
if (line.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
{
    if (inPrintData && currentPrintData != null)
    {
        nodes.Add(currentPrintData);
        inPrintData = false;
        currentPrintData = null;
    }
    else if (inStrData)
    {
        // STRDATA content is skipped (not added to AST)
        inStrData = false;
    }
    else
    {
        throw new ParseException("ENDDATA without matching PRINTDATA", fileName, lineNumber);
    }
    continue;
}
```

**End-of-file validation (after line 200)**:
```csharp
// Check for unclosed STRDATA
if (inStrData)
{
    throw new ParseException("STRDATA without matching ENDDATA", fileName, lineNumber);
}
```

#### Test Structure

**File**: `tools/ErbParser.Tests/StrDataParseTests.cs`

**Test methods**:
1. `ParsesBasicStrDataBlock()` - Verify basic STRDATA...ENDDATA parsing with no AST nodes created
2. `ThrowsOnNestedStrData()` - Verify nested STRDATA throws ParseException
3. `ThrowsOnUnclosedStrData()` - Verify unclosed STRDATA at EOF throws ParseException
3. `ParsesSakuyaNtrFile()` - Integration test for `Game/ERB/口上/4_咲夜/NTR口上_お持ち帰り.ERB`
4. `ParsesMarisaNtrFile()` - Integration test for `Game/ERB/口上/10_魔理沙/NTR口上_お持ち帰り.ERB`

**Test data files** (create in `tools/ErbParser.Tests/TestData/`):
- `basic_strdata.erb`: Simple STRDATA block for unit test
- `nested_strdata.erb`: Nested STRDATA for error test

#### Edge Cases Handled

| Edge Case | Behavior | Test Coverage |
|-----------|----------|---------------|
| Nested STRDATA | Throw ParseException "Nested STRDATA is not allowed" | AC#5 unit test |
| ENDDATA without PRINTDATA or STRDATA | Throw ParseException "ENDDATA without matching PRINTDATA" | Existing SyntaxErrorTests |
| Unclosed STRDATA at EOF | Throw ParseException "STRDATA without matching ENDDATA" | New unit test (implicit in basic test) |
| STRDATA inside IF block | Requires explicit STRDATA skip logic in ParseIfBlock, ParseElseIfBranch, ParseElseBranch methods | AC#6, AC#7 integration tests |
| DATA/DATAFORM inside STRDATA | Consumed and ignored (no AST nodes created) | AC#4 unit test |

### Pattern Consistency

The implementation follows F633 PRINTDATA patterns:

| Pattern | PRINTDATA (F633) | STRDATA (F650) |
|---------|------------------|----------------|
| State variable | `inPrintData` (line 36) | `inStrData` (after line 37) |
| Current node | `currentPrintData` (line 37) | None (content skipped) |
| Detection | `line.StartsWith("PRINTDATA", ...)` (line 88) | `line.StartsWith("STRDATA", ...)` (after line 115) |
| Nesting check | `if (inPrintData) throw ...` (line 90) | `if (inStrData) throw ...` (new) |
| Terminator | `ENDDATA` closes block (line 105) | `ENDDATA` closes block (updated line 105) |
| EOF validation | Line 198 unclosed check | After line 200 unclosed check |

### Downstream Impact

**FileConverter (tools/ErbToYaml/FileConverter.cs)**:
- No changes required - STRDATA content is not in AST, so converter never encounters it
- Existing PRINTDATA/DATALIST conversion logic unchanged

**Batch Conversion (F639, F636-F643)**:
- After this fix, `dotnet run --project tools/ErbToYaml -- Game/ERB/口上/4_咲夜/NTR口上_お持ち帰り.ERB` succeeds
- NTR口上 files in all character directories will parse successfully

### Code Quality Verification

**AC#9 compliance**:
- No TODO/FIXME/HACK comments in implementation
- All edge cases handled with explicit error messages
- Pattern follows existing codebase conventions (OrdinalIgnoreCase, early continue, explicit state checks)

<!-- fc-phase-5-completed -->
## Tasks

### WBS (Work Breakdown Structure)

| Task# | Description | AC# | Estimated Lines | Dependencies | Status |
|:-----:|-------------|:---:|:---------------:|--------------|:------:|
| 1 | Add STRDATA state tracking field to ErbParser.cs | 1 | 5 | - | [x] |
| 2 | Implement STRDATA command detection logic | 2 | 15 | T1 | [x] |
| 2a | Add STRDATA skip logic to ParseIfBlock method | 2 | 20 | T1 | [x] |
| 2b | Add STRDATA skip logic to ParseElseIfBranch method | 2 | 20 | T1 | [x] |
| 2c | Add STRDATA skip logic to ParseElseBranch method | 2 | 20 | T1 | [x] |
| 3 | Update ENDDATA handler to support STRDATA termination | 2 | 20 | T1 | [x] |
| 4 | Add EOF validation for unclosed STRDATA blocks | 2 | 10 | T1 | [x] |
| 5 | Create StrDataParseTests.cs test file with class structure | 3 | 30 | - | [x] |
| 6 | Implement basic STRDATA parse unit test | 4 | 25 | T5 | [x] |
| 7 | Implement nested STRDATA error test | 5 | 20 | T5 | [x] |
| 8 | Create test data files (basic_strdata.erb, nested_strdata.erb) | 4, 5 | 15 | T5 | [x] |
| 9 | Implement Sakuya NTR integration test | 6 | 20 | T5 | [x] |
| 10 | Implement Marisa NTR integration test | 7 | 20 | T5 | [x] |
| 11 | Run PRINTDATA regression test suite | 8 | 0 | T1-T4 | [x] |
| 12 | Verify zero technical debt (Grep TODO/FIXME/HACK) | 9 | 0 | T1-T4 | [x] |
| 13 | Verify ErbParser build succeeds | 10 | 0 | T1-T12 | [x] |
| 14 | Implement unclosed STRDATA error test | 11 | 20 | T5 | [x] |

### Task Details

**Task 1: Add STRDATA state tracking field**
- **AC Coverage**: AC#1
- **Implementation**: Add `private bool inStrData = false;` field at line 38 (after existing `inPrintData` declaration at line 36)
- **Pattern**: Parallel to PRINTDATA state tracking (F633)
- **Validation**: Grep pattern `inStrData` matches in ErbParser.cs

**Task 2: Implement STRDATA command detection logic**
- **AC Coverage**: AC#2 (main loop only)
- **Implementation**: Add STRDATA detection block after line 115 (before DATAFORM check):
  ```csharp
  // Check for STRDATA
  if (line.StartsWith("STRDATA", StringComparison.OrdinalIgnoreCase))
  {
      if (inStrData)
      {
          throw new ParseException("Nested STRDATA is not allowed", fileName, lineNumber);
      }
      inStrData = true;
      continue;
  }
  ```
- **Pattern**: Follows PRINTDATA detection pattern (line 88-102)
- **Validation**: Grep pattern `STRDATA.*OrdinalIgnoreCase` matches in ErbParser.cs
- **Dependencies**: T1 (requires inStrData field)

**Task 2a: Add STRDATA skip logic to ParseIfBlock method**
- **AC Coverage**: AC#2 (IF body parsing)
- **Implementation**: Add STRDATA handling to ParseIfBlock method (mirror PRINTDATA pattern at lines 360-441):
  ```csharp
  // Add after PRINTDATA handling block around line 441
  if (trimmedLine.StartsWith("STRDATA", StringComparison.OrdinalIgnoreCase))
  {
      while (i < lines.Count)
      {
          i++;
          if (i >= lines.Count) break;
          string dataLine = lines[i].Trim();
          if (dataLine.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
          {
              break; // STRDATA block complete
          }
          // Skip STRDATA content (not added to AST)
      }
      continue;
  }
  ```
- **Dependencies**: T1 (requires understanding of STRDATA pattern)

**Task 2b: Add STRDATA skip logic to ParseElseIfBranch method**
- **AC Coverage**: AC#2 (ELSEIF body parsing)
- **Implementation**: Add STRDATA handling to ParseElseIfBranch method (mirror PRINTDATA pattern at lines 521-602):
  ```csharp
  // Add after PRINTDATA handling block around line 602
  if (trimmedLine.StartsWith("STRDATA", StringComparison.OrdinalIgnoreCase))
  {
      while (i < lines.Count)
      {
          i++;
          if (i >= lines.Count) break;
          string dataLine = lines[i].Trim();
          if (dataLine.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
          {
              break; // STRDATA block complete
          }
          // Skip STRDATA content (not added to AST)
      }
      continue;
  }
  ```
- **Dependencies**: T1 (requires understanding of STRDATA pattern)

**Task 2c: Add STRDATA skip logic to ParseElseBranch method**
- **AC Coverage**: AC#2 (ELSE body parsing)
- **Implementation**: Add STRDATA handling to ParseElseBranch method (mirror PRINTDATA pattern at lines 670-751):
  ```csharp
  // Add after PRINTDATA handling block around line 751
  if (trimmedLine.StartsWith("STRDATA", StringComparison.OrdinalIgnoreCase))
  {
      while (i < lines.Count)
      {
          i++;
          if (i >= lines.Count) break;
          string dataLine = lines[i].Trim();
          if (dataLine.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
          {
              break; // STRDATA block complete
          }
          // Skip STRDATA content (not added to AST)
      }
      continue;
  }
  ```
- **Dependencies**: T1 (requires understanding of STRDATA pattern)

**Task 3: Update ENDDATA handler to support STRDATA termination**
- **AC Coverage**: AC#2
- **Implementation**: Replace ENDDATA handler (lines 104-115) to handle both PRINTDATA and STRDATA:
  ```csharp
  // Check for ENDDATA
  if (line.Equals("ENDDATA", StringComparison.OrdinalIgnoreCase))
  {
      if (inPrintData && currentPrintData != null)
      {
          nodes.Add(currentPrintData);
          inPrintData = false;
          currentPrintData = null;
      }
      else if (inStrData)
      {
          // STRDATA content is skipped (not added to AST)
          inStrData = false;
      }
      else
      {
          throw new ParseException("ENDDATA without matching PRINTDATA", fileName, lineNumber);
      }
      continue;
  }
  ```
- **Pattern**: Extends existing PRINTDATA ENDDATA handler
- **Validation**: Code compiles and STRDATA blocks parse without errors
- **Dependencies**: T1 (requires inStrData field)

**Task 4: Add EOF validation for unclosed STRDATA blocks**
- **AC Coverage**: AC#2
- **Implementation**: Add unclosed STRDATA check after line 200 (after existing unclosed checks):
  ```csharp
  // Check for unclosed STRDATA
  if (inStrData)
  {
      throw new ParseException("STRDATA without matching ENDDATA", fileName, lineNumber);
  }
  ```
- **Pattern**: Parallel to PRINTDATA EOF validation (line 198)
- **Validation**: Unclosed STRDATA throws appropriate error
- **Dependencies**: T1 (requires inStrData field)

**Task 5: Create StrDataParseTests.cs test file with class structure**
- **AC Coverage**: AC#3
- **Implementation**: Create `tools/ErbParser.Tests/StrDataParseTests.cs` with:
  - Namespace: `ErbParser.Tests`
  - Class: `public class StrDataParseTests`
  - Test fixture setup (if needed for file paths)
- **Pattern**: Parallel to existing PrintDataParseTests.cs
- **Validation**: File exists at expected path
- **Dependencies**: None (standalone test file creation)

**Task 6: Implement basic STRDATA parse unit test**
- **AC Coverage**: AC#4
- **Implementation**: Write `ParsesBasicStrDataBlock()` test method:
  - Input: `STRDATA 悲鳴\n    DATA line1\n    DATA line2\nENDDATA`
  - Expected: Parser completes successfully, AST is empty (no nodes added)
  - Assert: `Assert.Empty(result.Nodes)`
- **Pattern**: Uses in-memory string parsing
- **Validation**: `dotnet test --filter FullyQualifiedName~StrDataParseTests.ParsesBasicStrDataBlock` succeeds
- **Dependencies**: T5 (requires test file), T1-T4, T2a-T2c (requires parser implementation)

**Task 7: Implement nested STRDATA error test**
- **AC Coverage**: AC#5
- **Implementation**: Write `ThrowsOnNestedStrData()` test method:
  - Input: `STRDATA var1\n    STRDATA var2\nENDDATA`
  - Expected: ParseException with message "Nested STRDATA is not allowed"
  - Assert: `Assert.Throws<ParseException>()` with message validation
- **Pattern**: Error validation test
- **Validation**: `dotnet test --filter FullyQualifiedName~StrDataParseTests.ThrowsOnNestedStrData` succeeds
- **Dependencies**: T5 (requires test file), T1-T4, T2a-T2c (requires parser implementation)

**Task 8: Create test data files**
- **AC Coverage**: AC#4, AC#5
- **Implementation**: Create test data files in `tools/ErbParser.Tests/TestData/`:
  - `basic_strdata.erb`: Simple STRDATA block for T6
  - `nested_strdata.erb`: Nested STRDATA for T7
- **Pattern**: Follow existing test data file conventions
- **Validation**: Files exist and contain valid ERB syntax
- **Dependencies**: T5 (requires test directory structure)

**Task 9: Implement Sakuya NTR integration test**
- **AC Coverage**: AC#6
- **Implementation**: Write `ParsesSakuyaNtrFile()` test method:
  - Input: Full file path `Game/ERB/口上/4_咲夜/NTR口上_お持ち帰り.ERB`
  - Expected: Parser completes without throwing ParseException
  - Assert: No exception thrown, result has nodes (file contains PRINTDATA too)
- **Pattern**: Integration test with real kojo file
- **Validation**: `dotnet test --filter FullyQualifiedName~StrDataParseTests.ParsesSakuyaNtrFile` succeeds
- **Dependencies**: T5 (requires test file), T1-T4, T2a-T2c (requires parser implementation)

**Task 10: Implement Marisa NTR integration test**
- **AC Coverage**: AC#7
- **Implementation**: Write `ParsesMarisaNtrFile()` test method:
  - Input: Full file path `Game/ERB/口上/10_魔理沙/NTR口上_お持ち帰り.ERB`
  - Expected: Parser completes without throwing ParseException
  - Assert: No exception thrown, result has nodes
- **Pattern**: Integration test with real kojo file
- **Validation**: `dotnet test --filter FullyQualifiedName~StrDataParseTests.ParsesMarisaNtrFile` succeeds
- **Dependencies**: T5 (requires test file), T1-T4, T2a-T2c (requires parser implementation)

**Task 11: Run PRINTDATA regression test suite**
- **AC Coverage**: AC#8
- **Implementation**: Execute `dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~PrintDataParseTests`
- **Expected**: All existing PRINTDATA tests pass (0 failures)
- **Rationale**: Verify STRDATA implementation doesn't break existing functionality
- **Dependencies**: T1-T4, T2a-T2c (requires parser implementation complete)

**Task 12: Verify zero technical debt**
- **AC Coverage**: AC#9
- **Implementation**: Execute Grep pattern `TODO|FIXME|HACK` on `tools/ErbParser/ErbParser.cs`
- **Expected**: 0 matches
- **Rationale**: Ensure clean implementation without deferred work markers
- **Dependencies**: T1-T4, T2a-T2c (requires parser implementation complete)

**Task 13: Verify ErbParser build succeeds**
- **AC Coverage**: AC#10
- **Implementation**: Execute `dotnet build tools/ErbParser`
- **Expected**: Exit code 0, no compilation errors
- **Rationale**: Final build verification before feature completion
- **Dependencies**: T1-T14 (all tasks complete)

**Task 14: Implement unclosed STRDATA error test**
- **AC Coverage**: AC#11
- **Implementation**: Write `ThrowsOnUnclosedStrData()` test method:
  - Input: `STRDATA varname\n    DATA line1\n[EOF without ENDDATA]`
  - Expected: ParseException with message "STRDATA without matching ENDDATA"
  - Assert: `Assert.Throws<ParseException>()` with message validation
- **Pattern**: Error validation test (parallel to unclosed PRINTDATA)
- **Validation**: `dotnet test --filter FullyQualifiedName~StrDataParseTests.ThrowsOnUnclosedStrData` succeeds
- **Dependencies**: T5 (requires test file), T1-T4, T2a-T2c (requires parser implementation)

### AC:Task Coverage Matrix

| AC# | Covered By Tasks | Coverage Type |
|:---:|------------------|---------------|
| 1 | T1 | Direct implementation + validation |
| 2 | T2, T2a, T2b, T2c, T3, T4 | Multi-task (detection in all parsing contexts + termination + EOF) |
| 3 | T5 | Direct implementation |
| 4 | T6, T8 | Unit test + test data |
| 5 | T7, T8 | Unit test + test data |
| 6 | T9 | Integration test |
| 7 | T10 | Integration test |
| 8 | T11 | Regression test |
| 9 | T12 | Verification task |
| 10 | T13 | Build verification |
| 11 | T14 | Unit test (error handling) |

**AC:Task Alignment**: All 11 ACs covered. AC#2 requires multiple tasks (T2 main loop + T2a-T2c IF body parsing + T3-T4) to satisfy complete STRDATA handling. AC#4 and AC#5 each require test implementation (T6/T7) plus test data (T8).

## Implementation Contract

### Files to Modify

| File | Type | Lines | Pattern to Follow |
|------|------|:-----:|-------------------|
| `tools/ErbParser/ErbParser.cs` | Update | ~110 | F633 PRINTDATA state tracking pattern (lines 36-37, 88-115) + IF body parsing methods |

### Files to Create

| File | Type | Lines | Pattern to Follow |
|------|------|:-----:|-------------------|
| `tools/ErbParser.Tests/StrDataParseTests.cs` | Create | ~120 | Existing `PrintDataParseTests.cs` structure |
| `tools/ErbParser.Tests/TestData/basic_strdata.erb` | Create | ~5 | Minimal STRDATA block for unit test |
| `tools/ErbParser.Tests/TestData/nested_strdata.erb` | Create | ~4 | Nested STRDATA for error test |

### Code Patterns

**State Tracking Pattern (from F633)**:
- Location: After line 37 in ErbParser.cs
- Add boolean field: `private bool inStrData = false;`
- Pattern source: `inPrintData` field at line 36

**Command Detection Pattern (from F633)**:
- Location: After line 115 (before DATAFORM check)
- Use `StartsWith("STRDATA", StringComparison.OrdinalIgnoreCase)`
- Nested check: `if (inStrData) throw ParseException("Nested STRDATA is not allowed")`
- Pattern source: PRINTDATA detection at lines 88-102

**Terminator Handler Pattern (from F633)**:
- Location: Replace lines 104-115
- Extend ENDDATA handler with `else if (inStrData)` branch
- STRDATA branch: Set `inStrData = false`, skip content (no AST nodes)
- Pattern source: PRINTDATA ENDDATA handler at lines 104-115

**EOF Validation Pattern (from F633)**:
- Location: After line 200
- Add unclosed check: `if (inStrData) throw ParseException("STRDATA without matching ENDDATA")`
- Pattern source: PRINTDATA EOF validation at line 198

**Test Structure Pattern (from existing tests)**:
- Namespace: `ErbParser.Tests`
- Class naming: `StrDataParseTests` (parallel to `PrintDataParseTests`)
- Test method naming: `ParsesBasicStrDataBlock`, `ThrowsOnNestedStrData`, etc.
- Assertions: Use xUnit `Assert.Empty()`, `Assert.Throws<ParseException>()`
- File parsing: Use `ErbParser.Parse(filePath)` API

### Key Constraints

| Constraint | Requirement | Validation |
|------------|-------------|------------|
| **No AST nodes for STRDATA** | STRDATA content must be skipped (not added to AST) | AC#4: Assert.Empty(result.Nodes) on STRDATA-only file |
| **Shared ENDDATA terminator** | Both PRINTDATA and STRDATA use ENDDATA | ENDDATA handler checks both `inPrintData` and `inStrData` |
| **No nested STRDATA** | Nested STRDATA must throw ParseException | AC#5: ThrowsOnNestedStrData test |
| **Zero technical debt** | No TODO/FIXME/HACK comments allowed | AC#9: Grep verification |
| **PRINTDATA regression safety** | Existing PRINTDATA tests must pass | AC#8: Run PrintDataParseTests suite |

### Edge Cases to Handle

| Edge Case | Required Behavior | Implementation Location |
|-----------|-------------------|-------------------------|
| Nested STRDATA | Throw ParseException "Nested STRDATA is not allowed" | STRDATA detection block (T2) |
| Unclosed STRDATA at EOF | Throw ParseException "STRDATA without matching ENDDATA" | EOF validation (T4) |
| ENDDATA without PRINTDATA or STRDATA | Throw ParseException "ENDDATA without matching PRINTDATA" | ENDDATA handler else branch (T3) |
| DATA/DATAFORM inside STRDATA | Consume and skip (no AST nodes) | Natural behavior (STRDATA state prevents node creation) |
| STRDATA inside IF block | Parse normally (line-by-line processing) | No special handling needed |

### Success Criteria Summary

**Implementation Complete When**:
1. All 13 tasks completed (T1-T13)
2. All 10 ACs pass verification
3. `dotnet build tools/ErbParser` succeeds
4. `dotnet test tools/ErbParser.Tests` shows 0 failures
5. Grep verification shows 0 TODO/FIXME/HACK comments
6. Both Sakuya and Marisa NTR files parse successfully

**Quality Gates**:
- Code compiles without warnings
- All unit tests pass (AC#4, AC#5)
- All integration tests pass (AC#6, AC#7)
- PRINTDATA regression tests pass (AC#8)
- Zero technical debt markers (AC#9)

**Downstream Validation**:
- After completion, F639 AC#3 (Sakuya batch conversion) should unblock
- F636 AC#3 (Marisa batch conversion) should unblock
- Other character conversions (F637-F643) will handle STRDATA correctly
