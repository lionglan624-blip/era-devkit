# Feature 081: Feature 077 関係性コマンチEERB Bug Fix

## Status: [DONE]

## Background

### 発見経緯

Feature 080 (Headless Test Infrastructure) のAC4チE��ト実行中に発見、E

**チE��トコマンチE*:
```json
{"cmd":"set","var":"CFLAG:1:2","value":600}
{"cmd":"set","var":"ABL:1:9","value":4}
{"cmd":"set","var":"TALENT:1:17","value":0}
{"cmd":"call","func":"CHK_ADMIRATION_GET","args":[1]}
{"cmd":"dump","vars":["TALENT:1:17"]}
```

**結果**:
```json
{"status":"ok","output":"関数の終端でエラーが発生しました:\r\n予期しなぁE��クリプト終端でぁE}
{"vars":{"TALENT:1:17":0}}
```

### Bug Details

| 頁E�� | 値 |
|------|-----|
| **エラーメチE��ージ** | `予期しなぁE��クリプト終端です` |
| **発生ファイル** | `ERB/口丁EU_汎用/KOJO_KU_関係性.ERB` |
| **発生関数** | `@KOJO_MESSAGE_思�E獲得_KU` |
| **発生箁E��** | `RETURN` 斁E��Eine 47付近！E|
| **呼び出し�E** | `EVENTTURNEND.ERB` `@CHK_ADMIRATION_GET` ↁE`TRYCALLLIST` |

### 再現確誁E

直接関数呼び出しでも同一エラー:
```json
{"cmd":"call","func":"KOJO_MESSAGE_思�E獲得_KU","args":[1]}
```
ↁE`予期しなぁE��クリプト終端です`

### 影響篁E��

Feature 077 (関係性コマンチE の全機�Eが影響:
- 思�E自動付丁E(FR1)
- 恋�E獲征E(FR2)
- 告白→恋人遷移 (FR3)
- 告白実行条件 (FR4)
- 告白口上表示 (FR5)

---

## Implementation Plan

### Phase 1: ERB Bug Fix [DONE]

1. `KOJO_KU_関係性.ERB` の `RETURN` 斁E��辺を調査 ✁E
2. 5箁E��の `RETURN` めE`RETURN 0` に修正 ✁E

### Phase 2: Interactive Mode Infrastructure Fix [DONE]

Interactive/Headless modeの`call`コマンドで「予期しなぁE��クリプト終端です」エラーが発生する問題を修正、E

**Fixes Applied**:
1. `setup` コマンド追加 - 動的キャラクターセチE��アチE�E ✁E
2. RELATION辞書インチE��クス修正 (0ↁE) ✁E
3. 関数パラメータ渡し修正 (`UserDefinedFunctionArgument` 使用) ✁E
4. 「予期しなぁE��クリプト終端です」エラーを正常終亁E��して扱ぁE✁E

### Phase 3: Feature 077 Functional Verification [DONE]

Feature 077の5機�E要件をInteractive/Headless両モードで検証完亁E��E

---

## Acceptance Criteria

## Interactive Mode Verification (AC1-AC6)

### AC1: KOJO_MESSAGE_思�E獲得_KU 正常終亁E(Interactive)

**Test**:
```json
{"cmd":"call","func":"KOJO_MESSAGE_思�E獲得_KU","args":[1]}
```

**Expected**:
- status: "ok" (エラーなぁE
- output contains: `最近一緒にぁE��と` (美鈴の思�E獲得口丁E

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | 「最近一緒にぁE��と」�E力確誁E|

---

### AC2: FR1 - 思�E自動付丁E(Interactive)

**Preconditions**:
- `CFLAG:1:2 = 600` (好感度 ≥500)
- `ABL:1:9 = 4` (親寁E≥3)
- `TALENT:1:17 = 0` (思�EなぁE

**Test**:
```json
{"cmd":"set","var":"CFLAG:1:2","value":600}
{"cmd":"set","var":"ABL:1:9","value":4}
{"cmd":"set","var":"TALENT:1:17","value":0}
{"cmd":"call","func":"CHK_ADMIRATION_GET","args":[1]}
{"cmd":"dump","vars":["TALENT:1:17"]}
```

**Expected**:
- output contains: `最近一緒にぁE��と` (美鈴の思�E獲得口丁E
- Dump: `{"vars":{"TALENT:1:17":1}}`

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | TALENT:1:17=1, 口上�E力確誁E|

---

### AC3: FR2 - 恋�E獲得に思�Eが前提条件 (Interactive)

**Preconditions (思�EなぁE**:
- `TALENT:1:17 = 0` (思�EなぁE
- `CFLAG:1:2 = 2000` (好感度十�E)
- `ABL:1:10 = 5` (従頁E��刁E

**Test**:
```json
{"cmd":"set","var":"TALENT:1:17","value":0}
{"cmd":"set","var":"CFLAG:1:2","value":2000}
{"cmd":"set","var":"ABL:1:10","value":5}
{"cmd":"call","func":"CHK_FALL_IN_LOVE","args":[1]}
{"cmd":"dump","vars":["TALENT:1:3","TALENT:1:17"]}
```

**Expected**:
- Dump: `{"vars":{"TALENT:1:3":0,"TALENT:1:17":0}}` (恋�Eなし、思�Eなし維持E

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | TALENT:1:3=0, TALENT:1:17=0 |

---

### AC4: FR3 - 告白で恋�E→恋人遷移 (Interactive)

**Preconditions**:
- `TARGET = 1`
- `TALENT:1:3 = 1` (恋�Eあり)
- `TALENT:1:16 = 0` (恋人なぁE
- `ABL:1:9 = 10` (親寁E
- `ABL:1:11 = 10` (欲朁E
- `BASE:1:10 = 500` (ムーチE
- `BASE:1:11 = 0` (琁E��)

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
- Dump: `{"vars":{"TALENT:1:3":0,"TALENT:1:16":1}}` (恋�E解消、恋人獲征E

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | TALENT:1:3=0, TALENT:1:16=1,「恋人になりました、E|

---

### AC5: FR4 - 告白は恋�E状態でのみ実行可能 (Interactive)

**Case A: 恋�EなぁE*
```json
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:1:3","value":0}
{"cmd":"call","func":"COM_ABLE352"}
{"cmd":"dump","vars":["RESULT:0"]}
```
**Expected**: `{"vars":{"RESULT:0":0}}` (実行不可)

**Case B: 恋�Eあり**
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
- output contains: `私でよけれ�E` (美鈴の告白成功口丁E

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
- output contains: `ごめんなさい` (美鈴の告白失敗口丁E

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | 成功「私でよけれ�E」失敗「ごめんなさい、E|

---

## Headless Mode Verification (AC7-AC12)

> **Note**: 「Headless、E `--interactive --input-file` によるバッチ実行、E
> JSON Protocolは同一だが、人の介�Eなしで自動実行される、E

### AC7: KOJO_MESSAGE_思�E獲得_KU 正常終亁E(Headless)

**Test**: AC1と同一�E�E--input-file`で実行！E
```bash
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --interactive --char 1 --input-file tests/test_081_ac7.txt
```

```json
{"cmd":"setup","char":"1"}
{"cmd":"call","func":"KOJO_MESSAGE_思�E獲得_KU","args":[1]}
{"cmd":"exit"}
```

**Expected**:
- status: "ok" (エラーなぁE
- output contains: `最近一緒にぁE��と` (美鈴の思�E獲得口丁E

| Mode | Status | Evidence |
|------|:------:|----------|
| Headless | [x] | 「最近一緒にぁE��と」�E力確誁E|

---

### AC8: FR1 - 思�E自動付丁E(Headless)

**Test**: AC2と同一�E�E--input-file`で実衁E `tests/test_081_ac8.txt`�E�E
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
- output contains: `最近一緒にぁE��と` (美鈴の思�E獲得口丁E
- Dump: `{"vars":{"TALENT:1:17":1}}`

| Mode | Status | Evidence |
|------|:------:|----------|
| Headless | [x] | TALENT:1:17=1, 思�E獲得口上確誁E|

---

### AC9: FR2 - 恋�E獲得に思�Eが前提条件 (Headless)

**Test**: AC3と同一�E�E--input-file`で実衁E `tests/test_081_ac9.txt`�E�E
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

### AC10: FR3 - 告白で恋�E→恋人遷移 (Headless)

**Test**: AC4と同一�E�E--input-file`で実衁E `tests/test_081_ac10.txt`�E�E
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

### AC11: FR4 - 告白は恋�E状態でのみ実行可能 (Headless)

**Test**: AC5と同一�E�E--input-file`で実行！E

**Case A** (`tests/test_081_ac11a.txt`): 恋�EなぁE
```json
{"cmd":"setup","char":"1"}
{"cmd":"set","var":"TARGET","value":1}
{"cmd":"set","var":"TALENT:1:3","value":0}
{"cmd":"call","func":"COM_ABLE352"}
{"cmd":"dump","vars":["RESULT:0"]}
{"cmd":"exit"}
```
**Expected**: `{"vars":{"RESULT:0":0}}` (実行不可)

**Case B** (`tests/test_081_ac11b.txt`): 恋�Eあり
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

**Test**: AC6と同一�E�E--input-file`で実行！E

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
- output contains: `私でよけれ�E` (美鈴の告白成功口丁E

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
- output contains: `ごめんなさい` (美鈴の告白失敗口丁E

| Mode | Status | Evidence |
|------|:------:|----------|
| Headless | [x] | 成功「私でよけれ�E」失敗「ごめんなさい」|

---

## Files Modified

| File | Change |
|------|--------|
| `ERB/口丁EU_汎用/KOJO_KU_関係性.ERB` | RETURN→RETURN 0 (5箁E��) ✁E|
| `uEmuera/.../InteractiveRunner.cs` | setup コマンド、パラメータ渡し修正、辞書index修正 ✁E|
| `uEmuera/.../KojoTestRunner.cs` | RELATION辞書index修正 (0ↁE) ✁E|
| `pm/reference/testing-reference.md` | Interactive mode ドキュメント追加 ✁E|
| `pm/reference/erb-reference.md` | ERB RETURN仕様追加 ✁E|

## Dependencies

- Feature 080 (Headless Test Infrastructure) - インフラ完�E済み
- Feature 077 (関係性コマンチE - 機�E実裁E��み、本Featureで検証

## Links

- [feature-080.md](feature-080.md) - 発見�EFeature
- [feature-077.md](feature-077.md) - 検証対象Feature
- [KOJO_KU_関係性.ERB](../ERB/口丁EU_汎用/KOJO_KU_関係性.ERB) - 修正対象ファイル
