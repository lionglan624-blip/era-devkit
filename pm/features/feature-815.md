# Feature 815: Extract StubVariableStore Base Class in engine.Tests

## Status: [DONE]
<!-- fl-reviewed: 2026-02-23T08:30:43Z -->

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

## Type: infra

## Deviation Context
<!-- Written by /run Phase 9. Raw facts only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F801 |
| Discovery Phase | Commit (post-implementation) |
| Timestamp | 2026-02-23 |

### Observable Symptom
F801 added GetEquip/SetEquip to IVariableStore and GetTarget(int)/SetTarget(int,int)/GetSelectCom() to IEngineVariables. engine.Tests had 5 separate IVariableStore stub classes that all failed to compile (CS0535). Required manual stub additions in 5 files.

### Execution Evidence
| Field | Value |
|-------|-------|
| Command | `dotnet test engine.Tests/uEmuera.Tests.csproj` |
| Exit Code | 1 |
| Error Output | `CS0535: 'MockVariableStore' does not implement interface member 'IVariableStore.GetEquip(CharacterId, int)'` (×11 errors across 5 files) |
| Expected | Build success |
| Actual | Build failure in 5 test files |

### Files Involved
| File | Relevance |
|------|-----------|
| `engine.Tests/Tests/TestStubs.cs` | Already has StubEngineVariables; target for new StubVariableStore |
| `engine.Tests/Tests/GameInitializationTests.cs:22` | MockVariableStore (40 method stubs) |
| `engine.Tests/Tests/HeadlessIntegrationTests.cs:22` | MockVariableStore (40 method stubs) |
| `engine.Tests/Tests/NtrInitializationTests.cs:21` | TrackingVariableStore (40 method stubs, 2 custom) |
| `engine.Tests/Tests/StateSettingsTests.cs:36` | MockVariableStore (40 method stubs, 6 custom) |

### Attempted Solutions
| Attempt | Result | Why Failed |
|---------|--------|------------|
| Manual stub addition to 5 files | PARTIAL | Does not prevent recurrence on future IVariableStore additions |

### Parent Session Observations
Every Phase 21+ feature that adds methods to IVariableStore or IEngineVariables will hit this same problem. StubEngineVariables in TestStubs.cs already follows the shared pattern for IEngineVariables — IVariableStore needs the same treatment.

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)
Test infrastructure must scale with interface evolution. As C# migration (Phase 20+) progressively extends IVariableStore with new variable accessors, engine.Tests stubs must not require N-file updates per method addition. The SSOT for IVariableStore test stubs in engine.Tests should be a single shared base class in TestStubs.cs, following the established StubEngineVariables precedent.

### Problem (Current Issue)
IVariableStore has grown to 40 methods (20 getter/setter pairs in `Era.Core/Interfaces/IVariableStore.cs:15-108`) through incremental additions across features F393, F399, F400, F469, and F801. Because no shared StubVariableStore base class exists in `engine.Tests/Tests/TestStubs.cs` (unlike StubEngineVariables at line 64 which already centralizes IEngineVariables stubs), each of the 4 test files independently duplicates all 40 method implementations as private nested classes. This creates an O(N*M) maintenance burden: when IVariableStore gains new methods (as F801 added GetEquip/SetEquip), all 4 files must be manually updated, causing CS0535 build failures. The stubs were originally created during F365, F370, and F371 when the interface was smaller, and no refactoring was performed as the interface grew because each feature's scope was limited to adding methods.

### Goal (What to Achieve)
Extract a shared `internal class StubVariableStore : IVariableStore` in `engine.Tests/Tests/TestStubs.cs` with all 40 methods as `virtual` with safe return defaults. Refactor the 4 test files (`GameInitializationTests.cs`, `HeadlessIntegrationTests.cs`, `NtrInitializationTests.cs`, `StateSettingsTests.cs`) to inherit from this base class, keeping only their custom overrides and helper methods. Future IVariableStore extensions will require updating only 1 file (TestStubs.cs) instead of 4+.

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why did F801 cause build failures in engine.Tests? | F801 added GetEquip/SetEquip to IVariableStore, and 4 test files had independent IVariableStore implementations that lacked these methods (CS0535) | `feature-815.md:40-43` (Error Output) |
| 2 | Why do 4 test files each have independent IVariableStore implementations? | Each test file declares its own private class implementing all 40 IVariableStore methods | `GameInitializationTests.cs:22-64`, `HeadlessIntegrationTests.cs:22-64`, `NtrInitializationTests.cs:21-75`, `StateSettingsTests.cs:36-112` |
| 3 | Why is there no shared stub base class for IVariableStore? | TestStubs.cs has StubEngineVariables (line 64) and StubVisitorVariables (line 14) but the same pattern was never applied to IVariableStore | `TestStubs.cs:1-91` |
| 4 | Why was the pattern not applied to IVariableStore? | The stubs were created early (F365, F370, F371) when IVariableStore was smaller; the interface grew from ~12 to 40 methods across F393, F399, F400, F469, F801 | `IVariableStore.cs` feature annotations; test file doc comments |
| 5 | Why (Root)? | No refactoring was performed as the interface grew because each feature's scope was limited to adding methods, not consolidating test infrastructure | `feature-815.md:59-60` (Parent Session Observations) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | CS0535 build failures in 4 test files when IVariableStore gains new methods | No shared StubVariableStore base class centralizing the 40-method interface implementation |
| Where | 4 test files (GameInitializationTests, HeadlessIntegrationTests, NtrInitializationTests, StateSettingsTests) | TestStubs.cs: missing StubVariableStore alongside existing StubEngineVariables |
| Fix | Manually add missing methods to each file | Extract StubVariableStore base class with virtual methods; subclass in each test file |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F801 | [DONE] | Predecessor -- added GetEquip/SetEquip to IVariableStore, triggering the deviation discovery |
| F802-F812 | [DRAFT] | Successor -- Phase 21 Counter features likely to add more IVariableStore methods; will benefit from this refactoring |
| F783 | [DONE] | Related -- Phase 21 decomposition that created F802-F812 |
| F404 | [DONE] | Related -- IVariableStore ISP Segregation (historical context for interface growth) |
| F365 | [DONE] | Related -- Created GameInitializationTests MockVariableStore |
| F370 | [DONE] | Related -- Created StateSettingsTests MockVariableStore |
| F371 | [DONE] | Related -- Created NtrInitializationTests TrackingVariableStore |
| F800 | [DONE] | Related -- Also involved IVariableStore extension |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Technical viability | FEASIBLE | C# virtual method override pattern is standard; net10.0 / C# 14 target fully supports inheritance |
| Pattern precedent | FEASIBLE | StubEngineVariables in TestStubs.cs:64-91 follows this pattern for IEngineVariables (internal visibility, safe-default returns); virtual methods are a new addition required by C2 constraint for subclass overrides |
| Behavioral preservation | FEASIBLE | GameInit and Headless stubs are byte-identical (trivial replacement); NtrInit needs 2 overrides; StateSettings needs 4 overrides |
| Test isolation | FEASIBLE | Base class is `internal` (cross-file within test project); subclasses remain `private` nested classes |
| Build constraints | FEASIBLE | Additive refactoring; TreatWarningsAsErrors=true handled by matching StubEngineVariables pattern |
| Scope containment | FEASIBLE | Exactly 5 files in engine.Tests; Era.Core.Tests (26 files) explicitly out of scope |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| engine.Tests test infrastructure | HIGH | 4 test files refactored to use shared base class; ~150 lines of duplication eliminated |
| Future IVariableStore extensions | HIGH | N-file update problem reduced to 1-file update for all future method additions |
| Production code | LOW | Zero production code changes; test infrastructure only |
| Era.Core.Tests | LOW | Same problem exists at 26x scale but explicitly out of scope; follow-up feature needed |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Methods must be `virtual` | NtrInitializationTests and StateSettingsTests need overrides for custom behavior | StubVariableStore methods must be `virtual` to allow subclass customization |
| `internal` visibility required | TestStubs.cs precedent (StubEngineVariables at line 64) | StubVariableStore must be `internal` (not `public`) matching existing pattern |
| `TreatWarningsAsErrors=true` | `Directory.Build.props` | All unused parameters and methods must not trigger warnings |
| `Nullable=disable` in engine.Tests | `uEmuera.Tests.csproj:6` | No nullable annotations needed |
| `sealed` subclass compatibility | `NtrInitializationTests.cs:21` (TrackingVariableStore is `private sealed`) | `sealed` on subclass of non-sealed base is valid C#; no issue |
| Default return strategy | Behavior comparison across 4 stubs | Design decision: return-defaults (Ok(0)/0/no-op) following StubEngineVariables precedent; no test asserts on NotImplementedException |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Default behavior change from NIE to safe defaults alters test semantics | LOW | LOW | No test asserts on NotImplementedException; tests only call 2-4 methods per stub; changing uncalled methods is invisible |
| StateSettingsTests MockVariableStore has complex ConcurrentDictionary logic hard to preserve | LOW | MEDIUM | Override 4 methods (Get/SetCharacterFlag, Get/SetTalent) and keep helper methods (SetCFlag, GetCFlag, SetTALENT) in subclass |
| Era.Core.Tests stubs remain vulnerable to same N-file problem | HIGH | LOW | Explicitly out of scope for F815; separate follow-up feature needed |
| Future IVariableStore additions before F815 is implemented | LOW | LOW | No features currently planned to add methods before F815 |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| IVariableStore stub count in engine.Tests | `grep -r "class.*IVariableStore" engine.Tests/` | 4 | 4 private classes across 4 test files |
| IVariableStore method count | `grep -c ";" Era.Core/Interfaces/IVariableStore.cs` | 40 | 20 getter/setter pairs |
| Test count | `dotnet test engine.Tests/ --list-tests` | (run at implementation time) | Must be preserved after refactoring |

**Baseline File**: `.tmp/baseline-815.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | StubVariableStore must implement all 40 IVariableStore methods | `IVariableStore.cs:15-108` | AC must verify interface completeness (build success) |
| C2 | All methods in StubVariableStore must be `virtual` | NtrInitializationTests and StateSettingsTests need overrides | AC must verify virtual keyword presence |
| C3 | StubVariableStore must be `internal class` in TestStubs.cs | StubEngineVariables precedent at TestStubs.cs:64 | AC must verify access modifier and file location |
| C4 | Default behavior: getters return 0/Ok(0), setters no-op | StubEngineVariables precedent; no test asserts on NIE | AC must verify return-default strategy |
| C5 | GameInitializationTests and HeadlessIntegrationTests MockVariableStore become empty or minimal subclasses | These are byte-identical duplicates with no customization beyond base defaults | AC should verify these files no longer contain full method listings |
| C6 | NtrInitializationTests TrackingVariableStore must preserve SeedCflag/ReadCflag helper API | Dictionary-backed custom tracking with 2 helper methods | AC must verify SeedCflag and ReadCflag still function |
| C7 | StateSettingsTests MockVariableStore must preserve SetCFlag/GetCFlag/SetTALENT helper API | ConcurrentDictionary-backed with 3 convenience helpers | AC must verify these helpers still function |
| C8 | All existing tests must pass unchanged | Immutable Tests principle; zero-regression requirement | AC must verify dotnet test passes with zero failures |
| C9 | StubVariableStore namespace must be MinorShift.Emuera.Tests | `TestStubs.cs:4` namespace | AC must verify namespace consistency |
| C10 | No production code changes | Test infrastructure only | AC must verify no Era.Core or engine source changes |

### Constraint Details

**C1: Interface Completeness**
- **Source**: `Era.Core/Interfaces/IVariableStore.cs` -- 20 getter/setter pairs = 40 methods
- **Verification**: Build success confirms all interface methods implemented
- **AC Impact**: AC must include build verification (dotnet build/test)

**C2: Virtual Methods**
- **Source**: NtrInitializationTests overrides GetCharacterFlag/SetCharacterFlag; StateSettingsTests overrides Get/SetCharacterFlag and Get/SetTalent
- **Verification**: Grep for `virtual` keyword in StubVariableStore methods
- **AC Impact**: AC must verify at least one `virtual` method declaration in StubVariableStore

**C3: Internal Visibility and File Location**
- **Source**: StubEngineVariables at TestStubs.cs:64 uses `internal class`
- **Verification**: Grep for `internal class StubVariableStore` in TestStubs.cs
- **AC Impact**: AC must verify class declaration matches pattern

**C4: Default Return Strategy**
- **Source**: StubEngineVariables returns 0/string.Empty/no-op; no test asserts on NotImplementedException across any stub
- **Verification**: Grep for absence of `NotImplementedException` in StubVariableStore
- **AC Impact**: AC must verify StubVariableStore does not throw NotImplementedException

**C5: Duplicate Elimination**
- **Source**: GameInitializationTests.cs:22-64 and HeadlessIntegrationTests.cs:22-64 are byte-identical
- **Verification**: Line count reduction in these files
- **AC Impact**: AC should verify these files inherit from StubVariableStore

**C6: NtrInitializationTests Helper Preservation**
- **Source**: `NtrInitializationTests.cs:23-35` -- Dictionary-backed CFLAG + SeedCflag/ReadCflag
- **Verification**: Test execution of NtrInitialization tests
- **AC Impact**: AC must verify test suite passes (covers helper functionality)

**C7: StateSettingsTests Helper Preservation**
- **Source**: `StateSettingsTests.cs:38-73` -- ConcurrentDictionary-backed + SetCFlag/GetCFlag/SetTALENT
- **Verification**: Test execution of StateSettings tests
- **AC Impact**: AC must verify test suite passes (covers helper functionality)

**C8: Zero Regression**
- **Source**: Immutable Tests principle from CLAUDE.md
- **Verification**: `dotnet test engine.Tests/` passes with same test count
- **AC Impact**: AC must verify all tests pass

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F801 | [DONE] | Added GetEquip/SetEquip to IVariableStore that triggered this deviation |
| Successor | F802-F812 | [DRAFT] | Phase 21 Counter features will benefit from centralized stub; may add more IVariableStore methods |

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
| "Test infrastructure **must** scale with interface evolution" | A single shared base class must exist so future IVariableStore method additions require updating only 1 file | AC#1, AC#2, AC#17 (base class exists with virtual defaults); AC#3-6 + AC#8-11 (inheritance ensures subclasses use base methods; new IVariableStore methods only need adding to StubVariableStore) |
| "engine.Tests stubs **must not** require N-file updates per method addition" | The 4 test files must inherit from the shared base class instead of independently implementing IVariableStore | AC#3, AC#4, AC#5, AC#6 (inheritance) + AC#8, AC#9, AC#10, AC#11 (no direct IVariableStore bypass) |
| "SSOT for IVariableStore test stubs should be a **single** shared base class in TestStubs.cs" | StubVariableStore must be the single internal class in TestStubs.cs implementing IVariableStore with virtual methods and safe defaults | AC#1, AC#2, AC#7, AC#17 |
| "Test infrastructure only" (Goal scope boundary) | No changes to production code (Era.Core/, engine/) are permitted; this feature exclusively modifies engine.Tests | AC#14, AC#18 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | StubVariableStore class exists in TestStubs.cs | code | Grep(engine.Tests/Tests/TestStubs.cs) | matches | `internal class StubVariableStore : IVariableStore` | [x] |
| 2 | StubVariableStore methods are virtual | code | Grep(pattern="public virtual", path="engine.Tests/Tests/TestStubs.cs", output_mode="count") | gte | `40` | [x] |
| 3 | GameInitializationTests inherits from StubVariableStore | code | Grep(engine.Tests/Tests/GameInitializationTests.cs) | matches | `: StubVariableStore` | [x] |
| 4 | HeadlessIntegrationTests inherits from StubVariableStore | code | Grep(engine.Tests/Tests/HeadlessIntegrationTests.cs) | matches | `: StubVariableStore` | [x] |
| 5 | NtrInitializationTests inherits from StubVariableStore | code | Grep(engine.Tests/Tests/NtrInitializationTests.cs) | matches | `: StubVariableStore` | [x] |
| 6 | StateSettingsTests inherits from StubVariableStore | code | Grep(engine.Tests/Tests/StateSettingsTests.cs) | matches | `: StubVariableStore` | [x] |
| 7 | StubVariableStore uses safe return defaults | code | Grep(engine.Tests/Tests/TestStubs.cs) | matches | `virtual.*Result<int>.*Ok\(0\)` | [x] |
| 8 | GameInitializationTests has no direct IVariableStore implementation | code | Grep(engine.Tests/Tests/GameInitializationTests.cs) | not_matches | `class.*: IVariableStore` | [x] |
| 9 | HeadlessIntegrationTests has no direct IVariableStore implementation | code | Grep(engine.Tests/Tests/HeadlessIntegrationTests.cs) | not_matches | `class.*: IVariableStore` | [x] |
| 10 | NtrInitializationTests has no direct IVariableStore implementation | code | Grep(engine.Tests/Tests/NtrInitializationTests.cs) | not_matches | `class.*: IVariableStore` | [x] |
| 11 | StateSettingsTests has no direct IVariableStore implementation | code | Grep(engine.Tests/Tests/StateSettingsTests.cs) | not_matches | `class.*: IVariableStore` | [x] |
| 12 | TestStubs.cs has no NotImplementedException | code | Grep(engine.Tests/Tests/TestStubs.cs) | not_matches | `NotImplementedException` | [x] |
| 13 | All engine.Tests tests pass | test | dotnet test engine.Tests/ | succeeds | 0 test failures | [x] |
| 14 | No production code changes | code | Grep(Era.Core/) | not_matches | `StubVariableStore` | [x] |
| 15 | Era.Core.Tests extraction DRAFT created | file | Glob(pm/features/feature-816.md) | exists | file exists | [x] |
| 16 | F816 registered in index-features.md | code | Grep(pm/index-features.md) | matches | `F816` | [x] |
| 17 | StubVariableStore 1D getters use safe defaults | code | Grep(engine.Tests/Tests/TestStubs.cs) | matches | `virtual int Get.*=> 0` | [x] |
| 18 | No StubVariableStore in engine source | code | Grep(engine/) | not_matches | `StubVariableStore` | [x] |
| 19 | StubVariableStore is in correct namespace | code | Grep(engine.Tests/Tests/TestStubs.cs) | matches | `namespace MinorShift.Emuera.Tests` | [x] |

### AC Details

**AC#1: StubVariableStore class exists in TestStubs.cs**
- **Test**: `Grep("internal class StubVariableStore : IVariableStore", path="engine.Tests/Tests/TestStubs.cs")`
- **Expected**: At least 1 match confirming the class declaration exists
- **Rationale**: C3 constraint requires internal visibility; class must implement IVariableStore in TestStubs.cs following StubEngineVariables precedent (C1, C3)

**AC#2: StubVariableStore methods are virtual**
- **Test**: `Grep("public virtual", path="engine.Tests/Tests/TestStubs.cs", output_mode="count")`
- **Expected**: ≥ 40 matching lines (StubVariableStore contributes 40; existing stubs may add more)
- **Rationale**: C2 constraint requires all 40 IVariableStore methods to be `public virtual` so NtrInitializationTests and StateSettingsTests can override specific methods. Count-based verification ensures a partial implementation (e.g., only 4 virtual methods) cannot pass.

**AC#3: GameInitializationTests inherits from StubVariableStore**
- **Test**: `Grep(": StubVariableStore", path="engine.Tests/Tests/GameInitializationTests.cs")`
- **Expected**: At least 1 match showing MockVariableStore now extends StubVariableStore
- **Rationale**: C5 constraint - this file's MockVariableStore is byte-identical to HeadlessIntegrationTests; after refactoring it should inherit with minimal/no overrides

**AC#4: HeadlessIntegrationTests inherits from StubVariableStore**
- **Test**: `Grep(": StubVariableStore", path="engine.Tests/Tests/HeadlessIntegrationTests.cs")`
- **Expected**: At least 1 match showing MockVariableStore now extends StubVariableStore
- **Rationale**: C5 constraint - same as AC#3, byte-identical duplicate should become minimal subclass

**AC#5: NtrInitializationTests inherits from StubVariableStore**
- **Test**: `Grep(": StubVariableStore", path="engine.Tests/Tests/NtrInitializationTests.cs")`
- **Expected**: At least 1 match showing TrackingVariableStore now extends StubVariableStore
- **Rationale**: C6 constraint - TrackingVariableStore must preserve SeedCflag/ReadCflag helpers while inheriting base implementations

**AC#6: StateSettingsTests inherits from StubVariableStore**
- **Test**: `Grep(": StubVariableStore", path="engine.Tests/Tests/StateSettingsTests.cs")`
- **Expected**: At least 1 match showing MockVariableStore now extends StubVariableStore
- **Rationale**: C7 constraint - MockVariableStore must preserve SetCFlag/GetCFlag/SetTALENT helpers while inheriting base implementations

**AC#7: StubVariableStore uses safe return defaults**
- **Test**: `Grep("virtual.*Result<int>.*Ok\\(0\\)", path="engine.Tests/Tests/TestStubs.cs")`
- **Expected**: At least 1 match confirming virtual methods return Result<int>.Ok(0) as default
- **Rationale**: C4 constraint - default return strategy follows StubEngineVariables precedent (returns 0/Ok(0)/no-op). Currently 0 matches (StubVariableStore does not yet exist), confirming RED state. After implementation, virtual methods returning Result<int> should use Ok(0) defaults.

**AC#8: GameInitializationTests has no direct IVariableStore implementation**
- **Test**: `Grep("class.*: IVariableStore", path="engine.Tests/Tests/GameInitializationTests.cs")`
- **Expected**: 0 matches - MockVariableStore should inherit StubVariableStore, not IVariableStore directly
- **Rationale**: Ensures duplication is eliminated; currently matches 1 time (line 22), so this AC is in RED state

**AC#9: HeadlessIntegrationTests has no direct IVariableStore implementation**
- **Test**: `Grep("class.*: IVariableStore", path="engine.Tests/Tests/HeadlessIntegrationTests.cs")`
- **Expected**: 0 matches - MockVariableStore should inherit StubVariableStore, not IVariableStore directly
- **Rationale**: Currently matches 1 time (line 22), so this AC is in RED state

**AC#10: NtrInitializationTests has no direct IVariableStore implementation**
- **Test**: `Grep("class.*: IVariableStore", path="engine.Tests/Tests/NtrInitializationTests.cs")`
- **Expected**: 0 matches - TrackingVariableStore should inherit StubVariableStore, not IVariableStore directly
- **Rationale**: Currently matches 1 time (line 21), so this AC is in RED state

**AC#11: StateSettingsTests has no direct IVariableStore implementation**
- **Test**: `Grep("class.*: IVariableStore", path="engine.Tests/Tests/StateSettingsTests.cs")`
- **Expected**: 0 matches - MockVariableStore should inherit StubVariableStore, not IVariableStore directly
- **Rationale**: Currently matches 1 time (line 36), so this AC is in RED state

**AC#12: TestStubs.cs has no NotImplementedException**
- **Test**: `Grep("NotImplementedException", path="engine.Tests/Tests/TestStubs.cs")`
- **Expected**: 0 matches — TestStubs.cs should contain only safe-default stubs, never throwing NotImplementedException
- **Rationale**: C4 constraint — return-default strategy. Currently TestStubs.cs has 0 matches for `NotImplementedException` (existing stubs use safe defaults). After adding StubVariableStore, this AC verifies the new class follows the same safe-default pattern. Note: currently passes (no NIE in file), but becomes a regression guard ensuring no one adds NIE-throwing methods to TestStubs.cs.

**AC#13: All engine.Tests tests pass**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test engine.Tests/'`
- **Expected**: Exit code 0, all tests pass with zero failures
- **Rationale**: C8 constraint - zero regression; immutable tests principle. All existing tests (GameInitialization, HeadlessIntegration, NtrInitialization, StateSettings) must continue to pass after refactoring

**AC#14: No production code changes**
- **Test**: `Grep("StubVariableStore", path="Era.Core/")`
- **Expected**: 0 matches - StubVariableStore must not appear in production code
- **Rationale**: C10 constraint - this is test infrastructure only; production code (Era.Core, engine) must not be modified

**AC#15: Era.Core.Tests extraction DRAFT created**
- **Test**: `Glob("pm/features/feature-816.md")`
- **Expected**: file exists — Task#7 created the DRAFT feature file
- **Rationale**: Mandatory Handoff tracked in Tasks table; DRAFT creation checklist requires file existence verification

**AC#16: F816 registered in index-features.md**
- **Test**: `Grep("F816", path="pm/index-features.md")`
- **Expected**: At least 1 match — F816 row added to Active Features table in index-features.md
- **Rationale**: DRAFT Creation Checklist requires both file creation AND index registration. Verifies Task#7 completed both steps.

**AC#17: StubVariableStore 1D getters use safe defaults**
- **Test**: `Grep("virtual int Get.*=> 0", path="engine.Tests/Tests/TestStubs.cs")`
- **Expected**: At least 1 match confirming 1D getter methods return `int` with safe default `0`
- **Rationale**: C4 constraint — 1D getters (`GetFlag`, `GetTFlag`) return `int` (not `Result<int>`), so AC#7's pattern `virtual.*Result<int>.*Ok(0)` does not cover them. This AC fills the gap ensuring 1D getters follow the safe-default strategy. Scope note: existing stubs (StubVisitorVariables, StubEngineVariables) have zero virtual methods, so all matches come from StubVariableStore. 1D setter safe defaults (no-op `{ }`) are verified indirectly by AC#12 (no NotImplementedException) and AC#13 (all tests pass).

**AC#18: No StubVariableStore in engine source**
- **Test**: `Grep("StubVariableStore", path="engine/")`
- **Expected**: 0 matches — StubVariableStore must not appear in engine production source
- **Rationale**: C10 constraint — no production code changes. AC#14 checks `Era.Core/` but C10 also includes `engine/` source files. This AC closes the gap ensuring no engine source modifications.

**AC#19: StubVariableStore is in correct namespace**
- **Test**: `Grep("namespace MinorShift.Emuera.Tests", path="engine.Tests/Tests/TestStubs.cs")`
- **Expected**: At least 1 match confirming the existing namespace declaration
- **Rationale**: C9 constraint requires StubVariableStore to be in `MinorShift.Emuera.Tests` namespace (matching TestStubs.cs:4). AC#1 verifies class declaration but not namespace. Since `internal` is assembly-scoped (not namespace-scoped), a wrong namespace compiles fine but violates consistency.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Extract shared `internal class StubVariableStore : IVariableStore` in TestStubs.cs with all 40 methods as virtual with safe return defaults | AC#1, AC#2, AC#7, AC#12, AC#17, AC#19 |
| 2 | Refactor GameInitializationTests to inherit from base class | AC#3, AC#8 |
| 3 | Refactor HeadlessIntegrationTests to inherit from base class | AC#4, AC#9 |
| 4 | Refactor NtrInitializationTests to inherit, keeping custom overrides and helpers | AC#5, AC#10 |
| 5 | Refactor StateSettingsTests to inherit, keeping custom overrides and helpers | AC#6, AC#11 |
| 6 | All tests pass after refactoring (zero regression) | AC#13 |
| 7 | No production code changes | AC#14, AC#18 |
| 8 | Create DRAFT for Era.Core.Tests extraction | AC#15, AC#16 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Add `internal class StubVariableStore : IVariableStore` to `engine.Tests/Tests/TestStubs.cs`, following the established `StubEngineVariables` precedent for internal visibility and safe return defaults, with the addition of `virtual` modifiers required by C2 constraint (NtrInitializationTests and StateSettingsTests need method overrides). All 40 IVariableStore methods are declared `public virtual` with safe return defaults: 1D-array getters (`GetFlag`, `GetTFlag`) return `0`; 2D-array getters return `Result<int>.Ok(0)`; 1D setter `SetPalamLv` and `GetPalamLv` follow the same split; all setters are no-ops (`{ }`). This matches the C4 constraint (default-return strategy) and follows C#'s standard virtual-override pattern.

Each of the 4 test files is then refactored: its private inner stub class declaration changes from `class MockVariableStore : IVariableStore` (or `class TrackingVariableStore : IVariableStore`) to `class MockVariableStore : StubVariableStore` (or `class TrackingVariableStore : StubVariableStore`). The 40 boilerplate method bodies are deleted. Only the methods that require non-default behavior are kept as `public override` declarations.

**Subclass overrides required:**
- `GameInitializationTests.MockVariableStore`: No overrides needed. The base class safe defaults for all 40 methods are functionally equivalent to the current stub (`GetCharacterFlag`/`GetTalent` return `Ok(0)`, setters are no-op). Currently throws NIE on `GetFlag`/`GetTFlag` etc., but those are never called by the tests, so changing to safe defaults is invisible.
- `HeadlessIntegrationTests.MockVariableStore`: Identical to `GameInitializationTests` — no overrides needed.
- `NtrInitializationTests.TrackingVariableStore`: Overrides `GetCharacterFlag` and `SetCharacterFlag` with Dictionary-backed behavior (current lines 31-35). `SeedCflag`/`ReadCflag` helper methods and the `_cflags` field remain in the subclass. `GetTalent`/`SetTalent` already return `Ok(0)`/no-op in the current stub, matching base defaults — no override needed.
- `StateSettingsTests.MockVariableStore`: Overrides `GetCharacterFlag`, `SetCharacterFlag`, `GetTalent`, `SetTalent` with ConcurrentDictionary-backed behavior (current lines 53-73). `SetCFlag`/`GetCFlag`/`SetTALENT` helpers and the `_store` field remain in the subclass.

This approach satisfies all 19 ACs: AC#1 (class exists), AC#2 (virtual methods), AC#3-6 (each file inherits), AC#7 (safe Result<int> defaults), AC#8-11 (no direct IVariableStore in the 4 files), AC#12 (no NotImplementedException in StubVariableStore — safe-default strategy), AC#13 (zero test regression — custom overrides preserve all test-observable behavior), AC#14 (no production code changes in Era.Core/), AC#15 (feature-816.md DRAFT created), AC#16 (F816 registered in index-features.md), AC#17 (1D getter safe defaults — C4 gap-fill), AC#18 (no StubVariableStore in engine/ source — C10 gap-fill), AC#19 (namespace consistency — C9).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `internal class StubVariableStore : IVariableStore` declaration to `TestStubs.cs` |
| 2 | Declare all 40 IVariableStore methods with `public virtual` keyword in `StubVariableStore` |
| 3 | Change `GameInitializationTests` inner class declaration to `: StubVariableStore` |
| 4 | Change `HeadlessIntegrationTests` inner class declaration to `: StubVariableStore` |
| 5 | Change `NtrInitializationTests` inner class declaration to `: StubVariableStore`; keep `GetCharacterFlag`/`SetCharacterFlag` overrides and helpers |
| 6 | Change `StateSettingsTests` inner class declaration to `: StubVariableStore`; keep 4 overrides and 3 helper methods |
| 7 | Implement 2D-array getter methods in `StubVariableStore` as `public virtual Result<int> ... => Result<int>.Ok(0)` |
| 8 | Remove `class MockVariableStore : IVariableStore` from `GameInitializationTests.cs` — satisfied by AC#3 change |
| 9 | Remove `class MockVariableStore : IVariableStore` from `HeadlessIntegrationTests.cs` — satisfied by AC#4 change |
| 10 | Remove `class TrackingVariableStore : IVariableStore` from `NtrInitializationTests.cs` — satisfied by AC#5 change |
| 11 | Remove `class MockVariableStore : IVariableStore` from `StateSettingsTests.cs` — satisfied by AC#6 change |
| 12 | Implement all 40 StubVariableStore methods as safe return defaults (return 0 / Ok(0) / no-op); do not throw NotImplementedException in any method |
| 13 | All overrides preserve test-observable behavior identically; `dotnet test engine.Tests/` passes with same test count |
| 14 | `StubVariableStore` is in `engine.Tests/` only; no edits to `Era.Core/` or `engine/` source files |
| 15 | Task#7 creates `pm/features/feature-816.md` [DRAFT] for Era.Core.Tests StubVariableStore extraction |
| 16 | Task#7 registers F816 in `pm/index-features.md` Active Features table |
| 17 | Verify 1D getter methods in `StubVariableStore` return `int` with safe default `0` (C4 gap-fill for AC#7) |
| 18 | Verify `StubVariableStore` does not appear in `engine/` production source (C10 gap-fill for AC#14) |
| 19 | Verify `StubVariableStore` is declared within the `MinorShift.Emuera.Tests` namespace in TestStubs.cs (C9 constraint) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Default return for 1D getters (`GetFlag`, `GetTFlag`) | A: return 0, B: throw NIE | A: return 0 | Matches C4 constraint and StubEngineVariables precedent. Current stubs throw NIE but no test calls these methods, so changing to `0` is invisible and eliminates future build failures |
| Default return for 2D getters (`GetCharacterFlag`, etc.) | A: return `Result<int>.Ok(0)`, B: throw NIE | A: return `Result<int>.Ok(0)` | Matches C4 constraint; current stubs that DO use these methods (GameInit/Headless) already return `Ok(0)`. Safe default matches expected pattern |
| `virtual` vs non-virtual base methods | A: all methods `virtual`, B: only overridden methods `virtual` | A: all `virtual` | C2 constraint requires virtual. Making all methods virtual is future-proof: any test can override any method without modifying `StubVariableStore` |
| `sealed` on `TrackingVariableStore` | A: keep `sealed`, B: remove `sealed` | A: keep `sealed` | `sealed` on a subclass of a non-sealed base is valid C#. Preserves original design intent from NtrInitializationTests |
| `GetTalent`/`SetTalent` override in `NtrInitializationTests` | A: override (already returns Ok(0)/no-op), B: rely on base | B: rely on base default | Current `NtrInitializationTests` already returns `Ok(0)` for `GetTalent` and no-op for `SetTalent` — identical to base defaults. No override needed; eliminates unnecessary code |
| Helper method placement | A: helpers in subclasses, B: helpers in base | A: helpers in subclasses | `SetCFlag`/`GetCFlag`/`SetTALENT` (StateSettingsTests) and `SeedCflag`/`ReadCflag` (NtrInitializationTests) are test-specific conveniences. They belong in the subclass alongside the backing store field they reference |
| Where to place `StubVariableStore` in `TestStubs.cs` | A: after `StubVisitorVariables` (before `StubEngineVariables`), B: after `StubEngineVariables` | B: after `StubEngineVariables` at end of file | Maintains file growth order and groups with most recent stub. Either position satisfies AC#1 and AC#12 |

### Interfaces / Data Structures

`StubVariableStore` implements `IVariableStore` (`Era.Core/Interfaces/IVariableStore.cs`). All 40 methods are verified against the interface definition. No new interfaces or types are introduced. The class is `internal` (engine.Tests-scoped) and does not affect any production assembly.

**Method signature reference for StubVariableStore (all 40 methods):**

```csharp
// 1D array getters — return int
public virtual int GetFlag(FlagIndex index) => 0;
public virtual int GetTFlag(FlagIndex index) => 0;

// 1D array setters — no-op
public virtual void SetFlag(FlagIndex index, int value) { }
public virtual void SetTFlag(FlagIndex index, int value) { }

// Borderline 1D: GetPalamLv/SetPalamLv
public virtual Result<int> GetPalamLv(int index) => Result<int>.Ok(0);
public virtual void SetPalamLv(int index, int value) { }

// 2D character-scoped getters — return Result<int>.Ok(0)
public virtual Result<int> GetCharacterFlag(CharacterId character, CharacterFlagIndex flag) => Result<int>.Ok(0);
public virtual Result<int> GetAbility(CharacterId character, AbilityIndex ability) => Result<int>.Ok(0);
public virtual Result<int> GetTalent(CharacterId character, TalentIndex talent) => Result<int>.Ok(0);
public virtual Result<int> GetPalam(CharacterId character, PalamIndex index) => Result<int>.Ok(0);
public virtual Result<int> GetExp(CharacterId character, ExpIndex index) => Result<int>.Ok(0);
public virtual Result<int> GetBase(CharacterId character, BaseIndex index) => Result<int>.Ok(0);
public virtual Result<int> GetTCVar(CharacterId character, TCVarIndex index) => Result<int>.Ok(0);
public virtual Result<int> GetSource(CharacterId character, SourceIndex index) => Result<int>.Ok(0);
public virtual Result<int> GetMark(CharacterId character, MarkIndex index) => Result<int>.Ok(0);
public virtual Result<int> GetNowEx(CharacterId character, NowExIndex index) => Result<int>.Ok(0);
public virtual Result<int> GetMaxBase(CharacterId character, MaxBaseIndex index) => Result<int>.Ok(0);
public virtual Result<int> GetCup(CharacterId character, CupIndex index) => Result<int>.Ok(0);
public virtual Result<int> GetJuel(CharacterId character, int index) => Result<int>.Ok(0);
public virtual Result<int> GetGotJuel(CharacterId character, int index) => Result<int>.Ok(0);
public virtual Result<int> GetStain(CharacterId character, StainIndex index) => Result<int>.Ok(0);
public virtual Result<int> GetDownbase(CharacterId character, DownbaseIndex index) => Result<int>.Ok(0);
public virtual Result<int> GetEquip(CharacterId character, int index) => Result<int>.Ok(0);

// 2D character-scoped setters — no-op
public virtual void SetCharacterFlag(CharacterId character, CharacterFlagIndex flag, int value) { }
public virtual void SetAbility(CharacterId character, AbilityIndex ability, int value) { }
public virtual void SetTalent(CharacterId character, TalentIndex talent, int value) { }
public virtual void SetPalam(CharacterId character, PalamIndex index, int value) { }
public virtual void SetExp(CharacterId character, ExpIndex index, int value) { }
public virtual void SetBase(CharacterId character, BaseIndex index, int value) { }
public virtual void SetTCVar(CharacterId character, TCVarIndex index, int value) { }
public virtual void SetSource(CharacterId character, SourceIndex index, int value) { }
public virtual void SetMark(CharacterId character, MarkIndex index, int value) { }
public virtual void SetNowEx(CharacterId character, NowExIndex index, int value) { }
public virtual void SetMaxBase(CharacterId character, MaxBaseIndex index, int value) { }
public virtual void SetCup(CharacterId character, CupIndex index, int value) { }
public virtual void SetJuel(CharacterId character, int index, int value) { }
public virtual void SetGotJuel(CharacterId character, int index, int value) { }
public virtual void SetStain(CharacterId character, StainIndex index, int value) { }
public virtual void SetDownbase(CharacterId character, DownbaseIndex index, int value) { }
public virtual void SetEquip(CharacterId character, int index, int value) { }
```

Method count: 6 (1D) + 17 (2D getters) + 17 (2D setters) = 40. Matches IVariableStore interface exactly.

### Upstream Issues

<!-- Optional: Issues discovered during Technical Design that require upstream changes (AC gaps, constraint gaps, interface API gaps).
     Orchestrator reads this section after Phase 4 and dispatches micro-revisions if needed. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 2, 7, 12, 17, 19 | Add `internal class StubVariableStore : IVariableStore` to `engine.Tests/Tests/TestStubs.cs` inside the existing `MinorShift.Emuera.Tests` namespace; declare all 40 IVariableStore methods as `public virtual` with safe defaults (1D getters return `0`, no-op setters, 2D getters return `Result<int>.Ok(0)`) following the StubEngineVariables precedent | | [x] |
| 2 | 3, 8 | Refactor `GameInitializationTests.cs` inner `MockVariableStore` class declaration from `: IVariableStore` to `: StubVariableStore`; delete all 40 boilerplate method bodies (no overrides needed) | | [x] |
| 3 | 4, 9 | Refactor `HeadlessIntegrationTests.cs` inner `MockVariableStore` class declaration from `: IVariableStore` to `: StubVariableStore`; delete all 40 boilerplate method bodies (no overrides needed) | | [x] |
| 4 | 5, 10 | Refactor `NtrInitializationTests.cs` inner `TrackingVariableStore` class declaration from `: IVariableStore` to `: StubVariableStore`; delete all 40 boilerplate method bodies; keep `GetCharacterFlag`/`SetCharacterFlag` overrides and `SeedCflag`/`ReadCflag` helpers and `_cflags` field | | [x] |
| 5 | 6, 11 | Refactor `StateSettingsTests.cs` inner `MockVariableStore` class declaration from `: IVariableStore` to `: StubVariableStore`; delete all 40 boilerplate method bodies; keep `GetCharacterFlag`/`SetCharacterFlag`/`GetTalent`/`SetTalent` overrides and `SetCFlag`/`GetCFlag`/`SetTALENT` helpers and `_store` field | | [x] |
| 6 | 13, 14, 18 | Run `dotnet test engine.Tests/` via WSL and verify all tests pass with zero failures; confirm no changes were made to production code (`Era.Core/`, `engine/`) | | [x] |
| 7 | 15, 16 | Create `pm/features/feature-816.md` [DRAFT] for Era.Core.Tests StubVariableStore extraction (26 IVariableStore stubs with same duplication problem); register in `index-features.md` and update Next Feature number to 817 | | [x] |

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
| 1 | implementer | sonnet | feature-815.md Tasks 1-5, Technical Design "Interfaces / Data Structures" method list | `engine.Tests/Tests/TestStubs.cs` with `StubVariableStore` added; 4 test files refactored to inherit from `StubVariableStore` |
| 2 | ac-tester | sonnet | feature-815.md AC#1-19, modified files from Phase 1 | AC verification results; all 19 ACs PASS |
| 3 | implementer | sonnet | feature-815.md Task 7 (Mandatory Handoff creation) | `pm/features/feature-816.md` [DRAFT] created; `index-features.md` updated with F816 entry and Next Feature number = 817 |

### Pre-conditions

- F801 is [DONE] (Predecessor; GetEquip/SetEquip already added to IVariableStore)
- `engine.Tests/Tests/TestStubs.cs` contains `StubEngineVariables` at the end of the file (reference pattern)
- `Era.Core/Interfaces/IVariableStore.cs` has exactly 40 methods (20 getter/setter pairs)

### Execution Order

1. **Task 1** (TestStubs.cs): Read `engine.Tests/Tests/TestStubs.cs` to understand file structure; append `StubVariableStore` class after `StubEngineVariables` within the existing namespace block. Use the exact 40-method signature list from Technical Design "Interfaces / Data Structures" section.
2. **Tasks 2-3** (GameInitializationTests, HeadlessIntegrationTests): These are byte-identical stubs — change `: IVariableStore` to `: StubVariableStore`; remove all 40 method bodies. No overrides needed.
3. **Task 4** (NtrInitializationTests): Change `: IVariableStore` to `: StubVariableStore`; remove 40 boilerplate method bodies; retain `GetCharacterFlag`/`SetCharacterFlag` override methods, `_cflags` Dictionary field, and `SeedCflag`/`ReadCflag` helper methods.
4. **Task 5** (StateSettingsTests): Change `: IVariableStore` to `: StubVariableStore`; remove 40 boilerplate method bodies; retain `GetCharacterFlag`/`SetCharacterFlag`/`GetTalent`/`SetTalent` override methods, `_store` ConcurrentDictionary field, and `SetCFlag`/`GetCFlag`/`SetTALENT` helper methods.
5. **Task 6** (Test run): Run `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test engine.Tests/uEmuera.Tests.csproj'` and confirm exit code 0, all tests pass.
6. **Task 7** (Handoff DRAFT): Create `pm/features/feature-816.md` with [DRAFT] status describing Era.Core.Tests StubVariableStore extraction. Register in `index-features.md` and set Next Feature number to 817.

### Build Verification Steps

After Task 1-5 (before Task 6):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build engine.Tests/uEmuera.Tests.csproj'
```
Expected: Exit code 0. If non-zero: STOP → fix CS0535 or other compiler errors before proceeding.

Full test run (Task 6):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test engine.Tests/uEmuera.Tests.csproj'
```
Expected: Exit code 0, all tests pass.

### Success Criteria

- `engine.Tests/Tests/TestStubs.cs` contains `internal class StubVariableStore : IVariableStore` with 40 `public virtual` methods
- 4 test files each have `class Mock/TrackingVariableStore : StubVariableStore` (no `: IVariableStore` remaining)
- `dotnet test engine.Tests/` exits 0 with same test count as baseline
- No changes to `Era.Core/` or `engine/` source files
- `pm/features/feature-816.md` created with [DRAFT] status; `index-features.md` updated

### Rollback Plan

If refactoring causes unexpected test failures:
1. `git revert` the implementation commit
2. Report to user with specific error output
3. Create follow-up issue in feature-816.md or a new feature describing the failure root cause

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Era.Core.Tests has 26 IVariableStore stubs with the same duplication problem | Out of scope for F815 (engine.Tests only); same N-file update problem at 26x scale | Feature | F816 | Task#7 |

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists -> OK (file created during /run)
- Option B: Referenced Feature exists -> OK
- Option C: Phase exists in architecture.md -> OK
- Missing Task for Option A -> FL FAIL
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
| 2026-02-23 | DEVIATION | implementer | Tasks 1-5 | BLOCKED by TDD protection hook (pre-tdd-protection.ps1); hook temporarily bypassed with user approval |
| 2026-02-23 | START | implementer | Tasks 1-5 | - |
| 2026-02-23 | END | implementer | Tasks 1-5 | SUCCESS (build 0W 0E) |
| 2026-02-23 | END | ac-tester | Task 6 | SUCCESS (586 passed, 0 failed) |
| 2026-02-23 | START | implementer | Task 7 | - |
| 2026-02-23 | END | implementer | Task 7 | SUCCESS |
| 2026-02-23 | DEVIATION | Bash | ac_ops.py ac-check 815 | exit 1: Review Notes line 676 contained stale AC count "All 16 ACs"; fixed to avoid lint match |

---

<!-- fc-phase-6-completed -->

## Review Notes

<!-- FL persist_pending entries will be recorded here -->
- [fix] Phase2-Review iter1: feature-815.md:51-53 (Attempted Solutions table) | Template Result column expects FAIL/PARTIAL, not PASS
- [fix] Phase2-Review iter1: feature-815.md:629 (Execution Log section) | Blank line between section header and table
- [fix] Phase2-Review iter1: AC#7 matcher | AC#7 only covers Result<int> methods; added AC#17 for 1D getter safe defaults
- [fix] Phase2-Review iter1: AC#2 matcher | AC#2 matches only checks >= 1; changed to count >= 40
- [fix] Phase2-Review iter1: AC#14 matcher | AC#14 only checks Era.Core/; added AC#18 for engine/ source check
- [fix] Phase2-Review iter2: Implementation Contract Phase 2 | Said "16 ACs" but spec defines 18 ACs after iter1 additions
- [fix] Phase2-Review iter2: Philosophy Derivation table | AC#14 and AC#18 had no Philosophy derivation; added "Test infrastructure only" row
- [fix] Phase2-Review iter3: Feasibility Assessment + Technical Design | StubEngineVariables precedent falsely claimed virtual methods; corrected to scope precedent to internal visibility and safe defaults only
- [fix] Phase2-Review iter3: AC#17 Details | Added scope note clarifying zero existing virtual methods and 1D setter coverage delegation to AC#12+AC#13
- [fix] Phase2-Review iter4: Philosophy Derivation rows 1-2 | Clarified AC coverage for "1-file update" scalability claim: AC#3-6 (inheritance) + AC#8-11 (no bypass) guarantee the property
- [fix] Phase2-Review iter5: Technical Design Approach | AC count updated from 16→18 with AC#17 and AC#18 enumeration
- [fix] Phase4-ACValidation iter6: AC#2 matcher | Invalid `count` matcher changed to `gte` with Expected `40`
- [fix] Phase2-Review iter7: AC#2 Method | Added output_mode: count to Method column for gte matcher
- [fix] Phase2-Review iter7: AC#19 added | C9 namespace constraint had no AC; added AC#19 with matches namespace pattern; updated Task#1, Goal Coverage, AC Coverage, Implementation Contract

---

## Links

- [Predecessor: F801](feature-801.md) - Added GetEquip/SetEquip to IVariableStore, triggering this deviation
- [Successor: F802](feature-802.md) - Phase 21 Counter feature; will benefit from centralized stub
- [Successor: F803](feature-803.md) - Phase 21 Counter feature; will benefit from centralized stub
- [Successor: F804](feature-804.md) - Phase 21 Counter feature; will benefit from centralized stub
- [Successor: F805](feature-805.md) - Phase 21 Counter feature; will benefit from centralized stub
- [Successor: F806](feature-806.md) - Phase 21 Counter feature; will benefit from centralized stub
- [Successor: F807](feature-807.md) - Phase 21 Counter feature; will benefit from centralized stub
- [Successor: F808](feature-808.md) - Phase 21 Counter feature; will benefit from centralized stub
- [Successor: F809](feature-809.md) - Phase 21 Counter feature; will benefit from centralized stub
- [Successor: F810](feature-810.md) - Phase 21 Counter feature; will benefit from centralized stub
- [Successor: F811](feature-811.md) - Phase 21 Counter feature; will benefit from centralized stub
- [Successor: F812](feature-812.md) - Phase 21 Counter feature; will benefit from centralized stub
- [Related: F783](feature-783.md) - Phase 21 decomposition that created F802-F812
- [Related: F404](archive/feature-404.md) - IVariableStore ISP Segregation (historical interface growth context)
- [Related: F365](archive/feature-365.md) - Created GameInitializationTests MockVariableStore
- [Related: F370](archive/feature-370.md) - Created StateSettingsTests MockVariableStore
- [Related: F371](archive/feature-371.md) - Created NtrInitializationTests TrackingVariableStore
- [Related: F800](feature-800.md) - Also involved IVariableStore extension
- [Unrelated: F816](feature-816.md) - Era.Core.Tests StubVariableStore extraction (Mandatory Handoff destination)
