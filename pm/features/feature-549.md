# Feature 549: YamlDialogueLoader Implementation

## Status: [DONE]

> **Blocked by**: IDialogueLoader Interface Extraction (F542)

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: engine

## Created: 2026-01-18

---

## Summary

Implement `YamlDialogueLoader` concrete class for IDialogueLoader interface to enable YAML dialogue file loading. This implementation provides the Loading responsibility extracted from KojoEngine monolith.

**Output**: `Era.Core/Dialogue/Loading/YamlDialogueLoader.cs`

**Scope**: Single file (~150 lines) implementing IDialogueLoader with YAML parsing via YamlDotNet.

---

## Background

### Philosophy (Mid-term Vision)

Phase 18: KojoEngine SRP分割 - Concrete implementations of extracted interfaces enable independent testing, DI injection, and future extensibility. YamlDialogueLoader establishes single source of truth for all YAML dialogue file loading operations.

### Problem (Current Issue)

IDialogueLoader interface (F542) requires concrete implementation for YAML file loading. Current KojoEngine has file loading logic tightly coupled with evaluation and rendering, preventing independent testing and reuse.

**Investigation Note**: Current DialogueEntry type (F542) provides Id/Content properties sufficient for basic YAML dialogue loading. F552 (PriorityDialogueSelector) will define selection requirements separately.

### Goal (What to Achieve)

Create YamlDialogueLoader class with:
- Load(string path) method returning Result<DialogueFile>
- LoadAll(string directory) method for batch loading
- YAML parsing error handling with descriptive Result.Fail messages
- Zero technical debt (no TODO/FIXME/HACK markers)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | YamlDialogueLoader.cs exists | file | Glob | exists | "Era.Core/Dialogue/Loading/YamlDialogueLoader.cs" | [x] |
| 2 | Implements IDialogueLoader | code | Grep | contains | "class YamlDialogueLoader.*IDialogueLoader" | [x] |
| 3 | Load method signature | code | Grep | contains | "Result<DialogueFile> Load\\(string path\\)" | [x] |
| 4 | LoadAll method signature | code | Grep | contains | "Result<IReadOnlyList<DialogueFile>> LoadAll\\(string directory\\)" | [x] |
| 5 | YAML parsing using YamlDotNet | code | Grep | contains | "YamlDotNet" | [x] |
| 6 | Load error handling | code | Grep | contains | "Result<DialogueFile>.Fail" | [x] |
| 7 | LoadAll error handling | code | Grep | contains | "Result<IReadOnlyList<DialogueFile>>.Fail" | [x] |
| 8 | Null validation | code | Grep | contains | "ArgumentException" | [x] |
| 9 | Unit tests exist | file | Glob | exists | "Era.Core.Tests/Dialogue/Loading/YamlDialogueLoaderTests.cs" | [x] |
| 10 | Unit tests PASS | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~YamlDialogueLoaderTests" | [x] |
| 11 | Zero technical debt | code | Grep | not_contains | "(TODO|FIXME|HACK)" | [x] |

### AC Details

**AC#1**: File existence verification
- Test: Glob pattern="Era.Core/Dialogue/Loading/YamlDialogueLoader.cs"
- Expected: File exists

**AC#2**: Interface implementation
- Test: Grep pattern="class YamlDialogueLoader.*IDialogueLoader" path="Era.Core/Dialogue/Loading/YamlDialogueLoader.cs" type=cs
- Expected: Class declaration implements IDialogueLoader

**AC#3**: Load method signature
- Test: Grep pattern="Result<DialogueFile> Load\\(string path\\)" path="Era.Core/Dialogue/Loading/YamlDialogueLoader.cs" type=cs
- Expected: Matches IDialogueLoader.Load signature

**AC#4**: LoadAll method signature
- Test: Grep pattern="Result<IReadOnlyList<DialogueFile>> LoadAll\\(string directory\\)" path="Era.Core/Dialogue/Loading/YamlDialogueLoader.cs" type=cs
- Expected: Matches IDialogueLoader.LoadAll signature

**AC#5**: YAML parsing using YamlDotNet
- Test: Grep pattern="YamlDotNet" path="Era.Core/Dialogue/Loading/YamlDialogueLoader.cs" type=cs
- Expected: Uses YamlDotNet library for YAML parsing

**AC#6**: Load error handling
- Test: Grep pattern="Result<DialogueFile>.Fail" path="Era.Core/Dialogue/Loading/YamlDialogueLoader.cs" type=cs
- Expected: Uses Result pattern for error handling (file not found, parse error)

**AC#7**: LoadAll error handling
- Test: Grep pattern="Result<IReadOnlyList<DialogueFile>>.Fail" path="Era.Core/Dialogue/Loading/YamlDialogueLoader.cs" type=cs
- Expected: Uses Result pattern for batch loading errors

**AC#8**: Null validation
- Test: Grep pattern="ArgumentException" path="Era.Core/Dialogue/Loading/YamlDialogueLoader.cs" type=cs
- Expected: Validates path/directory parameters by throwing ArgumentException for null or empty values

**AC#9**: Unit tests exist
- Test: Glob pattern="Era.Core.Tests/Dialogue/Loading/YamlDialogueLoaderTests.cs"
- Expected: Test file exists

**AC#10**: Unit tests PASS
- Test: `dotnet test --filter FullyQualifiedName~YamlDialogueLoaderTests`
- Expected: All tests pass (Load success, Load failure, LoadAll success, LoadAll failure, null argument)
- Test naming convention: Test methods follow `Test{MethodName}{Scenario}` format (e.g., `TestLoadSuccess`, `TestLoadFileNotFound`, `TestLoadAllSuccess`)

**AC#11**: Zero technical debt
- Test: Grep pattern="(TODO|FIXME|HACK)" path="Era.Core/Dialogue/Loading/YamlDialogueLoader.cs" type=cs
- Expected: 0 matches (no debt markers introduced during implementation)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create YamlDialogueLoader.cs file | [x] |
| 2 | 2 | Implement IDialogueLoader interface | [x] |
| 3 | 3 | Implement Load method signature | [x] |
| 4 | 4 | Implement LoadAll method signature | [x] |
| 5 | 5 | Add YamlDotNet usage | [x] |
| 6 | 6 | Implement Load error handling | [x] |
| 7 | 7 | Implement LoadAll error handling | [x] |
| 8 | 8 | Add null validation | [x] |
| 9 | 9 | Create unit test file | [x] |
| 10 | 10 | Ensure unit tests PASS | [x] |
| 11 | 11 | Verify no TODO/FIXME/HACK comments introduced | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Interface Reference

From F542 (IDialogueLoader Interface Extraction):

```csharp
// Era.Core/Dialogue/Loading/IDialogueLoader.cs
using Era.Core.Types;

namespace Era.Core.Dialogue.Loading;

public interface IDialogueLoader
{
    /// <summary>Load dialogue file from specified path</summary>
    Result<DialogueFile> Load(string path);

    /// <summary>Load all dialogue files from specified directory</summary>
    Result<IReadOnlyList<DialogueFile>> LoadAll(string directory);
}
```

### YAML Schema Reference

Expected YAML structure for DialogueFile deserialization (current implementation):

```yaml
# dialogue-example.yaml
entries:
  - id: "greeting"
    content: "こんにちは"
  - id: "farewell"
    content: "また今度"
```

**Note**: F549 defines a NEW simplified YAML dialogue format (id/content) incompatible with existing kojo YAML files (condition/lines). This creates a parallel dialogue loading system for Phase 18 SRP split. Priority and condition properties are not included in current DialogueEntry type.

### Implementation Template

```csharp
// Era.Core/Dialogue/Loading/YamlDialogueLoader.cs
using Era.Core.Types;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Era.Core.Dialogue.Loading;

// Helper class for YAML deserialization
internal class DialogueFileData
{
    public required IReadOnlyList<DialogueEntry> Entries { get; init; }
}

public class YamlDialogueLoader : IDialogueLoader
{
    private readonly IDeserializer _deserializer;

    public YamlDialogueLoader()
    {
        // Using CamelCaseNamingConvention for new simplified dialogue format
        // (differs from KojoEngine's UnderscoredNamingConvention by design)
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public Result<DialogueFile> Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        if (!File.Exists(path))
            return Result<DialogueFile>.Fail($"YamlDialogueLoader: File not found: {path}");

        try
        {
            var yaml = File.ReadAllText(path);
            var fileData = _deserializer.Deserialize<DialogueFileData>(yaml);
            var file = new DialogueFile
            {
                Entries = fileData.Entries,
                FilePath = path
            };
            return Result<DialogueFile>.Ok(file);
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            return Result<DialogueFile>.Fail($"YAML parse error in {path}: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<DialogueFile>.Fail($"YamlDialogueLoader: {ex.Message}");
        }
    }

    public Result<IReadOnlyList<DialogueFile>> LoadAll(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Directory cannot be null or empty", nameof(directory));

        if (!Directory.Exists(directory))
            return Result<IReadOnlyList<DialogueFile>>.Fail($"YamlDialogueLoader: Directory not found: {directory}");

        try
        {
            var files = Directory.GetFiles(directory, "*.yaml");
            var results = new List<DialogueFile>();

            foreach (var file in files)
            {
                var loadResult = Load(file);
                if (loadResult is Result<DialogueFile>.Failure failure)
                    return Result<IReadOnlyList<DialogueFile>>.Fail(failure.Error);

                var dialogueFile = ((Result<DialogueFile>.Success)loadResult).Value;
                results.Add(dialogueFile);
            }

            return Result<IReadOnlyList<DialogueFile>>.Ok(results);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<DialogueFile>>.Fail($"YamlDialogueLoader: {ex.Message}");
        }
    }
}
```

### Error Message Format

| Error Type | Format |
|------------|--------|
| File not found | `"YamlDialogueLoader: File not found: {path}"` |
| YAML parse error | `"YAML parse error in {path}: {exception.Message}"` |
| Directory not found | `"YamlDialogueLoader: Directory not found: {directory}"` |

### Test Cases (Minimum)

| Test Name | Scenario | Expected Result |
|-----------|----------|-----------------|
| TestLoadSuccess | Valid YAML file | Result.Success with DialogueFile |
| TestLoadFileNotFound | Non-existent file | Result.Fail with "file not found" |
| TestLoadInvalidYaml | Malformed YAML | Result.Fail with "parse error" |
| TestLoadNullPath | Null path argument | ArgumentException |
| TestLoadAllSuccess | Directory with 3 YAML files | Result.Success with 3 DialogueFiles |
| TestLoadAllEmptyDirectory | Empty directory | Result.Success with empty list |
| TestLoadAllDirectoryNotFound | Non-existent directory | Result.Fail with "YamlDialogueLoader: directory not found" |
| TestLoadAllNullDirectory | Null directory argument | ArgumentException |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| None | - | - | - |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | feature creation | PENDING |
| 2026-01-26 08:31 | START | implementer | Tasks 1-8 | - |
| 2026-01-26 08:31 | END | implementer | Tasks 1-8 | SUCCESS |
| 2026-01-26 | DEVIATION | Bash | dotnet test YamlDialogueLoaderTests | exit code 1, 3 tests failed |
| 2026-01-26 | DEBUG | debugger | fix YamlDotNet deserialization | SUCCESS, 10/10 tests pass |

## Dependencies

| Type | ID | Description | Status |
|------|----|-------------|:------:|
| Predecessor | F542 | IDialogueLoader Interface Extraction | [DONE] |
| Related | F550 | ConditionEvaluator Implementation | - |
| Related | F551 | TemplateDialogueRenderer Implementation | - |
| Related | F552 | PriorityDialogueSelector Implementation | - |
| Successor | F553 | KojoEngine Facade Refactoring | - |

## Links

- [index-features.md](index-features.md)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 18 section lines 3917-4037
- [F541: Phase 18 Planning](feature-541.md)
- [F542: IDialogueLoader Interface Extraction](feature-542.md)
- [F550: ConditionEvaluator Implementation](feature-550.md)
- [F551: TemplateDialogueRenderer Implementation](feature-551.md)
- [F552: PriorityDialogueSelector Implementation](feature-552.md)
- [F553: KojoEngine Facade Refactoring](feature-553.md)
- [feature-template.md](reference/feature-template.md)
