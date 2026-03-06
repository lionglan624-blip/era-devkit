# Feature 399: IVariableStore Extensions for Training Processing

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

## Created: 2026-01-08

---

## Summary

Extend IVariableStore with SOURCE, MARK, NOWEX, MAXBASE, CUP array accessors to enable full FavorCalculator and MarkSystem implementation.

---

## Background

### Philosophy (Mid-term Vision)

**Full C# Architecture**: Progressive migration from ERB to C# for maintainability, type safety, and testability.

### Problem (Current Issue)

F393 implemented FavorCalculator as a STUB because the following variable arrays are not accessible via IVariableStore.

Note: Other variable arrays used by FAVOR_CALC (BASE, CFLAG, TALENT, ABL, TFLAG, FLAG) are already accessible via existing IVariableStore methods. Only the following 4 arrays are missing:

| Array | Purpose | ERB Usage |
|-------|---------|-----------|
| SOURCE | Experience source values (indices from source.csv: 快C, 快V, 情愛, 達成, etc.) | FAVOR_CALC lines 1033-1064 |
| MARK | Character marks (反発刻印, etc.) | FAVOR_CALC lines 1085-1095 |
| NOWEX | Current expression counters (C絶頂, V絶頂, A絶頂, B絶頂, 射精). Note: Uses ex.csv indices same as EXP, but NOWEX is a distinct array storing current session counters | FAVOR_CALC line 1065 |
| MAXBASE | Maximum base stat values (満足, ムード, 理性, 怒り) | FAVOR_CALC lines 1065-1078 |
| CUP | Cumulative parameter values (快Ｃ, 快Ｖ, 快Ａ, 快Ｂ, 苦痛, 恐怖, 恭順, 屈服, 反感, 不快) | F395 Mark System (TRACHECK.ERB lines 619, 656, 686, 748, 804) |

Without these, FavorCalculator returns 0 (neutral) instead of calculating actual favor changes, and F395 MarkSystem cannot access CUP values.

**Additional Dependencies** (addressed via existing mechanisms):
- **FLAG**: Already accessible via IVariableStore.GetFlag/SetFlag
- **SELECTCOM/ASSIPLAY**: Passed as method parameters to CalculateFavor
- **PLAYER NOWEX access**: FAVOR_CALC line 1065 requires both TARGET and PLAYER NOWEX values. FavorCalculator needs both CharacterId parameters.
- **MASTER_FAVOR_CHECK2**: Remains ERB call (legacy interop, not C# scope)

### Goal (What to Achieve)

1. Add SourceIndex, MarkIndex, NowExIndex, MaxBaseIndex, CupIndex strongly typed IDs
2. Add corresponding Get/Set methods to IVariableStore (including GetCup/SetCup)
3. Implement in VariableStore and CharacterVariables
4. Implement full FavorCalculator logic using the new accessors (replace STUB)
5. Add comprehensive tests verifying FavorCalculator calculates favor changes correctly

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SourceIndex type exists | file | Glob | exists | Era.Core/Types/SourceIndex.cs | [x] |
| 2 | MarkIndex type exists | file | Glob | exists | Era.Core/Types/MarkIndex.cs | [x] |
| 3 | NowExIndex type exists | file | Glob | exists | Era.Core/Types/NowExIndex.cs | [x] |
| 4 | MaxBaseIndex type exists | file | Glob | exists | Era.Core/Types/MaxBaseIndex.cs | [x] |
| 5 | CupIndex type exists | file | Glob | exists | Era.Core/Types/CupIndex.cs | [x] |
| 6 | IVariableStore has GetSource | code | Grep(IVariableStore.cs) | contains | GetSource | [x] |
| 7 | IVariableStore has GetMark | code | Grep(IVariableStore.cs) | contains | GetMark | [x] |
| 8 | IVariableStore has GetNowEx | code | Grep(IVariableStore.cs) | contains | GetNowEx | [x] |
| 9 | IVariableStore has GetMaxBase | code | Grep(IVariableStore.cs) | contains | GetMaxBase | [x] |
| 10 | IVariableStore has GetCup | code | Grep(IVariableStore.cs) | contains | GetCup | [x] |
| 11 | FavorCalculator uses GetSource | code | Grep(FavorCalculator.cs) | contains | GetSource | [x] |
| 12 | FavorCalculator uses GetMark | code | Grep(FavorCalculator.cs) | contains | GetMark | [x] |
| 13 | FavorCalculator uses GetNowEx | code | Grep(FavorCalculator.cs) | contains | GetNowEx | [x] |
| 14 | FavorCalculator uses GetMaxBase | code | Grep(FavorCalculator.cs) | contains | GetMaxBase | [x] |
| 15 | FavorCalculator accepts IVariableStore via DI | code | Grep(FavorCalculator.cs) | contains | (IVariableStore variables) | [x] |
| 16 | IFavorCalculator has updated signature | code | Grep(IFavorCalculator.cs) | matches | CalculateFavor.*CharacterId.*CharacterId.*int.*int | [x] |
| 17 | C# build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 18 | FavorCalculator tests pass | test | dotnet test --filter FullyQualifiedName~FavorCalculator | succeeds | - | [x] |

### AC Details

**AC#6-10 Method Signatures** (following existing IVariableStore pattern):
- `GetSource(CharacterId character, SourceIndex index) -> Result<int>`
- `SetSource(CharacterId character, SourceIndex index, int value) -> void`
- `GetMark(CharacterId character, MarkIndex index) -> Result<int>`
- `SetMark(CharacterId character, MarkIndex index, int value) -> void`
- `GetNowEx(CharacterId character, NowExIndex index) -> Result<int>`
- `SetNowEx(CharacterId character, NowExIndex index, int value) -> void`
- `GetMaxBase(CharacterId character, MaxBaseIndex index) -> Result<int>`
- `SetMaxBase(CharacterId character, MaxBaseIndex index, int value) -> void`
- `GetCup(CharacterId character, CupIndex index) -> Result<int>`
- `SetCup(CharacterId character, CupIndex index, int value) -> void`

**Note**: Set methods follow existing fire-and-forget pattern (void return, no separate AC needed as they are symmetric to Get methods).

**MarkIndex Well-Known Values** (from mark.csv):
- `MarkIndex.Pain = 0` (苦痛刻印)
- `MarkIndex.Pleasure = 1` (快楽刻印)
- `MarkIndex.Submission = 2` (屈服刻印)
- `MarkIndex.Resistance = 3` (反発刻印)
- `MarkIndex.AffairPleasure = 5` (浮気快楽刻印)
- `MarkIndex.AffairSubmission = 6` (浮気屈服刻印)

**MaxBaseIndex Well-Known Values** (same indices as BaseIndex, from Base.csv):
- `MaxBaseIndex.Mood = 10` (ムード)
- `MaxBaseIndex.Reason = 11` (理性)
- `MaxBaseIndex.Anger = 12` (怒り)
- `MaxBaseIndex.Satisfaction = 13` (満足)

Note: MaxBaseIndex uses correct CSV values. The BaseIndex.Satisfaction bug (残課題) is a separate issue in existing code; MaxBaseIndex in this feature follows Base.csv correctly.

**NowExIndex Well-Known Values** (from ex.csv):
- `NowExIndex.OrgasmC = 0` (Ｃ絶頂)
- `NowExIndex.OrgasmV = 1` (Ｖ絶頂)
- `NowExIndex.OrgasmA = 2` (Ａ絶頂)
- `NowExIndex.OrgasmB = 3` (Ｂ絶頂)
- `NowExIndex.Ejaculation = 11` (射精)

**SourceIndex Well-Known Values** (from source.csv):
- `SourceIndex.PleasureC = 0` (快Ｃ)
- `SourceIndex.PleasureV = 1` (快Ｖ)
- `SourceIndex.PleasureA = 2` (快Ａ)
- `SourceIndex.PleasureB = 3` (快Ｂ)
- `SourceIndex.Love = 10` (情愛)
- `SourceIndex.Achievement = 12` (達成)
- `SourceIndex.Pain = 13` (苦痛)
- `SourceIndex.Fear = 14` (恐怖)
- `SourceIndex.Lust = 15` (欲情)
- `SourceIndex.Obedience = 16` (恭順)
- `SourceIndex.Exposure = 17` (露出)
- `SourceIndex.Submission = 18` (屈従)
- `SourceIndex.Joy = 20` (歓楽)
- `SourceIndex.Conquest = 21` (征服)
- `SourceIndex.Passive = 22` (受動)
- `SourceIndex.Filth = 30` (不潔)
- `SourceIndex.Depression = 31` (鬱屈)
- `SourceIndex.Deviation = 32` (逸脱)
- `SourceIndex.Antipathy = 33` (反感)

**CupIndex Well-Known Values** (uses PALAM indices, from palam.csv):
- `CupIndex.PleasureC = 0` (快Ｃ)
- `CupIndex.PleasureV = 1` (快Ｖ)
- `CupIndex.PleasureA = 2` (快Ａ)
- `CupIndex.PleasureB = 3` (快Ｂ)
- `CupIndex.Obedience = 10` (恭順)
- `CupIndex.Lust = 11` (欲情)
- `CupIndex.Submission = 12` (屈服)
- `CupIndex.Pain = 15` (苦痛)
- `CupIndex.Fear = 16` (恐怖)
- `CupIndex.Antipathy = 30` (反感)
- `CupIndex.Displeasure = 31` (不快)

**AC#11-14 FavorCalculator Method Usage**: These ACs verify that FavorCalculator (Era.Core/Training/FavorCalculator.cs) invokes the new IVariableStore accessors. The Grep should target the implementation file, not comments. Note: GetCup is NOT used by FavorCalculator - it is provided for F395 MarkSystem (TRACHECK.ERB lines 619, 656, 686, 748, 804).

**AC#15 Dependency Injection**: FavorCalculator constructor must accept IVariableStore via DI to access the new array methods. The existing FavorCalculator is stateless; this change requires:
1. Add `IVariableStore _variables` field to FavorCalculator
2. Update constructor: `public FavorCalculator(IVariableStore variables)`
3. DI registration already exists in F393; no change needed for ServiceCollection

**IFavorCalculator Signature Change** (BREAKING CHANGE): The interface method signature changes from:
- Old: `int CalculateFavor(CharacterId target)`
- New: `int CalculateFavor(CharacterId target, CharacterId player, int selectCom, int assiPlay)`

This allows PLAYER NOWEX access and SELECTCOM/ASSIPLAY values required by FAVOR_CALC logic. All callers of IFavorCalculator.CalculateFavor must be updated to provide the additional parameters.

**AC#18 Test Location**: New FavorCalculator behavior tests should be added to Era.Core.Tests/FavorCalculatorTests.cs (flat structure matching existing TrainingProcessorTests.cs pattern).

**AC#18 Test Coverage** (implied by implementation):
- SOURCE values contribute to favor calculation
- MARK:反発刻印 reduces favor
- NOWEX orgasm values contribute via satisfaction
- MAXBASE limits affect calculation boundaries

**LOCAL Variables**: FAVOR_CALC uses LOCAL extensively for intermediate calculations. In C# implementation, these are method-local variables within CalculateFavor - they do not require IVariableStore access.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create SourceIndex type | [x] |
| 2 | 2 | Create MarkIndex type | [x] |
| 3 | 3 | Create NowExIndex type | [x] |
| 4 | 4 | Create MaxBaseIndex type | [x] |
| 5 | 5 | Create CupIndex type | [x] |
| 6 | 6-10 | Add Get/Set methods to IVariableStore interface | [x] |
| 7 | 6-10 | Implement methods in CharacterVariables and VariableStore | [x] |
| 8 | 15 | Add IVariableStore dependency to FavorCalculator constructor | [x] |
| 9 | 16 | Update IFavorCalculator signature and callers: add player, selectCom, assiPlay parameters | [x] |
| 10 | 11-14 | Implement FavorCalculator logic using new accessors (replace STUB, excluding MASTER_FAVOR_CHECK2 which remains ERB) | [x] |
| 11 | 17 | Verify C# build succeeds | [x] |
| 12 | 18 | Add/update FavorCalculator tests | [x] |

**Note**: Task#6-7 batch AC#6-10 (5 symmetric Get methods + implementations) intentionally. Task#10 batches AC#11-14 (4 FavorCalculator usages) for the same reason.

**Scope Boundary**: Task#10 implements the C#-portable portion of FAVOR_CALC logic:
- **Included**: Favor modifier calculation using SOURCE, MARK, NOWEX, MAXBASE values (read-only access)
- **Excluded**: State modifications (BASE:満足/ムード/理性/怒り, CFLAG:怒り, FLAG, TALENT:傷心) remain in ERB wrapper
- **Excluded**: MASTER_FAVOR_CHECK2 (ERB legacy interop) remains an ERB call

FavorCalculator.CalculateFavor returns the calculated favor delta value. The ERB caller (FAVOR_CALC or wrapper) is responsible for applying state modifications based on this value.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F393 | Created the STUB FavorCalculator that this feature completes |
| Successor | F395 | Mark System consumes MARK accessors from this feature |

---

## Links

- [feature-393.md](feature-393.md) - Parent feature (created STUB)
- [feature-395.md](feature-395.md) - Mark System (related MARK usage)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 6 definition

---

## 残課題

| Issue | Description | Tracked In |
|-------|-------------|------------|
| BaseIndex.Satisfaction 誤り | BaseIndex.Satisfaction = 12 は誤り。Base.csv では 満足=13, 怒り=12。既存バグ要修正 | Phase 29 Task 6 |

**→ Phase 29 Task 6 として追跡**: [full-csharp-architecture.md](designs/full-csharp-architecture.md) Phase 29: Validation

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-08 | create | opus | Follow-up from F393 残課題 | PROPOSED |
| 2026-01-08 07:32 | START | implementer | Task 1-5 | - |
| 2026-01-08 07:32 | END | implementer | Task 1-5 | SUCCESS |
| 2026-01-08 07:40 | START | implementer | Task 8 | - |
| 2026-01-08 07:40 | END | implementer | Task 8 | SUCCESS |
| 2026-01-08 07:41 | START | implementer | Task 6 | - |
| 2026-01-08 07:41 | END | implementer | Task 6 | SUCCESS |
| 2026-01-08 07:42 | START | implementer | Task 7 | - |
| 2026-01-08 07:42 | END | implementer | Task 7 | SUCCESS |
| 2026-01-08 07:43 | START | implementer | Task 9 | - |
| 2026-01-08 07:43 | END | implementer | Task 9 | SUCCESS |
| 2026-01-08 07:44 | START | implementer | Task 10 | - |
| 2026-01-08 07:44 | END | implementer | Task 10 | SUCCESS |
| 2026-01-08 07:45 | START | opus | Task 11 (build verify) | - |
| 2026-01-08 07:45 | END | opus | Task 11 (build verify) | SUCCESS |
| 2026-01-08 07:45 | START | opus | Task 12 (test verify) | - |
| 2026-01-08 07:45 | END | opus | Task 12 (test verify) | SUCCESS: 22/22 passed |
| 2026-01-08 07:54 | START | implementer | AC#13-14 fix | - |
| 2026-01-08 07:54 | END | implementer | AC#13-14 fix | SUCCESS |
