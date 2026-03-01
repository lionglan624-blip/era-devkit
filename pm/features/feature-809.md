# Feature 809: COMABLE Core

## Status: [DONE]
<!-- fl-reviewed: 2026-02-24T02:20:23Z -->

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

Phase 21: Counter System C# migration follows the established F801 DI injection pattern (with one documented deviation: GlobalComableFilter injected as concrete class — see Key Decisions). COMABLE Core is the SSOT for COM command availability checking (IComAvailabilityChecker interface), providing C# infrastructure for future ERB dispatch wiring (F810/F811 scope). All migrated logic must be query-only (read-only access, no side effects), equivalence-tested, and zero-technical-debt.

### Problem (Current Issue)

F809 is a skeletal DRAFT with 2 generic tasks and 1 AC, because the architecture's IComAvailabilityChecker was designed abstractly with an `IsAvailable(ComId, CharacterId, IGameState)` signature (phase-20-27-game-systems.md:127-131) without analyzing the actual ERB dispatch pattern. The real COMABLE.ERB contains 124 individual `@COM_ABLE{N}` pure functions (IDs 0-648) accessing ~15 variable domains (TFLAG, CFLAG, ABL, TALENT, PALAM, STAIN, EQUIP, TEQUIP, BASE, MAXBASE, EXP, ITEM, plus engine built-ins ASSIPLAY, NOITEM), plus 3-4 utility functions (HAS_PENIS, HAS_VAGINA, MASTER_POSE, BATHROOM/BEDROOM). The IGameState parameter is a high-level system control interface (Save/Load/Quit) and cannot provide variable access. Additionally, 5 interface gaps exist: ASSIPLAY and NOITEM have no C# accessors, EXPLV has no getter in IVariableStore, TRAINNAME has no method in ICsvNameResolver, and MASTER_POSE has no C# equivalent (defined in SOURCE_POSE.ERB, F811 scope).

### Goal (What to Achieve)

Migrate all 124 `@COM_ABLE{N}` functions from COMABLE.ERB and the `@GLOBAL_COMABLE` centralized gating function from COMABLE2.ERB into C# classes under `Era.Core/Counter/Comable/`, following the F801 DI injection pattern (constructor-injected interfaces, not IGameState). Resolve remaining 3 interface gaps (NOITEM, EXPLV setter, TRAINNAME) via additive extensions; verify ASSIPLAY (F812) and EXPLV getter (F804) already present. Expose GLOBAL_COMABLE as public API for F810 consumption. Abstract MASTER_POSE behind an injectable stub interface. IComAvailabilityChecker implements only `IsAvailable(int comId)` — `GetUnavailableReason` is deferred to F813 (architecture spec deviation: phase-20-27-game-systems.md:130 defines 2 methods, but ERB has no reason codes).

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why? | F809 has only 2 stub tasks and 1 generic AC, insufficient for 124 COM_ABLE functions across 3,748 lines | `pm/features/feature-809.md:55-66` |
| 2 | Why? | F783 planning created minimal DRAFT templates deferring detailed AC/Task generation to /fc | `pm/features/feature-801.md:53-54` |
| 3 | Why? | The architecture spec defines IComAvailabilityChecker with 2 methods using IGameState parameter, without mapping to actual ERB code patterns | `docs/architecture/phases/phase-20-27-game-systems.md:127-131` |
| 4 | Why? | COMABLE.ERB uses 124 individual @COM_ABLE{N} functions accessing ~15 variable domains plus GLOBAL_COMABLE centralized gating, while IGameState is a system control interface (Save/Load/Quit) | `Game/ERB/COMABLE.ERB:4-11`, `Era.Core/Interfaces/IGameState.cs:12-44` |
| 5 | Why (Root)? | The architecture's IComAvailabilityChecker was designed as an abstract API contract without analyzing the actual ERB dispatch pattern (TRYCALLFORM COM_ABLE{N}), the GLOBAL_COMABLE centralized filter, or the ~15 variable domain dependencies and 5 interface gaps (ASSIPLAY, NOITEM, EXPLV, TRAINNAME, MASTER_POSE) | `Era.Core/Counter/ActionValidator.cs:11-16` (F801 pattern), `Game/ERB/COMABLE2.ERB:5-149` |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | F809 has 2 stub tasks and 1 generic AC | Architecture IComAvailabilityChecker designed without analyzing 124-function ERB structure, ~15 variable domains, and 5 interface gaps |
| Where | feature-809.md Tasks/AC sections | phase-20-27-game-systems.md interface definition (IGameState parameter mismatch) |
| Fix | Add more tasks/ACs manually | Redesign interface to use F801 DI injection pattern with ~7 constructor-injected interfaces; add 5 additive interface extensions |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F783 | [DONE] | Predecessor -- Phase 21 Planning (parent decomposition) |
| F801 | [DONE] | Related -- established DI injection pattern (ActionValidator) |
| F810 | [DRAFT] | Successor -- COMABLE Extended (COMABLE_300/400 call GLOBAL_COMABLE 44 times) |
| F811 | [BLOCKED] | Related -- SOURCE Entry System (owns MASTER_POSE in SOURCE_POSE.ERB:334; SOURCE_CALLCOM.ERB:70 calls COM_ABLE) |
| F813 | [DRAFT] | Successor -- Post-Phase Review Phase 21 |
| F815 | [DONE] | Related -- StubVariableStore base class for testing |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Interface infrastructure | FEASIBLE | IVariableStore, ITEquipVariables, ICommonFunctions, ILocationService, IItemVariables, IStringVariables all exist with needed methods |
| Architecture interface alignment | NEEDS_REVISION | IComAvailabilityChecker uses IGameState (wrong); must follow F801 DI injection pattern with constructor-injected interfaces |
| Interface gaps (additive) | FEASIBLE | 5 additive extensions needed (ASSIPLAY, NOITEM, EXPLV, TRAINNAME, MASTER_POSE) -- same safe pattern as F801 |
| Scope magnitude | FEASIBLE | 124 functions are large but highly repetitive pure-function pattern |
| Test infrastructure | FEASIBLE | F801 and F815 established pattern with StubVariableStore base class |
| Predecessor status | FEASIBLE | F783 is [DONE] |
| MASTER_POSE cross-feature dependency | FEASIBLE | Abstractable via injectable stub interface (like ICounterUtilities in F801) |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core/Interfaces | MEDIUM | 4 additive method extensions across IEngineVariables, IItemVariables, IVariableStore, ICsvNameResolver |
| Era.Core/Counter/Comable | HIGH | New subsystem: ComableChecker, GlobalComableFilter, IComAvailabilityChecker, IComableUtilities |
| engine.Tests/StubVariableStore | LOW | Must implement new interface methods (additive stubs) |
| F810 COMABLE Extended | HIGH | F810 depends on GLOBAL_COMABLE being public API from this feature |
| F811 SOURCE Entry | MEDIUM | SOURCE_CALLCOM.ERB:70 calls COM_ABLE{500+N}; MASTER_POSE abstracted via stub |
| Era.Core/Interfaces/ concrete implementations | MEDIUM | NullEngineVariables, EngineVariables, NullItemVariables, EngineItemVariables, NullCsvNameResolver must implement new methods |
| Era.Core.Tests mock implementations | HIGH | ~39 mock classes across 26 files must implement new interface methods (CS0535) |
| src/tools/dotnet/KojoComparer/YamlRunner.cs | LOW | VariableStoreAdapter must implement SetExpLv (GetExpLv already added by F804) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| IComAvailabilityChecker signature mismatch with architecture | phase-20-27-game-systems.md:127-131 | Must redesign interface to use DI injection pattern instead of IGameState |
| 124 individual checker functions | COMABLE.ERB (3,748 lines) | Large class risk; must split by command range for SRP |
| GLOBAL_COMABLE shared with F810 | COMABLE2.ERB; COMABLE_300.ERB:11, COMABLE_400.ERB:17 | Must be placed in shared public location accessible to F810 |
| TRAINNAME is a CSV-loaded constant array | VariableCode.cs | Need accessor method on ICsvNameResolver |
| SAVESTR:0 command filter in GLOBAL_COMABLE | COMABLE2.ERB:9-13 | Needs IStringVariables.GetSaveStr with regex/contains equivalent |
| TreatWarningsAsErrors build policy | Directory.Build.props | All code must compile warning-free |
| ComId range wider than documented | ComId.cs documents 0-299 but actual range is 0-648 | Must update documentation |
| MASTER_POSE defined in F811 scope | SOURCE_POSE.ERB:334 | Must abstract behind injectable interface to avoid circular dependency |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| ComableChecker becomes God class (124 functions) | HIGH | MEDIUM | Split by command range (0-99, 100-199, 200-299, etc.) |
| MASTER_POSE complex dependency with F811 | MEDIUM | HIGH | Create IComableUtilities with stub implementation; concrete impl deferred to F811 |
| Interface extension breaking existing tests | LOW | HIGH | Additive-only changes; update StubVariableStore base class |
| 124-function migration fidelity | MEDIUM | HIGH | Equivalence tests per command range group |
| GLOBAL_COMABLE video camera logic complexity | MEDIUM | MEDIUM | Self-contained 76-line SELECTCASE block with 40+ command names; isolate in GlobalComableFilter |
| Architecture doc update conflicts | LOW | LOW | Document deviation in execution log |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| C# Comable files exist | `ls Era.Core/Counter/Comable/` | 0 files | Directory does not exist yet |
| Interface gap: ASSIPLAY | `grep -r "GetAssiPlay" Era.Core/` | 1 match (STALE) | GetAssiPlay already added to IEngineVariables by F812 (IEngineVariables.cs:98); baseline was 0 matches before F812 |
| Interface gap: NOITEM on IItemVariables | `grep -r "GetNoItem" Era.Core/Interfaces/IItemVariables.cs` | 0 matches | Method does not exist on IItemVariables (F804 added GetNoItem to IVariableStore; F809 adds a separate GetNoItem to IItemVariables) |
| Interface gap: EXPLV | `grep -r "GetExpLv" Era.Core/Interfaces/IVariableStore.cs` | 1 match (STALE) | GetExpLv already added to IVariableStore by F804; baseline was 0 matches before F804 |
| Interface gap: TRAINNAME | `grep -r "GetTrainName" Era.Core/` | 0 matches | Method does not exist |
| dotnet build | WSL dotnet build | PASS | Baseline before changes |
| dotnet test | WSL dotnet test | PASS | Baseline before changes |

**Baseline File**: `.tmp/baseline-809.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Architecture mandates IComAvailabilityChecker interface | phase-20-27-game-systems.md:127-131 | AC must verify interface file exists in Era.Core/Counter/Comable/ |
| C2 | Zero technical debt | Sub-Feature Requirements | AC must use not_matches TODO/FIXME/HACK |
| C3 | Equivalence tests required | Sub-Feature Requirements | AC must verify test files exist |
| C4 | GLOBAL_COMABLE must be public API for F810 | COMABLE_300.ERB:11, COMABLE_400.ERB:17 (44 calls) | AC must verify public class/method accessibility |
| C5 | All 124 COM_ABLE functions must be migrated | COMABLE.ERB (124 @COM_ABLE functions) | AC must verify coverage (all ComId ranges) |
| C6 | F801 DI injection pattern required | ActionValidator.cs:11-16 | AC must verify constructor injection (no IGameState) |
| C7 | Binary RETURN 0/1 pattern | COMABLE.ERB function pattern | AC must verify bool return type |
| C8 | 5 additive interface extensions | IEngineVariables, IItemVariables, IVariableStore, ICsvNameResolver | AC must verify new methods exist |
| C9 | MASTER_POSE abstractable via stub | F801 ICounterUtilities pattern | AC must verify IComableUtilities interface + stub |
| C10 | Interface Dependency Scan: ASSIPLAY | COMABLE.ERB (57 uses across functions) | AC must verify GetAssiPlay() exists on appropriate interface |
| C11 | Interface Dependency Scan: NOITEM | COMABLE.ERB (65 uses) | AC must verify GetNoItem() exists on appropriate interface |
| C12 | Interface Dependency Scan: TRAINNAME | COMABLE2.ERB:76-124 (video camera restrictions) | AC must verify GetTrainName(int) exists on ICsvNameResolver |
| C13 | Interface Dependency Scan: EXPLV | COMABLE.ERB (10 uses) | AC must verify GetExpLv(int) exists on IVariableStore |

### Constraint Details

**C1: IComAvailabilityChecker Interface**
- **Source**: Architecture spec phase-20-27-game-systems.md:127-131
- **Verification**: `ls Era.Core/Counter/Comable/IComAvailabilityChecker.cs`
- **AC Impact**: Interface must exist; signature must use DI injection pattern (not IGameState)

**C4: GLOBAL_COMABLE Public API**
- **Source**: COMABLE_300.ERB and COMABLE_400.ERB call GLOBAL_COMABLE 44 times total (F810 scope)
- **Verification**: `grep -r "public.*GlobalComableFilter\|public.*IsGloballyAvailable" Era.Core/Counter/Comable/`
- **AC Impact**: Class and method must be public; F810 must be able to inject and call it

**C6: DI Injection Pattern**
- **Source**: F801 ActionValidator.cs:11-16 uses constructor injection of 5 interfaces
- **Verification**: `grep -r "IGameState" Era.Core/Counter/Comable/` should return 0 matches
- **AC Impact**: ComableChecker must inject IVariableStore, ITEquipVariables, IEngineVariables, ICommonFunctions, ILocationService, IItemVariables, IComableUtilities, GlobalComableFilter via constructor (8 parameters; IStringVariables and ICsvNameResolver go to GlobalComableFilter only)

**C9: MASTER_POSE Stub Interface**
- **Source**: COMABLE.ERB calls MASTER_POSE 25 times; function defined in SOURCE_POSE.ERB (F811 scope)
- **Verification**: `ls Era.Core/Counter/Comable/IComableUtilities.cs`
- **AC Impact**: Must create IComableUtilities with MasterPose(int,int,int) method; stub returns 0 (conservative default)

**C10: ASSIPLAY Interface Extension**
- **Source**: COMABLE.ERB uses ASSIPLAY 57 times as engine built-in variable
- **Verification**: `grep "GetAssiPlay" Era.Core/Interfaces/IEngineVariables.cs`
- **AC Impact**: Additive method on IEngineVariables; must not break existing tests

**C12: TRAINNAME Interface Extension**
- **Source**: COMABLE2.ERB:76-124 uses TRAINNAME:comId for video camera restriction logic
- **Verification**: `grep "GetTrainName" Era.Core/Interfaces/ICsvNameResolver.cs`
- **AC Impact**: Additive method on ICsvNameResolver; CSV-loaded constant array

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| Successor | F810 | [DRAFT] | COMABLE Extended -- F810 calls GLOBAL_COMABLE 44 times (COMABLE_300.ERB:11, COMABLE_400.ERB:17) |
| Successor | F813 | [DRAFT] | Post-Phase Review Phase 21 |
| Related | F801 | [DONE] | Pattern reference -- established DI injection pattern (ActionValidator.cs:11-16) |
| Related | F811 | [BLOCKED] | SOURCE Entry System -- owns MASTER_POSE (SOURCE_POSE.ERB:334); SOURCE_CALLCOM.ERB:70 calls COM_ABLE{500+N} |
| Related | F815 | [DONE] | StubVariableStore base class for testing |

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
| "COMABLE Core is the SSOT for COM command availability checking" | ComableChecker class must exist as C# layer SSOT (ERB dispatch wiring deferred to F810/F811 — tracked in Mandatory Handoffs) | AC#1, AC#2 |
| "All migrated logic must be query-only (read-only, no side effects)" | ComableChecker and GlobalComableFilter must be query-only bool-returning functions (no setter calls in Comable subsystem) | AC#3, AC#57 |
| "equivalence-tested" | Equivalence tests must exist for each command range | AC#7, AC#25, AC#26a, AC#26b, AC#26c, AC#55a, AC#55b, AC#55c |
| "zero-technical-debt" | No TODO/FIXME/HACK in migrated files; SSOT documents updated | AC#15, AC#22 |
| "All 124 COM_ABLE functions" | Switch expression must cover all COM IDs including the highest (648) | AC#23, AC#28, AC#32, AC#50, AC#31a, AC#31b, AC#31c |
| "F801 DI injection pattern" | Constructor-injected interfaces, no IGameState | AC#4, AC#5, AC#33, AC#35, AC#37, AC#40, AC#45, AC#46, AC#58 |
| "GlobalComableFilter concrete injection (deviates from F801 interface pattern)" | GlobalComableFilter injected as concrete class; justified by no polymorphism requirement; testable via direct construction with stub dependencies | AC#27, AC#34 |
| "Resolve all 5 interface gaps" | ASSIPLAY, NOITEM, EXPLV, TRAINNAME, MASTER_POSE interface methods must exist | AC#8, AC#9, AC#10, AC#11, AC#12, AC#13 |
| "IVariableStore getter/setter convention (Zero Debt Upfront)" | EXPLV is writable (OPTION_2.ERB:43-52); IVariableStore convention pairs every getter with setter to prevent future debt. GetNoItem exempt — NOITEM is engine-managed read-only (no ERB writes found; see Key Decisions) | AC#21 |
| "Expose GLOBAL_COMABLE as public API for F810 consumption" | GlobalComableFilter must be public | AC#6 |
| "Abstract MASTER_POSE behind an injectable stub interface" | IComableUtilities with MasterPose method must exist | AC#12, AC#13, AC#56 |
| "MASTER_POSE stub causes known equivalence deviation for 25 COM_ABLE functions" | 25 MASTER_POSE-dependent functions return stub value (0) until F811 provides concrete implementation; equivalence is partial for these functions | AC#12, AC#13 (stub existence); F811 Mandatory Handoff (concrete impl) |
| "GetUnavailableReason deferred to F813" | IComAvailabilityChecker exposes single IsAvailable method only | AC#30, AC#35 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IComAvailabilityChecker interface exists | file | Glob(Era.Core/Counter/Comable/IComAvailabilityChecker.cs) | exists | - | [ ] |
| 2 | ComableChecker class implements IComAvailabilityChecker | code | Grep(Era.Core/Counter/Comable/ComableChecker*.cs) | contains | `: IComAvailabilityChecker` | [ ] |
| 3 | ComableChecker methods return bool | code | Grep(Era.Core/Counter/Comable/ComableChecker*.cs) | matches | `public bool IsAvailable` | [ ] |
| 4 | ComableChecker uses DI injection (constructor-injected interfaces) | code | Grep(Era.Core/Counter/Comable/ComableChecker*.cs) | contains | `IVariableStore variables` | [ ] |
| 5 | No IGameState dependency in Comable subsystem | code | Grep(Era.Core/Counter/Comable/) | not_matches | `IGameState` | [ ] |
| 6 | GlobalComableFilter is public sealed class with public method | code | Grep(Era.Core/Counter/Comable/GlobalComableFilter.cs) | matches | `public sealed class GlobalComableFilter` | [ ] |
| 7 | Equivalence tests contain actual test methods for ComableChecker | code | Grep(Era.Core.Tests/Counter/ComableCheckerTests.cs) | matches | `\[Fact\]\|\[Theory\]` | [ ] |
| 8 | GetAssiPlay method exists on IEngineVariables (added by F812, verified by F809) | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | `GetAssiPlay\(` | [ ] |
| 9 | GetNoItem method added to IItemVariables | code | Grep(Era.Core/Interfaces/IItemVariables.cs) | matches | `GetNoItem\(` | [ ] |
| 10 | GetExpLv method exists on IVariableStore (added by F804, verified by F809) | code | Grep(Era.Core/Interfaces/IVariableStore.cs) | matches | `GetExpLv\(` | [ ] |
| 11 | GetTrainName method added to ICsvNameResolver | code | Grep(Era.Core/Interfaces/ICsvNameResolver.cs) | matches | `GetTrainName\(` | [ ] |
| 12 | IComableUtilities interface exists with 3-parameter MasterPose method | code | Grep(Era.Core/Counter/Comable/IComableUtilities.cs) | matches | `MasterPose\(int.*int.*int` | [ ] |
| 13 | IComableUtilities stub implementation exists | code | Grep(Era.Core/Counter/Comable/) | matches | `class\s+\w+\s*:\s*IComableUtilities` | [ ] |
| 14 | dotnet build succeeds | build | dotnet build Era.Core/ | succeeds | - | [ ] |
| 15 | No TODO/FIXME/HACK in Comable subsystem | code | Grep(Era.Core/Counter/Comable/) | not_matches | `TODO|FIXME|HACK` | [ ] |
| 16 | dotnet test passes for Era.Core.Tests | test | dotnet test Era.Core.Tests/ | succeeds | - | [ ] |
| 17 | IEngineVariables backward compatibility (method count preserved at 25) | code | Grep(path="Era.Core/Interfaces/IEngineVariables.cs", pattern="^\s+(int\|void\|string\|bool\|Result<int>)\s+\w+\(") | count_equals | 25 | [ ] |
| 18 | IItemVariables backward compatibility (method count preserved + 1) | code | Grep(path="Era.Core/Interfaces/IItemVariables.cs", pattern="^\s+(int\|void\|string\|bool\|Result<int>)\s+\w+\(") | count_equals | 7 | [ ] |
| 19 | IVariableStore backward compatibility (method count preserved + 1) | code | Grep(path="Era.Core/Interfaces/IVariableStore.cs", pattern="^\s+(int\|void\|string\|bool\|Result<int>)\s+\w+\(") | count_equals | 43 | [ ] |
| 20 | ICsvNameResolver backward compatibility (method count preserved + 1) | code | Grep(path="Era.Core/Interfaces/ICsvNameResolver.cs", pattern="^\s+(int\|void\|string\|bool\|Result<int>)\s+\w+\(") | count_equals | 6 | [ ] |
| 21 | SetExpLv method added to IVariableStore | code | Grep(Era.Core/Interfaces/IVariableStore.cs) | matches | `SetExpLv\(` | [ ] |
| 22 | engine-dev SKILL.md updated with new interfaces/methods | code | Grep(.claude/skills/engine-dev/SKILL.md) | matches | `IComAvailabilityChecker` | [ ] |
| 23 | ComableChecker switch covers highest COM ID (648) | code | Grep(Era.Core/Counter/Comable/ComableChecker*.cs) | matches | `648\s*=>` | [ ] |
| 24 | GlobalComableFilter public method IsGloballyBlocked exists | code | Grep(Era.Core/Counter/Comable/GlobalComableFilter.cs) | matches | `public bool IsGloballyBlocked\(` | [ ] |
| 25 | GlobalComableFilter equivalence tests cover 5 branch groups | code | Grep(path="Era.Core.Tests/Counter/GlobalComableFilterTests.cs", pattern="\[Fact\]\|\[Theory\]") | gte | 5 | [ ] |
| 26a | ComableChecker tests cover Range2x (200-299) | code | Grep(Era.Core.Tests/Counter/ComableCheckerTests.cs) | matches | `IsAvailable\(2[0-9]{2}\)` | [ ] |
| 26b | ComableChecker tests cover Range5x (500-599) | code | Grep(Era.Core.Tests/Counter/ComableCheckerTests.cs) | matches | `IsAvailable\(5[0-9]{2}\)` | [ ] |
| 26c | ComableChecker tests cover Range6x (600-648) | code | Grep(Era.Core.Tests/Counter/ComableCheckerTests.cs) | matches | `IsAvailable\(6([0-3][0-9]|4[0-8])\)` | [ ] |
| 27 | GlobalComableFilter injected via ComableChecker constructor | code | Grep(Era.Core/Counter/Comable/ComableChecker.cs) | matches | `GlobalComableFilter\s+\w+[,)]` | [ ] |
| 28 | All 4 ComableChecker range partial files exist | file | Glob(Era.Core/Counter/Comable/ComableChecker.Range*.cs) | count_equals | 4 | [ ] |
| 29 | engine.Tests passes after StubVariableStore extension | test | dotnet test engine.Tests/ | succeeds | - | [ ] |
| 30 | IComAvailabilityChecker interface has exactly 1 method signature | code | Grep(path="Era.Core/Counter/Comable/IComAvailabilityChecker.cs", pattern="^\s+(bool|int|void|string)\s+\w+\(") | count_equals | 1 | [ ] |
| 31a | COM_ABLE507 anomaly case tested | code | Grep(Era.Core.Tests/Counter/ComableCheckerTests.cs) | matches | `IsAvailable\(507\)` | [ ] |
| 31b | COM_ABLE512 anomaly case tested | code | Grep(Era.Core.Tests/Counter/ComableCheckerTests.cs) | matches | `IsAvailable\(512\)` | [ ] |
| 31c | COM_ABLE189 anomaly case tested | code | Grep(Era.Core.Tests/Counter/ComableCheckerTests.cs) | matches | `IsAvailable\(189\)` | [ ] |
| 32 | ComableChecker switch expression dispatches 123 COM_ABLE functions via IsAvailableN() | code | Grep(path="Era.Core/Counter/Comable/ComableChecker.cs", pattern="=> IsAvailable\\d+\\(\\)") | count_equals | 123 | [ ] |
| 33 | IComableUtilities injected as ComableChecker constructor parameter | code | Grep(Era.Core/Counter/Comable/ComableChecker.cs) | matches | `IComableUtilities\s+\w+[,)]` | [ ] |
| 34 | GlobalComableFilter constructible in test context with stub dependencies | code | Grep(Era.Core.Tests/Counter/GlobalComableFilterTests.cs) | matches | `new GlobalComableFilter\(` | [ ] |
| 35 | GetUnavailableReason not prematurely added to IComAvailabilityChecker | code | Grep(Era.Core/Counter/Comable/IComAvailabilityChecker.cs) | not_matches | `GetUnavailableReason` | [ ] |
| 35a | GlobalComableFilter constructor has IVariableStore | code | Grep(Era.Core/Counter/Comable/GlobalComableFilter.cs) | contains | `IVariableStore variables` | [ ] |
| 35b | GlobalComableFilter constructor has IStringVariables | code | Grep(Era.Core/Counter/Comable/GlobalComableFilter.cs) | matches | `IStringVariables` | [ ] |
| 35c | GlobalComableFilter constructor has ICsvNameResolver | code | Grep(Era.Core/Counter/Comable/GlobalComableFilter.cs) | matches | `ICsvNameResolver` | [ ] |
| 35d | GlobalComableFilter constructor has IEngineVariables | code | Grep(Era.Core/Counter/Comable/GlobalComableFilter.cs) | matches | `IEngineVariables` | [ ] |
| 35e | GlobalComableFilter constructor has ILocationService | code | Grep(Era.Core/Counter/Comable/GlobalComableFilter.cs) | matches | `ILocationService` | [ ] |
| 36a | engine-dev SKILL.md documents GetAssiPlay | code | Grep(.claude/skills/engine-dev/SKILL.md) | matches | `GetAssiPlay` | [ ] |
| 36b | engine-dev SKILL.md documents GetNoItem | code | Grep(.claude/skills/engine-dev/SKILL.md) | matches | `GetNoItem` | [ ] |
| 36c | engine-dev SKILL.md documents GetTrainName | code | Grep(.claude/skills/engine-dev/SKILL.md) | matches | `GetTrainName` | [ ] |
| 36d | engine-dev SKILL.md documents GetExpLv | code | Grep(.claude/skills/engine-dev/SKILL.md) | matches | `GetExpLv` | [ ] |
| 36e | engine-dev SKILL.md documents SetExpLv | code | Grep(.claude/skills/engine-dev/SKILL.md) | matches | `SetExpLv` | [ ] |
| 37 | ICommonFunctions injected as ComableChecker constructor parameter | code | Grep(Era.Core/Counter/Comable/ComableChecker.cs) | matches | `ICommonFunctions\s+\w+[,)]` | [ ] |
| 38 | GlobalComableFilter tests verify video camera branch logic | code | Grep(Era.Core.Tests/Counter/GlobalComableFilterTests.cs) | matches | `ビデオカメラ\|VideoCamera\|videocamera\|video.?camera` | [ ] |
| 39 | GlobalComableFilter tests verify SELECTCASE range routing logic | code | Grep(Era.Core.Tests/Counter/GlobalComableFilterTests.cs) | matches | `GetTFlag\|TFlag\|tflag\|COMABLE管理\|ComableManagement` | [ ] |
| 40 | ITEquipVariables injected as ComableChecker constructor parameter | code | Grep(Era.Core/Counter/Comable/ComableChecker.cs) | matches | `ITEquipVariables\s+\w+[,)]` | [ ] |
| 41 | ComableChecker coordinator invokes globalFilter.IsGloballyBlocked | code | Grep(Era.Core/Counter/Comable/ComableChecker.cs) | matches | `globalFilter\.IsGloballyBlocked\(` | [ ] |
| 42 | GlobalComableFilter tests verify BATHROOM exclusion set | code | Grep(Era.Core.Tests/Counter/GlobalComableFilterTests.cs) | matches | `350\|351\|354` | [ ] |
| 43 | COM_ABLE512 uses IsGloballyBlocked(57) redirection in Range5x | code | Grep(Era.Core/Counter/Comable/ComableChecker.Range5x.cs) | matches | `IsGloballyBlocked\(57\)` | [ ] |
| 44 | ComableChecker coordinator has TFLAG:100 pre-check | code | Grep(Era.Core/Counter/Comable/ComableChecker.cs) | matches | `TFlagComable` | [ ] |
| 45 | ILocationService injected as ComableChecker constructor parameter | code | Grep(Era.Core/Counter/Comable/ComableChecker.cs) | matches | `ILocationService\s+\w+[,)]` | [ ] |
| 46 | IItemVariables injected as ComableChecker constructor parameter | code | Grep(Era.Core/Counter/Comable/ComableChecker.cs) | matches | `IItemVariables\s+\w+[,)]` | [ ] |
| 49 | ComableCheckerTests has TFLAG:100 master gate test | code | Grep(Era.Core.Tests/Counter/ComableCheckerTests.cs) | matches | `TFlagComable\|TFLAG.*100\|Tflag.*Comable` | [ ] |
| 50 | COM_ABLE189 permanently disabled case exists in switch | code | Grep(Era.Core/Counter/Comable/ComableChecker*.cs) | matches | `189\s*=>\s*false` | [ ] |
| 51 | GlobalComableFilter tests verify sleep-state branch | code | Grep(Era.Core.Tests/Counter/GlobalComableFilterTests.cs) | matches | `睡眠|SleepState|sleepState|sleep.state` | [ ] |
| 52 | IEngineVariables injected as ComableChecker constructor parameter | code | Grep(Era.Core/Counter/Comable/ComableChecker.cs) | matches | `IEngineVariables\s+\w+[,)]` | [ ] |
| 53 | engine-dev SKILL.md documents IComableUtilities interface | code | Grep(.claude/skills/engine-dev/SKILL.md) | matches | `IComableUtilities` | [ ] |
| 54 | GlobalComableFilter SAVESTR blacklist uses slash-delimited format | code | Grep(Era.Core/Counter/Comable/GlobalComableFilter.cs) | matches | `\$"/{comId}/"` | [ ] |
| 55a | ComableChecker tests cover Range0x low (0-66) | code | Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="IsAvailable\\(([0-9]|[1-5][0-9]|6[0-6])\\)") | gte | 1 | [ ] |
| 55b | ComableChecker tests cover Range0x mid (67-133) | code | Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="IsAvailable\\((6[7-9]|[7-9][0-9]|1[0-2][0-9]|13[0-3])\\)") | gte | 1 | [ ] |
| 55c | ComableChecker tests cover Range0x high (134-199) | code | Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="IsAvailable\\((13[4-9]|1[4-9][0-9])\\)") | gte | 1 | [ ] |
| 56 | StubComableUtilities MasterPose returns conservative default 0 | code | Grep(Era.Core/Counter/Comable/StubComableUtilities.cs) | matches | `return 0` | [ ] |
| 57 | No setter calls in Comable subsystem (query-only enforcement) | code | Grep(Era.Core/Counter/Comable/) | not_matches | `\.Set[A-Z]` | [ ] |
| 58 | No Stub instantiation inside ComableChecker | code | Grep(Era.Core/Counter/Comable/ComableChecker*.cs) | not_matches | `new Stub[A-Z]` | [ ] |
| 59 | KojoComparer VariableStoreAdapter has GetExpLv/SetExpLv stubs | code | Grep(src/tools/dotnet/KojoComparer/YamlRunner.cs) | matches | `GetExpLv\|SetExpLv` | [ ] |

### AC Details

**AC#1: IComAvailabilityChecker interface exists**
- **Test**: `Glob("Era.Core/Counter/Comable/IComAvailabilityChecker.cs")`
- **Expected**: File exists
- **Rationale**: C1 constraint requires the architecture-mandated interface file. This is the core abstraction for COM command availability checking.

**AC#2: ComableChecker class implements IComAvailabilityChecker**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker*.cs", pattern=": IComAvailabilityChecker")`
- **Expected**: Pattern matches (interface implementation on constructor closing line)
- **Rationale**: The main checker class must implement the interface to serve as SSOT. With C# primary constructor syntax, `: IComAvailabilityChecker` appears at the end of the last constructor parameter line (e.g. `GlobalComableFilter globalFilter) : IComAvailabilityChecker`), not on the same line as the class name. Glob pattern `ComableChecker*.cs` allows for split-by-range files per Risk mitigation.

**AC#3: ComableChecker methods return bool**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker*.cs", pattern="public bool IsAvailable")`
- **Expected**: Pattern matches
- **Rationale**: C7 constraint requires bool return type matching the binary RETURN 0/1 pattern from ERB. The IsAvailable method is the primary public API.

**AC#4: ComableChecker uses DI injection (constructor-injected interfaces)**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker*.cs", pattern="IVariableStore variables")`
- **Expected**: Pattern matches (IVariableStore as first DI parameter in primary constructor)
- **Rationale**: C6 constraint requires F801 DI injection pattern. Pattern verifies constructor starts with `IVariableStore` (the primary domain interface), confirming DI injection rather than empty constructor. Full constructor injects IVariableStore, ITEquipVariables, IEngineVariables, ICommonFunctions, ILocationService, IItemVariables, IComableUtilities, GlobalComableFilter. IStringVariables and ICsvNameResolver are only needed by GlobalComableFilter (which is pre-constructed and injected), so ComableChecker does not receive them directly.

**AC#5: No IGameState dependency in Comable subsystem**
- **Test**: `Grep(path="Era.Core/Counter/Comable/", pattern="IGameState")`
- **Expected**: Pattern not found
- **Rationale**: C6 constraint explicitly prohibits IGameState. The architecture originally designed IComAvailabilityChecker with IGameState parameter, but Root Cause Analysis identified this as incorrect. DI injection pattern replaces it.

**AC#6: GlobalComableFilter is public sealed class with public method**
- **Test**: `Grep(path="Era.Core/Counter/Comable/GlobalComableFilter.cs", pattern="public sealed class GlobalComableFilter")`
- **Expected**: Pattern matches
- **Rationale**: C4 constraint requires public accessibility for F810 consumption. COMABLE_300.ERB and COMABLE_400.ERB call GLOBAL_COMABLE 44 times total.

**AC#7: Equivalence tests contain actual test methods for ComableChecker**
- **Test**: `Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="\\[Fact\\]|\\[Theory\\]")`
- **Expected**: Pattern matches (at least one test method attribute found)
- **Rationale**: C3 constraint requires equivalence tests. Verifies test file contains actual xUnit test methods (not just an empty file). Test methods assert C# migration matches ERB behavior for each command range group.

**AC#8: GetAssiPlay method added to IEngineVariables**
- **Test**: `Grep(path="Era.Core/Interfaces/IEngineVariables.cs", pattern="GetAssiPlay\\(")`
- **Expected**: Pattern matches
- **Rationale**: C10 constraint. ASSIPLAY is used 57 times in COMABLE.ERB as engine built-in variable. Needs typed accessor on IEngineVariables.

**AC#9: GetNoItem method added to IItemVariables**
- **Test**: `Grep(path="Era.Core/Interfaces/IItemVariables.cs", pattern="GetNoItem\\(")`
- **Expected**: Pattern matches
- **Rationale**: C11 constraint. NOITEM is used 65 times in COMABLE.ERB for item availability checks. Needs accessor on IItemVariables.

**AC#10: GetExpLv method exists on IVariableStore**
- **Test**: `Grep(path="Era.Core/Interfaces/IVariableStore.cs", pattern="GetExpLv\\(")`
- **Expected**: Pattern matches
- **Rationale**: C13 constraint. EXPLV is used 10 times in COMABLE.ERB. GetExpLv was already added to IVariableStore by F804 (IVariableStore.cs line 115). This AC verifies the pre-existing method is present; no new addition required for this interface in F809. Task 3 adds only SetExpLv as the new paired setter.

**AC#11: GetTrainName method added to ICsvNameResolver**
- **Test**: `Grep(path="Era.Core/Interfaces/ICsvNameResolver.cs", pattern="GetTrainName\\(")`
- **Expected**: Pattern matches
- **Rationale**: C12 constraint. TRAINNAME:comId is used in COMABLE2.ERB:76-124 for video camera restriction logic in GLOBAL_COMABLE. Needs accessor on ICsvNameResolver.

**AC#12: IComableUtilities interface exists with 3-parameter MasterPose method**
- **Test**: `Grep(path="Era.Core/Counter/Comable/IComableUtilities.cs", pattern="MasterPose\\(int.*int.*int")`
- **Expected**: Pattern matches (verifies 3 int parameters matching SOURCE_POSE.ERB:334 @MASTER_POSE(ARG,ARG:1,ARG:2))
- **Rationale**: C9 constraint. MASTER_POSE is called 25 times in COMABLE.ERB but defined in SOURCE_POSE.ERB (F811 scope). Must abstract behind injectable interface with matching 3-parameter signature `int MasterPose(int pose, int arg1, int arg2)`. The strengthened matcher prevents a 1- or 2-parameter signature from passing.

**AC#13: IComableUtilities stub implementation exists**
- **Test**: `Grep(path="Era.Core/Counter/Comable/", pattern="class\\s+\\w+\\s*:\\s*IComableUtilities")`
- **Expected**: Pattern matches (class declaration with colon implementing the interface)
- **Rationale**: C9 constraint. Stub returns conservative default (0/false) until F811 provides concrete implementation. Follows F801 pattern where ICounterUtilities has a separate implementation. The stronger pattern verifies actual interface implementation (class declaration with colon), not just any mention of the interface name.

**AC#14: dotnet build succeeds**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'`
- **Expected**: Exit code 0
- **Rationale**: TreatWarningsAsErrors policy (Directory.Build.props). All new code must compile warning-free.

**AC#15: No TODO/FIXME/HACK in Comable subsystem**
- **Test**: `Grep(path="Era.Core/Counter/Comable/", pattern="TODO|FIXME|HACK")`
- **Expected**: Pattern not found
- **Rationale**: C2 constraint requires zero technical debt. All migrated code must be complete without deferred work markers.

**AC#16: dotnet test passes for Era.Core.Tests**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/'`
- **Expected**: Exit code 0
- **Rationale**: Regression gate. New code and interface extensions must not break existing tests.

**AC#17: IEngineVariables backward compatibility (method count preserved + 1)**
- **Test**: `Grep(path="Era.Core/Interfaces/IEngineVariables.cs", pattern="^\s+(int|void|string|bool|Result<int>)\s+\w+\(")` count
- **Expected**: 25 (GetAssiPlay already present from F812; no new method added by F809 — count stays at 25)
- **Rationale**: Additive-only change verification. Ensures no existing methods were accidentally removed.

**AC#18: IItemVariables backward compatibility (method count preserved + 1)**
- **Test**: `Grep(path="Era.Core/Interfaces/IItemVariables.cs", pattern="^\s+(int|void|string|bool|Result<int>)\s+\w+\(")` count
- **Expected**: 7 (current 6 + 1 GetNoItem)
- **Rationale**: Additive-only change verification for IItemVariables.

**AC#19: IVariableStore backward compatibility (method count preserved + 1)**
- **Test**: `Grep(path="Era.Core/Interfaces/IVariableStore.cs", pattern="^\s+(int|void|string|bool|Result<int>)\s+\w+\(")` count
- **Expected**: 43 (current pattern-matched count 42 (GetCharacterString excluded as Result<string>) + 1 SetExpLv)
- **Rationale**: Additive-only change verification for IVariableStore. The grep pattern matches return types `int|void|string|bool|Result<int>` but does NOT match `Result<string>`. GetCharacterString has return type `Result<string>` and is excluded from the pattern, giving a baseline of 42 matched methods. Only SetExpLv is new for this feature.

**AC#20: ICsvNameResolver backward compatibility (method count preserved + 1)**
- **Test**: `Grep(path="Era.Core/Interfaces/ICsvNameResolver.cs", pattern="^\s+(int|void|string|bool|Result<int>)\s+\w+\(")` count
- **Expected**: 6 (current 5 + 1 GetTrainName)
- **Rationale**: Additive-only change verification for ICsvNameResolver.

**AC#21: SetExpLv method added to IVariableStore**
- **Test**: `Grep(path="Era.Core/Interfaces/IVariableStore.cs", pattern="SetExpLv\\(")`
- **Expected**: Pattern matches
- **Rationale**: C13 constraint. IVariableStore defines both GetExpLv (AC#10) and SetExpLv per the interface convention that every getter has a corresponding setter. Technical Design explicitly specifies `void SetExpLv(int index, int value)`. This AC ensures the setter is not accidentally omitted during implementation.

**AC#22: engine-dev SKILL.md updated with new interfaces/methods**
- **Test**: `Grep(path=".claude/skills/engine-dev/SKILL.md", pattern="IComAvailabilityChecker")`
- **Expected**: Pattern matches
- **Rationale**: Per `.claude/reference/ssot-update-rules.md` lines 15-16, new methods on `IVariableStore` and new interfaces in `Era.Core/Interfaces/` require updating `.claude/skills/engine-dev/SKILL.md`. F809 creates IComAvailabilityChecker, IComableUtilities (new interfaces) and extends 4 existing interfaces. The SSOT must reflect these additions.

**AC#23: ComableChecker switch covers highest COM ID (648)**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker*.cs", pattern="648\\s*=>")`
- **Expected**: Pattern matches
- **Rationale**: C5 constraint requires all 124 COM_ABLE functions migrated. The switch expression in `IsAvailable(int comId)` must cover all COM IDs through the highest ID (648). Verifying that `648 =>` is present in the switch expression confirms range coverage and that the Range6x partial class was not accidentally omitted. Absence of this case would mean the highest COM IDs silently return `false` (default) instead of executing the correct availability logic.

**AC#24: GlobalComableFilter public method IsGloballyBlocked exists**
- **Test**: `Grep(path="Era.Core/Counter/Comable/GlobalComableFilter.cs", pattern="public bool IsGloballyBlocked\\(")`
- **Expected**: Pattern matches
- **Rationale**: C4 constraint requires public API for F810 consumption. AC#6 verifies class existence; this AC verifies the specific method signature F810 depends on (IsGloballyBlocked returns true=blocked, ERB polarity).

**AC#25: GlobalComableFilter equivalence tests cover 5 branch groups**
- **Test**: `Grep(path="Era.Core.Tests/Counter/GlobalComableFilterTests.cs", pattern="\\[Fact\\]|\\[Theory\\]")` count
- **Expected**: >= 5 (at least 5 test methods, one per branch group)
- **Rationale**: Philosophy mandates 'equivalence-tested'. GlobalComableFilter migrates COMABLE2.ERB's GLOBAL_COMABLE (149 lines) with 4 branch groups requiring test coverage: (1) SAVESTR blacklist filter (lines 9-13) — both "comId in blacklist → blocked" and "comId not in blacklist → passes" paths; (2) 7 SELECTCASE range branches (lines 16-67): CASE 000-199 (TFLAG:COMABLE管理!=2 || CFLAG:うふふ==3 → blocked), CASE 200-299 (TFLAG:COMABLE管理!=2 → blocked), CASE 300-309 (TFLAG:COMABLE管理!=1 → blocked), CASE 310-399 (TFLAG:COMABLE管理==2 → blocked; sub-branch BATHROOM+CFLAG:うふふ!=2 with exclusions {350,351,354}), CASE 400-499 (TFLAG:COMABLE管理==2 → blocked), CASE 500-599 (same condition as 000-199), CASE 600-699 (CFLAG:うふふ!=3 → blocked); (3) sleep-state check (line 68): TARGET:1>0 && CFLAG:睡眠==1 && comId outside 400-499 → blocked; (4) 3 video camera branches (lines 74-147): branch A `(TFLAG:ビデオカメラ==1 && !ASSIPLAY) || (TFLAG:ビデオカメラ==2 && ASSIPLAY)` (44-command allowlist, compound OR), branch B `TFLAG:ビデオカメラ==1 && ASSIPLAY` (2-command allowlist: 二穴挿し, 助手を犯す — most restrictive), branch C `TFLAG:ビデオカメラ==2 && !ASSIPLAY` (4-command allowlist); (5) CASEELSE pass-through: a comId outside 000-699 (e.g., comId=800) must pass through the SELECTCASE without being blocked (verifies the empty CASEELSE at COMABLE2.ERB:66 is correctly translated as no-op). Separate test file verifies branch coverage for all 5 branch groups.

**AC#26a: ComableChecker tests cover Range2x (200-299)**
- **Test**: `Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="IsAvailable\\(2[0-9]{2}\\)")`
- **Expected**: Pattern matches (at least one test call with a COM ID in the 200-299 range)
- **Rationale**: Verifies at least one test call uses a COM ID in Range2x. The combined set of AC#26a, AC#26b, AC#26c (with AC#55 for Range0x) ensures all 4 ranges have explicit test coverage.

**AC#26b: ComableChecker tests cover Range5x (500-599)**
- **Test**: `Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="IsAvailable\\(5[0-9]{2}\\)")`
- **Expected**: Pattern matches (at least one test call with a COM ID in the 500-599 range)
- **Rationale**: Verifies at least one test call uses a COM ID in Range5x. The combined set of AC#26a, AC#26b, AC#26c (with AC#55 for Range0x) ensures all 4 ranges have explicit test coverage.

**AC#26c: ComableChecker tests cover Range6x (600-648)**
- **Test**: `Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="IsAvailable\\(6([0-3][0-9]|4[0-8])\\)")`
- **Expected**: Pattern matches (at least one test call with a COM ID in the 600-648 range, covering actual Range6x migrated functions)
- **Rationale**: Verifies at least one test call uses a COM ID in Range6x (strictly 600-648, not 649). The pattern `6([0-3][0-9]|4[0-8])` matches 600-639 (via `6[0-3][0-9]`) and 640-648 (via `64[0-8]`), excluding 649 which is outside the migrated range. The previous pattern `6[0-4][0-9]` incorrectly matched 649. The combined set of AC#26a, AC#26b, AC#26c (with AC#55 for Range0x) ensures all 4 ranges have explicit test coverage.

**AC#27: GlobalComableFilter injected via ComableChecker constructor**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker.cs", pattern="GlobalComableFilter\\s+\\w+[,)]")`
- **Expected**: Pattern matches (constructor parameter declaration)
- **Rationale**: Key Decision documents GlobalComableFilter is injected as concrete class (not interface). AC#4 only verifies IVariableStore is the first parameter. This AC ensures GlobalComableFilter appears as a constructor parameter (either `GlobalComableFilter globalFilter,` or `GlobalComableFilter globalFilter)` at end). Without this, an implementer could instantiate GlobalComableFilter internally, breaking F810's ability to inject the same instance.

**AC#28: All 4 ComableChecker range partial files exist**
- **Test**: `Glob("Era.Core/Counter/Comable/ComableChecker.Range*.cs")` count
- **Expected**: 4 files (Range0x.cs, Range2x.cs, Range5x.cs, Range6x.cs)
- **Rationale**: C5 constraint requires all 124 functions migrated. AC#23 verifies the switch covers the highest ID (648) but cannot confirm all 4 range partial files were created. This AC verifies all 4 range files exist, ensuring no range group was accidentally omitted. Combined with AC#23 (highest ID) and AC#26a/26b/26c (per-range test coverage), this provides structural completeness verification.

**AC#29: engine.Tests passes after StubVariableStore extension**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test engine.Tests/'`
- **Expected**: Exit code 0
- **Rationale**: Task 6b adds GetExpLv/SetExpLv virtual stubs to StubVariableStore in engine.Tests/Tests/TestStubs.cs. AC#14 and AC#16 only verify Era.Core and Era.Core.Tests. This AC ensures the StubVariableStore extension does not break engine.Tests compilation or existing tests.

**AC#31a: COM_ABLE507 anomaly case tested**
- **Test**: `Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="IsAvailable\\(507\\)")`
- **Expected**: Pattern matches
- **Rationale**: COM_ABLE507 intentionally skips GLOBAL_COMABLE (COMABLE.ERB:2997-3004). The coordinator pre-check excludes 507 (`comId != 507`). Each anomaly gets its own AC to guarantee independent verification — a single OR-alternation pattern with count_gte cannot distinguish which IDs contributed to the count.

**AC#31b: COM_ABLE512 anomaly case tested**
- **Test**: `Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="IsAvailable\\(512\\)")`
- **Expected**: Pattern matches
- **Rationale**: COM_ABLE512 uses GLOBAL_COMABLE(57) instead of GLOBAL_COMABLE(512) (COMABLE.ERB:3136-3141). The coordinator pre-check excludes 512 (`comId != 512`); IsAvailable512() in Range5x.cs calls `globalFilter.IsGloballyBlocked(57)` explicitly.

**AC#31c: COM_ABLE189 anomaly case tested**
- **Test**: `Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="IsAvailable\\(189\\)")`
- **Expected**: Pattern matches
- **Rationale**: COM_ABLE189 is permanently disabled (all conditions commented out in COMABLE.ERB:2722-2750, returns false unconditionally). Uses `189 => false` in the switch expression. Test must verify this unconditional false behavior.

**AC#30: IComAvailabilityChecker interface has exactly 1 method signature**
- **Test**: `Grep(path="Era.Core/Counter/Comable/IComAvailabilityChecker.cs", pattern="^\s+(bool|int|void|string)\s+\w+\(")` count
- **Expected**: 1 (only IsAvailable method)
- **Rationale**: Architecture spec (phase-20-27-game-systems.md:130) defines 2 methods (IsAvailable + GetUnavailableReason), but GetUnavailableReason is deferred to F813 (Upstream Issues). This AC prevents an implementer from accidentally adding a stub GetUnavailableReason method. Single-method contract enforced.

**AC#55a: ComableChecker tests cover Range0x low (0-66)**
- **Test**: `Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="IsAvailable\\(([0-9]|[1-5][0-9]|6[0-6])\\)")`
- **Expected**: At least 1 match (gte 1)
- **Rationale**: Range0x contains 87 COM_ABLE functions (70% of migration). Split into 3 sub-ranges to enforce distinct COM ID testing, mirroring AC#26a/26b/26c pattern for other ranges. Low range covers COM IDs 0-66. Pattern includes single-digit IDs 0-9 (e.g., `IsAvailable(0)`) via `[0-9]`, two-digit 10-59 via `[1-5][0-9]`, and 60-66 via `6[0-6]`.

**AC#55b: ComableChecker tests cover Range0x mid (67-133)**
- **Test**: `Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="IsAvailable\\((6[7-9]|[7-9][0-9]|1[0-2][0-9]|13[0-3])\\)")`
- **Expected**: At least 1 match (gte 1)
- **Rationale**: Mid range covers COM IDs 67-133. Pattern includes `6[7-9]` for 67-69, `[7-9][0-9]` for 70-99, `1[0-2][0-9]` for 100-129, `13[0-3]` for 130-133. Ensures tests span the middle of Range0x, not just boundary values.

**AC#55c: ComableChecker tests cover Range0x high (134-199)**
- **Test**: `Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="IsAvailable\\((13[4-9]|1[4-9][0-9])\\)")`
- **Expected**: At least 1 match (gte 1)
- **Rationale**: High range covers COM IDs 134-199. Pattern: `13[4-9]` for 134-139, `1[4-9][0-9]` for 140-199. Non-overlapping with AC#55b (ends at 133). Note: COM_ABLE189 (permanently disabled per AC#50) is within this range; AC#31c independently verifies IsAvailable(189) is tested. AC#55c's gte 1 matcher ensures at least one test exists for any ID in 134-199.

**AC#32: ComableChecker switch expression dispatches 123 COM_ABLE functions via IsAvailableN()**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker.cs", pattern="=> IsAvailable\\d+\\(\\)")` count
- **Expected**: 123 (123 dispatch entries + COM_ABLE189 which uses `189 => false` directly)
- **Rationale**: AC#23 verifies the highest ID (648) and AC#29 verifies 4 range files exist, but neither enforces the total function count. The Goal states "Migrate all 124 @COM_ABLE{N} functions" — a partial implementation with fewer functions would pass AC#23 and AC#29 but fail this count check. Grep scoped to the coordinator file only (ComableChecker.cs, not *.cs) to prevent false positives from range file expression-body methods. COM_ABLE189 is permanently disabled (all conditions commented out in COMABLE.ERB:2722-2750) and uses `189 => false` in the switch (documented in Upstream Issues), so it does not match the `IsAvailableN()` dispatch pattern. The 123 count = Range0x (86 dispatching + 1 direct false) + Range2x (4) + Range5x (16, includes 507 and 512) + Range6x (17). COM_ABLE507 and COM_ABLE512 both use `=> IsAvailableN()` dispatch in the switch expression — the coordinator-level global filter exclusion only affects the pre-check before the switch, not the switch case format itself. Combined with AC#31 (which verifies IsAvailable(189) is tested), all 124 functions are covered. Evidence: COMABLE.ERB contains 87 @COM_ABLE{N} functions in IDs 0-199 (verified by Grep `^@COM_ABLE[0-9]+` in COMABLE.ERB, filtering IDs 0-199: 86 dispatching + 1 permanently-disabled COM_ABLE189).

**AC#35a: GlobalComableFilter constructor has IVariableStore**
- **Test**: `Grep(path="Era.Core/Counter/Comable/GlobalComableFilter.cs", pattern="IVariableStore variables")`
- **Expected**: Pattern matches
- **Rationale**: GlobalComableFilter has 5 constructor-injected interfaces per Technical Design. IVariableStore provides TFLAG/CFLAG access for SELECTCASE branch conditions.

**AC#35b: GlobalComableFilter constructor has IStringVariables**
- **Test**: `Grep(path="Era.Core/Counter/Comable/GlobalComableFilter.cs", pattern="IStringVariables")`
- **Expected**: Pattern matches
- **Rationale**: IStringVariables is required for the SAVESTR blacklist check (COMABLE2.ERB:9-13). Without it, the first gating condition is non-functional.

**AC#35c: GlobalComableFilter constructor has ICsvNameResolver**
- **Test**: `Grep(path="Era.Core/Counter/Comable/GlobalComableFilter.cs", pattern="ICsvNameResolver")`
- **Expected**: Pattern matches
- **Rationale**: ICsvNameResolver is required for TRAINNAME video camera check (COMABLE2.ERB:76-124). Without it, the 3 video camera branches cannot resolve command names.

**AC#35d: GlobalComableFilter constructor has IEngineVariables**
- **Test**: `Grep(path="Era.Core/Counter/Comable/GlobalComableFilter.cs", pattern="IEngineVariables")`
- **Expected**: Pattern matches
- **Rationale**: IEngineVariables provides engine.GetTarget(), engine.GetMaster(), and GetAssiPlay() — required for CFLAG character resolution (TARGET/MASTER context) and video camera ASSIPLAY branches.

**AC#35e: GlobalComableFilter constructor has ILocationService**
- **Test**: `Grep(path="Era.Core/Counter/Comable/GlobalComableFilter.cs", pattern="ILocationService")`
- **Expected**: Pattern matches
- **Rationale**: ILocationService provides IsBathroom() — required for CASE 310-399 BATHROOM sub-branch (COMABLE2.ERB:42). Without it, the BATHROOM check cannot be implemented.

**AC#36a: engine-dev SKILL.md documents GetAssiPlay**
- **Test**: `Grep(path=".claude/skills/engine-dev/SKILL.md", pattern="GetAssiPlay")`
- **Expected**: Pattern matches
- **Rationale**: Per SSOT update rules, new methods on existing interfaces require SKILL.md documentation. Each extension method gets its own AC to guarantee independent verification — a single OR-pattern cannot distinguish which methods contributed to the match count. See AC#31a-c for the same pattern.

**AC#36b: engine-dev SKILL.md documents GetNoItem**
- **Test**: `Grep(path=".claude/skills/engine-dev/SKILL.md", pattern="GetNoItem")`
- **Expected**: Pattern matches
- **Rationale**: Independent verification for GetNoItem on IItemVariables (65 uses in COMABLE.ERB).

**AC#36c: engine-dev SKILL.md documents GetTrainName**
- **Test**: `Grep(path=".claude/skills/engine-dev/SKILL.md", pattern="GetTrainName")`
- **Expected**: Pattern matches
- **Rationale**: Independent verification for GetTrainName on ICsvNameResolver (COMABLE2.ERB:76-124 video camera check).

**AC#36d: engine-dev SKILL.md documents GetExpLv**
- **Test**: `Grep(path=".claude/skills/engine-dev/SKILL.md", pattern="GetExpLv")`
- **Expected**: Pattern matches
- **Rationale**: Independent verification for GetExpLv on IVariableStore (10 uses in COMABLE.ERB).

**AC#36e: engine-dev SKILL.md documents SetExpLv**
- **Test**: `Grep(path=".claude/skills/engine-dev/SKILL.md", pattern="SetExpLv")`
- **Expected**: Pattern matches
- **Rationale**: SetExpLv is added to IVariableStore (AC#21, Task 3) as the paired setter for GetExpLv. Per SSOT update rules, all new IVariableStore methods require SKILL.md documentation. AC#36d covers the getter; this AC covers the setter.

**AC#33: IComableUtilities injected as ComableChecker constructor parameter**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker.cs", pattern="IComableUtilities\\s+\\w+[,)]")`
- **Expected**: Pattern matches (IComableUtilities appears as constructor parameter)
- **Rationale**: AC#12 verifies IComableUtilities interface exists and AC#13 verifies a stub implementation exists, but neither ensures the interface is actually injected into ComableChecker. Without injection, the 25 MASTER_POSE calls in Range methods would need to use `new StubComableUtilities()` directly, preventing F811 from later providing a concrete implementation via DI. This AC ensures the injectable pattern is enforced.

**AC#34: GlobalComableFilter constructible in test context with stub dependencies**
- **Test**: `Grep(path="Era.Core.Tests/Counter/GlobalComableFilterTests.cs", pattern="new GlobalComableFilter\\(")`
- **Expected**: Pattern matches (GlobalComableFilter instantiated with stub dependencies)
- **Rationale**: GlobalComableFilter is injected as a concrete class (not interface) into ComableChecker (Key Decision). Both GlobalComableFilterTests.cs (direct testing) and ComableCheckerTests.cs (indirect via ComableChecker constructor) must construct GlobalComableFilter with stub implementations. This AC verifies the concrete class is constructible in the test context — if the constructor is incompatible with test stubs, all GlobalComableFilter and ComableChecker tests break. AC#25 verifies test methods exist but not that GlobalComableFilter is instantiated. This AC closes the gap between the concrete injection design decision and test feasibility.

**AC#35: GetUnavailableReason not prematurely added to IComAvailabilityChecker**
- **Test**: `Grep(path="Era.Core/Counter/Comable/IComAvailabilityChecker.cs", pattern="GetUnavailableReason")`
- **Expected**: Pattern not found
- **Rationale**: Architecture spec (phase-20-27-game-systems.md:130) defines GetUnavailableReason as a second method, but it is deferred to F813. AC#32 uses count_equals=1 with a pattern that only matches {bool, int, void, string} return types — a method with a custom return type (e.g., `ComUnavailableReason`) would bypass AC#32. This not_matches AC provides a direct guard against the specific deferred method being prematurely added, regardless of its return type.

**AC#37: ICommonFunctions injected as ComableChecker constructor parameter**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker.cs", pattern="ICommonFunctions\\s+\\w+[,)]")`
- **Expected**: Pattern matches (ICommonFunctions appears as constructor parameter)
- **Rationale**: ICommonFunctions provides HAS_PENIS/HAS_VAGINA methods used across ~15+ COMABLE.ERB functions. AC#4 only verifies IVariableStore as first parameter, and AC#37 verifies IComableUtilities. This AC closes the gap for ICommonFunctions, ensuring the DI pattern includes the domain utility interface. Without it, an implementer could omit the dependency and use hardcoded logic.

**AC#38: GlobalComableFilter tests verify video camera branch logic**
- **Test**: `Grep(path="Era.Core.Tests/Counter/GlobalComableFilterTests.cs", pattern="ビデオカメラ|VideoCamera|videocamera|video.?camera")`
- **Expected**: Pattern matches
- **Rationale**: AC#25 requires 5+ test methods but cannot enforce which branch groups are covered. The video camera branches (COMABLE2.ERB:74-147) are the most complex logic in GLOBAL_COMABLE with 3 distinct sub-branches (44-command allowlist, 2-command allowlist, 4-command allowlist). This AC verifies the test file explicitly references the video camera concept, ensuring at least one test targets this branch group. Combined with AC#25 (5 method minimum) and AC#38 (constructibility), this provides coverage breadth verification.

**AC#39: GlobalComableFilter tests verify SELECTCASE range routing logic**
- **Test**: `Grep(path="Era.Core.Tests/Counter/GlobalComableFilterTests.cs", pattern="GetTFlag|TFlag|tflag|COMABLE管理|ComableManagement")`
- **Expected**: Pattern matches
- **Rationale**: AC#25 requires 5+ test methods but cannot enforce which branch groups are covered. AC#42 enforces video camera coverage. This AC enforces the SELECTCASE range routing branch group (COMABLE2.ERB:16-67) which uses TFLAG:COMABLE管理 as the primary branching condition across 7 CASE ranges. The test file must reference this concept (either via the Japanese constant name or English equivalent) to demonstrate range routing logic is being tested. Without this, all 5+ test methods could focus on SAVESTR blacklist and video camera while the core routing logic goes untested.

**AC#40: ITEquipVariables injected as ComableChecker constructor parameter**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker.cs", pattern="ITEquipVariables\\s+\\w+[,)]")`
- **Expected**: Pattern matches (ITEquipVariables appears as constructor parameter)
- **Rationale**: ITEquipVariables provides TEQUIP/EQUIP variable access used by COM_ABLE functions that check equipment states. AC#4 verifies IVariableStore, AC#41 ICommonFunctions, AC#37 IComableUtilities, AC#28 GlobalComableFilter — but ITEquipVariables was missing individual verification. This AC ensures the TEQUIP domain interface is not accidentally omitted from the constructor.

**AC#41: ComableChecker coordinator invokes globalFilter.IsGloballyBlocked**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker.cs", pattern="globalFilter\\.IsGloballyBlocked\\(")`
- **Expected**: Pattern matches
- **Rationale**: AC#28 verifies GlobalComableFilter is injected as a constructor parameter, and AC#6/AC#24 verify the class and method exist. But injection without invocation passes all those ACs. This AC verifies the coordinator's IsAvailable method body actually calls the centralized filter, confirming the GLOBAL_COMABLE pre-check design is implemented. Without this, an implementer could inject GlobalComableFilter, write all 123 range methods, and never apply the centralized gating — each COM_ABLE function would miss the GLOBAL_COMABLE filter.

**AC#42: GlobalComableFilter tests verify BATHROOM exclusion set**
- **Test**: `Grep(path="Era.Core.Tests/Counter/GlobalComableFilterTests.cs", pattern="350|351|354")`
- **Expected**: Pattern matches (at least one exclusion set ID referenced in tests)
- **Rationale**: COMABLE2.ERB CASE 310-399 has a BATHROOM sub-branch with exclusion IDs {350,351,354} that are NOT blocked even when the BATHROOM condition would normally block. AC#43 verifies SELECTCASE range routing is referenced in tests, but cannot enforce that the BATHROOM exclusion logic is specifically tested. If the implementer omits the exclusion check, commands 350/351/354 would be incorrectly blocked in BATHROOM context, and no existing AC would detect this. This AC requires at least one of the exclusion set IDs to appear in the test file.

**AC#43: COM_ABLE512 uses IsGloballyBlocked(57) redirection in Range5x**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker.Range5x.cs", pattern="IsGloballyBlocked\\(57\\)")`
- **Expected**: Pattern matches
- **Rationale**: COM_ABLE512 is a documented anomaly (COMABLE.ERB:3136-3141): it calls GLOBAL_COMABLE(57) instead of GLOBAL_COMABLE(512). The coordinator excludes 512 from the centralized pre-check (`comId != 512`), and IsAvailable512() must call `globalFilter.IsGloballyBlocked(57)` explicitly. AC#31b verifies the test exists but cannot verify the internal redirection. AC#45 verifies the coordinator's general invocation but not the per-function anomaly. This AC directly verifies the ERB polarity preservation for COM_ABLE512.

**AC#44: ComableChecker coordinator has TFLAG:100 pre-check**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker.cs", pattern="TFlagComable")`
- **Expected**: Pattern matches
- **Rationale**: The coordinator's IsAvailable method has a TFLAG:100 guard as the first pre-check (`if (variables.GetTFlag((FlagIndex)TFlagComable) == 0) return false`). This is the master COMABLE-management-disabled gate — when TFLAG:100 is 0, ALL commands are unavailable regardless of individual checks. AC#45 verifies the globalFilter call but not this separate TFLAG:100 gate. Without this AC, an implementer could omit the TFLAG:100 check and leave COMABLE functions active even when the management flag is unset.

**AC#45: ILocationService injected as ComableChecker constructor parameter**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker.cs", pattern="ILocationService\\s+\\w+[,)]")`
- **Expected**: Pattern matches (ILocationService appears as constructor parameter)
- **Rationale**: ILocationService provides IsBathroom()/IsBedroom() methods used by COM_ABLE functions that check location state. AC#4 only verifies IVariableStore as first parameter; AC#41 verifies ICommonFunctions; AC#44 verifies ITEquipVariables; AC#37 verifies IComableUtilities; AC#28 verifies GlobalComableFilter. This AC closes the gap for ILocationService, ensuring the location domain interface is not accidentally omitted from the ComableChecker constructor.

**AC#46: IItemVariables injected as ComableChecker constructor parameter**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker.cs", pattern="IItemVariables\\s+\\w+[,)]")`
- **Expected**: Pattern matches (IItemVariables appears as constructor parameter)
- **Rationale**: IItemVariables provides GetNoItem() and item access methods used by COM_ABLE functions that check item availability (NOITEM is used 65 times in COMABLE.ERB). AC#9 verifies GetNoItem exists on the interface but does not verify it is injected into ComableChecker. Without injection, range methods cannot access item variables and would need to use alternative access paths, breaking the DI pattern.

**AC#49: ComableCheckerTests has TFLAG:100 master gate test**
- **Test**: `Grep(path="Era.Core.Tests/Counter/ComableCheckerTests.cs", pattern="TFlagComable|TFLAG.*100|Tflag.*Comable")`
- **Expected**: Pattern matches
- **Rationale**: The TFLAG:100 pre-check is the single master gate controlling all 124 COM commands — when TFLAG:100 == 0, ALL commands return false regardless of individual checks. AC#44 verifies the constant `TFlagComable` exists in the coordinator implementation, but no AC verifies the gate behavior is actually tested. Without this AC, an implementer could add a test file that never exercises the master gate path, leaving the most critical pre-check without test coverage. This AC ensures `TFlagComable`, `TFLAG.*100`, or `Tflag.*Comable` appears in ComableCheckerTests.cs, confirming at least one test targets the TFLAG:100 gate.

**AC#50: COM_ABLE189 permanently disabled case exists in switch**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker*.cs", pattern="189\\s*=>\\s*false")`
- **Expected**: Pattern matches
- **Rationale**: COM_ABLE189 is permanently disabled (all conditions commented out in COMABLE.ERB:2722-2750, returns false unconditionally). The switch expression must contain `189 => false` as a direct constant-false dispatch, not a call to `IsAvailable189()`. AC#31c verifies the test for this anomaly, but no AC verifies the implementation-side constant-false case is present in the switch. Without this AC, an implementer could accidentally add an `IsAvailable189()` method in Range0x with incorrect logic, bypassing the documented permanent-disable behavior.

**AC#51: GlobalComableFilter tests verify sleep-state branch**
- **Test**: `Grep(path="Era.Core.Tests/Counter/GlobalComableFilterTests.cs", pattern="睡眠|SleepState|sleepState|sleep.state")`
- **Expected**: Pattern matches
- **Rationale**: The sleep-state branch (TARGET:1>0 && CFLAG:睡眠==1 && comId outside 400-499 → blocked, COMABLE2.ERB line 68) is one of 5 branch groups in GlobalComableFilter. AC#42 (video camera), AC#43 (SELECTCASE routing), AC#46 (BATHROOM exclusion) enforce specific branches, but sleep-state had no targeted AC. AC#25 requires 5+ test methods but cannot enforce which branch groups are covered. Without this AC, an implementer could skip the sleep-state branch in tests while AC#25 still passes via other branch group tests. This AC ensures the sleep-state concept is explicitly referenced in GlobalComableFilterTests.cs.

**AC#52: IEngineVariables injected as ComableChecker constructor parameter**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker.cs", pattern="IEngineVariables\\s+\\w+[,)]")`
- **Expected**: Pattern matches (IEngineVariables appears as constructor parameter)
- **Rationale**: IEngineVariables provides GetAssiPlay() (used 57 times in COMABLE.ERB), GetTarget(), GetMaster(). Mirrors AC#41/44/49/50/51/52 pattern. AC#4 only verifies IVariableStore as the first constructor parameter; AC#35d verifies IEngineVariables in GlobalComableFilter but not in ComableChecker itself. Without this AC, an implementer could omit IEngineVariables from the ComableChecker constructor, breaking range methods that access engine built-ins (ASSIPLAY, TARGET, MASTER).

**AC#53: engine-dev SKILL.md documents IComableUtilities interface**
- **Test**: `Grep(path=".claude/skills/engine-dev/SKILL.md", pattern="IComableUtilities")`
- **Expected**: Pattern matches
- **Rationale**: AC#22 verifies IComAvailabilityChecker is documented. This AC provides cross-check by verifying IComableUtilities (the second new interface created by F809) is also documented, ensuring the SSOT update covers more than just one interface. Together AC#22 + AC#53 verify both new interfaces are in SKILL.md.

**AC#54: GlobalComableFilter SAVESTR blacklist uses slash-delimited format**
- **Test**: `Grep(path="Era.Core/Counter/Comable/GlobalComableFilter.cs", pattern="\\$\"/{comId}/")`
- **Expected**: Pattern matches
- **Rationale**: COMABLE2.ERB:9-10 uses LOCALS = "/"+TOSTR(ARG)+"/" then STRCOUNT(SAVESTR:0, LOCALS) — the slash delimiters prevent substring false positives (e.g., comId=5 matching "/50/"). Without delimiters, .Contains("5") would match "50", "51", etc. This AC verifies the C# implementation preserves the ERB's delimiter-based matching pattern. The interpolated string `$"/{comId}/"` is the canonical C# translation documented in the Technical Design's GlobalComableFilter code snippet.

**AC#56: StubComableUtilities MasterPose returns conservative default 0**
- **Test**: `Grep(path="Era.Core/Counter/Comable/StubComableUtilities.cs", pattern="return 0")`
- **Expected**: Pattern matches
- **Rationale**: Philosophy requires MasterPose stub to return 0 (conservative default: pose not satisfied until F811 provides concrete implementation). Without this AC, an implementation returning 1 (permissive) would pass all other ACs while incorrectly enabling 25 COM_ABLE functions that depend on MASTER_POSE.

**AC#57: No setter calls in Comable subsystem (query-only enforcement)**
- **Test**: `Grep(path="Era.Core/Counter/Comable/", pattern="\.Set[A-Z]")`
- **Expected**: Pattern not found (0 matches)
- **Rationale**: Philosophy mandates "All migrated logic must be query-only (read-only access, no side effects)." AC#36 only guards SetExpLv specifically. AC#57 provides comprehensive guard against ALL setter calls (Set*) in the Comable subsystem. Since ComableChecker injects IVariableStore which exposes setters, this AC prevents accidental state mutation in COM_ABLE migration.

**AC#58: No Stub instantiation inside ComableChecker**
- **Test**: `Grep(path="Era.Core/Counter/Comable/ComableChecker*.cs", pattern="new Stub[A-Z]")`
- **Expected**: Pattern not found (0 matches)
- **Rationale**: DI injection pattern requires dependencies to be injected via constructor, not instantiated internally. Prevents accidental hard-coding of `new StubComableUtilities()` inside range methods, which would bypass the IComableUtilities injection and create hidden coupling to the temporary stub. AC#33 verifies the constructor parameter; AC#58 verifies no internal instantiation bypasses it.

**AC#59: KojoComparer VariableStoreAdapter has GetExpLv/SetExpLv stubs**
- **Test**: `Grep(path="src/tools/dotnet/KojoComparer/YamlRunner.cs", pattern="GetExpLv|SetExpLv")`
- **Expected**: Pattern matches
- **Rationale**: Task 3c requires updating `src/tools/dotnet/KojoComparer/YamlRunner.cs` VariableStoreAdapter with `GetExpLv`/`SetExpLv` stubs (as documented in the Mandatory Handoffs table and Execution Order step 3c). Without this, the KojoComparer build will fail after F809 adds `SetExpLv` to `IVariableStore` (CS0535: class does not implement interface member). This AC verifies the stubs were added by confirming both method names appear in YamlRunner.cs.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Migrate all 124 @COM_ABLE{N} functions from COMABLE.ERB into C# classes under Era.Core/Counter/Comable/ | AC#1, AC#2, AC#3, AC#4, AC#23, AC#28, AC#31a, AC#31b, AC#31c, AC#32, AC#43, AC#44, AC#50 |
| 2 | Migrate @GLOBAL_COMABLE from COMABLE2.ERB into C# | AC#6, AC#24, AC#25, AC#38, AC#39, AC#41, AC#42, AC#54 |
| 3 | Follow F801 DI injection pattern (constructor-injected interfaces, not IGameState) | AC#4, AC#5, AC#27, AC#35a, AC#35b, AC#35c, AC#35d, AC#35e, AC#34, AC#37, AC#40, AC#45, AC#46, AC#52 |
| 4 | Resolve interface gap: ASSIPLAY | AC#8, AC#17 |
| 5 | Resolve interface gap: NOITEM | AC#9, AC#18 |
| 6 | Resolve interface gap: EXPLV | AC#10, AC#19, AC#21 |
| 7 | Resolve interface gap: TRAINNAME | AC#11, AC#20 |
| 8 | Expose GLOBAL_COMABLE as public API for F810 consumption | AC#6, AC#24 |
| 9 | Abstract MASTER_POSE behind injectable stub interface | AC#12, AC#13, AC#33, AC#56 |
| 10 | Zero technical debt | AC#15, AC#22, AC#57, AC#58 |
| 11 | Equivalence tested | AC#7, AC#25, AC#26a, AC#26b, AC#26c, AC#55a, AC#55b, AC#55c, AC#31a, AC#31b, AC#31c, AC#38, AC#39, AC#42, AC#49, AC#51 |
| 12 | Build and test succeeds | AC#14, AC#16, AC#29, AC#59 |
| 13 | SSOT update (engine-dev SKILL.md reflects new interfaces/methods) | AC#22, AC#53, AC#36a, AC#36b, AC#36c, AC#36d, AC#36e |
| 14 | IComAvailabilityChecker single-method contract (GetUnavailableReason deferred to F813) | AC#30, AC#35 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Mirror the F801 ActionValidator pattern exactly: one sealed class per command range, all injected via constructor parameters, returning `bool` from `IsAvailable(ComId id)`. The core entry point is `ComableChecker` (coordinator) which dispatches to range-specific partial classes: `ComableChecker.Range0x.cs` (0-199), `ComableChecker.Range2x.cs` (200-299), `ComableChecker.Range5x.cs` (500-599), `ComableChecker.Range6x.cs` (600-648). Each range partial contains the corresponding `@COM_ABLE{N}` logic as a private bool-returning method called from a central `switch` expression inside `IsAvailable`. The `GLOBAL_COMABLE` function is extracted into a separate `GlobalComableFilter` public class with a single `IsGloballyBlocked(int comId)` public method, consumed by both `ComableChecker` (before individual checks) and by F810.

Five interface extensions are added as additive-only changes: `GetAssiPlay()` on `IEngineVariables`, `GetNoItem()` on `IItemVariables`, `GetExpLv(int)` / `SetExpLv(int, int)` on `IVariableStore`, and `GetTrainName(int)` on `ICsvNameResolver`. `MASTER_POSE` is abstracted as `IComableUtilities.MasterPose(int pose, int arg1, int arg2)` with a conservative stub returning `0`.

Each `@COM_ABLE{N}` function follows a fixed translation pattern:
1. Global TFLAG:100 check and `GlobalComableFilter` pre-check are done ONCE in the coordinator's `IsAvailable(int comId)` method, before the switch dispatch (not repeated in each range method). Two exceptions: COM_ABLE507 intentionally omits the GLOBAL_COMABLE call in ERB (COMABLE.ERB:2997-3004) and COM_ABLE512 calls GLOBAL_COMABLE(57) instead of GLOBAL_COMABLE(512) (COMABLE.ERB:3136-3141). The coordinator excludes both from the centralized check (`comId != 507 && comId != 512`); COM_ABLE512's range method handles the global filter explicitly with `globalFilter.IsGloballyBlocked(57)`.
2. Each `@COM_ABLE{N}` function's individual conditions are translated in the range methods as early-return `false` guards.

This satisfies all ACs by construction: file structure (AC#1, AC#2), bool return (AC#3), DI constructor (AC#4), no IGameState (AC#5), public GlobalComableFilter (AC#6), tests (AC#7), four interface extensions (AC#8-11), IComableUtilities stub (AC#12-13), build (AC#14), no debt markers (AC#15), test pass (AC#16), backward-compatibility counts (AC#17-20), SetExpLv setter (AC#21), engine-dev SKILL.md SSOT update (AC#22).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create IComAvailabilityChecker.cs in Era.Core/Counter/Comable/ |
| 2 | ComableChecker class declaration with : IComAvailabilityChecker |
| 3 | Public bool IsAvailable method in ComableChecker |
| 4 | Primary constructor with IVariableStore as first parameter |
| 5 | No IGameState references in Era.Core/Counter/Comable/ |
| 6 | GlobalComableFilter declared as public sealed class |
| 7 | Add [Fact]/[Theory] test methods to ComableCheckerTests.cs |
| 8 | Add int GetAssiPlay() to IEngineVariables.cs |
| 9 | Add int GetNoItem() to IItemVariables.cs |
| 10 | Add int GetExpLv(int index) to IVariableStore.cs |
| 11 | Add string GetTrainName(int comId) to ICsvNameResolver.cs |
| 12 | IComableUtilities.MasterPose(int, int, int) signature |
| 13 | StubComableUtilities : IComableUtilities class creation |
| 14 | Run dotnet build Era.Core/ — exit code 0 |
| 15 | Grep Era.Core/Counter/Comable/ for TODO/FIXME/HACK — 0 matches |
| 16 | Run dotnet test Era.Core.Tests/ — exit code 0 |
| 17 | Grep IEngineVariables.cs for method signatures — count equals 25 |
| 18 | Grep IItemVariables.cs for method signatures — count equals 7 |
| 19 | Grep IVariableStore.cs for method signatures — count equals 43 (current pattern-matched count 42 (GetCharacterString excluded as Result<string>) + 1 SetExpLv) |
| 20 | Grep ICsvNameResolver.cs for method signatures — count equals 6 |
| 21 | Add void SetExpLv(int index, int value) to IVariableStore.cs |
| 22 | Add IComAvailabilityChecker documentation to engine-dev SKILL.md |
| 23 | Switch expression case `648 => IsAvailable648()` in Range6x.cs |
| 24 | Public bool IsGloballyBlocked(int comId) method in GlobalComableFilter.cs |
| 25 | Create 5+ [Fact]/[Theory] test methods in GlobalComableFilterTests.cs covering all branch groups |
| 26a | ComableCheckerTests.cs contains at least one `IsAvailable(NNN)` call where NNN is in Range2x (200-299) |
| 26b | ComableCheckerTests.cs contains at least one `IsAvailable(NNN)` call where NNN is in Range5x (500-599) |
| 26c | ComableCheckerTests.cs contains at least one `IsAvailable(NNN)` call where NNN is in Range6x (600-648) |
| 53 | Add IComableUtilities documentation to engine-dev SKILL.md |
| 27 | GlobalComableFilter parameter in ComableChecker primary constructor |
| 28 | Create 4 partial files: ComableChecker.Range0x.cs, Range2x.cs, Range5x.cs, Range6x.cs |
| 29 | Run dotnet test engine.Tests/ — exit code 0 |
| 31a | ComableCheckerTests.cs contains `IsAvailable(507)` — verifies COM_ABLE507 (skips GLOBAL_COMABLE) anomaly is tested |
| 31b | ComableCheckerTests.cs contains `IsAvailable(512)` — verifies COM_ABLE512 (uses GLOBAL_COMABLE(57)) anomaly is tested |
| 31c | ComableCheckerTests.cs contains `IsAvailable(189)` — verifies COM_ABLE189 (permanently disabled) anomaly is tested |
| 30 | Single bool IsAvailable(int comId) method in IComAvailabilityChecker |
| 55a | ComableCheckerTests.cs contains at least one `IsAvailable(N)` call where N is in Range0x low (0-66) |
| 55b | ComableCheckerTests.cs contains at least one `IsAvailable(N)` call where N is in Range0x mid (67-133) |
| 55c | ComableCheckerTests.cs contains at least one `IsAvailable(N)` call where N is in Range0x high (134-199, excluding 189) |
| 32 | 123 switch dispatch entries matching `=> IsAvailableN()` pattern |
| 35a | GlobalComableFilter constructor has IVariableStore — verifies DI injection pattern |
| 35b | GlobalComableFilter has IStringVariables — required for SAVESTR blacklist |
| 35c | GlobalComableFilter has ICsvNameResolver — required for TRAINNAME video camera |
| 35d | GlobalComableFilter has IEngineVariables — required for TARGET/MASTER/ASSIPLAY |
| 35e | GlobalComableFilter has ILocationService — required for BATHROOM sub-branch |
| 36a | engine-dev SKILL.md documents GetAssiPlay — verifies IEngineVariables extension in SSOT |
| 36b | engine-dev SKILL.md documents GetNoItem — verifies IItemVariables extension in SSOT |
| 36c | engine-dev SKILL.md documents GetTrainName — verifies ICsvNameResolver extension in SSOT |
| 36d | engine-dev SKILL.md documents GetExpLv — verifies IVariableStore getter extension in SSOT |
| 36e | engine-dev SKILL.md documents SetExpLv — verifies IVariableStore setter extension in SSOT |
| 33 | IComableUtilities parameter in ComableChecker constructor |
| 34 | `new GlobalComableFilter(...)` construction in GlobalComableFilterTests.cs |
| 35 | No GetUnavailableReason in IComAvailabilityChecker.cs |
| 57 | No Set* calls in Era.Core/Counter/Comable/ (comprehensive query-only guard) |
| 37 | ICommonFunctions parameter in ComableChecker constructor |
| 38 | Video camera test reference in GlobalComableFilterTests.cs |
| 39 | TFLAG/GetTFlag/COMABLE管理 reference in GlobalComableFilterTests.cs (SELECTCASE routing) |
| 40 | ITEquipVariables parameter in ComableChecker constructor |
| 41 | globalFilter.IsGloballyBlocked() call in ComableChecker.cs coordinator |
| 42 | BATHROOM exclusion IDs {350,351,354} in GlobalComableFilterTests.cs |
| 43 | IsGloballyBlocked(57) call in ComableChecker.Range5x.cs |
| 44 | TFlagComable constant usage in ComableChecker.cs |
| 45 | ILocationService parameter in ComableChecker constructor |
| 46 | IItemVariables parameter in ComableChecker constructor |
| 49 | TFLAG:100 master gate test reference in ComableCheckerTests.cs |
| 50 | `189 => false` constant case in switch expression |
| 51 | Sleep-state test reference in GlobalComableFilterTests.cs |
| 52 | IEngineVariables parameter in ComableChecker constructor |
| 54 | GlobalComableFilter.IsGloballyBlocked uses $"/{comId}/" for SAVESTR blacklist matching |
| 56 | StubComableUtilities.MasterPose returns 0 (conservative default: pose not satisfied) |
| 58 | No `new Stub*` instantiation inside ComableChecker*.cs (DI injection — no internal stub creation) |
| 59 | Grep `src/tools/dotnet/KojoComparer/YamlRunner.cs` for `GetExpLv|SetExpLv` — pattern must match (verifies VariableStoreAdapter updated per Task 3c) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|--------------------|----------|-----------|
| Class splitting strategy for 124 functions | A: Single class (God class), B: Split by range (0-199, 200-299, 500-599, 600-648), C: One class per COM_ABLE | B: Split by range using partial classes | Aligns with ERB file's range comments (0番台, 200番台, etc.); partial classes keep one physical file per range without breaking the single `ComableChecker` type name; avoids the ~3,700-line God class risk |
| GLOBAL_COMABLE ERB polarity translation | A: Return `true` = available, `false` = blocked; B: Mirror ERB polarity exactly (true=blocked) | B: Mirror ERB polarity — `IsGloballyBlocked` returns `true` when blocked | `IsGloballyBlocked` preserves ERB polarity (RETURNF 1 = blocked → true, RETURNF 0 = allowed → false), avoids double-negation bug at call site (`if (IsGloballyBlocked(id)) return false` is direct), and is noted in the Upstream Issues section |
| IComAvailabilityChecker method signature | A: `bool IsAvailable(int comId)`, B: `bool IsAvailable(ComId comId)` (typed), C: `bool IsAvailable(int comId, CharacterId charId)` | A: `bool IsAvailable(int comId)` | COM IDs are simple scalars (not value-object-worthy at this phase); avoids creating a new `ComId` type; character context is provided via injected IVariableStore/IEngineVariables (MASTER, PLAYER, ASSI are global engine state, not per-call parameters) |
| MASTER_POSE stub return value | A: Return 0 (conservative: pose not assumed), B: Return 1 (permissive) | A: Return 0 | Return 0 means "MASTER_POSE not satisfied" — in COMABLE.ERB contexts, `MASTER_POSE(6,1,1) == 0` gates erection-less penetration; stub returning 0 means the erection check fallback applies (conservative), which is safe until F811 provides concrete implementation |
| GetExpLv signature on IVariableStore | A: `int GetExpLv(int index)` (1D global array), B: `Result<int> GetExpLv(CharacterId, ExpIndex)` (2D character-scoped, incorrect) | A: `int GetExpLv(int index)` | EXPLV is a 1D global threshold array (VariableSize.csv: EXPLV,1000), not character-scoped; using CharacterId+ExpIndex would be a type mismatch; 1D index matches actual ERB access pattern EXPLV:N |
| GetAssiPlay location (IEngineVariables vs separate interface) | A: Add to IEngineVariables, B: Add to new IAssiVariables interface | A: Add to IEngineVariables | ASSIPLAY is an engine built-in scalar like ASSI, MASTER; ISP grouping already places all engine scalars on IEngineVariables; one new method does not warrant a new interface |
| GlobalComableFilter dependency injection | A: Inject as `IGlobalComableFilter` interface, B: Inject as concrete `GlobalComableFilter` class | B: Inject concrete class (deviates from F801) | F810 calls GlobalComableFilter directly; only one implementation exists. Note: this deviates from the F801 ICounterUtilities pattern (which uses an interface). Concrete injection is acceptable here because GlobalComableFilter has no polymorphism requirement, and tests can construct it with stub dependencies. |
| ComableChecker constructor parameter count (8 parameters) | A: Accept 8 DI parameters, B: Group into facade interfaces (IComableVariableAccess etc.), C: Reduce scope | A: Accept 8 DI parameters | 124 functions access ~15 variable domains (TFLAG, CFLAG, ABL, TALENT, PALAM, STAIN, EQUIP, TEQUIP, BASE, MAXBASE, EXP, ITEM, plus engine built-ins). IStringVariables (SAVESTR) and ICsvNameResolver (TRAINNAME) are only needed by GlobalComableFilter (which handles COMABLE2.ERB:9-13 and :76-124 respectively); GlobalComableFilter is pre-constructed and injected as a concrete class, so ComableChecker does not need these interfaces directly. Each remaining interface serves a distinct ISP boundary: IVariableStore (character arrays), ITEquipVariables (TEQUIP), IEngineVariables (engine scalars), ICommonFunctions (HAS_PENIS/HAS_VAGINA), ILocationService (BATHROOM/BEDROOM), IItemVariables (NOITEM), IComableUtilities (MASTER_POSE), GlobalComableFilter (centralized gating). Creating facade interfaces would add abstraction without reducing coupling (facade would still delegate to all 8). The parameter count is justified by the domain complexity. |
| Return type: Result<bool> vs bool | A: `Result<bool> IsAvailable(int comId)` (architecture spec pattern), B: `bool IsAvailable(int comId)` | B: `bool IsAvailable(int comId)` | COM_ABLE functions are pure reads with binary RETURN 0/1 — no failure modes exist (no I/O, no exceptions, no invalid state). F801 ActionValidator uses `bool IsActable()` (not Result<bool>), establishing the precedent. Result<bool> adds unwrap overhead without value since the only possible outcomes are true/false. Architecture spec's Result<bool> was designed without analyzing ERB function pattern. |
| Parameter types: typed vs raw | A: `IsAvailable(ComId com, CharacterId target, IGameState state)` (architecture spec), B: `IsAvailable(int comId)` (DI pattern) | B: `IsAvailable(int comId)` | Architecture spec's CharacterId parameter assumes per-call character context, but COMABLE.ERB functions access character data via global engine state (MASTER/TARGET/ASSI), not per-call parameters. Character context is provided by injected IVariableStore/IEngineVariables (DI pattern from F801). IGameState is a system control interface (Save/Load/Quit), unusable for variable access. Deviation tracked in Mandatory Handoffs for F813 architecture doc update. |
| Test structure for 124 functions | A: One test class per range file, B: Two test files (ComableCheckerTests.cs + GlobalComableFilterTests.cs), C: Single giant test class | B: Two test files | Mirrors the two public classes; each range group is tested within ComableCheckerTests.cs via nested classes or test groups; GlobalComableFilter gets its own file for SELECTCASE/video camera branch coverage |
| GetNoItem location (IItemVariables vs IEngineVariables) | A: Add to IEngineVariables (global scalar like ASSIPLAY), B: Add to IItemVariables (item-contextual) | B: Add to IItemVariables | NOITEM semantically restricts item availability (IF NOITEM → skip item-dependent commands); COMABLE.ERB callers that check NOITEM also access other IItemVariables methods (GetItem, GetItemSales). Placing NOITEM on IItemVariables keeps the item-restriction domain coherent. ASSIPLAY was placed on IEngineVariables because it is an engine-level play mode flag unrelated to items. |
| StubComableUtilities visibility | A: `internal sealed class` (Era.Core-only access), B: `public sealed class` (cross-assembly access) | A: `internal sealed class` | StubComableUtilities is a conservative placeholder until F811 provides concrete implementation. The composition root that constructs ComableChecker will be in Era.Core (engine integration); tests use InternalsVisibleTo (Era.Core.csproj) for access. Making it public would expose the stub as a public API contract, which contradicts its temporary nature. F801 uses test-local stubs (TestCounterUtilities in ActionValidatorTests.cs) for the same pattern. |
| GetNoItem setter exemption (Zero Debt Upfront) | A: Add SetNoItem paired with GetNoItem (full convention), B: Exempt GetNoItem from getter/setter pairing | B: Exempt GetNoItem — no SetNoItem added | GetNoItem is exempt from getter/setter pairing: NOITEM is an engine-managed read-only flag (codebase search found zero ERB writes to NOITEM — all 67 occurrences across COMABLE.ERB, COMABLE_400.ERB, TOILET_COUNTER_ACTABLE.ERB are read-only comparisons `NOITEM == 0`). Unlike EXPLV which is writable (OPTION_2.ERB:43-52) and therefore gets a paired SetExpLv, NOITEM is computed/set by the engine item system internally and never assigned by game scripts. Adding SetNoItem would create a misleading API implying writable state. |
| IComableUtilities.MasterPose vs ICounterUtilities.MasterPose | Temporary duplication accepted: ICounterUtilities.MasterPose(int,int)→CharacterId serves WC Counter (F804), IComableUtilities.MasterPose(int,int,int)→int serves COMABLE with different arity and return type. Unification tracked in Mandatory Handoffs for F811. |
| ComableChecker GetNoItem call source (IItemVariables vs IVariableStore) | ComableChecker injects both IVariableStore (which has GetNoItem from F804) and IItemVariables (which F809 adds GetNoItem to). ComableChecker range methods MUST call items.GetNoItem() (IItemVariables parameter) for ISP alignment, not variables.GetNoItem() (IVariableStore). Future: Consider removing GetNoItem from IVariableStore once IItemVariables version is established (track in Mandatory Handoffs for F813). |

### Interfaces / Data Structures

#### IComAvailabilityChecker

```csharp
namespace Era.Core.Counter.Comable;

/// <summary>
/// SSOT for COM command availability checking.
/// Migrates 124 @COM_ABLE{N} functions from COMABLE.ERB.
/// Feature 809 - COMABLE Core
/// </summary>
public interface IComAvailabilityChecker
{
    /// <summary>
    /// Determines if the given COM command is available.
    /// Migrates the TRYCALLFORM COM_ABLE{N} dispatch pattern.
    /// Returns true = command available, false = command blocked.
    /// </summary>
    bool IsAvailable(int comId);
}
```

#### ComableChecker (coordinator + partial class structure)

```csharp
// Era.Core/Counter/Comable/ComableChecker.cs (coordinator)
public sealed partial class ComableChecker(
    IVariableStore variables,
    ITEquipVariables tequip,
    IEngineVariables engine,
    ICommonFunctions common,
    ILocationService location,
    IItemVariables items,
    IComableUtilities comableUtils,
    GlobalComableFilter globalFilter) : IComAvailabilityChecker
{
    // TFLAG:100 constant (COMABLE management flag index)
    private const int TFlagComable = 100;

    public bool IsAvailable(int comId)
    {
        // All functions require TFLAG:100 (COMABLE management flag)
        if (variables.GetTFlag((FlagIndex)TFlagComable) == 0) return false;

        // Global filter pre-check (most functions use this)
        // COM_ABLE507 intentionally skips GLOBAL_COMABLE in ERB (COMABLE.ERB:2997-3004)
        // COM_ABLE512 handles its own global filter call with comId=57 (COMABLE.ERB:3136-3141)
        if (comId != 507 && comId != 512 && globalFilter.IsGloballyBlocked(comId)) return false;

        return comId switch
        {
            0 => IsAvailable0(),
            1 => IsAvailable1(),
            // ... (dispatches to range-specific partial methods)
            _ => false,
        };
    }
}

// Era.Core/Counter/Comable/ComableChecker.Range0x.cs (0-199)
public sealed partial class ComableChecker
{
    private bool IsAvailable0() { /* translates @COM_ABLE0 logic */ }
    private bool IsAvailable1() { /* translates @COM_ABLE1 logic */ }
    // ...
}

// Era.Core/Counter/Comable/ComableChecker.Range2x.cs (200-299)
// Era.Core/Counter/Comable/ComableChecker.Range5x.cs (500-599)
// Era.Core/Counter/Comable/ComableChecker.Range6x.cs (600-648)
```

#### GlobalComableFilter

```csharp
// Era.Core/Counter/Comable/GlobalComableFilter.cs
public sealed class GlobalComableFilter(
    IVariableStore variables,
    IEngineVariables engine,
    IStringVariables strings,
    ICsvNameResolver csvNames,
    ILocationService location)
{
    /// <summary>
    /// Centralized availability gate for all COM commands.
    /// Migrates @GLOBAL_COMABLE from COMABLE2.ERB.
    /// Returns true = command is BLOCKED (ERB polarity: RETURNF 1 = blocked).
    /// Returns false = command passes global filter (ERB polarity: RETURNF 0 = allowed).
    /// Callers invert: if IsGloballyBlocked(id) return false (unavailable).
    ///
    /// SAVESTR blacklist translation (COMABLE2.ERB lines 9-13):
    ///   LOCALS = "/"+TOSTR(ARG)+"/"
    ///   結果 = STRCOUNT(SAVESTR:0, LOCALS)
    ///   IF 結果 → RETURNF 1
    /// Translates to: strings.GetSaveStr(new SaveStrIndex(0)).Contains($"/{comId}/")
    /// If the blacklist string contains the comId, the command is blocked (return true).
    /// </summary>
    public bool IsGloballyBlocked(int comId) { /* translates COMABLE2.ERB */ }
}
```

#### GlobalComableFilter CFLAG Character Resolution

| ERB Variable | Character | C# Call | Lines |
|--------------|-----------|---------|-------|
| CFLAG:うふふ | TARGET (default) | `variables.GetCharacterFlag(CharacterId(engine.GetTarget()), うふふIndex)` | 19, 37, 50, 56, 62 |
| CFLAG:睡眠 | TARGET (default) | `variables.GetCharacterFlag(CharacterId(engine.GetTarget()), 睡眠Index)` | 68 |
| CFLAG:MASTER:現在位置 | MASTER (explicit) | `variables.GetCharacterFlag(CharacterId(engine.GetMaster()), 現在位置Index)` | 42 |
| TARGET:1 | TARGET (system) | `engine.GetTarget(1) > 0` (check if secondary TARGET (index 1) character exists) | 68 |
| TFLAG:COMABLE管理 | - (global) | `variables.GetTFlag((FlagIndex)COMABLE管理Index)` | 19, 25, 31, 37, 50, 56 |
| TFLAG:ビデオカメラ | - (global) | `variables.GetTFlag((FlagIndex)ビデオカメラIndex)` | 74, 126, 136 |

Note: The method name `IsGloballyBlocked` (not `IsGloballyAvailable`) preserves the ERB polarity (return true = blocked) to avoid a double-negation bug at the call site. AC#6 matcher `public sealed class GlobalComableFilter` still passes; the method name clarification does not conflict with the AC grep pattern.

#### IComableUtilities

```csharp
// Era.Core/Counter/Comable/IComableUtilities.cs
namespace Era.Core.Counter.Comable;

/// <summary>
/// Cross-phase utility abstractions for COMABLE-specific external functions.
/// Follows F801 ICounterUtilities pattern.
/// Feature 809 - COMABLE Core
/// </summary>
public interface IComableUtilities
{
    /// <summary>
    /// Migrates MASTER_POSE(pose, arg1, arg2) from SOURCE_POSE.ERB (F811 scope).
    /// Stub returns 0 (pose not satisfied) until F811 provides concrete implementation.
    /// </summary>
    int MasterPose(int pose, int arg1, int arg2);
}
```

#### Interface Extensions

```csharp
// Addition to IEngineVariables:
/// <summary>Get ASSIPLAY value (assistant play mode: 0=inactive, 1=active)</summary>
/// Feature 809 - COMABLE Core (ASSIPLAY used 57 times in COMABLE.ERB)
int GetAssiPlay();

// Addition to IItemVariables:
/// <summary>Get NOITEM value (global item restriction flag: 0=items allowed)</summary>
/// Feature 809 - COMABLE Core (NOITEM used 65 times in COMABLE.ERB)
int GetNoItem();

// Addition to IVariableStore (setter only — getter already exists from F804):
// EXPLV: experience level array (1D global threshold array, VariableSize.csv: EXPLV,1000)
// Feature 804 - already added: Result<int> GetExpLv(int level); (IVariableStore.cs line 115)
// Feature 809 - COMABLE Core adds paired setter:
void SetExpLv(int index, int value);

// Addition to ICsvNameResolver:
/// <summary>Get TRAINNAME value (COM display name) by COM ID</summary>
/// Feature 809 - COMABLE Core (TRAINNAME used in COMABLE2.ERB:76-124 video camera check)
string GetTrainName(int comId);
```

#### StubVariableStore extension (engine.Tests)

Add two virtual methods to `StubVariableStore` in `engine.Tests/Tests/TestStubs.cs` following existing pattern:
```csharp
public virtual Result<int> GetExpLv(int level) => Result<int>.Ok(0);
public virtual void SetExpLv(int index, int value) { }
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#3 matcher `public bool IsAvailable` is ambiguous — partial classes each contain private bool methods. The `public bool IsAvailable(int comId)` exists only in the coordinator file. Grep on `ComableChecker*.cs` will match the coordinator. No fix needed — pattern is sound. | AC Definition Table, AC#3 | No change needed; coordinator file ComableChecker.cs contains the public method |
| GlobalComableFilter method name: AC#6 checks `public.*class GlobalComableFilter` (class-level) but the public method name matters for F810 consumption. Using `IsGloballyBlocked` (true=blocked) vs `IsGloballyAvailable` (true=available) is a naming decision captured here. Recommend `IsGloballyBlocked` to preserve ERB polarity and avoid double-negation. | AC Definition Table, AC#6 | wbs-generator should note that `IsGloballyBlocked` is the method name; F810 dependency description should reference `IsGloballyBlocked` |
| BATHROOM and BEDROOM are called in COMABLE2.ERB:42 (`BATHROOM(CFLAG:MASTER:現在位置)`) and implicitly via CFLAG location checks. `ILocationService.IsBathroom(int)` already exists. No gap. | Technical Constraints | No change needed — ILocationService already covers this |
| COM_ABLE507 intentionally skips GLOBAL_COMABLE (COMABLE.ERB:2997-3004) — coordinator must exclude comId 507 from the global filter pre-check | Technical Design > ComableChecker code snippet, Approach, Execution Order step 5 | Applied: coordinator uses `comId != 507 && comId != 512 && globalFilter.IsGloballyBlocked(comId)` |
| COM_ABLE512 uses GLOBAL_COMABLE(57), not GLOBAL_COMABLE(512) | COMABLE.ERB:3136-3141 | Preserve ERB behavior: COM_ABLE512's range method must call `globalFilter.IsGloballyBlocked(57)` explicitly (not rely on the coordinator's centralized check which passes comId=512). The coordinator's `comId != 512` exception removes 512 from the centralized check; `IsAvailable512()` in Range5x.cs calls `globalFilter.IsGloballyBlocked(57)` explicitly before its individual conditions. |
| COM_ABLE189 is permanently disabled (bare `RETURN 0`, all conditions commented out at COMABLE.ERB:2722-2750) | Execution Order step 5 | Third anomaly alongside 507 and 512. The switch case `189 => false` is correct; no coordinator exclusion needed. Implementer should not attempt to translate the commented-out code. |
| Architecture spec defines `GetUnavailableReason(ComId, CharacterId)` as second method on IComAvailabilityChecker (phase-20-27-game-systems.md:130), but F809 implements only `IsAvailable(int)` | IComAvailabilityChecker interface, Mandatory Handoffs | ERB has no reason codes (RESULT always 0 on block paths). Method deferred to F813 Post-Phase Review. IComAvailabilityChecker interface has 1 method (not 2 as architecture spec planned). |

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
| :---: | :---: | --- | :---: | :---: |
| 1 | 1, 12, 30, 35 | Create Era.Core/Counter/Comable/ directory with IComAvailabilityChecker.cs and IComableUtilities.cs interfaces | | [x] |
| 2 | 13, 56 | Create StubComableUtilities.cs implementing IComableUtilities with MasterPose returning 0 | | [x] |
| 3 | 8[V], 9, 10[V], 11, 17, 18, 19, 20, 21 | Add additive interface extensions: GetAssiPlay to IEngineVariables (already exists from F812 — verify only [V]), GetNoItem to IItemVariables, SetExpLv to IVariableStore (GetExpLv already exists from F804 — verify only [V]), GetTrainName to ICsvNameResolver | | [x] |
| 3b | 14 | Update Era.Core concrete/null implementations (NullEngineVariables, EngineVariables, NullItemVariables, EngineItemVariables, NullCsvNameResolver) with new interface methods. Return safe defaults (0 for int, string.Empty for string, no-op for void). | | [x] |
| 3c | 16, 59 | Update all Era.Core.Tests mock/test implementations that implement IVariableStore (~26 classes), IEngineVariables (~9 classes), IItemVariables (~1 class), ICsvNameResolver (~3 classes) with stub implementations of new interface methods. Also update src/tools/dotnet/KojoComparer/YamlRunner.cs VariableStoreAdapter with GetExpLv/SetExpLv stubs. Return safe defaults. | | [x] |
| 4 | 6, 24, 35a, 35b, 35c, 35d, 35e, 54 | Create GlobalComableFilter.cs with public sealed class and IsGloballyBlocked(int comId) method migrating @GLOBAL_COMABLE from COMABLE2.ERB | | [x] |
| 5 | 2, 3, 4, 5, 23, 27, 28, 32, 33, 37, 40, 41, 43, 44, 45, 46, 50, 52 | Create ComableChecker.cs (coordinator) and partial range files ComableChecker.Range0x.cs (0-199, 86 IsAvailableN() methods — `189 => false` in coordinator switch), ComableChecker.Range2x.cs (200-299, 4 functions), ComableChecker.Range5x.cs (500-599, 16 functions), ComableChecker.Range6x.cs (600-648, 17 functions) migrating all 124 @COM_ABLE{N} functions from COMABLE.ERB | | [x] |
| 6 | 7, 25, 26a, 26b, 26c, 29, 31a, 31b, 31c, 55a, 55b, 55c, 34, 38, 39, 42, 49, 51 | Create equivalence test files Era.Core.Tests/Counter/ComableCheckerTests.cs and GlobalComableFilterTests.cs; update StubVariableStore in engine.Tests with GetExpLv/SetExpLv stubs | | [x] |
| 7 | 22, 53, 36a, 36b, 36c, 36d, 36e | Update engine-dev SKILL.md with new interfaces (IComAvailabilityChecker, IComableUtilities) and interface extensions (GetAssiPlay, GetNoItem, GetExpLv, GetTrainName) per SSOT update rules | | [x] |
| 8 | 14, 15, 16, 57, 58, 22, 53, 36a, 36b, 36c, 36d, 36e | Run dotnet build Era.Core/ and dotnet test Era.Core.Tests/; verify no TODO/FIXME/HACK markers; verify engine-dev SKILL.md SSOT updates; confirm all pass | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
| :---: | --------- | --------- | --------- |
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-809.md (Tasks 1-3: interfaces + stubs + interface extensions) | IComAvailabilityChecker.cs, IComableUtilities.cs, StubComableUtilities.cs, 4 interface method additions |
| 2 | implementer | sonnet | feature-809.md (Task 4: GlobalComableFilter) + COMABLE2.ERB | GlobalComableFilter.cs |
| 3 | implementer | sonnet | feature-809.md (Task 5: ComableChecker ranges) + COMABLE.ERB | ComableChecker.cs, ComableChecker.Range0x.cs, ComableChecker.Range2x.cs, ComableChecker.Range5x.cs, ComableChecker.Range6x.cs |
| 4 | tester | sonnet | feature-809.md (Task 6: tests + StubVariableStore) | ComableCheckerTests.cs, GlobalComableFilterTests.cs, StubVariableStore stub additions |
| 5 | implementer | sonnet | feature-809.md (Task 7: SSOT update) | engine-dev SKILL.md update |
| 6 | tester | sonnet | feature-809.md (Task 8: build+test gate + zero-debt verification) | Exit code 0 for dotnet build + dotnet test; AC#15 no TODO/FIXME/HACK in Comable subsystem |

### Pre-conditions

- F783 [DONE] (Phase 21 Planning parent)
- F801 [DONE] (DI injection pattern reference: ActionValidator.cs:11-16)
- F815 [DONE] (StubVariableStore base class for testing)
- `Era.Core/Interfaces/IEngineVariables.cs` currently has 25 methods (GetAssiPlay already present from F812); stays at 25 after Task 3
- `Era.Core/Interfaces/IItemVariables.cs` currently has 6 methods → after Task 3: 7
- `Era.Core/Interfaces/IVariableStore.cs` currently has 43 total method declarations (42 match AC#19 grep pattern `^\s+(int|void|string|bool|Result<int>)\s+\w+\(`; `Result<string> GetCharacterString` excluded as its return type does not match the pattern). After Task 3 (SetExpLv only): 44 total, 43 grep-pattern-matched (AC#19 target). GetExpLv already exists from F804; only SetExpLv is new.
- `Era.Core/Interfaces/ICsvNameResolver.cs` currently has 5 methods → after Task 3: 6

### Execution Order

1. **Task 1** (interfaces): Create `Era.Core/Counter/Comable/` directory. Create `IComAvailabilityChecker.cs` with `bool IsAvailable(int comId)`. Create `IComableUtilities.cs` with `int MasterPose(int pose, int arg1, int arg2)`. Both files must include namespace declaration `namespace Era.Core.Counter.Comable;` and XML doc comments.
2. **Task 2** (stub): Create `StubComableUtilities.cs` — `internal sealed class StubComableUtilities : IComableUtilities` — `MasterPose` returns 0 (conservative default: pose not satisfied until F811 provides concrete implementation).
3. **Task 3** (interface extensions, additive-only): Verify `int GetAssiPlay();` exists on `IEngineVariables.cs` — already added by F812 (IEngineVariables.cs:98), no new addition needed. Add `int GetNoItem();` to `IItemVariables.cs`. Add `void SetExpLv(int index, int value);` to `IVariableStore.cs` — note: `GetExpLv(int level)` was already added by F804 (IVariableStore.cs line 115), so only the SetExpLv setter is new. Add `string GetTrainName(int comId);` to `ICsvNameResolver.cs`. All additions are additive only — do NOT modify existing methods.
3b. **Task 3b** (Era.Core concrete implementations): Update NullEngineVariables, EngineVariables, NullItemVariables, EngineItemVariables, and NullCsvNameResolver in Era.Core with the new interface methods. Use safe defaults: `return 0;` for int methods, `return string.Empty;` for string methods, no-op body for void methods.
3c. **Task 3c** (Era.Core.Tests mock implementations + tools): Update all test/mock classes in Era.Core.Tests that implement IVariableStore (~26 classes), IEngineVariables (~9 classes), IItemVariables (~1 class), and ICsvNameResolver (~3 classes) with stub implementations of the new interface methods. Also update `src/tools/dotnet/KojoComparer/YamlRunner.cs` VariableStoreAdapter with `GetExpLv(int index) => 0` and `SetExpLv(int index, int value) { }` stubs. Return safe defaults.
4. **Task 4** (GlobalComableFilter): Create `GlobalComableFilter.cs`. Public sealed class, constructor-injected with `IVariableStore variables, IEngineVariables engine, IStringVariables strings, ICsvNameResolver csvNames, ILocationService location`. Method: `public bool IsGloballyBlocked(int comId)` — migrates COMABLE2.ERB @GLOBAL_COMABLE (149 lines). ERB polarity: RETURNF 1 = blocked → C# return `true`; RETURNF 0 = allowed → C# return `false`. The SELECTCASE video camera block (lines 76-124) uses `csvNames.GetTrainName(comId)` for string comparison.
5. **Task 5** (ComableChecker ranges): Create `ComableChecker.cs` (coordinator, `public sealed partial class`) with primary constructor injecting all 8 dependencies (IVariableStore, ITEquipVariables, IEngineVariables, ICommonFunctions, ILocationService, IItemVariables, IComableUtilities, GlobalComableFilter — IStringVariables and ICsvNameResolver are not injected into ComableChecker; they belong to GlobalComableFilter only). `IsAvailable(int comId)` applies global pre-checks ONCE (TFLAG:100 check via `variables.GetTFlag((FlagIndex)TFlagComable) == 0` and `comId != 507 && comId != 512 && globalFilter.IsGloballyBlocked(comId)`) before dispatching to range-specific private partial methods via switch expression. Two exceptions bypass the centralized global filter: COM_ABLE507 intentionally skips GLOBAL_COMABLE entirely (COMABLE.ERB:2997-3004); COM_ABLE512 calls GLOBAL_COMABLE(57) instead of GLOBAL_COMABLE(512) (COMABLE.ERB:3136-3141) and must call `globalFilter.IsGloballyBlocked(57)` explicitly inside `IsAvailable512()` in Range5x.cs. Additional anomaly: COM_ABLE189 has all logic commented out in ERB (COMABLE.ERB:2722-2750); the function body is only `RETURN 0`. Implement as `189 => false` (unconditional constant false) — do not attempt to translate the commented-out conditions. Create `ComableChecker.Range0x.cs` (COM IDs 0-199, 86 IsAvailableN() dispatch methods — COM_ABLE189 uses `189 => false` directly in coordinator switch, not as a Range0x method; source: COMABLE.ERB has 87 @COM_ABLE{N} functions in IDs 0-199 = 86 dispatching + 1 permanently-disabled COM_ABLE189), `ComableChecker.Range2x.cs` (200-299, 4 functions), `ComableChecker.Range5x.cs` (500-599, 16 functions), `ComableChecker.Range6x.cs` (600-648, 17 functions). Each `@COM_ABLE{N}` becomes a private `bool IsAvailableN()` method containing only the per-function individual conditions as early-return `false` guards (the global pre-checks are NOT repeated in range methods, except IsAvailable512 which adds its own explicit `globalFilter.IsGloballyBlocked(57)` call).
6. **Task 6** (tests + StubVariableStore):
   - **6a**: Create test files: `Era.Core.Tests/Counter/ComableCheckerTests.cs` with equivalence tests per range group (minimum 5 Assert for Range0x, minimum 3 for other ranges), and `Era.Core.Tests/Counter/GlobalComableFilterTests.cs` with branch coverage for all 5 branch groups of GLOBAL_COMABLE: (1) SAVESTR blacklist filter (lines 9-13, both blocked and pass paths), (2) 7 SELECTCASE range branches (CASE 000-199: TFLAG:COMABLE管理/CFLAG:うふふ gate, CASE 200-299: TFLAG:COMABLE管理 gate, CASE 300-309: 日常 mode only, CASE 310-399: non-うふふ + BATHROOM sub-branch with exclusions {350,351,354}, CASE 400-499: non-うふふ, CASE 500-599: same as 000-199, CASE 600-699: 自慰 only), (3) sleep-state check (TARGET:1>0 && CFLAG:睡眠==1, comId outside 400-499 → blocked), (4) 3 video camera branches (branch A: compound OR 44-command allowlist, branch B: 2-command allowlist — most restrictive, branch C: 4-command allowlist), (5) CASEELSE pass-through (comId ≥ 700 → not blocked by SELECTCASE, proceeds to sleep/video checks).
   - **6b**: Update `StubVariableStore` in `engine.Tests/Tests/TestStubs.cs`: add `public virtual Result<int> GetExpLv(int level) => Result<int>.Ok(0);` and `public virtual void SetExpLv(int index, int value) { }`. Note: StubEngineVariables does NOT need GetAssiPlay() override — IEngineVariables provides default implementation `int GetAssiPlay() => 0;` (added by F812).
7. **Task 7** (SSOT update): Update `.claude/skills/engine-dev/SKILL.md` to document new interfaces (IComAvailabilityChecker, IComableUtilities) and interface extensions (GetAssiPlay, GetNoItem, GetExpLv/SetExpLv, GetTrainName) per SSOT update rules.
8. **Task 8** (build + test gate + zero-debt verification): Run `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'` → must exit 0. Run `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/'` → must exit 0. Verify no TODO/FIXME/HACK markers exist in `Era.Core/Counter/Comable/` (AC#15). STOP if any step fails.

### Build Verification Steps

```bash
# Build gate (TreatWarningsAsErrors policy)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'

# Test gate (includes StubVariableStore extension verification)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/'

# engine.Tests gate (StubVariableStore extension verification)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test engine.Tests/'
```

### Success Criteria

All 53 ACs pass (AC#26→26a/26b/26c split, AC#31→31a/31b/31c, AC#35→35a-35e, AC#36→36a-36e, AC#34-52 added, AC#47/AC#48 removed — IStringVariables/ICsvNameResolver not injected into ComableChecker, AC#55→55a/55b/55c split, AC#36 removed — subsumed by AC#57, AC#56/57/58/59 added):
- AC#1-5: Comable subsystem structure and DI pattern
- AC#6, AC#24: GlobalComableFilter public API (class existence + IsGloballyBlocked method signature)
- AC#7, AC#25, AC#26a, AC#26b, AC#26c: Equivalence test files exist (ComableCheckerTests.cs + GlobalComableFilterTests.cs); per-range test coverage verified (Range2x, Range5x, Range6x independently)
- AC#8-11: 4 interface extensions present (getters)
- AC#12-13: IComableUtilities + stub
- AC#14,16: Build and test pass (Task 8)
- AC#15: Zero technical debt (no TODO/FIXME/HACK) (Task 8)
- AC#17-20: Backward compatibility counts verified
- AC#21: SetExpLv setter on IVariableStore
- AC#22, AC#53: engine-dev SKILL.md SSOT update (IComAvailabilityChecker + IComableUtilities both documented) (Task 7)
- AC#23: ComableChecker switch covers highest COM ID (648)
- AC#27: GlobalComableFilter injected via ComableChecker constructor (concrete class injection per Key Decision)
- AC#28: All 4 range partial files exist
- AC#29: engine.Tests passes after StubVariableStore extension
- AC#31a/31b/31c: COM_ABLE507/512/189 anomaly cases tested independently
- AC#30: IComAvailabilityChecker single-method contract
- AC#55a/55b/55c: Range0x test coverage split by sub-range (low 0-66, mid 67-133, high 134-199)
- AC#32: Total 123 switch dispatch entries (COM_ABLE189 uses `189 => false` directly)
- AC#35a-35e: GlobalComableFilter DI constructor (all 5 interfaces: IVariableStore, IEngineVariables, IStringVariables, ICsvNameResolver, ILocationService)
- AC#36a/36b/36c/36d/36e: All 5 SSOT extension methods independently verified in SKILL.md (4 getters + SetExpLv setter)
- AC#33: IComableUtilities injected in ComableChecker constructor
- AC#34: GlobalComableFilter constructible in test context (concrete class testability)
- AC#35: GetUnavailableReason not prematurely added (single-method contract guard)
- AC#57: No setter calls in Comable subsystem (comprehensive query-only guard — Set* pattern)
- AC#37: ICommonFunctions injected as ComableChecker constructor parameter (HAS_PENIS/HAS_VAGINA domain utility)
- AC#38: GlobalComableFilter tests reference video camera branch logic (most complex branch group coverage)
- AC#39: GlobalComableFilter tests reference SELECTCASE range routing logic (TFLAG:COMABLE管理), ensuring the core routing branch group is tested
- AC#40: ITEquipVariables injected as ComableChecker constructor parameter (TEQUIP/EQUIP domain interface)
- AC#41: ComableChecker coordinator invokes globalFilter.IsGloballyBlocked (centralized gating actually called)
- AC#42: GlobalComableFilter tests reference BATHROOM exclusion set IDs (350/351/354), ensuring sub-branch exception is tested
- AC#43: ComableChecker.Range5x.cs contains `IsGloballyBlocked(57)` — verifies COM_ABLE512's ERB anomaly (GLOBAL_COMABLE(57) redirection) is preserved in implementation
- AC#44: ComableChecker.cs contains `TFlagComable` — verifies the TFLAG:100 master gate pre-check is present in the coordinator
- AC#45: ILocationService injected as ComableChecker constructor parameter (location domain interface)
- AC#46: IItemVariables injected as ComableChecker constructor parameter (item domain interface)
- AC#49: ComableCheckerTests references TFLAG:100 master gate (all-commands-disabled gate tested)
- AC#50: ComableChecker switch contains `189 => false` — verifies COM_ABLE189 permanent-disable constant-false case is in the switch expression (not delegated to a Range0x method)
- AC#51: GlobalComableFilter tests reference sleep-state branch logic (COMABLE2.ERB line 68 sleep-state exclusion branch tested)
- AC#52: IEngineVariables injected as ComableChecker constructor parameter (engine built-in variable interface for GetAssiPlay/GetTarget/GetMaster)
- AC#54: GlobalComableFilter SAVESTR blacklist uses $"/{comId}/" slash-delimited format (preserves ERB COMABLE2.ERB:9-10 delimiter pattern to prevent substring false positives)
- AC#59: KojoComparer VariableStoreAdapter has GetExpLv/SetExpLv stubs (src/tools/dotnet/KojoComparer/YamlRunner.cs updated per Task 3c; prevents CS0535 build failure after SetExpLv added to IVariableStore)

### Error Handling

| Situation | Action |
|-----------|--------|
| dotnet build fails (TreatWarningsAsErrors) | STOP — fix all warnings before proceeding |
| Interface extension breaks existing StubVariableStore | Add virtual stub methods to StubVariableStore; do NOT remove existing implementations |
| 124 functions not all found in COMABLE.ERB | Grep `@COM_ABLE` in COMABLE.ERB to enumerate exact IDs; do not assume count |
| MASTER_POSE concrete implementation needed | STOP — defer to F811. Stub returns 0; do not implement concretely |

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| MASTER_POSE concrete implementation (MasterPose method in IComableUtilities) | SOURCE_POSE.ERB:334 defines MASTER_POSE in F811 scope; concrete implementation would create circular dependency | Feature | F811 | (F811 already exists as [DRAFT]) |
| GlobalComableFilter public API consumption (IsGloballyBlocked method) | F810 calls GLOBAL_COMABLE 44 times (COMABLE_300.ERB:11, COMABLE_400.ERB:17); F810 must inject GlobalComableFilter directly | Feature | F810 | (F810 already exists as [DRAFT]) |
| EXPLV duplication unification (PainStateChecker + AbilityGrowthProcessor) | PainStateChecker.cs and AbilityGrowthProcessor.cs both contain private EXPLV arrays with different values; once F809 adds GetExpLv to IVariableStore, these should use the shared interface method | Feature | F813 | (F813 already exists as [DRAFT]) |
| GetUnavailableReason method on IComAvailabilityChecker | Architecture spec (phase-20-27-game-systems.md:130) defines `GetUnavailableReason(ComId, CharacterId)` as second method, but ERB has no reason codes (RESULT always 0 on block paths); deferred until UI/UX requires user-facing block explanations | Feature | F813 | (F813 already exists as [DRAFT]) |
| Architecture doc Phase 21 IComAvailabilityChecker update | Actual interface deviates from spec: 1 method vs 2, bool vs Result<bool>, int comId vs (ComId, CharacterId, IGameState) | Feature | F813 | (F813 already exists as [DRAFT]) |
| SOURCE_CALLCOM.ERB COM_ABLE dispatch → IComAvailabilityChecker migration | SOURCE_CALLCOM.ERB:70 calls `CALLFORM COM_ABLE{500+TFLAG:50}` directly at ERB level, bypassing IComAvailabilityChecker. F811 must migrate this call to use IComAvailabilityChecker.IsAvailable(500+N) to enforce SSOT. | Feature | F811 | (F811 already exists as [BLOCKED]) |
| OPTION_2.ERB EXPLV setter migration to IVariableStore.SetExpLv | OPTION_2.ERB:43-52 is the only identified ERB write caller for EXPLV; SetExpLv added to IVariableStore by F809 (Zero Debt Upfront convention) but the ERB caller migration is not in F809 scope | Feature | F813 | (F813 already exists as [DRAFT]) |
| ComId.cs XML doc range update (0-299 → 0-648) | F809 migrates COM IDs up to 648 but ComId.cs doc says 0-299; updating the type file doc is not F809 scope (F809 creates Comable subsystem, not types) | Feature | F813 | (F813 already exists as [DRAFT]) |
| MASTER_POSE abstraction unification | ICounterUtilities.MasterPose(int,int)→CharacterId vs IComableUtilities.MasterPose(int,int,int)→int serve different callers but abstract same ERB function @MASTER_POSE. Unify when F811 provides concrete implementation. | Feature | F811 | F811 already [BLOCKED] |
| GetNoItem IVariableStore→IItemVariables migration | GetNoItem exists on both IVariableStore (F804) and IItemVariables (F809). After F809, consumers should prefer IItemVariables.GetNoItem(). Remove IVariableStore.GetNoItem() in future cleanup. | Feature | F813 | (F813 already exists as [DRAFT]) |
| tools/KojoComparer VariableStoreAdapter | KojoComparer VariableStoreAdapter implements IVariableStore directly; must add GetExpLv/SetExpLv stubs | Existing file | src/tools/dotnet/KojoComparer/YamlRunner.cs | Task 3c |

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
| 2026-02-24 11:41 | START | implementer | Task 4 | - |
| 2026-02-24 11:42 | END | implementer | Task 4 | SUCCESS |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: Reference section | Removed duplicate '## Reference (from previous session)' with stale Tasks table
- [fix] Phase2-Review iter1: Technical Design > GlobalComableFilter method name | Aligned IsGloballyAvailable → IsGloballyBlocked across Approach, AC Coverage, Key Decisions (ERB polarity preservation)
- [fix] Phase2-Review iter1: AC Definition Table + AC Details + Goal Coverage | Added AC#23 for 124-function migration completeness (Grep case 648 in switch)
- [fix] Phase2-Review iter1: Technical Design > Approach > Translation Pattern | Fixed compile bug: `!_variables.GetTFlag(...) != 0` → `_variables.GetTFlag(...) == 0`
- [fix] Phase2-Review iter2: Technical Design > ComableBaseCheck code snippet | Fixed IsGloballyAvailable → IsGloballyBlocked and removed parameterless helper (comId out of scope)
- [fix] Phase2-Review iter2: AC#7 | Strengthened from Glob exists → Grep matches [Fact]/[Theory] to verify actual test methods exist
- [fix] Phase2-Review iter3: AC Definition Table + AC Details + Goal Coverage | Added AC#24 (IsGloballyBlocked method signature) and AC#25 (GlobalComableFilterTests.cs test methods)
- [fix] Phase2-Review iter3: Tasks table | Realigned AC#15 to Task 7, removed AC#16 from Task 6, added AC#25 to Task 6
- [fix] Phase2-Review iter4: AC Definition Table + AC Details + Goal Coverage | Added AC#26 (multi-range test coverage) and AC#26 (IComableUtilities in SKILL.md)
- [fix] Phase2-Review iter5: AC#4 | Strengthened matcher from `ComableChecker\(` → `ComableChecker\(IVariableStore` to verify DI constructor
- [fix] Phase3-Maintainability iter6: Key Decisions > GlobalComableFilter injection | Corrected factual error (ICounterUtilities IS an interface); updated rationale to acknowledge F801 deviation
- [fix] Phase3-Maintainability iter6: Task 5 + Approach + Execution Order | Updated function count estimates to verified values (87, 4, 16, 17 = 124 total)
- [fix] Phase3-Maintainability iter6: Execution Order step 6 | Added explicit sub-step sequencing (6a tests, 6b StubVariableStore); SKILL.md moved to separate Task 7
- [fix] Phase3-Maintainability iter6: AC#13 | Strengthened matcher to `class\s+\w+\s*:\s*IComableUtilities` (verifies interface implementation)
- [fix] Phase3-Maintainability iter6: Approach paragraph | Standardized file naming to ComableChecker.Range{N}x.cs throughout
- [fix] Phase2-Review iter7: Technical Design > ComableChecker code snippet | Centralized TFLAG+GlobalComableFilter pre-checks in coordinator IsAvailable() before switch dispatch; resolved comId scope contradiction
- [fix] Phase2-Review iter7: GlobalComableFilter + AC#25 + Execution Order | Added SAVESTR blacklist translation note and strengthened test requirement to cover both SELECTCASE and SAVESTR branches
- [fix] Phase2-Review iter8: ComableChecker coordinator | Added comId!=507 && comId!=512 exceptions for COM_ABLE507 (skips GLOBAL_COMABLE) and COM_ABLE512 (uses GLOBAL_COMABLE(57))
- [fix] Phase2-Review iter8: Upstream Issues | Documented COM_ABLE507/512 exceptions with ERB line references
- [fix] Phase2-Review iter8: AC#25 detail + AC Coverage + Execution Order 6a | Enumerated all 3 video camera branches with line references
- [fix] Phase2-Review iter9: AC#25 + AC Coverage + Execution Order 6a | Fixed Branch A compound OR: added `|| (TFLAG:ビデオカメラ==2 && ASSIPLAY)` clause (COMABLE2.ERB:74)
- [fix] Phase4-ACValidation iter10: AC#17-20 | Added grep pattern to Method column for count_equals matchers (was missing, only in Details)
- [fix] Phase2-Review iter1: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Philosophy Derivation | Added AC#27 for GlobalComableFilter constructor injection verification
- [fix] Phase2-Review iter1: Tasks table + Implementation Contract + Execution Order | Split Task 6 into Task 6 (tests+StubVariableStore) and Task 7 (SSOT update); renumbered old Task 7 to Task 8
- [fix] Phase2-Review iter2: Goal Coverage row 11 | Removed AC#16 from 'Equivalence tested' (AC#16 is regression gate, not equivalence verification)
- [fix] Phase2-Uncertain iter3: AC#26 | Strengthened matcher from `IsAvailable.*` to `IsAvailable\(` to prevent false matches from comments/strings
- [fix] Phase2-Review iter4: AC#25 Detail + AC Coverage + Execution Order 6a | Extended branch coverage from 2 groups (SAVESTR+video) to all 4 groups (SAVESTR, 7 SELECTCASE ranges, sleep-state, video)
- [fix] Phase3-Maintainability iter5: Mandatory Handoffs | Added EXPLV duplication unification handoff (PainStateChecker + AbilityGrowthProcessor → F813)
- [fix] Phase3-Maintainability iter5: Key Decisions | Added 10-parameter constructor justification (domain complexity: 15 variable domains → 10 ISP-aligned interfaces)
- [fix] Phase2-Review iter6: AC#12 | Strengthened matcher from `MasterPose\(` to `MasterPose\(int.*int.*int` to verify 3-parameter signature
- [fix] Phase2-Uncertain iter6: AC Definition Table + AC Details + Goal Coverage + Task 5 | Added AC#28 for range file existence (Glob count_equals 4)
- [fix] Phase2-Review iter6: Technical Design > GlobalComableFilter | Added CFLAG Character Resolution table (うふふ/睡眠→TARGET, MASTER:現在位置→MASTER)
- [fix] Phase2-Review iter7: Execution Order step 5 + Upstream Issues | Added COM_ABLE189 anomaly (permanently disabled, bare RETURN 0)
- [fix] Phase2-Review iter7: AC#25 Detail + AC Coverage + Execution Order 6a | Added CASEELSE pass-through as 5th branch group (comId ≥ 700)
- [fix] Phase3-Maintainability iter8: Mandatory Handoffs + Upstream Issues | Added GetUnavailableReason architecture deviation tracking (deferred to F813)
- [fix] Phase2-Review iter9: AC Definition Table + AC Details + Task 8 + Build Verification + Success Criteria | Added AC#29 for engine.Tests gate (StubVariableStore extension verification)
- [fix] Phase2-Review iter10: AC Definition Table + AC Details + AC Coverage + Task 6 + Goal Coverage + Success Criteria | Added AC#31 for COM_ABLE507/512 exception test coverage
- [resolved-applied] Phase2-Pending iter1: EXPLV is a 1D global threshold array (VariableSize.csv: EXPLV,1000), not 2D character-scoped. GetExpLv(CharacterId, ExpIndex) signature is wrong → should be int GetExpLv(int index). SetExpLv signature is also wrong → should be void SetExpLv(int index, int value) (EXPLV IS writable per OPTION_2.ERB:43-52, NOT read-only). AC#19 count remains 42 (both getter and setter corrected to 1D). Affects AC#10, AC#21, AC#19, Technical Design interface extensions, Key Decisions, Execution Order step 3, Implementation Contract, StubVariableStore snippet.
- [fix] Phase2-Review iter1: ## Summary section | Removed non-template ## Summary section between ## Type and ## Background
- [fix] Phase2-Review iter1: Tasks table Task 6 | Added AC#29 to Task 6 AC# column (StubVariableStore extension ownership)
- [fix] Phase2-Review iter2: AC Definition Table + AC Details + AC Coverage + Philosophy Derivation + Goal Coverage + Success Criteria + Task 1 | Added AC#30 for IComAvailabilityChecker single-method contract (GetUnavailableReason deferred to F813)
- [fix] Phase2-Review iter2: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Success Criteria + Task 6 | Added AC#31 for Range0x test coverage (3+ COM IDs in 0-199)
- [fix] Phase2-Review iter2: AC#31 | Extended matcher to include COM_ABLE189 anomaly (all 3 documented anomalies now covered: 507, 512, 189)
- [fix] Phase2-Review iter3: AC#31 | Changed matcher from `matches` to `count_gte | 3` to enforce ALL 3 anomaly IDs present (alternation with `matches` only requires 1 hit)
- [fix] Phase2-Review iter3: Philosophy + Philosophy Derivation | Changed 'pure-function' to 'query-only (read-only access, no side effects)' — ComableChecker reads mutable injected state, so 'pure-function' was inaccurate
- [fix] Phase2-Review iter4: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Success Criteria + Task 5 | Added AC#32 for total 124 COM_ABLE switch dispatch count verification
- [fix] Phase2-Review iter5: AC#32 | Changed expected from 124 to 123 (COM_ABLE189 uses `189 => false` directly, not IsAvailable189())
- [fix] Phase2-Review iter5: Success Criteria | Changed 'All 33 ACs' to 'All 34 ACs'
- [fix] Phase2-Review iter5: Philosophy Derivation | Added AC#12, AC#13 to 'Resolve all 5 interface gaps' row (MASTER_POSE is 5th gap)
- [fix] Phase2-Review iter6: AC Coverage row 34 | Fixed '124 matches' → '123 matches' (COM_ABLE189 uses `189 => false`)
- [fix] Phase2-Review iter6: Tasks table Task 8 | Removed AC#29 dual ownership (AC#29 belongs to Task 6 only)
- [fix] Phase2-Review iter7: Task 5 description | Clarified Range0x as '86 IsAvailableN() + 189 => false' (prevent implementer creating 87 IsAvailableN() methods breaking AC#32)
- [fix] Phase2-Review iter7: Philosophy Derivation | Separated AC#27 from F801 compliance row into own 'GlobalComableFilter concrete injection' deviation row
- [fix] Phase2-Review iter8: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Philosophy Derivation + Task 4 + Success Criteria | Added AC#35 for GlobalComableFilter DI constructor verification
- [fix] Phase2-Review iter8: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 7 + Success Criteria | Added AC#36 for SSOT extension methods in SKILL.md
- [fix] Phase2-Review iter9: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 5 + Success Criteria | Added AC#33 for IComableUtilities constructor injection verification
- [fix] Phase2-Review iter10: AC#36 | Added GetExpLv to matcher (was missing 1 of 4 extension methods)
- [fix] Phase3-Maintainability iter10: Mandatory Handoffs | Added architecture doc Phase 21 IComAvailabilityChecker update handoff to F813
- [fix] Phase2-Review iter1: Success Criteria | Fixed '124 switch dispatch' → '123' to match AC#32 expected value
- [fix] Phase2-Review iter1: AC#31 | Split AC#31 (OR-alternation count_gte=3) into AC#31a/31b/31c (independent matches per anomaly ID: 507, 512, 189)
- [fix] Phase2-Review iter1: AC#32 rationale | Added explicit confirmation that COM_ABLE507/512 use standard IsAvailableN() dispatch
- [fix] Phase2-Uncertain iter1: AC Definition Table + AC Details + AC Coverage + Philosophy Derivation + Goal Coverage + Task 6 + Success Criteria | Added AC#34 for GlobalComableFilter test constructibility verification
- [fix] Phase2-Review iter2: Execution Order step 5 | Clarified Range0x = '87 functions = 86 IsAvailableN() dispatch + 1 direct 189 => false' to prevent implementer ambiguity
- [fix] Phase2-Review iter2: Mandatory Handoffs | Added SOURCE_CALLCOM.ERB COM_ABLE dispatch → IComAvailabilityChecker migration handoff to F811
- [fix] Phase2-Review iter2: [pending] EXPLV item | Corrected SetExpLv from 'should be removed (read-only)' to 'should be corrected to 1D signature (EXPLV IS writable per OPTION_2.ERB)'
- [fix] Phase2-Review iter3: AC#36 | Split AC#36 (OR-pattern) into AC#36a/36b/36c/36d (independent verification per extension method)
- [resolved-applied] Phase2-Uncertain iter3: Philosophy claims 'SSOT serving USERCOM.ERB and COMF*.ERB' but no AC/wiring verifies ERB callers use IComAvailabilityChecker. → User chose (a): revised Philosophy to 'C# infrastructure for future ERB dispatch wiring (F810/F811 scope)'
- [fix] Phase2-Review iter4: AC#36e | Added AC#36e for SetExpLv SKILL.md verification (paired setter was missing independent AC)
- [fix] Phase2-Review iter4: Goal section | Added IComAvailabilityChecker 1-method-only deviation statement (GetUnavailableReason deferred to F813)
- [fix] Phase2-Review iter5: AC#25 Detail + Execution Order step 6a | Fixed Branch A video camera allowlist count from 42 to 44 (verified against COMABLE2.ERB:77-120)
- [fix] Phase2-Review iter6: AC#25 | Strengthened matcher from matches → count_gte 5 (one per branch group)
- [fix] Phase2-Review iter6: Philosophy Derivation | Moved AC#21 from 'interface gaps' row to new 'IVariableStore getter/setter convention' row (SetExpLv is not an interface gap but a convention-derived addition)
- [fix] Phase2-Review iter7: AC Definition Table + AC Details + Task 1 + Philosophy Derivation + AC Coverage + Success Criteria | Added AC#35 not_matches guard for GetUnavailableReason on IComAvailabilityChecker (strengthens AC#30 single-method contract)
- [fix] Phase2-Review iter8: AC#35 | Split AC#35 into AC#35a/35b/35c (IVariableStore + IStringVariables + ICsvNameResolver) — verifies all critical constructor dependencies for GlobalComableFilter
- [fix] Phase2-Review iter9: AC#35 | Added AC#35d for IEngineVariables in GlobalComableFilter (required for GetTarget/GetMaster/GetAssiPlay)
- [fix] Phase2-Review iter9: AC#26 | Strengthened matcher from matches → count_gte 3 (ensures multiple non-0x range coverage)
- [fix] Phase2-Review iter10: AC#35 | Added AC#35e for ILocationService in GlobalComableFilter (required for BATHROOM sub-branch)
- [fix] Phase2-Review iter1: Review Notes line 1011 | Corrected stale '6c SKILL.md' reference to 'SKILL.md moved to separate Task 7'
- [fix] Phase2-Review iter1: AC#26 | Split AC#26 into AC#26a/26b/26c (per-range coverage: Range2x, Range5x, Range6x) — fixes vulnerability where Range5x alone could satisfy count_gte=3
- [fix] Phase2-Uncertain iter1: AC Definition Table + AC Details + AC Coverage + Philosophy Derivation + Goal Coverage + Task 8 + Success Criteria | Added AC#36 not_matches guard for SetExpLv within Comable subsystem (query-only philosophy enforcement)
- [fix] Phase2-Review iter1: Task 5 + Execution Order step 5 | Clarified 189 => false placement in coordinator switch (not Range0x.cs method)
- [fix] Phase2-Review iter2: AC#31 rationale | Clarified '87 functions' → '87 COM IDs with 86 IsAvailableN() methods' (COM_ABLE189 in coordinator switch, not Range0x.cs)
- [fix] Phase2-Review iter3: Mandatory Handoffs | Added OPTION_2.ERB EXPLV setter migration handoff to F813 (SetExpLv added to IVariableStore but ERB caller migration not in F809 scope)
- [fix] Phase2-Review iter4: AC Definition Table + AC Details + AC Coverage + Philosophy Derivation + Goal Coverage + Task 5 + Success Criteria | Added AC#37 for ICommonFunctions in ComableChecker constructor (HAS_PENIS/HAS_VAGINA DI gap)
- [fix] Phase2-Review iter4: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 6 + Success Criteria | Added AC#38 for video camera branch logic test coverage in GlobalComableFilterTests.cs
- [fix] Phase2-Review iter5: AC#26c | Tightened matcher from `6[0-9]{2}` to `6[0-4][0-9]` — prevents false positives from IDs 650-699 outside Range6x migrated range (600-648)
- [fix] Phase2-Review iter6: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 6 + Success Criteria | Added AC#39 for SELECTCASE range routing test enforcement in GlobalComableFilterTests.cs (TFLAG:COMABLE管理 branch group)
- [fix] Phase2-Review iter7: AC Definition Table + AC Details + AC Coverage + Philosophy Derivation + Goal Coverage + Task 5 + Success Criteria | Added AC#40 for ITEquipVariables in ComableChecker constructor (TEQUIP/EQUIP domain interface DI gap)
- [fix] Phase2-Review iter8: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 5 + Success Criteria | Added AC#41 for globalFilter.IsGloballyBlocked invocation in coordinator (injection-without-invocation gap)
- [fix] Phase2-Review iter8: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 6 + Success Criteria | Added AC#42 for BATHROOM exclusion set {350,351,354} test coverage in GlobalComableFilterTests.cs
- [fix] Phase2-Review iter9: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 5 + Success Criteria | Added AC#43 for COM_ABLE512 IsGloballyBlocked(57) redirection in Range5x.cs (ERB anomaly fidelity)
- [fix] Phase2-Review iter10: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 5 + Success Criteria | Added AC#44 for TFlagComable coordinator pre-check (TFLAG:100 master gate)
- [fix] Phase4-ACValidation iter10: Goal Coverage row 1 | Added AC#35 (missing from Goal Coverage, flagged by ac-check lint)
- [fix] Phase4-ACValidation iter10: Success Criteria | Updated AC total count from 58 to 60
- [fix] Phase2-Review iter1: EXPLV signatures | Resolved [pending] EXPLV dimensionality: corrected all 2D signatures (CharacterId, ExpIndex) to 1D (int index) across AC#10, AC#21, Technical Design, Key Decisions, Execution Order, Implementation Contract, StubVariableStore
- [fix] Phase2-Review iter1: AC Details section | Reordered AC#40-AC#44 details from before AC#1 to after AC#39 (ascending order compliance)
- [fix] Phase2-Review iter1: Philosophy | Qualified F801 DI injection pattern claim with GlobalComableFilter concrete class deviation note
- [fix] Phase2-Review iter2: Key Decisions | Added Result<bool>→bool and ComId+CharacterId→int architecture deviation justification rows
- [fix] Phase2-Review iter3: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 5 + Success Criteria | Added AC#45-52 for missing ComableChecker constructor DI parameters (ILocationService, IItemVariables, IStringVariables, ICsvNameResolver)
- [fix] Phase2-Review iter4: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 6 + Success Criteria | Added AC#49 for TFLAG:100 master gate test coverage (ComableCheckerTests verifies gate behavior)
- [fix] Phase2-Review iter5: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 5 + Success Criteria | Added AC#50 for COM_ABLE189 constant-false switch case verification
- [fix] Phase2-Review iter5: Goal Coverage row 10 | Added AC#22 to Zero technical debt row (matching Philosophy Derivation)
- [fix] Phase2-Review iter5: AC#6 | Strengthened matcher from `public.*class` to `public sealed class` (verifies sealed modifier per Technical Design)
- [fix] Phase2-Review iter6: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 6 + Success Criteria | Added AC#51 for sleep-state branch test coverage in GlobalComableFilterTests.cs
- [fix] Phase2-Review iter7: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 5 + Success Criteria | Added AC#52 for IEngineVariables in ComableChecker constructor (provides GetAssiPlay, GetTarget, GetMaster)
- [fix] Phase2-Review iter8: CFLAG Character Resolution table | Fixed TARGET:1 mapping from engine.GetTarget() (TARGET:0) to engine.GetTarget(1) (secondary TARGET index 1)
- [fix] Phase4-ACValidation iter9: AC Definition Table | Fixed AC number collisions from renumber tool (restored AC#27, AC#33, AC#39, AC#40 from incorrectly renumbered AC#26, AC#31, AC#35, AC#36)
- [fix] Phase4-ACValidation iter9: AC#25, AC#33 | Fixed invalid matcher `count_gte` → `gte`
- [fix] Phase4-ACValidation iter9: AC#30 | Fixed Method format to include search pattern for count_equals method signature count
- [fix] Phase2-Review iter10: Goal Coverage row 11 | Added AC#31a, AC#31b, AC#31c to 'Equivalence tested' row (anomaly case traceability)
- [fix] Phase3-Maintainability iter10: AC Definition Table | Fixed duplicate AC numbers (restored AC#28, AC#37, AC#43, AC#44 and other collisions from renumber tool)
- [fix] PostLoop-UserFix: Philosophy | Revised scope from 'serving USERCOM.ERB and COMF*.ERB' to 'C# infrastructure for future ERB dispatch wiring (F810/F811 scope)'
- [fix] PostLoop-UserFix: AC Definition Table + AC Details + Key Decisions + Technical Design + Success Criteria | Removed IStringVariables+ICsvNameResolver from ComableChecker constructor (10→8 params), deleted AC#47+AC#48
- [resolved-applied] Phase2-Review iter6: ComableChecker constructor has IStringVariables (AC#47) and ICsvNameResolver (AC#48), but SAVESTR and TRAINNAME are only used in GlobalComableFilter (COMABLE2.ERB), not in individual @COM_ABLE{N} functions (COMABLE.ERB). GlobalComableFilter is injected as pre-constructed concrete class. If these params are stored but unused in ComableChecker, CS9113 (unused primary constructor parameter) triggers under TreatWarningsAsErrors. → Applied (a): removed IStringVariables+ICsvNameResolver from ComableChecker constructor (8 params), deleted AC#47+AC#48, updated Key Decision to "8 parameters".
- [fix] Phase2-Review iter1: Tasks table | Added missing Status column (template compliance)
- [fix] Phase2-Review iter1: AC Coverage table | Filled in empty How to Satisfy rows for all ACs
- [fix] Phase2-Review iter1: Task Tags table | Added missing Phase 4 column (template compliance)
- [fix] Phase2-Uncertain iter1: Philosophy Derivation table | Added MASTER_POSE stub equivalence deviation row (25 functions have partial equivalence pending F811)
- [fix] Phase2-Review iter1: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Tasks + Success Criteria | Renumbered AC#26 (SKILL.md IComableUtilities) to AC#53 to resolve collision with AC#26a/26b/26c
- [fix] Phase2-Uncertain iter2: AC#32 AC Definition Table + AC Details | Scoped Grep path from ComableChecker*.cs to ComableChecker.cs (coordinator only) to prevent false positives from range files
- [fix] Phase2-Review iter2: Goal Coverage row 1 | Removed AC#35 from migration completeness row (AC#35 is deferred-method guard, not migration verification)
- [fix] Phase2-Review iter3: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 4 + Success Criteria | Added AC#54 for SAVESTR slash-delimited format verification (prevents substring false positives)
- [fix] Phase2-Review iter3: AC#31 | Raised count_gte from 3 to 5 for improved Range0x sampling coverage
- [fix] Phase3-Maintainability iter4: Mandatory Handoffs | Added ComId.cs XML doc range update (0-299→0-648) handoff to F813
- [fix] Phase2-Review iter5: AC Definition Table + AC Details + AC Coverage + Goal Coverage + Task 6 + Success Criteria | Renumbered AC#31 (Range0x coverage) to AC#55 to resolve collision with AC#31a/31b/31c
- [fix] Phase2-Review iter6: AC#26a/26b/26c rationale | Fixed stale reference AC#33 → AC#55 for Range0x coverage (AC#33 is IComableUtilities injection, not Range0x)
- [fix] Phase2-Review iter7: AC#49 rationale | Fixed stale reference AC#48 → AC#44 (AC#48 was removed)
- [fix] Phase4-ACValidation iter8: Success Criteria | Fixed stale AC count 'All 66 ACs' → 'All 51 ACs'
- [resolved-skipped] Phase3-Maintainability iter9: GlobalComableFilter concrete injection vs interface (loop — same concern raised and rejected in iter4, iter5, iter7)
- [fix] Phase3-Maintainability iter9: Key Decisions | Added GetNoItem placement justification (IItemVariables vs IEngineVariables — item-restriction domain coherence)
- [fix] Phase3-Maintainability iter9: Tasks table Task 8 | Added SSOT verification ACs (AC#22, AC#53, AC#36a-36e) to tester gate
- [fix] Phase3-Maintainability iter9: Key Decisions | Added StubComableUtilities visibility justification (internal — temporary stub, InternalsVisibleTo for tests)
- [resolved-applied] Phase2-Review iter10: AC#55 distinct-ID weakness (loop — same concern raised in iter5, iter8)
- [fix] Phase2-Review iter10: Philosophy Derivation | Qualified SSOT claim to 'C# layer SSOT' (ERB dispatch wiring deferred to F810/F811)
- [fix] Phase2-Review iter1: AC Definition Table | Moved AC#53 from between AC#26c/AC#27 to sequential position after AC#52
- [fix] Phase2-Review iter1: AC#25 + AC#55 AC Definition Table | Added missing Grep pattern strings to Method column
- [fix] Phase2-Uncertain iter1: AC#56 | Added new AC for StubComableUtilities MasterPose conservative default return 0 (Philosophy-to-AC gap)
- [fix] Phase2-Review iter2: AC#57 | Added new AC for query-only enforcement (not_matches .Set[A-Z] in Comable subsystem)
- [fix] Phase2-Review iter2: AC#26c | Fixed regex from 6[0-4][0-9] (600-649) to strict 600-648 range
- [fix] PostLoop-UserFix: AC#55 | Split into AC#55a/55b/55c for distinct Range0x sub-range coverage (low 0-66, mid 67-133, high 134-199)
- [fix] Phase2-Review iter1: AC#36 | Removed redundant AC (subsumed by AC#57 .Set[A-Z] not_matches)
- [fix] Phase2-Review iter2: AC#58 | Added new AC for no Stub instantiation inside ComableChecker (DI pattern enforcement)
- [fix] Phase4-ACValidation iter3: Success Criteria | Fixed stale AC count 'All 54 ACs' → 'All 52 ACs'
- [fix] Phase4-ACValidation iter3: AC#55a | Fixed regex alternation grouping (added regex group parens around alternation, matching AC#55b/55c pattern)
- [fix] Phase2-Review iter4: AC#55b | Fixed regex to include 6[7-9] for IDs 67-69 (was starting at 70, missing 67-69)
- [fix] Phase2-Review iter4: AC#55c | Fixed regex from (1[3-9][0-9]|19[0-9]) to (13[4-9]|1[4-9][0-9]) for strict 134-199 range (was overlapping with AC#55b at 130-133)
- [fix] Phase2-Review iter5: AC#55c rationale | Removed misleading 'excluding 189' note; clarified AC#31c independently verifies 189
- [fix] Phase2-Review iter6: AC#55a | Fixed regex to include single-digit IDs 0-9 (was [0-5][0-9] requiring 2 digits, missing COM IDs 0-9)
- [fix] Phase3-Maintainability iter7: Tasks + Execution Order | Added Task 3b (Era.Core concrete/null impl updates) and Task 3c (Era.Core.Tests mock updates) for interface extension cascade
- [fix] Phase3-Maintainability iter7: Task 6b | Expanded scope to include StubEngineVariables in engine.Tests
- [fix] Phase3-Maintainability iter7: Impact Analysis | Added Era.Core concrete impls, Era.Core.Tests mocks, tools/KojoComparer cascade impact rows
- [fix] Phase3-Maintainability iter7: Mandatory Handoffs | Added KojoComparer VariableStoreAdapter update entry
- [fix] Phase2-Review iter8: Philosophy Derivation | Added AC#55a/55b/55c to 'equivalence-tested' row (Range0x coverage was missing from derivation)
- [fix] Phase2-Review iter9: Task 3c + Mandatory Handoffs | Added KojoComparer VariableStoreAdapter explicitly to Task 3c scope; removed ambiguous 'or separate maintenance' from handoff
- [fix] Phase2-Review iter1: AC Definition Table | Reordered AC rows to ascending numeric order (was out of sequence: 31a-c before 30, 55a-c before 32, etc.)
- [fix] Phase2-Review iter1: AC Details section | Moved AC#53 detail block from between AC#26c/AC#27 to after AC#52 (sequential order compliance)
- [fix] Phase2-Review iter1: Tasks table | Normalized header separator row spacing to match template
- [fix] Phase2-Review iter1: AC#9, AC#10, AC#19, Baseline, Interface gap analysis | Fixed: Baseline IVariableStore count updated 40→43 (F804 added GetExpLv+GetNoItem). AC#19 count_equals updated 42→44 (current 43 + 1 SetExpLv). AC#10 rationale updated: GetExpLv pre-exists from F804, AC#10 verifies pre-existing method. Task 3 updated: GetExpLv verify-only, only SetExpLv is new. Baseline NOITEM/EXPLV rows updated as stale. Technical Design Interface Extensions block updated: GetExpLv removed from new additions, SetExpLv-only noted. Note: Result<int> GetExpLv(int level) vs int GetExpLv(int index) signature discrepancy: actual signature is Result<int> GetExpLv(int level) per IVariableStore.cs; spec mentioned int return — this is pre-existing F804 API, no change needed for F809.
- [fix] Phase2-Review iter3: AC#19 | Corrected count_equals 44→43 (grep pattern excludes Result<string> GetCharacterString; pattern-matched count = 42 current + 1 SetExpLv = 43)
- [fix] Phase2-Review iter4: AC Definition Table + AC Details + AC Coverage + Task 3c + Success Criteria | Added AC#59 for KojoComparer VariableStoreAdapter GetExpLv/SetExpLv stub verification (tools/ build gate gap)
- [fix] Phase2-Review iter4: Implementation Contract Pre-conditions | Clarified IVariableStore count: 43 total (42 grep-pattern-matched + 1 Result<string>); after Task 3: 44 total, 43 grep-matched (AC#19 target)
- [fix] Phase2-Review iter5: AC#32 rationale + Execution Order step 5 | Added evidence citation for Range0x 86-function count (verified: 87 @COM_ABLE functions in COMABLE.ERB IDs 0-199, minus COM_ABLE189 permanent disable = 86 IsAvailableN dispatch)
- [fix] Phase2-Review iter6: Key Decisions + Philosophy Derivation | Added GetNoItem getter/setter exemption justification (NOITEM is engine-set via ITEM:0-based, read-only for game logic; no SetNoItem needed)
- [fix] Phase3-Maintainability iter7: Key Decisions + Mandatory Handoffs | Added IComableUtilities.MasterPose vs ICounterUtilities.MasterPose duplication documentation (temporary, unification tracked to F811)
- [fix] Phase3-Maintainability iter7: Key Decisions + Mandatory Handoffs | Added GetNoItem call ambiguity resolution (ComableChecker range methods use items.GetNoItem() per ISP; IVariableStore.GetNoItem() cleanup tracked to F813)
- [fix] Phase2-Review iter8: Goal Coverage | Moved AC#30 from Goal Item 1 (migration coverage) to new Goal Item 14 (IComAvailabilityChecker single-method contract with AC#35)
- [fix] Phase2-Review iter9: Philosophy Derivation | Expanded '124 COM_ABLE functions' row from AC#23 alone to AC#23, AC#28, AC#32, AC#50, AC#31a/b/c (complete migration verification coverage)
- [fix] Phase2-Review iter10: AC#55a/55b/55c | Fixed regex alternation: replaced \\| (literal backslash-pipe) with | (ERE alternation) in all three Range0x sub-range patterns
- [fix] Phase5-Feasibility iter10: Pre-conditions + Task 3 + Baseline | Updated IEngineVariables: 24→25 (GetAssiPlay already exists from F812), Task 3 GetAssiPlay marked verify-only, Baseline updated
- [fix] Phase7-FinalRefCheck iter10: Links section | Added F804 and F812 to Links (referenced in Baseline/Pre-conditions but missing from Links)
- [fix] Phase2-Review iter1: AC Definition Table line 331 | Inserted blank line between AC#59 table row and ### AC Details heading (markdown rendering fix)
- [fix] Phase2-Uncertain iter2: AC#8 + AC#10 AC Definition Table | Changed 'added' to 'exists (added by F812/F804, verified by F809)' — pre-existing methods, not new additions
- [fix] Phase2-Review iter2: Goal section | Revised '5 interface gaps' to '3 remaining + 2 verified pre-existing' (ASSIPLAY from F812, EXPLV getter from F804)
- [fix] Phase2-Review iter2: Philosophy Derivation query-only row | Extended scope from 'ComableChecker methods' to 'ComableChecker and GlobalComableFilter' (AC#57 covers entire Comable subsystem)
- [fix] Phase2-Review iter3: Execution Order 6b + StubVariableStore snippet | Fixed GetExpLv return type from `int` to `Result<int>` and param from `index` to `level` (matching IVariableStore.cs actual signature)
- [fix] Phase2-Review iter4: AC#17 description | Changed 'method count preserved + 1' to 'method count preserved at 25' (GetAssiPlay already added by F812, no +1 in F809)
- [fix] Phase2-Review iter4: Tasks table Task 3 | Added [V] annotation to AC#8 and AC#10 (verify-only, pre-existing from F812/F804, no TDD RED phase)
- [fix] Phase2-Review iter4: AC#39 matcher | Broadened from naming-convention-only to behavioral pattern (GetTFlag/TFlag/tflag/COMABLE管理/ComableManagement)
- [fix] Phase2-Review iter5: Task 6b StubEngineVariables | Removed unnecessary GetAssiPlay() override instruction (IEngineVariables has DIM `GetAssiPlay() => 0` from F812)

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning
- [Related: F801](feature-801.md) - Main Counter Core (DI injection pattern reference: ActionValidator.cs:11-16)
- [Successor: F810](feature-810.md) - COMABLE Extended (calls GlobalComableFilter.IsGloballyBlocked 44 times)
- [Related: F811](feature-811.md) - SOURCE Entry System (owns MASTER_POSE in SOURCE_POSE.ERB:334; IComableUtilities concrete impl)
- [Successor: F813](feature-813.md) - Post-Phase Review Phase 21
- [Related: F815](feature-815.md) - StubVariableStore base class for testing
- [Related: F804](feature-804.md) - WC Counter Core (added GetExpLv, GetNoItem, GetAssiPlay to interfaces)
- [Related: F812](feature-812.md) - GetAssiPlay default implementation on IEngineVariables

