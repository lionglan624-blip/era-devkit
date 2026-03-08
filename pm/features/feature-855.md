# Feature 855: Phase 25 Planning

## Status: [DONE]
<!-- fl-reviewed: 2026-03-08T12:57:50Z -->

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
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity — each completed phase triggers planning for the next phase, producing sub-feature DRAFTs that are the SSOT for the next phase's scope. Phase 25 covers NTR/Visitor/Location system migrations — a significantly larger scope than Phase 24.

### Problem (Current Issue)

Phase 25 scope (~24,000 lines across 38+ files) cannot be implemented as a single feature because the NTR Implementation group alone (14 files, ~14,069 lines, 8 HIGH-complexity files) exceeds typical feature volume limits by an order of magnitude (`phase-20-27-game-systems.md:867-895`). The architecture doc recommends a 3-group split (NTR, Visitor/Event, Location), but this first-level decomposition is insufficient -- the NTR group dwarfs the other two combined (~10,077 lines) and has significant internal coupling through CALL chains (NTR.ERB orchestrates NTR_FRIENDSHIP, NTR_SEX, NTR_EXHIBITION, etc.), requiring second-level decomposition based on functional cohesion and CALL graph analysis. Additionally, 11 deferred obligations from F852 (5 items) and F829 (6 OB items) must be routed to concrete sub-feature IDs, because they currently reference only "Phase 25" without actionable destinations.

### Goal (What to Achieve)

Produce sub-feature DRAFTs that decompose Phase 25 into manageable implementation units, covering all 8 architecture Tasks (`phase-20-27-game-systems.md:836-844`), routing all 11 deferred obligations to concrete sub-feature IDs, and including 2 mandatory transition features (Post-Phase Review Phase 25 + Phase 26 Planning). Decomposition uses functional subsystem ordering (not DDD layer ordering as Phase 24 used), informed by F854 post-phase review findings and CALL graph coupling analysis.

### Predecessor Obligations (for Mandatory Handoffs)

| Source Feature | Obligation | Category | Status |
|:--------------:|------------|----------|:------:|
| F852 | NtrParameters/Susceptibility mutation methods | deferred | pending |
| F852 | ExposureDegree Value Object | deferred | pending |
| F852 | NtrProgressionCreated domain event | deferred | pending |
| F852 | CalculateOrgasmCoefficient method declaration | deferred | pending |
| F852 | MARK system state integration for CHK_NTR_SATISFACTORY | deferred | pending |
| F850 | Ubiquitous Language cross-VO terminology consistency — verified consistent; bilingual XML doc pattern applied uniformly across NtrRoute, NtrPhase, NtrParameters, Susceptibility | handoff | resolved |
| F852 | 26 pre-existing ComEquivalence test failures (missing game YAML/config fixtures); out-of-scope for Phase 24; root cause unrelated to NTR domain | out-of-scope | pending |
| F829 | OB-02: SANTA cosplay text output | deferred | pending |
| F829 | OB-03: CLOTHES_ACCESSORY/INtrQuery wiring | deferred | pending |
| F829 | OB-04: IVariableStore 2D SAVEDATA stubs | deferred | pending |
| F829 | OB-05: NullMultipleBirthService runtime impl | deferred | pending |
| F829 | OB-08: I3DArrayVariables GetDa/SetDa DA gap | deferred | pending |
| F829 | OB-10: CP-2 behavioral flow test | deferred | pending |

### Architecture Task Coverage

<!-- Architecture Task 11: Phase 25 Planning -->

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does F855 exist as a bare DRAFT stub? | F849 Task 11 created F855 as a minimal stub expecting /fc to complete it | `feature-849.md:30-34` |
| 2 | Why can't Phase 25 be implemented as a single feature? | Phase 25 scope is ~24,000 lines across 38+ files -- far exceeding any single feature's volume limit | `phase-20-27-game-systems.md:796-799` |
| 3 | Why is the architecture doc's 3-group split insufficient? | The NTR Implementation group (14,069 lines) alone is larger than the other two groups combined (10,077 lines) and has 8 HIGH-complexity files | `phase-20-27-game-systems.md:867-895` |
| 4 | Why does the NTR group need internal decomposition? | NTR.ERB orchestrates calls to NTR_FRIENDSHIP, NTR_SEX, NTR_EXHIBITION, etc. with significant internal coupling; NTR_UTIL is called by nearly every other NTR file | `NTR.ERB:48-199`, `NTR_FRIENDSHIP.ERB:387,480` |
| 5 | Why (Root)? | Phase 25 is the first phase to migrate NTR system ERB files into the Phase 24 DDD Bounded Context (providing concrete INtrCalculator implementations, NtrParameters mutation), requiring multi-level functional subsystem decomposition with CALL graph-informed ordering -- not DDD layer ordering as Phase 24 used | `Era.Core/NTR/Domain/Services/INtrCalculator.cs:13` ("Concrete implementation provided in Phase 25") |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | F855 is a bare DRAFT stub with no Tasks, ACs, or decomposition | Phase 25's ~24,000-line scope requires multi-level decomposition (3 groups as first level, NTR internal split as second level) with functional subsystem ordering |
| Where | `pm/features/feature-855.md` (empty AC and Task tables) | `phase-20-27-game-systems.md:794-947` (Phase 25 section) + NTR/ ERB CALL graph coupling |
| Fix | Manually fill in a few tasks | Systematic decomposition informed by CALL graph analysis, obligation routing, and architecture doc Task coverage |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F854 | [DONE] | Predecessor: Post-Phase Review Phase 24; unblocks F855 |
| F849 | [DONE] | Pattern reference: Phase 24 Planning (produced 6 sub-features F850-F855) |
| F852 | [DONE] | Source of 5 deferred obligations routed to F855 |
| F850 | [DONE] | Source of 1 resolved handoff (cross-VO terminology) |
| F829 | [DONE] | Source of 6 deferred obligations (OB-02, OB-03, OB-04, OB-05, OB-08, OB-10) routed to Phase 25 |
| F830 | [CANCELLED] | Standalone: CVARSET BulkReset + IS_DOUTEI (OB-06/OB-07, NOT Phase 25 scope) |
| F833 | [DONE] | Standalone: IEngineVariables indexed stubs (OB-09, NOT Phase 25 scope) |
| F378 | [DONE] | AFFAIR_DISCLOSURE deferred to Phase 25 Task 6 |
| F406 | [DONE] | OrgasmProcessor.CalculateOrgasmRequirementCoefficient stub (routed to F856 NTR Core scope) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Predecessor F854 satisfied | FEASIBLE | F854 [DONE] per index-features.md |
| Architecture doc Phase 25 section defined | FEASIBLE | phase-20-27-game-systems.md:794-947 with 8 Tasks, 3 sub-groups, Sub-Feature Requirements |
| F852 5 deferred items routeable to sub-features | FEASIBLE | All 5 map to NTR Implementation group (domain model expansion) |
| F829 6 obligations routeable | FEASIBLE | OB-02/03 to NTR/Event group, OB-04/05 to infrastructure, OB-08 confirmed relevant (ROOM_SMELL_WHOSE in NTR_MASTER_SEX.ERB 5 times), OB-10 to first sub-feature |
| Sub-feature ID range available | FEASIBLE | Next Feature number = 856 per index-features.md |
| Decomposition pattern available | FEASIBLE | F849 provides proven planning feature template |
| ERB source files accessible | FEASIBLE | All 38+ files present in game repo; line counts verified within 0.1% of architecture doc estimates |
| Phase 24 domain model provides foundation | FEASIBLE | 21 C# files in Era.Core/NTR/ namespace complete |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Phase 25 scope | HIGH | Determines decomposition of ~24,000 lines into manageable sub-features; incorrect decomposition creates cascading issues |
| Deferred obligation routing | HIGH | 11 obligations from F852/F829 must be assigned concrete sub-feature IDs; unrouted obligations become forgotten debt |
| Phase 24 domain model consumers | MEDIUM | Sub-features will provide concrete implementations for INtrCalculator, NtrParameters mutation, etc. |
| Feature ID allocation | LOW | Consumes feature IDs F856+ from the global sequence; Next Feature number must be incremented correctly |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Sub-features must inherit Phase 25 Philosophy | phase-20-27-game-systems.md:938 | Every sub-feature DRAFT must include inherited Philosophy text |
| Sub-features must include debt cleanup tasks (TODO/FIXME/HACK) | phase-20-27-game-systems.md:939 | AC with not_contains verification required in each sub-feature |
| Sub-features must include equivalence tests | phase-20-27-game-systems.md:940 | Legacy ERB vs C# equivalence tests mandatory in each sub-feature |
| Sub-features must include zero-debt ACs | phase-20-27-game-systems.md:941 | Debt verification ACs in each sub-feature |
| Transition features mandatory (Tasks 7-8 of architecture) | phase-20-27-game-systems.md:843-844 | Post-Phase Review Phase 25 + Phase 26 Planning DRAFTs required |
| NTR_UTIL is foundational dependency | CALL graph analysis (called by nearly every NTR file) | Must be migrated before other NTR subsystems |
| NTR_MESSAGE is self-contained (2,337 lines, 100+ functions) | CALL graph analysis (no outbound CALL to other NTR files) | Can be independent sub-feature |
| EVENT_MESSAGE_COM has no CALL to NTR/ core logic | CALL graph analysis (4,599 lines; NTR references limited to kojo callbacks TRYCCALLFORM NTR_KOJO_* and display helpers NTR_NAME) | Event system cleanly separable from NTR core implementation |
| INtrCalculator concrete implementation is Phase 25 scope | INtrCalculator.cs:13 | CanAdvance/CanChangeRoute implementations consume NTR_UTIL.ERB logic |
| NtrParameters needs expansion beyond current 2 fields | NtrParameters.cs:9-35 (SlaveLevel, FavLevel only) | NTR_CHK_FAVORABLY reads CFLAG/TALENT arrays beyond current fields |
| SRP split prescribed for NTR engine | phase-20-27-game-systems.md:830-834 | NtrEngine.cs, NtrActionProcessor.cs, NtrMessageGenerator.cs, NtrEventHandler.cs |
| Strongly Typed IDs required | phase-20-27-game-systems.md:815-818 | VisitorId, NtrEventId must be assigned to sub-features |
| IVisitorSystem interface prescribed | phase-20-27-game-systems.md:820-828 | Must be assigned to Visitor/Event sub-feature |
| OB-08 re-evaluation trigger satisfied | feature-829.md:666, NTR_MASTER_SEX.ERB:605-746 | ROOM_SMELL_WHOSE used 5 times in NTR_MASTER_SEX.ERB; OB-08 stays in Phase 25 |
| Phase 25 external callers NOT in scope | CALL graph (67 files call NTR_ functions) | Kojo, COM, system files migrated in later phases; Phase 25 scope limited to 38+ files in architecture doc |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| NTR Implementation group (14,069 lines) too large for single sub-feature | HIGH | HIGH | Apply second-level decomposition based on CALL graph clusters: NTR core/util, NTR actions/sex, NTR master scenes, NTR messages, NTR behavioral systems |
| NTR file internal coupling prevents clean decomposition | MEDIUM | HIGH | Map CALL dependencies within NTR/ folder; accept coupling and use larger sub-features if splitting at specific boundary is infeasible |
| 11 F852+F829 deferred obligations misrouted to wrong sub-feature | MEDIUM | MEDIUM | Create explicit obligation-to-sub-feature mapping table in F855 tasks; verify each obligation has exactly 1 destination |
| EVENT_MESSAGE_COM (4,599 lines) exceeds single feature volume | HIGH | MEDIUM | Dedicated sub-feature for event message system |
| Cross-file CALL dependencies create ordering constraints within NTR | MEDIUM | HIGH | NTR_UTIL first, then NTR_ACTION, then consumer files; document ordering in sub-feature Dependencies |
| Architecture doc deliverable paths may diverge from actual implementation | MEDIUM | LOW | Phase 24 showed 3 naming + 1 structural divergence; expect similar corrections in Post-Phase Review Phase 25 |
| 26 ComEquivalence test failures block Phase 25 test execution | LOW | MEDIUM | Failures are pre-existing and unrelated to NTR domain; document as out-of-scope with concrete tracking destination |
| 67 external callers of NTR_ functions may appear to require Phase 25 scope expansion | LOW | HIGH | External callers (kojo, COMF, SYSTEM.ERB) are NOT migrated in Phase 25; scope limited to architecture doc file list |

---

## AC Design Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Architecture doc prescribes 8 Tasks for Phase 25 | phase-20-27-game-systems.md:836-844 | All 8 Tasks must be covered by sub-features; AC verifies task coverage |
| C2 | Sub-Feature Requirements: Philosophy, debt cleanup, equivalence, zero-debt AC (4 elements) | phase-20-27-game-systems.md:934-941 | Every implementation sub-feature DRAFT must include these 4 elements |
| C3 | Transition features mandatory (architecture Tasks 7-8) | phase-20-27-game-systems.md:843-844 | Two transition sub-feature DRAFTs required; AC verifies file existence and index registration |
| C4 | F855 itself is Type: research | feature-855.md:17 | F855 produces DRAFT files, not C# code; ACs use Glob/Grep matchers |
| C5 | F852 5 deferred items must be routed to specific sub-features | feature-855.md:39-43 | Each obligation must have exactly 1 destination sub-feature ID |
| C6 | F829 6 obligations must be routed (OB-08 confirmed relevant) | phase-20-27-game-systems.md:803-809 | OB-08 re-evaluation trigger satisfied; all 6 must be assigned concrete sub-feature IDs |
| C7 | Next Feature number must be incremented correctly | index-features.md (currently 856) | After allocating N IDs, Next Feature = 856 + N |
| C8 | Phase 25 SRP split prescribed | phase-20-27-game-systems.md:830-834 | NtrEngine.cs, NtrActionProcessor.cs, NtrMessageGenerator.cs, NtrEventHandler.cs must be assigned to sub-features |
| C9 | Strongly Typed IDs required (VisitorId, NtrEventId) | phase-20-27-game-systems.md:815-818 | Must be assigned to sub-features |
| C10 | IVisitorSystem interface prescribed | phase-20-27-game-systems.md:820-828 | Must be assigned to Visitor/Event sub-feature |
| C11 | 26 ComEquivalence failures must have tracking destination | feature-855.md:45 | Must route to concrete feature or document as tracked with explicit reason |

### Constraint Details

**C1: Architecture Task Coverage**
- **Source**: phase-20-27-game-systems.md:836-844 lists 8 explicit Tasks (6 implementation + 2 transition)
- **Verification**: Grep sub-feature DRAFTs for Architecture Task markers; verify 8/8 covered
- **AC Impact**: AC must verify that every architecture Task is assigned to at least one sub-feature
- **Collection Members** (MANDATORY): Visitor System Migration, Event System Migration, Turn-end Processing, NTR Subsystem Migration, Location Extensions, AFFAIR_DISCLOSURE Migration, Post-Phase Review Phase 25, Phase 26 Planning

**C2: Sub-Feature Requirements**
- **Source**: phase-20-27-game-systems.md:934-941
- **Verification**: Grep each sub-feature DRAFT for Philosophy inheritance, debt cleanup task, equivalence test task, zero-debt AC
- **AC Impact**: Each implementation sub-feature must contain all 4 elements; transition features exempt from equivalence/debt
- **Collection Members** (MANDATORY): Element 1: Philosophy inheritance, Element 2: Debt cleanup task (TODO/FIXME/HACK), Element 3: Equivalence tests, Element 4: Zero-debt ACs

**C3: Transition Features**
- **Source**: Architecture doc Tasks 7-8
- **Verification**: Glob for feature files + Grep index-features.md for registration
- **AC Impact**: Two DRAFTs required: Post-Phase Review Phase 25 (Type: infra) + Phase 26 Planning (Type: research)

**C4: Research Type Constraint**
- **Source**: feature-855.md:17
- **Verification**: F855 produces DRAFT markdown files, not C# code
- **AC Impact**: All ACs must use Glob/Grep matchers (not build/test matchers); no build verification required

**C5: F852 Deferred Item Routing**
- **Source**: feature-855.md Predecessor Obligations table
- **Verification**: Grep sub-feature DRAFTs for each obligation keyword
- **AC Impact**: Each of the 5 pending F852 items must appear in exactly 1 sub-feature's scope
- **Collection Members** (MANDATORY): Item 1: NtrParameters/Susceptibility mutation methods, Item 2: ExposureDegree Value Object, Item 3: NtrProgressionCreated domain event, Item 4: CalculateOrgasmCoefficient method declaration, Item 5: MARK system state integration for CHK_NTR_SATISFACTORY

**C6: F829 Obligation Routing**
- **Source**: phase-20-27-game-systems.md:803-809
- **Verification**: Grep sub-feature DRAFTs for each OB identifier
- **AC Impact**: Each of the 6 OB items must be assigned to a concrete sub-feature ID
- **Collection Members** (MANDATORY): OB-02: SANTA cosplay, OB-03: CLOTHES_ACCESSORY/INtrQuery wiring, OB-04: IVariableStore 2D SAVEDATA stubs, OB-05: NullMultipleBirthService, OB-08: I3DArrayVariables GetDa/SetDa DA gap, OB-10: CP-2 behavioral flow test

**C7: Next Feature Number Increment**
- **Source**: index-features.md
- **Verification**: Grep index-features.md for "Next Feature number"
- **AC Impact**: Must be incremented past all allocated sub-feature IDs to prevent ID collision

**C8: Phase 25 SRP Split**
- **Source**: phase-20-27-game-systems.md:830-834
- **Verification**: Sub-feature DRAFTs mention the prescribed C# class names in their Goal sections
- **AC Impact**: NtrEngine.cs assigned to F856, NtrActionProcessor.cs to F856, NtrMessageGenerator.cs to F860, NtrEventHandler.cs to F861. Verified indirectly by AC#2 (Architecture Task markers) and sub-feature Goal content.

**C9: Strongly Typed IDs**
- **Source**: phase-20-27-game-systems.md:815-818
- **Verification**: VisitorId and NtrEventId mentioned in sub-feature DRAFT Goals
- **AC Impact**: Both IDs assigned to F861 (Visitor/Event Core). Verified indirectly by AC#2 and sub-feature Goal content.

**C10: IVisitorSystem Interface**
- **Source**: phase-20-27-game-systems.md:820-828
- **Verification**: IVisitorSystem mentioned in F861 Goal section
- **AC Impact**: Assigned to F861 (Visitor/Event Core). Verified indirectly by AC#2 and sub-feature Goal content.

**C11: ComEquivalence Failure Tracking**
- **Source**: feature-855.md:45
- **Verification**: Grep sub-feature DRAFTs for "ComEquivalence" keyword
- **AC Impact**: 26 failures must have a concrete tracking destination in F856's Mandatory Handoff section. Covered by AC#7 (ComEquivalence keyword in obligation routing pattern).

---

## Dependencies
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F854 | [DONE] | Post-Phase Review Phase 24 must complete before Phase 25 Planning |
| Related | F849 | [DONE] | Pattern reference: Phase 24 Planning methodology (produced F850-F855) |
| Related | F852 | [DONE] | Source of 5 deferred obligations routed via F854 to F855 |
| Related | F829 | [DONE] | Source of 6 deferred obligations (OB-02/03/04/05/08/10) routed to Phase 25 |
| Related | F378 | [DONE] | AFFAIR_DISCLOSURE deferred to Phase 25 Task 6 |
| Related | F406 | [DONE] | OrgasmProcessor.CalculateOrgasmRequirementCoefficient stub (routed to F856 NTR Core scope) |
| Related | F830 | [CANCELLED] | Standalone: OB-06/OB-07 (NOT Phase 25 scope) |
| Related | F833 | [DONE] | Standalone: OB-09 (NOT Phase 25 scope) |
| Successor | F856 | [DRAFT] | NTR Core/Util Foundation (created by this feature) |
| Successor | F857 | [DRAFT] | NTR Behavioral Systems (created by this feature) |
| Successor | F858 | [DRAFT] | NTR Master Scenes (created by this feature) |
| Successor | F859 | [DRAFT] | NTR Extended Systems (created by this feature) |
| Successor | F860 | [DRAFT] | NTR Message Generator (created by this feature) |
| Successor | F861 | [DRAFT] | Visitor/Event Core (created by this feature) |
| Successor | F862 | [DRAFT] | EVENT_MESSAGE_COM Migration (created by this feature) |
| Successor | F863 | [DRAFT] | Location Extensions Migration (created by this feature) |
| Successor | F864 | [DRAFT] | AFFAIR_DISCLOSURE Migration (created by this feature) |
| Successor | F865 | [DRAFT] | Post-Phase Review Phase 25 (created by this feature) |
| Successor | F866 | [DRAFT] | Phase 26 Planning (created by this feature) |

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
<!-- Written by: ac-designer -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "each completed phase triggers planning for the next phase" | All 8 architecture Tasks must be assigned to sub-features | AC#2, AC#3, AC#4, AC#5, AC#6, AC#7, AC#8, AC#9 |
| "producing sub-feature DRAFTs that are the SSOT for the next phase's scope" | Sub-feature DRAFT files must exist on disk | AC#1 |
| "SSOT for the next phase's scope" | Every implementation sub-feature must inherit Phase 25 Philosophy | AC#14 |
| "SSOT for the next phase's scope" | Sub-Feature Requirements (debt cleanup, equivalence tests, zero-debt ACs) must be present | AC#15, AC#30 |

<!-- Deviation: 38 ACs exceed research type 3-5 guideline and 30 soft limit. Justified: 8 Architecture Tasks (AC#2-9) + 12 obligation routing (AC#16-27) + 11 sub-feature creation/registration (AC#1,10-13,28-29) + 4 sub-feature requirements (AC#14-15,30) + 8 predecessor chain (AC#31-38) require granular verification. AND conjunctions split per TDD parseability. Precedent: F424/F437. -->
### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Sub-feature DRAFT files created | file | Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="^# Feature") | gte | 11 | [x] |
| 2 | Visitor System Migration covered | file | Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="Visitor System Migration") | gte | 1 | [x] |
| 3 | Event System Migration covered | file | Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="Event System Migration") | gte | 1 | [x] |
| 4 | Turn-end Processing covered | file | Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="Turn-end Processing") | gte | 1 | [x] |
| 5 | NTR Subsystem Migration covered | file | Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="NTR Subsystem Migration") | gte | 1 | [x] |
| 6 | Location Extensions covered | file | Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="Location Extensions") | gte | 1 | [x] |
| 7 | AFFAIR_DISCLOSURE Migration covered | file | Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="AFFAIR_DISCLOSURE") | gte | 1 | [x] |
| 8 | Post-Phase Review Phase 25 covered | file | Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="Post-Phase Review Phase 25") | gte | 1 | [x] |
| 9 | Phase 26 Planning covered | file | Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="Phase 26 Planning") | gte | 1 | [x] |
| 10 | Transition F865 file exists | file | Glob(pm/features/feature-865.md) | exists | - | [x] |
| 11 | Transition F865 registered in index | file | Grep(pm/index-features.md, pattern="Post-Phase Review Phase 25") | contains | `Post-Phase Review Phase 25` | [x] |
| 12 | Transition F866 file exists | file | Glob(pm/features/feature-866.md) | exists | - | [x] |
| 13 | Transition F866 registered in index | file | Grep(pm/index-features.md, pattern="Phase 26 Planning") | contains | `Phase 26 Planning` | [x] |
| 14 | Philosophy inheritance in implementation sub-features | file | Grep(pm/features/feature-8{5[6-9],6[0-4]}.md, pattern="Pipeline Continuity") | gte | 9 | [x] |
| 15 | Debt cleanup pattern in implementation sub-features | file | Grep(pm/features/feature-8{5[6-9],6[0-4]}.md, pattern="TODO\|FIXME\|HACK") | gte | 9 | [x] |
| 16 | F852: NtrParameters routed to F856 | file | Grep(pm/features/feature-856.md, pattern="NtrParameters") | contains | `NtrParameters` | [x] |
| 17 | F852: ExposureDegree routed to F856 | file | Grep(pm/features/feature-856.md, pattern="ExposureDegree") | contains | `ExposureDegree` | [x] |
| 18 | F852: NtrProgressionCreated routed to F856 | file | Grep(pm/features/feature-856.md, pattern="NtrProgressionCreated") | contains | `NtrProgressionCreated` | [x] |
| 19 | F852: CalculateOrgasmCoefficient routed to F856 | file | Grep(pm/features/feature-856.md, pattern="CalculateOrgasmCoefficient") | contains | `CalculateOrgasmCoefficient` | [x] |
| 20 | F852: CHK_NTR_SATISFACTORY routed to F857 | file | Grep(pm/features/feature-857.md, pattern="CHK_NTR_SATISFACTORY") | contains | `CHK_NTR_SATISFACTORY` | [x] |
| 21 | F829: OB-02 routed to F861 | file | Grep(pm/features/feature-861.md, pattern="OB-02") | contains | `OB-02` | [x] |
| 22 | F829: OB-03 routed to F856 | file | Grep(pm/features/feature-856.md, pattern="OB-03") | contains | `OB-03` | [x] |
| 23 | F829: OB-04 routed to F856 | file | Grep(pm/features/feature-856.md, pattern="OB-04") | contains | `OB-04` | [x] |
| 24 | F829: OB-05 routed to F856 | file | Grep(pm/features/feature-856.md, pattern="OB-05") | contains | `OB-05` | [x] |
| 25 | F829: OB-08 routed to F858 | file | Grep(pm/features/feature-858.md, pattern="OB-08") | contains | `OB-08` | [x] |
| 26 | F829: OB-10 routed to F856 | file | Grep(pm/features/feature-856.md, pattern="OB-10") | contains | `OB-10` | [x] |
| 27 | ComEquivalence tracking routed to F856 | file | Grep(pm/features/feature-856.md, pattern="ComEquivalence") | contains | `ComEquivalence` | [x] |
| 28 | Next Feature number incremented | file | Grep(pm/index-features.md, pattern="Next Feature number") | matches | `Next Feature number.*: 867` | [x] |
| 29 | Index registration of all sub-features | file | Grep(path="pm/index-features.md", pattern="F8(5[6-9]\|6[0-6])") | gte | 11 | [x] |
| 30 | Equivalence test coverage in implementation sub-features | file | Grep(pm/features/feature-8{5[6-9],6[0-4]}.md, pattern="equivalence") | gte | 9 | [x] |
| 31 | Predecessor chain: F857 depends on F856 | file | Grep(pm/features/feature-857.md, pattern="Predecessor.*F856") | contains | `F856` | [x] |
| 32 | Predecessor chain: F860 depends on F856 | file | Grep(pm/features/feature-860.md, pattern="Predecessor.*F856") | contains | `F856` | [x] |
| 33 | Predecessor chain: F861 depends on F860 | file | Grep(pm/features/feature-861.md, pattern="Predecessor.*F860") | contains | `F860` | [x] |
| 34 | Predecessor chain: F858 depends on F857 | file | Grep(pm/features/feature-858.md, pattern="Predecessor.*F857") | contains | `F857` | [x] |
| 35 | Predecessor chain: F859 depends on F856 | file | Grep(pm/features/feature-859.md, pattern="Predecessor.*F856") | contains | `F856` | [x] |
| 36 | Predecessor chain: F862 depends on F861 | file | Grep(pm/features/feature-862.md, pattern="Predecessor.*F861") | contains | `F861` | [x] |
| 37 | Predecessor chain: F863 depends on F861 | file | Grep(pm/features/feature-863.md, pattern="Predecessor.*F861") | contains | `F861` | [x] |
| 38 | Predecessor chain: F864 depends on F861 | file | Grep(pm/features/feature-864.md, pattern="Predecessor.*F861") | contains | `F861` | [x] |

### AC Details

**AC#1: Sub-feature DRAFT files created**
- **Test**: `Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="^# Feature")` — Expected: >= 11 files matching heading pattern
- **Rationale**: 9 implementation (F856-F864) + 2 transition (F865-F866) = 11 total. Uses Grep with file heading pattern for gte-compatible counting.

**AC#2: Visitor System Migration covered**
- **Test**: `Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="Visitor System Migration")` — Expected: >= 1
- **Derivation**: phase-20-27-game-systems.md:836-844 arch doc Task 1 (Visitor System Migration) → F861.

**AC#3: Event System Migration covered**
- **Test**: `Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="Event System Migration")` — Expected: >= 1
- **Derivation**: phase-20-27-game-systems.md:836-844 arch doc Task 2 (Event System Migration) → F861, F862.

**AC#4: Turn-end Processing covered**
- **Test**: `Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="Turn-end Processing")` — Expected: >= 1
- **Derivation**: phase-20-27-game-systems.md:836-844 arch doc Task 3 (Turn-end Processing) → F861.

**AC#5: NTR Subsystem Migration covered**
- **Test**: `Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="NTR Subsystem Migration")` — Expected: >= 1
- **Derivation**: phase-20-27-game-systems.md:836-844 arch doc Task 4 (NTR Subsystem Migration) → F856, F857, F858, F859, F860.

**AC#6: Location Extensions covered**
- **Test**: `Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="Location Extensions")` — Expected: >= 1
- **Derivation**: phase-20-27-game-systems.md:836-844 arch doc Task 5 (Location Extensions) → F863.

**AC#7: AFFAIR_DISCLOSURE Migration covered**
- **Test**: `Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="AFFAIR_DISCLOSURE")` — Expected: >= 1
- **Derivation**: phase-20-27-game-systems.md:836-844 arch doc Task 6 (AFFAIR_DISCLOSURE Migration) → F864.

**AC#8: Post-Phase Review Phase 25 covered**
- **Test**: `Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="Post-Phase Review Phase 25")` — Expected: >= 1
- **Derivation**: phase-20-27-game-systems.md:836-844 arch doc Task 7 (Post-Phase Review Phase 25) → F865.

**AC#9: Phase 26 Planning covered**
- **Test**: `Grep(pm/features/feature-8{5[6-9],6[0-6]}.md, pattern="Phase 26 Planning")` — Expected: >= 1
- **Derivation**: phase-20-27-game-systems.md:836-844 arch doc Task 8 (Phase 26 Planning) → F866.

**AC#10: Transition F865 file exists**
- **Test**: `Glob(pm/features/feature-865.md)` — Expected: exists
- **Rationale**: C3 constraint requires file existence.

**AC#11: Transition F865 registered in index**
- **Test**: `Grep(pm/index-features.md, pattern="Post-Phase Review Phase 25")` — Expected: contains `Post-Phase Review Phase 25`
- **Rationale**: C3 constraint requires index registration.

**AC#12: Transition F866 file exists**
- **Test**: `Glob(pm/features/feature-866.md)` — Expected: exists
- **Rationale**: C3 constraint requires file existence.

**AC#13: Transition F866 registered in index**
- **Test**: `Grep(pm/index-features.md, pattern="Phase 26 Planning")` — Expected: contains `Phase 26 Planning`
- **Rationale**: C3 constraint requires index registration.

**AC#14: Philosophy inheritance in 9 implementation sub-features**
- **Test**: `Grep(pm/features/feature-8{5[6-9],6[0-4]}.md, pattern="Pipeline Continuity")` — Expected: >= 9. C2 Element 1. Excludes transition features F865/F866 (exempt per Technical Design).

**AC#15: Debt cleanup pattern in 9 implementation sub-features**
- **Test**: `Grep(pm/features/feature-8{5[6-9],6[0-4]}.md, pattern="TODO|FIXME|HACK")` — Expected: >= 9. C2 Element 2+4. Excludes transition features F865/F866 (exempt per Technical Design).

**AC#16: F852 NtrParameters routed to F856**
- **Test**: `Grep(pm/features/feature-856.md, pattern="NtrParameters")` — Expected: contains `NtrParameters`
- **Derivation**: C5 constraint. NTR_UTIL.ERB contains parameter mutation logic.

**AC#17: F852 ExposureDegree routed to F856**
- **Test**: `Grep(pm/features/feature-856.md, pattern="ExposureDegree")` — Expected: contains `ExposureDegree`
- **Derivation**: C5 constraint. Value Object design during NTR_UTIL migration phase.

**AC#18: F852 NtrProgressionCreated routed to F856**
- **Test**: `Grep(pm/features/feature-856.md, pattern="NtrProgressionCreated")` — Expected: contains `NtrProgressionCreated`
- **Derivation**: C5 constraint. Domain event wiring at system integration layer.

**AC#19: F852 CalculateOrgasmCoefficient routed to F856**
- **Test**: `Grep(pm/features/feature-856.md, pattern="CalculateOrgasmCoefficient")` — Expected: contains `CalculateOrgasmCoefficient`
- **Derivation**: C5 constraint. INtrCalculator implementation stub at NTR core layer.

**AC#20: F852 CHK_NTR_SATISFACTORY routed to F857**
- **Test**: `Grep(pm/features/feature-857.md, pattern="CHK_NTR_SATISFACTORY")` — Expected: contains `CHK_NTR_SATISFACTORY`
- **Derivation**: C5 constraint. CHK_NTR_SATISFACTORY logic lives in behavioral systems.

**AC#21: F829 OB-02 routed to F861**
- **Test**: `Grep(pm/features/feature-861.md, pattern="OB-02")` — Expected: contains `OB-02`
- **Derivation**: C6 constraint. Visitor/Event system enables SANTA cosplay UI primitives.

**AC#22: F829 OB-03 routed to F856**
- **Test**: `Grep(pm/features/feature-856.md, pattern="OB-03")` — Expected: contains `OB-03`
- **Derivation**: C6 constraint. INtrQuery interface defined at NTR core migration.

**AC#23: F829 OB-04 routed to F856**
- **Test**: `Grep(pm/features/feature-856.md, pattern="OB-04")` — Expected: contains `OB-04`
- **Derivation**: C6 constraint. Infrastructure stubs at foundation sub-feature boundary.

**AC#24: F829 OB-05 routed to F856**
- **Test**: `Grep(pm/features/feature-856.md, pattern="OB-05")` — Expected: contains `OB-05`
- **Derivation**: C6 constraint. 8-method stub implementation at foundation layer.

**AC#25: F829 OB-08 routed to F858**
- **Test**: `Grep(pm/features/feature-858.md, pattern="OB-08")` — Expected: contains `OB-08`
- **Derivation**: C6 constraint. ROOM_SMELL_WHOSE appears 5 times in NTR_MASTER_SEX.ERB.

**AC#26: F829 OB-10 routed to F856**
- **Test**: `Grep(pm/features/feature-856.md, pattern="OB-10")` — Expected: contains `OB-10`
- **Derivation**: C6 constraint. CP-2 behavioral flow test at first implementation sub-feature.

**AC#27: ComEquivalence tracking routed to F856**
- **Test**: `Grep(pm/features/feature-856.md, pattern="ComEquivalence")` — Expected: contains `ComEquivalence`
- **Derivation**: C11 constraint. 26 pre-existing failures tracked as Mandatory Handoff from F856.

**AC#28: Next Feature number = 867**
- **Test**: `Grep(pm/index-features.md, pattern="Next Feature number")` — matches `Next Feature number.*: 867`. C7 constraint.

**AC#29: Index registration of all 11 sub-features**
- **Test**: `Grep(pm/index-features.md, pattern="F8(5[6-9]|6[0-6])")` — Expected: >= 11.
- **Note**: Both AC#1 (file existence) and AC#29 (index registration) must PASS. DRAFT Creation Checklist (F837 lesson) requires both verification steps.

**AC#30: Equivalence test coverage in 9 implementation sub-features**
- **Test**: `Grep(pm/features/feature-8{5[6-9],6[0-4]}.md, pattern="equivalence")` — Expected: >= 9. C2 Element 3.

**AC#31: Predecessor chain — F857 depends on F856**
- **Test**: `Grep(pm/features/feature-857.md, pattern="Predecessor.*F856")` — Expected: contains `F856`
- **Derivation**: Functional subsystem ordering constraint. F857 (NTR Behavioral) depends on F856 (NTR Core/Util) per CALL graph analysis.

**AC#32: Predecessor chain — F860 depends on F856**
- **Test**: `Grep(pm/features/feature-860.md, pattern="Predecessor.*F856")` — Expected: contains `F856`
- **Derivation**: Functional subsystem ordering constraint. F860 (NTR Message) depends on F856 (NTR Core/Util) per CALL graph analysis.

**AC#33: Predecessor chain — F861 depends on F860**
- **Test**: `Grep(pm/features/feature-861.md, pattern="Predecessor.*F860")` — Expected: contains `F860`
- **Derivation**: Functional subsystem ordering constraint. F861 (Visitor/Event) depends on F860 (NTR Message) per CALL graph analysis.

**AC#34: Predecessor chain — F858 depends on F857**
- **Test**: `Grep(pm/features/feature-858.md, pattern="Predecessor.*F857")` — Expected: contains `F857`
- **Derivation**: Functional subsystem ordering constraint. F858 (NTR Master Scenes) depends on F857 (NTR Behavioral) — master scenes reference behavioral state.

**AC#35: Predecessor chain — F859 depends on F856**
- **Test**: `Grep(pm/features/feature-859.md, pattern="Predecessor.*F856")` — Expected: contains `F856`
- **Derivation**: Functional subsystem ordering constraint. F859 (NTR Extended Systems) depends on F856 (NTR Core/Util) — extended systems reference core utilities.

**AC#36: Predecessor chain — F862 depends on F861**
- **Test**: `Grep(pm/features/feature-862.md, pattern="Predecessor.*F861")` — Expected: contains `F861`
- **Derivation**: Functional subsystem ordering constraint. F862 (EVENT_MESSAGE_COM) depends on F861 (Visitor/Event Core) — EventDispatcher.cs interface required.

**AC#37: Predecessor chain — F863 depends on F861**
- **Test**: `Grep(pm/features/feature-863.md, pattern="Predecessor.*F861")` — Expected: contains `F861`
- **Derivation**: Functional subsystem ordering constraint. F863 (Location Extensions) depends on F861 (Visitor/Event Core) — uses IVisitorSystem.

**AC#38: Predecessor chain — F864 depends on F861**
- **Test**: `Grep(pm/features/feature-864.md, pattern="Predecessor.*F861")` — Expected: contains `F861`
- **Derivation**: Functional subsystem ordering constraint. F864 (AFFAIR_DISCLOSURE) depends on F861 (Visitor/Event Core) — IEventContext defined in F861.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Produce sub-feature DRAFTs that decompose Phase 25 | AC#1 |
| 2 | Covering all 8 architecture Tasks | AC#2, AC#3, AC#4, AC#5, AC#6, AC#7, AC#8, AC#9 |
| 3 | Routing all 11 deferred obligations + ComEquivalence tracking to concrete sub-feature IDs | AC#16, AC#17, AC#18, AC#19, AC#20, AC#21, AC#22, AC#23, AC#24, AC#25, AC#26, AC#27 |
| 4 | Including 2 mandatory transition features (Post-Phase Review Phase 25 + Phase 26 Planning) | AC#10, AC#11, AC#12, AC#13 |
| 5 | Decomposition uses functional subsystem ordering | AC#2, AC#3, AC#4, AC#5, AC#6, AC#7, AC#8, AC#9, AC#31, AC#32, AC#33, AC#34, AC#35, AC#36, AC#37, AC#38 |
| 6 | Sub-Feature Requirements: Philosophy inheritance | AC#14 |
| 7 | Sub-Feature Requirements: debt cleanup, zero-debt ACs | AC#15 |
| 8 | Sub-Feature Requirements: equivalence tests | AC#30 |
| 9 | Index updated with all sub-features | AC#29 |
| 10 | Next Feature number incremented correctly | AC#28 |

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer -->

### Approach

F855 is a research-type planning feature. Its deliverable is a set of sub-feature DRAFT files plus index registration — no C# code, no tests, no builds. The implementation approach is:

1. **Allocate 11 sub-feature IDs** from the current Next Feature number (856): F856–F866.
2. **Create 11 DRAFT feature files** in `pm/features/`, organized into three groups with second-level NTR decomposition:

   **Group 1: NTR Subsystems (Architecture Task 4: NTR Subsystem Migration — second-level split into 5 sub-features)**

   The architecture doc's single "NTR Implementation" group (14 files, 14,055 lines) requires second-level decomposition based on CALL graph analysis:

   - NTR_UTIL.ERB (1,500 lines) provides foundational utility functions (`NTR_COUNTER`, `NTR_CHK_FAVORABLY`, `NTR_RESET_VISITOR_ACTION`, `NTR_PUNISHMENT`, `NTR_ADD_SURRENDER`) called by NTR_ACTION, NTR_FRIENDSHIP, NTR_EXHIBITION, and NTR.ERB — must be first.
   - NTR_ACTION.ERB (406 lines) defines action primitives (`NTR_KISS`, `NTR_PET`, `NTR_BUST_PET`, `NTR_FELLATIO`) called by NTR_FRIENDSHIP — depends on NTR_UTIL.
   - NTR_FRIENDSHIP.ERB (918 lines) depends on NTR_UTIL + NTR_ACTION + NTR_EXHIBITION — second-tier consumer.
   - NTR_EXHIBITION.ERB (469 lines) depends on NTR_UTIL; called by NTR_FRIENDSHIP.
   - NTR_SEX.ERB (1,150 lines) depends on NTR_UTIL + NTR_ACTION.
   - NTR_MASTER_SEX.ERB (1,507 lines) + NTR_MASTER_3P_SEX.ERB (2,200 lines) are self-contained master scene processors.
   - NTR_MESSAGE.ERB (2,337 lines) is self-contained (no outbound CALL to other NTR files); implements NtrMessageGenerator per SRP split.
   - NTR_TAKEOUT.ERB (1,794 lines), NTR_VISITOR.ERB (541 lines), NTR_COMF416.ERB (272 lines), NTR陥落イベント.ERB (206 lines) are extended support systems.
   - NTR.ERB (251 lines) is the orchestrator; NTR_OPTION.ERB (504 lines) is configuration-only.

   Sub-feature assignments:
   - `feature-856.md`: NTR Core/Util Foundation (Type: erb). Files: NTR_UTIL.ERB (1,500), NTR_ACTION.ERB (406), NTR.ERB (251), NTR_OPTION.ERB (504). Total: ~2,661 lines. Produces: NtrEngine.cs (system integration), NtrActionProcessor.cs, NtrUtilService.cs, NtrOptions.cs. **Receives domain model obligations from F852 (NtrParameters mutation, ExposureDegree, NtrProgressionCreated, CalculateOrgasmCoefficient) and F829 OB-03/OB-04/OB-05/OB-10.**
   - `feature-857.md`: NTR Behavioral Systems (Type: erb). Files: NTR_FRIENDSHIP.ERB (918), NTR_EXHIBITION.ERB (469), NTR_SEX.ERB (1,150). Total: ~2,537 lines. Produces: FriendshipSystem.cs, ExhibitionHandler.cs. **Receives F852 MARK system state integration for CHK_NTR_SATISFACTORY.**
   - `feature-858.md`: NTR Master Scenes (Type: erb). Files: NTR_MASTER_SEX.ERB (1,507), NTR_MASTER_3P_SEX.ERB (2,200). Total: ~3,707 lines. Produces: ThreesomeHandler.cs + master sex scene processor. **Receives F829 OB-08 (I3DArrayVariables GetDa/SetDa — ROOM_SMELL_WHOSE appears 5 times in NTR_MASTER_SEX.ERB).**
   - `feature-859.md`: NTR Extended Systems (Type: erb). Files: NTR_TAKEOUT.ERB (1,794), NTR_VISITOR.ERB (541), NTR_COMF416.ERB (272), NTR陥落イベント.ERB (206). Total: ~2,813 lines. Produces: DateOutSystem.cs, CorruptionEvents.cs, NtrVisitorHandler.cs. **NTR Subsystem Migration split: extended support subsystems separated from behavioral cluster (NTR_FRIENDSHIP/EXHIBITION/SEX) per CALL graph analysis — no friendship/exhibition coupling.**
   - `feature-860.md`: NTR Message Generator (Type: erb). Files: NTR_MESSAGE.ERB (2,337). Total: ~2,337 lines. Produces: NtrMessageGenerator.cs per SRP split mandate. Self-contained with no outbound NTR CALL dependencies — can be implemented in parallel after F856.

   **Group 2: Visitor & Event Systems (Architecture Tasks 1-3: Visitor System, Event System, Turn-end Processing — split into 2 sub-features)**

   - `feature-861.md`: Visitor/Event Core (Type: erb). Files: INTRUDER.ERB (323), EVENT_KOJO.ERB (392), EVENT_MESSAGE.ERB (258), EVENT_MESSAGE_ORGASM.ERB (305), EVENTCOMEND.ERB (658), EVENTTURNEND.ERB (700), 情事中に踏み込み.ERB (246). Total: ~2,882 lines. Produces: VisitorAI.cs, ScheduleManager.cs, EventDispatcher.cs, EventHandlers.cs, IVisitorSystem interface, VisitorId + NtrEventId strongly typed IDs. **Receives F829 OB-02 (SANTA cosplay — requires engine-layer UI primitives now available).**
   - `feature-862.md`: EVENT_MESSAGE_COM Migration (Type: erb). Files: EVENT_MESSAGE_COM.ERB (4,599). Total: ~4,599 lines. Self-contained: architecture doc noted CALL graph analysis shows NTR references limited to kojo callbacks (TRYCCALLFORM NTR_KOJO_*) and display helpers (NTR_NAME) — no NTR core dependency. Produces: ComEventMessageHandler.cs. Predecessor: F861 (EventDispatcher interface required).

   **Group 3: Location Extensions + Standalone Migrations (Architecture Tasks 5-6)**

   - `feature-863.md`: Location Extensions Migration (Type: erb). Files: all 訪問者宅拡張/*.ERB (16 files: COMF460ex*.ERB 6 files, COMF461ex*.ERB 3 files, COMF462ex.ERB, COMF466ex*.ERB 3 files, COMF467ex.ERB, COMF46x.ERB, PLACEex.ERB). Total: ~2,500 lines. Produces: VisitorHomeExtension.cs.
   - `feature-864.md`: AFFAIR_DISCLOSURE Migration (Type: erb). Files: INFO.ERB:693-795 (~103 lines). Produces: AffairDisclosure.cs in `src/Era.Core/Events/`. Requires IEventContext defined in F861. Predecessor: F861.

   **Transition Features (Architecture Tasks 7-8)**

   - `feature-865.md`: Post-Phase Review Phase 25 (Type: infra). Predecessors: F858, F859, F862, F863, F864 (all terminal implementation sub-features must complete before review).
   - `feature-866.md`: Phase 26 Planning (Type: research). Predecessor: F865.

3. **Register all 11** in `pm/index-features.md` under a new `### Phase 25: AI & Visitor Systems` section with [DRAFT] status and correct Depends On values.
4. **Increment Next Feature number** from 856 to 867 in `pm/index-features.md`.

**Decomposition Rationale**: Architecture doc lists 8 Tasks but allocation produces 11 features because:
- Architecture Task 4 (NTR Subsystem Migration) is split into F856+F857+F858+F859+F860 — the 14-file NTR group (14,055 lines) requires second-level decomposition per CALL graph analysis. F856 (core/util) + F860 (message, self-contained 2,337 lines) split the foundation; F857 (behavioral cluster) + F858 (master scenes) + F859 (extended support) split the consumers.
- Architecture Tasks 1-3 (Visitor System, Event System, Turn-end Processing) are split into F861+F862 — EVENT_MESSAGE_COM alone is 4,599 lines; combining with other event files would create a ~7,500-line single feature.
- Architecture Tasks 5-8 (Location Extensions, AFFAIR_DISCLOSURE, Post-Phase Review, Phase 26 Planning) map 1:1 to F863, F864, F865, F866.

**Predecessor/Successor chain** (CALL graph ordering):
- F856 → Predecessor: F855 (this feature)
- F857 → Predecessor: F856 (depends on NTR_UTIL and NTR_ACTION)
- F858 → Predecessor: F857 (master scenes reference behavioral state)
- F859 → Predecessor: F856 (extended systems reference core utilities; can be parallel with F857-F858 in principle, but single-chain for simplicity)
- F860 → Predecessor: F856 (NTR_MESSAGE uses NTR_UTIL functions; self-contained otherwise)
- F861 → Predecessor: F860 (visitor/event system is a consumer of NTR message infrastructure)
- F862 → Predecessor: F861 (EventDispatcher.cs interface required)
- F863 → Predecessor: F861 (Location extensions use IVisitorSystem)
- F864 → Predecessor: F861 (IEventContext defined in F861)
- F865 → Predecessors: F858, F859, F862, F863, F864 (post-phase review after ALL implementation sub-features complete)
- F866 → Predecessor: F865 (planning follows review)

**Obligation Routing** (all 11 deferred items):

| Obligation | Source | Routed To | Domain Rationale |
|------------|--------|-----------|-----------------|
| NtrParameters/Susceptibility mutation methods | F852 | F856 | NTR_UTIL.ERB contains parameter mutation logic; domain model expansion aligns with core foundation |
| ExposureDegree Value Object | F852 | F856 | Value Object design during NTR_UTIL migration phase |
| NtrProgressionCreated domain event | F852 | F856 | Domain event wiring at system integration layer (NtrEngine.cs) |
| CalculateOrgasmCoefficient method declaration | F852 | F856 | INtrCalculator implementation stub; belongs at NTR core layer |
| MARK system state integration for CHK_NTR_SATISFACTORY | F852 | F857 | CHK_NTR_SATISFACTORY logic lives in behavioral systems (NTR_FRIENDSHIP territory) |
| OB-02: SANTA cosplay text output | F829 | F861 | Visitor/Event system migration enables engine-layer UI primitives for CLOTHE_EFFECT |
| OB-03: CLOTHES_ACCESSORY/INtrQuery wiring | F829 | F856 | INtrQuery interface defined when NTR system migrates (NTR core sub-feature) |
| OB-04: IVariableStore 2D SAVEDATA stubs | F829 | F856 | Infrastructure stubs belong at the foundation sub-feature boundary |
| OB-05: NullMultipleBirthService runtime impl | F829 | F856 | 8-method stub implementation; infrastructure concern at foundation layer |
| OB-08: I3DArrayVariables GetDa/SetDa DA gap | F829 | F858 | ROOM_SMELL_WHOSE appears 5 times in NTR_MASTER_SEX.ERB (confirmed by F852/feature-829.md:666) |
| OB-10: CP-2 behavioral flow test | F829 | F856 | Shop→Counter→State flow test; behavioral infrastructure verified at first implementation sub-feature |

**Sub-Feature Requirements** (from `phase-20-27-game-systems.md:934-941`) applied to F856–F864:
- Philosophy: "Pipeline Continuity" inherited in each implementation sub-feature
- Debt cleanup: TODO/FIXME/HACK removal Task in each implementation sub-feature
- Equivalence tests: legacy ERB vs C# equivalence test Task in each implementation sub-feature
- Zero-debt AC: `not_matches` TODO|FIXME|HACK AC in each implementation sub-feature
- Transition features F865 and F866 are exempt from equivalence/debt requirements

**Task Coverage Table** (for AC#2 verification):

| Architecture Task | Description | Covered By |
|:-----------------:|-------------|------------|
| Visitor System Migration | Visitor System Migration | F861 |
| Event System Migration | Event System Migration | F861, F862 |
| Turn-end Processing | Turn-end Processing | F861 |
| NTR Subsystem Migration | NTR Subsystem Migration | F856, F857, F858, F859, F860 |
| Location Extensions | Location Extensions | F863 |
| AFFAIR_DISCLOSURE Migration | AFFAIR_DISCLOSURE Migration | F864 |
| Post-Phase Review Phase 25 | Post-Phase Review Phase 25 | F865 |
| Phase 26 Planning | Phase 26 Planning | F866 |

**ComEquivalence Test Failures (26 pre-existing)**: These 26 failures are pre-existing and unrelated to the NTR domain (missing game YAML/config fixtures). They will be tracked as a Mandatory Handoff from F856 to a dedicated fixture-repair feature (to be created during F856 Task execution if the failures block test execution). This satisfies C11.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create 11 DRAFT files: feature-856.md through feature-866.md; Grep heading pattern returns 11 >= 11 |
| 2 | Sub-feature files contain `<!-- Architecture Task: Visitor System Migration -->` marker; Grep returns >= 1 |
| 3 | Sub-feature files contain `<!-- Architecture Task: Event System Migration -->` marker; Grep returns >= 1 |
| 4 | Sub-feature files contain `<!-- Architecture Task: Turn-end Processing -->` marker; Grep returns >= 1 |
| 5 | Sub-feature files contain `<!-- Architecture Task: NTR Subsystem Migration -->` marker; Grep returns >= 1 |
| 6 | Sub-feature files contain `<!-- Architecture Task: Location Extensions -->` marker; Grep returns >= 1 |
| 7 | Sub-feature files contain `<!-- Architecture Task: AFFAIR_DISCLOSURE Migration -->` marker; Grep returns >= 1 |
| 8 | Sub-feature files contain `<!-- Architecture Task: Post-Phase Review Phase 25 -->` marker; Grep returns >= 1 |
| 9 | Sub-feature files contain `<!-- Architecture Task: Phase 26 Planning -->` marker; Grep returns >= 1 |
| 10 | F865 file exists via Glob |
| 11 | Index contains "Post-Phase Review Phase 25" via Grep |
| 12 | F866 file exists via Glob |
| 13 | Index contains "Phase 26 Planning" via Grep |
| 14 | 9 implementation sub-features include "Pipeline Continuity"; Grep returns 9 >= 9 |
| 15 | 9 implementation sub-features include `TODO\|FIXME\|HACK` pattern; Grep returns 9 >= 9 |
| 16 | F856 contains "NtrParameters" |
| 17 | F856 contains "ExposureDegree" |
| 18 | F856 contains "NtrProgressionCreated" |
| 19 | F856 contains "CalculateOrgasmCoefficient" |
| 20 | F857 contains "CHK_NTR_SATISFACTORY" |
| 21 | F861 contains "OB-02" |
| 22 | F856 contains "OB-03" |
| 23 | F856 contains "OB-04" |
| 24 | F856 contains "OB-05" |
| 25 | F858 contains "OB-08" |
| 26 | F856 contains "OB-10" |
| 27 | F856 contains "ComEquivalence" |
| 28 | Index Next Feature number matches 867; Grep matches regex |
| 29 | Index contains 11 entries matching F856-F866 pattern; Grep returns 11 >= 11 |
| 30 | 9 implementation sub-features include "equivalence"; Grep returns 9 >= 9 |
| 31 | F857 Dependencies contains "Predecessor.*F856"; Grep returns match |
| 32 | F860 Dependencies contains "Predecessor.*F856"; Grep returns match |
| 33 | F861 Dependencies contains "Predecessor.*F860"; Grep returns match |
| 34 | F858 Dependencies contains "Predecessor.*F857"; Grep returns match |
| 35 | F859 Dependencies contains "Predecessor.*F856"; Grep returns match |
| 36 | F862 Dependencies contains "Predecessor.*F861"; Grep returns match |
| 37 | F863 Dependencies contains "Predecessor.*F861"; Grep returns match |
| 38 | F864 Dependencies contains "Predecessor.*F861"; Grep returns match |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| NTR second-level decomposition granularity | A: 3 sub-features (core, behavioral, message/scenes), B: 5 sub-features (core/util, behavioral, master, extended, message), C: 7 sub-features (1:1 with major file groups) | B: 5 sub-features (F856-F860) | Option A creates an ~8,000-line master scenes+message sub-feature. Option C over-decomposes small files (~200-272 lines). Option B respects CALL graph clusters: NTR_UTIL+ACTION (foundation), FRIENDSHIP+EXHIBITION+SEX (behavioral), MASTER_SEX+3P (master scenes), TAKEOUT+VISITOR+COMF416+陥落 (extended support), MESSAGE (self-contained generator). |
| NTR_MESSAGE placement | A: with NTR Core (F856), B: standalone sub-feature (F860), C: with NTR Behavioral (F857) | B: standalone F860 | NTR_MESSAGE.ERB is 2,337 lines (largest NTR file after NTR_MASTER_3P_SEX); it is architecturally self-contained with no outbound CALL to other NTR files; NtrMessageGenerator.cs is prescribed by SRP split. Standalone placement allows parallel development after F856. |
| EVENT_MESSAGE_COM placement | A: grouped with other Event files (F861), B: standalone sub-feature (F862) | B: standalone F862 | EVENT_MESSAGE_COM.ERB is 4,599 lines — combining with other event files (2,882 lines) would create a 7,481-line single feature. Architecture constraints note it is cleanly separable (NTR references limited to kojo callbacks). |
| AFFAIR_DISCLOSURE predecessor | A: F863 (Location Extensions), B: F861 (Visitor/Event Core), C: F855 (this feature) | B: F861 | AFFAIR_DISCLOSURE requires IEventContext defined in F861. Location Extensions (F863) also depends on F861 but is not a prerequisite for AFFAIR_DISCLOSURE; F864 can implement in parallel with F863 after F861. |
| F852 obligation routing: CalculateOrgasmCoefficient | A: F856 (NTR Core), B: F860 (NTR Message), C: F858 (Master Scenes) | A: F856 | INtrCalculator concrete implementation is the primary Phase 25 goal per `INtrCalculator.cs:13`; the CalculateOrgasmCoefficient method declaration is part of the domain service interface expansion, not message generation. Routes to foundation sub-feature. |
| NTR predecessor chain structure | A: linear chain (F856→F857→F858→F859→F860→F861), B: parallel branches (F856 → {F857, F860} in parallel → F861), C: simplified linear (F856→F857→F858→F859→F860→F861 with F862-F864 branching off F861) | C: simplified linear with F861 fan-out | Linear chain is safe and explicit; parallel branches complicate Task ordering in wbs-generator. F861 natural fan-out (F862, F863, F864 all depend on F861 independently) is architecturally sound. |
| Next Feature number increment | A: 867 (856 + 11 allocated IDs), B: 870 (round number buffer) | A: 867 | Increment to exactly 856 + 11 = 867. RESEARCH.md Issue 15 requires incrementing past all allocated IDs. No buffer needed; buffer only wastes ID space. |
| ComEquivalence failure tracking (C11) | A: create dedicated fixture-repair feature now, B: route as Mandatory Handoff from F856 with Task to create new feature during /run | B: Mandatory Handoff from F856 | Creating a new feature now (F867+) extends the allocated range unnecessarily and changes AC#8's expected value. The failures are pre-existing and blocked by missing fixtures; a Mandatory Handoff from F856 tracks them concretely without premature allocation. |
| Type assignment for NTR sub-features | A: Type: engine (C# engine), B: Type: erb (ERB-to-C# migration) | B: Type: erb | NTR files are ERB source being migrated to C# (producing .cs files in Era.Core). Verification uses headless game execution (`uEmuera.Headless`) per CLAUDE.md: "erb = ヘッドレスゲーム実行". Phase 24 sub-features were Type: engine (pure C# design); Phase 25 is migration from ERB source. |
| NTR_VISITOR.ERB placement | A: F861 (Visitor/Event Core, per architecture doc Task 1 grouping), B: F859 (NTR Extended Systems, per NTR Subsystems CALL graph grouping) | B: F859 | Architecture doc lists NTR_VISITOR.ERB in both Visitor group (Task 1) and NTR Subsystems table. CALL graph analysis shows NTR_VISITOR.ERB has stronger coupling to NTR/ files (NTR_UTIL calls, NTR state access) than to INTRUDER.ERB. F859 groups it with NTR_TAKEOUT and NTR陥落 (extended NTR support). Expected divergence to be noted in F865 Post-Phase Review. |

### Interfaces / Data Structures

No new interfaces or data structures defined by F855 itself. F855 produces markdown DRAFT files only.

**Sub-feature DRAFT minimum required sections** (for implementer reference):

`feature-856.md` (NTR Core/Util Foundation, Type: erb):
- `## Type: erb`
- Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Core/Util Foundation"
- Goal: Migrate NTR_UTIL.ERB, NTR_ACTION.ERB, NTR.ERB, NTR_OPTION.ERB to C# in `src/Era.Core/NTR/`. Produce NtrEngine.cs, NtrActionProcessor.cs, NtrUtilService.cs, NtrOptions.cs. Implement INtrCalculator concrete methods (CanAdvance, CanChangeRoute) consuming NTR_UTIL.ERB logic. Expand NtrParameters/Susceptibility with mutation methods. Add ExposureDegree Value Object. Wire NtrProgressionCreated domain event. Declare CalculateOrgasmCoefficient.
- Receives obligations: NtrParameters mutation, ExposureDegree VO, NtrProgressionCreated event, CalculateOrgasmCoefficient stub, OB-03/OB-04/OB-05/OB-10
- Dependencies: Predecessor → F855
- AC: debt-cleanup (not_matches TODO|FIXME|HACK), equivalence tests, zero-debt
- Architecture Task Coverage: `<!-- Architecture Task: NTR Subsystem Migration -->`

`feature-857.md` (NTR Behavioral Systems, Type: erb):
- `## Type: erb`
- Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Behavioral Systems"
- Goal: Migrate NTR_FRIENDSHIP.ERB, NTR_EXHIBITION.ERB, NTR_SEX.ERB to C#. Produce FriendshipSystem.cs, ExhibitionHandler.cs. Integrate MARK system state for CHK_NTR_SATISFACTORY.
- Dependencies: Predecessor → F856
- Architecture Task Coverage: `<!-- Architecture Task: NTR Subsystem Migration -->`

`feature-858.md` (NTR Master Scenes, Type: erb):
- `## Type: erb`
- Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Master Scenes"
- Goal: Migrate NTR_MASTER_SEX.ERB, NTR_MASTER_3P_SEX.ERB. Produce ThreesomeHandler.cs. Implement I3DArrayVariables GetDa/SetDa (OB-08, ROOM_SMELL_WHOSE_SAMEN).
- Dependencies: Predecessor → F857
- Architecture Task Coverage: `<!-- Architecture Task: NTR Subsystem Migration -->`

`feature-859.md` (NTR Extended Systems, Type: erb):
- `## Type: erb`
- Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Extended Systems"
- Goal: Migrate NTR_TAKEOUT.ERB, NTR_VISITOR.ERB, NTR_COMF416.ERB, NTR陥落イベント.ERB. Produce DateOutSystem.cs, CorruptionEvents.cs, NtrVisitorHandler.cs.
- Dependencies: Predecessor → F856
- Architecture Task Coverage: `<!-- Architecture Task: NTR Subsystem Migration -->`

`feature-860.md` (NTR Message Generator, Type: erb):
- `## Type: erb`
- Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Message Generator"
- Goal: Migrate NTR_MESSAGE.ERB (2,337 lines, 100+ functions). Produce NtrMessageGenerator.cs per SRP split mandate. Self-contained — no outbound NTR CALL dependencies.
- Dependencies: Predecessor → F856
- Architecture Task Coverage: `<!-- Architecture Task: NTR Subsystem Migration -->`

`feature-861.md` (Visitor/Event Core, Type: erb):
- `## Type: erb`
- Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — Visitor/Event Core"
- Goal: Migrate INTRUDER.ERB, EVENT_KOJO.ERB, EVENT_MESSAGE.ERB, EVENT_MESSAGE_ORGASM.ERB, EVENTCOMEND.ERB, EVENTTURNEND.ERB, 情事中に踏み込み.ERB. Produce VisitorAI.cs, ScheduleManager.cs, EventDispatcher.cs, EventHandlers.cs. Define IVisitorSystem interface. Assign VisitorId + NtrEventId strongly typed IDs. Resolve OB-02 (SANTA cosplay).
- Dependencies: Predecessor → F860
- Architecture Task Coverage: `<!-- Architecture Task: Visitor System Migration -->`, `<!-- Architecture Task: Event System Migration -->`, `<!-- Architecture Task: Turn-end Processing -->`

`feature-862.md` (EVENT_MESSAGE_COM Migration, Type: erb):
- `## Type: erb`
- Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — COM Event Message System"
- Goal: Migrate EVENT_MESSAGE_COM.ERB (4,599 lines) to ComEventMessageHandler.cs. NTR references limited to kojo callbacks (TRYCCALLFORM NTR_KOJO_*) and display helpers (NTR_NAME) — no NTR core dependency.
- Dependencies: Predecessor → F861
- Architecture Task Coverage: `<!-- Architecture Task: Event System Migration -->`

`feature-863.md` (Location Extensions, Type: erb):
- `## Type: erb`
- Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — Location Extensions"
- Goal: Migrate all 訪問者宅拡張/*.ERB (16 files). Produce VisitorHomeExtension.cs.
- Dependencies: Predecessor → F861
- Architecture Task Coverage: `<!-- Architecture Task: Location Extensions -->`

`feature-864.md` (AFFAIR_DISCLOSURE Migration, Type: erb):
- `## Type: erb`
- Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — AFFAIR_DISCLOSURE"
- Goal: Migrate INFO.ERB:693-795 (~103 lines). Produce AffairDisclosure.cs in `src/Era.Core/Events/`. Requires IEventContext defined in F861. References F378 "AFFAIR_DISCLOSURE Deferral Plan".
- Dependencies: Predecessor → F861
- Architecture Task Coverage: `<!-- Architecture Task: AFFAIR_DISCLOSURE Migration -->`

`feature-865.md` (Post-Phase Review Phase 25, Type: infra):
- `## Type: infra`
- Standard Post-Phase Review template; verifies architecture doc Phase 25 section integrity against F856–F864 deliverables
- Dependencies: Predecessors → F858, F859, F862, F863, F864
- Architecture Task Coverage: `<!-- Architecture Task: Post-Phase Review Phase 25 -->`

`feature-866.md` (Phase 26 Planning, Type: research):
- `## Type: research`
- Standard Phase Planning template; decomposes Phase 26 scope into sub-features
- Dependencies: Predecessor → F865
- Architecture Task Coverage: `<!-- Architecture Task: Phase 26 Planning -->`

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#9 Grep pattern `F8(5[6-9]\|6[0-9]\|7[0-9])` is broader than the allocated range F856-F866; would match F867-F879 spuriously if those are created later | AC Definition Table AC#9 | Update AC#9 pattern to `F8(5[6-9]\|6[0-6])` to match exactly F856-F866. Also update AC#1 Glob pattern from `feature-8{5[6-9],6[0-9],7[0-9]}.md` to `feature-8{5[6-9],6[0-6]}.md` to match exactly 11 allocated files. |
| AC#1 Expected `gte 8` and AC#5/AC#6 Expected `gte 6`: Technical Design defines 11 sub-features (9 implementation + 2 transition); AC#1 Expected should be updated to `gte 11`, AC#5/AC#6 to `gte 9` for full coverage verification | AC Definition Table AC#1, AC#5, AC#6 and AC Details | Update: AC#1 Expected from `gte 8` to `gte 11`; AC#5/AC#6 Expected from `gte 6` to `gte 9` (9 implementation sub-features: F856-F864). Update AC Details Derivation text accordingly. |
| AC#2 Grep pattern `feature-8{5[6-9],6[0-9],7[0-9]}.md` is broader than allocated and would need to find 8 Architecture Task markers across 11 files — Technical Design confirms 8 unique Tasks covered; pattern should be narrowed | AC Definition Table AC#2 | Update pattern to `feature-8{5[6-9],6[0-6]}.md` to match allocated range F856-F866. Expected `gte 8` is correct (8 unique Architecture Task markers). |
| AC#8 regex `86[0-9]` would match 860-869; the actual target value is 867; a more precise match `867` should be used or at minimum `86[6-9]` to exclude 860-865 as false-positive (if Next Feature number were left at 856 that would FAIL, but if partially incremented to 860-865 it would incorrectly PASS) | AC Definition Table AC#8 | Update Expected regex to `matches.*867` or tighten the AC Details to note Next Feature number = 867 specifically. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2, 3, 4, 5, 6, 7, 8, 9, 14, 15, 16, 17, 18, 19, 22, 23, 24, 26, 27, 30 | Create `pm/features/feature-856.md` [DRAFT]: NTR Core/Util Foundation (Type: erb). Include Philosophy ("Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Core/Util Foundation"), debt cleanup AC (not_matches TODO\|FIXME\|HACK), equivalence test AC, zero-debt AC, `<!-- Architecture Task: NTR Subsystem Migration -->` marker. Route F852 obligations (NtrParameters/Susceptibility mutation, ExposureDegree Value Object, NtrProgressionCreated domain event, CalculateOrgasmCoefficient declaration) and F829 obligations (OB-03 CLOTHES_ACCESSORY/INtrQuery, OB-04 IVariableStore 2D stubs, OB-05 NullMultipleBirthService, OB-10 CP-2 behavioral flow test). Include ComEquivalence Mandatory Handoff placeholder. Predecessor: F855. | | [x] |
| 2 | 1, 2, 3, 4, 5, 6, 7, 8, 9, 14, 15, 20, 25, 30, 31, 32, 34, 35 | Create `pm/features/feature-857.md` [DRAFT] (NTR Behavioral Systems, receives F852 CHK_NTR_SATISFACTORY/MARK system, `<!-- Architecture Task: NTR Subsystem Migration -->`), `pm/features/feature-858.md` [DRAFT] (NTR Master Scenes, receives F829 OB-08 I3DArrayVariables GetDa/SetDa, `<!-- Architecture Task: NTR Subsystem Migration -->`), `pm/features/feature-859.md` [DRAFT] (NTR Extended Systems, `<!-- Architecture Task: NTR Subsystem Migration -->`), `pm/features/feature-860.md` [DRAFT] (NTR Message Generator, `<!-- Architecture Task: NTR Subsystem Migration -->`). Each includes Philosophy, debt cleanup AC, equivalence test AC, zero-debt AC, and correct predecessor. | | [x] |
| 3 | 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 21, 30, 33, 36, 37, 38 | Create `pm/features/feature-861.md` [DRAFT] (Visitor/Event Core, receives F829 OB-02 SANTA cosplay, `<!-- Architecture Task: Visitor System Migration -->`, `<!-- Architecture Task: Event System Migration -->`, `<!-- Architecture Task: Turn-end Processing -->`), `pm/features/feature-862.md` [DRAFT] (EVENT_MESSAGE_COM Migration, `<!-- Architecture Task: Event System Migration -->`), `pm/features/feature-863.md` [DRAFT] (Location Extensions, `<!-- Architecture Task: Location Extensions -->`), `pm/features/feature-864.md` [DRAFT] (AFFAIR_DISCLOSURE Migration, `<!-- Architecture Task: AFFAIR_DISCLOSURE Migration -->`). Each includes Philosophy, debt cleanup AC, equivalence test AC, zero-debt AC. Create `pm/features/feature-865.md` [DRAFT] (Post-Phase Review Phase 25, Type: infra, `<!-- Architecture Task: Post-Phase Review Phase 25 -->`) and `pm/features/feature-866.md` [DRAFT] (Phase 26 Planning, Type: research, `<!-- Architecture Task: Phase 26 Planning -->`). | | [x] |
| 4 | 10, 11, 12, 13, 28, 29 | Register all 11 sub-features (F856–F866) in `pm/index-features.md` under a new `### Phase 25: AI & Visitor Systems` section with [DRAFT] status and correct Depends On values. Increment Next Feature number from 856 to 867. | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

**When to use `[I]`**:
- AC Expected value cannot be determined before implementation
- Task output depends on investigation or runtime discovery
- Downstream tasks depend on this task's concrete output

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-855.md Technical Design (sub-feature spec F856) | feature-856.md [DRAFT] |
| 2 | implementer | sonnet | feature-855.md Technical Design (sub-feature specs F857–F860) | feature-857.md, feature-858.md, feature-859.md, feature-860.md [DRAFT] |
| 3 | implementer | sonnet | feature-855.md Technical Design (sub-feature specs F861–F866) | feature-861.md, feature-862.md, feature-863.md, feature-864.md, feature-865.md, feature-866.md [DRAFT] |
| 4 | implementer | sonnet | list of F856–F866 metadata from Technical Design | index-features.md updated (11 new rows + Next Feature = 867) |

### Pre-conditions

- F854 is [DONE] (confirmed: Post-Phase Review Phase 24 completed)
- `pm/index-features.md` currently shows Next Feature number: 856
- No existing files `pm/features/feature-856.md` through `pm/features/feature-866.md`

### Execution Order

**Phase 1 — Create F856 first** (highest obligation density; sets Pattern for subsequent files):

1. Create `pm/features/feature-856.md` with ALL of:
   - Header: `# Feature 856: NTR Core/Util Foundation`
   - `## Status: [DRAFT]`
   - `## Type: erb`
   - `## Background / Philosophy`: "Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Core/Util Foundation"
   - Problem: NTR_UTIL.ERB provides foundational functions called by nearly every NTR file; must migrate before dependent systems
   - Goal: Migrate NTR_UTIL.ERB (1,500), NTR_ACTION.ERB (406), NTR.ERB (251), NTR_OPTION.ERB (504) to C# in `src/Era.Core/NTR/`. Produce NtrEngine.cs, NtrActionProcessor.cs, NtrUtilService.cs, NtrOptions.cs. Implement INtrCalculator concrete methods (CanAdvance, CanChangeRoute). Expand NtrParameters/Susceptibility with mutation methods. Add ExposureDegree Value Object. Wire NtrProgressionCreated domain event. Declare CalculateOrgasmCoefficient. Implement OB-03 CLOTHES_ACCESSORY/INtrQuery wiring, OB-04 IVariableStore 2D SAVEDATA stubs, OB-05 NullMultipleBirthService runtime impl, OB-10 CP-2 behavioral flow test.
   - `<!-- Architecture Task: NTR Subsystem Migration -->`
   - Dependencies: Predecessor → F855
   - Mandatory Handoff placeholder for 26 ComEquivalence test failures (if failures block test execution, create dedicated fixture-repair feature during /run)
   - Placeholder AC table including: equivalence test AC, debt cleanup AC (`not_matches` TODO|FIXME|HACK), zero-debt AC

**Phase 2 — Create F857, F858, F859, F860** (remaining NTR sub-features):

2. Create `pm/features/feature-857.md` (NTR Behavioral Systems):
   - Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Behavioral Systems"
   - Goal: Migrate NTR_FRIENDSHIP.ERB (918), NTR_EXHIBITION.ERB (469), NTR_SEX.ERB (1,150). Produce FriendshipSystem.cs, ExhibitionHandler.cs. Integrate MARK system state for CHK_NTR_SATISFACTORY (F852 obligation).
   - `<!-- Architecture Task: NTR Subsystem Migration -->`
   - Predecessor: F856

3. Create `pm/features/feature-858.md` (NTR Master Scenes):
   - Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Master Scenes"
   - Goal: Migrate NTR_MASTER_SEX.ERB (1,507), NTR_MASTER_3P_SEX.ERB (2,200). Produce ThreesomeHandler.cs. Implement I3DArrayVariables GetDa/SetDa for ROOM_SMELL_WHOSE (OB-08, confirmed 5 occurrences in NTR_MASTER_SEX.ERB).
   - `<!-- Architecture Task: NTR Subsystem Migration -->`
   - Predecessor: F857

4. Create `pm/features/feature-859.md` (NTR Extended Systems):
   - Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Extended Systems"
   - Goal: Migrate NTR_TAKEOUT.ERB (1,794), NTR_VISITOR.ERB (541), NTR_COMF416.ERB (272), NTR陥落イベント.ERB (206). Produce DateOutSystem.cs, CorruptionEvents.cs, NtrVisitorHandler.cs.
   - `<!-- Architecture Task: NTR Subsystem Migration -->`
   - Predecessor: F856

5. Create `pm/features/feature-860.md` (NTR Message Generator):
   - Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Message Generator"
   - Goal: Migrate NTR_MESSAGE.ERB (2,337 lines, 100+ functions). Produce NtrMessageGenerator.cs per SRP split mandate. Self-contained — no outbound NTR CALL dependencies.
   - `<!-- Architecture Task: NTR Subsystem Migration -->`
   - Predecessor: F856

**Phase 3 — Create F861, F862, F863, F864, F865, F866**:

6. Create `pm/features/feature-861.md` (Visitor/Event Core):
   - Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — Visitor/Event Core"
   - Goal: Migrate INTRUDER.ERB, EVENT_KOJO.ERB, EVENT_MESSAGE.ERB, EVENT_MESSAGE_ORGASM.ERB, EVENTCOMEND.ERB, EVENTTURNEND.ERB, 情事中に踏み込み.ERB. Produce VisitorAI.cs, ScheduleManager.cs, EventDispatcher.cs, EventHandlers.cs. Define IVisitorSystem interface. Assign VisitorId + NtrEventId strongly typed IDs. Resolve OB-02 (SANTA cosplay — engine-layer UI primitives now available).
   - `<!-- Architecture Task: Visitor System Migration -->`, `<!-- Architecture Task: Event System Migration -->`, `<!-- Architecture Task: Turn-end Processing -->`
   - Predecessor: F860

7. Create `pm/features/feature-862.md` (EVENT_MESSAGE_COM Migration):
   - Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — COM Event Message System"
   - Goal: Migrate EVENT_MESSAGE_COM.ERB (4,599 lines) to ComEventMessageHandler.cs. NTR references limited to kojo callbacks (TRYCCALLFORM NTR_KOJO_*) and display helpers (NTR_NAME) — no NTR core dependency.
   - `<!-- Architecture Task: Event System Migration -->`
   - Predecessor: F861

8. Create `pm/features/feature-863.md` (Location Extensions Migration):
   - Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — Location Extensions"
   - Goal: Migrate all 訪問者宅拡張/*.ERB (16 files: COMF460ex*.ERB 6 files, COMF461ex*.ERB 3 files, COMF462ex.ERB, COMF466ex*.ERB 3 files, COMF467ex.ERB, COMF46x.ERB, PLACEex.ERB). Produce VisitorHomeExtension.cs.
   - `<!-- Architecture Task: Location Extensions -->`
   - Predecessor: F861

9. Create `pm/features/feature-864.md` (AFFAIR_DISCLOSURE Migration):
   - Philosophy: "Pipeline Continuity — Phase 25: AI & Visitor Systems — AFFAIR_DISCLOSURE"
   - Goal: Migrate INFO.ERB:693-795 (~103 lines). Produce AffairDisclosure.cs in `src/Era.Core/Events/`. Requires IEventContext defined in F861. References F378 "AFFAIR_DISCLOSURE Deferral Plan".
   - `<!-- Architecture Task: AFFAIR_DISCLOSURE Migration -->`
   - Predecessor: F861

10. Create `pm/features/feature-865.md` (Post-Phase Review Phase 25):
    - Type: infra; standard Post-Phase Review template
    - `<!-- Architecture Task: Post-Phase Review Phase 25 -->`
    - Predecessors: F858, F859, F862, F863, F864 (all terminal implementation sub-features)

11. Create `pm/features/feature-866.md` (Phase 26 Planning):
    - Type: research; standard Phase Planning template
    - `<!-- Architecture Task: Phase 26 Planning -->`
    - Predecessor: F865

**Phase 4 — Index registration**:

12. Edit `pm/index-features.md`: add new `### Phase 25: AI & Visitor Systems` section with 11 rows (F856–F866), each with [DRAFT] status and correct Depends On.
13. Update `Next Feature number` from 856 to 867 in `pm/index-features.md`.

### Build Verification Steps

Research type: no build required. Verification is file existence (Glob) and content (Grep).

### Success Criteria

- 11 files exist: `pm/features/feature-856.md` through `pm/features/feature-866.md`
- Each of F856–F864 contains "Pipeline Continuity" in Philosophy section
- Each of F856–F864 contains "TODO|FIXME|HACK" in a debt cleanup AC
- 8 unique `<!-- Architecture Task: {Description} -->` markers distributed across F856–F866
- All 12 obligation/tracking keywords present in sub-feature files (NtrParameters, ExposureDegree, NtrProgressionCreated, CalculateOrgasmCoefficient, CHK_NTR_SATISFACTORY, OB-02, OB-03, OB-04, OB-05, OB-08, OB-10, ComEquivalence)
- `pm/index-features.md` contains "Post-Phase Review Phase 25" and "Phase 26 Planning"
- `pm/index-features.md` shows `Next Feature number.*: 867`
- `pm/index-features.md` contains 11 entries matching `F8(5[6-9]|6[0-6])`

### Error Handling

- If a file already exists (F856–F866): STOP → Report to user (ID collision)
- If index-features.md Next Feature number is not 856: STOP → Report to user (unexpected state)
- If obligation keyword cannot fit naturally into a sub-feature's Goal: STOP → Report to user

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-08T13:05 | PHASE_START | orchestrator | Phase 1 Initialize | F855 [REVIEWED]→[WIP] |
<!-- run-phase-1-completed -->
| 2026-03-08T13:07 | PHASE_END | orchestrator | Phase 2 Investigation | Artifacts confirmed: Next Feature=856, no F856-F866 exist |
<!-- run-phase-2-completed -->
| 2026-03-08 | START | implementer | Task 1 | - |
| 2026-03-08 | END | implementer | Task 1 | SUCCESS — feature-856.md [DRAFT] created |
| 2026-03-08 | START | implementer | Task 2 | - |
| 2026-03-08 | END | implementer | Task 2 | SUCCESS — feature-857.md, feature-858.md, feature-859.md, feature-860.md [DRAFT] created |
| 2026-03-08T13:20 | END | implementer | Task 3 | SUCCESS — feature-861.md through feature-866.md [DRAFT] created |
| 2026-03-08T13:22 | END | orchestrator | Task 4 | SUCCESS — 11 rows added to index-features.md, Next Feature=867 |
<!-- run-phase-4-completed -->
| 2026-03-08T13:24 | DEVIATION | Bash | ac_ops.py ac-check 855 | exit 1: UnicodeEncodeError cp932 codec — PRE-EXISTING env issue |
| 2026-03-08T13:25 | DEVIATION | Bash | ac_ops.py ac-check 855 (retry w/ UTF-8) | exit 1: 2 warnings about Implementation Contract count verification — informational prose lint, not AC structure errors |
| 2026-03-08T13:30 | PHASE_END | ac-tester | Phase 7 Verification | 38/38 ACs PASS |
<!-- run-phase-7-completed -->
| 2026-03-08T13:35 | PHASE_END | feature-reviewer | Phase 8 Post-Review | READY — quality OK, 8.2 skipped (no extensibility), 8.3 N/A |
<!-- run-phase-8-completed -->
| 2026-03-08T13:38 | DEVIATION | Bash | ac-static-verifier 855 | 17/38 — tool doesn't support brace expansion globs; ac-tester manually verified all 38 PASS |
| 2026-03-08T13:38 | DEVIATION | Bash | verify-logs.py 855 | exit 1: ERR:1|38 — stale from static verifier tool limitation |
| 2026-03-08T13:42 | PHASE_END | orchestrator | Phase 9 Report & Approval | User approved |
<!-- run-phase-9-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [resolved-applied] fc validation: [AC-CNT] 30 ACs exceeds research type 3-5 guideline. 30 ACs required by ac-validator: AND conjunctions split into individual ACs per TDD parseability. Justified: 8 Architecture Tasks + 12 obligations + 11 sub-features require granular verification. Precedent: F424/F437.
- [info] Phase1-DriftChecked: F833 (Related)
- [fix] Phase2-Review iter1: AC#7 | AC#7 single OR-pattern grep with gte 12 cannot verify all 12 keywords individually — split into AC#7 (F852, 5 AND checks) + AC#8 (F829+ComEquivalence, 7 AND checks)
- [fix] Phase2-Review iter1: AC#6/Goal Coverage row 7 | AC#6 only covers debt cleanup (TODO|FIXME|HACK), not equivalence tests (C2 Element 3) — added AC#11 for equivalence test coverage, updated Goal Coverage
- [fix] Phase2-Review iter1: F859 Architecture Task marker | F859 extended support files mislabeled as "NTR Action/Sex Migration" — added clarifying rationale to Architecture Task 2 marker
- [fix] Phase3-Maintainability iter2: AC#2 | AC#2 total count cannot verify 8 unique Tasks — split into 8 individual AND checks per Architecture Task number
- [fix] Phase3-Maintainability iter2: F406 status | F406 showed "Related" instead of [DONE] in Related Features and Dependencies tables
- [fix] Phase3-Maintainability iter2: Implementation Contract Phase 1 | Input description said F856-F864 but output is only F856
- [fix] Phase4-ACValidation iter3: AC table | Split AND conjunctions into individual ACs per ac-validator TDD parseability. AC#2→AC#2-9 (8 Tasks), AC#3→AC#10-11, AC#4→AC#12-13, AC#7→AC#16-20 (5 F852), AC#8→AC#21-27 (7 F829+ComEquivalence). Total: 11→30 ACs. Updated AC Details, Coverage, Goal Coverage, Tasks, deviation note.
- [fix] Phase2-Review iter4: F865 predecessor chain | F858, F859, F862, F863 were dead-end branches not connected to F865 (Post-Phase Review). Added all terminal sub-features as F865 predecessors.
- [fix] Phase7-FinalRefCheck iter5: AC#29 Details | F837 orphan link — added F837 reference to AC#29 Details (DRAFT Creation Checklist lesson)
- [resolved-applied] Phase2-Pending iter1: [AC-SSOT] Architecture Task number remapping — replaced number-based markers (Architecture Task N) with description-based markers aligned to architecture doc (phase-20-27-game-systems.md:836-844). Option (B) applied: AC#2-9 patterns and sub-feature markers now use descriptions (Visitor System Migration, Event System Migration, Turn-end Processing, NTR Subsystem Migration, Location Extensions, AFFAIR_DISCLOSURE Migration, Post-Phase Review Phase 25, Phase 26 Planning).
- [fix] Phase2-Review iter1: Predecessor Obligations table | Missing 6 F829 obligations (OB-02 through OB-10) despite Problem/Goal claiming 11 deferred obligations
- [fix] Phase2-Review iter1: AC#14/15 glob pattern | feature-8{5[6-9],6[0-6]} included transition features F865/F866 — changed to feature-8{5[6-9],6[0-4]} matching AC#30
- [fix] Phase2-Review iter1: Goal Coverage row 5 | Ordering claim not AC-verifiable — added note that ordering verified by predecessor chain structure
- [fix] Phase2-Review iter2: Goal Coverage row 5 + AC table | Added AC#31-33 for predecessor chain verification (F857→F856, F860→F856, F861→F860) to back functional subsystem ordering claim
- [fix] Phase4-ACCount iter3: AC Definition Table | Added deviation comment for 33 ACs exceeding 30 soft limit (justified: granular obligation routing + predecessor chain verification)
- [fix] Phase2-Review iter4: F406 description | "Phase 25 Task 7 scope" was semantically wrong — CalculateOrgasmCoefficient routed to F856 NTR Core, not Task 7 (Post-Phase Review)
- [fix] Phase2-Review iter1(restart): Group headers + Decomposition Rationale | Stale number-based Task references — Group 1 said "Architecture Tasks 1-3" for NTR (should be Task 4), Group 2 said "Task 4" for Visitor (should be Tasks 1-3). Decomposition Rationale rewritten with architecture doc descriptions.
- [fix] Phase2-Review iter2(restart): Key Decisions | NTR_VISITOR.ERB placement divergence from architecture doc Task 1 not documented — added Key Decision entry with CALL graph rationale
- [fix] Phase2-Review iter1: AC#31-33 / Goal Coverage 5 | Predecessor chain ACs covered only 3 of 10 relationships — added AC#34-38 for F858→F857, F859→F856, F862→F861, F863→F861, F864→F861. Updated deviation note (33→38), AC Coverage, Goal Coverage item 5
- [fix] Phase4-ACValidation iter3(restart): AC#1 | Glob+gte matcher invalid per AC Type Requirements — changed to Grep with ^# Feature heading pattern for gte-compatible counting

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

---

<!-- fc-phase-6-completed -->
## Links

[Predecessor: F854](feature-854.md) - Post-Phase Review Phase 24; unblocks F855
[Related: F849](feature-849.md) - Phase 24 Planning; pattern reference (produced F850-F855)
[Related: F852](feature-852.md) - Source of 5 deferred obligations routed to F855
[Related: F850](feature-850.md) - Source of 1 resolved cross-VO handoff
[Related: F829](feature-829.md) - Source of 6 deferred obligations (OB-02/03/04/05/08/10)
[Related: F378](feature-378.md) - AFFAIR_DISCLOSURE deferred to Phase 25 Task 6
[Related: F406](feature-406.md) - OrgasmProcessor.CalculateOrgasmRequirementCoefficient stub (routed to F856 NTR Core scope)
[Related: F830](feature-830.md) - Standalone: OB-06/OB-07 (NOT Phase 25 scope)
[Related: F833](feature-833.md) - Standalone: OB-09 (NOT Phase 25 scope)
[Related: F837](feature-837.md) - DRAFT Creation Checklist lesson (AC#9 rationale)
