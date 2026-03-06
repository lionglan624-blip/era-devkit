# Feature 484: SCOMF Prerequisite Checks

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

## Created: 2026-01-13

---

## Summary

**Phase 14: Era.Core Engine - SCOMF IsScenarioAvailable Detail Conditions**

Extend SpecialTraining.IsScenarioAvailable with prerequisite checks based on TALENT/ABL/FLAG conditions for each SCOMF1-16 scenario. Currently only validates range (1-16), deferred from F473.

**Prerequisite**: F473 (SCOMF Full Implementation) completed - provides base IsScenarioAvailable method.

**Output**:
- Enhanced `Era.Core/Training/SpecialTraining.cs` - IsScenarioAvailable with TALENT/ABL/FLAG checks
- Unit tests in `Era.Core.Tests/Training/ScenarioPrerequisiteTests.cs`

**Volume**: ~200 lines (prerequisite logic + tests)

---

## Background

### Philosophy (Mid-term Vision)

**Phase 14: Era.Core Engine** - Establish pure C# game engine with headless execution capability. SCOMF prerequisite checks enable proper scenario availability validation based on character state, preventing impossible scenarios from being available.

### Problem (Current Issue)

F473 implemented ExecuteScenario for SCOMF1-16 but deferred IsScenarioAvailable detail conditions. Current implementation only validates range (1-16):

```csharp
public bool IsScenarioAvailable(int scenarioId, CharacterId target)
{
    return scenarioId >= 1 && scenarioId <= 16;
}
```

This allows scenarios to be available even when character lacks required TALENT/ABL/FLAG values, causing logic errors.

### Goal (What to Achieve)

1. **Implement prerequisite checks** - TALENT/ABL/FLAG conditions per scenario
2. **Define scenario requirements** - Document required conditions for each SCOMF
3. **Test availability validation** - Unit tests for prerequisite logic
4. **Maintain backward compatibility** - Keep existing ExecuteScenario logic unchanged
5. **Eliminate technical debt** - Zero TODO/FIXME/HACK comments

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IsScenarioAvailable has prerequisite logic | code | Grep | contains | "HasScomf.*Prerequisites" | [x] |
| 2 | SCOMF1 prerequisite test | test | Bash | succeeds | "dotnet test --filter Scomf1Prerequisite" | [x] |
| 3 | SCOMF16 prerequisite test | test | Bash | succeeds | "dotnet test --filter Scomf16Prerequisite" | [x] |
| 4 | Multiple scenarios prerequisite | test | Bash | succeeds | "dotnet test --filter MultipleScenarioPrerequisites" | [x] |
| 5 | Missing TALENT (Neg) | test | Bash | succeeds | "dotnet test --filter MissingTalentScenarioUnavailable" | [x] |
| 6 | Insufficient ABL (Neg) | test | Bash | succeeds | "dotnet test --filter InsufficientAblScenarioUnavailable" | [x] |
| 7 | Build succeeds | build | Bash | succeeds | "dotnet build Era.Core" | [x] |
| 8 | Zero technical debt | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |

### AC Details

**AC#1**: IsScenarioAvailable prerequisite logic verification
- Test: Grep pattern="HasScomf.*Prerequisites" path="Era.Core/Training/SpecialTraining.cs"
- Expected: Contains prerequisite helper method calls in switch expression
- Verifies: Prerequisite methods implemented (HasScomf1Prerequisites, etc.)

**AC#2**: SCOMF1 prerequisite validation (behavior test)
- Test: `dotnet test --filter Scomf1Prerequisite`
- Test cases: Character with required TALENT available, Without required TALENT unavailable
- Expected: All SCOMF1 prerequisite tests pass
- Verifies: SCOMF1 (シックスナイン) prerequisites correctly checked

**AC#3**: SCOMF16 prerequisite validation (behavior test)
- Test: `dotnet test --filter Scomf16Prerequisite`
- Test cases: Character with lactation TALENT available, Without lactation unavailable
- Expected: All SCOMF16 prerequisite tests pass
- Verifies: SCOMF16 (授乳手コキ) prerequisites correctly checked

**AC#4**: Multiple scenarios prerequisite (behavior test)
- Test: `dotnet test --filter MultipleScenarioPrerequisites`
- Test cases: Check available scenarios for character, Verify only matching scenarios returned
- Expected: All multi-scenario tests pass
- Verifies: Prerequisite logic works across all 16 scenarios

**AC#5**: Missing TALENT rejection (Negative test)
- Test: `dotnet test --filter MissingTalentScenarioUnavailable`
- Test cases: Character lacks required TALENT returns false
- Expected: All missing TALENT tests pass
- Verifies: Proper rejection when prerequisites not met

**AC#6**: Insufficient ABL rejection (Negative test)
- Test: `dotnet test --filter InsufficientAblScenarioUnavailable`
- Test cases: Character has too low ABL value returns false
- Expected: All insufficient ABL tests pass
- Verifies: Proper validation of ABL thresholds

**AC#7**: Build verification
- Test: `dotnet build Era.Core`
- Expected: Build succeeded
- Verifies: No compile errors

**AC#8**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Training/SpecialTraining.cs"
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core.Tests/Training/ScenarioPrerequisiteTests.cs"
- Expected: 0 matches in both files
- Verifies: Clean implementation without deferred work

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | - | Document prerequisite requirements: verify TBD indices (性経験, Gスポット開発, 子宮開発, 素股経験, 手淫技巧, 授乳) from ERB source. Blocker for Task#2 | [x] |
| 2 | 1 | Implement IsScenarioAvailable with TALENT/ABL/FLAG checks per scenario | [x] |
| 3 | 2,3,4 | Write unit tests for scenario prerequisite validation (SCOMF1, SCOMF16, multiple) | [x] |
| 4 | 5,6 | Write negative tests (missing TALENT, insufficient ABL) | [x] |
| 5 | 7 | Verify build succeeds with all changes | [x] |
| 6 | 8 | Remove all TODO/FIXME/HACK comments from prerequisite implementation | [x] |

<!-- AC:Task 1:1 Rule: 8 ACs = 6 Tasks (Task#3 covers AC#2-4 as unified prerequisite tests, Task#4 covers AC#5-6 as negative tests) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Prerequisite Requirements by Scenario

**SCOMF Prerequisite Matrix** (to be verified during implementation):

| SCOMF# | Name | TALENT/MARK Req | EXP/ABL Req | Other Req |
|:------:|------|-----------------|-------------|-----------|
| 1 | シックスナイン | 性経験(TBD) | 口淫経験(EXP:25) >= 1 | None |
| 2 | 岩清水 | 性経験(TBD) | 奉仕精神(ABL:13) >= 1 | None |
| 3 | Gスポット刺激 | 性経験(TBD), Gスポット開発(TBD) | 性交経験(EXP:20?) >= 1 | None |
| 4 | 乱れ牡丹 | 性経験(TBD), 快楽刻印(MARK:1) | 性交経験(EXP:20?) >= 3 | None |
| 5 | 手淫フェラ | 性経験(TBD) | 手淫技巧(TBD) >= 2, 口淫経験(EXP:25) >= 2 | None |
| 6 | 挿入Ｇスポ責め | 性経験(TBD), Gスポット開発(TBD) | 性交経験(EXP:20?) >= 3 | None |
| 7 | 挿入子宮口責め | 性経験(TBD), 子宮開発(TBD) | 性交経験(EXP:20?) >= 5 | None |
| 8 | ６９パイズリ | 性経験(TBD) | パイズリ経験(EXP:26) >= 2, 口淫経験(EXP:25) >= 2 | バスト >= C |
| 9 | ダブルフェラ | 性経験(TBD) | 口淫経験(EXP:25) >= 3 | 複数プレイ可能 |
| 10 | ダブル素股 | 性経験(TBD) | 素股経験(TBD-NOT IN CSV) >= 2 | 複数プレイ可能 |
| 11 | ダブルパイズリ | 性経験(TBD) | パイズリ経験(EXP:26) >= 2 | バスト >= C, 複数プレイ可能 |
| 12 | パイズリフェラ | 性経験(TBD) | パイズリ経験(EXP:26) >= 3, 口淫経験(EXP:25) >= 3 | バスト >= C |
| 13 | 交互挿入 | 性経験(TBD) | 性交経験(EXP:20?) >= 5 | 複数プレイ可能 |
| 14 | 母乳飲み | 授乳(TALENT:149?) | None | None |
| 15 | 授乳する | 授乳(TALENT:149?) | None | None |
| 16 | 授乳手コキ | 授乳(TALENT:149?) | 手淫技巧(TBD) >= 1 | None |

**Note**: Variable type column headers above use logical groupings. Actual variable types verified from CSV (see Index Mapping Table below).

**Index Mapping Table** (verified from CSV):

| Name (Japanese) | Variable Type | CSV Index | Source File | Notes |
|-----------------|---------------|:---------:|-------------|-------|
| 口淫経験 | EXP | 25 | EXP.CSV | Verified |
| パイズリ経験 | EXP | 26 | EXP.CSV | Verified |
| Ｖ性交経験 | EXP | 20 | EXP.CSV | Likely maps to "性交経験" |
| 手淫経験 | EXP | 24 | EXP.CSV | Verified |
| キス経験 | EXP | 27 | EXP.CSV | Note: 素股経験 NOT found in CSV |
| 奉仕精神 | ABL | 13 | ABL.CSV | Verified |
| 母乳体質 | TALENT | 149 | TALENT.CSV | Likely maps to "授乳" |
| 快楽刻印 | MARK | 1 | MARK.CSV | Verified |

**TBD - Requires ERB Source Verification**:
- 性経験: No direct TALENT found. May use EXP-based check (e.g., EXP:20 Ｖ性交経験 > 0)
- Gスポット開発: Not found in TALENT.CSV. Requires ERB analysis.
- 子宮開発: Not found in TALENT.CSV. Requires ERB analysis.
- 授乳: Likely TALENT:149 (母乳体質). Verify in ERB.
- 素股経験: NOT in EXP.CSV (EXP:27=キス経験). Requires ERB analysis.
- 手淫技巧: NOT confirmed in ABL.CSV. May be ABL or different variable type.

**Implementation Note**: Use `new ExpIndex(N)` for EXP variables (requires IVariableStore.GetExp method), `new AbilityIndex(N)` for ABL, `new TalentIndex(N)` for TALENT, `new MarkIndex(N)` for MARK.

### Implementation Approach

**Current IsScenarioAvailable** (F473):
```csharp
public bool IsScenarioAvailable(int scenarioId, CharacterId target)
{
    return scenarioId >= 1 && scenarioId <= 16;
}
```

**Enhanced IsScenarioAvailable** (this feature):
```csharp
public bool IsScenarioAvailable(int scenarioId, CharacterId target)
{
    // Range check
    if (scenarioId < 1 || scenarioId > 16)
        return false;

    // Prerequisite checks per scenario
    return scenarioId switch
    {
        1 => HasScomf1Prerequisites(target),
        2 => HasScomf2Prerequisites(target),
        // ... (all 16 scenarios)
        16 => HasScomf16Prerequisites(target),
        _ => false
    };
}

private bool HasScomf1Prerequisites(CharacterId target)
{
    // Check TALENT:性経験 (TBD - verify actual index from ERB)
    // var sexExpResult = _variableStore.GetTalent(target, new TalentIndex(/* TBD */));
    // if (sexExpResult is not Result<int>.Success sexExp || sexExp.Value == 0)
    //     return false;

    // Check EXP:口淫経験 >= 1 (verified: EXP:25)
    var oralExpResult = _variableStore.GetExp(target, new ExpIndex(25));
    if (oralExpResult is not Result<int>.Success oralExp || oralExp.Value < 1)
        return false;

    return true;
}

// ... similar methods for SCOMF2-16
```

### Error Handling

IsScenarioAvailable returns `bool`, not `Result<bool>`. Variable access failures should:
- Return `false` (assume prerequisite not met if cannot check)
- Note: IVariableStore returns Result<T> for variable access errors; no separate logging required

### Test Naming Convention

Test methods follow `Test{ScenarioId}Prerequisite{Condition}` format:
- `TestScomf1PrerequisiteWithTalent` - SCOMF1 available with required TALENT
- `TestScomf1PrerequisiteWithoutTalent` - SCOMF1 unavailable without TALENT
- `TestMultipleScenarioPrerequisitesFiltered` - Multiple scenario check
- `TestMissingTalentScenarioUnavailableError` - Negative case

This ensures AC filter patterns match correctly.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F471 | Phase 14 Planning must complete first |
| Predecessor | F473 | SCOMF Full Implementation provides base IsScenarioAvailable |
| Dependency | F469 | SCOMF Variable Infrastructure provides IVariableStore |

---

## Links

- [feature-471.md](feature-471.md) - Phase 14 Planning (parent feature)
- [feature-473.md](feature-473.md) - SCOMF Full Implementation (predecessor, deferred work)
- [feature-469.md](feature-469.md) - SCOMF Variable Infrastructure (dependency, provides IVariableStore)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 14 Task 13
- [feature-template.md](reference/feature-template.md) - Feature template

---

## 残課題 (Deferred Tasks)

None.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-13 | create | spec-writer | Created from F471 Phase 14 Planning | PROPOSED |
| 2026-01-13 20:14 | implement | implementer | Task 1-6: Prerequisites implemented following TDD Phase 3 | SUCCESS |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-13 FL iter1**: [resolved] Phase2-Validate - AC#1 Matcher: Grep pattern 'TALENT|ABL|FLAG' may not match actual method calls (GetTalent/GetAbility). Consider pattern='HasScomf.*Prerequisites' to verify prerequisite methods exist. → Applied: Changed to 'HasScomf.*Prerequisites'
- **2026-01-13 FL iter1**: [resolved] Phase2-Validate - Task#1 AC#: Task#1 'Document prerequisite requirements' marked AC# 1,2 but documentation alone does not directly satisfy code ACs. → Applied: Changed Task#1 AC# to '-' (prerequisite for Task#2)
- **2026-01-13 FL iter1**: [applied] Phase2-Validate - AC#2 Invalid: "prerequisite checks deferred" comment does not exist in actual SpecialTraining.cs. → User decision: Deleted AC#2 and Task#3. ACs renumbered 9→8, Tasks renumbered 7→6.
- **2026-01-13 FL iter2**: [resolved] Phase2-Validate - AC#9 path syntax: comma-separated paths not standard. → Applied: Split into two separate Grep tests.
- **2026-01-13 FL iter3**: [resolved] Phase2-Validate - Prerequisite Matrix: Added Index Mapping Table with verified CSV indices (EXP:25 口淫経験, EXP:26 パイズリ経験, etc.). TBD items documented for ERB verification.
- **2026-01-13 FL iter4**: [resolved] Phase2-Validate - Column headers: Renamed ABL→EXP/ABL, marked TBD items inline. Sample code corrected: GetAbility→GetExp for 口淫経験. 素股経験 NOT in CSV documented.
- **2026-01-13 FL iter5**: [resolved] Phase2-Validate - Task#1 TBD list: Added missing '授乳' to TBD items list.
