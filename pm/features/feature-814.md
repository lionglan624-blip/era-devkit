# Feature 814: Phase 22 Planning

## Status: [REVIEWED]
<!-- fl-reviewed: 2026-03-04T12:54:56Z -->

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

Pipeline Continuity -- Each phase completion triggers next phase planning with empirical CALL/TRYCALL/CALLFORM analysis to derive correct inter-feature dependency graphs and domain-scoped triage of carry-forward obligations, ensuring obligation routing respects subsystem boundaries rather than sequential pipeline order. Phase 22 (State Systems) is the SSOT for Clothing, Pregnancy, Stain, Room, Weather, Sleep, Menstrual, and Relationship subsystem migration scope.

### Problem (Current Issue)

Phase 22 decomposition requires two distinct scoping activities that the current pipeline conflates into a single planning step. First, the 14 ERB files (~9,264 lines) must be decomposed into sub-features with inter-feature dependencies derived from empirical CALL/TRYCALL/CALLFORM analysis, because Phase 21 (F783) used file-prefix grouping only (`docs/architecture/migration/full-csharp-architecture.md:97`), resulting in all sub-features having only F783 as Predecessor while actual call-chain dependencies (F803->F801, F805->F803/F804, etc.) were missing. The mandatory "Sub-Feature Dependency Analysis" procedure (`docs/architecture/migration/full-csharp-architecture.md:95-108`) was created to prevent this recurrence. Second, 35 deferred obligations from F813 were routed to F814 as the next sequential planning feature, but approximately 28-30 of these are Phase 21 Counter System refactoring debt with no natural home in Phase 22 State Systems scope, because the pipeline progression mechanism routes all unresolved obligations to the next feature regardless of domain scope. Additionally, Phase 22 has hidden cross-subsystem coupling: `PREGNACY_S.ERB:426` calls `子供気温耐性取得` defined in `天候.ERB:822`, creating a Pregnancy->Weather dependency that pure file-prefix grouping would miss.

### Goal (What to Achieve)

Decompose Phase 22 into 7 implementation sub-features (F819-F825, per Technical Design allocation) plus 2 transition features (Post-Phase Review Phase 22 + Phase 23 Planning) with correct inter-feature Predecessor dependencies derived from CALL/TRYCALL/CALLFORM analysis, and triage all 35 deferred obligations into Phase 22 scope (13 items: #7, #20-22, #24-30, #32-33), Counter Redux carry-forward (20 items: #2-6, #8-19, #31, #34-35), user decision pending (#23), or N+4 re-deferral (#1) as documented in Technical Design.

**Deferred Obligations from F813**

The following 35 obligations were transferred from F813 (Post-Phase Review Phase 21) and must be addressed during F814 planning or implementation:

| # | Issue | Origin |
|:-:|-------|--------|
| 1 | N+4 --unit deprecation NOT_FEASIBLE re-deferral | F782->F783->F813 |
| 2 | IShrinkageSystem runtime implementation | F803 |
| 3 | IEngineVariables GetTime/SetTime behavioral override | F806 |
| 4 | WcCounterMessageSex constructor complexity reduction (16 deps) | F806 |
| 5 | CFlag/Cflag naming normalization | F807 |
| 6 | WC_VA_FITTING caller documentation | F806 |
| 7 | IComHandler DI registration strategy | F811 |
| 8 | WcCounterMessage constructor bloat (12 params) | F807 |
| 9 | WcCounterMessageTease behavioral test coverage | F807 |
| 10 | NOITEM photography bug (ERB original bug) | F806 |
| 11 | KOJO 3-param overload verification | F806 |
| 12 | WcCounterMessageSex duplicate constant names | F806 |
| 13 | Dispatch() dual offender convention unification | F806 |
| 14 | IWcCounterMessageTease interface extraction | F807 |
| 15 | Character ID constant consolidation | F807 |
| 16 | F807 AC#34 local function enforcement gap | F807 |
| 17 | ICharacterStringVariables VariableStore implementation | F803 |
| 18 | EXP_UP logic duplication | F803 |
| 19 | ICounterSourceHandler ISP violation | F803 |
| 20 | CFlagIndex typed struct | F803 |
| 21 | EquipIndex typed struct | F803 |
| 22 | IComableUtilities/ICounterUtilities TimeProgress/IsAirMaster/GetTargetNum consolidation | F810 |
| 23 | NtrReversalSource/NtrAgreeSource REGRESSION fix if found | F813 Task 14 |
| 24 | NullCounterUtilities concrete implementation | F801/F804 |
| 25 | NullWcSexHaraService concrete implementation | F811 |
| 26 | NullNtrUtilityService concrete implementation | F811 |
| 27 | NullTrainingCheckService concrete implementation | F811 |
| 28 | NullKnickersSystem concrete implementation | F811 |
| 29 | NullEjaculationProcessor concrete implementation | F811 |
| 30 | NullKojoMessageService concrete implementation | F811 |
| 31 | RotorOut IWcCounterMessageItem migration consideration | F813 Task 13 |
| 32 | ERB file boundary != domain boundary /fc verification step | F808 |
| 33 | NullComHandler concrete implementation | F811 |
| 34 | IEngineVariables GetTime/SetTime NuGet version bump | F806 |
| 35 | WcCounterMessageNtr class-level split | F813 |

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why must F814 perform decomposition with dependency analysis? | F783 produced all sub-features with only F783 as Predecessor, missing actual inter-feature call-chain dependencies | `docs/architecture/migration/full-csharp-architecture.md:97` |
| 2 | Why were dependencies missed in F783? | No CALL/TRYCALL/CALLFORM cross-file analysis was performed; decomposition used file-prefix grouping only | `docs/architecture/migration/full-csharp-architecture.md:99-108` |
| 3 | Why does Phase 22 have hidden cross-subsystem coupling despite appearing modular? | PREGNACY_S.ERB calls `子供気温耐性取得` defined in 天候.ERB, and calls external functions in 体設定.ERB and 多生児パッチ.ERB | `PREGNACY_S.ERB:426`, `天候.ERB:822` |
| 4 | Why are 35 deferred obligations complicating decomposition scope? | F813 routed all unresolved Phase 21 obligations to F814 as the next sequential planning feature, regardless of whether they belong to Phase 22 domain | `feature-813.md:653-689` |
| 5 | Why (Root)? | Architecture decomposition requires empirical call-graph analysis per mandatory procedure, and the pipeline progression mechanism routes obligations by sequence (not domain), creating scope-mismatch between Counter System debt and State Systems planning | `docs/architecture/migration/full-csharp-architecture.md:95-108`, `feature-814.md:39-77` |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Phase 22 not yet decomposed into sub-features with correct dependencies | File-prefix grouping misses cross-file CALL dependencies; obligation routing ignores domain scope boundaries |
| Where | `feature-814.md` (DRAFT with no ACs/Tasks) | `docs/architecture/migration/full-csharp-architecture.md:95-108` (mandatory procedure) and pipeline obligation routing |
| Fix | Create sub-features with guessed dependencies | Perform empirical CALL/TRYCALL/CALLFORM analysis and triage obligations by domain scope (Phase 22 vs Counter debt) |

---

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F783 | [DONE] | Phase 21 Planning -- pattern precedent and Mandatory Handoff origin |
| F813 | [DONE] | Post-Phase Review Phase 21 -- source of 35 deferred obligations |
| F647 | [DONE] | Phase 20 Planning -- earlier pattern precedent |
| F782 | [DONE] | Post-Phase Review Phase 20 -- N+4 obligation origin chain |
| F797 | [DONE] | IVariableStore migration |
| F811 | [DONE] | SOURCE Entry System -- deferred IKnickersSystem to Phase 22 |
| F801 | [DONE] | Main Counter Core -- NullCounterUtilities deferred |
| F803 | [DONE] | Main Counter Source -- IShrinkageSystem etc. deferred |
| F804 | [DONE] | Counter Utilities Extended -- NullCounterUtilities partial origin |
| F806 | [DONE] | WC Counter Message SEX -- multiple deferrals |
| F807 | [DONE] | WC Counter Message TEASE -- multiple deferrals |
| F808 | [DONE] | WC Counter Message ITEM+NTR -- ERB boundary lesson |
| F810 | [DONE] | Comable/Counter Utilities -- consolidation deferred |
| F815 | [DONE] | Golden Test Design |
| F816 | [DONE] | StubVariableStore |
| F818 | [DONE] | ac-static-verifier |

---

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Architecture spec exists | FEASIBLE | `docs/architecture/migration/phase-20-27-game-systems.md:250-383` defines Phase 22 scope |
| Pattern precedent available | FEASIBLE | F783/F647 provide exact planning template |
| ERB files accessible for CALL analysis | FEASIBLE | All 14 files readable in `C:\Era\game\ERB\` |
| Existing Era.Core interfaces provide foundation | FEASIBLE | IClothingSystem (11-12 methods), IKnickersSystem, IPregnancySettings, IWeatherSettings, IStainLoader exist |
| Scope decomposable per granularity rules | FEASIBLE | 9,264 lines across 8 natural subsystems fits exactly 7 sub-features (F819-F825 per Technical Design allocation) |
| Deferred obligations triageable | FEASIBLE | 13 are Phase 22 scope (#7, #20-22, #24-30, #32-33); 20 are Counter Redux carry-forward; 1 user decision (#23); 1 N+4 re-deferral (#1) |
| Dependencies satisfied | FEASIBLE | F783 [DONE], F813 [DONE] |
| Mandatory procedure documented | FEASIBLE | Sub-Feature Dependency Analysis at `docs/architecture/migration/full-csharp-architecture.md:95-108` |

**Verdict**: FEASIBLE

---

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Phase 22 sub-features | HIGH | All Phase 22 implementation sub-features depend on this decomposition for scope, dependencies, and obligation assignment |
| index-features.md | MEDIUM | Phase 22 section must be added with all sub-feature registrations |
| Deferred obligation tracking | MEDIUM | 35 obligations must be triaged; ~28-30 Counter debt items need separate destination features or carry-forward tracking |
| Era.Core interfaces | LOW | Existing interfaces (IClothingSystem, IKnickersSystem, etc.) will be extended in sub-features, not modified by this planning feature |
| Phase 23 Planning | LOW | Transition feature created as output but not yet executed |

---

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Phase 22 must run alone (no concurrent phases) | `docs/strategy/design-reference.md:537` | All sub-features sequential; CharacterState exclusive writes |
| ACs 5-12, Tasks 3-7 per sub-feature | `docs/architecture/migration/full-csharp-architecture.md:124-127` | Limits sub-feature granularity |
| 1 subsystem per Feature | `docs/architecture/migration/full-csharp-architecture.md:127` | Independent subsystems only per sub-feature |
| Sub-Feature Dependency Analysis mandatory | `docs/architecture/migration/full-csharp-architecture.md:95-108` | Must perform 4-step CALL analysis before DRAFT creation |
| Philosophy inheritance: "Phase 22: State Systems" | `docs/architecture/migration/phase-20-27-game-systems.md:373` | All sub-features must inherit this philosophy |
| Zero-debt AC, equivalence tests, debt resolution mandatory | `docs/architecture/migration/phase-20-27-game-systems.md:374-377` | Sub-feature requirements for each implementation feature |
| CP-2 Step 2b in each sub-feature (F819-F825), Step 2c at Phase Review (F826) | `docs/architecture/migration/full-csharp-architecture.md:249-251`; `docs/architecture/migration/phase-20-27-game-systems.md:299,354-367` | E2E checkpoint ownership per SSOT: each sub-feature owns its subsystem-scoped Step 2b; F825's Step 2b covers DI integration / cross-subsystem wiring |
| Roslynator investigation must be assigned | `docs/architecture/migration/phase-20-27-game-systems.md:300` | Must be placed in a sub-feature |
| Existing ClothingSystem.cs needs stub completion not creation | `ClothingSystem.cs` (628 lines, Phase 3 stubs) | Extension workflow, not greenfield |
| IKnickersSystem full implementation deferred to Phase 22 | `IKnickersSystem.cs:5` | Explicit Phase 22 obligation from F811 |
| PREGNACY_S.ERB has external call dependencies | `PREGNACY_S.ERB:376,424-429` | Interface stubs needed for 体設定.ERB, 多生児パッチ.ERB functions |

---

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Deferred obligation triage creates scope inflation | HIGH | MEDIUM | Strict 3-bucket triage: Phase 22 (~5-7), Counter Redux (~25), cross-cutting (~3-5) |
| PREGNACY_S.ERB external dependencies not yet migrated | MEDIUM | HIGH | Stub interfaces for cross-phase deps (体設定.ERB, 多生児パッチ.ERB) |
| NtrReversalSource/NtrAgreeSource REGRESSION unaddressed | MEDIUM | MEDIUM | Route obligation #23 to separate fix feature |
| Existing ClothingSystem.cs Phase 3 stubs require careful extension | MEDIUM | MEDIUM | Document Phase 3 stub inventory; verify IClothingSystem backward compatibility |
| Cross-subsystem call (PREGNACY_S to 天候) creates unexpected dep | LOW | HIGH | Weather must be predecessor of Pregnancy in dependency graph |
| IClothingSystem extension may break existing consumers | LOW | HIGH | Use interface inheritance or default methods |
| N+4 deprecation chain continues indefinitely | HIGH | LOW | Re-defer with explicit note and review threshold |
| ROOM_SMELL NTR dependency creates cross-phase concern | MEDIUM | LOW | Internal NTR functions SET smell; no external NTR CALL required |
| Phase 22 smaller scope may lead to under-decomposition | LOW | MEDIUM | Follow granularity rules strictly (ACs 5-12, Tasks 3-7) |

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Philosophy "Phase 22: State Systems" in all sub-features | `docs/architecture/migration/phase-20-27-game-systems.md:373` | Grep verification of philosophy string in each sub-feature DRAFT |
| C2 | Debt resolution tasks in each sub-feature | `docs/architecture/migration/phase-20-27-game-systems.md:374` | Grep for debt resolution task rows in sub-feature files |
| C3 | Equivalence test tasks in each sub-feature | `docs/architecture/migration/phase-20-27-game-systems.md:375` | Grep for equivalence test task rows in sub-feature files |
| C4 | Zero-debt AC in each sub-feature | `docs/architecture/migration/phase-20-27-game-systems.md:376` | Each sub-feature must have zero-debt AC |
| C5 | Inter-feature Predecessors from CALL analysis | `docs/architecture/migration/full-csharp-architecture.md:99-108` | Verify non-F814 Predecessors exist where CALL dependencies found |
| C6 | Transition features created (infra + research) | `docs/architecture/migration/full-csharp-architecture.md:132-139` | Verify Post-Phase Review (infra) and Phase 23 Planning (research) types |
| C7 | CP-2 Step 2c assigned to Post-Phase Review | `docs/architecture/migration/phase-20-27-game-systems.md:299,354-367` | Verify E2E checkpoint ownership in transition feature |
| C8 | Roslynator investigation assigned | `docs/architecture/migration/phase-20-27-game-systems.md:300` | Verify Roslynator task assigned to a sub-feature |
| C9 | IKnickersSystem full impl assigned | `docs/architecture/migration/phase-20-27-game-systems.md:285` | Verify in Clothing sub-feature |
| C10 | All 35 deferred obligations triaged | `feature-814.md:39-77` | Each obligation assigned to Phase 22 sub-feature, separate feature, or carry-forward |
| C11 | All 14 ERB files assigned to sub-features | Phase 22 scope | Each ERB file in at least one DRAFT |
| C12 | index-features.md Phase 22 section added | `docs/architecture/migration/full-csharp-architecture.md:106` | Bold formatting for non-[DONE] deps per convention |
| C13 | ClothingSystem.cs stub completion assigned | `ClothingSystem.cs` Phase 3 stubs | Phase 3 stubs explicit task in Clothing sub-feature |
| C14 | CP-2 E2E AC in each sub-feature | `docs/architecture/migration/phase-20-27-game-systems.md:377` | Each sub-feature must contain a CP-2 E2E AC |

### Constraint Details

**C1: Philosophy Inheritance**
- **Source**: `docs/architecture/migration/phase-20-27-game-systems.md:373` mandates "Phase 22: State Systems" philosophy
- **Verification**: Grep each sub-feature DRAFT for philosophy string
- **AC Impact**: ac-designer must include philosophy verification AC

**C2: Debt Resolution Tasks**
- **Source**: `docs/architecture/migration/phase-20-27-game-systems.md:374` mandates debt resolution tasks in each sub-feature
- **Verification**: Grep each sub-feature DRAFT (F819-F825) for TODO/FIXME/HACK removal task
- **AC Impact**: AC#8 verifies debt resolution task presence across all implementation sub-features
- **Format Note**: AC#8 grep pattern `Remove.*TODO.*FIXME.*HACK` intentionally enforces exact canonical stub phrasing (see Technical Design DRAFT content structure). Sub-features MUST use the exact stub text format; this is a deliberate format constraint, not incidental

**C3: Equivalence Test Tasks**
- **Source**: `docs/architecture/migration/phase-20-27-game-systems.md:375` mandates equivalence test tasks in each sub-feature
- **Verification**: Grep each sub-feature DRAFT (F819-F825) for equivalence test task
- **AC Impact**: AC#18 verifies equivalence test task presence across all implementation sub-features

**C4: Zero-Debt AC**
- **Source**: `docs/architecture/migration/phase-20-27-game-systems.md:376` mandates zero-debt AC in each sub-feature
- **Verification**: Grep each sub-feature DRAFT (F819-F825) for zero-debt AC
- **AC Impact**: AC#19 verifies zero-debt AC presence across all implementation sub-features

**C5: CALL-Derived Predecessors**
- **Source**: `docs/architecture/migration/full-csharp-architecture.md:95-108` mandatory 4-step procedure
- **Verification**: Compare sub-feature Dependencies tables against CALL analysis results
- **AC Impact**: ac-designer must verify at least one sub-feature has non-F814 Predecessor (e.g., Weather->Pregnancy dependency)

**C6: Transition Features Created**
- **Source**: `docs/architecture/migration/full-csharp-architecture.md:132-139` requires Post-Phase Review (infra) and Phase 23 Planning (research) transition features
- **Verification**: Glob existence check for feature-826.md and feature-827.md; Grep for correct Type fields
- **AC Impact**: AC#3 verifies file existence and correct Type assignments

**C7: CP-2 Step 2c in Post-Phase Review**
- **Source**: `docs/architecture/migration/phase-20-27-game-systems.md:299,354-367` assigns E2E checkpoint Step 2c to Post-Phase Review
- **Verification**: Grep feature-826.md for "CP-2 Step 2c" or "E2E checkpoint"
- **AC Impact**: AC#6 verifies checkpoint assignment (CP-2 Step 2c) and obligations #1/#32 assignment in the Post-Phase Review transition feature (compound AC shared with C10). Scope distinction: AC#6 covers the C10 subset for obligations #1/#32 routed to F826 only; AC#9 covers overall 35-obligation triage documentation completeness; AC#10 covers specific sub-feature assignments

**C8: Roslynator Investigation Assigned**
- **Source**: `docs/architecture/migration/phase-20-27-game-systems.md:300` requires Roslynator investigation in a sub-feature
- **Verification**: Grep feature-820.md for "Roslynator" (assigned per Technical Design Key Decision)
- **AC Impact**: AC#10 verifies Roslynator assignment in the designated sub-feature

**C9: IKnickersSystem Full Implementation Assigned**
- **Source**: `docs/architecture/migration/phase-20-27-game-systems.md:285` and F811 deferral to Phase 22
- **Verification**: Grep feature-819.md for "IKnickersSystem" (Clothing Core sub-feature)
- **AC Impact**: AC#10 verifies IKnickersSystem assignment in the Clothing Core sub-feature

**C10: Obligation Triage**
- **Source**: 35 obligations in `feature-814.md:39-77` transferred from F813
- **Verification**: Each obligation has documented destination (Phase 22 sub-feature ID, separate feature ID, or carry-forward note)
- **AC Impact**: ac-designer must include obligation triage completeness AC

**C11: All 14 ERB Files Assigned**
- **Source**: Phase 22 scope includes 14 ERB files (~9,264 lines) across 8 subsystems
- **Verification**: Grep each sub-feature DRAFT for each of the 14 ERB filenames; every filename must appear in at least one DRAFT
- **AC Impact**: AC#4 verifies all 14 ERB filenames are assigned (compound AC with 14 individual checks)

**C12: index-features.md Phase 22 Section**
- **Source**: `docs/architecture/migration/full-csharp-architecture.md:106` requires index registration with bold formatting for non-[DONE] deps
- **Verification**: Grep index-features.md for "### Phase 22: State Systems" section header and "Next Feature number" updated to 828
- **AC Impact**: AC#2 verifies section header; AC#11 verifies Next Feature number increment

**C13: ClothingSystem Stub Completion**
- **Source**: `ClothingSystem.cs` has 628 lines with Phase 3 stubs; `IKnickersSystem.cs:5` has Phase 22 deferral comment
- **Verification**: Grep Clothing sub-feature for stub completion task
- **AC Impact**: ac-designer must account for extension workflow (not greenfield creation)

**C14: CP-2 E2E AC in Each Sub-Feature**
- **Source**: `docs/architecture/migration/phase-20-27-game-systems.md:377` lists "AC: CP-2 E2E" as mandatory Sub-Feature Requirement
- **Verification**: Grep each sub-feature DRAFT (F819-F825) for CP-2 E2E AC. The CP-2 E2E entry must appear in the AC Definition Table (not as a Task only). DRAFT template places it as an AC row (`| N+1 | CP-2 E2E checkpoint: [Subsystem] integration verified`)
- **Scope Distinction**: F819-F824 each contain a subsystem-scoped CP-2 E2E AC verifying their individual subsystem integration. F825's CP-2 E2E covers DI integration / cross-subsystem wiring as its subsystem-scoped Step 2b. Each sub-feature (F819-F825) independently owns its own Step 2b E2E per SSOT: `Step 2b — Phase 22 各sub-feature 完了時: 当該系統の E2E 追加（服装->State変化、妊娠->日送り 等）/ Step 2a の全 E2E が退行なし` (`full-csharp-architecture.md:249-251`)
- **AC Impact**: AC#20 scoped to F819-F825 (count_equals 7); each sub-feature has its own subsystem-scoped CP-2 E2E per SSOT

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning -- pattern precedent and Mandatory Handoff origin |
| Predecessor | F813 | [DONE] | Post-Phase Review Phase 21 -- source of 35 deferred obligations |
| Related | F647 | [DONE] | Phase 20 Planning -- earlier pattern precedent |
| Related | F782 | [DONE] | Post-Phase Review Phase 20 -- N+4 obligation origin chain |
| Related | F811 | [DONE] | SOURCE Entry System -- deferred IKnickersSystem to Phase 22 |
| Successor | F819 | [DRAFT] | Clothing Core (created by this feature) |
| Successor | F820 | [DRAFT] | Clothing Extended (created by this feature) |
| Successor | F821 | [DRAFT] | Weather System (created by this feature) |
| Successor | F822 | [DRAFT] | Pregnancy System (created by this feature) |
| Successor | F823 | [DRAFT] | Room & Stain (created by this feature) |
| Successor | F824 | [DRAFT] | Sleep & Menstrual (created by this feature) |
| Successor | F825 | [DRAFT] | Relationships & DI Integration (created by this feature) |
| Successor | F826 | [DRAFT] | Post-Phase Review Phase 22 (created by this feature) |
| Successor | F827 | [DRAFT] | Phase 23 Planning (created by this feature) |

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
| Predecessor | F540 | [DONE] | Era.Core setup required |
| Successor | F567 | [BLOCKED] | F567 depends on this feature |
| Related | F100 | [DONE] | Reference implementation |
-->

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Each phase completion triggers next phase planning" | Must create transition features (Post-Phase Review Phase 22 + Phase 23 Planning) with CP-2 Step 2c checkpoint | AC#3, AC#6 (CP-2 Step 2c portion) |
| "empirical CALL/TRYCALL/CALLFORM analysis to derive correct inter-feature dependency graphs" | Sub-features must have Predecessor dependencies derived from CALL analysis, not only F814 | AC#5, AC#13, AC#14 (pre-known edges; Task 1 [I]-tag covers dynamic results — see Review Notes [resolved-applied] Phase2-Review iter4) |
| "Phase 22 (State Systems) is the SSOT for Clothing, Pregnancy, Stain, Room, Weather, Sleep, Menstrual, and Relationship subsystem migration scope" | All 14 ERB files and all listed subsystems must be covered by sub-features | AC#1, AC#4 |
| "Pipeline Continuity" | Philosophy inherited in all sub-features; quality requirements (debt, equivalence, zero-debt) in each | AC#7, AC#8, AC#18, AC#19, AC#20 |
| "domain-scoped triage of carry-forward obligations, ensuring obligation routing respects subsystem boundaries" | Deferred obligations must be triaged by domain into Phase 22 scope, Counter Redux carry-forward, user decision, and N+4 re-deferral buckets before sub-feature scope is fixed | AC#9, AC#15 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Implementation sub-feature DRAFT files created (7 per Technical Design) | file | Glob(pm/features/feature-819.md) + Glob(pm/features/feature-820.md) + Glob(pm/features/feature-821.md) + Glob(pm/features/feature-822.md) + Glob(pm/features/feature-823.md) + Glob(pm/features/feature-824.md) + Glob(pm/features/feature-825.md) | count_equals | 7 | [ ] |
| 2 | Phase 22 section header exists in index-features.md with sub-feature entries | file | Grep(pm/index-features.md, pattern="### Phase 22: State Systems") + Grep(pm/index-features.md, pattern="\| F819 |\| F820 |\| F821 |\| F822 |\| F823 |\| F824 |\| F825 |\| F826 |\| F827 ") | matches | "### Phase 22: State Systems" AND gte 9 table row matches for sub-feature IDs (compound: both checks must pass) | [ ] |
| 3 | Transition features created with correct types (infra + research) | file | Glob(pm/features/feature-826.md) + Glob(pm/features/feature-827.md) + Grep(pm/features/feature-826.md, pattern="Type: infra") + Grep(pm/features/feature-827.md, pattern="Type: research") | matches | 2 files exist AND "Type: infra" AND "Type: research" | [ ] |
| 4 | All 14 Phase 22 ERB files assigned to sub-feature DRAFTs | file | 14 individual Grep(pm/features/feature-{819..825}.md, pattern=FILENAME) for each of: CLOTHES.ERB, CLOTHE_EFFECT.ERB, CLOTHES_SYSTEM.ERB, CLOTHES_Cosplay.ERB, 天候.ERB, PREGNACY_S.ERB, PREGNACY_S_CHILD_MOVEMENT.ERB, PREGNACY_S_EVENT.ERB, PREGNACY_S_EVENT0.ERB, ROOM_SMELL.ERB, STAIN.ERB, 睡眠深度.ERB, 生理機能追加パッチ.ERB, 続柄.ERB; each must return files_with_matches >= 1 | count_equals | 14 | [ ] |
| 5 | CALL-derived Predecessor dependencies exist (not only F814) | file | Grep(pm/features/feature-822.md, pattern="Predecessor.*F821") | matches | "Predecessor.*F821" | [ ] |
| 6 | CP-2 Step 2c + obligations #1/#32 assigned to Post-Phase Review transition feature (C7, C10) | file | Grep(pm/features/feature-826.md, pattern="CP-2 Step 2c") + Grep(pm/features/feature-826.md, pattern="obligation.*#1[^0-9]") + Grep(pm/features/feature-826.md, pattern="obligation.*#32[^0-9]") | matches | "CP-2 Step 2c" AND "obligation.*#1[^0-9]" AND "obligation.*#32[^0-9]" | [ ] |
| 7 | Philosophy "Phase 22: State Systems" in all implementation sub-features (C1) | file | Grep(pm/features/feature-{819..825}.md, pattern="Phase 22: State Systems") | count_equals | 7 | [ ] |
| 8 | Debt resolution task in all implementation sub-features (C2) | file | Grep(pm/features/feature-{819..825}.md, pattern="Remove.*TODO.*FIXME.*HACK") | count_equals | 7 | [ ] |
| 9 | All 35 deferred obligations triaged with documented destinations (C10) | file | Grep(pm/features/feature-814.md, pattern="## Obligation Triage") + Grep(pm/features/feature-814.md, pattern="obligation #1:\|obligation #23:\|obligation #35:") + Grep(pm/features/feature-814.md, pattern="obligation #24:") + Grep(pm/features/feature-814.md, pattern="obligation #8:") + Grep(pm/features/feature-814.md, pattern="obligation #7:") | matches | "## Obligation Triage" AND "obligation #1:\|obligation #23:\|obligation #35:" AND "obligation #24:" AND "obligation #8:" AND "obligation #7:" | [ ] |
| 10 | Specific assignments verified: IKnickersSystem (C9), Roslynator (C8), ClothingSystem stubs (C13), CFlagIndex (#20), EquipIndex (#21), NullKnickersSystem (#28 in F819), IComableUtilities consolidation (#22 in F825), NullComHandler (#33 in F825) | file | Grep(pm/features/feature-819.md, pattern="IKnickersSystem") + Grep(pm/features/feature-820.md, pattern="Roslynator") + Grep(pm/features/feature-819.md, pattern="ClothingSystem.*stub") + Grep(pm/features/feature-819.md, pattern="CFlagIndex") + Grep(pm/features/feature-819.md, pattern="EquipIndex") + Grep(pm/features/feature-819.md, pattern="NullKnickersSystem") + Grep(pm/features/feature-825.md, pattern="IComableUtilities") + Grep(pm/features/feature-825.md, pattern="NullComHandler") | matches | "IKnickersSystem" in F819 AND "Roslynator" in F820 AND "ClothingSystem.*stub" in F819 AND "CFlagIndex" in F819 AND "EquipIndex" in F819 AND "NullKnickersSystem" in F819 AND "IComableUtilities" in F825 AND "NullComHandler" in F825 | [ ] |
| 11 | Next Feature number incremented past all allocated IDs (C12) | file | Grep(pm/index-features.md, pattern="Next Feature number.*828") | matches | "Next Feature number.*828" | [ ] |
| 12 | F825 CP-2 E2E covers DI integration / cross-subsystem wiring | file | Grep(pm/features/feature-825.md, pattern="CP-2.*E2E.*DI|DI.*integration.*E2E") | matches | "CP-2.*E2E.*DI\|DI.*integration.*E2E" | [ ] |
| 13 | Additional CALL-derived Predecessor dependency: F820 has F819 as Predecessor (C5) | file | Grep(pm/features/feature-820.md, pattern="Predecessor.*F819") | matches | "Predecessor.*F819" | [ ] |
| 14 | F825 (DI Integration) has all Phase 22 sub-feature Predecessors (C5) | file | Grep(pm/features/feature-825.md, pattern="Predecessor.*F81[9]\|Predecessor.*F82[0-4]") | count_equals | 6 | [ ] |
| 15 | Counter Redux carry-forward obligations appended to F813 Mandatory Handoffs with representative obligation numbers + GetNWithVisitor transfer | file | Grep(pm/features/feature-813.md, pattern="Counter Redux") + Grep(pm/features/feature-813.md, pattern="obligation.*#2[^0-9]\|obligation.*#19[^0-9]\|obligation.*#35[^0-9]") + Grep(pm/features/feature-813.md, pattern="obligation.*#10[^0-9]") + Grep(pm/features/feature-813.md, pattern="GetNWithVisitor") | matches | "Counter Redux" AND "obligation.*#2[^0-9]\|obligation.*#19[^0-9]\|obligation.*#35[^0-9]" AND "obligation.*#10[^0-9]" AND "GetNWithVisitor" | [ ] |
| 16 | F826 (Post-Phase Review) has all Phase 22 implementation sub-features as Predecessors | file | Grep(pm/features/feature-826.md, pattern="Predecessor.*F81[9]\|Predecessor.*F82[0-5]") | count_equals | 7 | [ ] |
| 17 | F827 (Phase 23 Planning) has F826 as Predecessor and contains Phase 23 reference | file | Grep(pm/features/feature-827.md, pattern="Predecessor.*F826") + Grep(pm/features/feature-827.md, pattern="Phase 23") | matches | "Predecessor.*F826" AND "Phase 23" | [ ] |
| 18 | Equivalence test task in all implementation sub-features (C3) | file | Grep(pm/features/feature-{819..825}.md, pattern="equivalence.*test") | count_equals | 7 | [ ] |
| 19 | Zero-debt AC in all implementation sub-features (C4) | file | Grep(pm/features/feature-{819..825}.md, pattern="zero.*debt") | count_equals | 7 | [ ] |
| 20 | CP-2 E2E AC in all implementation sub-features (C14) | file | Grep(pm/features/feature-{819..825}.md, pattern="CP-2 E2E checkpoint:") | count_equals | 7 | [ ] |
| 21 | Task 1 CALL analysis output file exists | file | Glob(_out/tmp/phase22-callgraph.txt) | matches | file exists | [ ] |
| 22 | Null*Service obligations (#24-27, #29-30) assigned to F825 DI Integration | file | Grep(pm/features/feature-825.md, pattern="NullCounterUtilities") + Grep(pm/features/feature-825.md, pattern="NullWcSexHaraService") + Grep(pm/features/feature-825.md, pattern="NullNtrUtilityService") + Grep(pm/features/feature-825.md, pattern="NullTrainingCheckService") + Grep(pm/features/feature-825.md, pattern="NullEjaculationProcessor") + Grep(pm/features/feature-825.md, pattern="NullKojoMessageService") | matches | all 6 Null*Service names present in feature-825.md | [ ] |
| 23 | Phase22 metrics file exists | file | Glob(_out/tmp/phase22-metrics.txt) | matches | _out/tmp/phase22-metrics.txt | [ ] |
| 24 | Phase22 callgraph contains cross-subsystem dependency | file | Grep(_out/tmp/phase22-callgraph.txt, pattern="PREGNACY_S\|子供気温耐性取得") | matches | PREGNACY_S or 子供気温耐性取得 | [ ] |

### AC Details

**AC#1: Implementation sub-feature DRAFT files created (7 per Technical Design)**
- **Test**: Glob existence check for each of the 7 allocated implementation sub-feature files: `feature-819.md` through `feature-825.md`
- **Expected**: Exactly 7 implementation sub-feature DRAFT files exist on disk (F819-F825)
- **Derivation**: Technical Design allocated 7 implementation sub-features (F819-F825). `count_equals 7` ensures all allocated files are created. Individual Glob checks per file avoid matching existing features (F810-F818) that a range pattern like `feature-8[12]*.md` would include
- **Rationale**: Satisfies Goal Item 1. Uses explicit file list to prevent vacuous pre-pass against existing features. Content quality verified separately by AC#7/AC#8/AC#18/AC#19/AC#20
- **Note**: The 7 files are: F819 (Clothing Core), F820 (Clothing Extended), F821 (Weather), F822 (Pregnancy), F823 (Room & Stain), F824 (Sleep & Menstrual), F825 (Relationships & DI)

**AC#2: Phase 22 section header exists in index-features.md with sub-feature entries**
- **Test**: (1) `Grep("### Phase 22: State Systems", "pm/index-features.md")` -- verify section header exists. (2) `Grep("| F819 || F820 || F821 || F822 || F823 || F824 || F825 || F826 || F827 ", "pm/index-features.md")` -- verify each ID appears as a table row entry (pipe-prefixed), gte 9 matches
- **Expected**: Section header "### Phase 22: State Systems" exists AND at least 9 table row matches for the 9 sub-feature IDs (F819-F827) in the Active Features table
- **Derivation**: C12 requires index registration for each allocated feature. The second Grep uses table-row-anchored patterns (`| F819 ` with pipe prefix) to ensure each ID appears as an Active Features table entry, not merely as a cross-reference or mention anywhere in the file. This prevents false positives where IDs appear in obligation lists, review notes, or other non-table contexts. gte 9 accounts for the 9 allocated features (F819-F827: 7 implementation + 2 transition)
- **Rationale**: Table-row-anchored pattern is stricter than bare ID matching (`F819|F820|...`), eliminating false positives from IDs that appear in non-table positions. Compound format (both checks must pass) ensures both the section and its entries exist

**AC#4: All 14 Phase 22 ERB files assigned to sub-feature DRAFTs**
- **Test**: For each of the 14 ERB files (CLOTHES.ERB, CLOTHE_EFFECT.ERB, CLOTHES_SYSTEM.ERB, CLOTHES_Cosplay.ERB, 天候.ERB, PREGNACY_S.ERB, PREGNACY_S_CHILD_MOVEMENT.ERB, PREGNACY_S_EVENT.ERB, PREGNACY_S_EVENT0.ERB, ROOM_SMELL.ERB, STAIN.ERB, 睡眠深度.ERB, 生理機能追加パッチ.ERB, 続柄.ERB), Grep the sub-feature DRAFT files (feature-{819..825}.md) to confirm each filename appears in at least one DRAFT
- **Expected**: All 14 ERB filenames appear in at least one sub-feature DRAFT (compound: 14 checks, all must pass)
- **Derivation**: 14 ERB files identified during investigation (~9,264 total lines). Per C11, every ERB file must be assigned to ensure no scope gaps
- **Rationale**: Satisfies C11 (All 14 ERB files assigned to sub-features). Prevents scope gaps where an ERB file is omitted from all sub-feature DRAFTs
- **Note**: Compound AC -- all 14 individual Grep checks must pass (AND conjunction)

**AC#5: CALL-derived Predecessor dependencies exist (not only F814)**
- **Test**: `Grep("Predecessor.*F821", "pm/features/feature-822.md")` -- verify Pregnancy sub-feature (F822) has Weather sub-feature (F821) as Predecessor
- **Expected**: Pattern `Predecessor.*F821` matches in feature-822.md, confirming the CALL-derived dependency is recorded
- **Derivation**: C5 requires inter-feature Predecessors from CALL analysis. The known cross-subsystem dependency (PREGNACY_S.ERB:426 calls `子供気温耐性取得` in 天候.ERB:822) requires Weather (F821) to be a Predecessor of Pregnancy (F822). Scoping to feature-822.md specifically avoids matching existing features that already contain "Predecessor" entries
- **Rationale**: Satisfies C5. Targets the specific known CALL dependency rather than broadly grepping all of `pm/features/`. Additional inter-sub-feature Predecessors (F820->F819, F825->all) are structurally required by Technical Design but this AC verifies the empirically-derived one

**AC#7: Philosophy "Phase 22: State Systems" in all implementation sub-features (C1)**
- **Test**: `Grep("Phase 22: State Systems", feature-{819..825}.md)` -- count files_with_matches == 7
- **Expected**: All 7 implementation sub-features contain "Phase 22: State Systems" philosophy string
- **Derivation**: AC#1 fixes the sub-feature count at exactly 7 (F819-F825). C1 mandates philosophy inheritance "in each sub-feature" per `docs/architecture/migration/phase-20-27-game-systems.md:373`. `count_equals 7` enforces the "mandatory in each" constraint without allowing silent gaps
- **Rationale**: Satisfies C1. Exact philosophy string ensures inheritance, not loose pattern matching. Scoped to feature-{819..825}.md to prevent matching existing features

**AC#8: Debt resolution task in all implementation sub-features (C2)**
- **Test**: `Grep("Remove.*TODO.*FIXME.*HACK", feature-{819..825}.md)` -- count files_with_matches == 7
- **Expected**: All 7 implementation sub-features contain a debt resolution task row
- **Derivation**: AC#1 fixes the sub-feature count at exactly 7 (F819-F825). C2 mandates debt resolution tasks per `docs/architecture/migration/phase-20-27-game-systems.md:374`. Pattern `Remove.*TODO.*FIXME.*HACK` targets the task stub text format, avoiding false positives from raw debt markers in code
- **Rationale**: Satisfies C2. Scoped to feature-{819..825}.md to prevent matching existing features

**AC#9: All 35 deferred obligations triaged with documented destinations (C10)**
- **Test**: (1) `Grep("## Obligation Triage", "pm/features/feature-814.md")` -- verify section header exists. (2) `Grep("obligation #1:|obligation #23:|obligation #35:", "pm/features/feature-814.md")` -- verify representative obligations with colon-terminated format. (3) `Grep(pm/features/feature-814.md, pattern="obligation #24:")` -- verify Phase-22-scope bucket (DI/Null services). (4) `Grep(pm/features/feature-814.md, pattern="obligation #8:")` -- verify Counter Redux bucket. (5) `Grep(pm/features/feature-814.md, pattern="obligation #7:")` -- verify Phase-22-scope bucket (IComHandler DI registration)
- **Expected**: Section header exists AND obligations #1:, #23:, #35: are present AND "obligation #24:" matches (Phase-22-scope DI/Null) AND "obligation #8:" matches (Counter Redux) AND "obligation #7:" matches (Phase-22-scope IComHandler)
- **Derivation**: 35 obligations transferred from F813 (lines 47-83 of feature-814.md). C10 requires each obligation to have a documented destination. Row-counting via `^\| [0-9]+` is infeasible because the file contains multiple numbered tables whose rows all match. 6 representative samples (#1, #7, #8, #23, #24, #35) cover all 4 triage buckets. Colon-terminated format (`obligation #N:`) prevents false-positive matching against AC table rows or Review Notes entries that contain obligation number references in non-triage context (self-referential pattern avoidance)
- **Rationale**: Satisfies C10 (obligation triage completeness). Representative sampling trades exhaustive count for false-negative avoidance. Full 35-obligation completeness is enforced by Task 5's Execution Order contract

**AC#10: Specific assignments verified (C8, C9, C13, obligations #20/#21/#22/#28/#33)**
- **Test**: (1) `Grep("IKnickersSystem", "pm/features/feature-819.md")` -- C9. (2) `Grep("Roslynator", "pm/features/feature-820.md")` -- C8. (3) `Grep("ClothingSystem.*stub", "pm/features/feature-819.md")` -- C13. (4) `Grep("CFlagIndex", "pm/features/feature-819.md")` -- obligation #20. (5) `Grep("EquipIndex", "pm/features/feature-819.md")` -- obligation #21. (6) `Grep("NullKnickersSystem", "pm/features/feature-819.md")` -- obligation #28. (7) `Grep("IComableUtilities", "pm/features/feature-825.md")` -- obligation #22 (interface consolidation). (8) `Grep("NullComHandler", "pm/features/feature-825.md")` -- obligation #33 (domain routing)
- **Expected**: All eight patterns match: IKnickersSystem/ClothingSystem.*stub/CFlagIndex/EquipIndex/NullKnickersSystem in F819 AND Roslynator in F820 AND IComableUtilities/NullComHandler in F825
- **Derivation**: C8 Roslynator in F820, C9 IKnickersSystem in F819, C13 ClothingSystem stubs in F819. Obligations #20/#21 (typed structs) in F819. #28 (NullKnickersSystem) in F819. #22 (IComableUtilities/ICounterUtilities consolidation) in F825. #33 (NullComHandler) in F825. Each Grep scoped to specific allocated file
- **Rationale**: Groups eight specific-assignment constraints into one AC. Verifies domain routing across F819 (clothing), F820 (tooling), F825 (DI/consolidation)
- **Note**: All eight individual checks must pass (AND conjunction)

**AC#11: Next Feature number incremented past all allocated IDs (C12)**
- **Test**: `Grep("Next Feature number.*828", "pm/index-features.md")` -- verify the number has been updated to 828
- **Expected**: Pattern "Next Feature number.*828" matches in index-features.md. Technical Design allocated F819-F827 (9 features), so 819 + 9 = 828
- **Derivation**: Technical Design determined 9 allocated features (F819-F827: 7 implementation + 2 transition). Next Feature number must be incremented from 819 to 828. The value 828 is deterministic from Technical Design; `matches` matcher is used because the Grep pattern already encodes the exact expected value (not a range)
- **Rationale**: Satisfies C12 (index-features.md updated). Prevents ID collision on subsequent `/fc` invocations

**AC#12: F825 CP-2 E2E covers DI integration / cross-subsystem wiring**
- **Test**: `Grep("CP-2.*E2E.*DI|DI.*integration.*E2E", "pm/features/feature-825.md")` -- verify F825 contains a CP-2 E2E checkpoint specifically for DI integration
- **Expected**: Pattern "CP-2.*E2E.*DI|DI.*integration.*E2E" matches in feature-825.md, confirming DI integration E2E is present
- **Derivation**: Per SSOT (full-csharp-architecture.md:249-251), F825's subsystem-scoped Step 2b E2E covers DI integration / cross-subsystem wiring (its own subsystem deliverable). AC#20 verifies all 7 sub-features have CP-2 E2E; AC#12 verifies that F825's CP-2 E2E is specifically for DI integration (not just a generic checkpoint). AC#6 verifies Step 2c in F826
- **Rationale**: Ensures F825's CP-2 E2E entry is substantively correct (DI integration scope), not merely present. Complements AC#6 (Step 2c in F826) and AC#20 (CP-2 E2E in all 7 sub-features)

**AC#13: Additional CALL-derived Predecessor dependency: F820 has F819 as Predecessor (C5)**
- **Test**: `Grep("Predecessor.*F819", "pm/features/feature-820.md")` -- verify F820 (Clothing Extended) has F819 (Clothing Core) as Predecessor
- **Expected**: Pattern "Predecessor.*F819" matches in feature-820.md
- **Derivation**: Technical Design CALL analysis shows CLOTHES_SYSTEM.ERB and CLOTHES_Cosplay.ERB call functions defined in CLOTHES.ERB, requiring F819 to be a Predecessor of F820. Complements AC#5 (F822→F821 Weather→Pregnancy) to provide broader verification of the CALL-derived dependency graph
- **Rationale**: Satisfies C5 more comprehensively. AC#5 verified the cross-subsystem (Weather→Pregnancy) dependency; AC#13 verifies the intra-subsystem (Clothing Core→Extended) dependency. Together they cover two distinct CALL-analysis-derived edges

**AC#14: F825 (DI Integration) has all Phase 22 sub-feature Predecessors (C5)**
- **Test**: `Grep("Predecessor.*F81[9]|Predecessor.*F82[0-4]", "pm/features/feature-825.md", output_mode="count")` -- count matching Predecessor rows (not files)
- **Expected**: Exactly 6 matches (line count), confirming F825 lists all sub-feature predecessors. Each Predecessor must appear on a separate table row in the Dependencies section (one row per Predecessor), consistent with feature-template.md format
- **Derivation**: Technical Design specifies F825 depends on all F819-F824 (DI Integration requires all subsystem interfaces to be defined). `count_equals 6` enforces the complete fan-in constraint. AC#5 (cross-subsystem), AC#13 (intra-subsystem), and AC#14 (integration fan-in) together cover three distinct dependency graph patterns. Uses `output_mode="count"` to count matching lines within a single file (default `files_with_matches` would return 0 or 1)
- **Rationale**: Satisfies C5 for the integration layer. F825 is the convergence point of all Phase 22 sub-features; `count_equals 6` ensures no Predecessor is omitted

**AC#15: Counter Redux carry-forward obligations appended to F813 Mandatory Handoffs with representative obligation numbers**
- **Test**: (1) `Grep("Counter Redux", "pm/features/feature-813.md")` -- verify section label exists. (2) `Grep("obligation.*#2[^0-9]|obligation.*#19[^0-9]|obligation.*#35[^0-9]", "pm/features/feature-813.md")` -- verify representative obligations present. (3) `Grep(pm/features/feature-813.md, pattern="obligation.*#10[^0-9]")` -- verify mid-batch obligation present. (4) `Grep(pm/features/feature-813.md, pattern="GetNWithVisitor")` -- verify separately-tracked obligation transfer
- **Expected**: "Counter Redux" AND representative obligations (#2, #19, #35) match AND "obligation.*#10[^0-9]" matches AND "GetNWithVisitor" matches in feature-813.md
- **Derivation**: Task 5 appends 20 Counter Redux obligations (#2-6, #8-19, #31, #34-35) to F813 Mandatory Handoffs. 4 representative samples (#2 first, #10 mid-batch, #19 last-sequential, #35 last) ensures actual obligation content was transferred. GetNWithVisitor (INtrUtilityService full impl) is a separately-tracked carry-forward outside the 35-item list (resolved-applied in Review Notes), verified independently
- **Rationale**: Ensures carry-forward obligations are actually transferred with content, not just triaged in feature-814.md. GetNWithVisitor check covers the one obligation outside the 35-item list that routes to F813

**AC#16: F826 (Post-Phase Review) has all Phase 22 implementation sub-features as Predecessors**
- **Test**: `Grep("Predecessor.*F81[9]|Predecessor.*F82[0-5]", "pm/features/feature-826.md", output_mode="count")` -- count matching Predecessor rows (not files)
- **Expected**: Exactly 7 matches (line count: F819-F825), confirming Post-Phase Review waits for all implementation sub-features
- **Derivation**: Technical Design dependency table (line 492) specifies F826 Predecessors include F814 + all F819-F825. Without this AC, F826 could be created with only F814 as Predecessor, allowing Post-Phase Review to run before implementation completes — violating the sequential Phase 22 constraint. Uses `output_mode="count"` to count matching lines within a single file (default `files_with_matches` would return 0 or 1)
- **Rationale**: Ensures the Post-Phase Review transition feature correctly depends on all implementation sub-features. Complements AC#14 (F825 fan-in) with F826 fan-in

**AC#17: F827 (Phase 23 Planning) has F826 as Predecessor and contains Phase 23 reference**
- **Test**: (1) `Grep("Predecessor.*F826", "pm/features/feature-827.md")` -- verify F826 as Predecessor. (2) `Grep("Phase 23", "pm/features/feature-827.md")` -- verify Phase 23 content reference
- **Expected**: Both patterns match in feature-827.md
- **Derivation**: Technical Design (line 493) specifies F827 Predecessor = F826 (planning triggers after review). Phase 23 content reference ensures the planning feature is substantive (not empty stub). Complements AC#3 (existence + type) with content verification
- **Rationale**: Ensures Pipeline Continuity philosophy: "each phase completion triggers next phase planning" requires both correct sequencing (Predecessor) and substantive content (Phase 23 reference)

**AC#18: Equivalence test task in all implementation sub-features (C3)**
- **Test**: `Grep("equivalence.*test", feature-{819..825}.md)` -- count files_with_matches == 7
- **Expected**: All 7 implementation sub-features contain an equivalence test task row
- **Derivation**: AC#1 fixes the sub-feature count at exactly 7 (F819-F825). C3 mandates equivalence test tasks per `docs/architecture/migration/phase-20-27-game-systems.md:375`
- **Rationale**: Satisfies C3. Scoped to feature-{819..825}.md

**AC#19: Zero-debt AC in all implementation sub-features (C4)**
- **Test**: `Grep("zero.*debt", feature-{819..825}.md)` -- count files_with_matches == 7
- **Expected**: All 7 implementation sub-features contain a zero-debt AC
- **Derivation**: AC#1 fixes the sub-feature count at exactly 7 (F819-F825). C4 mandates zero-debt AC per `docs/architecture/migration/phase-20-27-game-systems.md:376`
- **Rationale**: Satisfies C4. Scoped to feature-{819..825}.md

**AC#20: CP-2 E2E AC in all implementation sub-features (C14)**
- **Test**: `Grep("CP-2 E2E checkpoint:", feature-{819..825}.md)` -- count files_with_matches == 7
- **Expected**: All 7 implementation sub-features contain a subsystem-scoped CP-2 E2E AC. F825's CP-2 E2E covers DI integration / cross-subsystem wiring as its own subsystem-scoped Step 2b
- **Derivation**: AC#1 fixes the sub-feature count at exactly 7 (F819-F825). C14 mandates CP-2 E2E AC per `docs/architecture/migration/phase-20-27-game-systems.md:377`. Per SSOT (full-csharp-architecture.md:249-251), each sub-feature owns its own Step 2b E2E
- **Rationale**: Satisfies C14. Scoped to feature-{819..825}.md

**AC#21: Task 1 CALL analysis output file exists**
- **Test**: `Glob("_out/tmp/phase22-callgraph.txt")` -- verify file exists after Task 1 execution
- **Expected**: File `_out/tmp/phase22-callgraph.txt` exists on disk
- **Derivation**: Task 1 performs CALL/TRYCALL/CALLFORM/JUMP analysis and documents results in `_out/tmp/phase22-callgraph.txt` per Technical Design Phase 1. This is a deterministic file existence check (the file is always created when Task 1 completes successfully), so no [I] tag is needed
- **Rationale**: Verifies Task 1 completion with a concrete, deterministic output. The file existence confirms analysis was performed and documented. Content verification is procedurally enforced via the Task 3 STOP gate

**AC#23: Phase22 metrics file exists**
- **Test**: `Glob("_out/tmp/phase22-metrics.txt")` -- verify metrics output was created
- **Expected**: File exists (Glob returns match)
- **Derivation**: Task 2 produces metrics analysis output. Mirrors AC#21 pattern (callgraph.txt existence). Without this AC, Task 2 could fail silently.
- **Rationale**: Symmetric coverage with AC#21 (Task 1 output verification)
- **Note**: Task 2's [I] tag reflects AC#4 uncertainty only; AC#23 (file existence) is deterministic and should be verified regardless of [I] tag (consistent with AC#21 precedent where [I] was removed for the same reason)

**AC#24: Phase22 callgraph contains cross-subsystem dependency**
- **Test**: `Grep("PREGNACY_S|子供気温耐性取得", "_out/tmp/phase22-callgraph.txt")` -- verify known cross-subsystem dependency is documented
- **Expected**: Pattern matches (file contains the cross-subsystem call reference)
- **Derivation**: Philosophy requires "empirical CALL/TRYCALL/CALLFORM analysis". AC#21 verifies the file exists but not its content. This AC verifies the known critical cross-subsystem dependency (PREGNACY_S.ERB→天候.ERB via 子供気温耐性取得) is actually documented in the analysis output, confirming empirical analysis was performed and the file is not empty/trivial
- **Rationale**: Complements AC#21 (existence) with content verification; provides evidence that Task 1's analysis actually captured the cross-subsystem dependency documented in the Problem section

**AC#22: Null*Service obligations (#24-27, #29-30) assigned to F825 DI Integration**
- **Test**: Six Grep checks on `pm/features/feature-825.md`: (1) `Grep("NullCounterUtilities")` -- obligation #24. (2) `Grep("NullWcSexHaraService")` -- obligation #25. (3) `Grep("NullNtrUtilityService")` -- obligation #26. (4) `Grep("NullTrainingCheckService")` -- obligation #27. (5) `Grep("NullEjaculationProcessor")` -- obligation #29. (6) `Grep("NullKojoMessageService")` -- obligation #30
- **Expected**: All 6 Null*Service names present in feature-825.md (AND conjunction)
- **Derivation**: Obligations #24-27, #29-30 are assigned to F825 per Obligation Triage Plan but no AC previously verified they appear in feature-825.md. AC#10 covers NullComHandler (#33) and NullKnickersSystem (#28 in F819), leaving the 6 Null*Service obligations (#24-27, #29-30) unverified. Each name is sufficiently unique to identify the obligation without false positives
- **Rationale**: Satisfies C10 for obligations #24-27, #29-30 specifically. Complements AC#10 (which covers #28 and #33) to complete Null*Service domain routing verification across F819 and F825
- **Note**: All 6 individual Grep checks must pass (AND conjunction). NullCounterUtilities (#24) and the 5 service names (#25-27, #29-30) share the F825 consolidation destination per Key Decision "Null*Service obligations routing"

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Create 7 implementation sub-features (F819-F825) | AC#1, AC#2, AC#4, AC#7, AC#8, AC#10, AC#11, AC#12, AC#18, AC#19, AC#20, AC#23 |
| 2 | Create 2 transition features (Post-Phase Review Phase 22 + Phase 23 Planning) | AC#3, AC#6, AC#16, AC#17 |
| 3 | Correct inter-feature Predecessor dependencies from CALL/TRYCALL/CALLFORM analysis | AC#5, AC#13, AC#14, AC#21, AC#24 |
| 4 | Triage all 35 deferred obligations | AC#6, AC#9, AC#10, AC#15, AC#22 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

The implementation follows the F783 planning pattern adapted for Phase 22's 8-subsystem scope: **CALL Graph Analysis → Volume-Informed Grouping → Obligation Triage → DRAFT File Creation → Index/Registry Updates**.

Phase 22 has 14 ERB files across 8 natural subsystems totaling ~9,264 lines. The mandatory Sub-Feature Dependency Analysis procedure (`docs/architecture/migration/full-csharp-architecture.md:95-108`) requires empirical CALL/TRYCALL/CALLFORM analysis before DRAFT creation to derive inter-feature Predecessor dependencies. The known cross-subsystem dependency (PREGNACY_S.ERB:426 calls `子供気温耐性取得` defined in 天候.ERB:822) requires the Weather sub-feature to be a Predecessor of the Pregnancy sub-feature.

**Feature ID Allocation (F819-F827, 9 total)**

Starting from Next Feature number 819 (current value in index-features.md):

| Feature | Subsystem | Source Files | Approx Lines | Type |
|---------|-----------|--------------|:------------:|------|
| F819 | Clothing Core | CLOTHES.ERB, CLOTHE_EFFECT.ERB | ~2,284 | erb |
| F820 | Clothing Extended | CLOTHES_SYSTEM.ERB, CLOTHES_Cosplay.ERB | ~1,381 | erb |
| F821 | Weather System | 天候.ERB | ~839 | erb |
| F822 | Pregnancy System | PREGNACY_S.ERB, PREGNACY_S_CHILD_MOVEMENT.ERB, PREGNACY_S_EVENT.ERB, PREGNACY_S_EVENT0.ERB | ~2,534 | erb |
| F823 | Room & Stain | ROOM_SMELL.ERB, STAIN.ERB | ~1,440 | erb |
| F824 | Sleep & Menstrual | 睡眠深度.ERB, 生理機能追加パッチ.ERB | ~407 | erb |
| F825 | Relationships & DI Integration | 続柄.ERB | ~379 | erb |
| F826 | Post-Phase Review Phase 22 | (infra transition) | - | infra |
| F827 | Phase 23 Planning | (research transition) | - | research |

**Next Feature number after allocation: 828**

Note: Clothing is split into two features (F819 Core + F820 Extended) because 3,665 lines with IKnickersSystem full implementation + ClothingSystem.cs stub completion + Roslynator investigation would exceed the 5-12 AC / 3-7 Task granularity limits if combined. F820 (Clothing Extended) absorbs the Roslynator investigation task, and F825 (Relationships & DI Integration) owns CP-2 Step 2b, keeping these infrastructure concerns separate from content migration.

Note: F825's DI Integration scope (Null*Service obligations, IComHandler, consolidation) is supporting infrastructure for Phase 22 completion, not a 9th subsystem per Philosophy scope. The Relationships subsystem (続柄.ERB) is the sole Phase-22-SSOT subsystem in F825; DI Integration enables cross-subsystem wiring required for Phase 22 closure.

**CALL-Derived Predecessor Dependencies**

The mandatory 4-step CALL analysis (Step 1: Grep CALL/TRYCALL/CALLFORM; Step 2: derive caller→callee direction; Step 3: declare B as Predecessor of A when A calls B's functions; Step 4: index-features.md bold formatting) produces this dependency graph:

| Sub-Feature | Predecessor(s) | Dependency Basis |
|-------------|----------------|------------------|
| F819 (Clothing Core) | F814 | No Phase 22 internal calls; ClothingSystem.cs stubs pre-exist |
| F820 (Clothing Extended) | F814, **F819** | CLOTHES_SYSTEM.ERB and CLOTHES_Cosplay.ERB call functions defined in CLOTHES.ERB |
| F821 (Weather System) | F814 | 天候.ERB is a callee only within Phase 22 scope; no Phase 22 internal deps |
| F822 (Pregnancy System) | F814, **F821** | PREGNACY_S.ERB:426 calls `子供気温耐性取得` defined in 天候.ERB:822 (CRITICAL cross-subsystem dep) |
| F823 (Room & Stain) | F814 | ROOM_SMELL.ERB and STAIN.ERB are independent; NTR smell functions are internal SET, no external CALL |
| F824 (Sleep & Menstrual) | F814 | 睡眠深度.ERB and 生理機能追加パッチ.ERB are independent; no Phase 22 internal CALL deps |
| F825 (Relationships & DI) | F814, **F819**, **F820**, **F821**, **F822**, **F823**, **F824** | DI Integration requires all subsystem interfaces to be defined; 続柄.ERB relationships are used by clothing/pregnancy contexts |
| F826 (Post-Phase Review 22) | F814, **F819**, **F820**, **F821**, **F822**, **F823**, **F824**, **F825** | Post-Phase Review runs after all implementation sub-features complete |
| F827 (Phase 23 Planning) | **F826** | Planning only triggers after Post-Phase Review passes |

Note: PREGNACY_S.ERB also references functions from 体設定.ERB and 多生児パッチ.ERB (external to Phase 22). These are not Phase 22 Predecessor relationships; they require interface stubs or cross-phase dependency documentation in F822.

**Obligation Triage Plan (35 obligations from F813)**

Triage into 3 buckets: Phase 22 scope, Counter Redux (carry-forward), cross-cutting fix features.

| # | Obligation | Destination | Rationale |
|:-:|------------|-------------|-----------|
| 1 | N+4 --unit deprecation NOT_FEASIBLE re-deferral | Carry-forward to F826 (Post-Phase Review 22) | Trigger condition not yet met; re-triage at Phase 22 review |
| 2 | IShrinkageSystem runtime implementation | Carry-forward note in F813 (Counter Redux) | Phase 21 Counter scope; not State Systems |
| 3 | IEngineVariables GetTime/SetTime behavioral override | Carry-forward note in F813 (Counter Redux) | Counter/Engine scope; not State Systems |
| 4 | WcCounterMessageSex constructor complexity reduction | Carry-forward note in F813 (Counter Redux) | Counter System scope |
| 5 | CFlag/Cflag naming normalization | Carry-forward note in F813 (Counter Redux) | Cross-cutting; not Phase 22 specific |
| 6 | WC_VA_FITTING caller documentation | Carry-forward note in F813 (Counter Redux) | Counter scope |
| 7 | IComHandler DI registration strategy | F825 (Relationships & DI Integration) | DI integration sub-feature; directly applicable |
| 8 | WcCounterMessage constructor bloat (12 params) | Carry-forward note in F813 (Counter Redux) | Counter scope |
| 9 | WcCounterMessageTease behavioral test coverage | Carry-forward note in F813 (Counter Redux) | Counter scope |
| 10 | NOITEM photography bug (ERB original bug) | Carry-forward note in F813 (Counter Redux) | Counter ERB bug; not Phase 22 |
| 11 | KOJO 3-param overload verification | Carry-forward note in F813 (Counter Redux) | Kojo/Counter scope |
| 12 | WcCounterMessageSex duplicate constant names | Carry-forward note in F813 (Counter Redux) | Counter scope |
| 13 | Dispatch() dual offender convention unification | Carry-forward note in F813 (Counter Redux) | Counter scope |
| 14 | IWcCounterMessageTease interface extraction | Carry-forward note in F813 (Counter Redux) | Counter scope |
| 15 | Character ID constant consolidation | Carry-forward note in F813 (Counter Redux) | Cross-cutting; Counter Redux carry-forward pending dedicated consolidation feature |
| 16 | F807 AC#34 local function enforcement gap | Carry-forward note in F813 (Counter Redux) | Counter scope |
| 17 | ICharacterStringVariables VariableStore implementation | Carry-forward note in F813 (Counter Redux) | Counter/infra scope |
| 18 | EXP_UP logic duplication | Carry-forward note in F813 (Counter Redux) | Counter scope |
| 19 | ICounterSourceHandler ISP violation | Carry-forward note in F813 (Counter Redux) | Counter scope |
| 20 | CFlagIndex typed struct | F819 (Clothing Core) | CFLAG slots are central to ClothingSystem.cs; if CALL analysis reveals broader usage beyond Clothing scope, escalate to user before reassigning to F826 |
| 21 | EquipIndex typed struct | F819 (Clothing Core) | EQUIP slots are core clothing system concern |
| 22 | IComableUtilities/ICounterUtilities TimeProgress/IsAirMaster/GetTargetNum consolidation | F825 (Relationships & DI Integration) | DI/interface consolidation task |
| 23 | NtrReversalSource/NtrAgreeSource REGRESSION fix if found | F813 carry-forward (user decision required before further routing) | Potential regression not yet confirmed; carried forward to F813 pending user investigation decision |
| 24 | NullCounterUtilities concrete implementation | F825 (Relationships & DI Integration) | Null*Service pattern; DI integration scope |
| 25 | NullWcSexHaraService concrete implementation | F825 (Relationships & DI Integration) | Null*Service pattern; DI integration scope |
| 26 | NullNtrUtilityService concrete implementation | F825 (Relationships & DI Integration) | Null*Service pattern; DI integration scope. Note: GetNWithVisitor(int)→int returns default/stub value (0) since NTR_VISITOR.ERB is not in Phase 22's 14 ERB files; full implementation deferred to Counter Redux carry-forward per GetNWithVisitor resolution |
| 27 | NullTrainingCheckService concrete implementation | F825 (Relationships & DI Integration) | Null*Service pattern; DI integration scope |
| 28 | NullKnickersSystem concrete implementation | F819 (Clothing Core) | Clothing system scope; IKnickersSystem full impl |
| 29 | NullEjaculationProcessor concrete implementation | F825 (Relationships & DI Integration) | Null*Service pattern; DI integration scope |
| 30 | NullKojoMessageService concrete implementation | F825 (Relationships & DI Integration) | Null*Service pattern; DI integration scope |
| 31 | RotorOut IWcCounterMessageItem migration consideration | Carry-forward note in F813 (Counter Redux) | Counter Message scope |
| 32 | ERB file boundary != domain boundary /fc verification step | F826 (Post-Phase Review 22) | Review procedure enhancement; applicable at post-phase |
| 33 | NullComHandler concrete implementation | F825 (Relationships & DI Integration) | Null*Service pattern; DI integration scope |
| 34 | IEngineVariables GetTime/SetTime NuGet version bump | Carry-forward note in F813 (Counter Redux) | Engine/Counter scope; not Phase 22 |
| 35 | WcCounterMessageNtr class-level split | Carry-forward note in F813 (Counter Redux) | Counter scope |

**Counter Redux carry-forward summary**: Obligations #2-6, #8-19, #31, #34-35 (20 items) remain Counter System debt with no natural home in Phase 22. These are documented in F813 as carry-forward obligations pending a Counter Redux feature decision by the user.

**User decision pending**: Obligation #23 (NtrReversalSource REGRESSION) has a documented F813 carry-forward destination pending user investigation decision.

**Phase 22 scope obligations**: #7, #20-22, #24-30, #32-33 (13 items) assigned to Phase 22 sub-features as documented above.

**N+4 re-deferral**: Obligation #1 routed to F826 (Post-Phase Review Phase 22) for re-triage.

**Implementation Phases**

**Phase 1: CALL Graph Analysis**
- Grep CALL/TRYCALL/CALLFORM/JUMP across all 14 ERB files in `C:\Era\game\ERB\`
- Map cross-file call directions per sub-feature boundary
- Confirm PREGNACY_S.ERB -> 天候.ERB dependency (PREGNACY_S.ERB:426 calls `子供気温耐性取得`)
- Identify any additional cross-file dependencies within Phase 22 scope
- Document in `_out/tmp/phase22-callgraph.txt`

**Phase 2: Verify Empirical File Metrics**
- Confirm actual line counts via `wc -l` for all 14 Phase 22 ERB files
- Validate grouping decisions against granularity rules (5-12 ACs, 3-7 Tasks per sub-feature)
- Document in `_out/tmp/phase22-metrics.txt`

**Phase 3: Create DRAFT Files (F819-F827)**
- Each implementation DRAFT (F819-F825) includes:
  - Type: `erb`
  - Background inheriting "Phase 22: State Systems" philosophy (C1)
  - Scope Reference listing assigned ERB files and function ranges
  - Task stubs: debt resolution (`Remove TODO/FIXME/HACK comments`) (C2), equivalence tests (`Write equivalence tests against ERB baseline`) (C3)
  - AC stub: zero-debt AC (`No TODO/FIXME/HACK comments remain`) (C4)
  - Inter-feature Predecessor rows derived from CALL analysis (Step 3 of mandatory procedure)
  - Obligation assignments from triage table above
- F819 additionally owns: IKnickersSystem full implementation (C9), ClothingSystem.cs stub completion (C13), NullKnickersSystem (obligation #28), CFlagIndex typed struct (obligation #20), EquipIndex typed struct (obligation #21)
- F820 additionally owns: Roslynator investigation (C8)
- F825 additionally owns: CP-2 E2E for DI integration (its subsystem-scoped Step 2b), all Null*Service obligations (#24-27, #29-30, #33), IComHandler DI registration (#7), consolidation (#22)
- F826 (Post-Phase Review Phase 22): type `infra`; owns CP-2 Step 2c E2E checkpoint (C7), obligation #1 re-triage, obligation #32 (ERB boundary verification)
- F827 (Phase 23 Planning): type `research`

**Phase 4: Obligation Triage Documentation**
- Add "## Obligation Triage" section to feature-814.md documenting all 35 obligations with assigned destinations (satisfies AC#9)
- Counter Redux carry-forward obligations (#2-6, #8-19, #31, #34-35) appended to F813 Mandatory Handoffs

**Phase 5: Index and Registry Updates**
- Add `### Phase 22: State Systems` section to `index-features.md` listing F819-F827
- Update "Next Feature number" from 819 to 828
- Bold-format non-[DONE] Predecessors per convention (`docs/architecture/migration/full-csharp-architecture.md:106`)

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Phase 3 creates F819-F825 (7 implementation DRAFTs). Glob checks for each of the 7 specific files (feature-819.md through feature-825.md). count_equals 7 satisfied |
| 2 | Phase 5 creates `### Phase 22: State Systems` section in index-features.md listing F819-F827. Grep("### Phase 22: State Systems", index-features.md) returns match |
| 3 | Phase 3 creates F826 (type: infra) and F827 (type: research). Glob existence check for both files AND Grep("Type: infra", feature-826.md) AND Grep("Type: research", feature-827.md) all pass |
| 4 | Phase 3 assigns all 14 ERB filenames to DRAFTs per Feature Allocation table. Grep for each of the 14 filenames across sub-feature DRAFTs confirms presence in at least one file |
| 5 | Phase 3 assigns F821 as Predecessor of F822 (Weather -> Pregnancy CALL dependency). Grep("Predecessor.*F821", feature-822.md) returns match. Note: Task 1 feeds CALL data into Task 3; AC#5 verifies the pre-known F822→F821 edge created by Task 3, not Task 1's dynamic call graph output (procedurally enforced via Task 3 STOP gate; see Review Notes [resolved-applied] iter7) |
| 6 | F826 (Post-Phase Review Phase 22) includes CP-2 Step 2c E2E checkpoint task. Grep("CP-2 Step 2c", feature-826.md) returns match |
| 7 | Phase 3 creates DRAFTs with "Phase 22: State Systems" philosophy per C1. Grep scoped to feature-{819..825}.md returns count_equals 7 files_with_matches |
| 8 | Phase 3 creates DRAFTs with debt resolution task (Remove.*TODO.*FIXME.*HACK). Grep scoped to feature-{819..825}.md returns count_equals 7 files_with_matches |
| 9 | Phase 4 adds "## Obligation Triage" section to feature-814.md with all 35 obligation destinations documented. Section header matches AND representative obligations (#1, #7, #8, #23, #24, #35) documented with destinations, covering all 4 triage buckets |
| 10 | F819 assigns IKnickersSystem (C9), ClothingSystem stubs (C13), CFlagIndex (#20), EquipIndex (#21), NullKnickersSystem (#28); F820 assigns Roslynator (C8); F825 assigns IComableUtilities consolidation (#22), NullComHandler (#33). Eight scoped Grep checks all match. matches confirmed |
| 11 | Phase 5 updates "Next Feature number" to 828 (819 + 9 allocated features = 828). Grep("Next Feature number.*828", index-features.md) returns match |
| 12 | F825 (Relationships & DI Integration) includes CP-2 E2E checkpoint for DI integration / cross-subsystem wiring (its subsystem-scoped Step 2b). Grep("CP-2.*E2E.*DI|DI.*integration.*E2E", feature-825.md) returns match |
| 13 | F820 (Clothing Extended) has F819 (Clothing Core) as Predecessor per CALL analysis. Grep("Predecessor.*F819", feature-820.md) returns match |
| 14 | F825 (DI Integration) has all 6 Phase 22 sub-feature Predecessors per Technical Design. Grep for Predecessor F819-F824 in feature-825.md returns count_equals 6 |
| 15 | Phase 4 appends Counter Redux carry-forward obligations to F813 Mandatory Handoffs with representative obligation numbers + GetNWithVisitor transfer. Grep("Counter Redux", feature-813.md) AND representative obligations (#2, #19, #35) match AND GetNWithVisitor match |
| 16 | Phase 3 creates F826 with all Phase 22 implementation sub-feature Predecessors (F819-F825). Grep for Predecessor F819-F825 in feature-826.md returns count_equals 7 |
| 17 | Phase 3 creates F827 with F826 as Predecessor and Phase 23 content reference. Grep("Predecessor.*F826", feature-827.md) AND Grep("Phase 23", feature-827.md) both match |
| 18 | Phase 3 creates DRAFTs with equivalence test task (equivalence.*test). Grep scoped to feature-{819..825}.md returns count_equals 7 files_with_matches |
| 19 | Phase 3 creates DRAFTs with zero-debt AC (zero.*debt). Grep scoped to feature-{819..825}.md returns count_equals 7 files_with_matches |
| 20 | Phase 3 creates DRAFTs with CP-2 E2E AC (CP-2 E2E checkpoint:). Grep scoped to feature-{819..825}.md returns count_equals 7 (each sub-feature owns its own subsystem-scoped Step 2b E2E per SSOT) files_with_matches |
| 21 | Task 1 creates `_out/tmp/phase22-callgraph.txt`. Glob existence check confirms file present after CALL analysis completes |
| 22 | Task 3 creates F825 with all 6 Null*Service names (NullCounterUtilities, NullWcSexHaraService, NullNtrUtilityService, NullTrainingCheckService, NullEjaculationProcessor, NullKojoMessageService). Six Grep checks on feature-825.md all match (AND conjunction) |
| 23 | Task 2 creates `_out/tmp/phase22-metrics.txt`. Glob existence check confirms file present after metrics analysis completes |
| 24 | Task 1 writes PREGNACY_S.ERB→天候.ERB cross-subsystem call into `_out/tmp/phase22-callgraph.txt`. Grep("PREGNACY_S\|子供気温耐性取得", phase22-callgraph.txt) returns match, confirming empirical CALL analysis content |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Clothing split into 2 vs 1 feature | 1 feature (3,665 lines, all clothing ERBs) vs 2 features (Core: CLOTHES.ERB + CLOTHE_EFFECT.ERB; Extended: CLOTHES_SYSTEM.ERB + CLOTHES_Cosplay.ERB) | **2 features (F819+F820)** | 3,665 lines + IKnickersSystem full impl + ClothingSystem.cs 628-line stub completion + Roslynator investigation would exceed 5-12 AC / 3-7 Task granularity. CLOTHES.ERB (1,999 lines) + CLOTHE_EFFECT.ERB (285 lines) form a natural Core unit; CLOTHES_SYSTEM.ERB (964) + CLOTHES_Cosplay.ERB (417) form Extended mechanics. CALL analysis will confirm F820→F819 dependency (clothing mechanics call clothing state) |
| Roslynator investigation placement | F819 (Clothing Core), F820 (Clothing Extended), F825 (Relationships & DI), standalone feature | **F820 (Clothing Extended)** | Roslynator investigation is a dev tooling task orthogonal to game content; placing it in the Extended feature keeps Core focused on IKnickersSystem and ClothingSystem.cs completion. F820 is otherwise the smallest Clothing feature and can absorb the tooling investigation |
| CP-2 Step 2b vs Step 2c placement | 2b in each sub-feature; 2c in Post-Phase Review | **2b in each sub-feature (F819-F825), 2c in F826** | Architecture requirement per SSOT (full-csharp-architecture.md:249-251): Step 2b is subsystem-scoped E2E at each sub-feature completion. Each of F819-F825 adds its own subsystem E2E (Clothing→State change, Weather→weather state, etc.). F825's Step 2b covers DI integration / cross-subsystem wiring. F826 (Post-Phase Review) owns Step 2c per `docs/architecture/migration/phase-20-27-game-systems.md:299,354-367` |
| Null*Service obligations routing | Scatter to each owning sub-feature vs consolidate in DI integration feature | **Consolidate in F825 (Relationships & DI Integration)** | 8 Null*Service implementations (#24-30, #33) share identical implementation pattern (empty implementations of deferred interfaces). Consolidating in F825 eliminates cross-feature coordination; DI integration is the natural point where all service registrations must be resolved |
| Weather System grouping | Combined with Room & Stain vs standalone feature | **Standalone (F821)** | 天候.ERB (839 lines) is a Predecessor of Pregnancy (F822). Creating it as a standalone feature clarifies the dependency chain and avoids making Room & Stain depend on Weather unnecessarily |
| Sleep & Menstrual grouping | Combined with Room & Stain vs standalone pair vs separate features | **Combined as F824 (407 lines total)** | 睡眠深度.ERB (244) + 生理機能追加パッチ.ERB (163) = 407 lines total. Both are independent biological state systems with no CALL relationships to other Phase 22 subsystems. Combining respects the lower volume limit (407 lines fits comfortably in 5-12 ACs / 3-7 Tasks) |
| Obligation #23 (NtrReversalSource REGRESSION) routing | Route to F819/F822/F825 vs separate fix feature vs carry-forward | **Separate fix feature (user decision required)** | Obligation says "REGRESSION fix if found" — the regression has not been confirmed. Routing to an implementation feature may embed an investigation that disrupts its scope. User must decide: create a dedicated regression-investigation feature, or carry-forward pending confirmation |
| CFlagIndex typed struct (#20) routing | F819 (Clothing Core) vs F826 (Post-Phase Review 22) | **F819 (Clothing Core)** | CFLAG slots are central to ClothingSystem.cs; CFlagIndex is most naturally owned by the feature that completes ClothingSystem stubs. If CALL analysis reveals broader usage beyond Clothing scope, escalate to user before reassigning to F826 |

### Interfaces / Data Structures

This is a `research` type feature. No new C# interfaces or data structures are created by F814 itself. The sub-features created as output will implement or extend the following existing interfaces per `docs/architecture/migration/phase-20-27-game-systems.md:257-282`:

**Existing interfaces relevant to Phase 22 sub-features**:
- `IClothingSystem` (Era.Core/Interfaces/IClothingSystem.cs) -- extended by F819
- `IKnickersSystem` (Era.Core/Counter/Source/IKnickersSystem.cs) -- fully implemented by F819
- `IPregnancySettings` -- consumed by F822
- `IWeatherSettings` -- consumed by F821
- `IStainLoader` -- consumed by F823

**New interfaces to be designed during sub-feature implementation** (not designed in this planning feature):
- `IPregnancySystem` -- F822 ownership; requires `Result<PregnancyState> GetState(CharacterId)` and `Result<Unit> AdvanceDay(CharacterId)` per `docs/architecture/migration/phase-20-27-game-systems.md:265-270`
- `IEnvironmentSystem` (or `IWeatherSystem`) -- F821 ownership; `Weather CurrentWeather { get; }` per `docs/architecture/migration/phase-20-27-game-systems.md:272-276`
- `IRoomState` / `IStainManager` -- F823 ownership; shapes TBD by CALL analysis
- `ISleepDepth` / `IMenstrualCycle` -- F824 ownership; shapes TBD by CALL analysis
- `IRelationships` -- F825 ownership; shapes TBD by CALL analysis

**DRAFT content structure for each implementation sub-feature (F819-F825)**:

Each DRAFT file must include the following stub sections to satisfy ACs 7, 8, 10, 18, 19, 20, and C14:
```markdown
### Philosophy (Mid-term Vision)
Phase 22: State Systems -- [subsystem description]

### Tasks (stub)
| Task# | AC# | Description | Tag | Status |
| 1 | - | Remove TODO/FIXME/HACK comments from migrated code | | [ ] |
| 2 | - | Write equivalence tests against ERB baseline | | [ ] |

### AC (stub)
| AC# | Description | ... |
| N | No TODO/FIXME/HACK comments remain in [Subsystem] implementation (zero debt) | ... |
| N+1 | CP-2 E2E checkpoint: [Subsystem] integration verified | ... |
```

**Note**: Task stub text must use exact phrasing shown above. AC#8/AC#18/AC#19/AC#20 grep patterns (`Remove.*TODO.*FIXME.*HACK`, `equivalence.*test`, `zero.*debt`, `CP-2 E2E checkpoint:`) match these exact formats.

### Upstream Issues

<!-- No upstream issues found during Technical Design. The 11 ACs are satisfiable by the proposed approach.
     Obligation #23 (NtrReversalSource REGRESSION) requires user decision -- documented in Key Decisions above,
     not treated as an Upstream Issue since it is a routing decision, not an AC/constraint gap. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 21, 24 | Perform CALL/TRYCALL/CALLFORM/JUMP analysis across all 14 Phase 22 ERB files; confirm PREGNACY_S.ERB→天候.ERB dependency; identify all additional inter-sub-feature call directions; document in `_out/tmp/phase22-callgraph.txt` | | [ ] |
| 2 | 4, 23 | Verify empirical line counts (`wc -l`) for all 14 Phase 22 ERB files; validate grouping decisions against granularity rules (5-12 ACs, 3-7 Tasks); document in `_out/tmp/phase22-metrics.txt`. Note: granularity applies to final sub-feature specs after /fc, not to initial DRAFTs (which have stub ACs/Tasks); Task 2 validates that ERB groupings ALLOW proper granularity, feeding into Task 3's DRAFT creation (AC#4 verification) | [I] | [ ] |
| 3 | 1, 4, 5, 7, 8, 10, 12, 13, 14, 18, 19, 20, 22 | Create 7 implementation sub-feature DRAFT files (F819-F825): each with Type: erb, "Phase 22: State Systems" philosophy, assigned ERB files, CALL-derived Predecessor rows, debt resolution task stub, equivalence test task stub, zero-debt AC stub, subsystem-scoped CP-2 E2E AC stub per SSOT; F819 additionally owns IKnickersSystem full impl + ClothingSystem.cs stub completion + NullKnickersSystem + CFlagIndex (#20) + EquipIndex (#21); F820 owns Roslynator investigation; F825 owns CP-2 E2E for DI integration (its subsystem-scoped Step 2b) + all Null*Service obligations (#7, #22, #24-30, #33). **STOP gate**: If Task 1 CALL analysis reveals inter-sub-feature dependencies not covered by AC#5/13/14, or if CFlagIndex usage extends beyond CLOTHES*.ERB scope (obligation #20 routing to F819 invalidated), STOP and report to user before creating DRAFTs — user must decide whether to add new ACs or accept procedural-only enforcement | | [ ] |
| 4 | 3, 6, 16, 17 | Create 2 transition feature DRAFT files: F826 (Type: infra, Post-Phase Review Phase 22) with CP-2 Step 2c E2E checkpoint task, obligation #1 re-triage + #32 ERB boundary verification, and all F819-F825 Predecessors; F827 (Type: research, Phase 23 Planning) with F826 as Predecessor and Phase 23 content. Note: index-features.md registration consolidated in Task 6 (single-owner for shared file) | | [ ] |
| 5 | 9, 15 | Add "## Obligation Triage" section to feature-814.md documenting all 35 deferred obligation destinations (13 Phase 22 scope, 20 Counter Redux carry-forward, 1 user decision #23, 1 N+4 re-deferral #1) — each row must use "obligation #N: [description]" phrasing (colon-terminated) to satisfy AC#9 Grep patterns (colon prevents self-referential false-positive against AC table rows); append Counter Redux carry-forward items (#2-6, #8-19, #31, #34-35) to F813 Mandatory Handoffs under section label "Counter Redux carry-forward" (exact label required for AC#15 verification) — each entry must include "obligation #N" phrasing in the Issue column to satisfy AC#15 Grep patterns; also append obligation #23 (NtrReversalSource REGRESSION, user decision required) and GetNWithVisitor (INtrUtilityService full implementation, Counter Redux) to F813 Mandatory Handoffs separately | | [ ] |
| 6 | 2, 11 | Add `### Phase 22: State Systems` section to index-features.md listing all 9 features (F819-F827, including transition features F826-F827) with bold-formatted non-[DONE] Predecessors per convention; update "Next Feature number" from 819 to 828. Sole owner of index-features.md modifications | | [ ] |
| 7 | 1-24 | Verify all output files exist and are correctly formatted (AC pre-push check); commit and push to remote. Note: Task 7 is a verification pass — primary AC satisfaction is by Tasks 1-6 (creation tasks); Task 7 confirms all ACs as pre-push gate | | [ ] |

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
| 1 | researcher | sonnet | 14 ERB files in `C:\Era\game\ERB\` (CLOTHES*.ERB, PREGNACY_S*.ERB, STAIN*.ERB, ROOM_*.ERB, 天候.ERB, 睡眠深度.ERB, 生理機能追加パッチ.ERB, 続柄.ERB) | `_out/tmp/phase22-callgraph.txt`, `_out/tmp/phase22-metrics.txt` (Tasks 1-2) |
| 2 | file-creator | sonnet | CALL graph + metrics from Phase 1, Technical Design allocation table, obligation triage table | F819-F825 DRAFT files, F826-F827 DRAFT files (Tasks 3-4) |
| 3 | file-creator | sonnet | feature-814.md obligation triage plan (Technical Design section), F813 Mandatory Handoffs | "## Obligation Triage" section in feature-814.md, F813 Mandatory Handoffs updated with section labeled "Counter Redux carry-forward" (Task 5) |
| 4 | file-creator | sonnet | index-features.md current state, F819-F827 file list | index-features.md with Phase 22 section + updated Next Feature number (Task 6) |
| 5 | orchestrator | opus | All created/modified files | Git commits pushed to remote (Task 7) |

### Pre-conditions

- F783 status: [DONE] (verified)
- F813 status: [DONE] (verified)
- All 14 ERB files readable at `C:\Era\game\ERB\` (no WSL required; Grep/Read tools sufficient)
- `pm/index-features.md` "Next Feature number" is currently 819

### Execution Order

1. **Task 1 (CALL Analysis)**: Must complete before Task 3 -- DRAFT files require correct Predecessor rows derived from CALL analysis. [I] tag: exact additional dependencies unknown until Grep results examined.
2. **Task 2 (Metrics)**: Must complete before Task 3 -- validate grouping decisions with actual line counts. [I] tag: actual counts may differ from Technical Design estimates (~9,264 total).
3. **Task 3 (Implementation DRAFTs F819-F825)**: Depends on Tasks 1+2. Creates 7 files with CALL-derived Predecessor deps and all required content stubs. Do NOT use the DRAFT content structure from Technical Design directly -- Tasks 1+2 may revise Predecessor deps.
4. **Task 4 (Transition DRAFTs F826-F827)**: Can run in parallel with Task 3 (no dependencies between implementation and transition DRAFTs). F826 Predecessor list must include all F819-F825 (known from Technical Design). Note: index-features.md registration is NOT Task 4's responsibility — consolidated in Task 6 to avoid shared-file conflicts.
5. **Task 5 (Obligation Triage section)**: Can run in parallel with Tasks 3-4. Append to feature-814.md after Technical Design section. Append Counter Redux obligations to F813 per Technical Design.
6. **Task 6 (Index Update)**: Depends on Tasks 3-4 completing -- all F819-F827 IDs and statuses must be known. Use bold formatting for non-[DONE] Predecessors per `docs/architecture/migration/full-csharp-architecture.md:106`.
7. **Task 7 (Push)**: After all Tasks 1-6 complete and verified. Stage all new feature files + modified index-features.md + modified feature-814.md.

### Build Verification Steps

This is a `research` type feature. No C# build is required. Verification is file-existence and content-grep based per AC definitions.

**Pre-push verification checklist**:
- [ ] Glob for each of feature-{819..825}.md exists; count_equals 7 (AC#1)
- [ ] `Grep("### Phase 22: State Systems", "pm/index-features.md")` matches AND `Grep("| F819 ||| F820 ||| F821 ||| F822 ||| F823 ||| F824 ||| F825 ||| F826 ||| F827 ", "pm/index-features.md")` gte 9 table row matches (AC#2)
- [ ] Glob for feature-826.md and feature-827.md exist; `Grep("Type: infra", "pm/features/feature-826.md")` AND `Grep("Type: research", "pm/features/feature-827.md")` match (AC#3)
- [ ] All 14 ERB filenames appear in at least one sub-feature DRAFT (AC#4)
- [ ] `Grep("Predecessor.*F821", "pm/features/feature-822.md")` matches (AC#5)
- [ ] `Grep("CP-2 Step 2c", "pm/features/feature-826.md")` matches AND `Grep("obligation.*#1[^0-9]", "pm/features/feature-826.md")` matches AND `Grep("obligation.*#32[^0-9]", "pm/features/feature-826.md")` matches (AC#6)
- [ ] `Grep("Phase 22: State Systems")` across feature-{819..825}.md returns count_equals 7 files_with_matches (AC#7)
- [ ] Debt resolution `Grep("Remove.*TODO.*FIXME.*HACK")` scoped to feature-{819..825}.md, count_equals 7 (AC#8)
- [ ] Equivalence test `Grep("equivalence.*test")` scoped to feature-{819..825}.md, count_equals 7 (AC#18)
- [ ] Zero-debt AC `Grep("zero.*debt")` scoped to feature-{819..825}.md, count_equals 7 (AC#19)
- [ ] CP-2 E2E `Grep("CP-2 E2E checkpoint:")` scoped to feature-{819..825}.md, count_equals 7 (AC#20)
- [ ] `Grep("## Obligation Triage", "pm/features/feature-814.md")` matches AND `Grep("obligation #1:|obligation #23:|obligation #35:", "pm/features/feature-814.md")` matches AND `Grep(pm/features/feature-814.md, pattern="obligation #24:")` matches AND `Grep(pm/features/feature-814.md, pattern="obligation #8:")` matches AND `Grep(pm/features/feature-814.md, pattern="obligation #7:")` matches (AC#9)
- [ ] `Grep("IKnickersSystem", feature-819.md)` AND `Grep("Roslynator", feature-820.md)` AND `Grep("ClothingSystem.*stub", feature-819.md)` AND `Grep("CFlagIndex", feature-819.md)` AND `Grep("EquipIndex", feature-819.md)` AND `Grep("NullKnickersSystem", feature-819.md)` AND `Grep("IComableUtilities", feature-825.md)` AND `Grep("NullComHandler", feature-825.md)` all match (AC#10)
- [ ] `Grep("Next Feature number.*828", "pm/index-features.md")` matches (AC#11)
- [ ] `Grep("CP-2.*E2E.*DI|DI.*integration.*E2E", "pm/features/feature-825.md")` matches (AC#12)
- [ ] `Grep("Predecessor.*F819", "pm/features/feature-820.md")` matches (AC#13)
- [ ] `Grep("Predecessor.*F81[9]|Predecessor.*F82[0-4]", "pm/features/feature-825.md")` count_equals 6 (AC#14)
- [ ] `Grep("Counter Redux", "pm/features/feature-813.md")` matches AND `Grep("obligation.*#2[^0-9]|obligation.*#19[^0-9]|obligation.*#35[^0-9]", "pm/features/feature-813.md")` matches AND `Grep(pm/features/feature-813.md, pattern="obligation.*#10[^0-9]")` matches AND `Grep(pm/features/feature-813.md, pattern="GetNWithVisitor")` matches (AC#15)
- [ ] `Grep("Predecessor.*F81[9]|Predecessor.*F82[0-5]", "pm/features/feature-826.md")` count_equals 7 (AC#16)
- [ ] `Grep("Predecessor.*F826", "pm/features/feature-827.md")` matches AND `Grep("Phase 23", "pm/features/feature-827.md")` matches (AC#17)
- [ ] Glob(`_out/tmp/phase22-callgraph.txt`) file exists (AC#21)
- [ ] `Grep("PREGNACY_S|子供気温耐性取得", "_out/tmp/phase22-callgraph.txt")` matches (AC#24)
- [ ] `Glob("_out/tmp/phase22-metrics.txt")` matches (AC#23)
- [ ] `Grep("NullCounterUtilities", "pm/features/feature-825.md")` AND `Grep("NullWcSexHaraService", "pm/features/feature-825.md")` AND `Grep("NullNtrUtilityService", "pm/features/feature-825.md")` AND `Grep("NullTrainingCheckService", "pm/features/feature-825.md")` AND `Grep("NullEjaculationProcessor", "pm/features/feature-825.md")` AND `Grep("NullKojoMessageService", "pm/features/feature-825.md")` all match (AC#22)

### Success Criteria

All 24 ACs pass pre-push verification. 9 new DRAFT files created (F819-F827). feature-814.md has Obligation Triage section. index-features.md has Phase 22 section with Next Feature number = 828. `_out/tmp/phase22-callgraph.txt` exists. `_out/tmp/phase22-metrics.txt` exists. feature-825.md contains all 6 Null*Service names.

### Error Handling

- **CALL analysis reveals unexpected dependency**: Update DRAFT Predecessor rows to match; re-verify AC#5 still passes (>= 1 non-F814 Predecessor).
- **Actual ERB line counts exceed granularity limits**: Propose re-grouping to user before creating DRAFTs. Do not create DRAFTs with known AC violations.
- **F813 Mandatory Handoffs section requires format adjustment**: Follow existing F813 format exactly. If Counter Redux obligations already exist in F813, deduplicate.
- **Obligation #23 routing decision**: Do NOT create a new feature file for obligation #23 during /run. Document as "user decision required" in the Obligation Triage section only.

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| Counter Redux carry-forward obligations (#2-6, #8-19, #31, #34-35): 20 items of Phase 21 Counter System debt with no Phase 22 scope | No natural home in Phase 22 State Systems; must persist in F813 pending Counter Redux feature decision | Feature | F813 | Task 5 | [ ] | |
| Obligation #23 (NtrReversalSource/NtrAgreeSource REGRESSION): user decision required before routing | Potential regression not yet confirmed; routing to implementation feature risks scope disruption | Feature | F813 | Task 5 | [ ] | |
| GetNWithVisitor (INtrUtilityService full implementation): Counter Redux carry-forward | NTR_VISITOR.ERB not in Phase 22 scope; resolved-applied in Review Notes as Counter Redux routing | Feature | F813 | Task 5 | [ ] | |
| Obligation #1 (N+4 --unit deprecation NOT_FEASIBLE re-deferral): review trigger not yet met | Re-deferral condition depends on test framework status at Phase 22 Review | Feature | F826 | Task 4 | [ ] | |
| Obligation #32 (ERB file boundary != domain boundary verification step): review procedure enhancement | Most applicable at Post-Phase Review when all Phase 22 ERB files have been migrated | Feature | F826 | Task 4 | [ ] | |
| Phase 23 Planning execution | Phase 23 Planning cannot start until Post-Phase Review Phase 22 passes | Feature | F827 | Task 4 | [ ] | |

<!-- Transferred + Result columns (F811/F805 Lesson):
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: 作成済み(A), 追記済み(B), 記載済み(C), 確認済み(既存)
- Phase 9.4.1 で転記実行・Result記入。Phase 10.0.2 で検証のみ
- Prevents "Destination filled but content never transferred" gap
-->

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK
- Missing Task for Option A → FL FAIL
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

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: Summary, Technical Constraints | design-reference.md bare filename replaced with full path docs/strategy/design-reference.md
- [fix] Phase2-Review iter1: Technical Design note line 444 | Roslynator assignment corrected from F825 to F820 to match Key Decision and AC#10
- [fix] Phase2-Uncertain iter1: AC#7, AC#8 | Threshold changed from gte 6 to count_equals 7; derivation rationale updated to reflect AC#1 fixing count at 7
- [fix] Phase2-Review iter1: AC Definition Table, AC Details, AC Coverage, Build Verification | Added AC#12 (CP-2 Step 2b in F825); updated Task 3 AC# coverage, Goal Coverage, Success Criteria
- [fix] Phase2-Uncertain iter1: Obligation Triage row #20, Key Decision | CFlagIndex committed to F819 (removed ambiguous "or F826")
- [fix] Phase2-Review iter2: AC#8 | Debt resolution pattern changed from TODO\|FIXME\|HACK to Remove.*TODO.*FIXME.*HACK to avoid false positives
- [fix] Phase2-Review iter2: AC Definition Table, AC Details, AC Coverage, Build Verification | Added AC#13 (F820→F819 CALL-derived Predecessor); updated Task 3, Goal Coverage
- [fix] Phase2-Review iter2: Technical Constraints | CP-2 Step 2b text updated to reflect consolidation in F825 per Key Decision
- [fix] Phase2-Review iter2: AC#9 | Strengthened from section header existence to row count verification (count_equals 35) (subsequently revised to representative sampling in iter16)
- [fix] Phase2-Review iter3: Technical Design heading | Feature ID Allocation corrected from "F819-F828, 10 total" to "F819-F827, 9 total"
- [fix] Phase2-Review iter3: AC Design Constraints, AC#8 | Added C14 (CP-2 E2E per sub-feature per phase-20-27-game-systems.md:377); AC#8 extended with 4th grep for CP-2 E2E; DRAFT template updated
- [fix] Phase2-Review iter4: AC Definition Table, AC Details, AC Coverage | Added AC#14 (F825 sub-feature Predecessor verification); updated Goal Coverage, Task 3, Build Verification
- [fix] Phase2-Review iter4: Obligation Triage row #23 | Changed from "Separate fix feature (user decision required)" to "F813 carry-forward" to match Mandatory Handoffs destination
- [fix] Phase2-Review iter4: C14 Constraint Details | Clarified CP-2 E2E must appear as AC row, not Task
- [fix] Phase2-Review iter4: Goal | Updated from "6-8 sub-features" to "7 sub-features (F819-F825)" to match Technical Design allocation and AC#1
- [fix] Phase2-Review iter5: Task 3 | Removed #20 and #21 from F825 obligation list (routed to F819 per Triage table)
- [fix] Phase2-Review iter5: Goal Coverage | Moved AC#12 from Goal Item 2 to Goal Item 1 (F825 is implementation, not transition)
- [fix] Phase2-Review iter5: AC#4 | Clarified compound AC uses count_equals 14 with individual filename checks scoped to feature-{819..825}.md
- [fix] Phase2-Review iter5: Technical Design summary | Counter Redux carry-forward corrected to 20 items (excluding #23 which is separate user-decision bucket)
- [fix] Phase2-Review iter6: Task 3 AC# column | Added AC#5 (Task 3 creates F822 that satisfies AC#5)
- [fix] Phase2-Review iter6: AC#4 | Restated as compound type with all_pass matcher and AND conjunction semantics
- [fix] Phase2-Review iter7: AC#14 | Changed from gte 1 to count_equals 6 to enforce all 6 sub-feature Predecessors (F819-F824)
- [fix] Phase2-Review iter7: AC Definition Table, AC Details | Added AC#15 (Counter Redux carry-forward in F813 verification); updated Task 5 AC coverage
- [resolved-applied] Phase2-Review iter7: CALL graph empirical completeness — ACs 5/13/14 verify 3 pre-specified edges only; if Task 1 discovers additional call dependencies, no AC enforces their inclusion in DRAFTs. This is an inherent [I]-tag limitation: ACs are written before empirical analysis. The Execution Order (Task 1→Task 3 contract) requires CALL-derived Predecessor rows but has no AC verification for dynamic results. Resolution: STOP gate added to Task 3 — if Task 1 reveals uncovered deps, STOP before DRAFT creation.
- [fix] Phase2-Review iter7: AC#8 DRAFT template | Added note that exact stub text must be used for debt resolution task
- [fix] Phase2-Review iter8: Goal | Aligned Goal bucket counts with Technical Design triage plan (3-bucket→4-bucket: 13 Phase 22 scope + 20 Counter Redux + 1 user decision #23 + 1 N+4 #1)
- [fix] Phase2-Review iter8: C14 Constraint Details | Clarified CP-2 E2E vs Step 2b scope distinction (F819-F824 subsystem-scoped, F825 consolidated Step 2b)
- [fix] Phase2-Review iter8: AC#14 Details | Added explicit row format assumption (one Predecessor per table row)
- [fix] Phase2-Review iter9: Task 3 | Removed obligation #32 from F825 list (correctly routed to F826 per Obligation Triage Plan)
- [fix] Phase2-Review iter9: AC#6 | Replaced ambiguous "Post-Phase Review feature file" with concrete "pm/features/feature-826.md"
- [fix] Phase2-Review iter10: AC#2 | Extended method to also verify sub-feature ID entries (F819-F827) count_equals 9, not just section header
- [fix] Phase2-Review iter10: Phase 3 F819 | Added CFlagIndex typed struct (obligation #20) to F819's additionally-owns list
- [fix] Phase2-Review iter11: Philosophy Derivation | Updated CALL analysis AC Coverage from AC#5 to AC#5, AC#13, AC#14 with [I]-tag qualification
- [fix] Phase2-Review iter12: AC#9 | Fixed regex pattern from ^\| [0-9] to ^\| [0-9]+ to match multi-digit obligation row numbers (10-35)
- [fix] Phase2-Review iter12: Task 3 | Added CFlagIndex (#20) to F819's obligation list in Task 3 description (consistency with Technical Design Phase 3)
- [fix] Phase2-Review iter13: AC#2 | Changed count_equals 9 to gte 9 to prevent false failures from cross-references in index-features.md
- [fix] Phase2-Review iter14: AC#14 | Split regex alternation into 2 separate Grep calls to avoid markdown table pipe escaping issue
- [fix] Phase2-Review iter14: AC#6 | Extended to verify obligations #1 and #32 presence in F826 (previously only verified CP-2 Step 2c)
- [fix] Phase4-ACValidation iter15: AC#9 | Fixed invalid Grep syntax (removed section scoping from path)
- [fix] Phase4-ACValidation iter15: AC#14 | Cleaned Expected value (removed parenthetical explanation)
- [fix] Phase2-Review iter16: AC#9 | Changed from whole-file row count (false-negative due to 104+ matching rows across tables) to representative obligation sampling (#1, #23, #35)
- [fix] Phase2-Review iter1: feature-814.md Summary section | Removed non-template ## Summary section (content covered by Goal)
- [fix] Phase2-Review iter1: feature-814.md Deferred Obligations section | Changed ## to ### subsection of Background (template compliance)
- [fix] Phase2-Review iter1: AC#4 Matcher | Fixed count_equals to all_pass (matching logged iter6 fix)
- [fix] Phase2-Review iter1: AC Coverage AC#9 | Updated stale "exactly 35 rows" to "representative obligations (#1, #23, #35)"
- [fix] Phase2-Review iter1: AC#8 CP-2 E2E scope | Narrowed 4th grep to F819-F824 (count_equals 6), F825 verified by AC#12
- [fix] Phase2-Review iter1: AC#2 Matcher | Changed matches to all_pass for compound verification
- [fix] Phase2-Review iter1: Review Notes iteration labels | Renumbered second sequence iter8-iter16 for unique labels
- [fix] Phase2-Review iter1: Review Notes iter2 AC#9 | Added "(subsequently revised)" annotation to stale description
- [fix] Phase2-Review iter2: Key Decision, C14, AC#8, AC#12, Build Verification | Aligned CP-2 Step 2b with SSOT: each sub-feature owns its own subsystem E2E (was: consolidated in F825)
- [fix] Phase2-Review iter2: Philosophy Derivation | Fixed broken "[pending] note" reference to explicit "Review Notes [pending] Phase2-Review iter7"
- [fix] Phase2-Review iter3: AC#4 Matcher | Clarification: iter1/iter6 logged "all_pass" but all_pass is not in VALID_MATCHERS (ac_ops.py). count_equals retained as correct matcher
- [fix] Phase2-Review iter3: AC#2 Matcher | Clarification: iter1 logged "all_pass" but all_pass is not in VALID_MATCHERS (ac_ops.py). matches retained as correct matcher
- [fix] Phase7-FinalRefCheck iter3: Links section | Added missing F804 (Referenced in obligation #24 origin "F801/F804")
- [resolved-applied] Phase3-Maintainability iter3: GetNWithVisitor (INtrUtilityService) obligation leaked — SSOT phase-20-27-game-systems.md:286 lists it as Phase 22, F811 deferred to "Phase 21 or Phase 22", but NTR_VISITOR.ERB is not in Phase 22's 14 ERB files. Not tracked in F814's 35 obligations. Resolution: Option B — Route to F813 Counter Redux carry-forward (NTR_VISITOR.ERB not in Phase 22 scope; matches Counter Redux routing pattern for #2-6, #8-19).
- [fix] Phase2-Review iter4: Background section | Changed ### Deferred Obligations from F813 to bold text (non-heading) for template compliance (Background allows only 3 subsection headings)
- [fix] Phase2-Review iter4: AC#6, AC#9, Build Verification | Changed obligation.*#1 patterns to obligation.*#1[^0-9] and obligation #1 to obligation #1[^0-9] to prevent false-matching multi-digit obligation numbers (#10-#19)
- [fix] Phase2-Review iter4: Task 7 | Updated description from "Push all commits to remote" to "Verify all output files exist and are correctly formatted (AC pre-push check); commit and push to remote" to align with AC#1, AC#2, AC#11 coverage
- [fix] Phase2-Review iter5: AC#14 | Unified AC Definition Table, AC Details, and Build Verification to single Grep with combined regex pattern; eliminates 3-way implementation ambiguity
- [fix] Phase2-Review iter5: GetNWithVisitor [pending] | Resolved as Option B (Counter Redux carry-forward to F813) — NTR_VISITOR.ERB not in Phase 22's 14 ERB files, matches Counter Redux routing pattern
- [fix] Phase2-Review iter5: Review Notes line 853 clarification | Note: iter1 AC#8 CP-2 E2E scope narrowing (count_equals 6) was subsequently reverted by iter2 (count_equals 7 is correct per SSOT); original [fix] entry preserved as immutable audit history
- [fix] Phase2-Review iter6: AC#6 | Removed '|E2E checkpoint' alternative from first Grep pattern; now uses 'CP-2 Step 2c' only to specifically verify Step 2c ownership (not any E2E mention)
- [fix] Phase2-Review iter7: Philosophy Derivation | Clarified AC#6 mapping: "Each phase completion triggers next phase planning" maps to AC#3 and AC#6 (CP-2 Step 2c portion); AC#6's obligation #1/#32 routing portion derives from C10 (obligation triage), not pipeline continuity
- [fix] Phase2-Review iter8: AC#16, AC#17 | Added AC#16 (F826 has all F819-F825 Predecessors, count_equals 7) and AC#17 (F827 has F826 Predecessor + Phase 23 content); updated Task 4 AC# to include 16, 17; updated Goal Coverage, AC Coverage, Build Verification, Success Criteria
- [fix] Phase2-Review iter8: AC#15 | Strengthened with representative Counter Redux obligation numbers (#2, #19, #35) to verify actual content transfer, not just label
- [fix] Phase3-Maintainability iter6: Obligation Triage #26 | Added GetNWithVisitor stub behavior note (returns default 0 since NTR_VISITOR.ERB not in Phase 22 scope); clarifies NullNtrUtilityService implementation basis for F825
- [fix] Phase4-ACValidation iter7: AC#3, AC#6, AC#9, AC#12, AC#15, AC#17 | Replaced descriptive Expected values with concrete regex patterns for testability
- [resolved-skipped] Phase4-ACValidation iter7: AC#2 compound matcher inconsistency — Matcher column 'matches' with Expected containing 'gte 9' for sub-feature ID count. Split into two ACs rejected 3 times (iter1, iter7, iter8) because 'all_pass' is not valid matcher and compound format is documented design decision. Resolution: User accepted as design exception — F814 is research type with manual verification; compound format functions correctly for human verification. ac_ops.py mechanical verification not applicable.
- [fix] Phase7-FinalRefCheck iter8: phase-20-27-game-systems.md | Replaced 27 bare filename references with full path docs/architecture/migration/phase-20-27-game-systems.md (2 remaining in immutable Review Notes entries)
- [fix] Phase1-RefCheck iter1: Related Features table | Added F804 row (was in Links but missing from Related Features table)
- [fix] Phase2-Review iter1: AC#4 Method and Details | Replaced <ERB filename> placeholder and glob patterns with explicit 14 ERB filenames from Technical Design allocation table
- [fix] Phase2-Uncertain iter2: C14 Constraint Details | Added verbatim SSOT quote from full-csharp-architecture.md:249-251 for traceability
- [fix] Phase2-Uncertain iter3: AC#8 4th grep | Changed pattern from CP-2.*E2E\|E2E.*checkpoint to CP-2 E2E checkpoint: (colon-terminated) for AC-row-only matching per C14 requirement
- [fix] Phase2-Review iter4: Goal Coverage table | Added AC#6 to Goal Item 4 (Triage all 35 obligations) — AC#6 verifies obligations #1/#32 routing to F826 (C10 contribution)
- [fix] Phase2-Review iter5: Goal Coverage table | Removed AC#16 from Goal Item 3 (CALL-derived deps) — F826 Predecessors are sequential, not CALL-derived
- [fix] Phase2-Review iter5: AC#6 Grep pattern | Removed OR alternatives (re-triage, ERB.*boundary) from obligation patterns to prevent false-positive on generic text
- [fix] Phase2-Uncertain iter5: AC#10 Matcher | Changed from count_equals 3 to matches (compound) for consistency with AC#3/AC#6 pattern
- [fix] Phase2-Review iter6: AC#10 Details and AC Coverage | Updated stale count_equals descriptions to matches semantics after iter5 matcher change
- [fix] Phase3-Maintainability iter7: Task 5 description + Implementation Contract | Added explicit "Counter Redux carry-forward" label requirement for AC#15 verification consistency
- [fix] Phase2-Review iter8: Task 5 description | Added obligation #23 F813 transfer (was in Mandatory Handoffs but missing from Task 5 explicit list)
- [fix] Phase2-Review iter9: Feasibility Assessment | Updated stale estimates: "6-8 sub-features" → "7 (F819-F825)", "~5-7 Phase 22/~28-30 Counter" → "13/20/1/1" per Technical Design triage
- [fix] Phase2-Review iter10: Mandatory Handoffs + Task 5 | Added GetNWithVisitor carry-forward (resolved-applied at line 881 but missing from execution path)
- [fix] PostLoop-UserFix iter1: Task 3 | Added STOP gate for uncovered CALL dependencies (resolved [pending] Phase2-Review iter7)
- [fix] Phase2-Review iter1: AC#8 Definition Table, AC Details, AC Coverage, Build Verification, Success Criteria, Constraints | Split compound AC#8 (4 checks) into AC#8 (C2 debt), AC#18 (C3 equivalence), AC#19 (C4 zero-debt), AC#20 (C14 CP-2 E2E) for independent mechanical verification
- [fix] Phase2-Uncertain iter2: AC#9, AC#15 | Anchored unanchored obligation patterns: #23→#23[^0-9], #35→#35[^0-9], #19→#19[^0-9] for trailing-digit boundary consistency
- [fix] Phase2-Uncertain iter2: AC#6 | Anchored obligation.*#32→obligation.*#32[^0-9] for trailing-digit boundary consistency with sibling #1[^0-9] pattern
- [fix] Phase2-Review iter2: C7 Constraint Details | Updated AC Impact note to reflect AC#6 compound nature (CP-2 Step 2c + obligations #1/#32, shared with C10)
- [fix] Phase2-Review iter3: Task 5 | Added explicit "obligation #N" phrasing requirement for Obligation Triage section (AC#9) and F813 Mandatory Handoffs append (AC#15) to prevent format mismatch false negatives
- [fix] Phase2-Review iter1: Background section | Moved Deferred Obligations table from Background (after Goal) to standalone ### subsection after Background closing --- (template compliance: Background allows only Philosophy/Problem/Goal)
- [fix] Phase2-Review iter1: Task 7 AC# column | Updated from "1, 2, 11" to "1-20" to reflect Task 7's role as final verification gate for all ACs per Build Verification checklist
- [fix] Phase2-Review iter1: Task 3 STOP gate | Extended to include CFlagIndex scope check (obligation #20 routing validation) alongside inter-sub-feature dependency coverage check
- [fix] Phase2-Review iter2: AC#9 | Added 2 more representative samples (#24 Phase-22-scope, #8 Counter Redux) for 4-bucket coverage (N+4 #1, Phase-22 #24, Counter Redux #8, user-decision #23, boundary #35)
- [fix] Phase2-Review iter2: AC#15 | Added mid-batch sample (#10) to verify interior items in Counter Redux carry-forward batch
- [fix] Phase2-Review iter3: AC#10 | Extended with CFlagIndex and EquipIndex Grep checks for F819 (obligations #20/#21 Phase 22 scope verification)
- [fix] Phase2-Uncertain iter3: Technical Design F825 | Added note clarifying DI Integration is supporting infrastructure for Phase 22 completion, not a 9th subsystem
- [fix] Phase2-Review iter1: AC Coverage AC#5 | Added annotation clarifying Task 1→Task 3 data flow and procedural STOP gate enforcement (Task 1 dynamic output not directly AC-verified)
- [fix] Phase2-Review iter1: AC#9 | Added obligation #7 (IComHandler, Phase-22-scope) as 6th representative sample for improved Phase-22-scope bucket coverage
- [fix] Phase2-Review iter1: C2 Constraint Details | Added Format Note documenting AC#8 canonical stub phrasing as deliberate format constraint
- [fix] Phase2-Review iter2: AC#14, AC#16 Details | Added output_mode="count" to Grep specification (default files_with_matches returns 0/1, not line count needed for count_equals 6/7)
- [fix] Phase2-Review iter2: Philosophy Derivation | Added obligation triage row (Problem-derived) mapping to AC#9, AC#15 for traceability
- [fix] Phase2-Uncertain iter2: Task 7 | Added clarifying note that Task 7 is verification pass, not primary AC source
- [resolved-skipped] Phase2-Uncertain iter3: AC#9 exhaustive obligation verification — representative sampling (6 samples) accepted as design exception. Task 5 procedural enforcement + AC#10/AC#22 specific assignment verification provides sufficient coverage.
- [fix] Phase2-Review iter3: AC#11 | Changed matcher from gte to matches; Grep pattern encodes exact value 828, gte on string result is semantically invalid
- [fix] Phase2-Review iter4: Philosophy + Philosophy Derivation | Extended Philosophy to include obligation triage ("domain-scoped triage of carry-forward obligations"); updated derivation row from "(Problem-derived)" to direct Philosophy quote for proper traceability chain
- [resolved-applied] Phase2-Review iter4: Task 1 AC#5 alignment — Task 1 [I] claimed AC#5 but AC#5 verifies pre-known edge created by Task 3. Resolved via option (b): added AC#21 (Glob(_out/tmp/phase22-callgraph.txt) file existence check) as Task 1's dedicated deterministic AC; Task 1 AC# changed from 5 to 21; [I] tag removed (AC#21 is a deterministic file existence check, not uncertain).
- [fix] Phase2-Review iter5: Task 2 | Added clarifying note that granularity applies to final specs after /fc, not DRAFTs; Task 2 validates grouping allows proper granularity
- [fix] Phase2-Review iter5: AC#10 | Added NullComHandler (obligation #33) Grep check for F825 to verify Null*Service domain routing correctness
- [fix] Phase2-Review iter5: C7 Constraint Details | Added AC#6/AC#9/AC#10 scope distinction for C10 coverage boundaries
- [fix] Phase2-Review iter6: AC#10 | Added NullKnickersSystem (#28) Grep check for F819 (obligation domain routing verification)
- [fix] Phase2-Review iter6: Task 4, Task 6, Execution Order | Removed index-features.md registration from Task 4; consolidated all index modifications in Task 6 (single-owner for shared file)
- [fix] Phase2-Review iter7: AC#9 | Changed obligation patterns from `obligation.*#N[^0-9]` to colon-terminated `obligation #N:` format to prevent self-referential false-positive matching against AC table rows and Review Notes entries
- [fix] Phase2-Review iter7: AC#15 | Added GetNWithVisitor Grep check for F813 to verify separately-tracked carry-forward obligation transfer (outside 35-item list)
- [fix] Phase2-Review iter8: Obligation Triage #15 | Corrected rationale from "Phase 23+" to "Counter Redux carry-forward" to match F813 destination
- [fix] Phase2-Review iter8: AC#10 | Added IComableUtilities (#22) Grep check for F825 (interface consolidation domain routing)
- [fix] Phase2-Review iter8: Philosophy Derivation | Fixed iter reference from iter7 to iter4 (correct [pending] entry location)
- [fix] Phase2-Review iter1: Deferred Obligations heading | Changed ### Deferred Obligations from F813 to bold text (non-heading) for template compliance (orphaned ### between Background --- and Root Cause Analysis)
- [fix] Phase2-Review iter1: Task 1 AC#5 alignment | Added AC#21 (callgraph.txt file existence), updated Task 1 AC# from 5 to 21, removed [I] tag (AC#21 is deterministic); resolved [pending] Phase2-Review iter4
- [fix] Phase2-Review iter1: AC coverage obligations #24-27,#29-30 | Added AC#22 (6 Null*Service Grep checks in F825); updated Task 3, Task 7, Goal Coverage, Build Verification
- [fix] Phase2-Review iter1: Background section | Moved Deferred Obligations block into Background section (before closing ---) for template compliance
- [fix] Phase2-Review iter1: AC Definition Table, AC Details, AC Coverage, Build Verification | Added AC#23 (phase22-metrics.txt existence check); updated Task 2, Task 7, Goal Coverage
- [fix] Phase2-Review iter2: Goal Coverage table | Moved AC#23 from Goal Item 1 to Goal Item 3 (metrics enables dependency analysis, not sub-feature creation)
- [fix] Phase2-Review iter2: AC#2 | Strengthened second Grep pattern from bare ID matching to table-row-anchored pattern for index-features.md registration verification
- [resolved-applied] Phase2-Review iter3: Goal Coverage AC#23 placement — AC#23 moved from Goal 3 to Goal 1 per user decision (metrics validates grouping for sub-feature creation, not dependency correctness)
- [fix] Phase2-Review iter3: AC#23 Details | Added note clarifying AC#23 is deterministic despite Task 2's [I] tag (consistent with AC#21 precedent)
- [fix] Phase3-Maintainability iter4: Philosophy Derivation | Updated stale [pending] reference to [resolved-applied] for Phase2-Review iter4 (AC#21 addition resolved the concern)
- [fix] Phase2-Review iter5: AC Definition Table, AC Details, AC Coverage, Build Verification | Added AC#24 (callgraph content verification via PREGNACY_S/子供気温耐性取得 pattern); updated Task 1, Task 7, Goal Coverage
- [fix] PostLoop-UserFix iter6: Goal Coverage table | Moved AC#23 from Goal Item 3 to Goal Item 1 per user decision

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning
- [Predecessor: F813](feature-813.md) - Post-Phase Review Phase 21
- [Related: F647](feature-647.md) - Phase 20 Planning
- [Related: F782](feature-782.md) - Post-Phase Review Phase 20 (N+4 obligation origin chain)
- [Related: F811](feature-811.md) - SOURCE Entry System (IKnickersSystem deferral)
- [Related: F797](feature-797.md) - IVariableStore migration
- [Related: F801](feature-801.md) - Main Counter Core (NullCounterUtilities deferred)
- [Related: F804](feature-804.md) - Counter Utilities Extended (NullCounterUtilities partial origin)
- [Related: F803](feature-803.md) - Main Counter Source (IShrinkageSystem etc. deferred)
- [Related: F806](feature-806.md) - WC Counter Message SEX (multiple deferrals)
- [Related: F807](feature-807.md) - WC Counter Message TEASE (multiple deferrals)
- [Related: F808](feature-808.md) - WC Counter Message ITEM+NTR (ERB boundary lesson)
- [Related: F810](feature-810.md) - Comable/Counter Utilities (consolidation deferred)
- [Related: F815](feature-815.md) - Golden Test Design
- [Related: F816](feature-816.md) - StubVariableStore
- [Related: F818](feature-818.md) - ac-static-verifier
- [Successor: F819](feature-819.md) - Clothing Core
- [Successor: F820](feature-820.md) - Clothing Extended
- [Successor: F821](feature-821.md) - Weather System
- [Successor: F822](feature-822.md) - Pregnancy System
- [Successor: F823](feature-823.md) - Room & Stain
- [Successor: F824](feature-824.md) - Sleep & Menstrual
- [Successor: F825](feature-825.md) - Relationships & DI Integration
- [Successor: F826](feature-826.md) - Post-Phase Review Phase 22 (infra)
- [Successor: F827](feature-827.md) - Phase 23 Planning (research)
