# Feature 378: INFO.ERB Migration - Event Handling Functions

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate event handling functions from INFO.ERB to C# InfoEvent.cs. This is Part 3 of INFO.ERB migration.

**Context**: Part of INFO.ERB split: F373 (Print), F381 (State), F378 (Event), F379 (Equipment), F380 (Orchestration).

**Scope Limitation**: AFFAIR_DISCLOSURE (~103 lines) is deferred to Phase 17 (AI & Visitor Systems / NTR Events) due to extensive runtime dependencies (IN_ROOM, CAN_MOVE, KOJO_EVENT, etc.). F378 implements CheckCooking and CheckPrivateRoom only (~30 lines combined).

---

## Background

### Philosophy (Mid-term Vision)

**Foundation-First Migration**: Event handling functions manage NTR-related game events like affair disclosure, room checks, and cooking timers. These contain significant game logic.

### Problem (Current Issue)

Event handling functions in INFO.ERB:
- **AFFAIR_DISCLOSURE (lines 693-795, ~103 lines)** - NTR affair disclosure event → **Deferred to Phase 17**
- INFO_CheckMyRoom (lines 1268-1282, ~15 lines) - Room occupation check → **F378 scope**
- CEACK_COOKING (lines 798-812, ~15 lines) - Cooking timer check → **F378 scope** (Note: misspelled in source ERB; C# uses CheckCooking)

**Current State**:
- AFFAIR_DISCLOSURE is complex NTR event logic with heavy runtime dependencies (IN_ROOM, CAN_MOVE, KOJO_EVENT, TOUCH_SET, 天候, TALENT checks) → requires IEventContext from Phase 17
- INFO_CheckMyRoom checks for room conflicts (simple parameterized function)
- CEACK_COOKING manages cooking timer state (simple parameterized function)

### Goal (What to Achieve)

1. Analyze all event handling functions and document dependencies (including deferred AFFAIR_DISCLOSURE)
2. Create Era.Core/Event/InfoEvent.cs with CheckCooking and CheckPrivateRoom
3. Implement event functions using IGameContext pattern for runtime dependencies (following F366 patterns)
4. Create xUnit test cases for event functions
5. Document InfoEvent API and Phase 17 deferral rationale

**C# Naming**: CheckCooking (CEACK_COOKING), CheckPrivateRoom (INFO_CheckMyRoom)
**Deferred**: CheckAffairDisclosure (AFFAIR_DISCLOSURE) → Phase 17 (full-csharp-architecture.md)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Event functions analysis documented | file | Grep "## Event Functions Analysis" in Game/agents/feature-378.md | contains | "## Event Functions Analysis" | [x] |
| 2 | InfoEvent.cs created | file | Glob Era.Core/Event/InfoEvent.cs | exists | Era.Core/Event/InfoEvent.cs | [x] |
| 3 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 4 | InfoEvent tests created | file | Glob Era.Core.Tests/InfoEventTests.cs | exists | Era.Core.Tests/InfoEventTests.cs | [x] |
| 5 | All event tests pass (Pos) | test | dotnet test --filter InfoEventTests | succeeds | - | [x] |
| 6 | Minimum test coverage exists | code | Grep "\\[Fact\\]" in Era.Core.Tests/InfoEventTests.cs | gte | 4 | [x] |
| 7 | API documentation created | file | Grep "## InfoEvent API" in Game/agents/feature-378.md | contains | "## InfoEvent API" | [x] |
| 8 | Negative test for cooking not expired | code | Grep "NotExpired" in Era.Core.Tests/InfoEventTests.cs | contains | "NotExpired" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze all event functions (including deferred AFFAIR_DISCLOSURE): document runtime dependencies | [x] |
| 2 | 2 | Create Era.Core/Event/InfoEvent.cs with CheckCooking and CheckPrivateRoom | [x] |
| 3 | 3 | Verify C# build succeeds after InfoEvent.cs creation | [x] |
| 4 | 4 | Create Era.Core.Tests/InfoEventTests.cs using xUnit patterns | [x] |
| 5 | 5 | Run all InfoEvent tests and verify they pass | [x] |
| 6 | 6 | Verify test coverage: ensure minimum 4 test methods exist | [x] |
| 7 | 7 | Document InfoEvent API and Phase 17 deferral in feature-378.md | [x] |
| 8 | 8 | Add negative test for cooking-not-expired scenario | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F373 | Follows F373 patterns for C# migration |
| Predecessor | F372 | Requires LocationSystem.cs for place/room checks (IsOpenPlace, GetPlaceName) |
| Predecessor | F366 | Requires CommonFunctions.cs |
| Predecessor | F364 | Requires Constants.cs |
| Successor | F380 | SHOW_STATUS depends on event functions |

---

## Links

- [feature-364.md](feature-364.md) - Constants (predecessor)
- [feature-366.md](feature-366.md) - Common Functions (predecessor)
- [feature-372.md](feature-372.md) - Location System (predecessor)
- [feature-373.md](feature-373.md) - Print Display (predecessor)
- [feature-381.md](feature-381.md) - State Management (sibling)
- [feature-379.md](feature-379.md) - Equipment Display (sibling)
- [feature-380.md](feature-380.md) - SHOW_STATUS (successor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 17 (AFFAIR_DISCLOSURE target)
- Game/ERB/INFO.ERB - AFFAIR_DISCLOSURE (lines 693-795), CEACK_COOKING (lines 798-812), INFO_CheckMyRoom (lines 1268-1282)

---

## AFFAIR_DISCLOSURE Deferral Plan

### Why Deferred to Phase 17

AFFAIR_DISCLOSURE has extensive runtime dependencies that require IEventContext infrastructure:

| Dependency | Type | Current Status | Phase 17 Solution |
|------------|------|----------------|-------------------|
| IN_ROOM | ERB function | Not migrated | IEventContext.GetCharactersInRoom() |
| CAN_MOVE | ERB function | Not migrated | IEventContext.CanSee() |
| KOJO_EVENT | ERB function | Complex callback | INtrEventHandler interface |
| TOUCH_SET | ERB function | Side effect | IEventContext.SetTouch() |
| 天候 | ERB function | Global state | IWeatherService |
| CFLAG/TALENT | Runtime arrays | Parameter injection | Strongly Typed IDs (Phase 4) |

### F380 (SHOW_STATUS) Impact

**SHOW_STATUS calls AFFAIR_DISCLOSURE at INFO.ERB lines 72, 81.**

**F380 Design Decision**: StatusOrchestrator.cs maintains AFFAIR_DISCLOSURE call in ERB.

```
F380 StatusOrchestrator (C#)
    ├── CheckCooking() ← F378 C# (migrated)
    ├── CheckPrivateRoom() ← F378 C# (migrated)
    └── AFFAIR_DISCLOSURE() ← ERB call (deferred)
```

**Transition Path**:
1. **Phase 3 (F378, F380)**: F380 orchestrates via mixed C#/ERB calls
2. **Phase 17**: AFFAIR_DISCLOSURE migrated with IEventContext
3. **Post-Phase 17 Refactoring**: F380 updated to pure C# orchestration

### Technical Debt Tracking

| Item | Location | Resolution Phase |
|------|----------|------------------|
| AFFAIR_DISCLOSURE ERB | INFO.ERB:693-795 | Phase 17 |
| F380 mixed C#/ERB calls | StatusOrchestrator.cs | Phase 17 refactoring |

**Reference**: `Game/agents/designs/full-csharp-architecture.md` Phase 17 (AI & Visitor Systems)

---

## Event Functions Analysis

### Analyzed Functions

| Function | Lines | LOC | Status | Dependencies |
|----------|-------|-----|--------|--------------|
| AFFAIR_DISCLOSURE | 693-795 | ~103 | **Deferred to Phase 17** | IN_ROOM, CAN_MOVE, KOJO_EVENT, TOUCH_SET, 天候, TALENT, CFLAG runtime arrays |
| CEACK_COOKING | 798-812 | ~15 | **F378 Scope** | TIME_PROGRESS, FLAG:料理, TCVAR:PLAYER, GETDISHNAME, GETDISHMENU |
| INFO_CheckMyRoom | 1268-1282 | ~15 | **F378 Scope** | TALENT:恋慕, NO (character ID), CFLAG:開始位置, GET_PRIVATE_ROOM, TRYCALLLIST kojo |

### CEACK_COOKING Analysis (lines 798-812)

**Purpose**: Check cooking timer state and update dish freshness.

**Logic Flow**:
1. **Expired (>720 min)**: Dish becomes inedible → discard (FLAG:料理=0, reset timers)
2. **Stale (>360 min, state<2)**: Dish smells bad → update state to 2
3. **Cold (>60 min, state<1, non-dessert)**: Dish gets cold → update state to 1
4. **Fresh (<60 min)**: No state change

**Parameters**:
- INPUT: FLAG:料理 (dish ID), TCVAR:PLAYER:305 (timer start), TCVAR:PLAYER:306 (state)
- OUTPUT: Updated TCVAR:PLAYER:306 (state), FLAG:料理 (cleared on expire), PRINT messages

**C# Signature**:
```csharp
public static CookingCheckResult CheckCooking(
    int dishId,
    int minutesElapsed,
    int currentState,
    bool isDessert = false
)
```

**Return Types**:
- `CookingState` enum: Fresh, Cold, Stale, Expired
- `CookingCheckResult` class: State, ShouldDiscard, NewStateValue, Message

**Dependencies**:
- GETDISHNAME (dish name lookup) → deferred to calling context
- GETDISHMENU (menu category) → simplified to `isDessert` boolean parameter
- TIME_PROGRESS → replaced with `minutesElapsed` parameter

### INFO_CheckMyRoom Analysis (lines 1268-1282)

**Purpose**: Check if non-lover character conflicts with master's private room assignment.

**Logic Flow**:
1. **Check eligibility**: Skip if TALENT:恋慕 OR (NO==148 OR NO==149) [lover or child exemption]
2. **Check conflict**: If GET_PRIVATE_ROOM(ARG) == CFLAG:MASTER:開始位置
3. **Reassign**: Set CFLAG:MASTER:開始位置 = 15 (場所_あなた私室), call kojo COM_K{ID}_104_1_3

**Parameters**:
- INPUT: ARG (character ID), TALENT:恋慕, NO (character ID), CFLAG:開始位置
- OUTPUT: CFLAG:MASTER:開始位置 (updated to 15), kojo call, PRINT message

**C# Signature**:
```csharp
public static PrivateRoomCheckResult CheckPrivateRoom(
    int characterId,
    bool isLover,
    bool isChild,
    int masterStartLocation,
    int characterPrivateRoom
)
```

**Return Types**:
- `PrivateRoomCheckResult` class: ShouldReassignRoom, NewRoomId, Message

**Dependencies**:
- GET_PRIVATE_ROOM → simplified to `characterPrivateRoom` parameter (caller provides)
- CALLNAME → deferred to calling context for message formatting
- TRYCALLLIST kojo → deferred to calling context (C# returns flag, ERB handles kojo)

### AFFAIR_DISCLOSURE Deferral Rationale

**Complexity**: 103 lines with extensive runtime dependencies requiring IEventContext infrastructure.

**Critical Dependencies NOT in Phase 3**:
| Dependency | Usage | Phase 17 Solution |
|------------|-------|-------------------|
| IN_ROOM("MAX", ...) | Find characters in same room with うふふ flag | IEventContext.GetCharactersInRoom() |
| CAN_MOVE(loc1, loc2) | Line-of-sight check (return 2 = visible) | IEventContext.CanSee() |
| KOJO_EVENT(type, ch1, ch2) | Trigger character dialogue callbacks | INtrEventHandler.TriggerKojoEvent() |
| TOUCH_SET(0,0,0,1) | Reset touch state after affair | IEventContext.ResetTouchState() |
| 天候(天候値) | Weather check for fog/darkness | IWeatherService.GetCurrentWeather() |
| TALENT runtime checks | TALENT:恋慕, TALENT:親愛, etc. | Strongly Typed IDs (Phase 4) + IGameContext |

**Design Decision**: F378 implements only **parameterized functions** (CheckCooking, CheckPrivateRoom) that do NOT require runtime context. AFFAIR_DISCLOSURE requires full IEventContext and is deferred to Phase 17 (AI & Visitor Systems).

---

## InfoEvent API

### CheckCooking Method

```csharp
public static CookingCheckResult CheckCooking(
    int dishId,
    int minutesElapsed,
    int currentState,
    bool isDessert = false)
```

**Purpose**: Check cooking timer state and determine dish freshness.

**Parameters**:
- `dishId`: Dish ID from FLAG:料理
- `minutesElapsed`: Minutes elapsed since cooking (from TIME_PROGRESS)
- `currentState`: Current state value (TCVAR:PLAYER:306)
- `isDessert`: True if dish is dessert (desserts don't get cold)

**Returns**: `CookingCheckResult` with state, message, and update flags

**Behavior**:
- **Expired (>720 min)**: `State=Expired`, `ShouldDiscard=true`, `Message="食べられない"`
- **Stale (>360 min, state<2)**: `State=Stale`, `NewStateValue=2`, `Message="嫌な臭い"`
- **Cold (>60 min, state<1, non-dessert)**: `State=Cold`, `NewStateValue=1`, `Message="冷えてしまった"`
- **Fresh (<60 min)**: `State=Fresh`, `NewStateValue=currentState`, `Message=null`

**Freshness Thresholds**:
| State | Time Range | Applies To | Action |
|-------|------------|------------|--------|
| Fresh | 0-60 min | All dishes | No change |
| Cold | 60-360 min | Non-desserts only | Update state to 1 |
| Stale | 360-720 min | All dishes | Update state to 2 |
| Expired | >720 min | All dishes | Discard dish |

### CheckPrivateRoom Method

```csharp
public static PrivateRoomCheckResult CheckPrivateRoom(
    int characterId,
    bool isLover,
    bool isChild,
    int masterStartLocation,
    int characterPrivateRoom)
```

**Purpose**: Check if non-lover character conflicts with master's private room assignment.

**Parameters**:
- `characterId`: Character ID to check
- `isLover`: True if TALENT:恋慕 is set
- `isChild`: True if NO:148 or NO:149 (Meiling's children)
- `masterStartLocation`: Master's current start location (CFLAG:MASTER:開始位置)
- `characterPrivateRoom`: Character's private room (from GET_PRIVATE_ROOM)

**Returns**: `PrivateRoomCheckResult` with reassignment flag, new room ID, and message

**Behavior**:
- **Lover or Child**: `ShouldReassignRoom=false` (exemption - can share room)
- **Room Conflict (non-lover)**: `ShouldReassignRoom=true`, `NewRoomId=15`, `Message="自分の部屋で寝るよう言った"`
- **No Conflict**: `ShouldReassignRoom=false` (different rooms)

**Logic Flow**:
```
IF isLover OR isChild THEN
    → No conflict (return false)
ELSE IF masterStartLocation == characterPrivateRoom THEN
    → Room conflict (reassign to room 15)
ELSE
    → No conflict (different rooms)
```

### CookingState Enum

```csharp
public enum CookingState
{
    Fresh = 0,   // 0-60 minutes, no state change
    Cold = 1,    // 60-360 minutes (non-desserts)
    Stale = 2,   // 360-720 minutes
    Expired = 3  // >720 minutes (inedible)
}
```

Maps to TCVAR:PLAYER:306 state values in ERB.

### Result Classes

#### CookingCheckResult

```csharp
public class CookingCheckResult
{
    public CookingState State { get; set; }        // Current cooking state
    public bool ShouldDiscard { get; set; }        // True if dish should be discarded (FLAG:料理 = 0)
    public int NewStateValue { get; set; }         // New state value to update TCVAR:PLAYER:306
    public string? Message { get; set; }           // Message fragment (Japanese), null if no message needed
}
```

**Usage**: Caller formats `Message` with dish name using GETDISHNAME.

#### PrivateRoomCheckResult

```csharp
public class PrivateRoomCheckResult
{
    public bool ShouldReassignRoom { get; set; }   // True if master's room should be reassigned
    public int NewRoomId { get; set; }             // New room ID for CFLAG:MASTER:開始位置 (15 if reassigned)
    public string? Message { get; set; }           // Message fragment (Japanese), null if no message needed
}
```

**Usage**: Caller formats `Message` with character name using CALLNAME. ERB handles kojo call (COM_K{ID}_104_1_3) if `ShouldReassignRoom=true`.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | fl | Created as part of INFO.ERB split (F373 scope reduction) | PROPOSED |
| 2026-01-06 14:45 | START | implementer | Task 1: Event functions analysis | - |
| 2026-01-06 14:47 | END | implementer | Tasks 1-3: Analysis + InfoEvent.cs creation | SUCCESS |
| 2026-01-06 15:30 | START | implementer | Tasks 4-8: Tests + API documentation | - |
| 2026-01-06 15:35 | END | implementer | Tasks 4-8: Verification and documentation complete | SUCCESS |
