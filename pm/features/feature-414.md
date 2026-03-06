# Feature 414: TalentIndex CSV Validation Tests

## Status: [DONE]

## Phase: 7 (Technical Debt Consolidation)

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

## Created: 2026-01-09

---

## Summary

Create unit tests that validate all TalentIndex well-known values match their corresponding indices in Talent.csv. This prevents regression of the bug fixed in F413 where ERB→C# migration introduced incorrect index values.

---

## Background

### Philosophy (Mid-term Vision)

**Full C# Architecture**: Type safety enables compile-time checking, self-documenting code, and safer refactoring. Automated validation ensures centralized well-known values remain correct.

### Problem (Current Issue)

F413 fixed 7 incorrect TalentIndex values in AbilityGrowthProcessor. Root cause analysis revealed:

| Issue | Detail |
|-------|--------|
| Root Cause | ERB uses name-based access (`TALENT:ARG:器用な指`), C# requires numeric indices |
| Failure Mode | Manual index lookup during ERB→C# migration introduced typos |
| Risk | Future well-known value additions may repeat the same mistake |

No automated test exists to catch index mismatches between C# code and CSV source of truth.

### Goal (What to Achieve)

1. Create test that reads Talent.csv and validates all TalentIndex well-known values
2. Test fails immediately if any index mismatch is detected
3. New well-known values automatically validated when added (requires manual addition to test dictionary)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | TalentIndexTests.cs exists | file | Glob | exists | Era.Core.Tests/TalentIndexTests.cs | [x] |
| 2 | CSV parsing reads Talent.csv | code | Grep | contains | Talent.csv | [x] |
| 3 | Test validates Virginity index | code | Grep | contains | Virginity | [x] |
| 4 | Test validates SkilledFingers index | code | Grep | contains | SkilledFingers | [x] |
| 5 | Test validates all 15 well-known values | code | Grep | count_equals | 15 | [x] |
| 6 | C# build succeeds | build | dotnet | succeeds | - | [x] |
| 7 | All TalentIndex tests pass | test | dotnet test --filter | succeeds | TalentIndexTests | [x] |
| 8 | Category=Validation applied | code | Grep | contains | Category("Validation") | [x] |
| 9 | Test detects incorrect index (Neg) | test | TDD | fails_then_passes | - | [x] |

### AC Details

**AC#1**: Create new test file `Era.Core.Tests/TalentIndexTests.cs`.

**AC#2**: Test reads `Game/CSV/Talent.csv` to get authoritative index values.

**AC#3-4**: Spot-check that specific well-known values are validated.

**AC#5**: Pattern: `Assert.Equal` (xUnit), File: `Era.Core.Tests/TalentIndexTests.cs`, Matcher: count_equals (expects 15 matches). Well-known values:
- Virginity(0), Affection(3), Courage(10), SelfControl(20), Emotionless(22)
- Chastity(30), LearningSpeed(50), SkilledFingers(51), TongueUser(52), NeedleMaster(53)
- LewdPot(74), AnalManiac(75), LewdNipples(76), Sadist(82), BustSize(105)

**AC#6-7**: No regression, tests pass.

**AC#8**: Use Category="Validation" to group with other CSV validation tests.

**AC#9**: TDD verification - test correctly fails when given incorrect index, then passes with correct index. Verification method: Temporarily modify one TalentIndex value, confirm test failure, then revert.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-2 | Create TalentIndexTests.cs with CSV parsing | [x] |
| 2 | 3-5 | Implement validation for all 15 well-known values | [x] |
| 3 | 6-8 | Verify build and tests pass with Category | [x] |
| 4 | 9 | TDD verification: confirm test fails on incorrect index | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F413 | Root cause analysis identified need for validation tests |

---

## Links

- [feature-413.md](feature-413.md) - Parent feature (root cause analysis source)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 7 definition

---

## Out-of-Scope Note

**ArchitectureTests.AC7 Failure**: Regression test found `AC7_StaticClassDeclarations_OnlyPureConstants_Equals_6` fails (expected 6, got 7 static classes). Root cause: Uncommitted WIP file `Era.Core/DependencyInjection/CallbackFactories.cs` from F405/F406 adds a 7th static class. This is pre-existing and unrelated to F414.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 | create | opus | Follow-up from F413 再発防止策 | PROPOSED |
| 2026-01-09 07:24 | START | implementer | Task 1-2 | - |
| 2026-01-09 07:24 | END | implementer | Task 1-2 | SUCCESS |
| 2026-01-09 07:30 | START | opus | Task 3-4 (TDD verification) | - |
| 2026-01-09 07:30 | END | opus | Task 3-4 | SUCCESS (RED→GREEN verified) |
| 2026-01-09 07:32 | - | regression | Era.Core.Tests | PASS:322/323 (1 pre-existing failure) |
