# Feature 400: Phase 6 Training Cleanup Migration (AFTERTRA.ERB)

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

Migrate AFTERTRA.ERB (JUEL_CHECK/DENIAL_CHECK, ~138 lines) to C# Era.Core. Requires IVariableStore extensions for JUEL/GOTJUEL/PALAMLV array access. Completes F394's AfterTraining stub implementation.

---

## Background

### Philosophy (Mid-term Vision)

**Full C# Architecture**: Progressive migration from ERB to C# for maintainability, type safety, and testability.

### Problem (Current Issue)

F394 narrowed scope to BEFORETRAIN.ERB only, deferring AFTERTRA.ERB because:
- IVariableStore lacks JUEL/GOTJUEL/PALAMLV accessors
- F399 does not cover these variable types
- JUEL_CHECK/DENIAL_CHECK logic (~100 lines) requires these accessors

AFTERTRA.ERB contains:
- @JUEL_CHECK (lines 37-96): PALAM-to-JUEL conversion logic
- @DENIAL_CHECK (lines 98-138): Juel consumption for denial training

### Goal (What to Achieve)

1. Extend IVariableStore with JUEL/GOTJUEL/PALAMLV accessors
2. Implement JUEL_CHECK algorithm in C#
3. Implement DENIAL_CHECK algorithm in C#
4. Replace F394's AfterTraining stub with actual implementation
5. Category=Training unit tests verifying legacy behavior equivalence

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IVariableStore GetJuel method exists | file | Grep("GetJuel", Era.Core/Interfaces/IVariableStore.cs) | count_gte | 1 | [x] |
| 2 | IVariableStore SetJuel method exists | file | Grep("SetJuel", Era.Core/Interfaces/IVariableStore.cs) | count_gte | 1 | [x] |
| 3 | IVariableStore GetGotJuel method exists | file | Grep("GetGotJuel", Era.Core/Interfaces/IVariableStore.cs) | count_gte | 1 | [x] |
| 4 | IVariableStore SetGotJuel method exists | file | Grep("SetGotJuel", Era.Core/Interfaces/IVariableStore.cs) | count_gte | 1 | [x] |
| 5 | IVariableStore GetPalamLv method exists | file | Grep("GetPalamLv", Era.Core/Interfaces/IVariableStore.cs) | count_gte | 1 | [x] |
| 6 | IJuelProcessor interface exists | file | Glob | exists | Era.Core/Training/IJuelProcessor.cs | [x] |
| 7 | JuelProcessor implementation exists | file | Glob | exists | Era.Core/Training/JuelProcessor.cs | [x] |
| 8 | JuelCheck method exists | file | Grep("JuelCheck", Era.Core/Training/JuelProcessor.cs) | count_gte | 1 | [x] |
| 9 | DenialCheck method exists | file | Grep("DenialCheck", Era.Core/Training/JuelProcessor.cs) | count_gte | 1 | [x] |
| 10 | IJuelProcessor DI registration | code | Grep(Era.Core/DependencyInjection/) | contains | IJuelProcessor | [x] |
| 11 | C# build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 12 | JuelProcessor unit tests exist | file | Glob | exists | Era.Core.Tests/JuelProcessorTests.cs | [x] |
| 13 | JuelCheck test exists | file | Grep("JuelCheck_", Era.Core.Tests/JuelProcessorTests.cs) | count_gte | 1 | [x] |
| 14 | DenialCheck test exists | file | Grep("DenialCheck_", Era.Core.Tests/JuelProcessorTests.cs) | count_gte | 1 | [x] |
| 15 | All training tests pass | test | dotnet test --filter Category=Training | succeeds | - | [x] |
| 16 | Invalid character returns failure (Neg) | test | dotnet test --filter Category=Training | contains | InvalidCharacter_ShouldReturnFailure | [x] |
| 17 | Null parameter throws exception (Neg) | test | dotnet test --filter Category=Training | contains | NullParameter_ShouldThrow | [x] |

### AC Details

**Method Signatures**:
- `IVariableStore.GetJuel(CharacterId character, int index) -> Result<int>`
- `IVariableStore.SetJuel(CharacterId character, int index, int value) -> void`
- `IVariableStore.GetGotJuel(CharacterId character, int index) -> Result<int>`
- `IVariableStore.SetGotJuel(CharacterId character, int index, int value) -> void`
- `IVariableStore.GetPalamLv(int index) -> Result<int>` (global, not per-character; uses Result<T> for consistency with F399 pattern)
- `IJuelProcessor.JuelCheck(CharacterId target) -> void` - PALAM-to-JUEL conversion
- `IJuelProcessor.DenialCheck(CharacterId target) -> void` - Juel consumption for denial

**AC#1-5**: IVariableStore extensions for JUEL/GOTJUEL/PALAMLV array access
**AC#6-9**: JuelProcessor interface and implementation (JUEL_CHECK + DENIAL_CHECK)
**AC#10**: DI registration per F377 container pattern
**AC#11**: Build verification
**AC#12-15**: TDD verification with legacy behavior equivalence

**F394 AfterTraining Integration**: After F400 is complete, F394's AfterTraining stub should call IJuelProcessor.JuelCheck() and IJuelProcessor.DenialCheck(). This integration can be done as part of F400 or as a minor update to F394.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-2 | Add GetJuel/SetJuel to IVariableStore and implementations | [x] |
| 2 | 3-4 | Add GetGotJuel/SetGotJuel to IVariableStore and implementations | [x] |
| 3 | 5 | Add GetPalamLv to IVariableStore and implementations | [x] |
| 4 | 6 | Create IJuelProcessor interface | [x] |
| 5 | 7 | Create JuelProcessor class skeleton | [x] |
| 6 | 8 | Implement JuelCheck method (AFTERTRA.ERB lines 37-96) | [x] |
| 7 | 9 | Implement DenialCheck method (AFTERTRA.ERB lines 98-138) | [x] |
| 8 | 10 | Register IJuelProcessor in DI container | [x] |
| 9 | 11 | Verify C# build succeeds | [x] |
| 10 | 12 | Create JuelProcessorTests.cs test file | [x] |
| 11 | 13 | Create JuelCheck unit tests | [x] |
| 12 | 14 | Create DenialCheck unit tests | [x] |
| 13 | 15 | Verify all training tests pass | [x] |
| 14 | 16 | Create InvalidCharacter_ShouldReturnFailure negative test | [x] |
| 15 | 17 | Create NullParameter_ShouldThrow negative test | [x] |

<!-- AC:Task 1:1 Rule Note: Tasks 1-2 batch AC#1-2 and AC#3-4 respectively (symmetric Get/Set pairs) - intentional batching per F399 precedent -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F394 | Parent feature (created this as follow-up from 残課題) |
| Predecessor | F393 | Training subsystem patterns (ITrainingProcessor, Result types) |
| Predecessor | F399 | IVariableStore extension pattern (SOURCE/MARK/NOWEX/MAXBASE/CUP methods) |
| Interface | IVariableStore | Extends with JUEL/GOTJUEL/PALAMLV accessors |

---

## Deliverables

| File | Purpose |
|------|---------|
| Era.Core/Interfaces/IVariableStore.cs | Extended with JUEL/GOTJUEL/PALAMLV methods |
| Era.Core/Variables/CharacterVariables.cs | JUEL/GOTJUEL implementation |
| Era.Core/Variables/VariableStore.cs | PALAMLV implementation |
| Era.Core/Training/IJuelProcessor.cs | Juel processor interface |
| Era.Core/Training/JuelProcessor.cs | JUEL_CHECK/DENIAL_CHECK implementation |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | DI registration (modified) |
| Era.Core.Tests/JuelProcessorTests.cs | Unit tests |

---

## Links

- [feature-377.md](feature-377.md) - F377 Phase 4 Architecture Refactoring (DI container pattern)
- [feature-393.md](feature-393.md) - F393 Training Processing Core Migration (patterns)
- [feature-394.md](feature-394.md) - Parent feature (AfterTraining stub)
- [feature-399.md](feature-399.md) - F399 IVariableStore Extensions (similar scope)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 6 definition

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-08 | create | opus | Follow-up from F394 残課題 (AFTERTRA.ERB) | PROPOSED |
| 2026-01-08 11:52 | START | implementer | Task 1-3 | - |
| 2026-01-08 11:52 | END | implementer | Task 1-3 | SUCCESS |
| 2026-01-08 11:56 | START | implementer | Task 4-7 | - |
| 2026-01-08 11:56 | END | implementer | Task 4-7 | SUCCESS |
| 2026-01-08 11:58 | START | implementer | Task 8 | - |
| 2026-01-08 11:58 | END | implementer | Task 8 (DI registration) | SUCCESS |
| 2026-01-08 11:59 | START | opus | Task 9 | - |
| 2026-01-08 11:59 | END | opus | Task 9 (Build verification) | SUCCESS |
| 2026-01-08 12:00 | START | implementer | Task 10-15 | - |
| 2026-01-08 12:02 | END | implementer | Task 10-15 | SUCCESS |
| 2026-01-08 12:05 | START | feature-reviewer | Post-review | - |
| 2026-01-08 12:05 | END | feature-reviewer | Post-review | NEEDS_REVISION |
| 2026-01-08 12:06 | START | opus | SSOT update (engine-dev SKILL.md) | - |
| 2026-01-08 12:06 | END | opus | SSOT update (engine-dev SKILL.md) | SUCCESS |
