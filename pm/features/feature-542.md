# Feature 542: IDialogueLoader Interface Extraction

## Status: [DONE]


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

## Background

### Philosophy (Mid-term Vision)

Phase 18: KojoEngine SRP分割 - Splitting KojoEngine monolith into single-responsibility components following SOLID principles. This establishes IDialogueLoader interface for dialogue file loading operations, enabling future implementation with consistent YAML loading behavior and simplified maintenance. SSOT enforcement will be handled by implementation features (F549 YamlDialogueLoader).

### Problem (Current Issue)

KojoEngine (391-line monolith) violates SRP by combining loading, evaluation, rendering, and selection responsibilities. Loading responsibility (YAML file I/O) needs extraction into dedicated interface for:
- Testability: Mock loading during unit tests
- Flexibility: Support alternative formats (JSON, XML) via strategy pattern
- Maintainability: Isolate file I/O concerns from business logic

### Goal (What to Achieve)

Extract IDialogueLoader interface defining loading contract:
- Load single dialogue file from path
- Load all dialogue files from directory
- Result&lt;T&gt; return types for error handling
- Establish foundation for YamlDialogueLoader implementation (F549)

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IDialogueLoader.cs exists | file | Glob | exists | "Era.Core/Dialogue/Loading/IDialogueLoader.cs" | [x] |
| 2 | Interface is public | code | Grep | contains | "public interface IDialogueLoader" | [x] |
| 3 | Load method signature | code | Grep | contains | "Result<DialogueFile> Load\\(string path\\)" | [x] |
| 4 | LoadAll method signature | code | Grep | contains | "Result<IReadOnlyList<DialogueFile>> LoadAll\\(string directory\\)" | [x] |
| 5 | XML documentation on interface | code | Grep | contains | "Dialogue.*loader" | [x] |
| 6 | XML documentation on Load | code | Grep | contains | "Load.*dialogue file" | [x] |
| 7 | XML documentation on LoadAll | code | Grep | contains | "Load.*all.*dialogue files" | [x] |
| 8 | Namespace declaration | code | Grep | contains | "namespace Era.Core.Dialogue.Loading" | [x] |
| 9 | Using statements | code | Grep | contains | "using Era.Core.Types" | [x] |
| 10 | Zero technical debt | code | Grep | not_contains | "TODO\\|FIXME\\|HACK" | [x] |
| 11 | Tests PASS | test | Bash | succeeds | "dotnet test" | [x] |
| 12 | DialogueFile type exists | file | Glob | exists | "Era.Core/Dialogue/DialogueFile.cs" | [x] |

### AC Details

**AC#1**: File existence verification
- Test: `Glob("Era.Core/Dialogue/Loading/IDialogueLoader.cs")`
- Expected: File exists

**AC#2**: Interface accessibility
- Test: `Grep("public interface IDialogueLoader", "Era.Core/Dialogue/Loading/IDialogueLoader.cs")`
- Expected: Interface is public for external use

**AC#3**: Load method signature
- Test: `Grep("Result<DialogueFile> Load\\(string path\\)", "Era.Core/Dialogue/Loading/IDialogueLoader.cs")`
- Expected: Method returns Result<DialogueFile> with string path parameter

**AC#4**: LoadAll method signature
- Test: `Grep("Result<IReadOnlyList<DialogueFile>> LoadAll\\(string directory\\)", "Era.Core/Dialogue/Loading/IDialogueLoader.cs")`
- Expected: Method returns Result<IReadOnlyList<DialogueFile>> with string directory parameter

**AC#5**: XML documentation on interface
- Test: `Grep("/// <summary>.*dialogue.*loader", "Era.Core/Dialogue/Loading/IDialogueLoader.cs")`
- Expected: Interface has summary comment describing dialogue loading responsibility (case-insensitive pattern)

**AC#6**: XML documentation on Load
- Test: `Grep("/// <summary>.*Load.*dialogue file", "Era.Core/Dialogue/Loading/IDialogueLoader.cs")`
- Expected: Load method has summary describing single file loading

**AC#7**: XML documentation on LoadAll
- Test: `Grep("/// <summary>.*Load.*all.*dialogue files", "Era.Core/Dialogue/Loading/IDialogueLoader.cs")`
- Expected: LoadAll method has summary describing directory loading

**AC#8**: Namespace declaration
- Test: `Grep("namespace Era.Core.Dialogue.Loading", "Era.Core/Dialogue/Loading/IDialogueLoader.cs")`
- Expected: Namespace matches directory structure per ENGINE.md Issue 21

**AC#9**: Using statements
- Test: `Grep("using Era.Core.Types", "Era.Core/Dialogue/Loading/IDialogueLoader.cs")`
- Expected: Result<T> type requires Era.Core.Types namespace

**AC#10**: Zero technical debt
- Test: `Grep("TODO\\|FIXME\\|HACK", "Era.Core/Dialogue/Loading/IDialogueLoader.cs")`
- Expected: 0 matches (no technical debt in new interface)

**AC#11**: Tests PASS
- Test: `dotnet test`
- Expected: All existing tests continue to pass (no regression)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create IDialogueLoader.cs file in Era.Core/Dialogue/Loading/ | [x] |
| 2 | 2 | Define public IDialogueLoader interface declaration | [x] |
| 3 | 3 | Add Load method signature to interface | [x] |
| 4 | 4 | Add LoadAll method signature to interface | [x] |
| 5 | 8 | Add namespace declaration to interface | [x] |
| 6 | 9 | Add using statements to interface | [x] |
| 7 | 5 | Add XML documentation to interface | [x] |
| 8 | 6 | Add XML documentation to Load method | [x] |
| 9 | 7 | Add XML documentation to LoadAll method | [x] |
| 10 | 10 | Remove any TODO/FIXME/HACK comments (負債解消) | [x] |
| 11 | 11 | Verify all tests PASS | [x] |
| 12 | 12 | Create DialogueFile type definition | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Interface Specification

**Note**: DialogueFile type must be defined before implementing this interface (see Task#10 and AC#12).

Per architecture.md lines 3920-3925:

```csharp
// Era.Core/Dialogue/Loading/IDialogueLoader.cs
using Era.Core.Types;

namespace Era.Core.Dialogue.Loading;

/// <summary>
/// Dialogue file loading interface following SRP principle.
/// Responsible only for file I/O operations, not evaluation or rendering.
/// </summary>
public interface IDialogueLoader
{
    /// <summary>
    /// Load a single dialogue file from the specified path.
    /// </summary>
    /// <param name="path">File path to YAML dialogue file</param>
    /// <returns>Result containing DialogueFile on success, or error message on failure</returns>
    Result<DialogueFile> Load(string path);

    /// <summary>
    /// Load all dialogue files from the specified directory.
    /// </summary>
    /// <param name="directory">Directory path containing dialogue files</param>
    /// <returns>Result containing list of DialogueFile instances on success, or error message on failure</returns>
    Result<IReadOnlyList<DialogueFile>> LoadAll(string directory);
}
```

### DialogueFile Type Specification

Per Task#12 requirement for dialogue file representation:

```csharp
// Era.Core/Dialogue/DialogueFile.cs
using Era.Core.Types;

namespace Era.Core.Dialogue;

/// <summary>
/// Represents a loaded dialogue file with its entries and metadata.
/// </summary>
public record DialogueFile
{
    /// <summary>
    /// Collection of dialogue entries from the file.
    /// </summary>
    public required IReadOnlyList<DialogueEntry> Entries { get; init; }

    /// <summary>
    /// Original file path for reference.
    /// </summary>
    public required string FilePath { get; init; }
}

/// <summary>
/// Individual dialogue entry within a file.
/// </summary>
public record DialogueEntry
{
    /// <summary>
    /// Unique identifier for this dialogue entry.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Dialogue content/text.
    /// </summary>
    public required string Content { get; init; }
}
```

### Error Handling Strategy

Per ENGINE.md Issue 2:

| Scenario | Return Type | Example |
|----------|-------------|---------|
| File not found | `Result.Fail()` | `Result<DialogueFile>.Fail("File not found: {path}")` |
| Invalid YAML | `Result.Fail()` | `Result<DialogueFile>.Fail("YAML parse error: {details}")` |
| Directory not found | `Result.Fail()` | `Result<IReadOnlyList<DialogueFile>>.Fail("Directory not found: {directory}")` |
| Empty directory | `Result.Ok()` | `Result<IReadOnlyList<DialogueFile>>.Ok(new List<DialogueFile>())` |

**Null Arguments**: Throw `ArgumentNullException` (programmer error, not recoverable).

### Design Rationale

**Why Interface-First?**
- Enables YamlDialogueLoader implementation (F549) without changing consumers
- Supports future alternative loaders (JsonDialogueLoader, DatabaseLoader)
- Facilitates unit testing with mock loaders

**Why Result&lt;T&gt; Pattern?**
- Explicit error handling without exceptions (performance)
- Forces callers to handle loading failures
- Consistent with F468 Result&lt;T&gt; pattern

**Why IReadOnlyList&lt;DialogueFile&gt;?**
- Prevents modification of loaded collection
- Signals immutability contract to consumers

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- [user-skipped] Phase1-Uncertain iter2: DialogueFile type handoff lacks Creation Task in Tasks table - Option A requires actionable Task per template
- [user-applied] Phase1-External iter5: F549 blocked-by message references F540 but should reference F542
- [user-applied] Phase1-Uncertain iter5: AC#3 HTML entity vs literal character inconsistency (minor styling issue)

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| DialogueFile type definition | Interface references DialogueFile but type may not exist yet | Task | Task#12 |
| YamlDialogueLoader implementation | Interface extraction does not include implementation | Feature | F549 |
| DI registration | Registering IDialogueLoader with container is out of scope | Feature | F553 |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | feature creation | PENDING |
| 2026-01-26 06:47 | START | implementer | Task 1-12 | - |
| 2026-01-26 06:47 | END | implementer | Task 1-12 | SUCCESS |
| 2026-01-26 07:00 | DEVIATION | feature-reviewer | post-review | NEEDS_REVISION: IDialogueLoader not in engine-dev SKILL |
| 2026-01-26 07:05 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION: DialogueEntry spec vs impl mismatch |

---

## Dependencies

| Type | ID | Description | Status |
|------|----|-------------|:------:|
| Predecessor | F625 | Post-Phase Review Phase 17 (Data Migration) | [DONE] |
| Successor | F549 | YamlDialogueLoader Implementation | - |

---

## Links

- [index-features.md](index-features.md)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md#phase-18-kojoengine-srp分割) - Phase 18 specification (lines 3890-4064)
- [feature-541.md](feature-541.md) - Phase 18 Planning
- [feature-template.md](reference/feature-template.md)
