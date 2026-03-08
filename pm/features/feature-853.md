# Feature 853: NTR Application Services + Anti-Corruption Layer

## Status: [DONE]
<!-- fl-reviewed: 2026-03-08T04:53:21Z -->

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
<!-- fc-phase-2-completed -->
## Background

### Philosophy (Mid-term Vision)

Phase 24: NTR Bounded Context -- Application + Infrastructure Layer is the SSOT for bridging the NTR Domain Model (F850-F852) with existing game system consumers. Pipeline Continuity -- Application Services are the consumer layers that invoke the Domain Service (F852 INtrCalculator) for guard-then-mutate orchestration, and the Anti-Corruption Layer translates between the new DDD typed model and legacy ERB-era interfaces (INtrQuery) using Aggregate state reads. This feature completes the Phase 24 DDD layered architecture by providing the orchestration and persistence layers above the Domain.

### Problem (Current Issue)

The NTR Bounded Context has a complete Domain layer (Value Objects F850, Aggregate F851, Domain Service F852) but no Application or Infrastructure layers exist to consume it. Existing INtrQuery consumers (ClothingSystem with 7 call sites at `ClothingSystem.cs:992-1051`, PregnancyAnnouncement with 4 call sites at `PregnancyAnnouncement.cs:273-314`) are bridged to a `NullNtrQuery` stub (`ServiceCollectionExtensions.cs:115`) that always returns false, because no Anti-Corruption Layer exists to translate from the legacy `CheckNtrFavorably(CharacterId, int)` contract to the new NtrProgression Aggregate's typed phase/route model. Additionally, the Aggregate's `AdvancePhase()` method is parameterless (`NtrProgression.cs:40-49`) -- it does not accept an INtrCalculator guard -- meaning the Application Command layer must own the eligibility-check-then-mutate orchestration pattern.

### Goal (What to Achieve)

Implement Application Services and Anti-Corruption Layer for the NTR Bounded Context:
- Application Commands: `AdvanceNtrPhaseCommand` and `ChangeNtrRouteCommand` that orchestrate INtrCalculator eligibility checks before invoking NtrProgression Aggregate mutations
- Application Queries: `GetNtrStatusQuery` and `GetSusceptibilityQuery` that read NtrProgression Aggregate state
- Event Handler: `NtrPhaseAdvancedHandler` for the NtrPhaseAdvanced domain event
- Infrastructure: `NtrProgressionRepository` extending InMemoryRepository pattern, with CharacterId-based lookup for ACL support
- Anti-Corruption Layer: ACL class implementing INtrQuery to bridge legacy consumers to the NtrProgression Aggregate, replacing NullNtrQuery
- DI Registration: Register all new services in ServiceCollectionExtensions, replacing NullNtrQuery with ACL

**DDD Input Reference**: `pm/reference/ntr-ddd-input.md` -- OB-03 INtrQuery routes to Phase 24 definition (F829 deferred obligation). The Anti-Corruption Layer must bridge INtrQuery consumers with the new NtrProgression Aggregate.

### Architecture Task Coverage

<!-- Architecture Task 8: NTR Application Services設計 -->
<!-- Architecture Task 9: Anti-Corruption Layer設計（既存システムとの橋渡し） -->

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do INtrQuery consumers get incorrect results? | NullNtrQuery always returns false regardless of NTR state | `ServiceCollectionExtensions.cs:115` |
| 2 | Why is NullNtrQuery still registered? | No typed implementation bridges INtrQuery to the NtrProgression Aggregate | Grep for `AntiCorruption` returns no files in core repo |
| 3 | Why has no ACL been built? | Phase 24 was designed bottom-up: VOs (F850) -> Aggregate (F851) -> Domain Service (F852) -> Application/Infrastructure (F853) | `phase-20-27-game-systems.md:591-601` |
| 4 | Why does the Application layer carry extra orchestration responsibility? | NtrProgression.AdvancePhase() is parameterless -- it does not accept INtrCalculator as a guard parameter | `NtrProgression.cs:40-49` |
| 5 | Why (Root)? | No Anti-Corruption Layer exists to translate between INtrQuery's flat `CheckNtrFavorably(CharacterId, int)` contract and the NtrProgression Aggregate's typed phase/route model, and no Application Services exist to orchestrate INtrCalculator eligibility checks before Aggregate mutations | `INtrQuery.cs:19`, `INtrCalculator.cs:24-33` |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | INtrQuery consumers receive false for all NTR checks via NullNtrQuery | No ACL translates between legacy INtrQuery interface and new NtrProgression Aggregate model |
| Where | `ServiceCollectionExtensions.cs:115` (NullNtrQuery registration) | Missing `NTR/Application/` and `NTR/Infrastructure/` layers |
| Fix | Implement NullNtrQuery with hardcoded returns | Build Application Services (Commands/Queries/EventHandlers), Infrastructure (Repository/ACL), and replace NullNtrQuery with ACL |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F850 | [DONE] | Predecessor chain -- NTR Value Objects (NtrRoute, NtrPhase, Susceptibility) |
| F851 | [DONE] | Predecessor chain -- NtrProgression Aggregate + Domain Events |
| F852 | [DONE] | Direct predecessor -- INtrCalculator Domain Service interface |
| F854 | [DRAFT] | Successor -- Post-Phase Review Phase 24 |
| F855 | [DRAFT] | Successor -- Phase 25 Planning |
| F819 | [DONE] | Related -- ClothingSystem introduced INtrQuery dependency |
| F829 | [DONE] | Related -- OB-03 deferred obligation for INtrQuery typed implementation |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Domain layer prerequisites complete | FEASIBLE | F850-F852 all [DONE]; all VOs, Aggregate, Events, INtrCalculator interface exist |
| Repository pattern established | FEASIBLE | `InMemoryRepository<T, TId>` base class + `CharacterRepository` provide reusable template |
| Command/Handler pattern exists | FEASIBLE | `ICommand<TResult>` and `ICommandHandler<TCommand, TResult>` patterns exist in Era.Core |
| DI registration pattern exists | FEASIBLE | `ServiceCollectionExtensions.cs` has 500+ lines of registration patterns to follow |
| INtrQuery consumers identified | FEASIBLE | Exactly 2 consumers: ClothingSystem (7 calls), PregnancyAnnouncement (4 calls) |
| ACL bridging is well-scoped | FEASIBLE | INtrQuery has single method `CheckNtrFavorably(CharacterId, int)` -- narrow interface |
| No external I/O dependency | FEASIBLE | All in-memory; no database, no file system access required |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| NTR Bounded Context | HIGH | Completes the DDD layered architecture (Application + Infrastructure on top of Domain) |
| INtrQuery consumers | HIGH | ClothingSystem and PregnancyAnnouncement will receive real NTR state via ACL instead of NullNtrQuery false |
| DI composition root | MEDIUM | ServiceCollectionExtensions.cs gains new registrations; NullNtrQuery replaced by ACL |
| IUnitOfWork | LOW | NOT modified -- NtrProgressionRepository uses standalone DI registration (bounded context isolation) |
| Existing tests | LOW | No existing tests broken; new tests needed for Application/Infrastructure layers |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| INtrCalculator is interface-only (no concrete implementation in Phase 24) | `INtrCalculator.cs:13` | Tests must use mock/stub INtrCalculator implementations |
| NtrProgression.AdvancePhase() is parameterless | `NtrProgression.cs:40-49` | Application Commands MUST call INtrCalculator.CanAdvance() before AdvancePhase() -- guard-then-mutate pattern |
| IUnitOfWork only has Characters property | `IUnitOfWork.cs:8` | NtrProgressionRepository uses standalone DI registration, NOT added to IUnitOfWork (bounded context isolation) |
| INtrQuery threshold is raw int | `INtrQuery.cs:19` | ACL must translate between typed NtrProgression VOs and raw int threshold comparisons |
| IRepository lacks CharacterId-based lookup | `IRepository.cs:5-14` | Repository needs custom GetByCharacterId method for ACL to find NtrProgression by character |
| No IQuery/IQueryHandler abstraction exists | Grep returned no matches | Queries use direct service pattern (not CQRS abstraction) |
| Phase 24 scope is interface/contract only | `INtrCalculator.cs:13` | Concrete INtrCalculator implementation belongs to Phase 25; ACL may use stub evaluation logic |
| NtrProgressionId is Guid-based | `NtrProgressionId.cs:3-10` | Satisfies IRepository<T, TId> where TId : struct constraint |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| ACL cannot map CharacterId to NtrProgression without custom repository query | HIGH | HIGH | Add GetByCharacterId method to NtrProgressionRepository; all 3 investigators identified this gap |
| ACL scope creep into real NTR evaluation logic (Phase 25 territory) | MEDIUM | HIGH | ACL must only translate INtrQuery calls to NtrProgression Aggregate reads; actual FAV evaluation belongs in Phase 25 concrete INtrCalculator |
| No concrete INtrCalculator for integration testing | HIGH | LOW | Tests use mock INtrCalculator; Phase 24 scope is interface/contract only |
| Event handler with no subscribers in Phase 24 | MEDIUM | LOW | NtrPhaseAdvancedHandler can be a minimal skeleton; subscribers connect in Phase 25 |
| IUnitOfWork modification breaks existing consumers | LOW | MEDIUM | Use separate IRepository<NtrProgression, NtrProgressionId> DI registration; do NOT extend IUnitOfWork |
| Query pattern design divergence (no IQuery abstraction) | MEDIUM | LOW | Use direct query service classes; CQRS abstraction is not required for Phase 24 scope |

---

## Baseline Measurement
<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| NTR Application files | `ls src/Era.Core/NTR/Application/` | 0 (directory does not exist) | Target: 5+ files |
| NTR Infrastructure files | `ls src/Era.Core/NTR/Infrastructure/` | 0 (directory does not exist) | Target: 2+ files |
| NullNtrQuery registration | `grep -c "NullNtrQuery" src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` | 1 | Target: 0 (replaced by ACL) |
| INtrQuery consumers | `grep -rc "CheckNtrFavorably" src/Era.Core/` | 11 (7 ClothingSystem + 4 PregnancyAnnouncement) | Should remain unchanged |

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Commands must follow ICommand/ICommandHandler pattern | `Era.Core/Commands/ICommand.cs`, `ICommandHandler.cs` | Verify command classes implement ICommand<TResult>, handlers implement ICommandHandler |
| C2 | ACL must implement INtrQuery interface | `INtrQuery.cs:10-20` | Verify ACL class implements INtrQuery and provides CheckNtrFavorably |
| C3 | Application Commands must call INtrCalculator before Aggregate mutation | `NtrProgression.cs:40-49` (parameterless AdvancePhase) | Verify command handler calls CanAdvance/CanChangeRoute before invoking Aggregate methods |
| C4 | Repository must extend InMemoryRepository pattern | `Infrastructure/InMemoryRepository.cs:7` | Verify NtrProgressionRepository extends InMemoryRepository<NtrProgression, NtrProgressionId> |
| C5 | Repository needs CharacterId-based lookup | ACL requires CharacterId-to-NtrProgression mapping | Verify GetByCharacterId method exists on repository |
| C6 | DI registration must replace NullNtrQuery with ACL | `ServiceCollectionExtensions.cs:115` | Verify NullNtrQuery registration removed and ACL registered as INtrQuery |
| C7 | No TODO/FIXME/HACK in deliverables | Standard quality constraint | Grep verification across NTR/ directory |
| C8 | Event handler for NtrPhaseAdvanced domain event | Architecture plan Task 8 | Verify NtrPhaseAdvancedHandler exists in EventHandlers/ |

### Constraint Details

**C1: ICommand/ICommandHandler Pattern**
- **Source**: Existing Command pattern in Era.Core (`ICommand.cs`, `ICommandHandler.cs`)
- **Verification**: Check that `ICommand<TResult>` and `ICommandHandler<TCommand, TResult>` interfaces exist and are implemented
- **AC Impact**: ACs must verify both Command record/class AND Handler class for each command
- **Collection Members**: 2 commands: AdvanceNtrPhaseCommand, ChangeNtrRouteCommand

**C2: INtrQuery ACL Implementation**
- **Source**: `INtrQuery.cs:10-20` defines single method `CheckNtrFavorably(CharacterId, int)`
- **Verification**: Grep ACL file for `: INtrQuery` or `implements INtrQuery`
- **AC Impact**: AC must verify ACL implements the interface AND that DI registration replaces NullNtrQuery

**C3: Guard-Then-Mutate Orchestration**
- **Source**: NtrProgression.AdvancePhase() at `NtrProgression.cs:40-49` has no INtrCalculator parameter
- **Verification**: Grep command handler for INtrCalculator.CanAdvance call before NtrProgression.AdvancePhase call
- **AC Impact**: Behavioral AC must verify that command handler rejects advancement when CanAdvance returns false

**C4: InMemoryRepository Extension**
- **Source**: `InMemoryRepository<T, TId>` at `Infrastructure/InMemoryRepository.cs:7` with ConcurrentDictionary storage
- **Verification**: Check NtrProgressionRepository inherits from InMemoryRepository
- **AC Impact**: Verify class hierarchy and generic type parameters

**C5: CharacterId Lookup**
- **Source**: ACL receives `CharacterId` from INtrQuery consumers but IRepository only supports NtrProgressionId
- **Verification**: Grep repository for GetByCharacterId or equivalent method
- **AC Impact**: AC must verify lookup method exists; this is the critical bridging mechanism

**C6: DI Registration Replacement**
- **Source**: `ServiceCollectionExtensions.cs:115` currently registers NullNtrQuery
- **Verification**: Grep for NullNtrQuery (should be absent) and ACL registration (should be present)
- **AC Impact**: Two ACs: one not_matches NullNtrQuery, one matches ACL registration

**C7: No Technical Debt**
- **Source**: Standard quality constraint — all NTR deliverables must be debt-free
- **Verification**: Grep across `NTR/` directory for `TODO|FIXME|HACK`
- **AC Impact**: AC#18 verifies no debt markers exist in NTR deliverable files

**C8: NtrPhaseAdvanced Event Handler**
- **Source**: Architecture plan Task 8; NtrPhaseAdvanced domain event defined in F851
- **Verification**: File exists in Application/EventHandlers/
- **AC Impact**: File existence AC; handler may be minimal skeleton in Phase 24

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F852 | [DONE] | INtrCalculator Domain Service interface -- Application Commands invoke `CanAdvance()` and `CanChangeRoute()` (CALL at command handler level) |
| Successor | F854 | [DRAFT] | Post-Phase Review Phase 24 -- reviews F850-F853 deliverables |
| Successor | F855 | [DRAFT] | Phase 25 Planning -- concrete implementations depend on F853 Application layer |
| Related | F851 | [DONE] | NtrProgression Aggregate -- Commands/Queries operate on this Aggregate |
| Related | F850 | [DONE] | NTR Value Objects -- Command/Query parameters use NtrRoute, NtrPhase, Susceptibility |
| Related | F819 | [DONE] | ClothingSystem introduced INtrQuery dependency pattern |
| Related | F829 | [DONE] | OB-03 deferred obligation -- INtrQuery typed implementation fulfilled by ACL |

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
| "completes the Phase 24 DDD layered architecture by providing the orchestration and persistence layers above the Domain" | Application Commands exist (orchestration layer) | AC#1, AC#2, AC#3, AC#4 |
| "completes the Phase 24 DDD layered architecture by providing the orchestration and persistence layers above the Domain" | Application Queries exist (read layer) | AC#5, AC#6 |
| "completes the Phase 24 DDD layered architecture by providing the orchestration and persistence layers above the Domain" | Infrastructure Repository exists (persistence layer) | AC#7, AC#8 |
| "completes the Phase 24 DDD layered architecture by providing the orchestration and persistence layers above the Domain" | Event handler exists (domain event layer) | AC#9 |
| "translate between the new DDD typed model and legacy ERB-era interfaces (INtrQuery)" | ACL implements INtrQuery | AC#10, AC#11, AC#24, AC#25 |
| "translate between the new DDD typed model and legacy ERB-era interfaces (INtrQuery)" | ACL replaces NullNtrQuery in DI | AC#12, AC#13 |
| "Application Services are the consumer layers that invoke the Domain Service (F852 INtrCalculator)" | Command handlers invoke INtrCalculator before Aggregate mutation | AC#14, AC#15, AC#22, AC#23 |
| "completes the Phase 24 DDD layered architecture by providing the orchestration and persistence layers above the Domain" | DI registration connects all layers | AC#12, AC#13, AC#16, AC#21 |
| "completes the Phase 24 DDD layered architecture by providing the orchestration and persistence layers above the Domain" | Minimum deliverable file count | AC#17 |
| "completes the Phase 24 DDD layered architecture by providing the orchestration and persistence layers above the Domain" | No technical debt in deliverables | AC#18 |
| "completes the Phase 24 DDD layered architecture by providing the orchestration and persistence layers above the Domain" | Solution builds and tests pass | AC#19, AC#20 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | AdvanceNtrPhaseCommand implements ICommand | code | Grep(C:\Era\core\src\Era.Core\NTR\Application\Commands\AdvanceNtrPhaseCommand.cs, pattern="ICommand<") | contains | `ICommand<` | [x] |
| 2 | AdvanceNtrPhaseHandler implements ICommandHandler | code | Grep(C:\Era\core\src\Era.Core\NTR\Application\Commands\AdvanceNtrPhaseHandler.cs, pattern="ICommandHandler<") | contains | `ICommandHandler<` | [x] |
| 3 | ChangeNtrRouteCommand implements ICommand | code | Grep(C:\Era\core\src\Era.Core\NTR\Application\Commands\ChangeNtrRouteCommand.cs, pattern="ICommand<") | contains | `ICommand<` | [x] |
| 4 | ChangeNtrRouteHandler implements ICommandHandler | code | Grep(C:\Era\core\src\Era.Core\NTR\Application\Commands\ChangeNtrRouteHandler.cs, pattern="ICommandHandler<") | contains | `ICommandHandler<` | [x] |
| 5 | GetNtrStatusQuery service exists | code | Grep(C:\Era\core\src\Era.Core\NTR\Application\Queries\GetNtrStatusQuery.cs, pattern="class GetNtrStatusQuery") | contains | `class GetNtrStatusQuery` | [x] |
| 6 | GetSusceptibilityQuery service exists | code | Grep(C:\Era\core\src\Era.Core\NTR\Application\Queries\GetSusceptibilityQuery.cs, pattern="class GetSusceptibilityQuery") | contains | `class GetSusceptibilityQuery` | [x] |
| 7 | NtrProgressionRepository extends InMemoryRepository | code | Grep(C:\Era\core\src\Era.Core\NTR\Infrastructure\NtrProgressionRepository.cs, pattern="InMemoryRepository<NtrProgression") | contains | `InMemoryRepository<NtrProgression` | [x] |
| 8 | NtrProgressionRepository has GetByCharacterId method | code | Grep(C:\Era\core\src\Era.Core\NTR\Infrastructure\NtrProgressionRepository.cs, pattern="GetByCharacterId") | contains | `GetByCharacterId` | [x] |
| 9 | NtrPhaseAdvancedHandler event handler exists | code | Grep(C:\Era\core\src\Era.Core\NTR\Application\EventHandlers\NtrPhaseAdvancedHandler.cs, pattern="class NtrPhaseAdvancedHandler") | contains | `class NtrPhaseAdvancedHandler` | [x] |
| 10 | ACL implements INtrQuery interface | code | Grep(C:\Era\core\src\Era.Core\NTR\Infrastructure\NtrQueryAcl.cs, pattern=": INtrQuery") | contains | `: INtrQuery` | [x] |
| 11 | ACL has CheckNtrFavorably method | code | Grep(C:\Era\core\src\Era.Core\NTR\Infrastructure\NtrQueryAcl.cs, pattern="CheckNtrFavorably") | contains | `CheckNtrFavorably` | [x] |
| 12 | NullNtrQuery registration removed from DI | code | Grep(C:\Era\core\src\Era.Core\DependencyInjection\ServiceCollectionExtensions.cs, pattern="NullNtrQuery") | not_contains | `NullNtrQuery` | [x] |
| 13 | ACL registered as INtrQuery in DI | code | Grep(C:\Era\core\src\Era.Core\DependencyInjection\ServiceCollectionExtensions.cs, pattern="NtrQueryAcl") | contains | `NtrQueryAcl` | [x] |
| 14 | AdvanceNtrPhaseHandler calls INtrCalculator.CanAdvance | code | Grep(C:\Era\core\src\Era.Core\NTR\Application\Commands\AdvanceNtrPhaseHandler.cs, pattern="CanAdvance") | contains | `CanAdvance` | [x] |
| 15 | ChangeNtrRouteHandler calls INtrCalculator.CanChangeRoute | code | Grep(C:\Era\core\src\Era.Core\NTR\Application\Commands\ChangeNtrRouteHandler.cs, pattern="CanChangeRoute") | contains | `CanChangeRoute` | [x] |
| 16 | DI registers NtrProgressionRepository | code | Grep(C:\Era\core\src\Era.Core\DependencyInjection\ServiceCollectionExtensions.cs, pattern="NtrProgressionRepository") | contains | `NtrProgressionRepository` | [x] |
| 17 | NTR Application/Infrastructure/Domain-Repositories file count | file | Glob(C:\Era\core\src\Era.Core\NTR\Application\**\*.cs,C:\Era\core\src\Era.Core\NTR\Infrastructure\**\*.cs,C:\Era\core\src\Era.Core\NTR\Domain\Repositories\**\*.cs) | gte | 7 | [x] |
| 18 | No TODO/FIXME/HACK in NTR deliverables | code | Grep(C:\Era\core\src\Era.Core\NTR\, pattern="TODO|FIXME|HACK") | not_matches | `TODO|FIXME|HACK` | [x] |
| 19 | Era.Core solution builds successfully | build | dotnet build C:\Era\core\src\Era.Core\Era.Core.csproj | succeeds | - | [x] |
| 20 | NTR Application/Infrastructure tests pass | test | dotnet test C:\Era\core\src\Era.Core.Tests\ --filter "FullyQualifiedName~NTR" --blame-hang-timeout 10s | succeeds | - | [x] |
| 21 | DI registers all Application/Infrastructure services | code | Grep(C:\Era\core\src\Era.Core\DependencyInjection\ServiceCollectionExtensions.cs, pattern="ICommandHandler.*AdvanceNtrPhase\|ICommandHandler.*ChangeNtrRoute\|GetNtrStatusQuery\|GetSusceptibilityQuery\|NtrPhaseAdvancedHandler") | count_equals | 5 | [x] |
| 22 | AdvanceNtrPhaseHandler test verifies guard rejection | code | Grep(C:\Era\core\src\Era.Core.Tests\NTR\Application\Commands\AdvanceNtrPhaseHandlerTests.cs, pattern="CanAdvance.*false\|Fail\|Eligibility\|rejected\|ineligible") | contains | CanAdvance.*false | [x] |
| 23 | ChangeNtrRouteHandler test verifies guard rejection | code | Grep(C:\Era\core\src\Era.Core.Tests\NTR\Application\Commands\ChangeNtrRouteHandlerTests.cs, pattern="CanChangeRoute.*false\|Fail\|Eligibility\|rejected\|ineligible") | contains | CanChangeRoute.*false | [x] |
| 24 | NtrQueryAcl test verifies unknown character returns false | code | Grep(C:\Era\core\src\Era.Core.Tests\NTR\Infrastructure\NtrQueryAclTests.cs, pattern="false.*unknown\|not found\|no progression\|unknown.*false") | contains | no progression | [x] |
| 25 | NtrQueryAcl test verifies threshold comparison | code | Grep(C:\Era\core\src\Era.Core.Tests\NTR\Infrastructure\NtrQueryAclTests.cs, pattern="SubmissionDegree\|threshold.*true\|above.*threshold\|below.*threshold") | contains | SubmissionDegree | [x] |

### AC Details

**AC#17: NTR Application/Infrastructure/Domain-Repositories file count**
- **Test**: `Glob(C:\Era\core\src\Era.Core\NTR\Application\**\*.cs,C:\Era\core\src\Era.Core\NTR\Infrastructure\**\*.cs,C:\Era\core\src\Era.Core\NTR\Domain\Repositories\**\*.cs)`
- **Expected**: `gte 7`
- **Derivation**: 7 new files minimum: AdvanceNtrPhaseCommand.cs (1) + AdvanceNtrPhaseHandler.cs (2) + ChangeNtrRouteCommand.cs (3) + ChangeNtrRouteHandler.cs (4) + GetNtrStatusQuery.cs (5) + GetSusceptibilityQuery.cs (6) + NtrProgressionRepository.cs (7). Additional files: NtrPhaseAdvancedHandler.cs (8), INtrProgressionRepository.cs (9, in Domain/Repositories/ per DIP), NtrQueryAcl.cs (10). Conservative floor = 7 to allow design flexibility for file organization. Glob includes `NTR/Domain/Repositories/` because INtrProgressionRepository.cs is a new deliverable of this feature placed in the Domain layer per DIP.
- **Rationale**: Ensures the Application, Infrastructure, and Domain/Repositories layers contain a minimum set of deliverables as specified in Goal. Constraint C1 (2 commands with handlers = 4 files), C4/C5 (1 repository interface + 1 implementation), C2 (1 ACL), C8 (1 event handler) = 7 minimum.

**AC#18: No TODO/FIXME/HACK in NTR deliverables**
- **Test**: `Grep(C:\Era\core\src\Era.Core\NTR\, pattern="TODO|FIXME|HACK")`
- **Expected**: `not_matches TODO|FIXME|HACK`
- **Vacuousness Note**: The target path `NTR/` currently contains only Domain layer files (F850-F852) which have zero TODO/FIXME/HACK markers. The Application/ and Infrastructure/ directories do not yet exist. This AC becomes meaningful post-implementation when new files are created — it prevents debt markers from being introduced in the new deliverables. The AC is intentionally scoped to the entire `NTR/` directory (not just Application/Infrastructure) to also guard against debt regression in existing Domain files.
- **V3 Exception**: Accepted as post-implementation regression guard. Cannot be in RED state pre-implementation because the target directory contains no debt markers. This is inherent to quality-gate ACs for new file creation features.

**AC#21: DI registers all Application/Infrastructure services**
- **Test**: `Grep(ServiceCollectionExtensions.cs, pattern="ICommandHandler.*AdvanceNtrPhase|ICommandHandler.*ChangeNtrRoute|GetNtrStatusQuery|GetSusceptibilityQuery|NtrPhaseAdvancedHandler")`
- **Expected**: `count_equals 5`
- **Derivation**: 5 services: `ICommandHandler<AdvanceNtrPhaseCommand, Unit>, AdvanceNtrPhaseHandler` (1) + `ICommandHandler<ChangeNtrRouteCommand, Unit>, ChangeNtrRouteHandler` (2) + GetNtrStatusQuery (3) + GetSusceptibilityQuery (4) + NtrPhaseAdvancedHandler (5). Command handlers registered via ICommandHandler interface (not concrete type) for proper DI resolution. AC#13 covers NtrQueryAcl separately; AC#16 covers NtrProgressionRepository separately.

**AC#22: AdvanceNtrPhaseHandler test verifies guard rejection**
- **Test**: `Grep(C:\Era\core\src\Era.Core.Tests\NTR\Application\Commands\AdvanceNtrPhaseHandlerTests.cs, pattern="CanAdvance.*false|Fail|Eligibility|rejected|ineligible")`
- **Expected**: CanAdvance.*false
- **Derivation**: AC Design Constraint C3 requires guard-then-mutate orchestration. AC#14 verifies the production code calls `CanAdvance`, but does not verify the handler actually rejects when `CanAdvance` returns false. AC#22 ensures a test exists that exercises the guard-rejection path, confirming behavioral correctness beyond structural presence.

**AC#23: ChangeNtrRouteHandler test verifies guard rejection**
- **Test**: `Grep(C:\Era\core\src\Era.Core.Tests\NTR\Application\Commands\ChangeNtrRouteHandlerTests.cs, pattern="CanChangeRoute.*false|Fail|Eligibility|rejected|ineligible")`
- **Expected**: CanChangeRoute.*false
- **Derivation**: Same rationale as AC#22 but for ChangeNtrRouteHandler. Ensures the guard-then-mutate pattern (AC Design Constraint C3) is behaviorally tested, not just structurally present (AC#15).

**AC#24: NtrQueryAcl test verifies unknown character returns false**
- **Test**: `Grep(C:\Era\core\src\Era.Core.Tests\NTR\Infrastructure\NtrQueryAclTests.cs, pattern="false.*unknown|not found|no progression|unknown.*false")`
- **Expected**: no progression
- **Derivation**: NtrQueryAcl.CheckNtrFavorably must return false for unknown characters (safe default preserving NullNtrQuery behavior). AC#11 verifies CheckNtrFavorably exists structurally but does not verify the unknown-character behavioral path. This AC ensures a test exercises the not-found case where GetByCharacterId fails.

**AC#25: NtrQueryAcl test verifies threshold comparison**
- **Test**: `Grep(C:\Era\core\src\Era.Core.Tests\NTR\Infrastructure\NtrQueryAclTests.cs, pattern="SubmissionDegree|threshold.*true|above.*threshold|below.*threshold")`
- **Expected**: SubmissionDegree
- **Derivation**: NtrQueryAcl uses `SubmissionDegree >= threshold` as the Phase 24 heuristic (Key Decision: B selected). AC#11 verifies CheckNtrFavorably exists but does not verify the threshold comparison logic. This AC ensures a test exercises the comparison path, confirming the ACL correctly translates between the legacy int threshold and the typed Susceptibility model.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | AdvanceNtrPhaseCommand and ChangeNtrRouteCommand orchestrating INtrCalculator checks before Aggregate mutations | AC#1, AC#2, AC#3, AC#4, AC#14, AC#15, AC#22, AC#23 |
| 2 | GetNtrStatusQuery and GetSusceptibilityQuery reading NtrProgression Aggregate state | AC#5, AC#6 |
| 3 | NtrPhaseAdvancedHandler for NtrPhaseAdvanced domain event | AC#9 |
| 4 | NtrProgressionRepository extending InMemoryRepository with CharacterId-based lookup | AC#7, AC#8 |
| 5 | ACL implementing INtrQuery to bridge legacy consumers to NtrProgression Aggregate, replacing NullNtrQuery | AC#10, AC#11, AC#24, AC#25 |
| 6 | DI Registration: Register all new services, replacing NullNtrQuery with ACL | AC#12, AC#13, AC#16, AC#21 |
| 7 | Minimum deliverable file count for Application/Infrastructure layers | AC#17 |
| 8 | No technical debt (TODO/FIXME/HACK) in NTR deliverables | AC#18 |
| 9 | Era.Core solution builds successfully | AC#19 |
| 10 | NTR Application/Infrastructure tests pass | AC#20 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer -->

### Approach

The feature introduces three new directory trees in `Era.Core/NTR/`: `Application/Commands/`, `Application/Queries/`, `Application/EventHandlers/`, and `Infrastructure/`. This follows the DDD layered architecture pattern already used in the broader Era.Core design.

**Implementation structure (10 files):**

_Application/Commands_ (4 files): Two command/handler pairs using the existing `ICommand<TResult>` / `ICommandHandler<TCommand, TResult>` contract. Each handler receives an `INtrCalculator` and an `INtrProgressionRepository` via constructor injection. The guard-then-mutate pattern is: (1) load aggregate from repository via `GetByCharacterId()`, (2) call `INtrCalculator.CanAdvance()` or `CanChangeRoute()`, (3) if ineligible return a failure Result, (4) invoke the aggregate mutation method, (5) persist via repository `Update()`.

_Application/Queries_ (2 files): Two standalone query service classes (not CQRS IQuery abstraction -- none exists) that accept `INtrProgressionRepository` and return read-only aggregate state. `GetNtrStatusQuery` returns current phase and route; `GetSusceptibilityQuery` returns the `Susceptibility` value object.

_Application/EventHandlers_ (1 file): `NtrPhaseAdvancedHandler` is a minimal skeleton that accepts `NtrPhaseAdvanced` domain events. No subscribers connect in Phase 24 scope; the handler exists to anchor the extension point for Phase 25.

_Domain/Repositories/INtrProgressionRepository_ (1 file): Domain-specific repository interface extending `IRepository<NtrProgression, NtrProgressionId>` with `GetByCharacterId(CharacterId)` method. Lives in Domain layer per DIP — abstractions must be in the inner layer (Domain), not outer (Infrastructure). Eliminates duplicated `GetAll().Where(p => p.TargetCharacter == ...)` logic across 4 consumers (2 handlers + 2 queries).

_Infrastructure/NtrProgressionRepository_ (1 file): Extends `InMemoryRepository<NtrProgression, NtrProgressionId>` and implements `INtrProgressionRepository`. Adds `GetByCharacterId(CharacterId characterId)` method that calls `GetAll()` and uses LINQ `FirstOrDefault` to find the progression whose `TargetCharacter` matches. Returns `Result<NtrProgression>` (Ok if found, Fail if not found).

_Infrastructure/NtrQueryAcl_ (1 file): Implements `INtrQuery`. Constructor injects `INtrProgressionRepository` (domain-specific interface with `GetByCharacterId` access). In `CheckNtrFavorably(CharacterId character, int threshold)`: call `GetByCharacterId(character)`, if not found return `false` (safe default preserving NullNtrQuery behavior for unknown characters), if found compare `progression.CurrentSusceptibility` against the raw `threshold`. Since the concrete `INtrCalculator` implementation belongs to Phase 25, the ACL uses a direct susceptibility-based comparison: `progression.CurrentSusceptibility.SubmissionDegree >= threshold` as the bridging heuristic. This is the ACL scope: translate the legacy `int threshold` query into typed aggregate reads -- actual FAV evaluation stays with Phase 25.

_DI update_: `ServiceCollectionExtensions.cs` line 115 replaces `services.AddSingleton<INtrQuery, NullNtrQuery>()` with `services.AddSingleton<NtrProgressionRepository>()`, `services.AddSingleton<INtrProgressionRepository>(...)` (factory delegate), and `services.AddSingleton<INtrQuery, NtrQueryAcl>()`. `IRepository<NtrProgression, NtrProgressionId>` and `INtrProgressionRepository` are registered separately via factory delegates (NOT added to `IUnitOfWork`).

**How ACs are satisfied:** ACs#1-4 verify command/handler file content; ACs#5-6 verify query files; AC#7-8 verify repository class, INtrProgressionRepository implementation, and `GetByCharacterId`; AC#9 verifies event handler; ACs#10-11 verify ACL interface implementation; ACs#12-13 verify DI swap; AC#14-15 verify guard calls in handlers; AC#16 verifies repository DI; AC#17 counts 10 deliverable files (gte 7); AC#18 verifies no debt markers; ACs#19-20 verify build and test pass; ACs#24-25 verify ACL behavioral tests.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | `AdvanceNtrPhaseCommand` record declares `ICommand<Unit>` in class declaration; Grep finds `ICommand<` |
| 2 | `AdvanceNtrPhaseHandler` class implements `ICommandHandler<AdvanceNtrPhaseCommand, Unit>`; Grep finds `ICommandHandler<` |
| 3 | `ChangeNtrRouteCommand` record declares `ICommand<Unit>`; Grep finds `ICommand<` |
| 4 | `ChangeNtrRouteHandler` class implements `ICommandHandler<ChangeNtrRouteCommand, Unit>`; Grep finds `ICommandHandler<` |
| 5 | `GetNtrStatusQuery.cs` file contains `class GetNtrStatusQuery`; Grep finds `class GetNtrStatusQuery` |
| 6 | `GetSusceptibilityQuery.cs` file contains `class GetSusceptibilityQuery`; Grep finds `class GetSusceptibilityQuery` |
| 7 | `NtrProgressionRepository` class body has `: InMemoryRepository<NtrProgression,` and `, INtrProgressionRepository` in its declaration; Grep finds `InMemoryRepository<NtrProgression` |
| 8 | `NtrProgressionRepository` contains method `GetByCharacterId` (implementing `INtrProgressionRepository`); Grep finds `GetByCharacterId` |
| 9 | `NtrPhaseAdvancedHandler.cs` file contains `class NtrPhaseAdvancedHandler`; Grep finds `class NtrPhaseAdvancedHandler` |
| 10 | `NtrQueryAcl` class declaration includes `: INtrQuery`; Grep finds `: INtrQuery` |
| 11 | `NtrQueryAcl` contains method `CheckNtrFavorably`; Grep finds `CheckNtrFavorably` |
| 12 | `ServiceCollectionExtensions.cs` no longer contains `NullNtrQuery`; not_contains check passes |
| 13 | `ServiceCollectionExtensions.cs` contains `NtrQueryAcl`; Grep finds `NtrQueryAcl` |
| 14 | `AdvanceNtrPhaseHandler.Handle()` calls `_calculator.CanAdvance(progression)` before invoking `AdvancePhase()`; Grep finds `CanAdvance` |
| 15 | `ChangeNtrRouteHandler.Handle()` calls `_calculator.CanChangeRoute(progression, command.TargetRoute)` before invoking `ChangeRoute()`; Grep finds `CanChangeRoute` |
| 16 | `ServiceCollectionExtensions.cs` contains `NtrProgressionRepository` in a registration call; Grep finds `NtrProgressionRepository` |
| 17 | 10 new `.cs` files created across `NTR/Application/`, `NTR/Infrastructure/`, and `NTR/Domain/Repositories/` (INtrProgressionRepository.cs in Domain/Repositories/ per DIP); gte 7 passes |
| 18 | All new files authored without TODO/FIXME/HACK comments; not_matches passes |
| 19 | All new files reference valid namespaces, interfaces, and types; `dotnet build` exits 0 |
| 20 | New xUnit tests cover command handler guard logic and repository lookup; `dotnet test --filter NTR` exits 0 |
| 21 | `ServiceCollectionExtensions.cs` contains `AddSingleton` for all 5 Application/EventHandler/Query services via ICommandHandler interface registration for command handlers; count_equals 5 |
| 22 | `AdvanceNtrPhaseHandlerTests.cs` contains test pattern verifying guard rejection when `CanAdvance` returns false; Grep finds guard-rejection test pattern |
| 23 | `ChangeNtrRouteHandlerTests.cs` contains test for guard rejection when `CanChangeRoute()` returns false; Grep finds rejection test pattern |
| 24 | `NtrQueryAclTests.cs` contains test verifying unknown character returns false (safe default); Grep finds unknown-character-false test pattern |
| 25 | `NtrQueryAclTests.cs` contains test verifying SubmissionDegree threshold comparison logic; Grep finds threshold-comparison test pattern |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Query pattern (IQuery abstraction vs direct service class) | A: CQRS IQuery/IQueryHandler abstraction, B: Direct query service class | B: Direct query service class | No IQuery abstraction exists in Era.Core (Grep confirmed). Adding one is Phase 25+ territory. Direct query class is simpler and consistent with existing patterns. |
| ACL threshold bridging heuristic | A: Always return false (NullNtrQuery behavior), B: SubmissionDegree >= threshold comparison, C: Delegate to INtrCalculator | B: SubmissionDegree >= threshold | Option A defeats the ACL purpose. Option C requires a concrete INtrCalculator (Phase 25). Option B uses the available typed Susceptibility state for a meaningful Phase 24 heuristic; actual FAV evaluation stays in Phase 25. |
| Repository DI registration granularity | A: Add NtrProgression to IUnitOfWork, B: Register IRepository<NtrProgression, NtrProgressionId> + NtrProgressionRepository separately | B: Separate DI registration | Technical Constraint: IUnitOfWork only has Characters property; bounded context isolation requires NOT adding to IUnitOfWork. NtrQueryAcl uses INtrProgressionRepository for GetByCharacterId; also register as IRepository<NtrProgression, NtrProgressionId> for handler injection. |
| NtrPhaseAdvancedHandler completeness | A: Full subscriber logic (Phase 25 behavior), B: Minimal skeleton with structured logging stub | B: Minimal skeleton | Phase 24 scope is interface/contract only. Handler must exist for AC#9 but actual subscribers belong to Phase 25. Skeleton avoids Phase 25 scope creep. |
| GetByCharacterId return type | A: `NtrProgression?` (nullable), B: `Result<NtrProgression>` | B: `Result<NtrProgression>` | Consistent with InMemoryRepository.GetById return type (Result<T>). ACL caller can check result success; fail-loud pattern preferred over nullable. |
| Command result type | A: `Unit`, B: `bool`, C: `CommandResult` record | A: `Unit` | Consistent with existing ICommand pattern; Result wrapping provided by ICommandHandler return type. |
| Handler repository dependency | A: `IRepository<NtrProgression, NtrProgressionId>` (generic interface), B: `INtrProgressionRepository` (domain-specific interface with GetByCharacterId) | B: `INtrProgressionRepository` | Eliminates duplicated CharacterId lookup logic across 4 consumers (2 handlers + 2 queries). ACL already uses concrete NtrProgressionRepository for GetByCharacterId; handlers and queries should have the same capability via an interface for DI testability. |
| INtrProgressionRepository layer placement | A: Infrastructure (alongside implementation), B: Domain/Repositories (per DIP — interfaces in Domain) | B: Domain/Repositories | IRepository<T,TId> is in Era.Core.Domain. Application-layer handlers/queries depend on INtrProgressionRepository. Per DIP, abstractions must be in the inner layer (Domain), not outer (Infrastructure). Placing interface in Infrastructure would create Application→Infrastructure dependency violating DDD layered architecture. |

### Interfaces / Data Structures

```csharp
// Application/Commands/AdvanceNtrPhaseCommand.cs
namespace Era.Core.NTR.Application.Commands;

/// <summary>Command to advance NTR progression to the next phase for a character.</summary>
public record AdvanceNtrPhaseCommand(CharacterId TargetCharacter) : ICommand<Unit>
{
    public CommandId Id { get; } = CommandId.None;
}

// Application/Commands/AdvanceNtrPhaseHandler.cs
namespace Era.Core.NTR.Application.Commands;

public sealed class AdvanceNtrPhaseHandler
    : ICommandHandler<AdvanceNtrPhaseCommand, Unit>
{
    private readonly INtrCalculator _calculator;
    private readonly INtrProgressionRepository _repository;

    public AdvanceNtrPhaseHandler(INtrCalculator calculator,
        INtrProgressionRepository repository)
    {
        _calculator = calculator;
        _repository = repository;
    }

    public Task<Result<Unit>> Handle(AdvanceNtrPhaseCommand command, CancellationToken ct)
    {
        // Guard-then-mutate: load via GetByCharacterId, check eligibility, mutate, persist
        var lookupResult = _repository.GetByCharacterId(command.TargetCharacter);
        if (!lookupResult.IsSuccess)
            return Task.FromResult(Result<Unit>.Fail("NtrProgression not found"));

        var progression = lookupResult.Value;
        if (!_calculator.CanAdvance(progression))
            return Task.FromResult(Result<Unit>.Fail("Eligibility check failed"));

        var result = progression.AdvancePhase();
        _repository.Update(progression);
        return Task.FromResult(result);
    }
}

// Application/Commands/ChangeNtrRouteCommand.cs
public record ChangeNtrRouteCommand(CharacterId TargetCharacter, NtrRoute TargetRoute)
    : ICommand<Unit>
{
    public CommandId Id { get; } = CommandId.None;
}

// Application/Commands/ChangeNtrRouteHandler.cs
public sealed class ChangeNtrRouteHandler
    : ICommandHandler<ChangeNtrRouteCommand, Unit>
{
    // Constructor injects INtrCalculator + INtrProgressionRepository
    // Guard: _repository.GetByCharacterId(command.TargetCharacter), then _calculator.CanChangeRoute(progression, command.TargetRoute) before ChangeRoute()
}

// Application/Queries/GetNtrStatusQuery.cs
namespace Era.Core.NTR.Application.Queries;

/// <summary>Read-only query for NTR phase and route status.</summary>
public sealed class GetNtrStatusQuery
{
    private readonly INtrProgressionRepository _repository;

    public GetNtrStatusQuery(INtrProgressionRepository repository)
        => _repository = repository;

    /// <summary>Returns (CurrentPhase, CurrentRoute) or null if character has no progression.</summary>
    public (NtrPhase Phase, NtrRoute Route)? Execute(CharacterId character)
    {
        var result = _repository.GetByCharacterId(character);
        return !result.IsSuccess ? null : (result.Value.CurrentPhase, result.Value.CurrentRoute);
    }
}

// Application/Queries/GetSusceptibilityQuery.cs
/// <summary>Read-only query for Susceptibility state.</summary>
public sealed class GetSusceptibilityQuery
{
    private readonly INtrProgressionRepository _repository;

    public GetSusceptibilityQuery(INtrProgressionRepository repository)
        => _repository = repository;

    public Susceptibility? Execute(CharacterId character)
    {
        var result = _repository.GetByCharacterId(character);
        return !result.IsSuccess ? null : result.Value.CurrentSusceptibility;
    }
}

// Application/EventHandlers/NtrPhaseAdvancedHandler.cs
namespace Era.Core.NTR.Application.EventHandlers;

/// <summary>
/// Skeleton handler for NtrPhaseAdvanced domain event.
/// Phase 24: no subscribers. Phase 25 connects cross-context reactions here.
/// </summary>
public sealed class NtrPhaseAdvancedHandler
{
    public void Handle(NtrPhaseAdvanced domainEvent)
    {
        // Phase 25: add subscribers (stat updates, logging, notification)
    }
}

// Domain/Repositories/INtrProgressionRepository.cs
namespace Era.Core.NTR.Domain.Repositories;

public interface INtrProgressionRepository : IRepository<NtrProgression, NtrProgressionId>
{
    Result<NtrProgression> GetByCharacterId(CharacterId characterId);
}

// Infrastructure/NtrProgressionRepository.cs
using Era.Core.NTR.Domain.Repositories;

namespace Era.Core.NTR.Infrastructure;

/// <summary>In-memory repository for NtrProgression aggregates with CharacterId-based lookup.</summary>
public sealed class NtrProgressionRepository
    : InMemoryRepository<NtrProgression, NtrProgressionId>, INtrProgressionRepository
{
    public Result<NtrProgression> GetByCharacterId(CharacterId characterId)
    {
        var match = GetAll().FirstOrDefault(p => p.TargetCharacter == characterId);
        return match != default
            ? Result<NtrProgression>.Ok(match)
            : Result<NtrProgression>.Fail($"No NtrProgression found for character {characterId.Value}");
    }
}

// Infrastructure/NtrQueryAcl.cs
using Era.Core.NTR.Domain.Repositories;

namespace Era.Core.NTR.Infrastructure;

/// <summary>
/// Anti-Corruption Layer: bridges legacy INtrQuery consumers (ClothingSystem, PregnancyAnnouncement)
/// to the NtrProgression Aggregate. Replaces NullNtrQuery in DI composition root.
/// Phase 24 heuristic: SubmissionDegree >= threshold. Phase 25 delegates to INtrCalculator.
/// </summary>
public sealed class NtrQueryAcl : INtrQuery
{
    private readonly INtrProgressionRepository _repository;

    public NtrQueryAcl(INtrProgressionRepository repository) => _repository = repository;

    public bool CheckNtrFavorably(CharacterId character, int threshold)
    {
        var result = _repository.GetByCharacterId(character);
        if (!result.IsSuccess)
            return false; // safe default: unknown character = not favorable

        return result.Value.CurrentSusceptibility.SubmissionDegree >= threshold;
    }
}
```

**DI registration changes in `ServiceCollectionExtensions.cs`:**
```csharp
// Remove (line 115):
// services.AddSingleton<INtrQuery, NullNtrQuery>();

// Add:
services.AddSingleton<NtrProgressionRepository>();
services.AddSingleton<IRepository<NtrProgression, NtrProgressionId>>(
    sp => sp.GetRequiredService<NtrProgressionRepository>());
services.AddSingleton<INtrProgressionRepository>(
    sp => sp.GetRequiredService<NtrProgressionRepository>());
services.AddSingleton<INtrQuery, NtrQueryAcl>();
services.AddSingleton<ICommandHandler<AdvanceNtrPhaseCommand, Unit>, AdvanceNtrPhaseHandler>();
services.AddSingleton<ICommandHandler<ChangeNtrRouteCommand, Unit>, ChangeNtrRouteHandler>();
services.AddSingleton<GetNtrStatusQuery>();
services.AddSingleton<GetSusceptibilityQuery>();
services.AddSingleton<NtrPhaseAdvancedHandler>();
```

**Method Ownership:**

| Method | Owner Class | Domain Rationale |
|--------|-------------|-----------------|
| `GetByCharacterId(CharacterId)` | `INtrProgressionRepository (implemented by NtrProgressionRepository)` | Repository domain: aggregate lookup by alternate key |
| `CheckNtrFavorably(CharacterId, int)` | `NtrQueryAcl` | ACL domain: INtrQuery contract fulfillment |
| `Execute(CharacterId)` (status) | `GetNtrStatusQuery` | Query domain: read-only phase/route projection |
| `Execute(CharacterId)` (susceptibility) | `GetSusceptibilityQuery` | Query domain: read-only susceptibility projection |
| `Handle(AdvanceNtrPhaseCommand, CancellationToken)` | `AdvanceNtrPhaseHandler` | Command domain: guard-then-mutate orchestration |
| `Handle(ChangeNtrRouteCommand, CancellationToken)` | `ChangeNtrRouteHandler` | Command domain: guard-then-mutate orchestration |
| `Handle(NtrPhaseAdvanced)` | `NtrPhaseAdvancedHandler` | Event domain: domain event extension point |

No method appears in two classes. No cross-class method dependency conflicts.

### Upstream Issues

<!-- Optional: Issues discovered during design that require upstream changes (AC gaps, constraint gaps, interface gaps).
     Orchestrator reads this section after Phase 4 and dispatches micro-revisions if needed. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| `IRepository<NtrProgression, NtrProgressionId>` registration in DI will produce two singleton registrations (direct `NtrProgressionRepository` + the interface mapping) unless a factory delegate pattern is used. AC#16 only checks for `NtrProgressionRepository` string in ServiceCollectionExtensions -- implementation detail is safe to resolve during /run. | Technical Constraints | Implementer should register NtrProgressionRepository as concrete, then resolve via factory for IRepository interface. AC#16 is unaffected. |
| `CommandId` is `int`-based with no `New()` factory (`CommandId.cs` verified). Code stubs use `CommandId.None`. Implementer should verify whether the NTR command domain requires distinguishable IDs or whether `CommandId.None` is acceptable. | Interfaces / Data Structures | Implementer to confirm `CommandId.None` is acceptable for NTR Application Commands. No AC change required. |

---

<!-- fc-phase-5-completed -->
## Tasks

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. N ACs : 1 Task allowed. No orphan Tasks. -->

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2, 3, 4, 14, 15 | Create Application Commands layer in `C:\Era\core\src\Era.Core\NTR\Application\Commands\`: `AdvanceNtrPhaseCommand.cs` (record implementing `ICommand<Unit>`), `AdvanceNtrPhaseHandler.cs` (implements `ICommandHandler<AdvanceNtrPhaseCommand, Unit>`, constructor-injects `INtrCalculator` and `INtrProgressionRepository`, uses `GetByCharacterId()` instead of `GetAll().Where()`, calls `CanAdvance()` before `AdvancePhase()`), `ChangeNtrRouteCommand.cs` (record implementing `ICommand<Unit>`), `ChangeNtrRouteHandler.cs` (implements `ICommandHandler<ChangeNtrRouteCommand, Unit>`, constructor-injects `INtrProgressionRepository`, uses `GetByCharacterId()`, calls `CanChangeRoute()` before `ChangeRoute()`). Guard-then-mutate pattern: load aggregate via `GetByCharacterId`, check eligibility, mutate, persist. `CommandId.None` used for command IDs. | | [x] |
| 2 | 5, 6 | Create Application Queries layer in `C:\Era\core\src\Era.Core\NTR\Application\Queries\`: `GetNtrStatusQuery.cs` (direct service class, NOT CQRS IQuery abstraction — none exists; constructor-injects `INtrProgressionRepository`, `Execute(CharacterId)` uses `GetByCharacterId()` and returns `(NtrPhase Phase, NtrRoute Route)?`), `GetSusceptibilityQuery.cs` (constructor-injects `INtrProgressionRepository`, `Execute(CharacterId)` uses `GetByCharacterId()` and returns `Susceptibility?`). Both use `INtrProgressionRepository.GetByCharacterId()` instead of duplicated `GetAll().FirstOrDefault()` LINQ pattern. | | [x] |
| 3 | 9 | Create Application EventHandlers layer in `C:\Era\core\src\Era.Core\NTR\Application\EventHandlers\NtrPhaseAdvancedHandler.cs`: minimal skeleton class `NtrPhaseAdvancedHandler` with `Handle(NtrPhaseAdvanced domainEvent)` method containing no-op body. Phase 24 scope only — no subscribers; handler is an extension point for Phase 25. | | [x] |
| 4 | 7, 8 | Create Repository interface and implementation: (a) `C:\Era\core\src\Era.Core\NTR\Domain\Repositories\INtrProgressionRepository.cs` — interface extending `IRepository<NtrProgression, NtrProgressionId>` with `GetByCharacterId(CharacterId)` returning `Result<NtrProgression>`. Per DIP, interface lives in Domain layer. (b) `C:\Era\core\src\Era.Core\NTR\Infrastructure\NtrProgressionRepository.cs` — sealed class extending `InMemoryRepository<NtrProgression, NtrProgressionId>` and implementing `INtrProgressionRepository`, adds `GetByCharacterId(CharacterId characterId)` method returning `Result<NtrProgression>` using `GetAll().FirstOrDefault()` with `TargetCharacter` equality. Returns `Result<NtrProgression>.Fail(...)` if no match found (fail-loud, not nullable). | | [x] |
| 5 | 10, 11 | Create Infrastructure ACL in `C:\Era\core\src\Era.Core\NTR\Infrastructure\NtrQueryAcl.cs`: sealed class implementing `INtrQuery`, constructor-injects `INtrProgressionRepository` (domain-specific interface for `GetByCharacterId` access), `CheckNtrFavorably(CharacterId character, int threshold)` calls `GetByCharacterId(character)` and returns `false` if not found (safe default preserving NullNtrQuery behavior for unknown characters), else returns `progression.CurrentSusceptibility.SubmissionDegree >= threshold` (Phase 24 heuristic; actual FAV evaluation deferred to Phase 25). | | [x] |
| 6 | 12, 13, 16, 21 | Update DI registration in `C:\Era\core\src\Era.Core\DependencyInjection\ServiceCollectionExtensions.cs`: remove `services.AddSingleton<INtrQuery, NullNtrQuery>()` (line 115), add `services.AddSingleton<NtrProgressionRepository>()`, `services.AddSingleton<IRepository<NtrProgression, NtrProgressionId>>(sp => sp.GetRequiredService<NtrProgressionRepository>())`, `services.AddSingleton<INtrProgressionRepository>(sp => sp.GetRequiredService<NtrProgressionRepository>())`, `services.AddSingleton<INtrQuery, NtrQueryAcl>()`, plus ICommandHandler interface registrations for `ICommandHandler<AdvanceNtrPhaseCommand, Unit>, AdvanceNtrPhaseHandler` and `ICommandHandler<ChangeNtrRouteCommand, Unit>, ChangeNtrRouteHandler`, and direct registrations for `GetNtrStatusQuery`, `GetSusceptibilityQuery`, `NtrPhaseAdvancedHandler`. NtrProgressionRepository NOT added to IUnitOfWork (bounded context isolation). | | [x] |
| 7 | 17, 18, 19, 20, 22, 23, 24, 25 | Write xUnit tests in `C:\Era\core\src\Era.Core.Tests\NTR\` covering: `AdvanceNtrPhaseHandler` guard-then-mutate logic (rejects when `CanAdvance` returns false, advances when eligible), `ChangeNtrRouteHandler` guard logic (rejects when `CanChangeRoute` returns false), `NtrProgressionRepository.GetByCharacterId` lookup (found and not-found cases), `NtrQueryAcl.CheckNtrFavorably` threshold comparison (below threshold returns false, at/above threshold returns true, unknown character returns false). Use mock `INtrCalculator`. Then verify no TODO/FIXME/HACK in NTR deliverables, run `dotnet build Era.Core.csproj` (exit 0), run `dotnet test Era.Core.Tests.csproj --filter NTR` (exit 0). | | [x] |

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| [I] | Investigation required before implementation | **Skip** | Mini-TDD cycle |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-853.md Technical Design (Interfaces / Data Structures, Approach sections) | `NTR/Application/Commands/AdvanceNtrPhaseCommand.cs` (`ICommand<Unit>`), `AdvanceNtrPhaseHandler.cs` (`ICommandHandler<AdvanceNtrPhaseCommand, Unit>`), `ChangeNtrRouteCommand.cs` (`ICommand<Unit>`), `ChangeNtrRouteHandler.cs` (`ICommandHandler<ChangeNtrRouteCommand, Unit>`) |
| 2 | implementer | sonnet | feature-853.md Technical Design (Interfaces / Data Structures, Approach sections) | `NTR/Application/Queries/GetNtrStatusQuery.cs`, `GetSusceptibilityQuery.cs` |
| 3 | implementer | sonnet | feature-853.md Technical Design (Approach — EventHandlers section) | `NTR/Application/EventHandlers/NtrPhaseAdvancedHandler.cs` |
| 4 | implementer | sonnet | feature-853.md Technical Design (Interfaces / Data Structures — INtrProgressionRepository + NtrProgressionRepository sections) | `NTR/Domain/Repositories/INtrProgressionRepository.cs`, `NTR/Infrastructure/NtrProgressionRepository.cs` |
| 5 | implementer | sonnet | feature-853.md Technical Design (Interfaces / Data Structures — NtrQueryAcl section) | `NTR/Infrastructure/NtrQueryAcl.cs` |
| 6 | implementer | sonnet | feature-853.md Technical Design (DI registration changes code block) | Updated `ServiceCollectionExtensions.cs` (NullNtrQuery removed, ACL + all new services registered) |
| 7 | implementer | sonnet | feature-853.md Technical Design (Approach, Key Decisions, AC Coverage), feature-853.md AC Definition Table | `NTR/Application/Commands/AdvanceNtrPhaseHandlerTests.cs`, `NTR/Application/Commands/ChangeNtrRouteHandlerTests.cs`, `NTR/Infrastructure/NtrProgressionRepositoryTests.cs`, `NTR/Infrastructure/NtrQueryAclTests.cs`; build/test pass confirmation |

### Pre-conditions

- F852 is [DONE]: `INtrCalculator` interface exists at `C:\Era\core\src\Era.Core\NTR\Domain\Services\INtrCalculator.cs`
- F851 is [DONE]: `NtrProgression` aggregate exists at `C:\Era\core\src\Era.Core\NTR\Domain\Aggregates\NtrProgression.cs` with parameterless `AdvancePhase()` and `ChangeRoute(NtrRoute)` methods
- F850 is [DONE]: `NtrRoute`, `NtrPhase`, `Susceptibility` value objects exist at `C:\Era\core\src\Era.Core\NTR\Domain\ValueObjects\`
- `NullNtrQuery` is currently registered at `ServiceCollectionExtensions.cs:115`
- `ICommand<TResult>` and `ICommandHandler<TCommand, TResult>` interfaces exist in Era.Core
- `InMemoryRepository<T, TId>` base class exists at `C:\Era\core\src\Era.Core\Infrastructure\InMemoryRepository.cs`

### Execution Order

**Phase 1 — Create Application Commands layer (Task 1)**

1. Create directory `C:\Era\core\src\Era.Core\NTR\Application\Commands\` (if it does not exist)
2. Create `AdvanceNtrPhaseCommand.cs`: file-scoped namespace `Era.Core.NTR.Application.Commands`, record `AdvanceNtrPhaseCommand(CharacterId TargetCharacter)` implementing `ICommand<Unit>` with `CommandId Id { get; } = CommandId.None` (CommandId is int-based with no New() factory — use None sentinel)
3. Create `AdvanceNtrPhaseHandler.cs`: sealed class implementing `ICommandHandler<AdvanceNtrPhaseCommand, Unit>`, constructor injects `INtrCalculator _calculator` and `INtrProgressionRepository _repository`, `Handle()` returns `Task<Result<Unit>>` using guard-then-mutate pattern: load via `_repository.GetByCharacterId(command.TargetCharacter)`, call `_calculator.CanAdvance(progression)`, if ineligible return failure Result, else call `progression.AdvancePhase()`, `_repository.Update(progression)`, return result
4. Create `ChangeNtrRouteCommand.cs`: record `ChangeNtrRouteCommand(CharacterId TargetCharacter, NtrRoute TargetRoute)` implementing `ICommand<Unit>` with `CommandId.None`
5. Create `ChangeNtrRouteHandler.cs`: sealed class implementing `ICommandHandler<ChangeNtrRouteCommand, Unit>`, constructor injects `INtrCalculator` and `INtrProgressionRepository`, uses `_repository.GetByCharacterId()` then calls `_calculator.CanChangeRoute(progression, command.TargetRoute)` before invoking `progression.ChangeRoute(command.TargetRoute)` — guard-then-mutate pattern identical to AdvanceNtrPhaseHandler
6. Use the exact code stubs from Technical Design "Interfaces / Data Structures" section as implementation templates

**Phase 2 — Create Application Queries layer (Task 2)**

1. Create directory `C:\Era\core\src\Era.Core\NTR\Application\Queries\` (if it does not exist)
2. Create `GetNtrStatusQuery.cs`: sealed class with file-scoped namespace `Era.Core.NTR.Application.Queries`, constructor injects `INtrProgressionRepository`, `Execute(CharacterId character)` returns `(NtrPhase Phase, NtrRoute Route)?` using `_repository.GetByCharacterId(character)`. Direct service class pattern (NOT CQRS IQuery abstraction — Key Decision: B selected)
3. Create `GetSusceptibilityQuery.cs`: sealed class, constructor injects `INtrProgressionRepository`, `Execute(CharacterId character)` returns `Susceptibility?` using `_repository.GetByCharacterId(character)`

**Phase 3 — Create Application EventHandlers layer (Task 3)**

1. Create directory `C:\Era\core\src\Era.Core\NTR\Application\EventHandlers\` (if it does not exist)
2. Create `NtrPhaseAdvancedHandler.cs`: sealed class with file-scoped namespace `Era.Core.NTR.Application.EventHandlers`, `Handle(NtrPhaseAdvanced domainEvent)` method with empty body (no-op skeleton). Phase 24 scope: no subscribers, no logic — handler is minimal skeleton regardless of whether Phase 25 subscriber logic could be anticipated. Do NOT add Phase 25 subscriber logic.

**Phase 4 — Create Repository Interface and Implementation (Task 4)**

1. Create directory `C:\Era\core\src\Era.Core\NTR\Domain\Repositories\` (if it does not exist)
2. Create `INtrProgressionRepository.cs` in `Domain\Repositories\`: file-scoped namespace `Era.Core.NTR.Domain.Repositories`, interface `INtrProgressionRepository` extending `IRepository<NtrProgression, NtrProgressionId>` with `Result<NtrProgression> GetByCharacterId(CharacterId characterId)` method. Per DIP, interface lives in Domain layer (inner layer holds abstractions).
3. Create directory `C:\Era\core\src\Era.Core\NTR\Infrastructure\` (if it does not exist)
4. Create `NtrProgressionRepository.cs` in `Infrastructure\`: file-scoped namespace `Era.Core.NTR.Infrastructure`, sealed class extending `InMemoryRepository<NtrProgression, NtrProgressionId>` and implementing `INtrProgressionRepository` (add `using Era.Core.NTR.Domain.Repositories;`), add `GetByCharacterId(CharacterId characterId)` method returning `Result<NtrProgression>` (NOT nullable — Key Decision: B selected), using `GetAll().FirstOrDefault(p => p.TargetCharacter == characterId)`, return `Result<NtrProgression>.Fail(...)` if null
5. Return type is `Result<NtrProgression>` consistent with `InMemoryRepository.GetById` return type

**Phase 5 — Create Infrastructure ACL (Task 5)**

1. Create `NtrQueryAcl.cs` in `C:\Era\core\src\Era.Core\NTR\Infrastructure\`: sealed class implementing `: INtrQuery`, constructor injects `INtrProgressionRepository repository`
2. `CheckNtrFavorably(CharacterId character, int threshold)`: call `_repository.GetByCharacterId(character)`, if `!result.IsSuccess` return `false` (safe default: unknown character = not favorable, preserving NullNtrQuery behavior), else return `result.Value.CurrentSusceptibility.SubmissionDegree >= threshold` (Phase 24 heuristic using typed SubmissionDegree — actual FAV evaluation deferred to Phase 25; use this comparison regardless of whether a concrete INtrCalculator is later available)
3. No TODO/FIXME/HACK markers

**Phase 6 — Update DI registration (Task 6)**

1. Open `C:\Era\core\src\Era.Core\DependencyInjection\ServiceCollectionExtensions.cs`
2. Delete entirely `services.AddSingleton<INtrQuery, NullNtrQuery>();` at line 115 (remove the entire line including any NullNtrQuery reference)
3. Add in its place (preserving surrounding code structure):
   - `services.AddSingleton<NtrProgressionRepository>();`
   - `services.AddSingleton<IRepository<NtrProgression, NtrProgressionId>>(sp => sp.GetRequiredService<NtrProgressionRepository>());` (factory delegate pattern to avoid duplicate singleton)
   - `services.AddSingleton<INtrProgressionRepository>(sp => sp.GetRequiredService<NtrProgressionRepository>());`
   - `services.AddSingleton<INtrQuery, NtrQueryAcl>();`
   - `services.AddSingleton<ICommandHandler<AdvanceNtrPhaseCommand, Unit>, AdvanceNtrPhaseHandler>();`
   - `services.AddSingleton<ICommandHandler<ChangeNtrRouteCommand, Unit>, ChangeNtrRouteHandler>();`
   - `services.AddSingleton<GetNtrStatusQuery>();`
   - `services.AddSingleton<GetSusceptibilityQuery>();`
   - `services.AddSingleton<NtrPhaseAdvancedHandler>();`
4. Do NOT add NtrProgressionRepository to IUnitOfWork — bounded context isolation requires separate registration

**Phase 7 — Write tests and verify build (Task 7)**

1. Create test directory `C:\Era\core\src\Era.Core.Tests\NTR\Application\Commands\` and `NTR\Infrastructure\` as needed
2. Write `AdvanceNtrPhaseHandlerTests.cs`: tests for (a) guard rejects advancement when mock `INtrCalculator.CanAdvance()` returns false, (b) guard allows advancement and calls `AdvancePhase()` when `CanAdvance()` returns true. Use `[Trait("Category", "Unit")]`
3. Write `ChangeNtrRouteHandlerTests.cs`: tests for (a) guard rejects route change when `CanChangeRoute()` returns false, (b) allows route change when eligible
4. Write `NtrProgressionRepositoryTests.cs`: tests for (a) `GetByCharacterId` found case returns Ok, (b) `GetByCharacterId` not-found case returns Fail
5. Write `NtrQueryAclTests.cs`: tests for (a) unknown character returns false, (b) SubmissionDegree below threshold returns false, (c) SubmissionDegree at/above threshold returns true
6. Run build: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build src/Era.Core/Era.Core.csproj'` — must exit 0
7. Run tests: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test src/Era.Core.Tests/Era.Core.Tests.csproj --filter "FullyQualifiedName~NTR" --blame-hang-timeout 10s'` — must exit 0

### Build Verification Steps

```bash
# Build Era.Core (TreatWarningsAsErrors=true — must be warning-free)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build src/Era.Core/Era.Core.csproj'

# Run NTR-filtered tests only
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test src/Era.Core.Tests/Era.Core.Tests.csproj --filter "FullyQualifiedName~NTR" --blame-hang-timeout 10s'
```

### Success Criteria

- `NTR/Application/Commands/` contains 4 files: AdvanceNtrPhaseCommand.cs, AdvanceNtrPhaseHandler.cs, ChangeNtrRouteCommand.cs, ChangeNtrRouteHandler.cs
- `NTR/Application/Queries/` contains 2 files: GetNtrStatusQuery.cs, GetSusceptibilityQuery.cs
- `NTR/Application/EventHandlers/` contains 1 file: NtrPhaseAdvancedHandler.cs
- `NTR/Domain/Repositories/` contains 1 file: INtrProgressionRepository.cs
- `NTR/Infrastructure/` contains 2 files: NtrProgressionRepository.cs, NtrQueryAcl.cs
- Total 10 new production `.cs` files (satisfies AC#17 gte 7)
- `ServiceCollectionExtensions.cs` contains no `NullNtrQuery` reference (AC#12)
- `ServiceCollectionExtensions.cs` contains `NtrQueryAcl` (AC#13) and `NtrProgressionRepository` (AC#16)
- No TODO/FIXME/HACK markers across NTR/ directory (AC#18)
- `dotnet build Era.Core.csproj` exits 0 (AC#19)
- `dotnet test --filter NTR` exits 0 (AC#20)

### Error Handling

- If `ICommand<TResult>` or `ICommandHandler<TCommand, TResult>` signatures differ from spec: Read actual interface files before implementing; do NOT assume generic parameter order
- If `InMemoryRepository<T, TId>` does not expose `GetAll()`: Read actual base class before implementing
- If `NtrProgression.AdvancePhase()` or `ChangeRoute()` signatures differ from spec: Read `NtrProgression.cs` and update command handlers accordingly
- If `SubmissionDegree` property name differs on `Susceptibility`: Read `Susceptibility.cs` before implementing NtrQueryAcl
- If build fails with CS1591 (missing XML docs): Add `/// <summary>` tags to all public types and members
- If existing NTR tests fail after DI changes: STOP → Report to user; do NOT modify existing test files
- If duplicate singleton registration error occurs: Use factory delegate pattern for `IRepository<NtrProgression, NtrProgressionId>` as documented in Approach section

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

<!-- Transferred + Result columns (F811/F805 Lesson):
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: 作成済み(A), 追記済み(B), 記載済み(C), 確認済み(既存)
- Phase 9.4.1 で転記実行・Result記入。Phase 10.0.2 で検証のみ
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
| 2026-03-08T05:10 | PHASE_1 | initializer | Status [REVIEWED]->[WIP], dependency gate passed | READY |
| 2026-03-08T05:12 | PHASE_2 | explorer | Codebase investigation: all prerequisites confirmed | OK |
| 2026-03-08T05:16 | PHASE_3 | implementer | TDD RED: 4 test files, 11 tests, 8 CS0234 errors (expected) | RED |
| 2026-03-08T05:25 | PHASE_4 | implementer | Tasks 1-6: 10 production files, DI updated. Build 0W/0E. 11 NTR tests pass | GREEN |
| 2026-03-08T05:35 | PHASE_7 | ac-tester | 25/25 ACs PASS | OK |
| 2026-03-08T05:40 | PHASE_8 | feature-reviewer | Quality review: OK. Doc-check: skipped (no extensibility). SSOT: N/A | OK |
| 2026-03-08T05:45 | PHASE_9 | orchestrator | 25/25 PASS, 0 DEVIATION. User approved. | OK |
| 2026-03-08T05:50 | PHASE_10 | finalizer | [WIP]->[DONE], staged 3 files | READY_TO_COMMIT |
| 2026-03-08T05:50 | CodeRabbit | 2 Major → fixed | Guard repo update on AdvancePhase/ChangeRoute failure |

<!-- run-phase-1-completed -->
<!-- run-phase-2-completed -->
<!-- run-phase-3-completed -->
<!-- run-phase-4-completed -->
<!-- run-phase-7-completed -->
<!-- run-phase-8-completed -->
<!-- run-phase-9-completed -->
<!-- run-phase-10-completed -->
---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [resolved-applied] Phase3 iter1: [AC-VAC] AC#18 accepted as vacuous not_matches -- post-implementation regression guard for TODO/FIXME/HACK in NTR directory
- [fix] Phase2-Review iter1: Technical Design ICommand type | Changed ICommand<Result<Unit>> to ICommand<Unit> to avoid double-wrapping Task<Result<Result<Unit>>>
- [fix] Phase2-Review iter1: Technical Design DI registration | Changed handler registration from concrete to ICommandHandler interface pattern matching existing codebase
- [fix] Phase2-Review iter1: AC Coverage behavioral gap | Added AC#22 for guard-rejection behavioral verification in test files
- [fix] Phase2-Review iter1: Review Notes format | Reformatted V3 Exception free-text to standard [resolved-applied] tag format
- [fix] Phase2-Review iter2: AC Coverage symmetry | Added AC#23 for ChangeNtrRouteHandler guard-rejection behavioral verification (symmetry with AC#22)
- [fix] Phase2-Review iter3: Repository interface extraction | Introduced INtrProgressionRepository interface to eliminate duplicated GetAll().Where() lookup logic across 4 consumers; updated handlers/queries to inject INtrProgressionRepository; file count 9→10
- [fix] Phase2-Review iter3: ACL behavioral AC gap | Added AC#24 (unknown character returns false) and AC#25 (threshold comparison) for NtrQueryAcl behavioral test verification
- [fix] Phase2-Review iter4: Philosophy overclaim | Revised Philosophy to separate ACL (Aggregate state reads) from Application Services (INtrCalculator invocation)
- [fix] Phase2-Review iter4: NtrQueryAcl DI consistency | Changed NtrQueryAcl from concrete NtrProgressionRepository injection to INtrProgressionRepository interface (consistency with Key Decision)
- [fix] Phase2-Review iter5: DDD layer placement | Moved INtrProgressionRepository from Infrastructure to Domain/Repositories per DIP; added Key Decision documenting rationale; updated AC#17 Glob to include Domain/Repositories
- [fix] Phase4-ACValidation iter6: AC#19,20 matcher | Changed build/test AC matchers from equals/0 to succeeds/- per testing skill convention
- [fix] Phase4-ACValidation iter6: AC#22-25 Expected | Changed descriptive Expected values to concrete strings (CanAdvance.*false, CanChangeRoute.*false, no progression, SubmissionDegree)

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

### /imp 853 (2026-03-08)
- [rejected] ac-designer guard-then-mutate behavioral AC行追加 — F851で同種提案却下済み（DDD固有・単一feature観察）
- [revised] ac_ops.py N15: Type=build/test → succeeds/fails matcher強制チェック → `src/tools/python/ac_ops.py` (対象をac-designerからac_ops.pyに変更)
- [rejected] ac-designer ACL/Adapter behavioral AC行追加 — DDD固有パターン、単一feature観察
- [applied] 前任ファイル反復Read — 共有セッション由来のメトリクスノイズ確認（変更不要）
- [revised] Hook JSON parse limit調査記録 → fail-open確認、大payload(9K-23Kバイト)でparse failure（低優先度調査）

---

<!-- fc-phase-6-completed -->
## Links

[Predecessor: F852](feature-852.md) - INtrCalculator Domain Service interface (Application Commands invoke CanAdvance/CanChangeRoute)
[Related: F851](feature-851.md) - NtrProgression Aggregate (Commands/Queries operate on this Aggregate)
[Related: F850](feature-850.md) - NTR Value Objects (Command/Query parameters use NtrRoute, NtrPhase, Susceptibility)
[Successor: F854](feature-854.md) - Post-Phase Review Phase 24 (reviews F850-F853 deliverables)
[Successor: F855](feature-855.md) - Phase 25 Planning (concrete implementations depend on F853 Application layer)
[Related: F819](feature-819.md) - ClothingSystem introduced INtrQuery dependency pattern
[Related: F829](feature-829.md) - OB-03 deferred obligation for INtrQuery typed implementation fulfilled by ACL
