# Feature 380: INFO.ERB Migration - SHOW_STATUS Orchestration

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate SHOW_STATUS orchestration function from INFO.ERB to C# StatusOrchestrator.cs. This is Part 5 (final) of INFO.ERB migration.

**Context**: Part of INFO.ERB split: F373 (Print), F381 (State), F382 (TrainMode Display), F378 (Event), F379 (Equipment), F380 (Orchestration). Depends on all previous INFO.ERB features.

---

## Background

### Philosophy (Mid-term Vision)

**Foundation-First Migration**: SHOW_STATUS is the main orchestrator function called every game turn. It coordinates character movement, attitude updates, event checks, and status display by calling functions from F373-F379 and external systems.

### Problem (Current Issue)

SHOW_STATUS function in INFO.ERB:
- SHOW_STATUS (lines 3-214, ~212 lines) - Main turn orchestrator

**SHOW_STATUS responsibilities** (complete list from INFO.ERB lines 3-210):

**Already migrated to C#**:
- Cooking timer check (CEACK_COOKING) → F378
- Private room reassignment (INFO_CheckMyRoom) → F378
- Character attitude (CHARA_ATTITUDE) → F381

**External ERB calls (not migrated)**:
- Character movement (CHARA_MOVEMENT)
- Inhabitant/visitor/intruder actions (INHABITANT_DO, VISITER_DO, INTRUDER_DO)
- Special event calls (SHOW_TOUCH, 従者E_進展, GoOut_SeeYou, NTR_COM416_BATHTIME_INTERRUPT, PREGNACY_S_EVENT)
- Clothes management (CLOTHES_RESET, CLOTHES_SETTING_TRAIN)
- Counter event orchestration (EVENT_COUNTER, EVENT_COUNTER_TRAIN, EVENT_WC_COUNTER, EVENT_WC_COUNTER_TRAIN, KOJO_MESSAGE_COUNTER)
- Weather updates (天候/weather system)

**Deferred to Phase 17**:
- Affair disclosure (AFFAIR_DISCLOSURE)

**Display calls (delegated to F373/F379/F382)**:
- INFO_Print* functions, SHOW_EQUIP*, DRAWLINE

**Current State**:
- SHOW_STATUS is the most complex function in INFO.ERB
- Calls functions from all INFO.ERB categories
- Calls external systems (movement, weather, inhabitants)
- **Mixed C#/ERB orchestration**: Some calls are to C# (F378), others remain ERB
- Must be migrated last after all dependencies complete

### Goal (What to Achieve)

1. Analyze SHOW_STATUS dependencies comprehensively
2. Create Era.Core/Orchestration/StatusOrchestrator.cs
3. Implement orchestration pattern with injected dependencies
4. Create xUnit test cases for orchestration flow
5. Document StatusOrchestrator API for Phase 4+ reference

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SHOW_STATUS analysis documented | file | Grep "## SHOW_STATUS Analysis" in Game/agents/feature-380.md | contains | "## SHOW_STATUS Analysis" | [x] |
| 2 | StatusOrchestrator.cs created | file | Glob Era.Core/Orchestration/StatusOrchestrator.cs | exists | Era.Core/Orchestration/StatusOrchestrator.cs | [x] |
| 3 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 4 | StatusOrchestrator tests created | file | Glob Era.Core.Tests/StatusOrchestratorTests.cs | exists | Era.Core.Tests/StatusOrchestratorTests.cs | [x] |
| 5 | All orchestration tests pass (Pos) | test | dotnet test --filter StatusOrchestratorTests | succeeds | - | [x] |
| 6 | Minimum test coverage exists | code | Grep "\\[Fact\\]" in Era.Core.Tests/StatusOrchestratorTests.cs (count mode) | gte | 8 | [x] |
| 7 | API documentation created | file | Grep "## StatusOrchestrator API" in Game/agents/feature-380.md | contains | "## StatusOrchestrator API" | [x] |
| 8 | Negative test for skip scenario exists | code | Grep "NoTarget\|WhenTargetSleeping" in Era.Core.Tests/StatusOrchestratorTests.cs | contains | "NoTarget" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze SHOW_STATUS: document "## SHOW_STATUS Analysis" section per categories in Background/Problem section (8 categories: C# calls, External ERB, Special events, Clothes, Counter events, Deferred, Display, Logic flow) | [x] |
| 2 | 2 | Create Era.Core/Orchestration/StatusOrchestrator.cs with dependency injection pattern (includes directory creation) | [x] |
| 3 | 3 | Verify C# build succeeds after StatusOrchestrator.cs creation | [x] |
| 4 | 4 | Create Era.Core.Tests/StatusOrchestratorTests.cs using xUnit patterns | [x] |
| 5 | 5 | Run all StatusOrchestrator tests and verify they pass | [x] |
| 6 | 6 | Verify test coverage: ensure minimum 8 test methods exist | [x] |
| 7 | 7 | Document StatusOrchestrator API with usage examples in feature-380.md | [x] |
| 8 | 8 | Add negative test for NoTarget/WhenTargetSleeping scenarios in StatusOrchestratorTests.cs | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F373 | Requires InfoPrint display functions |
| Predecessor | F381 | Requires InfoState state management |
| Predecessor | F382 | Requires InfoTrainModeDisplay for character list display |
| Predecessor | F378 | Requires InfoEvent event handling |
| Predecessor | F379 | Requires InfoEquip equipment display |
| Predecessor | F366 | Requires CommonFunctions.cs |
| Predecessor | F364 | Requires Constants.cs |
| Predecessor | F372 | Requires LocationSystem.cs for OPENPLACE equivalent (IsOpenPlace) |
| External | CHARA_MOVEMENT | Character movement system (not migrated) |
| External | INHABITANT_DO | Inhabitant action system (not migrated) |
| External | Weather system | Weather update functions (not migrated) |
| External | AFFAIR_DISCLOSURE | NTR affair disclosure (ERB, deferred to Phase 17) |

---

## Links

- [feature-373.md](feature-373.md) - Print Display (predecessor)
- [feature-381.md](feature-381.md) - State Management (predecessor)
- [feature-382.md](feature-382.md) - TrainMode Display (predecessor)
- [feature-378.md](feature-378.md) - Event Handling (predecessor)
- [feature-379.md](feature-379.md) - Equipment Display (predecessor)
- [feature-366.md](feature-366.md) - CommonFunctions (predecessor)
- [feature-364.md](feature-364.md) - Constants (predecessor)
- [feature-372.md](feature-372.md) - LocationSystem (predecessor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 17 (AFFAIR_DISCLOSURE migration)
- Game/ERB/INFO.ERB - Source file (lines 3-214)

---

## Mixed C#/ERB Orchestration Design

### Design Decision

**StatusOrchestrator.cs returns orchestration state; ERB wrapper handles external calls.**

StatusOrchestrator.cs does NOT call ERB functions directly. Instead, it:
1. Executes migrated C# functions (F373-F382)
2. Returns orchestration state/flags indicating which external actions are needed
3. ERB SHOW_STATUS wrapper interprets flags and calls external ERB functions

```
SHOW_STATUS (ERB wrapper) - Handles ALL external ERB calls
├── result = StatusOrchestrator.Execute()     ← C# orchestration
│
├── [Movement/Actions - always called]
│   ├── CHARA_MOVEMENT                        ← External ERB
│   ├── INHABITANT_DO                         ← External ERB
│   ├── VISITER_DO                            ← External ERB
│   └── INTRUDER_DO                           ← External ERB
│
├── [Special Events - conditional]
│   ├── IF condition → SHOW_TOUCH             ← External ERB
│   ├── IF condition → 従者E_進展              ← External ERB
│   ├── IF condition → GoOut_SeeYou           ← External ERB
│   ├── IF condition → NTR_COM416_BATHTIME_INTERRUPT  ← External ERB
│   └── IF condition → PREGNACY_S_EVENT       ← External ERB
│
├── [Clothes Management - conditional]
│   ├── IF condition → CLOTHES_RESET          ← External ERB
│   └── IF condition → CLOTHES_SETTING_TRAIN  ← External ERB
│
├── [Counter Events - conditional on COMABLE管理]
│   ├── IF COMABLE管理 == 3/4 → EVENT_COUNTER*      ← External ERB
│   ├── IF COMABLE管理 == 3/4 → EVENT_WC_COUNTER*   ← External ERB
│   └── IF condition → KOJO_MESSAGE_COUNTER   ← External ERB
│
└── [Deferred to Phase 17]
    └── IF result.NeedsAffairDisclosure → AFFAIR_DISCLOSURE  ← ERB (Phase 17)

StatusOrchestrator.cs (C#) - Returns OrchestrationResult
├── CheckCooking()               ← C# (F378)
├── CheckPrivateRoom()           ← C# (F378)
├── CalculateCharacterAttitude() ← C# (F381)
├── Display coordination         ← Delegates to F373/F379/F382
└── Returns OrchestrationResult with flags
```

**Scope Limitation**: StatusOrchestrator.cs handles orchestration logic and returns flags. The ERB wrapper remains responsible for calling external ERB functions. This is intentional - full migration of external calls (CHARA_MOVEMENT, INHABITANT_DO, etc.) is out of scope for F380.

### OrchestrationResult Structure

```csharp
public class OrchestrationResult
{
    // Core orchestration state
    public int ComableState { get; set; }        // COMABLE管理 value (0-4)
    public bool HasValidTarget { get; set; }     // Target selected and valid

    // Conditional call flag (ERB wrapper uses this to decide call)
    public bool NeedsAffairDisclosure { get; set; }  // Phase 17 deferred
    // Note: Movement/Actions (CHARA_MOVEMENT, INHABITANT_DO, VISITER_DO, INTRUDER_DO)
    // are always called by ERB wrapper - no flags needed

    // Display state (delegates to F373/F379/F382)
    public bool DisplayCompleted { get; set; }
}
```

### Why This Is Not Technical Debt (Until Phase 17)

1. **Intentional Design**: Mixed orchestration is the documented migration strategy, not an oversight
2. **Clear Boundary**: C# calls (migrated functions) vs ERB calls (external/deferred) are explicitly documented
3. **Testable**: StatusOrchestrator.cs can be unit tested by mocking ERB call interfaces
4. **Traceable**: Phase 17 will convert AFFAIR_DISCLOSURE ERB call to C# call

### Phase 17 Refactoring Requirement

After Phase 17 completes AFFAIR_DISCLOSURE migration:

1. **Update StatusOrchestrator.cs**: Replace ERB call with C# `CheckAffairDisclosure()` call
2. **Update Tests**: Add tests for new C# call path
3. **Remove ERB Dependency**: AFFAIR_DISCLOSURE can be removed from INFO.ERB External list
4. **Verify**: Full orchestration flow works with pure C#

**Reference**: See [feature-378.md](feature-378.md) "AFFAIR_DISCLOSURE Deferral Plan" for detailed transition path.

---

## StatusOrchestrator API

### Class: StatusOrchestrator

**Namespace**: `Era.Core.Orchestration`

**Purpose**: Orchestrate SHOW_STATUS logic, coordinate migrated C# functions, return flags for ERB wrapper.

### Method: Execute

```csharp
public static OrchestrationResult Execute(
    Func<int> getComableState,           // COMABLE管理 accessor
    Func<int> getTargetId,               // TARGET accessor
    Action<int> setComableState,         // COMABLE管理 setter
    Func<int, int, bool> checkCooking,   // F378 InfoEvent.CheckCooking
    Func<int, PrivateRoomCheckResult> checkPrivateRoom,  // F378 InfoEvent.CheckPrivateRoom
    Func<int, int> calculateAttitude     // F381 InfoState.CalculateCharacterAttitude
)
```

**Returns**: `OrchestrationResult` with orchestration state and conditional call flags.

**Pattern**: Follows F381 InfoState pattern with Func/Action parameters for external dependencies.

---

## SHOW_STATUS Analysis

### Overview
SHOW_STATUS (INFO.ERB lines 3-214) is the main turn orchestrator called every game turn. It coordinates character movement, attitude updates, event checks, and status display.

### Category 1: Already Migrated C# Functions (F378/F381)

**F378 InfoEvent Functions**:
- Line 16-17: `CEACK_COOKING` → `InfoEvent.CheckCooking()` - Cooking timer check
- Line 73: `INFO_CheckMyRoom(LOCAL)` → `InfoEvent.CheckPrivateRoom()` - Private room reassignment

**F381 InfoState Functions**:
- Line 21: `CHARA_ATTITUDE` → `InfoState.CalculateCharacterAttitude()` - Character attitude calculation
- Line 85: `INFO_SetTarget` → `InfoState.SetTrainingTarget()` - Target selection
- Line 134: `INFO_SetTrainMode` state logic → `InfoState.SetTrainMode()` - Train mode state transitions

### Category 2: External ERB Calls (Not Migrated)

**Character Movement & Actions**:
- Line 29: `CHARA_MOVEMENT` - Character movement system (external, not part of INFO.ERB)
- Line 48: `INHABITANT_DO` - Inhabitant actions (external)
- Line 49: `VISITER_DO` - Visitor actions (external)
- Line 50: `INTRUDER_DO` - Intruder (Koishi) actions (external)

**Location System**:
- Line 31: `IN_ROOM()` - Find character in room (external function)
- Line 79: `CAN_MOVE()` - Check movement possibility between locations (external function)
- Line 102, 113: `BATHROOM()`, `OPENPLACE()` - Location type checks (external)

### Category 3: Special Events

**Conditional Event Calls**:
- Line 10: `SHOW_TOUCH` - Touch event (called when TFLAG:COMABLE管理 == 2)
- Line 12: `従者E_進展` - Servant event progression (called when FLAG:雑多設定 bit 2 is set)
- Line 37: `KOJO_EVENT(8, LOCAL, LOCAL:1)` - Character encounter kojo (馴れ合い強度 context)
- Line 44: `NTR_COM416_BATHTIME_INTERRUPT` - Bathtime interruption event (when FLAG:1840 == 10)
- Line 65: `GoOut_SeeYou(LOCAL, 0)` - Go-out declaration event
- Line 71, 80: `AFFAIR_DISCLOSURE(LOCAL, mode)` - Affair disclosure event (deferred to Phase 17)

### Category 4: Clothes Management

**Clothing Reset & Settings**:
- Line 89: `CLOTHES_RESET(LOCAL)` - Reset character clothes (when うふふ ends or character moves)
- Line 90, 109: `CLOTHES_SETTING_TRAIN(LOCAL)` - Set training clothes
- Line 91-96: TEQUIP reset loop (excludes diuretic at index 27)

**Special Clothing Conditions**:
- Line 104-109: Bathroom play setting (TEQUIP:お風呂場プレイ)
- Line 114-118: Outdoor play setting (TEQUIP:野外プレイ)
- Line 60-61: Panty confirmation flag (下半身下着が見える状態)

### Category 5: Counter Events

**Counter Event Orchestration** (Lines 156-207):
- **Condition**: `TFLAG:COMABLE管理 == 3 || TFLAG:COMABLE管理 == 4` (うふふ mode)
- **Split by TALENT:MASTER:肉便器**:
  - If 肉便器: Call `EVENT_WC_COUNTER*` series (lines 159-179)
  - Else: Call `EVENT_COUNTER*` series (lines 184-203)
- **Common Pattern**:
  1. Loop through TARGET array (exclude TARGET:LOCAL == 0)
  2. Call `EVENT_*_COUNTER(TARGET:LOCAL, LOCAL)` - Counter calculation
  3. Call `EVENT_*_COUNTER_POSE(TARGET:LOCAL, LOCAL)` - Pose update
  4. Call `EVENT_*_COUNTER_COMBINATION` - Combination calculation
  5. Call `EVENT_*_COUNTER_SOURCE(TARGET:LOCAL, LOCAL)` - Source assignment
  6. If `TCVAR:(TARGET:LOCAL):20` → Call `KOJO_MESSAGE_*_COUNTER(TARGET:LOCAL)` - Counter kojo
  7. Call `PREGNACY_S_EVENT(LOCAL)` - Pregnancy special event (TRYCALL)

### Category 6: Deferred (Phase 17)

**AFFAIR_DISCLOSURE** (Lines 71, 80):
- Affair disclosure system (NTR mechanics)
- Called when character encounters MASTER:
  - Mode 0 (line 71): Same room encounter (newly entered room)
  - Mode 1 (line 80): Visual range encounter (different rooms but visible via CAN_MOVE == 2)
- **Deferred Reason**: Complex NTR system requires Phase 17 comprehensive migration
- **Current Strategy**: StatusOrchestrator returns `NeedsAffairDisclosure` flag; ERB wrapper calls AFFAIR_DISCLOSURE

### Category 7: Display (F373/F379/F382)

**Display Delegation**:
- Line 126: `DRAWLINE` - Draw separator line (F373 InfoPrint)
- Line 128: `INFO_PrintCurrentTime(MASTER)` - Current time display (F373)
- Line 130: `INFO_PrintCurrentPosition(MASTER)` - Current position display (F373)
- Line 134: `INFO_SetTrainMode` - Train mode display (F382 InfoTrainModeDisplay)
- Line 136: `SHOW_EQUIP_2` - Special play situation display (F379)
- Line 138: `INFO_PrintHPMPBar(TARGET, MASTER)` - HP/MP bar display (F373)
- Line 141-145: Target-specific displays (F373):
  - `INFO_PrintTargetCloth(TARGET)` - Target clothing
  - `INFO_PrintTargetMood(TARGET)` - Target mood
  - `INFO_PrintTargetReason(TARGET)` - Target reason
  - `INFO_PrintTargetFeeling(TARGET)` - Target feeling
  - `INFO_PrintPalams(TARGET)` - Target parameters
- Line 148-149: Master state display (F373/F379)
- Line 153: `SHOW_EQUIP_1` - うふふ中 display (F379)

### Category 8: Logic Flow

**Main Flow** (Lines 3-209):

1. **Initialization** (Lines 3-8):
   - Reset TFLAG:100, TFLAG:101 counters
   - Decrement TFLAG:101 if non-zero

2. **Pre-Movement Events** (Lines 9-17):
   - SHOW_TOUCH (if COMABLE管理 == 2)
   - 従者E_進展 (if FLAG:雑多設定 bit 2)
   - CEACK_COOKING (if FLAG:料理 > 0) → F378

3. **Character State Updates** (Lines 19-21):
   - CHARA_ATTITUDE → F381 (all characters' 馴れ合い強度度 calculation)

4. **Movement & Interactions** (Lines 23-51):
   - If TFLAG:195 == 1: Skip movement (just reset flag)
   - Else:
     - CHARA_MOVEMENT (character movement)
     - 馴れ合い強度 encounter check (lines 30-39)
     - NTR bathtime interrupt check (lines 41-46)
     - INHABITANT_DO, VISITER_DO, INTRUDER_DO (lines 48-50)

5. **Time & Sleep State** (Lines 52-53):
   - Update TFLAG:300 = DATETIME()
   - Clear CFLAG:MASTER:睡眠 = 0

6. **Room Encounter Processing** (Lines 54-83):
   - Loop through all characters
   - For each character in same room as MASTER:
     - Check panty visibility (line 60-61)
     - If newly encountered (!同室 && !睡眠):
       - GoOut_SeeYou check (lines 64-69)
       - AFFAIR_DISCLOSURE (line 71) → Deferred Phase 17
       - INFO_CheckMyRoom (line 73) → F378
       - Set 同室 = 1 (line 74)
   - For characters not in same room:
     - Visual range AFFAIR_DISCLOSURE check (lines 77-80)
     - Clear 同室 = 0 (line 81)

7. **Target Selection** (Line 85):
   - INFO_SetTarget → F381 (target selection and 馴れ合い強度 setup)

8. **Clothing Management** (Lines 86-122):
   - Loop through all characters
   - If !うふふ OR moved: Reset clothes and TEQUIP (lines 88-101)
   - Bathroom clothing check (lines 102-112)
   - Outdoor play check (lines 113-121)

9. **Display Coordination** (Lines 124-155):
   - DRAWLINE → F373
   - Time/Position display → F373
   - Train mode setup → F381/F382
   - Target/Master state display → F373/F379

10. **Counter Events** (Lines 156-207):
    - If COMABLE管理 == 3 or 4:
      - EVENT_COUNTER* or EVENT_WC_COUNTER* series
      - KOJO_MESSAGE_COUNTER
      - PREGNACY_S_EVENT

**Control Flow Patterns**:
- **Early Exit**: Line 66-68 (CONTINUE when character goes out)
- **Conditional Blocks**: Lines 25-51 (TFLAG:195 skip movement), 140-146 (TARGET > 0), 156-207 (COMABLE管理 states)
- **Nested Loops**: Lines 57-83 (character room processing), 159-206 (counter events with TARGET array)
- **State Flags**: TFLAG:COMABLE管理 controls event flow (special touch, display, counter events)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | fl | Created as part of INFO.ERB split (F373 scope reduction) | PROPOSED |
| 2026-01-06 21:19 | START | implementer | Tasks 1-8 | - |
| 2026-01-06 21:19 | complete | implementer | Task 1: SHOW_STATUS Analysis documented (8 categories) | SUCCESS |
| 2026-01-06 21:19 | complete | implementer | Task 2: Created Era.Core/Orchestration/StatusOrchestrator.cs | SUCCESS |
| 2026-01-06 21:19 | complete | implementer | Task 3: C# build succeeded | SUCCESS |
| 2026-01-06 21:19 | complete | implementer | Task 4: Test file exists (created in Phase 3) | SUCCESS |
| 2026-01-06 21:19 | complete | implementer | Task 5: All 8 tests passed | SUCCESS |
| 2026-01-06 21:19 | complete | implementer | Task 6: 8 [Fact] attributes verified | SUCCESS |
| 2026-01-06 21:19 | complete | implementer | Task 7: API documentation exists | SUCCESS |
| 2026-01-06 21:19 | complete | implementer | Task 8: Negative tests verified (NoTarget, WhenTargetSleeping) | SUCCESS |
| 2026-01-06 21:19 | END | implementer | Tasks 1-8 | SUCCESS |
