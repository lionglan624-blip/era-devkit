# Feature 489: MockVariableStore NotImplementedException Fix

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

## Created: 2026-01-14

---

## Summary

Fix 5 NotImplementedException test failures in SpecialTrainingTests by updating MockVariableStore to return `Result.Fail` instead of throwing.

**Output**: Updated `Era.Core.Tests/Training/SpecialTrainingTests.cs` (MockVariableStore class).

**Volume**: ~12 lines changes (within ~300 line limit).

**Scope Boundary**: This feature ONLY addresses NotImplementedException failures. The remaining ~17 Assert.IsType() failures (assembly type mismatch) are out of scope and tracked in F490.

---

## Background

### Philosophy (Mid-term Vision)

**Mock Completeness** - MockVariableStore should implement all IVariableStore methods with valid return types (Result.Fail for unimplemented operations) rather than throwing exceptions. This ensures tests fail with meaningful assertions rather than unexpected exceptions.

### Problem (Current Issue)

22 SpecialTrainingTests failures exist with TWO distinct root causes (discovered during F488 FL review):

1. **NotImplementedException (5 failures)**: MockVariableStore.GetTalent throws instead of returning Result.Fail. When tests call ExecuteScenario for SCOMF scenarios that check TALENT:149, GetTalent throws and tests fail with exception.

2. **Assert.IsType() type mismatch (17 failures)**: Assembly loading issue where `Result<TrainingResult>` from two contexts don't match. **OUT OF SCOPE** - tracked in F490.

### Goal (What to Achieve)

1. Fix MockVariableStore.GetTalent to return `Result<int>.Fail` instead of throwing
2. Defensively fix all other Result-returning methods (same pattern)
3. Reduce test failures from 22 to ~17 (NotImplementedException failures resolved)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | MockVariableStore file exists | file | Glob | exists | Era.Core.Tests/Training/SpecialTrainingTests.cs | [x] |
| 2 | GetTalent returns Result.Fail | code | Grep | contains | GetTalent.*=>.*Result<int>\.Fail | [x] |
| 3 | No NotImplementedException in Result methods | code | Grep | not_contains | Result<int>.*throw new NotImplementedException | [x] |
| 4 | No NotImplementedException in test output | output | Bash | not_contains | NotImplementedException | [x] |
| 5 | Build succeeds | build | Bash | succeeds | dotnet build Era.Core.Tests | [x] |
| 6 | Zero technical debt | code | Grep | not_contains | TODO\|FIXME\|HACK | [x] |

### AC Details

**AC#1**: MockVariableStore file exists
- Test: Glob pattern="Era.Core.Tests/Training/SpecialTrainingTests.cs"
- Expected: File exists

**AC#2**: GetTalent returns Result.Fail
- Test: Grep pattern="GetTalent.*=>.*Result<int>\.Fail" path="Era.Core.Tests/Training/SpecialTrainingTests.cs"
- Expected: Match found (method returns Result.Fail instead of throwing)

**AC#3**: No NotImplementedException in Result methods
- Test: Grep pattern="Result<int>.*throw new NotImplementedException" path="Era.Core.Tests/Training/SpecialTrainingTests.cs"
- Expected: 0 matches (all Result-returning methods should return Result.Fail)

**AC#4**: No NotImplementedException in test output
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~SpecialTrainingTests" 2>&1 | grep NotImplementedException`
- Expected: 0 matches (no NotImplementedException in test output after fix)
- Note: Tests may still fail due to Assert.IsType issue (out of scope), but NotImplementedException should be eliminated

**AC#5**: Build succeeds
- Test: `dotnet build Era.Core.Tests`
- Expected: Exit code 0

**AC#6**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core.Tests/Training/SpecialTrainingTests.cs"
- Expected: 0 matches in MockVariableStore section

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | Update MockVariableStore Result-returning methods to return Result.Fail | [x] |
| 2 | 4,5,6 | Build and verify tests | [x] |

<!-- AC:Task 1:1 Rule: 6 ACs = 2 Tasks (batch waivers below) -->
<!-- **Batch verification waiver (Task 1)**: All 12 Result-returning methods in same class, same pattern per F384 precedent. -->
<!-- **Batch verification waiver (Task 2)**: Build and test verification is single operation. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### MockVariableStore Fix

Replace `throw new NotImplementedException()` with `Result<int>.Fail(...)` for all Result-returning methods:

```csharp
// Current (Bad)
public Result<int> GetTalent(CharacterId character, TalentIndex talent)
    => throw new NotImplementedException();

// Fixed (Good)
public Result<int> GetTalent(CharacterId character, TalentIndex talent)
    => Result<int>.Fail("MockVariableStore.GetTalent not implemented");
```

**Methods to fix** (return `Result<int>.Fail` instead of throwing):

| Method | Line | Note |
|--------|------|------|
| GetCharacterFlag | 106 | Defensive |
| GetAbility | 108 | Defensive (SCOMF2 prerequisite) |
| GetTalent | 110 | Root cause of 5 failures (SCOMF 8,11,12,14,15,16 prerequisites) |
| GetPalam | 112 | Defensive |
| GetBase | 114 | Defensive |
| GetMark | 116 | Defensive (SCOMF4 prerequisite) |
| GetNowEx | 118 | Defensive |
| GetMaxBase | 120 | Defensive |
| GetCup | 122 | Defensive |
| GetJuel | 124 | Defensive |
| GetGotJuel | 126 | Defensive |
| GetPalamLv | 128 | Defensive |

**Methods unchanged** (void setters and non-Result methods can keep throwing):
- All `Set*` methods (void return)
- `GetFlag`, `GetTFlag` (int return, not Result)

### Error Message Format

MockVariableStore error messages: `"MockVariableStore.{MethodName} not implemented"`

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Supersedes | F488 | Cancelled due to incorrect scope; this feature has correct analysis |

---

## Links

- [feature-488.md](feature-488.md) - Cancelled predecessor (incorrect scope)
- [feature-473.md](feature-473.md) - SCOMF Implementation (SpecialTrainingTests source)
- [feature-490.md](feature-490.md) - Assert.IsType() assembly mismatch fix (out of scope, ~17 failures remain after this fix)

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-14 | create | opus | Created with correct scope from F488 analysis | PROPOSED |
| 2026-01-14 09:38 | START | implementer | Task 1 | - |
| 2026-01-14 09:38 | END | implementer | Task 1 | SUCCESS |
| 2026-01-14 09:38 | START | implementer | Task 2 | - |
| 2026-01-14 09:38 | END | implementer | Task 2 | SUCCESS |
