# Feature 685: HeadlessUI Console.SetOut Cleanup Fix

## Status: [DONE]

## Type: engine

## Created: 2026-01-30

---

## Summary

HeadlessUI Console.SetOut Cleanup Fix. Fix the pre-existing Console.SetOut cleanup bug in HeadlessUITests where `Console.SetOut(Console.Out)` is a no-op after `SetOut(stringWriter)`, and ensure all test methods use proper try/finally cleanup patterns.

---

## Links

- [feature-682.md](feature-682.md) - Consumer-Side Display Mode Interpretation (Predecessor - identified the cleanup bug)

---

## Notes

- Created as F682 successor (残課題: Fix existing TestOutputDialogueValid Console.SetOut cleanup pattern)
- Pre-existing issue: line 51 uses Console.SetOut(Console.Out) which is no-op after SetOut(stringWriter)
- Scope: Fix cleanup pattern in all HeadlessUITests test methods (existing + new from F682)

### F682 Handoff (2026-01-30)
- F682 adds 9 new test methods (TestDisplayMode{Default,Newline,Wait,KeyWait,KeyWaitNewline,KeyWaitWait,Display,DisplayNewline,DisplayWait}) that also lack try/finally cleanup
- All test methods (existing TestOutputDialogueValid + 9 new) should be wrapped in try/finally to ensure Console.SetOut(originalOut) runs even on assertion failure
- New tests use improved pattern (captures originalOut before SetOut) but still lack try/finally guard

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Console.SetOut/SetIn cleanup in HeadlessUITests fails to restore original streams, and assertion failures can leak redirected streams to subsequent tests
2. Why: Old test methods (F479-era) call `Console.SetOut(Console.Out)` to "restore", but after `SetOut(stringWriter)`, `Console.Out` already returns the stringWriter -- so `SetOut(Console.Out)` is a no-op that re-assigns the same stringWriter
3. Why: The .NET `Console.Out` property dynamically returns whatever writer is currently set (it does not cache the original). The code author likely assumed `Console.Out` retains a reference to the original stdout writer
4. Why: The original F479 tests were written without capturing `Console.Out` before redirection, and no try/finally guard was added because the tests were simple and the no-op bug did not cause visible failures (xUnit creates separate process-level isolation per test class)
5. Why: The Console.SetOut/SetIn redirect-and-restore pattern requires explicit capture of the original writer before redirection and a try/finally guard to ensure restoration on exception -- a pattern that was not enforced or documented when the original tests were written

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| `Console.SetOut(Console.Out)` is a no-op after `SetOut(stringWriter)` | `Console.Out` dynamically returns the current writer, not the original. Must capture original before redirection. |
| Assertion failures can leak redirected Console streams to subsequent tests | No try/finally guard wraps the Console redirect + assertion + cleanup sequence |
| `Console.SetIn(Console.In)` has the same no-op bug in TestReadInputValid and TestReadInputEmpty | Same root cause: `Console.In` dynamically returns current reader after `SetIn(stringReader)` |

### Conclusion

The root cause is a **missing capture-before-redirect pattern** combined with **missing exception-safety guards**. Two distinct bugs exist:

1. **No-op restore** (4 old test methods): `Console.SetOut(Console.Out)` and `Console.SetIn(Console.In)` are no-ops because `Console.Out`/`Console.In` return the redirected writer/reader, not the original. Fix: capture `var originalOut = Console.Out` before `SetOut(stringWriter)`.

2. **Missing try/finally** (all 13 test methods): If an assertion fails, the cleanup code after the assertion is skipped, leaking the redirected stream. Fix: wrap the redirect-act-assert-cleanup sequence in try/finally.

The 9 newer F682 test methods already fix bug #1 (they capture `originalOut` before redirect) but still have bug #2 (no try/finally). The 4 older F479-era methods have both bugs.

**Affected methods** (13 total):
- **Bug #1 + #2** (4 methods): TestOutputDialogueValid, TestOutputStateValid, TestReadInputValid, TestReadInputEmpty
- **Bug #2 only** (9 methods): TestDisplayModeDefault, TestDisplayModeNewline, TestDisplayModeWait, TestDisplayModeKeyWait, TestDisplayModeKeyWaitNewline, TestDisplayModeKeyWaitWait, TestDisplayModeDisplay, TestDisplayModeDisplayNewline, TestDisplayModeDisplayWait

**Additional finding**: TestReadInputValid (line 116) and TestReadInputEmpty (line 172) also use `Console.SetIn(Console.In)` which has the identical no-op bug for stdin redirection. The fix must also address Console.SetIn cleanup.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F479 | [DONE] | Origin | Original HeadlessUI implementation that introduced the 4 test methods with the no-op cleanup bug |
| F682 | [DONE] | Predecessor | Consumer-Side Display Mode Interpretation. Added 9 new test methods with improved capture pattern but no try/finally. Identified and documented the cleanup bug as 残課題→F685 |
| F680 | [DONE] | Related | xUnit v3→v2 rollback. Test isolation behavior may differ between xUnit versions, affecting visibility of the leaked-stream bug |

### Pattern Analysis

This is a **test hygiene debt** pattern. The original F479 tests used an incorrect cleanup idiom (`Console.SetOut(Console.Out)`) that happened to not cause visible failures because xUnit v2 runs test methods sequentially within a class and the leaked StringWriter still "works" as a Console.Out target (it just writes to the wrong destination). The bug became visible during F682 review when the codebase was examined more carefully, but practical test failures were never observed because:
- xUnit v2 test methods within a class run sequentially (no parallelism interference)
- The leaked StringWriter accepts writes without error (no crash)
- Each test creates its own StringWriter, so leaked writes from a previous test go to an abandoned StringWriter

The lack of observable failure explains why this bug persisted from F479 through F682 without detection.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Standard .NET pattern: capture original stream before redirect, wrap in try/finally. Well-known idiom. |
| Scope is realistic | YES | 13 test methods in a single file (529 lines). Mechanical transformation: add `var originalOut = Console.Out` + try/finally wrapper. No logic changes. |
| No blocking constraints | YES | F682 is [DONE]. No prerequisite features needed. Pure test-only change with no production code impact. |

**Verdict**: FEASIBLE

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F682 | [DONE] | Added the 9 new test methods that need try/finally cleanup. Must be done first so all 13 methods exist. |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| System.Console | Runtime | None | Standard .NET Console redirect API. No version-specific behavior. |
| xUnit v2 | Test Framework | None | Test isolation model is compatible with try/finally cleanup. |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| Era.Core.Tests/HeadlessUITests.cs | HIGH | The only file being modified. All 13 test methods are consumers of Console.SetOut/SetIn redirect pattern. |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core.Tests/HeadlessUITests.cs | Update | Fix all 13 test methods: (1) Add `var originalOut = Console.Out` capture in 4 old methods, (2) Add try/finally wrapper in all 13 methods, (3) Fix `Console.SetIn(Console.In)` no-op in 2 methods (TestReadInputValid, TestReadInputEmpty) |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Test methods must not change assertion logic | Immutable Tests principle (TDD) | LOW - F685 only changes cleanup patterns, not assertion content. But this is a cleanup fix, not a test logic change, so the constraint should be satisfied. |
| xUnit v2 test runner (not v3) | F680 rollback | LOW - try/finally pattern works identically in both versions |
| Console.SetOut/SetIn affect process-global state | .NET Console API design | MEDIUM - Ensures cleanup is critical for test isolation. Reinforces need for try/finally. |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Mechanical edit error in one of 13 methods | Low | Low | Each method follows identical pattern. Review diff for consistency. |
| try/finally changes test behavior in edge cases | Very Low | Low | try/finally only affects exception paths. Normal test execution is identical. No assertion logic changes. |
| Console.SetIn cleanup missed | Low | Medium | Investigation identified 2 methods with SetIn bug (lines 116, 172). Ensure fix covers both SetOut and SetIn. |

---

## Background

### Philosophy (Mid-term Vision)

Test methods that redirect process-global Console streams must absolutely guarantee stream restoration, regardless of test outcome. Every test method using Console.SetOut or Console.SetIn must capture the original stream before redirection and restore it in a finally block. No test method may use the no-op pattern `Console.SetOut(Console.Out)` after redirection.

### Problem (Current Issue)

HeadlessUITests has two cleanup bugs: (1) 4 old methods use the no-op `Console.SetOut(Console.Out)` / `Console.SetIn(Console.In)` pattern that fails to restore original streams, and (2) all 13 methods with Console redirection lack try/finally guards, meaning assertion failures leak redirected streams to subsequent tests.

### Goal (What to Achieve)

1. Fix the no-op restore pattern in all 4 old methods (TestOutputDialogueValid, TestOutputStateValid, TestReadInputValid, TestReadInputEmpty) by capturing `var originalOut = Console.Out` (and `var originalIn = Console.In` for SetIn methods) before redirection
2. Wrap all 13 Console-redirecting test methods in try/finally to guarantee stream restoration on assertion failure
3. Fix Console.SetIn no-op cleanup in the 2 methods that redirect stdin (TestReadInputValid, TestReadInputEmpty)
4. All existing tests must continue to pass after the cleanup fix (no behavioral change)

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "must absolutely guarantee stream restoration" | Every Console-redirecting method must have try/finally | AC#2 |
| "Every test method using Console.SetOut or Console.SetIn must capture the original stream before redirection" | All 13 methods must have `var originalOut = Console.Out` before `Console.SetOut(...)` | AC#1 |
| "restore it in a finally block" | Cleanup code (`Console.SetOut(originalOut)`) must be inside `finally` block | AC#2 |
| "No test method may use the no-op pattern" | Zero occurrences of `Console.SetOut(Console.Out)` and `Console.SetIn(Console.In)` | AC#3 |
| "all 13 methods with Console redirection lack try/finally" | All 13 methods must gain try/finally | AC#2 |
| "Fix Console.SetIn no-op cleanup in the 2 methods" | TestReadInputValid and TestReadInputEmpty must capture `var originalIn = Console.In` before `Console.SetIn(...)` | AC#4 |
| "All existing tests must continue to pass" | dotnet test succeeds with zero failures | AC#5 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | All 4 old SetOut methods capture originalOut before redirect | `code` | Grep(Era.Core.Tests/HeadlessUITests.cs) | `count_equals` | 13 occurrences of `var originalOut = Console.Out` | [x] |
| 2 | All 13 Console-redirecting methods use try/finally | `code` | Grep(Era.Core.Tests/HeadlessUITests.cs) | `count_equals` | 13 occurrences of `finally` | [x] |
| 3a | No Console.SetOut no-op patterns remain | `code` | Grep(Era.Core.Tests/HeadlessUITests.cs) | `not_contains` | `Console.SetOut(Console.Out)` | [x] |
| 3b | No Console.SetIn no-op patterns remain | `code` | Grep(Era.Core.Tests/HeadlessUITests.cs) | `not_contains` | `Console.SetIn(Console.In)` | [x] |
| 4 | 2 SetIn methods capture originalIn before redirect | `code` | Grep(Era.Core.Tests/HeadlessUITests.cs) | `count_equals` | 2 occurrences of `var originalIn = Console.In` | [x] |
| 5 | All tests pass after cleanup fix | `build` | `dotnet test Era.Core.Tests` | `succeeds` | Exit code 0, all tests pass | [x] |

### AC Details

**AC#1: All 4 old SetOut methods capture originalOut before redirect**
- The 4 old methods (TestOutputDialogueValid, TestOutputStateValid, TestReadInputValid, TestReadInputEmpty) currently lack `var originalOut = Console.Out` before `Console.SetOut(stringWriter)`. After fix, all 13 Console-redirecting methods must have this capture line.
- Verification: Grep for `var originalOut = Console.Out` in HeadlessUITests.cs and count exactly 13 matches (4 old methods fixed + 9 new methods already correct).
- Edge case: Ensure the capture line appears BEFORE the `Console.SetOut(stringWriter)` call, not after.

**AC#2: All 13 Console-redirecting methods use try/finally**
- Every method that calls `Console.SetOut` or `Console.SetIn` must wrap the Act/Assert/Cleanup sequence in a try/finally block, with cleanup in the finally block.
- Verification: Grep for `finally` keyword in HeadlessUITests.cs and count exactly 13 matches (one per Console-redirecting method).
- Edge case: Methods that do NOT use Console redirect (TestGameEngineIsHeadless, TestOutputDialogueNull, TestReadScriptedInputValid, TestReadScriptedInputEmpty) must NOT have try/finally added (unnecessary complexity).

**AC#3a: No Console.SetOut no-op patterns remain**
- After fix, zero occurrences of `Console.SetOut(Console.Out)` must exist in the file. This pattern is always a no-op after redirection and indicates a bug.
- Verification: Grep for `Console.SetOut(Console.Out)` pattern and confirm zero matches.
- Note: The correct pattern is `Console.SetOut(originalOut)`.

**AC#3b: No Console.SetIn no-op patterns remain**
- After fix, zero occurrences of `Console.SetIn(Console.In)` must exist in the file. This pattern is always a no-op after redirection and indicates a bug.
- Verification: Grep for `Console.SetIn(Console.In)` pattern and confirm zero matches.
- Note: The correct pattern is `Console.SetIn(originalIn)`.

**AC#4: 2 SetIn methods capture originalIn before redirect**
- TestReadInputValid and TestReadInputEmpty both call `Console.SetIn(stringReader)` and need `var originalIn = Console.In` captured before redirection.
- Verification: Grep for `var originalIn = Console.In` in HeadlessUITests.cs and count exactly 2 matches.
- Edge case: Only these 2 methods use Console.SetIn; no other methods should have this capture.

**AC#5: All tests pass after cleanup fix**
- The cleanup fix must not change any test behavior. All existing tests must continue to pass.
- Verification: Run `dotnet test Era.Core.Tests` and confirm exit code 0 with all tests passing.
- Edge case: The try/finally pattern changes exception flow but should not affect the happy path. If a test was previously passing, it must still pass.

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Single-file mechanical transformation** with two distinct fix patterns:

1. **Pattern A: Fix old methods with no-op cleanup** (4 methods: TestOutputDialogueValid, TestOutputStateValid, TestReadInputValid, TestReadInputEmpty)
   - Add `var originalOut = Console.Out` before `Console.SetOut(stringWriter)`
   - Add `var originalIn = Console.In` before `Console.SetIn(stringReader)` (for the 2 SetIn methods)
   - Wrap Act/Assert/Cleanup sequence in `try { ... } finally { Console.SetOut(originalOut); }`
   - Replace `Console.SetOut(Console.Out)` with `Console.SetOut(originalOut)` in finally block
   - Replace `Console.SetIn(Console.In)` with `Console.SetIn(originalIn)` in finally block

2. **Pattern B: Add try/finally to new methods** (9 methods: TestDisplayMode*)
   - These methods already capture `var originalOut = Console.Out` correctly
   - Wrap Act/Assert/Cleanup sequence in `try { ... } finally { Console.SetOut(originalOut); }`
   - Move existing `Console.SetOut(originalOut)` call into finally block

**Transformation preserves all test logic**. Only cleanup patterns change. The try block contains Act + Assert, finally block contains Console restore + Dispose calls.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Pattern A adds `var originalOut = Console.Out` to 4 old methods. Combined with 9 new methods that already have this line, yields 13 total occurrences. |
| 2 | Both Pattern A and Pattern B wrap the Act/Assert/Cleanup sequence in try/finally. All 13 Console-redirecting methods gain a finally block (one per method). |
| 3a | Pattern A replaces `Console.SetOut(Console.Out)` with `Console.SetOut(originalOut)` in finally block. After transformation, zero Console.SetOut no-op patterns remain. |
| 3b | Pattern A replaces `Console.SetIn(Console.In)` with `Console.SetIn(originalIn)` in finally block. After transformation, zero Console.SetIn no-op patterns remain. |
| 4 | Pattern A adds `var originalIn = Console.In` before `Console.SetIn(stringReader)` in TestReadInputValid and TestReadInputEmpty. Yields exactly 2 occurrences. |
| 5 | Transformation preserves all Arrange/Act/Assert logic. Only cleanup mechanism changes (inline → finally block). All tests continue to pass because try/finally does not affect happy-path execution. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Scope of try block | (A) Wrap entire method body, (B) Wrap only Act/Assert, (C) Wrap only Act | B: Wrap Act/Assert | Arrange section does not need exception safety (no Console state yet). Cleanup code must be in finally, not try. Wrap Act/Assert because assertion failures must trigger finally. |
| Capture timing | (A) Capture originalOut before Arrange, (B) Capture after Arrange but before Act | B: After Arrange, before Act | Matches existing F682 pattern (line 259, 290, etc.). Captures original Console.Out at latest safe point before redirection. Keeps Arrange section clean. |
| Dispose location | (A) Keep Dispose in finally, (B) Move Dispose after finally block | A: Keep Dispose in finally | Ensures StringWriter/StringReader are disposed even if Console.SetOut fails (unlikely but defensive). Matches defensive coding principle. |
| Method exclusion | (A) Add try/finally to all test methods, (B) Add only to Console-redirecting methods | B: Only Console-redirecting methods | Methods without Console redirect (TestGameEngineIsHeadless, TestOutputDialogueNull, TestReadScriptedInputValid, TestReadScriptedInputEmpty) do not need exception safety for Console cleanup. Adding unnecessary try/finally violates YAGNI and reduces code clarity. |

### Implementation Pattern

**Pattern A Template (old methods with no-op cleanup):**

```csharp
[Fact]
public void TestOutputDialogueValid()
{
    // Arrange
    var headlessUI = new HeadlessUI();
    var dialogue = DialogueResult.Create(...);

    var originalOut = Console.Out;  // ADD THIS LINE
    var stringWriter = new StringWriter();

    try  // ADD TRY BLOCK
    {
        Console.SetOut(stringWriter);

        // Act
        headlessUI.OutputDialogue(dialogue);

        // Assert
        var output = stringWriter.ToString();
        Assert.Contains("[Dialogue] 最近一緒にいると...", output);
        Assert.Contains("[Dialogue] 心が温かくなりますわ", output);
    }
    finally  // ADD FINALLY BLOCK
    {
        Console.SetOut(originalOut);  // REPLACE Console.Out with originalOut
        stringWriter.Dispose();
    }
}
```

**Pattern B Template (new methods with correct capture but no try/finally):**

```csharp
[Fact]
[Trait("AC", "5")]
public void TestDisplayModeDefault()
{
    // Arrange
    var headlessUI = new HeadlessUI();
    var dialogue = DialogueResult.Create(...);

    var originalOut = Console.Out;  // ALREADY EXISTS
    var stringWriter = new StringWriter();

    try  // ADD TRY BLOCK
    {
        Console.SetOut(stringWriter);

        // Act
        headlessUI.OutputDialogue(dialogue);

        // Assert
        var output = stringWriter.ToString();
        Assert.Contains("[Dialogue] Test line", output);
    }
    finally  // ADD FINALLY BLOCK
    {
        Console.SetOut(originalOut);  // MOVE FROM CLEANUP SECTION
        stringWriter.Dispose();
    }
}
```

**Pattern A with SetIn (2 methods: TestReadInputValid, TestReadInputEmpty):**

```csharp
[Fact]
public void TestReadInputValid()
{
    // Arrange
    var headlessUI = new HeadlessUI();
    var testInput = "311";

    var originalIn = Console.In;   // ADD THIS LINE
    var originalOut = Console.Out; // ADD THIS LINE
    var stringReader = new StringReader(testInput);
    var stringWriter = new StringWriter();

    try  // ADD TRY BLOCK
    {
        Console.SetIn(stringReader);
        Console.SetOut(stringWriter);

        // Act
        var result = headlessUI.ReadInput();

        // Assert
        Assert.Equal(testInput, result);
    }
    finally  // ADD FINALLY BLOCK
    {
        Console.SetIn(originalIn);   // REPLACE Console.In with originalIn
        Console.SetOut(originalOut); // REPLACE Console.Out with originalOut
        stringReader.Dispose();
        stringWriter.Dispose();
    }
}
```

### Affected Methods

**Pattern A (4 methods):**
- Line 29: `TestOutputDialogueValid` - SetOut only
- Line 66: `TestOutputStateValid` - SetOut only
- Line 99: `TestReadInputValid` - SetIn + SetOut both
- Line 155: `TestReadInputEmpty` - SetIn + SetOut both

**Pattern B (9 methods):**
- Line 250: `TestDisplayModeDefault`
- Line 281: `TestDisplayModeNewline`
- Line 312: `TestDisplayModeWait`
- Line 344: `TestDisplayModeKeyWait`
- Line 376: `TestDisplayModeDisplay`
- Line 407: `TestDisplayModeDisplayWait`
- Line 439: `TestDisplayModeKeyWaitNewline`
- Line 470: `TestDisplayModeKeyWaitWait`
- Line 503: `TestDisplayModeDisplayNewline`

**Excluded methods (no Console redirect, no changes needed):**
- Line 132: `TestGameEngineIsHeadless`
- Line 184: `TestOutputDialogueNull`
- Line 203: `TestReadScriptedInputValid`
- Line 227: `TestReadScriptedInputEmpty`

### Validation Strategy

After transformation:
1. **Grep verification**: Count occurrences to verify AC#1, AC#2, AC#4
2. **Grep negative verification**: Confirm zero matches for no-op patterns (AC#3)
3. **Build verification**: `dotnet test Era.Core.Tests` must succeed with all tests passing (AC#5)

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3a,3b,4 | Apply Pattern A to 4 old methods: Add originalOut/originalIn capture, wrap in try/finally, replace no-op cleanup | [x] |
| 2 | 2,3a | Apply Pattern B to 9 new methods: Wrap Act/Assert/Cleanup in try/finally, move Console.SetOut(originalOut) to finally block | [x] |
| 3 | 1,2,3a,3b,4,5 | Verify all ACs with code inspection (Grep) and test execution (dotnet test) | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1 | Pattern A template from Technical Design | 4 methods fixed with originalOut/originalIn capture + try/finally |
| 2 | implementer | sonnet | T2 | Pattern B template from Technical Design | 9 methods wrapped with try/finally |
| 3 | ac-tester | haiku | T3 | AC verification commands from AC Details | Test results for all 5 ACs |

**Constraints** (from Technical Design):
1. Pattern A applies to 4 old methods (TestOutputDialogueValid line 29, TestOutputStateValid line 66, TestReadInputValid line 99, TestReadInputEmpty line 155)
2. Pattern B applies to 9 new methods (TestDisplayMode* starting at line 250)
3. Excluded methods (TestGameEngineIsHeadless, TestOutputDialogueNull, TestReadScriptedInputValid, TestReadScriptedInputEmpty) must NOT be modified
4. try block wraps Act + Assert sections only (not Arrange)
5. finally block contains Console.SetOut/SetIn restore + Dispose calls
6. Capture originalOut/originalIn after Arrange section, before Act section

**Pre-conditions**:
- F682 is [DONE] (all 13 Console-redirecting test methods exist in HeadlessUITests.cs)
- Era.Core.Tests builds successfully
- All existing tests pass before transformation

**Success Criteria**:
- AC#1: Grep finds exactly 13 occurrences of `var originalOut = Console.Out`
- AC#2: Grep finds exactly 13 occurrences of `finally` keyword
- AC#3a: Grep finds zero occurrences of `Console.SetOut(Console.Out)`
- AC#3b: Grep finds zero occurrences of `Console.SetIn(Console.In)`
- AC#4: Grep finds exactly 2 occurrences of `var originalIn = Console.In`
- AC#5: `dotnet test Era.Core.Tests` exits with code 0, all tests pass

**Rollback Plan**:

If issues arise after implementation:
1. Revert commit with `git revert <commit-hash>`
2. Notify user of rollback with failure details
3. Create follow-up feature for fix with additional investigation into why try/finally caused unexpected behavior (unlikely scenario)

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Date | Phase | Note |
|------|-------|------|
| 2026-01-31 | Phase 1 | Initialized, [REVIEWED] → [WIP] |
| 2026-01-31 | Phase 2 | Investigation confirmed 13 methods need transformation |
| 2026-01-31 | Phase 3 | Skipped (cleanup fix, no new tests) |
| 2026-01-31 | Phase 4 | T1 Pattern A (4 methods), T2 Pattern B (9 methods) - SUCCESS |
| 2026-01-31 | Phase 6 | All 6 ACs verified PASS |
| 2026-01-31 | Phase 7 | Post-review OK, doc-check OK |
