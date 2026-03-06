# Feature 477: CommandProcessor Implementation

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

## Created: 2026-01-13

---

## Summary

**CommandProcessor COM execution engine** - Central dispatcher for COM (Command) execution with handler resolution and error handling.

Implement `CommandProcessor` providing COM execution orchestration, handler resolution via IComRegistry, execution context management, and error recovery. This component dispatches player/system commands to registered COM handlers and manages execution state.

**Output**: `Era.Core/CommandProcessor.cs` and DI registration in `ServiceCollectionExtensions.cs`.

**Volume**: ~220 lines (within ~300 line limit for engine type).

---

## Background

### Philosophy (Mid-term Vision)

**Phase 14: Era.Core Engine** - Pure C# game engine implementation enabling headless execution for automated testing and CI/CD integration. CommandProcessor provides uniform COM execution interface abstracting handler dispatch complexity.

### Problem (Current Issue)

Phase 14 requires COM execution infrastructure:
- COM handlers registered in IComRegistry (Phase 12) but no execution orchestration exists
- GameEngine needs uniform interface for COM execution
- Execution context must be managed (arguments, state, scope)
- Errors during COM execution must be handled gracefully

### Goal (What to Achieve)

1. Implement `CommandProcessor` class providing COM execution
2. Resolve COM handlers via IComRegistry
3. Manage execution context with argument passing
4. Handle COM execution errors with Result<T>
5. Register in DI container
6. Delete all tech debt (TODO/FIXME/HACK)
7. Verify tests pass after implementation

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CommandProcessor.cs exists | file | Glob | exists | Era.Core/CommandProcessor.cs | [x] |
| 2 | ICommandProcessor interface exists | code | Grep | contains | interface ICommandProcessor | [x] |
| 3 | CommandProcessor implements ICommandProcessor | code | Grep | contains | CommandProcessor : ICommandProcessor | [x] |
| 4 | Execute method signature | code | Grep | contains | Execute\\(ComId .*, IComContext | [x] |
| 5 | IComRegistry dependency | code | Grep | contains | IComRegistry | [x] |
| 6 | Handler resolution logic | code | Grep | contains | TryGet | [x] |
| 7 | Error handling with Result | code | Grep | contains | Result.*\\.Fail | [x] |
| 8 | DI registration with interface | file | Grep | contains | AddSingleton<ICommandProcessor, CommandProcessor> | [x] |
| 9 | CommandProcessorTests pass (positive/negative) | test | Bash | succeeds | dotnet test --filter CommandProcessorTests | [x] |
| 10 | Namespace declaration | code | Grep | contains | namespace Era\\.Core | [x] |
| 11 | Zero technical debt | code | Grep | not_contains | TODO\\|FIXME\\|HACK | [x] |
| 12 | Unit tests pass | test | Bash | succeeds | dotnet test Era.Core.Tests | [x] |

### AC Details

**AC#1**: File existence verification
- Test: Glob pattern="Era.Core/CommandProcessor.cs"
- Expected: File exists

**AC#2**: CommandProcessor class definition
- Test: Grep pattern="public class CommandProcessor" path="Era.Core/CommandProcessor.cs"
- Expected: Class declaration present

**AC#3**: Execute method signature
- Test: Grep pattern="Result<ComResult> Execute" path="Era.Core/CommandProcessor.cs"
- Expected: Execute method returns Result<ComResult>

**AC#4**: IComRegistry dependency
- Test: Grep pattern="IComRegistry" path="Era.Core/CommandProcessor.cs"
- Expected: Constructor or field references IComRegistry for handler resolution

**AC#5**: Handler resolution logic
- Test: Grep pattern="GetHandler" path="Era.Core/CommandProcessor.cs"
- Expected: Calls IComRegistry.GetHandler to resolve COM handlers

**AC#6**: Error handling with Result
- Test: Grep pattern="Result\\.Fail" path="Era.Core/CommandProcessor.cs"
- Expected: Uses Result.Fail for recoverable errors (handler not found, execution error)

**AC#7**: DI registration
- Test: Grep pattern="AddSingleton.*CommandProcessor" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: DI registration present (interface optional, class required)

**AC#8**: Execute returns Fail on missing handler (negative test)
- Test: `dotnet test --filter FullyQualifiedName~CommandProcessorTests`
- Expected: All CommandProcessorTests pass
- Includes test for Execute(ComId.C999, args) when handler not registered returns Result.Fail

**AC#9**: Execute succeeds with valid handler (positive test)
- Test: `dotnet test --filter FullyQualifiedName~CommandProcessorTests`
- Expected: All CommandProcessorTests pass
- Includes test for Execute(ComId.C0, args) with registered handler returns Result.Success

**AC#10**: Namespace declaration
- Test: Grep pattern="namespace Era\\.Core" path="Era.Core/CommandProcessor.cs"
- Expected: Proper namespace declaration

**AC#11**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/CommandProcessor.cs"
- Expected: 0 matches

**AC#12**: Unit tests pass
- Test: `dotnet test Era.Core.Tests`
- Expected: All Era.Core tests pass (exit code 0)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,10 | Create Era.Core/CommandProcessor.cs with namespace declaration | [x] |
| 2 | 2 | Define ICommandProcessor interface with Execute method | [x] |
| 3 | 3,4,5,6,7 | Implement CommandProcessor class implementing ICommandProcessor | [x] |
| 4 | 8 | Register ICommandProcessor with CommandProcessor in DI | [x] |
| 5 | 9 | Write CommandProcessorTests with positive and negative test cases | [x] |
| 6 | 11 | Remove all TODO/FIXME/HACK comments | [x] |
| 7 | 12 | Run dotnet test and fix any failures | [x] |

<!-- AC:Task 1:1 Rule: 12 ACs = 7 Tasks (batch waivers for Task 2: related implementation steps per F384, Task 4: related test cases per testing SKILL) -->

<!-- **Batch verification waiver (Task 2)**: Following F384 precedent for related method implementation with dependencies. -->
<!-- **Batch verification waiver (Task 4)**: Positive/negative test cases for same method are related per testing SKILL. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Class Definition

Per Phase 14 Task 4 (architecture.md line 3365):

```csharp
// Era.Core/CommandProcessor.cs
using Era.Core.Types;
using Era.Core.Commands;

namespace Era.Core;

/// <summary>
/// COM execution orchestrator with handler resolution and error handling.
/// </summary>
public class CommandProcessor
{
    private readonly IComRegistry _registry;

    public CommandProcessor(IComRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <summary>Execute COM with given ID and arguments</summary>
    /// <param name="comId">COM ID</param>
    /// <param name="args">Execution arguments</param>
    /// <returns>Success with ComResult if executed, Fail if handler not found or execution error</returns>
    public Result<ComResult> Execute(ComId comId, ComArgs args)
    {
        // Implementation: Handler resolution, execution, error handling
    }
}
```

### Implementation Requirements

| Requirement | Verification |
|-------------|--------------|
| **Result<T> usage** | Execute returns Result for error handling |
| **Error messages** | Japanese format: "{Operation}に失敗しました: {reason}" |
| **Handler resolution** | Use IComRegistry.GetHandler(ComId) |
| **Null checking** | Constructor validates IComRegistry is not null (ArgumentNullException) |

### Error Message Format

- Handler not found: `"COM{comId}のハンドラーが見つかりません"`
- Execution error: `"COM{comId}の実行に失敗しました: {details}"`
- Invalid arguments: `"COM{comId}の引数が不正です"`

### Execution Algorithm

1. Validate arguments (throw ArgumentNullException for null args - programmer error)
2. Resolve handler via `_registry.GetHandler(comId)`
3. If handler not found, return `Result.Fail("COM{comId}のハンドラーが見つかりません")`
4. Execute handler with args: `handler.Execute(args)`
5. If execution throws or returns Fail, return `Result.Fail("COM{comId}の実行に失敗しました: {error}")`
6. Return Success with ComResult

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<CommandProcessor>();
```

**Note**: No interface defined (CommandProcessor is concrete class used directly by GameEngine).

### Test Requirements

**Positive Tests**:
- Execute with registered handler returns Success<ComResult>
- Execute with valid arguments passes args to handler
- Execute returns handler's ComResult

**Negative Tests**:
- Execute with unregistered ComId returns Fail with handler not found error
- Execute with handler throwing exception returns Fail with execution error
- Execute with null args throws ArgumentNullException (programmer error)

**Test Naming Convention**: Test methods follow `Test{MethodName}{Scenario}` format (e.g., `TestExecuteMissingHandler`, `TestExecuteValidHandler`).

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F471 | Phase 14 Planning defines this feature |
| Predecessor | F474 | GameEngine calls CommandProcessor during turn processing |
| Reference | Phase 12 | IComRegistry and COM handlers (F452-F462) |

---

## Links

- [feature-471.md](feature-471.md) - Phase 14 Planning (parent feature)
- [feature-474.md](feature-474.md) - GameEngine (caller)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 14 Task 4 definition
- [feature-template.md](reference/feature-template.md) - Feature structure guidelines

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-13 | create | spec-writer | Created from F471 Phase 14 Planning Task 4 | PROPOSED |
