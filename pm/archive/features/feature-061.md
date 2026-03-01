# Feature 061: Headless口上テスト - 高度機能

## Status: [DONE] (Phase 1)

## Background

Feature 058-060で基本的な口上テスト機能が揃ったが、複雑なシナリオ（複数キャラ、訪問者、NTR）のテストや、Claude連携、kojo-mapperとの統合など高度な機能が不足している。

### 想定ユースケース

- **複数キャラシナリオ**: 3Pシーン、同室キャラがいる状況のテスト
- **訪問者シナリオ**: NTR系口上のテスト
- **Claude連携**: 対話的に口上をテスト・修正
- **カバレッジ分析**: kojo-mapperと連携して分岐網羅率を確認

## Goals

1. 複数キャラクター（ASSI、同室キャラ）を設定できる
2. 訪問者の状態を設定できる
3. 実行後の状態を確認できる
4. 対話的にテストできる（Claude連携）
5. kojo-mapperと連携してカバレッジを確認できる

## Proposed Solution

### 複数キャラクター設定

**CLI**:
```bash
# ASSIを指定
uEmuera --unit test.json --char "十六夜咲夜" --assi "フラン"

# 同室キャラを指定
uEmuera --unit test.json --char "十六夜咲夜" --present "パチュリー,小悪魔"

# 部屋を指定
uEmuera --unit test.json --char "十六夜咲夜" --room 13
```

**シナリオファイル**:
```json
{
  "name": "3Pシーンテスト",
  "characters": {
    "TARGET": "十六夜咲夜",
    "MASTER": "レミリア",
    "ASSI": "フラン"
  },
  "location": {
    "room": 13,
    "present": ["パチュリー", "小悪魔"]
  },
  "state": {
    "ASSIPLAY": 1,
    "CFLAG:TARGET:好感度": 5000,
    "CFLAG:ASSI:好感度": 3000
  },
  "call": "KOJO_MESSAGE_COM_K4_3P",
  "expect": {"no_errors": true}
}
```

### 訪問者設定

**CLI**:
```bash
# 訪問者を咲夜私室に配置
uEmuera --unit test.json --visitor-at 13

# 訪問者のお気に入りを設定
uEmuera --unit test.json --visitor-at 13 --visitor-favorite "十六夜咲夜"
```

**シナリオファイル**:
```json
{
  "name": "NTR口上テスト",
  "character": "十六夜咲夜",
  "visitor": {
    "position": 13,
    "favorite": "十六夜咲夜",
    "present_with_target": true
  },
  "state": {
    "TALENT:TARGET:NTR": 1,
    "CFLAG:TARGET:訪問者好感度": 3000
  },
  "call": "KOJO_MESSAGE_NTR_K4",
  "expect": {"output_contains": "訪問者"}
}
```

### 状態確認（dump-vars）

**CLI**:
```bash
# 実行後の変数値を出力
uEmuera --unit test.json --dump-vars "CFLAG:TARGET:2,TALENT:TARGET:*,RESULT"
```

**出力**:
```
--- STATE AFTER ---
CFLAG:TARGET:2 = 5000
TALENT:TARGET:3 = 1
TALENT:TARGET:6 = 0
TALENT:TARGET:27 = 1
RESULT = 1
--- END STATE ---
```

**シナリオファイル**:
```json
{
  "dump_vars_after": ["CFLAG:TARGET:2", "TALENT:TARGET:*", "RESULT"]
}
```

### 対話モード（Claude連携）

```bash
# 対話モードで起動
uEmuera --unit --interactive
```

**動作**:
1. 初期化後、標準入力からJSONコマンドを受け付け
2. 各コマンドを実行し、結果をJSON出力
3. 状態を維持したまま次のコマンドを受け付け

**コマンド例**:
```json
{"cmd": "call", "func": "KOJO_MESSAGE_COM_K4_300", "char": "十六夜咲夜"}
{"cmd": "set", "var": "CFLAG:2", "value": 5000}
{"cmd": "call", "func": "KOJO_MESSAGE_COM_K4_300", "char": "十六夜咲夜"}
{"cmd": "dump", "vars": ["CFLAG:TARGET:2"]}
{"cmd": "exit"}
```

**レスポンス例**:
```json
{"status": "ok", "output": "何について話そうか……？\n..."}
{"status": "ok"}
{"status": "ok", "output": "...恋慕分岐の出力..."}
{"status": "ok", "vars": {"CFLAG:4:2": 5000}}
{"status": "exit"}
```

### kojo-mapper連携

```bash
# kojo-mapperの出力からテストシナリオを自動生成
python kojo_mapper.py --output-test-scenarios > scenarios/auto-generated.json

# 実行時カバレッジをkojo-mapperに送信
uEmuera --unit test.json --coverage-report coverage.json
python kojo_mapper.py --merge-coverage coverage.json
```

**カバレッジレポート**:
```json
{
  "file": "KOJO_K4_会話親密.ERB",
  "function": "KOJO_MESSAGE_COM_K4_300",
  "branches_total": 5,
  "branches_hit": 2,
  "coverage": 0.4,
  "branches": [
    {"line": 42, "condition": "CFLAG:TARGET:2 >= 3000", "hit": true},
    {"line": 50, "condition": "TALENT:TARGET:27 == 1", "hit": false},
    {"line": 55, "condition": "TALENT:TARGET:6 == 1", "hit": true},
    {"line": 60, "condition": "TFLAG:200 == 1", "hit": false},
    {"line": 65, "condition": "CFLAG:TARGET:21 >= 500", "hit": false}
  ]
}
```

## Scope

### In Scope

| 機能 | 説明 |
|------|------|
| `--assi <name>` | ASSIキャラ指定 |
| `--present <names>` | 同室キャラ指定（カンマ区切り） |
| `--room <n>` | 部屋指定 |
| `characters` in JSON | 複数キャラ設定 |
| `location` in JSON | 位置・同室設定 |
| `--visitor-at <room>` | 訪問者位置設定 |
| `--visitor-favorite <name>` | 訪問者お気に入り設定 |
| `visitor` in JSON | 訪問者設定 |
| `--dump-vars <list>` | 実行後状態確認 |
| `--interactive` | 対話モード |
| `--coverage-report <file>` | カバレッジレポート出力 |

### Out of Scope (将来課題)

- ブレークポイント・ステップ実行（対話的デバッグはGUIで）
- ERBキャッシュ・デーモンモード（初期化高速化）
- 差分コンパイル

### Feature 060 からの引継ぎ（並列実行改善）

Feature 060で`--parallel`を実装したが、静的変数のため同一プロセス内並列は不可。
プロセス分離による真の並列実行を実装する。

**方式**: 各テストを別プロセスで実行
```bash
# 内部動作: 4並列ワーカー、各テストを dotnet run で起動
uEmuera --unit suite.json --parallel 4
```

**メリット**:
- プロセス終了でメモリ完全解放 → メモリリークなし
- 静的変数の干渉なし
- OS レベルの並列性

**デメリット**:
- プロセス起動オーバーヘッド（約1秒/テスト）
- 並列数はRAMに依存（1プロセス約500MB）

**自動並列数決定**:
```csharp
// 空きメモリから安全な並列数を計算
var availableMemory = GetAvailablePhysicalMemory(); // OS API
var threshold = GetMemoryThreshold(); // 実測値（暫定500MB、実装時に計測して調整）
var safeParallel = (int)(availableMemory / threshold);
safeParallel = Math.Max(1, Math.Min(safeParallel, Environment.ProcessorCount));
```

**メモリスレッショルド検証手順**:
1. 1プロセスでテスト実行し、ピークメモリを計測
2. 複数回実行して安定値を取得
3. 安全マージン（1.2〜1.5倍）を乗せてスレッショルド決定
4. 設定ファイル or 定数として保存

**動的調整**:
- 実行中に空きメモリが1GB未満になったら新規ワーカー起動を待機
- 完了したワーカーのメモリ解放を待ってから次を起動

## Multiple Character Specification

### characters フィールド

```json
{
  "characters": {
    "TARGET": "十六夜咲夜",
    "MASTER": "レミリア",
    "ASSI": "フラン"
  }
}
```

| キー | 説明 | デフォルト |
|------|------|-----------|
| `TARGET` | 対象キャラ | 必須 |
| `MASTER` | 主体キャラ | PLAYER (0) |
| `ASSI` | アシストキャラ | 未設定 |

### location フィールド

```json
{
  "location": {
    "room": 13,
    "present": ["パチュリー", "小悪魔"]
  }
}
```

設定される変数:
- `CFLAG:TARGET:現在位置` = room
- `CFLAG:パチュリー:現在位置` = room
- `CFLAG:パチュリー:同室` = 1
- `CFLAG:小悪魔:現在位置` = room
- `CFLAG:小悪魔:同室` = 1

### state でのシンボル展開

```json
{
  "characters": {
    "TARGET": "十六夜咲夜",
    "ASSI": "フラン"
  },
  "state": {
    "CFLAG:TARGET:好感度": 5000,
    "CFLAG:ASSI:好感度": 3000
  }
}
```

展開結果:
- `CFLAG:4:2` = 5000 (十六夜咲夜の好感度)
- `CFLAG:6:2` = 3000 (フランの好感度)

## Visitor Specification

### visitor フィールド

```json
{
  "visitor": {
    "position": 13,
    "favorite": "十六夜咲夜",
    "dislike": null,
    "stay_time": 10,
    "accompanying": false,
    "present_with_target": true
  }
}
```

設定される変数:
- `FLAG:訪問者の現在位置` = position
- `FLAG:訪問者のお気に入り` = キャラインデックス
- `FLAG:訪問者の嫌いな相手` = キャラインデックス (null時は999)
- `FLAG:訪問者滞在時間カウンタ` = stay_time
- `FLAG:訪問者同行フラグ` = accompanying ? 1 : 0

### NTR好感度レベル設定

```json
{
  "visitor": {
    "position": 13,
    "favorably_level": {
      "十六夜咲夜": "FAV_うふふする程度"
    }
  }
}
```

| レベル | 値 | 意味 |
|--------|---|------|
| `FAV_寝取られ` | 9 | 完全に寝取られた |
| `FAV_寝取られ寸前` | 8 | 陥落寸前 |
| `FAV_寝取られそう` | 7 | 危険な状態 |
| `FAV_主人より高い` | 6 | 訪問者の方が好き |
| `FAV_うふふする程度` | 4 | 性行為OK |
| `FAV_奉仕する程度` | 3 | 奉仕OK |
| `FAV_体を触らせる程度` | 2 | 触れてOK |
| `FAV_キスする程度` | 1 | キスまでOK |

## Interactive Mode Specification

### プロトコル

```
[初期化完了]
<-- {"status": "ready"}

[コマンド送信]
--> {"cmd": "call", "func": "...", "char": "..."}
<-- {"status": "ok", "output": "...", "duration_ms": 120}

[変数設定]
--> {"cmd": "set", "var": "CFLAG:2", "value": 5000}
<-- {"status": "ok"}

[状態確認]
--> {"cmd": "dump", "vars": ["CFLAG:TARGET:2"]}
<-- {"status": "ok", "vars": {"CFLAG:4:2": 5000}}

[終了]
--> {"cmd": "exit"}
<-- {"status": "exit"}
```

### コマンド一覧

| cmd | パラメータ | 説明 |
|-----|-----------|------|
| `call` | `func`, `char`, `inputs` | 関数呼び出し |
| `set` | `var`, `value` | 変数設定 |
| `dump` | `vars` | 変数値取得 |
| `reset` | - | 状態リセット |
| `exit` | - | 終了 |

### Claude連携フロー

```
Claude: 口上を確認したいです
User: uEmuera --unit --interactive を実行

Claude: {"cmd": "call", "func": "KOJO_MESSAGE_COM_K4_300", "char": "十六夜咲夜"}
System: {"status": "ok", "output": "何について話そうか……？\n..."}

Claude: 恋慕分岐を確認します
Claude: {"cmd": "set", "var": "CFLAG:2", "value": 5000}
System: {"status": "ok"}

Claude: {"cmd": "call", "func": "KOJO_MESSAGE_COM_K4_300", "char": "十六夜咲夜"}
System: {"status": "ok", "output": "...恋慕分岐の出力..."}
```

## Acceptance Criteria

### 複数キャラ・訪問者 (Phase 1 - Implemented)
- [x] `--assi <name>` でASSIが設定される
- [x] `--present <names>` で同室キャラが設定される
- [x] `--room <n>` で部屋が設定される
- [x] `characters` in JSON で複数キャラが設定される
- [x] `location` in JSON で位置・同室が設定される
- [x] `--visitor-at <room>` で訪問者位置が設定される
- [x] `visitor` in JSON で訪問者状態が設定される

### 状態確認 (Phase 1 - Implemented)
- [x] `--dump-vars` で実行後の変数値が表示される
- [x] ワイルドカード（`TALENT:TARGET:*`）で非ゼロ値一覧が表示される

### 対話モード・カバレッジ (Phase 2 - Deferred)
- [ ] `--interactive` で対話モードが起動する
- [ ] 対話モードで複数コマンドを連続実行できる
- [ ] `--coverage-report` でカバレッジレポートが出力される

### 並列実行改善（Feature 060 引継ぎ - Phase 2 - Deferred）
- [ ] `--parallel N` でプロセス分離による並列実行ができる
- [ ] `--parallel auto` で空きメモリから並列数を自動決定（デフォルト）
- [ ] 100件以上のテストでもメモリが線形増加しない（プロセス分離で解放）
- [ ] 実行速度がテスト数に比例する（N件目で鈍化しない）
- [ ] 並列数 × スレッショルド程度のメモリで収まる（パンクしない）
- [ ] 並列実行時のメモリ使用量をログ出力できる（`--verbose`）
- [ ] 実行中にメモリ不足を検知したら並列数を動的に減らす
- [ ] **メモリスレッショルド実測**: 1プロセスあたりの実メモリ使用量を計測し、適切な値を決定（暫定500MB）

> **Note**: Phase 2 items (interactive mode, coverage report, process-level parallel) are deferred to future work due to complexity. Phase 1 provides the core multi-character and visitor testing capabilities.

## Test Plan

```bash
# 複数キャラテスト
uEmuera --unit test.json --char "十六夜咲夜" --assi "フラン" --present "パチュリー"
# → ASSI=6, 同室にパチュリーが設定されることを確認

# 訪問者テスト
uEmuera --unit test.json --char "十六夜咲夜" --visitor-at 13 --visitor-favorite "十六夜咲夜"
# → 訪問者関連変数が設定されることを確認

# 状態確認テスト
uEmuera --unit test.json --dump-vars "CFLAG:TARGET:2,RESULT"
# → 実行後の変数値が表示されることを確認

# 対話モードテスト
echo '{"cmd":"call","func":"KOJO_MESSAGE_COM_K4_300","char":"十六夜咲夜"}
{"cmd":"exit"}' | uEmuera --unit --interactive
# → 正常にレスポンスが返ることを確認
```

## Effort Estimate

- **Size**: Medium
- **Risk**: Medium（対話モードのプロトコル設計が重要）
- **Dependencies**: Feature 060（バッチテストと分岐トレース）

## Links

- [feature-060.md](feature-060.md) - バッチテストと分岐トレース（前提）
- [reference/kojo-reference.md](reference/kojo-reference.md) - 口上システム概要
- [reference/testing-reference.md](reference/testing-reference.md) - テスト戦略
