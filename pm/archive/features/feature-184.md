# Feature 184: Test FAIL Log Preservation

## Status: [DONE]

## Type: engine

## Background

### Problem

Feature 183 (Test Workflow Integrity & Protection) の AC4 が BLOCKED となった。
FAIL時のテストログが上書きされ、履歴が消失する問題。

現状:
- テスト実行毎にログファイルが上書き
- FAIL時も同じファイル名で保存
- 後から FAIL 原因を追跡できない

### Goal

FAIL時は `logs/debug/failed/` に履歴としてコピー保存する。
本番ログ (`logs/ac/`, `logs/regression/`) は従来通り上書きし、最終承認時に全PASS確認可能な状態を維持。

### Context

Feature 181 で 63/160 テストが FAIL した際、ログが上書きされ追跡が困難だった。

**初期案の問題**:
当初は「FAIL時にタイムスタンプ付きファイル名で同じディレクトリに保存」を検討したが、
Feature 160-165 の設計思想と競合することが判明:

| 要件 | 初期案の問題 |
|------|-------------|
| 最終承認で全PASS確認 | logs/ac/ に failed-*.json が混在すると確認が煩雑 |
| デバッグと本番の分離 | 本番ログディレクトリにデバッグ用ファイルが混在 |

**解決**: FAIL履歴は `logs/debug/failed/` に分離保存。本番ログは常に最新で上書き。

### Design Philosophy (Feature 160-165 継承)

| 領域 | 役割 | 操作 |
|------|------|------|
| `tests/ac/`, `tests/regression/` | 本番テスト | 新規作成○、編集× (Hook保護) |
| `tests/debug/` | デバッグテスト | 自由編集○ |
| `logs/ac/`, `logs/regression/` | 本番ログ | 常に最新で上書き |
| `logs/debug/` | デバッグログ | 履歴保持 |
| `logs/debug/failed/` | **FAIL履歴** | 自動保存 (本Feature) |

### Workflow Integration (Feature 183 継承)

```
/imple 実行
    ↓
Phase 6-8: テスト実行
    ↓
FAIL発生
    ├─→ 本番ログ (logs/ac/) に最新結果を上書き
    └─→ FAIL履歴 (logs/debug/failed/) に自動コピー ★本Feature
    ↓
debugger 呼び出し
    ├─→ logs/debug/failed/ を参照してFAIL原因調査
    └─→ tests/debug/ でデバッグテスト作成
    ↓
実装修正 (テストシナリオ編集禁止)
    ↓
テスト再実行 → PASS
    ↓
Phase 9-10: 最終承認
    └─→ logs/ac/, logs/regression/ が全PASS確認
```

**ポイント**:
1. **バグ発生時**: debugger が `logs/debug/failed/` を参照して原因調査
2. **ログ履歴**: FAIL時のみ詳細情報が `logs/debug/failed/` に自動保存
3. **本番テスト**: 編集禁止 (Feature 163 Hook)、ログは上書き
4. **最終承認**: `logs/ac/`, `logs/regression/` が全PASS で承認

### Log Structure (Feature 161 拡張)

```
logs/
├── ac/           ← 本番（最終承認用）常に上書き
├── regression/   ← 本番（最終承認用）常に上書き
└── debug/
    ├── *.json    ← tests/debug/ の結果（既存）
    └── failed/   ← FAIL履歴（自動保存）【NEW】
        ├── ac/
        │   └── kojo/
        │       └── feature-181/
        │           └── feature-181-K1-20231223-12345678.json  ← タイムスタンプにミリ秒含む
        ├── regression/
        │   └── scenario-wakeup-20231223-12345678.json
        └── strict/   ← --strict-warnings FAIL履歴【NEW】
            └── strict-20231223-12345678.txt
```

### FAIL時の保存ポリシー

| 対象 | PASS時 | FAIL時 |
|------|--------|--------|
| unit test (--unit) | 本番ログ上書き | 本番ログ上書き + **logs/debug/failed/ac/** |
| regression (--flow) | 本番ログ上書き | 本番ログ上書き + **logs/debug/failed/regression/** |
| strict (--strict-warnings) | コンソールのみ | **logs/debug/failed/strict/** |

**本番は全て丸想定 → FAIL詳細はデバッグのみに保存**

---

## Acceptance Criteria

### コード実装（静的検証）

| AC# | Description | Target | Type | Matcher | Expected | Status |
|:---:|-------------|--------|------|---------|----------|:------:|
| 1 | DeriveFailedLogPath メソッド実装 | TestPathUtils.cs | code | contains | "DeriveFailedLogPath" | [x] |
| 2 | KojoTestRunner FAIL時コピー呼出 | KojoTestRunner.cs | code | contains | "DeriveFailedLogPath" | [x] |
| 3 | KojoBatchRunner FAIL時コピー呼出 | KojoBatchRunner.cs | code | contains | "DeriveFailedLogPath" | [x] |
| 4 | JSONレポートに stderr 含む | KojoTestResult.cs | code | matches | "stderr\\s*=" | [x] |
| 5 | strict FAIL時ログ保存 | post-code-write.ps1 | code | contains | "logs/debug/failed/strict" | [x] |

### ユニットテスト（engine.Tests）

| AC# | Description | Target | Type | Matcher | Expected | Status |
|:---:|-------------|--------|------|---------|----------|:------:|
| 6 | DeriveFailedLogPath ac変換 (ポジ) | TestPathUtilsTests.cs | test | succeeds | - | [x] |
| 7 | DeriveFailedLogPath regression変換 (ポジ) | TestPathUtilsTests.cs | test | succeeds | - | [x] |
| 8 | DeriveFailedLogPath debug除外 (ネガ) | TestPathUtilsTests.cs | test | succeeds | - | [x] |

### ACテスト（ポジネガ）

**検証方法**: tests/ac/ パスでテスト実行し、`[Test] Saved FAIL log:` 出力を確認。tests/debug/ は対象外（AC8設計通り）。

| AC# | Description | Target | Type | Matcher | Expected | Status |
|:---:|-------------|--------|------|---------|----------|:------:|
| 9 | KojoTestRunner FAIL時保存 (ポジ) | tests/ac/* FAIL時 | stdout | contains | "Saved FAIL log:" | [x] |
| 10 | KojoTestRunner PASS時非保存 (ネガ) | tests/debug/* | stdout | not_contains | "Saved FAIL log:" | [x] |
| 11 | KojoBatchRunner FAIL時保存 (ポジ) | tests/ac/* FAIL時 | stdout | contains | "Saved FAIL log:" | [x] |
| 12 | KojoBatchRunner PASS時非保存 (ネガ) | tests/debug/* | stdout | not_contains | "Saved FAIL log:" | [x] |
| 13 | strict FAIL時保存 (ポジ) | tests/debug/feature-184/syntax-error.ERB | stdout | contains | "Strict FAIL log saved:" | [x] |
| 14 | strict PASS時非保存 (ネガ) | tests/debug/feature-184/syntax-ok.ERB | stdout | not_contains | "Strict FAIL log saved:" | [x] |

### ドキュメント

| AC# | Description | Target | Type | Matcher | Expected | Status |
|:---:|-------------|--------|------|---------|----------|:------:|
| 15 | testing skill にログ構造記載 | testing.md | code | contains | "logs/debug/failed" | [x] |
| 16 | imple.md FAIL履歴説明追記 | imple.md | code | contains | "logs/debug/failed" | [x] |
| 17 | debugger.md FAIL履歴参照先追記 | debugger.md | code | contains | "logs/debug/failed" | [x] |
| 18 | finalizer.md ログ全PASS確認手順追記 | finalizer.md | code | contains | "logs/debug/failed" | [x] |

---

## Tasks

### コード実装

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | TestPathUtils: DeriveFailedLogPath 追加 | [x] |
| 2 | 2 | KojoTestRunner: FAIL判定 → failed/ コピー | [x] |
| 3 | 3 | KojoBatchRunner: FAIL判定 → failed/ コピー | [x] |
| 4 | 4 | KojoTestResult: BuildResultObject に stderr 追加 | [x] |
| 5 | 5 | post-code-write.ps1: strict FAIL時 logs/debug/failed/strict/ に保存 | [x] |

### ユニットテスト作成

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 6 | 6 | TestPathUtilsTests: ac変換テスト追加 | [x] |
| 7 | 7 | TestPathUtilsTests: regression変換テスト追加 | [x] |
| 8 | 8 | TestPathUtilsTests: debug除外テスト追加 | [x] |

### ACテスト作成

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 9 | 9 | ACテスト: KojoTestRunner FAIL時保存確認 | [x] |
| 10 | 10 | ACテスト: KojoTestRunner PASS時非保存確認 | [x] |
| 11 | 11 | ACテスト: KojoBatchRunner FAIL時保存確認 | [x] |
| 12 | 12 | ACテスト: KojoBatchRunner PASS時非保存確認 | [x] |
| 13 | 13 | ACテスト: syntax-error.ERB作成 → hook FAIL確認 | [x] |
| 14 | 14 | ACテスト: syntax-ok.ERB作成 → hook非発火確認 | [x] |

### ドキュメント

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 15 | 15 | testing skill: ログ構造追記 | [x] |
| 16 | 16 | imple.md: Phase 6-8 FAIL履歴説明追記 | [x] |
| 17 | 17 | debugger.md: FAIL履歴参照先追記 | [x] |
| 18 | 18 | finalizer.md: ログ全PASS確認手順追記 | [x] |

---

## Design

### 変更ファイル一覧

| File | 変更内容 |
|------|----------|
| `engine/Assets/Scripts/Emuera/Headless/TestPathUtils.cs` | DeriveFailedLogPath メソッド追加 |
| `engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs` | FAIL時に failed/ へコピー |
| `engine/Assets/Scripts/Emuera/Headless/KojoBatchRunner.cs` | FAIL時に failed/ へコピー |
| `engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs` | BuildResultObject に stderr 追加 |
| `.claude/hooks/post-code-write.ps1` | strict FAIL時に logs/debug/failed/strict/ へ保存 |
| `engine.Tests/TestPathUtilsTests.cs` | ユニットテスト追加 (AC6-8) |
| `Game/tests/debug/feature-184/` | ACテストシナリオ (AC9-14) |
| `.claude/skills/testing.md` | ログ構造の説明追加 |
| `.claude/commands/imple.md` | Phase 6-8 自動コピー説明、Phase 10 ログ検証手順 |
| `.claude/agents/debugger.md` | FAIL履歴参照先 (`logs/debug/failed/`) 追記 |
| `.claude/agents/finalizer.md` | ログ全PASS確認手順追記 |

### Task 1: TestPathUtils.cs 追加

```csharp
/// <summary>
/// FAIL時のログ保存先パスを生成
/// tests/ac/feature-181/test.json → logs/debug/failed/ac/feature-181/test-20231223-123456.json
/// </summary>
public static string DeriveFailedLogPath(string testPath)
{
    testPath = Path.GetFullPath(testPath);

    // tests/ac/ or tests/regression/ を判定
    string subDir = "";
    if (testPath.Contains("tests" + Path.DirectorySeparatorChar + "ac"))
        subDir = "ac";
    else if (testPath.Contains("tests" + Path.DirectorySeparatorChar + "regression"))
        subDir = "regression";
    else
        return null; // debug/ などは対象外

    // パス変換: tests/ac/... → logs/debug/failed/ac/...
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
// RunScenario() 内、既存のログ出力後に追加:
// 122-123行目付近

// Feature 184: FAIL時は logs/debug/failed/ にもコピー
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
// OutputResults() 内、既存のログ出力後に追加:
// 854-858行目付近

// Feature 184: FAIL時は logs/debug/failed/ にもコピー（バッチ全体のサマリー）
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

**バッチ実行時の動作**:
- 複数テスト実行時は `firstScenarioPath` から派生したパスに保存
- サマリー全体（全テスト結果）が FAIL履歴として保存される
- 個別テストのFAIL詳細はサマリー内の `results` 配列で確認可能

### Task 4: KojoTestResult.cs - BuildResultObject に stderr 追加

**問題**: `Stderr` プロパティは存在するが、JSONレポートに含まれていない

```csharp
// KojoTestResult.cs BuildResultObject() 修正
// 387-459行目付近

// PASS時
return new
{
    name = result.Name,
    function = result.Function,
    status = "pass",
    duration_ms = result.DurationMs,
    output = result.Output,
    stderr = result.Stderr,  // ← 追加
    branches = branchesArray,
    expect_results = expectResultsArray
};

// FAIL時
return new
{
    name = result.Name,
    function = result.Function,
    status = "fail",
    duration_ms = result.DurationMs,
    errors = errorMessages,
    output = result.Output,
    stderr = result.Stderr,  // ← 追加
    branches = branchesArray,
    expect_results = expectResultsArray
};
```

**効果**: FAIL履歴ログに stderr が含まれ、debugger がエラー原因を調査可能

### Task 5: post-code-write.ps1 - strict FAIL時ログ保存

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

**修正後**:
```powershell
# 3. Strict Syntax Check (ERB only)
if ($isERB -and -not $hasError) {
    $strictResult = dotnet run ... --strict-warnings 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "[Hook] Strict warnings FAILED"
        $strictResult | ForEach-Object { Write-Error "  $_" }
        $hasError = $true

        # Feature 184: FAIL時は logs/debug/failed/strict/ に保存
        $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
        $failedDir = "Game/logs/debug/failed/strict"
        if (-not (Test-Path $failedDir)) {
            New-Item -ItemType Directory -Path $failedDir -Force | Out-Null
        }
        $logPath = "$failedDir/strict-$timestamp.txt"
        $strictResult | Out-File -FilePath $logPath -Encoding UTF8
        Write-Host "[Hook] Strict FAIL log saved: $logPath"
    }
}
```

**効果**: strict FAIL時のエラー詳細が `logs/debug/failed/strict/` に履歴として残り、debugger が参照可能

### Task 6-8: ユニットテスト (engine.Tests/TestPathUtilsTests.cs)

```csharp
[TestClass]
public class TestPathUtilsTests
{
    // Task 6: AC6 - ac変換テスト (ポジ)
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

    // Task 7: AC7 - regression変換テスト (ポジ)
    [TestMethod]
    public void DeriveFailedLogPath_RegressionPath_ReturnsFailedPath()
    {
        var input = @"C:\Era\Game\tests\regression\scenario.json";
        var result = TestPathUtils.DeriveFailedLogPath(input);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains(@"logs\debug\failed\regression"));
    }

    // Task 8: AC8 - debug除外テスト (ネガ)
    [TestMethod]
    public void DeriveFailedLogPath_DebugPath_ReturnsNull()
    {
        var input = @"C:\Era\Game\tests\debug\test.json";
        var result = TestPathUtils.DeriveFailedLogPath(input);

        Assert.IsNull(result);  // debug/ は対象外
    }
}
```

### Task 9-14: ACテスト設計

ACテストは `Game/tests/debug/feature-184/` に配置（本番テスト保護のため）。

**ディレクトリ構成**:
```
tests/debug/feature-184/
├── force-fail.json          ← Task 9: 単体FAIL
├── force-pass.json          ← Task 10: 単体PASS
├── batch-with-fail/         ← Task 11: バッチFAIL
│   ├── test-pass.json
│   └── test-fail.json
├── batch-all-pass/          ← Task 12: バッチPASS
│   ├── test-pass-1.json
│   └── test-pass-2.json
├── syntax-error.ERB         ← Task 13: strict FAIL
└── syntax-ok.ERB            ← Task 14: strict PASS
```

**テスト実行方法**:
```bash
# Task 9: FAIL時保存確認 (ポジ)
dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit Game/tests/debug/feature-184/force-fail.json
# 期待: "[Test] Saved FAIL log:" がコンソールに出力される

# Task 10: PASS時非保存確認 (ネガ)
dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit Game/tests/debug/feature-184/force-pass.json
# 期待: "[Test] Saved FAIL log:" がコンソールに出力されない

# Task 11: バッチFAIL時保存確認 (ポジ)
dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit Game/tests/debug/feature-184/batch-with-fail/
# 期待: "[Test] Saved FAIL log:" がコンソールに出力される

# Task 12: バッチPASS時非保存確認 (ネガ)
dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit Game/tests/debug/feature-184/batch-all-pass/
# 期待: "[Test] Saved FAIL log:" がコンソールに出力されない
```

**テストシナリオ**:

```json
// force-fail.json - 意図的にFAILするテスト
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
// force-pass.json - 意図的にPASSするテスト
{
  "scenario": "Feature 184 PASS Test",
  "tests": [{
    "name": "force-pass",
    "function": "@SYSTEM_TITLE",
    "expect": [{ "type": "output", "contains": "紅魔館" }]
  }]
}
```

**Task 13-14**: strict テストは post-code-write.ps1 hook で確認（手動検証）。
- ポジ: 構文エラーのある ERB 編集 → `Strict FAIL log saved:` 出力
- ネガ: 正常な ERB 編集 → `Strict FAIL log saved:` 出力なし

---

## Review Notes

- **2025-12-23**: Feature 183 AC4 から分離。エンジンC#修正が必要なため独立フィーチャー化。
- **2025-12-23**: レビュー指摘対応。設計方針を明確化 - 本番ログ上書き + FAIL履歴は logs/debug/failed/ に保存。
- **2025-12-23**: ACを検証可能な形式に修正（コード存在確認）。Task-AC対応を整理。バッチ実行時の動作を明確化。
- **2025-12-23**: Feature 183 要件「コンソール出力永続化」対応。AC4/Task4 追加 - BuildResultObject に stderr 追加。
- **2025-12-23**: strict (--strict-warnings) FAIL時ログ保存を追加。AC5/Task5 追加 - post-code-write.ps1 修正。
- **2025-12-23**: レビュー指摘対応 - ポジネガ両方のテスト追加。AC 7→18件、Task 9→18件に拡張。engine.Tests ユニットテスト + ACテスト設計追加。
- **2025-12-23**: 最終レビュー修正:
  - AC4: `"stderr = result.Stderr"` → 正規表現 `"stderr\\s*="` に変更（空白差異対策）
  - AC9-14: 検証方法明確化 - `stdout` タイプ + `"Saved FAIL log:"` メッセージ
  - タイムスタンプ: `yyyyMMdd-HHmmss` → `yyyyMMdd-HHmmssff` （並列実行時衝突回避）
  - Task 11-12: ディレクトリ構成を明確化（batch-with-fail/, batch-all-pass/）
  - コンソール出力追加: `[Test] Saved FAIL log: {path}` メッセージ

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| | | | | |

---

## Links

- [feature-183.md](feature-183.md) - 親フィーチャー (Test Workflow Integrity)
- [feature-164.md](feature-164.md) - Engine Log Path Auto-Determination
