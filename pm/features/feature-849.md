# Feature 849: Phase 24 Planning

## Status: [DONE]
<!-- fl-reviewed: 2026-03-07T00:21:24Z -->

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

## Type: research

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity — each completed phase triggers planning for the next phase, producing sub-feature DRAFTs that are the SSOT for the next phase's scope. Phase 24 (NTR Bounded Context) is the first DDD design phase in the NTR system migration, requiring decomposition by DDD layer (Domain/Application/Infrastructure) rather than by file dependency graph (F783/F814 pattern) or document creation (F827 pattern).

### Problem (Current Issue)

Phase 24 is a DDD design+implementation hybrid phase (not pure research like Phase 23, nor pure ERB migration like Phases 20-22), requiring a planning methodology that decomposes 11 architecture-doc Tasks (`docs/architecture/migration/phase-20-27-game-systems.md:591-602`) into sub-features aligned with DDD layer boundaries. The current F849 DRAFT is a minimal skeleton created by F827 Task 4 (`feature-827.md:285`) with no decomposition strategy. Phase 24 introduces NEW C# DDD concepts (NtrRoute R0-R6, NtrPhase 0-7) not present in ERB source (`phase-20-27-game-systems.md:575`), meaning the planning must respect both the empirical input from Phase 23 analysis (`ntr-ddd-input.md`) and the architecture doc's prescribed DDD structure.

### Goal (What to Achieve)

Decompose Phase 24 (NTR Bounded Context design) into 3-4 implementation sub-features plus 2 transition sub-features, grouped by DDD layer. Each implementation sub-feature is Type: engine (producing C# code in `core` repo). Each sub-feature DRAFT includes Philosophy inheritance, TODO/FIXME debt cleanup scope, and zero-debt AC per Sub-Feature Requirements (`phase-20-27-game-systems.md:777-783`). All 11 architecture-doc Tasks are covered. Transition features (Post-Phase Review Phase 24 + Phase 25 Planning) are created as Type: infra and Type: research respectively.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is F849 incomplete? | F849 is a bare DRAFT skeleton with no Tasks, ACs, or decomposition strategy | `pm/features/feature-849.md:1-31` |
| 2 | Why was it left as a skeleton? | F827 Task 4 created it as a minimal stub expecting `/fc` to complete it | `pm/features/feature-827.md:285` |
| 3 | Why can't existing planning patterns (F783/F814) be applied directly? | Phase 24 has 11 Tasks spanning DDD design+implementation, not ERB file migration | `docs/architecture/migration/phase-20-27-game-systems.md:591-602` |
| 4 | Why is Phase 24 qualitatively different? | It introduces NEW DDD concepts (NtrRoute, NtrPhase) not present in ERB source, combining empirical domain modeling with new construct creation | `docs/architecture/migration/phase-20-27-game-systems.md:575` |
| 5 | Why (Root)? | Planning methodology must decompose by DDD layer (Domain/Application/Infrastructure) because Phase 24 produces compilable C# code organized by Aggregate/VO/Event/Service boundaries, not by source file dependencies | `docs/architecture/migration/phase-20-27-game-systems.md:639-671` |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | F849 DRAFT is an empty skeleton with no decomposition | Phase 24 is a DDD hybrid phase requiring layer-based decomposition methodology |
| Where | `pm/features/feature-849.md` | Architecture doc Phase 24 section (`phase-20-27-game-systems.md:567-790`) |
| Fix | Copy F827 planning pattern | Develop DDD-layer-aligned sub-feature grouping from 11 Tasks |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F848 | [DONE] | Predecessor: Post-Phase Review Phase 23 (unblocks F849) |
| F847 | [DONE] | Related: Phase 23 NTR Kojo Reference Analysis (produced ntr-ddd-input.md, ntr-kojo-analysis.md) |
| F827 | [DONE] | Related: Phase 23 Planning (parent that created F849 DRAFT; methodology reference) |
| F783 | [DONE] | Related: Phase 21 Planning (migration planning pattern, partially applicable) |
| F814 | [DONE] | Related: Phase 22 Planning (migration planning pattern, partially applicable) |
| F829 | [DONE] | Related: Phase 22 Deferred Obligations Consolidation (OB-03 INtrQuery routes to Phase 24 definition, Phase 25 wiring) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Phase 24 scope clearly defined | FEASIBLE | `phase-20-27-game-systems.md:567-790`: 11 Tasks, 7 deliverable groups, 4 Success Criteria, directory tree |
| Phase 23 analysis input available | FEASIBLE | Both `ntr-ddd-input.md` (122 lines) and `ntr-kojo-analysis.md` (277 lines) exist and are complete |
| Predecessor F848 satisfied | FEASIBLE | F848 [DONE], Phase 23 Phase Status = DONE |
| Sub-feature decomposition possible | FEASIBLE | Architecture doc Tasks 1-11 provide natural grouping boundaries along DDD layers |
| Next Feature number available | FEASIBLE | Current Next Feature = 850; sufficient range for 5-6 sub-features |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Feature Registry | MEDIUM | 5-6 new sub-feature DRAFTs registered in index-features.md |
| Phase 24 Scope | HIGH | Defines the complete scope and ordering of Phase 24 implementation |
| Dependency Chain | MEDIUM | Establishes predecessor/successor relationships among sub-features |
| core Repo | LOW | No direct code changes; sub-features will produce C# code when executed |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Phase 24 deliverables are C# code, not documents | `phase-20-27-game-systems.md:751-761` | Implementation sub-features must be Type: engine |
| NtrRoute R0-R6 and NtrPhase 0-7 are NEW concepts | `phase-20-27-game-systems.md:575` | Design decisions required during implementation, not derivable from ERB source |
| Sub-Feature Requirements mandate Philosophy inheritance, debt cleanup, zero-debt AC | `phase-20-27-game-systems.md:777-783` | Every sub-feature DRAFT must include these elements |
| NTR Mark Integration (F406) is Phase 25 scope, NOT Phase 24 | `phase-20-27-game-systems.md:604-637` | F849 must NOT include NTR Mark tasks in Phase 24 sub-features |
| Transition features mandatory (Tasks 10-11) | `phase-20-27-game-systems.md:601-602` | Post-Phase Review Phase 24 + Phase 25 Planning DRAFTs required |
| Phase 13 DDD Foundation provides AggregateRoot base class | `docs/architecture/migration/full-csharp-architecture.md:385` | Phase 24 sub-features have implicit dependency on Phase 13 completion |
| Deliverables live in core repo | `phase-20-27-game-systems.md:642-671` | Sub-features require cross-repo implementation (`C:\Era\core`) |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Applying ERB migration template to DDD design phase | HIGH | HIGH | Document DDD-layer decomposition rationale explicitly in each sub-feature |
| Over-decomposition into too many sub-features | MEDIUM | MEDIUM | Keep to 5-6 sub-features (3-4 implementation + 2 transition) |
| Grounded concepts (NtrRoute, NtrPhase, NtrParameters) require design decisions not yet made | MEDIUM | MEDIUM | Mark grounded-concept tasks with [I] tag; reference ntr-ddd-input.md validation status |
| Phase 13 DDD Foundation not yet complete | LOW | HIGH | Phase 24 execution is future; Phase 13 dependency documented in sub-feature Dependencies |
| Sub-feature ordering incorrect (e.g., Aggregate before Value Objects) | LOW | MEDIUM | Ensure proper predecessor chain: Language/VOs before Aggregate, Aggregate before Services |

---

<!-- fc-phase-2-completed -->
## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Architecture doc prescribes 11 Tasks for Phase 24 | `phase-20-27-game-systems.md:591-602` | All 11 Tasks must be covered by sub-features; AC verifies coverage |
| C2 | Sub-Feature Requirements: Philosophy, debt cleanup, zero-debt AC | `phase-20-27-game-systems.md:777-783` | Every sub-feature DRAFT must include these; AC verifies presence |
| C3 | Transition features mandatory (Tasks 10-11) | `phase-20-27-game-systems.md:601-602` | Two transition sub-feature DRAFTs required; AC verifies file existence and index registration |
| C4 | F849 itself is Type: research | `feature-849.md:5` | F849 produces DRAFT files, not C# code; ACs use Glob/Grep matchers |
| C5 | DDD input must be referenced in sub-features | `pm/reference/ntr-ddd-input.md` | Sub-feature backgrounds must reference empirical validation status (Validated vs Grounded) |
| C6 | NTR Mark Integration is Phase 25 scope | `phase-20-27-game-systems.md:604-637` | Must NOT include NTR Mark tasks in Phase 24 sub-features |
| C7 | Next Feature number must be incremented correctly | `pm/index-features.md` | After allocating N IDs, Next Feature = 850 + N |
| C8 | Sub-features follow DDD layer ordering | DDD convention + `phase-20-27-game-systems.md:639-671` | Predecessor declarations must enforce: Language/VOs before Aggregate, Aggregate before Services |

### Constraint Details

**C1: 11-Task Coverage**
- **Source**: Architecture doc Phase 24 section, Tasks 1-11
- **Verification**: Count sub-feature task mappings against architecture Tasks 1-11
- **AC Impact**: AC must verify all 11 Tasks are assigned to sub-features (no orphan tasks)
- **Collection Members** (MANDATORY): Task 1: NTR Ubiquitous Language定義（用語辞書）, Task 2: NtrProgression Aggregate設計・実装, Task 3: NtrRoute Value Object実装（R0-R6）, Task 4: NtrPhase Value Object実装（Phase 0-7）, Task 5: NtrParameters Value Object実装（FAV_*, 露出度等）, Task 6: NTR Domain Events定義（PhaseAdvanced, RouteChanged等）, Task 7: INtrCalculator Domain Service設計, Task 8: NTR Application Services設計, Task 9: Anti-Corruption Layer設計（既存システムとの橋渡し）, Task 10: Post-Phase Review Phase 24, Task 11: Phase 25 Planning

**C2: Sub-Feature Requirements**
- **Source**: `phase-20-27-game-systems.md:777-783`
- **Verification**: Grep each sub-feature DRAFT for Philosophy section and debt-cleanup scope
- **AC Impact**: AC should verify Philosophy inheritance text and TODO/FIXME/HACK cleanup scope in each sub-feature

**C3: Transition Features**
- **Source**: Architecture doc Tasks 10-11
- **Verification**: Glob for transition feature files + Grep index-features.md for registration
- **AC Impact**: Both file existence AND index registration must be verified (AND conjunction per RESEARCH.md Issue 16)

**C4: F849 is Type: research**
- **Source**: `feature-849.md:17`
- **Verification**: F849 produces markdown DRAFT files, not C# code
- **AC Impact**: ACs use Glob/Grep matchers (file existence, content verification), not dotnet_test

**C5: DDD Input Referenced**
- **Source**: `pm/reference/ntr-ddd-input.md`
- **Verification**: Grep each implementation sub-feature for "ntr-ddd-input" or "Validated"/"Grounded" language
- **AC Impact**: Ensures sub-features distinguish empirically Validated vs Grounded DDD concepts

**C6: NTR Mark Exclusion**
- **Source**: `phase-20-27-game-systems.md:604-637`
- **Verification**: Grep sub-feature files for NTR Mark / MarkIndex / GET_MARK_LEVEL — must return zero matches
- **AC Impact**: Prevents Phase 25 scope from leaking into Phase 24 sub-features

**C7: Next Feature Number**
- **Source**: `pm/index-features.md` Next Feature number field
- **Verification**: Grep for "Next Feature number" in index after sub-feature allocation
- **AC Impact**: Prevents ID collision with subsequent `/fc` invocations

**C8: DDD Layer Ordering**
- **Source**: DDD convention + `phase-20-27-game-systems.md:639-671`
- **Verification**: Grep F851/F852/F853 for "Predecessor" declarations enforcing Value Objects → Aggregate → Services → Application ordering
- **AC Impact**: Ensures sub-features execute in correct dependency order

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F848 | [DONE] | Post-Phase Review Phase 23 -- must be [DONE] before F849 proceeds |
| Related | F847 | [DONE] | Phase 23 NTR Kojo Reference Analysis (produced ntr-ddd-input.md) |
| Related | F827 | [DONE] | Phase 23 Planning (parent that created F849 DRAFT) |
| Related | F829 | [DONE] | Phase 22 Deferred Obligations (OB-03 INtrQuery routes to Phase 24 definition) |
| Related | F783 | [DONE] | Phase 21 Planning (migration planning pattern reference) |
| Related | F814 | [DONE] | Phase 22 Planning (migration planning pattern reference) |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} → This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This → F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
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

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "producing sub-feature DRAFTs that are the SSOT for the next phase's scope" | All sub-feature DRAFT files must exist on disk | AC#1 |
| "producing sub-feature DRAFTs that are the SSOT for the next phase's scope" | All sub-features must be registered in index-features.md | AC#2 |
| "producing sub-feature DRAFTs that are the SSOT for the next phase's scope" | All 11 architecture Tasks must be covered (completeness = SSOT) | AC#3 |
| "each completed phase triggers planning for the next phase" | Transition features (Post-Phase Review + Next Phase Planning) must be created | AC#1, AC#2 |
| "each completed phase triggers planning for the next phase" | Transition features must have correct Types (F854=infra, F855=research) | AC#11, AC#12 |
| "each completed phase triggers planning for the next phase" | Transition feature predecessor chain must enforce sequential ordering | AC#13 |
| "requiring decomposition by DDD layer (Domain/Application/Infrastructure) rather than by file dependency graph" | Sub-features must be grouped by DDD layer, not by file | AC#9 |
| "requiring decomposition by DDD layer (Domain/Application/Infrastructure) rather than by file dependency graph" | Predecessor chain must enforce DDD layer ordering | AC#6 |
| "requiring decomposition by DDD layer (Domain/Application/Infrastructure) rather than by file dependency graph" | DDD input validation status must be referenced | AC#10 |
| "producing sub-feature DRAFTs that are the SSOT for the next phase's scope" | Philosophy inheritance must be present in each sub-feature | AC#4 |
| "producing sub-feature DRAFTs that are the SSOT for the next phase's scope" | Debt cleanup scope must be included | AC#5 |
| "producing sub-feature DRAFTs that are the SSOT for the next phase's scope" | Next Feature number must be incremented to prevent ID collision | AC#7 |
| "producing sub-feature DRAFTs that are the SSOT for the next phase's scope" | NTR Mark tasks must be excluded from Phase 24 scope | AC#8 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | All sub-feature DRAFT files exist | file | Glob(pm/features/feature-85[0-5].md) | gte | 5 | [x] |
| 2 | All sub-features registered in index-features.md | file | Grep(pm/index-features.md, pattern="F85[0-5]") | gte | 5 | [x] |
| 3 | All 11 architecture Tasks referenced in sub-feature files | file | Grep(pm/features/feature-85[0-5].md, pattern="Architecture Task [0-9]+") | gte | 11 | [x] |
| 4 | Philosophy inheritance in each implementation sub-feature | file | Grep(pm/features/feature-85[0-3].md, pattern="Phase 24.*NTR Bounded Context") | gte | 3 | [x] |
| 5 | Debt cleanup AC in each implementation sub-feature | file | Grep(pm/features/feature-85[0-3].md, pattern="TODO\|FIXME\|HACK") | gte | 3 | [x] |
| 6 | DDD layer ordering: F851→F850, F852→F851, F853→F852 predecessor chain | file | Grep(pm/features/feature-85[1-3].md, pattern="Predecessor.*F85[0-2]") | gte | 3 | [x] |
| 7 | Next Feature number incremented past allocated range | file | Grep(pm/index-features.md, pattern="Next Feature number") | matches | 85[6-9]\|8[6-9][0-9] | [x] |
| 8 | NTR Mark tasks excluded from Phase 24 sub-features | file | Grep(pm/features/feature-85[0-5].md, pattern="NTR Mark\|NTR mark\|MarkIndex\|GET_MARK_LEVEL") | not_matches | NTR Mark\|NTR mark\|MarkIndex\|GET_MARK_LEVEL | [x] |
| 9 | Implementation sub-features are Type: engine | file | Grep(pm/features/feature-85[0-3].md, pattern="Type: engine") | gte | 3 | [x] |
| 10 | DDD input validation status referenced in sub-features | file | Grep(pm/features/feature-85[0-3].md, pattern="ntr-ddd-input\|Validated\|Grounded") | gte | 3 | [x] |
| 11 | Transition features have correct Types (F854=infra, F855=research) | file | Grep(pm/features/feature-854.md, pattern="Type: infra") | gte | 1 | [x] |
| 12 | Phase 25 Planning is Type: research | file | Grep(pm/features/feature-855.md, pattern="Type: research") | gte | 1 | [x] |
| 13 | Transition feature predecessor chain (F854→F853, F855→F854) | file | Grep(pm/features/feature-85[4-5].md, pattern="Predecessor.*F85[3-4]") | gte | 2 | [x] |

### AC Details

**AC#1: All sub-feature DRAFT files exist**
- **Test**: `Glob(pm/features/feature-85[0-5].md)`
- **Expected**: `gte 5` (3-4 implementation + 2 transition sub-features)
- **Derivation**: Goal specifies 3-4 implementation sub-features + 2 transition sub-features = 5-6 total. Minimum 5.
- **Rationale**: RESEARCH.md Issue 16 requires file existence verification (not just index registration). Constraint C3.

**AC#2: All sub-features registered in index-features.md**
- **Test**: `Grep(pm/index-features.md, pattern="F85[0-5]")`
- **Expected**: `gte 5`
- **Derivation**: Same as AC#1 — 5-6 sub-features must all appear in index. Minimum 5.
- **Rationale**: RESEARCH.md Issue 16 requires both file existence AND index registration. Constraint C3.

**AC#3: All 11 architecture Tasks referenced in sub-feature files**
- **Test**: `Grep(pm/features/feature-85[0-5].md, pattern="Architecture Task [0-9]+")`
- **Expected**: `gte 11` (each sub-feature references its assigned Tasks; 11 total across 6 files)
- **Derivation**: Architecture doc Phase 24 prescribes exactly 11 Tasks (Tasks 1-11 at phase-20-27-game-systems.md:591-602). Each sub-feature file must reference the architecture Tasks it covers using "Architecture Task N" markers. Verified across sub-feature files, not self-referentially in F849.
- **Rationale**: Constraint C1 — no orphan tasks allowed. Verified in deliverable files, not planning doc. Implementer must use exactly one "Architecture Task N" marker per assigned task with no duplicates to ensure count reflects distinct coverage.

**AC#4: Philosophy inheritance in each implementation sub-feature**
- **Test**: `Grep(pm/features/feature-85[0-3].md, pattern="Phase 24.*NTR Bounded Context")`
- **Expected**: `gte 3` (minimum 3 implementation sub-features)
- **Derivation**: Sub-Feature Requirements (phase-20-27-game-systems.md:781) mandate Philosophy inheritance with "Phase 24: NTR Bounded Context" text. 3-4 implementation sub-features, minimum 3.
- **Rationale**: Constraint C2 — Philosophy inheritance required.

**AC#5: Debt cleanup AC in each implementation sub-feature**
- **Test**: `Grep(pm/features/feature-85[0-3].md, pattern="TODO|FIXME|HACK")`
- **Expected**: `gte 3`
- **Derivation**: Sub-Feature Requirements (phase-20-27-game-systems.md:782-783) mandate debt cleanup tasks and zero-debt ACs. Each implementation sub-feature must reference TODO/FIXME/HACK pattern in its AC table. Minimum 3 implementation sub-features.
- **Rationale**: Constraint C2 — zero-debt AC required.

**AC#6: DDD layer ordering: F851→F850, F852→F851, F853→F852 predecessor chain**
- **Test**: `Grep(pm/features/feature-85[1-3].md, pattern="Predecessor.*F85[0-2]")`
- **Expected**: `gte 3` (F851→F850, F852→F851, F853→F852 — all 3 required)
- **Derivation**: DDD convention requires: Value Objects (F850) before Aggregate (F851), Aggregate before Services (F852), Services before Application (F853). All 3 predecessor relationships enforced.
- **Rationale**: Constraint C8 — predecessor chain enforces DDD layer ordering with specific targets, not just "Predecessor" keyword presence. Risk R5 mitigation.

**AC#7: Next Feature number incremented past allocated range**
- **Test**: `Grep(pm/index-features.md, pattern="Next Feature number")`
- **Expected**: `matches 85[6-9]|8[6-9][0-9]`
- **Rationale**: RESEARCH.md Issue 15 — prevents ID collision with subsequent /fc invocations. Constraint C7.

**AC#9: Implementation sub-features are Type: engine**
- **Test**: `Grep(pm/features/feature-85[0-3].md, pattern="Type: engine")`
- **Expected**: `gte 3`
- **Derivation**: Goal mandates each implementation sub-feature is Type: engine (producing C# code in core repo). 3-4 implementation sub-features, minimum 3.
- **Rationale**: Goal item 3 requires engine type for implementation sub-features.

**AC#10: DDD input validation status referenced in sub-features**
- **Test**: `Grep(pm/features/feature-85[0-3].md, pattern="ntr-ddd-input|Validated|Grounded")`
- **Expected**: `gte 3`
- **Derivation**: Constraint C5 requires sub-feature backgrounds to reference empirical validation status from ntr-ddd-input.md. Each of 3-4 implementation sub-features must reference this. Minimum 3.
- **Rationale**: Constraint C5 — DDD input traceability ensures sub-features distinguish Validated vs Grounded concepts.

**AC#11: Transition features have correct Types (F854=infra)**
- **Test**: `Grep(pm/features/feature-854.md, pattern="Type: infra")`
- **Expected**: `gte 1`
- **Derivation**: Goal Item 6 mandates F854 is Type: infra. Constraint C3 requires transition features.
- **Rationale**: Ensures transition features match their prescribed types.

**AC#12: Phase 25 Planning is Type: research**
- **Test**: `Grep(pm/features/feature-855.md, pattern="Type: research")`
- **Expected**: `gte 1`
- **Derivation**: Goal Item 6 mandates F855 is Type: research.
- **Rationale**: Ensures transition features match their prescribed types.

**AC#13: Transition feature predecessor chain (F854→F853, F855→F854)**
- **Test**: `Grep(pm/features/feature-85[4-5].md, pattern="Predecessor.*F85[3-4]")`
- **Expected**: `gte 2` (F854→F853 and F855→F854)
- **Derivation**: Philosophy "each completed phase triggers planning for the next phase" requires sequential ordering. F854 must depend on F853 (last implementation sub-feature), F855 must depend on F854 (post-phase review before next phase planning).
- **Rationale**: Completes pipeline continuity verification for transition features. AC#6 covers DDD layer ordering (F851-F853); AC#13 covers transition feature ordering (F854-F855).

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Decompose Phase 24 into 3-4 implementation sub-features + 2 transition sub-features | AC#1, AC#2 |
| 2 | Grouped by DDD layer | AC#3, AC#6 |
| 3 | Each implementation sub-feature is Type: engine | AC#9 |
| 4 | Each sub-feature DRAFT includes Philosophy inheritance, debt cleanup, zero-debt AC | AC#4, AC#5 |
| 5 | All 11 architecture-doc Tasks are covered | AC#3 |
| 6 | Transition features (Post-Phase Review Phase 24 + Phase 25 Planning) created as Type: infra and Type: research | AC#1, AC#2, AC#11, AC#12, AC#13 |
| 7 | NTR Mark Integration excluded from Phase 24 scope | AC#8 |
| 8 | Next Feature number incremented to prevent ID collision | AC#7 |
| 9 | Sub-feature backgrounds reference ntr-ddd-input.md Validated vs Grounded status | AC#10 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

F849 is a research-type planning feature. Its deliverable is a set of sub-feature DRAFT files plus index registration — no C# code, no tests, no builds. The implementation approach is:

1. **Allocate six sub-feature IDs** from the current Next Feature number (850): F850–F855.
2. **Create six DRAFT feature files** in `pm/features/`, grouped by DDD layer:

   **Domain Layer (Tasks 1-7)**:
   - `feature-850.md`: NTR Ubiquitous Language + Value Objects (Type: engine). Covers Tasks 1, 3, 4, 5 — language definitions and all four Value Object types (NtrRoute, NtrPhase, NtrParameters, Susceptibility). Grouped together because all four tasks produce pure domain primitives with no dependencies on other sub-features.
   - `feature-851.md`: NtrProgression Aggregate + Domain Events (Type: engine). Covers Tasks 2, 6 — Aggregate root and the domain events it raises. Aggregate design requires Value Objects defined in F850; domain events are raised by the Aggregate, making co-location natural.
   - `feature-852.md`: INtrCalculator Domain Service (Type: engine). Covers Task 7 — the single Domain Service interface that operates on the Aggregate. Separated from the Aggregate because INtrCalculator is an abstraction (interface only in Phase 24) that the Aggregate depends on but that has its own design complexity.

   **Application + Infrastructure Layer (Tasks 8-9)**:
   - `feature-853.md`: Application Services + Anti-Corruption Layer (Type: engine). Covers Tasks 8, 9 — Application Commands/Queries and the Anti-Corruption Layer that bridges existing game systems. Grouped together because both layers consume the Domain Model; neither produces domain logic.

   **Transition Features (Tasks 10-11)**:
   - `feature-854.md`: Post-Phase Review Phase 24 (Type: infra). Covers Task 10.
   - `feature-855.md`: Phase 25 Planning (Type: research). Covers Task 11.

3. **Register all six** in `pm/index-features.md` under a new `### Phase 24: NTR Bounded Context` section with [DRAFT] status and correct Depends On values.
4. **Increment Next Feature number** from 850 to 856 in `pm/index-features.md`.

**Decomposition Rationale**: Architecture doc lists 11 Tasks but allocation produces 6 features because:
- Tasks 1, 3, 4, 5 (Ubiquitous Language + 3 Value Objects) are merged into F850 — all produce domain primitives with no inter-task dependencies. Per feature-template.md granularity guidelines (Type: engine ≤ 300 lines), each Value Object file is ~30–50 lines, making a single feature appropriate.
- Tasks 2, 6 (Aggregate + Domain Events) are merged into F851 — Domain Events are raised exclusively by the Aggregate, making separation artificial.
- Tasks 8, 9 (Application + ACL) are merged into F853 — both consume the domain layer and neither introduces new domain logic.
- Tasks 10, 11 are 1:1 with transition features (F854, F855).

**DDD Layer Ordering** enforced via predecessor chain:
- F851 Predecessor → F850 (Aggregate needs Value Objects)
- F852 Predecessor → F851 (Domain Service interface operates on Aggregate)
- F853 Predecessor → F852 (Application Services invoke Domain Service)
- F854 Predecessor → F853 (Post-Phase Review after all implementation)

**Sub-Feature Requirements** (from `phase-20-27-game-systems.md:777-783`) applied to F850–F853:
- Philosophy: "Phase 24: NTR Bounded Context" inherited from this feature's Philosophy section
- Debt cleanup: TODO/FIXME/HACK removal Task included in each implementation sub-feature
- Zero-debt AC: `not_matches` TODO|FIXME|HACK AC included in each implementation sub-feature

**Task Coverage Table** (for AC#3 verification):

| Task | Description | Covered By | Task N covered |
|:----:|-------------|------------|:--------------:|
| Task 1 covered | NTR Ubiquitous Language定義（用語辞書） | F850 | Task 1 covered |
| Task 2 covered | NtrProgression Aggregate設計・実装 | F851 | Task 2 covered |
| Task 3 covered | NtrRoute Value Object実装（R0-R6） | F850 | Task 3 covered |
| Task 4 covered | NtrPhase Value Object実装（Phase 0-7） | F850 | Task 4 covered |
| Task 5 covered | NtrParameters Value Object実装（FAV_*, 露出度等） | F850 | Task 5 covered |
| Task 6 covered | NTR Domain Events定義（PhaseAdvanced, RouteChanged等） | F851 | Task 6 covered |
| Task 7 covered | INtrCalculator Domain Service設計 | F852 | Task 7 covered |
| Task 8 covered | NTR Application Services設計 | F853 | Task 8 covered |
| Task 9 covered | Anti-Corruption Layer設計（既存システムとの橋渡し） | F853 | Task 9 covered |
| Task 10 covered | Post-Phase Review Phase 24 (Type: infra) | F854 | Task 10 covered |
| Task 11 covered | Phase 25 Planning (Type: research) | F855 | Task 11 covered |

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `pm/features/feature-850.md` through `feature-855.md` — 6 DRAFT files, satisfying `gte 5` |
| 2 | Register F850–F855 in `pm/index-features.md` under `### Phase 24: NTR Bounded Context` section — 6 rows satisfying `gte 5` |
| 3 | Each sub-feature file (F850–F855) includes "Architecture Task N" markers for its assigned Tasks — 11 total across 6 files, satisfying `gte 11` |
| 4 | Each of F850–F853 includes `## Background` with "Phase 24: NTR Bounded Context" in Philosophy section — satisfying `gte 3` (minimum 3 of 4 implementation sub-features) |
| 5 | Each of F850–F853 includes a debt-cleanup AC with `TODO|FIXME|HACK` pattern in its AC Definition Table — satisfying `gte 3` |
| 6 | F851 has Predecessor → F850, F852 has Predecessor → F851, F853 has Predecessor → F852 — all 3 files contain `Predecessor.*F85[0-2]` rows satisfying `gte 3` |
| 7 | Increment `Next Feature number` in `pm/index-features.md` from 850 to 856 — value `856` matches pattern `85[6-9]` |
| 8 | None of F850–F855 reference NTR Mark tasks: no mention of `NTR Mark|NTR mark|MarkIndex|GET_MARK_LEVEL` in implementation sub-feature bodies (NTR Mark is Phase 25 scope per C6) |
| 9 | Each of F850–F853 has `## Type: engine` line — satisfying `gte 3` |
| 10 | Each of F850–F853 references `ntr-ddd-input` or uses Validated/Grounded language in Background — satisfying `gte 3` |
| 11 | `feature-854.md` contains `Type: infra` — satisfying `gte 1` |
| 12 | `feature-855.md` contains `Type: research` — satisfying `gte 1` |
| 13 | F854 has `Predecessor.*F853` and F855 has `Predecessor.*F854` — 2 matches satisfying `gte 2` |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Number of implementation sub-features | A: 4 features (1 per DDD layer group), B: 7 features (1:1 with implementation Tasks), C: 3 features (Domain/Application+Infra/Transition pair) | A: 4 features (F850–F853) | Architecture doc prescribes 9 implementation tasks; grouping by domain primitive type and layer boundary produces 4 naturally scoped features within engine-type volume limits. Option B over-decomposes (each Value Object file is ~30 lines, too small for a feature). Option C under-decomposes (Domain layer Tasks 1-7 span 300+ lines). |
| Value Objects: split or merge | A: one sub-feature per VO (3 features), B: all VOs + Ubiquitous Language in one feature | B: merge into F850 | All three VOs (NtrRoute, NtrPhase, NtrParameters) and Ubiquitous Language are domain primitives with no inter-task dependencies. Each file is ~30–60 lines; merging keeps within engine-type volume limit (~300 lines total). Separate features would create artificial granularity. |
| Aggregate + Domain Events: split or merge | A: separate features, B: merge into F851 | B: merge into F851 | Domain Events are raised exclusively within NtrProgression.AdvancePhase() and NtrProgression.ChangeRoute(). Testing domain events requires the Aggregate; co-location in F851 is architecturally coherent. |
| Application + ACL grouping | A: separate features (F853 Application, F854 ACL), B: merge into one feature | B: merge into F853 | Both Application Services and ACL are consumer layers of the domain model. Neither introduces domain logic. Separation would produce two thin features of ~100 lines each. |
| Transition feature IDs | A: F854 = Post-Phase Review, F855 = Phase 25 Planning, B: reverse order | A: F854 infra, F855 research | Sequential: Post-Phase Review precedes next phase Planning per standard pattern (F848→F849 precedent). |
| Next Feature number increment | A: increment to 856 (next after F855), B: leave at 850 | A: increment to 856 | Prevents ID collision with subsequent /fc invocations per RESEARCH.md Issue 15 and C7. |

### Interfaces / Data Structures

No interfaces or data structures applicable. F849 is a planning feature producing markdown DRAFT files only.

**Sub-feature DRAFT minimum required sections** (for implementer reference):

`feature-850.md` (NTR Ubiquitous Language + Value Objects, Type: engine):
- `## Type: engine`
- Philosophy: "Phase 24: NTR Bounded Context — Domain Layer (Value Objects)"
- Goal: Implement NTR Ubiquitous Language definition (glossary), NtrRoute (R0-R6), NtrPhase (0-7), NtrParameters, Susceptibility VOs in `src/Era.Core/NTR/Domain/ValueObjects/`
- Background references `pm/reference/ntr-ddd-input.md` — distinguish Validated (FavLevel, AffairPermission, CorruptionState) vs Grounded (NtrRoute, NtrPhase, NtrParameters) candidates
- Dependencies: Predecessor → F849
- AC: debt-cleanup (not_matches TODO|FIXME|HACK)
- Deliverables: `NtrRoute.cs`, `NtrPhase.cs`, `NtrParameters.cs`, `Susceptibility.cs` + ubiquitous language glossary (markdown or inline doc comments)
- Architecture Task Coverage: Architecture Task 1, Architecture Task 3, Architecture Task 4, Architecture Task 5

`feature-851.md` (NtrProgression Aggregate + Domain Events, Type: engine):
- `## Type: engine`
- Philosophy: "Phase 24: NTR Bounded Context — Domain Layer (Aggregate)"
- Goal: Implement NtrProgression as AggregateRoot + NtrPhaseAdvanced, NtrRouteChanged, NtrExposureIncreased, NtrCorrupted domain event classes in `src/Era.Core/NTR/Domain/`
- Background references `ntr-ddd-input.md` NtrProgression Aggregate section (Validated)
- Dependencies: Predecessor → F850
- AC: debt-cleanup (not_matches TODO|FIXME|HACK)
- Architecture Task Coverage: Architecture Task 2, Architecture Task 6

`feature-852.md` (INtrCalculator Domain Service, Type: engine):
- `## Type: engine`
- Philosophy: "Phase 24: NTR Bounded Context — Domain Layer (Services)"
- Goal: Design and implement `INtrCalculator` interface in `src/Era.Core/NTR/Domain/Services/` — CanAdvance(), CanChangeRoute() at minimum
- Background references `ntr-ddd-input.md` — NtrParameters Grounded status (required parameters for calculator TBD during design)
- Dependencies: Predecessor → F851
- AC: debt-cleanup (not_matches TODO|FIXME|HACK)
- Architecture Task Coverage: Architecture Task 7

`feature-853.md` (Application Services + ACL, Type: engine):
- `## Type: engine`
- Philosophy: "Phase 24: NTR Bounded Context — Application + Infrastructure Layer"
- Goal: Implement Application Commands (AdvanceNtrPhaseCommand, ChangeNtrRouteCommand), Queries (GetNtrStatusQuery, GetSusceptibilityQuery), EventHandlers, NtrProgressionRepository, AntiCorruptionLayer in `src/Era.Core/NTR/Application/` and `src/Era.Core/NTR/Infrastructure/`
- Background references `ntr-ddd-input.md` — OB-03 INtrQuery routes to Phase 24 definition (F829)
- Dependencies: Predecessor → F852
- AC: debt-cleanup (not_matches TODO|FIXME|HACK)
- Architecture Task Coverage: Architecture Task 8, Architecture Task 9

`feature-854.md` (Post-Phase Review Phase 24, Type: infra):
- `## Type: infra`
- Standard Post-Phase Review template; verifies architecture doc Phase 24 section integrity against F850–F853 deliverables
- Dependencies: Predecessor → F853
- Architecture Task Coverage: Architecture Task 10

`feature-855.md` (Phase 25 Planning, Type: research):
- `## Type: research`
- Standard Phase Planning template; decomposes Phase 25 NTR/Visitor/Location systems into sub-features
- Background notes Phase 25 granularity warning from architecture doc (`phase-20-27-game-systems.md:793-796`): ~24,000 lines, 38+ files, 3 recommended sub-groups
- Dependencies: Predecessor → F854
- Architecture Task Coverage: Architecture Task 11

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| (none — AC#3 self-referential concern resolved by switching to sub-feature file grep) | — | — |

---

<!-- fc-phase-5-completed -->
## Tasks

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. N ACs : 1 Task allowed. No orphan Tasks. -->

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 3, 4, 5, 8, 9, 10 | Create `pm/features/feature-850.md` — Domain Layer (NTR Ubiquitous Language + Value Objects: NtrRoute R0-R6, NtrPhase 0-7, NtrParameters, Susceptibility); Type: engine; Philosophy: "Phase 24: NTR Bounded Context — Domain Layer (Value Objects)"; include debt-cleanup AC (not_matches TODO\|FIXME\|HACK); reference ntr-ddd-input.md Validated vs Grounded status; Predecessor → F849; no NTR Mark content | | [x] |
| 2 | 1, 3, 4, 5, 6, 8, 9, 10 | Create `pm/features/feature-851.md` — Domain Layer (NtrProgression Aggregate + Domain Events: NtrPhaseAdvanced, NtrRouteChanged, NtrExposureIncreased, NtrCorrupted); Type: engine; Philosophy: "Phase 24: NTR Bounded Context — Domain Layer (Aggregate)"; include debt-cleanup AC; reference ntr-ddd-input.md; Predecessor → F850; no NTR Mark content | | [x] |
| 3 | 1, 3, 4, 5, 6, 8, 9, 10 | Create `pm/features/feature-852.md` — Domain Layer (INtrCalculator Domain Service: CanAdvance(), CanChangeRoute() interface); Type: engine; Philosophy: "Phase 24: NTR Bounded Context — Domain Layer (Services)"; include debt-cleanup AC; reference ntr-ddd-input.md Grounded status for NtrParameters; Predecessor → F851; no NTR Mark content | | [x] |
| 4 | 1, 3, 4, 5, 6, 8, 9, 10 | Create `pm/features/feature-853.md` — Application + Infrastructure Layer (Commands, Queries, EventHandlers, NtrProgressionRepository, AntiCorruptionLayer); Type: engine; Philosophy: "Phase 24: NTR Bounded Context — Application + Infrastructure Layer"; include debt-cleanup AC; reference ntr-ddd-input.md OB-03 INtrQuery (F829); Predecessor → F852; no NTR Mark content | | [x] |
| 5 | 1, 3, 8, 11, 13 | Create `pm/features/feature-854.md` — Post-Phase Review Phase 24; Type: infra; standard post-phase review template; verifies architecture doc Phase 24 integrity against F850–F853 deliverables; Predecessor → F853; no NTR Mark content | | [x] |
| 6 | 1, 3, 8, 12, 13 | Create `pm/features/feature-855.md` — Phase 25 Planning; Type: research; standard phase planning template; note Phase 25 granularity warning (~24,000 lines, 38+ files, 3 recommended sub-groups from phase-20-27-game-systems.md:793-796); Predecessor → F854; no NTR Mark content | | [x] |
| 7 | 2, 7 | Register F850–F855 in `pm/index-features.md` under new `### Phase 24: NTR Bounded Context` section with [DRAFT] status and correct Depends On values; increment Next Feature number from 850 to 856 | | [x] |

### Task Tags

| Tag | Meaning |
|:---:|---------|
| [I] | Investigation required — involves design decisions or unknowns that need exploration before implementation |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | `pm/features/feature-849.md` Technical Design § Interfaces / Data Structures (feature-850.md spec), `pm/reference/ntr-ddd-input.md` | `pm/features/feature-850.md` [DRAFT] |
| 2 | implementer | sonnet | `pm/features/feature-849.md` Technical Design § Interfaces / Data Structures (feature-851.md spec), `pm/features/feature-850.md` | `pm/features/feature-851.md` [DRAFT] |
| 3 | implementer | sonnet | `pm/features/feature-849.md` Technical Design § Interfaces / Data Structures (feature-852.md spec), `pm/features/feature-851.md` | `pm/features/feature-852.md` [DRAFT] |
| 4 | implementer | sonnet | `pm/features/feature-849.md` Technical Design § Interfaces / Data Structures (feature-853.md spec), `pm/features/feature-852.md` | `pm/features/feature-853.md` [DRAFT] |
| 5 | implementer | sonnet | `pm/features/feature-849.md` Technical Design § Interfaces / Data Structures (feature-854.md spec) | `pm/features/feature-854.md` [DRAFT] |
| 6 | implementer | sonnet | `pm/features/feature-849.md` Technical Design § Interfaces / Data Structures (feature-855.md spec), `docs/architecture/migration/phase-20-27-game-systems.md:793-796` | `pm/features/feature-855.md` [DRAFT] |
| 7 | implementer | sonnet | `pm/index-features.md`, F850–F855 titles and statuses | Updated `pm/index-features.md` with Phase 24 section, Next Feature = 856 |

### Pre-conditions

- F848 is [DONE] (Post-Phase Review Phase 23 complete)
- `pm/reference/ntr-ddd-input.md` exists and is readable
- `docs/architecture/migration/phase-20-27-game-systems.md` exists (Phase 24 section at lines 567–790)
- Current Next Feature number in `pm/index-features.md` is 850

### Execution Order

Steps must execute sequentially in Phase order. Each sub-feature DRAFT is a prerequisite input for the successor's Predecessor declaration.

1. **Phase 1**: Create `feature-850.md` with all required sections per Technical Design spec
   - Type: engine
   - Philosophy sub-section: "Phase 24: NTR Bounded Context — Domain Layer (Value Objects)"
   - Background references `pm/reference/ntr-ddd-input.md` — distinguish Validated (FavLevel, AffairPermission, CorruptionState) vs Grounded (NtrRoute, NtrPhase, NtrParameters) candidates
   - Dependencies table: `Predecessor | F849 | [PROPOSED] | Phase 24 Planning (parent)`
   - AC Definition Table (DRAFT placeholder): include a debt-cleanup AC row matching `TODO|FIXME|HACK` pattern (`not_matches`)
   - Goal: NtrRoute.cs (R0-R6), NtrPhase.cs (0-7), NtrParameters.cs, Susceptibility.cs, ubiquitous language glossary in `src/Era.Core/NTR/Domain/ValueObjects/`
   - No mention of NTR Mark, MarkIndex, or GET_MARK_LEVEL
   - Architecture Task Coverage: Architecture Task 1, Architecture Task 3, Architecture Task 4, Architecture Task 5

2. **Phase 2**: Create `feature-851.md` with all required sections per Technical Design spec
   - Type: engine
   - Philosophy sub-section: "Phase 24: NTR Bounded Context — Domain Layer (Aggregate)"
   - Background references `ntr-ddd-input.md` NtrProgression Aggregate section (Validated)
   - Dependencies table: `Predecessor | F850 | [DRAFT] | NTR UL + Value Objects (required before Aggregate)`
   - AC Definition Table (DRAFT placeholder): include debt-cleanup AC row
   - Goal: NtrProgression as AggregateRoot, NtrPhaseAdvanced/NtrRouteChanged/NtrExposureIncreased/NtrCorrupted domain events in `src/Era.Core/NTR/Domain/`
   - No mention of NTR Mark, MarkIndex, or GET_MARK_LEVEL
   - Architecture Task Coverage: Architecture Task 2, Architecture Task 6

3. **Phase 3**: Create `feature-852.md` with all required sections per Technical Design spec
   - Type: engine
   - Philosophy sub-section: "Phase 24: NTR Bounded Context — Domain Layer (Services)"
   - Background references `ntr-ddd-input.md` — NtrParameters Grounded status (required parameters for calculator TBD during design)
   - Dependencies table: `Predecessor | F851 | [DRAFT] | NtrProgression Aggregate (INtrCalculator operates on Aggregate)`
   - AC Definition Table (DRAFT placeholder): include debt-cleanup AC row
   - Goal: INtrCalculator interface in `src/Era.Core/NTR/Domain/Services/` — CanAdvance(), CanChangeRoute() at minimum
   - No mention of NTR Mark, MarkIndex, or GET_MARK_LEVEL
   - Architecture Task Coverage: Architecture Task 7

4. **Phase 4**: Create `feature-853.md` with all required sections per Technical Design spec
   - Type: engine
   - Philosophy sub-section: "Phase 24: NTR Bounded Context — Application + Infrastructure Layer"
   - Background references `ntr-ddd-input.md` — OB-03 INtrQuery routes to Phase 24 definition (F829)
   - Dependencies table: `Predecessor | F852 | [DRAFT] | INtrCalculator Domain Service (Application Services invoke Domain Service)`
   - AC Definition Table (DRAFT placeholder): include debt-cleanup AC row
   - Goal: AdvanceNtrPhaseCommand, ChangeNtrRouteCommand, GetNtrStatusQuery, GetSusceptibilityQuery, EventHandlers, NtrProgressionRepository, AntiCorruptionLayer in `src/Era.Core/NTR/Application/` and `src/Era.Core/NTR/Infrastructure/`
   - No mention of NTR Mark, MarkIndex, or GET_MARK_LEVEL
   - Architecture Task Coverage: Architecture Task 8, Architecture Task 9

5. **Phase 5**: Create `feature-854.md` with all required sections per Technical Design spec
   - Type: infra
   - Standard Post-Phase Review template
   - Goal: Verify architecture doc Phase 24 section integrity against F850–F853 deliverables
   - Dependencies table: `Predecessor | F853 | [DRAFT] | Final Phase 24 implementation sub-feature`
   - No mention of NTR Mark, MarkIndex, or GET_MARK_LEVEL
   - Architecture Task Coverage: Architecture Task 10

6. **Phase 6**: Create `feature-855.md` with all required sections per Technical Design spec
   - Type: research
   - Standard Phase Planning template
   - Background must note Phase 25 granularity warning from `phase-20-27-game-systems.md:793-796`: ~24,000 lines, 38+ files, 3 recommended sub-groups
   - Dependencies table: `Predecessor | F854 | [DRAFT] | Post-Phase Review Phase 24 must complete before Phase 25 Planning`
   - No mention of NTR Mark, MarkIndex, or GET_MARK_LEVEL
   - Architecture Task Coverage: Architecture Task 11

7. **Phase 7**: Edit `pm/index-features.md`
   - Add new section `### Phase 24: NTR Bounded Context` with 6 rows (F850–F855), each with Status [DRAFT] and correct Depends On values
   - Increment "Next Feature number" field from 850 to 856
   - Verify the section is placed after the Phase 23 section in logical order

### Success Criteria

- `pm/features/feature-850.md` through `pm/features/feature-855.md` all exist (6 files)
- Each of F850–F853 contains "Phase 24.*NTR Bounded Context" in Philosophy section
- Each of F850–F853 contains a debt-cleanup AC referencing `TODO|FIXME|HACK`
- Each of F850–F853 is Type: engine; F854 is Type: infra; F855 is Type: research
- Each of F850–F853 references `ntr-ddd-input` or Validated/Grounded language in Background
- F851, F852, F853 each have a Predecessor declaration (enforcing DDD layer ordering)
- `pm/index-features.md` contains F850–F855 entries (6 registrations)
- `pm/index-features.md` Next Feature number = 856
- None of F850–F855 contain NTR Mark / MarkIndex / GET_MARK_LEVEL references

### Error Handling

- If `ntr-ddd-input.md` cannot be located: STOP → Report to user (required for Background content of F850–F853)
- If `pm/index-features.md` Next Feature number is not 850: STOP → Report to user (ID collision risk)
- If any sub-feature file already exists: STOP → Report to user (do not overwrite existing work)

---

## Mandatory Handoffs

<!-- CRITICAL: Use deferred-task-protocol.md Option B Guards. Destinations must be specific Feature IDs, not TBD. -->
<!-- Option A: Successor Feature (created by this feature). Option B: Existing Feature. Option C: New Feature (create now). -->
<!-- Validation: Every row must have Destination ID. DRAFT Creation task = Task# that creates the destination. -->
<!-- DRAFT Creation: For Option C, create the feature file immediately when adding to this table. -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-07T01:00 | START | initializer | [REVIEWED] → [WIP] | READY:849:research |
<!-- run-phase-1-completed -->
| 2026-03-07T01:02 | INVESTIGATE | orchestrator | Artifact confirmation: ntr-ddd-input.md, phase-20-27-game-systems.md, index Next=850 | All confirmed |
<!-- run-phase-2-completed -->
| 2026-03-07T01:05 | IMPLEMENT | orchestrator | Tasks 1-7: Created F850-F855 DRAFTs + index registration | 6 files + index updated, Next=856 |
<!-- run-phase-4-completed -->
| 2026-03-07T01:08 | VERIFY | ac-tester | 13/13 ACs PASS | All PASS, no debug needed |
<!-- run-phase-7-completed -->
| 2026-03-07T01:12 | REVIEW | feature-reviewer | Quality review (post) | READY |
<!-- run-phase-8-completed -->
| 2026-03-07T01:15 | DEVIATION | ac-static-verifier | AC#5,#10 false-FAIL (exit 1) | Tool pipe-escaping issue, manual grep confirms 4/4 PASS each |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase2-Review iter1: AC Definition Table AC#3 | AC#3 self-referential — changed to grep sub-feature files for Architecture Task markers
- [fix] Phase2-Review iter1: Goal Coverage / AC Definition Table | Added AC#11 (F854 Type: infra) and AC#12 (F855 Type: research) for Goal Item 6 coverage
- [fix] Phase2-Review iter1: AC Definition Table AC#6 | Strengthened to verify specific predecessor chain (F851→F850, F852→F851, F853→F852) with gte 3
- [fix] Phase2-Review iter2: Tasks table AC# columns | AC#3 moved from Task 7 to Tasks 1-6 (sub-feature files contain Architecture Task markers, not index-features.md)
- [fix] Phase2-Review iter3: AC Definition Table / Philosophy Derivation | Added AC#13 for transition feature predecessor chain (F854→F853, F855→F854) completing pipeline continuity coverage
- [fix] Phase3-Maintainability iter4: F850 spec / Task 1 | Added Susceptibility.cs Value Object to F850 scope (architecture doc directory tree line 651, dependency of GetSusceptibilityQuery in F853)
- [fix] Phase2-Review iter5: AC#3 Details | Added distinctness note — implementer must use exactly one "Architecture Task N" marker per assigned task

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F848](feature-848.md) - Post-Phase Review Phase 23
- [Related: F827](feature-827.md) - Phase 23 Planning (parent)
- [Related: F847](feature-847.md) - Phase 23 NTR Kojo Reference Analysis
- [Related: F829](feature-829.md) - Phase 22 Deferred Obligations
- [Related: F783](feature-783.md) - Phase 21 Planning
- [Related: F814](feature-814.md) - Phase 22 Planning
- [Related: F406](feature-406.md) - NTR Mark Integration (Phase 25 scope, excluded from Phase 24)
- [Successor: F850](feature-850.md) - NTR Ubiquitous Language + Value Objects (created by this feature)
- [Successor: F851](feature-851.md) - NtrProgression Aggregate + Domain Events (created by this feature)
- [Successor: F852](feature-852.md) - INtrCalculator Domain Service (created by this feature)
- [Successor: F853](feature-853.md) - Application Services + Anti-Corruption Layer (created by this feature)
- [Successor: F854](feature-854.md) - Post-Phase Review Phase 24 (created by this feature)
- [Successor: F855](feature-855.md) - Phase 25 Planning (created by this feature)
