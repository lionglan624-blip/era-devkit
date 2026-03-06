# Feature 379: INFO.ERB Migration - Equipment and Stain Display

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate equipment display and stain info functions from INFO.ERB to C# InfoEquip.cs. This is Part 4 of INFO.ERB migration.

**Context**: Part of INFO.ERB split: F373 (Print), F381 (State), F378 (Event), F379 (Equipment), F380 (Orchestration).

---

## Background

### Philosophy (Mid-term Vision)

**Foundation-First Migration**: Equipment and stain display functions render complex character state information. These are display functions but with heavier runtime dependencies than INFO_Print* functions.

### Problem (Current Issue)

Equipment/stain display functions in INFO.ERB:
- STAIN_INFO (lines 215-406, ~192 lines) - Detailed stain/dirty state display
- SHOW_EQUIP_1 (lines 440-502, ~63 lines) - Equipment display type 1
- SHOW_EQUIP_2 (lines 419-433, ~15 lines) - Equipment display type 2
- SHOW_EQUIP_3 (lines 508-550, ~43 lines) - Equipment display type 3 (parameterized)
- SAVEINFO (lines 411-413, ~3 lines) - Save info display

**Deferred to F381**:
- SHOW_INFO_PALAM (lines 555-609) - Depends on GETPALAMLV (not migrated)
- SORT_CFLAG (lines 615-629) - Character flag sorting utility (used by INFO_SetTarget)

**Current State**:
- STAIN_INFO is large and complex (~192 lines)
- SHOW_EQUIP_* functions display character equipment
- Functions depend on TEQUIP, STAIN, CFLAG arrays

### Goal (What to Achieve)

1. Analyze equipment/stain functions and document dependencies
2. Create Era.Core/Common/InfoEquip.cs with display utilities
3. Implement type-safe wrappers with appropriate interfaces
4. Create xUnit test cases for display functions
5. Document InfoEquip API for Phase 4+ reference

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Equipment functions analysis documented | file | Grep "## Equipment Functions Analysis" in Game/agents/feature-379.md | contains | "## Equipment Functions Analysis" | [x] |
| 2 | InfoEquip.cs created | file | Glob Era.Core/Common/InfoEquip.cs | exists | Era.Core/Common/InfoEquip.cs | [x] |
| 3 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 4 | InfoEquip tests created | file | Glob Era.Core.Tests/InfoEquipTests.cs | exists | Era.Core.Tests/InfoEquipTests.cs | [x] |
| 5 | All equipment tests pass (Pos) | test | dotnet test --filter InfoEquipTests | succeeds | - | [x] |
| 6 | Minimum test coverage exists | code | Grep "\\[Fact\\]" in Era.Core.Tests/InfoEquipTests.cs (count mode) | gte | 8 | [x] |
| 7 | API documentation created | file | Grep "## InfoEquip API" in Game/agents/feature-379.md | contains | "## InfoEquip API" | [x] |
| 8 | Empty equipment test exists (Neg) | code | Grep "Empty" in Era.Core.Tests/InfoEquipTests.cs | contains | "Empty" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze STAIN_INFO, SHOW_EQUIP_*, SAVEINFO: document runtime dependencies | [x] |
| 2 | 2 | Create Era.Core/Common/InfoEquip.cs with equipment display functions | [x] |
| 3 | 3 | Verify C# build succeeds after InfoEquip.cs creation | [x] |
| 4 | 4 | Create Era.Core.Tests/InfoEquipTests.cs using xUnit patterns | [x] |
| 5 | 5 | Run all InfoEquip tests and verify they pass | [x] |
| 6 | 6 | Verify test coverage: ensure minimum 8 test methods exist | [x] |
| 7 | 7 | Document InfoEquip API with usage examples in feature-379.md | [x] |
| 8 | 8 | Add negative test for empty equipment handling in InfoEquipTests.cs | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F373 | Follows F373 patterns for C# migration |
| Predecessor | F366 | Requires CommonFunctions.cs |
| Predecessor | F364 | Requires Constants.cs |
| Successor | F380 | SHOW_STATUS depends on equipment display |

---

## Links

- [feature-373.md](feature-373.md) - Print Display (predecessor)
- [feature-366.md](feature-366.md) - CommonFunctions (predecessor)
- [feature-364.md](feature-364.md) - Constants (predecessor)
- [feature-381.md](feature-381.md) - State Management (sibling)
- [feature-378.md](feature-378.md) - Event Handling (sibling)
- [feature-380.md](feature-380.md) - SHOW_STATUS (successor)
- Game/ERB/INFO.ERB - Source file (lines 215-550)

---

## Equipment Functions Analysis

### STAIN_INFO (INFO.ERB:215-406)

**Purpose**: Display detailed stain/dirty state for MASTER, TARGET, and ASSI characters.

**Runtime Dependencies**:
- `STAIN[characterId][part]` - Stain bitflags per body part (8 parts: mouth, hand, P, V, A, B, vaginal, intestinal)
- `HAS_PENIS(charId)` - Gender check (from CommonFunctions.cs)
- `HAS_VAGINA(charId)` - Gender check (from CommonFunctions.cs)
- `FLAG:汚れ非表示設定` - Hidden stain display flag
- `FLAG:ＮＴＲパッチ設定` - NTR patch setting

**Stain Types** (bitflags):
- bit 0 (1): 愛液 (love juice)
- bit 1 (2): ペニス (penis)
- bit 2 (4): 精液 (semen)
- bit 3 (8): アナル (anal)
- bit 4 (16): 母乳 (breast milk)
- bit 5 (32): 粘液 (mucus)
- bit 6 (64): 破瓜の血 (virgin blood)

### SHOW_EQUIP_2 (INFO.ERB:419-433)

**Purpose**: Display global play mode equipment (video recording, outdoor, shame, bath, newlywed).

**Runtime Dependencies**:
- `TEQUIP:28` - Video recording state
- `TEQUIP:野外プレイ` - Outdoor play mode (1=indoor exposure, 2=outdoor)
- `TEQUIP:30` - Shame play flag
- `TEQUIP:お風呂場プレイ` - Bath play
- `TEQUIP:33` - Newlywed play

### SHOW_EQUIP_1 (INFO.ERB:440-502)

**Purpose**: Display TARGET-specific equipment (insertion state and active equipment list).

**Runtime Dependencies**:
- `INVAGINA(TARGET)` - From CommonFunctions.cs (returns 0=none, 1=penis, 2=vibrator)
- `INANAL(TARGET)` - From CommonFunctions.cs (returns 0=none, 1=penis, 2=anal vibrator, 3=anal beads)
- `TEQUIP[TARGET][11-27,32,34,36,膣鏡]` - Equipment state flags

### SHOW_EQUIP_3 (INFO.ERB:508-550)

**Purpose**: Display non-removable equipment (rotors, chastity belt, caps) with state.

**Runtime Dependencies**:
- `CFLAG[charId]:ローター挿入` - Rotor inserted
- `CFLAG[charId]:ローターA挿入` - Anal rotor inserted
- `CFLAG[charId]:ペニス用貞操帯着用` - Chastity belt
- `CFLAG[charId]:WC_ニプルキャップ装着` - Nipple cap state (0=none, 1=stopped, >1=active)
- Additional CFLAG for clit cap, vibrator, anal vibrator, rotors

### SAVEINFO (INFO.ERB:411-413)

**Purpose**: Simple day display for save file.

**Runtime Dependencies**:
- `DAY` - Global day counter

---

## InfoEquip API

### Overview

`Era.Core.Common.InfoEquip` provides C# implementations of equipment and stain display functions from INFO.ERB.

### Methods

#### FormatSaveInfo

```csharp
public static string FormatSaveInfo(int day)
```

Formats day display for save info.

**Parameters**:
- `day`: Current day number

**Returns**: Formatted string (e.g., "5日目")

**ERB Source**: `@SAVEINFO @ INFO.ERB:411-413`

#### FormatShowEquip2

```csharp
public static string FormatShowEquip2(
    bool isVideoRecording, int tapeRemaining,
    int outdoorPlayMode, bool shamePlay, bool bathPlay, bool newlywedPlay)
```

Formats global equipment state display.

**Parameters**:
- `isVideoRecording`: Video recording active flag
- `tapeRemaining`: Number of tapes remaining
- `outdoorPlayMode`: 0=none, 1=indoor exposure, 2=outdoor
- `shamePlay`: Shame play active flag
- `bathPlay`: Bath play active flag
- `newlywedPlay`: Newlywed play active flag

**Returns**: Concatenated equipment state string

**ERB Source**: `@SHOW_EQUIP_2 @ INFO.ERB:419-433`

#### FormatShowEquip1

```csharp
public static string FormatShowEquip1(
    string targetName, int vInsertionType, int aInsertionType, bool[] activeEquipment)
```

Formats TARGET equipment display.

**Parameters**:
- `targetName`: Target character name
- `vInsertionType`: Vaginal insertion (0=none, 1=penis, 2=vibrator)
- `aInsertionType`: Anal insertion (0=none, 1=penis, 2=anal vibrator, 3=anal beads)
- `activeEquipment`: Active equipment array (TEQUIP simulation)

**Returns**: Formatted equipment display string

**ERB Source**: `@SHOW_EQUIP_1 @ INFO.ERB:441-502`

#### FormatShowEquip3

```csharp
public static string FormatShowEquip3(
    int characterId, bool rotorInserted, bool analRotorInserted, bool chastityBelt,
    int nippleCapState, int clitCapState, int vibratorState, int analVibratorState,
    int cRotorState, int vRotorState, int aRotorState)
```

Formats non-removable equipment display.

**Parameters**:
- `characterId`: Character ID
- `rotorInserted`: Rotor inserted flag
- `analRotorInserted`: Anal rotor inserted flag
- `chastityBelt`: Chastity belt equipped flag
- `*State`: Equipment state (0=none, 1=stopped, >1=active)

**Returns**: Formatted non-removable equipment display string

**ERB Source**: `@SHOW_EQUIP_3 @ INFO.ERB:508-550`

#### GetStainTypes

```csharp
public static string[] GetStainTypes(int stainFlags)
```

Extracts stain type names from bitflags.

**Parameters**:
- `stainFlags`: Stain flags bitfield

**Returns**: Array of stain type names (愛液, ペニス, 精液, etc.)

**ERB Source**: Part of `@STAIN_INFO @ INFO.ERB:249-262`

#### FormatStainInfo

```csharp
public static string FormatStainInfo(
    string characterName, bool hasPenis, bool hasVagina, int[] stainArray)
```

Formats detailed stain info display.

**Parameters**:
- `characterName`: Character name
- `hasPenis`: Has penis flag (affects P display)
- `hasVagina`: Has vagina flag (affects V display)
- `stainArray`: Stain array [8] for body parts

**Returns**: Formatted stain info display string

**ERB Source**: `@STAIN_INFO @ INFO.ERB:215-406`

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | fl | Created as part of INFO.ERB split (F373 scope reduction) | PROPOSED |
| 2026-01-06 18:46 | START | implementer | Task 2 | - |
| 2026-01-06 18:46 | END | implementer | Task 2 | SUCCESS |
