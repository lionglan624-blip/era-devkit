# Feature 487: GameSaveState Types

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

**GameSaveState type definitions** - Serializable state record types for JSON save/load operations.

Create `GameSaveState`, `CharacterSaveState`, `PlayerSaveState`, and `GameContextSaveState` records in Era.Core/Types/ for use by StateManager (F475). These records are serializable DTOs distinct from runtime state classes.

**Output**: `Era.Core/Types/GameSaveState.cs` containing all save state records.

**Volume**: ~50 lines (within ~300 line limit for engine type).

**Note**: Named `*SaveState` to distinguish from runtime state classes (e.g., existing `GameState` service class).

---

## Background

### Philosophy (Mid-term Vision)

**Phase 14: Era.Core Engine** - Pure C# game engine implementation enabling headless execution for automated testing and CI/CD integration. Serializable state types provide data persistence independent of runtime behavior.

### Problem (Current Issue)

- F475 (StateManager) requires GameSaveState type for JSON serialization
- No serializable state records exist in Era.Core
- Naming collision: `GameState` class already exists as IGameState implementation
- Dependency types (CharacterState, PlayerState, GameContext as serializable records) undefined

### Goal (What to Achieve)

1. Create GameSaveState record with Characters, Player, Context, GameOver properties
2. Create CharacterSaveState record for character persistence
3. Create PlayerSaveState record for player persistence
4. Create GameContextSaveState record for game context persistence
5. Ensure all records are JSON-serializable with System.Text.Json
6. Delete all tech debt (TODO/FIXME/HACK)
7. Verify existing tests still pass (regression guard)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | GameSaveState.cs exists | file | Glob | exists | Era.Core/Types/GameSaveState.cs | [x] |
| 2 | GameSaveState record | code | Grep | contains | public record GameSaveState | [x] |
| 3 | CharacterSaveState record | code | Grep | contains | public record CharacterSaveState | [x] |
| 4 | PlayerSaveState record | code | Grep | contains | public record PlayerSaveState | [x] |
| 5 | GameContextSaveState record | code | Grep | contains | public record GameContextSaveState | [x] |
| 6 | GameSaveState.Characters property | code | Grep | contains | Dictionary<string, CharacterSaveState> Characters | [x] |
| 7 | GameSaveState.Player property | code | Grep | contains | PlayerSaveState Player | [x] |
| 8 | GameSaveState.Context property | code | Grep | contains | GameContextSaveState Context | [x] |
| 9 | Namespace declaration | code | Grep | contains | namespace Era\\.Core\\.Types | [x] |
| 10 | Zero technical debt | code | Grep | not_contains | TODO|FIXME|HACK | [x] |
| 11 | Build succeeds | build | Bash | succeeds | dotnet build Era.Core | [x] |
| 12 | Unit tests pass | test | Bash | succeeds | dotnet test Era.Core.Tests | [x] |
| 13 | No Unity dependencies | code | Grep | not_contains | using UnityEngine | [x] |

### AC Details

**AC#1**: GameSaveState.cs file existence
- Test: Glob pattern="Era.Core/Types/GameSaveState.cs"
- Expected: File exists

**AC#2**: GameSaveState record definition
- Test: Grep pattern="public record GameSaveState" path="Era.Core/Types/GameSaveState.cs"
- Expected: Record definition found

**AC#3**: CharacterSaveState record definition
- Test: Grep pattern="public record CharacterSaveState" path="Era.Core/Types/GameSaveState.cs"
- Expected: Record definition found

**AC#4**: PlayerSaveState record definition
- Test: Grep pattern="public record PlayerSaveState" path="Era.Core/Types/GameSaveState.cs"
- Expected: Record definition found

**AC#5**: GameContextSaveState record definition
- Test: Grep pattern="public record GameContextSaveState" path="Era.Core/Types/GameSaveState.cs"
- Expected: Record definition found

**AC#6**: Characters dictionary property
- Test: Grep pattern="Dictionary<string, CharacterSaveState> Characters" path="Era.Core/Types/GameSaveState.cs"
- Expected: Dictionary property with CharacterSaveState values

**AC#7**: Player property
- Test: Grep pattern="PlayerSaveState Player" path="Era.Core/Types/GameSaveState.cs"
- Expected: PlayerSaveState property

**AC#8**: Context property
- Test: Grep pattern="GameContextSaveState Context" path="Era.Core/Types/GameSaveState.cs"
- Expected: GameContextSaveState property

**AC#9**: Namespace declaration
- Test: Grep pattern="namespace Era\\.Core\\.Types" path="Era.Core/Types/GameSaveState.cs"
- Expected: Proper namespace

**AC#10**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Types/GameSaveState.cs"
- Expected: 0 matches

**AC#11**: Build succeeds
- Test: `dotnet build Era.Core`
- Expected: Build succeeds (exit code 0)

**AC#12**: Unit tests pass
- Test: `dotnet test Era.Core.Tests`
- Expected: All Era.Core tests pass (exit code 0)

**AC#13**: No Unity dependencies
- Test: Grep pattern="using UnityEngine" path="Era.Core/Types/GameSaveState.cs"
- Expected: 0 matches (pure C# for headless)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4,5,6,7,8,9,13 | Create Era.Core/Types/GameSaveState.cs with all record definitions | [x] |
| 2 | 10 | Verify no TODO/FIXME/HACK in GameSaveState.cs | [x] |
| 3 | 11 | Run `dotnet build Era.Core` to verify compilation succeeds | [x] |
| 4 | 12 | Run `dotnet test Era.Core.Tests` to verify no regressions | [x] |

<!-- AC:Task 1:1 Rule: 13 ACs, 4 Tasks. Batch waiver: Task 1 covers AC#1-9,13 (related record definitions in single file) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Record Definitions

Per architecture.md line 1158-1172, adapted for serialization:

```csharp
// Era.Core/Types/GameSaveState.cs
using System.Text.Json;
// Note: ImplicitUsings enabled in project; System.Collections.Generic available
namespace Era.Core.Types;

/// <summary>
/// Serializable game state for JSON save/load operations.
/// Named *SaveState to distinguish from runtime state classes.
/// </summary>
public record GameSaveState
{
    public Dictionary<string, CharacterSaveState> Characters { get; init; } = new();
    public PlayerSaveState Player { get; init; } = new();
    public GameContextSaveState Context { get; init; } = new();
    public bool GameOver { get; init; }

    public static GameSaveState CreateNew() => new();
}

/// <summary>
/// Serializable character state for save persistence.
/// </summary>
public record CharacterSaveState
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Dictionary<string, int> Flags { get; init; } = new();
    public Dictionary<string, string> Attributes { get; init; } = new();
}

/// <summary>
/// Serializable player state for save persistence.
/// </summary>
public record PlayerSaveState
{
    public string Name { get; init; } = string.Empty;
    public Dictionary<string, int> Flags { get; init; } = new();
    public int Money { get; init; }
}

/// <summary>
/// Serializable game context for save persistence.
/// </summary>
public record GameContextSaveState
{
    public int Day { get; init; }
    public int Time { get; init; }
    public string Location { get; init; } = string.Empty;
    public Dictionary<string, JsonElement> Variables { get; init; } = new();
}
```

### Design Rationale

| Decision | Rationale |
|----------|-----------|
| **Record types** | Immutable, value-based equality, built-in `with` expressions for modifications |
| **`*SaveState` naming** | Avoids collision with runtime `GameState` class (IGameState impl) and `CharacterState` (Era.Core.Common.CharacterSetup.cs runtime class). SaveState = serializable DTO, State = runtime behavior |
| **Dictionary for Flags** | ERA games use FLAG arrays; Dictionary provides name-based access |
| **Single file** | All related records together for cohesion; ~50 lines total |
| **Default initializers** | Prevents null reference issues during deserialization |
| **JsonElement for Variables** | Preserves arbitrary JSON structure during save/load; enables extensibility without schema changes. F475 StateManager handles serialization |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F471 | Phase 14 Planning defines this feature |
| Successor | F475 | StateManager uses GameSaveState for persistence |

---

## Links

- [feature-471.md](feature-471.md) - Phase 14 Planning (parent feature)
- [feature-475.md](feature-475.md) - StateManager Implementation (uses these types)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Architecture reference (lines 1158-1172)
- [feature-template.md](reference/feature-template.md) - Feature structure guidelines

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-13 | create | FL | Created as predecessor for F475 StateManager | PROPOSED |
| 2026-01-13 22:05 | START | implementer | Task 1 | - |
| 2026-01-13 22:05 | END | implementer | Task 1 | SUCCESS |
| 2026-01-13 22:05 | START | implementer | Task 2 | - |
| 2026-01-13 22:05 | END | implementer | Task 2 | SUCCESS |
| 2026-01-13 22:05 | START | implementer | Task 3 | - |
| 2026-01-13 22:05 | END | implementer | Task 3 | SUCCESS |
| 2026-01-13 22:05 | START | implementer | Task 4 | - |
| 2026-01-13 22:05 | END | implementer | Task 4 | SUCCESS |
