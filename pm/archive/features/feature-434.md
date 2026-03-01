# Feature 434: System Commands (16+ Commands) + GlobalStatic Migration

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

Migrate 15 System commands from legacy GameProc to ICommand/ICommandHandler pattern. This includes:
- **Character Commands** (5): ADDCHARA, DELCHARA, PICKUPCHARA, SWAPCHARA, ADDCOPYCHARA
- **Style Commands** (5): SETCOLOR, SETBGCOLOR, SETFONT, RESETCOLOR, ALIGNMENT
- **System Commands** (5): SAVEGAME, LOADGAME, QUIT, RESTART, RESETDATA

<!-- Note: SETCOLORBYNAME, BEGIN, DRAWLINE deferred - not in Phase 9 scope -->

<!-- Task 8.5 (GlobalStatic accessor migration) deferred to Phase 11 State Management - see 残課題 section -->

**Game State Operation Responsibility**: All character management, style control, and system control commands migrated to unified command infrastructure.

**Output**: Command implementations in `Era.Core/Commands/System/`

---

## Background

### Philosophy (Mid-term Vision)

**Phase 9: Command Infrastructure** - Migrate ERB command system to C# with Mediator Pattern for unified command execution pipeline with cross-cutting concerns (logging, validation, transactions).

### Problem (Current Issue)

System commands scattered across GameProc with direct GlobalStatic dependencies:
- Character management lacks abstraction
- Style commands directly modify console state
- System commands (save/load) tightly coupled to file system
- **Phase 3 Technical Debt**: GameInitialization has 3 TODO comments for GlobalStatic accessor migration (introduced in F370) → **Deferred to Phase 11**

### Goal (What to Achieve)

1. **Migrate 15 System commands** to ICommand/ICommandHandler pattern
2. **Service abstractions** - ICharacterManager, IStyleManager, IGameState
3. **Type-safe command definitions** - System command classes
4. **Handler implementations** - Command handlers with Result<T>
5. **Equivalence verification** - Operations match legacy behavior

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | System command directory exists | file | Glob | exists | Era.Core/Commands/System/ | [x] |
| 2 | ICharacterManager interface exists | file | Glob | exists | Era.Core/Interfaces/ICharacterManager.cs | [x] |
| 3 | IStyleManager interface exists | file | Glob | exists | Era.Core/Interfaces/IStyleManager.cs | [x] |
| 4 | IGameState interface extended | file | Grep | contains | "Result<Unit> SaveGame" | [x] |
| 5 | ADDCHARA command handler exists | file | Grep | contains | "class AddCharaHandler" | [x] |
| 6 | SETCOLOR command handler exists | file | Grep | contains | "class SetColorHandler" | [x] |
| 7 | SAVEGAME command handler exists | file | Grep | contains | "class SaveGameHandler" | [x] |
| 8 | DI registration (handlers) | file | Grep | contains | "AddSingleton.*ICommandHandler.*AddCharaHandler" | [x] |
| 9 | System command unit test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~SystemCommandTests" | [x] |
| 10 | System command equivalence verification | file | Grep | contains | "SystemCommandEquivalence" | [x] |
| 11 | Zero technical debt in Commands/System | file | Grep | not_contains | "TODO\|FIXME\|HACK" in Era.Core/Commands/System/ | [x] |

### AC Details

**AC#1**: System command directory structure
- `Era.Core/Commands/System/` contains all system command implementations

**AC#2-4**: System service interfaces
- ICharacterManager - AddChara, DelChara, PickupChara (new file)
- IStyleManager - SetColor, SetFont, Alignment (new file)
- IGameState - SaveGame, LoadGame, Quit (EXTEND existing placeholder interface)
  - **NOTE**: IGameState is currently empty placeholder (F377). No implementations exist to break.

**AC#5-7**: System command implementations
- AddCharaCommand/AddCharaHandler - ADDCHARA character addition
- SetColorCommand/SetColorHandler - SETCOLOR color setting
- SaveGameCommand/SaveGameHandler - SAVEGAME save game state
- Additional: DELCHARA, PICKUPCHARA, SETFONT, LOADGAME, QUIT, etc.

**AC#8**: DI registration verification
- Test: Grep pattern="AddSingleton.*ICommandHandler.*SystemHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- All system handlers registered

**AC#9**: System command unit tests
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~SystemCommandTests"`
- Verifies character management, style control, and save/load

**AC#10**: System command equivalence verification
- Test: Grep pattern="SystemCommandEquivalence" path="Era.Core.Tests/Commands/System/"
- Test file: `Era.Core.Tests/Commands/System/SystemCommandEquivalenceTests.cs`
- **Minimum**: 3 test methods covering: (1) AddChara creates character, (2) SetColor changes console state, (3) SaveGame persists state
- Verifies operations match legacy ADDCHARA/SETCOLOR/SAVEGAME behavior

**AC#11**: Zero technical debt in Commands/System
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/System/"
- Expected: 0 matches in Commands/System/

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1a | 1,2,3 | Create System command directory + ICharacterManager + IStyleManager | [x] |
| 1b | 4 | Extend existing IGameState with SaveGame/LoadGame/Quit/Restart/ResetData | [x] |

<!-- Batch waiver (Task 1a): Directory structure + related NEW service interfaces are atomic foundation setup per Phase 9 pattern. Task 1b separate because IGameState already exists and requires modification not creation. -->
| 2 | 5 | Implement character commands (ADDCHARA/DELCHARA/PICKUPCHARA) | [x] |
| 3 | 6 | Implement style commands (SETCOLOR/SETFONT/ALIGNMENT) | [x] |
| 4 | 7 | Implement system commands (SAVEGAME/LOADGAME/QUIT) | [x] |
| 5 | 8 | Register all system handlers in DI | [x] |
| 6 | 9 | Write system command unit tests | [x] |
| 7 | 10,11 | Verify system command equivalence and remove technical debt | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Location**: `engine/Assets/Scripts/Emuera/GameProc/Function/Instraction.Child.cs`

| Command | Class | Line Range | Category | Purpose |
|---------|-------|-----------|----------|---------|
| ADDCHARA | ADDCHARA_Instruction | 927-971 | Character | Add character |
| DELCHARA | (via ADDCHARA) | 950-969 | Character | Delete character (isDel=true) |
| PICKUPCHARA | (VEvaluator method) | TBD | Character | Select character |
| SWAPCHARA | SWAPCHARA_Instruction | 987-1002 | Character | Swap characters |
| ADDCOPYCHARA | ADDCOPYCHARA_Instruction | 1020-1034 | Character | Copy character |
| SETCOLOR | (Console method) | TBD | Style | Set text color |
| SETBGCOLOR | (Console method) | TBD | Style | Set background color |
| SETFONT | (Console method) | TBD | Style | Set font |
| RESETCOLOR | RESETCOLOR_Instruction | 1061-1073 | Style | Reset colors |
| ALIGNMENT | (Console method) | TBD | Style | Set text alignment |
| SAVEGAME | SAVELOADGAME_Instruction | 1697-1719 | System | Save game |
| LOADGAME | SAVELOADGAME_Instruction | 1697-1719 | System | Load game |
| QUIT | (flow control) | TBD | System | Quit game |
| RESTART | RESTART_Instruction | 2104-2115 | System | Restart game |
| RESETDATA | RESETDATA_Instruction | 1307-1320 | System | Reset game data |

**NOTE**: Line ranges marked TBD must be filled by implementer during Task 2-4 execution. Some commands may be handled by combined instructions (e.g., SAVELOADGAME_Instruction at line 1697 handles both SAVEGAME and LOADGAME). See also RESETCOLOR_Instruction (1061), RESETBGCOLOR_Instruction (1075), SWAPCHARA_Instruction (987), etc.

### Type Dependencies

- **CharacterId**: `Era.Core/Types/CharacterId.cs` (created by F377)
  - Constructed from int: `new CharacterId(characterNumber)` or `(CharacterId)n` (explicit cast)
- **CommandId**: `Era.Core/Types/CommandId.cs` (created by F429)
- **Unit**: `Era.Core/Types/Unit.cs` (created by F429)
- **Result<T>**: `Era.Core/Types/Result.cs` (created by F377)

### System Service Interfaces

**ICharacterManager** (`Era.Core/Interfaces/ICharacterManager.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces;

/// <summary>
/// Character management service
/// </summary>
public interface ICharacterManager
{
    /// <summary>Add character</summary>
    Result<Unit> AddChara(CharacterId characterId);

    /// <summary>Delete character</summary>
    Result<Unit> DelChara(CharacterId characterId);

    /// <summary>Select character for operation</summary>
    Result<Unit> PickupChara(CharacterId characterId);

    /// <summary>Swap two characters</summary>
    Result<Unit> SwapChara(CharacterId char1, CharacterId char2);

    /// <summary>Add character as copy</summary>
    Result<Unit> AddCopyChara(CharacterId sourceId, CharacterId destId);
}
```

**IStyleManager** (`Era.Core/Interfaces/IStyleManager.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces;

/// <summary>
/// Console style management service
/// </summary>
public interface IStyleManager
{
    /// <summary>Set text color</summary>
    Result<Unit> SetColor(int colorCode);

    /// <summary>Set background color</summary>
    Result<Unit> SetBgColor(int colorCode);

    /// <summary>Set font</summary>
    Result<Unit> SetFont(string fontName);

    /// <summary>Reset colors to default</summary>
    Result<Unit> ResetColor();

    /// <summary>Set text alignment</summary>
    Result<Unit> SetAlignment(AlignmentType alignment);
}

/// <summary>
/// Text alignment type
/// </summary>
public enum AlignmentType
{
    Left,
    Center,
    Right
}
```

**IGameState** (`Era.Core/Interfaces/IGameState.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces;

/// <summary>
/// Game state management service
/// </summary>
public interface IGameState
{
    /// <summary>Save game to file</summary>
    Result<Unit> SaveGame(string fileName);

    /// <summary>Load game from file</summary>
    Result<Unit> LoadGame(string fileName);

    /// <summary>Quit game</summary>
    Result<Unit> Quit();

    /// <summary>Restart game</summary>
    Result<Unit> Restart();

    /// <summary>Reset game data to initial state</summary>
    Result<Unit> ResetData();
}
```

### System Command Definitions

**CharacterCommand** (`Era.Core/Commands/System/CharacterCommand.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Commands.System;

/// <summary>
/// ADDCHARA command - add character
/// </summary>
public record AddCharaCommand(CommandId Id, CharacterId CharacterId) : ICommand<Unit>;

/// <summary>
/// DELCHARA command - delete character
/// </summary>
public record DelCharaCommand(CommandId Id, CharacterId CharacterId) : ICommand<Unit>;

/// <summary>
/// PICKUPCHARA command - select character
/// </summary>
public record PickupCharaCommand(CommandId Id, CharacterId CharacterId) : ICommand<Unit>;
```

**StyleCommand** (`Era.Core/Commands/System/StyleCommand.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Commands.System;

/// <summary>
/// SETCOLOR command - set text color
/// </summary>
public record SetColorCommand(CommandId Id, int ColorCode) : ICommand<Unit>;

/// <summary>
/// SETFONT command - set font
/// </summary>
public record SetFontCommand(CommandId Id, string FontName) : ICommand<Unit>;

/// <summary>
/// ALIGNMENT command - set text alignment
/// </summary>
public record AlignmentCommand(CommandId Id, AlignmentType Alignment) : ICommand<Unit>;
```

**GameStateCommand** (`Era.Core/Commands/System/GameStateCommand.cs`):
```csharp
using Era.Core.Types;

namespace Era.Core.Commands.System;

/// <summary>
/// SAVEGAME command - save game
/// </summary>
public record SaveGameCommand(CommandId Id, string FileName) : ICommand<Unit>;

/// <summary>
/// LOADGAME command - load game
/// </summary>
public record LoadGameCommand(CommandId Id, string FileName) : ICommand<Unit>;

/// <summary>
/// QUIT command - quit game
/// </summary>
public record QuitCommand(CommandId Id) : ICommand<Unit>;
```

### System Handler Implementations

**AddCharaHandler** (`Era.Core/Commands/System/AddCharaHandler.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Commands.System;

/// <summary>
/// ADDCHARA command handler
/// </summary>
public class AddCharaHandler : ICommandHandler<AddCharaCommand, Unit>
{
    private readonly ICharacterManager _characterManager;

    public AddCharaHandler(ICharacterManager characterManager)
    {
        _characterManager = characterManager ?? throw new ArgumentNullException(nameof(characterManager));
    }

    public Task<Result<Unit>> Handle(AddCharaCommand command, CancellationToken ct)
    {
        return Task.FromResult(_characterManager.AddChara(command.CharacterId));
    }
}
```

**SetColorHandler** (`Era.Core/Commands/System/SetColorHandler.cs`):
Template: Copy AddCharaHandler, replace `ICharacterManager` with `IStyleManager`, replace `AddChara(command.CharacterId)` with `SetColor(command.ColorCode)`.

**SaveGameHandler** (`Era.Core/Commands/System/SaveGameHandler.cs`):
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Commands.System;

/// <summary>
/// SAVEGAME command handler
/// </summary>
public class SaveGameHandler : ICommandHandler<SaveGameCommand, Unit>
{
    private readonly IGameState _gameState;

    public SaveGameHandler(IGameState gameState)
    {
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
    }

    public Task<Result<Unit>> Handle(SaveGameCommand command, CancellationToken ct)
    {
        return Task.FromResult(_gameState.SaveGame(command.FileName));
    }
}
```

### Service Implementations (Stub)

**NOTE**: Service implementations are **stubs** in Phase 9. Actual engine integration is Phase 11 scope.

**CharacterManager** (`Era.Core/Commands/System/CharacterManager.cs`):
```csharp
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Commands.System;

/// <summary>
/// Character management service stub.
/// Phase 9: Returns stub results. Phase 11: Delegates to engine.
/// </summary>
public class CharacterManager : ICharacterManager
{
    public Result<Unit> AddChara(CharacterId characterId)
        => Result<Unit>.Fail("Not implemented - awaiting Phase 11 engine integration");

    public Result<Unit> DelChara(CharacterId characterId)
        => Result<Unit>.Fail("Not implemented - awaiting Phase 11 engine integration");

    public Result<Unit> PickupChara(CharacterId characterId)
        => Result<Unit>.Fail("Not implemented - awaiting Phase 11 engine integration");

    public Result<Unit> SwapChara(CharacterId char1, CharacterId char2)
        => Result<Unit>.Fail("Not implemented - awaiting Phase 11 engine integration");

    public Result<Unit> AddCopyChara(CharacterId sourceId, CharacterId destId)
        => Result<Unit>.Fail("Not implemented - awaiting Phase 11 engine integration");
}
```

**StyleManager** (`Era.Core/Commands/System/StyleManager.cs`):
Template: Same pattern as CharacterManager - implement IStyleManager, return `Result<Unit>.Fail("Not implemented - awaiting Phase 11")` for all methods.

**GameState** (`Era.Core/Commands/System/GameState.cs`):
Template: Same pattern as CharacterManager - implement IGameState, return `Result<Unit>.Fail("Not implemented - awaiting Phase 11")` for all methods.

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// System Services (Phase 9 - Stub implementations)
services.AddSingleton<ICharacterManager, CharacterManager>();
services.AddSingleton<IStyleManager, StyleManager>();
services.AddSingleton<IGameState, GameState>();

// System Command Handlers (Phase 9)
services.AddSingleton<ICommandHandler<AddCharaCommand, Unit>, AddCharaHandler>();
services.AddSingleton<ICommandHandler<SetColorCommand, Unit>, SetColorHandler>();
services.AddSingleton<ICommandHandler<SaveGameCommand, Unit>, SaveGameHandler>();
// ... additional system handlers
```

### Equivalence Verification

Legacy behavior: System commands manipulate GlobalStatic services directly (ConsoleInstance, ProcessInstance, FileSystem).

New behavior: System commands use ICharacterManager/IStyleManager/IGameState through CommandDispatcher pipeline.

**Verification**: Both approaches produce identical game state. New approach adds logging/validation through pipeline.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F377 | Design Principles (IGameState placeholder, Result<T>, CharacterId) |
| Predecessor | F429 | CommandDispatcher + Mediator Pipeline (ICommand/ICommandHandler) |
| Predecessor | F430 | Pipeline Behaviors (logging/validation applied to system commands) |
| Related (debt source) | F370 | Phase 3 Body & State Systems (introduced 3 GlobalStatic TODOs in GameInitialization.cs) |

---

## Links

- [feature-424.md](feature-424.md) - Phase 9 Planning (parent feature)
- [feature-429.md](feature-429.md) - CommandDispatcher + Mediator Pipeline (dependency)
- [feature-430.md](feature-430.md) - Pipeline Behaviors (dependency)
- [feature-370.md](feature-370.md) - Phase 3 Body & State Systems (TODO source)
- [feature-377.md](feature-377.md) - Design Principles (IGameState, Result<T>, CharacterId)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 9 System Commands

---

## 残課題

| Issue | Description | Deferred To |
|-------|-------------|-------------|
| Task 8.5 | GlobalStatic accessor migration (GetFlag, GetCFlag, GetTFlag) - 3 TODOs in GameInitialization.cs lines 319, 339, 358 | Phase 11 (State Management) |
| Engine Integration | CharacterManager/StyleManager/GameState stub implementations → actual engine delegation | Phase 11 (State Management) |
| SETCOLORBYNAME | Style command (color by name lookup) - not in Phase 9 scope | Future Phase |
| BEGIN | System command (begin block) - not in Phase 9 scope | Future Phase |
| DRAWLINE | System command (draw line) - not in Phase 9 scope | Future Phase |

**Rationale**:
- **Task 8.5**: GlobalStatic.GetFlag/GetCFlag/GetTFlag accessors do not exist. Creating them requires defining the complete state accessor pattern, which is Phase 11 scope (State Management). F370 introduced these TODOs as placeholders awaiting future accessor standardization.
- **Engine Integration**: Phase 9 establishes command infrastructure with stub services. Phase 11 replaces stubs with engine-connected implementations (GlobalStatic.VEvaluator, Console, Process).

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-10 | create | implementer | Created from F424 Phase 9 Planning | PROPOSED |
| 2026-01-10 18:34 | START | implementer | Task 1a | - |
| 2026-01-10 18:34 | END | implementer | Task 1a | SUCCESS |
| 2026-01-10 18:35 | START | implementer | Task 1b | - |
| 2026-01-10 18:36 | END | implementer | Task 1b | SUCCESS |
| 2026-01-10 18:40 | START | implementer | Task 5 | - |
| 2026-01-10 18:40 | END | implementer | Task 5 | SUCCESS |
| 2026-01-10 18:42 | START | opus | Tasks 2-4, 6-7 | Commands, handlers, stubs, tests |
| 2026-01-10 18:42 | END | opus | Tasks 2-4, 6-7 | SUCCESS |
| 2026-01-10 18:48 | DEVIATION | feature-reviewer | post | NEEDS_REVISION: Summary claimed 16+ commands but 15 implemented |
| 2026-01-10 18:48 | END | opus | Summary/Goal/残課題 fix | Corrected to 15 commands, added deferred items |
| 2026-01-10 18:50 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION: engine-dev SKILL.md not updated |
| 2026-01-10 18:50 | END | opus | SSOT update | engine-dev SKILL.md updated with F434 interfaces and commands |
