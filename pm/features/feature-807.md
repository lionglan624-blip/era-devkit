# Feature 807: WC Counter Message TEASE

## Status: [DONE]
<!-- fl-reviewed: 2026-03-02T11:57:18Z -->

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

Phase 21 Counter System migration converts ERB counter message handlers to C# with structural migration equivalence (static verification of class structure, interface injection, dispatch delegation, and call-site counts; behavioral equivalence verified in F813 Post-Phase Review). Each ERB message category (SEX, TEASE, ITEM+NTR) is migrated as a separate feature to maintain single-responsibility scope and migration quality. WC Counter Message TEASE is the canonical implementation of TEASE-category message handlers in Era.Core (IWcCounterMessageTease interface extraction deferred to F813 per Mandatory Handoffs).

### Problem (Current Issue)

WcCounterMessage.cs dispatch stubs for actions 21-27 and branch actions (29,1-3) return 0 without producing any output (WcCounterMessage.cs:241-311), because TOILET_COUNTER_MESSAGE_TEASE.ERB (3,483 lines, 10 functions) has not been migrated to C#. The ERB source contains 33+ TRYCALL delegations to F808 ITEM/NTR functions, 66+ KOJO dispatch calls, 2 interactive INPUT loops (MESSAGE25/MESSAGE26 -- the first INPUT usage in the Counter subsystem), 100+ state mutations (CFLAG/EQUIP/TCVAR/TALENT), and per-character branching (MESSAGE27). This complexity was not analyzed during F783 Phase 21 Planning, which decomposed by ERB file name without cross-call chain or per-function complexity assessment.

### Goal (What to Achieve)

Implement all 10 TEASE message handlers (MESSAGE21-27, MESSAGE29_1-3) as a new WcCounterMessageTease class, replacing the dispatch stubs in WcCounterMessage.cs with delegation calls, so that TEASE-category counter message handlers are structurally migrated with correct interface injection, dispatch delegation, and call-site counts. Register the new class in DI and integrate IInputHandler for MESSAGE25/MESSAGE26 interactive loops. All handler methods must use block-body syntax with explicit `return 1;` to enable static verification via grep-based AC counting. Transfer deferred obligations to F813 Post-Phase Review: behavioral test coverage, IWcCounterMessageTease interface extraction, character ID constant consolidation, constructor growth mitigation, CFlag/Cflag naming normalization, and AC#34 local function enforcement gap. Behavioral equivalence (output and state mutation correctness) is verified in F813.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why? | WcCounterMessage.cs dispatch stubs for actions 21-27 and (29,1-3) return 0 with no handler bodies | WcCounterMessage.cs:241-248, 306-309 |
| 2 | Why? | TOILET_COUNTER_MESSAGE_TEASE.ERB has not been migrated to C# | feature-807.md Status: [DRAFT] |
| 3 | Why? | The feature file has only 2 placeholder tasks and 1 trivial AC, insufficient to guide implementation of 3,483 lines across 10 functions | feature-807.md:63-77 |
| 4 | Why? | F783 Phase 21 Planning decomposed by ERB file grouping without analyzing cross-file TRYCALL chains or per-function complexity | feature-808.md:53 (root cause documented in F808 5-whys) |
| 5 | Why (Root)? | The ERB source has 10 functions with 33+ TRYCALL external calls, 100+ state mutations, 66+ KOJO dispatches, 2 INPUT loops, and GOTO flow control -- requiring comprehensive /fc completion before implementation | TOILET_COUNTER_MESSAGE_TEASE.ERB:1-3484 |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Dispatch stubs return 0 for TEASE actions; no TEASE message output produced | TEASE ERB (3,483 lines, 10 functions) not migrated; feature file lacks per-function tasks and ACs |
| Where | WcCounterMessage.cs:241-311 (stub methods) | TOILET_COUNTER_MESSAGE_TEASE.ERB (10 functions with complex dependencies) |
| Fix | Hardcode return values in stubs | Create WcCounterMessageTease class implementing all 10 handlers with proper interface delegation, INPUT handling, and state mutation |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| F805 | [DONE] | WC Counter Message Core -- dispatch infrastructure, WcCounterMessage.cs |
| F806 | [PROPOSED] | WC Counter Message SEX -- sibling, same dispatch pattern, no cross-calls |
| F808 | [DONE] | WC Counter Message ITEM + NTR -- provides IWcCounterMessageItem (17 methods) and IWcCounterMessageNtr (6 methods) |
| F813 | [DRAFT] | Post-Phase Review Phase 21 (successor, blocked by F807) |
| F814 | [DRAFT] | Phase 22 Planning (successor of F813) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Scope clarity | FEASIBLE | Single ERB file, 10 bounded functions mapped to dispatch stubs |
| Infrastructure readiness | FEASIBLE | All required interfaces exist (IWcCounterMessageItem, IWcCounterMessageNtr, IInputHandler, ICommonFunctions, etc.) |
| Predecessor completeness | FEASIBLE | F805 [DONE], F808 [DONE] -- dispatch infrastructure and ITEM/NTR interfaces available |
| State mutation complexity | FEASIBLE | High volume (100+ writes) but localized and testable, follows existing patterns |
| INPUT handling (MESSAGE25/26) | FEASIBLE | IInputHandler interface exists; first usage in Counter but pattern established in Shop subsystem |
| Kojo dispatch (66+ calls) | FEASIBLE | IKojoMessageService exists with default no-op implementation |
| Code volume | FEASIBLE | 3,483 ERB lines but structurally repetitive branching patterns |
| Pattern precedent | FEASIBLE | F805 and F808 establish class extraction and DI registration patterns |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| WcCounterMessage dispatch | HIGH | 10 stubs replaced with delegation to WcCounterMessageTease |
| DI composition root | MEDIUM | ServiceCollectionExtensions.cs updated with new registration |
| Counter subsystem architecture | MEDIUM | First IInputHandler integration in Counter (architectural precedent) |
| F808 interface consumption | MEDIUM | First consumer of IWcCounterMessageItem and IWcCounterMessageNtr |
| F806 (sibling) | LOW | No cross-calls; fully independent |
| F813 Phase review | LOW | Unblocks post-Phase 21 review |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| WcCounterMessage constructor already has 10 dependencies | WcCounterMessage.cs:39-61 | Must extract WcCounterMessageTease as separate class to avoid constructor explosion |
| INPUT loop semantics require IInputHandler | TEASE.ERB:2377-2468, 2549-2668 | MESSAGE25 and MESSAGE26 use INPUT with GOTO-based retry; must translate to while loops |
| GOTO-based flow control in ERB | TEASE.ERB:2468, 2668 | GOTO INPUT_LOOP_WC_1/2 must map to C# while-loop with IInputHandler |
| TRYCALL no-op semantics | ERB TRYCALL pattern (33+ calls) | Must use null-conditional (?.) or guaranteed non-null injection for F808 delegations |
| TRYCALLFORM KOJO dynamic dispatch | 66+ KOJO calls | IKojoMessageService.KojoMessageWcCounter with character index and action type |
| PRINTDATA random text selection | TEASE.ERB:124-127 | MESSAGE21 uses PRINTDATA; requires IRandomProvider or equivalent |
| EQUIP state mutations during execution | Multiple EQUIP writes in MESSAGE22/23 | ITEquipVariables must support SetEquip/SetTEquip |
| GETBIT/SETBIT bitfield operations | ~300 operations in MESSAGE24 | BitfieldUtility exists; must map ERB bit names to constants |
| Character-specific branching | 131 character references in MESSAGE27 | Must define character ID constants |
| TreatWarningsAsErrors | Directory.Build.props | All C# code must compile warning-free |
| #DIM local variables | TEASE.ERB:1839, 3021-3022 | Translatable to standard C# local variables |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| MESSAGE24 complexity (~1,068 lines, deep item-type nesting) | HIGH | HIGH | Decompose into sub-methods per item category |
| Constructor dependency explosion if not extracted | HIGH | MEDIUM | Extract WcCounterMessageTease as separate class following F808 pattern |
| INPUT loop translation errors (MESSAGE25/26) | MEDIUM | HIGH | Use IInputHandler + while-loop pattern; test each branch path |
| GETBIT/SETBIT off-by-one or name mapping errors | MEDIUM | MEDIUM | Map ERB bit names to named constants; cross-reference F808 |
| CFLAG constant mapping errors (30+ distinct names) | MEDIUM | MEDIUM | Derive constants from ERB source; verify against existing mappings |
| Large test surface (10 handlers, multiple branches each) | HIGH | MEDIUM | TDD RED->GREEN per handler; start with trivial MESSAGE29_1-3 |
| NTR state mutation complexity in MESSAGE26 (TALENT removal + INPUT) | MEDIUM | HIGH | Isolate NTR logic; test regain path independently |
| EQUIP variable pattern inconsistency | MEDIUM | LOW | Use ITEquipVariables consistently |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Dispatch stub count | Grep "return 0" in WcCounterMessage.cs actions 21-27, (29,1-3) | 10 stubs | All currently return 0 |
| IInputHandler usage in Counter | Grep "IInputHandler" in Counter/ | 0 references | First integration |
| WcCounterMessageTease existence | Glob WcCounterMessageTease.cs | Does not exist | New file to create |

**Baseline File**: `_out/tmp/baseline-807.txt` (generated at `/run` runtime)

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Each dispatch action (21-27, 29_1-3) must return 1 | ERB RETURN 1 at end of each function | AC must verify non-zero return for all 10 handlers |
| C2 | MESSAGE24 calls 14 distinct IWcCounterMessageItem methods (out of 17 total); MESSAGE25 reuses 5 and adds ReportSetItems (9 call sites, 15 distinct across TEASE) | TEASE.ERB:765-1837 (MESSAGE24), 1838-2522 (MESSAGE25) | AC must verify correct delegation per item type |
| C3 | MESSAGE25 and MESSAGE26 require IInputHandler integration | TEASE.ERB:2377-2468, 2549-2668 | AC must verify INPUT request/response handling and retry on invalid input |
| C4 | KOJO dispatch is TRYCALLFORM (no-op if absent) | 66+ KOJO calls throughout | AC must verify null-tolerant dispatch via IKojoMessageService |
| C5 | NTR revelation path shared across MESSAGE21, 25, 26 | TEASE.ERB:15, 1860, 2542 | AC must verify INtrRevelationHandler delegation |
| C6 | MESSAGE22 and MESSAGE23 mutate EQUIP state | TEASE.ERB:478-480, 757-760 | AC must verify equipment mutation via ITEquipVariables |
| C7 | MESSAGE26 can set/clear NTR TALENT | TEASE.ERB:2682 | AC must verify TALENT write (TALENT:MASTER:NTR=0) |
| C8 | MESSAGE27 clears bladder TCVAR | TEASE.ERB:3443 | AC must verify TCVAR write (bladder reset) |
| C9 | Equipment-description branching is pervasive (179 EQUIP reads) | Entire TEASE.ERB | ACs should test representative equipment combinations |
| C10 | Response category branching (WC_response_category) is primary dispatch | Used extensively throughout handlers | AC must verify response category constant is defined and used as branching variable (per-category behavioral distinctness deferred to F813 behavioral test scope) |
| C11 | IInputHandler is first usage in Counter subsystem | IInputHandler.cs; WcCounterMessage.cs:39-61 has no IInputHandler | AC must verify IInputHandler injection into WcCounterMessageTease constructor |
| C12 | WcCounterMessageTease must be registered in DI | F808 pattern (ServiceCollectionExtensions.cs:195-197) | AC must verify AddSingleton/AddTransient registration |
| C13 | All interfaces from Technical Design constructor spec must be injected | Technical Design: Interfaces/Data Structures (WcCounterMessageTease constructor, 11 parameters) | AC must verify constructor injection for each required interface |
| C14 | IWcCounterMessageNtr delegation: 13 OshikkoLooking calls in MESSAGE27 per character | TEASE.ERB:3284-3308 (13 character-specific TRYCALL) | AC must verify _ntr. call count matches 13 |
| C15 | _engine.GetResult() restricted to INPUT handlers only | AC#28 Design + Technical Design (INPUT loop section) | Only HandleMessage25 and HandleMessage26 call _engine.GetResult(); other 8 handlers do not read input results; AC#28 count_equals 2 depends on this |
| C16 | while-loop body must use break (not return 1) on valid input | AC#9 single-exit mandate + AC#33 while(true) count_equals 2 | MESSAGE25/26 while(true) loops must break on valid input and fall through to terminal return 1; `return 1;` inside loop body is prohibited (would inflate AC#9 count beyond 10) |

### Constraint Details

**C1: Dispatch Return Value**
- **Source**: Each of the 10 ERB functions ends with RETURN 1
- **Verification**: Grep for RETURN 1 at end of each MESSAGE function in TEASE.ERB
- **AC Impact**: Every handler method must return 1 (not 0 which is the current stub value)

**C2: MESSAGE24 Item Delegation**
- **Source**: TEASE.ERB:765-1837 (MESSAGE24) contains TRYCALL to 14 distinct IWcCounterMessageItem methods (16 call sites: RotorIn×2 and ClothOut×2 with different parameters account for the 2 extra call sites); MESSAGE25 (1838-2522) reuses 5 and adds ReportSetItems (9 call sites). Total: 25 call sites, 15 distinct methods out of 17.
- **Verification**: Cross-reference IWcCounterMessageItem.cs method list against TRYCALL targets
- **AC Impact**: AC must verify correct method is called for each item type branch

**C3: INPUT Loop Integration**
- **Source**: TEASE.ERB:2377-2468 (MESSAGE25) and 2549-2668 (MESSAGE26) use INPUT with GOTO retry
- **Verification**: Confirm IInputHandler.RequestNumericInput exists; confirm IEngineVariables.GetResult() exists (IEngineVariables.cs:14) for reading input results
- **AC Impact**: AC must verify INPUT request, valid input acceptance (0 or 1), and retry on invalid input

**C4: KOJO Null-Tolerant Dispatch**
- **Source**: 66+ TRYCALLFORM KOJO_WC_COUNTER* calls; TRYCALLFORM is no-op if function absent
- **Verification**: Confirm IKojoMessageService.KojoMessageWcCounter has default no-op implementation
- **AC Impact**: AC must verify dispatch does not throw when kojo handler is absent

**C5: NTR Revelation Delegation**
- **Source**: TRYCALL EVENT_WC_COUNTER_MESSAGE_NTRrevelation at TEASE.ERB:15, 1860, 2542
- **Verification**: Confirm INtrRevelationHandler.Execute exists in F808
- **AC Impact**: AC must verify delegation to NTR revelation handler in MESSAGE21, 25, 26

**C6: Equipment State Mutation**
- **Source**: MESSAGE22 (TEASE.ERB:478-480) and MESSAGE23 (TEASE.ERB:757-760) write EQUIP values
- **Verification**: Confirm ITEquipVariables.SetEquip/SetTEquip methods exist
- **AC Impact**: AC must verify equipment state changes after handler execution

**C7: NTR TALENT Removal**
- **Source**: MESSAGE26 (TEASE.ERB:2682) sets TALENT:MASTER:NTR=0
- **Verification**: Confirm IVariableStore.SetTalent method exists
- **AC Impact**: AC must verify TALENT write on NTR regain path

**C8: Bladder TCVAR Reset**
- **Source**: MESSAGE27 (TEASE.ERB:3443) clears bladder urine amount
- **Verification**: Confirm IVariableStore.SetTCVar method exists
- **AC Impact**: AC must verify TCVAR write for bladder reset

**C9: Equipment Description Branching**
- **Source**: 179 EQUIP reads across entire TEASE.ERB controlling output branching
- **Verification**: Count EQUIP references in ERB source
- **AC Impact**: ACs should cover representative equipment combinations (not exhaustive)

**C10: Response Category Dispatch**
- **Source**: WC_response_category used as primary branching variable in multiple handlers
- **Verification**: Grep for response category usage in TEASE.ERB
- **AC Impact**: AC must verify response category constant is defined and used as branching variable (per-category behavioral distinctness deferred to F813 behavioral test scope)

**C11: IInputHandler Injection**
- **Source**: IInputHandler.cs exists but WcCounterMessage.cs:39-61 does not include it
- **Verification**: Grep for IInputHandler in Counter/ directory (currently 0 references)
- **AC Impact**: WcCounterMessageTease must receive IInputHandler via constructor injection

**C12: DI Registration**
- **Source**: F808 registered its services in ServiceCollectionExtensions.cs:195-197
- **Verification**: Grep for AddSingleton pattern in ServiceCollectionExtensions.cs
- **AC Impact**: WcCounterMessageTease must have matching registration

**C13: Interface Injection Completeness**
- **Source**: Technical Design Interfaces/Data Structures — WcCounterMessageTease constructor (11 parameters: IVariableStore, IEngineVariables, ITEquipVariables, ITextFormatting, IConsoleOutput, IKojoMessageService, IRandomProvider, IInputHandler, IWcCounterMessageItem, IWcCounterMessageNtr, INtrRevelationHandler?)
- **Verification**: Each interface in constructor spec must appear as constructor parameter
- **AC Impact**: Every interface dependency must have injection verification AC

**C14: IWcCounterMessageNtr Delegation**
- **Source**: TEASE.ERB:3284-3308 — MESSAGE27 calls `EVENT_WC_COUNTER_MESSAGE_OSHIKKO_looking` 13 times (once per character)
- **Verification**: Count TRYCALL EVENT_WC_COUNTER_MESSAGE_OSHIKKO_looking in MESSAGE27
- **AC Impact**: AC#19 must verify count_equals 13 for `_ntr.` call sites

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| Predecessor | F805 | [DONE] | WC Counter Source + Message Core -- dispatch infrastructure, WcCounterMessage.cs |
| Predecessor | F808 | [DONE] | WC Counter Message ITEM + NTR -- IWcCounterMessageItem (17 methods), IWcCounterMessageNtr (6 methods); TRYCALL CALL from TEASE.ERB:15,823,837,1372-2225,1860,2542,3284-3308 |
| Successor | F813 | [DRAFT] | Post-Phase Review Phase 21 |
| Related | F806 | [PROPOSED] | WC Counter Message SEX -- sibling, same dispatch pattern, no cross-calls |

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
| "Phase 21 Counter System migration converts ERB counter message handlers to C# with structural migration equivalence" | WcCounterMessageTease class must exist implementing all 10 handlers | AC#1, AC#2, AC#3 |
| "Each ERB message category is migrated as a separate feature to maintain single-responsibility scope" | WcCounterMessageTease must be a NEW class separate from WcCounterMessage | AC#1 |
| "WC Counter Message TEASE is the canonical implementation of TEASE-category message handlers (interface extraction deferred to F813)" | DI-registered, dispatch stubs replaced with delegation, all dependencies injected | AC#2, AC#3, AC#4, AC#5 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | WcCounterMessageTease class exists as sealed class in Counter namespace | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | public sealed class WcCounterMessageTease | [x] |
| 2 | DI registration in ServiceCollectionExtensions | code | Grep(src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | AddSingleton.*WcCounterMessageTease | [x] |
| 3 | Dispatch stubs replaced with delegation (actions 21-27) | code | Grep(src/Era.Core/Counter/WcCounterMessage.cs) | count_equals | 7 | [x] |
| 4 | Branch dispatch stubs replaced with delegation (29_1-3) | code | Grep(src/Era.Core/Counter/WcCounterMessage.cs) | count_equals | 3 | [x] |
| 5 | Constructor injects IInputHandler (first Counter subsystem usage) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | contains | IInputHandler | [x] |
| 6 | Constructor injects IWcCounterMessageItem for MESSAGE24 delegation | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | contains | IWcCounterMessageItem | [x] |
| 7 | Constructor injects IWcCounterMessageNtr for NTR delegation (C14) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | contains | IWcCounterMessageNtr | [x] |
| 8 | Constructor injects INtrRevelationHandler for NTR revelation (C5) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | contains | INtrRevelationHandler | [x] |
| 9 | All 10 handler methods return 1 (C1) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 10 | [x] |
| 10 | IInputHandler used in MESSAGE25/MESSAGE26 for interactive input (C3) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 2 | [x] |
| 11 | IKojoMessageService injected for KOJO dispatch (C4) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | contains | IKojoMessageService | [x] |
| 12 | ITEquipVariables injected for EQUIP mutations (C6) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | contains | ITEquipVariables | [x] |
| 13 | Exactly 10 handler methods (MESSAGE21-27 + MESSAGE29_1-3) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 10 | [x] |
| 14 | Total _item. delegation call sites across all handlers (C2: MESSAGE24 16 sites + MESSAGE25 9 sites = 25) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 25 | [x] |
| 15 | Build succeeds with TreatWarningsAsErrors | build | dotnet build | succeeds | - | [x] |
| 16 | WC_response_category (CflagWcResponseCategory) defined and used in WcCounterMessageTease (C10) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | gte | 2 | [x] |
| 17 | SetTalent called for NTR TALENT removal in MESSAGE26 (C7) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | SetTalent | [x] |
| 18 | SetTCVar called for bladder reset in MESSAGE27 (C8) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | SetTCVar | [x] |
| 19 | IWcCounterMessageNtr delegation call sites in MESSAGE27 (C14: 13 OshikkoLooking calls) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 13 | [x] |
| 20 | IVariableStore injected (C13) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | contains | IVariableStore | [x] |
| 21 | IEngineVariables injected (C13) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | contains | IEngineVariables | [x] |
| 22 | ITextFormatting injected (C13) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | contains | ITextFormatting | [x] |
| 23 | IConsoleOutput injected (C13) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | contains | IConsoleOutput | [x] |
| 24 | IRandomProvider injected for PRINTDATA random text (C13) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | contains | IRandomProvider | [x] |
| 25 | INtrRevelationHandler actually called for NTR revelation path in MESSAGE21, 25, 26 (C5) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 3 | [x] |
| 26 | SetEquip/SetTEquip actually called for EQUIP mutation in MESSAGE22 AND MESSAGE23 (C6) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | gte | 2 | [x] |
| 27 | BitfieldUtility.SetBit called for SETBIT bitfield operations in MESSAGE24 | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | BitfieldUtility\\.SetBit | [x] |
| 28 | _engine.GetResult() called in MESSAGE25/MESSAGE26 INPUT loops to read input value (C3) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 2 | [x] |
| 29 | IKojoMessageService actually called for KOJO dispatch (C4) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | gte | 10 | [x] |
| 30 | WcCounterMessageTease is constructor-injected in WcCounterMessage (not directly instantiated) | code | Grep(src/Era.Core/Counter/WcCounterMessage.cs) | matches | WcCounterMessageTease tease | [x] |
| 31 | BitfieldUtility.GetBit called for GETBIT bitfield reads in MESSAGE24 | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | BitfieldUtility\\.GetBit | [x] |
| 32 | SetCharacterFlag called for CFLAG state mutations in handler bodies | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | SetCharacterFlag | [x] |
| 33 | while (true) INPUT retry loop exists in MESSAGE25 and MESSAGE26 (C3) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 2 | [x] |
| 34 | No private int-returning methods (instance or static) in WcCounterMessageTease (AC#9 invariant) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 0 | [x] |
| 35 | Excluded IWcCounterMessageItem methods (BottomClothOut, BottomClothOff) not called (C2) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 0 | [x] |
| 36 | GetEquip/GetTEquip called for EQUIP read operations driving equipment branching (C9) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | gte | 1 | [x] |
| 37 | ReportSetItems called exactly once in MESSAGE25 (C2 spot-check for method identity) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 1 | [x] |
| 38 | ClothOut called exactly twice in MESSAGE24 (C2 per-handler distribution anchor) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 2 | [x] |
| 39 | No early-exit return 0 in WcCounterMessageTease (AC#9 single-exit enforcement) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 0 | [x] |
| 40 | IRandomProvider actually called for MESSAGE21 PRINTDATA random text (C13) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | _randomProvider\\. | [x] |
| 41 | RotorIn called exactly twice in MESSAGE24 (C2 per-handler distribution anchor) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 2 | [x] |
| 42 | KojoActionMessage29_1 constant used in handler (AC#29 per-handler spot-check for branch actions) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | KojoActionMessage29_1 | [x] |
| 43 | Total _ntr. call sites bounded to 13 (OshikkoLooking only, no F806-scoped NTR methods called from TEASE) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 13 | [x] |
| 44 | KojoActionMessage29_2 constant used in handler (AC#29 branch-action spot-check) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | KojoActionMessage29_2 | [x] |
| 45 | KojoActionMessage29_3 constant used in handler (AC#29 branch-action spot-check) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | KojoActionMessage29_3 | [x] |
| 46 | IConsoleOutput actually called in handler bodies (injection+call-site pattern) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | gte | 10 | [x] |
| 47 | ITextFormatting actually called in handler bodies (injection+call-site pattern) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | _textFormatting\\. | [x] |
| 48 | KojoActionMessage21 constant used in handler (AC#29 main-action spot-check) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | KojoActionMessage21 | [x] |
| 49 | KojoActionMessage27 constant used in handler (AC#29 main-action spot-check) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | KojoActionMessage27 | [x] |
| 50 | InsertItemIn called in MESSAGE24 and MESSAGE25 (C2 method-identity anchor: 2 total call sites) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 2 | [x] |
| 51 | KojoActionMessage22 constant used in handler (AC#29 main-action spot-check) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | KojoActionMessage22 | [x] |
| 52 | KojoActionMessage23 constant used in handler (AC#29 main-action spot-check) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | KojoActionMessage23 | [x] |
| 53 | KojoActionMessage24 constant used in handler (AC#29 main-action spot-check) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | KojoActionMessage24 | [x] |
| 54 | KojoActionMessage25 constant used in handler (AC#29 main-action spot-check) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | KojoActionMessage25 | [x] |
| 55 | KojoActionMessage26 constant used in handler (AC#29 main-action spot-check) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | matches | KojoActionMessage26 | [x] |
| 56 | No public handler uses expression-body syntax (AC#9 invariant enforcement) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 0 | [x] |
| 57 | F813 updated with deferred obligation tracking items (Task 6 verification) | code | Grep(pm/features/feature-813.md) | contains | F807 Mandatory Handoffs | [x] |
| 58 | break; used in MESSAGE25/MESSAGE26 while-loop bodies for valid-input exit (C16 enforcement) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | gte | 2 | [x] |
| 59 | CliCapOut called exactly 3 times across MESSAGE24 (×1) and MESSAGE25 (×2) (C2 per-handler distribution anchor) | code | Grep(src/Era.Core/Counter/WcCounterMessageTease.cs) | count_equals | 3 | [x] |
| 60 | F813 handoff section contains constructor growth mitigation obligation (AC#57 content anchor) | code | Grep(pm/features/feature-813.md) | contains | コンストラクタ肥大化 | [x] |
| 61 | F813 handoff section contains AC#34 local function enforcement gap obligation (Task 6 item (f) anchor) | code | Grep(pm/features/feature-813.md) | contains | ローカル関数 | [x] |

### AC Details

**AC#1: WcCounterMessageTease class exists as sealed class in Counter namespace**
- **Test**: `Grep "public sealed class WcCounterMessageTease" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found -- class is sealed, in Counter namespace, in its own file
- **Rationale**: C1/C13 -- new class must be extracted (not merged into WcCounterMessage) to maintain single-responsibility. Follows F808 WcCounterMessageItem/WcCounterMessageNtr pattern.

**AC#2: DI registration in ServiceCollectionExtensions**
- **Test**: `Grep "AddSingleton.*WcCounterMessageTease" src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`
- **Expected**: Match found for AddSingleton registration
- **Rationale**: C12 -- all new Counter classes must be registered in DI composition root, following F808 pattern (ServiceCollectionExtensions.cs:195-197).

**AC#3: Dispatch stubs replaced with delegation (actions 21-27)**
- **Test**: `Grep "_tease\.HandleMessage2[1-7]" src/Era.Core/Counter/WcCounterMessage.cs` with count
- **Expected**: Exactly 7 matches (count_equals 7) -- one per action 21-27 replacing the stub `=> 0` lines
- **Rationale**: C1 -- all 7 dispatch stubs must delegate to WcCounterMessageTease. count_equals ensures every stub is replaced (not just one).

**AC#4: Branch dispatch stubs replaced with delegation (29_1-3)**
- **Test**: `Grep "_tease\.HandleMessage29_" src/Era.Core/Counter/WcCounterMessage.cs` with count
- **Expected**: Exactly 3 matches (count_equals 3) -- one per branch 29_1, 29_2, 29_3
- **Rationale**: C1 -- all 3 DispatchWithBranch() stubs must delegate. count_equals ensures every branch stub is replaced.

**AC#5: Constructor injects IInputHandler (first Counter subsystem usage)**
- **Test**: `Grep "IInputHandler" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found for constructor parameter and/or field assignment
- **Rationale**: C3/C11 -- IInputHandler is required for MESSAGE25/MESSAGE26 INPUT loops. This is the first usage of IInputHandler in the Counter subsystem (Baseline: 0 references).

**AC#6: Constructor injects IWcCounterMessageItem for MESSAGE24 delegation**
- **Test**: `Grep "IWcCounterMessageItem" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found for constructor parameter and/or field reference
- **Rationale**: C2 -- MESSAGE24 calls 14+ IWcCounterMessageItem methods for item-type branching. Interface must be injected.

**AC#7: Constructor injects IWcCounterMessageNtr for NTR delegation**
- **Test**: `Grep "IWcCounterMessageNtr" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found for constructor parameter and/or field reference
- **Rationale**: C14 -- IWcCounterMessageNtr is required for MESSAGE27's 13 OshikkoLooking delegation calls per character. Interface must be injected.

**AC#8: Constructor injects INtrRevelationHandler for NTR revelation (C5)**
- **Test**: `Grep "INtrRevelationHandler" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found for constructor parameter and/or field reference
- **Rationale**: C5 -- NTR revelation path shared across MESSAGE21, 25, 26 requires INtrRevelationHandler?.Execute() delegation (nullable, matching F805 pattern).

**AC#9: All 10 handler methods return 1 (C1)**
- **Test**: `Grep "return 1;" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 10 matches (count_equals 10). Each handler has exactly ONE `return 1;` at the end of its body. **Expression-body syntax (`=> 1`) is forbidden for handler methods** — all 10 handlers MUST use block-body with explicit `return 1;` statement. Expression-body form would not produce the `return 1;` token and cause AC#9 to under-count. **`return 1;` inside while-loop body is forbidden (C16)** — MESSAGE25/26 must use `break` on valid input and fall through to the single terminal `return 1;`. A `return 1;` inside the loop would inflate the count beyond 10.
- **Rationale**: C1 -- ERB functions all end with single RETURN 1 per function. The C# translation must maintain single-exit-point style: each of the 10 public handler methods (HandleMessage21-27, HandleMessage29_1/2/3) has exactly one `return 1;` as the terminal statement. Private helper methods MUST return void (not int) per Technical Design, so they cannot produce `return 1;` false positives. C16 ensures while-loop bodies in MESSAGE25/26 use break (not return) to exit, preserving the single-exit count invariant. This ensures count_equals 10 correctly verifies all 10 public handlers without false negatives from multiple return paths, expression-body syntax, or while-loop return statements.
- **Verification Prerequisites**: AC#9 count_equals 10 is only valid when AC#34 (no private int methods → 0 false-positive `return 1;`) and AC#56 (no expression-body → 0 false-negative from `=> 1`) both pass. Verify AC#34 and AC#56 before AC#9.

**AC#10: IInputHandler used in MESSAGE25/MESSAGE26 for interactive input (C3)**
- **Test**: `Grep "RequestNumericInput" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 2 matches — one `RequestNumericInput` call site in HandleMessage25 and one in HandleMessage26 (1 static call site per while-loop per handler = 2 total). Ensures BOTH handlers implement the INPUT loop, not just one.
- **Rationale**: C3 -- MESSAGE25 and MESSAGE26 have GOTO-based INPUT retry loops that must translate to IInputHandler.RequestNumericInput() calls with while-loop retry. count_equals 2 enforces completeness (both handlers), mirroring AC#14 and AC#19 precedent.

**AC#11: IKojoMessageService injected for KOJO dispatch (C4)**
- **Test**: `Grep "IKojoMessageService" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found for constructor parameter and/or field reference
- **Rationale**: C4 -- 66+ TRYCALLFORM KOJO calls require IKojoMessageService injection. Null-tolerant dispatch via default no-op implementation.

**AC#12: ITEquipVariables injected for EQUIP mutations (C6)**
- **Test**: `Grep "ITEquipVariables" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found for constructor parameter and/or field reference
- **Rationale**: C6 -- MESSAGE22 and MESSAGE23 mutate EQUIP state; 179 EQUIP reads across all handlers. ITEquipVariables must be injected.

**AC#13: Exactly 10 handler methods (MESSAGE21-27 + MESSAGE29_1-3)**
- **Test**: `Grep "public int HandleMessage" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 10 methods (HandleMessage21 through HandleMessage27 + HandleMessage29_1/2/3)
- **Rationale**: C1 -- all 10 dispatch stubs must have corresponding handler methods. Derivation: 7 direct (21-27) + 3 branch (29_1, 29_2, 29_3) = 10 methods.

**AC#14: Total _item. delegation call sites across all handlers (C2)**
- **Test**: `Grep "_item\." src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 25 matches — MESSAGE24 contributes 16 call sites (14 distinct methods: RotorIn×2 and ClothOut×2 with different parameters), MESSAGE25 contributes 9 call sites at TEASE.ERB lines 1999 (ItemWithOtherItem), 2006 (CliCapOut), 2008 (RotorPantsOut), 2027 (InsertItemOut), 2048 (InsertItemIn), 2151 (CliCapOut), 2154 (RotorPantsOut), 2180 (InsertItemOut), 2225 (ReportSetItems). IWcCounterMessageItem defines 17 methods total; TEASE uses 15 of them (BottomClothOut and BottomClothOff are not called). Each ERB TRYCALL maps 1:1 to one C# `_item.Method()` call per the Technical Design.
- **Rationale**: C2 requires verifying MESSAGE24 delegates to IWcCounterMessageItem methods. count_equals 25 verifies the complete delegation pattern across all handlers that use ITEM functions (MESSAGE24 + MESSAGE25), not just MESSAGE24 in isolation. The grep pattern `_item\.` matches method call sites only (not field declarations), providing an exact count of delegation invocations.

**AC#15: Build succeeds with TreatWarningsAsErrors**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build src/Era.Core/'`
- **Expected**: Exit code 0 (build succeeds with no warnings)
- **Rationale**: Directory.Build.props TreatWarningsAsErrors ensures all new code compiles warning-free.

**AC#16: WC_response_category (CflagWcResponseCategory) defined and used in WcCounterMessageTease (C10)**
- **Test**: `Grep "CflagWcResponseCategory" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: At least 2 matches (gte 2) — the constant declaration (`private const int CflagWcResponseCategory = 714;`) counts as 1 match, and at least 1 usage in handler branching logic (e.g., `_variables.GetCFlag(target, CflagWcResponseCategory)`) counts as 1+. gte 2 ensures the constant is both defined AND consumed, not just declared.
- **Rationale**: C10 requires that response category branching is implemented as the primary dispatch variable. The previous `matches` matcher could pass on the declaration line alone (false-pass risk). gte 2 distinguishes declaration-only from declaration+usage, confirming the constant is consumed in handler logic.

**AC#17: SetTalent called for NTR TALENT removal in MESSAGE26 (C7)**
- **Test**: `Grep "SetTalent" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found -- `SetTalent` is called in HandleMessage26 for NTR regain path (ERB: TALENT:MASTER:NTR=0, TEASE.ERB:2682).
- **Rationale**: C7 requires verifying TALENT write on the NTR regain path in MESSAGE26. Injection of IVariableStore (no separate AC) does not guarantee the mutation is performed. This AC verifies that SetTalent is actually called, not just that the interface is available.

**AC#18: SetTCVar called for bladder reset in MESSAGE27 (C8)**
- **Test**: `Grep "SetTCVar" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found -- `SetTCVar` is called in HandleMessage27 for bladder urine amount reset (ERB: TCVAR bladder, TEASE.ERB:3443).
- **Rationale**: C8 requires verifying TCVAR write for bladder reset in MESSAGE27. This AC verifies SetTCVar is actually called. Without this check, MESSAGE27 could compile with all interfaces injected but never perform the bladder reset mutation.

**AC#19: IWcCounterMessageNtr delegation call sites in MESSAGE27 (13 OshikkoLooking calls)**
- **Test**: `Grep "_ntr\.OshikkoLooking" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 13 matches — MESSAGE27 (lines 3284-3308) calls `_ntr.OshikkoLooking(characterId)` once for each of 13 characters (レミリア, パチュリー, フラン, 咲夜, 美鈴, 小悪魔, 子悪魔, チルノ, 大妖精, 魔理沙, 霊夢, ルーミア, アリス). IWcCounterMessageNtr defines 6 methods total; only OshikkoLooking is called from TEASE (other NTR methods are called from SEX/F806).
- **Rationale**: AC#7 verifies IWcCounterMessageNtr injection presence but not usage. The 33+ TRYCALL delegations to F808 NTR functions require `_ntr.{Method}()` calls in the implementation. This AC bridges the gap between injection and delegation.

**AC#20: IVariableStore injected (C13)**
- **Test**: `Grep "IVariableStore" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found for constructor parameter and/or field reference
- **Rationale**: C13 -- IVariableStore provides state read/write (SetTalent, SetTCVar, GetCFlag, etc.). Required for all state mutation operations.

**AC#21: IEngineVariables injected (C13)**
- **Test**: `Grep "IEngineVariables" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found for constructor parameter and/or field reference
- **Rationale**: C13 -- IEngineVariables provides engine state access (MASTER, TARGET, etc.).

**AC#22: ITextFormatting injected (C13)**
- **Test**: `Grep "ITextFormatting" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found for constructor parameter and/or field reference
- **Rationale**: C13 -- ITextFormatting provides text formatting operations (color, alignment, etc.).

**AC#23: IConsoleOutput injected (C13)**
- **Test**: `Grep "IConsoleOutput" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found for constructor parameter and/or field reference
- **Rationale**: C13 -- IConsoleOutput provides console rendering (PRINT/PRINTL equivalent).

**AC#24: IRandomProvider injected for PRINTDATA random text (C13)**
- **Test**: `Grep "IRandomProvider" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found for constructor parameter and/or field reference
- **Rationale**: C13 -- IRandomProvider provides random selection for MESSAGE21 PRINTDATA block (TEASE.ERB:124-127).

**AC#25: INtrRevelationHandler actually called for NTR revelation path in MESSAGE21, 25, 26 (C5)**
- **Test**: `Grep "_ntrRevelationHandler\\?" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 3 matches — one `_ntrRevelationHandler?.Execute()` call site in HandleMessage21 (TEASE.ERB:15), one in HandleMessage25 (TEASE.ERB:1860), and one in HandleMessage26 (TEASE.ERB:2542). The `\?` literal matches the null-conditional operator, distinguishing call sites from field declaration and constructor assignment.
- **Rationale**: C5 requires verifying INtrRevelationHandler delegation in all 3 handlers that share the NTR revelation path. AC#8 only checks injection presence. count_equals 3 ensures MESSAGE21, MESSAGE25, AND MESSAGE26 all delegate, not just one. Follows the AC#10 (count_equals 2), AC#14 (count_equals 25), AC#19 (count_equals 13) count-based precedent for multi-site delegations.

**AC#26: SetEquip/SetTEquip actually called for EQUIP mutation in MESSAGE22 AND MESSAGE23 (C6)**
- **Test**: `Grep "Set(T)?Equip" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: At least 2 matches (gte 2) — `SetEquip` or `SetTEquip` is called in BOTH HandleMessage22 (TEASE.ERB:478-480) AND HandleMessage23 (TEASE.ERB:757-760) for EQUIP state mutations. gte 2 ensures both handlers perform mutations, not just one.
- **Rationale**: C6 cites two distinct ERB locations requiring EQUIP writes. Previous `matches` matcher could pass with only one handler's mutations implemented while the other's were silently omitted. gte 2 enforces per-handler coverage consistent with C6's dual-handler scope. AC#12 verifies injection presence; this AC verifies the mutation is actually performed in both handlers.

**AC#27: BitfieldUtility.SetBit called for SETBIT bitfield operations in MESSAGE24**
- **Test**: `Grep "BitfieldUtility\\.SetBit" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — `BitfieldUtility.SetBit()` is called for SETBIT state mutations in MESSAGE24 (Technical Constraint: ~300 GETBIT/SETBIT operations).
- **Rationale**: Follows the AC#17 (SetTalent for C7), AC#18 (SetTCVar for C8), AC#26 (SetEquip for C6) precedent: every state mutation type in Technical Constraints has a dedicated call-site verification AC. Without this AC, MESSAGE24's bitfield mutations could be silently omitted.

**AC#28: _engine.GetResult() called in MESSAGE25/MESSAGE26 INPUT loops to read input value (C3)**
- **Test**: `Grep "_engine\\.GetResult" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 2 matches — one `_engine.GetResult()` call in HandleMessage25 and one in HandleMessage26. No private helper; handlers call `_engine.GetResult()` directly. Complements AC#10 (RequestNumericInput count_equals 2) by verifying the response-reading half of the INPUT contract.
- **Rationale**: C3 requires verifying INPUT request/response handling and retry on invalid input. AC#10 verifies the request side (RequestNumericInput). This AC verifies the response side: handlers call `_engine.GetResult()` inline to read the integer input result after RequestNumericInput, following the ShopSystem pattern where GetResult is a thin wrapper around engine vars. Without both ACs, an implementation could request input but never read the response.

**AC#29: IKojoMessageService actually called for KOJO dispatch (C4)**
- **Test**: `Grep "_kojoMessage\\." src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: At least 10 matches (gte 10) — Technical Design defines 10 KojoAction constants (KojoActionMessage21 through KojoActionMessage29_3), one per handler. Each handler calls `_kojoMessage.KojoMessageWcCounter()` at least once per the ERB source (66+ TRYCALLFORM KOJO calls across all handlers). gte 10 ensures every handler has at least one KOJO dispatch call. The exact count exceeds 10 because some handlers call KOJO multiple times (e.g., per response category branch), but the per-handler minimum of 10 is deterministic from the Technical Design's KojoAction constant table.
- **Rationale**: C4 requires verifying null-tolerant dispatch via IKojoMessageService. AC#11 only checks injection presence. Previous `matches` matcher was fragile: a single call site out of 66+ would pass. gte 10 enforces per-handler dispatch coverage consistent with the Technical Design's KojoAction constants, following the AC#14 (count_equals 25), AC#19 (count_equals 13), AC#25 (count_equals 3) count-based verification pattern.

**AC#30: WcCounterMessageTease is constructor-injected in WcCounterMessage (not directly instantiated)**
- **Test**: `Grep "WcCounterMessageTease tease" src/Era.Core/Counter/WcCounterMessage.cs`
- **Expected**: Match found — `WcCounterMessageTease tease` as constructor parameter. The pattern specifically matches the parameter declaration, not `new WcCounterMessageTease(...)` instantiation or field type references.
- **Rationale**: Key Decision Row 2 selects constructor parameter (DI-resolved) over direct instantiation. AC#3/AC#4 verify delegation call sites but not the injection pattern. Without this AC, an implementation could directly instantiate WcCounterMessageTease, bypassing DI and breaking the architecture.

**AC#31: BitfieldUtility.GetBit called for GETBIT bitfield reads in MESSAGE24**
- **Test**: `Grep "BitfieldUtility\\.GetBit" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — `BitfieldUtility.GetBit()` is called for GETBIT state reads in MESSAGE24 (Technical Constraint: ~300 GETBIT/SETBIT operations).
- **Rationale**: AC#27 covers the write path (SetBit). This AC covers the read path (GetBit) which drives all item-type branching decisions in MESSAGE24. Without GetBit, MESSAGE24's branching logic cannot function correctly. Follows AC#27 precedent for bitfield operation verification.

**AC#32: SetCharacterFlag called for CFLAG state mutations in handler bodies**
- **Test**: `Grep "SetCharacterFlag" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — `_variables.SetCharacterFlag()` or equivalent IVariableStore method is called for direct CFLAG state mutations in handler bodies (distinct from SETBIT bit-level writes covered by AC#27).
- **Rationale**: Problem states "100+ state mutations (CFLAG/EQUIP/TCVAR/TALENT)." EQUIP (AC#26), TCVAR (AC#18), TALENT (AC#17), and SETBIT (AC#27) all have dedicated call-site verification ACs. CFLAG direct writes (SetCharacterFlag via IVariableStore) lacked equivalent coverage. Follows AC#17/AC#18/AC#26/AC#27 mutation call-site verification pattern.

**AC#33: while (true) INPUT retry loop exists in MESSAGE25 and MESSAGE26 (C3)**
- **Test**: `Grep "while \\(true\\)" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 2 matches — one `while (true)` loop in HandleMessage25 and one in HandleMessage26. Each translates the ERB GOTO-based INPUT retry (TEASE.ERB:2468, 2668) to a C# while-loop per Technical Design mandate.
- **Rationale**: C3 requires verifying 'INPUT request/response handling and retry on invalid input.' AC#10 (RequestNumericInput count_equals 2) and AC#28 (_engine.GetResult count_equals 2) verify call-site existence but not the retry loop structure. An implementation without a while-loop could call RequestNumericInput and GetResult once without retrying, passing AC#10 and AC#28 while violating C3's retry requirement. count_equals 2 enforces both handlers implement the loop pattern.

**AC#34: No private int-returning methods (instance or static) in WcCounterMessageTease (AC#9 invariant)**
- **Test**: `Grep "private (static )?int \\w+\\(" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 0 matches — no private methods (instance or static) return int. Pattern `private (static )?int \w+\(` targets both `private int methodName(` and `private static int methodName(` declarations regardless of casing convention. `\w+` matches any method name (PascalCase or camelCase), eliminating the naming convention escape hatch. Excludes `private const int` (no parentheses), `private readonly int` (no parentheses), and plain `private int` fields (no `(`). All 10 public handler methods return int (verified by AC#9 count_equals 10 and AC#13 count_equals 10). Private helper methods (any handler decomposition per Technical Design) MUST return void. count_equals 0 prevents private int helpers from inflating AC#9's `return 1;` count.
- **Rationale**: Technical Design mandates "Private helper methods (returning void) are permitted within HandleMessage24." AC#9 detail states "Private helper methods MUST return void (not int) per Technical Design, so they cannot produce `return 1;` false positives." This constraint was documented but not enforced by any AC. AC#34 makes the invariant mechanically verifiable. The `(static )?` group covers both instance and static private methods. **Limitation**: AC#34 cannot detect C# local functions (which lack the `private` keyword). Technical Design explicitly prohibits local functions in WcCounterMessageTease to close this gap.

**AC#35: Excluded IWcCounterMessageItem methods (BottomClothOut, BottomClothOff) not called (C2)**
- **Test**: `Grep "_item\\.BottomCloth" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 0 matches — C2 Constraint Detail states "IWcCounterMessageItem defines 17 methods total; TEASE uses 15 of them (BottomClothOut and BottomClothOff are not called)." Pattern `_item\.BottomCloth` matches any `_item.BottomClothOut(` or `_item.BottomClothOff(` call site. count_equals 0 ensures neither excluded method is called.
- **Rationale**: AC#14 (count_equals 25) verifies the total `_item.` call count but not which methods are called. An implementation calling BottomClothOut 25 times would pass AC#14 while violating structural migration equivalence. AC#35 closes the method identity gap by enforcing the C2 exclusion invariant. Combined with AC#14, this bounds both total count (25) and method identity (no excluded methods).

**AC#36: GetEquip/GetTEquip called for EQUIP read operations driving equipment branching (C9)**
- **Test**: `Grep "Get(T)?Equip" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: At least 1 match (gte 1) — `GetEquip` or `GetTEquip` is called to read equipment state for branching decisions. C9 documents 179 EQUIP reads across the entire TEASE.ERB controlling output branching. The exact count is not deterministic from spec (varies by handler decomposition), but gte 1 ensures the read path exists.
- **Rationale**: AC#12 verifies ITEquipVariables injection (contains). AC#26 verifies SetEquip/SetTEquip write calls (gte 2). The read path — which drives 179 branching decisions — was unverified at the call-site level. An implementation could inject ITEquipVariables and perform writes without ever reading equipment state, silently omitting all equipment-description branching. Follows AC#26 (write) / AC#31 (GetBit read) / AC#27 (SetBit write) paired read/write verification pattern.

**AC#37: ReportSetItems called exactly once in MESSAGE25 (C2 spot-check for method identity)**
- **Test**: `Grep "_item\\.ReportSetItems" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 1 match (count_equals 1) — `_item.ReportSetItems()` is called once in HandleMessage25 (TEASE.ERB:2225). ReportSetItems is unique to MESSAGE25 (not called in MESSAGE24 or any other handler). It is the one method in MESSAGE25's 9 call sites that is NOT shared with MESSAGE24.
- **Rationale**: AC#14 (count_equals 25) + AC#35 (no BottomCloth) bound total count and excluded methods, but do not verify which 15 distinct methods are called. A spot-check for ReportSetItems (unique to MESSAGE25, 1 of 15 distinct methods) anchors method identity verification. Combined with AC#14 and AC#35, this significantly constrains the implementation toward correct method dispatch while avoiding full enumeration of all 15 methods.

**AC#38: ClothOut called exactly twice in MESSAGE24 (C2 per-handler distribution anchor)**
- **Test**: `Grep "_item\\.ClothOut" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 2 matches (count_equals 2) — C2 Constraint Detail documents "ClothOut×2 with different parameters" in MESSAGE24. ClothOut is called exclusively in MESSAGE24 (not in MESSAGE25's 9 call sites). count_equals 2 anchors MESSAGE24's per-handler call-site distribution.
- **Rationale**: AC#14 (count_equals 25) verifies total _item. call sites but not per-handler distribution. AC#37 anchors MESSAGE25 (ReportSetItems×1). AC#38 anchors MESSAGE24 (ClothOut×2). Together with AC#35 (no BottomCloth), these spot-checks verify per-handler method identity without full enumeration: MESSAGE24 has ClothOut×2 (AC#38), MESSAGE25 has ReportSetItems×1 (AC#37), neither has BottomCloth (AC#35), and total is 25 (AC#14).

**AC#39: No early-exit return 0 in WcCounterMessageTease (AC#9 single-exit enforcement)**
- **Test**: `Grep "return 0;" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 0 matches (count_equals 0) — no `return 0;` statement exists in WcCounterMessageTease.cs. All 10 handlers use `return 1;` as the single terminal statement (AC#9). Private helpers return void (AC#34). An early `return 0;` would be a guard-exit violating the single-exit mandate and producing wrong output.
- **Rationale**: AC#9 (count_equals 10 for `return 1;`) verifies the presence of correct return values but does not verify the absence of incorrect early-exit returns. An implementer could add `return 0;` in a while-loop error path (MESSAGE25/26) or a branch guard (MESSAGE24), passing AC#9 while violating single-exit semantics. The original stubs returned 0 (`=> 0` in WcCounterMessage.cs), making `return 0` a likely copy-paste error. AC#39 mechanically enforces the prohibition.

**AC#40: IRandomProvider actually called for MESSAGE21 PRINTDATA random text (C13)**
- **Test**: `Grep "_randomProvider\\." src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — `_randomProvider.Next()` or equivalent call site in HandleMessage21 for PRINTDATA random text selection (TEASE.ERB:124-127).
- **Rationale**: AC#24 verifies IRandomProvider injection (contains) but not usage. Every other state-mutating interface has both injection and call-site verification ACs: SetTalent (AC#17), SetTCVar (AC#18), SetEquip (AC#26), SetCharacterFlag (AC#32), GetBit/SetBit (AC#31/AC#27), _ntr (AC#19), _ntrRevelationHandler (AC#25), _kojoMessage (AC#29), _item (AC#14), _engine.GetResult (AC#28). IRandomProvider was the only injected interface lacking call-site verification. AC#40 closes this gap.

**AC#41: RotorIn called exactly twice in MESSAGE24 (C2 per-handler distribution anchor)**
- **Test**: `Grep "_item\\.RotorIn" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 2 matches (count_equals 2) — C2 Constraint Detail documents "RotorIn×2 with different parameters" in MESSAGE24. RotorIn is called exclusively in MESSAGE24 (not in MESSAGE25's 9 call sites). count_equals 2 anchors MESSAGE24's per-handler call-site distribution.
- **Rationale**: AC#14 (count_equals 25) verifies total _item. call sites but not per-handler distribution. AC#37 anchors MESSAGE25 (ReportSetItems×1). AC#38 anchors MESSAGE24 (ClothOut×2). AC#41 adds a second MESSAGE24 anchor (RotorIn×2), further constraining method identity. Combined with AC#35 (no BottomCloth), these 4 spot-checks verify method identity without full enumeration: MESSAGE24 has ClothOut×2 (AC#38) and RotorIn×2 (AC#41), MESSAGE25 has ReportSetItems×1 (AC#37), neither has BottomCloth (AC#35), and total is 25 (AC#14).

**AC#42: KojoActionMessage29_1 constant used in handler (AC#29 per-handler spot-check)**
- **Test**: `Grep "KojoActionMessage29_1" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — `KojoActionMessage29_1` is referenced in HandleMessage29_1 for KOJO dispatch. The Technical Design Kojo action constants table defines KojoActionMessage29_1 (ERB action ID 29 branch 1) for HandleMessage29_1.
- **Rationale**: AC#29 (gte 10 for `_kojoMessage.`) verifies a file-wide call count but cannot guarantee per-handler dispatch. MESSAGE29_1-3 are "branch actions" with shorter ERB bodies; an implementation could omit KOJO calls from these handlers while compensating with extra calls in MESSAGE21-27. AC#42 spot-checks that at least one MESSAGE29_* handler uses its KojoAction constant, anchoring per-handler dispatch for the branch action group. Combined with AC#29's file-wide gte 10, this ensures both total coverage and branch-action participation.

**AC#43: Total _ntr. call sites bounded to 13 (OshikkoLooking only)**
- **Test**: `Grep "_ntr\\." src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 13 matches — the total `_ntr.` call count equals the OshikkoLooking-specific count (AC#19 count_equals 13). This bounds all `_ntr.` calls to OshikkoLooking, preventing any other IWcCounterMessageNtr method (WithPetting, RotorOut, etc.) from being called from TEASE.
- **Rationale**: IWcCounterMessageNtr defines 6 methods; AC#19 detail states "only OshikkoLooking is called from TEASE (other NTR methods are called from SEX/F806)." AC#19 verifies the specific method and count but does not prevent additional `_ntr.` calls to other methods. This gap is the exact analogue of the IWcCounterMessageItem exclusion pattern: AC#14 bounds total `_item.` calls to 25, and AC#35 excludes BottomClothOut/BottomClothOff. AC#43 bounds total `_ntr.` calls to 13 (matching AC#19), ensuring no F806-scoped NTR methods leak into TEASE handlers. Pattern: AC#14+AC#35 (item exclusion) mirrors AC#19+AC#43 (ntr exclusion).

**AC#44: KojoActionMessage29_2 constant used in handler (AC#29 branch-action spot-check)**
- **Test**: `Grep "KojoActionMessage29_2" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — `KojoActionMessage29_2` is referenced in HandleMessage29_2 for KOJO dispatch.
- **Rationale**: AC#42 spot-checks MESSAGE29_1 only. AC#44 extends branch-action coverage to MESSAGE29_2. Combined with AC#42 (29_1) and AC#45 (29_3), all 3 branch-action handlers are verified to participate in KOJO dispatch, closing the per-handler distribution gap for the branch-action group.

**AC#45: KojoActionMessage29_3 constant used in handler (AC#29 branch-action spot-check)**
- **Test**: `Grep "KojoActionMessage29_3" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — `KojoActionMessage29_3` is referenced in HandleMessage29_3 for KOJO dispatch.
- **Rationale**: Completes branch-action KOJO coverage alongside AC#42 (29_1) and AC#44 (29_2). All 3 branch-action handlers now have KOJO dispatch verification.

**AC#46: IConsoleOutput actually called in handler bodies (injection+call-site pattern)**
- **Test**: `Grep "_console\\." src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: At least 10 matches (gte 10) — every handler produces PRINT output per the ERB source; minimum 1 `_console.` call per handler.
- **Rationale**: AC#23 verifies IConsoleOutput injection (contains) but not usage. The injection→call-site pairing is established for all other output/mutation interfaces (AC#40 for IRandomProvider, AC#29 for IKojoMessageService, AC#14 for IWcCounterMessageItem, etc.). IConsoleOutput was the only injected output interface lacking call-site verification. An implementation could inject IConsoleOutput and never invoke it, producing no visible output while passing all other ACs.

**AC#47: ITextFormatting actually called in handler bodies (injection+call-site pattern)**
- **Test**: `Grep "_textFormatting\\." src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — `_textFormatting.` is called in at least one handler for text formatting operations.
- **Rationale**: AC#22 verifies ITextFormatting injection (contains) but not usage. Completes the injection→call-site pairing alongside AC#46 (IConsoleOutput). ITextFormatting drives color, alignment, and formatting operations in the ERB source.

**AC#48: KojoActionMessage21 constant used in handler (AC#29 main-action spot-check)**
- **Test**: `Grep "KojoActionMessage21" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — `KojoActionMessage21` is referenced in HandleMessage21 for KOJO dispatch.
- **Rationale**: AC#42/44/45 spot-check KojoAction constants for MESSAGE29_1/2/3 (branch actions). AC#48 extends coverage to the main-action group by anchoring MESSAGE21, the first handler. Combined with AC#49 (MESSAGE27), this brackets the main-action range.

**AC#49: KojoActionMessage27 constant used in handler (AC#29 main-action spot-check)**
- **Test**: `Grep "KojoActionMessage27" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — `KojoActionMessage27` is referenced in HandleMessage27 for KOJO dispatch.
- **Rationale**: Completes main-action range anchoring alongside AC#48 (MESSAGE21). MESSAGE27 is the highest-complexity main handler (131 character references, 13 OshikkoLooking calls). Combined with AC#42/44/45 (branch actions) and AC#48 (MESSAGE21), 5 of 10 handlers now have individual KOJO constant verification.

**AC#50: InsertItemIn called in MESSAGE24 and MESSAGE25 (C2 method-identity anchor: 2 total call sites)**
- **Test**: `Grep "_item\\.InsertItemIn" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 2 matches (count_equals 2) — `_item.InsertItemIn()` is called once in HandleMessage24 (TEASE.ERB:1466) and once in HandleMessage25 (TEASE.ERB:2048). InsertItemIn is shared between MESSAGE24 and MESSAGE25.
- **Rationale**: AC#37 (ReportSetItems×1) anchors one MESSAGE25-unique method. AC#50 verifies InsertItemIn is called in both handlers where the ERB source requires it. Together with AC#14 (total 25), AC#35 (no BottomCloth), AC#37 (ReportSetItems×1), AC#38 (ClothOut×2), AC#41 (RotorIn×2), and AC#50 (InsertItemIn×2), the spot-check coverage accounts for method-specific identity verification across MESSAGE24 and MESSAGE25.

**AC#51: KojoActionMessage22 constant used in handler (AC#29 main-action spot-check)**
- **Test**: `Grep "KojoActionMessage22" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — KojoActionMessage22 referenced in HandleMessage22.
- **Rationale**: Part of per-handler KojoAction verification set (AC#42/44/45/48/49/51-55) ensuring all 10 handlers use their named KOJO constant.

**AC#52: KojoActionMessage23 constant used in handler (AC#29 main-action spot-check)**
- **Test**: `Grep "KojoActionMessage23" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — KojoActionMessage23 referenced in HandleMessage23.
- **Rationale**: Part of per-handler KojoAction verification set.

**AC#53: KojoActionMessage24 constant used in handler (AC#29 main-action spot-check)**
- **Test**: `Grep "KojoActionMessage24" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — KojoActionMessage24 referenced in HandleMessage24.
- **Rationale**: Part of per-handler KojoAction verification set.

**AC#54: KojoActionMessage25 constant used in handler (AC#29 main-action spot-check)**
- **Test**: `Grep "KojoActionMessage25" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — KojoActionMessage25 referenced in HandleMessage25.
- **Rationale**: Part of per-handler KojoAction verification set.

**AC#55: KojoActionMessage26 constant used in handler (AC#29 main-action spot-check)**
- **Test**: `Grep "KojoActionMessage26" src/Era.Core/Counter/WcCounterMessageTease.cs`
- **Expected**: Match found — KojoActionMessage26 referenced in HandleMessage26.
- **Rationale**: Completes per-handler KojoAction constant verification for all 10 handlers. Combined with AC#42/44/45 (branch actions) and AC#48/49 (main-action brackets), all 10 Technical Design KojoAction constants now have individual ACs.

**AC#56: No public handler uses expression-body syntax (AC#9 invariant enforcement)**
- **Test**: `Grep "public int HandleMessage.*=>" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 0 matches — no public handler method uses expression-body syntax (`=> 1`). All 10 handlers MUST use block-body with explicit `return 1;` statement per AC#9's expression-body prohibition.
- **Rationale**: AC#9 counts `return 1;` tokens (count_equals 10). Expression-body `=> 1` does not produce a `return 1;` token, causing AC#9 to under-count. The prohibition is documented in AC#9 detail text but was not mechanically verifiable by any AC. AC#56 enforces the prohibition directly, making AC#9's count_equals 10 invariant reliable regardless of implementer style preferences.

**AC#57: F813 updated with deferred obligation tracking items (Task 6 verification)**
- **Test**: `Grep "F807 Mandatory Handoffs" pm/features/feature-813.md`
- **Expected**: Match found — F813 contains a dedicated "F807 Mandatory Handoffs" section header, confirming Task 6 transferred the handoff content. The pattern "F807 Mandatory Handoffs" matches the actual section header in F813 (line 51).
- **Rationale**: Task 6 transfers 6 deferred obligations to F813: (a) behavioral test coverage for WcCounterMessageTease handlers, (b) IWcCounterMessageTease interface extraction decision, (c) character ID constant consolidation, (d) WcCounterMessage constructor growth mitigation, (e) CFlag/Cflag naming normalization, (f) AC#34 local function enforcement gap. AC#57 verifies the dedicated section exists. Pattern corrected from "F807 Handoffs" to "F807 Mandatory Handoffs" to match F813's actual section header format.

**AC#58: break; used in MESSAGE25/MESSAGE26 while-loop bodies for valid-input exit (C16 enforcement)**
- **Test**: `Grep "break;" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: At least 2 matches (gte 2) — at minimum one `break;` in HandleMessage25's while(true) loop and one in HandleMessage26's while(true) loop. Additional `break;` statements from switch/case in other handlers (e.g., MESSAGE24 item-type branching) are expected and do not violate C16.
- **Rationale**: C16 mandates that while-loop bodies use `break` (not `return 1;`) to exit on valid input. AC#33 verifies `while (true)` count_equals 2 but not the exit mechanism. gte 2 ensures at least 2 break; exist (one per INPUT handler) while allowing additional break; from switch/case statements across 10 handlers spanning ~3,483 ERB lines. count_equals 2 was infeasible because `break;` is common in switch/case constructs throughout MESSAGE24's deep item-type nesting.

**AC#59: CliCapOut called exactly 3 times across MESSAGE24 and MESSAGE25 (C2 per-handler distribution anchor)**
- **Test**: `Grep "_item\\.CliCapOut" src/Era.Core/Counter/WcCounterMessageTease.cs` with count
- **Expected**: Exactly 3 matches (count_equals 3) — CliCapOut called ×1 in HandleMessage24 (one item-type branch) and ×2 in HandleMessage25 (TEASE.ERB:2006 and TEASE.ERB:2151).
- **Rationale**: C2 per-handler distribution anchor following AC#38 (ClothOut×2) and AC#41 (RotorIn×2) pattern. CliCapOut spans both MESSAGE24 and MESSAGE25 handlers (unlike ClothOut/RotorIn which are MESSAGE24-only), anchoring cross-handler method identity verification. Combined with AC#14 (total 25), AC#35 (no BottomCloth), AC#37 (ReportSetItems×1), AC#38 (ClothOut×2), AC#41 (RotorIn×2), and AC#50 (InsertItemIn×2), this completes per-handler _item. distribution coverage.

**AC#60: F813 handoff section contains constructor growth mitigation obligation (AC#57 content anchor)**
- **Test**: `Grep "コンストラクタ肥大化" pm/features/feature-813.md`
- **Expected**: Match found — F813's "F807 Mandatory Handoffs" section contains "コンストラクタ肥大化" (constructor bloat), confirming obligation (d) was actually written (not just an empty section header).
- **Rationale**: AC#57 verifies section header existence only. An empty section would pass AC#57 without transferring any of the 6 obligations. AC#60 anchors on obligation (d) "WcCounterMessageコンストラクタ肥大化対策" as the content verification. Pattern uses Japanese "コンストラクタ肥大化" to match F813's actual text (line 55).

**AC#61: F813 handoff section contains AC#34 local function enforcement gap obligation (Task 6 item (f) anchor)**
- **Test**: `Grep "ローカル関数" pm/features/feature-813.md`
- **Expected**: Match found — F813's "F807 Mandatory Handoffs" section contains "ローカル関数" (local function), confirming obligation (f) was written.
- **Rationale**: AC#57 verifies section header, AC#60 verifies obligation (d). Obligation (f) "AC#34 local function enforcement gap" was added to Mandatory Handoffs but had no content-level AC. F813's existing 5 items (lines 52-56) pre-date obligation (f); without AC#61, Task 6 can complete without writing obligation (f) to F813.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Implement all 10 TEASE message handlers (MESSAGE21-27, MESSAGE29_1-3) as a new WcCounterMessageTease class | AC#1, AC#13 |
| 2 | Replace dispatch stubs in WcCounterMessage.cs with delegation calls | AC#3, AC#4, AC#30 |
| 3 | TEASE-category counter message handlers structurally migrated with correct call-site counts and state mutation interfaces | AC#9, AC#6, AC#7, AC#12, AC#14, AC#17, AC#18, AC#19, AC#20, AC#21, AC#22, AC#23, AC#24, AC#26, AC#27, AC#31, AC#32, AC#34, AC#35, AC#36, AC#37, AC#38, AC#39, AC#40, AC#41, AC#43, AC#46, AC#47, AC#50, AC#59 |
| 4 | Register the new class in DI | AC#2 |
| 5 | Integrate IInputHandler for MESSAGE25/MESSAGE26 interactive loops | AC#5, AC#10, AC#28, AC#33, AC#58 |
| 6 | Delegate to INtrRevelationHandler for NTR revelation path | AC#8, AC#25 |
| 7 | KOJO dispatch via IKojoMessageService | AC#11, AC#29, AC#42, AC#44, AC#45, AC#48, AC#49, AC#51, AC#52, AC#53, AC#54, AC#55 |
| 8 | Build succeeds (TreatWarningsAsErrors) | AC#15 |
| 9 | Response category branching (WC_response_category) implemented as primary dispatch variable (C10) | AC#16 |
| 10 | Handler method style enforcement (block-body, no expression-body) | AC#56 |
| 11 | Deferred obligation transfer to F813 | AC#57, AC#60, AC#61 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

One new sealed class is created in `Era.Core/Counter/`: `WcCounterMessageTease`, following the F808 extraction pattern (WcCounterMessageItem / WcCounterMessageNtr). It implements all 10 TEASE handler methods. `WcCounterMessage.Dispatch()` and `DispatchWithBranch()` are updated to delegate to the new class instead of returning 0.

**Class decomposition:**

1. `WcCounterMessageTease` — migrates all 10 handler functions from TOILET_COUNTER_MESSAGE_TEASE.ERB (MESSAGE21-27, MESSAGE29_1-3). Receives all required interfaces via constructor injection. Each handler returns `int` (1 on success, matching ERB `RETURN 1`).

2. `WcCounterMessage` (modified) — Dispatch() switch cases 21-27 and DispatchWithBranch() cases (29,1-3) are changed from `=> 0` to `=> _tease.HandleMessage2N()` / `=> _tease.HandleMessage29_N()`. A private `readonly WcCounterMessageTease _tease` field is added to the class and injected via the constructor.

**No new interface for WcCounterMessageTease:** F808's ITEM and NTR classes required interfaces because they are called by multiple sibling features via TRYCALL. WcCounterMessageTease is called only from WcCounterMessage.Dispatch() -- there is no cross-feature injection need. WcCounterMessage holds the concrete type directly (same pattern as WcCounterReaction and WcCounterPunishment). This avoids unnecessary interface proliferation.

**WcCounterMessage constructor growth tracking:**
After F807 (11 params) and F806/SEX (12 params), WcCounterMessage.cs reaches the practical constructor parameter limit. Mitigation options (grouping message-category handlers into a dispatching service, or parameter object pattern) are tracked for F813 Phase 21 review scope. F807 and F806 follow the established concrete-type pattern (WcCounterReaction, WcCounterPunishment) and do not change the architecture.

**IInputHandler integration (first Counter usage):**
MESSAGE25 and MESSAGE26 each contain an ERB GOTO-based INPUT retry loop. The C# translation uses a standard `while (true)` pattern calling `_inputHandler.RequestNumericInput(prompt)`, then calling `_engine.GetResult()` to read the integer result. On valid input (0 or 1), execution continues. On invalid input, the loop retries. This mirrors the established Shop subsystem pattern (ShopSystem.cs uses RequestNumericInput at lines 188, 206, 246, 304).

**MESSAGE24 decomposition strategy:**
MESSAGE24 is ~1,068 ERB lines with deep item-type nesting. It is implemented as `HandleMessage24()` with delegation to `_item.{Method}(offender)` for each item-type branch, following the pattern the ERB source already establishes via TRYCALL into the ITEM file. Private helper methods (returning void) are permitted within any handler to decompose complex logic into manageable blocks (e.g., per item category in MESSAGE24 as identified in the Risk mitigation). **All private methods in WcCounterMessageTease MUST return void** — no private method (instance or static) may return int (enforced by AC#34). **Local functions (C# local functions declared inside method bodies) are also prohibited** — local functions lack the `private` keyword and would evade AC#34's pattern. All helper decomposition must use private void instance methods. The 10 public handler methods are the only int-returning methods; each retains a single `return 1;` at the end. AC#9 count_equals 10 applies only to the 10 public handler methods.

**NTR revelation path (MESSAGE21, MESSAGE25, MESSAGE26):**
These handlers check for the NTR revelation path and call `_ntrRevelationHandler?.Execute(target, action)` using nullable dispatch, matching the TRYCALL no-op semantics from the ERB source (TEASE.ERB:15, 1860, 2542). This is the same pattern used by WcCounterMessage.HandleMessage13().

**PRINTDATA random text (MESSAGE21):**
MESSAGE21 uses PRINTDATA for random text selection (TEASE.ERB:124-127). The C# equivalent uses `IRandomProvider` to select a block index, then `IConsoleOutput` for output. This follows the F808 WcCounterMessageNtr PRINTDATA pattern.

**EQUIP mutations (MESSAGE22, MESSAGE23):**
TEASE.ERB:478-480 and 757-760 write EQUIP values. The C# equivalent calls `_tEquipVariables.SetEquip()` / `_tEquipVariables.SetTEquip()`. ITEquipVariables is already injected in the same set of interfaces required by F808 classes.

**TALENT write (MESSAGE26):**
MESSAGE26 sets `TALENT:MASTER:NTR=0` on the NTR regain path (TEASE.ERB:2682). The C# equivalent calls `_variables.SetTalent(master, new TalentIndex(TalentNtr), 0)`.

**TCVAR reset (MESSAGE27):**
MESSAGE27 clears bladder urine amount (TEASE.ERB:3443). The C# equivalent calls `_variables.SetTCVar(offender, TCVarIndex.BladderAmount, 0)` or equivalent TCVarIndex constant.

**GETBIT/SETBIT bitfield operations (~300 in MESSAGE24):**
Mapped to `BitfieldUtility.GetBit()` / `BitfieldUtility.SetBit()` with named constants for each bit name, following the F808 WcCounterMessageNtr withPETTING pattern.

**Character ID constants (MESSAGE27 -- 131 character references):**
MESSAGE27 branches per character. Character IDs are defined as private int constants in WcCounterMessageTease following the naming convention established in F808 sibling classes (`private const int CflagWcResponseCategory = 714;` etc.).

**Namespace and file layout:**
```
Era.Core/Counter/
  WcCounterMessageTease.cs       (new sealed class)
  WcCounterMessage.cs            (modified: add _tease field, update Dispatch/DispatchWithBranch)
Era.Core/DependencyInjection/
  ServiceCollectionExtensions.cs (modified: add AddSingleton<WcCounterMessageTease>)
```

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core/Counter/WcCounterMessageTease.cs` with `public sealed class WcCounterMessageTease` |
| 2 | Add `services.AddSingleton<WcCounterMessageTease>()` in ServiceCollectionExtensions.cs (Phase 21 - Feature 807 comment block) |
| 3 | In WcCounterMessage.Dispatch(), replace `21 => 0` through `27 => 0` with `21 => _tease.HandleMessage21()` through `27 => _tease.HandleMessage27()`; add `private readonly WcCounterMessageTease _tease` field |
| 4 | In WcCounterMessage.DispatchWithBranch(), replace `(29, 1) => 0`, `(29, 2) => 0`, `(29, 3) => 0` with `(29, 1) => _tease.HandleMessage29_1()`, etc. |
| 5 | Declare `IInputHandler` constructor parameter and `_inputHandler` private readonly field in WcCounterMessageTease |
| 6 | Declare `IWcCounterMessageItem` constructor parameter and `_item` private readonly field in WcCounterMessageTease |
| 7 | Declare `IWcCounterMessageNtr` constructor parameter and `_ntr` private readonly field in WcCounterMessageTease |
| 8 | Declare `INtrRevelationHandler?` constructor parameter and `_ntrRevelationHandler` private readonly field in WcCounterMessageTease |
| 9 | Every handler method body ends with `return 1;`; `Grep "return 1;"` must yield exactly 10 matches |
| 10 | In HandleMessage25() and HandleMessage26(), translate GOTO INPUT_LOOP to `while (true)` using `_inputHandler.RequestNumericInput(prompt)`; `Grep "RequestNumericInput"` must yield exactly 2 matches (1 per handler) |
| 11 | Declare `IKojoMessageService` constructor parameter and `_kojoMessage` private readonly field in WcCounterMessageTease |
| 12 | Declare `ITEquipVariables` constructor parameter and `_tEquipVariables` private readonly field in WcCounterMessageTease |
| 13 | Implement all 10 public methods: HandleMessage21 through HandleMessage27, HandleMessage29_1, HandleMessage29_2, HandleMessage29_3 |
| 14 | In HandleMessage24 (16 sites) and HandleMessage25 (9 sites), delegate to `_item.{Method}()` for each TRYCALL; `Grep "_item\."` in WcCounterMessageTease.cs must yield exactly 25 matches |
| 15 | All new code in Era.Core compiles under `dotnet build src/Era.Core/` with TreatWarningsAsErrors=true; exit code 0 |
| 16 | Define `private const int CflagWcResponseCategory = 714;` and reference it in at least one handler branching call (e.g., `_variables.GetCFlag(target, CflagWcResponseCategory)`); `Grep "CflagWcResponseCategory"` must find at least 2 matches (1 declaration + 1+ usage) |
| 17 | In HandleMessage26, call `_variables.SetTalent(master, TalentNtr, 0)` for NTR regain path; `Grep "SetTalent"` must find at least one match |
| 18 | In HandleMessage27, call `_variables.SetTCVar(offender, TCVarIndex.BladderAmount, 0)` for bladder reset; `Grep "SetTCVar"` must find at least one match |
| 19 | In HandleMessage27, call `_ntr.OshikkoLooking(characterId)` for 13 characters; `Grep "_ntr\.OshikkoLooking"` must yield exactly 13 matches |
| 20 | Declare `IVariableStore` constructor parameter and `_variables` private readonly field |
| 21 | Declare `IEngineVariables` constructor parameter and `_engine` private readonly field |
| 22 | Declare `ITextFormatting` constructor parameter and `_textFormatting` private readonly field |
| 23 | Declare `IConsoleOutput` constructor parameter and `_console` private readonly field |
| 24 | Declare `IRandomProvider` constructor parameter and `_randomProvider` private readonly field (for PRINTDATA in MESSAGE21) |
| 25 | In handler bodies, call `_ntrRevelationHandler?.Execute()` for NTR revelation path in MESSAGE21, MESSAGE25, MESSAGE26; `Grep "_ntrRevelationHandler\\?"` must yield exactly 3 matches |
| 26 | In HandleMessage22 AND HandleMessage23, call `_tEquipVariables.SetEquip()` or `SetTEquip()`; `Grep "Set(T)?Equip"` must yield at least 2 matches |
| 27 | In MESSAGE24, call `BitfieldUtility.SetBit()` for SETBIT operations; `Grep "BitfieldUtility\\.SetBit"` must find at least one match |
| 28 | In HandleMessage25 and HandleMessage26, call `_engine.GetResult()` directly to read the integer input result after RequestNumericInput; `Grep "_engine\\.GetResult"` must yield exactly 2 matches |
| 29 | In handler bodies, call `_kojoMessage.KojoMessageWcCounter()` for KOJO dispatch; `Grep "_kojoMessage\\."` must yield at least 10 matches (1 per handler) |
| 30 | In WcCounterMessage.cs, add `WcCounterMessageTease tease` as constructor parameter; `Grep "WcCounterMessageTease tease"` in WcCounterMessage.cs must find at least one match |
| 31 | In MESSAGE24, call `BitfieldUtility.GetBit()` for GETBIT bitfield reads; `Grep "BitfieldUtility\\.GetBit"` must find at least one match |
| 32 | In handler bodies, call `_variables.SetCharacterFlag()` for CFLAG state mutations; `Grep "SetCharacterFlag"` must find at least one match |
| 33 | In HandleMessage25 and HandleMessage26, implement `while (true)` INPUT retry loop; `Grep "while \\(true\\)"` must yield exactly 2 matches |
| 34 | All private helper methods in WcCounterMessageTease return void; `Grep "private (static )?int \\w+\\("` must yield 0 matches (AC#9 invariant) |
| 35 | Excluded IWcCounterMessageItem methods (BottomClothOut, BottomClothOff) not called; `Grep "_item\\.BottomCloth"` must yield 0 matches (C2 exclusion invariant) |
| 36 | In handler bodies, call `_tEquipVariables.GetEquip()` or `GetTEquip()` for EQUIP read operations; `Grep "Get(T)?Equip"` must yield at least 1 match |
| 37 | In HandleMessage25, call `_item.ReportSetItems()` exactly once; `Grep "_item\\.ReportSetItems"` must yield exactly 1 match (C2 spot-check) |
| 38 | In HandleMessage24, call `_item.ClothOut()` exactly twice; `Grep "_item\\.ClothOut"` must yield exactly 2 matches (C2 per-handler anchor) |
| 39 | No `return 0;` in WcCounterMessageTease; `Grep "return 0;"` must yield 0 matches (single-exit enforcement) |
| 40 | In HandleMessage21, call `_randomProvider.Next()` for PRINTDATA random text; `Grep "_randomProvider\\."` must find at least one match |
| 41 | In HandleMessage24, call `_item.RotorIn()` exactly twice (with different parameters); `Grep "_item\\.RotorIn"` must yield exactly 2 matches (C2 per-handler anchor) |
| 42 | In HandleMessage29_1, reference `KojoActionMessage29_1` constant for KOJO dispatch; `Grep "KojoActionMessage29_1"` must find at least one match (AC#29 branch-action spot-check) |
| 43 | Total `_ntr.` call sites bounded to 13 (OshikkoLooking only); `Grep "_ntr\\."` must yield exactly 13 matches (no F806-scoped NTR methods called from TEASE) |
| 44 | In HandleMessage29_2, reference `KojoActionMessage29_2` constant for KOJO dispatch; `Grep "KojoActionMessage29_2"` must find at least one match |
| 45 | In HandleMessage29_3, reference `KojoActionMessage29_3` constant for KOJO dispatch; `Grep "KojoActionMessage29_3"` must find at least one match |
| 46 | In handler bodies, call `_console.PrintLine()` or equivalent for console output; `Grep "_console\\."` must yield at least 10 matches (1 per handler) |
| 47 | In handler bodies, call `_textFormatting.SetColor()` or equivalent for text formatting; `Grep "_textFormatting\\."` must find at least one match |
| 48 | In HandleMessage21, reference `KojoActionMessage21` constant for KOJO dispatch; `Grep "KojoActionMessage21"` must find at least one match |
| 49 | In HandleMessage27, reference `KojoActionMessage27` constant for KOJO dispatch; `Grep "KojoActionMessage27"` must find at least one match |
| 50 | In HandleMessage24 (TEASE.ERB:1466) and HandleMessage25 (TEASE.ERB:2048), call `_item.InsertItemIn()`; `Grep "_item\\.InsertItemIn"` must yield exactly 2 matches (C2 InsertItemIn shared anchor) |
| 51 | In HandleMessage22, reference `KojoActionMessage22` constant for KOJO dispatch; `Grep "KojoActionMessage22"` must find at least one match |
| 52 | In HandleMessage23, reference `KojoActionMessage23` constant for KOJO dispatch; `Grep "KojoActionMessage23"` must find at least one match |
| 53 | In HandleMessage24, reference `KojoActionMessage24` constant for KOJO dispatch; `Grep "KojoActionMessage24"` must find at least one match |
| 54 | In HandleMessage25, reference `KojoActionMessage25` constant for KOJO dispatch; `Grep "KojoActionMessage25"` must find at least one match |
| 55 | In HandleMessage26, reference `KojoActionMessage26` constant for KOJO dispatch; `Grep "KojoActionMessage26"` must find at least one match |
| 56 | All 10 public handler methods use block-body (not expression-body `=> 1`); `Grep "public int HandleMessage.*=>"` must yield 0 matches |
| 57 | After Task 6 execution, F813 contains dedicated "F807 Mandatory Handoffs" section; `Grep "F807 Mandatory Handoffs" pm/features/feature-813.md` must find at least one match |
| 58 | In HandleMessage25 and HandleMessage26 while-loop bodies, use `break;` for valid-input exit (C16); `Grep "break;"` must yield at least 2 matches (additional break; from switch/case allowed) |
| 59 | In HandleMessage24, call `_item.CliCapOut()` once, and in HandleMessage25 call `_item.CliCapOut()` twice; `Grep "_item\\.CliCapOut"` must yield exactly 3 matches (C2 CliCapOut cross-handler anchor) |
| 60 | After Task 6 execution, F813 "F807 Mandatory Handoffs" section contains "コンストラクタ肥大化" text (AC#57 content anchor); `Grep "コンストラクタ肥大化" pm/features/feature-813.md` must find at least one match |
| 61 | After Task 6 execution, F813 "F807 Mandatory Handoffs" section contains "ローカル関数" text (Task 6 item (f) anchor); `Grep "ローカル関数" pm/features/feature-813.md` must find at least one match |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Interface for WcCounterMessageTease? | A: Create IWcCounterMessageTease interface. B: Use concrete type directly (like WcCounterReaction/WcCounterPunishment) | B: Concrete type | WcCounterMessageTease is called exclusively from WcCounterMessage.Dispatch(). No other class injects it. Creating an interface introduces unnecessary indirection with no testability or composability benefit at this scope. Pattern precedent: WcCounterReaction and WcCounterPunishment are held as concrete types in WcCounterMessage. |
| WcCounterMessage constructor injection of WcCounterMessageTease | A: Add as constructor parameter (DI-resolved). B: Instantiate directly in constructor | A: Constructor parameter | Consistent with WcCounterReaction and WcCounterPunishment pattern. DI resolves all transitive dependencies of WcCounterMessageTease automatically. |
| DI lifetime for WcCounterMessageTease | A: Singleton. B: Transient | A: AddSingleton | WcCounterMessageTease is stateless (no mutable fields). Consistent with WcCounterMessageItem, WcCounterMessageNtr, WcCounterMessage registrations. |
| INPUT loop translation (MESSAGE25/MESSAGE26) | A: `while (true)` with RequestNumericInput + ProvideInput. B: Recursive helper method | A: while loop | Direct translation of ERB GOTO retry pattern. Mirrors ShopSystem.cs RequestNumericInput usage (lines 188, 206, 246, 304). Avoids stack depth risk of recursion for long input sessions. |
| NtrRevelationHandler nullable injection in WcCounterMessageTease | A: `INtrRevelationHandler?` (nullable, optional). B: Required non-null | A: Nullable optional (default = null) | Matches TRYCALL no-op semantics from TEASE.ERB:15,1860,2542. Consistent with WcCounterMessage.HandleMessage13() pattern (`_ntrRevelationHandler?.Execute()`). |
| CHARACTER_ID constants for MESSAGE27 branching | A: Define as private constants in WcCounterMessageTease. B: Reference a shared CharacterConstants class | A: Private constants | MESSAGE27 character references are TEASE-specific domain knowledge. No shared constants class exists in Era.Core yet. Following WcCounterMessage.cs pattern (CFLAG indices defined as private const int). |
| MESSAGE24 sub-method decomposition | A: Single HandleMessage24() body with all logic inline. B: Private void helper methods per item category called from HandleMessage24() | B: Private helpers | MESSAGE24 is ~1,068 ERB lines. Risk mitigation (line 120) recommends "Decompose into sub-methods per item category." Private helpers reduce nesting depth while keeping AC#9 count_equals 10 valid (only public handlers counted). |

### Interfaces / Data Structures

**WcCounterMessageTease constructor** (new, `Era.Core/Counter/WcCounterMessageTease.cs`):
```csharp
public sealed class WcCounterMessageTease
{
    private readonly IVariableStore _variables;
    private readonly IEngineVariables _engine;
    private readonly ITEquipVariables _tEquipVariables;
    private readonly ITextFormatting _textFormatting;
    private readonly IConsoleOutput _console;
    private readonly IKojoMessageService _kojoMessage;
    private readonly IRandomProvider _randomProvider;
    private readonly IInputHandler _inputHandler;
    private readonly IWcCounterMessageItem _item;
    private readonly IWcCounterMessageNtr _ntr;
    private readonly INtrRevelationHandler? _ntrRevelationHandler;

    public WcCounterMessageTease(
        IVariableStore variables,
        IEngineVariables engine,
        ITEquipVariables tEquipVariables,
        ITextFormatting textFormatting,
        IConsoleOutput console,
        IKojoMessageService kojoMessage,
        IRandomProvider randomProvider,
        IInputHandler inputHandler,
        IWcCounterMessageItem item,
        IWcCounterMessageNtr ntr,
        INtrRevelationHandler? ntrRevelationHandler = null) { ... }

    // 10 public handler methods: HandleMessage21-27, HandleMessage29_1/2/3
    // All return int (1 on success)
}
```

**WcCounterMessage modifications** (`Era.Core/Counter/WcCounterMessage.cs`):
- Add constructor parameter: `WcCounterMessageTease tease`
- Add field: `private readonly WcCounterMessageTease _tease;`
- Update `Dispatch()` switch: `21 => _tease.HandleMessage21()`, ..., `27 => _tease.HandleMessage27()`
- Update `DispatchWithBranch()` switch: `(29, 1) => _tease.HandleMessage29_1()`, `(29, 2) => _tease.HandleMessage29_2()`, `(29, 3) => _tease.HandleMessage29_3()`

**CFLAG constants** (in WcCounterMessageTease):
| Constant Name | ERB Name | Index |
|--------------|----------|-------|
| `CflagWcResponseCategory` | WC_応答分類 | 714 |
| (EQUIP, TALENT, TCVAR constants derived from TEASE.ERB source) | | |

**CFLAG naming convention note:** WcCounterMessageTease uses `Cflag` (lowercase f) following the F808 sibling class convention (WcCounterMessageItem.cs, WcCounterMessageNtr.cs). The parent class WcCounterMessage.cs uses `CFlag` (capital F) for the same index 714 (F805 convention). This intra-chain inconsistency is a known convention split. Resolution: adopt `Cflag` (lowercase f) as the go-forward convention; rename in WcCounterMessage.cs is tracked for F813 review scope.

**CFLAG constant duplication note:** CFLAG index constants (e.g., CflagWcResponseCategory=714, CflagFavor=2) are duplicated as private const in each Counter/ class that uses them (WcCounterMessage, WcCounterSourceHandler, WcCounterMessageItem, WcCounterMessageNtr, and now WcCounterMessageTease). This is an existing debt pattern inherited from F805/F808, not introduced by F807. F807 follows the established pattern for consistency. Consolidation into a shared CounterCflagConstants class or typed CFlagIndex struct is tracked for F813 review scope.

**TALENT constants** (in WcCounterMessageTease):
| Constant Name | ERB Name | Index |
|--------------|----------|-------|
| `TalentNtr` | NTR talent index | (from WcCounterMessageNtr.cs: `TalentNtr = 6`) |

**Kojo action type constants** (in WcCounterMessageTease):
| Constant | ERB action ID | Handlers |
|----------|--------------|---------|
| `KojoActionMessage21` | 21 | HandleMessage21 |
| `KojoActionMessage22` | 22 | HandleMessage22 |
| `KojoActionMessage23` | 23 | HandleMessage23 |
| `KojoActionMessage24` | 24 | HandleMessage24 |
| `KojoActionMessage25` | 25 | HandleMessage25 |
| `KojoActionMessage26` | 26 | HandleMessage26 |
| `KojoActionMessage27` | 27 | HandleMessage27 |
| `KojoActionMessage29_1` | 29 (branch 1) | HandleMessage29_1 |
| `KojoActionMessage29_2` | 29 (branch 2) | HandleMessage29_2 |
| `KojoActionMessage29_3` | 29 (branch 3) | HandleMessage29_3 |

**ServiceCollectionExtensions.cs addition:**
```csharp
// Counter System (Phase 21) - Feature 807
services.AddSingleton<WcCounterMessageTease>();
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| WcCounterMessage constructor will gain a new `WcCounterMessageTease` parameter — the DI registration for `IWcCounterOutputHandler, WcCounterMessage` (ServiceCollectionExtensions.cs:191) automatically resolves `WcCounterMessageTease` if it is registered as AddSingleton before the WcCounterMessage line | Dependencies section | No fix needed; but note in Tasks: register WcCounterMessageTease before WcCounterMessage in the DI call order |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 5, 6, 7, 8, 9, 11, 12, 13, 20, 21, 22, 23, 24, 34, 56 | Create `Era.Core/Counter/WcCounterMessageTease.cs`: `public sealed class WcCounterMessageTease` with all 11 constructor-injected fields (IVariableStore, IEngineVariables, ITEquipVariables, ITextFormatting, IConsoleOutput, IKojoMessageService, IRandomProvider, IInputHandler, IWcCounterMessageItem, IWcCounterMessageNtr, INtrRevelationHandler?) and exactly 10 stub handler method signatures (public int HandleMessage21-27, HandleMessage29_1/2/3, each returning 1); Verification order: AC#34 and AC#56 must pass before AC#9 (per AC#9 Verification Prerequisites) | | [x] |
| 2 | 10, 14, 16, 17, 18, 19, 25, 26, 27, 28, 29, 31, 32, 33, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 58, 59 | Implement all 10 handler method bodies in WcCounterMessageTease: MESSAGE21-27 and MESSAGE29_1-3 migrated from TOILET_COUNTER_MESSAGE_TEASE.ERB; MESSAGE25 and MESSAGE26 must use `while (true)` with `_inputHandler.RequestNumericInput()` for INPUT retry loops; all handlers have single exit point with return 1; MESSAGE24 delegates to `_item.*` methods; MESSAGE26 calls SetTalent (C7); MESSAGE27 calls SetTCVar (C8); `_ntr.OshikkoLooking()` called 13 times per character in MESSAGE27 (C14); `_ntrRevelationHandler?.Execute()` called for NTR revelation in MESSAGE21/25/26 (C5); define and reference CflagWcResponseCategory constant for response category branching | | [x] |
| 3 | 3, 4, 30 | Update `Era.Core/Counter/WcCounterMessage.cs`: add `private readonly WcCounterMessageTease _tease` field and constructor parameter; replace stub `=> 0` with `=> _tease.HandleMessage2N()` for actions 21-27 in Dispatch(); replace stub `(29, N) => 0` with `=> _tease.HandleMessage29_N()` for (29,1-3) in DispatchWithBranch() | | [x] |
| 4 | 2 | Update `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`: add `services.AddSingleton<WcCounterMessageTease>()` registration before the WcCounterMessage registration line (Phase 21 - Feature 807 comment block) | | [x] |
| 5 | 15 | Run `dotnet build src/Era.Core/` via WSL and confirm zero errors and zero warnings (TreatWarningsAsErrors=true) | | [x] |
| 6 | 57, 60, 61 | Transfer deferred obligations to F813: (a) behavioral test coverage for WcCounterMessageTease handlers, (b) IWcCounterMessageTease interface extraction decision, (c) character ID constant consolidation, (d) WcCounterMessage constructor growth mitigation evaluation, (e) CFlag/Cflag naming convention normalization, (f) AC#34 local function enforcement gap tracking. Update F813 with concrete tracking items for each handoff. | | [x] |

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
| 1 | implementer | sonnet | feature-807.md Task 1, WcCounterMessageTease constructor spec from Technical Design (Interfaces/Data Structures) | `Era.Core/Counter/WcCounterMessageTease.cs` (sealed class, 11 injected fields, 10 stub handler methods returning 1) |
| 2 | implementer | sonnet | feature-807.md Task 2, TOILET_COUNTER_MESSAGE_TEASE.ERB (all 10 functions), IWcCounterMessageItem/IWcCounterMessageNtr/IInputHandler APIs | `Era.Core/Counter/WcCounterMessageTease.cs` (all 10 handler bodies implemented; MESSAGE25/26 with while-loop input; no return 0) |
| 3 | implementer | sonnet | feature-807.md Task 3, `Era.Core/Counter/WcCounterMessage.cs` current content | `Era.Core/Counter/WcCounterMessage.cs` (_tease field added; Dispatch and DispatchWithBranch updated with delegation calls) |
| 4 | implementer | sonnet | feature-807.md Task 4, `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` current content | `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` (AddSingleton<WcCounterMessageTease> inserted before WcCounterMessage registration) |
| 5 | implementer | sonnet | feature-807.md Task 5 (AC#15) | Build output confirming zero errors and zero warnings |
| 6 | implementer | sonnet | feature-807.md Task 6, Mandatory Handoffs table, F813 feature file | F813 updated with concrete tracking items for each deferred obligation |

> **Pre-conditions**: F805 [DONE] (dispatch stubs at WcCounterMessage.cs:241-248, 306-309; all interfaces exist). F808 [DONE] (IWcCounterMessageItem 17 methods, IWcCounterMessageNtr 6 methods). TreatWarningsAsErrors=true.
>
> **Error Handling**: IInputHandler signature → check ShopSystem.cs:188,206,246,304. Constructor ambiguity → append WcCounterMessageTease as last parameter. INtrRevelationHandler? DI → already registered by F808 T6.

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| No unit test ACs defined for WcCounterMessageTease | F807 ACs are all code/build static checks; behavioral test coverage (handler branching, INPUT loop paths, NTR revelation, EQUIP mutations) not verified by current ACs | Feature | F813 | T6 | [x] | 確認済み |
| WcCounterMessage dispatch routing untestable without IWcCounterMessageTease interface | WcCounterMessageTease injected as concrete type (Key Decision Row 1); WcCounterMessage.Dispatch() cannot be unit-tested in isolation without constructing real WcCounterMessageTease with all 11 deps. Options: (A) Create IWcCounterMessageTease in F813, (B) Accept as permanent debt | Feature | F813 | T6 | [x] | 確認済み |
| Character ID constants (13 private const in MESSAGE27) not tracked for consolidation | F806 SEX may define overlapping character IDs for per-character branching. No shared CharacterConstants class exists. Key Decision Row 6 defers to "when shared class exists" but no Feature ID tracks creation | Feature | F813 | T6 | [x] | 確認済み |
| WcCounterMessage constructor growth mitigation (11 params post-F807, 12 post-F806) | Must evaluate parameter object or dispatching service pattern before further Counter subsystem expansion | Feature | F813 | T6 | [x] | 確認済み |
| CFlag/Cflag naming convention normalization across Counter/ classes | WcCounterMessage.cs uses CFlag prefix (F805), F808 siblings and F807 use Cflag prefix; intra-chain inconsistency | Feature | F813 | T6 | [x] | 確認済み |
| AC#34 local function gap: grep pattern cannot detect C# local functions returning int | Local functions lack `private` keyword, evading AC#34's `private (static )?int \w+\(` pattern; Technical Design prohibits local functions but no mechanical AC enforcement exists | Feature | F813 | T6 | [x] | 確認済み |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-02 | fc | consensus-synthesizer, ac-designer, tech-designer | Completed /fc Phases 1-6 | [PROPOSED] |
| 2026-03-02 | /run P1 | initializer | Set [WIP], verified predecessors [DONE] | READY |
| 2026-03-02 | /run P4-T1 | implementer | Created WcCounterMessageTease.cs skeleton (11 fields, 10 stub methods) | SUCCESS |
| 2026-03-02 | DEVIATION | smart-implementer | Task 2 handler body implementation | Prompt too long - agent exceeded context limits after 72 tool uses; escalated to debugger for AC count fixes |
| 2026-03-02 | /run P4-T2 | smart-implementer + debugger | Implemented all 10 handler bodies from ERB; fixed AC#34/9/14/37/59 count mismatches | SUCCESS |
| 2026-03-02 12:31 | START | implementer | Task 3 | - |
| 2026-03-02 12:31 | END | implementer | Task 3 | SUCCESS |
| 2026-03-02 12:33 | START | implementer | Task 4 | - |
| 2026-03-02 12:33 | END | implementer | Task 4 | SUCCESS |
| 2026-03-02 21:35 | START | implementer | Task 6 | - |
| 2026-03-02 21:35 | END | implementer | Task 6 | SUCCESS |
| 2026-03-02 | /run P7 | ac-tester | All 61 ACs PASS (OK:61/61) | SUCCESS |
| 2026-03-02 | /run P8 | feature-reviewer | Quality review READY, 8.2 skip (no extensibility), 8.3 N/A | SUCCESS |
| 2026-03-02 | DEVIATION | verify-logs.py | --scope feature:807 | exit 1: _out/logs/prod not found (cross-repo: ac-static-verifier incompatible with feature-in-devkit+code-in-core; AC verification done by ac-tester) |
| 2026-03-02 | DEVIATION | pre-commit | core build | exit 1: WcCounterMessageTests.cs missing WcCounterMessageTease constructor param (PRE-EXISTING test not updated for new dependency); fixed by debugger |

---

<!-- fc-phase-6-completed -->
## Review Notes
- [fix] Phase1-RefCheck iter1: Links section | F814 missing from Links section (added [Related: F814])
- [resolved-applied] Phase2-Pending iter1: Philosophy 'full equivalence' claim has zero behavioral verification in F807 ACs. All 15 ACs are static code/build checks. Mandatory Handoffs defers to F813 [DRAFT] with no binding scope. Decision needed: (A) Add behavioral unit test ACs to F807, (B) Bind F813 as Predecessor, or (C) Accept current deferral design. → Resolution: Option A selected — Philosophy narrowed to 'structural migration equivalence'; behavioral equivalence deferred to F813.
- [fix] Phase2-Review iter1: AC#14 | AC#14 weakly verified C2 (only _item. presence). Strengthened to count_equals 14 matching IWcCounterMessageItem method calls.
- [fix] Phase2-Review iter1: AC Definition Table | C10 (response category) had zero AC coverage. Added AC#16 for CflagWcResponseCategory presence.
- [fix] Phase2-Review iter1: Deferred Obligations section | Non-template section removed. Content was redundant with Technical Constraints and Tasks.
- [fix] Phase2-Review iter1: Implementation Contract | Removed 5 non-template subsections (Pre-conditions, Execution Order, Build Verification Steps, Success Criteria, Error Handling). Key info consolidated into blockquote notes.
- [fix] Phase2-Review iter2: AC#9 | Changed from not_contains 'return 0;' to count_equals 10 for 'return 1;' (positive assertion). Fixed AC Detail/Table matcher inconsistency.
- [fix] Phase2-Review iter3: AC Definition Table | C7 (SetTalent) had zero AC coverage. Added AC#17 for SetTalent presence in MESSAGE26.
- [fix] Phase2-Review iter3: AC Definition Table | C8 (SetTCVar) had zero AC coverage. Added AC#18 for SetTCVar presence in MESSAGE27.
- [fix] Phase2-Review iter3: Goal Coverage row 3 | Added AC#17, AC#18 to 'equivalent output and state mutations' coverage.
- [fix] Phase2-Review iter4: AC Definition Table | C13 interface injection coverage gap. Added AC#20-24 for IVariableStore, IEngineVariables, ITextFormatting, IConsoleOutput, IRandomProvider.
- [fix] Phase2-Review iter4: AC Definition Table | No AC verified _ntr. call sites. Added AC#19 for NTR delegation usage.
- [fix] Phase2-Review iter5: AC#3, AC#4 | Changed from matches to count_equals 7/3. Ensures all 10 stubs are replaced, not just 1.
- [fix] Phase2-Review iter5: AC Definition Table | C5 _ntrRevelationHandler call presence gap. Added AC#25 (mirrors AC#19 pattern).
- [fix] Phase2-Review iter6: AC#9 | Added single-exit-point mandate to prevent count_equals 10 false negatives from multiple return paths.
- [fix] Phase2-Review iter6: Tasks | Moved AC#13 from Task 2 to Task 1 (stub creation aligns with method count verification).
- [fix] Phase2-Review iter6: AC#25 | Changed matcher from _ntrRevelationHandler to _ntrRevelationHandler\\? (dot-anchored to match call sites, not field declaration).
- [resolved-applied] Phase2-Loop iter7: AC#14 count_equals 14 vs C2 "14+" semantics. Resolved: (A) enumerated exact count from ERB source — 14 distinct methods in MESSAGE24 (16 call sites) + 6 in MESSAGE25 = 22 total. C2 updated to "14 distinct", AC#14 updated to count_equals 22.
- [resolved-applied] Phase2-Loop iter7: AC#9 single-exit-point mandate. Resolved: (A) kept mandate — ERB source confirms exactly one RETURN 1 per function (verified at @EVENT_WC_COUNTER_MESSAGE21-27, 29_1-3 function boundaries). C# should faithfully mirror this pattern. count_gte is not a valid matcher.
- [fix] Phase2-Review iter1: --- separator | Missing --- horizontal rule before ## Technical Design (template compliance)
- [fix] Phase2-Review iter1: C2/AC#14 | C2 updated from "14+" to "14 distinct" methods (verified from ERB source); AC#14 count_equals updated from 14 to 22 (total _item. call sites: MESSAGE24 16 + MESSAGE25 6); AC Detail and AC Coverage updated
- [fix] Phase2-Review iter2: AC#9 + Technical Design | Resolved contradiction between Risks mitigation (decompose MESSAGE24) and Technical Design (no sub-methods). Now permits private void helpers. AC#9 detail clarified: count_equals 10 applies to public handler methods only. Added Key Decision for MESSAGE24 sub-method decomposition.
- [fix] Phase2-Review iter3: Goal Coverage | Moved AC#20-24 from Goal Item 1 (class creation) to Goal Item 3 (ERB equivalence). AC#20-24 verify interface injection for state access, not handler implementation.
- [fix] Phase2-Review iter4: Key Decisions | Removed obsolete Row 5 ("MESSAGE24 sub-method decomposition: B: Inline delegations") which contradicted Row 8 ("B: Private helpers") added in iter2.
- [fix] Phase2-Review iter5: Technical Design | Fixed CFLAG naming convention from CFlagWcResponseCategory (capital F, WcCounterMessage.cs pattern) to CflagWcResponseCategory (lowercase f, F808 sibling pattern). Verified against WcCounterMessageItem.cs and WcCounterMessageNtr.cs.
- [fix] Phase2-Review iter6: AC#26 | Added AC#26 for SetEquip/SetTEquip call-site verification (C6). Follows AC#17/AC#18 precedent for mutation call-site vs injection-only.
- [fix] Phase2-Review iter6: AC#19 | Upgraded from matches (presence-only) to count_equals 13. MESSAGE27 calls _ntr.OshikkoLooking() 13 times (one per character at TEASE.ERB:3284-3308). Consistent with AC#14 count-based verification.
- [fix] Phase2-Review iter7: C14 + Task 2 | Added C14 constraint for IWcCounterMessageNtr delegation (distinct from C5 INtrRevelationHandler). Updated AC#7, AC#19 to cite C14. Clarified Task 2 tag: C14 for _ntr delegation, C5 for _ntrRevelationHandler.
- [fix] Phase2-Review iter7: AC#14 Detail | Added per-line ERB citations for MESSAGE25's 6 _item call sites (lines 1999, 2006, 2008, 2027, 2048, 2225).
- [fix] Phase2-Review iter8: AC#10 | Upgraded from matches (presence-only) to count_equals 2. Ensures BOTH MESSAGE25 and MESSAGE26 implement INPUT loops. Follows AC#14/AC#19 count-based precedent.
- [fix] Phase2-Review iter9: AC#25 Detail/Coverage | Fixed inconsistency: Detail and Coverage used plain "_ntrRevelationHandler" while Definition Table used "_ntrRevelationHandler\\?". Aligned all to use "_ntrRevelationHandler\\?" (null-conditional pattern).
- [fix] Phase2-Review iter1: Upstream Issues | Removed 2 stale entries (AC#13 gte→count_equals, AC#9 not_matches→count_equals) that were resolved in previous FL iterations but not cleared from the table.
- [fix] Phase2-Review iter2: AC#7 Rationale | Changed constraint citation from C5 to C14. Removed incorrect method list (WithPetting, RotorOut) — only OshikkoLooking is called from TEASE per C14/AC#19.
- [fix] Phase2-Review iter3: AC#25 | Upgraded from matches to count_equals 3. C5 names 3 handlers (MESSAGE21, 25, 26) requiring _ntrRevelationHandler delegation. Follows AC#10/AC#14/AC#19 count-based precedent.
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#27 (BitfieldUtility.SetBit for SETBIT ops in MESSAGE24). Follows AC#17/AC#18/AC#26 mutation call-site pattern.
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#28 (ProvideInput count_equals 2 for INPUT response-reading in MESSAGE25/26). Complements AC#10 (RequestNumericInput).
- [fix] Phase2-Review iter4: C2 Constraint Details | Added RotorIn×2 and ClothOut×2 enumeration to explain 14 distinct → 16 call sites.
- [fix] Phase2-Review iter5: AC Definition Table | Added AC#29 (_kojoMessage. matches for KOJO dispatch call-site verification). C4 requires dispatch verification, not just injection (AC#11).
- [fix] Phase2-Review iter5: AC#9 Detail | Added explicit void-return constraint for private helpers (from Technical Design Approach). Prevents count_equals 10 false positives from int-returning helpers.
- [fix] Phase2-Review iter6: AC Definition Table | Added AC#30 (WcCounterMessageTease constructor injection in WcCounterMessage.cs). Verifies DI injection pattern per Key Decision Row 2.
- [fix] Phase2-Review iter7: AC#16 | Upgraded from matches to gte 2. Prevents false-pass on constant declaration alone (matches only requires 1 match; gte 2 requires declaration + usage).
- [fix] Phase2-Review iter7: C13 | Changed source from "Interface Dependency Scan" (non-existent) to "Technical Design: Interfaces/Data Structures" with enumerated 11 parameters.
- [fix] Phase2-Review iter8: AC#16 AC Coverage | Updated "How to Satisfy" text to match gte 2 matcher (was "at least one match", now "at least 2 matches").
- [fix] Phase3-Maintainability iter9: Technical Design | Added CFLAG naming convention note documenting Cflag/CFlag split between F808 siblings and F805 parent. Resolution tracked for F813.
- [fix] Phase3-Maintainability iter9: Technical Design | Added WcCounterMessage constructor growth tracking paragraph. After F807 (11) + F806 (12), mitigation tracked for F813 review.
- [fix] Phase3-Maintainability iter9: Technical Design | Removed orphan Era.Core.Tests/Counter/ from file layout. Behavioral testing deferred to F813 per Mandatory Handoffs.
- [fix] Phase2-Uncertain iter1: AC#19 | Changed grep pattern from _ntr\. (file-wide) to _ntr\.OshikkoLooking (method-specific) with count_equals 13. Directly verifies C14's 13 OshikkoLooking calls without false-pass risk from other _ntr. methods.
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#31 (BitfieldUtility.GetBit for GETBIT bitfield reads in MESSAGE24). Completes read/write bitfield coverage alongside AC#27 (SetBit).
- [fix] Phase2-Review iter2: AC#28 + Technical Design | Corrected AC#28 from ProvideInput to GetResult. ProvideInput is engine-side injection; handlers use _engine.GetResult() directly per ShopSystem.cs pattern. Technical Design INPUT loop description also corrected.
- [fix] Phase2-Review iter3: AC#28 | Changed grep pattern from "GetResult" (false-fail: private helper decl + engine call = 3+ matches) to "_engine\.GetResult" count_equals 2 (direct inline calls). Removed private GetResult() helper from Technical Design. Aligned _engineVars → _engine per Interfaces spec SSOT.
- [fix] Phase2-Review iter3: C3 Constraint Detail | Removed stale ProvideInput from verification step. ProvideInput is engine-side; C3 now verifies only RequestNumericInput existence.
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#32 (SetCharacterFlag for CFLAG direct write call-site verification). Completes mutation coverage: EQUIP(AC#26), TCVAR(AC#18), TALENT(AC#17), SETBIT(AC#27), CFLAG(AC#32).
- [fix] Phase2-Review iter5: AC#26 | Changed grep pattern from SetEquip to Set(T)?Equip (regex alternation). SetTEquip does not contain SetEquip as substring; pattern now catches both ITEquipVariables methods.
- [fix] Phase2-Review iter6: C3 Constraint Detail | Added IEngineVariables.GetResult() SSOT citation (IEngineVariables.cs:14). AC#28 grep _engine\.GetResult now has verifiable source reference.
- [fix] Phase2-Review iter7: AC Definition Table | Added AC#33 (while (true) count_equals 2 for INPUT retry loop in MESSAGE25/26). C3 requires retry verification; AC#10/AC#28 only verify call-site counts, not loop structure.
- [resolved-applied] Phase2-Review iter1: Review Notes entries use FL SKILL.md persist_fix format (no category-code) but template comment (line 482) specifies [{category-code}] field. SSOT conflict resolved: Skills > templates per CLAUDE.md hierarchy (SSOT: Skills > CLAUDE.md > commands > agents). FL SKILL.md persist_fix/persist_pending format is authoritative. No change to entries needed.
- [fix] Phase2-Uncertain iter1: Technical Constraints | Added C15: _engine.GetResult() restricted to INPUT handlers (AC#28 count_equals 2 assumption documented as explicit constraint)
- [fix] Phase2-Review iter1: Execution Log | Added /fc workflow completion entry (was empty despite Phases 1-6 completed)
- [fix] Phase2-Review iter1: Technical Design Kojo constants | Replaced placeholder row with enumerated KojoActionMessage constants for all 10 handlers
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#34 (private int count_equals 0) enforcing void-return constraint for private helpers. AC#9 count_equals 10 invariant now mechanically verifiable.
- [fix] Phase2-Review iter3: C10 AC Impact | Downgraded from "verify each category produces distinct output path" to "verify constant defined and used as branching variable". Per-category behavioral distinctness deferred to F813 behavioral test scope.
- [fix] Phase2-Uncertain iter3: AC#34 | Narrowed grep pattern from "private int " to "private int [A-Z]". Targets method declarations only (C# methods uppercase). Avoids false-fail on private int fields or private const int constants.
- [fix] Phase2-Review iter3: AC#30 | Strengthened grep from "WcCounterMessageTease" to "WcCounterMessageTease tease". Distinguishes DI constructor parameter from direct new instantiation.
- [fix] Phase3-Maintainability iter4: Technical Design | Added CFLAG constant duplication note documenting existing debt pattern (not introduced by F807). Consolidation tracked for F813.
- [resolved-applied] Phase3-Maintainability iter4: F813 [DRAFT] lacks concrete tasks for F807 deferred obligations: (a) behavioral test coverage for WcCounterMessageTease handlers (Mandatory Handoffs row 1), (b) CFLAG constant consolidation across Counter/ classes, (c) WcCounterMessage constructor parameter consolidation, (d) Cflag/CFlag casing normalization. All 4 items are documented in F807 but F813 has no corresponding tracking. → Resolution: F807 Mandatory Handoffs section added to F813 with 3 tracking items (behavioral test coverage, IWcCounterMessageTease interface extraction, character ID constant consolidation).
- [fix] Phase1-RefCheck iter1: Baseline Measurement | Added "(generated at /run runtime)" note to baseline file reference (_out/tmp/ is gitignored)
- [fix] Phase2-Review iter1: Technical Design file layout | Fixed ServiceCollectionExtensions.cs listed under Era.Core/Counter/ instead of Era.Core/DependencyInjection/
- [fix] Phase2-Review iter1: AC#34 | Removed trailing HTML comment after AC Definition Table (moved to AC Details per template pattern)
- [fix] Phase2-Review iter1: AC#34 | Narrowed grep pattern from "private int [A-Z]" to "private int [A-Z]\\w*\\(" to target method declarations only (excludes non-const/non-readonly PascalCase fields)
- [fix] Phase3-Maintainability iter2: AC#29 | Upgraded from matches to gte 10. Technical Design defines 10 KojoAction constants (1 per handler); gte 10 ensures per-handler KOJO dispatch coverage.
- [fix] Phase3-Maintainability iter2: Mandatory Handoffs | Added row for WcCounterMessage dispatch routing testability gap (concrete type prevents isolation testing)
- [fix] Phase3-Maintainability iter2: Mandatory Handoffs | Added row for character ID constant consolidation tracking (13 private const in MESSAGE27, F806 overlap)
- [fix] Phase2-Review iter3: AC#34 | Expanded grep pattern from "private int" to "private (static )?int" to cover both instance and static private int methods (static escape hatch)
- [fix] Phase2-Review iter3: AC Definition Table | Added AC#35 (_item\.BottomCloth count_equals 0) enforcing C2 excluded-method invariant. Complements AC#14 count_equals 22 with method identity verification.
- [fix] Phase2-Review iter4: AC#9 Detail | Added expression-body prohibition. Block-body with explicit `return 1;` mandatory for all 10 handlers (prevents AC#9 false-negative from `=> 1` syntax).
- [fix] Phase2-Review iter4: AC#26 | Upgraded from matches to gte 2. C6 cites MESSAGE22 (478-480) AND MESSAGE23 (757-760); gte 2 enforces per-handler EQUIP mutation coverage.
- [fix] Phase2-Review iter5: AC Definition Table | Added AC#36 (Get(T)?Equip gte 1) for EQUIP read-path call-site verification. C9 documents 179 EQUIP reads; read path was unverified.
- [fix] Phase2-Uncertain iter5: AC Definition Table | Added AC#37 (_item\.ReportSetItems count_equals 1) as C2 method-identity spot-check. ReportSetItems is unique to MESSAGE25 (not shared with MESSAGE24).
- [fix] Phase2-Review iter6: AC Design Constraints | Moved C15 from Technical Constraints table to AC Design Constraints with proper C15 ID label. Technical Constraints uses (Constraint, Source, Impact) columns; AC Design Constraints uses (ID, Constraint, Source, AC Implication).
- [fix] Phase2-Review iter6: AC Definition Table | Added AC#38 (_item\.ClothOut count_equals 2) as C2 per-handler distribution anchor for MESSAGE24. Complements AC#37 (MESSAGE25 anchor).
- [fix] Phase2-Review iter7: AC Definition Table | Added AC#39 (return 0; count_equals 0) enforcing AC#9 single-exit mandate. Prevents early-exit return 0 in handlers.
- [fix] Phase2-Review iter8: AC Design Constraints | Added C16 (while-loop body must use break, not return 1, on valid input). Prevents AC#9 count inflation from return 1 inside MESSAGE25/26 while-loops.
- [fix] Phase2-Review iter8: AC Definition Table | Added AC#40 (_randomProvider\. matches) for IRandomProvider call-site verification in MESSAGE21. Completes injection→call-site verification pattern.
- [fix] Phase2-Review iter1: [pending] line 860 | Resolved SSOT conflict: SKILL.md persist_fix format > template category-code per CLAUDE.md hierarchy (Skills > templates). Tag changed to [resolved-applied].
- [fix] Phase2-Review iter1: Technical Design Approach | Extended void-return constraint from MESSAGE24-only to ALL private methods in WcCounterMessageTease, aligning with AC#34 enforcement scope.
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#41 (_item\.RotorIn count_equals 2) as second C2 per-handler distribution anchor for MESSAGE24. Complements AC#38 (ClothOut×2) and AC#37 (ReportSetItems×1).
- [fix] Phase2-Review iter2: AC#34 | Broadened grep pattern from `[A-Z]\w*` to `\w+` to catch private int methods regardless of naming convention (PascalCase/camelCase). Eliminates naming convention escape hatch.
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#42 (KojoActionMessage29_1 matches) as per-handler KOJO dispatch spot-check for branch actions. AC#29 gte 10 is file-wide; AC#42 anchors MESSAGE29_* dispatch participation.
- [fix] Phase4-ACValidation iter3: AC#33 | Escaped regex parentheses in grep pattern: `while (true)` → `while \(true\)`. Unescaped parens are regex group operators in ripgrep, matching `while true` instead of literal `while (true)`.
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#43 (_ntr\. count_equals 13) bounding total NTR call sites to OshikkoLooking-only. Mirrors AC#14+AC#35 item exclusion pattern for NTR: AC#19 verifies specific method, AC#43 bounds total count.
- [fix] Phase2-Review iter5: AC Definition Table | Added AC#44 (KojoActionMessage29_2 matches) and AC#45 (KojoActionMessage29_3 matches) completing branch-action KOJO dispatch coverage. All 3 MESSAGE29_* handlers now spot-checked (AC#42/44/45).
- [fix] Phase2-Review iter6: AC Definition Table | Added AC#46 (_console\. gte 10) and AC#47 (_textFormatting\. matches) closing injection→call-site gap for IConsoleOutput and ITextFormatting.
- [fix] Phase2-Review iter6: AC Definition Table | Added AC#48 (KojoActionMessage21) and AC#49 (KojoActionMessage27) as main-action KOJO constant spot-checks. 5 of 10 handlers now have individual KojoAction verification.
- [fix] Phase2-Review iter6: AC Definition Table | Added AC#50 (_item\.InsertItemIn count_equals 1) as second MESSAGE25 method-identity anchor alongside AC#37 (ReportSetItems).
- [fix] Phase2-Review iter7: AC Definition Table | Added AC#51-55 (KojoActionMessage22-26 matches) completing per-handler KOJO constant verification for all 10 handlers. AC#29 gte 10 + 10 individual constant spot-checks now fully enforce C4 KOJO dispatch.
- [fix] Phase2-Review iter8: C2/AC#14/AC#50 | Critical ERB enumeration fix: MESSAGE25 has 9 _item call sites (not 6), verified against TEASE.ERB. 3 missed: CliCapOut(2151), RotorPantsOut(2154), InsertItemOut(2180). AC#14 updated 22→25. AC#50 InsertItemIn updated 1→2 (shared with MESSAGE24 line 1466).
- [fix] Phase2-Review iter1: AC#43 Rationale | Stale count '22' updated to '25' to match AC#14 count_equals 25 (was stale from pre-iter8 value)
- [fix] Phase2-Review iter1: AC Details section | Reordered AC#41-45 to ascending numerical sequence (was #44, #45, #43, #42, #41)
- [fix] Phase2-Review iter1: AC#20 Description | Changed from 'Remaining 5 constructor interfaces' to 'IVariableStore injected (C13)' to match actual matcher scope (contains | IVariableStore)
- [fix] Phase2-Review iter2: Tasks + Mandatory Handoffs | Added Task 6 (handoff transfer to F813) and updated all 3 Mandatory Handoffs rows with Creation Task: T6. Ensures deferred obligations have concrete tracking mechanism per 'Track What You Skip' principle.
- [fix] Phase2-Review iter3: AC Definition Table | Added AC#56 (public int HandleMessage.*=> count_equals 0) enforcing expression-body prohibition. Makes AC#9 count_equals 10 invariant mechanically verifiable. Added to Task 1 AC list.
- [fix] Phase2-Review iter3: AC Definition Table | Added AC#57 (F813 contains WcCounterMessageTease) verifying Task 6 handoff transfer. Resolves orphan Task 6 (AC# was '-').
- [fix] Phase2-Uncertain iter4: AC#9 Detail | Added Verification Prerequisites note documenting AC#34/AC#56 dependency chain. AC#9 count_equals 10 requires AC#34 and AC#56 as preconditions.
- [fix] Phase2-Review iter5: Tasks | Moved AC#9 from Task 2 to Task 1. AC#9 count_equals 10 is satisfied by Task 1 stubs (each returning 1) and cannot gate Task 2.
- [fix] Phase2-Review iter5: AC Definition Table | Added AC#58 (break; count_equals 2) enforcing C16 mandate. Directly verifies break; presence in MESSAGE25/26 while-loop bodies. Assigned to Task 2.
- [fix] Phase4-ACValidation iter6: AC#58 | Changed matcher from count_equals 2 to gte 2. break; is common in switch/case across 10 handlers (MESSAGE24 item-type nesting); count_equals 2 would false-fail on valid implementations.
- [fix] Phase2-Review iter7: Tasks | Moved AC#34 from Task 2 to Task 1. AC#34 is a prerequisite for AC#9 (Task 1) per Verification Prerequisites; executing AC#34 in Task 2 (after Task 1) violated the documented dependency chain.
- [fix] Phase2-Review iter7: AC#57 | Strengthened from 'WcCounterMessageTease' to 'F807 Handoffs' pattern. Previous pattern had false-pass risk from pre-existing references in F813. New pattern verifies dedicated section header unique to Task 6 deliverable.
- [fix] Phase2-Review iter8: AC Definition Table | Added AC#59 (GetResult.*== gte 2) enforcing C3 INPUT retry guard condition. Prevents always-break implementation that passes AC#10/AC#28/AC#33/AC#58 without validating input.
- [fix] Phase2-Review iter9: AC#59 | Deleted AC#59 — GetResult.*== pattern has false-negative when result stored in local variable (common C# pattern). C3 retry enforcement covered by existing AC chain: AC#28 (GetResult count_equals 2) + AC#33 (while(true) count_equals 2) + AC#58 (break; gte 2) + AC#9 (return 1; count_equals 10). Conditional guard is behavioral concern deferred to F813.
- [fix] PostLoop-UserFix post-loop: Philosophy section | Narrowed 'full equivalence' → 'structural migration equivalence (static verification of class structure, interface injection, dispatch delegation, and call-site counts; behavioral equivalence verified in F813 Post-Phase Review)'. Philosophy Derivation table and AC#35 rationale also updated.
- [fix] PostLoop-UserFix post-loop: F813 feature file | Added 'F807 Mandatory Handoffs' section to F813 with 3 tracking items: (1) behavioral test coverage, (2) IWcCounterMessageTease interface extraction, (3) character ID constant consolidation. Follows F808/F805/F803 handoff pattern.
- [fix] Phase2-Review iter1: Goal section | Narrowed Goal from 'produce equivalent output and state mutations' to 'structurally migrated with correct interface injection, dispatch delegation, and call-site counts'. Goal Coverage row 3 also updated. Aligns with Philosophy narrowing.
- [fix] Phase2-Review iter1: F813 F807 Mandatory Handoffs | Added items 4 (WcCounterMessage constructor growth mitigation) and 5 (CFlag/Cflag naming normalization). Both documented in F807 Technical Design as F813-tracked.
- [fix] Phase2-Review iter1: Philosophy section | Qualified SSOT claim to 'canonical implementation' with interface extraction deferral note. Philosophy Derivation table updated to match.
- [fix] Phase2-Review iter2: Mandatory Handoffs table | Added rows 4 (constructor growth mitigation) and 5 (CFlag/Cflag naming normalization) to F807 Mandatory Handoffs table. F813 already tracked these items but F807 was not the SSOT for its own deferred obligations.
- [fix] Phase2-Review iter3: Task 6 + AC#57 Detail | Expanded Task 6 description from 3 to 5 items matching all 5 Mandatory Handoffs rows (added (d) constructor growth, (e) CFlag naming). AC#57 Detail rationale also updated to enumerate all 5.
- [fix] Phase2-Review iter4: Technical Design + AC#34 Detail | Added explicit local function prohibition. C# local functions lack `private` keyword and evade AC#34 pattern. All helper decomposition must use private void instance methods.
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#59 (_item\.CliCapOut count_equals 3) as MESSAGE25 per-handler distribution anchor. CliCapOut called ×1 in MESSAGE24 + ×2 in MESSAGE25 = 3 total. Follows AC#38/AC#41 pattern for MESSAGE24.
- [fix] Phase2-Review iter1: AC Details section | Added AC#59 Detail entry (was missing despite AC#59 in Definition Table and AC Coverage)
- [fix] Phase2-Review iter1: AC Coverage table | Added AC#59 row to Technical Design AC Coverage table (was missing, ended at row 58)
- [fix] Phase2-Review iter1: AC Definition Table + AC Details + Goal Coverage + Task 6 + AC Coverage | Added AC#60 (F813 contains "constructor growth" text) as content anchor for AC#57. AC#57 only verifies section header; AC#60 verifies actual obligation content was transferred.
- [fix] Phase2-Review iter1: Mandatory Handoffs table | Added AC#34 local function enforcement gap row. Local functions lack `private` keyword, evading AC#34 grep pattern. Formally tracked for F813 review.
- [fix] Phase2-Review iter2: Goal section | Added F813 obligation transfer sentence to Goal. Goal Coverage item 11 (AC#57, AC#60) was orphaned — no matching Goal sentence. Now explicitly states all 6 deferred obligations.
- [fix] Phase2-Review iter3: AC#57 Detail Rationale | Updated obligation count from 5 to 6, added (f) AC#34 local function enforcement gap. Aligns with Task 6 description and 6-row Mandatory Handoffs table.
- [fix] Phase2-Review iter4: AC#57 pattern | Changed from "F807 Handoffs" to "F807 Mandatory Handoffs". F813 actual section header is "F807 Mandatory Handoffs" (line 51); old pattern was not a substring match.
- [fix] Phase2-Review iter4: AC#60 pattern | Changed from "constructor growth" (English) to "コンストラクタ肥大化" (Japanese). F813 uses Japanese text for obligation (d); old English pattern had no match.
- [fix] Phase2-Review iter4: Goal section | Added block-body enforcement sentence. Goal Coverage item 10 (AC#56) was orphaned — no matching Goal sentence.
- [fix] Phase2-Review iter5: AC Definition Table + AC Details + Goal Coverage + Task 6 + AC Coverage | Added AC#61 (F813 contains "ローカル関数" text) as content anchor for obligation (f) AC#34 local function gap. Mirrors AC#60 pattern.
- [fix] Phase2-Review iter5: Task 1 Description | Added verification ordering note: AC#34 and AC#56 must pass before AC#9 (per AC#9 Detail Verification Prerequisites).

---

## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning
- [Predecessor: F805](feature-805.md) - WC Counter Source + Message Core
- [Predecessor: F808](feature-808.md) - WC Counter Message ITEM + NTR
- [Related: F806](feature-806.md) - WC Counter Message SEX
- [Successor: F813](feature-813.md) - Post-Phase Review Phase 21
- [Related: F814](feature-814.md) - Phase 22 Planning
