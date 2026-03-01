# Feature 808: WC Counter Message ITEM + NTR

## Status: [PROPOSED]

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

---

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

Phase 21 Counter System C# migration is the SSOT for all WC counter message logic. Each message-category feature (F805 Core, F806 SEX, F807 TEASE, F808 ITEM+NTR) migrates its ERB functions into injectable C# services behind interfaces, enabling TDD and eliminating runtime TRYCALL coupling. F808 scope covers TOILET_COUNTER_MESSAGE_ITEM.ERB (2,442 lines, 17 functions) and TOILET_COUNTER_MESSAGE_NTR.ERB (1,037 lines, 6 public functions + 2 private helpers), which together serve as shared utilities called by F806 and F807.

### Problem (Current Issue)

F808's 17 ITEM functions and 6 NTR public functions have no C# interfaces despite being hard dependencies of F806 (SEX) and F807 (TEASE) via TRYCALL. F806 makes 19+ TRYCALLs into F808 functions (bottomCLOTH_OFF at SEX.ERB:407,2578; bottomCLOTH_OUT at SEX.ERB:1540,1815,1903,2037; withFirstSex at SEX.ERB:1492-1516; withPETTING at SEX.ERB:2499). F807 makes 28+ TRYCALLs into F808 functions (NTRrevelation at TEASE.ERB:15,1860,2542; setITEMs at TEASE.ERB:823; bottomITEM_SHOW at TEASE.ERB:837; all device functions at TEASE.ERB:1372-2225). Only INtrRevelationHandler exists as a pre-defined interface (registered as null sentinel at ServiceCollectionExtensions.cs:194); all 16 remaining ITEM functions and 5 NTR observation functions lack corresponding C# interfaces. Because F808 is currently classified as merely "Related" to F806/F807, the hard TRYCALL coupling is not properly modeled, and neither F806 nor F807 can compile their C# migrations without F808's interfaces.

### Goal (What to Achieve)

Migrate all 23 ERB functions (17 ITEM + 6 NTR public) from TOILET_COUNTER_MESSAGE_ITEM.ERB and TOILET_COUNTER_MESSAGE_NTR.ERB into C# classes behind injectable interfaces, plus 2 NTR private helpers (NTRreversal_SOURCE, NTRagree_SOURCE). Specifically: (1) Create IWcCounterMessageItem interface exposing all 17 ITEM functions as methods, (2) Create IWcCounterMessageNtr interface exposing NTR observation functions, (3) Implement NtrRevelationHandler replacing the null sentinel in DI, (4) Register all new handlers in ServiceCollectionExtensions.cs, and (5) Reclassify F808 as Predecessor of F806 and F807 to correctly model the call chain dependency.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why can F806/F807 not implement their C# migrations? | Because they TRYCALL 23 F808 public functions that have no C# interfaces | SEX.ERB:407,1540,2578; TEASE.ERB:15,823,837,1372-2542 |
| 2 | Why do these TRYCALL targets have no C# interfaces? | Because F805 only defined INtrRevelationHandler for NTRrevelation; the other 23 functions were deferred to F808 | INtrRevelationHandler.cs:10-12; no IWcCounterMessageItem exists |
| 3 | Why were they deferred? | Because F805 scope covered only MESSAGE10-16 core dispatcher and the NTRrevelation forward reference | WcCounterMessage.cs:230-296 (dispatch stubs return 0) |
| 4 | Why does the dependency model not reflect this blocking relationship? | Because F808 is classified as "Related" to F806/F807 instead of Predecessor | feature-808.md Dependencies table |
| 5 | Why (Root)? | Because the original Phase 21 decomposition did not analyze cross-file TRYCALL chains to determine interface-level blocking dependencies between sibling features | F783 Phase 21 Planning created features by ERB file grouping without cross-call analysis |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | F808 is a skeletal [DRAFT] with 1 trivial AC and 2 generic tasks | 23 public ERB functions (17 ITEM + 6 NTR) lack C# interfaces needed by F806/F807 via TRYCALL |
| Where | feature-808.md minimal content | Era.Core/Counter/ directory missing IWcCounterMessageItem, IWcCounterMessageNtr, NtrRevelationHandler |
| Fix | Add more ACs to existing draft | Create injectable interfaces and implementations for all 24 functions; reclassify dependency as Predecessor |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| F805 | [DONE] | Defines WcCounterMessage.cs, INtrRevelationHandler, dispatch infrastructure |
| F806 | [DRAFT] | SEX calls bottomCLOTH_OFF, bottomCLOTH_OUT, withFirstSex, withPETTING (F808 is Predecessor) |
| F807 | [DRAFT] | TEASE calls NTRrevelation, setITEMs, bottomITEM_SHOW, CLOTH_OUT, all device functions (F808 is Predecessor) |
| F813 | [DRAFT] | Post-Phase Review Phase 21 |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Scope clarity | FEASIBLE | 2 source files clearly identified (ITEM: 2,442 lines, 17 functions; NTR: 1,037 lines, 6 public + 2 private functions) |
| Infrastructure readiness | FEASIBLE | IVariableStore, IEngineVariables, ITEquipVariables, ICommonFunctions, ITextFormatting, BitfieldUtility all exist in Era.Core |
| Interface contracts | FEASIBLE | INtrRevelationHandler defined with Execute(CharacterId, int); pattern established for new interfaces |
| TRYCALL semantics | FEASIBLE | Nullable interface injection pattern (?.) established in WcCounterMessage.cs:403 |
| External dependencies | FEASIBLE | NTR_RESET_VISITOR_ACTION can be stubbed via INtrUtilityService extension; DATETIME available via ICounterUtilities |
| Cross-feature coupling | FEASIBLE | F806/F807 extensively call F808 functions; manageable by defining interfaces before sibling implementation |
| State mutation complexity | FEASIBLE | NTRrevelation writes CFLAG/TALENT/FLAG; withPETTING has 22 SOURCE mutations, 11 SETBIT calls -- complex but testable via mock IVariableStore |
| Scope size | FEASIBLE | 3,479 lines total; structurally repetitive ITEM functions reduce per-function complexity |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| F806 (SEX) migration | HIGH | F806 cannot compile C# without F808's IWcCounterMessageItem interface (bottomCLOTH_OFF, bottomCLOTH_OUT, withFirstSex, withPETTING) |
| F807 (TEASE) migration | HIGH | F807 cannot compile C# without F808's IWcCounterMessageItem + IWcCounterMessageNtr interfaces (30+ TRYCALL sites) |
| DI composition root | MEDIUM | ServiceCollectionExtensions.cs must replace null sentinel and register 3+ new handlers |
| INtrUtilityService | MEDIUM | Must be extended with NtrResetVisitorAction method for NTRrevelation_ATTACK |
| WC_SexHara_MESSAGE (Phase 26) | LOW | External caller of NTRrevelation_ATTACK (WC_SexHara_MESSAGE.ERB:32); outside Phase 21 scope but must have callable interface |
| Test infrastructure | LOW | New unit test files for ITEM and NTR handlers; no changes to test framework |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| TRYCALL semantics: C# equivalent requires nullable interface injection (?.) | ERB TRYCALL = no-op if function missing | All ITEM/NTR interfaces must support nullable injection pattern |
| NTR_NAME(0) has no C# equivalent | TOILET_COUNTER_MESSAGE_NTR.ERB:850; ShopSystem.NtrName throws NotImplementedException | NTRrevelation text requires stub/facade via INtrUtilityService or equivalent |
| STAIN variable bitwise operations | TOILET_COUNTER_MESSAGE_NTR.ERB:641-784 | withPETTING uses STAIN bitwise OR; requires IVariableStore.GetStain/SetStain |
| DATETIME() built-in function | TOILET_COUNTER_MESSAGE_NTR.ERB:904,926 | NTRrevelation writes CFLAG = DATETIME(); needs ICounterUtilities.GetDateTime() |
| GETBIT/SETBIT on CFLAG | TOILET_COUNTER_MESSAGE_NTR.ERB:374-786 | BitfieldUtility.GetBit/SetBit available in Era.Core/Utilities/BitfieldUtility.cs |
| PRINTDATA random selection | TOILET_COUNTER_MESSAGE_NTR.ERB:803-828 | withPETTING uses PRINTDATA; needs IRandomProvider or IConsoleOutput.PrintData |
| TreatWarningsAsErrors=true | Directory.Build.props | All new code must compile warning-free |
| NTR_RESET_VISITOR_ACTION external dependency | TOILET_COUNTER_MESSAGE_NTR.ERB:943; NTR_UTIL.ERB:1110 | Must add NtrResetVisitorAction to INtrUtilityService (stub returns no-op until NTR_UTIL migration) |
| KOJO dispatch via TRYCALLFORM | TOILET_COUNTER_MESSAGE_NTR.ERB:898 | TRYCALLFORM KOJO_WC_COUNTER_NTRrevelation_1_K{NO:TARGET} must use IKojoMessageService |
| withPETTING SOURCE mutations | TOILET_COUNTER_MESSAGE_NTR.ERB:562-842 | 22 SOURCE:ARG:X += 100 writes require IVariableStore.SetSource with SourceIndex access |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| F806/F807 blocked on F808 interfaces | HIGH | HIGH | Define interfaces early; implement F808 before F806/F807 start; reclassify as Predecessor |
| Interface bloat from 18 ITEM methods | HIGH | MEDIUM | Design IWcCounterMessageItem with method-per-function; group logically (device insertion, device removal, cloth operations) |
| withPETTING body-part logic complexity (636 lines) | MEDIUM | MEDIUM | Thorough equivalence testing with mock IVariableStore; split into helper methods |
| NTR_NAME(0) stub creates silent regression | MEDIUM | MEDIUM | Document stub behavior clearly; add to INtrUtilityService with explicit NotImplementedException until migration |
| NTRrevelation_ATTACK external state mutations | MEDIUM | MEDIUM | Careful model for FLAG/CFLAG/TALENT mutations; delegate NTR_RESET_VISITOR_ACTION to INtrUtilityService stub |
| STAIN interface gap delays withPETTING | MEDIUM | LOW | Verify IVariableStore.GetStain/SetStain exists; if not, create stub |
| 24 functions exceed manageable PR size | MEDIUM | LOW | Split into 2-3 implementation tasks: ITEM functions, NTR functions, DI registration |
| ROTOR_OUT is dead code (never called) | LOW | LOW | Migrate for completeness but document as unreferenced (NTR.ERB:7-40) |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ITEM ERB functions | grep -c "^@" TOILET_COUNTER_MESSAGE_ITEM.ERB | 17 | Function entry points in ITEM file |
| NTR ERB functions | grep -c "^@" TOILET_COUNTER_MESSAGE_NTR.ERB | 6 | Function entry points in NTR file |
| Null sentinel registrations | grep -c "=> null!" ServiceCollectionExtensions.cs | 1 | INtrRevelationHandler null sentinel to be replaced |
| Existing C# ITEM interfaces | ls Era.Core/Counter/IWcCounterMessageItem.cs 2>/dev/null | 0 (not found) | Must be created by F808 |
| Existing C# NTR interfaces | ls Era.Core/Counter/IWcCounterMessageNtr.cs 2>/dev/null | 0 (not found) | Must be created by F808 |

**Baseline File**: `_out/tmp/baseline-808.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | INtrRevelationHandler.Execute must be implemented as a concrete class | INtrRevelationHandler.cs:12 | AC must verify concrete NtrRevelationHandler class exists and is DI-registered |
| C2 | Null sentinel in DI must be replaced with concrete registration | ServiceCollectionExtensions.cs:194 | AC must grep for AddSingleton.*INtrRevelationHandler.*NtrRevelationHandler (not => null!) |
| C3 | 17 ITEM functions must be callable by F806/F807 via injectable interface | Cross-call analysis: SEX.ERB, TEASE.ERB | AC must verify IWcCounterMessageItem interface defines all 17 methods |
| C4 | NTR observation functions must be callable by F806/F807 via injectable interface | Cross-call analysis: TEASE.ERB:15,3284-3308; NTR.ERB:7-40 | AC must verify IWcCounterMessageNtr interface defines RotorOut, OshikkoLooking, WithFirstSex(offender, painLevel), WithPetting, NtrRevelation, NtrRevelationAttack(attacker) (6 methods) |
| C5 | NTRrevelation state mutations must be preserved (CFLAG:WC_管理人は誰, WC_前任管理人, TALENT:管理人) | NTR.ERB:918-920 | ACs must test CFLAG/TALENT mutation outcomes |
| C6 | withPETTING SOURCE writes must be preserved (22 SOURCE:ARG:X += 100 writes) | NTR.ERB:562-842 | ACs must verify SOURCE variable increments per body-part |
| C7 | withPETTING SETBIT operations on WC_箇所埋まり must be bitfield operations | TOILET_COUNTER_MESSAGE_NTR.ERB:374-786 (11 SETBIT calls) | ACs must verify SETBIT/GETBIT behavior using BitfieldUtility |
| C8 | NTRrevelation_ATTACK must delegate to NTR_RESET_VISITOR_ACTION via INtrUtilityService | NTR.ERB:943 | AC must verify delegation call to INtrUtilityService.NtrResetVisitorAction |
| C9 | KOJO dispatch in NTRrevelation must use IKojoMessageService | NTR.ERB:898 | AC must verify IKojoMessageService is called for kojo dispatch |
| C10 | Internal self-call: ITEM_WITHotherITEM calls ReportSetItems | ITEM.ERB:266 | AC must verify internal delegation pattern |
| C11 | NTRrevelation_ATTACK must be exposed through callable interface for WC_SexHara_MESSAGE | WC_SexHara_MESSAGE.ERB:32 | F808 must expose NTRrevelation_ATTACK through IWcCounterMessageNtr |
| C12 | NTRreversal_SOURCE and NTRagree_SOURCE are internal helper functions | NTR.ERB:960-1034 | These must be private methods; ACs should cover favor/surrender calculation outcomes |
| C13 | Each ITEM function must check WC_応答分類 for response branching | Pattern across ITEM functions | ACs must verify branch coverage for response categories |
| C14 | ERB types accessed via interfaces | Interface Dependency Scan | Constructor injection ACs for IVariableStore, IEngineVariables, ITEquipVariables, ITextFormatting, IConsoleOutput, IKojoMessageService, IRandomProvider, ICounterUtilities, INtrUtilityService |
| C15 | NTR_NAME(0) must be callable via INtrUtilityService.GetNtrName | NTR.ERB:850; ShopSystem.NtrName throws NotImplementedException | AC must verify INtrUtilityService declares GetNtrName AND WcCounterMessageNtr calls it |

### Constraint Details

**C1: INtrRevelationHandler Implementation**
- **Source**: F805 deferred obligation; INtrRevelationHandler.cs defines Execute(CharacterId, int)
- **Verification**: grep for "class NtrRevelationHandler.*INtrRevelationHandler" in Era.Core/Counter/
- **AC Impact**: AC must verify both class existence and correct interface implementation

**C2: Null Sentinel Replacement**
- **Source**: ServiceCollectionExtensions.cs:194 registers `sp => null!` for INtrRevelationHandler
- **Verification**: grep ServiceCollectionExtensions.cs for "INtrRevelationHandler" -- must show concrete type, not null lambda
- **AC Impact**: AC must use not_matches for null sentinel AND matches for concrete registration

**C3: ITEM Interface Completeness**
- **Source**: Cross-call analysis showing F806 calls bottomCLOTH_OFF/OUT, F807 calls 15+ ITEM functions
- **Verification**: Count methods on IWcCounterMessageItem interface; must be >= 17
- **AC Impact**: Each major ITEM function group (device insert, device remove, cloth ops, display) should have representative AC

**C4: NTR Interface Completeness**
- **Source**: F807 calls NTRrevelation (TEASE.ERB:15), OSHIKKO_looking (TEASE.ERB:3284); F806 calls withFirstSex (SEX.ERB:1492), withPETTING (SEX.ERB:2499); ROTOR_OUT (NTR.ERB:7-40) migrated for completeness (dead code, see Risks); NTRrevelation_ATTACK exposed for WC_SexHara_MESSAGE (C11)
- **Verification**: Count methods on IWcCounterMessageNtr interface; must be >= 6
- **AC Impact**: Each NTR public function must be exposed through the interface

**C5: NTRrevelation State Mutations**
- **Source**: NTR.ERB:918-920 writes CFLAG:MASTER:WC_管理人は誰=0, CFLAG:MASTER:WC_前任管理人, TALENT:TARGET:管理人=0
- **Verification**: Unit test must verify SetCharacterFlag calls for these indices
- **AC Impact**: Behavioral test AC verifying mutation sequence

**C6: withPETTING SOURCE Writes**
- **Source**: TOILET_COUNTER_MESSAGE_NTR.ERB:562-842 contains 22 SOURCE:ARG:X += 100 writes across body-part targeting paths
- **Verification**: Unit tests must verify IVariableStore.SetSource calls for representative body-part paths
- **AC Impact**: AC#20 verifies IVariableStore injection; behavioral tests in AC#40 cover SOURCE mutation outcomes

**C7: withPETTING SETBIT Operations**
- **Source**: TOILET_COUNTER_MESSAGE_NTR.ERB:374-786 contains 11 SETBIT calls on CFLAG:MASTER:WC_箇所埋まり
- **Verification**: Implementation must use BitfieldUtility.SetBit/GetBit for bitfield read-modify-write
- **AC Impact**: AC#34 verifies BitfieldUtility usage in WcCounterMessageNtr.cs

**C8: NTR_RESET_VISITOR_ACTION Delegation**
- **Source**: NTR.ERB:943 calls NTR_RESET_VISITOR_ACTION (NTR_UTIL.ERB:1110); no C# equivalent
- **Verification**: INtrUtilityService must be extended with NtrResetVisitorAction method
- **AC Impact**: AC must verify both interface extension AND delegation call from NTRrevelation_ATTACK

**C9: KOJO Dispatch in NTRrevelation**
- **Source**: TOILET_COUNTER_MESSAGE_NTR.ERB:898 uses TRYCALLFORM KOJO_WC_COUNTER_NTRrevelation_1_K{NO:TARGET}
- **Verification**: Implementation must call IKojoMessageService.KojoMessageWcCounter with NTR revelation action type
- **AC Impact**: AC#35 (WcCounterMessageNtr injects IKojoMessageService)

**C10: ITEM_WITHotherITEM Internal Self-Call**
- **Source**: TOILET_COUNTER_MESSAGE_ITEM.ERB:266 TRYCALLs ReportSetItems (same file, line 2273)
- **Verification**: ItemWithOtherItem() must directly call ReportSetItems() within same class
- **AC Impact**: AC#30 verifies ReportSetItems( call site exists in WcCounterMessageItem.cs

**C11: NTRrevelation_ATTACK External Exposure**
- **Source**: WC_SexHara_MESSAGE.ERB:32 TRYCALLs NTRrevelation_ATTACK from outside Phase 21 scope
- **Verification**: NtrRevelationAttack(CharacterId attacker) must be declared in IWcCounterMessageNtr interface; ERB source NTR.ERB:933-936 confirms ARG=attacker character only (no second parameter)
- **AC Impact**: AC#31 verifies NtrRevelationAttack appears in IWcCounterMessageNtr.cs with attacker-only signature

**C12: Private Helper Functions**
- **Source**: TOILET_COUNTER_MESSAGE_NTR.ERB:960-1034 defines NTRreversal_SOURCE and NTRagree_SOURCE as internal helper functions (not called externally)
- **Verification**: These must be private methods in WcCounterMessageNtr, not exposed through IWcCounterMessageNtr
- **AC Impact**: AC#32 and AC#33 verify private visibility of NtrReversalSource and NtrAgreeSource

**C13: WC_応答分類 Response Branching**
- **Source**: Pattern across all ITEM functions — each checks CFLAG:TARGET:WC_応答分類 (SELECTCASE) for response category branching (虐待/冷淡/快虐/性戯/誘惑/愛玩/同情/子供)
- **Verification**: Unit tests must cover representative response category branches
- **AC Impact**: Covered via behavioral test AC#52 (WC_応答分類 response branching verification)

**C14: Constructor Dependencies**
- **Source**: Interface Dependency Scan across all 3 investigations
- **Verification**: Each constructor parameter type must appear as field in implementation class
- **AC Impact**: Grep-based ACs verifying field declarations for each injected interface

**C15: NTR_NAME Stub via INtrUtilityService**
- **Source**: TOILET_COUNTER_MESSAGE_NTR.ERB:850 calls NTR_NAME(0) for NTR character name display
- **Verification**: INtrUtilityService declares GetNtrName default method; WcCounterMessageNtr calls _ntrUtility.GetNtrName()
- **AC Impact**: AC#42 verifies method declaration; AC#43 verifies call site

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| Predecessor | F805 | [DONE] | WC Counter Source + Message Core -- defines WcCounterMessage.cs, INtrRevelationHandler, dispatch infrastructure |
| Successor | F806 | [DRAFT] | WC Counter Message SEX -- SEX TRYCALLs F808 bottomCLOTH_OFF (ITEM.ERB:1182 via SEX.ERB:407,2578), bottomCLOTH_OUT (ITEM.ERB:1072 via SEX.ERB:1540,1815,1903,2037), withFirstSex (NTR.ERB:109 via SEX.ERB:1492-1516), withPETTING (NTR.ERB:209 via SEX.ERB:2499) |
| Successor | F807 | [DRAFT] | WC Counter Message TEASE -- TEASE TRYCALLs F808 NTRrevelation (NTR.ERB:849 via TEASE.ERB:15,1860,2542), setITEMs (ITEM.ERB:2111 via TEASE.ERB:823), bottomITEM_SHOW (ITEM.ERB:1731 via TEASE.ERB:837), CLOTH_OUT (ITEM.ERB:1310 via TEASE.ERB:1372), OSHIKKO_looking (NTR.ERB:44 via TEASE.ERB:3284-3308), all device functions |
| Successor | F813 | [DRAFT] | Post-Phase Review Phase 21 |
| External | NTR_UTIL.ERB | unmigrated | NTR_RESET_VISITOR_ACTION (NTR_UTIL.ERB:1110) -- needed by NTRrevelation_ATTACK; stub via INtrUtilityService |
| External | NTR_NAME function | unmigrated | ShopSystem.NtrName throws NotImplementedException; needed by NTRrevelation (NTR.ERB:850) |
| External | KOJO WC NTR dispatch | unmigrated | KOJO_WC_COUNTER_NTRrevelation_1_K{NO:TARGET} -- stub via IKojoMessageService |
| External | WC_SexHara_MESSAGE.ERB | unmigrated (Phase 26) | Calls NTRrevelation_ATTACK (WC_SexHara_MESSAGE.ERB:32) -- outside Phase 21 scope |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |
| External | - | None | Non-feature dependency (unmigrated ERB, external code). Reference only. No blocking. |

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
| "SSOT for all WC counter message logic" | All 17 ITEM + 6 NTR public functions must have C# interface methods with non-stub implementations, including correct call-site behaviors (KOJO dispatch, DATETIME timestamp) | AC#1, AC#2, AC#3, AC#4, AC#47, AC#48, AC#49, AC#50, AC#51, AC#52 |
| "injectable C# services behind interfaces" | IWcCounterMessageItem + IWcCounterMessageNtr interfaces must exist with DI registration | AC#1, AC#2, AC#7, AC#8, AC#9 |
| "enabling TDD" | Unit test files must exist for ITEM, NTR, and NtrRevelation handlers | AC#10, AC#11, AC#12 |
| "enabling TDD" | Behavioral test scenarios must verify CFLAG mutations (C5), TALENT mutations (C5), SOURCE writes (C6), SETBIT operations (C7), and favor/surrender calculations (C12) | AC#34, AC#53, AC#35, AC#36, AC#39, AC#40, AC#41, AC#54, AC#44, AC#45, AC#46 |
| "eliminating runtime TRYCALL coupling" | F808 creates injectable C# interfaces (IWcCounterMessageItem, IWcCounterMessageNtr) and DI registrations enabling F806/F807 to inject interfaces instead of TRYCALL; null sentinel replaced with concrete NtrRevelationHandler. TRYCALL elimination completes when F806/F807 consume these interfaces — F808 provides the prerequisite infrastructure (interfaces + DI) verified by these ACs | AC#1, AC#2, AC#3, AC#4, AC#5, AC#6, AC#7, AC#8, AC#9 |
| "F808 is Predecessor of F806 and F807" | F806/F807 dependency tables must list F808 as Predecessor | AC#15, AC#16 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IWcCounterMessageItem interface file exists | file | Glob(Era.Core/Counter/IWcCounterMessageItem.cs) | exists | - | [ ] |
| 2 | IWcCounterMessageItem interface declares 17 methods | code | Grep(path="Era.Core/Counter/IWcCounterMessageItem.cs", pattern="^\\s+(void|int|bool).*\\(") | gte | 17 | [ ] |
| 3 | IWcCounterMessageNtr interface file exists | file | Glob(Era.Core/Counter/IWcCounterMessageNtr.cs) | exists | - | [ ] |
| 4 | IWcCounterMessageNtr interface declares 6 public methods | code | Grep(path="Era.Core/Counter/IWcCounterMessageNtr.cs", pattern="^\\s+(void|int|bool).*\\(") | gte | 6 | [ ] |
| 5 | NtrRevelationHandler concrete class exists | code | Grep(Era.Core/Counter/) | matches | `class NtrRevelationHandler.*INtrRevelationHandler` | [ ] |
| 6 | Null sentinel removed from ServiceCollectionExtensions | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | not_matches | `INtrRevelationHandler.*=> null!` | [ ] |
| 7 | All 3 new handlers registered in DI | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs, pattern="AddSingleton.*INtrRevelationHandler.*NtrRevelationHandler\|AddSingleton.*IWcCounterMessageItem.*WcCounterMessageItem\|AddSingleton.*IWcCounterMessageNtr.*WcCounterMessageNtr") | gte | 3 | [ ] |
| 8 | WcCounterMessageItem implementation class exists | code | Grep(Era.Core/Counter/) | matches | `class WcCounterMessageItem.*IWcCounterMessageItem` | [ ] |
| 9 | WcCounterMessageNtr implementation class exists | code | Grep(Era.Core/Counter/) | matches | `class WcCounterMessageNtr.*IWcCounterMessageNtr` | [ ] |
| 10 | WcCounterMessageItem unit test file exists | file | Glob(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs) | exists | - | [ ] |
| 11 | WcCounterMessageNtr unit test file exists | file | Glob(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs) | exists | - | [ ] |
| 12 | NtrRevelationHandler unit test file exists | file | Glob(Era.Core.Tests/Counter/NtrRevelationHandlerTests.cs) | exists | - | [ ] |
| 13 | WcCounterMessageItem injects all 5 interfaces | code | Grep(Era.Core/Counter/WcCounterMessageItem.cs, pattern="IVariableStore\|ITEquipVariables\|ITextFormatting\|IConsoleOutput\|IEngineVariables") | gte | 5 | [ ] |
| 14 | WcCounterMessageNtr injects all 9 interfaces | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="IVariableStore\|IConsoleOutput\|INtrUtilityService\|ICounterUtilities\|IRandomProvider\|IKojoMessageService\|ITEquipVariables\|IEngineVariables\|ITextFormatting") | gte | 9 | [ ] |
| 15 | F806 dependency table lists F808 as Predecessor | code | Grep(pm/features/feature-806.md) | matches | `Predecessor.*F808` | [ ] |
| 16 | F807 dependency table lists F808 as Predecessor | code | Grep(pm/features/feature-807.md) | matches | `Predecessor.*F808` | [ ] |
| 17 | NtrRevelationHandler injects IWcCounterMessageNtr | code | Grep(Era.Core/Counter/NtrRevelationHandler.cs) | matches | `IWcCounterMessageNtr` | [ ] |
| 18 | INtrUtilityService extended with NtrResetVisitorAction | code | Grep(Era.Core/Counter/INtrUtilityService.cs) | matches | `NtrResetVisitorAction` | [ ] |
| 19 | WcCounterMessageNtr.NtrRevelationAttack delegates to NtrResetVisitorAction with attacker.Value | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs) | matches | `\.NtrResetVisitorAction\(attacker\.Value` | [ ] |
| 20 | ITEM_WITHotherITEM delegates to ReportSetItems (call site + declaration) | code | Grep(Era.Core/Counter/WcCounterMessageItem.cs, pattern="ReportSetItems\\(") | gte | 2 | [ ] |
| 21 | NTRrevelation_ATTACK exposed through IWcCounterMessageNtr as NtrRevelationAttack(CharacterId attacker) | code | Grep(Era.Core/Counter/IWcCounterMessageNtr.cs) | matches | `NtrRevelationAttack` | [ ] |
| 22 | NTRreversal_SOURCE is private in implementation | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs) | matches | `private.*NtrReversalSource` | [ ] |
| 23 | NTRagree_SOURCE is private in implementation | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs) | matches | `private.*NtrAgreeSource` | [ ] |
| 24 | withPETTING uses BitfieldUtility for SETBIT | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs) | matches | `BitfieldUtility\.(SetBit|GetBit)` | [ ] |
| 25 | NtrRevelationHandler.Execute forwards both parameters to NtrRevelation | code | Grep(Era.Core/Counter/NtrRevelationHandler.cs) | matches | `\.NtrRevelation\([^,]+,\s*[^)]+\)` | [ ] |
| 26 | C# solution builds without errors | build | dotnet build Era.Core/ | succeeds | - | [ ] |
| 27 | All unit tests pass | test | dotnet test Era.Core.Tests/ --blame-hang-timeout 10s | succeeds | - | [ ] |
| 28 | No TODO/FIXME/HACK in new ITEM handler | code | Grep(Era.Core/Counter/WcCounterMessageItem.cs) | not_matches | `TODO|FIXME|HACK` | [ ] |
| 29 | No TODO/FIXME/HACK in new NTR handler | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs) | not_matches | `TODO|FIXME|HACK` | [ ] |
| 30 | No TODO/FIXME/HACK in NtrRevelationHandler | code | Grep(Era.Core/Counter/NtrRevelationHandler.cs) | not_matches | `TODO|FIXME|HACK` | [ ] |
| 31 | WcCounterMessageItem implementation file exists | file | Glob(Era.Core/Counter/WcCounterMessageItem.cs) | exists | - | [ ] |
| 32 | WcCounterMessageNtr implementation file exists | file | Glob(Era.Core/Counter/WcCounterMessageNtr.cs) | exists | - | [ ] |
| 33 | NtrRevelationHandler implementation file exists | file | Glob(Era.Core/Counter/NtrRevelationHandler.cs) | exists | - | [ ] |
| 34 | WcCounterMessageNtrTests verifies NtrRevelation CFLAG state mutations (管理人) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="Kanrinin\|管理人") | gte | 2 | [ ] |
| 35 | WcCounterMessageNtrTests verifies SOURCE increments (multi-path) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="SetSource\|SourceIndex") | gte | 3 | [ ] |
| 36 | WcCounterMessageNtrTests verifies BitfieldUtility usage (multi-path) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="BitfieldUtility") | gte | 3 | [ ] |
| 37 | F806 no longer lists F808 as Related | code | Grep(pm/features/feature-806.md) | not_matches | `Related.*F808` | [ ] |
| 38 | F807 no longer lists F808 as Related | code | Grep(pm/features/feature-807.md) | not_matches | `Related.*F808` | [ ] |
| 39 | WcCounterMessageItemTests verifies WC_応答分類 response branching | code | Grep(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs) | matches | `応答分類\|ResponseCategory\|WcResponseCategory` | [ ] |
| 40 | WcCounterMessageItemTests verifies EQUIP state checks | code | Grep(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs) | matches | `ITEquipVariables\|EquipVariables\|GetEquip` | [ ] |
| 41 | WcCounterMessageItemTests verifies CFLAG/FLAG variable access | code | Grep(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs) | matches | `GetCharacterFlag\|SetCharacterFlag\|IVariableStore` | [ ] |
| 42 | INtrUtilityService declares GetNtrName method | code | Grep(Era.Core/Counter/INtrUtilityService.cs) | matches | `GetNtrName` | [ ] |
| 43 | WcCounterMessageNtr calls GetNtrName for NTR_NAME | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs) | matches | `\.GetNtrName\(` | [ ] |
| 44 | WcCounterMessageNtrTests contains Moq Verify or xUnit Assert calls | code | Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs) | matches | `Assert\.\|\.Verify\(` | [ ] |
| 45 | WcCounterMessageItemTests contains Moq Verify or xUnit Assert calls | code | Grep(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs) | matches | `Assert\.\|\.Verify\(` | [ ] |
| 46 | NtrRevelationHandlerTests contains Moq Verify or xUnit Assert calls | code | Grep(Era.Core.Tests/Counter/NtrRevelationHandlerTests.cs) | matches | `Assert\.\|\.Verify\(` | [ ] |
| 47 | WcCounterMessageItem implementation contains non-stub logic (injected service usage) | code | Grep(Era.Core/Counter/WcCounterMessageItem.cs, pattern="_variables\\\.\|_console\\\.\|_engine\\\.\|_tEquipVariables\\\.\|_textFormatting\\\.") | gte | 10 | [ ] |
| 48 | WcCounterMessageNtr implementation contains non-stub logic (injected service usage) | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="_variables\\\.\|_console\\\.\|_engine\\\.\|_ntrUtility\\\.\|_kojoMessage\\\.\|_counterUtilities\\\.\|_randomProvider\\\.") | gte | 10 | [ ] |
| 49 | WcCounterMessageNtr calls IKojoMessageService for KOJO dispatch with target.Value | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="_kojoMessage\\.[A-Za-z]+.*target\\.Value") | matches | - | [ ] |
| 50 | WcCounterMessageNtr calls GetDateTime for CFLAG timestamp | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs) | matches | `\.GetDateTime\(` | [ ] |
| 51 | No NotImplementedException stubs in ITEM handler | code | Grep(Era.Core/Counter/WcCounterMessageItem.cs) | not_matches | `NotImplementedException` | [ ] |
| 52 | No NotImplementedException stubs in NTR handler | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs) | not_matches | `NotImplementedException` | [ ] |
| 53 | WcCounterMessageNtrTests verifies TALENT state mutations | code | Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="SetTalent\|TalentIndex") | matches | - | [ ] |
| 54 | WcCounterMessageNtrTests covers favor/surrender calculation (C12) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="Reversal\|Agree\|favor\|surrender") | matches | - | [ ] |
| 55 | WithPetting uses offender parameter for SetSource calls | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetSource\\(offender") | matches | - | [ ] |
| 56 | WithPetting SETBIT uses GetMaster for CFLAG:MASTER:WC_箇所埋まり | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="GetMaster") | matches | - | [ ] |
| 57 | WcCounterMessageItem reads WC_応答分類 for response branching | code | Grep(Era.Core/Counter/WcCounterMessageItem.cs) | matches | `応答分類\|ResponseCategory\|WcResponseCategory` | [ ] |
| 58 | WcCounterMessageNtr implementation calls SetTalent (C5 TALENT mutation) | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs) | matches | `SetTalent` | [ ] |
| 59 | WithPetting does not use GetTarget for SetSource (wrong-character prevention) | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetSource.*GetTarget") | not_matches | - | [ ] |
| 60 | WcCounterMessageNtr calls SetCharacterFlag for NtrRevelation CFLAG mutations (C5) | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetCharacterFlag.*Kanrinin|SetCharacterFlag.*管理人") | gte | 2 | [ ] |
| 61 | NtrRevelation CFLAG writes target MASTER character (not target/attacker) | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetCharacterFlag.*master|SetCharacterFlag.*Master") | gte | 2 | [ ] |
| 62 | IWcCounterMessageItem declares critical ERB-mapped methods by name | code | Grep(Era.Core/Counter/IWcCounterMessageItem.cs, pattern="BottomClothOff\|BottomClothOut\|SetItems\|BottomItemShow\|ClothOut\|VibratorIn") | gte | 6 | [ ] |
| 63 | IWcCounterMessageNtr declares critical ERB-mapped methods by name | code | Grep(Era.Core/Counter/IWcCounterMessageNtr.cs, pattern="RotorOut\|OshikkoLooking\|WithFirstSex\|WithPetting\|NtrRevelation\|NtrRevelationAttack") | gte | 6 | [ ] |
| 64 | NtrReversalSource is called from NtrRevelation | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="NtrReversalSource\(") | gte | 4 | [ ] |
| 65 | WcCounterMessageItemTests verifies ItemWithOtherItem triggers ReportSetItems | code | Grep(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs, pattern="ItemWithOtherItem\|ReportSetItems") | gte | 2 | [ ] |
| 66 | WcCounterMessageNtrTests verifies WC_管理人は誰 is reset to 0 | code | Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="管理人は誰.*0\|Kanrinin.*0\|SetCharacterFlag.*管理人.*0") | matches | - | [ ] |
| 67 | NtrAgreeSource is called from NtrRevelation | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="NtrAgreeSource\(") | matches | - | [ ] |
| 68 | WcCounterMessageNtrTests verifies NtrRevelationAttack state mutations | code | Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="NtrRevelationAttack\|RevelationAttack") | gte | 2 | [ ] |
| 69 | WcCounterMessageNtr.cs implements NtrRevelationAttack visitor FLAG resets | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetFlag.*訪問者\|SetFlag.*Visitor") | gte | 4 | [ ] |
| 70 | WcCounterMessageNtr.cs writes literal 0 for WC_管理人は誰 CFLAG reset | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetCharacterFlag.*[Mm]aster.*[Kk]anrinin.*0\|SetCharacterFlag.*[Mm]aster.*管理人.*0") | matches | - | [ ] |
| 71 | WcCounterMessageNtr.cs implements NtrRevelationAttack attacker CFLAG writes | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetCharacterFlag.*attacker") | gte | 2 | [ ] |
| 72 | NtrRevelationAttack calls NtrReversalSource with attacker argument | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="NtrReversalSource\(attacker") | matches | - | [ ] |
| 73 | WcCounterMessageNtrTests verifies WC_前任管理人 assignment | code | Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="前任管理人\|ZenninKanrinin\|PreviousManager") | matches | - | [ ] |
| 74 | WcCounterMessageNtr.cs sets WC_前任管理人 to target-derived value | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetCharacterFlag.*[Mm]aster.*[Zz]ennin.*target\|SetCharacterFlag.*[Mm]aster.*前任.*target") | matches | - | [ ] |
| 75 | NtrRevelation calls NtrAgreeSource with target argument | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="NtrAgreeSource\(target") | matches | - | [ ] |
| 76 | NtrRevelation calls NtrReversalSource with target argument | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="NtrReversalSource\(target") | matches | - | [ ] |
| 77 | NtrRevelation writes TALENT:管理人=0 with literal 0 (impl-side C5) | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetTalent.*[Tt]arget.*0\|SetTalent.*0.*[Tt]arget") | matches | - | [ ] |
| 78 | WcCounterMessageNtrTests verifies TALENT:管理人 reset to 0 (test-side C5) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="SetTalent.*0\|TalentIndex.*0") | matches | - | [ ] |
| 79 | WcCounterMessageNtrTests verifies NtrRevelationAttack visitor FLAG resets (test-side) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="SetFlag.*訪問者\|SetFlag.*Visitor\|Visitor.*Flag") | gte | 2 | [ ] |
| 80 | WcCounterMessageItemTests covers non-branching ITEM functions (method names in test) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs, pattern="NippleCapSet\|CliCapSet\|CliCapOut\|RotorPantsOut\|RotorIn\|BottomClothOut\|BottomClothOff\|ClothOut\|BottomItemShow\|SetItems\|ReportSetItems") | gte | 5 | [ ] |
| 81 | WcCounterMessageNtrTests verifies NtrRevelationAttack attacker CFLAG assertions (test-side) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="attacker.*CFLAG\|attacker.*Kanrinin\|SetCharacterFlag.*attacker") | gte | 2 | [ ] |
| 82 | WcCounterMessageNtr KOJO dispatch includes action type parameter | code | Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="_kojoMessage\..*actionType\|_kojoMessage\..*[Rr]evelation") | matches | - | [ ] |


### AC Details

**AC#1: IWcCounterMessageItem interface file exists**
- **Test**: `Glob(Era.Core/Counter/IWcCounterMessageItem.cs)`
- **Expected**: File exists
- **Rationale**: Goal item 1 requires creating this interface. File must exist as deliverable. (C3)

**AC#2: IWcCounterMessageItem interface declares 17 methods**
- **Test**: `Grep(path="Era.Core/Counter/IWcCounterMessageItem.cs", pattern="^\s+(void|int|bool).*\(")` counting method signatures
- **Expected**: Count >= 17 (one per ERB function: NIPPLECAP_SET, CLICAP_SET, NIPPLECAP_OUT, CLICAP_OUT, ITEM_WITHotherITEM, ROTORpants_OUT, VIBRATOR_IN, ANALVIB_IN, ROTOR_IN(offender, insertionLocation), InsertITEM_IN, InsertITEM_OUT, bottomCLOTH_OUT(offender, targetArea), bottomCLOTH_OFF, CLOTH_OUT(offender, nippleCapStart), bottomITEM_SHOW, setITEMs, ReportSetItems)
- **Rationale**: C3 requires all 17 ITEM functions callable by F806/F807. Count verification ensures completeness. (C3)

**AC#3: IWcCounterMessageNtr interface file exists**
- **Test**: `Glob(Era.Core/Counter/IWcCounterMessageNtr.cs)`
- **Expected**: File exists
- **Rationale**: Goal item 2 requires creating this interface. (C4)

**AC#4: IWcCounterMessageNtr interface declares 6 public methods**
- **Test**: `Grep(path="Era.Core/Counter/IWcCounterMessageNtr.cs", pattern="^\s+(void|int|bool).*\(")` counting method signatures
- **Expected**: Count >= 6 (ROTOR_OUT, OSHIKKO_looking, withFirstSex(offender, painLevel), withPETTING, NTRrevelation, NTRrevelation_ATTACK(attacker))
- **Rationale**: C4 and C11 require all 6 NTR public functions exposed through interface. (C4, C11)

**AC#5: NtrRevelationHandler concrete class exists**
- **Test**: `Grep(Era.Core/Counter/)` for `class NtrRevelationHandler.*INtrRevelationHandler`
- **Expected**: Pattern matches (concrete class implementing INtrRevelationHandler)
- **Rationale**: Goal item 3 requires implementing NtrRevelationHandler. F805 deferred obligation. (C1)

**AC#6: Null sentinel removed from ServiceCollectionExtensions**
- **Test**: `Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs)` for `INtrRevelationHandler.*=> null!`
- **Expected**: Pattern NOT found (null sentinel replaced)
- **Rationale**: C2 requires replacing the null sentinel. Currently exists at line 194. (C2)

**AC#7: All 3 new handlers registered in DI**
- **Test**: `Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs, pattern="AddSingleton.*INtrRevelationHandler.*NtrRevelationHandler\|AddSingleton.*IWcCounterMessageItem.*WcCounterMessageItem\|AddSingleton.*IWcCounterMessageNtr.*WcCounterMessageNtr")` count occurrences
- **Expected**: Count >= 3 (all 3 AddSingleton registrations present)
- **Rationale**: Goal item 4 requires registering all new handlers in DI. Grouped from 3 individual ACs: INtrRevelationHandler (replacing null sentinel, C1/C2), IWcCounterMessageItem (C14), IWcCounterMessageNtr (C14). (C2, C14)

**AC#8: WcCounterMessageItem implementation class exists**
- **Test**: `Grep(Era.Core/Counter/)` for `class WcCounterMessageItem.*IWcCounterMessageItem`
- **Expected**: Pattern matches
- **Rationale**: Goal item 1 requires not just the interface but a concrete implementation. (C3)

**AC#9: WcCounterMessageNtr implementation class exists**
- **Test**: `Grep(Era.Core/Counter/)` for `class WcCounterMessageNtr.*IWcCounterMessageNtr`
- **Expected**: Pattern matches
- **Rationale**: Goal item 2 requires a concrete NTR implementation class. (C4)


**AC#10: WcCounterMessageItem unit test file exists**
- **Test**: `Glob(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs)`
- **Expected**: File exists
- **Rationale**: TDD requirement -- ITEM handler must have unit tests.

**AC#11: WcCounterMessageNtr unit test file exists**
- **Test**: `Glob(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs)`
- **Expected**: File exists
- **Rationale**: TDD requirement -- NTR handler must have unit tests.

**AC#12: NtrRevelationHandler unit test file exists**
- **Test**: `Glob(Era.Core.Tests/Counter/NtrRevelationHandlerTests.cs)`
- **Expected**: File exists
- **Rationale**: TDD requirement -- NtrRevelationHandler must have dedicated tests. (C1, C5)

**AC#13: WcCounterMessageItem injects all 5 interfaces**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageItem.cs, pattern="IVariableStore\|ITEquipVariables\|ITextFormatting\|IConsoleOutput\|IEngineVariables")` count occurrences
- **Expected**: Count >= 5 (all 5 constructor-injected interfaces present in file)
- **Rationale**: C14 requires constructor injection for all dependent interfaces. Grouped from 5 individual ACs (IVariableStore for CFLAG/FLAG/SOURCE access, ITEquipVariables for EQUIP state, ITextFormatting for PRINTFORM, IConsoleOutput for PRINT output, IEngineVariables for TARGET/MASTER). (C14)

**AC#14: WcCounterMessageNtr injects all 9 interfaces**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="IVariableStore\|IConsoleOutput\|INtrUtilityService\|ICounterUtilities\|IRandomProvider\|IKojoMessageService\|ITEquipVariables\|IEngineVariables\|ITextFormatting")` count occurrences
- **Expected**: Count >= 9 (all 9 constructor-injected interfaces present in file)
- **Rationale**: C14 requires constructor injection for all dependent interfaces. Grouped from 9 individual ACs: IVariableStore (CFLAG/TALENT/SOURCE via C5/C6/C14), IConsoleOutput (PRINT output, C14), INtrUtilityService (NTR_RESET_VISITOR_ACTION delegation, C8/C14), ICounterUtilities (DATETIME(), C14), IRandomProvider (PRINTDATA random, C14), IKojoMessageService (KOJO dispatch, C9/C14), ITEquipVariables (EQUIP state branching, C14), IEngineVariables (TARGET/MASTER, C14), ITextFormatting (PRINTFORM, C14). (C14)

**AC#15: F806 dependency table lists F808 as Predecessor**
- **Test**: `Grep(pm/features/feature-806.md)` for `Predecessor.*F808`
- **Expected**: Pattern matches (currently shows Related, must be changed to Predecessor)
- **Rationale**: Goal item 5 requires reclassifying F808 as Predecessor. F806 calls bottomCLOTH_OFF, bottomCLOTH_OUT, withFirstSex, withPETTING from F808. (C3)

**AC#16: F807 dependency table lists F808 as Predecessor**
- **Test**: `Grep(pm/features/feature-807.md)` for `Predecessor.*F808`
- **Expected**: Pattern matches (currently shows Related, must be changed to Predecessor)
- **Rationale**: Goal item 5 requires reclassifying F808 as Predecessor. F807 calls NTRrevelation, setITEMs, bottomITEM_SHOW, CLOTH_OUT, and all device functions from F808. (C3)


**AC#17: NtrRevelationHandler injects IWcCounterMessageNtr**
- **Test**: `Grep(Era.Core/Counter/NtrRevelationHandler.cs)` for `IWcCounterMessageNtr`
- **Expected**: Pattern matches
- **Rationale**: NtrRevelationHandler is a pure bridge; must inject IWcCounterMessageNtr to delegate Execute() to NtrRevelation(). (C1)


**AC#18: INtrUtilityService extended with NtrResetVisitorAction**
- **Test**: `Grep(Era.Core/Counter/INtrUtilityService.cs)` for `NtrResetVisitorAction`
- **Expected**: Pattern matches
- **Rationale**: C8 requires INtrUtilityService to be extended with NtrResetVisitorAction method for NTRrevelation_ATTACK delegation. (C8)

**AC#19: WcCounterMessageNtr.NtrRevelationAttack delegates to NtrResetVisitorAction with attacker**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs)` for `\.NtrResetVisitorAction\(attacker\.Value`
- **Expected**: Pattern matches (CharacterId unwrapped to int via .Value as required by NtrResetVisitorAction(int characterIndex) signature)
- **Rationale**: C8 requires actual delegation call in WcCounterMessageNtr.NtrRevelationAttack(). (C8)


**AC#20: ITEM_WITHotherITEM delegates to ReportSetItems (call site + declaration)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageItem.cs, pattern="ReportSetItems\\(")` count occurrences
- **Expected**: Count >= 2 (1 = method declaration only; 2+ = declaration + call site from ItemWithOtherItem body)
- **Rationale**: C10 requires internal self-call pattern: ITEM_WITHotherITEM calls ReportSetItems (ITEM.ERB:266). Using `gte 2` ensures the pattern appears both in the method declaration and as an actual call site within the ItemWithOtherItem method body, distinguishing declaration-only from actual internal delegation. (C10)

**AC#21: NTRrevelation_ATTACK exposed through IWcCounterMessageNtr with attacker-only signature**
- **Test**: `Grep(Era.Core/Counter/IWcCounterMessageNtr.cs)` for `NtrRevelationAttack`
- **Expected**: Pattern matches (signature: `void NtrRevelationAttack(CharacterId attacker)` — attacker only, no actionType parameter per NTR.ERB:933-936)
- **Rationale**: C11 requires NTRrevelation_ATTACK to be callable by WC_SexHara_MESSAGE (Phase 26) through the interface. ERB source at NTR.ERB:933-936 confirms ARG=attacker character only; no second parameter. (C11)

**AC#22: NTRreversal_SOURCE is private in implementation**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs)` for `private.*NtrReversalSource`
- **Expected**: Pattern matches
- **Rationale**: C12 specifies NTRreversal_SOURCE and NTRagree_SOURCE are internal helpers, must be private methods. (C12)

**AC#23: NTRagree_SOURCE is private in implementation**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs)` for `private.*NtrAgreeSource`
- **Expected**: Pattern matches
- **Rationale**: C12 specifies these as internal helper functions. (C12)

**AC#24: withPETTING uses BitfieldUtility for SETBIT**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs)` for `BitfieldUtility\.(SetBit|GetBit)`
- **Expected**: Pattern matches
- **Rationale**: C7 requires SETBIT/GETBIT operations on WC_箇所埋まり to use BitfieldUtility. 11 SETBIT calls in ERB source. (C7)


**AC#25: NtrRevelationHandler.Execute forwards both parameters to NtrRevelation**
- **Test**: `Grep(Era.Core/Counter/NtrRevelationHandler.cs)` for `\.NtrRevelation\([^,]+,\s*[^)]+\)`
- **Expected**: Pattern matches (verifies two-argument forwarding call regardless of parameter naming)
- **Rationale**: Bridge pattern verification — Execute() must call IWcCounterMessageNtr.NtrRevelation(). (C1)



**AC#26: C# solution builds without errors**
- **Test**: `dotnet build Era.Core/`
- **Expected**: Build succeeds (exit code 0)
- **Rationale**: TreatWarningsAsErrors=true; all new code must compile warning-free. (Technical Constraint)

**AC#27: All unit tests pass**
- **Test**: `dotnet test Era.Core.Tests/ --blame-hang-timeout 10s`
- **Expected**: All tests pass
- **Rationale**: TDD requirement; no regressions allowed.

**AC#28: No TODO/FIXME/HACK in new ITEM handler**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageItem.cs)` for `TODO|FIXME|HACK`
- **Expected**: Pattern NOT found
- **Rationale**: Zero technical debt policy.

**AC#29: No TODO/FIXME/HACK in new NTR handler**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs)` for `TODO|FIXME|HACK`
- **Expected**: Pattern NOT found
- **Rationale**: Zero technical debt policy.

**AC#30: No TODO/FIXME/HACK in NtrRevelationHandler**
- **Test**: `Grep(Era.Core/Counter/NtrRevelationHandler.cs)` for `TODO|FIXME|HACK`
- **Expected**: Pattern NOT found
- **Rationale**: Zero technical debt policy.

**AC#31: WcCounterMessageItem implementation file exists**
- **Test**: `Glob(Era.Core/Counter/WcCounterMessageItem.cs)`
- **Expected**: File exists
- **Rationale**: Concrete implementation file must be created as deliverable. (C3)

**AC#32: WcCounterMessageNtr implementation file exists**
- **Test**: `Glob(Era.Core/Counter/WcCounterMessageNtr.cs)`
- **Expected**: File exists
- **Rationale**: Concrete implementation file must be created as deliverable. (C4)

**AC#33: NtrRevelationHandler implementation file exists**
- **Test**: `Glob(Era.Core/Counter/NtrRevelationHandler.cs)`
- **Expected**: File exists
- **Rationale**: Concrete implementation file must be created as deliverable. (C1)

**AC#34: WcCounterMessageNtrTests verifies NtrRevelation CFLAG state mutations (管理人)**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="Kanrinin|管理人")` count occurrences
- **Expected**: Count >= 2 (both CFLAG constants: WC_管理人は誰 and WC_前任管理人)
- **Rationale**: C5 requires two distinct NtrRevelation CFLAG writes (WC_管理人は誰=0 at NTR.ERB:918 and WC_前任管理人 at NTR.ERB:919). Pattern narrowed from `SetCharacterFlag|CflagWc` to `Kanrinin|管理人` to disambiguate from withPETTING SETBIT tests that also use SetCharacterFlag (via BitfieldUtility read-modify-write on WC_箇所埋まり). Requiring gte 2 ensures both CFLAG constants are referenced in the test file, not just one. (C5)

**AC#35: WcCounterMessageNtrTests verifies SOURCE increments (multi-path)**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="SetSource|SourceIndex")` count occurrences
- **Expected**: Count >= 3 (multi-path coverage of representative body-part SOURCE writes)
- **Rationale**: C6 requires 22 SOURCE:ARG:X += 100 writes in withPETTING across distinct body-part targeting paths. A single occurrence would indicate trivial coverage. Requiring gte 3 ensures representative path coverage (e.g., head, chest/groin, limbs). (C6)

**AC#36: WcCounterMessageNtrTests verifies BitfieldUtility usage (multi-path)**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="BitfieldUtility")` count occurrences
- **Expected**: Count >= 3 (multi-path coverage of representative bitfield operations)
- **Rationale**: C7 requires 11 SETBIT operations on WC_箇所埋まり using BitfieldUtility. Requiring gte 3 ensures multiple distinct bit position scenarios are tested (e.g., head, body, limb SETBIT calls), following the AC#48 multi-path pattern. (C7)

**AC#37: F806 no longer lists F808 as Related**
- **Test**: `Grep(pm/features/feature-806.md)` for `Related.*F808`
- **Expected**: Pattern NOT found (old Related row must be removed, not just augmented with a Predecessor row)
- **Rationale**: Goal item 5 requires reclassification from Related to Predecessor, not addition of a duplicate row. (C3)

**AC#38: F807 no longer lists F808 as Related**
- **Test**: `Grep(pm/features/feature-807.md)` for `Related.*F808`
- **Expected**: Pattern NOT found
- **Rationale**: Same as AC#37 for F807 (parallel to F806 Related removal verification). (C3)

**AC#39: WcCounterMessageItemTests verifies WC_応答分類 response branching**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs)` for `応答分類|ResponseCategory|WcResponseCategory`
- **Expected**: Pattern matches
- **Rationale**: C13 requires branch coverage for response categories (虚待/冷淡/快虚/性戯/誘惑/愛玩/同情/子供). Unit tests must reference the WC_応答分類 response branching. (C13)

**AC#40: WcCounterMessageItemTests verifies EQUIP state checks**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs)` for `ITEquipVariables|EquipVariables|GetEquip`
- **Expected**: Pattern matches
- **Rationale**: ITEM functions check EQUIP state for cloth/device operations (C3, C14). Unit tests must verify EQUIP-based branching.

**AC#41: WcCounterMessageItemTests verifies CFLAG/FLAG variable access**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs)` for `GetCharacterFlag|SetCharacterFlag|IVariableStore`
- **Expected**: Pattern matches
- **Rationale**: ITEM functions read CFLAG for WC_応答分類 branching (C13). Unit tests must mock and verify variable access patterns.


**AC#42: INtrUtilityService declares GetNtrName method**
- **Test**: `Grep(Era.Core/Counter/INtrUtilityService.cs)` for `GetNtrName`
- **Expected**: Pattern matches
- **Rationale**: NTR_NAME(0) at NTR.ERB:850 requires a C# equivalent. Default interface method with empty string stub follows established INtrUtilityService pattern (GetNWithVisitor, NtrResetVisitorAction). (C14)

**AC#43: WcCounterMessageNtr calls GetNtrName for NTR_NAME**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs)` for `\.GetNtrName\(`
- **Expected**: Pattern matches
- **Rationale**: WcCounterMessageNtr.NtrRevelation must call _ntrUtility.GetNtrName(0) — NTR_NAME(0) at NTR.ERB:850 passes literal 0 (NTR name system index), not target.Value. Prevents runtime NotImplementedException from ShopSystem.NtrName. (C15)

**AC#44: WcCounterMessageNtrTests contains Moq Verify or xUnit Assert calls**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs)` for `Assert\.|\.Verify\(`
- **Expected**: Pattern matches
- **Rationale**: Ensures test file contains actual behavioral assertions (xUnit Assert or Moq Verify), not just mock setup. Combined with AC#47-49 domain ACs, this guarantees meaningful behavioral coverage. (C5, C6, C7)

**AC#45: WcCounterMessageItemTests contains Moq Verify or xUnit Assert calls**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs)` for `Assert\.|\.Verify\(`
- **Expected**: Pattern matches
- **Rationale**: Ensures ITEM test file contains actual behavioral assertions. Combined with AC#52-54 domain ACs, guarantees meaningful behavioral coverage. (C13)

**AC#46: NtrRevelationHandlerTests contains Moq Verify or xUnit Assert calls**
- **Test**: `Grep(Era.Core.Tests/Counter/NtrRevelationHandlerTests.cs)` for `Assert\.|\.Verify\(`
- **Expected**: Pattern matches
- **Rationale**: Ensures NtrRevelationHandler bridge test file contains actual behavioral assertions verifying delegation behavior. (C1)

**AC#47: WcCounterMessageItem implementation contains non-stub logic**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageItem.cs, pattern="_variables\.|_console\.|_engine\.|_tEquipVariables\.|_textFormatting\.")` count occurrences
- **Expected**: Count >= 10 (17 methods each using at least some of all 5 injected services)
- **Rationale**: Prevents stub-only implementations. Pattern covers all 5 injected services: IVariableStore (_variables), IConsoleOutput (_console), IEngineVariables (_engine), ITEquipVariables (_tEquipVariables), ITextFormatting (_textFormatting). Threshold of 10 is conservative — real migration of 17 ITEM functions with text formatting will far exceed this. (C3, C14)

**AC#48: WcCounterMessageNtr implementation contains non-stub logic**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="_variables\.|_console\.|_engine\.|_ntrUtility\.|_kojoMessage\.|_counterUtilities\.|_randomProvider\.")` count occurrences
- **Expected**: Count >= 10 (6 public methods + 2 private helpers using all 9 injected services)
- **Rationale**: Same as AC#47 for the NTR implementation. Pattern expanded to cover all injected services including _kojoMessage (C9 KOJO dispatch), _counterUtilities (DATETIME), _randomProvider (PRINTDATA). Prevents stub implementations that omit critical dependencies. (C4, C5, C6, C7, C9, C14)

**AC#49: WcCounterMessageNtr calls IKojoMessageService for KOJO dispatch with target**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="_kojoMessage\.[A-Za-z]+.*target\.Value")` presence check
- **Expected**: Pattern matches
- **Rationale**: C9 requires NTRrevelation to dispatch KOJO via IKojoMessageService (NTR.ERB:898 TRYCALLFORM KOJO_WC_COUNTER_NTRrevelation_1_K{NO:TARGET}). IKojoMessageService takes int, so CharacterId→int unwrap via .Value is required (following AC#19 pattern). AC#14 verifies injection. This AC verifies the call site forwards target.Value to the KOJO dispatch method. (C9)

**AC#50: WcCounterMessageNtr calls GetDateTime for CFLAG timestamp**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs)` for `\.GetDateTime\(`
- **Expected**: Pattern matches
- **Rationale**: NTR.ERB:904,926 writes `CFLAG = DATETIME()` in NTRrevelation. ICounterUtilities.GetDateTime() is the C# equivalent. AC#14 verifies injection; this AC verifies the call site, following the AC#42+43 pairwise pattern for GetNtrName. (C14)

**AC#51: No NotImplementedException stubs in ITEM handler**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageItem.cs)` for `NotImplementedException`
- **Expected**: Pattern NOT found
- **Rationale**: Philosophy requires non-stub implementations. AC#28 catches TODO/FIXME/HACK but not NotImplementedException which is the codebase's established stub pattern (ShopSystem.NtrName). Combined with AC#47 (gte 10 service usage) provides comprehensive anti-stub gate.

**AC#52: No NotImplementedException stubs in NTR handler**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs)` for `NotImplementedException`
- **Expected**: Pattern NOT found
- **Rationale**: Same as AC#51 for the NTR implementation file. Combined with AC#48 (gte 10 service usage).

**AC#53: WcCounterMessageNtrTests verifies TALENT state mutations**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="SetTalent|TalentIndex")` presence check
- **Expected**: Pattern matches
- **Rationale**: C5 requires NTRrevelation TALENT write (管理人=0 at NTR.ERB:920). TALENT uses a distinct setter (SetTalent) from CFLAG (SetCharacterFlag). Split from CFLAG (AC#47) to guarantee both mutation types are independently tested. (C5)

**AC#54: WcCounterMessageNtrTests covers favor/surrender calculation (C12)**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="Reversal|Agree|favor|surrender")` presence check
- **Expected**: Pattern matches
- **Rationale**: C12 states "ACs should cover favor/surrender calculation outcomes." NtrReversalSource (NTR.ERB:960-996) computes favor reversal and NtrAgreeSource (NTR.ERB:997-1034) computes surrender. These private helpers are only testable indirectly through public methods (NtrRevelation calls them), but test files must reference the calculation concepts. (C12)

**AC#55: WithPetting uses offender parameter for SetSource calls**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetSource\(offender")` presence check
- **Expected**: Pattern matches
- **Rationale**: ERB SOURCE:ARG:X += 100 targets ARG (offender character, NTR.ERB:210: ARG=同席者のキャラ番号), not TARGET. Using _engine.GetTarget() would mutate wrong character. AC ensures SetSource targets the offender parameter from WithPetting(CharacterId offender). (C6)

**AC#56: WithPetting SETBIT uses GetMaster for CFLAG:MASTER:WC_箇所埋まり**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="GetMaster")` presence check
- **Expected**: Pattern matches
- **Rationale**: ERB SETBIT operations target CFLAG:MASTER:WC_箇所埋まり (toilet manager character, confirmed NTR.ERB:374). The bitfield belongs to MASTER, not TARGET or offender. Using _engine.GetTarget() would mutate wrong character's bitfield. Parallel to AC#55 (SOURCE targets offender). (C7)

**AC#57: WcCounterMessageItem reads WC_応答分類 for response branching**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageItem.cs)` for `応答分類|ResponseCategory|WcResponseCategory`
- **Expected**: Pattern matches
- **Rationale**: C13 requires each ITEM function to check WC_応答分類 for response branching. AC#39 verifies the test file references this concept; this AC verifies the implementation file actually reads the CFLAG. Prevents stub implementations that bypass response-category SELECTCASE branching. (C13)

**AC#58: WcCounterMessageNtr implementation calls SetTalent (C5 TALENT mutation)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs)` for `SetTalent`
- **Expected**: Pattern matches
- **Rationale**: C5 requires NtrRevelation to write TALENT:TARGET:管理人=0 (NTR.ERB:920). AC#53 verifies the test file references SetTalent, but no implementation-side AC existed. Following the pairwise pattern (implementation + test coverage), this ensures WcCounterMessageNtr actually calls SetTalent. (C5)

**AC#59: WithPetting does not use GetTarget for SetSource (wrong-character prevention)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetSource.*GetTarget")` negative check
- **Expected**: Pattern NOT found
- **Rationale**: AC#55 verifies SetSource(offender is called, but if the implementer writes `var offender = _engine.GetTarget()` then aliases target as offender, AC#55 passes while the wrong character is targeted. This not_matches AC catches direct GetTarget-to-SetSource chains. ERB confirms SOURCE:ARG writes use ARG=offender (NTR.ERB:210), not TARGET. (C6)

**AC#60: WcCounterMessageNtr calls SetCharacterFlag for NtrRevelation CFLAG mutations (C5)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetCharacterFlag.*Kanrinin|SetCharacterFlag.*管理人")` count occurrences
- **Expected**: Count >= 2 (both CFLAG constants: WC_管理人は誰 and WC_前任管理人)
- **Rationale**: C5 requires NtrRevelation to write CFLAG:WC_管理人は誰=0 (NTR.ERB:918) and CFLAG:WC_前任管理人 (NTR.ERB:919). AC#34 verifies the test file references these constants (gte 2). AC#58 covers TALENT implementation-side. This AC completes the pairwise pattern for CFLAG: AC#34 (test-side) + AC#60 (impl-side). Dual-language pattern follows AC#34 convention. Requires gte 2 to ensure both CFLAG writes are implemented, paralleling AC#34's test-side gte 2. (C5)

**AC#61: NtrRevelation CFLAG writes target MASTER character (not target/attacker)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetCharacterFlag.*master|SetCharacterFlag.*Master")` count occurrences
- **Expected**: Count >= 2 (both CFLAG:MASTER writes: WC_管理人は誰 and WC_前任管理人 target master, not NtrRevelation's target parameter)
- **Rationale**: C5 specifies CFLAG:MASTER:WC_管理人は誰 and CFLAG:MASTER:WC_前任管理人 (NTR.ERB:918-919). AC#56 verifies GetMaster for withPETTING SETBIT, but NtrRevelation's CFLAG writes also target MASTER. AC#56 could be satisfied by withPETTING alone. This AC ensures NtrRevelation's two SetCharacterFlag calls pass `master` (from GetMaster) rather than `target` parameter. (C5)

**AC#62: IWcCounterMessageItem declares critical ERB-mapped methods by name**
- **Test**: `Grep(Era.Core/Counter/IWcCounterMessageItem.cs, pattern="BottomClothOff\|BottomClothOut\|SetItems\|BottomItemShow\|ClothOut\|VibratorIn")` count occurrences
- **Expected**: Count >= 6 (each of the 6 critical cross-called ERB-mapped method names present in the interface)
- **Rationale**: AC#2 verifies method count (gte 17) but not specific names. These 6 methods are the most cross-called by F806/F807 (C3). Complements AC#2 count with name verification. (C3)

**AC#63: IWcCounterMessageNtr declares critical ERB-mapped methods by name**
- **Test**: `Grep(Era.Core/Counter/IWcCounterMessageNtr.cs, pattern="RotorOut\|OshikkoLooking\|WithFirstSex\|WithPetting\|NtrRevelation\|NtrRevelationAttack")` count occurrences
- **Expected**: Count >= 6 (all 6 NTR public method names present in the interface)
- **Rationale**: AC#4 verifies method count (gte 6) but not specific names. All 6 NTR methods are cross-called by F806/F807 (C4, C11). (C4, C11)

**AC#64: NtrReversalSource is called from NtrRevelation**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="NtrReversalSource\(")` count occurrences
- **Expected**: Count >= 4 (1 declaration + 2 NtrRevelation calls at NTR.ERB:903,925 + 1 NtrRevelationAttack call at NTR.ERB:952)
- **Rationale**: Previous gte 3 was insufficient -- declaration matches the pattern, so gte 4 requires all three call sites. (C12)

**AC#65: WcCounterMessageItemTests verifies ItemWithOtherItem triggers ReportSetItems**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs, pattern="ItemWithOtherItem\|ReportSetItems")` count occurrences
- **Expected**: Count >= 2 (both terms appear in the test file, confirming delegation behavior test coverage)
- **Rationale**: AC#30 could be satisfied without actual delegation. This test-side AC ensures the test file verifies the delegation behavior. (C10)

**AC#66: WcCounterMessageNtrTests verifies WC_管理人は誰 is reset to 0**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="管理人は誰.*0\|Kanrinin.*0\|SetCharacterFlag.*管理人.*0")` presence check
- **Expected**: Pattern matches
- **Rationale**: C5 requires NtrRevelation to write CFLAG:MASTER:WC_管理人は誰=0. AC#34 and AC#60 don't verify the value 0. (C5)

**AC#67: NtrAgreeSource is called from NtrRevelation**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="NtrAgreeSource\(")` presence check
- **Expected**: Pattern matches
- **Rationale**: C12 requires NtrAgreeSource to be called from NtrRevelation. Split from original AC#78 to prevent one helper's calls from satisfying both. (C12)

**AC#68: WcCounterMessageNtrTests verifies NtrRevelationAttack state mutations**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="NtrRevelationAttack\|RevelationAttack")` count occurrences
- **Expected**: Count >= 2
- **Rationale**: NTRrevelation_ATTACK contains 11 state mutations but had no dedicated behavioral test AC. AC#28 only verifies NtrResetVisitorAction delegation. (C8, C11)

**AC#69: WcCounterMessageNtr.cs implements NtrRevelationAttack visitor FLAG resets**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetFlag.*訪問者\|SetFlag.*Visitor")` count occurrences
- **Expected**: Count >= 4 (NtrRevelationAttack has 6 visitor FLAG writes at NTR.ERB:942-949)
- **Rationale**: NtrRevelationAttack had zero impl-side ACs for visitor FLAG mutations. (C8, C11)

**AC#70: WcCounterMessageNtr.cs writes literal 0 for WC_管理人は誰 CFLAG reset**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetCharacterFlag.*[Mm]aster.*[Kk]anrinin.*0\|SetCharacterFlag.*[Mm]aster.*管理人.*0")` presence check
- **Expected**: Pattern matches
- **Rationale**: AC#60 verifies SetCharacterFlag targets MASTER with Kanrinin, but not the literal value 0. (C5)

**AC#71: WcCounterMessageNtr.cs implements NtrRevelationAttack attacker CFLAG writes**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetCharacterFlag.*attacker")` count occurrences
- **Expected**: Count >= 2 (NtrRevelationAttack has 3 CFLAG writes targeting attacker at NTR.ERB:947-955)
- **Rationale**: Split from AC#83 to independently verify attacker-targeted CFLAG mutations. (C8, C11)

**AC#72: NtrRevelationAttack calls NtrReversalSource with attacker argument**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="NtrReversalSource\(attacker")` presence check
- **Expected**: Pattern matches
- **Rationale**: NtrReversalSource is called from both NtrRevelation (with target) and NtrRevelationAttack (with attacker). AC#78 only checks call count, not argument. Follows AC#69 pattern. (C12, C8)

**AC#73: WcCounterMessageNtrTests verifies WC_前任管理人 assignment**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="前任管理人\|ZenninKanrinin\|PreviousManager")` presence check
- **Expected**: Pattern matches
- **Rationale**: C5 requires two distinct CFLAG writes: WC_管理人は誰=0 (AC#80) and WC_前任管理人=TARGET. AC#47 pattern could be satisfied without 前任管理人. (C5)

**AC#74: WcCounterMessageNtr.cs sets WC_前任管理人 to target-derived value**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetCharacterFlag.*[Mm]aster.*[Zz]ennin.*target\|SetCharacterFlag.*[Mm]aster.*前任.*target")` presence check
- **Expected**: Pattern matches
- **Rationale**: Impl-side value verification for WC_前任管理人=TARGET. Follows AC#84 pattern. (C5)

**AC#75: NtrRevelation calls NtrAgreeSource with target argument**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="NtrAgreeSource\(target")` presence check
- **Expected**: Pattern matches
- **Rationale**: Parallel to AC#86. AC#81 only verifies call existence, not argument. (C12)

**AC#76: NtrRevelation calls NtrReversalSource with target argument**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="NtrReversalSource\(target")` presence check
- **Expected**: Pattern matches
- **Rationale**: Symmetric to AC#86 and AC#89. AC#78 only verifies call count, not argument identity. (C12)

**AC#77: NtrRevelation writes TALENT:管理人=0 with literal 0 (impl-side C5)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="SetTalent.*[Tt]arget.*0\|SetTalent.*0.*[Tt]arget")` presence check
- **Expected**: Pattern matches
- **Rationale**: Pairwise with AC#92. AC#72 verifies SetTalent but not value=0. Following AC#84 pattern for value verification. (C5)

**AC#78: WcCounterMessageNtrTests verifies TALENT:管理人 reset to 0 (test-side C5)**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="SetTalent.*0\|TalentIndex.*0")` presence check
- **Expected**: Pattern matches
- **Rationale**: Pairwise with AC#91. Following AC#80 pattern for test-side value verification. (C5)

**AC#79: WcCounterMessageNtrTests verifies NtrRevelationAttack visitor FLAG resets (test-side)**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="SetFlag.*訪問者\|SetFlag.*Visitor\|Visitor.*Flag")` count occurrences
- **Expected**: Count >= 2
- **Rationale**: Pairwise with AC#83. Parallel to CFLAG (AC#75/AC#47), TALENT (AC#91/AC#92), SOURCE (AC#69/AC#48). (C5)

**AC#80: WcCounterMessageItemTests covers non-branching ITEM functions (method names in test)**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageItemTests.cs, pattern="NippleCapSet\|CliCapSet\|CliCapOut\|RotorPantsOut\|RotorIn\|BottomClothOut\|BottomClothOff\|ClothOut\|BottomItemShow\|SetItems\|ReportSetItems")` count occurrences
- **Expected**: Count >= 5
- **Rationale**: AC#52 covers 6 branching functions. The remaining 11 non-branching functions have no dedicated test-name AC. (C3, C13)

**AC#81: WcCounterMessageNtrTests verifies NtrRevelationAttack attacker CFLAG assertions (test-side)**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs, pattern="attacker.*CFLAG\|attacker.*Kanrinin\|SetCharacterFlag.*attacker")` count occurrences
- **Expected**: Count >= 2
- **Rationale**: Pairwise with AC#85. Closes NtrRevelationAttack asymmetry between NtrRevelation (full pairwise) and NtrRevelationAttack (impl-only). (C5, C8)

**AC#82: WcCounterMessageNtr KOJO dispatch includes action type parameter**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageNtr.cs, pattern="_kojoMessage\..*actionType\|_kojoMessage\..*[Rr]evelation")` presence check
- **Expected**: Pattern matches
- **Rationale**: AC#63 verifies target forwarding but not actionType. ERB TRYCALLFORM uses `_1_` for NTR action type (NTR.ERB:898). (C9)

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Create IWcCounterMessageItem interface exposing all 17 ITEM functions as methods | AC#1, AC#2, AC#8, AC#10, AC#13, AC#20, AC#28, AC#31, AC#39, AC#40, AC#41, AC#45, AC#47, AC#51, AC#57, AC#62, AC#65, AC#80 |
| 2 | Create IWcCounterMessageNtr interface exposing NTR observation functions | AC#3, AC#4, AC#9, AC#11, AC#14, AC#18, AC#19, AC#21, AC#22, AC#23, AC#24, AC#29, AC#32, AC#34, AC#35, AC#36, AC#42, AC#43, AC#44, AC#48, AC#49, AC#50, AC#52, AC#53, AC#54, AC#55, AC#56, AC#58, AC#59, AC#60, AC#61, AC#63, AC#64, AC#66, AC#67, AC#68, AC#69, AC#70, AC#71, AC#72, AC#73, AC#74, AC#75, AC#76, AC#77, AC#78, AC#79, AC#81, AC#82 |
| 3 | Implement NtrRevelationHandler replacing null sentinel in DI | AC#5, AC#6, AC#7, AC#12, AC#17, AC#25, AC#30, AC#33, AC#46 |
| 4 | Register all new handlers in ServiceCollectionExtensions.cs | AC#7, AC#26, AC#27 |
| 5 | Reclassify F808 as Predecessor of F806 and F807 | AC#15, AC#16, AC#37, AC#38 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Three new sealed implementation classes are created in `Era.Core/Counter/`, each behind a new interface, following the established F805 pattern (WcCounterMessage, WcCounterSourceHandler). All classes use constructor injection and are registered in `ServiceCollectionExtensions.cs` as Singleton.

**Class decomposition:**

1. `IWcCounterMessageItem` / `WcCounterMessageItem` — migrates all 17 TOILET_COUNTER_MESSAGE_ITEM.ERB functions. These are utility functions called by the WC counter source handler and the F806/F807 message handlers. The class is self-contained and does not need to call NTR functions.

2. `IWcCounterMessageNtr` / `WcCounterMessageNtr` — migrates the 6 public functions from TOILET_COUNTER_MESSAGE_NTR.ERB (ROTOR_OUT, OSHIKKO_looking, withFirstSex, withPETTING, NTRrevelation, NTRrevelation_ATTACK) plus 2 private helper methods (NtrReversalSource, NtrAgreeSource). The internal call from ITEM_WITHotherITEM to ReportSetItems is satisfied within WcCounterMessageItem itself (same class, no cross-class call needed — C10). NTRrevelation_ATTACK is also exposed on this interface to satisfy C11 (WC_SexHara_MESSAGE external caller).

3. `NtrRevelationHandler` (implements `INtrRevelationHandler`) — a pure bridge/adapter that satisfies the F805 `INtrRevelationHandler.Execute(CharacterId, int)` contract by delegating to `IWcCounterMessageNtr.NtrRevelation()`. This replaces the `sp => null!` sentinel at ServiceCollectionExtensions.cs:194. NtrRevelationHandler contains no business logic; all NTRrevelation logic (CFLAG/TALENT state mutations, KOJO dispatch, NTR_RESET_VISITOR_ACTION delegation) lives canonically in WcCounterMessageNtr.

**Separation rationale:** INtrRevelationHandler.Execute has a fixed signature (`void Execute(CharacterId, int)`) established by F805. NtrRevelationHandler is a thin bridge that delegates to IWcCounterMessageNtr.NtrRevelation(), ensuring a single canonical implementation of NTRrevelation logic in WcCounterMessageNtr. The bridge pattern avoids multi-interface implementation complexity: WcCounterMessage (F805) injects INtrRevelationHandler? for TRYCALL no-op semantics; F807 directly injects IWcCounterMessageNtr.

**INtrUtilityService extension:** Two new default-interface methods are added to INtrUtilityService. `void NtrResetVisitorAction(int characterIndex) { }` (no-op stub) satisfies C8 without breaking existing implementors. `string GetNtrName(int characterIndex) => ""` (empty string stub) satisfies C15 for NTR_NAME(0) at NTR.ERB:850 without breaking existing implementors. WcCounterMessageNtr calls both via the injected INtrUtilityService.

**Dependency wiring for withPETTING (NTR.ERB:209):**
- 22 SOURCE:ARG:X += 100 writes → IVariableStore.SetSource(CharacterId, SourceIndex, value)
- 11 SETBIT calls on WC_箇所埋まり → BitfieldUtility.SetBit / GetBit on CFLAG value read from IVariableStore
- PRINTDATA random selection → IRandomProvider.NextFromArray or Next(count) for data-list random block selection

**Namespace and file layout:**
```
Era.Core/Counter/
  IWcCounterMessageItem.cs      (new interface)
  WcCounterMessageItem.cs       (new implementation)
  IWcCounterMessageNtr.cs       (new interface)
  WcCounterMessageNtr.cs        (new implementation)
  NtrRevelationHandler.cs       (new implementation of INtrRevelationHandler)
  INtrUtilityService.cs         (modified: add NtrResetVisitorAction default method)
  ServiceCollectionExtensions.cs (modified: replace null sentinel, add 2 new registrations)

Era.Core.Tests/Counter/
  WcCounterMessageItemTests.cs  (new)
  WcCounterMessageNtrTests.cs   (new)
  NtrRevelationHandlerTests.cs  (new)
```

**Dependency reclassification:** feature-806.md and feature-807.md Dependency tables are updated from `Related | F808` to `Predecessor | F808`.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core/Counter/IWcCounterMessageItem.cs` file |
| 2 | Declare exactly 17 method signatures in IWcCounterMessageItem: NippleCapSet, CliCapSet, NippleCapOut, CliCapOut, ItemWithOtherItem, RotorPantsOut, VibratorIn, AnalVibIn, RotorIn, InsertItemIn, InsertItemOut, BottomClothOut, BottomClothOff, ClothOut, BottomItemShow, SetItems, ReportSetItems |
| 3 | Create `Era.Core/Counter/IWcCounterMessageNtr.cs` file |
| 4 | Declare exactly 6 method signatures in IWcCounterMessageNtr: RotorOut, OshikkoLooking, WithFirstSex, WithPetting, NtrRevelation, NtrRevelationAttack |
| 5 | Create `Era.Core/Counter/NtrRevelationHandler.cs` with `public sealed class NtrRevelationHandler : INtrRevelationHandler` |
| 6 | Replace `services.AddSingleton<INtrRevelationHandler>(sp => null!)` in ServiceCollectionExtensions.cs |
| 7 | Replace null sentinel and add all 3 AddSingleton registrations (INtrRevelationHandler→NtrRevelationHandler, IWcCounterMessageItem→WcCounterMessageItem, IWcCounterMessageNtr→WcCounterMessageNtr) in ServiceCollectionExtensions.cs |
| 8 | Create `Era.Core/Counter/WcCounterMessageItem.cs` with `public sealed class WcCounterMessageItem : IWcCounterMessageItem` |
| 9 | Create `Era.Core/Counter/WcCounterMessageNtr.cs` with `public sealed class WcCounterMessageNtr : IWcCounterMessageNtr` |
| 10 | Create `Era.Core.Tests/Counter/WcCounterMessageItemTests.cs` with at minimum one test class and one [Fact] |
| 11 | Create `Era.Core.Tests/Counter/WcCounterMessageNtrTests.cs` with at minimum one test class and one [Fact] |
| 12 | Create `Era.Core.Tests/Counter/NtrRevelationHandlerTests.cs` with at minimum one test class and one [Fact] |
| 13 | Declare all 5 interface fields in WcCounterMessageItem constructor (IVariableStore, IEngineVariables, ITEquipVariables, ITextFormatting, IConsoleOutput); inject in constructor |
| 14 | Declare all 9 interface fields in WcCounterMessageNtr constructor (IVariableStore, IEngineVariables, ITEquipVariables, ITextFormatting, IConsoleOutput, IKojoMessageService, IRandomProvider, ICounterUtilities, INtrUtilityService); inject in constructor |
| 15 | Edit feature-806.md: change `Related \| F808` row to `Predecessor \| F808` in Dependencies table |
| 16 | Edit feature-807.md: change `Related \| F808` row to `Predecessor \| F808` in Dependencies table |
| 17 | Declare `private readonly IWcCounterMessageNtr _ntrMessage;` field in NtrRevelationHandler; inject in constructor |
| 18 | Add `void NtrResetVisitorAction(int characterIndex) { }` default method to INtrUtilityService interface |
| 19 | In WcCounterMessageNtr.NtrRevelationAttack(), call `_ntrUtility.NtrResetVisitorAction(attacker.Value)` (CharacterId→int unwrap required) |
| 20 | Inside WcCounterMessageItem.ItemWithOtherItem(), call `ReportSetItems(...)` -- satisfied by implementing the internal call (ITEM.ERB:266 pattern) |
| 21 | IWcCounterMessageNtr method signature `void NtrRevelationAttack(CharacterId attacker)` must appear in the interface file (attacker only, no actionType per NTR.ERB:933-936) |
| 22 | Implement NtrReversalSource as `private void NtrReversalSource(...)` in WcCounterMessageNtr |
| 23 | Implement NtrAgreeSource as `private void NtrAgreeSource(...)` in WcCounterMessageNtr |
| 24 | In withPETTING implementation: read CFLAG value for WC_箇所埋まり via IVariableStore, call `BitfieldUtility.SetBit(field, bit)` for each of the 11 SETBIT ERB operations |
| 25 | In NtrRevelationHandler.Execute(), call `_ntrMessage.NtrRevelation(target, actionType)` |
| 26 | All new files compile under TreatWarningsAsErrors=true; no warnings in Era.Core/ after changes |
| 27 | WcCounterMessageItemTests, WcCounterMessageNtrTests, NtrRevelationHandlerTests all pass; no regression in existing tests |
| 28 | No `TODO`, `FIXME`, or `HACK` strings present in WcCounterMessageItem.cs |
| 29 | No `TODO`, `FIXME`, or `HACK` strings present in WcCounterMessageNtr.cs |
| 30 | No `TODO`, `FIXME`, or `HACK` strings present in NtrRevelationHandler.cs |
| 31 | `Era.Core/Counter/WcCounterMessageItem.cs` exists as a file |
| 32 | `Era.Core/Counter/WcCounterMessageNtr.cs` exists as a file |
| 33 | `Era.Core/Counter/NtrRevelationHandler.cs` exists as a file |
| 34 | Test methods in WcCounterMessageNtrTests referencing both 管理人 CFLAG constants (WC_管理人は誰, WC_前任管理人) — gte 2 ensures both are covered (disambiguated from withPETTING SETBIT) |
| 35 | Test methods in WcCounterMessageNtrTests that verify SetSource is called with correct SourceIndex values after WithPetting execution (multi-path, gte 3 occurrences) |
| 36 | Test methods in WcCounterMessageNtrTests that verify BitfieldUtility usage across at least 3 distinct bit position scenarios (C7 11 SETBIT calls) |
| 37 | After editing feature-806.md Dependencies table, verify no `Related \| F808` row remains |
| 38 | After editing feature-807.md Dependencies table, verify no `Related \| F808` row remains |
| 39 | Test methods in WcCounterMessageItemTests that exercise representative WC_応答分類 SELECTCASE branches (response category paths) |
| 40 | Test methods in WcCounterMessageItemTests that verify EQUIP state checks via mocked ITEquipVariables |
| 41 | Test methods in WcCounterMessageItemTests that verify CFLAG/FLAG variable access via mocked IVariableStore |
| 42 | Add `string GetNtrName(int characterIndex) => "";` default method to INtrUtilityService interface |
| 43 | In WcCounterMessageNtr.NtrRevelation(), call `_ntrUtility.GetNtrName(0)` for NTR_NAME(0) — literal 0 is the NTR name index per ERB source |
| 44 | Test methods in WcCounterMessageNtrTests must call Assert.* (xUnit) or mock.Verify() (Moq) for behavioral verification of CFLAG/SOURCE/SETBIT outcomes |
| 45 | Test methods in WcCounterMessageItemTests must call Assert.* or mock.Verify() for behavioral verification of EQUIP/CFLAG/response-category outcomes |
| 46 | Test methods in NtrRevelationHandlerTests must call Assert.* or mock.Verify() for behavioral verification of delegation to IWcCounterMessageNtr |
| 47 | WcCounterMessageItem.cs must contain gte 10 occurrences of injected service usage (_variables, _console, _engine, _tEquipVariables, _textFormatting) to verify non-stub logic migration |
| 48 | WcCounterMessageNtr.cs must contain gte 10 occurrences of injected service usage (all 9 injected services) to verify non-stub logic migration |
| 49 | WcCounterMessageNtr.NtrRevelation must call _kojoMessage with target.Value for KOJO dispatch (C9, NTR.ERB:898, CharacterId→int unwrap) |
| 50 | WcCounterMessageNtr.NtrRevelation must call _counterUtilities.GetDateTime() for CFLAG timestamp (C14) |
| 51 | WcCounterMessageItem.cs must NOT contain NotImplementedException (established stub pattern) |
| 52 | WcCounterMessageNtr.cs must NOT contain NotImplementedException (established stub pattern) |
| 53 | Test methods in WcCounterMessageNtrTests that verify SetTalent (TALENT:管理人=0) is called after NtrRevelation execution (NTR.ERB:920) |
| 54 | Test methods in WcCounterMessageNtrTests that exercise NtrReversalSource/NtrAgreeSource favor/surrender calculation paths (C12) |
| 55 | In WithPetting implementation, SetSource calls use the offender parameter (not _engine.GetTarget()) as target CharacterId (C6, NTR.ERB:210) |
| 56 | In WithPetting implementation, SETBIT/GETBIT calls target GetMaster character for CFLAG:MASTER:WC_箇所埋まり (C7, NTR.ERB:374) |
| 57 | WcCounterMessageItem.cs must reference WC_応答分類 (or C# equivalent ResponseCategory/WcResponseCategory) for C13 response branching |
| 58 | WcCounterMessageNtr.cs must call SetTalent for TALENT:TARGET:管理人=0 mutation (C5 NTR.ERB:920) |
| 59 | WcCounterMessageNtr.cs must NOT call SetSource with GetTarget (wrong character — C6 ARG=offender per NTR.ERB:210) |
| 60 | WcCounterMessageNtr.cs must call SetCharacterFlag gte 2 times for C5 NtrRevelation CFLAG writes (both WC_管理人は誰 and WC_前任管理人 constants) |
| 61 | WcCounterMessageNtr.cs NtrRevelation SetCharacterFlag calls must use master variable (CFLAG:MASTER per C5, NTR.ERB:918-919) |
| 62 | IWcCounterMessageItem interface must declare methods: BottomClothOff, BottomClothOut, SetItems, BottomItemShow, ClothOut, VibratorIn (6 critical cross-called names) |
| 63 | IWcCounterMessageNtr interface must declare methods: RotorOut, OshikkoLooking, WithFirstSex, WithPetting, NtrRevelation, NtrRevelationAttack (all 6 NTR public names) |
| 64 | WcCounterMessageNtr.cs must call NtrReversalSource gte 4 times (1 declaration + 3 call sites from NtrRevelation x2 and NtrRevelationAttack x1) |
| 65 | WcCounterMessageItemTests.cs must reference both ItemWithOtherItem and ReportSetItems (test-side delegation verification) |
| 66 | WcCounterMessageNtrTests.cs must verify WC_管理人は誰=0 reset (CFLAG value 0 check) |
| 67 | WcCounterMessageNtr.cs must call NtrAgreeSource (at least one call site from NtrRevelation) |
| 68 | WcCounterMessageNtrTests.cs must reference NtrRevelationAttack or RevelationAttack gte 2 times |
| 69 | WcCounterMessageNtr.cs must call SetFlag for 訪問者 or Visitor gte 4 times (visitor FLAG resets in NtrRevelationAttack) |
| 70 | WcCounterMessageNtr.cs must call SetCharacterFlag targeting master with Kanrinin value 0 (WC_管理人は誰=0) |
| 71 | WcCounterMessageNtr.cs must call SetCharacterFlag targeting attacker gte 2 times (NtrRevelationAttack CFLAG writes) |
| 72 | WcCounterMessageNtr.cs NtrRevelationAttack must call NtrReversalSource with attacker argument |
| 73 | WcCounterMessageNtrTests.cs must reference WC_前任管理人 or ZenninKanrinin (test-side C5 verification) |
| 74 | WcCounterMessageNtr.cs must call SetCharacterFlag targeting master with 前任 or zenin and target argument (WC_前任管理人=target) |
| 75 | WcCounterMessageNtr.cs NtrRevelation must call NtrAgreeSource(target) with target argument |
| 76 | WcCounterMessageNtr.cs NtrRevelation must call NtrReversalSource(target) with target argument |
| 77 | WcCounterMessageNtr.cs must call SetTalent targeting target with value 0 (TALENT:管理人=0) |
| 78 | WcCounterMessageNtrTests.cs must verify SetTalent with value 0 or TalentIndex.*0 (test-side TALENT:管理人=0) |
| 79 | WcCounterMessageNtrTests.cs must reference SetFlag for 訪問者 or Visitor gte 2 times (test-side visitor FLAG verification) |
| 80 | WcCounterMessageItemTests.cs must reference gte 5 ITEM method names (NippleCapSet, CliCapSet, etc.) for non-branching function coverage |
| 81 | WcCounterMessageNtrTests.cs must reference attacker.*CFLAG or SetCharacterFlag.*attacker gte 2 times (test-side NtrRevelationAttack CFLAG assertions) |
| 82 | WcCounterMessageNtr.cs _kojoMessage call must include actionType or Revelation parameter (KOJO dispatch action type verification) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| NtrRevelationHandler vs merging into WcCounterMessageNtr | A: Separate NtrRevelationHandler as bridge delegating to WcCounterMessageNtr. B: Make WcCounterMessageNtr also implement INtrRevelationHandler | A: Separate bridge class | INtrRevelationHandler.Execute has a fixed F805 contract. A dedicated bridge class delegates to IWcCounterMessageNtr.NtrRevelation(), keeping a single canonical implementation in WcCounterMessageNtr while satisfying F805's TRYCALL no-op contract. Avoids multi-interface implementation complexity. |
| NtrResetVisitorAction stub | A: Default interface method (no-op). B: Separate NullNtrUtilityService. C: Throw NotImplementedException | A: Default interface method | Consistent with existing INtrUtilityService pattern (GetNWithVisitor already has default `=> 0`). Does not require changes to existing implementors. Clearly documents stub nature in the interface. |
| NTRrevelation_ATTACK interface exposure | A: Expose via IWcCounterMessageNtr. B: Create separate IWcSexHaraService overload | A: IWcCounterMessageNtr | WC_SexHara_MESSAGE only calls NTRrevelation_ATTACK; it makes sense to inject IWcCounterMessageNtr to access it. No separate interface needed for a single cross-file call. |
| PRINTDATA random selection | A: IRandomProvider.Next(count) to select DATALIST block index. B: IConsoleOutput.PrintData with pre-selected lines | A: IRandomProvider.Next(count) then IConsoleOutput.PrintData | Matches existing pattern in WcCounterMessage where caller selects randomly and passes string[] to PrintData. Keeps IConsoleOutput stateless. |
| DI lifetime for new services | A: Singleton. B: Transient | A: Singleton | All three classes are stateless (no mutable fields); Singleton avoids per-request allocation. Consistent with WcCounterMessage, WcCounterSourceHandler registration pattern. |
| withPETTING SETBIT approach | A: Read-modify-write via IVariableStore.GetCharacterFlag + BitfieldUtility.SetBit + SetCharacterFlag. B: Extend IVariableStore with SetBit helper | A: Read-modify-write using existing API | IVariableStore already exposes GetCharacterFlag/SetCharacterFlag. BitfieldUtility.SetBit is a static helper. Adding a SetBit shortcut to IVariableStore is unnecessary abstraction. |
| WcCounterMessageNtr 9-dependency constructor | A: Keep as single class with 9 deps. B: Split into WcCounterMessageNtrCore + WcCounterMessageNtrContext | A: Single class | The 9 dependencies mirror the ERB file's implicit global state access (CFLAG, TALENT, SOURCE, EQUIP, PRINT, KOJO, RANDOM, DATETIME, NTR_UTIL). All 6+2 functions operate on the same NTR revelation/observation context and share most dependencies. Splitting would create artificial seams between tightly cohesive NTR operations (e.g., WithPetting and NtrRevelation both need IVariableStore, IConsoleOutput, IEngineVariables, ITEquipVariables). The dep count reflects ERB's inherent complexity surface, not an SRP violation. |

### Interfaces / Data Structures

**IWcCounterMessageItem** (new, `Era.Core/Counter/IWcCounterMessageItem.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Counter;

/// <summary>
/// WC counter ITEM message functions.
/// Migrates TOILET_COUNTER_MESSAGE_ITEM.ERB (2,442 lines, 17 functions).
/// Called by F806 (SEX) and F807 (TEASE) counter message handlers via TRYCALL semantics.
/// Feature 808 - WC Counter Message ITEM + NTR
/// </summary>
public interface IWcCounterMessageItem
{
    void NippleCapSet(CharacterId offender);
    void CliCapSet(CharacterId offender);
    void NippleCapOut(CharacterId offender);
    void CliCapOut(CharacterId offender);
    void ItemWithOtherItem(CharacterId offender);
    void RotorPantsOut(CharacterId offender);
    void VibratorIn(CharacterId offender);
    void AnalVibIn(CharacterId offender);
    void RotorIn(CharacterId offender, int insertionLocation);
    void InsertItemIn(CharacterId offender);
    void InsertItemOut(CharacterId offender);
    void BottomClothOut(CharacterId offender, int targetArea);
    void BottomClothOff(CharacterId offender);
    void ClothOut(CharacterId offender, int nippleCapStart);
    void BottomItemShow(CharacterId offender);
    void SetItems(CharacterId offender);
    void ReportSetItems(CharacterId offender);
}
```

**IWcCounterMessageNtr** (new, `Era.Core/Counter/IWcCounterMessageNtr.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Counter;

/// <summary>
/// WC counter NTR message functions (public surface).
/// Migrates TOILET_COUNTER_MESSAGE_NTR.ERB (1,037 lines, 6 public functions).
/// NTRreversal_SOURCE and NTRagree_SOURCE are private helpers in implementation.
/// Feature 808 - WC Counter Message ITEM + NTR
/// </summary>
public interface IWcCounterMessageNtr
{
    void RotorOut(CharacterId offender);
    void OshikkoLooking(CharacterId offender);
    void WithFirstSex(CharacterId offender, int painLevel);
    void WithPetting(CharacterId offender);
    void NtrRevelation(CharacterId target, int actionType);
    void NtrRevelationAttack(CharacterId attacker);
}
```

**WcCounterMessageItem constructor** (new, `Era.Core/Counter/WcCounterMessageItem.cs`):
```csharp
public sealed class WcCounterMessageItem : IWcCounterMessageItem
{
    private readonly IVariableStore _variables;
    private readonly IEngineVariables _engine;
    private readonly ITEquipVariables _tEquipVariables;
    private readonly ITextFormatting _textFormatting;
    private readonly IConsoleOutput _console;

    public WcCounterMessageItem(
        IVariableStore variables,
        IEngineVariables engine,
        ITEquipVariables tEquipVariables,
        ITextFormatting textFormatting,
        IConsoleOutput console) { ... }

    // 17 public method implementations
    // ItemWithOtherItem calls ReportSetItems(offender) directly (same class, no cross-class call needed — C10)
}
```

**WcCounterMessageNtr constructor** (new, `Era.Core/Counter/WcCounterMessageNtr.cs`):
```csharp
public sealed class WcCounterMessageNtr : IWcCounterMessageNtr
{
    private readonly IVariableStore _variables;
    private readonly IEngineVariables _engine;
    private readonly ITEquipVariables _tEquipVariables;
    private readonly ITextFormatting _textFormatting;
    private readonly IConsoleOutput _console;
    private readonly IKojoMessageService _kojoMessage;
    private readonly IRandomProvider _randomProvider;
    private readonly ICounterUtilities _counterUtilities;
    private readonly INtrUtilityService _ntrUtility;

    public WcCounterMessageNtr(
        IVariableStore variables,
        IEngineVariables engine,
        ITEquipVariables tEquipVariables,
        ITextFormatting textFormatting,
        IConsoleOutput console,
        IKojoMessageService kojoMessage,
        IRandomProvider randomProvider,
        ICounterUtilities counterUtilities,
        INtrUtilityService ntrUtility) { ... }

    // 6 public methods (NtrRevelation and NtrRevelationAttack contain CFLAG/TALENT mutations, KOJO dispatch, NTR_RESET_VISITOR_ACTION delegation)
    // 2 private helpers:
    private void NtrReversalSource(CharacterId target) { ... }
    private void NtrAgreeSource(CharacterId target) { ... }
}
```

**NtrRevelationHandler constructor** (new, `Era.Core/Counter/NtrRevelationHandler.cs`):
```csharp
public sealed class NtrRevelationHandler : INtrRevelationHandler
{
    private readonly IWcCounterMessageNtr _ntrMessage;

    public NtrRevelationHandler(IWcCounterMessageNtr ntrMessage)
    {
        _ntrMessage = ntrMessage;
    }

    public void Execute(CharacterId target, int actionType)
    {
        // Pure bridge: delegate to canonical implementation in WcCounterMessageNtr
        _ntrMessage.NtrRevelation(target, actionType);
    }
}
```

**INtrUtilityService extension** (modified, `Era.Core/Counter/INtrUtilityService.cs`):
```csharp
// Add to existing interface:
/// <summary>
/// NTR_RESET_VISITOR_ACTION (NTR_UTIL.ERB:1110) stub.
/// Resets visitor state after NTR revelation attack.
/// Returns no-op until NTR_UTIL.ERB migration (future phase).
/// Feature 808 - WC Counter Message ITEM + NTR
/// </summary>
void NtrResetVisitorAction(int characterIndex) { }

/// <summary>
/// NTR_NAME(0) stub (NTR.ERB:850).
/// Returns NTR character name. Returns empty string until NTR_NAME migration.
/// Feature 808 - WC Counter Message ITEM + NTR
/// </summary>
string GetNtrName(int characterIndex) => "";
```

**ServiceCollectionExtensions.cs changes:**
```csharp
// Replace (line ~194):
// services.AddSingleton<INtrRevelationHandler>(sp => null!);  // REMOVE

// Add in Counter System (Phase 21) - Feature 808 block:
services.AddSingleton<IWcCounterMessageItem, WcCounterMessageItem>();
services.AddSingleton<IWcCounterMessageNtr, WcCounterMessageNtr>();
services.AddSingleton<INtrRevelationHandler, NtrRevelationHandler>();
```

**withPETTING SETBIT pattern** (example for 11 SETBIT calls):
```csharp
// ERB: SETBIT CFLAG:MASTER:WC_箇所埋まり, 0  (MASTER character, confirmed NTR.ERB:374)
var master = new CharacterId(_engine.GetMaster());  // CFLAG:MASTER = toilet manager character
int field = _variables.GetCharacterFlag(master, new CharacterFlagIndex(CflagWcKashoUmari)).Match(v => v, _ => 0);
field = BitfieldUtility.SetBit(field, 0);  // bit 0 for first body part
_variables.SetCharacterFlag(master, new CharacterFlagIndex(CflagWcKashoUmari), field);
```

**SOURCE write pattern** (example for 22 SOURCE:ARG:X += 100):
```csharp
// ERB: SOURCE:ARG:0 += 100 — ARG = offender (同席者のキャラ番号, NTR.ERB:210)
// offender parameter from WithPetting(CharacterId offender) — NOT _engine.GetTarget()
var src = _variables.GetSource(offender, SourceIndex.FromIndex(0)).Match(v => v, _ => 0);
_variables.SetSource(offender, SourceIndex.FromIndex(0), src + 100);
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#20 description says "ITEM_WITHotherITEM delegates to ReportSetItems" but the Grep matcher checks `WcCounterMessageItem.cs` for `ReportSetItems\(`. Since both ItemWithOtherItem and ReportSetItems are on the same class, `ItemWithOtherItem` calls `ReportSetItems(offender)` directly (no separate helper needed). The AC as written is satisfiable: the file will contain `ReportSetItems(` as the call site. No upstream change needed — note for implementor only. | AC#20 detail | No change; Grep will match the call site within ItemWithOtherItem body |
| IWcCounterMessageNtr AC#4 expects `>=6` method count using pattern `^\s+(void\|int\|bool).*\(`. NtrRevelation and NtrRevelationAttack both return void and take CharacterId+int parameters. NtrRevelationAttack is also listed as a separate requirement in AC#21. Verify the Grep pattern matches all 6 signatures (void return, correctly indented). Interface bodies return void for all 6 NTR public functions — no int/bool returns — so the pattern is adequate. | AC#4 / AC#21 | No change needed; all 6 NTR interface methods return void |
| WcCounterMessageItem does not inject ICommonFunctions or IKojoMessageService per AC#13-19. TOILET_COUNTER_MESSAGE_ITEM.ERB contains zero COMMON function calls (verified by Grep), confirming ICommonFunctions is not needed by any of the 17 ITEM functions. IKojoMessageService is similarly not used by ITEM functions (KOJO dispatch is only in NTR functions, covered by WcCounterMessageNtr). No AC gap exists. | Technical Constraints C14 | No change needed; ICommonFunctions confirmed unnecessary by ERB source analysis |
| AC#34-49, AC#39-54 use Grep-based patterns (e.g., `SetCharacterFlag\|CflagWc`) to verify behavioral test content. These patterns are minimum presence gates that confirm test files reference the correct domain concepts. Actual behavioral equivalence verification is delegated to AC#27 (all unit tests pass). Implementors should ensure test methods contain meaningful assertions, not just mock setup lines, as the Grep pattern alone cannot distinguish assertion from setup. AC#44-60 close this gap by adding assertion-presence verification (Moq Verify + xUnit Assert) for all 3 test files. | AC#34-54, AC#44-60 | No code change; AC#44-60 added to enforce assertion presence |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2, 3, 4, 21, 62, 63 | Create IWcCounterMessageItem.cs and IWcCounterMessageNtr.cs interface files with all method signatures (17 ITEM + 6 NTR public) | | [ ] |
| 2 | 8, 13, 20, 28, 31, 47, 51, 57, 65, 80 | Implement WcCounterMessageItem.cs: sealed class with constructor injection (IVariableStore, IEngineVariables, ITEquipVariables, ITextFormatting, IConsoleOutput), 17 public methods migrated from TOILET_COUNTER_MESSAGE_ITEM.ERB, ItemWithOtherItem calling ReportSetItems internally | | [ ] |
| 3 | 9, 14, 19, 22, 23, 24, 29, 32, 43, 48, 49, 50, 52, 55, 56, 58, 59, 60, 61, 64, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 82 | Implement WcCounterMessageNtr.cs: sealed class with constructor injection (IVariableStore, IEngineVariables, ITEquipVariables, ITextFormatting, IConsoleOutput, IKojoMessageService, IRandomProvider, ICounterUtilities, INtrUtilityService), 6 public methods migrated from TOILET_COUNTER_MESSAGE_NTR.ERB including NTRrevelation logic (CFLAG/TALENT mutations, KOJO dispatch, NTR_RESET_VISITOR_ACTION delegation, GetNtrName call for NTR_NAME(0)), 2 private helpers (NtrReversalSource, NtrAgreeSource), BitfieldUtility usage for withPETTING SETBIT operations | | [ ] |
| 4 | 18, 42 | Extend INtrUtilityService with NtrResetVisitorAction default interface method (no-op stub) and GetNtrName default method (empty string stub) | | [ ] |
| 5 | 5, 17, 25, 30, 33 | Implement NtrRevelationHandler.cs: sealed class implementing INtrRevelationHandler as pure bridge, constructor injection (IWcCounterMessageNtr), Execute delegates to IWcCounterMessageNtr.NtrRevelation | | [ ] |
| 6 | 6, 7 | Update ServiceCollectionExtensions.cs: remove null sentinel for INtrRevelationHandler, add AddSingleton registrations for IWcCounterMessageItem, IWcCounterMessageNtr, INtrRevelationHandler | | [ ] |
| 7 | 15, 16, 37, 38 | Update feature-806.md and feature-807.md: change Related F808 rows to Predecessor F808 in their Dependencies tables; verify old Related rows are removed | | [ ] |
| 8 | 10, 11, 12, 27, 34, 35, 36, 39, 40, 41, 44, 45, 46, 53, 54, 66, 68, 73, 78, 79, 81 | Create WcCounterMessageItemTests.cs, WcCounterMessageNtrTests.cs, and NtrRevelationHandlerTests.cs with representative unit tests; WcCounterMessageNtrTests must include tests for CFLAG state mutations (C5), SOURCE increments (C6), and BitfieldUtility operations (C7); verify all tests pass | | [ ] |
| 9 | 26 | Run dotnet build Era.Core/ and confirm zero errors and zero warnings (TreatWarningsAsErrors=true) | | [ ] |

<!-- fc-phase-5-completed -->
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
| 1 | implementer | sonnet | feature-808.md Tasks T1, IWcCounterMessageItem/IWcCounterMessageNtr interface specs from Technical Design | Era.Core/Counter/IWcCounterMessageItem.cs, Era.Core/Counter/IWcCounterMessageNtr.cs |
| 2 | implementer | sonnet | feature-808.md Tasks T4, INtrUtilityService.cs current content | Era.Core/Counter/INtrUtilityService.cs (extended with NtrResetVisitorAction and GetNtrName default methods) |
| 3 | implementer | sonnet | feature-808.md Tasks T2, WcCounterMessageItem constructor spec from Technical Design, TOILET_COUNTER_MESSAGE_ITEM.ERB | Era.Core/Counter/WcCounterMessageItem.cs (17 methods, 5 injected interfaces) |
| 4 | implementer | sonnet | feature-808.md Tasks T3, WcCounterMessageNtr constructor spec from Technical Design, TOILET_COUNTER_MESSAGE_NTR.ERB | Era.Core/Counter/WcCounterMessageNtr.cs (6 public methods, 2 private helpers, BitfieldUtility usage, 9 injected interfaces) |
| 5 | implementer | sonnet | feature-808.md Tasks T5, NtrRevelationHandler bridge spec from Technical Design | Era.Core/Counter/NtrRevelationHandler.cs (pure bridge implementing INtrRevelationHandler, 1 injected interface: IWcCounterMessageNtr) |
| 6 | implementer | sonnet | feature-808.md Tasks T6, ServiceCollectionExtensions.cs current content | Era.Core/DependencyInjection/ServiceCollectionExtensions.cs (null sentinel removed, 3 new AddSingleton registrations) |
| 7 | implementer | sonnet | feature-808.md Tasks T7, feature-806.md and feature-807.md current dependency tables | feature-806.md and feature-807.md (Related F808 → Predecessor F808 in Dependencies tables) |
| 8 | tester | sonnet | feature-808.md Tasks T8, WcCounterMessageItem.cs, WcCounterMessageNtr.cs, NtrRevelationHandler.cs | Era.Core.Tests/Counter/WcCounterMessageItemTests.cs, WcCounterMessageNtrTests.cs, NtrRevelationHandlerTests.cs |
| 9 | implementer | sonnet | feature-808.md Tasks T9 | Build output confirming zero errors and zero warnings |

### Pre-conditions

- F805 [DONE]: WcCounterMessage.cs, INtrRevelationHandler.cs, IVariableStore, IEngineVariables, ITEquipVariables, ITextFormatting, IConsoleOutput, IKojoMessageService, IRandomProvider, ICounterUtilities, INtrUtilityService all exist in Era.Core
- F783 [DONE]: Phase 21 planning complete
- TreatWarningsAsErrors=true enforced in Directory.Build.props

### Execution Order

1. **Phase 1 (T1) before Phase 3, 4, 5**: Interfaces must exist before implementations compile.
2. **Phase 2 (T4) before Phase 4 (T3)**: INtrUtilityService.NtrResetVisitorAction must exist before WcCounterMessageNtr.NtrRevelationAttack references it.
3. **Phase 3 (T2)**: WcCounterMessageItem is independent of T3 and T4.
4. **Phase 4 (T3) after Phase 2 (T4)**: WcCounterMessageNtr depends on INtrUtilityService.NtrResetVisitorAction.
5. **Phase 3–4 (T2, T3) before Phase 5 (T5)**: NtrRevelationHandler requires IWcCounterMessageNtr to exist.
6. **Phase 6 (T6) after Phase 3–5**: DI registration requires all three implementation classes to exist.
7. **Phase 7 (T7) any order**: Documentation-only change to feature-806.md and feature-807.md.
8. **Phase 8 (T8) after Phase 3–5**: Test files require the implementation classes.
9. **Phase 9 (T9) after all**: Build verification is the final gate.

### Build Verification Steps

Run via WSL:
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --blame-hang-timeout 10s'
```

### Success Criteria

- All 82 ACs pass
- Zero build warnings (TreatWarningsAsErrors=true)
- All existing Era.Core.Tests pass (no regressions)
- New test files have at least one [Fact] each

### Error Handling

- If IVariableStore.GetStain/SetStain does not exist: verify IVariableStore API; use GetCharacterFlag/SetCharacterFlag with BitfieldUtility as specified in Technical Design
- If INtrUtilityService already has NtrResetVisitorAction: skip Phase 2 (T4); verify existing signature matches
- If NTR_NAME stub causes compile error: add stub method to INtrUtilityService or use existing ShopSystem.NtrName wrapper

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| NTR_NAME(0) unmigrated | ShopSystem.NtrName throws NotImplementedException; stub returns "" until NTR_UTIL migration | Phase | Phase 25 (NTR_UTIL) | - | [ ] | |
| NTR_RESET_VISITOR_ACTION no-op stub | NTR_UTIL.ERB:1110 visitor state reset silently dropped; no-op until NTR_UTIL migration | Phase | Phase 25 (NTR_UTIL) | - | [ ] | |

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

- [fix] Phase2-Review iter1: Deferred Obligations section | Non-template section removed (template compliance)
- [fix] Phase2-Review iter1: fc-phase markers | Reordered fc-phase completion comments to match template section ownership
- [fix] Phase2-Review iter1: Mandatory Handoffs alignment | Fixed Transferred column centered alignment
- [fix] Phase2-Uncertain iter1: NtrRevelationHandler design | Restructured as pure bridge delegating to IWcCounterMessageNtr (SSOT fix, F802 delegation precedent)
- [fix] Phase2-Review iter1: AC#10-14 behavioral gap | Added AC#34-49 for behavioral test coverage (C5 CFLAG, C6 SOURCE, C7 SETBIT)
- [fix] Phase2-Review iter1: AC#15-23 pattern gap | Added AC#37-51 not_matches for F806/F807 Related removal verification
- [fix] Phase2-Review iter2: C13 coverage gap | Added AC#39 for WC_応答分類 response branching behavioral test
- [fix] Phase2-Review iter2: C14 ICommonFunctions | Removed ICommonFunctions from C14 (not used in ERB), added ICounterUtilities
- [fix] Phase2-Review iter2: ITEM behavioral gap | Added AC#40-54 for WcCounterMessageItem behavioral test coverage (EQUIP, CFLAG)
- [fix] Phase2-Review iter3: C14 ITextFormatting gap | Added AC#55 for WcCounterMessageNtr ITextFormatting injection, updated constructor spec
- [fix] Phase2-Review iter4: C4 constraint | Updated C4 to list all 6 IWcCounterMessageNtr methods (added RotorOut, NtrRevelationAttack)
- [fix] Phase2-Review iter4: Execution Order | Fixed Phase 2/5 ordering note (NtrRevelationHandler no longer uses INtrUtilityService; WcCounterMessageNtr does)
- [fix] Phase2-Review iter4: Upstream Issues | Added behavioral AC pattern note for AC#34-54 (grep minimum gates)
- [resolved-applied] Phase2-Pending iter5: NTR_NAME(0) unmigrated — added AC#42 (INtrUtilityService.GetNtrName declaration), AC#43 (WcCounterMessageNtr call site), C15 constraint, GetNtrName default method to INtrUtilityService extension, Mandatory Handoff row (Phase 26 NTR_UTIL)
- [fix] PostLoop-UserFix post-loop: INtrUtilityService.GetNtrName | Added C15, AC#42-57, GetNtrName default method, Mandatory Handoff (user chose ADOPT)
- [resolved-applied] Phase2-Pending iter5: AC#34-54 behavioral grep patterns — grep on test files cannot distinguish assertions from mock setup (loop from iter4); strengthening requires mock framework choice (xUnit Assert vs NSubstitute Received vs Moq Verify) — resolved by adding AC#44-60 assertion-presence ACs (Moq Verify + xUnit Assert) for all 3 test files
- [fix] PostLoop-UserFix post-loop: AC#34-54 assertion gap | Added AC#44-60 assertion-presence ACs (Moq Verify + xUnit Assert) for all 3 test files (user chose ADOPT)
- [fix] Phase2-Review iter1: AC Details formatting | Inserted missing blank line between AC#39 and AC#40 detail blocks
- [fix] Phase2-Uncertain iter1: Upstream Issues ICommonFunctions | Updated ICommonFunctions note from "monitor" to "confirmed unnecessary" with ERB evidence (0 COMMON calls)
- [fix] Phase2-Review iter2: AC#35 matcher improvement | Strengthened AC#35 from matches (presence) to gte 3 (multi-path coverage) for SOURCE write verification (C6)
- [fix] Phase2-Review iter3: Implementation non-stub verification | Added AC#47-62 verifying WcCounterMessageItem.cs and WcCounterMessageNtr.cs contain gte 10 injected service usage occurrences (prevents NotImplementedException stub bodies)
- [pending] Phase2-Pending iter4: Behavioral equivalence gap — AC#47/62 (gte 10 service usage) prevent empty stubs but do not verify correctness of migrated ERB logic. Grep-based ACs cannot distinguish "correct migration" from "has service calls." The existing gate (AC#27 tests pass + AC#39-54 domain patterns + AC#44-60 assertions) delegates correctness to runtime. Reviewer proposes adding behavioral equivalence ACs per function group. This requires novel design decisions about what patterns to verify.
- [fix] Phase2-Review iter4: AC#20 matcher improvement | Strengthened AC#20 from matches to gte 2 (ensures ReportSetItems call site exists in addition to method declaration, distinguishing declaration-only from internal delegation)
- [fix] Phase2-Review iter5: AC#34 TALENT gap | Expanded AC#34 matcher from SetCharacterFlag|CflagWc to include SetTalent|TalentIndex (C5 TALENT:管理人=0 mutation at NTR.ERB:920)
- [fix] Phase2-Review iter5: Technical Design inconsistency | Removed ReportSetItemsInternal private helper from WcCounterMessageItem constructor spec (contradicted surrounding text stating "no separate helper needed")
- [fix] Phase3-Maintainability iter6: Mandatory Handoff Phase correction | Changed NTR_NAME(0) handoff destination from Phase 26 to Phase 25 (NTR_UTIL is in Phase 25 AI & Visitor Systems, not Phase 26)
- [fix] Phase3-Maintainability iter6: Missing Mandatory Handoff | Added NTR_RESET_VISITOR_ACTION no-op stub handoff to Phase 25 (NTR_UTIL) — deferred behavior leak prevention
- [fix] Phase3-Maintainability iter7: ERB parameter signatures | Fixed IWcCounterMessageItem: RotorIn(+int insertionLocation), BottomClothOut(+int targetArea), ClothOut(+int nippleCapStart) — verified against ITEM.ERB:557,1072,1310
- [fix] Phase3-Maintainability iter7: ERB parameter signatures | Fixed IWcCounterMessageNtr: WithFirstSex(+int painLevel), NtrRevelationAttack(CharacterId attacker) — verified against NTR.ERB:109,933; removed extra actionType param
- [fix] Phase2-Review iter8: AC Coverage target→attacker | Fixed AC Coverage row 28 parameter from target.Value to attacker.Value (stale after NtrRevelationAttack parameter rename)
- [fix] Phase2-Review iter8: AC#19 argument verification | Strengthened AC#19 matcher from `.NtrResetVisitorAction\(` to `.NtrResetVisitorAction\(attacker` (verifies correct argument semantics)
- [fix] Phase2-Review iter9: AC#48 pattern expansion | Extended AC#48 to cover all 9 injected services (_kojoMessage, _counterUtilities, _randomProvider added) — prevents stub-only implementations omitting KOJO/DATETIME
- [fix] Phase2-Review iter9: C9 KOJO call-site AC | Added AC#49 verifying _kojoMessage call site in WcCounterMessageNtr.cs (C9 required call-site, not just injection)
- [fix] Phase2-Review iter9: DATETIME call-site AC | Added AC#50 verifying _counterUtilities.GetDateTime call in WcCounterMessageNtr.cs (follows AC#42+57 pairwise pattern)
- [fix] Phase2-Review iter10: AC#25 parameter forwarding | Strengthened AC#25 matcher from `.NtrRevelation\(` to `.NtrRevelation\(target,\s*actionType\)` (verifies both params forwarded in bridge)
- [fix] Phase4-ACValidation iter10: AC#18 Goal Coverage | Added AC#18 to Goal item 2 in Goal Coverage Verification table (ac_ops.py lint detected orphan)
- [fix] Phase2-Review iter1: Dependencies SSOT comment | Added External type to SSOT Dependency Types comment (4 rows use External for unmigrated ERB dependencies)
- [fix] Phase2-Review iter1: Philosophy Derivation scope | Expanded "eliminating TRYCALL coupling" derived requirement to include interface creation + DI registration (AC#1-11), not just null sentinel (AC#5-7)
- [fix] Phase2-Uncertain iter1: NotImplementedException stub gap | Added AC#51-66 not_matches for NotImplementedException in WcCounterMessageItem.cs and WcCounterMessageNtr.cs (established stub pattern not covered by AC#28-43 TODO/FIXME/HACK check)
- [fix] Phase2-Review iter1: AC#25 matcher fragility | Changed AC#25 pattern from `\.NtrRevelation\(target,\s*actionType\)` to `\.NtrRevelation\([^,]+,\s*[^)]+\)` (parameter-name-agnostic two-argument forwarding)
- [fix] Phase2-Review iter2: AC#34 CFLAG/TALENT split | Split AC#34 into AC#34 (CFLAG: SetCharacterFlag|CflagWc) and AC#53 (TALENT: SetTalent|TalentIndex) to ensure independent coverage of distinct C5 mutation types
- [fix] Phase2-Review iter3: AC#19 .Value enforcement | Changed AC#19 matcher from `\.NtrResetVisitorAction\(attacker` to `\.NtrResetVisitorAction\(attacker\.Value` (CharacterId→int unwrap required by interface signature)
- [fix] Phase2-Review iter3: C12 favor/surrender AC | Added AC#54 verifying WcCounterMessageNtrTests covers NtrReversalSource/NtrAgreeSource calculation paths (C12 constraint: "ACs should cover favor/surrender calculation outcomes")
- [fix] Phase2-Review iter4: SOURCE write target fix | Fixed Technical Design SOURCE write pattern: removed _engine.GetTarget(), documented offender parameter usage (ARG=同席者のキャラ番号, NTR.ERB:210)
- [fix] Phase2-Review iter4: SetSource offender AC | Added AC#55 verifying SetSource(offender usage in WithPetting — prevents wrong-character SOURCE mutation (C6)
- [fix] Phase3-Maintainability iter5: 9-dep Key Decision | Added Key Decisions entry documenting WcCounterMessageNtr 9-dependency constructor rationale (cohesive NTR context, ERB implicit global state)
- [fix] Phase3-Maintainability iter5: SETBIT MASTER target | Clarified withPETTING SETBIT example: CFLAG:MASTER:WC_箇所埋まり targets MASTER character (toilet manager), confirmed NTR.ERB:374
- [fix] Phase2-Review iter6: AC#49 target forwarding | Strengthened AC#49 from `_kojoMessage\.` to `_kojoMessage\.[A-Za-z]+.*target` (verifies KOJO dispatch forwards target CharacterId per C9)
- [fix] Phase2-Review iter6: AC#36 multi-path | Strengthened AC#36 from matches to gte 3 (follows AC#35 multi-path precedent for C7 11 SETBIT calls)
- [fix] Phase2-Review iter7: AC#49 .Value enforcement | Changed AC#49 pattern to `_kojoMessage\.[A-Za-z]+.*target\.Value` (CharacterId→int unwrap required by IKojoMessageService, follows AC#19 pattern)
- [fix] Phase2-Review iter8: AC#34 disambiguation | Narrowed AC#34 from `SetCharacterFlag|CflagWc` to `Kanrinin|管理人` — prevents withPETTING SETBIT tests (CflagWcKashoUmari) from satisfying C5 NtrRevelation CFLAG AC
- [fix] Phase2-Review iter9: AC#47 pattern completion | Added _textFormatting\. to AC#47 pattern (5th injected service was missing from non-stub verification)
- [fix] Phase2-Uncertain iter9: Philosophy Derivation trace | Added AC#49 (KOJO call-site) and AC#50 (DATETIME call-site) to SSOT Philosophy Derivation row
- [fix] Phase2-Review iter10: AC#43 argument fix | Changed AC Coverage row 57 from GetNtrName(target.Value) to GetNtrName(0) — NTR_NAME(0) at NTR.ERB:850 is literal 0 (NTR name system index)
- [fix] Phase2-Review iter10: SETBIT MASTER AC | Added AC#56 verifying GetMaster usage in WcCounterMessageNtr.cs for CFLAG:MASTER:WC_箇所埋まり SETBIT operations (parallel to AC#55 for SOURCE offender)
- [fix] Phase2-Review iter1: AC Definition Table ordering | Moved AC#53 from between AC#34/48 to correct position between AC#52/68 (ascending order compliance)
- [fix] Phase2-Review iter1: AC Coverage table ordering | Moved AC#49/64 from after AC#56 to correct position between AC#48/65 (ascending order compliance)
- [fix] Phase2-Review iter2: AC Coverage table AC#53 ordering | Moved AC#53 from between AC#34/48 to correct position between AC#52/68 in AC Coverage table (consistent with AC Definition Table)
- [fix] Phase2-Review iter2: Philosophy Derivation AC grouping | Reordered 'enabling TDD' behavioral row AC list to place AC#53 adjacent to AC#34 (both C5 co-derivatives)
- [fix] Phase2-Review iter3: AC#34 multi-path strengthening | Changed AC#34 from matches to gte 2 — C5 requires two distinct CFLAG writes (WC_管理人は誰, WC_前任管理人), single match insufficient
- [fix] Phase2-Review iter3: C13 implementation-side gap | Added AC#57 verifying WcCounterMessageItem.cs references 応答分類/ResponseCategory (prevents stub bypassing SELECTCASE branching)
- [fix] Phase2-Review iter4: C5 TALENT implementation-side gap | Added AC#58 verifying WcCounterMessageNtr.cs calls SetTalent (C5 TALENT:管理人=0 at NTR.ERB:920)
- [fix] Phase2-Review iter4: C6 wrong-character prevention | Added AC#59 not_matches verifying WcCounterMessageNtr.cs does not use SetSource with GetTarget (ARG=offender, not TARGET)
- [fix] Phase2-Review iter5: Success Criteria stale count | Updated AC count from 70 to 74 (AC#57-74 added in iter3-5)
- [fix] Phase2-Review iter5: C5 CFLAG implementation-side gap | Added AC#60 verifying SetCharacterFlag for NtrRevelation CFLAG mutations (pairwise: AC#34 test + AC#60 impl)
- [fix] Phase2-Review iter5: Philosophy Derivation C12 grouping | Moved AC#54 before assertion-presence ACs (AC#44-60) for domain-specific grouping
- [fix] Phase2-Review iter6: AC#60 gte 2 + dual-language | Changed AC#60 from matches to gte 2, added 管理人 to pattern (parallels AC#34, covers romanization variants)
- [fix] Phase2-Review iter6: C5 MASTER targeting gap | Added AC#61 verifying NtrRevelation CFLAG writes use master variable (CFLAG:MASTER per NTR.ERB:918-919, parallel to AC#56 for withPETTING)
- [fix] Phase2-Review iter6: Success Criteria count | Updated from 74 to 75
- [fix] Recovery: AC#62-96 restored from session JSONL (d461a087, 6b4f7d0a, 63cab05e, 7b02e27c) after accidental git checkout -- wiped all FL session changes
- [fix] Phase2-Review iter1: AC Details ordering | Moved AC#53 detail block from between AC#34/AC#35 to correct position between AC#52/AC#54 (ascending order compliance)
- [fix] Phase2-Review iter1: Review Notes comment path | Changed pm/reference/error-taxonomy.md to docs/reference/error-taxonomy.md (SSOT path correction)
- [fix] Phase2-Review iter1: Baseline File path | Changed .tmp/baseline-808.txt to _out/tmp/baseline-808.txt (CLAUDE.md file placement policy)
- [fix] Phase2-Review iter2: C15 AC reference | Fixed C15 Constraint Details: AC#56/AC#57 → AC#42/AC#43 (GetNtrName declaration/call site)
- [fix] Phase2-Review iter3: Philosophy Derivation scope | Clarified "eliminating runtime TRYCALL coupling" row: F808 provides prerequisite infrastructure, TRYCALL elimination completes when F806/F807 consume interfaces
- [pending] Phase2-Pending iter4: AC#47/48 per-function non-stub coverage — gte 10 service usages can be concentrated in 1 method while 16 others are empty `{ }` bodies. Overlaps existing [pending] at iter4 (behavioral equivalence gap). Resolution requires threshold analysis of expected service calls per function group.
- [fix] Phase3-Maintainability iter4: AC#38 stale ref | Changed "Same as AC#50" to "Same as AC#37" (AC#50 is GetDateTime, AC#37 is F806 Related removal)
- [fix] Phase3-Maintainability iter4: Stale cross-references | Fixed AC#48 (AC#61→AC#47), AC#49 (AC#28→AC#19, AC#35→AC#14), AC#50 (AC#56+57→AC#42+43), AC#51 (AC#41→AC#28, AC#61→AC#47), AC#52 (AC#65→AC#51, AC#62→AC#48), AC#56 (AC#69→AC#55), AC#57 (AC#52→AC#39), AC#58 (AC#67→AC#53), AC#59 (AC#69→AC#55), AC#60 (AC#47→AC#34, AC#72→AC#58, AC#47+74→AC#34+60), AC#61 (AC#70→AC#56), AC#66 (AC#47+75→AC#34+60), AC#70 (AC#75→AC#60)
- [pending] Phase4-ACLint iter5: AC#25 regex false positive — ac_ops reports unbalanced parentheses for `\.NtrRevelation\([^,]+,\s*[^)]+\)` but regex is valid (character class `[^)]+` contains literal `)`)
- [pending] Phase4-ACLint iter5: 20 ACs (4,7,13,14,20,34,35,36,48,60,61,63,64,65,68,69,71,79,80,81) report "gte matcher lacks derivation" — AC Details DO contain threshold derivation text but ac_ops format pattern may not recognize it

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning
- [Predecessor: F805](feature-805.md) - WC Counter Source + Message Core
- [Successor: F806](feature-806.md) - WC Counter Message SEX
- [Successor: F807](feature-807.md) - WC Counter Message TEASE
- [Successor: F813](feature-813.md) - Post-Phase Review Phase 21

---
