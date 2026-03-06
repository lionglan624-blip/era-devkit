# Feature 474: GameEngine Implementation

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

**GameEngine main loop implementation** - Core orchestration component for turn-based game execution.

Implement `GameEngine` with `IGameEngine` interface (Phase 4 design) providing main game loop, turn processing, and initialization. This component orchestrates all game systems (KojoEngine, NtrEngine, CommandProcessor, StateManager) to execute game turns.

**Output**: `Era.Core/GameEngine.cs` and DI registration in `ServiceCollectionExtensions.cs`.

**Volume**: ~250 lines (within ~300 line limit for engine type).

---

## Background

### Philosophy (Mid-term Vision)

**Phase 14: Era.Core Engine** - Pure C# game engine implementation enabling headless execution for automated testing and CI/CD integration. GameEngine is the central orchestrator coordinating all subsystems.

### Problem (Current Issue)

Phase 14 requires core game engine implementation:
- IGameEngine interface defined in Phase 4 but no implementation exists
- Main game loop pattern needs C# implementation
- Turn-based execution requires coordination between subsystems
- Headless execution requires proper initialization and state management

### Goal (What to Achieve)

1. Implement `GameEngine` class with `IGameEngine` interface
2. Provide turn-based game loop with `ProcessTurn()` method
3. Support initialization with `GameConfig`
4. Expose `IGameState` for external state access
5. Register in DI container
6. Delete all tech debt (TODO/FIXME/HACK)
7. Verify tests pass after implementation

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | GameEngine.cs exists | file | Glob | exists | Era.Core/GameEngine.cs | [x] |
| 2 | IGameEngine interface | code | Grep | contains | public interface IGameEngine | [x] |
| 3 | GameEngine implements IGameEngine | code | Grep | contains | public class GameEngine : IGameEngine | [x] |
| 4 | State property exists | code | Grep | contains | IGameState State | [x] |
| 5 | ProcessTurn method exists | code | Grep | contains | Result<GameTick> ProcessTurn | [x] |
| 6 | Initialize method exists | code | Grep | contains | void Initialize.*GameConfig | [x] |
| 7 | DI registration | file | Grep | contains | AddSingleton.*IGameEngine.*GameEngine | [x] |
| 8 | ProcessTurn positive/negative tests | test | Bash | succeeds | dotnet test --filter GameEngineTests | [x] |
| 9 | Namespace declaration | code | Grep | contains | namespace Era\\.Core | [x] |
| 10 | Zero technical debt | code | Grep | not_contains | TODO\|FIXME\|HACK | [x] |
| 11 | Unit tests pass | test | Bash | succeeds | dotnet test Era.Core.Tests | [x] |
| 12 | No unsafe code | code | Grep | not_contains | unsafe\\s+(class\|struct\|void\|static) | [x] |
| 13 | GameConfig type exists | file | Glob | exists | Era.Core/Types/GameConfig.cs | [x] |
| 14 | GameTick type exists | file | Glob | exists | Era.Core/Types/GameTick.cs | [x] |

### AC Details

**AC#1**: File existence verification
- Test: Glob pattern="Era.Core/GameEngine.cs"
- Expected: File exists

**AC#2**: IGameEngine interface exists
- Test: Grep pattern="public interface IGameEngine" path="Era.Core/"
- Expected: Interface definition found

**AC#3**: GameEngine implements IGameEngine
- Test: Grep pattern="public class GameEngine : IGameEngine" path="Era.Core/GameEngine.cs"
- Expected: Class declaration with interface implementation

**AC#4**: State property exists
- Test: Grep pattern="IGameState State" path="Era.Core/GameEngine.cs"
- Expected: Property returning IGameState

**AC#5**: ProcessTurn method exists
- Test: Grep pattern="Result<GameTick> ProcessTurn" path="Era.Core/GameEngine.cs"
- Expected: Method signature matches Phase 4 design

**AC#6**: Initialize method exists
- Test: Grep pattern="void Initialize.*GameConfig" path="Era.Core/GameEngine.cs"
- Expected: Initialization method accepting GameConfig

**AC#7**: DI registration
- Test: Grep pattern="AddSingleton.*IGameEngine.*GameEngine" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: DI registration present (pattern assumes single-line registration per Era.Core convention)

**AC#8**: ProcessTurn positive/negative tests
- Test: `dotnet test --filter FullyQualifiedName~GameEngineTests`
- Expected: All GameEngineTests pass
- Verifies positive cases (ProcessTurn returns Ok<GameTick> with TurnNumber after initialization)
- Verifies negative cases (ProcessTurn returns Fail when uninitialized)
- Note: HeadlessMode behavior verification deferred to F479 (HeadlessUI Implementation)

**AC#9**: Namespace declaration
- Test: Grep pattern="namespace Era\\.Core" path="Era.Core/GameEngine.cs"
- Expected: Proper namespace declaration

**AC#10**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/GameEngine.cs, Era.Core/IGameEngine.cs, Era.Core/Types/GameConfig.cs, Era.Core/Types/GameTick.cs"
- Expected: 0 matches in feature-created files (excludes shared ServiceCollectionExtensions.cs)

**AC#11**: Unit tests pass
- Test: `dotnet test Era.Core.Tests`
- Expected: All Era.Core tests pass (exit code 0)

**AC#12**: No unsafe code blocks
- Test: Grep pattern="unsafe\s+(class|struct|void|static)" path="Era.Core/GameEngine.cs, Era.Core/IGameEngine.cs"
- Expected: 0 matches (pattern matches C# unsafe blocks, not comments/strings)

**AC#13**: GameConfig type exists
- Test: Glob pattern="Era.Core/Types/GameConfig.cs"
- Expected: File exists with GameConfig record/class definition
- Note: Namespace `Era.Core.Types` implicit per directory location (Implementation Contract specifies exact namespace)

**AC#14**: GameTick type exists
- Test: Glob pattern="Era.Core/Types/GameTick.cs"
- Expected: File exists with GameTick record/class definition
- Note: Namespace `Era.Core.Types` implicit per directory location (Implementation Contract specifies exact namespace)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 13,14 | Create GameConfig and GameTick types in Era.Core/Types/ | [x] |
| 2 | 1,9 | Create Era.Core/GameEngine.cs (includes namespace declaration per Implementation Contract) | [x] |
| 3 | 2 | Create IGameEngine interface in Era.Core/IGameEngine.cs | [x] |
| 4 | 3,4,5,6 | Implement IGameEngine members (State, ProcessTurn, Initialize) | [x] |
| 5 | 7 | Register GameEngine in ServiceCollectionExtensions.cs | [x] |
| 6 | 8 | Write GameEngineTests with positive and negative test cases | [x] |
| 7 | 10,12 | Verify code quality (no TODO/FIXME/HACK, no unsafe blocks) | [x] |
| 8 | 11 | Run dotnet test and fix any failures | [x] |

<!-- AC:Task 1:1 Rule: 14 ACs = 8 Tasks (batch waiver for Task 1, Task 2, Task 4, Task 7 per F384 precedent for related verification checks) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Type Definitions

GameConfig and GameTick types required by IGameEngine interface:

```csharp
// Era.Core/Types/GameConfig.cs
namespace Era.Core.Types;

/// <summary>
/// Game configuration for engine initialization.
/// </summary>
public sealed record GameConfig(
    string GamePath,
    bool HeadlessMode = false
);

// Era.Core/Types/GameTick.cs
namespace Era.Core.Types;

/// <summary>
/// Represents a single game turn tick result.
/// </summary>
public readonly record struct GameTick(
    int TurnNumber,
    bool IsComplete
);
```

### Interface Definition

Per Phase 4 Design Requirements (designs/full-csharp-architecture.md):

**Note on IGameState**: The existing `IGameState` interface (`Era.Core/Interfaces/IGameState.cs`) is a **game control interface** providing SaveGame, LoadGame, Quit, Restart, ResetData, SetVariable methods. The `State` property in `IGameEngine` exposes this control interface for external system commands (e.g., headless mode quit handling, save/load triggers). This is NOT observable game state data - that responsibility belongs to individual domain aggregates (Character, etc.).

```csharp
// Era.Core/IGameEngine.cs
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core;

/// <summary>
/// Core game engine providing main game loop and turn processing.
/// </summary>
public interface IGameEngine
{
    /// <summary>Get game state control interface for system commands</summary>
    IGameState State { get; }

    /// <summary>Process one game turn</summary>
    /// <returns>Success with GameTick if turn completed, Fail if error occurred</returns>
    Result<GameTick> ProcessTurn();

    /// <summary>Initialize game engine with configuration</summary>
    /// <param name="config">Game configuration</param>
    void Initialize(GameConfig config);
}
```

### Implementation Class

```csharp
// Era.Core/GameEngine.cs
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core;

/// <summary>
/// Core game engine implementation providing main game loop.
/// </summary>
public class GameEngine : IGameEngine
{
    private readonly IGameState _gameState;
    private GameConfig? _config;
    private bool _isInitialized;
    private int _turnNumber;

    public GameEngine(IGameState gameState)
    {
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
    }

    public IGameState State => _gameState;

    public void Initialize(GameConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;
        _isInitialized = true;
        _turnNumber = 0;
    }

    public Result<GameTick> ProcessTurn()
    {
        if (!_isInitialized)
            return Result<GameTick>.Fail("ゲームエンジンが初期化されていません");

        _turnNumber++;
        // Orchestration logic for subsystems (F475-F478) added in later phases
        return Result<GameTick>.Ok(new GameTick(_turnNumber, IsComplete: false));
    }
}
```

### Implementation Requirements

| Requirement | Verification |
|-------------|--------------|
| **Result<T> usage** | ProcessTurn returns Result<GameTick> for recoverable errors |
| **Error messages** | Japanese format: "{Operation}に失敗しました: {reason}" |
| **State initialization** | Initialize() must be called before ProcessTurn() |
| **Thread safety** | Not required (single-threaded game loop) |

### Error Message Format

- Uninitialized state: `"ゲームエンジンが初期化されていません"`
- Turn processing error: `"ターン処理に失敗しました: {details}"`

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IGameEngine, GameEngine>();
```

### Test Requirements

**Dependency Handling**: GameEngine constructor requires IGameState (existing interface). Future orchestration dependencies (IKojoEngine, INtrEngine, ICommandProcessor from F475-F478) will be added as **optional** constructor parameters in later phases. Current tests verify GameEngine wiring with IGameState only.

**Positive Tests**:
- Initialize with valid GameConfig succeeds
- ProcessTurn after initialization returns Ok<GameTick>
- State property returns non-null IGameState after initialization

**Negative Tests**:
- ProcessTurn before Initialize returns Fail with error message
- Initialize with null GameConfig throws ArgumentNullException (programmer error)

**Test Naming Convention**: Test methods follow `Test{MethodName}{Scenario}` format (e.g., `TestProcessTurnUninitialized`, `TestInitializeValid`).

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F471 | Phase 14 Planning defines this feature |
| Uses | IGameState | Era.Core/Interfaces/IGameState.cs - Constructor dependency for game state control |
| Successor | F475 | StateManager orchestrated by IGameEngine for state management |
| Successor | F476 | KojoEngine called by GameEngine during turn processing |
| Successor | F477 | CommandProcessor called by GameEngine during turn processing |
| Successor | F478 | NtrEngine called by GameEngine during turn processing |

---

## Links

- [feature-471.md](feature-471.md) - Phase 14 Planning (parent feature)
- [feature-475.md](feature-475.md) - StateManager Implementation (successor)
- [feature-476.md](feature-476.md) - KojoEngine Implementation (successor)
- [feature-477.md](feature-477.md) - CommandProcessor Implementation (successor)
- [feature-478.md](feature-478.md) - NtrEngine Implementation (successor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 14 Task 1 definition
- [feature-template.md](reference/feature-template.md) - Feature structure guidelines

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-13 FL iter1**: [resolved] Phase2-Validate - AC#2 path: The broader path 'Era.Core/' is intentional per Implementation Contract but could be more specific after implementation decision. Enhancement suggestion.
- **2026-01-13 FL iter1**: [resolved] Phase2-Validate - Dependencies section: Stub handling clarified in Implementation Contract Test Requirements section.
- **2026-01-13 FL iter3**: [skipped] Phase2-Validate - Philosophy section: Enhancement suggestion to add note about Phase 4 design superseding prior patterns. Minor, subjective enhancement.
- **2026-01-13 FL iter4**: [applied] Phase2-Validate - Dependencies section: Added IGameState as 'Uses' dependency for traceability.
- **2026-01-13 FL iter7**: [skipped] Phase2-Validate - Test file location: Enhancement suggestion to add test file path to Test Requirements. Not explicitly required by ENGINE.md.
- **2026-01-13 FL iter8**: [skipped] Phase2-Validate - using System: Enhancement suggestion to add explicit 'using System;' to Implementation Class snippet. Depends on Era.Core.csproj ImplicitUsings setting.
- **2026-01-13 FL iter9**: [applied] Phase2-Validate - AC#10 scope: Removed ServiceCollectionExtensions.cs from tech debt check to prevent retroactive failures.
- **2026-01-13 FL iter9**: [skipped] Phase2-Validate - Type origin notes: Enhancement to add comments in Interface Definition noting where types come from (IGameState from Era.Core.Interfaces, etc.). Subjective.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-13 | create | spec-writer | Created from F471 Phase 14 Planning Task 1 | PROPOSED |
| 2026-01-13 20:16 | START | implementer | Task 1-8 | - |
| 2026-01-13 20:16 | END | implementer | Task 1-8 | SUCCESS |
| 2026-01-13 20:22 | START | ac-tester | AC#1-14 | - |
| 2026-01-13 20:22 | END | ac-tester | AC#1-14 | PASS:14/14 |
| 2026-01-13 20:22 | START | feature-reviewer | mode: post | - |
| 2026-01-13 20:22 | END | feature-reviewer | mode: post | NEEDS_REVISION (SSOT) |
| 2026-01-13 20:22 | - | opus | SSOT update | engine-dev SKILL.md updated |
| 2026-01-13 20:22 | START | feature-reviewer | mode: doc-check | - |
| 2026-01-13 20:22 | END | feature-reviewer | mode: doc-check | READY |
