# Feature 805: WC Counter Source + Message Core

## Status: [DONE]
<!-- fl-reviewed: 2026-02-28T00:21:47Z -->

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

Phase 21 Counter System C# migration is the SSOT for all counter-related game logic. Each ERB counter file is migrated to a dedicated C# class behind DI interfaces, enabling unit-testable, type-safe counter processing. F805 covers the WC (toilet) counter source dispatch and core message routing, which together form the entry point that sibling message handlers (F806/F807/F808) depend on.

### Problem (Current Issue)

TOILET_COUNTER_SOURCE.ERB (1411 lines) and TOILET_COUNTER_MESSAGE.ERB (496 lines) remain as unmigrated ERB scripts. The IWcCounterSourceHandler interface (Era.Core/Counter/IWcCounterSourceHandler.cs) is a stub created by F811 with no implementing class. The source file contains a 172-line favor calculation subsystem (WC_COUNTER_FAVOR_CALC, lines 1184-1356) that uses the HETEROSEX function (line 1219), which has no C# equivalent in ICommonFunctions. The message file contains WC_LOVEranking (lines 478-496), a FUNCTION called externally from EVENTTURNEND.ERB:81, requiring public exposure via an interface. Until F805 implements these, the WC counter source dispatch is a no-op stub and sibling features F806/F807/F808 cannot proceed because they depend on the message dispatcher defined here.

### Goal (What to Achieve)

Migrate TOILET_COUNTER_SOURCE.ERB and TOILET_COUNTER_MESSAGE.ERB to C# classes that implement IWcCounterSourceHandler and IWcCounterOutputHandler (SendMessage, HandleReaction, HandlePunishment) respectively, using the established F803 CounterSourceHandler and F802 CounterMessage DI patterns. Add the missing HeteroSex method to ICommonFunctions. Expose WC_LOVEranking as a public interface method. Replace the F811 stub with a full implementation that injects ITouchStateManager for touch state access.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is WC counter source dispatch a no-op? | IWcCounterSourceHandler has only a stub implementation from F811 | Era.Core/Counter/IWcCounterSourceHandler.cs:10-14 |
| 2 | Why is it still a stub? | F805 was created as a DRAFT planning placeholder from F783 decomposition and has not been implemented | pm/features/feature-805.md:2,44 |
| 3 | Why does the DRAFT underestimate scope? | Line counts were estimated before a prior refactoring (commit-era F033, no feature file) split TOILET_COUNTER_MESSAGE into 4 sub-files, and helper functions (FAVOR_CALC 172 lines, ForcedOrgasm 50 lines, WC_LOVEranking 19 lines) were not separately identified | TOILET_COUNTER_MESSAGE.ERB:5-9, TOILET_COUNTER_SOURCE.ERB:1184,1361 |
| 4 | Why are interface gaps not documented? | WC_COUNTER_FAVOR_CALC uses HETEROSEX (COMMON.ERB:148-158) which was never needed by any previously migrated feature (F803 uses HasPenis/HasVagina but not gender-comparison) | Era.Core/Interfaces/ICommonFunctions.cs:10-11, TOILET_COUNTER_SOURCE.ERB:1219 |
| 5 | Why can sibling features not proceed? | F806/F807/F808 depend on the message dispatcher (TRYCALLFORM EVENT_WC_COUNTER_MESSAGE{N}) defined in TOILET_COUNTER_MESSAGE.ERB which F805 must implement first | TOILET_COUNTER_MESSAGE.ERB:21,24 |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | WC counter source dispatch is a no-op stub; sibling message features blocked | Two large ERB files (1411+496 lines) unmigrated; IWcCounterSourceHandler stub has no implementation; HETEROSEX interface gap blocks favor calculation migration |
| Where | Era.Core/Counter/IWcCounterSourceHandler.cs (stub), TOILET_COUNTER_SOURCE.ERB, TOILET_COUNTER_MESSAGE.ERB | Missing WcCounterSourceHandler.cs and WcCounterMessage.cs implementing classes; missing HeteroSex in ICommonFunctions |
| Fix | Leave stub in place | Implement full C# classes following F803/F802 patterns, add HeteroSex to ICommonFunctions, expose WC_LOVEranking via interface |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| F802 | [DONE] | Main Counter Output (CounterMessage.cs pattern for message dispatch) |
| F803 | [DONE] | Main Counter Source (CounterSourceHandler.cs pattern; provides DATUI_*/PainCheckVMaster via ICounterSourceHandler) |
| F804 | [DONE] | WC Counter Core (defines IWcCounterOutputHandler interface) |
| F811 | [DONE] | SOURCE Entry System (provides IWcCounterSourceHandler stub, ITouchStateManager, SourceEntrySystem call site) |
| F806 | [DRAFT] | WC Counter Message SEX (depends on F805 dispatcher) |
| F807 | [DRAFT] | WC Counter Message TEASE (depends on F805 dispatcher) |
| F808 | [DRAFT] | WC Counter Message ITEM + NTR (depends on F805 dispatcher; F805 MESSAGE13 TRYCALLs NTRrevelation) |
| F813 | [DRAFT] | Post-Phase Review Phase 21 |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Pattern availability | FEASIBLE | CounterSourceHandler.cs (F803) and CounterMessage.cs (F802) provide direct 1:1 patterns |
| Interface contracts | FEASIBLE | IWcCounterSourceHandler and IWcCounterOutputHandler stubs exist; ITouchStateManager available from F811 |
| Interface gaps | FEASIBLE | Only gap is HeteroSex in ICommonFunctions -- straightforward addition with default implementation |
| Predecessor dependencies | FEASIBLE | F783, F803, F804, F811 all [DONE] |
| DI integration | FEASIBLE | SourceEntrySystem.cs already injects IWcCounterSourceHandler stub at line 249 |
| Scope clarity | FEASIBLE | Clear ERB function boundaries; corrected line counts: SOURCE=1411, MESSAGE=496 |
| Complexity | FEASIBLE | ~60 SELECTCASE branches + 172-line favor calc are large but structurally repetitive |
| AC volume | FEASIBLE | 58 ACs exceed erb template guideline (8-15) proportionally: 1907 ERB lines (3.8x the ~500 line erb guideline) require comprehensive constructor dependency verification (14 DI ACs), behavioral tests (17 test ACs), and interface contract ACs. Feature cannot be split further — WcCounterSourceHandler and WcCounterMessage are tightly coupled via shared DI and the same IWcCounterOutputHandler interface |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| WC Counter System | HIGH | Replaces stub with full source dispatch, unblocking all WC counter functionality |
| Sibling Features F806/F807/F808 | HIGH | Message dispatcher implementation unblocks 3 successor features |
| ICommonFunctions | MEDIUM | Adding HeteroSex method extends shared interface; backward compatible |
| SourceEntrySystem (F811) | LOW | No changes needed; already wired to inject IWcCounterSourceHandler |
| Main Counter (F803) | LOW | F805 consumes ICounterSourceHandler.DatUI*/PainCheckVMaster but does not modify F803 |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| HETEROSEX function has no C# equivalent | ICommonFunctions.cs (no HeteroSex method) | Must add HeteroSex to ICommonFunctions before FAVOR_CALC can compile |
| Must use ITouchStateManager, not ITouchSet | F811 mandatory handoff (feature-805.md:72-73) | Different parameter signature from F803's ITouchSet; TouchSet takes (int, int, int, bool) |
| IWcCounterSourceHandler has single method | IWcCounterSourceHandler.cs:13 | FavorCalc and ForcedOrgasm must be internal/private or via separate interface |
| TRYCALLFORM dynamic dispatch in MESSAGE | TOILET_COUNTER_MESSAGE.ERB:21,24 | Must implement switch/dictionary dispatch pattern for action IDs |
| MESSAGE13 calls into F808 scope (NTRrevelation) | TOILET_COUNTER_MESSAGE.ERB:283 | Must use TRYCALL semantics: no-op if handler absent |
| WC_LOVEranking called externally | TOILET_COUNTER_MESSAGE.ERB:478, EVENTTURNEND.ERB:81 | Must be exposed as public interface method, not internal |
| PAIN_CHECK_V (not V_MASTER) at line 930 | TOILET_COUNTER_SOURCE.ERB:930 | Reverse-rape case uses IPainStateChecker.CheckPain directly |
| KOJO TRYCALLFORM dispatch | TOILET_COUNTER_MESSAGE.ERB:136-431 | 8 kojo calls must use IKojoMessageService or equivalent facade |
| DATUI_MESSAGE from WC context | TOILET_COUNTER_MESSAGE.ERB:16 | Must call F802 CounterMessage.DatuiMessage or equivalent. Note: ERB's DATUI_MESSAGE is a shared global function (Game/ERB/COUNTER_MESSAGE.ERB:484); F802 migrated it as private in CounterMessage.cs. WcCounterMessage must re-implement the same logic privately (DRY duplication tracked as Mandatory Handoff to F813 for post-phase extraction to shared IDatuiMessageService) |
| SOURCE loop indices 0-99 for zero-activity check | TOILET_COUNTER_SOURCE.ERB:1350-1356 | Integer-only arithmetic; FOR LOCAL,0,100 (exclusive upper bound) |
| Phase 21 DI registrations are incremental | ServiceCollectionExtensions.cs | F805 registers: (a) F805-owned classes (WcCounterSourceHandler, WcCounterMessage) and (b) F804-owned dependency classes (WcCounterReaction, WcCounterPunishment) because WcCounterMessage takes them as concrete constructor parameters and DI must resolve them. Also registers INtrRevelationHandler null sentinel (AC#57). Transitive dependencies (ICounterSourceHandler from F803, ITouchStateManager from F811, IKojoMessageService, IShrinkageSystem) are NOT registered in the DI container — Phase 21 counter system DI integration is deferred to later features (e.g., F813 Post-Phase Review). Unit tests use direct construction with mocked dependencies, bypassing DI. **Runtime note**: AddSingleton<IWcCounterSourceHandler, WcCounterSourceHandler>() and AddSingleton<IWcCounterOutputHandler, WcCounterMessage>() will throw InvalidOperationException if DI resolution is triggered before F813 completes all transitive registrations; this is by design (Phase 21 incremental registration) |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Scope underestimation from incorrect DRAFT line counts | HIGH | MEDIUM | Corrected to 1411/496; total 1907 lines |
| WC_COUNTER_FAVOR_CALC complexity (172 lines of compound arithmetic) | MEDIUM | HIGH | Separate class or method with dedicated unit tests |
| Missing HeteroSex blocks compilation | HIGH | LOW | Add to ICommonFunctions with default implementation |
| ITouchStateManager vs ITouchSet parameter mismatch | MEDIUM | MEDIUM | Use ITouchStateManager per F811 obligation; adapt parameter types |
| Sibling features F806/F807/F808 blocked until F805 completes | HIGH | HIGH | Prioritize message dispatcher design for unblocking |
| WC_LOVEranking external dependency missed in original DRAFT | HIGH | MEDIUM | Explicit AC for public interface exposure |
| NTRrevelation TRYCALL forward reference to F808 | MEDIUM | LOW | No-op stub pattern (TRYCALL semantics = silent failure) |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| SOURCE ERB lines | `wc -l Game/ERB/TOILET_COUNTER_SOURCE.ERB` | 1411 | Corrected from DRAFT estimate of 838 |
| MESSAGE ERB lines | `wc -l Game/ERB/TOILET_COUNTER_MESSAGE.ERB` | 496 | Corrected from DRAFT estimate of 1069 |
| IWcCounterSourceHandler methods | `grep -c "void\|int\|bool" Era.Core/Counter/IWcCounterSourceHandler.cs` | 1 | Stub: HandleWcCounterSource only |
| ICommonFunctions methods | `grep -c "\\(.*\\);" Era.Core/Interfaces/ICommonFunctions.cs` | 15 | Pre-migration baseline (method signature count matching AC#7 pattern) |
| Existing test count | `dotnet test --no-build --list-tests 2>/dev/null \| grep -c "WcCounter"` | 0 | No WC counter tests exist |

**Baseline File**: `.tmp/baseline-805.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Must implement IWcCounterSourceHandler.HandleWcCounterSource | IWcCounterSourceHandler.cs:13 | Verify interface implementation exists and replaces stub |
| C2 | Must use ITouchStateManager (F811), not ITouchSet (F803) | feature-805.md deferred obligations | Verify ITouchStateManager injection in constructor |
| C3 | Must delegate to ICounterSourceHandler.DatUI helpers from F803 | TOILET_COUNTER_SOURCE.ERB:570-923 (23 CALL DATUI_* calls) | Verify delegation calls to ICounterSourceHandler |
| C4 | FAVOR_CALC SOURCE loop checks indices 0-99 for zero activity | TOILET_COUNTER_SOURCE.ERB:1349-1356 | Verify zero-activity guard zeroes accumulator tracking value (FLAG:好感度上昇の累積値), not the already-applied CFLAG favor |
| C5 | ForcedOrgasm minimum floor of 2 | TOILET_COUNTER_SOURCE.ERB:1408-1409 | Verify floor enforcement |
| C6 | Message dispatcher must handle branch vs non-branch modes | TOILET_COUNTER_MESSAGE.ERB:19-25 | Test both dispatch paths (with/without messageVariant) |
| C7 | NTRrevelation TRYCALL must be no-op when absent | TOILET_COUNTER_MESSAGE.ERB:283 | Test graceful fallback (no exception) |
| C8 | SETBIT master counter control bit positions (lines 1039-1177) | TOILET_COUNTER_SOURCE.ERB:1039-1177 | Verify all MCC bit flag mappings |
| C9 | CFLAG history rotation at entry | TOILET_COUNTER_SOURCE.ERB:9-10 | Verify WC_前々回/前回セクハラ rotation |
| C10 | Early exit guard when 決定フラグ > 1 | TOILET_COUNTER_SOURCE.ERB:3-4 | Verify early return path |
| C11 | WC_LOVEranking must be publicly exposed via interface | TOILET_COUNTER_MESSAGE.ERB:478-496, EVENTTURNEND.ERB:81 | Verify public method returning rank value |
| C12 | HeteroSex method must be added to ICommonFunctions | TOILET_COUNTER_SOURCE.ERB:1219, ICommonFunctions.cs | Verify interface extension with backward compatibility |
| C13 | MESSAGE functions return RESULT=1 for PrintLine guard | TOILET_COUNTER_MESSAGE.ERB:26-27 | Verify nonzero result triggers line break |
| C14 | DATUI_MESSAGE called before dispatch | TOILET_COUNTER_MESSAGE.ERB:16 | Verify undressing message precedes action dispatch |
| C15 | Reverse-rape case must use IPainStateChecker.CheckPain directly, not PainCheckVMaster | TOILET_COUNTER_SOURCE.ERB:930 | Verify reverse-rape path calls CheckPain, not PainCheckVMaster |

### Constraint Details

**C1: IWcCounterSourceHandler Implementation**
- **Source**: F811 created stub interface; F805 must provide actual implementation
- **Verification**: `grep "class.*IWcCounterSourceHandler" Era.Core/Counter/WcCounterSourceHandler.cs`
- **AC Impact**: AC must verify implementing class exists and SourceEntrySystem.cs:249 call site works

**C2: ITouchStateManager Injection**
- **Source**: F811 mandatory handoff (feature-805.md:72-73); ITouchStateManager has different signature from ITouchSet
- **Verification**: Constructor parameter includes ITouchStateManager, not ITouchSet
- **AC Impact**: AC must grep for ITouchStateManager in WcCounterSourceHandler constructor

**C3: DATUI Helper Delegation**
- **Source**: TOILET_COUNTER_SOURCE.ERB lines 570-923 contain 23 CALL DATUI_* statements
- **Verification**: WcCounterSourceHandler calls ICounterSourceHandler.DatUIBottom/Top/BottomT/TopT
- **AC Impact**: AC must verify delegation to F803 interface, not reimplementation

**C4: Zero-Activity Guard in FAVOR_CALC**
- **Source**: TOILET_COUNTER_SOURCE.ERB:1349-1356 loops SOURCE indices 0-99; if none active, sets increment to 0
- **Verification**: Unit test with all SOURCE values at 0
- **AC Impact**: Boundary test: no activity means accumulator tracking value (FLAG:好感度上昇の累積値) is zeroed; CFLAG:好感度 is already applied at line 1341 before the check

**C5: ForcedOrgasm Floor**
- **Source**: TOILET_COUNTER_SOURCE.ERB:1408-1409 enforces minimum of 2
- **Verification**: Unit test with values that would produce less than 2
- **AC Impact**: Verify floor(2) for each of 5 body-part sub-cases

**C6: Message Dispatch Modes**
- **Source**: TOILET_COUNTER_MESSAGE.ERB:19-25; branch mode iterates 1-4, non-branch dispatches single
- **Verification**: Unit tests for both paths
- **AC Impact**: Two dispatch paths must be tested independently

**C7: NTRrevelation No-Op**
- **Source**: TRYCALL at TOILET_COUNTER_MESSAGE.ERB:283; F808 not yet implemented
- **Verification**: Call with no registered handler must not throw
- **AC Impact**: Negative test: no exception when handler absent

**C8: MCC Bit Flags**
- **Source**: TOILET_COUNTER_SOURCE.ERB:1039-1177; ~30 SETBIT action mappings
- **Verification**: Representative branch coverage of SETBIT operations
- **AC Impact**: Verify bit positions match ERB source for key action types

**C9: CFLAG History Rotation**
- **Source**: TOILET_COUNTER_SOURCE.ERB:9-10; WC_前々回セクハラ = WC_前回セクハラ, WC_前回セクハラ = current action
- **Verification**: Unit test verifying two-step rotation
- **AC Impact**: Must test rotation order (previous-previous before previous)

**C10: Early Exit Guard**
- **Source**: TOILET_COUNTER_SOURCE.ERB:3-4; SIF TCVAR:ARG:カウンター行動決定フラグ > 1 RETURN 0
- **Verification**: Unit test with flag > 1
- **AC Impact**: Verify no side effects when early-exiting

**C11: WC_LOVEranking Public Exposure**
- **Source**: TOILET_COUNTER_MESSAGE.ERB:478-496 (FUNCTION); called from EVENTTURNEND.ERB:81
- **Verification**: Interface method exists and returns int rank
- **AC Impact**: Must verify return value (not void); must be accessible outside counter scope

**C12: HeteroSex Interface Extension**
- **Source**: TOILET_COUNTER_SOURCE.ERB:1219 uses HETEROSEX(ARG,MASTER); no C# equivalent exists
- **Verification**: ICommonFunctions.cs contains HeteroSex method; backward compatible (existing method count preserved)
- **AC Impact**: Must test gender-comparison logic; verify existing methods unchanged

**C13: MESSAGE Result Guard**
- **Source**: TOILET_COUNTER_MESSAGE.ERB:26-27; SIF RESULT then PRINTL
- **Verification**: Unit test with RESULT=1 and RESULT=0
- **AC Impact**: PrintLine only called when message was actually printed

**C14: DATUI_MESSAGE Precondition**
- **Source**: TOILET_COUNTER_MESSAGE.ERB:16; CALL DATUI_MESSAGE before any dispatch
- **Verification**: Verify DatuiMessage called before dispatch in message handler
- **AC Impact**: Sequence test: undressing message must precede action message

**C15: Reverse-Rape CheckPain Path**
- **Source**: TOILET_COUNTER_SOURCE.ERB:930; reverse-rape case uses PAIN_CHECK_V (not V_MASTER)
- **Verification**: Unit test verifying IPainStateChecker.CheckPain called and ICounterSourceHandler.PainCheckVMaster NOT called for reverse-rape action
- **AC Impact**: AC must test both positive (CheckPain called) and negative (PainCheckVMaster not called) assertions

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| Predecessor | F803 | [DONE] | Main Counter Source -- provides DATUI_Bottom/Top/BottomT/TopT and PainCheckVMaster via ICounterSourceHandler; TOILET_COUNTER_SOURCE.ERB:570-923 contains 23 CALL DATUI_* |
| Predecessor | F804 | [DONE] | WC Counter Core -- defines IWcCounterOutputHandler interface that F805 implements (SendMessage, HandleReaction, HandlePunishment) |
| Predecessor | F811 | [DONE] | SOURCE Entry System -- provides IWcCounterSourceHandler stub (F805 replaces), ITouchStateManager (F805 consumes), SourceEntrySystem call site at line 249 |
| Successor | F806 | [DRAFT] | WC Counter Message SEX -- depends on F805 message dispatcher (TRYCALLFORM EVENT_WC_COUNTER_MESSAGE{N}) |
| Successor | F807 | [DRAFT] | WC Counter Message TEASE -- depends on F805 message dispatcher |
| Successor | F808 | [DRAFT] | WC Counter Message ITEM + NTR -- depends on F805 message dispatcher; F805 MESSAGE13 TRYCALLs NTRrevelation (non-blocking) |
| Successor | F813 | [DRAFT] | Post-Phase Review Phase 21 |
| Related | F802 | [DONE] | Main Counter Output -- CounterMessage.cs provides pattern for message dispatch; DatuiMessage already migrated |

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
| "SSOT for **all** counter-related game logic" | WC counter source and message must be migrated to C# (no ERB-only logic remains for this scope). Verification strategy: AC#56 provides structural completeness (count_gte: 55 dispatch methods, a proxy for ERB branch coverage); AC#21 and AC#27 provide representative behavioral verification for DATUI delegation and reverse-rape paths. Exception: C8 MCC bit constants (~30 SETBIT mappings) verified by representative sampling per AC Design Constraint C8; compile-time type safety provides exhaustive structural coverage, AC#17 verifies representative positions | AC#1, AC#2, AC#17, AC#21, AC#27, AC#56 |
| "**Each** ERB counter file is migrated to a **dedicated** C# class" | TOILET_COUNTER_SOURCE.ERB -> WcCounterSourceHandler.cs; TOILET_COUNTER_MESSAGE.ERB -> WcCounterMessage.cs (separate files) | AC#1, AC#2 |
| "behind **DI interfaces**, enabling **unit-testable**" | Classes implement interfaces (IWcCounterSourceHandler, IWcCounterOutputHandler); constructor injection for all dependencies. NTRrevelation forward reference uses `INtrRevelationHandler?` per Key Decision (DI-standard pattern; F808 implements the interface) | AC#1, AC#3, AC#14, AC#15 |
| "**type-safe** counter processing" | Use CharacterId typed parameter, not raw int; use ITouchStateManager (not ITouchSet) per F811 mandate. CharacterId enforced transitively: IWcCounterSourceHandler.cs:13 defines `HandleWcCounterSource(CharacterId target, int arg1)`, AC#1 (class implements interface) + AC#15 (build) enforce parameter type. Exception: WcLoveRanking uses `int characterIndex` per Key Decision (ERB caller boundary — EVENTTURNEND.ERB:81 passes raw index with no CharacterId conversion path) | AC#1, AC#3, AC#15 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | WcCounterSourceHandler implements IWcCounterSourceHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `class WcCounterSourceHandler.*IWcCounterSourceHandler` | [x] |
| 2 | WcCounterMessage class exists (Pos) | file | Glob(Era.Core/Counter/WcCounterMessage.cs) | exists | - | [x] |
| 3 | ITouchStateManager injected in WcCounterSourceHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `ITouchStateManager` | [x] |
| 4 | ITouchSet NOT used in WcCounterSourceHandler (Neg) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | not_matches | `ITouchSet` | [x] |
| 5 | ICounterSourceHandler delegation for DATUI helpers (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `ICounterSourceHandler` | [x] |
| 6 | HeteroSex added to ICommonFunctions (Pos) | code | Grep(Era.Core/Interfaces/ICommonFunctions.cs) | matches | `HeteroSex` | [x] |
| 7 | ICommonFunctions backward compatible -- method count (Pos, re-baseline Expected before testing) | code | Grep(path="Era.Core/Interfaces/ICommonFunctions.cs", pattern="\\(.*\\);") | count_equals | 16 | [x] |
| 8 | WcLoveRanking public interface method (Pos) | code | Grep(Era.Core/Counter/IWcCounterOutputHandler.cs) | matches | `int.*WcLoveRanking` | [x] |
| 9 | F811 stub comment removed from interface (Pos) | code | Grep(Era.Core/Counter/IWcCounterSourceHandler.cs) | not_matches | `stub` | [x] |
| 10 | Early exit guard unit test (Pos, C10) | test | dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*EarlyExit" | succeeds | - | [x] |
| 11 | CFLAG history rotation unit test (Pos, C9) | test | dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*HistoryRotation" | succeeds | - | [x] |
| 12 | ForcedOrgasm floor of 2 unit test (Pos, C5) | test | dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*ForcedOrgasm.*Floor" | succeeds | - | [x] |
| 13 | Zero-activity guard unit test (Pos, C4) | test | dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*ZeroActivity" | succeeds | - | [x] |
| 14 | NTRrevelation no-op when handler absent (Neg, C7) | test | dotnet test --filter "FullyQualifiedName~WcCounterMessage.*NtrRevelation.*NoOp" | succeeds | - | [x] |
| 15 | C# build succeeds | build | dotnet build Era.Core/ | succeeds | - | [x] |
| 16 | WcCounterMessage implements IWcCounterOutputHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterMessage.cs) | matches | `class WcCounterMessage.*IWcCounterOutputHandler` | [x] |
| 17 | MCC bit flag mapping unit test (Pos, C8) | test | dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*MccBit" | succeeds | - | [x] |
| 18 | Message dispatch branch vs non-branch modes unit test (Pos, C6) | test | dotnet test --filter "FullyQualifiedName~WcCounterMessage.*DispatchBranch" | succeeds | - | [x] |
| 19 | MESSAGE result guard unit test (Pos, C13) | test | dotnet test --filter "FullyQualifiedName~WcCounterMessage.*ResultGuard" | succeeds | - | [x] |
| 20 | IKojoMessageService used for kojo dispatch in WcCounterMessage (Pos) | code | Grep(Era.Core/Counter/WcCounterMessage.cs) | matches | `IKojoMessageService` | [x] |
| 21 | DATUI delegation behavioral test (Pos, C3) | test | dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*DatuiDelegation" | succeeds | - | [x] |
| 22 | HeteroSex behavioral test (Pos, C12) | test | dotnet test --filter "FullyQualifiedName~CommonFunctions.*HeteroSex" | succeeds | - | [x] |
| 23 | DATUI_MESSAGE sequence precondition test (Pos, C14) | test | dotnet test --filter "FullyQualifiedName~WcCounterMessage.*DatuiMessageSequence" | succeeds | - | [x] |
| 24 | HandleReaction behavioral test (Pos) | test | dotnet test --filter "FullyQualifiedName~WcCounterMessage.*HandleReaction" | succeeds | - | [x] |
| 25 | HandlePunishment behavioral test (Pos) | test | dotnet test --filter "FullyQualifiedName~WcCounterMessage.*HandlePunishment" | succeeds | - | [x] |
| 26 | WcLoveRanking behavioral test (Pos, C11) | test | dotnet test --filter "FullyQualifiedName~WcCounterMessage.*WcLoveRanking" | succeeds | - | [x] |
| 27 | Reverse-rape path uses CheckPain not PainCheckVMaster (Pos, C15) | test | dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*ReversedRape.*CheckPain" | succeeds | - | [x] |
| 28 | FAVOR_CALC HeteroSex integration test (Pos, C4/C12) | test | dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*FavorCalc.*HeteroSex" | succeeds | - | [x] |
| 29 | IConsoleOutput injected in WcCounterMessage (Pos) | code | Grep(Era.Core/Counter/WcCounterMessage.cs) | matches | `IConsoleOutput` | [x] |
| 30 | NTRrevelation invoked when handler provided (Pos, C7) | test | dotnet test --filter "FullyQualifiedName~WcCounterMessage.*NtrRevelation.*Invoked" | succeeds | - | [x] |
| 31 | IShrinkageSystem injected in WcCounterSourceHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `IShrinkageSystem` | [x] |
| 32 | IVirginityManager injected in WcCounterSourceHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `IVirginityManager` | [x] |
| 33 | IKojoMessageService.KojoMessageWcCounter called with correct characterIndex and actionType for kojo dispatch (Pos) | test | dotnet test --filter "FullyQualifiedName~WcCounterMessage.*KojoDispatch" | succeeds | - | [x] |
| 34 | WcCounterSourceHandler registered in DI composition root (Pos) | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `IWcCounterSourceHandler.*WcCounterSourceHandler` | [x] |
| 35 | WcCounterMessage registered in DI composition root (Pos) | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `IWcCounterOutputHandler.*WcCounterMessage` | [x] |
| 36 | Representative action-ID-to-variant dispatch routing test (Pos, C6-ext) | test | dotnet test --filter "FullyQualifiedName~WcCounterMessage.*DispatchRouting" | succeeds | - | [x] |
| 37 | IEngineVariables injected in WcCounterSourceHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `IEngineVariables` | [x] |
| 38 | ITEquipVariables injected in WcCounterSourceHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `ITEquipVariables` | [x] |
| 39 | ICharacterStringVariables injected in WcCounterSourceHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `ICharacterStringVariables` | [x] |
| 40 | IRandomProvider injected in WcCounterSourceHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `IRandomProvider` | [x] |
| 41 | WcCounterReaction registered in DI composition root (Pos) | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `WcCounterReaction` | [x] |
| 42 | WcCounterPunishment registered in DI composition root (Pos) | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `WcCounterPunishment` | [x] |
| 43 | ICommonFunctions injected in WcCounterSourceHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `ICommonFunctions` | [x] |
| 44 | ITEquipVariables injected in WcCounterMessage (Pos) | code | Grep(Era.Core/Counter/WcCounterMessage.cs) | matches | `ITEquipVariables` | [x] |
| 45 | ITextFormatting injected in WcCounterMessage (Pos) | code | Grep(Era.Core/Counter/WcCounterMessage.cs) | matches | `ITextFormatting` | [x] |
| 46 | IPainStateChecker injected in WcCounterSourceHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `IPainStateChecker` | [x] |
| 47 | IEngineVariables injected in WcCounterMessage (Pos) | code | Grep(Era.Core/Counter/WcCounterMessage.cs) | matches | `IEngineVariables` | [x] |
| 48 | IVariableStore injected in WcCounterSourceHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `IVariableStore` | [x] |
| 49 | IVariableStore injected in WcCounterMessage (Pos) | code | Grep(Era.Core/Counter/WcCounterMessage.cs) | matches | `IVariableStore` | [x] |
| 50 | ICounterUtilities injected in WcCounterSourceHandler (Pos) | code | Grep(Era.Core/Counter/WcCounterSourceHandler.cs) | matches | `ICounterUtilities` | [x] |
| 51 | IRandomProvider injected in WcCounterMessage (Pos) | code | Grep(Era.Core/Counter/WcCounterMessage.cs) | matches | `IRandomProvider` | [x] |
| 52 | DatuiMessage behavioral correctness test (Pos, C14-ext) | test | dotnet test --filter "FullyQualifiedName~WcCounterMessage.*DatuiMessageBehavior" | succeeds | - | [x] |
| 53 | INtrRevelationHandler defines Execute method (Pos) | code | Grep(Era.Core/Counter/INtrRevelationHandler.cs) | matches | `void Execute\(` | [x] |
| 54 | KojoMessageWcCounter two-parameter overload exists (Pos) | code | Grep(Era.Core/Counter/IKojoMessageService.cs) | matches | `KojoMessageWcCounter.*int.*int` | [x] |
| 55 | WcCounterMessage calls two-parameter KojoMessageWcCounter (Pos) | code | Grep(Era.Core/Counter/WcCounterMessage.cs) | matches | `KojoMessageWcCounter\(.*,` | [x] |
| 56 | WcCounterSourceHandler dispatch method coverage (Pos) | code | Grep(path="Era.Core/Counter/WcCounterSourceHandler.cs", pattern="private void (Handle\|Process\|Dispatch)[A-Z]") | gte | 55 | [x] |
| 57 | INtrRevelationHandler DI null sentinel registered (Pos) | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `INtrRevelationHandler.*null` | [x] |
| 58 | IWcCounterOutputHandler stub label removed (Pos) | code | Grep(Era.Core/Counter/IWcCounterOutputHandler.cs) | not_matches | `Stub` | [x] |

### AC Details

**AC#1: WcCounterSourceHandler implements IWcCounterSourceHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `class WcCounterSourceHandler.*IWcCounterSourceHandler`
- **Expected**: Pattern matches -- implementing class exists and declares interface implementation
- **Rationale**: C1 requires replacing the F811 stub with a full implementation. The class must implement the interface so SourceEntrySystem.cs line 249 call site works without modification. Currently no implementing class exists (RED state confirmed via Grep).

**AC#2: WcCounterMessage class exists (Pos)**
- **Test**: `Glob(Era.Core/Counter/WcCounterMessage.cs)`
- **Expected**: File exists
- **Rationale**: Philosophy requires each ERB file migrated to a dedicated C# class. TOILET_COUNTER_MESSAGE.ERB must become WcCounterMessage.cs, following the F802 CounterMessage.cs pattern. File does not exist currently (RED state confirmed via Glob).

**AC#3: ITouchStateManager injected in WcCounterSourceHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `ITouchStateManager`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: C2 mandates ITouchStateManager from F811 (not ITouchSet from F803). This is a deferred obligation from F811 mandatory handoffs. File does not exist yet (RED state).

**AC#4: ITouchSet NOT used in WcCounterSourceHandler (Neg)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `ITouchSet`
- **Expected**: Pattern NOT found -- WcCounterSourceHandler must not reference ITouchSet (only ITouchStateManager)
- **Rationale**: C2 negative verification. F803 uses ITouchSet but F805 must use ITouchStateManager per F811 mandate. ITouchStateManager does not contain the substring ITouchSet (ITouchSta... vs ITouchSe...), so no regex exclusion is needed — a plain ITouchSet pattern correctly detects any direct ITouchSet reference without false-matching ITouchStateManager. File does not exist yet, so this will be verified after creation.

**AC#5: ICounterSourceHandler delegation for DATUI helpers (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `ICounterSourceHandler`
- **Expected**: Pattern matches -- WcCounterSourceHandler delegates to F803's ICounterSourceHandler for DATUI_Bottom/Top/BottomT/TopT
- **Rationale**: C3 requires delegation to F803 interface (23 DATUI_* calls in ERB), not reimplementation. File does not exist yet (RED state).

**AC#6: HeteroSex added to ICommonFunctions (Pos)**
- **Test**: `Grep(Era.Core/Interfaces/ICommonFunctions.cs)` for pattern `HeteroSex`
- **Expected**: Pattern matches -- new method declaration exists
- **Rationale**: C12 requires adding HeteroSex to ICommonFunctions for FAVOR_CALC migration (TOILET_COUNTER_SOURCE.ERB:1219). Currently no HeteroSex method exists (RED state confirmed via Grep).

**AC#7: ICommonFunctions backward compatible -- method count (Pos)**
- **Test**: `Grep(path="Era.Core/Interfaces/ICommonFunctions.cs", pattern="\\(.*\\);")` with count_equals
- **Expected**: 16 (current baseline: 15 methods + 1 new HeteroSex). **Implementation-time dependent**: Pre-conditions require re-verifying baseline count before Task 1. If baseline differs from 15, update Expected to (actual baseline + 1). See Error Handling section.
- **Rationale**: C12 backward compatibility. Existing 15 methods must be preserved; only HeteroSex added. Current count is 15 (RED state: 15 != 16). Note: This AC is fragile to concurrent interface changes. The implementation-time re-baseline step in Pre-conditions and Error Handling mitigates staleness risk.

**AC#8: WcLoveRanking public interface method (Pos)**
- **Test**: `Grep(Era.Core/Counter/IWcCounterOutputHandler.cs)` for pattern `int.*WcLoveRanking`
- **Expected**: Pattern matches -- WC_LOVEranking exposed as public method returning int
- **Rationale**: C11 requires WC_LOVEranking (TOILET_COUNTER_MESSAGE.ERB:478-496) to be publicly exposed via interface because EVENTTURNEND.ERB:81 calls it externally. Currently no such method exists in IWcCounterOutputHandler (RED state confirmed via Grep).

**AC#9: F811 stub comment removed from interface (Pos)**
- **Test**: `Grep(Era.Core/Counter/IWcCounterSourceHandler.cs)` for pattern `stub`
- **Expected**: Pattern NOT found -- stub comment should be replaced with proper documentation after implementation
- **Rationale**: The interface currently contains "WC counter source handler interface stub" comment (confirmed via Grep: line 6). After F805 provides full implementation, the stub label must be removed. Currently matches (non-vacuous not_matches).

**AC#10: Early exit guard unit test (Pos, C10)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*EarlyExit"`
- **Expected**: Test passes -- verifies HandleWcCounterSource returns immediately with no side effects when decision flag > 1
- **Rationale**: C10 verifies TOILET_COUNTER_SOURCE.ERB:3-4 early exit guard (SIF TCVAR:ARG:CounterActionDecisionFlag > 1 RETURN 0). Boundary condition: no history rotation, no MCC updates when early-exiting.

**AC#11: CFLAG history rotation unit test (Pos, C9)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*HistoryRotation"`
- **Expected**: Test passes -- verifies two-step CFLAG rotation order (previous-previous before previous)
- **Rationale**: C9 verifies TOILET_COUNTER_SOURCE.ERB:9-10 rotation (WC_PreviousPreviousHarassment = WC_PreviousHarassment, WC_PreviousHarassment = current). Order matters: rotating in wrong order would lose the previous-previous value.

**AC#12: ForcedOrgasm floor of 2 unit test (Pos, C5)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*ForcedOrgasm.*Floor"`
- **Expected**: Test passes -- verifies minimum floor of 2 is enforced for forced orgasm calculation
- **Rationale**: C5 verifies TOILET_COUNTER_SOURCE.ERB:1408-1409 minimum enforcement. Values that would compute to less than 2 must be clamped to 2.

**AC#13: Zero-activity guard unit test (Pos, C4)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*ZeroActivity"`
- **Expected**: Test passes -- verifies accumulator tracking value (FLAG:好感度上昇の累積値) receives 0 when all SOURCE indices 0-99 are zero; CFLAG:好感度 is applied before the check and is NOT affected by the guard
- **Rationale**: C4 verifies TOILET_COUNTER_SOURCE.ERB:1349-1356 boundary: CFLAG:好感度 is incremented at line 1341 before the zero-activity check. The guard at lines 1349-1355 only zeroes 上昇予定値 for the accumulator (FLAG:好感度上昇の累積値 at line 1356), not the actual favor. Test must use an action code that activates the FAVOR_CALC path (any action that reaches TOILET_COUNTER_SOURCE.ERB:1184 without early-exiting), then assert: (1) IVariableStore writes 0 to FLAG:好感度上昇の累積値 (accumulator zeroed), and (2) IVariableStore increments CFLAG:好感度 (favor applied before the guard). Both assertions prevent trivial pass-through from early exit or mock defaults.

**AC#14: NTRrevelation no-op when handler absent (Neg, C7)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterMessage.*NtrRevelation.*NoOp"`
- **Expected**: Test passes -- calling message dispatch for MESSAGE13 without NTRrevelation handler does not throw
- **Rationale**: C7 verifies TRYCALL semantics at TOILET_COUNTER_MESSAGE.ERB:283. F808 (not yet implemented) provides NTRrevelation handler; F805 must gracefully no-op when absent. This is a negative test: no exception = PASS.

**AC#15: C# build succeeds**
- **Test**: `dotnet build Era.Core/`
- **Expected**: Build succeeds with exit code 0
- **Rationale**: All new files must compile without errors. TreatWarningsAsErrors is enabled in Directory.Build.props, so warnings also fail the build. This verifies type-safety and interface contract compliance across all new code.

**AC#16: WcCounterMessage implements IWcCounterOutputHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessage.cs)` for pattern `class WcCounterMessage.*IWcCounterOutputHandler`
- **Expected**: Pattern matches -- implementing class exists and declares interface implementation
- **Rationale**: Philosophy requires "each ERB counter file migrated to a dedicated C# class behind DI interfaces". AC#2 only checks file existence; AC#16 verifies actual interface implementation. Currently no implementing class exists (RED state).

**AC#17: MCC bit flag mapping unit test (Pos, C8)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*MccBit"`
- **Expected**: Test passes -- verifies representative MCC bit positions match ERB source values
- **Rationale**: C8 requires verification of SETBIT master counter control bit positions (TOILET_COUNTER_SOURCE.ERB:1039-1177, ~30 mappings). Test validates key bit constants (MccKissDisabled=2, MccHugDisabled=1, MccButtDisabled=0) against known ERB values.

**AC#18: Message dispatch branch vs non-branch modes unit test (Pos, C6)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterMessage.*DispatchBranch"`
- **Expected**: Test passes -- verifies both dispatch paths (branch mode iterates 1-4, non-branch dispatches single)
- **Rationale**: C6 requires testing both dispatch paths in TOILET_COUNTER_MESSAGE.ERB:19-25. Branch mode iterates messageVariant 1-4; non-branch dispatches a single message. Both paths must be independently tested.

**AC#19: MESSAGE result guard unit test (Pos, C13)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterMessage.*ResultGuard"`
- **Expected**: Test passes -- verifies PrintLine called only when message was actually printed (RESULT=1)
- **Rationale**: C13 requires verification of TOILET_COUNTER_MESSAGE.ERB:26-27 RESULT guard. PrintLine should only be called when a message handler returns nonzero. Prevents phantom line breaks.

**AC#20: IKojoMessageService used for kojo dispatch in WcCounterMessage (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessage.cs)` for pattern `IKojoMessageService`
- **Expected**: Pattern matches -- kojo dispatch uses injected IKojoMessageService interface
- **Rationale**: Technical Constraint requires "8 kojo calls must use IKojoMessageService or equivalent facade" (TOILET_COUNTER_MESSAGE.ERB:136-431). This verifies the injected interface is used rather than hardcoded dispatch, ensuring unit-testability of kojo calls.

**AC#21: DATUI delegation behavioral test (Pos, C3)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*DatuiDelegation"`
- **Expected**: Test passes -- verifies delegation for representative action codes covering all 4 DATUI types (DatUIBottom, DatUITop, DatUIBottomT, DatUITopT). For each type, a DATUI-triggering action code is dispatched through HandleWcCounterSource and the ICounterSourceHandler mock asserts the corresponding method is called
- **Rationale**: C3 requires 23 DATUI_* calls delegate to ICounterSourceHandler (not reimplementation). AC#5 only checks string presence. This behavioral test verifies actual delegation by mocking ICounterSourceHandler and asserting it receives DATUI calls. Testing all 4 types (Bottom/Top/BottomT/TopT) covers the qualitatively different delegation paths while following the F803 representative sampling precedent.

**AC#22: HeteroSex behavioral test (Pos, C12)**
- **Test**: `dotnet test --filter "FullyQualifiedName~CommonFunctions.*HeteroSex"`
- **Expected**: Test passes -- verifies HeteroSex returns 0 (heterosexual, male/female), 1 (same-sex lesbian, female/female), 2 (same-sex gay, male/male) mirroring COMMON.ERB:148-158
- **Rationale**: C12 states "Must test gender-comparison logic". AC#6 and AC#7 are grep-based (string presence and count). This behavioral test verifies the three return values used as index offsets in FAVOR_CALC (`HETEROSEX(ARG,MASTER) + 16`). Incorrect return values would silently corrupt favor calculations.

**AC#23: DATUI_MESSAGE sequence precondition test (Pos, C14)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterMessage.*DatuiMessageSequence"`
- **Expected**: Test passes -- verifies DatuiMessage is called before action-specific dispatch in SendMessage
- **Rationale**: C14 requires "DATUI_MESSAGE called before dispatch" (TOILET_COUNTER_MESSAGE.ERB:16). SendMessage must call DatuiMessage before any action message dispatch. A wrong call order would silently break undressing message sequencing.

**AC#24: HandleReaction behavioral test (Pos)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterMessage.*HandleReaction"`
- **ERB Source**: `TOILET_COUNTER.ERB` lines 53-56 (`IF CFLAG:加害者:うふふ == 1` → `CALL EVENT_WC_COUNTER_REACTION(加害者,行動順)` → `RETURN 0`); reaction logic in `TOILET_COUNTER_REACTION.ERB` lines 2-131 (`@EVENT_WC_COUNTER_REACTION(ARG,ARG:1)`)
- **Expected**: Test passes -- real WcCounterReaction instance (with mocked dependencies) receives delegation from HandleReaction with correct offenderId and actionOrder arguments when WcCounterMessage.HandleReaction is invoked
- **Rationale**: IWcCounterOutputHandler.HandleReaction is implemented in WcCounterMessage by delegating to the injected WcCounterReaction (F804 class), following the F802 CounterOutputHandler delegation pattern. Philosophy requires "unit-testable" for all migrated methods. AC#15 (build) only proves compilation, not behavioral correctness. The test constructs a real WcCounterReaction with mocked dependencies and verifies delegation with correct arguments.

**AC#25: HandlePunishment behavioral test (Pos)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterMessage.*HandlePunishment"`
- **ERB Source**: `TOILET_COUNTER.ERB` lines 57-69 (`ELSEIF CFLAG:加害者:うふふ == 2` → `IF 行動順 == 1 && CFLAG:加害者:MASTERの弱味 && BASE:加害者:満足 == MAXBASE:加害者:満足` → `CALL EVENT_WC_COUNTER_PUNISHMENT(加害者,行動順)` → all-character うふふ reset loop lines 63-67); punishment logic in `TOILET_COUNTER_PUNISHMENT.ERB` lines 2-45 (`@EVENT_WC_COUNTER_PUNISHMENT(ARG,ARG:1)`, with `WHILE LOCAL` dispatch to `WC_PUNISHMENT_0` through `WC_PUNISHMENT_9`)
- **Expected**: Test passes -- real WcCounterPunishment instance (with mocked dependencies) receives delegation from HandlePunishment with correct offenderId and actionOrder arguments when WcCounterMessage.HandlePunishment is invoked; the all-character うふふ CFLAG reset loop is internal to WcCounterPunishment (F804) and is not re-verified here
- **Rationale**: IWcCounterOutputHandler.HandlePunishment is implemented in WcCounterMessage by delegating to the injected WcCounterPunishment (F804 class), following the F802 CounterOutputHandler delegation pattern. Philosophy requires "unit-testable" for all migrated methods. AC#15 (build) only proves compilation, not behavioral correctness. The test constructs a real WcCounterPunishment with mocked dependencies and verifies delegation with correct arguments; the punishment execution logic (including the うふふ reset loop) is the responsibility of WcCounterPunishment and is tested within F804 scope.

**AC#26: WcLoveRanking behavioral test (Pos, C11)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterMessage.*WcLoveRanking"`
- **Expected**: Test passes -- verifies WcLoveRanking returns correct rank for representative character state
- **Rationale**: C11 requires WcLoveRanking exposed as public method returning rank value. AC#8 only checks interface declaration. The 19-line ranking logic (TOILET_COUNTER_MESSAGE.ERB:478-496) is non-trivial and called externally from EVENTTURNEND.ERB:81. Behavioral test verifies correct ranking output.

**AC#27: Reverse-rape path uses CheckPain not PainCheckVMaster (Pos, C15)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*ReversedRape.*CheckPain"`
- **Expected**: Test passes -- verifies the reverse-rape branch calls IPainStateChecker.CheckPain directly and does NOT call ICounterSourceHandler.PainCheckVMaster
- **Rationale**: Technical Constraint C15 specifies "Reverse-rape case must use IPainStateChecker.CheckPain directly, not PainCheckVMaster" (TOILET_COUNTER_SOURCE.ERB:930). Using the wrong pain-check variant silently corrupts game logic. Test mocks both IPainStateChecker and ICounterSourceHandler, triggers the reverse-rape action code, and asserts CheckPain called and PainCheckVMaster NOT called.

**AC#28: FAVOR_CALC HeteroSex integration test (Pos, C4/C12)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterSourceHandler.*FavorCalc.*HeteroSex"`
- **Expected**: Test passes -- verifies CalculateFavorIncrement produces correct favor amount for each HeteroSex return (0=hetero, 1=lesbian, 2=gay) via the `HETEROSEX(ARG,MASTER) + 16` index offset
- **Rationale**: AC#22 tests HeteroSex in isolation (correct return values 0/1/2). AC#13 tests zero-activity boundary. Neither covers the composition path where HeteroSex output feeds into FAVOR_CALC index arithmetic (TOILET_COUNTER_SOURCE.ERB:1219, offset `+16`). Wrong index silently corrupts favor calculations — the integration path must be tested with all 3 gender combinations to verify correct favor deltas.

**AC#29: IConsoleOutput injected in WcCounterMessage (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessage.cs)` for pattern `IConsoleOutput`
- **Expected**: Pattern matches -- IConsoleOutput dependency is injected in WcCounterMessage constructor
- **Rationale**: TOILET_COUNTER_MESSAGE.ERB contains PRINT statements (lines 26-27 RESULT guard triggers PRINTL). WcCounterMessage requires IConsoleOutput for text output. The constructor sketch explicitly includes it. Without this dependency, message output would fail silently or require an alternative output mechanism.

**AC#30: NTRrevelation invoked when handler provided (Pos, C7)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterMessage.*NtrRevelation.*Invoked"`
- **Expected**: Test passes -- verifies that when a non-null INtrRevelationHandler? is provided to WcCounterMessage constructor, MESSAGE13 dispatch invokes the handler
- **Rationale**: AC#14 tests the negative path (no-op when handler absent). AC#30 tests the positive path: when F808 provides a concrete INtrRevelationHandler? via DI, MESSAGE13 must invoke it. Without this, the NTRrevelation integration could silently remain a no-op even after F808 wires the handler. Tests the `_ntrRevelationHandler?.Execute()` call with a non-null implementation.

**AC#31: IShrinkageSystem injected in WcCounterSourceHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `IShrinkageSystem`
- **Expected**: Pattern matches -- IShrinkageSystem dependency is injected in WcCounterSourceHandler constructor
- **Rationale**: TOILET_COUNTER_SOURCE.ERB contains 25 calls to shrinkage-related functions (締り具合変動). Following F803 precedent (which has individual ACs for IShrinkageSystem injection), the WC counter source handler must inject this dependency for proper migration.

**AC#32: IVirginityManager injected in WcCounterSourceHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `IVirginityManager`
- **Expected**: Pattern matches -- IVirginityManager dependency is injected in WcCounterSourceHandler constructor
- **Rationale**: TOILET_COUNTER_SOURCE.ERB contains 12 LOST_VIRGIN calls. Following F803 precedent (which has individual ACs for IVirginityManager injection), the WC counter source handler must inject this dependency for proper migration.

**AC#33: IKojoMessageService.KojoMessageWcCounter called with correct characterIndex and actionType for kojo dispatch (Pos)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterMessage.*KojoDispatch"`
- **Expected**: Test passes -- verifies KojoMessageWcCounter is called with correct characterIndex AND actionType for a representative kojo-triggering action code
- **Rationale**: Technical Constraint requires "8 kojo calls must use IKojoMessageService" (TOILET_COUNTER_MESSAGE.ERB:136-431). AC#20 only verifies grep (string presence). This behavioral test mocks IKojoMessageService and asserts KojoMessageWcCounter receives the correct characterIndex and actionType when SendMessage dispatches a kojo-triggering action. Upstream Issues resolved: Option A selected (KojoMessageWcCounter(int characterIndex, int actionType) overload).

**AC#34: WcCounterSourceHandler registered in DI composition root (Pos)**
- **Test**: `Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs)` for pattern `IWcCounterSourceHandler.*WcCounterSourceHandler`
- **Expected**: Pattern matches -- DI registration line exists mapping interface to concrete class
- **Rationale**: Philosophy requires "behind DI interfaces". AC#1 verifies class declaration but not DI registration. Without registration in ServiceCollectionExtensions.cs, SourceEntrySystem.cs:249 call site would receive no implementation at runtime. Currently no registration exists (RED state).

**AC#35: WcCounterMessage registered in DI composition root (Pos)**
- **Test**: `Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs)` for pattern `IWcCounterOutputHandler.*WcCounterMessage`
- **Expected**: Pattern matches -- DI registration line exists mapping interface to concrete class
- **Rationale**: Philosophy requires "behind DI interfaces". AC#2/AC#16 verify class existence and interface implementation but not DI registration. Without registration, WcActionSelector and WcEventKojo (F804) would receive no implementation at runtime. Currently no registration exists (RED state).

**AC#36: Representative action-ID-to-variant dispatch routing test (Pos, C6-ext)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterMessage.*DispatchRouting"`
- **Expected**: Test passes -- verifies that a representative action ID is dispatched to the correct message variant number (N) that sibling features F806/F807/F808 will register against
- **Rationale**: AC#18 tests the branching mechanism (whether 1-4 iteration occurs vs single dispatch) but does not verify the routing table that maps action IDs to variant numbers. If the mapping is wrong, F806/F807/F808 handlers would never be invoked despite correct branch logic. This AC closes the gap between "dispatch mechanism exists" (AC#18) and "dispatch routes correctly to sibling features." Test mocks a variant-specific handler, dispatches a known action ID, and asserts the correct variant handler is called.

**AC#37: IEngineVariables injected in WcCounterSourceHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `IEngineVariables`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: Philosophy requires "constructor injection for all dependencies." IEngineVariables is in the constructor sketch but had no verifying AC. Consistent with AC#3/5/31/32 pattern.

**AC#38: ITEquipVariables injected in WcCounterSourceHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `ITEquipVariables`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: Philosophy requires "constructor injection for all dependencies." ITEquipVariables is in the constructor sketch but had no verifying AC. Consistent with AC#3/5/31/32 pattern.

**AC#39: ICharacterStringVariables injected in WcCounterSourceHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `ICharacterStringVariables`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: Philosophy requires "constructor injection for all dependencies." ICharacterStringVariables is used for CSTR writes (TOILET_COUNTER_SOURCE.ERB:931). Constructor sketch lists it explicitly. Consistent with AC#3/5/31/32 pattern.

**AC#40: IRandomProvider injected in WcCounterSourceHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `IRandomProvider`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: Philosophy requires "constructor injection for all dependencies." IRandomProvider is in the constructor sketch for random operations. Consistent with AC#3/5/31/32 pattern.

**AC#41: WcCounterReaction registered in DI composition root (Pos)**
- **Test**: `Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs)` for pattern `WcCounterReaction`
- **Expected**: Pattern matches -- DI registration exists for concrete class
- **Rationale**: WcCounterMessage constructor requires WcCounterReaction as a concrete constructor parameter (delegation pattern from F802). Without DI registration, IWcCounterOutputHandler resolution fails at runtime because the DI container cannot resolve the transitive WcCounterReaction dependency.

**AC#42: WcCounterPunishment registered in DI composition root (Pos)**
- **Test**: `Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs)` for pattern `WcCounterPunishment`
- **Expected**: Pattern matches -- DI registration exists for concrete class
- **Rationale**: WcCounterMessage constructor requires WcCounterPunishment as a concrete constructor parameter (delegation pattern from F802). Without DI registration, IWcCounterOutputHandler resolution fails at runtime because the DI container cannot resolve the transitive WcCounterPunishment dependency.

**AC#43: ICommonFunctions injected in WcCounterSourceHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `ICommonFunctions`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: Philosophy requires "constructor injection for all dependencies." ICommonFunctions is required for HeteroSex call in FAVOR_CALC (TOILET_COUNTER_SOURCE.ERB:1219). Consistent with AC#31-40 pattern for other constructor dependencies.

**AC#44: ITEquipVariables injected in WcCounterMessage (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessage.cs)` for pattern `ITEquipVariables`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: TOILET_COUNTER_MESSAGE.ERB:97 uses TEQUIP:MASTER:0. Following F802 CounterMessage.cs pattern which injects ITEquipVariables.

**AC#45: ITextFormatting injected in WcCounterMessage (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessage.cs)` for pattern `ITextFormatting`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: TOILET_COUNTER_MESSAGE.ERB:267/271 uses PANTS_DESCRIPTION/PANTSNAME. Following F802 CounterMessage.cs pattern which injects ITextFormatting.

**AC#46: IPainStateChecker injected in WcCounterSourceHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `IPainStateChecker`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: Required for C15 reverse-rape path (TOILET_COUNTER_SOURCE.ERB:930). Completes constructor dependency AC coverage for WcCounterSourceHandler.

**AC#47: IEngineVariables injected in WcCounterMessage (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessage.cs)` for pattern `IEngineVariables`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: Required for engine variable access in message dispatch. Following F802 CounterMessage.cs pattern. Completes constructor dependency AC coverage for WcCounterMessage.

**AC#48: IVariableStore injected in WcCounterSourceHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `IVariableStore`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: Primary data-access dependency for all CFLAG/FLAG/SOURCE operations. Completes constructor dependency AC coverage for WcCounterSourceHandler.

**AC#49: IVariableStore injected in WcCounterMessage (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessage.cs)` for pattern `IVariableStore`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: Primary data-access dependency for all CFLAG/FLAG operations. Completes constructor dependency AC coverage for WcCounterMessage.

**AC#50: ICounterUtilities injected in WcCounterSourceHandler (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `ICounterUtilities`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: TOILET_COUNTER_SOURCE.ERB uses EXP_UP at lines 967, 979, 995 (3 calls) mapping to ICounterUtilities.CheckExpUp. Required for experience gain logic migration.

**AC#51: IRandomProvider injected in WcCounterMessage (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessage.cs)` for pattern `IRandomProvider`
- **Expected**: Pattern matches in constructor or field declaration
- **Rationale**: TOILET_COUNTER_MESSAGE.ERB contains 13 PRINTDATA/PRINTDATAL constructs requiring random text selection. Following F804 WcCounterReaction/WcCounterPunishment pattern which both inject IRandomProvider for the same purpose.

**AC#52: DatuiMessage behavioral correctness test (Pos, C14-ext)**
- **Test**: `dotnet test --filter "FullyQualifiedName~WcCounterMessage.*DatuiMessageBehavior"`
- **Expected**: Test passes -- verifies WcCounterMessage.DatuiMessage produces correct IConsoleOutput.Print call for a representative TEQUIP state, matching ERB source (Game/ERB/COUNTER_MESSAGE.ERB:484) behavior
- **Rationale**: AC#23 tests call ordering (DatuiMessage before dispatch) but not output correctness. The DatuiMessage re-implementation in WcCounterMessage (DRY duplication from F802, tracked in Mandatory Handoffs to F813) could silently corrupt undressing messages if the logic diverges from the ERB source. This AC closes the behavioral gap.

**AC#53: INtrRevelationHandler defines Execute method (Pos)**
- **Test**: `Grep(Era.Core/Counter/INtrRevelationHandler.cs)` for pattern `void Execute(`
- **Expected**: Pattern matches — interface defines Execute() method for NTRrevelation dispatch
- **Rationale**: Key Decision mandates INtrRevelationHandler as the DI-standard replacement for Action? pattern. F808 implements this interface for NTRrevelation dispatch. File creation is a prerequisite for F808 integration. WcCounterMessage calls _ntrRevelationHandler?.Execute() (MESSAGE13 path). File existence alone does not verify the interface contract. F808 must implement Execute() — this AC ensures the contract is specified.

**AC#54: KojoMessageWcCounter two-parameter overload exists (Pos)**
- **Test**: `Grep(Era.Core/Counter/IKojoMessageService.cs)` for pattern `KojoMessageWcCounter.*int.*int`
- **Expected**: Pattern matches — two-parameter overload (characterIndex, actionType) exists in the interface
- **Rationale**: Upstream Issue resolution (Option A) requires adding actionType parameter to distinguish 6 distinct kojo dispatch types. Task 31 adds this overload. WcCounterMessage (Task 5) calls this overload via AC#55.

**AC#55: WcCounterMessage calls two-parameter KojoMessageWcCounter (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessage.cs)` for pattern `KojoMessageWcCounter\(.*,`
- **Expected**: Pattern matches — WcCounterMessage uses the two-parameter overload (not the single-parameter stub)
- **Rationale**: Ensures WcCounterMessage correctly passes actionType to IKojoMessageService for all 8 kojo dispatch calls, enabling proper action-type routing in the kojo system.

**AC#56: WcCounterSourceHandler dispatch method coverage (Pos)**
- **Test**: `Grep(Era.Core/Counter/WcCounterSourceHandler.cs)` for pattern `private void (Handle|Process|Dispatch)[A-Z]` with count_gte: 55
- **Expected**: count_gte: 55 (ERB baseline: 101 top-level CASE entries across two SELECTCASE blocks; after consolidation ~55-60 unique dispatch paths; 55 uses the lower bound of the ~55-60 consolidation estimate, allowing minimal consolidation margin while preventing significant branch omissions)
- **Rationale**: Philosophy claims "SSOT for all counter-related game logic." Without this AC, a WcCounterSourceHandler that omits 50 of ~60+ branches would pass all other ACs. This AC closes the coverage gap by verifying the total number of migrated dispatch methods meets the minimum acceptable floor relative to the ERB source branch count. The naming convention requirement (Handle*/Process*/Dispatch* prefix) distinguishes dispatch helpers from other private methods so the regex count reflects only SELECTCASE branch coverage. Implementer MUST use one of these prefixes for SELECTCASE branch helpers to distinguish them from non-dispatch methods like CalculateFavorIncrement and SetForcedOrgasmCount.

**AC#57: INtrRevelationHandler DI null sentinel registered (Pos)**
- **Test**: `Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs)` for pattern `INtrRevelationHandler.*null`
- **Expected**: Pattern matches — null sentinel registration (not concrete class) exists so WcCounterMessage can resolve from DI container without F808. Pattern includes `null` to distinguish the overridable null sentinel from a concrete NullNtrRevelationHandler class (which would break F808 DI override).
- **Rationale**: MS DI does not resolve unregistered nullable interface parameters as null. Without a null sentinel, AddSingleton<IWcCounterOutputHandler, WcCounterMessage>() throws InvalidOperationException when INtrRevelationHandler is absent. F808 overrides the null sentinel with a concrete implementation.

**AC#58: IWcCounterOutputHandler stub label removed (Pos)**
- **Test**: `Grep(Era.Core/Counter/IWcCounterOutputHandler.cs)` for pattern `Stub`
- **Expected**: Pattern does NOT match — "Stub" label removed from summary doc comment after Task 5 implements the interface.
- **Rationale**: Symmetry with AC#9 (IWcCounterSourceHandler.cs stub removal). Task 5 mandates removing the "Stub" label; this AC verifies the cleanup occurred.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Migrate TOILET_COUNTER_SOURCE.ERB to C# class implementing IWcCounterSourceHandler | AC#1, AC#10, AC#11, AC#12, AC#13, AC#17, AC#27, AC#28, AC#56 |
| 2 | Migrate TOILET_COUNTER_MESSAGE.ERB to C# class implementing IWcCounterOutputHandler (SendMessage, HandleReaction, HandlePunishment) | AC#2, AC#8, AC#14, AC#15, AC#16, AC#18, AC#19, AC#20, AC#23, AC#24, AC#25, AC#26, AC#29, AC#30, AC#33, AC#36, AC#44, AC#45, AC#47, AC#49, AC#51, AC#52, AC#54, AC#55, AC#58 |
| 3 | Use established F803/F802 DI patterns (AC#34/35/41/42: F805 own-class DI registration; AC#53: INtrRevelationHandler interface creation only — its DI registration deferred to F808 per Mandatory Handoffs) | AC#3, AC#5, AC#21, AC#31, AC#32, AC#34, AC#35, AC#37, AC#38, AC#39, AC#40, AC#41, AC#42, AC#43, AC#46, AC#48, AC#50, AC#53, AC#57 |
| 4 | Add missing HeteroSex method to ICommonFunctions | AC#6, AC#7, AC#22 |
| 5 | Expose WC_LOVEranking as public interface method | AC#8, AC#26 |
| 6 | Replace F811 stub with full implementation injecting ITouchStateManager | AC#1, AC#3, AC#4, AC#9 |
| 7 | All new code compiles without errors or warnings | AC#15 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

F805 produces four coordinated changes following the F803/F802 DI patterns:

1. **WcCounterSourceHandler.cs** (new file, `Era.Core/Counter/`) -- implements `IWcCounterSourceHandler.HandleWcCounterSource`. Mirrors `CounterSourceHandler.cs` structure: `sealed class`, constructor injection of all dependencies, `private void` dispatch helpers for each SELECTCASE branch. Injects `ITouchStateManager` (not `ITouchSet`) per F811 mandate, and `ICounterSourceHandler` for DATUI_* delegation. Contains the `WC_COUNTER_FAVOR_CALC` subsystem as a `private int CalculateFavorIncrement(CharacterId target)` method and `WC_COUNTER_COUNTorForcedOrgasm` as `private void SetForcedOrgasmCount(CharacterId target, int bodyPart)`.

2. **WcCounterMessage.cs** (new file, `Era.Core/Counter/`) -- implements `IWcCounterOutputHandler` (`SendMessage`, `HandleReaction`, `HandlePunishment`) and provides `WcLoveRanking`. Mirrors `CounterMessage.cs` structure: primary `SendMessage` calls `DatuiMessage` then dispatches via `Dispatch`/`DispatchWithBranch` switch. The 8 kojo calls use `IKojoMessageService.KojoMessageWcCounter`. `HandleReaction` delegates to the injected `WcCounterReaction` (F804) and `HandlePunishment` delegates to the injected `WcCounterPunishment` (F804), following the F802 CounterOutputHandler delegation pattern. NTRrevelation handler is injected as `INtrRevelationHandler? ntrRevelationHandler` to enable standard DI resolution. Null sentinel registration in ServiceCollectionExtensions.cs ensures DI resolution succeeds before F808 provides the implementation. F808 registers the concrete implementation, overriding the null sentinel.

3. **ICommonFunctions extension** -- add `int HeteroSex(int targetGender, int masterGender)` method. Returns 0 (heterosexual), 1 (same-sex lesbian), 2 (same-sex gay), mirroring `COMMON.ERB:148-158`.

4. **IWcCounterOutputHandler extension** -- add `int WcLoveRanking(int characterIndex)` method to expose the `WC_LOVEranking` FUNCTION called externally from EVENTTURNEND.ERB:81.

5. **IWcCounterSourceHandler cleanup** -- remove "stub" comment, replace with proper documentation.

This approach satisfies all 58 ACs: code/file ACs (AC#1-9,16,20,29,31,32,34,35,37-58) by static grep/glob on the new files; test ACs (AC#10-14,17-19,21-28,30,33,36,52) by unit tests in `Era.Core.Tests/Counter/` that mock all injected dependencies; build AC (AC#15) by the existing `TreatWarningsAsErrors` build gate.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core/Counter/WcCounterSourceHandler.cs` with `public sealed class WcCounterSourceHandler : IWcCounterSourceHandler`. Grep matches `class WcCounterSourceHandler.*IWcCounterSourceHandler`. |
| 2 | Create `Era.Core/Counter/WcCounterMessage.cs`. Glob confirms file exists. |
| 3 | Declare `private readonly ITouchStateManager _touchStateManager;` in `WcCounterSourceHandler` constructor. Grep matches `ITouchStateManager`. |
| 4 | Do not reference `ITouchSet` anywhere in `WcCounterSourceHandler.cs`. Grep pattern `ITouchSet` finds no match (ITouchStateManager does not contain the substring ITouchSet, so no false-positive risk). |
| 5 | Declare `private readonly ICounterSourceHandler _counterSourceHandler;` in `WcCounterSourceHandler` constructor for DATUI_* delegation. Grep matches `ICounterSourceHandler`. |
| 6 | Add `int HeteroSex(int targetGender, int masterGender);` to `ICommonFunctions.cs`. Grep matches `HeteroSex`. |
| 7 | Only add HeteroSex (1 new method), preserving existing 15. Total = 16. Count of lines matching `\(.*\);` = 16. |
| 8 | Add `int WcLoveRanking(int characterIndex);` to `IWcCounterOutputHandler.cs`. Grep matches `int.*WcLoveRanking`. |
| 9 | Edit `IWcCounterSourceHandler.cs`: replace `/// <summary>WC counter source handler interface stub.` with proper doc comment that does not contain "stub". |
| 10 | Unit test `WcCounterSourceHandlerTests.HandleWcCounterSource_EarlyExit_WhenDecisionFlagGreaterThanOne`: mock `IVariableStore.GetTCVar` to return `decisionFlag=2`; assert no further calls made (no side effects). Test class name contains "WcCounterSourceHandler" and method contains "EarlyExit". |
| 11 | Unit test `WcCounterSourceHandlerTests.HandleWcCounterSource_HistoryRotation_UpdatesPreviousPreviousBeforePrevious`: set `WC_前回セクハラ=5`; after call, verify `WC_前々回セクハラ=5` and `WC_前回セクハラ=currentAction`. Order asserted: previous-previous set from previous before previous overwritten. |
| 12 | Unit test `WcCounterSourceHandlerTests.SetForcedOrgasmCount_ForcedOrgasm_Floor_EnforcesMinimumOfTwo`: supply inputs that would compute count < 2; assert result clamped to 2 for each of the 5 body-part cases. |
| 13 | Unit test `WcCounterSourceHandlerTests.CalculateFavorIncrement_ZeroActivity_ReturnsZeroWhenAllSourceIndicesZero`: use an action code that activates FAVOR_CALC path (any non-early-exit action reaching line 1184); all SOURCE[0..99] = 0; assert (1) accumulator tracking value (FLAG:好感度上昇の累積値) receives 0, and (2) CFLAG:好感度 is incremented (favor applied before guard). Tested via the public `HandleWcCounterSource` path. |
| 14 | Unit test `WcCounterMessageTests.SendMessage_NtrRevelation_NoOp_WhenHandlerAbsent`: construct `WcCounterMessage` without NTRrevelation handler; dispatch action corresponding to MESSAGE13; assert no exception thrown. |
| 15 | Build `Era.Core/` with `dotnet build Era.Core/`. All new files compile. `TreatWarningsAsErrors` enforced by `Directory.Build.props`. |
| 16 | Create `Era.Core/Counter/WcCounterMessage.cs` with `public sealed class WcCounterMessage : IWcCounterOutputHandler`. Grep matches `class WcCounterMessage.*IWcCounterOutputHandler`. |
| 17 | Unit test in `Era.Core.Tests/Counter/WcCounterSourceHandlerTests.cs`: method `MccBitMapping_VerifiesBitPositionsMatchErbSource` verifies representative bit positions from MCC constants. |
| 18 | Unit test in `Era.Core.Tests/Counter/WcCounterMessageTests.cs`: method `DispatchBranch_TestsBothBranchAndNonBranchModes` verifies both branch and non-branch dispatch paths. |
| 19 | Unit test in `Era.Core.Tests/Counter/WcCounterMessageTests.cs`: method `ResultGuard_PrintLineOnlyWhenResultNonZero` verifies PrintLine guard behavior. |
| 20 | Declare `private readonly IKojoMessageService _kojoMessage` in `WcCounterMessage` constructor. Grep matches `IKojoMessageService`. |
| 21 | Unit test in `Era.Core.Tests/Counter/WcCounterSourceHandlerTests.cs`: method `DatuiDelegation_DelegatesToCounterSourceHandler` verifies mock ICounterSourceHandler receives delegation calls for all 4 DATUI types (DatUIBottom, DatUITop, DatUIBottomT, DatUITopT) via representative action codes. |
| 22 | Unit test in `Era.Core.Tests/CommonFunctionsTests.cs` (or appropriate test file): method `HeteroSex_ReturnsCorrectGenderComparisonValues` verifies return values 0/1/2 for hetero/lesbian/gay pairs. |
| 23 | Unit test in `Era.Core.Tests/Counter/WcCounterMessageTests.cs`: method `DatuiMessageSequence_CalledBeforeDispatch` verifies DatuiMessage mock is called before action dispatch mock. |
| 24 | Unit test in `Era.Core.Tests/Counter/WcCounterMessageTests.cs`: method `HandleReaction_DelegatesToExpectedBehavior` verifies reaction handling for representative input. |
| 25 | Unit test in `Era.Core.Tests/Counter/WcCounterMessageTests.cs`: method `HandlePunishment_DelegatesToExpectedBehavior` verifies punishment handling for representative input. |
| 26 | Unit test in `Era.Core.Tests/Counter/WcCounterMessageTests.cs`: method `WcLoveRanking_ReturnsCorrectRankForGivenCharacterState` verifies ranking logic for representative inputs. |
| 27 | Unit test in `Era.Core.Tests/Counter/WcCounterSourceHandlerTests.cs`: method `ReversedRapePath_UsesCheckPainNotPainCheckVMaster` mocks IPainStateChecker and ICounterSourceHandler; triggers reverse-rape action; asserts CheckPain called and PainCheckVMaster NOT called. |
| 28 | Unit test in `Era.Core.Tests/Counter/WcCounterSourceHandlerTests.cs`: method `FavorCalc_HeteroSexIntegration_CorrectFavorForEachGenderCombination` verifies CalculateFavorIncrement produces correct favor via `HETEROSEX(ARG,MASTER) + 16` index offset for hetero (0), lesbian (1), gay (2) gender combinations. |
| 29 | Verify `IConsoleOutput` injection in `WcCounterMessage.cs` constructor. Grep matches `IConsoleOutput`. |
| 30 | Unit test in `Era.Core.Tests/Counter/WcCounterMessageTests.cs`: method `SendMessage_NtrRevelation_Invoked_WhenHandlerProvided` verifies that MESSAGE13 dispatch calls `INtrRevelationHandler.Execute()`. |
| 31 | Declare `private readonly IShrinkageSystem _shrinkageSystem` in `WcCounterSourceHandler` constructor. Grep matches `IShrinkageSystem`. |
| 32 | Declare `private readonly IVirginityManager _virginityManager` in `WcCounterSourceHandler` constructor. Grep matches `IVirginityManager`. |
| 33 | Unit test in `Era.Core.Tests/Counter/WcCounterMessageTests.cs`: method `KojoDispatch_CallsKojoMessageWcCounter_WithCorrectCharacterIndexAndActionType` mocks IKojoMessageService and verifies KojoMessageWcCounter called with correct characterIndex AND actionType for representative kojo action. |
| 34 | Add `services.AddSingleton<IWcCounterSourceHandler, WcCounterSourceHandler>();` to `ServiceCollectionExtensions.cs` Phase 21 Counter System section. Grep matches `IWcCounterSourceHandler.*WcCounterSourceHandler`. |
| 35 | Add `services.AddSingleton<IWcCounterOutputHandler, WcCounterMessage>();` to `ServiceCollectionExtensions.cs` Phase 21 Counter System section. Grep matches `IWcCounterOutputHandler.*WcCounterMessage`. |
| 36 | Unit test in WcCounterMessageTests.cs: DispatchRouting_MapsActionIdToCorrectVariant verifies representative action-ID-to-variant mapping |
| 37 | Grep verification of IEngineVariables injection in WcCounterSourceHandler.cs |
| 38 | Grep verification of ITEquipVariables injection in WcCounterSourceHandler.cs |
| 39 | Grep verification of ICharacterStringVariables injection in WcCounterSourceHandler.cs |
| 40 | Grep verification of IRandomProvider injection in WcCounterSourceHandler.cs |
| 41 | Add `services.AddSingleton<WcCounterReaction>();` (or equivalent concrete registration) to `ServiceCollectionExtensions.cs` Phase 21 Counter System section. Grep matches `WcCounterReaction`. |
| 42 | Add `services.AddSingleton<WcCounterPunishment>();` (or equivalent concrete registration) to `ServiceCollectionExtensions.cs` Phase 21 Counter System section. Grep matches `WcCounterPunishment`. |
| 43 | Grep verification of ICommonFunctions injection in WcCounterSourceHandler.cs |
| 44 | Declare `private readonly ITEquipVariables _tEquipVariables;` in `WcCounterMessage` constructor. Grep matches `ITEquipVariables`. |
| 45 | Declare `private readonly ITextFormatting _textFormatting;` in `WcCounterMessage` constructor. Grep matches `ITextFormatting`. |
| 46 | Grep verification of IPainStateChecker injection in WcCounterSourceHandler.cs |
| 47 | Grep verification of IEngineVariables injection in WcCounterMessage.cs |
| 48 | Grep verification of IVariableStore injection in WcCounterSourceHandler.cs |
| 49 | Grep verification of IVariableStore injection in WcCounterMessage.cs |
| 50 | Grep verification of ICounterUtilities injection in WcCounterSourceHandler.cs |
| 51 | Grep verification of IRandomProvider injection in WcCounterMessage.cs |
| 52 | Unit test verifying DatuiMessage output matches ERB source behavior |
| 53 | Create Era.Core/Counter/INtrRevelationHandler.cs defining the NTRrevelation dispatch interface for MESSAGE13. F808 provides implementation. |
| 54 | Add void KojoMessageWcCounter(int characterIndex, int actionType) overload to IKojoMessageService.cs |
| 55 | Verify WcCounterMessage.cs uses the two-parameter KojoMessageWcCounter overload (not the single-parameter stub) |
| 56 | Grep WcCounterSourceHandler.cs for private method count; count_gte 55. Verifies all SELECTCASE branches migrated as private dispatch helpers (ERB baseline: 101 CASE entries → ~55-60 after consolidation) |
| 57 | Grep ServiceCollectionExtensions.cs for INtrRevelationHandler.*null pattern. Verifies null sentinel (not concrete class) registered so WcCounterMessage resolves from DI before F808 |
| 58 | Grep IWcCounterOutputHandler.cs for 'Stub'; must NOT match. Verifies stub label removed after Task 5 implements the interface (symmetry with AC#9) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| NTRrevelation TRYCALL implementation | A: nullable `Action?` field; B: nullable interface `INtrRevelationHandler?`; C: no-op default implementation via optional interface | A: `INtrRevelationHandler?` interface | DI-standard pattern: INtrRevelationHandler is DI-resolvable via standard AddSingleton. Requires explicit null sentinel registration (`AddSingleton<INtrRevelationHandler>(sp => null!)`) in Task 26 to avoid InvalidOperationException when F808 is absent. F808 registers the concrete implementation, overriding the null sentinel. Null-safe (optional DI registration = no-op, matching TRYCALL semantics). |
| FAVOR_CALC / ForcedOrgasm placement | A: in WcCounterSourceHandler as private methods; B: in a separate helper class | A: private methods in WcCounterSourceHandler | F803 pattern keeps all source logic in one class; these are not shared with other classes; separate class adds indirection without benefit for this scope |
| WcLoveRanking interface placement | A: in IWcCounterOutputHandler (F804); B: new dedicated IWcCounterQueryService | A: add to IWcCounterOutputHandler | Minimal interface change; EVENTTURNEND.ERB calls it in the same WC counter context; consistent with F804 scope. ISP exception: WcLoveRanking (query returning int) mixed with void side-effect methods violates Interface Segregation, but only one production class (WcCounterMessage) implements IWcCounterOutputHandler, plus two test implementors (WcKojoTestOutputHandler, WcSelectorTestOutputHandler) which require `WcLoveRanking => throw new NotImplementedException()` stubs (handled in Task 2). Separating into IWcLoveRankingService adds interface overhead for zero polymorphism benefit at current scope. |
| HeteroSex return type | A: `int` (0/1/2); B: `bool` (same-sex yes/no); C: enum | A: `int` | Direct translation of HETEROSEX ERB FUNCTION which returns 0, 1, or 2 used as both a boolean guard and an index offset (`HETEROSEX(ARG,MASTER) + 16`) |
| CFLAG constant naming | A: raw int constants in WcCounterSourceHandler; B: new WcCFlagIndex typed wrapper | A: raw int private const fields (same as CounterSourceHandler pattern) | F803 uses `private const int CFlagXxx = N` pattern; WC constants use the same approach for consistency |
| Test visibility for CalculateFavorIncrement | A: internal + InternalsVisibleTo; B: test via public HandleWcCounterSource path | B: test via public HandleWcCounterSource path | Avoids exposing internals; unit test can mock IVariableStore SOURCE reads and verify the behavioral outcome (no favor applied to CFLAG); simpler test setup |
| WcLoveRanking parameter type | A: `CharacterId` (type-safe, consistent with other IWcCounterOutputHandler methods); B: `int` (raw index, matching ERB caller convention) | B: `int characterIndex` | WC_LOVEranking is called externally from EVENTTURNEND.ERB:81 which passes `奴隷` (raw ERB character array index) with no CharacterId wrapper. No CharacterId resolution path exists at the ERB call site. Using CharacterId would require an ERB→C# CharacterId conversion that is outside F805 scope. The Philosophy Derivation "type-safe" row acknowledges this boundary; ERB-facing APIs use raw int until full ERB migration. |
| WcCounterMessage responsibility scope | A: Split into WcCounterMessage (message dispatch) + WcCounterOutputHandler (reaction/punishment), mirroring F802 pattern; B: Single class implementing all IWcCounterOutputHandler methods | B: Single class WcCounterMessage | IWcCounterOutputHandler was defined by F804 with all methods bundled (SendMessage, HandleReaction, HandlePunishment); splitting would require interface change outside F805 scope. WC counter reaction/punishment logic is simpler than main counter, making separation overhead unjustified. |
| HandleReaction/HandlePunishment implementation | A: delegate to F804's WcCounterReaction/WcCounterPunishment via constructor injection; B: reimplement logic inline in WcCounterMessage | A: delegate to WcCounterReaction and WcCounterPunishment | F802 CounterOutputHandler delegates to CounterReaction/CounterPunishment (established pattern). WcCounterReaction (342 lines, 5 pose branches) and WcCounterPunishment (120 lines, random variant dispatch) are fully implemented by F804. Inline reimplementation would violate DRY with 462 lines of duplicated code. Delegation keeps WcCounterMessage focused on message dispatch (SendMessage) while reaction/punishment are separate concerns. |
| WcCounterReaction/WcCounterPunishment test pattern | A: mock via interfaces (requires new IWcCounterReaction/IWcCounterPunishment); B: real instances with mocked dependencies (F802 pattern) | B: real instances with mocked dependencies | F802 CounterOutputHandler uses identical pattern: injects concrete sealed CounterReaction/CounterPunishment and tests with `new CounterReaction(mockVars, mockUtils, ...)`. Sealed classes cannot be mocked by standard .NET frameworks. Creating interfaces solely for testability adds overhead for zero polymorphism benefit. Test coverage is equivalent: verifying delegation through the real class with controlled dependencies. |

### Interfaces / Data Structures

**ICommonFunctions addition** (Era.Core/Interfaces/ICommonFunctions.cs):
```csharp
// Gender Comparison Functions
/// <summary>
/// Determines same-sex relationship between two characters.
/// Returns 0 (heterosexual), 1 (same-sex lesbian), 2 (same-sex gay).
/// Mirrors COMMON.ERB:148-158 HETEROSEX function.
/// </summary>
int HeteroSex(int targetGender, int masterGender);
```

**IWcCounterOutputHandler addition** (Era.Core/Counter/IWcCounterOutputHandler.cs):
```csharp
/// <summary>
/// Calculates love ranking for a character among all current characters.
/// Migrates WC_LOVEranking (TOILET_COUNTER_MESSAGE.ERB:478-496).
/// Called externally from EVENTTURNEND.ERB:81.
/// </summary>
int WcLoveRanking(int characterIndex);
```

**WcCounterSourceHandler constructor signature** (sketch):
```csharp
public WcCounterSourceHandler(
    IVariableStore variableStore,
    IEngineVariables engineVariables,
    ITEquipVariables tEquipVariables,
    ICharacterStringVariables characterStringVariables,  // CSTR writes (e.g. line 931)
    ITouchStateManager touchStateManager,        // F811: not ITouchSet
    ICounterSourceHandler counterSourceHandler,  // F803: DATUI_* delegation
    ICommonFunctions commonFunctions,
    IPainStateChecker painStateChecker,
    IShrinkageSystem shrinkageSystem,            // 締り具合変動 (25 calls)
    IVirginityManager virginityManager,          // LOST_VIRGIN (12 calls)
    IRandomProvider randomProvider,
    ICounterUtilities counterUtilities)          // EXP_UP (lines 967, 979, 995 → CheckExpUp)
```

**WcCounterMessage constructor signature** (sketch):
```csharp
public sealed class WcCounterMessage(
    IVariableStore variables,
    IEngineVariables engine,
    ITEquipVariables tEquipVariables,
    ITextFormatting textFormatting,
    IConsoleOutput console,
    IKojoMessageService kojoMessage,
    IRandomProvider randomProvider,           // PRINTDATA random text selection (13 calls)
    WcCounterReaction reaction,
    WcCounterPunishment punishment,
    INtrRevelationHandler? ntrRevelationHandler = null)
    : IWcCounterOutputHandler
```

**MCC bit constants in WcCounterSourceHandler**:
```csharp
// TCVAR:26 WC master counter control bits (TOILET_COUNTER_SOURCE.ERB:1040)
private const int MccKissDisabled    = 2;
private const int MccHugDisabled     = 1;
private const int MccButtDisabled    = 0;
private const int MccBreastDisabled  = 3;
private const int MccAnalDisabled    = 4;
private const int MccClitDisabled    = 5;
private const int MccFingerDisabled  = 6;
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#7 expected count (16) assumed correct at design time but must be verified post-HeteroSex addition. If any other ICommonFunctions change lands before F805 Task execution, count becomes stale. | AC Definition Table (AC#7) | Re-run baseline grep at implementation time; adjust Expected if count differs from 16 |
| IWcCounterOutputHandler (F804) must gain `WcLoveRanking` -- this modifies an F804-defined interface. AC#8 verifies presence in that file. Two test classes already implement IWcCounterOutputHandler: `WcKojoTestOutputHandler` (Era.Core.Tests/Counter/WcEventKojoTests.cs) and `WcSelectorTestOutputHandler` (Era.Core.Tests/Counter/WcActionSelectorTests.cs). Both will fail to compile until they add the method. | Technical Constraints | Task 2 must add `int WcLoveRanking(int characterIndex) => throw new NotImplementedException();` stub to both WcKojoTestOutputHandler and WcSelectorTestOutputHandler |
| ICommonFunctions (Task 1) must gain HeteroSex -- ~16 test classes in Era.Core.Tests/ directly implement ICommonFunctions (CharacterCustomizerTests, ConsoleOutputDelegationTests, CounterOutputHandlerTests, CounterSourceHandlerTests, etc.) and will fail to compile until they add the method | AC Definition Table (AC#6, AC#7) | **RESOLVED: Approach A (DIM) with `throw`** — add `int HeteroSex(int targetGender, int masterGender) => throw new NotImplementedException(nameof(HeteroSex));` as DIM. Consistent with WcLoveRanking stub pattern. `throw` ensures unexpected calls fail loudly in tests (not silent `0` = heterosexual). |
| IWcCounterOutputHandler.cs:13 comment documents `messageVariant: 1 for 乳搾り手コキ branch` only. Actual branch mode iterates variants 1-4 per C6 constraint and AC#18. Comment is misleading but F804 scope. | IWcCounterOutputHandler.cs comment (F804) | Task 5 implementer should update the comment to document `messageVariant: null for default call, 1-4 for branch mode iteration` during WcCounterMessage implementation |
| IKojoMessageService.KojoMessageWcCounter(int) has insufficient signature for F805's 6 distinct kojo dispatch types (KOJO_WC_COUNTER10, KOJO_WC_COUNTER12, KOJO_WC_COUNTER15, KOJO_WC_COUNTER16, KOJO_WC_COUNTER_com350, KOJO_WC_COUNTER_NTRrevelation_0 with TCVAR:20 arg). Current method only takes characterIndex. **RESOLVED: Option A selected** — extend IKojoMessageService with `KojoMessageWcCounter(int characterIndex, int actionType)` overload. AC#33 updated to verify both characterIndex and actionType. | Technical Constraints (KOJO TRYCALLFORM dispatch), AC#20, AC#33 | Option A: extend IKojoMessageService with action-type parameter overload `KojoMessageWcCounter(int characterIndex, int actionType)`. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 6, 7 | Add `int HeteroSex(int targetGender, int masterGender)` to ICommonFunctions.cs; implement in CommonFunctions.cs mirroring COMMON.ERB:148-158; add DIM `int HeteroSex(int targetGender, int masterGender) => throw new NotImplementedException(nameof(HeteroSex));` for backward compatibility (~16 test classes, consistent with WcLoveRanking stub pattern) | | [x] |
| 2 | 8 | Add `int WcLoveRanking(int characterIndex)` to IWcCounterOutputHandler.cs; add stub implementation to WcKojoTestOutputHandler (WcEventKojoTests.cs) and WcSelectorTestOutputHandler (WcActionSelectorTests.cs) | | [x] |
| 3 | 9 | Remove "stub" comment from IWcCounterSourceHandler.cs; replace with proper doc comment | | [x] |
| 4 | 1, 3, 4, 5, 31, 32, 37, 38, 39, 40, 43, 46, 48, 50, 56 | Create Era.Core/Counter/WcCounterSourceHandler.cs implementing IWcCounterSourceHandler with ITouchStateManager and ICounterSourceHandler injection. SELECTCASE branch helpers MUST use Handle*/Process*/Dispatch* prefix naming convention (required for AC#56 verification) | | [x] |
| 5 | 2, 16, 20, 29, 44, 45, 47, 49, 51, 55, 58 | Create Era.Core/Counter/WcCounterMessage.cs implementing IWcCounterOutputHandler with WcLoveRanking; delegates HandleReaction to WcCounterReaction and HandlePunishment to WcCounterPunishment (F804); follows F802 CounterOutputHandler delegation pattern. Also: update IWcCounterOutputHandler.cs:13 comment (messageVariant supports 1-4 for branch mode, not just 1), and remove "Stub" label from IWcCounterOutputHandler.cs summary doc comment | | [x] |
| 6 | 10 | Write unit test WcCounterSourceHandlerTests.HandleWcCounterSource_EarlyExit_WhenDecisionFlagGreaterThanOne | | [x] |
| 7 | 11 | Write unit test WcCounterSourceHandlerTests.HandleWcCounterSource_HistoryRotation_UpdatesPreviousPreviousBeforePrevious | | [x] |
| 8 | 12 | Write unit test WcCounterSourceHandlerTests.SetForcedOrgasmCount_ForcedOrgasm_Floor_EnforcesMinimumOfTwo | | [x] |
| 9 | 13 | Write unit test WcCounterSourceHandlerTests.CalculateFavorIncrement_ZeroActivity_ReturnsZeroWhenAllSourceIndicesZero | | [x] |
| 10 | 14 | Write unit test WcCounterMessageTests.SendMessage_NtrRevelation_NoOp_WhenHandlerAbsent | | [x] |
| 11 | 15 | Verify C# build passes for Era.Core/ | | [x] |
| 12 | 17 | Write unit test WcCounterSourceHandlerTests.MccBitMapping_VerifiesBitPositionsMatchErbSource | | [x] |
| 13 | 18 | Write unit test WcCounterMessageTests.DispatchBranch_TestsBothBranchAndNonBranchModes | | [x] |
| 14 | 19 | Write unit test WcCounterMessageTests.ResultGuard_PrintLineOnlyWhenResultNonZero | | [x] |
| 15 | 20 | Verify IKojoMessageService injection in WcCounterMessage.cs (verification-only: AC#20 grep runs after Task 5 creates the file) | | [x] |
| 16 | 21 | Write unit test WcCounterSourceHandlerTests.DatuiDelegation_DelegatesToCounterSourceHandler | | [x] |
| 17 | 22 | Write unit test CommonFunctionsTests.HeteroSex_ReturnsCorrectGenderComparisonValues | | [x] |
| 18 | 23 | Write unit test WcCounterMessageTests.DatuiMessageSequence_CalledBeforeDispatch | | [x] |
| 19 | 24 | Write unit test WcCounterMessageTests.HandleReaction_DelegatesToExpectedBehavior | | [x] |
| 20 | 25 | Write unit test WcCounterMessageTests.HandlePunishment_DelegatesToExpectedBehavior | | [x] |
| 21 | 26 | Write unit test WcCounterMessageTests.WcLoveRanking_ReturnsCorrectRankForGivenCharacterState | | [x] |
| 22 | 27 | Write unit test WcCounterSourceHandlerTests.ReversedRapePath_UsesCheckPainNotPainCheckVMaster | | [x] |
| 23 | 28 | Write unit test WcCounterSourceHandlerTests.FavorCalc_HeteroSexIntegration_CorrectFavorForEachGenderCombination | | [x] |
| 24 | 30 | Write unit test WcCounterMessageTests.SendMessage_NtrRevelation_Invoked_WhenHandlerProvided | | [x] |
| 25 | 33 | Write unit test WcCounterMessageTests.KojoDispatch_CallsKojoMessageWcCounter_WithCorrectCharacterIndexAndActionType: verify Mock.Verify asserts both characterIndex and actionType arguments | | [x] |
| 26 | 34, 35, 41, 42, 57 | Add DI registrations for IWcCounterSourceHandler → WcCounterSourceHandler, IWcCounterOutputHandler → WcCounterMessage, WcCounterReaction (concrete), and WcCounterPunishment (concrete) in ServiceCollectionExtensions.cs; includes INtrRevelationHandler null sentinel registration (AddSingleton<INtrRevelationHandler>(sp => null!)) for pre-F808 DI resolution | | [x] |
| 27 | 36 | Write unit test WcCounterMessageTests.DispatchRouting_MapsActionIdToCorrectVariant | [I] | [x] |
| 28 | 37, 38, 39, 40, 43, 46, 48, 50 | Verify remaining constructor dependencies (IEngineVariables, ITEquipVariables, ICharacterStringVariables, IRandomProvider, ICommonFunctions, IPainStateChecker, IVariableStore, ICounterUtilities) in WcCounterSourceHandler.cs (verification-only: AC#37-40, AC#43, AC#46, AC#48, AC#50 grep runs after Task 4 creates the file) | | [x] |
| 29 | 52 | Write unit test WcCounterMessageTests.DatuiMessageBehavior_MatchesErbSourceOutput verifying DatuiMessage output matches ERB source behavior | | [x] |
| 30 | 53 | Create Era.Core/Counter/INtrRevelationHandler.cs defining NTRrevelation dispatch interface for MESSAGE13; F808 implements this interface | | [x] |
| 31 | 54 | Add KojoMessageWcCounter(int characterIndex, int actionType) overload to IKojoMessageService.cs (resolved Upstream Issue). Also add stub implementation to StubKojoMessageService in Era.Core.Tests/Counter/Source/SourceEntrySystemTests.cs (compilation impact, same pattern as Task 1/Task 2) | | [x] |
| 32 | 56 | Verify WcCounterSourceHandler has >= 55 dispatch methods matching Handle*/Process*/Dispatch* naming convention (ERB baseline: 101 CASE entries → ~55-60 after consolidation) (verification-only: run after Task 4 creates the file) | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

**Example**:
```markdown
| 1 | 1 | Add API endpoint | | [ ] |        ← KNOWN: Expected response format is specified
| 2 | 2 | Calculate aggregates | [I] | [ ] | ← UNCERTAIN: Actual totals unknown until implemented
| 3 | 3 | Format report | | [ ] |           ← KNOWN: Uses Task 2's output (determined after Task 2)
```

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

**TDD Order Note**: This contract places implementation (Phases 2-3) before tests (Phase 4), deviating from standard TDD RED→GREEN. This follows the F803 CounterSourceHandler.cs precedent where the entire source handler is created as a single atomic migration unit. The large atomic migration pattern (1411-line ERB → single C# class) makes test-first impractical because test stubs would require the full class structure to compile against. Tests are written immediately after implementation and verified in Phase 5 (build).

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-805.md Tasks 1-3, 26, 30, 31 (interface edits + DI registration) | Updated ICommonFunctions.cs, IWcCounterOutputHandler.cs, IWcCounterSourceHandler.cs, ServiceCollectionExtensions.cs, INtrRevelationHandler.cs, IKojoMessageService.cs |
| 2 | implementer | sonnet | feature-805.md Task 4 (WcCounterSourceHandler.cs creation) | Era.Core/Counter/WcCounterSourceHandler.cs |
| 3 | implementer | sonnet | feature-805.md Task 5 (WcCounterMessage.cs creation) | Era.Core/Counter/WcCounterMessage.cs |
| 4 | implementer | sonnet | feature-805.md Tasks 6-10, 12-27, 29 (unit tests + verifications) | Era.Core.Tests/Counter/WcCounterSourceHandlerTests.cs, WcCounterMessageTests.cs |
| 5 | implementer | sonnet | feature-805.md Task 11 (build verification) | Build PASS confirmation |

### Pre-conditions

- F783, F803, F804, F811 all [DONE] (verified via Dependencies table)
- Baseline ICommonFunctions method count = 15 (re-verify with grep before Task 1)
- Two test classes implement IWcCounterOutputHandler: WcKojoTestOutputHandler (WcEventKojoTests.cs) and WcSelectorTestOutputHandler (WcActionSelectorTests.cs). Task 2 must add `int WcLoveRanking(int characterIndex) => throw new NotImplementedException();` stub to both.
- ~16 test classes in Era.Core.Tests/ directly implement ICommonFunctions. Adding HeteroSex to the interface without a default implementation will break these classes. Task 1 must handle (see Upstream Issues).
- WcCounterReaction constructor (F804): `WcCounterReaction(IVariableStore, ICounterUtilities, IRandomProvider, IEngineVariables, ITEquipVariables)` — all interface dependencies, fully mockable for AC#24 tests.
- WcCounterPunishment constructor (F804): `WcCounterPunishment(IVariableStore, IRandomProvider, IEngineVariables, ICommonFunctions)` — all interface dependencies, fully mockable for AC#25 tests.

### Execution Order

1. **Tasks 1-3 first** (interface changes): Edit the three existing interfaces before creating the implementing classes. The build will fail until both the interface additions and their implementations exist; therefore complete all interface edits in Phase 1 before moving to Phase 2. Task 26 (DI registration) can be done in Phase 1 alongside interface edits.
2. **Task 4** (WcCounterSourceHandler.cs): Follow CounterSourceHandler.cs structure exactly. Constructor: IVariableStore, IEngineVariables, ITEquipVariables, ICharacterStringVariables, ITouchStateManager, ICounterSourceHandler, ICommonFunctions, IPainStateChecker, IShrinkageSystem, IVirginityManager, IRandomProvider, ICounterUtilities. All SELECTCASE branches as private void Handle*/Process*/Dispatch* helpers (naming convention required for AC#56). FAVOR_CALC as `private int CalculateFavorIncrement(CharacterId)`. ForcedOrgasm as `private void SetForcedOrgasmCount(CharacterId, int)`. Task 4 is a single atomic migration unit following CounterSourceHandler.cs (F803) precedent, where the entire source handler is created in one task.
3. **Task 5** (WcCounterMessage.cs): Follow CounterMessage.cs structure. Constructor includes `INtrRevelationHandler? ntrRevelationHandler = null`. SendMessage calls DatuiMessage first (C14), then dispatches. MESSAGE13 invokes `_ntrRevelationHandler?.Execute()` for TRYCALL semantics. 8 kojo calls use IKojoMessageService. Implements WcLoveRanking per AC#8.
4. **Tasks 6-10, 12-27, 29, 32** (unit tests and verifications): Create `Era.Core.Tests/Counter/WcCounterSourceHandlerTests.cs` and `Era.Core.Tests/Counter/WcCounterMessageTests.cs`. Mock all injected dependencies via Mock<T>. Method names must contain the filter substrings used in AC#10-14,17-19,21-28,30,33,36 test filters exactly. **Note**: Task 27 is tagged `[I]` — it requires Mini-TDD sequencing (determine action-ID-to-variant mapping from ERB source first, then write test to match the concrete implementation). Execute Task 27 after the mapping is determined, not as part of the standard test batch. **Task 32** (verification-only): run after Task 4 creates WcCounterSourceHandler.cs; verify the private dispatch method count is >= 55 (AC#56 expected value is pre-set at 55).
5. **Task 11** (build): Run `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'`.

### Build Verification Steps

```bash
# C# build
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'

# Unit tests (with hang protection)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --blame-hang-timeout 10s --filter "FullyQualifiedName~WcCounter"'

# ERB loading warnings check
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR/Game && /home/siihe/.dotnet/dotnet run --project ../engine/uEmuera.Headless.csproj -- . --strict-warnings < /dev/null 2>&1'
```

### Success Criteria

- All 58 ACs pass static verification (AC#1-9,16,20,29,31,32,34,35,37-58 via Grep/Glob, AC#10-14,17-19,21-28,30,33,36,52 via dotnet test, AC#15 via build)
- No new ERB loading warnings introduced
- Test filter substrings for AC#10-14 match actual test method names exactly

### Error Handling

- IWcCounterOutputHandler has two existing implementing test classes (WcKojoTestOutputHandler, WcSelectorTestOutputHandler): Task 2 must add `int WcLoveRanking(int characterIndex) => throw new NotImplementedException();` stub to both; this is already reflected in Task 2 description
- If AC#7 expected count (16) is wrong at implementation time: re-verify baseline count, update AC#7 Expected to (baseline + 1), document in Execution Log before proceeding
- If DI registration for WcCounterSourceHandler / WcCounterMessage is needed in a composition root: check F811 SourceEntrySystem.cs; if already wired, no change needed; if not, add as part of Task 4/5 scope

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature exists -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred |
|-------|--------|-------------|----------------|---------------|:-----------:|
| F806 depends on WcCounterMessage message dispatcher | Sibling feature unblocked once F805 implements TRYCALLFORM dispatch | Feature | F806 | N/A | [x] |
| F807 depends on WcCounterMessage message dispatcher | Sibling feature unblocked once F805 implements TRYCALLFORM dispatch | Feature | F807 | N/A | [x] |
| F808 depends on WcCounterMessage message dispatcher | Sibling feature unblocked once F805 implements TRYCALLFORM dispatch | Feature | F808 | N/A | [x] |
| NTRrevelation handler (MESSAGE13) forward reference to F808 | F808 provides the NTRrevelation handler by implementing `INtrRevelationHandler`; F805 injects `INtrRevelationHandler?` defaulting to null (TRYCALL no-op semantics); standard DI `AddSingleton<INtrRevelationHandler, F808NtrRevelationHandler>()` resolves the dependency when F808 is registered. | Feature | F808 | N/A | [x] |
| Action-ID-to-variant dispatch routing table | F805 implements the dispatch routing table mapping action IDs to message variant numbers (N=1..4 or direct). F806/F807/F808 must register handlers for the correct variant numbers. Task 27 [I] determines the concrete mapping at implementation time; once known, the mapping must be transferred to F806/F807/F808 Technical Constraints before their /fc phase. | Feature | F806, F807, F808 | N/A | [x] |
| DatuiMessage DRY extraction | ERB DATUI_MESSAGE is a shared global function (Game/ERB/COUNTER_MESSAGE.ERB:484) used by both main counter and WC counter. F802 migrated it as `private void DatuiMessage` in CounterMessage.cs. F805 re-implements privately in WcCounterMessage (unavoidable: F802's method is private and sealed). Post-phase, extract to shared `IDatuiMessageService` to eliminate duplication. Note: F813 is [DRAFT]; transfer at /run Phase 10.0.2 (before F813's /fc overwrites content) | Feature | F813 | N/A | [x] |

<!-- Transferred column (F811 Lesson):
- [ ] = Not yet written to destination file
- [x] = Content confirmed in destination file (grep verified)
- Phase 10.0.2 mechanically verifies ALL rows are [x] before commit
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
| 2026-02-28 | DEVIATION | ac-static-verifier | AC#54 code verification | FAIL: File not found Era.Core/Interfaces/IKojoMessageService.cs (AC path wrong; actual file at Era.Core/Counter/IKojoMessageService.cs) |
| 2026-02-28 | DEVIATION | feature-reviewer | Post-review NEEDS_REVISION | CRITICAL: CalculateFavorIncrement semantic migration error — ERB applies favor before zero-activity check, C# reverses order; CalculateFavorIncrement drastically simplified (170-line ERB → 20 lines, just adds 1); AC#13 test contradicts AC spec (asserts Times.Never for favor vs spec requires Times.Once) |
| 2026-02-28 | FIX | implementer | CalculateFavorIncrement tests updated | Tests updated to match full ERB implementation (already present): COMABLE管理=1 setup, FlagIndex(12), correct assertion patterns. Full implementation was already in place (200+ lines). 2793 tests pass. |
| 2026-02-28 | DEVIATION | doc-reviewer | Doc consistency NEEDS_REVISION | engine-dev SKILL.md stale: IWcCounterSourceHandler still says stub, IWcCounterOutputHandler method count 3→4, IKojoMessageService count 9→10, INtrRevelationHandler missing, HeteroSex undocumented |
| 2026-02-28 | CodeRabbit | coderabbit | 2 Minor (修正不要) | null! DI登録パターン(F808で置換予定), WcLoveRankingエッジケース(ERB等価) |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: Deferred Obligations | EVENT_WC_COUNTER_SOURCE.ERB → TOILET_COUNTER_SOURCE.ERB (wrong filename)
- [fix] Phase1-RefCheck iter1: index-features.md F805 row | Missing F811 in Depends On column
- [fix] Phase2-Review iter1: Deferred Obligations section | Removed non-template section (content already in Technical Constraints C2 and Pre-conditions)
- [fix] Phase2-Review iter1: Task Tags guidance | Added missing Anti-pattern line per template
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#16 (WcCounterMessage implements IWcCounterOutputHandler) - file existence alone insufficient
- [fix] Phase2-Review iter1: AC Definition Table / Tasks | Added AC#17 + Task 12 (MCC bit flag mapping test for C8)
- [fix] Phase2-Review iter1: AC Definition Table / Tasks | Added AC#18 + Task 13 (dispatch branch test for C6), AC#19 + Task 14 (RESULT guard test for C13)
- [resolved-applied] Phase2-Pending iter2: [AC-005] No AC verifies DI wiring of WcCounterSourceHandler/WcCounterMessage in composition root. SourceEntrySystem.cs uses IWcCounterSourceHandler interface (not concrete class name), so AC target file and grep pattern need investigation. Current spec assumes DI auto-wiring but no AC enforces it.
- [fix] Phase2-Review iter2: AC#7 Details | Added implementation-time re-baseline requirement and staleness risk note
- [fix] Phase2-Review iter3: AC Definition Table / Tasks | Added AC#20 + Task 15 (IKojoMessageService kojo dispatch verification)
- [fix] Phase2-Review iter3: Goal Coverage | Added AC#15 to Goal 2 (build enforces HandleReaction/HandlePunishment interface compliance)
- [fix] Phase2-Review iter4: AC Definition Table / Tasks | Added AC#21 + Task 16 (DATUI delegation behavioral test for C3)
- [fix] Phase2-Review iter4: AC Definition Table / Tasks | Added AC#22 + Task 17 (HeteroSex behavioral test for C12)
- [fix] Phase2-Review iter4: AC Definition Table / Tasks | Added AC#23 + Task 18 (DATUI_MESSAGE sequence test for C14)
- [fix] Phase2-Review iter5: AC Definition Table / Tasks | Added AC#24 + Task 19 (HandleReaction behavioral test), AC#25 + Task 20 (HandlePunishment behavioral test)
- [fix] Phase2-Review iter5: AC Definition Table / Tasks | Added AC#26 + Task 21 (WcLoveRanking behavioral test for C11)
- [fix] Phase2-Review iter5: Philosophy Derivation | Added NTRrevelation Action? exception note + AC#14 to DI interfaces row
- [fix] Phase2-Review iter6: Technical Design / Success Criteria | Updated "15 ACs" → "26 ACs" throughout + Execution Order step 4
- [fix] Phase2-Review iter6: Key Decisions | Added WcLoveRanking parameter type decision (int justified by ERB caller boundary)
- [fix] Phase3-Maintainability iter7: Philosophy Derivation | Added WcLoveRanking int exception note to type-safe row
- [fix] Phase3-Maintainability iter7: Key Decisions | Added WcCounterMessage responsibility scope decision (single class justified by F804 interface design)
- [fix] Phase3-Maintainability iter7: Execution Order | Added Task 4 atomic migration justification note
- [fix] Phase3-Maintainability iter7: Mandatory Handoffs | Updated NTRrevelation row with Action? constructor parameter details
- [fix] Phase3-Maintainability iter7: Task 15 | Clarified as verification-only task
- [fix] Phase2-Review iter8: AC Definition Table / Tasks | Added AC#27 + Task 22 (reverse-rape CheckPain path for C7-ext)
- [fix] Phase2-Review iter9: C4 / AC#13 | Corrected zero-activity guard semantics: guard zeroes accumulator (FLAG:好感度上昇の累積値), not CFLAG favor (already applied at line 1341)
- [fix] Phase2-Review iter10: Implementation Contract | Added TDD Order Note justifying implementation-before-tests deviation (F803 atomic migration precedent)
- [fix] Phase3-Maintainability iter10: Technical Design | Added IShrinkageSystem, IVirginityManager, ICharacterStringVariables to WcCounterSourceHandler constructor sketch
- [fix] Phase2-Review iter1: Execution Order step 2 | Added ICharacterStringVariables, IShrinkageSystem, IVirginityManager to constructor list (matching Technical Design sketch)
- [fix] Phase1-RefCheck iter1: Technical Constraints C9 | Clarified abbreviated COUNTER_MESSAGE.ERB:484 → Game/ERB/COUNTER_MESSAGE.ERB:484
- [fix] Phase2-Review iter1: AC Design Constraints | Added C15 (Reverse-rape CheckPain path) with Constraint Details; updated AC#27 label from C7-ext to C15
- [fix] Phase2-Review iter1: Mandatory Handoffs | Split F806/F807/F808 combined row into 3 separate rows (one per Destination ID)
- [fix] Phase2-Review iter2: AC Definition Table / Tasks | Added AC#28 + Task 23 (FAVOR_CALC HeteroSex integration test for C4/C12); updated Goal Coverage, AC Coverage, Implementation Contract, Success Criteria
- [fix] Phase3-Maintainability iter3: AC Definition Table | Added AC#29 (IConsoleOutput injection in WcCounterMessage); updated Goal Coverage, AC Coverage, Success Criteria
- [fix] Phase2-Review iter4: Tasks table | Updated Task 5 AC# from '2, 16, 20' to '2, 16, 20, 29' (AC#29 orphan fix)
- [fix] Phase2-Review iter4: AC Definition Table / Tasks | Added AC#30 + Task 24 (NTRrevelation positive path test); updated Goal Coverage, AC Coverage, Implementation Contract, Success Criteria
- [fix] Phase2-Review iter5: Baseline Measurement | Updated ICommonFunctions method count from 14 to 15 (aligned grep pattern with AC#7's \\(.*\\); pattern)
- [fix] Phase2-Review iter5: AC#24/AC#25 Details | Added ERB source line references and concrete assertion specifications for HandleReaction and HandlePunishment
- [fix] Phase2-Review iter6: AC#4 | Simplified regex from ITouchSet[^M] to ITouchSet; corrected rationale (ITouchStateManager does not contain ITouchSet substring)
- [resolved-applied] Phase3-Maintainability iter7: [DES-001] WcCounterMessage constructor sketch missing WcCounterReaction/WcCounterPunishment dependencies. F804 provides these classes but constructor sketch and Key Decision don't address delegation vs reimplementation. Must decide: (A) inject WcCounterReaction/WcCounterPunishment for delegation (following F802 CounterOutputHandler pattern) or (B) reimplement inline. Affects constructor signature, AC#24/AC#25 assertions, and Task 5 scope.
- [fix] Phase3-Maintainability iter7: Key Decisions | Added ISP exception justification to WcLoveRanking interface placement rationale
- [fix] Phase3-Maintainability iter7: AC#24/AC#25 Details | Removed caller-side assertions (WcActionSelector RETURN 0 behavior) from Expected; focused on implementer behavior only
- [fix] Phase2-Review iter8: AC Definition Table / Tasks | Added AC#31 + AC#32 (IShrinkageSystem/IVirginityManager injection verification per F803 precedent); updated Task 4, Goal Coverage, AC Coverage, Success Criteria
- [fix] Phase2-Review iter1: Upstream Issues / Pre-conditions / Task 2 | Changed hypothetical IWcCounterOutputHandler implementors to concrete: WcKojoTestOutputHandler (WcEventKojoTests.cs) and WcSelectorTestOutputHandler (WcActionSelectorTests.cs); expanded Task 2 scope to add WcLoveRanking stubs
- [fix] Phase2-Review iter1: AC Definition Table / Tasks | Added AC#33 + Task 25 (IKojoMessageService kojo dispatch behavioral test); updated Goal Coverage, AC Coverage, Implementation Contract, Success Criteria
- [fix] Phase2-Review iter1: Task Tags subsection | Added missing Example code block per template
- [fix] Phase2-Review iter1: Implementation Contract blockquote | Moved TDD Order Note outside blockquote to match template structure
- [resolved-applied] Phase2-Review iter1: [AC-005] Two existing [pending] items (AC-005 DI wiring, DES-001 delegation decision) remain from prior FL sessions; must be resolved in POST-LOOP before /run
- [resolved-applied] Phase2-Review iter2: [DES-002] AC#24/AC#25 cite TOILET_COUNTER.ERB (lines 53-69) which is outside F805 migration scope (TOILET_COUNTER_SOURCE.ERB + TOILET_COUNTER_MESSAGE.ERB). HandleReaction/HandlePunishment implementation scope depends on DES-001 resolution (delegation vs inline). Cannot fix until DES-001 is resolved.
- [fix] PostLoop iter2: AC Definition Table / Tasks | Added AC#34 + AC#35 + Task 26 (DI composition root registration for IWcCounterSourceHandler and IWcCounterOutputHandler); resolved AC-005
- [fix] Phase2-Review iter1: Tasks table | Added AC#26 to Task 5 AC# column (WcLoveRanking implementation obligation)
- [resolved-applied] Phase2-Review iter2: [DES-003] WcCounterMessage constructor injects concrete sealed classes WcCounterReaction/WcCounterPunishment. Sealed classes cannot be mocked by standard .NET mocking frameworks (Moq/NSubstitute). AC#24/AC#25 say "mock WcCounterReaction/WcCounterPunishment" which is impossible. Must decide: (A) create IWcCounterReaction/IWcCounterPunishment interfaces in F804 scope (Mandatory Handoff), (B) unseal the classes and add virtual methods, (C) use integration tests with concrete classes, or (D) use wrapper/adapter pattern.
- [fix] PostLoop iter2: AC#24/AC#25 Details + Key Decisions | Resolved DES-003: changed "mock WcCounterReaction/WcCounterPunishment" to "real instances with mocked dependencies" per F802 pattern
- [resolved-applied] Phase2-Pending iter1: Tasks table, Task 5 AC# column | Task 5 lists AC#26 but AC#26 is a test AC owned by Task 21; adding AC#26 to Task 5 was a deliberate fix (prior session iter1) but reviewer argues test ACs should only be owned by test-writing tasks (loop: add→remove conflict)
- [fix] Phase2-Uncertain iter1: Philosophy Derivation | Added C8 MCC bit representative sampling exception note and AC#17 to "SSOT for all" row
- [fix] Phase2-Review iter1: AC Definition Table / Tasks | Added AC#36 + Task 27 (dispatch routing test for action-ID-to-variant mapping, closing AC#18 gap)
- [fix] Phase2-Review iter2: AC Definition Table / Tasks | Added AC#37-40 + Task 28 (constructor dependency injection verification for IEngineVariables, ITEquipVariables, ICharacterStringVariables, IRandomProvider)
- [fix] Phase3-Maintainability iter3: Task 26 / AC Definition Table | Added AC#41-42 + expanded Task 26 for WcCounterReaction/WcCounterPunishment DI registration (transitive dependency resolution)
- [fix] Phase3-Maintainability iter4: Upstream Issues / Task 1 / Pre-conditions | Documented ICommonFunctions HeteroSex compilation impact on ~16 test implementors
- [fix] Phase2-Review iter5: Task 27 | Added [I] tag (action-ID-to-variant mapping depends on ERB source reading at implementation time)
- [fix] Phase2-Review iter6: AC Definition Table / Tasks | Added AC#43 (ICommonFunctions injection verification in WcCounterSourceHandler, completing constructor dependency AC pattern)
- [fix] Phase3-Maintainability iter7: WcCounterMessage constructor sketch / AC Definition Table | Added ITEquipVariables + ITextFormatting to constructor and AC#44-45 (following F802 CounterMessage.cs pattern)
- [fix] Phase2-Review iter8: AC Definition Table / Tasks | Added AC#46-49 (IPainStateChecker, IEngineVariables, IVariableStore injection verification — completing all constructor dependency AC coverage for both classes)
- [fix] Phase2-Review iter9: Goal / Goal Coverage | Corrected "IWcCounterOutputHandler.SendMessage" to "IWcCounterOutputHandler (SendMessage, HandleReaction, HandlePunishment)" in Goal and Goal Coverage row 2
- [fix] Phase2-Review iter9: Execution Order step 4 | Added Task 27 [I] tag Mini-TDD sequencing note
- [fix] Phase2-Review iter10: Mandatory Handoffs | Updated NTRrevelation row with concrete DI wiring mechanism (Action? requires factory registration); added routing table handoff row for F806/F807/F808
- [fix] Phase3-Maintainability iter10: Constructor sketch / AC Definition Table | Added ICounterUtilities to WcCounterSourceHandler constructor + AC#50 (EXP_UP migration dependency)
- [fix] Phase2-Review iter1: AC Coverage table | Added Task# third column header and backfilled empty cells for AC#1-35 (column count mismatch fix)
- [fix] Phase2-Review iter1: Feasibility Assessment | Added AC volume justification row (50 ACs for 1907 ERB lines)
- [fix] Phase2-Review iter1: Goal Coverage row 2 | Added AC#8, AC#26 to IWcCounterOutputHandler migration coverage (WcLoveRanking part of interface scope)
- [resolved-applied] Phase2-Uncertain iter1: Key Decisions / Task 26 | Action? ntrRevelationHandler constructor default parameter cannot be resolved by standard Microsoft.Extensions.DependencyInjection AddSingleton<IWcCounterOutputHandler, WcCounterMessage>(). DI container will fail at runtime unless custom factory is used. Spec defers to F808 Mandatory Handoffs but no AC verifies DI resolution succeeds without F808. Design decision needed: (A) define INtrRevelationHandler interface (DI-resolvable), or (B) keep Action? and add DI resolution verification AC.
- [fix] Phase2-Review iter2: AC#13 Details + AC Coverage row 13 | Specified concrete test assertions: (1) accumulator zeroed, (2) favor incremented; added action code activation requirement to prevent trivial pass-through
- [fix] Phase2-Review iter3: AC#21 Details + AC Coverage row 21 | Expanded DATUI delegation test to cover all 4 DATUI types (DatUIBottom/Top/BottomT/TopT) via representative action codes, closing C3 coverage gap
- [fix] Phase3-Maintainability iter4: Technical Constraints | Added Phase 21 DI registration incrementality constraint (F805 registers own classes only; transitive deps deferred to F813)
- [fix] Phase3-Maintainability iter4: Key Decisions ISP exception | Updated WcLoveRanking ISP rationale to acknowledge 2 test implementors (WcKojoTestOutputHandler, WcSelectorTestOutputHandler) handled by Task 2
- [fix] Phase3-Maintainability iter5: Technical Constraints + Mandatory Handoffs | Documented DatuiMessage DRY duplication (ERB shared global → F802 private → F805 re-implements privately); added Mandatory Handoff to F813 for IDatuiMessageService extraction
- [fix] Phase3-Maintainability iter6: Upstream Issues | Added IKojoMessageService.KojoMessageWcCounter signature gap (6 distinct kojo dispatch types require action-type parameter or alternative approach)
- [fix] Phase3-Maintainability iter7: Constructor sketch + AC#51 + Task 5 + Goal Coverage | Added IRandomProvider to WcCounterMessage constructor (13 PRINTDATA calls); added AC#51 for grep verification; updated AC count to 51
- [fix] Phase2-Review iter1: Mandatory Handoffs | Truncated Creation Task column from "N/A (informational; ...)" to "N/A" (template format compliance)
- [resolved-applied] Phase2-Uncertain iter1: AC#33 / Upstream Issues | IKojoMessageService.KojoMessageWcCounter(int) has insufficient signature for 6 distinct kojo dispatch types. AC#33 verifies characterIndex but cannot verify action-type routing. Upstream Issues documents options A/B/C but leaves resolution to implementer. Design decision needed: resolve interface extension before /run so AC#33 can assert correct action-type value.
- [fix] Phase2-Review iter2: Key Decisions + Technical Design | Changed NTRrevelation from Action? to INtrRevelationHandler (user decision: DI-standard pattern)
- [fix] Phase2-Review iter2: Upstream Issues + AC#33 | Resolved IKojoMessageService signature: added actionType parameter (user decision: Option A)
- [fix] Phase2-Review iter2: AC Definition Table / Tasks | Added AC#52 (DatuiMessage behavioral correctness test for C14-ext)
- [fix] Phase2-Review iter3: Task 25 + AC Coverage | Updated KojoDispatch test name to include actionType verification (Task-to-AC#33 alignment)
- [fix] Phase2-Review iter1: AC Coverage table | Removed non-template Task# third column from all 52 rows (template specifies 2 columns: AC# | How to Satisfy)
- [fix] Phase2-Review iter1: Tasks table Task 5 | Removed AC#26 from Task 5 AC# list (dual ownership with Task 21; Task 21 exclusively owns AC#26)
- [resolved-applied] Phase2-Review iter2: Philosophy Derivation / AC Coverage | Philosophy "SSOT for all counter-related game logic" claim insufficiently verified. **Resolution (post-loop iter3)**: Option A — added AC#56 (private dispatch method count_gte verification) + Task 32 [I] to verify all SELECTCASE branches migrated
- [fix] Phase2-Review iter2: Goal Coverage row 6 + Philosophy Derivation | Removed AC#4 from Goal Coverage row 6 and Philosophy Derivation "type-safe" row (AC#4 is vacuously true on non-existent file; AC#3 already covers ITouchStateManager positive mandate)
- [fix] Phase2-Review iter3: Tasks table / AC#52 | Moved AC#52 from Task 5 (implementation) to new Task 29 (test-writing); updated Implementation Contract Phase 4 to include Task 29
- [resolved-applied] Phase2-Review iter4: Philosophy Derivation "type-safe" row | AC#3 only greps for ITouchStateManager, does not verify HandleWcCounterSource accepts CharacterId parameter. No AC verifies CharacterId usage. Need decision: (A) add AC verifying HandleWcCounterSource.*CharacterId grep, or (B) confirm IWcCounterSourceHandler.cs already defines CharacterId in interface (making AC#1+AC#15 indirect coverage sufficient). **Resolution (iter2)**: Option B confirmed — IWcCounterSourceHandler.cs:13 defines `HandleWcCounterSource(CharacterId target, int arg1)`. AC#1 (class implements interface) + AC#15 (build succeeds) transitively enforce CharacterId usage.
- [fix] Phase3-Maintainability iter4: Tasks / AC | Added Task 30 + AC#53 (INtrRevelationHandler interface creation); Key Decision mandates interface but no task existed
- [fix] Phase3-Maintainability iter4: Tasks / AC | Added Task 31 + AC#54 (KojoMessageWcCounter two-parameter overload in IKojoMessageService); Upstream Issue resolved but no task implemented it
- [fix] Phase3-Maintainability iter4: Tasks / AC | Added AC#55 (two-parameter KojoMessageWcCounter call verification in WcCounterMessage.cs); added to Task 5 AC# column
- [fix] Phase1-RefCheck iter1: Root Cause Analysis L48 | F033 reference clarified as historical (no feature file exists); inline explanation added
- [fix] Phase2-Review iter1: Feasibility/Approach/Success Criteria | Updated "52 ACs" → "55 ACs" at 3 locations; extended AC ranges to include AC#53-55
- [fix] Phase2-Review iter1: Goal Coverage row 2 | Added AC#54, AC#55 to Goal 2 (WcCounterMessage migration covers kojo dispatch overload)
- [fix] Phase2-Review iter2: Goal Coverage row 3 | Added clarification that AC#53 covers interface creation; DI registration deferred to F808 per Mandatory Handoffs
- [fix] Phase2-Uncertain iter1: Pre-conditions | Added WcCounterReaction/WcCounterPunishment constructor signatures from F804 source (all interface deps, confirming AC#24/AC#25 mockability)
- [fix] Phase2-Review iter2: Goal Coverage row 3 | Clarified DI scope: AC#34/35/41/42 cover F805 own-class registration; only INtrRevelationHandler DI deferred to F808
- [resolved-applied] Phase2-Review iter2: [pending] CharacterId type-safe | Confirmed IWcCounterSourceHandler.cs:13 defines CharacterId; AC#1+AC#15 provide indirect coverage (option B)
- [resolved-applied] Phase2-Pending iter3: INtrRevelationHandler? DI resolution — MS DI does NOT resolve unregistered nullable interface parameters as null. **Resolution (post-loop iter3)**: Option A — added null sentinel registration (AddSingleton<INtrRevelationHandler>(sp => null!)) to Task 26 + AC#57 (DI registration grep); corrected Key Decision rationale
- [fix] PostLoop iter3: AC Definition Table / Tasks / Goal Coverage / Philosophy | Added AC#56 + Task 32 [I] (SELECTCASE branch migration completeness); resolved SSOT philosophy coverage gap
- [fix] PostLoop iter3: Key Decision / Task 26 / AC#57 | Added INtrRevelationHandler null sentinel registration + DI resolution verification AC
- [fix] Phase2-Review iter1: AC Details | Added missing AC#53, AC#54, AC#55 detail blocks (Test/Expected/Rationale)
- [fix] Phase2-Review iter1: AC#56 | Changed regex from `private .*(void|int) ` to `private void (Handle|Process|Dispatch)[A-Z]` (dispatch-only naming convention)
- [fix] Phase2-Review iter1: AC#53 | Changed from Glob file-existence to Grep `void Execute(` (verifies interface contract for F808 integration)
- [fix] Phase2-Review iter2: AC#56 / Task 32 | Removed [I] tag; set count_gte: 45 based on ERB analysis (101 CASE entries → ~55-60 after consolidation; 45 floor)
- [fix] Phase2-Review iter3+: AC#56 / Task 32 | Raised count_gte floor from 45 to 55 (lower bound of ~55-60 consolidation estimate; prevents significant branch omissions)
- [fix] Phase2-Review iter3: Upstream Issues | Added IWcCounterOutputHandler.cs messageVariant comment fix note (variants 1-4, not just 1)
- [fix] Phase2-Review iter4: Task 4 / Execution Order | Added Handle*/Process*/Dispatch* naming convention requirement for AC#56 verification
- [fix] Phase2-Review iter5: Task 4 AC# column | Added AC#37,38,39,40,43,46,48,50 (constructor dependency injection ACs that Task 4 must implement)
- [fix] Phase2-Review iter5: Philosophy Derivation | Acknowledged AC#56 as structural proxy; added AC#21,AC#27 for behavioral verification; documented multi-layer verification strategy
- [fix] Phase2-Review iter6: Technical Constraints | Clarified DI registration ownership: F805-owned (WcCounterSourceHandler, WcCounterMessage) vs F804-owned dependency classes (WcCounterReaction, WcCounterPunishment); added INtrRevelationHandler null sentinel
- [fix] Phase2-Review iter7: AC#56 / Task 32 | Raised count_gte floor from 45 to 55 (lower bound of ~55-60 consolidation estimate; prevents ~25% branch omissions)
- [fix] Phase2-Review iter8: AC Coverage row 30 | Changed stale "Action handler" to "INtrRevelationHandler.Execute()" (aligning with Key Decision)
- [fix] Phase2-Uncertain iter8: Philosophy Derivation type-safe row | Added AC#1, AC#15 for transitive CharacterId enforcement alongside AC#3
- [fix] Phase2-Review iter1: AC#56 Definition Table | Fixed Matcher/Expected column swap: moved regex pattern to Method parameter, Matcher from 'count_gte: 55' to 'count_gte', Expected from regex to '55'
- [resolved-skipped] Phase2-Review iter1: AC#56 behavioral coverage gap — AC#56 is structural proxy (method count), only AC#21/AC#27 provide behavioral tests for ~55 dispatch branches. Multi-layer strategy documented but ~53 branches have zero behavioral coverage. (Loop: same concern as iter2/iter5 resolution — previously accepted as multi-layer strategy). User decision: multi-layer strategy (AC#56 structural + AC#21/AC#27 behavioral representative + AC#15 build gate) accepted as sufficient, consistent with F803 precedent
- [fix] Phase2-Review iter1: AC#7 Description | Added '(re-baseline Expected before testing)' to make fragility obligation visible in AC Definition Table SSOT row
- [fix] Phase3-Maintainability iter2: Technical Constraints DI incrementality | Added runtime note: AddSingleton calls throw InvalidOperationException before F813 completes transitive registrations (by design)
- [fix] Phase3-Maintainability iter2: Task 5 description | Added IWcCounterOutputHandler.cs:13 comment correction (messageVariant 1-4) and stub label removal obligations
- [fix] Phase3-Maintainability iter2: Mandatory Handoffs DatuiMessage row | Added F813 [DRAFT] risk note and Phase 10.0.2 transfer timing
- [fix] Phase3-Maintainability iter3: Task 31 description | Added StubKojoMessageService compilation impact obligation (same pattern as Task 1/Task 2)
- [fix] Phase2-Review iter4: AC#57 | Strengthened grep pattern from 'INtrRevelationHandler' to 'INtrRevelationHandler.*null' (null sentinel specificity; prevents concrete NullNtrRevelationHandler from breaking F808 DI override)
- [fix] Phase2-Review iter5: AC Definition Table / Tasks | Added AC#58 (IWcCounterOutputHandler.cs stub label removal, symmetry with AC#9); added to Task 5 AC# column, Goal Coverage row 2, AC Coverage, Success Criteria
- [fix] Phase2-Review iter5: Task 4 AC# column | Added AC#56 as co-owner (Task 4 implements naming convention, Task 32 verifies count)
- [resolved-applied] Phase2-Review iter6: Task 1 / Upstream Issues | ICommonFunctions HeteroSex DIM default value unspecified. If Approach A with `=> 0`, ~16 test classes silently return valid game-logic value (heterosexual) instead of failing on unexpected HeteroSex calls. User decision: Approach A with `throw new NotImplementedException(nameof(HeteroSex))` (consistent with WcLoveRanking stub pattern)
- [fix] PostLoop iter6: Task 1 description + Upstream Issues | Changed HeteroSex DIM default from `=> 0` to `=> throw new NotImplementedException(nameof(HeteroSex))` (user decision: fail-loud pattern)
- [fix] Phase4-ACValidation iter7: Goal Coverage row 6 | Added AC#4 (ITouchSet NOT used) to Goal row 6 (ac-check lint fix)
- [fix] Phase4-ACValidation iter7: AC#56 Matcher | Changed invalid 'count_gte' to valid 'gte' (matcher list compliance)
- [fix] Phase4-ACValidation iter7: AC#53 Expected | Removed trailing '\\|' from regex; corrected to 'void Execute\\(' (regex fix)

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning
- [Predecessor: F803](feature-803.md) - Counter Source (provides DATUI_*/PainCheckVMaster helpers)
- [Predecessor: F804](feature-804.md) - WC Counter Core (defines IWcCounterOutputHandler)
- [Predecessor: F811](feature-811.md) - SOURCE Entry System (provides ITouchStateManager, IWcCounterSourceHandler stub)
- [Related: F802](feature-802.md) - Main Counter Output (CounterMessage.cs pattern)
- [Successor: F806](feature-806.md) - WC Counter Message SEX
- [Successor: F807](feature-807.md) - WC Counter Message TEASE
- [Successor: F808](feature-808.md) - WC Counter Message ITEM + NTR
- [Successor: F813](feature-813.md) - Post-Phase Review Phase 21
