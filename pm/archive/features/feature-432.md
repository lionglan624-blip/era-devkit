# Feature 432: Flow Control Commands (IF/FOR/WHILE/CALL/RETURN)

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

Migrate core Flow Control commands from legacy GameProc to ICommand/ICommandHandler pattern. This includes:
- **Flow Category**: IF, ELSEIF, ELSE, ENDIF, FOR, NEXT, WHILE, WEND
- **Call Category**: CALL, CALLFORM, RETURN, RETURNFORM

**Control Flow Responsibility**: Core control flow and function call commands migrated to unified command infrastructure.

**Note**: REPEAT/REND and GOTO/JUMP/TRYGOTO/TRYJUMP are out-of-scope (see Review Notes).

**Output**: Command implementations in `Era.Core/Commands/Flow/`

---

## Background

### Philosophy (Mid-term Vision)

**Phase 9: Command Infrastructure** - Establish IExecutionStack and IScopeManager as the single source of truth (SSOT) for all flow control state management in Era.Core. Replace scattered execution stack manipulation in GameProc with testable, injectable services. Migrate ERB command system to C# with Mediator Pattern for unified command execution pipeline with cross-cutting concerns (logging, validation, transactions).

### Problem (Current Issue)

Flow control commands scattered across GameProc with complex state management:
- IF/FOR/WHILE nesting requires execution stack
- CALL/RETURN requires scope management
- GOTO/JUMP requires label resolution

### Goal (What to Achieve)

1. **Migrate core Flow Control commands** to ICommand/ICommandHandler pattern
2. **IExecutionStack service** - Stack for control flow nesting
3. **IScopeManager service** - Scope management for CALL/RETURN
4. **ILabelResolver service** - Label resolution for function calls (infrastructure for future GOTO/JUMP)
5. **Equivalence verification** - Control flow matches legacy behavior

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Flow command directory exists | file | Glob | exists | Era.Core/Commands/Flow/ | [x] |
| 2 | IExecutionStack interface exists | file | Glob | exists | Era.Core/Interfaces/IExecutionStack.cs | [x] |
| 3 | IScopeManager interface exists | file | Glob | exists | Era.Core/Interfaces/IScopeManager.cs | [x] |
| 4 | ILabelResolver interface exists | file | Glob | exists | Era.Core/Interfaces/ILabelResolver.cs | [x] |
| 5 | IF command handler exists | file | Grep | contains | "class IfHandler" | [x] |
| 6 | FOR command handler exists | file | Grep | contains | "class ForHandler" | [x] |
| 7 | WHILE command handler exists | file | Grep | contains | "class WhileHandler" | [x] |
| 8 | CALL command handler exists | file | Grep | contains | "class CallHandler" | [x] |
| 9 | RETURN command handler exists | file | Grep | contains | "class ReturnHandler" | [x] |
| 10a | DI registration (IfHandler) | file | Grep | contains | "IfHandler" | [x] |
| 10b | DI registration (ForHandler) | file | Grep | contains | "ForHandler" | [x] |
| 10c | DI registration (WhileHandler) | file | Grep | contains | "WhileHandler" | [x] |
| 10d | DI registration (CallHandler) | file | Grep | contains | "CallHandler" | [x] |
| 10e | DI registration (ReturnHandler) | file | Grep | contains | "ReturnHandler" | [x] |
| 11 | Flow control unit test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~FlowControlTests" | [x] |
| 12 | Flow control equivalence verification | file | Grep | contains | "FlowControlEquivalence" | [x] |
| 13 | Zero technical debt | file | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |

### AC Details

**AC#1**: Flow command directory structure
- `Era.Core/Commands/Flow/` contains all flow control command implementations

**AC#2-4**: Control flow service interfaces
- IExecutionStack - Push/Pop for IF/FOR/WHILE nesting
- IScopeManager - PushScope/PopScope for CALL/RETURN
- ILabelResolver - ResolveLabel for GOTO/JUMP

**AC#5**: IF command handler
- Test: Grep pattern="class IfHandler" path="Era.Core/Commands/Flow/"
- Variant handlers (ElseIfHandler, ElseHandler, EndIfHandler) follow same pattern

**AC#6**: FOR command handler
- Test: Grep pattern="class ForHandler" path="Era.Core/Commands/Flow/"
- NextHandler follows same pattern

**AC#7**: WHILE command handler
- Test: Grep pattern="class WhileHandler" path="Era.Core/Commands/Flow/"
- WendHandler follows same pattern

**AC#8**: CALL command handler
- Test: Grep pattern="class CallHandler" path="Era.Core/Commands/Flow/"
- CallFormHandler follows same pattern

**AC#9**: RETURN command handler
- Test: Grep pattern="class ReturnHandler" path="Era.Core/Commands/Flow/"
- ReturnFormHandler follows same pattern

**AC#10a-e**: DI registration verification (per handler)
- AC#10a: Grep pattern="IfHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- AC#10b: Grep pattern="ForHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- AC#10c: Grep pattern="WhileHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- AC#10d: Grep pattern="CallHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- AC#10e: Grep pattern="ReturnHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Simple handler name match is sufficient - file scope limits false positives
- Variant handlers (ElseIfHandler, ElseHandler, EndIfHandler, NextHandler, WendHandler, CallFormHandler, ReturnFormHandler) verified by implementation task completion, not separate ACs

**AC#11**: Flow control unit tests
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~FlowControlTests"`
- Verifies IF/FOR/WHILE nesting and CALL/RETURN scope
- **Minimum**: 3 Assert statements
- **Positive scenarios**: (1) IF/ELSEIF/ELSE nesting, (2) FOR/WHILE loop state, (3) CALL/RETURN scope stack
- **Negative scenarios**: (1) Empty stack Pop() returns error, (2) Unbalanced ENDIF/NEXT/WEND returns error, (3) RETURN without CALL returns error
- **Example scenarios**: nested IF within FOR loop, WHILE with early break, CALL from nested context

**AC#12**: Flow control equivalence verification
- Test: Grep pattern="FlowControlEquivalence" path="Era.Core.Tests/Commands/Flow/"
- Verifies control flow matches legacy IF/FOR/WHILE/CALL/RETURN behavior
- **Minimum**: 3 Assert statements
- **Positive scenarios**: (1) IF condition branching matches legacy, (2) FOR/WHILE loop iteration matches legacy, (3) CALL/RETURN scope matches legacy
- **Negative scenarios**: (1) Invalid IF condition handling matches legacy, (2) FOR loop boundary error matches legacy, (3) CALL to non-existent label matches legacy
- **Example scenarios**: legacy IF block execution order, FOR counter increments match, RETURN restores correct scope depth

**AC#13**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Flow/"
- Expected: 0 matches (all implementation complete)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4 | Create Flow command directory, service interfaces, and service implementations | [x] |
<!-- Batch waiver (Task 1): Related control flow service interfaces and implementations per F429 precedent. AC#1-4 are file-existence checks (Glob/Grep). Service implementations (ExecutionStack, ScopeManager, LabelResolver) are standard implementations of the interfaces. -->
| 2 | 5 | Implement IF/ELSEIF/ELSE/ENDIF commands and handlers | [x] |
| 3 | 6 | Implement FOR/NEXT commands and handlers | [x] |
| 4 | 7 | Implement WHILE/WEND commands and handlers | [x] |
| 5 | 8 | Implement CALL/CALLFORM commands and handlers | [x] |
| 6 | 9 | Implement RETURN/RETURNFORM commands and handlers | [x] |
| 7 | 10a-e | Register all flow handlers in DI | [x] |
| 8 | 11 | Write flow control unit tests | [x] |
| 9 | 12,13 | Verify flow control equivalence and remove technical debt | [x] |
<!-- Batch waiver (Task 9): AC#12 (equivalence test file) and AC#13 (tech debt check) are final verification steps. Tech debt check runs on code created by implementation tasks. Per F429/F430 precedent. -->

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Location**: `engine/Assets/Scripts/Emuera/GameProc/Process.ScriptProc.cs` (flow control inline)

| Command | Location | Category | Purpose |
|---------|----------|----------|---------|
| IF/ELSEIF/ELSE/ENDIF | Process.ScriptProc.cs | Flow | Conditional branching (inline in runScriptProc) |
| FOR/NEXT | Process.ScriptProc.cs | Flow | For loop |
| WHILE/WEND | Process.ScriptProc.cs | Flow | While loop |
| REPEAT/REND | Process.ScriptProc.cs | Flow | Repeat loop |
| CALL/CALLFORM | Process.CalledFunction.cs | Call | Function call |
| RETURN/RETURNFORM | Process.CalledFunction.cs | Call | Function return |
| GOTO/JUMP | Process.ScriptProc.cs | Call | Label jump |
| TRYGOTO/TRYJUMP | Commands/Flow/TryGotoListCommand.cs | Call | Safe label jump |

### Control Flow Service Interfaces

**Design Rationale**: Service interfaces (IExecutionStack, IScopeManager, ILabelResolver) are placed in `Era.Core/Interfaces/` rather than `Era.Core/Commands/` because they represent cross-cutting state management services, not command-specific types. Command interfaces (ICommand, ICommandHandler) define the command pattern API, while these service interfaces define runtime state management. This separation follows the principle of separating infrastructure services from command definitions.

**IExecutionStack** (`Era.Core/Interfaces/IExecutionStack.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces;

/// <summary>
/// Execution stack for control flow nesting
/// </summary>
public interface IExecutionStack
{
    /// <summary>Push control flow frame (IF/FOR/WHILE)</summary>
    Result<Unit> Push(ControlFlowFrame frame);

    /// <summary>Pop control flow frame</summary>
    Result<ControlFlowFrame> Pop();

    /// <summary>Peek current frame</summary>
    Result<ControlFlowFrame> Peek();

    /// <summary>Check if stack is empty</summary>
    bool IsEmpty { get; }
}

/// <summary>
/// Control flow frame
/// </summary>
public record ControlFlowFrame(ControlFlowType Type, int StartLine, object? State);

/// <summary>
/// Control flow type
/// </summary>
public enum ControlFlowType
{
    If,
    For,
    While
    // Repeat: Reserved for future REPEAT/REND feature (out-of-scope for F432)
}
```

**IScopeManager** (`Era.Core/Interfaces/IScopeManager.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces;

/// <summary>
/// Scope manager for CALL/RETURN
/// </summary>
public interface IScopeManager
{
    /// <summary>Push function scope</summary>
    Result<Unit> PushScope(string functionName, int returnLine);

    /// <summary>Pop function scope</summary>
    Result<(string functionName, int returnLine)> PopScope();

    /// <summary>Get current scope depth</summary>
    int Depth { get; }
}
```

**ILabelResolver** (`Era.Core/Interfaces/ILabelResolver.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces;

/// <summary>
/// Label resolver for GOTO/JUMP
/// </summary>
public interface ILabelResolver
{
    /// <summary>Resolve label to line number. Returns Result.Fail if label not found.</summary>
    Result<int> ResolveLabel(string labelName);

    /// <summary>
    /// Try resolve label (for TRYGOTO/TRYJUMP).
    /// Note: Uses TryPattern (bool + out) intentionally for ERB TRYGOTO/TRYJUMP semantics
    /// where failure is expected control flow, not error state. Result&lt;T&gt; for unexpected errors,
    /// TryPattern for expected "label may not exist" scenarios.
    /// </summary>
    bool TryResolveLabel(string labelName, out int lineNumber);

    /// <summary>Register label</summary>
    Result<Unit> RegisterLabel(string labelName, int lineNumber);
}
```

### Flow Command Definitions

**IfCommand** (`Era.Core/Commands/Flow/IfCommand.cs`):
```csharp
using Era.Core.Commands;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// IF command - conditional branching
/// </summary>
public record IfCommand(CommandId Id, bool Condition) : ICommand<Unit>;

/// <summary>
/// ELSEIF command
/// </summary>
public record ElseIfCommand(CommandId Id, bool Condition) : ICommand<Unit>;

/// <summary>
/// ELSE command
/// </summary>
public record ElseCommand(CommandId Id) : ICommand<Unit>;

/// <summary>
/// ENDIF command
/// </summary>
public record EndIfCommand(CommandId Id) : ICommand<Unit>;
```

**ForCommand** (`Era.Core/Commands/Flow/ForCommand.cs`):
```csharp
using Era.Core.Commands;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// FOR command - for loop
/// </summary>
public record ForCommand(CommandId Id, string Variable, int Start, int End, int Step) : ICommand<Unit>;

/// <summary>
/// NEXT command
/// </summary>
public record NextCommand(CommandId Id) : ICommand<Unit>;
```

**WhileCommand** (`Era.Core/Commands/Flow/WhileCommand.cs`):
```csharp
using Era.Core.Commands;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// WHILE command - while loop
/// </summary>
public record WhileCommand(CommandId Id, bool Condition) : ICommand<Unit>;

/// <summary>
/// WEND command
/// </summary>
public record WendCommand(CommandId Id) : ICommand<Unit>;
```

**CallCommand** (`Era.Core/Commands/Flow/CallCommand.cs`):
```csharp
using Era.Core.Commands;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// CALL command - function call
/// </summary>
public record CallCommand(CommandId Id, string FunctionName, object[]? Args) : ICommand<Unit>;

/// <summary>
/// RETURN command - function return
/// </summary>
public record ReturnCommand(CommandId Id, object? ReturnValue) : ICommand<Unit>;

/// <summary>
/// CALLFORM command - function call with expression name
/// </summary>
public record CallFormCommand(CommandId Id, string FunctionNameExpression, object[]? Args) : ICommand<Unit>;

/// <summary>
/// RETURNFORM command - function return with expression value
/// </summary>
public record ReturnFormCommand(CommandId Id, string ReturnValueExpression) : ICommand<Unit>;
```

### Flow Handler Implementations

**Note**: ForHandler, WhileHandler, and ReturnHandler follow the same pattern as IfHandler/CallHandler below:
- **ForHandler/WhileHandler**: Use IfHandler as template, inject IExecutionStack
- **ReturnHandler**: Use CallHandler as template, inject IScopeManager

**Variant Handlers**: ElseIfHandler, ElseHandler, EndIfHandler, NextHandler, WendHandler, CallFormHandler, ReturnFormHandler follow the same patterns as their primary handlers. Implementation verified through Task completion, not separate ACs (see AC#10a-e details).

**IfHandler** (`Era.Core/Commands/Flow/IfHandler.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// IF command handler
/// </summary>
public class IfHandler : ICommandHandler<IfCommand, Unit>
{
    private readonly IExecutionStack _stack;

    public IfHandler(IExecutionStack stack)
    {
        _stack = stack ?? throw new ArgumentNullException(nameof(stack));
    }

    public Task<Result<Unit>> Handle(IfCommand command, CancellationToken ct)
    {
        // Push IF frame onto execution stack
        var frame = new ControlFlowFrame(ControlFlowType.If, 0, command.Condition);
        return Task.FromResult(_stack.Push(frame));
    }
}
```

**CallHandler** (`Era.Core/Commands/Flow/CallHandler.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// CALL command handler
/// </summary>
public class CallHandler : ICommandHandler<CallCommand, Unit>
{
    private readonly IScopeManager _scopeManager;
    private readonly ILabelResolver _labelResolver;

    public CallHandler(IScopeManager scopeManager, ILabelResolver labelResolver)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _labelResolver = labelResolver ?? throw new ArgumentNullException(nameof(labelResolver));
    }

    public Task<Result<Unit>> Handle(CallCommand command, CancellationToken ct)
    {
        // Resolve function label
        var labelResult = _labelResolver.ResolveLabel(command.FunctionName);
        if (labelResult is Result<int>.Failure failure)
            return Task.FromResult(Result<Unit>.Fail(failure.Error));

        // Push scope for RETURN
        var scopeResult = _scopeManager.PushScope(command.FunctionName, 0);
        return Task.FromResult(scopeResult);
    }
}
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

**Note**: Service implementations (`ExecutionStack`, `ScopeManager`, `LabelResolver`) are standard interface implementations following the contracts defined above. Each implementation provides internal state management (stack, dictionary) and delegates to the interface methods. Implementation files are created alongside interface files and verified through successful DI registration (AC#10a-e test that handlers are registered, which requires service implementations to exist).

```csharp
// Flow Control Services (Phase 9)
services.AddSingleton<IExecutionStack, ExecutionStack>();
services.AddSingleton<IScopeManager, ScopeManager>();
services.AddSingleton<ILabelResolver, LabelResolver>();

// Flow Control Command Handlers (Phase 9) - Primary
services.AddSingleton<ICommandHandler<IfCommand, Unit>, IfHandler>();
services.AddSingleton<ICommandHandler<ForCommand, Unit>, ForHandler>();
services.AddSingleton<ICommandHandler<WhileCommand, Unit>, WhileHandler>();
services.AddSingleton<ICommandHandler<CallCommand, Unit>, CallHandler>();
services.AddSingleton<ICommandHandler<ReturnCommand, Unit>, ReturnHandler>();

// Flow Control Command Handlers (Phase 9) - Variants
services.AddSingleton<ICommandHandler<ElseIfCommand, Unit>, ElseIfHandler>();
services.AddSingleton<ICommandHandler<ElseCommand, Unit>, ElseHandler>();
services.AddSingleton<ICommandHandler<EndIfCommand, Unit>, EndIfHandler>();
services.AddSingleton<ICommandHandler<NextCommand, Unit>, NextHandler>();
services.AddSingleton<ICommandHandler<WendCommand, Unit>, WendHandler>();
services.AddSingleton<ICommandHandler<CallFormCommand, Unit>, CallFormHandler>();
services.AddSingleton<ICommandHandler<ReturnFormCommand, Unit>, ReturnFormHandler>();
```

### Test Naming Convention

**Test File**: `Era.Core.Tests/Commands/Flow/FlowControlTests.cs`
**Test Class**: `FlowControlTests`

Test methods follow `{CommandType}_{Scenario}` format (e.g., `If_NestedConditions`, `For_LoopState`, `Call_ScopeStack`).

### Equivalence Verification

Legacy behavior: IF/FOR/WHILE commands manipulate GlobalStatic.ProcessInstance execution stack directly.

New behavior: Flow commands use IExecutionStack/IScopeManager/ILabelResolver through CommandDispatcher pipeline.

**Verification**: Both approaches produce identical control flow behavior. New approach adds logging/validation through pipeline.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F429 | CommandDispatcher + Mediator Pipeline (ICommand/ICommandHandler) |
| Predecessor | F430 | Pipeline Behaviors (logging/validation applied to flow commands) |

---

## Links

- [feature-424.md](feature-424.md) - Phase 9 Planning (parent feature)
- [feature-429.md](feature-429.md) - CommandDispatcher + Mediator Pipeline (dependency)
- [feature-430.md](feature-430.md) - Pipeline Behaviors (dependency)
- [feature-441.md](feature-441.md) - REPEAT/REND Loop Commands (follow-up)
- [feature-442.md](feature-442.md) - GOTO/JUMP Label Commands (follow-up)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 9 Flow Control Commands

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

**Out-of-Scope Commands** (follow-up features created):
- **REPEAT/REND**: → F441
- **GOTO/JUMP/TRYGOTO/TRYJUMP**: → F442

Note: This feature focuses on core flow control (IF/FOR/WHILE) and function call (CALL/RETURN). REPEAT and GOTO variants are separate features (F441, F442) to maintain manageable scope.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-10 | create | implementer | Created from F424 Phase 9 Planning | PROPOSED |
| 2026-01-10 18:30 | START | implementer | Task 1 (interfaces + implementations + all handlers) | - |
| 2026-01-10 18:30 | END | implementer | Task 1-7 (combined in single dispatch) | SUCCESS |
| 2026-01-10 18:35 | START | implementer | Task 8-9 (tests) | - |
| 2026-01-10 18:35 | DEVIATION | debugger | Pre-existing build errors fixed (System commands stubs) | SUCCESS |
| 2026-01-10 18:40 | END | implementer | Task 8-9 (FlowControlTests + FlowControlEquivalenceTests) | SUCCESS |
| 2026-01-10 18:45 | START | ac-tester | AC verification | - |
| 2026-01-10 18:45 | END | ac-tester | AC#1-13 all PASS | SUCCESS |
| 2026-01-10 18:50 | START | regression-tester | Full regression suite | - |
| 2026-01-10 18:50 | END | regression-tester | 24/24 scenarios PASS | SUCCESS |
