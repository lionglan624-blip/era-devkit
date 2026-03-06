# Feature 427: ShiftJisHelper共通化

## Status: [DONE]

## Type: engine

## Created: 2026-01-09

---

## Summary

Extract duplicated HalfwidthChars HashSet and GetByteCount/IsHalfwidth methods from StringFunctions.cs and ArrayFunctions.cs to a shared ShiftJisHelper class.

---

## Background

### Philosophy
DRY (Don't Repeat Yourself) - Establish ShiftJisHelper as the single source of truth for all Shift-JIS byte counting operations across Era.Core, ensuring consistent encoding behavior and simplified future maintenance.

### Problem
F419実装時に発見: StringFunctions.cs と ArrayFunctions.cs で HalfwidthChars HashSet と関連メソッドが重複している。

重複コード:
- `HalfwidthChars` HashSet (半角文字セット)
- `IsHalfwidth(char c)` メソッド
- `GetByteCount(string str)` メソッド

### Goal
共通クラス `ShiftJisHelper.cs` を作成し、重複コードを統合する。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ShiftJisHelper.cs exists | file | Glob | exists | Era.Core/Encoding/ShiftJisHelper.cs | [x] |
| 2 | StringFunctions uses ShiftJisHelper | code | Grep | contains | ShiftJisHelper.GetByteCount | [x] |
| 3 | ArrayFunctions uses ShiftJisHelper | code | Grep | contains | ShiftJisHelper.GetByteCount | [x] |
| 4 | No duplicate HalfwidthChars in StringFunctions | code | Grep | not_contains | HalfwidthChars | [x] |
| 5 | No duplicate HalfwidthChars in ArrayFunctions | code | Grep | not_contains | HalfwidthChars | [x] |
| 6 | All tests pass | test | dotnet test | succeeds | --filter Category=DataFunctions | [x] |
| 7 | IsHalfwidth is public | code | Grep | contains | public static bool IsHalfwidth | [x] |
| 8 | ByteIndexToCharIndex uses ShiftJisHelper.IsHalfwidth | code | Grep | contains | ShiftJisHelper.IsHalfwidth | [x] |

### AC Details

**AC#1**: ShiftJisHelper.cs exists at Era.Core/Encoding/ShiftJisHelper.cs
- Contains: `public static class ShiftJisHelper`
- Public methods: `public static int GetByteCount(string str)`, `public static bool IsHalfwidth(char c)`
- Contains: `private static readonly HashSet<char> HalfwidthChars`

**AC#2**: StringFunctions.cs uses ShiftJisHelper
- Grep: `ShiftJisHelper.GetByteCount` in Era.Core/Functions/StringFunctions.cs

**AC#3**: ArrayFunctions.cs uses ShiftJisHelper
- Grep: `ShiftJisHelper.GetByteCount` in Era.Core/Functions/ArrayFunctions.cs

**AC#4/5**: No duplicate HalfwidthChars
- Grep: `HalfwidthChars` should NOT match in StringFunctions.cs (AC#4)
- Grep: `HalfwidthChars` should NOT match in ArrayFunctions.cs (AC#5)

**AC#6**: All tests pass
- Command: `dotnet test Era.Core.Tests --filter Category=DataFunctions`

**AC#7**: IsHalfwidth is public
- Grep: `public static bool IsHalfwidth` in Era.Core/Encoding/ShiftJisHelper.cs

**AC#8**: ByteIndexToCharIndex uses ShiftJisHelper.IsHalfwidth
- Grep: `ShiftJisHelper.IsHalfwidth` in Era.Core/Functions/StringFunctions.cs

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,7 | Create Era.Core/Encoding/ folder and ShiftJisHelper.cs with public methods | [x] |
| 2 | 2,3,4,5,8 | Refactor StringFunctions/ArrayFunctions to use ShiftJisHelper and remove duplicates | [x] |
| 3 | 6 | Verify all tests pass | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F419 | Source of duplication |

---

## Review Notes

- **2026-01-09 FL iter1**: [resolved] AC#2/AC#3 - IsHalfwidth method coverage → Added AC#7/AC#8 for public IsHalfwidth
- **2026-01-09 FL iter2**: [resolved] AC Details lists IsHalfwidth as method → Added AC#7 to verify public accessibility
- **2026-01-09 FL iter3**: [resolved] Task#2/Task#3 merged per user decision

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 | create | opus | Created as follow-up from F419 Out-of-Scope | PROPOSED |
| 2026-01-09 21:39 | END | implementer | Task 1 | SUCCESS |
| 2026-01-09 21:41 | START | implementer | Task 2 | - |
| 2026-01-09 21:41 | END | implementer | Task 2 | SUCCESS |
| 2026-01-09 21:42 | END | opus | Task 3 (verify tests) | SUCCESS |
| 2026-01-09 21:42 | DEVIATION | opus | Architecture test | Updated static class count 7→8 |

---

## Links

- [feature-419.md](feature-419.md) - 親Feature (重複発見元)
- [index-features.md](index-features.md)
