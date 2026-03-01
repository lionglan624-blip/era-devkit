# Feature 803: Main Counter Source

## Status: [DONE]
<!-- fl-reviewed: 2026-02-25T23:57:59Z -->

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

## Background

### Philosophy (Mid-term Vision)

Phase 21 Counter System C# migration establishes Era.Core as the single source of truth for all counter subsystem logic except CSTR runtime storage (tracked in F813), shrinkage runtime storage (F813), CFLAG indices (F813), and EQUIP indices (F813). Each ERB file migrates to a dedicated C# class with typed indices and interface-driven dependency injection, enabling testable, maintainable game logic independent of the legacy ERB interpreter.

### Problem (Current Issue)

C# migration of Main Counter source processing: event source dispatch and parameter resolution for main counter events.

| Source File | Approx Lines | Description |
|-------------|:------------:|-------------|
| COUNTER_SOURCE.ERB | 920 | Event source processing for main counter |

F803 is a skeletal DRAFT with only 2 generic tasks and 1 AC, because F783 (Phase 21 Planning) created all sub-feature DRAFTs from a minimal template deferring detailed analysis to /fc. The actual COUNTER_SOURCE.ERB (920 lines) contains ~36 SELECTCASE branches dispatching counter actions, a second SELECTCASE block for SETBIT master counter control (~30 branches), 4 DATUI undressing helper functions, and PAIN_CHECK_V_MASTER -- none of which are represented in the current DRAFT. The C# type system is incomplete for this migration: SourceIndex.cs lacks 12 well-known constants (e.g., Seduction=50, Humiliation=51, Provocation=52, Service=53), TCVarIndex.cs lacks several counter-related constants, StainIndex.cs has no well-known constants, ExpIndex.cs is missing ~6 indices, IVariableStore has no CSTR (character string) accessors (needed at 19 call sites), and ~8 external functions (TOUCH_SET, LOST_VIRGIN_*, CLOTHES_SETTING_TRAIN, PAIN_CHECK_V, etc.) need interface abstractions.

### Goal (What to Achieve)

Migrate COUNTER_SOURCE.ERB to a C# CounterSourceHandler class in Era.Core/Counter/ with: (1) all ~36 counter action dispatch branches faithfully translated, (2) SETBIT master counter control block translated to C# bitwise operations, (3) DATUI_* undressing helpers as shared public methods, (4) PAIN_CHECK_V_MASTER with correct TIMES float-to-int truncation, (5) SourceIndex constants verified present (12 pre-existing from F812, regression guard); StainIndex/ExpIndex/TCVarIndex constants added, (6) CSTR string accumulation interface strategy resolved (runtime storage deferred to F813), (7) all external calls behind injectable interfaces, and (8) equivalence tests verifying behavioral parity via category-representative tests (6 dispatch ranges covering all ~36 branches) with the original ERB.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is F803 insufficient for implementation? | It has only 2 generic tasks and 1 AC -- no architecture, no branch analysis, no interface gap identification | `pm/features/feature-803.md:48-61` |
| 2 | Why does it lack detailed analysis? | F783 created all Phase 21 DRAFTs from a minimal template, deferring AC/Task generation to /fc | `pm/features/feature-783.md:411,439-449` |
| 3 | Why was the template minimal? | F783 focused on decomposition correctness (file grouping, subsystem boundaries), not implementation readiness | `pm/features/feature-783.md:384-386` |
| 4 | Why is implementation readiness critical for F803? | COUNTER_SOURCE.ERB is 920 lines with ~36 CASE branches, ~15 external function calls, 19 CSTR usages, and SETBIT/TIMES ERA built-ins requiring careful C# translation | `Game/ERB/COUNTER_SOURCE.ERB:1-920` |
| 5 | Why (Root)? | The C# type system and interfaces are incomplete for F803's needs: SourceIndex lacks 12 constants, IVariableStore has no CSTR methods, and no interface exists for TOUCH_SET or other external functions -- the migration cannot proceed without these additions | `Era.Core/Types/SourceIndex.cs:30-49`, `Era.Core/Interfaces/IVariableStore.cs` |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | F803 has 2 placeholder tasks and 1 AC | C# type system (SourceIndex, TCVarIndex, StainIndex, ExpIndex) missing 13+ constants; IVariableStore missing CSTR; no interfaces for 8 external function dependencies |
| Where | `pm/features/feature-803.md` tasks/AC sections | `Era.Core/Types/SourceIndex.cs`, `Era.Core/Interfaces/IVariableStore.cs`, absent ICounterSourceHandler |
| Fix | Add more generic tasks | Analyze all 920 lines; add missing typed constants; define CSTR strategy; create external call interfaces; generate branch-level equivalence ACs |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F783 | [DONE] | Predecessor -- Phase 21 Planning (parent decomposition) |
| F801 | [DONE] | Predecessor -- writes TCVAR:CounterAction consumed by F803; provides CounterActionId enum |
| F802 | [WIP] | Related -- sibling Phase 21 feature, no direct call dependency |
| F805 | [DRAFT] | Successor -- TOILET_COUNTER_SOURCE.ERB calls DATUI_*/PAIN_CHECK_V_MASTER helpers defined in F803 scope |
| F811 | [PROPOSED] | Related -- bidirectional call dependency (F803 calls TOUCH_SET from SOURCE_POSE.ERB; F811's SOURCE.ERB calls EVENT_COUNTER_SOURCE) |
| F812 | [DONE] | Related -- sibling Phase 21 feature, no direct call dependency |
| F813 | [DRAFT] | Successor -- Post-Phase Review Phase 21 |
| F815 | [DONE] | Related -- provides StubVariableStore test infrastructure |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Source file structured and accessible | FEASIBLE | 920 lines, well-structured SELECTCASE blocks with repetitive branch pattern |
| CounterActionId enum available | FEASIBLE | F801 created complete enum in `Era.Core/Counter/CounterActionId.cs` |
| IVariableStore covers SOURCE/STAIN/EXP/EQUIP | FEASIBLE | GetSource/SetSource, GetStain/SetStain, GetExp/SetExp, GetEquip/SetEquip all present |
| Missing SourceIndex/TCVarIndex/StainIndex/ExpIndex constants | FEASIBLE | Additive changes only; no breaking modifications |
| External functions abstractable via DI | FEASIBLE | Standard interface + DI pattern; IClothingSystem, ICommonFunctions, IVirginityManager already exist |
| CSTR gap (19 write sites, no setter) | FEASIBLE | Resolution: ICharacterStringVariables interface with SetCharacterString (AC#12); runtime VariableStore implementation deferred to F813 |
| TOUCH_SET cross-feature dependency with F811 | FEASIBLE | Abstractable behind ITouchSet interface; no scope merge required |
| F805 helper sharing (DATUI_*/PAIN_CHECK_V_MASTER) | FEASIBLE | Public methods in shared class; F805 declares F803 as Predecessor |
| Code volume (920 lines) | FEASIBLE | Repetitive structure amenable to data-driven approach |
| TreatWarningsAsErrors | FEASIBLE | Must compile clean; standard constraint |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core/Types/ | MEDIUM | 4 type files modified (SourceIndex, TCVarIndex, StainIndex, ExpIndex) -- additive constant additions |
| Era.Core/Counter/ | HIGH | New CounterSourceHandler class + interface; new MasterCounterControl logic; new UndressingHandler; new PainCheckVMaster |
| Era.Core/Interfaces/ | MEDIUM | Potential IVariableStore extension for CSTR, or new ICStrVariables interface |
| F805 (TOILET_COUNTER_SOURCE) | MEDIUM | F805 depends on DATUI_*/PAIN_CHECK_V_MASTER helpers extracted by F803 |
| F811 (SOURCE/SOURCE_POSE) | LOW | F811 calls EVENT_COUNTER_SOURCE via ICounterSourceHandler; no code change to F811 required |
| Equivalence tests | HIGH | Branch-level tests required for ~36 dispatch branches + SETBIT block + DATUI helpers |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| IVariableStore has a CSTR getter (GetCharacterString) but no setter. 19 CSTR write sites in COUNTER_SOURCE.ERB need `SetCharacterString` via ICharacterStringVariables interface. | `Era.Core/Interfaces/IVariableStore.cs` | 19 CSTR write sites need SetCharacterString via ICharacterStringVariables interface |
| 13 missing SourceIndex well-known constants | `Era.Core/Types/SourceIndex.cs:30-49` vs `CSV/source.csv` | Must add constants for seduction, humiliation, provocation, service, coercion, sadism, etc. |
| 8+ missing TCVarIndex constants | `Era.Core/Types/TCVarIndex.cs:30-45` | Additional counter-related TCVAR indices needed |
| StainIndex has no well-known constants | `Era.Core/Types/StainIndex.cs:29-30` | Must add stain constants used by COUNTER_SOURCE.ERB |
| ExpIndex missing ~6 indices | `Era.Core/Types/ExpIndex.cs` | Additional experience indices needed |
| SETBIT is ERA built-in | ERA engine | Must translate to C# bitwise OR operations |
| TIMES instruction uses float multiplication | `COUNTER_SOURCE.ERB:896-919` | Float-to-int truncation must match ERA semantics |
| TOUCH_SET defined in F811 scope (SOURCE_POSE.ERB) | `Game/ERB/SOURCE_POSE.ERB:289` | ~26 call sites in F803 need interface abstraction |
| Reaction cases (500-601) use literal ints, not CounterActionId | `COUNTER_SOURCE.ERB:539-631` | Separate concept from CounterActionId enum values |
| PAIN_CHECK_V_MASTER distinct from IPainStateChecker | `COUNTER_SOURCE.ERB:893-920` | Modifies SOURCE:Sadism, not Pain/Antipathy like PAIN_CHECK_V |
| Global-scope SOURCE writes (no character prefix) | `COUNTER_SOURCE.ERB:300,524` | Must resolve to default TARGET character |
| RAND:N expressions | `COUNTER_SOURCE.ERB:136,154,...` | Translate to random.Next(N) |
| EQUIP indices not typed | `COUNTER_SOURCE.ERB:321,365,...` | Need CSV mapping for named EQUIP indices |
| TreatWarningsAsErrors=true | `Directory.Build.props` | All new code must compile without warnings |
| EXP_UP function (3 call sites in reaction cases) | `COUNTER_SOURCE.ERB:562,574,590` | Conditional exp gain: `SIF !EXP_UP(index, ARG)` gates EXP increment. Add to ICounterUtilities or new interface |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| CSTR gap blocks compilation (19 call sites) | HIGH | HIGH | Design new ICStrVariables interface or extend IVariableStore; resolve strategy early |
| F811 circular dependency via TOUCH_SET | MEDIUM | MEDIUM | Abstract TOUCH_SET behind ITouchSet interface; F803 depends on interface, not F811 implementation |
| F805 breakage when helpers migrated to C# | MEDIUM | HIGH | Extract DATUI_*/PAIN_CHECK_V_MASTER as public shared methods; declare F805 as Successor |
| Transcription errors in ~36 dispatch branches | MEDIUM | MEDIUM | Data-driven approach where possible; branch-level equivalence tests |
| SourceIndex constant additions conflict with other features | LOW | LOW | Additive changes only; no overlapping values |
| TIMES float-to-int rounding mismatch | MEDIUM | MEDIUM | Test ERA TIMES behavior explicitly; replicate truncation semantics |
| Reaction cases (500-601) confused with CounterActionId | MEDIUM | LOW | Document as separate concept; consider separate enum or constants class |
| Global SOURCE writes resolve to wrong character | MEDIUM | HIGH | Explicit TARGET resolution with tests; verify ERA default character behavior |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| SourceIndex constant count | `grep -c "public static readonly SourceIndex" Era.Core/Types/SourceIndex.cs` | 31 | 12 constants (Liquid through Sadism) already added by prior features. AC#7 (count_equals 31) may need revision. |
| TCVarIndex constant count | `grep -c "public static readonly TCVarIndex" Era.Core/Types/TCVarIndex.cs` | 16 | F801/F812 added 8 constants (CounterAction, CounterDecisionFlag, SubAction, UndressingPlayerLower, UndressingPlayerUpper, UndressingTargetLower, UndressingTargetUpper, SixtyNineTransition). AC#43 (count_equals 20) = 16 baseline + 4 truly new. |
| IVariableStore method count | `grep -c "Result<int>\|void Set\|int Get" Era.Core/Interfaces/IVariableStore.cs` | ~34 | Current accessor methods |
| COUNTER_SOURCE.ERB line count | `wc -l Game/ERB/COUNTER_SOURCE.ERB` | 920 | Source file scope |

**Baseline File**: `.tmp/baseline-803.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | All ~36 dispatch branches must produce identical SOURCE/STAIN/EXP mutations | `COUNTER_SOURCE.ERB:27-632` | Per-branch or grouped equivalence tests required |
| C2 | SETBIT master counter control must produce identical bitmasks | `COUNTER_SOURCE.ERB:637-786` | Bitwise operation verification per CASE |
| C3 | 4 DATUI_* functions must zero EQUIP identically across undressing levels | `COUNTER_SOURCE.ERB:789-890` | Equipment state verification per function |
| C4 | PAIN_CHECK_V_MASTER TIMES multiplication must match ERA truncation | `COUNTER_SOURCE.ERB:893-920` | Threshold boundary tests with float multipliers |
| C5 | CSTR:x:10 string accumulation must be representable in C# | 19 occurrences across branches | CSTR strategy must be decided; AC for interface availability |
| C6 | Pre-check guard (TCVAR:ARG:カウンター行動決定フラグ > 1 return and CFLAG:ARG:うふふ==2 pre-SELECTCASE block) must be preserved | `COUNTER_SOURCE.ERB:2-3,8-26` | Early return and special path tests |
| C7 | External calls must be behind injectable interfaces | All CALL/CALLF to TOUCH_SET, LOST_VIRGIN_*, CLOTHES_SETTING_TRAIN, PAIN_CHECK_V, ONCE, HAS_PENIS/HAS_VAGINA, etc. | DI verification for each external dependency |
| C8 | F801 must be declared as Predecessor | Data flow: TCVAR:CounterAction written by F801 | Dependency table update |
| C9 | STAIN bitwise OR must match ERA semantics | `COUNTER_SOURCE.ERB:22-23,...` | STAIN merge verification |
| C10 | Reaction cases (500-601) use literal ints, not CounterActionId | `COUNTER_SOURCE.ERB:539-631` | Separate handling from CounterActionId dispatch |
| C11 | SourceIndex 12 constants must remain present (regression guard; all 12 already added by F812) | `SourceIndex.cs` vs `CSV/source.csv` | Named constant regression verification |
| C12 | DATUI_*/PAIN_CHECK_V_MASTER must be public for F805 reuse | `TOILET_COUNTER_SOURCE.ERB:20,570,627` calls these helpers | Public interface or shared class |
| C13 | IVariableStore has a CSTR getter (GetCharacterString) but no setter. 19 CSTR write sites in COUNTER_SOURCE.ERB need `SetCharacterString` via ICharacterStringVariables interface. | `IVariableStore.cs` has GetCharacterString but no SetCharacterString | 19 CSTR write sites need SetCharacterString via ICharacterStringVariables |
| C14 | TOUCH_SET interface gap (~26 call sites from F803) | `SOURCE_POSE.ERB:289` (F811 scope) | Interface Dependency Scan finding |
| C15 | EXPLV accessor gap | `COUNTER_SOURCE.ERB:895-919` | Interface Dependency Scan finding; workaround via hardcoded thresholds or PalamLv |

### Constraint Details

**C1: Branch Dispatch Equivalence**
- **Source**: Investigation of `COUNTER_SOURCE.ERB:27-632` -- ~36 CASE branches dispatching on SOURCE value
- **Verification**: Compare C# output per branch against ERB execution for identical inputs
- **AC Impact**: Equivalence tests must cover representative branches from each category (seduction, humiliation, service, coercion, pleasure, reaction)

**C2: Master Counter Control Bitmask**
- **Source**: `COUNTER_SOURCE.ERB:637-786` -- second SELECTCASE block performing SETBIT operations
- **Verification**: Verify C# bitwise OR produces identical bitmask values for each CASE
- **AC Impact**: Bitfield verification tests needed

**C3: DATUI Undressing Helpers**
- **Source**: `COUNTER_SOURCE.ERB:789-890` -- 4 functions (DATUI_BOTTOM, DATUI_BOTTOM_T, DATUI_TOP, DATUI_TOP_T)
- **Verification**: Verify EQUIP values zeroed correctly per undressing level; verify CLOTHES_SETTING_TRAIN called
- **AC Impact**: Equipment state verification per function; public visibility for F805 reuse

**C4: PAIN_CHECK_V_MASTER Truncation**
- **Source**: `COUNTER_SOURCE.ERB:893-920` -- uses TIMES instruction with float multipliers on EXPLV thresholds
- **Verification**: Verify float-to-int truncation matches ERA TIMES behavior at boundary values
- **AC Impact**: Threshold boundary tests with explicit truncation verification

**C5: CSTR String Accumulation**
- **Source**: 19 occurrences of `CSTR:x:10 += LOCALS` across branches -- ejaculation partner tracking
- **Verification**: Verify chosen CSTR strategy correctly appends character strings
- **AC Impact**: Interface availability AC + string append verification

**C6: Pre-check Guard Preservation**
- **Source**: `COUNTER_SOURCE.ERB:2-3,8-26` -- TCVAR:ARG:カウンター行動決定フラグ > 1 early return and CFLAG:ARG:うふふ==2 pre-SELECTCASE block
- **Verification**: Test early return and special path produce identical state changes
- **AC Impact**: AC#40 (guard existence), AC#41 (test for early return + うふふ==2 special path)

**C7: External Call Interfaces**
- **Source**: ~8 distinct external functions called from COUNTER_SOURCE.ERB
- **Verification**: Each external call has a corresponding interface method injectable via DI
- **AC Impact**: Interface existence + injection verification per external dependency

**C8: F801 Predecessor Declaration**
- **Source**: F801 writes TCVAR:CounterAction consumed at COUNTER_SOURCE.ERB:27
- **Verification**: Dependencies table includes F801 as Predecessor
- **AC Impact**: Dependency table update (structural)

**C9: STAIN Bitwise OR Semantics**
- **Source**: COUNTER_SOURCE.ERB:22-23 and similar -- `STAIN:TARGET:x |= value`
- **Verification**: Verify C# bitwise OR produces identical stain merge values
- **AC Impact**: AC#4 (StainIndex constants), AC#45 (STAIN merge behavior test)

**C10: Reaction Cases Separate from CounterActionId**
- **Source**: COUNTER_SOURCE.ERB:539-631 -- literal ints 500-601 not in CounterActionId enum
- **Verification**: Reaction cases handled via separate dispatch (not enum switch)
- **AC Impact**: AC#26 (reaction case numbers), AC#33 (HandleReactionCases method)

**C11: SourceIndex Regression Guard**
- **Source**: SourceIndex.cs vs CSV/source.csv -- all 12 constants already added by F812 (baseline 31)
- **Verification**: Count public static readonly SourceIndex fields (must remain ≥ 31)
- **AC Impact**: AC#3 (Seduction sample), AC#7 (total count 31, regression guard)

**C12: Shared Helper Public Visibility**
- **Source**: `TOILET_COUNTER_SOURCE.ERB:20,570,627,...` (F805 scope) calls DATUI_*, PAIN_CHECK_V_MASTER
- **Verification**: Grep for public visibility modifier on helper methods
- **AC Impact**: Methods must be public or exposed via interface for F805 consumption

**C13: CSTR Interface Gap**
- **Source**: IVariableStore.cs has a CSTR getter (GetCharacterString) but no setter; 19 CSTR write sites in COUNTER_SOURCE.ERB need SetCharacterString via ICharacterStringVariables interface
- **Verification**: New ICharacterStringVariables interface provides SetCharacterString (GetCharacterString may already exist on IVariableStore)
- **AC Impact**: AC#12 (interface existence), AC#29 (injection), AC#42 (behavioral test)

**C14: TOUCH_SET Interface Gap**
- **Source**: SOURCE_POSE.ERB:289 (F811 scope); ~26 call sites from F803
- **Verification**: ITouchSet interface provides TouchSet method injectable via DI
- **AC Impact**: AC#18 (ITouchSet file existence)

**C15: EXPLV Accessor Gap**
- **Source**: COUNTER_SOURCE.ERB:895-919 -- EXPLV thresholds for PainCheckVMaster
- **Verification**: ExpLvTable.cs addition (Task#17) eliminates duplication; both PainStateChecker and CounterSourceHandler reference shared static class (no EXPLV accessor needed)
- **AC Impact**: AC#11 (PainCheckVMaster existence), AC#22 (boundary test)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| Predecessor | F801 | [DONE] | Writes TCVAR:CounterAction consumed at `COUNTER_SOURCE.ERB:27`; provides CounterActionId enum |
| Successor | F805 | [DRAFT] | TOILET_COUNTER_SOURCE.ERB calls DATUI_*/PAIN_CHECK_V_MASTER helpers at `TOILET_COUNTER_SOURCE.ERB:20,570,627` |
| Successor | F813 | [DRAFT] | Post-Phase Review Phase 21 |
| Related | F802 | [WIP] | Sibling Phase 21 feature, no direct call dependency |
| Related | F811 | [PROPOSED] | Bidirectional call: F803 calls TOUCH_SET from `SOURCE_POSE.ERB:289` (~26 sites); F811 calls EVENT_COUNTER_SOURCE from `SOURCE.ERB:38` |
| Related | F812 | [DONE] | Sibling Phase 21 feature, no direct call dependency |
| Related | F815 | [DONE] | Provides StubVariableStore test infrastructure |

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

## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Era.Core as the single source of truth for all counter subsystem logic except CSTR runtime storage (F813), shrinkage runtime storage (F813)" | CounterSourceHandler class must exist in Era.Core/Counter/ | AC#1 |
| "Each ERB file migrates to a dedicated C# class" | ICounterSourceHandler interface + CounterSourceHandler implementation | AC#1, AC#2 |
| "typed indices and interface-driven dependency injection" | Missing SourceIndex/StainIndex/ExpIndex constants added; external calls behind interfaces. Note: CFLAG/EQUIP use raw-int constants per Key Decisions (CFLAG/EQUIP index typing rows); full typed-index migration tracked in Mandatory Handoffs → F813. | AC#3, AC#4, AC#5, AC#7, AC#20, AC#23, AC#43, AC#48 |
| "testable, maintainable game logic independent of the legacy ERB interpreter" | Unit tests pass; build succeeds with TreatWarningsAsErrors | AC#14, AC#15, AC#22, AC#24, AC#25, AC#41, AC#42, AC#45, AC#46, AC#50, AC#53, AC#54, AC#57, AC#58, AC#59, AC#60, AC#61, AC#62, AC#65, AC#66, AC#70, AC#71, AC#81, AC#82, AC#83, AC#84, AC#85, AC#87 |
| "Each ERB file migrates to a dedicated C# class" | Dispatch method handles all ~36 CounterActionId cases + reaction cases | AC#6, AC#20, AC#26, AC#33, AC#40, AC#49, AC#52, AC#74, AC#80 |
| "Each ERB file migrates to a dedicated C# class" | Master counter control method uses bitwise OR (SETBIT translation) | AC#8, AC#32, AC#44, AC#51, AC#72, AC#77, AC#78 |
| "Each ERB file migrates to a dedicated C# class" | 4 DATUI methods are public for F805 reuse | AC#9, AC#10, AC#16, AC#17, AC#47, AC#56, AC#64 |
| "Each ERB file migrates to a dedicated C# class" | Pain check method with TIMES-equivalent float-to-int truncation | AC#11, AC#22, AC#63, AC#67, AC#68, AC#69, AC#73 |
| "Era.Core as the single source of truth for all counter subsystem logic except CSTR runtime storage (F813), shrinkage runtime storage (F813)" | ICharacterStringVariables interface; runtime VariableStore implementation deferred to F813 per Mandatory Handoffs | AC#12, AC#29, AC#42, AC#75, AC#79 |
| "interface-driven dependency injection" | No direct external function calls; all via DI | AC#13, AC#18, AC#19, AC#21, AC#27, AC#28, AC#29, AC#30, AC#31, AC#34, AC#35, AC#36, AC#37, AC#38, AC#39, AC#55, AC#76, AC#86 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ICounterSourceHandler interface exists in Era.Core/Counter/ | file | Glob(Era.Core/Counter/ICounterSourceHandler.cs) | exists | - | [x] |
| 2 | CounterSourceHandler class implements ICounterSourceHandler | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `class CounterSourceHandler.*ICounterSourceHandler` | [x] |
| 3 | SourceIndex has Seduction constant (50) | code | Grep(Era.Core/Types/SourceIndex.cs) | matches | `Seduction = new\(50\)` | [x] |
| 4 | StainIndex has well-known constants (8 stain types) | code | Grep(Era.Core/Types/StainIndex.cs) | count_equals | `public static readonly StainIndex` = 8 | [x] |
| 5 | ExpIndex has VSexExp constant (20) | code | Grep(Era.Core/Types/ExpIndex.cs) | matches | `VSexExp = new\(20\)` | [x] |
| 6 | Dispatch method handles CounterActionId.SeductiveGesture case | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `SeductiveGesture` | [x] |
| 7 | SourceIndex has all 12 missing constants (total 31) | code | Grep(Era.Core/Types/SourceIndex.cs) | count_equals | `public static readonly SourceIndex` = 31 | [x] |
| 8 | UpdateMasterCounterControl method exists | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `UpdateMasterCounterControl` | [x] |
| 9 | DATUI_BOTTOM method is public | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `public.*DatUIBottom\(` | [x] |
| 10 | DATUI_TOP method is public | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `public.*DatUITop\(` | [x] |
| 11 | PainCheckVMaster method exists | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `public.*PainCheckVMaster\(` | [x] |
| 12 | ICharacterStringVariables interface has CSTR string accessor | code | Grep(Era.Core/Interfaces/ICharacterStringVariables.cs) | matches | `SetCharacterString` | [x] |
| 13 | CounterSourceHandler constructor injects IVariableStore | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `IVariableStore` | [x] |
| 14 | Era.Core builds without warnings | build | dotnet build Era.Core/ | succeeds | - | [x] |
| 15 | All unit tests pass | test | dotnet test Era.Core.Tests/ | succeeds | - | [x] |
| 16 | DATUI_BOTTOM_T method is public | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `public.*DatUIBottomT\(` | [x] |
| 17 | DATUI_TOP_T method is public | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `public.*DatUITopT\(` | [x] |
| 18 | ITouchSet interface file exists | file | Glob(Era.Core/Counter/ITouchSet.cs) | exists | - | [x] |
| 19 | IShrinkageSystem interface file exists | file | Glob(Era.Core/Counter/IShrinkageSystem.cs) | exists | - | [x] |
| 20 | HandleCounterSource dispatch is exhaustive (default throws on unknown) | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `_ =>.*throw\|default.*throw\|ArgumentOutOfRangeException` | [x] |
| 21 | CounterSourceHandler constructor injects IVirginityManager | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `IVirginityManager` | [x] |
| 22 | PainCheckVMaster boundary truncation test exists | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `PainCheckVMaster` = 3 | [x] |
| 23 | TCVarIndex has MasterCounterControl constant | code | Grep(Era.Core/Types/TCVarIndex.cs) | matches | `MasterCounterControl = new\(` | [x] |
| 24 | Dispatch branch equivalence tests exist for all 6 CounterActionId dispatch ranges | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `CounterActionId\.SeductiveGesture\|CounterActionId\.MilkHandjob\|CounterActionId\.Kiss\|CounterActionId\.Masturbation\|CounterActionId\.ForcedCunnilingus\|CounterActionId\.MissionaryInsert` = 6 | [x] |
| 25 | Master counter control bitfield tests cover both MASTER and ARG characters | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `MasterCounter\|UpdateMasterCounterControl` = 3 | [x] |
| 26 | All 7 reaction case numbers (500, 502-505, 600-601) are handled in dispatch | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | gte | `case 500\|case 502\|case 503\|case 504\|case 505\|case 600\|case 601\|500 =>\|502 =>\|503 =>\|504 =>\|505 =>\|600 =>\|601 =>` = 7 | [x] |
| 27 | CounterSourceHandler constructor injects IRandomProvider | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `IRandomProvider` | [x] |
| 28 | ICounterUtilities has CheckExpUp method | code | Grep(Era.Core/Counter/ICounterUtilities.cs) | matches | `CheckExpUp` | [x] |
| 29 | CounterSourceHandler constructor injects ICharacterStringVariables | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `ICharacterStringVariables` | [x] |
| 30 | CounterSourceHandler constructor injects IShrinkageSystem | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `IShrinkageSystem` | [x] |
| 31 | CounterSourceHandler calls CheckExpUp for EXP_UP translation | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `CheckExpUp` | [x] |
| 32 | UpdateMasterCounterControl uses bitwise OR assignment | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `\|=` | [x] |
| 33 | Reaction cases dispatched via dedicated HandleReactionCases method | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `HandleReactionCases` | [x] |
| 34 | CounterSourceHandler constructor injects IClothingSystem | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `IClothingSystem` | [x] |
| 35 | CounterSourceHandler constructor injects IEngineVariables | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `IEngineVariables` | [x] |
| 36 | CounterSourceHandler constructor injects ICommonFunctions | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `ICommonFunctions` | [x] |
| 37 | CounterSourceHandler constructor injects IPainStateChecker | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `IPainStateChecker` | [x] |
| 38 | CounterSourceHandler constructor injects ITEquipVariables | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `ITEquipVariables` | [x] |
| 39 | CounterSourceHandler constructor injects ICounterUtilities | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `ICounterUtilities` | [x] |
| 40 | Pre-check guard: early return when CounterDecisionFlag > 1 | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `CounterDecisionFlag` | [x] |
| 41 | Pre-check guard test: early return + うふふ==2 special path tests exist | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `EarlyReturn\|PreCheck\|Ufufu\|SpecialPath` = 4 | [x] |
| 42 | CSTR string append behavior test exists | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `SetCharacterString\|CSTR.*Equal` = 6 | [x] |
| 43 | TCVarIndex has all 4 truly new constants (total 20) | code | Grep(Era.Core/Types/TCVarIndex.cs) | count_equals | `public static readonly TCVarIndex` = 20 | [x] |
| 44 | UpdateMasterCounterControl has sufficient SETBIT-to-bitshift translations | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | gte | `1 <<` = 57 | [x] |
| 45 | STAIN bitwise OR merge behavior test exists | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `Stain\|StainIndex` = 3 | [x] |
| 46 | Dispatch branch equivalence test asserts concrete output state | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `Assert\.Equal\|GetSource\|GetStain` = 3 | [x] |
| 47 | ICounterSourceHandler interface declares HandleCounterSource, DATUI and PainCheckVMaster methods | code | Grep(Era.Core/Counter/ICounterSourceHandler.cs) | gte | `void HandleCounterSource\|void DatUIBottom\|void DatUIBottomT\|void DatUITop\|void DatUITopT\|void PainCheckVMaster` = 6 | [x] |
| 48 | ExpIndex has all new constants (total >= 14) | code | Grep(Era.Core/Types/ExpIndex.cs) | gte | `public static readonly ExpIndex` = 14 | [x] |
| 49 | HandleCounterSource has sufficient dispatch cases (branch completeness) | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | gte | `CounterActionId\.[A-Z]` = 36 | [x] |
| 50 | Equivalence tests call SetSource/GetSource sufficient times for behavioral parity (dispatch + うふふ==2) | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `SetSource\|GetSource` = 20 | [x] |
| 51 | MCC bitfield test asserts concrete bitmask output state | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `MasterCounterControl\|MasterCounter` = 3 | [x] |
| 52 | Reaction cases 500, 502-505 implement SubAction nested sub-dispatch (600-601 excluded per Technical Design) | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | gte | `SubAction` = 10 | [x] |
| 53 | Reaction case sub-dispatch tests reference SubAction | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `SubAction` = 10 | [x] |
| 54 | Equivalence tests invoke handler methods (not just reference names) | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `HandleCounterSource\|\.DatUIBottom\|\.DatUITop\|\.PainCheckVMaster\|\.UpdateMasterCounterControl` = 5 | [x] |
| 55 | CounterSourceHandler constructor injects ITouchSet | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `ITouchSet` | [x] |
| 56 | DATUI helper tests verify EQUIP zeroing behavior | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `DatUIBottom\|DatUIBottomT\|DatUITop\|DatUITopT` = 4 | [x] |
| 57 | TestCounterUtilities stub has CheckExpUp method | code | Grep(Era.Core.Tests/Counter/ActionValidatorTests.cs) | matches | `CheckExpUp` | [x] |
| 58 | SelectorTestCounterUtils stub has CheckExpUp method | code | Grep(Era.Core.Tests/Counter/ActionSelectorTests.cs) | matches | `CheckExpUp` | [x] |
| 59 | Dispatch branch tests contain Assert.Equal assertions for output verification | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `Assert\.Equal` = 2 | [x] |
| 60 | CounterSourceHandler.cs has no TODO/FIXME/HACK markers | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | not_contains | `TODO\|FIXME\|HACK` | [x] |
| 61 | ICounterSourceHandler.cs has no TODO/FIXME/HACK markers | code | Grep(Era.Core/Counter/ICounterSourceHandler.cs) | not_contains | `TODO\|FIXME\|HACK` | [x] |
| 62 | Reaction cases 600-601 have behavioral test coverage | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `600\|601\|玉揉み\|TesticularMassage\|AnalStimulation` = 2 | [x] |
| 63 | PainCheckVMaster uses int-cast truncation pattern (ERA TIMES fidelity) | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `\(int\)\(.*\*` | [x] |
| 64 | DATUI helpers zero EQUIP indices via SetEquip calls | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | gte | `SetEquip` = 4 | [x] |
| 65 | Case 505 sub-case 2 ungated EXP increment test exists | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | matches | `505.*Masturbation\|Masturbation.*505\|手コキ\|手淫\|SubCase2.*Ungated\|UngatedExp` | [x] |
| 66 | Dispatch branch tests read SOURCE/STAIN state for behavioral parity verification | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `GetSource\|GetStain` = 2 | [x] |
| 67 | ExpLvTable.cs contains PalamLv threshold array | code | Grep(Era.Core/Counter/ExpLvTable.cs) | matches | `PalamLv` | [x] |
| 68 | PainStateChecker references ExpLvTable.PalamLv | code | Grep(Era.Core/Character/PainStateChecker.cs) | gte | `ExpLvTable\.PalamLv` = 2 | [x] |
| 69 | CounterSourceHandler references ExpLvTable for EXPLV/PALAMLV | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | gte | `ExpLvTable` = 2 | [x] |
| 70 | Equivalence tests verify EXP state mutations | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `GetExp\|SetExp` = 3 | [x] |
| 71 | All ICounterUtilities stubs include CheckExpUp | code | Grep(Era.Core.Tests/Counter/) | gte | `CheckExpUp` = 7 | [x] |
| 72 | HandleCounterSource invokes UpdateMasterCounterControl | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `UpdateMasterCounterControl\(` | [x] |
| 73 | PainStateChecker inline PALAMLV removed after extraction | code | Grep(Era.Core/Character/PainStateChecker.cs) | not_contains | `private static readonly int\[\] PALAMLV` | [x] |
| 74 | Reaction case int-range guard exists before CounterActionId cast | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `>= 500\|500.*601\|IsReactionCase\|isReaction` | [x] |
| 75 | SetCharacterString called in production CounterSourceHandler | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | gte | `SetCharacterString` = 19 | [x] |
| 76 | TouchSet called in production CounterSourceHandler | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | gte | `\.TouchSet\(` = 25 | [x] |
| 77 | MCC test invocations cover multiple CASE groups | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `UpdateMasterCounterControl\|MasterCounter.*Assert\|Assert.*MasterCounter` = 5 | [x] |
| 78 | UpdateMasterCounterControl writes TCVAR for both MASTER and ARG | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | gte | `SetTCVar.*MasterCounterControl\|MasterCounterControl.*SetTCVar` = 2 | [x] |
| 79 | CSTR behavioral test asserts accumulated string via GetCharacterString | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `GetCharacterString.*Assert\|Assert.*GetCharacterString\|Equal.*GetCharacterString` = 2 | [x] |
| 80 | HandleReactionCases is called from HandleCounterSource (call-chain verification) | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | matches | `HandleReactionCases\(` | [x] |
| 81 | CheckExpUp gated path: EXP not incremented when CheckExpUp returns false | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `CheckExpUp.*false\|ExpUp.*Skip\|!.*CheckExpUp\|ExpGated` = 2 | [x] |
| 82 | IEngineVariables MASTER resolution behavioral test exists | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `GetMaster\|MasterCharacter\|StubEngineVariables.*[Mm]aster\|engineVariables.*[Mm]aster` = 2 | [x] |
| 83 | New F803 files do not reference ERB interpreter types | code | Grep(Era.Core/Counter/) | not_contains | `EmuEra\|GlobalStatic\|Interpreter\|ErbScript` | [x] |
| 84 | ICharacterStringVariables.cs does not reference ERB interpreter types | code | Grep(Era.Core/Interfaces/ICharacterStringVariables.cs) | not_contains | `EmuEra\|GlobalStatic\|Interpreter\|ErbScript` | [x] |
| 85 | うふふ==2 STAIN side-effects are asserted in tests | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | gte | `Ufufu.*Stain\|Stain.*Ufufu\|うふふ.*GetStain\|GetStain.*うふふ` = 1 | [x] |
| 86 | UpdateShrinkage called in production CounterSourceHandler | code | Grep(Era.Core/Counter/CounterSourceHandler.cs) | gte | `\.UpdateShrinkage\(` = 8 | [x] |
| 87 | うふふ==2 test sets CFLAG precondition co-located with HandleCounterSource invocation | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs, multiline=true) | matches | `CharacterFlagIndex.*317\|CFlagUfufu.*317[\s\S]{0,800}HandleCounterSource\|HandleCounterSource[\s\S]{0,800}CharacterFlagIndex.*317\|CFlagUfufu.*317` | [x] |
| 88 | Case 502 (手淫) reaction behavioral spot-check test exists | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | matches | `Case502\|Reaction.*502\|ReactionCase.*502\|_502_\|502.*手淫\|手淫.*502` | [x] |
| 89 | Case 503 (フェラチオ) reaction behavioral spot-check test exists | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | matches | `Case503\|Reaction.*503\|ReactionCase.*503\|_503_\|503.*Fellatio\|Fellatio.*503` | [x] |
| 90 | Case 504 (パイズリ) reaction behavioral spot-check test exists | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs) | matches | `Case504\|Reaction.*504\|ReactionCase.*504\|_504_\|504.*Paizuri\|Paizuri.*504` | [x] |
| 91 | うふふ==2 test asserts TCVAR or EXP mutation co-located with Ufufu context | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs, multiline=true) | matches | `(Ufufu\|うふふ\|CFlagUfufu)[\s\S]{0,1200}(EjaculationLocationFlag\|EjaculationPleasureIntensity\|VSexExp\|GetExp\|SetTCVar)\|(EjaculationLocationFlag\|EjaculationPleasureIntensity\|VSexExp\|GetExp\|SetTCVar)[\s\S]{0,1200}(Ufufu\|うふふ\|CFlagUfufu)` | [x] |
| 92 | うふふ==2 test asserts SOURCE mutation co-located with Ufufu context | code | Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs, multiline=true) | matches | `(Ufufu\|うふふ\|CFlagUfufu)[\s\S]{0,1200}(GetSource\|SetSource\|SourceIndex)\|(GetSource\|SetSource\|SourceIndex)[\s\S]{0,1200}(Ufufu\|うふふ\|CFlagUfufu)` | [x] |

### AC Details

**AC#1: ICounterSourceHandler interface exists in Era.Core/Counter/**
- **Test**: Glob pattern="Era.Core/Counter/ICounterSourceHandler.cs"
- **Expected**: File exists
- **Rationale**: Goal item (1) requires a CounterSourceHandler class in Era.Core/Counter/. The interface must be defined first per DI pattern. Constraint C7 requires injectable interfaces.

**AC#2: CounterSourceHandler class implements ICounterSourceHandler**
- **Test**: Grep pattern=`class CounterSourceHandler.*ICounterSourceHandler` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches (class declaration inherits interface)
- **Rationale**: Verifies the implementation class exists and implements the correct interface. Goal item (1) specifies "CounterSourceHandler class in Era.Core/Counter/".

**AC#3: SourceIndex has Seduction constant (50)**
- **Test**: Grep pattern=`Seduction = new\(50\)` path="Era.Core/Types/SourceIndex.cs"
- **Expected**: Pattern matches
- **Rationale**: Constraint C11 requires 13 missing SourceIndex constants. Seduction (誘惑=50) is the most frequently used in dispatch branches. This is a representative sample AC; AC#7 verifies the total count.

**AC#4: StainIndex has well-known constants (8 stain types)**
- **Test**: Grep pattern=`public static readonly StainIndex` path="Era.Core/Types/StainIndex.cs" | count
- **Expected**: 8 (Mouth=0, Hand=1, Penile=2, Vaginal=3, Anal=4, Breast=5, InVagina=6, InIntestine=7)
- **Rationale**: Constraint C9 requires STAIN bitwise OR to match ERA semantics. The ERB uses 8 named stain indices (口,手,Ｐ,Ｖ,Ａ,Ｂ,膣内,腸内). Currently StainIndex has 0 constants (baseline).
- **Count Derivation Note**: The count of 8 is derived from ERB analysis of COUNTER_SOURCE.ERB stain indices, not from Stain.csv. Task#2's `[I]` tag applies to the sequential ORDER and integer VALUES of stain indices (which require CSV verification), not to the COUNT. If CSV verification in Task#2 reveals additional stain types used by COUNTER_SOURCE.ERB, both Task#2 description and AC#4 expected count must be updated accordingly.
- **Value Verification Note**: AC#4 verifies only the aggregate count of StainIndex constants, not individual integer values. Incorrect values (e.g., Vaginal=99 instead of 3) would pass AC#4 while producing wrong STAIN bitmasks in the bitwise OR operations (Constraint C9). Task#2's `[I]` tag is the designed verification mechanism — the implementer must verify each value against Stain.csv. No AC spot-checks individual StainIndex values (unlike AC#5 which spot-checks ExpIndex.VSexExp=20). This is a known soft-floor limitation symmetric with TCVarIndex value-verification (see Review Notes [pending]).

**AC#5: ExpIndex has VSexExp constant (20)**
- **Test**: Grep pattern=`VSexExp = new\(20\)` path="Era.Core/Types/ExpIndex.cs"
- **Expected**: Pattern matches
- **Rationale**: COUNTER_SOURCE.ERB uses EXP:Ｖ性交経験 (index 20) at lines 24, 423, 533. Goal item (5) requires missing index constants. Currently ExpIndex lacks VSexExp, ASexExp, MasturbationExp, PaizuriExp, SadisticExp, KissExp (VExp already exists from F812).

**AC#6: Dispatch method handles CounterActionId.SeductiveGesture case**
- **Test**: Grep pattern=`SeductiveGesture` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: Goal item (1) requires "all ~36 counter action dispatch branches faithfully translated". SeductiveGesture (CNT_色っぽい仕草) is the first CASE branch. Constraint C1 requires per-branch equivalence.

**AC#7: SourceIndex has all 12 missing constants (total 31)**
- **Test**: Grep pattern=`public static readonly SourceIndex` path="Era.Core/Types/SourceIndex.cs" | count
- **Expected**: 31 (current baseline is 31; 12 constants Liquid-Sadism already present from prior features. AC#7 serves as regression guard ensuring constants are not removed)
- **Rationale**: Constraint C11 requires all missing SourceIndex constants. Baseline is 31 per measurement (12 constants pre-existing). AC#7 verifies constants remain present as regression guard
- **Count Derivation Note**: The count of 31 assumes all 12 constants listed in Technical Design are distinct from the baseline 19. Task#1's `[I]` tag applies to integer VALUES (requiring CSV verification), not to the constant COUNT or identity. However, if CSV verification reveals that Liquid=9 or SexualActivity=11 aliases an existing constant, the implementer must update AC#7's expected value accordingly before proceeding. The count_equals matcher enforces structural precision — the implementer adjusts the expected value during [I] verification, not the matcher type.

**AC#8: UpdateMasterCounterControl method exists**
- **Test**: Grep pattern=`UpdateMasterCounterControl` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches (method name appears in production code)
- **Rationale**: Goal item (2) requires "SETBIT master counter control block translated to C# bitwise operations". AC#8 verifies the method exists in production code. AC#25 verifies test coverage for bitfield correctness. Together they ensure MCC logic is both implemented and tested.

**AC#9: DATUI_BOTTOM method is public**
- **Test**: Grep pattern=`public.*DatUIBottom\(` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: Goal item (3) requires "DATUI_* undressing helpers as shared public methods". Constraint C12 requires public visibility for F805 reuse. DATUI_BOTTOM (line 789) and DATUI_BOTTOM_T (line 818) zero EQUIP for bottom clothing.

**AC#10: DATUI_TOP method is public**
- **Test**: Grep pattern=`public.*DatUITop\(` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: Same as AC#9 but for DATUI_TOP (line 848) and DATUI_TOP_T (line 870). Both top and bottom helpers must be independently verified.

**AC#11: PainCheckVMaster method exists**
- **Test**: Grep pattern=`public.*PainCheckVMaster\(` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: Goal item (4) requires "PAIN_CHECK_V_MASTER with correct TIMES float-to-int truncation". Constraint C4 requires threshold boundary tests. The method (COUNTER_SOURCE.ERB:893-920) applies EXPLV/PALAMLV-based multipliers to SOURCE:加虐.

**AC#12: ICharacterStringVariables interface has CSTR string accessor**
- **Test**: Grep pattern=`SetCharacterString` path="Era.Core/Interfaces/ICharacterStringVariables.cs"
- **Expected**: Pattern matches at least once
- **Rationale**: Goal item (6) requires "CSTR string accumulation strategy resolved". Technical Design chose ICharacterStringVariables with GetCharacterString/SetCharacterString methods. Constraint C5 identifies 19 CSTR:x:10 call sites. The getter (GetCharacterString) already exists on IVariableStore, so this AC targets the specific ICharacterStringVariables.cs file and verifies only the setter (SetCharacterString) to prevent false-positive satisfaction from the pre-existing getter.

**AC#13: CounterSourceHandler constructor injects IVariableStore**
- **Test**: Grep pattern=`IVariableStore` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: Goal item (7) requires "all external calls behind injectable interfaces". Constraint C7 requires DI for each external dependency. IVariableStore is the primary dependency for SOURCE/STAIN/EXP/EQUIP/TCVAR access.

**AC#14: Era.Core builds without warnings**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'`
- **Expected**: Exit code 0 (TreatWarningsAsErrors=true in Directory.Build.props)
- **Rationale**: Technical Constraint: TreatWarningsAsErrors=true. All new code must compile cleanly.

**AC#15: All unit tests pass**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/'`
- **Expected**: Exit code 0 (all tests pass)
- **Rationale**: Goal item (8) requires "equivalence tests verifying branch-level behavioral parity". TDD RED-GREEN cycle ensures implementation correctness.

**AC#16: DATUI_BOTTOM_T method is public**
- **Test**: Grep pattern=`public.*DatUIBottomT\(` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: Constraint C12 requires public visibility for F805 reuse. DATUI_BOTTOM_T (COUNTER_SOURCE.ERB:818) handles target character bottom undressing.

**AC#17: DATUI_TOP_T method is public**
- **Test**: Grep pattern=`public.*DatUITopT\(` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: Same as AC#16 but for top undressing variant (COUNTER_SOURCE.ERB:870).

**AC#18: ITouchSet interface file exists**
- **Test**: Glob pattern="Era.Core/Counter/ITouchSet.cs"
- **Expected**: File exists
- **Rationale**: Technical Design requires ITouchSet interface for TOUCH_SET abstraction (~26 call sites). Constraint C14.

**AC#19: IShrinkageSystem interface file exists**
- **Test**: Glob pattern="Era.Core/Counter/IShrinkageSystem.cs"
- **Expected**: File exists
- **Rationale**: Technical Design requires IShrinkageSystem interface for 締り具合変動 abstraction (8 call sites).

**AC#20: HandleCounterSource dispatch is exhaustive (default throws on unknown)**
- **Test**: Grep pattern=`_ =>.*throw|default.*throw|ArgumentOutOfRangeException` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches (exhaustive switch with throwing default)
- **Rationale**: Goal Item 1 requires "all ~36 counter action dispatch branches faithfully translated". Constraint C1 requires per-branch equivalence. An exhaustive switch with a throwing default ensures no CounterActionId values are silently ignored.

**AC#21: CounterSourceHandler constructor injects IVirginityManager**
- **Test**: Grep pattern=`IVirginityManager` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: Goal Item 7 requires "all external calls behind injectable interfaces". The constructor must inject 11 interfaces per Technical Design. AC#13 verifies IVariableStore, AC#18 verifies ITouchSet, AC#19 verifies IShrinkageSystem. AC#21 spot-checks IVirginityManager (needed for LOST_VIRGIN_* calls at 3 sites).

**AC#22: PainCheckVMaster boundary truncation test exists**
- **Test**: Grep pattern=`PainCheckVMaster` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Expected**: count >= 3 (test method declaration + at least 2 boundary tests at threshold multiplier values)
- **Rationale**: Goal Item 4 requires "PAIN_CHECK_V_MASTER with correct TIMES float-to-int truncation". Constraint C4 requires "threshold boundary tests with float multipliers". AC#11 only verifies method existence. AC#22 ensures at least three references exist: the test method declaration plus two boundary test invocations (boundary above and below the threshold), guaranteeing both sides of the truncation boundary are tested rather than just a single trivial reference.
- **Precision Note**: AC#22's `PainCheckVMaster` pattern counts all method name references and cannot distinguish above-threshold invocations from below-threshold invocations. A single test method calling PainCheckVMaster 3 times with the same boundary value satisfies gte=3 without testing both sides. **Combined constraint**: Task#14 explicitly documents "PainCheckVMaster boundary values" in the test requirement, and the implementer-facing constraint C4 mandates "threshold boundary tests with float multipliers" — these ensure the test implementation covers both sides even though the AC matcher cannot structurally enforce it.

**AC#23: TCVarIndex has MasterCounterControl constant**
- **Test**: Grep pattern=`MasterCounterControl = new\(` path="Era.Core/Types/TCVarIndex.cs"
- **Expected**: Pattern matches
- **Rationale**: Task#6 adds 9 TCVarIndex constants. MasterCounterControl (マスターカウンター制御) is the most critical for F803's UpdateMasterCounterControl method. Unlike SourceIndex (AC#3, AC#7), StainIndex (AC#4), and ExpIndex (AC#5), TCVarIndex had no dedicated AC.

**AC#24: Dispatch branch equivalence tests exist for all 6 CounterActionId dispatch ranges**
- **Test**: Grep pattern=`CounterActionId\.SeductiveGesture|CounterActionId\.MilkHandjob|CounterActionId\.Kiss|CounterActionId\.Masturbation|CounterActionId\.ForcedCunnilingus|CounterActionId\.MissionaryInsert` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Matcher**: gte
- **Expected**: count >= 6 (one match per CounterActionId dispatch range representative ensures all 6 dispatch ranges with actual COUNTER_SOURCE.ERB branches have at least one behavioral test)
- **Rationale**: Goal Item 8 requires behavioral parity via category-representative tests. Constraint C1 requires per-branch equivalence. The 6 pattern keywords are fully-qualified CounterActionId enum member names with confirmed CASE branches in COUNTER_SOURCE.ERB, one representative per dispatch range: SeductiveGesture (10-16: social), MilkHandjob (29: the only 21-29 range branch in the ERB), Kiss (30-35: kiss/embrace), Masturbation (50-60: stimulation), ForcedCunnilingus (70-75: forced acts), MissionaryInsert (80-91: penetration). The 40-47 range (VirginOffering through SexualReliefA) has zero dispatch branches in COUNTER_SOURCE.ERB and is excluded (Key Decision documented). Fully-qualified `CounterActionId.X` format prevents cross-contamination from bare substrings: `Kiss` appears in KissExp/VirginityType.Kiss, `Masturbation` appears in MasturbationExp/AC#65 case 505 tests. Prior SourceIndex cross-contamination fix (Review Notes line 1651) used bare enum names; this further refinement uses qualified names.

**AC#25: Master counter control bitfield tests cover both MASTER and ARG characters**
- **Test**: Grep pattern=`MasterCounter|UpdateMasterCounterControl` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Matcher**: gte
- **Expected**: count >= 3 (at minimum: MASTER-character MCC test + ARG-character MCC test + shared setup/assertion; ensures both character paths are tested, not just presence of a single MCC test)
- **Rationale**: Goal Item 8 requires equivalence tests. Constraint C2 requires "bitwise operation verification per CASE". The ERB MCC block has character-specific CASE branches — some write TCVAR:MASTER, others write TCVAR:ARG. AC#25's previous `matches` matcher was satisfied by a single test covering only one character path. The gte=3 threshold ensures multiple MCC test references exist, structurally enforcing coverage of both MASTER and ARG character paths. Combined with AC#78 (production code dual-character verification) and AC#77 (MCC test invocation + assertion depth), this closes the MASTER/ARG indistinguishability gap documented in AC#78's Precision Note.
- **Precision Note**: The gte=3 threshold counts keyword occurrences file-wide and cannot structurally distinguish MASTER-targeted tests from ARG-targeted tests. A test suite calling UpdateMasterCounterControl 3+ times exclusively for MASTER-only inputs satisfies AC#25 while leaving ARG-targeted branches untested. Combined constraint: Task#14 description requires "both character paths are tested" + AC#77 (MCC test invocation + assertion gte=5) provides depth. The file-wide matcher is a known limitation shared with AC#78 (production-side).

**AC#26: All 7 reaction case numbers (500, 502-505, 600-601) are handled in dispatch**
- **Test**: Grep pattern=`case 500|case 502|case 503|case 504|case 505|case 600|case 601|500 =>|502 =>|503 =>|504 =>|505 =>|600 =>|601 =>` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Matcher**: gte
- **Expected**: count >= 7 (one line match per each of the 7 reaction case numbers: 500, 502, 503, 504, 505, 600, 601; both `case N` style and `N =>` switch expression style are accepted)
- **Rationale**: Constraint C10 requires "separate handling from CounterActionId dispatch" for reaction cases (500-601). The Technical Design specifies a nested int switch. AC#26 verifies that the reaction case range is actually implemented, not silently ignored by the outer CounterActionId switch. COUNTER_SOURCE.ERB has exactly 7 distinct CASE values (500, 502, 503, 504, 505, 600, 601) — CASE 501 is intentionally absent from the ERB source (lines 539-631 confirm the sequence jumps 500→502 with no 501 handler). All 7 existing cases should be verified, not just a 3-sample spot check. The pattern supports both traditional `switch` statement syntax (`case 500`) and C# switch expression syntax (`500 =>`), ensuring implementation flexibility.

**AC#27: CounterSourceHandler constructor injects IRandomProvider**
- **Test**: Grep pattern=`IRandomProvider` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: COUNTER_SOURCE.ERB has 15 RAND:N expressions requiring random.Next(N). IRandomProvider injection follows ActionSelector pattern from F801. Technical Constraints acknowledge RAND:N translation requirement.

**AC#28: ICounterUtilities has CheckExpUp method**
- **Test**: Grep pattern=`CheckExpUp` path="Era.Core/Counter/ICounterUtilities.cs"
- **Expected**: Pattern matches
- **Rationale**: COUNTER_SOURCE.ERB calls EXP_UP at 3 sites in reaction cases (502-504). Technical Design maps EXP_UP to ICounterUtilities.CheckExpUp. This AC verifies the method is added to the interface.

**AC#29: CounterSourceHandler constructor injects ICharacterStringVariables**
- **Test**: Grep pattern=`ICharacterStringVariables` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: Goal Item 6 requires "CSTR string accumulation strategy resolved". AC#12 verifies the ICharacterStringVariables interface exists in Era.Core/Interfaces/. AC#29 verifies it is injected into CounterSourceHandler, following the same pattern as AC#13 (IVariableStore), AC#21 (IVirginityManager), AC#27 (IRandomProvider).

**AC#30: CounterSourceHandler constructor injects IShrinkageSystem**
- **Test**: Grep pattern=`IShrinkageSystem` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: Technical Design requires IShrinkageSystem for 締り具合変動 abstraction (8 call sites). AC#19 verifies the interface file exists. AC#30 verifies injection into CounterSourceHandler, following the AC#13/AC#21/AC#27/AC#29 pattern.

**AC#31: CounterSourceHandler calls CheckExpUp for EXP_UP translation**
- **Test**: Grep pattern=`CheckExpUp` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: COUNTER_SOURCE.ERB has 3 EXP_UP call sites in reaction cases (502-504). AC#28 verifies CheckExpUp exists on ICounterUtilities interface. AC#31 verifies CounterSourceHandler actually calls it, ensuring the EXP_UP sites are translated.

**AC#32: UpdateMasterCounterControl uses bitwise OR assignment**
- **Test**: Grep pattern=`\|=` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches (bitwise OR assignment operator appears in production code)
- **Rationale**: Goal Item 2 requires "SETBIT master counter control block translated to C# bitwise operations". AC#8 verifies the method exists. AC#25 verifies test coverage exists. AC#32 verifies the production code actually uses bitwise OR assignment (`|=`), ensuring SETBIT semantics are faithfully translated rather than using replacement or addition.
- **Precision Note**: The `\|=` matcher is file-scoped (not method-scoped). STAIN merge operations (`stain |= value`) elsewhere in the file can also satisfy this matcher without UpdateMasterCounterControl containing any `|=`. This is a known limitation accepted via combined constraint: AC#25 (MCC behavioral test gte=3) and AC#77 (MCC test invocations gte=5) provide behavioral verification that UpdateMasterCounterControl produces correct bitmasks. AC#32 acts as a structural spot-check; the behavioral correctness guarantee comes from AC#25+AC#77.

**AC#33: Reaction cases dispatched via dedicated HandleReactionCases method**
- **Test**: Grep pattern=`HandleReactionCases` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches (method name appears in production code)
- **Rationale**: Constraint C10 requires "separate handling from CounterActionId dispatch" for reaction cases (500-601). The Key Decision specifies nested int switch. AC#26 verifies reaction case numbers appear but doesn't enforce structural separation from the CounterActionId enum switch. AC#33 verifies a dedicated `HandleReactionCases` private method exists, ensuring the C10 separation requirement is structurally enforced.

**AC#34: CounterSourceHandler constructor injects IClothingSystem**
- **Test**: Grep pattern=`IClothingSystem` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: IClothingSystem is required by all 4 DATUI helpers (CLOTHES_SETTING_TRAIN calls at lines 816, 845, 868, 890). Follows the injection verification pattern of AC#13/AC#21/AC#27/AC#29/AC#30.

**AC#35: CounterSourceHandler constructor injects IEngineVariables**
- **Test**: Grep pattern=`IEngineVariables` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: IEngineVariables is required for global TCVAR MASTER writes (7 occurrences of `TCVAR:行為者 = ARG`). Provides MASTER character resolution.

**AC#36: CounterSourceHandler constructor injects ICommonFunctions**
- **Test**: Grep pattern=`ICommonFunctions` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: ICommonFunctions provides HasPenis/HasVagina for gender-conditional logic in dispatch branches.

**AC#37: CounterSourceHandler constructor injects IPainStateChecker**
- **Test**: Grep pattern=`IPainStateChecker` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: IPainStateChecker is required for the single PAIN_CHECK_V(TARGET) call at line 525 (CNT_騎乗位逆レイプ branch). Distinct from PainCheckVMaster which is inlined.

**AC#38: CounterSourceHandler constructor injects ITEquipVariables**
- **Test**: Grep pattern=`ITEquipVariables` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: ITEquipVariables is required for TEQUIP writes (lines 424, 535) recording sexual activity partners.

**AC#39: CounterSourceHandler constructor injects ICounterUtilities**
- **Test**: Grep pattern=`ICounterUtilities` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: ICounterUtilities is the 12th injected dependency. Provides IsOnce (for ONCE calls) and CheckExpUp (for EXP_UP translation, AC#28/AC#31). Completes the DI injection verification pattern for all 12 constructor parameters.

**AC#40: Pre-check guard: early return when CounterDecisionFlag > 1**
- **Test**: Grep pattern=`CounterDecisionFlag` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: Constraint C6 requires "Pre-check guard (TCVAR:ARG:カウンター行動決定フラグ > 1 return) must be preserved". This guard is at COUNTER_SOURCE.ERB:2 and causes early return before dispatch. AC#40 verifies the guard references CounterDecisionFlag in production code.

**AC#41: Pre-check guard test: early return + うふふ==2 special path tests exist**
- **Test**: Grep pattern=`EarlyReturn|PreCheck|Ufufu|SpecialPath` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Matcher**: gte
- **Expected**: count >= 4 (4 C6-exclusive keywords: EarlyReturn for guard path test, PreCheck for guard method/test naming, Ufufu for うふふ==2 special path test, SpecialPath for special path naming)
- **Rationale**: Constraint C6 requires behavioral preservation of the early-return guard AND the CFLAG:ARG:うふふ==2 pre-SELECTCASE block. AC#40 verifies production code presence (including CounterDecisionFlag). AC#41 verifies tests covering both paths exist. CounterDecisionFlag was removed from the matcher because it is a production identifier that appears in any test's arrange step (setting the flag to bypass/trigger the guard), making it inflatable by non-C6 tests. The 4 remaining keywords are C6-exclusive test naming conventions. SetSource/GetSource were also previously removed to prevent inflation by unrelated dispatch branch tests (AC#50 covers SOURCE accessor usage file-wide with gte=20).
- **Naming Constraint**: The matcher requires English keywords (EarlyReturn, PreCheck, Ufufu, SpecialPath) in test method or class names. Implementer MUST use these English terms — Japanese test naming (e.g., 早期リターンテスト) would fail the matcher.

**AC#42: CSTR string append behavior test exists**
- **Test**: Grep pattern=`SetCharacterString|CSTR.*Equal` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Expected**: count >= 6 (at least 3 references per CSTR pattern: player-accumulates-target (14 ERB sites) and target-accumulates-player (5 ERB sites). The gte=6 threshold ensures both patterns have dedicated test coverage, preventing satisfaction by testing only one accumulation direction.)
- **Rationale**: Goal Item 6 requires "CSTR string accumulation strategy resolved". AC#12 verifies interface exists, AC#29 verifies injection, AC#75 verifies all 19 production call sites. AC#42 verifies sufficient test depth across both CSTR accumulation patterns. Two distinct patterns exist: (1) `CSTR:PLAYER:10 += {ARG}/` (player accumulates target ID, 14 sites) and (2) `CSTR:ARG:10 += {PLAYER}/` (target accumulates player ID, 5 sites). The gte=6 threshold (3 per pattern) ensures both are independently tested.
- **Precision Note**: The inflatable sub-pattern `CharacterString.*Assert` was removed because it matches DI injection assertions (e.g., `Assert.NotNull(characterStringVariables)`), stub class declarations, and type-checking assertions that are not CSTR content verification. AC#79 (GetCharacterString read-back gte=2) separately covers assertion-side verification of CSTR values. The gte=6 threshold intends 3 per CSTR pattern, but the Grep matcher cannot structurally enforce per-pattern minimums — all 6 matches could come from the player-accumulates-target direction alone. Implementer MUST include test methods for both directions: Task#14 requires at least one test explicitly exercising the target-accumulates-player path (CSTR:ARG:10 += {PLAYER}/, e.g., Ｖセックス or 正常位 branches).

**AC#43: TCVarIndex has all 4 truly new constants (total 20)**
- **Test**: Grep pattern=`public static readonly TCVarIndex` path="Era.Core/Types/TCVarIndex.cs" | count
- **Expected**: 20 (baseline 16 + 4 truly new: MasterCounterControl, EjaculationLocationFlag, EjaculationPleasureIntensity, PositionRelationship; SubAction/LowerBodyUndressAction/UpperBodyUndressAction/TargetLowerBodyUndressAction/TargetUpperBodyUndressAction/AboutToCome already added by F801/F812)
- **Rationale**: Follows AC#7 (SourceIndex count_equals) pattern. AC#23 spot-checks MasterCounterControl but the other 3 truly new constants have no individual AC. AC#43 provides aggregate coverage ensuring all 4 new constants are added.
- **Count Derivation Note**: Baseline 16 (F801/F812 added 6 constants: SubAction, UndressingPlayerLower, UndressingPlayerUpper, UndressingTargetLower, UndressingTargetUpper, SixtyNineTransition). The count of 4 truly new constants is from ERB analysis with F801/F812 overlap removed. Task#6's `[I]` tag applies to integer values needing DIM.ERH verification, not to the count.

**AC#44: UpdateMasterCounterControl has sufficient SETBIT-to-bitshift translations (branch completeness)**
- **Test**: Grep pattern=`1 <<` path="Era.Core/Counter/CounterSourceHandler.cs" | count
- **Expected**: count >= 57 [I] (MCC block has ~57 SETBIT operations; 100% SETBIT coverage required per Zero Debt Upfront principle. [I] tag: implementer must verify exact SETBIT count from COUNTER_SOURCE.ERB:637-786 before implementation and update this threshold if count differs from 57)
- **Rationale**: Goal Item 2 requires "SETBIT master counter control block translated to C# bitwise operations". AC#8 verifies method name, AC#32 verifies `|=` exists, AC#25 verifies test exists. AC#44 provides branch completeness assurance by counting `1 <<` (bit-shift) occurrences in the file. Each SETBIT branch translates to `currentValue | (1 << bit)`, so the count of `1 <<` directly reflects MCC branch coverage without conflation from STAIN merge operations. The threshold of 57 (100% of ~57 total SETBITs) enforces complete implementation with zero unimplemented branches.
- **Precision Note**: The `1 <<` pattern is file-scoped (not method-scoped). Non-MCC bit-shift expressions elsewhere in CounterSourceHandler.cs could inflate the count. This is a known limitation accepted via combined constraint: AC#25 (MCC behavioral test gte=3) and AC#77 (MCC test invocations gte=5) verify UpdateMasterCounterControl correctness behaviorally. STAIN merge operations use direct OR (`|`) not bit shifts, avoiding cross-contamination from that source. If multiple SETBIT operations share a single expression line, count may undercount (unsafe but unlikely given SETBIT-per-branch ERB structure).
- **Implementation Constraint**: Technical Design mandates `currentValue | (1 << bit)` pattern for all SETBIT translations. Alternative implementations (e.g., pre-computed bitmask tables) are not permitted per design. AC#44 threshold is aligned with this constraint.

**AC#45: STAIN bitwise OR merge behavior test exists**
- **Test**: Grep pattern=`Stain|StainIndex` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Matcher**: gte
- **Expected**: count >= 3 (covers うふふ==2 STAIN bitwise OR mutations + dispatch branch STAIN writes + assertion references)
- **Rationale**: Constraint C9 requires "STAIN bitwise OR must match ERA semantics". AC#4 verifies StainIndex constant count (structural). AC#45 verifies sufficient STAIN test references exist, ensuring both dispatch branch STAIN writes and the うふふ==2 block's `STAIN |=` mutations (COUNTER_SOURCE.ERB:22-23) are tested. The gte=3 threshold prevents satisfaction by a single incidental reference and ensures both code paths contributing STAIN mutations have test coverage.

**AC#46: Dispatch branch equivalence test asserts concrete output state**
- **Test**: Grep pattern=`Assert\.Equal\|GetSource\|GetStain` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs" | count
- **Expected**: count >= 3 (at least 3 assertion/verification references on concrete SOURCE/STAIN state values across test methods)
- **Rationale**: Goal Item 8 requires "equivalence tests verifying branch-level behavioral parity with the original ERB". AC#24 only checks branch name presence in test file. AC#46 ensures the tests actually assert and verify concrete output state (SOURCE/STAIN variable reads), verifying behavioral parity rather than just method existence. SetSource/SetStain are excluded from the matcher because they are setup operations (test arrangement), not assertions — including them would allow the threshold to be satisfied by setup code alone. AC#59 (Assert.Equal gte=2) and AC#66 (GetSource|GetStain gte=2) provide complementary coverage; AC#46 adds a combined minimum across assertion and read patterns.

**AC#47: ICounterSourceHandler interface declares HandleCounterSource, DATUI and PainCheckVMaster methods**
- **Test**: Grep pattern=`void HandleCounterSource\|void DatUIBottom\|void DatUIBottomT\|void DatUITop\|void DatUITopT\|void PainCheckVMaster` path="Era.Core/Counter/ICounterSourceHandler.cs" | count
- **Expected**: count >= 6 (HandleCounterSource entry point + 5 helper method signatures)
- **Rationale**: AC#1 only verifies ICounterSourceHandler.cs file exists. AC#9, AC#10, AC#16, AC#17, AC#11 verify the IMPLEMENTATION class has public methods but don't verify the signatures exist on the INTERFACE. F805 depends on ICounterSourceHandler via DIP (not the concrete class). If methods are only on the class and not the interface, F805 DIP compliance breaks. AC#47 covers HandleCounterSource and 5 helper names. HandleCounterSource is the primary entry point explicitly named in Goal Item 1 and Technical Design. Without it in the matcher, the main method signature could be missing from the interface while all ACs pass.

**AC#48: ExpIndex has all new constants (total >= 14)**
- **Test**: Grep pattern=`public static readonly ExpIndex` path="Era.Core/Types/ExpIndex.cs" | count
- **Expected**: count >= 14 (baseline 11 + 6 new: VSexExp=20, ASexExp=22, MasturbationExp=24, PaizuriExp=26, SadisticExp=100, KissExp=27. Total = 17, threshold 14 provides margin.)
- **Rationale**: AC#5 only spot-checks VSexExp=20. Task#3 adds 6 new ExpIndex constants (VExp and AExp2 removed — already exist from F812; KissExp corrected to index 27 per CSV). Without a count AC, constants could be silently omitted. Threshold 14 is conservative (actual target 17) to account for [I]-tagged implementer verification.
- **Count Derivation Note**: The count of 14 minimum is conservative. Task#3's `[I]` tag applies to all integer VALUES and potential aliases. The implementer may add up to 8 new constants (total 16) if CSV verification confirms no aliases, or fewer if aliases exist.
- **Value Verification Note**: AC#48 verifies aggregate count, not individual integer values. AC#5 spot-checks VSexExp=20 as the single value verification. The remaining 5 new constants (ASexExp=22, MasturbationExp=24, PaizuriExp=26, SadisticExp=100, KissExp=27) have no individual value ACs. Task#3's `[I]` tag is the designed verification mechanism. This is a known soft-floor limitation symmetric with TCVarIndex and StainIndex value-verification (see Review Notes [pending]).

**AC#49: HandleCounterSource has sufficient dispatch cases (branch completeness)**
- **Test**: Grep pattern=`CounterActionId\.[A-Z]` path="Era.Core/Counter/CounterSourceHandler.cs" | count
- **Expected**: count >= 36 (matches the actual 36 CNT_ CASE branches in COUNTER_SOURCE.ERB confirmed by ERB source grep)
- **Rationale**: Goal Item 1 claims "all ~36 counter action dispatch branches faithfully translated". AC#6 spot-checks SeductiveGesture, AC#20 verifies exhaustive default throw, AC#24 verifies 7 branch names in tests. None enforce a minimum count of dispatch cases in production code. A 10-case switch with a throwing default passes all three. AC#49 provides a floor matching the actual branch count (36) that prevents token implementations from passing.
- **Precision Note**: The matcher `CounterActionId\.[A-Z]` counts all enum member references (case labels, method arguments, local variables) — it is a soft floor, not a structural guarantee of 36 dispatch cases. Non-dispatch references (e.g., passing enum values to helpers, exception messages) can inflate the count. This is a known trade-off from reverting the previous case-label-specific pattern which had double-counting issues. **Combined constraint**: AC#49 (gte=36 enum references) + AC#20 (exhaustive default throw) + AC#24 (7 categories in tests) + AC#6 (SeductiveGesture spot-check) together make it extremely unlikely that a handler with fewer than 36 real dispatch cases satisfies all four. AC#49 alone is insufficient; it functions as a combined floor enforced by the AC set.

**AC#50: Equivalence tests call SetSource/GetSource sufficient times for behavioral parity**
- **Test**: Grep pattern=`SetSource\|GetSource` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs" | count
- **Expected**: count >= 20 (dispatch branches + うふふ==2 block + PainCheckVMaster collectively modify ~50 SOURCE values; 20 is a floor ensuring substantial state manipulation across test methods)
- **Rationale**: AC#46 verifies `Assert.Equal|GetSource|SetSource|GetStain|SetStain` >= 3, which is a very low bar. AC#50 specifically counts SOURCE accessor calls (SetSource/GetSource) which are the primary state mutation/assertion pattern for equivalence tests. The うふふ==2 block alone modifies 6 SOURCE values (性行動, 征服, 液体, 加虐, 強要, 与快Ｖ at COUNTER_SOURCE.ERB:10-15); dispatch branches collectively modify dozens more. A threshold of 20 ensures the test file contains substantial SOURCE state manipulation, not just token assertions. This addresses the gap where AC#41 verifies うふふ==2 test existence but not its assertion depth, and where AC#46's threshold of 3 can be satisfied without covering うふふ==2 mutations.
- **Precision Note**: The SetSource|GetSource pattern is grepped in CounterSourceHandlerTests.cs only. The local stubs in this file (StubEngineVariables, StubShrinkageSystem, StubTouchSet, StubCharacterStringVariables) do not implement IVariableStore methods and thus do not inflate the count. SetSource/GetSource calls come from test method bodies that set up state via the F815 StubVariableStore dependency and read back for assertion. AC#46 (Assert.Equal count >= 3) and AC#66 (GetSource|GetStain count >= 2) provide complementary behavioral verification that test methods actually assert on state values.

**AC#51: MCC bitfield test asserts concrete bitmask output state**
- **Test**: Grep pattern=`MasterCounterControl\|MasterCounter` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs" | count
- **Expected**: count >= 3 (setup + assertion for at least 2 MCC test cases, referencing the constant by name)
- **Rationale**: AC#25 verifies test method name existence (`MasterCounter|UpdateMasterCounterControl`). AC#32 verifies `|=` in production code. AC#44 verifies `1 <<` count >= 25. None verify that tests actually assert specific bitmask OUTPUT values. AC#51 ensures the test file references MasterCounterControl/MasterCounter at least 3 times (indicating meaningful state setup and assertion in test methods). Constraint C2 requires "bitwise operation verification per CASE" — this is verified by tests that set up specific CASE inputs and assert specific bitmask outputs, which must reference MasterCounterControl by name.

**AC#52: Reaction cases 500, 502-505 implement SubAction nested sub-dispatch (600-601 excluded per Technical Design)**
- **Test**: Grep pattern=`SubAction` path="Era.Core/Counter/CounterSourceHandler.cs" | count
- **Matcher**: gte
- **Expected**: count >= 10 (5 reaction cases × 2 minimum references each: GetTCVar read + switch dispatch reference)
- **Rationale**: Reaction cases 500, 502-505 each contain nested SELECTCASE on TCVAR:ARG:カウンター行動の派生 (SubAction, F801-established name at index 21) with distinct sub-case behaviors (TCVAR writes, SOURCE writes, CSTR appends, EXP_UP gating, EXP increments). AC#26 verifies top-level case numbers. AC#33 verifies HandleReactionCases method name. Neither verifies the nested sub-dispatch logic exists. A HandleReactionCases that handles the outer switch but returns without sub-dispatch would pass all ACs while missing all conditional sub-case logic. 5 reaction cases use nested SubAction dispatch (cases 500, 502-505). Threshold 10 (5 cases × 2 minimum references) ensures all 5 cases implement the nested dispatch, making omission of any single case detectable.

**AC#53: Reaction case sub-dispatch tests reference SubAction**
- **Test**: Grep pattern=`SubAction` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs" | count
- **Expected**: count >= 10 (5 reaction sub-cases × 2 minimum references each: test setup + assertion, matching AC#52 production-code threshold)
- **Rationale**: AC#52 verifies production code implements the nested sub-dispatch for all 5 reaction sub-cases (500, 502-505) with gte=10. AC#53 enforces symmetric test coverage: each of the 5 sub-cases should have at least 2 test references (setup + assertion) to SubAction, ensuring sub-case branching is exercised in tests with the same coverage floor as production code. Uses `SubAction` (F801-established name at index 21) to match AC#52's pattern specificity and avoid false inflation from incidental substring matches.
- **Precision Note**: AC#53's bare `SubAction` pattern may count non-behavioral references (test variable names, comments). The gte=10 floor is a soft constraint that can be satisfied by 2 heavily-referenced sub-cases (5+ refs each) while leaving remaining sub-cases unimplemented. Behavioral verification depends on AC#59 (Assert.Equal gte=2), AC#62 (cases 600-601 behavioral), AC#65 (case 505 sub-case 2 ungated EXP), and AC#81 (CheckExpUp gated path). Spot-check coverage: case 505 (AC#65), cases 600-601 (AC#62). Sub-cases 502, 503, 504 now have individual behavioral spot-check ACs (AC#88, AC#89, AC#90) — their coverage no longer relies solely on the gte=10 soft floor and implementer discipline. Implementers MUST include test ARRANGE (SetTCVar) and ASSERT (GetTCVar + Assert.Equal) patterns for EACH of the 5 reaction sub-cases (500, 502, 503, 504, 505).

**AC#54: Equivalence tests invoke handler methods (not just reference names)**
- **Test**: Grep pattern=`HandleCounterSource\|\.DatUIBottom\|\.DatUITop\|\.PainCheckVMaster\|\.UpdateMasterCounterControl` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs" | count
- **Expected**: count >= 5 (at least 5 method invocations across test methods — covering dispatch, DATUI, PainCheckVMaster, and MCC tests)
- **Rationale**: AC#24 verifies branch names appear in test file (could be in comments/declarations). AC#46/AC#50 verify assertion/accessor counts (could be in test setup without handler invocation). AC#54 verifies the tests actually CALL the handler's public methods by matching method invocation patterns (`.MethodName` indicating member access). This closes the gap where all existing ACs could pass on a test file that references names and sets up state but never invokes the handler.

**AC#55: CounterSourceHandler constructor injects ITouchSet**
- **Test**: Grep pattern=`ITouchSet` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches
- **Rationale**: ITouchSet is the 4th constructor parameter per Technical Design (TOUCH_SET abstraction with ~26 call sites). AC#18 verifies the ITouchSet.cs file exists but not that it is injected into CounterSourceHandler. Every other of the 12 constructor dependencies has a dedicated injection AC (AC#13 IVariableStore, AC#21 IVirginityManager, AC#27 IRandomProvider, AC#29 ICharacterStringVariables, AC#30 IShrinkageSystem, AC#34 IClothingSystem, AC#35 IEngineVariables, AC#36 ICommonFunctions, AC#37 IPainStateChecker, AC#38 ITEquipVariables, AC#39 ICounterUtilities). AC#55 closes the gap for the 12th injection (ITouchSet).

**AC#56: DATUI helper tests verify EQUIP zeroing behavior**
- **Test**: Grep pattern=`DatUIBottom\|DatUIBottomT\|DatUITop\|DatUITopT` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs" | count
- **Expected**: count >= 4 (at least one test per DATUI helper variant)
- **Rationale**: Constraint C3 requires "4 DATUI_* functions must zero EQUIP identically across undressing levels" with "equipment state verification per function" as AC Impact. AC#9, AC#10, AC#16, AC#17 only verify public visibility. AC#47 verifies interface signatures. AC#54 verifies method invocation exists. AC#56 ensures each of the 4 DATUI helpers has dedicated test coverage by name, closing the behavioral verification gap for C3.

**AC#57: TestCounterUtilities stub has CheckExpUp method**
- **Test**: Grep pattern=`CheckExpUp` path="Era.Core.Tests/Counter/ActionValidatorTests.cs"
- **Expected**: Pattern matches at least once
- **Rationale**: Task#16 adds CheckExpUp to ICounterUtilities. TestCounterUtilities in ActionValidatorTests.cs implements ICounterUtilities for testing. Without a CheckExpUp stub, ActionValidatorTests.cs will fail to compile. AC#28 only verifies the interface declaration; AC#57 verifies the test stub exists.

**AC#58: SelectorTestCounterUtils stub has CheckExpUp method**
- **Test**: Grep pattern=`CheckExpUp` path="Era.Core.Tests/Counter/ActionSelectorTests.cs"
- **Expected**: Pattern matches at least once
- **Rationale**: SelectorTestCounterUtils in ActionSelectorTests.cs also implements ICounterUtilities for testing. Without a CheckExpUp stub, ActionSelectorTests.cs will fail to compile. AC#28 only verifies the interface declaration; AC#58 verifies the second test stub exists.

**AC#59: Dispatch branch tests contain Assert.Equal assertions for output verification**
- **Test**: Grep pattern=`Assert\.Equal` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs" | count
- **Expected**: count >= 2 (at least 2 Assert.Equal assertions verifying output values after HandleCounterSource invocation)
- **Rationale**: Goal Item 8 requires "equivalence tests verifying branch-level behavioral parity with the original ERB". Split from original combined matcher for robustness — decoupling assertion existence (AC#59) from state-read patterns (AC#66) ensures multi-line assertion styles are supported. AC#59 verifies test assertions exist; AC#66 verifies state reads exist. Together they guarantee behavioral parity verification without requiring same-line co-location.

**AC#60: CounterSourceHandler.cs has no TODO/FIXME/HACK markers**
- **Test**: Grep pattern=`TODO\|FIXME\|HACK` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern does NOT match (zero occurrences)
- **Rationale**: Phase 21 Sub-Feature Requirements mandate zero technical debt markers in new files. CounterSourceHandler.cs is the primary deliverable of F803. Any TODO/FIXME/HACK indicates deferred work that should be tracked via Mandatory Handoffs instead.

**AC#61: ICounterSourceHandler.cs has no TODO/FIXME/HACK markers**
- **Test**: Grep pattern=`TODO\|FIXME\|HACK` path="Era.Core/Counter/ICounterSourceHandler.cs"
- **Expected**: Pattern does NOT match (zero occurrences)
- **Rationale**: ICounterSourceHandler.cs is a public interface consumed by F805 and F811. No technical debt markers should exist in interface declarations.

**AC#62: Reaction cases 600-601 have behavioral test coverage**
- **Test**: Grep pattern=`600\|601\|玉揉み\|TesticularMassage\|AnalStimulation` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs" | count
- **Expected**: count >= 2 (at least one test reference per reaction case)
- **Rationale**: Reaction cases 600 (玉揉み) and 601 (アナル愛撫) perform SOURCE/TCVAR mutations documented in Technical Design but were previously excluded from behavioral test ACs (AC#52/AC#53 only cover SubAction cases 500, 502-505). AC#26 verifies case numbers exist in production code but not that tests exercise their behavior. AC#62 ensures at least minimal test coverage for these two cases.

**AC#63: PainCheckVMaster uses int-cast truncation pattern (ERA TIMES fidelity)**
- **Test**: Grep pattern=`\(int\)\(.*\*` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: matches (at least one `(int)(... *` cast-multiplication pattern)
- **Rationale**: Goal Item 4 requires "correct TIMES float-to-int truncation". ERA's TIMES instruction truncates toward zero via int cast, not Math.Round. AC#11 verifies method existence, AC#22 verifies test boundary invocations, but neither verifies the production code uses the correct `(int)(value * multiplier)` truncation pattern. An implementation using `Math.Round` would pass AC#11 and AC#22 while silently diverging from ERA TIMES semantics. AC#63 directly verifies the truncation implementation in production code.

**AC#64: DATUI helpers zero EQUIP indices via SetEquip calls**
- **Test**: Grep pattern=`SetEquip` path="Era.Core/Counter/CounterSourceHandler.cs" | count
- **Expected**: count >= 4 (at least one SetEquip call per DATUI helper: DatUIBottom, DatUIBottomT, DatUITop, DatUITopT)
- **Rationale**: Constraint C3 requires "4 DATUI_* functions must zero EQUIP identically across undressing levels" with "equipment state verification per function". AC#9/AC#10/AC#16/AC#17 verify method visibility (public). AC#56 verifies tests reference the method names. But no AC verified the production code actually calls SetEquip to zero EQUIP indices. An implementation where DATUI methods only delegate to CLOTHES_SETTING_TRAIN without zeroing EQUIP would pass all existing ACs while violating C3.

**AC#65: Case 505 sub-case 2 ungated EXP increment test exists**
- **Test**: Grep pattern=`505.*Masturbation\|Masturbation.*505\|手コキ\|手淫\|SubCase2.*Ungated\|UngatedExp` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Expected**: matches (at least one test references the 505 sub-case 2 ungated EXP increment)
- **Rationale**: Technical Design documents that reaction case 505 sub-case 2 (手コキ) has an unconditional EXP increment without CheckExpUp gate — the only such case. An implementation that incorrectly gates this EXP increment through CheckExpUp passes AC#31 (CheckExpUp exists) and AC#53 (SubAction test references) while violating C1 (branch dispatch equivalence). AC#65 verifies a dedicated test exists for this specific ungated behavior.

**AC#66: Dispatch branch tests read SOURCE/STAIN state for behavioral parity verification**
- **Test**: Grep pattern=`GetSource\|GetStain` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs" | count
- **Expected**: count >= 2 (at least 2 GetSource/GetStain calls verifying SOURCE/STAIN state)
- **Rationale**: Companion to AC#59. While AC#59 ensures Assert.Equal assertions exist, AC#66 ensures the tests actually read SOURCE/STAIN state via GetSource/GetStain. An implementation that asserts non-SOURCE values (e.g., only checks method calls) would pass AC#59 but fail AC#66. Together, AC#59+AC#66 guarantee that dispatch branch tests verify concrete SOURCE/STAIN output values matching ERB behavior.

**AC#67: ExpLvTable.cs contains PalamLv threshold array**
- **Test**: Grep pattern=`PalamLv` path="Era.Core/Counter/ExpLvTable.cs"
- **Expected**: matches (PalamLv array member exists in ExpLvTable)
- **Rationale**: Task#17 adds the PALAMLV threshold array to the existing ExpLvTable.cs static class (created by F812). Without this AC, the PALAMLV addition could be silently skipped while AC#14/AC#15 still pass. Grep match is the simplest verification that ExpLvTable.cs was extended with the PalamLv array.

**AC#68: PainStateChecker references ExpLvTable.PalamLv**
- **Test**: Grep pattern=`ExpLvTable\.PalamLv` path="Era.Core/Character/PainStateChecker.cs"
- **Matcher**: gte
- **Expected**: count >= 2 (GetPalamLv method body must reference ExpLvTable.PalamLv for threshold lookup + overflow path; threshold of 1 could be satisfied by import/comment alone)
- **Rationale**: AC#67 verifies the PalamLv array exists in ExpLvTable, but PainStateChecker could still use its own inline arrays. The pattern specifically targets PalamLv references to distinguish from pre-existing ExpLvTable.Thresholds (EXPLV) references — a broad `ExpLvTable` match could be satisfied by the pre-existing EXPLV access without any PalamLv extraction having occurred. AC#68 uses `ExpLvTable\.PalamLv` gte=2 to ensure PainStateChecker references the extracted PalamLv array in at least two locations (threshold lookup + overflow path), confirming the inline PALAMLV was replaced and is actively used in the logic paths.

**AC#69: CounterSourceHandler references ExpLvTable for EXPLV/PALAMLV**
- **Test**: Grep pattern=`ExpLvTable` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Matcher**: gte
- **Expected**: count >= 2 (one reference for EXPLV thresholds, one for PALAMLV thresholds in PainCheckVMaster)
- **Rationale**: AC#67 verifies ExpLvTable.cs contains PalamLv. AC#68 verifies PainStateChecker references it. But CounterSourceHandler.PainCheckVMaster also needs these threshold arrays (per C15 and Task#13). Without AC#69, PainCheckVMaster could inline its own arrays, defeating the deduplication purpose of Task#17.

**AC#70: Equivalence tests verify EXP state mutations**
- **Test**: Grep pattern=`GetExp\|SetExp` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Matcher**: gte
- **Expected**: count >= 3 (at least 3 EXP state accesses: CheckExpUp-gated path test + ungated path test + state assertion)
- **Rationale**: C1 requires all dispatch branches to produce identical SOURCE/STAIN/EXP mutations. AC#66 verifies GetSource|GetStain in tests (gte=2) but omits GetExp. EXP mutations occur in reaction cases (502-504 via CheckExpUp gating, 505 sub-case 2 ungated) and potentially in the うふふ==2 pre-check block. AC#70 threshold of 3 ensures multiple EXP state assertions across test methods, aligned with AC#66's floor of 2 for SOURCE/STAIN.

**AC#71: All ICounterUtilities stubs include CheckExpUp**
- **Test**: Grep pattern=`CheckExpUp` path="Era.Core.Tests/Counter/" | count
- **Expected**: count >= 7 (5 ICounterUtilities stubs across test files: ActionValidatorTests, ActionSelectorTests, WcCounterReactionTests, WcActionSelectorTests, WcActionValidatorTests; plus at least 2 references in CounterSourceHandlerTests.cs from AC#81 gated-path tests)
- **Rationale**: Task#16 adds CheckExpUp to ICounterUtilities interface. AC#57/AC#58 verify 2 of 5 known stubs. AC#71 provides aggregate coverage ensuring all ICounterUtilities implementations in Counter test files include the new method, preventing compilation failures. The gte=7 threshold accounts for CounterSourceHandlerTests.cs being in the same directory scope (Era.Core.Tests/Counter/) — that file will contain CheckExpUp references from gated-path tests (AC#81), which would inflate a gte=5 threshold and mask a missing stub. The 7 = 5 stubs + 2 minimum CounterSourceHandlerTests.cs references.

**AC#72: HandleCounterSource invokes UpdateMasterCounterControl**
- **Test**: Grep pattern=`UpdateMasterCounterControl\(` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: Pattern matches (call to UpdateMasterCounterControl appears in production code, confirming the MCC block is invoked from the event flow)
- **Rationale**: AC#8 verifies the method exists, AC#32/AC#44 verify the method's content, AC#51 verifies test coverage. But none verify the method is actually CALLED from HandleCounterSource. In COUNTER_SOURCE.ERB, the SETBIT MCC block (lines 637-786) runs after the main dispatch. Without AC#72, an implementation could have a complete but orphaned UpdateMasterCounterControl that is never invoked.

**AC#73: PainStateChecker PALAMLV Removal**
- **Test**: Grep pattern=`private static readonly int\[\] PALAMLV` path="Era.Core/Character/PainStateChecker.cs"
- **Expected**: Pattern does NOT match (zero occurrences — inline PALAMLV array removed by Task#17 ExpLvTable extraction)
- **Rationale**: Task#17 extracts PALAMLV to shared ExpLvTable.cs. AC#68 verifies PainStateChecker references ExpLvTable (gte=2), but does not verify the inline PALAMLV array is removed. Without AC#73, PainStateChecker could reference ExpLvTable while still containing the duplicated inline array.

**AC#74: Reaction case int-range guard exists before CounterActionId cast**
- **Test**: Grep pattern=`>= 500\|500.*601\|IsReactionCase\|isReaction` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Matcher**: matches
- **Expected**: at least 1 match confirming the two-stage routing (int range check before enum cast) documented in Technical Design
- **Rationale**: Technical Design specifies reaction cases (500-601) use literal ints, not CounterActionId enum values (Constraint C10). AC#26 verifies case numbers exist in code, AC#33 verifies HandleReactionCases method exists, but neither verifies the int-range guard that routes values to HandleReactionCases before attempting CounterActionId cast. Without this guard, unknown enum values (500-601) would throw at cast time, breaking reaction case handling.

**AC#75: SetCharacterString called in production CounterSourceHandler**
- **Test**: Grep pattern=`SetCharacterString` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Matcher**: gte
- **Expected**: count >= 19 [I] (19 confirmed CSTR write sites in COUNTER_SOURCE.ERB)
- **Rationale**: Constraint C5 requires 19 CSTR write sites to be representable in C#. AC#12 verifies the ICharacterStringVariables interface exists, AC#29 verifies injection into the constructor, AC#42 verifies test usage. None verify that SetCharacterString is actually invoked in production code. An implementation could inject the interface and never call it, satisfying all three ACs while leaving 19 CSTR sites untranslated. AC#75 closes this gap by requiring substantial SetCharacterString call-site presence in the handler.

**AC#76: TouchSet called in production CounterSourceHandler**
- **Test**: Grep pattern=`\.TouchSet\(` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Matcher**: gte
- **Expected**: count >= 25 [I] (25 confirmed active TOUCH_SET calls in COUNTER_SOURCE.ERB; 3 commented-out excluded)
- **Rationale**: Constraint C14 requires ITouchSet interface gap closure for ~26 call sites. AC#55 verifies ITouchSet injection in the constructor, but not that TouchSet() is actually invoked. Parallel to AC#75 (SetCharacterString call-site coverage). Without AC#76, an implementation could inject ITouchSet and never call it.
- **Precision Note**: The Grep searches the entire CounterSourceHandler.cs file, including private helper methods. The gte=25 threshold assumes dispatch branches call `.TouchSet()` directly or via at most one level of helper extraction. If an implementation groups all 26 call sites into a single private helper, only 1 `\.TouchSet\(` match would exist. The threshold of 25 ensures meaningful distribution of call sites across dispatch branches. Same pattern applies to AC#75 (SetCharacterString gte=19 [I]).

**AC#77: MCC test invocations cover multiple CASE groups**
- **Test**: Grep pattern=`UpdateMasterCounterControl\|MasterCounter.*Assert\|Assert.*MasterCounter` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Matcher**: gte
- **Expected**: count >= 5 (minimum 5 references combining MCC method invocations and bitmask output assertions across multiple test methods, ensuring at least 2-3 distinct CASE groups are tested with actual value assertions)
- **Rationale**: Goal Item 2 requires 'SETBIT master counter control block translated to C# bitwise operations' with ~30 CASE branches. AC#25 only verifies test name presence (matches), AC#51 counts MasterCounterControl references >= 3 which can be satisfied by a single test setup+assertion. Neither ensures Assert.Equal on bitmask output values from multiple CASE inputs. AC#77 combines UpdateMasterCounterControl invocations with MasterCounter Assert patterns to enforce both: (a) multiple MCC test calls to different CASE inputs, and (b) actual assertion on bitmask output values.

**AC#78: UpdateMasterCounterControl writes TCVAR for both MASTER and ARG**
- **Test**: Grep pattern=`SetTCVar.*MasterCounterControl\|MasterCounterControl.*SetTCVar` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Matcher**: gte
- **Expected**: count >= 2 (the file must contain both SetTCVar-to-MASTER and SetTCVar-to-ARG patterns across different CASE branches)
- **Rationale**: The ERB MCC block has character-specific CASE branches — some write TCVAR:MASTER:マスターカウンター制御, others write TCVAR:ARG:マスターカウンター制御. No single CASE writes both. AC#78 verifies the file contains at least two SetTCVar+MasterCounterControl references across different CASE branches. Without AC#78, an implementation updating only one character's TCVAR passes all other ACs while diverging from ERB behavior.
- **Precision Note**: AC#78's matcher (`SetTCVar.*MasterCounterControl`) counts total references and cannot distinguish MASTER writes from ARG writes — both use the same `TCVarIndex.MasterCounterControl` constant. An implementation with two MASTER-only writes satisfies gte=2 without any ARG writes. This is a known limitation. **Combined constraint**: Task#11 explicitly documents character-specific writes per ERB source, and AC#25 (MCC test coverage) verifies the test exercises both MASTER and ARG CASE branches at runtime. AC#78 functions as a structural floor, not a standalone guarantee.

**AC#79: CSTR behavioral test asserts accumulated string via GetCharacterString**
- **Test**: Grep pattern=`GetCharacterString.*Assert\|Assert.*GetCharacterString\|Equal.*GetCharacterString` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Matcher**: gte
- **Expected**: count >= 2 (at least 2 assertion-adjacent GetCharacterString calls covering both CSTR patterns)
- **Rationale**: AC#42 verifies SetCharacterString is called in tests (gte=3), but SetCharacterString calls could be test setup (arrangement) without any assertion on the accumulated result. AC#79 ensures the test actually reads back the string via GetCharacterString AND asserts the value (the matcher requires GetCharacterString co-located with Assert/Equal). This prevents stub class boilerplate (interface method implementations) from inflating the count. Goal Item 6 requires "CSTR string accumulation strategy resolved" — write-only testing (AC#42) without assertion-backed read-back verification (AC#79) leaves the append semantics unverified. The gte=2 threshold ensures at least two assertion-adjacent read-backs covering both CSTR patterns (player accumulates target ID, target accumulates player ID).

**AC#80: HandleReactionCases is called from HandleCounterSource (call-chain verification)**
- **Test**: Grep pattern=`HandleReactionCases\(` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Matcher**: matches
- **Expected**: Pattern matches at least once
- **Rationale**: AC#33 verifies HandleReactionCases method EXISTS (declaration) but not that it is CALLED from HandleCounterSource. Without AC#80, an implementation could define a complete HandleReactionCases method but route reaction cases inline within HandleCounterSource, violating the Technical Design's structural separation (Key Decision D: dedicated HandleReactionCases method). This mirrors AC#72 which verifies UpdateMasterCounterControl is called from HandleCounterSource. The `\(` suffix ensures the match targets a call expression (with opening parenthesis), not just the method name in a declaration or comment.

**AC#81: CheckExpUp gated path: EXP not incremented when CheckExpUp returns false**
- **Test**: Grep pattern=`CheckExpUp.*false|ExpUp.*Skip|!.*CheckExpUp|ExpGated` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Matcher**: gte
- **Expected**: count >= 2 (at least 2 references to CheckExpUp gated-path testing, covering the conditional gate semantics for reaction cases 502-504)
- **Rationale**: Technical Constraints document that `SIF !EXP_UP(index, ARG)` gates EXP increment in reaction cases 502-504. AC#31 verifies CheckExpUp is called in production code, but an implementation that calls CheckExpUp and ignores the return value (always incrementing EXP) passes AC#31 while violating behavioral equivalence. AC#81 verifies the test file contains references to the gated path (CheckExpUp returning false → EXP not incremented), ensuring the conditional semantics are tested. The gte=2 threshold ensures at least two keyword references (e.g., test method name + stub configuration), confirming the gated path is exercised.
- **Precision Note**: AC#81's matcher operates file-wide without co-location constraint against HandleReactionCases or HandleCounterSource invocation. A test that configures `CheckExpUp.*false` in an unrelated helper method (not exercising the reaction case path) would satisfy AC#81 while leaving the gated path untested. This is mitigated by: (1) AC#80 verifies HandleReactionCases is called from HandleCounterSource, (2) AC#53 requires SubAction references gte=10 covering reaction case test infrastructure, (3) the test file is scoped to CounterSourceHandlerTests.cs where CheckExpUp references are overwhelmingly likely to be in reaction-case test context. A multiline co-location matcher was considered but rejected: the gated-path test likely configures the stub (CheckExpUp returns false) in test setup and asserts the result in a separate block, exceeding practical co-location window sizes.

**AC#82: IEngineVariables MASTER resolution behavioral test exists**
- **Test**: Grep pattern=`GetMaster|MasterCharacter|StubEngineVariables.*[Mm]aster|engineVariables.*[Mm]aster` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Matcher**: gte
- **Expected**: count >= 2 (at least 2 references to MASTER character resolution in test setup/assertions)
- **Rationale**: Goal Item 7 requires all external calls behind injectable interfaces. IEngineVariables is injected (AC#35 verifies structural injection). AC#82 verifies that tests actually exercise the MASTER character resolution via IEngineVariables — the 7 global TCVAR:行為者=ARG sites need GetMaster() to resolve the MASTER character. Without this, an implementation could inject IEngineVariables but resolve MASTER via hardcoded literal, satisfying AC#35 while producing incorrect character-scoped TCVAR writes. The gte=2 threshold ensures at least two references (e.g., stub configuration + assertion on MASTER character output).

**AC#83: New F803 files do not reference ERB interpreter types**
- **Test**: Grep pattern=`EmuEra|GlobalStatic|Interpreter|ErbScript` path="Era.Core/Counter/"
- **Expected**: Pattern NOT found (not_contains) across all files in Era.Core/Counter/
- **Rationale**: Philosophy claims "testable, maintainable game logic independent of the legacy ERB interpreter." Verifies no direct references to engine-layer static types leak into the domain model or its interfaces. Era.Core must depend only on its own interfaces, never on the EmuEra engine layer. Directory-scoped to cover CounterSourceHandler.cs, ICounterSourceHandler.cs, ITouchSet.cs, and other new F803 files.

**AC#84: ICharacterStringVariables.cs does not reference ERB interpreter types**
- **Test**: Grep pattern=`EmuEra|GlobalStatic|Interpreter|ErbScript` path="Era.Core/Interfaces/ICharacterStringVariables.cs"
- **Expected**: Pattern NOT found (not_contains)
- **Rationale**: ICharacterStringVariables.cs lives in Era.Core/Interfaces/ (outside Era.Core/Counter/ where AC#83 checks). Same Philosophy-derived constraint applies: domain interfaces must not reference the ERB interpreter layer.

**AC#85: うふふ==2 STAIN side-effects are asserted in tests**
- **Test**: Grep pattern=`Ufufu.*Stain|Stain.*Ufufu|うふふ.*GetStain|GetStain.*うふふ` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Expected**: count >= 1
- **Rationale**: Constraint C6 requires preservation of the うふふ==2 pre-SELECTCASE block which modifies STAIN before dispatch (COUNTER_SOURCE.ERB lines 8-26). AC#41 verifies test keyword presence and AC#45 verifies STAIN references, but neither enforces that the うふふ==2 path's STAIN output is specifically asserted. This co-location matcher ensures at least one test verifies STAIN mutations in the context of the うふふ==2 special path.

**AC#86: UpdateShrinkage called in production CounterSourceHandler**
- **Test**: Grep pattern=`\.UpdateShrinkage\(` path="Era.Core/Counter/CounterSourceHandler.cs"
- **Expected**: count >= 8 [I] (8 confirmed 締り具合変動 call sites in COUNTER_SOURCE.ERB; [I] tag: implementer must verify exact count)
- **Rationale**: Goal Item 7 requires "all external calls behind injectable interfaces." AC#30 verifies IShrinkageSystem is injected into CounterSourceHandler constructor. AC#86 verifies the 8 ERB call sites are actually translated to UpdateShrinkage() calls in production code. Follows the same pattern as AC#75 (SetCharacterString gte=19) and AC#76 (TouchSet gte=25).

**AC#87: うふふ==2 test sets CFLAG precondition co-located with HandleCounterSource invocation**
- **Test**: Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs, multiline=true) for CFLAG 317 setup pattern within 800 chars of HandleCounterSource call
- **Expected**: At least one multiline match showing `CharacterFlagIndex(317)` or `CFlagUfufu` setup and HandleCounterSource invocation co-located within the same test method
- **Rationale**: Constraint C6 requires CFLAG:ARG:うふふ==2 pre-SELECTCASE block to be preserved. AC#85 verifies STAIN side-effects are asserted alongside "Ufufu", but without co-location enforcement, CFLAG setup could match incidentally in a stub or unrelated test method. The multiline 800-char window ensures the CFLAG precondition and handler invocation appear in the same test method scope, enforcing the ARRANGE→ACT sequence required to exercise the C6 guard path. Matcher uses raw int 317 (= うふふ CFLAG index) matching codebase pattern: `private const int CFlagUfufu = 317` (ActionSelector/ComableChecker precedent).
- **Precision Note**: The 800-char window is sufficient for a typical test method (ARRANGE: ~200 chars stub setup + CFLAG set, ACT: ~100 chars handler call, total ~300-500 chars). If the test method exceeds 800 chars between CFLAG setup and handler call, the implementer should restructure the test for readability.

**AC#88: Case 502 (手淫) reaction behavioral spot-check test exists**
- **Test**: Grep pattern=`Case502|Reaction.*502|ReactionCase.*502|_502_|502.*手淫|手淫.*502` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Expected**: matches (at least one test references case 502 by number or name)
- **Rationale**: AC#53 verifies aggregate SubAction references (gte=10) but can be satisfied by heavy testing of cases 500 and 505 alone, leaving case 502 (手淫) untested. Case 502 has CheckExpUp-gated EXP increment (COUNTER_SOURCE.ERB:562) and distinct SOURCE/CSTR mutations per SubAction sub-case. AC#88 ensures at least one test method explicitly references case 502 by number or identifying name, closing the individual branch coverage gap.

**AC#89: Case 503 (フェラチオ) reaction behavioral spot-check test exists**
- **Test**: Grep pattern=`Case503|Reaction.*503|ReactionCase.*503|_503_|503.*Fellatio|Fellatio.*503` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Expected**: matches (at least one test references case 503 by number or name)
- **Rationale**: Same gap as AC#88. Case 503 (フェラチオ) has CheckExpUp-gated EXP increment (COUNTER_SOURCE.ERB:574) and distinct SOURCE mutations (SERVICE += 300 in sub-case 2). AC#89 ensures at least one test method explicitly references case 503.

**AC#90: Case 504 (パイズリ) reaction behavioral spot-check test exists**
- **Test**: Grep pattern=`Case504|Reaction.*504|ReactionCase.*504|_504_|504.*Paizuri|Paizuri.*504` path="Era.Core.Tests/Counter/CounterSourceHandlerTests.cs"
- **Expected**: matches (at least one test references case 504 by number or name)
- **Rationale**: Same gap as AC#88/AC#89. Case 504 (パイズリ) has CheckExpUp-gated EXP increment (COUNTER_SOURCE.ERB:590) and distinct SOURCE mutations. AC#90 ensures at least one test method explicitly references case 504.

**AC#91: うふふ==2 test asserts TCVAR or EXP mutation co-located with Ufufu context**
- **Test**: Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs, multiline=true) pattern=`(Ufufu|うふふ|CFlagUfufu)[\s\S]{0,1200}(EjaculationLocationFlag|EjaculationPleasureIntensity|VSexExp|GetExp|SetTCVar)|(EjaculationLocationFlag|EjaculationPleasureIntensity|VSexExp|GetExp|SetTCVar)[\s\S]{0,1200}(Ufufu|うふふ|CFlagUfufu)`
- **Expected**: matches (at least one multiline match showing Ufufu context keyword within 1200 chars of a TCVAR or EXP assertion keyword)
- **Rationale**: The うふふ==2 pre-SELECTCASE block (COUNTER_SOURCE.ERB:8-26) performs TCVAR mutations (EjaculationLocationFlag=1, EjaculationPleasureIntensity+=400) and EXP increment (Ｖ性交経験++). AC#85 verifies STAIN co-location with Ufufu. AC#87 verifies CFLAG precondition co-location. But no AC verifies TCVAR/EXP mutations are tested within the Ufufu context. AC#70 (GetExp|SetExp gte=3) is file-wide and can be satisfied by non-Ufufu tests. AC#91 closes this gap by requiring TCVAR/EXP assertion keywords to appear within 1200 chars of Ufufu context keywords.
- **Precision Note**: The 1200-char window accommodates a typical test method with ARRANGE (stub setup ~300 chars + CFLAG set ~100 chars), ACT (HandleCounterSource call ~100 chars), and ASSERT (multiple assertion lines ~400 chars). The bidirectional pattern (Ufufu before/after TCVAR/EXP) handles both assertion-first and setup-first test structures.

**AC#92: うふふ==2 test asserts SOURCE mutation co-located with Ufufu context**
- **Test**: Grep(Era.Core.Tests/Counter/CounterSourceHandlerTests.cs, multiline=true) pattern=`(Ufufu|うふふ|CFlagUfufu)[\s\S]{0,1200}(GetSource|SetSource|SourceIndex)|(GetSource|SetSource|SourceIndex)[\s\S]{0,1200}(Ufufu|うふふ|CFlagUfufu)`
- **Expected**: matches (at least one multiline match showing Ufufu context keyword within 1200 chars of a SOURCE assertion keyword)
- **Rationale**: The うふふ==2 pre-SELECTCASE block modifies 6 SOURCE values (SexualActivity, Conquest, Liquid, Sadism, Coercion, GivePleasureV). AC#50 (SetSource|GetSource gte=20) is file-wide and can be satisfied entirely by dispatch branch tests without any Ufufu-path SOURCE assertions. AC#92 closes this gap by requiring SOURCE assertion keywords (GetSource|SetSource|SourceIndex) to appear within 1200 chars of Ufufu context keywords. This completes the co-location coverage: AC#85 (STAIN), AC#87 (CFLAG precondition), AC#91 (TCVAR/EXP), AC#92 (SOURCE).
- **Precision Note**: Same 1200-char window rationale as AC#91. The bidirectional pattern handles both assertion-first and setup-first test structures.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Migrate COUNTER_SOURCE.ERB to CounterSourceHandler class in Era.Core/Counter/ with all ~36 dispatch branches | AC#1, AC#2, AC#6, AC#20, AC#26, AC#33, AC#40, AC#49, AC#52, AC#74, AC#80 |
| 2 | SETBIT master counter control block translated to C# bitwise operations | AC#8, AC#32, AC#44, AC#51, AC#72, AC#77, AC#78 |
| 3 | DATUI_* undressing helpers as shared public methods | AC#9, AC#10, AC#16, AC#17, AC#47, AC#56, AC#64 |
| 4 | PAIN_CHECK_V_MASTER with correct TIMES float-to-int truncation | AC#11, AC#22, AC#63, AC#67, AC#68, AC#69, AC#73 |
| 5 | SourceIndex/StainIndex/ExpIndex/TCVarIndex constants present (SourceIndex: regression guard; others: additive) | AC#3, AC#4, AC#5, AC#7, AC#23, AC#43, AC#48 |
| 6 | CSTR string accumulation interface strategy resolved (runtime storage deferred to F813) | AC#12, AC#29, AC#42, AC#75, AC#79 |
| 7 | All external calls behind injectable interfaces | AC#13, AC#18, AC#19, AC#21, AC#27, AC#28, AC#29, AC#30, AC#31, AC#34, AC#35, AC#36, AC#37, AC#38, AC#39, AC#55, AC#76, AC#86 |
| 8 | Equivalence tests verifying behavioral parity via category-representative tests (6 dispatch ranges covering all ~36 branches); build succeeds. Note: shrinkage behavioral parity (8 UpdateShrinkage call sites) is deferred to F813 — tests verify call-site existence only (AC#86), not runtime parity | AC#14, AC#15, AC#24, AC#25, AC#41, AC#45, AC#46, AC#50, AC#53, AC#54, AC#57, AC#58, AC#59, AC#60, AC#61, AC#62, AC#65, AC#66, AC#70, AC#71, AC#81, AC#82, AC#83, AC#84, AC#85, AC#87, AC#88, AC#89, AC#90, AC#91, AC#92 |

---

## Technical Design

### Approach

CounterSourceHandler is implemented as a sealed class in `Era.Core/Counter/` following the same constructor-injection pattern as ActionSelector (F801). The class implements a new `ICounterSourceHandler` interface exposing a single `HandleCounterSource(CharacterId target, int arg1)` entry point, plus public helper methods for DATUI_* and PainCheckVMaster that F805 will consume.

**CSTR Strategy**: CSTR:x:10 (ejaculation-partner tracking) is a character-scoped string variable not covered by IVariableStore or IStringVariables. The chosen strategy is to add two methods to a new `ICharacterStringVariables` interface segregated per ISP: `string GetCharacterString(CharacterId character, CstrIndex index)` and `void SetCharacterString(CharacterId character, CstrIndex index, string value)`. Two CSTR access patterns exist: (1) **14 sites**: `CSTR:PLAYER:10 += {ARG}/` → `_charStrings.SetCharacterString(player, 10, _charStrings.GetCharacterString(player, 10) + $"{target.Value}/")` (player accumulates target's ID); (2) **5 sites** (Ｖセックス, 正常位, 後背位, 背面座位, 騎乗位逆レイプ): `CSTR:ARG:10 += {PLAYER}/` → `_charStrings.SetCharacterString(target, 10, _charStrings.GetCharacterString(target, 10) + $"{player.Value}/")` (target accumulates player's ID). Both patterns use the same ICharacterStringVariables interface. This avoids mutating the bloated IVariableStore interface.

**TOUCH_SET Strategy**: TOUCH_SET is defined in F811 scope (SOURCE_POSE.ERB). Rather than creating a new ITouchSet interface in F803, a `ITouchSet` interface is added in `Era.Core/Counter/` with a single `void TouchSet(int mode, int type, CharacterId target)` method, injected into CounterSourceHandler. This breaks the circular dependency: F803 depends on the interface (defined here), F811 implements it.

**Reaction case routing**: The ERB entry point `@EVENT_COUNTER_SOURCE` uses `SELECTCASE TCVAR:ARG:カウンター行動` (a single integer SELECTCASE). Both CounterActionId enum values and reaction case ints (500-601) appear as CASE branches in the same SELECTCASE block. In C#, `HandleCounterSource` receives the counter action value as an int (via `variables.GetTCVar(target, TCVarIndex.CounterAction)`). The dispatch logic first checks if the value is in the reaction case range (500-601) and routes to `HandleReactionCases(int reactionCaseId, ...)`, then casts to `(CounterActionId)` for the main enum switch. This two-stage routing allows AC#26 (reaction case int literals) and AC#49 (CounterActionId enum references) to coexist: the int check catches 500-601 before the enum cast, preventing unknown enum values from reaching the default throw arm. AC#20's exhaustive switch with throwing default applies to the CounterActionId switch (non-reaction cases only).

**Global-scope SOURCE writes**: `SOURCE:征服 += 100` (line 300, no character prefix) and `SOURCE:快Ｖ = 500` (line 524) resolve to the TARGET character in ERA context (the default CHARA context in event handlers). Map to `variables.SetSource(target, SourceIndex.Conquest, ...)` and `variables.SetSource(target, SourceIndex.GivePleasureV, 500)` respectively.

**Global-scope TCVAR writes**: `TCVAR:行為者 = ARG` (7 occurrences: lines 81, 295, 405, 432, 459, 488, 515) writes to a MASTER-scoped TCVAR in ERA (unqualified TCVAR defaults to MASTER context). Map to `variables.SetTCVar(master, TCVarIndex.Actor, target.Value)` where `master` is resolved from IEngineVariables.

**CFLAG reads/writes**: `CFLAG:MASTER:ローターA挿入` (line 70) and `CFLAG:ARG:ぱんつ確認` (lines 124, 322, 366) use raw integer indices via `variables.GetCharacterFlag(character, index)` / `SetCharacterFlag`. Define private int constants within CounterSourceHandler (matching ActionSelector's raw-index pattern).

**BASE/MAXBASE access**: `BASE:PLAYER:射精` and `MAXBASE:PLAYER:射精` (line 68, 乳搾り手コキ case) use raw integer indices via `variables.GetBase(player, index)` / `SetBase` and `GetMaxBase`. Define private int constant within CounterSourceHandler.

**PainCheckVMaster**: Replicates PAIN_CHECK_V_MASTER (lines 893-920) directly in CounterSourceHandler using `(int)(value * multiplier)` for TIMES truncation. PALAMLV and EXPLV threshold arrays are stored in the shared `Era.Core/Counter/ExpLvTable.cs` static class (created by F812, extended by F803 Task#17 with PalamLv array) that both `PainStateChecker` (`Era.Core/Character/PainStateChecker.cs`) and `CounterSourceHandler` reference, eliminating duplication. IPainStateChecker covers PAIN_CHECK_V (SOURCE:苦痛/反感) -- PainCheckVMaster is a distinct operation modifying SOURCE:加虐 only, so it is NOT delegated to IPainStateChecker.

**SETBIT Translation**: ERA `SETBIT var, bit` ≡ C# `var |= (1 << bit)`. 7 MCC_ named constants exist in `Era.Core/Common/Constants.cs` (lines 339-345, bits 0-6). For these 7, use `currentValue |= (1 << Constants.MCC_X)` which expands to `1 <<` in source. For the remaining ~50 SETBIT operations (bits 7+), use literal `1 << bitIndex` directly in UpdateMasterCounterControl. This ensures AC#44's `1 <<` count threshold (>= 57 [I]) is satisfiable — do NOT define additional named MCC_ constants in Constants.cs for F803 scope. TCVAR:MASTER:マスターカウンター制御 and TCVAR:ARG:マスターカウンター制御 are written via `variables.SetTCVar(character, TCVarIndex.MasterCounterControl, currentValue | (1 << bit))` using the typed constant from Task#6/AC#23. **CRITICAL**: MCC CASE branches are character-specific — some write only to MASTER (`variables.SetTCVar(master, ...)`), others write only to ARG/target (`variables.SetTCVar(target, ...)`). Do NOT write both characters per case. The ERB source (lines 637-786) specifies which character each CASE branch targets.

**EQUIP indices**: ClothingSystem.cs already documents the integer indices (5=下半身下着１, 6=下半身下着２, 7=下半身上着１, 8=下半身上着２, 9=スカート, 10=上半身下着１, 11=上半身下着２, 12=上半身上着１, 13=上半身上着２, 14=ボディースーツ, 15=ワンピース, 16=着物, 17=レオタード). DATUI helpers use these int indices directly via IVariableStore.GetEquip/SetEquip -- no separate EquipIndex type required at this stage (additive safety via named constants inside CounterSourceHandler).

**DATUI/PainCheckVMaster placement rationale**: These helpers remain in CounterSourceHandler rather than a separate class because: (1) the ERB defines them within COUNTER_SOURCE.ERB scope, (2) F805 injects ICounterSourceHandler interface per DIP (not the concrete CounterSourceHandler type), and (3) extraction would require a new interface and class for 5 methods that are tightly coupled to the counter source context (EQUIP manipulation, IClothingSystem calls). The methods are exposed on ICounterSourceHandler so F805 depends on the abstraction, not the implementation. Note: ICounterSourceHandler exposes 3 responsibilities (dispatch, undressing, pain check) on a single interface — this is a known ISP trade-off. See pending review item for potential IUndressingHandler extraction.

**CLOTHES_SETTING_TRAIN**: Called from all 4 DATUI helpers (lines 816, 845, 868, 890). Maps to `clothingSystem.SettingTrain(target.Value, getEquipFunc, setEquipFunc)` via IClothingSystem. The callback-based signature requires passing IVariableStore GetEquip/SetEquip as lambdas.

**Virginity calls**: IVirginityManager exists at `Era.Core/Character/IVirginityManager.cs` with `CheckLostVirginity(target, partner, VirginityType)`. LOST_VIRGIN_M (kiss virginity) and LOST_VIRGIN_A (anal virginity) map to VirginityType.Kiss and VirginityType.Anal respectively. LOST_VIRGIN (vaginal virginity) maps to VirginityType.Vaginal.

**締り具合変動 (ShrinkageVariation)**: This external function call (8 sites in ERB) does not have a C# interface yet. It must be added to a new `IShrinkageSystem` interface in `Era.Core/Counter/` injected into CounterSourceHandler.

**ONCE calls**: `ICounterUtilities.IsOnce(key, limit, offender)` already exists (from F801). Maps to `CALLF ONCE(key, limit, ARG)` calls.

**EXP_UP calls**: `EXP_UP(expIndex, character)` is called at 3 sites in reaction cases (502-504) to gate experience increments. Map to `ICounterUtilities.CheckExpUp(CharacterId character, int expIndex)` (add method to existing interface since it's a utility check function similar to IsOnce).

**Case 505 sub-case 2 exception**: Reaction case 505, sub-case 2 (手コキ) contains an unconditional `EXP:ARG:手淫経験 ++` (no `EXP_UP` gate). This maps to a direct `variables.SetExp(target, ExpIndex.MasturbationExp, variables.GetExp(target, ExpIndex.MasturbationExp) + 1)` without calling `CheckExpUp`. This is the only reaction case sub-case with an ungated EXP increment.

**Reaction cases 600-601 (non-SubAction):**

Unlike cases 500, 502-505, cases 600-601 do NOT use nested SubAction sub-dispatch. Their behavior is simpler:
- **CASE 600 (玉揉み)**: TCVAR:PLAYER:EjaculationPleasureIntensity += 200, SOURCE:ARG: Seduction/Service/Provocation += 200, Conquest += 300, conditional Sadism += 100
- **CASE 601 (アナル愛撫)**: TCVAR:PLAYER:EjaculationPleasureIntensity += 100, SOURCE:ARG: Humiliation += 300, Sadism += 100, GivePleasureA += 300

These cases are verified by AC#26 (case number existence) and AC#62 (behavioral test coverage) but have no nested sub-dispatch ACs (AC#52/AC#53 apply only to SubAction cases).

**HAS_PENIS/HAS_VAGINA**: These are already covered by `ICommonFunctions.HasPenis(genderValue)` and `HasVagina(genderValue)`. The gender value is obtained via `variables.GetTalent(character, TalentIndex.Gender)`.

**TEQUIP writes**: `TEQUIP:PLAYER:Ｖセックス = ARG` (line 424) and `TEQUIP:ARG:Ｖセックス = PLAYER` (line 535) record sexual activity partners. Map to `tequip.SetTEquip(player, EquipmentIndex.VSex, target.Value)` and `tequip.SetTEquip(target, EquipmentIndex.VSex, player.Value)` respectively. ITEquipVariables is already injected. Note: `EquipmentIndex` is a static class with int constants in `Era.Core/Types/EquipmentIndex.cs` (not a typed struct).

**PAIN_CHECK_V call (line 525)**: The CNT_騎乗位逆レイプ branch calls PAIN_CHECK_V(TARGET) (modifying SOURCE:苦痛/反感 of the TARGET character), which is covered by the existing `IPainStateChecker.CheckPain()`. CounterSourceHandler injects IPainStateChecker for this single call site.

This approach satisfies all 92 ACs: type-system additions (AC#3, AC#4, AC#5, AC#7) are additive-only changes to existing files; new files (ICounterSourceHandler, CounterSourceHandler) provide AC#1, AC#2; the implementation body provides AC#6, AC#8, AC#9, AC#10, AC#11; the new ICharacterStringVariables interface provides AC#12; constructor injection provides AC#13, AC#21, AC#29 (ICharacterStringVariables injection provides AC#29), AC#30 (IShrinkageSystem injection provides AC#30); clean compilation provides AC#14; equivalence unit tests provide AC#15, AC#22; public DATUI_*T methods provide AC#16, AC#17; new ITouchSet and IShrinkageSystem interfaces provide AC#18, AC#19; exhaustive switch default arm provides AC#20; TCVarIndex MasterCounterControl constant provides AC#23; dispatch branch tests provide AC#24; MCC bitfield tests provide AC#25; reaction case handling provides AC#26; random number generation provides AC#27; ICounterUtilities.CheckExpUp provides AC#28; CheckExpUp call-site translation provides AC#31; bitwise OR assignment provides AC#32; HandleReactionCases method provides AC#33; remaining 5 DI injection verifications provide AC#34-AC#38; ICounterUtilities injection provides AC#39; pre-check guard provides AC#40-AC#41; CSTR behavioral test provides AC#42; TCVarIndex count verification provides AC#43; concrete output state assertions provide AC#46; ICounterSourceHandler interface method signatures provide AC#47; ExpIndex count verification provides AC#48; SOURCE accessor call count provides AC#50; MasterCounterControl reference count provides AC#51; SubAction nested sub-dispatch in production code provides AC#52; SubAction sub-dispatch test coverage provides AC#53; method invocation verification in tests provides AC#54; ITouchSet constructor injection provides AC#55; DATUI helper per-variant test coverage provides AC#56; TestCounterUtilities CheckExpUp stub provides AC#57; SelectorTestCounterUtils CheckExpUp stub provides AC#58; Assert.Equal assertions in tests provide AC#59; GetSource/GetStain state reads in tests provide AC#66; reaction cases 600-601 behavioral test coverage provides AC#62; PainCheckVMaster int-cast truncation pattern in production code provides AC#63; DATUI SetEquip zeroing in production code provides AC#64; case 505 sub-case 2 ungated EXP increment test provides AC#65; ExpLvTable.cs PalamLv array existence provides AC#67; PainStateChecker ExpLvTable reference provides AC#68; CounterSourceHandler ExpLvTable reference provides AC#69; int-range guard (500-601 check) before CounterActionId enum cast provides AC#74. Additionally: MCC `1 <<` count (gte=57 [I]) provides AC#44; STAIN bitwise OR merge test provides AC#45; dispatch branch count (CounterActionId references gte=36) provides AC#49; no TODO/FIXME/HACK markers in CounterSourceHandler.cs provides AC#60; no TODO/FIXME/HACK in ICounterSourceHandler.cs provides AC#61; EXP state mutation tests (GetExp/SetExp gte=3) provide AC#70; aggregate ICounterUtilities CheckExpUp stubs (gte=5) provide AC#71; HandleCounterSource calling UpdateMasterCounterControl provides AC#72; PainStateChecker inline PALAMLV removal provides AC#73; SetCharacterString call-site count (gte=19 [I]) provides AC#75; TouchSet call-site count (gte=25 [I]) provides AC#76; MCC test invocation+assertion count (gte=5) provides AC#77; dual-character TCVAR write in UpdateMasterCounterControl (gte=2) provides AC#78; CSTR GetCharacterString read-back assertion in tests (gte=2) provides AC#79; HandleReactionCases call from HandleCounterSource provides AC#80; CheckExpUp gated-path test (EXP not incremented when false, gte=2) provides AC#81; IEngineVariables MASTER resolution behavioral test (gte=2) provides AC#82; no ERB interpreter type references in Era.Core/Counter/ provides AC#83; no ERB interpreter type references in ICharacterStringVariables.cs provides AC#84; うふふ==2 STAIN co-location assertion in tests provides AC#85; UpdateShrinkage call-site count in CounterSourceHandler (gte=8 [I]) provides AC#86; case 502 reaction spot-check provides AC#88; case 503 reaction spot-check provides AC#89; case 504 reaction spot-check provides AC#90; うふふ==2 TCVAR/EXP co-location provides AC#91; うふふ==2 SOURCE co-location provides AC#92.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core/Counter/ICounterSourceHandler.cs` with `void HandleCounterSource(CharacterId target, int arg1)` |
| 2 | Create `Era.Core/Counter/CounterSourceHandler.cs`: `public sealed class CounterSourceHandler : ICounterSourceHandler` |
| 3 | Add `public static readonly SourceIndex Seduction = new(50)` to SourceIndex.cs with the full set of 12 new constants |
| 4 | Add 8 named constants to StainIndex.cs: Mouth=0, Hand=1, Penile=2, Vaginal=3, Anal=4, Breast=5, InVagina=6, InIntestine=7 |
| 5 | Add `public static readonly ExpIndex VSexExp = new(20)` plus ASexExp, MasturbationExp, PaizuriExp, SadisticExp, KissExp to ExpIndex.cs (VExp/AExp2 already exist from F812; KissExp=27 per CSV) |
| 6 | Implement all CounterActionId dispatch cases in `HandleCounterSource` switch expression, including `CounterActionId.SeductiveGesture` |
| 7 | Add all 12 missing SourceIndex constants (Liquid=9, SexualActivity=11, GivePleasureC=40..B=43, Seduction=50..Sadism=55); baseline 19 → 31 total |
| 8 | Implement `UpdateMasterCounterControl` method in CounterSourceHandler: translate each SETBIT to `currentValue |= (1 << bitIndex)`, write back via SetTCVar |
| 9 | Declare `public void DatUIBottom(CharacterId target, CharacterId player, int level)` in CounterSourceHandler |
| 10 | Declare `public void DatUITop(CharacterId target, CharacterId player, int level)` in CounterSourceHandler |
| 11 | Declare `public void PainCheckVMaster(CharacterId target, CharacterId player)` in CounterSourceHandler |
| 12 | Create `Era.Core/Interfaces/ICharacterStringVariables.cs` with `GetCharacterString(CharacterId, CstrIndex)` and `SetCharacterString(CharacterId, CstrIndex, string)` methods; inject into CounterSourceHandler |
| 13 | CounterSourceHandler primary constructor includes `IVariableStore variables` parameter used in body |
| 14 | New files must compile without nullable warnings, unused-variable warnings, or CS8618; all existing tests must remain green |
| 15 | Create `Era.Core.Tests/Counter/CounterSourceHandlerTests.cs` with equivalence tests for representative dispatch branches, DATUI helpers, PainCheckVMaster boundary values, and MCC bitfield correctness |
| 16 | Declare `public void DatUIBottomT(CharacterId target, CharacterId player, int level)` in CounterSourceHandler for target character bottom undressing |
| 17 | Declare `public void DatUITopT(CharacterId target, CharacterId player, int level)` in CounterSourceHandler for target character top undressing |
| 18 | Create `Era.Core/Counter/ITouchSet.cs` with `void TouchSet(int mode, int type, CharacterId target)` for TOUCH_SET abstraction |
| 19 | Create `Era.Core/Counter/IShrinkageSystem.cs` with `void UpdateShrinkage(CharacterId character, int amount, int type)` for 締り具合変動 abstraction |
| 20 | Implement HandleCounterSource with exhaustive switch on CounterActionId, adding a throwing default/discard arm for unhandled values |
| 21 | Ensure CounterSourceHandler primary constructor includes IVirginityManager parameter alongside all other DI interfaces |
| 22 | Write unit test methods in CounterSourceHandlerTests.cs that exercise PainCheckVMaster with boundary TIMES multiplier values |
| 23 | Add `public static readonly TCVarIndex MasterCounterControl = new(26)` (verify value against DIM.ERH) to TCVarIndex.cs |
| 24 | Write test methods in CounterSourceHandlerTests.cs referencing all 6 CounterActionId enum members with confirmed COUNTER_SOURCE.ERB dispatch branches: CounterActionId.SeductiveGesture (10-16), CounterActionId.MilkHandjob (21-29), CounterActionId.Kiss (30-35), CounterActionId.Masturbation (50-60), CounterActionId.ForcedCunnilingus (70-75), CounterActionId.MissionaryInsert (80-91). Matcher uses fully-qualified `CounterActionId.X` format (gte=6). |
| 25 | Write test methods in CounterSourceHandlerTests.cs verifying MCC/UpdateMasterCounterControl bitfield correctness |
| 26 | Implement reaction case handling (500, 502-505, 600-601) in HandleCounterSource using nested int switch per Technical Design Key Decision |
| 27 | Ensure CounterSourceHandler primary constructor includes IRandomProvider parameter for RAND:N translations |
| 28 | Add `bool CheckExpUp(CharacterId character, int expIndex)` method to ICounterUtilities interface |
| 29 | Ensure CounterSourceHandler primary constructor includes ICharacterStringVariables parameter (AC#12 interface injected per CSTR strategy) |
| 30 | Ensure CounterSourceHandler primary constructor includes IShrinkageSystem parameter for 締り具合変動 calls |
| 31 | Call counterUtils.CheckExpUp in reaction case branches (502-504) for EXP_UP translation |
| 32 | Implement `UpdateMasterCounterControl` method body using bitwise OR assignment (`|=`) for each SETBIT translation |
| 33 | Extract reaction case handling (500-601) to a dedicated private `HandleReactionCases(int reactionCaseId, CharacterId target, ...)` method called from `HandleCounterSource` |
| 34 | Ensure CounterSourceHandler primary constructor includes IClothingSystem parameter for CLOTHES_SETTING_TRAIN calls |
| 35 | Ensure CounterSourceHandler primary constructor includes IEngineVariables parameter for MASTER character resolution |
| 36 | Ensure CounterSourceHandler primary constructor includes ICommonFunctions parameter for HasPenis/HasVagina |
| 37 | Ensure CounterSourceHandler primary constructor includes IPainStateChecker parameter for PAIN_CHECK_V call |
| 38 | Ensure CounterSourceHandler primary constructor includes ITEquipVariables parameter for TEQUIP writes |
| 39 | Ensure CounterSourceHandler primary constructor includes ICounterUtilities parameter for IsOnce/CheckExpUp |
| 40 | Implement pre-check guard referencing CounterDecisionFlag for early return (C6) |
| 41 | Write test covering pre-check guard early return and うふふ==2 special path |
| 42 | Write test verifying CSTR string append behavior using StubCharacterStringVariables |
| 43 | Add all 10 new TCVarIndex constants (AC#23 spot-checks MasterCounterControl; AC#43 verifies total count = 20) |
| 44 | Implement all ~57 MCC SETBIT branches using `currentValue | (1 << bit)` bitshift translation (AC#44 verifies count >= 57 [I]) |
| 45 | Write test verifying STAIN bitwise OR merge behavior using StainIndex constants |
| 46 | Write equivalence tests in CounterSourceHandlerTests.cs that Assert.Equal on concrete SOURCE/STAIN/EXP state values after dispatch branch execution |
| 47 | Declare DatUIBottom, DatUIBottomT, DatUITop, DatUITopT, PainCheckVMaster method signatures on ICounterSourceHandler interface |
| 48 | Add all new ExpIndex constants to ExpIndex.cs; count verified by gte |
| 49 | Implement all ~36 CounterActionId dispatch cases in HandleCounterSource; count verified by gte on switch case labels |
| 50 | Write equivalence tests that exercise SetSource/GetSource for うふふ==2 block and dispatch branches (count verified by gte) |
| 51 | Write MCC bitfield tests that assert concrete bitmask output values using MasterCounterControl/MasterCounter references |
| 52 | Implement nested sub-dispatch on SubAction within HandleReactionCases |
| 53 | Write test methods that exercise SubAction sub-case branching |
| 54 | Write equivalence tests that invoke HandleCounterSource, DatUIBottom, DatUITop, PainCheckVMaster, and UpdateMasterCounterControl methods |
| 55 | Ensure CounterSourceHandler primary constructor includes ITouchSet parameter for TOUCH_SET call translations |
| 56 | Write test methods exercising each DATUI helper variant (DatUIBottom, DatUIBottomT, DatUITop, DatUITopT) for EQUIP zeroing |
| 57 | Add `CheckExpUp` stub method (returning false) to `TestCounterUtilities` in `Era.Core.Tests/Counter/ActionValidatorTests.cs` |
| 58 | Add `CheckExpUp` stub method (returning false) to `SelectorTestCounterUtils` in `Era.Core.Tests/Counter/ActionSelectorTests.cs` |
| 59 | Write equivalence tests in CounterSourceHandlerTests.cs that call `handler.HandleCounterSource(target, arg1)` then assert with `Assert.Equal` verifying concrete output values after HandleCounterSource invocation |
| 60 | Ensure CounterSourceHandler.cs contains no TODO, FIXME, or HACK markers; all deferred work must be tracked via Mandatory Handoffs |
| 61 | Ensure ICounterSourceHandler.cs contains no TODO, FIXME, or HACK markers; interface declarations must be clean of technical debt annotations |
| 62 | Write test methods in CounterSourceHandlerTests.cs that reference reaction case 600 (玉揉み/TesticularMassage) and case 601 (アナル愛撫/AnalStimulation) by case number or name, verifying their SOURCE/TCVAR mutation behavior |
| 63 | Implement PainCheckVMaster using `(int)(value * multiplier)` cast-truncation pattern (not Math.Round) to match ERA TIMES semantics |
| 64 | Implement DATUI helper methods with SetEquip calls to zero EQUIP indices per ERB COUNTER_SOURCE.ERB:789-890 |
| 65 | Write test for reaction case 505 sub-case 2 (手コキ) verifying ungated EXP increment without CheckExpUp call |
| 66 | In CounterSourceHandlerTests.cs equivalence tests, read SOURCE/STAIN state via `_stubVariables.GetSource(target, SourceIndex.X)` or `GetStain` to verify behavioral parity with ERB |
| 67 | Add PALAMLV threshold array to existing `Era.Core/Counter/ExpLvTable.cs`; Grep verifies PalamLv member exists |
| 68 | Update `PainStateChecker.cs` to reference `ExpLvTable.PalamLv` instead of inline PALAMLV array; Grep verifies `ExpLvTable\.PalamLv` appears at least once |
| 69 | Use `ExpLvTable` EXPLV and PALAMLV arrays in CounterSourceHandler.PainCheckVMaster instead of inline arrays |
| 70 | Write test assertions calling `GetExp` or `SetExp` to verify EXP mutation parity in reaction case sub-dispatch branches |
| 71 | Add CheckExpUp stub (returning false) to all 5 ICounterUtilities test stubs including 3 WC test files |
| 72 | Ensure HandleCounterSource calls UpdateMasterCounterControl in the event flow (not just method existence) |
| 73 | Grep(Era.Core/Character/PainStateChecker.cs) not_contains `PALAMLV` after ExpLvTable extraction |
| 74 | Implement int-range guard (500-601 check) in HandleCounterSource before CounterActionId enum cast; Grep verifies range check pattern present |
| 75 | Translate 19 CSTR:x:10 write sites to SetCharacterString calls in CounterSourceHandler.cs; Grep verifies production call count gte=19 [I] |
| 76 | Translate ~26 TOUCH_SET call sites to .TouchSet() calls in CounterSourceHandler.cs; Grep verifies production call count gte=25 [I] |
| 77 | Write MCC tests that invoke UpdateMasterCounterControl with multiple CASE inputs and Assert.Equal on bitmask output; Grep verifies invocation+assertion count |
| 78 | Implement UpdateMasterCounterControl to call SetTCVar for both MASTER and ARG characters; Grep verifies at least 2 SetTCVar calls referencing MasterCounterControl |
| 79 | Write CSTR behavioral tests that read back accumulated string via GetCharacterString to assert append semantics (player accumulates target, target accumulates player) |
| 80 | Ensure HandleReactionCases is called from HandleCounterSource (matches `HandleReactionCases\(` verifies call-chain per Key Decision D structural separation) |
| 81 | Write test methods in CounterSourceHandlerTests.cs with a configurable CheckExpUp stub (returning false) and verify EXP is NOT incremented for gated reaction cases (502-504). Grep verifies `CheckExpUp.*false\|ExpUp.*Skip\|!.*CheckExpUp\|ExpGated` gte=2 references in test file. |
| 82 | Write test methods in CounterSourceHandlerTests.cs verifying IEngineVariables MASTER resolution is called during dispatch. Grep verifies `GetMaster\|MasterCharacter\|StubEngineVariables.*[Mm]aster\|engineVariables.*[Mm]aster` gte=2 references in test file. |
| 83 | Ensure no files in Era.Core/Counter/ import or reference EmuEra, GlobalStatic, Interpreter, or ErbScript namespaces/types |
| 84 | Ensure `Era.Core/Interfaces/ICharacterStringVariables.cs` does not import or reference EmuEra, GlobalStatic, Interpreter, or ErbScript namespaces/types |
| 85 | Write at least one test in CounterSourceHandlerTests.cs asserting a STAIN mutation that occurs in the うふふ==2 pre-SELECTCASE block (co-locate Ufufu/うふふ keyword with Stain/GetStain keyword in the same test method or assertion) |
| 86 | Translate all 8 締り具合変動 call sites in COUNTER_SOURCE.ERB to `shrinkageSystem.UpdateShrinkage(...)` calls in CounterSourceHandler.cs dispatch method body; Grep verifies production call count gte=8 [I] |
| 87 | In the うふふ==2 test method(s), set CFLAG 317 (うふふ) to value 2 via `SetCharacterFlag(..., new CharacterFlagIndex(317), 2)` or `CFlagUfufu` constant before calling `handler.HandleCounterSource(...)` to exercise the C6 guard path (matches ActionSelector/ComableChecker raw-int pattern) |
| 88 | Write at least one test in CounterSourceHandlerTests.cs with method name or context referencing case 502 (手淫): e.g. `HandleReactionCase502_...` or `Reaction_502_...` |
| 89 | Write at least one test in CounterSourceHandlerTests.cs with method name or context referencing case 503 (フェラチオ): e.g. `HandleReactionCase503_...` or `Reaction_503_...` |
| 90 | Write at least one test in CounterSourceHandlerTests.cs with method name or context referencing case 504 (パイズリ): e.g. `HandleReactionCase504_...` or `Reaction_504_...` |
| 91 | In the うふふ==2 test method(s), assert at least one TCVAR mutation (EjaculationLocationFlag or EjaculationPleasureIntensity via SetTCVar) or EXP mutation (VSexExp via GetExp) in the same test method that references Ufufu/うふふ/CFlagUfufu |
| 92 | In the うふふ==2 test method(s), assert at least one SOURCE mutation (GetSource for SexualActivity, Conquest, Liquid, Sadism, Coercion, or GivePleasureV) in the same test method that references Ufufu/うふふ/CFlagUfufu |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| CSTR strategy | A: Extend IVariableStore; B: New ICharacterStringVariables interface; C: Pass string accumulator via method param | B: New ICharacterStringVariables | ISP compliance: CSTR is a distinct access pattern from int arrays. Avoids bloating IVariableStore (already 34 methods). Consistent with IStringVariables (SAVESTR) precedent from F789. |
| TOUCH_SET abstraction scope | A: Define ITouchSet in F803 scope now; B: Leave as TODO, implement in F811; C: Skip and use a delegate | A: Define ITouchSet in Era.Core/Counter/ now | F803 has 26 call sites requiring the interface NOW. Defining it in F803 scope avoids blocking F803 on F811 (Related, not Predecessor). F811 implements the interface later. |
| 締り具合変動 abstraction | A: Inline no-op stub; B: New IShrinkageSystem interface; C: Extend ICounterUtilities | B: New IShrinkageSystem interface in Era.Core/Counter/ | 8 call sites in F803; distinct responsibility from ICounterUtilities (which covers IsAirMaster, etc.). Follows single-responsibility. |
| PainCheckVMaster implementation | A: Reuse IPainStateChecker; B: Inline logic in CounterSourceHandler; C: New IPainCheckVMaster interface | B: Inline in CounterSourceHandler with static EXPLV/PALAMLV tables | PAIN_CHECK_V_MASTER modifies SOURCE:加虐 (not SOURCE:苦痛/反感 which IPainStateChecker returns). Logic is 25 lines -- not worth a separate interface. Matches PainStateChecker's self-contained hardcoded-table pattern. Note: PainCheckVMaster is exposed on ICounterSourceHandler interface for F805 consumption; implementation remains inlined in CounterSourceHandler. |
| EQUIP index typing | A: New EquipIndex typed struct; B: Use raw int constants in CounterSourceHandler | B: Raw int private constants in CounterSourceHandler | EQUIP indices used only within DATUI helpers (no cross-class reuse yet). Additive typing can be deferred to a later feature. ClothingSystem already uses raw ints. |
| CFLAG index typing | A: New CFlagIndex typed struct; B: Use raw int private constants in CounterSourceHandler | B: Raw int private constants in CounterSourceHandler | CFLAG indices used only within CounterSourceHandler dispatch branches (no cross-class reuse). Consistent with EQUIP index typing decision above. CounterSourceHandler is the only consumer; additive typing can be deferred to a later feature that introduces CFlagIndex. |
| CstrIndex typing | A: Named CstrIndex.EjaculationPartner constant + Task/AC; B: Raw `new CstrIndex(10)` with Key Decision | B: Raw `new CstrIndex(10)` at call sites | CSTR index 10 (射精パートナー) is the only CstrIndex value used. `new CstrIndex(10)` provides compile-time type safety (cannot accidentally pass int). Named constant adds no value for a single index — matches CFLAG/EQUIP raw-int precedent. CstrIndex named constants deferred to F813 if cross-class reuse emerges. |
| Reaction cases (500-601) structure | A: Second switch on CounterActionId enum (would fail since 500-601 not in enum); B: if/else on int value; C: Nested switch inside the primary switch CASE entry; D: Extract to dedicated private HandleReactionCases method | D: Dedicated private `HandleReactionCases(int reactionCaseId, CharacterId target, ...)` method called from `HandleCounterSource` | Reaction cases (500-601) use literal ints, not CounterActionId enum. Constraint C10 requires "separate handling from CounterActionId dispatch". Extracting to a dedicated method structurally enforces this separation (AC#33 verifiable), avoids mixing int literals into the enum switch, and keeps HandleCounterSource focused on CounterActionId dispatch. |
| CounterActionId 40-47 dispatch absence | A: Add CASE branches for 40-47; B: Document as intentionally absent; C: Investigate further | B: Document as intentionally absent | COUNTER_SOURCE.ERB:27-632 contains no CASE branches for values 40-47 (VirginOffering through SexualReliefA). ERB-verified: the SELECTCASE at line 27 jumps from range 30-35 to range 50-60 with no 40-47 handlers. These CounterActionId enum values exist (defined in F801) but have no counter source processing logic. AC#24 covers 6 dispatch ranges with confirmed ERB branches; 40-47 exclusion is intentional, not a translation omission. |
| DATUI_BOTTOM vs DATUI_BOTTOM_T naming | A: DatUIBottom (PLAYER target) + DatUIBottomT (ARG target); B: DatUIBottom(CharacterId targetChar) parametric | A: Two separate methods on ICounterSourceHandler interface | ERB has 4 distinct functions -- BOTTOM/BOTTOM_T/TOP/TOP_T -- each called independently. Separate methods allow F805 to call by name, matching the ERB call sites exactly. F805 injects ICounterSourceHandler (not concrete class) per DIP, so methods are exposed on the interface. |

### Interfaces / Data Structures

**New interface: `ICounterSourceHandler`** (`Era.Core/Counter/ICounterSourceHandler.cs`)

```csharp
namespace Era.Core.Counter;

public interface ICounterSourceHandler
{
    /// <summary>
    /// Main entry point: @EVENT_COUNTER_SOURCE(ARG, ARG:1) in COUNTER_SOURCE.ERB.
    /// </summary>
    void HandleCounterSource(CharacterId target, int arg1);

    /// <summary>Bottom undressing for PLAYER character.</summary>
    void DatUIBottom(CharacterId target, CharacterId player, int level);

    /// <summary>Bottom undressing for TARGET character.</summary>
    void DatUIBottomT(CharacterId target, CharacterId player, int level);

    /// <summary>Top undressing for PLAYER character.</summary>
    void DatUITop(CharacterId target, CharacterId player, int level);

    /// <summary>Top undressing for TARGET character.</summary>
    void DatUITopT(CharacterId target, CharacterId player, int level);

    /// <summary>Pain check for master counter (SOURCE:加虐 modification).</summary>
    void PainCheckVMaster(CharacterId target, CharacterId player);
}
```

**New interface: `ITouchSet`** (`Era.Core/Counter/ITouchSet.cs`)

```csharp
namespace Era.Core.Counter;

public interface ITouchSet
{
    /// <summary>
    /// Handles touch/contact state recording.
    /// Corresponds to CALL TOUCH_SET(mode, type, target) in COUNTER_SOURCE.ERB.
    /// Implemented by SOURCE_POSE.ERB handler (F811 scope).
    /// </summary>
    void TouchSet(int mode, int type, CharacterId target);
}
```

**New interface: `IShrinkageSystem`** (`Era.Core/Counter/IShrinkageSystem.cs`)

```csharp
namespace Era.Core.Counter;

public interface IShrinkageSystem
{
    /// <summary>
    /// Updates tightness variation for a character part.
    /// Corresponds to CALL 締り具合変動, character, amount, type in COUNTER_SOURCE.ERB.
    /// </summary>
    void UpdateShrinkage(CharacterId character, int amount, int type);
}
```

**New interface: `ICharacterStringVariables`** (`Era.Core/Interfaces/ICharacterStringVariables.cs`)

```csharp
namespace Era.Core.Interfaces;

public interface ICharacterStringVariables
{
    /// <summary>
    /// Gets character-scoped string variable (CSTR:character:index).
    /// Returns empty string for uninitialized slots.
    /// </summary>
    string GetCharacterString(CharacterId character, CstrIndex index);

    /// <summary>
    /// Sets character-scoped string variable (CSTR:character:index).
    /// Fire-and-forget; null values coalesced to empty string.
    /// </summary>
    void SetCharacterString(CharacterId character, CstrIndex index, string value);
}
```

**CounterSourceHandler constructor** (primary constructor pattern matching ActionSelector):

```csharp
public sealed class CounterSourceHandler(
    IVariableStore variables,
    IEngineVariables engine,
    ITEquipVariables tequip,
    ITouchSet touchSet,
    IClothingSystem clothingSystem,
    ICommonFunctions commonFunctions,
    IVirginityManager virginityManager,
    IPainStateChecker painStateChecker,
    IShrinkageSystem shrinkageSystem,
    ICounterUtilities counterUtils,
    ICharacterStringVariables charStrings,
    IRandomProvider random) : ICounterSourceHandler
```

**New TCVarIndex constants** (truly new additions; F801/F812 constants reused by name):

```csharp
// Counter source handler indices - Feature 803
public static readonly TCVarIndex MasterCounterControl = new(26); // マスターカウンター制御
// NOTE: UndressingPlayerLower = new(71) already exists (F801/F812 name for LowerBodyUndressAction, reuse TCVarIndex.UndressingPlayerLower)
// NOTE: UndressingPlayerUpper = new(72) already exists (F801/F812 name for UpperBodyUndressAction, reuse TCVarIndex.UndressingPlayerUpper)
// NOTE: UndressingTargetLower = new(73) already exists (F801/F812 name for TargetLowerBodyUndressAction, reuse TCVarIndex.UndressingTargetLower)
// NOTE: UndressingTargetUpper = new(74) already exists (F801/F812 name for TargetUpperBodyUndressAction, reuse TCVarIndex.UndressingTargetUpper)
public static readonly TCVarIndex EjaculationLocationFlag = new(100); // 射精箇所フラグ
public static readonly TCVarIndex EjaculationPleasureIntensity = new(101); // 射精快感強度
public static readonly TCVarIndex PositionRelationship = new(60); // 位置関係
// NOTE: SubAction = new(21) already exists (F801-established name at index 21, reuse TCVarIndex.SubAction)
// NOTE: SixtyNineTransition = new(27) already exists (F801/F812-established name for イきそう, reuse TCVarIndex.SixtyNineTransition)
```

Note: TCVarIndex integer values for 射精箇所フラグ, 射精快感強度, and 位置関係 (the 4 truly new constants including MasterCounterControl) must be verified against `DIM.ERH` before implementation. The values listed above are estimates from code analysis and are marked `[I]` for implementer verification. F801/F812 constants (SubAction/UndressingPlayer*/UndressingTarget*/SixtyNineTransition) are already defined — reuse by their established names.

**New SourceIndex constants** to add (19 baseline → 31):

```csharp
public static readonly SourceIndex Liquid = new(9);           // 液体
public static readonly SourceIndex SexualActivity = new(11);  // 性行動
public static readonly SourceIndex GivePleasureC = new(40);   // 与快Ｃ
public static readonly SourceIndex GivePleasureV = new(41);   // 与快Ｖ
public static readonly SourceIndex GivePleasureA = new(42);   // 与快Ａ
public static readonly SourceIndex GivePleasureB = new(43);   // 与快Ｂ
public static readonly SourceIndex Seduction = new(50);       // 誘惑
public static readonly SourceIndex Humiliation = new(51);     // 辱め
public static readonly SourceIndex Provocation = new(52);     // 挑発
public static readonly SourceIndex Service = new(53);         // 奉仕
public static readonly SourceIndex Coercion = new(54);        // 強要
public static readonly SourceIndex Sadism = new(55);          // 加虐
```

**New ExpIndex constants** to add:

```csharp
public static readonly ExpIndex VSexExp = new(20);           // Ｖ性交経験
public static readonly ExpIndex ASexExp = new(22);           // Ａ性交経験
public static readonly ExpIndex MasturbationExp = new(24);   // 手淫経験
public static readonly ExpIndex PaizuriExp = new(26);        // パイズリ経験
public static readonly ExpIndex SadisticExp = new(100);      // 嗜虐快楽経験
public static readonly ExpIndex KissExp = new(27);           // キス経験 (CSV-verified: index 27)
```

Note: ExpIndex integer values for 嗜虐快楽経験 must be verified against `Exp.csv` or `DIM.ERH` before implementation; 100 is an estimate.

**New StainIndex constants** to add (0 baseline → 8):

```csharp
public static readonly StainIndex Mouth = new(0);        // 口
public static readonly StainIndex Hand = new(1);         // 手
public static readonly StainIndex Penile = new(2);       // Ｐ
public static readonly StainIndex Vaginal = new(3);      // Ｖ
public static readonly StainIndex Anal = new(4);         // Ａ
public static readonly StainIndex Breast = new(5);       // Ｂ
public static readonly StainIndex InVagina = new(6);     // 膣内
public static readonly StainIndex InIntestine = new(7);  // 腸内
```

Note: Stain index integer values must be verified against `Stain.csv` or equivalent before implementation; assumed to be 0-7 sequential.

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| TCVarIndex constants for 射精箇所フラグ, 射精快感強度, 下半身脱衣行動, 上半身脱衣行動, 位置関係, カウンター行動の派生, イきそう are not yet in TCVarIndex.cs; integer values must be verified against DIM.ERH before use | Technical Constraints | Implementer must Grep DIM.ERH for `#DIM CONST TCVAR_` or `TCVarIndex` values before coding; values in Interfaces/Data Structures are estimates |
| ExpIndex for 嗜虐快楽経験 (足コキ at line 223) has unknown index value; listed as 100 in this design but must be verified against Exp.csv/DIM.ERH | Technical Constraints | Implementer must verify before adding the constant |
| ExpIndex for キス経験 (KissExp) has unknown index value; listed as 8 but must be verified against Exp.csv/DIM.ERH | Technical Constraints | Implementer must verify before adding the constant |
| StainIndex integer values (0-7 sequential assumed) must be confirmed against Stain.csv | Technical Constraints | Implementer must verify before adding the constants; if non-sequential, update the values accordingly |
| SourceIndex values for Liquid=9 and SexualActivity=11 must be confirmed against source.csv (gap at index 10 exists; SOURCE:液体 may be a different index) | AC Design Constraints (C11) | Implementer must grep source.csv for 液体 and 性行動 to confirm index values before coding |
| IShrinkageSystem interface is new and has no implementation -- wbs-generator must include a Task to create a stub implementation for tests | Upstream Tasks | wbs-generator should add Task to create StubShrinkageSystem in Era.Core.Tests |
| ITouchSet interface is new with no implementation in F803 scope -- wbs-generator must include a Task to create a stub implementation for tests | Upstream Tasks | wbs-generator should add Task to create StubTouchSet in Era.Core.Tests |
| ICharacterStringVariables interface is new with no VariableStore implementation -- CSTR is not stored in VariableStore today; VariableStore extension is outside F803 scope | Impact Analysis | F803 scope includes interface definition + stub only. VariableStore implementation deferred to a successor feature (to be tracked). AC#12 only verifies the interface exists, not VariableStore implementation. |
| LOST_VIRGIN(ARG) at line 534 takes 1 argument but IVirginityManager.CheckLostVirginity requires (target, partner). Implicit partner is PLAYER (騎乗位逆レイプ context) | Technical Design > Virginity calls | Implementer must pass PLAYER as implicit partner parameter |
| EXP_UP logic exists as private `CheckExpUp` in `AbilityGrowthProcessor.cs` (Era.Core/Training). F803 adds the method to ICounterUtilities interface; implementation may duplicate the logic. Future refactoring should extract to shared implementation. | Technical Design > EXP_UP calls | Implementer may implement directly in ICounterUtilities implementation; extraction from AbilityGrowthProcessor is a follow-up concern |
| **ExpIndex VExp collision**: ExpIndex.cs already has `VExp = new(1)`. F803's proposed `VExp = new(0)` for Ｖ経験 would cause a duplicate field name compiler error. The [I] tag on Task#3 ensures values are verified against CSV before implementation, but the naming conflict must be resolved at implementation time (e.g., use `VaginalExp` for index 0 or verify if existing VExp(1) is the correct mapping). | Technical Design > ExpIndex constants | Implementer must resolve naming collision before adding VExp(0) constant; check ExpIndex.cs for existing VExp field |
| **SourceIndex pre-existing constants**: SourceIndex.cs already has 31 constants (including the 12 that F803 was designed to add: Liquid, SexualActivity, GivePleasureC-B, Seduction-Sadism). Task#1/AC#7 may be satisfied without implementation. Verify at Task#1 execution time whether any additional constants are needed. | Baseline Measurement | Implementer must verify SourceIndex.cs count before adding any constants; AC#7 (count_equals 31) may already pass |
| **PALAMLV value ambiguity**: Two different PALAMLV value sets exist: (1) PainStateChecker.cs inline `{0, 100, 500, 3000, 10000, 30000, 60000, 100000, 150000, 250000}` from OPTION_2.ERB initialization, (2) GameOptions.setPalamLv `{30, 50, 100, ...}` (game settings). These serve different purposes: PainStateChecker values are static PALAMLV level thresholds (experience-to-level conversion), GameOptions values are configurable game parameters. Task#17 extracts PainStateChecker's values to ExpLvTable.cs. Implementer must verify extracted values match COUNTER_SOURCE.ERB:893-920 PALAMLV references. | Technical Design > PainCheckVMaster | Implementer must verify PALAMLV values against COUNTER_SOURCE.ERB source before extraction; use PainStateChecker.cs values (OPTION_2.ERB origin) as authoritative for ExpLvTable.PalamLv [I] |
| **TCVarIndex 6 duplicate constants**: TCVarIndex.cs already has 16 constants (baseline documented as 10 is stale). 6 of 10 planned "new" F803 constants already exist: SubAction=new(21) (≈CounterActionDerivation), UndressingPlayerLower=new(22) (≈LowerBodyUndressAction), UndressingTargetLower=new(23) (≈TargetLowerBodyUndressAction), UndressingPlayerUpper=new(24) (≈UpperBodyUndressAction), UndressingTargetUpper=new(25) (≈TargetUpperBodyUndressAction), SixtyNineTransition=new(27) (≈AboutToCome). Implementer must reuse existing constants or resolve alias naming before adding duplicates. AC#43 count_equals=20 remains correct (16 baseline + 4 truly new = 20). | Technical Design > TCVarIndex constants | Implementer must verify TCVarIndex.cs current state before adding constants; reuse existing F801/F812 constants where same TCVAR index is shared. Naming decision resolved (PostLoop-UserFix): use F801/F812 names (Option A applied). |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 3, 7 | Verify 12 SourceIndex constants present in `Era.Core/Types/SourceIndex.cs` (all 12 already added by F812; AC#7 is regression guard — no additions expected). If any are missing, add them after verifying values against `engine/build/uEmueraStandalone/eraKoumakan/CSV/source.csv` | [I] | [x] |
| 2 | 4 | Add 8 StainIndex constants to `Era.Core/Types/StainIndex.cs` (Mouth=0..InIntestine=7) after verifying sequential order against `engine/build/uEmueraStandalone/eraKoumakan/CSV/Stain.csv` or `Game/ERB/DIM.ERH` | [I] | [x] |
| 3 | 5, 48 | Add missing ExpIndex constants to `Era.Core/Types/ExpIndex.cs` (VSexExp=20, ASexExp=22, MasturbationExp=24, PaizuriExp=26, SadisticExp=100, KissExp=27) after verifying values against `engine/build/uEmueraStandalone/eraKoumakan/CSV/exp.csv`. Note: VExp and AExp2 removed — already exist from F812 (VExp=new(1), AExp=new(2)). KissExp corrected from 8→27 per CSV (index 8 = OrgasmExperienceB). | [I] | [x] |
| 4 | 12 | Create `Era.Core/Interfaces/ICharacterStringVariables.cs` with `GetCharacterString(CharacterId, CstrIndex)` and `SetCharacterString(CharacterId, CstrIndex, string)` methods | | [x] |
| 5 | 1, 47 | Create `Era.Core/Counter/ICounterSourceHandler.cs` interface with HandleCounterSource, DatUIBottom, DatUIBottomT, DatUITop, DatUITopT, PainCheckVMaster method signatures (per Interfaces/Data Structures section) | | [x] |
| 6 | 14, 15, 23, 43 | Add 4 truly new TCVarIndex constants to `Era.Core/Types/TCVarIndex.cs` after verifying integer values against `Game/ERB/DIM.ERH` for MasterCounterControl, EjaculationLocationFlag, EjaculationPleasureIntensity, PositionRelationship. Reuse existing F801/F812 names for overlapping indices: SubAction (index 21, was CounterActionDerivation), UndressingPlayerLower (index 22, was LowerBodyUndressAction), UndressingPlayerUpper (index 24, was UpperBodyUndressAction), UndressingTargetLower (index 23, was TargetLowerBodyUndressAction), UndressingTargetUpper (index 25, was TargetUpperBodyUndressAction), SixtyNineTransition (index 27, was AboutToCome) | [I] | [x] |
| 7 | 18 | Create `Era.Core/Counter/ITouchSet.cs` interface with `void TouchSet(int mode, int type, CharacterId target)` | | [x] |
| 8 | 19 | Create `Era.Core/Counter/IShrinkageSystem.cs` interface with `void UpdateShrinkage(CharacterId character, int amount, int type)` | | [x] |
| 9 | 2, 13, 21, 27, 29, 30, 34, 35, 36, 37, 38, 39, 55 | Create `Era.Core/Counter/CounterSourceHandler.cs` implementing `ICounterSourceHandler` with primary constructor injecting IVariableStore, IEngineVariables, ITEquipVariables, ITouchSet, IClothingSystem, ICommonFunctions, IVirginityManager, IPainStateChecker, IShrinkageSystem, ICounterUtilities, ICharacterStringVariables, IRandomProvider | | [x] |
| 10 | 6, 20, 26, 31, 33, 40, 49, 52, 72, 74, 75, 76, 80, 86 | Implement `HandleCounterSource` dispatch method in CounterSourceHandler covering all ~36 CounterActionId cases (seduction, humiliation, service, coercion, pleasure), pre-check guard (C6), and extract reaction cases (500-601) to dedicated `HandleReactionCases` private method with nested sub-dispatch on TCVarIndex.SubAction within cases 500, 502-505 (cases 600-601 have no nested dispatch per Technical Design). Translate all 19 CSTR:x:10 write sites to SetCharacterString calls (AC#75) and all ~26 TOUCH_SET call sites to touchSet.TouchSet() calls (AC#76) within dispatch method body. | | [x] |
| 11 | 8, 32, 44, 78 | Implement `UpdateMasterCounterControl` method in CounterSourceHandler translating each SETBIT to bitwise OR assignment (`|=`) across all ~30 CASE branches (~57 total SETBIT operations, multiple per branch). Each CASE branch writes SetTCVar to the exact character (MASTER or ARG) specified in the ERB — some cases write MASTER only, others write ARG only. Do NOT write both characters per case. AC#78 (gte=2) verifies the file contains both SetTCVar-MASTER and SetTCVar-ARG patterns across different cases. Implementer must verify exact SETBIT count from COUNTER_SOURCE.ERB:637-786 and update AC#44 threshold if actual count differs from 57. | [I] | [x] |
| 12 | 9, 10, 16, 17, 56, 64 | Implement public `DatUIBottom`, `DatUIBottomT`, `DatUITop`, `DatUITopT` undressing helper methods in CounterSourceHandler zeroing EQUIP indices per ERB COUNTER_SOURCE.ERB:789-890 | | [x] |
| 13 | 11, 63, 69 | Implement public `PainCheckVMaster` method in CounterSourceHandler replicating TIMES float-to-int truncation (`(int)(value * multiplier)`) from COUNTER_SOURCE.ERB:893-920 using shared ExpLvTable static class (extended from PainStateChecker duplication) | | [x] |
| 14 | 22, 24, 25, 41, 42, 45, 46, 50, 51, 53, 54, 56, 59, 62, 65, 66, 70, 77, 79, 81, 82, 85, 87 | Create `Era.Core.Tests/Counter/CounterSourceHandlerTests.cs` with stubs: `StubEngineVariables` (implements IEngineVariables), `StubShrinkageSystem` (implements IShrinkageSystem), `StubTouchSet` (implements ITouchSet), `StubCharacterStringVariables` (implements ICharacterStringVariables), and equivalence tests for representative dispatch branches, DATUI helpers, PainCheckVMaster boundary values, and MCC bitfield correctness. Note: CheckExpUp stub in StubCounterUtilities must be configurable (return true/false) to exercise both gated and ungated EXP paths in reaction case tests (cases 500-505). AC#41 naming constraint: test methods for C6 pre-check guard MUST use English keywords (EarlyReturn, PreCheck, Ufufu, SpecialPath) — Japanese naming fails the matcher. うふふ==2 block (COUNTER_SOURCE.ERB:8-26): test MUST assert all side effects — (a) 6 SOURCE values (SexualActivity, Conquest, Liquid, Sadism, Coercion, GivePleasureV), (b) PainCheckVMaster invocation (verify via mock/stub call count), (c) EXP:Ｖ性交経験 increment (GetExp assertion), (d) TCVAR:EjaculationLocationFlag=1 and EjaculationPleasureIntensity+=400 (GetTCVar assertions), (e) STAIN bitwise OR (covered by AC#85), (f) CSTR append (covered by AC#42). AC#50 counts SOURCE file-wide but does not enforce per-path coverage. | | [x] |
| 15 | 14, 15, 60, 61, 83, 84 | Build and verify Era.Core compiles with zero warnings (TreatWarningsAsErrors); run all Era.Core.Tests and confirm all pass | | [x] |
| 16 | 28, 57, 58 | Add `bool CheckExpUp(CharacterId character, int expIndex)` method to `Era.Core/Counter/ICounterUtilities.cs` interface and add `CheckExpUp` stub (returning false) to `TestCounterUtilities` in `Era.Core.Tests/Counter/ActionValidatorTests.cs` and `SelectorTestCounterUtils` in `Era.Core.Tests/Counter/ActionSelectorTests.cs` | | [x] |
| 17 | 14, 15, 67, 68, 73 | Add PALAMLV threshold array to existing `Era.Core/Counter/ExpLvTable.cs`; update PainStateChecker to reference ExpLvTable instead of inline arrays. After extraction, verify existing PainStateChecker tests in Era.Core.Tests/Character/ still pass (covered by Task#15 dotnet test run) | | [x] |
| 18 | 71 | Add `CheckExpUp` stub (returning false) to ICounterUtilities implementations in `Era.Core.Tests/Counter/WcCounterReactionTests.cs`, `Era.Core.Tests/Counter/WcActionSelectorTests.cs`, and `Era.Core.Tests/Counter/WcActionValidatorTests.cs` | | [x] |
| 88 | 88 | Write at least one test in CounterSourceHandlerTests.cs explicitly exercising reaction case 502 (手淫) SubAction sub-dispatch with assertion on output state | | [x] |
| 89 | 89 | Write at least one test in CounterSourceHandlerTests.cs explicitly exercising reaction case 503 (フェラチオ) SubAction sub-dispatch with assertion on output state | | [x] |
| 90 | 90 | Write at least one test in CounterSourceHandlerTests.cs explicitly exercising reaction case 504 (パイズリ) SubAction sub-dispatch with assertion on output state | | [x] |
| 91 | 91 | In the うふふ==2 test method(s), assert TCVAR (EjaculationLocationFlag=1 or EjaculationPleasureIntensity+=400 via SetTCVar) and/or EXP (Ｖ性交経験 via GetExp/SetExp) mutations co-located with Ufufu test context (AC#91 multiline co-location) | | [x] |
| 92 | 92 | In the うふふ==2 test method(s), assert SOURCE mutations (6 values: SexualActivity, Conquest, Liquid, Sadism, Coercion, GivePleasureV via GetSource) co-located with Ufufu test context (AC#92 multiline co-location) | | [x] |

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
| 1 | implementer | sonnet | feature-803.md Tasks 1-3, 6, 17, engine/build/.../CSV/source.csv, Stain.csv, exp.csv, Game/ERB/DIM.ERH | Updated SourceIndex.cs, StainIndex.cs, ExpIndex.cs, TCVarIndex.cs with verified constants + ExpLvTable.cs (PalamLv added) + PainStateChecker.cs (inline PALAMLV replaced with ExpLvTable.PalamLv per AC#68, AC#73) |
| 2 | implementer | sonnet | feature-803.md Tasks 4-5, 7-8, 16, 18, Era.Core/Interfaces/, Era.Core/Counter/, Era.Core/Counter/ICounterUtilities.cs, Era.Core.Tests/Counter/ActionValidatorTests.cs, Era.Core.Tests/Counter/ActionSelectorTests.cs, Era.Core.Tests/Counter/WcCounterReactionTests.cs, Era.Core.Tests/Counter/WcActionSelectorTests.cs, Era.Core.Tests/Counter/WcActionValidatorTests.cs | New ICharacterStringVariables.cs, ICounterSourceHandler.cs, ITouchSet.cs, IShrinkageSystem.cs; CheckExpUp added to ICounterUtilities + test stubs in ActionValidatorTests, ActionSelectorTests, WcCounterReactionTests, WcActionSelectorTests, WcActionValidatorTests |
| 2.5 | implementer | sonnet | feature-803.md Task 9, all interfaces from Phase 2 | CounterSourceHandler.cs skeleton (constructor + empty method stubs, compiles clean) |
| 3 | implementer | sonnet | feature-803.md Task 14 (harness only), StubVariableStore (F815), all new interfaces from Phase 2, CounterSourceHandler.cs from Phase 2.5 | CounterSourceHandlerTests.cs with test stubs and harness (no test methods) |
| 4 | implementer | sonnet | feature-803.md Tasks 10-13 + Task 14 (test methods), COUNTER_SOURCE.ERB, all interfaces from Phase 2 | CounterSourceHandler.cs + equivalence tests via interleaved TDD |
| 5 | implementer | sonnet | feature-803.md Task 15, full Era.Core codebase | Build verification (zero warnings) + all tests pass |

### Pre-conditions

- F801 [DONE]: CounterActionId enum and TCVAR:CounterAction write path available
- F815 [DONE]: StubVariableStore base class available in Era.Core.Tests
- `Game/ERB/COUNTER_SOURCE.ERB` accessible for line-by-line reference
- `engine/build/uEmueraStandalone/eraKoumakan/CSV/source.csv`, `engine/build/uEmueraStandalone/eraKoumakan/CSV/Stain.csv`, `engine/build/uEmueraStandalone/eraKoumakan/CSV/exp.csv`, `Game/ERB/DIM.ERH` readable for constant verification

### Execution Order

1. **Task 1-3, 6, 17** (CSV verification + constant additions + ExpLvTable extension): Implementer MUST grep CSV/ERH files first. Values marked `[I]` in Technical Design are estimates. Do NOT code estimated values; verify actuals.
2. **Task 4, 5, 7, 8, 16, 18** (Interface creation + CheckExpUp + WC stubs): All four new interfaces plus ICounterUtilities.CheckExpUp addition and test stubs. Create in order: ICharacterStringVariables → ICounterSourceHandler → ITouchSet → IShrinkageSystem → CheckExpUp (ICounterUtilities + ActionValidatorTests stub + ActionSelectorTests stub) → Task#18 (WcCounterReactionTests, WcActionSelectorTests, WcActionValidatorTests CheckExpUp stubs). **Task#16 and Task#18 are atomic (both must complete before any dotnet build check): Task#16 adds CheckExpUp to ICounterUtilities (public contract change), and Task#18 updates all 5 WC test stubs to implement the new method — if Task#16 runs without Task#18, existing WC test files will fail to compile. Task#16 must also complete before Task#10 (HandleReactionCases calls CheckExpUp).**
3. **Task 9** (CounterSourceHandler skeleton): Constructor + empty method stubs only. Must compile before proceeding.
4. **Task 14 (test harness)**: Create `CounterSourceHandlerTests.cs` with stubs only — `StubShrinkageSystem`, `StubTouchSet`, `StubCharacterStringVariables`, `StubEngineVariables` — and test class skeleton. No actual test methods yet. Must compile before proceeding.
5. **Tasks 10-13 + Task 14 test methods** (Interleaved TDD): For each implementation task, write the corresponding equivalence test methods (RED) from Task 14 scope, then implement the production code (GREEN). Order: dispatch → MCC → DATUI → PainCheckVMaster.
6. **Task 15** (Verification): Run `dotnet build Era.Core/` then `dotnet test Era.Core.Tests/` via WSL. Fix any failures before declaring complete.

### Build Verification Steps

```bash
# Phase 1-3 build check (after each phase)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'

# Phase 4-5 full test run
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/'
```

### Success Criteria

- All 90 ACs pass (checked by ac-tester)
- `dotnet build Era.Core/` exits with code 0 (zero warnings, TreatWarningsAsErrors=true)
- `dotnet test Era.Core.Tests/` exits with code 0 (all tests pass)
- No TODO/FIXME/HACK in new files

### Error Handling

- If CSV verification reveals different index values from Technical Design estimates: update constants accordingly; do NOT use estimated values
- If `ICharacterStringVariables` requires a stub in `engine.Tests/` (not just `Era.Core.Tests/`): create a second stub there and add an AC gap note to Review Notes
- If TCVarIndex values conflict with existing constants: STOP and report to user before proceeding

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| ICharacterStringVariables VariableStore implementation: CSTR is not stored in VariableStore today; connecting the interface to the engine VariableStore is outside F803 scope | F803 scope is interface definition + stub only; runtime VariableStore extension requires engine-layer changes tracked separately | Feature | F813 | - |
| EXP_UP logic duplication: CheckExpUp exists as private in AbilityGrowthProcessor; F803 adds public method to ICounterUtilities interface, creating duplication | Extraction to shared implementation is a refactoring concern; F803 scope is interface method addition only | Feature | F813 | - |
| ICounterSourceHandler ISP violation: single interface exposes 3 responsibilities (dispatch via HandleCounterSource, undressing via DatUI helpers, pain check via PainCheckVMaster) | F803 scope cannot extract IUndressingHandler without additional interface churn; ISP compliance requires separate interfaces per responsibility. Technical Design acknowledges this as a known trade-off | Feature | F813 | - |
| CFlagIndex typed struct: CFLAG dispatch branches use raw int private constants (CFLAG:MASTER:ローターA挿入, CFLAG:ARG:ぱんつ確認, etc.) instead of typed CFlagIndex | Key Decision defers CFlagIndex typing to a later feature; raw int pattern matches ActionSelector precedent. Cross-class reuse not yet needed | Feature | F813 | - |
| EquipIndex typed struct: DATUI helper methods use raw int EQUIP constants (装備:17 レオタード, etc.) instead of typed EquipIndex | Key Decision defers EquipIndex typing to a later feature; raw int pattern matches ActionSelector precedent. Cross-class reuse not yet needed | Feature | F813 | - |
| IShrinkageSystem runtime implementation: F803 creates IShrinkageSystem interface (Era.Core/Counter/IShrinkageSystem.cs) with no engine-layer implementation; production calls use stub/no-op until implemented | F803 scope is interface + stub only; runtime 締り具合変動 logic requires engine-layer integration tracked separately. Same pattern as ICharacterStringVariables deferred implementation | Feature | F813 | - |

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
| 2026-02-26T00:00 | START | orchestrator | /run 803 resume | [WIP] confirmed, fl-reviewed present |
| 2026-02-26T00:01 | DEVIATION | orchestrator | dotnet build Era.Core.Tests/ | PRE-EXISTING: 7 ICounterUtilities stubs missing CheckExpUp (Task 16/18 incomplete from prior session) |
| 2026-02-26T01:00 | DEVIATION | ac-static-verifier | code ACs | 12/87 FAIL: AC#2,20,42,44,52,64,75,78,79,85,87,91 |
| 2026-02-26T16:00 | DEVIATION | orchestrator | session-resume | Implementation lost: smart-implementer worktree changes not merged back. CounterSourceHandler.cs=83 lines (skeleton), Tests=0 [Fact] methods |
| 2026-02-26T16:30 | DEVIATION | feature-reviewer | Phase 8.1 quality review | NEEDS_REVISION: DATUI helpers incomplete (only 2 EQUIP indices vs per-level sets). Full re-implementation required |
| 2026-02-26T17:00 | FIX | smart-implementer | CounterSourceHandler.cs re-implementation | 1411 lines, all methods implemented, 0 warnings, DATUI per-level correct |
| 2026-02-26T17:30 | FIX | smart-implementer | CounterSourceHandlerTests.cs test methods | 29 [Fact] tests added (1146 lines total), 29/29 PASS |
| 2026-02-26T17:40 | FIX | orchestrator | SourceCalculator.cs revert | Reverted to HEAD (F812 out-of-scope changes removed) |
| 2026-02-27T00:00 | RESUME | orchestrator | /run 803 resume | [WIP] confirmed, all tasks [x], build 0 warnings, 2698/2698 tests pass |
| 2026-02-27T00:01 | VERIFY | ac-tester | Phase 7 AC verification | 92/92 ACs PASS (ac-tester reported AC#15 BLOCKED but direct retest confirmed 2698/2698 pass) |
| 2026-02-27T00:02 | DEVIATION | doc-reviewer | Phase 8.2 doc-check | NEEDS_REVISION: engine-dev SKILL.md missing ICounterUtilities.CheckExpUp (10→11), 3 new interfaces (ICounterSourceHandler, ITouchSet, IShrinkageSystem) |
| 2026-02-27T00:03 | FIX | orchestrator | Phase 8.2 SSOT fix | engine-dev SKILL.md updated: ICounterUtilities 10→11, added 3 interfaces to Counter System table, added F803 well-known values (StainIndex 8, ExpIndex 6, TCVarIndex 4) |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: Dependencies/Related Features | F811 status [DRAFT] → [BLOCKED] (sync with actual)
- [fix] Phase1-RefCheck iter1: Pre-conditions | CSV/source.csv, Stain.csv, Exp.csv paths updated to engine/build/.../CSV/ actual locations
- [fix] Phase1-RefCheck iter1: Pre-conditions | Game/ERH/DIM.ERH → Game/ERB/DIM.ERH (directory typo)
- [fix] Phase1-RefCheck iter1: Tasks 1-3,6 | CSV/ERH file path references updated to actual locations
- [fix] Phase1-RefCheck iter1: Implementation Contract Phase 1 | CSV path references updated
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#20 (exhaustive dispatch), AC#21 (IVirginityManager DI), AC#22 (PainCheckVMaster test), AC#23 (TCVarIndex MasterCounterControl)
- [fix] Phase2-Review iter1: Task#6 AC# | Updated mapping from "14, 15" to "14, 15, 23"
- [fix] Phase2-Review iter1: Goal Coverage | Updated items 1,4,5,7 with new AC references
- [fix] Phase2-Review iter1: Philosophy Derivation | Updated "typed indices" row with AC#20, AC#21, AC#23
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#24 (dispatch branch tests), AC#25 (MCC bitfield tests), AC#26 (reaction case handling)
- [fix] Phase2-Review iter2: Task#10 AC# | Updated to "6, 20, 26"; Task#14 AC# updated to "15, 22, 24, 25"
- [fix] Phase2-Review iter2: Goal Coverage | Updated items 1,8 with AC#24, AC#25, AC#26
- [fix] Phase2-Review iter3: Task#14 | Added StubCharacterStringVariables to stubs list
- [fix] Phase2-Review iter3: AC#8 | Tightened matcher from `\|=` to `UpdateMasterCounterControl` (method existence, AC#25 covers test)
- [fix] Phase3-Maintainability iter4: Constructor | Added IRandomProvider (12th param) + AC#27 for RAND:N translation
- [fix] Phase3-Maintainability iter4: Technical Constraints | Added EXP_UP external function (3 reaction case sites)
- [fix] Phase3-Maintainability iter4: ExpIndex constants | Added KissExp (キス経験) to new constants list
- [fix] Phase3-Maintainability iter4: TCVarIndex constants | Added AboutToCome (イきそう) to new constants list
- [fix] Phase3-Maintainability iter4: AC#26 | Tightened matcher from `500\|600` to `case 500\|case 600`
- [fix] Phase3-Maintainability iter4: Technical Design | Added EXP_UP calls mapping to ICounterUtilities.CheckExpUp
- [fix] Phase3-Maintainability iter4: Technical Design | Added TEQUIP writes, global SOURCE/TCVAR, CFLAG, BASE/MAXBASE, CLOTHES_SETTING_TRAIN docs
- [fix] Phase3-Maintainability iter4: Upstream Issues | Added LOST_VIRGIN(ARG) signature note, KissExp verification note
- [fix] Phase3-Maintainability iter5: Technical Design | CLOTHES_SETTING_TRAIN corrected to SettingTrain callback signature
- [fix] Phase3-Maintainability iter5: Technical Design | Added EQUIP index 17 (レオタード)
- [fix] Phase3-Maintainability iter5: AC#26 | Tightened to `case 500\|case 502\|case 600` (3 representative reaction cases)
- [fix] Phase3-Maintainability iter5: AC#20 | Broadened to include ArgumentOutOfRangeException
- [fix] Phase3-Maintainability iter5: AC Definition Table | Added AC#28 (ICounterUtilities.CheckExpUp) + Task#16
- [fix] Phase3-Maintainability iter5: Technical Design | Added DATUI/PainCheckVMaster placement rationale
- [fix] Phase3-Maintainability iter5: Upstream Issues | Added EXP_UP AbilityGrowthProcessor duplication note
- [fix] Phase3-Maintainability iter6: Task#9 AC# | Updated from "2, 13" to "2, 13, 21, 27" (orphaned AC fix)
- [fix] Phase3-Maintainability iter6: C6 | Corrected FLAG:22 → TCVAR:ARG:カウンター行動決定フラグ (ERB-verified)
- [fix] Phase3-Maintainability iter6: Mandatory Handoffs | Added EXP_UP duplication extraction tracking → F813
- [fix] Phase2-Review iter7: AC Definition Table | Added AC#29 (ICharacterStringVariables injection in CounterSourceHandler)
- [fix] Phase2-Review iter7: Task#9 AC# | Updated to "2, 13, 21, 27, 29"
- [fix] Phase2-Review iter7: Goal Coverage Item 6 | Updated to "AC#12, AC#29"
- [fix] Phase2-Review iter1: AC Coverage table | Added missing AC#29 row
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#32 (bitwise OR verification in UpdateMasterCounterControl) + AC#33 (HandleReactionCases method for C10 separation)
- [fix] Phase2-Review iter1: Goal Coverage | Updated Items 1,2 with AC#33, AC#32 respectively
- [fix] Phase2-Review iter1: Task#10 AC# | Updated to "6, 20, 26, 31, 33"; Task#11 AC# updated to "8, 32"
- [fix] Phase2-Review iter1: Key Decision | Reaction cases structure updated from option C to D (dedicated HandleReactionCases method)
- [fix] Phase2-Review iter2: AC#32 Details | Added Precision Note documenting file-scoped matcher limitation and AC#25 behavioral guarantee
- [fix] Phase2-Review iter3: Task#16 | Added CheckExpUp stub requirement for TestCounterUtilities and SelectorTestCounterUtils
- [fix] Phase2-Review iter3: Task#6 | Added AboutToCome to TCVarIndex constants list
- [fix] Phase2-Review iter3: Goal Coverage Item 5 | Removed AC#23 (TCVarIndex); added Goal Item 5b for TCVarIndex constants → AC#23
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#34-#38 (5 missing DI injection ACs: IClothingSystem, IEngineVariables, ICommonFunctions, IPainStateChecker, ITEquipVariables)
- [fix] Phase2-Review iter4: Task#9 AC# | Updated to "2, 13, 21, 27, 29, 30, 34, 35, 36, 37, 38"
- [fix] Phase2-Review iter4: Goal Coverage Item 7 | Added AC#34-#38
- [fix] Phase2-Review iter4: AC#4 Details | Added Count Derivation Note documenting [I] tag boundary (count from ERB, values from CSV)
- [fix] Phase2-Review iter5: AC Definition Table | Added AC#39 (ICounterUtilities injection, completing 12/12 DI verification)
- [fix] Phase2-Review iter5: Task#9 AC# | Updated to include AC#39; Goal Coverage Item 7 updated
- [resolved-skipped] Phase2-Uncertain iter5: Mandatory Handoffs destination for ICharacterStringVariables VariableStore implementation routed to F813 (Post-Phase Review) — user decision: F813 deferred obligations scope is appropriate; /fc will generate concrete tasks
- [fix] Phase2-Review iter6: AC Definition Table | Added AC#40 (C6 pre-check guard), AC#41 (pre-check test), AC#42 (CSTR behavioral test), AC#43 (TCVarIndex count_equals=20)
- [fix] Phase2-Review iter6: Goal Coverage | Item 1 += AC#40,AC#41; Item 5b += AC#43; Item 6 += AC#42
- [fix] Phase2-Review iter6: Task AC# | Task#10 += AC#40; Task#14 += AC#41,AC#42; Task#6 += AC#43
- [fix] Phase2-Review iter7: Success Criteria + Technical Design | Updated stale AC count from 31 → 43
- [fix] Phase2-Review iter7: Philosophy Derivation | Added AC#22, AC#41, AC#42 to "testable, maintainable" row
- [fix] Phase2-Review iter8: AC Definition Table | Added AC#44 (MCC branch completeness count_gte=15 for |= occurrences)
- [fix] Phase2-Review iter8: Goal Coverage Item 2 | Added AC#44; Task#11 AC# updated to "8, 32, 44"
- [fix] Phase2-Review iter8: Success Criteria + Technical Design | Updated AC count from 43 → 44
- [fix] Phase2-Review iter9: AC#41 matcher | Broadened from `CounterDecisionFlag|EarlyReturn|PreCheck` to include `Ufufu|SpecialPath` for うふふ==2 block coverage
- [fix] Phase2-Review iter10: C11 + 5 Whys Level 5 | Corrected SourceIndex missing constant count from 13 → 12
- [fix] Phase2-Review iter10: AC Definition Table | Added AC#45 (STAIN bitwise OR merge behavior test); AC count updated to 45
- [fix] Phase2-Review iter10: Goal Coverage Item 8 | Added AC#45; Task#14 AC# updated
- [resolved-applied] Phase3-Maintainability iter10: F805 depends on concrete CounterSourceHandler for DATUI_*/PainCheckVMaster — should depend on interface (IUndressingHelper or similar) per Dependency Inversion. Key Decision rationale says "future reuse" but F805 already requires reuse NOW
- [fix] Phase3-Maintainability iter10: Mandatory Handoffs | Added EXPLV/PALAMLV hardcoded threshold table duplication tracking → F813
- [fix] Phase4-ACValidation iter10: AC#44 | Changed invalid matcher `count_gte` → `gte` (valid matcher per testing SKILL.md)
- [fix] PostLoop-UserFix iter3: ICounterSourceHandler | Extended interface with DatUIBottom/DatUIBottomT/DatUITop/DatUITopT/PainCheckVMaster for F805 DIP compliance
- [fix] Phase1-RefCheck iter1: Links | Added [Predecessor: F803] to F805 Links section (reciprocal link)
- [fix] Phase1-RefCheck iter1: Links | Added [Predecessor: F803] to F813 Links section (reciprocal link)
- [fix] Phase2-Review iter1: Structure | Moved ## Summary + ### Scope Reference into ## Background > ### Overview (template compliance)
- [fix] Phase2-Review iter1: Constraint Details | Added missing detail blocks for C6, C8, C9, C10, C11, C13, C14, C15
- [fix] Phase2-Review iter2: AC#24 | Broadened matcher from 4 to 7 categories (added Provocation|Sadism|Pleasure) for dispatch branch coverage
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#46 (behavioral parity assertion), AC#47 (ICounterSourceHandler interface methods), AC#48 (ExpIndex count_gte=14)
- [fix] Phase2-Review iter1: Task#14 AC# | Added AC#46; Task#5 AC# added AC#47; Task#3 AC# added AC#48
- [fix] Phase2-Review iter1: Task#3 description | Added VExp, AExp2, KissExp to match Technical Design
- [fix] Phase2-Review iter1: Goal Coverage | Item 3 += AC#47; Item 5 += AC#48; Item 8 += AC#46
- [fix] Phase2-Review iter1: Background | Removed non-template ### Overview subsection header
- [fix] Phase2-Review iter1: Constraint Details | Reordered C6/C7/C12 to sequential numeric order
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#49 (dispatch branch count gte=30); AC count updated to 49
- [fix] Phase2-Review iter2: Task#10 AC# | Added AC#49; Goal Coverage Item 1 += AC#49
- [fix] Phase2-Review iter3: AC Definition Table | Added AC#50 (SOURCE accessor count gte=10), AC#51 (MCC bitmask assertion count gte=3); AC count updated to 51
- [fix] Phase2-Review iter3: Task#14 AC# | Added AC#50, AC#51; Goal Coverage Items 2,8 updated
- [fix] Phase2-Review iter4: AC#47 | Extended matcher to include HandleCounterSource (count_gte=5→6)
- [fix] Phase2-Review iter4: AC#26 | Strengthened matcher from 3 to 7 reaction case numbers (500-505, 600-601)
- [fix] Phase2-Review iter5: AC Definition Table | Added AC#52 (CounterActionDerivation in production), AC#53 (CounterActionDerivation in tests); AC count updated to 53
- [fix] Phase2-Review iter5: Technical Design | Added case 505 sub-case 2 unconditional EXP note
- [fix] Phase2-Review iter6: AC Definition Table | Added AC#54 (handler method invocation count gte=5); AC count updated to 54
- [fix] Phase2-Review iter6: AC#7 Details | Added Count Derivation Note for [I]-tagged CSV verification impact on count_equals
- [fix] Phase2-Review iter7: AC Definition Table | Added AC#55 (ITouchSet injection); AC count updated to 55; Task#9 AC# += 55
- [fix] Phase3-Maintainability iter8: Technical Design | Removed forbidden "future reuse" YAGNI text from DATUI/PainCheckVMaster placement rationale; replaced with ISP trade-off acknowledgment
- [resolved-applied] Phase3-Maintainability iter8: CounterSourceHandler has 12 constructor parameters (God Class concern). Consider extracting DATUI_*/PainCheckVMaster to separate UndressingHandler class with IUndressingHandler interface to reduce dependency count and satisfy ISP. ICounterSourceHandler currently exposes 3 responsibilities (dispatch, undressing, pain check). → Tracked in Mandatory Handoffs → F813
- [resolved-applied] Phase3-Maintainability iter8: EXPLV/PALAMLV threshold arrays will be duplicated between PainStateChecker and CounterSourceHandler.PainCheckVMaster. → Already tracked in Mandatory Handoffs → F813
- [fix] Phase2-Review iter9: Technical Design | Added reaction case routing clarification (two-stage int check → enum cast)
- [fix] Phase2-Review iter10: AC Definition Table | Added AC#56 (DATUI behavioral test count gte=4); AC count updated to 56
- [fix] Phase2-Review iter10: AC#52 | Strengthened matcher from matches to gte=3 for CounterActionDerivation sub-dispatch verification
- [fix] Phase3-Maintainability iter10: Technical Design | Added reversed CSTR pattern (5 sites: CSTR:ARG:10 += {PLAYER}/)
- [fix] Phase4-ACValidation iter10: AC#46, AC#47, AC#48 | Changed invalid matcher `count_gte` → `gte` (valid matcher per testing SKILL.md)
- [fix] Phase2-Review iter1: AC Details | Moved AC#49 detail block to correct sequential position (after AC#48, before AC#50)
- [fix] Phase2-Review iter1: Task#5 | Updated description to list all 6 ICounterSourceHandler method signatures
- [fix] Phase2-Review iter1: Key Decisions | Added CFLAG index typing decision entry
- [resolved-applied] Phase2-Review iter1: Two [pending] items from previous FL session (God Class 12-param concern + EXPLV/PALAMLV duplication) → ISP violation now tracked in Mandatory Handoffs → F813; EXPLV/PALAMLV already tracked in Mandatory Handoffs → F813
- [fix] Phase2-Review iter2: AC#22 | Strengthened matcher from matches to gte with count >= 2 for PainCheckVMaster boundary test verification
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#57 (CheckExpUp stub in ActionValidatorTests), AC#58 (CheckExpUp stub in ActionSelectorTests); AC count updated to 58; Task#16 AC# updated
- [fix] Phase2-Review iter3: AC Definition Table | Added AC#59 (behavioral parity assertion count gte=2); AC count updated to 59; Task#14 AC# updated; Goal Item 8 updated
- [resolved-applied] Phase2-Review iter3: ICounterSourceHandler ISP violation (dispatch+undressing+pain on single interface) → now tracked in Mandatory Handoffs → F813
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#60 (PainCheckVMaster truncation assertion gte=1); AC count updated to 60; Goal Item 4 updated; Task#14 AC# updated
- [fix] Phase2-Review iter4: Task#12 | Added AC#56 to AC# column for DATUI behavioral verification alignment
- [fix] Phase2-Review iter5: Mandatory Handoffs | Added ICounterSourceHandler ISP violation tracking → F813; resolved 3 related [pending] items (iter8, iter1, iter3)
- [fix] Phase2-Review iter6: AC#60 | Removed (void-method matcher structurally unmatchable); strengthened AC#22 to count >= 3; AC count back to 59
- [fix] Phase2-Review iter6: AC#44 | Changed matcher from `\|=` to `1 <<` (MCC-specific SETBIT pattern, avoids STAIN |= inflation)
- [fix] Phase2-Review iter6: Implementation Contract | Added Task#16 to Phase 2; added execution ordering note
- [fix] Phase2-Review iter7: Philosophy Derivation | Updated 'external calls' row to include all 18 interface injection ACs; trimmed 'typed indices' row to type-system ACs only
- [fix] Phase2-Review iter7: Philosophy Derivation | Added AC#22 to PAIN_CHECK_V_MASTER row for behavioral truncation verification
- [fix] Phase2-Review iter7: AC#47 | Hardened matcher from method name to `void MethodName` declaration pattern
- [fix] Phase2-Uncertain iter8: AC#52 | Raised threshold from count >= 3 to count >= 8 (covers 5 nested CounterActionDerivation dispatch reaction cases)
- [fix] Phase2-Uncertain iter8: AC#44 | Raised threshold from count >= 15 to count >= 25 (reflects ~57 SETBIT operations in MCC block)
- [resolved-skipped] Phase2-Review iter8: CSTR 19-call-site runtime gap — user decision: 現状維持。strategy resolved = 設計決定完了の意。VariableStore実装はMandatory Handoffs → F813で追跡済み
- [resolved-applied] Phase2-Review iter9: AC#44 threshold raised 25→50 per user decision (88% of ~57 SETBITs)
- [fix] PostLoop-UserFix iter9: AC#44 | Raised threshold from count >= 25 to count >= 50
- [resolved-applied] Phase2-Review iter9: AC#52 threshold raised 8→10 per user decision (5 cases × 2 refs)
- [fix] PostLoop-UserFix iter9: AC#52 | Raised threshold from count >= 8 to count >= 10
- [fix] Phase2-Review restart-iter1: Task#11 | Updated description from '~30 CASE branches' to '~30 CASE branches (~57 total SETBIT operations, multiple per branch)'
- [fix] Phase2-Review restart-iter1: Technical Design | Added reaction cases 600-601 behavioral specification (non-CounterActionDerivation)
- [fix] Phase3-Maintainability restart-iter2: AC Definition Table | Added AC#60 (not_contains TODO/FIXME/HACK in CounterSourceHandler.cs), AC#61 (not_contains in ICounterSourceHandler.cs); AC count updated to 61; Task#15 AC# updated
- [resolved-applied] Phase3-Maintainability restart-iter2: EXPLV/PALAMLV extraction to shared ExpLvTable.cs (F812) within F803 scope — user approved; Task#17 added, Mandatory Handoff removed
- [fix] Phase2-Review restart-iter3: AC#59 | Corrected How-to-Satisfy text from handler.GetSource to _stubVariables.GetSource (IVariableStore stub pattern)
- [fix] Phase2-Review restart-iter3: AC#52 | Clarified description to carve out cases 600-601 (no CounterActionDerivation sub-dispatch)
- [resolved-applied] Phase2-Review restart-iter3: AC#59 same-line matcher concern — user approved split to AC#59 (Assert.Equal count) + AC#66 (GetSource|GetStain count)
- [fix] Phase2-Review restart-iter4: AC Definition Table | Added AC#62 (reaction cases 600-601 behavioral test coverage gte=2); AC count updated to 62; Task#14 AC# updated; Goal Item 8 updated
- [fix] Phase2-Review iter1: AC#59 row | Fixed malformed column format (Type='Grep' → 'code', Method restructured to Grep(path) convention)
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#63 (PainCheckVMaster int-cast truncation pattern in production code); AC count updated to 63; Task#13 AC# updated; Goal Item 4 updated; Philosophy Derivation updated
- [resolved-applied] Phase3-Maintainability iter2: ICounterSourceHandler ISP violation (3 responsibilities) — loop-detected; tracked in Mandatory Handoffs → F813
- [resolved-applied] Phase3-Maintainability iter2: 12-param God Class constructor — loop-detected; tracked in Mandatory Handoffs → F813
- [resolved-applied] Phase3-Maintainability iter2: EXP_UP duplication deferral to F813 — loop-detected; tracked in Mandatory Handoffs → F813
- [fix] Phase7-FinalRefCheck iter2: Links | Added [Related: F789] (SAVESTR precedent referenced in Key Decisions)
- [fix] Phase4-ACValidation iter3: AC#22 | Fixed Expected column format from prose to standard gte format (`PainCheckVMaster` = 3)
- [fix] Phase2-Review iter4: Goal Coverage Item 7 | Added missing AC#29 (ICharacterStringVariables injection)
- [fix] Phase2-Review iter4: Technical Design | Corrected SETBIT Translation to use typed TCVarIndex.MasterCounterControl instead of raw new TCVarIndex(TCVarMasterCounterControl)
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#64 (DATUI SetEquip zeroing verification gte=4); AC count updated to 64; Task#12 AC# updated; Goal Item 3 updated
- [fix] Phase2-Review iter5: Task#14 | Added StubEngineVariables to test stubs list (IEngineVariables required for CounterSourceHandler constructor)
- [fix] Phase2-Review iter6: Philosophy Derivation | Updated SETBIT row to include AC#44, AC#51
- [fix] Phase2-Review iter6: AC Definition Table | Added AC#65 (case 505 sub-case 2 ungated EXP test); AC count updated to 65; Task#14 AC# updated; Goal Item 8 updated
- [fix] Phase2-Review iter1: AC Details | Reordered AC#64/AC#65 detail blocks to numeric order (AC#64 before AC#65)
- [fix] Phase2-Review iter1: Review Notes | Changed 3 loop-detected [pending] items (ISP violation, God Class, EXP_UP) to [resolved-applied] — all tracked in Mandatory Handoffs → F813
- [fix] PostLoop-UserFix iter1: Task#13/Technical Design | Extracted EXPLV/PALAMLV to shared ExpLvTable.cs (F812) within F803 scope; removed Mandatory Handoff → F813
- [fix] PostLoop-UserFix iter1: AC#59 | Split same-line matcher to AC#59 (Assert.Equal gte=2) + AC#66 (GetSource|GetStain gte=2); AC count updated to 66; Task#14 AC# updated; Goal Item 8 updated
- [fix] Phase2-Review iter1: AC Definition Table | Moved AC#66 row from between AC#59/AC#60 to after AC#65 (sequential numeric order); moved AC#66 Details block correspondingly
- [fix] Phase2-Review iter1: Goal Coverage | Merged Goal Item 5b into Goal Item 5 (integer Goal Item numbers only per template)
- [fix] Phase2-Review iter1: AC Coverage table | Moved AC#66 row to sequential position after AC#65
- [fix] Phase2-Review iter1: AC#52 Details | Changed invalid matcher `count_gte` → `gte`
- [fix] Phase2-Review iter1: AC Coverage table | Changed `count_gte` → `gte` in AC#48, AC#49, AC#50 How-to-Satisfy text
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#67 (ExpLvTable.cs contains PalamLv), AC#68 (PainStateChecker references ExpLvTable); AC count updated to 68; Task#17 AC# updated
- [fix] Phase2-Review iter1: AC#50 Details | Fixed Expected from count >= 10 to count >= 20 (matching Definition Table)
- [resolved-applied] Phase2-Uncertain iter1: Philosophy claim 'Era.Core as single source of truth for all counter subsystem logic' vs Mandatory Handoff deferring ICharacterStringVariables VariableStore to F813 — 19 CSTR call sites have interface stubs only, no runtime storage → Philosophy narrowed to exclude CSTR string accumulation (F813 tracked)
- [fix] Phase2-Review iter2: AC#68 | Strengthened matcher from `matches` to `gte` count >= 2 (prevents dead import/comment satisfying AC)
- [fix] Phase2-Review iter3: AC#50 Details | Fixed rationale text 'threshold of 10' → 'threshold of 20' (matching Definition Table Expected = 20)
- [fix] Phase2-Review iter3: Implementation Contract | Reordered Execution Order for TDD compliance: Task#14 harness before Tasks#10-13, interleaved RED→GREEN
- [fix] Phase2-Review iter4: Task#10 description | Added explicit CounterActionDerivation nested sub-dispatch mention for AC#52 traceability
- [fix] Phase2-Review iter5: AC Definition Table | Added AC#69 (CounterSourceHandler references ExpLvTable gte=2); AC count updated to 69; Task#13 AC# updated; Goal Item 4 updated
- [fix] Phase2-Review iter5: AC#50 Details | Added Precision Note clarifying grep scope excludes local stubs
- [fix] Phase2-Uncertain iter6: AC#53 | Raised threshold from gte=2 to gte=10 (symmetric with AC#52 production-code threshold: 5 sub-cases × 2 refs)
- [fix] Phase2-Review iter7: AC Definition Table | Added AC#70 (GetExp|SetExp in tests gte=1 for C1 EXP parity); AC count updated to 70; Task#14 AC# updated; Goal Item 8 updated
- [fix] Phase2-Review iter8: Implementation Contract | Added Task#17 to Phase 1 and Execution Order Step 1 (ExpLvTable.cs extension before Task#13)
- [fix] Phase2-Review iter8: AC#26 | Changed matcher from `matches` to `gte` count=7 (enforces all 7 reaction case numbers present)
- [fix] Phase2-Review iter8: Goal Coverage Item 4 | Added AC#67, AC#68 (ExpLvTable deliverables)
- [fix] Phase2-Uncertain iter9: AC#49 | Raised threshold from gte=30 to gte=36 (matches actual 36 CNT_ CASE branches in COUNTER_SOURCE.ERB)
- [fix] Phase2-Review iter10: Technical Design SETBIT Translation | Clarified: 7 existing MCC_ constants use `1 << Constants.MCC_X`; remaining ~50 use literal `1 << bitIndex` (ensures AC#44 gte=50 is satisfiable)
- [fix] Phase3-Maintainability iter10: C13 | Updated stale claim 'no CSTR accessor' → 'getter exists, no setter' (verified: IVariableStore.GetCharacterString exists)
- [fix] Phase3-Maintainability iter10: Upstream Issues | Added ExpIndex VExp naming collision warning (VExp=new(1) already exists)
- [fix] Phase3-Maintainability iter10: Baseline + Upstream Issues | Updated SourceIndex baseline from 19→31 (12 constants already present); added pre-existing constants note
- [resolved-applied] Phase3-Maintainability iter10: F813 Deferred Obligations missing 3 F803 Mandatory Handoff entries (ICharacterStringVariables VariableStore, EXP_UP duplication, ISP violation) — cross-feature fix required → Propagated 3 entries to F813 Deferred Obligations section
- [fix] Phase2-Review iter2: AC#7 Details | Updated baseline from 19→31 (pre-existing); AC#7 now serves as regression guard
- [fix] Phase2-Review iter2: AC#44 Details | Added Implementation Constraint note referencing Technical Design mandate for `1 <<` pattern
- [fix] Phase2-Review iter3: Goal Coverage | Moved AC#57/AC#58 from Goal Item 7 (interface DI) to Goal Item 8 (build succeeds) — they verify compile compatibility, not interface injection
- [fix] Phase2-Review iter4: Goal Item 5 | Rephrased to 'SourceIndex: regression guard; others: additive' (AC#7 is pre-satisfied)
- [fix] Phase2-Review iter4: AC#70 | Raised threshold from gte=1 to gte=3 (aligned with AC#66 floor; covers gated+ungated+assertion)
- [fix] Phase2-Review iter5: AC#71 added | Aggregate CheckExpUp stub coverage (gte=5) for all ICounterUtilities stubs including 3 WC test files
- [fix] Phase2-Review iter6: Task#18 added | CheckExpUp stub addition for 3 WC test files (AC#71 was orphaned without Task coverage)
- [fix] Phase2-Review iter7: AC#72 added | Verifies HandleCounterSource invokes UpdateMasterCounterControl (MCC call chain gap)
- [fix] Phase2-Review iter7: AC#26 | Updated matcher pattern to support both switch statement (case N) and switch expression (N =>) syntax
- [fix] Phase3-Maintainability iter8: ICharacterStringVariables | Changed raw int index to CstrIndex for consistency with codebase typed index pattern
- [fix] Phase3-Maintainability iter8: Implementation Contract Phase 2 | Added Task#18 (WC test file CheckExpUp stubs)
- [fix] Phase3-Maintainability iter8: Success Criteria | Updated AC count from 70 to 72
- [fix] Phase4-ACValidation iter9: Tech Design | Updated 'All 70 ACs' → 'All 72 ACs'; added AC#71/AC#72 to AC Coverage table
- [resolved-applied] Phase3-Maintainability iter8: ThresholdTables.cs (Task#17, AC#67-69) vs existing ExpLvTable.cs (F812) — responsibility overlap resolved: extend ExpLvTable.cs with PalamLv array; Task#17/AC#67-69 updated accordingly
- [fix] Phase4-ACValidation iter10: AC#12 | Narrowed Method from Grep(Era.Core/Interfaces/) to Grep(Era.Core/Interfaces/ICharacterStringVariables.cs) and Expected to SetCharacterString only (prevents false-positive from existing IVariableStore.GetCharacterString)
- [fix] Phase4-ACValidation iter10: AC#48 Details | Updated baseline from 8 to 11 (actual current ExpIndex count)
- [resolved-applied] Phase1-CodebaseDrift iter1: ExpIndex VExp collision — F803 plans VExp=new(0) but F812 already added VExp=new(1). F803's index 0 is incorrect (index 0 is CExp per F812). Remove VExp from F803 planned additions.
- [resolved-applied] Phase1-CodebaseDrift iter1: ExpIndex AExp2 collision — F803 plans AExp2=new(1) but index 1 is VExp (F812). Adding AExp2=new(1) would create misleading alias. AExp=new(2) already exists for A experience. Remove AExp2 from planned additions.
- [resolved-applied] Phase1-CodebaseDrift iter1: ExpIndex KissExp vs OrgasmExperienceB — CSV verified: index 8 = Ｂ絶頂経験 (OrgasmExperienceB), index 27 = キス経験 (KissExp). Corrected KissExp from new(8) to new(27) in Task#3, AC#48 details.
- [resolved-applied] Phase1-CodebaseDrift iter1: ExpIndex addition count stale — Updated: 6 new constants (not 8). VExp/AExp2 removed (F812 pre-existing). Task#3 description, AC#48 details, AC#5 rationale, Implementation Overview all corrected.
- [resolved-applied] Phase1-CodebaseDrift iter1: C11 stale text — Technical Constraint C11 updated to regression guard (all 12 constants already added by F812)
- [fix] Phase1-CodebaseDrift iter1: C11 | Updated constraint text from 'must be added' to 'regression guard; already added by F812'
- [info] Phase1-DriftChecked: F812 (Related)
- [fix] Phase2-Review iter1: Mandatory Handoffs | Simplified Creation Task column from verbose text to plain '-' for all 3 Option B entries (template compliance)
- [resolved-applied] Phase2-Uncertain iter2: CstrIndex named constant — user decision: B) raw `new CstrIndex(10)` with Key Decision entry matching CFLAG/EQUIP pattern. Single index doesn't warrant named constant.
- [fix] Phase2-Review iter2: Implementation Contract | Added Phase 2.5 for Task#9 (CounterSourceHandler.cs skeleton); updated Phase 3 input to include CounterSourceHandler.cs from Phase 2.5
- [fix] Phase3-Maintainability iter3: AC Definition Table | Added AC#73 (PainStateChecker inline PALAMLV removal not_contains); AC count updated to 73; Task#17 AC# updated; Goal Item 4 updated
- [fix] Phase2-Review iter4: Mandatory Handoffs | Added CFlagIndex and EquipIndex typed struct deferrals to F813 (Track What You Skip compliance)
- [fix] Phase2-Review iter5: AC#73 | Tightened matcher from `PALAMLV` to `private static readonly int\[\] PALAMLV` (prevents false failure from comment matches)
- [fix] Phase2-Review iter5: AC#73 Details | Added missing Test and Expected fields to Detail block
- [fix] Phase2-Review iter5: AC#68 | Changed matcher from `ExpLvTable` gte=2 to `ExpLvTable\.PalamLv` gte=1 (prevents pre-satisfaction by existing EXPLV refs)
- [fix] Phase2-Review iter6: Philosophy Derivation | Added AC#67, AC#68, AC#69, AC#73 to PAIN_CHECK_V_MASTER row; added AC#72 to SETBIT row
- [fix] Phase2-Review iter6: Technical Design | Updated stale AC count from 72 → 73
- [fix] Phase2-Review iter7: Implementation Contract Phase 1 | Added PainStateChecker.cs to Output column (Task#17 scope includes AC#68/AC#73)
- [fix] Phase2-Review iter7: Philosophy Derivation | Moved AC#57/AC#58 from 'external calls' to 'testable, maintainable' row (consistent with Goal Coverage move from Item 7 to Item 8)
- [fix] Phase3-Maintainability iter8: F813 Deferred Obligations | Propagated CFlagIndex (#4) and EquipIndex (#5) typed struct deferrals from F803 Mandatory Handoffs
- [fix] Phase2-Review iter1: Philosophy Derivation | Added AC#43, AC#48 to 'typed indices' row (TCVarIndex + ExpIndex aggregate coverage)
- [fix] Phase2-Uncertain iter1: AC#42 | Strengthened matcher from `matches` CharacterString to `gte` >= 3 SetCharacterString (prevents satisfaction by stub class declaration alone)
- [fix] Phase2-Review iter1: ExpIndex drift | Resolved 4 Phase1-CodebaseDrift pending items: VExp/AExp2 removed from Task#3 (F812 pre-existing); KissExp corrected 8→27 per CSV; AC#48/AC#5 details updated
- [fix] Phase2-Review iter1: Implementation Overview | Updated ExpIndex deliverable row to remove VExp, add SadisticExp/KissExp
- [fix] Phase2-Review iter1: AC#49 | Strengthened matcher from `CounterActionId\.` to `case CounterActionId\.\|CounterActionId\.\w+ =>` (targets switch case labels specifically, eliminates non-case enum reference inflation)
- [fix] Phase2-Review iter2: Goal Item 5 + Task#1 | Updated stale text: SourceIndex 12 constants already added by F812; Goal 5 rephrased to 'regression guard'; Task#1 changed to 'Verify present' (no additions expected)
- [fix] Phase3-Maintainability iter3: AC#68 | Raised threshold from gte=1 to gte=2 (GetPalamLv method body must reference ExpLvTable.PalamLv for lookup + overflow; 1 could be import/comment only)
- [fix] Phase3-Maintainability iter3: Task#14 | Added configurable CheckExpUp stub note for reaction case gated/ungated EXP path testing
- [fix] Phase2-Uncertain iter4: AC Definition Table | Added AC#74 (reaction case int-range guard before CounterActionId cast); AC count updated to 74; Task#10 AC# updated; Goal Item 1 updated
- [fix] Phase2-Uncertain iter5: AC Definition Table | Added AC#75 (SetCharacterString call-site presence in production CounterSourceHandler); AC count updated to 75; Task#10 AC# updated; Goal Item 6 updated; Philosophy Derivation CSTR row updated; Success Criteria updated
- [fix] Phase2-Review iter6: AC#41 | Strengthened matcher from `matches` to `gte` count >= 5 (ensures multiple test methods + assertion depth for うふふ==2 block's 12 SOURCE mutations; presence-only was insufficient)
- [fix] Phase2-Review iter7: AC#24 | Changed matcher from `matches` to `gte` count >= 7 (ensures all 7 dispatch categories have test coverage; previous matches was satisfied by single category)
- [fix] Phase2-Review iter7: AC Definition Table | Added AC#76 (TouchSet call-site presence in production CounterSourceHandler gte=10); AC count updated to 76; Task#10 AC# updated; Goal Item 7 updated
- [fix] Phase2-Review iter8: AC Definition Table | Added AC#77 (MCC test invocations + bitmask assertions gte=5); AC count updated to 77; Task#14 AC# updated; Goal Item 2 updated
- [fix] Phase4-ACLint iter9: Technical Design | Fixed stale AC count 76 → 77
- [resolved-applied] Phase2-Review iter10: AC#44 threshold 50 → 57 [I] per user decision (100% coverage, Zero Debt Upfront)
- [resolved-applied] Phase2-Review iter10: AC#41 うふふ==2 assertion depth — threshold raised 5 → 12 per user decision (12 SOURCE mutations require 12+ keyword references)
- [fix] Phase2-Review iter1: Implementation Contract Phase table | Removed Task#6 from Phase 2 Input "Tasks 4-8" → "Tasks 4-5, 7-8" (Task#6 already in Phase 1 per Execution Order Step 1)
- [fix] Phase2-Review iter1: Task#14 AC# column | Removed AC#15 from Task#14 (AC#15 = "All unit tests pass" belongs exclusively to Task#15; Task#14 only creates test file)
- [fix] Phase2-Review iter2: Philosophy Derivation | Synced dispatch row AC Coverage with Goal Coverage Item 1: added AC#40, AC#49, AC#52, AC#53, AC#74
- [fix] Phase2-Review iter2: Philosophy Derivation | Synced SETBIT row AC Coverage with Goal Coverage Item 2: added AC#77, AC#78
- [fix] Phase2-Review iter2: Philosophy Derivation | Added AC#76 to external calls row (TouchSet call-site production verification)
- [fix] Phase2-Review iter3: Philosophy Derivation | Synced 'testable, maintainable' row with Goal Item 8: added AC#45, AC#50, AC#54, AC#59, AC#60, AC#61, AC#62, AC#65, AC#66, AC#70, AC#71
- [fix] Phase2-Review iter3: Philosophy Derivation | Synced DATUI row with Goal Item 3: added AC#47, AC#64
- [fix] Phase2-Review iter3: Technical Design AC summary | Appended 13 missing AC coverage explanations (AC#44, AC#45, AC#49, AC#60, AC#61, AC#70-AC#78)
- [fix] Phase2-Review iter4: Task#10/Task#11 AC# | Moved AC#72 from Task#11 to Task#10 (AC#72 verifies HandleCounterSource calls UpdateMasterCounterControl — caller responsibility is Task#10, not Task#11)
- [fix] Phase2-Review iter5: AC#53 matcher | Changed from `CounterActionDerivation\|Derivation` to `CounterActionDerivation` (bare `Derivation` substring was inflatable by incidental matches; now symmetric with AC#52 production pattern)
- [fix] Phase2-Review iter5: Task#16 AC# | Removed AC#71 (aggregate gte=5 unverifiable at Task#16 completion with only 2 of 5 stubs; AC#71 belongs to Task#18 where 5th stub completes the count)
- [fix] Phase2-Review iter6: Task#10 description | Appended CSTR (19 sites → SetCharacterString, AC#75) and TOUCH_SET (26 sites → TouchSet, AC#76) translation requirements
- [fix] Phase2-Review iter6: Task#11 description | Appended dual-character SetTCVar requirement for MASTER and ARG (AC#78)
- [fix] Phase3-Maintainability iter10: ExpIndex code block | Removed VExp/AExp2 from Technical Design code block (already exist from F812, per Task#3 and Review Notes)
- [fix] Phase3-Maintainability iter10: AC#49 | Fixed double-counting pattern; changed from `case CounterActionId\.\|CounterActionId\.\w+ =>` to `CounterActionId\.[A-Z]` (single-count per enum reference)
- [fix] Phase3-Maintainability iter10: Philosophy Derivation | Added CFLAG/EQUIP raw-int exception note per Key Decisions (tracked F813)
- [fix] Phase3-Maintainability iter10: AC Definition Table | Added AC#78 (MCC dual-character TCVAR write verification gte=2); AC count updated to 78; Task#11 AC# updated; Goal Item 2 updated
- [fix] Phase3-Maintainability iter1: Implementation Contract Execution Order | Clarified Task#16/Task#18 atomicity (both must complete before build check; Task#16 public contract change breaks WC stubs without Task#18)
- [resolved-skipped] Phase3-Maintainability iter1: ISP violation (ICounterSourceHandler 3 responsibilities) — user decision: skip; F813 Mandatory Handoff追跡済み (4th loop)
- [resolved-applied] Phase3-Maintainability iter1: PALAMLV value ambiguity — user decision: Upstream Issues追記。PainStateChecker値（OPTION_2.ERB起源）が権威的。GameOptionsは別目的。[I]タグで実装者確認
- [resolved-skipped] Phase3-Maintainability iter1: 12-param God Class constructor — user decision: skip; F813 Mandatory Handoff追跡済み (loop)
- [resolved-skipped] Phase3-Maintainability iter1: EXP_UP duplication (AbilityGrowthProcessor vs ICounterUtilities.CheckExpUp) — user decision: skip; F813 Mandatory Handoff追跡済み (loop)
- [fix] PostLoop-UserFix iter2: Key Decisions | Added CstrIndex typing row (option B: raw `new CstrIndex(10)` matching CFLAG/EQUIP precedent)
- [fix] PostLoop-UserFix iter2: AC#44 | Raised threshold from count >= 50 to count >= 57 [I] (100% SETBIT coverage per user decision)
- [fix] PostLoop-UserFix iter2: AC#41 | Raised threshold from count >= 5 to count >= 12 (12 SOURCE mutations in うふふ==2 block per user decision)
- [fix] PostLoop-UserFix iter2: Upstream Issues | Added PALAMLV value ambiguity entry (PainStateChecker=authoritative, GameOptions=別目的)
- [fix] Phase2-Review iter3: AC#41 Details | Updated Expected from count >= 5 to count >= 12 (sync with Definition Table)
- [fix] Phase2-Review iter3: AC#44 Details | Updated Expected from count >= 50 to count >= 57 [I] (sync with Definition Table)
- [fix] Phase2-Review iter3: AC#46 | Removed SetSource|SetStain from matcher (setup operations, not assertions); updated rationale
- [fix] Phase4-ACValidation iter2: Task#11 | Added [I] tag to match AC#44's [I]-tagged Expected (57 SETBIT count needs ERB verification)
- [fix] Phase2-Review iter3: AC Definition Table | Added AC#79 (GetCharacterString read-back in CSTR tests gte=2); AC count updated to 79; Task#14 AC# updated; Goal Item 6 updated
- [fix] Phase2-Review iter4: AC#49 Precision Note | Corrected to document soft floor nature (non-dispatch enum references can inflate count); added combined constraint explanation with AC#20+AC#24+AC#6
- [fix] Phase2-Review iter5: AC Definition Table | Added AC#80 (HandleReactionCases call-chain verification); AC count updated to 80; Task#10 AC# updated; Goal Item 1 updated
- [fix] Phase2-Review iter6: AC#79 | Strengthened matcher from raw GetCharacterString to assertion-adjacent pattern (GetCharacterString.*Assert|Assert.*GetCharacterString|Equal.*GetCharacterString) to prevent stub body inflation
- [fix] Phase2-Review iter7: Task#11 + AC#78 + Technical Design | Corrected MCC character-specific writes: each CASE writes MASTER or ARG independently (not both). ERB-verified: some cases target MASTER only, others target ARG only. Prevented silent behavioral divergence.
- [resolved-applied] Phase2-Review iter8: AC#78 MASTER/ARG indistinguishability -- user decision: Precision Note added documenting soft floor nature. Task#11 + AC#25 provide combined constraint.
- [fix] PostLoop-UserFix iter8: AC#78 Precision Note | Added MASTER/ARG indistinguishability limitation and combined constraint (Task#11 + AC#25) documentation
- [fix] Phase2-Review iter9: AC#41 Details | Corrected rationale from '12 SOURCE mutations' to '6 SOURCE mutations + other operations x 2+ keyword refs = 12+ total'
- [fix] Phase2-Review iter10: Technical Design | Fixed TEquipIndex.VSex to EquipmentIndex.VSex (TEquipIndex is non-existent type; actual type is EquipmentIndex static class)
- [fix] Phase2-Review iter1: Background | Moved Source File table from unheaded position under ## Background into ### Problem subsection (template compliance)
- [fix] Phase2-Review iter1: Philosophy | Added CFLAG/EQUIP index exclusion to Philosophy text parallel to CSTR exclusion (consistency with Key Decisions)
- [resolved-applied] Phase2-Pending iter1: AC#78 + AC#25 | AC#78 Precision Note acknowledges MASTER/ARG indistinguishability; AC#25 strengthened from matches to gte=3 to enforce both MASTER and ARG character test paths.
- [fix] PostLoop-UserFix iter2: AC#25 | Strengthened matcher from matches to gte=3 (MASTER/ARG dual-character MCC test coverage enforcement)
- [resolved-applied] Phase2-Uncertain iter2: AC#41 | Extended pattern to include SetSource|GetSource; gte=12 now justified by 6 SOURCE mutations + 5 pre-check keywords.
- [fix] PostLoop-UserFix iter2: AC#41 | Extended matcher pattern from 5 to 7 keywords (added SetSource|GetSource for SOURCE mutation assertion coverage)
- [fix] Phase2-Review iter1: Technical Design | Fixed stale AC#44 threshold reference from '>= 50' to '>= 57 [I]' (sync with AC Definition Table)
- [fix] Phase2-Review iter1: AC#68 Details | Fixed stale rationale 'gte=1' → 'gte=2' (sync with Definition Table matcher)
- [fix] Phase2-Review iter2: Philosophy + Goal Item 6 | Narrowed CSTR exclusion from 'CSTR string accumulation' to 'CSTR runtime storage'; Goal Item 6 clarified as 'interface strategy resolved (runtime storage deferred to F813)'
- [resolved-applied] Phase2-Review iter3: AC#24 | Matcher used SourceIndex category names (Humiliation, Sadism, Pleasure) instead of CounterActionId enum members; cross-contamination risk where SourceIndex references in test assertions satisfy matcher without testing dispatch branches. Critical severity.
- [fix] Phase2-Review iter3: AC#24 | Replaced 7 SourceIndex category names with actual CounterActionId enum members (SeductiveGesture, ButtTorment, Kiss, Masturbation, ForcedCunnilingus, MissionaryInsert, VirginOffering) to prevent cross-contamination
- [fix] Phase2-Uncertain iter4: Goal Item 8 + Goal text | Qualified 'branch-level behavioral parity' → 'behavioral parity via category-representative tests (7 categories covering all ~36 dispatch branches)' to align with AC#24 category-level design
- [resolved-applied] Phase2-Review iter3: AC#24 | SourceIndex cross-contamination fixed by using CounterActionId enum members. Now further refined in iter5.
- [fix] Phase2-Review iter5: AC#24 | Replaced ButtTorment(21)/VirginOffering(40) with MilkHandjob(29) — ButtTorment and VirginOffering have no CASE branches in COUNTER_SOURCE.ERB. Removed 40-47 range (0 branches). Reduced gte=7 → gte=6 (6 actual dispatch ranges).
- [fix] Phase2-Review iter6: AC Definition Table | Added AC#81 (CheckExpUp gated path: EXP not incremented when CheckExpUp returns false, gte=2); Task#14 AC# updated; Goal Item 8 updated
- [fix] Phase2-Review iter7: Goal Coverage | Moved AC#53 from Goal Item 1 to Goal Item 8 (test AC belongs in test goal, not production dispatch goal)
- [fix] Phase2-Review iter7: AC#22 | Added Precision Note documenting boundary-distinction limitation and combined constraint (Task#14 + C4)
- [fix] Phase2-Review iter1: Review Notes | Changed stale [pending] to [resolved-applied] for iter3 AC#24 SourceIndex cross-contamination
- [fix] Phase2-Review iter1: AC Coverage row 24 | Updated from 7 SourceIndex categories to 6 CounterActionId enum members matching AC#24 matcher
- [fix] Phase2-Review iter1: AC Coverage table | Added missing AC#81 row (CheckExpUp gated path How-to-Satisfy)
- [fix] Phase2-Review iter2: AC#41 | Removed inflatable SetSource|GetSource from matcher; narrowed to C6-specific keywords only (gte=5)
- [fix] Phase2-Uncertain iter2: AC Definition Table | Added AC#82 (IEngineVariables MASTER resolution behavioral test gte=2); AC count updated to 82; Task#14 AC# updated; Goal Item 7 updated
- [fix] Phase2-Review iter2: AC Coverage table | Added AC#82 row (IEngineVariables MASTER resolution How-to-Satisfy)
- [fix] Phase2-Review iter3: AC#41 Details | Synced with Definition Table: removed SetSource|GetSource, updated to 5 keywords gte=5 and updated rationale
- [fix] Phase2-Review iter4: Goal Coverage | Moved AC#82 from Goal Item 7 to Goal Item 8; added AC#82 to Philosophy 'testable, maintainable' row
- [fix] Phase2-Review iter4: Philosophy Derivation | Moved AC#53 from dispatch row to 'testable, maintainable' row (consistent with Goal Coverage iter7 move)
- [resolved-applied] Phase2-Review iter5: AC#41/C6 うふふ==2 STAIN assertion gap — AC#45 strengthened from matches to gte=3 per user decision (Option A)
- [fix] PostLoop-UserFix iter5: AC#45 | Strengthened matcher from matches to gte=3 (うふふ==2 STAIN bitwise OR + dispatch branch coverage); AC Details updated
- [fix] Phase2-Review iter1: Philosophy Derivation | Added AC#81 to 'testable, maintainable' row; added AC#80 to dispatch row; added AC#42, AC#79 to CSTR row; updated CSTR text to ICharacterStringVariables
- [fix] Phase4-ACLint iter2: AC#82 Details | Added missing AC Details block for AC#82 (IEngineVariables MASTER resolution behavioral test)
- [fix] Phase3-Maintainability iter3: CheckExpUp parameter order | Aligned ICounterUtilities.CheckExpUp from (int, CharacterId) to (CharacterId, int) matching AbilityGrowthProcessor.CheckExpUp — prevents F813 breaking change
- [fix] Phase2-Review iter4: AC#25 Details | Added Precision Note documenting MASTER/ARG test-side indistinguishability (symmetrical with AC#78 production-side gap)
- [fix] Phase2-Review iter4: AC#76 Details | Added Precision Note documenting helper extraction threshold assumption and file-wide Grep scope
- [fix] Phase2-Review iter1: Philosophy Derivation | Replaced Goal-text Absolute Claims (rows 5-10) with Philosophy-text claims per template mandate
- [fix] Phase2-Review iter1: Feasibility Assessment | Changed CSTR row Assessment from NEEDS_REVISION to FEASIBLE (ICharacterStringVariables strategy resolved in AC#12)
- [fix] Phase2-Review iter1: Acceptance Criteria | Added missing <!-- Written by: ac-designer (Phase 3) --> ownership comment
- [resolved-applied] Phase2-Pending iter1: AC#32/AC#44 file-scoped matchers do not enforce UpdateMasterCounterControl method scope — STAIN |= operations can satisfy AC#32; non-MCC 1 << can inflate AC#44 count. → User decision: A) Precision Note + 複合制約で受容。AC#25/AC#77がbehavioral testで補完。
- [fix] PostLoop-UserFix iter7: AC#32/AC#44 Precision Notes | Updated to document file-scope limitation as known accepted constraint; added AC#25+AC#77 combined constraint references
- [resolved-applied] Phase2-Pending iter1: No not_contains AC verifying CounterSourceHandler.cs does not reference ERB interpreter types (EmuEra, GlobalStatic, Interpreter, ErbScript). Philosophy claims 'independent of the legacy ERB interpreter' but no AC enforces this constraint. → AC#83 added.
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#83 (not_contains EmuEra/GlobalStatic/Interpreter/ErbScript in CounterSourceHandler.cs); AC count updated to 83; Task#15 AC# updated; Goal Item 8 updated; Philosophy 'testable, maintainable' row updated
- [fix] Phase2-Uncertain iter2: AC#75 | Raised threshold from gte=10 to gte=19 [I] (19 confirmed CSTR write sites in COUNTER_SOURCE.ERB)
- [fix] Phase2-Uncertain iter2: AC#76 | Raised threshold from gte=10 to gte=25 [I] (25 confirmed active TOUCH_SET calls in COUNTER_SOURCE.ERB; 3 commented-out excluded)
- [fix] Phase2-Review iter3: Philosophy Derivation | Synced stale 'CSTR string accumulation' → 'CSTR runtime storage' in rows 300/308 (Philosophy line 25 already corrected in prior /fl)
- [fix] Phase2-Review iter3: AC#83 | Widened scope from CounterSourceHandler.cs to Era.Core/Counter/ directory (covers all new F803 files including interfaces)
- [fix] Phase2-Review iter4: AC Definition Table | Added AC#84 (ICharacterStringVariables.cs not_contains ERB types) + AC#85 (うふふ==2 STAIN co-location assertion); AC count updated to 85; Task#14/15 AC# updated; Goal Item 8 + Philosophy 'testable, maintainable' row updated
- [fix] Phase2-Review iter5: Mandatory Handoffs | Added IShrinkageSystem runtime implementation tracking → F813 (same deferred pattern as ICharacterStringVariables)
- [fix] Phase3-Maintainability iter6: F813 Deferred Obligations | Propagated IShrinkageSystem runtime implementation (#6) from F803 Mandatory Handoffs to F813
- [resolved-applied] Phase2-Uncertain iter7: AC#42 (gte=3) may be insufficient to verify both CSTR patterns independently. → User decision: A) AC#42 gte=3→6に引き上げ（各パターン×3参照で両パターンカバレッジ保証）
- [fix] PostLoop-UserFix iter7: AC#42 | Raised threshold from gte=3 to gte=6 (3 per CSTR pattern: player-accumulates-target + target-accumulates-player); updated AC Details rationale
- [fix] Phase2-Review restart-iter1: Technical Design | Fixed stale AC count 82 → 85 and AC#75/AC#76 thresholds gte=10 → gte=19 [I]/gte=25 [I] in AC summary paragraph
- [fix] Phase2-Review restart-iter2: 締り具合変動 call count | Corrected from 5 to 8 in Technical Design, AC#19/AC#30 Details, Key Decisions (ERB verified: 8 sites at lines 25,238,265,426,453,482,509,536)
- [fix] Phase2-Review restart-iter2: AC Definition Table | Added AC#86 (UpdateShrinkage call-site count gte=8 [I]); AC count updated to 86; Task#10 AC# updated; Goal Item 7 + Philosophy 'interface-driven DI' row updated
- [fix] Phase2-Review iter1: AC Definition Table | Moved [I] tag from Expected column to Description column for AC#44, AC#75, AC#76, AC#86 (template compliance: [I] tag defined for Tasks table only)
- [fix] Phase2-Review iter2: AC#41 Details | Added Naming Constraint note requiring English keywords (EarlyReturn, PreCheck, Ufufu, SpecialPath) in test method names
- [fix] Phase2-Review iter3: Task#14 | Added AC#41 naming constraint note (English keywords EarlyReturn/PreCheck/Ufufu/SpecialPath mandatory for C6 tests)
- [resolved-skipped] Phase2-Uncertain iter1: AC#24/AC#49/AC#50 dispatch branch coverage gap — user decision: 代表テスト戦略 + 集約カウンタ（AC#49/AC#50/AC#53）で十分。残り30ブランチの個別ACは不要
- [fix] Phase2-Uncertain iter1: Goal Coverage | Moved AC#41 from Goal Item 1 to Goal Item 8 (AC#41 is a test AC for pre-check guard, not a dispatch branch)
- [fix] Phase2-Uncertain iter1: Goal Coverage | Added shrinkage behavioral parity deferral note to Goal Item 8 (matching Goal Item 6 CSTR deferral pattern)
- [fix] Phase2-Review iter1: AC Definition Table | Removed [I] tag from AC#44, AC#75, AC#76, AC#86 Description column (template compliance: [I] tag is defined for Tasks table only, not AC table; AC#44 threshold 57 verified against ERB SETBIT count)
- [resolved-skipped] Phase2-Uncertain iter1: AC#41 English naming constraint -- user decision: Grep matcher requires naming convention. Precision Note documents constraint. No change needed
- [resolved-skipped] Phase2-Pending iter1: Goal Item 8 shrinkage behavioral parity gap -- user decision: IShrinkageSystem is stub-only, deferred to F813 via Mandatory Handoffs. Stub behavioral tests have no value
- [fix] Phase2-Review iter2: AC Definition Table | Added AC#87 (うふふ==2 CFLAG precondition verification in tests); AC count 86→87; Task#14 AC# updated; Philosophy/Goal/AC Coverage rows updated
- [resolved-skipped] Phase2-Uncertain iter2: TCVarIndex 8 of 10 new constants lack individual value-verification ACs -- user decision: [I] tag + Upstream Issues documentation sufficient. Implementer verifies against DIM.ERH/CSV at implementation time
- [fix] Phase4-ACLint iter3: Technical Design | Fixed stale AC count 86 → 87 in Technical Design summary paragraph (line 941)
- [fix] Phase2-Review iter4: AC#87 | Strengthened matcher: added multiline co-location constraint (CFLAG setup within 800 chars of HandleCounterSource invocation); updated Description, Details block with Precision Note
- [fix] Phase2-Review iter5: AC#42 | Removed inflatable sub-pattern `CharacterString.*Assert` (matches DI assertions, stub declarations); narrowed to `SetCharacterString|CSTR.*Equal` gte=6; added Precision Note documenting removal rationale
- [fix] Phase2-Review iter5: AC#53 | Added Precision Note documenting soft-floor nature of bare `CounterActionDerivation` pattern gte=10; cross-referenced companion ACs (AC#59, AC#62, AC#65, AC#81) for behavioral verification
- [fix] Phase2-Review iter6: AC#50 Details | Corrected stale "12 SOURCE values" to "6 SOURCE values" (verified: COUNTER_SOURCE.ERB:10-15 has 6 SOURCE mutations in うふふ==2 block)
- [fix] Phase2-Review iter6: AC#42 Details | Added Precision Note documenting per-pattern distinguishability limitation; added implementer guidance requiring both CSTR directions tested
- [resolved-skipped] Phase2-Uncertain iter7: AC#42 bidirectional CSTR enforcement gap -- user decision: AC#42 gte=6 + AC#79 gte=2 + Task description sufficient. Grep cannot distinguish CSTR direction
- [fix] Phase2-Review iter7: AC#4/AC#48 Details | Added Value Verification Notes documenting individual value-verification gap for StainIndex/ExpIndex (symmetric with TCVarIndex [pending])
- [fix] Phase2-Review iter7: AC#53 Details | Strengthened Precision Note: documented sub-case 502/503/504 lack individual spot-check ACs; added implementer guidance for per-sub-case test coverage
- [fix] Phase2-Review iter8: Technical Design | Fixed KissExp SSOT inconsistency: code block new(8) → new(27) (CSV-verified per Review Notes line 1564; Task#3 already shows KissExp=27)
- [fix] Phase2-Review iter9: AC#26 Details | Added ERB citation confirming CASE 501 intentional absence (COUNTER_SOURCE.ERB lines 539-631, sequence 500→502 with no 501 handler)
- [fix] Phase2-Review iter9: AC#81 Details | Added Precision Note documenting file-wide matcher limitation without co-location; justified rejection of multiline co-location (stub setup vs assertion separation); cross-referenced mitigating ACs (AC#80, AC#53)
- [fix] Phase2-Review iter10: AC#71 | Raised threshold from gte=5 to gte=7 (accounts for CounterSourceHandlerTests.cs CheckExpUp references from AC#81 gated-path tests inflating directory-scoped count; 7 = 5 stubs + 2 minimum CSH test references)
- [resolved-applied] Phase3-Maintainability iter10: TCVarIndex baseline stale (10→16) + 6 duplicate constants (SubAction/UndressingPlayer*/UndressingTarget*/SixtyNineTransition already exist) + CounterActionDerivation vs SubAction alias naming conflict (same index 21). Decisions needed: (A) reuse existing F801/F812 constant names in F803 code + update AC#52/AC#53 matchers from CounterActionDerivation to SubAction, OR (B) rename F802's constants to F803's preferred names (breaking change), OR (C) accept both names coexisting. Task#6 description and Technical Design code block must be updated to reflect only 4 truly new constants. → PostLoop-UserFix: reuse F801/F812 constant names (SubAction etc), update Task#6/Technical Design/AC matchers
- [resolved-applied] Phase3-Maintainability iter10: ICounterSourceHandler ISP extraction (Mandatory Handoffs → F813 item #3) will affect F805 (Successor) — F805 consumes DatUI helpers via ICounterSourceHandler. When ISP extracts IUndressingHandler, F805 must update its injection. This cross-feature impact is not documented in F813's deferred obligations. → PostLoop-UserFix: added F805 DI impact to F813 deferred obligations
- [fix] Phase3-Maintainability iter10: Task#17 | Added PainStateChecker regression verification note (verify existing tests pass after PALAMLV extraction, covered by Task#15 dotnet test run)
- [fix] Phase3-Maintainability iter10: Upstream Issues | Added TCVarIndex 6 duplicate constants entry documenting stale baseline (10→16) and naming conflicts with existing F801/F812 constants
- [fix] Phase2-Review iter1: Philosophy + Philosophy Derivation | Added IShrinkageSystem runtime storage to SSOT exclusion list (alongside CSTR/CFLAG/EQUIP); verified against Mandatory Handoffs item #6
- [fix] Phase2-Uncertain iter2: AC#41 | Removed inflatable CounterDecisionFlag from matcher (non-C6-exclusive: appears in any test arrange step); reduced gte=5 → gte=4 retaining 4 C6-exclusive keywords (EarlyReturn, PreCheck, Ufufu, SpecialPath)
- [resolved-applied] Phase2-Review iter2: AC#53 sub-cases 502/503/504 lack individual behavioral spot-check ACs. gte=10 soft floor satisfiable by exhaustive case 500 + case 505 testing alone. Conflicts with prior user decision (line 1695: 代表テスト戦略で十分、残りブランチ個別AC不要). Reviewer proposes adding 502/503/504 spot-checks or extending AC#53 matcher. → PostLoop-UserFix: added AC#88/AC#89/AC#90 spot-check ACs for cases 502/503/504
- [fix] Phase3-Maintainability iter3: Key Decisions | Added CounterActionId 40-47 dispatch absence documentation (ERB-verified: no CASE branches for values 40-47 in COUNTER_SOURCE.ERB:27-632; intentional, not translation omission)
- [fix] Phase3-Maintainability iter3: Task#14 | Added うふふ==2 SOURCE mutations guidance: test MUST assert all 6 SOURCE values (SexualActivity, Conquest, Liquid, Sadism, Coercion, GivePleasureV) — AC#50 counts file-wide but does not enforce per-path coverage
- [fix] Phase2-Review iter4: AC#87 | Replaced unsatisfiable matcher `SetCharacterFlag.*うふふ.*2` with `CharacterFlagIndex.*317|CFlagUfufu.*317` (codebase uses raw int 317 for CFLAG うふふ, not Japanese string literal; matches ActionSelector/ComableChecker precedent)
- [fix] Phase2-Review iter4: Task#14 | Extended うふふ==2 block guidance to include non-SOURCE side effects: PainCheckVMaster invocation, EXP:Ｖ性交経験 increment, TCVAR:EjaculationLocationFlag/EjaculationPleasureIntensity assertions, STAIN bitwise OR (AC#85), CSTR append (AC#42)
- [resolved-applied] Phase2-Review iter5: うふふ==2 block TCVAR assertions (EjaculationLocationFlag=1, EjaculationPleasureIntensity+=400) and EXP:Ｖ性交経験 increment have no dedicated AC. AC#70 (GetExp|SetExp gte=3) is file-wide. Task#14 guidance documents requirement but no AC matcher enforces co-location with Ufufu test path. Consider adding AC#88 multiline co-location or strengthening AC#41/AC#70. → PostLoop-UserFix: added AC#91 multiline co-location for TCVAR/EXP with Ufufu context
- [fix] Phase2-Review iter5: AC#24 | Changed matcher from bare enum names to fully-qualified `CounterActionId.X` format (prevents cross-contamination: bare Kiss matches KissExp/VirginityType.Kiss, bare Masturbation matches MasturbationExp/AC#65 case 505 tests)
- [resolved-applied] Phase2-Review iter6: うふふ==2 block SOURCE assertions (6 mutations: SexualActivity, Conquest, Liquid, Sadism, Coercion, GivePleasureV) have no co-located AC. AC#50 (gte=20) is file-wide. Task#14 guidance mandates asserting all 6 but no AC enforces co-location with Ufufu test path. Consider AC#88 multiline co-location or extending AC#41 to include SOURCE constants. → PostLoop-UserFix: added AC#92 multiline co-location for SOURCE with Ufufu context
- [fix] PostLoop-UserFix iter6: Task#6 + Technical Design + AC#52/AC#53 | Updated TCVarIndex baseline from 10→16; replaced CounterActionDerivation with SubAction (F801-established name at index 21); reduced truly-new constants count to reflect F801/F812 additions
- [fix] PostLoop-UserFix iter6: F813 deferred obligations | Added F805 DI injection impact note to ISP extraction item (IUndressingHandler extraction will require F805 to update its ICounterSourceHandler consumption)
- [fix] PostLoop-UserFix iter6: AC#88/AC#89/AC#90 + Task#88/89/90 | Added individual behavioral spot-check ACs for reaction cases 502 (手淫), 503 (フェラチオ), 504 (パイズリ); updated AC#53 Precision Note, Goal Coverage, AC Coverage, Approach section
- [fix] PostLoop-UserFix iter6: AC#91 + Task#91 | Added うふふ==2 TCVAR/EXP co-location spot-check AC (multiline 1200-char window: Ufufu context + EjaculationLocationFlag/EjaculationPleasureIntensity/VSexExp/GetExp/SetTCVar)
- [fix] PostLoop-UserFix iter6: AC#92 + Task#92 | Added うふふ==2 SOURCE co-location spot-check AC (multiline 1200-char window: Ufufu context + GetSource/SetSource/SourceIndex); completes 4-mutation symmetry with AC#85 (STAIN), AC#87 (CFLAG), AC#91 (TCVAR/EXP)
- [fix] Phase2-Review PostLoop-restart iter1: AC#91/AC#92 | Fixed trivially-satisfiable regex patterns: added parentheses to group alternations correctly for multiline co-location enforcement (bare alternations matched independently without co-location)
- [fix] Phase2-Review PostLoop-restart iter1: AC#43 | Updated description from "10 new constants" to "4 truly new constants" (consistency with Task#6 baseline correction)
- [fix] Phase3-Maintainability PostLoop-restart iter1: Baseline Measurement | Updated TCVarIndex baseline from 10→16 with full F801/F812 constant list (8 additions); Note column now documents AC#43 derivation (16+4=20)

---

## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning (parent decomposition)
- [Predecessor: F801](feature-801.md) - Counter Core (CounterActionId, TCVAR:CounterAction)
- [Successor: F805](feature-805.md) - Toilet Counter Source (consumes DATUI_*/PainCheckVMaster public helpers)
- [Successor: F813](feature-813.md) - Post-Phase Review Phase 21
- [Related: F802](feature-802.md) - Sibling Phase 21 feature, no direct call dependency
- [Related: F811](feature-811.md) - Source/Source Pose (bidirectional: F803 calls TOUCH_SET via ITouchSet; F811 calls EVENT_COUNTER_SOURCE)
- [Related: F812](feature-812.md) - Sibling Phase 21 feature, no direct call dependency
- [Related: F789](feature-789.md) - String Variables Pattern (SAVESTR precedent for ICharacterStringVariables)
- [Related: F815](feature-815.md) - Provides StubVariableStore test infrastructure

