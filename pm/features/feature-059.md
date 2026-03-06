# Feature 059: Headless口上テスト - シナリオファイルと自動検証

## Status: [DONE]

### Known Limitations

- `var_equals` は RESULT 変数のみ対応（他変数は後続Feature 060/061で拡張予定）

## Background

Feature 058で口上関数の直接呼び出しが可能になったが、CLIオプションでの指定は複雑なテストには不向き。JSONシナリオファイルで状態・入力・期待値を定義し、自動検証できる仕組みが必要。

### 想定ユースケース

- **回帰テスト**: 口上追加時に既存口上が壊れていないことを自動検証
- **CI統合**: JSONシナリオをコミット時に自動実行
- **ドキュメント化**: シナリオファイル自体がテスト仕様書になる

## Goals

1. JSONシナリオファイルでテストを定義できる
2. 期待値（expect）で出力を自動検証できる
3. 出力形式を選択できる（human/quiet/json）

## Proposed Solution

### シナリオファイル形式

```json
{
  "name": "K4 会話口上テスト - 恋慕分岐",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十六夜咲夜",
  "state": {
    "CFLAG:2": 5000,
    "TALENT:3": 1
  },
  "inputs": [1],
  "expect": {
    "output_contains": "何について話そうか",
    "no_errors": true
  }
}
```

### CLI使用例

```bash
# シナリオファイルを実行
uEmuera --unit test.json

# 出力形式を指定
uEmuera --unit test.json --output compact   # デフォルト
uEmuera --unit test.json --output quiet     # 失敗のみ
uEmuera --unit test.json --output json      # JSON形式

# レポートファイルに出力
uEmuera --unit test.json --report result.json
```

### 出力例

**compactモード（デフォルト）**:
```
✓ K4 会話口上テスト - 恋慕分岐 (0.12s)

=== SUMMARY ===
1/1 passed (0.12s)
```

**失敗時**:
```
✗ K4 会話口上テスト - 恋慕分岐 (0.11s)
  [ERROR] Expected output to contain "何について話そうか" but got:
  --- ACTUAL OUTPUT ---
  エラー: TARGET未設定
  --- END ---

=== SUMMARY ===
0/1 passed, 1 failed (0.11s)
```

**jsonモード**:
```json
{
  "status": "pass",
  "name": "K4 会話口上テスト - 恋慕分岐",
  "function": "KOJO_MESSAGE_COM_K4_300",
  "duration_ms": 120,
  "output": "何について話そうか……？\n[ 1]-他愛も無い雑談\n...",
  "errors": [],
  "warnings": []
}
```

## Scope

### In Scope

| 機能 | 説明 |
|------|------|
| `--unit <file.json>` | シナリオファイル読み込み |
| `expect.output_contains` | 出力に含まれることを検証 |
| `expect.output_not_contains` | 出力に含まれないことを検証 |
| `expect.output_matches` | 正規表現マッチ |
| `expect.no_errors` | エラーなしを検証 |
| `expect.no_warnings` | 警告なしを検証 |
| `expect.var_equals` | 変数値を検証 |
| `--output compact\|quiet\|json` | 出力形式 |
| `--report <file>` | レポートファイル出力 |

### Out of Scope (後続Feature)

- バッチテスト（複数シナリオ一括） → Feature 060
- 分岐トレース → Feature 060
- 乱数制御 → Feature 060
- 複数キャラ設定 → Feature 061

## Scenario File Specification

### 基本構造

```json
{
  "name": "テスト名（必須）",
  "call": "関数名（必須）",
  "character": "キャラクター名（必須）",
  "master": "MASTERキャラ名（省略時PLAYER）",
  "state": {
    "変数名": 値
  },
  "inputs": [入力値リスト],
  "expect": {
    "検証条件": 値
  }
}
```

### state での変数指定

```json
{
  "state": {
    "CFLAG:2": 5000,
    "CFLAG:TARGET:好感度": 5000,
    "TALENT:3": 1,
    "TALENT:TARGET:恋慕": 1,
    "FLAG:200": 1,
    "TFLAG:193": 1
  }
}
```

- 数値インデックス: `CFLAG:2`
- シンボル名: `CFLAG:TARGET:好感度`
- `TARGET` は `character` で指定したキャラに展開される

### expect 仕様

| チェック種別 | 説明 | 例 |
|-------------|------|---|
| `output_contains` | 出力に含まれる（文字列or配列） | `"output_contains": "何について"` |
| `output_not_contains` | 出力に含まれない | `"output_not_contains": "エラー"` |
| `output_matches` | 正規表現マッチ | `"output_matches": "\\[ \\d+\\]-"` |
| `output_equals` | 完全一致 | `"output_equals": "固定テキスト"` |
| `no_errors` | エラーなし | `"no_errors": true` |
| `no_warnings` | 警告なし | `"no_warnings": true` |
| `var_equals` | 変数値チェック | `"var_equals": {"RESULT": 1}` |

```json
{
  "expect": {
    "output_contains": ["何について", "雑談"],
    "output_not_contains": ["エラー", "未定義"],
    "no_errors": true,
    "var_equals": {
      "RESULT": 1
    }
  }
}
```

## Output Formats

### compact（デフォルト）

成功時は1行、失敗時は詳細表示:

```
✓ テスト1 (0.12s)
✓ テスト2 (0.15s)
✗ テスト3 (0.11s)
  [ERROR] Expected output to contain "XXX"
  --- ACTUAL OUTPUT ---
  ...
  --- END ---

=== SUMMARY ===
2/3 passed, 1 failed (0.38s)
```

### quiet

失敗テストの詳細のみ:

```
✗ テスト3 (0.11s)
  [ERROR] Expected output to contain "XXX"
  ...

=== SUMMARY ===
2/3 passed, 1 failed (0.38s)
```

### json

```json
{
  "summary": {
    "total": 3,
    "passed": 2,
    "failed": 1,
    "duration_ms": 380
  },
  "results": [
    {"name": "テスト1", "status": "pass", "duration_ms": 120},
    {"name": "テスト2", "status": "pass", "duration_ms": 150},
    {
      "name": "テスト3",
      "status": "fail",
      "duration_ms": 110,
      "error": "Expected output to contain \"XXX\"",
      "output": "..."
    }
  ]
}
```

## Acceptance Criteria

- [x] `--unit <file.json>` でシナリオファイルを読み込める
- [x] `expect.output_contains` で出力を検証できる
- [x] `expect.output_not_contains` で出力を検証できる
- [x] `expect.output_matches` で正規表現検証できる
- [x] `expect.no_errors` でエラーなしを検証できる
- [ ] `expect.var_equals` で変数値を検証できる (RESULT変数のみ対応、改善必要)
- [x] `--output-mode compact` で成功1行/失敗詳細が表示される
- [x] `--output-mode quiet` で失敗のみ表示される
- [x] `--output-mode json` でJSON形式出力される
- [x] `--report <file>` でレポートファイルが出力される
- [x] 検証成功時exit 0、失敗時exit 1が返る

### Concrete Test Scenarios

実装検証用のシナリオファイル（`Game/tests/kojo/scenarios/` に配置）:

| # | ファイル名 | 検証対象 | 期待結果 |
|---|-----------|----------|----------|
| S1 | `k4-basic.json` | 基本実行 + output_contains | PASS |
| S2 | `k4-love.json` | state設定 + 恋慕分岐 | PASS |
| S3 | `k4-no-error.json` | output_not_contains + no_errors | PASS |
| S4 | `k4-regex.json` | output_matches（選択肢パターン） | PASS |
| S5 | `k4-combined.json` | 複合expect（contains + not_contains + no_errors） | PASS |
| S6 | `k4-fail-expected.json` | 意図的失敗（exit 1確認用） | FAIL |
| S7 | `k4-var-check.json` | var_equals（RESULT検証） | PASS |

#### S1: k4-basic.json
```json
{
  "name": "K4基本実行",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十六夜咲夜",
  "expect": {
    "output_contains": "何について話そうか"
  }
}
```

#### S2: k4-love.json
```json
{
  "name": "K4恋慕分岐",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十六夜咲夜",
  "state": {
    "CFLAG:TARGET:好感度": 5000,
    "TALENT:TARGET:恋慕": 1
  },
  "expect": {
    "output_contains": "何について話そうか"
  }
}
```

#### S3: k4-no-error.json
```json
{
  "name": "K4エラーなし検証",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十六夜咲夜",
  "expect": {
    "output_not_contains": ["エラー", "未定義", "Error"],
    "no_errors": true
  }
}
```

#### S4: k4-regex.json
```json
{
  "name": "K4選択肢パターン検証",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十六夜咲夜",
  "expect": {
    "output_matches": "\\[\\s*\\d+\\].*雑談"
  }
}
```

#### S5: k4-combined.json
```json
{
  "name": "K4複合検証",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十六夜咲夜",
  "state": {
    "CFLAG:TARGET:好感度": 3000
  },
  "expect": {
    "output_contains": ["何について", "雑談"],
    "output_not_contains": "エラー",
    "no_errors": true
  }
}
```

#### S6: k4-fail-expected.json
```json
{
  "name": "K4意図的失敗（存在しない文字列）",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十六夜咲夜",
  "expect": {
    "output_contains": "この文字列は存在しないはず_XYZ123"
  }
}
```

#### S7: k4-var-check.json
```json
{
  "name": "K4変数検証",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十六夜咲夜",
  "inputs": [1],
  "expect": {
    "no_errors": true,
    "var_equals": {
      "RESULT": 1
    }
  }
}
```

### AC Verification Matrix

| AC項目 | 検証シナリオ |
|--------|-------------|
| シナリオファイル読み込み | S1〜S7全て |
| output_contains | S1, S2, S5, S6 |
| output_not_contains | S3, S5 |
| output_matches | S4 |
| no_errors | S3, S5, S7 |
| var_equals | S7 |
| exit 0（成功） | S1〜S5, S7 |
| exit 1（失敗） | S6 |
| --output compact | 手動確認 |
| --output quiet | 手動確認 |
| --output json | 手動確認 |
| --report | 手動確認 |

## Test Plan

```bash
# 基本シナリオテスト
echo '{"name":"test","call":"KOJO_MESSAGE_COM_K4_300","character":"十六夜咲夜","expect":{"output_contains":"何について"}}' > test.json
uEmuera --unit test.json
# → ✓ test (0.xxs)

# expect失敗テスト
echo '{"name":"test","call":"KOJO_MESSAGE_COM_K4_300","character":"十六夜咲夜","expect":{"output_contains":"存在しない文字列"}}' > test.json
uEmuera --unit test.json
# → ✗ test (0.xxs) + エラー詳細

# JSON出力テスト
uEmuera --unit test.json --output json
# → {"status":"pass",...}

# レポートファイルテスト
uEmuera --unit test.json --report result.json
# → result.jsonが生成される
```

## Effort Estimate

- **Size**: Medium
- **Risk**: Low（Feature 058の上に構築）
- **Dependencies**: Feature 058（口上関数の直接実行）

## Links

- [feature-058.md](feature-058.md) - 口上関数の直接実行（前提）
- [feature-060.md](feature-060.md) - バッチテストと分岐トレース（後続）
- [reference/testing-reference.md](reference/testing-reference.md) - テスト戦略
