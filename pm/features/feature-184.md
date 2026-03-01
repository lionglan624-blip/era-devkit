# Feature 184: Test FAIL Log Preservation

## Status: [DONE]

## Type: engine

## Background

### Problem

Feature 183 (Test Workflow Integrity & Protection) の AC4 ぁEBLOCKED となった、E
FAIL時�EチE��トログが上書きされ、履歴が消失する問題、E

現状:
- チE��ト実行毎にログファイルが上書ぁE
- FAIL時も同じファイル名で保孁E
- 後かめEFAIL 原因を追跡できなぁE

### Goal

FAIL時�E `logs/debug/failed/` に履歴としてコピ�E保存する、E
本番ログ (`logs/ac/`, `logs/regression/`) は従来通り上書きし、最終承認時に全PASS確認可能な状態を維持、E

### Context

Feature 181 で 63/160 チE��トが FAIL した際、ログが上書きされ追跡が困難だった、E

**初期案�E問顁E*:
当�Eは「FAIL時にタイムスタンプ付きファイル名で同じチE��レクトリに保存」を検討したが、E
Feature 160-165 の設計思想と競合することが判昁E

| 要件 | 初期案�E問顁E|
|------|-------------|
| 最終承認で全PASS確誁E| logs/ac/ に failed-*.json が混在すると確認が煩雁E|
| チE��チE��と本番の刁E�� | 本番ログチE��レクトリにチE��チE��用ファイルが混在 |

**解決**: FAIL履歴は `logs/debug/failed/` に刁E��保存。本番ログは常に最新で上書き、E

### Design Philosophy (Feature 160-165 継承)

| 領域 | 役割 | 操佁E|
|------|------|------|
| `tests/ac/`, `tests/regression/` | 本番チE��チE| 新規作�E○、編雁E�E(Hook保護) |
| `tests/debug/` | チE��チE��チE��チE| 自由編雁E�� |
| `logs/ac/`, `logs/regression/` | 本番ログ | 常に最新で上書ぁE|
| `logs/debug/` | チE��チE��ログ | 履歴保持 |
| `logs/debug/failed/` | **FAIL履歴** | 自動保孁E(本Feature) |

### Workflow Integration (Feature 183 継承)

```
/imple 実衁E
    ↁE
Phase 6-8: チE��ト実衁E
    ↁE
FAIL発甁E
    ├─ↁE本番ログ (logs/ac/) に最新結果を上書ぁE
    └─ↁEFAIL履歴 (logs/debug/failed/) に自動コピ�E ☁E��Feature
    ↁE
debugger 呼び出ぁE
    ├─ↁElogs/debug/failed/ を参照してFAIL原因調査
    └─ↁEtests/debug/ でチE��チE��チE��ト作�E
    ↁE
実裁E��正 (チE��トシナリオ編雁E��止)
    ↁE
チE��ト�E実衁EↁEPASS
    ↁE
Phase 9-10: 最終承誁E
    └─ↁElogs/ac/, logs/regression/ が�EPASS確誁E
```

**ポインチE*:
1. **バグ発生時**: debugger ぁE`logs/debug/failed/` を参照して原因調査
2. **ログ履歴**: FAIL時�Eみ詳細惁E��ぁE`logs/debug/failed/` に自動保孁E
3. **本番チE��チE*: 編雁E��止 (Feature 163 Hook)、ログは上書ぁE
4. **最終承誁E*: `logs/ac/`, `logs/regression/` が�EPASS で承誁E

### Log Structure (Feature 161 拡張)

```
logs/
├── ac/           ↁE本番�E�最終承認用�E�常に上書ぁE
├── regression/   ↁE本番�E�最終承認用�E�常に上書ぁE
└── debug/
    ├── *.json    ↁEtests/debug/ の結果�E�既存！E
    └── failed/   ↁEFAIL履歴�E��E動保存）【NEW、E
        ├── ac/
        ━E  └── kojo/
        ━E      └── feature-181/
        ━E          └── feature-181-K1-20231223-12345678.json  ↁEタイムスタンプにミリ秒含む
        ├── regression/
        ━E  └── scenario-wakeup-20231223-12345678.json
        └── strict/   ↁE--strict-warnings FAIL履歴【NEW、E
            └── strict-20231223-12345678.txt
```

### FAIL時�E保存�Eリシー

| 対象 | PASS晁E| FAIL晁E|
|------|--------|--------|
| unit test (--unit) | 本番ログ上書ぁE| 本番ログ上書ぁE+ **logs/debug/failed/ac/** |
| regression (--flow) | 本番ログ上書ぁE| 本番ログ上書ぁE+ **logs/debug/failed/regression/** |
| strict (--strict-warnings) | コンソールのみ | **logs/debug/failed/strict/** |

**本番は全て丸想宁EↁEFAIL詳細はチE��チE��のみに保孁E*

---

## Acceptance Criteria

### コード実裁E��静皁E��証�E�E

| AC# | Description | Target | Type | Matcher | Expected | Status |
|:---:|-------------|--------|------|---------|----------|:------:|
| 1 | DeriveFailedLogPath メソチE��実裁E| TestPathUtils.cs | code | contains | "DeriveFailedLogPath" | [x] |
| 2 | KojoTestRunner FAIL時コピ�E呼出 | KojoTestRunner.cs | code | contains | "DeriveFailedLogPath" | [x] |
| 3 | KojoBatchRunner FAIL時コピ�E呼出 | KojoBatchRunner.cs | code | contains | "DeriveFailedLogPath" | [x] |
| 4 | JSONレポ�Eトに stderr 含む | KojoTestResult.cs | code | matches | "stderr\\s*=" | [x] |
| 5 | strict FAIL時ログ保孁E| post-code-write.ps1 | code | contains | "logs/debug/failed/strict" | [x] |

### ユニットテスト！Engine.Tests�E�E

| AC# | Description | Target | Type | Matcher | Expected | Status |
|:---:|-------------|--------|------|---------|----------|:------:|
| 6 | DeriveFailedLogPath ac変換 (ポジ) | TestPathUtilsTests.cs | test | succeeds | - | [x] |
| 7 | DeriveFailedLogPath regression変換 (ポジ) | TestPathUtilsTests.cs | test | succeeds | - | [x] |
| 8 | DeriveFailedLogPath debug除夁E(ネガ) | TestPathUtilsTests.cs | test | succeeds | - | [x] |

### ACチE��ト（�Eジネガ�E�E

**検証方況E*: tests/ac/ パスでチE��ト実行し、`[Test] Saved FAIL log:` 出力を確認。tests/debug/ は対象外！EC8設計通り�E�、E

| AC# | Description | Target | Type | Matcher | Expected | Status |
|:---:|-------------|--------|------|---------|----------|:------:|
| 9 | KojoTestRunner FAIL時保孁E(ポジ) | tests/ac/* FAIL晁E| stdout | contains | "Saved FAIL log:" | [x] |
| 10 | KojoTestRunner PASS時非保孁E(ネガ) | tests/debug/* | stdout | not_contains | "Saved FAIL log:" | [x] |
| 11 | KojoBatchRunner FAIL時保孁E(ポジ) | tests/ac/* FAIL晁E| stdout | contains | "Saved FAIL log:" | [x] |
| 12 | KojoBatchRunner PASS時非保孁E(ネガ) | tests/debug/* | stdout | not_contains | "Saved FAIL log:" | [x] |
| 13 | strict FAIL時保孁E(ポジ) | tests/debug/feature-184/syntax-error.ERB | stdout | contains | "Strict FAIL log saved:" | [x] |
| 14 | strict PASS時非保孁E(ネガ) | tests/debug/feature-184/syntax-ok.ERB | stdout | not_contains | "Strict FAIL log saved:" | [x] |

### ドキュメンチE

| AC# | Description | Target | Type | Matcher | Expected | Status |
|:---:|-------------|--------|------|---------|----------|:------:|
| 15 | testing skill にログ構造記輁E| testing.md | code | contains | "logs/debug/failed" | [x] |
| 16 | imple.md FAIL履歴説明追訁E| imple.md | code | contains | "logs/debug/failed" | [x] |
| 17 | debugger.md FAIL履歴参�E先追訁E| debugger.md | code | contains | "logs/debug/failed" | [x] |
| 18 | finalizer.md ログ全PASS確認手頁E��訁E| finalizer.md | code | contains | "logs/debug/failed" | [x] |

---

## Tasks

### コード実裁E

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | TestPathUtils: DeriveFailedLogPath 追加 | [x] |
| 2 | 2 | KojoTestRunner: FAIL判宁EↁEfailed/ コピ�E | [x] |
| 3 | 3 | KojoBatchRunner: FAIL判宁EↁEfailed/ コピ�E | [x] |
| 4 | 4 | KojoTestResult: BuildResultObject に stderr 追加 | [x] |
| 5 | 5 | post-code-write.ps1: strict FAIL晁Elogs/debug/failed/strict/ に保孁E| [x] |

### ユニットテスト作�E

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 6 | 6 | TestPathUtilsTests: ac変換チE��ト追加 | [x] |
| 7 | 7 | TestPathUtilsTests: regression変換チE��ト追加 | [x] |
| 8 | 8 | TestPathUtilsTests: debug除外テスト追加 | [x] |

### ACチE��ト作�E

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 9 | 9 | ACチE��チE KojoTestRunner FAIL時保存確誁E| [x] |
| 10 | 10 | ACチE��チE KojoTestRunner PASS時非保存確誁E| [x] |
| 11 | 11 | ACチE��チE KojoBatchRunner FAIL時保存確誁E| [x] |
| 12 | 12 | ACチE��チE KojoBatchRunner PASS時非保存確誁E| [x] |
| 13 | 13 | ACチE��チE syntax-error.ERB作�E ↁEhook FAIL確誁E| [x] |
| 14 | 14 | ACチE��チE syntax-ok.ERB作�E ↁEhook非発火確誁E| [x] |

### ドキュメンチE

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 15 | 15 | testing skill: ログ構造追訁E| [x] |
| 16 | 16 | imple.md: Phase 6-8 FAIL履歴説明追訁E| [x] |
| 17 | 17 | debugger.md: FAIL履歴参�E先追訁E| [x] |
| 18 | 18 | finalizer.md: ログ全PASS確認手頁E��訁E| [x] |

---

## Design

### 変更ファイル一覧

| File | 変更冁E�� |
|------|----------|
| `engine/Assets/Scripts/Emuera/Headless/TestPathUtils.cs` | DeriveFailedLogPath メソチE��追加 |
| `engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs` | FAIL時に failed/ へコピ�E |
| `engine/Assets/Scripts/Emuera/Headless/KojoBatchRunner.cs` | FAIL時に failed/ へコピ�E |
| `engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs` | BuildResultObject に stderr 追加 |
| `.claude/hooks/post-code-write.ps1` | strict FAIL時に logs/debug/failed/strict/ へ保孁E|
| `engine.Tests/TestPathUtilsTests.cs` | ユニットテスト追加 (AC6-8) |
| `test/debug/feature-184/` | ACチE��トシナリオ (AC9-14) |
| `.claude/skills/testing.md` | ログ構造の説明追加 |
| `.claude/commands/imple.md` | Phase 6-8 自動コピ�E説明、Phase 10 ログ検証手頁E|
| `.claude/agents/debugger.md` | FAIL履歴参�E允E(`logs/debug/failed/`) 追訁E|
| `.claude/agents/finalizer.md` | ログ全PASS確認手頁E��訁E|

### Task 1: TestPathUtils.cs 追加

```csharp
/// <summary>
/// FAIL時�Eログ保存�Eパスを生戁E
/// tests/ac/feature-181/test.json ↁElogs/debug/failed/ac/feature-181/test-20231223-123456.json
/// </summary>
public static string DeriveFailedLogPath(string testPath)
{
    testPath = Path.GetFullPath(testPath);

    // tests/ac/ or tests/regression/ を判宁E
    string subDir = "";
    if (testPath.Contains("tests" + Path.DirectorySeparatorChar + "ac"))
        subDir = "ac";
    else if (testPath.Contains("tests" + Path.DirectorySeparatorChar + "regression"))
        subDir = "regression";
    else
        return null; // debug/ などは対象夁E

    // パス変換: tests/ac/... ↁElogs/debug/failed/ac/...
    var logPath = testPath.Replace(
        "tests" + Path.DirectorySeparatorChar + subDir,
        "logs" + Path.DirectorySeparatorChar + "debug" +
        Path.DirectorySeparatorChar + "failed" +
        Path.DirectorySeparatorChar + subDir);

    var dir = Path.GetDirectoryName(logPath);
    var name = Path.GetFileNameWithoutExtension(logPath);
    var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmssff"); // ミリ秒追加で並列実行時の衝突回避

    if (!string.IsNullOrEmpty(dir))
        Directory.CreateDirectory(dir);

    return Path.Combine(dir, $"{name}-{timestamp}.json");
}
```

### Task 2: KojoTestRunner.cs 修正

```csharp
// RunScenario() 冁E��既存�Eログ出力後に追加:
// 122-123行目付迁E

// Feature 184: FAIL時�E logs/debug/failed/ にもコピ�E
if (summary.Failed > 0)
{
    var failedLogPath = TestPathUtils.DeriveFailedLogPath(scenarioPath);
    if (failedLogPath != null)
    {
        KojoTestResultFormatter.WriteReportFile(summary, failedLogPath);
        Console.WriteLine($"[Test] Saved FAIL log: {failedLogPath}");
    }
}
```

### Task 3: KojoBatchRunner.cs 修正

```csharp
// OutputResults() 冁E��既存�Eログ出力後に追加:
// 854-858行目付迁E

// Feature 184: FAIL時�E logs/debug/failed/ にもコピ�E�E�バチE��全体�Eサマリー�E�E
if (summary.Failed > 0 && !string.IsNullOrEmpty(firstScenarioPath))
{
    var failedLogPath = TestPathUtils.DeriveFailedLogPath(firstScenarioPath);
    if (failedLogPath != null)
    {
        KojoTestResultFormatter.WriteReportFile(summary, failedLogPath);
        Console.WriteLine($"[Test] Saved FAIL log: {failedLogPath}");
    }
}
```

**バッチ実行時の動佁E*:
- 褁E��チE��ト実行時は `firstScenarioPath` から派生したパスに保孁E
- サマリー全体（�EチE��ト結果�E�が FAIL履歴として保存される
- 個別チE��ト�EFAIL詳細はサマリー冁E�E `results` 配�Eで確認可能

### Task 4: KojoTestResult.cs - BuildResultObject に stderr 追加

**問顁E*: `Stderr` プロパティは存在するが、JSONレポ�Eトに含まれてぁE��ぁE

```csharp
// KojoTestResult.cs BuildResultObject() 修正
// 387-459行目付迁E

// PASS晁E
return new
{
    name = result.Name,
    function = result.Function,
    status = "pass",
    duration_ms = result.DurationMs,
    output = result.Output,
    stderr = result.Stderr,  // ↁE追加
    branches = branchesArray,
    expect_results = expectResultsArray
};

// FAIL晁E
return new
{
    name = result.Name,
    function = result.Function,
    status = "fail",
    duration_ms = result.DurationMs,
    errors = errorMessages,
    output = result.Output,
    stderr = result.Stderr,  // ↁE追加
    branches = branchesArray,
    expect_results = expectResultsArray
};
```

**効极E*: FAIL履歴ログに stderr が含まれ、debugger がエラー原因を調査可能

### Task 5: post-code-write.ps1 - strict FAIL時ログ保孁E

**現状** (84-96行目):
```powershell
# 3. Strict Syntax Check (ERB only)
if ($isERB -and -not $hasError) {
    $strictResult = dotnet run ... --strict-warnings 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "[Hook] Strict warnings FAILED"
        $strictResult | ForEach-Object { Write-Error "  $_" }
        $hasError = $true
    }
}
```

**修正征E*:
```powershell
# 3. Strict Syntax Check (ERB only)
if ($isERB -and -not $hasError) {
    $strictResult = dotnet run ... --strict-warnings 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "[Hook] Strict warnings FAILED"
        $strictResult | ForEach-Object { Write-Error "  $_" }
        $hasError = $true

        # Feature 184: FAIL時�E logs/debug/failed/strict/ に保孁E
        $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
        $failedDir = "_out/logs/debug/failed/strict"
        if (-not (Test-Path $failedDir)) {
            New-Item -ItemType Directory -Path $failedDir -Force | Out-Null
        }
        $logPath = "$failedDir/strict-$timestamp.txt"
        $strictResult | Out-File -FilePath $logPath -Encoding UTF8
        Write-Host "[Hook] Strict FAIL log saved: $logPath"
    }
}
```

**効极E*: strict FAIL時�Eエラー詳細ぁE`logs/debug/failed/strict/` に履歴として残り、debugger が参照可能

### Task 6-8: ユニットテスチE(engine.Tests/TestPathUtilsTests.cs)

```csharp
[TestClass]
public class TestPathUtilsTests
{
    // Task 6: AC6 - ac変換チE��チE(ポジ)
    [TestMethod]
    public void DeriveFailedLogPath_AcPath_ReturnsFailedPath()
    {
        var input = @"C:\Era\Game\tests\ac\kojo\feature-181\test.json";
        var result = TestPathUtils.DeriveFailedLogPath(input);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains(@"logs\debug\failed\ac"));
        Assert.IsTrue(result.Contains("feature-181"));
        Assert.IsTrue(result.EndsWith(".json"));
    }

    // Task 7: AC7 - regression変換チE��チE(ポジ)
    [TestMethod]
    public void DeriveFailedLogPath_RegressionPath_ReturnsFailedPath()
    {
        var input = @"C:\Era\Game\tests\regression\scenario.json";
        var result = TestPathUtils.DeriveFailedLogPath(input);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains(@"logs\debug\failed\regression"));
    }

    // Task 8: AC8 - debug除外テスチE(ネガ)
    [TestMethod]
    public void DeriveFailedLogPath_DebugPath_ReturnsNull()
    {
        var input = @"C:\Era\Game\tests\debug\test.json";
        var result = TestPathUtils.DeriveFailedLogPath(input);

        Assert.IsNull(result);  // debug/ は対象夁E
    }
}
```

### Task 9-14: ACチE��ト設訁E

ACチE��ト�E `test/debug/feature-184/` に配置�E�本番チE��ト保護のため�E�、E

**チE��レクトリ構�E**:
```
tests/debug/feature-184/
├── force-fail.json          ↁETask 9: 単体FAIL
├── force-pass.json          ↁETask 10: 単体PASS
├── batch-with-fail/         ↁETask 11: バッチFAIL
━E  ├── test-pass.json
━E  └── test-fail.json
├── batch-all-pass/          ↁETask 12: バッチPASS
━E  ├── test-pass-1.json
━E  └── test-pass-2.json
├── syntax-error.ERB         ↁETask 13: strict FAIL
└── syntax-ok.ERB            ↁETask 14: strict PASS
```

**チE��ト実行方況E*:
```bash
# Task 9: FAIL時保存確誁E(ポジ)
dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit test/debug/feature-184/force-fail.json
# 期征E "[Test] Saved FAIL log:" がコンソールに出力される

# Task 10: PASS時非保存確誁E(ネガ)
dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit test/debug/feature-184/force-pass.json
# 期征E "[Test] Saved FAIL log:" がコンソールに出力されなぁE

# Task 11: バッチFAIL時保存確誁E(ポジ)
dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit test/debug/feature-184/batch-with-fail/
# 期征E "[Test] Saved FAIL log:" がコンソールに出力される

# Task 12: バッチPASS時非保存確誁E(ネガ)
dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit test/debug/feature-184/batch-all-pass/
# 期征E "[Test] Saved FAIL log:" がコンソールに出力されなぁE
```

**チE��トシナリオ**:

```json
// force-fail.json - 意図皁E��FAILするチE��チE
{
  "scenario": "Feature 184 FAIL Test",
  "tests": [{
    "name": "force-fail",
    "function": "@SYSTEM_TITLE",
    "expect": [{ "type": "output", "contains": "NEVER_MATCH_STRING" }]
  }]
}
```

```json
// force-pass.json - 意図皁E��PASSするチE��チE
{
  "scenario": "Feature 184 PASS Test",
  "tests": [{
    "name": "force-pass",
    "function": "@SYSTEM_TITLE",
    "expect": [{ "type": "output", "contains": "紁E��館" }]
  }]
}
```

**Task 13-14**: strict チE��ト�E post-code-write.ps1 hook で確認（手動検証�E�、E
- ポジ: 構文エラーのある ERB 編雁EↁE`Strict FAIL log saved:` 出劁E
- ネガ: 正常な ERB 編雁EↁE`Strict FAIL log saved:` 出力なぁE

---

## Review Notes

- **2025-12-23**: Feature 183 AC4 から刁E��。エンジンC#修正が忁E��なため独立フィーチャー化、E
- **2025-12-23**: レビュー持E��対応。設計方針を明確匁E- 本番ログ上書ぁE+ FAIL履歴は logs/debug/failed/ に保存、E
- **2025-12-23**: ACを検証可能な形式に修正�E�コード存在確認）。Task-AC対応を整琁E��バチE��実行時の動作を明確化、E
- **2025-12-23**: Feature 183 要件「コンソール出力永続化」対応、EC4/Task4 追加 - BuildResultObject に stderr 追加、E
- **2025-12-23**: strict (--strict-warnings) FAIL時ログ保存を追加、EC5/Task5 追加 - post-code-write.ps1 修正、E
- **2025-12-23**: レビュー持E��対忁E- ポジネガ両方のチE��ト追加、EC 7ↁE8件、Task 9ↁE8件に拡張。engine.Tests ユニットテスチE+ ACチE��ト設計追加、E
- **2025-12-23**: 最終レビュー修正:
  - AC4: `"stderr = result.Stderr"` ↁE正規表現 `"stderr\\s*="` に変更�E�空白差異対策！E
  - AC9-14: 検証方法�E確匁E- `stdout` タイチE+ `"Saved FAIL log:"` メチE��ージ
  - タイムスタンチE `yyyyMMdd-HHmmss` ↁE`yyyyMMdd-HHmmssff` �E�並列実行時衝突回避�E�E
  - Task 11-12: チE��レクトリ構�Eを�E確化！Eatch-with-fail/, batch-all-pass/�E�E
  - コンソール出力追加: `[Test] Saved FAIL log: {path}` メチE��ージ

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| | | | | |

---

## Links

- [feature-183.md](feature-183.md) - 親フィーチャー (Test Workflow Integrity)
- [feature-164.md](feature-164.md) - Engine Log Path Auto-Determination
