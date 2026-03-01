# Feature 382: INFO.ERB Migration - TrainMode Display Functions

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate display logic from INFO_SetTrainMode to C# InfoTrainModeDisplay.cs. This is the display portion split from F381.

**Context**: Part of INFO.ERB split: F373 (Print), F381 (State), F382 (TrainMode Display), F378 (Event), F379 (Equipment), F380 (Orchestration).

**Origin**: Display logic (lines 1209-1262) extracted from INFO_SetTrainMode per F381 scope split.

---

## Background

### Philosophy (Mid-term Vision)

**Separation of Concerns**: State management and display logic should be separate. F381 handles state calculation, F382 handles the display of character lists during train mode initialization.

### Problem (Current Issue)

INFO_SetTrainMode display logic (lines 1209-1262):
- Character list display with PRINTFORM
- Color-coded character names (SETCOLOR/RESETCOLOR)
- Interactive buttons (PRINTBUTTON)
- NTR name display (NTR_NAME)

**Current State**:
- Display logic embedded in state management function
- Tight coupling between state calculation and UI output
- Cannot be unit tested without display subsystem

### Display Functions Analysis

**Scope**: Lines 1209-1262 (two distinct display sections)

**Section 1: Character List Display (lines 1209-1251)**
- "ここにいる:" header
- Visitor button (if CFLAG:訪問者:状態)
- Intruder entry (if こいし present)
- Character loop with PRINTBUTTON

**Section 2: Target Character Stats Display (lines 1253-1262)**
- 好感度 (favorability) display
- 屈服度 (submission) display
- 怒り (anger) status display

**Data Dependencies**:
- Character list from iterating CFLAG:現在位置 == POS_NOW (not from SetTrainMode)
- SetTrainMode provides COMABLE管理 state (0-3) for command availability
- CFLAG:睡眠, CFLAG:同行 for status display
- Visitor flag (CFLAG:訪問者:状態)
- Intruder flag (こいし presence detection)

**External ERB Functions Called**:
- `NTR_NAME(0)` - Get NTR target name for visitor display → Pass as Func<int, string> parameter
- `PRINTBUTTON` - Interactive character selection
- `SETCOLOR/RESETCOLOR` - Color-coded status display

**Display Algorithm**:
1. Check visitor status → display "NTR_NAME(0)に会いに行きます" button
2. Check intruder status → display "こいし" entry
3. Loop through character list (CFLAG:現在位置 == POS_NOW):
   - Apply color based on CFLAG:奴隷:怒り (anger status)
   - Format: "[name] [status_indicator]"
   - Create PRINTBUTTON for each character
4. Display target character stats (好感度/屈服度/怒り)

**Output Format**:
- Character buttons with color coding (normal/angry state)
- Visitor special button (if applicable)
- Intruder special entry (if applicable)
- Target character stats panel

### Goal (What to Achieve)

1. Create standalone Era.Core/Common/InfoTrainModeDisplay.cs with display functions
2. Implement display helpers that accept character list and status data
3. Create xUnit test cases for display logic
4. Document API for future F380 integration (F380 will call F382's API when implemented)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | InfoTrainModeDisplay.cs created | file | Glob Era.Core/Common/InfoTrainModeDisplay.cs | exists | Era.Core/Common/InfoTrainModeDisplay.cs | [x] |
| 2 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 3 | Display tests created | file | Glob Era.Core.Tests/InfoTrainModeDisplayTests.cs | exists | Era.Core.Tests/InfoTrainModeDisplayTests.cs | [x] |
| 4 | All display tests pass (Pos) | test | dotnet test --filter InfoTrainModeDisplayTests | succeeds | - | [x] |
| 5 | Minimum test coverage exists | code | Grep "\\[Fact\\]" in Era.Core.Tests/InfoTrainModeDisplayTests.cs | gte | 4 | [x] |
| 6 | Empty list test exists (Neg) | code | Grep "Empty" in Era.Core.Tests/InfoTrainModeDisplayTests.cs | contains | "Empty" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create Era.Core/Common/InfoTrainModeDisplay.cs with display functions | [x] |
| 2 | 2 | Verify C# build succeeds after creation | [x] |
| 3 | 3 | Create Era.Core.Tests/InfoTrainModeDisplayTests.cs using xUnit patterns | [x] |
| 4 | 4 | Run all display tests and verify they pass | [x] |
| 5 | 5 | Verify test coverage: ensure minimum 4 test methods covering (1) character list with multiple characters, (2) visitor display, (3) intruder display, (4) empty character list | [x] |
| 6 | 6 | Add negative test for empty character list handling | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F381 | Requires InfoState.cs for state data (SetTrainMode result) |
| Predecessor | F373 | Follows F373 patterns for parameterized display functions |
| Successor | F380 | SHOW_STATUS orchestration depends on display functions |

---

## Links

- [feature-381.md](feature-381.md) - State Management (predecessor, scope split origin)
- [feature-373.md](feature-373.md) - Print Display (predecessor)
- [feature-380.md](feature-380.md) - SHOW_STATUS (successor)
- Game/ERB/INFO.ERB - Source file (lines 1209-1262)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | fl | Created as display logic split from F381 | PROPOSED |
| 2026-01-06 20:21 | START | implementer | Task 3 (tests) | - |
| 2026-01-06 20:21 | END | implementer | Task 3 (tests) | SUCCESS |
| 2026-01-06 20:25 | START | implementer | Task 1 (implementation) | - |
| 2026-01-06 20:25 | END | implementer | Task 1 (implementation) | SUCCESS |
| 2026-01-06 20:30 | VERIFY | ac-tester | All ACs verified | PASS:6/6 |
