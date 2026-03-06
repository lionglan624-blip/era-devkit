# Feature 160: File Exists Matcher Implementation

## Status: [DONE]

## Type: engine

## Background

Feature 161-165 (Test Infrastructure Reorganization) の前提条件。
ファイル/ディレクトリ存在確認のmatcherがないと、フォルダ構造やmigrationのACを自動検証できない。

## Dependencies

- なし（最初に実装）

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | CheckFileExists メソッド存在確認 | code | contains | "public static ExpectCheckResult CheckFileExists" | [x] |
| 2 | CheckFileNotExists メソッド存在確認 | code | contains | "public static ExpectCheckResult CheckFileNotExists" | [x] |
| 3 | FileExists プロパティ追加確認 | code | contains | "public List<string> FileExists" | [x] |
| 4 | FileNotExists プロパティ追加確認 | code | contains | "public List<string> FileNotExists" | [x] |
| 5 | ビルド成功 | build | succeeds | - | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | KojoExpectValidator: CheckFileExists メソッド追加 | [x] |
| 2 | 2 | KojoExpectValidator: CheckFileNotExists メソッド追加 | [x] |
| 3 | 3 | KojoTestExpect: FileExists プロパティ追加 | [x] |
| 4 | 4 | KojoTestExpect: FileNotExists プロパティ追加 | [x] |
| 5 | 1,2 | Validate() メソッドに file_exists/file_not_exists 処理追加 | [x] |
| 6 | 5 | ビルド確認 | [x] |

## Design

### KojoExpectValidator.cs 追加

```csharp
/// <summary>
/// Check that a file or directory exists.
/// </summary>
public static ExpectCheckResult CheckFileExists(string path)
{
    bool exists = File.Exists(path) || Directory.Exists(path);
    return new ExpectCheckResult
    {
        Passed = exists,
        CheckType = "file_exists",
        Message = exists
            ? $"Path exists: {path}"
            : $"Expected path to exist: {path}",
        Expected = path,
        Actual = exists ? "exists" : "not found"
    };
}

/// <summary>
/// Check that a file or directory does NOT exist.
/// </summary>
public static ExpectCheckResult CheckFileNotExists(string path)
{
    bool exists = File.Exists(path) || Directory.Exists(path);
    return new ExpectCheckResult
    {
        Passed = !exists,
        CheckType = "file_not_exists",
        Message = !exists
            ? $"Path does not exist: {path}"
            : $"Expected path to NOT exist: {path}",
        Expected = $"NOT: {path}",
        Actual = exists ? "exists" : "not found"
    };
}
```

### KojoTestExpect.cs 追加

```csharp
[JsonPropertyName("file_exists")]
public List<string> FileExists { get; set; }

[JsonPropertyName("file_not_exists")]
public List<string> FileNotExists { get; set; }
```

### Validate() メソッド追加

```csharp
// Check file_exists
if (expect.FileExists != null)
{
    foreach (var path in expect.FileExists)
    {
        results.Add(CheckFileExists(path));
    }
}

// Check file_not_exists
if (expect.FileNotExists != null)
{
    foreach (var path in expect.FileNotExists)
    {
        results.Add(CheckFileNotExists(path));
    }
}
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 | Phase 1 | initializer | Feature state check | READY |
| 2025-12-21 | Phase 2 | explorer | Code investigation | Complete |
| 2025-12-21 | Phase 3 | implementer | Task 1-6 implementation | SUCCESS |
| 2025-12-21 | Phase 6 | regression-tester | Regression tests | OK:85/85 |
| 2025-12-21 | Phase 7 | ac-tester | AC1-5 verification | All PASS |
| 2025-12-21 | Phase 8 | finalizer | Commit & finalize | [DONE] |

---

## Links

- [feature-161.md](feature-161.md) - Folder Structure (依存先)
- [feature-162.md](feature-162.md) - Migration (依存先)
