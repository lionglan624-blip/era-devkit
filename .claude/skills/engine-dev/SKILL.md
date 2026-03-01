---
name: engine-dev
description: uEmuera engine development reference. Use when modifying C# engine code, GlobalStatic interfaces, adding ERB commands, headless mode CLI options.
---

# Engine Reference (uEmuera)

> **Last Updated:** 2026-02-12
> **Purpose:** Essential engine knowledge for C# modifications

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

## Core Interfaces

### IProcess
```csharp
void Initialize();
void DoScript();
void Input(string input);
ProcessState State { get; }
```

### IFileSystem
```csharp
bool FileExists(string path);
string ReadAllText(string path, Encoding encoding);
void WriteAllText(string path, string contents, Encoding encoding);
string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
DateTime GetLastWriteTime(string path);  // F580: Cache invalidation support
```

### IParserMediator
```csharp
IWarningCollector WarningCollector { get; set; }
void Initialize(EmueraConsole console);
void Warn(string str, ScriptPosition pos, int level);
void FlushWarningList();
```

### IWarningCollector
```csharp
void AddWarning(string str, ScriptPosition pos, int level, string stack);
bool HasWarnings { get; }
void Flush();
IEnumerable<Warning> GetWarnings();
```

### IEmueraConsole
```csharp
void Print(string text);
void PrintLine(string text);
void WaitInput();
void SetWindowTitle(string str);
bool IsTimeOut { get; }
StringStyle StringStyle { get; }
```

### ICommandDispatcher
```csharp
void InitializeCommands();
bool TryExecuteCommand(ScriptCommandContext ctx, InstructionLine func);
(bool found, bool continueExec) TryExecuteFlowCommand(ScriptCommandContext ctx, InstructionLine func);
```

### IFatalErrorHandler (F592)
```csharp
// engine/Assets/Scripts/Emuera/Services/IFatalErrorHandler.cs - Fatal error handling
void HandleFatalError(FatalErrorType errorType, string message, Exception? exception = null);
// Logs error to Console.Error and calls Environment.Exit(1) via IEnvironmentExitHandler

// FatalErrorType enum: YamlLoadFailure, ConfigurationMissing, InitializationFailure
// FatalErrorExitCodes.FATAL_ERROR_EXIT_CODE = 1
```
**Note**: IEnvironmentExitHandler abstracts Environment.Exit() for testability.

### IErrorDialogService (F594, F596)
```csharp
// engine/Assets/Scripts/Emuera/Services/IErrorDialogService.cs - Error dialog interface
void ShowFatalError(string title, string message, Exception exception);  // Fatal error with exception
void ShowRuntimeError(string title, string message);  // Runtime error without exception
void ShowConfigurationError(string message);  // Configuration error
```
**Implementations**: UnityErrorDialog (GUI, MonoBehaviour-based), ConsoleErrorHandler (headless, plain C#).
**Note**: GlobalStatic.Reset() sets to null. HeadlessRunner registers ConsoleErrorHandler on startup.

### ILocalizationService (F598)
```csharp
// engine/Assets/Scripts/Emuera/Services/ILocalizationService.cs - Localization interface
string GetLocalizedErrorMessage(string errorKey);          // Error message translation
string GetLocalizedRecoverySuggestion(string suggestionKey); // Recovery suggestion localization
string[] LocalizeDialogButtons(string[] buttonTexts);      // Dialog button localization
string GetDefaultLanguage();                               // Default language fallback
void SetPreferredLanguage(string languageCode);            // Set language preference via PlayerPrefs
string GetPreferredLanguage();                             // Get language preference
void ValidateTranslationKeys(string[] requiredKeys);       // Translation key validation
```
**Implementation**: DefaultLocalizationService (pass-through fallback, PlayerPrefs persistence).
**Note**: UnityErrorDialog uses LocalizationService?.GetLocalizedErrorMessage() with null-safe fallback. Translation content deferred to future feature.

### IErrorAnalyticsService (F597, F604)
```csharp
// engine/Assets/Scripts/Emuera/Services/IErrorAnalyticsService.cs - Error analytics interface
bool HasAnalyticsConsent { get; }                          // Check if user granted consent
void SetAnalyticsConsent(bool consent);                    // Set user consent for analytics
void CollectErrorData(string errorType, string message, string stackTrace); // Collect error data (only if consent granted)
string AnonymizeMessage(string message);                   // Anonymize sensitive data (user names, IPs, file paths)
IEnumerable<ErrorMetric> GetCollectedMetrics();            // Retrieve collected metrics (read-only)
Result<Unit> SaveErrorMetrics(string filePath = null);     // Save metrics to local JSON file (F604)
```
**Implementation**: ErrorAnalyticsService (in-memory storage, privacy-first design).
**Note**: Analytics disabled by default. UnityErrorDialog calls CollectErrorData automatically. ErrorMetric record holds Type, Message, StackTrace, Timestamp. MetricsSnapshot record for JSON serialization (F604).

### IRemoteTransmission (F605)
```csharp
// engine/Assets/Scripts/Emuera/Services/RemoteTransmission/IRemoteTransmission.cs - Remote analytics transmission interface
Task<bool> TransmitAsync(IEnumerable<ErrorMetric> metrics, TransmissionConfig config); // Transmit metrics to remote endpoint
Task<bool> ValidateEndpointAsync(string endpoint);                                      // Validate endpoint URL (HTTPS required)
```
**Implementation**: HttpTransmissionService (System.Net.Http, cross-platform).
**Components**:
- TransmissionConfig: Endpoint URL, timeout, retry, batching settings
- PrivacySettings: Opt-in/opt-out, data filtering controls
- RetryPolicy: Exponential backoff for failed transmissions
- BatchTransmission: Batch processing for efficient network usage
- RateLimitingService: Rate limiting protection
- AuthenticationProvider: API key authentication
- NetworkFailureHandler: Network failure detection
- InvalidEndpointException: Validation error for malformed endpoints
**Note**: Transmission disabled by default via PrivacySettings. Uses ErrorMetric from IErrorAnalyticsService.

### IPerformanceMetrics (F607)
```csharp
// src/Era.Core/Monitoring/IPerformanceMetrics.cs - Performance metrics interface
void RecordExecutionTime(string operation, long milliseconds);  // Operation execution time
void RecordMemoryUsage(string component, long bytes);           // Component memory usage
void RecordResourceLoadTime(string resource, long milliseconds); // Resource loading time
void SetPerformanceThreshold(string metric, double threshold);   // Performance threshold
PerformanceMetricsSnapshot GetCurrentMetrics();                  // Current metrics snapshot
```
**Implementation**: PerformanceMetricsService (src/Era.Core/Monitoring/PerformanceMetricsService.cs, in-memory storage).
**Note**: Throws ArgumentNullException for null operation/component/resource names, ArgumentOutOfRangeException for negative values. PerformanceMetricsSnapshot record holds ExecutionTimes, MemoryUsages, ResourceLoadTimes, Thresholds.

### IPerformanceAnalyticsService (F607)
```csharp
// src/Era.Core/Monitoring/IPerformanceAnalyticsService.cs - Performance analytics interface
void AnalyzeMetrics(PerformanceMetricsSnapshot metrics);  // Analyze performance metrics
```
**Implementation**: PerformanceAnalyticsService (src/Era.Core/Monitoring/PerformanceAnalyticsService.cs).
**Note**: Integrates performance metrics with existing analytics infrastructure from F597.

### IDashboardService (F606)
```csharp
// engine/Assets/Scripts/Emuera/Services/IDashboardService.cs - Real-time dashboard interface
Task<Result<Unit>> StartServer(int port = 8080);          // Start dashboard HTTP server
Task<Result<Unit>> StopServer();                          // Stop dashboard HTTP server
Task<DashboardData> GetRealTimeMetrics();                 // Get current metrics for display
bool IsRunning { get; }                                   // Check if server is running
```
**Implementation**: DashboardService (engine/Assets/Scripts/Emuera/Services/DashboardService.cs).
**Supporting types**: DashboardData (ErrorMetrics, SystemHealthMetrics, LastUpdated), ErrorMetrics (TotalErrors, ErrorsLastHour, ErrorsByType, RecoveryRate), SystemHealthMetrics (Status, MemoryUsageMB, CpuUsagePercent, ActiveConnections).
**Features**: Embedded HttpListener on port 8080, WebSocket live updates every 5 seconds, Chart.js dashboard at `/`, metrics API at `/api/metrics`.
**Note**: Integrates with IErrorAnalyticsService (F597) for error data. PerformanceCounter is Windows-specific (cross-platform support deferred to F655).

### ISpecification<T> (F546)
```csharp
// src/Era.Core/Dialogue/Specifications/ISpecification.cs - Specification Pattern interface
// Phase 18 KojoEngine SRP分割: type-safe, composable condition evaluation for TALENT/ABL/EXP branching
bool IsSatisfiedBy(T entity);                        // Evaluate if entity satisfies this specification
ISpecification<T> And(ISpecification<T> other);      // Composite: both specifications satisfied
ISpecification<T> Or(ISpecification<T> other);       // Composite: either specification satisfied
ISpecification<T> Not();                             // Negation: specification not satisfied

// src/Era.Core/Dialogue/Specifications/SpecificationBase.cs - Abstract base class
// Provides default And/Or/Not implementations, concrete specs only implement IsSatisfiedBy
public abstract class SpecificationBase<T> : ISpecification<T>
{
    public abstract bool IsSatisfiedBy(T entity);    // Implement in concrete specifications
    public virtual ISpecification<T> And(ISpecification<T> other) => new AndSpecification<T>(this, other);
    public virtual ISpecification<T> Or(ISpecification<T> other) => new OrSpecification<T>(this, other);
    public virtual ISpecification<T> Not() => new NotSpecification<T>(this);
}

// Composite specifications (public, F548):
// AndSpecification<T>, OrSpecification<T>, NotSpecification<T> : SpecificationBase<T>
```
**Related features**: F547 (Concrete Specifications: TALENTSpecification, ABLThresholdSpecification), F548 (Composite Specifications: public versions with SpecificationBase inheritance).

### ILabelResolver (F432, F442)
```csharp
Result<int> ResolveLabel(string labelName);  // @FUNCTION resolution
bool TryResolveLabel(string labelName, out int lineNumber);  // TRYJUMP
Result<int> ResolveLocalLabel(string labelName, string currentFunctionName);  // GOTO $LABEL
bool TryResolveLocalLabel(string labelName, string currentFunctionName, out int lineNumber);  // TRYGOTO
Result<Unit> RegisterLabel(string labelName, int lineNumber);  // Label registration
```

### IScopeManager (F432, F442)
```csharp
Result<Unit> PushScope(string functionName, int returnLine, bool isJump = false);  // isJump for JUMP semantics
bool TryPushScope(string functionName, int returnLine, bool isJump = false);  // TRYJUMP
Result<(string functionName, int returnLine)> PopScope();
int Depth { get; }
```

### IConsoleOutput (F431)
```csharp
// src/Era.Core/Interfaces/IConsoleOutput.cs - Print command abstraction
Result<Unit> Print(string text);           // PRINT - inline output
Result<Unit> PrintLine(string text);       // PRINTL - with newline
Result<Unit> PrintWait(string text);       // PRINTW - wait for input
Result<Unit> PrintForm(string format, params object[] args);  // PRINTFORM
Result<Unit> PrintData(string[] selectedLines);  // PRINTDATA - pre-selected lines
Result<Unit> PrintButton(string text, long value);   // PRINTBUTTON (int)
Result<Unit> PrintButton(string text, string value); // PRINTBUTTON (string)
Result<Unit> PrintButtonCentered(string text, long value);  // PRINTBUTTONC
Result<Unit> DrawLine();                                     // DRAWLINE - separator line with newline (F788)
Result<Unit> ClearLine(int count);                           // CLEARLINE - delete N lines, best-effort (F788)
Result<Unit> PrintColumnLeft(string text);                   // PRINTLC - left-column PadRight(20), NOT centering (F788)
Result<Unit> Bar(long value, long max, long length);         // BAR - visual bar [*****...] (F788)
```
**Note**: IConsoleOutput is implemented by runtime hosts (Headless/GUI), not Era.Core.

### IKojoEngine (F452→F476)
```csharp
// src/Era.Core/IKojoEngine.cs - Dialogue engine for YAML kojo parsing and condition evaluation
// F476: Simplified to single-method interface with Result type, replacing F452 three-method design
Result<DialogueResult> GetDialogue(CharacterId character, ComId com, IEvaluationContext ctx);
// Returns Success with DialogueResult if dialogue found, Fail if file missing or no matching condition
// F681: Multi-entry selection and rendering - selects all matching entries, aggregates DialogueLines with per-entry displayMode
Result<DialogueResult> GetDialogueMulti(CharacterId character, ComId com, IEvaluationContext ctx);
// Returns Success with aggregated DialogueResult from all matching entries, Fail if file missing or no entries match

// src/Era.Core/Types/DialogueResult.cs - F476/F676/F683: Immutable record for dialogue selection results
// F676: Added DialogueLine structured type with display metadata, Lines property for backward compat
// F683: Lines property marked [Obsolete] - use DialogueLines instead
public record DialogueResult
{
    public IReadOnlyList<DialogueLine> DialogueLines { get; }  // F676: Structured lines with display metadata
    [Obsolete] public IReadOnlyList<string> Lines { get; }     // F683: DEPRECATED - use DialogueLines instead
    public static DialogueResult Create(IReadOnlyList<DialogueLine> dialogueLines);  // Factory method (private constructor)
}

// src/Era.Core/Types/DialogueResult.cs - F676: Single dialogue line with display mode metadata
public record DialogueLine(string Text, DisplayMode DisplayMode);
```

### IDialogueLoader (F542)
```csharp
// src/Era.Core/Dialogue/Loading/IDialogueLoader.cs - File loading interface (Phase 18 SRP分割)
// Responsible only for file I/O operations, not evaluation or rendering
Result<DialogueFile> Load(string path);
Result<IReadOnlyList<DialogueFile>> LoadAll(string directory);
// DialogueFile record at src/Era.Core/Dialogue/DialogueFile.cs (F542)
// DialogueEntry record for individual entries with Id, Content, Priority (int), Condition (DialogueCondition?), and DisplayMode (DisplayMode) properties - F552/F676
// F676: Added DisplayMode property for display variant behavior (default: DisplayMode.Default)

// src/Era.Core/Dialogue/DialogueFile.cs - F676: Display mode for dialogue rendering behavior
public enum DisplayMode { Default, Newline, Wait, KeyWait, KeyWaitNewline, KeyWaitWait, Display, DisplayNewline, DisplayWait }
```

### IDialogueRenderer (F544)
```csharp
// src/Era.Core/Dialogue/Rendering/IDialogueRenderer.cs - Template rendering interface (Phase 18 SRP分割)
// Responsible only for template expansion with variable substitution, not loading or condition evaluation
Result<string> Render(string template, IEvaluationContext context);
// Returns Success with rendered string, Fail on template syntax error or undefined variable
// Template format: "{CALLNAME}", "{TALENT:恋慕}" etc. - implementation in F551 (TemplateDialogueRenderer)
```

### IConditionEvaluator (F543)
```csharp
// src/Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs - Condition evaluation interface (Phase 18 SRP分割)
// Responsible only for evaluating dialogue conditions, not loading or rendering
bool Evaluate(DialogueCondition condition, IEvaluationContext context);
// Returns true if condition is satisfied, false otherwise
// DialogueCondition record at src/Era.Core/Dialogue/Conditions/DialogueCondition.cs (F543)
// Record fields: Type, TalentType?, AblType?, Threshold?, Operand?, Operands?, SingleOperand? - implementation in F550 (ConditionEvaluator)
```

### IDialogueSelector (F545)
```csharp
// src/Era.Core/Dialogue/Selection/IDialogueSelector.cs - Dialogue entry selection interface (Phase 18 SRP分割)
// Responsible only for selecting appropriate dialogue entry based on priority and conditions
Result<DialogueEntry> Select(IReadOnlyList<DialogueEntry> entries, IEvaluationContext context);
// Returns Success with selected DialogueEntry, Fail if no entry matches conditions
// F681: Multi-entry selection - returns all matching entries in document order (not priority order)
Result<IReadOnlyList<DialogueEntry>> SelectAll(IReadOnlyList<DialogueEntry> entries, IEvaluationContext context);
// Default: throws NotSupportedException. PriorityDialogueSelector implements with document-order filtering
// WARNING: SelectAll may return entries from mutually exclusive branches (IF/ELSEIF) - consumer responsibility
// DialogueEntry record at src/Era.Core/Dialogue/DialogueFile.cs (F542) - stub, F546-F547 full implementation
// Selection algorithm in F552 (PriorityDialogueSelector): filter by condition → sort by priority → return highest
```

### IGameEngine (F474)
```csharp
// src/Era.Core/IGameEngine.cs - Core game engine for main game loop
IGameState State { get; }                        // Game state control interface for system commands
Result<GameTick> ProcessTurn();                  // Process one game turn (returns Ok<GameTick> or Fail)
void Initialize(GameConfig config);             // Initialize engine with configuration

// src/Era.Core/Types/GameConfig.cs - Configuration for engine initialization
public sealed record GameConfig(string GamePath, bool HeadlessMode = false);

// src/Era.Core/Types/GameTick.cs - Game turn tick result
public readonly record struct GameTick(int TurnNumber, bool IsComplete);

// src/Era.Core/GameEngine.cs - Implementation
// Constructor: GameEngine(IGameState gameState)
// ProcessTurn returns Fail("ゲームエンジンが初期化されていません") if not initialized
```

### ICommandProcessor (F477)
```csharp
// src/Era.Core/CommandProcessor.cs - COM execution orchestrator
Result<ComResult> Execute(ComId comId, IComContext context);  // Resolve handler via IComRegistry.TryGet, execute COM

// src/Era.Core/CommandProcessor.cs - Implementation
// Constructor: CommandProcessor(IComRegistry registry)
// Returns Fail("COM{id}のハンドラーが見つかりません") if handler not found
// Propagates ICom.Execute result directly
```

### IInputHandler (F481, extended F777)
```csharp
// src/Era.Core/Input/IInputHandler.cs - Input command processing
Result<Unit> RequestNumericInput(string prompt, int? min = null, int? max = null);
Result<Unit> RequestStringInput(string prompt);
Result<object> ProvideInput(string input);  // Returns boxed int or string
bool IsWaitingForInput { get; }
Result<Unit> RequestOneInput(string prompt);  // ONEINPUT equivalent - F777

// src/Era.Core/Input/InputHandler.cs - Implementation
// src/Era.Core/Input/InputRequest.cs - Value object (Numeric/String factory methods)
// src/Era.Core/Input/InputValidator.cs - Range validation (min/max checking)
// Single pending request only - duplicate requests rejected with Result.Fail
```

### INtrEngine (F478)
```csharp
// src/Era.Core/INtrEngine.cs - NTR parameter calculation engine
Result<NtrParameters> Calculate(CharacterId target, CharacterId actor, NtrAction action);
// Returns Success with NtrParameters if calculated, Fail if invalid input
// Validation: negative CharacterId or self-NTR (target == actor) returns Fail

// src/Era.Core/Types/NtrAction.cs - NTR action types
public enum NtrAction { Witness, Report, Rumor, Direct }  // NTR action types affecting affection and jealousy

// src/Era.Core/Types/NtrParameters.cs - NTR calculation results
public record NtrParameters(int AffectionChange, int JealousyValue);
// AffectionChange: -100 to +100, JealousyValue: 0 to 100
```

### IStateManager (F475)
```csharp
// src/Era.Core/Interfaces/IStateManager.cs - State persistence for JSON save/load
Result<GameSaveState> Load(string path);                    // Load state from JSON file
Result<Unit> Save(string path, GameSaveState state);        // Save state to JSON file
// Returns Success on successful I/O, Fail on file not found/invalid JSON/I/O error
// Uses System.Text.Json for serialization, UTF-8 encoding

// src/Era.Core/StateManager.cs - Implementation
// Constructor: StateManager() (no dependencies - pure I/O)
// Error messages in Japanese: "セーブファイルが見つかりません", "JSON形式が不正です", etc.
```

### ICom / IComRegistry / IComContext (F452, F464)
```csharp
// src/Era.Core/Commands/Com/ICom.cs - Base COM interface
ComId Id { get; }
string Name { get; }
Result<ComResult> Execute(IComContext context);

// src/Era.Core/Commands/Com/IEquipmentCom.cs - ISP-segregated equipment interface (extends ICom)
void ExecuteEquipmentEffect(CharacterId target, EquipmentResult result);

// src/Era.Core/Commands/Com/IComRegistry.cs - Auto-discovery registry (F464: attribute-based)
ICom Get(ComId id);                         // Get COM or throw KeyNotFoundException
bool TryGet(ComId id, out ICom com);        // Safe get
IEnumerable<ICom> GetAll();                 // All discovered COMs

// src/Era.Core/Commands/Com/ComIdAttribute.cs - Legacy ID attribute (F464)
[ComId(42)] public class ClitoralCap : EquipmentComBase  // Semantic name + ID preserved

// src/Era.Core/Commands/Com/IComContext.cs - Execution context
CharacterId Target { get; }                 // Training target
CharacterId Actor { get; }                  // Executing character
IAbilitySystem Abilities { get; }           // Ability growth
IKojoEngine Kojo { get; }                   // Dialogue rendering
Dictionary<string, object> EvalContext { get; }    // Condition evaluation context
Dictionary<string, string> Placeholders { get; }   // Render placeholders
```

### IEntryPointRegistry (F791)
```csharp
// src/Era.Core/Functions/IEntryPointRegistry.cs - Procedure-style entry point dispatch
Result<int> Invoke(string name);            // Call registered entry point by name (case-insensitive)
void Register(string name, Func<int> handler);  // Register entry point handler
// Returns Result<int> to support both void (returns 0) and int-returning procedures (SHOW_SHOP, USERSHOP)
// Distinct from IFunctionRegistry (IBuiltInFunction returns Result<object>, not compatible with void/int procedures)
```

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
- `src/Era.Core/Training/` - Training lifecycle and processing (see Phase 6 Training Pattern section for complete file listing)
- `src/Era.Core/Character/` - Character state tracking domain: ICharacterStateTracker (orchestrator), IVirginityManager (LOST_VIRGIN*), IExperienceGrowthCalculator (EXP_GOT_CHECK), IPainStateChecker (PAIN_CHECK*) - F396
- `src/Era.Core/Characters/` - Character data service: ICharacterDataService/NullCharacterDataService (runtime CALLNAME resolution for dialogue rendering) - F628. **Note**: Distinct from src/Era.Core/Character/ (state tracking) and src/Era.Core/Functions/ICharacterDataAccess (CSV template access)
- `src/Era.Core/Expressions/` - Phase 8 expression system: IExpressionParser/ExpressionParser (AST parsing), ExpressionNode hierarchy (LiteralIntNode, LiteralStringNode, BinaryNode, UnaryNode, FunctionCallNode, VariableNode, TernaryNode), ArgsEndWith/TermEndWith enums, IMinimalEvaluator (equivalence testing) - F416; IOperatorRegistry (operator evaluation), IOperator/IBinaryOperator/IUnaryOperator (operator interfaces), OperatorRegistry (implementation with Dictionary<string, IBinaryOperator> Strategy pattern - F501), OperatorCategory enum - F417; `src/Era.Core/Expressions/Operators/` - IBinaryOperator implementations (29 operators: AddOperator, SubtractOperator, MultiplyOperator, DivideOperator, ModuloOperator, EqualOperator, NotEqualOperator, LessThanOperator, GreaterThanOperator, LessOrEqualOperator, GreaterOrEqualOperator, StringEqualOperator, StringNotEqualOperator, StringLessThanOperator, StringGreaterThanOperator, StringLessOrEqualOperator, StringGreaterOrEqualOperator, AndOperator, OrOperator, XorOperator, NotAndOperator, NotOrOperator, BitwiseAndOperator, BitwiseOrOperator, BitwiseXorOperator, LeftShiftOperator, RightShiftOperator, StringConcatOperator, StringRepeatOperator) - F501
- `src/Era.Core/Functions/` - Phase 8 function system: IFunctionRegistry/FunctionRegistry (function lookup/registration), IBuiltInFunction (function interface), IEvaluationContext (execution context) - F421; **stateless pure functions** (no engine dependencies): IMathFunctions, IRandom/IRandomFunctions, IConversionFunctions - F418; IStringFunctions, IArrayFunctions - F419; IStringFunctionsExtended/StringFunctionsExtended (STRCOUNT, CHARATU, TOHALF, TOFULL, ENCODETOUNI - extended string functions) - F425; IValueComparisonFunctions/ValueComparisonFunctions (GROUPMATCH, NOSAMES, ALLSAMES - variadic value comparison) - F426; game state functions: ICharacterFunctions/CharacterFunctions (21 character functions: GETCHARA, FINDCHARA, CSV*, GETPALAMLV, GETEXPLV etc.), ISystemFunctions/SystemFunctions (8 system functions: GETTIME, GETMILLISECOND, VARSIZE etc.), ICharacterDataAccess/NullCharacterDataAccess (character data abstraction) - F420. **Note**: For engine-dependent functions, see `Headless/Functions/`
- `engine/Assets/Scripts/Emuera/Headless/Functions/` - Engine-dependent functions (require runtime Console/ExpressionMediator): IEngineFunctions/EngineFunctions (LINEISEMPTY, GETLINESTR, STRFORM, STRJOIN), EngineFunctionsFactory - F428. **Architectural boundary**: stateless functions go in src/Era.Core/Functions/, stateful engine functions go here
- `src/Era.Core/Encoding/` - Encoding utilities: ShiftJisHelper.cs (Shift-JIS byte counting for String/Array functions) - F427
- `src/Era.Core/Random/` - Random number provider abstraction: IRandomProvider (Next, NextFromArray, Seed), SystemRandomProvider (production), SeededRandomProvider (deterministic testing), RandomConstants (NoSeed = -1) - F501. **Note**: Parallel interface to src/Era.Core/Functions/IRandom (F418); does NOT extend IRandom.
- `src/Era.Core/DependencyInjection/` - DI configuration (ServiceCollectionExtensions.cs, CallbackFactories.cs)
- `src/Era.Core/Commands/` - Phase 9 Command Infrastructure (Mediator Pattern): ICommand<TResult> (command marker with CommandId), ICommandHandler<TCommand,TResult> (handler interface returning Task<Result<TResult>>), IPipelineBehavior<TCommand,TResult> (pipeline behaviors for cross-cutting concerns), ICommandDispatcher (Mediator dispatcher with Dispatch<TResult> method), CommandDispatcher (implementation with DI-based handler resolution), CommandContext (execution context with CharacterId?, Scope?, Data?) - F429. **Note**: This is distinct from the legacy engine ICommandDispatcher (GlobalStatic.CommandDispatcher) which handles ERB script execution via FunctionCode lookup.
- `src/Era.Core/Commands/Behaviors/` - Pipeline behaviors for cross-cutting concerns (F430, F467): LoggingBehavior<TCommand,TResult> (command execution logging with timing), ValidationBehavior<TCommand,TResult> (input validation for IValidatable commands, returns Result<Unit> failure early), TransactionBehavior<TCommand,TResult> (F467: full UnitOfWork integration - commits on success, rolls back on exception; requires Scoped DI lifetime), IValidatable marker interface (optional validation contract with Validate() returning Result<Unit>). DI registration order: Logging → Validation → Transaction.
- `src/Era.Core/Commands/Flow/` - Phase 9 Flow Commands (F432, F441, F442): IExecutionStack (control flow nesting for IF/FOR/WHILE/REPEAT), IScopeManager (CALL/RETURN scope with isJump for JUMP semantics), ILabelResolver (function/label resolution with ResolveLocalLabel for GOTO). Flow handlers: IfHandler, ForHandler, WhileHandler, CallHandler, ReturnHandler (F432). REPEAT handlers: RepeatHandler, RendHandler (F441) - countup semantics (0→N-1), State tuple (Counter, LoopEnd), COUNT:0 integration via IGameState.SetVariable. Label handlers: GotoHandler, JumpHandler, TryGotoHandler, TryJumpHandler (F442).
- `src/Era.Core/Commands/Print/` - Phase 9 Print Commands (F431): 7 print command handlers migrated from legacy GameProc. Commands: PrintCommand, PrintLCommand, PrintWCommand, PrintFormCommand, PrintDataCommand, PrintButtonCommand, PrintButtonCCommand. Handlers delegate to IConsoleOutput abstraction (implemented by runtime hosts). All handlers registered as Singletons in DI.
- `src/Era.Core/Commands/Special/` - Phase 9/13 Special Commands: SCOMF1-16 command handlers (ScomfCommands.cs defining 16 Scomf{N}Command records, Scomf{N}Handler.cs files implementing ICommandHandler) for special training scenarios. F435 (Phase 9) created stubs, F469 (Phase 13) added SCOMF variable infrastructure, F473 (Phase 13) implemented full SCOMF logic (SOURCE/STAIN/EXP/TCVAR updates).
- `src/Era.Core/Commands/System/` - Phase 9 System Commands (F434): Character commands (AddCharaCommand/Handler, DelCharaCommand/Handler, PickupCharaCommand/Handler, SwapCharaCommand/Handler, AddCopyCharaCommand/Handler), Style commands (SetColorCommand/Handler, SetBgColorCommand/Handler, SetFontCommand/Handler, ResetColorCommand/Handler, AlignmentCommand/Handler), GameState commands (SaveGameCommand/Handler, LoadGameCommand/Handler, QuitCommand/Handler, RestartCommand/Handler, ResetDataCommand/Handler). Stub service implementations (CharacterManager.cs, StyleManager.cs, GameState.cs) return Result.Fail until Phase 11 engine integration.
- `src/Era.Core/Commands/Variable/` - Phase 9 Variable & Array Commands (F433): VarSetCommand/VarSetHandler (VARSET batch variable assignment), VarSizeCommand/VarSizeHandler (VARSIZE array size), ArrayCopyCommand/ArrayCopyHandler (ARRAYCOPY), ArrayRemoveCommand/ArrayRemoveHandler (ARRAYREMOVE), ArraySortCommand/ArraySortHandler (ARRAYSORT), ArrayShiftCommand/ArrayShiftHandler (ARRAYSHIFT). Handlers inject IVariableResolver/IVariableStore but defer actual manipulation to Phase 11.
- `src/Era.Core/Commands/Com/` - Phase 12 COM System (F452, F464): ICom (base interface), IEquipmentCom (ISP-segregated equipment interface), IComRegistry (auto-discovery registry with attribute-based lookup), IComContext (execution context), ComBase (standard COM base class), EquipmentComBase (equipment-enabled COM base class), ComIdAttribute (legacy ID preservation). **F464 Directory Structure (semantic naming)**: Daily/ (17 日常), Training/{Touch,Oral,Penetration,Equipment,Bondage,Undressing,Utility}/ (90 調教), Utility/ (22 行動), Masturbation/ (自慰), Visitor/ (4 NTR), System/ (2 システム: DayEnd[888], Dummy[999]). 135 total COMs with semantic class names (e.g., ClitoralCap, Missionary) and [ComId(N)] attributes.

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
- [ ] Add unit tests in `src/engine.Tests/` for new code
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

---

## Phase 3 Migration Pattern (Era.Core.Common)

For Phase 3+ ERB→C# migrations, use game constants from `Era.Core.Common`:

```csharp
using Era.Core.Common;

// Access DIM.ERH constants
if (target == Constants.人物_美鈴) { ... }
if (location == Constants.場所_訪問者宅) { ... }
for (int i = 0; i < Constants.移動ルートMAX; i++) { ... }

// Bitfield operations (1pN converted to 1 << N)
if ((flag & Constants.訪問者宅_牢屋解錠済) != 0) { ... }
```

See `pm/features/feature-364.md` "## Migration Pattern" section for complete examples.

---

## Phase 4 DI Pattern (Era.Core Interfaces)

Phase 4 introduces DI-ready architecture. Use interfaces and strongly typed IDs for new code:

### Strongly Typed IDs
```csharp
using Era.Core.Types;

// Type-safe character and location IDs (replaces raw int)
CharacterId target = CharacterId.Meiling;
LocationId location = LocationId.Gate;
ComId comId = new ComId(100);  // COM command index - F452

// Implicit conversion to int for backward compatibility
int charId = target;  // OK
```

### Interface Injection
```csharp
using Era.Core.Interfaces;

// Constructor injection (preferred pattern)
public class MyService(ILocationService locationService, ICommonFunctions common)
{
    public void DoWork()
    {
        var name = locationService.GetPlaceName(LocationId.Gate);
        var formatted = common.FormatValue(123);
    }
}
```

### Result Type
```csharp
using Era.Core.Types;

// Explicit error handling
Result<int> result = calculator.Calculate(params);
return result.Match(
    onSuccess: value => $"Result: {value}",
    onFailure: error => $"Error: {error}"
);
```

### DI Registration
```csharp
using Era.Core.DependencyInjection;

// Register all Era.Core services (includes AddTrainingCallbacks)
services.AddEraCore();
```

**Callback Factories (F405)**: `CallbackFactories.cs` registers variable accessor delegates for components that need Result<T> unwrapping:
```csharp
// Registered automatically via AddEraCore() → AddTrainingCallbacks()
Func<CharacterId, CupIndex, int>  // CUP accessor for ExperienceGrowthCalculator
Func<CharacterId, int, bool>      // TEQUIP accessor for VirginityManager
```

See `pm/features/feature-377.md` "## Pattern Documentation" section for complete examples.

---

## Phase 5 Variable System Pattern (Era.Core Variable Types)

Phase 5 introduces strongly typed variable indices and variable system interfaces for type-safe ERA variable access:

### Strongly Typed Variable Indices
```csharp
using Era.Core.Types;

// Type-safe array indices (prevents mixing indices from different arrays)
FlagIndex flag = (FlagIndex)100;           // FLAG array index (1D)
CharacterFlagIndex cflag = (CharacterFlagIndex)50;  // CFLAG array index (2D)
AbilityIndex abl = (AbilityIndex)10;       // ABL array index (2D)
TalentIndex talent = (TalentIndex)5;       // TALENT array index (2D)
PalamIndex palam = (PalamIndex)20;         // PALAM array index (2D)
ExpIndex exp = (ExpIndex)3;                // EXP array index (2D)
BaseIndex baseIdx = BaseIndex.Mood;        // BASE array index (2D) - F393
TCVarIndex tcvar = TCVarIndex.Actor;       // TCVAR array index (2D) - F393
SourceIndex source = SourceIndex.Love;     // SOURCE array index (2D) - F399
DownbaseIndex downbase = DownbaseIndex.Stamina; // DOWNBASE array index (2D) - F402
StainIndex stain = StainIndex.Breast;      // STAIN array index (2D) - F469, F803 well-known constants
MarkIndex mark = MarkIndex.Resistance;     // MARK array index (2D) - F399
NowExIndex nowex = NowExIndex.OrgasmC;     // NOWEX array index (2D) - F399
NowExIndex doubleOrgasm = NowExIndex.DoubleOrgasm; // 二重絶頂(4) - F402
ExIndex ex = ExIndex.OrgasmC;              // EX array index (2D) - F402
MaxBaseIndex maxbase = MaxBaseIndex.Mood;  // MAXBASE array index (2D) - F399
CupIndex cup = CupIndex.PleasureC;         // CUP array index (2D) - F399
JuelIndex juel = JuelIndex.Obedience;      // JUEL array index (2D) - F460
LocalVariableIndex local = (LocalVariableIndex)0;  // LOCAL variable index

// F395 Mark System well-known values
TCVarIndex pleasureIntensity = TCVarIndex.PleasureIntensity;     // 快楽強度(106)
TCVarIndex resistSuppression = TCVarIndex.ResistanceMarkSuppression; // 反発刻印取得抑制(44)
MarkIndex resistHistory = MarkIndex.ResistanceHistory;           // 反発刻印履歴(4)
TalentIndex sadist = TalentIndex.Sadist;                         // サド(82)
TalentIndex needleMaster = TalentIndex.NeedleMaster;             // 針さばき(53)

// F403 Virginity management well-known values
TCVarIndex defloration = TCVarIndex.Defloration;                 // 破瓜(15)

// F801 Counter System well-known values
TCVarIndex counterAction = TCVarIndex.CounterAction;               // セクハラ内容(20)
TCVarIndex counterDecisionFlag = TCVarIndex.CounterDecisionFlag;   // カウンター行動決定フラグ(30)
TalentIndex virginity = TalentIndex.Virginity;                   // 処女(0)
TalentIndex affection = TalentIndex.Affection;                   // 恋慕(3) - F410
TalentIndex chastity = TalentIndex.Chastity;                     // 貞操(30) - F410

// F803 Counter Source well-known values
StainIndex mouth = StainIndex.Mouth;                                 // 口(0)
StainIndex hand = StainIndex.Hand;                                   // 手(1)
StainIndex penile = StainIndex.Penile;                               // Ｐ(2)
StainIndex vaginal = StainIndex.Vaginal;                             // Ｖ(3)
StainIndex anal = StainIndex.Anal;                                   // Ａ(4)
StainIndex breast = StainIndex.Breast;                               // Ｂ(5)
StainIndex inVagina = StainIndex.InVagina;                           // 膣内(6)
StainIndex inIntestine = StainIndex.InIntestine;                     // 腸内(7)
ExpIndex vSexExp = ExpIndex.VSexExp;                                 // Ｖ性交経験(20)
ExpIndex aSexExp = ExpIndex.ASexExp;                                 // Ａ性交経験(21)
ExpIndex masturbationExp = ExpIndex.MasturbationExp;                 // 手淫経験(24)
ExpIndex paizuriExp = ExpIndex.PaizuriExp;                           // パイズリ経験(26)
ExpIndex kissExp = ExpIndex.KissExp;                                 // キス経験(27)
ExpIndex sadisticExp = ExpIndex.SadisticExp;                         // 加虐経験(100)
TCVarIndex masterCounterControl = TCVarIndex.MasterCounterControl;   // マスターカウンター制御(26)
TCVarIndex ejaculationLocationFlag = TCVarIndex.EjaculationLocationFlag; // 射精場所フラグ(28)
TCVarIndex ejaculationPleasureIntensity = TCVarIndex.EjaculationPleasureIntensity; // 射精快楽強度(29)
TCVarIndex positionRelationship = TCVarIndex.PositionRelationship;   // 体位の関係(31)

// F802 Counter Output well-known values
TCVarIndex subAction = TCVarIndex.SubAction;                       // サブアクション(21)
TCVarIndex undressingPlayerLower = TCVarIndex.UndressingPlayerLower; // 脱衣_プレイヤー下半身(22)
TCVarIndex undressingTargetLower = TCVarIndex.UndressingTargetLower; // 脱衣_ターゲット下半身(23)
TCVarIndex undressingPlayerUpper = TCVarIndex.UndressingPlayerUpper; // 脱衣_プレイヤー上半身(24)
TCVarIndex undressingTargetUpper = TCVarIndex.UndressingTargetUpper; // 脱衣_ターゲット上半身(25)
TCVarIndex sixtyNineTransition = TCVarIndex.SixtyNineTransition;   // シックスナイン移行(27)

// F413 AbilityGrowthProcessor well-known values
TalentIndex learningSpeed = TalentIndex.LearningSpeed;           // 習得速度(50)
TalentIndex skilledFingers = TalentIndex.SkilledFingers;         // 器用な指(51)
TalentIndex tongueUser = TalentIndex.TongueUser;                 // 舌使い(52)
TalentIndex lewdPot = TalentIndex.LewdPot;                       // 淫壺(74)
TalentIndex analManiac = TalentIndex.AnalManiac;                 // 尻穴狂い(75)
TalentIndex lewdNipples = TalentIndex.LewdNipples;               // 淫乳(76)
TalentIndex bustSize = TalentIndex.BustSize;                     // バストサイズ(105)

// F406 EquipmentIndex static class (TEQUIP equipment indices)
// Note: EquipmentIndex is a static class with const int, not a struct
int clitCap = EquipmentIndex.ClitCap;         // クリキャップ(11) - F406
int vibrator = EquipmentIndex.Vibrator;       // バイブ(13) - F406
int vSex = EquipmentIndex.VSex;               // Ｖセックス(50) - F406
int aSex = EquipmentIndex.ASex;               // Ａセックス(51) - F406

// F403 ExpIndex well-known values (Orgasm experience, Virginity management)
ExpIndex orgasmExp = ExpIndex.OrgasmExperience;                  // 絶頂経験(10)
ExpIndex orgasmExpC = ExpIndex.OrgasmExperienceC;                // Ｃ絶頂経験(5)
ExpIndex orgasmExpV = ExpIndex.OrgasmExperienceV;                // Ｖ絶頂経験(6)
ExpIndex orgasmExpA = ExpIndex.OrgasmExperienceA;                // Ａ絶頂経験(7)
ExpIndex orgasmExpB = ExpIndex.OrgasmExperienceB;                // Ｂ絶頂経験(8)
ExpIndex aExp = ExpIndex.AExp;                                    // Ａ経験(2)
ExpIndex oralExp = ExpIndex.OralExp;                              // 口淫経験(25)
ExpIndex pornExp = ExpIndex.PornExperience;                       // ポルノ経験(113) - F775

// F804 WC Counter Core well-known values
CstrIndex cstr = (CstrIndex)0;                                     // CSTR array index (2D) - F804

// F812 SourceIndex well-known values (additions)
SourceIndex liquid = SourceIndex.Liquid;                           // 液体(9)
SourceIndex sexualActivity = SourceIndex.SexualActivity;           // 行為(11)
SourceIndex givePleasureC = SourceIndex.GivePleasureC;             // 与快Ｃ(40)
SourceIndex givePleasureV = SourceIndex.GivePleasureV;             // 与快Ｖ(41)
SourceIndex givePleasureA = SourceIndex.GivePleasureA;             // 与快Ａ(42)
SourceIndex givePleasureB = SourceIndex.GivePleasureB;             // 与快Ｂ(43)
SourceIndex seduction = SourceIndex.Seduction;                     // 誘惑(50)
SourceIndex humiliation = SourceIndex.Humiliation;                 // 辱め(51)
SourceIndex provocation = SourceIndex.Provocation;                 // 挑発(52)
SourceIndex service = SourceIndex.Service;                         // 奉仕(53)
SourceIndex coercion = SourceIndex.Coercion;                       // 強要(54)
SourceIndex sadism = SourceIndex.Sadism;                           // 加虐(55)

// F812 CupIndex well-known values (additions)
CupIndex goodwill = CupIndex.Goodwill;                             // 好感(5)
CupIndex superiority = CupIndex.Superiority;                       // 優越(6)
CupIndex learning = CupIndex.Learning;                             // 学習(7)
CupIndex cupLubrication = CupIndex.Lubrication;                    // 潤滑(9)
CupIndex shame = CupIndex.Shame;                                   // 羞恥(13)
CupIndex depression = CupIndex.Depression;                         // 鬱積(32)

// F812 CharacterFlagIndex well-known values (additions)
CharacterFlagIndex rotatorVInsertion = CharacterFlagIndex.RotatorVInsertion; // ローター挿入(15)
CharacterFlagIndex rotatorAInsertion = CharacterFlagIndex.RotatorAInsertion; // ローターA挿入(16)
CharacterFlagIndex mansionRank = CharacterFlagIndex.MansionRank;             // 館ランク(310)

// F812 ExpIndex well-known values (additions)
ExpIndex cExp = ExpIndex.CExp;                                     // Ｃ経験(0)
ExpIndex vExp = ExpIndex.VExp;                                     // Ｖ経験(1)
ExpIndex bExp = ExpIndex.BExp;                                     // Ｂ経験(3)

// F460 CharacterFlagIndex well-known values
CharacterFlagIndex favor = CharacterFlagIndex.Favor;              // 好感度(2)

// F460 JuelIndex well-known values
JuelIndex pleasureC = JuelIndex.PleasureC;                        // 快Ｃ(0)
JuelIndex pleasureV = JuelIndex.PleasureV;                        // 快Ｖ(1)
JuelIndex pleasureA = JuelIndex.PleasureA;                        // 快Ａ(2)
JuelIndex pleasureB = JuelIndex.PleasureB;                        // 快Ｂ(3)
JuelIndex lubrication = JuelIndex.Lubrication;                    // 潤滑(9)
JuelIndex juelObedience = JuelIndex.Obedience;                    // 恭順(10)
JuelIndex juelSubmission = JuelIndex.Submission;                  // 屈服(12)
JuelIndex juelPain = JuelIndex.Pain;                              // 苦痛(15)
JuelIndex juelFear = JuelIndex.Fear;                              // 恐怖(16)
JuelIndex juelAntipathy = JuelIndex.Antipathy;                    // 反感(30)
JuelIndex denial = JuelIndex.Denial;                              // 否定(100)

// Implicit conversion to int for backward compatibility
int flagValue = flag;  // OK
```

### Variable Store Interfaces (F404 ISP Pattern)

F404 segregated the original IVariableStore into ISP-compliant interfaces. F412 added ITEquipVariables, F789 added IStringVariables and I3DArrayVariables, F781 added IVisitorVariables (visitor appearance SAVEDATA access) and IVisitorSettings (visitor settings business logic), F779 added IBodySettings (body settings business logic):

| Interface | Methods | Purpose |
|-----------|---------|---------|
| IVariableStore | 43 | Core variables (FLAG, TFLAG, CFLAG, ABL, TALENT, PALAM, EXP, BASE, TCVAR, SOURCE, MARK, NOWEX, MAXBASE, CUP, JUEL, GOTJUEL, PALAMLV, STAIN, DOWNBASE, EQUIP, CSTR, EXPLV, NOITEM) |
| ITrainingVariables | 6 | Training-specific (BASE, TCVAR, CUP) |
| ICharacterStateVariables | 14 | State tracking (SOURCE, MARK, NOWEX, MAXBASE, CDOWN, EX) - F412 added GetCDown/SetCDown, GetEx/SetEx |
| IJuelVariables | 6 | Juel system (JUEL, GOTJUEL, PALAMLV) |
| ITEquipVariables | 4 | Equipment flags (TEQUIP) - F412 |
| IStringVariables | 2 | String variables (SAVESTR) - F789. GetSaveStr(SaveStrIndex) returns string, SetSaveStr fire-and-forget |
| I3DArrayVariables | 4 | 3D integer arrays (TA, TB) - F789. GetTa/GetTb return Result<int>, SetTa/SetTb fire-and-forget |
| IVisitorVariables | 46 | Visitor appearance variables (23 get/set pairs) - F781. Global SAVEDATA access for visitor character customization |
| IVisitorSettings | - | Visitor settings business logic (dedup, compaction, validation) - F781. Implemented by VisitorSettings |
| IBodySettings | 8 | Body settings business logic (dedup, compaction, validation, tightness mapping, P size offset, daily change) - F779. Implemented by BodySettings |
| IGeneticsService | 3 | Genetics-related body settings operations (extracted from IBodySettings in F800) - F800. Implemented by GeneticsService |
| ICharacterStringVariables | 2 | Character-scoped string variables (CSTR) - F802. GetCharacterString/SetCharacterString using CstrIndex |
| ITextFormatting | 3 | Text formatting stubs for clothing/body descriptions - F802. GetPantsDescription, GetPantsName, GetOppaiDescription |
| IVariableStoreExtensions | - | Extension methods: GetCFlag, SetCFlag, GetTalent (F782); GetTalentValue, GetBaseValue, GetMaxBaseValue, GetAbilityValue, GetExpValue, GetMarkValue, GetJuelValue (F800). `src/Era.Core/Interfaces/IVariableStoreExtensions.cs` |

**IBodySettings** (`src/Era.Core/Interfaces/IBodySettings.cs`) — F779:
- `void Tidy(int characterId)`: Dedup + mutual exclusion validation + slot compaction for body options (4 slots), hair/eye/V/P option pairs (2 slots each). Does NOT sync derived values (behavioral difference from IVisitorSettings).
- `int ValidateBodyOption(int characterId, int candidateValue, int slot)`: 5-range-group mutual exclusion (1-9, 10-29, 30-49, 50-54, 55-59). Returns 1 (accept) or 0 (reject).
- `int ValidatePenisOption(int characterId, int candidateValue, int slot)`: P-specific range 1-2 mutual exclusion + duplicate check. Returns 1 (accept) or 0 (reject).
- `int ValidateSimpleDuplicate(int candidateValue, int otherSlotValue)`: Pairwise equality check for hair/eye/V option pairs. Returns 1 (different=accept) or 0 (same=reject).
- `int CalculatePenisSize(int rawInput)`: Returns rawInput - 2 (ERB line 712 offset).
- `int GetTightnessBaseValue(int selectionIndex)`: Non-linear V/A tightness BASE mapping (0→0, 1→100, 2→250, 3→450, 4→700).
- `void BodyDetailInit(int characterId, ...)`: Character body parameter initialization (existing, F377).
- `void BodyChangeDaily(int characterId)`: Daily hair growth processing (body_settings.ERB:932-939) - F780.

**IGeneticsService** (`src/Era.Core/Interfaces/IGeneticsService.cs`) — F800:
- `void BodySettingsGenetics(int childId, int fatherId, int motherId, int multiBirthFlag, int siblingId)`: Genetics inheritance for child characters (trait assignment, skin/hair color inheritance, twin field copying).
- `void BodySettingsChildPGrowth(int childId)`: Child P growth calculation based on parent genetics.
- `void BodySettingsChildHairChange(int childId)`: Child hair change logic based on genetic inheritance.
**Implementation**: `GeneticsService` (`src/Era.Core/State/GeneticsService.cs`). Constructor accepts `IVariableStore`, `IRandomProvider`, `IVisitorVariables`, `IEngineVariables`, `IBodySettings`, `Func<int, int> multiBirthFatherCheck`. Calls `IBodySettings.Tidy()` internally.

**ITextFormatting** (`src/Era.Core/Interfaces/ITextFormatting.cs`) — F802:
- `string GetPantsDescription(int equipIndex)`: Clothing description prefix (PANTS_DESCRIPTION ERB equivalent)
- `string GetPantsName(int equipIndex)`: Clothing item name (PANTSNAME ERB equivalent)
- `string GetOppaiDescription(int characterId)`: Bust/chest description (OPPAI_DESCRIPTION ERB equivalent)
- Stub returns empty string for headless/test. Used by CounterMessage for text output.

**ICharacterStringVariables** (`src/Era.Core/Interfaces/ICharacterStringVariables.cs`) — F802:
- `string GetCharacterString(CharacterId character, CstrIndex index)`: Character-scoped CSTR read
- `void SetCharacterString(CharacterId character, CstrIndex index, string value)`: Character-scoped CSTR write (fire-and-forget)

**CharacterFlagIndex** new constants (F779) — `src/Era.Core/Types/CharacterFlagIndex.cs`:
- Hair: `HairLength` (500), `HairLengthCategory` (501), `HairOption1` (502), `HairOption2` (503), `HairBaseColor` (504), `HairColor` (505)
- Eye: `EyeColorRight` (506), `EyeColorLeft` (507), `EyeExpression` (508), `EyeOption1` (509), `EyeOption2` (510)
- Skin: `SkinBaseColor` (511), `SkinColor` (512)
- Body: `BodyOption1` (515), `BodyOption2` (516), `BodyOption3` (517), `BodyOption4` (518)
- V/P: `VPosition` (402), `VOption1` (403), `VOption2` (404), `PSize` (406), `POption1` (407), `POption2` (408)

**BaseIndex** new constants (F779) — `src/Era.Core/Types/BaseIndex.cs`:
- `ALooseness` (26), `VLooseness` (27)

```csharp
using Era.Core.Interfaces;
using Era.Core.Types;

// F404 ISP: Depend only on interfaces you need
public class BasicChecksProcessor(
    IVariableStore coreVariables,
    ITrainingVariables trainingVariables)
{
    public void CheckMood(CharacterId character)
    {
        // Core variables from IVariableStore
        Result<int> ablResult = coreVariables.GetAbility(character, (AbilityIndex)10);

        // F804: WC Counter methods (CSTR, EXPLV, NOITEM)
        Result<string> cstr = coreVariables.GetCharacterString(character, (CstrIndex)0);
        Result<int> expLv = coreVariables.GetExpLv(3);  // 1D global lookup (not character-scoped)
        int noItem = coreVariables.GetNoItem();          // scalar accessor

        // F809: COMABLE Core methods (SetExpLv paired setter for GetExpLv)
        coreVariables.SetExpLv(3, 100);                 // set EXPLV level threshold

        // Training variables from ITrainingVariables
        Result<int> moodResult = trainingVariables.GetBase(character, BaseIndex.Mood);
        Result<int> actorResult = trainingVariables.GetTCVar(character, TCVarIndex.Actor);
    }
}

public class EquipmentProcessor(
    ITEquipVariables equipmentVariables,
    ICharacterStateVariables stateVariables)
{
    public void ProcessEquipment(CharacterId character)
    {
        // Equipment flags from ITEquipVariables (F412)
        int cliCapIndex = 11;  // クリキャップ
        Result<int> equipped = equipmentVariables.GetTEquip(character, cliCapIndex);
        equipmentVariables.SetTEquip(character, cliCapIndex, 1);

        // CDOWN (pleasure reduction) from ICharacterStateVariables (F412)
        Result<int> cDown = stateVariables.GetCDown(character, (PalamIndex)0);
        stateVariables.SetCDown(character, (PalamIndex)0, 100);

        // EX (orgasm counters) from ICharacterStateVariables (F412)
        Result<int> exC = stateVariables.GetEx(character, ExIndex.OrgasmC);
        stateVariables.SetEx(character, ExIndex.OrgasmC, 5);
    }
}

public class JuelProcessor(
    IVariableStore coreVariables,
    IJuelVariables juelVariables)
{
    public void ProcessJuel(CharacterId character)
    {
        // Core variables (PALAM, EXP)
        Result<int> palam = coreVariables.GetPalam(character, (PalamIndex)0);

        // Juel variables from IJuelVariables
        Result<int> obedienceJuel = juelVariables.GetJuel(character, 10);
        juelVariables.SetJuel(character, 10, 1000);
        Result<int> threshold = juelVariables.GetPalamLv(1);  // 1D global
    }
}

// VariableStore implements all 8 interfaces (single implementation, multiple views)
// DI registration: services.AddSingleton<VariableStore>() provides all 8 interfaces
```

### Variable Scope Interface
```csharp
using Era.Core.Interfaces;
using Era.Core.Types;

// IVariableScope - Scope stack management for LOCAL/ARG variables
public class FunctionExecutor(IVariableScope scope)
{
    public void CallFunction()
    {
        scope.PushLocal();  // Enter new local scope
        scope.SetLocal((LocalVariableIndex)0, 42);
        // ... function body ...
        scope.PopLocal();   // Exit scope, cleanup locals
    }
}
```

### Variable Resolver Interface
```csharp
using Era.Core.Interfaces;
using Era.Core.Types;

// IVariableResolver - Resolve variable identifiers to references
public class MyService(IVariableResolver resolver)
{
    public void ResolveVariable()
    {
        // Resolve simple pattern (FLAG:123)
        Result<VariableReference> result = resolver.Resolve("FLAG:123");

        // Resolve character pattern with CSV lookup (CFLAG:0:好感度)
        result = resolver.Resolve("CFLAG:0:好感度");

        // TryResolve pattern
        if (resolver.TryResolve("TFLAG:実行値", out var reference))
        {
            // Use reference (Scope, Index, CharacterId?, Code?)
        }
    }
}
```

### Variable Definition Loader Interface
```csharp
using Era.Core.Interfaces;
using Era.Core.Types;

// IVariableDefinitionLoader - Load CSV/YAML variable definitions
public interface IVariableDefinitionLoader
{
    // Load CSV file and get name→index mappings
    Result<CsvVariableDefinitions> LoadFromCsv(string csvPath);

    // Load YAML file and get name→index mappings (F617)
    // Converts YAML variable definition format to CsvVariableDefinitions for compatibility
    Result<CsvVariableDefinitions> LoadFromYaml(string yamlPath);
}

public class DefinitionService(IVariableDefinitionLoader loader)
{
    public void LoadFromCsvFile()
    {
        // Load CSV file and get name→index mappings (legacy format)
        Result<CsvVariableDefinitions> result = loader.LoadFromCsv("TestData/CFLAG.csv");
        result.Match(
            onSuccess: defs => {
                // defs.NameToIndex["好感度"] → 2
                // defs.IndexToName[2] → "好感度"
            },
            onFailure: error => Console.WriteLine($"Load failed: {error}")
        );
    }

    public void LoadFromYamlFile()
    {
        // Load YAML file (F617) - preferred for new code
        Result<CsvVariableDefinitions> result = loader.LoadFromYaml("Game/data/Talent.yaml");
        result.Match(
            onSuccess: defs => {
                // defs.NameToIndex["処女"] → 0
                // defs.IndexToName[0] → "処女"
            },
            onFailure: error => Console.WriteLine($"Load failed: {error}")
        );
    }
}

// YAML Format (Game/data/Talent.yaml) - F617
// ============================================
// definitions:
//   - index: 0
//     name: "処女"           # Talent name matching ITalentLoader format
//   - index: 1
//     name: "童貞"
//   # ... continue with all talent definitions matching CSV equivalent
//
// Notes:
// - LoadFromYaml converts YAML definitions to CsvVariableDefinitions for backward compatibility
// - Intended as transitional bridge during F591 (Legacy CSV File Removal)
// - ITalentLoader remains canonical for new talent loading code
// - After F591 removes CSV files, CSV fallback logic in TalentIndexTests can be removed
```

### Supporting Types
```csharp
// VariableScopeType - Scope classification
VariableScopeType scope = VariableScopeType.Global;  // Global, Local, Character

// VariableReference - Resolved variable reference DTO (Scope, Index, CharacterId?, Code?)
var reference = new VariableReference(VariableScopeType.Character, 10, characterId: 1, code: VariableCode.CFLAG);

// CsvVariableDefinitions - Loaded CSV definitions
var definitions = new CsvVariableDefinitions(nameToIndex, indexToName);
```

See `pm/features/feature-384.md` for complete type definitions and interface contracts.

---

## Phase 6 Training Pattern (Era.Core.Training)

Phase 6 introduces training lifecycle and processing migration from ERB to C#. Use training interfaces for lifecycle management:

### Training Setup Interface
```csharp
using Era.Core.Interfaces;
using Era.Core.Types;

// ITrainingSetup - Pre/post training lifecycle management
public class GameController(ITrainingSetup trainingSetup)
{
    public void StartTrainingDay(CharacterId target)
    {
        // Called at EVENTTRAIN lifecycle point (day start)
        // Resets BASE/TCVAR, applies stamina recovery (MASTER only), clears sleep flags
        trainingSetup.BeforeTraining(target);

        // ... training command selection and execution ...

        // Called after training completes (stub for future JUEL_CHECK/DENIAL_CHECK)
        trainingSetup.AfterTraining(target);
    }
}
```

### Training Processor Interface
```csharp
using Era.Core.Interfaces;
using Era.Core.Types;
using Era.Core.Training;

// ITrainingProcessor - Training execution and result calculation
public class TrainingSystem(ITrainingProcessor processor)
{
    public void ExecuteTraining(CharacterId target, CommandId command)
    {
        // Execute training command and calculate result (returns Result<TrainingResult>)
        Result<TrainingResult> result = processor.Process(target, command);

        // Match on Result for explicit error handling
        result.Match(
            onSuccess: trainingResult =>
            {
                // TrainingResult contains: CharacterId, List<StateChange>, Success, ErrorMessage
                foreach (var change in trainingResult.Changes)
                {
                    // change.Variable: "ABL:従順", "BASE:ムード", etc.
                    // change.Delta: amount of change (positive or negative)
                    // change.Reason: optional explanation
                    Console.WriteLine($"{change.Variable} changed by {change.Delta} ({change.Reason})");
                }
            },
            onFailure: error => Console.WriteLine($"Training failed: {error}")
        );
    }
}
```

### Training Result Type
```csharp
using Era.Core.Types;
using Era.Core.Training;

// TrainingResult - Training execution outcome (mutable class)
var result = new TrainingResult(CharacterId.Meiling)
    .AddChange(new AbilityChange(AbilityIndex.Obedient, 10))
    .AddChange(new TalentChange(TalentIndex.Virgin, -1))
    .AddChange(new ExpChange(ExpIndex.VExp, 100));

// StateChange hierarchy (F401) - Type-safe variable changes
// Abstract base: public abstract record StateChange;
// Concrete subtypes:
//   AbilityChange(AbilityIndex Index, int Delta)    - Additive
//   TalentChange(TalentIndex Index, int Delta)      - Additive
//   CFlagChange(CharacterFlagIndex Index, int Value) - Assignment
//   TCVarChange(TCVarIndex Index, int Value)        - Assignment
//   TFlagChange(FlagIndex Index, int Value)         - Assignment
//   ExpChange(ExpIndex Index, int Value)            - Additive
//   MarkHistoryChange(MarkIndex Index, int Value)   - Assignment
//   SourceChange(SourceIndex Index, int Delta)      - Additive (F402)
//   DownbaseChange(DownbaseIndex Index, int Delta)  - Additive (F402)
//   NowExChange(NowExIndex Index, int Value)        - Assignment (F402)
//   ExChange(ExIndex Index, int Delta)              - Additive (F402)
//   TimeChange(int Delta)                           - Additive (F402)

// Access via properties
CharacterId target = result.CharacterId;
List<StateChange> changes = result.Changes;
bool success = result.Success;
string? error = result.ErrorMessage;
```

### Key Files
```
src/Era.Core/Training/
├── ITrainingSetup.cs           - Pre/post training lifecycle
├── ITrainingProcessor.cs       - Training processing core
├── TrainingSetup.cs            - Lifecycle implementation
├── TrainingProcessor.cs        - Processing orchestrator
├── TrainingResult.cs           - Result types (TrainingResult)
├── StateChange.cs              - Abstract StateChange hierarchy (F401)
├── IAbilityGrowthProcessor.cs  - Sub-processor: ability growth
├── IEquipmentProcessor.cs      - Sub-processor: equipment effects
├── IOrgasmProcessor.cs         - Sub-processor: orgasm processing
├── IFavorCalculator.cs         - Sub-processor: favor calculation
├── IBasicChecksProcessor.cs    - Sub-processor: basic checks
├── IMarkSystem.cs              - Sub-processor: MARK system
├── IJuelProcessor.cs           - Sub-processor: JUEL reward (F400)
├── JuelProcessor.cs            - JUEL_CHECK/DENIAL_CHECK implementation (F400)
├── ISpecialTraining.cs         - Special training interface (F435)
└── SpecialTraining.cs          - Special training implementation (F435 stub, F473 full, F484 prerequisites)
```

See `pm/features/feature-393.md` and `pm/features/feature-394.md` for complete training system architecture.

### Character State Tracking (Era.Core.Character) - F396

Character state tracking is separate from Training and handles virginity management, experience growth, and pain state checking.

```
src/Era.Core/Character/
├── ICharacterStateTracker.cs       - Orchestrator interface
├── CharacterStateTracker.cs        - Orchestrator implementation
├── IVirginityManager.cs            - LOST_VIRGIN* interface
├── VirginityManager.cs             - Virginity management (lines 79-214)
├── VirginityState.cs               - Types: VirginityType, VirginityLossMethod, VirginityChange
├── IExperienceGrowthCalculator.cs  - EXP_GOT_CHECK interface
├── ExperienceGrowthCalculator.cs   - Experience growth (lines 854-946)
├── ExperienceState.cs              - Type: ExperienceChange
├── IPainStateChecker.cs            - PAIN_CHECK* interface
└── PainStateChecker.cs             - Pain modifiers (lines 1152-1217), Types: PainModifier, PainType
```

See `pm/features/feature-396.md` for complete character state tracking architecture.

---

## Phase 13 DDD Pattern (Era.Core.Domain, Era.Core.Infrastructure) - F465, F466, F467, F468

Phase 13 introduces Domain-Driven Design patterns for encapsulating business rules and enforcing invariants.

### Key Directories

- `src/Era.Core/Domain/` - DDD domain model components
  - `AggregateRoot.cs` - Generic base class for all aggregates
  - `IRepository.cs` - Generic repository interface (F466)
  - `IUnitOfWork.cs` - Unit of Work interface for transaction boundaries (F467)
  - `Aggregates/` - Aggregate root implementations (Character)
  - `ValueObjects/` - Immutable value types (CharacterName, CharacterStats)
  - `Events/` - Domain events (IDomainEvent, CharacterCreatedEvent)
- `src/Era.Core/Process/` - Process execution state management (F480)
  - `IProcessState.cs` - Execution state machine interface (call stack, scope management)
  - `ProcessState.cs` - IProcessState implementation with IVariableScope coordination
  - `CallStack.cs` - CALL/RETURN address tracking
  - `ExecutionContext.cs` - Program counter and execution state tracking
- `src/Era.Core/Input/` - Input command processing (F481)
  - `IInputHandler.cs` - Input handler interface for INPUT/INPUTS commands
  - `InputHandler.cs` - Request state management, duplicate request rejection
  - `InputRequest.cs` - Value object with InputType enum (Numeric/String)
  - `InputValidator.cs` - Numeric range validation with Japanese error messages
- `src/Era.Core/Infrastructure/` - Infrastructure implementations (F466, F467, F468)
  - `InMemoryRepository.cs` - Generic in-memory repository with ConcurrentDictionary
  - `CharacterRepository.cs` - Typed repository for Character aggregate
  - `UnitOfWork.cs` - In-memory UnitOfWork implementation (F467)
  - `VariableStoreAdapter.cs` - Adapter bridging Character aggregate with IVariableStore (F468)
- `src/Era.Core/Monitoring/` - Performance and analytics monitoring (F607)
  - `IPerformanceMetrics.cs` - Performance metrics collection interface
  - `PerformanceMetricsService.cs` - Performance metrics implementation (execution timing, memory usage, resource loading)
  - `IPerformanceAnalyticsService.cs` - Performance analytics integration interface
  - `PerformanceAnalyticsService.cs` - Analytics integration implementation
  - `PerformanceConfig.cs` - Performance monitoring configuration

### AggregateRoot<TId> Base Class
```csharp
using Era.Core.Domain;
using Era.Core.Domain.Events;

// Generic aggregate root with strongly-typed ID and domain event support
public abstract class AggregateRoot<TId> where TId : struct
{
    public TId Id { get; protected init; }  // Immutable after creation
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    protected void AddDomainEvent(IDomainEvent @event);
    public void ClearDomainEvents();
}

// Example usage in concrete aggregate
public class Character : AggregateRoot<CharacterId>
{
    public CharacterName Name { get; private set; }
    public CharacterStats Stats { get; private set; }
    public AbilitySet Abilities { get; private set; }    // F472
    public TalentSet Talents { get; private set; }       // F472

    public static Character Create(CharacterId id, CharacterName name, CharacterStats stats);
    public void ApplyTraining(string trainingType, AbilitySet newAbilities, TalentSet newTalents);  // F472
}
```

### Domain Event Pattern
```csharp
using Era.Core.Domain.Events;

// Interface for all domain events
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

// Example event
public record CharacterCreatedEvent(CharacterId CharacterId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

// F472: Training applied event
public record TrainingAppliedEvent(CharacterId CharacterId, string TrainingType) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

### Value Object Patterns
```csharp
using Era.Core.Domain.ValueObjects;
using Era.Core.Types;

// Value object with validation (Result-based factory)
public readonly record struct CharacterName
{
    public string Value { get; }
    public static Result<CharacterName> Create(string name);  // Returns Fail for empty/whitespace
}

// Value object with immutable operations
public readonly record struct CharacterStats(int Health, int Stamina, int Frustration, int Loyalty)
{
    public CharacterStats ConsumeStamina(int amount) =>
        this with { Stamina = Math.Max(0, Stamina - amount) };  // Returns new instance
}

// F472: AbilitySet - Immutable ability collection
public readonly record struct AbilitySet
{
    public int GetAbility(AbilityIndex index);  // Returns 0 for missing
    public AbilitySet WithAbility(AbilityIndex index, int value);  // Returns new instance
}

// F472: TalentSet - Immutable talent collection
public readonly record struct TalentSet
{
    public int GetTalent(TalentIndex index);  // Returns 0 for missing
    public bool HasTalent(TalentIndex index);  // True if value > 0
    public TalentSet WithTalent(TalentIndex index, int value);  // Returns new instance
}

// Usage
var nameResult = CharacterName.Create("美鈴");
nameResult.Match(
    onSuccess: name => Character.Create(id, name, stats),
    onFailure: error => throw new InvalidOperationException(error)
);
```

### Repository Pattern (F466)
```csharp
using Era.Core.Domain;
using Era.Core.Infrastructure;
using Era.Core.Types;

// IRepository<T, TId> - Generic repository interface
public interface IRepository<T, TId>
    where T : AggregateRoot<TId>
    where TId : struct
{
    Result<T> GetById(TId id);        // Returns Fail if not found
    IReadOnlyList<T> GetAll();        // Returns all aggregates
    void Add(T aggregate);            // Throws InvalidOperationException on duplicate
    void Update(T aggregate);         // Overwrites existing
    void Remove(TId id);              // No-op if not found
}

// InMemoryRepository<T, TId> - Thread-safe in-memory implementation
var repository = new InMemoryRepository<Character, CharacterId>();
repository.Add(character);            // Thread-safe via ConcurrentDictionary
var result = repository.GetById(id);  // Returns Result<Character>
result.Match(
    onSuccess: c => Console.WriteLine(c.Name),
    onFailure: e => Console.WriteLine(e)
);

// CharacterRepository - Typed convenience class
var charRepo = new CharacterRepository();
charRepo.Add(character);
```

### Unit of Work Pattern (F467)
```csharp
using Era.Core.Domain;
using Era.Core.Infrastructure;
using Era.Core.Types;

// IUnitOfWork - Coordinates repository changes within transaction boundaries
public interface IUnitOfWork : IDisposable
{
    IRepository<Aggregates.Character, CharacterId> Characters { get; }
    Task<int> CommitAsync(CancellationToken ct = default);
    void Rollback();
}

// UnitOfWork - In-memory implementation
var unitOfWork = new UnitOfWork(characterRepository);
unitOfWork.Characters.Add(character);
await unitOfWork.CommitAsync();  // Returns 1 (in-memory stub)
unitOfWork.Rollback();           // No-op for in-memory, reloads state in DB implementation

// TransactionBehavior integration - Pipeline behavior wraps command execution
// Commits on success, rolls back on exception
public class TransactionBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<TResult>> Handle(TCommand request,
        Func<Task<Result<TResult>>> next, CancellationToken ct)
    {
        var result = await next();
        if (result is Result<TResult>.Success)
            await _unitOfWork.CommitAsync(ct);
        return result;
    }
}

// DI Registration (Scoped - Singleton cannot inject Scoped)
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<IRepository<Character, CharacterId>, CharacterRepository>();  // F468
services.AddScoped<VariableStoreAdapter>();  // F468
services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
```

### DDD Patterns Summary

| Pattern | Implementation | Purpose |
|---------|----------------|---------|
| Aggregate Root | `AggregateRoot<TId>` | Encapsulate business rules, enforce invariants |
| Repository | `IRepository<T, TId>` | Collection-like aggregate access (F466) |
| Unit of Work | `IUnitOfWork` | Transaction coordination across repositories (F467) |
| Adapter | `VariableStoreAdapter` | Bridge DDD aggregate with legacy IVariableStore (F468) |
| Domain Events | `IDomainEvent` | Track state changes for notification |
| Value Objects | `readonly record struct` | Immutable value types with equality |
| Factory Methods | `Create()` returning `Result<T>` | Validation on construction |
| Strongly Typed IDs | `CharacterId` (existing) | Type-safe identity |

See `pm/features/feature-465.md` for DDD foundation, `pm/features/feature-466.md` for Repository pattern, `pm/features/feature-467.md` for UnitOfWork pattern, and `pm/features/feature-468.md` for Legacy Bridge + DI Integration.

---

## Phase 14 Save State Types (src/Era.Core/Types/GameSaveState.cs) - F487

Phase 14 introduces serializable state record types for JSON save/load operations. These are **DTOs** distinct from runtime state classes.

### Naming Convention

| Type | Purpose | Note |
|------|---------|------|
| `*SaveState` | Serializable DTO for persistence | `GameSaveState`, `CharacterSaveState`, etc. |
| `*State` (existing) | Runtime behavior class | `GameState` (IGameState impl), `CharacterState` |

### Record Types

```csharp
using System.Text.Json;
using Era.Core.Types;

// Root save state - aggregates all game data for persistence
public record GameSaveState
{
    public Dictionary<string, CharacterSaveState> Characters { get; init; } = new();
    public PlayerSaveState Player { get; init; } = new();
    public GameContextSaveState Context { get; init; } = new();
    public bool GameOver { get; init; }

    public static GameSaveState CreateNew() => new();
}

// Character persistence (keyed by Id in parent Dictionary)
public record CharacterSaveState
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public Dictionary<string, int> Flags { get; init; } = new();       // CFLAG equivalent
    public Dictionary<string, string> Attributes { get; init; } = new(); // Flexible attributes
}

// Player persistence
public record PlayerSaveState
{
    public string Name { get; init; } = string.Empty;
    public Dictionary<string, int> Flags { get; init; } = new();       // Player flags
    public int Money { get; init; }
}

// Game context persistence
public record GameContextSaveState
{
    public int Day { get; init; }
    public int Time { get; init; }
    public string Location { get; init; } = string.Empty;
    public Dictionary<string, JsonElement> Variables { get; init; } = new();  // Extensible
}
```

### Usage with StateManager (F475)

```csharp
using Era.Core.Types;

// StateManager loads/saves GameSaveState via System.Text.Json
public interface IStateManager
{
    Result<GameSaveState> Load(string path);
    Result<Unit> Save(string path, GameSaveState state);
}

// Example usage
var stateManager = services.GetRequiredService<IStateManager>();
var result = stateManager.Load("saves/slot1.json");
result.Match(
    onSuccess: state => {
        var meiling = state.Characters["meiling"];
        Console.WriteLine($"Loaded {meiling.Name}");
    },
    onFailure: error => Console.WriteLine($"Load failed: {error}")
);
```

### Design Decisions

| Decision | Rationale |
|----------|-----------|
| **Record types** | Immutable, value-based equality, `with` expressions |
| **`*SaveState` naming** | Avoids collision with runtime state classes |
| **Default initializers** | Prevents null during deserialization |
| **`JsonElement` for Variables** | Preserves arbitrary JSON structure for extensibility |
| **Single file** | All related records together (~50 lines) |

See `pm/features/feature-487.md` for implementation details.

## Phase 20 Engine Data Access (src/Era.Core/Interfaces) - F790

Phase 20 introduces two ISP-segregated interfaces for engine built-in variable access and CSV constant name resolution.

### IEngineVariables (F790, extended F775, F776, F777, F801)

```csharp
// src/Era.Core/Interfaces/IEngineVariables.cs - Engine built-in variable access (mixed read/write since F775)
// DI: AddSingleton<IEngineVariables, NullEngineVariables>() (default)

// Scalar methods (deferred to engine adapter until wired)
int GetResult();    // RESULT (function return / user input)
int GetMoney();     // MONEY:0
void SetMoney(int value); // MONEY:0 setter - F775
int GetDay();       // DAY:0
int GetMaster();    // MASTER:0 (master character index)
int GetAssi();      // ASSI:0 (assistant character index)
int GetCount();     // COUNT:0 (general counter)

// Delegation methods
int GetCharaNum();          // → ICharacterDataAccess.GetCharacterCount()
int GetRandom(int max);     // → (int)IRandomProvider.Next(max)

// Character-scoped methods (int index → CharacterId conversion internally)
string GetName(int characterIndex);     // NAME (scaffolding: returns "")
string GetCallName(int characterIndex); // → ICharacterDataService.GetCallName()
int GetIsAssi(int characterIndex);      // ISASSI (scaffolding: returns 0)
int GetCharacterNo(int characterIndex); // NO (character CSV number) - F776

// F777: Character customization setters
void SetName(int characterIndex, string name);     // NAME setter
void SetCallName(int characterIndex, string callName); // CALLNAME setter
void SetMaster(int value);         // MASTER setter
int GetTarget();                   // TARGET getter
void SetTarget(int value);         // TARGET setter
int GetPlayer();                   // PLAYER getter
void SetPlayer(int value);         // PLAYER setter
void SetCharacterNo(int characterIndex, int value); // NO setter

// F801: Counter system indexed TARGET and SELECTCOM access
int GetTarget(int index);              // TARGET:index (indexed array access)
void SetTarget(int index, int value);  // TARGET:index setter
int GetSelectCom();                    // SELECTCOM (selected command number)

// F812: Assistant play mode flag
int GetAssiPlay();                     // ASSIPLAY:0 (default interface method returning 0)

// F811: Previous command tracking
int GetPrevCom();                      // PREVCOM (previous command number)
void SetPrevCom(int value);            // PREVCOM setter
```

### IUpVariables (F811)

```csharp
// src/Era.Core/Interfaces/IUpVariables.cs - UP array (training progress) access
// UP: 1D global array indexed by stat
int GetUp(int index);
void SetUp(int index, int value);
```

### Counter System Interfaces (F801)

Phase 21 Counter System core. `src/Era.Core/Counter/` namespace:

| Interface | Methods | Purpose |
|-----------|---------|---------|
| ICounterSystem | 1 | Architecture-mandated owner interface: `SelectAction(CharacterId, int) → CounterActionId?` |
| IActionSelector | 1 | Strategy Pattern DI seam: `SelectAction(CharacterId, int) → CounterActionId?` |
| IActionValidator | 1 | Pure validation: `IsActable(CharacterId, CounterActionId) → bool` |
| ICounterUtilities | 11 | Cross-phase utilities: IsAirMaster, IsProtectedCheckOnly, CheckStain, IsVirginM, IsOnce + F804: TimeProgress, RestGetUrge, GetDateTime, GetTargetNum, MasterPose + F803: CheckExpUp |
| ICounterOutputHandler | 2 | F802 stub: HandleReaction, HandlePunishment |
| IWcCounterOutputHandler | 4 | F804 interface, F805 impl (WcCounterMessage): SendMessage(CharacterId, int?), HandleReaction, HandlePunishment, WcLoveRanking(int) |
| IWcCounterSystem | 1 | F811 WC dispatch: SelectAction(CharacterId, int) → CounterActionId? |
| ISourceCalculator | 35 | F812: SOURCE1.ERB calculation functions — pleasure modifiers, source-to-CUP converters, parameter adjusters |
| IComAvailabilityChecker | 1 | F809 COMABLE Core: `IsAvailable(int comId) → bool` — checks COM command availability (migrates 124 @COM_ABLE{N} functions from COMABLE.ERB). `src/Era.Core/Counter/Comable/` namespace |
| IComableUtilities | 1 | F809 COMABLE Core: `MasterPose(int pose, int arg1, int arg2) → int` — COMABLE-specific utilities (MASTER_POSE stub). `src/Era.Core/Counter/Comable/` namespace |
| ITouchStateManager | 6 | F811: Touch state management (TouchSet, MasterPose, TouchResetM, TouchResetT, TouchSuccession, ShowTouch). `src/Era.Core/Counter/Source/` namespace. Cross-feature interface for F802/F803/F805/F809 |
| ISourceSystem | 2 | F811: SOURCE entry point (SourceCheck, ChastityBeltCheck). `src/Era.Core/Counter/Source/` namespace |
| IComHandler | 1 | F811: COM dispatch handler (Handle). `src/Era.Core/Counter/` namespace |
| IKnickersSystem | 1 | F811: Phase 22 stub (ChangeKnickers). `src/Era.Core/Counter/Source/` namespace |
| ICounterSourceHandler | 1 | F803/F811: Counter source handler (HandleCounterSource). `src/Era.Core/Counter/` namespace |
| IWcCounterSourceHandler | 1 | F811 stub → F805 impl (WcCounterSourceHandler): WC counter source handler (HandleWcCounterSource). `src/Era.Core/Counter/` namespace |
| ICombinationCounter | 1 | F811: Combination counter stub (AccumulateCombinations). `src/Era.Core/Counter/` namespace |
| IWcCombinationCounter | 1 | F811: WC combination counter stub (AccumulateCombinations). `src/Era.Core/Counter/` namespace |
| ITrainingCheckService | 14 | F811: External facade for TRACHECK.ERB utility functions. `src/Era.Core/Counter/` namespace |
| IKojoMessageService | 10 | F811/F805: External facade for EVENT_KOJO.ERB message functions (KojoMessageWcCounter(int,int) overload added F805). `src/Era.Core/Counter/` namespace |
| INtrRevelationHandler | 1 | F805: NTR revelation dispatch for WcCounterMessage MESSAGE13 — Execute(CharacterId, int). Null-registered; F808 provides concrete impl. `src/Era.Core/Counter/` namespace |
| INtrUtilityService | 3 | F811: External facade for NTR_UTIL.ERB (NtrMark5, NtrAddSurrender, GetNWithVisitor). `src/Era.Core/Counter/` namespace |
| IWcSexHaraService | 3 | F811: External facade for WC_SexHara.ERB functions. `src/Era.Core/Counter/` namespace |
| ICounterSourceHandler | 6 | F803: Main counter source handler — HandleCounterSource, DatUIBottom, DatUIBottomT, DatUITop, DatUITopT, PainCheckVMaster |
| ITouchSet | 1 | F803: Touch/contact state recording — `TouchSet(int mode, int type, CharacterId target)`. Implemented by SOURCE_POSE.ERB handler (F811 scope) |
| IShrinkageSystem | 1 | F803: Tightness variation — `UpdateShrinkage(CharacterId character, int amount, int type)`. Corresponds to 締り具合変動 ERB calls |

**CounterActionId** (`src/Era.Core/Counter/CounterActionId.cs`) — 52-member enum mapping DIM.ERH CNT_ constants (range 10-91).

**Implementations**: ActionSelector (`IActionSelector` + `ICounterSystem`), ActionValidator (`IActionValidator`). F804: WcActionSelector (`IActionSelector` + `IWcCounterSystem`), WcActionValidator (`IActionValidator`). F805: WcCounterSourceHandler (`IWcCounterSourceHandler`), WcCounterMessage (`IWcCounterOutputHandler`).

### ISourceCalculator (F812)

```csharp
// src/Era.Core/Counter/ISourceCalculator.cs - SOURCE1.ERB calculation functions
// 35 void methods for SOURCE1.ERB calculation functions, injected into SOURCE Entry System (F811)
// DI: AddSingleton<ISourceCalculator, SourceCalculator>()

// All methods: void Source*(CharacterId slave, CharacterId master)
void SourceExp(CharacterId slave, CharacterId master);
void SourcePleasureC(CharacterId slave, CharacterId master);
void SourcePleasureV(CharacterId slave, CharacterId master);
void SourcePleasureA(CharacterId slave, CharacterId master);
void SourcePleasureB(CharacterId slave, CharacterId master);
// ... 30 more: SourceGivePleasureC/V/A/B, SourceCvabExtra, SourceAffection,
//     SourceSexualActivity, SourceAchievement, SourcePain, SourceFear,
//     SourceLiquid, SourceArousal, SourceObedience, SourceExposure,
//     SourceSubmission, SourcePleasureEnjoyment, SourceConquest, SourcePassive,
//     SourceSeduction, SourceHumiliation, SourceProvocation, SourceService,
//     SourceCoercion, SourceSadism, SourceFilth, SourceDepression,
//     SourceDeviation, SourceAntipathy, SourceExtra, SourceDownbase
```

**SourceCalculator** (`src/Era.Core/Counter/SourceCalculator.cs`): `sealed class` with primary constructor DI:
```csharp
public sealed class SourceCalculator(
    IVariableStore variables,
    IEngineVariables engine,
    ITEquipVariables tequip,
    ICommonFunctions common,
    IRelationVariables relation)
```

### ICsvNameResolver (F790, extended F777)

```csharp
// src/Era.Core/Interfaces/ICsvNameResolver.cs - CSV constant name array read access
// DI: AddSingleton<ICsvNameResolver, NullCsvNameResolver>() (default)

string GetAblName(int index);   // ABLNAME (from ABL.CSV)
string GetExpName(int index);   // EXPNAME (from EXP.CSV)
string GetMarkName(int index);  // MARKNAME (from MARK.CSV)
string GetPalamName(int index); // PALAMNAME (from PALAM.CSV)
string GetTalentName(int index);   // TALENTNAME (from TALENT.CSV) - F777
string GetTrainName(int comId);    // TRAINNAME (COM display name) - F809
```

### IItemVariables (F776)

```csharp
// src/Era.Core/Interfaces/IItemVariables.cs - Item variable array access (shop subsystem)
// DI: AddSingleton<IItemVariables, NullItemVariables>() (default)

// ITEM array (owned item count, read/write)
int GetItem(int itemId);
void SetItem(int itemId, int value);

// ITEMSALES array (availability flag: 0=available, -1=sold out, -2=locked, >0=limited stock)
int GetItemSales(int itemId);
void SetItemSales(int itemId, int value);

// ITEMPRICE array (item price, read-only CSV)
int GetItemPrice(int itemId);

// ITEMNAME array (item display name, read-only CSV)
string GetItemName(int itemId);

// NOITEM: global item restriction flag (0=items allowed) - F809
int GetNoItem();
```

### Implementations

| Class | Type | Pattern |
|-------|------|---------|
| `NullEngineVariables` | `internal sealed` | Returns 0 / string.Empty (null object pattern) |
| `NullCsvNameResolver` | `internal sealed` | Returns string.Empty (null object pattern) |
| `EngineVariables` | `internal sealed` | Delegates to ICharacterDataAccess, ICharacterDataService, IRandomProvider |
| `NullItemVariables` | `internal sealed` | Returns 0 / -2 (LOCKED) / string.Empty (null object pattern) |
| `EngineItemVariables` | `internal sealed` | Delegates to GlobalStatic named arrays (ITEM/ITEMSALES/ITEMPRICE/ITEMNAME) |

### IRelationVariables (F812)

```csharp
// src/Era.Core/Interfaces/IRelationVariables.cs - RELATION 2D array access
// Segregated from IVariableStore per ISP (same pattern as ITEquipVariables for TEQUIP)
// DI: AddSingleton<IRelationVariables, NullRelationVariables>()

Result<int> GetRelation(CharacterId character, int otherCharacterNo);
// Maps to RELATION:character:(NO:otherCharacter). otherCharacterNo is CSV registration number (NO).
// Returns relation value (1-200, 100=neutral) or error.
```

**NullRelationVariables** (`src/Era.Core/Interfaces/NullRelationVariables.cs`): returns `Success(100)` (neutral).
