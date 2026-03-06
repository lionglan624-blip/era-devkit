# Feature 442: GOTO/JUMP Label Commands

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

Migrate GOTO/JUMP/TRYGOTO/TRYJUMP label commands from legacy GameProc to ICommand/ICommandHandler pattern.

**Control Flow Responsibility**: Label jump commands migrated to unified command infrastructure using ILabelResolver.

**Output**: Command implementations in `Era.Core/Commands/Flow/` (GotoCommand.cs, JumpCommand.cs, TryGotoCommand.cs, TryJumpCommand.cs)

---

## Background

### Philosophy (Mid-term Vision)

**Phase 9: Command Infrastructure** - Establish ILabelResolver as SSOT for local label jumps (`$LABEL` within function) and extend IScopeManager for cross-function calls with IsJump semantics (`@FUNCTION` with special RETURN behavior).

**Design Rationale**:
- **GOTO vs CALL/JUMP**: GOTO jumps to `$LABEL` within current function (no scope change). CALL/JUMP call `@FUNCTION` with scope push. Separate handlers for different target types (label vs function).
- **JUMP vs CALL**: Both push scope via IScopeManager. JUMP sets IsJump=true causing special RETURN behavior (double-pop to replace caller's scope). CALL sets IsJump=false (normal push/pop). Separate handlers for different return semantics.
- **TRY variants**: Return `Result<bool>` where the inner bool indicates label/function found (true) or not found (false). The Result wrapper is always Ok - failure is communicated via the bool value, not Result.Fail. Different return type requires separate handler classes per ICommandHandler<TCommand, TResult> pattern.
- **Maintainability**: Single responsibility per handler enables isolated testing and independent modification of scope/error behavior.

### Problem (Current Issue)

GOTO/JUMP commands scattered in GameProc with different resolution mechanisms:
- GOTO jumps to `$LABEL` within current function (uses ILabelResolver)
- JUMP calls `@FUNCTION` with IsJump=true flag (uses function resolution, not label resolution)
- TRYGOTO/TRYJUMP are safe variants returning success/failure
- Uses ILabelResolver (for GOTO) and function lookup (for JUMP) infrastructure

### Goal (What to Achieve)

1. **Migrate GOTO/JUMP/TRYGOTO/TRYJUMP commands** to ICommand/ICommandHandler pattern
2. **Leverage ILabelResolver** from F432 for label resolution
3. **Equivalence verification** - Match legacy behavior:
   - GOTO: Jumps to `$LABEL` within current function, no scope change
   - JUMP: Calls `@FUNCTION` with IsJump=true, pushes scope with special RETURN behavior
   - TRYGOTO: Returns true/false, never throws on missing label
   - TRYJUMP: Returns true/false, never throws on missing function

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | GOTO/JUMP command files exist | file | Glob | count_equals | 4 command files in Flow/ | [ ] |
| 2 | GOTO command handler exists | file | Grep | contains | "class GotoHandler" | [ ] |
| 3 | JUMP command handler exists | file | Grep | contains | "class JumpHandler" | [ ] |
| 4 | TRYGOTO command handler exists | file | Grep | contains | "class TryGotoHandler" | [ ] |
| 5 | TRYJUMP command handler exists | file | Grep | contains | "class TryJumpHandler" | [ ] |
| 6a | GOTO handlers use ILabelResolver | file | Grep | contains | "ILabelResolver" | [ ] |
| 6b | JUMP handlers use IScopeManager | file | Grep | contains | "IScopeManager" | [ ] |
| 7 | Interface extensions (ILabelResolver/IScopeManager) | file | Grep | contains | "ResolveLocalLabel" | [ ] |
| 8 | DI registration (GotoHandler) | file | Grep | contains | "GotoHandler" | [ ] |
| 9 | DI registration (JumpHandler) | file | Grep | contains | "JumpHandler" | [ ] |
| 10 | DI registration (TryGotoHandler) | file | Grep | contains | "TryGotoHandler" | [ ] |
| 11 | DI registration (TryJumpHandler) | file | Grep | contains | "TryJumpHandler" | [ ] |
| 12 | GOTO/JUMP unit test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~GotoJumpTests" | [ ] |
| 13 | GOTO/JUMP equivalence verification | file | Grep | contains | "GotoJumpEquivalence" | [ ] |
| 14 | Zero technical debt | file | Grep | not_contains | - | [ ] |

### AC Details

**AC#1**: GOTO/JUMP command files existence
- Test: Glob pattern="Era.Core/Commands/Flow/*Command.cs" | grep -c "Goto\|Jump"
- Expected: 4 files (GotoCommand.cs, JumpCommand.cs, TryGotoCommand.cs, TryJumpCommand.cs)

**AC#2-5**: Command handler existence
- AC#2: Grep pattern="class GotoHandler" path="Era.Core/Commands/Flow/"
- AC#3: Grep pattern="class JumpHandler" path="Era.Core/Commands/Flow/"
- AC#4: Grep pattern="class TryGotoHandler" path="Era.Core/Commands/Flow/"
- AC#5: Grep pattern="class TryJumpHandler" path="Era.Core/Commands/Flow/"

**AC#6a**: GOTO handlers use ILabelResolver
- Test: Grep pattern="ILabelResolver" path="Era.Core/Commands/Flow/GotoHandler.cs"
- Also verify TryGotoHandler.cs
- GOTO commands resolve `$LABEL` within current function using ILabelResolver

**AC#6b**: JUMP handlers use ILabelResolver and IScopeManager
- Test: Grep pattern="ILabelResolver" path="Era.Core/Commands/Flow/JumpHandler.cs"
- Also: Grep pattern="IScopeManager" path="Era.Core/Commands/Flow/JumpHandler.cs"
- Also verify TryJumpHandler.cs
- JUMP commands resolve `@FUNCTION` via ILabelResolver then push scope via IScopeManager with isJump=true (per F432 CallHandler pattern)

**AC#7**: Interface extensions (ILabelResolver and IScopeManager)
- Test: Grep pattern="ResolveLocalLabel" path="Era.Core/Interfaces/ILabelResolver.cs"
- Also: Grep pattern="bool isJump" path="Era.Core/Interfaces/IScopeManager.cs"
- Also: Grep pattern="TryPushScope" path="Era.Core/Interfaces/IScopeManager.cs"
- Verifies: (1) ILabelResolver extended with ResolveLocalLabel for $LABEL, (2) IScopeManager extended with isJump parameter, (3) TryPushScope method added
- Note: Optional parameters are source-compatible but binary-breaking; acceptable for internal Era.Core use

**AC#8-11**: DI registration verification (per handler)
- AC#8: Grep pattern="GotoHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- AC#9: Grep pattern="JumpHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- AC#10: Grep pattern="TryGotoHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- AC#11: Grep pattern="TryJumpHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Simple handler name match is sufficient - file scope limits false positives (per F432 AC#10a-e)

**AC#12**: GOTO/JUMP unit tests
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~GotoJumpTests"`
- **Minimum**: 3 Assert statements
- **Test methods**: `Goto_ValidLabel_JumpsToLabel`, `Goto_InvalidLabel_ReturnsError`, `TryGoto_InvalidLabel_ReturnsFalse`, `Jump_ValidFunction_PushesScope`, `TryJump_InvalidFunction_ReturnsFalse`
- **Positive scenarios**: (1) GOTO jumps to valid label, (2) JUMP calls valid function with IsJump, (3) TRYGOTO returns true for valid label
- **Negative scenarios**: (1) GOTO to non-existent label returns error, (2) TRYGOTO to non-existent label returns false without error, (3) TRYJUMP to non-existent function returns false without error

**AC#13**: GOTO/JUMP equivalence verification
- Test: Grep pattern="GotoJumpEquivalence" path="Era.Core.Tests/Commands/Flow/"
- Verifies label jump behavior matches legacy GOTO/JUMP/TRYGOTO/TRYJUMP behavior
- **Minimum**: 3 Assert statements
- **Test methods**: `GotoEquivalence_LocalLabelJump`, `JumpEquivalence_FunctionCall`, `TryGotoEquivalence_FailureHandling`, `TryJumpEquivalence_FailureHandling`
- **Positive scenarios**: (1) GOTO local jump matches legacy, (2) JUMP cross-function matches legacy, (3) TRYGOTO/TRYJUMP failure handling matches legacy
- **Negative scenarios**: (1) GOTO to invalid label error matches legacy, (2) JUMP stack state matches legacy

**AC#14**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Flow/*Goto*.cs" and "Era.Core/Commands/Flow/*Jump*.cs"
- Expected: 0 matches in F442-created files only

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 7 | Extend ILabelResolver and IScopeManager interfaces | [x] |
<!-- Note: Must complete before Tasks 2-3 since handlers depend on extended interfaces. Adds ResolveLocalLabel to ILabelResolver and isJump/TryPushScope to IScopeManager -->
| 2 | 1,2,3 | Implement GOTO/JUMP commands and handlers | [x] |
<!-- Batch waiver (Task 2): GOTO/JUMP command records and handlers are created together in same-file edit operation. AC#1 verifies directory structure (file existence), AC#2-3 verify handler classes (Grep existence). All are file-existence checks for semantically paired commands. -->
| 3 | 4,5 | Implement TRYGOTO/TRYJUMP commands and handlers | [x] |
<!-- Batch waiver (Task 3): TRYGOTO/TRYJUMP are semantically paired commands per ERB spec. AC#4-5 are handler checks. -->
| 4 | 6a,6b | Verify GOTO handlers use ILabelResolver, JUMP handlers use IScopeManager | [x] |
| 5 | 8,9,10,11 | Register all label jump handlers in DI | [x] |
<!-- Batch waiver (Task 5): DI registration is atomic operation for all handlers. Per F432 Task#7 precedent. -->
| 6 | 12 | Write GOTO/JUMP unit tests | [x] |
| 7 | 13,14 | Verify GOTO/JUMP equivalence and remove technical debt | [x] |
<!-- Batch waiver (Task 7): AC#13 (equivalence test file) and AC#14 (tech debt check) are final verification steps. Tech debt check runs on code created by implementation tasks. Per F432 Task#9 precedent. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Location**: `engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs` (GOTO_Instruction, CALL_Instruction classes)

| Command | Location | Category | Purpose |
|---------|----------|----------|---------|
| GOTO | Function/Instraction.Child.cs (GOTO_Instruction) | Flow | Jump to label within current function |
| JUMP | Function/Instraction.Child.cs (CALL_Instruction) | Flow | Call function with IsJump=true (special RETURN behavior) |
| TRYGOTO | Function/Instraction.Child.cs (GOTO_Instruction) | Flow | Safe GOTO (returns bool) |
| TRYJUMP | Function/Instraction.Child.cs (CALL_Instruction) | Flow | Safe JUMP (returns bool) |

### Type Dependencies

| Type | Source | Purpose |
|------|--------|---------|
| CommandId | Era.Core/Commands/CommandId.cs | Command identification |
| Unit | Era.Core/Types/Unit.cs | Void return type |
| Result<T> | Era.Core/Types/Result.cs | Result monad |
| ILabelResolver | Era.Core/Interfaces/ILabelResolver.cs | Label resolution for GOTO (from F432, extended with function context) |
| IScopeManager | Era.Core/Interfaces/IScopeManager.cs | Scope management for JUMP (from F432, extended with isJump parameter) |

**Note**: ILabelResolver extended with function-context methods for `$LABEL` resolution:
```csharp
// Existing method (for @FUNCTION resolution)
Result<int> ResolveLabel(string labelName);

// New method for $LABEL within function context (GOTO)
Result<int> ResolveLocalLabel(string labelName, string currentFunctionName);
bool TryResolveLocalLabel(string labelName, string currentFunctionName, out int lineNumber);
```
GOTO uses `ResolveLocalLabel()` to find `$LABEL` within current function scope. CALL/JUMP continue using `ResolveLabel()` for `@FUNCTION` resolution.

**Note**: IScopeManager extended with isJump parameter and TryPushScope:
```csharp
// Extended PushScope with optional isJump (backwards compatible)
Result<Unit> PushScope(string functionName, int returnLine, bool isJump = false);
// New method for TRYJUMP
bool TryPushScope(string functionName, int returnLine, bool isJump = false);
```
When `isJump=true`, RETURN handles scope pop differently (double-pop to replace caller's scope).

### Command Definitions

**GotoCommand** (`Era.Core/Commands/Flow/GotoCommand.cs`):
```csharp
using Era.Core.Commands;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// GOTO command - jump to $LABEL within current function
/// </summary>
public record GotoCommand(CommandId Id, string LabelName, string CurrentFunctionName) : ICommand<Unit>;
```

**JumpCommand** (`Era.Core/Commands/Flow/JumpCommand.cs`):
```csharp
using Era.Core.Commands;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// JUMP command - call function with IsJump=true (special RETURN behavior)
/// </summary>
public record JumpCommand(CommandId Id, string FunctionName) : ICommand<Unit>;
```

**TryGotoCommand** (`Era.Core/Commands/Flow/TryGotoCommand.cs`):
```csharp
using Era.Core.Commands;

namespace Era.Core.Commands.Flow;

/// <summary>
/// TRYGOTO command - safe GOTO returning success/failure
/// </summary>
public record TryGotoCommand(CommandId Id, string LabelName, string CurrentFunctionName) : ICommand<bool>;
```

**TryJumpCommand** (`Era.Core/Commands/Flow/TryJumpCommand.cs`):
```csharp
using Era.Core.Commands;

namespace Era.Core.Commands.Flow;

/// <summary>
/// TRYJUMP command - safe JUMP returning success/failure
/// </summary>
public record TryJumpCommand(CommandId Id, string FunctionName) : ICommand<bool>;
```

### Handler Implementations

**GotoHandler** (`Era.Core/Commands/Flow/GotoHandler.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// GOTO command handler
/// </summary>
public class GotoHandler : ICommandHandler<GotoCommand, Unit>
{
    private readonly ILabelResolver _labelResolver;

    public GotoHandler(ILabelResolver labelResolver)
    {
        _labelResolver = labelResolver ?? throw new ArgumentNullException(nameof(labelResolver));
    }

    public Task<Result<Unit>> Handle(GotoCommand command, CancellationToken ct)
    {
        // Resolve $LABEL within current function context
        var labelResult = _labelResolver.ResolveLocalLabel(command.LabelName, command.CurrentFunctionName);
        if (labelResult is Result<int>.Failure failure)
            return Task.FromResult(Result<Unit>.Fail(failure.Error));

        // Handler validates label exists; actual jump to resolved line number
        // is performed by execution context (interpreter loop) using resolved value
        return Task.FromResult(Result<Unit>.Ok(Unit.Value));
    }
}
```

**JumpHandler** (`Era.Core/Commands/Flow/JumpHandler.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// JUMP command handler - calls function with IsJump=true
/// </summary>
public class JumpHandler : ICommandHandler<JumpCommand, Unit>
{
    private readonly ILabelResolver _labelResolver;
    private readonly IScopeManager _scopeManager;

    public JumpHandler(ILabelResolver labelResolver, IScopeManager scopeManager)
    {
        _labelResolver = labelResolver ?? throw new ArgumentNullException(nameof(labelResolver));
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
    }

    public Task<Result<Unit>> Handle(JumpCommand command, CancellationToken ct)
    {
        // Resolve @FUNCTION (like CallHandler pattern from F432)
        var labelResult = _labelResolver.ResolveLabel(command.FunctionName);
        if (labelResult is Result<int>.Failure failure)
            return Task.FromResult(Result<Unit>.Fail(failure.Error));

        // Push scope with IsJump=true for special RETURN behavior
        var scopeResult = _scopeManager.PushScope(command.FunctionName, 0, isJump: true);
        return Task.FromResult(scopeResult);
    }
}
```

**TryGotoHandler** (`Era.Core/Commands/Flow/TryGotoHandler.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// TRYGOTO command handler - safe GOTO
/// </summary>
public class TryGotoHandler : ICommandHandler<TryGotoCommand, bool>
{
    private readonly ILabelResolver _labelResolver;

    public TryGotoHandler(ILabelResolver labelResolver)
    {
        _labelResolver = labelResolver ?? throw new ArgumentNullException(nameof(labelResolver));
    }

    public Task<Result<bool>> Handle(TryGotoCommand command, CancellationToken ct)
    {
        // TryResolveLocalLabel returns bool - never throws
        var success = _labelResolver.TryResolveLocalLabel(command.LabelName, command.CurrentFunctionName, out _);
        return Task.FromResult(Result<bool>.Ok(success));
    }
}
```

**TryJumpHandler** (`Era.Core/Commands/Flow/TryJumpHandler.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// TRYJUMP command handler - safe JUMP (returns bool, never throws on missing function)
/// </summary>
public class TryJumpHandler : ICommandHandler<TryJumpCommand, bool>
{
    private readonly ILabelResolver _labelResolver;
    private readonly IScopeManager _scopeManager;

    public TryJumpHandler(ILabelResolver labelResolver, IScopeManager scopeManager)
    {
        _labelResolver = labelResolver ?? throw new ArgumentNullException(nameof(labelResolver));
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
    }

    public Task<Result<bool>> Handle(TryJumpCommand command, CancellationToken ct)
    {
        // Try resolve @FUNCTION - return false if not found
        if (!_labelResolver.TryResolveLabel(command.FunctionName, out _))
            return Task.FromResult(Result<bool>.Ok(false));

        // Try push scope with IsJump=true
        var scopeResult = _scopeManager.TryPushScope(command.FunctionName, 0, isJump: true);
        return Task.FromResult(Result<bool>.Ok(scopeResult));
    }
}
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:
```csharp
// GOTO/JUMP Label Command Handlers (Phase 9 - F442)
services.AddSingleton<ICommandHandler<GotoCommand, Unit>, GotoHandler>();
services.AddSingleton<ICommandHandler<JumpCommand, Unit>, JumpHandler>();
services.AddSingleton<ICommandHandler<TryGotoCommand, bool>, TryGotoHandler>();
services.AddSingleton<ICommandHandler<TryJumpCommand, bool>, TryJumpHandler>();
```

### Test Naming Convention

**Test File**: `Era.Core.Tests/Commands/Flow/GotoJumpTests.cs`
**Test Class**: `GotoJumpTests`

Test methods follow `{CommandType}_{Scenario}` format (e.g., `Goto_ValidLabel`, `Jump_CrossFunction`, `TryGoto_InvalidLabel`).

### Equivalence Verification

Legacy behavior: GOTO/JUMP commands use GlobalStatic label resolution inline.

New behavior: GOTO/JUMP use ILabelResolver through CommandDispatcher pipeline.

**Verification**: Both approaches produce identical label jump behavior. New approach adds logging/validation through pipeline.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F432 | Flow Control Commands (ILabelResolver) |

---

## Links

- [feature-432.md](feature-432.md) - Flow Control Commands (parent)
- [index-features.md](index-features.md)

---

## Review Notes

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-10 | create | opus | Created as F432 follow-up | PROPOSED |
| 2026-01-10 20:28 | START | implementer | Task 1 | - |
| 2026-01-10 20:28 | END | implementer | Task 1 | SUCCESS |
| 2026-01-10 20:30 | START | implementer | Task 2 | - |
| 2026-01-10 20:30 | END | implementer | Task 2 | SUCCESS |
| 2026-01-10 20:31 | START | implementer | Task 3 | - |
| 2026-01-10 20:31 | END | implementer | Task 3 | SUCCESS |
| 2026-01-10 20:34 | START | implementer | Task 4 | - |
| 2026-01-10 20:34 | END | implementer | Task 4 | SUCCESS |
| 2026-01-10 20:35 | START | implementer | Task 5 | - |
| 2026-01-10 20:35 | END | implementer | Task 5 | SUCCESS |
| 2026-01-10 20:36 | START | implementer | Task 6 | - |
| 2026-01-10 20:37 | END | implementer | Task 6 | SUCCESS |
| 2026-01-10 20:39 | START | implementer | Task 7 | - |
| 2026-01-10 20:39 | END | implementer | Task 7 | SUCCESS |
