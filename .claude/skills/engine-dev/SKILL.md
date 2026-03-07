---
name: engine-dev
description: uEmuera engine development reference. Use when modifying C# engine code, GlobalStatic interfaces, adding ERB commands, headless mode CLI options.
---

# Engine Reference (uEmuera)

> **Last Updated:** 2026-03-01
> **Purpose:** Essential engine knowledge for C# modifications

---

## Sub-File Loading

**MANDATORY**: After reading this entry point, load the sub-file(s) relevant to your task:

| Sub-File | Content | Load When |
|----------|---------|-----------|
| `INTERFACES.md` | All 39+ interface signatures with code examples | Implementing or modifying C# interfaces, checking method signatures |
| `PATTERNS.md` | Phase 3-20 migration patterns, DI, DDD, Variable System examples | Writing new C# code following established architecture patterns |

Load command: `Read(.claude/skills/engine-dev/INTERFACES.md)` or `Read(.claude/skills/engine-dev/PATTERNS.md)`

If your task involves BOTH interface changes AND pattern following, load BOTH files.

---

## C# Language Features

For C# 14 specific patterns (primary constructors, extension members, collection expressions), see:
- [csharp-14 skill](../csharp-14/SKILL.md)

---

## Naming Conventions

See [naming-conventions-15.md](../../../docs/architecture/naming-conventions-15.md) for Phase 15 audit results and Phase 16+ standards.

---

## GlobalStatic.cs (Service Locator)

| Property | Interface | Usage |
|----------|-----------|-------|
| `ProcessInstance` | `IProcess` | Script execution |
| `FileSystem` | `IFileSystem` | File I/O |
| `ParserMediatorInstance` | `IParserMediator` | ERB parsing, warnings |
| `ConfigInstance` | `IConfigReader` | Configuration |
| `ConsoleInstance` | `IEmueraConsole` | Console I/O |
| `CommandDispatcher` | `ICommandDispatcher` | Command routing |
| `ProcessErrorHandler` | `IProcessErrorHandler` | Error handling |
| `CharacterManagerInstance` | `ICharacterManager` | Character management (F461) |
| `StyleManagerInstance` | `IStyleManager` | Style management (F461) |
| `GameStateInstance` | `IGameState` | Game state (F461) |
| `VariableSizeService` | `IVariableSizeService` | YAML variable size config (F558) |
| `GameBaseService` | `IGameBaseService` | YAML game base config (F558) |
| `FatalErrorHandler` | `IFatalErrorHandler` | Fatal error exit handling (F592) |
| `ErrorDialogService` | `IErrorDialogService` | Error dialog: GUI (F594) / Console (F596) |
| `LocalizationService` | `ILocalizationService` | Error dialog localization (F598) |
| `ErrorAnalyticsService` | `IErrorAnalyticsService` | Error analytics collection (F597) |
| `PerformanceMetricsService` | `IPerformanceMetrics` | Performance metrics collection (F607) |
| `PerformanceAnalyticsService` | `IPerformanceAnalyticsService` | Performance analytics integration (F607) |
| `DashboardService` | `IDashboardService` | Real-time dashboard visualization (F606) |

---

## Key Directories

- `Assets/Scripts/Emuera/` - Pure C# (GameProc, GameData, GameView, Sub)
- `Assets/Scripts/Emuera/Services/` - Engine service implementations (CharacterManagerImpl, StyleManagerImpl, GameStateImpl) - F461 DI bridge
- `uEmuera.Headless/` - CLI project
- `src/Era.Core/Common/` - Game constants (static classes: Constants.cs, VariableDefinitions.cs, ColorSettings.cs, RelationshipTypes.cs) and service implementations
- `src/Era.Core/Event/` - Event handling functions (InfoEvent.cs)
- `src/Era.Core/State/` - Body, pregnancy, weather, NTR, INFO, and visitor settings state management (BodySettings.cs, GeneticsService.cs, PregnancySettings.cs, WeatherSettings.cs, NtrInitialization.cs, InfoState.cs, VisitorSettings.cs)
- `src/Era.Core/Orchestration/` - SHOW_STATUS orchestration coordinator (StatusOrchestrator.cs)
- `src/Era.Core/Interfaces/` - Service interfaces for DI (IGameInitializer, ILocationService, IInfoState, IVariableStore, ITrainingVariables, ICharacterStateVariables, IJuelVariables, ITEquipVariables, IStringVariables, I3DArrayVariables, ICharacterManager, IStyleManager, IConsoleOutput, IStateManager, IItemVariables (item variable arrays), etc. - 39 interfaces total). F789 added IStringVariables (SAVESTR string access) and I3DArrayVariables (TA/TB 3D integer arrays). F434 added ICharacterManager (ADDCHARA/DELCHARA/PICKUPCHARA/SWAPCHARA/ADDCOPYCHARA) and IStyleManager (SETCOLOR/SETBGCOLOR/SETFONT/RESETCOLOR/ALIGNMENT with AlignmentType enum). F434 extended IGameState with SaveGame/LoadGame/Quit/Restart/ResetData methods. F441 added IGameState.SetVariable(name, index, value) for COUNT:0 system variable integration (stub returns Fail until Phase 11). F475 added IStateManager for JSON save/load operations. F791 extended IGameState with BeginTrain/SaveGameDialog/LoadGameDialog methods for mode transition.
- `src/Era.Core/Types/` - Strongly typed IDs, variable indices, Result type, Unit type, and type conversion (CharacterId.cs, LocationId.cs, FlagIndex.cs, SaveStrIndex.cs, EquipmentIndex.cs, ITypeConverter.cs, TypeConverter.cs, Unit.cs, DialogueResult.cs, etc.) - F422 added type conversion, F430 added Unit type for void Result<Unit>, F476 added DialogueResult record for kojo dialogue rendering, F789 added SaveStrIndex for SAVESTR array indexing
- `src/Era.Core/Variables/` - Variable storage, scope, and resolution (VariableCode.cs, VariableStore.cs, VariableScope.cs, CharacterVariables.cs, LocalScope.cs, VariableResolver.cs, VariableDefinitionLoader.cs)
- `src/Era.Core/Training/` - Training lifecycle and processing (see PATTERNS.md Phase 6 section)
- `src/Era.Core/Character/` - Character state tracking domain: ICharacterStateTracker (orchestrator), IVirginityManager (LOST_VIRGIN*), IExperienceGrowthCalculator (EXP_GOT_CHECK), IPainStateChecker (PAIN_CHECK*) - F396
- `src/Era.Core/Characters/` - Character data service: ICharacterDataService/NullCharacterDataService (runtime CALLNAME resolution for dialogue rendering) - F628. **Note**: Distinct from src/Era.Core/Character/ (state tracking) and src/Era.Core/Functions/ICharacterDataAccess (CSV template access)
- `src/Era.Core/Expressions/` - Phase 8 expression system: IExpressionParser/ExpressionParser (AST parsing), ExpressionNode hierarchy (LiteralIntNode, LiteralStringNode, BinaryNode, UnaryNode, FunctionCallNode, VariableNode, TernaryNode), ArgsEndWith/TermEndWith enums, IMinimalEvaluator (equivalence testing) - F416; IOperatorRegistry (operator evaluation), IOperator/IBinaryOperator/IUnaryOperator (operator interfaces), OperatorRegistry (implementation with Dictionary<string, IBinaryOperator> Strategy pattern - F501), OperatorCategory enum - F417; `src/Era.Core/Expressions/Operators/` - IBinaryOperator implementations (29 operators: AddOperator, SubtractOperator, MultiplyOperator, DivideOperator, ModuloOperator, EqualOperator, NotEqualOperator, LessThanOperator, GreaterThanOperator, LessOrEqualOperator, GreaterOrEqualOperator, StringEqualOperator, StringNotEqualOperator, StringLessThanOperator, StringGreaterThanOperator, StringLessOrEqualOperator, StringGreaterOrEqualOperator, AndOperator, OrOperator, XorOperator, NotAndOperator, NotOrOperator, BitwiseAndOperator, BitwiseOrOperator, BitwiseXorOperator, LeftShiftOperator, RightShiftOperator, StringConcatOperator, StringRepeatOperator) - F501
- `src/Era.Core/Functions/` - Phase 8 function system: IFunctionRegistry/FunctionRegistry (function lookup/registration), IBuiltInFunction (function interface), IEvaluationContext (execution context) - F421; **stateless pure functions** (no engine dependencies): IMathFunctions, IRandom/IRandomFunctions, IConversionFunctions - F418; IStringFunctions, IArrayFunctions - F419; IStringFunctionsExtended/StringFunctionsExtended (STRCOUNT, CHARATU, TOHALF, TOFULL, ENCODETOUNI - extended string functions) - F425; IValueComparisonFunctions/ValueComparisonFunctions (GROUPMATCH, NOSAMES, ALLSAMES - variadic value comparison) - F426; game state functions: ICharacterFunctions/CharacterFunctions (21 character functions: GETCHARA, FINDCHARA, CSV*, GETPALAMLV, GETEXPLV etc.), ISystemFunctions/SystemFunctions (8 system functions: GETTIME, GETMILLISECOND, VARSIZE etc.), ICharacterDataAccess/NullCharacterDataAccess (character data abstraction) - F420. **Note**: For engine-dependent functions, see `Headless/Functions/`
- `engine/Assets/Scripts/Emuera/Headless/Functions/` - Engine-dependent functions (require runtime Console/ExpressionMediator): IEngineFunctions/EngineFunctions (LINEISEMPTY, GETLINESTR, STRFORM, STRJOIN), EngineFunctionsFactory - F428. **Architectural boundary**: stateless functions go in src/Era.Core/Functions/, stateful engine functions go here
- `src/Era.Core/Encoding/` - Encoding utilities: ShiftJisHelper.cs (Shift-JIS byte counting for String/Array functions) - F427
- `src/Era.Core/Random/` - Random number provider abstraction: IRandomProvider (Next, NextFromArray, Seed), SystemRandomProvider (production), SeededRandomProvider (deterministic testing), RandomConstants (NoSeed = -1) - F501. **Note**: Parallel interface to src/Era.Core/Functions/IRandom (F418); does NOT extend IRandom.
- `src/Era.Core/DependencyInjection/` - DI configuration (ServiceCollectionExtensions.cs, CallbackFactories.cs)
- `src/Era.Core/Commands/` - Phase 9 Command Infrastructure (Mediator Pattern): ICommand<TResult> (command marker with CommandId), ICommandHandler<TCommand,TResult> (handler interface returning Task<Result<TResult>>), IPipelineBehavior<TCommand,TResult> (pipeline behaviors for cross-cutting concerns), ICommandDispatcher (Mediator dispatcher with Dispatch<TResult> method), CommandDispatcher (implementation with DI-based handler resolution), CommandContext (execution context with CharacterId?, Scope?, Data?) - F429. **Note**: This is distinct from the legacy engine ICommandDispatcher (GlobalStatic.CommandDispatcher) which handles ERB script execution via FunctionCode lookup.
- `src/Era.Core/Commands/Behaviors/` - Pipeline behaviors for cross-cutting concerns (F430, F467): LoggingBehavior<TCommand,TResult> (command execution logging with timing), ValidationBehavior<TCommand,TResult> (input validation for IValidatable commands, returns Result<Unit> failure early), TransactionBehavior<TCommand,TResult> (F467: full UnitOfWork integration - commits on success, rolls back on exception; requires Scoped DI lifetime), IValidatable marker interface (optional validation contract with Validate() returning Result<Unit>). DI registration order: Logging -> Validation -> Transaction.
- `src/Era.Core/Commands/Flow/` - Phase 9 Flow Commands (F432, F441, F442): IExecutionStack (control flow nesting for IF/FOR/WHILE/REPEAT), IScopeManager (CALL/RETURN scope with isJump for JUMP semantics), ILabelResolver (function/label resolution with ResolveLocalLabel for GOTO). Flow handlers: IfHandler, ForHandler, WhileHandler, CallHandler, ReturnHandler (F432). REPEAT handlers: RepeatHandler, RendHandler (F441) - countup semantics (0->N-1), State tuple (Counter, LoopEnd), COUNT:0 integration via IGameState.SetVariable. Label handlers: GotoHandler, JumpHandler, TryGotoHandler, TryJumpHandler (F442).
- `src/Era.Core/Commands/Print/` - Phase 9 Print Commands (F431): 7 print command handlers migrated from legacy GameProc. Commands: PrintCommand, PrintLCommand, PrintWCommand, PrintFormCommand, PrintDataCommand, PrintButtonCommand, PrintButtonCCommand. Handlers delegate to IConsoleOutput abstraction (implemented by runtime hosts). All handlers registered as Singletons in DI.
- `src/Era.Core/Commands/Special/` - Phase 9/13 Special Commands: SCOMF1-16 command handlers (ScomfCommands.cs defining 16 Scomf{N}Command records, Scomf{N}Handler.cs files implementing ICommandHandler) for special training scenarios. F435 (Phase 9) created stubs, F469 (Phase 13) added SCOMF variable infrastructure, F473 (Phase 13) implemented full SCOMF logic (SOURCE/STAIN/EXP/TCVAR updates).
- `src/Era.Core/Commands/System/` - Phase 9 System Commands (F434): Character commands (AddCharaCommand/Handler, DelCharaCommand/Handler, PickupCharaCommand/Handler, SwapCharaCommand/Handler, AddCopyCharaCommand/Handler), Style commands (SetColorCommand/Handler, SetBgColorCommand/Handler, SetFontCommand/Handler, ResetColorCommand/Handler, AlignmentCommand/Handler), GameState commands (SaveGameCommand/Handler, LoadGameCommand/Handler, QuitCommand/Handler, RestartCommand/Handler, ResetDataCommand/Handler). Stub service implementations (CharacterManager.cs, StyleManager.cs, GameState.cs) return Result.Fail until Phase 11 engine integration.
- `src/Era.Core/Commands/Variable/` - Phase 9 Variable & Array Commands (F433): VarSetCommand/VarSetHandler (VARSET batch variable assignment), VarSizeCommand/VarSizeHandler (VARSIZE array size), ArrayCopyCommand/ArrayCopyHandler (ARRAYCOPY), ArrayRemoveCommand/ArrayRemoveHandler (ARRAYREMOVE), ArraySortCommand/ArraySortHandler (ARRAYSORT), ArrayShiftCommand/ArrayShiftHandler (ARRAYSHIFT). Handlers inject IVariableResolver/IVariableStore but defer actual manipulation to Phase 11.
- `src/Era.Core/Commands/Com/` - Phase 12 COM System (F452, F464): ICom (base interface), IEquipmentCom (ISP-segregated equipment interface), IComRegistry (auto-discovery registry with attribute-based lookup), IComContext (execution context), ComBase (standard COM base class), EquipmentComBase (equipment-enabled COM base class), ComIdAttribute (legacy ID preservation). **F464 Directory Structure (semantic naming)**: Daily/ (17 daily), Training/{Touch,Oral,Penetration,Equipment,Bondage,Undressing,Utility}/ (90 training), Utility/ (22 action), Masturbation/, Visitor/ (4 NTR), System/ (2: DayEnd[888], Dummy[999]). 135 total COMs with semantic class names (e.g., ClitoralCap, Missionary) and [ComId(N)] attributes.

---

## Extension Points

### Safe Extensions
1. **New ERB Commands**: `Instruction.cs` + `Process.ScriptProc.cs`
2. **New System Functions**: `FunctionMethod.cs`
3. **New Variables**: `VariableCode.cs` + `ConstantData.cs`

### Avoid
- Modifying `GlobalStatic.cs` without thorough testing
- Changing core interfaces without updating all implementations

---

## Headless Mode

**CLI Options**: `--unit <func>`, `--debug`, `--parallel [N]`, `--fail-fast`, `--diff`

**Build**: `dotnet build engine/uEmuera.Headless.csproj`

### HeadlessUI (F479)

Console-based UI for headless game execution (testing and CI/CD).

```csharp
// src/Era.Core/HeadlessUI.cs
public class HeadlessUI
{
    // Output dialogue to console with [Dialogue] prefix
    void OutputDialogue(DialogueResult dialogue);

    // Output game state with [State] prefix
    void OutputState(GameTick tick);

    // Interactive console input with "> " prompt
    string ReadInput();

    // Scripted input for automated tests
    string ReadScriptedInput(Queue<string> scriptedInputs);
}

// GameEngine.IsHeadless convenience property
public bool IsHeadless => _config?.HeadlessMode ?? false;
```

---

## Testability Pattern

### GlobalStatic DI
```csharp
// GlobalStatic.cs
private static IFileSystem _fileSystem = new FileSystem();
public static IFileSystem FileSystem
{
    get => _fileSystem;
    set => _fileSystem = value ?? new FileSystem();
}
```

**Test Usage**:
```csharp
GlobalStatic.FileSystem = mockFileSystem;  // Setup
GlobalStatic.FileSystem = null;            // Teardown (resets to default)
```

---

## Procedure

1. Read `pm/features/feature-{ID}.md` for task requirements
2. Identify target C# files using Glob in `engine/` directory
3. Read existing interface patterns and dependencies
4. Implement changes following existing conventions
5. Run `dotnet build` to verify compilation
6. Run `dotnet test` to verify tests pass

## Quality

### Required Items

- [ ] Use GlobalStatic for service dependencies
- [ ] Add unit tests in engine repo (`C:\Era\engine\engine.Tests/`) for new code
- [ ] Follow existing conventions (PascalCase methods, camelCase locals)
- [ ] Use nullable annotations (`?`) where appropriate
- [ ] XML comments for public APIs

### Recommended Items

- [ ] Add detailed inline comments for complex logic
- [ ] Include usage examples in XML comments

### NG Items

| Situation | NG Expression |
|-----------|---------------|
| Service dependency | Direct instantiation instead of GlobalStatic |
| Flow control | Using exceptions for control flow |
| Interface changes | Modifying interface without updating all implementations |

---

## Constraints

| Constraint | Rationale |
|------------|-----------|
| No direct GlobalStatic modification (except tests) | Maintain DI pattern |
| No exception-based flow control | Performance and readability |
| Update all implementations when changing interface | Type safety |
