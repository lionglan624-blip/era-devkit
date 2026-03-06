# Feature 441: REPEAT/REND Loop Commands

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

Migrate REPEAT/REND loop commands from legacy GameProc to ICommand/ICommandHandler pattern.

**Control Flow Responsibility**: REPEAT loop command migrated to unified command infrastructure.

**Output**: Command implementations in `Era.Core/Commands/Flow/`

---

## Background

### Philosophy (Mid-term Vision)

**Phase 9: Command Infrastructure** - Establish IExecutionStack as the single source of truth (SSOT) for all flow control state management in Era.Core. REPEAT/REND extends flow control commands with iteration counting semantics. Integration with COUNT:0 system variable requires IGameState extension, bridging Phase 9 (Command Infrastructure) with Phase 11 (Variable Access) concerns. Stub approach maintains separation while enabling testable REPEAT implementation.

### Problem (Current Issue)

REPEAT/REND loop command scattered in GameProc with counter state management:
- REPEAT requires counter tracking
- REND decrements counter and loops back
- Uses same execution stack as IF/FOR/WHILE (F432)

### Goal (What to Achieve)

1. **Migrate REPEAT/REND commands** to ICommand/ICommandHandler pattern
2. **Extend ControlFlowType enum** with Repeat variant
3. **Equivalence verification** - REPEAT loop matches legacy behavior

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | REPEAT command handler exists | file | Grep | contains | "class RepeatHandler" | [x] |
| 2 | REND command handler exists | file | Grep | contains | "class RendHandler" | [x] |
| 3 | ControlFlowType.Repeat exists | file | Grep | contains | "Repeat" | [x] |
| 4 | IGameState.SetVariable interface | file | Grep | contains | "SetVariable" in IGameState.cs | [x] |
| 5 | GameState.SetVariable implementation | file | Grep | contains | "SetVariable" in GameState.cs | [x] |
| 6 | DI registration (RepeatHandler) | file | Grep | contains | "RepeatHandler" | [x] |
| 7 | DI registration (RendHandler) | file | Grep | contains | "RendHandler" | [x] |
| 8 | REPEAT unit test | test | - | succeeds | dotnet test --filter RepeatTests | [x] |
| 9 | REPEAT equivalence verification | file | Grep | contains | "RepeatEquivalence" | [x] |
| 10 | Zero technical debt | file | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |

### AC Details

**AC#1**: REPEAT command handler
- Test: Grep pattern="class RepeatHandler" path="Era.Core/Commands/Flow/"

**AC#2**: REND command handler
- Test: Grep pattern="class RendHandler" path="Era.Core/Commands/Flow/"

**AC#3**: ControlFlowType enum extension
- Test: Grep pattern="Repeat" path="Era.Core/Interfaces/IExecutionStack.cs"
- Adds `Repeat` to existing ControlFlowType enum

**AC#4**: IGameState.SetVariable interface
- Test: Grep pattern="SetVariable" path="Era.Core/Interfaces/IGameState.cs"
- Adds SetVariable method signature to IGameState interface

**AC#5**: GameState.SetVariable implementation
- Test: Grep pattern="SetVariable" path="Era.Core/Commands/System/GameState.cs"
- Implements SetVariable method (stub for Phase 11 integration)

**AC#6-7**: DI registration verification
- AC#6: Grep pattern="RepeatHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- AC#7: Grep pattern="RendHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"

**AC#8**: REPEAT unit tests
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~RepeatTests"`
- **Minimum**: 3 Assert statements
- **Test Setup**: Mock IGameState.SetVariable to return Ok (production stub returns Fail). Verify SetVariable called with correct arguments (name="COUNT", index=0, value=counter).
- **Positive scenarios**: (1) REPEAT pushes frame with counter, (2) REND increments counter and loops, (3) REPEAT 0 returns Ok without calling SetVariable or pushing frame, (4) Nested REPEAT: inner REND exit restores outer COUNT
- **Negative scenarios**: (1) REND without REPEAT returns error, (2) REND on non-REPEAT frame (e.g., FOR/WHILE) returns error

**AC#9**: REPEAT equivalence verification
- Test: Grep pattern="RepeatEquivalence" path="Era.Core.Tests/Commands/Flow/"
- **Minimum**: 3 Assert statements
- **Positive scenarios**: (1) REPEAT N initializes frame with State=(0, N), (2) N consecutive REND calls produce counter sequence 0→1→...→N-1→exit, (3) Nested REPEAT: outer COUNT restored when inner exits
- **Negative scenarios**: (1) REPEAT -1 executes zero times (same as REPEAT 0), (2) REPEAT with large N - verify no overflow in counter logic

**AC#10**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Flow/"
- **Paths checked**: Era.Core/Commands/Flow/RepeatCommand.cs, Era.Core/Commands/Flow/RepeatHandler.cs, Era.Core/Commands/Flow/RendHandler.cs
- Expected: 0 matches in new files
- Note: IGameState.cs and GameState.cs additions are single-line method declarations (no TODO risk)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4,5 | Implement REPEAT/REND commands, handlers, and IGameState extension | [x] |
<!-- Batch waiver (Task 1): Related REPEAT/REND command implementation per F432 precedent. AC#1-3 are file-existence checks (Grep). AC#4-5 (IGameState.SetVariable interface + impl) is co-located because it's a dependency for REPEAT COUNT integration - single method addition. -->
| 2 | 6,7 | Register REPEAT handlers in DI | [x] |
<!-- Batch waiver (Task 2): DI registration for related handlers per F432 precedent. -->
| 3 | 8,9,10 | Write REPEAT tests and verify equivalence | [x] |
<!-- Batch waiver (Task 3): AC#8 (unit test), AC#9 (equivalence test file), AC#10 (tech debt check) are final verification steps per F432 precedent. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Locations**:
- `engine/Assets/Scripts/Emuera/GameProc/Process.ScriptProc.cs` (flow control inline)
- `engine/Assets/Scripts/Emuera/GameProc/ArgumentBuilder.cs` (REPEAT argument parsing, COUNT setup)
- `engine/Assets/Scripts/Emuera/GameData/Instraction.Child.cs` (REND loop logic)

| Command | Location | Purpose |
|---------|----------|---------|
| REPEAT | Process.ScriptProc.cs | Repeat loop with counter |
| REPEAT | ArgumentBuilder.cs:947-957 | COUNT variable setup, SpForNextArgment |
| REND | Process.ScriptProc.cs | End repeat loop |
| REND | Instraction.Child.cs:2198-2224 | Loop increment and exit check |

### Counter Semantics (Post-F443 Design Decision)

**Counter direction**: Countup (0→N-1) - REQUIRED for legacy compatibility

**State storage**: ControlFlowFrame.State stores C# value tuple `(int Counter, int LoopEnd)`:
- `Frame.State`: Value tuple with Counter (current iteration 0 to N-1) and LoopEnd (target count N)
- `COUNT:0`: System variable (synchronized with Counter via IGameState.SetVariable)

> **Note**: ControlFlowFrame signature is `(ControlFlowType Type, int StartLine, object? State)`. The State parameter accepts any object, so we use a value tuple `(int Counter, int LoopEnd)` to store both values.

> **Test Strategy**: Production stub returns Fail (SetVariable not implemented). Unit tests mock IGameState with SetVariable returning Ok. Tests verify SetVariable was called with correct arguments (name="COUNT", index=0, value=counter). Actual COUNT:0 synchronization awaits Phase 11.

**Loop logic**:
```
REPEAT N:
  Push frame: State=(Counter: 0, LoopEnd: N)
  Set COUNT:0 = 0

REND:
  Extract (Counter, LoopEnd) from State
  Pop current frame
  Increment: newCounter = Counter + 1
  Check: if newCounter >= LoopEnd → exit (frame already popped)
         else → Push updated frame with newCounter, set COUNT:0 = newCounter, continue loop
```

> **Increment Timing**: Counter is incremented BEFORE the exit check. When Counter reaches LoopEnd, the loop has completed N iterations (0 through N-1).

**Example (REPEAT 3)**:
```
Iteration 1: COUNT:0 = 0, State.Counter = 0
Iteration 2: COUNT:0 = 1, State.Counter = 1
Iteration 3: COUNT:0 = 2, State.Counter = 2
After REND: Counter (3) >= LoopEnd (3) → Exit
```

### REPEAT Command Definitions

**RepeatCommand** (`Era.Core/Commands/Flow/RepeatCommand.cs`):

> **File Structure**: RepeatCommand.cs contains both RepeatCommand and RendCommand records (paired commands). RepeatHandler.cs and RendHandler.cs are separate files for handlers (per F432 pattern: command records in one file, handlers in separate files).

```csharp
using Era.Core.Commands;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// REPEAT command - repeat loop with count
/// Counter semantics: Countup (0→N-1) with COUNT:0 system variable integration
/// </summary>
public record RepeatCommand(CommandId Id, int Count) : ICommand<Unit>;

/// <summary>
/// REND command - end repeat loop
/// Increments counter and exits when counter >= LoopEnd
/// </summary>
public record RendCommand(CommandId Id) : ICommand<Unit>;
```

### ControlFlowType Enum Extension

Add `Repeat` to existing enum in `Era.Core/Interfaces/IExecutionStack.cs`:

> **Note**: Remove existing comment `// Repeat: Reserved for future REPEAT/REND feature (out-of-scope for F432)` and add Repeat variant.

```csharp
public enum ControlFlowType
{
    If,
    For,
    While,
    Repeat  // Add for F441
}
```

### IGameState Interface Extension

Add `SetVariable` method to `Era.Core/Interfaces/IGameState.cs`:

> **Additive Change**: This extends IGameState interface with a new method. Existing implementations (GameState.cs) must add the new method. No breaking change to existing consumers - they continue to work with existing methods.

```csharp
/// <summary>Set a variable value by name and index</summary>
/// <param name="name">Variable name (e.g., "COUNT")</param>
/// <param name="index">Array index (e.g., 0 for COUNT:0)</param>
/// <param name="value">Value to set</param>
Result<Unit> SetVariable(string name, int index, long value);
```

> **Note**: This extends the existing IGameState interface. The implementation (GameState class) must also be updated.

**GameState Implementation** (`Era.Core/Commands/System/GameState.cs`):
```csharp
public Result<Unit> SetVariable(string name, int index, long value)
    => Result<Unit>.Fail("Not implemented - awaiting Phase 11 variable access integration");
```

> **Stub Rationale**: Full variable system integration requires Phase 11 (Variable Access Layer). Production stub returns Fail (consistent with other GameState methods). Unit tests mock IGameState.SetVariable to return Ok for test scenarios. AC#8-9 tests verify SetVariable was called with correct arguments, not actual COUNT synchronization.

> **Phase 9 Limitation**: REPEAT command returns Fail in production because SetVariable stub returns Fail. This is intentional scope separation - Phase 9 delivers testable infrastructure, Phase 11 provides working variable integration. ERB scripts using REPEAT will fail until Phase 11 completes.

> **Execution Model Note**: Handlers manage stack state only (per F432 ForHandler/NextHandler pattern). Loop continuation (jump back to loop body) is the responsibility of the executor/interpreter layer (Phase 10+). StartLine=0 is placeholder matching F432 precedent.

### REPEAT Handler Implementations

**RepeatHandler** (`Era.Core/Commands/Flow/RepeatHandler.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// REPEAT command handler - pushes frame with countup counter
/// Counter semantics: State stores current iteration (Countup from 0 to N-1)
/// REPEAT N executes body N times: counter starts at 0, increments each REND
/// Integrates with COUNT:0 system variable for legacy compatibility
/// </summary>
public class RepeatHandler : ICommandHandler<RepeatCommand, Unit>
{
    private readonly IExecutionStack _stack;
    private readonly IGameState _gameState;

    public RepeatHandler(IExecutionStack stack, IGameState gameState)
    {
        _stack = stack ?? throw new ArgumentNullException(nameof(stack));
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
    }

    public Task<Result<Unit>> Handle(RepeatCommand command, CancellationToken ct)
    {
        // REPEAT 0 or negative: skip loop (do not push frame)
        if (command.Count <= 0)
            return Task.FromResult(Result<Unit>.Ok(Unit.Value));

        // Push REPEAT frame: State=(Counter: 0, LoopEnd: Count)
        var state = (Counter: 0, LoopEnd: command.Count);
        var frame = new ControlFlowFrame(ControlFlowType.Repeat, 0, state);
        var pushResult = _stack.Push(frame);
        if (pushResult is Result<Unit>.Failure)
            return Task.FromResult(pushResult);

        // Initialize COUNT:0 system variable
        var setResult = _gameState.SetVariable("COUNT", 0, 0);
        if (setResult is Result<Unit>.Failure)
            return Task.FromResult(setResult);

        return Task.FromResult(Result<Unit>.Ok(Unit.Value));
    }
}
```

**RendHandler** (`Era.Core/Commands/Flow/RendHandler.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Commands.Flow;

/// <summary>
/// REND command handler - increments counter and loops back
/// Counter semantics: State stores current iteration (countup)
/// When counter >= LoopEnd, loop is complete
/// Error cases:
///   - Empty stack: Peek() fails, error message propagates from IExecutionStack
///   - Non-REPEAT frame: Returns "REPEATに対応しないRENDです"
/// </summary>
public class RendHandler : ICommandHandler<RendCommand, Unit>
{
    private readonly IExecutionStack _stack;
    private readonly IGameState _gameState;

    public RendHandler(IExecutionStack stack, IGameState gameState)
    {
        _stack = stack ?? throw new ArgumentNullException(nameof(stack));
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
    }

    public Task<Result<Unit>> Handle(RendCommand command, CancellationToken ct)
    {
        // Peek current frame
        var peekResult = _stack.Peek();
        if (peekResult is Result<ControlFlowFrame>.Failure peekFailure)
            return Task.FromResult(Result<Unit>.Fail(peekFailure.Error));

        var frame = ((Result<ControlFlowFrame>.Success)peekResult).Value;
        if (frame.Type != ControlFlowType.Repeat)
            return Task.FromResult(Result<Unit>.Fail("REPEATに対応しないRENDです"));

        // Extract (Counter, LoopEnd) from State tuple
        var (counter, loopEnd) = ((int Counter, int LoopEnd))frame.State!;

        // Increment counter
        var newCounter = counter + 1;

        // Pop current frame
        var popResult = _stack.Pop();
        if (popResult is Result<ControlFlowFrame>.Failure popFailure)
            return Task.FromResult(Result<Unit>.Fail(popFailure.Error));

        // Check loop exit condition (countup: counter >= LoopEnd)
        if (newCounter >= loopEnd)
        {
            // Loop complete, frame already popped
            // Restore outer REPEAT's COUNT if nested (best-effort, failure is acceptable)
            var peekOuter = _stack.Peek();
            if (peekOuter is Result<ControlFlowFrame>.Success outerSuccess &&
                outerSuccess.Value.Type == ControlFlowType.Repeat)
            {
                var (outerCounter, _) = ((int, int))outerSuccess.Value.State!;
                _gameState.SetVariable("COUNT", 0, outerCounter); // Ignore result - loop exit succeeds regardless
            }
            return Task.FromResult(Result<Unit>.Ok(Unit.Value));
        }

        // More iterations remaining, push updated frame with new counter
        var newState = (Counter: newCounter, LoopEnd: loopEnd);
        var newFrame = frame with { State = newState };
        var pushResult = _stack.Push(newFrame);
        if (pushResult is Result<Unit>.Failure pushFailure)
            return Task.FromResult(pushFailure);

        // Update COUNT:0 system variable
        var setResult = _gameState.SetVariable("COUNT", 0, newCounter);
        if (setResult is Result<Unit>.Failure)
            return Task.FromResult(setResult);

        return Task.FromResult(Result<Unit>.Ok(Unit.Value));
    }
}
```

### DI Registration

Add to `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:
```csharp
// REPEAT/REND Command Handlers (Phase 9 - F441)
services.AddSingleton<ICommandHandler<RepeatCommand, Unit>, RepeatHandler>();
services.AddSingleton<ICommandHandler<RendCommand, Unit>, RendHandler>();
```

### Test Naming Convention

**Test File**: `Era.Core.Tests/Commands/Flow/RepeatTests.cs`
**Equivalence File**: `Era.Core.Tests/Commands/Flow/RepeatEquivalenceTests.cs`

Test methods follow `Repeat_{Scenario}` format (e.g., `Repeat_PushesFrameWithCounter`, `Rend_DecrementsCounter`).

### Test Mock Setup Pattern

```csharp
// Mock setup for IGameState.SetVariable
var mockGameState = new Mock<IGameState>();
mockGameState
    .Setup(g => g.SetVariable(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<long>()))
    .Returns(Result<Unit>.Ok(Unit.Value));

// Verify SetVariable called with correct arguments
mockGameState.Verify(g => g.SetVariable("COUNT", 0, expectedCounter), Times.Once());
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F432 | Flow Control Commands (IExecutionStack, ControlFlowType) |
| Predecessor | F443 | COUNT Variable Usage Analysis (design decision) |
| Predecessor | F377 | IGameState interface (being extended) |
| Predecessor | F434 | GameState implementation (being extended) |

---

## Links

- [feature-432.md](feature-432.md) - Flow Control Commands (parent)
- [feature-443.md](feature-443.md) - REPEAT COUNT Variable Usage Analysis
- [feature-377.md](feature-377.md) - IGameState interface (being extended)
- [feature-434.md](feature-434.md) - GameState implementation (being extended)
- [index-features.md](index-features.md)

---

## Review Notes

- **2026-01-10 FL iter7**: [resolved] Counter semantics - Updated to countup (0→N-1) per F443 design decision.
- **2026-01-10 FL iter7**: [resolved] COUNT integration - IGameState.SetVariable added to AC#4 and Dependencies.
- **2026-01-10 FL iter7**: [resolved] Loop continuation logic - Updated to `counter >= LoopEnd` with increment.
- **2026-01-10 FL iter7**: [resolved] Nested REPEAT handling - AC#8 scenario (4) covers nested behavior.
- **2026-01-10 FL iter7**: [resolved] REPEAT 0 skip logic - RepeatHandler checks Count <= 0 before pushing frame.
- **2026-01-10**: [resolved] F443 completed - Design decision: Full legacy compliance (countup semantics, COUNT:0 integration).
- **2026-01-10 FL iter8**: [resolved] ControlFlowFrame.LoopEnd issue - Using State tuple (Counter, LoopEnd) instead.
- **2026-01-10 FL iter8**: [resolved] AC regex pattern - Fixed `TODO|FIXME|HACK` (removed backslash escapes).

**Related Feature**: [feature-443.md](feature-443.md) - REPEAT COUNT Variable Usage Analysis (DONE)

### Design Decision: Full Legacy Compliance Required

**Date**: 2026-01-10 (Post-F443 analysis)

**Finding**: COUNT:0 system variable is actively used by user scripts with both READ and WRITE access patterns across 20+ files.

**Evidence** (from count-usage-analysis.md):
- Scripts READ COUNT for conditionals: `SIF COUNT == 2`, `COUNT == 0`
- Scripts use COUNT as array index: `STAIN:MASTER:COUNT`, `TA:対象者:LOOP_CHR:COUNT`
- Scripts WRITE COUNT: `TFLAG:COUNT = 0`, `LOCAL:COUNT = 0`
- Counter direction is countup (0→N-1), not countdown (N→0)

**Decision**: Option A - Full legacy compliance
- Counter direction: Countup (0→N-1)
- State storage: COUNT:0 system variable integration required
- Loop check: `counter >= LoopEnd` (exit when counter reaches loop end)
- Increment: counter++ at end of REPEAT block (before exit check)

**Impact**:
- Current Implementation Contract (countdown semantics) is REJECTED
- Implementation Contract updated to specify countup semantics with COUNT integration
- RepeatHandler must integrate with IGameState to update COUNT:0
- RendHandler must increment counter (not decrement) and check `counter >= LoopEnd`

**Rationale**: Breaking compatibility would break 20+ files. Technical debt of COUNT:0 global state is acceptable vs. breaking existing scripts.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-10 | create | opus | Created as F432 follow-up | PROPOSED |
| 2026-01-10 21:53 | START | implementer | Task 1 | - |
| 2026-01-10 21:53 | END | implementer | Task 1 | SUCCESS |
| 2026-01-10 21:54 | START | implementer | Task 2 | - |
| 2026-01-10 21:54 | END | implementer | Task 2 | SUCCESS |
| 2026-01-10 21:56 | START | implementer | Task 3 | - |
| 2026-01-10 21:56 | END | implementer | Task 3 | SUCCESS |
