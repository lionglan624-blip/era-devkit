# Feature 604: SaveErrorMetrics Implementation

## Status: [DONE]

## Type: engine

## Background

### Philosophy (Mid-term Vision)
Error analytics data persistence enables long-term analysis and historical tracking of error patterns, supporting data-driven improvements to system reliability. Local persistence should be efficient, privacy-preserving, and resilient to application crashes during error collection.

### Problem (Current Issue)
F597 Error Analytics and Telemetry provides in-memory error collection but lacks persistent storage. The SaveErrorMetrics method is deferred as a NotImplementedException, preventing long-term error analysis, trend identification, and recovery between application restarts.

### Goal (What to Achieve)
Implement SaveErrorMetrics method with JSON-based local file persistence, including file format design, crash-safe writing, data retention management, and error recovery for corrupted files.

---

## Scope Discipline

**明確にIN SCOPE**:
- SaveErrorMetrics method implementation in ErrorAnalyticsService
- JSON file format design for error metrics persistence
- Crash-safe atomic file writing operations
- Data retention management (file rotation/cleanup)
- Error recovery for corrupted persistence files
- Unit tests for file I/O operations

**明確にOUT OF SCOPE**:
- Remote transmission of persisted data (→ Future feature)
- Real-time file monitoring/watching (→ Future feature)
- Data compression or encryption (→ Future feature)
- Cross-platform path handling optimizations (→ Future feature)
- Database backend alternative (→ Future feature)
- Async file I/O (sync acceptable for expected small data volumes)
- Automatic file retention/cleanup (→ F605, this feature only provides manual save)

---

## Implementation Contract

### Interface Update Contract
```csharp
// Complete IErrorAnalyticsService interface modification
using System;
using System.Collections.Generic;
using Era.Core.Types;  // <-- Add this using directive

namespace MinorShift.Emuera.Services;

public interface IErrorAnalyticsService
{
    // ... existing methods ...

    /// <summary>Saves collected metrics to local file system</summary>
    /// <param name="filePath">Optional custom path. If null, uses default location</param>
    /// <returns>Result indicating success or failure reason</returns>
    Result<Unit> SaveErrorMetrics(string filePath = null);
}
```

### ErrorAnalyticsService Implementation
```csharp
using System.IO;
using System.Linq;
using System.Text.Json;
using Era.Core.Types;

public class ErrorAnalyticsService : IErrorAnalyticsService
{
    private readonly string _defaultMetricsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "uEmuera", "Metrics", "error-metrics.json");

    public ErrorAnalyticsService()
    {
        CleanupTempFiles();
    }

    // ... existing implementation ...

    public Result<Unit> SaveErrorMetrics(string filePath = null)
    {
        try
        {
            var targetPath = filePath ?? _defaultMetricsPath;
            EnsureDirectoryExists(Path.GetDirectoryName(targetPath));

            var collectedMetrics = GetCollectedMetrics().ToArray();
            var metricsData = new MetricsSnapshot(
                "1.0",
                DateTime.UtcNow,
                collectedMetrics.Length,
                collectedMetrics);

            // Atomic write operation
            var tempPath = targetPath + ".tmp";
            var json = JsonSerializer.Serialize(metricsData, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            File.WriteAllText(tempPath, json);
            File.Move(tempPath, targetPath, true);

            return Result<Unit>.Ok(Unit.Value);
        }
        catch (Exception ex)
        {
            // Log full exception for debugging (includes stack trace)
            System.Diagnostics.Debug.WriteLine($"SaveErrorMetrics failed: {ex}");
            return Result<Unit>.Fail($"Failed to save error metrics: {ex.Message}");
        }
    }

    private static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    // Crash recovery: Clean up orphaned .tmp files on service initialization
    private void CleanupTempFiles()
    {
        var dir = Path.GetDirectoryName(_defaultMetricsPath);
        if (Directory.Exists(dir))
        {
            foreach (var tmpFile in Directory.GetFiles(dir, "*.tmp"))
            {
                try { File.Delete(tmpFile); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"CleanupTempFiles: Failed to delete {tmpFile}: {ex.Message}"); }
            }
        }
    }
}
```

### MetricsSnapshot Record (Separate File)
```csharp
// engine/Assets/Scripts/Emuera/Services/MetricsSnapshot.cs
namespace MinorShift.Emuera.Services;

/// <summary>Root object for error metrics JSON serialization</summary>
/// <remarks>Separate file for reuse by F606 (visualization) and F607 (aggregation)</remarks>
public record MetricsSnapshot(
    string FormatVersion,
    DateTime GeneratedAt,
    int TotalErrors,
    ErrorMetric[] Metrics);
```

### JSON File Format Specification
```json
{
  "formatVersion": "1.0",
  "generatedAt": "2026-01-23T15:30:00.000Z",
  "totalErrors": 3,
  "metrics": [
    {
      "type": "FileNotFoundException",
      "message": "Could not find file '{anonymous}/config.ini'",
      "stackTrace": "at System.IO.File.ReadAllText...",
      "timestamp": "2026-01-23T15:25:00.000Z"
    }
  ]
}
```

### Test Implementation Contract
```csharp
using System.IO;
using Xunit;
using MinorShift.Emuera.Services;

namespace uEmuera.Tests
{
    public class SaveErrorMetricsTests
    {
        // Verify file created at default path with correct JSON structure
        [Fact]
        public void TestSaveErrorMetrics_WhenDefaultPath() { }

        // Verify file created at specified path
        [Fact]
        public void TestSaveErrorMetrics_WhenCustomPath() { }

        // Verify directory auto-created when parent path doesn't exist
        [Fact]
        public void TestSaveErrorMetrics_WhenDirectoryNotExists() { }

        // Verify Result.Fail returned on IOException
        [Fact]
        public void TestSaveErrorMetrics_WhenAtomicWriteFailure() { }

        // Negative test: Verify Result.Fail returned for invalid path
        [Fact]
        public void TestSaveErrorMetrics_WhenInvalidPath() { }

        // Negative test: Verify Result.Fail returned when permission denied
        [Fact]
        public void TestSaveErrorMetrics_WhenPermissionDenied() { }

        // Edge case: Verify behavior when no errors collected
        [Fact]
        public void TestSaveErrorMetrics_WhenEmptyMetrics() { }
    }
}
```

### Test Naming Convention
- Test class: `SaveErrorMetricsTests`
- Test file: `engine.Tests/Tests/SaveErrorMetricsTests.cs`
- Method pattern: `Test{MethodName}_{Condition}`
- Example: `TestSaveErrorMetrics_WhenDefaultPath()`

### File Path Defaults
- Default location: `%LOCALAPPDATA%\uEmuera\Metrics\error-metrics.json` (platform-specific separators used at runtime)
- Atomic write pattern: Write to `.tmp` extension, then rename
- Directory auto-creation if parent path doesn't exist

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SaveErrorMetrics method in interface | file | Grep(engine/Assets/Scripts/Emuera/Services/IErrorAnalyticsService.cs) | contains | "SaveErrorMetrics" | [x] |
| 2 | SaveErrorMetrics implementation | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | matches | "Result<Unit>.*SaveErrorMetrics" | [x] |
| 3 | Default path configuration | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "_defaultMetricsPath" | [x] |
| 4 | JSON serialization usage | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "JsonSerializer.Serialize" | [x] |
| 5 | Atomic write pattern | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | ".tmp" | [x] |
| 6 | Directory creation logic | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "EnsureDirectoryExists" | [x] |
| 7 | Result type return pattern | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | matches | "Result<Unit>\.Ok\(Unit\.Value\)" | [x] |
| 8 | Error handling with Result.Fail | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | matches | "Result<Unit>\.Fail.*Failed to save error metrics" | [x] |
| 9 | Unit tests execution | test | Bash | succeeds | "dotnet test engine.Tests --filter SaveErrorMetricsTests" | [x] |
| 10 | No TODO in interface | file | Grep(engine/Assets/Scripts/Emuera/Services/IErrorAnalyticsService.cs) | not_contains | "TODO" | [x] |
| 11 | No FIXME in interface | file | Grep(engine/Assets/Scripts/Emuera/Services/IErrorAnalyticsService.cs) | not_contains | "FIXME" | [x] |
| 12 | No HACK in interface | file | Grep(engine/Assets/Scripts/Emuera/Services/IErrorAnalyticsService.cs) | not_contains | "HACK" | [x] |
| 13 | No TODO in implementation | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | not_contains | "TODO" | [x] |
| 14 | No FIXME in implementation | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | not_contains | "FIXME" | [x] |
| 15 | No HACK in implementation | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | not_contains | "HACK" | [x] |
| 16 | Format version in JSON output | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "FormatVersion" | [x] |
| 17 | Generated timestamp in output | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "GeneratedAt" | [x] |
| 18 | Metrics count tracking | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "TotalErrors" | [x] |
| 19 | LocalApplicationData path usage | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "Environment.SpecialFolder.LocalApplicationData" | [x] |
| 20 | uEmuera subdirectory structure | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "uEmuera" | [x] |
| 21 | File.Move for atomic operation | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "File.Move" | [x] |
| 22 | Exception handling in try-catch | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "catch (Exception" | [x] |
| 23 | JsonSerializerOptions configuration | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "JsonSerializerOptions" | [x] |
| 24 | WriteIndented for readable output | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "WriteIndented = true" | [x] |
| 25 | Temp file cleanup implementation | file | Grep(engine/Assets/Scripts/Emuera/Services/ErrorAnalyticsService.cs) | contains | "CleanupTempFiles" | [x] |
| 26 | MetricsSnapshot record type | file | Grep(engine/Assets/Scripts/Emuera/Services/MetricsSnapshot.cs) | matches | "record MetricsSnapshot" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add SaveErrorMetrics method to IErrorAnalyticsService interface | [x] |
| 2 | 2,3,19,20 | Implement SaveErrorMetrics method with default path configuration | [x] |
| 3 | 4,16,17,18,23,24,26 | Add JSON serialization with format specification | [x] |
| 4 | 5,6,21 | Implement atomic write pattern with directory creation | [x] |
| 5 | 7,8,22 | Add Result<T> error handling with try-catch wrapper | [x] |
| 6 | 9 | Create comprehensive unit tests for file I/O operations | [x] |
| 7 | 25 | Implement temp file cleanup for crash recovery | [x] |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F597 | [DONE] | Error Analytics and Telemetry - base service implementation |

---

## Mandatory Handoffs

| Component | Destination | Description |
|-----------|-------------|-------------|
| Data retention management | F605 (create) | Automatic cleanup of old metrics files based on age/size limits |
| Error metrics visualization | F606 (create) | Read persisted JSON files for dashboard/reporting display |
| Metrics aggregation and reporting | F607 (create) | Analysis tools for error trends, frequency reports from saved data |
| Cross-platform path optimization | F609 (create) | Platform-specific optimizations for Linux/macOS file paths |

---

## Review Notes
- [resolved-invalid] Phase1-Uncertain iter3: AC#7 and AC#8 regex patterns with escaped characters - validated as correct Python regex
- [resolved-applied] Phase1-Uncertain iter3: Task 7 maps to verification ACs (not implementation tasks) - removed Task 7
- [resolved-invalid] Phase1-Uncertain iter4: AC#7 regex pattern with escaped backslashes - validated as correct Python regex
- [resolved-invalid] Phase1-Uncertain iter4: AC#8 regex pattern with escaped backslashes - validated as correct Python regex
- [resolved-applied] Phase1-Uncertain iter7: CleanupTempFiles method defined but initialization call not shown - added constructor call
- [resolved-invalid] Phase1-Uncertain iter7: No AC for PropertyNamingPolicy.CamelCase - implementation detail covered by AC#19
- [resolved-valid] Phase1-Uncertain iter8: AC#7 regex pattern verified as correct Python regex escaping
- [resolved-applied] Phase2-Maintainability iter10: Exception details swallowed in catch block - added Debug.WriteLine for full exception
- [resolved-applied] Phase2-Maintainability iter10: MetricsSnapshot tight coupling - separated to MetricsSnapshot.cs
- [resolved-applied] Phase2-Maintainability iter10: Empty catch in CleanupTempFiles - added Debug.WriteLine logging
- [resolved-skipped] Phase2-Maintainability iter10: Hardcoded path prevents testability - Skip (optional parameter SaveErrorMetrics(filePath) provides sufficient testability)
- [resolved-skipped] Phase2-Maintainability iter10: 26 ACs for 7 Tasks (3.7:1 ratio) - Skip (engine type implementation detail ACs are acceptable)
- [resolved-skipped] Phase2-Maintainability iter10: Task 6 test file existence not tracked by AC - Skip (AC#9 test execution implies file exists)
- [resolved-skipped] Phase3-ACValidation iter10: AC Type 'file' vs 'code' naming convention for Grep matchers - Skip (both work identically)
- [resolved-applied] Phase3-ACValidation iter10: AC#9 Type 'build' vs 'test' for dotnet test command - changed to 'test'

---

## Execution Log

| Timestamp | Event | Source | Action | Detail |
|-----------|-------|--------|--------|--------|
| 2026-01-24 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION: engine-dev SKILL.md missing SaveErrorMetrics documentation |
| 2026-01-24 | RESOLVED | implementer | SSOT update | Updated engine-dev SKILL.md with SaveErrorMetrics and MetricsSnapshot |

---

## Links
- [index-features.md](index-features.md)
- [feature-597.md](feature-597.md)
- [feature-605.md](feature-605.md)
- [feature-606.md](feature-606.md)
- [feature-607.md](feature-607.md)
- [feature-609.md](feature-609.md)