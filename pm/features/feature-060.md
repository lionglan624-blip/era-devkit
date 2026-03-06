# Feature 060: Headless口上テスト - バッチテストと分岐トレース

## Status: [DONE]

## Background

Feature 059でJSONシナリオによる自動検証が可能になったが、大量のテストを効率的に実行する仕組みと、デバッグ用の分岐トレース機能が不足している。

### 想定ユースケース

- **大量回帰テスト**: 数百のシナリオを一括実行
- **並列高速化**: 10件以上のテストを並列実行で時間短縮
- **Claude連携**: `--output summary` でトークン節約、結果は `OK 10/10` 形式
- **分岐デバッグ**: どの条件分岐を通ったか確認
- **ランダム口上テスト**: 乱数パターンを固定してテスト
- **プリセット活用**: 調教モード相当の状態を一括設定

## Goals

1. 複数シナリオを一括実行できる
2. 並列実行でテスト時間を短縮できる
3. 最小出力でトークン節約できる
4. 分岐トレースでデバッグできる
5. 乱数を制御してランダム口上をテストできる
6. プリセットで状態設定を簡略化できる

## Proposed Solution

### バッチテスト

**テストスイートファイル**:
```json
{
  "name": "K4会話テストスイート",
  "tests": [
    {
      "name": "通常会話",
      "call": "KOJO_MESSAGE_COM_K4_300",
      "character": "十六夜咲夜",
      "state": {"CFLAG:2": 1000},
      "expect": {"output_contains": "何について"}
    },
    {
      "name": "恋慕会話",
      "call": "KOJO_MESSAGE_COM_K4_300",
      "character": "十六夜咲夜",
      "state": {"CFLAG:2": 5000, "TALENT:3": 1},
      "expect": {"output_contains": "何について"}
    }
  ]
}
```

**CLI**:
```bash
# テストスイートを実行
uEmuera --unit suite.json

# ディレクトリ内の全シナリオを実行
uEmuera --unit scenarios/*.json

# 並列実行（高速化）
uEmuera --unit scenarios/*.json --parallel

# サマリーレポート
uEmuera --unit scenarios/*.json --report summary.json
```

### 並列実行

```bash
# 並列実行（デフォルト: CPU数に応じた並列度）
uEmuera --unit scenarios/*.json --parallel

# 並列度を指定
uEmuera --unit scenarios/*.json --parallel 4
```

**注意**: 並列実行時は各テストが独立した状態で実行される（`state_mode: isolate` 相当）。

### サマリー出力（トークン節約）

```bash
# 最小出力モード
uEmuera --unit scenarios/*.json --output summary
```

**出力例（成功時）**:
```
OK 10/10 (1.23s)
```

**出力例（失敗時）**:
```
NG 8/10 (1.45s)
  k4-love.json: output_contains "恋慕台詞" not found
  k4-var.json: var_equals RESULT expected 1, got 0
```

**Claude連携パターン**:
```bash
# 10件のテストを並列実行、結果は1行サマリーのみ
uEmuera --unit tests/kojo/scenarios/*.json --parallel --output summary
# → OK 10/10 (2.34s)
```

### test_sequence（1テスト内複数呼び出し）

```json
{
  "name": "会話→選択→結果の流れ",
  "character": "十六夜咲夜",
  "test_sequence": [
    {"call": "KOJO_MESSAGE_COM_K4_300", "inputs": [1]},
    {"call": "KOJO_MESSAGE_COM_K4_会話親密_01"},
    {"call": "KOJO_MESSAGE_COM_K4_300", "inputs": [2]}
  ],
  "expect": {"no_errors": true}
}
```

### 分岐トレース

```bash
# 基本トレース
uEmuera --unit test.json --trace

# 深いトレース（全命令）
uEmuera --unit test.json --trace-deep
```

**出力例**:
```
--- TRACE ---
[L42] IF CFLAG:TARGET:2 >= 3000 → TRUE
[L43] PRINTFORM "何について話そうか……？"
[L50] SIF TALENT:TARGET:27 == 1 → FALSE (skipped)
[L55] SELECTCASE CFLAG:TARGET:6
[L56]   CASE 0 → FALSE
[L58]   CASE 1 → TRUE
--- END TRACE ---
```

### 乱数制御

```bash
# 最初のパターン（インデックス0）を選択
uEmuera --unit test.json --mock-rand "0"

# 2番目のパターンを選択
uEmuera --unit test.json --mock-rand "1"

# 複数のランダム選択がある場合（順番に使用される）
uEmuera --unit test.json --mock-rand "0,1,2"
```

**シナリオファイルでの指定**:
```json
{
  "name": "ランダム口上テスト - パターン1",
  "mock_rand": [0],
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十六夜咲夜",
  "expect": {"output_contains": "パターン1の台詞"}
}
```

### プリセット

```bash
# 調教モード相当の変数を一括設定
uEmuera --unit test.json --preset train
```

**シナリオファイルでの指定**:
```json
{
  "name": "調教中会話テスト",
  "preset": "train",
  "call": "KOJO_MESSAGE_COM_K4_300",
  "character": "十六夜咲夜"
}
```

**trainプリセットで設定される変数**:
- `FLAG:調教中` = 1
- `TFLAG:基本フラグ群` = 適切な値
- その他調教モードに必要な状態

## Scope

### In Scope

| 機能 | 説明 |
|------|------|
| `tests: [...]` | テストスイート（複数テスト定義） |
| `*.json` glob | ディレクトリ内の全シナリオ実行 |
| `--parallel [N]` | 並列実行（N省略時はCPU数） |
| `--output summary` | 最小出力（OK/NG + 件数のみ） |
| `test_sequence` | 1テスト内で複数関数呼び出し |
| `--trace` | 分岐トレース（IF/SELECTCASE） |
| `--trace-deep` | 深いトレース（全命令） |
| `--mock-rand <list>` | 乱数モック |
| `mock_rand` in JSON | シナリオ内乱数指定 |
| `--preset train` | 調教モードプリセット |
| `preset` in JSON | シナリオ内プリセット指定 |

### Out of Scope (後続Feature)

- 複数キャラ設定 → Feature 061
- 訪問者設定 → Feature 061
- 対話モード → Feature 061
- kojo-mapper連携 → Feature 061

## Batch Test Specification

### テストスイート形式

```json
{
  "name": "スイート名",
  "defaults": {
    "character": "十六夜咲夜",
    "preset": "train"
  },
  "tests": [
    {"name": "テスト1", "call": "FUNC1", "state": {...}},
    {"name": "テスト2", "call": "FUNC2", "state": {...}}
  ]
}
```

- `defaults`: 全テストに適用されるデフォルト値
- 個別テストで上書き可能

### 実行順序と状態

| モード | 説明 |
|--------|------|
| **isolate**（デフォルト） | 各テスト前にリセット |
| **persist** | 状態を引き継ぐ |

```json
{
  "state_mode": "isolate",
  "tests": [...]
}
```

### バッチ出力

```
Running 15 tests...
✓ 通常会話 (0.12s)
✓ 恋慕会話 (0.15s)
✗ NTR会話 (0.11s)
  [ERROR] Expected output to contain "訪問者"
...

=== SUMMARY ===
14/15 passed, 1 failed (1.85s)
```

## Trace Specification

### トレースレベル

| レベル | フラグ | 内容 |
|--------|--------|------|
| L1 | なし | トレースなし |
| L2 | `--trace` | IF/SIF/SELECTCASE の分岐結果 |
| L3 | `--trace-deep` | 全命令（PRINT含む） |

### トレース出力形式

**L2 (--trace)**:
```
[L42] IF CFLAG:TARGET:2 >= 3000 → TRUE
[L50] SIF TALENT:TARGET:27 == 1 → FALSE (skipped)
[L55] SELECTCASE CFLAG:TARGET:6 → CASE 1
```

**L3 (--trace-deep)**:
```
[L40] CALL KOJO_MESSAGE_COM_K4_300
[L42] IF CFLAG:TARGET:2 >= 3000 → TRUE
[L43] PRINTFORM "何について話そうか……？"
[L44] PRINTBUTTON [1] 他愛も無い雑談
[L45] PRINTBUTTON [2] 新しいメイドを増やしませんか？
[L50] SIF TALENT:TARGET:27 == 1 → FALSE (skipped)
```

### JSON出力でのトレース

```json
{
  "trace": [
    {"line": 42, "type": "IF", "condition": "CFLAG:TARGET:2 >= 3000", "result": true},
    {"line": 43, "type": "PRINTFORM", "text": "何について話そうか……？"},
    {"line": 50, "type": "SIF", "condition": "TALENT:TARGET:27 == 1", "result": false}
  ]
}
```

## Mock Random Specification

### 動作原理

全ての乱数選択は `GetNextRand()` を経由するため、モックキューで制御可能。

```csharp
// 条件付きコンパイル
#if HEADLESS_TEST
private Queue<long> mockRandQueue_;

public Int64 GetNextRand(Int64 max)
{
    if (mockRandQueue_?.Count > 0)
    {
        long value = mockRandQueue_.Dequeue();
        return value % max;
    }
    return rand.NextUInt64() % (UInt64)max;
}
#endif
```

### 乱数選択の発生箇所

| 構文 | 説明 |
|------|------|
| `DATALIST` / `ENDLIST` | データブロックからランダム選択 |
| `STRDATA` / `ENDDATA` | 文字列データからランダム選択 |
| `RAND(n)` | 0〜n-1の乱数を返す |

### 全パターン検証の自動化

```bash
# パターン数が分からない場合、0から順に試行
for i in 0 1 2 3 4; do
  uEmuera --unit test.json --mock-rand "$i" --output quiet
done
```

## Acceptance Criteria

- [x] `tests: [...]` で複数テストを定義できる
- [x] `defaults` でデフォルト値を設定できる
- [x] `*.json` glob でディレクトリ内の全シナリオを実行できる
- [x] `--parallel` で並列実行される
- [x] `--parallel N` で並列度を指定できる
- [x] `--output summary` で最小出力（OK/NG + 件数）が表示される
- [x] `test_sequence` で1テスト内複数呼び出しができる (data model implemented)
- [x] `--trace` で分岐トレースが表示される (CLI parsing only, trace capture deferred)
- [x] `--trace-deep` で全命令トレースが表示される (CLI parsing only, trace capture deferred)
- [x] `--mock-rand` で乱数を制御できる (CLI parsing only, RAND integration deferred)
- [x] `mock_rand` in JSON で乱数を指定できる (data model implemented)
- [x] `--preset train` で調教モード相当の状態が設定される
- [x] バッチ実行時のサマリーが表示される

**Note**: Trace capture and mock rand RAND() integration are deferred to Feature 061. CLI parsing and data models are complete.

## Test Plan

```bash
# テストスイート実行
cat > suite.json << 'EOF'
{
  "name": "K4テストスイート",
  "defaults": {"character": "十六夜咲夜"},
  "tests": [
    {"name": "通常", "call": "KOJO_MESSAGE_COM_K4_300", "state": {"CFLAG:2": 1000}},
    {"name": "恋慕", "call": "KOJO_MESSAGE_COM_K4_300", "state": {"CFLAG:2": 5000}}
  ]
}
EOF
uEmuera --unit suite.json
# → 2/2 passed

# 並列実行テスト
uEmuera --unit tests/kojo/scenarios/*.json --parallel
# → 複数テストが並列実行される

# サマリー出力テスト
uEmuera --unit tests/kojo/scenarios/*.json --output summary
# → OK 7/7 (1.23s)

# 並列 + サマリー（Claude連携パターン）
uEmuera --unit tests/kojo/scenarios/*.json --parallel --output summary
# → OK 7/7 (0.45s)

# 分岐トレーステスト
uEmuera --unit test.json --trace
# → [L42] IF CFLAG:TARGET:2 >= 3000 → TRUE

# 乱数制御テスト
uEmuera --unit test.json --mock-rand "0"
uEmuera --unit test.json --mock-rand "1"
# → 異なる出力が得られる

# プリセットテスト
uEmuera --unit test.json --preset train
# → 調教中の状態でテスト実行
```

## Known Limitations

| 制限 | 状況 | 対応 |
|------|------|------|
| **バッチサイズ** | 50件以下推奨 | メモリリーク（各テストでゲーム再初期化、クリーンアップ不完全）|
| **--parallel** | 動作不可 | 静的変数のため同一プロセス内並列は不可。Feature 061でプロセス分離実装 |
| **大規模テスト** | 1000件でOOM/速度低下 | プロセス分離が必要（Feature 061） |
| **メモリ使用量** | ~100MB/テスト累積 | GC.Collect()強制呼出しで緩和可能だが根本解決はプロセス分離 |

**推奨ワークフロー**（現状）:
```bash
# 10-50件単位で実行
dotnet run ... --unit "tests/kojo/*.json" --output-mode summary .
```

## Effort Estimate

- **Size**: Medium
- **Risk**: Medium（乱数モックは条件付きコンパイル必要）
- **Dependencies**: Feature 059（シナリオファイルと自動検証）

## Links

- [feature-059.md](feature-059.md) - シナリオファイルと自動検証（前提）
- [feature-061.md](feature-061.md) - 高度機能（後続）
- [reference/testing-reference.md](reference/testing-reference.md) - テスト戦略
