# Feature 080: Headless Test Infrastructure完成

## Status: [BLOCKED]

**Blocking**: Feature 081 (Feature 077 ERB Bug Fix) 完了待ち

**詳細**: AC4テスト中に `KOJO_KU_関係性.ERB` の `RETURN` 文でランタイムエラー「予期しないスクリプト終端です」を発見。Feature 081で修正後にAC4-AC8を検証予定。

## Background

Feature 079で発見された問題を解決し、Feature 077の機能要件をHeadless/Interactive両モードで完全検証可能にする。

### Feature 079で発見された問題

| 問題 | 本Feature対象 | 状態 |
|------|:-------------:|------|
| **P1: PALAM未対応** | ✅ | StateInjectorに実装追加 |
| **P2: MARK未対応** | ✅ | StateInjectorに実装追加 |
| **P3: WAIT文ブロック** | ✅ | Headless時のWAIT自動スキップ |
| **P4: CsvNo混同** | - | ドキュメント化で解決済み |

## Implementation Plan

### Phase 1: PALAM/MARK Support

VariableResolver/StateInjectorにPALAM/MARK変数の解決・設定・読取を追加。

**現状分析** (実装済み部分):
- `ResolvePalamIndex()` ✅
- `ResolveMarkIndex()` ✅
- `SetPalam()` ✅
- `SetMark()` ✅

**未実装**:
- `InjectVariable()` にPALAM/MARKケース追加
- `ReadVariable()` にPALAM/MARK/BASE/TALENT/ABLケース追加

### Phase 2: WAIT文自動スキップ

Headlessモード時にWAIT文をブロックせず自動スキップする。

**実装方針**:
- WAIT命令実行時にHeadlessモードを検出
- Headlessモードの場合、入力待ちをスキップして即座に継続

---

## Acceptance Criteria

> **Goal**: Feature 077の5機能要件をInteractive/Headless両モードで完全検証可能にする

---

### AC1: PALAM Variable Setting (Infrastructure)

**Preconditions**: `--interactive --char 4` (登録番号1になる)

**Test Commands**:
```json
{"cmd":"set","var":"PALAM:1:8","value":100}
{"cmd":"dump","vars":["PALAM:1:8"]}
```

**Expected Results**:
- Status: `{"status":"ok"}`
- Dump: `{"vars":{"PALAM:1:8":100}}`

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | `{"vars":{"PALAM:1:8":100}}` |

---

### AC2: MARK Variable Setting (Infrastructure)

**Preconditions**: `--interactive --char 4`

**Test Commands**:
```json
{"cmd":"set","var":"MARK:1:0","value":10}
{"cmd":"dump","vars":["MARK:1:0"]}
```

**Expected Results**:
- Status: `{"status":"ok"}`
- Dump: `{"vars":{"MARK:1:0":10}}`

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [x] | `{"vars":{"MARK:1:0":10}}` |

---

### AC3: WAIT Auto-Skip (Infrastructure)

**Test**: Headlessモードで WAIT文を含む関数を呼び出し、ブロックせず完了すること

| Mode | Status | Evidence |
|------|:------:|----------|
| Headless | [x] | InteractiveRunner line 486-490, KojoTestRunner line 1229-1230 |

**Note**: WAIT auto-skip is already implemented in both runners. Verified by code review.

---

### AC4-AC8: Feature 077 Validation

**Status**: [B] Blocked by Feature 081

AC4-AC8はFeature 077の機能要件を検証するもの。Feature 081でERBバグ修正後に検証を実施。

---

### AC4: FR1 - 思慕自動付与 (Feature 077)

**Preconditions**:
- `CFLAG:1:2 = 600` (好感度 ≥500)
- `ABL:1:9 = 4` (親密 ≥3)
- `TALENT:1:17 = 0` (思慕なし)

**Test Commands**:
```json
{"cmd":"set","var":"CFLAG:1:2","value":600}
{"cmd":"set","var":"ABL:1:9","value":4}
{"cmd":"set","var":"TALENT:1:17","value":0}
{"cmd":"call","func":"CHK_ADMIRATION_GET","args":[1]}
{"cmd":"dump","vars":["TALENT:1:17"]}
```

**Expected Results**:
- Output: `思慕を得た` または該当口上
- Dump: `{"vars":{"TALENT:1:17":1}}`

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [B] | → Feature 081 |
| Headless | [B] | → Feature 081 |

---

### AC5: FR2 - 恋慕獲得に思慕が前提条件 (Feature 077)

**Preconditions (思慕なし)**:
- `TALENT:1:17 = 0` (思慕なし)
- `CFLAG:1:2 = 2000` (好感度十分)
- `ABL:1:10 = 5` (従順十分)

**Test Commands**:
```json
{"cmd":"set","var":"TALENT:1:17","value":0}
{"cmd":"set","var":"CFLAG:1:2","value":2000}
{"cmd":"set","var":"ABL:1:10","value":5}
{"cmd":"call","func":"CHK_FALL_IN_LOVE","args":[1]}
{"cmd":"dump","vars":["TALENT:1:3","TALENT:1:17"]}
```

**Expected Results**:
- Dump: `{"vars":{"TALENT:1:3":0,"TALENT:1:17":0}}` (恋慕なし、思慕なし維持)

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [B] | → Feature 081 |
| Headless | [B] | → Feature 081 |

---

### AC6: FR3 - 告白で恋慕→恋人遷移 (Feature 077)

**Preconditions**:
- `TARGET = 1`
- `TALENT:1:3 = 1` (恋慕あり)
- `TALENT:1:16 = 0` (恋人なし)
- `ABL:1:9 = 10` (親密)
- `ABL:1:11 = 10` (欲望)
- `BASE:1:10 = 500` (ムード)
- `BASE:1:11 = 0` (理性)

**Test Commands**:
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

**Expected Results**:
- Output: `恋人になりました`
- Dump: `{"vars":{"TALENT:1:3":0,"TALENT:1:16":1}}` (恋慕解消、恋人獲得)

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [B] | → Feature 081 |
| Headless | [B] | → Feature 081 |

---

### AC7: FR4 - 告白は恋慕状態でのみ実行可能 (Feature 077)

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
| Interactive | [B] | → Feature 081 |
| Headless | [B] | → Feature 081 |

---

### AC8: FR5 - 告白成功/失敗時の口上表示 (Feature 077)

**成功ケース**: AC6と同条件
**Expected Output**: 告白成功口上（例: 「あなた様…私の全ては、あなたのものです」）

**失敗ケース**:
- `TALENT:1:3 = 1`, `ABL:1:9 = 0`, `ABL:1:11 = 0`, `BASE:1:11 = 1000`

**Expected Output**: 告白失敗口上（例: 「申し訳ありません…今はまだ、お受けすることができません」）

| Mode | Status | Evidence |
|------|:------:|----------|
| Interactive | [B] | → Feature 081 |
| Headless | [B] | → Feature 081 |

---

## Files to Modify

| File | Change |
|------|--------|
| `StateInjector.cs` | Add PALAM/MARK cases to InjectVariable(), ReadVariable() |
| `FunctionMethodManager.cs` or WAIT handler | Add headless auto-skip |

## Dependencies

- Feature 079 (Interactive Mode Complete) - ✅ Complete
- Feature 077 (関係性コマンド) - [WIP] 機能実装済み、検証待ち

## Links

- [feature-079.md](feature-079.md) - 発見元Feature
- [feature-077.md](feature-077.md) - 検証対象Feature
- [testing-reference.md](reference/testing-reference.md) - Variable Reference
