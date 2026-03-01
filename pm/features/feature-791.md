# Feature 791: Engine State Transitions & Entry Point Routing

## Status: [DONE]
<!-- fl-reviewed: 2026-02-15T00:00:00Z -->

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

## Type: engine

## Created: 2026-02-14

---

## Summary

Provide game mode state transition methods (BeginTrain, dialog-mode SaveGame/LoadGame) and ERB entry point routing (IFunctionRegistry adapter for procedure-style entry points like SHOW_SHOP/USERSHOP) required by Phase 20 ERB migrations.

---

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. Phase 20 ERB migrations encounter engine state transitions and entry point dispatch patterns that have no C# interface. Phase 14 delivered IFunctionRegistry for built-in math/string functions, but not for ERB procedure-style entry points.

### Problem (Current Issue)

Three engine integration gaps block full Phase 20 ERB migration:

1. **BeginTrain()**: Game mode transition from shop to training. Used by ShopSystem.UserShop() and ShopSystem.DebugEnterUfufu(). Neither IGameState nor IGameEngine provides this method. Currently throws NotSupportedException stub.

2. **Dialog-mode SaveGame()/LoadGame()**: ERB SAVEGAME/LOADGAME are parameterless dialog-mode commands (pop up slot selection UI). IGameState.SaveGame(string)/LoadGame(string) are file-path-based — signatures are incompatible. Currently throws NotSupportedException stub.

3. **IFunctionRegistry entry point routing**: IFunctionRegistry (F421) handles IBuiltInFunction (math/string functions returning Result<object>). ERB entry points (SHOW_SHOP, USERSHOP) are void/int-returning procedures — different abstraction. Need either an adapter wrapping C# methods as IBuiltInFunction, or a separate IEntryPointRegistry for procedure-style dispatch.

F774 Shop Core documented all three gaps as Mandatory Handoffs (originally misdirected to "Phase 14").

### Goal (What to Achieve)

1. Add BeginTrain() to IGameState (or new IGameModeManager interface)
2. Add dialog-mode save/load methods (parameterless, UI-interactive)
3. Create entry point routing interface infrastructure for procedure-style dispatch (actual registration deferred to engine initialization)

<!-- Sub-Feature Requirements (architecture.md): /fc時に以下を反映すること
  1. Philosophy継承: Phase 20: Equipment & Shop Systems
  2. Tasks: 負債解消 (TODO/FIXME/HACKコメント削除タスクを含む)
  3. Tasks: 等価性検証 (legacy実装との等価性テストを含む)
  4. AC: 負債ゼロ (技術負債ゼロを検証するACを含む)
-->

---

## Scope Reference

### Source Files

| File | Lines | Functions | Note |
|------|------:|-----------|------|
| Era.Core/Interfaces/IGameState.cs | 34 | 6 existing | Extend with BeginTrain, dialog Save/Load |
| Era.Core/Commands/System/GameState.cs | 29 | 6 stubs | Era.Core-side IGameState implementation (stubs) |
| engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs | 52 | 6 stubs | Engine-side IGameState implementation |
| Era.Core/Functions/IFunctionRegistry.cs | 25 | 2 existing | Adapter or new registry for entry points |
| Era.Core/Functions/IBuiltInFunction.cs | 18 | 1 existing | Expression function interface (Result<object>) |
| Era.Core/Shop/ShopSystem.cs | ~355 | ShowShop, UserShop, etc. | Contains 3 NotSupportedException stubs |
| engine/Assets/Scripts/Emuera/GameProc/Process.SystemProc.cs | ~900 | beginTrain, beginSaveGame, etc. | Engine state machine (reference) |
| engine/Assets/Scripts/Emuera/GameProc/Process.State.cs | ~320 | SetBegin, SaveLoadData | Engine state transitions (reference) |

### F774 Mandatory Handoff Origin

| Issue | Location |
|-------|----------|
| BeginTrain() method | ShopSystem.cs:338-339 NotSupportedException stub |
| Dialog-mode SaveGame()/LoadGame() | ShopSystem.cs:341-345 NotSupportedException stubs |
| IFunctionRegistry registration for SHOW_SHOP/USERSHOP | ShopSystem.cs:39 (ShowShop), :78 (UserShop) entry point methods |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (parent) |
| Predecessor | F774 | [DONE] | Shop Core (Mandatory Handoff origin) |
| Successor | F782 | [DRAFT] | Post-Phase Review depends on F791 |
| Successor | F793 | [DONE] | GameStateImpl Engine-Side Delegation |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} → This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This → F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block
-->

---

<!-- fc-phase-2-completed -->
## Investigation (Consensus of 3 Investigators)

### Root Cause Analysis

#### Hypothesis 1 (PRIMARY) -- Confidence: HIGH (3/3 agree)

**Statement**: Three engine integration gaps exist because Era.Core interfaces were designed incrementally per-phase. IGameState (Phase 9) covered file-path-based save/load only. IFunctionRegistry (Phase 14) covered math/string expression functions only. Phase 20 is the first phase to migrate ERB code (SHOP.ERB) that uses engine state machine transitions (BEGIN TRAIN), dialog-mode UI commands (SAVEGAME/LOADGAME), and procedure-style entry point dispatch (SHOW_SHOP/USERSHOP). The gap is structural and by design (incremental extension architecture).

**Why Chain**:
1. ShopSystem.cs:338-345 has 3 NotSupportedException stubs for BeginTrain(), SaveGame(), LoadGame()
2. These operations require engine-level state transitions: BEGIN TRAIN sets SystemStateCode.Train_Begin; SAVEGAME/LOADGAME set SaveGame_Begin/LoadGame_Begin (Process.State.cs:23,47,53,57)
3. IGameState was designed in Phase 9 (F377/F434) with only file-path-based SaveGame(string)/LoadGame(string) -- no mode transitions
4. The engine's game mode transition is a complex state machine (SystemStateCode enum with 30+ states) never abstracted into Era.Core
5. Root: Architecture follows incremental interface extension. Phase 20 is first to need state transition commands

#### Hypothesis 2 (SECONDARY) -- Confidence: HIGH (1) / MEDIUM (2)

**Statement**: IFunctionRegistry/IBuiltInFunction was designed exclusively for value-returning expression functions (Result<object>). SHOW_SHOP (void) and USERSHOP (int) are procedure-style entry points using a fundamentally different engine dispatch mechanism (callFunction() via script processor, not function registry). These two dispatch patterns serve different purposes and require separate abstractions.

**Why Chain**:
1. F774 documented "IFunctionRegistry registration for SHOW_SHOP/USERSHOP" as a Mandatory Handoff gap
2. IBuiltInFunction.Execute() returns Result<object> with IEvaluationContext -- designed for ABS, RAND, TOSTR
3. Engine's callFunction("SHOW_SHOP") dispatches to ERB entry points as void procedures, not through function infrastructure (Process.SystemProc.cs:686,738)
4. F421 was scoped to Phase 14 "Expression & Function System" for 100+ math/string built-in functions
5. Root: "Functions" (math/string value-returning) and "entry points" (ERB procedure dispatch) are entirely separate mechanisms in the legacy engine. Only the former was abstracted.

### Evidence Summary

| # | Type | Location | Finding |
|:-:|------|----------|---------|
| 1 | Stub | ShopSystem.cs:338-339 | BeginTrain() throws NotSupportedException |
| 2 | Stub | ShopSystem.cs:341-342 | SaveGame() throws NotSupportedException |
| 3 | Stub | ShopSystem.cs:344-345 | LoadGame() throws NotSupportedException |
| 4 | Call site | ShopSystem.cs:86,333 | BeginTrain() called from UserShop() and DebugEnterUfufu() |
| 5 | Call site | ShopSystem.cs:107,111 | SaveGame()/LoadGame() called from UserShop() |
| 6 | Interface | IGameState.cs:14-17 | SaveGame(string)/LoadGame(string) require filename |
| 7 | Interface | IGameState.cs:11-33 | No BeginTrain() method exists |
| 8 | Stub impl | GameState.cs:10-29 | Era.Core-side: all methods return Result.Fail |
| 9 | Stub impl | GameStateImpl.cs:14-21 | Engine-side: all methods return Result.Fail |
| 10 | Engine | Process.State.cs:182-201 | SetBegin(keyword) handles TRAIN, SHOP state transitions |
| 11 | Engine | Process.State.cs:236-244 | SaveLoadData() sets SystemStateCode |
| 12 | Engine | Process.SystemProc.cs:250-260 | beginTrain() calls UpdateInBeginTrain() |
| 13 | Engine | Process.SystemProc.cs:814-833 | Save/load dialog with slot selection UI |
| 14 | Engine | Instraction.Child.cs:1727-1748 | SAVEGAME/LOADGAME parameterless, checks __CAN_SAVE__ |
| 15 | Engine | VariableEvaluator.cs:1564-1603 | UpdateInBeginTrain() resets 8+ arrays (TFLAG, TSTR, GOTJUEL, TEQUIP, EX, STAIN, PALAM, SOURCE, TCVAR) |
| 16 | Interface | IBuiltInFunction.cs:17 | Execute returns Result<object> |
| 17 | Interface | IFunctionRegistry.cs:17-24 | GetFunction returns Result<IBuiltInFunction> |
| 18 | Engine | Process.SystemProc.cs:686,738 | callFunction("SHOW_SHOP"/"USERSHOP") |
| 19 | Handoff | feature-774.md:928-930 | F774 Mandatory Handoff rows for all 3 gaps |

### Affected Files

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core/Interfaces/IGameState.cs | MODIFY | Add BeginTrain(), dialog-mode SaveGameDialog()/LoadGameDialog() |
| Era.Core/Commands/System/GameState.cs | MODIFY | Stub implementations for new IGameState methods |
| engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs | MODIFY | Engine-side implementations for new IGameState methods |
| Era.Core/Shop/ShopSystem.cs | MODIFY | Replace 3 NotSupportedException stubs with IGameState calls |
| NEW: Era.Core/Functions/IEntryPointRegistry.cs (or similar) | CREATE | Procedure-style entry point routing interface |
| NEW: Entry point implementation | CREATE | Concrete IEntryPointRegistry implementation |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | MODIFY | DI registration for new interfaces |
| NEW: Era.Core.Tests/ test files | CREATE | Unit tests (TDD) |

### Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|-------|
| Gap 1: BeginTrain() | FEASIBLE | Straightforward interface method addition |
| Gap 2: Dialog-mode Save/Load | FEASIBLE | Design decision needed: two paths (extend IGameState vs new interface); thin delegation over 14-state sub-machine recommended |
| Gap 3: Entry point routing | FEASIBLE | Design decision needed: three approaches (adapter wrapping IBuiltInFunction, separate IEntryPointRegistry, or direct method dispatch) |
| Test infrastructure | FEASIBLE | Existing patterns in Era.Core.Tests |
| DI registration | FEASIBLE | ServiceCollectionExtensions.cs precedent |
| Breaking change risk | FEASIBLE | Adding methods is non-breaking for consumers |
| Dual IGameState implementation | FEASIBLE | Both GameState.cs and GameStateImpl.cs follow same stub pattern |

**Verdict**: FEASIBLE

### Impact Analysis

| Area | Impact | Description |
|------|--------|-------------|
| IGameState consumers | LOW | Adding methods is non-breaking; existing 6 methods unchanged |
| Era.Core DI container | LOW | One new singleton registration (IEntryPointRegistry) |
| ShopSystem | HIGH | 3 NotSupportedException stubs replaced with IGameState calls |
| Engine GameStateImpl | MEDIUM | 3 new method implementations required (initially stubs) |
| IFunctionRegistry | NONE | Untouched; separate IEntryPointRegistry avoids modification |

### Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| IGameState has 2 implementations (GameState + GameStateImpl) | Both files | Both must implement new methods |
| IGameState uses Result<Unit> convention | IGameState.cs:14-25 | New methods must follow same pattern |
| Dialog-mode save/load must be parameterless | Instraction.Child.cs:1731 | Different signature from SaveGame(string) |
| SAVEGAME/LOADGAME require __CAN_SAVE__ flag | Instraction.Child.cs:1738-1743 | Constraint must be preserved |
| BeginTrain() resets 8+ variable arrays in engine | VariableEvaluator.cs:1564-1603 | May interact with F789 IVariableStore |
| SAVEGAME/LOADGAME trigger 14-state sub-machine | Process.State.cs:53-60 | Full reimplementation complex; thin delegation preferred |
| IBuiltInFunction.Execute returns Result<object> | IBuiltInFunction.cs:17 | Incompatible with void/int procedures |
| IFunctionRegistry uses ConcurrentDictionary, OrdinalIgnoreCase | FunctionRegistry.cs:17 | New registry should follow same patterns |
| TreatWarningsAsErrors=true | Directory.Build.props | All code must compile warning-free with XML docs |

### Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Dialog Save/Load scope creep (14-state sub-machine) | HIGH | HIGH | Thin delegation approach; defer complex flow to engine host |
| BeginTrain variable reset completeness | MEDIUM | MEDIUM | Equivalence test against VariableEvaluator.UpdateInBeginTrain() |
| Entry point routing over-engineering | MEDIUM | LOW | Choose simplest viable approach |
| Interface proliferation | MEDIUM | MEDIUM | ISP-guided grouping; justify each new interface |
| GameState + GameStateImpl dual update drift | LOW | MEDIUM | Atomic updates to both files |

### Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Era.Core build warnings | `dotnet build Era.Core` | 0 | TreatWarningsAsErrors=true |
| Era.Core.Tests pass count | `dotnet test Era.Core.Tests` | All pass | No regressions allowed |
| NotSupportedException count in ShopSystem | `Grep NotSupportedException ShopSystem.cs` | 3 | Must become 0 |

**Baseline File**: `.tmp/baseline-791.txt`

### Design Decisions Needed (for /fc)

| # | Decision | Options | Recommendation |
|:-:|----------|---------|----------------|
| 1 | Where to add BeginTrain() | (a) Extend IGameState, (b) New IGameModeManager | Extend IGameState -- consistent with existing pattern, avoids interface proliferation |
| 2 | Dialog-mode save/load approach | (a) Overload on IGameState, (b) New parameterless methods on IGameState, (c) New interface | New parameterless methods on IGameState (SaveGameDialog/LoadGameDialog) -- avoids overload ambiguity |
| 3 | Entry point routing mechanism | (a) Adapter wrapping IBuiltInFunction, (b) Separate IEntryPointRegistry, (c) Direct method dispatch | Separate IEntryPointRegistry -- cleanest separation of concerns, avoids forcing procedural semantics into expression-function interface |

### AC Design Constraints

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | BeginTrain() callable from ShopSystem without exceptions | ShopSystem.cs:86,333 | Verify invocability, no NotSupportedException |
| C2 | Dialog-mode save/load parameterless | Instraction.Child.cs:1731 | No filename parameter; SaveGame(string) unchanged |
| C3 | Entry point routing by string name | Process.SystemProc.cs:686,738 | SHOW_SHOP -> ShowShop(), USERSHOP -> UserShop() |
| C4 | IGameState backward compatibility | IGameState.cs:11-33 | All 6 existing methods still function |
| C5 | BeginTrain variable reset equivalence | VariableEvaluator.cs:1564-1603 | Deferred to engine integration (D5: GameStateImpl delegates to UpdateInBeginTrain()). Era.Core stub returns Fail. |
| C6 | DI registration for new interfaces | ServiceCollectionExtensions.cs | All new interfaces DI-registered and resolvable |
| C7 | All 3 NotSupportedException stubs replaced | ShopSystem.cs:338-345 | Zero NotSupportedException in ShopSystem |
| C8 | __CAN_SAVE__ constraint preserved | Instraction.Child.cs:1738 | Deferred to engine integration (D4: thin delegation to Process.SaveLoadData()). Engine handles __CAN_SAVE__ internally. |
| C9 | Result<Unit> return type convention | IGameState.cs pattern | New methods follow existing convention |
| C10 | Zero technical debt | Architecture sub-feature requirement | No TODO/FIXME/HACK in new code |
| C11 | TreatWarningsAsErrors XML docs | Directory.Build.props | Build succeeds with no warnings |

### Constraint Details

**C1: BeginTrain() callable from ShopSystem without exceptions**
- **Source**: ShopSystem.cs:86,333 call sites throw NotSupportedException
- **Verification**: Grep ShopSystem.cs for NotSupportedException (expect 0)
- **AC Impact**: AC#1 (interface method exists), AC#10 (call site uses IGameState), AC#7 (no exceptions)

**C2: Dialog-mode save/load parameterless**
- **Source**: Instraction.Child.cs:1731 SAVEGAME/LOADGAME have no arguments
- **Verification**: IGameState method signatures have no parameters
- **AC Impact**: AC#2, AC#3 (parameterless method signatures)

**C3: Entry point routing by string name**
- **Source**: Process.SystemProc.cs:686,738 callFunction("SHOW_SHOP"/"USERSHOP")
- **Verification**: IEntryPointRegistry.Invoke accepts string name parameter
- **AC Impact**: AC#5, AC#6a (Invoke with Result<int>), AC#6b (Register), AC#16 (round-trip test)

**C4: IGameState backward compatibility**
- **Source**: IGameState.cs:11-33 existing 6 methods
- **Verification**: All existing methods unchanged in interface and implementations
- **AC Impact**: AC#1-3 add new methods without modifying existing ones; AC#19 verifies all 6 existing methods preserved

**C5: BeginTrain variable reset equivalence**
- **Source**: VariableEvaluator.cs:1564-1603 resets TFLAG, TSTR, GOTJUEL, TEQUIP, EX, STAIN, PALAM, SOURCE, TCVAR
- **Verification**: Deferred to engine integration (D5: GameStateImpl delegates to UpdateInBeginTrain())
- **AC Impact**: No direct AC; Era.Core stub returns Fail. Engine-side equivalence verified at integration time.

**C6: DI registration for new interfaces**
- **Source**: ServiceCollectionExtensions.cs existing registration pattern
- **Verification**: Grep ServiceCollectionExtensions.cs for IEntryPointRegistry
- **AC Impact**: AC#9 (DI registration present)

**C7: All 3 NotSupportedException stubs replaced**
- **Source**: ShopSystem.cs:338-345 three stub methods
- **Verification**: Grep ShopSystem.cs for NotSupportedException (expect 0)
- **AC Impact**: AC#7 (not_contains NotSupportedException)

**C8: __CAN_SAVE__ constraint preserved**
- **Source**: Instraction.Child.cs:1738 checks __CAN_SAVE__ before allowing save/load
- **Verification**: Deferred to engine integration (D4: thin delegation to Process.SaveLoadData())
- **AC Impact**: No direct AC; engine handles __CAN_SAVE__ internally via Process state machine.

**C9: Result<Unit> return type convention**
- **Source**: IGameState.cs pattern (all methods return Result<Unit>)
- **Verification**: New method signatures match Result<Unit> return type
- **AC Impact**: AC#1, AC#2, AC#3 (Grep patterns verify Result<Unit> return type)

**C10: Zero technical debt**
- **Source**: Architecture sub-feature requirement (CLAUDE.md Zero Debt Upfront)
- **Verification**: Grep new/modified files for TODO/FIXME/HACK
- **AC Impact**: AC#15 (not_matches TODO|FIXME|HACK)

**C11: TreatWarningsAsErrors XML docs**
- **Source**: Directory.Build.props TreatWarningsAsErrors=true
- **Verification**: dotnet build Era.Core succeeds with 0 warnings
- **AC Impact**: AC#13 (build succeeds with zero warnings)

### Interface Dependency Scan

| Interface | File | Method Needed | Exists? | Gap Description |
|-----------|------|---------------|:-------:|-----------------|
| IGameState | IGameState.cs | BeginTrain() | NO | No mode transition method |
| IGameState | IGameState.cs | SaveGameDialog() | NO | Only file-path SaveGame(string) |
| IGameState | IGameState.cs | LoadGameDialog() | NO | Only file-path LoadGame(string) |
| IFunctionRegistry | IFunctionRegistry.cs | Register procedure-style | NO | Only IBuiltInFunction (Result<object>) |
| IBuiltInFunction | IBuiltInFunction.cs | Procedure Execute | NO | Only Result<object> return |
| IVariableStore | IVariableStore.cs | Batch reset for training | PARTIAL | Individual Set methods exist, no batch ResetForTraining() |
| IGameEngine | IGameEngine.cs | State machine transition | NO | Only ProcessTurn/Initialize |

### Sibling Feature Call Chain Analysis

No hard call chain dependencies. F791 is pure interface infrastructure. BEGIN TRAIN, SAVEGAME, LOADGAME appear only in SHOP.ERB (F774 scope). No sibling features (F775-F781, F788-F790) require these capabilities.

### Related Features

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Predecessor -- Phase 20 Planning parent |
| F774 | [DONE] | Predecessor -- Mandatory Handoff origin |
| F421 | [DONE] (archived) | Related -- IFunctionRegistry design reference |
| F788 | [DONE] | Related -- IConsoleOutput Extensions (parallel Phase 20 infra) |
| F789 | [DONE] | Related -- IVariableStore Extensions (parallel; no dependency — D5 delegates BeginTrain reset to engine UpdateInBeginTrain() directly) |
| F790 | [DONE] | Related -- Engine Data Access Layer (parallel Phase 20 infra) |
| F782 | [DRAFT] | Successor -- Post-Phase Review depends on F791 |

### Discrepancies Found

| # | Issue | Resolution |
|:-:|-------|------------|
| 1 | Scope Reference line numbers were stale (ShopSystem.cs:614, :860-865) | FIXED: Updated to actual lines (338-339, 341-345) |
| 2 | Scope Reference engine path was wrong (Scripts/Process/) | FIXED: Updated to Scripts/Emuera/GameProc/ |
| 3 | index-features.md shows Depends On: F647 only; feature file has both F647 and F774 | FIXED: Added F774 to index-features.md |
| 4 | NotSupportedException messages say "Phase 14 Mandatory Handoff" | Cosmetically incorrect (should be Phase 20); low priority, will be fixed when stubs are replaced |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "have no C# interface" (state transitions) | IGameState must be extended with BeginTrain(), SaveGameDialog(), LoadGameDialog() methods; both Era.Core and engine implementations updated; existing methods preserved | AC#1, AC#2, AC#3, AC#4, AC#8, AC#17, AC#18, AC#19 |
| "not for ERB procedure-style entry points" (IFunctionRegistry) | A separate IEntryPointRegistry must be created for procedure-style dispatch, distinct from IBuiltInFunction; DI-registered; IFunctionRegistry must remain unmodified | AC#5, AC#6a, AC#6b, AC#9, AC#16, AC#20 |
| "Each phase completion triggers next phase planning" (Pipeline Continuity) | All 3 NotSupportedException stubs in ShopSystem must be replaced with working IGameState calls | AC#7, AC#10, AC#11, AC#12 |
| "Zero Debt Upfront" (CLAUDE.md Design Principle) | New code compiles warning-free, all tests pass, zero TODO/FIXME/HACK markers, SSOT documentation updated | AC#13, AC#14, AC#15, AC#21, AC#22 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IGameState declares BeginTrain method | code | Grep(Era.Core/Interfaces/IGameState.cs) | matches | `Result<Unit>\s+BeginTrain\(\)` | [x] |
| 2 | IGameState declares SaveGameDialog method | code | Grep(Era.Core/Interfaces/IGameState.cs) | matches | `Result<Unit>\s+SaveGameDialog\(\)` | [x] |
| 3 | IGameState declares LoadGameDialog method | code | Grep(Era.Core/Interfaces/IGameState.cs) | matches | `Result<Unit>\s+LoadGameDialog\(\)` | [x] |
| 4 | GameState stub implements new methods | code | Grep(Era.Core/Commands/System/GameState.cs) | count_equals | `public Result<Unit> (BeginTrain|SaveGameDialog|LoadGameDialog)\(\)` = 3 | [x] |
| 5 | IEntryPointRegistry interface file exists | file | Glob(Era.Core/Functions/IEntryPointRegistry.cs) | exists | Era.Core/Functions/IEntryPointRegistry.cs | [x] |
| 6a | IEntryPointRegistry declares Invoke method with Result<int> | code | Grep(Era.Core/Functions/IEntryPointRegistry.cs) | matches | `Result<int>\s+Invoke\(string` | [x] |
| 6b | IEntryPointRegistry declares Register method with Func<int> handler | code | Grep(Era.Core/Functions/IEntryPointRegistry.cs) | matches | `void\s+Register\(string.*Func<int>` | [x] |
| 7 | ShopSystem has zero NotSupportedException | code | Grep(Era.Core/Shop/ShopSystem.cs) | not_contains | NotSupportedException | [x] |
| 8 | GameStateImpl implements new IGameState methods | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs) | count_equals | `public Result<Unit> (BeginTrain|SaveGameDialog|LoadGameDialog)\(\)` = 3 | [x] |
| 9 | DI registration for IEntryPointRegistry | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | contains | IEntryPointRegistry | [x] |
| 10 | ShopSystem calls IGameState.BeginTrain at both call sites | code | Grep(Era.Core/Shop/ShopSystem.cs) | count_equals | `_gameState\.BeginTrain\(\)` = 2 | [x] |
| 11 | ShopSystem calls IGameState.SaveGameDialog | code | Grep(Era.Core/Shop/ShopSystem.cs) | matches | `_gameState\.SaveGameDialog\(\)` | [x] |
| 12 | ShopSystem calls IGameState.LoadGameDialog | code | Grep(Era.Core/Shop/ShopSystem.cs) | matches | `_gameState\.LoadGameDialog\(\)` | [x] |
| 13 | Build succeeds with zero warnings | build | dotnet build Era.Core | succeeds | 0 warnings | [x] |
| 14 | Unit tests pass | test | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 15 | Zero technical debt in new/modified files | code | Grep(Era.Core/Interfaces/IGameState.cs,Era.Core/Commands/System/GameState.cs,Era.Core/Functions/IEntryPointRegistry.cs,Era.Core/Functions/EntryPointRegistry.cs,Era.Core/Shop/ShopSystem.cs,Era.Core/DependencyInjection/ServiceCollectionExtensions.cs,engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs,Era.Core.Tests/Interfaces/GameStateTests.cs,Era.Core.Tests/Functions/EntryPointRegistryTests.cs) | not_matches | `TODO|FIXME|HACK` | [x] |
| 16 | EntryPointRegistry Register+Invoke round-trip | test | dotnet test --filter EntryPointRegistryTests | succeeds | All pass (register, invoke, case-insensitive, unregistered fail, duplicate overwrites, void procedure returns 0, handler exception returns Fail, Register null/whitespace name throws ArgumentException, Register null handler throws ArgumentNullException, Invoke null/whitespace name returns Fail) | [x] |
| 17 | IGameState stub methods have unit tests | code | Grep(Era.Core.Tests/) | matches | `BeginTrain|SaveGameDialog|LoadGameDialog` | [x] |
| 18 | IGameState XML doc reflects expanded responsibility | code | Grep(Era.Core/Interfaces/IGameState.cs, -i) | matches | `(mode transition|state transition|training mode|game mode)` | [x] |
| 19 | IGameState existing methods preserved (backward compatibility) | code | Grep(Era.Core/Interfaces/IGameState.cs) | count_equals | `Result<Unit>\s+(SaveGame|LoadGame|Quit|Restart|ResetData|SetVariable)\(` = 6 | [x] |
| 20 | IFunctionRegistry unmodified (D3 separation) | code | Grep(Era.Core/Functions/IFunctionRegistry.cs) | count_equals | `(GetFunction|Register)\(` = 2 | [x] |
| 21 | engine-dev SKILL.md documents IEntryPointRegistry | code | Grep(.claude/skills/engine-dev/SKILL.md) | contains | IEntryPointRegistry | [x] |
| 22 | engine-dev SKILL.md documents IGameState new methods | code | Grep(.claude/skills/engine-dev/SKILL.md) | matches | `BeginTrain|SaveGameDialog|LoadGameDialog` | [x] |

### AC Details

**AC#1: IGameState declares BeginTrain method**
- **Test**: Grep pattern=`Result<Unit>\s+BeginTrain\(\)` path=Era.Core/Interfaces/IGameState.cs
- **Expected**: 1 match (parameterless method returning Result<Unit>)
- **Rationale**: C1 constraint -- BeginTrain() must exist on IGameState to replace the NotSupportedException stub in ShopSystem. C9 constraint -- follows Result<Unit> convention.

**AC#2: IGameState declares SaveGameDialog method**
- **Test**: Grep pattern=`Result<Unit>\s+SaveGameDialog\(\)` path=Era.Core/Interfaces/IGameState.cs
- **Expected**: 1 match (parameterless method, distinct from SaveGame(string))
- **Rationale**: C2 constraint -- dialog-mode save is parameterless (no filename argument). Named SaveGameDialog to avoid overload ambiguity with existing SaveGame(string). C4 constraint -- existing SaveGame(string) remains unchanged.

**AC#3: IGameState declares LoadGameDialog method**
- **Test**: Grep pattern=`Result<Unit>\s+LoadGameDialog\(\)` path=Era.Core/Interfaces/IGameState.cs
- **Expected**: 1 match (parameterless method, distinct from LoadGame(string))
- **Rationale**: C2 constraint -- dialog-mode load is parameterless. Named LoadGameDialog to avoid overload ambiguity with existing LoadGame(string). C4 constraint -- existing LoadGame(string) remains unchanged.

**AC#4: GameState stub implements new methods**
- **Test**: Grep pattern=`public Result<Unit> (BeginTrain|SaveGameDialog|LoadGameDialog)\(\)` path=Era.Core/Commands/System/GameState.cs type=cs | count
- **Expected**: 3 matches (one stub implementation for each new IGameState method)
- **Rationale**: Era.Core-side GameState stub must implement all new IGameState methods. Stubs return Result<Unit>.Fail per existing convention. Both implementations (GameState + GameStateImpl) must stay in sync.

**AC#5: IEntryPointRegistry interface file exists**
- **Test**: Glob pattern=Era.Core/Functions/IEntryPointRegistry.cs
- **Expected**: File exists
- **Rationale**: C3 constraint -- entry point routing by string name requires a dedicated interface. Hypothesis 2 confirms IBuiltInFunction (Result<object>) is incompatible with void/int procedure dispatch. Separate interface follows ISP.

**AC#6a: IEntryPointRegistry declares Invoke method with Result<int>**
- **Test**: Grep pattern=`Result<int>\s+Invoke\(string` path=Era.Core/Functions/IEntryPointRegistry.cs
- **Expected**: 1 match (Invoke returns Result<int>, not void or Result<object>)
- **Rationale**: C3 constraint -- SHOW_SHOP is void, USERSHOP returns int. Result<int> accommodates both (void procedures return 0). Return type verification ensures IBuiltInFunction (Result<object>) separation per Hypothesis 2.

**AC#6b: IEntryPointRegistry declares Register method with Func<int> handler**
- **Test**: Grep pattern=`void\s+Register\(string.*Func<int>` path=Era.Core/Functions/IEntryPointRegistry.cs
- **Expected**: 1 match (Register accepts string name and Func<int> handler)
- **Rationale**: Register method enables runtime registration of entry points. D6 chose Func<int> over Action and Func<int[], int> — pattern enforces handler type to prevent signature drift. Follows IFunctionRegistry pattern (ConcurrentDictionary, OrdinalIgnoreCase).

**AC#7: ShopSystem has zero NotSupportedException**
- **Test**: Grep pattern=NotSupportedException path=Era.Core/Shop/ShopSystem.cs
- **Expected**: 0 matches
- **Rationale**: C7 constraint -- all 3 NotSupportedException stubs (BeginTrain, SaveGame, LoadGame at lines 338-345) must be replaced with IGameState method calls. This is the primary deliverable of F791.

**AC#8: GameStateImpl implements new IGameState methods**
- **Test**: Grep pattern=`public Result<Unit> (BeginTrain|SaveGameDialog|LoadGameDialog)\(\)` path=engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs type=cs | count
- **Expected**: 3 matches (one implementation for each new method)
- **Rationale**: IGameState has dual implementations (GameState in Era.Core, GameStateImpl in engine). Both must implement new methods to satisfy interface contract. Engine-side implementation will eventually delegate to Process state machine.

**AC#9: DI registration for IEntryPointRegistry**
- **Test**: Grep pattern=IEntryPointRegistry path=Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- **Expected**: At least 1 match (AddSingleton registration)
- **Rationale**: C6 constraint -- all new interfaces must be DI-registered and resolvable. Follows existing pattern in ServiceCollectionExtensions.cs.

**AC#10: ShopSystem calls IGameState.BeginTrain at both call sites**
- **Test**: Grep pattern=`_gameState\.BeginTrain\(\)` path=Era.Core/Shop/ShopSystem.cs | count
- **Expected**: 2 matches (UserShop line ~86 and DebugEnterUfufu line ~333, both converted from private stub)
- **Rationale**: C1 constraint -- BeginTrain() must be callable from ShopSystem without exceptions. The private stub method is removed entirely; both call sites (UserShop and DebugEnterUfufu) must delegate to injected IGameState. count_equals=2 ensures neither call site is missed.

**AC#11: ShopSystem calls IGameState.SaveGameDialog**
- **Test**: Grep pattern=`_gameState\.SaveGameDialog\(\)` path=Era.Core/Shop/ShopSystem.cs
- **Expected**: 1 match (replacing the private SaveGame() stub in UserShop result==200 branch)
- **Rationale**: C2/C8 constraints -- dialog-mode save must be parameterless and preserve __CAN_SAVE__ semantics via engine delegation.

**AC#12: ShopSystem calls IGameState.LoadGameDialog**
- **Test**: Grep pattern=`_gameState\.LoadGameDialog\(\)` path=Era.Core/Shop/ShopSystem.cs
- **Expected**: 1 match (replacing the private LoadGame() stub in UserShop result==300 branch)
- **Rationale**: C2 constraint -- dialog-mode load must be parameterless. Delegates to engine for slot selection UI.

**AC#13: Build succeeds with zero warnings**
- **Test**: `dotnet build Era.Core` (via WSL)
- **Expected**: Build succeeds, 0 warnings
- **Rationale**: C11 constraint -- TreatWarningsAsErrors=true requires all new code to have XML documentation comments and compile warning-free.

**AC#14: Unit tests pass**
- **Test**: `dotnet test Era.Core.Tests` (via WSL)
- **Expected**: All tests pass
- **Rationale**: TDD requirement -- new interface methods and entry point registry must have unit tests. Existing tests must not regress.

**AC#15: Zero technical debt in new/modified files**
- **Test**: Grep pattern=`TODO|FIXME|HACK` paths=Era.Core/Interfaces/IGameState.cs, Era.Core/Commands/System/GameState.cs, Era.Core/Functions/IEntryPointRegistry.cs, Era.Core/Functions/EntryPointRegistry.cs, Era.Core/Shop/ShopSystem.cs, Era.Core/DependencyInjection/ServiceCollectionExtensions.cs, engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs, Era.Core.Tests/Interfaces/GameStateTests.cs, Era.Core.Tests/Functions/EntryPointRegistryTests.cs
- **Expected**: 0 matches across all listed files
- **Rationale**: C10 constraint -- zero technical debt in new code. Covers all files where F791 creates or modifies code, including test files.

**AC#16: EntryPointRegistry Register+Invoke round-trip**
- **Test**: `dotnet test Era.Core.Tests --filter EntryPointRegistryTests` (via WSL)
- **Expected**: All tests pass (register handler, invoke returns handler result, case-insensitive lookup, unregistered returns Fail, duplicate registration overwrites handler, void procedure registered with () => 0 returns Result<int>.Ok(0), handler that throws exception returns Result<int>.Fail with exception message, Register with null/whitespace name throws ArgumentException, Register with null handler throws ArgumentNullException, Invoke with null/whitespace name returns Result<int>.Fail)
- **Rationale**: Goal 3 functional verification. Structural ACs (AC#5,6a,6b,9) verify interface exists but not behavior. EntryPointRegistry is a concrete runtime class requiring behavioral tests including overwrite semantics. Void-procedure test verifies D6 convention (void procedures return 0 via Func<int>). Exception test verifies Result contract (handler errors returned as Fail, not thrown). Register name validation test verifies consistency with FunctionRegistry pattern. Register null handler test verifies ArgumentNullException guard. Invoke name validation test verifies Result<int> contract is maintained for invalid inputs (no unhandled ArgumentNullException from ConcurrentDictionary).

**AC#17: IGameState stub methods have unit tests**
- **Test**: Grep pattern=`BeginTrain|SaveGameDialog|LoadGameDialog` path=Era.Core.Tests/
- **Expected**: At least 3 matches (test methods covering each new IGameState stub)
- **Rationale**: Task 9 creates IGameState stub tests but AC#14 (all tests pass) is too generic to verify their existence. This AC ensures the specific stub test coverage exists.

**AC#18: IGameState XML doc reflects expanded responsibility**
- **Test**: Grep pattern=`(mode transition|state transition|training mode|game mode)` path=Era.Core/Interfaces/IGameState.cs flags=-i (case-insensitive)
- **Expected**: 1+ match (interface-level or method-level XML doc mentions mode/state transitions or training/game mode)
- **Rationale**: Task#1 requires updating IGameState XML doc to reflect expanded responsibility (persistence + lifecycle + mode transitions). AC#1-3 only verify method signatures; AC#13 only verifies docs exist (build gate). Broadened alternation pattern accepts semantically equivalent phrasings ("mode transition", "state transition", "training mode", "game mode") to avoid false negatives from reasonable wording variations. Case-insensitive flag handles variation in casing.

**AC#19: IGameState existing methods preserved (backward compatibility)**
- **Test**: Grep pattern=`Result<Unit>\s+(SaveGame|LoadGame|Quit|Restart|ResetData|SetVariable)\(` path=Era.Core/Interfaces/IGameState.cs | count
- **Expected**: 6 matches (all 6 existing IGameState methods remain unchanged)
- **Rationale**: C4 constraint -- IGameState backward compatibility. AC#1-3 verify new methods exist but do not verify existing methods are preserved. This AC ensures no existing method is accidentally removed or modified during interface extension. Count = 6 (SaveGame(string), LoadGame(string), Quit(), Restart(), ResetData(), SetVariable(string, int, long)).

**AC#20: IFunctionRegistry unmodified (D3 separation)**
- **Test**: Grep pattern=`(GetFunction|Register)\(` path=Era.Core/Functions/IFunctionRegistry.cs | count
- **Expected**: 2 matches (exactly 2 existing methods: GetFunction, Register)
- **Rationale**: Design Decision D3 chose separate IEntryPointRegistry specifically to avoid modifying IFunctionRegistry. Impact Analysis states "IFunctionRegistry: NONE: Untouched." This AC enforces the separation invariant. If an implementer adds methods to IFunctionRegistry (violating D3), the count would exceed 2.

**AC#21: engine-dev SKILL.md documents IEntryPointRegistry**
- **Test**: Grep pattern=IEntryPointRegistry path=.claude/skills/engine-dev/SKILL.md
- **Expected**: At least 1 match (IEntryPointRegistry documented in engine-dev SSOT)
- **Rationale**: ssot-update-rules.md requires new interfaces to be documented in relevant skills. IFunctionRegistry is already documented in engine-dev SKILL.md. IEntryPointRegistry (same Functions namespace, same registration pattern) must follow the same convention.

**AC#22: engine-dev SKILL.md documents IGameState new methods**
- **Test**: Grep pattern=`BeginTrain|SaveGameDialog|LoadGameDialog` path=.claude/skills/engine-dev/SKILL.md
- **Expected**: At least 1 match (new IGameState methods documented in engine-dev SSOT)
- **Rationale**: IGameState is documented in engine-dev SKILL.md (line 39, 336). Adding 3 new methods (BeginTrain, SaveGameDialog, LoadGameDialog) must be reflected in the SSOT to maintain documentation accuracy.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Add BeginTrain() to IGameState (or new IGameModeManager interface) | AC#1, AC#4, AC#7, AC#8, AC#10, AC#17, AC#18, AC#19, AC#22 |
| 2 | Add dialog-mode save/load methods (parameterless, UI-interactive) | AC#2, AC#3, AC#4, AC#7, AC#8, AC#11, AC#12, AC#17, AC#22 |
| 3 | Create entry point routing interface infrastructure for procedure-style dispatch | AC#5, AC#6a, AC#6b, AC#9, AC#16, AC#20, AC#21 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Three independent extensions to interface infrastructure:

**1. IGameState Mode Transition Methods**

Add three new methods to `IGameState` interface:
- `BeginTrain()`: Triggers training mode state transition (BEGIN TRAIN)
- `SaveGameDialog()`: Opens dialog-mode save UI (SAVEGAME)
- `LoadGameDialog()`: Opens dialog-mode load UI (LOADGAME)

Both Era.Core stub (`GameState.cs`) and engine implementation (`GameStateImpl.cs`) implement all three methods. Era.Core stubs return `Result<Unit>.Fail` with appropriate message. Engine implementation delegates to Process state machine (`SetBegin("TRAIN")`, `SaveLoadData()` with dialog mode).

**2. IEntryPointRegistry for Procedure Dispatch**

Create new `IEntryPointRegistry` interface separate from `IFunctionRegistry`:
- `IBuiltInFunction.Execute()` returns `Result<object>` for expression evaluation
- Entry points (SHOW_SHOP, USERSHOP) are void/int procedures with different semantics
- `IEntryPointRegistry.Invoke(string name)` returns `Result<int>` (void procedures return 0)
- Uses same naming convention (ConcurrentDictionary, OrdinalIgnoreCase per IFunctionRegistry precedent)

Two implementations:
- `EntryPointRegistry.cs` (Era.Core): Full implementation with ConcurrentDictionary<string, Func<int>> (OrdinalIgnoreCase). Returns Fail when entry point not registered.
- Engine integration: Register ShopSystem methods via DI property injection

**3. ShopSystem Integration**

Replace three private NotSupportedException stub methods with IGameState calls:
- `BeginTrain()` → `_gameState.BeginTrain()`
- `SaveGame()` → `_gameState.SaveGameDialog()`
- `LoadGame()` → `_gameState.LoadGameDialog()`

Entry point registration deferred to engine initialization (outside F791 scope). ShopSystem methods (ShowShop, UserShop) already public and suitable for registration.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `Result<Unit> BeginTrain();` method signature to IGameState interface |
| 2 | Add `Result<Unit> SaveGameDialog();` method signature to IGameState interface |
| 3 | Add `Result<Unit> LoadGameDialog();` method signature to IGameState interface |
| 4 | Implement all three new IGameState methods in GameState.cs (stubs returning Result.Fail) |
| 5 | Create new file `Era.Core/Functions/IEntryPointRegistry.cs` |
| 6a | Define `Result<int> Invoke(string name);` in IEntryPointRegistry (return type ensures separation from IBuiltInFunction Result<object>) |
| 6b | Define `void Register(string name, Func<int> handler);` in IEntryPointRegistry |
| 7 | Remove all three NotSupportedException stub methods from ShopSystem.cs, replace with IGameState calls |
| 8 | Implement all three new IGameState methods in GameStateImpl.cs (engine-side delegation to Process) |
| 9 | Add `services.AddSingleton<IEntryPointRegistry, EntryPointRegistry>();` to ServiceCollectionExtensions.cs |
| 10 | Replace `throw new NotSupportedException` at BeginTrain call sites with `_gameState.BeginTrain()` |
| 11 | Replace `throw new NotSupportedException` at SaveGame call sites with `_gameState.SaveGameDialog()` |
| 12 | Replace `throw new NotSupportedException` at LoadGame call sites with `_gameState.LoadGameDialog()` |
| 13 | Build Era.Core with TreatWarningsAsErrors=true (XML docs required for all new interfaces/methods) |
| 14 | Create unit tests for all new IGameState method stubs, IEntryPointRegistry stub behavior |
| 15 | Grep all modified files for TODO/FIXME/HACK to ensure zero technical debt |
| 16 | Create and run EntryPointRegistryTests verifying register, invoke, case-insensitive, and unregistered behavior |
| 17 | Verify IGameState stub test methods exist in Era.Core.Tests (BeginTrain, SaveGameDialog, LoadGameDialog) |
| 18 | Grep IGameState.cs for '(mode transition\|state transition\|training mode\|game mode)' (case-insensitive) in XML doc to verify expanded responsibility documentation |
| 19 | Grep IGameState.cs count_equals for 6 existing method signatures (SaveGame, LoadGame, Quit, Restart, ResetData, SetVariable) to verify backward compatibility (C4) |
| 20 | Grep IFunctionRegistry.cs count_equals for 2 existing methods (GetFunction, Register) to verify IFunctionRegistry is unmodified (D3) |
| 21 | Grep engine-dev SKILL.md for IEntryPointRegistry to verify SSOT documentation |
| 22 | Grep engine-dev SKILL.md for BeginTrain/SaveGameDialog/LoadGameDialog to verify SSOT documentation |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **D1**: Where to add BeginTrain() | (a) Extend IGameState, (b) Create new IGameModeManager | Extend IGameState (a) | Consistent with existing IGameState responsibility (game lifecycle: Save/Load/Quit/Restart/ResetData). Avoids interface proliferation. Single service boundary for all game state transitions. |
| **D2**: Dialog-mode save/load method signature | (a) Overload SaveGame(), (b) New parameterless methods SaveGameDialog/LoadGameDialog, (c) New interface IDialogStateManager | New parameterless methods (b) | Avoids C# overload ambiguity (SaveGame() vs SaveGame(string)). Method name suffix makes dialog-mode intent explicit. Preserves backward compatibility with existing file-path-based methods. Option (c) rejected for interface proliferation. |
| **D3**: Entry point routing mechanism | (a) Adapter wrapping IBuiltInFunction, (b) Separate IEntryPointRegistry, (c) Direct method dispatch in GameEngine | Separate IEntryPointRegistry (b) | ISP compliance: IBuiltInFunction (Result<object> expressions) and entry points (void/int procedures) serve different purposes. Option (a) forces procedural semantics into expression interface. Option (c) couples GameEngine to ShopSystem. Separate interface enables clean registration pattern matching IFunctionRegistry precedent. |
| **D4**: Dialog-mode implementation scope | (a) Full 14-state sub-machine reimplementation, (b) Thin delegation to engine Process | Thin delegation (b) | Process.SaveLoadData() manages complex 14-state sub-machine (SaveGame_Begin → SaveGame_000 → ... → SaveGame_End). Full reimplementation is scope creep. Thin delegation approach: IGameState stub returns Fail, engine implementation calls Process.SaveLoadData() or equivalent. Defer full C# migration to later Phase when state machine abstraction exists. |
| **D5**: BeginTrain variable reset handling | (a) Call IVariableStore.Set for each of 8+ arrays, (b) Delegate to engine UpdateInBeginTrain(), (c) Create IVariableStore.ResetForTraining() in F791 | Delegate to engine (b) | VariableEvaluator.UpdateInBeginTrain() (lines 1564-1603) resets TFLAG, TSTR, GOTJUEL, TEQUIP, EX, STAIN, PALAM, SOURCE, TCVAR. F789 MAY add batch reset interface, but it's not a blocker. GameStateImpl.BeginTrain() can call GlobalStatic.VEvaluator.UpdateInBeginTrain() directly. Option (c) is scope creep (F789 responsibility). |
| **D6**: Entry point handler signature | (a) `Func<int>` parameterless, (b) `Func<int[], int>` with args, (c) `Action` for void | `Func<int>` (a) | Current call sites (ShopSystem.ShowShop, UserShop) require no arguments from registry dispatch. Return int (void procedures return 0) allows uniform Result<int> return type. Option (b) deferred until argument-passing entry points emerge. Option (c) loses return value capability. |

### Interfaces / Data Structures

**IGameState Extension** (Era.Core/Interfaces/IGameState.cs):

```csharp
public interface IGameState
{
    // Existing methods (unchanged)
    Result<Unit> SaveGame(string fileName);
    Result<Unit> LoadGame(string fileName);
    Result<Unit> Quit();
    Result<Unit> Restart();
    Result<Unit> ResetData();
    Result<Unit> SetVariable(string name, int index, long value);

    // NEW methods (F791)
    /// <summary>Begin training mode (BEGIN TRAIN)</summary>
    Result<Unit> BeginTrain();

    /// <summary>Save game with dialog-mode UI (SAVEGAME)</summary>
    Result<Unit> SaveGameDialog();

    /// <summary>Load game with dialog-mode UI (LOADGAME)</summary>
    Result<Unit> LoadGameDialog();
}
```

**IEntryPointRegistry** (Era.Core/Functions/IEntryPointRegistry.cs):

```csharp
using Era.Core.Types;

namespace Era.Core.Functions;

/// <summary>
/// Registry for procedure-style ERB entry points (SHOW_SHOP, USERSHOP).
/// Distinct from IFunctionRegistry (expression functions returning Result&lt;object&gt;).
/// Feature 791 - Engine State Transitions & Entry Point Routing
/// </summary>
public interface IEntryPointRegistry
{
    /// <summary>
    /// Invoke registered entry point by name.
    /// </summary>
    /// <param name="name">Entry point name (case-insensitive)</param>
    /// <returns>Success with int return value (0 for void procedures), Failure if not registered</returns>
    Result<int> Invoke(string name);

    /// <summary>
    /// Register entry point handler.
    /// </summary>
    /// <param name="name">Entry point name (case-insensitive)</param>
    /// <param name="handler">Handler function returning int (0 for void procedures)</param>
    void Register(string name, Func<int> handler);
}
```

**EntryPointRegistry Implementation** (Era.Core/Functions/EntryPointRegistry.cs):

```csharp
using System;
using System.Collections.Concurrent;
using Era.Core.Types;

namespace Era.Core.Functions;

/// <summary>
/// Implementation of IEntryPointRegistry for Era.Core with ConcurrentDictionary-backed storage.
/// Entry points are registered at runtime via Register() and dispatched via Invoke().
/// Feature 791 - Engine State Transitions & Entry Point Routing
/// </summary>
public class EntryPointRegistry : IEntryPointRegistry
{
    private readonly ConcurrentDictionary<string, Func<int>> _handlers
        = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public Result<int> Invoke(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<int>.Fail("Entry point name cannot be null or whitespace");

        if (_handlers.TryGetValue(name, out var handler))
        {
            try
            {
                return Result<int>.Ok(handler());
            }
            catch (Exception ex)
            {
                return Result<int>.Fail($"Entry point '{name}' threw: {ex.Message}");
            }
        }
        return Result<int>.Fail($"Entry point '{name}' not registered");
    }

    /// <inheritdoc/>
    public void Register(string name, Func<int> handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        _handlers[name] = handler ?? throw new ArgumentNullException(nameof(handler));
    }
}
```

**GameState Stub Extensions** (Era.Core/Commands/System/GameState.cs):

```csharp
// Add to existing file
public Result<Unit> BeginTrain()
    => Result<Unit>.Fail("BEGIN TRAIN requires engine integration");

public Result<Unit> SaveGameDialog()
    => Result<Unit>.Fail("SAVEGAME dialog mode requires engine integration");

public Result<Unit> LoadGameDialog()
    => Result<Unit>.Fail("LOADGAME dialog mode requires engine integration");
```

**GameStateImpl Extensions** (engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs):

```csharp
// Add to existing file
public Result<Unit> BeginTrain()
{
    // Stub: delegates to engine in F793 (SetBegin("TRAIN") + UpdateInBeginTrain())
    return Result<Unit>.Fail("Stub: engine delegation pending F793");
}

public Result<Unit> SaveGameDialog()
{
    // Stub: delegates to engine in F793 (Process.SaveLoadData() with dialog mode)
    return Result<Unit>.Fail("Stub: engine delegation pending F793");
}

public Result<Unit> LoadGameDialog()
{
    // Stub: delegates to engine in F793 (Process.SaveLoadData() with dialog mode)
    return Result<Unit>.Fail("Stub: engine delegation pending F793");
}
```

**ShopSystem Integration Pattern** (Era.Core/Shop/ShopSystem.cs):

```csharp
// BEFORE (F774 stub):
private void BeginTrain()
    => throw new NotSupportedException("...");

// AFTER (F791 integration):
// Remove private stub method entirely
// At call sites (line 86, 333), fire-and-forget (DES-001: stubs return Fail; Result discarded):
_gameState.BeginTrain();
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| ~~AC#6 Expected pattern incomplete~~ | ~~AC Details (AC#6)~~ | RESOLVED: Split into AC#6a (Invoke with Result<int>) and AC#6b (Register with void) in iter3. |
| ~~AC#4, AC#8 count verification ambiguous~~ | ~~AC Definition Table~~ | RESOLVED: AC#4/AC#8 updated to use specific `public Result<Unit> (BeginTrain\|SaveGameDialog\|LoadGameDialog)\(\)` pattern in iter1. |
| ~~AC#15 file list in Expected column~~ | ~~AC Details (AC#15)~~ | RESOLVED: File paths are in Method column (Grep path). Expected column contains only the pattern. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|:---------- ---|:---:|:------:|
| 1 | 1,2,3,18,19 | Extend IGameState interface with BeginTrain, SaveGameDialog, LoadGameDialog methods; update XML doc to reflect expanded responsibility (persistence + lifecycle + mode transitions); verify existing methods preserved | | [x] |
| 2 | 4 | Implement Era.Core stub methods in GameState.cs for new IGameState methods | | [x] |
| 3 | 8 | Implement engine-side methods in GameStateImpl.cs for new IGameState methods | | [x] |
| 4 | 5,6a,6b | Create IEntryPointRegistry interface with Invoke and Register methods | | [x] |
| 5 | 16 | Create EntryPointRegistry implementation in Era.Core (ConcurrentDictionary-backed, full Invoke/Register behavior) | | [x] |
| 6 | 9 | Register IEntryPointRegistry in ServiceCollectionExtensions.cs | | [x] |
| 7 | 7,10,11,12 | Remove private BeginTrain()/SaveGame()/LoadGame() stub methods from ShopSystem, replace each call site with direct IGameState calls (_gameState.BeginTrain(), _gameState.SaveGameDialog(), _gameState.LoadGameDialog()), and update call-site comments to remove NotSupportedException references | | [x] |
| 8 | 13 | Build Era.Core with zero warnings (TreatWarningsAsErrors compliance) | | [x] |
| 9 | 14,16,17 | Create unit tests for IGameState stub methods and IEntryPointRegistry | | [x] |
| 10 | 15 | Verify zero technical debt in all new/modified files | | [x] |
| 11 | 20 | Verify IFunctionRegistry.cs unmodified (D3 separation enforcement) | | [x] |
| 12 | 21,22 | Update engine-dev SKILL.md with IEntryPointRegistry interface and IGameState new methods (BeginTrain, SaveGameDialog, LoadGameDialog) | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

No [I] tags used in this feature. All Tasks have deterministic AC Expected values.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | ac-tester | haiku | Task 9 | Era.Core.Tests/ unit tests (RED state — tests written before implementation) |
| 2 | implementer | sonnet | Task 1 | IGameState.cs with 3 new method signatures + updated XML doc |
| 3 | implementer | sonnet | Task 2 | GameState.cs with 3 stub implementations |
| 4 | implementer | sonnet | Task 3 | GameStateImpl.cs with 3 stub implementations |
| 5 | implementer | sonnet | Task 4 | IEntryPointRegistry.cs interface file |
| 6 | implementer | sonnet | Task 5 | EntryPointRegistry.cs full implementation (ConcurrentDictionary-backed) |
| 7 | implementer | sonnet | Task 6 | ServiceCollectionExtensions.cs DI registration |
| 8 | implementer | sonnet | Task 7 | ShopSystem.cs NotSupportedException stubs replaced |
| 9 | implementer | sonnet | Task 8 | Build verification + GREEN state (dotnet build + dotnet test via WSL) |
| 10 | implementer | sonnet | Task 10 | Technical debt verification via Grep |
| 11 | implementer | sonnet | Task 11 | IFunctionRegistry.cs unmodified verification via Grep |
| 12 | implementer | sonnet | Task 12 | engine-dev SKILL.md updated with IEntryPointRegistry + IGameState new methods |

### Execution Order

**Task 9 (Phase 1)**: Test creation first (TDD RED — write failing tests for IGameState stubs and EntryPointRegistry). Note: EntryPointRegistry tests reference IEntryPointRegistry.cs (created in Phase 5) and will not compile until then. This is expected TDD RED behavior — compilation failure is a valid RED state. Interface code snippets in Technical Design provide the contract for test authoring.
**Tasks 1-4 (Phases 2-5)**: Interface definitions (can be parallelized by file)
**Task 5 (Phase 6)**: EntryPointRegistry implementation (depends on Task 4 IEntryPointRegistry interface)
**Task 6 (Phase 7)**: DI registration (depends on Task 4 IEntryPointRegistry + Task 5 EntryPointRegistry)
**Task 7 (Phase 8)**: ShopSystem integration (depends on Task 1 IGameState extension)
**Task 8 (Phase 9)**: Build + test verification (blocking gate — TDD GREEN state, all tests pass)
**Task 10 (Phase 10)**: Technical debt check (final verification)
**Task 11 (Phase 11)**: IFunctionRegistry unmodified verification
**Task 12 (Phase 12)**: engine-dev SKILL.md SSOT update (depends on Tasks 1, 4 for interface documentation)

### Build Verification

**Command** (via WSL):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core'
```

**Success Criteria**: Exit code 0, zero warnings (TreatWarningsAsErrors=true enforced)

### Test Requirements

**Test Files**: Create in `Era.Core.Tests/`
- `Interfaces/GameStateTests.cs` - Test new IGameState stub methods return Fail with appropriate messages
- `Functions/EntryPointRegistryTests.cs` - Test Register → Invoke round-trip, unregistered entry point returns Fail

**Pattern**: Use existing `ResultAssert.cs` helpers for Result<T> assertions

### DI Registration

**File**: `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`

**Code**:
```csharp
services.AddSingleton<IEntryPointRegistry, EntryPointRegistry>();
```

### Interface Code Snippets

All interface code snippets provided in Technical Design section (lines 450-597) include namespace, using statements, and XML documentation per Issue 21 guidance.

### Error Handling

**Technical Constraints**:
- IGameState methods return `Result<Unit>` (C9 constraint)
- IEntryPointRegistry.Invoke returns `Result<int>` (void procedures return 0)
- All stubs return `Result.Fail` with descriptive error message indicating engine integration required
- EntryPointRegistry.Register throws `ArgumentNullException` if handler is null

### Success Criteria

- All 23 ACs pass
- Build succeeds with 0 warnings
- Unit tests pass (dotnet test Era.Core.Tests)
- Zero NotSupportedException in ShopSystem.cs (AC#7)
- Zero TODO/FIXME/HACK in new/modified files (AC#15)

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| GameStateImpl.BeginTrain() engine delegation | D5: stub returns Fail; engine needs to delegate to UpdateInBeginTrain() for 8+ array resets | Feature | F793 | - |
| GameStateImpl.SaveGameDialog()/LoadGameDialog() engine delegation | D4: stubs return Fail; engine needs thin delegation to Process.SaveLoadData() (14-state sub-machine) | Feature | F793 | - |
| SHOW_SHOP/USERSHOP entry point registration into IEntryPointRegistry | Goal 3 defers actual registration to engine initialization; ShopSystem.ShowShop()/UserShop() are public and suitable for registration | Feature | F793 | - |

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK
- Missing Task for Option A → FL FAIL
-->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-15 16:22 | START | implementer | Task 9 (TDD RED) | - |
| 2026-02-15 16:22 | END | implementer | Task 9 (TDD RED) | SUCCESS - Tests created, compilation failure confirmed |
| 2026-02-15 16:25 | START | implementer | Task 4 | - |
| 2026-02-15 16:25 | END | implementer | Task 4 | SUCCESS |
| 2026-02-15 16:25 | START | implementer | Task 1 | - |
| 2026-02-15 16:25 | END | implementer | Task 1 | SUCCESS |
| 2026-02-15 16:26 | START | implementer | Task 2 | - |
| 2026-02-15 16:26 | END | implementer | Task 2 | SUCCESS |
| 2026-02-15 16:27 | START | implementer | Task 5 | - |
| 2026-02-15 16:27 | END | implementer | Task 5 | SUCCESS |
| 2026-02-15 16:29 | START | implementer | Task 6 | - |
| 2026-02-15 16:29 | END | implementer | Task 6 | SUCCESS |
| 2026-02-15 16:29 | START | implementer | Task 7 | - |
| 2026-02-15 16:29 | END | implementer | Task 7 | SUCCESS |
| 2026-02-15 16:30 | DEVIATION | orchestrator | Task 8 dotnet test | 1 FAIL: Register_NullName_ThrowsArgumentException - Assert.Throws exact type mismatch (ArgumentNullException vs ArgumentException). Fixed: Assert.ThrowsAny<ArgumentException> |
| 2026-02-15 16:30 | END | orchestrator | Task 8 | SUCCESS - Build 0 warnings, 1964 tests pass |
| 2026-02-15 16:35 | DEVIATION | orchestrator | Phase 7 ac-static-verifier | exit 1: 5 count_equals ACs failed due to tool format parsing limitation (Expected format 'Pattern (N)' vs actual 'pattern = N'). All 5 verified manually via Grep: AC#4=3, AC#8=3, AC#10=2, AC#19=6, AC#20=2 — all PASS |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [resolved-skipped] Phase2-Uncertain iter1: [FMT-002] Root Cause Analysis section uses non-template 'Investigation (Consensus of 3 Investigators)' format instead of standard '5 Whys' + 'Symptom vs Root Cause' tables. Content is substantive but format deviates from feature-template.md. Resolution: 3-investigator consensus provides richer multi-perspective analysis for structural gap analysis. Reformatting completed section risks information loss for no functional benefit.
- [resolved-skipped] Phase2-Uncertain iter1: [PHI-001] Philosophy Derivation row 3 maps 'Pipeline Continuity' to AC#7,10,11,12 (stub replacement) but this is semantically weak. Resolution: Mapping is defensible — row 1 covers interface declaration (AC#1-4,8,17), row 3 covers integration into ShopSystem (AC#7,10-12). Without stub replacement, Phase 20 ERB migration remains blocked regardless of interface existence. Two rows are complementary (declare vs integrate), not redundant.
- [fix] Phase2-Review iter1: Impact Analysis section | Added Impact Analysis subsection (5 areas: IGameState consumers, DI container, ShopSystem, GameStateImpl, IFunctionRegistry)
- [fix] Phase2-Review iter1: Baseline Measurement section | Added Baseline Measurement subsection (3 metrics: build warnings, test pass count, NotSupportedException count)
- [fix] Phase2-Review iter1: AC Definition Table + AC Details + Goal Coverage | Added AC#16 EntryPointRegistry Register+Invoke round-trip test; updated Goal 3 coverage and Task#9 AC mapping
- [fix] Phase2-Review iter1: AC Design Constraints C5/C8 | Updated C5/C8 AC Implication to clarify deferral to engine integration (D5/D4)
- [fix] Phase2-Review iter2: Success Criteria | Changed 'All 15 ACs pass' to 'All 16 ACs pass' (AC#16 added in iter1)
- [fix] Phase2-Review iter2: AC Design Constraints | Added Constraint Details subsection (C1-C11) with Source/Verification/AC Impact per feature-template.md
- [fix] Phase2-Review iter3: Technical Design Approach | Changed EntryPointRegistry description from 'Stub returning Fail' to 'Full implementation with ConcurrentDictionary'
- [resolved-applied] Phase2-Review iter3: [AC-003] AC#4/AC#8 pattern 'BeginTrain()|SaveGameDialog()|LoadGameDialog()' with count_equals=3 may match comments like 'UpdateInBeginTrain()' in GameStateImpl.cs. Proposed fix: use 'public Result<Unit> (BeginTrain|SaveGameDialog|LoadGameDialog)\(\)' pattern.
- [fix] Phase3-Maintainability iter1: AC#4 AC#8 AC Definition Table + AC Details | Updated grep patterns from 'BeginTrain()|SaveGameDialog()|LoadGameDialog()' to 'public Result<Unit> (BeginTrain|SaveGameDialog|LoadGameDialog)\(\)' for specificity
- [resolved-skipped] Phase2-Review iter3: [FMT-003] Dependencies section at line 88 placed between Background and Investigation, deviating from template order (should be after AC Design Constraints). Resolution: Section ordering is cosmetic. Dependencies content and cross-references are correct. Reordering completed document provides no functional benefit.
- [resolved-skipped] Phase2-Uncertain iter3: [FMT-004] Non-template sections (Created, Summary, Scope Reference) present in F791. Resolution: Accepted as batch convention per F789/F790/F791 precedent. These sections provide useful context (creation date, scope boundaries) not covered by template sections.
- [fix] Phase2-Review iter4: AC#15 file list | Added EntryPointRegistry.cs, GameStateImpl.cs, ServiceCollectionExtensions.cs to AC#15 path list (3 missing files)
- [fix] Phase2-Review iter4: Technical Design code snippet | Changed 'EntryPointRegistry Stub Implementation' to 'EntryPointRegistry Implementation'; updated XML doc comment
- [resolved-applied] Phase2-Uncertain iter4: [AC-004] Goal 3 says 'for ERB procedure names to C# class methods' but no AC verifies SHOW_SHOP/USERSHOP are actually routable. Technical Design defers registration. Fix: Narrowed Goal 3 text to 'Create entry point routing interface infrastructure for procedure-style dispatch (actual registration deferred to engine initialization)'. Updated Goal Coverage table to match.
- [fix] Phase2-Review iter5: Feasibility Assessment table | Renamed column 'Notes' to 'Evidence' per template
- [fix] Phase2-Review iter5: Task#5 description | Changed 'stub implementation' to 'implementation (ConcurrentDictionary-backed, full Invoke/Register behavior)'
- [resolved-applied] Phase2-Uncertain iter5: [FMT-005] AC#6 Method column omits grep pattern (pattern only in AC Details). Same convention as AC#4/AC#8. If addressed, should apply to all count_equals ACs consistently.
- [fix] Phase2-Review iter6: Implementation Contract Phase 5 | Changed Output from 'stub implementation' to 'full implementation (ConcurrentDictionary-backed)'
- [fix] Phase2-Review iter6: Philosophy Derivation row 2 | Added AC#16 to AC Coverage (procedure-style entry point behavioral test)
- [resolved-skipped] Phase2-Uncertain iter6: [DES-001] No AC verifies ShopSystem handles Result.Fail from IGameState calls. Resolution: Intentional for infrastructure phase. Both stubs return Fail; ShopSystem discards result. When engine-side GameStateImpl provides real delegation (Mandatory Handoff DES-002), Result will be Ok. Failure path testing belongs to engine integration scope, not interface infrastructure.
- [fix] Phase2-Review iter7: AC Definition Table + Details + Goal Coverage | Added AC#17 IGameState stub test existence; updated Task#9 to AC#14,16,17; updated Success Criteria to 17 ACs
- [fix] Phase3-Maintainability iter8: Task#8,#10 Tag | Added [V] (verification) tags to distinguish from implementation tasks
- [resolved-applied] Phase3-Maintainability iter8: [DES-002] GameStateImpl 3 new stubs return Result.Fail ('Not implemented') with no tracking for engine-side implementation. Fix: Added 2 Mandatory Handoff entries — (1) BeginTrain() engine delegation via D5 (UpdateInBeginTrain), (2) SaveGameDialog()/LoadGameDialog() engine delegation via D4 (Process.SaveLoadData). Destination: Phase 20 engine integration feature.
- [resolved-skipped] Phase3-Maintainability iter8: [DES-003] IEntryPointRegistry.Register uses Func<int> (parameterless). Resolution: D6 explicitly documents this as conscious trade-off. Adding unused params int[] args violates YAGNI. Current call sites (ShowShop, UserShop) need no arguments. When argument-passing entry points emerge, new overload can be added without breaking existing registrations (additive, not breaking change).
- [resolved-applied] Phase3-Maintainability iter8: [DES-004] IGameState responsibility expanded from 'state persistence' to include 'mode transitions' (BeginTrain). D1 chose this. Consider updating IGameState XML doc to reflect expanded responsibility scope.
- [fix] Phase3-Maintainability iter1: Task#1 description + Implementation Contract Phase 1 | Added XML doc update requirement to reflect expanded IGameState responsibility (persistence + lifecycle + mode transitions)
- [resolved-applied] Phase3-Maintainability iter8: [DES-005] F789 IVariableStore relationship is 'MAY benefit' (vague). Fix: Clarified to 'no dependency'. D5 delegates BeginTrain variable reset to engine's UpdateInBeginTrain() directly (8+ array resets handled engine-side). F789 batch reset is independent optimization, not required by F791. Updated Related Features F789 entry.
- [fix] Phase2-Review iter2: Feasibility Assessment table | Changed 'Breaking change risk' Assessment from 'LOW' to 'FEASIBLE' per template enum
- [fix] Phase2-Review iter2: Execution Order | Fixed Task 5 dependency on Task 4 (was incorrectly claimed parallelizable with Tasks 1-6)
- [fix] Phase2-Review iter2: Execution Order | Corrected TDD comment from '[I] tag guidance' to 'TDD RED-GREEN' (no tasks have [I] tag)

- [fix] Phase2-Review iter3: AC#6 split | Split AC#6 into AC#6a (Invoke with Result<int> return type) and AC#6b (Register with void return type) for return type verification
- [fix] Phase2-Review iter3: AC#16 duplicate registration | Added 'duplicate registration overwrites handler' to AC#16 Expected test cases
- [fix] Phase2-Review iter4: C3 AC Impact | Updated stale AC#6 reference to AC#6a, AC#6b after split
- [fix] Phase2-Review iter4: Upstream Issues | Marked AC#6 and AC#4/AC#8 upstream issues as RESOLVED
- [fix] Phase2-Review iter5: Philosophy Derivation | Added AC#17 to row 1 AC Coverage ('have no C# interface')
- [fix] Phase2-Review iter5: Upstream Issues | Marked AC#15 upstream issue as RESOLVED (file list already in Method column)
- [fix] Phase2-Review iter6: Goal Coverage | Added AC#7 to Goals 1 and 2 (NotSupportedException = integration verification)
- [fix] Phase7-FinalRefCheck iter7: Links section | Added F782 (Successor: Post-Phase Review) to Links
- [resolved-applied] Phase2-Pending iter1: [DES-006] Mandatory Handoffs: Both rows have Destination ID = '-' and Creation Task = '-'. Per template, handoffs require Option A (Creation Task), B (existing Feature ID), or C (Phase in architecture.md). Neither is satisfied — TBD violation. Resolution: Option A applied — created F793 [DRAFT] for GameStateImpl engine-side delegation.
- [fix] Phase2-Review iter1: AC#10 matcher | Changed from 'matches' (1+) to 'count_equals' = 2 to ensure both BeginTrain call sites (UserShop, DebugEnterUfufu) are converted
- [fix] Phase2-Uncertain iter1: AC#18 XML doc | Added AC#18 (IGameState XML doc reflects expanded responsibility with 'mode transition' pattern) to verify Task#1 XML doc update requirement
- [fix] Phase2-Review iter2: Success Criteria | Changed 'All 18 ACs pass' to 'All 19 ACs pass' (AC#6a/6b are separate rows + AC#18 added)
- [fix] Phase2-Review iter2: AC#18 pattern | Tightened from 'mode transition' to 'Interface.*mode transition' to anchor to interface-level summary XML doc
- [fix] Phase2-Review iter2: Task#8,#10 Tag | Removed undefined [V] tags; these are deterministic verification tasks per template (build=succeeds, grep=0 matches), use (none) tag
- [fix] Phase2-Review iter3: Task#7 description | Clarified to explicitly specify removing private stub methods and replacing call sites with direct IGameState calls (avoids ambiguity between modifying stub body vs inlining)
- [fix] Phase2-Review iter3: AC#16 Expected | Added 'void procedure returns 0' test case to verify D6 convention (Func<int> returning 0 for void procedures)
- [fix] Phase2-Review iter3: Implementation Contract | Restructured for TDD: Phase 1 = ac-tester (RED, tests first), Phases 2-8 = implementer (GREEN, implementation), Phase 9 = build+test verification
- [fix] Phase2-Review iter3: Feasibility Assessment | Moved 'DESIGN DECISION NEEDED' qualifier from Assessment enum to Evidence column (template enum: FEASIBLE/NEEDS_REVISION/NOT_FEASIBLE only)
- [fix] Phase2-Review iter4: AC#19 backward compatibility | Added AC#19 to verify all 6 existing IGameState methods preserved (C4 constraint had no enforcement AC)
- [fix] Phase2-Review iter4: AC#20 IFunctionRegistry separation | Added AC#20 to verify IFunctionRegistry.cs unmodified (D3 design decision enforcement)
- [fix] Phase2-Uncertain iter4: AC#18 case-insensitive flag | Added -i flag to AC#18 Method column for consistency with AC Details
- [fix] Phase4-ACAlignment iter5: Task#1 AC coverage | Extended from [1,2,3] to [1,2,3,18,19] (AC#18 XML doc + AC#19 backward compat are direct Task#1 consequences)
- [fix] Phase4-ACAlignment iter5: Task#11 + AC#20 | Added Task#11 for IFunctionRegistry unmodified verification (AC#20 was orphan — negative assertion needs explicit task)
- [fix] Phase2-Uncertain iter1: Execution Order Task 9 | Added note clarifying EntryPointRegistry tests won't compile until Phase 5 (IEntryPointRegistry.cs creation), which is expected TDD RED behavior
- [fix] Phase2-Review iter2: Mandatory Handoffs Creation Task | Changed 'FL POST-LOOP Option A' to '-' for both rows (F793 exists → Option B)
- [fix] Phase2-Review iter2: Dependencies table | Added Successor rows for F782 [DRAFT] and F793 [DRAFT]
- [resolved-applied] Phase2-Review iter3: [DES-007] IEntryPointRegistry created (Goal 3) but actual registration of SHOW_SHOP/USERSHOP deferred to 'engine initialization' with no Mandatory Handoff entry. F793 only covers GameStateImpl delegation (D4/D5), not entry point registration. Violates 'Track What You Skip'. Resolution: Added third Mandatory Handoff row for entry point registration → F793.
- [fix] Phase2-Review iter3: Task#7 description | Added 'and update call-site comments to remove NotSupportedException references' (comments at call sites would trigger AC#7 not_contains matcher)
- [fix] Phase2-Review iter3: AC#15 file list | Added Era.Core.Tests/Interfaces/GameStateTests.cs and Era.Core.Tests/Functions/EntryPointRegistryTests.cs to AC#15 method column and AC Details
- [info] Phase1-DriftChecked: F788 (Related -- IConsoleOutput Extensions)
- [info] Phase1-DriftChecked: F790 (Related -- Engine Data Access Layer)
- [info] Phase1-DriftChecked: F789 (Related -- IVariableStore Extensions)
- [fix] Phase2-Review iter1: Mandatory Handoffs table | Added third row for SHOW_SHOP/USERSHOP entry point registration → F793 (resolved DES-007)
- [fix] Phase2-Review iter1: Technical Design ShopSystem code snippet | Removed if-Failure check, aligned with DES-001 fire-and-forget resolution
- [resolved-applied] Phase3-Maintainability iter2: [DES-008] Mandatory Handoff row 3 (SHOW_SHOP/USERSHOP entry point registration) points to F793, but F793 Goal only covers GameStateImpl delegation. F793 does not mention entry point registration. Destination mismatch. Resolution: F793 Goal expanded to include item 4 (entry point registration via IEntryPointRegistry).
- [fix] PostLoop-UserFix iter1: F793 Goal + Problem | Added entry point registration (SHOW_SHOP/USERSHOP → IEntryPointRegistry) as Goal item 4 and Problem item 4 to resolve DES-008 destination mismatch
- [fix] Phase3-Maintainability iter2: Technical Design GameStateImpl code snippet | Updated stub comments to reference F793 explicitly instead of vague 'Implementation deferred' phrasing
- [fix] Phase3-Maintainability iter3: EntryPointRegistry.Invoke code snippet | Added try-catch wrapping for handler() invocation to maintain Result<int> contract; added handler exception test case to AC#16
- [fix] Phase3-Maintainability iter3: EntryPointRegistry.Register code snippet | Added name parameter validation (ArgumentException.ThrowIfNullOrWhiteSpace) matching FunctionRegistry pattern; added name validation test case to AC#16
- [fix] Phase2-Review iter1: AC#18 pattern | Broadened from 'Interface.*mode transition' to '(mode transition|state transition|training mode|game mode)' to accept semantically equivalent phrasings and avoid false negatives
- [fix] Phase2-Review iter2: AC#6b pattern | Changed from 'void\s+Register\(string' to 'void\s+Register\(string.*Func<int>' to enforce D6 handler type decision
- [fix] Phase2-Review iter2: Task#5 AC mapping | Changed from AC#5 to AC#16 (AC#5 verifies interface file = Task#4's output; AC#16 tests EntryPointRegistry behavior = Task#5's output)
- [fix] Phase2-Review iter3: Execution Order | Separated Task 6 from parallel group (Tasks 1-4); Task 6 depends on Task 4 (IEntryPointRegistry) + Task 5 (EntryPointRegistry)
- [fix] Phase3-Maintainability iter4: SSOT update | Added Task#12, AC#21, AC#22 for engine-dev SKILL.md documentation of IEntryPointRegistry and IGameState new methods; added Implementation Contract Phase 12; updated Success Criteria to 23 ACs
- [fix] Phase2-Review iter5: Philosophy Derivation row 4 | Added AC#21, AC#22 to 'Zero Debt Upfront' row (SSOT documentation accuracy)
- [fix] Phase2-Review iter5: EntryPointRegistry.Invoke code snippet | Added null/whitespace name validation (return Fail instead of unhandled exception from ConcurrentDictionary); added Invoke null-name test case to AC#16
- [fix] Phase2-Review iter6: AC#16 Expected | Added 'Register null handler throws ArgumentNullException' test case (Technical Design code defines handler null guard but AC#16 did not test it)
- [fix] Phase3-Maintainability iter1: AC Definition Table patterns | Replaced backslash-pipe (\|) with pipe (|) for ripgrep alternation in AC#4, AC#8, AC#15, AC#17, AC#18, AC#19, AC#20, AC#22
- [fix] Phase1-RefCheck iter1: Links section | Added F377 and F434 (archive) to Links as Related references (referenced in Investigation section but missing from Links)
- [fix] Phase3-Maintainability iter1: Dependencies section | Added Dependency Types SSOT template comment
- [fix] Phase3-Maintainability iter1: Tasks section | Added AC Coverage Rule comment and Task Tags subsection
- [fix] Phase3-Maintainability iter1: Mandatory Handoffs section | Added template comments (CRITICAL, Option A/B/C, Validation)

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning
- [Predecessor: F774](feature-774.md) - Shop Core (Mandatory Handoff origin)
- [Related: F377](archive/feature-377.md) - IGameState Design (Phase 9)
- [Related: F434](archive/feature-434.md) - IGameState Design (Phase 9)
- [Related: F421](archive/feature-421.md) - IFunctionRegistry / Function Call Mechanism
- [Related: F788](feature-788.md) - IConsoleOutput Phase 20 Extensions
- [Related: F789](feature-789.md) - IVariableStore Phase 20 Extensions
- [Related: F790](feature-790.md) - Engine Data Access Layer
- [Successor: F782](feature-782.md) - Post-Phase Review
- [Successor: F793](feature-793.md) - GameStateImpl Engine-Side Delegation
