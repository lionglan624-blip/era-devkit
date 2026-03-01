# Flow Testing Reference

flow test 専用リファレンス。共通情報は [SKILL.md](SKILL.md) 参照。

---

## Overview

Flow tests = 状態注入 + メニュー操作による回帰テスト。

**Purpose**: ゲーム状態を注入し、メニュー操作でゲームループを実行、exit code 0 で PASS。

---

## Execution Path

```
--flow → HeadlessRunner → ProcessLevelParallelRunner.RunFlowTests()
  → FlowTestScenario.FromScenarioFile()
    → scenario-{name}.json + input-{name}.txt をペアリング
  → RunFlowTestInProcess()
    → 別プロセスで --inject + --input-file を実行
```

**重要**: `--flow` は KojoTestRunner ではなく ProcessLevelParallelRunner を使用。

---

## File Pairing

| ファイル | 役割 |
|----------|------|
| `scenario-{name}.json` | 状態注入（キャラクター状態、変数設定） |
| `input-{name}.txt` | メニュー操作（数値入力、1行1コマンド） |

**ペアリングロジック** (FlowTestScenario.FromScenarioFile):
- `scenario-xxx.json` → `input-xxx.txt` を自動検索
- input ファイルがなければ状態注入のみで終了

---

## Scenario Structure

```json
{
  "name": "Test identifier",
  "description": "Human-readable description",
  "characters": {
    "CharacterName": {
      "CFLAG:index": value,
      "TALENT:index": value
    }
  },
  "add_characters": [csv_numbers],
  "copy": [
    {"from": "CharacterName", "to": "MASTER|TARGET", "var": "CFLAG:index"}
  ]
}
```

---

## Input File Format

**形式**: 1行1コマンド（メニュー選択番号）

```
0       ← 最初からはじめる
9       ← メニュー選択
100     ← 終了コマンド
```

**注意**: JSON Protocol (assert_equals 等) ではない。

---

## Pass/Fail Criteria

| Condition | Result |
|-----------|:------:|
| ゲームループが正常終了 (exit 0) | PASS |
| 状態注入で例外 | FAIL |
| JSON パースエラー | FAIL |
| 無効なキャラクター参照 | FAIL |
| タイムアウト | FAIL |

---

## Execution

```bash
# All scenarios (recommended, auto-parallel)
cd Game
dotnet run --project ../engine/uEmuera.Headless.csproj -- . \
  --flow "tests/regression/scenario-*.json"
```

**Note**: F213 で AutoSave ファイル競合問題を修正済み。`--parallel` 未指定で動的にワーカー数を計算（メモリ/CPU から最適値）。

### 重要な制限事項

**単一ファイル指定時は input ファイルが読み込まれない**:

| 指定方法 | input ペアリング | 動作 |
|----------|:---------------:|------|
| `--flow "scenario-*.json" --parallel N` | ✅ | 正常 |
| `--flow scenario-wakeup.json --parallel 1` | ✅ | 正常 |
| `--flow scenario-wakeup.json` (parallel なし) | ❌ | **input 無視** |

**理由**: HeadlessRunner.cs の条件分岐で、単一ファイル + parallel なしの場合は通常の inject モードになる。

**推奨**: 常に glob パターンまたは `--parallel` オプションを使用する。

```bash
# NG: input ファイルが読み込まれない
--flow tests/regression/scenario-wakeup.json

# OK: parallel オプション付き
--flow tests/regression/scenario-wakeup.json --parallel 1

# OK: glob パターン
--flow "tests/regression/scenario-*.json"
```

**Note**: ディレクトリ指定 (`--flow tests/regression/`) は動作しない。glob パターンを使用。

---

## Test Directory

```
tests/regression/
├── scenario-wakeup.json          # 状態注入
├── input-wakeup.txt              # メニュー操作
├── scenario-sc-001-*.json        # P0 シナリオ
├── input-sc-001-*.txt
└── ... (24 シナリオ)
```

---

## Regression Testing Rules

regression-tester MUST:
- Run all 24 scenarios using `dotnet run -- . --flow "tests/regression/scenario-*.json"` (auto-parallel)
- Verify all tests pass (24/24)
- Check expect field validation results
- Report any failures with "[Expect] FAIL" details

---

## Expect Field Verification

Each scenario JSON can include an `expect` field for validation:

```json
{
  "expect": {
    "output_contains": ["text that must appear"],
    "output_not_contains": ["text that must NOT appear"],
    "var_equals": {
      "TALENT:1:18": 0
    }
  }
}
```

Verification results:
- PASS: `[Expect] PASS: expectation met`
- FAIL: `[Expect] FAIL:` with expected/actual details

---

## Implementation

- [ProcessLevelParallelRunner.cs](../../../engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs)
- [HeadlessRunner.cs](../../../engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs)
