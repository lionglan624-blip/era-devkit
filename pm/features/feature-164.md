# Feature 164: Engine Log Path Auto-Determination

## Status: [DONE]

## Type: engine

## Background

Test Infrastructure Reorganization の第5段階、E
チE��ト実行時のログ出力�Eをエンジンが�E動決定し、E-reportオプションによる改ざんを防止、E

### Problem (解決対象)

| 問顁E| 具体侁E|
|------|--------|
| ログ出力�Eの改ざん | `--report logs/verified/` で本番ログに偽裁E|

### Solution

チE��トパス ↁEログパス自動変換:
- `tests/ac/kojo/feature-135/test.json` ↁE`logs/ac/kojo/feature-135/test-result.json`
- `tests/ac/erb/feature-135/test.json` ↁE`logs/ac/erb/feature-135/test-result.json`

## Dependencies

- Feature 161 (Folder Structure) - logs/ フォルダが忁E��E

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | KojoTestRunner logs/ac/自動�E劁E| file | exists | _out/logs/ac/kojo/test-164-ac1-result.json | [x] |
| 2 | ProcessLevelParallelRunner logs/ac/自動�E劁E| file | exists | _out/logs/ac/erb/test-164-ac2-result.json | [x] |
| 3 | --reportオプション無要E| output | not_contains | "Report written to:" | [x] |
| 4 | ビルド�E劁E| build | succeeds | - | [x] |

> **⚠�E�EAC 2 注愁E*: ProcessLevelParallelRunner は現在ファイル出力機�Eなし。Task 3 で PrintFlowTestResults に WriteResultToFile を追加して実裁E��E
>
> **⚠�E�EAC 3 注愁E*: チE��トファイルパス未持E��、EC 1 のチE��ト�Eで `--report custom/path` を渡し、�E力に "Report written to:" が含まれなぁE��とを確認、E

### AC Test Method

| AC# | 検証方況E| 手頁E|
|:---:|----------|------|
| 1 | inject実衁E| `--unit tests/ac/kojo/feature-164/` ↁElogs/ac/kojo/ 確誁E|
| 2 | inject実衁E| `--inject tests/ac/erb/feature-164/` ↁElogs/ac/erb/ 確誁E|
| 3 | inject実衁E| `--report custom/path` 持E��EↁE無視されることを確誁E|

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | TestPathUtils.cs 新規作�E (DeriveLogPath, GetOutputPath) | [x] |
| 2 | 1 | KojoTestRunner: TestPathUtils.GetOutputPath 使用 | [x] |
| 3 | 2 | ProcessLevelParallelRunner: TestPathUtils.DeriveLogPath 使用 | [x] |
| 4 | 4 | ビルド確誁E| [x] |

## Design

### TestPathUtils.cs (新規作�E)

```csharp
namespace MinorShift.Emuera.Test
{
    public static class TestPathUtils
    {
        /// <summary>
        /// チE��トファイルパスからログ出力パスを�E動決宁E
        /// tests/ac/kojo/feature-135/test.json ↁElogs/ac/kojo/feature-135/test-result.json
        /// tests/ac/erb/feature-135/test.json ↁElogs/ac/erb/feature-135/test-result.json
        /// tests/debug/test.json ↁElogs/debug/test-result.json
        /// </summary>
        public static string DeriveLogPath(string testPath)
        {
            // tests/ ↁElogs/ に変換
            var logPath = testPath.Replace("tests" + Path.DirectorySeparatorChar,
                                            "logs" + Path.DirectorySeparatorChar);
            logPath = logPath.Replace("tests/", "logs/");

            // ファイル名に -result を追加
            var dir = Path.GetDirectoryName(logPath);
            var name = Path.GetFileNameWithoutExtension(logPath);
            var resultPath = Path.Combine(dir, $"{name}-result.json");

            // チE��レクトリが存在しなぁE��合�E作�E
            Directory.CreateDirectory(dir);

            return resultPath;
        }

        /// <summary>
        /// --report オプションを無視し、�E動決定パスを使用
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
// 既存�E --report 処琁E��置揁E
var outputPath = TestPathUtils.GetOutputPath(testFilePath, options.ReportPath);
WriteResult(outputPath, result);
```

### ProcessLevelParallelRunner.cs 変更

```csharp
// PrintFlowTestResults にファイル出力追加
private void PrintFlowTestResults(List<FlowTestResult> results, long totalDurationMs)
{
    // 既存�EConsole出劁E..

    // 吁E��果をログファイルに出劁E
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
| 2025-12-21T10:00:00Z | Feature initialized | initializer | Status: PROPOSED ↁEWIP | READY |
| 2025-12-21T10:05:00Z | Investigation | Explore | Analyzed KojoTestRunner, ProcessLevelParallelRunner, TestPathUtils | READY |
| 2025-12-21T10:10:00Z | Tasks 1-3 implemented | implementer | Created TestPathUtils.cs, modified KojoTestRunner.cs and ProcessLevelParallelRunner.cs | SUCCESS |
| 2025-12-21T10:12:00Z | AC1/AC3 debug | debugger | Fixed TestPathUtils.DeriveLogPath (Path.GetFullPath), modified KojoBatchRunner.OutputResults | FIXED |
| 2025-12-21T10:13:00Z | AC verification | ac-tester | AC1-4 verified | ALL PASS |
| 2025-12-21T10:15:00Z | Feature finalization | finalizer | Status: WIP ↁEDONE, index-features updated | COMPLETE |

---

## Links

- [feature-161.md](feature-161.md) - Folder Structure (依存�E)
- [feature-165.md](feature-165.md) - Documentation Update (依存�E)
