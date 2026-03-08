# Feature 856: NTR Core/Util Foundation

## Status: [DRAFT]

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

## Type: erb

<!-- Architecture Task: NTR Subsystem Migration -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Core/Util Foundation. Each completed phase triggers planning for the next phase, producing sub-feature DRAFTs that are the SSOT for the next phase's scope.

### Problem (Current Issue)

NTR_UTIL.ERB provides foundational utility functions (NTR_COUNTER, NTR_CHK_FAVORABLY, NTR_RESET_VISITOR_ACTION, NTR_PUNISHMENT, NTR_ADD_SURRENDER) called by nearly every NTR file. These must be migrated to C# before dependent systems (F857 NTR Behavioral, F858 NTR Master Scenes, F859 NTR Extended, F860 NTR Message) can proceed. Additionally, the Phase 24 domain model (NtrParameters, Susceptibility) requires expansion with mutation methods, and several deferred obligations from F852 and F829 must be resolved at this foundation layer.

### Goal (What to Achieve)

Migrate NTR_UTIL.ERB (1,500 lines), NTR_ACTION.ERB (406 lines), NTR.ERB (251 lines), NTR_OPTION.ERB (504 lines) to C# in `src/Era.Core/NTR/`. Produce:
- NtrEngine.cs (system integration, orchestrator)
- NtrActionProcessor.cs (action primitives: NTR_KISS, NTR_PET, NTR_BUST_PET, NTR_FELLATIO)
- NtrUtilService.cs (utility functions)
- NtrOptions.cs (configuration)

Implement INtrCalculator concrete methods (CanAdvance, CanChangeRoute) consuming NTR_UTIL.ERB logic. Expand NtrParameters/Susceptibility with mutation methods (F852 obligation). Add ExposureDegree Value Object (F852). Wire NtrProgressionCreated domain event (F852). Declare CalculateOrgasmCoefficient method (F852/F406). Implement OB-03 CLOTHES_ACCESSORY/INtrQuery wiring, OB-04 IVariableStore 2D SAVEDATA stubs, OB-05 NullMultipleBirthService runtime impl, OB-10 CP-2 behavioral flow test.

### Predecessor Obligations (for Mandatory Handoffs)

| Source Feature | Obligation | Category | Status |
|:--------------:|------------|----------|:------:|
| F852 | NtrParameters/Susceptibility mutation methods | deferred | pending |
| F852 | ExposureDegree Value Object | deferred | pending |
| F852 | NtrProgressionCreated domain event | deferred | pending |
| F852 | CalculateOrgasmCoefficient method declaration | deferred | pending |
| F829 | OB-03: CLOTHES_ACCESSORY/INtrQuery wiring | deferred | pending |
| F829 | OB-04: IVariableStore 2D SAVEDATA stubs | deferred | pending |
| F829 | OB-05: NullMultipleBirthService runtime impl | deferred | pending |
| F829 | OB-10: CP-2 behavioral flow test | deferred | pending |
| F852 | 26 pre-existing ComEquivalence test failures (missing game YAML/config fixtures) | out-of-scope | pending |

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F855 | [DONE] | Phase 25 Planning must complete first |
| Successor | F857 | [DRAFT] | NTR Behavioral Systems depends on NTR Core/Util |
| Successor | F859 | [DRAFT] | NTR Extended Systems depends on NTR Core/Util |
| Successor | F860 | [DRAFT] | NTR Message Generator depends on NTR Core/Util |

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Debt cleanup: no TODO/FIXME/HACK in migrated code | file | Grep(src/Era.Core/NTR/, pattern="TODO\|FIXME\|HACK") | not_matches | - | [ ] |
| 2 | Legacy ERB vs C# equivalence tests pass | test | dotnet test --filter "Category=Equivalence" | pass | - | [ ] |
| 3 | Zero-debt: no TODO/FIXME/HACK in test code | file | Grep(src/tools/dotnet/**/NTR*, pattern="TODO\|FIXME\|HACK") | not_matches | - | [ ] |

## Tasks

_To be completed by /fc_

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| 26 pre-existing ComEquivalence test failures (missing game YAML/config fixtures) | PRE-EXISTING: out-of-scope for NTR domain migration; root cause is missing test fixtures | Dedicated fixture-repair feature | - | Create during /run if failures block test execution | | |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

## Links

[Predecessor: F855](feature-855.md) - Phase 25 Planning
[Related: F852](feature-852.md) - Source of domain model obligations
[Related: F829](feature-829.md) - Source of OB-03/04/05/10
[Related: F406](feature-406.md) - CalculateOrgasmCoefficient stub origin
