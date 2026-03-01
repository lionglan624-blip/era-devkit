# Feature 488: Fix Pre-existing Test Failures

## Status: [CANCELLED]

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

## Created: 2026-01-14

---

## Summary

Fix 23 pre-existing test failures in Era.Core.Tests that were discovered during F476/F478 implementation but marked as out-of-scope.

**Output**: Updated `Era.Core.Tests/Training/SpecialTrainingTests.cs` and `Era.Core.Tests/ArchitectureTests.cs`.

**Volume**: ~50 lines changes (within ~300 line limit).

---

## Background

### Philosophy (Mid-term Vision)

**Test Suite Health** - All Era.Core.Tests must pass to maintain CI integrity. PRE-EXISTING failures that accumulate without tracking lead to broken windows syndrome where new failures go unnoticed.

### Problem (Current Issue)

23 test failures discovered during F476/F478 implementation:

1. **SpecialTrainingTests (22 failures)**: MockVariableStore throws `NotImplementedException` instead of returning `Result.Fail`. When `SpecialTraining.HasScomf16Prerequisites()` calls `GetTalent()`, it throws instead of returning a Result, causing test failures.

2. **ArchitectureTests (1 failure)**: `AC7_StaticClassDeclarations_OnlyPureConstants_Equals_7` expects 8 static classes but actual count is 9 due to new static classes added in recent features.

### Goal (What to Achieve)

1. Fix MockVariableStore to return `Result.Fail` instead of throwing `NotImplementedException`
2. Update ArchitectureTests expected static class count to match actual
3. All 23 previously failing tests pass
4. Era.Core.Tests suite fully green (exit code 0)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | MockVariableStore.GetTalent returns Result | code | Grep | contains | Result.*Fail.*GetTalent | [ ] |
| 2 | MockVariableStore.GetAbility returns Result | code | Grep | contains | Result.*Fail.*GetAbility | [ ] |
| 3 | ArchitectureTests count updated | code | Grep | contains | Assert.Equal\\(9 | [ ] |
| 4 | SpecialTrainingTests pass | test | Bash | succeeds | dotnet test --filter SpecialTrainingTests | [ ] |
| 5 | ArchitectureTests pass | test | Bash | succeeds | dotnet test --filter ArchitectureTests | [ ] |
| 6 | Era.Core.Tests all pass | test | Bash | succeeds | dotnet test Era.Core.Tests | [ ] |
| 7 | Zero technical debt | code | Grep | not_contains | TODO\|FIXME\|HACK | [ ] |

### AC Details

**AC#1**: MockVariableStore.GetTalent implementation
- Test: Grep pattern="Result.*Fail.*GetTalent" path="Era.Core.Tests/Training/SpecialTrainingTests.cs"
- Expected: GetTalent returns Result.Fail instead of throwing NotImplementedException

**AC#2**: MockVariableStore.GetAbility implementation
- Test: Grep pattern="Result.*Fail.*GetAbility" path="Era.Core.Tests/Training/SpecialTrainingTests.cs"
- Expected: GetAbility returns Result.Fail instead of throwing NotImplementedException

**AC#3**: ArchitectureTests static class count
- Test: Grep pattern="Assert.Equal\\(9" path="Era.Core.Tests/ArchitectureTests.cs"
- Expected: Count updated from 8 to 9

**AC#4**: SpecialTrainingTests pass
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~SpecialTrainingTests"`
- Expected: All tests pass (exit code 0)

**AC#5**: ArchitectureTests pass
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~ArchitectureTests"`
- Expected: All tests pass (exit code 0)

**AC#6**: Full test suite pass
- Test: `dotnet test Era.Core.Tests`
- Expected: All tests pass (exit code 0)

**AC#7**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core.Tests/"
- Expected: 0 matches in modified files (SpecialTrainingTests.cs, ArchitectureTests.cs)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Update MockVariableStore to return Result.Fail for unimplemented methods | [ ] |
| 2 | 3 | Update ArchitectureTests expected static class count to 9 | [ ] |
| 3 | 4,5,6,7 | Run tests and verify all pass | [ ] |

<!-- AC:Task 1:1 Rule: 7 ACs = 3 Tasks (batch waivers below) -->
<!-- **Batch verification waiver (Task 1)**: All 12 Result-returning methods verified by AC#4 (SpecialTrainingTests pass implies all methods return Result correctly instead of throwing). AC#1,2 are explicit spot-checks; AC#4 is comprehensive verification. -->
<!-- **Batch verification waiver (Task 3)**: Test execution verification is single operation. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### MockVariableStore Fix

Replace `NotImplementedException` with `Result.Fail` for methods called by SpecialTraining:

```csharp
// Current (Bad)
public Result<int> GetTalent(CharacterId character, TalentIndex talent)
    => throw new NotImplementedException();

// Fixed (Good)
public Result<int> GetTalent(CharacterId character, TalentIndex talent)
    => Result<int>.Fail("MockVariableStore.GetTalent not implemented");
```

**Methods to fix** (return `Result<int>.Fail` instead of throwing):

*Root cause methods (called by SpecialTraining and causing failures):*
- `GetTalent` (SCOMF8,11,12,14,15,16)
- `GetAbility` (SCOMF2)
- `GetMark` (SCOMF4)

*Defensive fix (same pattern, prevent future failures):*
- `GetCharacterFlag`
- `GetPalam`
- `GetBase`
- `GetNowEx`
- `GetMaxBase`
- `GetCup`
- `GetJuel`
- `GetGotJuel`
- `GetPalamLv`

**Methods unchanged** (void setters and non-Result methods can keep throwing):
- All `Set*` methods (void return)
- `GetFlag`, `GetTFlag` (int return, not Result)

### ArchitectureTests Fix

Update the assertion in `ArchitectureTests.cs`:

```csharp
// Current
Assert.Equal(8, staticClassCount);

// Fixed
Assert.Equal(9, staticClassCount);
```

### Error Message Format

MockVariableStore error messages: `"MockVariableStore.{MethodName} not implemented"`

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Superseded by | F489 | Replacement feature with correct scope |
| Discovery | F476 | First documented these failures in Review Notes |
| Discovery | F478 | Blocked by these failures, marked AC#12 as PRE-EXISTING |

---

## Links

- [feature-489.md](feature-489.md) - **Replacement feature** (correct scope)
- [feature-476.md](feature-476.md) - KojoEngine (first documented failures)
- [feature-478.md](feature-478.md) - NtrEngine (blocked by failures)
- [feature-473.md](feature-473.md) - SCOMF Implementation (SpecialTrainingTests source)

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-14 FL iter1**: [pending] Phase2-Validate - AC Table Method column: Testing SKILL shows inconsistent formats (line 59: 'Grep' alone, line 351: 'Grep(path)'). Feature uses 'Grep' in Method with path in AC Details. May be pedantic given SKILL inconsistency.
- **2026-01-14 FL iter2**: [pending] Phase2-Validate - Summary Volume: ~50 lines estimate may be inaccurate. Actual change is ~13 lines. ~50 is conservative upper bound, ~20 is more accurate. Subjective.
- **2026-01-14 FL iter3**: [skipped] Phase2-Validate - CRITICAL: Spec claims 23 failures (22 SpecialTraining + 1 ArchitectureTests) but actual is 22 SpecialTraining, 0 ArchitectureTests. Moreover, most SpecialTraining failures are Assert.IsType() type mismatch (assembly issue), NOT NotImplementedException. MockVariableStore fix only addresses 2-3 failures (scenarioId 15,16). AC#3 invalid (already 9), Problem/Goal sections need rewrite. Scope reduction required.
- **2026-01-14 FL**: **CANCELLED** by user. FL review revealed fundamental spec inaccuracies. User chose to cancel and create new Feature with correct analysis.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-14 | create | opus | Created to track PRE-EXISTING failures from F476/F478 | PROPOSED |
| 2026-01-14 | fl | opus | FL review revealed spec inaccuracies | CANCELLED |
