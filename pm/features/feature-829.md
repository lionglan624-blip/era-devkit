# Feature 829: Phase 22 Deferred Obligations Consolidation

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

## Type: infra

## Deviation Context
<!-- Written by /run Phase 9 (F826). Raw facts only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F826 |
| Discovery Phase | Phase 9 |
| Timestamp | 2026-03-06 |

### Consolidated Obligations from Phase 22

The following deferred items were consolidated during F826 (Post-Phase Review Phase 22). Each item was originally deferred by a Phase 22 sub-feature and requires concrete resolution.

**Obligation #1 re-deferral (N+4 chain)**:
- --unit deprecation NOT_FEASIBLE: 100+ kojo JSON test files and 9+ skill files still depend on --unit; trigger condition ("kojo no longer requires ERB test runner") unmet; chain: F782->F813->F814->F826->F829

**Deferred from F819 (Clothing System)**:
- SANTA cosplay text output (CLOTHE_EFFECT.ERB @SANTA PRINT calls): UI-layer concern; Era.Core has no UI primitives; requires engine layer
- CLOTHES_ACCESSORY requires INtrQuery (NullNtrQuery returns false): INtrQuery dependency not injected in Clothing System; behavioral gap until NTR system migrates

**Deferred from F822 (Pregnancy System)**:
- IVariableStore 2D SAVEDATA accessor stubs (GetBirthCountByParent/SetBirthCountByParent return 0/no-op): Full runtime implementation requires generic 2D SAVEDATA variable accessor on IVariableStore
- NullMultipleBirthService runtime implementation (8-method stub, multi-birth logic not migrated)

**Deferred from F824 (Sleep & Menstrual)**:
- CVARSET bulk reset shared method extraction (IVariableStore.BulkResetCharacterFlags): inlined by F824; extraction needed when MOVEMENT.ERB migrates (second call site)
- IS_DOUTEI shared utility extraction (ICharacterUtilities.IsDoutei): 22 call sites across 14 files; F824 inlined 1; extraction when second C# call site appears

**Deferred from F825 (Relationships & DI Integration)**:
- I3DArrayVariables GetDa/SetDa DA interface gap (ROOM_SMELL_WHOSE_SAMEN): cross-cutting DA interface gap
- IEngineVariables indexed methods no-op stubs (GetDay/SetDay/GetTime/SetTime): pending engine repo implementation

**Cross-cutting**:
- CP-2 Step 2c behavioral flow test (Shop->Counter->State): structural DI resolution only in F826; behavioral flow deferred because Phase 22 services use Null stubs
- Roslynator Analyzers investigation (Phase 22 Task 12): assigned to F819 by F814, but F819 declared out of scope; leaked obligation

**Tooling**:
- ac-static-verifier.py numeric Expected parsing for Grep-method ACs: cannot parse plain numeric Expected (e.g., `37`) when Method uses `Grep(path, pattern="...")` format

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)
Deferred obligations are the SSOT for incomplete migration work: every obligation discovered during Phase N execution that belongs to Phase N+2 or later must have a concrete, trackable destination before the originating phase boundary closes. No obligation may exist without an assigned resolution phase or feature.

### Problem (Current Issue)
Phase 22's incremental sub-feature execution pattern (F819, F822, F824, F825) systematically produced 12 deferred obligations that accumulated in F826's Mandatory Handoffs table without concrete resolution destinations, because the phase-gated migration architecture creates a structural gap: obligations requiring infrastructure not yet available (UI primitives, NTR system, generic 2D SAVEDATA, engine runtime) cannot be resolved within Phase 22's "State Systems" scope, and destination features for Phase 24-27 do not yet exist. Additionally, the Roslynator investigation obligation leaked through two features (F814 assigned to F819, F819 declared out-of-scope) with zero investigation performed, and the --unit deprecation chain has been re-deferred 4 times (F782->F813->F814->F826) because its trigger condition remains unmet.

### Goal (What to Achieve)
Route all 12 deferred obligations from F826's Mandatory Handoffs to concrete destinations (Phase ID, Feature ID, or NOT_FEASIBLE disposition with documented trigger condition), ensuring zero untracked obligations remain at the Phase 22 boundary. For the ac-static-verifier bug, verify whether the issue reproduces and scope a fix if confirmed. Document the --unit deprecation as NOT_FEASIBLE (N+5) with the same trigger condition. Assign the Roslynator investigation to a concrete destination feature.

### Predecessor Obligations (for Mandatory Handoffs)

| Source Feature | Obligation | Category | Status |
|:--------------:|------------|----------|:------:|
| F826 | --unit deprecation NOT_FEASIBLE re-deferral (N+4 chain F782->F813->F814->F826) | deferred | pending |
| F826 (from F819) | SANTA cosplay text output (CLOTHE_EFFECT.ERB @SANTA PRINT calls) | handoff | pending |
| F826 (from F819) | CLOTHES_ACCESSORY requires INtrQuery (NullNtrQuery returns false) | handoff | pending |
| F826 (from F822) | IVariableStore 2D SAVEDATA accessor stubs (GetBirthCountByParent/SetBirthCountByParent) | handoff | pending |
| F826 (from F822) | NullMultipleBirthService runtime implementation (8-method stub) | handoff | pending |
| F826 (from F824) | CVARSET bulk reset shared method extraction (IVariableStore.BulkResetCharacterFlags) | handoff | pending |
| F826 (from F824) | IS_DOUTEI shared utility extraction (ICharacterUtilities.IsDoutei) | handoff | pending |
| F826 (from F825) | I3DArrayVariables GetDa/SetDa DA interface gap | handoff | pending |
| F826 (from F825) | IEngineVariables indexed methods no-op stubs (GetDay/SetDay/GetTime/SetTime) | handoff | pending |
| F826 | CP-2 Step 2c behavioral flow test (Shop->Counter->State) | handoff | pending |
| F826 (from F814) | Roslynator Analyzers investigation (Phase 22 Task 12) | deferred | pending |
| F826 | ac-static-verifier.py numeric Expected parsing for Grep-method ACs | deferred | pending |

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do 12 obligations need a consolidation feature? | F826 collected obligations from 7 sub-features but its scope is verification and routing, not resolution | `pm/features/feature-826.md:611-624` |
| 2 | Why did sub-features defer these items? | Each obligation crosses subsystem boundaries requiring infrastructure not yet available (UI primitives, NTR system, 2D SAVEDATA, engine runtime) | `Era.Core\Interfaces\IMultipleBirthService.cs:37-47` (NullMultipleBirthService 8 no-op methods) |
| 3 | Why can't they be resolved at the Phase 22 boundary? | Phase 22 scope is "State Systems" migration; obligations requiring engine-layer, NTR system, or generic variable store changes are architecturally out-of-scope | `docs/architecture/migration/phase-20-27-game-systems.md:564-604` (Phase 24 = NTR Bounded Context) |
| 4 | Why weren't they routed directly to Phase 24-27 features? | Those destination features don't exist yet; only phase-level definitions exist in architecture.md | `pm/features/feature-827.md:103` (F827 explicitly excludes obligation triage) |
| 5 | Why (Root)? | The phase-gated migration architecture creates a structural gap: obligations discovered during Phase N that belong to Phase N+2 or later have no existing destination feature, requiring an explicit consolidation feature to bridge the gap before the phase boundary closes | `pm/features/feature-829.md:19-27` (F829 created as deviation from F826) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 12 deferred obligations accumulated in F826's Mandatory Handoffs table | Phase-gated architecture lacks a mechanism for routing obligations to future phases that have no concrete features yet |
| Where | F826 Mandatory Handoffs (surface: individual sub-feature deferral decisions) | Migration architecture design: per-feature deferral model without cross-phase consolidation step |
| Fix | Manually track each obligation in spreadsheet or comments | Create explicit consolidation feature (F829) that routes each obligation to a concrete destination before the phase boundary closes |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F826 | [WIP] | Predecessor: Post-Phase Review Phase 22 (source of all 12 handoffs) |
| F827 | [DRAFT] | Sibling: Phase 23 Planning (explicitly excludes obligation triage) |
| F819 | [DONE] | Source: SANTA cosplay, CLOTHES_ACCESSORY/INtrQuery, Roslynator leak |
| F822 | [DONE] | Source: IVariableStore 2D stubs, NullMultipleBirthService |
| F824 | [DONE] | Source: BulkResetCharacterFlags, IS_DOUTEI extraction |
| F825 | [DONE] | Source: I3DArrayVariables DA gap, IEngineVariables indexed stubs |
| F814 | [DONE] | Source: Roslynator investigation assignment (routed to F819, rejected) |
| F813 | [DONE] | Chain: --unit deprecation N+3 link |
| F782 | [DONE] | Chain: --unit deprecation origin |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Scope clarity | FEASIBLE | All 12 obligations enumerated with source features and descriptions in F826:611-624 |
| Technical complexity | FEASIBLE | Primary work is documentation/routing; only potential code change is ac-static-verifier bug fix |
| Dependencies | FEASIBLE | F826 is sole predecessor; all source features are [DONE] |
| Risk level | FEASIBLE | No architectural risk; obligations are routed, not implemented |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Phase 22 closure | HIGH | F829 must complete before Phase 22 obligations are fully tracked |
| Phase 24-27 planning | MEDIUM | Routing decisions inform future phase scoping |
| Tooling (ac-static-verifier) | LOW | Bug fix affects Grep-method AC verification but has workaround |
| Migration workflow | MEDIUM | Establishes precedent for post-phase obligation consolidation |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| F829 is Type: infra -- no C# migration code | `feature-829.md:17` | Tasks must be documentation/routing/tooling only, not C# implementation |
| Phase 23 (F827) is research type, not a valid obligation destination | `feature-827.md:103,132` | Obligations must route to Phase 24-27 or standalone features, not F827 |
| --unit deprecation trigger unmet | 8 skill files (28 occurrences), 76+ feature files (418+ occurrences), 100+ kojo JSON test files | Cannot deprecate --unit until kojo no longer requires ERB test runner |
| Roslynator has zero presence in codebase | No PackageReference in any repo | Investigation must start from scratch |
| Engine repo stubs require cross-repo coordination | `IEngineVariables.cs:118-130` default interface methods | Cannot be resolved within devkit or core repos alone |
| ac-static-verifier Format C code exists but may still fail at runtime | `ac-static-verifier.py:880-890,1032-1040` vs F826 deviation log | Bug needs reproduction verification before scoping fix |
| F826 must finalize before F829 can pass /fl | `pm/index-features.md` F826 = [WIP] | F829 is BLOCKED until F826 reaches [DONE] |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| --unit deprecation chain extends indefinitely (N+5, N+6...) | HIGH | LOW | Document NOT_FEASIBLE with same trigger condition; chain is structurally bounded by ERB-to-C# migration completion |
| Obligation routing without concrete feature IDs creates TBD violations | MEDIUM | HIGH | Route to phase-level destinations (Phase 24, Phase 25) per Deferred Task Protocol Option C |
| Roslynator investigation scope creep into full analyzer setup | MEDIUM | MEDIUM | Scope as standalone investigation-only feature |
| ac-static-verifier fix introduces regression in existing AC verification | LOW | MEDIUM | Existing test suite provides regression coverage; TDD approach |
| Phase 24-27 architecture changes invalidate routing decisions | LOW | MEDIUM | Route to phases not individual features; re-route when phase planning features are created |
| F829 scope creeps into implementing obligation fixes rather than routing | LOW | HIGH | Enforce Type: infra constraint; all code-level stubs stay in core/engine repos |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Obligation count in F826 Mandatory Handoffs | `grep -c "F829" pm/features/feature-826.md` | 12 | All 12 rows have Destination ID = F829 |
| --unit references in skill files | `grep -rl "\-\-unit" .claude/skills/ \| wc -l` | 8 | Skill files referencing --unit |
| Roslynator PackageReference count | `grep -r "Roslynator" --include="*.csproj" \| wc -l` | 0 | Zero presence in codebase |

**Baseline File**: `_out/tmp/baseline-829.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | F829 is Type: infra -- all deliverables are documentation/routing artifacts | `feature-829.md:17` | ACs must use file/content matchers (Grep, Glob), not build/dotnet_test |
| C2 | Each of 12 obligations must have a concrete destination (Phase ID, Feature ID, or NOT_FEASIBLE) | Deferred Task Protocol + F826:611-624 | ACs must verify each obligation has documented destination |
| C3 | --unit deprecation must be NOT_FEASIBLE (N+5) with trigger condition | Chain: F782->F813->F814->F826->F829 | AC must verify NOT_FEASIBLE disposition and trigger condition documented |
| C4 | Roslynator must get a concrete destination feature or phase | `phase-20-27-game-systems.md:300` Task 12 + MEMORY.md Phase 5 link | AC must verify Roslynator routed to concrete destination |
| C5 | ac-static-verifier bug needs reproduction verification before fix | F826 deviation log vs existing code at lines 880-890 | If bug reproduces: AC must include regression test. If already fixed: AC verifies no-op |
| C6 | No obligations may route to Phase 23 (F827) | `feature-827.md:103,132` | ACs must verify zero obligations assigned to F827/Phase 23 |
| C7 | Trigger-based obligations must document concrete trigger conditions | Deferred Task Protocol | ACs must verify trigger documentation for conditional obligations |

### Constraint Details

**C1: Infra Type Enforcement**
- **Source**: Feature type declaration in `feature-829.md:17`
- **Verification**: Type field = infra; no .cs files in affected files list
- **AC Impact**: All ACs must verify documentation artifacts (markdown content, routing tables), not compiled code output

**C2: Obligation Destination Completeness**
- **Source**: F826 Mandatory Handoffs table (12 rows, all Destination ID = F829, all Transferred [x])
- **Verification**: Count obligations in Deviation Context; verify each has destination in output
- **AC Impact**: AC must verify all 12 obligations are accounted for with no "TBD" destinations

**C3: --unit NOT_FEASIBLE Chain**
- **Source**: N+4 chain documented in F826:613; trigger condition "kojo no longer requires ERB test runner"
- **Verification**: Grep for trigger condition text in routing output
- **AC Impact**: AC must verify both NOT_FEASIBLE disposition AND trigger condition are documented

**C4: Roslynator Destination**
- **Source**: Phase 22 Task 12 (`phase-20-27-game-systems.md:300`) + MEMORY.md Roslyn Analyzers note
- **Verification**: Grep for Roslynator destination in routing output
- **AC Impact**: AC must verify Roslynator is assigned to a specific destination (not left floating)

**C5: ac-static-verifier Verification**
- **Source**: F826 deviation log ("ac-static-verifier.py --ac-type file exit 1") vs existing Format C code at `ac-static-verifier.py:880-890`
- **Verification**: Attempt to reproduce the failure case
- **AC Impact**: If bug reproduces, AC must verify fix + regression test. If already fixed, AC verifies current behavior is correct

**C6: Phase 23 Exclusion**
- **Source**: `feature-827.md:103` ("F826 obligations routed to F829, not F827") and F827 type: research
- **Verification**: Grep routing output for any F827/Phase 23 destination
- **AC Impact**: AC must verify zero obligations routed to Phase 23

**C7: Trigger Condition Documentation**
- **Source**: Deferred Task Protocol requires concrete triggers for conditional obligations
- **Verification**: Each "extract when X" or "implement when Y" obligation has its trigger spelled out
- **AC Impact**: ACs for trigger-based obligations (IS_DOUTEI "second C# call site", BulkReset "MOVEMENT.ERB migrates") must verify trigger text exists

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F826 | [DONE] | Post-Phase Review Phase 22; must be [DONE] before F829 can pass /fl |
| Related | F827 | [DRAFT] | Phase 23 Planning; sibling feature, explicitly separated from obligation routing |
| Related | F819 | [DONE] | Clothing System; source of 2 obligations + Roslynator leak |
| Related | F822 | [DONE] | Pregnancy System; source of 2 obligations |
| Related | F824 | [DONE] | Sleep & Menstrual; source of 2 obligations |
| Related | F825 | [DONE] | Relationships & DI Integration; source of 2 obligations |
| Related | F814 | [DONE] | Phase 22 Planning; assigned Roslynator to F819 |
| Related | F813 | [DONE] | --unit deprecation chain N+3 link |
| Related | F782 | [DONE] | --unit deprecation chain origin |

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
| "every obligation discovered during Phase N execution...must have a concrete, trackable destination" | All 12 obligations from F826 Mandatory Handoffs have documented destinations (Phase ID, Feature ID, or NOT_FEASIBLE) | AC#1, AC#2, AC#4, AC#6 |
| "No obligation may exist without an assigned resolution phase or feature" | Zero obligations remain without destination; no TBD entries in routing table | AC#1, AC#2, AC#11 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Obligation Routing section contains 12 entries | file | Grep(pm/features/feature-829.md, pattern="\\| OB-[0-9]+ \\|") | gte | 12 | [ ] |
| 2 | No TBD in Obligation Routing destination column | file | Grep(pm/features/feature-829.md, pattern="^\\| OB-.*TBD") | not_matches | `TBD` in routing row | [ ] |
| 3 | Routing section header exists | file | Grep(pm/features/feature-829.md, pattern="^## Obligation Routing") | matches | `## Obligation Routing` | [ ] |
| 4 | --unit deprecation disposition is NOT_FEASIBLE | file | Grep(pm/features/feature-829.md, pattern="^\\| OB-01.*NOT_FEASIBLE") | matches | `NOT_FEASIBLE` in routing row | [ ] |
| 5 | --unit N+5 chain and trigger documented | file | Grep(pm/features/feature-829.md, pattern="^### OB-01") | gte | 1 | [ ] |
| 6 | Roslynator routed to concrete destination | file | Grep(pm/features/feature-829.md, pattern="^\\| OB-11.*Phase [0-9]+") | matches | Phase destination | [ ] |
| 7 | ac-static-verifier reproduction result documented | file | Grep(pm/features/feature-829.md, pattern="^### OB-12") | gte | 1 | [ ] |
| 8 | Zero routing entries target Phase 23 or F827 | file | Grep(pm/features/feature-829.md, pattern="^\\| OB-.*Phase 23") | not_matches | no Phase 23 destination | [ ] |
| 9 | Trigger conditions documented for conditional obligations | file | Grep(pm/features/feature-829.md, pattern="Trigger:.*when") | gte | 4 | [ ] |
| 10 | Architecture doc updated with F829 routing references | file | Grep(docs/architecture/migration/phase-20-27-game-systems.md, pattern="F829") | matches | `F829` cross-reference | [ ] |
| 11 | Routing details section exists with per-obligation analysis | file | Grep(pm/features/feature-829.md, pattern="^### OB-") | gte | 12 | [ ] |

### AC Details

**AC#1: Obligation Routing section contains 12 entries**
- **Test**: `Grep(pm/features/feature-829.md, pattern="\\| OB-[0-9]+ \\|")`
- **Expected**: >= 12 matches (one routing row per obligation)
- **Rationale**: Philosophy requires every obligation to have a trackable destination. 12 obligations enumerated in Predecessor Obligations table.
- **Derivation**: 12 obligations from F826 Mandatory Handoffs: OB-01 --unit deprecation, OB-02 SANTA cosplay, OB-03 CLOTHES_ACCESSORY/INtrQuery, OB-04 IVariableStore 2D SAVEDATA stubs, OB-05 NullMultipleBirthService, OB-06 CVARSET BulkReset extraction, OB-07 IS_DOUTEI extraction, OB-08 I3DArrayVariables DA gap, OB-09 IEngineVariables indexed stubs, OB-10 CP-2 behavioral flow test, OB-11 Roslynator investigation, OB-12 ac-static-verifier numeric parsing.

**AC#9: Trigger conditions documented for conditional obligations**
- **Test**: `Grep(pm/features/feature-829.md, pattern="Trigger:.*when")`
- **Expected**: >= 4 trigger condition entries
- **Rationale**: Constraint C7 requires trigger documentation for conditional obligations.
- **Derivation**: At least 4 obligations are trigger-based: (1) OB-01 --unit ("when kojo no longer requires ERB test runner"), (2) OB-06 BulkReset ("when MOVEMENT.ERB migrates"), (3) OB-07 IS_DOUTEI ("when second C# call site appears"), (4) OB-03 CLOTHES_ACCESSORY ("when NTR system migrates").

**AC#11: Routing details section exists with per-obligation analysis**
- **Test**: `Grep(pm/features/feature-829.md, pattern="^### OB-")`
- **Expected**: >= 12 matches (one subsection per obligation)
- **Rationale**: Each obligation requires documented analysis explaining destination choice, not just a table row.
- **Derivation**: 12 obligations, each requiring a `### OB-{NN}` subsection with destination rationale. Pattern uses `^` anchor to match only section headers, not inline references.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Route all 12 deferred obligations to concrete destinations | AC#1, AC#2, AC#3, AC#11 |
| 2 | Zero untracked obligations at Phase 22 boundary | AC#1, AC#2 |
| 3 | ac-static-verifier bug: verify reproduction and scope fix | AC#7 |
| 4 | --unit deprecation as NOT_FEASIBLE (N+5) with trigger | AC#4, AC#5 |
| 5 | Roslynator investigation to concrete destination | AC#6 |
| 6 | No obligations to Phase 23/F827 (C6 constraint) | AC#8 |
| 7 | Trigger conditions documented for conditional obligations (C7 constraint) | AC#9 |
| 8 | Phase architecture doc updated with routing references | AC#10 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

F829 is a pure documentation/routing feature (Type: infra). Its entire deliverable is the `## Obligation Routing` section written into `pm/features/feature-829.md` itself, plus a cross-reference line added to `docs/architecture/migration/phase-20-27-game-systems.md`.

The approach is:

1. Write a routing summary table (`## Obligation Routing`) with one row per obligation (OB-01 through OB-12). Each row specifies the obligation ID, short name, and concrete destination (Phase ID, Feature ID, or NOT_FEASIBLE disposition).
2. Write one `### OB-{NN}` subsection per obligation with: destination rationale, trigger condition (for conditional obligations using `Trigger: when <condition>`), and migration chain context where applicable.
3. Add a one-line cross-reference to `phase-20-27-game-systems.md` under Phase 22 so that future planning features can trace obligations back to F829.

The routing logic for each obligation:

- **OB-01 (--unit deprecation)**: NOT_FEASIBLE disposition. Trigger condition unchanged from prior chain links: when kojo no longer requires ERB test runner. Documents the N+5 chain (F782->F813->F814->F826->F829).
- **OB-02 (SANTA cosplay text output)**: Routes to Phase 25. CLOTHE_EFFECT.ERB @SANTA PRINT calls require engine-layer UI primitives, which Phase 25 establishes via VisitorAI and EventHandler infrastructure.
- **OB-03 (CLOTHES_ACCESSORY/INtrQuery)**: Routes to Phase 25. INtrQuery dependency is satisfied when the NTR Bounded Context (Phase 24) defines INtrQuery and Phase 25 implements the Anti-Corruption Layer. Trigger: when NTR system migrates (Phase 25).
- **OB-04 (IVariableStore 2D SAVEDATA stubs)**: Routes to Phase 27 (Variable Store infrastructure). GetBirthCountByParent/SetBirthCountByParent require generic 2D SAVEDATA variable accessor on IVariableStore, which is a cross-cutting engine infrastructure concern.
- **OB-05 (NullMultipleBirthService)**: Routes to Phase 27. Runtime implementation requires multi-birth logic (ERB source: PREGNACY_S.ERB multi-birth branches) and OB-04 (2D SAVEDATA) as prerequisite.
- **OB-06 (CVARSET BulkReset extraction)**: Routes to standalone devkit feature (new feature to be created by wbs-generator task). Trigger: when MOVEMENT.ERB migrates (second call site appears), which falls into Phase 26+ scope.
- **OB-07 (IS_DOUTEI extraction)**: Routes to standalone devkit feature (same new feature as OB-06 or separate). Trigger: when second C# call site for IsDoutei appears (22 ERB call sites across 14 files; extraction triggered by second migration).
- **OB-08 (I3DArrayVariables GetDa/SetDa DA gap)**: Routes to Phase 25. ROOM_SMELL_WHOSE_SAMEN is a cross-cutting DA interface gap; Phase 25 expands the visitor/room infrastructure where DA arrays are managed.
- **OB-09 (IEngineVariables indexed stubs)**: Routes to engine repo (cross-repo). GetDay/SetDay/GetTime/SetTime no-op stubs require engine repo implementation. Destination: engine repo IEngineVariables implementation task, tracked under Phase 26 (engine integration phase).
- **OB-10 (CP-2 Step 2c behavioral flow test)**: Routes to Phase 25 sub-feature (first Phase 25 implementation sub-feature). CP-2 Step 2c Shop->Counter->State behavioral flow test requires all Phase 22 services to have non-stub implementations, which Phase 25 completes.
- **OB-11 (Roslynator investigation)**: Routes to standalone devkit infra feature. Phase 22 Task 12 assigned investigation; the investigation itself has zero prerequisites and can be scoped as a dedicated research-type feature.
- **OB-12 (ac-static-verifier numeric parsing)**: Routes to devkit tooling feature. Verify reproduction and scope fix if confirmed. Destination is a concrete devkit infra feature to be created by wbs-generator task.

This design satisfies all 11 ACs:
- AC#1: 12 routing rows in the `## Obligation Routing` table.
- AC#2: No TBD destinations -- all 12 are concrete (Phase ID, Feature ID, or NOT_FEASIBLE).
- AC#3: The `## Obligation Routing` header exists.
- AC#4: OB-01 row contains NOT_FEASIBLE.
- AC#5: `### OB-01` subsection exists with chain and trigger documented.
- AC#6: OB-11 row contains `Phase` destination (standalone infra feature or Phase 27).
- AC#7: `### OB-12` subsection exists with reproduction result documented.
- AC#8: Zero rows target Phase 23 or F827 -- all destinations are Phase 24-27 or standalone features.
- AC#9: At least 4 `Trigger: when` entries (OB-01, OB-03, OB-06, OB-07).
- AC#10: F829 cross-reference added to `phase-20-27-game-systems.md`.
- AC#11: 12 `### OB-` subsection headers exist.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Write `## Obligation Routing` table with exactly 12 rows using `\| OB-01 \|` through `\| OB-12 \|` format |
| 2 | All 12 destination cells contain Phase N, Feature ID, or NOT_FEASIBLE -- no cell contains the text TBD |
| 3 | Write `## Obligation Routing` as a top-level section header |
| 4 | OB-01 row in routing table contains `NOT_FEASIBLE` in the Disposition/Destination column |
| 5 | Write `### OB-01` subsection containing N+5 chain reference and `Trigger: when kojo no longer requires ERB test runner` |
| 6 | Write OB-11 routing row with `Phase` keyword and concrete destination (standalone infra feature scoped to pre-Phase 24) |
| 7 | Write `### OB-12` subsection documenting the reproduction verification result for the ac-static-verifier numeric Expected parsing bug |
| 8 | All routing rows in `## Obligation Routing` table use Phase 24-27 or standalone feature destinations; zero rows reference Phase 23 or F827 |
| 9 | Write `Trigger: when <condition>` lines in OB-01, OB-03, OB-06, and OB-07 subsections (minimum 4 trigger entries) |
| 10 | Add a line containing `F829` to `docs/architecture/migration/phase-20-27-game-systems.md` under Phase 22 Deferred Obligations note |
| 11 | Write `### OB-01` through `### OB-12` subsections (12 total section headers matching `^### OB-` pattern) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| OB-02 (SANTA cosplay) destination | Phase 24, Phase 25, standalone feature | Phase 25 | SANTA PRINT calls require engine-layer UI primitives; Phase 25 is the first phase with VisitorAI/EventHandler infrastructure that can support UI-layer output from clothing effects |
| OB-03 (CLOTHES_ACCESSORY/INtrQuery) destination | Phase 24, Phase 25, standalone feature | Phase 25 | INtrQuery is defined in Phase 24 (NTR Bounded Context) but wired via Anti-Corruption Layer in Phase 25; the Clothing System needs the ACL to inject INtrQuery, making Phase 25 the correct destination |
| OB-04/OB-05 (2D SAVEDATA/NullMultipleBirthService) destination | Phase 25, Phase 26, Phase 27 | Phase 27 | Generic 2D SAVEDATA variable accessor is cross-cutting engine infrastructure with no Phase 25/26 trigger; Phase 27 (the final game systems integration phase) is the appropriate consolidation destination |
| OB-06/OB-07 (BulkReset/IS_DOUTEI extraction) destination | Phase 26, Phase 27, standalone feature | Standalone feature (new) | These are trigger-based shared utility extractions; creating a dedicated standalone feature preserves scope boundaries and avoids embedding ERB-migration-triggered tasks inside a phase planning feature |
| OB-08 (I3DArrayVariables DA gap) destination | Phase 24, Phase 25, Phase 26 | Phase 25 | ROOM_SMELL_WHOSE_SAMEN DA gap arises in room/smell infrastructure; Phase 25 expands visitor+room infrastructure and is the natural fit for the DA interface extension |
| OB-09 (IEngineVariables indexed stubs) destination | Phase 25 engine repo task, Phase 26, standalone | Phase 26 (engine repo) | GetDay/SetDay/GetTime/SetTime require engine repo changes outside devkit/core scope; Phase 26 is the engine integration phase where engine-side implementations are expected |
| OB-10 (CP-2 behavioral flow test) destination | Phase 24 planning feature, Phase 25 sub-feature, standalone | Phase 25 (first implementation sub-feature) | CP-2 Step 2c requires all Phase 22 services to move from Null stubs to real implementations; this precondition is satisfied in Phase 25 when NTR/Visitor systems provide non-stub behavior |
| OB-11 (Roslynator) destination | Phase 22 Task 12 (missed), Phase 23, Phase 24 planning, standalone infra feature | Standalone infra feature | Roslynator investigation has no migration prerequisites; routing to a standalone research/infra feature prevents it from being swallowed by Phase 23/24 scope and ensures it is explicitly tracked |
| OB-12 (ac-static-verifier) routing | Route to standalone tooling feature regardless of bug status | Standalone devkit tooling feature | Whether the bug reproduces or is already fixed, the reproduction result must be documented and a feature must be created to track the fix (or close it as no-op) |
| F829 cross-reference placement in architecture doc | Under Phase 22 Tasks, under Phase 22 Deferred Obligations, as a new section | Under Phase 22 Deferred Obligations block | The obligations originate at the Phase 22 boundary; placing the F829 reference where Phase 22 deferred items are described keeps future readers on the direct tracing path |

### Interfaces / Data Structures

This feature produces no new interfaces or data structures. The deliverable is entirely markdown content.

The `## Obligation Routing` section structure:

```markdown
## Obligation Routing

| ID | Obligation | Source | Destination | Disposition | Notes |
|:--:|------------|--------|-------------|-------------|-------|
| OB-01 | --unit deprecation | F826 (chain: F782->F813->F814->F826) | N+5 chain | NOT_FEASIBLE | Trigger: when kojo no longer requires ERB test runner |
| OB-02 | SANTA cosplay text output | F826 (from F819) | Phase 25 | DEFERRED | UI-layer primitives available in Phase 25 |
| OB-03 | CLOTHES_ACCESSORY/INtrQuery | F826 (from F819) | Phase 25 | DEFERRED | Trigger: when NTR system migrates (Phase 25) |
| OB-04 | IVariableStore 2D SAVEDATA stubs | F826 (from F822) | Phase 27 | DEFERRED | Requires generic 2D SAVEDATA on IVariableStore |
| OB-05 | NullMultipleBirthService | F826 (from F822) | Phase 27 | DEFERRED | Depends on OB-04 (2D SAVEDATA) |
| OB-06 | CVARSET BulkReset extraction | F826 (from F824) | Standalone feature | DEFERRED | Trigger: when MOVEMENT.ERB migrates (second call site) |
| OB-07 | IS_DOUTEI extraction | F826 (from F824) | Standalone feature | DEFERRED | Trigger: when second C# call site appears |
| OB-08 | I3DArrayVariables GetDa/SetDa DA gap | F826 (from F825) | Phase 25 | DEFERRED | Room/smell DA interface gap |
| OB-09 | IEngineVariables indexed stubs | F826 (from F825) | Phase 26 (engine repo) | DEFERRED | Cross-repo; engine-side implementation |
| OB-10 | CP-2 behavioral flow test | F826 | Phase 25 | DEFERRED | Requires non-stub Phase 22 services |
| OB-11 | Roslynator investigation | F826 (from F814/F819) | Standalone infra feature | DEFERRED | Zero prerequisites; standalone research scope |
| OB-12 | ac-static-verifier numeric parsing | F826 | Standalone tooling feature | DEFERRED | Reproduce + fix (or close as no-op) |

### OB-01: --unit Deprecation (NOT_FEASIBLE, N+5)

**Destination**: NOT_FEASIBLE
**Chain**: F782 -> F813 -> F814 -> F826 -> F829 (N+5)
**Trigger**: when kojo no longer requires ERB test runner
**Rationale**: 8 skill files (28 occurrences), 76+ feature files (418+ occurrences), and 100+ kojo JSON test files still reference --unit. The deprecation trigger condition remains unmet. Chain is structurally bounded by ERB-to-C# migration completion. NOT_FEASIBLE disposition is carried forward with the same trigger; do not increment chain label unless F829 is also superseded.

### OB-02: SANTA Cosplay Text Output

**Destination**: Phase 25
**Source**: CLOTHE_EFFECT.ERB @SANTA PRINT calls (F819 deferral)
**Rationale**: UI-layer PRINT output requires engine-layer primitives. Phase 25 establishes VisitorAI/EventHandler infrastructure that can expose display primitives. The clothing cosplay effect can be wired to output at Phase 25 when engine-side output channels are available.

### OB-03: CLOTHES_ACCESSORY / INtrQuery

**Destination**: Phase 25
**Source**: NullNtrQuery returns false for CLOTHES_ACCESSORY (F819 deferral)
**Trigger**: when NTR system migrates (Phase 25)
**Rationale**: INtrQuery is defined in Phase 24 (NTR Bounded Context design) but injected via Anti-Corruption Layer in Phase 25. The Clothing System's dependency on INtrQuery can only be wired once the ACL is operational in Phase 25.

### OB-04: IVariableStore 2D SAVEDATA Stubs

**Destination**: Phase 27
**Source**: GetBirthCountByParent/SetBirthCountByParent return 0/no-op (F822 deferral)
**Rationale**: Requires generic 2D SAVEDATA variable accessor on IVariableStore -- a cross-cutting engine infrastructure concern with no Phase 25/26 trigger. Phase 27 is the final game systems integration phase and the appropriate consolidation point.

### OB-05: NullMultipleBirthService Runtime Implementation

**Destination**: Phase 27
**Source**: 8-method no-op stub; multi-birth logic not migrated (F822 deferral)
**Rationale**: Depends on OB-04 (2D SAVEDATA) as prerequisite and requires multi-birth ERB logic (PREGNACY_S.ERB multi-birth branches) which is in Phase 25+ scope. Phase 27 is the earliest feasible consolidation point after both prerequisites are met.

### OB-06: CVARSET BulkReset Extraction

**Destination**: Standalone devkit feature (trigger-gated)
**Source**: IVariableStore.BulkResetCharacterFlags inlined by F824; extraction needed at second call site (F824 deferral)
**Trigger**: when MOVEMENT.ERB migrates (second C# call site for BulkResetCharacterFlags appears)
**Rationale**: Extraction before the second call site appears creates premature abstraction. A standalone trigger-gated feature ensures extraction happens atomically with the second migration. MOVEMENT.ERB is expected in Phase 26+.

### OB-07: IS_DOUTEI Extraction

**Destination**: Standalone devkit feature (trigger-gated)
**Source**: ICharacterUtilities.IsDoutei inlined by F824; 22 call sites across 14 ERB files (F824 deferral)
**Trigger**: when second C# call site for IsDoutei appears
**Rationale**: Same trigger-gated extraction pattern as OB-06. 22 ERB call sites means many future migration features will encounter this; standalone feature isolates the extraction concern.

### OB-08: I3DArrayVariables GetDa/SetDa DA Gap

**Destination**: Phase 25
**Source**: ROOM_SMELL_WHOSE_SAMEN cross-cutting DA interface gap (F825 deferral)
**Rationale**: DA array interface gaps in room/smell infrastructure align with Phase 25's visitor+room expansion scope. The I3DArrayVariables interface extension can be scoped as a sub-task of the Phase 25 planning feature.

### OB-09: IEngineVariables Indexed Stubs

**Destination**: Phase 26 (engine repo)
**Source**: GetDay/SetDay/GetTime/SetTime no-op default interface methods (F825 deferral)
**Rationale**: Engine repo changes cannot be resolved within devkit or core repos. Phase 26 is the engine integration phase. Cross-repo coordination tracked here; engine repo task to be created during Phase 26 planning.

### OB-10: CP-2 Step 2c Behavioral Flow Test

**Destination**: Phase 25 (first implementation sub-feature)
**Source**: Shop->Counter->State behavioral flow; DI resolution only, behavioral flow deferred (F826 deferral)
**Rationale**: CP-2 Step 2c requires Phase 22 services to move from Null stubs to real implementations. This precondition is met when Phase 25 NTR/Visitor services replace the Null stubs currently used in integration tests.

### OB-11: Roslynator Analyzers Investigation

**Destination**: Standalone devkit infra feature (research type)
**Source**: Phase 22 Task 12 (assigned to F819 by F814; F819 declared out of scope; zero investigation performed)
**Rationale**: Roslynator investigation has no migration prerequisites and can proceed independently at any time. Routing to a standalone research/infra feature ensures it is explicitly tracked and cannot be swallowed by Phase 23/24 scope creep.

### OB-12: ac-static-verifier Numeric Expected Parsing

**Destination**: Standalone devkit tooling feature
**Source**: ac-static-verifier.py cannot parse plain numeric Expected (e.g., `37`) when Method uses Grep format (F826 deferral)
**Rationale**: Reproduction verification is needed before scoping the fix. A standalone tooling feature will: (1) attempt to reproduce the failure case, (2) document the result, (3) scope fix if confirmed (with regression test), or close as no-op if already fixed.
```

### Upstream Issues

<!-- No upstream issues found during Technical Design.
     AC patterns and expected values are consistent with the routing content designed above.
     OB-11 destination uses "standalone infra feature" phrasing; AC#6 pattern ^\\| OB-11.*Phase [0-9]+ will
     not match unless the routing table row for OB-11 includes a Phase reference. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#6 pattern `^\\| OB-11.*Phase [0-9]+` requires the OB-11 routing row to contain a Phase number, but OB-11 destination is a standalone feature not assigned to a specific Phase. The row content as designed would be "Standalone infra feature" with no Phase N substring. | AC Definition Table, AC#6 | Revise OB-11 destination to include a Phase qualifier (e.g., "Standalone infra feature (pre-Phase 24)") so the row text contains `Phase 24`; or update AC#6 pattern to allow feature-ID matches as well as Phase matches. Recommended: add `(pre-Phase 24)` parenthetical to OB-11 destination in the routing table row so the pattern `Phase [0-9]+` matches without changing the AC. |

---

## Tasks

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement -> Write test -> Verify |

---

## Implementation Contract

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

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A->B->A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} -> `{target}` or -- {reason} -->

---

## Links
- [Predecessor: F826](feature-826.md) - Post-Phase Review Phase 22 (discovery origin)
- [Related: F827](feature-827.md) - Phase 23 Planning
- [Related: F819](feature-819.md) - Clothing System
- [Related: F822](feature-822.md) - Pregnancy System
- [Related: F824](feature-824.md) - Sleep & Menstrual
- [Related: F825](feature-825.md) - Relationships & DI Integration
- [Related: F814](feature-814.md) - Phase 22 Planning
- [Related: F813](feature-813.md) - --unit deprecation chain
- [Related: F782](feature-782.md) - --unit deprecation origin
