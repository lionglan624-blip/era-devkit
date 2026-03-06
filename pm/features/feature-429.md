# Feature 429: CommandDispatcher + Mediator Pipeline

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

## Created: 2026-01-10

---

## Summary

Implement CommandDispatcher architecture with Mediator Pattern for Phase 9 Command Infrastructure. This feature establishes the core command execution pipeline with `ICommand<TResult>`, `ICommandHandler<TCommand, TResult>`, `ICommandDispatcher`, and `IPipelineBehavior<TCommand, TResult>` interfaces.

**Architecture Foundation**: This feature provides the infrastructure for all Phase 9 command implementations (F430-F435).

**Output**: Core command infrastructure in `Era.Core/Commands/`

---

## Background

### Philosophy (Mid-term Vision)

**Phase 9: Command Infrastructure** - Establish CommandDispatcher as the single source of truth (SSOT) for all command execution in Era.Core. Migrate ERB command system to C# with Mediator Pattern, providing unified execution pipeline with cross-cutting concerns (logging, validation, transactions). This enables testable, extensible command handlers that replace legacy static CommandRegistry patterns and support long-term maintenance through clean separation of concerns.

### Problem (Current Issue)

Current command system lacks:
- Unified execution pipeline
- Cross-cutting concerns (logging, validation, transactions)
- Type-safe command/handler pattern
- Extensible pipeline behaviors

**Note**: Existing `engine/Assets/Scripts/Emuera/Sub/CommandDispatcher.cs` handles ERB script execution via FunctionCode lookup. This feature creates Era.Core/Commands infrastructure for C# command handlers using Mediator Pattern - a parallel system for the new C# runtime, not a migration of the existing dispatcher.

### Goal (What to Achieve)

1. **CommandDispatcher** - Central command execution orchestrator
2. **Core Interfaces** - ICommand, ICommandHandler, IPipelineBehavior, ICommandDispatcher
3. **CommandContext** - Execution context management
4. **Strongly Typed IDs** - CommandId for type-safe command identification
5. **DI Registration** - Service registration for command infrastructure

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CommandId compatibility verified | file | Glob | exists | Era.Core/Types/CommandId.cs | [x] |
| 2 | ICommand.cs exists | file | Glob | exists | Era.Core/Commands/ICommand.cs | [x] |
| 3 | ICommandHandler.cs exists | file | Glob | exists | Era.Core/Commands/ICommandHandler.cs | [x] |
| 4 | IPipelineBehavior.cs exists | file | Glob | exists | Era.Core/Commands/IPipelineBehavior.cs | [x] |
| 5 | ICommandDispatcher.cs exists | file | Glob | exists | Era.Core/Commands/ICommandDispatcher.cs | [x] |
| 6 | CommandDispatcher.cs exists | file | Glob | exists | Era.Core/Commands/CommandDispatcher.cs | [x] |
| 7 | CommandContext.cs exists | file | Glob | exists | Era.Core/Commands/CommandContext.cs | [x] |
| 8 | DI registration | file | Grep | contains | "AddSingleton.*ICommandDispatcher.*CommandDispatcher" | [x] |
| 9 | CommandDispatcher unit test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~CommandDispatcherTests" | [x] |
| 10 | Pipeline ordering test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~PipelineOrderingTests" | [x] |
| 11 | Dispatch equivalence verification | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~DispatchEquivalenceTests" | [x] |
| 12 | Zero technical debt | file | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |

### AC Details

**AC#1**: CommandId compatibility verified
- File: `Era.Core/Types/CommandId.cs` (existing, created by F377)
- Uses `readonly record struct` pattern with implicit int conversion
- **Verification**: File existence (Glob) implies compatibility since F377 designed CommandId specifically for Phase 9 command infrastructure with correct operators and equality semantics.

**AC#2-5**: Core interface definitions
- `ICommand<TResult>` - Command marker with CommandId
- `ICommandHandler<TCommand, TResult>` - Command handler interface
- `IPipelineBehavior<TCommand, TResult>` - Pipeline behavior interface
- `ICommandDispatcher` - Dispatcher interface

**AC#6**: CommandDispatcher implementation
- Resolves ICommandHandler from DI
- Executes IPipelineBehavior chain in registration order
- Returns Result<TResult>

**AC#7**: CommandContext implementation
- Execution context (CharacterId, Scope, etc.)
- Passed through pipeline
- **Note**: CommandContext is a standalone data class for Phase 9 foundation. Integration with ICommand/IPipelineBehavior interfaces is deferred to F430 (Pipeline Behaviors) where context-aware behaviors will consume it.

**AC#8**: DI registration verification
- Test: Grep pattern="AddSingleton.*ICommandDispatcher.*CommandDispatcher" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: Registration exists

**AC#9**: CommandDispatcher unit test
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~CommandDispatcherTests"`
- Verifies handler resolution and execution

**AC#10**: Pipeline ordering test
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~PipelineOrderingTests"`
- Verifies behaviors execute in registration order

**AC#11**: Dispatch equivalence verification
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~DispatchEquivalenceTests"`
- Verifies dispatcher behavior matches legacy CommandRegistry pattern
- **Minimum**: 3 test methods covering: (1) registered handler execution, (2) pipeline behavior ordering, (3) unregistered command error

**AC#12**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/"
- Expected: 0 matches (all implementation complete)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Verify existing CommandId meets ICommand requirements | [x] |
| 2 | 2,3,4,5 | Create core interfaces (ICommand/ICommandHandler/IPipelineBehavior/ICommandDispatcher) | [x] |
| 3 | 6 | Implement CommandDispatcher with handler resolution and pipeline execution | [x] |
| 4 | 7 | Create CommandContext for execution context | [x] |
| 5 | 8 | Register CommandDispatcher in DI | [x] |
| 6 | 9 | Write CommandDispatcher unit tests | [x] |
| 7 | 10 | Write pipeline ordering tests | [x] |
| 8 | 11,12 | Final verification: dispatch equivalence tests and tech debt check | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->
<!-- Batch waiver (Task 2): Related interface signatures per F384 precedent. AC#2-5 are file-existence checks (Glob), not behavioral verification. -->
<!-- Batch waiver (Task 8): AC#11 (equivalence tests) and AC#12 (tech debt check) are final verification steps. Tech debt check runs on files created by AC#11 tests. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Core Interfaces

**CommandId Strongly Typed ID**: Uses existing `Era.Core/Types/CommandId.cs` (created by F377).
- Pattern: `readonly record struct` with `int Value` property
- Provides implicit int conversion and equality (via record struct semantics)
- No modification needed for F429 scope

**ICommand Interface** (`Era.Core/Commands/ICommand.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Commands;

/// <summary>
/// Command marker interface
/// </summary>
/// <typeparam name="TResult">Command result type</typeparam>
public interface ICommand<TResult>
{
    /// <summary>Command identifier</summary>
    CommandId Id { get; }
}
```

**ICommandHandler Interface** (`Era.Core/Commands/ICommandHandler.cs`):
```csharp
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Types;

namespace Era.Core.Commands;

/// <summary>
/// Command handler interface
/// </summary>
/// <typeparam name="TCommand">Command type</typeparam>
/// <typeparam name="TResult">Result type</typeparam>
public interface ICommandHandler<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>Handle command execution</summary>
    Task<Result<TResult>> Handle(TCommand command, CancellationToken ct);
}
```

**IPipelineBehavior Interface** (`Era.Core/Commands/IPipelineBehavior.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Types;

namespace Era.Core.Commands;

/// <summary>
/// Pipeline behavior for cross-cutting concerns
/// </summary>
/// <typeparam name="TCommand">Command type</typeparam>
/// <typeparam name="TResult">Result type</typeparam>
public interface IPipelineBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Handle pipeline behavior
    /// </summary>
    /// <param name="request">Command</param>
    /// <param name="next">Next behavior or handler</param>
    /// <param name="ct">Cancellation token</param>
    Task<Result<TResult>> Handle(TCommand request,
        Func<Task<Result<TResult>>> next, CancellationToken ct);
}
```

**ICommandDispatcher Interface** (`Era.Core/Commands/ICommandDispatcher.cs`):
```csharp
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Types;

namespace Era.Core.Commands;

/// <summary>
/// Command dispatcher interface
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>Dispatch command to handler through pipeline</summary>
    Task<Result<TResult>> Dispatch<TResult>(ICommand<TResult> command, CancellationToken ct = default);
}
```

### CommandDispatcher Implementation

**CommandDispatcher** (`Era.Core/Commands/CommandDispatcher.cs`):
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Era.Core.Commands;

/// <summary>
/// Command dispatcher implementation with pipeline support
/// </summary>
public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task<Result<TResult>> Dispatch<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        if (command == null)
            return Result<TResult>.Fail("Command cannot be null");

        var commandType = command.GetType();
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

        var handler = _serviceProvider.GetService(handlerType);
        if (handler == null)
            return Result<TResult>.Fail($"No handler registered for command type {commandType.Name}");

        // Get all pipeline behaviors for this command type
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(commandType, typeof(TResult));
        var behaviors = _serviceProvider.GetServices(behaviorType).Cast<object>().Reverse().ToList();

        // Build pipeline chain
        Func<Task<Result<TResult>>> pipeline = async () =>
        {
            var handleMethod = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResult>, TResult>.Handle));
            if (handleMethod == null)
                return Result<TResult>.Fail($"Handle method not found on handler {handlerType.Name}");

            var result = await (Task<Result<TResult>>)handleMethod.Invoke(handler, new object[] { command, ct })!;
            return result;
        };

        // Wrap with behaviors (in reverse order of registration)
        foreach (var behavior in behaviors)
        {
            var currentPipeline = pipeline;
            var currentBehavior = behavior;

            pipeline = async () =>
            {
                var handleMethod = behaviorType.GetMethod(nameof(IPipelineBehavior<ICommand<TResult>, TResult>.Handle));
                if (handleMethod == null)
                    return Result<TResult>.Fail($"Handle method not found on behavior {behaviorType.Name}");

                var result = await (Task<Result<TResult>>)handleMethod.Invoke(currentBehavior,
                    new object[] { command, currentPipeline, ct })!;
                return result;
            };
        }

        return await pipeline();
    }
}
```

### CommandContext Implementation

**CommandContext** (`Era.Core/Commands/CommandContext.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Commands;

/// <summary>
/// Command execution context
/// </summary>
public class CommandContext
{
    /// <summary>Target character ID. Null for system-wide commands (e.g., SAVEDATA, CONFIG).</summary>
    public CharacterId? CharacterId { get; init; }

    /// <summary>Execution scope identifier</summary>
    public string? Scope { get; init; }

    /// <summary>Additional context data</summary>
    public object? Data { get; init; }
}
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// Command Infrastructure (Phase 9)
services.AddSingleton<ICommandDispatcher, CommandDispatcher>();
```

### Design Rationale

**Mediator Pattern Benefits**:
1. **Decoupling** - Commands don't know their handlers
2. **Pipeline** - Cross-cutting concerns applied uniformly
3. **Testability** - Mock handlers and behaviors independently
4. **Extensibility** - Add behaviors without changing commands

**IPipelineBehavior Ordering**:
- Behaviors execute in registration order
- LoggingBehavior (F430) → ValidationBehavior (F430) → TransactionBehavior (F430)
- Reverse order for completion (transaction commits before logging)

**Test Naming Convention**: Test classes follow `{ClassName}Tests` format (e.g., `CommandDispatcherTests`, `PipelineOrderingTests`). Test methods should be descriptive of the scenario being tested.

**Reflection Usage**: CommandDispatcher uses reflection for dynamic handler resolution (MakeGenericType, GetMethod, Invoke). This is a standard Mediator pattern implementation trade-off enabling type-safe command dispatch without compile-time handler registration. Runtime errors from missing handlers are caught and returned as Result.Fail. The null-forgiving operator (`!`) on Invoke results is intentional - null indicates a contract violation that should fail fast during development.

### Equivalence Verification

Legacy behavior from `engine/Assets/Scripts/Emuera/GameProc/Commands/CommandRegistry.cs`:
1. Look up command by identifier
2. Execute command with context
3. Return execution result

New behavior:
1. Resolve ICommandHandler by command type
2. Execute pipeline behaviors
3. Execute handler
4. Return Result<T>

**Verification**: Both approaches execute commands with context and return results. Pipeline adds cross-cutting concerns without changing command semantics.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F423 | Phase 8 Post-Phase Review must pass first |
| Predecessor | F377 | Design Principles (static class禁止, Strongly Typed ID, Result型) |
| Successor | F430 | Pipeline Behaviors (depends on IPipelineBehavior) |
| Successor | F431 | Print Commands (depends on ICommand/ICommandHandler) |
| Successor | F432 | Flow Control Commands (depends on ICommand/ICommandHandler) |
| Successor | F433 | Variable & Array Commands (depends on ICommand/ICommandHandler) |
| Successor | F434 | System Commands (depends on ICommand/ICommandHandler) |
| Successor | F435 | SCOMF Special Commands (depends on ICommand/ICommandHandler) |

---

## Links

- [feature-424.md](feature-424.md) - Phase 9 Planning (parent feature)
- [feature-377.md](feature-377.md) - Phase 4 Design Principles (reference)
- [feature-423.md](feature-423.md) - Phase 8 Post-Phase Review (dependency)
- [feature-430.md](feature-430.md) - Pipeline Behaviors (CommandContext integration deferred)
- [feature-431.md](feature-431.md) - Print Commands (successor)
- [feature-432.md](feature-432.md) - Flow Control Commands (successor)
- [feature-433.md](feature-433.md) - Variable & Array Commands (successor)
- [feature-434.md](feature-434.md) - System Commands (successor)
- [feature-435.md](feature-435.md) - SCOMF Special Commands (successor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 9 definition

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-10 | create | implementer | Created from F424 Phase 9 Planning | PROPOSED |
| 2026-01-10 15:34 | START | implementer | Task 1-5 | - |
| 2026-01-10 15:36 | END | implementer | Task 1-5 | SUCCESS |
