# Feature 545: IDialogueSelector Interface Extraction

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

Phase 18: KojoEngine SRP分割 - Splitting KojoEngine monolith into single-responsibility components following SOLID principles. This establishes IDialogueSelector as the single source of truth for all dialogue entry selection operations, ensuring consistent priority-based selection behavior and simplified future maintenance.

### Problem (Current Issue)

KojoEngine (391-line monolith) violates SRP by combining loading, evaluation, rendering, and selection responsibilities. Selection responsibility (choosing dialogue entry based on priority and conditions) needs extraction into dedicated interface for:
- Testability: Mock selection during unit tests
- Flexibility: Support alternative selection strategies (random, weighted, priority-based) via strategy pattern
- Maintainability: Isolate selection logic from file I/O and rendering

### Goal (What to Achieve)

Extract IDialogueSelector interface defining selection contract:
- Select single DialogueEntry from list based on IEvaluationContext
- Result&lt;DialogueEntry&gt; return type for error handling (no matching entry)
- Establish foundation for PriorityDialogueSelector implementation (F552)

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IDialogueSelector.cs exists | file | Glob | exists | "Era.Core/Dialogue/Selection/IDialogueSelector.cs" | [x] |
| 2 | Interface is public | code | Grep(Era.Core/Dialogue/Selection/IDialogueSelector.cs) | contains | "public interface IDialogueSelector" | [x] |
| 3 | Select method signature | code | Grep(Era.Core/Dialogue/Selection/IDialogueSelector.cs) | contains | "Result<DialogueEntry> Select" | [x] |
| 4 | XML documentation on interface | code | Grep(Era.Core/Dialogue/Selection/IDialogueSelector.cs) | contains | "Dialogue entry selection interface" | [x] |
| 5 | XML documentation on Select | code | Grep(Era.Core/Dialogue/Selection/IDialogueSelector.cs) | contains | "Select a dialogue entry" | [x] |
| 6 | Namespace declaration | code | Grep(Era.Core/Dialogue/Selection/IDialogueSelector.cs) | contains | "namespace Era.Core.Dialogue.Selection" | [x] |
| 7 | Using statements | code | Grep(Era.Core/Dialogue/Selection/IDialogueSelector.cs) | contains | "using Era.Core.Types" | [x] |
| 8 | Zero technical debt | code | Grep(Era.Core/Dialogue/Selection/IDialogueSelector.cs) | not_matches | "TODO\\|FIXME\\|HACK" | [x] |
| 9 | Tests PASS | test | Bash | succeeds | "dotnet test" | [x] |

### AC Details

**AC#1**: File existence verification
- Test: `Glob("Era.Core/Dialogue/Selection/IDialogueSelector.cs")`
- Expected: File exists

**AC#2**: Interface accessibility
- Test: `Grep("public interface IDialogueSelector", "Era.Core/Dialogue/Selection/IDialogueSelector.cs")`
- Expected: Interface is public for external use

**AC#3**: Select method signature
- Test: `Grep("Result<DialogueEntry> Select", "Era.Core/Dialogue/Selection/IDialogueSelector.cs")`
- Expected: Method returns Result&lt;DialogueEntry&gt; with appropriate signature (simplified pattern for flexibility)

**AC#4**: XML documentation on interface
- Test: `Grep("Dialogue entry selection interface", "Era.Core/Dialogue/Selection/IDialogueSelector.cs")`
- Expected: Interface has summary comment describing dialogue selection responsibility

**AC#5**: XML documentation on Select
- Test: `Grep("Select a dialogue entry", "Era.Core/Dialogue/Selection/IDialogueSelector.cs")`
- Expected: Select method has summary describing entry selection logic

**AC#6**: Namespace declaration
- Test: `Grep("namespace Era.Core.Dialogue.Selection", "Era.Core/Dialogue/Selection/IDialogueSelector.cs")`
- Expected: Namespace matches directory structure per ENGINE.md Issue 21

**AC#7**: Using statements
- Test: `Grep("using Era.Core.Types", "Era.Core/Dialogue/Selection/IDialogueSelector.cs")`
- Expected: Result&lt;T&gt; type requires Era.Core.Types namespace

**AC#8**: Zero technical debt
- Test: `Grep("TODO\\|FIXME\\|HACK", "Era.Core/Dialogue/Selection/IDialogueSelector.cs")` with not_matches matcher
- Expected: 0 matches (no technical debt in new interface)

**AC#9**: Tests PASS
- Test: `dotnet test`
- Expected: All existing tests continue to pass (no regression)

**Negative Test Coverage**: Engine type typically requires negative test ACs, but this feature extracts a pure interface without implementation logic. Error case testing (null parameters, empty collections) is deferred to F552 (PriorityDialogueSelector implementation).

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create Era.Core/Dialogue/Selection/ directory structure | [x] |
| 2 | 2 | Define public interface IDialogueSelector | [x] |
| 3 | 3 | Add Select method signature | [x] |
| 4 | 6 | Add namespace declaration | [x] |
| 5 | 7 | Add using statements | [x] |
| 6 | 4 | Add XML documentation to interface | [x] |
| 7 | 5 | Add XML documentation to Select method | [x] |
| 8 | 8 | Remove any TODO/FIXME/HACK comments (負債解消) | [x] |
| 9 | 9 | Verify all tests PASS | [x] |
| 10 | - | Create F627 for dialogue-specific IEvaluationContext | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->
<!-- Note: Task#10 is creation task for Handoff Option A, not bound by AC:Task 1:1 rule -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Interface Specification

Per architecture.md lines 3948-3952:

```csharp
// Era.Core/Dialogue/Selection/IDialogueSelector.cs
using Era.Core.Types;

namespace Era.Core.Dialogue.Selection;

/// <summary>
/// Dialogue entry selection interface following SRP principle.
/// Responsible only for selecting appropriate dialogue entry, not loading or rendering.
/// </summary>
public interface IDialogueSelector
{
    /// <summary>
    /// Select a dialogue entry from the provided list based on priority and conditions.
    /// </summary>
    /// <param name="entries">List of dialogue entries to select from</param>
    /// <param name="context">Evaluation context for condition checking</param>
    /// <returns>Result containing selected DialogueEntry on success, or error message if no entry matches</returns>
    Result<DialogueEntry> Select(IReadOnlyList<DialogueEntry> entries, IEvaluationContext context);
}
```

### Error Handling Strategy

Per ENGINE.md Issue 2:

| Scenario | Return Type | Example |
|----------|-------------|---------|
| No matching entry | `Result.Fail()` | `Result<DialogueEntry>.Fail("No dialogue entry matches current conditions")` |
| Empty entries list | `Result.Fail()` | `Result<DialogueEntry>.Fail("No dialogue entries available for selection")` |
| Multiple matches (priority tie) | `Result.Ok()` | Return highest priority entry (implementation detail in F552) |
| Single match | `Result.Ok()` | `Result<DialogueEntry>.Ok(selectedEntry)` |

**Null Arguments**: Throw `ArgumentNullException` (programmer error, not recoverable).

### Design Rationale

**Why Result&lt;DialogueEntry&gt; Return?**
- Selection can fail if no entry matches conditions (recoverable error)
- Forces callers to handle "no dialogue available" case
- Consistent with other dialogue interfaces (IDialogueLoader, IDialogueRenderer)

**Why IReadOnlyList&lt;DialogueEntry&gt; Parameter?**
- Prevents modification of input collection
- Signals immutability contract to implementers
- Efficient for iteration (no defensive copy needed)

**Why IEvaluationContext Parameter?**
- Provides state for condition evaluation (TALENT, ABL, EXP)
- Delegates condition checking to IConditionEvaluator (F543/F550)
- Supports selection based on character state

### Selection Strategy (Implementation in F552)

Expected priority-based selection behavior:

| Priority | Condition | Selected? | Notes |
|:--------:|-----------|:---------:|-------|
| 1 | TALENT:恋慕 >= 5 | Yes (if true) | Highest priority checked first |
| 2 | TALENT:恋慕 >= 3 | Yes (if no priority 1 match) | Fallback |
| 3 | (none) | Yes (if no higher match) | Default entry |

**Selection Algorithm** (documented for implementer in F552):
1. Filter entries by condition satisfaction (via IConditionEvaluator)
2. Sort remaining entries by priority (descending)
3. Return highest priority entry
4. If no entries match, return Result.Fail

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- [resolved-applied] Phase1 iter1: Blocked-by references F540 which was cancelled. F625 (replacement) is [DONE] - fix should target only the blockquote removal/update, not suggest changes to Dependencies table which is already correct
- [resolved-invalid] Phase1 iter1: IEvaluationContext type location ambiguity - AC#7's 'using Era.Core.Types' is correct for Result<T> type, not for IEvaluationContext. Feature already defers IEvaluationContext definition to F546-F547 in Handoffs table
- [resolved-applied] Phase1 iter5: IEvaluationContext architectural gap - Era.Core.Functions.IEvaluationContext lacks Talents/GetAbl methods needed for dialogue selection. Added explicit handoff to F547-extended for dialogue-specific evaluation context and Task#10 for feature creation
- [resolved-invalid] Phase1 iter5: Using statement import missing for IEvaluationContext - duplicate of already resolved Phase1 iter1 issue. AC#7's 'using Era.Core.Types' is for Result<T>, IEvaluationContext import intentionally deferred
- [resolved-invalid] Phase1 iter6: F552 Dependencies table issue - issue location incorrect (issue is in blockquote line 5-6 not Dependencies table). F552's Dependencies table already correctly shows F545 as predecessor
- [resolved-invalid] Phase1 iter7: Missing using statement for IEvaluationContext - duplicate of resolved Phase1 iter1/iter5 issues. Feature intentionally defers IEvaluationContext definition to F627, import will be determined by namespace selected
- [resolved-invalid] Phase1 iter9: AC#8 TODO pattern fix - fix incomplete, only suggests changing Expected pattern without changing Matcher from 'not_contains' to 'not_matches'. Per ENGINE.md Issue 39, regex alternation patterns require not_matches matcher
- [resolved-invalid] Phase1 iter10: Task#10 F627 creation task invalid - Task#10 IS valid per CLAUDE.md Deferred Task Protocol Option A and PHASE-7.md validation logic. Creation tasks are not bound by AC:Task 1:1 rule, file will be created during /run
- [resolved-applied] Phase6 iter10: F627 referenced in Creation Task T10 but feature file does not exist - User confirmed F627 creation via Task#10 during /run execution

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

| 課題 | 理由 | 追跡先 | 追跡先ID | Creation Task |
|------|------|--------|----------|-------------|
| DialogueEntry type definition | Interface references DialogueEntry but type may not exist yet | Feature | F546-F547 | - |
| IEvaluationContext with Talents/GetAbl | Interface parameter needs dialogue-specific evaluation context with Talents property and GetAbl method for dialogue selection | Feature | F627 | T10 |
| PriorityDialogueSelector implementation | Interface extraction does not include implementation | Feature | F552 | - |
| IConditionEvaluator integration | Selection requires condition evaluation for filtering | Feature | F550 | - |
| DI registration | Registering IDialogueSelector with container is out of scope | Feature | F553 | - |
| Selection behavior scenarios | Documentation of all selection scenarios (priority ties, edge cases, empty lists) requires implementation analysis | Feature | F552 | - |
| Current use case coverage analysis | Enumeration of existing KojoEngine selection patterns for interface compatibility verification | Feature | F552 | - |
| Error handling validation | Testing of error handling contract requires implementation | Feature | F552 | - |
| Strategy pattern composition design | Documentation of how alternative selection strategies will be instantiated and composed | Feature | F552 | - |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | feature creation | PENDING |
| 2026-01-26 | init | initializer | [REVIEWED]→[WIP] | READY:545:engine |
| 2026-01-26 | DEVIATION | implementer | dotnet build Era.Core/ | BUILD_FAIL: DialogueEntry type not found (CS0246) - expected per handoff |
| 2026-01-26 | resolution | implementer | Created stub DialogueEntry | User approved stub creation |
| 2026-01-26 | T1-T9 | implementer | Tasks 1-9 completed | SUCCESS - build passes, 1351 tests pass |
| 2026-01-26 | T10 | orchestrator | Created F627 [DRAFT] | Handoff Option A complete |
| 2026-01-26 | DEVIATION | doc-check | engine-dev/SKILL.md | Missing IDialogueSelector documentation |
| 2026-01-26 | resolution | orchestrator | Updated engine-dev/SKILL.md | Added IDialogueSelector section |

---

## Dependencies

| Type | ID | Description | Status |
|------|----|-------------|:------:|
| Predecessor | F625 | Post-Phase Review Phase 17 (Data Migration) | [DONE] |
| Successor | F552 | PriorityDialogueSelector Implementation | [BLOCKED] |
| Related | F543 | IConditionEvaluator Interface Extraction | [PROPOSED] |
| Related | F550 | ConditionEvaluator Implementation | - |

---

## Links

- [index-features.md](index-features.md)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md#phase-18-kojoengine-srp分割) - Phase 18 specification (lines 3890-4064)
- [feature-541.md](feature-541.md) - Phase 18 Planning
- [feature-template.md](reference/feature-template.md)
- [feature-625.md](feature-625.md) - Post-Phase Review Phase 17 (predecessor)
- [feature-552.md](feature-552.md) - PriorityDialogueSelector Implementation (successor)
- [feature-543.md](feature-543.md) - IConditionEvaluator Interface Extraction (related)
- [feature-546.md](feature-546.md) - DialogueEntry type definition (handoff)
- [feature-547.md](feature-547.md) - Dialogue specification patterns (handoff)
- [feature-550.md](feature-550.md) - ConditionEvaluator Implementation (related)
- [feature-553.md](feature-553.md) - DI registration (handoff)
