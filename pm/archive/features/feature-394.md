# Feature 394: Phase 6 Training Lifecycle Migration

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

Migrate training pre-training lifecycle (BEFORETRAIN.ERB) to C# Era.Core with TrainingSetup class. AFTERTRA.ERB (JUEL_CHECK/DENIAL_CHECK) deferred to future feature pending IVariableStore extensions.

---

## Background

### Philosophy (Mid-term Vision)

**Full C# Architecture**: Progressive migration from ERB to C# for maintainability, type safety, and testability.

### Problem (Current Issue)

Training lifecycle logic is in BEFORETRAIN.ERB (~255 lines, in-scope: ~125 lines):
- BEFORETRAIN.ERB: Pre-training setup, state initialization (BASE/CFLAG/TCVAR reset, stamina recovery, sleep flags)

Note: AFTERTRA.ERB (JUEL_CHECK/DENIAL_CHECK, ~138 lines) deferred - requires IVariableStore JUEL/GOTJUEL/PALAMLV extensions.

This makes lifecycle management hard to test and reason about as a cohesive unit.

### Goal (What to Achieve)

Migrate training lifecycle to C# Era.Core with:
- TrainingSetup class encapsulating pre/post operations
- Clear separation of setup and cleanup responsibilities
- Category=Training unit tests verifying legacy behavior equivalence

**In-Scope** (core lifecycle operations):

From BEFORETRAIN.ERB (@EVENTTRAIN):
- BASE/CFLAG/TCVAR reset operations (orchestration at lines 9-29, helper functions: @BEFORETRAIN_ResetBases lines 136-166, @BEFORETRAIN_ResetTCVars lines 171-183)
- Daily stamina recovery calculation (lines 102-107, MASTER-only: applies only when target == MASTER)
- Sleep flag reset (lines 116, 125: CFLAG:MASTER:睡眠 only, not CLOTHES_SETTING/EQUIP logic)

**Required Variable Accessors** (verified in IVariableStore or F399):
- BASE: GetBase/SetBase (existing)
- CFLAG: GetCharacterFlag/SetCharacterFlag (existing)
- TCVAR: GetTCVar/SetTCVar (existing)
- EXP: GetExp (existing, for TCVAR initialization)
- MAXBASE: GetMaxBase/SetMaxBase (F399 provides)
- TALENT: GetTalent (existing, for 回復速度)

**Out-of-Scope** (separate systems):
- JUEL_CHECK/DENIAL_CHECK (AFTERTRA.ERB lines 37-138) - requires JUEL/GOTJUEL/PALAMLV accessors not in IVariableStore; deferred to future feature (new feature required, F399 does not cover these)
- GOODMORNING scene printing (BEFORETRAIN.ERB lines 188-256, presentation layer)
- CLOTHES_SETTING and EQUIP logic (BEFORETRAIN.ERB lines 117-124, presentation/equipment layer)
- CHARA_MOVEMENT (movement system, separate feature)
- ABLUP (ability growth, covered by F393/F392)
- Character-specific event triggers (Reimu movement, visitor appearance)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ITrainingSetup interface exists | file | Glob | exists | Era.Core/Training/ITrainingSetup.cs | [x] |
| 2 | TrainingSetup class exists | file | Glob | exists | Era.Core/Training/TrainingSetup.cs | [x] |
| 3 | BeforeTraining method exists | file | Grep("void BeforeTraining\\(CharacterId", Era.Core/Training/TrainingSetup.cs) | count_gte | 1 | [x] |
| 4 | AfterTraining stub method exists | file | Grep("void AfterTraining\\(CharacterId", Era.Core/Training/TrainingSetup.cs) | count_gte | 1 | [x] |
| 5 | ITrainingSetup DI registration | file | Grep(Era.Core/DependencyInjection/) | contains | ITrainingSetup | [x] |
| 6 | C# build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 7 | Era.Core.Tests build succeeds | build | dotnet build Era.Core.Tests | succeeds | - | [x] |
| 8 | TrainingSetup unit tests exist | file | Glob | exists | Era.Core.Tests/TrainingSetupTests.cs | [x] |
| 9 | BeforeTraining test exists | file | Grep("BeforeTraining_", Era.Core.Tests/TrainingSetupTests.cs) | count_gte | 1 | [x] |
| 10 | All training lifecycle tests pass | test | dotnet test --filter Category=Training | succeeds | - | [x] |

### AC Details

**Method Signatures** (per F393 pattern):
- `ITrainingSetup.BeforeTraining(CharacterId target) -> void` - called at EVENTTRAIN lifecycle point (day start)
- `ITrainingSetup.AfterTraining(CharacterId target) -> void` - stub for future implementation (JUEL_CHECK/DENIAL_CHECK)
- Note: Lifecycle methods operate on single character; caller iterates over all characters
- MASTER-specific: BeforeTraining internally checks if target == MASTER and applies stamina recovery only for MASTER
- Return type: void (not Result<T>) - lifecycle methods are setup/teardown operations with no meaningful return value; errors handled via exceptions per .NET convention for initialization code

**AC#1-2**: Training lifecycle interface and implementation per F393 pattern
**AC#3**: BeforeTraining method (BEFORETRAIN.ERB logic: reset bases, reset TCVars, stamina recovery, sleep flags)
**AC#4**: AfterTraining stub method - empty method body (no-op), placeholder for future JUEL_CHECK/DENIAL_CHECK implementation
**AC#5**: DI registration per F377 container pattern
**AC#6-7**: Build verification
**AC#8-10**: TDD verification with BeforeTraining tests (AfterTraining tests deferred with implementation)
- Note: AC#10 Category=Training runs F393+F394 tests together (Phase 6 integration verification)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create ITrainingSetup interface | [x] |
| 2 | 2 | Create TrainingSetup class skeleton | [x] |
| 3 | 3 | Implement BeforeTraining method (In-Scope: reset bases, reset TCVars, stamina recovery, sleep flag) | [x] |
| 4 | 4 | Implement AfterTraining stub method (no-op, placeholder) | [x] |
| 5 | 5 | Register ITrainingSetup in DI container | [x] |
| 6 | 6-7 | Verify C# builds (Era.Core and Era.Core.Tests) | [x] |
| 7 | 8 | Create TrainingSetupTests.cs test file | [x] |
| 8 | 9 | Create BeforeTraining unit tests | [x] |
| 9 | 10 | Verify all training lifecycle tests pass | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F390 | Phase 6 planning feature |
| Predecessor | F393 | Same domain (Training subsystem), shared Types (TrainingResult, CharacterId), IVariableStore pattern |
| Predecessor | F399 | Provides MAXBASE accessors required for stamina recovery (lines 102-107) |
| Interface | IVariableStore | BASE/CFLAG/TCVAR/JUEL variable access (GetBase, SetBase, GetTCVar, SetTCVar) |

---

## Deliverables

| File | Purpose |
|------|---------|
| Era.Core/Training/ITrainingSetup.cs | Training lifecycle interface |
| Era.Core/Training/TrainingSetup.cs | Pre/post training lifecycle implementation |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | DI registration (modified) |
| Era.Core.Tests/TrainingSetupTests.cs | Unit tests for training lifecycle |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 6 definition
- [feature-377.md](feature-377.md) - F377 Design Principles (YAGNI/KISS)
- [feature-390.md](feature-390.md) - Phase 6 planning
- [feature-393.md](feature-393.md) - F393 Training Processing Core Migration
- [feature-392.md](feature-392.md) - F392 Ability System (ABLUP reference in Out-of-Scope)
- [feature-399.md](feature-399.md) - F399 IVariableStore Extensions (provides MAXBASE accessors)
- [feature-400.md](feature-400.md) - F400 Training Cleanup Migration (AFTERTRA.ERB follow-up)

---

## Review Notes

- **2026-01-08 FL iter10**: [resolved] JUEL_CHECK/DENIAL_CHECK scope - narrowed to BEFORETRAIN.ERB only (user approved Option A). AFTERTRA.ERB deferred pending IVariableStore JUEL/GOTJUEL/PALAMLV extensions. → **Tracked in F400**

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-07 13:30 | create | implementer | Created from F390 Phase 6 planning | PROPOSED |
| 2026-01-08 10:49 | START | implementer | Task 1-5 | - |
| 2026-01-08 10:49 | END | implementer | Task 1-5 | SUCCESS |
| 2026-01-08 11:15 | START | implementer | Doc update (engine-dev SKILL) | - |
| 2026-01-08 11:15 | END | implementer | Doc update (engine-dev SKILL) | SUCCESS |
