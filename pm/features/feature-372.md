# Feature 372: COMMON_PLACE.ERB Migration - Location/Movement System

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate COMMON_PLACE.ERB location and movement system functions to C# LocationSystem.cs. This establishes type-safe location management and navigation functions.

**Context**: F366 successor, Phase 3 from full-csharp-architecture.md. Requires F366 CommonFunctions.cs completion.

---

## Background

### Philosophy (Mid-term Vision)

**Foundation-First Migration**: COMMON_PLACE.ERB contains shared location/movement functions (319 lines, 10 functions) used throughout the codebase - location queries, room property checks, place state management, etc. Migrating these to C# enables type-safe location operations and prepares Phase 4+ features.

### Problem (Current Issue)

COMMON_PLACE.ERB (319 lines, 10 functions; note: duplicate CASE 場所_魔理沙の家 at lines 51 and 58 - dead code in source) contains:
- Location name lookup: @GETPLACENAME, @NTR_GETPLACENAME
- Character location queries: @IN_ROOM, @GET_N_IN_ROOM
- Room constant: @MAXROOM
- Room property checks: @BEDROOM, @BATHROOM, @KITCHEN, @OPENPLACE, @MAP_DISTINCTION

**Current State**:
- All ERB files depend on COMMON_PLACE.ERB for location operations
- No C# equivalent for location system functions
- Phase 4+ features cannot reference LocationSystem until migration complete

### Goal (What to Achieve)

**Phase 1 Scope** (7 functions):
- Pure: @MAXROOM, @BATHROOM, @KITCHEN, @OPENPLACE, @MAP_DISTINCTION
- Partial: @GETPLACENAME (with callNameResolver), @BEDROOM (with hasPlaymat)

**Deferred** (3 functions): @IN_ROOM, @GET_N_IN_ROOM, @NTR_GETPLACENAME

1. Analyze COMMON_PLACE.ERB function groups and dependencies
2. Create Era.Core/Common/LocationSystem.cs with Phase 1 functions
3. Implement type-safe wrappers using Constants.cs for location IDs
4. Create xUnit test cases for location functions
5. Verify behavior matches ERB legacy implementation
6. Document LocationSystem API for Phase 4+ reference

**Migration Strategy (Phase 1)**:
- **Pure functions** (directly migratable): @MAXROOM, @BATHROOM, @KITCHEN, @OPENPLACE, @MAP_DISTINCTION - use Constants.cs for location IDs. Note: MAP_DISTINCTION returns 1 for external locations (場所_湖, 場所_湖南部, 場所_チルノの家, 場所_大妖精の家, 場所_魔理沙の家, 場所_魔法の森内部, 場所_ルーミアの住処, 場所_アリスの家). Duplicate CASE block for 場所_魔理沙の家 in ERB is dead code and will NOT be migrated
- **Partial migration** (parameter injection):
  - @GETPLACENAME → `GetPlaceName(int locationId, Func<int, string>? callNameResolver = null)` - callNameResolver provides CALLNAME lookup for 11 dynamic locations (パチュリー私室, 小悪魔私室, 咲夜私室, あなた私室, レミリア私室, チルノの家, 大妖精の家, 魔理沙の家, ルーミアの住処, アリスの家, 訪問者宅 + NTR_NAME(0)); null returns location code or placeholder
  - @BEDROOM → `IsBedroom(int locationId, bool hasPlaymat = false)` - when hasPlaymat=false, 大浴場 returns false (no bed). Caller must provide ITEM:20 value (true if ITEM:20 > 0) for accurate 大浴場 check
- **Runtime-dependent functions** (deferred to Phase 4+): @IN_ROOM, @GET_N_IN_ROOM - require CFLAG/TALENT/ABL arrays; @NTR_GETPLACENAME - requires NTR_CHK_VISIBLE from NTR_UTIL.ERB (cross-file dependency)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | COMMON_PLACE.ERB analysis documented | file | Grep "## COMMON_PLACE.ERB Analysis" in Game/agents/feature-372.md | contains | "## COMMON_PLACE.ERB Analysis" | [x] |
| 2 | LocationSystem.cs created | file | Glob Era.Core/Common/LocationSystem.cs | exists | Era.Core/Common/LocationSystem.cs | [x] |
| 3 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 4 | LocationSystem tests created | file | Glob engine.Tests/Tests/LocationSystemTests.cs | exists | engine.Tests/Tests/LocationSystemTests.cs | [x] |
| 5 | All location tests pass (Pos) | test | dotnet test --filter LocationSystemTests | succeeds | - | [x] |
| 6 | Minimum test coverage exists | code | Grep "\\[Fact\\]" in engine.Tests/Tests/LocationSystemTests.cs | gte | 5 | [x] |
| 7 | API documentation created | file | Grep "## LocationSystem API" in Game/agents/feature-372.md | contains | "## LocationSystem API" | [x] |
| 8 | Invalid location test exists (Neg) | code | Grep "InvalidLocation" in engine.Tests/Tests/LocationSystemTests.cs | contains | "InvalidLocation" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze COMMON_PLACE.ERB: categorize functions, identify dependencies, document in feature-372.md | [x] |
| 2 | 2 | Create Era.Core/Common/LocationSystem.cs with Phase 1 functions: GetPlaceName, MaxRoom, IsBedroom, IsBathroom, IsKitchen, IsOpenPlace, MapDistinction | [x] |
| 3 | 3 | Verify C# build succeeds after LocationSystem.cs creation | [x] |
| 4 | 4 | Create engine.Tests/Tests/LocationSystemTests.cs using xUnit patterns | [x] |
| 5 | 5 | Run all LocationSystem tests and verify they pass | [x] |
| 6 | 6 | Verify test coverage: ensure minimum 5 test methods exist | [x] |
| 7 | 7 | Document LocationSystem API with usage examples in feature-372.md | [x] |
| 8 | 8 | Add negative test method with name containing "InvalidLocation" (e.g., GetPlaceName_InvalidLocation_ReturnsEmpty) | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F366 | Requires CommonFunctions.cs completion |
| Predecessor | F364 | Requires Constants.cs for location IDs |

---

## Links

- [feature-366.md](feature-366.md) - COMMON.ERB Migration (predecessor)
- [feature-364.md](feature-364.md) - Constants.cs (prerequisite)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 reference
- Game/ERB/COMMON_PLACE.ERB - Source file to migrate
- Game/ERB/NTR/NTR_UTIL.ERB - NTR_CHK_VISIBLE dependency
- Game/archive/original-source/originalSource(era紅魔館protoNTR/ERB/COMMON_PLACE.ERB - Archive version for comparison

---

## COMMON_PLACE.ERB Analysis

### Function Categories

**Phase 1 Functions (7 migrated):**

1. **@MAXROOM()** - Pure constant function
   - Returns: 場所番号最大値+1 = 31
   - No dependencies

2. **@BATHROOM(ARG)** - Pure location check
   - Returns: 1 for 場所_大浴場, 0 otherwise
   - No dependencies

3. **@KITCHEN(ARG)** - Pure location check
   - Returns: 1 for locations with kitchen (厨房, チルノの家, 大妖精の家, 魔理沙の家, アリスの家)
   - No dependencies

4. **@OPENPLACE(ARG)** - Pure location visibility check
   - Returns: 2 (outdoor: 正門, 庭, 湖, 湖南部, 3Fテラス), 1 (indoor open: 広間, 二階踊り場, 二階廊下), 0 (private)
   - No dependencies

5. **@MAP_DISTINCTION(ARG)** - Pure external location check
   - Returns: 1 for locations external to mansion (湖, 湖南部, チルノの家, 大妖精の家, 魔理沙の家, 魔法の森内部, ルーミアの住処, アリスの家)
   - No dependencies

6. **@GETPLACENAME(場所番号, 場所番号詳細)** - Partial migration
   - Static names: Most locations return hardcoded strings
   - Dynamic names: 11 locations require CALLNAME lookup (パチュリー私室, 小悪魔私室, 咲夜私室, あなた私室, レミリア私室, チルノの家, 大妖精の家, 魔理沙の家, ルーミアの住処, アリスの家, 訪問者宅)
   - Migration: Uses `Func<int, string>? callNameResolver` parameter injection
   - Note: Duplicate CASE block for 場所_魔理沙の家 at lines 51 and 58 in source ERB (dead code, not migrated)

7. **@BEDROOM(ARG)** - Partial migration with state dependency
   - Base bedrooms: パチュリー私室, 小悪魔私室, 守衛小屋, 咲夜私室, あなた私室, レミリア私室, 地下室, チルノの家, 大妖精の家, 魔理沙の家, ルーミアの住処, アリスの家
   - Special case: 大浴場 returns 1 only when ITEM:20 > 0 (playmat present)
   - Migration: Uses `bool hasPlaymat` parameter injection (caller must provide ITEM:20 state)

**Deferred Functions (3, Phase 4+):**

8. **@IN_ROOM(MINORMAX, 場所, 対象変数, 変数番号, 睡眠SKIP)** - Runtime-dependent
   - Requires: CFLAG, TCVAR, ABL, TALENT arrays
   - Complexity: Queries character states by location
   - Deferred until character runtime state available in C#

9. **@GET_N_IN_ROOM(ARG, ARG:1, ARG:2, ARG:3)** - Runtime-dependent
   - Requires: CFLAG, FLAG arrays, MASTER/CHARANUM state
   - Complexity: Counts characters at location with filters
   - Deferred until character runtime state available in C#

10. **@NTR_GETPLACENAME(ARG)** - Cross-file dependency
    - Requires: NTR_CHK_VISIBLE from NTR_UTIL.ERB
    - Complexity: Conditional location name based on visibility state
    - Deferred until NTR system functions migrated

### Migration Strategy Validation

- **7/10 functions migrated** (70% completion for Phase 1 scope)
- Pure functions: 5 (MAXROOM, BATHROOM, KITCHEN, OPENPLACE, MAP_DISTINCTION) - direct migration successful
- Partial migration: 2 (GETPLACENAME, BEDROOM) - parameter injection pattern validated
- Deferred: 3 (IN_ROOM, GET_N_IN_ROOM, NTR_GETPLACENAME) - require Phase 4+ runtime state

### Source Code Notes

- **Dead code detected**: Lines 58-60 contain duplicate CASE 場所_魔理沙の家 block (unreachable after line 50-51)
- **NTR dependency**: 訪問者宅 name requires NTR_NAME(0) function call
- **State dependency**: 大浴場 bedroom status requires ITEM:20 (playmat) runtime check

---

## LocationSystem API

### Era.Core.Common.LocationSystem

All functions are static methods in the `Era.Core.Common.LocationSystem` class.

#### MaxRoom()

```csharp
int MaxRoom()
```

Returns the maximum room number (31).

**Usage:**
```csharp
int maxRooms = LocationSystem.MaxRoom(); // 31
```

#### IsBathroom(int locationId)

```csharp
bool IsBathroom(int locationId)
```

Checks if location has bathroom facilities.

**Parameters:**
- `locationId`: Location ID from Constants.場所_xxx

**Returns:** True if location is 大浴場

**Usage:**
```csharp
bool hasBath = LocationSystem.IsBathroom(Constants.場所_大浴場); // true
bool hasBath2 = LocationSystem.IsBathroom(Constants.場所_広間); // false
```

#### IsKitchen(int locationId)

```csharp
bool IsKitchen(int locationId)
```

Checks if location has kitchen facilities.

**Parameters:**
- `locationId`: Location ID from Constants.場所_xxx

**Returns:** True if location has a kitchen (厨房, チルノの家, 大妖精の家, 魔理沙の家, アリスの家)

**Usage:**
```csharp
bool hasKitchen = LocationSystem.IsKitchen(Constants.場所_厨房); // true
bool hasKitchen2 = LocationSystem.IsKitchen(Constants.場所_チルノの家); // true
```

#### IsOpenPlace(int locationId)

```csharp
int IsOpenPlace(int locationId)
```

Checks if location is an open place with visibility.

**Parameters:**
- `locationId`: Location ID from Constants.場所_xxx

**Returns:**
- 0: Private room (default)
- 1: Indoor open space (広間, 二階踊り場, 二階廊下)
- 2: Outdoor space (正門, 庭, 湖, 湖南部, 3Fテラス)

**Usage:**
```csharp
int openLevel = LocationSystem.IsOpenPlace(Constants.場所_湖); // 2 (outdoor)
int openLevel2 = LocationSystem.IsOpenPlace(Constants.場所_広間); // 1 (indoor open)
int openLevel3 = LocationSystem.IsOpenPlace(Constants.場所_パチュリー私室); // 0 (private)
```

#### MapDistinction(int locationId)

```csharp
int MapDistinction(int locationId)
```

Checks if location is external to the mansion (different map area).

**Parameters:**
- `locationId`: Location ID from Constants.場所_xxx

**Returns:**
- 1: External location (湖, 湖南部, チルノの家, 大妖精の家, 魔理沙の家, 魔法の森内部, ルーミアの住処, アリスの家)
- 0: Internal to mansion (default)

**Usage:**
```csharp
int isExternal = LocationSystem.MapDistinction(Constants.場所_湖); // 1
int isExternal2 = LocationSystem.MapDistinction(Constants.場所_広間); // 0
```

#### GetPlaceName(int locationId, Func<int, string>? callNameResolver = null)

```csharp
string GetPlaceName(int locationId, Func<int, string>? callNameResolver = null)
```

Gets location name string with optional character name resolution.

**Parameters:**
- `locationId`: Location ID from Constants.場所_xxx
- `callNameResolver`: Optional function to resolve character names for dynamic locations. Pass null for static locations. For dynamic locations without resolver, returns empty string.

**Returns:** Location name string

**Dynamic Locations (require resolver):**
- パチュリー私室, 小悪魔私室, 咲夜私室, あなた私室, レミリア私室: Pass character ID → returns "{character name}私室"
- チルノの家, 大妖精の家, 魔理沙の家, ルーミアの住処, アリスの家: Pass character ID → returns "{character name}の家" or "の住処"
- 訪問者宅: Pass 0 for NTR_NAME(0) → returns "{visitor name}宅"

**Usage:**
```csharp
// Static location
string name1 = LocationSystem.GetPlaceName(Constants.場所_正門, null); // "正門"

// Dynamic location with resolver
string name2 = LocationSystem.GetPlaceName(
    Constants.場所_パチュリー私室,
    charId => "パチュリー"
); // "パチュリー私室"

// Dynamic location without resolver (returns empty)
string name3 = LocationSystem.GetPlaceName(Constants.場所_パチュリー私室, null); // ""
```

#### IsBedroom(int locationId, bool hasPlaymat = false)

```csharp
bool IsBedroom(int locationId, bool hasPlaymat = false)
```

Checks if location has a bed.

**Parameters:**
- `locationId`: Location ID from Constants.場所_xxx
- `hasPlaymat`: True if ITEM:20 > 0 (playmat present). Only affects 大浴場.

**Returns:** True if location has a bed or playmat

**Standard Bedrooms:**
- パチュリー私室, 小悪魔私室, 守衛小屋, 咲夜私室, あなた私室, レミリア私室, 地下室
- チルノの家, 大妖精の家, 魔理沙の家, ルーミアの住処, アリスの家

**Special Case:**
- 大浴場: Returns true only when hasPlaymat=true (ITEM:20 > 0)

**Usage:**
```csharp
bool isBedroom1 = LocationSystem.IsBedroom(Constants.場所_パチュリー私室); // true
bool isBedroom2 = LocationSystem.IsBedroom(Constants.場所_大浴場, false); // false (no playmat)
bool isBedroom3 = LocationSystem.IsBedroom(Constants.場所_大浴場, true); // true (with playmat)
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | implementer | Created as F366 successor for location system migration | PROPOSED |
| 2026-01-06 17:37 | implement | implementer | Tasks 1-3, 5-7 complete: Analysis, implementation, build, tests, API docs | SUCCESS |
