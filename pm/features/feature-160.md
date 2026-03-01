# Feature 160: File Exists Matcher Implementation

## Status: [DONE]

## Type: engine

## Background

Feature 161-165 (Test Infrastructure Reorganization) 縺ｮ蜑肴署譚｡莉ｶ縲・
繝輔ぃ繧､繝ｫ/繝・ぅ繝ｬ繧ｯ繝医Μ蟄伜惠遒ｺ隱阪・matcher縺後↑縺・→縲√ヵ繧ｩ繝ｫ繝讒矩繧・igration縺ｮAC繧定・蜍墓､懆ｨｼ縺ｧ縺阪↑縺・・

## Dependencies

- 縺ｪ縺暦ｼ域怙蛻昴↓螳溯｣・ｼ・

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | CheckFileExists 繝｡繧ｽ繝・ラ蟄伜惠遒ｺ隱・| code | contains | "public static ExpectCheckResult CheckFileExists" | [x] |
| 2 | CheckFileNotExists 繝｡繧ｽ繝・ラ蟄伜惠遒ｺ隱・| code | contains | "public static ExpectCheckResult CheckFileNotExists" | [x] |
| 3 | FileExists 繝励Ο繝代ユ繧｣霑ｽ蜉遒ｺ隱・| code | contains | "public List<string> FileExists" | [x] |
| 4 | FileNotExists 繝励Ο繝代ユ繧｣霑ｽ蜉遒ｺ隱・| code | contains | "public List<string> FileNotExists" | [x] |
| 5 | 繝薙Ν繝画・蜉・| build | succeeds | - | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | KojoExpectValidator: CheckFileExists 繝｡繧ｽ繝・ラ霑ｽ蜉 | [x] |
| 2 | 2 | KojoExpectValidator: CheckFileNotExists 繝｡繧ｽ繝・ラ霑ｽ蜉 | [x] |
| 3 | 3 | KojoTestExpect: FileExists 繝励Ο繝代ユ繧｣霑ｽ蜉 | [x] |
| 4 | 4 | KojoTestExpect: FileNotExists 繝励Ο繝代ユ繧｣霑ｽ蜉 | [x] |
| 5 | 1,2 | Validate() 繝｡繧ｽ繝・ラ縺ｫ file_exists/file_not_exists 蜃ｦ逅・ｿｽ蜉 | [x] |
| 6 | 5 | 繝薙Ν繝臥｢ｺ隱・| [x] |

## Design

### KojoExpectValidator.cs 霑ｽ蜉

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

### KojoTestExpect.cs 霑ｽ蜉

```csharp
[JsonPropertyName("file_exists")]
public List<string> FileExists { get; set; }

[JsonPropertyName("file_not_exists")]
public List<string> FileNotExists { get; set; }
```

### Validate() 繝｡繧ｽ繝・ラ霑ｽ蜉

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

- [feature-161.md](feature-161.md) - Folder Structure (萓晏ｭ伜・)
- [feature-162.md](feature-162.md) - Migration (萓晏ｭ伜・)
