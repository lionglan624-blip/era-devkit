# Feature 062: Headless口上テスト - Phase 2 (高度機能拡張)

## Status: [DONE] (Phase 2a Interactive Mode complete)

## Background

Feature 061 (Phase 1) で複数キャラ設定、訪問者設定、--dump-vars を実装した。
Phase 2 では、Claude連携のための対話モード、kojo-mapper連携のためのカバレッジレポート、
およびプロセス分離による真の並列実行を実装する。

### Phase 1 (Feature 061) で完了した機能
- `--assi`, `--present`, `--room` CLI options
- `characters`, `location` in JSON scenario
- `--visitor-at`, `--visitor-favorite` CLI options
- `visitor` in JSON scenario
- `--dump-vars` option with wildcard support

### Phase 2 で実装する機能
1. **対話モード** (`--interactive`): Claude連携のためのJSON-RPCライクなインターフェース
2. **カバレッジレポート** (`--coverage-report`): kojo-mapper連携用の分岐カバレッジ出力
3. **プロセス分離並列実行**: メモリリークなしの真の並列テスト

## Goals

1. `--interactive` モードでClaudeが対話的に口上をテスト・確認できる
2. `--coverage-report` でkojo-mapperと連携してカバレッジを分析できる
3. プロセス分離により100件以上のテストでもメモリが安定する

## Proposed Solution

### 対話モード (`--interactive`)

Claudeが起動し、stdin/stdoutでJSON通信を行う。自動デバッグワークフローの一環。

```bash
# 基本起動
uEmuera --unit --interactive --char "十六夜咲夜" Game/

# 初期状態付き
uEmuera --unit --interactive --char "十六夜咲夜" --state "CFLAG:2=5000" Game/

# セッションログ出力
uEmuera --unit --interactive --char "十六夜咲夜" --log session.log Game/
```

**設計方針**:

| 項目 | 仕様 |
|------|------|
| 出力 | **差分のみ返却**（コマンドごとの新規出力のみ、履歴蓄積なし） |
| ログ | `--log <file>` でコマンド/レスポンス全記録 |
| タイムアウト | 各コマンドに `timeout_ms` パラメータ（デフォルト30秒） |
| ERBエラー | JSON返却、セッション維持（`{"status": "error", ...}`） |
| INPUT/WAIT | スキップ（WAIT無視、INPUT→デフォルト0） |
| 並列セッション | 不要（単一セッションのみ） |

**プロトコル**:
```
[初期化完了]
<-- {"status": "ready"}

[コマンド送信]
--> {"cmd": "call", "func": "...", "timeout_ms": 30000}
<-- {"status": "ok", "output": "新規出力のみ", "lines": 3, "duration_ms": 120}

[変数設定]
--> {"cmd": "set", "var": "CFLAG:2", "value": 5000}
<-- {"status": "ok"}

[状態確認]
--> {"cmd": "dump", "vars": ["CFLAG:TARGET:2"]}
<-- {"status": "ok", "vars": {"CFLAG:4:2": 5000}}

[タイムアウト発生]
--> {"cmd": "call", "func": "SLOW_FUNC", "timeout_ms": 1000}
<-- {"status": "timeout", "error": "Command timed out after 1000ms"}

[ERBエラー発生]
--> {"cmd": "call", "func": "BROKEN_FUNC"}
<-- {"status": "error", "error": "Undefined variable: HOGE", "line": 42, "file": "KOJO.ERB"}

[終了]
--> {"cmd": "exit"}
<-- {"status": "exit"}
```

**コマンド一覧**:

| cmd | パラメータ | 説明 |
|-----|-----------|------|
| `call` | `func`, `inputs`, `timeout_ms` | 関数呼び出し（出力は差分のみ） |
| `set` | `var`, `value` | 変数設定 |
| `dump` | `vars` | 変数値取得 |
| `reset` | - | 状態リセット（初期化時の状態に戻る） |
| `exit` | - | 終了 |

**タスクキル**:
- 各コマンド: `timeout_ms` でタイムアウト
- 正常終了: `{"cmd": "exit"}`
- 強制終了: Ctrl+C / SIGINT（親プロセスから）

### カバレッジレポート (`--coverage-report`)

```bash
uEmuera --unit test.json --coverage-report coverage.json
```

**出力形式**:
```json
{
  "file": "KOJO_K4_会話親密.ERB",
  "function": "KOJO_MESSAGE_COM_K4_300",
  "branches_total": 5,
  "branches_hit": 2,
  "coverage": 0.4,
  "branches": [
    {"line": 42, "condition": "CFLAG:TARGET:2 >= 3000", "hit": true},
    {"line": 50, "condition": "TALENT:TARGET:27 == 1", "hit": false}
  ]
}
```

### プロセス分離並列実行

**方式**: 各テストを別プロセスで実行
```bash
# 内部動作: 4並列ワーカー、各テストを dotnet run で起動
uEmuera --unit suite.json --parallel 4
```

**メリット**:
- プロセス終了でメモリ完全解放 → メモリリークなし
- 静的変数の干渉なし
- OS レベルの並列性

**自動並列数決定**:
```csharp
// 空きメモリから安全な並列数を計算
var availableMemory = GetAvailablePhysicalMemory(); // OS API
var threshold = GetMemoryThreshold(); // 実測値（暫定500MB、実装時に計測して調整）
var safeParallel = (int)(availableMemory / threshold);
safeParallel = Math.Max(1, Math.Min(safeParallel, Environment.ProcessorCount));
```

**動的調整**:
- 実行中に空きメモリが1GB未満になったら新規ワーカー起動を待機
- 完了したワーカーのメモリ解放を待ってから次を起動

## Scope

### In Scope

| 機能 | 説明 |
|------|------|
| `--interactive` | 対話モード（JSON-RPC風プロトコル） |
| `--log <file>` | 対話セッションログ出力 |
| `call` cmd | 関数呼び出し（差分出力、タイムアウト対応） |
| `set` cmd | 変数設定 |
| `dump` cmd | 変数値取得 |
| `reset` cmd | 状態リセット |
| ERBエラーハンドリング | JSON返却、セッション維持 |
| INPUT/WAITスキップ | WAIT無視、INPUT→デフォルト0 |
| `--coverage-report <file>` | カバレッジレポート出力 |
| Process-level parallel | プロセス分離による並列実行 |
| `--parallel auto` | 空きメモリから並列数自動決定 |
| Memory threshold | 1プロセスあたりのメモリ使用量計測 |
| Dynamic adjustment | 実行中のメモリ監視と並列数調整 |

### Out of Scope

- ブレークポイント・ステップ実行
- ERBキャッシュ・デーモンモード
- 差分コンパイル
- 複数並列セッション

## Acceptance Criteria

### 対話モード (Phase 2a - Complete)
- [x] `--interactive` で対話モードが起動する
- [x] 初期化完了時に `{"status": "ready"}` が出力される
- [x] `call` コマンドで関数を呼び出せる
- [x] `call` の出力は**差分のみ**（前回からの新規出力のみ返却）
- [x] `call` で `timeout_ms` パラメータによるタイムアウトが機能する
- [x] `set` コマンドで変数を設定できる
- [x] `dump` コマンドで変数値を取得できる
- [x] `reset` コマンドで状態をリセットできる
- [x] `exit` コマンドで終了できる
- [x] 対話モードで複数コマンドを連続実行できる
- [x] ERBエラー時に `{"status": "error", ...}` が返却され、セッションは維持される
- [x] INPUT/WAITはスキップされる（WAIT無視、INPUT→0）
- [x] `--log <file>` でコマンド/レスポンス全記録が出力される
- [x] Ctrl+C / SIGINT で graceful shutdown される
- [x] 不正JSONコマンドに対して `{"status": "error", "error": "Invalid JSON: ..."}` が返却される
- [x] 未知のcmdに対して `{"status": "error", "error": "Unknown command: xxx"}` が返却される

### カバレッジレポート (Phase 2b - Deferred)
- [ ] `--coverage-report <file>` でJSONファイルが出力される
- [ ] 分岐の総数と実行された分岐数がレポートされる
- [ ] 各分岐の条件と実行有無がレポートされる

### プロセス分離並列実行 (Phase 2c - Deferred)
- [ ] `--parallel N` でプロセス分離による並列実行ができる
- [ ] `--parallel auto` で空きメモリから並列数を自動決定
- [ ] 100件以上のテストでもメモリが線形増加しない
- [ ] 実行速度がテスト数に比例する（N件目で鈍化しない）
- [ ] 並列数 × スレッショルド程度のメモリで収まる
- [ ] 並列実行時のメモリ使用量をログ出力できる（`--verbose`）
- [ ] 実行中にメモリ不足を検知したら並列数を動的に減らす
- [ ] **メモリスレッショルド実測**: 1プロセスあたりの実メモリ使用量を計測

## Test Plan

```bash
# 対話モードテスト
echo '{"cmd":"call","func":"KOJO_MESSAGE_COM_K4_300","char":"十六夜咲夜"}
{"cmd":"set","var":"CFLAG:2","value":5000}
{"cmd":"call","func":"KOJO_MESSAGE_COM_K4_300","char":"十六夜咲夜"}
{"cmd":"exit"}' | uEmuera --unit --interactive
# → 正常にレスポンスが返ることを確認

# カバレッジレポートテスト
uEmuera --unit test.json --coverage-report coverage.json
cat coverage.json
# → branches_total, branches_hit が含まれることを確認

# プロセス分離並列テスト
uEmuera --unit suite.json --parallel 4 --verbose
# → メモリ使用量が安定していることを確認
```

## Effort Estimate

- **Size**: Large
- **Risk**: Medium-High（対話モードのプロトコル設計、プロセス分離の実装）
- **Dependencies**: Feature 061 (Phase 1)

## Links

- [feature-061.md](feature-061.md) - Phase 1（複数キャラ・訪問者・dump-vars）
- [feature-060.md](feature-060.md) - バッチテストと分岐トレース
- [reference/kojo-reference.md](reference/kojo-reference.md) - 口上システム概要
- [reference/testing-reference.md](reference/testing-reference.md) - テスト戦略
