# Feature 023: Process God Object Split Phase 3

## Status: [DONE]

## Overview

Extract CommandDispatcher from Process class, continuing the Process god object decomposition.

Phase 1 (Feature 020): ProcessErrorHandler - exception handling and error recovery
Phase 2 (Feature 021): ProcessInitializer - initialization and resource loading
Phase 3 (this): CommandDispatcher - command dispatch coordination

**Scope Refinement**: After analysis, `runScriptProc()` core execution loop is tightly coupled with ProcessState and skip state. This phase focused on extracting the command dispatch coordination layer instead of the full script executor.

## Problem

Process class remains a ~3,400 line god object across 5 partial files. While the Command Pattern infrastructure (Feature 005) extracted individual command handlers, the command registration and dispatch coordination logic still lived in Process.ScriptProc.cs:

- Command registration (~80 lines)
- Command dispatch coordination (~10 lines per method)
- Skip state management (still in Process, too deeply embedded)

## Goals

1. ~~Extract IScriptExecutor interface~~ → Extract ICommandDispatcher interface
2. Move command registration to CommandDispatcher service
3. Enable DI injection via GlobalStatic pattern (consistent with Feature 020, 021)
4. Maintain 100% backward compatibility

## Acceptance Criteria

- [x] ICommandDispatcher interface defined in Sub/ (7 methods)
- [x] CommandDispatcher class implements command registration and dispatch
- [x] GlobalStatic.CommandDispatcher DI property added
- [x] Process delegates to injected CommandDispatcher
- [x] Unit tests for CommandDispatcher (12 tests)
- [x] Build succeeds
- [x] Regression tests pass (85/85)
- [x] engine-reference.md updated

## Scope

### In Scope
- Extract command registration to CommandDispatcher
- Create ICommandDispatcher interface
- DI registration in GlobalStatic
- Unit tests for extracted service

### Out of Scope
- Skip state extraction (too deeply embedded in Process and commands)
- runScriptProc() core loop extraction (requires significant refactoring)
- Process.State.cs refactoring (separate feature if needed)
- runSystemProc() extraction (may be Phase 4)

## Technical Approach

### Interface Design (Final)
```csharp
internal interface ICommandDispatcher
{
    void InitializeCommands();
    bool TryExecuteCommand(ScriptCommandContext ctx, InstructionLine func);
    (bool found, bool continueExec) TryExecuteFlowCommand(ScriptCommandContext ctx, InstructionLine func);
    bool HasNormalCommand(FunctionCode code);
    bool HasFlowCommand(FunctionCode code);
    int NormalCommandCount { get; }
    int FlowCommandCount { get; }
}
```

### DI Pattern (GlobalStatic)
```csharp
private static ICommandDispatcher _commandDispatcher = new CommandDispatcher();
public static ICommandDispatcher CommandDispatcher
{
    get => _commandDispatcher;
    set => _commandDispatcher = value ?? new CommandDispatcher();
}
```

### Process Delegation
```csharp
// In Process.ScriptProc.cs
private void InitializeCommandRegistry()
{
    GlobalStatic.CommandDispatcher.InitializeCommands();
}

void doNormalFunction(InstructionLine func)
{
    var ctx = GetCommandContext();
    if (GlobalStatic.CommandDispatcher.TryExecuteCommand(ctx, func))
    {
        SyncSkipStateFromContext();
        return;
    }
    // Fallback to switch statement...
}
```

## Effort Estimate

- **Size**: Medium (~80 lines extracted, ~200 lines new)
- **Risk**: Low - focused extraction with minimal coupling
- **Testability**: ★★★★★ (enables command dispatch testing)
- **Sessions**: 1

## Files Changed

### Created
- `uEmuera/Assets/Scripts/Emuera/Sub/ICommandDispatcher.cs` (56 lines)
- `uEmuera/Assets/Scripts/Emuera/Sub/CommandDispatcher.cs` (145 lines)
- `uEmuera/Tests/CommandDispatcherTests.cs` (196 lines)

### Modified
- `uEmuera/Assets/Scripts/Emuera/GlobalStatic.cs` - Add DI property
- `uEmuera/Assets/Scripts/Emuera/GameProc/Process.ScriptProc.cs` - Delegate to dispatcher (~80 lines removed)
- `Game/agents/reference/engine-reference.md` - Document new interface

## Dependencies

- Feature 005 [DONE]: Command Pattern infrastructure (used by CommandDispatcher)
- Feature 020 [DONE]: ProcessErrorHandler (error handling interface)
- Feature 021 [DONE]: ProcessInitializer (initialization interface)

## Links

- [index-features.md](../index-features.md) - Feature tracking
- [WBS-023.md](WBS-023.md) - Work breakdown structure
- [WBS-020.md](WBS-020.md) - Phase 1 reference
- [WBS-021.md](WBS-021.md) - Phase 2 reference
- [engine-reference.md](../reference/engine-reference.md) - Architecture docs
