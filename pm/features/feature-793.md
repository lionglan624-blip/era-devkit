# Feature 793: GameStateImpl Engine-Side Delegation

## Status: [DONE]
<!-- fl-reviewed: 2026-02-16T00:00:00Z -->

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

---

## Review Context
<!-- Written by FL POST-LOOP Step 6.3. Review findings only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F791 |
| Discovery Point | Philosophy Gate (POST-LOOP Step 6.3) |
| Timestamp | 2026-02-15 |

### Identified Gap

F791 added 3 new methods to IGameState interface (BeginTrain, SaveGameDialog, LoadGameDialog) with Era.Core stubs returning Result.Fail. The engine-side implementation (GameStateImpl.cs) also has stubs returning Fail. The gap is that these stubs do not delegate to the actual engine Process state machine methods, leaving the functionality unimplemented on the engine side.

### Review Evidence
| Field | Value |
|-------|-------|
| Gap Source | F791 Mandatory Handoffs section |
| Derived Task | "GameStateImpl.BeginTrain() → delegate to Process.SetBegin('TRAIN') (variable reset handled by engine state machine)" |
| Comparison Result | "GameStateImpl.SaveGameDialog()/LoadGameDialog() → delegate to Process.SaveLoadData() with dialog mode" |
| DEFER Reason | "D5: stub returns Fail; engine needs to delegate to UpdateInBeginTrain() for 8+ array resets; D4: stubs return Fail; engine needs thin delegation to Process.SaveLoadData() (14-state sub-machine)" |

### Files Involved
| File | Relevance |
|------|-----------|
| engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs | Target file - contains stub implementations to be replaced with engine delegation |
| engine/Assets/Scripts/Emuera/GameProc/Process.SystemProc.cs | BeginTrain source - contains beginTrain() method (lines 250-260) |
| engine/Assets/Scripts/Emuera/GameProc/Process.State.cs | State transition source - SetBegin("TRAIN") (lines 182-201), SaveLoadData() (lines 236-244) |
| engine/Assets/Scripts/Emuera/GameData/Variable/VariableEvaluator.cs | Variable reset source - UpdateInBeginTrain() (lines 1564-1603) resets 8+ arrays |

### Parent Review Observations

F791 Design Decision D5 specifies: "GameStateImpl.BeginTrain() can call GlobalStatic.VEvaluator.UpdateInBeginTrain() directly." The variable reset includes TFLAG, TSTR, GOTJUEL, TEQUIP, EX, STAIN, PALAM, SOURCE, TCVAR.

F791 Design Decision D4 specifies: "Process.SaveLoadData() manages complex 14-state sub-machine (SaveGame_Begin → SaveGame_000 → ... → SaveGame_End). Thin delegation approach: engine implementation calls Process.SaveLoadData() or equivalent."

Both D4 and D5 deferred actual engine-side delegation to "Phase 20 engine integration feature". This feature (F793) materializes that deferral.

---

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - GameStateImpl.cs is the single source of truth for engine-side state operations delegated from Era.Core interfaces. Each Phase 20 interface method must have a working engine-side delegation implementation so that when DI wiring routes to GameStateImpl (F782), the delegation logic is ready. Runtime effect requires F782 DI wiring override (ShopSystem → GameStateImpl instead of Era.Core stub). Dispatch consumer wiring and entry point registration are also deferred to F782.

### Problem (Current Issue)

F791 created IGameState interface methods (BeginTrain, SaveGameDialog, LoadGameDialog) but deliberately deferred engine-side delegation via Mandatory Handoffs D4/D5. GameStateImpl.cs (lines 55, 61, 67) contains 3 stubs returning `Result.Fail("Stub: engine delegation pending F793")` because F791 scoped only interface creation and stub placement, not the actual wiring to GlobalStatic.Process and GlobalStatic.VEvaluator.

This architectural phasing means:
1. ShopSystem.UserShop() calls IGameState.BeginTrain() but receives Fail because GameStateImpl does not delegate to `Process.getCurrentState.SetBegin("TRAIN")`
2. SAVEGAME/LOADGAME dialog-mode commands remain non-functional because GameStateImpl does not call `Process.saveCurrentState(true)` followed by `Process.getCurrentState.SaveLoadData()`

The precedent for this delegation pattern exists in CharacterManagerImpl.cs (line 18: `GlobalStatic.VEvaluator.AddCharacter(characterId.Value)`) which demonstrates the try-catch-Result pattern for engine API calls from the same assembly.

### Goal (What to Achieve)

Implement engine-side delegation in GameStateImpl.cs for all 3 stub IGameState methods:
1. BeginTrain() delegates to `Process.getCurrentState.SetBegin("TRAIN")` (state transition only, not full EVENTTRAIN pipeline — variable reset handled by engine's beginTrain() via state machine)
2. SaveGameDialog() delegates to `Process.saveCurrentState(true)` then `Process.getCurrentState.SaveLoadData(true)` (two-step pattern per Instraction.Child.cs:1745-1747)
3. LoadGameDialog() delegates to `Process.saveCurrentState(true)` then `Process.getCurrentState.SaveLoadData(false)` (same two-step pattern, isSave=false)

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do BeginTrain/SaveGameDialog/LoadGameDialog return Fail? | GameStateImpl.cs contains stubs returning Result.Fail instead of delegating to engine APIs | GameStateImpl.cs:55,61,67 |
| 2 | Why are these stubs instead of real implementations? | F791 deliberately scoped only interface creation and stub placement, deferring engine delegation | F791 Mandatory Handoffs D4, D5 |
| 3 | Why did F791 defer engine delegation? | Engine APIs (Process state machine, VEvaluator variable resets) require careful two-step patterns and state guard handling that exceeded F791's interface-creation scope | Instraction.Child.cs:1745-1747 (saveCurrentState before SaveLoadData); Process.State.cs:215 (__CAN_BEGIN__ check) |
| 4 | Why can't Era.Core implement the delegation directly? | Era.Core cannot reference engine layer (GlobalStatic); implementations must live in engine/ assembly where GlobalStatic is accessible | ENGINE.md Issue 29; CharacterManagerImpl.cs:18 precedent |
| 5 | Why (Root)? | Phased architecture separates interface definition (Era.Core) from engine delegation (engine/), requiring a dedicated feature to wire each set of stubs to their engine APIs | F791 architecture: Era.Core interfaces + engine implementations pattern |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | GameStateImpl methods return Result.Fail for BeginTrain/SaveGameDialog/LoadGameDialog | F791 phased architecture deferred engine delegation to a successor feature (F793) via D4/D5 Mandatory Handoffs |
| Where | GameStateImpl.cs:55,61,67 (stub return statements) | Architectural boundary between Era.Core (interfaces) and engine/ (GlobalStatic-dependent implementations) |
| Fix | Hardcode return Ok (hides the problem) | Implement proper delegation to Process.getCurrentState.SetBegin("TRAIN"), Process.saveCurrentState(), Process.getCurrentState.SaveLoadData with null checks and try-catch error handling |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F791 | [DONE] | Predecessor - created IGameState interface methods and stubs that F793 fills |
| F782 | [DRAFT] | Successor - Post-Phase Review Phase 20; depends on F793 completion |
| F774 | [DONE] | Related - Shop Core (SHOP.ERB + SHOP2.ERB); ShopSystem.cs calls IGameState methods |
| F788 | [DONE] | Related - IConsoleOutput Phase 20 Extensions; same Phase 20 migration |
| F789 | [DONE] | Related - IVariableStore Phase 20 Extensions; same Phase 20 migration |
| F790 | [DONE] | Related - Engine Data Access Layer; same Phase 20 migration |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| API Accessibility | FEASIBLE | GlobalStatic.Process (public static, GlobalStatic.cs:35) is accessible from GameStateImpl in the same assembly |
| Method Signatures | FEASIBLE | SetBegin(string) is public void (Process.State.cs:182); SaveLoadData(bool) is public void (Process.State.cs:236); saveCurrentState(bool) is public void (Process.ScriptProc.cs:977) |
| Precedent | FEASIBLE | CharacterManagerImpl.cs demonstrates identical pattern: try { GlobalStatic.VEvaluator.Method(); return Ok; } catch { return Fail; } |
| Entry Point Registration | FEASIBLE | IEntryPointRegistry.Register(string, Func<int>) accepts lambda closures; ShopSystem.ShowShop() returns void (wrap as () => { shop.ShowShop(); return 0; }) and UserShop() returns int directly |
| Volume | FEASIBLE | Estimated ~80 lines of implementation code (3 method bodies + registration bootstrap), well within 300-line limit |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| GameStateImpl.cs | HIGH | 3 stub methods replaced with engine delegation logic |
| ShopSystem runtime | HIGH | BeginTrain/SaveGameDialog/LoadGameDialog calls will succeed instead of returning Fail |
| IEntryPointRegistry | MEDIUM | SHOW_SHOP and USERSHOP entry points registered, enabling procedure dispatch |
| Engine startup/init | LOW | Registration bootstrap code added at engine initialization point |
| Existing IGameState methods | LOW | Pre-existing 6 methods (SaveGame, LoadGame, Quit, Restart, ResetData, SetVariable) remain unchanged |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| BeginTrain scope limited to UpdateInBeginTrain() + SetBegin("TRAIN") | Process.State.cs:264-312 (Begin() dispatches based on begintype); EVENTTRAIN is engine-internal pipeline | Must NOT attempt to replicate full EVENTTRAIN event chain; only variable reset and state transition |
| SaveLoad two-step pattern: saveCurrentState(true) BEFORE SaveLoadData() | Instraction.Child.cs:1745-1747 (existing engine pattern) | Must call saveCurrentState(true) first to backup ProcessState, then getCurrentState.SaveLoadData(isSave) |
| __CAN_SAVE__/__CAN_BEGIN__ state guards throw CodeEE | Process.State.cs:204-234 (SetBegin checks __CAN_BEGIN__ flag); Instraction.Child.cs:1738 (SAVEGAME instruction checks __CAN_SAVE__ before calling SaveLoadData) | Must wrap in try-catch converting CodeEE to Result.Fail; note __CAN_SAVE__ check is at instruction level, not in SaveLoadData() itself |
| GlobalStatic.Process may be null | GlobalStatic.cs:283-287 (Reset() sets to null) | Must add null check for Process before accessing, returning Result.Fail if null |
| IShopSystem does not declare ShowShop()/UserShop() | IShopSystem.cs (only GetAvailableItems and Purchase) | Registration must resolve ShopSystem as concrete type or add methods to interface; design decision for tech-designer |
| Era.Core cannot reference engine | Architecture boundary (ENGINE.md Issue 29) | All engine delegation code must live in engine/ assembly, not Era.Core |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| GlobalStatic not initialized when delegation called | LOW | HIGH | Null check with descriptive Result.Fail message before each API call |
| CodeEE thrown from invalid state transition | MEDIUM | MEDIUM | try-catch CodeEE, convert to Result.Fail with state context |
| SaveLoadData sub-machine side effects | LOW | HIGH | Use thin delegation only (call existing engine methods), do not replicate internal logic |
| Entry point registration timing | MEDIUM | MEDIUM | Register after engine initialization completes; verify registration site in engine lifecycle |
| ShopSystem concrete type resolution for registration | LOW | MEDIUM | Design decision: use DI service resolution or direct type reference |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| GameStateImpl stub count | Grep "Stub: engine delegation pending F793" in GameStateImpl.cs | 3 | Lines 55, 61, 67 |
| IEntryPointRegistry registered handlers | Grep "Register" in engine/ for EntryPointRegistry usage | 0 | No registration calls exist in engine/ |
| Existing IGameState methods | Grep "Result<Unit>" in GameStateImpl.cs | 9 | 6 pre-existing + 3 new stubs |

**Baseline File**: `.tmp/baseline-793.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | BeginTrain must only do SetBegin("TRAIN"), not UpdateInBeginTrain or full EVENTTRAIN (CON-003) | Process.State.cs:264-312; Investigation 3/3 agreement; CON-003 resolution | AC must verify SetBegin call exists and UpdateInBeginTrain/EVENTTRAIN do NOT exist |
| C2 | SaveLoad requires two-step pattern: saveCurrentState then SaveLoadData | Instraction.Child.cs:1745-1747 | AC must verify both calls present in correct order |
| C3 | State guard exceptions (CodeEE) must be caught in BeginTrain and converted to Result.Fail | Process.State.cs:204-234 | AC must verify try-catch pattern wrapping engine calls |
| C4 | Null safety for GlobalStatic.Process | GlobalStatic.cs:283-287 (Reset() sets to null) | AC must verify null check for Process before delegation |
| C5 | ShowShop/UserShop not on IShopSystem interface | IShopSystem.cs declares only GetAvailableItems/Purchase | AC for entry point registration must account for concrete type resolution |
| C6 | Pre-existing IGameState methods must remain unchanged | GameStateImpl.cs:14-50 (6 methods) | AC must verify existing methods not modified (backward compatibility) |
| C7 | CharacterManagerImpl try-catch-Result pattern is the established precedent | CharacterManagerImpl.cs:14-25 | AC should verify new implementations follow same pattern |
| C8 | Caller responsible for valid game state before SaveGameDialog/LoadGameDialog | Process.ScriptProc.cs:982; Instraction.Child.cs:1738 | No AC needed — caller responsibility, not implementation concern |

### Constraint Details

**C1: BeginTrain Scope Limitation**
- **Source**: All 3 investigations agree. Process.State.cs Begin() method dispatches based on BeginType; EVENTTRAIN is triggered by engine's event pipeline (callFunction), not by GameStateImpl.
- **Verification**: Grep GameStateImpl.cs for "SetBegin" (should match) and "UpdateInBeginTrain" (should NOT match per CON-003); Grep for "EVENTTRAIN" or "callFunction" (should NOT match)
- **AC Impact**: Positive AC for delegation calls; negative AC confirming no EVENTTRAIN replication; negative AC confirming no UpdateInBeginTrain call (CON-003)

**C2: Two-Step SaveLoad Pattern**
- **Source**: Instraction.Child.cs:1745-1747 shows `Process.saveCurrentState(true)` immediately before `Process.getCurrentState.SaveLoadData(isSave)`. All 3 investigations identify this pattern.
- **Verification**: Read Instraction.Child.cs:1745-1747 to confirm pattern
- **AC Impact**: AC must verify both saveCurrentState and SaveLoadData calls present in SaveGameDialog/LoadGameDialog methods

**C3: CodeEE Exception Handling (BeginTrain only)**
- **Source**: Process.State.cs:201 throws CodeEE for invalid BEGIN keyword; Process.State.cs:215-233 throws CodeEE when __CAN_BEGIN__ flag not set. SaveLoadData() (line 236-244) sets enum only, never throws. saveCurrentState() (line 977-987) clones state only.
- **Verification**: Grep Process.State.cs for "throw new CodeEE" within SetBegin
- **AC Impact**: BeginTrain try-catch catches CodeEE from SetBegin. SaveGameDialog/LoadGameDialog try-catch is purely defensive (CON-002).

**C4: Null Safety**
- **Source**: GlobalStatic.cs:283-287 shows Reset() sets Process to null. Process.getCurrentState is guaranteed non-null when Process is non-null in normal operation (initialized in Process.Initialize()). Any edge-case NRE is caught by the try-catch.
- **Verification**: Grep GlobalStatic.cs Reset() method
- **AC Impact**: AC must verify null check for Process before delegation

**C5: IShopSystem Interface Gap**
- **Source**: IShopSystem.cs declares only GetAvailableItems(ShopId) and Purchase(CharacterId, ItemId). ShowShop() and UserShop() are on concrete ShopSystem class only. All 3 investigations identify this gap.
- **Verification**: Read IShopSystem.cs interface definition
- **AC Impact**: Entry point registration AC must account for how ShowShop/UserShop are accessed (concrete type, interface extension, or new interface)

**C6: Backward Compatibility**
- **Source**: GameStateImpl.cs has 6 pre-existing methods (SaveGame, LoadGame, Quit, Restart, ResetData, SetVariable) that must not be modified.
- **Verification**: Grep for existing method signatures
- **AC Impact**: count_equals or similar AC verifying pre-existing method count unchanged

**C7: Established Delegation Pattern (try-catch-Result only)**
- **Source**: CharacterManagerImpl.cs:14-25 demonstrates: try { GlobalStatic.API.Method(); return Result<Unit>.Ok(Unit.Value); } catch (Exception ex) { return Result<Unit>.Fail(message); }. Note: CharacterManagerImpl does NOT perform null checks on GlobalStatic fields — null safety (C4) is a separate concern derived from GlobalStatic.Reset() analysis, not from CharacterManagerImpl precedent.
- **Verification**: Read CharacterManagerImpl.cs
- **AC Impact**: New implementations should follow identical try-catch-Result pattern structure. Null checks (C4) are an additional requirement beyond this precedent.

**C8: Caller Responsibility for Game State Validity**
- **Source**: saveCurrentState() silently no-ops if state is null (Process.ScriptProc.cs:982). SaveLoadData() sets enum without precondition check (Process.State.cs:236-244). __CAN_SAVE__ enforced at instruction level (Instraction.Child.cs:1738), not in delegated methods.
- **Verification**: Caller (ShopSystem via Era.Core) must ensure valid game state before calling SaveGameDialog/LoadGameDialog
- **AC Impact**: No AC needed — caller responsibility documented, not implementation-level verification. GameStateImpl is a facade; precondition enforcement is caller's domain.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F791 | [DONE] | IGameState interface methods (BeginTrain, SaveGameDialog, LoadGameDialog) and stubs |
| Successor | F782 | [DRAFT] | Post-Phase Review Phase 20; depends on F793 engine delegation completion |
| Related | F774 | [DONE] | Shop Core - ShopSystem.cs calls IGameState methods |
| Related | F788 | [DONE] | IConsoleOutput Phase 20 Extensions - same Phase 20 migration |
| Related | F789 | [DONE] | IVariableStore Phase 20 Extensions - same Phase 20 migration |
| Related | F790 | [DONE] | Engine Data Access Layer - same Phase 20 migration |

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

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "GameStateImpl.cs is the single source of truth for engine-side state operations" | All 3 stub methods must be replaced with real engine delegation | AC#1, AC#2, AC#3, AC#4, AC#5, AC#10, AC#11, AC#12 |
| "Each Phase 20 interface method must have a working engine implementation" | BeginTrain, SaveGameDialog, LoadGameDialog must delegate to actual engine APIs (not stubs) | AC#2, AC#3, AC#4, AC#5, AC#6, AC#10, AC#11, AC#12 |
| "Engine-side delegation ready for F782 DI wiring" | Zero F793 stubs remaining in GameStateImpl after implementation | AC#1 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | No stub Result.Fail in new methods (Neg) | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs) | not_matches | Fail\("Stub: | [x] |
| 2 | BeginTrain delegates to SetBegin (Pos) | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs) | matches | SetBegin.*TRAIN | [x] |
| 3 | BeginTrain does NOT call UpdateInBeginTrain (Neg) | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs) | not_matches | UpdateInBeginTrain | [x] |
| 4 | Try-catch in all 3 new methods (Pos) | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs) | count_equals | `catch` = 4 | [x] |
| 5 | Null check for GlobalStatic.Process in all 3 methods (Pos) | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs) | count_equals | `GlobalStatic\.Process\s*==\s*null\|GlobalStatic\.Process\s+is\s+null` = 3 | [x] |
| 6 | Pre-existing 6 IGameState methods unchanged (Pos) | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs) | count_equals | `public Result<Unit> (SaveGame\|LoadGame\|Quit\|Restart\|ResetData\|SetVariable)\(` = 6 | [x] |
| 7 | C# build succeeds | build | dotnet build engine/uEmuera.Headless.csproj | succeeds | - | [x] |
| 8 | Engine unit tests pass | test | dotnet test engine.Tests/ | succeeds | - | [x] |
| 9 | No technical debt in GameStateImpl (Neg) | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs) | not_matches | TODO\|FIXME\|HACK | [x] |
| 10 | LoadGameDialog calls saveCurrentState (Pos) | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs) | count_equals | `saveCurrentState\(true\)` = 2 | [x] |
| 11 | SaveGameDialog two-step order (Pos) | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs, multiline=true) | matches | SaveGameDialog[\s\S]*?saveCurrentState\(true\)[\s\S]*?SaveLoadData\(true\) | [x] |
| 12 | LoadGameDialog two-step order (Pos) | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs, multiline=true) | matches | LoadGameDialog[\s\S]*?saveCurrentState\(true\)[\s\S]*?SaveLoadData\(false\) | [x] |
| 13 | Pre-existing method stubs preserved (Pos) | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs) | count_equals | `no engine API available\|not supported via API\|awaiting Phase 21` = 5 | [x] |
| 14 | SetBegin called exactly once (Pos) | code | Grep(engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs) | count_equals | `SetBegin` = 1 | [x] |

### AC Details

**AC#1: No stub Result.Fail in new methods (Neg)**
- **Test**: Grep pattern=`Fail\("Stub:` path=`engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs`
- **Expected**: 0 matches (currently matches 3 times -- non-vacuous)
- **Rationale**: Broader check ensuring no stub-prefixed failure messages remain in the file. Catches partial replacements. [C1, C2]

**AC#2: BeginTrain delegates to SetBegin (Pos)**
- **Test**: Grep pattern=`SetBegin.*TRAIN` path=`engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs`
- **Expected**: 1+ matches (currently 0 -- RED state)
- **Rationale**: Per Goal 1, BeginTrain() must call SetBegin("TRAIN") for state transition. Limited to state transition only, NOT variable reset (UpdateInBeginTrain) or full EVENTTRAIN pipeline. [C1, CON-003]

**AC#3: BeginTrain does NOT call UpdateInBeginTrain (Neg)**
- **Test**: Grep pattern=`UpdateInBeginTrain` path=`engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs`
- **Expected**: 0 matches (CON-003 resolution: engine state machine handles variable resets via beginTrain(), not GameStateImpl)
- **Rationale**: Per CON-003 resolution, BeginTrain must NOT directly call UpdateInBeginTrain to avoid double variable reset execution. Engine's beginTrain() handles this via state machine dispatch. [C1, CON-003]

**AC#4: Try-catch in all 3 new methods (Pos)**
- **Test**: Grep pattern=`catch` path=`engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs` | count
- **Expected**: 4 (baseline: 1 from Quit method at line 31; new methods BeginTrain + SaveGameDialog + LoadGameDialog each add 1 = 4 total)
- **Baseline verification**: Grep `catch` in current GameStateImpl.cs must return exactly 1 before implementation. If baseline differs, adjust Expected = baseline + 3.
- **Rationale**: Per C3 (BeginTrain: catches CodeEE from SetBegin state guard) and C7. SaveGameDialog/LoadGameDialog try-catch is defensive only (CON-002: neither SaveLoadData nor saveCurrentState throws CodeEE). Follows CharacterManagerImpl precedent. Count verification is complemented by AC#2 and AC#10-12 which verify per-method delegation calls (requiring try-catch wrapping for Result<Unit> return). [C3, C7]

**AC#5: Null check for GlobalStatic.Process in all 3 methods (Pos)**
- **Test**: Grep pattern=`GlobalStatic\.Process\s*==\s*null|GlobalStatic\.Process\s+is\s+null` path=`engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs` | count
- **Expected**: 3 (currently 0 -- RED state; BeginTrain + SaveGameDialog + LoadGameDialog each check Process for null)
- **Rationale**: Per C4, GlobalStatic.Reset() sets Process to null. All 3 new methods call Process and must check for null first. count_equals=3 ensures per-method null safety (matches alone would pass with only 1 check). [C4]

**AC#6: Pre-existing 6 IGameState methods unchanged (Pos)**
- **Test**: Grep pattern=`public Result<Unit> (SaveGame|LoadGame|Quit|Restart|ResetData|SetVariable)\(` path=`engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs` | count
- **Expected**: 6 (baseline: 6 pre-existing method signatures must remain intact)
- **Rationale**: Per C6, the pre-existing 6 IGameState methods must remain unchanged for backward compatibility. count_equals ensures none are accidentally removed or duplicated. [C6]

**AC#7: C# build succeeds**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build engine/uEmuera.Headless.csproj'`
- **Expected**: Build succeeds (exit code 0)
- **Rationale**: All code changes must compile successfully. TreatWarningsAsErrors is enabled.

**AC#8: Engine unit tests pass**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test engine.Tests/'`
- **Expected**: All tests pass (exit code 0)
- **Rationale**: Regression prevention for engine-level changes.

**AC#9: No technical debt in GameStateImpl (Neg)**
- **Test**: Grep pattern=`TODO|FIXME|HACK` path=`engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs`
- **Expected**: 0 matches
- **Rationale**: Zero debt upfront principle. No technical debt markers should be introduced during implementation.

**AC#10: LoadGameDialog calls saveCurrentState (Pos)**
- **Test**: Grep pattern=`saveCurrentState\(true\)` path=`engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs` | count
- **Expected**: 2 (currently 0 -- RED state; SaveGameDialog + LoadGameDialog each call saveCurrentState(true))
- **Rationale**: Per C2, the two-step pattern (saveCurrentState THEN SaveLoadData) applies to BOTH SaveGameDialog and LoadGameDialog. count_equals=2 ensures both SaveGameDialog and LoadGameDialog include the saveCurrentState(true) call. [C2]

**AC#11: SaveGameDialog two-step order (Pos)**
- **Test**: Grep pattern=`SaveGameDialog[\s\S]*?saveCurrentState\(true\)[\s\S]*?SaveLoadData\(true\)` path=`engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs` multiline=true
- **Expected**: 1+ matches (currently 0 -- RED state)
- **Rationale**: Per C2, saveCurrentState(true) must be called BEFORE SaveLoadData(true) in SaveGameDialog. Pattern anchored to method name to prevent cross-method boundary matching. [C2]

**AC#12: LoadGameDialog two-step order (Pos)**
- **Test**: Grep pattern=`LoadGameDialog[\s\S]*?saveCurrentState\(true\)[\s\S]*?SaveLoadData\(false\)` path=`engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs` multiline=true
- **Expected**: 1+ matches (currently 0 -- RED state)
- **Rationale**: Per C2, saveCurrentState(true) must be called BEFORE SaveLoadData(false) in LoadGameDialog. Pattern anchored to method name to prevent cross-method boundary matching. [C2]

**AC#13: Pre-existing method stubs preserved (Pos)**
- **Test**: Grep pattern=`no engine API available|not supported via API|awaiting Phase 21` path=`engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs` | count
- **Expected**: 5 (SaveGame "no engine API available" line 16, LoadGame "no engine API available" line 21, Restart "not supported via API" line 39, ResetData "no engine API available" line 44, SetVariable "awaiting Phase 21" line 49)
- **Rationale**: Per C6, pre-existing method bodies must remain unchanged during F793 implementation. AC#6 verifies signature count but not body content. This AC ensures the 5 stub Fail messages in pre-existing methods are not accidentally modified. Pattern uses specific message fragments to avoid false positives from new implementation code. [C6]

**AC#14: SetBegin called exactly once (Pos)**
- **Test**: Grep pattern=`SetBegin` path=`engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs` | count
- **Expected**: 1 (currently 0 -- RED state; non-vacuous complement to AC#2)
- **Rationale**: Per C1, BeginTrain must call SetBegin("TRAIN") exactly once for state transition. Per C1, scope is limited to state transition only. count_equals=1 provides non-vacuous scope creep protection by ensuring only one state transition call exists. More than 1 SetBegin call would indicate additional unauthorized state transitions beyond the scoped BeginTrain operation. [C1]

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | BeginTrain() delegates to SetBegin("TRAIN") | AC#1, AC#2, AC#3, AC#4, AC#5, AC#14 |
| 2 | SaveGameDialog() delegates to saveCurrentState(true) then SaveLoadData(true) | AC#1, AC#4, AC#5, AC#10, AC#11 |
| 3 | LoadGameDialog() delegates to saveCurrentState(true) then SaveLoadData(false) | AC#1, AC#4, AC#5, AC#10, AC#12 |

---

<!-- fc-phase-4-completed -->

## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

Implement engine-side delegation in GameStateImpl.cs following the established CharacterManagerImpl pattern (try-catch-Result). GameStateImpl serves as an **orchestration facade** for IGameState operations — a single method may coordinate multiple engine subsystems (e.g., BeginTrain calls both VEvaluator for variable resets and Process for state transition). This is intentional: IGameState represents high-level game operations, and the implementation encapsulates the multi-subsystem coordination required by each operation.

Each method will:
1. Check GlobalStatic.Process for null (return Fail if null)
2. Delegate to appropriate engine APIs within try-catch
3. Convert exceptions to Result.Fail with descriptive messages
4. Return Result.Ok on success

This approach satisfies all 14 ACs:
- AC#1: Stub removal verified via negative grep
- AC#2: Engine delegation verified via positive grep for SetBegin call
- AC#3: CON-003 guard verified via negative grep (no UpdateInBeginTrain)
- AC#4-5: Try-catch and null check pattern verified via grep/count
- AC#6: Backward compatibility verified via count_equals
- AC#7: Build gate
- AC#8: Test gate
- AC#9: Zero debt verified via negative grep
- AC#10: saveCurrentState count verified via count_equals
- AC#11-12: Two-step ordering verified via multiline grep
- AC#13: Pre-existing method stub preservation verified via count_equals
- AC#14: SetBegin scope guard verified via count_equals=1

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | No stub Fail messages remain → grep no longer matches `Fail\("Stub:` |
| 2 | BeginTrain() calls `GlobalStatic.Process.getCurrentState.SetBegin("TRAIN")` → grep matches SetBegin.*TRAIN |
| 3 | BeginTrain does NOT call UpdateInBeginTrain → grep not_matches UpdateInBeginTrain (CON-003 guard) |
| 4 | Each new method has try-catch block → grep count=4 (1 existing in Quit + 3 new) |
| 5 | BeginTrain/SaveGameDialog/LoadGameDialog check GlobalStatic.Process for null → grep count_equals=3 (one per method) |
| 6 | Pre-existing 6 methods (SaveGame, LoadGame, Quit, Restart, ResetData, SetVariable) remain unchanged → grep count=6 |
| 7 | Implementation compiles without errors/warnings → dotnet build succeeds |
| 8 | Engine unit tests pass → dotnet test engine.Tests/ succeeds |
| 9 | No TODO/FIXME/HACK markers introduced → grep not_matches TODO\|FIXME\|HACK |
| 10 | SaveGameDialog + LoadGameDialog both call saveCurrentState(true) → count_equals 2 |
| 11 | SaveGameDialog: saveCurrentState(true) appears before SaveLoadData(true) → multiline grep matches |
| 12 | LoadGameDialog: saveCurrentState(true) appears before SaveLoadData(false) → multiline grep matches |
| 13 | Pre-existing method stub messages preserved (5 specific messages) → count_equals 5 |
| 14 | SetBegin called exactly once in GameStateImpl → count_equals SetBegin=1 (non-vacuous C1 scope guard) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| SaveLoad state backup timing | (A) saveCurrentState within method, (B) caller responsibility | A: saveCurrentState within method | Per C2 constraint, two-step pattern (saveCurrentState then SaveLoadData) is established precedent from Instraction.Child.cs:1745-1747; encapsulating both steps in GameStateImpl ensures correct usage |
| Null check scope | (A) Single check for GlobalStatic.Process, (B) No null check (rely on try-catch) | A: Single check for GlobalStatic.Process | GlobalStatic.Reset() sets Process to null (GlobalStatic.cs:283-287). Explicit null check provides descriptive Result.Fail message. Process.getCurrentState is guaranteed non-null when Process is non-null in normal operation (initialized in Process.Initialize()). Any edge-case NRE is caught by the try-catch. |

### Interfaces / Data Structures

**GameStateImpl Method Implementations** (engine/Assets/Scripts/Emuera/Services/GameStateImpl.cs):

Replace stubs (lines 52-68) with:

```csharp
/// <summary>Begin training mode state transition. Delegates to GlobalStatic engine APIs.</summary>
public Result<Unit> BeginTrain()
{
    if (GlobalStatic.Process == null)
        return Result<Unit>.Fail("Cannot begin training: Process not initialized");

    try
    {
        // State transition only — engine's beginTrain() handles variable resets
        // (UpdateInBeginTrain) via state machine dispatch (not full EVENTTRAIN)
        GlobalStatic.Process.getCurrentState.SetBegin("TRAIN");

        return Result<Unit>.Ok(Unit.Value);
    }
    catch (Exception ex)
    {
        return Result<Unit>.Fail($"Failed to begin training: {ex.Message}");
    }
}

/// <summary>Open dialog-mode save game UI. Delegates to GlobalStatic engine APIs.</summary>
public Result<Unit> SaveGameDialog()
{
    if (GlobalStatic.Process == null)
        return Result<Unit>.Fail("Cannot open save dialog: Process not initialized");

    try
    {
        // Two-step pattern per C2 (Instraction.Child.cs:1745-1747)
        // Step 1: Backup current ProcessState
        GlobalStatic.Process.saveCurrentState(true);

        // Step 2: Enter save dialog state machine (14-state sub-machine)
        GlobalStatic.Process.getCurrentState.SaveLoadData(true); // isSave=true

        return Result<Unit>.Ok(Unit.Value);
    }
    catch (Exception ex)
    {
        return Result<Unit>.Fail($"Failed to open save dialog: {ex.Message}");
    }
}

/// <summary>Open dialog-mode load game UI. Delegates to GlobalStatic engine APIs.</summary>
public Result<Unit> LoadGameDialog()
{
    if (GlobalStatic.Process == null)
        return Result<Unit>.Fail("Cannot open load dialog: Process not initialized");

    try
    {
        // Two-step pattern per C2 (Instraction.Child.cs:1745-1747)
        // Step 1: Backup current ProcessState
        GlobalStatic.Process.saveCurrentState(true);

        // Step 2: Enter load dialog state machine
        GlobalStatic.Process.getCurrentState.SaveLoadData(false); // isSave=false

        return Result<Unit>.Ok(Unit.Value);
    }
    catch (Exception ex)
    {
        return Result<Unit>.Fail($"Failed to open load dialog: {ex.Message}");
    }
}
```

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 7 | Create unit tests for GameStateImpl delegation (TDD RED) | | [x] |
| 2 | 2,3,4,5,14 | Implement GameStateImpl.BeginTrain() with engine delegation | | [x] |
| 3 | 4,5,10,11 | Implement GameStateImpl.SaveGameDialog() with engine delegation | | [x] |
| 4 | 4,5,10,12 | Implement GameStateImpl.LoadGameDialog() with engine delegation | | [x] |
| 5 | 1,6,7,8,9,13 | Build verification + test execution (TDD GREEN) | | [x] |

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

**Example**:
```markdown
| 1 | 1 | Add API endpoint | | [ ] |        ← KNOWN: Expected response format is specified
| 2 | 2 | Calculate aggregates | [I] | [ ] | ← UNCERTAIN: Actual totals unknown until implemented
| 3 | 3 | Format report | | [ ] |           ← KNOWN: Uses Task 2's output (determined after Task 2)
```

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

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | ac-tester | sonnet | AC#7 | Unit tests for GameStateImpl delegation (TDD RED) |
| 2 | implementer | sonnet | Task 2 | GameStateImpl.BeginTrain() engine delegation |
| 3 | implementer | sonnet | Task 3 | GameStateImpl.SaveGameDialog() engine delegation |
| 4 | implementer | sonnet | Task 4 | GameStateImpl.LoadGameDialog() engine delegation |
| 5 | implementer | sonnet | Task 5 | Build + test verification (TDD GREEN) |

### Execution Order

**Task 1 (Phase 1)**: TDD RED - Create unit tests for GameStateImpl delegation methods. Tests will verify null checks, exception handling, and delegation patterns. Tests will fail until implementation (Tasks 2-4) completes.

**Tasks 2-4 (Phases 2-4)**: Implementation of 3 GameStateImpl methods (can be parallelized by method).

**Task 5 (Phase 5)**: Build + test verification (blocking gate — TDD GREEN state, all tests pass).

### Build Verification

**Command** (via WSL):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build engine/uEmuera.Headless.csproj'
```

**Success Criteria**: Exit code 0, zero warnings (TreatWarningsAsErrors=true enforced).

### Test Requirements

**Test File**: Create in `engine.Tests/Services/`
- `GameStateImplTests.cs` - Test GameStateImpl.BeginTrain/SaveGameDialog/LoadGameDialog delegation with null checks and exception handling

**Pattern**: Use existing test patterns from engine.Tests/ for GlobalStatic-dependent service tests.

### Code Snippets

All code snippets are provided in Technical Design section. Snippets include:
- GameStateImpl method implementations (3 methods)

### Error Handling

**Technical Constraints**:
- All GameStateImpl methods return `Result<Unit>` (C7 precedent)
- Null checks required for GlobalStatic.Process (C4)
- CodeEE exceptions must be caught and converted to Result.Fail (C3)
- Try-catch pattern follows CharacterManagerImpl precedent (C7)

### Success Criteria

- All 14 ACs pass
- Build succeeds with 0 warnings (via WSL)
- Engine unit tests pass (dotnet test engine.Tests/ via WSL)
- Zero F793 stub messages in GameStateImpl.cs (AC#1)
- Zero TODO/FIXME/HACK in modified files (AC#9)

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Entry point registration (SHOW_SHOP/USERSHOP handlers + GlobalStatic.EntryPointRegistry property) | Deferred from F793; requires DI resolution chain available only in F782 Post-Phase Review | Feature | F782 | - |
| Dispatch consumer wiring (Process.SystemProc → IEntryPointRegistry.Invoke) | Excluded from F793 scope; requires modifying engine dispatch to check IEntryPointRegistry before ERB fallback | Feature | F782 | - |
| DI wiring override (ShopSystem resolves GameStateImpl instead of Era.Core GameState stub) | GameStateImpl delegation has no runtime effect until DI container wires GameStateImpl as IGameState | Feature | F782 | - |

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
| 2026-02-16 09:44 | START | implementer | Task 1 | - |
| 2026-02-16 09:44 | END | implementer | Task 1 | SUCCESS |
| 2026-02-16 09:47 | START | implementer | Tasks 2-4 | - |
| 2026-02-16 09:47 | END | implementer | Tasks 2-4 | SUCCESS |
| 2026-02-16 | DEVIATION | ac-static-verifier | AC#3 code grep | AC#3 FAIL: comment on line 61 mentions UpdateInBeginTrain, triggering not_matches false positive |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] POST-LOOP iter1: [SCP-004] Created F793 [DRAFT] via FL POST-LOOP Step 6.3 Review Context template (F791 Philosophy Gate deferral materialization)
- [fix] Phase2-Review iter1: [AC-005] AC Definition Table / Goal Coverage | Added AC#18 (LoadGameDialog calls saveCurrentState count_equals=2) to cover C2 two-step pattern gap in Goal 3
- [fix] Phase2-Review iter1: [FMT-002] AC#9 | Updated description from 'Try-catch pattern in BeginTrain' to 'Try-catch in all 3 new methods', added baseline verification note to AC Details
- [fix] Phase2-Review iter1: [AC-001] AC#9 AC Details | Fixed title and added baseline verification step to address count fragility
- [fix] Phase2-Review iter2: [TSK-002] Tasks table Task#4 | Added AC#18 to Task#4 AC# column
- [fix] Phase2-Review iter2: [FMT-002] Success Criteria | Updated AC count from 17 to 20
- [fix] Phase2-Review iter2: [AC-005] Philosophy / Philosophy Derivation | Added third absolute claim for IEntryPointRegistry + derivation row mapping to AC#13,14
- [fix] Phase2-Uncertain iter2: [FMT-001] Mandatory Handoffs | Added template comments (CRITICAL, Option A/B/C, Validation)
- [fix] Phase2-Review iter2: [AC-005] AC Definition Table / Goal Coverage | Added AC#19 (SaveGameDialog two-step order) and AC#20 (LoadGameDialog two-step order) for C2 ordering verification
- [fix] Phase2-Review iter3: [FMT-002] Mandatory Handoffs | Replaced plaintext '(none)' with 5-column table format per template
- [fix] Phase2-Review iter3: [FMT-002] Review Notes | Added category codes to all entries per error-taxonomy.md
- [fix] Phase2-Review iter4: [AC-005] AC Definition Table / Goal Coverage | Added AC#21 (bootstrap invocation count_equals=2) to verify RegisterShopEntryPoints is both defined and called
- [fix] Phase2-Review iter4: [TSK-002] Tasks table | Added Task#8 [I] for wiring EntryPointBootstrap into engine initialization + Implementation Contract Phase 8
- [fix] Phase2-Review iter4: [AC-002] AC#19/AC#20 | Anchored multiline patterns to method names to prevent cross-method boundary false positives
- [resolved-applied] Phase2-Pending iter5: [AC-005] Philosophy narrowed: Removed 'end-to-end without runtime stubs' claim. Now states 'engine-side delegation ready for F782 DI wiring'. Runtime effect explicitly noted as requiring F782.
- [fix] Phase2-Review iter5: [INV-003] AC#9 AC Details | Fixed baseline line reference from 37 to 31
- [fix] Phase2-Review iter6: [FMT-001] Review Notes | Added missing Tag rules and Category codes comment lines per template
- [fix] Phase2-Review iter6: [AC-005] AC Definition Table | Added AC#22 (pre-existing method stubs preserved count_equals=5) for C6 body preservation
- [fix] Phase2-Review iter7: [FMT-002] AC Details | Swapped AC#21 and AC#22 detail blocks to restore sequential ordering
- [fix] Phase2-Review iter7: [AC-001] AC#22 | Fixed expected count from 4 to 5 (SaveGame, LoadGame, Restart, ResetData, SetVariable = 5 stubs)
- [fix] Phase2-Review iter7: [FMT-002] Implementation Contract | Reordered phases so Task 8 (wire bootstrap) runs before Task 7 (build gate)
- [fix] PostLoop-UserFix post-loop-2: [CON-001] Removed entry point registration (SHOW_SHOP/USERSHOP handlers, EntryPointBootstrap.cs, GlobalStatic.EntryPointRegistry property) from F793 scope; moved Goal 4 to F782 as Mandatory Handoff per CON-001/AC-005 fix instructions
- [fix] Phase3-Maintainability iter8: [INV-003] Key Decisions | Corrected null check granularity option B: getCurrentState is constructor-initialized, never null when Process is non-null, no separate null check needed
- [fix] Phase2-Review iter9: [FMT-001] Mandatory Handoffs | Added full Validation (FL PHASE-7) multi-line block + DRAFT Creation Checklist per template
- [fix] Phase3-Maintainability iter10: [AC-004] AC#17 (formerly AC#22) | Tightened pattern from 'not implemented|not supported|awaiting Phase 21' to 'no engine API available|not supported via API|awaiting Phase 21' to prevent false positives from new code
- [fix] Phase2-Review iter2: [FMT-001] Removed non-standard `## Created: 2026-02-15` section (date already in Review Context Origin Timestamp field)
- [fix] Phase2-Review iter3: [TSK-003] Implementation Contract | Swapped Phase 5 (Task 5) and Phase 6 (Task 6) to fix compilation dependency: EntryPointBootstrap references GlobalStatic.EntryPointRegistry which must exist first
- [resolved-applied] Phase2-Pending iter6: [CON-003] Double UpdateInBeginTrain execution: User chose Option A — removed UpdateInBeginTrain() from GameStateImpl.BeginTrain(), SetBegin("TRAIN") only. Removed AC#3 (UpdateInBeginTrain check) and AC#11 (VEvaluator null check). ACs renumbered 1-24.
- [resolved-applied] Phase2-Pending iter6: [CON-004] Non-atomic BeginTrain: Subsumed by CON-003 resolution — UpdateInBeginTrain removed from GameStateImpl, eliminating atomicity concern.
- [fix] Phase3-Maintainability iter6: [PHI-002] Philosophy | Narrowed entry point registration scope to match Goal (pre-CON-001 fix)
- [fix] Phase3-Maintainability iter7: [LKP-001] Mandatory Handoffs | Added 2 handoff rows: dispatch consumer wiring (→F782) and DI wiring override (→F782) for deferred scope items (pre-CON-001 fix)
- [fix] Phase1-RefCheck iter1: [LNK-001] F782 Links | Added F793 cross-reference to F782's Links section
- [fix] Phase4-ACValidation iter1: [AC-004] AC#9,12,18,21,22,26 (now #9,12,18,17,18) | Added backtick wrapping to count_equals Expected patterns per ac-static-verifier Format A
- [fix] Phase3-Maintainability iter1: [DOC-002] C7 Constraint Details | Clarified C7 covers try-catch-Result pattern only; null safety (C4) is separate concern not from CharacterManagerImpl precedent
- [fix] Phase2-Review iter4: [AC-004] AC#10 (now #10) | Changed from matches to count_equals=3 to ensure per-method Process null check in all 3 new methods (BeginTrain + SaveGameDialog + LoadGameDialog)
- [fix] PostLoop-UserFix post-loop: [CON-003] Goal 1, AC table, AC Details, AC Coverage, Technical Design, Task#2, Philosophy | Removed UpdateInBeginTrain from BeginTrain design; removed AC#3 (UpdateInBeginTrain check) and AC#11 (VEvaluator null check); ACs renumbered 1-24
- [resolved-applied] Phase3-Maintainability iter8: [CON-001] ShopSystem unreachable from engine/: Resolved by removing Goal 4 (entry point registration) from F793 scope. Moved to F782 as Mandatory Handoff.
- [resolved-applied] Phase3-Maintainability iter8: [AC-005] Dead code registration: Resolved by removing Goal 4. Entry point registration no longer in F793 scope.
- [resolved-applied] Phase2-Pending iter2: [AC-006] Consolidated: removed AC#1 (⊂AC#2), AC#4 (⊂AC#14+15), AC#5 (⊂AC#15), AC#6 (⊂AC#16), AC#7 (vacuous, ⊂AC#18). 18→13 ACs (within 8-15 limit).
- [resolved-applied] Phase2-Pending iter3: [CON-002] Narrowed C3 to BeginTrain only. SaveLoadData() sets enum (line 236-244), saveCurrentState() clones state (line 977-987) — neither throws CodeEE. SaveGameDialog/LoadGameDialog try-catch is purely defensive (NPE if state null).
- [resolved-applied] Phase2-Pending iter6: [AC-007] Accepted fragility. Per-method delegation verified by AC#3/15/16. Try-catch structurally required for Result<Unit> from void engine methods. Misdistribution structurally impossible.
- [resolved-applied] Phase3-Maintainability iter6: [LKP-001] Upstream Issues dangling reference: Resolved — Upstream Issues section removed as part of Goal 4 cleanup (entry point registration issues no longer relevant).
- [resolved-applied] Phase3-Maintainability iter6: [CON-005] Caller responsibility. saveCurrentState() handles null state silently (line 982). SaveLoadData() sets enum only. __CAN_SAVE__ enforced at instruction level, not in delegated methods. Added C8 constraint.
- [resolved-applied] Phase4-ACAlignment iter1: [TSK-002] All blockers resolved (CON-002→B, CON-005→B, AC-006→A, AC-007→B).
- [fix] Phase2-Review iter1: [FMT-002] AC Definition Table | Renumbered all ACs sequentially 1-14 (was non-sequential 2,3,8-18 with gaps at 1,4,5,6,7 from post-consolidation). Updated all cross-references: Philosophy Derivation, AC Details, Goal Coverage, AC Coverage, Tasks, Implementation Contract.
- [fix] Phase2-Review iter1: [TSK-002] Tasks table | Fixed stale AC# references: Task#3 removed non-existent AC#4,5 → AC#4,5,10,11; Task#4 removed non-existent AC#6 → AC#4,5,10,12 (post-consolidation cleanup)
- [fix] Phase2-Review iter1: [AC-004] AC Details | Fixed AC Details heading numbering to match AC Definition Table; fixed AC#10 stale cross-ref to removed AC#5; fixed AC#14 stale cross-ref to old AC#8; added AC#8 Details block
- [fix] Phase2-Review iter1: [AC-005] AC Definition Table / Goal Coverage | Added AC#3 (not_matches UpdateInBeginTrain) as negative CON-003 guard; updated Goal 1 coverage, Task#2 AC#, Approach, Success Criteria
- [fix] Phase2-Review iter1: [CON-001] C1 Constraint table | Updated description from 'BeginTrain must only do UpdateInBeginTrain + SetBegin' to 'SetBegin only, not UpdateInBeginTrain' per CON-003 resolution
- [fix] Phase2-Review iter1: [DOC-002] Code Snippets | Removed stale references to EntryPointBootstrap.cs and GlobalStatic.EntryPointRegistry (moved to F782)
- [fix] Phase2-Review iter2: [DOC-002] Root Cause Analysis Fix cell | Removed stale VEvaluator.UpdateInBeginTrain() reference per CON-003 resolution
- [fix] Phase2-Review iter2: [CON-001] C4, Technical Constraints, Error Handling, Key Decisions, Approach | Removed all stale VEvaluator null check references post-CON-003 (no F793 method accesses VEvaluator)
- [fix] Phase2-Uncertain iter2: [DOC-002] Feasibility Assessment | Removed stale VEvaluator API Accessibility and UpdateInBeginTrain Method Signatures references post-CON-003
- [fix] Phase3-Maintainability iter3: [DOC-002] Title | Removed 'Entry Point Registration' from title (moved to F782 per CON-001). Updated index-features.md
- [fix] Phase3-Maintainability iter3: [AC-004] AC#6 Details | Fixed pattern from `\|` to `|` for ripgrep alternation (backslash-pipe is literal in ripgrep)
- [fix] Phase3-Maintainability iter3: [DOC-002] AC#2 Rationale | Updated 'variable reset + state transition' to 'state transition only' per CON-003

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F791](feature-791.md) - IGameState mode transitions + IEntryPointRegistry procedure dispatch
- [Successor: F782](feature-782.md) - Post-Phase Review Phase 20
- [Related: F774](feature-774.md) - Shop Core (SHOP.ERB + SHOP2.ERB) - ShopSystem calls IGameState methods
- [Related: F788](feature-788.md) - IConsoleOutput Phase 20 Extensions
- [Related: F789](feature-789.md) - IVariableStore Phase 20 Extensions
- [Related: F790](feature-790.md) - Engine Data Access Layer
