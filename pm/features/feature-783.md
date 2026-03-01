# Feature 783: Phase 21 Planning

## Status: [DONE]
<!-- fl-reviewed: 2026-02-22T16:42:59Z -->

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

## Summary

Feature to create Features: Phase 21 Planning. Decompose Phase 21 into 12 implementation sub-features (F801-F812) plus 2 transition features (F813-F814) following the F647/F555 planning pattern. Sub-features: F801 Main Counter Core, F802 Main Counter Output, F803 Main Counter Source, F804 WC Counter Core, F805 WC Counter Source+Message, F806 WC Counter Message SEX, F807 WC Counter Message TEASE, F808 WC Counter Message ITEM+NTR, F809 COMABLE Core, F810 COMABLE Extended, F811 SOURCE Entry System, F812 SOURCE1 Extended, F813 Post-Phase Review Phase 21 (infra), F814 Phase 22 Planning (research).

---

## Phase Context

### Completed Phases (as of Phase 21 Planning)

Phase 0-19: DONE
Phase 20: WIP
Phase 21-34: TODO

**SSOT**: 各 Phase セクションの `**Phase Status**` (designs/phases/*.md)
**Note**: Mandatory Handoffs の引き継ぎ先は未完了 Phase のみ有効。

---

## Deferred Obligations

### Deferred Obligations from F782

**N+4 --unit deprecation obligation** (DEFERRED from F782 → F813):
N+4 --unit deprecation: NOT_FEASIBLE — trigger condition: C# migration functionally complete (kojo no longer requires ERB test runner; kojo testing pipeline dependency resolved). Tracking destination: F813 (Post-Phase Review Phase 21) — F813の/fc時にdeprecation追跡タスクとして具体化する

**Stale Stub Annotations: "See Phase 20 sibling feature" × 6** (DEFERRED from F782; ShopSystem 4 + ShopDisplay 2 = 6)

6 NotImplementedException stubs in Era.Core/Shop/ (ShopSystem.cs, ShopDisplay.cs) are annotated "See Phase 20 sibling feature". After F782 marks Phase 20 as DONE, these annotations become stale — the Phase 20 sibling features are all [DONE] and can no longer serve as destination references.

**Action Required**: When F783 undergoes /fc, include a Task to:
- Update 6 stubs annotated "See Phase 20 sibling feature" to reference concrete Phase 21+ feature IDs after decomposition

---

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. SSOT: `designs/full-csharp-architecture.md` Phase 21 section defines the scope. Same planning pattern as F647 (Phase 20) and F555 (Phase 19).

### Problem (Current Issue)

Phase 21 (~25,800 lines, 30 files) remains a monolithic scope because the architecture design grouped four functionally distinct subsystems (Main Counter, WC Counter, COMABLE, SOURCE) by shared "counter/action" theme without analyzing internal cohesion or coupling patterns (`phase-20-27-game-systems.md:109`). Call graph analysis reveals that SOURCE.ERB is the orchestration hub dispatching to both Counter variants (`SOURCE.ERB:21-40`), not a peer subsystem, and that WC Counter alone (~14,000 lines) exceeds the entire Phase 20 scope (4,173 lines). Additionally, 6 stale stub annotations in Era.Core/Shop/ reference "See Phase 20 sibling feature" but all 6 functions are defined outside Phase 21 scope -- mapping to Phases 3, 22, 25, and 26 -- with 2 already migrated (ILocationService.GetPlaceName, ICommonFunctions.HasVagina). Without decomposition, the scope is 6x Phase 20 with no actionable feature boundaries.

### Goal (What to Achieve)

Decompose Phase 21 into 12 implementation sub-features plus 2 mandatory transition features (Post-Phase Review Phase 21 and Phase 22 Planning), with call-graph-informed boundaries that respect the 4 subsystem structure and AC/Task volume limits. Update 6 stale stub annotations in Era.Core/Shop/ to reference correct target phases. Ensure each sub-feature inherits Phase 21 philosophy and includes debt resolution tasks, equivalence test tasks, and zero-debt ACs per architecture requirements (`phase-20-27-game-systems.md:232-239`).

<!-- Sub-Feature Requirements (architecture.md:4629-4637): /fc時に以下を反映すること
  1. Philosophy継承: Phase 21: Counter System
  2. Tasks: 負債解消 (TODO/FIXME/HACKコメント削除タスクを含む)
  3. Tasks: 等価性検証 (legacy実装との等価性テストを含む)
  4. AC: 負債ゼロ (技術負債ゼロを検証するACを含む)
-->

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is Phase 21 unactionable? | It is ~25,800 lines across 30 files with no decomposition -- 6x Phase 20 scope | `phase-20-27-game-systems.md:109` |
| 2 | Why is the scope so large? | It bundles 4 functionally distinct subsystems (Main Counter, WC Counter, COMABLE, SOURCE) that differ in coupling and complexity | `phase-20-27-game-systems.md:113-203` |
| 3 | Why were they bundled together? | Architecture design grouped them by shared "counter/action" theme without analyzing internal cohesion | `phase-20-27-game-systems.md:134-139` (SRP/Strategy Pattern note) |
| 4 | Why was internal cohesion not analyzed? | No call graph analysis was performed to reveal that SOURCE.ERB orchestrates both Counter and WC Counter, rather than being a peer | `SOURCE.ERB:21-40` (dispatches to EVENT_COUNTER or EVENT_WC_COUNTER) |
| 5 | Why (Root)? | The architecture decomposition was coarse-grained; empirical call graph analysis is needed to determine proper feature boundaries -- same root cause as F647 (Phase 20 line count estimates were 91% wrong) | F647 precedent; `phase-20-27-game-systems.md:109` |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Phase 21 is listed as TODO with undefined feature boundaries | Architecture grouped 4 subsystems by theme without call graph or cohesion analysis |
| Where | `phase-20-27-game-systems.md:107-245` (single Phase 21 section) | Architecture decomposition methodology (theme-based vs. coupling-based) |
| Fix | Manually split files into arbitrary groups | Analyze call graph, identify orchestration hubs, split by functional cohesion with interface boundaries |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Pattern precedent -- Phase 20 Planning (10 sub-features for 4,173 lines) |
| F555 | [DONE] | Pattern precedent -- Phase 19 Planning (15 sub-features) |
| F782 | [DONE] | Predecessor -- Post-Phase Review Phase 20 (deferred stale stub obligation) |
| F800 | [DONE] | Related -- GeneticsService extraction (IVariableStore dedup pattern) |
| F774-F781 | [DONE] | Phase 20 sub-features -- Era.Core migration patterns and interface conventions |
| F452 | [DONE] | Related -- Phase 12 COM system (read-shared by Phase 21 COMABLE) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Architecture spec exists | FEASIBLE | `phase-20-27-game-systems.md:107-245` defines scope, interfaces, deliverables |
| Pattern precedent available | FEASIBLE | F647 (Phase 20 Planning) and F555 (Phase 19 Planning) provide exact template |
| Call graph analyzable | FEASIBLE | ERB CALL/CALLFORM patterns verified via grep across all 30 files |
| Dependencies satisfied | FEASIBLE | F782 [DONE], F647 [DONE] -- both predecessors complete |
| Era.Core infrastructure ready | FEASIBLE | IVariableStore, ITEquipVariables, ICommonFunctions, ILocationService exist |
| Scope decomposable | FEASIBLE | 4 distinct subsystems identified with clear functional boundaries |
| Stale stubs resolvable | FEASIBLE | All 6 stubs mapped to correct target phases; 2 already migrated |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Phase 21 sub-features | HIGH | Creates 8-12 implementation features defining all Counter System migration work |
| Phase transition pipeline | HIGH | Creates Post-Phase Review Phase 21 (infra) and Phase 22 Planning (research) transition features |
| Era.Core/Shop stale stubs | MEDIUM | Updates 6 stub annotations to reference correct target phases (Phases 3/22/25/26) |
| index-features.md | MEDIUM | Adds Phase 21 section with all sub-feature registrations |
| Phase 22 readiness | LOW | Phase 22 Planning feature created as DRAFT, ready for future /fc |
| NTR_UTIL cross-phase contract | LOW | Documents 26 SOURCE_* function signatures that Phase 25 depends on |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Feature splitting: AC 5-12, Tasks 3-7 per sub-feature | `full-csharp-architecture.md:109-113` | Must split ~30 files into features respecting volume limits |
| Sub-Feature Requirements: Philosophy inheritance | `phase-20-27-game-systems.md:232-239` | Every sub-feature must inherit "Phase 21: Counter System" philosophy |
| Sub-Feature Requirements: Debt resolution tasks | `phase-20-27-game-systems.md:237` | Must include TODO/FIXME/HACK removal tasks in each sub-feature |
| Sub-Feature Requirements: Equivalence tests | `phase-20-27-game-systems.md:238` | Must include legacy equivalence tests in each sub-feature |
| Sub-Feature Requirements: Zero debt AC | `phase-20-27-game-systems.md:239` | Must include AC verifying zero technical debt |
| Mandatory transition features | `full-csharp-architecture.md:132-139` | Must create Post-Phase Review (infra) + Phase 22 Planning (research) |
| Phase 22 must run alone | `design-reference.md:537` | Phase 22 (Clothing) cannot run concurrently with other phases |
| Phase 12 COM read sharing | `design-reference.md:532` | Must not modify COM interfaces (IComRegistry) |
| COUNTER_CONBINATION typo | `phase-20-27-game-systems.md:165` | Must correct to "Combination" in C# migration |
| SOURCE.ERB is main orchestrator | `SOURCE.ERB:4-41` | Creates interface boundary; SOURCE feature should be sequenced appropriately |
| Next Feature number starts at 801 | `index-features.md:178` | Sub-features allocated from F801 onward |
| Stale stubs map to utilities, not Counter | Investigation (3/3 consensus) | 4 of 6 stubs are outside Phase 21 scope; 2 already migrated |
| 37 FIXME comments are benign boilerplate | `SOURCE*.ERB` (all "function description") | C# migration replaces logic; no functional debt |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| WC Counter messages (10,480+ lines) too large for single feature | HIGH | HIGH | Split WC Counter into 2-3 sub-features by action category (SEX, TEASE+ITEM, NTR) |
| SOURCE.ERB dual-orchestration creates unclear ownership | MEDIUM | HIGH | Define SOURCE as entry point feature with clear ICounterSystem interface boundary |
| COMABLE 109+ individual check functions create testing complexity | MEDIUM | MEDIUM | Group by command number range; use Strategy Pattern per architecture spec |
| NTR_UTIL.ERB (Phase 25) directly calls 26 SOURCE_* functions | MEDIUM | LOW | Document interface contract; signatures must be preserved across migration |
| Cross-phase deps on CLOTHES_SETTING_TRAIN (Phase 22) | MEDIUM | MEDIUM | Use IClothingSystem stub methods; Phase 22 runs alone after Phase 21 |
| Stale stub resolution may be partially blocked | HIGH | LOW | Update annotations to reference Phase numbers; 2 stubs already resolvable |
| Sub-feature count exceeds AC/Task volume limits | LOW | LOW | Proportional splitting based on F647 precedent; adjust during implementation |
| External callers (INFO.ERB, COMF*.ERB) need interface stability | LOW | HIGH | Define ICounterSystem and IComAvailabilityChecker interfaces early |

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Philosophy inheritance in all sub-features | `phase-20-27-game-systems.md:236` | AC must verify grep for "Phase 21" philosophy string in all created features |
| C2 | Debt resolution tasks in sub-features | `phase-20-27-game-systems.md:237` | AC must verify TODO/FIXME/HACK removal tasks exist in sub-feature specs |
| C3 | Equivalence test tasks in sub-features | `phase-20-27-game-systems.md:238` | AC must verify equivalence test tasks exist in sub-feature specs |
| C4 | Zero debt AC in sub-features | `phase-20-27-game-systems.md:239` | AC must verify zero-debt AC exists in sub-feature specs |
| C5 | Post-Phase Review type: infra | `full-csharp-architecture.md:136` | AC must verify Type field is "infra" for Post-Phase Review feature |
| C6 | Phase 22 Planning type: research | `full-csharp-architecture.md:136` | AC must verify Type field is "research" for Phase 22 Planning feature |
| C7 | Stale stubs updated | `feature-783.md:53-54` | AC must verify "See Phase 20 sibling feature" annotations are replaced |
| C8 | index-features.md updated | F647 pattern | AC must verify Phase 21 section exists in index with all sub-features |
| C9 | Research type: 3-5 AC guideline | RESEARCH.md | AC count for F783 itself should be 3-5 |
| C10 | Next Feature number incremented | RESEARCH.md Issue 15 | AC must verify Next Feature number is past all allocated IDs |
| C11 | ICounterSystem and IComAvailabilityChecker mandatory | `phase-20-27-game-systems.md:120-132` | Architecture-mandated interfaces must be assigned to specific sub-features |
| C12 | Interface Dependency Scan: all needed variable interfaces exist | Investigation (3/3 consensus) | No new variable interfaces required; IVariableStore, ITEquipVariables complete |
| C13 | Interface Dependency Scan: ILocationService already provides GetPlaceName | `Era.Core/Common/LocationSystem.cs:144` | GETPLACENAME stub can be resolved without new interface |
| C14 | Interface Dependency Scan: ICommonFunctions.HasVagina exists | `Era.Core/Interfaces/ICommonFunctions.cs:10-12` | HAS_VAGINA stub can be resolved without new interface |

### Constraint Details

**C1: Philosophy Inheritance**
- **Source**: `phase-20-27-game-systems.md:232-239` Sub-Feature Requirements
- **Verification**: Grep all created feature files for "Phase 21" in Philosophy section
- **AC Impact**: Each sub-feature DRAFT must contain philosophy inheriting "Phase 21: Counter System"

**C2: Debt Resolution Tasks**
- **Source**: `phase-20-27-game-systems.md:237` mandatory debt cleanup
- **Verification**: Grep sub-feature specs for TODO/FIXME/HACK task entries
- **AC Impact**: ac-designer should verify task tables include debt resolution rows

**C3: Equivalence Test Tasks**
- **Source**: `phase-20-27-game-systems.md:238` mandatory equivalence testing
- **Verification**: Grep sub-feature specs for equivalence test task entries
- **AC Impact**: ac-designer should verify task tables include equivalence test rows

**C4: Zero Debt AC**
- **Source**: `phase-20-27-game-systems.md:239` mandatory zero-debt verification
- **Verification**: Grep sub-feature specs for zero-debt AC entries
- **AC Impact**: Each sub-feature must have an AC that verifies zero technical debt post-migration

**C7: Stale Stub Update**
- **Source**: F782 deferred obligation (`feature-783.md:49-54`)
- **Verification**: Grep `Era.Core/Shop/` for "See Phase 20 sibling feature" should return 0 after update
- **AC Impact**: Task must update all 6 stubs with correct phase references; 2 can be fully resolved (GETPLACENAME -> ILocationService, HAS_VAGINA -> ICommonFunctions.HasVagina)

**C10: Next Feature Number Increment**
- **Source**: RESEARCH.md Issue 15 -- prevents ID collision
- **Verification**: Grep index-features.md for "Next Feature number" value
- **AC Impact**: Must be incremented past the last allocated feature ID

**C11: Architecture-Mandated Interfaces**
- **Source**: `phase-20-27-game-systems.md:120-132` interface definitions
- **Verification**: Sub-feature specs must assign ICounterSystem and IComAvailabilityChecker creation
- **AC Impact**: At least one sub-feature must own each mandatory interface creation

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (decomposed this feature) |
| Predecessor | F782 | [DONE] | Post-Phase Review Phase 20 (deferred stale stub obligation to this feature) |
| Related | F555 | [DONE] | Phase 19 Planning (pattern precedent: 15 sub-features) |
| Related | F800 | [DONE] | GeneticsService extraction (IVariableStore dedup pattern) |
| Related | F452 | [DONE] | Phase 12 COM system (read-shared by Phase 21 COMABLE via IComRegistry) |
| Successor | Phase 21 sub-features (F801+) | N/A | Created by this feature |

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

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Each phase completion triggers next phase planning" | Must create transition features (Post-Phase Review Phase 21 + Phase 22 Planning) | AC#3 |
| "continuous development pipeline" | All Phase 21 scope must be covered by sub-features with no gaps | AC#1, AC#2, AC#6, AC#9, AC#11, AC#12, AC#13 |
| "clear phase boundaries" | Sub-features must have clear scope boundaries with call-graph-informed grouping | AC#1, AC#8 |
| "documented transition points" | Sub-features registered in index-features.md Phase 21 section | AC#2, AC#5 |
| "Same planning pattern as F647/F555" | Follow F647 pattern: decomposition, index registration, transition features, stale stub resolution | AC#1, AC#2, AC#3, AC#4, AC#5 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Implementation sub-feature file count matches Goal commitment | file | Glob(pm/features/feature-80[1-9].md, pm/features/feature-81[0-2].md) | gte | 12 | [x] |
| 2 | Index updated with Phase 21 section and all sub-features individually registered | file | Grep(pm/index-features.md) | gte | 14 | [x] |
| 3 | Transition features created with correct types | file | Grep(pm/index-features.md) + Glob(pm/features/feature-813.md, feature-814.md) + Grep(pm/features/feature-813.md) + Grep(pm/features/feature-814.md) | contains | "Post-Phase Review Phase 21" AND "Phase 22 Planning" in index; files exist; "Type: infra" in F813 AND "Type: research" in F814 | [x] |
| 4 | Stale stub annotations resolved | code | Grep(Era.Core/Shop/) | not_matches | "See Phase 20 sibling feature" | [x] |
| 5 | Next Feature number incremented past all allocated IDs | file | Grep(pm/index-features.md) | matches | "Next Feature number.*815" | [x] |
| 6 | Philosophy inheritance (C1): "Phase 21: Counter System" in all implementation sub-features | file | Grep(pm/features/feature-80[1-9].md, feature-81[0-2].md) | gte | 12 | [x] |
| 7 | Architecture-mandated interfaces assigned (C11) | file | Grep(pm/features/feature-801.md) + Grep(pm/features/feature-809.md) | contains | "ICounterSystem (owner)" in F801 AND "IComAvailabilityChecker (owner)" in F809 | [x] |
| 8 | 4-subsystem structure preserved with SOURCE orchestrator assignment | file | Grep(pm/features/feature-80[1-9].md, feature-81[0-2].md) + Grep(pm/features/feature-811.md) | contains | "Main Counter" AND "WC Counter" AND "COMABLE" AND "SOURCE" AND "SOURCE.ERB" in F811 | [x] |
| 9 | All 30 Phase 21 ERB source files assigned to sub-feature DRAFTs (scope exhaustiveness) | file | 30× Grep(each ERB filename, pm/features/feature-80[1-9].md\|feature-81[0-2].md) | contains | Each of 30 ERB filenames appears in at least 1 DRAFT (compound: 30 checks, all must pass) | [x] |
| 10 | Era.Core build succeeds after stale stub delegation changes | build | Bash(dotnet build Era.Core/) | succeeds | Build completes with 0 errors | [x] |
| 11 | Debt resolution task (C2): "Remove TODO/FIXME/HACK" in all implementation sub-features | file | Grep(pm/features/feature-80[1-9].md, feature-81[0-2].md) | gte | 12 | [x] |
| 12 | Equivalence test task (C3): "Write equivalence tests" in all implementation sub-features | file | Grep(pm/features/feature-80[1-9].md, feature-81[0-2].md) | gte | 12 | [x] |
| 13 | Zero debt AC (C4): "zero technical debt" in all implementation sub-features | file | Grep(pm/features/feature-80[1-9].md, feature-81[0-2].md) | gte | 12 | [x] |

### AC Details

**AC#1: Implementation sub-feature file count matches Goal commitment**
- **Test**: `Glob("pm/features/feature-80[1-9].md")` + `Glob("pm/features/feature-81[0-2].md")` — count total matching files >= 12
- **Expected**: All 12 implementation sub-feature files (F801-F812) exist on disk, confirming the full decomposition count per Goal commitment
- **Rationale**: Satisfies Goal Item 1 commitment to 12 implementation sub-features. Uses Glob (file existence) instead of Grep (content) to independently verify file count — AC#6 separately verifies content quality. gte 12 enforces the definitive allocation determined by Technical Design (3 Main Counter + 5 WC Counter + 2 COMABLE + 2 SOURCE = 12)

**AC#2: Index updated with Phase 21 section and all sub-features individually registered**
- **Test**: `Grep("F80[1-9]|F81[0-4]", "pm/index-features.md")` count >= 14
- **Expected**: At least 14 matches for F801-F814 individual feature entries in the Phase 21 section of index-features.md (12 implementation features F801-F812 + F813 + F814 transition features)
- **Rationale**: Satisfies C8 (index-features.md updated). Verifies not only that the Phase 21 section exists, but that all individual sub-features F801-F814 are registered. F647 pattern precedent: Phase 20 section listed all F774-F783 entries individually

**AC#3: Transition features created with correct types**
- **Test**: `Grep("Post-Phase Review Phase 21", "pm/index-features.md")` + `Grep("Phase 22 Planning", "pm/index-features.md")` + `Glob("pm/features/feature-8*.md")` to verify DRAFT files exist for both transition features + `Grep("Type: infra", "pm/features/feature-813.md")` + `Grep("Type: research", "pm/features/feature-814.md")` to verify Type fields
- **Expected**: (1) "Post-Phase Review Phase 21" appears in index-features.md. (2) "Phase 22 Planning" appears in index-features.md. (3) Both transition feature DRAFT files exist on disk. (4) feature-813.md contains "Type: infra" (C5). (5) feature-814.md contains "Type: research" (C6)
- **Rationale**: Satisfies C5 (Post-Phase Review type: infra — verified by Type field check), C6 (Phase 22 Planning type: research — verified by Type field check). Per RESEARCH.md Issue 16, both file existence (Glob) AND index registration (Grep) must be verified. Type field verification per C5/C6 Constraint Details. Transition features are mandatory per `full-csharp-architecture.md:132-139`
- **Note**: All five checks (two index Grep + Glob file existence + two Type field Grep) must pass (AND conjunction)

**AC#4: Stale stub annotations resolved**
- **Test**: `Grep("See Phase 20 sibling feature", "Era.Core/Shop/")`
- **Expected**: Zero matches -- all 6 stale annotations in ShopSystem.cs (lines 387, 433, 436, 439) and ShopDisplay.cs (lines 435, 437) have been updated to reference correct target phases or resolved with existing interfaces
- **Rationale**: Satisfies C7 (stale stubs updated). F782 deferred obligation requires updating all 6 "See Phase 20 sibling feature" annotations. Per C13, GETPLACENAME can reference ILocationService. Per C14, HAS_VAGINA can reference ICommonFunctions.HasVagina. Remaining 4 stubs must reference correct Phase numbers (3, 22, 25, or 26 per investigation)

**AC#5: Next Feature number incremented past all allocated IDs**
- **Test**: `Grep("Next Feature number", "pm/index-features.md")` → verify value matches regex `815`
- **Expected**: "Next Feature number" line contains exactly 815, confirming increment past all 14 allocated sub-feature IDs (F801-F814)
- **Rationale**: Satisfies C10 (Next Feature number incremented). Prevents ID collision on subsequent `/fc` invocations. Current value is 801; after allocating F801-F814 (14 features), the Next Feature number is deterministically 815 per Technical Design. Exact match (not range) ensures the correct allocation count is enforced

**AC#6: Philosophy inheritance (C1)**
- **Test**: `Grep("Phase 21: Counter System", "pm/features/feature-80[1-9].md|feature-81[0-2].md", output_mode="files_with_matches")` >= 12
- **Expected**: All 12 implementation sub-features (F801-F812) contain "Phase 21: Counter System" philosophy string
- **Rationale**: Satisfies C1 per `phase-20-27-game-systems.md:232-239`. Exact philosophy string ensures inheritance

**AC#11: Debt resolution task (C2)**
- **Test**: `Grep("Remove TODO/FIXME/HACK", "pm/features/feature-80[1-9].md|feature-81[0-2].md", output_mode="files_with_matches")` >= 12
- **Expected**: All 12 implementation sub-features contain debt resolution task stub
- **Rationale**: Satisfies C2. Exact DRAFT template task description prevents false positives

**AC#12: Equivalence test task (C3)**
- **Test**: `Grep("Write equivalence tests", "pm/features/feature-80[1-9].md|feature-81[0-2].md", output_mode="files_with_matches")` >= 12
- **Expected**: All 12 implementation sub-features contain equivalence test task stub
- **Rationale**: Satisfies C3. Exact DRAFT template task description prevents false positives

**AC#13: Zero debt AC (C4)**
- **Test**: `Grep("zero technical debt", "pm/features/feature-80[1-9].md|feature-81[0-2].md", output_mode="files_with_matches")` >= 12
- **Expected**: All 12 implementation sub-features contain zero-debt AC stub
- **Rationale**: Satisfies C4. Exact DRAFT template AC description prevents false positives

**AC#7: Architecture-mandated interfaces assigned (C11)**
- **Test**: `Grep("ICounterSystem (owner)", "pm/features/feature-801.md")` + `Grep("IComAvailabilityChecker (owner)", "pm/features/feature-809.md")` -- both must match
- **Expected**: feature-801.md contains "ICounterSystem (owner)" (Main Counter Core owns this interface) and feature-809.md contains "IComAvailabilityChecker (owner)" (COMABLE Core owns this interface). Both Grep checks must pass
- **Rationale**: Satisfies C11 (architecture-mandated interfaces assigned). Per `phase-20-27-game-systems.md:120-132`, ICounterSystem and IComAvailabilityChecker must be pre-assigned in specific DRAFT files so implementers inherit the interface ownership without requiring a separate audit pass. Using "(owner)" suffix distinguishes ownership from mere reference in implementer features

**AC#8: 4-subsystem structure preserved with SOURCE orchestrator assignment**
- **Test**: `Grep("Main Counter", "pm/features/feature-80[1-9].md|feature-81[0-2].md")`, `Grep("WC Counter", "pm/features/feature-80[1-9].md|feature-81[0-2].md")`, `Grep("COMABLE", "pm/features/feature-80[1-9].md|feature-81[0-2].md")`, `Grep("SOURCE", "pm/features/feature-80[1-9].md|feature-81[0-2].md")` -- all four subsystem names must appear. Additionally `Grep("SOURCE.ERB", "pm/features/feature-811.md")` must match to verify SOURCE.ERB is assigned to F811 (entry-point orchestrator)
- **Expected**: 4 distinct subsystem groupings exist: "Main Counter" appears in at least one file (F801-F803), "WC Counter" appears in at least one file (F804-F808), "COMABLE" appears in at least one file (F809-F810), "SOURCE" appears in at least one file (F811-F812). F811 feature contains "SOURCE.ERB" confirming entry-point orchestrator assignment. Satisfies Goal Item 3
- **Rationale**: Verifies that the call-graph-informed 4-subsystem decomposition from `phase-20-27-game-systems.md:113-203` is reflected in the created DRAFT files, and that SOURCE.ERB as orchestration hub is correctly assigned to F811 rather than being buried in a combined feature

**AC#9: All 30 Phase 21 ERB source files assigned to sub-feature DRAFTs (scope exhaustiveness)**
- **Test**: For each of the following 30 ERB filenames, Grep the filename across `pm/features/feature-80[1-9].md|feature-81[0-2].md` and verify at least one match:
  COUNTER_SELECT.ERB, COUNTER_ACTABLE.ERB, COUNTER_MESSAGE.ERB, COUNTER_POSE.ERB, COUNTER_REACTION.ERB, COUNTER_PUNISHMENT.ERB, COUNTER_CONBINATION.ERB, COUNTER_SOURCE.ERB, TOILET_COUNTER.ERB, TOILET_COUNTER_ACTABLE.ERB, TOILET_COUNTER_COMBINATION.ERB, TOILET_COUNTER_POSE.ERB, TOILET_COUNTER_PUNISHMENT.ERB, TOILET_COUNTER_REACTION.ERB, TOILET_EVENT_KOJO.ERB, TOILET_COUNTER_SOURCE.ERB, TOILET_COUNTER_MESSAGE.ERB, TOILET_COUNTER_MESSAGE_SEX.ERB, TOILET_COUNTER_MESSAGE_TEASE.ERB, TOILET_COUNTER_MESSAGE_ITEM.ERB, TOILET_COUNTER_MESSAGE_NTR.ERB, COMABLE.ERB, COMABLE2.ERB, COMABLE_300.ERB, COMABLE_400.ERB, SOURCE.ERB, SOURCE_CALLCOM.ERB, SOURCE_POSE.ERB, SOURCE_SHOOT.ERB, SOURCE1.ERB
- **Expected**: All 30 ERB filenames from Technical Design Phase 3 Subsystem Grouping table appear in at least one sub-feature DRAFT. No Phase 21 ERB file is left unassigned.
- **Rationale**: Satisfies Philosophy "continuous development pipeline" requirement that "All Phase 21 scope must be covered by sub-features with no gaps." AC#1 verifies file count; AC#9 verifies content coverage — preventing DRAFTs that exist but reference wrong/incomplete source files. Uniqueness of assignment (each ERB file in exactly one DRAFT) is enforced by the Technical Design Phase 3 Subsystem Grouping table, which is the SSOT for file-to-feature mapping; AC#9 verifies coverage, not uniqueness.

**AC#10: Era.Core build succeeds after stale stub delegation changes**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'`
- **Expected**: Build succeeds with exit code 0 and 0 errors. Confirms that GETPLACENAME → ILocationService.GetPlaceName and HAS_VAGINA → ICommonFunctions.HasVagina delegation changes compile correctly
- **Rationale**: Task#2 modifies runtime C# code, not just annotations. AC#4 only verifies old string is removed; AC#10 verifies the replacement code actually compiles. Without this, a typo in delegation (e.g., wrong method name) would pass AC#4 but fail at build time

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Decompose Phase 21 into 12 implementation sub-features | AC#1, AC#6, AC#9, AC#11, AC#12, AC#13 |
| 2 | Plus 2 mandatory transition features (Post-Phase Review Phase 21 + Phase 22 Planning) | AC#3 |
| 3 | Call-graph-informed boundaries respecting 4 subsystem structure and volume limits | AC#8 |
| 4 | Update 6 stale stub annotations in Era.Core/Shop/ | AC#4, AC#10 |
| 5 | Each sub-feature inherits philosophy, debt resolution, equivalence tests, zero-debt ACs | AC#6, AC#11, AC#12, AC#13 |
| 6 | Index-features.md updated with Phase 21 section | AC#2 |
| 7 | Next Feature number incremented past allocated IDs | AC#5 |
| 8 | Architecture-mandated interfaces assigned to specific sub-features (C11) | AC#7 |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The implementation follows the F647 planning pattern adapted for Phase 21's 6x larger scope: **Empirical Metrics → Call Graph Analysis → Volume-Informed Subsystem Grouping → DRAFT File Creation → Stale Stub Resolution → Index/Registry Updates**.

The 4-subsystem architecture (Main Counter, WC Counter, COMABLE, SOURCE) maps to distinct feature groups. The critical volume problem is WC Counter at 14,128 lines (vs. Phase 20's total of 4,173 lines), which requires splitting into 5 sub-features (core + source/message + 3 message categories). COMABLE at 4,998 lines requires 2 sub-features. SOURCE at 4,359 lines requires 2 sub-features. Main Counter at 2,382 lines plus COUNTER_SOURCE (920 lines) requires 3 sub-features. This yields 12 implementation sub-features plus 2 transition features = 14 total sub-features allocated as F801–F814.

**Phase 1: Verify Empirical File Metrics**
- Confirm actual line counts via `wc -l` for all 30 Phase 21 ERB files
- Baseline empirical counts (pre-confirmed):
  - Main Counter: 2,382 lines (8 files)
  - WC Counter: 14,128 lines (13 files including TOILET_EVENT_KOJO.ERB)
  - COMABLE: 4,998 lines (4 files)
  - SOURCE: 4,359 lines (5 files)
  - Grand total: ~25,867 lines
- Document in `.tmp/phase21-metrics.txt`

**Phase 2: Call Graph Analysis**
- Use `Grep("^@", file)` to map function definitions per ERB file
- Use `Grep("CALL|TRYCALL|CALLFORM", file)` to map cross-file calls
- Identify external callers: INFO.ERB, COMF*.ERB, NTR_UTIL.ERB (26 SOURCE_* function signatures)
- Confirm SOURCE.ERB as entry-point orchestrator dispatching to EVENT_COUNTER or EVENT_WC_COUNTER
- Document ICounterSystem interface boundary (owned by F801) and IComAvailabilityChecker (owned by F809)

**Phase 3: Subsystem Grouping (Feature Allocation)**

| Feature | Subsystem | Source Files | Approx Lines |
|---------|-----------|--------------|:------------:|
| F801 | Main Counter Core | COUNTER_SELECT.ERB, COUNTER_ACTABLE.ERB | 599 |
| F802 | Main Counter Output | COUNTER_MESSAGE.ERB, COUNTER_POSE.ERB, COUNTER_REACTION.ERB, COUNTER_PUNISHMENT.ERB, COUNTER_CONBINATION.ERB | 863 |
| F803 | Main Counter Source | COUNTER_SOURCE.ERB | 920 |
| F804 | WC Counter Core | TOILET_COUNTER.ERB, TOILET_COUNTER_ACTABLE.ERB, TOILET_COUNTER_COMBINATION.ERB, TOILET_COUNTER_POSE.ERB, TOILET_COUNTER_PUNISHMENT.ERB, TOILET_COUNTER_REACTION.ERB, TOILET_EVENT_KOJO.ERB | 1,741 |
| F805 | WC Counter Source + Message Core | TOILET_COUNTER_SOURCE.ERB, TOILET_COUNTER_MESSAGE.ERB | 1,907 |
| F806 | WC Counter Message SEX | TOILET_COUNTER_MESSAGE_SEX.ERB | 3,518 |
| F807 | WC Counter Message TEASE | TOILET_COUNTER_MESSAGE_TEASE.ERB | 3,483 |
| F808 | WC Counter Message ITEM + NTR | TOILET_COUNTER_MESSAGE_ITEM.ERB, TOILET_COUNTER_MESSAGE_NTR.ERB | 3,479 |
| F809 | COMABLE Core | COMABLE.ERB, COMABLE2.ERB | 3,897 |
| F810 | COMABLE Extended | COMABLE_300.ERB, COMABLE_400.ERB | 1,101 |
| F811 | SOURCE Entry System | SOURCE.ERB, SOURCE_CALLCOM.ERB, SOURCE_POSE.ERB, SOURCE_SHOOT.ERB | 2,400 |
| F812 | SOURCE1 Extended | SOURCE1.ERB | 1,959 |
| F813 | Post-Phase Review Phase 21 | (infra transition) | - |
| F814 | Phase 22 Planning | (research transition) | - |

**Next Feature number after allocation: 815**

**Phase 4: Stale Stub Resolution**

| Stub | File | Current Text | Resolution | Target |
|------|------|-------------|------------|--------|
| `子供訪問_設定` | ShopSystem.cs:387 | "See Phase 20 sibling feature" | Reference Phase 22 (State Systems) | `"KODOMOHOSYUU_SETTING - See Phase 22 (State Systems)"` |
| `GETPLACENAME` | ShopSystem.cs:433 | "See Phase 20 sibling feature" | Delegate to ILocationService.GetPlaceName (C13) | Replace throw with `_locationService.GetPlaceName(placeId)` |
| `NTR_NAME` | ShopSystem.cs:436 | "See Phase 20 sibling feature" | Reference Phase 25 (NTR Utility) | `"NTR_NAME - See Phase 25 (NTR Utility)"` |
| `CHILDREM_CHECK` | ShopSystem.cs:439 | "See Phase 20 sibling feature" | Reference Phase 3 (pregnancy check) | `"CHILDREM_CHECK - See Phase 3 (Pregnancy System)"` |
| `HAS_VAGINA` | ShopDisplay.cs:435 | "See Phase 20 sibling feature" | Delegate to ICommonFunctions.HasVagina (C14) | Replace throw with `_commonFunctions.HasVagina(chr)` |
| `GET_MARK_LEVEL_OWNERNAME` | ShopDisplay.cs:437 | "See Phase 20 sibling feature" | Reference Phase 26 (Mark System) | `"GET_MARK_LEVEL_OWNERNAME - See Phase 26 (Mark System)"` |

2 stubs resolve immediately (GETPLACENAME → ILocationService, HAS_VAGINA → ICommonFunctions). 4 stubs transition from stale "Phase 20 sibling feature" to concrete phase references. All 6 pass AC#4 (zero matches for "See Phase 20 sibling feature").

**Phase 5: Create DRAFT Files**
- Generate feature-801.md through feature-814.md (14 files)
- Each implementation DRAFT (F801–F812) includes:
  - Type: `erb`
  - Background inheriting "Phase 21: Counter System" philosophy (C1)
  - Scope Reference listing source ERB files and function ranges
  - Task stubs: debt resolution (C2), equivalence tests (C3)
  - AC stub: zero-debt AC (C4)
  - Interface assignment: F801 owns ICounterSystem; F809 owns IComAvailabilityChecker (C11)
- F813 (Post-Phase Review): type `infra` (C5)
- F814 (Phase 22 Planning): type `research` (C6)

**Phase 6: Index and Registry Updates**
- Add `### Phase 21: Counter System` section to `index-features.md` listing F801–F814
- Update "Next Feature number" to 815 (C10)
- Rename existing `### Phase 20: Equipment & Shop Systems` entry for F783 to move F783 out of Phase 20 section (F783 is a Phase 21 planning feature, currently listed under Phase 20 due to index structure)

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Phase 3 grouping produces 12 implementation sub-features (F801-F812). Glob(feature-80[1-9].md) + Glob(feature-81[0-2].md) returns >= 12 files on disk. gte 12 satisfied (12 files) |
| 2 | Phase 6 creates `### Phase 21: Counter System` section in index-features.md listing all F801-F814 entries individually. Grep("F80[1-9]|F81[0-4]", index-features.md) returns >= 14 matches for F801-F814 individual registrations |
| 3 | Phase 5 creates F813 (type: infra, "Post-Phase Review Phase 21") and F814 (type: research, "Phase 22 Planning") as DRAFT files plus index registration. All five checks pass: Grep("Post-Phase Review Phase 21"), Grep("Phase 22 Planning"), Glob(feature-8*.md) for both files, Grep("Type: infra", feature-813.md), Grep("Type: research", feature-814.md) |
| 4 | Phase 4 resolves all 6 stale "See Phase 20 sibling feature" annotations: 2 via interface delegation (GETPLACENAME, HAS_VAGINA), 4 via phase number references (子供訪問_設定→Phase 22, NTR_NAME→Phase 25, CHILDREM_CHECK→Phase 3, GET_MARK_LEVEL_OWNERNAME→Phase 26). Grep("See Phase 20 sibling feature", Era.Core/Shop/) returns 0 matches |
| 5 | Phase 6 updates "Next Feature number" to 815 after allocating F801-F814 (14 features). Exact match "815" enforced per deterministic allocation |
| 6 | Phase 5 creates DRAFTs with "Phase 21: Counter System" philosophy per C1. Grep returns >= 12 files |
| 7 | F801 assigns ICounterSystem (owner) and F809 assigns IComAvailabilityChecker (owner) per C11. Grep("ICounterSystem (owner)", feature-801.md) passes; Grep("IComAvailabilityChecker (owner)", feature-809.md) passes. Both checks required |
| 8 | Technical Design Phase 3 Subsystem Grouping assigns files to 4 subsystems. Feature titles/summaries in feature-80[1-9].md|feature-81[0-2].md collectively contain all 4 subsystem names: "Main Counter" (F801-F803), "WC Counter" (F804-F808), "COMABLE" (F809-F810), "SOURCE" (F811-F812). SOURCE.ERB in F811 confirms entry-point orchestrator assignment |
| 9 | Phase 5 creates DRAFTs containing all 30 ERB filenames from Technical Design Phase 3 Subsystem Grouping table. Each ERB filename appears in at least one DRAFT's scope reference. Grep for each of the 30 filenames confirms presence in at least one feature-80[1-9].md or feature-81[0-2].md file |
| 10 | Phase 2 modifies ShopSystem.cs and ShopDisplay.cs with delegation code. Build verification confirms the C# changes compile. Bash(dotnet build Era.Core/) returns exit code 0 |
| 11 | Phase 5 creates DRAFTs with "Remove TODO/FIXME/HACK" debt resolution task per C2. Grep returns >= 12 files |
| 12 | Phase 5 creates DRAFTs with "Write equivalence tests" equivalence test task per C3. Grep returns >= 12 files |
| 13 | Phase 5 creates DRAFTs with "zero technical debt" zero-debt AC per C4. Grep returns >= 12 files |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| WC Counter split strategy | (A) By subsystem (Core/Messages = 2 features), (B) By message category (SEX, TEASE, ITEM+NTR = 3 message features + 1 core + 1 source = 5 features) | B - 5 WC features | TOILET_COUNTER_MESSAGE_SEX (3,518 lines) and MESSAGE_TEASE (3,483 lines) each exceed the 500-line erb guideline individually. MESSAGE_ITEM (2,442 lines) pairs with MESSAGE_NTR (1,037 lines) = 3,479 lines as one feature. Option A would create features at 14,000+ lines, violating all granularity limits |
| COMABLE split strategy | (A) 1 feature (all 4,998 lines), (B) By file group (COMABLE+COMABLE2 = 3,897 lines, COMABLE_300+400 = 1,101 lines) | B - 2 features | COMABLE.ERB alone is 3,748 lines -- nearly 8x the 500-line limit. COMABLE2 (149 lines) is tightly coupled with COMABLE.ERB (extends same logic). COMABLE_300 and _400 are command-range-specific extensions that form a natural second grouping |
| SOURCE split strategy | (A) 1 feature (SOURCE+SOURCE1+helpers), (B) SOURCE entry system vs SOURCE1 extended | B - 2 features | SOURCE1.ERB alone is 1,959 lines exceeding single-feature limit. SOURCE.ERB (1,437 lines) groups naturally with thin helper files (SOURCE_CALLCOM:94, SOURCE_POSE:359, SOURCE_SHOOT:510) = 2,400 lines -- reasonable for one feature |
| Main Counter split strategy | (A) 1 feature (2,382 lines), (B) Core (SELECT+ACTABLE) vs Output (MESSAGE+POSE+REACTION+PUNISHMENT+COMBINATION) | B - 2 features | Main counter total is 2,382 lines (4.7x limit). Core (599 lines) owns ICounterSystem interface and action selection logic. Output (863 lines) handles message/pose/reaction -- separate concern per SRP from architecture spec |
| TOILET_EVENT_KOJO placement | (A) Separate kojo feature, (B) Bundle with WC Counter Core | B - Bundle with WC Core | TOILET_EVENT_KOJO.ERB is only 69 lines and is WC Counter-specific dialogue; bundling into F804 WC Counter Core keeps it co-located with the system that invokes it |
| Interface ownership assignment | (A) Defer to /fc of each sub-feature, (B) Pre-assign in DRAFT | B - Pre-assign in DRAFT | C11 mandates ICounterSystem and IComAvailabilityChecker be assigned to specific sub-features. Pre-assigning in DRAFT ensures this constraint propagates to implementers without requiring a separate audit pass |
| Stale stub resolution scope | (A) All 6 as annotation updates only, (B) 2 immediate delegations + 4 phase references | B - Mixed resolution | C13 confirms ILocationService.GetPlaceName exists. C14 confirms ICommonFunctions.HasVagina exists. These 2 can be fully resolved (throw → delegation call). The other 4 map to phases not yet implemented; annotation update to concrete phase number is the correct deferred resolution |
| Feature ID range | (A) F801-F810 (10 features), (B) F801-F812 implementation + F813-F814 transition (14 total) | B - F801-F814 | Empirical line count analysis reveals 12 implementation features are needed (3 Main Counter + 5 WC Counter + 2 COMABLE + 2 SOURCE). Transition features are mandatory. Total 14 features, Next = 815 |

### Interfaces / Data Structures

**Phase 21 Feature Allocation Table** (to be documented in Execution Log):

```
| Feature | Type | Source Files | Lines | Key Interfaces | External Callers |
|---------|------|-------------|-------|----------------|------------------|
| F801 | erb | COUNTER_SELECT.ERB, COUNTER_ACTABLE.ERB | 599 | ICounterSystem (owner) | SOURCE.ERB (via dispatch) |
| F802 | erb | COUNTER_MESSAGE.ERB, COUNTER_POSE.ERB, COUNTER_REACTION.ERB, COUNTER_PUNISHMENT.ERB, COUNTER_CONBINATION.ERB | 863 | - | COUNTER_SELECT.ERB |
| F803 | erb | COUNTER_SOURCE.ERB | 920 | - | SOURCE.ERB |
| F804 | erb | TOILET_COUNTER.ERB, TOILET_COUNTER_ACTABLE.ERB, TOILET_COUNTER_COMBINATION.ERB, TOILET_COUNTER_POSE.ERB, TOILET_COUNTER_PUNISHMENT.ERB, TOILET_COUNTER_REACTION.ERB, TOILET_EVENT_KOJO.ERB | 1,741 | - | SOURCE.ERB (via dispatch) |
| F805 | erb | TOILET_COUNTER_SOURCE.ERB, TOILET_COUNTER_MESSAGE.ERB | 1,907 | - | TOILET_COUNTER.ERB |
| F806 | erb | TOILET_COUNTER_MESSAGE_SEX.ERB | 3,518 | - | TOILET_COUNTER_MESSAGE.ERB |
| F807 | erb | TOILET_COUNTER_MESSAGE_TEASE.ERB | 3,483 | - | TOILET_COUNTER_MESSAGE.ERB |
| F808 | erb | TOILET_COUNTER_MESSAGE_ITEM.ERB, TOILET_COUNTER_MESSAGE_NTR.ERB | 3,479 | - | TOILET_COUNTER_MESSAGE.ERB |
| F809 | erb | COMABLE.ERB, COMABLE2.ERB | 3,897 | IComAvailabilityChecker (owner) | INFO.ERB, COMF*.ERB |
| F810 | erb | COMABLE_300.ERB, COMABLE_400.ERB | 1,101 | IComAvailabilityChecker (implementer) | COMABLE.ERB |
| F811 | erb | SOURCE.ERB, SOURCE_CALLCOM.ERB, SOURCE_POSE.ERB, SOURCE_SHOOT.ERB | 2,400 | - | NTR_UTIL.ERB (26 SOURCE_* calls) |
| F812 | erb | SOURCE1.ERB | 1,959 | - | SOURCE.ERB |
| F813 | infra | (Post-Phase Review Phase 21) | - | - | - |
| F814 | research | (Phase 22 Planning) | - | - | - |
```

**Sub-Feature DRAFT Template Requirements** (each F801–F812 must include):

```markdown
## Background
### Philosophy (Mid-term Vision)
Phase 21: Counter System — {subsystem-specific rationale}

## Tasks
| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | N | Remove TODO/FIXME/HACK comments in migrated C# files | | [ ] |
| 2 | N | Write equivalence tests comparing C# output to legacy ERB behavior | | [ ] |

## Acceptance Criteria
| AC# | Description | ... |
| N | No TODO/FIXME/HACK comments remain in migrated files (zero technical debt) | ... |
```

**Mandatory Interface Assignments** (per C11):
- `ICounterSystem` → F801 (Main Counter Core, owns `Era.Core/Counter/ActionSelector.cs` and `ActionValidator.cs`) — DRAFT must contain "ICounterSystem (owner)"
- `IComAvailabilityChecker` → F809 (COMABLE Core, owns `Era.Core/Counter/Comable/ComableChecker.cs`) — DRAFT must contain "IComAvailabilityChecker (owner)"

### Upstream Issues

<!-- Optional: Issues discovered during Technical Design that require upstream changes (AC gaps, constraint gaps, interface API gaps).
     Orchestrator reads this section after Phase 4 and dispatches micro-revisions if needed.
     Content may be empty if no upstream issues found. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#1 matcher "Phase 21.*Counter" may not match if DRAFT philosophy uses "Phase 21: Counter System" with colon (no ".*" needed; regex anchoring unclear) | AC Definition Table AC#1 | Verify the Grep pattern `Phase 21.*Counter` matches `"Phase 21: Counter System"` -- it does match (colon and space between "21" and "Counter"). No change needed. |
| TOILET_EVENT_KOJO.ERB not listed in phase-20-27-game-systems.md Phase 21 WC Counter Files table | Technical Constraints | File exists (69 lines, confirmed). No upstream change needed; implementation team should include it in F804 scope |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 3, 6, 7, 8, 9, 11, 12, 13 | Create DRAFT feature files F801–F812 (12 implementation sub-features: Main Counter Core/Output/Source, WC Counter Core/Source+Message/SEX/TEASE/ITEM+NTR, COMABLE Core/Extended, SOURCE Entry/SOURCE1) and F813 (Post-Phase Review Phase 21, type: infra) and F814 (Phase 22 Planning, type: research); each F801–F812 DRAFT must include "Phase 21: Counter System" philosophy, debt resolution task stub, equivalence test task stub, and zero-debt AC stub; F801 assigns ICounterSystem; F809 assigns IComAvailabilityChecker; feature titles/summaries reflect subsystem grouping (Main Counter, WC Counter, COMABLE, SOURCE) | | [x] |
| 2 | 4, 10 | Resolve all 6 stale "See Phase 20 sibling feature" annotations in Era.Core/Shop/: replace GETPLACENAME (ShopSystem.cs:433) with ILocationService.GetPlaceName delegation; replace HAS_VAGINA (ShopDisplay.cs:435) with ICommonFunctions.HasVagina delegation; update 子供訪問_設定 (ShopSystem.cs:387) to reference Phase 22; NTR_NAME (ShopSystem.cs:436) to Phase 25; CHILDREM_CHECK (ShopSystem.cs:439) to Phase 3; GET_MARK_LEVEL_OWNERNAME (ShopDisplay.cs:437) to Phase 26. After all 6 stub changes are complete, run `dotnet build Era.Core/` and confirm exit code 0 (zero errors) | | [x] |
| 3 | 2, 3, 5 | Update pm/index-features.md: add "### Phase 21: Counter System" section listing F801–F814 with titles and statuses; register F813 as "Post-Phase Review Phase 21" and F814 as "Phase 22 Planning"; increment "Next Feature number" to 815; also create F813 and F814 files as part of Task 1 and register them here | | [x] |

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
| 1 | implementer | sonnet | feature-783.md Technical Design (Phase 3: Subsystem Grouping table, Phase 5: Create DRAFT Files, Sub-Feature DRAFT Template Requirements) | feature-801.md through feature-814.md [DRAFT] (12 implementation sub-features + 2 transition features) |
| 2 | implementer | sonnet | feature-783.md Technical Design (Phase 4: Stale Stub Resolution table; C13, C14 interface constraints) | Era.Core/Shop/ShopSystem.cs (3 stubs updated), Era.Core/Shop/ShopDisplay.cs (2 stubs updated); 1 comment-only stub updated |
| 3 | implementer | sonnet | feature-783.md Technical Design (Phase 5: Create DRAFT Files, F813/F814 section; Phase 6: Index and Registry Updates); Technical Design "Next Feature number after allocation: 815" | feature-813.md [DRAFT] (type: infra), feature-814.md [DRAFT] (type: research), pm/index-features.md (Phase 21 section added, Next Feature number = 815) |

### Pre-conditions

- F647 [DONE] and F782 [DONE] (verified in Dependencies table)
- Era.Core/Shop/ contains ShopSystem.cs and ShopDisplay.cs with 6 "See Phase 20 sibling feature" annotations (ShopSystem.cs:387, 433, 436, 439; ShopDisplay.cs:435, 437)
- ILocationService.GetPlaceName exists at `Era.Core/Common/LocationSystem.cs:144`
- ICommonFunctions.HasVagina exists at `Era.Core/Interfaces/ICommonFunctions.cs:10-12`
- Next Feature number is 801 (index-features.md:178)

### Execution Order

1. **Phase 1** (Task 1): Create all DRAFT files F801–F814 using the Sub-Feature DRAFT Template Requirements from Technical Design. Each F801–F812 DRAFT includes: (a) Background with "Phase 21: Counter System" philosophy, (b) Task stubs for debt resolution and equivalence testing, (c) AC stub for zero-debt verification, (d) Interface assignment for F801 (ICounterSystem) and F809 (IComAvailabilityChecker). F813 type: infra. F814 type: research.
2. **Phase 2** (Task 2): Edit Era.Core/Shop/ShopSystem.cs and ShopDisplay.cs to replace all 6 "See Phase 20 sibling feature" annotations per the Stale Stub Resolution table in Technical Design.
3. **Phase 3** (Task 3): Edit pm/index-features.md to add "### Phase 21: Counter System" section listing F801–F814 (using feature titles from Technical Design Subsystem Grouping table), register F813 and F814, and set "Next Feature number" to 815.

### Build Verification

- After Phase 2: Run `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'` to confirm C# changes compile without errors.

### Success Criteria

- AC#1: `Glob("pm/features/feature-80[1-9].md")` + `Glob("pm/features/feature-81[0-2].md")` returns >= 12 files on disk
- AC#2: `Grep("F80[1-9]|F81[0-4]", "pm/index-features.md")` returns >= 14 matches for F801-F814 individual registrations
- AC#3: Glob confirms feature-813.md and feature-814.md exist; Grep confirms both registered in index; Grep("Type: infra", feature-813.md) and Grep("Type: research", feature-814.md) pass
- AC#4: `Grep("See Phase 20 sibling feature", "Era.Core/Shop/")` returns 0 matches
- AC#5: `Grep("Next Feature number", "pm/index-features.md")` returns value matching `815`
- AC#6: `Grep("Phase 21: Counter System", "pm/features/feature-80[1-9].md|feature-81[0-2].md")` >= 12 files
- AC#11: `Grep("Remove TODO/FIXME/HACK", "pm/features/feature-80[1-9].md|feature-81[0-2].md")` >= 12 files
- AC#12: `Grep("Write equivalence tests", "pm/features/feature-80[1-9].md|feature-81[0-2].md")` >= 12 files
- AC#13: `Grep("zero technical debt", "pm/features/feature-80[1-9].md|feature-81[0-2].md")` >= 12 files
- AC#7: `Grep("ICounterSystem (owner)", "pm/features/feature-801.md")` matches; `Grep("IComAvailabilityChecker (owner)", "pm/features/feature-809.md")` matches
- AC#8: feature-80[1-9].md|feature-81[0-2].md files collectively contain "Main Counter", "WC Counter", "COMABLE", "SOURCE" (one Grep per subsystem name, each must match at least one file); `Grep("SOURCE.ERB", "pm/features/feature-811.md")` matches
- AC#9: Grep each of 30 ERB filenames across `pm/features/feature-80[1-9].md|feature-81[0-2].md` — all 30 must appear in at least one file
- AC#10: `dotnet build Era.Core/` succeeds (exit code 0) after stub delegation changes

### Error Handling

- If Era.Core build fails after Phase 2: Revert stub changes; report to user (STOP)
- If feature ID collision detected (F801+ already exists): STOP and report to user
- If any sub-feature DRAFT does not fit the Sub-Feature DRAFT Template: STOP and report — do not create a non-conforming file

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Phase 21 implementation sub-features (Main Counter Core, Output, Main Counter Source, WC Counter Core, Source+Message, SEX, TEASE, ITEM+NTR, COMABLE Core, Extended, SOURCE Entry, SOURCE1) | Scope decomposition — each sub-feature requires separate /fc + /run cycle for C# migration | Feature | F801–F812 | Task#1 |
| Post-Phase Review Phase 21 | Mandatory transition feature — review after Phase 21 complete | Feature | F813 | Task#1 |
| Phase 22 Planning | Mandatory transition feature — next phase decomposition | Feature | F814 | Task#1 |

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
| 2026-02-23 | START | implementer | Task 1 (F801-F814 DRAFTs) | - |
| 2026-02-23 | END | implementer | Task 1 | SUCCESS (14 DRAFT files created) |
| 2026-02-23 | START | implementer | Task 2 (stale stubs) | - |
| 2026-02-23 | END | implementer | Task 2 | SUCCESS (Era.Core build OK; test constructor fixes applied) |
| 2026-02-23 | START | orchestrator | Task 3 (index update) | - |
| 2026-02-23 | END | orchestrator | Task 3 | SUCCESS (Phase 21 section added, Next=815) |
| 2026-02-23 | START | ac-tester | AC verification | - |
| 2026-02-23 | END | ac-tester | AC verification | 13/13 PASS |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: Links F555 | Updated link path from feature-555.md to archive/feature-555.md (archived feature)
- [fix] Phase1-RefCheck iter1: Links F452 | Updated link path from feature-452.md to archive/feature-452.md (archived feature)
- [fix] Phase2-Review iter1: line 19 | Removed non-template "## Created: 2026-02-11" field (not in feature-template.md)
- [fix] Phase2-Review iter1: Review Context section | Renamed "## Review Context" to "## Deferred Obligations" (content mismatch with template's Review Context format)
- [fix] Phase2-Review iter1: Sub-Feature DRAFT Template code block | Added missing header separator row in Tasks table inside code block
- [fix] Phase2-Uncertain iter1: AC Definition Table AC#1 | Split quality attribute sub-checks into formal AC#6 (C1-C4), AC#7 (C11), AC#8 (subsystem structure); AC#1 now file count only
- [fix] Phase2-Review iter1: Goal Coverage row 3 | Added AC#8 for 4-subsystem structure verification covering Goal Item 3
- [fix] Phase2-Uncertain iter1: AC#2 | Strengthened matcher from "matches ### Phase 21" to "gte 12" for individual F801-F814 registration verification
- [fix] Phase2-Review iter2: AC#6 matcher | Changed from collective "contains" to per-file "gte 10" verification; prevents collective-pass where strings appear only in transition features
- [fix] Phase2-Review iter2: AC#6 C1 gap | Added "Phase 21" philosophy string to AC#6 verification (was missing per C1 Constraint Details requirement)
- [fix] Phase2-Review iter3: Goal Coverage Item 5 | Removed AC#7 from Goal Item 5 (AC#7=C11 interface, not C1-C4 quality); added Goal Item 8 for C11→AC#7
- [fix] Phase2-Review iter3: AC#6 matchers | Strengthened from weak keywords ("debt","zero") to exact phrases ("Phase 21: Counter System","TODO/FIXME/HACK","equivalence test","zero-debt")
- [fix] Phase2-Uncertain iter3: Goal/AC#2 alignment | Updated Goal from "8-12" to "12" implementation sub-features to match Technical Design commitment
- [fix] Phase2-Uncertain iter4: F803 subsystem label | Renamed F803 subsystem from "COUNTER_SOURCE" to "Main Counter Source" for explicit 4-subsystem structure assignment
- [fix] Phase2-Review iter5: Task#3 AC coverage | Added AC#3 to Task#3 AC# column (AC#3 index registration half owned by Task#3)
- [fix] Phase2-Review iter5: "10" → "12" stale count | Fixed stale "10 implementation sub-features" in Approach, Key Decisions, Task#1, AC Coverage, Implementation Contract to match Phase 3 table (F801-F812 = 12)
- [fix] Phase2-Review iter6: AC#7 table Method | Updated from Grep(feature-8*.md) to file-specific Grep(feature-801.md) + Grep(feature-809.md) to match AC Details intent and prevent collective-pass
- [fix] Phase2-Review iter6: Summary | Added RESEARCH.md-required "Feature to create Features" marker and F801-F814 sub-feature list
- [fix] Phase2-Review iter7: AC#1 threshold | Updated gte from 8 to 12 to match Goal commitment to 12 implementation sub-features; updated AC Details, AC Coverage, Success Criteria
- [fix] Phase2-Review iter8: AC#3 Type verification | Added Grep("Type: infra", feature-813.md) + Grep("Type: research", feature-814.md) to satisfy C5/C6 Type field verification requirement
- [fix] Phase2-Uncertain iter8: AC#6 C2 matcher | Strengthened from "TODO/FIXME/HACK" to "Remove TODO/FIXME/HACK" (exact DRAFT template task description) to prevent false positives from non-task sections
- [fix] Phase2-Review iter9: AC#6 threshold | Changed gte from 10 to 12 — C1-C4 require ALL implementation sub-features to include quality attributes; previous tolerance of 2 was unjustified
- [fix] Phase2-Review iter10: Success Criteria AC#6 | Fixed stale ">= 10" to ">= 12" for "Phase 21: Counter System" grep in Success Criteria (missed during iter9 threshold update)
- [fix] Phase3-Maintainability iter10: Links F803 | Updated stale "COUNTER_SOURCE" label to "Main Counter Source" (matching Technical Design Phase 3 table after iter4 rename)
- [resolved-applied] Phase3-Maintainability iter10: N+4 --unit deprecation obligation has no concrete tracking destination Feature ID. Deferred Task Protocol requires concrete destination, but item is marked NOT_FEASIBLE. User decision needed: create tracking feature or accept NOT_FEASIBLE annotation as sufficient.
- [fix] PostLoop-UserFix iter4: Deferred Obligations | Added F813 as tracking destination for N+4 --unit deprecation obligation (user decision: Option A)
- [resolved-skipped] Phase2-Review iter1: Phase Context (lines 27-38) and Deferred Obligations (lines 40-54) are non-template sections inserted between Type and Background. Template defines strict section order. User decision: accepted as research-type convention (F647/F555 precedent).
- [fix] Phase2-Review iter1: line 19 | Removed --- horizontal rule between Type and Summary (not in template)
- [fix] Phase2-Review iter1: AC#2 threshold | Changed gte from 12 to 14 (all 14 sub-features F801-F814 must be registered)
- [fix] Phase2-Review iter1: AC#1/AC#6/AC#8 glob | Restricted from feature-8*.md to feature-80[1-9].md|feature-81[0-2].md (excludes F813/F814 transition features from count)
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#9 for scope exhaustiveness (all 30 Phase 21 ERB files assigned to DRAFTs); updated Philosophy Derivation, Goal Coverage, AC Coverage, Task#1 AC#, Success Criteria
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#10 for build verification after stale stub delegation changes; updated Goal Coverage, AC Coverage, Task#2 AC#, Success Criteria
- [fix] Phase2-Review iter3: AC#6 C3 matcher | Strengthened from "equivalence test" to "Write equivalence tests" (exact DRAFT template task description) to prevent false positives from non-task sections
- [fix] Phase2-Review iter3: AC#8 matcher | Added Grep("SOURCE.ERB", feature-811.md) to formal Definition Table matcher (was in Details only)
- [fix] Phase2-Review iter4: Task#2 description | Appended build verification step (dotnet build Era.Core/) to make AC#10 achievable from Task#2 alone
- [fix] Phase2-Review iter5: AC#3 Definition Table | Expanded Method column to reflect all 5 checks (2x Grep index + Glob files + 2x Grep Type fields) matching AC Details
- [fix] Phase2-Review iter6: AC#1 Details | Added explicit output_mode="files_with_matches" to clarify gte 12 counts files not lines
- [fix] Phase2-Review iter7: AC#6 C4 matcher | Strengthened from "zero-debt" to "zero technical debt" (exact DRAFT template AC description)
- [fix] Phase2-Review iter7: AC#7 matcher | Strengthened from plain interface names to ownership phrases "ICounterSystem (owner)" and "IComAvailabilityChecker (owner)"
- [resolved-applied] Phase2-Uncertain iter1: AC#9 verifies coverage (all 30 ERB files assigned) but no AC verifies uniqueness of assignment. User decision: TD table is SSOT for uniqueness; AC#9 Rationale updated with note.
- [fix] PostLoop-UserFix iter4: AC#9 Rationale | Added uniqueness enforcement note referencing Technical Design Phase 3 table (user decision: Option A)
- [fix] Phase4-ACValidation iter1: AC#1,#2,#6,#9 Matcher | Changed invalid matcher "count_gte" to valid "gte" per testing SKILL.md Matchers table
- [fix] Phase4-ACValidation iter1: AC#10 Type | Changed Type from "code" to "build" per testing SKILL.md AC Types table (build verification)
- [resolved-skipped] Phase2-Uncertain iter2: IActionSelector mentioned at phase-20-27-game-systems.md:138-139 under Strategy Pattern but not included in C11 or AC#7. User decision: accept current C11 scope (IActionSelector is design note, not formal interface definition).
- [fix] Phase2-Review iter2: AC#1 Method | Changed from Grep (content) to Glob (file existence) to independently verify file count — eliminates semantic overlap with AC#6 C1 check
- [fix] Phase2-Review iter3: AC#5 Expected | Changed regex "8[1-9][0-9]" (810-899 range) to exact "815" — allocation is deterministic (F801-F814 = 14 features)
- [resolved-applied] Phase2-Review iter4: AC#6 compound Expected "gte 12 per string: X, Y, Z, W" is not parseable by ac-static-verifier. User decision: Option A — split into 4 separate ACs (AC#6, AC#11, AC#12, AC#13). Each AC now has a single Grep command with gte 12 matcher. AC count exceeds research 3-5 guideline but user accepted this as necessary for verifier compatibility.
- [fix] Phase2-Review iter1r: Mandatory Handoffs | Replaced stale "COUNTER_SOURCE" with "Main Counter Source" in Issue column (missed during iter4 rename)
- [fix] Phase2-Uncertain iter2r: AC#9 Definition Table | Changed Method from single Grep to "30× Grep(each ERB filename)" and Matcher from "gte" to "contains" with compound check description
- [fix] Phase2-Review iter2r: IC Phase 1 Output | Added F813/F814 to Phase 1 output (matching Execution Order Step 1 "Create all DRAFT files F801-F814")
- [resolved-skipped] Phase2-Review iter1: AC#8 volume limits gap — Philosophy "clear phase boundaries" includes volume limits (AC 5-12, Tasks 3-7 per sub-feature per full-csharp-architecture.md:109-113) but no AC verifies sub-feature DRAFTs comply with volume limits. User decision: /fc enforces volume limits on final features; DRAFTs are intentionally minimal stubs.
- [fix] Phase2-Review iter1: AC Definition Table | Reordered AC rows from non-sequential (#1-6, #11-13, #7-10) to sequential (#1-13)
- [fix] Phase2-Review iter1: AC Coverage table | Reordered AC rows from non-sequential (#1-6, #11-13, #7-10) to sequential (#1-13)
- [fix] Phase2-Uncertain iter1: AC#9 Details | Enumerated all 30 ERB filenames explicitly in AC#9 Test description (was only referenced from Technical Design table)
- [fix] Phase7-FinalRefCheck iter2: Links section | Added F774-F781 individual links (referenced in Related Features table but missing from Links section)

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning (pattern precedent and parent)
- [Predecessor: F782](feature-782.md) - Post-Phase Review Phase 20 (deferred stale stub obligation)
- [Related: F555](archive/feature-555.md) - Phase 19 Planning (pattern precedent: 15 sub-features)
- [Related: F800](feature-800.md) - GeneticsService extraction (IVariableStore dedup pattern)
- [Related: F452](archive/feature-452.md) - Phase 12 COM system (read-shared by Phase 21 COMABLE via IComRegistry)
- [Related: F774](feature-774.md) - Phase 20 sub-feature (Era.Core migration pattern)
- [Related: F775](feature-775.md) - Phase 20 sub-feature (Era.Core migration pattern)
- [Related: F776](feature-776.md) - Phase 20 sub-feature (Era.Core migration pattern)
- [Related: F777](feature-777.md) - Phase 20 sub-feature (Era.Core migration pattern)
- [Related: F778](feature-778.md) - Phase 20 sub-feature (Era.Core migration pattern)
- [Related: F779](feature-779.md) - Phase 20 sub-feature (Era.Core migration pattern)
- [Related: F780](feature-780.md) - Phase 20 sub-feature (Era.Core migration pattern)
- [Related: F781](feature-781.md) - Phase 20 sub-feature (Era.Core migration pattern)
- [Successor: F801](feature-801.md) - Main Counter Core (created by this feature)
- [Successor: F802](feature-802.md) - Main Counter Output (created by this feature)
- [Successor: F803](feature-803.md) - Main Counter Source (created by this feature)
- [Successor: F804](feature-804.md) - WC Counter Core (created by this feature)
- [Successor: F805](feature-805.md) - WC Counter Source + Message Core (created by this feature)
- [Successor: F806](feature-806.md) - WC Counter Message SEX (created by this feature)
- [Successor: F807](feature-807.md) - WC Counter Message TEASE (created by this feature)
- [Successor: F808](feature-808.md) - WC Counter Message ITEM + NTR (created by this feature)
- [Successor: F809](feature-809.md) - COMABLE Core (created by this feature)
- [Successor: F810](feature-810.md) - COMABLE Extended (created by this feature)
- [Successor: F811](feature-811.md) - SOURCE Entry System (created by this feature)
- [Successor: F812](feature-812.md) - SOURCE1 Extended (created by this feature)
- [Successor: F813](feature-813.md) - Post-Phase Review Phase 21 (created by this feature)
- [Successor: F814](feature-814.md) - Phase 22 Planning (created by this feature)
