# Feature 822: Pregnancy System Migration

## Status: [DONE]
<!-- fl-reviewed: 2026-03-05T07:59:45Z -->

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
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)

Phase 22: State Systems -- All ERB state subsystems must be migrated to C# with ISP-compliant interfaces, equivalence-tested against ERB baselines, achieving zero-debt implementation. The pregnancy subsystem (PREGNACY_S.ERB, PREGNACY_S_CHILD_MOVEMENT.ERB, PREGNACY_S_EVENT.ERB, PREGNACY_S_EVENT0.ERB; 2,534 lines total) is the SSOT for pregnancy lifecycle, child birth/growth, name generation, child movement events, and pregnancy events within this phase.

### Problem (Current Issue)

PREGNACY_S.ERB (2,372 lines, ~25 functions) contains at least 5 distinct domain concerns -- pregnancy announcement/NTR messaging, birth process, child growth/talent acquisition, name generation data (~1,300 lines of lookup tables, 55% of file), and milking -- forced into a single file because ERB has no module system. This violates SRP and the F808 lesson (obligation #32) which mandates domain decomposition analysis during /fc. The spec lists 3 external ERB dependencies but code analysis reveals 12+ external function calls including N_T_R display helpers, NTR_CHK_FAVORABLY, HAS_PENIS, GET_PRIVATE_ROOM, GET_N_IN_ROOM, and TIME_PROGRESS. The largest interface gap is IMultipleBirthService (8 methods from 多生児パッチ.ERB with no C# equivalent), followed by missing IVariableStore accessor for the 2D SAVEDATA variable `親別出産数`, and user-interactive INPUT/INPUTS loops (PREGNACY_S.ERB:241-248, 521-543) that require an abstraction layer for testability. Existing interfaces (IPregnancySettings, IGeneticsService, ICalendarService, IMenstrualCycle, INtrQuery) cover their respective call sites but are insufficient for the full migration scope.

**Deferred from F824**: 妊娠処理変更パッチ.ERB:278 resets CFLAG:生理周期=0 on conception. F824 provides `IMenstrualCycle.ResetCycle(int characterId)` as the integration contract. Pregnancy code must call `ResetCycle()` instead of directly writing CFLAG:生理周期.

### Goal (What to Achieve)

Migrate all four pregnancy ERB files to C# with mandatory domain decomposition into separate classes per F808 lesson (not 1:1 ERB-to-class). Create IMultipleBirthService stub interface (8 methods) for 多生児パッチ.ERB dependency. Implement name generation as a data-driven component (strategy for ~1,300 lines of lookup tables). Provide user INPUT abstraction for interactive loops. Integrate with existing Era.Core interfaces (ICalendarService, IGeneticsService, IMenstrualCycle, INtrQuery). Achieve zero-debt implementation with equivalence tests against ERB baseline.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is PREGNACY_S.ERB migration complex? | 2,372 lines with 5+ distinct domains forced into single file | PREGNACY_S.ERB lines 13-2372: announcement, birth, growth, naming, milking |
| 2 | Why are domains mixed in one file? | ERB has no module system; functions are grouped by file prefix convention only | ERB flat namespace -- no classes, no namespaces |
| 3 | Why does this matter for C# migration? | 1:1 ERB-to-class mapping would create a monolithic class violating SRP, per F808 lesson | F808 obligation #32 -- domain decomposition mandatory |
| 4 | Why is the external dependency surface understated? | Spec lists 3 ERB files but code calls 12+ external functions across NTR, location, time systems | PREGNACY_S.ERB calls N_T_R, NTR_CHK_FAVORABLY, HAS_PENIS, GET_PRIVATE_ROOM, etc. |
| 5 | Why (Root)? | The combination of (a) mandatory multi-class decomposition, (b) IMultipleBirthService gap (8 methods with no C# equivalent), (c) ~1,300 lines of name data requiring data-driven design, and (d) user INPUT loops requiring testability abstraction creates a scope that the current 3-task spec cannot cover | No IMultipleBirthService in Era.Core; PREGNACY_S.ERB:1049-2351 name data; PREGNACY_S.ERB:241-248 INPUT loops |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Feature has 3 stub tasks for 2,534 lines of ERB code | ERB flat namespace hides 5+ separable domains requiring independent C# classes, plus 8-method interface gap and data-externalization decision |
| Where | Feature-822.md Tasks table | PREGNACY_S.ERB structural complexity + missing IMultipleBirthService + name data volume |
| Fix | Add more tasks to existing spec | Domain decomposition analysis, IMultipleBirthService stub creation, name data strategy, INPUT abstraction layer |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F814 | [DONE] | Phase 22 Planning -- decomposition and obligation triage |
| F821 | [DONE] | Weather System -- provides ICalendarService.ChildTemperatureResistance called at PREGNACY_S.ERB:426 |
| F824 | [DONE] | Sleep & Menstrual -- provides IMenstrualCycle.ResetCycle integration contract for conception |
| F808 | [DONE] | SRP lesson -- obligation #32 mandates domain decomposition during /fc |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Existing interface coverage | FEASIBLE | IPregnancySettings, IGeneticsService, ICalendarService, IMenstrualCycle, INtrQuery all exist with required methods |
| IMultipleBirthService gap | NEEDS_REVISION | 8 methods from 多生児パッチ.ERB have no C# interface -- must create stub |
| Domain decomposition | NEEDS_REVISION | 5+ domains require multi-class design; current spec has no decomposition |
| Name data volume | FEASIBLE | ~1,300 lines are pure lookup tables -- low algorithmic complexity, needs data-driven strategy |
| User INPUT abstraction | NEEDS_REVISION | PREGNACY_S.ERB:241-248, 521-543 have interactive loops with no existing C# abstraction |
| Predecessor availability | FEASIBLE | F821 [DONE], F814 [DONE], F824 [DONE] -- all predecessors complete |

**Verdict**: NEEDS_REVISION

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core interfaces | HIGH | New IMultipleBirthService interface (8 methods) + null implementation required |
| Era.Core State classes | HIGH | Multiple new classes from domain decomposition (pregnancy lifecycle, birth, growth, naming, movement, events) |
| Test infrastructure | MEDIUM | User INPUT abstraction needed for deterministic testing of interactive loops |
| Downstream features | MEDIUM | F822 pregnancy interfaces will be consumed by future features in the pregnancy domain |
| Name data management | LOW | ~1,300 lines of lookup tables -- data-driven approach keeps code volume manageable |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| F808 obligation #32: domain decomposition mandatory | F808 lesson, F814 planning | Cannot create single monolithic PregnancySystem class |
| IMenstrualCycle.ResetCycle contract from F824 | F824 Goal item 5 | Must call ResetCycle() on conception, not directly write CFLAG:生理周期 |
| TRYCALL vs CALL distinction | ERB call semantics | TRYCALL targets = no-op stubs acceptable; CALL targets = mandatory stub with defined behavior |
| IMultipleBirthService does not exist | Era.Core interface scan | Must create new interface + null/default implementation |
| 親別出産数 2D SAVEDATA variable | IVariableStore interface scan | No accessor exists; must extend or work around |
| Interactive INPUT/INPUTS loops | PREGNACY_S.ERB:241-248, 521-543 | Requires IUserInput or callback abstraction for testability |
| ADDCHARA engine call | PREGNACY_S.ERB:356-360 | ICharacterManager.AddChara exists -- verify compatibility |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Domain decomposition boundaries unclear | MEDIUM | HIGH | Tech-designer performs dependency clustering analysis per F808 lesson |
| Name data externalization increases file count | LOW | LOW | Data-driven approach (embedded resource or static arrays) -- tech-design decision |
| IMultipleBirthService stub insufficient for runtime | LOW | MEDIUM | Stub returns safe defaults; runtime implementation deferred to future feature |
| INPUT abstraction introduces API surface not used elsewhere | MEDIUM | LOW | Design minimal interface (IUserInput with single method); reusable for other interactive ERB migrations |
| External dependency surface larger than documented | MEDIUM | MEDIUM | AC Design Constraints encode all 12+ external calls for tech-designer verification |
| 親別出産数 accessor requires IVariableStore changes | LOW | MEDIUM | May use existing generic accessor if available; otherwise extend interface |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Source ERB line count | `wc -l PREGNACY_S*.ERB` | 2,534 | 4 files total |
| PREGNACY_S.ERB function count | `grep -c "^@" PREGNACY_S.ERB` | ~25 | Approximate from investigation |
| Name data lines | Lines 1049-2351 in PREGNACY_S.ERB | ~1,300 | 55% of main file |
| External call count | `grep -c "CALL\|TRYCALL" PREGNACY_S.ERB` | 12+ | Includes N_T_R, NTR calls |

**Baseline File**: `_out/tmp/baseline-822.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Domain decomposition required -- 5+ classes from PREGNACY_S.ERB | F808 obligation #32 | ACs must verify separate class files exist, not single monolithic class |
| C2 | IMultipleBirthService stub (8 methods) | Interface Dependency Scan | AC must verify interface exists with method count (gte 8), plus null/default implementation |
| C3 | IMenstrualCycle.ResetCycle integration | F824 contract | AC must verify pregnancy code calls ResetCycle, not direct CFLAG write |
| C4 | Name data (~1,300 lines) handling strategy | PREGNACY_S.ERB:1049-2351 | AC must verify name generation works without embedding 1,300 lines in logic class |
| C5 | User INPUT abstraction | PREGNACY_S.ERB:241-248, 521-543 | AC must verify interactive loops use abstraction, not direct console I/O |
| C6 | TRYCALL targets as no-op stubs | ERB TRYCALL semantics | AC for TRYCALL-target stubs can accept no-op; CALL-target stubs need defined behavior |
| C7 | 12+ external function calls | Interface Dependency Scan | ACs must verify each external call is routed through an interface, not hardcoded |
| C8 | 親別出産数 2D SAVEDATA accessor | IVariableStore gap | AC must verify 2D variable access through typed interface |
| C9 | NTR display helpers (N_T_R) used 6 times | Interface Dependency Scan | AC must verify NTR name display uses INtrUtilityService, not direct ERB call |

### Constraint Details

**C1: Domain Decomposition**
- **Source**: F808 lesson obligation #32; all 3 investigators agree PREGNACY_S.ERB has 5+ separable domains
- **Verification**: Count distinct class files created from PREGNACY_S.ERB; verify constructor dependencies overlap <50% between classes
- **AC Impact**: ac-designer must create file_exists ACs for each decomposed class, not just one PregnancySystem.cs

**C2: IMultipleBirthService Stub**
- **Source**: 多生児パッチ.ERB has 8 functions called by PREGNACY_S.ERB with no C# interface in Era.Core
- **Verification**: `grep -r "IMultipleBirthService" Era.Core/` should find interface + registration
- **AC Impact**: ac-designer must verify interface method count (gte 8) and null/default implementation exists

**C3: IMenstrualCycle.ResetCycle Contract**
- **Source**: F824 provides ResetCycle(int characterId) -- pregnancy must call this on conception
- **Verification**: Grep pregnancy birth code for ResetCycle call
- **AC Impact**: Behavioral AC verifying ResetCycle is called during conception flow

**C4: Name Data Strategy**
- **Source**: PREGNACY_S.ERB lines 1049-2351 contain ~1,300 lines of Japanese name lookup tables
- **Verification**: Name generation class exists separately from pregnancy logic classes
- **AC Impact**: ac-designer must verify name data is in a dedicated class/resource, not mixed into lifecycle logic

**C5: User INPUT Abstraction**
- **Source**: PREGNACY_S.ERB:241-248 (INPUT for naming), 521-543 (INPUTS for naming)
- **Verification**: No direct Console.ReadLine in pregnancy code; uses IUserInput or callback
- **AC Impact**: AC must verify abstraction exists and is injected, enabling deterministic testing

**C6: TRYCALL vs CALL Stub Behavior**
- **Source**: ERB TRYCALL = optional (no-op if missing); CALL = mandatory
- **Verification**: Review each external call for TRYCALL vs CALL prefix
- **AC Impact**: CALL-target stubs need behavioral verification; TRYCALL-target stubs only need existence

**C7: External Function Coverage**
- **Source**: All 3 investigators found 10-12+ external calls beyond the 3 listed in original spec
- **Verification**: Grep PREGNACY_S.ERB for all CALL/TRYCALL/CALLFORM lines
- **AC Impact**: Each external call must route through a typed interface -- ACs for injection verification

**C8: 親別出産数 Variable Access**
- **Source**: IVariableStore has no accessor for this 2D SAVEDATA variable
- **Verification**: Check IVariableStore for 2D array accessors
- **AC Impact**: AC must verify typed access, not raw index manipulation

**C9: NTR Display Integration**
- **Source**: N_T_R display helper called 6 times in announcement/message functions
- **Verification**: INtrUtilityService.GetNtrName or equivalent exists
- **AC Impact**: AC must verify NTR name display uses service injection

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning -- decomposition and obligation triage |
| Predecessor | F821 | [DONE] | Weather System -- PREGNACY_S.ERB:426 CALL 子供気温耐性取得 requires ICalendarService.ChildTemperatureResistance (file: PREGNACY_S.ERB:426) |
| Related | F824 | [DONE] | Sleep & Menstrual -- provides IMenstrualCycle.ResetCycle contract for conception integration |
| Related | F808 | [DONE] | SRP lesson -- obligation #32 mandates domain decomposition |

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
| "All ERB state subsystems must be migrated to C# with ISP-compliant interfaces" | Each pregnancy domain has its own ISP-compliant interface/class | AC#1, AC#2 |
| "equivalence-tested against ERB baselines" | Equivalence tests exist for pregnancy lifecycle, birth, growth, naming, events | AC#14, AC#15 |
| "achieving zero-debt implementation" | No TODO/FIXME/HACK markers in pregnancy code | AC#16 |
| "The pregnancy subsystem (PREGNACY_S.ERB, PREGNACY_S_CHILD_MOVEMENT.ERB, PREGNACY_S_EVENT.ERB, PREGNACY_S_EVENT0.ERB; 2,534 lines total) is the SSOT" | All 4 ERB files migrated -- separate classes per domain, not 1:1 file mapping | AC#1, AC#3, AC#4, AC#5 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Domain decomposition: 5+ separate class files from PREGNACY_S.ERB (not monolithic) | code | Grep(Era.Core/State/, pattern="class.*Pregnancy|class.*Birth|class.*ChildGrowth|class.*NameGenerat|class.*Milking|class.*ChildMovement|class.*PregnancyEvent") | gte | 5 | [x] |
| 2 | IMultipleBirthService interface exists with 8+ methods | code | Grep(Era.Core/Interfaces/IMultipleBirthService.cs, pattern="^\s+(void|bool|int|Result)") | gte | 8 | [x] |
| 3 | IMultipleBirthService null implementation exists | code | Grep(Era.Core/, pattern="class.*NullMultipleBirth.*:.*IMultipleBirthService") | matches | `NullMultipleBirth.*IMultipleBirthService` | [x] |
| 4 | Name generation in dedicated class/resource, not in lifecycle logic | file | Glob(Era.Core/State/*Name*Gen*.cs) | exists | exists | [x] |
| 5 | Name data separated from pregnancy logic (data-driven, not inline in birth class) | code | Grep(Era.Core/State/*Birth*.cs, pattern="RANDOM_N_GIRL|RANDOM_N_BOY") | not_matches | `RANDOM_N_GIRL|RANDOM_N_BOY` | [x] |
| 6 | IInputHandler injected into BirthProcess for user input abstraction | code | Grep(Era.Core/State/BirthProcess.cs, pattern="IInputHandler") | matches | `IInputHandler` | [x] |
| 7 | Pregnancy code uses IInputHandler, not Console.ReadLine | code | Grep(Era.Core/State/, pattern="Console.ReadLine|Console.Read") | not_matches | `Console.ReadLine|Console.Read` | [x] |
| 8 | IMenstrualCycle.ResetCycle called in pregnancy conception code (F824 contract) | code | Grep(Era.Core/State/, pattern="ResetCycle") | matches | `ResetCycle` | [x] |
| 9 | No direct CFLAG:生理周期 write in pregnancy code | code | Grep(Era.Core/State/, pattern="生理周期.*=|SetCharacterFlag.*生理周期") | not_matches | `生理周期.*=` | [x] |
| 10 | 親別出産数 accessed through typed extension methods (GetBirthCountByParent/SetBirthCountByParent) | code | Grep(Era.Core/, pattern="GetBirthCountByParent|SetBirthCountByParent") | matches | `GetBirthCountByParent` | [x] |
| 11 | NTR display uses injected service (INtrQuery or INtrUtilityService), not direct ERB call | code | Grep(Era.Core/State/*Pregnancy*Announcement*.cs, pattern="INtrQuery|INtrUtility|_ntr") | matches | `INtrQuery|INtrUtility|_ntr` | [x] |
| 12 | External calls routed through interfaces: ICalendarService (子供気温耐性取得), IGeneticsService (体設定_遺伝), ICharacterManager (ADDCHARA), INtrUtilityService (N_T_R), ICommonFunctions (HAS_PENIS) | code | Grep(Era.Core/State/, pattern="ICalendarService|IGeneticsService|ICharacterManager|INtrUtilityService|ICommonFunctions") | gte | 5 | [x] |
| 13 | TRYCALL targets (生まれる人数, 里子, 私室戻しDEBUG, 多生児出産口上, 両親がMASTER以外の出産口上) use IMultipleBirthService or optional delegates | code | Grep(Era.Core/State/, pattern="IMultipleBirthService|_multipleBirth|multipleBirthService") | gte | 2 | [x] |
| 14 | Equivalence tests exist for pregnancy system (all 7 classes in Era.Core.Tests.State.Pregnancy namespace) | exit_code | dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~Pregnancy" --blame-hang-timeout 10s | succeeds | 0 | [x] |
| 15 | Build succeeds with zero warnings | build | dotnet build Era.Core/ | succeeds | 0 | [x] |
| 16 | No TODO/FIXME/HACK in pregnancy implementation files | code | Grep(Era.Core/State/*Pregnancy*.cs,Era.Core/State/*Birth*.cs,Era.Core/State/*ChildGrowth*.cs,Era.Core/State/*NameGen*.cs,Era.Core/State/*Milking*.cs,Era.Core/State/*ChildMovement*.cs,Era.Core/State/*PregnancyEvent*.cs, pattern="TODO|FIXME|HACK") | not_matches | `TODO|FIXME|HACK` | [x] |
| 17 | IMultipleBirthService registered in DI composition root | code | Grep(Era.Core/DependencyInjection/, pattern="IMultipleBirthService") | matches | `IMultipleBirthService` | [x] |
| 18 | IInputHandler registered in DI composition root | code | Grep(Era.Core/DependencyInjection/, pattern="IInputHandler") | matches | `IInputHandler` | [x] |
| 19 | Child movement domain class exists (from PREGNACY_S_CHILD_MOVEMENT.ERB) | file | Glob(Era.Core/State/*ChildMovement*.cs) | exists | exists | [x] |
| 20 | Pregnancy event domain class exists (from PREGNACY_S_EVENT*.ERB) | file | Glob(Era.Core/State/*PregnancyEvent*.cs) | exists | exists | [x] |

### AC Details

**AC#1: Domain decomposition: 5+ separate class files from PREGNACY_S.ERB**
- **Test**: `Grep Era.Core/State/ for class declarations matching pregnancy domain names`
- **Expected**: `>= 5 distinct class matches`
- **Derivation**: PREGNACY_S.ERB contains 5+ separable domains per F808 obligation #32: (1) announcement/NTR messaging, (2) birth process, (3) child growth/talent, (4) name generation, (5) milking. Plus PREGNACY_S_CHILD_MOVEMENT.ERB (child movement) and PREGNACY_S_EVENT*.ERB (events) = 7 domains minimum. Floor of 5 accounts for legitimate merging of closely-related domains.
- **Rationale**: C1 constraint -- domain decomposition mandatory, cannot create single monolithic class

**AC#2: IMultipleBirthService interface exists with 8+ methods**
- **Test**: `Grep Era.Core/Interfaces/IMultipleBirthService.cs for method signatures`
- **Expected**: `>= 8 method declarations`
- **Derivation**: 多生児パッチ.ERB provides 8 functions called by PREGNACY_S.ERB: 生まれる人数, 多生児フラグ処理, 多生児出産口上 (x2 call patterns), 里子, 両親がMASTER以外の出産口上, 子宮内体積設定, 子供相性設定. All 8 confirmed from CALL/TRYCALL analysis.
- **Rationale**: C2 constraint -- IMultipleBirthService gap must be filled with stub interface

**AC#12: External calls routed through interfaces**
- **Test**: `Grep Era.Core/State/ for interface type references`
- **Expected**: `>= 5 distinct interface references across pregnancy files`
- **Derivation**: 5 mandatory external interface dependencies: ICalendarService (子供気温耐性取得 at line 426), IGeneticsService (体設定_遺伝 at line 376, 体設定_子供髪変更 at line 706, 体設定_子供Ｐ成長 at line 707), ICharacterManager (ADDCHARA at line 356-360), INtrUtilityService (N_T_R at lines 20, 39, 513, 515), ICommonFunctions (HAS_PENIS at line 710). IInputHandler (INPUT at lines 241-248, 521-543) is verified separately by AC#6. GET_PRIVATE_ROOM/GET_N_IN_ROOM are CFLAG reads, not service calls.
- **Rationale**: C7 constraint -- all external calls must route through typed interfaces

**AC#13: TRYCALL targets use IMultipleBirthService**
- **Test**: `Grep Era.Core/State/ for IMultipleBirthService usage`
- **Expected**: `>= 2 references to IMultipleBirthService in pregnancy state classes`
- **Derivation**: TRYCALL targets from PREGNACY_S.ERB: 生まれる人数 (line 231), 里子 (line 281), 私室戻しDEBUG (line 308, 556), 多生児出産口上 (line 447, 465), 両親がMASTER以外の出産口上 (line 500). These are optional calls (TRYCALL = no-op OK per C6) routed through IMultipleBirthService. At minimum 2 classes (birth, child-birth) reference the service.
- **Rationale**: C6 constraint -- TRYCALL targets need existence via stub; C2 constraint -- routed through IMultipleBirthService

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Migrate all four pregnancy ERB files to C# with mandatory domain decomposition | AC#1, AC#19, AC#20 |
| 2 | Create IMultipleBirthService stub interface (8 methods) for 多生児パッチ.ERB dependency | AC#2, AC#3, AC#17 |
| 3 | Implement name generation as data-driven component | AC#4, AC#5 |
| 4 | Provide user INPUT abstraction for interactive loops | AC#6, AC#7, AC#18 |
| 5 | Integrate with existing Era.Core interfaces (ICalendarService, IGeneticsService, IMenstrualCycle, INtrQuery) | AC#8, AC#9, AC#11, AC#12 |
| 6 | Achieve zero-debt implementation with equivalence tests against ERB baseline | AC#14, AC#15, AC#16 |
| 7 | 親別出産数 2D SAVEDATA typed access | AC#10 |
| 8 | TRYCALL targets routed through IMultipleBirthService | AC#13 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The pregnancy system is decomposed into 7 domain classes from the 4 ERB source files, following the F808 SRP obligation. Each class has a focused responsibility and is injected with only the interfaces it needs — preventing the monolithic-class anti-pattern.

**Class map (ERB source → C# class):**

| C# Class | Source ERB | Domain |
|----------|-----------|--------|
| `PregnancyAnnouncement` | PREGNACY_S.ERB (lines 13–230) | NTR messaging, pregnancy announcement display |
| `BirthProcess` | PREGNACY_S.ERB (lines 231–560) | Birth orchestration, INPUT for naming, ADDCHARA |
| `ChildGrowth` | PREGNACY_S.ERB (lines 561–730) | Talent acquisition, genetics, calendar growth |
| `ChildNameGenerator` | PREGNACY_S.ERB (lines 1049–2351) | Name lookup tables (data-driven, ~1,300 lines) |
| `MilkingProcess` | PREGNACY_S.ERB (lines 731–1048) | Milking logic |
| `ChildMovement` | PREGNACY_S_CHILD_MOVEMENT.ERB | Child room movement |
| `PregnancyEventHandler` | PREGNACY_S_EVENT.ERB + PREGNACY_S_EVENT0.ERB | Pregnancy event dispatch |

**Interface strategy:**

- `IMultipleBirthService` — new stub interface (8 methods) for 多生児パッチ.ERB. `NullMultipleBirthService` returns safe defaults (int methods return 0, void methods no-op). Registered in DI as `AddSingleton<IMultipleBirthService, NullMultipleBirthService>`.
- `IInputHandler` — **already exists** (`Era.Core.Input.IInputHandler`). `BirthProcess` injects it for INPUT (naming) and INPUTS (string naming) loops. No new `IUserInput` interface needed; AC#6 and AC#7 are satisfied by verifying `IInputHandler` usage.
- `INtrUtilityService` — **already exists** (`Era.Core.Counter.INtrUtilityService`). `PregnancyAnnouncement` injects it for N_T_R name display (6 call sites, via `GetNtrName`). AC#9 (C9) is satisfied by injecting this existing service.
- `親別出産数` typed access — add `GetBirthCountByParent(int motherId, int fatherId)` and `SetBirthCountByParent(int motherId, int fatherId, int value)` to `IVariableStoreExtensions.cs` as extension methods over `IVariableStore`. The underlying SAVEDATA 2D array maps to the existing generic variable access path. Named constant `VariableDefinitions.SaveData_BirthCountByParent` already exists. AC#10 is satisfied by grepping for `BirthCountByParent`.
- `ILocationService` — `GET_PRIVATE_ROOM` and `GET_N_IN_ROOM` are local CFLAG reads implemented directly in `BirthProcess` per Key Decisions — no `ILocationService` extension needed. `GET_PRIVATE_ROOM` reads `CFLAG:私室`; `GET_N_IN_ROOM` loops over characters checking `CFLAG:現在地`. Both are trivial operations that don't warrant interface abstraction.
- `IMenstrualCycle.ResetCycle` — called in `BirthProcess` on conception to satisfy F824 contract. AC#8 verifies its presence, AC#9 (C3) verifies no raw CFLAG write.

**Name data strategy:** `ChildNameGenerator` uses `static readonly` arrays (one per name category: boy surname, girl surname, boy given name, girl given name). Arrays are initialized inline as C# collection literals in the class file — no external data file, no runtime I/O. This is the same pattern used in `GeneticsService` for `ValidSkinColors`. AC#4 verifies the dedicated class exists; AC#5 verifies birth class has no embedded name tables.

**Equivalence test approach:** Unit tests in `Era.Core.Tests/State/` exercise each class with known inputs, verifying outputs match documented ERB behavior from code comments. No headless game required for structural/unit tests. AC#14 filters by `FullyQualifiedName~Pregnancy`.

All 20 ACs are satisfied by this design. Key interface verification confirmed: `IInputHandler` (already exists), `INtrUtilityService.GetNtrName` (already exists), `IMenstrualCycle.ResetCycle` (confirmed in IMenstrualCycle.cs), `IGeneticsService.BodySettingsGenetics/BodySettingsChildPGrowth/BodySettingsChildHairChange` (confirmed), `ICalendarService.ChildTemperatureResistance` (confirmed), `ICharacterManager.AddChara` (confirmed), `ICommonFunctions.HasPenis` (confirmed).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create 7 class files in `Era.Core/State/`: `PregnancyAnnouncement.cs`, `BirthProcess.cs`, `ChildGrowth.cs`, `ChildNameGenerator.cs`, `MilkingProcess.cs`, `ChildMovement.cs`, `PregnancyEventHandler.cs`. Grep pattern matches class declarations in those files. |
| 2 | Create `Era.Core/Interfaces/IMultipleBirthService.cs` with 8 method signatures (GetBirthCount, SetMultiBirthFlags, DisplayBirthAnnouncement, DisplaySubsequentBirthAnnouncement, ProcessFosterChild, DisplayNonMasterParentAnnouncement, InitUterusVolumeMultiple, SetChildCompatibility). |
| 3 | Create `NullMultipleBirthService : IMultipleBirthService` in `Era.Core/Interfaces/` (same file as interface, sealed class pattern matching NullNtrQuery). |
| 4 | `ChildNameGenerator.cs` exists in `Era.Core/State/`. Glob `Era.Core/State/*NameGen*.cs` matches. |
| 5 | No name table arrays appear in `BirthProcess.cs` or other lifecycle classes. All string arrays live only in `ChildNameGenerator.cs`. |
| 6 | `BirthProcess.cs` injects `IInputHandler` (existing at `Era.Core/Input/IInputHandler.cs`). AC#6 greps `BirthProcess.cs` for `IInputHandler` — satisfied by constructor injection. Satisfies C5 constraint. |
| 7 | No `Console.ReadLine` or `Console.Read` in any `Era.Core/State/` pregnancy class. `BirthProcess` uses injected `IInputHandler`. |
| 8 | `BirthProcess.cs` calls `_menstrualCycle.ResetCycle(characterId)` when conception occurs. Grep `Era.Core/State/` for `ResetCycle` matches. |
| 9 | No `生理周期.*=` assignment in `Era.Core/State/` pregnancy classes. `ResetCycle` is the only write path. |
| 10 | `IVariableStoreExtensions.cs` gains `GetBirthCountByParent` and `SetBirthCountByParent` extension methods. Grep `Era.Core/` for `BirthCountByParent` matches. |
| 11 | `PregnancyAnnouncement.cs` injects `INtrUtilityService` as `_ntr` field. Grep for `_ntr` in that file matches. |
| 12 | Pregnancy classes collectively inject `ICommonFunctions`, `ICalendarService`, `IGeneticsService`, `ICharacterManager`, `INtrUtilityService` (5 distinct external-call interfaces). AC#12 grep pattern matches these 5 across `Era.Core/State/` pregnancy files. `IInputHandler` injection is separately verified by AC#6. |
| 13 | `BirthProcess.cs` and `ChildGrowth.cs` each reference `IMultipleBirthService` for TRYCALL-mapped optional operations. Grep for `IMultipleBirthService` in `Era.Core/State/` finds 2+ matches. |
| 14 | Tests in `Era.Core.Tests/State/Pregnancy/` namespace (`Era.Core.Tests.State.Pregnancy`) for all 7 classes. Filter `FullyQualifiedName~Pregnancy` catches all because namespace contains "Pregnancy". |
| 15 | All new classes compile without warnings under `TreatWarningsAsErrors=true`. |
| 16 | No TODO/FIXME/HACK in any of the 7 new class files. |
| 17 | `ServiceCollectionExtensions.cs` gains `AddSingleton<IMultipleBirthService, NullMultipleBirthService>()` in the "State Systems (Phase 22) - Feature 822" block. |
| 18 | `IInputHandler` is already registered as `AddSingleton<IInputHandler, InputHandler>()` in DI root. AC#18 greps DI for `IInputHandler` — satisfied by existing registration. |
| 19 | `ChildMovement.cs` exists in `Era.Core/State/`. Glob `Era.Core/State/*ChildMovement*.cs` matches. |
| 20 | `PregnancyEventHandler.cs` exists in `Era.Core/State/`. Glob `Era.Core/State/*PregnancyEvent*.cs` matches. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| IUserInput vs IInputHandler for INPUT abstraction | A: Create new `IUserInput` interface; B: Reuse existing `IInputHandler` | B: Reuse `IInputHandler` | `IInputHandler` at `Era.Core.Input.IInputHandler` already provides `RequestNumericInput` and `RequestStringInput` — exactly what PREGNACY_S.ERB INPUT/INPUTS loops need. Creating a duplicate interface is unnecessary debt. |
| NTR display interface | A: Create `INtrUtilityService.GetNtrName` extension; B: Use existing `GetNtrName` default method | B: Use existing default method | `INtrUtilityService.GetNtrName(int characterIndex)` already exists with a default stub returning `""`. `PregnancyAnnouncement` uses this directly — no new interface needed. |
| 親別出産数 accessor location | A: New dedicated interface `ISaveDataStore`; B: Extension methods on `IVariableStore` | B: Extension methods | Matches established `IVariableStoreExtensions` pattern (used for `GetCFlag`, `GetTalentValue`, etc.). Avoids new interface proliferation. SAVEDATA 2D array accessed via existing generic variable path. |
| Name data storage | A: Embedded JSON/YAML resource file; B: Static C# arrays in `ChildNameGenerator`; C: Database | B: Static C# arrays | Zero runtime I/O, zero dependencies, type-safe. Same pattern as `ValidSkinColors` in `GeneticsService`. ~1,300 lines of lookup data compiles to minimal IL. |
| GET_PRIVATE_ROOM / GET_N_IN_ROOM routing | A: Add methods to `ILocationService`; B: Implement as CFLAG reads + loop in `BirthProcess` | B: Local implementation | Both ERB functions are trivial reads: `GET_PRIVATE_ROOM` reads `CFLAG:私室`; `GET_N_IN_ROOM` loops over characters checking `CFLAG:現在地`. Adding them to `ILocationService` would violate ISP (location service has no character-loop responsibility). |
| Class split for PREGNACY_S.ERB milking domain | A: Include milking in `ChildGrowth`; B: Separate `MilkingProcess` class | B: Separate class | Milking (lines 731–1048) uses `IVariableStore` and `IConsoleOutput` only — 0% dependency overlap with `ChildGrowth` which needs `IGeneticsService` and `ICalendarService`. F808 constraint: <50% overlap = separate class mandatory. |
| TRYCALL 私室戻しDEBUG routing | A: Route through `IMultipleBirthService`; B: Guard with null-check delegate; C: Ignore (TRYCALL = no-op) | C: No-op (TRYCALL semantics) | `私室戻しDEBUG` is a debug-only function not part of production birth flow. TRYCALL means "execute if exists, skip otherwise." C# migration has no debug hook equivalent — no-op is the correct behavior per C6 constraint. |
| ChildNameGenerator injection | A: Extract `IChildNameGenerator` interface; B: Inject as concrete class | B: Concrete class | `ChildNameGenerator` is a pure data class with `static readonly` arrays and no side effects — mocking provides zero value. Same pattern as `GeneticsService` `ValidSkinColors`. Interface extraction would add unnecessary abstraction for a class that is deterministic by construction. |

### Interfaces / Data Structures

**New: `IMultipleBirthService`** (`Era.Core/Interfaces/IMultipleBirthService.cs`)

```csharp
namespace Era.Core.Interfaces;

/// <summary>
/// Stub interface for multiple birth operations migrated from 多生児パッチ.ERB.
/// Runtime implementation deferred to future feature. NullMultipleBirthService
/// provides safe defaults (returns 0 / no-op) for all methods.
/// Feature 822 - Pregnancy System Migration (Phase 22).
/// </summary>
public interface IMultipleBirthService
{
    /// <summary>Determine birth count for current delivery. Source: @生まれる人数</summary>
    int GetBirthCount(int motherId);

    /// <summary>Apply multi-birth flags to newborn. Source: @多生児フラグ処理</summary>
    void SetMultiBirthFlags(int childId, int multiBirthFlag, int siblingId);

    /// <summary>Display multi-birth announcement (first child). Source: @多生児出産口上(0)</summary>
    void DisplayBirthAnnouncement(int childId, int motherId);

    /// <summary>Display multi-birth announcement (subsequent children). Source: @多生児出産口上(1+)</summary>
    void DisplaySubsequentBirthAnnouncement(int childId, int siblingId, int motherId);

    /// <summary>Process foster child placement. Source: @里子</summary>
    void ProcessFosterChild(int childId, int motherId);

    /// <summary>Display birth announcement when parents are not MASTER. Source: @両親がMASTER以外の出産口上</summary>
    void DisplayNonMasterParentAnnouncement(int childId, int motherId, int fatherId);

    /// <summary>Initialize uterus volume for multiple pregnancy. Source: @子宮内体積設定</summary>
    void InitUterusVolumeMultiple(int motherId);

    /// <summary>Set child compatibility traits. Source: @子供相性設定</summary>
    void SetChildCompatibility(int childId, int motherId, int fatherId);
}

/// <summary>Null implementation of IMultipleBirthService. All births treated as single birth.</summary>
public sealed class NullMultipleBirthService : IMultipleBirthService
{
    public int GetBirthCount(int motherId) => 1;
    public void SetMultiBirthFlags(int childId, int multiBirthFlag, int siblingId) { }
    public void DisplayBirthAnnouncement(int childId, int motherId) { }
    public void DisplaySubsequentBirthAnnouncement(int childId, int siblingId, int motherId) { }
    public void ProcessFosterChild(int childId, int motherId) { }
    public void DisplayNonMasterParentAnnouncement(int childId, int motherId, int fatherId) { }
    public void InitUterusVolumeMultiple(int motherId) { }
    public void SetChildCompatibility(int childId, int motherId, int fatherId) { }
}
```

**Extension: `IVariableStoreExtensions.cs`** (append to existing file)

```csharp
// 親別出産数: 2D SAVEDATA birth-count-by-parent tracker (305x305)
// Variable name defined in VariableDefinitions.SaveData_BirthCountByParent
// Feature 822 - Pregnancy System Migration
public static int GetBirthCountByParent(this IVariableStore variables, int motherId, int fatherId)
    => /* delegate to generic 2D SAVEDATA accessor when available; stub returns 0 */ 0;

public static void SetBirthCountByParent(this IVariableStore variables, int motherId, int fatherId, int value)
    => /* delegate to generic 2D SAVEDATA accessor when available; no-op stub */ return;
```

Note: The generic 2D SAVEDATA accessor does not yet exist on `IVariableStore`. The extension methods stub the API surface (satisfying AC#10 grep) and will delegate to the real accessor when the SAVEDATA variable system is implemented. This is the same deferred-stub pattern used throughout Era.Core.

**`BirthProcess` constructor (representative, non-exhaustive):**

```csharp
public BirthProcess(
    IVariableStore variables,
    IEngineVariables engine,
    IConsoleOutput console,
    IInputHandler input,           // replaces INPUT/INPUTS loops
    IMenstrualCycle menstrualCycle, // F824 contract: ResetCycle on conception
    ICharacterManager characterManager, // ADDCHARA
    IGeneticsService genetics,
    IPregnancySettings pregnancySettings,
    IMultipleBirthService multipleBirth,
    ChildNameGenerator nameGenerator)  // concrete dependency (not interface)
```

**`PregnancyAnnouncement` constructor:**

```csharp
public PregnancyAnnouncement(
    IVariableStore variables,
    IEngineVariables engine,
    IConsoleOutput console,
    INtrUtilityService ntr,   // N_T_R display (GetNtrName)
    INtrQuery ntrQuery)       // NTR_CHK_FAVORABLY
```

**`ChildGrowth` constructor:**

```csharp
public ChildGrowth(
    IVariableStore variables,
    IEngineVariables engine,
    IGeneticsService genetics,
    ICalendarService calendar,   // ChildTemperatureResistance
    ICommonFunctions common,     // HasPenis
    IMultipleBirthService multipleBirth)
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| ~~AC#6 IUserInput→IInputHandler~~ | AC#6 — AC Definition Table | **RESOLVED**: AC#6 updated to grep `Era.Core/Input/` for `interface IInputHandler` |
| ~~AC#7 description alignment~~ | AC#7 — AC Definition Table | **RESOLVED**: Description updated to reference `IInputHandler` |
| ~~AC#18 IUserInput→IInputHandler registration~~ | AC#18 — AC Definition Table | **RESOLVED**: AC#18 updated to grep for `IInputHandler` |
| ~~AC#12 ILocationService removal~~ | AC#12 — AC Definition Table | **RESOLVED**: Pattern updated to `ICalendarService|IGeneticsService|ICharacterManager|INtrUtilityService|ICommonFunctions` (IInputHandler verified by AC#6) |

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 2, 3 | Create `Era.Core/Interfaces/IMultipleBirthService.cs` with 8-method interface and `NullMultipleBirthService` sealed class (safe-default implementations) | | [x] |
| 2 | 10 | Add `GetBirthCountByParent` and `SetBirthCountByParent` extension methods to `Era.Core/Interfaces/IVariableStoreExtensions.cs` | | [x] |
| 3 | 4, 5 | Create `Era.Core/State/ChildNameGenerator.cs` with all name lookup tables as `static readonly` arrays (boy/girl surnames and given names from PREGNACY_S.ERB lines 1049–2351) | | [x] |
| 4 | 1, 11 | Create `Era.Core/State/PregnancyAnnouncement.cs` migrating PREGNACY_S.ERB lines 13–230 (announcement/NTR messaging), injecting `INtrUtilityService`, `INtrQuery`, `IVariableStore`, `IEngineVariables`, `IConsoleOutput` | | [x] |
| 5 | 1, 6, 7, 8, 9, 13 | Create `Era.Core/State/BirthProcess.cs` migrating PREGNACY_S.ERB lines 231–560 (birth orchestration, INPUT naming loops, ADDCHARA), injecting `IInputHandler`, `IMenstrualCycle` (ResetCycle), `ICharacterManager`, `IGeneticsService`, `IMultipleBirthService`, `ChildNameGenerator`, `IVariableStore`, `IEngineVariables`, `IConsoleOutput`, `IPregnancySettings` | | [x] |
| 6 | 1, 12, 13 | Create `Era.Core/State/ChildGrowth.cs` migrating PREGNACY_S.ERB lines 561–730 (talent acquisition, genetics, calendar growth), injecting `ICalendarService`, `IGeneticsService`, `ICommonFunctions`, `IMultipleBirthService`, `IVariableStore`, `IEngineVariables` | | [x] |
| 7 | 1 | Create `Era.Core/State/MilkingProcess.cs` migrating PREGNACY_S.ERB lines 731–1048 (milking logic), injecting `IVariableStore`, `IConsoleOutput` | | [x] |
| 8 | 1, 19 | Create `Era.Core/State/ChildMovement.cs` migrating PREGNACY_S_CHILD_MOVEMENT.ERB (child room movement logic) | | [x] |
| 9 | 1, 20 | Create `Era.Core/State/PregnancyEventHandler.cs` migrating PREGNACY_S_EVENT.ERB and PREGNACY_S_EVENT0.ERB (pregnancy event dispatch) | | [x] |
| 10 | 17, 18 | Register `IMultipleBirthService` → `NullMultipleBirthService` in `ServiceCollectionExtensions.cs` under "State Systems (Phase 22) - Feature 822" block; verify `IInputHandler` registration already present | | [x] |
| 11 | 14 | Write unit tests in `Era.Core.Tests/State/Pregnancy/` namespace for all 7 classes: `PregnancyAnnouncementTests`, `BirthProcessTests`, `ChildGrowthTests`, `ChildNameGeneratorTests`, `MilkingProcessTests`, `ChildMovementTests`, `PregnancyEventHandlerTests` — namespace `Era.Core.Tests.State.Pregnancy` ensures `FullyQualifiedName~Pregnancy` filter catches all 7 | | [x] |
| 12 | 15, 16 | Run `dotnet build Era.Core/` and verify zero warnings; grep all 7 new class files for TODO/FIXME/HACK and confirm no matches | | [x] |

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
| 1 | implementer | sonnet | feature-822.md (Tasks T1–T2), Era.Core/Interfaces/ | IMultipleBirthService.cs, IVariableStoreExtensions.cs (extended) |
| 2 | implementer | sonnet | feature-822.md (Task T3), PREGNACY_S.ERB lines 1049–2351 | Era.Core/State/ChildNameGenerator.cs |
| 3 | implementer | sonnet | feature-822.md (Tasks T4–T9), PREGNACY_S.ERB, PREGNACY_S_CHILD_MOVEMENT.ERB, PREGNACY_S_EVENT.ERB, PREGNACY_S_EVENT0.ERB | 6 new .cs files in Era.Core/State/ |
| 4 | implementer | sonnet | feature-822.md (Task T10), Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | DI registration block added |
| 5 | tester | sonnet | feature-822.md (Task T11), all 7 Era.Core/State/ class files | Era.Core.Tests/State/Pregnancy/*Tests.cs (7 files) |
| 6 | tester | sonnet | feature-822.md (Task T12) | dotnet build output, grep output |

### Pre-conditions

| Condition | Verification |
|-----------|-------------|
| F821 [DONE] — ICalendarService.ChildTemperatureResistance exists | Grep Era.Core/Interfaces/ICalendarService.cs for ChildTemperatureResistance |
| F824 [DONE] — IMenstrualCycle.ResetCycle(int characterId) exists | Grep Era.Core/Interfaces/IMenstrualCycle.cs for ResetCycle |
| IInputHandler already exists at Era.Core/Input/IInputHandler.cs | Grep Era.Core/Input/ for interface IInputHandler |
| INtrUtilityService.GetNtrName exists | Grep Era.Core/Interfaces/INtrUtilityService.cs for GetNtrName |
| ICharacterManager.AddChara exists | Grep Era.Core/Interfaces/ICharacterManager.cs for AddChara |

### Execution Order

1. **T1 first**: `IMultipleBirthService` must exist before any class injects it (T5, T6)
2. **T2 independent**: `IVariableStoreExtensions` extension methods have no intra-feature dependency
3. **T3 before T5**: `ChildNameGenerator` must exist before `BirthProcess` takes it as constructor parameter
4. **T4–T9 order**: Each class is independent; implement in any order after T1/T3 complete
5. **T10 after T1**: DI registration requires interface to exist
6. **T11 after T4–T9**: Tests require all implementation classes to exist
7. **T12 last**: Build + zero-debt verification runs after all code is written

### Build Verification Steps

```bash
# Run from WSL
cd /mnt/c/Era/core
/home/siihe/.dotnet/dotnet build Era.Core/Era.Core.csproj
# Expected: Build succeeded with 0 Warning(s)

/home/siihe/.dotnet/dotnet test Era.Core.Tests/ --blame-hang-timeout 10s --results-directory _out/test-results --filter "FullyQualifiedName~Pregnancy"
# Expected: All pass
```

### Success Criteria

All 20 ACs pass. Specifically:
- 7 class files exist in `Era.Core/State/` (AC#1, AC#4, AC#19, AC#20)
- `IMultipleBirthService.cs` has 8+ methods; `NullMultipleBirthService` exists (AC#2, AC#3)
- No name tables in `BirthProcess.cs` or lifecycle classes (AC#5)
- `IInputHandler` used in pregnancy state code; no `Console.ReadLine` (AC#6, AC#7)
- `ResetCycle` called in `BirthProcess`; no raw 生理周期 assignment (AC#8, AC#9)
- `BirthCountByParent` methods exist in extensions (AC#10)
- `_ntr` / `INtrUtilityService` referenced in `PregnancyAnnouncement.cs` (AC#11)
- 5+ interface types referenced across pregnancy state files (AC#12)
- `IMultipleBirthService` referenced in 2+ state files (AC#13)
- All Pregnancy-filtered tests pass (AC#14)
- Build succeeds zero warnings (AC#15)
- No TODO/FIXME/HACK in any of the 7 new files (AC#16)
- DI registrations present for `IMultipleBirthService` and `IInputHandler` (AC#17, AC#18)

### Error Handling

| Situation | Action |
|-----------|--------|
| `IVariableStore` has no generic 2D SAVEDATA accessor | Implement `GetBirthCountByParent`/`SetBirthCountByParent` as stubs returning 0/no-op with comment; do NOT create new interface |
| ERB line range produces ambiguous domain boundary | Follow Technical Design class map exactly; do NOT invent new classes |
| Constructor parameter count exceeds 10 | STOP — report to user; split class per F808 <50% dependency overlap rule |
| `dotnet build` produces warnings | Fix all warnings before proceeding; `TreatWarningsAsErrors=true` is in effect |
| Test filter `FullyQualifiedName~Pregnancy` finds 0 tests | All test classes are in `Era.Core.Tests.State.Pregnancy` namespace — filter should always match; if 0, check namespace declaration |

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| `IVariableStore` generic 2D SAVEDATA accessor not yet implemented (stub only in T2) | Full runtime implementation requires SAVEDATA variable system work | Feature | F826 | — | [x] | 追記済み |
| `NullMultipleBirthService` is a stub; runtime multi-birth logic from 多生児パッチ.ERB not migrated | Runtime implementation deferred; stub provides safe defaults for all call sites | Feature | F826 | — | [x] | 追記済み |

<!-- Transferred + Result columns (F811/F805 Lesson):
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: 作成済み(A), 追記済み(B), 記載済み(C), 確認済み(既存)
- Phase 9.4.1 で転記実行・Result記入。Phase 10.0.2 で検証のみ
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
| 2026-03-05T08:10 | START | initializer | [REVIEWED] → [WIP] | READY:822:erb |
| 2026-03-05T08:10 | PHASE_COMPLETE | orchestrator | Phase 1 Initialize | OK |
<!-- run-phase-1-completed -->
| 2026-03-05T08:15 | PHASE_COMPLETE | orchestrator | Phase 2 Investigation | OK |
<!-- run-phase-2-completed -->
| 2026-03-05T08:20 | PHASE_COMPLETE | orchestrator | Phase 3 TDD RED (N/A: C# unit tests need impl classes to compile; headless test N/A) | SKIP |
<!-- run-phase-3-completed -->
| 2026-03-05T17:33 | END | implementer | Task 7: Create MilkingProcess.cs | SUCCESS |
| 2026-03-05T18:52 | END | implementer | Task 9: Create PregnancyEventHandler.cs | SUCCESS |
| 2026-03-05T17:52 | START | implementer | Task 11: Write unit tests (7 files) | - |
| 2026-03-05T17:52 | END | implementer | Task 11: Write unit tests (7 files), 34 tests pass | SUCCESS |
| 2026-03-05T18:55 | PHASE_COMPLETE | orchestrator | Phase 4 Implementation (T1-T12 all SUCCESS) | OK |
<!-- run-phase-4-completed -->
| 2026-03-05T19:00 | Phase 5 | orchestrator | Refactoring review | SKIP (no refactoring needed — new code, no duplication) |
<!-- run-phase-5-completed -->
| 2026-03-05T19:10 | PHASE_COMPLETE | orchestrator | Phase 7 Verification — AC 20/20 PASS | OK |
<!-- run-phase-7-completed -->
| 2026-03-05T19:15 | DEVIATION | feature-reviewer | Phase 8.1 quality review | NEEDS_REVISION: 2 critical bugs (BirthProcess drug resistance target, ChildMovement basement check), 1 major DRY violation |
| 2026-03-05T19:20 | FIX | debugger | 3 issues fixed | SUCCESS: drug resistance target, basement check, LocationHelper extraction |
| 2026-03-05T19:25 | DEVIATION | feature-reviewer | Phase 8.2 doc-check | NEEDS_REVISION: INTERFACES.md + PATTERNS.md SSOT updates missing |
| 2026-03-05T19:30 | FIX | implementer | SSOT docs updated | SUCCESS: INTERFACES.md + PATTERNS.md |
| 2026-03-05T19:30 | PHASE_COMPLETE | orchestrator | Phase 8 Post-Review | OK (2 deviations fixed) |
<!-- run-phase-8-completed -->
| 2026-03-05T19:35 | PHASE_COMPLETE | orchestrator | Phase 9 Report & Approval | OK |
<!-- run-phase-9-completed -->
| 2026-03-05T19:40 | COMMIT | orchestrator | core: eaa0595, devkit: 654d15b | CI PASSED (3278+807 tests) |
| 2026-03-05T19:45 | CodeRabbit | 9 Minor (修正不要) | Input stub pattern (by-design), false positives (NullMultipleBirthService/Result type), faithful ERB reproduction (stamina clamp) |
| 2026-03-05T19:45 | PHASE_COMPLETE | orchestrator | Phase 10 Finalize | OK |
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: Goal Item 2 vs AC#2 vs Technical Design IMultipleBirthService interface | Added missing InitUterusVolumeMultiple method, updated AC#2 threshold 7→8, aligned method count across Goal/AC/interface
- [fix] Phase2-Review iter1: AC#6 pre-existing infrastructure | Replaced AC#6 to verify IInputHandler injection into BirthProcess instead of checking pre-existing interface existence
- [fix] Phase2-Review iter2: Implementation Contract > Success Criteria | Updated stale '7+ methods' to '8+ methods' to match corrected AC#2 threshold; also updated C2 constraint details
- [fix] Phase2-Review iter3: AC Coverage AC#6 | Removed stale IUserInput reference, updated to reflect BirthProcess.cs injection check
- [fix] Phase2-Review iter3: AC Coverage AC#18 | Removed stale IUserInput reference, updated to reflect existing IInputHandler registration
- [fix] Phase2-Review iter3: C2 Constraint Details | Changed '7-8 functions' to '8 functions' for consistency
- [fix] Phase2-Review iter4: AC#14 / Task 11 | Specified Pregnancy sub-namespace (Era.Core.Tests.State.Pregnancy) so FullyQualifiedName~Pregnancy filter catches all 7 test classes
- [fix] Phase2-Review iter5: AC#12 pattern | Replaced redundant IInputHandler with ICommonFunctions (HAS_PENIS external call) — IInputHandler already covered by AC#6
- [fix] Phase3-Maintainability iter6: Technical Design ILocationService | Removed stale 'AC#12 tracks ILocationService' reference, clarified CFLAG reads per Key Decisions
- [fix] Phase3-Maintainability iter6: Task 7 AC mapping | Removed AC#12 from T7 — MilkingProcess injects no AC#12 pattern interfaces
- [fix] Phase3-Maintainability iter6: Key Decisions | Added ChildNameGenerator concrete injection rationale (pure data class, no mocking value)
- [fix] Phase2-Review iter7: AC#12 Details | Replaced stale IInputHandler with ICommonFunctions (HAS_PENIS) to match AC Definition Table pattern; updated Upstream Issues resolution note
- [fix] Phase4-ACValidation iter8: AC#5 | Removed overly broad 姓|名 from grep pattern — common kanji would false-fail; RANDOM_N_GIRL|RANDOM_N_BOY sufficient
- [fix] Phase4-ACValidation iter8: AC#15 | Changed build type matcher from matches to succeeds — matches not supported for build type

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning
- [Predecessor: F821](feature-821.md) - Weather System (cross-subsystem call dependency: ICalendarService.ChildTemperatureResistance)
- [Related: F824](feature-824.md) - Sleep & Menstrual (IMenstrualCycle.ResetCycle contract for conception)
- [Related: F808](feature-808.md) - SRP lesson (domain decomposition obligation #32)

