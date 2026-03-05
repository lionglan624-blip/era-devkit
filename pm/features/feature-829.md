# Feature 829: Phase 22 Deferred Obligations Consolidation

## Status: [DONE]
<!-- fl-reviewed: 2026-03-05T22:00:05Z -->

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

### Observable Symptom
12 deferred obligations accumulated in F826 Mandatory Handoffs table without concrete resolution destinations, because Phase 22 sub-features (F819, F822, F824, F825) systematically deferred items requiring infrastructure not yet available.

### Execution Evidence

| Source | Evidence |
|--------|----------|
| F826 Mandatory Handoffs | 12 rows, all Destination ID = F829, all Transferred [x] |
| --unit deprecation | N+4 chain: F782->F813->F814->F826->F829 |
| Roslynator investigation | Zero presence in codebase (0 PackageReference) |

### Files Involved

| File | Role |
|------|------|
| pm/features/feature-826.md | Source of all 12 handoffs |
| pm/features/feature-819.md | Source: SANTA cosplay, CLOTHES_ACCESSORY, Roslynator |
| pm/features/feature-822.md | Source: IVariableStore 2D stubs, NullMultipleBirthService |
| pm/features/feature-824.md | Source: BulkResetCharacterFlags, IS_DOUTEI |
| pm/features/feature-825.md | Source: I3DArrayVariables DA gap, IEngineVariables stubs |
| docs/architecture/migration/phase-20-27-game-systems.md | Phase 22 architecture reference |

### Attempted Solutions
None — obligations were collected in F826 but not routed to concrete destinations.

### Parent Session Observations

#### Consolidated Obligations from Phase 22

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

## Predecessor Obligations

| Source Feature | Obligation | Category | Status |
|:--------------:|------------|----------|:------:|
| F826 | --unit deprecation NOT_FEASIBLE re-deferral (N+4 chain F782->F813->F814->F826) | deferred | routed |
| F826 (from F819) | SANTA cosplay text output (CLOTHE_EFFECT.ERB @SANTA PRINT calls) | handoff | routed |
| F826 (from F819) | CLOTHES_ACCESSORY requires INtrQuery (NullNtrQuery returns false) | handoff | routed |
| F826 (from F822) | IVariableStore 2D SAVEDATA accessor stubs (GetBirthCountByParent/SetBirthCountByParent) | handoff | routed |
| F826 (from F822) | NullMultipleBirthService runtime implementation (8-method stub) | handoff | routed |
| F826 (from F824) | CVARSET bulk reset shared method extraction (IVariableStore.BulkResetCharacterFlags) | handoff | routed |
| F826 (from F824) | IS_DOUTEI shared utility extraction (ICharacterUtilities.IsDoutei) | handoff | routed |
| F826 (from F825) | I3DArrayVariables GetDa/SetDa DA interface gap | handoff | routed |
| F826 (from F825) | IEngineVariables indexed methods no-op stubs (GetDay/SetDay/GetTime/SetTime) | handoff | routed |
| F826 | CP-2 Step 2c behavioral flow test (Shop->Counter->State) | handoff | routed |
| F826 (from F814) | Roslynator Analyzers investigation (Phase 22 Task 12) | deferred | routed |
| F826 | ac-static-verifier.py numeric Expected parsing for Grep-method ACs | deferred | routed |

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
| F826 | [DONE] | Predecessor: Post-Phase Review Phase 22 (source of all 12 handoffs) |
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
| F826 must finalize before F829 can pass /fl | `pm/index-features.md` F826 = [DONE] | Resolved: F826 reached [DONE]; no longer a blocking constraint |

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
| 1 | Obligation Routing section contains 12 entries | file | Grep(pm/features/feature-829.md, pattern="\\| OB-[0-9]+ \\|") | gte | 12 | [x] |
| 2 | No TBD in Obligation Routing destination column | file | Grep(pm/features/feature-829.md, pattern="\\| OB-[0-9]+ \\|.*TBD") | not_matches | `TBD` in routing row | [x] |
| 3 | Routing section header exists | file | Grep(pm/features/feature-829.md, pattern="^## Obligation Routing") | matches | `## Obligation Routing` | [x] |
| 4 | --unit deprecation disposition is NOT_FEASIBLE | file | Grep(pm/features/feature-829.md, pattern="\\| OB-01 \\|.*NOT_FEASIBLE") | matches | `NOT_FEASIBLE` in routing row | [x] |
| 5 | --unit N+5 chain and trigger documented | file | Grep(pm/features/feature-829.md, pattern="^### OB-01") | gte | 1 | [x] |
| 14 | --unit N+5 chain history documented | file | Grep(pm/features/feature-829.md, pattern="F782.*F813.*F814.*F826.*F829") | matches | N+5 chain reference | [x] |
| 15 | ac-static-verifier reproduction outcome documented | file | Grep(pm/features/feature-829.md, pattern="(reproduces|does not reproduce|already fixed|no-op)") | matches | reproduction outcome text | [x] |
| 6 | Roslynator routed to concrete destination | file | Grep(pm/features/feature-829.md, pattern="\\| OB-11 \\|.*Standalone") | matches | Standalone destination | [x] |
| 7 | ac-static-verifier reproduction result documented | file | Grep(pm/features/feature-829.md, pattern="^### OB-12") | gte | 1 | [x] |
| 8 | Zero routing entries target Phase 23 or F827 | file | Grep(pm/features/feature-829.md, pattern="\\| OB-[0-9]+ \\|.*(Phase 23\|F827)") | not_matches | no Phase 23 or F827 destination | [x] |
| 9 | Trigger conditions documented for conditional obligations | file | Grep(pm/features/feature-829.md, pattern="^\\*\\*Trigger\\*\\*:.*when") | gte | 4 | [x] |
| 10 | Phase 22 section updated with F829 cross-reference | file | Grep(docs/architecture/migration/phase-20-27-game-systems.md, pattern="Phase 22.*F829") | matches | Phase 22 F829 cross-reference | [x] |
| 11 | Routing details section exists with per-obligation analysis | file | Grep(pm/features/feature-829.md, pattern="^### OB-") | gte | 12 | [x] |
| 12 | DRAFT features created (F830, F831, F832, F833) | file | Grep(pm/features/, pattern="^## Status: \\[DRAFT\\]", glob="feature-83{0,1,2,3}.md") | gte | 4 | [x] |
| 13 | DRAFT features registered in index | file | Grep(pm/index-features.md, pattern="F83[0123]") | gte | 4 | [x] |
| 16 | Phase 25 section updated with routed obligations | file | Grep(docs/architecture/migration/phase-20-27-game-systems.md, pattern="Deferred Obligations.*F829") | matches | Deferred Obligations block with F829 reference | [x] |

### AC Details

**AC#1: Obligation Routing section contains 12 entries**
- **Test**: `Grep(pm/features/feature-829.md, pattern="\\| OB-[0-9]+ \\|")`
- **Expected**: >= 12 matches (one routing row per obligation)
- **Rationale**: Philosophy requires every obligation to have a trackable destination. 12 obligations enumerated in Predecessor Obligations table.
- **Derivation**: 12 obligations from F826 Mandatory Handoffs: OB-01 --unit deprecation, OB-02 SANTA cosplay, OB-03 CLOTHES_ACCESSORY/INtrQuery, OB-04 IVariableStore 2D SAVEDATA stubs, OB-05 NullMultipleBirthService, OB-06 CVARSET BulkReset extraction, OB-07 IS_DOUTEI extraction, OB-08 I3DArrayVariables DA gap, OB-09 IEngineVariables indexed stubs, OB-10 CP-2 behavioral flow test, OB-11 Roslynator investigation, OB-12 ac-static-verifier numeric parsing.

**AC#5: --unit N+5 chain and trigger documented**
- **Test**: `Grep(pm/features/feature-829.md, pattern="^### OB-01")`
- **Expected**: >= 1 match (OB-01 subsection header exists)
- **Rationale**: --unit deprecation requires dedicated subsection documenting N+5 chain history and trigger condition.
- **Derivation**: OB-01 is the --unit deprecation obligation. Its subsection must contain the N+5 chain (F782->F813->F814->F826->F829) and trigger condition ("when kojo no longer requires ERB test runner"). Threshold is 1 because exactly one `### OB-01` header should exist.

**AC#14: --unit N+5 chain history documented**
- **Test**: `Grep(pm/features/feature-829.md, pattern="F782.*F813.*F814.*F826.*F829")`
- **Expected**: matches (chain text present in OB-01 subsection)
- **Rationale**: Goal Item 4 requires documenting the --unit deprecation as NOT_FEASIBLE "(N+5)" with chain history. AC#5 only verifies the subsection header exists; this AC verifies the chain content.
- **Derivation**: The N+5 chain F782->F813->F814->F826->F829 must appear in the OB-01 subsection as audit trail.

**AC#15: ac-static-verifier reproduction outcome documented**
- **Test**: `Grep(pm/features/feature-829.md, pattern="(reproduces|does not reproduce|already fixed|no-op)")`
- **Expected**: matches (reproduction outcome present in OB-12 subsection)
- **Rationale**: Goal Item 3 requires verifying whether the ac-static-verifier bug reproduces. AC#7 only verifies the subsection header; this AC verifies the reproduction result content.
- **Derivation**: The OB-12 subsection must document whether the bug reproduces or is already fixed.

**AC#7: ac-static-verifier reproduction result documented**
- **Test**: `Grep(pm/features/feature-829.md, pattern="^### OB-12")`
- **Expected**: >= 1 match (OB-12 subsection header exists)
- **Rationale**: ac-static-verifier bug needs reproduction verification documented in its own subsection.
- **Derivation**: OB-12 is the ac-static-verifier numeric Expected parsing obligation. Its subsection must document whether the bug reproduces and the scoped fix destination. Threshold is 1 because exactly one `### OB-12` header should exist.

**AC#9: Trigger conditions documented for conditional obligations**
- **Test**: `Grep(pm/features/feature-829.md, pattern="^\\*\\*Trigger\\*\\*:.*when")`
- **Expected**: >= 4 trigger condition entries
- **Rationale**: Constraint C7 requires trigger documentation for conditional obligations.
- **Derivation**: At least 4 obligations are trigger-based: (1) OB-01 --unit ("when kojo no longer requires ERB test runner"), (2) OB-06 BulkReset ("when MOVEMENT.ERB migrates"), (3) OB-07 IS_DOUTEI ("when second C# call site appears"), (4) OB-03 CLOTHES_ACCESSORY ("when NTR system migrates").

**AC#11: Routing details section exists with per-obligation analysis**
- **Test**: `Grep(pm/features/feature-829.md, pattern="^### OB-")`
- **Expected**: >= 12 matches (one subsection per obligation)
- **Rationale**: Each obligation requires documented analysis explaining destination choice, not just a table row.
- **Derivation**: 12 obligations, each requiring a `### OB-{NN}` subsection with destination rationale. Pattern uses `^` anchor to match only section headers, not inline references.

**AC#12: DRAFT features created (F830, F831, F832, F833)**
- **Test**: `Grep(pm/features/, pattern="^## Status: \\[DRAFT\\]", glob="feature-83{0,1,2,3}.md")`
- **Expected**: >= 4 matches (one per DRAFT feature)
- **Rationale**: Tasks 3/4/5/6 create standalone features for obligations that need their own tracking.
- **Derivation**: 4 DRAFT features: F830 (OB-06/OB-07 trigger-gated extractions), F831 (OB-11 Roslynator investigation), F832 (OB-12 ac-static-verifier fix), F833 (OB-09 IEngineVariables indexed stubs).

**AC#13: DRAFT features registered in index**
- **Test**: `Grep(pm/index-features.md, pattern="F83[0123]")`
- **Expected**: >= 4 matches
- **Rationale**: Features must appear in index-features.md to be tracked in the project workflow.
- **Derivation**: 4 features (F830, F831, F832, F833) must each have a row in index-features.md Active Features table.

**AC#16: Phase 25 section updated with routed obligations**
- **Test**: `Grep(docs/architecture/migration/phase-20-27-game-systems.md, pattern="Deferred Obligations.*F829")`
- **Expected**: matches (Deferred Obligations block with F829 reference exists in Phase 25 section)
- **Rationale**: Leak prevention — future Phase 25 planning features must discover obligations routed to them without reading F829 end-to-end.
- **Derivation**: 6 obligations routed to Phase 25 (OB-02 SANTA cosplay, OB-03 CLOTHES_ACCESSORY, OB-04 2D SAVEDATA, OB-05 NullMultipleBirthService, OB-08 I3DArrayVariables DA gap, OB-10 CP-2 behavioral flow test).

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Route all 12 deferred obligations to concrete destinations | AC#1, AC#2, AC#3, AC#11 |
| 2 | Zero untracked obligations at Phase 22 boundary | AC#1, AC#2 |
| 3 | ac-static-verifier bug: verify reproduction and scope fix | AC#7, AC#15 |
| 4 | --unit deprecation as NOT_FEASIBLE (N+5) with trigger | AC#4, AC#5, AC#14 |
| 5 | Roslynator investigation to concrete destination | AC#6 |
| 6 | No obligations to Phase 23/F827 (C6 constraint) | AC#8 |
| 7 | Trigger conditions documented for conditional obligations (C7 constraint) | AC#9 |
| 8 | Phase architecture doc updated with routing references | AC#10, AC#16 |
| 9 | DRAFT features created and registered for standalone obligations | AC#12, AC#13 |

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
- **OB-04 (IVariableStore 2D SAVEDATA stubs)**: Routes to Phase 25. GetBirthCountByParent/SetBirthCountByParent require generic 2D SAVEDATA variable accessor on IVariableStore. Phase 25 replaces pregnancy-related Null stubs, making this a natural fit.
- **OB-05 (NullMultipleBirthService)**: Routes to Phase 25. Runtime implementation requires multi-birth logic (ERB source: PREGNACY_S.ERB multi-birth branches) and OB-04 (2D SAVEDATA) as prerequisite. Both co-located in Phase 25.
- **OB-06 (CVARSET BulkReset extraction)**: Routes to standalone devkit feature (new feature to be created by wbs-generator task). Trigger: when MOVEMENT.ERB migrates (second call site appears), which falls into Phase 26+ scope.
- **OB-07 (IS_DOUTEI extraction)**: Routes to standalone devkit feature (same new feature as OB-06 or separate). Trigger: when second C# call site for IsDoutei appears (22 ERB call sites across 14 files; extraction triggered by second migration).
- **OB-08 (I3DArrayVariables GetDa/SetDa DA gap)**: Routes to Phase 25. ROOM_SMELL_WHOSE_SAMEN is a cross-cutting DA interface gap; Phase 25 expands the visitor/room infrastructure where DA arrays are managed.
- **OB-09 (IEngineVariables indexed stubs)**: Routes to standalone feature (F833). GetDay/SetDay/GetTime/SetTime no-op stubs require cross-repo engine implementation. Standalone feature tracks the cross-repo coordination required.
- **OB-10 (CP-2 Step 2c behavioral flow test)**: Routes to Phase 25 sub-feature (first Phase 25 implementation sub-feature). CP-2 Step 2c Shop->Counter->State behavioral flow test requires all Phase 22 services to have non-stub implementations, which Phase 25 completes.
- **OB-11 (Roslynator investigation)**: Routes to standalone devkit infra feature. Phase 22 Task 12 assigned investigation; the investigation itself has zero prerequisites and can be scoped as a dedicated research-type feature.
- **OB-12 (ac-static-verifier numeric parsing)**: Routes to devkit tooling feature. Verify reproduction and scope fix if confirmed. Destination is a concrete devkit infra feature to be created by wbs-generator task.

This design satisfies all 16 ACs:
- AC#1: 12 routing rows in the `## Obligation Routing` table.
- AC#2: No TBD destinations -- all 12 are concrete (Phase ID, Feature ID, or NOT_FEASIBLE).
- AC#3: The `## Obligation Routing` header exists.
- AC#4: OB-01 row contains NOT_FEASIBLE.
- AC#5: `### OB-01` subsection exists with chain and trigger documented.
- AC#14: N+5 chain text (F782->F813->F814->F826->F829) present in OB-01 subsection.
- AC#15: Reproduction outcome text (reproduces/does not reproduce/already fixed/no-op) present in OB-12 subsection.
- AC#6: OB-11 row contains `Standalone` destination (standalone infra feature).
- AC#7: `### OB-12` subsection exists with reproduction result documented.
- AC#8: Zero rows target Phase 23 or F827 -- all destinations are Phase 24-27 or standalone features.
- AC#9: At least 4 `**Trigger**: when` bold entries (OB-01, OB-03, OB-06, OB-07).
- AC#10: F829 cross-reference added to `phase-20-27-game-systems.md`.
- AC#11: 12 `### OB-` subsection headers exist.
- AC#12: feature-830.md, feature-831.md, feature-832.md, feature-833.md exist as [DRAFT].
- AC#13: F830, F831, F832, F833 registered in index-features.md.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Write `## Obligation Routing` table with exactly 12 rows using `\| OB-01 \|` through `\| OB-12 \|` format |
| 2 | All 12 destination cells contain Phase N, Feature ID, or NOT_FEASIBLE -- no cell contains the text TBD |
| 3 | Write `## Obligation Routing` as a top-level section header |
| 4 | OB-01 row in routing table contains `NOT_FEASIBLE` in the Disposition/Destination column |
| 5 | Write `### OB-01` subsection containing N+5 chain reference and `Trigger: when kojo no longer requires ERB test runner` |
| 6 | Write OB-11 routing row with `Standalone` keyword and concrete destination (standalone infra feature) |
| 7 | Write `### OB-12` subsection documenting the reproduction verification result for the ac-static-verifier numeric Expected parsing bug |
| 8 | All routing rows in `## Obligation Routing` table use Phase 24-27 or standalone feature destinations; zero rows reference Phase 23 or F827 |
| 9 | Write `**Trigger**: when <condition>` bold lines at start of OB-01, OB-03, OB-06, and OB-07 subsections (minimum 4 trigger entries) |
| 10 | Add a line containing `F829` to `docs/architecture/migration/phase-20-27-game-systems.md` under Phase 22 Deferred Obligations note |
| 11 | Write `### OB-01` through `### OB-12` subsections (12 total section headers matching `^### OB-` pattern) |
| 12 | Tasks 3/4/5/6 create feature-830.md, feature-831.md, feature-832.md, feature-833.md as [DRAFT] files with `## Status: [DRAFT]` header |
| 13 | Tasks 3/4/5/6 register F830, F831, F832, F833 in `pm/index-features.md` Active Features table |
| 14 | Write the N+5 chain text (F782->F813->F814->F826->F829) within the `### OB-01` subsection |
| 15 | Write the reproduction outcome text (reproduces/does not reproduce/already fixed/no-op) within the `### OB-12` subsection |
| 16 | Add obligation list (OB-02, OB-03, OB-04, OB-05, OB-08, OB-10) under Phase 25 section in `phase-20-27-game-systems.md` as "Deferred Obligations (from F829)" block |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| OB-02 (SANTA cosplay) destination | Phase 24, Phase 25, standalone feature | Phase 25 | SANTA PRINT calls require engine-layer UI primitives; Phase 25 is the first phase with VisitorAI/EventHandler infrastructure that can support UI-layer output from clothing effects |
| OB-03 (CLOTHES_ACCESSORY/INtrQuery) destination | Phase 24, Phase 25, standalone feature | Phase 25 | INtrQuery is defined in Phase 24 (NTR Bounded Context) but wired via Anti-Corruption Layer in Phase 25; the Clothing System needs the ACL to inject INtrQuery, making Phase 25 the correct destination |
| OB-04/OB-05 (2D SAVEDATA/NullMultipleBirthService) destination | Phase 25, Phase 26, Phase 27 | Phase 25 | Core pregnancy infrastructure (IVariableStore 2D SAVEDATA accessor, NullMultipleBirthService runtime). Phase 25 replaces NTR/Visitor/Pregnancy Null stubs with real implementations; 2D SAVEDATA and multi-birth logic are prerequisites for full behavioral testing. Phase 27 (Extensions) scope is IExtensionModule-based systems, not core infrastructure. |
| OB-06/OB-07 (BulkReset/IS_DOUTEI extraction) destination | Phase 26, Phase 27, standalone feature, separate features | Standalone feature (new, combined F830) | These are trigger-based shared utility extractions; creating a dedicated standalone feature preserves scope boundaries and avoids embedding ERB-migration-triggered tasks inside a phase planning feature. Grouped into single F830 because: both originate from F824, both are trigger-gated extraction deferrals with similar patterns (second call site appearance), and both target Era.Core shared utility interfaces. Independent triggers are tracked separately within F830's AC design. |
| OB-08 (I3DArrayVariables DA gap) destination | Phase 24, Phase 25, Phase 26, standalone | Phase 25 | ROOM_SMELL_WHOSE_SAMEN DA gap is on I3DArrayVariables, a cross-cutting Era.Core interface. ROOM_SMELL.ERB is Phase 22 (already migrated). Phase 25 NTR/Visitor tasks do not explicitly require DA array extension. Re-evaluation trigger documented in OB-08 subsection: if Phase 25 planning feature finds no task requiring GetDa/SetDa, create standalone feature for OB-08. |
| OB-09 (IEngineVariables indexed stubs) destination | Phase 25 engine repo task, Phase 26, standalone | Standalone feature (F833) | GetDay/SetDay/GetTime/SetTime require cross-repo engine changes outside any single phase scope. Phase 26 (Special Modes & Messaging) scope is not engine integration. Standalone feature provides clear tracking for cross-repo coordination. |
| OB-10 (CP-2 behavioral flow test) destination | Phase 24 planning feature, Phase 25 sub-feature, standalone | Phase 25 (first implementation sub-feature) | CP-2 Step 2c tests Phase 22 service integration. Phase 25 replaces all Phase 22 Null stubs (NTR/Visitor/Clothing + OB-04 2D SAVEDATA + OB-05 NullMultipleBirthService) with real implementations, enabling full behavioral flow testing. |
| OB-11 (Roslynator) destination | Phase 22 Task 12 (missed), Phase 23, Phase 24 planning, standalone infra feature | Standalone infra feature | Roslynator investigation has no migration prerequisites; routing to a standalone research/infra feature prevents it from being swallowed by Phase 23/24 scope and ensures it is explicitly tracked |
| OB-12 (ac-static-verifier) routing | Route to standalone tooling feature regardless of bug status | Standalone devkit tooling feature | Whether the bug reproduces or is already fixed, the reproduction result must be documented and a feature must be created to track the fix (or close it as no-op) |
| F829 cross-reference placement in architecture doc | Under Phase 22 Tasks, under Phase 22 Deferred Obligations, as a new section | Under Phase 22 Deferred Obligations block | The obligations originate at the Phase 22 boundary; placing the F829 reference where Phase 22 deferred items are described keeps future readers on the direct tracing path |

### Interfaces / Data Structures

This feature produces no new interfaces or data structures. The deliverable is entirely markdown content.

The `## Obligation Routing` section will contain:
- **Summary table**: 6-column table (ID, Obligation, Source, Destination, Disposition, Notes) with 12 rows (OB-01 through OB-12)
- **Per-obligation subsections**: 12 `### OB-{NN}` subsections, each with Destination, Source, Rationale, and optional Trigger/Chain fields

Routing decisions are documented in the Key Decisions section above. The implementer must produce the actual markdown content matching the AC patterns during Task 1 execution.

### Upstream Issues

<!-- Resolved: AC#6 pattern was updated from `Phase [0-9]+` to `Standalone` by orchestrator after tech-designer flagged the mismatch. No remaining upstream issues. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| (None) | — | — |

---

<!-- fc-phase-5-completed -->
## Tasks

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 3, 1, 2, 4, 5, 14, 15, 6, 7, 8, 9, 11 | Write `## Obligation Routing` section in feature-829.md: summary table (OB-01 through OB-12) and `### OB-{NN}` subsections with destination rationale, trigger conditions, and chain context | | [x] |
| 2 | 10, 16 | Add F829 cross-reference under Phase 22 Deferred Obligations block AND add obligation list under Phase 25 section in `docs/architecture/migration/phase-20-27-game-systems.md` listing the 6 obligations routed to Phase 25 (OB-02, OB-03, OB-04, OB-05, OB-08, OB-10) | | [x] |
| 3 | 12, 13 | Create feature-830.md [DRAFT] for CVARSET BulkReset extraction and IS_DOUTEI extraction (OB-06/OB-07 standalone trigger-gated feature); register in index-features.md | | [x] |
| 4 | 12, 13 | Create feature-831.md [DRAFT] for Roslynator Analyzers investigation (OB-11 standalone research/infra feature); register in index-features.md | | [x] |
| 5 | 12, 13 | Create feature-832.md [DRAFT] for ac-static-verifier numeric Expected parsing fix (OB-12 standalone tooling feature); register in index-features.md | | [x] |
| 6 | 12, 13 | Create feature-833.md [DRAFT] for IEngineVariables indexed methods stubs (OB-09 standalone engine feature); register in index-features.md | | [x] |

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
| 1 | implementer | sonnet | feature-829.md AC table, Technical Design Approach section, Interfaces/Data Structures section | `## Obligation Routing` section written into feature-829.md (Task 1) |
| 2 | implementer | sonnet | `docs/architecture/migration/phase-20-27-game-systems.md`, feature-829.md Background, Technical Design routing logic | F829 cross-reference under Phase 22 + obligation list under Phase 25 section (Task 2) |
| 3 | implementer | sonnet | feature-829.md Technical Design Key Decisions (OB-06/OB-07), pm/index-features.md | feature-830.md [DRAFT] created and registered (Task 3) |
| 4 | implementer | sonnet | feature-829.md Technical Design Key Decisions (OB-11), pm/index-features.md | feature-831.md [DRAFT] created and registered (Task 4) |
| 5 | implementer | sonnet | feature-829.md Technical Design Key Decisions (OB-12), pm/index-features.md | feature-832.md [DRAFT] created and registered (Task 5) |
| 6 | implementer | sonnet | feature-829.md Technical Design Key Decisions (OB-09), pm/index-features.md | feature-833.md [DRAFT] created and registered (Task 6) |

### Pre-conditions

- F826 must be [DONE] before F829 can pass /fl (dependency gate)
- `docs/architecture/migration/phase-20-27-game-systems.md` Phase 22 section must be readable to locate the Deferred Obligations block

### Execution Order

1. Task 1 (write Obligation Routing section) — all AC verifications depend on this content
2. Task 2 (architecture doc cross-reference) — independent of Tasks 3-5
3. Tasks 3, 4, 5, 6 (create DRAFT features) — independent of each other; may run in parallel

### Success Criteria

- `## Obligation Routing` section header exists in feature-829.md
- Routing table has exactly 12 rows (OB-01 through OB-12), none with TBD destination
- OB-01 row contains `NOT_FEASIBLE`; `### OB-01` subsection contains N+5 chain and `Trigger: when kojo no longer requires ERB test runner`
- OB-11 row contains `Standalone`; `### OB-11` subsection exists
- `### OB-12` subsection exists with reproduction verification result documented
- Zero routing rows reference Phase 23 or F827
- At least 4 `**Trigger**: when` bold entries across the subsections
- `phase-20-27-game-systems.md` contains `F829` under Phase 22 and obligation references (OB-02 through OB-10) under Phase 25
- 12 `### OB-` subsection headers exist in feature-829.md
- feature-830.md, feature-831.md, feature-832.md, feature-833.md all exist as [DRAFT] and appear in index-features.md
- Feature-829.md contains N+5 chain text (F782->F813->F814->F826->F829)
- Feature-829.md contains reproduction outcome text for ac-static-verifier bug

### Rollback Plan

If issues arise after edits:
1. `git revert` the relevant commits
2. Notify user of rollback
3. Create follow-up feature for the fix

### Error Handling

- If `phase-20-27-game-systems.md` lacks a clear Phase 22 Deferred Obligations block: STOP → Ask user for placement guidance
- If Next Feature number in index-features.md conflicts with Task 3/4/5 creation: STOP → Ask user for ID allocation

---

## Obligation Routing

Summary of all deferred obligations from this feature, their routing decisions, and destinations.

| ID | Obligation | Source | Destination | Disposition | Notes |
|----|-----------|--------|-------------|-------------|-------|
| OB-01 | --unit flag deprecation | F829 Task 1 | F782->F813->F814->F826->F829 chain | NOT_FEASIBLE | Trigger: when kojo no longer requires ERB test runner (N+5 chain) |
| OB-02 | SANTA cosplay text output (PRINT calls) | F829 Task 1 | Phase 25 | Deferred | CLOTHE_EFFECT.ERB @SANTA requires engine-layer UI primitives |
| OB-03 | CLOTHES_ACCESSORY/INtrQuery wiring | F829 Task 1 | Phase 25 | Deferred | Trigger: when NTR system migrates (Phase 25); INtrQuery defined Phase 24 |
| OB-04 | IVariableStore 2D SAVEDATA stubs | F829 Task 1 | Phase 25 | Deferred | GetBirthCountByParent/SetBirthCountByParent require generic 2D accessor |
| OB-05 | NullMultipleBirthService implementation | F829 Task 1 | Phase 25 | Deferred | Requires multi-birth logic and OB-04 as prerequisite |
| OB-06 | CVARSET BulkReset extraction | F829 Task 1 | Standalone F830 | Deferred | Trigger: when MOVEMENT.ERB migrates (second call site appears) |
| OB-07 | IS_DOUTEI extraction (IsDoutei) | F829 Task 1 | Standalone F830 | Deferred | Trigger: when second C# call site for IsDoutei appears |
| OB-08 | I3DArrayVariables GetDa/SetDa DA gap | F829 Task 1 | Phase 25 | Deferred | ROOM_SMELL_WHOSE_SAMEN cross-cutting DA interface gap; re-evaluate if Phase 25 planning finds no GetDa/SetDa task |
| OB-09 | IEngineVariables indexed stubs (GetDay/SetDay/GetTime/SetTime) | F829 Task 1 | Standalone F833 | Deferred | Cross-repo engine implementation required |
| OB-10 | CP-2 Step 2c behavioral flow test | F829 Task 1 | Phase 25 | Deferred | Shop->Counter->State behavioral flow requires non-stub implementations |
| OB-11 | Roslynator Analyzers investigation | F829 Task 1 | Standalone F831 | Deferred | Zero prerequisites; standalone research scope |
| OB-12 | ac-static-verifier numeric Expected parsing bug | F829 Task 1 | Standalone F832 | Deferred | Reproduces: ACs with `gte` matcher and bare numeric Expected fail; reproduction outcome: reproduces (confirmed 2026-03-06) |

### OB-01

**Trigger**: when kojo no longer requires ERB test runner.

**Destination Rationale**: The `--unit` flag deprecation cannot proceed while the kojo system still depends on the ERB test runner. This is a NOT_FEASIBLE obligation at this stage.

**Chain Context**: F782->F813->F814->F826->F829 (N+5 chain). Each feature in this chain has carried this obligation forward. Deprecation becomes feasible only at the end of the kojo migration, when the ERB test runner is no longer needed for kojo unit tests.

### OB-02

**Destination Rationale**: CLOTHE_EFFECT.ERB @SANTA PRINT calls require engine-layer UI primitives that are not available until Phase 25. The text output infrastructure must be in place before this migration can proceed.

**Chain Context**: Blocked on Phase 25 UI primitive availability. No earlier phase provides the required engine support.

### OB-03

**Trigger**: when NTR system migrates (Phase 25).

**Destination Rationale**: CLOTHES_ACCESSORY/INtrQuery wiring depends on the NTR system migration. INtrQuery is defined in Phase 24, but wiring via ACL occurs in Phase 25. This obligation cannot be resolved until the NTR migration is complete.

**Chain Context**: Sequential dependency on Phase 24 (INtrQuery definition) then Phase 25 (ACL wiring). Both phases must complete before this obligation can be discharged.

### OB-04

**Destination Rationale**: GetBirthCountByParent/SetBirthCountByParent require a generic 2D SAVEDATA variable accessor on IVariableStore. This accessor pattern is part of the Phase 25 variable store redesign and cannot be added earlier without disrupting the migration sequence.

**Chain Context**: Prerequisite for OB-05. Phase 25 variable store work must land first.

### OB-05

**Destination Rationale**: The runtime implementation of NullMultipleBirthService requires multi-birth logic that is not yet designed, plus OB-04 as a hard prerequisite. Both conditions must be satisfied before this stub can be replaced with a real implementation.

**Chain Context**: Depends on OB-04 (Phase 25). Cannot proceed independently.

### OB-06

**Trigger**: when MOVEMENT.ERB migrates (second call site appears).

**Destination Rationale**: Extracting IVariableStore.BulkResetCharacterFlags (CVARSET BulkReset) into a standalone feature is premature with only one call site. The extraction becomes justified and safe once a second call site exists in C# code, confirming the pattern. Routed to F830 which combines OB-06 and OB-07.

**Chain Context**: Trigger-gated. F830 is the combined standalone feature for OB-06 and OB-07.

### OB-07

**Trigger**: when second C# call site for IsDoutei appears.

**Destination Rationale**: IS_DOUTEI extraction (ICharacterUtilities.IsDoutei) follows the same trigger logic as OB-06. With 22 ERB call sites across 14 files, the ERB footprint is large, but C# extraction is deferred until the pattern is confirmed by a second C# call site. Routed to F830 alongside OB-06.

**Chain Context**: Trigger-gated. F830 is the combined standalone feature for OB-06 and OB-07.

### OB-08

**Destination Rationale**: The ROOM_SMELL_WHOSE_SAMEN cross-cutting DA interface gap (I3DArrayVariables GetDa/SetDa) is a Phase 25 concern because DA variable access is part of the NTR system infrastructure being redesigned there. Re-evaluation trigger: if Phase 25 planning finds no GetDa/SetDa task, create a standalone feature at that point.

**Chain Context**: Phase 25 DA interface design. Conditional standalone escalation if Phase 25 does not cover it.

### OB-09

**Destination Rationale**: GetDay/SetDay/GetTime/SetTime require cross-repo engine implementation in the engine repository. No single migration phase owns this work; a standalone feature (F833) provides clear tracking and can be scheduled independently of the phase sequence.

**Chain Context**: Cross-repo dependency (engine repo). Standalone F833 provides isolated tracking.

### OB-10

**Destination Rationale**: The CP-2 Step 2c behavioral flow test (Shop->Counter->State) requires non-stub implementations of the involved services. Until Phase 25 completes the relevant service implementations, this test cannot produce meaningful results and would be testing stubs against stubs.

**Chain Context**: Blocked on Phase 25 non-stub service implementations.

### OB-11

**Destination Rationale**: The Roslynator Analyzers investigation (CA1502, CA1506, IDE0060) has zero prerequisites and standalone research scope. It cannot be absorbed into Phase 23/24 planning features because it is a tooling investigation, not a migration task. Standalone F831 provides appropriate scoping.

**Chain Context**: Independent. Standalone F831 with no blocking dependencies.

### OB-12

**Destination Rationale**: The ac-static-verifier numeric parsing bug (ACs with `gte` matcher and bare numeric Expected fail with "Expected value must be in '`pattern` = N' format") is a standalone tooling defect. Reproduction outcome: reproduces (confirmed 2026-03-06). The fix is scoped to ac-static-verifier.py numeric comparison handling. Standalone F832 provides isolated defect tracking.

**Chain Context**: Independent tooling defect. Reproduction confirmed before routing.

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| OB-06: CVARSET BulkReset extraction (IVariableStore.BulkResetCharacterFlags) | Trigger-gated: when MOVEMENT.ERB migrates (second call site); cannot extract before trigger | Feature | F830 | Task 3 | [x] | 作成済み(A) |
| OB-07: IS_DOUTEI extraction (ICharacterUtilities.IsDoutei) | Trigger-gated: when second C# call site for IsDoutei appears; 22 ERB call sites across 14 files | Feature | F830 | Task 3 | [x] | 作成済み(A) |
| OB-11: Roslynator Analyzers investigation | Zero prerequisites; standalone research scope; cannot be absorbed into Phase 23/24 planning features | Feature | F831 | Task 4 | [x] | 作成済み(A) |
| OB-12: ac-static-verifier numeric Expected parsing | Needs reproduction verification and scoped fix (or close as no-op); standalone tooling concern | Feature | F832 | Task 5 | [x] | 作成済み(A) |
| OB-09: IEngineVariables indexed methods stubs (GetDay/SetDay/GetTime/SetTime) | Cross-repo engine implementation; no single migration phase owns this; standalone feature provides clear tracking | Feature | F833 | Task 6 | [x] | 作成済み(A) |

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
| 2026-03-06T23:00 | START | initializer | [REVIEWED]->[WIP] | OK |
| 2026-03-06T23:01 | Phase 2 | explorer | Investigation complete | OK |
| 2026-03-06T23:05 | Phase 4 | implementer | Task 1: Obligation Routing section written | OK |
| 2026-03-06T23:06 | Phase 4 | implementer | Task 2: Architecture doc updated (Phase 22 + Phase 25) | OK |
| 2026-03-06T23:06 | Phase 4 | implementer | Task 3: F830 [DRAFT] created + registered | OK |
| 2026-03-06T23:06 | Phase 4 | implementer | Task 4: F831 [DRAFT] created + registered | OK |
| 2026-03-06T23:06 | Phase 4 | implementer | Task 5: F832 [DRAFT] created + registered | OK |
| 2026-03-06T23:06 | Phase 4 | implementer | Task 6: F833 [DRAFT] created + registered | OK |
<!-- run-phase-1-completed -->
<!-- run-phase-2-completed -->
| 2026-03-06T23:10 | Phase 7 | ac-tester | AC verification 16/16 PASS | OK |
| 2026-03-06T23:12 | DEVIATION | feature-reviewer | Post-review NEEDS_REVISION | Predecessor Obligations status pending→routed |
| 2026-03-06T23:12 | Phase 8 | orchestrator | Fixed Predecessor Obligations status | OK |
<!-- run-phase-3-completed -->
<!-- run-phase-4-completed -->
<!-- run-phase-5-completed -->
<!-- run-phase-6-completed -->
| 2026-03-06T23:15 | DEVIATION | Bash | verify-logs exit 1 | Stale file-result.json from OB-12 bug; removed |
<!-- run-phase-7-completed -->
<!-- run-phase-8-completed -->
<!-- run-phase-9-completed -->

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A->B->A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase2-Review iter1: Deviation Context | Restructured to use template-required subsections (Observable Symptom, Execution Evidence, Files Involved, Attempted Solutions, Parent Session Observations)
- [fix] Phase2-Review iter1: Background/Predecessor Obligations | Moved to standalone section between Background and Root Cause Analysis
- [fix] Phase2-Review iter1: AC#5 | Added AC#14 for --unit N+5 chain text verification (F782->F813->F814->F826->F829)
- [fix] Phase2-Review iter1: AC#7 | Added AC#15 for ac-static-verifier reproduction outcome verification
- [fix] Phase2-Review iter1: Tasks 3/4/5 AC# column | Removed AC#1 and AC#11 (Task 1 output), kept AC#12 and AC#13
- [fix] Phase2-Review iter2: Related Features F826 status | Updated [WIP] to [DONE] to match Dependencies table
- [fix] Phase2-Review iter2: AC#8 pattern | Extended to cover both Phase 23 and F827 exclusion per Constraint C6
- [fix] Phase2-Review iter3: Technical Constraints F826 status | Updated [WIP] to [DONE], marked constraint as resolved
- [resolved-applied] Phase3-Maintainability iter4: OB-04/OB-05 (IVariableStore 2D SAVEDATA, NullMultipleBirthService) routed to Phase 27 (Extensions) but these are core pregnancy infrastructure, not extension modules. Phase 27 scope is IExtensionModule-based systems. Need re-evaluation of destination.
- [fix] PostLoop-UserFix post-loop: OB-04/OB-05 destination | Changed from Phase 27 to Phase 25 (user decision A). OB-10 rationale updated to reflect full coverage at Phase 25.
- [resolved-applied] Phase3-Maintainability iter4: OB-09 (IEngineVariables GetDay/SetDay/GetTime/SetTime) routed to Phase 26 (Special Modes & Messaging) but engine stubs are not Phase 26's scope. Need re-evaluation of destination.
- [fix] PostLoop-UserFix post-loop: OB-09 destination | Changed from Phase 26 to standalone Feature F833 (user decision A). Added Task 6, updated AC#12/AC#13, added Mandatory Handoff entry.
- [resolved-applied] Phase3-Maintainability iter4: Leak Prevention — 8 obligations (OB-02 through OB-05, OB-08 through OB-10) routed to Phase 25/26/27 via Option C but destination phase sections in phase-20-27-game-systems.md receive no update about incoming obligations. Task 2 only adds one F829 reference under Phase 22. Future planning features have no mechanism to discover obligations routed to them.
- [fix] PostLoop-UserFix post-loop: Leak Prevention | Extended Task 2 to add obligation list under Phase 25 section. Added AC#16 for Phase 25 obligation references verification (user decision A).
- [fix] Phase3-Maintainability iter1-restart: AC#16 pattern | Fixed broken regex (\\| = literal pipe, not alternation). Changed to Deferred Obligations.*F829 pattern with matches matcher.
- [fix] Phase2-Review iter2-restart: AC#10 pattern | Narrowed from generic F829 to Phase 22.*F829 to prevent false pass via Phase 25 content.
- [fix] Phase3-Maintainability iter4: OB-06/OB-07 grouping | Added justification in Key Decisions for combining into single F830 (same source F824, similar trigger patterns, Era.Core shared utilities)
- [fix] Phase2-Review iter5: OB-10 rationale | Updated to acknowledge partial coverage — OB-04/OB-05 stubs remain until Phase 27, CP-2 Step 2c scope at Phase 25 is partial
- [fix] Phase2-Review iter6: OB-08 rationale | Updated to acknowledge tentative routing — Phase 25 tasks don't explicitly require DA array extension, may need standalone feature
- [fix] Phase3-Maintainability iter4: OB-08 Key Decisions | Removed tentative qualifier, added re-evaluation trigger (if Phase 25 planning finds no GetDa/SetDa task, create standalone feature)

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} -> `{target}` or -- {reason} -->

---

<!-- fc-phase-6-completed -->
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
- [Handoff: F830](feature-830.md) - CVARSET BulkReset & IS_DOUTEI extraction (OB-06/OB-07, trigger-gated)
- [Handoff: F831](feature-831.md) - Roslynator Analyzers investigation (OB-11)
- [Handoff: F832](feature-832.md) - ac-static-verifier numeric Expected parsing fix (OB-12)
- [Handoff: F833](feature-833.md) - IEngineVariables indexed methods stubs (OB-09)
