# Feature 392: Phase 6 Ability System Core Migration

## Status: [DONE]

## Type: engine

## Created: 2026-01-07

---

## Summary

Migrate ability system core (ABL.ERB, ABL_UP_DATA.ERB, ABLUP.ERB) to C# Era.Core with strongly typed IDs and Result types.

---

## Background

### Philosophy (Mid-term Vision)

**Full C# Architecture**: Progressive migration from ERB to C# for maintainability, type safety, and testability.

### Problem (Current Issue)

Ability system logic is scattered across ERB files (actual: ~2,311 lines total):
- ABL.ERB: Ability level-up UI and JUEL requirement display (~123 lines)
- ABL_UP_DATA.ERB: Growth data definitions (~1,983 lines)
- ABLUP.ERB: ALL COMMENTS - skip migration (~205 lines)

Actual scope: ~2,106 lines (ABL.ERB + ABL_UP_DATA.ERB only).

This makes ability logic hard to test, refactor, and maintain.

### Goal (What to Achieve)

Migrate ability system to C# Era.Core with:
- Strongly typed IDs (reuse existing AbilityIndex, TalentIndex, ExpIndex from Era.Core/Types/)
- IAbilitySystem interface for ability queries and growth application
- Result type for error handling
- Category=Ability unit tests verifying legacy behavior equivalence

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IAbilitySystem interface exists | file | Glob | exists | Era.Core/Ability/IAbilitySystem.cs | [x] |
| 2 | AbilitySystem implementation exists | file | Glob | exists | Era.Core/Ability/AbilitySystem.cs | [x] |
| 3 | AbilityGrowth calculation exists | file | Glob | exists | Era.Core/Ability/AbilityGrowth.cs | [x] |
| 4 | GrowthData definition exists | file | Glob | exists | Era.Core/Ability/GrowthData.cs | [x] |
| 5 | IAbilitySystem DI registration | code | Grep | contains | services.AddSingleton<IAbilitySystem | [x] |
| 6 | C# build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 7 | Era.Core.Tests build succeeds | build | dotnet build Era.Core.Tests | succeeds | - | [x] |
| 8 | Ability unit tests exist | code | Grep(Category=Ability, Era.Core.Tests/) | gte | 1 | [x] |
| 9 | All ability tests pass | test | dotnet test --filter Category=Ability | succeeds | - | [x] |

### AC Details

**Strongly Typed IDs**: F392 uses existing AbilityIndex/TalentIndex/ExpIndex from Era.Core/Types/ (not AbilityId/TalentId/ExperienceId per architecture.md). This is an intentional YAGNI decision - these types already exist and serve the same purpose.

**Method Signatures**:
- `GetAbility(CharacterId character, AbilityIndex ability) -> Result<int>`
- `HasTalent(CharacterId character, TalentIndex talent) -> Result<bool>`
- `ApplyGrowth(CharacterId character, GrowthResult growth) -> void`

**Scope Clarification**: ABL.ERB (~123 lines) contains ability level-up selection/execution logic (SHOW_ABLUP_SELECT iterates abilities and checks JUEL/EXP requirements, USERABLUP executes upgrades). ABL_UP_DATA.ERB (~1,983 lines) contains the actual growth data definitions (JUEL_DEMAND, EXP_DEMAND patterns). This migration focuses on IAbilitySystem interface foundation that wraps both display and data access.

**Growth Data Loading**: GrowthData.cs defines data structures/interfaces only. The actual ABL_UP_DATA.ERB content (1,983 lines of JUEL_DEMAND/EXP_DEMAND patterns) will be loaded at runtime via CSV/configuration loader (similar to F388 pattern). Full data migration is deferred to a separate feature.

**AC#1-4**: Core ability system components per architecture.md deliverables
**AC#5**: DI registration per F377 container pattern. IAbilitySystem is a facade that:
- Delegates to IVariableStore (from F386) for raw ABL/TALENT/EXP array access
- Adds domain-specific logic: HasTalent returns bool (value > 0), ApplyGrowth handles multi-variable growth atomically
- Provides clean ability-focused API without exposing underlying array mechanics
**AC#6-7**: Build verification
**AC#8-9**: TDD verification with legacy behavior equivalence

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Define IAbilitySystem interface with GetAbility, HasTalent, ApplyGrowth methods (uses existing AbilityIndex, TalentIndex, ExpIndex) | [x] |
| 2 | 2 | Implement AbilitySystem class (delegates to IVariableStore) | [x] |
| 3 | 3 | Implement AbilityGrowth calculation logic | [x] |
| 4 | 4 | Implement GrowthData data structures (interface/types only; actual 1,983-line ABL_UP_DATA.ERB content loaded as runtime configuration) | [x] |
| 5 | 5 | Register IAbilitySystem in DI container | [x] |
| 6 | 6-7 | Verify C# builds (Era.Core and Era.Core.Tests) | [x] |
| 7 | 8 | Create unit tests for ability operations (Category=Ability) | [x] |
| 8 | 9 | Verify all ability tests pass | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F390 | Phase 6 planning feature |
| Predecessor | F386 | IVariableStore for ABL/TALENT/EXP array access |
| Predecessor | F388 | Phase 5 variable system (IVariableResolver) |
| Successor | F393 | Training validation depends on ability queries |

---

## Deliverables

| File | Purpose |
|------|---------|
| Era.Core/Ability/IAbilitySystem.cs | Ability system interface |
| Era.Core/Ability/AbilitySystem.cs | Ability system implementation |
| Era.Core/Ability/AbilityGrowth.cs | Growth calculation logic |
| Era.Core/Ability/GrowthData.cs | Growth data definitions |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | DI registration (modified) |
| Era.Core.Tests/AbilitySystemTests.cs | Unit tests for ability operations |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 6 definition
- [feature-377.md](feature-377.md) - F377 Design Principles (YAGNI/KISS, Strongly Typed IDs)
- [feature-384.md](feature-384.md) - Typed IDs (AbilityIndex, TalentIndex, ExpIndex)
- [feature-386.md](feature-386.md) - IVariableStore (Predecessor)
- [feature-388.md](feature-388.md) - Phase 5 variable system (Predecessor)
- [feature-390.md](feature-390.md) - Phase 6 planning
- [feature-393.md](feature-393.md) - Training validation (Successor)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-07 13:30 | create | implementer | Created from F390 Phase 6 planning | PROPOSED |
| 2026-01-07 21:40 | START | /do 392 | Initialize, status REVIEWED→WIP | READY |
| 2026-01-07 21:41 | END | implementer | Tasks 1-5 (IAbilitySystem, AbilitySystem, AbilityGrowth, GrowthData, DI) | SUCCESS |
| 2026-01-07 21:41 | DEVIATION | debugger | Test GetAbility_ValidCharacter_ReturnsSuccess failed | Fixed: added test setup |
| 2026-01-07 21:42 | END | ac-tester | Tasks 6-8 (Build verification, test creation, test pass) | SUCCESS |
| 2026-01-07 21:42 | END | feature-reviewer | Post-review (mode: post) | READY |
| 2026-01-07 21:42 | END | feature-reviewer | Doc-check (mode: doc-check) | NEEDS_REVISION→FIXED |
| 2026-01-07 21:43 | END | finalizer | Status WIP→DONE | SUCCESS |
