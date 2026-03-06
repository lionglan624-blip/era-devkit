# Feature 381: INFO.ERB Migration - State Management Functions

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate state management functions (state logic only) from INFO.ERB to C# InfoState.cs. This is Part 2 of INFO.ERB migration.

**Context**: Part of INFO.ERB split: F373 (Print), F381 (State), F382 (TrainMode Display), F378 (Event), F379 (Equipment), F380 (Orchestration).

**Scope Clarification**: INFO_SetTrainMode contains both state logic (lines 1125-1208) and display logic (lines 1209-1262). F381 migrates state logic only. Display logic is deferred to F382.

---

## Background

### Philosophy (Mid-term Vision)

**Foundation-First Migration**: State management functions control character attitude calculation, target selection, and train mode initialization. These are core game logic functions with complex runtime dependencies.

### Problem (Current Issue)

State management functions in INFO.ERB:
- CHARA_ATTITUDE (lines 636-689, 54 lines) - Character attitude calculation
- INFO_SetTarget (lines 1081-1119, 39 lines) - Target character selection
- INFO_SetTrainMode state logic (lines 1125-1208, 84 lines) - Train mode state management (display logic in F382)
- SHOW_INFO_PALAM (lines 555-609, 55 lines) - Parameter info display (moved from F379)
- SORT_CFLAG (lines 615-629, 15 lines) - Character flag sorting utility (moved from F379)

**Current State**:
- Functions embedded in INFO.ERB with heavy runtime dependencies (CFLAG, TARGET, TALENT, etc.)
- CHARA_ATTITUDE called by SHOW_STATUS every turn
- INFO_SetTarget/INFO_SetTrainMode manage core game state

**External Dependencies** (ERB functions called - handled via Func/Action parameters per F371 pattern):
- CHARA_ATTITUDE: GET_MARK_LEVEL (mark level retrieval)
- INFO_SetTarget: TCVAR (temporary character variable)
- SHOW_INFO_PALAM: GETPALAMLV (parameter level calculation), PRINTCPERLINE (display width)
- INFO_SetTrainMode state logic: CAN_MOVE (movement check), OPENPLACE (location state), GET_TARGETNUM (target count), CHK_350_STAIN_SEMEN (stain check)

**Migration Strategy**: External dependencies will be passed as Func/Action parameters following F371 NtrInitialization pattern. This enables unit testing via mocks while maintaining runtime flexibility.

### Goal (What to Achieve)

1. Analyze state management functions and document dependencies
2. Create Era.Core/State/InfoState.cs with state management utilities
3. Implement type-safe wrappers with appropriate parameter interfaces
4. Create xUnit test cases for state functions
5. Document InfoState API for Phase 4+ reference

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | State functions analysis documented | file | Grep "## State Functions Analysis" in Game/agents/feature-381.md | contains | "## State Functions Analysis" | [x] |
| 2 | InfoState.cs created | file | Glob Era.Core/State/InfoState.cs | exists | Era.Core/State/InfoState.cs | [x] |
| 3 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 4 | InfoState tests created | file | Glob Era.Core.Tests/InfoStateTests.cs | exists | Era.Core.Tests/InfoStateTests.cs | [x] |
| 5 | All state tests pass (Pos) | test | dotnet test --filter InfoStateTests | succeeds | - | [x] |
| 6 | Minimum test coverage exists | code | Grep "\\[Fact\\]" in Era.Core.Tests/InfoStateTests.cs (count mode) | gte | 8 | [x] |
| 7 | API documentation created | file | Grep "## InfoState API" in Game/agents/feature-381.md | contains | "## InfoState API" | [x] |
| 8 | Invalid target test exists (Neg) | code | Grep "Invalid" in Era.Core.Tests/InfoStateTests.cs | contains | "Invalid" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze CHARA_ATTITUDE, INFO_SetTarget, INFO_SetTrainMode, SHOW_INFO_PALAM, SORT_CFLAG: document runtime dependencies | [x] |
| 2 | 2 | Create Era.Core/State/InfoState.cs with state management functions | [x] |
| 3 | 3 | Verify C# build succeeds after InfoState.cs creation | [x] |
| 4 | 4 | Create Era.Core.Tests/InfoStateTests.cs using xUnit patterns | [x] |
| 5 | 5 | Run all InfoState tests and verify they pass | [x] |
| 6 | 6 | Verify test coverage: ensure minimum 8 test methods exist | [x] |
| 7 | 7 | Document InfoState API with usage examples in feature-381.md | [x] |
| 8 | 8 | Add negative test for invalid target handling in InfoStateTests.cs | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F373 | Follows F373 patterns for C# migration |
| Predecessor | F372 | Requires LocationSystem.cs for OPENPLACE equivalent (IsOpenPlace) |
| Predecessor | F366 | Requires CommonFunctions.cs |
| Predecessor | F364 | Requires Constants.cs |
| Predecessor | F370 | Requires Body/State systems |
| Successor | F380 | SHOW_STATUS depends on state functions |
| Successor | F382 | TrainMode display depends on state logic |

---

## Links

- [feature-364.md](feature-364.md) - Constants (predecessor)
- [feature-366.md](feature-366.md) - Common Functions (predecessor)
- [feature-370.md](feature-370.md) - State Initialization (predecessor)
- [feature-372.md](feature-372.md) - LocationSystem (predecessor)
- [feature-373.md](feature-373.md) - Print Display (predecessor)
- [feature-378.md](feature-378.md) - Event Handling (sibling)
- [feature-379.md](feature-379.md) - Equipment Display (sibling)
- [feature-380.md](feature-380.md) - SHOW_STATUS (successor)
- [feature-382.md](feature-382.md) - TrainMode Display (successor, display logic split)
- Game/ERB/INFO.ERB - Source file (lines 555-629, 636-689, 1081-1119, 1125-1208)

---

## State Functions Analysis

### CHARA_ATTITUDE (lines 636-689)

**Purpose**: Calculate character attitude (馴れ合い強度) representing how aggressively characters act toward MASTER.

**Intensity Levels**:
- 0: Skinship only (スキンシップまで)
- 1: Light harassment (軽いセクハラしてくる)
- 2: Harassment (セクハラしてくる)
- 3+: Push down (押し倒してくる)

**Algorithm**:
1. Initialize all characters' 好感度/屈服度 to non-negative values
2. Calculate 好感度基準値 (base affection) from:
   - TALENT (管理人, 恋慕): +10 each
   - Marks (GET_MARK_LEVEL for 屈服刻印, 快楽刻印): LV×3 each
   - ABL (親密, 欲望): LV×10 (max 50) each
   - PALAM (欲情, 好意): (GETPALAMLV level)×5 each
   - BASE (ムード, 理性): ムード/50 + (1000-理性)/30
   - CFLAG (既成事実うふふ): +5
3. Calculate 馴れ合い上限値 (max intensity):
   - Base: 1
   - +1 if 既成事実:告白
   - +1 if TALENT:淫乱
   - +2 if MASTER弱味 OR TALENT:MASTER:肉便器
4. Apply modifiers:
   - If no weakness: multiply base by欲情 ratio (0.5-1.0)
   - If 怒り OR 睡眠: max intensity = 0
5. Calculate 仮強度 = base / (50 + 一線越えない×10) - (NTR + 公衆便所)
6. Clamp to max: CFLAG:馴れ合い強度度 = MIN(仮強度, 馴れ合い上限値)

**External Dependencies**:
- `GET_MARK_LEVEL(奴隷, 刻印番号, MASTER)`: Mark level retrieval → Pass as `Func<int, int, int, int>`
- `GETPALAMLV(PALAM:奴隷:param, 5)`: Parameter level calculation → Pass as `Func<int, int, int>`

**Variables**: CFLAG (好感度, 屈服度, 既成事実, MASTERの弱味, 怒り, 睡眠, 馴れ合い強度度), TALENT (管理人, 恋慕, 淫乱, 一線越えない, NTR, 公衆便所, MASTER:肉便器), ABL (親密, 欲望), PALAM (欲情, 好意), BASE (ムード, 理性)

---

### INFO_SetTarget (lines 1081-1119)

**Purpose**: Select training target and set up 馴れ合い強度 for characters in the same room.

**Algorithm**:
1. Initialize TARGET array to 0
2. Sort characters by CFLAG:館内地位 using SORT_CFLAG
3. Iterate through sorted characters:
   - If same location as MASTER:
     - Add to TARGET array
     - Set as TARGET:0 if no target yet
     - Prefer previous target (TFLAG:現在のTARGET)
     - Calculate TCVAR:馴れ合い強度 = MIN(CFLAG:馴れ合い強度度, 起きてるキャラの最小馴れ合い強度)
       - 馴れ合い強度 limited by highest-rank awake character in room
     - Track minimum 馴れ合い強度 of awake characters
     - Set TCVAR:MASTER:馴れ合い強度 to max among room characters
4. Update TFLAG:現在のTARGET to selected target

**External Dependencies**: None (uses SORT_CFLAG defined in same file)

**Variables**: TARGET (target array), RESULT (sorted character list from SORT_CFLAG), CFLAG (現在位置, 館内地位, 馴れ合い強度度, 睡眠), TCVAR (馴れ合い強度), TFLAG (現在のTARGET)

---

### INFO_SetTrainMode State Logic (lines 1125-1208)

**Purpose**: Manage train mode state transitions (うふふモード, 押し倒し, 告白). This function contains both state logic (lines 1125-1208) and display logic (lines 1209-1262). **F381 scope: state logic only. Display logic deferred to F382.**

**State Logic Algorithm**:
1. Get target (奴隷 = TARGET:0) and current location (POS_NOW)
2. If target exists and is awake:
   - Count 同室人数 (adjacent room characters via CAN_MOVE check)
   - Set TFLAG:COMABLE管理 based on conditions:
     - If CFLAG:奴隷:うふふ == 1 → COMABLE管理 = 2
     - If CFLAG:奴隷:うふふ == 2 → COMABLE管理 = 3
     - If押し倒し conditions met → COMABLE管理 = 3:
       - TCVAR:MASTER:馴れ合い強度 > 2
       - No adjacent room characters
       - BASE:満足 < MAXBASE:満足
       - Not OPENPLACE (via LocationSystem.IsOpenPlace)
       - Not visitor location
       - Not NTR or 公衆便所
       - CHK_350_STAIN_SEMEN check passes
       - Print押し倒し message and call KOJO_EVENT(12)
       - Set CFLAG:うふふ = 2 for all targets and MASTER
     - Else if 告白 conditions met → COMABLE管理 = 1:
       - GET_TARGETNUM() < 2
       - TCVAR:MASTER:130 (specific flag)
       - No既成事実:告白 yet
       - Not sleeping, has気力
       - Calculate告白判定値 (similar to CHARA_ATTITUDE base calculation)
       - If判定値 > 180 + 一線越えない×20:
         - Print告白 message and call KOJO_EVENT(13)
         - SETBIT CFLAG:既成事実, 0
     - Else → COMABLE管理 = 1
3. If no target or target sleeping → COMABLE管理 = 0

**External Dependencies**:
- `CAN_MOVE(POS_NOW, target_pos)`: Movement check → Pass as `Func<int, int, int>`
- `LocationSystem.IsOpenPlace(POS_NOW)`: Already migrated in F372, use directly
- `GET_TARGETNUM()`: Get target count → Pass as `Func<int>`
- `CHK_350_STAIN_SEMEN(奴隷)`: Stain check → Pass as `Func<int, bool>`
- `GET_MARK_LEVEL(奴隷, 刻印番号, MASTER)`: Mark level → Pass as `Func<int, int, int, int>`
- `GETPALAMLV(PALAM:param, 5)`: Parameter level → Pass as `Func<int, int, int>`

**Variables**: TARGET, TFLAG (COMABLE管理, 現在のTARGET), CFLAG (睡眠, 現在位置, うふふ, 屈服度, 好感度, 既成事実), TCVAR (MASTER:馴れ合い強度, MASTER:130), BASE (満足, ムード, 理性, 気力), MAXBASE, ABL (親密, 欲望), PALAM (欲情, 好意), TALENT (一線越えない, NTR, 公衆便所, MASTER:肉便器), FLAG (訪問者の現在位置)

---

### SHOW_INFO_PALAM (lines 555-609)

**Purpose**: Display parameter information with progress bars and formatted values.

**Algorithm**:
1. Loop through all PALAM indices (0-99)
2. For each parameter with a name:
   - Print parameter name and level (GETPALAMLV with 15 levels)
   - Display progress bar:
     - If LV >= 15: show "********"
     - Else: show 8-character bar based on progress to next level
   - Format value with SI prefix (k/M/G/T/P) for large numbers
   - Special case: PALAM:9 (摩擦) shows "摩擦 0" if >= 20000
   - Use PRINTCPERLINE() for layout (columns per line)
3. Print final newline

**External Dependencies**:
- `GETPALAMLV(PALAM:(ARG:0):LOCAL, 15)`: Parameter level calculation → Pass as `Func<int, int, int>`
- `PRINTCPERLINE()`: Display width calculation → Pass as `Func<int>` (for layout decisions, not actual printing)

**Variables**: PALAM, PALAMNAME, PALAMLV

---

### SORT_CFLAG (lines 615-629)

**Purpose**: Sort characters by CFLAG value in descending order.

**Algorithm**:
1. Initialize LOCAL and RESULT arrays
2. For each character slot (1 to CHARANUM):
   - Find character with highest CFLAG:ARG value not yet in RESULT
   - Store character ID in RESULT array
3. Returns sorted character IDs in RESULT:1 to RESULT:CHARANUM

**External Dependencies**: None (utility function)

**Variables**: CFLAG (indexed by ARG), RESULT (output array), LOCAL (temp variables)

---

## InfoState API

### CalculateCharacterAttitude

**Signature**:
```csharp
public static int CalculateCharacterAttitude(
    int characterId,
    int charCount,
    Func<int, int, int, int> getMarkLevel,
    Func<int, int, int> getPalamLv,
    Func<int, int, int> getCflag,
    Action<int, int, int> setCflag,
    Func<int, int, int> getTalent,
    Func<int, int, int> getAbl,
    Func<int, int, int> getPalam,
    Func<int, int, int> getBase)
```

**Purpose**: Calculate character attitude (馴れ合い強度) representing how aggressively characters act toward MASTER. Source: INFO.ERB @CHARA_ATTITUDE (lines 636-689).

**Parameters**:
- `characterId`: Target character ID
- `charCount`: Total character count
- `getMarkLevel`: MARK level getter (charId, markType, masterId) => level
- `getPalamLv`: PALAM level getter (palamValue, levelMax) => level
- `getCflag`: CFLAG getter (charId, index) => value
- `setCflag`: CFLAG setter (charId, index, value)
- `getTalent`: TALENT getter (charId, index) => value
- `getAbl`: ABL getter (charId, index) => value
- `getPalam`: PALAM getter (charId, index) => value
- `getBase`: BASE getter (charId, index) => value

**Return Value**: Calculated intensity value (馴れ合い強度度) ranging from 0 (skinship only) to 3+ (push down).

**Usage Example**:
```csharp
// Calculate attitude for character 1
int intensity = InfoState.CalculateCharacterAttitude(
    characterId: 1,
    charCount: 50,
    getMarkLevel: (charId, markType, masterId) => MockMarkLevel(charId, markType),
    getPalamLv: (value, maxLevel) => MockPalamLevel(value, maxLevel),
    getCflag: (charId, index) => MockCflag[charId, index],
    setCflag: (charId, index, value) => MockCflag[charId, index] = value,
    getTalent: (charId, index) => MockTalent[charId, index],
    getAbl: (charId, index) => MockAbl[charId, index],
    getPalam: (charId, index) => MockPalam[charId, index],
    getBase: (charId, index) => MockBase[charId, index]
);

// Result: intensity value from 0-3+ indicating character's aggressiveness
```

---

### SetTrainingTarget

**Signature**:
```csharp
public static int SetTrainingTarget(
    int masterLocation,
    int charCount,
    int[] targetArray,
    Func<int, int, int> getCflag,
    Action<int, int, int> setTcvar,
    Func<int> getTflagCurrentTarget,
    Action<int> setTflagCurrentTarget)
```

**Purpose**: Select training target and set up 馴れ合い強度 for characters in the same room. Source: INFO.ERB @INFO_SetTarget (lines 1081-1119).

**Parameters**:
- `masterLocation`: MASTER's current location
- `charCount`: Total character count
- `targetArray`: TARGET array to populate (modified in-place)
- `getCflag`: CFLAG getter (charId, index) => value
- `setTcvar`: TCVAR setter (charId, index, value)
- `getTflagCurrentTarget`: TFLAG:現在のTARGET getter
- `setTflagCurrentTarget`: TFLAG:現在のTARGET setter

**Return Value**: Selected target ID (0 if no valid target).

**Usage Example**:
```csharp
// Select training target in location 5
int[] targetArray = new int[50];
int selectedTarget = InfoState.SetTrainingTarget(
    masterLocation: 5,
    charCount: 50,
    targetArray: targetArray,
    getCflag: (charId, index) => MockCflag[charId, index],
    setTcvar: (charId, index, value) => MockTcvar[charId, index] = value,
    getTflagCurrentTarget: () => MockPreviousTarget,
    setTflagCurrentTarget: (target) => MockPreviousTarget = target
);

// Result: selectedTarget contains chosen character ID
// targetArray[0] = selectedTarget, targetArray[1..n] = other characters in room
// TCVAR:馴れ合い強度 set for all characters in room
```

---

### SetTrainMode

**Signature**:
```csharp
public static int SetTrainMode(
    int targetId,
    int masterLocation,
    int charCount,
    Func<int, int, int> getCflag,
    Func<int, int, int> canMove,
    Func<int> getTargetNum,
    Func<int, bool> checkStainSemen,
    Func<int, int, int, int> getMarkLevel,
    Func<int, int, int> getPalamLv)
```

**Purpose**: Manage train mode state transitions (うふふモード, 押し倒し, 告白). Source: INFO.ERB @INFO_SetTrainMode state logic (lines 1125-1208). **Note**: Display logic (lines 1209-1262) deferred to F382.

**Parameters**:
- `targetId`: Target character ID
- `masterLocation`: MASTER's current location
- `charCount`: Total character count
- `getCflag`: CFLAG getter (charId, index) => value
- `canMove`: CAN_MOVE function (from, to) => result
- `getTargetNum`: GET_TARGETNUM function () => count
- `checkStainSemen`: CHK_350_STAIN_SEMEN function (charId) => result
- `getMarkLevel`: GET_MARK_LEVEL function (charId, markType, masterId) => level
- `getPalamLv`: GETPALAMLV function (palamValue, levelMax) => level

**Return Value**: COMABLE管理 state (0-3):
- 0: No target or target sleeping
- 1: Default awake state
- 2: うふふモード stage 1
- 3: うふふモード stage 2 or 押し倒し

**Usage Example**:
```csharp
// Determine train mode state for target
int comableState = InfoState.SetTrainMode(
    targetId: 1,
    masterLocation: 5,
    charCount: 50,
    getCflag: (charId, index) => MockCflag[charId, index],
    canMove: (from, to) => MockCanMove(from, to),
    getTargetNum: () => MockTargetCount,
    checkStainSemen: (charId) => MockStainCheck(charId),
    getMarkLevel: (charId, markType, masterId) => MockMarkLevel(charId, markType),
    getPalamLv: (value, maxLevel) => MockPalamLevel(value, maxLevel)
);

// Result: comableState indicates available training mode (0-3)
// Used by SHOW_STATUS to determine available commands
```

---

### SortCflag

**Signature**:
```csharp
public static int[] SortCflag(
    int cflagIndex,
    int charCount,
    Func<int, int, int> getCflag)
```

**Purpose**: Sort characters by CFLAG value in descending order. Source: INFO.ERB @SORT_CFLAG (lines 615-629).

**Parameters**:
- `cflagIndex`: CFLAG index to sort by
- `charCount`: Total character count
- `getCflag`: CFLAG getter (charId, index) => value

**Return Value**: Sorted character IDs (descending by CFLAG value). Array length matches `charCount`.

**Usage Example**:
```csharp
// Sort characters by 館内地位 (status in mansion)
int[] sortedChars = InfoState.SortCflag(
    cflagIndex: 310, // 館内地位
    charCount: 50,
    getCflag: (charId, index) => MockCflag[charId, index]
);

// Result: sortedChars[0] = character with highest 館内地位
//         sortedChars[1] = character with second highest, etc.
// Used by SetTrainingTarget for target selection priority
```

---

### FormatInfoPalam

**Signature**:
```csharp
public static string FormatInfoPalam(
    int characterId,
    Func<int, string> getPalamName,
    Func<int, int> getPalam,
    Func<int, int, int> getPalamLevel,
    Func<int> getPrintWidth)
```

**Purpose**: Format parameter information with progress bars and formatted values for display. Source: INFO.ERB @SHOW_INFO_PALAM (lines 555-609).

**Parameters**:
- `characterId`: Character ID
- `getPalamName`: PALAMNAME getter (index) => name
- `getPalam`: PALAM getter (index) => value
- `getPalamLevel`: GETPALAMLV function (value, maxLevel) => level
- `getPrintWidth`: PRINTCPERLINE function () => columns per line

**Return Value**: Formatted parameter info string with progress bars and SI-formatted values.

**Usage Example**:
```csharp
// Format parameter display for character 1
string palamInfo = InfoState.FormatInfoPalam(
    characterId: 1,
    getPalamName: (index) => MockPalamNames[index],
    getPalam: (index) => MockPalam[1, index],
    getPalamLevel: (value, maxLevel) => CalculateLevel(value, maxLevel),
    getPrintWidth: () => 3 // 3 columns per line
);

// Result: "欲情 Lv3 ***----- 1500 好意 Lv5 *****--- 2500 ...\n"
// Format: "{name} Lv{level} {bar} {value}" with SI prefixes (k/M/G/T/P)
// Used by display layer (F382) for INFO command output
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | fl | Created as part of INFO.ERB split (F373 scope reduction) | PROPOSED |
| 2026-01-06 19:09 | START | implementer | Task 1 - Analyze state functions and create RED state tests | - |
| 2026-01-06 19:09 | END | implementer | Task 1 - Documented 5 functions + created InfoStateTests.cs (11 tests, RED state confirmed) | SUCCESS |
| 2026-01-06 19:14 | START | implementer | Task 2 - Create InfoState.cs implementation | - |
| 2026-01-06 19:14 | END | implementer | Task 2 - Created InfoState.cs, build succeeds, 11/11 tests pass (GREEN state) | SUCCESS |
| 2026-01-06 19:17 | START | implementer | Task 7 - Document InfoState API with usage examples | - |
| 2026-01-06 19:17 | END | implementer | Task 7 - Added InfoState API documentation (5 methods with examples), build succeeds | SUCCESS |
