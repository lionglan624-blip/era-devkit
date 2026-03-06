# Feature 700: PRINTDATAW/K/D DisplayMode Variants

## Status: [DONE]

## Type: engine

## Created: 2026-01-31

---

## Summary

Extend DisplayModeCapture to support PRINTDATAW (wait), PRINTDATAK (key-wait), and PRINTDATAD (display) variants in addition to PRINTDATAL (newline).

---

## Background

### Trigger (Reactive)

F678 implemented DisplayModeCapture for PRINTDATAL only (1,575 occurrences). PRINTDATAW/K/D variants currently have 0 occurrences in kojo files. This feature should be implemented when kojo files start using these variants.

### Problem (Current Issue)

DisplayModeCapture only captures "newline" displayMode. Other PRINTDATA variants (W/K/D) are not captured.

### Goal (What to Achieve)

1. Extend DisplayModeCapture to map PRINTDATAW → "wait", PRINTDATAK → "keyWait", PRINTDATAD → "display"
2. Update ErbRunner to parse these new displayMode values
3. Verify equivalence testing works for all PRINTDATA variants

### Philosophy (Mid-term Vision)

Complete DisplayMode capture parity between ERB and YAML formats for equivalence testing. All 9 PRINTDATA variants must have correct metadata capture to enable comprehensive ERB↔YAML comparison validation. This supports the long-term goal of ERB-to-YAML migration with confidence in behavioral equivalence.

---

## Links

- [feature-678.md](feature-678.md) - Predecessor: DisplayModeCapture foundation
- [feature-677.md](feature-677.md) - Related: KojoComparer DisplayMode Awareness
- [feature-684.md](feature-684.md) - Related: GUI Consumer Display Mode Interpretation
- [feature-686.md](feature-686.md) - Successor: KojoTestRunner/InteractiveRunner DisplayMode Integration

---

## Notes

- Created from F678 残課題
- Reactive: implement when first kojo uses W/K/D variants

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: DisplayModeCapture only captures "newline" displayMode, ignoring PRINTDATAW/K/D variants
2. Why: F678 implemented PRINTDATAL support only, as it was the only variant used in kojo files (1,575 occurrences)
3. Why: The PRINT_DATA_Instruction.DoInstruction() capture logic (Instraction.Child.cs:237-242) uses a binary check: `func.Function.IsNewLine() ? "newline" : "none"`
4. Why: F678 was scoped to practical needs - other variants had 0 occurrences, so extended mapping was deferred
5. Why: PRINTDATA variant flags (ISPRINTDFUNC, ISPRINTKFUNC, PRINT_WAITINPUT) exist and are checked for behavior but not captured for metadata

### Symptom vs Root Cause

| Symptom (Current State) | Root Cause (Technical Issue) |
|-------------------------|------------------------------|
| PRINTDATAW/K/D variants produce displayMode="none" in capture | DisplayModeCapture.AddLine() receives "none" for all non-PRINTDATAL variants |
| ErbRunner only maps "newline" → DisplayMode.Newline | Binary mapping in switch expression (line 135-140) ignores other displayMode strings |
| No equivalence testing for W/K/D variants | Capture logic lacks multi-variant flag interrogation |

### Conclusion

The root cause is **incomplete variant flag mapping** in the capture integration point. The PRINT_DATA_Instruction already has methods to identify all variant characteristics:
- `func.Function.IsNewLine()` → PRINTDATAL, PRINTDATAKL, PRINTDATADL
- `func.Function.IsWaitInput()` → PRINTDATAW, PRINTDATAKW, PRINTDATADW
- `func.Function.IsPrintKFunction()` → PRINTDATAK, PRINTDATAKL, PRINTDATAKW
- `func.Function.IsPrintDFunction()` → PRINTDATAD, PRINTDATADL, PRINTDATADW

The capture logic needs to combine these flags to produce the correct displayMode string matching Era.Core.Dialogue.DisplayMode enum values.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F678 | [DONE] | Predecessor | DisplayModeCapture foundation - PRINTDATAL only |
| F677 | [DONE] | Related | KojoComparer DisplayMode Awareness - DiffEngine comparison |
| F684 | [DONE] | Related | GUI Consumer Display Mode Interpretation - DisplayModeConsumer for YAML rendering |
| F686 | [DONE] | Successor | KojoTestRunner/InteractiveRunner DisplayMode Integration |

### Pattern Analysis

This feature follows the same **parallel metadata capture** pattern established by F678:
- F678 added capture for PRINTDATAL → "newline"
- F700 extends the same capture point to handle all 9 PRINTDATA variants
- No architectural changes needed - only mapping logic extension

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | All variant flags already exist (IsNewLine, IsWaitInput, IsPrintKFunction, IsPrintDFunction). Just need to combine them. |
| Scope is realistic | YES | Single capture point modification + ErbRunner mapping extension. Volume: ~50 lines of code changes. |
| No blocking constraints | YES | F678 [DONE]. DisplayModeCapture infrastructure exists. Era.Core.DisplayMode enum already has all 9 values. |

**Verdict**: FEASIBLE

**Implementation approach**:
1. Extend capture logic in PRINT_DATA_Instruction.DoInstruction() (Instraction.Child.cs:237-242) to derive full displayMode from flag combinations
2. Update ErbRunner displayMode parsing (lines 135-140) to map all 9 displayMode strings
3. Add unit tests for each variant mapping

**Variant-to-DisplayMode Mapping**:

| ERB Variant | Flags | DisplayMode String | Era.Core.DisplayMode |
|-------------|-------|-------------------|---------------------|
| PRINTDATA | none | "default" | Default |
| PRINTDATAL | IsNewLine | "newline" | Newline |
| PRINTDATAW | IsWaitInput | "wait" | Wait |
| PRINTDATAK | IsPrintKFunction | "keyWait" | KeyWait |
| PRINTDATAKL | IsPrintKFunction + IsNewLine | "keyNewline" | KeyWaitNewline |
| PRINTDATAKW | IsPrintKFunction + IsWaitInput | "keyWaitWait" | KeyWaitWait |
| PRINTDATAD | IsPrintDFunction | "display" | Display |
| PRINTDATADL | IsPrintDFunction + IsNewLine | "displayNewline" | DisplayNewline |
| PRINTDATADW | IsPrintDFunction + IsWaitInput | "displayWait" | DisplayWait |

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F678 | [DONE] | DisplayModeCapture foundation - provides capture service and JSON output structure |
| Related | F677 | [DONE] | DiffEngine displayModes comparison capability |
| Related | F684 | [DONE] | DisplayModeConsumer pattern for all 9 DisplayMode values |
| Successor | F686 | [DONE] | May benefit from complete variant support |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| engine/uEmuera.Headless | Build | Low | Existing project, no new dependencies |
| Era.Core.Dialogue.DisplayMode | Runtime | None | Enum already has all 9 values |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/KojoComparer/ErbRunner.cs | MEDIUM | Parses displayMode from JSON, needs mapping extension |
| tools/KojoComparer/BatchProcessor.cs | LOW | Passes displayModes to DiffEngine (no changes needed) |
| tools/KojoComparer.Tests/* | LOW | May need additional test cases for new variants |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs | Update | Extend DisplayModeCapture.AddLine() call with full variant mapping |
| tools/KojoComparer/ErbRunner.cs | Update | Extend displayMode string → enum mapping switch expression |
| tools/KojoComparer.Tests/ErbRunnerTests.cs | Update | Add test cases for all 9 displayMode mappings |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| HEADLESS_MODE preprocessor directive | engine/uEmuera.Headless.csproj | LOW - Existing constraint from F678 |
| DisplayMode enum values must match YAML schema | dialogue-schema.json | LOW - Enum already has correct values |
| Flag combination order matters for D+K variants | FunctionIdentifier flag parsing | MEDIUM - Must check D before K due to PRINTDATAKD possibility (not currently used) |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| 0 current usage means testing is theoretical only | High | Low | Defer implementation until first kojo uses W/K/D (reactive feature) |
| Flag combination edge cases (e.g., PRINTDATAKDLW) | Low | Low | Standard variants are well-defined; exotic combinations would fail validation |
| DisplayMode string mismatch with YAML schema | Low | Medium | Use exact enum names from Era.Core.Dialogue.DisplayMode |
| Breaking change to JSON output format | Low | Low | Additive change only - existing "newline" mapping unchanged |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Extend the existing DisplayModeCapture integration in PRINT_DATA_Instruction.DoInstruction() to derive the complete displayMode string from flag combinations. This is a straightforward extension of F678's pattern, leveraging the four existing flag methods (IsNewLine, IsWaitInput, IsPrintKFunction, IsPrintDFunction) to produce all 9 displayMode strings.

**Key insight from Root Cause Analysis**: The capture logic currently uses a binary check `func.Function.IsNewLine() ? "newline" : "none"` (line 240). The fix is to replace this with a multi-way decision tree that combines all four flag methods.

**Implementation strategy**:
1. **Capture Logic (Instraction.Child.cs)**: Replace the binary ternary operator with a nested if-else structure that checks flags in priority order (D > K > W > L) to derive the correct displayMode string
2. **ErbRunner Mapping (ErbRunner.cs)**: Extend the switch expression to map all 9 displayMode strings to their corresponding DisplayMode enum values
3. **Unit Tests (DisplayModeEquivalenceTests.cs)**: Add parameterized test method to verify all 9 displayMode mappings

**Design rationale**:
- **Flag check order matters**: Check IsPrintDFunction first, then IsPrintKFunction, to handle potential overlap (though current ERB variants are mutually exclusive)
- **String values match enum names exactly**: "wait" → DisplayMode.Wait, "keyWait" → DisplayMode.KeyWait, etc. (camelCase convention from Era.Core.Dialogue.DisplayMode enum)
- **Backwards compatibility**: Existing "newline" mapping preserved exactly; additive change only
- **Zero technical debt**: Complete all 9 mappings upfront, no TODOs or partial implementations

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Compile engine project after capture logic changes - ensures HEADLESS_MODE preprocessor block is syntactically correct |
| 2 | Compile tools/KojoComparer after ErbRunner.cs switch expression update - validates new enum references |
| 3 | In capture logic, add else-if branch for base case: no flags set → "default" (PRINTDATA) |
| 4 | Preserve existing IsNewLine check in final else-if (already captures "newline" from F678) |
| 5 | Add else-if for IsWaitInput && !IsPrintKFunction && !IsPrintDFunction → "wait" (PRINTDATAW) |
| 6 | Add else-if for IsPrintKFunction && !IsNewLine && !IsWaitInput → "keyWait" (PRINTDATAK) |
| 7 | Add else-if for IsPrintKFunction && IsNewLine → "keyWaitNewline" (PRINTDATAKL) |
| 8 | Add else-if for IsPrintKFunction && IsWaitInput → "keyWaitWait" (PRINTDATAKW) |
| 9 | Add else-if for IsPrintDFunction && !IsNewLine && !IsWaitInput → "display" (PRINTDATAD) |
| 10 | Add else-if for IsPrintDFunction && IsNewLine → "displayNewline" (PRINTDATADL) |
| 11 | Add else-if for IsPrintDFunction && IsWaitInput → "displayWait" (PRINTDATADW) |
| 12 | In ErbRunner switch expression, add case `"wait" => DisplayMode.Wait` |
| 13 | In ErbRunner switch expression, add cases for `"keyWait" => DisplayMode.KeyWait`, `"keyWaitNewline" => DisplayMode.KeyWaitNewline`, `"keyWaitWait" => DisplayMode.KeyWaitWait` |
| 14 | In ErbRunner switch expression, add cases for `"display" => DisplayMode.Display`, `"displayNewline" => DisplayMode.DisplayNewline`, `"displayWait" => DisplayMode.DisplayWait` |
| 15 | File already exists (checked in git status) - verify with Glob during implementation |
| 16 | Add [Theory] test method with [InlineData] for all 9 displayMode string → enum mappings. Test ErbRunner.ParseDisplayMode() directly (unit test, no headless execution required) |
| 17 | Grep for `TODO\|FIXME\|HACK` regex in changed files - expect 0 matches |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Flag check order in capture logic | A) Random order B) Alphabetical C) Priority order (D > K > W > L) | C | Ensures deterministic behavior if future ERB variants combine D+K flags. Check most specific flags first (D, K) before generic modifiers (W, L) |
| displayMode string naming | A) Lowercase ("keywait") B) UPPERCASE ("KEYWAIT") C) camelCase ("keyWait") | C | displayMode strings use camelCase ("keyWait") which map to PascalCase enum values (DisplayMode.KeyWait). Consistent with existing "newline" convention from F678. ErbRunner uses ToLowerInvariant() for case-insensitive parsing |
| Switch expression vs if-else in ErbRunner | A) Keep switch expression B) Convert to if-else | A | Switch expressions are more readable for 9-way mapping. Pattern established in F678. No performance difference for string equality |
| Unit test approach | A) Integration tests with headless mode B) Direct unit tests of parsing logic C) Parameterized theory tests | C | Integration tests already exist (skipped) from F678. This feature needs quick validation of 9 mappings. [Theory] with [InlineData] covers all cases efficiently without headless mode overhead |
| Capture logic structure | A) Single nested ternary B) If-else chain C) Switch expression on flag combination | B | If-else chain is most readable for multi-flag interrogation. Allows early exit for specific combinations. Ternary would be unreadable at 9 branches. Switch on flags would require bitwise combination which is less clear |

### Data Structures

No new data structures required. This feature extends existing infrastructure:

**Existing structures (unchanged)**:
- `Era.Core.Dialogue.DisplayMode` enum (already has all 9 values)
- `DisplayModeCapture.AddLine(string text, string displayMode)` signature (already accepts string)
- `ErbRunner.ExecuteAsync()` return type (already returns `Task<(string, List<DisplayMode>)>`)

**Flag methods (existing, used for mapping)**:
```csharp
// FunctionIdentifier.cs
bool IsPrintDFunction()    // ISPRINTDFUNC flag
bool IsPrintKFunction()    // ISPRINTKFUNC flag
bool IsNewLine()           // PRINT_NEWLINE flag
bool IsWaitInput()         // PRINT_WAITINPUT flag
```

**Capture logic pattern (to be implemented)**:
```csharp
#if HEADLESS_MODE
if (Headless.DisplayModeCapture.IsCapturing)
{
    string displayMode;

    // Check D variants first (most specific)
    if (func.Function.IsPrintDFunction() && func.Function.IsNewLine())
        displayMode = "displayNewline";
    else if (func.Function.IsPrintDFunction() && func.Function.IsWaitInput())
        displayMode = "displayWait";
    else if (func.Function.IsPrintDFunction())
        displayMode = "display";
    // Check K variants
    else if (func.Function.IsPrintKFunction() && func.Function.IsNewLine())
        displayMode = "keyWaitNewline";
    else if (func.Function.IsPrintKFunction() && func.Function.IsWaitInput())
        displayMode = "keyWaitWait";
    else if (func.Function.IsPrintKFunction())
        displayMode = "keyWait";
    // Check simple modifiers
    else if (func.Function.IsNewLine())
        displayMode = "newline";
    else if (func.Function.IsWaitInput())
        displayMode = "wait";
    else
        displayMode = "default";

    Headless.DisplayModeCapture.AddLine(str, displayMode);
}
#endif
```

**ErbRunner mapping pattern (to be implemented)**:
```csharp
var displayMode = displayModeStr.ToLowerInvariant() switch
{
    "newline" => DisplayMode.Newline,
    "wait" => DisplayMode.Wait,
    "keywait" => DisplayMode.KeyWait,
    "keywaitnewline" => DisplayMode.KeyWaitNewline,
    "keywaitwait" => DisplayMode.KeyWaitWait,
    "display" => DisplayMode.Display,
    "displaynewline" => DisplayMode.DisplayNewline,
    "displaywait" => DisplayMode.DisplayWait,
    "default" => DisplayMode.Default,
    _ => DisplayMode.Default
};
```

**Note on case sensitivity**: The switch expression uses `ToLowerInvariant()` before matching, so capture logic can use camelCase strings ("keyWait") which become lowercase ("keywait") for matching. This preserves readability in capture logic while ensuring robust parsing.

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "All 9 PRINTDATA variants must be correctly mapped" | Capture logic maps each variant to correct displayMode string | AC#3-11 |
| "DisplayMode strings must match Era.Core.DisplayMode enum" | String values consistent with enum names | AC#3-11 |
| "ErbRunner must parse all displayMode strings" | Switch expression extended for all 9 cases | AC#12-14 |
| "Equivalence testing works for all variants" | Unit tests verify mapping correctness | AC#15-16 |
| "Build must succeed" | Both engine and tools build without errors | AC#1, AC#2 |
| "Zero technical debt" | No TODO/FIXME/HACK markers in changed files | AC#17 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Engine build succeeds | build | dotnet build engine/uEmuera.Headless.csproj | succeeds | - | [x] |
| 2 | KojoComparer build succeeds | build | dotnet build tools/KojoComparer/ | succeeds | - | [x] |
| 3 | Capture maps PRINTDATA → "default" | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs) | contains | = "default" | [x] |
| 4 | Capture maps PRINTDATAL → "newline" | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs) | contains | = "newline" | [x] |
| 5 | Capture maps PRINTDATAW → "wait" | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs) | contains | = "wait" | [x] |
| 6 | Capture maps PRINTDATAK → "keyWait" | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs) | contains | = "keyWait" | [x] |
| 7 | Capture maps PRINTDATAKL → "keyWaitNewline" | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs) | contains | = "keyWaitNewline" | [x] |
| 8 | Capture maps PRINTDATAKW → "keyWaitWait" | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs) | contains | = "keyWaitWait" | [x] |
| 9 | Capture maps PRINTDATAD → "display" | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs) | contains | = "display" | [x] |
| 10 | Capture maps PRINTDATADL → "displayNewline" | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs) | contains | = "displayNewline" | [x] |
| 11 | Capture maps PRINTDATADW → "displayWait" | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs) | contains | = "displayWait" | [x] |
| 12 | ErbRunner parses "wait" displayMode | code | Grep(tools/KojoComparer/ErbRunner.cs) | contains | Era.Core.Dialogue.DisplayMode.Wait | [x] |
| 13 | ErbRunner parses all K variants | code | Grep(tools/KojoComparer/ErbRunner.cs) | contains | Era.Core.Dialogue.DisplayMode.KeyWait | [x] |
| 14 | ErbRunner parses all D variants | code | Grep(tools/KojoComparer/ErbRunner.cs) | contains | Era.Core.Dialogue.DisplayMode.Display | [x] |
| 15 | DisplayMode mapping unit tests exist | file | Glob(tools/KojoComparer.Tests/DisplayModeEquivalenceTests.cs) | exists | - | [x] |
| 16 | Unit tests pass | test | dotnet test tools/KojoComparer.Tests/ --filter DisplayModeEquivalenceTests | succeeds | - | [x] |
| 17a | No TODO markers in changed files | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs,tools/KojoComparer/ErbRunner.cs) | not_contains | TODO | [x] |
| 17b | No FIXME markers in changed files | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs,tools/KojoComparer/ErbRunner.cs) | not_contains | FIXME | [x] |
| 17c | No HACK markers in changed files | code | Grep(engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs,tools/KojoComparer/ErbRunner.cs) | not_contains | HACK | [x] |

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 3,4,5,6,7,8,9,10,11 | Extend capture logic in Instraction.Child.cs to map all 9 PRINTDATA variants to displayMode strings | [x] |
| 2 | 16 | Extract current inline displayMode parsing logic from ExecuteAsync into public static method ParseDisplayMode(string) for testability | [x] |
| 3 | 12,13,14 | Extend ParseDisplayMode() method to parse all displayMode strings to DisplayMode enum | [x] |
| 4 | 15,16 | Add parameterized test method to existing DisplayModeEquivalenceTests.cs for all 9 displayMode mappings | [x] |
| 5 | 1,2,17a,17b,17c | Verify builds succeed and zero technical debt | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T4 | Technical Design section, capture logic pattern, ErbRunner mapping pattern | Modified Instraction.Child.cs, ErbRunner.cs, DisplayModeEquivalenceTests.cs |
| 2 | ac-tester | haiku | T5 | AC table with build/Grep/test commands | Verification results for all 17 ACs |

**Constraints** (from Technical Design):

1. **Flag check order**: Check IsPrintDFunction first, then IsPrintKFunction, then simple modifiers (W, L). This ensures deterministic behavior if future ERB variants combine flags.
2. **displayMode string naming**: Use camelCase exactly matching Era.Core.Dialogue.DisplayMode enum names ("wait", "keyWait", "displayNewline", etc.).
3. **HEADLESS_MODE directive**: All capture logic changes must be within `#if HEADLESS_MODE` preprocessor block.
4. **Backwards compatibility**: Preserve existing "newline" mapping from F678 exactly; this is additive change only.
5. **Case-insensitive parsing**: ErbRunner switch expression uses `ToLowerInvariant()` before matching, so capture can use camelCase strings which become lowercase for robust parsing.

**Pre-conditions**:

- F678 [DONE] - DisplayModeCapture infrastructure exists with AddLine(string text, string displayMode) signature
- engine/uEmuera.Headless.csproj compiles successfully
- tools/KojoComparer/ compiles successfully
- Era.Core.Dialogue.DisplayMode enum has all 9 values: Default, Newline, Wait, KeyWait, KeyWaitNewline, KeyWaitWait, Display, DisplayNewline, DisplayWait

**Success Criteria**:

- All 17 ACs pass verification
- Capture logic produces correct displayMode string for all 9 PRINTDATA variants based on flag combinations
- ErbRunner.ParseDisplayMode() correctly maps all 9 displayMode strings to DisplayMode enum values
- Unit tests verify all mappings without requiring headless execution
- Both engine and tools projects build without errors
- Zero TODO/FIXME/HACK markers in changed files

**Execution Steps**:

### Phase 1: Implementation (Task 1-4)

**Note**: Tasks 2 and 3 are sequenced to avoid code duplication. Task 2 extracts existing logic, Task 3 extends the extracted method.

**Task 1**: Extend capture logic in Instraction.Child.cs (line ~240)

Replace the existing binary ternary operator `func.Function.IsNewLine() ? "newline" : "none"` with nested if-else chain:

```csharp
#if HEADLESS_MODE
if (Headless.DisplayModeCapture.IsCapturing)
{
    string displayMode;

    // Check D variants first (most specific)
    if (func.Function.IsPrintDFunction() && func.Function.IsNewLine())
        displayMode = "displayNewline";
    else if (func.Function.IsPrintDFunction() && func.Function.IsWaitInput())
        displayMode = "displayWait";
    else if (func.Function.IsPrintDFunction())
        displayMode = "display";
    // Check K variants
    else if (func.Function.IsPrintKFunction() && func.Function.IsNewLine())
        displayMode = "keyWaitNewline";
    else if (func.Function.IsPrintKFunction() && func.Function.IsWaitInput())
        displayMode = "keyWaitWait";
    else if (func.Function.IsPrintKFunction())
        displayMode = "keyWait";
    // Check simple modifiers
    else if (func.Function.IsNewLine())
        displayMode = "newline";
    else if (func.Function.IsWaitInput())
        displayMode = "wait";
    else
        displayMode = "default";

    Headless.DisplayModeCapture.AddLine(str, displayMode);
}
#endif
```

**Task 2**: Extract current inline displayMode parsing logic from ExecuteAsync into public static method ParseDisplayMode(string) for testability

Extract existing switch expression (lines 135-139) into public static method in ErbRunner class:

```csharp
public static Era.Core.Dialogue.DisplayMode ParseDisplayMode(string displayModeStr)
{
    return displayModeStr.ToLowerInvariant() switch
    {
        "newline" => Era.Core.Dialogue.DisplayMode.Newline,
        _ => Era.Core.Dialogue.DisplayMode.Default
    };
}
```

Then replace inline logic in ExecuteAsync (line ~137) with: `ParseDisplayMode(displayModeStr ?? "none")`

**Task 3**: Extend ParseDisplayMode() method to parse all displayMode strings to DisplayMode enum

Extend the extracted method with comprehensive mapping for all 9 PRINTDATA variants:

```csharp
public static Era.Core.Dialogue.DisplayMode ParseDisplayMode(string displayModeStr)
{
    return displayModeStr.ToLowerInvariant() switch
    {
        "newline" => Era.Core.Dialogue.DisplayMode.Newline,
        "wait" => Era.Core.Dialogue.DisplayMode.Wait,
        "keywait" => Era.Core.Dialogue.DisplayMode.KeyWait,
        "keywaitnewline" => Era.Core.Dialogue.DisplayMode.KeyWaitNewline,
        "keywaitwait" => Era.Core.Dialogue.DisplayMode.KeyWaitWait,
        "display" => Era.Core.Dialogue.DisplayMode.Display,
        "displaynewline" => Era.Core.Dialogue.DisplayMode.DisplayNewline,
        "displaywait" => Era.Core.Dialogue.DisplayMode.DisplayWait,
        "default" => Era.Core.Dialogue.DisplayMode.Default,
        _ => Era.Core.Dialogue.DisplayMode.Default
    };
}
```

**Task 4**: Add parameterized test method to existing DisplayModeEquivalenceTests.cs

File already exists from F678. Add parameterized test method:

```csharp
[Theory]
[InlineData("default", Era.Core.Dialogue.DisplayMode.Default)]
[InlineData("newline", Era.Core.Dialogue.DisplayMode.Newline)]
[InlineData("wait", Era.Core.Dialogue.DisplayMode.Wait)]
[InlineData("keyWait", Era.Core.Dialogue.DisplayMode.KeyWait)]
[InlineData("keyWaitNewline", Era.Core.Dialogue.DisplayMode.KeyWaitNewline)]
[InlineData("keyWaitWait", Era.Core.Dialogue.DisplayMode.KeyWaitWait)]
[InlineData("display", Era.Core.Dialogue.DisplayMode.Display)]
[InlineData("displayNewline", Era.Core.Dialogue.DisplayMode.DisplayNewline)]
[InlineData("displayWait", Era.Core.Dialogue.DisplayMode.DisplayWait)]
public void ParseDisplayMode_AllVariants_CorrectMapping(string displayModeStr, Era.Core.Dialogue.DisplayMode expected)
{
    // Test ErbRunner.ParseDisplayMode() directly (Task 3 extracts this method)
    var result = ErbRunner.ParseDisplayMode(displayModeStr);
    Assert.Equal(expected, result);
}
```

### Phase 2: Verification (Task 5)

Run ac-tester to verify:
1. `dotnet build engine/uEmuera.Headless.csproj` succeeds (AC#1)
2. `dotnet build tools/KojoComparer/` succeeds (AC#2)
3. Grep verifications for all displayMode strings in Instraction.Child.cs (AC#3-11)
4. Grep verifications for DisplayMode enum values in ErbRunner.cs (AC#12-14)
5. Glob verification for DisplayModeEquivalenceTests.cs existence (AC#15)
6. `dotnet test tools/KojoComparer.Tests/ --filter DisplayModeEquivalenceTests` succeeds (AC#16)
7. Grep verification for zero TODO/FIXME/HACK in changed files (AC#17a,17b,17c)

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert <commit-hash>`
2. Notify user of rollback with error details
3. Create follow-up feature for fix with additional investigation into flag combination logic or enum mapping errors

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-31 08:01 | START | implementer | Task 1-3 | - |
| 2026-01-31 08:01 | END | implementer | Task 1-3 | SUCCESS - All builds pass |
| 2026-01-31 | Phase 3 | TDD RED confirmed - ParseDisplayMode test created, fails due to missing method |
| 2026-01-31 | Phase 4 | Tasks 1-3 completed - capture logic extended, ParseDisplayMode extracted and extended |
| 2026-01-31 | Phase 4 | TDD GREEN confirmed - 9/9 ParseDisplayMode tests pass |
| 2026-01-31 | Phase 6 | ac-tester | All 17 ACs verified PASS |

---

### AC Details

**AC#1: Engine build succeeds**
- Test: `dotnet build engine/uEmuera.Headless.csproj`
- Expected: Exit code 0
- Verifies capture logic changes compile correctly with HEADLESS_MODE preprocessor directive

**AC#2: KojoComparer build succeeds**
- Test: `dotnet build tools/KojoComparer/`
- Expected: Exit code 0
- Verifies ErbRunner displayMode mapping changes compile correctly

**AC#3: Capture maps PRINTDATA → "default"**
- Test: Grep pattern=`"default"` path=`engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs`
- Expected: Match found in displayMode derivation logic
- Base case: PRINTDATA (no flags) → "default" displayMode string

**AC#4: Capture maps PRINTDATAL → "newline"**
- Test: Grep pattern=`"newline"` path=`engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs`
- Expected: Match found (existing from F678)
- Regression check: existing PRINTDATAL mapping preserved

**AC#5: Capture maps PRINTDATAW → "wait"**
- Test: Grep pattern=`"wait"` (exact context: as displayMode string, not method call)
- Expected: Match found in flag combination logic
- Variant: IsWaitInput() && !IsPrintKFunction() && !IsPrintDFunction() → "wait"

**AC#6: Capture maps PRINTDATAK → "keyWait"**
- Test: Grep pattern=`"keyWait"` path=`engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs`
- Expected: Match found
- Variant: IsPrintKFunction() && !IsNewLine() && !IsWaitInput() → "keyWait"

**AC#7: Capture maps PRINTDATAKL → "keyWaitNewline"**
- Test: Grep pattern=`"keyWaitNewline"` path=`engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs`
- Expected: Match found
- Variant: IsPrintKFunction() && IsNewLine() → "keyWaitNewline"

**AC#8: Capture maps PRINTDATAKW → "keyWaitWait"**
- Test: Grep pattern=`"keyWaitWait"` path=`engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs`
- Expected: Match found
- Variant: IsPrintKFunction() && IsWaitInput() → "keyWaitWait"

**AC#9: Capture maps PRINTDATAD → "display"**
- Test: Grep pattern=`"display"` (exact context: as displayMode string)
- Expected: Match found
- Variant: IsPrintDFunction() && !IsNewLine() && !IsWaitInput() → "display"

**AC#10: Capture maps PRINTDATADL → "displayNewline"**
- Test: Grep pattern=`"displayNewline"` path=`engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs`
- Expected: Match found
- Variant: IsPrintDFunction() && IsNewLine() → "displayNewline"

**AC#11: Capture maps PRINTDATADW → "displayWait"**
- Test: Grep pattern=`"displayWait"` path=`engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs`
- Expected: Match found
- Variant: IsPrintDFunction() && IsWaitInput() → "displayWait"

**AC#12: ErbRunner parses "wait" displayMode**
- Test: Grep pattern=`DisplayMode.Wait` path=`tools/KojoComparer/ErbRunner.cs`
- Expected: Match found in switch expression
- Verifies mapping: "wait" → DisplayMode.Wait

**AC#13: ErbRunner parses all K variants**
- Test: Grep pattern=`DisplayMode.KeyWait` path=`tools/KojoComparer/ErbRunner.cs`
- Expected: Match found (at least one)
- Verifies mappings: "keyWait"/"keyWaitNewline"/"keyWaitWait" → corresponding DisplayMode values

**AC#14: ErbRunner parses all D variants**
- Test: Grep pattern=`DisplayMode.Display` path=`tools/KojoComparer/ErbRunner.cs`
- Expected: Match found (at least one)
- Verifies mappings: "display"/"displayNewline"/"displayWait" → corresponding DisplayMode values

**AC#15: DisplayMode mapping unit tests exist**
- Test: Glob pattern=`tools/KojoComparer.Tests/DisplayModeEquivalenceTests.cs`
- Expected: File exists
- Note: File already exists from F678. Task 3 adds parameterized test method for all 9 displayMode mappings

**AC#16: Unit tests pass**
- Test: `dotnet test tools/KojoComparer.Tests/ --filter DisplayModeEquivalenceTests`
- Expected: Exit code 0, all tests pass
- Verifies all 9 displayMode mappings work correctly in ErbRunner

**AC#17a: No TODO markers in changed files**
- Test: Grep pattern=`TODO` paths=[Instraction.Child.cs, ErbRunner.cs]
- Expected: 0 matches
- Ensures no TODO markers in implementation

**AC#17b: No FIXME markers in changed files**
- Test: Grep pattern=`FIXME` paths=[Instraction.Child.cs, ErbRunner.cs]
- Expected: 0 matches
- Ensures no FIXME markers in implementation

**AC#17c: No HACK markers in changed files**
- Test: Grep pattern=`HACK` paths=[Instraction.Child.cs, ErbRunner.cs]
- Expected: 0 matches
- Ensures no HACK markers in implementation
