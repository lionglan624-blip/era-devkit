# Feature 059: Headless口上テスチE- シナリオファイルと自動検証

## Status: [DONE]

### Known Limitations

- `var_equals` は RESULT 変数のみ対応（他変数は後続Feature 060/061で拡張予定！E

## Background

Feature 058で口上関数の直接呼び出しが可能になったが、CLIオプションでの持E���E褁E��なチE��トには不向き、ESONシナリオファイルで状態�E入力�E期征E��を定義し、�E動検証できる仕絁E��が忁E��、E

### 想定ユースケース

- **回帰チE��チE*: 口上追加時に既存口上が壊れてぁE��ぁE��とを�E動検証
- **CI統吁E*: JSONシナリオをコミット時に自動実衁E
- **ドキュメント化**: シナリオファイル自体がチE��ト仕様書になめE

## Goals

1. JSONシナリオファイルでチE��トを定義できる
2. 期征E���E�Expect�E�で出力を自動検証できる
3. 出力形式を選択できる�E�Euman/quiet/json�E�E

## Proposed Solution

### シナリオファイル形弁E

```json
{
  "name": "K4 会話口上テスチE- 恋�E刁E��E,
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十�E夜咲夁E,
  "state": {
    "CFLAG:2": 5000,
    "TALENT:3": 1
  },
  "inputs": [1],
  "expect": {
    "output_contains": "何につぁE��話そうぁE,
    "no_errors": true
  }
}
```

### CLI使用侁E

```bash
# シナリオファイルを実衁E
uEmuera --unit test.json

# 出力形式を持E��E
uEmuera --unit test.json --output compact   # チE��ォルチE
uEmuera --unit test.json --output quiet     # 失敗�Eみ
uEmuera --unit test.json --output json      # JSON形弁E

# レポ�Eトファイルに出劁E
uEmuera --unit test.json --report result.json
```

### 出力侁E

**compactモード（デフォルト！E*:
```
✁EK4 会話口上テスチE- 恋�E刁E��E(0.12s)

=== SUMMARY ===
1/1 passed (0.12s)
```

**失敗時**:
```
✁EK4 会話口上テスチE- 恋�E刁E��E(0.11s)
  [ERROR] Expected output to contain "何につぁE��話そうぁE but got:
  --- ACTUAL OUTPUT ---
  エラー: TARGET未設宁E
  --- END ---

=== SUMMARY ===
0/1 passed, 1 failed (0.11s)
```

**jsonモーチE*:
```json
{
  "status": "pass",
  "name": "K4 会話口上テスチE- 恋�E刁E��E,
  "function": "KOJO_MESSAGE_COM_K4_300",
  "duration_ms": 120,
  "output": "何につぁE��話そうか……�E�\n[ 1]-他�Eも無ぁE��談\n...",
  "errors": [],
  "warnings": []
}
```

## Scope

### In Scope

| 機�E | 説昁E|
|------|------|
| `--unit <file.json>` | シナリオファイル読み込み |
| `expect.output_contains` | 出力に含まれることを検証 |
| `expect.output_not_contains` | 出力に含まれなぁE��とを検証 |
| `expect.output_matches` | 正規表現マッチE|
| `expect.no_errors` | エラーなしを検証 |
| `expect.no_warnings` | 警告なしを検証 |
| `expect.var_equals` | 変数値を検証 |
| `--output compact\|quiet\|json` | 出力形弁E|
| `--report <file>` | レポ�Eトファイル出劁E|

### Out of Scope (後続Feature)

- バッチテスト（褁E��シナリオ一括�E�EↁEFeature 060
- 刁E��トレース ↁEFeature 060
- 乱数制御 ↁEFeature 060
- 褁E��キャラ設宁EↁEFeature 061

## Scenario File Specification

### 基本構造

```json
{
  "name": "チE��ト名�E�忁E��！E,
  "call": "関数名（忁E��！E,
  "character": "キャラクター名（忁E��！E,
  "master": "MASTERキャラ名（省略時PLAYER�E�E,
  "state": {
    "変数吁E: 値
  },
  "inputs": [入力値リスチE,
  "expect": {
    "検証条件": 値
  }
}
```

### state での変数持E��E

```json
{
  "state": {
    "CFLAG:2": 5000,
    "CFLAG:TARGET:好感度": 5000,
    "TALENT:3": 1,
    "TALENT:TARGET:恋�E": 1,
    "FLAG:200": 1,
    "TFLAG:193": 1
  }
}
```

- 数値インチE��クス: `CFLAG:2`
- シンボル吁E `CFLAG:TARGET:好感度`
- `TARGET` は `character` で持E��したキャラに展開されめE

### expect 仕槁E

| チェチE��種別 | 説昁E| 侁E|
|-------------|------|---|
| `output_contains` | 出力に含まれる�E�文字�Eor配�E�E�E| `"output_contains": "何につぁE��"` |
| `output_not_contains` | 出力に含まれなぁE| `"output_not_contains": "エラー"` |
| `output_matches` | 正規表現マッチE| `"output_matches": "\\[ \\d+\\]-"` |
| `output_equals` | 完�E一致 | `"output_equals": "固定テキスチE` |
| `no_errors` | エラーなぁE| `"no_errors": true` |
| `no_warnings` | 警告なぁE| `"no_warnings": true` |
| `var_equals` | 変数値チェチE�� | `"var_equals": {"RESULT": 1}` |

```json
{
  "expect": {
    "output_contains": ["何につぁE��", "雑諁E],
    "output_not_contains": ["エラー", "未定義"],
    "no_errors": true,
    "var_equals": {
      "RESULT": 1
    }
  }
}
```

## Output Formats

### compact�E�デフォルト！E

成功時�E1行、失敗時は詳細表示:

```
✁EチE��チE (0.12s)
✁EチE��チE (0.15s)
✁EチE��チE (0.11s)
  [ERROR] Expected output to contain "XXX"
  --- ACTUAL OUTPUT ---
  ...
  --- END ---

=== SUMMARY ===
2/3 passed, 1 failed (0.38s)
```

### quiet

失敗テスト�E詳細のみ:

```
✁EチE��チE (0.11s)
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
    {"name": "チE��チE", "status": "pass", "duration_ms": 120},
    {"name": "チE��チE", "status": "pass", "duration_ms": 150},
    {
      "name": "チE��チE",
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
- [ ] `expect.var_equals` で変数値を検証できる (RESULT変数のみ対応、改喁E��E��E
- [x] `--output-mode compact` で成功1衁E失敗詳細が表示されめE
- [x] `--output-mode quiet` で失敗�Eみ表示されめE
- [x] `--output-mode json` でJSON形式�E力される
- [x] `--report <file>` でレポ�Eトファイルが�E力される
- [x] 検証成功時exit 0、失敗時exit 1が返る

### Concrete Test Scenarios

実裁E��証用のシナリオファイル�E�Etest/kojo/scenarios/` に配置�E�E

| # | ファイル吁E| 検証対象 | 期征E��果 |
|---|-----------|----------|----------|
| S1 | `k4-basic.json` | 基本実衁E+ output_contains | PASS |
| S2 | `k4-love.json` | state設宁E+ 恋�E刁E��E| PASS |
| S3 | `k4-no-error.json` | output_not_contains + no_errors | PASS |
| S4 | `k4-regex.json` | output_matches�E�選択肢パターン�E�E| PASS |
| S5 | `k4-combined.json` | 褁E��expect�E�Eontains + not_contains + no_errors�E�E| PASS |
| S6 | `k4-fail-expected.json` | 意図皁E��敗！Exit 1確認用�E�E| FAIL |
| S7 | `k4-var-check.json` | var_equals�E�EESULT検証�E�E| PASS |

#### S1: k4-basic.json
```json
{
  "name": "K4基本実衁E,
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十�E夜咲夁E,
  "expect": {
    "output_contains": "何につぁE��話そうぁE
  }
}
```

#### S2: k4-love.json
```json
{
  "name": "K4恋�E刁E��E,
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十�E夜咲夁E,
  "state": {
    "CFLAG:TARGET:好感度": 5000,
    "TALENT:TARGET:恋�E": 1
  },
  "expect": {
    "output_contains": "何につぁE��話そうぁE
  }
}
```

#### S3: k4-no-error.json
```json
{
  "name": "K4エラーなし検証",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十�E夜咲夁E,
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
  "character": "十�E夜咲夁E,
  "expect": {
    "output_matches": "\\[\\s*\\d+\\].*雑諁E
  }
}
```

#### S5: k4-combined.json
```json
{
  "name": "K4褁E��検証",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十�E夜咲夁E,
  "state": {
    "CFLAG:TARGET:好感度": 3000
  },
  "expect": {
    "output_contains": ["何につぁE��", "雑諁E],
    "output_not_contains": "エラー",
    "no_errors": true
  }
}
```

#### S6: k4-fail-expected.json
```json
{
  "name": "K4意図皁E��敗（存在しなぁE��字�E�E�E,
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十�E夜咲夁E,
  "expect": {
    "output_contains": "こ�E斁E���Eは存在しなぁE�Eず_XYZ123"
  }
}
```

#### S7: k4-var-check.json
```json
{
  "name": "K4変数検証",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十�E夜咲夁E,
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

| AC頁E�� | 検証シナリオ |
|--------|-------------|
| シナリオファイル読み込み | S1〜S7全て |
| output_contains | S1, S2, S5, S6 |
| output_not_contains | S3, S5 |
| output_matches | S4 |
| no_errors | S3, S5, S7 |
| var_equals | S7 |
| exit 0�E��E功！E| S1〜S5, S7 |
| exit 1�E�失敗！E| S6 |
| --output compact | 手動確誁E|
| --output quiet | 手動確誁E|
| --output json | 手動確誁E|
| --report | 手動確誁E|

## Test Plan

```bash
# 基本シナリオチE��チE
echo '{"name":"test","call":"KOJO_MESSAGE_COM_K4_300","character":"十�E夜咲夁E,"expect":{"output_contains":"何につぁE��"}}' > test.json
uEmuera --unit test.json
# ↁE✁Etest (0.xxs)

# expect失敗テスチE
echo '{"name":"test","call":"KOJO_MESSAGE_COM_K4_300","character":"十�E夜咲夁E,"expect":{"output_contains":"存在しなぁE��字�E"}}' > test.json
uEmuera --unit test.json
# ↁE✁Etest (0.xxs) + エラー詳細

# JSON出力テスチE
uEmuera --unit test.json --output json
# ↁE{"status":"pass",...}

# レポ�EトファイルチE��チE
uEmuera --unit test.json --report result.json
# ↁEresult.jsonが生成される
```

## Effort Estimate

- **Size**: Medium
- **Risk**: Low�E�Eeature 058の上に構築！E
- **Dependencies**: Feature 058�E�口上関数の直接実行！E

## Links

- [feature-058.md](feature-058.md) - 口上関数の直接実行（前提！E
- [feature-060.md](feature-060.md) - バッチテストと刁E��トレース�E�後続！E
- [reference/testing-reference.md](../reference/testing-reference.md) - チE��ト戦略
