# Feature 552: PriorityDialogueSelector Implementation

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

## Summary

Implement `PriorityDialogueSelector` concrete class for IDialogueSelector interface to enable priority-based dialogue selection from candidate entries. This implementation provides the Selection responsibility extracted from KojoEngine monolith.

**Output**: `Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs`

**Scope**: Single file (~150 lines) implementing IDialogueSelector with priority-based selection and context-based filtering.

---

## Background

### Philosophy (Mid-term Vision)

Phase 18: KojoEngine SRP分割 - Concrete implementations of extracted interfaces enable independent testing, DI injection, and future extensibility. PriorityDialogueSelector establishes single source of truth for all dialogue selection operations with priority ordering and condition filtering.

### Problem (Current Issue)

IDialogueSelector interface (F545) requires concrete implementation for dialogue selection. Current KojoEngine has selection logic tightly coupled with loading and rendering, preventing independent testing and reuse. Priority-based selection (high-specificity conditions first, generic fallbacks last) is hard-coded rather than modular.

### Goal (What to Achieve)

Create PriorityDialogueSelector class with:
- Select(IReadOnlyList<DialogueEntry>, IEvaluationContext) method returning Result<DialogueEntry>
- Priority ordering (highest priority first)
- Context-based filtering using IConditionEvaluator
- Fallback handling when no entries match conditions
- Zero technical debt (no TODO/FIXME/HACK markers)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | DialogueEntry has Priority property | code | Grep(Era.Core/Dialogue/DialogueFile.cs) | contains | "public.*int Priority" | [x] |
| 2 | DialogueEntry has Condition property | code | Grep(Era.Core/Dialogue/DialogueFile.cs) | contains | "DialogueCondition.*Condition" | [x] |
| 3 | PriorityDialogueSelector.cs exists | file | Glob | exists | "Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs" | [x] |
| 4 | Implements IDialogueSelector | code | Grep(Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs) | contains | "class PriorityDialogueSelector.*IDialogueSelector" | [x] |
| 5 | Select method signature | code | Grep(Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs) | contains | "Result<DialogueEntry> Select" | [x] |
| 6 | Priority ordering | code | Grep(Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs) | contains | "OrderByDescending.*Priority" | [x] |
| 7 | Condition filtering | code | Grep(Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs) | contains | "IConditionEvaluator" | [x] |
| 8 | No match handling | code | Grep(Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs) | contains | "Result<DialogueEntry>.Fail" | [x] |
| 9 | Null validation | code | Grep(Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs) | contains | "ArgumentNullException" | [x] |
| 10 | Unit tests exist | file | Glob | exists | "Era.Core.Tests/Dialogue/Selection/PriorityDialogueSelectorTests.cs" | [x] |
| 11 | Priority selection test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~PriorityDialogueSelectorTests.TestSelectHighestPriority" | [x] |
| 12 | Condition filtering test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~PriorityDialogueSelectorTests.TestSelectWithCondition" | [x] |
| 13 | Zero technical debt | code | Grep(Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs) | not_contains | "TODO" | [x] |
| 13.1 | Zero technical debt (FIXME) | code | Grep(Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs) | not_contains | "FIXME" | [x] |
| 13.2 | Zero technical debt (HACK) | code | Grep(Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs) | not_contains | "HACK" | [x] |
| 14 | DI registration | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | contains | "AddSingleton.*IDialogueSelector.*PriorityDialogueSelector" | [x] |
| 15 | No match test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~PriorityDialogueSelectorTests.TestSelectNoMatch" | [x] |
| 16 | Null entries test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~PriorityDialogueSelectorTests.TestSelectNullEntries" | [x] |
| 17 | Null context test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~PriorityDialogueSelectorTests.TestSelectNullContext" | [x] |

### AC Details

**AC#1**: DialogueEntry has Priority property
- Test: Grep pattern="public.*int Priority" path="Era.Core/Dialogue/DialogueFile.cs" type=cs
- Expected: DialogueEntry record has int Priority property
- **Prerequisite**: Task#0 must complete before this AC can pass (modifies DialogueFile.cs to add properties)

**AC#2**: DialogueEntry has Condition property
- Test: Grep pattern="DialogueCondition.*Condition" path="Era.Core/Dialogue/DialogueFile.cs" type=cs
- Expected: DialogueEntry record has DialogueCondition? Condition property
- **Prerequisite**: Task#0 must complete before this AC can pass

**AC#3**: File existence verification
- Test: Glob pattern="Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs"
- Expected: File exists

**AC#4**: Interface implementation
- Test: Grep pattern="class PriorityDialogueSelector.*IDialogueSelector" path="Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs" type=cs
- Expected: Class declaration implements IDialogueSelector

**AC#5**: Select method signature
- Test: Grep pattern="Result<DialogueEntry> Select" path="Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs" type=cs
- Expected: Matches IDialogueSelector.Select signature

**AC#6**: Priority ordering
- Test: Grep pattern="OrderByDescending.*Priority" path="Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs" type=cs
- Expected: Sorts entries by priority (descending)

**AC#7**: Condition filtering
- Test: Grep pattern="IConditionEvaluator" path="Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs" type=cs
- Expected: Uses IConditionEvaluator to filter entries by conditions

**AC#8**: No match handling
- Test: Grep pattern="Result<DialogueEntry>.Fail" path="Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs" type=cs
- Expected: Uses Result pattern for error handling (no matching entries)

**AC#9**: Null validation
- Test: Grep pattern="ArgumentNullException" path="Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs" type=cs
- Expected: Validates entries/context parameters

**AC#10**: Unit tests exist
- Test: Glob pattern="Era.Core.Tests/Dialogue/Selection/PriorityDialogueSelectorTests.cs"
- Expected: Test file exists

**AC#11**: Priority selection test
- Test: `dotnet test --filter FullyQualifiedName~PriorityDialogueSelectorTests.TestSelectHighestPriority`
- Expected: Test verifies highest priority entry selected when multiple entries match conditions

**AC#12**: Condition filtering test
- Test: `dotnet test --filter FullyQualifiedName~PriorityDialogueSelectorTests.TestSelectWithCondition`
- Expected: Test verifies only entries satisfying conditions are considered

**AC#13**: Zero technical debt (TODO)
- Test: Grep pattern="TODO" path="Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs" type=cs
- Expected: 0 matches (no TODO markers)

**AC#13.1**: Zero technical debt (FIXME)
- Test: Grep pattern="FIXME" path="Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs" type=cs
- Expected: 0 matches (no FIXME markers)

**AC#13.2**: Zero technical debt (HACK)
- Test: Grep pattern="HACK" path="Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs" type=cs
- Expected: 0 matches (no HACK markers)

**AC#14**: DI registration
- Test: Grep pattern="AddSingleton.*IDialogueSelector.*PriorityDialogueSelector" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs" type=cs
- Expected: Service registered in DI container

**AC#15**: No match test
- Test: `dotnet test --filter FullyQualifiedName~PriorityDialogueSelectorTests.TestSelectNoMatch`
- Expected: Test verifies Result.Fail returned when no entries match conditions

**AC#16**: Null entries test
- Test: `dotnet test --filter FullyQualifiedName~PriorityDialogueSelectorTests.TestSelectNullEntries`
- Expected: Test verifies ArgumentNullException thrown for null entries parameter

**AC#17**: Null context test
- Test: `dotnet test --filter FullyQualifiedName~PriorityDialogueSelectorTests.TestSelectNullContext`
- Expected: Test verifies ArgumentNullException thrown for null context parameter

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | 1,2 | Update DialogueEntry record: add int Priority and DialogueCondition? Condition properties (requires using Era.Core.Dialogue.Conditions) | [x] |
| 1 | 3,4,5,6,7 | Implement PriorityDialogueSelector class with Select method, priority ordering, and condition filtering (Requires Task#0 completion first) | [x] |
| 2 | 8,9 | Add error handling with Result pattern and null validation | [x] |
| 3 | 10,11,12,15,16,17 | Create unit tests for priority selection, condition filtering, and error handling and verify all tests PASS | [x] |
| 4 | 13,13.1,13.2 | Remove any TODO/FIXME/HACK comments introduced during implementation | [x] |
| 5 | 14 | Register PriorityDialogueSelector in DI container | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->
<!-- BATCH WAIVER: Task#1 combines core class implementation (AC#3-7) due to atomicity requirement - class structure, interface, methods, and validation must be implemented together for proper functionality -->
<!-- BATCH WAIVER: Task#2 combines error handling patterns (AC#8,9) - Result.Fail for no match and ArgumentNullException for null parameters are related error handling concerns -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Interface Reference

From F545 (IDialogueSelector Interface Extraction):

```csharp
// Era.Core/Dialogue/Selection/IDialogueSelector.cs
using Era.Core.Types;

namespace Era.Core.Dialogue.Selection;

public interface IDialogueSelector
{
    /// <summary>Select best matching dialogue entry from candidates</summary>
    Result<DialogueEntry> Select(IReadOnlyList<DialogueEntry> entries, IEvaluationContext context);
}
```

### Implementation Template

```csharp
// Era.Core/Dialogue/Selection/PriorityDialogueSelector.cs
using Era.Core.Dialogue.Conditions;
using Era.Core.Dialogue.Evaluation;
using Era.Core.Functions;
using Era.Core.Types;

namespace Era.Core.Dialogue.Selection;

public class PriorityDialogueSelector : IDialogueSelector
{
    private readonly IConditionEvaluator _evaluator;

    public PriorityDialogueSelector(IConditionEvaluator evaluator)
    {
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
    }

    public Result<DialogueEntry> Select(IReadOnlyList<DialogueEntry> entries, IEvaluationContext context)
    {
        if (entries == null)
            throw new ArgumentNullException(nameof(entries));
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        // Filter entries matching conditions, then order by priority (descending)
        var candidates = entries
            .Where(e => e.Condition == null || _evaluator.Evaluate(e.Condition, context))
            .OrderByDescending(e => e.Priority)
            .ToList();

        if (candidates.Count == 0)
            return Result<DialogueEntry>.Fail("No dialogue entries match the current context");

        return Result<DialogueEntry>.Ok(candidates[0]);
    }
}
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IDialogueSelector, PriorityDialogueSelector>();
```

### Error Message Format

| Error Type | Format |
|------------|--------|
| No matching entries | `"No dialogue entries match the current context"` |

### Selection Algorithm

1. **Filter**: Keep only entries where:
   - Entry has no condition (unconditional entry), OR
   - Entry condition evaluates to true using IConditionEvaluator
2. **Sort**: Order remaining entries by Priority (descending)
3. **Select**: Return first entry (highest priority)
4. **Fallback**: If no entries remain after filtering, return Result.Fail

### Test Cases (Minimum)

| Test Name | Scenario | Expected Result |
|-----------|----------|-----------------|
| TestSelectHighestPriority | 3 entries (priority 10, 20, 15), all match conditions | Returns entry with priority 20 |
| TestSelectWithCondition | 2 entries, only 1 satisfies condition | Returns matching entry |
| TestSelectUnconditionalFallback | 2 entries: conditional (fails) + unconditional (priority 0) | Returns unconditional entry |
| TestSelectNoMatch | 2 entries with conditions, neither satisfied | Result.Fail with "No dialogue entries match" |
| TestSelectNullEntries | Null entries argument | ArgumentNullException |
| TestSelectNullContext | Null context argument | ArgumentNullException |

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| DialogueEntry extension to support priority/condition - modifies DialogueFile.cs record (Era.Core/Dialogue/DialogueFile.cs) | Task#0 directly modifies F549's DialogueEntry.cs output file to add Priority/Condition properties | F552#T0 | T0 |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | feature creation | PENDING |
| 2026-01-26 15:15 | START | implementer | Task#3 (Phase 3 TDD) | - |
| 2026-01-26 15:15 | END | implementer | Task#3 (Phase 3 TDD) | SUCCESS |
| 2026-01-26 15:17 | START | implementer | Task#0 | - |
| 2026-01-26 15:17 | END | implementer | Task#0 | SUCCESS |
| 2026-01-26 15:19 | START | implementer | Task#1 | - |
| 2026-01-26 15:19 | END | implementer | Task#1 | SUCCESS |
| 2026-01-26 15:19 | START | implementer | Task#5 (DI registration) | - |
| 2026-01-26 15:19 | END | implementer | Task#5 (DI registration) | SUCCESS |

## Dependencies

| Type | ID | Description | Status |
|------|----|-------------|:------:|
| Predecessor | F545 | IDialogueSelector Interface Extraction | [DONE] |
| Predecessor | F550 | ConditionEvaluator Implementation | [DONE] |
| Related | F549 | YamlDialogueLoader Implementation (DialogueEntry extended by F552#T0) | [DONE] |
| Related | F551 | TemplateDialogueRenderer Implementation | - |
| Successor | F553 | KojoEngine Facade Refactoring | - |

## Links

- [index-features.md](index-features.md)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 18 section lines 3917-4037
- [F541: Phase 18 Planning](feature-541.md)
- [F545: IDialogueSelector Interface Extraction](feature-545.md)
- [F549: YamlDialogueLoader Implementation](feature-549.md)
- [F550: ConditionEvaluator Implementation](feature-550.md)
- [F551: TemplateDialogueRenderer Implementation](feature-551.md)
- [F553: KojoEngine Facade Refactoring](feature-553.md)
- [feature-template.md](reference/feature-template.md)

## Review Notes
- DialogueEntry property extension (Task#0) is prerequisite for all other ACs to succeed
- [resolved-applied] Phase1 iter4: AC#7 pattern changed to safer `IConditionEvaluator` pattern per user decision
- [resolved] Phase1 iter6: Task#1 batch waiver (5 ACs) follows F550/F551 precedent - atomicity requirement is valid for single class implementation
