# Feature 395: Phase 6 Mark System Migration

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

## Created: 2026-01-07

---

## Summary

Migrate mark system (刻印システム from TRACHECK.ERB lines 566-849) to C# Era.Core with IMarkSystem interface.

---

## Background

### Philosophy (Mid-term Vision)

**Full C# Architecture**: Progressive migration from ERB to C# for maintainability, type safety, and testability.

### Problem (Current Issue)

Mark system logic is embedded in TRACHECK.ERB (actual: ~284 lines, lines 566-849):
- MARK_GOT_CHECK: Mark acquisition validation
- 屈服刻印* (Submission marks): Submission-based marking logic
- 快楽刻印* (Pleasure marks): Pleasure-based marking logic
- 反発刻印* (Resistance marks): Resistance-based marking logic
- 苦痛刻印* (Pain marks): Pain-based marking logic

This makes mark system logic hard to test, extend, and maintain as part of training result processing.

### External Dependencies

**CUP Array Access**: Mark calculators require CUP (Cumulative Parameter) access (CUP:奴隷:快Ｃ, CUP:奴隷:苦痛, etc. at lines 619, 656, 686, 748, 804). Options:
1. Add GetCup/SetCup to F399 scope
2. Create separate feature for CUP access
3. Use `Func<CupIndex, int>` callback injection (like InfoState pattern) - **Recommended for initial implementation**

### Goal (What to Achieve)

Migrate mark system to C# Era.Core with:
- IMarkSystem interface for mark acquisition and validation
- MarkType enum for strongly typed mark categories
- Result<MarkAcquisition> return type for error handling
- Training result domain logic extracted from validation code
- Category=Mark unit tests verifying legacy behavior equivalence

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | MarkType enum exists | file | Read(Era.Core/Training/MarkType.cs) | exists | - | [x] |
| 2 | IMarkSystem interface exists | file | Read(Era.Core/Training/IMarkSystem.cs) | exists | - | [x] |
| 3 | MarkSystem implementation exists | file | Read(Era.Core/Training/MarkSystem.cs) | exists | - | [x] |
| 4 | SubmissionMarkCalculator exists | code | Grep("SubmissionMarkCalculator", Era.Core/Training/MarkSystem.cs) | exists | - | [x] |
| 5 | PleasureMarkCalculator exists | code | Grep("PleasureMarkCalculator", Era.Core/Training/MarkSystem.cs) | exists | - | [x] |
| 6 | ResistanceMarkCalculator exists | code | Grep("ResistanceMarkCalculator", Era.Core/Training/MarkSystem.cs) | exists | - | [x] |
| 7 | PainMarkCalculator exists | code | Grep("PainMarkCalculator", Era.Core/Training/MarkSystem.cs) | exists | - | [x] |
| 8 | IMarkSystem DI registration | code | Grep("IMarkSystem", Era.Core/DependencyInjection/) | contains | "AddSingleton<IMarkSystem" | [x] |
| 9 | C# build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 10 | Era.Core.Tests build succeeds | build | dotnet build Era.Core.Tests | succeeds | - | [x] |
| 11 | Mark system unit tests exist | file | Grep("Category=Mark", Era.Core.Tests/) | count_gte | 5 | [x] |
| 12 | All mark system tests pass | test | dotnet test --filter Category=Mark | succeeds | - | [x] |
| 13 | Invalid trainer rejected | test | dotnet test --filter "Category=Mark&DisplayName~InvalidTrainer" | succeeds | - | [x] |
| 14 | Self-training rejected | test | dotnet test --filter "Category=Mark&DisplayName~SelfTraining" | succeeds | - | [x] |

### AC Details

**AC#1**: MarkType enum per F377 design principles (Submission, Pleasure, Resistance, Pain)

**AC#2**: IMarkSystem interface with method signatures:
- `AcquireMark(CharacterId target, CharacterId trainer) -> Result<MarkAcquisitionResult>`

**Validation (ERB lines 569-576)**: Returns empty result if:
- trainer < 0 (invalid trainer)
- target == trainer (self-training not allowed)

**ERB RETURN mapping**: ERB RETURN values (0=no change, 1=mark acquired) captured via `AcquiredMarks.Count > 0`.

**Note**: ValidateMark removed (YAGNI). ERB source has no separate validation method. If needed later, add as new feature.

**MarkAcquisitionResult type** (supports multiple marks per call):
```csharp
public record MarkAcquisition(
    MarkType Type,
    MarkLevel Level,
    CharacterId? Master          // 主人 = trainer (調教者). Maps to CFLAG:*刻印主人 (lines 603,730,789,846)
);

public record MarkAcquisitionResult(
    List<MarkAcquisition> AcquiredMarks,
    List<StateChange> SideEffects
);

// StateChange is abstract base type with concrete subtypes:
public abstract record StateChange;
public record AbilityChange(AblIndex Index, int Delta) : StateChange;
public record CFlagChange(CFlagIndex Index, int Value) : StateChange;
public record TCVarChange(TCVarIndex Index, int Value) : StateChange;
public record TFlagChange(FlagIndex Index, int Value) : StateChange;  // TFLAG:25 writes
public record ExpChange(ExpIndex Index, int Value) : StateChange;
public record MarkHistoryChange(MarkIndex Index, int Value) : StateChange;

// ERB lines: ABL:597-599,722-726,776-782,823-827; CFLAG:603,730,769-770,789,846;
// TCVAR:642,673,703,731,790,847; TFLAG:598-599,724-726,778,781,825-826; EXP:834-842; MARK:4=792

// Note: MarkIndex.ResistanceHistory = new(4) tracks max resistance mark level (line 792)
```

**MarkIndex well-known value** (required for ResistanceMarkCalculator):
- MarkIndex.ResistanceHistory = new(4) - tracks max resistance mark level

**MarkLevel enum** (mark intensity 0-3):
```csharp
public enum MarkLevel { None = 0, Lv1 = 1, Lv2 = 2, Lv3 = 3 }
```

**AC#3**: MarkSystem.AcquireMark orchestrates 4 calculators mirroring MARK_GOT_CHECK call sequence (lines 577-580):
1. PainMarkCalculator (苦痛刻印取得チェック) - AC#7
2. PleasureMarkCalculator (快楽刻印取得チェック) - AC#5
3. SubmissionMarkCalculator (屈服刻印取得チェック) - AC#4
4. ResistanceMarkCalculator (反発刻印取得チェック) - AC#6

**Note**: AC# numbers (4-7) reflect file organization, not execution order. Implementation must follow the sequence above.

**Aggregation**: Returns `Result<MarkAcquisitionResult>` containing all acquired marks (List<MarkAcquisition>) since ERB can acquire multiple mark types in a single call.

**Pure Calculators**: Calculators are pure functions - they only determine MarkLevel. MarkSystem orchestrator applies all state changes (MARK, ABL, CFLAG, TCVAR, TFLAG:25, EXP writes) based on calculator results.

**TFLAG:25 Handling**: All mark acquisitions may trigger TFLAG:25 writes (ERB lines 598-599, 724-726, 778, 781, 825-826). The orchestrator emits TFlagChange(FlagIndex.Flag25, value) as SideEffects.

**AC#4**: SubmissionMarkCalculator.Calculate() internally calls 3 sub-methods and returns MAX:
- CalculateFromPleasure() (屈服刻印取得チェック_快楽)
- CalculateFromFear() (屈服刻印取得チェック_恐怖)
- CalculateFromObedience() (屈服刻印取得チェック_従順)

Returns `MAX(CalculateFromPleasure, MAX(CalculateFromFear, CalculateFromObedience))` per ERB lines 590-592.

**AC#4-7 Constructor Pattern**: All calculators receive via constructor injection:
- `Func<CupIndex, int> cupAccessor` - per External Dependencies recommendation. CUP indices (full-width as in ERB): 快Ｃ, 快Ｖ, 快Ａ, 快Ｂ, 苦痛, 恐怖, 恭順, 屈服, 反感, 不快.
- `Func<MarkIndex, int> markAccessor` - for reading existing mark levels (lines 622, 638, 669, 700, 716-717, 762, 772, 792, 817). Uses IVariableStore.GetMark from F399.

**AC#5**: PleasureMarkCalculator additionally requires `Func<TCVarIndex, int> tcvarAccessor` for TCVAR:快楽強度 (line 714).

**TCVarIndex well-known values** (required for mark calculators):
- PainMarkAcquired = 40, PleasureMarkAcquired = 41, SubmissionMarkAcquired = 42, ResistanceMarkAcquired = 43
- ResistanceMarkSuppression = 44, PleasureIntensity = 106

**AC#5-7**: PleasureMarkCalculator, ResistanceMarkCalculator, PainMarkCalculator each implement:
- `Calculate(CharacterId target, CharacterId trainer) -> MarkLevel`

**AC#6 Note**: ResistanceMarkCalculator additionally requires `Func<TCVarIndex, int> tcvarAccessor` for TCVAR:反発刻印取得抑制 (line 743). This suppression flag prevents resistance mark acquisition when set.

**AC#7 Note**: PainMarkCalculator additionally requires:
- `Func<TalentIndex, bool> talentAccessor` for TALENT:調教者:サド check (line 833)
- `Func<int> getSelectCom` for SELECTCOM check - only when TALENT:調教者:針さばき is true (line 835: `SELECTCOM == 103`)

**TalentIndex well-known values** (required for PainMarkCalculator):
- TalentIndex.Sadist (サド) - trainer sadism talent check
- TalentIndex.NeedleMaster (針さばき) - trainer needle skill talent check

**AC#8**: DI registration per F377 container pattern

**AC#9-10**: Build verification

**AC#11-12**: TDD verification with legacy behavior equivalence

**AC#13-14**: Negative test cases (engine type requirement)
- AC#13: Test that trainer < 0 returns `Result<MarkAcquisitionResult>.Success` with empty `AcquiredMarks` list (not Failure - invalid trainer is a valid no-op scenario per ERB line 569 RETURN)
- AC#14: Test that target == trainer (self-training) returns `Result<MarkAcquisitionResult>.Success` with empty `AcquiredMarks` list (not Failure - per ERB line 571 RETURN)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | - | Add well-known values (TCVarIndex x6, MarkIndex.ResistanceHistory, TalentIndex x5) | [x] |
| 1 | 1 | Create MarkType enum and MarkLevel enum (Era.Core/Training/) | [x] |
| 2 | 2 | Define IMarkSystem interface with AcquireMark method | [x] |
| 3 | 3 | Implement MarkSystem orchestration (MARK_GOT_CHECK logic) | [x] |
| 4 | 4 | Implement SubmissionMarkCalculator (屈服刻印* logic) | [x] |
| 5 | 5 | Implement PleasureMarkCalculator (快楽刻印* logic) | [x] |
| 6 | 6 | Implement ResistanceMarkCalculator (反発刻印* logic) | [x] |
| 7 | 7 | Implement PainMarkCalculator (苦痛刻印* logic) | [x] |
| 8 | 8 | Register IMarkSystem in DI container | [x] |
| 9 | 9-10 | Verify C# builds (Era.Core and Era.Core.Tests) | [x] |
| 10 | 11 | Create unit tests for mark system (Category=Mark) | [x] |
| 11 | 12 | Verify all mark system tests pass | [x] |
| 12 | 13 | Create InvalidTrainer negative test | [x] |
| 13 | 14 | Create SelfTraining negative test | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F390 | Phase 6 planning feature |
| Predecessor | F393 | Training Processing patterns (Era.Core/Training/ structure, IVariableStore usage patterns) |
| Predecessor | F399 | IVariableStore MARK accessors (GetMark/SetMark for mark read/write) |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 6 definition
- [feature-377.md](feature-377.md) - F377 Design Principles (YAGNI/KISS, Strongly Typed IDs)
- [feature-390.md](feature-390.md) - Phase 6 planning
- [feature-399.md](feature-399.md) - F399 IVariableStore Extensions (MARK/CUP accessors)
- [feature-401.md](feature-401.md) - F401 StateChange Type Safety Migration (follow-up)

---

## 残課題

| Issue | Description | Tracked In |
|-------|-------------|------------|
| StateChange 統一 | F393 の string-based StateChange を abstract 階層に移行 | → F401 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-07 22:00 | create | implementer | Created from F390 revision (actual measurements) | PROPOSED |
| 2026-01-08 | START | opus | /do 395 | WIP |
| 2026-01-08 | END | implementer | Task 0-8: Implementation complete | SUCCESS |
| 2026-01-08 | END | opus | Verification: 13/13 Mark tests, 226/226 Era.Core.Tests | PASS |
| 2026-01-08 | END | opus | selectCom fix + workflow improvement (do.md Step 8.4.5) | SUCCESS |
| 2026-01-08 | END | opus | Final: 13/13 Mark, 239/239 Era.Core.Tests | DONE |
