# Feature 801: Main Counter Core

## Status: [DONE]
<!-- fl-reviewed: 2026-02-23T02:48:40Z -->

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

## Summary

C# migration of Main Counter core logic: ActionSelector.cs (COUNTER_SELECT.ERB), ActionValidator.cs (COUNTER_ACTABLE.ERB), ICounterSystem/IActionSelector/IActionValidator/ICounterUtilities interfaces, CounterActionId enum, plus additive extensions to IVariableStore (GetEquip/SetEquip) and IEngineVariables (GetTarget/SetTarget indexed, GetSelectCom).

<!-- fc-phase-1-completed -->

---

## Background

### Philosophy (Mid-term Vision)

Phase 21: Counter System -- Main Counter Core is the SSOT for counter action selection and validation logic. COUNTER_SELECT.ERB (probabilistic weighted action selection) and COUNTER_ACTABLE.ERB (pure action validation) are migrated to C# as ActionSelector.cs and ActionValidator.cs under Era.Core/Counter/, behind the architecture-mandated ICounterSystem interface. All external dependencies are injected via interfaces to maintain testability and phase isolation.

### Problem (Current Issue)

F801 is a skeletal DRAFT stub created by the Phase 21 planning feature (F783) with only 2 generic tasks and 1 AC, because F783 focused on decomposition correctness rather than implementation readiness. The ICounterSystem interface defined in the architecture spec (`phase-20-27-game-systems.md:120-125`) presumes a `GetAvailableActions` list-returning API, but the actual ERB logic in COUNTER_SELECT.ERB performs probabilistic weighted single-action selection (lines 241-259: randomize weights, filter by ACTABLE, select max, write to TCVAR). Additionally, two critical interface gaps block compilation: IVariableStore has no GetEquip accessor (`IVariableStore.cs` -- EQUIP marked "unused" in engine at `VariableCode.cs:223` despite active ERB usage at `COUNTER_ACTABLE.ERB:25,30`), and IEngineVariables exposes only scalar TARGET (`IEngineVariables.cs:69-73`) but COUNTER_ACTABLE.ERB:10-13 requires indexed TARGET array access (`TARGET:LOCAL`).

### Goal (What to Achieve)

Migrate COUNTER_SELECT.ERB (260 lines) and COUNTER_ACTABLE.ERB (341 lines) to C# as ActionSelector.cs and ActionValidator.cs under Era.Core/Counter/. Resolve the ICounterSystem interface to match actual ERB behavior (single-action probabilistic selection, not list-available). Add missing GetEquip to IVariableStore, add indexed GetTarget(int) to IEngineVariables, and create a CounterActionId enum for the ~52 CNT_ constants from DIM.ERH:323-374. All external utility function dependencies (6 functions from other phases) must be abstracted behind injectable interfaces. Create ICounterOutputHandler stub interface for F802's うふふ path output delegation (REACTION/PUNISHMENT). Equivalence tests must verify C# output matches legacy ERB behavior.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is F801 not implementation-ready? | DRAFT has only 2 stub tasks and 1 generic AC; no architecture or interface analysis was performed | `feature-801.md:55-66` |
| 2 | Why was no detailed analysis performed? | F783 created all Phase 21 DRAFTs from a minimal template; detailed AC/Task generation deferred to /fc | `feature-783.md:440-449` |
| 3 | Why did F783 use minimal templates? | F783 focused on decomposition correctness (file grouping, interface ownership) not implementation readiness | `feature-783.md:384-386` |
| 4 | Why does the architecture not capture implementation gaps? | Phase 21 architecture spec defines ICounterSystem at interface level only (2 methods), not implementation-level dependency analysis | `phase-20-27-game-systems.md:120-125` |
| 5 | Why (Root)? | The ICounterSystem interface was designed abstractly without mapping to actual ERB control flow; the ERB requires direct EQUIP array access (`COUNTER_ACTABLE.ERB:25,30`), indexed TARGET arrays (`COUNTER_ACTABLE.ERB:10-13`), and 6+ external utility functions that have no C# interfaces, and the ERB performs probabilistic single-action selection rather than the list-available pattern the interface assumes | `IVariableStore.cs` (no GetEquip), `IEngineVariables.cs:69-73` (scalar TARGET only), `COUNTER_SELECT.ERB:241-259` (weighted random select-one) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | F801 DRAFT has 2 stub tasks and 1 AC with no implementation analysis | ICounterSystem interface designed abstractly without ERB behavior mapping; EQUIP/TARGET interface gaps not surfaced during planning |
| Where | `feature-801.md` Tasks and AC sections | `phase-20-27-game-systems.md:120-125` (interface design), `IVariableStore.cs` (missing GetEquip), `IEngineVariables.cs` (missing indexed GetTarget) |
| Fix | Add more tasks and ACs manually | Reconcile ICounterSystem with actual ERB control flow; add missing variable accessors; create CounterActionId enum; abstract external utilities behind injectable interfaces |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F783 | [DONE] | Predecessor -- Phase 21 Planning (parent decomposition) |
| F802 | [DRAFT] | Sibling -- Main Counter Output (F801 CALLs EVENT_COUNTER_REACTION at COUNTER_SELECT.ERB:52 and EVENT_COUNTER_PUNISHMENT at COUNTER_SELECT.ERB:58; stubable via interface) |
| F803 | [DRAFT] | Sibling -- Main Counter Source |
| F809 | [DRAFT] | Sibling -- COMABLE Core (IComAvailabilityChecker owner) |
| F811 | [DRAFT] | Sibling -- SOURCE Entry System (SOURCE.ERB:37 dispatches to EVENT_COUNTER; F811 depends on F801) |
| F813 | [DRAFT] | Successor -- Post-Phase Review Phase 21 |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Source files identifiable and accessible | FEASIBLE | Both COUNTER_SELECT.ERB (260 lines) and COUNTER_ACTABLE.ERB (341 lines) are well-structured with Japanese comments |
| Architecture design exists | FEASIBLE | ICounterSystem mandated at `phase-20-27-game-systems.md:120-125`; ActionSelector.cs and ActionValidator.cs deliverables at lines 208-209 |
| Core logic self-contained for validation | FEASIBLE | ACTABLE is pure function (RETURNF 0/1) with no side effects -- excellent for TDD |
| Variable interfaces mostly exist | FEASIBLE | IVariableStore covers FLAG, TFLAG, CFLAG, ABL, TALENT, PALAM, BASE, TCVAR, STAIN, MAXBASE, PALAMLV; gaps (EQUIP, TARGET indexed) are additive-only changes |
| External utility dependencies abstractable | FEASIBLE | 6 utility functions (IS_AIR_MASTER, COM417_is_Protected_CheckOnly, CHK_COM312_STAIN, is_virgin_m, ONCE, HAS_PENIS/HAS_VAGINA) have clear signatures and can be injected |
| IClothingSystem available for cross-phase calls | FEASIBLE | IClothingSystem.SettingTrain and PresetNude already exist (`IClothingSystem.cs:53-56`) |
| Scope within volume limits | FEASIBLE | 601 total lines within erb type limit (~500 guideline, acceptable with validation purity) |
| Predecessor satisfied | FEASIBLE | F783 is [DONE] |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| IVariableStore interface | MEDIUM | Adding GetEquip/SetEquip is additive; concrete implementations (VariableStore.cs) must add stub methods to compile |
| IEngineVariables interface | MEDIUM | Adding GetTarget(int index) overload is additive; existing scalar GetTarget() preserved |
| Era.Core/Counter/ namespace | HIGH | New namespace with ActionSelector.cs, ActionValidator.cs, ICounterSystem.cs, CounterActionId enum |
| Era.Core.Tests/Counter/ | HIGH | New test namespace with equivalence tests for all ACTABLE branches and SELECT weight logic |
| F811 (SOURCE Entry System) | MEDIUM | F811 depends on F801's ICounterSystem interface being stable; interface changes after F801 completion would require F811 updates |
| F802 (Main Counter Output) | LOW | F801 calls F802 at boundary points only (early-exit paths); stubbed via interface |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| No EQUIP getter in IVariableStore | `IVariableStore.cs` -- absent; `VariableCode.cs:223` marks EQUIP as "unused" | Must add GetEquip/SetEquip for character-scoped 1D EQUIP array access |
| No indexed TARGET array accessor | `IEngineVariables.cs:69-73` -- only scalar TARGET:0 exposed | Must add GetTarget(int index) overload for TARGET:LOCAL pattern in ACTABLE:10-13 |
| ICounterSystem returns list but ERB selects one | `phase-20-27-game-systems.md:123` vs `COUNTER_SELECT.ERB:248-259` | Need to reconcile interface API with actual probabilistic single-select behavior |
| ICommonFunctions takes genderValue not characterId | `ICommonFunctions.cs:10-11` | Caller must resolve TALENT:gender first, then pass value to HasPenis/HasVagina |
| 6 external utility functions have no C# equivalent | COMF417, COMF312, memorial_base, NTR_UTIL, COMMON (ONCE) | Must define injectable interface abstractions for cross-phase utility calls |
| 52 CNT_ constants in DIM.ERH only | `DIM.ERH:323-374` (range 10-91) | Must create C# CounterActionId enum preserving exact numeric values |
| Strategy Pattern mandated | `phase-20-27-game-systems.md:138-139` | ActionSelector must follow IActionSelector pattern |
| TreatWarningsAsErrors=true | `Directory.Build.props` | All new C# code must compile warning-free |
| SELECTCOM variable access | `COUNTER_SELECT.ERB:21` | Need SELECTCOM accessor for early-exit command guard (351/400/490) |
| DRAFT line counts inaccurate | `feature-801.md:35-36` states 202+397; actual is 260+341 | Corrected in this synthesis |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| EQUIP interface gap blocks compilation | HIGH | HIGH | Add GetEquip/SetEquip to IVariableStore as F801 Task |
| TARGET indexed accessor gap blocks ACTABLE migration | HIGH | HIGH | Add GetTarget(int index) to IEngineVariables as F801 Task |
| ICounterSystem API mismatch causes rework | MEDIUM | MEDIUM | Reconcile during /fc Phase 4 (tech-designer); adapt to single-select pattern |
| External utility stubs proliferate across features | MEDIUM | MEDIUM | Define consolidated ICounterUtilities interface grouping all 6 external functions |
| Phase 22 CLOTHES calls create cross-phase dependency | LOW | MEDIUM | Use existing IClothingSystem (SettingTrain/PresetNude already available) |
| HasPenis/HasVagina parameter mismatch | MEDIUM | LOW | Resolve gender TALENT first, then pass value -- standard adapter pattern |
| ICounterSystem.ExecuteAction may exceed F801 scope | MEDIUM | MEDIUM | Scope F801 to selection+validation only; execution is F802/F803 concern |
| Probabilistic selection logic hard to test deterministically | MEDIUM | MEDIUM | Inject IRandomProvider; seed-controlled tests for weight calculation |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| COUNTER_SELECT.ERB line count | `wc -l Game/ERB/COUNTER_SELECT.ERB` | 260 | Corrected from DRAFT's 202 |
| COUNTER_ACTABLE.ERB line count | `wc -l Game/ERB/COUNTER_ACTABLE.ERB` | 341 | Corrected from DRAFT's 397 |
| Era.Core/Counter/ files | `ls Era.Core/Counter/ 2>/dev/null` | 0 (directory does not exist) | Greenfield namespace |
| IVariableStore GetEquip | `grep -c GetEquip Era.Core/Interfaces/IVariableStore.cs` | 0 | Missing -- must be added |
| IEngineVariables GetTarget overloads | `grep -c GetTarget Era.Core/Interfaces/IEngineVariables.cs` | 2 (scalar get+set) | Indexed overload missing |

**Baseline File**: `.tmp/baseline-801.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | ICounterSystem interface must be created | `phase-20-27-game-systems.md:120-125` | Verify ICounterSystem.cs exists in Era.Core/Counter/ |
| C2 | ACTABLE is pure function (no side effects) | `COUNTER_ACTABLE.ERB:4` (#FUNCTION) | ActionValidator can be tested with direct input/output assertions; verify returns bool without state modification |
| C3 | All 52 CNT_ action types must have C# equivalents | `DIM.ERH:323-374` | Verify CounterActionId enum covers all constants with matching numeric values |
| C4 | EQUIP accessor must exist in IVariableStore | Missing in `IVariableStore.cs` | Verify GetEquip/SetEquip methods exist |
| C5 | Indexed TARGET accessor must exist in IEngineVariables | Missing in `IEngineVariables.cs:69-73` | Verify GetTarget(int index) overload exists |
| C6 | Equivalence tests for all SELECTCASE branches in ACTABLE | `COUNTER_ACTABLE.ERB:16-339` (exactly 36 CASE branches) | Verify test coverage for each action validation branch |
| C7 | Zero technical debt | `phase-20-27-game-systems.md:237-239` | Verify no TODO/FIXME/HACK in migrated files |
| C8 | All utility dependencies must be injectable | DI pattern per architecture | Verify constructor injection for all external dependencies |
| C9 | SELECTCOM early-exit must be preserved | `COUNTER_SELECT.ERB:21-24` (commands 351/400/490) | Verify early return for specific command values |
| C10 | Double-execution guard must be preserved | `COUNTER_SELECT.ERB:44-46` (counter action decision flag) | Verify guard logic prevents duplicate execution |
| C11 | Weighted random selection reproduces ERB behavior | `COUNTER_SELECT.ERB:69-259` (4 personality components) | Verify weight calculation formulas match ERB |
| C12 | Bitwise STAIN operations correctly translated | `COUNTER_ACTABLE.ERB:182` | Verify bitwise equivalence |
| C13 | SELECTCOM accessor available | Interface Dependency Scan | Verify mechanism to read SELECTCOM value exists |
| C14 | TCVarIndex named constants for type safety | `TCVarIndex.cs` typed struct pattern | Verify CounterAction=20 and CounterDecisionFlag=30 constants exist in TCVarIndex.cs |

### Constraint Details

**C1: ICounterSystem Interface**
- **Source**: Architecture spec `phase-20-27-game-systems.md:120-125`
- **Verification**: `ls Era.Core/Counter/ICounterSystem.cs`
- **AC Impact**: Interface must exist but API signature should be reconciled with actual ERB behavior (single-select not list-available)

**C2: ACTABLE Pure Function**
- **Source**: `COUNTER_ACTABLE.ERB:4` declares `#FUNCTION` (RETURNF 0/1)
- **Verification**: Read ACTABLE source -- no variable writes, only RETURNF
- **AC Impact**: ActionValidator tests can use direct assertions without mocking state changes

**C3: CNT_ Constants Enum**
- **Source**: `DIM.ERH:323-374` defines 52 constants (range 10-91)
- **Verification**: `grep CNT_ Game/ERB/DIM.ERH | wc -l`
- **AC Impact**: CounterActionId enum must preserve exact numeric values for ERB equivalence

**C4: EQUIP Variable Accessor**
- **Source**: EQUIP used at `COUNTER_ACTABLE.ERB:25,30,86,91` but IVariableStore has no GetEquip
- **Verification**: `grep GetEquip Era.Core/Interfaces/IVariableStore.cs` returns 0 matches
- **AC Impact**: Must verify GetEquip(CharacterId, int index) and SetEquip exist after implementation

**C5: Indexed TARGET Accessor**
- **Source**: `COUNTER_ACTABLE.ERB:10-13` uses TARGET:LOCAL pattern (loop over characters)
- **Verification**: `grep 'GetTarget(int' Era.Core/Interfaces/IEngineVariables.cs` returns 0 matches
- **AC Impact**: Must verify GetTarget(int index) overload exists alongside scalar GetTarget()

**C6: ACTABLE Branch Coverage**
- **Source**: `COUNTER_ACTABLE.ERB:16-339` has exactly 36 SELECTCASE branches
- **Verification**: Count CASE statements in ACTABLE source
- **AC Impact**: Each branch must have at least one equivalence test

**C7: Zero Technical Debt**
- **Source**: `phase-20-27-game-systems.md:237-239` mandates debt-free migration
- **Verification**: Grep for TODO/FIXME/HACK in Era.Core/Counter/
- **AC Impact**: not_matches pattern on all migrated files

**C8: Injectable Dependencies**
- **Source**: Architecture mandates DI pattern
- **Verification**: Constructor parameters use interface types
- **AC Impact**: Verify all external calls go through injected interfaces, no static/direct coupling

**C9: SELECTCOM Early-Exit**
- **Source**: `COUNTER_SELECT.ERB:21-24` guards against commands 351, 400, 490
- **Verification**: Read SELECT source lines 21-24
- **AC Impact**: Verify ActionSelector returns early (no action) for these command values

**C10: Double-Execution Guard**
- **Source**: `COUNTER_SELECT.ERB:44-46` increments and checks flag
- **Verification**: Read SELECT source lines 44-46
- **AC Impact**: Verify guard prevents duplicate counter action selection per turn

**C11: Weighted Random Selection**
- **Source**: `COUNTER_SELECT.ERB:69-259` -- 4 personality harassment components
- **Verification**: Trace weight formulas in SELECT source
- **AC Impact**: Verify weight calculation with deterministic random seed produces expected action

**C12: Bitwise STAIN Operations**
- **Source**: `COUNTER_ACTABLE.ERB:182` uses bitwise operations on STAIN
- **Verification**: Read ACTABLE line 182
- **AC Impact**: Verify C# bitwise translation matches ERB behavior exactly

**C13: SELECTCOM Accessor**
- **Source**: Interface Dependency Scan (Explorer 3); `COUNTER_SELECT.ERB:21`
- **Verification**: `grep SELECTCOM Era.Core/Interfaces/*.cs` returns 0 matches
- **AC Impact**: Mechanism to access SELECTCOM value must be provided (new interface method or existing workaround)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| Successor | F811 | [DRAFT] | SOURCE Entry System -- SOURCE.ERB:37 CALLs EVENT_COUNTER; F811 depends on F801's ICounterSystem interface (CALL at SOURCE.ERB:37) |
| Successor | F813 | [DRAFT] | Post-Phase Review Phase 21 |
| Related | F802 | [DRAFT] | Main Counter Output -- F801 CALLs EVENT_COUNTER_REACTION (COUNTER_SELECT.ERB:52) and EVENT_COUNTER_PUNISHMENT (COUNTER_SELECT.ERB:58); side-effect calls at early-exit boundary, stubable via interface |
| Related | F803 | [DRAFT] | Main Counter Source |
| Related | F809 | [DRAFT] | COMABLE Core (IComAvailabilityChecker owner) |

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

<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "SSOT for counter action selection and validation logic" | ActionSelector.cs and ActionValidator.cs must exist as the C# implementations | AC#1, AC#2 |
| "ICounterSystem interface" | ICounterSystem.cs must exist in Era.Core/Counter/ | AC#3 |
| "All external dependencies are injected via interfaces" | Constructor injection for all external dependencies; no static/direct coupling | AC#8, AC#9 |
| "All external dependencies are injected via interfaces" | うふふ path REACTION/PUNISHMENT calls delegated to ICounterOutputHandler | AC#35, AC#36, AC#43 |
| "COUNTER_SELECT.ERB ... migrated to C# as ActionSelector.cs" | ActionSelector must reproduce weighted probabilistic single-action selection | AC#11, AC#12, AC#18 |
| "COUNTER_ACTABLE.ERB ... migrated to C# as ActionValidator.cs" | ActionValidator must reproduce all 36 SELECTCASE branch validation logic | AC#13, AC#14, AC#19 |
| "CounterActionId enum for the ~52 CNT_ constants" | Enum must cover all 52 CNT_ constants with exact numeric values | AC#5, AC#40, AC#41 |
| "Zero technical debt" | No TODO/FIXME/HACK in migrated files | AC#17 |
| "Equivalence tests must verify C# output matches legacy ERB behavior" | Unit tests for ActionValidator branches and ActionSelector weight logic | AC#18, AC#19 |
| "Add missing GetEquip to IVariableStore" | IVariableStore must have GetEquip/SetEquip methods | AC#6 |
| "Add indexed GetTarget(int) to IEngineVariables" | IEngineVariables must have GetTarget(int index) overload | AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ActionSelector.cs exists | file | Glob | exists | Era.Core/Counter/ActionSelector.cs | [ ] |
| 2 | ActionValidator.cs exists | file | Glob | exists | Era.Core/Counter/ActionValidator.cs | [ ] |
| 3 | ICounterSystem.cs exists | file | Glob | exists | Era.Core/Counter/ICounterSystem.cs | [ ] |
| 4 | IActionSelector.cs exists (Strategy Pattern) | file | Glob | exists | Era.Core/Counter/IActionSelector.cs | [ ] |
| 5 | CounterActionId enum exists with 52 members | code | Grep(Era.Core/Counter/CounterActionId.cs) | count_equals | `= \d+` = 52 | [ ] |
| 6 | IVariableStore declares GetEquip with Result<int> return | code | Grep(Era.Core/Interfaces/IVariableStore.cs) | matches | Result<int> GetEquip\(CharacterId | [ ] |
| 7 | IEngineVariables declares GetTarget(int) overload | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | int GetTarget\(int | [ ] |
| 8 | ActionValidator uses constructor injection | code | Grep(Era.Core/Counter/ActionValidator.cs) | matches | public (sealed )?class ActionValidator\(\|public ActionValidator\( | [ ] |
| 9 | ActionSelector uses constructor injection | code | Grep(Era.Core/Counter/ActionSelector.cs) | matches | public (sealed )?class ActionSelector\(\|public ActionSelector\( | [ ] |
| 10 | IVariableStore existing methods preserved | code | Grep(Era.Core/Interfaces/IVariableStore.cs) | gte | `(Get\|Set)(Flag\|TFlag\|CharacterFlag\|Ability\|Talent\|Palam\|Exp\|Base\|TCVar\|Source\|Mark\|NowEx\|MaxBase\|Cup\|Juel\|GotJuel\|PalamLv\|Stain\|Downbase)\(` = 38 | [ ] |
| 11 | ActionSelector injects IRandomProvider | code | Grep(Era.Core/Counter/ActionSelector.cs) | matches | IRandomProvider | [ ] |
| 12 | ActionSelector returns single action (not list) | code | Grep(Era.Core/Counter/ICounterSystem.cs) | matches | CounterActionId\?\s+SelectAction | [ ] |
| 13 | ActionValidator returns bool | code | Grep(Era.Core/Counter/ActionValidator.cs) | matches | bool\s+(IsActable\|Validate) | [ ] |
| 14 | ActionValidator handles 36 CASE branches | code | Grep(Era.Core/Counter/ActionValidator.cs) | count_equals | `CounterActionId\.\w+ =>` = 36 | [ ] |
| 15 | IEngineVariables existing methods preserved (incl. new Target overloads) | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | count_equals | `(int\|void\|string)\s+(Get\|Set)(Result\|Money\|Day\|Master\|Assi\|Count\|CharaNum\|Random\|Name\|CallName\|IsAssi\|CharacterNo\|Target\|Player)\(` = 23 | [ ] |
| 16 | IVariableStore declares SetEquip | code | Grep(Era.Core/Interfaces/IVariableStore.cs) | matches | SetEquip\(CharacterId | [ ] |
| 17 | Zero technical debt in Counter namespace | code | Grep(Era.Core/Counter/) | not_matches | TODO|FIXME|HACK | [ ] |
| 18 | ActionSelector equivalence tests exist | code | Grep(Era.Core.Tests/) | matches | ActionSelector.*Test|Test.*ActionSelector | [ ] |
| 19 | ActionValidator equivalence tests exist | code | Grep(Era.Core.Tests/) | matches | ActionValidator.*Test|Test.*ActionValidator | [ ] |
| 20 | ActionValidator injects ITEquipVariables | code | Grep(Era.Core/Counter/ActionValidator.cs) | matches | ITEquipVariables | [ ] |
| 21 | ActionValidator injects ICommonFunctions | code | Grep(Era.Core/Counter/ActionValidator.cs) | matches | ICommonFunctions | [ ] |
| 22 | Unit tests pass | test | dotnet test Era.Core.Tests | succeeds | - | [ ] |
| 23 | IEngineVariables declares SetTarget(int index) overload | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | void SetTarget\(int index | [ ] |
| 24 | ICounterUtilities.cs exists | file | Glob | exists | Era.Core/Counter/ICounterUtilities.cs | [ ] |
| 25 | IEngineVariables declares GetSelectCom | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | GetSelectCom | [ ] |
| 26 | IActionValidator.cs exists | file | Glob | exists | Era.Core/Counter/IActionValidator.cs | [ ] |
| 27 | ActionValidatorTests covers all 36 branches | code | Grep(Era.Core.Tests/Counter/ActionValidatorTests.cs) | gte | `\[Fact\]\|\[Theory\]` = 36 | [ ] |
| 28 | ActionSelectorTests uses SeededRandomProvider | code | Grep(Era.Core.Tests/Counter/ActionSelectorTests.cs) | matches | SeededRandomProvider\|SeededRandom | [ ] |
| 29 | ActionSelector implements ICounterSystem | code | Grep(Era.Core/Counter/ActionSelector.cs) | matches | IActionSelector.*ICounterSystem\|ICounterSystem.*IActionSelector | [ ] |
| 30 | ActionValidator default arm returns false | code | Grep(Era.Core/Counter/ActionValidator.cs) | matches | _ => false | [ ] |
| 31 | ActionSelector injects ITEquipVariables | code | Grep(Era.Core/Counter/ActionSelector.cs) | matches | ITEquipVariables | [ ] |
| 32 | ActionSelector writes TCVAR via CounterAction constant | code | Grep(Era.Core/Counter/ActionSelector.cs) | matches | SetTCVar.*CounterAction | [ ] |
| 33 | ActionValidator injects IEngineVariables | code | Grep(Era.Core/Counter/ActionValidator.cs) | matches | IEngineVariables | [ ] |
| 34 | IActionSelector declares SelectAction with correct return type | code | Grep(Era.Core/Counter/IActionSelector.cs) | matches | CounterActionId\?\s+SelectAction | [ ] |
| 35 | ActionSelector injects ICounterOutputHandler | code | Grep(Era.Core/Counter/ActionSelector.cs) | matches | ICounterOutputHandler | [ ] |
| 36 | ICounterOutputHandler.cs exists | file | Glob | exists | Era.Core/Counter/ICounterOutputHandler.cs | [ ] |
| 37 | ActionSelector injects IEngineVariables | code | Grep(Era.Core/Counter/ActionSelector.cs) | matches | IEngineVariables | [ ] |
| 38 | ICounterUtilities declares all 5 utility methods | code | Grep(Era.Core/Counter/ICounterUtilities.cs) | count_equals | `bool (IsAirMaster\|IsProtectedCheckOnly\|CheckStain\|IsVirginM\|IsOnce)\(` = 5 | [ ] |
| 39 | ActionSelector checks double-execution guard flag | code | Grep(Era.Core/Counter/ActionSelector.cs) | matches | カウンター行動決定\|CounterDecisionFlag | [ ] |
| 40 | CounterActionId first sentinel value | code | Grep(Era.Core/Counter/CounterActionId.cs) | matches | SeductiveGesture = 10 | [ ] |
| 41 | CounterActionId last sentinel value | code | Grep(Era.Core/Counter/CounterActionId.cs) | matches | ReverseRapeRide = 91 | [ ] |
| 42 | ActionSelectorTests covers sex-category weight zeroing | code | Grep(Era.Core.Tests/Counter/ActionSelectorTests.cs) | matches | MissionaryInsert\|CowgirlInsert\|SexCategory | [ ] |
| 43 | ActionSelector invokes HandleReaction and HandlePunishment | code | Grep(Era.Core/Counter/ActionSelector.cs) | matches | HandleReaction\|HandlePunishment | [ ] |
| 44 | ActionValidator uses single-pattern switch arms | code | Grep(Era.Core/Counter/ActionValidator.cs) | not_matches | or CounterActionId | [ ] |
| 45 | ActionSelector TCVAR write is null-guarded | code | Grep(Era.Core/Counter/ActionSelector.cs) | matches | if.*selectedAction.*!=.*null\|selectedAction is not null | [ ] |
| 46 | TCVarIndex declares CounterAction constant | code | Grep(Era.Core/Types/TCVarIndex.cs) | matches | CounterAction = new\(20\) | [ ] |
| 47 | TCVarIndex declares CounterDecisionFlag constant | code | Grep(Era.Core/Types/TCVarIndex.cs) | matches | CounterDecisionFlag = new\(30\) | [ ] |
| 48 | ICounterOutputHandler declares HandleReaction and HandlePunishment | code | Grep(Era.Core/Counter/ICounterOutputHandler.cs) | count_equals | `void Handle(Reaction\|Punishment)\(` = 2 | [ ] |

### AC Details

**AC#1: ActionSelector.cs exists**
- **Test**: Glob pattern="Era.Core/Counter/ActionSelector.cs"
- **Expected**: File exists
- **Rationale**: Core deliverable -- migrates COUNTER_SELECT.ERB (260 lines) probabilistic weighted single-action selection logic. Constraint C11.

**AC#2: ActionValidator.cs exists**
- **Test**: Glob pattern="Era.Core/Counter/ActionValidator.cs"
- **Expected**: File exists
- **Rationale**: Core deliverable -- migrates COUNTER_ACTABLE.ERB (341 lines) pure action validation logic. Constraint C2.

**AC#3: ICounterSystem.cs exists**
- **Test**: Glob pattern="Era.Core/Counter/ICounterSystem.cs"
- **Expected**: File exists
- **Rationale**: Architecture-mandated interface per phase-20-27-game-systems.md:120-125. Constraint C1. API must be reconciled to single-select pattern.

**AC#4: IActionSelector.cs exists (Strategy Pattern)**
- **Test**: Glob pattern="Era.Core/Counter/IActionSelector.cs"
- **Expected**: File exists
- **Rationale**: Strategy Pattern mandated by phase-20-27-game-systems.md:138-139. Constraint C16.

**AC#5: CounterActionId enum exists with 52 members**
- **Test**: Grep pattern=`= \d+` path="Era.Core/Counter/CounterActionId.cs" | count
- **Expected**: 52 matches (one per CNT_ constant from DIM.ERH:323-374)
- **Rationale**: All 52 CNT_ constants must have C# equivalents with exact numeric values for ERB equivalence. Constraint C3.

**AC#6: IVariableStore declares GetEquip with Result<int> return**
- **Test**: Grep pattern=`Result<int> GetEquip\(CharacterId` path="Era.Core/Interfaces/IVariableStore.cs"
- **Expected**: 1+ match
- **Rationale**: EQUIP array accessor missing from IVariableStore. Required for COUNTER_ACTABLE.ERB:25,30 (EQUIP:MASTER:9, EQUIP:MASTER:0). Constraint C4.

**AC#7: IEngineVariables declares GetTarget(int) overload**
- **Test**: Grep pattern=`int GetTarget\(int` path="Era.Core/Interfaces/IEngineVariables.cs"
- **Expected**: 1+ match
- **Rationale**: Indexed TARGET accessor required for COUNTER_ACTABLE.ERB:10-13 (TARGET:LOCAL loop). Currently only scalar GetTarget() exists. Constraint C5.

**AC#8: ActionValidator uses constructor injection**
- **Test**: Grep pattern=`public (sealed )?class ActionValidator\(\|public ActionValidator\(` path="Era.Core/Counter/ActionValidator.cs"
- **Expected**: 1+ match (matches both primary-constructor syntax `public sealed class ActionValidator(` and traditional `public ActionValidator(`)
- **Rationale**: All external dependencies must be injected via constructor. Constraint C8. Pattern supports both C# 12+ primary constructor and traditional syntax.

**AC#9: ActionSelector uses constructor injection**
- **Test**: Grep pattern=`public (sealed )?class ActionSelector\(\|public ActionSelector\(` path="Era.Core/Counter/ActionSelector.cs"
- **Expected**: 1+ match (matches both primary-constructor and traditional syntax)
- **Rationale**: All external dependencies must be injected via constructor. Constraint C8.

**AC#10: IVariableStore existing methods preserved**
- **Test**: Grep pattern=`(Get|Set)(Flag|TFlag|CharacterFlag|Ability|Talent|Palam|Exp|Base|TCVar|Source|Mark|NowEx|MaxBase|Cup|Juel|GotJuel|PalamLv|Stain|Downbase)\(` path="Era.Core/Interfaces/IVariableStore.cs" | count
- **Expected**: 38 (current 38 methods remain intact after GetEquip/SetEquip addition)
- **Rationale**: Interface extension must not break backward compatibility. Issue 63 pattern.

**AC#11: ActionSelector injects IRandomProvider**
- **Test**: Grep pattern=`IRandomProvider` path="Era.Core/Counter/ActionSelector.cs"
- **Expected**: 1+ match
- **Rationale**: Probabilistic selection requires injectable random for deterministic testing. Risk mitigation for non-deterministic test failures.

**AC#12: ActionSelector returns single action (not list)**
- **Test**: Grep pattern=`CounterActionId\?\s+SelectAction` path="Era.Core/Counter/ICounterSystem.cs"
- **Expected**: 1+ match -- method signature must return `CounterActionId?` (nullable single enum), not IReadOnlyList
- **Rationale**: ERB performs probabilistic single-action selection (COUNTER_SELECT.ERB:248-259), not list-available pattern. Constraint C9 (architecture reconciliation). Matcher verifies return type explicitly.

**AC#13: ActionValidator returns bool**
- **Test**: Grep pattern=`bool\s+(IsActable|Validate)` path="Era.Core/Counter/ActionValidator.cs"
- **Expected**: 1+ match
- **Rationale**: ACTABLE is pure function (RETURNF 0/1). C# equivalent must return bool. Constraint C2.

**AC#14: ActionValidator handles 36 CASE branches**
- **Test**: Grep pattern=`CounterActionId\.\w+ =>` path="Era.Core/Counter/ActionValidator.cs" | count
- **Expected**: 36 (matches COUNTER_ACTABLE.ERB SELECTCASE branch count)
- **Rationale**: All 36 validation branches must be migrated. Constraint C6.

**AC#15: IEngineVariables existing methods preserved**
- **Test**: Grep pattern=`(int|void|string)\s+(Get|Set)(Result|Money|Day|Master|Assi|Count|CharaNum|Random|Name|CallName|IsAssi|CharacterNo|Target|Player)\(` path="Era.Core/Interfaces/IEngineVariables.cs" | count
- **Expected**: 23 (current 21 methods + 2 new Target overloads: GetTarget(int index) and SetTarget(int index, int value) both match the `Target` branch of the pattern)
- **Rationale**: Interface extension must not break backward compatibility. The Target pattern matches both original scalar and new indexed overloads. GetSelectCom does not match the pattern (not in alternation).

**AC#16: IVariableStore declares SetEquip**
- **Test**: Grep pattern=`SetEquip\(CharacterId` path="Era.Core/Interfaces/IVariableStore.cs"
- **Expected**: 1+ match
- **Rationale**: Getter/setter pair required per Issue 65. SetEquip complements GetEquip. Constraint C4.

**AC#17: Zero technical debt in Counter namespace**
- **Test**: Grep pattern=`TODO|FIXME|HACK` path="Era.Core/Counter/"
- **Expected**: 0 matches
- **Rationale**: Zero technical debt mandated by phase-20-27-game-systems.md:237-239. Constraint C7.

**AC#18: ActionSelector equivalence tests exist**
- **Test**: Grep pattern=`ActionSelector.*Test|Test.*ActionSelector` path="Era.Core.Tests/"
- **Expected**: 1+ match (test class or test methods for weight calculation equivalence)
- **Rationale**: Equivalence tests required within same feature (Issue 48). Must verify weight formula and probabilistic selection with seeded random.

**AC#19: ActionValidator equivalence tests exist**
- **Test**: Grep pattern=`ActionValidator.*Test|Test.*ActionValidator` path="Era.Core.Tests/"
- **Expected**: 1+ match (test class or test methods for ACTABLE branch equivalence)
- **Rationale**: Equivalence tests required within same feature (Issue 48). Must cover all 36 SELECTCASE branches.

**AC#20: ActionValidator injects ITEquipVariables**
- **Test**: Grep pattern=`ITEquipVariables` path="Era.Core/Counter/ActionValidator.cs"
- **Expected**: 1+ match
- **Rationale**: ACTABLE uses TEQUIP at lines 27,79,88,97 (clothing state checks). ITEquipVariables is the existing ISP-segregated interface for TEQUIP access. Consensus finding C14.

**AC#21: ActionValidator injects ICommonFunctions**
- **Test**: Grep pattern=`ICommonFunctions` path="Era.Core/Counter/ActionValidator.cs"
- **Expected**: 1+ match
- **Rationale**: ACTABLE calls HAS_PENIS and HAS_VAGINA extensively. ICommonFunctions already provides HasPenis(int genderValue) and HasVagina(int genderValue). Constraint C8.

**AC#22: Unit tests pass**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/'`
- **Expected**: Exit code 0, all tests pass
- **Rationale**: All new code must compile and pass tests. TreatWarningsAsErrors=true (F708).

**AC#23: IEngineVariables declares SetTarget(int index) overload**
- **Test**: Grep pattern=`void SetTarget\(int index` path="Era.Core/Interfaces/IEngineVariables.cs"
- **Expected**: 1+ match
- **Rationale**: Getter/setter pair required per Issue 65. SetTarget(int index, int value) complements GetTarget(int index). Constraint C5.

**AC#24: ICounterUtilities.cs exists**
- **Test**: Glob pattern="Era.Core/Counter/ICounterUtilities.cs"
- **Expected**: File exists
- **Rationale**: Task#7 deliverable -- cross-phase utility abstraction interface grouping IsAirMaster, IsProtectedCheckOnly, CheckStain, IsVirginM, IsOnce. Required for ActionValidator and ActionSelector constructor injection.

**AC#25: IEngineVariables declares GetSelectCom**
- **Test**: Grep pattern=`GetSelectCom` path="Era.Core/Interfaces/IEngineVariables.cs"
- **Expected**: 1+ match
- **Rationale**: SELECTCOM accessor required for COUNTER_SELECT.ERB:21 early-exit guard (commands 351/400/490). Constraint C13.

**AC#26: IActionValidator.cs exists**
- **Test**: Glob pattern="Era.Core/Counter/IActionValidator.cs"
- **Expected**: File exists
- **Rationale**: Task#6 deliverable -- pure validation interface enabling ActionSelector to depend on abstraction. Required for mocking in ActionSelector tests (TDD).

**AC#27: ActionValidatorTests covers all 36 branches**
- **Test**: Grep pattern=`\[Fact\]\|\[Theory\]` path="Era.Core.Tests/Counter/ActionValidatorTests.cs" | count
- **Expected**: >= 36 (one test method per SELECTCASE branch)
- **Rationale**: AC#19 only verifies test class existence. This AC ensures behavioral equivalence testing depth — each of the 36 ACTABLE branches must have at least one dedicated test method.

**AC#28: ActionSelectorTests uses SeededRandomProvider**
- **Test**: Grep pattern=`SeededRandomProvider\|SeededRandom` path="Era.Core.Tests/Counter/ActionSelectorTests.cs"
- **Expected**: 1+ match
- **Rationale**: AC#18 only verifies test class existence. This AC ensures deterministic seeded random is actually used for weight calculation verification, enabling reproducible equivalence testing.

**AC#29: ActionSelector implements ICounterSystem**
- **Test**: Grep pattern=`IActionSelector.*ICounterSystem\|ICounterSystem.*IActionSelector` path="Era.Core/Counter/ActionSelector.cs"
- **Expected**: 1+ match
- **Rationale**: Architecture mandates ICounterSystem as the owner interface for F811 dependency. ActionSelector must declare both IActionSelector (Strategy Pattern) and ICounterSystem (architecture contract) in its base type list.

**AC#30: ActionValidator default arm returns false**
- **Test**: Grep pattern=`_ => false` path="Era.Core/Counter/ActionValidator.cs"
- **Expected**: 1+ match
- **Rationale**: ERB COUNTER_ACTABLE.ERB has `RETURNF 0` after ENDSELECT (line 340), meaning all 16 CNT_ values not covered by a CASE branch return false. The C# switch expression must have a `_ => false` default arm to preserve this behavior and prevent MatchFailureException for uncovered CounterActionId values.

**AC#31: ActionSelector injects ITEquipVariables**
- **Test**: Grep pattern=`ITEquipVariables` path="Era.Core/Counter/ActionSelector.cs"
- **Expected**: 1+ match
- **Rationale**: COUNTER_SELECT.ERB:86 reads TEQUIP:PLAYER:上半身着衣状況 for weight calculation in the うふふ path. ITEquipVariables must be injected for TEQUIP access.

**AC#32: ActionSelector writes TCVAR via CounterAction constant**
- **Test**: Grep pattern=`SetTCVar.*CounterAction` path="Era.Core/Counter/ActionSelector.cs"
- **Expected**: 1+ match
- **Rationale**: COUNTER_SELECT.ERB:259 writes `TCVAR:加害者:20 = セクハラ内容_ソフト` (the selected action). ActionSelector must perform this write via `SetTCVar(offender, TCVarIndex.CounterAction, (int)action)` using the named constant (not raw int 20), as downstream systems (COUNTER_MESSAGE, SOURCE) read TCVAR:20.

**AC#33: ActionValidator injects IEngineVariables**
- **Test**: Grep pattern=`IEngineVariables` path="Era.Core/Counter/ActionValidator.cs"
- **Expected**: 1+ match
- **Rationale**: COUNTER_ACTABLE.ERB uses SELECTCOM at lines 37 and 50 (CNT_スカートを捲る, CNT_耳元に息を吹きかける). IEngineVariables.GetSelectCom() required. Mirrors AC#20 (ITEquipVariables) and AC#21 (ICommonFunctions) pattern.

**AC#34: IActionSelector declares SelectAction with correct return type**
- **Test**: Grep pattern=`CounterActionId\?\s+SelectAction` path="Era.Core/Counter/IActionSelector.cs"
- **Expected**: 1+ match
- **Rationale**: AC#4 only verifies file existence. This AC verifies the Strategy Pattern interface declares the correct return type (CounterActionId?, not IReadOnlyList). Mirrors AC#12 for ICounterSystem.

**AC#35: ActionSelector injects ICounterOutputHandler**
- **Test**: Grep pattern=`ICounterOutputHandler` path="Era.Core/Counter/ActionSelector.cs"
- **Expected**: 1+ match
- **Rationale**: The うふふ path (COUNTER_SELECT.ERB:48-66) calls EVENT_COUNTER_REACTION and EVENT_COUNTER_PUNISHMENT. These are F802's responsibility. ActionSelector injects ICounterOutputHandler as a stub interface that F802 will implement. Enables early-exit paths for うふふ==1 (reaction) and うふふ==2 (punishment) without coupling ActionSelector to output/clothing systems.

**AC#36: ICounterOutputHandler.cs exists**
- **Test**: Glob pattern="Era.Core/Counter/ICounterOutputHandler.cs"
- **Expected**: File exists
- **Rationale**: Stub interface for F802 output handling. Declares HandleReaction(CharacterId offender, int actionOrder) and HandlePunishment(CharacterId offender, int actionOrder). F801 creates the interface; F802 implements it.

**AC#37: ActionSelector injects IEngineVariables**
- **Test**: Grep pattern=`IEngineVariables` path="Era.Core/Counter/ActionSelector.cs"
- **Expected**: 1+ match
- **Rationale**: ActionSelector uses IEngineVariables.GetSelectCom() for SELECTCOM early-exit guard (COUNTER_SELECT.ERB:21-24, commands 351/400/490). Mirrors AC#33 (ActionValidator injects IEngineVariables) pattern. Constraint C9/C13.

**AC#38: ICounterUtilities declares all 5 utility methods**
- **Test**: Grep pattern=`bool (IsAirMaster\|IsProtectedCheckOnly\|CheckStain\|IsVirginM\|IsOnce)\(` path="Era.Core/Counter/ICounterUtilities.cs" | count
- **Expected**: 5 (one per cross-phase utility method)
- **Rationale**: AC#24 only verifies file existence. This AC verifies all 5 methods are declared per Technical Design interface spec. Prevents partial interface declaration from passing.

**AC#39: ActionSelector checks double-execution guard flag**
- **Test**: Grep pattern=`カウンター行動決定|CounterDecisionFlag` path="Era.Core/Counter/ActionSelector.cs"
- **Expected**: 1+ match
- **Rationale**: Constraint C10. COUNTER_SELECT.ERB:44-46 increments TCVAR:加害者:カウンター行動決定フラグ and returns 0 if > 1 (prevents duplicate counter action selection per turn). ActionSelector must preserve this guard logic.

**AC#40: CounterActionId first sentinel value**
- **Test**: Grep pattern=`SeductiveGesture = 10` path="Era.Core/Counter/CounterActionId.cs"
- **Expected**: 1+ match
- **Rationale**: Anchors the first CNT_ constant value (CNT_色っぽい仕草 = 10 at DIM.ERH:323). Prevents value drift during migration. Complements AC#5 (count verification).

**AC#41: CounterActionId last sentinel value**
- **Test**: Grep pattern=`ReverseRapeRide = 91` path="Era.Core/Counter/CounterActionId.cs"
- **Expected**: 1+ match
- **Rationale**: Anchors the last CNT_ constant value (CNT_騎乗位逆レイプ = 91 at DIM.ERH:374). Prevents value drift during migration. Complements AC#5 (count verification).

**AC#42: ActionSelectorTests covers sex-category weight zeroing**
- **Test**: Grep pattern=`MissionaryInsert\|CowgirlInsert\|SexCategory` path="Era.Core.Tests/Counter/ActionSelectorTests.cs"
- **Expected**: 1+ match
- **Rationale**: Implementation Notes document critical ordering constraint: セクハラ内容_セックス is reset to 0 AFTER weight calculation, zeroing sex-category weights (indices 80-99). ActionSelectorTests must include at least one test verifying sex-category CounterActionId values (MissionaryInsert=80, CowgirlInsert=81, DoggyInsert=82, RearSeatInsert=83, ReverseRapeRide=91) are never selected. Complements AC#18 (test class existence) and AC#28 (SeededRandomProvider usage).

**AC#43: ActionSelector invokes HandleReaction and HandlePunishment**
- **Test**: Grep pattern=`HandleReaction\|HandlePunishment` path="Era.Core/Counter/ActionSelector.cs"
- **Expected**: 1+ match
- **Rationale**: AC#35 verifies ICounterOutputHandler injection but not invocation. The Philosophy claims "うふふ path REACTION/PUNISHMENT calls delegated to ICounterOutputHandler". ActionSelector must actually call `outputHandler.HandleReaction()` at the うふふ==1 branch (COUNTER_SELECT.ERB:52) and `outputHandler.HandlePunishment()` at the うふふ==2 branch (COUNTER_SELECT.ERB:58). Without this AC, delegation is injected but never exercised.

**AC#44: ActionValidator uses single-pattern switch arms only**
- **Test**: Grep pattern=`or CounterActionId` path="Era.Core/Counter/ActionValidator.cs"
- **Expected**: 0 matches
- **Rationale**: AC#14 counts 36 `CounterActionId.\w+ =>` arms. C# `or` pattern arms (`CounterActionId.A or CounterActionId.B =>`) would count as 1 arm for multiple cases, causing AC#14's count_equals to fail even if behavior is correct. This AC ensures single-pattern arms only, keeping AC#14 count reliable. Each of the 36 ACTABLE branches must be a separate switch arm.

**AC#45: ActionSelector TCVAR write is null-guarded**
- **Test**: Grep pattern=`if.*selectedAction.*!=.*null\|selectedAction is not null` path="Era.Core/Counter/ActionSelector.cs"
- **Expected**: 1+ match
- **Rationale**: Implementation Contract Phase 5 mandates "TCVAR write MUST be guarded by null-check on selected action (mirrors ERB SIF LOCAL:300 > 0 at line 258)". AC#32 only verifies SetTCVar with CounterAction appears in ActionSelector.cs but not the null-guard condition. Without this guard, ActionSelector would write a default/null value to TCVAR when no action is selected, violating ERB equivalence.

**AC#46: TCVarIndex declares CounterAction constant**
- **Test**: Grep pattern=`CounterAction = new\(20\)` path="Era.Core/Types/TCVarIndex.cs"
- **Expected**: 1+ match
- **Rationale**: TCVAR:20 (セクハラ内容/counter action result) is written by ActionSelector (COUNTER_SELECT.ERB:259). Named constant ensures type safety per TCVarIndex typed struct pattern; raw int 20 cast would violate the explicit-conversion-only design of TCVarIndex.

**AC#47: TCVarIndex declares CounterDecisionFlag constant**
- **Test**: Grep pattern=`CounterDecisionFlag = new\(30\)` path="Era.Core/Types/TCVarIndex.cs"
- **Expected**: 1+ match
- **Rationale**: TCVAR:30 (カウンター行動決定フラグ) is the double-execution guard flag (COUNTER_SELECT.ERB:44-46, TCVAR.csv:30). Named constant ensures type safety.

**AC#48: ICounterOutputHandler declares HandleReaction and HandlePunishment**
- **Test**: Grep pattern=`void Handle(Reaction\|Punishment)\(` path="Era.Core/Counter/ICounterOutputHandler.cs" | count
- **Expected**: 2 (one per method)
- **Rationale**: AC#36 only verifies file existence. This AC verifies the interface declares both delegation methods (HandleReaction, HandlePunishment) matching the うふふ path contract. Consistent with AC#38 (ICounterUtilities method count) and AC#34 (IActionSelector signature).

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Migrate COUNTER_SELECT.ERB to ActionSelector.cs | AC#1, AC#9, AC#11, AC#12, AC#18, AC#31, AC#32, AC#35, AC#37, AC#39, AC#45 |
| 2 | Migrate COUNTER_ACTABLE.ERB to ActionValidator.cs | AC#2, AC#8, AC#13, AC#14, AC#19, AC#20, AC#21, AC#26, AC#30, AC#33, AC#44 |
| 3 | ICounterSystem interface matching actual ERB behavior | AC#3, AC#12, AC#29 |
| 4 | Add missing GetEquip/SetEquip to IVariableStore | AC#6, AC#10, AC#16 |
| 5 | Add indexed GetTarget(int)/SetTarget(int)/GetSelectCom() to IEngineVariables | AC#7, AC#15, AC#23, AC#25 |
| 6 | Create CounterActionId enum for ~52 CNT_ constants | AC#5, AC#40, AC#41 |
| 7 | External utility dependencies abstracted behind injectable interfaces | AC#8, AC#9, AC#20, AC#21, AC#24, AC#38 |
| 8 | Strategy Pattern for ActionSelector | AC#4, AC#34 |
| 9 | Equivalence tests verify C# matches legacy ERB | AC#18, AC#19, AC#22, AC#27, AC#28, AC#42 |
| 10 | Zero technical debt | AC#17 |
| 11 | うふふ path stub interface for F802 | AC#35, AC#36, AC#43, AC#48 |
| 12 | TCVarIndex named constants for type-safe TCVAR access | AC#46, AC#47 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

The migration creates a greenfield `Era.Core/Counter/` namespace containing six new files and two interface extensions, then adds unit tests in `Era.Core.Tests/Counter/`. The architecture follows the Strategy Pattern mandated by the phase-20-27-game-systems.md spec: `IActionSelector` is the strategy interface, `ActionSelector` implements it, and `ICounterSystem` is the owning interface exposed to callers like F811 (SOURCE Entry System).

**ActionSelector.cs** (migrating COUNTER_SELECT.ERB, 260 lines):
The ERB function `@EVENT_COUNTER` performs three distinct phases:
1. **Pre-selection guard** (lines 1-67): SELECTCOM early-exit (commands 351/400/490), caller guard (`加害者 <= 0`), sleep guard, visitor-location guard (NTR/IS_AIR_MASTER/submission checks), double-execution guard (TCVAR:加害者:カウンター行動決定フラグ), and special-path dispatches (`うふふ` mode → REACTION/PUNISHMENT).
2. **Weight calculation** (lines 68-239): Compute 4 personality harassment components (恥じらい/誘惑/積極性/タチ), then calculate per-action weights for all 52 LOCAL:CNT_* slots, scaled by content-type multipliers (ソフト/着衣/脱衣愛撫/脱衣強要/セックス).
3. **Selection** (lines 240-260): Randomize all weights (`RAND:(MAX(w,1))`), filter by ACTABLE (`LOCAL:LOOP_CNT *= ACTABLE(加害者,LOOP_CNT)`), select maximum (`LOCAL:300` tracking), write result to `TCVAR:加害者:20`.

The C# translation uses `IRandomProvider.Next(long max)` for all RAND calls. Weight arrays are stored as `int[]` sized 100, indexed 0-99 (matching ERB LOCAL slot layout; LOCAL:300 sentinel maps to a separate `int maxWeight` variable). The method signature returns `CounterActionId?` (nullable enum — null means "no action selected").

**ActionValidator.cs** (migrating COUNTER_ACTABLE.ERB, 341 lines):
The ERB function `@ACTABLE` is a pure function (`#FUNCTION`, RETURNF 0/1). It pre-computes a "previous actions" lookup array from `TARGET:LOCAL` (lines 9-14), then evaluates a 36-branch SELECTCASE on the `カウンター行為` argument. The C# method signature is `bool IsActable(CharacterId offender, CounterActionId action)`. The pre-computation loop (TARGET indexed access) maps to `IEngineVariables.GetTarget(int index)`. The switch uses C# switch expression syntax (`CounterActionId.X =>`) to satisfy AC#14's grep matcher.

**Interface extension strategy**: IVariableStore gains `GetEquip(CharacterId, int)` and `SetEquip(CharacterId, int, int)` (raw int index, matching EQUIP array usage at ACTABLE:25,30,86,91). IEngineVariables gains `GetSelectCom()` (SELECTCOM scalar for early-exit guard), `GetTarget(int index)` and `SetTarget(int index, int value)` (indexed TARGET array, used in ACTABLE:10-13 loop).

**External utility interfaces**: Six utility functions from other phases have no C# equivalent. The design groups them by existing ISP-aligned boundaries:
- `ICommonFunctions` already covers `HasPenis/HasVagina` (called as `HAS_PENIS(MASTER)` → `_common.HasPenis(vars.GetTalent(master, TalentIndex.Gender).Match(v=>v, _=>0))`)
- `ITEquipVariables` already covers TEQUIP access (ACTABLE:27,79,88,97)
- A new `ICounterUtilities` interface covers the remaining 5 cross-phase functions: `IsAirMaster`, `IsProtectedCheckOnly`, `CheckStain`, `IsVirginM`, `IsOnce`

**CounterActionId enum**: 52 members with exact numeric values from DIM.ERH:323-374. Names follow C# PascalCase semantic English conventions. The file contains only enum member declarations (no other `= \d+` constructs) to ensure AC#5's count_equals matcher returns exactly 52.

**IActionSelector interface**: Declares `CounterActionId? SelectAction(CharacterId offender, int actionOrder)`. ActionSelector implements both IActionSelector and ICounterSystem (both have the same method signature). IActionValidator is exposed as a public interface to enable mocking in ActionSelector tests.

All 48 ACs are satisfied by this design. Tests in `Era.Core.Tests/Counter/` use `SeededRandomProvider` for deterministic weight calculation verification and direct parameter injection for ACTABLE branch testing.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core/Counter/ActionSelector.cs` implementing `IActionSelector` and `ICounterSystem` |
| 2 | Create `Era.Core/Counter/ActionValidator.cs` implementing `IActionValidator` with `bool IsActable(CharacterId, CounterActionId)` |
| 3 | Create `Era.Core/Counter/ICounterSystem.cs` declaring `CounterActionId? SelectAction(CharacterId, int)` |
| 4 | Create `Era.Core/Counter/IActionSelector.cs` declaring `CounterActionId? SelectAction(CharacterId, int)` (Strategy Pattern) |
| 5 | Create `Era.Core/Counter/CounterActionId.cs` as enum with all 52 CNT_ members from DIM.ERH:323-374, each assigned exact numeric value; file contains only enum member declarations |
| 6 | Add `Result<int> GetEquip(CharacterId character, int index)` to `Era.Core/Interfaces/IVariableStore.cs`; AC#6 matcher verifies `Result<int>` return type |
| 7 | Add `int GetTarget(int index)` overload to `Era.Core/Interfaces/IEngineVariables.cs` |
| 8 | ActionValidator constructor takes `IVariableStore`, `IEngineVariables`, `ITEquipVariables`, `ICommonFunctions`, `ICounterUtilities` as parameters |
| 9 | ActionSelector constructor takes `IVariableStore`, `IEngineVariables`, `IRandomProvider`, `IActionValidator`, `ICounterUtilities`, `ITEquipVariables`, `ICounterOutputHandler` as parameters; AC#37 verifies IEngineVariables injection explicitly |
| 10 | Verified: IVariableStore currently has 38 methods matching the grep pattern; adding GetEquip/SetEquip leaves these 38 intact |
| 11 | ActionSelector constructor parameter includes `IRandomProvider _random`; uses `_random.Next(max)` for weight randomization (COUNTER_SELECT.ERB:242) |
| 12 | `ICounterSystem.cs` declares `CounterActionId? SelectAction(CharacterId offender, int actionOrder)` — returns single nullable enum, not IReadOnlyList |
| 13 | ActionValidator declares `public bool IsActable(CharacterId offender, CounterActionId action)` — returns bool (migrates RETURNF 0/1) |
| 14 | ActionValidator body uses C# switch expression syntax: `return action switch { CounterActionId.X => ..., }` with 36 arm patterns covering all CASE branches |
| 15 | IEngineVariables has 21 existing methods; adding GetTarget(int)/SetTarget(int,int) matches the `Target` branch (+2 = 23); GetSelectCom not in pattern |
| 16 | Add `void SetEquip(CharacterId character, int index, int value)` to `Era.Core/Interfaces/IVariableStore.cs` |
| 17 | All new files in `Era.Core/Counter/` contain no TODO/FIXME/HACK comments |
| 18 | Create `Era.Core.Tests/Counter/ActionSelectorTests.cs` with test class name matching `ActionSelector.*Test` pattern, containing weight calculation and selection tests using `SeededRandomProvider` |
| 19 | Create `Era.Core.Tests/Counter/ActionValidatorTests.cs` with test class name matching `ActionValidator.*Test` pattern, covering all 36 CASE branch equivalence tests |
| 20 | ActionValidator constructor parameter includes `ITEquipVariables _tequip`; used for TEQUIP:MASTER:0/1 checks at ACTABLE:27,79,88,97 |
| 21 | ActionValidator constructor parameter includes `ICommonFunctions _common`; `_common.HasPenis(genderValue)` and `_common.HasVagina(genderValue)` replace `HAS_PENIS`/`HAS_VAGINA` ERB calls |
| 22 | Run `dotnet test Era.Core.Tests/` after implementation; expect exit code 0 |
| 23 | Add `void SetTarget(int index, int value)` overload to `Era.Core/Interfaces/IEngineVariables.cs` |
| 24 | Create `Era.Core/Counter/ICounterUtilities.cs` declaring all 5 cross-phase utility methods |
| 25 | Add `int GetSelectCom()` to `Era.Core/Interfaces/IEngineVariables.cs` |
| 26 | Create `Era.Core/Counter/IActionValidator.cs` declaring `bool IsActable(CharacterId offender, CounterActionId action)` |
| 27 | `Era.Core.Tests/Counter/ActionValidatorTests.cs` must contain >= 36 `[Fact]` or `[Theory]` test methods, one per ACTABLE branch |
| 28 | `Era.Core.Tests/Counter/ActionSelectorTests.cs` must use `SeededRandomProvider` for deterministic weight tests |
| 29 | ActionSelector.cs class declaration includes both `IActionSelector` and `ICounterSystem` in base type list |
| 30 | ActionValidator switch expression includes `_ => false` default arm (preserves ERB RETURNF 0 after ENDSELECT) |
| 31 | ActionSelector constructor includes `ITEquipVariables` for TEQUIP:PLAYER access in うふふ path weight calculation |
| 32 | ActionSelector calls `SetTCVar(offender, TCVarIndex.CounterAction, (int)action)` before returning, preserving COUNTER_SELECT.ERB:259 TCVAR write |
| 33 | ActionValidator constructor includes `IEngineVariables` for SELECTCOM access at ACTABLE:37,50 |
| 34 | IActionSelector.cs declares `CounterActionId? SelectAction(CharacterId, int)` matching ICounterSystem contract |
| 35 | ActionSelector constructor includes `ICounterOutputHandler outputHandler` parameter for うふふ path delegation |
| 36 | Create `Era.Core/Counter/ICounterOutputHandler.cs` (already in Phase 3 IC) |
| 37 | ActionSelector constructor includes `IEngineVariables engine` parameter (already listed; AC verifies explicitly) |
| 38 | `ICounterUtilities.cs` declares all 5 methods: IsAirMaster, IsProtectedCheckOnly, CheckStain, IsVirginM, IsOnce |
| 39 | ActionSelector checks and increments TCVAR:カウンター行動決定フラグ; returns null if > 1 (C10 double-execution guard) |
| 40 | Create `Era.Core/Counter/CounterActionId.cs` with `SeductiveGesture = 10` sentinel value |
| 41 | Create `Era.Core/Counter/CounterActionId.cs` with `ReverseRapeRide = 91` sentinel value |
| 42 | `Era.Core.Tests/Counter/ActionSelectorTests.cs` includes sex-category weight zeroing test verifying no MissionaryInsert/CowgirlInsert/etc. result |
| 43 | `Era.Core/Counter/ActionSelector.cs` calls `HandleReaction` and `HandlePunishment` for うふふ path delegation |
| 44 | `Era.Core/Counter/ActionValidator.cs` uses single-pattern switch arms only (no `or` pattern combining) |
| 45 | `Era.Core/Counter/ActionSelector.cs` contains null-check guard before TCVAR write |
| 46 | Add `CounterAction = new(20)` to `Era.Core/Types/TCVarIndex.cs` |
| 47 | Add `CounterDecisionFlag = new(30)` to `Era.Core/Types/TCVarIndex.cs` |
| 48 | Create `Era.Core/Counter/ICounterOutputHandler.cs` with HandleReaction and HandlePunishment method declarations (AC#36 existence + AC#48 signature count) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| ICounterSystem API: list vs single | A: Keep architecture spec `GetAvailableActions()→IReadOnlyList<ActionOption>` B: Reconcile to `SelectAction()→CounterActionId?` | B | ERB performs probabilistic single-action selection (COUNTER_SELECT.ERB:248-259 selects max); list-returning API does not match actual behavior; ICounterSystem reconciled to match ERB semantics (AC#12) |
| External utility grouping | A: One `ICounterUtilities` for all 6 functions B: Distribute across existing interfaces (ICommonFunctions, ITEquipVariables) + new `ICounterUtilities` for remaining 5 C: Separate interface per utility | B | ICommonFunctions already has HasPenis/HasVagina; ITEquipVariables already covers TEQUIP; grouping the 5 remaining utilities (IsAirMaster, IsProtectedCheckOnly, CheckStain, IsVirginM, IsOnce) into ICounterUtilities keeps ISP respected and avoids bloating existing interfaces |
| SELECTCOM accessor | A: Add `GetSelectCom()` to IEngineVariables B: Add new ISelectComVariables interface C: Pass SELECTCOM as method parameter to SelectAction | A | SELECTCOM is a scalar engine variable consistent with existing scalars (GetMaster/GetTarget/GetPlayer); additive change to IEngineVariables is the minimal-friction path; no ISP violation since callers already depend on IEngineVariables |
| EQUIP accessor typing | A: Raw `int` index B: Typed `EquipIndex` enum C: Use existing TEQUIP-style interface | A | EQUIP:MASTER:9 and EQUIP:MASTER:0 are raw integer accesses in ACTABLE:25,30,86,91; no existing EquipIndex enum exists; raw int matches TEQUIP accessor pattern (ITEquipVariables uses `int equipmentIndex`) |
| CounterActionId naming | A: Japanese names as-is (CNT_色っぽい仕草) B: Romanized transliteration C: Semantic English names | C | C# identifiers must follow PascalCase English conventions; semantic names (SeductiveGesture, Stare, BodyRub...) are self-documenting; numeric values are preserved exactly per C3 |
| ActionSelector weight array storage | A: `int[]` sized 100, indexed 0-99 B: `Dictionary<CounterActionId, int>` C: `int[]` sized 100, indexed 1-99 using 1-based access | A | ERB uses LOCAL:1-99 (1-indexed); C# int[100] with 0-based indexing provides O(1) access; LOCAL:0 and LOCAL:300 sentinel values map to `int maxWeight` variable; cleaner than 1-based indexing with off-by-one risk |
| IActionValidator interface | A: Expose as public interface `IActionValidator` B: Make ActionValidator an internal detail of ActionSelector | A | TDD requires ActionValidator to be independently testable (AC#19); ActionSelector depends on IActionValidator via constructor injection; exposing IActionValidator enables mocking in ActionSelector tests |
| ICounterSystem vs IActionSelector duplication | A: Keep both interfaces with identical `SelectAction()` signatures B: Collapse into a single interface C: Differentiate signatures | A | ICounterSystem is the architecture-mandated owner interface (phase-20-27-game-systems.md:120-125) consumed by F811 (SOURCE Entry System) as its public contract. IActionSelector is the Strategy Pattern seam (phase-20-27-game-systems.md:138-139) enabling internal swappability and test mocking. Despite identical current signatures, they represent distinct architectural roles: ICounterSystem is the cross-feature API boundary, IActionSelector is the intra-feature DI seam. Collapsing them would conflate architecture ownership with implementation strategy. Both are implemented by ActionSelector (AC#29). |
| うふふ path scope exclusion | A: Include IClothingSystem/ICounterReactionHandler in F801 B: Scope-exclude to F802 via ICounterOutputHandler stub | B | The うふふ path (COUNTER_SELECT.ERB:48-66) calls EVENT_COUNTER_REACTION/PUNISHMENT and CLOTHES operations. F802 (Main Counter Output) already owns REACTION/PUNISHMENT per Related Features table. ICounterOutputHandler provides a clean dependency boundary: F801 defines the interface, F802 implements it. This avoids coupling ActionSelector to IClothingSystem and keeps F801 focused on selection+validation logic. |

### Interfaces / Data Structures

**New files in `Era.Core/Counter/`**:

```csharp
// ICounterSystem.cs — owner interface (architecture-mandated)
public interface ICounterSystem
{
    CounterActionId? SelectAction(CharacterId offender, int actionOrder);
}

// IActionSelector.cs — Strategy Pattern interface
public interface IActionSelector
{
    CounterActionId? SelectAction(CharacterId offender, int actionOrder);
}

// IActionValidator.cs — pure validation interface (enables ActionSelector mocking in tests)
public interface IActionValidator
{
    bool IsActable(CharacterId offender, CounterActionId action);
}

// ICounterOutputHandler.cs — stub interface for F802 output handling (うふふ path)
public interface ICounterOutputHandler
{
    void HandleReaction(CharacterId offender, int actionOrder);
    /// <summary>
    /// Handles punishment path (うふふ==2). Responsible for:
    /// 1. Calling EVENT_COUNTER_PUNISHMENT
    /// 2. Iterating ALL characters to reset うふふ CFLAG to 0
    /// 3. Calling CLOTHES_RESET per character
    /// 4. Calling CLOTHES_SETTING_TRAIN per character
    /// (See COUNTER_SELECT.ERB:58-66)
    /// </summary>
    void HandlePunishment(CharacterId offender, int actionOrder);
}

// ICounterUtilities.cs — cross-phase utility abstractions (counter-specific)
public interface ICounterUtilities
{
    bool IsAirMaster(CharacterId offender);
    bool IsProtectedCheckOnly(CharacterId master, CharacterId offender, int brandType);
    bool CheckStain(CharacterId offender, CharacterId master);
    bool IsVirginM(CharacterId character);
    bool IsOnce(string key, int limit, CharacterId offender);
}

// CounterActionId.cs — 52-member enum with exact DIM.ERH numeric values
public enum CounterActionId
{
    SeductiveGesture = 10,
    Stare = 11,
    BodyRub = 12,
    SkirtFlip = 13,
    WhisperInEar = 14,
    ButtCaress = 15,
    Whisper = 16,
    ButtTorment = 21,
    PantsRemoval = 22,
    PantsSelection = 23,
    ItemAttachment = 24,
    SkirtLiftCommand = 25,
    ChastityBelt = 26,
    UrinationCommand = 27,
    MilkHandjob = 29,
    Kiss = 30,
    BackHug = 31,
    FrontHug = 32,
    ChestTouch = 33,
    CrotchGrope = 34,
    ShowUnderSkirt = 35,
    BreastGrip = 36,
    VirginOffering = 40,
    ForcedOrgasmP = 41,
    ForcedOrgasmV = 42,
    ForcedOrgasmA = 43,
    ForcedOrgasmC = 44,
    ForcedOrgasmB = 45,
    SexualReliefV = 46,
    SexualReliefA = 47,
    Masturbation = 50,
    Fellatio = 51,
    Frotteurism = 52,
    Paizuri = 53,
    LegJob = 54,
    AnalCaress = 55,
    AnalLick = 56,
    FingerInsertion = 57,
    BreastCaress = 58,
    NippleSuck = 59,
    DeepKiss = 60,
    ForcedCunnilingus = 70,
    ForcedFellatio = 71,
    AnalService = 72,
    PafPaf = 73,
    BreastRub = 74,
    BrushWash = 75,
    MissionaryInsert = 80,
    CowgirlInsert = 81,
    DoggyInsert = 82,
    RearSeatInsert = 83,
    ReverseRapeRide = 91,
}

// ActionSelector.cs — constructor signature (primary-constructor syntax, C# 12+)
public sealed class ActionSelector(
    IVariableStore variables,
    IEngineVariables engine,
    IRandomProvider random,
    IActionValidator validator,
    ICounterUtilities counterUtils,
    ITEquipVariables tequip,
    ICounterOutputHandler outputHandler) : IActionSelector, ICounterSystem
{
    public CounterActionId? SelectAction(CharacterId offender, int actionOrder) { ... }
    // Internal helpers:
    // private int[] ComputeWeights(CharacterId offender)
    // private void RandomizeWeights(int[] weights)
    // private void FilterByActable(int[] weights, CharacterId offender)
    // private CounterActionId? SelectMaxWeight(int[] weights)
}

// ActionValidator.cs — constructor signature (primary-constructor syntax, C# 12+)
public sealed class ActionValidator(
    IVariableStore variables,
    IEngineVariables engine,
    ITEquipVariables tequip,
    ICommonFunctions common,
    ICounterUtilities counterUtils) : IActionValidator
{
    public bool IsActable(CharacterId offender, CounterActionId action) { ... }
    // Internal helper:
    // private int[] BuildPreviousActionsLookup()
}
```

**Additive changes to existing interfaces**:

```csharp
// IVariableStore.cs additions
Result<int> GetEquip(CharacterId character, int index);    // EQUIP:chr:idx
void SetEquip(CharacterId character, int index, int value);

// IEngineVariables.cs additions
int GetSelectCom();                                         // SELECTCOM scalar
int GetTarget(int index);                                   // TARGET:LOCAL (indexed)
void SetTarget(int index, int value);                       // TARGET:LOCAL write
```

### Implementation Notes

**Weight Array Loop Bounds**: ERB `FOR LOOP_CNT,1,100` iterates indices 1-99 (1-indexed). C# `int[100]` uses 0-based indexing. The weight computation loop MUST iterate `for (int i = 1; i < 100; i++)` — index 0 has no CNT_ constant mapping and must not affect selection.

**セクハラ内容_セックス Reset Ordering**: At COUNTER_SELECT.ERB:231, `セクハラ内容_セックス` is reset to 0 BEFORE the sex-category individual weight calculations (lines 232-236) and BEFORE the multiplication loop (lines 237-239). The individual weight values (LOCAL:CNT_正常位挿入される etc.) are assigned positive values at lines 232-236, but then multiplied by `セクハラ内容_セックス` (which is 0) in the `FOR LOOP_CNT,80,99` loop at lines 237-239, permanently disabling sex-category selection. The C# implementation must place the reset (= 0) before both the individual assignments and the multiplication loop, matching the BEFORE constraint in Implementation Contract Phase 5.

**16 Uncovered CounterActionId Values**: Of the 52 CounterActionId enum members, only 36 have explicit CASE branches in COUNTER_ACTABLE.ERB. The remaining 16 values (CNT_囁く=16, CNT_お尻いじめ=21, CNT_パンツを脱がす=22, CNT_パンツ選択=23, CNT_アイテム装着される=24, CNT_スカートたくし上げ命令=25, CNT_貞操帯関連=26, CNT_飲尿命令=27, CNT_処女献上=40, CNT_強制絶頂P-B=41-45, CNT_性欲処理V/A=46-47) fall through to the implicit `RETURNF 0` at line 340. These are intentionally always-false in ACTABLE — they represent action types that either have no validation conditions or are handled by other subsystems. The `_ => false` default arm in ActionValidator correctly preserves this behavior.

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#14 uses grep pattern `CounterActionId.\w+ =>` which requires switch expression syntax. If implementation uses switch statement (`case CounterActionId.X:`), the pattern returns 0 matches. | Implementation Contract | No AC change needed; enforce switch expression syntax in Implementation Contract. The `=>` syntax is preferred C# 14 pattern anyway. |
| AC#5 `= \d+` pattern matches any `= <number>` in CounterActionId.cs; if the file contains non-member constructs (e.g., attributes, const fields), count would exceed 52. | Implementation Contract | Enforce in Implementation Contract: CounterActionId.cs contains only enum member declarations with no other numeric assignments. |
| IEngineVariables AC#15 backward-compatibility count: The pattern includes `Target` in the alternation. Adding GetTarget(int index) and SetTarget(int index, int value) ALSO matches the pattern (+2), raising count from 21 to 23. GetSelectCom is NOT in the pattern (excluded). | AC#15 Matcher | AC#15 expected count updated from 21 to 23. GetSelectCom remains excluded. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 6, 10, 16 | Add `GetEquip(CharacterId, int)` and `SetEquip(CharacterId, int, int)` to `Era.Core/Interfaces/IVariableStore.cs`; update concrete implementations (VariableStore.cs) with stub methods | | [x] |
| 2 | 7, 15, 23, 25 | Add `GetTarget(int index)`, `SetTarget(int index, int value)`, and `GetSelectCom()` to `Era.Core/Interfaces/IEngineVariables.cs`; update concrete implementations (EngineVariables.cs, NullEngineVariables.cs) with stub methods | | [x] |
| 3 | 5, 40, 41 | Create `Era.Core/Counter/CounterActionId.cs` enum with all 52 CNT_ members from DIM.ERH:323-374 using exact numeric values and semantic English PascalCase names; file contains only enum member declarations | | [x] |
| 4 | 3, 12 | Create `Era.Core/Counter/ICounterSystem.cs` declaring `CounterActionId? SelectAction(CharacterId offender, int actionOrder)` | | [x] |
| 5 | 4, 34 | Create `Era.Core/Counter/IActionSelector.cs` declaring `CounterActionId? SelectAction(CharacterId offender, int actionOrder)` (Strategy Pattern interface) | | [x] |
| 6 | 13, 26 | Create `Era.Core/Counter/IActionValidator.cs` declaring `bool IsActable(CharacterId offender, CounterActionId action)` | | [x] |
| 7 | 24, 38 | Create `Era.Core/Counter/ICounterUtilities.cs` declaring `IsAirMaster`, `IsProtectedCheckOnly`, `CheckStain`, `IsVirginM`, `IsOnce` (cross-phase utility abstractions) | | [x] |
| 8 | 2, 8, 13, 14, 20, 21, 30, 33, 44 | Create `Era.Core/Counter/ActionValidator.cs` implementing `IActionValidator` with constructor injection of `IVariableStore`, `IEngineVariables`, `ITEquipVariables`, `ICommonFunctions`, `ICounterUtilities`; migrate all 36 SELECTCASE branches from COUNTER_ACTABLE.ERB using C# switch expression syntax; include `_ => false` default arm | | [x] |
| 9 | 1, 9, 11, 12, 29, 31, 32, 35, 37, 39, 43, 45 | Create `Era.Core/Counter/ActionSelector.cs` implementing `IActionSelector` and `ICounterSystem` with constructor injection of `IVariableStore`, `IEngineVariables`, `IRandomProvider`, `IActionValidator`, `ICounterUtilities`, `ITEquipVariables`, `ICounterOutputHandler`; migrate COUNTER_SELECT.ERB guard logic, weight calculation, and probabilistic single-action selection; うふふ path delegates to ICounterOutputHandler | | [x] |
| 10 | 17 | Verify zero technical debt: no TODO/FIXME/HACK in any file under `Era.Core/Counter/` | | [x] |
| 11 | 18, 28, 42 | Create `Era.Core.Tests/Counter/ActionSelectorTests.cs` with test class `ActionSelectorTests` covering weight calculation and probabilistic selection using `SeededRandomProvider` | | [x] |
| 12 | 19, 27 | Create `Era.Core.Tests/Counter/ActionValidatorTests.cs` with test class `ActionValidatorTests` covering all 36 SELECTCASE branch equivalence tests (>= 36 [Fact]/[Theory] methods) | | [x] |
| 13 | 22 | Run `dotnet test Era.Core.Tests/` and confirm all tests pass (exit code 0) | | [x] |
| 14 | 36, 48 | Create `Era.Core/Counter/ICounterOutputHandler.cs` declaring `HandleReaction(CharacterId, int)` and `HandlePunishment(CharacterId, int)` (stub interface for F802 output handling) | | [x] |
| 15 | 46, 47 | Add `CounterAction = new(20)` and `CounterDecisionFlag = new(30)` named constants to `Era.Core/Types/TCVarIndex.cs` | | [x] |

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

**When NOT to use `[I]`**:
- Build/compile tasks (Expected = "succeeds" is always deterministic)
- Tasks with spec-defined outputs (API contracts, schema validation)
- Tasks that verify file existence (deterministic yes/no)
- Tasks where Expected can be calculated from requirements (counts from data, paths from config)
- Standard patterns with known outputs (error messages, status codes)

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1: Interface Extensions + TCVarIndex | implementer | sonnet | Task#1, Task#2, Task#15; `Era.Core/Interfaces/IVariableStore.cs`, `Era.Core/Interfaces/IEngineVariables.cs`, `Era.Core/Types/TCVarIndex.cs`; AC#6, AC#7, AC#10, AC#15, AC#16, AC#23, AC#46, AC#47; Technical Design interface extension specs | `IVariableStore.cs` with `GetEquip`/`SetEquip` added; `IEngineVariables.cs` with `GetTarget(int)`/`SetTarget(int,int)`/`GetSelectCom()` added; `TCVarIndex.cs` with `CounterAction = new(20)` and `CounterDecisionFlag = new(30)` added; concrete implementations updated with stub methods (throw NotImplementedException or return default) |
| 2: CounterActionId Enum | implementer | sonnet | Task#3; `Game/ERB/DIM.ERH:323-374`; AC#5, AC#40, AC#41; Technical Design CounterActionId enum spec (52 members, semantic English names, exact numeric values) | `Era.Core/Counter/CounterActionId.cs` with 52 members; file contains only enum member declarations |
| 3: Counter Interfaces | implementer | sonnet | Task#4, Task#5, Task#6, Task#7, Task#14; AC#3, AC#4, AC#12, AC#13, AC#36; Technical Design interface definitions for `ICounterSystem`, `IActionSelector`, `IActionValidator`, `ICounterUtilities`, `ICounterOutputHandler` | `ICounterSystem.cs`, `IActionSelector.cs`, `IActionValidator.cs`, `ICounterUtilities.cs`, `ICounterOutputHandler.cs` in `Era.Core/Counter/` |
| 4: ActionValidator Implementation | implementer | sonnet | Task#8; `Game/ERB/COUNTER_ACTABLE.ERB` (341 lines); AC#2, AC#8, AC#13, AC#14, AC#20, AC#21, AC#30, AC#33, AC#44; Technical Design ActionValidator constructor and switch expression spec; CONSTRAINT: use C# switch expression syntax (`CounterActionId.X =>`) for all 36 branches; MUST include `_ => false` default arm (preserves ERB RETURNF 0 after ENDSELECT) | `Era.Core/Counter/ActionValidator.cs` implementing `IActionValidator`; 36-arm switch expression + default arm; constructor injection of 5 interfaces |
| 5: ActionSelector Implementation | implementer | sonnet | Task#9; `Game/ERB/COUNTER_SELECT.ERB` (260 lines); AC#1, AC#9, AC#11, AC#12, AC#31, AC#32, AC#35, AC#37, AC#39, AC#43, AC#45; Technical Design ActionSelector constructor (incl. ITEquipVariables), weight array (int[100]), guard logic, weight calculation (4 personality components), probabilistic single-action selection; CONSTRAINT: returns `CounterActionId?` (null = no action); weight loop iterates indices 1-99 (not 0-99); セクハラ内容_セックス is reset to 0 BEFORE sex-category weight loop; TCVAR write via `TCVarIndex.CounterAction` MUST be guarded by null-check on selected action (mirrors ERB `SIF LOCAL:300 > 0` at line 258); class declaration and base type list MUST be on a single line | `Era.Core/Counter/ActionSelector.cs` implementing `IActionSelector` and `ICounterSystem` |
| 6: Zero Debt Verification | ac-tester | haiku | Task#10; AC#17; `Era.Core/Counter/` directory | Grep confirms 0 TODO/FIXME/HACK matches |
| 7: Equivalence Tests | implementer | sonnet | Task#11, Task#12; AC#18, AC#19, AC#27, AC#28, AC#42; `Era.Core.Tests/Counter/`; `ActionSelector.cs`, `ActionValidator.cs`; CONSTRAINT: test class names must match grep pattern (`ActionSelectorTests`, `ActionValidatorTests`); ActionValidatorTests must have >= 36 [Fact]/[Theory] methods; ActionSelectorTests must use SeededRandomProvider; ActionSelectorTests must include sex-category weight zeroing test verifying sex-category actions (MissionaryInsert/CowgirlInsert/etc.) are never selected | `Era.Core.Tests/Counter/ActionSelectorTests.cs`; `Era.Core.Tests/Counter/ActionValidatorTests.cs` |
| 8: Build and Test Verification | ac-tester | haiku | Task#13; AC#22; WSL dotnet test command | `dotnet test Era.Core.Tests/` exit code 0, all tests pass |

### Pre-conditions

- F783 is [DONE] (Predecessor satisfied)
- `Era.Core/Interfaces/IVariableStore.cs` exists and has 38 existing methods matching AC#10 grep pattern
- `Era.Core/Interfaces/IEngineVariables.cs` exists and has 21 existing methods matching AC#15 grep pattern
- `Era.Core/Counter/` directory does not exist (greenfield namespace)

### Execution Order

Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5 → Phase 6 → Phase 7 → Phase 8

**Rationale**: Interface extensions (Phase 1-3) must precede implementations (Phase 4-5) because ActionValidator and ActionSelector constructors reference the new interface methods. CounterActionId enum (Phase 2) must precede all Counter files since ActionValidator and ActionSelector switch on it.

### Build Verification

After Phase 5 (before tests), run:
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'
```
Expect: exit code 0, 0 errors, 0 warnings (TreatWarningsAsErrors=true).

### Success Criteria

All 48 ACs pass. `dotnet test Era.Core.Tests/` exits with code 0.

### Error Handling

- Compile error in Phase 1/2/3: STOP, report to user — interface extension may have broken backward compatibility
- AC#10 or AC#15 count mismatch: STOP, report — existing methods may have been accidentally removed
- AC#14 count != 36: STOP, report — switch expression syntax may not have been used, or branch count mismatch with COUNTER_ACTABLE.ERB
- AC#5 count != 52: STOP, report — CounterActionId.cs may contain non-member numeric constructs or wrong member count
- Test failure in Phase 8: STOP, report to user

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature exists -->
<!-- Option C (Phase): Phase exists in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| ICounterSystem API reconciled from `GetAvailableActions()→IReadOnlyList<ActionOption>` to `SelectAction()→CounterActionId?` | Architecture spec `phase-20-27-game-systems.md:120-125` no longer matches implementation; must be updated to reflect single-select pattern | Feature | F813 | - |
| ICounterOutputHandler implementation (うふふ path REACTION/PUNISHMENT/CLOTHES) | F801 defines stub interface; F802 must implement HandleReaction/HandlePunishment with actual EVENT_COUNTER_REACTION/PUNISHMENT and CLOTHES_RESET/SETTING_TRAIN logic | Feature | F802 | - |

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
| 2026-02-23 | START | implementer | IC Phase 1: Interface Extensions + TCVarIndex (Tasks 1,2,15) | - |
| 2026-02-23 | END | implementer | IC Phase 1: Interface Extensions + TCVarIndex (Tasks 1,2,15) | SUCCESS |
| 2026-02-23 | START | implementer | IC Phase 2: CounterActionId Enum (Task 3) | - |
| 2026-02-23 | END | implementer | IC Phase 2: CounterActionId Enum (Task 3) | SUCCESS |
| 2026-02-23 | START | implementer | IC Phase 3: Counter Interfaces (Tasks 4,5,6,7,14) | - |
| 2026-02-23 | END | implementer | IC Phase 3: Counter Interfaces (Tasks 4,5,6,7,14) | SUCCESS |
| 2026-02-23 | START | implementer | IC Phase 4: ActionValidator Implementation (Task 8) | - |
| 2026-02-23 | END | implementer | IC Phase 4: ActionValidator Implementation (Task 8) | SUCCESS |
| 2026-02-23 | START | implementer | IC Phase 5: ActionSelector Implementation (Task 9) | - |
| 2026-02-23 | END | implementer | IC Phase 5: ActionSelector Implementation (Task 9) | SUCCESS |
| 2026-02-23 | END | ac-tester | IC Phase 6: Zero Debt Verification (Task 10) | SUCCESS |
| 2026-02-23 | START | implementer | IC Phase 7: Equivalence Tests (Tasks 11,12) | - |
| 2026-02-23 | END | implementer | IC Phase 7: Equivalence Tests (Tasks 11,12) | SUCCESS |
| 2026-02-23 | END | ac-tester | IC Phase 8: Build and Test Verification (Task 13) | SUCCESS (2398 tests, 0 failures) |
| 2026-02-23 | END | ac-tester | Phase 7 AC Verification | 48/48 PASS (8 parser-format issues manually verified) |
| 2026-02-23 | DEVIATION | feature-reviewer | Phase 8.1 Quality Review | NEEDS_REVISION: Tasks 1,2,10,13,15 status not marked [x]; Execution Log missing IC Phase 1,2,6,8 entries |
| 2026-02-23 | END | opus | Phase 8.1 NEEDS_REVISION fix | Fixed: all Tasks marked [x], Execution Log entries added |
| 2026-02-23 | END | ac-tester | Phase 7 AC Re-verification | 48/48 PASS |
| 2026-02-23 | DEVIATION | feature-reviewer | Phase 8.1 Quality Review (re-run) | NEEDS_REVISION: engine-dev SKILL.md missing SSOT updates for IEngineVariables new methods and Counter namespace |
| 2026-02-23 | END | implementer | Phase 8.1 SSOT fix | Applied: IEngineVariables methods, Counter Interfaces section, TCVarIndex constants to engine-dev SKILL.md |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: AC#12 matcher | AC#12 matcher strengthened from `SelectAction` to `CounterActionId\?\s+SelectAction` to verify return type explicitly
- [fix] Phase2-Review iter1: AC#27,AC#28 addition | Added AC#27 (ActionValidatorTests >= 36 test methods) and AC#28 (ActionSelectorTests SeededRandomProvider usage) to enforce test coverage depth
- [fix] Phase2-Review iter2: AC#15 expected count | AC#15 count_equals updated from 21 to 23 (GetTarget(int)/SetTarget(int,int) match Target pattern)
- [fix] Phase2-Review iter2: AC#29 addition | Added AC#29 (ActionSelector implements ICounterSystem) to verify architecture contract
- [fix] Phase2-Review iter2: Constraint C3 text | Updated C3 from '~40 CNT_ action types' to '52 CNT_ action types' matching actual DIM.ERH count
- [fix] Phase2-Review iter3: AC#30 addition | Added AC#30 (ActionValidator default arm _ => false) to preserve ERB RETURNF 0 after ENDSELECT behavior
- [fix] Phase2-Review iter4: Mandatory Handoffs | Added handoff entry for ICounterSystem API reconciliation to F813 (Post-Phase Review)
- [fix] Phase3-Maintainability iter5: AC#31 addition | Added AC#31 (ActionSelector injects ITEquipVariables) for TEQUIP:PLAYER access in うふふ path
- [fix] Phase3-Maintainability iter5: AC#10 matcher | Changed AC#10 from count_equals to count_gte for forward compatibility
- [fix] Phase3-Maintainability iter5: Implementation Notes | Added weight array loop bounds, セクハラ内容_セックス reset ordering, and 16 uncovered CNT_ documentation
- [resolved-applied] Phase3-Maintainability iter5: ActionSelector missing IClothingSystem and ICounterReactionHandler dependencies for うふふ==2 path (COUNTER_SELECT.ERB:48-66 calls EVENT_COUNTER_REACTION/PUNISHMENT and CLOTHES_RESET/SETTING_TRAIN). Design decision needed: add interfaces or scope-exclude.
- [resolved-applied] Phase3-Maintainability iter5: ICounterSystem and IActionSelector identical contracts (raised 3x in Phase 2, rejected by validators as deliberate design). Location not found in file sections.
- [resolved-invalid] Phase3-Maintainability iter5: AC#27 pipe escaping — reviewer claims \| is literal pipe, but validated INVALID in Phase 2 iter3 per F792 (pipe escaping supported in AC system).
- [fix] Phase7-FinalRefCheck iter6: Links section | Added F708 (TreatWarningsAsErrors policy) to Links as Context reference
- [fix] Phase2-Review iter7: AC#8,AC#9 matchers | Updated to support both primary-constructor and traditional constructor syntax
- [fix] Phase2-Review iter8: AC#32 addition | Added AC#32 (ActionSelector writes TCVAR:20) for COUNTER_SELECT.ERB:259 write responsibility
- [fix] Phase2-Review iter8: AC#33 addition | Added AC#33 (ActionValidator injects IEngineVariables) for SELECTCOM access
- [fix] Phase2-Review iter8: AC#34 addition | Added AC#34 (IActionSelector CounterActionId? SelectAction) for Strategy Pattern contract verification
- [fix] Phase1-RefCheck iter1: Links section | Removed broken F708 link (feature-708.md does not exist)
- [fix] Phase2-Review iter1: Key Decisions | Added ICounterSystem vs IActionSelector duplication rationale documenting architectural role distinction
- [fix] Phase2-Uncertain iter1: Summary | Expanded Summary to include all major deliverables (interfaces, enum, extensions)
- [fix] Phase2-Review iter2: AC#6 matcher | Strengthened from `GetEquip\(CharacterId` to `Result<int> GetEquip\(CharacterId` to verify return type
- [fix] Phase2-Review iter3: C6 constraint | Updated from '26+ SELECTCASE branches' to 'exactly 36 SELECTCASE branches' matching actual ERB count
- [fix] Phase2-Review iter3: Implementation Contract Phase 5 | Added TCVAR:20 conditional write constraint (guard by null-check, mirrors ERB SIF LOCAL:300 > 0)
- [fix] PostLoop-UserFix iter4: うふふ path scope exclusion | User chose option A: scope-exclude to F802 via ICounterOutputHandler. Added AC#35, AC#36, Task#14, Key Decision, Mandatory Handoff to F802, updated ActionSelector constructor/Technical Design
- [fix] Phase2-Review iter1: AC#37 addition | Added AC#37 (ActionSelector injects IEngineVariables) for SELECTCOM early-exit guard coverage
- [fix] Phase2-Review iter1: AC#38 addition | Added AC#38 (ICounterUtilities declares all 5 utility methods) for method count verification beyond file existence
- [fix] Phase2-Review iter2: AC Coverage table | Added AC#35-39 to Technical Design AC Coverage, updated '34 ACs' to '39 ACs'
- [fix] Phase2-Review iter2: AC#39 addition | Added AC#39 (double-execution guard flag) for Constraint C10
- [fix] Phase2-Review iter2: Implementation Contract Phase 4 | Added AC#33 to Phase 4 input list
- [resolved-skipped] Phase3-Maintainability iter3: AC#10 count_gte vs count_equals — reviewer wants revert to count_equals=38 (A→B→A loop detected: originally count_equals, changed to count_gte in iter5, now proposed back). Forward-compat design decision contested. User chose: keep gte 38 (forward-compatible, deletion still detectable since Equip not in pattern).
- [fix] Phase4-ACValidation iter3: AC#10 matcher | Changed invalid `count_gte` to valid `gte` matcher
- [fix] Phase4-ACValidation iter3: AC#27 matcher+type | Changed invalid `count_gte` to `gte`, type `test` to `code`
- [fix] Phase4-ACValidation iter3: AC#18,#19,#28 type | Changed type from `test` to `code` (Grep-based static verification, not dotnet test)
- [fix] Phase2-Review iter1: AC#5 row | Escaped pipe between pattern and count to fix 8-column → 7-column markdown table
- [fix] Phase2-Review iter1: AC#10 row | Escaped all regex pipes and separator pipe to fix column count mismatch
- [fix] Phase2-Review iter1: AC#40,AC#41 addition | Added sentinel value ACs (SeductiveGesture=10, ReverseRapeRide=91) to verify CounterActionId numeric value anchoring
- [fix] Phase2-Review iter2: AC#42 addition | Added AC#42 (ActionSelectorTests sex-category weight zeroing) to verify Implementation Notes ordering constraint
- [fix] Phase3-Maintainability iter3: AC count | Updated '39 ACs' to '42 ACs' in Technical Design and Success Criteria
- [fix] Phase2-Review iter4: Implementation Notes | Fixed セクハラ内容_セックス reset ordering description to align with IC Phase 5 (AFTER→BEFORE)
- [fix] Phase2-Review iter4: Goal section | Added ICounterOutputHandler stub interface to Goal to match Goal Coverage row 11
- [fix] Phase2-Review iter5: AC#43 addition | Added AC#43 (ActionSelector calls HandleReaction/HandlePunishment) for うふふ delegation invocation verification
- [fix] Phase2-Review iter6: AC#44 addition | Added AC#44 (no multi-pattern switch arms in ActionValidator) to prevent `or` pattern combining that breaks AC#14 count
- [fix] Phase3-Maintainability iter7: Tasks/IC/Impact | Added concrete implementation stub updates to Task#1, Task#2, IC Phase 1, and fixed Impact Analysis claim
- [fix] Phase2-Review iter8: AC#45 addition | Added AC#45 (TCVAR:20 null-guard) to verify IC Phase 5 null-check constraint on selected action
- [fix] Phase2-Review iter9: C3 detail | Fixed C3 detail body from '~40 constants' to '52 constants' matching C3 table row
- [fix] Phase3-Maintainability iter10: HandlePunishment doc | Added XML doc comment to ICounterOutputHandler.HandlePunishment specifying all-character iteration scope
- [fix] Phase3-Maintainability iter10: IC Phase 5 | Added single-line class declaration constraint for AC#29 matcher reliability
- [resolved-applied] Phase3-Maintainability iter10: SetTCVar uses TCVarIndex typed struct, not raw int. Design says `SetTCVar(offender, 20, action)` but IVariableStore.SetTCVar takes TCVarIndex. Resolution: Added Task#15 (TCVarIndex named constants CounterAction=20, CounterDecisionFlag=30), AC#46, AC#47. Updated AC#32 matcher, IC Phase 1/5, Goal Coverage.
- [fix] Phase3-Maintainability iter1: AC#32,AC#46,AC#47,Task#15 | Resolved SetTCVar/TCVarIndex pending: added named constants task, updated AC#32 matcher to `SetTCVar.*CounterAction`, added AC#46/AC#47 for constant verification, updated IC Phase 1 and Phase 5, added Goal Coverage row 12
- [fix] Phase2-Review iter2: Success Criteria | Updated '45 ACs' to '48 ACs' in Technical Design and Success Criteria
- [fix] Phase2-Review iter2: AC#48 addition | Added AC#48 (ICounterOutputHandler declares HandleReaction/HandlePunishment) for structural interface verification. Updated Task#14 AC list, Goal Coverage row 11

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning
- [Successor: F811](feature-811.md) - SOURCE Entry System (calls EVENT_COUNTER)
- [Successor: F813](feature-813.md) - Post-Phase Review Phase 21
- [Related: F802](feature-802.md) - Main Counter Output (REACTION/PUNISHMENT calls)
- [Related: F803](feature-803.md) - Main Counter Source
- [Related: F809](feature-809.md) - COMABLE Core

