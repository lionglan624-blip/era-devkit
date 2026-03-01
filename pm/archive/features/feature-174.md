# Feature 174: Test CLI Unification (--unit / --flow / --debug)

## Status: [DONE]

## Type: engine

## Background

### Problem

現在のCLIは歴史的経緯で一貫性がない:

1. **モード命名の非対称性**: `--unit` vs `--inject` (目的 vs 実装詳細)
2. **オプション互換性なし**: `--char`, `--set` は `--unit` 専用
3. **JSON形式が2種類**: KojoTestScenario vs Scenario
4. **`--interactive` の用途不明確**: デバッグ用だが名前が汎用的

### Goal

1. テストモードを `--unit` / `--flow` / `--debug` に統一
2. 共通オプションを全モードで使用可能に
3. JSON形式を統一スキーマに（自動判定ロジック込み）
4. `--fail-fast`, `--diff` 追加機能の導入
5. `stderr` matcher type 追加（警告テスト用）
6. 後方互換性維持（deprecated warning付き）

---

## CLI Design

### Before/After

| Before | After | Note |
|--------|-------|------|
| `--inject <file>` | `--flow <file>` | リネーム（後方互換あり） |
| `--interactive` | `--debug` | 用途明確化（後方互換あり） |
| (なし) | `--fail-fast` | 新規追加 |
| (なし) | `--diff` | 新規追加 |

### 統一CLI構造

```
dotnet run -- . <mode> <target> [options]

Test Modes:
  --unit <func|file|dir>    Unit test (function call / scenario / batch)
  --flow <file|dir>         Flow test (state injection + game run)
  --debug                   Interactive debugging (JSON protocol)

Common Options (all modes):
  --char <name>             Set TARGET character
  --master <name>           Set MASTER character
  --set <var>=<val>         Set variable before execution
  --set-after-wakeup <v>=<n> Set variable after wakeup
  --mock-rand <list>        Mock random values (comma-separated)
  --timeout <ms>            Execution timeout (default: 30000)
  --inputs <list>           Input queue (comma-separated)

Execution Control:
  --parallel [N]            Parallel execution (default for directory mode)
  --sequential              Force sequential execution
  --fail-fast               Stop on first failure (NEW)

Output Options:
  --output <file>           Write output to file
  --output-mode <mode>      Format: compact, quiet, json, summary
  --report <file>           Write JSON report
  --verbose, -v             Verbose output
  --diff                    Show diff on assertion failure (NEW)

Debug Options:
  --trace                   Trace IF/SELECTCASE branches
  --trace-deep              Trace all instructions
  --dump-vars <list>        Dump variables after execution
  --coverage-report <file>  Write coverage report
  --log <file>              Session log (for --debug mode)

Save/Load:
  --load <slot>             Load from save slot
  --load-file <file>        Load from file
  --export-slot <slot>      Export save slot
```

### Deprecated (warning表示、動作は継続)

| Option | Replacement | Note |
|--------|-------------|------|
| `--inject` | `--flow` | 目的ベース命名へ |
| `--interactive` | `--debug` | 用途明確化 |

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | `--flow` でフローテスト実行 | output | contains | "[Headless] Loading scenario:" | [x] |
| 2 | `--inject` でフローテスト動作（後方互換） | output | contains | "[Headless] Loading scenario:" | [x] |
| 3 | `--inject` は deprecated warning 表示 | output | contains | "Warning: --inject is deprecated, use --flow instead" | [x] |
| 4 | `--debug` で対話モード起動 | output | contains | "\"status\":\"ready\"" | [x] |
| 5 | `--interactive` で対話モード動作（後方互換） | output | contains | "\"status\":\"ready\"" | [x] |
| 6 | `--interactive` は deprecated warning 表示 | output | contains | "Warning: --interactive is deprecated, use --debug instead" | [x] |
| 7 | `--fail-fast` で最初の失敗で停止 | output | contains | "[Batch] Stopped:" | [x] |
| 8 | `--diff` で差分表示（失敗時） | output | contains | "Expected:" | [x] |
| 9 | `--help` に新オプション表示 | output | contains | "--flow" | [x] |
| 10 | 統一JSONスキーマで自動判定 | output | contains | "[Headless] Detected scenario type:" | [x] |
| 11 | `stderr` matcher type でテスト警告検出 | output | contains | "stderr: PASSED" | [x] |
| 12 | engine ビルド成功 | build | succeeds | - | [x] |

---

## Tasks

<!-- 実装順序: 依存関係考慮済み。グループ化タスク許容（効率的実装のため） -->

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 11 | KojoTestResult: `stderr` matcher type 追加 + 検証用テストシナリオ作成 | [x] |
| 2 | 1,2,3 | HeadlessRunner: `--flow` オプション追加 + `--inject` deprecated warning (stdout出力) | [x] |
| 3 | 4,5,6 | HeadlessRunner: `--debug` オプション追加 + `--interactive` deprecated warning (stdout出力) | [x] |
| 4 | 7 | KojoBatchRunner: `--fail-fast` 実装 | [x] |
| 5 | 8 | KojoTestResult: `--diff` 実装 | [x] |
| 6 | 10 | 統一JSONスキーマ + 自動判定ロジック実装 | [x] |
| 7 | 9 | ShowHelp: 新オプション表示 (`--flow`, `--debug`, `--fail-fast`, `--diff`) | [x] |
| 8 | 12 | engine ビルド + 煙テスト | [x] |

---

## Design Details

### 1. `stderr` matcher type

```csharp
// ExpectChecker.cs
public enum ExpectType
{
    Output,
    Stderr,  // NEW: stderr専用
    Variable,
    Build,
    ExitCode,
    File
}

// KojoTestRunner.cs - stderr capture
var processInfo = new ProcessStartInfo
{
    RedirectStandardError = true,
    // ...
};
result.Stderr = process.StandardError.ReadToEnd();

// ExpectChecker.CheckExpectation()
case ExpectType.Stderr:
    return CheckContains(result.Stderr, expected);
```

### 2. 統一JSONスキーマ

```json
{
  "name": "テスト名",
  "description": "説明（オプション）",

  // === Unit Test用 ===
  "call": "FUNC_NAME",
  "character": "4",
  "master": "0",
  "state": { "CFLAG:TARGET:300": 5000 },
  "inputs": [1, 2, 3],
  "mock_rand": [0, 1, 2],
  "expect": { "output_contains": "..." },

  // バッチテスト用
  "tests": [...],
  "defaults": { "character": "4" },

  // === Flow Test用（Scenarioから統合） ===
  "variables": { "FLAG:0": 100 },
  "add_characters": [1, 2, 3],
  "characters": {
    "4": { "CFLAG:300": 5000 }
  },
  "copy": [
    { "from": "4", "to": "1", "var": "CFLAG:300" }
  ]
}
```

### 3. 自動判定ロジック

```csharp
enum ScenarioType { Unit, Flow, Unknown }

ScenarioType DetectType(KojoTestScenario scenario)
{
    // Unit: call または tests がある
    if (!string.IsNullOrEmpty(scenario.EffectiveFunction)
        || scenario.Tests.Count > 0)
        return ScenarioType.Unit;

    // Flow: variables, add_characters, characters, copy がある
    if (scenario.Variables != null
        || scenario.AddCharacters != null
        || scenario.Characters != null
        || scenario.Copy != null)
        return ScenarioType.Flow;

    return ScenarioType.Unknown;
}

// HeadlessRunner.cs
var type = DetectType(scenario);
Console.WriteLine($"[Headless] Detected scenario type: {type}");
```

### 4. `--fail-fast`

```csharp
// KojoBatchRunner.cs
if (options.FailFast && failCount > 0)
{
    Console.WriteLine($"[Batch] Stopped: {failCount} failure(s), --fail-fast enabled");
    break;
}
```

### 5. `--diff`

```csharp
// KojoTestResult.cs
if (options.ShowDiff && !result.Passed)
{
    Console.WriteLine($"  Expected: {result.Expected}");
    Console.WriteLine($"  Actual:   {result.Actual}");
    // 差分ハイライト
    ShowInlineDiff(result.Expected, result.Actual);
}
```

---

## Migration Guide

### 既存スクリプトの移行

```bash
# Before
dotnet run -- . --inject tests/scenario.json
dotnet run -- . --interactive --char 4

# After
dotnet run -- . --flow tests/scenario.json
dotnet run -- . --debug --char 4

# Deprecated options still work with warning
```

### JSON統一による変更

既存のJSONファイルはそのまま動作。自動判定により適切なモードで実行される。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 | CREATE | orchestrator | Feature 174 作成 | PROPOSED |
| 2025-12-21 | UPDATE | orchestrator | 包括的設計追加 | Updated |
| 2025-12-21 | REVIEW | /next | AC修正: AC8,9削除、警告メッセージ具体化、タスク再編 | Updated |
| 2025-12-21 | EXPAND | /next | スコープ拡張: 統一JSONスキーマ、自動判定、stderr matcher追加 | Updated |
| 2025-12-21 | REVIEW | opus | AC#3,6,11修正: stderr→stdout出力、AC#11検証具体化 | Updated |
| 2025-12-21 | FIX | debugger | AC#11: KojoTestResultFormatterに個別チェック結果出力を追加 | FIXED |
| 2025-12-21 | IMPLEMENT | implementer | Task 1-8 実装完了、全AC検証成功 | ALL PASS |
| 2025-12-21 | FINALIZE | finalizer | Feature 174完了、状態更新、ドキュメント確認 | READY_TO_COMMIT |

---

## Links

- [feature-159.md](feature-159.md) - CLI Option Rename: --kojo-test → --unit
- [feature-161.md](feature-161.md) - Test Folder Structure
- [feature-162.md](feature-162.md) - Test File Migration
- [feature-170.md](feature-170.md) - Default Parallel Execution
