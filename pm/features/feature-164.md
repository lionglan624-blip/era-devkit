# Feature 164: Engine Log Path Auto-Determination

## Status: [DONE]

## Type: engine

## Background

Test Infrastructure Reorganization の第5段階。
テスト実行時のログ出力先をエンジンが自動決定し、--reportオプションによる改ざんを防止。

### Problem (解決対象)

| 問題 | 具体例 |
|------|--------|
| ログ出力先の改ざん | `--report logs/verified/` で本番ログに偽装 |

### Solution

テストパス → ログパス自動変換:
- `tests/ac/kojo/feature-135/test.json` → `logs/ac/kojo/feature-135/test-result.json`
- `tests/ac/erb/feature-135/test.json` → `logs/ac/erb/feature-135/test-result.json`

## Dependencies

- Feature 161 (Folder Structure) - logs/ フォルダが必要

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | KojoTestRunner logs/ac/自動出力 | file | exists | Game/logs/ac/kojo/test-164-ac1-result.json | [x] |
| 2 | ProcessLevelParallelRunner logs/ac/自動出力 | file | exists | Game/logs/ac/erb/test-164-ac2-result.json | [x] |
| 3 | --reportオプション無視 | output | not_contains | "Report written to:" | [x] |
| 4 | ビルド成功 | build | succeeds | - | [x] |

> **⚠️ AC 2 注意**: ProcessLevelParallelRunner は現在ファイル出力機能なし。Task 3 で PrintFlowTestResults に WriteResultToFile を追加して実装。
>
> **⚠️ AC 3 注意**: テストファイルパス未指定。AC 1 のテスト内で `--report custom/path` を渡し、出力に "Report written to:" が含まれないことを確認。

### AC Test Method

| AC# | 検証方法 | 手順 |
|:---:|----------|------|
| 1 | inject実行 | `--unit tests/ac/kojo/feature-164/` → logs/ac/kojo/ 確認 |
| 2 | inject実行 | `--inject tests/ac/erb/feature-164/` → logs/ac/erb/ 確認 |
| 3 | inject実行 | `--report custom/path` 指定 → 無視されることを確認 |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | TestPathUtils.cs 新規作成 (DeriveLogPath, GetOutputPath) | [x] |
| 2 | 1 | KojoTestRunner: TestPathUtils.GetOutputPath 使用 | [x] |
| 3 | 2 | ProcessLevelParallelRunner: TestPathUtils.DeriveLogPath 使用 | [x] |
| 4 | 4 | ビルド確認 | [x] |

## Design

### TestPathUtils.cs (新規作成)

```csharp
namespace MinorShift.Emuera.Test
{
    public static class TestPathUtils
    {
        /// <summary>
        /// テストファイルパスからログ出力パスを自動決定
        /// tests/ac/kojo/feature-135/test.json → logs/ac/kojo/feature-135/test-result.json
        /// tests/ac/erb/feature-135/test.json → logs/ac/erb/feature-135/test-result.json
        /// tests/debug/test.json → logs/debug/test-result.json
        /// </summary>
        public static string DeriveLogPath(string testPath)
        {
            // tests/ → logs/ に変換
            var logPath = testPath.Replace("tests" + Path.DirectorySeparatorChar,
                                            "logs" + Path.DirectorySeparatorChar);
            logPath = logPath.Replace("tests/", "logs/");

            // ファイル名に -result を追加
            var dir = Path.GetDirectoryName(logPath);
            var name = Path.GetFileNameWithoutExtension(logPath);
            var resultPath = Path.Combine(dir, $"{name}-result.json");

            // ディレクトリが存在しない場合は作成
            Directory.CreateDirectory(dir);

            return resultPath;
        }

        /// <summary>
        /// --report オプションを無視し、自動決定パスを使用
        /// </summary>
        public static string GetOutputPath(string testPath, string userReportPath)
        {
            // userReportPath は無視し、常に自動決定パスを使用
            return DeriveLogPath(testPath);
        }
    }
}
```

### KojoTestRunner.cs 変更

```csharp
// 既存の --report 処理を置換
var outputPath = TestPathUtils.GetOutputPath(testFilePath, options.ReportPath);
WriteResult(outputPath, result);
```

### ProcessLevelParallelRunner.cs 変更

```csharp
// PrintFlowTestResults にファイル出力追加
private void PrintFlowTestResults(List<FlowTestResult> results, long totalDurationMs)
{
    // 既存のConsole出力...

    // 各結果をログファイルに出力
    foreach (var result in results)
    {
        var outputPath = TestPathUtils.DeriveLogPath(result.ScenarioFile);
        WriteResultToFile(outputPath, result);
    }
}
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21T10:00:00Z | Feature initialized | initializer | Status: PROPOSED → WIP | READY |
| 2025-12-21T10:05:00Z | Investigation | Explore | Analyzed KojoTestRunner, ProcessLevelParallelRunner, TestPathUtils | READY |
| 2025-12-21T10:10:00Z | Tasks 1-3 implemented | implementer | Created TestPathUtils.cs, modified KojoTestRunner.cs and ProcessLevelParallelRunner.cs | SUCCESS |
| 2025-12-21T10:12:00Z | AC1/AC3 debug | debugger | Fixed TestPathUtils.DeriveLogPath (Path.GetFullPath), modified KojoBatchRunner.OutputResults | FIXED |
| 2025-12-21T10:13:00Z | AC verification | ac-tester | AC1-4 verified | ALL PASS |
| 2025-12-21T10:15:00Z | Feature finalization | finalizer | Status: WIP → DONE, index-features updated | COMPLETE |

---

## Links

- [feature-161.md](feature-161.md) - Folder Structure (依存元)
- [feature-165.md](feature-165.md) - Documentation Update (依存先)
