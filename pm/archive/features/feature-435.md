# Feature 435: SCOMF Special Commands (16 Files Migration)

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

Migrate 16 SCOMF sexual act command files (SCOMF1.ERB - SCOMF16.ERB) from ERB to C# ICommand/ICommandHandler pattern. These are game-specific advanced sexual act commands derived from base training commands.

**SCOMF Commands**: Derived sexual act commands with complex SOURCE/STAIN/EXP manipulation:
- SCOMF1: シックスナイン (69 position - oral mutual)
- SCOMF2: 岩清水 (caress variant)
- SCOMF3: Gスポット刺激 (caress variant)
- SCOMF4: 乱れ牡丹 (intercourse variant)
- SCOMF5: 手淫フェラ (service variant)
- SCOMF6: 挿入Ｇスポ責め (intercourse variant)
- SCOMF7: 挿入子宮口責め (intercourse variant)
- SCOMF8: ６９パイズリ (service variant)
- SCOMF9: ダブルフェラ (assistant/lesbian variant)
- SCOMF10: ダブル素股 (service/lesbian variant)
- SCOMF11: ダブルパイズリ (assistant/lesbian variant)
- SCOMF12: パイズリフェラ (service variant)
- SCOMF13: 交互挿入 (intercourse variant)
- SCOMF14: 母乳飲み (caress variant)
- SCOMF15: 授乳する (assistant/lesbian variant)
- SCOMF16: 授乳手コキ (caress variant)

**Output**: Command handler stubs in `Era.Core/Commands/Special/` (Phase 9 infrastructure only - full logic migration deferred to Phase 11 State Management)

---

## Background

### Philosophy (Mid-term Vision)

**Phase 9: Command Infrastructure** - Migrate ERB command system to C# with Mediator Pattern for unified command execution pipeline with cross-cutting concerns (logging, validation, transactions).

### Problem (Current Issue)

SCOMF special commands implemented as ERB files without type safety:
- No compile-time error checking
- Difficult to refactor
- No shared infrastructure for logging/validation

### Goal (What to Achieve)

1. **Migrate 16 SCOMF files** to ICommand/ICommandHandler pattern (stub implementations)
2. **Game-specific interfaces** - ISpecialTraining for shared scenario execution logic
3. **Type-safe command definitions** - SCOMF command classes with strongly-typed parameters
4. **Handler stubs** - Command handlers returning `Result.Fail("NotImplemented: SCOMF{N}")` until Phase 11 State Management integration
5. **Infrastructure verification** - Handlers registered and dispatcher routes correctly (full equivalence deferred to Phase 11)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Special command directory exists | file | Glob | exists | Era.Core/Commands/Special/ | [x] |
| 2 | ISpecialTraining interface exists | file | Glob | exists | Era.Core/Training/ISpecialTraining.cs | [x] |
| 2a | SpecialTraining stub exists | file | Glob | exists | Era.Core/Training/SpecialTraining.cs | [x] |
| 2b | ISpecialTraining DI registration | file | Grep | contains | "AddSingleton.*ISpecialTraining.*SpecialTraining" | [x] |
| 3 | SCOMF1 command handler exists | file | Grep | contains | "class Scomf1Handler" | [x] |
| 4 | SCOMF2 command handler exists | file | Grep | contains | "class Scomf2Handler" | [x] |
| 5 | SCOMF3 command handler exists | file | Grep | contains | "class Scomf3Handler" | [x] |
| 6 | SCOMF4-16 command handlers exist | count | Grep | count_equals | 13 | [x] |
| 7 | DI registration (16 handlers) | count | Grep | count_equals | 16 | [x] |
| 8 | SCOMF command unit test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~ScomfCommandTests" | [x] |
| 9 | SCOMF stub returns NotImplemented | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~ScomfStubTests" | [x] |
| 10 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [x] |

### AC Details

**AC#1**: Special command directory structure
- `Era.Core/Commands/Special/` contains all SCOMF command implementations

**AC#2**: ISpecialTraining interface
- Abstraction for special training logic
- Injected into SCOMF command handlers

**AC#2a**: SpecialTraining stub implementation
- `Era.Core/Training/SpecialTraining.cs`
- Stub implementation that returns Result.Fail for consistency with handler pattern
- Required for DI registration to work
- Note: Interface implementation verified implicitly by build success (AC#8 dotnet test requires build)

**AC#2b**: ISpecialTraining DI registration
- Test: Grep pattern="AddSingleton.*ISpecialTraining.*SpecialTraining" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Verifies ISpecialTraining service is registered for handler injection

**AC#3-5**: SCOMF command implementations (explicit verification for first 3)
- Scomf1Command/Scomf1Handler - SCOMF1 special training
- Scomf2Command/Scomf2Handler - SCOMF2 special training
- Scomf3Command/Scomf3Handler - SCOMF3 special training

**AC#6**: SCOMF4-16 command handlers exist (batch verification)
- Test: Grep pattern="class Scomf(4|5|6|7|8|9|1[0-6])Handler" path="Era.Core/Commands/Special/"
- Expected: 13 matches (Scomf4Handler, Scomf5Handler, ..., Scomf16Handler)
- Note: Pattern explicitly lists 4-9 and 10-16 for clarity

**AC#7**: DI registration verification (16 handlers)
- Test: Grep pattern="Scomf[0-9]+Handler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs" count
- Expected: exactly 16 matches (Scomf1Handler through Scomf16Handler)
- Note: Any deviation from 16 indicates missing or extra handler registration

**AC#8**: SCOMF command unit tests
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~ScomfCommandTests"`
- Verifies SCOMF command execution for each of 16 commands
- Note: Test file location: `Era.Core.Tests/Commands/Special/ScomfCommandTests.cs`
- Note: Test must contain 16+ assertions (one per handler) - 0 tests run indicates missing test file

**AC#9**: SCOMF stub returns NotImplemented
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~ScomfStubTests"`
- Verifies all SCOMF handlers return `Result.Fail("NotImplemented: SCOMF{N}")` (Phase 9 stub behavior)
- Full equivalence testing deferred to Phase 11 State Management
- Note: Test file location: `Era.Core.Tests/Commands/Special/ScomfStubTests.cs`

**AC#10**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" paths="Era.Core/Commands/Special/, Era.Core/Training/ISpecialTraining.cs, Era.Core/Training/SpecialTraining.cs"
- Expected: 0 matches across all feature files

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,2a,2b | Create Special command directory, ISpecialTraining interface, SpecialTraining stub, and DI registration | [x] |
| 2 | 3 | Implement SCOMF1 command and handler | [x] |
| 3 | 4 | Implement SCOMF2 command and handler | [x] |
| 4 | 5 | Implement SCOMF3 command and handler | [x] |
| 5 | 6 | Implement SCOMF4-16 commands and handlers | [x] |
| 6 | 7 | Register all SCOMF handlers in DI | [x] |
| 7 | 8 | Write SCOMF command unit tests | [x] |
| 8 | 9,10 | Verify SCOMF stub behavior and remove technical debt | [x] |
| 9 | All | AC Verification and Status Update | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->
<!-- Batch waiver (Task 1): Directory creation, interface definition, stub implementation, and DI registration are atomic operations for ISpecialTraining component -->
<!-- Batch waiver (Task 5): SCOMF4-16 follow identical pattern to SCOMF1-3, batch implementation appropriate -->
<!-- Batch waiver (Task 8): AC#9 (stub behavior) and AC#10 (tech debt grep) are both final verification steps -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Location**: `Game/ERB/SCOMF*.ERB`

| File | Command Name | Category |
|------|--------------|----------|
| SCOMF1.ERB | シックスナイン | 愛撫・奉仕系派生 |
| SCOMF2.ERB | 岩清水 | 愛撫系派生 |
| SCOMF3.ERB | Gスポット刺激 | 愛撫系派生 |
| SCOMF4.ERB | 乱れ牡丹 | セックス系派生 |
| SCOMF5.ERB | 手淫フェラ | 奉仕系派生 |
| SCOMF6.ERB | 挿入Ｇスポ責め | セックス系派生 |
| SCOMF7.ERB | 挿入子宮口責め | セックス系派生 |
| SCOMF8.ERB | ６９パイズリ | 奉仕系派生 |
| SCOMF9.ERB | ダブルフェラ | 助手/レズ系派生 |
| SCOMF10.ERB | ダブル素股 | 奉仕/レズ系派生 |
| SCOMF11.ERB | ダブルパイズリ | 助手/レズ系派生 |
| SCOMF12.ERB | パイズリフェラ | 奉仕系派生 |
| SCOMF13.ERB | 交互挿入 | セックス系派生 |
| SCOMF14.ERB | 母乳飲み | 愛撫系派生 |
| SCOMF15.ERB | 授乳する | 助手/レズ系派生 |
| SCOMF16.ERB | 授乳手コキ | 愛撫系派生 |

### ISpecialTraining Interface

**ISpecialTraining** (`Era.Core/Training/ISpecialTraining.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Training;

/// <summary>
/// Special training service for SCOMF commands
/// </summary>
public interface ISpecialTraining
{
    /// <summary>Execute special training scenario</summary>
    /// <param name="scenarioId">Scenario identifier (1-16)</param>
    /// <param name="target">Target character</param>
    /// <returns>Training result</returns>
    Result<TrainingResult> ExecuteScenario(int scenarioId, CharacterId target);

    /// <summary>Check if scenario is available</summary>
    bool IsScenarioAvailable(int scenarioId, CharacterId target);
}
```

### SpecialTraining Stub Implementation

**SpecialTraining** (`Era.Core/Training/SpecialTraining.cs`):
```csharp
using System;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Training;

/// <summary>
/// Stub implementation for Phase 9 - full logic deferred to Phase 11
/// </summary>
public class SpecialTraining : ISpecialTraining
{
    public Result<TrainingResult> ExecuteScenario(int scenarioId, CharacterId target)
    {
        // Phase 9: Stub - returns Result.Fail for consistency with handler pattern
        return Result<TrainingResult>.Fail($"NotImplemented: SCOMF{scenarioId}");
    }

    public bool IsScenarioAvailable(int scenarioId, CharacterId target)
    {
        // Phase 9: Stub - always returns false
        return false;
    }
}
```

### SCOMF Command Definitions

**ScomfCommand** (`Era.Core/Commands/Special/ScomfCommand.cs`):
```csharp
using Era.Core.Commands;
using Era.Core.Types;
using Era.Core.Training;

namespace Era.Core.Commands.Special;

/// <summary>
/// SCOMF1 command - special training scenario 1
/// </summary>
public record Scomf1Command(CommandId Id, CharacterId Target) : ICommand<TrainingResult>;

/// <summary>
/// SCOMF2 command - special training scenario 2
/// </summary>
public record Scomf2Command(CommandId Id, CharacterId Target) : ICommand<TrainingResult>;

/// <summary>
/// SCOMF3 command - special training scenario 3
/// </summary>
public record Scomf3Command(CommandId Id, CharacterId Target) : ICommand<TrainingResult>;

// ... SCOMF4-16 similar pattern
```

### CommandId Assignment

CommandId values are provided by callers (command dispatchers/UI). SCOMF commands do not have predefined IDs. The CommandId parameter allows:
- Tracking command execution through pipeline behaviors (logging)
- Correlating commands with UI elements
- Supporting undo/redo operations in future

### SCOMF Handler Implementations

**Scomf1Handler** (`Era.Core/Commands/Special/Scomf1Handler.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Commands;
using Era.Core.Interfaces;
using Era.Core.Training;
using Era.Core.Types;

namespace Era.Core.Commands.Special;

/// <summary>
/// SCOMF1 command handler
/// </summary>
public class Scomf1Handler : ICommandHandler<Scomf1Command, TrainingResult>
{
    // _specialTraining injected for Phase 11 usage - not called in Phase 9 stub
    private readonly ISpecialTraining _specialTraining;

    public Scomf1Handler(ISpecialTraining specialTraining)
    {
        _specialTraining = specialTraining ?? throw new ArgumentNullException(nameof(specialTraining));
    }

    public Task<Result<TrainingResult>> Handle(Scomf1Command command, CancellationToken ct)
    {
        // Phase 9: Stub implementation - full logic deferred to Phase 11
        // In Phase 11, this will call: _specialTraining.ExecuteScenario(1, command.Target)
        return Task.FromResult(Result<TrainingResult>.Fail("NotImplemented: SCOMF1"));
    }
}
```

**Pattern for SCOMF2-16**: Similar structure returning `Result.Fail("NotImplemented: SCOMF{N}")`.

### File Structure

| File | Contents |
|------|----------|
| `Era.Core/Training/ISpecialTraining.cs` | ISpecialTraining interface |
| `Era.Core/Training/SpecialTraining.cs` | SpecialTraining stub implementation |
| `Era.Core/Commands/Special/ScomfCommands.cs` | All 16 SCOMF command records (Scomf1Command...Scomf16Command) |
| `Era.Core/Commands/Special/Scomf{N}Handler.cs` | Individual handler files (16 files: Scomf1Handler.cs...Scomf16Handler.cs) |

### Test Naming Convention

| Test Class | Purpose |
|------------|---------|
| `ScomfCommandTests` | Handler execution tests (AC#8) - verifies all 16 handlers can be invoked via CommandDispatcher |
| `ScomfStubTests` | Stub behavior tests (AC#9) - verifies all 16 handlers return Result.Fail("NotImplemented: SCOMF{N}") |

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// Special Training Service (Phase 9)
services.AddSingleton<ISpecialTraining, SpecialTraining>();

// SCOMF Command Handlers (Phase 9)
services.AddSingleton<ICommandHandler<Scomf1Command, TrainingResult>, Scomf1Handler>();
services.AddSingleton<ICommandHandler<Scomf2Command, TrainingResult>, Scomf2Handler>();
services.AddSingleton<ICommandHandler<Scomf3Command, TrainingResult>, Scomf3Handler>();
services.AddSingleton<ICommandHandler<Scomf4Command, TrainingResult>, Scomf4Handler>();
services.AddSingleton<ICommandHandler<Scomf5Command, TrainingResult>, Scomf5Handler>();
services.AddSingleton<ICommandHandler<Scomf6Command, TrainingResult>, Scomf6Handler>();
services.AddSingleton<ICommandHandler<Scomf7Command, TrainingResult>, Scomf7Handler>();
services.AddSingleton<ICommandHandler<Scomf8Command, TrainingResult>, Scomf8Handler>();
services.AddSingleton<ICommandHandler<Scomf9Command, TrainingResult>, Scomf9Handler>();
services.AddSingleton<ICommandHandler<Scomf10Command, TrainingResult>, Scomf10Handler>();
services.AddSingleton<ICommandHandler<Scomf11Command, TrainingResult>, Scomf11Handler>();
services.AddSingleton<ICommandHandler<Scomf12Command, TrainingResult>, Scomf12Handler>();
services.AddSingleton<ICommandHandler<Scomf13Command, TrainingResult>, Scomf13Handler>();
services.AddSingleton<ICommandHandler<Scomf14Command, TrainingResult>, Scomf14Handler>();
services.AddSingleton<ICommandHandler<Scomf15Command, TrainingResult>, Scomf15Handler>();
services.AddSingleton<ICommandHandler<Scomf16Command, TrainingResult>, Scomf16Handler>();
```

### Design Rationale

**Game-Specific Logic**:
- SCOMF commands are game-specific and contain custom training scenarios
- Each scenario has unique requirements and effects
- ISpecialTraining service encapsulates scenario execution logic

**Type Safety Benefits**:
- Strongly typed commands replace ERB function calls
- Compile-time error checking
- IDE support for refactoring

**Error Handling Pattern**:
- ISpecialTraining.ExecuteScenario returns `Result<TrainingResult>` for consistency with ICommandHandler pattern
- TrainingResult contains Success/ErrorMessage properties but these are for domain-level status (e.g., "training was effective")
- Result<T>.Fail is for technical/validation errors (e.g., "scenario not available", "invalid target")
- When Result.IsSuccess, TrainingResult.Success indicates domain outcome

**File Organization Rationale**:
- Commands in single file (ScomfCommands.cs) - simple records, grouped for brevity
- Handlers in separate files (Scomf1Handler.cs...Scomf16Handler.cs) - each will receive distinct business logic in Phase 11
- This separation supports independent modification when full logic is implemented

**Compiler Warning Acceptance**:
- Handlers inject `_specialTraining` field but don't use it in Phase 9 stubs
- CS0169 warning for unused field is expected and acceptable
- Field will be used in Phase 11 when full logic is implemented

### Equivalence Verification (Phase 11 Scope)

**Phase 9 Scope**: Infrastructure only. Handlers return NotImplementedException.

**Phase 11 Scope**: Full logic migration.
- Legacy behavior: SCOMF1-16 ERB files execute sexual act commands with SOURCE/STAIN/EXP manipulation.
- New behavior: Scomf1-16 commands invoke full logic through IVariableStore and character state interfaces.

**Verification Deferred**: Full equivalence testing (SOURCE calculations, STAIN transfers, etc.) requires Phase 11 State Management integration.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F429 | CommandDispatcher + Mediator Pipeline (ICommand/ICommandHandler) |
| Predecessor | F430 | Pipeline Behaviors (logging/validation applied to SCOMF commands) |
| Predecessor | F393 | Training System (TrainingResult type from Phase 6) |
| Successor | Phase 11 | State Management - Full SCOMF logic implementation requires IVariableStore, character state interfaces for TCVAR, STAIN, EXP, SOURCE manipulation |

**Note**: Phase 9 implements handler stubs only. Full game logic (SOURCE calculations, STAIN transfers, EXP updates, execution threshold checks) requires Phase 11 State Management integration.

---

## Links

- [feature-424.md](feature-424.md) - Phase 9 Planning (parent feature)
- [feature-429.md](feature-429.md) - CommandDispatcher + Mediator Pipeline (dependency)
- [feature-430.md](feature-430.md) - Pipeline Behaviors (dependency)
- [feature-393.md](feature-393.md) - Training System (TrainingResult type)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 9 SCOMF Special Commands

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-10 | create | implementer | Created from F424 Phase 9 Planning | PROPOSED |
| 2026-01-10 | START | initializer | Phase 1 Initialize | READY:435:engine |
| 2026-01-10 | START | explorer | Phase 2 Investigation | READY |
| 2026-01-10 | START | implementer | Phase 3 TDD Tests | SUCCESS (RED) |
| 2026-01-10 18:31 | START | implementer | Task 1 | - |
| 2026-01-10 18:31 | END | implementer | Task 1 | SUCCESS |
| 2026-01-10 18:34 | START | implementer | Task 2-5 | - |
| 2026-01-10 18:34 | END | implementer | Task 2-5 | SUCCESS |
| 2026-01-10 18:35 | START | implementer | Task 6 | - |
| 2026-01-10 18:35 | END | implementer | Task 6 | SUCCESS |
| 2026-01-10 18:39 | START | implementer | Task 8 | - |
| 2026-01-10 18:39 | END | implementer | Task 8 | SUCCESS |
| 2026-01-10 18:45 | START | ac-tester | AC Verification | - |
| 2026-01-10 18:45 | END | ac-tester | AC Verification | PASS: 12/12 ACs verified |
| 2026-01-10 18:48 | START | feature-reviewer | Phase 7 post | READY |
| 2026-01-10 18:49 | START | feature-reviewer | Phase 7 doc-check | NEEDS_REVISION |
| 2026-01-10 18:50 | END | opus | SSOT Update | engine-dev SKILL.md updated |
| 2026-01-10 18:55 | END | opus | 残課題検討 | Phase 12 タスク追記 (full-csharp-architecture.md) |
