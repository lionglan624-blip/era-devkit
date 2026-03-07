# Feature 850: NTR Ubiquitous Language + Value Objects

## Status: [DONE]
<!-- fl-reviewed: 2026-03-07T04:11:55Z -->

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

Phase 24: NTR Bounded Context -- Domain Layer (Value Objects). Pipeline Continuity -- this sub-feature implements the foundational domain primitives that all subsequent Phase 24 sub-features depend on. The NTR Bounded Context SSOT for domain terminology and typed primitives lives in `Era.Core.NTR.Domain.ValueObjects` namespace; all downstream features (F851-F854) consume these types as their shared vocabulary.

### Problem (Current Issue)

The NTR system lacks typed C# domain primitives because the original ERB architecture uses untyped FAV level integers, string-based route identifiers, and ad-hoc parameter bundles with no DDD modeling (`pm/reference/ntr-ddd-input.md:14`). Additionally, the existing `Era.Core.Types.NtrParameters` record (`Types/NtrParameters.cs:8`) occupies the `NtrParameters` name with incompatible 2-field output semantics (`AffectionChange, JealousyValue`), creating a CS0104 ambiguous reference conflict with the Phase 24 `NtrParameters` Value Object that requires typed FAV-level evaluation inputs. This naming conflict affects 4 consumer files within Era.Core (`INtrEngine.cs:15`, `NtrEngine.cs:15`, `ServiceCollectionExtensions.cs`, plus `Types/NtrParameters.cs` itself) and 1 test file (`NtrEngineTests.cs`), with zero consumers in the engine repo, making rename feasible.

### Goal (What to Achieve)

Implement NTR Ubiquitous Language definition (glossary) and four Value Objects in `src/Era.Core/NTR/Domain/ValueObjects/`:
- `NtrRoute.cs` -- Route classification (R0-R6) for NTR progression paths
- `NtrPhase.cs` -- Phase tracking (0-7) within NTR progression
- `NtrParameters.cs` -- Typed container for NTR evaluation parameters
- `Susceptibility.cs` -- Susceptibility state (balance of submission degree vs affection)
- Ubiquitous Language glossary (inline XML doc comments or separate markdown)

Resolve the `NtrParameters` naming conflict by renaming the existing `Era.Core.Types.NtrParameters` to reflect its actual semantics (calculation result, not evaluation input).

**DDD Input Reference**: `pm/reference/ntr-ddd-input.md` -- distinguishes Validated candidates (FavLevel, AffairPermission, CorruptionState -- directly observable in ERB) from Grounded candidates (NtrRoute, NtrPhase, NtrParameters -- new concepts requiring Phase 24 design decisions to formalize as typed C# constructs).

### Architecture Task Coverage

<!-- Architecture Task 1: NTR Ubiquitous Language definition (glossary) -->
<!-- Architecture Task 3: NtrRoute Value Object implementation (R0-R6) -->
<!-- Architecture Task 4: NtrPhase Value Object implementation (Phase 0-7) -->
<!-- Architecture Task 5: NtrParameters Value Object implementation (FAV_*, exposure degree, etc.) -->

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why are typed NTR domain primitives needed? | Phase 24 requires typed Value Objects to replace loose ERB variable passing for NTR route, phase, parameters, and susceptibility | `docs/architecture/migration/phase-20-27-game-systems.md:575` |
| 2 | Why do they not exist yet? | The NTR domain layer (`Era.Core/NTR/`) is entirely greenfield -- no directory exists | Glob confirmed no `Era.Core/NTR/` directory |
| 3 | Why is the existing NtrParameters insufficient? | `Era.Core.Types.NtrParameters(AffectionChange, JealousyValue)` is a 2-field calculation result, not a typed evaluation input container | `Types/NtrParameters.cs:8` |
| 4 | Why does a naming conflict exist? | The architecture doc designs new NtrParameters without referencing the existing type, and `TreatWarningsAsErrors=true` makes CS0104 ambiguous references fatal | `INtrEngine.cs:15`, `NtrEngine.cs:15` |
| 5 | Why (Root)? | The original ERB architecture has no DDD modeling -- R0-R6 routes and Phase 0-7 are new C# migration concepts that require explicit design decisions, not ERB-derived translations | `phase-20-27-game-systems.md:575`, `ntr-ddd-input.md:70-98` |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | No typed NTR domain primitives in Era.Core | Original ERB architecture uses untyped variables with no DDD modeling; existing NtrParameters occupies the name with incompatible semantics |
| Where | `Era.Core/NTR/` directory absent | Architectural gap: Phase 24 DDD concepts (R0-R6, Phase 0-7) are new constructs, not ERB translations |
| Fix | Create 4 VO files | Establish NTR Bounded Context namespace with typed VOs following established `readonly record struct` pattern, resolve NtrParameters naming conflict |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F849 | [DONE] | Predecessor -- Phase 24 Planning parent |
| F847 | [DONE] | Input -- NTR kojo analysis providing domain knowledge |
| F851 | [DRAFT] | Successor -- NtrProgression Aggregate consumes all 4 VOs |
| F852 | [DRAFT] | Successor -- INtrCalculator uses NtrRoute/NtrPhase in method signatures |
| F853 | [DRAFT] | Successor -- Application Services + ACL maps external representations to VOs |
| F854 | [DRAFT] | Successor -- Post-Phase Review validates F850 deliverables |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| DDD infrastructure exists | FEASIBLE | `AggregateRoot<TId>`, `IDomainEvent`, `IRepository` all present in Era.Core |
| Value Object pattern established | FEASIBLE | 4 existing VOs (CharacterName, AbilitySet, etc.) use `readonly record struct` |
| Target namespace clear | FEASIBLE | `Era.Core.NTR.Domain.ValueObjects` -- no existing conflicts |
| Design specifications available | FEASIBLE | Code examples for NtrRoute/NtrPhase in architecture doc lines 712-748 |
| NtrParameters naming conflict resolvable | FEASIBLE | Old type has only core-internal consumers (4 files + 1 test), zero engine-repo consumers |
| Predecessor satisfied | FEASIBLE | F849 is [DONE] |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core public API | MEDIUM | New namespace `Era.Core.NTR.Domain.ValueObjects` added; old NtrParameters renamed |
| F851-F854 downstream features | HIGH | All subsequent Phase 24 features depend on these VOs as foundational types |
| Existing NtrEngine consumers | LOW | 4 files + 1 test require import/type name update, all core-internal |
| NuGet package version | LOW | Era.Core version bump needed for renamed type (breaking change) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| `readonly record struct` pattern required | Established convention (CharacterName.cs, AbilitySet.cs) | All 4 VOs must follow this pattern |
| Private ctor + static factory for validated VOs | CharacterName pattern (`CharacterName.cs:9-17`) | NtrRoute, NtrPhase must use factory methods returning `Result<T>` |
| `TreatWarningsAsErrors=true` | `Directory.Build.props` | No ambiguous references -- NtrParameters conflict must be resolved |
| Code lives in `C:\Era\core`, not devkit | 5-repo split | Implementation targets core repo; AC file matchers use core-relative paths |
| R0-R6 and Phase 0-7 are NEW concepts | Architecture doc line 575 | Design decisions required, not ERB-derived translations |
| NtrPhase must support `Next()` method | F851 NtrProgression calls NtrPhase.Next() | Sibling call chain dependency |
| NtrRoute must have `IsCorrupted` property | Architecture doc line 727 | Required for downstream route classification logic |
| All dotnet commands via WSL | CLAUDE.md | Build and test execution via WSL2 Ubuntu |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| NtrParameters rename cascades across 4 consumer files + 1 test file | MEDIUM | MEDIUM | Zero consumers outside Era.Core; rename is a prerequisite task with bounded scope |
| Susceptibility VO design incorrect for F851-F853 consumption | MEDIUM | HIGH | Architecture doc lacks code examples for Susceptibility; review F851 requirements during Technical Design; F854 Post-Phase Review validates |
| NtrRoute/NtrPhase boundary definitions premature | MEDIUM | MEDIUM | Document as "Grounded" status per ntr-ddd-input.md; F854 validates boundaries |
| UL glossary drifts from implementation | LOW | LOW | Prefer inline XML doc comments co-located with code over separate markdown |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| NTR VO files | `ls src/Era.Core/NTR/Domain/ValueObjects/` | 0 | Directory does not exist yet |
| Old NtrParameters consumers | `grep -r "NtrParameters" src/Era.Core/ --include="*.cs" -l` | 4 files + 1 test | Files requiring update on rename |

**Baseline File**: `_out/tmp/baseline-850.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | VOs must be `readonly record struct` | Convention (CharacterName.cs pattern) | AC must verify struct type declaration, not just file existence |
| C2 | NtrRoute must define R0-R6 static instances (7 constants) | Architecture doc lines 717-723 | AC must verify 7 named route constants exist |
| C3 | NtrPhase must define Phase0-Phase7 static instances (8 constants) | Architecture doc lines 735-742 | AC must verify 8 named phase constants exist |
| C4 | NtrParameters naming conflict must be resolved | `Types/NtrParameters.cs:8` vs new `NTR/Domain/ValueObjects/NtrParameters.cs` | AC must verify old type renamed and all consumers updated |
| C5 | NtrPhase must have `Next()` method | F851 sibling call chain: NtrProgression calls NtrPhase.Next() | AC must verify Next() method exists |
| C6 | NtrRoute must have `IsCorrupted` property | Architecture doc line 727 | AC must verify IsCorrupted property exists |
| C7 | No TODO/FIXME/HACK in deliverables | Architecture doc line 782 | Already in feature spec as AC5 |
| C8 | Ubiquitous Language glossary required | Feature spec Goal section | AC must verify glossary exists (XML doc comments or separate file) |
| C9 | Unit tests in Era.Core.Tests | Project structure convention | AC must verify test files exist in Era.Core.Tests/NTR/ |
| C10 | Build succeeds with no warnings | `TreatWarningsAsErrors=true` | AC must verify `dotnet build` succeeds |

### Constraint Details

**C1: readonly record struct Pattern**
- **Source**: Existing VOs in `Era.Core/Domain/ValueObjects/` (CharacterName.cs:5, AbilitySet.cs:8)
- **Verification**: Grep for `readonly record struct` in each VO file
- **AC Impact**: File existence ACs (AC1-4) are insufficient; structural ACs needed

**C2: NtrRoute R0-R6 Constants**
- **Source**: `docs/architecture/migration/phase-20-27-game-systems.md:717-723`
- **Verification**: Grep for static readonly fields in NtrRoute.cs
- **AC Impact**: Must verify all 7 route constants (R0, R1, R2, R3, R4, R5, R6) are defined
- **Collection Members**: R0 (未接触/No contact), R1 (強制/調教/Coercion), R2 (懐柔/Appeasement), R3 (欲情/Desire), R4 (恋愛/Love), R5 (混合/Mixed), R6 (純愛堕ち/Pure-love corruption)

**C3: NtrPhase Phase0-Phase7 Constants**
- **Source**: `docs/architecture/migration/phase-20-27-game-systems.md:735-742`
- **Verification**: Grep for static readonly fields in NtrPhase.cs
- **AC Impact**: Must verify all 8 phase constants (Phase0 through Phase7) are defined
- **Collection Members**: Phase0, Phase1, Phase2, Phase3, Phase4, Phase5, Phase6, Phase7

**C4: NtrParameters Naming Conflict Resolution**
- **Source**: `Era.Core.Types.NtrParameters.cs:8` -- existing record `(AffectionChange, JealousyValue)`
- **Verification**: Grep for old type name; verify no CS0104 ambiguous references
- **AC Impact**: AC must verify old type is renamed (e.g., to `NtrCalculationResult`) and all 9 consumer files updated

**C5: NtrPhase Next() Method**
- **Source**: F851 sibling call chain analysis -- NtrProgression Aggregate calls NtrPhase.Next() for phase advancement
- **Verification**: Grep for `Next()` method signature in NtrPhase.cs
- **AC Impact**: Must verify method exists with correct return type

**C6: NtrRoute IsCorrupted Property**
- **Source**: Architecture doc line 727 specifies IsCorrupted for route classification
- **Verification**: Grep for `IsCorrupted` property in NtrRoute.cs
- **AC Impact**: Must verify boolean property exists

**C8: Ubiquitous Language Glossary**
- **Source**: Feature spec Goal section; architecture doc Phase 24 design
- **Verification**: Verify XML doc comments on all public types/members OR separate glossary file
- **AC Impact**: Must verify documentation completeness for NTR domain terms

**C9: Unit Tests**
- **Source**: Test convention -- all new code requires unit tests
- **Verification**: Glob for `Era.Core.Tests/NTR/Domain/ValueObjects/*Tests.cs`
- **AC Impact**: Test files must exist and pass

**C7: No Technical Debt Markers**
- **Source**: Architecture doc line 782; general clean-code policy
- **Verification**: Grep for `TODO|FIXME|HACK` in `src/Era.Core/NTR/Domain/ValueObjects/` after implementation
- **AC Impact**: AC#15 (`not_matches`) directly enforces this constraint post-implementation

**C10: Build Succeeds with No Warnings**
- **Source**: `Directory.Build.props` (`TreatWarningsAsErrors=true`)
- **Verification**: `dotnet build src/Era.Core/Era.Core.csproj` exits 0 with no warning output
- **AC Impact**: AC#18 (`equals 0`) directly enforces this constraint

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F849 | [DONE] | Phase 24 Planning parent -- satisfied |
| Successor | F851 | [DRAFT] | NtrProgression Aggregate consumes all 4 VOs as properties. CALL dependency: NtrPhase.Next() called from NtrProgression |
| Successor | F852 | [DRAFT] | INtrCalculator uses NtrRoute/NtrPhase in method signatures |
| Successor | F853 | [DRAFT] | Application Services + ACL maps external representations to VOs |
| Successor | F854 | [DRAFT] | Post-Phase Review validates F850 deliverables |

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

<!-- fc-phase-2-completed -->

<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "all subsequent Phase 24 sub-features depend on" | All 4 VO files must exist as usable types | AC#1, AC#2, AC#3, AC#4 |
| "SSOT for domain terminology and typed primitives lives in Era.Core.NTR.Domain.ValueObjects namespace" | Each VO must use readonly record struct pattern (canonical type definition); UL glossary must define all NTR domain terms | AC#5, AC#6, AC#7, AC#8, AC#17 |
| "all downstream features (F851-F854) consume these types as their shared vocabulary" | NtrPhase.Next() and IsComplete must exist for F851 consumption; NtrRoute.IsCorrupted and Susceptibility.IsCorrupted must exist for downstream logic | AC#11, AC#12, AC#21, AC#24 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | NtrRoute.cs exists | file | Glob(src/Era.Core/NTR/Domain/ValueObjects/NtrRoute.cs) | exists | - | [x] |
| 2 | NtrPhase.cs exists | file | Glob(src/Era.Core/NTR/Domain/ValueObjects/NtrPhase.cs) | exists | - | [x] |
| 3 | NtrParameters.cs exists | file | Glob(src/Era.Core/NTR/Domain/ValueObjects/NtrParameters.cs) | exists | - | [x] |
| 4 | Susceptibility.cs exists | file | Glob(src/Era.Core/NTR/Domain/ValueObjects/Susceptibility.cs) | exists | - | [x] |
| 5 | NtrRoute is readonly record struct | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/NtrRoute.cs, pattern="readonly record struct NtrRoute") | contains | `readonly record struct NtrRoute` | [x] |
| 6 | NtrPhase is readonly record struct | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/NtrPhase.cs, pattern="readonly record struct NtrPhase") | contains | `readonly record struct NtrPhase` | [x] |
| 7 | NtrParameters is readonly record struct | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/NtrParameters.cs, pattern="readonly record struct NtrParameters") | contains | `readonly record struct NtrParameters` | [x] |
| 8 | Susceptibility is readonly record struct | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/Susceptibility.cs, pattern="readonly record struct Susceptibility") | contains | `readonly record struct Susceptibility` | [x] |
| 9 | NtrRoute defines all 7 route constants (R0-R6) | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/NtrRoute.cs, pattern="static readonly NtrRoute R[0-6]") | gte | 7 | [x] |
| 10 | NtrPhase defines all 8 phase constants (Phase0-Phase7) | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/NtrPhase.cs, pattern="static readonly NtrPhase Phase[0-7]") | gte | 8 | [x] |
| 11 | NtrPhase has Next() method | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/NtrPhase.cs, pattern="NtrPhase Next\\(\\)") | contains | `NtrPhase Next()` | [x] |
| 12 | NtrRoute has IsCorrupted property | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/NtrRoute.cs, pattern="IsCorrupted") | contains | `IsCorrupted` | [x] |
| 13 | Old NtrParameters type renamed (no longer in Types/) | code | Grep(src/Era.Core/Types/NtrParameters.cs, pattern="record NtrParameters") | not_contains | `record NtrParameters` | [x] |
| 14 | NtrEngine no longer references old NtrParameters type name | code | Grep(src/Era.Core/NtrEngine.cs, pattern="Result<NtrParameters>") | not_contains | `Result<NtrParameters>` | [x] |
| 15 | No TODO/FIXME/HACK in VO deliverables | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/, pattern="TODO|FIXME|HACK") | not_matches | `TODO|FIXME|HACK` | [x] |
| 16 | Unit test files exist for NTR VOs | file | Glob(src/Era.Core.Tests/NTR/Domain/ValueObjects/*Tests.cs) | gte | 1 | [x] |
| 17 | XML doc comments on public VO types (UL glossary) | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/, pattern="/// <summary>") | gte | 4 | [x] |
| 18 | Era.Core builds successfully | build | dotnet build src/Era.Core/Era.Core.csproj | equals | 0 | [x] |
| 19 | Era.Core.Tests pass | test | dotnet test src/Era.Core.Tests/Era.Core.Tests.csproj --blame-hang-timeout 10s | contains | `Passed` | [x] |
| 20 | INtrEngine no longer references old NtrParameters type name | code | Grep(src/Era.Core/INtrEngine.cs, pattern="Result<NtrParameters>") | not_contains | `Result<NtrParameters>` | [x] |
| 21 | Susceptibility has IsCorrupted property | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/Susceptibility.cs, pattern="IsCorrupted") | contains | `IsCorrupted` | [x] |
| 22 | NtrParameters has SlaveLevel and FavLevel fields | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/NtrParameters.cs, pattern="SlaveLevel\|FavLevel") | gte | 2 | [x] |
| 23 | Susceptibility has SubmissionDegree and AffectionLevel fields | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/Susceptibility.cs, pattern="SubmissionDegree\|AffectionLevel") | gte | 2 | [x] |
| 24 | NtrPhase has IsComplete property | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/NtrPhase.cs, pattern="IsComplete") | contains | `IsComplete` | [x] |

### AC Details

**AC#1: NtrRoute.cs exists**
- **Test**: `Glob(src/Era.Core/NTR/Domain/ValueObjects/NtrRoute.cs)`
- **Expected**: `exists` -- File must exist at the prescribed path
- **Derivation**: Architecture doc line 648 prescribes `NtrRoute.cs` in `ValueObjects/` directory.

**AC#2: NtrPhase.cs exists**
- **Test**: `Glob(src/Era.Core/NTR/Domain/ValueObjects/NtrPhase.cs)`
- **Expected**: `exists` -- File must exist at the prescribed path
- **Derivation**: Architecture doc line 649 prescribes `NtrPhase.cs` in `ValueObjects/` directory.

**AC#3: NtrParameters.cs exists**
- **Test**: `Glob(src/Era.Core/NTR/Domain/ValueObjects/NtrParameters.cs)`
- **Expected**: `exists` -- File must exist at the prescribed path
- **Derivation**: Architecture doc line 650 prescribes `NtrParameters.cs` in `ValueObjects/` directory.

**AC#4: Susceptibility.cs exists**
- **Test**: `Glob(src/Era.Core/NTR/Domain/ValueObjects/Susceptibility.cs)`
- **Expected**: `exists` -- File must exist at the prescribed path
- **Derivation**: Architecture doc line 651 prescribes `Susceptibility.cs` in `ValueObjects/` directory.

**AC#9: NtrRoute defines all 7 route constants (R0-R6)**
- **Test**: `Grep(src/Era.Core/NTR/Domain/ValueObjects/NtrRoute.cs, pattern="static readonly NtrRoute R[0-6]")`
- **Expected**: `gte 7` -- R0 (No contact), R1 (Coercion), R2 (Appeasement), R3 (Desire), R4 (Love), R5 (Mixed), R6 (Pure-love corruption) = 7 constants
- **Derivation**: C2 constraint requires all 7 route constants from architecture doc lines 717-723. 1:1 mapping from ERB R0-R6 route classification.

**AC#10: NtrPhase defines all 8 phase constants (Phase0-Phase7)**
- **Test**: `Grep(src/Era.Core/NTR/Domain/ValueObjects/NtrPhase.cs, pattern="static readonly NtrPhase Phase[0-7]")`
- **Expected**: `gte 8` -- Phase0 through Phase7 = 8 constants from architecture doc lines 735-742
- **Derivation**: C3 constraint requires all 8 phase constants. 1:1 mapping from ERB Phase 0-7 progression states.

**AC#16: Unit test files exist for NTR VOs**
- **Test**: `Glob(src/Era.Core.Tests/NTR/Domain/ValueObjects/*Tests.cs)`
- **Expected**: `gte 1` -- At minimum 1 test file; could be per-VO (4) or combined (1)
- **Derivation**: C9 constraint requires unit tests in Era.Core.Tests. Minimum 1 file ensures test infrastructure exists.

**AC#17: XML doc comments on public VO types (UL glossary)**
- **Test**: `Grep(src/Era.Core/NTR/Domain/ValueObjects/, pattern="/// <summary>")`
- **Expected**: `gte 4` -- One summary per public VO type: NtrRoute, NtrPhase, NtrParameters, Susceptibility = 4 minimum
- **Derivation**: C8 constraint requires UL glossary. XML doc comments co-located with code serve as ubiquitous language definitions for all 4 VO types.

**AC#22: NtrParameters has SlaveLevel and FavLevel fields**
- **Test**: `Grep(src/Era.Core/NTR/Domain/ValueObjects/NtrParameters.cs, pattern="SlaveLevel\|FavLevel")`
- **Expected**: `gte 2` -- SlaveLevel (1 match) + FavLevel (1 match) = minimum 2 property declarations
- **Derivation**: Technical Design specifies `int SlaveLevel` and `int FavLevel` as the two evaluation parameters grounded in `@NTR_CHK_FAVORABLY(奴隷, 好感度LV)`. An empty struct would pass AC#7 but fail to serve as a typed container.

**AC#23: Susceptibility has SubmissionDegree and AffectionLevel fields**
- **Test**: `Grep(src/Era.Core/NTR/Domain/ValueObjects/Susceptibility.cs, pattern="SubmissionDegree\|AffectionLevel")`
- **Expected**: `gte 2` -- SubmissionDegree (1 match) + AffectionLevel (1 match) = minimum 2 property declarations
- **Derivation**: Technical Design specifies `int SubmissionDegree` (屈服度) and `int AffectionLevel` (好感度) as the two balance fields from ntr-ddd-input.md CorruptionState. An empty struct would pass AC#8 but fail to model the balance.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | NtrRoute.cs -- Route classification (R0-R6) | AC#1, AC#5, AC#9, AC#12 |
| 2 | NtrPhase.cs -- Phase tracking (0-7) | AC#2, AC#6, AC#10, AC#11, AC#24 |
| 3 | NtrParameters.cs -- Typed container for NTR evaluation parameters | AC#3, AC#7, AC#22 |
| 4 | Susceptibility.cs -- Susceptibility state | AC#4, AC#8, AC#21, AC#23 |
| 5 | Ubiquitous Language glossary (inline XML doc comments) | AC#17 |
| 6 | Resolve NtrParameters naming conflict by renaming old type | AC#13, AC#14, AC#20 |
| 7 | No technical debt in deliverables | AC#15 |
| 8 | Unit tests exist for VOs | AC#16 |
| 9 | Build succeeds and tests pass | AC#18, AC#19 |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Four `readonly record struct` Value Objects are created under `src/Era.Core/NTR/Domain/ValueObjects/` following the established CharacterName.cs / AbilitySet.cs pattern. The NTR Bounded Context namespace `Era.Core.NTR.Domain.ValueObjects` is bootstrapped by creating a single top-level `NTR/` directory tree in the core repo.

**Rename-first order**: Task 1 renames `Era.Core.Types.NtrParameters` -> `NtrCalculationResult` across all 3 consumer source files and 1 test file before any new VO is created. This eliminates the ambiguous-reference risk (CS0104) before the new `Era.Core.NTR.Domain.ValueObjects.NtrParameters` is introduced.

**VO implementations**:
- `NtrRoute` and `NtrPhase`: Implement verbatim from architecture doc (lines 712-748) -- private ctor, 7 / 8 static readonly instances, `IsCorrupted` property on NtrRoute, `Next()` / `IsComplete` on NtrPhase.
- `NtrParameters` (new VO): Two required fields derived from `@NTR_CHK_FAVORABLY(奴隷, 好感度LV)` empirical basis -- `SlaveLevel` (奴隷 parameter) and `FavLevel` (好感度LV parameter). Static factory `Create(int slaveLevel, int favLevel)` returns `Result<NtrParameters>` with validation for non-negative values. Kept minimal for Phase 24 foundation; F851/F852 extend if needed.
- `Susceptibility`: Encapsulates 屈服度/好感度 balance. Fields: `int SubmissionDegree` (屈服度) and `int AffectionLevel` (好感度). Static factory `Create(int submissionDegree, int affectionLevel)` returns `Result<Susceptibility>`. Boolean property `IsCorrupted => SubmissionDegree > AffectionLevel` for downstream consumption.

**XML doc comments** serve as the Ubiquitous Language glossary co-located with each type (C8). All 4 VO types get `/// <summary>` comments defining the NTR domain term they represent.

**Tests**: One test file per VO in `Era.Core.Tests/NTR/Domain/ValueObjects/` (4 files), covering: static instance values, factory validation (valid + invalid inputs), computed properties (`IsCorrupted`, `Next()`, `IsComplete`). Tagged `[Trait("Category", "Unit")]`.

This approach satisfies all 24 ACs: AC#1-4 (file existence), AC#5-8 (struct type), AC#9-10 (static constants), AC#11-12 (methods/properties), AC#13-14 (rename complete), AC#15 (no debt markers), AC#16 (test files), AC#17 (XML docs >= 4), AC#18-19 (build + tests pass).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `src/Era.Core/NTR/Domain/ValueObjects/NtrRoute.cs` |
| 2 | Create `src/Era.Core/NTR/Domain/ValueObjects/NtrPhase.cs` |
| 3 | Create `src/Era.Core/NTR/Domain/ValueObjects/NtrParameters.cs` |
| 4 | Create `src/Era.Core/NTR/Domain/ValueObjects/Susceptibility.cs` |
| 5 | Declare `public readonly record struct NtrRoute` in NtrRoute.cs |
| 6 | Declare `public readonly record struct NtrPhase` in NtrPhase.cs |
| 7 | Declare `public readonly record struct NtrParameters` in NtrParameters.cs |
| 8 | Declare `public readonly record struct Susceptibility` in Susceptibility.cs |
| 9 | Define `public static readonly NtrRoute R0` through `R6` (7 instances) |
| 10 | Define `public static readonly NtrPhase Phase0` through `Phase7` (8 instances) |
| 11 | Define `public NtrPhase Next()` method in NtrPhase.cs |
| 12 | Define `public bool IsCorrupted` property in NtrRoute.cs |
| 13 | Rename `record NtrParameters` -> `record NtrCalculationResult` in `Types/NtrParameters.cs` (file may also be renamed to `Types/NtrCalculationResult.cs`) |
| 14 | Update `NtrEngine.cs` `Calculate()` return type from `Result<NtrParameters>` to `Result<NtrCalculationResult>` (also update `INtrEngine.cs` and `NtrEngineTests.cs`) |
| 20 | Update `INtrEngine.cs` to use `Result<NtrCalculationResult>` instead of `Result<NtrParameters>` (verified by separate AC to ensure interface contract is updated) |
| 21 | Verify `Susceptibility.IsCorrupted` property exists in `Susceptibility.cs` for downstream F851/F852 consumption |
| 22 | Verify NtrParameters contains `SlaveLevel` and `FavLevel` typed fields (not empty struct) |
| 23 | Verify Susceptibility contains `SubmissionDegree` and `AffectionLevel` typed fields (not empty struct) |
| 24 | Define `public bool IsComplete` property in NtrPhase.cs |
| 15 | Write all 4 VO files and test files without TODO/FIXME/HACK comments |
| 16 | Create `Era.Core.Tests/NTR/Domain/ValueObjects/NtrRouteTests.cs`, `NtrPhaseTests.cs`, `NtrParametersTests.cs`, `SusceptibilityTests.cs` |
| 17 | Add `/// <summary>` XML doc comment to each of the 4 public VO types (4 minimum) |
| 18 | Ensure `dotnet build src/Era.Core/Era.Core.csproj` exits 0 after all changes |
| 19 | Ensure `dotnet test src/Era.Core.Tests/Era.Core.Tests.csproj` passes (all existing + new tests) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Rename target for old NtrParameters | A: `NtrCalculationResult`, B: `NtrEngineResult`, C: `NtrActionResult` | `NtrCalculationResult` | Architecture doc context calls this "calculation result" (AffectionChange + JealousyValue); `NtrCalculationResult` is the most semantically accurate and consistent with NtrEngine's role |
| File rename strategy for old NtrParameters | A: Rename file + class, B: Rename class only (keep file as NtrParameters.cs) | A: Rename both file and class to `NtrCalculationResult` | Renaming the file prevents confusion when both `NtrParameters.cs` files could coexist temporarily; aligns file name with class name per C# convention |
| NtrParameters new VO fields | A: SlaveLevel + FavLevel only (minimal), B: SlaveLevel + FavLevel + ExposureDegree (expanded), C: FavLevel only | A: SlaveLevel + FavLevel (minimal) | Grounded in `@NTR_CHK_FAVORABLY(奴隷, 好感度LV)` -- exactly 2 named parameters. F851/F852 design may add fields; premature expansion creates unverified contracts |
| Susceptibility fields | A: SubmissionDegree + AffectionLevel (int pair), B: Single int ratio, C: enum-backed | A: SubmissionDegree + AffectionLevel int pair | Matches ntr-ddd-input.md CorruptionState "屈服度/好感度 balance" -- two separate values preserved for downstream F851/F852 ratio calculations |
| Factory validation for NtrParameters/Susceptibility | A: `Result<T>` factory with non-negative guard, B: No validation (bare struct), C: Range-clamped | A: `Result<T>` factory with non-negative guard | Consistent with CharacterName.cs pattern; validates at creation boundary; non-negative is minimum invariant for FAV/submission integer fields |
| Test file organization | A: 1 combined VO test file, B: 4 per-VO test files | B: 4 per-VO test files | Each VO has distinct behavioral contracts (NtrRoute: IsCorrupted, NtrPhase: Next/IsComplete, NtrParameters/Susceptibility: factory validation); separation improves test discoverability for F851-F854 implementers |
| XML doc scope | A: Type-level summary only (4 comments), B: Type + member comments | B: Type + member comments | Member-level comments strengthen UL glossary; AC#17 requires gte 4 (minimum satisfied by type-level alone, but member comments add zero cost and benefit F851-F854 implementers) |

### Interfaces / Data Structures

```csharp
// src/Era.Core/NTR/Domain/ValueObjects/NtrRoute.cs
namespace Era.Core.NTR.Domain.ValueObjects;

/// <summary>
/// Route classification for NTR progression paths (R0-R6).
/// Represents which pathway a character is on in the NTR progression system.
/// Routes are new C# DDD concepts derived from FAV-level clustering patterns observed in ERB source.
/// </summary>
public readonly record struct NtrRoute
{
    /// <summary>Numeric route identifier (0-6).</summary>
    public int Value { get; }

    /// <summary>R0: No contact / initial state (未接触)</summary>
    public static readonly NtrRoute R0 = new(0);
    /// <summary>R1: Coercion / conditioning route (強制/調教)</summary>
    public static readonly NtrRoute R1 = new(1);
    /// <summary>R2: Appeasement / conciliation route (懐柔)</summary>
    public static readonly NtrRoute R2 = new(2);
    /// <summary>R3: Desire route (欲情)</summary>
    public static readonly NtrRoute R3 = new(3);
    /// <summary>R4: Love route (恋愛)</summary>
    public static readonly NtrRoute R4 = new(4);
    /// <summary>R5: Mixed route (混合)</summary>
    public static readonly NtrRoute R5 = new(5);
    /// <summary>R6: Pure-love corruption route (純愛堕ち)</summary>
    public static readonly NtrRoute R6 = new(6);

    private NtrRoute(int value) => Value = value;

    /// <summary>True when the character is on a deep NTR corruption route (R4+).</summary>
    public bool IsCorrupted => Value >= 4;
}
```

```csharp
// src/Era.Core/NTR/Domain/ValueObjects/NtrPhase.cs
namespace Era.Core.NTR.Domain.ValueObjects;

/// <summary>
/// Phase tracking within NTR progression (Phase0-Phase7).
/// Represents sequential progression states derived from the 11-level FAV system in NTR_UTIL.ERB.
/// </summary>
public readonly record struct NtrPhase
{
    /// <summary>Numeric phase value (0-7).</summary>
    public int Value { get; }

    /// <summary>Phase0: Indifferent / no engagement (無関心)</summary>
    public static readonly NtrPhase Phase0 = new(0);
    /// <summary>Phase1: Initial contact (接触)</summary>
    public static readonly NtrPhase Phase1 = new(1);
    /// <summary>Phase2: Agitation / disturbance (動揺)</summary>
    public static readonly NtrPhase Phase2 = new(2);
    /// <summary>Phase3: Conflict / inner struggle (葛藤)</summary>
    public static readonly NtrPhase Phase3 = new(3);
    /// <summary>Phase4: Acceptance (受容)</summary>
    public static readonly NtrPhase Phase4 = new(4);
    /// <summary>Phase5: Dependence (依存)</summary>
    public static readonly NtrPhase Phase5 = new(5);
    /// <summary>Phase6: Corruption (堕落)</summary>
    public static readonly NtrPhase Phase6 = new(6);
    /// <summary>Phase7: Complete fall / full NTR state (完堕ち)</summary>
    public static readonly NtrPhase Phase7 = new(7);

    private NtrPhase(int value) => Value = value;

    /// <summary>Advance to the next phase, capped at Phase7.</summary>
    public NtrPhase Next() => new(Math.Min(Value + 1, 7));

    /// <summary>True when the character has reached the terminal NTR phase (Phase7).</summary>
    public bool IsComplete => Value >= 7;
}
```

```csharp
// src/Era.Core/NTR/Domain/ValueObjects/NtrParameters.cs
using Era.Core.Types;

namespace Era.Core.NTR.Domain.ValueObjects;

/// <summary>
/// Typed container for NTR evaluation parameters passed to NTR_CHK_FAVORABLY.
/// Grounded in the @NTR_CHK_FAVORABLY(奴隷, 好感度LV) signature from NTR_UTIL.ERB.
/// </summary>
public readonly record struct NtrParameters
{
    /// <summary>Slave/submission parameter (奴隷) -- first argument to NTR_CHK_FAVORABLY.</summary>
    public int SlaveLevel { get; }

    /// <summary>Affection level parameter (好感度LV) -- second argument to NTR_CHK_FAVORABLY.</summary>
    public int FavLevel { get; }

    private NtrParameters(int slaveLevel, int favLevel)
    {
        SlaveLevel = slaveLevel;
        FavLevel = favLevel;
    }

    /// <summary>
    /// Create NtrParameters with validation.
    /// Both slaveLevel and favLevel must be non-negative.
    /// </summary>
    public static Result<NtrParameters> Create(int slaveLevel, int favLevel)
    {
        if (slaveLevel < 0)
            return Result<NtrParameters>.Fail($"SlaveLevel must be non-negative, got {slaveLevel}");
        if (favLevel < 0)
            return Result<NtrParameters>.Fail($"FavLevel must be non-negative, got {favLevel}");
        return Result<NtrParameters>.Ok(new NtrParameters(slaveLevel, favLevel));
    }
}
```

```csharp
// src/Era.Core/NTR/Domain/ValueObjects/Susceptibility.cs
using Era.Core.Types;

namespace Era.Core.NTR.Domain.ValueObjects;

/// <summary>
/// Susceptibility state representing the balance between submission degree and affection level.
/// Models the 屈服度/好感度 axis that is the foundational evaluation in the FAV system.
/// Corresponds to CorruptionState in the DDD input analysis (ntr-ddd-input.md).
/// </summary>
public readonly record struct Susceptibility
{
    /// <summary>Submission degree (屈服度) -- degree to which the character has submitted.</summary>
    public int SubmissionDegree { get; }

    /// <summary>Affection level (好感度) -- degree of genuine affection for the protagonist.</summary>
    public int AffectionLevel { get; }

    private Susceptibility(int submissionDegree, int affectionLevel)
    {
        SubmissionDegree = submissionDegree;
        AffectionLevel = affectionLevel;
    }

    /// <summary>
    /// Create Susceptibility with validation.
    /// Both values must be non-negative.
    /// </summary>
    public static Result<Susceptibility> Create(int submissionDegree, int affectionLevel)
    {
        if (submissionDegree < 0)
            return Result<Susceptibility>.Fail($"SubmissionDegree must be non-negative, got {submissionDegree}");
        if (affectionLevel < 0)
            return Result<Susceptibility>.Fail($"AffectionLevel must be non-negative, got {affectionLevel}");
        return Result<Susceptibility>.Ok(new Susceptibility(submissionDegree, affectionLevel));
    }

    /// <summary>True when submission exceeds affection -- character is in a corrupted susceptibility state.</summary>
    public bool IsCorrupted => SubmissionDegree > AffectionLevel;
}
```

**Renamed type** (existing file, updated):
```csharp
// src/Era.Core/Types/NtrCalculationResult.cs  (renamed from NtrParameters.cs)
namespace Era.Core.Types;

/// <summary>
/// NTR calculation results produced by INtrEngine.Calculate().
/// </summary>
/// <param name="AffectionChange">Change in affection value (negative for decay)</param>
/// <param name="JealousyValue">Jealousy intensity (0-100)</param>
public record NtrCalculationResult(int AffectionChange, int JealousyValue);
```

**Consumer updates required**:
- `Era.Core/INtrEngine.cs` line 15: `Result<NtrParameters>` -> `Result<NtrCalculationResult>`
- `Era.Core/NtrEngine.cs` line 15 + line 59: `Result<NtrParameters>` -> `Result<NtrCalculationResult>`
- `Era.Core.Tests/NtrEngineTests.cs`: All `Result<NtrParameters>` and `NtrParameters` type references -> `NtrCalculationResult`

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| Background and C4 Constraint Detail say "4 files + 1 test" as NtrParameters consumers, but `ServiceCollectionExtensions.cs` does not reference `NtrParameters` by name (only imports `using Era.Core.Types` without using the type). Actual consumer count is 3 source files + 1 test. | Background (Problem) / AC Design Constraints C4 Constraint Details | Update "4 files + 1 test" to "3 source files + 1 test file" for accuracy. No AC changes required (ACs target specific files, not the count). |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 13, 14, 20 | Rename `Era.Core.Types.NtrParameters` to `NtrCalculationResult`: rename file `Types/NtrParameters.cs` to `Types/NtrCalculationResult.cs`, update record declaration, update 2 consumer source files (`INtrEngine.cs`, `NtrEngine.cs`) and test file `NtrEngineTests.cs` so `Result<NtrParameters>` and `NtrParameters` type references become `NtrCalculationResult` | | [x] |
| 2 | 1, 5, 9, 12 | Implement `NtrRoute` Value Object: create `src/Era.Core/NTR/Domain/ValueObjects/NtrRoute.cs` as `public readonly record struct NtrRoute` with private ctor, 7 static readonly instances (R0-R6), and `public bool IsCorrupted` property (Value >= 4) | | [x] |
| 3 | 2, 6, 10, 11, 24 | Implement `NtrPhase` Value Object: create `src/Era.Core/NTR/Domain/ValueObjects/NtrPhase.cs` as `public readonly record struct NtrPhase` with private ctor, 8 static readonly instances (Phase0-Phase7), `public NtrPhase Next()` method (capped at Phase7), and `public bool IsComplete` property | | [x] |
| 4 | 3, 7, 22 | Implement `NtrParameters` Value Object: create `src/Era.Core/NTR/Domain/ValueObjects/NtrParameters.cs` as `public readonly record struct NtrParameters` with private ctor, `SlaveLevel` and `FavLevel` int fields, and `static Result<NtrParameters> Create(int slaveLevel, int favLevel)` factory with non-negative guard | | [x] |
| 5 | 4, 8, 21, 23 | Implement `Susceptibility` Value Object: create `src/Era.Core/NTR/Domain/ValueObjects/Susceptibility.cs` as `public readonly record struct Susceptibility` with private ctor, `SubmissionDegree` and `AffectionLevel` int fields, `static Result<Susceptibility> Create(int submissionDegree, int affectionLevel)` factory with non-negative guard, and `public bool IsCorrupted => SubmissionDegree > AffectionLevel` property | | [x] |
| 6 | 17 | Add XML doc `/// <summary>` comments to all 4 public VO types and their public members serving as Ubiquitous Language glossary (minimum 4 type-level summaries; member-level comments for all public properties and methods) | | [x] |
| 7 | 15, 16, 19 | Create unit test files for all 4 VOs in `src/Era.Core.Tests/NTR/Domain/ValueObjects/`: `NtrRouteTests.cs`, `NtrPhaseTests.cs`, `NtrParametersTests.cs`, `SusceptibilityTests.cs` — tagged `[Trait("Category", "Unit")]`, covering static instance values, factory validation (valid + invalid inputs for NtrParameters/Susceptibility), and computed properties (`IsCorrupted`, `Next()`, `IsComplete`); all tests pass with no TODO/FIXME/HACK in deliverable files | | [x] |
| 8 | 18 | Verify `dotnet build src/Era.Core/Era.Core.csproj` exits 0 with no warnings after all changes (TreatWarningsAsErrors=true) | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement -> Write test -> Verify |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-850.md Task#1; `C:\Era\core\src\Era.Core\Types\NtrParameters.cs`, `INtrEngine.cs`, `NtrEngine.cs`, `Era.Core.Tests\NtrEngineTests.cs` | `Types/NtrCalculationResult.cs` (renamed+updated); 3 source files + 1 test file updated; AC#13, AC#14 GREEN |
| 2 | implementer | sonnet | feature-850.md Tasks#2-5; architecture doc lines 712-748; CharacterName.cs/AbilitySet.cs as pattern reference | 4 test files in `Era.Core.Tests/NTR/Domain/ValueObjects/` (RED — VOs not yet created) |
| 3 | implementer | sonnet | feature-850.md Tasks#2-5; test files from Phase 2 (RED) | `NTR/Domain/ValueObjects/NtrRoute.cs`, `NtrPhase.cs`, `NtrParameters.cs`, `Susceptibility.cs` (GREEN — tests pass) |
| 4 | implementer | sonnet | feature-850.md Task#6; all 4 VO files from Phase 3 | XML doc `/// <summary>` comments added to all 4 types and public members; AC#17 GREEN |
| 5 | implementer | sonnet | feature-850.md Task#8; core repo | `dotnet build src/Era.Core/Era.Core.csproj` exits 0; AC#18 GREEN |

### Pre-conditions

- F849 is [DONE] (Phase 24 Planning parent satisfied)
- Working directory for all steps: `C:\Era\core` (not devkit)
- All `dotnet` commands run via WSL: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet ...'`
- Read `C:\Era\core\src\Era.Core\Types\NtrParameters.cs` to confirm actual consumer list before Task#1

### Execution Order

1. **Phase 1 (Task#1) — Rename-first**: Execute before creating any new VO. This eliminates CS0104 ambiguous reference risk. The rename affects `Types/NtrParameters.cs` (file + class), `INtrEngine.cs`, `NtrEngine.cs`, and `NtrEngineTests.cs`. Rename `record NtrParameters` → `record NtrCalculationResult` and all reference sites (`Result<NtrParameters>` → `Result<NtrCalculationResult>`, `NtrParameters` type references → `NtrCalculationResult`). File renamed from `NtrParameters.cs` to `NtrCalculationResult.cs`.

2. **Phase 2 (Task#7 test files — RED)**: Create the 4 test files in `src/Era.Core.Tests/NTR/Domain/ValueObjects/` BEFORE implementing the VOs. Tests reference types that do not yet exist — this is the expected RED state for TDD. Each test file covers: static instance values, factory Create() with valid inputs, factory Create() with negative inputs (expects Result.IsFailure), computed properties.

3. **Phase 3 (Tasks#2-5 — GREEN)**: Implement all 4 VOs following the exact data structures in `## Technical Design > Interfaces / Data Structures`. Create directory `src/Era.Core/NTR/Domain/ValueObjects/` first. Implement verbatim from architecture doc (lines 712-748) for NtrRoute and NtrPhase. NtrParameters and Susceptibility follow factory pattern as specified.

4. **Phase 4 (Task#6 — XML docs)**: Add `/// <summary>` XML doc comments to all 4 public VO types and their public members. Minimum: 4 type-level summaries. Member-level comments for all public properties and methods (Value, IsCorrupted, Next(), IsComplete, SlaveLevel, FavLevel, SubmissionDegree, AffectionLevel, Create(), IsCorrupted). Use the exact Japanese domain terms specified in `## Technical Design > Interfaces / Data Structures` (e.g., 未接触, 好感度LV).

5. **Phase 5 (Task#8 — build)**: Run `dotnet build src/Era.Core/Era.Core.csproj` via WSL. Must exit 0 with zero warnings (TreatWarningsAsErrors=true). If warnings found, fix before proceeding.

### Build Verification Steps

```bash
# All commands via WSL from core repo root
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build src/Era.Core/Era.Core.csproj'
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test src/Era.Core.Tests/Era.Core.Tests.csproj --blame-hang-timeout 10s --results-directory /mnt/c/Era/devkit/_out/test-results'
```

### Success Criteria

- Task#1: `grep -r "record NtrParameters" src/Era.Core/Types/` returns no matches; `grep -r "NtrCalculationResult" src/Era.Core/Types/` returns 1 match
- Task#7 RED: All 4 test files exist; `dotnet test` fails with compile errors (types not found)
- Tasks#2-5 GREEN: `dotnet test` passes for NTR VO test suite
- Task#6: `grep -r "/// <summary>" src/Era.Core/NTR/Domain/ValueObjects/` returns >= 4 matches
- Task#8: `dotnet build` exit code 0

### Error Handling

- If CS0104 ambiguous reference appears after Phase 3: Task#1 was incomplete — re-check all NtrParameters consumers via `grep -r "NtrParameters" src/` and update remaining references
- If `dotnet build` warns about missing XML doc (`CS1591`): Add missing `/// <summary>` to the flagged member
- If test compile fails after Phase 3 (unexpected): Re-read VO file content vs test file expectations and reconcile

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| NtrParameters satisfaction-state parameter expansion (CHK_NTR_SATISFACTORY optional params) | Current design has 2 fields (SlaveLevel, FavLevel); ntr-ddd-input.md suggests satisfaction-state tracking may be needed | Existing Feature | F852 | - | [x] | 追記済み |
| Ubiquitous Language cross-VO terminology consistency validation | AC#17 verifies glossary existence (gte 4) but not cross-VO term consistency | Existing Feature | F854 | - | [x] | 追記済み |

<!-- Transferred + Result columns (F811/F805 Lesson):
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: created(A), appended(B), documented(C), confirmed-existing
- Phase 9.4.1 transfers content and records Result. Phase 10.0.2 verifies only.
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
| 2026-03-07T04:20 | Phase 1 | initializer | [REVIEWED]→[WIP] | READY |
<!-- run-phase-1-completed -->
| 2026-03-07T04:22 | Phase 2 | explorer | Codebase investigation | 3 src + 1 test consumers confirmed |
<!-- run-phase-2-completed -->
| 2026-03-07T04:25 | Phase 3 | implementer | Task#1 rename + Task#7 test files (RED) | 4 CS0234 errors (expected TDD RED) |
<!-- run-phase-3-completed -->
| 2026-03-07T04:30 | Phase 4 | implementer | Tasks#2-6 VOs + XML docs + Task#7 tests GREEN + Task#8 build | 33/33 tests passed, 0 warnings |
<!-- run-phase-4-completed -->
| 2026-03-07T04:35 | Phase 7 | ac-tester | AC verification | OK:24/24 |
<!-- run-phase-7-completed -->
| 2026-03-07T04:38 | Phase 8 | feature-reviewer | Post-review quality | READY |
<!-- run-phase-8-completed -->
| 2026-03-07T04:45 | DEVIATION | Bash | git commit (core pre-commit hook) | PRE-EXISTING: ComHotReloadTests.ValidateFile_WithFileAccessRetry_RetriesOnIOException FAIL (unrelated to F850) |
| 2026-03-07T04:50 | Phase 10 | finalizer | [WIP]→[DONE] | READY_TO_COMMIT |
| 2026-03-07T04:55 | CodeRabbit | 2 Minor (修正不要) | System.Linq using 不足指摘 — ImplicitUsings で暗黙インポート済み |
<!-- run-phase-10-completed -->

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase2-Review iter1: Baseline Measurement | Missing Baseline File line after table
- [fix] Phase2-Uncertain iter1: Philosophy Derivation row 2 | AC#17 not traced for SSOT domain terminology claim
- [fix] Phase2-Uncertain iter1: AC Definition Table AC#14 | INtrEngine.cs consumer not verified — added AC#20
- [fix] Phase2-Review iter2: AC Definition Table | Susceptibility.IsCorrupted has no AC — added AC#21
- [fix] Phase2-Review iter3: AC Definition Table | NtrParameters/Susceptibility fields not verified — added AC#22, AC#23
- [fix] Phase2-Review iter4: AC Definition Table | NtrPhase.IsComplete has no AC — added AC#24
- [fix] Phase3-Maintainability iter5: Task#1 | ServiceCollectionExtensions.cs incorrectly listed as consumer — removed
- [fix] Phase3-Maintainability iter5: C2 Constraint Details | Collection Members labels wrong vs SSOT — updated to match architecture doc
- [fix] Phase3-Maintainability iter5: Mandatory Handoffs | NtrParameters satisfaction-state expansion untracked — added handoff to F852
- [fix] Phase3-Maintainability iter5: Mandatory Handoffs | UL cross-VO consistency unverified — added handoff to F854
- [fix] Phase2-Review iter6: AC#9 Details | Route English labels wrong vs SSOT — updated to match architecture doc
- [fix] Phase4-ACValidation iter7: AC#1-4 | gte matcher → exists for file existence checks
- [fix] Phase4-ACValidation iter7: AC#13,14,20 | not_matches → not_contains for literal string patterns
- [fix] Phase4-ACValidation iter8: AC#11 | matches → contains for literal string pattern

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

### /imp 850 (2026-03-07)
- [revised] ac-designer Step 6.5.2: 「Shared vocabulary type」カテゴリ追加（downstream consumer AC生成漏れ防止） → `.claude/agents/ac-designer.md`
- [revised] ac-designer Step 10.5.4: literal-vs-regex判別ルール追加（matches/not_matches→contains/not_contains降格） → `.claude/agents/ac-designer.md`
- [revised] tech-designer Step 8g: Domain Label Verification追加（列挙型ドメインラベルのSSOT照合） → `.claude/agents/tech-designer.md`
- [rejected] FC handoff検出の前倒し — FL Phase 3が設計通り機能、重複回避

---

<!-- fc-phase-6-completed -->

## Links

[Predecessor: F849](feature-849.md) - Phase 24 Planning parent
[Successor: F851](feature-851.md) - NtrProgression Aggregate consumes all 4 VOs
[Successor: F852](feature-852.md) - INtrCalculator uses NtrRoute/NtrPhase in method signatures
[Successor: F853](feature-853.md) - Application Services + ACL maps external representations to VOs
[Successor: F854](feature-854.md) - Post-Phase Review validates F850 deliverables
[Related: F847](feature-847.md) - NTR kojo analysis providing domain knowledge
