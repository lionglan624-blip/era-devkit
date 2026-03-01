# Feature 480: ProcessState Execution State Machine

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

**Phase 14: Era.Core Engine - ProcessState Implementation**

Implement ProcessState execution state machine for managing function call stack, local variable scope, and execution context in ERA scripting environment.

**Output**:
- `Era.Core/Process/IProcessState.cs` - Execution state machine interface
- `Era.Core/Process/ProcessState.cs` - Execution state machine (call stack, scope management)
- `Era.Core/Process/CallStack.cs` - CALL/RETURN address management
- `Era.Core/Process/ExecutionContext.cs` - Current execution position and system state
- Unit tests in `Era.Core.Tests/Process/ProcessStateTests.cs`

**Volume**: ~300 lines total

---

## Background

### Philosophy (Mid-term Vision)

**Phase 14: Era.Core Engine** - Establish pure C# game engine with headless execution capability. ProcessState forms the execution state machine foundation, enabling proper function call/return semantics and local variable scope management for ERA scripting.

### Problem (Current Issue)

ERA scripts use CALL/RETURN/JUMP control flow with local variable scopes, but no C# implementation exists to manage execution state. Without ProcessState, the engine cannot:
- Track function call stack for CALL/RETURN
- Manage local variable scopes (@LOCAL)
- Maintain execution position through JUMP
- Handle nested function calls

**Note on existing stacks**: IExecutionStack manages control flow frames (IF/FOR/WHILE/REPEAT block nesting). ProcessState.CallStack manages function call return addresses (CALL/RETURN). These are orthogonal concerns: control flow nesting vs function call nesting.

### Goal (What to Achieve)

1. **Implement IProcessState interface** - Execution state machine abstraction
2. **Implement ProcessState** - Call stack and scope management
3. **Implement CallStack** - CALL/RETURN address tracking
4. **Implement ExecutionContext** - Current execution position
5. **Register in DI** - ServiceCollectionExtensions integration
6. **Test state transitions** - Unit tests for push/pop operations
7. **Eliminate technical debt** - Zero TODO/FIXME/HACK comments

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IProcessState.cs exists | file | Glob | exists | "Era.Core/Process/IProcessState.cs" | [x] |
| 2 | ProcessState.cs exists | file | Glob | exists | "Era.Core/Process/ProcessState.cs" | [x] |
| 3 | CallStack.cs exists | file | Glob | exists | "Era.Core/Process/CallStack.cs" | [x] |
| 4 | ExecutionContext.cs exists | file | Glob | exists | "Era.Core/Process/ExecutionContext.cs" | [x] |
| 5 | IProcessState interface exists | code | Grep | contains | "public interface IProcessState" | [x] |
| 6 | ProcessState implements interface | code | Grep | contains | "class ProcessState : IProcessState" | [x] |
| 7 | DI registration | file | Grep | contains | "AddSingleton.*IProcessState.*ProcessState" | [x] |
| 8 | CallStack push/pop test | test | Bash | succeeds | "dotnet test --filter CallStackPushPop" | [x] |
| 9 | Scope coordination with IVariableScope | test | Bash | succeeds | "dotnet test --filter LocalScope" | [x] |
| 10 | Nested call test | test | Bash | succeeds | "dotnet test --filter NestedCall" | [x] |
| 11 | Invalid pop (Neg) | test | Bash | succeeds | "dotnet test --filter InvalidPop" | [x] |
| 12 | Build succeeds | build | Bash | succeeds | "dotnet build Era.Core" | [x] |
| 13 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [x] |

### AC Details

**AC#1**: IProcessState.cs file existence
- Test: Glob pattern="Era.Core/Process/IProcessState.cs"
- Expected: File exists
- Verifies: Interface file created

**AC#2**: ProcessState.cs file existence
- Test: Glob pattern="Era.Core/Process/ProcessState.cs"
- Expected: File exists
- Verifies: Primary implementation file created

**AC#3**: CallStack.cs file existence
- Test: Glob pattern="Era.Core/Process/CallStack.cs"
- Expected: File exists
- Verifies: Call stack component created

**AC#4**: ExecutionContext.cs file existence
- Test: Glob pattern="Era.Core/Process/ExecutionContext.cs"
- Expected: File exists
- Verifies: Execution context component created

**AC#5**: IProcessState interface definition
- Test: Grep pattern="public interface IProcessState" path="Era.Core/Process/IProcessState.cs"
- Expected: Contains interface definition
- Verifies: Abstraction defined for DI

**AC#6**: ProcessState implementation
- Test: Grep pattern="class ProcessState : IProcessState" path="Era.Core/Process/ProcessState.cs"
- Expected: Contains implementation declaration
- Verifies: Interface implemented

**AC#7**: DI registration in ServiceCollectionExtensions
- Test: Grep pattern="AddSingleton.*IProcessState.*ProcessState" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: Contains DI registration
- Verifies: Service registered for injection
- Note: ProcessState receives IVariableScope via constructor injection; DI container resolves automatically

**AC#8**: CallStack push/pop operations (behavior test)
- Test: `dotnet test --filter CallStackPushPop`
- Test cases: Push address, Pop returns same address, Stack depth tracking
- Expected: All push/pop tests pass
- Verifies: Basic call stack operations work correctly

**AC#9**: Local variable scope isolation (behavior test)
- Test: `dotnet test --filter LocalScope`
- Test cases: ProcessState delegates @LOCAL storage to IVariableScope. PushScope/PopScope coordinate with IVariableScope.PushLocal/PopLocal.
- Expected: All scope tests pass
- Verifies: Local variables properly scoped via IVariableScope integration

**AC#10**: Nested function call handling (behavior test)
- Test: `dotnet test --filter NestedCall`
- Test cases: CALL A -> CALL B -> RETURN B -> RETURN A sequence
- Expected: All nested call tests pass
- Verifies: Multiple call stack levels managed correctly

**AC#11**: Invalid pop error handling (Negative test)
- Test: `dotnet test --filter InvalidPop`
- Test cases:
  - PopCall on empty call stack returns Result.Fail with message "Cannot pop from empty call stack"
  - PopScope on empty scope stack returns Result.Fail with message "Cannot pop from empty scope stack"
- Expected: All invalid pop tests pass with correct error messages
- Verifies: Proper error handling for empty stack with documented error messages

**AC#12**: Build verification
- Test: `dotnet build Era.Core`
- Expected: Build succeeded
- Verifies: No compile errors

**AC#13**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Process/" -i (case-insensitive)
- Expected: 0 matches
- Verifies: Clean implementation without deferred work
- Note: Grep is case-sensitive by default; -i flag ensures all case variants are caught

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4 | Create Process/ directory and component files (IProcessState, ProcessState, CallStack, ExecutionContext) | [x] |
| 2 | 5 | Define IProcessState interface with PushCall/PopCall/PushScope/PopScope methods | [x] |
| 3 | 6 | Implement ProcessState with CallStack and scope management | [x] |
| 4 | 3 | Implement CallStack with address stack and depth tracking | [x] |
| 5 | 4 | Implement ExecutionContext with current position and system state | [x] |
| 6 | 7 | Register IProcessState in ServiceCollectionExtensions.cs | [x] |
| 7 | 8,9,10,11 | Write unit tests (push/pop, scope isolation, nested calls, negative cases) | [x] |
| 8 | 12 | Verify build succeeds with all changes | [x] |
| 9 | 13 | Remove all TODO/FIXME/HACK comments from Process/ directory | [x] |

<!-- AC:Task 1:1 Rule: 13 ACs = 9 Tasks (Task#1 creates files, Task#4/5 implement logic, Task#7 covers AC#8-11 as unified test suite) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Interface Definition

File: `Era.Core/Process/IProcessState.cs`

```csharp
using Era.Core.Types;

namespace Era.Core.Process;

/// <summary>
/// Execution state machine for managing function call stack and local variable scopes.
/// </summary>
public interface IProcessState
{
    /// <summary>Push function call onto stack with return address</summary>
    /// <param name="returnAddress">Address to return to after function completes</param>
    Result<Unit> PushCall(int returnAddress);

    /// <summary>Pop function call from stack and return address</summary>
    /// <returns>Result containing return address or error if stack empty</returns>
    Result<int> PopCall();

    /// <summary>Push new local variable scope</summary>
    Result<Unit> PushScope();

    /// <summary>Pop local variable scope, clearing all @LOCAL variables in scope</summary>
    Result<Unit> PopScope();

    /// <summary>Get current call stack depth</summary>
    int CallDepth { get; }

    /// <summary>Get current scope depth</summary>
    int ScopeDepth { get; }
}
```

### Error Message Format

**Error Messages**: Return `Result.Fail` for invalid operations with format:
- Empty stack pop: `"Cannot pop from empty call stack"`
- Empty scope pop: `"Cannot pop from empty scope stack"`

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IProcessState, ProcessState>();
```

**Prerequisites**: IVariableScope must be registered before IProcessState. Current registration order in ServiceCollectionExtensions.cs already satisfies this (IVariableScope at line 56). No additional registration changes needed beyond adding IProcessState.

### ProcessState Implementation Stub

File: `Era.Core/Process/ProcessState.cs`

```csharp
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Process;

/// <summary>
/// Execution state machine implementation.
/// </summary>
public class ProcessState : IProcessState
{
    private readonly IVariableScope _variableScope;
    private readonly CallStack _callStack = new();
    private int _scopeDepth;

    public ProcessState(IVariableScope variableScope)
    {
        _variableScope = variableScope;
    }

    public int CallDepth => _callStack.Depth;
    public int ScopeDepth => _scopeDepth;

    public Result<Unit> PushCall(int returnAddress)
    {
        _callStack.Push(returnAddress);
        return Result<Unit>.Ok(Unit.Value);
    }

    public Result<int> PopCall()
    {
        if (_callStack.Depth == 0)
            return Result<int>.Fail("Cannot pop from empty call stack");
        return Result<int>.Ok(_callStack.Pop());
    }

    public Result<Unit> PushScope()
    {
        _scopeDepth++;
        _variableScope.PushLocal();
        return Result<Unit>.Ok(Unit.Value);
    }

    public Result<Unit> PopScope()
    {
        if (_scopeDepth == 0)
            return Result<Unit>.Fail("Cannot pop from empty scope stack");
        _scopeDepth--;
        _variableScope.PopLocal();
        return Result<Unit>.Ok(Unit.Value);
    }
}
```

### CallStack Implementation Stub

File: `Era.Core/Process/CallStack.cs`

```csharp
namespace Era.Core.Process;

/// <summary>
/// Manages function call return addresses.
/// Internal helper class used by ProcessState.
/// </summary>
public class CallStack
{
    private readonly Stack<int> _addresses = new();

    /// <summary>Current stack depth</summary>
    public int Depth => _addresses.Count;

    /// <summary>Push return address onto stack</summary>
    public void Push(int returnAddress) => _addresses.Push(returnAddress);

    /// <summary>Pop return address from stack</summary>
    /// <returns>Return address</returns>
    /// <exception cref="InvalidOperationException">If stack is empty</exception>
    public int Pop() => _addresses.Pop();
}
```

### Implementation Requirements

| Requirement | Specification |
|-------------|---------------|
| Thread safety | NOT required - single-threaded execution per game instance |
| Call stack limit | No artificial limit - rely on .NET stack |
| Scope isolation | Scopes are independent - popping scope clears only that scope's variables |
| IVariableScope dependency | ProcessState constructor receives IVariableScope via DI |
| Result wrapping | IVariableScope.PushLocal/PopLocal return void. ProcessState wraps as Result<Unit>.Ok(Unit.Value). Result.Fail only returned when ScopeDepth is 0 on PopScope attempt. |

### Scope Management Design

ProcessState manages scope lifecycle but **delegates @LOCAL variable storage to IVariableScope**.

**Architecture**:
- ProcessState has `PushScope()` / `PopScope()` methods
- Internally, ProcessState coordinates with `IVariableScope` (injected via DI)
- When `PushScope()` is called: ProcessState calls `IVariableScope.PushLocal()`
- When `PopScope()` is called: ProcessState calls `IVariableScope.PopLocal()`
- @LOCAL variable access goes through `IVariableScope.GetLocal()` / `SetLocal()`

**Scope Lifecycle Example** (nested CALL scenario):
```
CALL A:
  ProcessState.PushCall(returnAddr)  → Call stack: [A]
  ProcessState.PushScope()           → IVariableScope.PushLocal()
  SetLocal("X", 1)                   → IVariableScope.SetLocal("X", 1) in scope level 1

  CALL B:
    ProcessState.PushCall(returnAddr) → Call stack: [A, B]
    ProcessState.PushScope()          → IVariableScope.PushLocal()
    SetLocal("X", 2)                  → IVariableScope.SetLocal("X", 2) in scope level 2

  RETURN B:
    ProcessState.PopScope()           → IVariableScope.PopLocal() clears scope 2
    ProcessState.PopCall()            → Call stack: [A]
    GetLocal("X")                     → Returns 1 (scope 1 still valid)

RETURN A:
  ProcessState.PopScope()             → IVariableScope.PopLocal() clears scope 1
  ProcessState.PopCall()              → Call stack: []
```

### ExecutionContext Role

**ExecutionContext is a separate file but internal to ProcessState (not exposed via IProcessState interface).**

Why separate file but internal:
- **Separation of Concerns**: ExecutionContext handles execution position tracking; CallStack handles return addresses; ProcessState orchestrates both
- **Testability**: ExecutionContext can be unit-tested independently
- **Future Extensibility**: May be exposed via interface in future phases when JUMP semantics are fully implemented

Current scope:
- ExecutionContext tracks current execution position (program counter) and system state
- ProcessState uses ExecutionContext internally for position management
- AC#4 verifies file creation; behavior testing is integrated via ProcessState tests

### ExecutionContext Implementation Stub

File: `Era.Core/Process/ExecutionContext.cs`

```csharp
namespace Era.Core.Process;

/// <summary>
/// Tracks current execution position and system state.
/// Internal to ProcessState - not exposed via interface.
/// </summary>
public class ExecutionContext
{
    /// <summary>Current program counter (line number or address)</summary>
    public int ProgramCounter { get; set; }

    /// <summary>Current function/label name being executed</summary>
    public string CurrentFunction { get; set; } = string.Empty;

    /// <summary>System execution state</summary>
    public ExecutionState State { get; set; } = ExecutionState.Running;
}

public enum ExecutionState
{
    Running,
    WaitingInput,
    Paused,
    Terminated
}
```

### Test Naming Convention

Test methods follow `Test{Operation}{Scenario}` format:
- `TestCallStackPushPop` - Basic push/pop operations
- `TestLocalScopeIsolation` - Scope variable isolation
- `TestNestedCallHandling` - Multi-level calls
- `TestInvalidPopReturnsError` - Negative case

This ensures AC filter patterns match correctly.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F471 | Phase 14 Planning must complete first |
| Successor | F481 | InputHandler depends on ProcessState for managing input context |

---

## Links

- [feature-471.md](feature-471.md) - Phase 14 Planning (parent feature)
- [feature-481.md](feature-481.md) - InputHandler (successor feature)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 14 Task 7
- [feature-template.md](reference/feature-template.md) - Feature template

---

## 残課題 (Deferred Tasks)

None.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-13 | create | spec-writer | Created from F471 Phase 14 Planning | PROPOSED |
| 2026-01-13 19:59 | START | implementer | Phase 3 TDD - Task 1-9 | - |
| 2026-01-13 19:59 | END | implementer | Phase 3 TDD - Task 1-9 | SUCCESS |
| 2026-01-13 | START | ac-tester | Phase 6 AC verification | - |
| 2026-01-13 | END | ac-tester | Phase 6 AC verification | SUCCESS (13/13 PASS) |
| 2026-01-13 | START | feature-reviewer | Phase 7 post-review | - |
| 2026-01-13 | END | feature-reviewer | Phase 7 post-review | READY |
| 2026-01-13 | DEVIATION | feature-reviewer | Phase 7 doc-check | NEEDS_REVISION |
| 2026-01-13 | - | opus | Fix engine-dev SKILL.md (add Process/) | - |
| 2026-01-13 | END | feature-reviewer | Phase 7 doc-check (retry) | READY |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-13 FL iter1**: [resolved] Phase2-Validate - Task#4 AC# column: Fixed - Task#4 now maps to AC#3 (file exists).
- **2026-01-13 FL iter1**: [resolved] Phase2-Validate - Task#5 AC# column: Fixed - Task#5 now maps to AC#4 (file exists). ExecutionContext role documented.
- **2026-01-13 FL iter2**: [resolved] Phase2-Validate - AC#9 Description: Updated to "Scope coordination with IVariableScope".
- **2026-01-13 FL iter3**: [accepted] Phase2-Validate - AC#8-11 filter patterns: dotnet test --filter uses substring matching, current patterns work.
- **2026-01-13 FL iter3**: [accepted] Phase2-Validate - Review Notes format: "Phase2-Validate" terminology is internal notation.
- **2026-01-13 FL iter6**: [accepted] Phase2-Validate - ExecutionContext stub: Minimal stub provided; full implementation left to implementer.
- **2026-01-13 FL iter6**: [accepted] Phase2-Validate - Task comment: Current 13 ACs = 9 Tasks mapping is technically correct.
