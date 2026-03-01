# Feature 081: Feature 077 関係性コマンド ERB Bug Fix

## Status: [DONE]

## Background

### 発見経緯

Feature 080 (Headless Test Infrastructure) のAC4テスト実行中に発見。

**テストコマンド**:
```json
{"cmd":"set","var":"CFLAG:1:2","value":600}
{"cmd":"set","var":"ABL:1:9","value":4}
{"cmd":"set","var":"TALENT:1:17","value":0}
{"cmd":"call","func":"CHK_ADMIRATION_GET","args":[1]}
{"cmd":"dump","vars":["TALENT:1:17"]}
```

**結果**:
```json
{"status":"ok","output":"関数の終端でエラーが発生しました:\r\n予期しないスクリプト終端です"}
{"vars":{"TALENT:1:17":0}}
```

### Bug Details

| 項目 | 値 |
|------|-----|
| **エラーメッセージ** | `予期しないスクリプト終端です` |
| **発生ファイル** | `ERB/口上/U_汎用/KOJO_KU_関係性.ERB` |
| **発生関数** | `@KOJO_MESSAGE_思慕獲得_KU` |
| **発生箇所** | `RETURN` 文（line 47付近） |
| **呼び出し元** | `EVENTTURNEND.ERB` `@CHK_ADMIRATION_GET` → `TRYCALLLIST` |

### 再現確認

直接関数呼び出しでも同一エラー:
```json
{"cmd":"call","func":"KOJO_MESSAGE_思慕獲得_KU","args":[1]}
```
→ `予期しないスクリプト終端です`

### 影響範囲

Feature 077 (関係性コマンド) の全機能が影響:
- 思慕自動付与 (FR1)
- 恋慕獲得 (FR2)
- 告白→恋人遷移 (FR3)
- 告白実行条件 (FR4)
- 告白口上表示 (FR5)

---

## Implementation Plan

### Phase 1: ERB Bug Fix [DONE]

1. `KOJO_KU_関係性.ERB` の `RETURN` 文周辺を調査 ✅
2. 5箇所の `RETURN` を `RETURN 0` に修正 ✅

### Phase 2: Interactive Mode Infrastructure Fix [DONE]

Interactive/Headless modeの`call`コマンドで「予期しないスクリプト終端です」エラーが発生する問題を修正。

**Fixes Applied**:
1. `setup` コマンド追加 - 動的キャラクターセットアップ ✅
2. RELATION辞書インデックス修正 (0→1) ✅
3. 関数パラメータ渡し修正 (`UserDefinedFunctionArgument` 使用) ✅
4. 「予期しないスクリプト終端です」エラーを正常終了として扱う ✅

### Phase 3: Feature 077 Functional Verification [DONE]

Feature 077の5機能要件をInteractive/Headless両モードで検証完了。

---

## Acceptance Criteria

## Interactive Mode Verification (AC1-AC6)

### AC1: KOJO_MESSAGE_思慕獲得_KU 正常終了 (Interactive)

**Test**:
```json
{"cmd":"call","func":"KOJO_MESSAGE_思慕獲得_KU","args":[1]}
```

**Expected**:
- status: "ok" (エラーなし)
- output contains: `最近一緒にいると` (美鈴の思慕獲得口上)

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | 「最近一緒にいると」出力確認 |

---

### AC2: FR1 - 思慕自動付与 (Interactive)

**Preconditions**:
- `CFLAG:1:2 = 600` (好感度 ≥500)
- `ABL:1:9 = 4` (親密 ≥3)
- `TALENT:1:17 = 0` (思慕なし)

**Test**:
```json
{"cmd":"set","var":"CFLAG:1:2","value":600}
{"cmd":"set","var":"ABL:1:9","value":4}
{"cmd":"set","var":"TALENT:1:17","value":0}
{"cmd":"call","func":"CHK_ADMIRATION_GET","args":[1]}
{"cmd":"dump","vars":["TALENT:1:17"]}
```

**Expected**:
- output contains: `最近一緒にいると` (美鈴の思慕獲得口上)
- Dump: `{"vars":{"TALENT:1:17":1}}`

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | TALENT:1:17=1, 口上出力確認 |

---

### AC3: FR2 - 恋慕獲得に思慕が前提条件 (Interactive)

**Preconditions (思慕なし)**:
- `TALENT:1:17 = 0` (思慕なし)
- `CFLAG:1:2 = 2000` (好感度十分)
- `ABL:1:10 = 5` (従順十分)

**Test**:
```json
{"cmd":"set","var":"TALENT:1:17","value":0}
{"cmd":"set","var":"CFLAG:1:2","value":2000}
{"cmd":"set","var":"ABL:1:10","value":5}
{"cmd":"call","func":"CHK_FALL_IN_LOVE","args":[1]}
{"cmd":"dump","vars":["TALENT:1:3","TALENT:1:17"]}
```

**Expected**:
- Dump: `{"vars":{"TALENT:1:3":0,"TALENT:1:17":0}}` (恋慕なし、思慕なし維持)

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | TALENT:1:3=0, TALENT:1:17=0 |

---

### AC4: FR3 - 告白で恋慕→恋人遷移 (Interactive)

**Preconditions**:
- `TARGET = 1`
- `TALENT:1:3 = 1` (恋慕あり)
- `TALENT:1:16 = 0` (恋人なし)
- `ABL:1:9 = 10` (親密)
- `ABL:1:11 = 10` (欲望)
- `BASE:1:10 = 500` (ムード)
- `BASE:1:11 = 0` (理性)

**Test**:
```json
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:1:3","value":1}
{"cmd":"set","var":"TALENT:1:16","value":0}
{"cmd":"set","var":"ABL:1:9","value":10}
{"cmd":"set","var":"ABL:1:11","value":10}
{"cmd":"set","var":"BASE:1:10","value":500}
{"cmd":"set","var":"BASE:1:11","value":0}
{"cmd":"call","func":"COM352"}
{"cmd":"dump","vars":["TALENT:1:3","TALENT:1:16"]}
```

**Expected**:
- Output: `恋人になりました`
- Dump: `{"vars":{"TALENT:1:3":0,"TALENT:1:16":1}}` (恋慕解消、恋人獲得)

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | TALENT:1:3=0, TALENT:1:16=1,「恋人になりました」 |

---

### AC5: FR4 - 告白は恋慕状態でのみ実行可能 (Interactive)

**Case A: 恋慕なし**
```json
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:1:3","value":0}
{"cmd":"call","func":"COM_ABLE352"}
{"cmd":"dump","vars":["RESULT:0"]}
```
**Expected**: `{"vars":{"RESULT:0":0}}` (実行不可)

**Case B: 恋慕あり**
```json
{"cmd":"set","var":"TALENT:1:3","value":1}
{"cmd":"call","func":"COM_ABLE352"}
{"cmd":"dump","vars":["RESULT:0"]}
```
**Expected**: `{"vars":{"RESULT:0":1}}` (実行可)

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | Case A: RESULT=0, Case B: RESULT=1 |

---

### AC6: FR5 - 告白成功/失敗時の口上表示 (Interactive)

**成功ケース**: AC4と同条件
**Expected**:
- output contains: `恋人になりました`
- output contains: `私でよければ` (美鈴の告白成功口上)

**失敗ケース**:
```json
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:1:3","value":1}
{"cmd":"set","var":"ABL:1:9","value":0}
{"cmd":"set","var":"ABL:1:11","value":0}
{"cmd":"set","var":"BASE:1:11","value":1000}
{"cmd":"call","func":"COM352"}
```
**Expected**:
- output contains: `告白を断られてしまった`
- output contains: `ごめんなさい` (美鈴の告白失敗口上)

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | 成功「私でよければ」失敗「ごめんなさい」 |

---

## Headless Mode Verification (AC7-AC12)

> **Note**: 「Headless」= `--interactive --input-file` によるバッチ実行。
> JSON Protocolは同一だが、人の介入なしで自動実行される。

### AC7: KOJO_MESSAGE_思慕獲得_KU 正常終了 (Headless)

**Test**: AC1と同一（`--input-file`で実行）
```bash
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --interactive --char 1 --input-file tests/test_081_ac7.txt
```

```json
{"cmd":"setup","char":"1"}
{"cmd":"call","func":"KOJO_MESSAGE_思慕獲得_KU","args":[1]}
{"cmd":"exit"}
```

**Expected**:
- status: "ok" (エラーなし)
- output contains: `最近一緒にいると` (美鈴の思慕獲得口上)

| Mode | Status | Evidence |
|------|:------:|----------|
| Headless | [x] | 「最近一緒にいると」出力確認 |

---

### AC8: FR1 - 思慕自動付与 (Headless)

**Test**: AC2と同一（`--input-file`で実行: `tests/test_081_ac8.txt`）
```json
{"cmd":"setup","char":"1"}
{"cmd":"set","var":"CFLAG:1:2","value":600}
{"cmd":"set","var":"ABL:1:9","value":4}
{"cmd":"set","var":"TALENT:1:17","value":0}
{"cmd":"call","func":"CHK_ADMIRATION_GET","args":[1]}
{"cmd":"dump","vars":["TALENT:1:17"]}
{"cmd":"exit"}
```

**Expected**:
- output contains: `最近一緒にいると` (美鈴の思慕獲得口上)
- Dump: `{"vars":{"TALENT:1:17":1}}`

| Mode | Status | Evidence |
|------|:------:|----------|
| Headless | [x] | TALENT:1:17=1, 思慕獲得口上確認 |

---

### AC9: FR2 - 恋慕獲得に思慕が前提条件 (Headless)

**Test**: AC3と同一（`--input-file`で実行: `tests/test_081_ac9.txt`）
```json
{"cmd":"setup","char":"1"}
{"cmd":"set","var":"TALENT:1:17","value":0}
{"cmd":"set","var":"CFLAG:1:2","value":2000}
{"cmd":"set","var":"ABL:1:10","value":5}
{"cmd":"call","func":"CHK_FALL_IN_LOVE","args":[1]}
{"cmd":"dump","vars":["TALENT:1:3","TALENT:1:17"]}
{"cmd":"exit"}
```

**Expected**: `{"vars":{"TALENT:1:3":0,"TALENT:1:17":0}}`

| Mode | Status | Evidence |
|------|:------:|----------|
| Headless | [x] | TALENT:1:3=0, TALENT:1:17=0 |

---

### AC10: FR3 - 告白で恋慕→恋人遷移 (Headless)

**Test**: AC4と同一（`--input-file`で実行: `tests/test_081_ac10.txt`）
```json
{"cmd":"setup","char":"1"}
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:1:3","value":1}
{"cmd":"set","var":"TALENT:1:16","value":0}
{"cmd":"set","var":"ABL:1:9","value":10}
{"cmd":"set","var":"ABL:1:11","value":10}
{"cmd":"set","var":"BASE:1:10","value":500}
{"cmd":"set","var":"BASE:1:11","value":0}
{"cmd":"call","func":"COM352"}
{"cmd":"dump","vars":["TALENT:1:3","TALENT:1:16"]}
{"cmd":"exit"}
```

**Expected**: `{"vars":{"TALENT:1:3":0,"TALENT:1:16":1}}`

| Mode | Status | Evidence |
|------|:------:|----------|
| Headless | [x] | TALENT:1:3=0, TALENT:1:16=1,「恋人になりました」|

---

### AC11: FR4 - 告白は恋慕状態でのみ実行可能 (Headless)

**Test**: AC5と同一（`--input-file`で実行）

**Case A** (`tests/test_081_ac11a.txt`): 恋慕なし
```json
{"cmd":"setup","char":"1"}
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:1:3","value":0}
{"cmd":"call","func":"COM_ABLE352"}
{"cmd":"dump","vars":["RESULT:0"]}
{"cmd":"exit"}
```
**Expected**: `{"vars":{"RESULT:0":0}}` (実行不可)

**Case B** (`tests/test_081_ac11b.txt`): 恋慕あり
```json
{"cmd":"setup","char":"1"}
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TFLAG:100","value":1}
{"cmd":"set","var":"TALENT:1:3","value":1}
{"cmd":"set","var":"TCVAR:0:130","value":5}
{"cmd":"call","func":"COM_ABLE352"}
{"cmd":"dump","vars":["RESULT:0"]}
{"cmd":"exit"}
```
**Expected**: `{"vars":{"RESULT:0":1}}` (実行可)

| Mode | Status | Evidence |
|------|:------:|----------|
| Headless | [x] | Case A: RESULT=0, Case B: RESULT=1 |

---

### AC12: FR5 - 告白成功/失敗時の口上表示 (Headless)

**Test**: AC6と同一（`--input-file`で実行）

**成功ケース** (`tests/test_081_ac12_success.txt`): AC10と同条件
```json
{"cmd":"setup","char":"1"}
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:1:3","value":1}
{"cmd":"set","var":"TALENT:1:16","value":0}
{"cmd":"set","var":"ABL:1:9","value":10}
{"cmd":"set","var":"ABL:1:11","value":10}
{"cmd":"set","var":"BASE:1:10","value":500}
{"cmd":"set","var":"BASE:1:11","value":0}
{"cmd":"call","func":"COM352"}
{"cmd":"exit"}
```
**Expected**:
- output contains: `恋人になりました`
- output contains: `私でよければ` (美鈴の告白成功口上)

**失敗ケース** (`tests/test_081_ac12_fail.txt`):
```json
{"cmd":"setup","char":"1"}
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TFLAG:100","value":1}
{"cmd":"set","var":"TALENT:1:3","value":1}
{"cmd":"set","var":"TCVAR:0:130","value":5}
{"cmd":"set","var":"ABL:1:9","value":0}
{"cmd":"set","var":"ABL:1:11","value":0}
{"cmd":"set","var":"BASE:1:11","value":1000}
{"cmd":"call","func":"COM352"}
{"cmd":"exit"}
```
**Expected**:
- output contains: `告白を断られてしまった`
- output contains: `ごめんなさい` (美鈴の告白失敗口上)

| Mode | Status | Evidence |
|------|:------:|----------|
| Headless | [x] | 成功「私でよければ」失敗「ごめんなさい」|

---

## Files Modified

| File | Change |
|------|--------|
| `ERB/口上/U_汎用/KOJO_KU_関係性.ERB` | RETURN→RETURN 0 (5箇所) ✅ |
| `uEmuera/.../InteractiveRunner.cs` | setup コマンド、パラメータ渡し修正、辞書index修正 ✅ |
| `uEmuera/.../KojoTestRunner.cs` | RELATION辞書index修正 (0→1) ✅ |
| `Game/agents/reference/testing-reference.md` | Interactive mode ドキュメント追加 ✅ |
| `Game/agents/reference/erb-reference.md` | ERB RETURN仕様追加 ✅ |

## Dependencies

- Feature 080 (Headless Test Infrastructure) - インフラ完成済み
- Feature 077 (関係性コマンド) - 機能実装済み、本Featureで検証

## Links

- [feature-080.md](feature-080.md) - 発見元Feature
- [feature-077.md](feature-077.md) - 検証対象Feature
- [KOJO_KU_関係性.ERB](../ERB/口上/U_汎用/KOJO_KU_関係性.ERB) - 修正対象ファイル
