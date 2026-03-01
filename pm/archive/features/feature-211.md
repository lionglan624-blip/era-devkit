# Feature 211: Empty Line Input Bug Fix

## Status: [DONE]

## Type: engine

## Depends: []

## Background

### Philosophy

F210 実装中に発見されたエンジンバグ。input ファイルの空行が WAIT 確認として機能しない。

### Problem

`HeadlessWindow.BufferInputFromFile()` が空行をスキップしている：

```csharp
// HeadlessWindow.cs line 83
if (!string.IsNullOrEmpty(line))
{
    inputQueue_.Enqueue(line);
}
```

**影響**:
- PRINTFORMW（WAIT付き出力）後のエンター確認が無視される
- テストシナリオで空行を入れても WAIT が解消されない
- var_equals 検証が WAIT 中に実行され、FAIL になる

### Root Cause

F210 テスト実行で観察された動作：
```
100 (確認) 入力
美鈴は[思慕]を失い、[恋慕]を得た     ← PRINTFORML
美鈴の部屋の合鍵を貰った              ← PRINTFORMW (WAIT)
[Headless] Input queue exhausted      ← 空行がキューにない
[Headless] Game stabilized - stopping
---
var_equals: FAIL                       ← TALENT:恋慕=1 が設定される前に終了
```

PRINTFORMW 後に `TALENT:奴隷:恋慕 = 1` が実行される前にゲームが終了。

### Goal

1. HeadlessWindow.cs を修正し、空行も input queue に入れる
2. input ファイルの空行が WAIT 確認として機能する
3. F210 の var_equals 検証が PASS する

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Enqueue呼び出しあり | code | grep | contains | inputQueue_.Enqueue(line) | [x] |
| 2 | IsNullOrEmptyフィルタ削除 | code | grep | not_contains | IsNullOrEmpty(line) | [x] |
| 3 | 空白行がキューに追加される | exit_code | dotnet test | equals | 0 | [x] |
| 4 | sc-002 var_equals PASS | output | --flow | contains | "var_equals: PASS" | [x] |
| 5 | sc-004 var_equals PASS | output | --flow | contains | "var_equals: PASS" | [x] |
| 6 | 24/24 PASS | output | --flow | contains | PASS: 24/24 | [x] |
| 7 | dotnet build 成功 | build | dotnet | succeeds | - | [x] |
| 8 | dotnet test 成功 | exit_code | dotnet | equals | 0 | [x] |

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | 1 | Enqueue呼び出し確認（grep） | HeadlessWindow.cs | [O] |
| 2 | 2 | IsNullOrEmptyフィルタ削除 | HeadlessWindow.cs | [O] |
| 3 | 3 | 空白行ユニットテスト作成・実行 (dotnet test) | engine.Tests/ | [O] |
| 4 | 4 | sc-002 var_equals テスト | tests/regression/ | [O] |
| 5 | 5 | sc-004 var_equals テスト | tests/regression/ | [O] |
| 6 | 6 | 24/24 回帰テスト | tests/regression/ | [O] |
| 7 | 7 | dotnet build 成功確認 | engine/ | [O] |
| 8 | 8 | dotnet test 成功確認 | engine.Tests/ | [O] |

---

## Technical Details

### 修正箇所

**ファイル**: `engine/Assets/Scripts/Emuera/Headless/HeadlessWindow.cs`

**Before (line 81-87)**:
```csharp
var lines = System.IO.File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
foreach (var line in lines)
{
    if (!string.IsNullOrEmpty(line))
    {
        inputQueue_.Enqueue(line);
    }
}
```

**After**:
```csharp
var lines = System.IO.File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
foreach (var line in lines)
{
    // Empty lines are valid input for WAIT confirmation (PRINTFORMW, etc.)
    inputQueue_.Enqueue(line);
}
```

### 空行の意味

| 入力 | 意味 |
|------|------|
| `888` | コマンド888を実行 |
| `` (空行) | エンター押下 = WAIT確認 |

WAIT状態（PRINTFORMW, INPUT など）では、空行がエンター確認として機能する必要がある。

### テストで確認

修正後、F210 のテストシナリオで：
1. `100` (確認) 入力
2. EVENTTURNEND 開始
3. CHK_FALL_IN_LOVE → PRINTFORMW
4. 空行 (エンター) → WAIT解消
5. `TALENT:奴隷:恋慕 = 1` 設定
6. ゲーム終了
7. var_equals: PASS

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-25 | - | F210 実装中に発見、分離して作成 | PROPOSED |
| 2025-12-25 | initializer | Initialize Feature 211 - Engine C# fix | WIP |
| 2025-12-25 | implementer | TDD RED: HeadlessWindowTests作成、FAIL確認 | RED |
| 2025-12-25 | implementer | IsNullOrEmptyフィルタ削除、GREEN確認 | GREEN |
| 2025-12-25 | ac-tester | AC検証 | OK:8/8 |
| 2025-12-25 | regression-tester | 回帰テスト | OK:24/24 |
| 2025-12-25 | feature-reviewer | Post-review | READY |
| 2025-12-25 | finalizer | Finalize Feature 211 - All objectives achieved | DONE |

---

## Links

- [feature-210.md](feature-210.md) - Scenario Input Sequence Fix（本Featureが前提）
- [HeadlessWindow.cs](../../engine/Assets/Scripts/Emuera/Headless/HeadlessWindow.cs)
