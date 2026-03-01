# Feature 396: Phase 6 Character State Tracking Migration

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

Migrate character state tracking (3 subsystems from TRACHECK.ERB: Virginity Management, Experience Growth, Pain State Checking) to C# Era.Core with ICharacterStateTracker interface.

---

## Background

### Philosophy (Mid-term Vision)

**Full C# Architecture**: Progressive migration from ERB to C# for maintainability, type safety, and testability.

### Problem (Current Issue)

Character state tracking logic is embedded in TRACHECK.ERB (actual: ~290 lines):
- 処女管理 (Virginity management): LOST_VIRGIN* functions (~136 lines, lines 79-214)
- 経験成長 (Experience growth): JUJUN_UP_CHECK/YOKUBO_UP_CHECK (lines 854-871) + EXP_GOT_CHECK (lines 876-946, ~90 lines total)
- 痛み判定 (Pain check): PAIN_CHECK_V/PAIN_CHECK_A (~66 lines, lines 1152-1217)

This makes character state tracking hard to test, extend, and maintain as part of training result processing.

### Goal (What to Achieve)

Migrate character state tracking to C# Era.Core with:
- ICharacterStateTracker interface for state tracking operations
- VirginityState and ExperienceState types for strongly typed state management
- Result<StateChange> return type for error handling
- Character state domain logic extracted from validation code
- Category=CharacterState unit tests verifying legacy behavior equivalence

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | VirginityState type exists | file | Glob | exists | Era.Core/Character/VirginityState.cs | [x] |
| 2 | ExperienceState type exists | file | Glob | exists | Era.Core/Character/ExperienceState.cs | [x] |
| 3 | ICharacterStateTracker interface exists | file | Glob | exists | Era.Core/Character/ICharacterStateTracker.cs | [x] |
| 4 | CharacterStateTracker implementation exists | file | Glob | exists | Era.Core/Character/CharacterStateTracker.cs | [x] |
| 5 | IVirginityManager interface exists | file | Glob | exists | Era.Core/Character/IVirginityManager.cs | [x] |
| 6 | VirginityManager implementation exists | file | Glob | exists | Era.Core/Character/VirginityManager.cs | [x] |
| 7 | IExperienceGrowthCalculator interface exists | file | Glob | exists | Era.Core/Character/IExperienceGrowthCalculator.cs | [x] |
| 8 | ExperienceGrowthCalculator implementation exists | file | Glob | exists | Era.Core/Character/ExperienceGrowthCalculator.cs | [x] |
| 9 | IPainStateChecker interface exists | file | Glob | exists | Era.Core/Character/IPainStateChecker.cs | [x] |
| 10 | PainStateChecker implementation exists | file | Glob | exists | Era.Core/Character/PainStateChecker.cs | [x] |
| 11 | ICharacterStateTracker DI registration | code | Grep("ICharacterStateTracker", Era.Core/) | contains | AddSingleton<ICharacterStateTracker | [x] |
| 12 | C# build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 13 | Era.Core.Tests build succeeds | build | dotnet build Era.Core.Tests | succeeds | - | [x] |
| 14 | Character state unit tests exist | file | Grep("Category=CharacterState", Era.Core.Tests/) | count_gte | 3 | [x] |
| 15 | All character state tests pass | test | dotnet test --filter Category=CharacterState | succeeds | - | [x] |

### AC Details

**AC#1-2**: Strongly typed state types per F377 design principles
**AC#3-4**: ICharacterStateTracker interface and implementation (orchestrator)
**AC#5-10**: Domain-specific interfaces and implementations per F390 analysis. Directory Era.Core/Character/ chosen to separate character state domain from training processing domain (Era.Core/Training/)

**ERB → C# Mapping**:
| ERB Function | C# Class | Lines |
|--------------|----------|-------|
| LOST_VIRGIN, LOST_VIRGIN_A, LOST_VIRGIN_M | VirginityManager | 79-214 |
| JUJUN_UP_CHECK, YOKUBO_UP_CHECK, EXP_GOT_CHECK | ExperienceGrowthCalculator | 854-871, 876-946 |
| PAIN_CHECK_V, PAIN_CHECK_A | PainStateChecker | 1152-1217 |

**Out of Scope** (F390 'その他' listed for F396 but deferred):
- SOURCE_SEX_CHECK (lines 221-518, ~298 lines) - Same-sex preference adjustment, deferred to future feature
- PLAYER_SKILL_CHECK (lines 520-561, ~42 lines) - Player skill effects, deferred to future feature

**Method Signatures**:
- `ICharacterStateTracker.TrackVirginityLoss(CharacterId target, CharacterId partner) -> Result<VirginityChange>`
- `IVirginityManager.CheckLostVirginity(CharacterId target, CharacterId partner, VirginityType type) -> Result<VirginityChange>`
- `IExperienceGrowthCalculator.ProcessGrowth(CharacterId target) -> Result<ExperienceChange>`
- `IPainStateChecker.CheckPain(CharacterId target, PainType type) -> PainModifier`

**Return Types**:
- `PainModifier`: Record type returning pain/resistance multipliers used by TIMES operations in ERB. `public record PainModifier(decimal PainMultiplier, decimal ResistanceMultiplier);`

**AC#11**: DI registration per F377 container pattern (services.AddSingleton<Interface, Implementation>)
**AC#12-13**: Build verification
**AC#14-15**: TDD verification with legacy behavior equivalence

**External Dependencies**:
- **SOURCE**: Character array. Used in LOST_VIRGIN (lines 132-157) and PAIN_CHECK_V/A (lines 1155-1171 for SOURCE:苦痛/反感 modification). Covered by F399 (Optional) or via callback injection
- **CUP**: Session array. Used in EXP_GOT_CHECK (lines 878, 921-945). Not covered by F399. Via callback injection (Func<CupIndex, int>)
- **PALAM**: Character array. Used in PAIN_CHECK_V/A (lines 1169-1216). Already accessible via IVariableStore.GetPalam
- **PALAMLV**: Threshold array (PALAMLV:1-4). Static thresholds for pain level. Define as constants in Era.Core/Types/PalamThresholds.cs
- **EXPLV**: Threshold array (EXPLV:1-5). Static thresholds for experience level. Define as constants in Era.Core/Types/ExperienceThresholds.cs
- **TFLAG**: Turn-temporary flags. Used in EXP_GOT_CHECK (line 891) for service command check (TFLAG:30). Already accessible via IVariableStore.GetTFlag
- **TEQUIP**: Session equipment array. Used in LOST_VIRGIN (lines 86-104, 117-122, 145, 170-175, 182, 185) for determining virginity loss method (バイブ, Ｖセックス, ビデオ撮影). Via callback injection (Func<int, bool>) using raw index - STUB approach until IVariableStore TEQUIP extension added
- **TCVAR**: Turn variables. Used in LOST_VIRGIN (lines 88, 92, 115, 168) for 行為者, 破瓜, 反発刻印取得抑制. Already accessible via IVariableStore.GetTCVar
- **TSTR**: Session strings. Used in LOST_VIRGIN (lines 127, 180) for video recording logs. Via callback injection for string access
- **EXP**: Character experience values. Used in PAIN_CHECK_V/A (lines 1154-1165, 1187-1201) for experience-based pain modifiers. Already accessible via IVariableStore.GetExp

**Recommended Approach**:
- Character/Session arrays (SOURCE, CUP): Callback injection pattern (Func<SourceIndex, int>, Func<CupIndex, int>) per F395
- Threshold arrays (PALAMLV, EXPLV): Define as static constants in Era.Core/Types/ since thresholds are fixed values

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Create VirginityState and ExperienceState types | [x] |
| 2 | 3,4 | Define ICharacterStateTracker interface and implement CharacterStateTracker orchestration | [x] |
| 3 | 5,6 | Define IVirginityManager and implement VirginityManager (LOST_VIRGIN* logic, lines 79-214) | [x] |
| 4 | 7,8 | Define IExperienceGrowthCalculator and implement ExperienceGrowthCalculator (JUJUN_UP/YOKUBO_UP/EXP_GOT logic, lines 854-946) | [x] |
| 5 | 9,10 | Define IPainStateChecker and implement PainStateChecker (PAIN_CHECK* logic, lines 1152-1217) | [x] |
| 6 | 11 | Register ICharacterStateTracker in DI container | [x] |
| 7 | 12,13 | Verify C# builds (Era.Core and Era.Core.Tests) | [x] |
| 8 | 14 | Create unit tests for character state tracking (Category=CharacterState) | [x] |
| 9 | 15 | Verify all character state tests pass | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F390 | Phase 6 planning feature |
| Predecessor | F393 | Training processing infrastructure (IVariableStore, Result<T> pattern) |
| Optional | F399 | IVariableStore extensions (SOURCE access). Callback injection pattern used to avoid blocking |
| Sibling | F395 | Mark System (both derived from TRACHECK.ERB, callback injection pattern reference) |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 6 definition
- [feature-377.md](feature-377.md) - F377 Design Principles (YAGNI/KISS, Strongly Typed IDs)
- [feature-390.md](feature-390.md) - Phase 6 planning
- [feature-393.md](feature-393.md) - F393 Training processing infrastructure (predecessor)
- [feature-395.md](feature-395.md) - F395 Mark System (sibling, callback injection pattern)
- [feature-399.md](feature-399.md) - F399 IVariableStore extensions (predecessor)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-07 22:00 | create | implementer | Created from F390 revision (actual measurements) | PROPOSED |
| 2026-01-08 | FL | feature-reviewer | FL review iteration 1 - External Dependencies additions (TEQUIP, TCVAR, TSTR, EXP) | - |
| 2026-01-08 | FL | feature-reviewer | FL review iteration 2 - AC format (Glob), interface ACs, Execution Log | - |
| 2026-01-08 | FL | feature-reviewer | FL review iteration 3 - PainModifier type, count_gte threshold | - |
| 2026-01-08 | FL | feature-reviewer | FL review iteration 4 - TEQUIP STUB clarification | REVIEWED |
| 2026-01-08 11:17 | START | implementer | Task 4 | - |
| 2026-01-08 11:17 | END | implementer | Task 4 | SUCCESS |
| 2026-01-08 11:21 | START | implementer | Task 5 | - |
| 2026-01-08 11:21 | END | implementer | Task 5 | SUCCESS |
| 2026-01-08 11:40 | START | opus | Phase 4 Tasks 1-3,6-9 | - |
| 2026-01-08 11:40 | END | opus | Phase 4 Tasks 1-3,6-9 | SUCCESS |
