# Feature 430: Pipeline Behaviors (Logging/Validation/Transaction)

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

Implement three pipeline behaviors for Phase 9 Command Infrastructure: LoggingBehavior (command execution logging), ValidationBehavior (input validation), and TransactionBehavior (UoW transaction management). These behaviors provide cross-cutting concerns for all command handlers.

**Cross-Cutting Concerns**: Logging, validation, and transactions applied uniformly across all commands through Mediator Pipeline.

**Output**: Behavior implementations in `Era.Core/Commands/Behaviors/` and `Era.Core/Types/Unit.cs`

---

## Background

### Philosophy (Mid-term Vision)

**Phase 9: Command Infrastructure** - Migrate ERB command system to C# with Mediator Pattern for unified command execution pipeline with cross-cutting concerns (logging, validation, transactions).

### Problem (Current Issue)

Cross-cutting concerns scattered across individual commands:
- Inconsistent logging formats
- Duplicated validation logic
- Manual transaction management

### Goal (What to Achieve)

1. **LoggingBehavior** - Unified command execution logging with timing
2. **ValidationBehavior** - Centralized input validation before execution
3. **TransactionBehavior** - Automatic transaction management for state changes
4. **DI Registration** - Register behaviors in correct execution order

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | LoggingBehavior.cs exists | file | Glob | exists | Era.Core/Commands/Behaviors/LoggingBehavior.cs | [x] |
| 2 | ValidationBehavior.cs exists | file | Glob | exists | Era.Core/Commands/Behaviors/ValidationBehavior.cs | [x] |
| 3 | TransactionBehavior.cs exists | file | Glob | exists | Era.Core/Commands/Behaviors/TransactionBehavior.cs | [x] |
| 4 | DI registration (LoggingBehavior) | file | Grep | contains | "AddSingleton.*IPipelineBehavior.*LoggingBehavior" | [x] |
| 5 | DI registration (ValidationBehavior) | file | Grep | contains | "AddSingleton.*IPipelineBehavior.*ValidationBehavior" | [x] |
| 6 | DI registration (TransactionBehavior) | file | Grep | contains | "AddSingleton.*IPipelineBehavior.*TransactionBehavior" | [x] |
| 7 | LoggingBehavior unit test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~LoggingBehaviorTests" | [x] |
| 8 | ValidationBehavior unit test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~ValidationBehaviorTests" | [x] |
| 9 | TransactionBehavior unit test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TransactionBehaviorTests" | [x] |
| 10 | Pipeline integration test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~PipelineIntegrationTest" | [x] |
| 11 | Behavior equivalence verification | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~BehaviorEquivalence" | [x] |
| 12 | Zero technical debt | file | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 13 | Unit.cs exists | file | Glob | exists | Era.Core/Types/Unit.cs | [x] |

### AC Details

**AC#1-3**: Behavior implementations
- `LoggingBehavior<TCommand, TResult>` - Logs command name, execution time, success/failure
- `ValidationBehavior<TCommand, TResult>` - Validates IValidatable commands before execution
- `TransactionBehavior<TCommand, TResult>` - Wraps execution in UoW transaction

**AC#4-6**: DI registration verification
- Test: Grep pattern="AddSingleton.*IPipelineBehavior.*{Behavior}" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Registration order: Logging (1) → Validation (2) → Transaction (3)

**AC#7-9**: Behavior unit tests
- LoggingBehavior: Verifies log entries created with command name and timing
- ValidationBehavior: Verifies validation errors returned before handler execution
- TransactionBehavior: Verifies pass-through behavior preserves Result unchanged (Phase 9 stub). Actual transaction commit/rollback to be implemented in Phase 11.

**AC#10**: Pipeline integration test
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~PipelineIntegrationTest"`
- Verifies all three behaviors execute in correct order
- Verifies logging occurs even when validation fails

**AC#11**: Behavior equivalence verification
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~BehaviorEquivalence"`
- Verifies behaviors apply cross-cutting concerns without changing command semantics
- **Minimum**: 3 Assert statements covering: (1) command result unchanged when behaviors applied, (2) LoggingBehavior logs command name, (3) ValidationBehavior short-circuits on IValidatable.Validate() failure

**AC#12**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Behaviors/" and "Era.Core/Types/Unit.cs"
- Expected: 0 matches in all files (all implementation complete)

**AC#13**: Unit.cs exists
- Test: Glob pattern="Era.Core/Types/Unit.cs"
- Expected: File exists (required for ValidationBehavior's Result<Unit> return type)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 13 | Create Era.Core/Types/Unit.cs for IValidatable.Validate() return type | [x] |
| 2 | 1,4 | Create LoggingBehavior and register in DI | [x] |
| 3 | 2,5 | Create ValidationBehavior and register in DI | [x] |
| 4 | 3,6 | Create TransactionBehavior and register in DI | [x] |
| 5 | 7 | Write LoggingBehavior unit tests | [x] |
| 6 | 8 | Write ValidationBehavior unit tests | [x] |
| 7 | 9 | Write TransactionBehavior unit tests | [x] |
| 8 | 10 | Write pipeline integration tests | [x] |
| 9 | 11,12 | Create BehaviorEquivalenceTests.cs and verify zero technical debt in Era.Core/Commands/Behaviors/ | [x] |

<!-- Batch waiver (Task 2-4): Implementation + DI registration are atomic operations per component. -->
<!-- Batch waiver (Task 9): AC#11 (equivalence tests in test file) + AC#12 (tech debt check on production code in Era.Core/Commands/Behaviors/) are final verification after all behaviors created. -->
<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### LoggingBehavior Implementation

**LoggingBehavior** (`Era.Core/Commands/Behaviors/LoggingBehavior.cs`):
```csharp
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Types;
using Microsoft.Extensions.Logging;

namespace Era.Core.Commands.Behaviors;

/// <summary>
/// Pipeline behavior for command execution logging
/// </summary>
public class LoggingBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ILogger<LoggingBehavior<TCommand, TResult>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TCommand, TResult>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<TResult>> Handle(TCommand request,
        Func<Task<Result<TResult>>> next, CancellationToken ct)
    {
        var commandName = typeof(TCommand).Name;
        _logger.LogInformation("Executing command {CommandName}", commandName);

        var stopwatch = Stopwatch.StartNew();
        var result = await next();
        stopwatch.Stop();

        var success = result is Result<TResult>.Success;
        _logger.LogInformation("Completed command {CommandName} in {ElapsedMs}ms: {Success}",
            commandName, stopwatch.ElapsedMilliseconds, success);

        return result;
    }
}
```

### ValidationBehavior Implementation

**ValidationBehavior** (`Era.Core/Commands/Behaviors/ValidationBehavior.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Types;

namespace Era.Core.Commands.Behaviors;

/// <summary>
/// Marker interface for validatable commands
/// </summary>
public interface IValidatable
{
    /// <summary>Validate command</summary>
    Result<Unit> Validate();
}

/// <summary>
/// Pipeline behavior for input validation
/// </summary>
public class ValidationBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public async Task<Result<TResult>> Handle(TCommand request,
        Func<Task<Result<TResult>>> next, CancellationToken ct)
    {
        // Only validate if command implements IValidatable
        if (request is IValidatable validatable)
        {
            var validationResult = validatable.Validate();
            if (validationResult is Result<Unit>.Failure failure)
            {
                return Result<TResult>.Fail(failure.Error);
            }
        }

        return await next();
    }
}
```

### TransactionBehavior Implementation

**TransactionBehavior** (`Era.Core/Commands/Behaviors/TransactionBehavior.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Types;

namespace Era.Core.Commands.Behaviors;

/// <summary>
/// Pipeline behavior for transaction management (stub for Phase 9)
/// </summary>
/// <remarks>
/// Full UoW transaction support deferred to Phase 11 State Management.
/// This implementation provides the interface contract for future integration.
/// </remarks>
public class TransactionBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    public async Task<Result<TResult>> Handle(TCommand request,
        Func<Task<Result<TResult>>> next, CancellationToken ct)
    {
        // Phase 9: Pass-through behavior
        // Phase 11: Wrap in UoW transaction (BeginTransaction → Commit/Rollback)
        var result = await next();
        return result;
    }
}
```

### Unit Type for Validation

**Unit Type** (`Era.Core/Types/Unit.cs` if not exists):
```csharp
namespace Era.Core.Types;

/// <summary>
/// Unit type for operations with no return value
/// </summary>
public readonly struct Unit
{
    public static readonly Unit Value = new();
}
```

### NuGet Package Prerequisite

Add to `Era.Core/Era.Core.csproj` (required for LoggingBehavior's ILogger<T>):
```xml
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="8.0.0" />
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// Pipeline Behaviors (Phase 9) - Order matters!
services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
```

**Execution Order**: Logging → Validation → Transaction → Handler → Transaction → Validation → Logging

### Design Rationale

**Pipeline Order Justification**:
1. **LoggingBehavior first** - Logs all commands including validation failures
2. **ValidationBehavior second** - Fails fast before transaction begins
3. **TransactionBehavior last** - Only creates transaction for valid commands

**IValidatable Pattern**:
- Optional marker interface
- Commands with validation implement IValidatable
- Commands without validation skip ValidationBehavior

**TransactionBehavior Stub**:
- Phase 9: Interface contract only
- Phase 11: Full UoW integration (BeginTransaction/Commit/Rollback)

### Equivalence Verification

Legacy behavior: Commands execute without centralized logging/validation/transactions.

New behavior: All commands execute through unified pipeline with cross-cutting concerns applied consistently.

**Verification**: Command execution results remain unchanged. Pipeline adds logging/validation/transaction management without altering command semantics.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F429 | CommandDispatcher + Mediator Pipeline (IPipelineBehavior interface, CommandContext) |
| Related | F377 | Design Principles (Result<T> pattern, Unit type follows similar pattern) |
| Successor | F431 | Print Commands (use pipeline behaviors) |
| Successor | F432 | Flow Control Commands (use pipeline behaviors) |
| Successor | F433 | Variable & Array Commands (use pipeline behaviors) |
| Successor | F434 | System Commands (use pipeline behaviors) |
| Successor | F435 | SCOMF Special Commands (use pipeline behaviors) |

---

## Links

- [feature-424.md](feature-424.md) - Phase 9 Planning (parent feature)
- [feature-429.md](feature-429.md) - CommandDispatcher + Mediator Pipeline (dependency)
- [feature-377.md](feature-377.md) - Design Principles (Result<T> pattern)
- [feature-431.md](feature-431.md) - Print Commands (successor)
- [feature-432.md](feature-432.md) - Flow Control Commands (successor)
- [feature-433.md](feature-433.md) - Variable & Array Commands (successor)
- [feature-434.md](feature-434.md) - System Commands (successor)
- [feature-435.md](feature-435.md) - SCOMF Special Commands (successor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 9 Pipeline Behaviors table

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-10 | create | implementer | Created from F424 Phase 9 Planning | PROPOSED |
| 2026-01-10 16:37 | START | implementer | Task 2,3,4 | - |
| 2026-01-10 16:37 | END | implementer | Task 2,3,4 | SUCCESS |
| 2026-01-10 | finalize | finalizer | Verified all AC/Tasks complete, all tests passing | READY_TO_COMMIT |
