# Feature 851: NtrProgression Aggregate + Domain Events

## Status: [DONE]
<!-- fl-reviewed: 2026-03-07T22:41:16Z -->

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

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

Phase 24: NTR Bounded Context -- Domain Layer (Aggregate). Pipeline Continuity -- the NtrProgression Aggregate is the SSOT for NTR state transitions within the Era.Core domain model. It encapsulates all NTR progression invariants (phase advancement, route changes, exposure tracking, corruption) -- the aggregate provides state mutation methods with basic structural guards (ceiling, idempotency, non-negative) and event publication; complex invariant validation (CanAdvance, CanChangeRoute) is delegated to the Domain Service in F852 -- and raises domain events for downstream consumers. It depends on Value Objects defined in F850 and is consumed by the Domain Service in F852.

### Problem (Current Issue)

The NTR progression system manages state transitions through imperative ERB scripts with no formal Aggregate boundary, because ERB has no type system or encapsulation primitives. FAV level changes, route transitions, and corruption state updates happen through direct variable mutation across multiple ERB files (e.g., `CFLAG` array manipulation in `COM*.ERB`). Since there is no AggregateRoot encapsulating these state transitions, invariants such as "corruption requires Phase 7 completion" or "route change must precede phase advancement" cannot be enforced at the domain layer. The existing `AggregateRoot<TId>` base class (`Era.Core/Domain/AggregateRoot.cs:8-16`) and the `Character` aggregate (`Era.Core/Domain/Aggregates/Character.cs:7-41`) prove the pattern is viable, but no NtrProgression aggregate exists to apply it to NTR state.

### Goal (What to Achieve)

Implement NtrProgression as AggregateRoot with strongly-typed ID and domain event classes in `Era.Core/NTR/Domain/`:
- `NtrProgressionId` -- Strongly-typed aggregate ID (readonly record struct, satisfying `TId : struct` constraint)
- `Aggregates/NtrProgression.cs` -- AggregateRoot encapsulating NTR state (FAV level, route, phase, corruption state) as a character-pair aggregate (TargetCharacter + Visitor)
- `Events/NtrPhaseAdvanced.cs` -- Domain event raised when NTR phase advances
- `Events/NtrRouteChanged.cs` -- Domain event raised when NTR route changes
- `Events/NtrExposureLevelChanged.cs` -- Domain event raised when exposure level changes
- `Events/NtrCorrupted.cs` -- Domain event raised when character enters corrupted state

**DDD Input Reference**: `pm/reference/ntr-ddd-input.md` -- NtrProgression Aggregate section (Validated: universal pattern across all 9 characters + U_general, all sharing FAV/TALENT branching model).

<!-- Architecture Task 2: NtrProgression Aggregate design/implementation -->
<!-- Architecture Task 6: NTR Domain Events definition (PhaseAdvanced, RouteChanged, etc.) -->

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is F851 needed? | NTR state transitions lack formal Aggregate boundary | `docs/architecture/migration/phase-20-27-game-systems.md:567-601` (Phase 24 scope) |
| 2 | Why no Aggregate boundary? | ERB scripts use direct variable mutation across files for FAV/route/phase state | ERB `CFLAG` array manipulation in `COM*.ERB` files |
| 3 | Why direct mutation? | ERB has no type system or encapsulation primitives to enforce invariants | ERB language design limitation |
| 4 | Why migrate now? | Phase 24 DDD design requires typed domain model before Phase 25 NTR implementation | `docs/architecture/migration/phase-20-27-game-systems.md:575` |
| 5 | Why typed model first (Root)? | AggregateRoot boundary + domain events enable event-driven architecture for kojo/UI decoupling and invariant enforcement | `Era.Core/Domain/AggregateRoot.cs:8-16` (existing base class proves pattern) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | NTR state transitions have no formal boundary | ERB lacks type system; no AggregateRoot exists for NTR domain |
| Where | Multiple ERB files (`COM*.ERB`) with direct `CFLAG` mutation | `Era.Core/NTR/Domain/` -- missing Aggregate and Events |
| Fix | Add validation checks in ERB scripts | Implement NtrProgression AggregateRoot with domain events following Character aggregate pattern |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F850 | [DONE] | Predecessor -- NTR UL + Value Objects (NtrPhase, NtrRoute, NtrParameters, Susceptibility) |
| F852 | [DRAFT] | Successor -- NTR Domain Service (INtrCalculator), consumes NtrProgression |
| F853 | [DRAFT] | Successor -- NTR ACL/Query layer, downstream of Aggregate |
| F854 | [DRAFT] | Successor -- NTR Application Service, downstream of Domain Service |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Prerequisite VOs exist | FEASIBLE | F850 [DONE]: NtrPhase, NtrRoute, NtrParameters, Susceptibility all exist and tested |
| Base class exists | FEASIBLE | `AggregateRoot<TId>` at `Era.Core/Domain/AggregateRoot.cs:8-16` |
| Event infrastructure exists | FEASIBLE | `IDomainEvent` at `Era.Core/Domain/Events/IDomainEvent.cs:3-6` |
| Reference pattern exists | FEASIBLE | `Character` aggregate at `Era.Core/Domain/Aggregates/Character.cs:7-41` |
| Architecture guidance exists | FEASIBLE | Reference code at `docs/architecture/migration/phase-20-27-game-systems.md:676-706` |
| No external dependencies | FEASIBLE | Pure domain modeling -- no I/O, no external services |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core NTR Domain | HIGH | New aggregate becomes central NTR state object |
| F852 Domain Service | HIGH | INtrCalculator will operate on NtrProgression aggregate |
| F853 ACL/Query | MEDIUM | ACL layer will wrap NtrProgression for legacy ERB compatibility |
| F854 Application Service | MEDIUM | Application service orchestrates NtrProgression lifecycle |
| Existing code | LOW | No modification to existing types; all new files |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| `TId : struct` constraint on `AggregateRoot<TId>` | `Era.Core/Domain/AggregateRoot.cs:8` | NtrProgressionId must be `readonly record struct` |
| `protected init` on `Id` property | `AggregateRoot<TId>` base class | Factory method must set Id via object initializer, not constructor parameter |
| `TreatWarningsAsErrors` | `Directory.Build.props` | All code must compile warning-free |
| INtrCalculator does not exist | F852 scope | Aggregate methods must NOT accept INtrCalculator parameter |
| NtrProgressionId does not exist | Grep across Era.Core: 0 matches | Must be created as part of this feature |
| Character-pair aggregate pattern | `docs/architecture/migration/phase-20-27-game-systems.md:575` | NtrProgression holds both TargetCharacter and Visitor (CharacterId pair), unlike single-entity Character aggregate |
| NTR events namespace separation | Existing `Era.Core/Domain/Events/` for general events | NTR events live in `Era.Core/NTR/Domain/Events/` (separate namespace) |
| NtrParameters naming collision | `Era.Core/Types/NtrParameters.cs` vs `Era.Core/NTR/Domain/ValueObjects/NtrParameters.cs` | Two types with same simple name in different namespaces; F851 code must use fully-qualified or using alias if both referenced |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| AC#1 glob path mismatch with Aggregates/ convention | HIGH | MEDIUM | AC Design Constraint C1: correct path to `NTR/Domain/Aggregates/NtrProgression.cs` |
| INtrCalculator forward dependency in method signatures | MEDIUM | HIGH | Exclude INtrCalculator from F851 method signatures; F852 provides calculator as separate concern |
| NtrExposureLevelChanged event lacks backing Value Object | MEDIUM | LOW | Document as design decision; event captures FAV-level visibility change, not a separate VO |
| NtrParameters naming collision causes ambiguous reference | LOW | LOW | Use fully-qualified names or using alias; collision is in different namespaces |
| Cross-repo execution: ACs reference Era.Core paths but build/test targets C:\Era\core | MEDIUM | MEDIUM | AC Design Constraint C5: ACs must account for cross-repo path resolution |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| NtrProgression files | `find Era.Core/NTR/Domain/Aggregates/ -name "*.cs" 2>/dev/null | wc -l` | 0 | No aggregate exists yet |
| NTR Domain Event files | `find Era.Core/NTR/Domain/Events/ -name "*.cs" 2>/dev/null | wc -l` | 0 | No NTR-specific events exist yet |
| NtrProgressionId | `grep -r "NtrProgressionId" Era.Core/ | wc -l` | 0 | Type does not exist |

**Baseline File**: `_out/tmp/baseline-851.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | NtrProgression must live in Aggregates/ subfolder | `Era.Core/Domain/Aggregates/Character.cs` convention + `docs/architecture/migration/phase-20-27-game-systems.md:646` | AC#1 glob path must be `NTR/Domain/Aggregates/NtrProgression.cs`, not `NTR/Domain/NtrProgression.cs` |
| C2 | NtrProgressionId must be readonly record struct | `AggregateRoot<TId>` requires `TId : struct` | AC must verify NtrProgressionId exists and is a struct |
| C3 | NtrExposureLevelChanged event has no backing VO | No ExposureDegree VO in F850 deliverables | AC for this event should verify structural existence only; behavioral tests deferred to F852+ |
| C4 | INtrCalculator must NOT appear in F851 | INtrCalculator is F852 scope; does not exist yet | ACs must verify no compile-time dependency on INtrCalculator |
| C5 | Cross-repo: code lives in C:\Era\core | Era.Core is a separate repository | AC paths referencing `src/Era.Core/` must be interpreted relative to core repo; build/test commands target core repo |
| C6 | Character-pair aggregate pattern | Architecture doc line 575, 677 | NtrProgression must hold TargetCharacter and Visitor as CharacterId properties; ACs should verify both properties exist |
| C7 | AggregateRoot inheritance | `Era.Core/Domain/AggregateRoot.cs` | AC must verify NtrProgression inherits AggregateRoot<NtrProgressionId> |
| C8 | Domain event record pattern | `Era.Core/Domain/Events/CharacterCreatedEvent.cs:5-8` | Each event must be a record implementing IDomainEvent with OccurredAt property |

### Constraint Details

**C1: Aggregates/ Subfolder Convention**
- **Source**: Existing `Character.cs` at `Era.Core/Domain/Aggregates/Character.cs` + architecture doc `phase-20-27-game-systems.md:646`
- **Verification**: `ls Era.Core/Domain/Aggregates/Character.cs` confirms convention
- **AC Impact**: AC#1 glob must target `NTR/Domain/Aggregates/` not `NTR/Domain/`

**C2: NtrProgressionId Struct Requirement**
- **Source**: `AggregateRoot<TId> where TId : struct` at `Era.Core/Domain/AggregateRoot.cs:8`
- **Verification**: `grep "where TId : struct" Era.Core/Domain/AggregateRoot.cs`
- **AC Impact**: AC must verify NtrProgressionId is `readonly record struct` with a Value property

**C3: NtrExposureLevelChanged Lacks VO Backing**
- **Source**: F850 deliverables (NtrPhase, NtrRoute, NtrParameters, Susceptibility) -- no ExposureDegree
- **Verification**: `grep -r "ExposureDegree" Era.Core/` returns 0 matches
- **AC Impact**: Event is structurally valid but behavioral semantics are deferred; ac-designer should not require VO-linked test for this event

**C4: INtrCalculator Exclusion**
- **Source**: INtrCalculator is defined in F852's scope; `grep -r "INtrCalculator" Era.Core/` returns 0 matches
- **Verification**: Grep for INtrCalculator in F851 deliverables must return 0
- **AC Impact**: Add negative AC: `not_matches "INtrCalculator"` in NtrProgression.cs

**C5: Cross-Repo Path Resolution**
- **Source**: Era.Core lives at `C:\Era\core`, not `C:\Era\devkit`
- **Verification**: `ls C:\Era\core\Era.Core\Domain\AggregateRoot.cs`
- **AC Impact**: All Glob/Grep paths must be resolved against core repo root; build command is `dotnet build` in core repo

**C6: Character-Pair Aggregate**
- **Source**: `docs/architecture/migration/phase-20-27-game-systems.md:575` -- "Aggregate is character x visitor pair"
- **Verification**: Architecture doc reference code shows `TargetCharacter` and `Visitor` properties
- **AC Impact**: AC should verify both `CharacterId` properties exist in NtrProgression

**C7: AggregateRoot Inheritance**
- **Source**: `Era.Core/Domain/AggregateRoot.cs:8-16`
- **Verification**: `grep "AggregateRoot" Era.Core/Domain/Aggregates/Character.cs` confirms pattern
- **AC Impact**: AC must grep for `AggregateRoot<NtrProgressionId>` in NtrProgression.cs

**C8: Domain Event Record Pattern**
- **Source**: `Era.Core/Domain/Events/CharacterCreatedEvent.cs:5-8`
- **Verification**: `grep "IDomainEvent" Era.Core/Domain/Events/CharacterCreatedEvent.cs`
- **AC Impact**: Each of 4 events must match `record.*IDomainEvent` pattern
- **Collection Members** (MANDATORY): The 4 domain events are:
  1. `NtrPhaseAdvanced` -- phase transition event
  2. `NtrRouteChanged` -- route change event
  3. `NtrExposureLevelChanged` -- exposure level change event
  4. `NtrCorrupted` -- corruption state event

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F850 | [DONE] | NTR UL + Value Objects (NtrPhase, NtrRoute, NtrParameters, Susceptibility) |
| Successor | F852 | [DRAFT] | NTR Domain Service (INtrCalculator) consumes NtrProgression |
| Successor | F853 | [DRAFT] | NTR ACL/Query layer, downstream of Aggregate |
| Successor | F854 | [DRAFT] | NTR Application Service, downstream of Domain Service |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block

Example:
| Predecessor | F540 | [CANCELLED] | Era.Core setup required |
| Successor | F567 | [DONE] | F567 depends on this feature |
| Related | F100 | [DONE] | Reference implementation |
-->

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "NtrProgression Aggregate is the SSOT for NTR state transitions" | NtrProgression AggregateRoot must exist and inherit AggregateRoot<NtrProgressionId> with factory method | AC#1, AC#2, AC#3, AC#9, AC#10 |
| "encapsulates all NTR progression invariants (phase advancement, route changes, exposure tracking, corruption)" | Aggregate must hold NtrPhase, NtrRoute, NtrParameters, Susceptibility state properties + ExposureLevel + IsFullyCorrupted flag | AC#5, AC#13, AC#14 |
| "raises domain events for downstream consumers" | All 4 domain events must exist and implement IDomainEvent; aggregate must have methods that raise them | AC#6, AC#7, AC#15 |
| "depends on Value Objects defined in F850" | Aggregate properties must use F850 VOs (NtrPhase, NtrRoute) | AC#5 |
| "consumed by the Domain Service in F852" | INtrCalculator must NOT appear in F851 (F852 scope) | AC#8 |
| Test coverage policy (CLAUDE.md) | New C# code must have unit tests verifying domain method behavior | AC#16, AC#17 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | NtrProgression.cs exists in Aggregates subfolder | file | Glob(C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs) | exists | - | [x] |
| 2 | NtrProgressionId readonly record struct exists | code | Grep(C:\Era\core\src\Era.Core\NTR, pattern="readonly record struct NtrProgressionId") | contains | `readonly record struct NtrProgressionId` | [x] |
| 3 | NtrProgression inherits AggregateRoot<NtrProgressionId> | code | Grep(C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs, pattern="AggregateRoot<NtrProgressionId>") | contains | `AggregateRoot<NtrProgressionId>` | [x] |
| 4 | NtrProgression has TargetCharacter and Visitor CharacterId properties | code | Grep(C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs, pattern="CharacterId TargetCharacter\|CharacterId Visitor") | gte | 2 | [x] |
| 5 | NtrProgression has NtrRoute, NtrPhase, NtrParameters, and Susceptibility state properties | code | Grep(C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs, pattern="NtrRoute\|NtrPhase\|NtrParameters\|Susceptibility") | gte | 4 | [x] |
| 6 | All 4 NTR domain event files exist | file | Glob(C:\Era\core\src\Era.Core\NTR\Domain\Events\*.cs) | gte | 4 | [x] |
| 7 | All 4 events implement IDomainEvent record pattern | code | Grep(C:\Era\core\src\Era.Core\NTR\Domain\Events, pattern="record.*IDomainEvent") | gte | 4 | [x] |
| 8 | INtrCalculator not referenced in F851 deliverables | code | Grep(C:\Era\core\src\Era.Core\NTR\Domain, pattern="INtrCalculator") | not_contains | `INtrCalculator` | [x] |
| 9 | NtrProgression has factory method Create | code | Grep(C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs, pattern="static.*NtrProgression.*Create") | contains | `NtrProgression` | [x] |
| 10 | NtrProgression has private constructor (DDD factory pattern) | code | Grep(C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs, pattern="private NtrProgression") | contains | `private NtrProgression` | [x] |
| 11 | No TODO/FIXME/HACK debt in deliverables | code | Grep(C:\Era\core\src\Era.Core\NTR\Domain, pattern="TODO|FIXME|HACK") | not_matches | `TODO|FIXME|HACK` | [x] |
| 12 | Era.Core project builds successfully | build | dotnet build C:\Era\core\src\Era.Core\Era.Core.csproj | succeeds | - | [x] |
| 13 | NtrProgression has IsFullyCorrupted boolean property | code | Grep(C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs, pattern="bool IsFullyCorrupted") | contains | `bool IsFullyCorrupted` | [x] |
| 14 | NtrProgression has ExposureLevel int property | code | Grep(C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs, pattern="int ExposureLevel") | contains | `int ExposureLevel` | [x] |
| 15 | NtrProgression has all 4 domain methods returning Result<Unit> | code | Grep(C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs, pattern="Result<Unit> AdvancePhase\|Result<Unit> ChangeRoute\|Result<Unit> SetExposureLevel\|Result<Unit> Corrupt") | gte | 4 | [x] |
| 16 | Unit test file exists for NtrProgression | file | Glob(C:\Era\core\src\Era.Core.Tests\NTR\Domain\NtrProgressionTests.cs) | exists | - | [x] |
| 17 | Unit tests pass | test | dotnet test C:\Era\core\src\Era.Core.Tests\Era.Core.Tests.csproj --filter "FullyQualifiedName~NtrProgressionTests" --blame-hang-timeout 10s | succeeds | - | [x] |

### AC Details

**AC#4: NtrProgression has TargetCharacter and Visitor CharacterId properties**
- **Test**: `Grep(C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs, pattern="CharacterId TargetCharacter|CharacterId Visitor")`
- **Expected**: `gte 2` (2 CharacterId properties: TargetCharacter + Visitor)
- **Rationale**: C6 constraint -- NtrProgression is a character-pair aggregate (architecture doc line 575, 677). Both properties must exist for the aggregate to represent an NTR relationship between two characters.
- **Derivation**: 1 TargetCharacter + 1 Visitor = 2 CharacterId properties

**AC#5: NtrProgression has NtrRoute, NtrPhase, NtrParameters, and Susceptibility state properties**
- **Test**: `Grep(C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs, pattern="NtrRoute|NtrPhase|NtrParameters|Susceptibility")`
- **Expected**: `gte 4` (minimum 4 references: 1 NtrRoute + 1 NtrPhase + 1 NtrParameters + 1 Susceptibility)
- **Rationale**: Philosophy claims aggregate "encapsulates all NTR progression invariants (phase advancement, route changes, exposure tracking, corruption)". Route, Phase, Parameters, and Susceptibility are the core state properties that represent the full NTR state SSOT.
- **Derivation**: 1 NtrRoute + 1 NtrPhase + 1 NtrParameters + 1 Susceptibility = 4 property references minimum

**AC#6: All 4 NTR domain event files exist**
- **Test**: `Glob(C:\Era\core\src\Era.Core\NTR\Domain\Events\*.cs)`
- **Expected**: `gte 4` (NtrPhaseAdvanced, NtrRouteChanged, NtrExposureLevelChanged, NtrCorrupted)
- **Rationale**: Goal specifies 4 domain event classes. C8 constraint requires each as a separate file following CharacterCreatedEvent convention.
- **Derivation**: 1 NtrPhaseAdvanced + 1 NtrRouteChanged + 1 NtrExposureLevelChanged + 1 NtrCorrupted = 4 event files

**AC#7: All 4 events implement IDomainEvent record pattern**
- **Test**: `Grep(C:\Era\core\src\Era.Core\NTR\Domain\Events, pattern="record.*IDomainEvent")`
- **Expected**: `gte 4` (each of 4 events must be a record implementing IDomainEvent)
- **Rationale**: C8 constraint -- each event must follow the `record ... : IDomainEvent` pattern established by CharacterCreatedEvent.
- **Derivation**: 4 event records: NtrPhaseAdvanced, NtrRouteChanged, NtrExposureLevelChanged, NtrCorrupted

**AC#15: NtrProgression has all 4 domain methods returning Result<Unit>**
- **Test**: `Grep(C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs, pattern="Result<Unit> AdvancePhase|Result<Unit> ChangeRoute|Result<Unit> SetExposureLevel|Result<Unit> Corrupt")`
- **Expected**: `gte 4` (4 domain methods: AdvancePhase, ChangeRoute, SetExposureLevel, Corrupt)
- **Rationale**: Philosophy claims aggregate "raises domain events for downstream consumers". Each domain method mutates state, raises the corresponding event, and returns Result<Unit> for F852 extension. Without these methods, event classes exist as orphans.
- **Derivation**: 1 AdvancePhase + 1 ChangeRoute + 1 SetExposureLevel + 1 Corrupt = 4 domain methods

**AC#16: Unit test file exists for NtrProgression**
- **Test**: `Glob(C:\Era\core\src\Era.Core.Tests\NTR\Domain\NtrProgressionTests.cs)`
- **Expected**: `exists`
- **Rationale**: Test coverage policy (CLAUDE.md) requires unit tests for new C# code. Domain methods have guard clause logic that must be regression-tested.

**AC#17: Unit tests pass**
- **Test**: `dotnet test C:\Era\core\src\Era.Core.Tests\Era.Core.Tests.csproj --filter "FullyQualifiedName~NtrProgressionTests" --blame-hang-timeout 10s`
- **Expected**: `succeeds` (exit code 0)
- **Rationale**: Tests must verify: AdvancePhase ceiling no-op, ChangeRoute same-route no-op, SetExposureLevel negative fail, Corrupt idempotency, happy paths with event raising, and Create() factory initialization.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | NtrProgressionId -- Strongly-typed aggregate ID (readonly record struct) | AC#2 |
| 2 | Aggregates/NtrProgression.cs -- AggregateRoot encapsulating NTR state as character-pair aggregate | AC#1, AC#3, AC#4, AC#5, AC#9, AC#10, AC#13, AC#14, AC#15 |
| 3 | Events/NtrPhaseAdvanced.cs -- Domain event for phase advancement | AC#6, AC#7 |
| 4 | Events/NtrRouteChanged.cs -- Domain event for route changes | AC#6, AC#7 |
| 5 | Events/NtrExposureLevelChanged.cs -- Domain event for exposure level change | AC#6, AC#7 |
| 6 | Events/NtrCorrupted.cs -- Domain event for corruption state | AC#6, AC#7 |
| 7 | INtrCalculator must NOT appear in F851 deliverables (F852 scope) | AC#8 |
| 8 | No technical debt markers in deliverables | AC#11 |
| 9 | Era.Core project builds successfully | AC#12 |
| 10 | Unit tests for domain method behavior | AC#16, AC#17 |

<!-- fc-phase-4-completed -->

---

## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

Implement NtrProgression as an AggregateRoot following the established Character aggregate pattern. The implementation spans three areas: (1) a strongly-typed ID struct, (2) the aggregate class with character-pair state and domain methods, and (3) four domain event records.

**Pattern source**: `Era.Core/Domain/Aggregates/Character.cs` and `Era.Core/Domain/Events/CharacterCreatedEvent.cs` establish the conventions to follow exactly.

**INtrCalculator exclusion strategy**: The architecture doc reference code shows domain methods accepting `INtrCalculator`, but C4 prohibits this dependency in F851 (INtrCalculator belongs to F852). F851 domain methods accept only strongly-typed Value Object arguments and enforce basic invariants without a calculator. The calculator-delegated invariants (CanAdvance, CanChangeRoute) are F852 scope. This is the correct DDD sequencing: the Aggregate defines what events it raises; the Domain Service (F852) decides when transitions are valid.

**Cross-repo execution**: All files are created in `C:\Era\core\src\Era.Core\NTR\Domain\`. Build validation targets `C:\Era\core\src\Era.Core\Era.Core.csproj`.

**NtrParameters naming**: The `Era.Core.NTR.Domain.ValueObjects.NtrParameters` (F850 VO) and `Era.Core.Types.NtrParameters` are distinct types in different namespaces. NtrProgression uses the VO version; if both appear in scope, the NTR namespace alias resolves the collision.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs` |
| 2 | Create `NtrProgressionId` as `readonly record struct` — either in its own file under `NTR\Domain\` or at the top of NtrProgression.cs (separate file preferred for discoverability) |
| 3 | Declare `NtrProgression : AggregateRoot<NtrProgressionId>` — the class declaration contains the literal `AggregateRoot<NtrProgressionId>` string |
| 4 | Declare `public CharacterId TargetCharacter { get; private set; }` and `public CharacterId Visitor { get; private set; }` as separate properties |
| 5 | Declare `public NtrRoute CurrentRoute { get; private set; }`, `public NtrPhase CurrentPhase { get; private set; }`, `public NtrParameters Parameters { get; private set; }`, and `public Susceptibility CurrentSusceptibility { get; private set; }` — all 4 type names appear in the file |
| 13 | Declare `public bool IsFullyCorrupted { get; private set; }` — set to true in Corrupt() method. Named IsFullyCorrupted (not IsCorrupted) to distinguish from NtrRoute.IsCorrupted (route classification threshold) |
| 14 | Declare `public int ExposureLevel { get; private set; }` — set in SetExposureLevel() method |
| 15 | Domain methods `AdvancePhase()`, `ChangeRoute(NtrRoute)`, `SetExposureLevel(int)`, `Corrupt()` — each returns `Result<Unit>`, includes guard clauses (no-op/idempotency/non-negative), mutates state, and calls `AddDomainEvent(...)` |
| 6 | Create four separate .cs files: `NtrPhaseAdvanced.cs`, `NtrRouteChanged.cs`, `NtrExposureLevelChanged.cs`, `NtrCorrupted.cs` in `NTR\Domain\Events\` |
| 7 | Each event record uses `record X(...) : IDomainEvent` declaration — `record.*IDomainEvent` pattern matches all 4 |
| 8 | NtrProgression.cs and NtrProgressionId.cs contain no reference to `INtrCalculator` — domain methods accept only typed VOs |
| 9 | Factory method `public static NtrProgression Create(NtrProgressionId id, CharacterId targetCharacter, CharacterId visitor)` using object initializer for `protected init` Id property |
| 10 | `private NtrProgression() { }` private parameterless constructor per DDD factory pattern |
| 11 | No TODO/FIXME/HACK comments in any deliverable file |
| 12 | Build `Era.Core.csproj` succeeds — verified by correct namespace declarations, no missing using directives, TreatWarningsAsErrors compliance |
| 16 | Create `C:\Era\core\src\Era.Core.Tests\NTR\Domain\NtrProgressionTests.cs` with unit tests covering all 4 domain method guard clauses and happy paths |
| 17 | Run `dotnet test` filtered to NtrProgressionTests — all tests pass |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| NtrProgressionId file location | (A) Separate `NtrProgressionId.cs` file, (B) Nested within `NtrProgression.cs` | A: Separate file | Follows Character/CharacterId convention; keeps types individually discoverable by LSP and grep |
| Domain method signatures (no INtrCalculator) | (A) Stub methods accepting only VOs, raising events directly; (B) No domain methods in F851 (state-only aggregate) | A: Stub methods with VO params | Methods establish the event-raising contract for F852 to wire; aggregate without methods is an anemic model. Methods accept typed VO arguments only. |
| NtrParameters inclusion in NtrProgression | (A) Include `NtrParameters Parameters` as aggregate property, (B) Omit from F851 (add in F852) | A: Include | Architecture reference code (line 683) shows `NtrParameters Parameters` as a core property; FAV/SlaveLevel parameters are part of NTR state SSOT |
| Exposure level type for NtrExposureLevelChanged event | (A) Raw `int` for exposure level, (B) Separate ExposureDegree VO (not in F850) | A: Raw int | C3 constraint: no ExposureDegree VO exists; C8 requires structural existence only; raw int is acceptable until F852+ adds VO |
| Susceptibility inclusion | (A) Include `Susceptibility` as aggregate property, (B) Omit from F851 | A: Include | Susceptibility VO (F850) models the 屈服度/好感度 balance that is a core NTR state axis; aggregate state is incomplete without it |
| Create factory event | (A) Raise domain event on Create (like Character raises CharacterCreatedEvent), (B) No event on Create | B: No event on Create | No state transition at creation (Phase0/R0 initial state). NtrProgressionCreated event deferred to F852 as Mandatory Handoff for event-driven observability |
| ExposureLevel directionality | (A) Monotonicity guard (only allow increases), (B) Allow bidirectional changes (absolute assignment) | B: Bidirectional | SetExposureLevel accepts absolute value, not delta. ExposureLevel semantics coupled with naming decision ([pending] in Review Notes). Bidirectional allows F852 Domain Service to enforce direction policy. |

### Interfaces / Data Structures

#### NtrProgressionId

```csharp
// C:\Era\core\src\Era.Core\NTR\Domain\NtrProgressionId.cs
namespace Era.Core.NTR.Domain;

/// <summary>
/// Strongly-typed identifier for NtrProgression aggregate.
/// Must be readonly record struct to satisfy AggregateRoot&lt;TId&gt; where TId : struct constraint.
/// </summary>
public readonly record struct NtrProgressionId
{
    public Guid Value { get; }

    public NtrProgressionId(Guid value) => Value = value;

    public static NtrProgressionId New() => new(Guid.NewGuid());
}
```

#### NtrProgression Aggregate

```csharp
// C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs
using Era.Core.Domain;
using Era.Core.NTR.Domain.Events;
using Era.Core.NTR.Domain.ValueObjects;
using Era.Core.Types;

namespace Era.Core.NTR.Domain.Aggregates;

/// <summary>
/// Aggregate root for NTR progression state between a target character and a visitor.
/// Encapsulates NTR phase, route, FAV parameters, and susceptibility as SSOT for NTR state transitions.
/// </summary>
public class NtrProgression : AggregateRoot<NtrProgressionId>
{
    public CharacterId TargetCharacter { get; private set; }
    public CharacterId Visitor { get; private set; }
    public NtrRoute CurrentRoute { get; private set; }
    public NtrPhase CurrentPhase { get; private set; }
    public NtrParameters Parameters { get; private set; }
    public Susceptibility CurrentSusceptibility { get; private set; }
    public bool IsFullyCorrupted { get; private set; }
    public int ExposureLevel { get; private set; }

    private NtrProgression() { } // Private constructor for DDD factory pattern

    /// <summary>
    /// Factory method: creates NtrProgression at initial state (Phase0, R0).
    /// Uses object initializer for protected init Id property (AggregateRoot constraint).
    /// </summary>
    public static NtrProgression Create(
        NtrProgressionId id,
        CharacterId targetCharacter,
        CharacterId visitor,
        NtrParameters parameters,
        Susceptibility susceptibility)
    {
        return new NtrProgression
        {
            Id = id,
            TargetCharacter = targetCharacter,
            Visitor = visitor,
            CurrentRoute = NtrRoute.R0,
            CurrentPhase = NtrPhase.Phase0,
            Parameters = parameters,
            CurrentSusceptibility = susceptibility,
        };
    }

    /// <summary>
    /// Advance NTR phase to the next stage. Raises NtrPhaseAdvanced event.
    /// Returns Ok if phase advanced; Ok (no-op) if already at ceiling.
    /// Complex validation (CanAdvance) is F852 scope (INtrCalculator).
    /// </summary>
    public Result<Unit> AdvancePhase()
    {
        var nextPhase = CurrentPhase.Next();
        if (nextPhase == CurrentPhase)
            return Result<Unit>.Ok(Unit.Value); // No-op at Phase7 ceiling

        var previousPhase = CurrentPhase;
        CurrentPhase = nextPhase;
        AddDomainEvent(new NtrPhaseAdvanced(Id, TargetCharacter, Visitor, previousPhase, CurrentPhase));
        return Result<Unit>.Ok(Unit.Value);
    }

    /// <summary>
    /// Change NTR route. Raises NtrRouteChanged event.
    /// Returns Ok if route changed; Ok (no-op) if same route.
    /// Route validity enforcement is F852 scope (INtrCalculator).
    /// </summary>
    public Result<Unit> ChangeRoute(NtrRoute newRoute)
    {
        if (newRoute == CurrentRoute)
            return Result<Unit>.Ok(Unit.Value); // No-op for same route

        var previousRoute = CurrentRoute;
        CurrentRoute = newRoute;
        AddDomainEvent(new NtrRouteChanged(Id, TargetCharacter, Visitor, previousRoute, CurrentRoute));
        return Result<Unit>.Ok(Unit.Value);
    }

    /// <summary>
    /// Set exposure level. Raises NtrExposureLevelChanged event.
    /// </summary>
    public Result<Unit> SetExposureLevel(int newExposureLevel)
    {
        if (newExposureLevel < 0)
            return Result<Unit>.Fail("Exposure level cannot be negative");

        if (newExposureLevel == ExposureLevel)
            return Result<Unit>.Ok(Unit.Value); // No-op for same value

        var previousExposureLevel = ExposureLevel;
        ExposureLevel = newExposureLevel;
        AddDomainEvent(new NtrExposureLevelChanged(Id, TargetCharacter, Visitor, previousExposureLevel, newExposureLevel));
        return Result<Unit>.Ok(Unit.Value);
    }

    /// <summary>
    /// Transition to corrupted state. Raises NtrCorrupted event.
    /// Idempotent: returns Ok (no-op) if already corrupted.
    /// </summary>
    public Result<Unit> Corrupt()
    {
        if (IsFullyCorrupted)
            return Result<Unit>.Ok(Unit.Value); // Idempotent

        IsFullyCorrupted = true;
        AddDomainEvent(new NtrCorrupted(Id, TargetCharacter, Visitor, CurrentPhase, CurrentRoute));
        return Result<Unit>.Ok(Unit.Value);
    }
}
```

#### Domain Events

```csharp
// C:\Era\core\src\Era.Core\NTR\Domain\Events\NtrPhaseAdvanced.cs
using Era.Core.NTR.Domain.ValueObjects;

namespace Era.Core.NTR.Domain.Events;

/// <summary>Raised when NTR phase advances to the next stage.</summary>
public record NtrPhaseAdvanced(
    NtrProgressionId AggregateId,
    CharacterId TargetCharacter,
    CharacterId Visitor,
    NtrPhase PreviousPhase,
    NtrPhase NewPhase) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

```csharp
// C:\Era\core\src\Era.Core\NTR\Domain\Events\NtrRouteChanged.cs
using Era.Core.NTR.Domain.ValueObjects;

namespace Era.Core.NTR.Domain.Events;

/// <summary>Raised when NTR route changes to a new route classification.</summary>
public record NtrRouteChanged(
    NtrProgressionId AggregateId,
    CharacterId TargetCharacter,
    CharacterId Visitor,
    NtrRoute PreviousRoute,
    NtrRoute NewRoute) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

```csharp
// C:\Era\core\src\Era.Core\NTR\Domain\Events\NtrExposureLevelChanged.cs
namespace Era.Core.NTR.Domain.Events;

/// <summary>
/// Raised when exposure level changes.
/// ExposureDegree VO deferred to F852+; raw int used per C3 constraint.
/// </summary>
public record NtrExposureLevelChanged(
    NtrProgressionId AggregateId,
    CharacterId TargetCharacter,
    CharacterId Visitor,
    int PreviousExposureLevel,
    int NewExposureLevel) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

```csharp
// C:\Era\core\src\Era.Core\NTR\Domain\Events\NtrCorrupted.cs
using Era.Core.NTR.Domain.ValueObjects;

namespace Era.Core.NTR.Domain.Events;

/// <summary>Raised when character enters corrupted NTR state (Phase7 completion).</summary>
public record NtrCorrupted(
    NtrProgressionId AggregateId,
    CharacterId TargetCharacter,
    CharacterId Visitor,
    NtrPhase FinalPhase,
    NtrRoute FinalRoute) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

**Note**: All event records reference `IDomainEvent` from `Era.Core.Domain.Events` namespace. Each event file needs `using Era.Core.Domain.Events;` for the interface.

#### IDomainEvent using directive

Each event file must include:
```csharp
using Era.Core.Domain.Events;
```
The `IDomainEvent` interface lives in `Era.Core.Domain.Events` namespace, not `Era.Core.NTR.Domain.Events`.

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| Architecture reference code (lines 686-705) shows `AdvancePhase(INtrCalculator)` and `ChangeRoute(NtrRoute, INtrCalculator)` signatures — these conflict with C4 (INtrCalculator exclusion). F851 uses VO-only signatures. | Technical Constraints (C4) | No AC change needed; C4 correctly excludes INtrCalculator. Document deviation from architecture reference in code comments. |
| NtrProgressionId uses `Guid` as backing type — architecture doc does not specify the underlying type. Character aggregate uses `CharacterId(int)` pattern but CharacterId is a domain type not a primitive ID struct. | Interfaces / Data Structures | No AC impact. Guid is the correct choice for a new aggregate ID (uniqueness without external coordination). Record struct satisfies TId : struct. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 2 | Create `C:\Era\core\src\Era.Core\NTR\Domain\NtrProgressionId.cs` as `readonly record struct NtrProgressionId` with `Guid Value` property, constructor, and static `New()` factory | | [x] |
| 2 | 1, 3, 4, 5, 8, 9, 10, 11, 13, 14, 15 | Create `C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs`: `NtrProgression : AggregateRoot<NtrProgressionId>` with TargetCharacter/Visitor CharacterId properties, NtrRoute/NtrPhase/NtrParameters/Susceptibility state properties, private constructor, static `Create(...)` factory method, and domain methods (AdvancePhase, ChangeRoute, SetExposureLevel, Corrupt) raising domain events -- no INtrCalculator references | | [x] |
| 3 | 6, 7, 11 | Create 4 domain event records in `C:\Era\core\src\Era.Core\NTR\Domain\Events\`: `NtrPhaseAdvanced.cs`, `NtrRouteChanged.cs`, `NtrExposureLevelChanged.cs`, `NtrCorrupted.cs` -- each as `public record X(...) : IDomainEvent` with `OccurredAt` property and `using Era.Core.Domain.Events;` directive | | [x] |
| 4 | 12 | Build `C:\Era\core\src\Era.Core\Era.Core.csproj` and verify exit code 0 (TreatWarningsAsErrors compliance) | | [x] |
| 5 | 16, 17 | Create `C:\Era\core\src\Era.Core.Tests\NTR\Domain\NtrProgressionTests.cs` with unit tests for all 4 domain methods: guard clauses (ceiling no-op, same-route no-op, negative exposure fail, idempotent corruption), happy paths (state mutation + event raising), and factory Create() initialization | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Feature 851 Tasks 1-3: create NtrProgressionId, NtrProgression aggregate, and 4 domain event records in `C:\Era\core\src\Era.Core\NTR\Domain\` | New files: `NTR/Domain/NtrProgressionId.cs`, `NTR/Domain/Aggregates/NtrProgression.cs`, `NTR/Domain/Events/NtrPhaseAdvanced.cs`, `NTR/Domain/Events/NtrRouteChanged.cs`, `NTR/Domain/Events/NtrExposureLevelChanged.cs`, `NTR/Domain/Events/NtrCorrupted.cs` |
| 2 | implementer | sonnet | Task 4: build Era.Core.csproj | Build success (exit code 0) |
| 3 | implementer | sonnet | Task 5: create NtrProgressionTests.cs with unit tests for domain methods (guard clauses, happy paths, factory Create) | Test file + all tests pass |

### Pre-conditions

- F850 [DONE]: `NtrPhase`, `NtrRoute`, `NtrParameters`, `Susceptibility` VOs exist in `C:\Era\core\src\Era.Core\NTR\Domain\ValueObjects\`
- `AggregateRoot<TId>` base class exists at `C:\Era\core\src\Era.Core\Domain\AggregateRoot.cs`
- `IDomainEvent` interface exists at `C:\Era\core\src\Era.Core\Domain\Events\IDomainEvent.cs`
- `CharacterId` type exists in Era.Core (used by Character aggregate at `C:\Era\core\src\Era.Core\Domain\Aggregates\Character.cs`)

### Execution Order

**Phase 1 — Task 1: Create NtrProgressionId**

Create `C:\Era\core\src\Era.Core\NTR\Domain\NtrProgressionId.cs`:
- Namespace: `Era.Core.NTR.Domain`
- Type: `public readonly record struct NtrProgressionId`
- Must satisfy `AggregateRoot<TId> where TId : struct` constraint
- Contains: `Guid Value` property, `public NtrProgressionId(Guid value)` constructor, `public static NtrProgressionId New()` factory
- No INtrCalculator references

**Phase 1 — Task 2: Create NtrProgression Aggregate**

Create `C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs`:
- Namespace: `Era.Core.NTR.Domain.Aggregates`
- Required usings: `Era.Core.Domain`, `Era.Core.NTR.Domain.Events`, `Era.Core.NTR.Domain.ValueObjects`, `Era.Core.Types`
- Class declaration: `public class NtrProgression : AggregateRoot<NtrProgressionId>` (literal string `AggregateRoot<NtrProgressionId>` must appear)
- Properties: `CharacterId TargetCharacter { get; private set; }`, `CharacterId Visitor { get; private set; }`, `NtrRoute CurrentRoute { get; private set; }`, `NtrPhase CurrentPhase { get; private set; }`, `NtrParameters Parameters { get; private set; }`, `Susceptibility CurrentSusceptibility { get; private set; }`, `bool IsFullyCorrupted { get; private set; }`, `int ExposureLevel { get; private set; }`
- Private constructor: `private NtrProgression() { }` (parameterless, for DDD factory pattern)
- Factory method: `public static NtrProgression Create(NtrProgressionId id, CharacterId targetCharacter, CharacterId visitor, NtrParameters parameters, Susceptibility susceptibility)` using object initializer for `protected init` Id property (do NOT assign Id via constructor parameter)
- Domain methods: `AdvancePhase()`, `ChangeRoute(NtrRoute newRoute)`, `SetExposureLevel(int newExposureLevel)`, `Corrupt()` -- each returns `Result<Unit>`, includes guard clauses (no-op at ceiling, same-route no-op, non-negative check, idempotency), mutates aggregate state, and calls `AddDomainEvent(...)` with the corresponding event record
- INtrCalculator must NOT appear anywhere in this file (C4 constraint); CanAdvance/CanChangeRoute invariant validation is F852 scope
- No TODO/FIXME/HACK comments

**Phase 1 — Task 3: Create 4 Domain Event Records**

Create 4 files in `C:\Era\core\src\Era.Core\NTR\Domain\Events\`:

1. `NtrPhaseAdvanced.cs` -- `public record NtrPhaseAdvanced(NtrProgressionId AggregateId, CharacterId TargetCharacter, CharacterId Visitor, NtrPhase PreviousPhase, NtrPhase NewPhase) : IDomainEvent` with `OccurredAt = DateTime.UtcNow`
2. `NtrRouteChanged.cs` -- `public record NtrRouteChanged(NtrProgressionId AggregateId, CharacterId TargetCharacter, CharacterId Visitor, NtrRoute PreviousRoute, NtrRoute NewRoute) : IDomainEvent` with `OccurredAt = DateTime.UtcNow`
3. `NtrExposureLevelChanged.cs` -- `public record NtrExposureLevelChanged(NtrProgressionId AggregateId, CharacterId TargetCharacter, CharacterId Visitor, int PreviousExposureLevel, int NewExposureLevel) : IDomainEvent` with `OccurredAt = DateTime.UtcNow` (raw int per C3 constraint; Previous+New pattern; CharacterId pair for event self-containment)
4. `NtrCorrupted.cs` -- `public record NtrCorrupted(NtrProgressionId AggregateId, CharacterId TargetCharacter, CharacterId Visitor, NtrPhase FinalPhase, NtrRoute FinalRoute) : IDomainEvent` with `OccurredAt = DateTime.UtcNow`

Each file must include `using Era.Core.Domain.Events;` (IDomainEvent lives in that namespace, not `Era.Core.NTR.Domain.Events`). The `record.*IDomainEvent` grep pattern must match all 4.

No TODO/FIXME/HACK comments in any event file.

**Phase 2 — Task 4: Build Verification**

Run: `dotnet build C:\Era\core\src\Era.Core\Era.Core.csproj`

- Must exit with code 0
- TreatWarningsAsErrors is active -- zero warnings permitted
- If build fails: STOP, report to user

### Success Criteria

- All 6 new files created in correct locations under `C:\Era\core\src\Era.Core\NTR\Domain\`
- `NtrProgressionId` is `readonly record struct` (satisfies `TId : struct`)
- `NtrProgression` declares `AggregateRoot<NtrProgressionId>` inheritance
- Both `CharacterId TargetCharacter` and `CharacterId Visitor` properties present
- Both `NtrRoute` and `NtrPhase` properties present (plus `NtrParameters`, `Susceptibility`)
- `IsFullyCorrupted` and `ExposureLevel` properties present with state mutation in domain methods
- Factory method `Create(...)` uses object initializer for Id (not constructor parameter)
- Private parameterless constructor present
- All 4 event records match `record.*IDomainEvent` pattern
- Zero INtrCalculator references in any F851 deliverable
- Zero TODO/FIXME/HACK in any F851 deliverable
- Era.Core.csproj build succeeds (exit code 0)

### Error Handling

- **NtrParameters naming collision**: Use fully-qualified name `Era.Core.NTR.Domain.ValueObjects.NtrParameters` or a using alias if both `Era.Core.Types.NtrParameters` and the VO type appear in scope simultaneously
- **missing using for IDomainEvent**: Each event file needs `using Era.Core.Domain.Events;` -- the interface is NOT in `Era.Core.NTR.Domain.Events`
- **`protected init` Id assignment**: The `Id` property uses `protected init` accessor -- assign via object initializer (`Id = id`) inside `Create()`, NOT via a constructor parameter
- **Build warnings as errors**: Any CS warning becomes a build error -- use explicit types where `var` would be ambiguous, ensure all usings are consumed

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| NtrParameters/Susceptibility mutation methods + events | Aggregate has no methods to update these properties post-creation; mutation path needed for runtime state changes | Existing Feature | F852 | - | [x] | 追記済み |
| ExposureDegree Value Object (replacing raw int in NtrExposureLevelChanged event) | C3 constraint: no ExposureDegree VO in F850; raw int is interim | Existing Feature | F852 | - | [x] | 追記済み |
| NtrProgressionCreated domain event | Create() factory does not raise a creation event (Key Decision: no transition at initialization). Event-driven consumers need aggregate creation visibility. | Existing Feature | F852 | - | [x] | 追記済み |

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->
<!-- Transferred + Result columns (F811/F805 Lesson):
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: created(A), appended(B), documented(C), confirmed(existing)
- Phase 9.4.1 executes transfer and fills Result. Phase 10.0.2 verifies only
- Prevents "Destination filled but content never transferred" gap
-->
<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists -> OK (file created during /run)
- Option B: Referenced Feature exists -> OK
- Option C: Phase exists in architecture.md -> OK
- Missing Task for Option A -> FL FAIL
-->
<!-- DRAFT Creation Checklist (Option A):
When a Task creates a new feature-{ID}.md [DRAFT], it MUST complete ALL of:
1. Create feature-{ID}.md file
2. Register in index-features.md (add row to Active Features table)
3. Update "Next Feature number" in index-features.md
AC for DRAFT creation MUST verify BOTH file existence AND index registration.
-->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-08T00:00 | PHASE_1_COMPLETE | orchestrator | Initialize | READY, [REVIEWED]->[WIP] |
<!-- run-phase-1-completed -->
| 2026-03-08T00:01 | PHASE_2_COMPLETE | orchestrator | Investigation | Codebase patterns confirmed |
<!-- run-phase-2-completed -->
| 2026-03-08T00:02 | PHASE_3_COMPLETE | orchestrator | TDD RED | Tests created, build fails as expected (RED confirmed) |
<!-- run-phase-3-completed -->
| 2026-03-08T00:05 | PHASE_4_COMPLETE | orchestrator | Implementation | Tasks 1-5 complete, build 0W/0E, 28/28 tests pass |
<!-- run-phase-4-completed -->
| 2026-03-08T00:10 | DEVIATION | ac-tester | AC#7 verify | FAIL: record.*IDomainEvent pattern=0 (multi-line record syntax) |
| 2026-03-08T00:11 | FIX | debugger | AC#7 fix | Reformatted 4 event records to single-line syntax, grep now returns 4 |
| 2026-03-08T00:12 | PHASE_7_COMPLETE | orchestrator | Verification | 17/17 ACs PASS (1 debug iteration) |
<!-- run-phase-7-completed -->
| 2026-03-08T00:15 | DEVIATION | feature-reviewer | Step 8.1 | NEEDS_REVISION: PATTERNS.md SSOT missing NTR Domain + Handoffs not transferred |
| 2026-03-08T00:16 | RESOLUTION | orchestrator | Step 8.2-8.3 | SSOT rules: no matching paths (NTR/Domain not in Types/, no interfaces). Handoffs: Phase 9.4.1 scope |
| 2026-03-08T00:16 | PHASE_8_COMPLETE | orchestrator | Post-Review | NEEDS_REVISION items deferred to Phase 9 (handoffs) |
<!-- run-phase-8-completed -->
| 2026-03-08T08:03 | DEVIATION | Bash | git commit (core) | exit 1: PRE-EXISTING ComHotReloadTests flaky timing test (unrelated to F851) |
| 2026-03-08T08:04 | PHASE_10_COMMIT | orchestrator | git commit (core) | 5a4a88a: 7 files, 484 insertions |
| 2026-03-08T08:04 | PHASE_10_COMMIT | orchestrator | git commit (devkit) | f98c941: 3 files, PM updates |
| 2026-03-08T08:05 | CodeRabbit | 3 Minor (修正不要) | False positives: C# child namespaces can access parent types without explicit using |
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase2-Review iter1: pm/features/feature-851.md:724-730 | Extra 'Reference (from previous session)' section removed
- [fix] Phase2-Review iter1: Technical Design > NtrProgression Aggregate | Added IsCorrupted and ExposureLevel properties with state mutation in Corrupt() and SetExposureLevel()
- [fix] Phase2-Review iter1: AC Definition Table + Philosophy Derivation | Added AC#13 (IsCorrupted) and AC#14 (ExposureLevel); updated Philosophy Derivation coverage
- [fix] Phase2-Uncertain iter1: Philosophy | Added qualifying statement about invariant deferral to F852 Domain Service
- [fix] Phase2-Review iter1: AC#5 + Goal Coverage | Expanded AC#5 pattern to include NtrParameters|Susceptibility (gte 4); updated Goal Coverage
- [fix] Phase2-Review iter2: AC#8 | Widened Grep scope from NTR\Domain\Aggregates to NTR\Domain to cover all F851 deliverables per C4
- [fix] Phase2-Review iter2: AC#11 | Widened Grep scope from NTR\Domain\Aggregates to NTR\Domain to cover all F851 deliverables
- [fix] Phase2-Review iter1: Philosophy + Philosophy Derivation | Added "exposure tracking" to invariant list; added AC#14 to derivation coverage
- [fix] Phase2-Review iter1: Implementation Contract > Success Criteria | Changed IsCorrupted to IsFullyCorrupted (consistency with AC#13/Technical Design)
- [fix] Phase2-Review iter1: Background | Removed non-template Architecture Task Coverage subsection heading
- [fix] Phase2-Review iter1: Acceptance Criteria | Added missing template-required ac-designer written-by comment
- [fix] Phase2-Review iter1: Technical Design | Added missing template-required tech-designer written-by comment
- [fix] Phase3-Maintainability iter2: AC Definition Table + Tasks + Implementation Contract | Added AC#16 (test file exists), AC#17 (tests pass), Task 5 (create unit tests), Implementation Contract Phase 3 (test execution)
- [fix] Phase3-Maintainability iter2: Mandatory Handoffs | Added NtrProgressionCreated event deferral handoff to F852
- [fix] Phase3-Maintainability iter2: Key Decisions | Updated Create factory event decision to B (no event, handoff); added ExposureLevel directionality decision (bidirectional)
- [fix] Phase2-Review iter3: Technical Design > SetExposureLevel | Added same-value guard (newExposureLevel == ExposureLevel → no-op) for consistency with other domain methods
- [fix] Phase2-Review iter4: Philosophy | Corrected overcorrection: "all invariant validation delegated" → "basic structural guards in aggregate, complex validation delegated to F852"
- [fix] Phase3-Maintainability iter5: Technical Design > NtrExposureLevelChanged + SetExposureLevel | Added PreviousExposureLevel to event record for Previous+New pattern consistency with other events
- [fix] Phase2-Review iter3: AC Definition Table | Added AC#15 verifying all 4 domain methods exist on aggregate; updated Philosophy Derivation, Goal Coverage, Task#2
- [fix] Phase2-Review iter4: Philosophy | Revised 'structural invariants enforced at aggregate' to 'all invariant validation delegated to F852 Domain Service' (design shows zero guards)
- [fix] Phase3-Maintainability iter5: Mandatory Handoffs | Added NtrParameters/Susceptibility mutation deferral to F852 + ExposureDegree VO deferral to F852
- [resolved-applied] Phase3-Maintainability iter5: Domain method return type void vs Result<Unit>. Applied: changed all 4 methods to Result<Unit>.
- [resolved-applied] Phase3-Maintainability iter5: SetExposureLevel(int) accepts negative values. Applied: added non-negative guard returning Fail.
- [resolved-applied] Phase3-Maintainability iter5: Corrupt() lacks idempotency guard. Applied: added if (IsFullyCorrupted) return Ok (idempotent).
- [resolved-applied] Phase3-Maintainability iter6: AdvancePhase() at Phase7 raises spurious event. Applied: added no-op guard if (nextPhase == CurrentPhase).
- [resolved-applied] Phase3-Maintainability iter6: ChangeRoute(sameRoute) raises spurious event. Applied: added if (newRoute == CurrentRoute) return Ok guard.
- [resolved-applied] Phase3-Maintainability iter6: IsCorrupted naming collision. Applied: renamed to IsFullyCorrupted to distinguish from NtrRoute.IsCorrupted.
- [fix] Phase4-ACValidation iter6: AC#1 | Changed Expected from '1' to '-' for exists matcher (F626 convention)
- [fix] PostLoop-UserFix post-loop: Technical Design + AC#15 | Changed domain methods void→Result<Unit>, added guard clauses (no-op/idempotency/non-negative), updated AC#15 pattern and AC Details
- [fix] PostLoop-UserFix post-loop: AC#13 + Technical Design | Renamed IsCorrupted→IsFullyCorrupted to avoid collision with NtrRoute.IsCorrupted
- [resolved-applied] Phase3-Maintainability iter7: Domain events carry only NtrProgressionId, not CharacterId pair. Downstream consumers must load aggregate for character routing. Fix: add TargetCharacter/Visitor to all 4 event records.
- [fix] PostLoop-UserFix post-loop: Technical Design + Implementation Contract | Added TargetCharacter/Visitor CharacterId pair to all 4 event records and aggregate method calls for event self-containment
- [resolved-applied] Phase3-Maintainability iter7: SetExposureLevel(int) performs absolute assignment but name implies delta/increment. Misleading API contract. Fix: rename to SetExposureLevel or change to delta semantics.
- [fix] PostLoop-UserFix post-loop: Technical Design + AC#15 + Implementation Contract | Renamed IncreaseExposure→SetExposureLevel and NtrExposureIncreased→NtrExposureLevelChanged for naming-semantics consistency

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

---

<!-- fc-phase-6-completed -->

## Links

[Predecessor: F850](feature-850.md) - NTR UL + Value Objects (NtrPhase, NtrRoute, NtrParameters, Susceptibility)
[Successor: F852](feature-852.md) - NTR Domain Service (INtrCalculator), consumes NtrProgression
[Successor: F853](feature-853.md) - NTR ACL/Query layer, downstream of Aggregate
[Successor: F854](feature-854.md) - NTR Application Service, downstream of Domain Service

