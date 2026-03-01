# Feature 811: SOURCE Entry System

## Status: [DONE]
<!-- fl-reviewed: 2026-02-26T23:37:39Z -->

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

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

Phase 21: Counter System -- all ERB counter logic migrated to C# with zero technical debt, equivalence-tested against legacy behavior. SOURCE Entry System is the SSOT for counter event orchestration: it owns the main entry point (@SOURCE_CHECK) that dispatches to Main Counter (F801) or WC Counter (F804) subsystems, manages touch state (TOUCH_SET/MASTER_POSE/TOUCH_SUCCESSION/SHOW_TOUCH), COM calling, and shooting/ejaculation logic across 4 ERB files (SOURCE.ERB, SOURCE_CALLCOM.ERB, SOURCE_POSE.ERB, SOURCE_SHOOT.ERB).

### Problem (Current Issue)

F811's DRAFT spec declares only F783 as predecessor and contains 2 generic stub tasks, because F783's Phase 21 decomposition focused on file-prefix grouping rather than call-chain dependency analysis (feature-783.md:407-422). SOURCE.ERB is architecturally an orchestrator hub that makes hard CALL dependencies on nearly every other Phase 21 subsystem: EVENT_COUNTER at SOURCE.ERB:37 (F801), EVENT_WC_COUNTER at SOURCE.ERB:25 (F804), EVENT_COUNTER_SOURCE at SOURCE.ERB:38 (F803), EVENT_COUNTER_COMBINATION at SOURCE.ERB:40 (F802), EVENT_WC_COUNTER_SOURCE at SOURCE.ERB:26 (F805), and 30+ SOURCE_* function calls at SOURCE.ERB:127-335 (F812). Additionally, SOURCE_POSE.ERB functions TOUCH_SET and MASTER_POSE are shared utilities called by 36+ files across F802, F803, F805, and F809, requiring an interface boundary. Five interface gaps exist (PREVCOM, UP, CSTR, EXP_UP, SETBIT) that must be resolved before migration can proceed; RELATION read is covered by F812's existing GetRelation (SOURCE.ERB:491 read-only). (EXPLV is F812-scope: SOURCE1.ERB:67-77.)

### Goal (What to Achieve)

Migrate SOURCE.ERB (1,437 lines), SOURCE_CALLCOM.ERB (94 lines), SOURCE_POSE.ERB (359 lines), and SOURCE_SHOOT.ERB (510 lines) to C# with: (1) correct predecessor declarations on F801 and F812, (2) stub-interface strategy for not-yet-[DONE] siblings F802/F803/F805, (3) ITouchStateManager interface exposing TOUCH_SET/MASTER_POSE for sibling consumption, (4) strategy/dictionary pattern for CALLFORM COM_ABLE{N} dynamic dispatch, (5) interface additions for PREVCOM/UP/CSTR/EXP_UP/SETBIT gaps (RELATION read handled by F812's existing GetRelation), (6) equivalence tests verifying C# output matches legacy ERB behavior, and (7) zero technical debt.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does F811 DRAFT have only F783 as predecessor and 2 stub tasks? | Because F783 created all Phase 21 DRAFTs with a minimal template focused on file decomposition, not dependency analysis | feature-811.md:44-47 |
| 2 | Why did F783 not analyze call-chain dependencies? | Because F783's decomposition grouped files by naming prefix (SOURCE_*), not by function call direction | feature-783.md:407-422 |
| 3 | Why does file-prefix grouping miss dependencies? | Because SOURCE.ERB @SOURCE_CHECK directly CALLs functions defined in 6 other sibling features (F801-F805, F812) at SOURCE.ERB:25-40, 127-335 | SOURCE.ERB:25-40, SOURCE.ERB:127-335 |
| 4 | Why are these cross-feature CALLs problematic? | Because C# migration requires DI interfaces for every cross-feature function call, and 30+ SOURCE_* calls to F812 alone make stub-only approaches infeasible for F812 | SOURCE.ERB:127-335, SOURCE1.ERB |
| 5 | Why (Root)? | Because F783's planning focused on file grouping (which files belong to which feature) not on call-chain-derived predecessor ordering; the critical direction of calls (F811 calling INTO F801/F804/F812 vs F803/F805 calling INTO F811) was not analyzed | feature-783.md:155, feature-783.md:407-422 |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | F811 DRAFT has only F783 as predecessor and 2 generic stub tasks | F783 decomposition used file-prefix grouping, not call-chain dependency analysis |
| Where | feature-811.md Dependencies table (line 44-47) | feature-783.md decomposition logic (line 407-422) |
| Fix | Manually add missing predecessors to Dependencies table | Analyze call graph to derive predecessor/successor ordering and declare F801/F812 as predecessors, define stub interfaces for remaining siblings |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F783 | [DONE] | Predecessor -- Phase 21 Planning (parent decomposition) |
| F801 | [DONE] | Predecessor -- F811 dispatches to EVENT_COUNTER via ICounterSystem (SOURCE.ERB:37) |
| F802 | [DONE] | Related -- F811 calls EVENT_COUNTER_COMBINATION (SOURCE.ERB:40); F802 calls MASTER_POSE (F811 scope) |
| F803 | [DONE] | Related -- Bidirectional: F811 calls EVENT_COUNTER_SOURCE (SOURCE.ERB:38); F803 calls TOUCH_SET 28 times (F811 scope) |
| F804 | [DONE] | Related -- F811 dispatches to EVENT_WC_COUNTER (SOURCE.ERB:25); stubable behind IWcCounterSystem interface |
| F805 | [DRAFT] | Related -- Bidirectional: F811 calls EVENT_WC_COUNTER_SOURCE (SOURCE.ERB:26); F805 calls TOUCH_SET 42 times (F811 scope) |
| F809 | [DONE] | Related -- SOURCE_CALLCOM.ERB:70 dynamically dispatches to COM_ABLE{N} (F809 scope); F809 calls MASTER_POSE (F811 scope) |
| F810 | [DONE] | Related -- COM_ABLE 400-series called dynamically from SOURCE_CALLCOM.ERB |
| F812 | [DONE] | Predecessor -- 30+ SOURCE_* function calls from SOURCE.ERB:127-335 |
| F813 | [DRAFT] | Successor -- Post-Phase Review Phase 21 |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Source ERB files identified and accessible | FEASIBLE | All 4 files confirmed: SOURCE.ERB (1437), SOURCE_CALLCOM.ERB (94), SOURCE_POSE.ERB (359), SOURCE_SHOOT.ERB (510) |
| Existing C# infrastructure sufficient | FEASIBLE | ICounterSystem, IActionSelector, IActionValidator, CounterActionId, IVariableStore, ITEquipVariables all exist |
| F801 predecessor available | FEASIBLE | F801 [DONE], ICounterSystem.SelectAction available |
| F812 predecessor available | FEASIBLE | F812 [DONE]; ISourceCalculator interface available |
| Sibling features (F802-F805) stubable | FEASIBLE | Individual CALL statements can be stubbed behind injectable interfaces |
| Interface gaps manageable | FEASIBLE | 5 interface additions scoped to F811 Tasks: PREVCOM (AC#14/15), UP (AC#16/17/38), SETBIT (AC#18), CSTR-write (AC#46), EXP_UP (inline calc, AC#47); RELATION read handled by F812's existing GetRelation |
| Code scope appropriate for erb type | FEASIBLE | 2,400 total lines manageable with proper decomposition into C# classes |
| Cross-phase external dependencies | FEASIBLE | ~15 external function dependencies (TRACHECK, KOJO_MESSAGE, etc.) abstractable behind DI interfaces |
| TOUCH_SET/MASTER_POSE shared utility boundary | FEASIBLE | Define ITouchStateManager interface owned by F811 for sibling consumption |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| IEngineVariables interface | HIGH | Must add GetPrevCom()/SetPrevCom() for PREVCOM access (SOURCE.ERB:186,340,494) |
| IVariableStore or new interface | HIGH | Must add UP array accessors (SOURCE.ERB:342-351); RELATION read at SOURCE.ERB:491 covered by F812's existing GetRelation |
| Sibling features F802/F803/F805/F809 | HIGH | F811 defines ITouchStateManager (TOUCH_SET, MASTER_POSE); siblings must consume this interface |
| Era.Core/Counter/Source/ namespace | MEDIUM | New C# classes: SourceEntrySystem, TouchStateManager, ComCaller, ShootingSystem |
| External function callers (EVENTCOMEND.ERB) | LOW | ChastityBelt_Check (SOURCE.ERB:546) exposed via ISourceSystem for external callers |
| F812 sequencing | HIGH | F812 must be completed before F811 can compile (30+ function dependencies) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| 30+ SOURCE_* calls from F811 to F812 | SOURCE.ERB:127-335 | F812 must provide ISourceCalculator interface or be completed first |
| CALLFORM COM_ABLE{N} dynamic dispatch | SOURCE_CALLCOM.ERB:70 | Requires strategy/dictionary pattern; N = 500 + TFLAG:50 |
| PREVCOM not in IEngineVariables | IEngineVariables.cs (no GetPrevCom) | Must add GetPrevCom()/SetPrevCom() |
| ~~RELATION array not in any interface~~ | ~~SOURCE.ERB:491~~ | ~~Must create accessor for RELATION:TARGET:(NO:PLAYER)~~ Resolved: F812's IRelationVariables.GetRelation() covers read-only access; SOURCE*.ERB never writes RELATION |
| UP array not in any interface | SOURCE.ERB:342-351 | Must create accessor for UP:index read/write |
| CSTR string variable not accessible | SOURCE_SHOOT.ERB:8-9 | Must add CSTR accessor to IStringVariables or new interface |
| EXP_UP used as both boolean and value | SOURCE.ERB:113,231,413 | C# must handle dual-use: IF EXP_UP(N,chr) and {EXP_UP(N,chr)} |
| SETBIT operation not in C# | SOURCE_SHOOT.ERB (extensive use) | Must implement bitfield utility for shooting logic |
| CLOTHES_Change_Knickers depends on Phase 22 | SOURCE.ERB:1381 | Must use stub; Phase 22 runs after Phase 21 |
| CHK_SEMEN_STAY has 10 variant sub-functions (~370 lines) | SOURCE.ERB:976-1345 | Heavy text output with NTR content; must test branching separately |
| TreatWarningsAsErrors enabled | Directory.Build.props | All C# must compile without warnings |
| EVENTCOMEND calls ChastityBelt_Check | EVENTCOMEND.ERB:136 | Must expose via ISourceSystem for external callers |
| ~~COUNTER_CONBINATION typo in ERB~~ | ~~feature-783.md:154~~ | ~~Must correct to "Combination" in C#~~ (Resolved by F802 [DONE]) |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| ~~F812 [DRAFT] blocks F811 implementation~~ | ~~HIGH~~ | ~~HIGH~~ | ~~Resolved: F812 [DONE], ISourceCalculator available~~ |
| TOUCH_SET/MASTER_POSE interface boundary unclear | HIGH | MEDIUM | Define ITouchStateManager interface early; F811 owns, siblings consume |
| Interface gap count (6) creates prerequisite work | MEDIUM | HIGH | Bundle interface additions into F811 Tasks; design for reuse |
| Dynamic CALLFORM dispatch hard to migrate | MEDIUM | MEDIUM | Dictionary-based strategy pattern mapping TFLAG:50 values to handlers |
| Bidirectional dependency with F803/F805 via TOUCH_SET | MEDIUM | HIGH | Extract TOUCH_SET into ITouchStateManager owned by F811; break circular dependency |
| CHK_SEMEN_STAY text creates large test surface (~370 lines, 10 variants) | MEDIUM | LOW | Test branching logic separately from text output |
| External function dependencies (~15) require stubs | LOW | MEDIUM | Group into cohesive facade interfaces per F801 pattern |
| Interface proliferation (10+ new interfaces) | HIGH | MEDIUM | Group related functions into cohesive interfaces (e.g., ITrainingUtilities) |
| Scope exceeds typical erb volume limit | LOW | MEDIUM | Accept; 2,400 lines is manageable with proper class decomposition |
| MasterPose behavioral divergence across 3 parallel implementations | LOW | MEDIUM | F813 Task#1 consolidation; Mandatory Handoffs adapter spec defines target signature |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ERB source line count | `wc -l Game/ERB/SOURCE.ERB Game/ERB/SOURCE_CALLCOM.ERB Game/ERB/SOURCE_POSE.ERB Game/ERB/SOURCE_SHOOT.ERB` | ~2,400 | Total lines across 4 source files |
| Existing C# Counter files | `ls Era.Core/Counter/` | 41 files (incl. Comable/): F801, F802, F803, F804, F809, F810, F812 infrastructure | F801/F802/F803/F804/F809/F810/F812 infrastructure available |
| FIXME comment count | `grep -c FIXME Game/ERB/SOURCE*.ERB` | Boilerplate only | All are "関数の説明を書いてください" -- not functional debt |

**Baseline File**: `.tmp/baseline-811.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Zero technical debt | feature-783.md:239 | Grep TODO/FIXME/HACK in migrated C# files |
| C2 | Equivalence tests required | feature-783.md:238 | C# behavior must match ERB for all migrated functions |
| C3 | Hard CALL deps on F801 (EVENT_COUNTER) and F812 (30+ SOURCE_*) | SOURCE.ERB:37, SOURCE.ERB:127-335 | Cannot compile without ICounterSystem and ISourceCalculator interfaces |
| C4 | TOUCH_SET/MASTER_POSE shared utilities | 36+ external callers across F802/F803/F805/F809 | Must expose via ITouchStateManager interface; verify interface accessibility |
| C5 | CALLFORM COM_ABLE{N} dynamic dispatch | SOURCE_CALLCOM.ERB:70 | Verify COM dispatch mechanism uses strategy/dictionary pattern |
| C6 | EVENT_SHOOT self-contained shooting logic | SOURCE_SHOOT.ERB:26-124 | Can test in isolation; verify cross-character ejaculation tracking |
| C7 | PRINTFORM/PRINTFORML for display output | SOURCE.ERB:447-534 | Use IConsoleOutput for verification of display output |
| C8 | PREVCOM, UP interface gaps; RELATION read-only (F812 GetRelation sufficient) | IEngineVariables, IVariableStore | Must include PREVCOM/UP interface additions in scope; verify with getter/setter ACs; RELATION read handled by F812 |
| C9 | CLOTHES_Change_Knickers Phase 22 dependency | SOURCE.ERB:1381 | Use stub; verify stub is replaceable |
| C10 | ITouchStateManager interface exists | Interface Dependency Scan | AC must verify interface definition with TOUCH_SET, TOUCH_RESET_M, TOUCH_RESET_T, MASTER_POSE methods |
| C11 | ISourceCalculator or F812 interface boundary | Interface Dependency Scan | AC must verify 30+ SOURCE_* function calls route through interface |
| C12 | EXP_UP dual-use (boolean + value) | Interface Dependency Scan | AC must verify C# handles both IF EXP_UP(N,chr) and {EXP_UP(N,chr)} value extraction |
| C13 | SETBIT bitfield operation | Interface Dependency Scan | AC must verify bitfield utility exists for SOURCE_SHOOT migration |
| C14 | Stub interfaces for F802/F803/F804/F805 | Interface Dependency Scan | AC must verify stub interfaces are injectable and replaceable |
| C15 | WC branch predicate: TALENT:MASTER:肉便器 | SOURCE.ERB:21 | AC must verify SourceEntrySystem routing condition references the 肉便器 talent check |
| C16 | SourceEntrySystem constructor parameter count must not exceed 25 | AC#125 |

### Constraint Details

**C1: Zero Technical Debt**
- **Source**: Phase 21 Sub-Feature Requirements (feature-783.md:239)
- **Verification**: Grep TODO/FIXME/HACK in Era.Core/Counter/Source/
- **AC Impact**: AC must verify no TODO/FIXME/HACK comments remain in migrated C# files

**C2: Equivalence Testing**
- **Source**: Phase 21 Sub-Feature Requirements (feature-783.md:238)
- **Verification**: Run equivalence tests comparing C# output to legacy ERB behavior
- **AC Impact**: AC must include equivalence test execution and PASS verification

**C3: Hard CALL Dependencies**
- **Source**: SOURCE.ERB:37 (EVENT_COUNTER), SOURCE.ERB:127-335 (30+ SOURCE_*)
- **Verification**: Verify ICounterSystem.SelectAction is called; verify ISourceCalculator interface covers all 30+ SOURCE_* functions
- **AC Impact**: AC must verify C# code references ICounterSystem and ISourceCalculator (or equivalent)

**C4: TOUCH_SET/MASTER_POSE Shared Interface**
- **Source**: 36+ external callers (COUNTER_SOURCE.ERB:9, TOILET_COUNTER_SOURCE.ERB:13, COMABLE.ERB, COUNTER_REACTION.ERB)
- **Verification**: Verify ITouchStateManager interface definition includes all 4 methods
- **AC Impact**: AC must verify interface exists with correct method signatures

**C5: Dynamic COM Dispatch**
- **Source**: SOURCE_CALLCOM.ERB:70 -- CALLFORM COM_ABLE{500+TFLAG:50}
- **Verification**: Verify dictionary/strategy pattern maps TFLAG:50 values to COM handlers
- **AC Impact**: AC must verify dispatch mechanism handles all TFLAG:50 values (1-16)

**C6: Shooting Logic Isolation**
- **Source**: SOURCE_SHOOT.ERB:26-124 -- EVENT_SHOOT with cross-character ejaculation tracking
- **Verification**: Unit tests for shooting logic independent of other subsystems
- **AC Impact**: AC must verify shooting logic handles multi-character scenarios

**C7: PRINTFORM/PRINTFORML Display Output**
- **Source**: SOURCE.ERB:447-534 (PRINTFORM/PRINTFORML commands)
- **Verification**: Use IConsoleOutput for verification of display output in tests
- **AC Impact**: AC must verify display output is produced via IConsoleOutput interface

**C8: Interface Gap Additions**
- **Source**: IEngineVariables.cs (no GetPrevCom), IVariableStore.cs (no UP); RELATION read-only at SOURCE.ERB:491 via F812's existing IRelationVariables.GetRelation()
- **Verification**: Grep for GetPrevCom, GetUp in interface files; RELATION requires no new setter (F812 GetRelation sufficient)
- **AC Impact**: AC must verify PREVCOM getter/setter additions exist; AC must verify IUpVariables with GetUp/SetUp exists

**C9: CLOTHES_Change_Knickers Phase 22 Dependency**
- **Source**: SOURCE.ERB:1381 (CLOTHES_Change_Knickers call)
- **Verification**: Grep Counter/Source/ for CLOTHES_Change_Knickers — must not appear; IKnickersSystem stub used instead
- **AC Impact**: AC#31 verifies no direct call; AC#32 verifies IKnickersSystem stub exists

**C10: ITouchStateManager Interface**
- **Source**: Interface Dependency Scan -- TOUCH_SET, TOUCH_RESET_M, TOUCH_RESET_T, MASTER_POSE used by F803/F805/F809
- **Verification**: Grep ITouchStateManager in Era.Core/
- **AC Impact**: AC must verify interface definition and implementation

**C11: ISourceCalculator or F812 Interface Boundary**
- **Source**: SOURCE.ERB:127-335 (30+ SOURCE_* function calls to F812)
- **Verification**: Grep for ISourceCalculator in Counter/Source/ to verify all 30+ calls route through interface
- **AC Impact**: AC must verify C# code references ISourceCalculator (or equivalent F812 interface)

**C12: EXP_UP Dual-Use**
- **Source**: SOURCE.ERB:113 (IF EXP_UP boolean), SOURCE.ERB:418 ({EXP_UP(N,chr)} value)
- **Verification**: Unit test both boolean and value extraction paths
- **AC Impact**: Resolved: AC#22/23 removed per DEP-DRIFT. EXP_UP computed inline as EXP[i]-TCVAR[400+i] per F812 pattern. Boolean/value dual-use verification covered by AC#47 (specific) and equivalence tests (AC#39, AC#41).

**C13: SETBIT Bitfield Operation**
- **Source**: SOURCE_SHOOT.ERB (extensive SETBIT usage)
- **Verification**: Grep Era.Core/ for SetBit with int parameter signatures
- **AC Impact**: AC#24 verifies BitfieldUtility.SetBit exists with correct signature

**C14: Stub Interfaces for F802/F803/F804/F805**
- **Source**: SOURCE.ERB:38 (EVENT_COUNTER_SOURCE→F803), SOURCE.ERB:26 (EVENT_WC_COUNTER_SOURCE→F805), SOURCE.ERB:40 (EVENT_COUNTER_COMBINATION→F802), SOURCE.ERB:28 (EVENT_WC_COUNTER_COMBINATION→F802 WC parallel)
- **Verification**: Grep Era.Core/Counter/ for each stub interface definition
- **AC Impact**: AC#26 (ICounterSourceHandler), AC#27 (IWcCounterSourceHandler), AC#28 (ICombinationCounter), AC#56 (IWcCombinationCounter)

**C15: WC Branch Predicate**
- **Source**: SOURCE.ERB:21 (IF TALENT:MASTER:肉便器 selects WC path)
- **Verification**: Grep SourceEntrySystem.cs for 肉便器 or C# equivalent (WcMode/IsWc)
- **AC Impact**: AC#58 verifies routing predicate reference in SourceEntrySystem

**C16: SourceEntrySystem constructor parameter cap**
- Maximum 25 constructor parameters (DI injected interfaces)
- Enforced by AC#125 (lte matcher on `private readonly I` field count)
- Rationale: orchestrator hub pattern (Key Decisions) justifies high count; cap prevents unbounded growth

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| Predecessor | F801 | [DONE] | Main Counter Core -- F811 dispatches to EVENT_COUNTER via ICounterSystem (CALL at SOURCE.ERB:37) |
| Predecessor | F812 | [DONE] | SOURCE1 Extended -- 30+ SOURCE_* function calls from SOURCE.ERB:127-335; too numerous to stub |
| Related | F802 | [DONE] | Main Counter Output -- F811 calls EVENT_COUNTER_COMBINATION (SOURCE.ERB:40); F802 calls MASTER_POSE (F811 scope) |
| Related | F803 | [DONE] | Main Counter Source -- Bidirectional: F811 calls EVENT_COUNTER_SOURCE (SOURCE.ERB:38); F803 calls TOUCH_SET 28 times |
| Related | F804 | [DONE] | WC Counter Core -- F811 dispatches to EVENT_WC_COUNTER (SOURCE.ERB:25); stubable behind interface |
| Related | F805 | [DRAFT] | WC Counter Source -- Bidirectional: F811 calls EVENT_WC_COUNTER_SOURCE (SOURCE.ERB:26); F805 calls TOUCH_SET 42 times |
| Related | F809 | [DONE] | COMABLE Core -- Dynamic CALLFORM COM_ABLE{N} from SOURCE_CALLCOM.ERB:70; F809 calls MASTER_POSE |
| Related | F810 | [DONE] | COMABLE Extended -- COM_ABLE 400-series called dynamically from SOURCE_CALLCOM.ERB |
| Successor | F813 | [DRAFT] | Post-Phase Review Phase 21 |
| External | TRACHECK.ERB | N/A | ~10 utility functions (TRACHECK_TIME, etc.) |
| External | EVENT_KOJO.ERB | N/A | 6 KOJO_MESSAGE functions (KOJO_MESSAGE_COM, etc.) |
| External | NTR_UTIL.ERB | N/A | NTR_MARK_5, NTR_ADD_SURRENDER |
| External | WC_SexHara.ERB | N/A | WC_SexHara, WC_SexHara_SOURCE |
| External | IOrgasmProcessor | Exists | ORGASM_ADD (SOURCE.ERB:202) |
| External | IVirginityManager | Exists | LOST_VIRGIN variants (SOURCE.ERB:131-133) |
| External | IFavorCalculator | Exists | FAVOR_CALC (SOURCE.ERB:544) |
| External | IMarkSystem | Exists | MARK_GOT_CHECK (SOURCE.ERB:471) |
| External | IShrinkageSystem | Exists | 締り具合変動 (shrinkage calls from SOURCE.ERB) |
| External | ICounterUtilities | Exists | CheckExpUp for EXP_UP dual-use (F803-provided) |

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
| "all ERB counter logic migrated to C#" | All 4 SOURCE ERB files (SOURCE.ERB, SOURCE_CALLCOM.ERB, SOURCE_POSE.ERB, SOURCE_SHOOT.ERB) have C# equivalents with behavioral verification | AC#1, AC#2, AC#4, AC#5, AC#32, AC#34 |
| "zero technical debt" | No TODO/FIXME/HACK comments in migrated C# files | AC#3 |
| "equivalence-tested against legacy behavior" | C# unit tests verify equivalence with ERB behavior | AC#4, AC#5, AC#32, AC#34, AC#35, AC#37, AC#40, AC#84, AC#85, AC#86, AC#87, AC#89, AC#93, AC#96, AC#97, AC#110, AC#115, AC#129, AC#131, AC#133 |
| "SSOT for counter event orchestration" | SourceEntrySystem owns @SOURCE_CHECK dispatching to ICounterSystem and IWcCounterSystem | AC#6, AC#41, AC#42, AC#44, AC#50, AC#68, AC#72, AC#79, AC#90, AC#95, AC#127, AC#128 |
| "owns the main entry point (@SOURCE_CHECK)" | SourceEntrySystem class exists and routes to subsystems | AC#6, AC#7, AC#51 |
| "owns ... COM calling" | ComCaller class implements CALLFORM COM_ABLE{N} dynamic dispatch via strategy/dictionary pattern; also owns SCOM/CAN_SCOM dispatch and CAN_COM/COM{SELECTCOM} dispatch with TRYCALLFORM fallback semantics; SourceEntrySystem owns CAN_COM entry condition logic | AC#13, AC#27, AC#33, AC#48, AC#57, AC#58, AC#59, AC#63, AC#67, AC#74, AC#83, AC#94, AC#120, AC#121, AC#132 |
| "owns ... shooting/ejaculation logic" | ShootingSystem class implements EVENT_SHOOT, EJAC_CHECK, SAMEN_SHOOT, SAMEN_DIRECTION from SOURCE_SHOOT.ERB | AC#23, AC#34, AC#35, AC#90, AC#109, AC#129 |
| "manages touch state (TOUCH_SET/MASTER_POSE/TOUCH_SUCCESSION/SHOW_TOUCH)" | SourceEntrySystem injects and calls ITouchStateManager for touch state management | AC#8, AC#9, AC#10, AC#11, AC#12, AC#39, AC#61, AC#62, AC#75, AC#106 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Source C# files exist in Counter/Source namespace (5 impl + 3 interface = 8 files) | file | Glob("Era.Core/Counter/Source/*.cs") | gte | 8 | [x] |
| 2 | Source C# test files exist (5 per migrated ERB file + SourceDisplayHandler) | file | Glob("Era.Core.Tests/Counter/Source/*.cs") | gte | 5 | [x] |
| 3 | No technical debt markers in migrated C# files | code | Grep(Era.Core/Counter/, glob="*.cs") | not_matches | `TODO\|FIXME\|HACK` | [x] |
| 4 | Equivalence tests build | build | dotnet build Era.Core.Tests/ | succeeds | - | [x] |
| 5 | Equivalence tests pass | test | dotnet test Era.Core.Tests/ | succeeds | - | [x] |
| 6 | SourceEntrySystem dispatches to ICounterSystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ICounterSystem` | [x] |
| 7 | SourceEntrySystem dispatches to IWcCounterSystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IWcCounterSystem` | [x] |
| 8 | ITouchStateManager public interface defined with TOUCH_SET | code | Grep(Era.Core/Counter/Source/ITouchStateManager.cs) | matches | `public interface ITouchStateManager` | [x] |
| 9 | ITouchStateManager declares TOUCH_SET method | code | Grep(Era.Core/Counter/Source/) | matches | `TouchSet\(` | [x] |
| 10 | ITouchStateManager declares MASTER_POSE method | code | Grep(Era.Core/Counter/Source/) | matches | `MasterPose\(` | [x] |
| 11 | ITouchStateManager declares TOUCH_RESET_M method | code | Grep(Era.Core/Counter/Source/) | matches | `TouchResetM\(` | [x] |
| 12 | ITouchStateManager declares TOUCH_RESET_T method | code | Grep(Era.Core/Counter/Source/) | matches | `TouchResetT\(` | [x] |
| 13 | COM dispatch uses dictionary/strategy pattern | code | Grep(Era.Core/Counter/Source/) | matches | `Dictionary.*ComHandler|IComDispatcher` | [x] |
| 14 | GetPrevCom added to IEngineVariables | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | `GetPrevCom\(` | [x] |
| 15 | SetPrevCom added to IEngineVariables | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | `SetPrevCom\(` | [x] |
| 16 | UP accessor added to IUpVariables interface | code | Grep(Era.Core/Interfaces/) | matches | `int GetUp\(int` | [x] |
| 17 | UP setter added | code | Grep(Era.Core/Interfaces/) | matches | `void SetUp\(int` | [x] |
| 18 | SETBIT bitfield utility exists | code | Grep(Era.Core/) | matches | `SetBit\(.*int.*int` | [x] |
| 19 | SourceEntrySystem consumes F804's IWcCounterSystem.SelectAction | code | Grep(Era.Core/Counter/Source/) | matches | `_wcCounterSystem\.SelectAction\|IWcCounterSystem.*SelectAction` | [x] |
| 20 | ICounterSourceHandler interface exists (F803-provided) | code | Grep(Era.Core/Counter/) | matches | `interface ICounterSourceHandler` | [x] |
| 21 | Stub interface for WC Counter Source handler | code | Grep(Era.Core/Counter/) | matches | `interface IWcCounterSourceHandler` | [x] |
| 22 | Stub interface for Combination Counter | code | Grep(Era.Core/Counter/) | matches | `interface ICombinationCounter` | [x] |
| 23 | ShootingSystem/EVENT_SHOOT logic migrated | code | Grep(Era.Core/Counter/Source/) | matches | `class.*ShootingSystem|class.*EjaculationSystem` | [x] |
| 24 | ChastityBelt_Check exposed via interface | code | Grep(Era.Core/Counter/Source/) | matches | `ChastityBeltCheck\(` | [x] |
| 25 | Phase 22 CLOTHES_Change_Knickers uses stub | code | Grep(Era.Core/Counter/Source/) | not_matches | `CLOTHES_Change_Knickers` | [x] |
| 26 | IKnickersSystem stub interface exists | code | Grep(Era.Core/Counter/Source/) | matches | `interface IKnickersSystem` | [x] |
| 27 | IComHandler interface defined in Counter namespace | code | Grep(Era.Core/Counter/) | matches | `interface IComHandler` | [x] |
| 28 | IEngineVariables pre-existing methods preserved | code | Grep(Era.Core/Interfaces/IEngineVariables.cs, pattern="(Get|Set)\\w+\\(") | gte | 27 | [x] |
| 29 | IStringVariables pre-existing methods preserved | code | Grep(Era.Core/Interfaces/IStringVariables.cs, pattern="(Get|Set)\\w+\\(") | gte | 2 | [x] |
| 30 | Predecessor F801 declared | file | Grep(pm/features/feature-811.md) | matches | `Predecessor.*F801.*\[DONE\]` | [x] |
| 31 | Predecessor F812 declared | file | Grep(pm/features/feature-811.md) | matches | `Predecessor.*F812` | [x] |
| 32 | Equivalence tests contain actual equivalence assertions | code | Grep(Era.Core.Tests/Counter/Source/, pattern="Assert\\.\\w+\\(") | gte | 10 | [x] |
| 33 | COM dispatch verifies all 16 TFLAG handler registrations | code | Grep(Era.Core.Tests/Counter/Source/) | matches | `Assert\.\w+\(16|\.Count.*==.*16` | [x] |
| 34 | Equivalence tests contain ERB-behavioral assertions per subsystem | code | Grep(Era.Core.Tests/Counter/Source/, pattern="SourceCheck\|TouchSet\|MasterPose\|EventShoot\|CallCom\|EjacCheck\|SamenShoot\|SamenDirection") | gte | 4 | [x] |
| 35 | CHK_SEMEN_STAY branching test covers all 10 variants via parameterized tests | code | Grep(Era.Core.Tests/Counter/Source/, pattern="InlineData.*SemenStay\|Theory.*SemenStay\|SemenStay.*InlineData") | gte | 10 | [x] |
| 36 | ISourceCalculator method calls routed in SourceEntrySystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs, pattern="_sourceCalculator\\.") | gte | 35 | [x] |
| 37 | SourceEntrySystem dispatch routing test exists (WC vs non-WC) | code | Grep(Era.Core.Tests/Counter/Source/) | matches | `SourceCheck.*Wc\|WcBranch\|NonWcBranch\|WcCounter.*Route` | [x] |
| 38 | IUpVariables interface exists as distinct interface | code | Grep(Era.Core/Interfaces/) | matches | `interface IUpVariables` | [x] |
| 39 | SourceEntrySystem references ITouchStateManager | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ITouchStateManager` | [x] |
| 40 | EXP_UP inline calculation test exists for dual-use | code | Grep(Era.Core.Tests/Counter/Source/) | matches | `ExpUp\|EXP_UP\|exp.*[Uu]p` | [x] |
| 41 | Orchestration interfaces not leaked to non-orchestrator classes | code | Grep(Era.Core/Counter/Source/ComCaller.cs) | not_matches | `ICounterSystem\|IWcCounterSystem\|ISourceCalculator\|ICounterSourceHandler\|IWcCounterSourceHandler\|ICombinationCounter\|IWcCombinationCounter` | [x] |
| 42 | Orchestration interfaces not leaked to ShootingSystem | code | Grep(Era.Core/Counter/Source/ShootingSystem.cs) | not_matches | `ICounterSystem\|IWcCounterSystem\|ISourceCalculator\|ICounterSourceHandler\|IWcCounterSourceHandler\|ICombinationCounter\|IWcCombinationCounter` | [x] |
| 43 | SourceEntrySystem calls ICounterSystem.SelectAction | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `_counterSystem\.SelectAction\|ICounterSystem.*SelectAction` | [x] |
| 44 | Orchestration interfaces not leaked to TouchStateManager | code | Grep(Era.Core/Counter/Source/TouchStateManager.cs) | not_matches | `ICounterSystem\|IWcCounterSystem\|ISourceCalculator\|ICounterSourceHandler\|IWcCounterSourceHandler\|ICombinationCounter\|IWcCombinationCounter` | [x] |
| 45 | ShootingSystem uses BitfieldUtility.SetBit | code | Grep(Era.Core/Counter/Source/ShootingSystem.cs) | matches | `BitfieldUtility\.SetBit` | [x] |
| 46 | CSTR write accessor exists (SetCharacterString on ICharacterStringVariables) | code | Grep(Era.Core/Interfaces/ICharacterStringVariables.cs) | matches | `SetCharacterString\(` | [x] |
| 47 | EXP_UP calculation via CheckExpUp in SourceEntrySystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `CheckExpUp\|checkExpUp\|_counterUtilities\.CheckExpUp` | [x] |
| 48 | SourceEntrySystem references ComCaller/IComDispatcher | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ComCaller\|IComDispatcher\|_comCaller` | [x] |
| 49 | Stub interface for WC Combination Counter | code | Grep(Era.Core/Counter/) | matches | `interface IWcCombinationCounter` | [x] |
| 50 | Non-orchestrator interface files do not reference orchestration or sibling-dispatch interfaces | code | Grep(Era.Core/Counter/Source/ITouchStateManager.cs) + Grep(Era.Core/Counter/Source/ISourceSystem.cs) + Grep(Era.Core/Counter/Source/IKnickersSystem.cs) | not_matches | `ICounterSystem\|IWcCounterSystem\|ISourceCalculator\|ICounterSourceHandler\|IWcCounterSourceHandler\|ICombinationCounter\|IWcCombinationCounter` | [x] |
| 51 | SourceEntrySystem WC routing condition references talent predicate | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `肉便器\|WcMode\|IsWc` | [x] |
| 52 | ITrainingCheckService stub interface exists | code | Grep(Era.Core/Counter/) | matches | `interface ITrainingCheckService` | [x] |
| 53 | IKojoMessageService stub interface exists | code | Grep(Era.Core/Counter/) | matches | `interface IKojoMessageService` | [x] |
| 54 | INtrUtilityService stub interface exists | code | Grep(Era.Core/Counter/) | matches | `interface INtrUtilityService` | [x] |
| 55 | IWcSexHaraService stub interface exists | code | Grep(Era.Core/Counter/) | matches | `interface IWcSexHaraService` | [x] |
| 56 | SourceEntrySystem references all external facade interfaces | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ITrainingCheckService\|IKojoMessageService\|INtrUtilityService\|IWcSexHaraService` | [x] |
| 57 | ComCaller handles SCOM dispatch (TRYCALLFORM SCOM{TFLAG:50} pattern) | code | Grep(Era.Core/Counter/Source/ComCaller.cs) | matches | `Scom\|SCOM\|ScomDispatch` | [x] |
| 58 | ComCaller handles CAN_COM/COM dispatch by SELECTCOM (TRYCALLFORM COM{SELECTCOM} pattern) | code | Grep(Era.Core/Counter/Source/ComCaller.cs) | matches | `SelectCom\|SELECTCOM\|ComDispatch.*selectCom\|selectCom.*dispatch` | [x] |
| 59 | ComCaller implements TRYCALLFORM fallback behavior (no-op when handler absent) | code | Grep(Era.Core/Counter/Source/ComCaller.cs) | matches | `TryGet\|ContainsKey\|TryInvoke\|null.*handler\|handler.*null` | [x] |
| 60 | ShootingSystem consumes SetCharacterString for CSTR write | code | Grep(Era.Core/Counter/Source/ShootingSystem.cs) | matches | `SetCharacterString` | [x] |
| 61 | ITouchStateManager declares TouchSuccession method | code | Grep(Era.Core/Counter/Source/) | matches | `TouchSuccession\(` | [x] |
| 62 | ITouchStateManager declares ShowTouch method | code | Grep(Era.Core/Counter/Source/ITouchStateManager.cs) | matches | `ShowTouch\(` | [x] |
| 63 | ComCaller injects ITouchStateManager for MASTER_POSE calls | code | Grep(Era.Core/Counter/Source/ComCaller.cs) | matches | `ITouchStateManager\|MasterPose` | [x] |
| 64 | SourceEntrySystem injects existing external interfaces | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IOrgasmProcessor\|IVirginityManager\|IFavorCalculator\|IMarkSystem` | [x] |
| 65 | SourceEntrySystem specifically references IWcCombinationCounter | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IWcCombinationCounter` | [x] |
| 66 | SourceEntrySystem references IUpVariables | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IUpVariables` | [x] |
| 67 | ComCaller registers handlers for all 16 TFLAG:50 values | code | Grep(Era.Core/Counter/Source/ComCaller.cs, pattern="\\.Add\\(\\|\\[\\d") | gte | 16 | [x] |
| 68 | External facade stub interfaces do not reference orchestration interfaces | code | Grep(Era.Core/Counter/ITrainingCheckService.cs) + Grep(Era.Core/Counter/IKojoMessageService.cs) + Grep(Era.Core/Counter/INtrUtilityService.cs) + Grep(Era.Core/Counter/IWcSexHaraService.cs) | not_matches | `ICounterSystem\|IWcCounterSystem\|ISourceCalculator\|ICounterSourceHandler\|IWcCounterSourceHandler\|ICombinationCounter\|IWcCombinationCounter` | [x] |
| 69 | SourceEntrySystem specifically references ICounterSourceHandler | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ICounterSourceHandler` | [x] |
| 70 | SourceEntrySystem specifically references ICombinationCounter | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ICombinationCounter` | [x] |
| 71 | SourceEntrySystem specifically references IWcCounterSourceHandler | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IWcCounterSourceHandler` | [x] |
| 72 | Sibling-dispatch stub interfaces do not reference orchestration interfaces | code | Grep(Era.Core/Counter/ICounterSourceHandler.cs) + Grep(Era.Core/Counter/IWcCounterSourceHandler.cs) + Grep(Era.Core/Counter/ICombinationCounter.cs) + Grep(Era.Core/Counter/IWcCombinationCounter.cs) | not_matches | `ICounterSystem\|IWcCounterSystem\|ISourceCalculator` | [x] |
| 73 | SourceEntrySystem calls ITouchStateManager.TouchSuccession | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `\.TouchSuccession\(` | [x] |
| 74 | SourceEntrySystem calls ComCaller dispatch method | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `_comCaller\.\|_comDispatcher\.` | [x] |
| 75 | TouchStateManager references IConsoleOutput for ShowTouch display | code | Grep(Era.Core/Counter/Source/TouchStateManager.cs) | matches | `IConsoleOutput` | [x] |
| 76 | SourceEntrySystem references IRelationVariables for RELATION read | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IRelationVariables` | [x] |
| 77 | ITouchStateManager.MasterPose returns int (not CharacterId) | code | Grep(Era.Core/Counter/Source/ITouchStateManager.cs) | matches | `int MasterPose\(` | [x] |
| 78 | SourceEntrySystem references IKnickersSystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IKnickersSystem` | [x] |
| 79 | IComHandler does not reference orchestration interfaces | code | Grep(Era.Core/Counter/IComHandler.cs) | not_matches | `ICounterSystem\|IWcCounterSystem\|ISourceCalculator` | [x] |
| 80 | SourceDisplayHandler.cs exists in Counter/Source | code | Grep(Era.Core/Counter/Source/SourceDisplayHandler.cs) | matches | `class SourceDisplayHandler` | [x] |
| 81 | SourceEntrySystem delegates to SourceDisplayHandler | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `SourceDisplayHandler\|ISourceDisplayHandler\|_displayHandler` | [x] |
| 82 | SourceDisplayHandler references IConsoleOutput | code | Grep(Era.Core/Counter/Source/SourceDisplayHandler.cs) | matches | `IConsoleOutput` | [x] |
| 83 | SourceEntrySystem contains COM call entry condition logic | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `CanCom\|CAN_COM\|canCom\|ComCondition\|comCallCondition` | [x] |
| 84 | SourceEntrySystemTests has >= 5 Assert calls | code | Grep(Era.Core.Tests/Counter/Source/SourceEntrySystemTests.cs, pattern="Assert\\.\\w+\\(") | gte | 5 | [x] |
| 85 | TouchStateManagerTests has >= 2 Assert calls | code | Grep(Era.Core.Tests/Counter/Source/TouchStateManagerTests.cs, pattern="Assert\\.\\w+\\(") | gte | 2 | [x] |
| 86 | ComCallerTests has >= 2 Assert calls | code | Grep(Era.Core.Tests/Counter/Source/ComCallerTests.cs, pattern="Assert\\.\\w+\\(") | gte | 2 | [x] |
| 87 | ShootingSystemTests has >= 5 Assert calls | code | Grep(Era.Core.Tests/Counter/Source/ShootingSystemTests.cs, pattern="Assert\\.\\w+\\(") | gte | 5 | [x] |
| 88 | SourceDisplayHandler implements ShowSourceParamCng method | code | Grep(Era.Core/Counter/Source/SourceDisplayHandler.cs) | matches | `ShowSourceParamCng\(` | [x] |
| 89 | SourceDisplayHandlerTests has >= 5 Assert calls | code | Grep(Era.Core.Tests/Counter/Source/SourceDisplayHandlerTests.cs, pattern="Assert\\.\\w+\\(") | gte | 5 | [x] |
| 90 | SourceEntrySystem calls ShootingSystem dispatch method | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `_shootingSystem\.\w+\(\|EventShoot\(` | [x] |
| 91 | SourceDisplayHandler implements ChkSemenStay method | code | Grep(Era.Core/Counter/Source/SourceDisplayHandler.cs) | matches | `ChkSemenStay\(` | [x] |
| 92 | SourceDisplayHandler implements ShowSource method | code | Grep(Era.Core/Counter/Source/SourceDisplayHandler.cs) | matches | `ShowSource\(` | [x] |
| 93 | CHK_SEMEN_STAY test verifies CFLAG state mutation | code | Grep(Era.Core.Tests/Counter/Source/, pattern="SemenStay.*Cflag\|Cflag.*SemenStay\|CFLAG.*SemenStay") | gte | 3 | [x] |
| 94 | CAN_COM entry condition unit-tested | code | Grep(Era.Core.Tests/Counter/Source/SourceEntrySystemTests.cs) | matches | `CanCom\|CAN_COM\|ComCondition\|comCallCondition` | [x] |
| 95 | Orchestration interfaces not leaked to SourceDisplayHandler | code | Grep(Era.Core/Counter/Source/SourceDisplayHandler.cs) | not_matches | `ICounterSystem\|IWcCounterSystem\|ISourceCalculator\|ICounterSourceHandler\|IWcCounterSourceHandler\|ICombinationCounter\|IWcCombinationCounter` | [x] |
| 96 | Equivalence tests contain concrete numeric expected values | code | Grep(Era.Core.Tests/Counter/Source/, pattern="Assert\\.Equal\\(-?\\d") | gte | 5 | [x] |
| 97 | SourceDisplayHandler behavioral equivalence test names | code | Grep(Era.Core.Tests/Counter/Source/SourceDisplayHandlerTests.cs) | matches | `ShowSourceParamCng\|ChkSemenStay\|ShowSource` | [x] |
| 98 | SourceEntrySystem specifically references IKojoMessageService | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IKojoMessageService` | [x] |
| 99 | SourceEntrySystem specifically references INtrUtilityService | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `INtrUtilityService` | [x] |
| 100 | SourceEntrySystem specifically references IWcSexHaraService | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IWcSexHaraService` | [x] |
| 101 | SourceEntrySystem specifically references IVirginityManager | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IVirginityManager` | [x] |
| 102 | SourceEntrySystem specifically references IFavorCalculator | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IFavorCalculator` | [x] |
| 103 | SourceEntrySystem specifically references IMarkSystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IMarkSystem` | [x] |
| 104 | SourceEntrySystem specifically references ITrainingCheckService | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ITrainingCheckService` | [x] |
| 105 | SourceEntrySystem specifically references IOrgasmProcessor | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IOrgasmProcessor` | [x] |
| 106 | ShowTouch dispatch call verified in SourceEntrySystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `\.ShowTouch\(` | [x] |
| 107 | SourceDisplayHandler does not contain CFLAG mutation code (display-only boundary) | code | Grep(Era.Core/Counter/Source/SourceDisplayHandler.cs) | not_matches | `SetCflag\|\.Cflag\s*=` | [x] |
| 108 | ISourceCalculator method name diversity in SourceEntrySystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs, pattern="_sourceCalculator\\.Source") | gte | 20 | [x] |
| 109 | CFLAG mutation logic exists in SourceEntrySystem or ShootingSystem (CHK_SEMEN_STAY boundary) | code | Grep(Era.Core/Counter/Source/) | matches | `SetCflag\|SemenStay\|精液残留` | [x] |
| 110 | SourceDisplayHandlerTests contains Assert.Equal numeric equivalence assertions | code | Grep(Era.Core.Tests/Counter/Source/SourceDisplayHandlerTests.cs, pattern="Assert\\.Equal\\(") | gte | 3 | [x] |
| 111 | ICounterSourceHandler declares HandleCounterSource method | code | Grep(Era.Core/Counter/ICounterSourceHandler.cs) | matches | `HandleCounterSource\(` | [x] |
| 112 | IWcCounterSourceHandler declares HandleWcCounterSource method | code | Grep(Era.Core/Counter/IWcCounterSourceHandler.cs) | matches | `HandleWcCounterSource\(` | [x] |
| 113 | ICombinationCounter declares AccumulateCombinations method | code | Grep(Era.Core/Counter/ICombinationCounter.cs) | matches | `AccumulateCombinations\(` | [x] |
| 114 | IWcCombinationCounter declares AccumulateCombinations method | code | Grep(Era.Core/Counter/IWcCombinationCounter.cs) | matches | `AccumulateCombinations\(` | [x] |
| 115 | TouchStateManagerTests contains TouchSuccession behavioral test | code | Grep(Era.Core.Tests/Counter/Source/TouchStateManagerTests.cs) | matches | `TouchSuccession` | [x] |
| 116 | SourceEntrySystem implements ISourceSystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `(: |, )ISourceSystem` | [x] |
| 117 | SourceEntrySystem DI field count within cap (C16: max 25) | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs, pattern="private readonly I") | lte | 25 | [x] |
| 118 | SourceEntrySystem constructor injects ISourceCalculator (naming-agnostic) | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ISourceCalculator` | [x] |
| 119 | ISourceCalculator declares all 35 SOURCE_* methods | code | Grep(Era.Core/Counter/ISourceCalculator.cs, pattern="void Source") | gte | 35 | [x] |
| 120 | SCOM dispatch path unit-tested | code | Grep(Era.Core.Tests/Counter/Source/ComCallerTests.cs) | matches | `Scom\|ScomDispatch\|SCOM\|scom` | [x] |
| 121 | SelectCom dispatch path unit-tested | code | Grep(Era.Core.Tests/Counter/Source/ComCallerTests.cs) | matches | `SelectCom\|selectCom\|SELECTCOM\|ComSelect` | [x] |
| 122 | ChkWcEquip method exists in SourceEntrySystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ChkWcEquip\|CheckWcEquip` | [x] |
| 123 | ChkInsRotorV method exists in SourceEntrySystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ChkInsRotorV\|CheckInsertRotorV\|RotorV` | [x] |
| 124 | ChkInsRotorA method exists in SourceEntrySystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ChkInsRotorA\|CheckInsertRotorA\|RotorA` | [x] |
| 125 | ChkSemenBath method exists in SourceEntrySystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ChkSemenBath\|CheckSemenBath\|SemenBath` | [x] |
| 126 | WC routing test references 肉便器 predicate or C# equivalent | code | Grep(Era.Core.Tests/Counter/Source/SourceEntrySystemTests.cs) | matches | `肉便器\|WcMode\|IsWc` | [x] |
| 127 | SourceEntrySystem references IShrinkageSystem | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `IShrinkageSystem\|_shrinkageSystem` | [x] |
| 128 | SourceEntrySystem references ICounterUtilities | code | Grep(Era.Core/Counter/Source/SourceEntrySystem.cs) | matches | `ICounterUtilities\|_counterUtilities` | [x] |
| 129 | ShootingSystem cross-character ejaculation test exists | code | Grep(Era.Core.Tests/Counter/Source/ShootingSystemTests.cs) | matches | `CrossCharacter\|MultiCharacter\|MultiTarget\|MultipleTarget\|cross.*ejacu` | [x] |
| 130 | TouchStateManager implementation class exists | code | Grep(Era.Core/Counter/Source/TouchStateManager.cs) | matches | `class TouchStateManager` | [x] |
| 131 | @SOURCE_CHECK sub-function equivalence test names in SourceEntrySystemTests | code | Grep(Era.Core.Tests/Counter/Source/SourceEntrySystemTests.cs, pattern="ChkWcEquip\|CheckWcEquip\|WcEquip\|RotorV\|RotorA\|SemenBath") | gte | 4 | [x] |
| 132 | COM dispatch TFLAG:50→handler behavioral mapping test (parameterized) | code | Grep(Era.Core.Tests/Counter/Source/ComCallerTests.cs, pattern="InlineData\|Theory") | gte | 3 | [x] |
| 133 | ChkWcEquip equivalence test assertion density | code | Grep(Era.Core.Tests/Counter/Source/SourceEntrySystemTests.cs, pattern="WcEquip\|CheckWcEquip\|ChkWcEquip") | gte | 3 | [x] |
| 134 | IShrinkageSystem interface declaration exists | code | Grep(Era.Core/Counter/) | matches | `interface IShrinkageSystem` | [x] |
| 135 | ICounterUtilities interface declaration exists | code | Grep(Era.Core/Counter/) | matches | `interface ICounterUtilities` | [x] |
| 136 | ShootingSystem injects ICharacterStringVariables | code | Grep(Era.Core/Counter/Source/ShootingSystem.cs) | matches | `ICharacterStringVariables` | [x] |
| 137 | ISourceSystem SSOT enforcement (single implementer) | code | Grep(Era.Core/Counter/Source/, pattern="(: |, )ISourceSystem") | lte | 1 | [x] |

### AC Details

**AC#1: Source C# files exist in Counter/Source namespace (5 impl + 3 interface = 8 files)**
- **Test**: `Glob("Era.Core/Counter/Source/*.cs")` count >= 8
- **Expected**: At least 8 .cs files exist: 5 implementation (SourceEntrySystem.cs, TouchStateManager.cs, ComCaller.cs, ShootingSystem.cs, SourceDisplayHandler.cs) + 3 interface (ITouchStateManager.cs, ISourceSystem.cs, IKnickersSystem.cs)
- **Rationale**: Philosophy claims "all ERB counter logic migrated to C#" — all 4 SOURCE ERB files must have C# equivalents, plus SourceDisplayHandler for display output and 3 interface files from Tasks 7-8 (C3)

**AC#2: Source C# test files exist**
- **Test**: `Glob("Era.Core.Tests/Counter/Source/*.cs")`
- **Expected**: At least 5 test .cs files exist (one per migrated ERB: SourceEntrySystemTests.cs, TouchStateManagerTests.cs, ComCallerTests.cs, ShootingSystemTests.cs, SourceDisplayHandlerTests.cs)
- **Rationale**: Verifies equivalence test files are created for the migrated code (C2)

**AC#3: No technical debt markers in migrated C# files**
- **Test**: `Grep(path="Era.Core/Counter/", glob="*.cs", pattern="TODO|FIXME|HACK")`
- **Expected**: No matches found in any F811-produced/modified .cs files (Counter/)
- **Rationale**: Zero technical debt requirement from Philosophy and C1 constraint (scope narrowed to F811-produced files; pre-existing TODOs in other Era.Core files are out of scope)

**AC#4: Equivalence tests build**
- **Test**: `dotnet build Era.Core.Tests/`
- **Expected**: Build succeeds with zero errors
- **Rationale**: All migrated code and tests must compile under TreatWarningsAsErrors (C2)

**AC#5: Equivalence tests pass**
- **Test**: `dotnet test Era.Core.Tests/`
- **Expected**: All tests pass
- **Rationale**: Equivalence tests must verify C# behavior matches legacy ERB (C2)

**AC#6: SourceEntrySystem dispatches to ICounterSystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ICounterSystem")`
- **Expected**: Pattern found in source files
- **Rationale**: F811 dispatches to EVENT_COUNTER via ICounterSystem (C3, SOURCE.ERB:37)

**AC#7: SourceEntrySystem dispatches to IWcCounterSystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IWcCounterSystem")`
- **Expected**: Pattern found in source files
- **Rationale**: F811 dispatches to EVENT_WC_COUNTER via IWcCounterSystem (C3, SOURCE.ERB:25)

**AC#8: ITouchStateManager interface defined with TOUCH_SET**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="interface ITouchStateManager")`
- **Expected**: Pattern found
- **Rationale**: ITouchStateManager is the cross-feature API boundary for 36+ external callers (C4, C10)

**AC#9: ITouchStateManager declares TOUCH_SET method**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="TouchSet\\(")`
- **Expected**: Pattern found
- **Rationale**: TOUCH_SET is called 28 times by F803 and 42 times by F805 (C4, C10)

**AC#10: ITouchStateManager declares MASTER_POSE method**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="MasterPose\\(")`
- **Expected**: Pattern found
- **Rationale**: MASTER_POSE called by F802 and F809 (C4, C10)

**AC#11: ITouchStateManager declares TOUCH_RESET_M method**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="TouchResetM\\(")`
- **Expected**: Pattern found
- **Rationale**: TOUCH_RESET_M method required per C10 constraint

**AC#12: ITouchStateManager declares TOUCH_RESET_T method**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="TouchResetT\\(")`
- **Expected**: Pattern found
- **Rationale**: TOUCH_RESET_T method required per C10 constraint

**AC#13: COM dispatch uses dictionary/strategy pattern**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="Dictionary.*ComHandler|IComDispatcher")`
- **Expected**: Pattern found
- **Rationale**: CALLFORM COM_ABLE{N} dynamic dispatch requires strategy/dictionary pattern (C5)

**AC#14: GetPrevCom added to IEngineVariables**
- **Test**: `Grep(path="Era.Core/Interfaces/IEngineVariables.cs", pattern="GetPrevCom\\(")`
- **Expected**: Pattern found
- **Rationale**: PREVCOM interface gap must be resolved for SOURCE.ERB:186,340,494 (C8)

**AC#15: SetPrevCom added to IEngineVariables**
- **Test**: `Grep(path="Era.Core/Interfaces/IEngineVariables.cs", pattern="SetPrevCom\\(")`
- **Expected**: Pattern found
- **Rationale**: PREVCOM setter required for paired get/set (C8)


**AC#16: UP accessor added to IUpVariables interface**
- **Test**: `Grep(path="Era.Core/Interfaces/", pattern="int GetUp\\(int")`
- **Expected**: Pattern found
- **Rationale**: IUpVariables interface read/write required at SOURCE.ERB:342-351 (C8)

**AC#17: UP setter added**
- **Test**: `Grep(path="Era.Core/Interfaces/", pattern="void SetUp\\(int")`
- **Expected**: Pattern found
- **Rationale**: Paired setter for UP accessor (C8)

**AC#18: SETBIT bitfield utility exists**
- **Test**: `Grep(path="Era.Core/", pattern="SetBit\\(.*int.*int")`
- **Expected**: Pattern found
- **Rationale**: SETBIT operation used extensively in SOURCE_SHOOT.ERB (C13)

**AC#19: SourceEntrySystem consumes F804's IWcCounterSystem.SelectAction**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="_wcCounterSystem\\.SelectAction|IWcCounterSystem.*SelectAction")`
- **Expected**: Pattern found (verifying actual SelectAction call, not just interface reference)
- **Rationale**: F804 already created IWcCounterSystem with SelectAction(CharacterId, int). F811 must consume it, not recreate (C3)

**AC#20: ICounterSourceHandler interface exists (F803-provided, consumed by F811)**
- **Test**: `Grep(path="Era.Core/Counter/", pattern="interface ICounterSourceHandler")`
- **Expected**: Pattern found
- **Rationale**: F811 calls EVENT_COUNTER_SOURCE (SOURCE.ERB:38). ICounterSourceHandler is provided by F803 [DONE]; F811 consumes it via DI injection, not creates it.

**AC#21: Stub interface for WC Counter Source handler**
- **Test**: `Grep(path="Era.Core/Counter/", pattern="interface IWcCounterSourceHandler")`
- **Expected**: Pattern found
- **Rationale**: F811 calls EVENT_WC_COUNTER_SOURCE (SOURCE.ERB:26); stub for F805 (C14)

**AC#22: Stub interface for Combination Counter**
- **Test**: `Grep(path="Era.Core/Counter/", pattern="interface ICombinationCounter")`
- **Expected**: Pattern found
- **Rationale**: F811 calls EVENT_COUNTER_COMBINATION (SOURCE.ERB:40); abstraction interface over F802's existing CounterCombination for DI injection into SourceEntrySystem (C14)

**AC#23: ShootingSystem/EVENT_SHOOT logic migrated**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="class.*ShootingSystem|class.*EjaculationSystem")`
- **Expected**: Pattern found
- **Rationale**: SOURCE_SHOOT.ERB EVENT_SHOOT self-contained shooting logic must be migrated (C6)

**AC#24: ChastityBelt_Check exposed via interface**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="ChastityBeltCheck\\(")`
- **Expected**: Pattern found
- **Rationale**: EVENTCOMEND.ERB:136 calls ChastityBelt_Check; must be exposed for external callers

**AC#25: Phase 22 CLOTHES_Change_Knickers uses stub**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="CLOTHES_Change_Knickers")`
- **Expected**: Pattern NOT found (direct call to Phase 22 function should not exist; use stub interface instead)
- **Rationale**: Phase 22 dependency must be abstracted behind stub (C9)

**AC#26: IKnickersSystem stub interface exists**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="interface IKnickersSystem")`
- **Expected**: Pattern found
- **Rationale**: Phase 22 CLOTHES_Change_Knickers must be abstracted behind IKnickersSystem stub (C9, upstream from tech-designer)


**AC#27: IComHandler interface defined in Counter namespace**
- **Test**: `Grep(path="Era.Core/Counter/", pattern="interface IComHandler")`
- **Expected**: Pattern found
- **Rationale**: ComCaller uses Dictionary<int, IComHandler> for COM dispatch; interface must exist (C5)

**AC#28: IEngineVariables pre-existing methods preserved**
- **Test**: `Grep(path="Era.Core/Interfaces/IEngineVariables.cs", pattern="(Get|Set)\\w+\\(")` count >= 27
- **Expected**: At least 27 method signatures (25 existing + 2 new = 27 after F811)
- **Rationale**: Backward compatibility: extending IEngineVariables must not remove existing methods

**AC#29: IStringVariables pre-existing methods preserved**
- **Test**: `Grep(path="Era.Core/Interfaces/IStringVariables.cs", pattern="(Get|Set)\\w+\\(")` count >= 2
- **Expected**: At least 2 method signatures (2 existing + 0 new = 2 after F811; CSTR resolved by F804 via GetCharacterString)
- **Rationale**: Backward compatibility: IStringVariables must preserve existing methods (F811 no longer modifies IStringVariables)

**AC#30: Predecessor F801 declared**
- **Test**: `Grep(path="pm/features/feature-811.md", pattern="Predecessor.*F801.*\\[DONE\\]")`
- **Expected**: Pattern found
- **Rationale**: Goal item 1 requires correct predecessor declaration for F801

**AC#31: Predecessor F812 declared**
- **Test**: `Grep(path="pm/features/feature-811.md", pattern="Predecessor.*F812")`
- **Expected**: Pattern found
- **Rationale**: Goal item 1 requires correct predecessor declaration for F812

**AC#32: Equivalence tests contain actual equivalence assertions**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/", pattern="Assert\\.\\w+\\(")` count >= 10
- **Expected**: At least 10 assertion calls across test files
- **Rationale**: Philosophy requires "equivalence-tested against legacy behavior" — tests must contain meaningful assertions, not just compile (C2)

**AC#33: COM dispatch verifies all 16 TFLAG handler registrations**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/", pattern="Assert\\.\\w+\\(16|\\.Count.*==.*16")`
- **Expected**: Pattern found (test asserts handler count equals 16 within Assert context)
- **Rationale**: Verifies test code explicitly asserts the COM dispatch dictionary contains exactly 16 handler entries (one per TFLAG:50 value). Requires Assert call context or .Count comparison — prevents false-pass from comments or string literals containing "16".

**AC#34: Equivalence tests contain ERB-behavioral assertions per subsystem**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/", pattern="SourceCheck|TouchSet|MasterPose|EventShoot|CallCom|EjacCheck|SamenShoot|SamenDirection")` count >= 4
- **Expected**: At least 4 matches across test files — confirming test methods reference migrated ERB functions by name
- **Rationale**: Philosophy requires "equivalence-tested against legacy behavior" per subsystem (SourceEntrySystem, TouchStateManager, ShootingSystem, ComCaller). Grep for ERB function names in tests confirms behavioral-level testing exists. Includes EJAC_CHECK, SAMEN_SHOOT, SAMEN_DIRECTION sub-functions from SOURCE_SHOOT.ERB (C2)

**AC#35: CHK_SEMEN_STAY branching test covers all 10 variants via parameterized tests**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/", pattern="InlineData.*SemenStay|Theory.*SemenStay|SemenStay.*InlineData")`
- **Expected**: Count >= 10 InlineData/Theory entries referencing SemenStay variants
- **Rationale**: CHK_SEMEN_STAY has 10 variant sub-functions (~370 lines) flagged as a risk. Test must cover all 10 branching variants. Changed from test naming pattern (SemenStay.*With etc.) to InlineData/Theory parameterized test data verification — prevents false-pass from test methods named after variants but containing no actual branch assertions. (Risk mitigation, Philosophy "equivalence-tested")

**AC#36: ISourceCalculator method calls routed in SourceEntrySystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="_sourceCalculator\\.")`
- **Expected**: Count >= 35
- **Rationale**: C3 requires "AC must verify C# code references ISourceCalculator" and C11 requires "35 SOURCE_* function calls route through interface". F812 (predecessor) provides ISourceCalculator with exactly 35 methods (verified: ISourceCalculator.cs lines 13-47). SourceEntrySystem must make >= 35 `_sourceCalculator.` method calls corresponding to all 35 SOURCE_* functions at SOURCE.ERB:127-335. Note: ITrainingCheckService covers TRACHECK.ERB utility functions (SOURCE.ERB:14, 100, 101 etc.) which are outside the SOURCE.ERB:127-335 range. All 35 SOURCE_* calls within 127-335 route exclusively through ISourceCalculator; there is no overlap with ITrainingCheckService.

**AC#37: SourceEntrySystem dispatch routing test exists (WC vs non-WC)**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/", pattern="SourceCheck.*Wc|WcBranch|NonWcBranch|WcCounter.*Route")`
- **Expected**: Pattern found
- **Rationale**: Philosophy claims "owns the main entry point (@SOURCE_CHECK) that dispatches to Main Counter (F801) or WC Counter (F804) subsystems". The routing decision (WC vs non-WC branch) must be unit tested (C3)

**AC#38: IUpVariables interface exists as distinct interface**
- **Test**: `Grep(path="Era.Core/Interfaces/", pattern="interface IUpVariables")`
- **Expected**: Pattern found
- **Rationale**: ISP design decision (Key Decisions table) mandates UP methods in a separate IUpVariables interface, not bundled into IRelationVariables. This AC enforces the architectural decision (C8)

**AC#39: SourceEntrySystem references ITouchStateManager**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ITouchStateManager")`
- **Expected**: Pattern found
- **Rationale**: Philosophy claims SourceEntrySystem "manages touch state (TOUCH_SET/MASTER_POSE/TOUCH_SUCCESSION/SHOW_TOUCH)". This AC verifies SourceEntrySystem actually injects/references ITouchStateManager, not just that the interface exists elsewhere (AC#8-12 verify interface definition)

**AC#40: EXP_UP inline calculation test exists for dual-use**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/", pattern="ExpUp|EXP_UP|exp.*[Uu]p")`
- **Expected**: Pattern found
- **Rationale**: C12 constraint flags EXP_UP dual-use (boolean IF EXP_UP and value {EXP_UP(N,chr)}). Inline calculation EXP[i]-TCVAR[400+i] must be tested for both boolean (non-zero = true) and value extraction paths. This AC verifies test code specifically references EXP_UP behavior, complementing general equivalence tests (AC#39, AC#41)

**AC#41: Orchestration interfaces not leaked to non-orchestrator classes**
- **Test**: `Grep(path="Era.Core/Counter/Source/ComCaller.cs", pattern="ICounterSystem|IWcCounterSystem|ISourceCalculator")`
- **Expected**: Pattern NOT found (orchestration interfaces belong only in SourceEntrySystem)
- **Rationale**: Philosophy claims SourceEntrySystem is "SSOT for counter event orchestration". Verifying ComCaller (and by extension ShootingSystem, TouchStateManager) do not reference orchestration interfaces enforces single-responsibility. Orchestration dispatch must flow through SourceEntrySystem only. Part of orchestration boundary enforcement set: AC#41 (ComCaller), AC#42 (ShootingSystem), AC#44 (TouchStateManager).

**AC#42: Orchestration interfaces not leaked to ShootingSystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/ShootingSystem.cs", pattern="ICounterSystem|IWcCounterSystem|ISourceCalculator")`
- **Expected**: Pattern NOT found (ShootingSystem handles EVENT_SHOOT independently; orchestration dispatch belongs only in SourceEntrySystem)
- **Rationale**: Complements AC#41 (ComCaller) to enforce Philosophy's SSOT orchestration claim. Part of orchestration boundary enforcement set: AC#41 (ComCaller), AC#42 (ShootingSystem), AC#44 (TouchStateManager).

**AC#43: SourceEntrySystem calls ICounterSystem.SelectAction**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="_counterSystem\\.SelectAction|ICounterSystem.*SelectAction")`
- **Expected**: Pattern found (verifying actual method call, not just interface reference)
- **Rationale**: Mirrors the AC#7→AC#19 pattern (IWcCounterSystem reference → SelectAction call verification) for the non-WC branch. C3 constraint explicitly requires "Verify ICounterSystem.SelectAction is called". AC#6 only checks interface presence; AC#43 verifies the actual SelectAction method call.

**AC#44: Orchestration interfaces not leaked to TouchStateManager**
- **Test**: `Grep(path="Era.Core/Counter/Source/TouchStateManager.cs", pattern="ICounterSystem|IWcCounterSystem|ISourceCalculator")`
- **Expected**: Pattern NOT found (TouchStateManager is a pure touch-state utility; orchestration dispatch belongs only in SourceEntrySystem)
- **Rationale**: Completes orchestration boundary enforcement set: AC#41 (ComCaller), AC#42 (ShootingSystem), AC#44 (TouchStateManager). All non-orchestrator classes in Counter/Source/ now verified.

**AC#45: ShootingSystem uses BitfieldUtility.SetBit**
- **Test**: `Grep(path="Era.Core/Counter/Source/ShootingSystem.cs", pattern="BitfieldUtility\\.SetBit")`
- **Expected**: Pattern found
- **Rationale**: SOURCE_SHOOT.ERB uses SETBIT extensively. AC#24 verifies BitfieldUtility exists; AC#52 verifies ShootingSystem actually uses it (no inline bit math bypassing the utility). Ensures zero technical debt.

**AC#46: CSTR write accessor exists (SetCharacterString on ICharacterStringVariables)**
- **Test**: `Grep(path="Era.Core/Interfaces/ICharacterStringVariables.cs", pattern="SetCharacterString\\(")`
- **Expected**: Pattern found
- **Rationale**: SOURCE_SHOOT.ERB:340 writes CSTR:ARG:11 = %CSTR:ARG:10%. F803 [WIP] created ICharacterStringVariables with SetCharacterString (ISP-based separation from IVariableStore). F811 consumes this interface for CSTR write operations.

**AC#47: EXP_UP calculation via CheckExpUp in SourceEntrySystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="CheckExpUp|checkExpUp|_counterUtilities\\.CheckExpUp")`
- **Expected**: Pattern found (confirms SourceEntrySystem calls ICounterUtilities.CheckExpUp for EXP_UP dual-use)
- **Rationale**: C12 requires EXP_UP dual-use (boolean + value). F803's ICounterUtilities.CheckExpUp (adopted per DEP-DRIFT resolution) eliminates SSOT duplication vs inline EXP[i]-TCVAR[400+i]. EXP_UP appears at SOURCE.ERB:113/231/413, all within SourceEntrySystem scope.

**AC#48: SourceEntrySystem references ComCaller/IComDispatcher**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ComCaller|IComDispatcher|_comCaller")`
- **Expected**: Pattern found (SourceEntrySystem injects and calls ComCaller for COM dispatch)
- **Rationale**: Philosophy claims SourceEntrySystem "owns ... COM calling". AC#48 verifies ComCaller doesn't leak orchestration interfaces (negative check); AC#55 verifies SourceEntrySystem actually references ComCaller (positive check). Without AC#55, an implementation could satisfy all other ACs while SourceEntrySystem never calls ComCaller.

**AC#49: Stub interface for WC Combination Counter**
- **Test**: `Grep(path="Era.Core/Counter/", pattern="interface IWcCombinationCounter")`
- **Expected**: Pattern found
- **Rationale**: SOURCE.ERB:28 calls EVENT_WC_COUNTER_COMBINATION in WC branch. Parallels ICombinationCounter (AC#28) for the non-WC branch. Stub for future WC combination implementation.

**AC#50: Non-orchestrator interface files do not reference orchestration or sibling-dispatch interfaces**
- **Test**: `Grep(path="Era.Core/Counter/Source/ITouchStateManager.cs", pattern="...")` + `Grep(path="Era.Core/Counter/Source/ISourceSystem.cs", pattern="...")` + `Grep(path="Era.Core/Counter/Source/IKnickersSystem.cs", pattern="...")`  where pattern = `ICounterSystem|IWcCounterSystem|ISourceCalculator|ICounterSourceHandler|IWcCounterSourceHandler|ICombinationCounter|IWcCombinationCounter`
- **Expected**: Pattern NOT found in any of the 3 interface files (only SourceEntrySystem.cs may reference orchestration and sibling-dispatch interfaces)
- **Rationale**: Complements per-class checks (AC#41 ComCaller, AC#42 ShootingSystem, AC#44 TouchStateManager) with interface file checks. Uses explicit file paths instead of glob negation (`!SourceEntrySystem.cs`) which is unreliable in the AC verification toolchain. Together AC#41/42/44/57 cover all non-SourceEntrySystem files in Counter/Source/.

**AC#51: SourceEntrySystem WC routing condition references talent predicate**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="肉便器|WcMode|IsWc")`
- **Expected**: Pattern found (SourceEntrySystem's WC routing condition must reference the 肉便器 talent check from SOURCE.ERB:21)
- **Rationale**: Philosophy claims SourceEntrySystem "owns the main entry point (@SOURCE_CHECK) that dispatches to Main Counter (F801) or WC Counter (F804) subsystems". The branching predicate (TALENT:MASTER:肉便器) must be present in SourceEntrySystem to ensure correct dispatch routing (C15). AC#44 only verifies a test exists; AC#58 verifies the condition itself.

**AC#52: ITrainingCheckService stub interface exists**
- **Test**: `Grep(path="Era.Core/Counter/", pattern="interface ITrainingCheckService")`
- **Expected**: Pattern found
- **Rationale**: TRACHECK.ERB and TRACHECK_*.ERB define ~10 utility functions called by SOURCE.ERB: TRACHECK_TIME (SOURCE.ERB:14), SOURCE_SEX_CHECK (SOURCE.ERB:100), PLAYER_SKILL_CHECK (SOURCE.ERB:101), EQUIP_CHECK (SOURCE.ERB:123), JUJUN_UP_CHECK (SOURCE.ERB:481), EXP_GOT_CHECK (SOURCE.ERB:369), TARGET_MILK_CHECK (SOURCE.ERB:226), MESSAGE_PALAMCNG_B2 (SOURCE.ERB:440), and function values MASTER_FAVOR_CHECK/TECHNIQUE_CHECK/MOOD_CHECK/REASON_CHECK (SOURCE.ERB:106-107). These training-check utilities are cohesively grouped as training state inspection functions, warranting a single stub facade interface. Stub only — full implementations remain in TRACHECK.ERB until migrated.

**AC#53: IKojoMessageService stub interface exists**
- **Test**: `Grep(path="Era.Core/Counter/", pattern="interface IKojoMessageService")`
- **Expected**: Pattern found
- **Rationale**: EVENT_KOJO.ERB and TOILET_EVENT_KOJO.ERB define 8 KOJO_MESSAGE variants called by SOURCE.ERB: KOJO_MESSAGE_COM (SOURCE.ERB:375), KOJO_MESSAGE_COUNTER (SOURCE.ERB:398), KOJO_MESSAGE_WC_COUNTER (SOURCE.ERB:384), KOJO_MESSAGE_PALAMCNG_A (SOURCE.ERB:459), KOJO_MESSAGE_PALAMCNG_B (SOURCE.ERB:441), KOJO_MESSAGE_PALAMCNG_C (SOURCE.ERB:463), KOJO_MESSAGE_PALAMCNG_D (SOURCE.ERB:467), KOJO_MESSAGE_PALAMCNG_E (SOURCE.ERB:436), KOJO_MESSAGE_MARKCNG (SOURCE.ERB:476). All are message-display functions for training dialogue output, cohesively grouped as a single message service facade. Stub only.

**AC#54: INtrUtilityService stub interface exists**
- **Test**: `Grep(path="Era.Core/Counter/", pattern="interface INtrUtilityService")`
- **Expected**: Pattern found
- **Rationale**: NTR_UTIL.ERB defines NTR_MARK_5 (SOURCE.ERB:211, SOURCE.ERB:390) and NTR_ADD_SURRENDER (SOURCE.ERB:901, SOURCE.ERB:963). Both functions are NTR-specific state mutation utilities (mark pleasure level, add surrender value) with cohesive NTR domain semantics, warranting a single stub facade interface. Stub only — full implementation in NTR_UTIL.ERB until migrated.

**AC#55: IWcSexHaraService stub interface exists**
- **Test**: `Grep(path="Era.Core/Counter/", pattern="interface IWcSexHaraService")`
- **Expected**: Pattern found
- **Rationale**: WC_SexHara.ERB and related files define 3 functions called by SOURCE.ERB: WC_SexHara (SOURCE.ERB:30), WC_SexHara_SOURCE (SOURCE.ERB:32), WC_SexHara_MESSAGEbase (SOURCE.ERB:388). All are WC sexual harassment game logic functions (check/apply/display), cohesively grouped as a single WC domain service facade. Stub only — full implementation remains in WC_SexHara.ERB until migrated.

**AC#56: SourceEntrySystem references all external facade interfaces**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ITrainingCheckService|IKojoMessageService|INtrUtilityService|IWcSexHaraService")`
- **Expected**: Pattern found (SourceEntrySystem must inject and reference all 4 facade interfaces for TRACHECK/KOJO_MESSAGE/NTR_UTIL/WC_SexHara dependencies)
- **Rationale**: Verifies SourceEntrySystem actually wires the 4 facade stub interfaces via constructor injection, not just that the interfaces exist elsewhere. Without this AC, Task 16 (interface creation) could be satisfied without SourceEntrySystem consuming the interfaces. Mirrors the pattern of AC#33 (sibling stub interfaces) and AC#46 (ITouchStateManager injection).

**AC#57: ComCaller handles SCOM dispatch (TRYCALLFORM SCOM{TFLAG:50} pattern)**
- **Test**: `Grep(path="Era.Core/Counter/Source/ComCaller.cs", pattern="Scom|SCOM|ScomDispatch")`
- **Expected**: Pattern found (ComCaller implements the SCOM{TFLAG:50} dispatch branch from SOURCE_CALLCOM.ERB:74/79)
- **Rationale**: SOURCE_CALLCOM.ERB:74 uses TRYCCALLFORM CAN_SCOM{TFLAG:50} and line 79 uses TRYCALLFORM SCOM{TFLAG:50}. These are a secondary dispatch path for special COM variants (e.g. シックスナイン, 岩清水, etc.) that fires when TFLAG:50 is set AND the special COM ability check passes (COM_ABLE{500+TFLAG:50} and CAN_SCOM{TFLAG:50}(1)). ComCaller must implement this path alongside the regular COM_ABLE dispatch (AC#13). Absence would silently skip all SCOM-type actions.

**AC#58: ComCaller handles CAN_COM/COM dispatch by SELECTCOM (TRYCALLFORM COM{SELECTCOM} pattern)**
- **Test**: `Grep(path="Era.Core/Counter/Source/ComCaller.cs", pattern="SelectCom|SELECTCOM|selectCom")`
- **Expected**: Pattern found (ComCaller references SELECTCOM for the CAN_COM/COM dispatch path at SOURCE_CALLCOM.ERB:88/93)
- **Rationale**: SOURCE_CALLCOM.ERB:88 uses TRYCCALLFORM CAN_COM{SELECTCOM} and line 93 uses TRYCALLFORM COM{SELECTCOM}. This is the fallback dispatch path executed when TFLAG:50 is 0 (no special COM variant applies). It dispatches directly by SELECTCOM value (the currently selected action number). ComCaller must implement this path; without it the primary COM dispatch for all non-special actions would be absent.

**AC#59: ComCaller implements TRYCALLFORM fallback behavior (no-op when handler absent)**
- **Test**: `Grep(path="Era.Core/Counter/Source/ComCaller.cs", pattern="TryGet|ContainsKey|TryInvoke|null.*handler|handler.*null")`
- **Expected**: Pattern found (ComCaller uses try-pattern lookup, not direct Dictionary access, mirroring TRYCALLFORM no-op semantics)
- **Rationale**: TRYCALLFORM in ERB silently does nothing if the target function does not exist (unlike CALLFORM which throws). SOURCE_CALLCOM.ERB uses TRYCALLFORM for all three dispatch paths (lines 72, 74, 79, 88, 93). ComCaller must replicate this fallback behavior — attempting a dispatch and no-oping when no handler is registered — rather than throwing on missing keys. This is the C# equivalent of TRYCALLFORM vs CALLFORM semantics.

**AC#60: ShootingSystem consumes SetCharacterString for CSTR write**
- **Test**: `Grep(path="Era.Core/Counter/Source/ShootingSystem.cs", pattern="SetCharacterString")`
- **Expected**: Pattern found (ShootingSystem calls SetCharacterString for CSTR:ARG:11 write at SOURCE_SHOOT.ERB:340)
- **Rationale**: AC#53 verifies SetCharacterString exists in the interface layer but does not verify consumption. ShootingSystem migrates SOURCE_SHOOT.ERB which writes CSTR:ARG:11 = %CSTR:ARG:10% at line 340. Without this AC, the interface addition (Task 1) could be satisfied while the actual call site (Task 12) remains unverified.

**AC#61: ITouchStateManager declares TouchSuccession method**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="TouchSuccession\\(")`
- **Expected**: Pattern found (ITouchStateManager or TouchStateManager declares/implements TouchSuccession)
- **Rationale**: SOURCE_POSE.ERB:71-285 defines TOUCH_SUCCESSION (215 lines), called by SOURCE.ERB:18. Technical Design Approach item 1 mentions "Calls ITouchStateManager (owned here) for TOUCH_SUCCESSION" but no AC previously verified this method exists. TouchSuccession manages the touch state transition logic at the start of each SOURCE_CHECK cycle.

**AC#62: ITouchStateManager declares ShowTouch method**
- **Test**: `Grep(path="Era.Core/Counter/Source/ITouchStateManager.cs", pattern="ShowTouch\\(")`
- **Expected**: Pattern found
- **Rationale**: SOURCE_POSE.ERB:4-70 defines SHOW_TOUCH (67 lines), a display function showing current touch state. Per Interface Snippets and Philosophy ("manages touch state ... SHOW_TOUCH"), ShowTouch belongs exclusively on ITouchStateManager. External callers (INFO.ERB:11) inject ITouchStateManager to call ShowTouch.

**AC#63: ComCaller injects ITouchStateManager for MASTER_POSE calls**
- **Test**: `Grep(path="Era.Core/Counter/Source/ComCaller.cs", pattern="ITouchStateManager|MasterPose")`
- **Expected**: Pattern found (ComCaller references ITouchStateManager or MasterPose method)
- **Rationale**: SOURCE_CALLCOM.ERB:42,49,52,55 calls MASTER_POSE 4 times to determine TFLAG:50 for combination actions (交互挿入, ダブルフェラ, ダブル素股, ダブルパイズリ). ComCaller must inject ITouchStateManager to access MasterPose. Without this AC, ComCaller could be implemented without the MasterPose dependency, causing missing TFLAG:50 determination for combination actions.

**AC#64: SourceEntrySystem injects existing external interfaces**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IOrgasmProcessor|IVirginityManager|IFavorCalculator|IMarkSystem")`
- **Expected**: Pattern found (SourceEntrySystem references at least one of these existing external interfaces)
- **Rationale**: SOURCE.ERB directly calls ORGASM_ADD (line 202), LOST_VIRGIN (lines 131-133), FAVOR_CALC (line 544), and MARK_GOT_CHECK (line 471). These are listed in Dependencies as External/Exists but had no AC verifying SourceEntrySystem consumes them. AC#63 only covers the 4 new facade stubs; AC#71 covers the 4 pre-existing external interfaces.

**AC#65: SourceEntrySystem specifically references IWcCombinationCounter**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IWcCombinationCounter")`
- **Expected**: Pattern found
- **Rationale**: The former sibling-dispatch OR-pattern AC (superseded) passed if ANY of ICounterSourceHandler/IWcCounterSourceHandler/ICombinationCounter/IWcCombinationCounter matched. IWcCombinationCounter could be omitted while the OR still passed. AC#56 only verifies the interface exists, not that SourceEntrySystem injects it. AC#72 closes this gap by requiring SourceEntrySystem.cs to specifically reference IWcCombinationCounter for EVENT_WC_COUNTER_COMBINATION dispatch.

**AC#66: SourceEntrySystem references IUpVariables**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IUpVariables")`
- **Expected**: Pattern found
- **Rationale**: ACs #18, #19, #45 verify IUpVariables interface exists but no AC verified SourceEntrySystem consumption. UP array is accessed at SOURCE.ERB:342-351 inside SOURCE_CHECK logic (SourceEntrySystem scope). Mirrors the pattern of AC#46 (ITouchStateManager injection), AC#71 (existing external interfaces), AC#63 (facade interfaces).

**AC#67: ComCaller registers handlers for all 16 TFLAG:50 values**
- **Test**: `Grep(path="Era.Core/Counter/Source/ComCaller.cs", pattern="\\.Add\\(|\\[\\d")` count >= 16
- **Expected**: At least 16 handler registration entries in ComCaller implementation
- **Rationale**: AC#40 verifies test code asserts handler count == 16, but no AC verified the implementation itself. A test could hardcode an assertion while the dictionary is incomplete. AC#74 verifies ComCaller.cs contains at least 16 handler registrations, complementing AC#40's test-side check.

**AC#68: External facade stub interfaces do not reference orchestration interfaces**
- **Test**: `Grep(path="Era.Core/Counter/ITrainingCheckService.cs", pattern="...")` + `Grep(path="Era.Core/Counter/IKojoMessageService.cs", pattern="...")` + `Grep(path="Era.Core/Counter/INtrUtilityService.cs", pattern="...")` + `Grep(path="Era.Core/Counter/IWcSexHaraService.cs", pattern="...")` where pattern = `ICounterSystem|IWcCounterSystem|ISourceCalculator|ICounterSourceHandler|IWcCounterSourceHandler|ICombinationCounter|IWcCombinationCounter`
- **Expected**: Pattern NOT found in any of the 4 facade stub interface files
- **Rationale**: AC#57 checks interface files in `Era.Core/Counter/Source/` (ITouchStateManager.cs, ISourceSystem.cs, IKnickersSystem.cs) but does not cover the 4 external facade stub interfaces created by Task 16 in `Era.Core/Counter/`. These facade interfaces should only declare stub methods for external dependencies; referencing orchestration or sibling-dispatch interfaces would violate interface segregation. Complements AC#57 (Source/ interface files) with Counter/ facade file coverage.

**AC#69: SourceEntrySystem specifically references ICounterSourceHandler**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ICounterSourceHandler")`
- **Expected**: Pattern found
- **Rationale**: The former sibling-dispatch OR-pattern AC (superseded) passed if ANY of the 4 stubs matched. ICounterSourceHandler could be omitted while the OR still passed. AC#76 closes this gap by requiring SourceEntrySystem.cs to specifically reference ICounterSourceHandler for EVENT_COUNTER_SOURCE dispatch (SOURCE.ERB:38, F803 stub). Mirrors the pattern AC#72 established for IWcCombinationCounter.

**AC#70: SourceEntrySystem specifically references ICombinationCounter**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ICombinationCounter")`
- **Expected**: Pattern found
- **Rationale**: Same OR-pattern gap as AC#72/AC#76. ICombinationCounter could be omitted from SourceEntrySystem while the former OR still passed. AC#77 requires SourceEntrySystem.cs to specifically reference ICombinationCounter for EVENT_COUNTER_COMBINATION dispatch (SOURCE.ERB:40, F802 stub).

**AC#71: SourceEntrySystem specifically references IWcCounterSourceHandler**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IWcCounterSourceHandler")`
- **Expected**: Pattern found
- **Rationale**: Completes the individual stub injection verification set. AC#72 (IWcCombinationCounter), AC#76 (ICounterSourceHandler), AC#77 (ICombinationCounter), AC#78 (IWcCounterSourceHandler) — all 4 sibling-dispatch stubs now have dedicated SourceEntrySystem injection checks, eliminating the former OR-pattern gap entirely. IWcCounterSourceHandler is the F805 stub for EVENT_WC_COUNTER_SOURCE dispatch (SOURCE.ERB:26).

**AC#72: Sibling-dispatch stub interfaces do not reference orchestration interfaces**
- **Test**: `Grep(path="Era.Core/Counter/ICounterSourceHandler.cs", pattern="...")` + `Grep(path="Era.Core/Counter/IWcCounterSourceHandler.cs", pattern="...")` + `Grep(path="Era.Core/Counter/ICombinationCounter.cs", pattern="...")` + `Grep(path="Era.Core/Counter/IWcCombinationCounter.cs", pattern="...")` where pattern = `ICounterSystem|IWcCounterSystem|ISourceCalculator`
- **Expected**: Pattern NOT found in any of the 4 sibling-dispatch stub interface files
- **Rationale**: AC#75 covers the 4 external facade stubs (ITrainingCheckService, etc.) and AC#57 covers the 3 interface files in Counter/Source/ (ITouchStateManager, ISourceSystem, IKnickersSystem). The 4 sibling-dispatch stub interfaces in Era.Core/Counter/ (ICounterSourceHandler, IWcCounterSourceHandler, ICombinationCounter, IWcCombinationCounter) had no orchestration leak check. These are single-method stub interfaces that should declare only their handler method; referencing orchestration interfaces would indicate improper coupling. Complements AC#57 (Source/ interfaces) and AC#75 (facade stubs) to close the Counter/ sibling-dispatch stub gap.

**AC#73: SourceEntrySystem calls ITouchStateManager.TouchSuccession**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="\\.TouchSuccession\\(")`
- **Expected**: Pattern found (verifying actual method invocation, not just interface reference)
- **Rationale**: AC#39 verifies SourceEntrySystem references ITouchStateManager (interface presence), but does not verify TouchSuccession is actually called. SOURCE.ERB:18 calls TOUCH_SUCCESSION — this maps to SourceEntrySystem scope. Mirrors the AC#6→AC#50 pattern (interface reference → method call verification). AC#68 only verifies the method exists on the interface; AC#80 verifies the orchestrator invokes it.

**AC#74: SourceEntrySystem calls ComCaller dispatch method**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="_comCaller\\.|_comDispatcher\\.")`
- **Expected**: Pattern found (verifying actual method invocation on ComCaller/IComDispatcher, not just field declaration)
- **Rationale**: AC#55 verifies SourceEntrySystem references ComCaller/IComDispatcher/_comCaller (pattern matches field declaration or import without invocation). Philosophy claims SourceEntrySystem "owns ... COM calling". Mirrors the AC#6→AC#50 pattern (interface reference → method call verification). Without AC#81, an implementation could declare `_comCaller` as a field while never invoking any dispatch method.


**AC#75: TouchStateManager references IConsoleOutput for ShowTouch display**
- **Test**: `Grep(path="Era.Core/Counter/Source/TouchStateManager.cs", pattern="IConsoleOutput")`
- **Expected**: Pattern found
- **Rationale**: SHOW_TOUCH (SOURCE_POSE.ERB:4-70) is a display function showing current touch state. TouchStateManager implements ShowTouch (AC#69) which requires IConsoleOutput for text output.

**AC#76: SourceEntrySystem references IRelationVariables for RELATION read**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IRelationVariables")`
- **Expected**: Pattern found
- **Rationale**: SOURCE.ERB:491 reads RELATION:TARGET:(NO:PLAYER) which requires IRelationVariables (F812's existing GetRelation). C8 constraint notes RELATION read handled by F812's GetRelation. Without this AC, the RELATION read at line 491 could be omitted.

**AC#77: ITouchStateManager.MasterPose returns int (not CharacterId)**
- **Test**: `Grep(path="Era.Core/Counter/Source/ITouchStateManager.cs", pattern="int MasterPose\\(")`
- **Expected**: Pattern found (verifying return type is `int`, not `CharacterId`)
- **Rationale**: Mandatory Handoffs "MasterPose consolidation" specifies F804 adapter must convert int→CharacterId. If F811 implementation uses CharacterId return type, the adapter specification breaks. Interface Snippets (line ~919) define `int MasterPose(...)`. AC#10 verifies MasterPose exists but does not enforce return type.

**AC#78: SourceEntrySystem references IKnickersSystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IKnickersSystem")`
- **Expected**: Pattern found
- **Rationale**: AC#32 verifies IKnickersSystem interface exists, AC#31 verifies no direct CLOTHES_Change_Knickers string. But no AC verified SourceEntrySystem actually injects IKnickersSystem. Technical Design says "SourceEntrySystem calls `_knickers.ChangeKnickers(character)`" (C9, Phase 22 stub). Every other interface injected into SourceEntrySystem has a positive injection AC (AC#46, #63, #71, #73, #82, #84). IKnickersSystem alone lacked this check.

**AC#79: IComHandler does not reference orchestration interfaces**
- **Test**: `Grep(path="Era.Core/Counter/IComHandler.cs", pattern="ICounterSystem|IWcCounterSystem|ISourceCalculator")`
- **Expected**: Pattern NOT found

**AC#80: SourceDisplayHandler.cs exists in Counter/Source**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceDisplayHandler.cs", pattern="class SourceDisplayHandler")`
- **Expected**: Pattern found

**AC#81: SourceEntrySystem delegates to SourceDisplayHandler**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="SourceDisplayHandler|ISourceDisplayHandler|_displayHandler")`
- **Expected**: Pattern found

**AC#82: SourceDisplayHandler references IConsoleOutput**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceDisplayHandler.cs", pattern="IConsoleOutput")`
- **Expected**: Pattern found

**AC#83: SourceEntrySystem contains COM call entry condition logic**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="CanCom|CAN_COM|canCom|ComCondition|comCallCondition")`
- **Expected**: Pattern found

**AC#84: SourceEntrySystemTests has >= 5 Assert calls**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/SourceEntrySystemTests.cs", pattern="Assert\\.\\w+\\(")` count >= 5
- **Expected**: At least 5 Assert calls (matches AC#95 pattern — gte 5 for 510-line scope; SOURCE.ERB is 1437 lines)

**AC#85: TouchStateManagerTests has >= 2 Assert calls**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/TouchStateManagerTests.cs", pattern="Assert\\.\\w+\\(")` count >= 2
- **Expected**: At least 2 Assert calls

**AC#86: ComCallerTests has >= 2 Assert calls**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/ComCallerTests.cs", pattern="Assert\\.\\w+\\(")` count >= 2
- **Expected**: At least 2 Assert calls

**AC#87: ShootingSystemTests has >= 5 Assert calls**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/ShootingSystemTests.cs", pattern="Assert\\.\\w+\\(")` count >= 5
- **Expected**: At least 5 Assert calls

**AC#88: SourceDisplayHandler implements ShowSourceParamCng method**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceDisplayHandler.cs", pattern="ShowSourceParamCng\\(")`
- **Expected**: Pattern found
- **Rationale**: SourceDisplayHandler owns SHOW_SOURCE_PALAMCNG display logic (~88 lines from SOURCE.ERB:447-534). AC#96 verifies the ShowSourceParamCng method exists as an individual check, replacing the OR-pattern `gte >= 3` which could false-pass if any one of the three methods was present. Splitting into AC#96/99/100 ensures all three methods are individually verified.

**AC#89: SourceDisplayHandlerTests has >= 5 Assert calls**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/SourceDisplayHandlerTests.cs", pattern="Assert\\.\\w+\\(")`
- **Expected**: count >= 5 (at least 5 assertion calls in SourceDisplayHandlerTests)
- **Rationale**: Completes the per-subsystem assertion density set: AC#92 (SourceEntrySystemTests), AC#93 (TouchStateManagerTests), AC#94 (ComCallerTests), AC#95 (ShootingSystemTests), AC#97 (SourceDisplayHandlerTests). SourceDisplayHandler owns SHOW_SOURCE_PALAMCNG (~88 lines) and CHK_SEMEN_STAY (~370 lines) display output — ~458 lines total, the largest display class in Counter/Source/. A minimum of 5 assertions matches the AC#95 pattern (ShootingSystemTests >= 5 for 510-line scope) and reflects the 458-line scope more accurately than a bare minimum of 2. Display tests combine standard Assert.{method}() calls (for output validation, null checks, state verification) with IConsoleOutput mock verification. AC#97's Assert.\w+\( matcher captures xUnit assertion calls; Moq Verify() calls are supplementary validation. The >= 5 threshold is achievable because SourceDisplayHandler methods return values and interact with state beyond pure display output (e.g., CHK_SEMEN_STAY CFLAG mutations). AC#104 (numeric assertions >= 5) is expected to be satisfied primarily by non-display test classes.

**AC#90: SourceEntrySystem calls ShootingSystem dispatch method**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="_shootingSystem\\.\\w+\\(|EventShoot\\(")`
- **Expected**: Pattern found (SourceEntrySystem dispatches to ShootingSystem via method call with parenthesis)
- **Rationale**: Philosophy claims SourceEntrySystem "owns ... shooting/ejaculation logic" and "owns the main entry point (@SOURCE_CHECK)". AC#29 verifies ShootingSystem class exists; AC#49 verifies ShootingSystem doesn't leak orchestration back. But no AC verifies SourceEntrySystem actually calls/dispatches to ShootingSystem. This parallels the AC#50 (ICounterSystem.SelectAction), AC#80 (.TouchSuccession(), AC#81 (_comCaller.) dispatch verification pattern. Matcher requires parenthesis to confirm actual method invocation (not just type reference or comment).

**AC#91: SourceDisplayHandler implements ChkSemenStay method**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceDisplayHandler.cs", pattern="ChkSemenStay\\(")`
- **Expected**: Pattern found
- **Rationale**: SourceDisplayHandler owns CHK_SEMEN_STAY display logic (~370 lines, 10 variants). AC#99 individually verifies ChkSemenStay method exists, preventing false-pass that could occur if AC#96's OR-pattern matched only ShowSourceParamCng. Splitting from original AC#96 ensures all three display method groups are independently verified. Note: ChkSemenStay on SourceDisplayHandler is the display-only dispatcher; CFLAG mutation logic stays in SourceEntrySystem per Key Decisions (C16).

**AC#92: SourceDisplayHandler implements ShowSource method**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceDisplayHandler.cs", pattern="ShowSource\\(")`
- **Expected**: Pattern found
- **Rationale**: SourceDisplayHandler owns the ShowSource display method. AC#100 individually verifies ShowSource method exists as the third required display method group, completing the AC#96/99/100 set. Without this individual check, the ShowSource method could be omitted while AC#96's original OR-pattern still passed.

**AC#93: CHK_SEMEN_STAY test verifies CFLAG state mutation**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/", pattern="SemenStay.*Cflag|Cflag.*SemenStay|CFLAG.*SemenStay")`
- **Expected**: At least 3 matches
- **Rationale**: CHK_SEMEN_STAY contains CFLAG state mutations at SOURCE.ERB:982-984 (CFLAG:奴隷:精液残留中 decrement/clear). Risks table states "test branching logic separately from text output". AC#42 verifies variant names exist in tests; AC#101 verifies that CFLAG state mutation is assertion-covered in SemenStay context, not just text output. All 3 pattern branches require SemenStay co-occurrence with Cflag/CFLAG, preventing false-passes from unrelated CFLAG tests. Raised from 1 to 3 — 10 CHK_SEMEN_STAY variants with CFLAG mutations require at minimum 3 distinct CFLAG assertion patterns.

**AC#94: CAN_COM entry condition unit-tested**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/SourceEntrySystemTests.cs", pattern="CanCom|CAN_COM|ComCondition|comCallCondition")`
- **Expected**: Pattern found
- **Rationale**: Philosophy claims SourceEntrySystem "owns ... COM calling" including CAN_COM entry condition logic (AC#91 verifies condition in source). AC#102 verifies the behavioral gate is unit-tested, following AC#44 precedent (WC routing test verifying the other major branch decision). Without AC#102, the CAN_COM condition could exist in code (AC#91) while never being tested. The same gap AC#44 closed for WC routing exists for COM entry condition — AC#102 closes it.

**AC#95: Orchestration interfaces not leaked to SourceDisplayHandler**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceDisplayHandler.cs", pattern="ICounterSystem|IWcCounterSystem|ISourceCalculator")`
- **Expected**: Pattern NOT found (SourceDisplayHandler is a display output class; orchestration interfaces belong only in SourceEntrySystem)
- **Rationale**: SourceDisplayHandler was added in iteration 4 (Task 17) after the orchestration boundary enforcement set (AC#41 ComCaller, AC#42 ShootingSystem, AC#44 TouchStateManager) was established. This AC extends the enforcement set to cover SourceDisplayHandler, ensuring Philosophy's "SSOT for counter event orchestration" claim is maintained across all non-orchestrator implementation classes in Counter/Source/.

**AC#96: Equivalence tests contain concrete numeric expected values**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/", pattern="Assert\\.Equal\\(-?\\d")` count >= 5
- **Expected**: At least 5 Assert.Equal calls with numeric expected values across test files
- **Rationale**: Philosophy requires "equivalence-tested against legacy behavior". AC#39 counts generic Assert calls (>= 10) and AC#92-95 count per-class minimums, but these could be satisfied with trivial assertions (Assert.NotNull, Assert.True). AC#104 requires at least 5 Assert.Equal calls with concrete numeric expected values (derived from ERB computation), proving tests actually compare C# results against expected ERB-derived values. This is the gold standard for equivalence verification.

**AC#97: SourceDisplayHandler behavioral equivalence test names**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/SourceDisplayHandlerTests.cs", pattern="ShowSourceParamCng|ChkSemenStay|ShowSource")`
- **Expected**: Pattern found (test file references SourceDisplayHandler behavioral method names)
- **Rationale**: AC#41 covers 4 subsystems (SourceEntrySystem, TouchStateManager, ShootingSystem, ComCaller) but excludes SourceDisplayHandler (~458 lines). AC#97 only counts assertion density. AC#105 ensures SourceDisplayHandler's behavioral methods (ShowSourceParamCng, ChkSemenStay, ShowSource) are named in tests, confirming behavioral-level equivalence testing for the 5th subsystem. Philosophy claims "equivalence-tested against legacy behavior" for "all ERB counter logic."

**AC#98: SourceEntrySystem specifically references IKojoMessageService**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IKojoMessageService")`
- **Expected**: Pattern found
- **Rationale**: Closes AC#63 OR-pattern gap. IKojoMessageService covers 8 KOJO_MESSAGE variants from EVENT_KOJO.ERB called by SOURCE.ERB. Individual check mirrors AC#72/76/77/78 pattern — AC#63 uses OR pattern (ITrainingCheckService|IKojoMessageService|INtrUtilityService|IWcSexHaraService) which passes if ANY one facade interface appears, allowing IKojoMessageService to be omitted from SourceEntrySystem while AC#63 still passes via ITrainingCheckService alone.

**AC#99: SourceEntrySystem specifically references INtrUtilityService**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="INtrUtilityService")`
- **Expected**: Pattern found
- **Rationale**: Closes AC#63 OR-pattern gap. INtrUtilityService covers NTR_MARK_5/NTR_ADD_SURRENDER from NTR_UTIL.ERB (SOURCE.ERB:211, SOURCE.ERB:390, SOURCE.ERB:901, SOURCE.ERB:963). Individual check mirrors AC#72/76/77/78 pattern.

**AC#100: SourceEntrySystem specifically references IWcSexHaraService**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IWcSexHaraService")`
- **Expected**: Pattern found
- **Rationale**: Closes AC#63 OR-pattern gap. IWcSexHaraService covers WC_SexHara/WC_SexHara_SOURCE/WC_SexHara_MESSAGEbase functions from WC_SexHara.ERB (SOURCE.ERB:30, SOURCE.ERB:32, SOURCE.ERB:388). Individual check mirrors AC#72/76/77/78 pattern.

**AC#101: SourceEntrySystem specifically references IVirginityManager**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IVirginityManager")`
- **Expected**: Pattern found
- **Rationale**: Closes AC#71 OR-pattern gap. AC#71 uses OR pattern (IOrgasmProcessor|IVirginityManager|IFavorCalculator|IMarkSystem) which passes if ANY one external interface is present. IVirginityManager covers LOST_VIRGIN variants at SOURCE.ERB:131-133. Individual check mirrors AC#72/76/77/78 pattern — each of the 4 pre-existing external interfaces must be individually verified in SourceEntrySystem.

**AC#102: SourceEntrySystem specifically references IFavorCalculator**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IFavorCalculator")`
- **Expected**: Pattern found
- **Rationale**: Closes AC#71 OR-pattern gap. IFavorCalculator covers FAVOR_CALC at SOURCE.ERB:544. Individual check mirrors AC#72/76/77/78 pattern.

**AC#103: SourceEntrySystem specifically references IMarkSystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IMarkSystem")`
- **Expected**: Pattern found
- **Rationale**: Closes AC#71 OR-pattern gap. IMarkSystem covers MARK_GOT_CHECK at SOURCE.ERB:471. Individual check mirrors AC#72/76/77/78 pattern.

**AC#104: SourceEntrySystem specifically references ITrainingCheckService**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ITrainingCheckService")`
- **Expected**: Pattern found

**AC#105: SourceEntrySystem specifically references IOrgasmProcessor**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IOrgasmProcessor")`
- **Expected**: Pattern found

**AC#106: ShowTouch dispatch call verified in SourceEntrySystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="\\.ShowTouch\\(")`
- **Expected**: Pattern found
- **Rationale**: Philosophy claims SourceEntrySystem "manages touch state (SHOW_TOUCH)". File-scoped grep verifies the actual dispatch call from SourceEntrySystem to ITouchStateManager.ShowTouch, not just the implementation in TouchStateManager. Dot-prefix targets call-site, not declaration.

**AC#107: SourceDisplayHandler CFLAG boundary**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceDisplayHandler.cs", pattern="SetCflag|\\.Cflag\\s*=")`
- **Expected**: NOT found (not_matches)
- **Rationale**: Key Decisions documents CHK_SEMEN_STAY CFLAG mutations (SOURCE.ERB:982-984) moved to SourceEntrySystem/ShootingSystem. SourceDisplayHandler must remain display-only. Enforces design boundary per AC#103 orchestration exclusion pattern.

**AC#108: ISourceCalculator method diversity**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="_sourceCalculator\\.Source")`
- **Expected**: Count >= 20 distinct method name prefixes
- **Rationale**: Complements AC#43 (total count >= 35) with a diversity check. AC#43 alone could be satisfied by repeated calls to a single method. This AC verifies at least 20 distinct `_sourceCalculator.Source*` method names appear (out of 35 in ISourceCalculator), ensuring the 35 SOURCE_* functions from SOURCE.ERB:127-335 are not collapsed into fewer methods.

**AC#109: ShootingSystem CFLAG mutation positive check**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="SetCflag|SemenStay|精液残留")`
- **Expected**: Pattern found (matches)
- **Rationale**: Positive-side enforcement of CHK_SEMEN_STAY design boundary. Key Decisions documents CFLAG mutations (SOURCE.ERB:982-984: CFLAG:奴隷:精液残留中 decrement/clear) moved to SourceEntrySystem/ShootingSystem per dual-ownership. AC#115 verifies SourceDisplayHandler is clean (negative); this AC verifies the mutation logic exists in the correct subsystem (positive). Directory-level grep allows either SourceEntrySystem or ShootingSystem per Key Decision.

**AC#110: SourceDisplayHandlerTests contains Assert.Equal numeric equivalence assertions**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/SourceDisplayHandlerTests.cs", pattern="Assert\\.Equal\\(")`
- **Expected**: Count >= 3
- **Rationale**: AC#42 verifies CHK_SEMEN_STAY variant naming patterns (naming convention check), AC#97 verifies broad assertion density (Assert.\w+\( >= 5, includes Assert.True/NotNull). Neither guarantees actual equality-based equivalence assertions. This AC ensures >= 3 Assert.Equal calls exist for numeric/string output verification, enforcing Philosophy's "equivalence-tested against legacy behavior" claim for display output.

**AC#111: ICounterSourceHandler declares HandleCounterSource method**
- **Test**: `Grep(path="Era.Core/Counter/ICounterSourceHandler.cs", pattern="HandleCounterSource\\(")`
- **Expected**: Pattern found
- **Rationale**: F803's existing ICounterSourceHandler defines HandleCounterSource(CharacterId target, int arg1). AC#119 verifies the method signature is present for sibling feature consumption.

**AC#112: IWcCounterSourceHandler declares HandleWcCounterSource method**
- **Test**: `Grep(path="Era.Core/Counter/IWcCounterSourceHandler.cs", pattern="HandleWcCounterSource\\(")`
- **Expected**: Pattern found
- **Rationale**: Interface Snippets define HandleWcCounterSource(int targetCharIndex, int loopChr) for F805 consumption. Mirrors AC#119 pattern.

**AC#113: ICombinationCounter declares AccumulateCombinations method**
- **Test**: `Grep(path="Era.Core/Counter/ICombinationCounter.cs", pattern="AccumulateCombinations\\(")`
- **Expected**: Pattern found
- **Rationale**: Method name aligns with F802's existing CounterCombination.AccumulateCombinations() → int[]. F811 creates the interface; F802's concrete class already has the implementation.

**AC#114: IWcCombinationCounter declares AccumulateCombinations method**
- **Test**: `Grep(path="Era.Core/Counter/IWcCombinationCounter.cs", pattern="AccumulateCombinations\\(")`
- **Expected**: Pattern found
- **Rationale**: Forward-compatible method for WC combination counter processing (F802 consumption). No Interface Snippet shown but mirrors ICombinationCounter pattern.

**AC#115: TouchStateManagerTests contains TouchSuccession behavioral test**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/TouchStateManagerTests.cs", pattern="TouchSuccession")`
- **Expected**: Pattern found
- **Rationale**: TOUCH_SUCCESSION (SOURCE_POSE.ERB:71-285, 215 lines) is the largest single function migrated to TouchStateManager. AC#41 test name patterns can pass via other methods (SourceCheck/TouchSet/MasterPose/EventShoot) without any TouchSuccession test. AC#68 verifies method declaration, AC#80 verifies call-site — neither ensures behavioral test exists. Follows AC#102 (CAN_COM condition test) and AC#44 (WC routing test) patterns for individually mandated major function tests.

**AC#116: SourceEntrySystem implements ISourceSystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="(: |, )ISourceSystem")`
- **Expected**: Pattern found (matches)
- **Rationale**: AC#30 verifies ChastityBeltCheck( exists in Counter/Source/ directory, but could false-positive on the ISourceSystem.cs interface declaration. This AC verifies SourceEntrySystem.cs concretely implements ISourceSystem, ensuring the orchestrator hub actually exposes ChastityBeltCheck to external callers (EVENTCOMEND.ERB consumption).

**AC#117: SourceEntrySystem DI field count within cap**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="private readonly I")` count <= 25
- **Expected**: At most 25 private readonly interface fields (DI injected dependencies)
- **Rationale**: C16 constraint — orchestrator hub pattern justifies 20+ deps (1:1 mapping to ERB CALL targets) but explicit ceiling prevents unbounded growth. Threshold 25 provides margin above current ~20 deps.

**AC#118: SourceEntrySystem constructor injects ISourceCalculator**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ISourceCalculator")`
- **Expected**: Pattern found (at least constructor parameter declaration)
- **Rationale**: Naming-agnostic injection verification. AC#43 and AC#116 depend on `_sourceCalculator` field naming convention; this AC verifies ISourceCalculator injection exists regardless of field name. Complements AC#43 (call count) and AC#116 (method diversity) by anchoring to the interface type.

**AC#119: ISourceCalculator declares all 35 SOURCE_* methods**
- **Test**: `Grep(path="Era.Core/Counter/ISourceCalculator.cs", pattern="void Source")`
- **Expected**: Count >= 35
- **Rationale**: Verifies F812's ISourceCalculator interface maintains all 35 SOURCE_* method declarations. Complements AC#43 (call-site count in SourceEntrySystem) and AC#116 (method name diversity). Prevents regression where interface methods are removed but call count is maintained through other patterns.

**AC#120: SCOM dispatch path unit-tested**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/ComCallerTests.cs", pattern="Scom|ScomDispatch|SCOM|scom")`
- **Expected**: Pattern found
- **Rationale**: AC#64 verifies SCOM dispatch exists in ComCaller.cs source code, but no AC verified SCOM dispatch is behaviorally tested. Parallels AC#44 (WC routing test) and AC#102 (CAN_COM test) for the COM dispatch paths. SOURCE_CALLCOM.ERB:74-79 SCOM branch is a separate dispatch mechanism from COM_ABLE.

**AC#121: SelectCom dispatch path unit-tested**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/ComCallerTests.cs", pattern="SelectCom|selectCom|SELECTCOM|ComSelect")`
- **Expected**: Pattern found
- **Rationale**: AC#65 verifies CAN_COM/COM{SELECTCOM} dispatch exists in ComCaller.cs source code, but no AC verified SelectCom dispatch is behaviorally tested. SOURCE_CALLCOM.ERB:88-93 COM{SELECTCOM} branch is a separate dispatch mechanism from COM_ABLE. Parallels AC#102 (CAN_COM entry condition test-existence).

**AC#122: ChkWcEquip method exists in SourceEntrySystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ChkWcEquip|CheckWcEquip")`
- **Expected**: Pattern found
- **Rationale**: SOURCE.ERB:551-857 CHK_WC_EQUIP (~306 lines) handles WC equipment effects (nipple caps, vibrators, TIME_PROGRESS calls). Called from @SOURCE_CHECK at line 84. Contains CFLAG mutations — must be in SourceEntrySystem, not SourceDisplayHandler.

**AC#123: ChkInsRotorV method exists in SourceEntrySystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ChkInsRotorV|CheckInsertRotorV|RotorV")`
- **Expected**: Pattern found
- **Rationale**: SOURCE.ERB:858-913 CHK_INS_ROTOR_V (~55 lines) handles Rotor V insertion check. Called from @SOURCE_CHECK at line 85. Uses NTR_ADD_SURRENDER (INtrUtilityService), mutates SOURCE/CFLAG/EXP arrays.

**AC#124: ChkInsRotorA method exists in SourceEntrySystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ChkInsRotorA|CheckInsertRotorA|RotorA")`
- **Expected**: Pattern found
- **Rationale**: SOURCE.ERB:917-975 CHK_INS_ROTOR_A (~58 lines) handles Rotor A insertion check. Called from @SOURCE_CHECK at line 86. Structure parallels CHK_INS_ROTOR_V.

**AC#125: ChkSemenBath method exists in SourceEntrySystem**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ChkSemenBath|CheckSemenBath|SemenBath")`
- **Expected**: Pattern found
- **Rationale**: SOURCE.ERB:1349-1357 CHK_SEMEN_BATH (~8 lines) handles semen bath duration check. Mutates CFLAG (汚れ継続時間, 汚れ発覚) and uses TIME_PROGRESS.

**AC#126: WC routing test references 肉便器 predicate**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/SourceEntrySystemTests.cs", pattern="肉便器|WcMode|IsWc")`
- **Expected**: Pattern found
- **Rationale**: AC#58 verifies the WC predicate exists in source code; AC#44 verifies a WC routing test exists. This AC bridges the gap — the WC routing test must reference the actual predicate condition (肉便器/WcMode/IsWc), not just test WC routing abstractly. Ensures behavioral equivalence with SOURCE.ERB's 肉便器 branching.

**AC#127: SourceEntrySystem IShrinkageSystem injection**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="IShrinkageSystem|_shrinkageSystem")`
- **Expected**: Pattern found
- **Rationale**: IShrinkageSystem (F803) handles 締り具合変動 (tightness variation) logic. SOURCE.ERB calls shrinkage-related functions that require this dependency. DEP-DRIFT resolved: IShrinkageSystem interface exists from F803; F811 consumes it via constructor injection.

**AC#128: SourceEntrySystem ICounterUtilities injection**
- **Test**: `Grep(path="Era.Core/Counter/Source/SourceEntrySystem.cs", pattern="ICounterUtilities|_counterUtilities")`
- **Expected**: Pattern found
- **Rationale**: ICounterUtilities (F803) provides CheckExpUp for EXP level-up processing. SOURCE.ERB calls EXP_UP logic that requires this dependency. DEP-DRIFT resolved: ICounterUtilities interface exists from F803; F811 consumes it via constructor injection.

**AC#129: ShootingSystem cross-character ejaculation test**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/ShootingSystemTests.cs", pattern="CrossCharacter|MultiCharacter|MultiTarget|MultipleTarget|cross.*ejacu")`
- **Expected**: Pattern found
- **Rationale**: Constraint C6 requires verification of cross-character ejaculation tracking (SOURCE_SHOOT.ERB handles multi-target ejaculation dispatch). AC#29/AC#49/AC#52 verify ShootingSystem structure but not the specific multi-target scenario. This AC ensures a test exists that explicitly exercises cross-character/multi-target ejaculation logic.

**AC#130: TouchStateManager implementation class exists**
- **Test**: `Grep(path="Era.Core/Counter/Source/TouchStateManager.cs", pattern="class TouchStateManager")`
- **Expected**: Pattern found
- **Rationale**: Mirrors AC#29 (ShootingSystem class check) and AC#88 (SourceDisplayHandler class check). AC#8-12 grep the directory and match interface method patterns in ITouchStateManager.cs, so they pass without the implementation class. AC#51 uses not_matches (trivially passes if file absent). This AC positively verifies the TouchStateManager implementation class exists in the correct file.

**AC#131: @SOURCE_CHECK sub-function equivalence test names in SourceEntrySystemTests**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/SourceEntrySystemTests.cs", pattern="ChkWcEquip|CheckWcEquip|WcEquip|RotorV|RotorA|SemenBath")`
- **Expected**: Count >= 4 (at least one match per sub-function: ChkWcEquip, ChkInsRotorV, ChkInsRotorA, ChkSemenBath)
- **Rationale**: AC#130-133 only verify method existence in SourceEntrySystem.cs (class-level). Philosophy claims "equivalence-tested against legacy behavior" but ~427 lines of SOURCE.ERB sub-functions (ChkWcEquip 306 lines, ChkInsRotorV 55 lines, ChkInsRotorA 58 lines, ChkSemenBath 8 lines) had no equivalence test ACs. Follows AC#41/AC#105 pattern for behavioral test name verification.

**AC#132: COM dispatch TFLAG:50→handler behavioral mapping test (parameterized)**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/ComCallerTests.cs", pattern="InlineData|Theory")`
- **Expected**: Count >= 3 (parameterized test attributes verifying TFLAG:50→IComHandler mapping)
- **Rationale**: AC#40 (handler count == 16) and AC#74 (registration count >= 16) only verify quantity, not correctness of TFLAG:50→handler mapping. Count checks pass even if all 16 entries map to the same handler. This AC verifies parameterized testing exists (InlineData/Theory attributes) to cover multiple TFLAG values, ensuring behavioral correctness of the dispatch dictionary.

**AC#133: ChkWcEquip equivalence test assertion density**
- **Test**: `Grep(path="Era.Core.Tests/Counter/Source/SourceEntrySystemTests.cs", pattern="WcEquip|CheckWcEquip|ChkWcEquip")`
- **Expected**: Count >= 3 (multiple assertions/tests referencing ChkWcEquip — prevents single-test false-pass)
- **Rationale**: AC#139 verifies test NAME presence for all 4 sub-functions but a single parameterized test could satisfy all 4 matches. ChkWcEquip (306 lines, SOURCE.ERB:551-857) is the largest sub-function with complex equipment-check branching. Analogous to AC#101 (CFLAG mutation assertion density >= 3 for CHK_SEMEN_STAY). AC#92 threshold raised from gte 2 to gte 5 to match AC#95 pattern (gte 5 for 510-line ShootingSystem scope; SourceEntrySystem covers 1437-line SOURCE.ERB).

**AC#134: IShrinkageSystem interface declaration exists**
- **Test**: `Grep(path="Era.Core/Counter/", pattern="interface IShrinkageSystem")`
- **Expected**: Pattern found
- **Rationale**: Defense-in-depth for F803-provided interface. AC#127 verifies SourceEntrySystem injects IShrinkageSystem but depends on interface existence. Guards against F803 interface restructuring regression.

**AC#135: ICounterUtilities interface declaration exists**
- **Test**: `Grep(path="Era.Core/Counter/", pattern="interface ICounterUtilities")`
- **Expected**: Pattern found
- **Rationale**: Defense-in-depth for F803-provided interface. AC#128 verifies SourceEntrySystem injects ICounterUtilities but depends on interface existence. Guards against F803 interface restructuring regression.

**AC#136: ShootingSystem injects ICharacterStringVariables**
- **Test**: `Grep(path="Era.Core/Counter/Source/ShootingSystem.cs", pattern="ICharacterStringVariables")`
- **Expected**: Pattern found
- **Rationale**: AC#60 verifies SetCharacterString usage but only checks the method name string. This AC enforces DI constructor injection of ICharacterStringVariables, consistent with other injection-verification ACs (AC#39, AC#64, AC#76). CSTR write at SOURCE_SHOOT.ERB:8-9 requires ICharacterStringVariables per C8 constraint.

**AC#137: ISourceSystem SSOT enforcement (single implementer)**
- **Test**: `Grep(path="Era.Core/Counter/Source/", pattern="(: |, )ISourceSystem")`
- **Expected**: Count <= 1 (only SourceEntrySystem implements ISourceSystem)
- **Rationale**: Philosophy claims "SOURCE Entry System is the SSOT for counter event orchestration." SSOT is absolute — no competing implementations allowed. AC#116 verifies SourceEntrySystem implements ISourceSystem but does not prevent a second class from also implementing it.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Correct predecessor declarations on F801 and F812 | AC#30, AC#31 |
| 2 | Stub-interface strategy for not-yet-[DONE] sibling F805; consume F802/F803 existing interfaces | AC#21, AC#22, AC#49, AC#65, AC#69, AC#70, AC#71, AC#112, AC#113, AC#114 |
| 3 | ITouchStateManager interface exposing TOUCH_SET/MASTER_POSE/TOUCH_SUCCESSION/SHOW_TOUCH | AC#8, AC#9, AC#10, AC#11, AC#12, AC#39, AC#61, AC#62, AC#73, AC#75, AC#77, AC#106 |
| 4 | Strategy/dictionary pattern for CALLFORM COM_ABLE{N} dynamic dispatch; SCOM/CAN_COM/COM dispatch paths; TRYCALLFORM fallback semantics; ComCaller ITouchStateManager for MASTER_POSE | AC#13, AC#27, AC#33, AC#57, AC#58, AC#59, AC#63, AC#67, AC#74, AC#83, AC#94, AC#120, AC#121, AC#132 |
| 5 | Interface additions for PREVCOM/UP/SETBIT/CSTR-write gaps (CSTR read resolved by F804, EXP_UP by inline calc, RELATION read handled by F812's existing GetRelation) | AC#14, AC#15, AC#16, AC#17, AC#18, AC#28, AC#29, AC#38, AC#46, AC#60, AC#66, AC#76, AC#136 |
| 6 | Equivalence tests verifying C# output matches legacy ERB behavior | AC#2, AC#4, AC#5, AC#32, AC#34, AC#35, AC#37, AC#40, AC#84, AC#85, AC#86, AC#87, AC#89, AC#93, AC#96, AC#97, AC#110, AC#115, AC#126, AC#129, AC#131, AC#133 |
| 7 | Zero technical debt and implementation correctness | AC#3, AC#45, AC#47, AC#109 |
| 8 | Phase 22 CLOTHES_Change_Knickers dependency deferred via IKnickersSystem stub | AC#25, AC#26, AC#78 |
| 9 | Source C# files exist in Counter/Source namespace | AC#1, AC#23, AC#80, AC#130 |
| 10 | Predecessor interface consumption and orchestration SSOT | AC#6, AC#7, AC#19, AC#20, AC#24, AC#36, AC#41, AC#42, AC#43, AC#44, AC#48, AC#50, AC#51, AC#64, AC#72, AC#79, AC#81, AC#82, AC#88, AC#90, AC#91, AC#92, AC#95, AC#101, AC#102, AC#103, AC#105, AC#107, AC#108, AC#111, AC#116, AC#117, AC#118, AC#119, AC#122, AC#123, AC#124, AC#125, AC#127, AC#128, AC#134, AC#135, AC#137 |
| 11 | External dependency facade stub interfaces for TRACHECK/KOJO_MESSAGE/NTR_UTIL/WC_SexHara function groups | AC#52, AC#53, AC#54, AC#55, AC#56, AC#68, AC#98, AC#99, AC#100, AC#104 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The migration follows the established F801 pattern: one C# class per ERB file, all wired via constructor injection (DI), namespace `Era.Core.Counter.Source`. The core design decisions are:

1. **SourceEntrySystem** (SOURCE.ERB): Top-level orchestrator. Dispatches to `ICounterSystem` (F801, existing) and new `IWcCounterSystem` stub for the WC branch. Calls `ISourceCalculator` (F812) for all 30+ SOURCE_* functions. Calls `ITouchStateManager` (owned here) for TOUCH_SUCCESSION.

2. **TouchStateManager + ITouchStateManager** (SOURCE_POSE.ERB): Owns TOUCH_SET, TOUCH_RESET_M, TOUCH_RESET_T, MASTER_POSE. F803/F805/F809 consume this interface. Defined in `Era.Core.Counter.Source` namespace.

3. **ComCaller** (SOURCE_CALLCOM.ERB): CALL_COM function. Uses `Dictionary<int, IComHandler>` keyed by TFLAG:50 value (1-16) to implement CALLFORM COM_ABLE{500+TFLAG:50} dynamic dispatch. Alternatively exposes `IComDispatcher` if handler list is open.

4. **ShootingSystem** (SOURCE_SHOOT.ERB): EVENT_SHOOT, EJAC_CHECK, SAMEN_SHOOT, SAMEN_DIRECTION. Uses `SetBit` utility method from a static `BitfieldUtility` class (Era.Core.Utilities namespace) for SETBIT operations.

5. **Interface gap resolution**: PREVCOM → `IEngineVariables.GetPrevCom()/SetPrevCom()`; RELATION → read-only at SOURCE.ERB:491 via F812's existing `IRelationVariables.GetRelation()` (no SetRelation needed in F811); UP → new `IUpVariables` interface; CSTR → read resolved by F804 `IVariableStore.GetCharacterString`; write requires `SetCharacterString` (SOURCE_SHOOT.ERB:340); EXP_UP → `ICounterUtilities.CheckExpUp` per DEP-DRIFT resolution (adopted from F803).

6. **Stub interfaces**: `ICounterSourceHandler`, `IWcCounterSourceHandler`, `ICombinationCounter` are stub interfaces with forward-compatible method signatures defined in `Era.Core.Counter` namespace, to be implemented by F803/F805/F802 respectively. (`IWcCounterSystem` already exists from F804; F811 consumes it, not creates it) Note: F803's ICounterSourceHandler has ISP violation (6 methods, 3 responsibilities) — tracked in F813 Deferred Obligations #3. F811 only consumes HandleCounterSource.

7. **CLOTHES_Change_Knickers**: Injected as `IKnickersSystem` stub (Phase 22 deferred). SourceEntrySystem calls `_knickers.ChangeKnickers(character)` — never the ERB string directly.

This approach satisfies all active ACs by relying on the same DI-constructor pattern used by F801 (ActionSelector), ensuring zero direct ERB calls remain in the C# layer.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create 8+ .cs files in Era.Core/Counter/Source/ (5 impl + 3 interface) |
| 2 | Create 5+ test files in Era.Core.Tests/Counter/Source/ |
| 3 | No TODO/FIXME/HACK in Era.Core/Counter/ migrated .cs files |
| 4 | dotnet build Era.Core.Tests/ succeeds with zero errors |
| 5 | dotnet test Era.Core.Tests/ all tests pass |
| 6 | SourceEntrySystem.cs references ICounterSystem (for EVENT_COUNTER dispatch) |
| 7 | SourceEntrySystem.cs references IWcCounterSystem (for EVENT_WC_COUNTER dispatch) |
| 8 | ITouchStateManager.cs defines public interface ITouchStateManager |
| 9 | ITouchStateManager or impl declares TouchSet( method |
| 10 | ITouchStateManager or impl declares MasterPose( method |
| 11 | ITouchStateManager or impl declares TouchResetM( method |
| 12 | ITouchStateManager or impl declares TouchResetT( method |
| 13 | ComCaller.cs contains Dictionary.*ComHandler or IComDispatcher pattern |
| 14 | IEngineVariables.cs adds GetPrevCom( method signature |
| 15 | IEngineVariables.cs adds SetPrevCom( method signature |
| 16 | Era.Core/Interfaces/ contains IUpVariables interface with int GetUp(int |
| 17 | Era.Core/Interfaces/ contains IUpVariables with void SetUp(int |
| 18 | Era.Core/ contains BitfieldUtility.SetBit(int, int) static method |
| 19 | SourceEntrySystem.cs calls _wcCounterSystem.SelectAction or IWcCounterSystem.SelectAction |
| 20 | Era.Core/Counter/ contains interface ICounterSourceHandler definition |
| 21 | Era.Core/Counter/ contains interface IWcCounterSourceHandler definition |
| 22 | Era.Core/Counter/ contains interface ICombinationCounter definition |
| 23 | Era.Core/Counter/Source/ contains class ShootingSystem or EjaculationSystem |
| 24 | Era.Core/Counter/Source/ contains ChastityBeltCheck( method |
| 25 | Era.Core/Counter/Source/ must NOT contain CLOTHES_Change_Knickers string |
| 26 | Era.Core/Counter/Source/ contains interface IKnickersSystem definition |
| 27 | Era.Core/Counter/ contains interface IComHandler definition |
| 28 | IEngineVariables.cs has >= 27 (Get|Set) method signatures preserved |
| 29 | IStringVariables.cs has >= 2 (Get|Set) method signatures preserved |
| 30 | feature-811.md Dependencies table contains Predecessor F801 [DONE] |
| 31 | feature-811.md Dependencies table contains Predecessor F812 entry |
| 32 | Era.Core.Tests/Counter/Source/ contains >= 10 Assert.\w+( calls total |
| 33 | Tests assert COM handler count equals 16 (Assert or .Count == 16) |
| 34 | Tests reference >= 4 ERB function names (SourceCheck/TouchSet/EventShoot etc.) |
| 35 | >= 10 InlineData/Theory parameterized entries referencing SemenStay variants |
| 36 | SourceEntrySystem.cs has >= 35 _sourceCalculator. method call sites |
| 37 | Tests contain WC vs non-WC routing test (WcBranch/NonWcBranch pattern) |
| 38 | Era.Core/Interfaces/ contains interface IUpVariables definition |
| 39 | SourceEntrySystem.cs references ITouchStateManager interface |
| 40 | Tests reference ExpUp/EXP_UP/exp.*Up pattern for dual-use calculation |
| 41 | ComCaller.cs must NOT reference ICounterSystem/IWcCounterSystem/ISourceCalculator |
| 42 | ShootingSystem.cs must NOT reference ICounterSystem/IWcCounterSystem/ISourceCalculator |
| 43 | SourceEntrySystem.cs calls _counterSystem.SelectAction or ICounterSystem.SelectAction |
| 44 | TouchStateManager.cs must NOT reference ICounterSystem/IWcCounterSystem/ISourceCalculator |
| 45 | ShootingSystem.cs calls BitfieldUtility.SetBit for SETBIT operations |
| 46 | ICharacterStringVariables.cs contains SetCharacterString( method |
| 47 | SourceEntrySystem.cs calls CheckExpUp/_counterUtilities.CheckExpUp for EXP_UP |
| 48 | SourceEntrySystem.cs references ComCaller/IComDispatcher/_comCaller field |
| 49 | Era.Core/Counter/ contains interface IWcCombinationCounter definition |
| 50 | ITouchStateManager.cs/ISourceSystem.cs/IKnickersSystem.cs must NOT reference orchestration interfaces |
| 51 | SourceEntrySystem.cs references 肉便器/WcMode/IsWc WC routing predicate |
| 52 | Era.Core/Counter/ contains interface ITrainingCheckService definition |
| 53 | Era.Core/Counter/ contains interface IKojoMessageService definition |
| 54 | Era.Core/Counter/ contains interface INtrUtilityService definition |
| 55 | Era.Core/Counter/ contains interface IWcSexHaraService definition |
| 56 | SourceEntrySystem.cs references all 4 facade interfaces (Training/Kojo/Ntr/WcSexHara) |
| 57 | ComCaller.cs contains Scom/SCOM/ScomDispatch pattern for SCOM dispatch |
| 58 | ComCaller.cs contains SelectCom/SELECTCOM/selectCom for COM dispatch path |
| 59 | ComCaller.cs uses TryGet/ContainsKey/TryInvoke for TRYCALLFORM no-op semantics |
| 60 | ShootingSystem.cs calls SetCharacterString for CSTR:ARG:11 write |
| 61 | ITouchStateManager or impl declares TouchSuccession( method |
| 62 | ITouchStateManager.cs declares ShowTouch( method |
| 63 | ComCaller.cs references ITouchStateManager or MasterPose for MASTER_POSE calls |
| 64 | SourceEntrySystem.cs references IOrgasmProcessor/IVirginityManager/IFavorCalculator/IMarkSystem |
| 65 | SourceEntrySystem.cs specifically references IWcCombinationCounter |
| 66 | SourceEntrySystem.cs specifically references IUpVariables |
| 67 | ComCaller.cs has >= 16 handler .Add( or [digit] registration entries |
| 68 | 4 facade stub files in Counter/ must NOT reference orchestration interfaces |
| 69 | SourceEntrySystem.cs specifically references ICounterSourceHandler |
| 70 | SourceEntrySystem.cs specifically references ICombinationCounter |
| 71 | SourceEntrySystem.cs specifically references IWcCounterSourceHandler |
| 72 | 4 sibling-dispatch stub files in Counter/ must NOT reference orchestration interfaces |
| 73 | SourceEntrySystem.cs calls .TouchSuccession( method on ITouchStateManager |
| 74 | SourceEntrySystem.cs calls _comCaller. or _comDispatcher. dispatch method |
| 75 | TouchStateManager.cs references IConsoleOutput for ShowTouch display output |
| 76 | SourceEntrySystem.cs references IRelationVariables for RELATION read |
| 77 | ITouchStateManager.cs declares int MasterPose( (return type must be int) |
| 78 | SourceEntrySystem.cs references IKnickersSystem for Phase 22 stub |
| 79 | IComHandler.cs must NOT reference ICounterSystem/IWcCounterSystem/ISourceCalculator |
| 80 | Era.Core/Counter/Source/SourceDisplayHandler.cs contains class SourceDisplayHandler |
| 81 | SourceEntrySystem.cs references SourceDisplayHandler/ISourceDisplayHandler/_displayHandler |
| 82 | SourceDisplayHandler.cs references IConsoleOutput for display output |
| 83 | SourceEntrySystem.cs contains CanCom/CAN_COM/ComCondition entry condition logic |
| 84 | SourceEntrySystemTests.cs has >= 5 Assert.\w+( calls |
| 85 | TouchStateManagerTests.cs has >= 2 Assert.\w+( calls |
| 86 | ComCallerTests.cs has >= 2 Assert.\w+( calls |
| 87 | ShootingSystemTests.cs has >= 5 Assert.\w+( calls |
| 88 | SourceDisplayHandler.cs declares ShowSourceParamCng( method |
| 89 | SourceDisplayHandlerTests.cs has >= 5 Assert.\w+( calls |
| 90 | SourceEntrySystem.cs calls _shootingSystem.\w+( or EventShoot( dispatch |
| 91 | SourceDisplayHandler.cs declares ChkSemenStay( method |
| 92 | SourceDisplayHandler.cs declares ShowSource( method |
| 93 | Tests contain >= 3 SemenStay.*Cflag/Cflag.*SemenStay CFLAG mutation assertions |
| 94 | SourceEntrySystemTests.cs references CanCom/CAN_COM/ComCondition entry condition test |
| 95 | SourceDisplayHandler.cs must NOT reference ICounterSystem/IWcCounterSystem/ISourceCalculator |
| 96 | Era.Core.Tests/Counter/Source/ has >= 5 Assert.Equal(-?digit numeric assertions |
| 97 | SourceDisplayHandlerTests.cs references ShowSourceParamCng/ChkSemenStay/ShowSource names |
| 98 | SourceEntrySystem.cs specifically references IKojoMessageService |
| 99 | SourceEntrySystem.cs specifically references INtrUtilityService |
| 100 | SourceEntrySystem.cs specifically references IWcSexHaraService |
| 101 | SourceEntrySystem.cs specifically references IVirginityManager |
| 102 | SourceEntrySystem.cs specifically references IFavorCalculator |
| 103 | SourceEntrySystem.cs specifically references IMarkSystem |
| 104 | SourceEntrySystem.cs specifically references ITrainingCheckService |
| 105 | SourceEntrySystem.cs specifically references IOrgasmProcessor |
| 106 | SourceEntrySystem.cs contains .ShowTouch( dispatch call |
| 107 | SourceDisplayHandler.cs must NOT contain SetCflag or .Cflag= (display-only) |
| 108 | SourceEntrySystem.cs has >= 20 _sourceCalculator.Source method name occurrences |
| 109 | Era.Core/Counter/Source/ contains SetCflag/SemenStay/精液残留 CFLAG mutation |
| 110 | SourceDisplayHandlerTests.cs has >= 3 Assert.Equal( numeric equivalence assertions |
| 111 | ICounterSourceHandler.cs declares HandleCounterSource( method |
| 112 | IWcCounterSourceHandler.cs declares HandleWcCounterSource( method |
| 113 | ICombinationCounter.cs declares AccumulateCombinations( method |
| 114 | IWcCombinationCounter.cs declares AccumulateCombinations( method |
| 115 | TouchStateManagerTests.cs contains TouchSuccession behavioral test reference |
| 116 | SourceEntrySystem implements ISourceSystem interface |
| 117 | SourceEntrySystem DI field count within cap |
| 118 | SourceEntrySystem ISourceCalculator injection (naming-agnostic) |
| 119 | ISourceCalculator 35 method declarations |
| 120 | SCOM dispatch path behaviorally tested in ComCallerTests |
| 121 | SelectCom dispatch path behaviorally tested in ComCallerTests |
| 122 | ChkWcEquip method exists in SourceEntrySystem |
| 123 | ChkInsRotorV method exists in SourceEntrySystem |
| 124 | ChkInsRotorA method exists in SourceEntrySystem |
| 125 | ChkSemenBath method exists in SourceEntrySystem |
| 126 | WC routing test exercises the correct 肉便器/WcMode/IsWc predicate condition |
| 127 | SourceEntrySystem IShrinkageSystem injection for 締り具合変動 calls |
| 128 | SourceEntrySystem ICounterUtilities injection for CheckExpUp calls |
| 129 | ShootingSystemTests covers cross-character ejaculation tracking scenario (C6 constraint) |
| 130 | TouchStateManager.cs contains `class TouchStateManager` implementation |
| 131 | @SOURCE_CHECK sub-function equivalence test names in SourceEntrySystemTests |
| 132 | COM dispatch TFLAG:50→handler behavioral mapping test (parameterized) |
| 133 | ChkWcEquip equivalence test assertion density (>= 3) |
| 134 | IShrinkageSystem interface declaration exists in Era.Core/Counter/ |
| 135 | ICounterUtilities interface declaration exists in Era.Core/Counter/ |
| 136 | ShootingSystem injects ICharacterStringVariables for CSTR writes |
| 137 | Only SourceEntrySystem implements ISourceSystem (SSOT enforcement) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| ~~Where to add RELATION/UP accessors~~ | ~~A: extend IVariableStore, B: new IRelationVariables, C: extend IEngineVariables~~ | ~~B+: extend existing IRelationVariables (F812) + new IUpVariables~~ | ~~F812 already created IRelationVariables with GetRelation; F811 adds SetRelation to extend the existing interface. UP goes into new IUpVariables per ISP.~~ SetRelation removed: SOURCE*.ERB never writes RELATION; F812's existing GetRelation covers read at SOURCE.ERB:491. UP still goes into new IUpVariables. |
| ~~Where to add CSTR accessors~~ | ~~A: new ICstrVariables, B: extend IStringVariables~~ | ~~B: extend IStringVariables~~ | ~~CSTR is a per-character string variable (CSTR:chr:slot). IStringVariables already handles SAVESTR. Adding CSTR as a second method pair keeps string variables co-located~~ |
| ~~Where to add EXP_UP accessors~~ | ~~A: extend IVariableStore, B: new IExpUpVariables~~ | ~~B: new IExpUpVariables~~ | ~~EXP_UP has dual boolean/value semantics unlike existing Result<int> IVariableStore pattern; new interface allows int return (boolean is non-zero int, value is also int) with clear dual-use semantics~~ |
| COM dispatch pattern | A: dictionary + IComHandler, B: delegate dictionary, C: switch statement | A: Dictionary<int, IComHandler> | Matches the constraint C5 requirement exactly; open/closed principle — new COM types added without modifying ComCaller; testable per handler |
| SETBIT utility location | A: static class in Era.Core/Utilities/, B: extension on int, C: inline math | A: static BitfieldUtility | Per pattern-consistency rule: static utility matches existing engine patterns; named method is self-documenting; avoids duplicate implementations across F811/F812 |
| IWcCounterSystem stub location | A: Era.Core/Counter/Source/, B: Era.Core/Counter/ | B: Era.Core/Counter/ | Stub interfaces for sibling features belong in the shared Counter namespace, not in the owned Source namespace. Follows ICounterOutputHandler precedent (F801) |
| PREVCOM accessor host | A: IEngineVariables, B: new ICommandVariables | A: IEngineVariables | PREVCOM is a scalar engine variable like SELECTCOM (already in IEngineVariables). Adding GetPrevCom/SetPrevCom to IEngineVariables maintains grouping consistency |
| SourceEntrySystem 20+ constructor dependencies | A: accept as-is (orchestrator hub), B: decompose into aggregate facades, C: extract sub-orchestrators | A: accept as-is | SOURCE.ERB is architecturally an orchestrator hub (Philosophy: "SSOT for counter event orchestration") that CALLs into 6+ sibling features. 20+ DI dependencies is the natural consequence of explicit dependency injection replacing implicit global state. Artificial intermediary facades would add indirection without reducing coupling. Each dependency maps 1:1 to an ERB CALL target. |
| @SOURCE_CHECK sub-functions in SourceEntrySystem | A: keep as private methods in SourceEntrySystem, B: extract to SourceEquipmentChecker helper class | A: keep as private methods | ChkWcEquip (SOURCE.ERB:551-857), ChkInsRotorV (858-913), ChkInsRotorA (917-975), ChkSemenBath (1349-1357) are @SOURCE_CHECK-local sub-functions that share SourceEntrySystem's DI dependencies and CFLAG state. **DI analysis**: ChkWcEquip alone references ~8 of SourceEntrySystem's ≤25 (C16 cap) constructor dependencies (IVariableStore, ICflagVariables, IComableChecker, IEquipVariables, IConsoleOutput, IShrinkageSystem, ICounterSourceHandler, ITouchStateManager). Extracting to SourceEquipmentChecker would forward 8+ dependencies — exceeding typical helper class coupling thresholds — while gaining no testability benefit (sub-functions are already tested via AC#131/AC#133 equivalence tests on SourceEntrySystemTests). These are private implementation details, not separate responsibilities — they are branching paths within the same orchestration flow. AC#122-133 verify their existence within SourceEntrySystem. |
| ISourceSystem external API scope | A: minimal (only externally-called functions), B: full orchestration surface | A: minimal | ISourceSystem exposes only ChastityBeltCheck (called by EVENTCOMEND.ERB:136). Scan of SOURCE*.ERB callers confirmed no other external entry points. AC#24/AC#116 verify this single-method interface. Future external callers would extend ISourceSystem, maintaining ISP. |
| IComHandler.Execute parameter design | A: ComContext record parameter, B: individual parameters, C: no parameters (global state) | A: ComContext | 16 TFLAG:50 handlers may require different context fields; ComContext record aggregates all fields (Target, ActionState, SelectCom, PrevCom). New fields added to record without interface change. Testable: mock ComContext in unit tests. ComCaller is sole creator within F811 scope, so positional construction is acceptable. If additional creators emerge, switch to named parameters. |
| CHK_SEMEN_STAY CFLAG state mutation boundary | A: separate (CFLAG mutation in SourceEntrySystem, display in SourceDisplayHandler), B: allow SourceDisplayHandler to own CFLAG writes | A: separate | SoC principle: SourceDisplayHandler is display-only (AC#95 enforces no orchestration interfaces). CFLAG writes at SOURCE.ERB:982-984 (精液残留中 decrement/clear) are game state mutations, not display. SourceEntrySystem/ShootingSystem calls CFLAG mutation method, then delegates display to SourceDisplayHandler. |
| CFLAG mutation method convention | A: SetCflag() method calls only, B: allow direct array mutation (Cflag[i]--, Cflag[i] -= 1) | A: SetCflag() only | AC#107 (not_matches on SourceDisplayHandler) and AC#109 (matches on SourceEntrySystem/ShootingSystem) both use `SetCflag` as the mutation detection token. Direct array mutations (Cflag[i]--, Cflag[i] -= 1) would evade these patterns. Mandating SetCflag() for all CFLAG writes makes AC#107/117 patterns complete by design. SourceDisplayHandler may READ Cflag values for display branching but MUST NOT call SetCflag. |
| ShowTouch placement in TouchStateManager | A: keep in TouchStateManager, B: extract to SourceDisplayHandler, C: new TouchDisplayHandler | A: keep in TouchStateManager | ShowTouch (SOURCE_POSE.ERB:4-70) iterates internal TOUCH_SET/MASTER_POSE state arrays for display. Extracting to SourceDisplayHandler would require exposing TouchStateManager's internal state. SoC exception: display is inherently coupled to touch state read, unlike SHOW_SOURCE_PALAMCNG which reads only SOURCE[]. |
| MasterPose dual-interface interim state | A: document coexistence, B: merge now, C: ignore | A: document coexistence | IComableUtilities.MasterPose (F803) and ITouchStateManager.MasterPose (F811) both exist. F813 [DRAFT] will consolidate. Interim: both interfaces are consumed independently; F803 callers use IComableUtilities, F811 callers use ITouchStateManager. No runtime conflict. |
| DI field naming convention | _lowerCamelCase for interface fields (e.g., ISourceCalculator → _sourceCalculator). AC#36/AC#108 depend on this convention. AC#118 provides naming-agnostic fallback. | C# DI convention + AC dependency documentation |

### Interfaces / Data Structures

**New interfaces in `Era.Core.Counter.Source`:**

```csharp
// ITouchStateManager.cs - owned by F811; consumed by F802/F803/F805/F809
namespace Era.Core.Counter.Source;

public interface ITouchStateManager
{
    // TOUCH_SET(ARG, ARG:1, ARG:2, ARG:3)
    // ARG: target body part (101-107), ARG:1: master body part (1-7),
    // ARG:2: character index, ARG:3: reset flag
    void TouchSet(int targetPart, int masterPart, int characterIndex, bool reset = false);

    // TOUCH_RESET_M(ARG) - reset master contact part across all targets
    void TouchResetM(int masterPart);

    // TOUCH_RESET_T(ARG) - reset all target body parts for one character
    void TouchResetT(int characterIndex);

    // MASTER_POSE(ARG, ARG:1, ARG:2) - returns character occupying master body part
    // Returns 0 if none found. ARG:2=true checks previous turn history.
    int MasterPose(int targetPart, int masterPart, bool prevTurn);

    // TOUCH_SUCCESSION(ARG) - manage touch state transition at SOURCE_CHECK cycle start
    // SOURCE_POSE.ERB:71-285 (215 lines). Called by SOURCE.ERB:18.
    void TouchSuccession(int targetCharIndex);

    // SHOW_TOUCH - display current touch state (SOURCE_POSE.ERB:4-70)
    // External caller: INFO.ERB:11
    void ShowTouch();
}
```

```csharp
// ISourceSystem.cs - exposed for external callers (EVENTCOMEND.ERB)
namespace Era.Core.Counter.Source;

public interface ISourceSystem
{
    // ChastityBelt_Check(SOURCE.ERB:546) - chastity belt check
    bool ChastityBeltCheck(CharacterId character);
}
```

**New stub interfaces in `Era.Core.Counter`:**

```csharp
// IWcCounterSystem — already exists from F804 (Era.Core/Counter/IWcCounterSystem.cs)
// Signature: CounterActionId? SelectAction(CharacterId offender, int actionOrder)
// F811 consumes this interface; does NOT create it.

// ICounterSourceHandler.cs - already exists from F803 (EVENT_COUNTER_SOURCE)
namespace Era.Core.Counter;
public interface ICounterSourceHandler
{
    void HandleCounterSource(CharacterId target, int arg1);
}

// IWcCounterSourceHandler.cs - stub for F805 (EVENT_WC_COUNTER_SOURCE)
namespace Era.Core.Counter;
public interface IWcCounterSourceHandler
{
    void HandleWcCounterSource(int targetCharIndex, int loopChr);
}

// ICombinationCounter.cs - abstraction over F802's existing CounterCombination (EVENT_COUNTER_COMBINATION)
namespace Era.Core.Counter;
public interface ICombinationCounter
{
    int[] AccumulateCombinations();
}

// IWcCombinationCounter.cs - WC parallel to ICombinationCounter (EVENT_WC_COUNTER_COMBINATION)
namespace Era.Core.Counter;
public interface IWcCombinationCounter
{
    int[] AccumulateCombinations();
}

// IComHandler.cs - COM dispatch handler interface
namespace Era.Core.Counter;
public interface IComHandler
{
    void Execute(ComContext context);
}

// ComContext.cs - COM execution context record
namespace Era.Core.Counter;
public record ComContext(CharacterId Target, int ActionState, int SelectCom, int PrevCom);
```

**New interface additions to existing files:**

```csharp
// IEngineVariables.cs additions (F811 scope):
/// <summary>Get PREVCOM value (previous command number)</summary>
/// Feature 811 - SOURCE Entry System
int GetPrevCom();

/// <summary>Set PREVCOM value (previous command number)</summary>
/// Feature 811 - SOURCE Entry System
void SetPrevCom(int value);
```

~~```csharp~~
~~// IRelationVariables.cs addition (F811 scope — extends F812 existing interface):~~
~~// F811 adds SetRelation; GetRelation already exists from F812~~
~~void SetRelation(CharacterId target, int playerCharIndex, int value);~~
~~```~~
<!-- SetRelation removed from F811 scope — SOURCE*.ERB never writes RELATION; F812 GetRelation sufficient -->

```csharp
// New Era.Core/Utilities/BitfieldUtility.cs:
namespace Era.Core.Utilities;
public static class BitfieldUtility
{
    // SETBIT field, bit  →  SetBit(ref field, bit)  or  field = SetBit(field, bit)
    public static int SetBit(int field, int bit) => field | (1 << bit);
    public static bool GetBit(int field, int bit) => (field & (1 << bit)) != 0;
}
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| (none) | - | - |

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
| :-----: | :---: | ------------- | :---: | :------: |
| 1 | 14, 15, 28, 29, 46 | Add GetPrevCom()/SetPrevCom() to IEngineVariables.cs; verify F803's ICharacterStringVariables.SetCharacterString is accessible (no addition needed — F803 already provides) | | [x] |
| 2 | 16, 17, 38 | Create IUpVariables.cs with GetUp/SetUp | | [x] |
| ~~3~~ | ~~20~~, ~~21~~, ~~36~~ | ~~Add GetCStr()/SetCStr() to IStringVariables.cs~~ (removed: F804 provides GetCharacterString via IVariableStore) | - | - |
| ~~4~~ | ~~22, 23~~ | ~~Create IExpUpVariables.cs~~ (removed: EXP_UP is computed, use inline calc from IVariableStore per F812 pattern) | - | - |
| 5 | 18 | Create Era.Core/Utilities/BitfieldUtility.cs with SetBit/GetBit static methods | | [x] |
| 6 | 20, 21, 22, 27, 49, 72, 79, 111, 112, 113, 114 | Create stub interfaces IWcCounterSourceHandler, ICombinationCounter, IWcCombinationCounter, IComHandler in Era.Core/Counter/ (ICounterSourceHandler already exists from F803 — verify AC#20/AC#111 compatibility; IWcCounterSystem already exists from F804) | | [x] |
| 7 | 8, 9, 10, 11, 12, 24, 50, 61, 62, 77 | Create ITouchStateManager.cs and ISourceSystem.cs in Era.Core/Counter/Source/ | | [x] |
| 8 | 1, 26, 50 | Create IKnickersSystem.cs stub interface in Era.Core/Counter/Source/ | | [x] |
| 9 | 1, 6, 7, 19, 25, 36, 39, 43, 47, 48, 51, 56, 64, 65, 66, 69, 70, 71, 73, 74, 76, 78, 81, 83, 90, 98, 99, 100, 101, 102, 103, 104, 105, 108, 116, 117, 118, 119, 122, 123, 124, 125, 127, 128, 134, 135, 137 | Implement SourceEntrySystem class (SOURCE.ERB migration, DI wiring, ICounterSystem + IWcCounterSystem + ISourceCalculator dispatch, stub interface calls, IKnickersSystem stub; inject and call ITrainingCheckService/IKojoMessageService/INtrUtilityService/IWcSexHaraService; inject IOrgasmProcessor/IVirginityManager/IFavorCalculator/IMarkSystem; inject IUpVariables for UP array access; delegate EXP_UP dual-use to ICounterUtilities.CheckExpUp; 肉便器 talent predicate in WC routing condition; delegates display output to SourceDisplayHandler (Task 17); owns CAN_COM entry condition logic; dispatches to ShootingSystem for EVENT_SHOOT; @SOURCE_CHECK sub-functions: ChkWcEquip (SOURCE.ERB:551-857), ChkInsRotorV (858-913), ChkInsRotorA (917-975), ChkSemenBath (1349-1357); inject IShrinkageSystem for shrinkage calls; inject ICounterUtilities for CheckExpUp) | | [x] |
| 10 | 1, 9, 10, 11, 12, 44, 61, 62, 75, 106, 130 | Implement TouchStateManager class (SOURCE_POSE.ERB migration, implementing ITouchStateManager with TouchSet, MasterPose, TouchResetM, TouchResetT, TouchSuccession, ShowTouch) | | [x] |
| 11 | 1, 13, 41, 57, 58, 59, 63, 67 | Implement ComCaller class with Dictionary<int, IComHandler> dispatch (SOURCE_CALLCOM.ERB migration); ITouchStateManager injection for MASTER_POSE calls; SCOM{TFLAG:50} dispatch path; CAN_COM/COM{SELECTCOM} dispatch path; TRYCALLFORM no-op fallback semantics; register all 16 TFLAG:50 handlers | | [x] |
| 12 | 1, 23, 42, 45, 60, 109, 136 | Implement ShootingSystem class (SOURCE_SHOOT.ERB migration) | | [x] |
| 13 | 2 | Create equivalence test skeleton files in Era.Core.Tests/Counter/Source/ | | [x] |
| 14a | 3 | Verify zero technical debt markers (TODO/FIXME/HACK) in migrated C# files | | [x] |
| 14b | 4, 5 | Verify equivalence tests build and all tests pass | | [x] |
| 14c | 32, 33, 34, 35, 37, 40, 84, 85, 86, 87, 89, 93, 94, 96, 97, 110, 115, 120, 121, 126, 129, 131, 132, 133 | Implement equivalence tests: assertion density (>=10), COM dispatch handler count (16), ERB-behavioral subsystem tests, CHK_SEMEN_STAY branching, WC vs non-WC dispatch routing, EXP_UP dual-use; per-subsystem Assert density (>= 2 per test class); CFLAG state mutation assertion for CHK_SEMEN_STAY; CAN_COM entry condition unit test; concrete numeric Assert.Equal assertions (>= 5); SourceDisplayHandler behavioral method name coverage; SourceDisplayHandlerTests Assert.Equal numeric equivalence (>= 3); SourceDisplayHandlerTests >= 5 assertions; TouchSuccession behavioral test; SCOM dispatch path test; SelectCom dispatch path test | | [x] |
| 15 | 30, 31 | Verify predecessor declarations for F801 and F812 exist in feature-811.md Dependencies table | | [x] |
| 16 | 52, 53, 54, 55, 56, 68 | Create stub facade interfaces ITrainingCheckService, IKojoMessageService, INtrUtilityService, IWcSexHaraService in Era.Core/Counter/ for TRACHECK.ERB/EVENT_KOJO.ERB/NTR_UTIL.ERB/WC_SexHara.ERB external function dependencies | | [x] |
| 17 | 80, 81, 82, 88, 91, 92, 95, 107 | Extract display-only sub-functions (SHOW_SOURCE_PALAMCNG ~88 lines, CHK_SEMEN_STAY display variants ~350 lines) into SourceDisplayHandler; CFLAG mutation logic at SOURCE.ERB:981-984 (精液残留中 decrement/clear) remains in SourceEntrySystem per Key Decisions. SourceDisplayHandler.ChkSemenStay dispatches to display sub-variants only (no SetCflag writes); inject IConsoleOutput for display; SourceEntrySystem delegates to SourceDisplayHandler; implement ShowSourceParamCng/ChkSemenStay/ShowSource methods | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
| :---: | --------- | --------- | --------- |
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

**When to use `[I]`**:
- AC Expected value cannot be determined before implementation
- Task output depends on investigation or runtime discovery
- Downstream tasks depend on this task's concrete output

**When NOT to use `[I]`**:
- Build/compile tasks (Expected = "succeeds" is always deterministic)
- Tasks with spec-defined outputs (API contracts, schema validation)
- Tasks that verify file existence (deterministic yes/no)
- Tasks where Expected can be calculated from requirements (counts from data, paths from config)
- Standard patterns with known outputs (error messages, status codes)

**Anti-pattern**: Using `[I]` to avoid writing concrete Expected values. `[I]` is for genuine uncertainty, not convenience.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-811.md Tasks 1-2, 5-8, 16 (interface work) | IEngineVariables additions, SetCharacterString addition, IUpVariables, BitfieldUtility, 4 stub interfaces in Counter/ (ICounterSourceHandler, IWcCounterSourceHandler, ICombinationCounter, IComHandler, IWcCombinationCounter), ITouchStateManager, ISourceSystem, IKnickersSystem, 4 facade stub interfaces in Counter/ (ITrainingCheckService, IKojoMessageService, INtrUtilityService, IWcSexHaraService) |
| 2 | implementer | sonnet | feature-811.md Tasks 9-12, 17 (class implementations) | SourceEntrySystem.cs, TouchStateManager.cs, ComCaller.cs, ShootingSystem.cs, SourceDisplayHandler.cs in Era.Core/Counter/Source/ |
| 3 | implementer | sonnet | feature-811.md Tasks 13, 14a-14c (tests) | Test files in Era.Core.Tests/Counter/Source/; passing dotnet test Era.Core.Tests |
| 4 | ac-tester | sonnet | feature-811.md all active ACs (141 total, 133 active) | AC verification results; Task 15 verification |

### Pre-conditions

Before Phase 1 begins, verify:
- `F801 [DONE]`: `ICounterSystem` interface and `ActionSelector`/`ActionValidator` implementations exist in `Era.Core/Counter/`
- `F812 [DONE]`: `ISourceCalculator` interface (or equivalent covering 30+ SOURCE_* functions) exists in `Era.Core/Counter/`
- `Era.Core/Interfaces/IEngineVariables.cs` exists (for PREVCOM additions)
- `Era.Core/Interfaces/IStringVariables.cs` exists

If F812 is not [DONE], STOP — F812 is a blocking predecessor.

### Execution Order

1. **Phase 1 — Interface Layer** (Tasks 1-2, 5-8, 16): All interface additions/creations must be done before any class implementations (Tasks 9-12), because the class implementations depend on all interfaces being defined and compilable.
   - Tasks 1-5: Modify/create interfaces in `Era.Core/Interfaces/` and `Era.Core/Utilities/`
   - Task 6: Create sibling stub interfaces in `Era.Core/Counter/`
   - Task 16: Create external facade stub interfaces in `Era.Core/Counter/` (ITrainingCheckService, IKojoMessageService, INtrUtilityService, IWcSexHaraService)
   - Tasks 7-8: Create new interfaces in `Era.Core/Counter/Source/`

2. **Phase 2 — Class Implementations** (Tasks 9-12, 17): Implement in dependency order:
   - Task 8 (IKnickersSystem) must precede Task 9 (SourceEntrySystem uses IKnickersSystem)
   - Task 7 (ITouchStateManager) must precede Task 10 (TouchStateManager implements ITouchStateManager)
   - Task 6 (IComHandler/IComDispatcher) must precede Task 11 (ComCaller uses Dictionary<int, IComHandler>)
   - Task 5 (BitfieldUtility) must precede Task 12 (ShootingSystem uses SetBit)
   - Task 9 (SourceEntrySystem) must precede Task 17 (SourceDisplayHandler — delegates from SourceEntrySystem per AC#89)

3. **Phase 3 — Tests** (Tasks 13, 14a-14c): Test files after all implementations compile.
   - Task 13 creates test file skeletons
   - Task 14a verifies zero technical debt markers
   - Task 14b verifies build and all tests pass
   - Task 14c implements equivalence tests (assertion density, handler count, subsystem tests, branching, routing)

4. **Phase 4 — Verification** (Task 15): AC tester verifies all active ACs (133 active, 8 N/A). Task 15 is a verification-only task confirming predecessor declarations in feature-811.md are present.

### Build Verification Steps

After each Phase, run:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'
```

After Phase 3:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/'
```

### DI Registration

F811 introduces new interfaces that implementing classes will register in a later Phase (when engine integration occurs). For now, stub implementations suffice for Era.Core.Tests. The following stubs are required:

- `IWcCounterSystem`: `NullWcCounterSystem` stub (throws or no-ops)
- `ICounterSourceHandler`: `NullCounterSourceHandler` stub
- `IWcCounterSourceHandler`: `NullWcCounterSourceHandler` stub
- `ICombinationCounter`: `NullCombinationCounter` stub
- `IWcCombinationCounter`: `NullWcCombinationCounter` stub
- `IKnickersSystem`: `NullKnickersSystem` stub
- `ITrainingCheckService`: `NullTrainingCheckService` stub
- `IKojoMessageService`: `NullKojoMessageService` stub
- `INtrUtilityService`: `NullNtrUtilityService` stub
- `IWcSexHaraService`: `NullWcSexHaraService` stub

These stubs should be defined adjacent to their interfaces for test use.

### Interface Snippets

**IEngineVariables.cs additions** (`Era.Core/Interfaces/IEngineVariables.cs`):
```csharp
/// <summary>Get PREVCOM value (previous command number)</summary>
/// Feature 811 - SOURCE Entry System
int GetPrevCom();

/// <summary>Set PREVCOM value (previous command number)</summary>
/// Feature 811 - SOURCE Entry System
void SetPrevCom(int value);
```

**IUpVariables.cs** (new file `Era.Core/Interfaces/IUpVariables.cs`):
```csharp
namespace Era.Core.Interfaces;

public interface IUpVariables
{
    // UP: training progress array (1D global, indexed by stat)
    int GetUp(int index);
    void SetUp(int index, int value);
}
```

**BitfieldUtility.cs** (new file `Era.Core/Utilities/BitfieldUtility.cs`):
```csharp
namespace Era.Core.Utilities;

public static class BitfieldUtility
{
    // SETBIT field, bit  →  field = SetBit(field, bit)
    public static int SetBit(int field, int bit) => field | (1 << bit);
    public static bool GetBit(int field, int bit) => (field & (1 << bit)) != 0;
}
```

**New external facade stub interfaces in `Era.Core.Counter`** (Task 16):

```csharp
// ITrainingCheckService.cs - stub facade for TRACHECK.ERB and TRACHECK_*.ERB utility functions
namespace Era.Core.Counter;
public interface ITrainingCheckService
{
    // TRACHECK_TIME (SOURCE.ERB:14) — advances game time
    void TraCheckTime();
    // SOURCE_SEX_CHECK(LOCAL) (SOURCE.ERB:100) — sex state check per character
    void SourceSexCheck(int characterIndex);
    // PLAYER_SKILL_CHECK(LOCAL) (SOURCE.ERB:101) — player skill checks per character
    void PlayerSkillCheck(int characterIndex);
    // EQUIP_CHECK(LOCAL) (SOURCE.ERB:123) — equip item check per character
    void EquipCheck(int characterIndex);
    // JUJUN_UP_CHECK (SOURCE.ERB:481) — obedience level up check
    void JujunUpCheck();
    // EXP_GOT_CHECK(LOCAL) (SOURCE.ERB:369) — experience acquisition check
    void ExpGotCheck(int characterIndex);
    // TARGET_MILK_CHECK(LOCAL) (SOURCE.ERB:226) — target milk check per character
    void TargetMilkCheck(int characterIndex);
    // MESSAGE_PALAMCNG_B2 (SOURCE.ERB:440) — orgasm-related message display
    void MessageParamCngB2();
    // MASTER_FAVOR_CHECK(ARG,ARG:1) (SOURCE.ERB:106) — master favor multiplier
    int MasterFavorCheck(int characterIndex, int sourceType);
    // TECHNIQUE_CHECK(ARG,ARG:1) (SOURCE.ERB:106) — technique multiplier
    int TechniqueCheck(int characterIndex, int sourceType);
    // MOOD_CHECK(ARG,ARG:1) (SOURCE.ERB:107) — mood multiplier
    int MoodCheck(int characterIndex, int sourceType);
    // REASON_CHECK(ARG,ARG:1) (SOURCE.ERB:107) — reason multiplier
    int ReasonCheck(int characterIndex, int sourceType);
    // SOURCE_ABLUP (SOURCE.ERB:431, TRACHECK_ABLUP.ERB) — ability-up processing
    void SourceAblUp(int characterIndex);
}
// Stub return semantics (for equivalence tests):
// - MasterFavorCheck: returns 1 (neutral multiplier — no scaling effect)
// - TechniqueCheck: returns 1 (neutral multiplier)
// - MoodCheck: returns 1 (neutral multiplier)
// - ReasonCheck: returns 1 (neutral multiplier)
// All void methods: no-op (state mutations deferred to ERB until migrated)

// IKojoMessageService.cs - stub facade for EVENT_KOJO.ERB / TOILET_EVENT_KOJO.ERB message functions
namespace Era.Core.Counter;
public interface IKojoMessageService
{
    // KOJO_MESSAGE_COM (SOURCE.ERB:375)
    void KojoMessageCom();
    // KOJO_MESSAGE_COUNTER(ARG) (SOURCE.ERB:398)
    void KojoMessageCounter(int characterIndex);
    // KOJO_MESSAGE_WC_COUNTER(ARG) (SOURCE.ERB:384)
    void KojoMessageWcCounter(int characterIndex);
    // KOJO_MESSAGE_PALAMCNG_A (SOURCE.ERB:459)
    void KojoMessageParamCngA();
    // KOJO_MESSAGE_PALAMCNG_B (SOURCE.ERB:441)
    void KojoMessageParamCngB();
    // KOJO_MESSAGE_PALAMCNG_C (SOURCE.ERB:463)
    void KojoMessageParamCngC();
    // KOJO_MESSAGE_PALAMCNG_D (SOURCE.ERB:467)
    void KojoMessageParamCngD();
    // KOJO_MESSAGE_PALAMCNG_E (SOURCE.ERB:436)
    void KojoMessageParamCngE();
    // KOJO_MESSAGE_MARKCNG(奴隷) (SOURCE.ERB:476)
    void KojoMessageMarkCng(int characterIndex);
}

// INtrUtilityService.cs - stub facade for NTR_UTIL.ERB NTR-specific functions
namespace Era.Core.Counter;
public interface INtrUtilityService
{
    // NTR_MARK_5(奴隷, 浮気快楽強度) (SOURCE.ERB:211, SOURCE.ERB:390)
    void NtrMark5(int characterIndex, int pleasureStrength);
    // NTR_ADD_SURRENDER(奴隷, 加算値) (SOURCE.ERB:901, SOURCE.ERB:963)
    void NtrAddSurrender(int characterIndex, int addValue);
}

// IWcSexHaraService.cs - stub facade for WC_SexHara.ERB and WC_SexHara_*.ERB functions
namespace Era.Core.Counter;
public interface IWcSexHaraService
{
    // WC_SexHara (SOURCE.ERB:30) — WC sexual harassment check and value calculation
    void WcSexHara();
    // WC_SexHara_SOURCE(ARG) (SOURCE.ERB:32) — WC harassment source processing
    void WcSexHaraSource(int harassLevel);
    // WC_SexHara_MESSAGEbase(ARG) (SOURCE.ERB:388) — WC harassment message display
    void WcSexHaraMessageBase(int harassLevel);
}
```

### Error Handling

- **F812 not [DONE]**: STOP. Do not proceed. F812 is a blocking predecessor. Report status to user.
- **Build failure after Phase 1**: Do not proceed to Phase 2. Fix all compilation errors first.
- **Test failure after Phase 3**: Do not mark AC#4/AC#5 as passing. Fix failing tests before proceeding.
- **TreatWarningsAsErrors**: All C# must compile without warnings. If warnings appear, treat as errors and fix before proceeding.

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred |
|-------|--------|-------------|----------------|---------------|:-----------:|
| IKnickersSystem full implementation (CLOTHES_Change_Knickers) | Phase 22 dependency; deferred per C9 | Phase | Phase 22 | - | [x] |
| ICounterSourceHandler stub replacement | F811 provides stub; F803 implements EVENT_COUNTER_SOURCE | Feature | F803 | - | [x] |
| IWcCounterSourceHandler stub replacement | F811 provides stub; F805 implements EVENT_WC_COUNTER_SOURCE | Feature | F805 | - | [x] |
| ICombinationCounter stub replacement | F811 provides stub; F802 [DONE] has CounterCombination implementation. F813 registers DI adapter (ICombinationCounter→CounterCombination) | Feature | F813 | - | [x] |
| IWcCombinationCounter stub replacement | F811 provides stub; F802 [DONE] has WcCounterCombination implementation. F813 registers DI adapter (IWcCombinationCounter→WcCounterCombination) | Feature | F813 | - | [x] |
| ITouchStateManager consumption | F811 defines interface; F803/F805/F809 must inject ITouchStateManager | Feature | F803, F805, F809 | - | [x] |
| F803 ITouchSet→ITouchStateManager migration | F803 CounterSourceHandler injects ITouchSet (26 references). When F811 completes, F803 must migrate from ITouchSet to ITouchStateManager. **Parameter mapping**: `ITouchSet.TouchSet(int mode, int type, CharacterId target)` → `ITouchStateManager.TouchSet(int targetPart, int masterPart, int character, bool reset = false)`. Concrete mapping: mode→targetPart, type→masterPart, target.Value→character (CharacterId→int unwrap), reset defaults to false. Migration is mechanical rewrite (no semantic change). **Tracking verified**: F813 Task#1 covers MasterPose SSOT unification; F803 deferred obligations include ITouchSet migration. | Feature | F803 | - | [x] |
| MasterPose consolidation | F811 ITouchStateManager.MasterPose(int targetPart, int masterPart, bool prevTurn)→int is canonical (SOURCE_POSE.ERB owner). **Adapter specification**: F804 ICounterUtilities.MasterPose(int,int)→CharacterId must delegate to ITouchStateManager.MasterPose(targetPart, masterPart, prevTurn:false) and convert int→CharacterId. F809 IComableUtilities.MasterPose(int,int,int)→int must delegate to ITouchStateManager.MasterPose(arg0, arg1, arg2!=0) mapping 3rd int to bool. Each feature removes its local MasterPose implementation and injects ITouchStateManager. | Feature | F813 | - | [x] |
| GetNWithVisitor full implementation | F811 stubs INtrUtilityService.GetNWithVisitor (default returns 0, disables 4/10 CHK_SEMEN_STAY visitor branches). NTR_VISITOR.ERB:503-513 GET_N_WITH_VISITER counts characters at visitor's location. Must be implemented when NTR_VISITOR is migrated. | Phase | Phase 21 or Phase 22 | - | [x] |

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
| 2026-02-27 09:00 | START | implementer | Task 14c | - |
| 2026-02-27 09:15 | END | implementer | Task 14c (ShootingSystemTests.cs created, 22 tests pass) | SUCCESS |
| 2026-02-27 10:00 | RESUME | orchestrator | WIP resume, all predecessors [DONE], build clean | - |
| 2026-02-27 10:05 | PASS | ac-tester | 137/137 ACs verified (AC#25 comment fix applied) | SUCCESS |
| 2026-02-27 10:10 | DEVIATION | feature-reviewer | Post-review: NEEDS_REVISION | 4 issues: TCVarActor=0 bug, GetNWithVisitor hardcoded 0, pleasureStrength=0, SSOT missing |
| 2026-02-27 10:30 | FIX | implementer | TCVarActor→TCVarActorIndex=116, fix character/element indices for TCVAR:行為者 | Build OK, 2758 tests pass |
| 2026-02-27 10:35 | FIX | implementer | pleasureStrength from orgasmResult.PleasureMarkStrength; WC visitor storage | Build OK |
| 2026-02-27 10:40 | FIX | implementer | GetNWithVisitor→INtrUtilityService (default interface method); SSOT engine-dev update | Build OK, 2758 tests pass |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: Baseline Measurement section | Created .tmp/baseline-811.txt with ERB line counts and existing C# Counter files
- [resolved-applied] Phase1-CodebaseDrift iter1: [DEP-DRIFT] IWcCounterSystem already exists (F804). AC#19 changed to consumption verification, Task 6 updated to remove IWcCounterSystem creation.
- [resolved-applied] Phase1-CodebaseDrift iter1: [DEP-DRIFT] CSTR — AC#20/21 and Task 3 removed. F811 will use F804's IVariableStore.GetCharacterString(CharacterId, CstrIndex).
- [resolved-applied] Phase1-CodebaseDrift iter1: [DEP-DRIFT] MasterPose — ITouchStateManager.MasterPose is canonical (F811 owns SOURCE_POSE.ERB). MasterPose consolidation handoff added for F804/F809.
- [fix] Phase1-CodebaseDrift iter1: AC#28 detail text | Updated baseline count from "24 existing" to "25 existing" (F812 added GetAssiPlay)
- [fix] Phase1-CodebaseDrift iter1: Feasibility Assessment | Updated interface gap count from 6 to 5 (CSTR resolved by F804)
- [info] Phase1-DriftChecked: F804 (Related)
- [resolved-applied] Phase1-CodebaseDrift iter1: [DEP-DRIFT] Baseline Measurement "Existing C# Counter files = ICounterSystem.cs + F801 files" is stale — Counter/ now contains 29 files including F804, F809, F812 contributions. Update Baseline to reflect current state.
- [info] Phase1-DriftChecked: F809 (Related)
- [fix] Phase2-Review iter1: AC#28, AC#29 Matcher column | Changed invalid matcher count_gte to gte
- [fix] Phase2-Review iter1: Constraint Details subsection | Added missing detail blocks for C7, C9, C11, C13, C14
- [fix] Phase2-Review iter1: Summary section | Removed non-template Summary section (content covered by Background)
- [fix] Phase2-Review iter1: Goal Coverage / AC Definition Table | Added AC#32 (equivalence test assertion density) and AC#33 (COM dispatch handler tests) with details, updated Goal Coverage and Task 14
- [resolved-applied] Phase3-Maintainability iter2: [DEP-DRIFT] IRelationVariables — option (a+b) applied: SetRelation added to F812 existing interface, GetUp/SetUp separated into IUpVariables. AC#16 changed to SetRelation verification. Task 2 updated.
- [resolved-applied] Phase3-Maintainability iter2: [DEP-DRIFT] EXP_UP — AC#22/23 and Task 4 removed. EXP_UP access will use inline calculation (EXP[i]-TCVAR[400+i]) from IVariableStore, following F812 pattern.
- [fix] Phase3-Maintainability iter2: Technical Design Approach | Updated "38 ACs" to "40 ACs"
- [fix] Phase3-Maintainability iter2: AC Coverage #38 | Updated "[DRAFT]" to "[DONE]" to match actual F812 status
- [fix] Phase2-Review iter3: Goal Coverage / AC Definition Table | Added AC#34 (ERB-behavioral subsystem test), AC#35 (CHK_SEMEN_STAY branching test); updated Goal Coverage, Task 14, AC Coverage, Success Criteria
- [fix] Phase2-Review iter4: AC Definition Table / Constraint C3/C11 | Added AC#36 (ISourceCalculator in SourceEntrySystem); added AC#37 (WC vs non-WC dispatch routing test); updated Goal Coverage, Tasks, AC Coverage, Success Criteria
- [fix] Phase2-Review iter5: Implementation Contract Phase 4 | Updated stale "38 ACs" to "44 ACs"
- [fix] Phase2-Review iter5: Goal Coverage | Moved AC#25/32 from Goal 2 (sibling stubs) to new Goal 8 (Phase 22 dependency); moved AC#1 from Goal 6 (equivalence) to new Goal 9 (file existence); removed F804 from Goal 2 title (now [DONE])
- [fix] Phase2-Review iter6: Task 10 AC mapping | Added AC#9-12 to Task 10 (TouchStateManager must implement all ITouchStateManager methods)
- [resolved-applied] Phase2-Review iter7: [AC-FALSE-PASS] AC#16 retargeted to SetRelation (F811 contribution). AC#19 already resolved.
- [resolved-applied] Phase2-Review iter7: [DEP-BOUNDARY] F809 ITouchStateManager consumption — resolved via MasterPose consolidation handoff to F804/F809.
- [resolved-applied] Phase2-Review iter7: [AC-WEAK] AC#33 strengthened to gte >= 16 count-based matcher for TFLAG:50 coverage.
- [fix] Phase2-Review iter8: Post-drift alignment | Task 2 AC#16 mapping added; Goal 5 removed N/A AC refs (20/21/22/23); Technical Design item 5 updated for CSTR/EXP_UP resolution; Implementation Contract Phase 1 output updated
- [fix] Phase2-Review iter9: Post-drift alignment | AC Coverage #16 desc fixed (SetRelation not Create); AC#19 matcher strengthened for SelectAction; AC#29 rationale corrected (0 new IStringVariables additions); Interface Snippets split IRelationVariables/IUpVariables
- [fix] Phase2-Review iter10: AC Details AC#20-23 | Struck through orphaned detail blocks to match N/A status in AC Definition Table; struck through AC Coverage rows 20-23; removed IStringVariables CSTR and IExpUpVariables snippets from Interface Snippets; updated Success Criteria
- [fix] Phase2-Uncertain iter10: Key Decisions / Interface Snippets | Updated IRelationVariables from "new" to "extend existing F812"; Interface Snippets now shows only SetRelation addition
- [fix] Phase2-Review iter10: Constraint Details | Reordered blocks to numeric sequence C1-C14
- [fix] Phase2-Review iter10: Tasks table | Standardized strikethrough notation for removed Task rows
- [fix] Phase2-Review iter2: AC Details AC#16 | Aligned AC#16 Details to match AC Definition Table (SetRelation in IRelationVariables.cs, not GetRelation)
- [resolved-applied] Phase2-Uncertain iter2: [TASK-GRANULARITY] Task 14 maps to 8 ACs (AC#3,4,5,39,40,41,42,44) covering diverse concerns. Reviewer suggests split into 3 subtasks. Validator uncertain — template allows N ACs:1 Task.
- [fix] PostLoop-UserFix iter3: Task 14 split | Split into Task 14a (AC#3: debt), 14b (AC#4,5: build/test), 14c (AC#32,40,41,42,44,47: equivalence tests)
- [fix] Phase2-Review iter3: IWcCounterSystem stub | Removed wrong-signature stub snippet; updated Approach item 6 to note F804 already provides IWcCounterSystem; updated AC Coverage row 25 to consume not create
- [fix] Phase2-Review iter3: C12 + Key Decisions | Updated C12 AC Impact to reflect EXP_UP inline calc resolution; struck through CSTR and EXP_UP Key Decisions rows
- [fix] Phase2-Review iter4: Philosophy Derivation | Added 'owns ... COM calling' row with AC#13, AC#27, AC#33
- [fix] Phase2-Review iter4: Pre-conditions + AC Coverage #36 | Removed stale '(for CSTR additions)' from Pre-conditions; updated AC Coverage #36 desc to remove stale GetCStr/SetCStr reference
- [fix] Phase2-Review iter5: AC#34 pattern | Extended to include EjacCheck|SamenShoot|SamenDirection for SOURCE_SHOOT.ERB sub-function behavioral coverage
- [fix] Phase2-Review iter6: Philosophy Derivation | Added 'owns ... shooting/ejaculation logic' row with AC#23, AC#34, AC#35
- [fix] Phase2-Review iter6: AC#33 matcher | Changed from ComCaller|ComDispatch|TFLAG|COM_ABLE to IComHandler for robust handler registration counting
- [fix] Phase2-Review iter6: AC#38 added | IUpVariables interface existence AC enforcing ISP design decision; updated Task 2, Goal 5, AC Coverage, Success Criteria; 44→45 ACs
- [fix] Phase2-Review iter7: Baseline Measurement | Updated to reflect 29 files in Counter/; resolved [pending] Phase1-CodebaseDrift Baseline item
- [fix] Phase2-Review iter7: AC#39 added | SourceEntrySystem ITouchStateManager reference AC; added Philosophy Derivation 'manages touch state' row; updated Task 9, Goal 3, AC Coverage; 45→46 ACs
- [fix] Phase2-Review iter8: AC#1 matcher | Changed from exists to gte >= 4 to verify all 4 ERB files have C# equivalents
- [resolved-applied] Phase2-Review iter8: [AC-REDUNDANCY] AC#17 (SetRelation in Interfaces/) is redundant with AC#16 (SetRelation in IRelationVariables.cs). Reviewer suggests removing AC#17. Validator confirmed valid.
- [fix] PostLoop-UserFix iter3: AC#17 removed | Struck through redundant AC#17; updated Task#2, Goal#5
- [resolved-applied] Phase2-Uncertain iter9: [AC-LOOP] AC#33 matcher (IComHandler >= 16) may false-pass from single registration. is_loop: matcher changed in iter6 from ComCaller pattern. Reviewer suggests InlineData-based counting.
- [fix] Phase1-DepSync iter1: Related Features F812 | Synced [DRAFT] → [DONE]; updated Feasibility row/Verdict to FEASIBLE; struck Risk row
- [fix] Phase2-Review iter1: AC#16 description | Removed hedging "IVariableStore or new interface" → "IUpVariables interface" per Key Decisions
- [fix] Phase2-Review iter1: AC#33 matcher | Changed from gte IComHandler >= 16 to matches Count.*==.*16 (handler count assertion)
- [fix] Phase2-Uncertain iter1: AC#40 added | EXP_UP inline calculation dual-use test AC; updated C12, Philosophy Derivation, Goal Coverage, Task 14, AC Coverage; 46→47 ACs
- [fix] Phase2-Review iter1: Task Tags | Added Anti-pattern warning line per template
- [fix] Phase2-Review iter2: Goal Coverage #6 | Removed AC#6/AC#7 (wiring checks) from equivalence test goal; moved to Goal #2 (stub-interface)
- [fix] Phase2-Review iter2: AC#19 Details | Synced test pattern to match AC Definition Table (_wcCounterSystem\.SelectAction|IWcCounterSystem.*SelectAction)
- [fix] Phase2-Review iter3: AC#6/AC#7 matchers | Strengthened to target SourceEntrySystem.cs specifically (matching AC#39 precision pattern)
- [fix] Phase2-Review iter4: AC Coverage #18 | Fixed inconsistency: "Create or extend IRelationVariables" → "Create IUpVariables.cs" per Key Decisions
- [fix] Phase2-Review iter4: AC#41 added | Orchestration uniqueness AC (ComCaller not_matches ICounterSystem/IWcCounterSystem/ISourceCalculator); updated Philosophy Derivation, Goal Coverage, Task 9, AC Coverage; 47→48 ACs
- [fix] Phase2-Review iter5: AC#42 added | ShootingSystem orchestration boundary check; AC#41 rationale updated with enforcement set note; 48→49 ACs
- [fix] Phase2-Review iter5: Goal Coverage #6 | Removed AC#23/AC#24 (structural checks); AC#23 → Goal #9, AC#24 → Goal #2
- [fix] Phase2-Review iter6: Task 9 AC# | Added orphaned AC#19 (IWcCounterSystem.SelectAction consumption)
- [fix] Phase2-Review iter6: Goal text | Removed F804 from "[DRAFT] siblings" (F804 is [DONE])
- [fix] Phase2-Review iter6: Goal Coverage #2/#10 | Reclassified AC#6/7/25/43/48/49 from Goal #2 to new Goal #10 (predecessor interface + orchestration SSOT); Goal #2 now stub-only
- [fix] Phase2-Review iter7: AC#43 added | ICounterSystem.SelectAction call verification (mirrors AC#19 for non-WC branch); 49→50 ACs
- [fix] Phase2-Review iter7: Task 9→11 | Moved AC#41 from Task 9 to Task 11 (ComCaller creates the artifact AC#41 verifies)
- [fix] Phase2-Review iter7: Goal Coverage #2/#4/#10 | Removed AC#24/AC#27 from Goal #2; AC#27 → Goal #4 (COM dispatch), AC#24 → Goal #10 (interface exposure)
- [fix] Phase2-Review iter8: AC#44 added | TouchStateManager orchestration boundary; completes enforcement set (AC#41/49/51); 50→51 ACs
- [fix] Phase2-Review iter8: AC#45 added | ShootingSystem BitfieldUtility usage verification; 51→52 ACs
- [fix] Phase2-Review iter9: AC#46 added | CSTR write accessor (SetCharacterString) — corrects incomplete DEP-DRIFT resolution (read-only GetCharacterString didn't cover SOURCE_SHOOT.ERB:340 write); 52→53 ACs
- [fix] Phase2-Uncertain iter9: AC#47 added | EXP_UP inline formula verification in C# source (EXP[i]-TCVAR[400+i]); 53→54 ACs
- [fix] Phase2-Review iter10: AC#46 Task | Moved from Task 12 (class) to Task 1 (interface layer) — build-order fix
- [fix] Phase2-Review iter10: AC#48 added | SourceEntrySystem ComCaller reference (positive orchestration verification); 54→55 ACs
- [fix] Phase2-Review iter10: Feasibility Assessment | Interface gaps row NEEDS_REVISION → FEASIBLE (all 6 gaps resolved)
- [resolved-applied] Phase3-Maintainability iter10: [SCOPE-GAP] ~15 external function dependencies (TRACHECK.ERB functions, KOJO_MESSAGE variants, WC_SexHara variants, NTR_UTIL functions) need DI stub interfaces. No ACs or Tasks exist for creating these facade interfaces. Requires new design decisions on interface groupings.
- [fix] PostLoop-UserFix iter3: External facade interfaces | Added ITrainingCheckService (TRACHECK.ERB utility group: TRACHECK_TIME, SOURCE_SEX_CHECK, PLAYER_SKILL_CHECK, EQUIP_CHECK, JUJUN_UP_CHECK, EXP_GOT_CHECK, TARGET_MILK_CHECK, MESSAGE_PALAMCNG_B2, MASTER_FAVOR_CHECK, TECHNIQUE_CHECK, MOOD_CHECK, REASON_CHECK), IKojoMessageService (EVENT_KOJO.ERB/TOILET_EVENT_KOJO.ERB message group: 8 KOJO_MESSAGE_* variants), INtrUtilityService (NTR_UTIL.ERB: NTR_MARK_5, NTR_ADD_SURRENDER), IWcSexHaraService (WC_SexHara.ERB group: WC_SexHara, WC_SexHara_SOURCE, WC_SexHara_MESSAGEbase); added AC#52-63, Task 16, Goal #11; updated Implementation Contract Phase 1/4; 54→59 ACs
- [resolved-applied] Phase3-Maintainability iter10: [SCOPE-GAP] SOURCE_CALLCOM.ERB:72-93 contains SCOM/CAN_SCOM/CAN_COM/COM dynamic dispatch beyond COM_ABLE{N}. TRYCALLFORM CAN_SCOM{TFLAG:50}(1), TRYCALLFORM SCOM{TFLAG:50}, TRYCALLFORM COM{SELECTCOM}. These dispatch patterns are not covered by any AC, Task, or Technical Design.
- [fix] PostLoop-UserFix iter3: SCOM/COM dispatch ACs | Added ACs for CAN_SCOM/SCOM/CAN_COM/COM dispatch (AC#57, AC#58, AC#59); updated Task 11 AC# list; updated Goal Coverage #4; updated Philosophy Derivation "owns ... COM calling" row; updated Success Criteria AC count 54→57
- [resolved-applied] Phase3-Maintainability iter10: [DEP-INCOMPATIBLE] MasterPose signature incompatibility: ICounterUtilities.MasterPose(int,int)→CharacterId vs IComableUtilities.MasterPose(int,int,int)→int vs ITouchStateManager.MasterPose(int,int,bool)→int. Handoff to F804/F809 lacks adapter pattern specification.
- [fix] PostLoop-UserFix iter3: MasterPose adapter | Added concrete adapter specification to Mandatory Handoffs: F804 delegates with prevTurn:false+int→CharacterId conversion; F809 delegates with int→bool mapping
- [fix] Phase3-Maintainability iter10: AC#49 added | IWcCombinationCounter stub (EVENT_WC_COUNTER_COMBINATION at SOURCE.ERB:28); 55→56 ACs (active: 52)
- [fix] Phase3-Maintainability iter10: Task 1 AC#29 | Reassigned orphaned AC#29 from removed Task 3 to Task 1
- [fix] Phase4-ACLint iter10: AC#20-23 Details | Struck through orphaned Details blocks; fixed Task 3 ref; corrected "56 ACs" → "52 ACs"
- [fix] Phase4-ACValidation iter10: AC#1 Expected | Changed `>= 4` to `4` (gte matcher format fix)
- [fix] Phase2-Review iter1: AC#28/AC#29 Expected | Moved regex pattern to Method column; Expected now plain number (24/2) per gte matcher format
- [fix] Phase2-Review iter1: Upstream Issues table | Replaced HTML comment with `(none)` row
- [fix] Phase2-Review iter1: Task#9 AC# | Removed AC#49 (belongs to Task#6 interface creation, not Task#9 implementation)
- [fix] Phase2-Review iter2: AC#50 added | Non-orchestrator orchestration leak check (directory-wide); complements file-enumerated AC#41/49/51 approach; 52→54 ACs (active: 54)
- [fix] Phase2-Review iter2: AC#51 added | WC routing predicate verification (TALENT:MASTER:肉便器); C15 constraint added; 53→54 ACs
- [fix] Phase2-Review iter1: Goal Coverage | Removed orphan rows 57/58 (AC numbers as Goal Items); AC#50/58 already covered in Goal #10
- [fix] Phase2-Review iter1: AC#50 rationale | Changed "supersedes" to "complements" AC#41/49/51 (defense-in-depth, not replacement)
- [fix] Phase3-Maintainability iter2: Interface Snippets | Changed GetPrevCom/SetPrevCom to default interface methods (int GetPrevCom() => 0; void SetPrevCom(int value) { }) matching F812 GetAssiPlay pattern
- [fix] Phase3-Maintainability iter2: Interface Snippets | Changed SetRelation parameter naming to match GetRelation (character, otherCharacterNo) and added default implementation
- [fix] Phase3-Maintainability iter2: AC counts | Reconciled AC count across Technical Design, Implementation Contract, Success Criteria sections
- [fix] Phase2-Review iter3: Success Criteria | Added missing AC#39 between AC#37 and AC#40
- [resolved-applied] Phase3-Maintainability iter2: [SCOPE-EXCESS] SetRelation (AC#16, Task 2) not needed for F811. SOURCE.ERB:491 only reads RELATION via GetRelation (F812). No SOURCE*.ERB file writes RELATION. Removing SetRelation reduces scope — user decision required.
- [fix] PostLoop-UserFix iter5: AC#16 removed | SetRelation removed from F811 scope; SOURCE*.ERB never writes RELATION; GetRelation (F812) sufficient for read at SOURCE.ERB:491; Task 2/Goal#5/Interface Snippets updated; 61→60 ACs
- [fix] Phase2-Review iter4: AC#33 path | Changed from Era.Core/Counter/Source/ to SourceEntrySystem.cs (matching AC#6/7/46 precision pattern)
- [fix] Phase2-Review iter4: AC#50 pattern | Extended to include sibling-dispatch stubs (ICounterSourceHandler/IWcCounterSourceHandler/ICombinationCounter/IWcCombinationCounter)
- [fix] Phase2-Review iter1: AC Coverage row 17 | Struck through orphaned row (AC#17 already N/A in Definition Table)
- [fix] Phase2-Review iter1: Interface Snippets IRelationVariables | Struck through orphaned SetRelation snippet (removed from F811 scope)
- [fix] Phase2-Review iter1: Approach section | Changed "all 60 ACs" to "all active ACs" to avoid stale hardcoded count
- [fix] Phase2-Review iter1: AC#50 Expected column | Moved pattern from Method to Expected column per not_matches convention (matching AC#41/49/51)
- [fix] Phase2-Review iter2: C14 Constraint Details | Added SOURCE.ERB:28 (EVENT_WC_COUNTER_COMBINATION→F804) and AC#49 (IWcCombinationCounter) to C14 coverage
- [fix] Phase2-Review iter2: Goal Coverage #6/#7 | Moved AC#47 from Goal 6 (equivalence tests) to Goal 7 (zero tech debt + implementation correctness); AC#47 is source-code grep, not test
- [fix] Phase2-Review iter3: AC#60 added | ShootingSystem SetCharacterString consumption (SOURCE_SHOOT.ERB:340 CSTR write); updated Task 12, Goal Coverage #5, AC Coverage; 60→61 active ACs
- [fix] Phase3-Maintainability iter4: AC#61 added | TouchSuccession method on ITouchStateManager (SOURCE_POSE.ERB:71-285, called by SOURCE.ERB:18); updated Task 7/10, Goal Coverage #3, AC Coverage, Interface Snippets
- [fix] Phase3-Maintainability iter4: AC#62 added | ShowTouch method on ITouchStateManager (SOURCE_POSE.ERB:4-70, external caller INFO.ERB:11); updated Task 7/10, Goal Coverage #3, AC Coverage, Interface Snippets
- [fix] Phase2-Review iter5: Philosophy Derivation | Added AC#61, AC#62 to 'manages touch state' row (aligning with Goal Coverage #3)
- [fix] Phase2-Review iter6: Mandatory Handoffs | Added IWcCombinationCounter to sibling stub replacement row with F804 destination (EVENT_WC_COUNTER_COMBINATION)
- [fix] Phase2-Review iter7: Task 1/AC#46 | Resolved SetCharacterString interface ambiguity: IVariableStore (pairing with existing GetCharacterString from F804); AC#46 now targets IVariableStore.cs specifically; AC Coverage row updated
- [fix] Phase2-Review iter8: AC#63 added | ComCaller ITouchStateManager injection for MASTER_POSE (SOURCE_CALLCOM.ERB:42,49,52,55); updated Task 11, Goal #4, AC Coverage
- [fix] Phase2-Review iter8: AC#64 added | SourceEntrySystem existing external interface injection (IOrgasmProcessor/IVirginityManager/IFavorCalculator/IMarkSystem); updated Task 9, Goal #10, AC Coverage
- [resolved-invalid] Phase2-Review iter9: [AC-LOOP] AC#50 glob='!SourceEntrySystem.cs' validity — raised 4 times (iter3/5/7/9), invalidated each time. Validators confirm ripgrep supports negation glob.
- [fix] Phase2-Review iter9: AC#65 added | SourceEntrySystem IWcCombinationCounter specific injection check (AC#33 OR pattern gap); updated Task 9, Goal #2, AC Coverage
- [fix] Phase2-Review iter10: Philosophy Derivation 'equivalence-tested' | Added AC#32, AC#34, AC#35, AC#37 to align with Goal Coverage #6
- [fix] Phase3-Maintainability iter10: Review Notes [pending] tag | Changed AC#50 loop item from [pending] to [resolved-invalid] (validators confirmed 4 times)
- [fix] Phase3-Maintainability iter10: Interface Snippets IComHandler | Added IComHandler interface definition with Execute() method for Dictionary<int,IComHandler> pattern
- [fix] Phase3-Maintainability iter10: DI Registration | Added IWcCombinationCounter, ITrainingCheckService, IKojoMessageService, INtrUtilityService, IWcSexHaraService stubs
- [fix] Phase4-ACLint iter10: AC count references | Updated "all 60 ACs" to "all active ACs (60 active, 6 N/A)" in Implementation Contract and Success Criteria
- [resolved-applied] Phase2-Pending iter1: Mandatory Handoffs | IWcCombinationCounter assigned to F804 [DONE] as implementation destination, but F804 cannot receive new work. Needs valid destination (F802 or new feature for WC Combination).
- [fix] Phase2-Review iter1: Execution Order AC count | Changed "(60 active, 6 N/A)" to "(69 active, 6 N/A)" at Execution Order Phase 4
- [fix] Phase2-Review iter1: AC section ownership | Added <!-- Written by: ac-designer (Phase 3) --> after ## Acceptance Criteria
- [fix] Phase2-Review iter1: AC#68 added | External facade stub interface orchestration leak check (ITrainingCheckService/IKojoMessageService/INtrUtilityService/IWcSexHaraService); updated Task 16, Goal #11, AC Coverage, Implementation Contract count (74→75, 68→69 active)
- [fix] Phase2-Review iter1: AC count reconciliation | Updated Implementation Contract (75 total, 69 active), Success Criteria (69 of 75), Execution Order (69 active, 6 N/A)
- [resolved-applied] Phase2-Pending iter2: C14 Constraint Details | SOURCE.ERB:28 attribution "→F804 WC variant" incorrect; F804 [DONE] doesn't own EVENT_WC_COUNTER_COMBINATION. Fix depends on IWcCombinationCounter destination decision (iter1 pending).
- [resolved-applied] Phase2-Pending iter2: Mandatory Handoffs row structure | Single row bundles F802/F803/F804/F805 ambiguously. Should split per-stub rows. Depends on IWcCombinationCounter destination decision (iter1 pending).
- [fix] PostLoop-UserFix iter2: IWcCombinationCounter destination | Changed from F804 [DONE] to F802 [PROPOSED]; split Mandatory Handoffs into per-stub rows (ICounterSourceHandler→F803, IWcCounterSourceHandler→F805, ICombinationCounter→F802, IWcCombinationCounter→F802); C14 attribution "→F804 WC variant" changed to "→F802 WC parallel"
- [fix] Phase2-Review iter3: AC#69/77 added | Individual stub injection checks for ICounterSourceHandler (AC#69) and ICombinationCounter (AC#70) in SourceEntrySystem.cs; mirrors AC#65 pattern for IWcCombinationCounter OR-pattern gap closure; updated Task 9, Goal #2, AC Coverage, counts (75→77 total, 69→71 active)
- [fix] Phase2-Review iter4: AC#71 added | IWcCounterSourceHandler individual injection check in SourceEntrySystem (completing AC#65/76/77/78 set for all 4 sibling stubs); updated Task 9, Goal #2, AC Coverage, counts (77→78 total, 71→72 active)
- [fix] Phase2-Review iter4: AC#47 path narrowed | Changed from Era.Core/Counter/Source/ to SourceEntrySystem.cs (EXP_UP formula belongs in SourceEntrySystem per SOURCE.ERB:113/231/413 scope)
- [fix] Phase2-Review iter5: Philosophy text | Added TOUCH_SUCCESSION/SHOW_TOUCH to Philosophy "manages touch state" claim; updated Derivation table row and AC#39 rationale to match
- [fix] Phase2-Review iter6: AC#72 added | Sibling-dispatch stub interface orchestration leak check (ICounterSourceHandler/IWcCounterSourceHandler/ICombinationCounter/IWcCombinationCounter in Counter/); updated Task 6, Goal #10, AC Coverage, counts (78→79 total, 72→73 active)
- [fix] Phase2-Review iter7: AC#73 added | SourceEntrySystem TouchSuccession call-site verification (parallels AC#43 SelectAction pattern); updated Task 9, Goal #3, AC Coverage, counts (79→81, 73→75 active)
- [fix] Phase2-Review iter7: AC#74 added | SourceEntrySystem ComCaller dispatch call-site verification (parallels AC#43 SelectAction pattern); updated Task 9, Goal #4, AC Coverage
- [fix] Phase3-Maintainability iter8: AC#82/83 added | IConsoleOutput injection for SourceEntrySystem and TouchStateManager display output (C7 constraint); updated Tasks 9/10, Goal #10, AC Coverage
- [fix] Phase3-Maintainability iter8: AC#76 added | IRelationVariables injection for SourceEntrySystem RELATION read at SOURCE.ERB:491 (C8 constraint); updated Task 9, Goal #5, AC Coverage
- [fix] Phase3-Maintainability iter8: AC#77 added | ITouchStateManager.MasterPose int return type enforcement per Mandatory Handoffs adapter spec; updated Task 7, Goal #3, AC Coverage
- [resolved-applied] Phase3-Maintainability iter8: SourceEntrySystem 20+ constructor deps | Reviewer suggests extracting display output (SHOW_SOURCE_PALAMCNG, CHK_SEMEN_STAY ~458 lines) into SourceDisplayHandler class. Requires new design decisions (class boundary, responsibility split).
- [fix] Phase3-Maintainability iter8: AC count reconciliation | Updated counts to 85 total, 79 active
- [fix] Phase2-Review iter1: AC Coverage #86 | Added missing AC#78 row to AC Coverage table (IKnickersSystem injection)
- [fix] Phase2-Review iter1: Implementation Contract Phase 4 counts | Updated 85→86 total; AC#33 marked N/A (superseded by AC#65/76/77/78); 79 active, 7 N/A
- [fix] Phase2-Review iter1: AC#33 N/A | Marked superseded by individual AC#65/76/77/78; removed from Task 9 AC# list
- [resolved-applied] Phase2-Uncertain iter1: [AC-GAP] COM calling ownership — no AC verifies SourceEntrySystem owns CAN_COM entry condition logic (when to invoke COM calling), only ComCaller reference (AC#48/81). Philosophy claims SourceEntrySystem "owns ... COM calling".
- [resolved-applied] Phase2-Uncertain iter1: [AC-WEAK] AC#34 equivalence matcher weakness — Grep for ERB function name strings does not verify actual equivalence assertions. Philosophy requires "equivalence-tested against legacy behavior" but AC#34 only checks test method name presence.
- [fix] Phase2-Review iter2: Success Criteria AC#33 | Struck through AC#33 in Success Criteria (N/A, superseded by AC#65/76/77/78)
- [fix] Phase2-Review iter2: Task 9→7/8 AC#50 | Moved AC#50 from Task 9 to Tasks 7 and 8 (interface file purity check belongs with file creators)
- [fix] Phase2-Review iter2: Philosophy Derivation SSOT row | Added AC#68 and AC#72 to 'SSOT for counter event orchestration' derivation
- [fix] Phase2-Review iter3: AC#79 added | IComHandler orchestration leak check (Counter/ interface omitted from AC#68/79); updated Task 6, Goal #10/SSOT row, AC Coverage, counts (86→87 total, 79→80 active)
- [fix] PostLoop-UserFix iter4: SourceDisplayHandler | Added Task 17, AC#80-90 for display output extraction; AC#1 Expected 4→5; SourceEntrySystem delegates to SourceDisplayHandler
- [fix] PostLoop-UserFix iter4: COM entry condition | Added AC#83 for CAN_COM entry condition logic in SourceEntrySystem; updated Philosophy Derivation "owns ... COM calling" row
- [fix] PostLoop-UserFix iter4: Per-subsystem assertion density | Added AC#84-95 for per-test-class Assert count >= 2; updated Philosophy Derivation "equivalence-tested" row; updated Task 14c
- [fix] PostLoop-UserFix iter4: AC#35 matcher strengthened | Changed from name-match to InlineData/Theory variant count (gte >= 3)
- [fix] Phase2-Review iter5: AC#82 N/A | Display delegated to SourceDisplayHandler (Task#17); AC#81/90 cover IConsoleOutput chain; removed from Task 9
- [fix] Phase2-Review iter5: AC#1 threshold | Raised from gte 5 to gte 8 (5 impl + 3 interface files in Counter/Source/); updated AC Details
- [fix] Phase2-Review iter6: AC#88/97 added | SourceDisplayHandler method existence (ShowSourceParamCng/ChkSemenStay) + test density; prevents empty-class false-pass; updated Task 17; 95→97 total, 87→89 active
- [fix] Phase2-Review iter6: AC#35 threshold | Raised from gte 3 to gte 8 (matching 8 documented sub-function variants in CHK_SEMEN_STAY)
- [fix] Phase3-Maintainability iter7: AC#35 Details | Synced Details block Expected from >= 3 to >= 8 (matching Definition Table); updated title and Success Criteria
- [fix] Phase3-Maintainability iter8: ITrainingCheckService | Added SourceAblUp (SOURCE.ERB:431, TRACHECK_ABLUP.ERB) to Interface Snippets
- [fix] Phase2-Review iter9: AC#88 matcher | Changed from OR-pattern `matches` to `gte >= 2` with `ShowSourceParamCng\(|ChkSemenStay\(` (prevents single-pattern false-pass)
- [fix] Phase2-Review iter9: AC#35 matcher | Removed `SemenStay.*\\d` branch (too broad); retained `InlineData.*SemenStay|Theory.*SemenStay` only
- [fix] Phase2-Review iter9: Goal Coverage #3/#10 | Moved AC#75 from Goal #10 (orchestration SSOT) to Goal #3 (manages touch state); added to Philosophy Derivation row
- [resolved-applied] Phase2-Uncertain iter4: [AC-WEAK] AC#35 CHK_SEMEN_STAY branching — Grep for test method name only; no AC verifies 8 sub-function branch variants are individually tested. Risk states "must test branching separately" but AC#35 accepts a single test method.
- [fix] Phase2-Review iter1: AC#35 consistency | Unified CHK_SEMEN_STAY variant count to 10 across AC Definition Table, AC Coverage, Success Criteria, and Risks table (was 3/8/10 inconsistency)
- [fix] Phase2-Review iter1: Risks table | Changed "8 variants" to "10 variants" for CHK_SEMEN_STAY
- [resolved-applied] Phase2-Uncertain iter1: [DESIGN-BOUNDARY] CHK_SEMEN_STAY (~370 lines, 10 variants) classified as display output in SourceDisplayHandler (Task 17), but contains game state mutations (CFLAG writes at SOURCE.ERB:982-984). Separating game logic (semen state decisions) from display (text output) may be needed. Requires design decision on boundary between SourceEntrySystem/ShootingSystem and SourceDisplayHandler.
- [fix] Phase2-Review iter1: Interface Snippets | Removed default implementations from GetPrevCom/SetPrevCom (=> 0 / { }) — declared as abstract interface members
- [fix] Phase2-Review iter2: AC#82 Details/Goal | Struck through AC#82 Details block (N/A disposition); removed AC#82 from Goal Coverage #10 (replaced with AC#79)
- [fix] Phase2-Review iter3: AC#35 table format | Fixed 8-column malformed row — moved pattern from Expected to Method column; put count 10 in Expected (matching AC#34 gte format)
- [fix] Phase2-Review iter4: Philosophy Derivation | Expanded "all ERB counter logic migrated" coverage from AC#1/2 to include AC#4/5/39/41 (build+test+equivalence verification)
- [fix] Phase2-Review iter5: AC#35 matcher | Strengthened from InlineData/Theory attribute-only to actual variant names (WithMaster/WithVisitor/Without/SemenStay) — prevents false-pass from identical entries
- [fix] Phase2-Uncertain iter5: AC#87 threshold | Raised ShootingSystemTests assertion minimum from >= 2 to >= 5 (510-line SOURCE_SHOOT.ERB scope warrants higher density)
- [fix] Phase2-Review iter6: AC#35 matcher | Replaced generic "Without" with SemenStay-anchored compound names (SemenStay.*With/SemenStay.*Without/WithMasterAndVisitor/etc.) — prevents false-pass from unrelated identifiers
- [fix] Phase2-Review iter1: AC#90 added | SourceEntrySystem ShootingSystem dispatch call-site verification (parallels AC#43/AC#73/AC#74 pattern); updated Task 9, Goal Coverage, Philosophy Derivation, AC Coverage, Implementation Contract counts; 97→98 total, 89→90 active
- [fix] Phase2-Uncertain iter1: AC#89 threshold | Raised SourceDisplayHandlerTests assertion minimum from >= 2 to >= 5 (458-line display migration scope warrants higher density, matching AC#87 pattern)
- [fix] Phase2-Review iter1: Success Criteria subsection | Removed non-template ### Success Criteria from Technical Design (content redundant with AC Definition Table)
- [fix] Phase2-Review iter1: AC#33 Details | Struck through AC#33 Details block to match N/A status in AC Definition Table (superseded by AC#65/76/77/78)
- [fix] Phase2-Review iter2: AC#90 matcher | Tightened from `_shootingSystem\.\|ShootingSystem\.\|EventShoot` to `_shootingSystem\.\\w+\\(\|EventShoot\\(` — requires parenthesis for method-call evidence, consistent with AC#43/AC#73/AC#74 patterns
- [fix] Phase2-Review iter3: AC#88 split | Split AC#88 OR-pattern (gte 3) into individual method checks: AC#88 (ShowSourceParamCng), AC#91 (ChkSemenStay), AC#92 (ShowSource) — mirrors AC#9/10/11/12 individual method pattern; prevents false-pass from single method appearing 3+ times
- [fix] Phase2-Review iter3: AC#93 added | CHK_SEMEN_STAY CFLAG state mutation assertion AC; Risks table requires "test branching separately from text output"; SOURCE.ERB:982-984 CFLAG writes must be assertion-covered; updated Task 14c, Goal #6, AC Coverage; 98→102 total, 90→94 active
- [fix] Phase2-Review iter3: AC#94 added | CAN_COM entry condition test AC; parallels AC#37 (WC routing test) for COM branch decision; AC#83 verifies condition in source, AC#94 verifies it's unit-tested; updated Task 14c, Goal #4, AC Coverage
- [fix] Phase2-Review iter4: AC#95 added | SourceDisplayHandler orchestration boundary enforcement (not_matches ICounterSystem/IWcCounterSystem/ISourceCalculator); extends AC#41/49/51 set to cover SourceDisplayHandler (added after enforcement set); updated Task 17, Goal #10, Philosophy Derivation, AC Coverage; 102→104 total, 94→96 active
- [fix] Phase2-Review iter4: AC#96 added | Equivalence test numeric assertion AC (Assert.Equal with numeric expected >= 5); strengthens Philosophy "equivalence-tested" claim beyond generic Assert counts (AC#32/92-95); updated Task 14c, Goal #6, Philosophy Derivation, AC Coverage
- [fix] Phase2-Review iter5: AC#97 added | SourceDisplayHandler behavioral equivalence test name AC (ShowSourceParamCng/ChkSemenStay/ShowSource in tests); extends AC#34 coverage to 5th subsystem; updated Task 14c, Goal #6, Philosophy Derivation, AC Coverage; 104→105 total, 96→97 active
- [fix] Phase2-Review iter5: AC#89 rationale | Clarified display test assertion paradigm (IConsoleOutput mock verification vs numeric Assert.Equal); AC#96 expected to be satisfied by non-display classes
- [fix] Phase2-Review iter6: AC#98-108 added | Individual facade injection ACs (IKojoMessageService, INtrUtilityService, IWcSexHaraService); closes AC#56 OR-pattern gap following AC#65/76/77/78 precedent; updated Task 9, Goal #11, AC Coverage
- [fix] Phase2-Review iter6: AC#89 rationale v2 | Resolved matcher/paradigm contradiction: Assert.\w+\( is correct matcher; display tests combine Assert calls with mock verification; >= 5 achievable due to CHK_SEMEN_STAY CFLAG mutations
- [fix] Phase2-Review iter6: AC#101-111 added | Individual external interface injection ACs (IVirginityManager, IFavorCalculator, IMarkSystem); closes AC#64 OR-pattern gap; updated Task 9, Goal #10, AC Coverage; 105→111 total, 97→103 active
- [fix] Phase2-Review iter7: AC#104 added | ITrainingCheckService individual injection AC; closes remaining AC#56 OR-pattern gap (1st interface omitted in iter6); updated Task 11, AC Coverage; 111→113 total, 103→105 active
- [fix] Phase2-Review iter7: AC#105 added | IOrgasmProcessor individual injection AC; closes remaining AC#64 OR-pattern gap (1st interface omitted in iter6); updated Task 10, AC Coverage
- [fix] Phase2-Review iter8: Task 9 AC# column | Added AC#104, AC#105 to Task 9 (SourceEntrySystem implementation); both ACs grep SourceEntrySystem.cs, matching AC#98-111 pattern already in Task 9
- [fix] Phase2-Review iter9: Implementation Contract AC count | Updated stale count from (111 total, 103 active) to (113 total, 105 active); also updated Execution Order Phase 4 description
- [fix] Phase2-Review iter10: Goal item 2 text | Changed "[DRAFT] siblings" to "not-yet-[DONE] siblings" (F802=[WIP], F803=[PROPOSED], only F805 is [DRAFT])
- [fix] Phase2-Review iter10: AC#106 added | ShowTouch dispatch call-site verification; parallel to AC#73 (TouchSuccession) — Philosophy claims SHOW_TOUCH managed but no prior AC verified call-site; updated Task 9, Goal #3, Philosophy Derivation; 113→114 total, 105→106 active
- [fix] Phase2-Review iter10: AC#36 strengthened | Changed from `matches ISourceCalculator` to `gte 20 _sourceCalculator\.` — verifies 30+ SOURCE_* calls routed through interface per C11 constraint, not just single reference
- [resolved-applied] Phase3-Maintainability iter10: [DESIGN-DECISION] IComHandler.Execute() has no parameters — COM handlers need execution context (target character, action state). Requires design decision on parameter passing (explicit ComContext parameter vs shared state).
- [fix] Phase3-Maintainability iter10: Key Decisions table | Added entry documenting SourceEntrySystem 20+ constructor dependency count as acceptable (orchestrator hub pattern); SOURCE.ERB CALLs 6+ sibling features, each DI dependency maps 1:1 to ERB CALL target
- [fix] Phase3-Maintainability iter10: AC#36 threshold | Raised from gte 20 to gte 30 (matching "30+ SOURCE_* function calls" claim in Goal, Problem, C3, C11)
- [fix] Phase4-ACValidation iter10: AC Details blocks | Added **AC#N:** block format entries for AC#79-95, AC#104-114 (lint tool requires block format, not table format)
- [fix] Phase4-ACValidation iter10: Goal Coverage gaps | Added AC#80-100 to Goal Coverage table (AC#80→Goal 9, AC#81-90,96,98-100→Goal 10, AC#83→Goal 4, AC#84-95,97→Goal 6)
- [fix] PostLoop-UserFix iter10: CHK_SEMEN_STAY design boundary | User chose A: separate CFLAG mutation from display. CFLAG writes (SOURCE.ERB:982-984) move to SourceEntrySystem/ShootingSystem; SourceDisplayHandler remains display-only. Added Key Decisions entry.
- [fix] PostLoop-UserFix iter10: IComHandler.Execute(ComContext) | User chose A: ComContext record parameter. Updated Interface Snippets IComHandler with Execute(ComContext context) and added ComContext record definition. Added Key Decisions entry.
- [fix] Phase2-Review iter1: AC#36 AC Coverage row | Aligned ">= 20" to ">= 30" to match AC Definition Table threshold (gte 30)
- [fix] Phase3-Maintainability iter2: AC#106 Task reassignment | Moved AC#106 from Task 9 (SourceEntrySystem) to Task 17 (SourceDisplayHandler) — ShowTouch is display output
- [fix] Phase3-Maintainability iter2: AC#107 added | SourceDisplayHandler CFLAG boundary enforcement (not_matches SetCflag/Cflag assignment); enforces display-only design per Key Decisions; updated Task 17, Goal #10, AC Coverage, Implementation Contract counts (114→115 total, 106→107 active)
- [fix] Phase3-Maintainability iter2: AC#36 Details | Added ITrainingCheckService boundary clarification note — TRACHECK.ERB functions outside SOURCE.ERB:127-335 range, no overlap with ISourceCalculator
- [resolved-applied] Phase2-Pending iter3: [AC-WEAK] AC#36 gte 30 counts total _sourceCalculator. invocations but doesn't verify method diversity — 30 calls could be to fewer distinct methods. Reviewer suggests complementary AC for >= 5 distinct _sourceCalculator.Source method names.
- [fix] Phase2-Review iter3: Goal Coverage #2 | Removed N/A AC#33 from Goal Item 2 coverage list (superseded by AC#65/76/77/78)
- [resolved-applied] Phase2-Pending iter4: [AC-GAP] No AC verifies CFLAG mutation logic (SetCflag/Cflag/SemenResidual) exists in SourceEntrySystem.cs or ShootingSystem.cs — positive-side of CHK_SEMEN_STAY design boundary unverified. AC#107 is negative-only. → Resolved by AC#109.
- [fix] Phase2-Review iter4: AC#2 threshold | Strengthened from exists to gte 4 (one test file per migrated ERB: SourceEntrySystemTests, TouchStateManagerTests, ComCallerTests, ShootingSystemTests)
- [fix] Phase2-Review iter5: AC#3 scope broadened | Changed from Counter/Source/ to Counter/ — covers stub interfaces created by Task 6/8/16 in Era.Core/Counter/
- [fix] Phase2-Review iter5: AC#36 Detail reconciled | Updated Detail block to match Definition Table: _sourceCalculator\. in SourceEntrySystem.cs, Count >= 30 (was stale broader-grep description)
- [fix] PostLoop-UserFix iter6: AC#108 added | ISourceCalculator method diversity check (gte 5 distinct sourceCalculator.Source names); complements AC#36 count threshold; updated Task 9, Goal #10, AC Coverage, Implementation Contract (115→116 total, 107→108 active)
- [fix] PostLoop-UserFix iter7: AC#109 added | ShootingSystem CFLAG mutation positive check (SetCflag/SemenStay/精液残留); resolves pending AC-GAP (positive-side of CHK_SEMEN_STAY boundary); updated Task 12, Goal Coverage (shooting/ejaculation row), AC Coverage, Implementation Contract (116→117 total, 108→109 active)
- [fix] Phase2-Review iter1: AC#1 Coverage + Task#8 | Added IKnickersSystem.cs to AC Coverage enumeration (was 7 files listed, now 8); added AC#1 to Task#8 AC# column
- [fix] Phase2-Uncertain iter2: AC#62 narrowed | Removed "or ISourceSystem" ambiguity; locked ShowTouch to ITouchStateManager.cs per Interface Snippets and Philosophy; updated AC Definition Table, Details, and AC Coverage
- [fix] Phase2-Uncertain iter3: AC#8 strengthened | Pattern from `interface ITouchStateManager` to `public interface ITouchStateManager`; path narrowed to ITouchStateManager.cs; ensures sibling features can consume the interface (F803/F805/F809)
- [resolved-skipped] Phase3-Maintainability iter4: [DEP-HANDOFF] MasterPose consolidation handoff destination F813 is [DRAFT] review feature. User chose A: keep in F813 — Post-Phase Review handles cross-feature adapter integration; adapter specs already documented in Mandatory Handoffs.
- [resolved-applied] Phase3-Maintainability iter4: [DESIGN-GUARD] SourceEntrySystem 20+ constructor deps: Key Decisions (iter10) documents as acceptable but no maximum count guard rail. Reviewer suggests explicit cap to prevent further growth.
- [fix] PostLoop-UserFix iter2: C16 constraint + AC#117 | Added C16 (SourceEntrySystem constructor cap <= 25) with AC#117 (lte matcher on private readonly I fields); updated Task 9, Goal #10, AC Coverage, Constraint Details
- [resolved-applied] Phase3-Maintainability iter4: [AC-THRESHOLD] AC#36 gte 30 vs ISourceCalculator's 35 methods — threshold could miss 5 uncalled methods. Changed 4+ times already (20→30). Exact ERB call count in SOURCE.ERB:127-335 needs verification before raising to 35.
- [fix] PostLoop-UserFix iter2: AC#36 threshold | Raised from gte 30 to gte 35 (ISourceCalculator.cs has exactly 35 methods, verified); updated AC Definition Table, AC Details, AC Coverage
- [fix] Phase3-Maintainability iter4: ITrainingCheckService stub returns | Added stub return semantics for 4 int-returning multiplier methods (MasterFavorCheck/TechniqueCheck/MoodCheck/ReasonCheck → 1 neutral)
- [fix] Phase3-Maintainability iter4: Key Decisions ShowTouch | Added ShowTouch placement decision (keep in TouchStateManager — display coupled to internal touch state)
- [fix] Phase3-Maintainability iter4: Key Decisions ComContext | Added ComCaller sole-creator note for positional record construction
- [fix] Phase2-Review iter5: AC#110 added | SourceDisplayHandlerTests Assert.Equal numeric equivalence AC; strengthens Philosophy "equivalence-tested" claim beyond AC#35 naming patterns; updated Task 14c, Goal #6, Philosophy Derivation, AC Coverage
- [fix] Phase2-Review iter5: Approach "empty stubs" text | Changed to "stub interfaces with forward-compatible method signatures" per Interface Snippets
- [fix] Phase2-Review iter5: AC#111-122 added | Stub interface method signature verification (HandleCounterSource/HandleWcCounterSource/ProcessCombination/ProcessWcCombination); ensures forward-compatibility for F802/F803/F805; updated Task 6, Goal #2, AC Coverage, Implementation Contract (117→122 total, 109→114 active)
- [fix] Phase2-Review iter6: AC#108 pattern fix | Changed sourceCalculator\.Source to _sourceCalculator\.Source (underscore prefix for private field DI convention, consistent with AC#36)
- [fix] Phase2-Review iter6: AC#115 added | TouchSuccession behavioral test in TouchStateManagerTests; 215-line function had no dedicated test AC; updated Task 14c, Goal #6, Philosophy Derivation, AC Coverage, Implementation Contract (122→123 total, 114→115 active)
- [fix] Phase2-Review iter1: AC#110 table format | Moved Assert\.Equal\( pattern from Expected column to Method column Grep parameters; Expected now plain number 3
- [fix] Phase2-Review iter1: AC#107/116/117 Details | Reformatted non-standard File/Pattern labels to template-required Test/Expected/Rationale format
- [fix] Phase2-Review iter1: AC#2 Details Expected | Changed "At least one test .cs file exists" to "At least 4 test .cs files exist" matching gte 4 in Definition Table
- [fix] Phase2-Review iter1: AC#116 added | SourceEntrySystem ISourceSystem implementation check; prevents AC#24 false-positive on interface declaration; updated Task 9, Goal #10, AC Coverage (123→124 total, 115→116 active)
- [fix] Phase2-Review iter2: AC#109 path broadened | Changed from ShootingSystem.cs to Era.Core/Counter/Source/ directory; Key Decisions documents dual-ownership (SourceEntrySystem/ShootingSystem); updated AC Details, AC Coverage
- [fix] Phase2-Uncertain iter3: Task#17 AC#110 | Added AC#110 to Task#17 AC# column (SourceDisplayHandlerTests Assert.Equal equivalence); reduces split-ownership risk between Task#17 and Task#14c
- [fix] Phase2-Review iter4: AC count reconciliation | Updated Implementation Contract Phase 4 (123→124 total, 115→116 active) and Execution Order Phase 4 (106→116 active)
- [resolved-applied] Phase2-Pending iter5: [AC-BRITTLE] AC#36 (_sourceCalculator\. gte 30) and AC#108 (_sourceCalculator\.Source gte 5) both hardcode the injected field name. If implementer uses different field naming convention, both ACs produce false negatives. No existing AC verifies ISourceCalculator is injected via constructor parameter. Requires design decision on how to address brittleness (add constructor injection AC, relax field name pattern, or document naming constraint).
- [fix] PostLoop-UserFix iter2: AC#118 + naming convention | Added AC#118 (ISourceCalculator naming-agnostic injection check) + Key Decisions DI field naming convention; updated Task 9, Goal #10, AC Coverage, Implementation Contract
- [fix] Phase2-Review iter1: AC Definition Table N/A rows | Removed 8 struck-through N/A rows (AC#16,17,20-23,33,82) — template has no precedent for strikethrough in table cells
- [resolved-applied] Phase2-Pending iter1: [AC-COMPLETENESS] AC#36 gte 30 counts _sourceCalculator. invocations but no AC verifies ISourceCalculator.cs declares >= 30 method signatures. Implementation could satisfy call-count AC while only implementing a fraction of the 30+ SOURCE_* functions. Propose: add AC for ISourceCalculator interface method declaration count >= 30, or raise AC#108 threshold to >= 15.
- [fix] PostLoop-UserFix iter2: AC#119 | Added ISourceCalculator method declaration count AC (gte 35); complements AC#36 call-count with declaration-count; updated Task 9, Goal #10, AC Coverage, Implementation Contract
- [resolved-applied] Phase2-Pending iter1: [AC-LOOP] AC#35 CHK_SEMEN_STAY name-pattern matcher weakness — test method naming does not guarantee branch coverage. AC#35 has been strengthened 6+ times (iter2-10 prior FL runs) and reviewer still finds it insufficient. Propose: change to InlineData/Theory count >= 10. AC#93 threshold (>= 1) may also be too low for 10-variant function.
- [fix] PostLoop-UserFix iter2: AC#35 + AC#93 | AC#35 changed from name-pattern (SemenStay.*With) to InlineData/Theory parameterized test verification; AC#93 threshold raised from gte 1 to gte 3; root cause fix for 6+ iteration loop
- [fix] Phase4-ACLint iter1: AC Details orphans | Removed struck-through AC#16, AC#33, AC#82 Details blocks (removed from Definition Table in iter1 N/A cleanup)
- [fix] Phase4-ACLint iter1: AC#109 Goal Coverage | Added AC#109 to Goal #7 (zero tech debt / implementation correctness)
- [fix] Phase2-Review iter2: Task 17 CHK_SEMEN_STAY boundary | Clarified display-only extraction; CFLAG mutation logic stays in SourceEntrySystem per Key Decisions; AC#91 rationale updated
- [fix] Phase2-Review iter2: AC#2 threshold | Raised from gte 4 to gte 5 to include SourceDisplayHandlerTests; updated AC Details and AC Coverage
- [fix] Phase2-Review iter3: AC#108 threshold | Raised from gte 5 to gte 20 — 35 ISourceCalculator methods exist; 5 was trivially satisfiable; 20 ensures meaningful diversity verification
- [fix] Phase2-Review iter4: Task 17 AC#97 alignment | Added AC#97 to Task 17 AC# list — Task 17 creates SourceDisplayHandlerTests.cs, AC#97 verifies ShowSourceParamCng/ChkSemenStay/ShowSource in that file; artifact producer must own verification AC
- [fix] Phase2-Review iter5: Task 17 Implementation Contract | Added Task 17 to Phase 2 with dependency on Task 9 (SourceEntrySystem delegates to SourceDisplayHandler per AC#81); was previously unassigned to any Phase
- [fix] Phase2-Review iter6: CFLAG mutation convention | Added Key Decisions entry mandating SetCflag() method for all CFLAG mutations; AC#107/117 patterns rely on SetCflag token for mutation detection; direct array mutations (Cflag[i]--, -=) would evade patterns; convention makes AC pair complete by design
- [fix] Phase2-Review iter7: AC#93 pattern tightening | Removed bare `SetCflag` alternative from AC#93 pattern — bare SetCflag matched any CFLAG mutation test, not just SemenStay-context ones; remaining 3 branches all require SemenStay co-occurrence with Cflag/CFLAG; updated AC Details rationale
- [fix] Phase2-Review iter8: AC#120/129 added | SCOM dispatch path (AC#120) and SelectCom dispatch path (AC#121) test-existence ACs; parallels AC#37/AC#94 for COM dispatch behavioral coverage; updated Task 14c, Goal #4, AC Coverage, Implementation Contract (127→129 total, 119→121 active)
- [resolved-applied] Phase2-Pending iter9: [SCOPE-GAP] 4 SOURCE.ERB internal functions missing → Added to Task 9 (SourceEntrySystem) + AC#122-133 method existence ACs. CHK_WC_EQUIP, CHK_INS_ROTOR_V, CHK_INS_ROTOR_A, CHK_SEMEN_BATH all contain CFLAG mutations → assigned to SourceEntrySystem. Updated Task 9 description, Goal #10, AC Coverage, Implementation Contract (129→133 total, 121→125 active).
- [fix] PostLoop-UserFix iter9: AC#122-133 added | @SOURCE_CHECK sub-function method existence ACs (ChkWcEquip, ChkInsRotorV, ChkInsRotorA, ChkSemenBath); resolves SCOPE-GAP for ~430 lines of SOURCE.ERB previously unaccounted for
- [resolved-applied] Phase3-Maintainability iter1: [DEP-DRIFT] ICounterSourceHandler already exists (F803) → Adopted F803's interface. Task 6 updated to remove ICounterSourceHandler creation (already exists). AC#20/AC#111 patterns already compatible with F803's HandleCounterSource(CharacterId,int). F811 consumes, not creates.
- [resolved-applied] Phase3-Maintainability iter1: [DEP-DRIFT] SetCharacterString on ICharacterStringVariables → AC#46 path updated from IVariableStore.cs to ICharacterStringVariables.cs. AC#60 already name-agnostic (greps ShootingSystem.cs for SetCharacterString). AC Details rationale updated.
- [resolved-applied] Phase3-Maintainability iter1: [DEP-DRIFT] ITouchSet vs ITouchStateManager → Both coexist. ITouchStateManager is F811's expanded interface (TouchSet+MasterPose+TouchSuccession+ShowTouch). F803's ITouchSet is interim. F813 consolidation will merge. Mandatory Handoffs updated.
- [resolved-applied] Phase3-Maintainability iter1: [DEP-DRIFT] CheckExpUp vs inline → Adopt F803's ICounterUtilities.CheckExpUp. AC#47 pattern to be updated from inline formula to CheckExpUp call reference. Eliminates SSOT duplication.
- [resolved-applied] Phase3-Maintainability iter1: [DEP-DRIFT] IShrinkageSystem → Verify SOURCE.ERB shrinkage calls and add to Dependencies if applicable. User approved adding dependency.
- [resolved-applied] Phase3-Maintainability iter1: [DEP-DRIFT] IClothingSystem.ChangeKnickers vs IKnickersSystem → Keep IKnickersSystem stub. Phase 22 CLOTHES system is separate Feature scope; IKnickersSystem stub maintains ISP separation for F811.
- [resolved-applied] Phase3-Maintainability iter1: [DEP-DRIFT] F803 interfaces not in Philosophy → User approved Philosophy Derivation update to reflect F803's interface adoptions (ICounterSourceHandler, ICharacterStringVariables, CheckExpUp, IShrinkageSystem). Deferred to Phase 2 re-run for actual edits.
- [resolved-applied] Phase3-Maintainability iter1: [DEP-HANDOFF] MasterPose dual-interface → Document in Key Decisions: IComableUtilities.MasterPose (F803) and ITouchStateManager.MasterPose (F811) coexist until F813 consolidation.
- [resolved-applied] Phase3-Maintainability iter1: [DEP-HANDOFF] F803 ITouchSet→ITouchStateManager → Add to Mandatory Handoffs: F803 CounterSourceHandler must migrate from ITouchSet to ITouchStateManager when F811 completes. User approved.
- [fix] Phase2-Review iter1: Tasks table Task 3/4 status | Changed `[-]` to `[ ]` — `[-]` means FAIL per template, removed tasks should not use FAIL marker
- [fix] Phase2-Review iter1: Task 1 description + AC Coverage row 53 | Updated IVariableStore → ICharacterStringVariables — F803 already provides SetCharacterString on ICharacterStringVariables; Task 1 no longer creates, only verifies
- [fix] Phase2-Review iter1: AC#126 added | WC routing test predicate verification (肉便器/WcMode/IsWc in SourceEntrySystemTests); closes gap between AC#51 (source predicate) and AC#37 (test existence); 133→134 total, 125→126 active
- [resolved-skipped] Phase2-Uncertain iter1: [AC-ROBUSTNESS] AC#35 SemenStay equivalence test matcher (InlineData.*SemenStay pattern, gte 10) may false-positive/negative. Reviewer proposes splitting into Theory attribute check + InlineData count. Validator uncertain — current matcher was deliberately crafted over 6+ iterations. User chose: keep current matcher.
- [fix] Phase2-Review iter2: Philosophy Derivation "owns ... COM calling" | Added AC#63, AC#67, AC#74, AC#94, AC#120, AC#121 to align with Goal Coverage row #4
- [fix] Phase3-Maintainability iter3: AC#47 Details/Coverage sync | Updated from inline EXP[i]-TCVAR[400+i] to CheckExpUp/ICounterUtilities per resolved-applied DEP-DRIFT decision
- [fix] Phase3-Maintainability iter3: Dependencies table | Added IShrinkageSystem and ICounterUtilities as External/Exists
- [fix] Phase3-Maintainability iter3: AC#127, AC#128 added | IShrinkageSystem and ICounterUtilities injection verification in SourceEntrySystem; 134→136 total, 126→128 active
- [fix] Phase3-Maintainability iter3: Interface Snippets | Fixed HandleCounterSource signature from (int targetCharIndex, int loopChr) to (CharacterId target, int arg1) per F803's actual implementation
- [fix] Phase3-Maintainability iter3: AC#111 rationale | Updated to reference F803's existing signature instead of Interface Snippets forward-compatible design
- [fix] Phase3-Maintainability iter3: Philosophy Derivation | Added AC#127, AC#128 to "SSOT for counter event orchestration" row
- [fix] Phase2-Review iter4: AC#129 added | Cross-character ejaculation tracking test (C6 constraint); closes gap between C6 requirement and existing test-existence ACs; 136→137 total, 128→129 active
- [fix] Phase3-Maintainability iter5: AC#28/AC#29 pattern | Fixed ripgrep alternation: `(Get\|Set)` → `(Get|Set)` — `\|` is literal pipe in ripgrep, not alternation
- [fix] Phase3-Maintainability iter5: Key Decisions | Added @SOURCE_CHECK sub-functions justification (keep as private methods in SourceEntrySystem)
- [fix] Phase3-Maintainability iter5: Key Decisions | Added ISourceSystem external API scope (minimal: ChastityBeltCheck only)
- [resolved-applied] Phase1-CodebaseDrift iter1: [BASELINE-STALE] Baseline Measurement "Existing C# Counter files = 29" is stale; Era.Core/Counter/ now contains ~41 .cs files after F802/F803/F810 completions
- [resolved-applied] Phase1-CodebaseDrift iter1: [METHOD-MISMATCH] AC#113 declares ICombinationCounter.ProcessCombination() but F802 [DONE] CounterCombination uses AccumulateCombinations(). Interface method name must align with F802's existing implementation.
- [resolved-applied] Phase1-CodebaseDrift iter1: [METHOD-MISMATCH] AC#114 declares IWcCombinationCounter.ProcessWcCombination() but F802 [DONE] WcCounterCombination may use different method name. Signature must align with F802's existing implementation.
- [resolved-applied] Phase1-CodebaseDrift iter1: [HANDOFF-STALE] Mandatory Handoffs assign ICombinationCounter→F802 and IWcCombinationCounter→F802 but F802 is now [DONE]. Handoff destinations need redirect (likely F813 Post-Phase Review).
- [fix] PostLoop-UserFix iter10: HANDOFF-STALE resolved | ICombinationCounter and IWcCombinationCounter stub replacement destinations changed from F802 [DONE] to F813 (Post-Phase Review DI adapter registration)
- [resolved-applied] Phase1-CodebaseDrift iter1: [RATIONALE-STALE] AC#22 rationale says "stub for F802" but F802 is [DONE] with concrete implementation. Should read "abstraction interface over F802's existing CounterCombination for DI consumption".
- [info] Phase1-DriftChecked: F802 (Related)
- [info] Phase1-DriftChecked: F803 (Related)
- [info] Phase1-DriftChecked: F810 (Related)
- [fix] Phase2-Review iter1: Implementation Contract Phase 4 AC count | Fixed 128→129 active (137 total - 8 N/A = 129)
- [fix] Phase2-Review iter1: AC#33 matcher | Tightened from `Count.*==.*16|16.*handler|handlers\.Count` to `Assert\.\w+\(16|\.Count.*==.*16` — requires Assert context, prevents false-pass from comments
- [fix] Phase2-Review iter2: AC#130 added | TouchStateManager class-existence AC (mirrors AC#23/AC#80 pattern); prevents false-pass from interface-only ACs; updated Task 10, Goal #9, AC Coverage, Implementation Contract (137→138 total, 129→130 active)
- [fix] Phase2-Review iter3: Task 17→14c test ACs | Moved AC#89, AC#97, AC#110 from Task 17 to Task 14c (TDD RED→GREEN: test creation belongs in Phase 3, not Phase 2); Task 17 now class-only; updated Implementation Contract Phase 2 output
- [fix] Phase2-Review iter4: AC#113/AC#114 method names | Changed ProcessCombination→AccumulateCombinations and ProcessWcCombination→AccumulateCombinations to match F802's existing implementation; updated Interface Snippets, AC Details, AC Coverage
- [fix] Phase2-Review iter4: AC#22 rationale | Changed "stub for F802" to "abstraction interface over F802's existing CounterCombination for DI injection" (F802 is [DONE])
- [fix] Phase2-Review iter5: Baseline Measurement | Updated Counter file count from 29 to 41 (verified via Glob); resolved [pending] Phase1-CodebaseDrift BASELINE-STALE
- [fix] Phase2-Review iter6: Philosophy Derivation 'equivalence-tested' | Added AC#89, AC#93, AC#129 to match Goal Coverage #6 (SSOT consistency)
- [fix] Phase2-Review iter7: AC#131 added | @SOURCE_CHECK sub-function equivalence test names (ChkWcEquip/ChkInsRotorV/ChkInsRotorA/ChkSemenBath ~427 lines); closes Philosophy "equivalence-tested" gap for AC#122-133 existence-only ACs; updated Task 14c, Goal #6, Philosophy Derivation, AC Coverage, Implementation Contract (138→140 total, 130→132 active)
- [fix] Phase2-Review iter7: AC#41/49/51/103 pattern extended | Added ICounterSourceHandler/IWcCounterSourceHandler/ICombinationCounter/IWcCombinationCounter to not_matches patterns; sibling-dispatch stubs are SourceEntrySystem-only orchestration contracts; root cause fix for all 4 enforcement ACs (not just AC#41)
- [fix] Phase2-Review iter7: AC#132 added | COM dispatch TFLAG:50→handler behavioral mapping test (parameterized InlineData/Theory gte 3); AC#33/AC#67 verify count only; updated Task 14c, Goal #4, Philosophy Derivation, AC Coverage
- [fix] Phase2-Review iter8: Goal Coverage orphans | Added AC#126 to Goal #6 (equivalence tests); AC#127, AC#128 to Goal #10 (orchestration SSOT); resolves 3 orphaned ACs with no Philosophy traceability
- [fix] Phase2-Review iter9: AC#84 threshold raised | Changed gte 2 to gte 5 (matching AC#87 pattern for 510-line scope; SOURCE.ERB is 1437 lines); updated AC Details, AC Coverage
- [fix] Phase2-Review iter9: AC#133 added | ChkWcEquip equivalence test assertion density (gte 3); prevents single-test false-pass for 306-line sub-function; follows AC#93 pattern; updated Task 14c, Goal #6, Philosophy Derivation, AC Coverage, Implementation Contract (140→141 total, 132→133 active)
- [fix] Phase2-Review iter10: AC#3 scope broadened | Changed from Grep(Era.Core/Counter/) to Grep(Era.Core/, glob="*.cs") — covers F811-created files in Interfaces/ (IUpVariables.cs) and Utilities/ (BitfieldUtility.cs) per Tasks 1, 2, 5; updated AC Details, AC Coverage
- [fix] Phase2-Review iter10: IWcCombinationCounter snippet added | Added explicit Interface Snippet for IWcCombinationCounter (mirrors ICombinationCounter with int[] AccumulateCombinations()); resolves implicit "mirrors" pattern in AC#114 rationale
- [resolved-applied] Phase3-Maintainability iter10: [SSOT-SPLIT] MasterPose canonical implementation split across 3 interfaces (ICounterUtilities.MasterPose, IComableUtilities.MasterPose, ITouchStateManager.MasterPose). F813 [DRAFT] consolidation task exists but has no concrete Task# for MasterPose SSOT unification. Risk: 3 parallel implementations may diverge during /run.
- [fix] PostLoop-UserFix iter10: MasterPose SSOT → F813 Task#1 | Added concrete Task#1 to F813 for MasterPose SSOT unification (3 interfaces → 1 canonical). Also added Tasks 2-6 for other F811 handoff items (DI adapters, ITouchSet migration, IShrinkageSystem runtime, baseline update)
- [fix] Phase3-Maintainability iter10: Key Decisions DI analysis | Added concrete DI count analysis (8/25 dependencies) to @SOURCE_CHECK sub-functions entry; documents why extraction to helper class rejected based on C16 cap
- [fix] Phase3-Maintainability iter10: Mandatory Handoffs parameter mapping | Added concrete ITouchSet→ITouchStateManager.TouchSet parameter mapping (mode→targetPart, type→masterPart, target.Value→character, reset=false)
- [fix] Phase4-ACLint iter10: AC Details blocks added | AC#126-129 (renumbered from 134-137) missing Details blocks; added rationale for WC routing predicate, IShrinkageSystem injection, ICounterUtilities injection, cross-character ejaculation test
- [fix] Phase4-ACLint iter10: AC renumbering | Closed 8 numbering gaps (16,17,20-23,33,82) via ac-renumber tool; 141→133 contiguous numbering; all cross-references updated
- [fix] Phase4-ACLint iter10: Status column restoration | ac-renumber tool stripped Status column from AC Definition Table; restored [ ] for active ACs via script
- [fix] Phase4-ACValidation iter10: AC type correction | 7 ACs (110,115,126,129,131,132,133) changed from type 'test' to 'code'; Grep-based static content checks use 'code' type per SSOT (test type = dotnet test execution)
- [fix] Phase2-Review iter1: Tasks table Status column | Added missing Status column (| [x] | for active rows, | - | for struck-through) per template requirement
- [fix] Phase2-Review iter1: AC Details struck-through entries | Removed 5 obsolete struck-through AC Detail blocks (~~AC#17 old~~, ~~AC#20~~, ~~AC#21~~, ~~AC#22~~, ~~AC#23~~) from AC Details section
- [fix] Phase2-Review iter1: Task Tags Phase 4 column | Added missing Phase 4 column to Task Tags table per template requirement
- [fix] Phase2-Review iter2: Feasibility Assessment AC refs | Updated stale AC cross-references: UP AC#18/19/45→AC#16/17/38, SETBIT AC#24→AC#18, CSTR-write AC#53→AC#46, EXP_UP AC#54→AC#47
- [fix] Phase3-Maintainability iter3: AC#3 scope | Narrowed from Era.Core/ to Era.Core/Counter/Source/ to match C1 Constraint Details; pre-existing TODO in OrgasmProcessor.cs caused false-fail risk
- [fix] Phase3-Maintainability iter3: ICounterSourceHandler ISP note | Added note to Approach item 6: F803's ICounterSourceHandler ISP violation tracked in F813 Deferred Obligations #3
- [fix] Phase3-Maintainability iter3: MasterPose divergence risk | Added risk row for 3 parallel MasterPose implementations during interim period before F813 consolidation
- [fix] Phase3-Maintainability iter3: COUNTER_CONBINATION constraint | Struck through stale constraint (Resolved by F802 [DONE])
- [fix] Phase2-Review iter4: AC#47 rationale | Removed stale AC#54 cross-reference (AC#54 is INtrUtilityService, not CheckExpUp verifier)
- [fix] Phase2-Review iter4: Technical Design item 5 EXP_UP | Changed stale 'inline calc EXP[i]-TCVAR[400+i]' to 'ICounterUtilities.CheckExpUp per DEP-DRIFT resolution' to match AC#47
- [fix] Phase2-Review iter5: AC Details stale cross-refs | Fixed 6 stale AC# references in AC#41/42/43/44 rationale texts (AC#48→41, AC#49→42, AC#51→44, AC#25→19)
- [fix] Phase2-Review iter5: AC#3 scope expanded | Changed from Counter/Source/ to Counter/ to cover stub interface files (IComHandler.cs, etc.) per zero-debt constraint
- [fix] Phase2-Review iter3: AC#106 path scope | Tightened from directory-wide Era.Core/Counter/Source/ to SourceEntrySystem.cs (file-scoped dispatch call verification)
- [fix] Phase2-Uncertain iter3: AC#134/135 added | IShrinkageSystem and ICounterUtilities interface existence guards (defense-in-depth for F803-provided interfaces); updated Task 9, Goal #10, AC Coverage (133→135 total)
- [fix] Phase2-Review iter4: AC#136 added | ShootingSystem ICharacterStringVariables DI injection verification (matches AC#39/AC#64 pattern); updated Task 12, Goal #5, AC Coverage (135→137 total)
- [fix] Phase2-Review iter4: AC#137 added | ISourceSystem SSOT enforcement (single implementer lte 1); enforces Philosophy "SSOT for counter event orchestration"; updated Task 9, Goal #10, AC Coverage
- [fix] Phase2-Review iter5: Goal #2 stale framing | Corrected F802/F803 from "not-yet-[DONE]" to "consume existing"; moved AC#20/AC#111 from Goal #2 to Goal #10 (predecessor interface consumption); updated AC#20 title/rationale from "stub" to "F803-provided consumed by F811"
- [fix] Phase2-Review iter6: Task#9 EXP_UP stale text | Changed "inline EXP[i]-TCVAR[400+i] formula" to "delegate EXP_UP dual-use to ICounterUtilities.CheckExpUp" per DEP-DRIFT resolution
- [fix] Phase2-Review iter6: Mandatory Handoffs tracking note | Added "Tracking verified: F813 Task#1, F803 deferred obligations" to ITouchSet→ITouchStateManager migration entry
- [fix] Phase3-Maintainability iter7: AC#73 rationale stale ref | Changed AC#46→AC#39 (ITouchStateManager interface presence)
- [fix] Phase3-Maintainability iter7: AC#65/69/70/71 stale AC#33 refs | Replaced stale AC#33 references with "former sibling-dispatch OR-pattern AC (superseded)" across 4 AC rationale texts
- [fix] Phase2-Review iter1: Task ~~3~~/~~4~~ column count | Added missing Tag column to struck-through Task rows (4→5 columns per template)
- [fix] Phase2-Review iter1: AC Coverage struck-through rows | Removed 8 orphan struck-through rows (~~16~~, ~~17~~, ~~20~~, ~~21~~, ~~22~~, ~~23~~, ~~33~~, ~~82~~) from AC Coverage table
- [fix] Phase2-Review iter1: AC Coverage column consistency | Removed trailing Task# column from AC#117-133 rows (3→2 columns per header)
- [fix] Phase2-Review iter1: AC#106 Task reassignment | Moved AC#106 from Task 17 (SourceDisplayHandler) to Task 10 (TouchStateManager implements ShowTouch)
- [fix] Phase2-Review iter1: AC#43 rationale | Corrected cross-reference from AC#50 to AC#7→AC#19 pattern (IWcCounterSystem → SelectAction)
- [fix] Phase3-Maintainability iter2: AC Coverage How to Satisfy | Populated 115 empty "How to Satisfy" cells (AC#1-115) with brief descriptions derived from AC Details

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning
- [Predecessor: F801](feature-801.md) - Main Counter Core
- [Predecessor: F812](feature-812.md) - SOURCE1 Extended
- [Related: F802](feature-802.md) - Main Counter Output
- [Related: F803](feature-803.md) - Main Counter Source
- [Related: F804](feature-804.md) - WC Counter Core
- [Related: F805](feature-805.md) - WC Counter Source
- [Related: F809](feature-809.md) - COMABLE Core
- [Related: F810](feature-810.md) - COMABLE Extended
- [Successor: F813](feature-813.md) - Post-Phase Review Phase 21
