# Feature 393: Phase 6 Training Processing Core Migration

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

Migrate training processing core (TRACHECK_*.ERB [4 files], basic checks, FavorCalculator) to C# Era.Core with ITrainingProcessor interface.

---

## Background

### Philosophy (Mid-term Vision)

**Full C# Architecture**: Progressive migration from ERB to C# for maintainability, type safety, and testability.

### Problem (Current Issue)

Training processing logic is scattered across multiple ERB files (actual: ~1,113 lines total):
- TRACHECK_ABLUP.ERB: Ability growth processing (~103 lines)
- TRACHECK_EQUIP.ERB: Equipment processing (~225 lines: EQUIP_CHECK lines 5-92 + EQUIP_INCERT_* lines 95-224)
- TRACHECK_ORGASM.ERB: Orgasm state processing (~515 lines)
- Basic checks (MOOD/REASON/TECHNIQUE/TIME): ~70 lines from TRACHECK.ERB (lines 4-70)
- FavorCalculator (FAVOR_CALC): ~200 lines from TRACHECK.ERB (lines 951-1149, excludes MASTER_FAVOR_CHECK2 at lines 1223-1238; FavorCalculator will call legacy ERB for MASTER_FAVOR_CHECK2)

Scope excludes: Mark System (~290 lines, lines 566-853) and Character State Tracking (~240 lines, other sections).

This makes processing logic hard to test, extend, and maintain.

### Goal (What to Achieve)

Migrate training processing core to C# Era.Core with:
- ITrainingProcessor interface for processing operations
- Result<TrainingResult> return type for error handling
- Cohesive processing subsystem: TRACHECK_*.ERB (4 files) + basic checks + FavorCalculator
- Category=Training unit tests verifying legacy behavior equivalence

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CommandId type exists | file | Glob | exists | Era.Core/Types/CommandId.cs | [x] |
| 2 | ITrainingProcessor interface exists | file | Glob | exists | Era.Core/Training/ITrainingProcessor.cs | [x] |
| 3 | TrainingProcessor implementation exists | file | Glob | exists | Era.Core/Training/TrainingProcessor.cs | [x] |
| 4 | TrainingResult type exists | file | Glob | exists | Era.Core/Training/TrainingResult.cs | [x] |
| 4a | TrainingResult contains CharacterId field | code | Grep | contains | CharacterId | [x] |
| 4b | TrainingResult contains Changes field | code | Grep | contains | Changes | [x] |
| 5 | IAbilityGrowthProcessor interface exists | file | Glob | exists | Era.Core/Training/IAbilityGrowthProcessor.cs | [x] |
| 6 | AbilityGrowthProcessor implementation exists | file | Glob | exists | Era.Core/Training/AbilityGrowthProcessor.cs | [x] |
| 7 | IEquipmentProcessor interface exists | file | Glob | exists | Era.Core/Training/IEquipmentProcessor.cs | [x] |
| 8 | EquipmentProcessor implementation exists | file | Glob | exists | Era.Core/Training/EquipmentProcessor.cs | [x] |
| 9 | IOrgasmProcessor interface exists | file | Glob | exists | Era.Core/Training/IOrgasmProcessor.cs | [x] |
| 10 | OrgasmProcessor implementation exists | file | Glob | exists | Era.Core/Training/OrgasmProcessor.cs | [x] |
| 11 | IFavorCalculator interface exists | file | Glob | exists | Era.Core/Training/IFavorCalculator.cs | [x] |
| 12 | FavorCalculator implementation exists | file | Glob | exists | Era.Core/Training/FavorCalculator.cs | [x] |
| 12a | IBasicChecksProcessor interface exists | file | Glob | exists | Era.Core/Training/IBasicChecksProcessor.cs | [x] |
| 12b | BasicChecksProcessor implementation exists | file | Glob | exists | Era.Core/Training/BasicChecksProcessor.cs | [x] |
| 13 | ITrainingProcessor DI registration | code | Grep(Era.Core/DependencyInjection/) | contains | ITrainingProcessor | [x] |
| 14 | IAbilityGrowthProcessor DI registration | code | Grep(Era.Core/DependencyInjection/) | contains | IAbilityGrowthProcessor | [x] |
| 15 | IEquipmentProcessor DI registration | code | Grep(Era.Core/DependencyInjection/) | contains | IEquipmentProcessor | [x] |
| 16 | IOrgasmProcessor DI registration | code | Grep(Era.Core/DependencyInjection/) | contains | IOrgasmProcessor | [x] |
| 17 | IFavorCalculator DI registration | code | Grep(Era.Core/DependencyInjection/) | contains | IFavorCalculator | [x] |
| 17a | IBasicChecksProcessor DI registration | code | Grep(Era.Core/DependencyInjection/) | contains | IBasicChecksProcessor | [x] |
| 18 | C# build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 19 | Era.Core.Tests build succeeds | build | dotnet build Era.Core.Tests | succeeds | - | [x] |
| 20 | Training unit tests exist | code | Grep(Category=Training, Era.Core.Tests/) | gte | 6 | [x] |
| 21 | All training processing tests pass | test | dotnet test --filter Category=Training | succeeds | - | [x] |
| 22 | Invalid command returns failure (Neg) | test | dotnet test --filter Category=Training | contains | InvalidCommand_ShouldReturnFailure | [x] |
| 23 | Null parameter throws exception (Neg) | test | dotnet test --filter Category=Training | contains | NullParameter_ShouldThrow | [x] |

### AC Details

**Method Signatures**:
- `ITrainingProcessor.Process(CharacterId target, CommandId command) -> Result<TrainingResult>`
- `IAbilityGrowthProcessor.ProcessGrowth(CharacterId target) -> Result<GrowthResult>` (consistent with F392 error handling pattern)
- `IEquipmentProcessor.ProcessEquipment(CharacterId target) -> EquipmentResult`
- `IOrgasmProcessor.ProcessOrgasm(CharacterId target, CharacterId trainer) -> OrgasmResult`
- `IFavorCalculator.CalculateFavor(CharacterId target) -> int`
- `IBasicChecksProcessor.GetTimeModifier(CommandId command) -> int`
- `IBasicChecksProcessor.GetFavorModifier(CharacterId target, int commandType) -> int`
- `IBasicChecksProcessor.GetTechniqueModifier(CharacterId actor, int commandType) -> int` (actor is trainer)
- `IBasicChecksProcessor.GetMoodModifier(CharacterId target, int commandType) -> int`
- `IBasicChecksProcessor.GetReasonModifier(CharacterId target, int commandType) -> int`
- CommandId: Strongly typed ID for training commands (created in this feature, following CharacterId pattern; originally deferred to Phase 8 in F377 but required here for ITrainingProcessor signature)
- commandType: Integer category used by ERB (0-3=normal, 10=assist, 30-33=resist, etc.) - matches ERB ARG:1

**Result Composition**: TrainingProcessor composes sub-processor results into TrainingResult.Changes collection. Sub-processors return domain-specific types; TrainingProcessor aggregates them into a unified result.

**AC#1**: CommandId type creation (prerequisite for interface)
**AC#2-4**: Core training processor components
**AC#5-12**: Sub-processor interfaces and implementations (DI-managed for testability)
- IAbilityGrowthProcessor: Ability growth processing (TRACHECK_ABLUP.ERB)
- IEquipmentProcessor: Equipment effect processing (TRACHECK_EQUIP.ERB)
- IOrgasmProcessor: Orgasm state processing (TRACHECK_ORGASM.ERB)
- IFavorCalculator: Favor calculation (TRACHECK.ERB FAVOR_CALC)
- IBasicChecksProcessor: Basic modifier checks (TRACHECK.ERB lines 4-70)

**AC#13-17**: DI registrations per F377 container pattern
**AC#18-19**: Build verification
**AC#20-21**: TDD verification with legacy behavior equivalence
- Test breakdown: TrainingProcessor (1), AbilityGrowthProcessor (1), EquipmentProcessor (1), OrgasmProcessor (1), FavorCalculator (1), BasicChecks integration (1) = 6 minimum
- AC#20 counts test methods with `[Category("Training")]` attribute in Era.Core.Tests/

**AC#22-23**: Negative test cases (error handling verification)
- Test naming convention: `{MethodName}_{Scenario}_Should{ExpectedBehavior}` (standard .NET test naming)

**Dependencies**:
- TrainingProcessor depends on IAbilitySystem (from F392) for ability/talent queries
- Sub-processors are injected into TrainingProcessor via DI

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create CommandId strongly typed ID (Era.Core/Types/) | [x] |
| 2 | 4 | Define TrainingResult type (needed by ITrainingProcessor) | [x] |
| 3 | 2 | Define ITrainingProcessor interface | [x] |
| 4 | 3 | Implement TrainingProcessor (orchestrates sub-processors) | [x] |
| 4a | 12a-12b | Define IBasicChecksProcessor and implement BasicChecksProcessor (TIME/FAVOR/TECHNIQUE/MOOD/REASON modifier functions from TRACHECK.ERB lines 4-70) | [x] |
| 5 | 5-6 | Define IAbilityGrowthProcessor and implement AbilityGrowthProcessor (TRACHECK_ABLUP.ERB logic) | [x] |
| 6 | 7-8 | Define IEquipmentProcessor and implement EquipmentProcessor (TRACHECK_EQUIP.ERB logic) | [x] |
| 7 | 9-10 | Define IOrgasmProcessor and implement OrgasmProcessor (TRACHECK_ORGASM.ERB logic) | [x] |
| 8 | 11-12 | Define IFavorCalculator and implement FavorCalculator (FAVOR_CALC from TRACHECK.ERB lines 951-1149, excludes MASTER_FAVOR_CHECK2) | [x] |
| 9 | 13-17 | Register all processors in DI container | [x] |
| 10 | 18-19 | Verify C# builds (Era.Core and Era.Core.Tests) | [x] |
| 11 | 20 | Create unit tests for training processing (Category=Training, min 6 tests) | [x] |
| 12 | 21 | Verify all training processing tests pass | [x] |
| 13 | 22-23 | Create negative tests (InvalidCommand, NullParameter) | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F390 | Phase 6 planning feature |
| Predecessor | F392 | Ability system (validation depends on ability queries) - **DONE** |
| Successor | F394 | Training lifecycle uses validation results |

---

## Deliverables

| File | Purpose |
|------|---------|
| Era.Core/Types/CommandId.cs | Strongly typed command ID |
| Era.Core/Training/ITrainingProcessor.cs | Training processor interface |
| Era.Core/Training/TrainingProcessor.cs | Training processor implementation (basic checks) |
| Era.Core/Training/TrainingResult.cs | Processing result type |
| Era.Core/Training/IAbilityGrowthProcessor.cs | Ability growth processor interface |
| Era.Core/Training/AbilityGrowthProcessor.cs | Ability growth processing (TRACHECK_ABLUP.ERB) |
| Era.Core/Training/IEquipmentProcessor.cs | Equipment processor interface |
| Era.Core/Training/EquipmentProcessor.cs | Equipment effect processing (TRACHECK_EQUIP.ERB) |
| Era.Core/Training/IOrgasmProcessor.cs | Orgasm processor interface |
| Era.Core/Training/OrgasmProcessor.cs | Orgasm state processing (TRACHECK_ORGASM.ERB) |
| Era.Core/Training/IFavorCalculator.cs | Favor calculator interface |
| Era.Core/Training/FavorCalculator.cs | Favor calculation (TRACHECK.ERB FAVOR_CALC) |
| Era.Core/Training/IBasicChecksProcessor.cs | Basic checks processor interface |
| Era.Core/Training/BasicChecksProcessor.cs | Basic modifier checks (TRACHECK.ERB lines 4-70) |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | DI registration (modified) |
| Era.Core.Tests/TrainingProcessorTests.cs | Unit tests for training processing |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 6 definition
- [feature-377.md](feature-377.md) - F377 Design Principles (YAGNI/KISS, Result types)
- [feature-390.md](feature-390.md) - Phase 6 planning
- [feature-392.md](feature-392.md) - F392 Ability System (validation depends on ability queries)
- [feature-394.md](feature-394.md) - F394 Training Lifecycle (uses validation results)
- [feature-395.md](feature-395.md) - F395 Mark System (TRACHECK.ERB lines 566-853)
- [feature-396.md](feature-396.md) - F396 Character State Tracking (TRACHECK.ERB other sections)
- [feature-399.md](feature-399.md) - F399 IVariableStore Extensions (follow-up: FavorCalculator full implementation)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-07 13:30 | create | implementer | Created from F390 Phase 6 planning | PROPOSED |
| 2026-01-08 01:56 | START | implementer | Task 1 | - |
| 2026-01-08 01:56 | END | implementer | Task 1 | SUCCESS |
| 2026-01-08 01:59 | START | implementer | Task 2 | - |
| 2026-01-08 01:59 | END | implementer | Task 2 | SUCCESS |
| 2026-01-08 02:02 | START | implementer | Task 3 | - |
| 2026-01-08 02:02 | END | implementer | Task 3 | SUCCESS |
| 2026-01-08 02:03 | START | implementer | Task 4a | - |
| 2026-01-08 02:08 | END | implementer | Task 4a | BLOCKED:DOC_MISMATCH |
| 2026-01-08 02:14 | START | implementer | Task 4a (retry) | - |
| 2026-01-08 02:14 | END | implementer | Task 4a (retry) | BLOCKED:MISSING_DEPENDENCY |
| 2026-01-08 02:16 | START | implementer | Task 4a (retry 2) | - |
| 2026-01-08 02:22 | END | implementer | Task 4a (retry 2) | SUCCESS |
| 2026-01-08 02:29 | START | implementer | Task 5 | - |
| 2026-01-08 02:29 | END | implementer | Task 5 | SUCCESS |
| 2026-01-08 02:27 | START | implementer | Task 6 | - |
| 2026-01-08 02:27 | END | implementer | Task 6 | SUCCESS |
| 2026-01-08 02:32 | START | implementer | Task 7 | - |
| 2026-01-08 02:32 | END | implementer | Task 7 | SUCCESS |
| 2026-01-08 02:37 | START | implementer | Task 8 | - |
| 2026-01-08 02:37 | END | implementer | Task 8 | SUCCESS |
| 2026-01-08 02:39 | START | implementer | Task 9 | - |
| 2026-01-08 02:39 | END | implementer | Task 9 | SUCCESS |
| 2026-01-08 02:41 | START | implementer | Task 4 | - |
| 2026-01-08 02:41 | END | implementer | Task 4 | SUCCESS |
| 2026-01-08 02:45 | START | verifier | Task 10 (build verification) | - |
| 2026-01-08 02:45 | END | verifier | Task 10 (build verification) | SUCCESS |
| 2026-01-08 02:45 | START | verifier | Task 11 (unit tests exist) | - |
| 2026-01-08 02:45 | END | verifier | Task 11 (unit tests exist) | SUCCESS (21 tests) |
| 2026-01-08 02:46 | START | verifier | Task 12 (tests pass) | - |
| 2026-01-08 02:46 | END | verifier | Task 12 (tests pass) | SUCCESS (21/21 pass) |
| 2026-01-08 02:46 | START | verifier | Task 13 (negative tests) | - |
| 2026-01-08 02:46 | END | verifier | Task 13 (negative tests) | SUCCESS |
| 2026-01-08 02:50 | START | feature-reviewer | Post-review | - |
| 2026-01-08 02:50 | END | feature-reviewer | Post-review | NEEDS_REVISION (minor) |
| 2026-01-08 02:55 | START | feature-reviewer | Doc-check | - |
| 2026-01-08 02:55 | END | feature-reviewer | Doc-check | READY |
| 2026-01-08 03:00 | - | opus | User approval | y |
| 2026-01-08 03:00 | - | opus | Created F399 (follow-up) | PROPOSED |
| 2026-01-08 03:05 | END | finalizer | Finalize | [DONE] |
