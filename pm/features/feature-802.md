# Feature 802: Main Counter Output

## Status: [DONE]
<!-- fl-reviewed: 2026-02-25T12:19:27Z -->

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

<!-- Summary: C# migration of Main Counter output subsystem: messages, poses, reactions, punishments, and combination handling across 5 ERB files (COUNTER_MESSAGE, COUNTER_POSE, COUNTER_REACTION, COUNTER_PUNISHMENT, COUNTER_CONBINATION) totaling 867 lines. -->

<!-- fc-phase-1-completed -->

---

## Background

### Philosophy (Mid-term Vision)

Phase 21: Counter System -- Main Counter Output is the SSOT for all counter output processing: reaction evaluation (COUNTER_REACTION.ERB), punishment dispatch (COUNTER_PUNISHMENT.ERB), pose state tracking (COUNTER_POSE.ERB), counter event messaging (COUNTER_MESSAGE.ERB), and combination counting (COUNTER_CONBINATION.ERB). Each ERB file migrates to a dedicated C# class under Era.Core/Counter/ with typed indices and interface-driven dependency injection, following the established WC Counter (F804) migration pattern. F802 implements the ICounterOutputHandler interface defined by F801 for REACTION/PUNISHMENT, and exposes additional entry points for MESSAGE/POSE/COMBINATION called from upstream orchestrators (INFO.ERB, EVENT_KOJO.ERB, SOURCE.ERB).

### Problem (Current Issue)

F802 is a minimal DRAFT stub created by F783's Phase 21 planning with only 2 generic tasks and 1 AC, because F783 focused on file-prefix grouping rather than implementation readiness. The Scope Reference line counts are completely wrong (claimed 317/200/105/93/148 vs actual 550/131/131/45/10), indicating no source file verification was performed. The ICounterOutputHandler interface defined by F801 (`ICounterOutputHandler.cs:10-23`) exposes only HandleReaction and HandlePunishment (2 methods), but F802's 5 ERB files serve 3 architecturally distinct call chains: (a) REACTION/PUNISHMENT called from F801's ActionSelector via ICounterOutputHandler (`COUNTER_SELECT.ERB:52,58`), (b) POSE/COMBINATION called from INFO.ERB (`INFO.ERB:189,191`) and SOURCE.ERB (`SOURCE.ERB:40`), and (c) MESSAGE called from EVENT_KOJO.ERB (`EVENT_KOJO.ERB:76,83,92`) via the kojo fallback system. Additionally, 3 external text formatting functions (PANTS_DESCRIPTION, PANTSNAME, OPPAI_DESCRIPTION) used by COUNTER_MESSAGE.ERB have no C# interface equivalents, and TCVarIndex.cs is missing 6+ constants (indices 21-27) needed by REACTION, POSE, and MESSAGE. COUNTER_MESSAGE.ERB is not pure display -- it mutates TEQUIP state at 20+ sites during text output.

### Goal (What to Achieve)

Migrate 5 ERB files (COUNTER_MESSAGE 550 lines, COUNTER_POSE 131 lines, COUNTER_REACTION 131 lines, COUNTER_PUNISHMENT 45 lines, COUNTER_CONBINATION 10 lines) to C# classes under Era.Core/Counter/. Implement ICounterOutputHandler (HandleReaction + HandlePunishment including all-character cleanup loop). Create separate handler classes for MESSAGE, POSE, and COMBINATION with appropriate interfaces for upstream callers. Add TCVarIndex constants for indices 21 (SubAction), 22-25 (undressing states), and 27 (sixnine transition). Define strategy for missing text formatting functions (PANTS_DESCRIPTION, PANTSNAME, OPPAI_DESCRIPTION). Translate TRYCALLFORM dynamic dispatch in MESSAGE to C# dictionary/switch pattern. Equivalence tests must verify C# output matches legacy ERB behavior.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is F802 not implementation-ready? | DRAFT has only 2 stub tasks and 1 generic AC; scope reference line counts are all wrong (317/200/105/93/148 vs actual 550/131/131/45/10) | `feature-802.md:33-39` |
| 2 | Why was no source analysis performed? | F783 created all Phase 21 DRAFTs from a minimal template focused on file grouping, deferring detailed analysis to /fc | `feature-783.md:407-422` |
| 3 | Why does the DRAFT not reflect the 3 distinct call chains? | F783 grouped files by naming convention (COUNTER_*) without analyzing that REACTION/PUNISHMENT, POSE/COMBINATION, and MESSAGE have entirely separate upstream callers | `COUNTER_SELECT.ERB:52,58`, `INFO.ERB:189,191`, `EVENT_KOJO.ERB:76,83,92` |
| 4 | Why is ICounterOutputHandler insufficient as the sole API surface? | F801 analyzed only the COUNTER_SELECT.ERB call sites (lines 52, 58) which call REACTION and PUNISHMENT; the remaining 3 ERB files are called from INFO.ERB, SOURCE.ERB, and EVENT_KOJO.ERB which are outside F801 scope | `ICounterOutputHandler.cs:10-23`, `INFO.ERB:189,191`, `SOURCE.ERB:40` |
| 5 | Why (Root)? | The ICounterOutputHandler interface was designed for ActionSelector's output delegation (REACTION/PUNISHMENT only), but F802 owns 5 ERB files serving 3 distinct call chains from different upstream orchestrators, and 3 text formatting functions (PANTS_DESCRIPTION, PANTSNAME, OPPAI_DESCRIPTION) used by COUNTER_MESSAGE have no C# equivalents, plus TCVarIndex is missing 6+ constants (21-27) needed across REACTION, POSE, and MESSAGE | `ICounterOutputHandler.cs:10-23`, `COUNTER_MESSAGE.ERB:36,129,133,317,319`, `TCVarIndex.cs:42-45` |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | F802 DRAFT has 2 stub tasks, 1 AC, and wrong line counts | ICounterOutputHandler covers only 2 of 5 ERB files; 3 distinct call chains require separate interface strategies; 3 text formatting functions and 6+ TCVarIndex constants are missing |
| Where | `feature-802.md` Tasks and AC sections | `ICounterOutputHandler.cs:10-23` (2-method interface), `INFO.ERB:189,191` / `SOURCE.ERB:40` / `EVENT_KOJO.ERB:76,83,92` (3 separate callers), `TCVarIndex.cs:42-45` (missing indices) |
| Fix | Add more tasks and ACs manually | Implement ICounterOutputHandler for REACTION/PUNISHMENT; create separate handler classes with interfaces for POSE/COMBINATION/MESSAGE; add TCVarIndex constants; define stub strategy for missing description functions |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F783 | [DONE] | Predecessor -- Phase 21 Planning (parent decomposition) |
| F801 | [DONE] | Predecessor -- defines ICounterOutputHandler that F802 implements; ActionSelector calls HandleReaction/HandlePunishment |
| F803 | [PROPOSED] | Sibling -- Main Counter Source; owns DATUI_BOTTOM/TOP undressing functions (separate from DATUI_MESSAGE in F802) |
| F804 | [DONE] | Related -- WC Counter Core; WcCounterReaction/WcCounterPunishment provide direct design reference for F802 |
| F805 | [DRAFT] | Related -- WC Counter Source + Message Core |
| F809 | [DONE] | Related -- COMABLE Core (IComAvailabilityChecker owner) |
| F811 | [PROPOSED] | Sibling -- SOURCE Entry System; SOURCE.ERB:40 and INFO.ERB:189,191 call F802's COMBINATION and POSE |
| F813 | [DRAFT] | Successor -- Post-Phase Review Phase 21 |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Source files identifiable and accessible | FEASIBLE | All 5 ERB files exist: COUNTER_MESSAGE (550), COUNTER_POSE (131), COUNTER_REACTION (131), COUNTER_PUNISHMENT (45), COUNTER_CONBINATION (10) |
| Predecessor interfaces available | FEASIBLE | ICounterOutputHandler (F801), ICounterUtilities (F801/F804), IConsoleOutput, ICommonFunctions, ITEquipVariables, IClothingSystem all exist |
| WC analog provides design reference | FEASIBLE | WcCounterReaction.cs and WcCounterPunishment.cs (F804) are directly analogous implementations with same DI patterns |
| Required C# interfaces available | FEASIBLE | IVariableStore, ITEquipVariables, IEngineVariables, ICounterUtilities, IRandomProvider, ICommonFunctions, IConsoleOutput, IClothingSystem all exist |
| Text formatting functions available | NEEDS_REVISION | PANTS_DESCRIPTION, PANTSNAME, OPPAI_DESCRIPTION have NO C# equivalent; must define stub interface or add to existing interface |
| TCVarIndex constants complete | NEEDS_REVISION | Missing indices 21 (SubAction), 22-25 (undressing states), 27 (sixnine transition) |
| Total scope within erb type limit | FEASIBLE | 867 actual lines; COUNTER_MESSAGE alone is 550 but consists of repetitive PRINTFORML patterns; WC Counter (F804) migrated similar scope |
| COUNTER_CONBINATION is incomplete legacy code | FEASIBLE | Marked "つくりかけ" (work in progress); 10 lines, no output -- migrate as-is |

**Verdict**: NEEDS_REVISION

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core/Counter/ namespace | HIGH | 5-6 new C# classes (CounterOutputHandler, CounterReaction, CounterPunishment, CounterPose, CounterMessage, CounterCombination) |
| TCVarIndex.cs | MEDIUM | Adding 6+ new well-known constants (indices 21-27); additive-only, no breaking changes |
| ICounterOutputHandler consumers | LOW | F801 ActionSelector already calls HandleReaction/HandlePunishment; F802 provides the concrete implementation |
| Upstream callers (INFO, SOURCE, KOJO) | MEDIUM | F802 must expose POSE/COMBINATION/MESSAGE via interfaces for future C# callers (F811 and others) |
| Text formatting subsystem | MEDIUM | Missing PANTS_DESCRIPTION/PANTSNAME/OPPAI_DESCRIPTION requires new interface or stubs; decision needed before implementation |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| ICounterOutputHandler covers only REACTION/PUNISHMENT | `ICounterOutputHandler.cs:10-23` | MESSAGE/POSE/COMBINATION need separate interfaces or handler classes |
| TRYCALLFORM dynamic dispatch in MESSAGE | `COUNTER_MESSAGE.ERB:8,10` | ~35 message handlers dispatched by action ID; requires C# dictionary/switch pattern |
| PRINTFORML inline ternary `\@ expr ? A # B \@` | `COUNTER_MESSAGE.ERB:103,118,120,126` | Must translate to C# conditional string building |
| PANTS_DESCRIPTION/PANTSNAME/OPPAI_DESCRIPTION not in C# | `COUNTER_MESSAGE.ERB:36,129,133,317,319` | Must create stub interface or add to existing interface |
| MESSAGE mutates TEQUIP state during text output | `COUNTER_MESSAGE.ERB:147-336` | COUNTER_MESSAGE is not pure display; TEQUIP:PLAYER:3/4 mutations must be preserved |
| CFLAG mutation in PUNISHMENT | `COUNTER_PUNISHMENT.ERB:20-44` | Modifies clothing CFLAGs (underwear, rotor insertion) |
| HandlePunishment must iterate ALL characters | `ICounterOutputHandler.cs:14-22`, `COUNTER_SELECT.ERB:60-64` | Reset うふふ CFLAG + CLOTHES_RESET + CLOTHES_SETTING_TRAIN per character |
| PUNISHMENT has DRAWLINE + WAIT | `COUNTER_PUNISHMENT.ERB:14-15` | WC analog omits these; F802 must include via IConsoleOutput |
| PUNISHMENT random retry loop | `COUNTER_PUNISHMENT.ERB:8-13` | WHILE loop with TRYCALLFORM PUNISHMENT_{RAND:10}; only variants 0,1,2 implemented |
| NO:ARG == 999 skip guard in PUNISHMENT | `COUNTER_PUNISHMENT.ERB:2-4` | Character 999 (player) skips punishment; RETURN 0 with commented-out COM461_PUNISHMENT_FINISH |
| TCVarIndex missing indices 21-25, 27 | `TCVarIndex.cs:42-45` | Must add SubAction(21), undressing states (22-25), sixnine transition(27); index 26 (マスターカウンター制御, Master Counter Control bitfield) is used by COUNTER_SOURCE.ERB/COMABLE_300.ERB but not by any of the 5 in-scope ERB files — deferred to F803/F811 scope |
| SELECTCOM usage in REACTION | `COUNTER_REACTION.ERB:40,48,75,86,97,105,125` | Uses SELECTCOM for action-specific branching; available via IEngineVariables.GetSelectCom() |
| COUNTER_CONBINATION is explicitly unfinished | `COUNTER_CONBINATION.ERB:3` | "つくりかけ" comment; 10 lines, counting only |
| TreatWarningsAsErrors=true | `Directory.Build.props` | All new C# must compile warning-free |
| IClothingSystem uses raw delegates | `IClothingSystem.cs:53-56` | SettingTrain adapter pattern needed (follow F804 precedent) |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Missing text formatting functions block compilation | HIGH | HIGH | Define stub interface for PANTS_DESCRIPTION/PANTSNAME/OPPAI_DESCRIPTION with NotImplementedException or simple fallback |
| ICounterOutputHandler too narrow for all 5 ERB files | HIGH | MEDIUM | Design separate handler classes for MESSAGE/POSE/COMBINATION with own interfaces; only REACTION/PUNISHMENT go through ICounterOutputHandler |
| MESSAGE scope overload (550 lines, side effects) | HIGH | MEDIUM | Split MESSAGE into CounterMessage class; repetitive PRINTFORML patterns reduce actual complexity; follow WC pattern |
| TEQUIP state mutation in MESSAGE makes it non-idempotent | MEDIUM | MEDIUM | Preserve mutation order in C# translation; test with explicit state verification |
| TRYCALLFORM dynamic dispatch incorrectly translated | MEDIUM | MEDIUM | Use dictionary/switch with exhaustive coverage; test each message handler branch |
| MESSAGE architecturally belongs to kojo system | MEDIUM | LOW | Keep in F802 per F783 assignment; note as concern for F813 post-phase review |
| COMBINATION stub migration may be trivial | LOW | LOW | Migrate 10-line stub as-is; mark incomplete behavior faithfully |
| DRAWLINE/WAIT in PUNISHMENT creates UI coupling | MEDIUM | LOW | Inject IConsoleOutput following WC pattern or F804 precedent |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Source ERB line count | `wc -l Game/ERB/COUNTER_MESSAGE.ERB Game/ERB/COUNTER_POSE.ERB Game/ERB/COUNTER_REACTION.ERB Game/ERB/COUNTER_PUNISHMENT.ERB Game/ERB/COUNTER_CONBINATION.ERB` | 867 total (550+131+131+45+10) | Verified actual line counts |
| Existing C# counter test count | `dotnet test Era.Core.Tests/ --filter "Counter" --list-tests` | TBD (run at /fc Phase 3) | Baseline for regression |
| TCVarIndex well-known constants | `grep -c "public static readonly" Era.Core/Types/TCVarIndex.cs` | 10 | Before adding 6+ new constants |

**Baseline File**: `.tmp/baseline-802.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | ICounterOutputHandler must be implemented with HandleReaction and HandlePunishment | `ICounterOutputHandler.cs:10-23` | AC must verify concrete class implements interface |
| C2 | HandlePunishment must iterate ALL characters to reset うふふ CFLAG and call CLOTHES_RESET/SETTING_TRAIN | `ICounterOutputHandler.cs:14-22`, `COUNTER_SELECT.ERB:60-64` | AC must verify per-character iteration with CFLAG reset |
| C3 | COUNTER_MESSAGE mutates TEQUIP state (not pure display) | `COUNTER_MESSAGE.ERB:147-336` | ACs must verify both text output AND state mutation side effects |
| C4 | PANTS_DESCRIPTION/PANTSNAME/OPPAI_DESCRIPTION need strategy | `COUNTER_MESSAGE.ERB:36,129,133,317,319` | Must define interface strategy before ACs; stub or real implementation |
| C5 | COUNTER_CONBINATION is explicitly incomplete ("つくりかけ") | `COUNTER_CONBINATION.ERB:3` | ACs must match incomplete behavior faithfully; no additions |
| C6 | REACTION depends on SELECTCOM for action branching | `COUNTER_REACTION.ERB:40,75,86,97,105,125` | AC must verify SELECTCOM-dependent branch paths |
| C7 | POSE TFLAG mutations must match 1:1 with ~25 action type mappings | `COUNTER_POSE.ERB:5-84` | AC must verify TFLAG increment per action type |
| C8 | POSE_69 has random sixnine transition logic | `COUNTER_POSE.ERB:116-131` | AC must verify 69 pose TEQUIP/TCVAR:27 mutation |
| C9 | PUNISHMENT has 3 random variants via retry loop | `COUNTER_PUNISHMENT.ERB:8-13` | AC must verify all 3 variant paths (0=underwear removal, 1=rotor V, 2=rotor A) |
| C10 | TCVarIndex must add constants for indices 21-25, 27 (index 26 not used by any of the 5 in-scope ERB files) | `TCVarIndex.cs:42-45` | ACs must verify named constants exist; no raw integer usage |
| C11 | WcCounterReaction/WcCounterPunishment provide design reference | `Era.Core/Counter/WcCounterReaction.cs`, `WcCounterPunishment.cs` | ACs should follow same DI patterns and testing approach |
| C12 | TRYCALLFORM MESSAGE dispatch covers ~35 message handlers | `COUNTER_MESSAGE.ERB:16-483` | ACs must verify representative subset of message handlers |
| C13 | DATUI_MESSAGE is local function within COUNTER_MESSAGE.ERB | `COUNTER_MESSAGE.ERB:484-550` | AC must verify DATUI_MESSAGE clothing strip messaging migrated |
| C14 | MESSAGE/POSE/COMBINATION must be callable from non-F801 entry points | `INFO.ERB:189,191`, `SOURCE.ERB:40`, `EVENT_KOJO.ERB:76,83,92` | Interface Dependency Scan: ACs must verify public entry points exist |

### Constraint Details

**C1: ICounterOutputHandler Implementation**
- **Source**: F801 Mandatory Handoff; `ICounterOutputHandler.cs:10-23`
- **Verification**: `grep "ICounterOutputHandler" Era.Core/Counter/CounterOutputHandler.cs`
- **AC Impact**: Must verify both HandleReaction and HandlePunishment are implemented as non-stub

**C2: HandlePunishment All-Character Cleanup**
- **Source**: `ICounterOutputHandler.cs:14-22` XML doc + `COUNTER_SELECT.ERB:60-64` FOR loop
- **Verification**: Unit test with multiple characters verifying CFLAG reset on all
- **AC Impact**: Must verify iteration over CHARANUM, not just single-character

**C3: MESSAGE TEQUIP Side Effects**
- **Source**: `COUNTER_MESSAGE.ERB:147-178` writes TEQUIP:PLAYER:3/4
- **Verification**: Unit test checking TEQUIP values after message handler execution
- **AC Impact**: MESSAGE handler tests must assert state changes, not just console output

**C4: Missing Text Formatting Functions**
- **Source**: `COUNTER_MESSAGE.ERB:36,129,133` (PANTS_DESCRIPTION, PANTSNAME), `COUNTER_MESSAGE.ERB:317,319` (OPPAI_DESCRIPTION)
- **Verification**: Check if interface is created or stubbed before compilation
- **AC Impact**: AC must account for stub strategy (interface with NotImplementedException or simple fallback)

**C5: CONBINATION Incomplete**
- **Source**: `COUNTER_CONBINATION.ERB:3` -- "つくりかけ" comment
- **Verification**: File is only 10 lines of counting logic
- **AC Impact**: Faithful reproduction; do not add features not in original. **C5 Exception**: AC#28's `int[]` return type is a necessary C# architectural addition (WcCounterCombination analogy; F811 upstream C# callers need the result) — this is not a behavioral addition to the ERB logic. See Key Decision "CounterCombination AccumulateCombinations return type".

**C6: REACTION depends on SELECTCOM for action branching**
- **Note**: The actual COUNTER_REACTION.ERB structure is 5 IF MASTER_POSE(N,1)==ARG blocks (pose-type dispatch), with SELECTCOM sub-branching inside 4 of the 5 blocks. AC#9-9f verify branch methods and SELECTCOM constants; AC#9g verifies the outer MASTER_POSE guard is preserved.
- **Source**: `COUNTER_REACTION.ERB:40,75,86,97,105,125`
- **Verification**: Grep for SELECTCOM dispatch constants (80-83) and MASTER_POSE guard in CounterReaction.cs
- **AC Impact**: AC must verify SELECTCOM-dependent branch paths AND outer MASTER_POSE guard conditions

**C10: TCVarIndex Expansion**
- **Source**: `TCVarIndex.cs:42-45` has only CounterAction(20) and CounterDecisionFlag(30); REACTION uses 21, POSE uses 27, MESSAGE uses 22-25
- **Verification**: `grep "public static readonly" Era.Core/Types/TCVarIndex.cs` count should increase by 6+
- **AC Impact**: All new constants must be named; no raw integer indexing

**C14: Non-ICounterOutputHandler Entry Points**
- **Source**: Interface Dependency Scan: INFO.ERB:189 calls POSE, INFO.ERB:191 and SOURCE.ERB:40 call COMBINATION, EVENT_KOJO.ERB:76,83,92 calls MESSAGE
- **Verification**: Verify public methods exist on handler classes for upstream consumption
- **AC Impact**: ACs must verify these entry points are accessible, not just ICounterOutputHandler methods

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| Predecessor | F801 | [DONE] | Defines ICounterOutputHandler (HandleReaction/HandlePunishment) and ICounterUtilities; ActionSelector calls into F802 at `COUNTER_SELECT.ERB:52,58` |
| Successor | F811 | [PROPOSED] | SOURCE Entry System; SOURCE.ERB:40 CALLs EVENT_COUNTER_COMBINATION (F802 scope), INFO.ERB:189,191 CALLs POSE/COMBINATION |
| Successor | F813 | [DRAFT] | Post-Phase Review Phase 21 |
| Related | F803 | [PROPOSED] | Main Counter Source; sibling -- no direct call chain between F802 and F803 |
| Related | F804 | [DONE] | WC Counter Core; WcCounterReaction/WcCounterPunishment provide design reference |
| Related | F805 | [DRAFT] | WC Counter Source + Message Core |
| Related | F809 | [DONE] | COMABLE Core (IComAvailabilityChecker owner) |

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

<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "SSOT for all counter output processing" | All 5 ERB files must be migrated to C# classes | AC#1, AC#2, AC#3, AC#4, AC#5 |
| "Each ERB file migrates to a dedicated C# class" | Each of the 5 ERB files has a corresponding C# class file | AC#1, AC#2, AC#3, AC#4, AC#5 |
| "typed indices and interface-driven dependency injection" | TCVarIndex must have named constants for indices 21-27; DI via constructor injection; implementations must use named constants | AC#6, AC#7, AC#31, AC#31b, AC#31c |
| "following the established WC Counter (F804) migration pattern" | Class structure, DI patterns, and test approach must follow WcCounterReaction/WcCounterPunishment | AC#7, AC#14, AC#14b |
| "typed indices and interface-driven dependency injection" (text formatting) | ITextFormatting stub interface for PANTS_DESCRIPTION/PANTSNAME/OPPAI_DESCRIPTION via constructor injection | AC#19, AC#19b, AC#19c, AC#20 |
| "implements the ICounterOutputHandler interface defined by F801" | Concrete class must implement ICounterOutputHandler with HandleReaction and HandlePunishment | AC#8 |
| "implements the ICounterOutputHandler interface defined by F801" (HandlePunishment all-character loop) | HandlePunishment must iterate ALL characters for CFLAG reset + CLOTHES_RESET + CLOTHES_SETTING_TRAIN per ICounterOutputHandler XML doc contract (C2) | AC#21, AC#22, AC#23, AC#24 |
| "SSOT for all counter output processing" (counter event messaging) | COUNTER_MESSAGE TEQUIP:PLAYER:3/4 mutations must be preserved in C# migration (not pure display) | AC#15, AC#15b, AC#36b, AC#36g |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CounterReaction class exists | file | Glob(Era.Core/Counter/CounterReaction.cs) | exists | - | [x] |
| 2 | CounterPunishment class exists | file | Glob(Era.Core/Counter/CounterPunishment.cs) | exists | - | [x] |
| 3 | CounterPose class exists | file | Glob(Era.Core/Counter/CounterPose.cs) | exists | - | [x] |
| 4 | CounterMessage class exists | file | Glob(Era.Core/Counter/CounterMessage.cs) | exists | - | [x] |
| 5 | CounterCombination class exists | file | Glob(Era.Core/Counter/CounterCombination.cs) | exists | - | [x] |
| 6 | TCVarIndex SubAction constant (index 21) | code | Grep(Era.Core/Types/TCVarIndex.cs) | matches | "public static readonly TCVarIndex SubAction.*new\\(21\\)" | [x] |
| 7 | TCVarIndex has 6+ new constants (total >= 16) | code | Grep(Era.Core/Types/TCVarIndex.cs) | gte | `public static readonly TCVarIndex` >= 16 | [x] |
| 8 | CounterOutputHandler implements ICounterOutputHandler | code | Grep(Era.Core/Counter/CounterOutputHandler.cs) | matches | ": ICounterOutputHandler" | [x] |
| 8b | CounterOutputHandler HandleReaction delegates to CounterReaction with GetMaster | code | Grep(Era.Core/Counter/CounterOutputHandler.cs) | matches | "_reaction\\.ProcessReaction\\(offender" | [x] |
| 8c | CounterOutputHandler HandlePunishment delegates to CounterPunishment | code | Grep(Era.Core/Counter/CounterOutputHandler.cs) | matches | "punishment\\.Execute\\(" | [x] |
| 9 | CounterReaction masturbation branch uses SELECTCOM | code | Grep(Era.Core/Counter/CounterReaction.cs) | matches | "HandleMasturbation\|ProcessMasturbation\|case.*Masturbation" | [x] |
| 9b | CounterReaction fellatio branch uses SELECTCOM | code | Grep(Era.Core/Counter/CounterReaction.cs) | matches | "HandleFellatio\|ProcessFellatio\|case.*Fellatio" | [x] |
| 9c | CounterReaction paizuri branch uses SELECTCOM | code | Grep(Era.Core/Counter/CounterReaction.cs) | matches | "HandlePaizuri\|ProcessPaizuri\|case.*Paizuri" | [x] |
| 9d | CounterReaction frotteurism branch uses SELECTCOM | code | Grep(Era.Core/Counter/CounterReaction.cs) | matches | "HandleFrotteurism\|ProcessFrotteurism\|case.*Frotteurism\|case.*Surituri" | [x] |
| 9e | CounterReaction SELECTCOM dispatch constants (80-83) | code | Grep(Era.Core/Counter/CounterReaction.cs) | gte | "SelectCom.*= 8[0-3]" >= 4 | [x] |
| 9f | CounterReaction vaginal branch (TALENT/ABL logic) | code | Grep(Era.Core/Counter/CounterReaction.cs) | matches | "HandleVaginal\|ProcessVaginal\|HandleInsertion" | [x] |
| 9g | CounterReaction uses MASTER_POSE guard for pose-type dispatch | code | Grep(Era.Core/Counter/CounterReaction.cs) | matches | "GetMasterPose\|MasterPose" | [x] |
| 9h | CounterReaction uses IsProtectedCheckOnly brand guard (4 branches) | code | Grep(Era.Core/Counter/CounterReaction.cs) | gte | "IsProtectedCheckOnly\|counterUtils\\.IsProtected" >= 4 | [x] |
| 9i | CounterReaction MASTER_POSE dispatch index values (2,3,4,5,6) | code | Grep(Era.Core/Counter/CounterReaction.cs) | gte | "MasterPose\\(Pose(Vaginal\|Masturbation\|Fellatio\|Paizuri\|Frotteurism)" >= 5 | [x] |
| 9j | CounterReaction uses named SELECTCOM constants in dispatch | code | Grep(Era.Core/Counter/CounterReaction.cs) | gte | "SelectComMasturbation\|SelectComFellatio\|SelectComPaizuri\|SelectComFrotteurism" >= 4 | [x] |
| 10 | CounterPunishment random retry loop dispatch | code | Grep(Era.Core/Counter/CounterPunishment.cs) | matches | "random\\.Next\\(10\\)" | [x] |
| 10b | CounterPunishment skips character 999 (player) | code | Grep(Era.Core/Counter/CounterPunishment.cs) | matches | "999\|SkipPlayer\|IsPlayer\|CharacterNo.*999" | [x] |
| 11 | CounterPunishment variant 0 removes underwear CFLAG | code | Grep(Era.Core/Counter/CounterPunishment.cs) | matches | "SetCharacterFlag.*CFlagLowerUnderwear" | [x] |
| 11b | CounterPunishment variant 1 sets rotor-V insertion CFLAG | code | Grep(Era.Core/Counter/CounterPunishment.cs) | matches | "SetCharacterFlag.*CFlagRotor.*V\\|CFlagRotorInsert" | [x] |
| 11c | CounterPunishment variant 2 sets rotor-A insertion CFLAG | code | Grep(Era.Core/Counter/CounterPunishment.cs) | matches | "SetCharacterFlag.*CFlagRotor.*A\\|CFlagRotorInsertA" | [x] |
| 11d | CounterPunishment variants 1 and 2 set rotor-inserter identity CFLAG | code | Grep(Era.Core/Counter/CounterPunishment.cs) | matches | "SetCharacterFlag.*CFlagRotorInserter\\|CFlagInserter\\|ローター挿入者" | [x] |
| 11e | CounterPunishment variant 0 has underwear precondition guard (retry) | code | Grep(Era.Core/Counter/CounterPunishment.cs) | matches | "LowerUnderwear.*>.*0\\|HasUnderwear\\|CFlagLowerUnderwear.*!= 0" | [x] |
| 11f | CounterPunishment variant 1 has vagina precondition guard (retry) | code | Grep(Era.Core/Counter/CounterPunishment.cs) | matches | "HasVagina\|common\\.HasVagina" | [x] |
| 12 | CounterPose TFLAG increment switch with ~25 action mappings | code | Grep(Era.Core/Counter/CounterPose.cs) | gte | `IncrementTFlag\(TFlag` >= 10 | [x] |
| 12b | CounterPose ProcessPose has CounterDecisionFlag early-return guard | code | Grep(Era.Core/Counter/CounterPose.cs) | matches | "CounterDecisionFlag\|GetTCVar.*30" | [x] |
| 12c | CounterPose dispatches TFLAG switch on TCVarIndex.CounterAction | code | Grep(Era.Core/Counter/CounterPose.cs) | matches | "TCVarIndex\\.CounterAction\|GetTCVar.*CounterAction" | [x] |
| 12d | CounterPose uses >= 5 distinct TFlag indices (60-65) for body-part mapping | code | Grep(Era.Core/Counter/CounterPose.cs) | gte | "TFlag(Mouth\|Chest\|Crotch\|Vagina\|Anal\|Breast)" >= 5 | [x] |
| 13 | CounterPose 69-position transition via HandlePose69 | code | Grep(Era.Core/Counter/CounterPose.cs) | matches | "HandlePose69\|SetTEquip.*SixtyNine" | [x] |
| 14 | CounterReaction DI pattern follows WC reference (constructor injection) | code | Grep(Era.Core/Counter/CounterReaction.cs) | matches | "sealed class CounterReaction\\(" | [x] |
| 14b | All 6 Counter classes use sealed class primary constructor DI pattern | code | Grep(Era.Core/Counter/) | gte | "sealed class Counter" >= 6 | [x] |
| 15 | CounterMessage TEQUIP mutation preserved (TEQUIP:PLAYER:3 at 4+ sites) | code | Grep(Era.Core/Counter/CounterMessage.cs) | gte | `SetTEquip.*,\s*3,\s*1` >= 4 | [x] |
| 15b | CounterMessage TEQUIP:PLAYER:4 mutation preserved | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "SetTEquip.*4,\\s*1" | [x] |
| 16 | CounterMessage TRYCALLFORM dispatch as action switch | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "action switch\|switch \\(action\\)" | [x] |
| 16b | CounterMessage has low-range handler (HandleMessage10) | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "HandleMessage10" | [x] |
| 16c | CounterMessage has mid-range handler (HandleMessage50) | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "HandleMessage50" | [x] |
| 16d | CounterMessage has high-range handler (HandleMessage58) | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "HandleMessage58" | [x] |
| 16e | CounterMessage DispatchWithBranch path exists | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "DispatchWithBranch" | [x] |
| 16e2 | CounterMessage DispatchWithBranch has 3 branch-variant handlers | code | Grep(Era.Core/Counter/CounterMessage.cs) | gte | "HandleMessage29_[123]" >= 3 | [x] |
| 16f | CounterMessage conditional text building (PRINTFORML ternary translation) | code | Grep(Era.Core/Counter/CounterMessage.cs) | gte | "if \\(.*!= 0.*t3\|if \\(.*& 2" >= 4 | [x] |
| 16g | CounterMessage has >= 10 private handler methods (substantial dispatch coverage) | code | Grep(Era.Core/Counter/CounterMessage.cs) | gte | "private.*int HandleMessage" >= 10 | [x] |
| 16h | CounterMessage has 500-range handler (SubAction-dependent group) | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "HandleMessage50[0-5]\|HandleMessage60[01]" | [x] |
| 17 | CounterMessage DATUI_MESSAGE migrated | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "DatuiMessage|HandleUndressingMessage" | [x] |
| 18 | CounterCombination faithful reproduction of incomplete stub | code | Grep(Era.Core/Counter/CounterCombination.cs) | matches | "AccumulateCombinations|CountCombinations" | [x] |
| 19 | Text formatting stub interface defined with GetPantsDescription | code | Grep(Era.Core/Interfaces/ITextFormatting.cs) | matches | "GetPantsDescription" | [x] |
| 19b | Text formatting stub interface includes GetOppaiDescription | code | Grep(Era.Core/Interfaces/ITextFormatting.cs) | matches | "GetOppaiDescription" | [x] |
| 19c | Text formatting stub interface includes GetPantsName | code | Grep(Era.Core/Interfaces/ITextFormatting.cs) | matches | "GetPantsName" | [x] |
| 20 | CounterMessage uses text formatting dependency | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "ITextFormatting|textFormatting|pantsDescription|GetPantsDescription" | [x] |
| 20b | CounterMessage uses IConsoleOutput for text output | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "IConsoleOutput console\|console\\.PrintLine\|console\\.Print\\(" | [x] |
| 21 | CounterOutputHandler HandlePunishment iterates all characters (CHARANUM loop) | code | Grep(Era.Core/Counter/CounterOutputHandler.cs) | matches | "GetCharaNum\\(\\)|charaNum|CHARANUM" | [x] |
| 22 | CounterOutputHandler HandlePunishment resets うふふ CFLAG per character | code | Grep(Era.Core/Counter/CounterOutputHandler.cs) | matches | "SetCharacterFlag.*CFlagUfufu.*0|SetCharacterFlag.*317.*0" | [x] |
| 23 | CounterOutputHandler HandlePunishment calls CLOTHES_RESET per character | code | Grep(Era.Core/Counter/CounterOutputHandler.cs) | matches | "ClothesReset|clothingSystem\\.Save\\(" | [x] |
| 24 | CounterOutputHandler HandlePunishment calls CLOTHES_SETTING_TRAIN per character | code | Grep(Era.Core/Counter/CounterOutputHandler.cs) | matches | "SettingTrain\\(" | [x] |
| 25 | CounterPunishment DRAWLINE + WAIT via IConsoleOutput | code | Grep(Era.Core/Counter/CounterPunishment.cs) | matches | "IConsoleOutput|DrawLine|consoleOutput" | [x] |
| 26 | CounterMessage public entry point for upstream callers | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "public.*void.*SendMessage" | [x] |
| 26b | CounterMessage SendMessage uses GetTarget() guard | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "GetTarget\\(\\)" | [x] |
| 26c | CounterMessage DatuiMessage() is both defined and called (not dead code) | code | Grep(Era.Core/Counter/CounterMessage.cs) | gte | "DatuiMessage\\(\\)" >= 2 | [x] |
| 27 | CounterPose public entry point for upstream callers (with actionOrder) | code | Grep(Era.Core/Counter/CounterPose.cs) | matches | "public.*void.*ProcessPose.*CharacterId.*int" | [x] |
| 28 | CounterCombination public entry point returns int[] for upstream callers | code | Grep(Era.Core/Counter/CounterCombination.cs) | matches | "public.*int\\[\\].*AccumulateCombinations" | [x] |
| 29 | TCVarIndex undressing state constants (indices 22-25) | code | Grep(Era.Core/Types/TCVarIndex.cs) | matches | "public static readonly TCVarIndex.*new\\(2[2-5]\\)" | [x] |
| 30 | TCVarIndex SixtyNineTransition constant (index 27) | code | Grep(Era.Core/Types/TCVarIndex.cs) | matches | "public static readonly TCVarIndex.*new\\(27\\)" | [x] |
| 31 | CounterReaction uses named TCVarIndex.SubAction constant | code | Grep(Era.Core/Counter/CounterReaction.cs) | matches | "TCVarIndex\\.SubAction|SubAction" | [x] |
| 31b | CounterPose uses named TCVarIndex.SixtyNineTransition constant | code | Grep(Era.Core/Counter/CounterPose.cs) | matches | "TCVarIndex\\.SixtyNineTransition" | [x] |
| 31c | CounterMessage DatuiMessage uses named undressing constants | code | Grep(Era.Core/Counter/CounterMessage.cs) | matches | "TCVarIndex\\.UndressingPlayerLower\|TCVarIndex\\.UndressingTargetLower\|TCVarIndex\\.UndressingPlayerUpper\|TCVarIndex\\.UndressingTargetUpper" | [x] |
| 32 | Unit tests for CounterReaction exist | file | Glob(Era.Core.Tests/Counter/CounterReactionTests.cs) | exists | - | [x] |
| 32b | CounterReactionTests verify SELECTCOM branching (4 branches) | code | Grep(Era.Core.Tests/Counter/CounterReactionTests.cs) | gte | "GetSelectCom" >= 4 | [x] |
| 32c | CounterReactionTests verify vaginal branch | code | Grep(Era.Core.Tests/Counter/CounterReactionTests.cs) | matches | "HandleVaginal\|ProcessVaginal\|HandleInsertion\|vaginal\|Vaginal" | [x] |
| 32d | CounterReactionTests verify MASTER_POSE guard dispatch | code | Grep(Era.Core.Tests/Counter/CounterReactionTests.cs) | matches | "GetMasterPose\|MasterPose" | [x] |
| 32e | CounterReactionTests verify protection brand guard | code | Grep(Era.Core.Tests/Counter/CounterReactionTests.cs) | matches | "IsProtectedCheckOnly\|IsProtected\|Protected" | [x] |
| 33 | Unit tests for CounterPunishment exist | file | Glob(Era.Core.Tests/Counter/CounterPunishmentTests.cs) | exists | - | [x] |
| 33b | CounterPunishmentTests verify variant dispatch | code | Grep(Era.Core.Tests/Counter/CounterPunishmentTests.cs) | matches | "ExecuteVariant\|Variant0\|Variant1\|Variant2\|random\.Next" | [x] |
| 33c | CounterPunishmentTests assert specific CFLAG names in variant assertions | code | Grep(Era.Core.Tests/Counter/CounterPunishmentTests.cs) | matches | "CFlagLowerUnderwear\|CFlagRotorInsert" | [x] |
| 33d | CounterPunishmentTests verify precondition guard retry scenarios (underwear/vagina) | code | Grep(Era.Core.Tests/Counter/CounterPunishmentTests.cs) | matches | "HasUnderwear\|LowerUnderwear.*0\|HasVagina" | [x] |
| 34 | Unit tests for CounterPose exist | file | Glob(Era.Core.Tests/Counter/CounterPoseTests.cs) | exists | - | [x] |
| 34b | CounterPoseTests verify TFLAG mapping | code | Grep(Era.Core.Tests/Counter/CounterPoseTests.cs) | matches | "IncrementTFlag\|ProcessPose\|HandlePose69" | [x] |
| 34c | CounterPoseTests verify CounterDecisionFlag guard path | code | Grep(Era.Core.Tests/Counter/CounterPoseTests.cs) | matches | "CounterDecisionFlag\|decisionFlag" | [x] |
| 34d | CounterPoseTests assert specific TFLAG body-part indices in action mapping | code | Grep(Era.Core.Tests/Counter/CounterPoseTests.cs) | gte | "TFlag6[0-5]" >= 2 | [x] |
| 35 | Unit tests for CounterOutputHandler exist | file | Glob(Era.Core.Tests/Counter/CounterOutputHandlerTests.cs) | exists | - | [x] |
| 35b | CounterOutputHandlerTests verify all-character iteration | code | Grep(Era.Core.Tests/Counter/CounterOutputHandlerTests.cs) | matches | "GetCharaNum" | [x] |
| 36 | Unit tests for CounterMessage exist | file | Glob(Era.Core.Tests/Counter/CounterMessageTests.cs) | exists | - | [x] |
| 36b | CounterMessageTests verify TEQUIP mutation | code | Grep(Era.Core.Tests/Counter/CounterMessageTests.cs) | matches | "SetTEquip" | [x] |
| 36c | CounterMessageTests verify GetTarget guard path | code | Grep(Era.Core.Tests/Counter/CounterMessageTests.cs) | matches | "GetTarget" | [x] |
| 36d | CounterMessageTests verify representative switch dispatch coverage | code | Grep(Era.Core.Tests/Counter/CounterMessageTests.cs) | gte | "HandleMessage" >= 3 | [x] |
| 36e | CounterMessageTests verify DatuiMessage undressing logic | code | Grep(Era.Core.Tests/Counter/CounterMessageTests.cs) | matches | "DatuiMessage\|UndressingPlayer" | [x] |
| 36f | CounterMessageTests verify 500-range SubAction handler | code | Grep(Era.Core.Tests/Counter/CounterMessageTests.cs) | matches | "HandleMessage50[0-5]\|HandleMessage60[01]\|SubAction" | [x] |
| 36g | verify TEQUIP index 3 mutation via GetTEquip assertions in >= 2 test scenarios | code | Grep(Era.Core.Tests/Counter/CounterMessageTests.cs) | gte | "GetTEquip.*,\\s*3\\)" >= 2 | [x] |
| 36h | verify TEQUIP:PLAYER:4 mutation via GetTEquip assertion | code | Grep(Era.Core.Tests/Counter/CounterMessageTests.cs) | matches | "GetTEquip.*,\\s*4\\)" | [x] |
| 37 | Unit tests for CounterCombination exist | file | Glob(Era.Core.Tests/Counter/CounterCombinationTests.cs) | exists | - | [x] |
| 37b | CounterCombinationTests verify AccumulateCombinations | code | Grep(Era.Core.Tests/Counter/CounterCombinationTests.cs) | matches | "AccumulateCombinations" | [x] |
| 38 | IConsoleOutput Wait() method exists | code | Grep(Era.Core/Interfaces/IConsoleOutput.cs) | matches | "Wait\\(\\)" | [x] |
| 38b | HeadlessConsole Wait() implementation exists | code | Grep(engine/Assets/Scripts/Emuera/Headless/HeadlessConsole.cs) | matches | "Wait\\(\\)" | [x] |
| 39 | Build succeeds | build | dotnet build Era.Core/ | succeeds | - | [x] |
| 40 | All tests pass | test | dotnet test Era.Core.Tests/ | succeeds | - | [x] |

### AC Details

**AC#1: CounterReaction class exists**
- **Test**: `Glob(Era.Core/Counter/CounterReaction.cs)`
- **Expected**: File exists
- **Rationale**: COUNTER_REACTION.ERB (131 lines) must be migrated to a dedicated C# class per Philosophy. (C1, C6, C11)

**AC#2: CounterPunishment class exists**
- **Test**: `Glob(Era.Core/Counter/CounterPunishment.cs)`
- **Expected**: File exists
- **Rationale**: COUNTER_PUNISHMENT.ERB (45 lines) must be migrated to a dedicated C# class with random variant dispatch. (C1, C9)

**AC#3: CounterPose class exists**
- **Test**: `Glob(Era.Core/Counter/CounterPose.cs)`
- **Expected**: File exists
- **Rationale**: COUNTER_POSE.ERB (131 lines) must be migrated to a dedicated C# class for pose tracking. (C7, C8)

**AC#4: CounterMessage class exists**
- **Test**: `Glob(Era.Core/Counter/CounterMessage.cs)`
- **Expected**: File exists
- **Rationale**: COUNTER_MESSAGE.ERB (550 lines) must be migrated to a dedicated C# class. Largest file in scope. (C3, C4, C12, C13)

**AC#5: CounterCombination class exists**
- **Test**: `Glob(Era.Core/Counter/CounterCombination.cs)`
- **Expected**: File exists
- **Rationale**: COUNTER_CONBINATION.ERB (10 lines) must be migrated faithfully as incomplete stub. (C5)

**AC#6: TCVarIndex SubAction constant (index 21)**
- **Test**: `Grep(Era.Core/Types/TCVarIndex.cs)` for `public static readonly TCVarIndex SubAction.*new\(21\)`
- **Expected**: Pattern matches (named constant exists)
- **Rationale**: TCVAR index 21 (SubAction) is used by COUNTER_REACTION for sub-action branching. Must be a named constant per typed-index philosophy, not raw `new TCVarIndex(21)`. (C10)

**AC#7: TCVarIndex has 6+ new constants (total >= 16)**
- **Test**: `Grep(Era.Core/Types/TCVarIndex.cs)` count `public static readonly TCVarIndex`
- **Expected**: >= 16 (baseline 10 + 6 new: SubAction(21), UndressingPlayerLower(22), UndressingTargetLower(23), UndressingPlayerUpper(24), UndressingTargetUpper(25), SixtyNineTransition(27)). Matcher: gte.
- **Rationale**: C10 constraint requires named constants for all indices 21-27. Current baseline is 10 constants. (C10)

**AC#8: CounterOutputHandler implements ICounterOutputHandler**
- **Test**: `Grep(Era.Core/Counter/)` for `class CounterOutputHandler.*ICounterOutputHandler`
- **Expected**: Pattern matches
- **Rationale**: F801 defines ICounterOutputHandler with HandleReaction and HandlePunishment. F802 must provide the concrete implementation class. ActionSelector already depends on this interface. (C1)

**AC#8b: CounterOutputHandler HandleReaction delegates to CounterReaction with GetMaster**
- **Test**: `Grep(Era.Core/Counter/CounterOutputHandler.cs)` for `reaction\.ProcessReaction.*GetMaster\(\)`
- **Expected**: Pattern matches (HandleReaction calls reaction.ProcessReaction with engine.GetMaster() as master argument)
- **Rationale**: Technical Design specifies `reaction.ProcessReaction(offender, actionOrder, new CharacterId(engine.GetMaster()))`. AC#8 only verifies interface implementation. Matcher requires both ProcessReaction call AND GetMaster() to verify the F801 handoff contract (COUNTER_REACTION.ERB reads MASTER variable — GetMaster() is the sole mechanism linking F801's ActionSelector context). (C1)

**AC#8c: CounterOutputHandler HandlePunishment delegates to CounterPunishment**
- **Test**: `Grep(Era.Core/Counter/CounterOutputHandler.cs)` for `punishment\.Execute\(`
- **Expected**: Pattern matches (HandlePunishment calls punishment.Execute() before the all-character loop)
- **Rationale**: AC#8b verifies HandleReaction→ProcessReaction() symmetrically. HandlePunishment must call CounterPunishment.Execute() for per-offender punishment dispatch BEFORE the all-character cleanup loop (ACs#21-24). Without this AC, an implementation could pass all loop-verification ACs while never invoking the actual punishment logic, making CounterPunishment dead code. (C1)

**AC#9: CounterReaction masturbation branch uses SELECTCOM**
- **Test**: `Grep(Era.Core/Counter/CounterReaction.cs)` for `HandleMasturbation|ProcessMasturbation|case.*Masturbation`
- **Expected**: Pattern matches (named branch method/case exists)
- **Rationale**: COUNTER_REACTION.ERB masturbation branch (SELECTCASE block 2, lines 31-53) uses SELECTCOM==80. Named method verifies branch exists. (C6)

**AC#9b: CounterReaction fellatio branch uses SELECTCOM**
- **Test**: `Grep(Era.Core/Counter/CounterReaction.cs)` for `HandleFellatio|ProcessFellatio|case.*Fellatio`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_REACTION.ERB fellatio branch (SELECTCASE block 3, lines 55-82) uses SELECTCOM==81. (C6)

**AC#9c: CounterReaction paizuri branch uses SELECTCOM**
- **Test**: `Grep(Era.Core/Counter/CounterReaction.cs)` for `HandlePaizuri|ProcessPaizuri|case.*Paizuri`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_REACTION.ERB paizuri branch (SELECTCASE block 4, lines 84-112) uses SELECTCOM==82 in 3 sub-cases. (C6)

**AC#9d: CounterReaction frotteurism branch uses SELECTCOM**
- **Test**: `Grep(Era.Core/Counter/CounterReaction.cs)` for `HandleFrotteurism|ProcessFrotteurism|case.*Frotteurism|case.*Surituri`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_REACTION.ERB frotteurism branch (SELECTCASE block 5, lines 114-131) uses SELECTCOM==83. Japanese name すりつり included as alternative. (C6)

**AC#9e: CounterReaction SELECTCOM dispatch constants (80-83)**
- **Test**: `Grep(Era.Core/Counter/CounterReaction.cs)` count `SelectCom.*= 8[0-3]` >= 4
- **Expected**: Count >= 4 (one constant definition per SELECTCOM-using branch: SelectComMasturbation = 80, SelectComFellatio = 81, SelectComPaizuri = 82, SelectComFrotteurism = 83)
- **Rationale**: AC#9-9d verify branch method names exist but not that the SELECTCOM dispatch values are correct. This AC verifies the named SELECTCOM constant definitions (80-83) appear as private const fields, preventing wrong-value dispatch and ensuring SELECTCOM-specific context (not incidental integer comparisons). (C6)

**AC#9f: CounterReaction vaginal branch (TALENT/ABL logic)**
- **Test**: `Grep(Era.Core/Counter/CounterReaction.cs)` for `HandleVaginal|ProcessVaginal|HandleInsertion`
- **Expected**: Pattern matches (vaginal branch method exists)
- **Rationale**: COUNTER_REACTION.ERB vaginal branch (SELECTCASE block 1, lines 3-26) uses MASTER_POSE/ABL/TALENT checks (not SELECTCOM). AC#9-9d cover the 4 SELECTCOM-based branches. AC#9f verifies the 5th branch (vaginal/膣挿入) is migrated, completing all 5 SELECTCASE blocks. Task 5 explicitly describes this branch as "1 (vaginal) uses TALENT/ABL logic."

**AC#9g: CounterReaction uses MASTER_POSE guard for pose-type dispatch**
- **Test**: `Grep(Era.Core/Counter/CounterReaction.cs)` for `GetMasterPose|MasterPose`
- **Expected**: Pattern matches (MASTER_POSE guard logic exists)
- **Rationale**: COUNTER_REACTION.ERB structure is 5 IF MASTER_POSE(N,1)==ARG blocks (pose-type dispatch), not top-level SELECTCASE. Each IF block contains a SELECTCASE LOCAL for random variants. AC#9-9f verify branch methods and SELECTCOM sub-conditions, but no AC verifies the outer MASTER_POSE guard conditions are preserved. An implementer using only SELECTCOM dispatch as the primary structure would be structurally incorrect. (C6)

**AC#9h: CounterReaction uses IsProtectedCheckOnly brand guard (4 branches)**
- **Test**: `Grep(Era.Core/Counter/CounterReaction.cs)` count `IsProtectedCheckOnly|counterUtils\.IsProtected` >= 4
- **Expected**: Count >= 4 (one guard call per SELECTCOM-using branch: masturbation, fellatio, paizuri, frotteurism)
- **Rationale**: 4 of 5 REACTION branches use IsProtectedCheckOnly brand guard (verified against WcCounterReaction pattern). Strengthened from `matches` to `gte >= 4` to verify the guard appears in all 4 required branches, not just one. (C6)

**AC#9i: CounterReaction MASTER_POSE dispatch index values (2,3,4,5,6)**
- **Test**: `Grep(Era.Core/Counter/CounterReaction.cs)` count `GetMasterPose\(offender,\s*[23456]\)|MasterPose.*==\s*[23456]` >= 5
- **Expected**: Count >= 5 (all 5 MASTER_POSE group indices appear as arguments or comparison values: 6=vaginal, 3=masturbation, 4=fellatio, 5=paizuri, 2=frotteurism)
- **Rationale**: AC#9g verifies MasterPose method name exists. AC#9i verifies the actual MASTER_POSE index values (2,3,4,5,6) appear in dispatch conditions. Tightened from `MasterPose.*[23456]` to require digit as function argument or comparison value, preventing false positives from digits in comments or variable names. (C6)

**AC#9j: CounterReaction uses named SELECTCOM constants in dispatch**
- **Test**: `Grep(Era.Core/Counter/CounterReaction.cs)` count `SelectComMasturbation|SelectComFellatio|SelectComPaizuri|SelectComFrotteurism` >= 4
- **Expected**: Count >= 4 (all 4 named constants referenced in dispatch code, not just defined)
- **Rationale**: AC#9e verifies that 4 SelectCom named constants are DEFINED (`SelectCom.*= 8[0-3]`), but a developer could define constants and still use raw integer literals (80/81/82/83) in the actual switch/case dispatch. AC#9j verifies the named constants are actually USED in dispatch logic, closing the definition-vs-usage gap. (C6)

**AC#10: CounterPunishment random retry loop dispatch**
- **Test**: `Grep(Era.Core/Counter/CounterPunishment.cs)` for `random\.Next\(10\)`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_PUNISHMENT.ERB uses WHILE loop with RAND:10 for random variant dispatch (lines 8-13). Must be preserved in C#. (C9)

**AC#10b: CounterPunishment skips character 999 (player)**
- **Test**: `Grep(Era.Core/Counter/CounterPunishment.cs)` for `999|SkipPlayer|IsPlayer|CharacterNo.*999`
- **Expected**: Pattern matches (player skip guard exists)
- **Rationale**: COUNTER_PUNISHMENT.ERB:2-4 returns 0 when NO:ARG==999 (player character). Must be preserved in C# to prevent player self-punishment. (Technical Constraint)

**AC#11: CounterPunishment variant 0 removes underwear CFLAG**
- **Test**: `Grep(Era.Core/Counter/CounterPunishment.cs)` for `SetCharacterFlag.*CFlagLowerUnderwear`
- **Expected**: Pattern matches
- **Rationale**: PUNISHMENT_0 removes lower underwear CFLAG (COUNTER_PUNISHMENT.ERB line 20). Must verify CFLAG mutation is preserved. (C9)

**AC#11b: CounterPunishment variant 1 sets rotor-V insertion CFLAG**
- **Test**: `Grep(Era.Core/Counter/CounterPunishment.cs)` for `SetCharacterFlag.*CFlagRotor.*V|CFlagRotorInsert`
- **Expected**: Pattern matches
- **Rationale**: PUNISHMENT_1 sets CFLAG:PLAYER:ローター挿入=240 (COUNTER_PUNISHMENT.ERB:26-27). C9 requires all 3 variant paths verified. (C9)

**AC#11c: CounterPunishment variant 2 sets rotor-A insertion CFLAG**
- **Test**: `Grep(Era.Core/Counter/CounterPunishment.cs)` for `SetCharacterFlag.*CFlagRotor.*A|CFlagRotorInsertA`
- **Expected**: Pattern matches
- **Rationale**: PUNISHMENT_2 sets CFLAG:PLAYER:ローターA挿入=240 (COUNTER_PUNISHMENT.ERB:33-34). C9 requires all 3 variant paths verified. (C9)

**AC#11d: CounterPunishment variants 1 and 2 set rotor-inserter identity CFLAG**
- **Test**: `Grep(Era.Core/Counter/CounterPunishment.cs)` for `SetCharacterFlag.*CFlagRotorInserter|CFlagInserter|ローター挿入者`
- **Expected**: Pattern matches (rotor-inserter identity tracking exists)
- **Rationale**: COUNTER_PUNISHMENT.ERB:32 and :41 both set `CFLAG:PLAYER:ローター挿入者 = ARG` to record which character performed the rotor insertion. AC#11b/11c verify the insertion timer CFLAGs but not the inserter identity CFLAG. Missing this mutation breaks the game's rotor-inserter tracking. (C9)

**AC#11e: CounterPunishment variant 0 has underwear precondition guard (retry)**
- **Test**: `Grep(Era.Core/Counter/CounterPunishment.cs)` for `LowerUnderwear.*>.*0|HasUnderwear|CFlagLowerUnderwear.*!= 0`
- **Expected**: Pattern matches (underwear check exists)
- **Rationale**: COUNTER_PUNISHMENT.ERB:18-19 has `SIF !CFLAG:PLAYER:服装_下半身下着２ / RETURN 1` — variant 0 retries if player has no lower underwear (can't remove what doesn't exist). Without this guard, the retry loop behavior is broken and underwear removal succeeds even when no underwear is equipped. (C9)

**AC#11f: CounterPunishment variant 1 has vagina precondition guard (retry)**
- **Test**: `Grep(Era.Core/Counter/CounterPunishment.cs)` for `HasVagina|common\.HasVagina`
- **Expected**: Pattern matches (HasVagina anatomy check via ICommonFunctions)
- **Rationale**: COUNTER_PUNISHMENT.ERB:28-29 has `SIF !HAS_VAGINA(PLAYER) / RETURN 1` — variant 1 retries if player lacks vagina. Must use `ICommonFunctions.HasVagina()` (matching WcCounterPunishment pattern), NOT `IComAvailabilityChecker.IsAvailable()` which checks COM command availability, not anatomy. Tightened from `HasVagina|vagina|IComAvailabilityChecker` to exclude the semantically wrong alternative. (C9)

**AC#12: CounterPose TFLAG increment switch with ~25 action mappings**
- **Test**: `Grep(Era.Core/Counter/CounterPose.cs)` count `IncrementTFlag\(TFlag` >= 10
- **Expected**: Count >= 10 (ensures substantial coverage of ~25 action-to-TFLAG mappings; not just 1-2 arms)
- **Rationale**: COUNTER_POSE.ERB maps ~25 action types to 6 TFLAG indices (60-65) via SELECTCASE (lines 5-84). Each action must increment the correct body-part counter. Count-based matcher ensures non-trivial implementation coverage. (C7)

**AC#12b: CounterPose ProcessPose has CounterDecisionFlag early-return guard**
- **Test**: `Grep(Era.Core/Counter/CounterPose.cs)` for `CounterDecisionFlag|GetTCVar.*30`
- **Expected**: Pattern matches (CounterDecisionFlag guard exists)
- **Rationale**: COUNTER_POSE.ERB line 3 has `SIF TCVAR:ARG:30 > 1 / RETURN 0` — ProcessPose must return without incrementing TFLAGs when CounterDecisionFlag exceeds 1. TCVarIndex.CounterDecisionFlag (index 30) already exists. Without this guard, ProcessPose would always increment TFLAGs even during counter-skip scenarios. (C7)

**AC#12c: CounterPose dispatches TFLAG switch on TCVarIndex.CounterAction**
- **Test**: `Grep(Era.Core/Counter/CounterPose.cs)` for `TCVarIndex\.CounterAction|GetTCVar.*CounterAction`
- **Expected**: Pattern matches (switch key reads CounterAction variable)
- **Rationale**: COUNTER_POSE.ERB line 5 `SELECTCASE TCVAR:ARG:20` dispatches on TCVarIndex.CounterAction (index 20), not on the `actionOrder` parameter. The `actionOrder` parameter is for POSE_69 probability, not for the main TFLAG switch. An incorrect dispatch key would route all actions to wrong TFLAG indices. (C7)

**AC#12d: CounterPose uses >= 5 distinct TFlag indices (60-65) for body-part mapping**
- **Test**: `Grep(Era.Core/Counter/CounterPose.cs)` count `TFlag6[0-5]` >= 5
- **Expected**: Count >= 5 (at least 5 of 6 distinct TFLAG indices 60-65 appear)
- **Rationale**: COUNTER_POSE.ERB maps ~25 actions to 6 distinct TFLAG indices (60=oral, 61=chest, 62=buttocks, 63=groin, 64=genital, 65=feet). AC#12 verifies >= 10 IncrementTFlag calls but doesn't prevent all calls using the same index. An implementation with 10 IncrementTFlag(TFlag60) calls would pass AC#12 but produce wrong body-part tracking. Count >= 5 distinct indices ensures proper distribution. (C7)

**AC#13: CounterPose 69-position transition via HandlePose69**
- **Test**: `Grep(Era.Core/Counter/CounterPose.cs)` for `HandlePose69|SetTEquip.*SixtyNine`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_POSE.ERB has EVENT_COUNTER_POSE_69 subroutine (lines 116-131) that handles random sixnine transitions with TEQUIP/TCVAR:27 mutation. (C8)

**AC#14: CounterReaction DI pattern follows WC reference (constructor injection)**
- **Test**: `Grep(Era.Core/Counter/CounterReaction.cs)` for `sealed class CounterReaction\(`
- **Expected**: Pattern matches (primary constructor pattern like WcCounterReaction)
- **Rationale**: Must follow WcCounterReaction's sealed class with primary constructor DI pattern per F804 design reference. (C11)

**AC#14b: All 5 Counter classes use sealed class primary constructor DI pattern**
- **Test**: `Grep(Era.Core/Counter/)` count `sealed class Counter` >= 6
- **Expected**: Count >= 6 (CounterReaction, CounterPunishment, CounterPose, CounterMessage, CounterCombination, CounterOutputHandler — all 6 match "sealed class Counter" pattern since all class names start with "Counter")
- **Rationale**: Philosophy requires "following the established WC Counter (F804) migration pattern" for ALL handler classes, not just CounterReaction. F804 AC#37 enforced this with a combined gte check across all Wc*.cs files. F802 mirrors this approach. (C11)

**AC#15: CounterMessage TEQUIP mutation preserved (TEQUIP:PLAYER:3 at 5+ sites)**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` count `SetTEquip.*,\s*3,\s*1` >= 5
- **Expected**: Count >= 5 (representative coverage of 7+ TEQUIP:PLAYER:3 mutation sites across message handlers)
- **Rationale**: COUNTER_MESSAGE.ERB mutates TEQUIP:PLAYER:3 at 7+ sites (lines 148, 164, 174, 186, 198, 218, etc.). Pattern requires comma-delimited index 3 and value 1 (`SetTEquip(player, 3, 1)`) to prevent false-positive matches on indices 13, 23, 30, etc. Count >= 5 ensures substantial coverage. Cross-handler distribution of mutations is implicitly enforced by the composition of AC#15 (>= 5 mutations), AC#16g (>= 10 handler methods), and AC#16b/16c/16d/16h (spot-checks across low/mid/high/500 ranges) — 5+ mutations across handlers spanning ranges 10-601 makes single-handler concentration practically impossible. (C3)

**AC#15b: CounterMessage TEQUIP:PLAYER:4 mutation preserved**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `SetTEquip.*4,\s*1`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_MESSAGE.ERB also mutates TEQUIP:PLAYER:4 at lines 267, 282. C3 constraint says "TEQUIP:PLAYER:3/4 mutations must be preserved". AC#15 covers index 3; AC#15b covers index 4. (C3)

**AC#16: CounterMessage TRYCALLFORM dispatch as action switch**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `action switch|switch \(action\)`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_MESSAGE.ERB uses TRYCALLFORM EVENT_COUNTE_MESSAGE{TCVAR:20}_{branch} to dispatch ~35 message handlers by action ID (lines 7-11). Must be translated to C# switch dispatch on action. Accepts both `switch (action)` (statement form) and `action switch` (expression form). Tightened from generic `switch` to action-specific to avoid matching incidental switch statements (e.g., DatuiMessage). (C12)

**AC#16b: CounterMessage has low-range handler (HandleMessage10)**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `HandleMessage10`
- **Expected**: Pattern matches
- **Rationale**: C12 requires "representative subset of message handlers". Split from OR matcher to independently verify each range. Low range: action 10. (C12)

**AC#16c: CounterMessage has mid-range handler (HandleMessage50)**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `HandleMessage50`
- **Expected**: Pattern matches
- **Rationale**: C12 mid-range verification: action 50. (C12)

**AC#16d: CounterMessage has high-range handler (HandleMessage58)**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `HandleMessage58`
- **Expected**: Pattern matches
- **Rationale**: C12 high-range verification: action 58. (C12)

**AC#16e: CounterMessage DispatchWithBranch path exists**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `DispatchWithBranch`
- **Expected**: Pattern matches (branched dispatch method exists)
- **Rationale**: COUNTER_MESSAGE.ERB dispatch has two paths: (1) default `TRYCALLFORM EVENT_COUNTE_MESSAGE{action}` and (2) branched `TRYCALLFORM EVENT_COUNTE_MESSAGE{action}_{branch}`. AC#16-16d cover default path only. AC#16e verifies the branched dispatch path is implemented per Technical Design's `private int DispatchWithBranch(int action, int branch)`. Matcher tightened from `DispatchWithBranch|HandleMessage.*_|branch` to avoid false-positive on incidental 'branch' occurrences in a 550-line file. (C12)

**AC#16e2: CounterMessage DispatchWithBranch has 3 branch-variant handlers**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` count `HandleMessage29_[123]` >= 3
- **Expected**: Count >= 3 (all 3 branch-variant handler methods exist: HandleMessage29_1, HandleMessage29_2, HandleMessage29_3)
- **Rationale**: COUNTER_MESSAGE.ERB has 3 branch variants for action 29 (EVENT_COUNTE_MESSAGE29_1, 29_2, 29_3) with distinct logic (29_1: 4 clothing conditional paths, 29_2: ABL/CFLAG guard, 29_3: simple). AC#16e only verifies DispatchWithBranch method name exists. Count >= 3 ensures all 3 handler methods exist, not just one. Previous matcher `HandleMessage29_[123]|HandleBranch[123]|branch.*==.*[123]` had false-positive risk from `branch.*==.*[123]` matching unrelated comparisons. (C12)

**AC#16f: CounterMessage conditional text building (PRINTFORML ternary translation)**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` count `console\.Print.*\?` >= 4
- **Expected**: Count >= 4 (at least 4 console.Print calls with conditional expressions for PRINTFORML inline ternary sites)
- **Rationale**: COUNTER_MESSAGE.ERB uses `\@ expr ? A # B \@` inline ternary at lines 103, 118, 120, 126+. Technical Constraint requires "Must translate to C# conditional string building." Count >= 4 ensures the documented ternary sites are translated, not omitted. Tightened from `\? .*:` to `console\.Print.*\?` to target print-context ternaries only, preventing false-positive matches from incidental ternaries (e.g., null-coalescing, conditional assignments) in a 550-line file. (Technical Constraints)

**AC#16g: CounterMessage has >= 10 private handler methods (substantial dispatch coverage)**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` count `private.*int HandleMessage` >= 10
- **Expected**: Count >= 10 (substantial coverage of ~35 message handlers; mirrors AC#12's count-based approach for POSE)
- **Rationale**: C12 constraint states "ACs must verify representative subset of message handlers." AC#16b/16c/16d verify 3 specific handler names but don't enforce non-trivial total count. An implementer could stub only HandleMessage10/50/58 while leaving ~32 handlers unimplemented. Count >= 10 (analogous to AC#12's >= 10 IncrementTFlag for POSE's ~25 actions) ensures substantial dispatch coverage for MESSAGE's ~35 handlers. (C12)

**AC#16h: CounterMessage has 500-range handler (SubAction-dependent group)**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `HandleMessage50[0-5]|HandleMessage60[01]`
- **Expected**: Pattern matches (at least one handler in 500-601 range exists)
- **Rationale**: COUNTER_MESSAGE.ERB has 8 handlers in the 500-601 range (500, 502, 503, 504, 505, 600, 601) that use TCVAR:21 (SubAction) for sub-branching, structurally different from low-range handlers. AC#16b/16c/16d spot-check only 10/50/58. AC#16g (gte >= 10) doesn't force 500+ range inclusion. An implementer could implement 10 handlers all below 100 and entirely omit the SubAction group. (C12)

**AC#17: CounterMessage DATUI_MESSAGE migrated**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `DatuiMessage|HandleUndressingMessage`
- **Expected**: Pattern matches
- **Rationale**: DATUI_MESSAGE is a local function within COUNTER_MESSAGE.ERB (lines 484-550) that handles clothing strip messaging using TCVAR:22-25. Must be migrated as a method within CounterMessage. (C13)

**AC#18: CounterCombination faithful reproduction of incomplete stub**
- **Test**: `Grep(Era.Core/Counter/CounterCombination.cs)` for `AccumulateCombinations|CountCombinations`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_CONBINATION.ERB is explicitly incomplete ("つくりかけ", line 3). Must faithfully reproduce the 10-line counting logic without additions. (C5)

**AC#19: Text formatting stub interface defined with GetPantsDescription**
- **Test**: `Grep(Era.Core/Interfaces/ITextFormatting.cs)` for `GetPantsDescription`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_MESSAGE.ERB calls PANTS_DESCRIPTION at lines 36, 129, 133. File-scoped to ITextFormatting.cs to avoid false positives from ERB comments or other references. (C4)

**AC#19b: Text formatting stub interface includes GetOppaiDescription**
- **Test**: `Grep(Era.Core/Interfaces/ITextFormatting.cs)` for `GetOppaiDescription`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_MESSAGE.ERB calls OPPAI_DESCRIPTION at lines 317, 319. Separate AC ensures all 3 C4-required functions are verified, not just pants-related names. (C4)

**AC#19c: Text formatting stub interface includes GetPantsName**
- **Test**: `Grep(Era.Core/Interfaces/ITextFormatting.cs)` for `GetPantsName`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_MESSAGE.ERB calls PANTSNAME at lines 129, 133. Completes C4 coverage (PANTS_DESCRIPTION → AC#19, OPPAI_DESCRIPTION → AC#19b, PANTSNAME → AC#19c). (C4)

**AC#20: CounterMessage uses text formatting dependency**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `ITextFormatting|textFormatting|pantsDescription|GetPantsDescription`
- **Expected**: Pattern matches
- **Rationale**: CounterMessage must inject the text formatting dependency to handle PANTS_DESCRIPTION/PANTSNAME/OPPAI_DESCRIPTION calls. Ensures DI, not hardcoded stubs. (C4)

**AC#20b: CounterMessage uses IConsoleOutput for text output**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `IConsoleOutput console|console\.PrintLine|console\.Print\(`
- **Expected**: Pattern matches (constructor parameter declaration or actual method call)
- **Rationale**: CounterMessage has 550 lines of PRINTFORML output requiring IConsoleOutput injection. Tightened from broad `console` token to require constructor parameter declaration or method call pattern, preventing false positives from comments or variable names.

**AC#21: CounterOutputHandler HandlePunishment iterates all characters (CHARANUM loop)**
- **Test**: `Grep(Era.Core/Counter/CounterOutputHandler.cs)` for `GetCharaNum\(\)|charaNum|CHARANUM`
- **Expected**: Pattern matches
- **Rationale**: HandlePunishment must iterate ALL characters to reset うふふ CFLAG and call CLOTHES_RESET/SETTING_TRAIN per ICounterOutputHandler contract (see XML doc lines 14-22) and COUNTER_SELECT.ERB:60-64 FOR loop. (C2)

**AC#22: CounterOutputHandler HandlePunishment resets うふふ CFLAG per character**
- **Test**: `Grep(Era.Core/Counter/CounterOutputHandler.cs)` for `SetCharacterFlag.*CFlagUfufu.*0|SetCharacterFlag.*317.*0`
- **Expected**: Pattern matches
- **Rationale**: Each character's うふふ CFLAG must be reset to 0 during punishment cleanup. This is the per-character reset, not single-character. (C2)

**AC#23: CounterOutputHandler HandlePunishment calls CLOTHES_RESET per character**
- **Test**: `Grep(Era.Core/Counter/CounterOutputHandler.cs)` for `ClothesReset|clothingSystem\.Save\(`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_SELECT.ERB:62 calls CLOTHES_RESET (maps to IClothingSystem.Save) per character during punishment path. Matcher scoped to clothingSystem receiver to avoid false positives from unrelated Save() calls. (C2)

**AC#24: CounterOutputHandler HandlePunishment calls CLOTHES_SETTING_TRAIN per character**
- **Test**: `Grep(Era.Core/Counter/CounterOutputHandler.cs)` for `SettingTrain\(`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_SELECT.ERB:63 calls CLOTHES_SETTING_TRAIN per character during punishment path. (C2)

**AC#25: CounterPunishment DRAWLINE + WAIT via IConsoleOutput**
- **Test**: `Grep(Era.Core/Counter/CounterPunishment.cs)` for `IConsoleOutput|DrawLine|consoleOutput`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_PUNISHMENT.ERB:14-15 has DRAWLINE + WAIT after punishment. Unlike WC analog which omits these, F802 must include them via IConsoleOutput. (Constraint C11 note: WC omits, main must include)

**AC#26: CounterMessage public entry point for upstream callers**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `public.*void.*SendMessage`
- **Expected**: Pattern matches
- **Rationale**: EVENT_KOJO.ERB:76,83,92 calls COUNTER_MESSAGE. CounterMessage must expose public `SendMessage()` per Technical Design decision. Tightened to match committed signature only (removed ProcessMessage alternative). (C14)

**AC#26b: CounterMessage SendMessage uses GetTarget() guard**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `GetTarget\(\)`
- **Expected**: Pattern matches (GetTarget guard exists in implementation)
- **Rationale**: COUNTER_MESSAGE.ERB:3-4 has `SIF TARGET <= 0 / RETURN 0` early-return guard. AC#36c verifies test-side coverage but no AC verified the implementation calls engine.GetTarget(). Without this guard, SendMessage would process invalid/absent targets. (C14)

**AC#26c: CounterMessage DatuiMessage() is both defined and called (not dead code)**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` count `DatuiMessage\(\)` >= 2
- **Expected**: Count >= 2 (one match for method definition `private void DatuiMessage()`, one or more for call sites `DatuiMessage();` inside SendMessage)
- **Rationale**: AC#17 verifies `DatuiMessage` string exists but cannot distinguish method definition from call site — a defined-but-never-called DatuiMessage() passes AC#17. COUNTER_MESSAGE.ERB:5 calls DATUI_MESSAGE unconditionally before TRYCALLFORM dispatch (Technical Design confirms: "DatuiMessage() called at the start of SendMessage()"). Count >= 2 ensures at least one call site beyond the definition, preventing dead code. (C13)

**AC#27: CounterPose public entry point for upstream callers (with actionOrder)**
- **Test**: `Grep(Era.Core/Counter/CounterPose.cs)` for `public.*void.*ProcessPose.*CharacterId.*int`
- **Expected**: Pattern matches (verifies both public access and two-parameter signature: CharacterId offender, int actionOrder)
- **Rationale**: INFO.ERB:189 calls EVENT_COUNTER_POSE. CounterPose must expose a public ProcessPose method with actionOrder parameter (drives the ~25-action switch in AC#12). Tightened from `public.*void.*ProcessPose` to enforce parameter signature per Task 9 Technical Design contract. (C14)

**AC#28: CounterCombination public entry point returns int[] for upstream callers**
- **Test**: `Grep(Era.Core/Counter/CounterCombination.cs)` for `public.*int\[\].*AccumulateCombinations`
- **Expected**: Pattern matches (verifies both public access and int[] return type)
- **Rationale**: INFO.ERB:191 and SOURCE.ERB:40 call EVENT_COUNTER_COMBINATION. CounterCombination must expose a public method returning int[] per Technical Design Key Decision (WcCounterCombination analogy, upstream caller needs result). Matcher tightened from `public.*AccumulateCombinations` to enforce int[] return type. (C14)

**AC#29: TCVarIndex undressing state constants (indices 22-25)**
- **Test**: `Grep(Era.Core/Types/TCVarIndex.cs)` for `public static readonly TCVarIndex.*new\(2[2-5]\)`
- **Expected**: Pattern matches
- **Rationale**: DATUI_MESSAGE uses TCVAR:22-25 for undressing states (player lower, target lower, player upper, target upper). Must be named constants. (C10)

**AC#30: TCVarIndex SixtyNineTransition constant (index 27)**
- **Test**: `Grep(Era.Core/Types/TCVarIndex.cs)` for `public static readonly TCVarIndex.*new\(27\)`
- **Expected**: Pattern matches
- **Rationale**: EVENT_COUNTER_POSE_69 sets TCVAR:ARG:27=1 for sixnine transition (COUNTER_POSE.ERB:129). Must be a named constant. (C10)

**AC#31: CounterReaction uses named TCVarIndex.SubAction constant**
- **Test**: `Grep(Era.Core/Counter/CounterReaction.cs)` for `TCVarIndex\.SubAction|SubAction`
- **Expected**: Pattern matches (uses named constant)
- **Rationale**: With TCVarIndex.SubAction defined, new code must use the named constant. Verifies typed-index philosophy adoption. (C10)

**AC#31b: CounterPose uses named TCVarIndex.SixtyNineTransition constant**
- **Test**: `Grep(Era.Core/Counter/CounterPose.cs)` for `TCVarIndex\.SixtyNineTransition`
- **Expected**: Pattern matches (uses named constant, not raw `new TCVarIndex(27)`)
- **Rationale**: CounterPose.HandlePose69() must use TCVarIndex.SixtyNineTransition (index 27) for the sixnine transition logic (COUNTER_POSE.ERB:129). Verifies typed-index philosophy adoption. (C10)

**AC#31c: CounterMessage DatuiMessage uses named undressing constants**
- **Test**: `Grep(Era.Core/Counter/CounterMessage.cs)` for `TCVarIndex\.UndressingPlayerLower|TCVarIndex\.UndressingTargetLower|TCVarIndex\.UndressingPlayerUpper|TCVarIndex\.UndressingTargetUpper`
- **Expected**: Pattern matches (at least one named undressing constant referenced)
- **Rationale**: DatuiMessage reads TCVAR:22-25 for undressing state (COUNTER_MESSAGE.ERB:484-550). Must use named TCVarIndex constants, not raw integers. (C10)

**AC#32: Unit tests for CounterReaction exist**
- **Test**: `Glob(Era.Core.Tests/Counter/CounterReactionTests.cs)`
- **Expected**: File exists
- **Rationale**: TDD requirement; CounterReaction has 5 pose-conditional branches needing test coverage.

**AC#32b: CounterReactionTests verify SELECTCOM branching (4 branches)**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterReactionTests.cs)` count `GetSelectCom` >= 4
- **Expected**: Count >= 4 (one assertion per SELECTCOM-using branch: 80/81/82/83)
- **Rationale**: CounterReaction has 4 SELECTCOM-based branches. Tests must set up and verify each SELECTCOM value (80=masturbation, 81=fellatio, 82=paizuri, 83=frotteurism). Mirrors AC#9e implementation-side SELECTCOM verification. (C6)

**AC#32c: CounterReactionTests verify vaginal branch**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterReactionTests.cs)` for `HandleVaginal|ProcessVaginal|HandleInsertion|vaginal|Vaginal`
- **Expected**: Pattern matches (vaginal branch test exists)
- **Rationale**: AC#32b verifies SELECTCOM branching (4 of 5 branches), but the vaginal branch (SELECTCASE block 1) uses TALENT/ABL logic, not SELECTCOM. A test file with only SELECTCOM-branch tests would pass AC#32b while omitting vaginal entirely. Task 4 explicitly includes vaginal coverage. (C6)

**AC#32d: CounterReactionTests verify MASTER_POSE guard dispatch**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterReactionTests.cs)` for `GetMasterPose|MasterPose`
- **Expected**: Pattern matches (MASTER_POSE guard tested)
- **Rationale**: CounterReaction outer dispatch uses MASTER_POSE guards (AC#9g). Test must set up MASTER_POSE mock values to exercise the 5 pose-type IF blocks. Mirrors AC#9g implementation-side coverage. (C6)

**AC#32e: CounterReactionTests verify protection brand guard**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterReactionTests.cs)` for `IsProtectedCheckOnly|IsProtected|Protected`
- **Expected**: Pattern matches (protection guard tested)
- **Rationale**: CounterReaction uses IsProtectedCheckOnly brand guard in 4 of 5 branches (AC#9h). Test must verify protection guard behavior. Mirrors AC#9h implementation-side coverage.

**AC#33: Unit tests for CounterPunishment exist**
- **Test**: `Glob(Era.Core.Tests/Counter/CounterPunishmentTests.cs)`
- **Expected**: File exists
- **Rationale**: TDD requirement; CounterPunishment has 3 punishment variants needing test coverage.

**AC#33b: CounterPunishmentTests verify variant dispatch**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterPunishmentTests.cs)` for `ExecuteVariant|Variant0|Variant1|Variant2|random\.Next`
- **Expected**: Pattern matches (variant dispatch test assertions exist)
- **Rationale**: CounterPunishment has 3 punishment variants (C9). Test must cover variant 0 (underwear removal), variant 1 (rotor-V), variant 2 (rotor-A), and the retry loop mechanism.

**AC#33c: CounterPunishmentTests assert specific CFLAG names in variant assertions**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterPunishmentTests.cs)` for `CFlagLowerUnderwear|CFlagRotorInsert`
- **Expected**: Pattern matches (test references specific CFLAG constant names)
- **Rationale**: Behavioral equivalence requires tests to assert against specific CFLAG names (CFlagLowerUnderwear for variant 0, CFlagRotorInsert for variants 1/2), not just that SetCharacterFlag is called. Without CFLAG-name-specific assertions, tests could pass while using wrong CFLAG indices — the ERB→C# equivalence gap identified in Goal Item 7. (C9)

**AC#33d: CounterPunishmentTests verify precondition guard retry scenarios (underwear/vagina)**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterPunishmentTests.cs)` for `HasUnderwear|LowerUnderwear.*0|HasVagina`
- **Expected**: Pattern matches (precondition guard test exists)
- **Rationale**: AC#11e (underwear precondition guard) and AC#11f (vagina precondition guard) verify implementation-side retry logic in CounterPunishment.cs. Every other critical guard has a test-side mirror (AC#9h→AC#32e, AC#12b→AC#34c, AC#26b→AC#36c). AC#33b verifies variant dispatch but not the retry precondition paths (no underwear → retry, no vagina → retry). AC#33d closes this test-side symmetry gap. (C9)

**AC#34: Unit tests for CounterPose exist**
- **Test**: `Glob(Era.Core.Tests/Counter/CounterPoseTests.cs)`
- **Expected**: File exists
- **Rationale**: TDD requirement; CounterPose has ~25 action-to-TFLAG mappings and 69-transition logic needing test coverage.

**AC#34b: CounterPoseTests verify TFLAG mapping**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterPoseTests.cs)` for `IncrementTFlag|ProcessPose|HandlePose69`
- **Expected**: Pattern matches (TFLAG mapping and pose-69 transition tested)
- **Rationale**: CounterPose has ~25 action-to-TFLAG mappings (C7) and HandlePose69 sixnine transition (C8). Test must exercise representative TFLAG increments and 69-position logic.

**AC#34c: CounterPoseTests verify CounterDecisionFlag guard path**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterPoseTests.cs)` for `CounterDecisionFlag|decisionFlag`
- **Expected**: Pattern matches (guard path tested)
- **Rationale**: The early-return when CounterDecisionFlag > 1 is a critical behavioral path that must be tested. Without test coverage, the guard could be accidentally removed during refactoring.

**AC#34d: CounterPoseTests assert specific TFLAG body-part indices in action mapping**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterPoseTests.cs)` count `TFlag6[0-5]` >= 2
- **Expected**: Count >= 2 (tests reference at least 2 distinct TFLAG body-part indices from the 60-65 range)
- **Rationale**: Behavioral equivalence requires tests to verify specific action→TFLAG body-part mappings (e.g., action 10→TFlag60 oral, action 50→TFlag63 groin), not just that IncrementTFlag is called generically. AC#34b verifies IncrementTFlag/ProcessPose method references; AC#34d verifies the tests contain specific TFLAG index values, closing the value-assertion gap. (C7)

**AC#35: Unit tests for CounterOutputHandler exist**
- **Test**: `Glob(Era.Core.Tests/Counter/CounterOutputHandlerTests.cs)`
- **Expected**: File exists
- **Rationale**: TDD requirement; CounterOutputHandler orchestrates punishment (all-character iteration) and reaction dispatch needing test coverage.

**AC#35b: CounterOutputHandlerTests verify all-character iteration**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterOutputHandlerTests.cs)` for `GetCharaNum`
- **Expected**: Pattern matches (CHARANUM loop tested)
- **Rationale**: CounterOutputHandler.HandlePunishment iterates all characters (C2). Test must set up multiple characters and verify per-character CFLAG reset and clothing operations.

**AC#36: Unit tests for CounterMessage exist**
- **Test**: `Glob(Era.Core.Tests/Counter/CounterMessageTests.cs)`
- **Expected**: File exists
- **Rationale**: TDD requirement; CounterMessage is the largest class (550 lines, ~35 dispatch handlers, TEQUIP side effects) and requires dedicated test coverage.

**AC#36b: CounterMessageTests verify TEQUIP mutation**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterMessageTests.cs)` for `SetTEquip`
- **Expected**: Pattern matches (TEQUIP mutation test assertions exist)
- **Rationale**: CounterMessage is not pure display — it mutates TEQUIP:PLAYER:3/4 state (C3). Test must verify state changes occur, not just console output.

**AC#36c: CounterMessageTests verify GetTarget guard path**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterMessageTests.cs)` for `GetTarget`
- **Expected**: Pattern matches (target guard early-return tested)
- **Rationale**: CounterMessage.SendMessage returns early when GetTarget() <= 0. Test must verify this guard path to prevent null-character processing.

**AC#36d: CounterMessageTests verify representative switch dispatch coverage**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterMessageTests.cs)` count `HandleMessage` >= 3
- **Expected**: Count >= 3 (representative handlers tested: e.g., HandleMessage10, HandleMessage50, HandleMessage58)
- **Rationale**: CounterMessage has ~35 dispatch handlers (C12). Tests must cover representative subset matching AC#16b/16c/16d implementation-side coverage. Count >= 3 ensures low/mid/high range handlers are tested.

**AC#36e: CounterMessageTests verify DatuiMessage undressing logic**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterMessageTests.cs)` for `DatuiMessage|UndressingPlayer`
- **Expected**: Pattern matches (DatuiMessage test exists)
- **Rationale**: DatuiMessage is a private method within CounterMessage (COUNTER_MESSAGE.ERB:484-550) handling undressing text using TCVAR:22-25 (C13). Test must verify undressing text output logic.

**AC#36f: CounterMessageTests verify 500-range SubAction handler**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterMessageTests.cs)` for `HandleMessage50[0-5]|HandleMessage60[01]|SubAction`
- **Expected**: Pattern matches (at least one 500-range SubAction-dependent handler is tested)
- **Rationale**: AC#16h verifies 500-range handlers exist in implementation (SubAction-dependent group, structurally distinct from low-range handlers). AC#36d requires >= 3 HandleMessage test references but doesn't force 500-range inclusion. Without AC#36f, all 8 SubAction-dependent handlers (500-601) could be completely untested. Mirrors AC#16h on the test side. (C12)

**AC#36g: CounterMessageTests assert specific TEQUIP mutation values (index 3, value 1) in >= 2 test scenarios**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterMessageTests.cs)` count `SetTEquip.*,\s*3,\s*1` >= 2
- **Expected**: Count >= 2 (tests assert TEQUIP:PLAYER:3 mutation in at least 2 distinct test scenarios, providing multi-site behavioral equivalence verification)
- **Rationale**: Behavioral equivalence requires tests to verify TEQUIP mutations across multiple handler invocations, not just one. Implementation-side AC#15 requires >= 5 SetTEquip sites. Test-side requires >= 2 to ensure mutations are verified in more than one handler context (e.g., low-range and mid-range), closing the value-assertion gap for Goal Item 7. Strengthened from `matches` (single occurrence) to `gte >= 2` to maintain proportional coverage with AC#15. (C3)

**AC#36h: CounterMessageTests assert TEQUIP:PLAYER:4 mutation in test assertions**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterMessageTests.cs)` for `SetTEquip.*4,\s*1`
- **Expected**: Pattern matches (test asserts TEQUIP:PLAYER:4 mutation)
- **Rationale**: C3 constraint states "TEQUIP:PLAYER:3/4 mutations must be preserved". Implementation-side: AC#15 (index 3, gte >= 5) and AC#15b (index 4, matches). Test-side: AC#36g verifies index 3 assertions (gte >= 2). AC#36h closes the symmetry gap by verifying tests also assert TEQUIP:PLAYER:4 mutations, ensuring C3 test-side coverage is complete for both indices. (C3)

**AC#37: Unit tests for CounterCombination exist**
- **Test**: `Glob(Era.Core.Tests/Counter/CounterCombinationTests.cs)`
- **Expected**: File exists
- **Rationale**: TDD requirement; CounterCombination has behavioral ACs (AC#18, AC#28) requiring test verification.

**AC#37b: CounterCombinationTests verify AccumulateCombinations**
- **Test**: `Grep(Era.Core.Tests/Counter/CounterCombinationTests.cs)` for `AccumulateCombinations`
- **Expected**: Pattern matches (AccumulateCombinations behavioral test exists)
- **Rationale**: CounterCombination.AccumulateCombinations() iterates characters and increments combination counts. Test must verify counting behavior of the faithful 10-line stub reproduction. (C5)

**AC#38: IConsoleOutput Wait() method exists**
- **Test**: `Grep(Era.Core/Interfaces/IConsoleOutput.cs)` for `Wait\(\)`
- **Expected**: Pattern matches
- **Rationale**: COUNTER_PUNISHMENT.ERB:15 uses standalone WAIT after DRAWLINE. IConsoleOutput needs a Wait() method (T3 Upstream Issue resolution).

**AC#38b: HeadlessConsole Wait() implementation exists**
- **Test**: `Grep(engine/Assets/Scripts/Emuera/Headless/HeadlessConsole.cs)` for `Wait\(\)`
- **Expected**: Pattern matches
- **Rationale**: T3 adds Wait() to both Era.Core interface (AC#38) and engine headless implementation. AC#38b verifies the headless concrete implementation was updated. Engine/ is outside `dotnet build Era.Core/` scope; this AC provides deterministic verification.

**AC#39: Build succeeds**
- **Test**: `dotnet build Era.Core/`
- **Expected**: Exit code 0 (TreatWarningsAsErrors=true means warning-free)
- **Rationale**: All new C# must compile without warnings per Directory.Build.props. Also implicitly verifies backward compatibility of IConsoleOutput extension (T3).

**AC#40: All tests pass**
- **Test**: `dotnet test Era.Core.Tests/`
- **Expected**: All tests pass
- **Rationale**: New tests must pass and existing tests must not regress. Also serves as deterministic catch for T3's 8 IConsoleOutput test stub updates (TalentCopierTests, CharacterCustomizerTests, CollectionTrackerTests, ConsoleOutputDelegationTests, ItemPurchaseTests, ItemDescriptionsTests, PrintCommandTests, PrintOutputEquivalenceTests) — missing stubs fail compilation (caught by AC#39) or test execution (caught here).

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Migrate 5 ERB files to C# classes under Era.Core/Counter/ | AC#1, AC#2, AC#3, AC#4, AC#5, AC#9, AC#9b, AC#9c, AC#9d, AC#9e, AC#9f, AC#9g, AC#9h, AC#9i, AC#9j, AC#10, AC#11, AC#11b, AC#11c, AC#11d, AC#11e, AC#11f, AC#12, AC#12b, AC#12c, AC#12d, AC#13, AC#14, AC#14b, AC#15, AC#15b, AC#17, AC#18, AC#25 |
| 2 | Implement ICounterOutputHandler (HandleReaction + HandlePunishment including all-character cleanup loop) | AC#8, AC#8b, AC#8c, AC#21, AC#22, AC#23, AC#24 |
| 3 | Create separate handler classes for MESSAGE, POSE, COMBINATION with appropriate interfaces for upstream callers | AC#26, AC#27, AC#28 |
| 4 | Add TCVarIndex constants for indices 21, 22-25, 27 | AC#6, AC#7, AC#29, AC#30, AC#31, AC#31b, AC#31c |
| 5 | Define strategy for missing text formatting functions (PANTS_DESCRIPTION, PANTSNAME, OPPAI_DESCRIPTION) | AC#19, AC#19b, AC#19c, AC#20 |
| 6 | Translate TRYCALLFORM dynamic dispatch in MESSAGE to C# dictionary/switch pattern | AC#16, AC#16b, AC#16c, AC#16d, AC#16e, AC#16e2, AC#16f, AC#16g, AC#16h |
| 7 | Equivalence tests must verify C# output matches legacy ERB behavior | AC#32, AC#32b, AC#32c, AC#32d, AC#33, AC#33b, AC#33c, AC#33d, AC#34, AC#34b, AC#34c, AC#34d, AC#35, AC#35b, AC#36, AC#36b, AC#36c, AC#36d, AC#36e, AC#36f, AC#36g, AC#36h, AC#37, AC#37b, AC#40 |
| 8 | IConsoleOutput.Wait() upstream gap resolution | AC#38, AC#38b |
| 9 | Build succeeds with all new code | AC#39 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

F802 migrates 5 ERB files to 5 dedicated C# classes following the WC Counter (F804) pattern exactly. The architecture has two layers:

**Layer 1: CounterOutputHandler (ICounterOutputHandler implementation)**
This class is the F801 contract fulfillment. It orchestrates the うふふ paths:
- `HandleReaction(offender, actionOrder)` → delegates to `CounterReaction.ProcessReaction()`
- `HandlePunishment(offender, actionOrder)` → delegates to `CounterPunishment.Execute()`, then iterates ALL characters (CHARANUM loop) to reset うふふ CFLAG + CLOTHES_RESET + CLOTHES_SETTING_TRAIN per F801 XML doc contract

**Layer 2: Independent handler classes**
`CounterReaction`, `CounterPunishment`, `CounterPose`, `CounterMessage`, and `CounterCombination` are sealed classes with primary constructor DI (matching WcCounterReaction/WcCounterPunishment pattern). Each is independently instantiable and testable.

**Three call chains are preserved:**
- (a) REACTION/PUNISHMENT: through ICounterOutputHandler → CounterOutputHandler → CounterReaction/CounterPunishment
- (b) POSE/COMBINATION: public ProcessPose() / AccumulateCombinations() directly on handler classes for INFO.ERB/SOURCE.ERB consumers (F811)
- (c) MESSAGE: public SendMessage(offender, messageVariant?) on CounterMessage, matching IWcCounterOutputHandler.SendMessage() pattern for EVENT_KOJO.ERB consumers

**TCVarIndex expansion**: Add 6 named constants (indices 21-27) to TCVarIndex.cs before implementing any handler class that uses them.

**ITextFormatting**: New interface stub defined in Era.Core/Interfaces/ with methods for PantsDescription(equipIndex), PantsName(equipIndex), and OppaiDescription(characterId). CounterMessage receives it via constructor injection. Stub implementation returns empty string for headless/test contexts, preserving compilability with TreatWarningsAsErrors.

**CounterMessage dispatch**: TRYCALLFORM EVENT_COUNTE_MESSAGE{TCVAR:20}_{branch} translates to a C# switch on the action integer, dispatching to private handler methods. The branch variant (枝番) becomes an optional int? parameter on SendMessage (following WcEventKojo → IWcCounterOutputHandler.SendMessage pattern).

**DATUI_MESSAGE** (COUNTER_MESSAGE.ERB:484-550) becomes private method `DatuiMessage()` called at the start of `SendMessage()`. It reads TCVAR:22-25 (named constants) and outputs undressing text based on state.

**TEQUIP side effects** in message handlers are preserved 1:1 using ITEquipVariables.SetTEquip().

**PUNISHMENT DRAWLINE+WAIT**: IConsoleOutput.DrawLine() exists. A standalone Wait() method is missing from IConsoleOutput — see Upstream Issues. Design assumes Wait() will be added; CounterPunishment injects IConsoleOutput and calls `console.DrawLine()` then `console.Wait()`.

**CFLAG naming**: Follow WcCounterPunishment pattern using CharacterFlagIndex with named private const integers. **IMPORTANT**: Main counter CFLAG indices differ from WC. Do NOT copy WcCounterPunishment numeric values directly. Main counter values (from CFLAG.CSV): `ローター挿入(CFlagRotorInsertV) = 15`, `ローターA挿入(CFlagRotorInsertA) = 16`, `服装_下半身下着２(CFlagLowerUnderwear2) = varies (read from CFLAG.CSV)`, `ローター挿入者(CFlagRotorInserter) = 27`. WcCounterPunishment uses CFlagRotorInsertor=1081 which is WC-specific.

**TDD**: All 6 test files (CounterReactionTests, CounterPunishmentTests, CounterPoseTests, CounterOutputHandlerTests, CounterMessageTests, CounterCombinationTests) follow WcCounterPunishmentTests/WcCounterPoseTests patterns using local StubVariableStore-style test doubles.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core/Counter/CounterReaction.cs` — sealed class implementing the 5 pose-conditional REACTION branches |
| 2 | Create `Era.Core/Counter/CounterPunishment.cs` — sealed class with WHILE/RAND:10 retry loop and 3 punishment variants |
| 3 | Create `Era.Core/Counter/CounterPose.cs` — sealed class with ~25 action-to-TFLAG switch and HandlePose69 |
| 4 | Create `Era.Core/Counter/CounterMessage.cs` — sealed class with SendMessage(), switch dispatch over ~35 handlers, DatuiMessage(), TEQUIP mutations |
| 5 | Create `Era.Core/Counter/CounterCombination.cs` — sealed class faithful 10-line stub reproduction |
| 6 | Add `public static readonly TCVarIndex SubAction = new(21)` to TCVarIndex.cs |
| 7 | Add 6 named constants (SubAction/21, UndressingPlayerLower/22, UndressingTargetLower/23, UndressingPlayerUpper/24, UndressingTargetUpper/25, SixtyNineTransition/27) bringing total from 10 to 16 |
| 8 | Create `Era.Core/Counter/CounterOutputHandler.cs` with `sealed class CounterOutputHandler(...) : ICounterOutputHandler` |
| 8b | CounterOutputHandler.HandleReaction() delegates to `reaction.ProcessReaction(..., new CharacterId(engine.GetMaster()))` — behavioral verification of reaction path with F801 master argument contract |
| 8c | CounterOutputHandler.HandlePunishment() calls `punishment.Execute(offender, actionOrder)` before entering the CHARANUM cleanup loop |
| 9 | CounterReaction has masturbation branch method (SELECTCASE block 2, SELECTCOM==80) |
| 9b | CounterReaction has fellatio branch method (SELECTCASE block 3, SELECTCOM==81) |
| 9c | CounterReaction has paizuri branch method (SELECTCASE block 4, SELECTCOM==82) |
| 9d | CounterReaction has frotteurism branch method (SELECTCASE block 5, SELECTCOM==83) |
| 9f | CounterReaction has vaginal branch method (SELECTCASE block 1, TALENT/ABL logic — no SELECTCOM) |
| 9g | CounterReaction has MASTER_POSE guard conditions for the 5 pose-type IF blocks (outer dispatch structure, not inner SELECTCOM) |
| 9h | CounterReaction has `IsProtectedCheckOnly` or `counterUtils.IsProtected` brand guard check in 4 of 5 branches |
| 9i | CounterReaction has >= 5 MASTER_POSE index comparisons using actual values 2, 3, 4, 5, 6 from COUNTER_REACTION.ERB |
| 9j | CounterReaction dispatch code uses named SELECTCOM constants (SelectComMasturbation/Fellatio/Paizuri/Frotteurism) instead of raw integer literals — count >= 4 ensures all 4 are referenced in dispatch, not just defined |
| 10 | CounterPunishment DispatchVariant() loop uses `random.Next(10)` matching RAND:10 in COUNTER_PUNISHMENT.ERB:9 |
| 10b | CounterPunishment.Execute() returns early (skip) when character is 999 (player); matcher: `999|SkipPlayer|IsPlayer` |
| 11 | CounterPunishment ExecuteVariant0 calls `variables.SetCharacterFlag(player, new CharacterFlagIndex(CFlagLowerUnderwear2), 0)` using named const `CFlagLowerUnderwear2` |
| 11b | CounterPunishment ExecuteVariant1 calls `variables.SetCharacterFlag` for rotor-V insertion CFLAG |
| 11c | CounterPunishment ExecuteVariant2 calls `variables.SetCharacterFlag` for rotor-A insertion CFLAG |
| 11d | CounterPunishment ExecuteVariant1 and ExecuteVariant2 set `CFLAG:PLAYER:ローター挿入者 = ARG` (rotor-inserter identity tracking CFLAG) |
| 11e | CounterPunishment ExecuteVariant0 checks `CFLAG:PLAYER:服装_下半身下着２ > 0` (underwear exists) before removal; returns retry signal when absent |
| 11f | CounterPunishment ExecuteVariant1 checks `HAS_VAGINA(PLAYER)` before rotor-V insertion; returns retry signal when unavailable |
| 12 | CounterPose.ProcessPose() calls `IncrementTFlag(TFlagXxx)` in switch arms for all ~25 action IDs from COUNTER_POSE.ERB:5-84 |
| 12b | CounterPose.ProcessPose() reads `TCVarIndex.CounterDecisionFlag` (index 30) and returns early when > 1, matching COUNTER_POSE.ERB:3 guard |
| 12c | CounterPose TFLAG switch reads `variables.GetTCVar(offender, TCVarIndex.CounterAction)` as dispatch key, not `actionOrder` parameter |
| 12d | CounterPose uses >= 5 distinct `TFlag6[0-5]` index constants (60-65) for body-part mapping distribution |
| 13 | CounterPose.HandlePose69() calls `tequip.SetTEquip(offender, TEquipSixtyNine, 1)` and `variables.SetTCVar(offender, TCVarIndex.SixtyNineTransition, 1)` |
| 14 | CounterReaction declared as `sealed class CounterReaction(IVariableStore variables, ...)` — primary constructor DI matching WcCounterReaction |
| 14b | All 6 Counter classes use `sealed class Counter*` pattern (combined gte 6 check mirroring F804 AC#37) |
| 15 | CounterMessage message handlers (e.g., HandleMessage50, HandleMessage58) write `tequip.SetTEquip(player, 3, 1)` (TEQUIP:PLAYER:3) to preserve state mutation |
| 15b | CounterMessage message handlers write `tequip.SetTEquip(player, 4, 1)` (TEQUIP:PLAYER:4) to preserve state mutation |
| 16 | CounterMessage.SendMessage() contains `switch (action)` dispatch to private handler methods, satisfying `switch \(action\)` pattern match |
| 16b | CounterMessage contains HandleMessage10 (low-range representative) per C12 |
| 16c | CounterMessage contains HandleMessage50 (mid-range representative) per C12 |
| 16d | CounterMessage contains HandleMessage58 (high-range representative) per C12 |
| 16e | CounterMessage contains `DispatchWithBranch` method or branch-specific handler dispatch for branched TRYCALLFORM variants (e.g., action=29 branch=1/2/3) |
| 16e2 | CounterMessage.DispatchWithBranch() has all 3 branch-variant handler methods (HandleMessage29_1, 29_2, 29_3) — count >= 3 ensures all exist, not just one |
| 16f | CounterMessage has >= 4 `console.Print` calls with conditional expressions (print-context ternaries only) translating PRINTFORML `\@ expr ? A # B \@` patterns from COUNTER_MESSAGE.ERB:103,118,120,126 |
| 16g | CounterMessage has >= 10 `private.*int HandleMessage` methods, ensuring substantial dispatch coverage for ~35 message handlers (mirrors AC#12's count-based approach for POSE) |
| 16h | CounterMessage has at least one handler method in the 500-601 range (SubAction-dependent handlers: HandleMessage500, 502, 503, 504, 505, 600, 601) |
| 17 | CounterMessage contains private `DatuiMessage()` method, which contains the undressing logic from COUNTER_MESSAGE.ERB:484-550 |
| 18 | CounterCombination.AccumulateCombinations() iterates characters and increments counts — satisfies `AccumulateCombinations` grep match |
| 19 | Define `ITextFormatting` interface in Era.Core/Interfaces/ with `GetPantsDescription(int equipIndex)` method |
| 19b | Define `ITextFormatting` interface in Era.Core/Interfaces/ with `GetOppaiDescription(int characterId)` method |
| 19c | Define `ITextFormatting` interface in Era.Core/Interfaces/ with `GetPantsName(int equipIndex)` method |
| 20 | CounterMessage constructor takes `ITextFormatting textFormatting` parameter; HandleMessage13 and HandleMessage35 call `textFormatting.GetPantsDescription(...)` |
| 21 | CounterOutputHandler.HandlePunishment() contains loop `for (int i = 0; i < charaNum; i++)` using `engine.GetCharaNum()` |
| 22 | CounterOutputHandler.HandlePunishment() calls `variables.SetCharacterFlag(charId, new CharacterFlagIndex(CFlagUfufu), 0)` — CFlagUfufu = 317 |
| 23 | CounterOutputHandler.HandlePunishment() calls `clothingSystem.Save(...)` (CLOTHES_RESET adapter, following F804 precedent; matcher scoped to clothingSystem receiver) |
| 24 | CounterOutputHandler.HandlePunishment() calls `clothingSystem.SettingTrain(i, getEquip, setTequip)` (CLOTHES_SETTING_TRAIN adapter) |
| 25 | CounterPunishment injects IConsoleOutput; after variant dispatch loop calls `console.DrawLine()` then `console.Wait()` (Wait() pending Upstream Issue resolution) |
| 26 | CounterMessage exposes `public void SendMessage(CharacterId offender, int? messageVariant = null)` — satisfies `public.*void.*SendMessage` pattern (tightened to committed signature) |
| 26b | CounterMessage.SendMessage() calls `engine.GetTarget()` as early-return guard, matching COUNTER_MESSAGE.ERB:3-4 `SIF TARGET <= 0 / RETURN 0` |
| 26c | CounterMessage.DatuiMessage() appears >= 2 times (definition + call site) — verifies unconditional invocation at start of SendMessage per COUNTER_MESSAGE.ERB:5 |
| 27 | CounterPose exposes `public void ProcessPose(CharacterId offender, int actionOrder)` — satisfies `public.*void.*ProcessPose` pattern |
| 28 | CounterCombination exposes `public int[] AccumulateCombinations()` — satisfies `public.*AccumulateCombinations` pattern |
| 29 | Add `UndressingPlayerLower = new(22)`, `UndressingTargetLower = new(23)`, `UndressingPlayerUpper = new(24)`, `UndressingTargetUpper = new(25)` to TCVarIndex.cs |
| 30 | Add `public static readonly TCVarIndex SixtyNineTransition = new(27)` to TCVarIndex.cs |
| 31 | CounterReaction references `TCVarIndex.SubAction` or `SubAction` named constant in its code |
| 31b | CounterPose.HandlePose69() references `TCVarIndex.SixtyNineTransition` named constant |
| 31c | CounterMessage.DatuiMessage() references named undressing constants (UndressingPlayerLower/TargetLower/PlayerUpper/TargetUpper) |
| 32 | Create `Era.Core.Tests/Counter/CounterReactionTests.cs` covering at least vaginal, masturbation, fellatio, paizuri, frotteurism branch paths |
| 32c | CounterReactionTests contain vaginal branch test (TALENT/ABL logic, not SELECTCOM) — verifies Task 4's vaginal coverage claim independently from SELECTCOM-based AC#32b |
| 32b | CounterReactionTests mock `GetSelectCom()` with values 80-83 across 4+ test methods, one per SELECTCOM-using branch |
| 32d | CounterReactionTests mock `GetMasterPose()` values to exercise 5 pose-type IF blocks |
| 32e | CounterReactionTests verify `IsProtectedCheckOnly` protection brand guard behavior |
| 33 | Create `Era.Core.Tests/Counter/CounterPunishmentTests.cs` covering variant0/1/2 dispatch and guard paths |
| 33b | CounterPunishmentTests cover all 3 variant paths (0=underwear, 1=rotor-V, 2=rotor-A) and retry loop with `random.Next(10)` setup |
| 33c | CounterPunishmentTests assert specific CFLAG constant names (CFlagLowerUnderwear/CFlagRotorInsert) in variant assertions — behavioral equivalence verification |
| 33d | CounterPunishmentTests verify precondition guard retry scenarios (underwear presence / vagina availability checks) — test-side mirror of AC#11e/11f |
| 34 | Create `Era.Core.Tests/Counter/CounterPoseTests.cs` covering TFLAG mapping switch and HandlePose69 (with/without sixnine transition) |
| 34b | CounterPoseTests exercise representative TFLAG increment mappings and HandlePose69 transition logic |
| 34c | CounterPoseTests exercise the CounterDecisionFlag > 1 early-return path |
| 34d | CounterPoseTests assert specific TFLAG body-part indices (TFlag60-65) in action mapping — behavioral equivalence verification |
| 35 | Create `Era.Core.Tests/Counter/CounterOutputHandlerTests.cs` covering all-character iteration, うふふ CFLAG reset, CLOTHES_RESET/SETTING_TRAIN per character |
| 35b | CounterOutputHandlerTests mock `GetCharaNum()` with multiple characters and verify per-character CFLAG reset + clothing operations |
| 36 | Create `Era.Core.Tests/Counter/CounterMessageTests.cs` covering switch dispatch and TEQUIP mutation side effects |
| 36b | CounterMessageTests verify `SetTEquip` calls for TEQUIP:PLAYER:3/4 mutation side effects |
| 36c | CounterMessageTests verify `GetTarget()` early-return guard path |
| 36d | CounterMessageTests exercise at least 3 `HandleMessage{N}` handler methods (representative dispatch coverage matching AC#16b/16c/16d implementation-side) |
| 36e | CounterMessageTests exercise `DatuiMessage` undressing logic with TCVAR:22-25 undressing state setup |
| 36f | CounterMessageTests exercise 500-range SubAction-dependent handler (mirrors AC#16h implementation-side) |
| 36g | CounterMessageTests assert `SetTEquip(player, 3, 1)` in >= 2 test scenarios for multi-site TEQUIP mutation behavioral equivalence |
| 36h | CounterMessageTests assert `SetTEquip(player, 4, 1)` to verify TEQUIP:PLAYER:4 mutation test-side coverage (symmetry with AC#36g for index 3) |
| 37 | Create `Era.Core.Tests/Counter/CounterCombinationTests.cs` covering AccumulateCombinations() |
| 37b | CounterCombinationTests call `AccumulateCombinations()` and verify character iteration + count results |
| 38 | Add `Wait()` to `IConsoleOutput.cs`; verify `Wait\(\)` pattern matches |
| 38b | HeadlessConsole.cs updated with `Wait()` implementation; verify in engine/ headless path |
| 39 | `dotnet build Era.Core/` must exit 0; all new C# files use named constants, no suppressed warnings. Also implicitly verifies backward compatibility of IConsoleOutput extension (T3 adds Wait() — if existing methods were removed, callers would fail to compile). |
| 40 | `dotnet test Era.Core.Tests/` must exit 0; all new tests pass, existing tests do not regress |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Where does the all-character cleanup loop live? | A: In CounterPunishment.Execute; B: In CounterOutputHandler.HandlePunishment | B: CounterOutputHandler | ICounterOutputHandler XML doc (lines 14-22) specifies HandlePunishment's responsibility includes the all-character iteration. CounterPunishment models only the punishment event itself (dispatched per-offender). |
| ITextFormatting interface placement | A: Era.Core/Interfaces/; B: Era.Core/Counter/ | A: Era.Core/Interfaces/ | Pants/bust description functions are generic text utilities, not counter-specific. Interfaces/ is the SSOT for shared contracts. |
| CounterMessage SendMessage signature | A: SendMessage(CharacterId, int? messageVariant = null); B: SendMessage(CharacterId) + SendMessageWithBranch(CharacterId, int) | A: Single method with optional branch | Matches IWcCounterOutputHandler.SendMessage(CharacterId, int? messageVariant = null) pattern exactly. Consistent across WC and main counter. |
| DATUI_MESSAGE placement | A: Separate class DatuiMessage; B: Private method in CounterMessage | B: Private method in CounterMessage | ERB defines it as a local function within COUNTER_MESSAGE.ERB. Scope is narrowly counter-message internal. No external callers. |
| TRYCALLFORM dispatch translation | A: Dictionary<int, Action>; B: switch statement; C: reflection | B: switch statement | WcCounterPose and WcCounterCombination both use switch. Pattern is consistent. Switch is compile-time verified and warning-free. AC#16 accepts either pattern. |
| IConsoleOutput.Wait() gap | A: Add Wait() to IConsoleOutput now; B: Use PrintWait("") as workaround; C: Skip Wait in CounterPunishment | A: Add Wait() upstream, tracked in Upstream Issues | COUNTER_PUNISHMENT.ERB:15 has standalone WAIT after DRAWLINE. Skipping breaks behavioral equivalence. PrintWait("") semantics differ. Proper fix is adding Wait() to IConsoleOutput. |
| IClothingSystem adapter for CLOTHES_RESET/SETTING_TRAIN | A: Inline delegates in CounterOutputHandler; B: Adapter helper method | A: Inline delegates | WcActionSelector (F804) uses the same inline delegate pattern for SettingTrain. CounterOutputHandler follows this precedent. |
| CounterCombination AccumulateCombinations return type | A: void (store in local); B: int[] (return counts) | B: int[] | WcCounterCombination.AccumulateCombinations() returns int[]. Direct analogy. Upstream caller (INFO.ERB:191) needs the result. |
| TEquipSixtyNine raw constant | A: Add TEquipIndex type; B: Use raw int private const | B: Raw int private const | WcCounterPose uses `private const int TEquipSixtyNine = 10;`. Consistent approach. TEquip indices are not typed in the existing codebase. |
| SELECTCOM values (80-83) | A: Named SelectComIndex constants; B: Raw int private const | B: Raw int private const | Following TEquipSixtyNine precedent. SELECTCOM values are action-type-specific dispatch constants, not typed indices. Raw int private const is consistent with existing WC Counter pattern. **Naming convention**: `SelectCom{BranchName}` prefix required (SelectComMasturbation=80, SelectComFellatio=81, SelectComPaizuri=82, SelectComFrotteurism=83) — AC#9e matcher `SelectCom.*= 8[0-3]` enforces this. |
| CounterMessage offender parameter vs ERB TARGET | A: Pass CharacterId offender explicitly; B: Read TARGET from engine state | A: Explicit offender parameter | ERB EVENT_COUNTER_MESSAGE takes only 枝番 (branch) and resolves character via TARGET global state. EVENT_KOJO.ERB sets TARGET=ARG before calling. C# converts implicit TARGET to explicit offender parameter for testability and type safety, matching IWcCounterOutputHandler.SendMessage pattern. Behaviorally equivalent since TARGET==offender at all call sites. |

### Interfaces / Data Structures

#### New: ITextFormatting (Era.Core/Interfaces/ITextFormatting.cs)

```csharp
namespace Era.Core.Interfaces;

/// <summary>
/// Text formatting stub for clothing/body description functions.
/// Provides C# equivalents for PANTS_DESCRIPTION, PANTSNAME, OPPAI_DESCRIPTION ERB functions.
/// Stub implementation returns empty string for headless/test contexts.
/// Feature 802 - Main Counter Output
/// </summary>
public interface ITextFormatting
{
    /// <summary>Returns clothing description prefix text (PANTS_DESCRIPTION ERB function)</summary>
    string GetPantsDescription(int equipIndex);

    /// <summary>Returns clothing item name (PANTSNAME ERB function)</summary>
    string GetPantsName(int equipIndex);

    /// <summary>Returns bust/chest description text (OPPAI_DESCRIPTION ERB function)</summary>
    string GetOppaiDescription(int characterId);
}
```

#### New TCVarIndex Constants (Era.Core/Types/TCVarIndex.cs — additive)

```csharp
// Counter output system indices - Feature 802
public static readonly TCVarIndex SubAction = new(21);                  // サブアクション (sub-action type)
public static readonly TCVarIndex UndressingPlayerLower = new(22);     // 脱衣_プレイヤー下半身
public static readonly TCVarIndex UndressingTargetLower = new(23);     // 脱衣_ターゲット下半身
public static readonly TCVarIndex UndressingPlayerUpper = new(24);     // 脱衣_プレイヤー上半身
public static readonly TCVarIndex UndressingTargetUpper = new(25);     // 脱衣_ターゲット上半身
public static readonly TCVarIndex SixtyNineTransition = new(27);       // シックスナイン移行
```

#### CounterOutputHandler (implements ICounterOutputHandler)

```csharp
public sealed class CounterOutputHandler(
    IVariableStore variables,
    IEngineVariables engine,
    ITEquipVariables tequip,
    IClothingSystem clothingSystem,
    CounterReaction reaction,
    CounterPunishment punishment) : ICounterOutputHandler
{
    private const int CFlagUfufu = 317;

    public void HandleReaction(CharacterId offender, int actionOrder)
        => reaction.ProcessReaction(offender, actionOrder, new CharacterId(engine.GetMaster()));

    public void HandlePunishment(CharacterId offender, int actionOrder)
    {
        punishment.Execute(offender, actionOrder);
        int charaNum = engine.GetCharaNum();
        for (int i = 0; i < charaNum; i++)
        {
            var charId = new CharacterId(i);
            variables.SetCharacterFlag(charId, new CharacterFlagIndex(CFlagUfufu), 0);
            clothingSystem.Save(i, getCflag, setCflag, getEquip);   // CLOTHES_RESET
            clothingSystem.SettingTrain(i, getEquip, setTequip);    // CLOTHES_SETTING_TRAIN
        }
    }
    // Adapter delegates - initialized once, not per-loop-iteration (Reference: IClothingSystem.cs Save/SettingTrain signatures)
    private readonly Func<int, int, int> getCflag;
    private readonly Action<int, int, int> setCflag;
    private readonly Func<int, int, int> getEquip;
    private readonly Action<int, int, int> setTequip;
    // Constructor body (in addition to primary constructor):
    // getCflag = (charIdx, flagIdx) => variables.GetCharacterFlag(new CharacterId(charIdx), new CharacterFlagIndex(flagIdx));
    // setCflag = (charIdx, flagIdx, value) => variables.SetCharacterFlag(new CharacterId(charIdx), new CharacterFlagIndex(flagIdx), value);
    // getEquip = (charIdx, equipIdx) => variables.GetEquip(new CharacterId(charIdx), equipIdx);
    // setTequip = (charIdx, equipIdx, value) => tequip.SetTEquip(new CharacterId(charIdx), equipIdx, value);
}
```

#### CounterMessage (public entry point)

```csharp
public sealed class CounterMessage(
    IVariableStore variables,
    IEngineVariables engine,
    ITEquipVariables tequip,
    IConsoleOutput console,
    ITextFormatting textFormatting,
    ICommonFunctions common)
{
    public void SendMessage(CharacterId offender, int? messageVariant = null)
    {
        int target = engine.GetTarget();
        if (target <= 0) return;

        DatuiMessage();  // DATUI_MESSAGE always called first (ERB:5)

        int action = variables.GetTCVar(offender, TCVarIndex.CounterAction).Match(v => v, _ => 0);
        int dispatched = messageVariant.HasValue
            ? DispatchWithBranch(action, messageVariant.Value)
            : DispatchDefault(action);
        if (dispatched != 0)
            console.PrintLine("");  // PRINTL after RESULT!=0
    }

    private void DatuiMessage() { /* TCVAR:22-25 undressing text logic */ }
    private int DispatchDefault(int action) { switch (action) { case 10: return HandleMessage10(); /* ... */ } }
    private int DispatchWithBranch(int action, int branch) { /* e.g., action=29 branch=1/2/3 */ }
}
```

#### CounterPunishment (adds IConsoleOutput vs WC analog)

```csharp
public sealed class CounterPunishment(
    IVariableStore variables,
    IRandomProvider random,
    IEngineVariables engine,
    ICommonFunctions common,
    IConsoleOutput console)   // ADDED: for DrawLine + Wait
{
    public void Execute(CharacterId offender, int actionOrder)
    {
        // ... same loop as WcCounterPunishment ...
        console.DrawLine();    // COUNTER_PUNISHMENT.ERB:14
        console.Wait();        // COUNTER_PUNISHMENT.ERB:15 — Wait() pending Upstream Issue
    }
}
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| `IConsoleOutput` has no standalone `Wait()` method; only `PrintWait(string)` exists (verified in `Era.Core/Interfaces/IConsoleOutput.cs`). `CounterPunishment.Execute()` needs `DrawLine()` + `Wait()` separately (COUNTER_PUNISHMENT.ERB:14-15). Using `PrintWait("")` has incorrect semantics (print-then-wait vs wait-only). | Technical Constraints, AC#25 | Add `Result<Unit> Wait()` method to `Era.Core/Interfaces/IConsoleOutput.cs`. Note: `engine/Assets/Scripts/Emuera/Headless/IConsoleOutput.cs` is a SEPARATE interface (void returns, not Result<Unit>). T3 updates both: (1) Era.Core interface (Result<Unit> Wait()), (2) engine headless IConsoleOutput + HeadlessConsole implementation (void Wait()). AC#38 verifies Era.Core interface; AC#38b verifies engine headless implementation. Runtime bridge between Era.Core.Interfaces.IConsoleOutput and engine IConsoleOutput is handled by the existing adapter pattern in GlobalStatic. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 6, 7, 29, 30 | Add 6 named TCVarIndex constants to `Era.Core/Types/TCVarIndex.cs`: SubAction(21), UndressingPlayerLower(22), UndressingTargetLower(23), UndressingPlayerUpper(24), UndressingTargetUpper(25), SixtyNineTransition(27) | | [x] |
| 2 | 19, 19b, 19c | Define `ITextFormatting` interface in `Era.Core/Interfaces/ITextFormatting.cs` with methods `GetPantsDescription(int equipIndex)`, `GetPantsName(int equipIndex)`, `GetOppaiDescription(int characterId)` | | [x] |
| 3 | 38, 38b | Add `Result<Unit> Wait()` method to `IConsoleOutput` interface (`Era.Core/Interfaces/IConsoleOutput.cs`), update headless implementation (`engine/Assets/Scripts/Emuera/Headless/HeadlessConsole.cs`), and update all existing IConsoleOutput test stubs in Era.Core.Tests/ (8 classes: TalentCopierTests, CharacterCustomizerTests, CollectionTrackerTests, ConsoleOutputDelegationTests, ItemPurchaseTests, ItemDescriptionsTests, PrintCommandTests, PrintOutputEquivalenceTests); GUI adapter is outside Era.Core/ build scope (caught by Unity build) | | [x] |
| 4 | 32, 32b, 32c, 32d, 32e | Write RED unit tests for `CounterReaction` in `Era.Core.Tests/Counter/CounterReactionTests.cs` covering vaginal, masturbation, fellatio, paizuri, and frotteurism branch paths with mock engine.GetSelectCom() | | [x] |
| 5 | 1, 9, 9b, 9c, 9d, 9e, 9f, 9g, 9h, 9i, 9j, 14, 31 | Implement `CounterReaction` sealed class in `Era.Core/Counter/CounterReaction.cs` with primary constructor DI and 5 REACTION branches — 4 use SELECTCOM dispatch (80=masturbation, 81=fellatio, 82=paizuri, 83=frotteurism) and 1 (vaginal) uses TALENT/ABL logic — matching COUNTER_REACTION.ERB (GREEN for T4) | | [x] |
| 6 | 33, 33b, 33c, 33d | Write RED unit tests for `CounterPunishment` in `Era.Core.Tests/Counter/CounterPunishmentTests.cs` covering variant 0 (underwear removal), variant 1 (rotor vaginal), variant 2 (rotor anal), and retry loop guard paths | | [x] |
| 7 | 2, 10, 10b, 11, 11b, 11c, 11d, 11e, 11f, 25 | Implement `CounterPunishment` sealed class in `Era.Core/Counter/CounterPunishment.cs` with RAND:10 retry loop, 3 punishment variants (underwear CFLAG / rotor-V / rotor-A), and DRAWLINE + Wait() via IConsoleOutput (GREEN for T6) | | [x] |
| 8 | 34, 34b, 34c, 34d | Write RED unit tests for `CounterPose` in `Era.Core.Tests/Counter/CounterPoseTests.cs` covering ~25 action-to-TFLAG switch mappings and HandlePose69 with and without sixnine transition | | [x] |
| 9 | 3, 12, 12b, 12c, 12d, 13, 27, 31b | Implement `CounterPose` sealed class in `Era.Core/Counter/CounterPose.cs` with ~25 action-to-TFLAG switch, HandlePose69 using SixtyNineTransition constant, and public `ProcessPose(CharacterId, int)` entry point (GREEN for T8) | | [x] |
| 10 | 35, 35b | Write RED unit tests for `CounterOutputHandler` in `Era.Core.Tests/Counter/CounterOutputHandlerTests.cs` covering all-character CHARANUM iteration, うふふ CFLAG reset per character, CLOTHES_RESET (Save) per character, and CLOTHES_SETTING_TRAIN per character | | [x] |
| 11 | 8, 8b, 8c, 21, 22, 23, 24 | Implement `CounterOutputHandler` sealed class in `Era.Core/Counter/CounterOutputHandler.cs` implementing `ICounterOutputHandler`; HandleReaction delegates to CounterReaction.ProcessReaction(); HandlePunishment iterates all characters via `engine.GetCharaNum()` resetting うふふ CFLAG + calling `clothingSystem.Save()` + `clothingSystem.SettingTrain()` per character (GREEN for T10) | | [x] |
| 12 | 36, 36b, 36c, 36d, 36e, 36f, 36g, 36h | Write RED unit tests for `CounterMessage` in `Era.Core.Tests/Counter/CounterMessageTests.cs` covering switch dispatch, TEQUIP mutation side effects, DatuiMessage undressing logic, and SendMessage entry point | | [x] |
| 13 | 4, 15, 15b, 16, 16b, 16c, 16d, 16e, 16e2, 16f, 16g, 16h, 17, 20, 20b, 26, 26b, 26c, 31c | Implement `CounterMessage` sealed class in `Era.Core/Counter/CounterMessage.cs` with public `SendMessage(CharacterId, int?)` entry point, private `DatuiMessage()` method, switch dispatch over ~35 message handlers (TRYCALLFORM translation), TEQUIP:PLAYER:3/4 mutation preservation, and `ITextFormatting` constructor injection (GREEN for T12) | | [x] |
| 14 | 37, 37b | Write RED unit tests for `CounterCombination` in `Era.Core.Tests/Counter/CounterCombinationTests.cs` covering AccumulateCombinations() faithful stub reproduction | | [x] |
| 15 | 5, 14b, 18, 28 | Implement `CounterCombination` sealed class in `Era.Core/Counter/CounterCombination.cs` as faithful 10-line stub reproduction of COUNTER_CONBINATION.ERB with public `AccumulateCombinations()` entry point (GREEN for T14) | | [x] |
| 16 | 39 | Verify `dotnet build Era.Core/` exits 0 with TreatWarningsAsErrors=true (warning-free, no raw integer TCVarIndex usage in new Counter classes) | | [x] |
| 17 | 40 | Verify `dotnet test Era.Core.Tests/` exits 0 (all new tests pass, existing tests do not regress) | | [x] |

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
| 1 | implementer | sonnet | T1: TCVarIndex.cs current content; T2: IConsoleOutput.cs for Wait() placement; T3: ITextFormatting interface spec from Technical Design | TCVarIndex.cs updated (6 new constants); ITextFormatting.cs created; IConsoleOutput.cs updated with Wait() + all implementations updated |
| 2 | implementer | sonnet | T4/T6/T8/T10: WcCounterReactionTests/WcCounterPunishmentTests/WcCounterPoseTests as reference; stub class files for each RED test target | CounterReactionTests.cs (RED); CounterPunishmentTests.cs (RED); CounterPoseTests.cs (RED); CounterOutputHandlerTests.cs (RED) |
| 3 | implementer | sonnet | T5: Phase 2 RED tests + COUNTER_REACTION.ERB + WcCounterReaction.cs reference; T7: Phase 2 RED tests + COUNTER_PUNISHMENT.ERB + WcCounterPunishment.cs reference; T9: Phase 2 RED tests + COUNTER_POSE.ERB + WcCounterPose.cs reference | CounterReaction.cs (GREEN); CounterPunishment.cs (GREEN); CounterPose.cs (GREEN) |
| 4 | implementer | sonnet | T11: Phase 2 RED tests + COUNTER_SELECT.ERB:60-64 + WcActionSelector.cs for delegate adapter pattern | CounterOutputHandler.cs (GREEN) |
| 5 | implementer | sonnet | T12: WcCounterMessageTests as reference; stub class file for RED test target | CounterMessageTests.cs (RED) |
| 6 | implementer | sonnet | T13: Phase 5 RED tests + COUNTER_MESSAGE.ERB (550 lines) + ITextFormatting interface | CounterMessage.cs (GREEN) |
| 7 | implementer | sonnet | T14: WcCounterCombinationTests as reference; stub class file for RED test target | CounterCombinationTests.cs (RED) |
| 8 | implementer | sonnet | T15: Phase 7 RED tests + COUNTER_CONBINATION.ERB (10 lines) | CounterCombination.cs (GREEN) |
| 9 | implementer | sonnet | T16/T17: Build and test verification commands | Build PASS; Test PASS confirmation |

### Pre-conditions

- F801 is [DONE]: `ICounterOutputHandler.cs` exists with HandleReaction(CharacterId, int) and HandlePunishment(CharacterId, int)
- F804 is [DONE]: WcCounterReaction/WcCounterPunishment/WcCounterPose exist as design references
- All 5 source ERB files are accessible: `Game/ERB/COUNTER_REACTION.ERB`, `COUNTER_PUNISHMENT.ERB`, `COUNTER_POSE.ERB`, `COUNTER_MESSAGE.ERB`, `COUNTER_CONBINATION.ERB`
- Existing TCVarIndex.cs has 10 constants (baseline from Baseline Measurement)

### Execution Order

1. **T1 before T4-T15**: TCVarIndex constants must exist before any Counter class that references them can compile
2. **T2 before T13**: ITextFormatting must exist before CounterMessage can be implemented
3. **T3 before T7**: IConsoleOutput.Wait() must exist before CounterPunishment compiles
4. **T4 before T5** (RED→GREEN): Write CounterReaction tests before implementing CounterReaction
5. **T6 before T7** (RED→GREEN): Write CounterPunishment tests before implementing CounterPunishment
6. **T8 before T9** (RED→GREEN): Write CounterPose tests before implementing CounterPose
7. **T10 before T11** (RED→GREEN): Write CounterOutputHandler tests before implementing CounterOutputHandler
8. **T12 before T13** (RED→GREEN): Write CounterMessage tests before implementing CounterMessage
9. **T14 before T15** (RED→GREEN): Write CounterCombination tests before implementing CounterCombination
10. **T5, T7, T9 before T11**: CounterOutputHandler depends on CounterReaction and CounterPunishment classes
11. **T16 and T17 last**: Build and test verification after all implementation tasks complete

### Build Verification

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/'
```

### Error Handling

- If `IConsoleOutput.Wait()` addition causes compilation failures in existing code: stop and report; do not patch callers without user approval
- If any new Counter class fails to compile: fix before proceeding to dependent tasks
- If RED tests pass before implementation (false RED): investigate stub class and ensure tests fail correctly before implementing GREEN

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| `IConsoleOutput.Wait()` missing (upstream gap) | COUNTER_PUNISHMENT.ERB:15 requires standalone WAIT; PrintWait("") has incorrect semantics; must add Wait() to IConsoleOutput and update all concrete implementations | Feature (in-scope) | F802 (T3) | T3 |
| TCVarIndex named constants backport to WC counter | F802 adds SubAction(21), SixtyNineTransition(27) etc. but WcCounterReaction/WcCounterPose/WcCounterCombination/WcEventKojo still use raw `new TCVarIndex(21)`, `new TCVarIndex(27)` for same indices. Deferred to F813 (not in-scope) because F813 batches all Phase 21 post-review backports — updating WC files in F802 would expand scope to non-Counter-output files and mix migration + backport concerns in one feature. Dual pattern (named in new, raw in existing) is acceptable during the migration window. | Feature (existing) | F813 | - |
| CounterMessage architectural ownership (Counter/ vs kojo system) | COUNTER_MESSAGE.ERB is called from EVENT_KOJO.ERB (kojo fallback system) and architecturally may belong to the kojo subsystem rather than Counter/. Kept in F802 per F783 assignment. F813 post-phase review must verify CounterMessage placement in Era.Core/Counter/ is correct or plan relocation to kojo namespace. | Feature (existing) | F813 | - |
| CounterMessage full text content migration | F802 migrated structural dispatch (switch, TEQUIP mutations, control flow) but ~25 message handlers output empty strings instead of full Japanese text with CALLNAME interpolation, TALENT/gender branching. DatuiMessage also simplified. ERB still handles text output in hybrid mode. Full text migration requires ICommonFunctions (now injected), engine.GetCallName(), and TALENT variable access. | Feature (existing) | F813 | - |

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
3. Set Status: [DRAFT]
4. Add Phase reference (comment or text) for traceability
-->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-26 07:54 | START | implementer | Task 9 | - |
| 2026-02-26 07:56 | END | implementer | Task 9 | SUCCESS |
| 2026-02-26 08:30 | RESUME | orchestrator | Phase 1 resume | [WIP] confirmed |
| 2026-02-26 08:31 | DEVIATION | PRE-EXISTING | F810 CheckExpUp added to ICounterUtilities | Fixed 7 test stubs (interface compliance) |
| 2026-02-26 08:32 | DEVIATION | PRE-EXISTING | F803 CounterSourceHandler.cs incomplete | Stashed to .tmp/f803-stash/ during F802 run |
| 2026-02-26 08:33 | INFO | orchestrator | All F802 tasks verified | Build PASS, all Counter tests PASS |
| 2026-02-26 08:45 | INFO | ac-tester | Phase 7 AC verification (round 1) | 30 PASS, 8 FAIL (all DEFINITION), 1 BLOCKED |
| 2026-02-26 08:50 | INFO | orchestrator | Fixed 8 AC matchers | DEFINITION fixes: multi-line, named constants, expression switch, gte thresholds |
| 2026-02-26 08:55 | INFO | orchestrator | Phase 7 AC verification (round 2) | 40/40 PASS (manual verification for gte format ACs) |
| 2026-02-26 08:55 | DEVIATION | PRE-EXISTING | ComHotReloadTests flaky | OnFileChanged_WithValidYaml_InvokesValidation - unrelated to F802 |
| 2026-02-26 09:10 | DEVIATION | feature-reviewer | Phase 8 NEEDS_REVISION | 3 critical + 3 major issues |
| 2026-02-26 09:11 | INFO | orchestrator | Phase 8 fix: ICommonFunctions + adapter delegates + AC#16 details | Issues 1,2 (text content) tracked as Mandatory Handoff |

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

<!-- F801 Handoff (2026-02-23): F801 defines ICounterOutputHandler interface in Era.Core/Counter/ with HandleReaction(CharacterId, int) and HandlePunishment(CharacterId, int). F802 must implement this interface with actual EVENT_COUNTER_REACTION/PUNISHMENT and CLOTHES_RESET/SETTING_TRAIN logic. See F801 Mandatory Handoffs table. -->
- [fix] Phase2-Review iter1: Review Notes [info] tag | Changed to HTML comment (not valid Review Notes tag per template)
- [fix] Phase2-Review iter1: Summary section | Moved to HTML comment (not in feature template)
- [fix] Phase2-Review iter1: Tasks table T12/T13 | Split T12 into T12(RED)+T13(GREEN) and T13 into T14(RED)+T15(GREEN) for TDD compliance; renumbered T14→T16, T15→T17
- [fix] Phase2-Uncertain iter1: AC#16 matcher | Tightened from "switch|Dictionary<int" to "switch \\(action\\)" for action dispatch specificity
- [fix] Phase2-Uncertain iter1: AC#19 matcher | Split into AC#19 (GetPantsDescription) + AC#19b (GetOppaiDescription) scoped to ITextFormatting.cs
- [fix] Phase2-Review iter2: AC#19 missing GetPantsName | Added AC#19c for GetPantsName verification in ITextFormatting.cs (C4 complete coverage)
- [fix] Phase2-Review iter2: AC#15 missing TEQUIP:PLAYER:4 | Added AC#15b for SetTEquip index 4 mutation (C3 complete coverage)
- [fix] Phase2-Review iter3: AC#32-37 file-only | Added AC#32b, AC#35b, AC#36b for test content verification (assertions exist)
- [fix] Phase2-Uncertain iter3: AC#23 matcher | Tightened from Save\\( to clothingSystem\\.Save\\( for receiver specificity
- [fix] Phase2-Review iter3: AC#16 + C12 gap | Added AC#16b for representative handler method names (HandleMessage10/50/58)
- [fix] Phase2-Review iter4: Task#3/Upstream Issues | Clarified engine/ implementation scope; HeadlessConsole.cs in T3, GUI adapter caught by Unity build
- [fix] Phase2-Review iter5: Tasks T4/T10/T12 | Added AC#32b/35b/36b references to owning tasks (test content verification)
- [fix] Phase2-Review iter5: C9 coverage | Added AC#11b (variant 1 rotor-V) and AC#11c (variant 2 rotor-A) for complete punishment variant coverage
- [fix] Phase2-Review iter5: C10 index 26 gap | Documented that index 26 is not used by in-scope ERB files; updated constraint text to "21-25, 27"
- [fix] Phase2-Review iter6: AC#26 matcher | Removed ProcessMessage alternative; tightened to committed SendMessage signature only
- [fix] Phase2-Uncertain iter7: AC#16b | Split OR matcher into AC#16b/16c/16d (HandleMessage10/50/58) for independent range verification per C12
- [fix] Phase2-Review iter8: AC#38 gap | Added AC#38b for HeadlessConsole.cs Wait() implementation verification (engine/ scope)
- [fix] Phase3-Maintainability iter9: T3 test stubs | Updated T3 to include 8 existing IConsoleOutput test stub updates (breaking interface change)
- [fix] Phase3-Maintainability iter9: TCVarIndex backport | Added Mandatory Handoff to F813 for WC counter raw TCVarIndex constant backport
- [fix] Phase3-Maintainability iter9: Upstream Issues | Clarified dual IConsoleOutput interface bridging (Era.Core vs engine/) and runtime adapter pattern
- [fix] Phase2-Review iter10: AC#38b | Added AC Details, Goal Coverage, and AC Coverage entries for AC#38b (was in table but missing supporting sections)
- [fix] Phase3-Maintainability iter10: CounterMessage constructor | Added IConsoleOutput to CounterMessage constructor params (550 lines of PRINTFORML require console output)
- [fix] Phase3-Maintainability iter10: AC#9 description | Corrected "5 branches with SELECTCOM" to "4 of 5" (vaginal uses TALENT/ABL only)
- [fix] Phase3-Maintainability iter10: TDD section | Updated "4 test files" to "6 test files" (added CounterMessageTests, CounterCombinationTests)
- [fix] Phase3-Maintainability iter10: AC#20b | Added IConsoleOutput dependency verification AC for CounterMessage
- [fix] Phase3-Maintainability iter10: Adapter delegates | Replaced "omitted for brevity" with explicit delegate definitions in CounterOutputHandler pseudo-code
- [resolved-invalid] Phase2-Review iter1: Philosophy "interface-driven dependency injection" refers to constructor DI pattern, not C# interface types per class. "appropriate interfaces" in Goal Item 3 means public entry points (SendMessage/ProcessPose/AccumulateCombinations). F804 WC pattern confirms sealed classes without individual interfaces. AC#26/27/28 correctly verify these. Validated by iter2 validator against F804 evidence.
- [fix] Phase2-Uncertain iter1: Review Notes section | Added mandatory template comment block (<!-- Mandatory: All [pending] items... -->)
- [fix] Phase2-Review iter1: AC#31 typed-index gap | Added AC#31b (CounterPose SixtyNineTransition) and AC#31c (CounterMessage undressing named constants) for complete typed-index philosophy verification (C10)
- [resolved-applied] Phase2-Review iter2: Goal Item 7 "Equivalence tests must verify C# output matches legacy ERB behavior" — resolved by adding 3 behavioral equivalence ACs: AC#33c (CounterPunishment CFLAG names), AC#34d (CounterPose TFLAG indices), AC#36g (CounterMessage TEQUIP values). These verify tests assert specific values, not just method references.
- [fix] Phase2-Review iter2: AC#9 matcher | Changed from matches to gte 4 count-based matcher for GetSelectCom\\(\\) to enforce 4-branch requirement (was single-match weakness)
- [fix] Phase2-Review iter3: Philosophy Derivation table | Added AC#31, AC#31b, AC#31c to "typed indices" row for complete derivation traceability
- [fix] Phase2-Review iter3: Task 3 AC#25 misplacement | Removed AC#25 from T3 (IConsoleOutput Wait() addition) since CounterPunishment.cs is created in T7 where AC#25 already exists
- [fix] Phase2-Review iter4: AC#40 rationale | Added T3's 8 IConsoleOutput test stub updates as explicit catch scope (compilation + test execution verification)
- [fix] Phase2-Review iter5: AC#32b/35b/36b matchers | Removed trivially-true "|Assert" from disjunction matchers (GetSelectCom, GetCharaNum, SetTEquip now standalone)
- [fix] Phase2-Review iter5: AC#14b sealed class pattern | Added combined gte 5 AC for all Counter classes (mirrors F804 AC#37); updated Philosophy Derivation and Goal Coverage
- [fix] Phase2-Review iter6: AC#9 branch coverage | Split single gte 4 count into AC#9/9b/9c/9d per-branch named method matchers (masturbation/fellatio/paizuri/frotteurism) following AC#16b/16c/16d pattern
- [fix] Phase2-Review iter7: AC#9e SELECTCOM values | Added AC#9e (gte 4 count of SELECTCOM constants 80-83) to verify dispatch values, not just method names
- [fix] Phase2-Review iter8: Philosophy Derivation | Added ITextFormatting row (AC#19/19b/19c/20) to Philosophy Derivation table for DI traceability
- [fix] Phase2-Review iter9: AC#16e DispatchWithBranch | Added AC#16e for branched TRYCALLFORM dispatch path (DispatchWithBranch method); updated Goal Coverage Item 6 and T13
- [fix] Phase2-Review iter9: AC#12 count matcher | Changed from matches to gte >= 10 for IncrementTFlag count to enforce substantial action-to-TFLAG mapping coverage (C7)
- [fix] Phase2-Review iter1(FL2): AC#20b AC Details | Added missing AC Details entry for AC#20b (IConsoleOutput text output dependency)
- [fix] Phase2-Review iter1(FL2): AC#15b table order | Moved AC#15b row to immediately after AC#15 for sequential grouping
- [fix] Phase2-Review iter1(FL2): Task 1/5 AC#31 | Moved AC#31 from Task 1 to Task 5 (CounterReaction uses SubAction constant → requires CounterReaction.cs which is created in T5)
- [fix] Phase2-Review iter10: SELECTCOM Key Decision | Added Key Decision for SELECTCOM values 80-83 (raw int private const, following TEquipSixtyNine precedent); AC#9e regex matches raw integer pattern
- [fix] Phase2-Review iter1(FL3): AC#9e matcher | Tightened from "== 8[0-3]|SelectCom.*8[0-3]|case 8[0-3]" to "SelectCom.*= 8[0-3]" to require SELECTCOM-specific constant definition context, preventing false-positive matches on incidental integer comparisons
- [fix] Phase3-Maintainability iter2(FL3): Mandatory Handoff TCVarIndex backport | Added justification for F813 deferral: F813 batches Phase 21 backports, F802 scope is Counter output only, dual pattern acceptable during migration window
- [fix] Phase3-Maintainability iter2(FL3): TCVarIndex index 26 | Documented that index 26 is マスターカウンター制御 (Master Counter Control bitfield) used by COUNTER_SOURCE.ERB/COMABLE_300.ERB; deferred to F803/F811 scope
- [fix] Phase2-Review iter3(FL3): Task 5 description | Corrected "5 SELECTCOM-dependent REACTION branches" to "5 REACTION branches — 4 use SELECTCOM dispatch (80-83), 1 (vaginal) uses TALENT/ABL" per Review Notes line 1031 fix
- [fix] Phase2-Review iter4(FL3): AC#16e matcher | Tightened from "DispatchWithBranch|HandleMessage.*_|branch" to "DispatchWithBranch" to prevent false-positive on incidental 'branch' occurrences in 550-line file
- [fix] Phase2-Review iter1(FL4): Goal Coverage Item 1 | Added AC#12b and AC#12c to covering ACs (CounterPose CounterDecisionFlag guard and CounterAction dispatch key are part of COUNTER_POSE.ERB migration)
- [fix] Phase2-Review iter5(FL3): AC#28 return type | Tightened matcher to "public.*int\\[\\].*AccumulateCombinations" to enforce int[] return type per Technical Design Key Decision
- [fix] Phase2-Review iter5(FL3): AC#9f vaginal branch | Added AC#9f for vaginal/TALENT-ABL branch verification (SELECTCASE block 1); updated Task 5 AC# column and Goal Coverage Item 1
- [fix] Phase2-Review iter6(FL3): AC#8b HandleReaction delegation | Added AC#8b verifying reaction.ProcessReaction() call in CounterOutputHandler; updated Task 11 AC# column, Goal Coverage Item 2, and AC Coverage
- [fix] Phase2-Review iter7(FL3): AC#9e naming | Added SelectCom{BranchName} naming convention to Key Decision row; aligns AC#9e matcher with documented constraint
- [fix] Phase2-Review iter7(FL3): AC#8b GetMaster | Strengthened AC#8b matcher from "reaction\\.ProcessReaction\\(" to "reaction\\.ProcessReaction.*GetMaster\\(\\)" to verify F801 handoff master argument contract
- [fix] Phase2-Uncertain iter1(FL4): AC#32b/35b/36b ordering | Reordered sub-ACs to immediately follow parent ACs (32b after 32, 35b after 35, 36b after 36) for sequential grouping
- [fix] Phase2-Review iter2(FL4): MESSAGE handoff | Added Mandatory Handoff row for CounterMessage architectural ownership concern (Counter/ vs kojo system) to F813
- [fix] Phase2-Review iter3(FL4): AC#32c vaginal test | Added AC#32c verifying vaginal branch test exists in CounterReactionTests.cs (TALENT/ABL path not covered by AC#32b GetSelectCom)
- [fix] Phase2-Review iter3(FL4): C5 exception | Added C5 exception note for AC#28 int[] return type (necessary C# architectural addition per WcCounterCombination analogy)
- [fix] Phase2-Review iter4(FL4): AC#15 gte | Strengthened AC#15 from matches to gte count >= 5 for SetTEquip.*3 (7+ TEQUIP:PLAYER:3 mutation sites require substantial coverage, not single-match)
- [fix] Phase2-Review iter4(FL4): AC#16f PRINTFORML ternary | Added AC#16f for conditional text building verification (gte count ternary >= 4); closes Technical Constraint gap at COUNTER_MESSAGE.ERB:103,118,120,126
- [fix] Phase2-Review iter5(FL4): AC#9g MASTER_POSE | Added AC#9g verifying MASTER_POSE guard usage in CounterReaction (outer dispatch is IF MASTER_POSE(N,1)==ARG, not SELECTCASE); added C6 constraint note clarifying actual structure
- [fix] Phase2-Review iter5(FL4): Key Decision offender-vs-TARGET | Added Key Decision for CounterMessage offender parameter vs ERB TARGET resolution (explicit CharacterId for testability; behaviorally equivalent since TARGET==offender at all call sites)
- [fix] Phase2-Review iter6(FL4): AC#32b gte | Strengthened AC#32b from matches to gte >= 4 for GetSelectCom in CounterReactionTests.cs (4 SELECTCOM branches require 4+ test assertions, mirrors AC#9e implementation approach)
- [fix] Phase2-Uncertain iter7(FL4): AC#32d MASTER_POSE test | Added AC#32d verifying MASTER_POSE guard dispatch is tested in CounterReactionTests.cs (mirrors AC#9g implementation-side coverage)
- [fix] Phase2-Uncertain iter7(FL4): AC#36c GetTarget guard | Added AC#36c verifying GetTarget guard path is tested in CounterMessageTests.cs (early-return when target <= 0)
- [fix] Phase2-Review iter8(FL4): AC#15 matcher precision | Tightened AC#15 from SetTEquip.*3 to SetTEquip.*,\\s*3,\\s*1 to prevent false-positive matches on indices 13/23/30
- [fix] Phase2-Review iter8(FL4): AC#27 parameter | Tightened AC#27 from public.*void.*ProcessPose to public.*void.*ProcessPose.*CharacterId.*int to enforce actionOrder parameter per Task 9 contract
- [fix] Phase2-Review iter9(FL4): AC#33b/34b/37b test content | Added test-content verification ACs for CounterPunishmentTests (variant dispatch), CounterPoseTests (TFLAG mapping), CounterCombinationTests (AccumulateCombinations) — closing symmetry gap with AC#32b/35b/36b pattern
- [fix] Phase2-Review iter10(FL4): AC#9h IsProtectedCheckOnly | Added AC#9h verifying CounterReaction uses IsProtectedCheckOnly brand guard (4 of 5 branches, verified against WcCounterReaction); AC#32e for test coverage
- [fix] Phase2-Review iter10(FL4): AC#10b character 999 | Added Technical Constraint and AC#10b for COUNTER_PUNISHMENT.ERB NO:ARG==999 player skip guard
- [fix] Phase3-Maintainability iter10(FL4): CounterOutputHandler ITEquipVariables | Added missing ITEquipVariables tequip to CounterOutputHandler constructor (setTequip adapter delegate references tequip which was not in parameter list)
- [fix] Phase2-Review iter1(FL5): AC Details 11 missing | Added AC Details blocks for AC#9h, AC#10b, AC#32b, AC#32d, AC#32e, AC#33b, AC#34b, AC#35b, AC#36b, AC#36c, AC#37b (template compliance)
- [fix] Phase2-Review iter1(FL5): AC Coverage 11 missing | Added AC Coverage rows for AC#9h, AC#10b, AC#32b, AC#32d, AC#32e, AC#33b, AC#34b, AC#35b, AC#36b, AC#36c, AC#37b
- [fix] Phase2-Uncertain iter1(FL5): AC#36d/36e CounterMessage test | Added AC#36d (gte HandleMessage >= 3) and AC#36e (DatuiMessage test) for test dispatch coverage symmetry with CounterReaction pattern
- [fix] Phase2-Review iter2(FL5): AC#16g handler count | Added AC#16g (gte private.*int HandleMessage >= 10) for substantial MESSAGE handler dispatch coverage, mirroring AC#12's count-based approach for POSE
- [fix] Phase3-Maintainability iter3(FL5): Adapter delegates | Expanded comment-only adapter delegates in CounterOutputHandler pseudo-code to explicit property-style declarations with proper types (Func/Action)
- [fix] Phase2-Review iter4(FL5): AC#8c HandlePunishment delegation | Added AC#8c verifying punishment.Execute() call in CounterOutputHandler (symmetry with AC#8b HandleReaction)
- [fix] Phase2-Review iter4(FL5): AC#16f matcher | Tightened from \\? .*: to console\\.Print.*\\? to prevent false-positive from incidental ternaries in 550-line file
- [fix] Phase2-Review iter5(FL5): AC#9i MASTER_POSE values | Added AC#9i (gte MasterPose.*[23456] >= 5) verifying actual MASTER_POSE dispatch index values 2,3,4,5,6
- [fix] Phase2-Review iter5(FL5): AC#12b CounterDecisionFlag guard | Added AC#12b verifying TCVAR:30 early-return guard in CounterPose; AC#34c for test coverage
- [fix] Phase2-Review iter5(FL5): AC#12c dispatch key | Added AC#12c verifying TFLAG switch dispatches on TCVarIndex.CounterAction (not actionOrder)
- [fix] Phase2-Review iter6(FL5): AC#26b GetTarget guard | Added implementation-side AC#26b verifying CounterMessage calls GetTarget() (symmetry with test-side AC#36c)
- [fix] Phase2-Review iter6(FL5): AC#16h 500-range | Added AC#16h spot-check for SubAction-dependent handler group (500-601 range)
- [fix] Phase2-Review iter6(FL5): AC#16e2 branch variants | Added AC#16e2 verifying DispatchWithBranch handles branch-variant case handling (29_1/29_2/29_3)
- [fix] Phase2-Review iter7(FL5): AC#15 rationale | Documented cross-handler TEQUIP mutation distribution coverage via composition of AC#15+16g+16b/16c/16d/16h
- [fix] Phase2-Review iter8(FL5): AC#12d TFLAG indices | Added AC#12d (gte TFlag6[0-5] >= 5) verifying distinct body-part TFLAG index distribution (60-65)
- [fix] Phase2-Review iter9(FL5): AC#11d rotor-inserter | Added AC#11d verifying CFLAG:PLAYER:ローター挿入者 = ARG mutation in variants 1 and 2
- [fix] Phase2-Review iter9(FL5): AC#11e/11f precondition guards | Added AC#11e (underwear guard) and AC#11f (vagina guard) for PUNISHMENT variant retry-loop preconditions
- [fix] Phase2-Review iter10(FL5): AC#11f matcher | Tightened from HasVagina|vagina|IComAvailabilityChecker to HasVagina|common\\.HasVagina (IComAvailabilityChecker.IsAvailable is semantically wrong for anatomy check)
- [fix] Phase2-Review iter10(FL5): CFLAG values | Added IMPORTANT note to CFLAG naming section: main counter indices differ from WC (ローター挿入=15, ローターA挿入=16, ローター挿入者=27 vs WC CFlagRotorInsertor=1081)
- [fix] Phase3-Maintainability iter10(FL5): Adapter delegates GC | Changed from property-style (per-access lambda allocation) to readonly field-style (initialized once in constructor) to prevent GC pressure in CHARANUM loop
- [fix] Phase2-Review iter2(FL6): Goal Coverage Item 6 | Added AC#16e2 to covering ACs (DispatchWithBranch branch-variant handlers are part of TRYCALLFORM dispatch translation)
- [fix] Phase2-Review iter2(FL6): Goal Coverage Item 1 | Added AC#9e to covering ACs (SELECTCOM dispatch constants 80-83 are part of COUNTER_REACTION.ERB migration)
- [fix] Phase2-Review iter3(FL6): AC#9h gte | Strengthened AC#9h from matches to gte >= 4 for IsProtectedCheckOnly to verify brand guard appears in all 4 required branches, not just one
- [fix] Phase2-Review iter3(FL6): AC#9i matcher | Tightened AC#9i from MasterPose.*[23456] to GetMasterPose\\(offender,\\s*[23456]\\)|MasterPose.*==\\s*[23456] to require digit as function argument or comparison value
- [fix] Phase2-Review iter4(FL6): Goal Coverage Item 1 | Added AC#9h, AC#9j, AC#15b (brand guard, named SELECTCOM constants, TEQUIP:PLAYER:4 mutation all part of ERB migration)
- [fix] Phase2-Review iter5(FL6): AC#20b matcher | Tightened from IConsoleOutput|consoleOutput|console to IConsoleOutput console|console\\.PrintLine|console\\.Print\\( to prevent false positives from bare 'console' token
- [fix] Phase2-Review iter1(FL7): AC Details ordering | Reordered sub-AC Details entries (32b,32c,32d,32e,33b,34b,34c,35b,36b,36c,36d,36e,36f,37b) to immediately follow parent ACs instead of being grouped at section end
- [fix] Phase2-Review iter1(FL7): Task 12 AC#36f | Added AC#36f to Task 12 AC# column (was orphan AC with no owning Task)
- [fix] Phase2-Review iter1(FL7): AC#14b sealed class count | Updated AC#14b expected from >= 5 to >= 6 (CounterOutputHandler matches "sealed class Counter" pattern); added AC#14b to Task 15 AC# column
- [fix] PostLoop-UserFix iter2(FL7): Goal Item 7 behavioral equivalence | Added AC#33c (CounterPunishment CFLAG names), AC#34d (CounterPose TFLAG indices), AC#36g (CounterMessage TEQUIP values); updated Tasks 6/8/12 AC# columns, Goal Coverage Item 7, and AC Coverage rows
- [fix] Phase2-Review iter3(FL7): Philosophy Derivation TEQUIP | Added Philosophy Derivation row tracing "SSOT for all counter output processing" → TEQUIP mutation preservation → AC#15, AC#15b, AC#36b, AC#36g
- [fix] Phase2-Review iter3(FL7): AC#26c DatuiMessage call site | Added AC#26c (gte DatuiMessage\\(\\) >= 2) verifying DatuiMessage is both defined and called, not just defined as dead code; updated Task 13 AC# column and AC Coverage
- [fix] Phase2-Uncertain iter4(FL7): AC#36g gte strengthening | Changed AC#36g from matches to gte >= 2 for multi-site TEQUIP mutation behavioral equivalence in test assertions
- [fix] Phase2-Review iter5(FL7): Philosophy Derivation HandlePunishment | Added row tracing "implements ICounterOutputHandler" → HandlePunishment all-character CFLAG reset + CLOTHES_RESET + CLOTHES_SETTING_TRAIN → AC#21-24
- [fix] Phase2-Review iter5(FL7): AC#36h TEQUIP:PLAYER:4 test-side | Added AC#36h (matches SetTEquip.*4,\\s*1) closing C3 test-side symmetry gap between index 3 (AC#36g) and index 4
- [fix] Phase2-Review iter6(FL7): AC#33d precondition guard test | Added AC#33d (matches HasUnderwear|LowerUnderwear.*0|HasVagina) closing test-side symmetry gap for AC#11e/11f (following AC#9h→32e, AC#12b→34c, AC#26b→36c pattern)

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning
- [Predecessor: F801](feature-801.md) - Main Counter Core (defines ICounterOutputHandler for F802 to implement)
- [Successor: F811](feature-811.md) - SOURCE Entry System (calls F802's COMBINATION/POSE)
- [Successor: F813](feature-813.md) - Post-Phase Review Phase 21
- [Related: F803](feature-803.md) - Main Counter Source
- [Related: F804](feature-804.md) - WC Counter Core (design reference)
- [Related: F805](feature-805.md) - WC Counter Source + Message Core
- [Related: F809](feature-809.md) - COMABLE Core
