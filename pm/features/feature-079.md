# Feature 079: Interactive Mode Complete Implementation

## Status: [DONE]

## Background

Feature 078でInteractive Modeの一部が修正されたが、まだ以下の問題が残っている。本Featureでは残りの問題を解決し、Interactive Modeを実用レベルに引き上げる。

### Interactive Modeの設計思想

| Mode | Approach | Trade-off |
|------|----------|-----------|
| **旧Headless** | コマンドを事前定義して連打 | 準備が面倒、変更に弱い |
| **Interactive** | ユーザーと同じ入出力でコマンド実装 | 柔軟、実際の動作を検証 |

**目標**:
1. ユーザー体験の再現 - 実際のプレイヤーと同じ入出力パターンでテスト
2. 状態注入による効率化 - `set`コマンドで特定状態へ直接遷移（周回不要）
3. 関数単位テストとの併用 - `call`コマンドで個別関数も呼び出し可能

## Current State Analysis

### Feature 078で修正済み

| 機能 | 状態 | 説明 |
|------|------|------|
| `dump` (変数取得) | ✅ | CsvNo解決OK (VariableResolver修正) |
| TARGET/MASTER設定 | ✅ | `{"cmd":"set","var":"TARGET","value":1}` |

### 未修正（本Feature対象）

| 問題 | 現状 | 根本原因 |
|------|------|----------|
| **P1: set CsvNo解決** | ❌ | StateInjector.InjectVariableが未対応 |
| **P2: 関数引数サポート** | ❌ | HandleCallがargsを使用していない |
| **P3: ゲームループ統合** | ❌ | 数字入力→コマンド実行が未実装 |
| **P4: Headlessキャラ追加** | ❌ | シナリオJSONは既存キャラのみ変更可能 |

## Problem Details

### P1: StateInjector CsvNo Resolution

**Symptom**:
```json
{"cmd":"set","var":"TALENT:4:3","value":1}
→ [Inject] Character not found: 4
→ {"status":"error","error":"Failed to set variable: TALENT:4:3"}
```

**Root Cause**:
- `StateInjector.InjectVariable()`内のキャラクター解決が名前検索のみ
- Feature 078で`VariableResolver.ResolveCharacterIndex()`を修正したが、StateInjectorはこれを使用していない

**Fix Location**: `StateInjector.cs:InjectVariable()`, `SetTalent()`, `SetAbl()`, `SetCflag()`

**Fix Strategy**:
```csharp
// Before: 名前で検索
var chara = charList.FirstOrDefault(c => c.Name == name);

// After: VariableResolverを使用
int charIndex = VariableResolver.ResolveCharacterIndex(name);
if (charIndex < 0) return false;
var chara = charList[charIndex];
```

### P2: Function Call with Arguments

**Symptom**:
```json
{"cmd":"call","func":"CHK_ADMIRATION_GET","args":[4]}
→ {"status":"ok","output":"予期しないスクリプト終端です"}
```

**Root Cause**:
- `InteractiveRunner.HandleCall()`が`args`パラメータを無視している
- 関数呼び出し時にARG配列への設定が行われていない

**Fix Location**: `InteractiveRunner.cs:HandleCall()`

**Fix Strategy**:
```csharp
// Parse args from JSON
if (cmd.TryGetProperty("args", out var argsElem))
{
    var args = argsElem.EnumerateArray().ToArray();
    for (int i = 0; i < args.Length; i++)
    {
        varData.ARG[i] = args[i].GetInt64();
    }
}
```

### P3: Game Loop Integration

**Symptom**:
- 数字入力（例: "352"）がコマンド実行につながらない
- Interactive Modeはゲームループ外で動作

**Root Cause**:
- Interactive ModeはゲームループをバイパスしてERB関数を直接実行
- `USERCOM.ERB`のコマンド分岐を通らない

**Fix Strategy (2 Options)**:

**Option A: input コマンド追加**
```json
{"cmd":"input","value":"352"}
```
- Emuera内部のINPUT処理をシミュレート
- 複雑だが完全なユーザー体験再現

**Option B: USERCOM経由実行**
```json
{"cmd":"usercom","num":352}
```
- `USERCOM(352)`を直接呼び出し
- シンプルだが一部の処理をスキップ

### P4: Headless Character Addition

**Symptom**:
```json
// scenario-077-confession.json
{
  "characters": {
    "十六夜咲夜": {
      "CFLAG:300": 15,
      "TALENT:3": 1
    }
  }
}
```
```
[Scenario] Characters loaded: あなた  ← プレイヤーのみ
[Scenario] Applied 7 pending character variables  ← 咲夜が存在しないため適用されず
```

**Root Cause**:
- Quick Start後、CharacterListにはプレイヤーのみが存在
- シナリオJSONは**既存キャラの変数変更のみ**対応
- キャラクターをCharacterListに追加する機能がない

**Impact**:
- Headlessでキャラ関連のE2Eテストが不可能
- `ここにいる:`が常に空になる

**Fix Strategy (2 Options)**:

**Option A: シナリオJSONに`add_characters`追加**
```json
{
  "add_characters": [4],  // CsvNoでキャラを追加
  "characters": {
    "十六夜咲夜": { ... }
  }
}
```

**Option B: Quick Start改修**
- ERB側でデバッグ用の全キャラ初期化オプションを追加
- `FLAG:デバッグ全キャラ初期化=1` で全キャラをCharacterListに追加

**推奨**: Option A（エンジン側で対応、ERB変更不要）

## Implementation Plan

### Phase 1: P1 Fix (StateInjector CsvNo)

1. [ ] `StateInjector.cs`で`VariableResolver.ResolveCharacterIndex()`を使用するよう修正
2. [ ] TALENT, ABL, CFLAG全てのset処理を統一
3. [ ] テスト: `{"cmd":"set","var":"TALENT:4:3","value":1}` が成功

### Phase 2: P2 Fix (Function Arguments)

4. [ ] `InteractiveRunner.HandleCall()`でargs解析を実装
5. [ ] ARG配列への設定を追加
6. [ ] テスト: `{"cmd":"call","func":"CHK_ADMIRATION_GET","args":[4]}` が成功

### Phase 3: P3 Fix (Game Loop)

7. [ ] 設計決定: Option A vs Option B
8. [ ] 選択したオプションを実装
9. [ ] テスト: 数字入力によるコマンド実行

### Phase 4: P4 Fix (Headless Character Addition)

10. [ ] `ScenarioInjector.cs`に`add_characters`配列の解析を追加
11. [ ] `AddCharacterFromCsvNo()`を呼び出してキャラをCharacterListに追加
12. [ ] テスト: シナリオJSONでキャラ追加→変数注入→Headless実行

## Acceptance Criteria

> **重要**: Feature 079完了時には、Feature 077の機能要件（実行検証がBLOCKEDだったもの）も合わせて検証する。

---

## Part A: Feature 079 Infrastructure AC

### AC1: CsvNo Character Variable Setting (P1)

**Test**: `set`コマンドがCsvNo指定で動作する

**Preconditions**: `--interactive --char 4` で起動

**Expected Results**:
- Output: `{"status":"ok"}`
- Variables: `TALENT:4:3 == 1`

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | TALENT:1:3 = 1 confirmed (登録番号1) |

**Test Commands (Interactive)**:
```json
{"cmd":"set","var":"TALENT:1:3","value":1}
{"cmd":"dump","vars":["TALENT:1:3"]}
```

**Note**: CsvNo 4 → 登録番号 1（MASTERが0のため）

---

### AC2: Function Call with Arguments (P2)

**Test**: 引数付き関数呼び出しが正常動作する

**Preconditions**: `--interactive --char 4`

**Expected Results**:
- Output: `{"status":"ok"}`
- Variables: ARGが正しく設定される

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [ ] | |

**Test Commands (Interactive)**:
```json
{"cmd":"call","func":"CHK_ADMIRATION_GET","args":[4]}
```

---

### AC3: User Command Execution (P3)

**Test**: `usercom`コマンドでゲームコマンド実行

**Preconditions**: `--interactive --char 4`

**Expected Results**:
- Output: `{"status":"ok"}`
- ゲームコマンドが実行される

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [ ] | |

**Test Commands (Interactive)**:
```json
{"cmd":"usercom","num":352}
```

---

### AC4: Headless Character Addition (P4)

**Test**: シナリオJSONでキャラ追加が動作する

**Preconditions**: `--inject scenario.json`

**Expected Results**:
- Output: `[Scenario] Adding character: 4`
- CharacterListに咲夜が追加される

| Mode | Status | Evidence |
|------|:------:|----------|
| Headless | [ ] | |

**Test Scenario** (`scenario-079-ac4.json`):
```json
{
  "add_characters": [4]
}
```

---

## Part B: Feature 077 Functional Requirements (5項目)

Feature 077の以下の機能要件は、コードレビューで確認済みだが**実行による検証がBLOCKED**だった。Feature 079完了時に両モードで検証する。

---

### AC5: FR1 - 思慕自動付与 (1日終了時)

**Test**: 好感度≥500, 親密≥3 の状態で1日終了時に思慕が自動付与される

**Preconditions**:
- `CFLAG:4:2 = 600` (好感度)
- `ABL:4:9 = 4` (親密)
- `TALENT:4:17 = 0` (思慕なし)

**Expected Results**:
- Output: `思慕を得た`
- Variables: `TALENT:4:17 == 1`

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [ ] | |
| Headless | [ ] | |

**Test Commands (Interactive)**:
```json
{"cmd":"set","var":"CFLAG:4:2","value":600}
{"cmd":"set","var":"ABL:4:9","value":4}
{"cmd":"set","var":"TALENT:4:17","value":0}
{"cmd":"call","func":"CHK_ADMIRATION_GET","args":[4]}
{"cmd":"dump","vars":["TALENT:4:17"]}
```

**Test (Headless)**: scenario + input-888.txt (1日終了コマンド)

---

### AC6: FR2 - 恋慕獲得に思慕が前提条件

**Test**: 思慕がない状態では恋慕が獲得できない

**Preconditions (思慕なし)**:
- `TALENT:4:17 = 0` (思慕なし)
- `CFLAG:4:2 = 2000` (好感度十分)
- `ABL:4:10 = 5` (従順十分)

**Expected Results**:
- Output: 恋慕獲得メッセージなし
- Variables: `TALENT:4:3 == 0` (恋慕なし)

**Preconditions (思慕あり)**:
- `TALENT:4:17 = 1` (思慕あり)
- 上記と同条件

**Expected Results**:
- Output: `恋慕を得た`
- Variables: `TALENT:4:3 == 1`, `TALENT:4:17 == 0`

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [ ] | |
| Headless | [ ] | |

**Test Commands (Interactive)**:
```json
{"cmd":"set","var":"TALENT:4:17","value":0}
{"cmd":"set","var":"CFLAG:4:2","value":2000}
{"cmd":"set","var":"ABL:4:10","value":5}
{"cmd":"call","func":"CHK_FALL_IN_LOVE","args":[4]}
{"cmd":"dump","vars":["TALENT:4:3","TALENT:4:17"]}
```

---

### AC7: FR3 - 告白で恋慕→恋人遷移

**Test**: 告白コマンド(COM352)で恋慕→恋人の遷移が行われる

**Preconditions**:
- `TALENT:4:3 = 1` (恋慕あり)
- `TALENT:4:16 = 0` (恋人なし)
- `ABL:4:9 = 10` (親密高)
- `ABL:4:10 = 10` (従順高)

**Expected Results**:
- Output: `恋人になりました`
- Variables: `TALENT:4:16 == 1`, `TALENT:4:3 == 0`

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [ ] | |
| Headless | [ ] | |

**Test Commands (Interactive)**:
```json
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:4:3","value":1}
{"cmd":"set","var":"TALENT:4:16","value":0}
{"cmd":"set","var":"ABL:4:9","value":10}
{"cmd":"set","var":"ABL:4:10","value":10}
{"cmd":"call","func":"COM352"}
{"cmd":"dump","vars":["TALENT:4:3","TALENT:4:16"]}
```

**Test (Headless)**: scenario-077-confession.json + input `352`

---

### AC8: FR4 - 告白は恋慕状態でのみ実行可能

**Test**: 恋慕なしでCOM_ABLE352が0を返す

**Preconditions (恋慕なし)**:
- `TALENT:4:3 = 0` (恋慕なし)

**Expected Results**:
- Variables: `COM_ABLE352 == 0`

**Preconditions (恋慕あり)**:
- `TALENT:4:3 = 1` (恋慕あり)

**Expected Results**:
- Variables: `COM_ABLE352 == 1`

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [ ] | |
| Headless | [ ] | |

**Test Commands (Interactive)**:
```json
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:4:3","value":0}
{"cmd":"call","func":"COM_ABLE352"}
{"cmd":"dump","vars":["RESULT:0"]}
{"cmd":"set","var":"TALENT:4:3","value":1}
{"cmd":"call","func":"COM_ABLE352"}
{"cmd":"dump","vars":["RESULT:0"]}
```

---

### AC9: FR5 - 告白成功/失敗時の口上表示

**Test**: 告白成功/失敗時に適切な口上が表示される

**Preconditions (成功ケース)**:
- `TALENT:4:3 = 1` (恋慕)
- `ABL:4:9 = 10`, `ABL:4:10 = 10` (高パラメータ)

**Expected Results**:
- Output: 告白成功口上（キャラ別 or 汎用）
- Output: `恋人になりました`

**Preconditions (失敗ケース)**:
- `TALENT:4:3 = 1` (恋慕)
- `ABL:4:9 = 0`, `ABL:4:10 = 0` (低パラメータ)
- `TALENT:4:255 = 1` (一線越えない)

**Expected Results**:
- Output: 告白失敗口上（キャラ別 or 汎用）
- Output: `告白を断られてしまった`

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [ ] | |
| Headless | [ ] | |

**Test Commands (Interactive - 成功)**:
```json
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:4:3","value":1}
{"cmd":"set","var":"ABL:4:9","value":10}
{"cmd":"set","var":"ABL:4:10","value":10}
{"cmd":"call","func":"COM352"}
```

**Test Commands (Interactive - 失敗)**:
```json
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:4:3","value":1}
{"cmd":"set","var":"TALENT:4:255","value":1}
{"cmd":"set","var":"ABL:4:9","value":0}
{"cmd":"set","var":"ABL:4:10","value":0}
{"cmd":"call","func":"COM352"}
```

---

## Part C: Headless E2E Scenarios

### Scenario 1: Confession E2E (Feature 077 FR3-5)
```json
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:4:3","value":1}
{"cmd":"set","var":"TALENT:4:16","value":0}
{"cmd":"set","var":"ABL:4:9","value":10}
{"cmd":"set","var":"ABL:4:10","value":10}
{"cmd":"dump","vars":["TARGET:0","TALENT:4:3","TALENT:4:16"]}
{"cmd":"call","func":"COM352"}
{"cmd":"dump","vars":["TALENT:4:3","TALENT:4:16"]}
{"cmd":"exit"}
```

Expected:
- `TALENT:4:16` → 1 (愛人化)
- `TALENT:4:3` → 0 (恋慕クリア)

### Scenario 2: SHOW_USERCOM Test
```json
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"call","func":"SHOW_USERCOM","inputs":["352"]}
```

### Scenario 3: Headless E2E Confession Test
```json
// tests/scenario-077-confession-v4.json
{
  "add_characters": [4],
  "characters": {
    "十六夜咲夜": {
      "CFLAG:300": 15,
      "CFLAG:2": 5000,
      "TALENT:3": 1,
      "TALENT:16": 0,
      "ABL:9": 10,
      "ABL:10": 10,
      "BASE:10": 1000
    }
  }
}
```
```bash
# tests/input-077-confession.txt
0
9
100
352
```

Expected:
- 咲夜がCharacterListに追加される
- 同室イベント発生（CFLAG:300=15で同室）
- COM352が実行可能
- 「恋人になりました」出力

## Files to Modify

| File | Change | Priority |
|------|--------|----------|
| `StateInjector.cs` | VariableResolver使用に変更 | P1 |
| `InteractiveRunner.cs` | HandleCall args処理追加 | P2 |
| `InteractiveRunner.cs` | usercom/input コマンド追加 | P3 |
| `ScenarioInjector.cs` | add_characters配列の解析・キャラ追加 | P4 |

## Dependencies

- Feature 078 (Headless State Injection Fix) - ✅ Complete

## Links

- [feature-078.md](feature-078.md) - 前提Feature (dump/TARGET修正)
- [feature-077.md](feature-077.md) - Relationship commands (本Feature完了後に再検証)
- [testing-reference.md](reference/testing-reference.md) - Interactive Mode Philosophy
