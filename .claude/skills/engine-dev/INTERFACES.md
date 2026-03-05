# Core Interfaces

> **Source**: Extracted from engine-dev SKILL.md for progressive loading.
> All interface signatures for uEmuera engine and Era.Core.

---

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

### ISpecification\<T\> (F546)
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

### IKojoEngine (F452->F476)
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
// Selection algorithm in F552 (PriorityDialogueSelector): filter by condition -> sort by priority -> return highest
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

### ICalendarService
- **File**: `src/Era.Core/Interfaces/ICalendarService.cs`
- **Impl**: `CalendarService` (`State/CalendarService.cs`)
- **DI**: Singleton
- **Methods**: `AdvanceDate()`, `GetMonthName(int)`, `ChildTemperatureResistance(int)`
- **Source**: 天候.ERB @日付変更, @日付_月, @子供気温耐性取得 (F821)

### IClimateDataService
- **File**: `src/Era.Core/Interfaces/IClimateDataService.cs`
- **Impl**: `ClimateDataService` (`State/ClimateDataService.cs`)
- **DI**: Singleton
- **Methods**: `GetWeatherName(int)`, `GetMaxTemperatureBase(int, int)`, `GetMinTemperatureBase(int, int)`, `GetPrecipitationProbabilityBase(int, int)`, `GetThunderProbabilityBase(int, int)`
- **Source**: 天候.ERB @天候, @年間基礎最高気温, @年間基礎最低気温, @年間基礎降水確率, @年間基礎雷発生確率 (F821)

### IWeatherSimulation
- **File**: `src/Era.Core/Interfaces/IWeatherSimulation.cs`
- **Impl**: `WeatherSimulation` (`State/WeatherSimulation.cs`)
- **DI**: Singleton
- **Methods**: `SetDailyTemperature()`, `SetCurrentTemperature()`, `UpdateWeatherState()`, `ApplyWeatherEffects()`
- **Source**: 天候.ERB @日間気温設定, @現在気温設定, @天候状態, @天候によるステータス増減処理 (F821)

### IDateInitializer
- **File**: `src/Era.Core/Interfaces/IDateInitializer.cs`
- **Impl**: `DateInitializer` (`Calendar/DateInitializer.cs`)
- **DI**: Singleton
- **Methods**: `InitializeDate()`
- **Source**: 天候.ERB @日付初期設定 (F828)

### ISleepDepth
- **File**: `src/Era.Core/Interfaces/ISleepDepth.cs`
- **Impl**: `SleepDepth` (`State/SleepDepth.cs`)
- **DI**: Singleton
- **Methods**: `CalculateNoise(int, int)`, `UpdateSleepDepth(int, int)`, `HandleWaking(int, int)`, `EvictCharacters(int, int)`
- **Source**: 睡眠深度.ERB @行為騒音, @特殊起床, @うふふ中起床口上 (F824)

### IMenstrualCycle
- **File**: `src/Era.Core/Interfaces/IMenstrualCycle.cs`
- **Impl**: `MenstrualCycle` (`State/MenstrualCycle.cs`)
- **DI**: Singleton
- **Methods**: `AdvanceCycle(int)`, `ApplyOvulationDrug(int)`, `FormatSimpleStatus(int)`, `ResetCycle(int)`
- **Source**: 生理機能追加パッチ.ERB @生理周期, @排卵誘発剤追加処理, @簡易追加情報 (F824)

### IHeartbreakService
- **File**: `src/Era.Core/Interfaces/IHeartbreakService.cs`
- **Impl**: `HeartbreakService` (`State/HeartbreakService.cs`)
- **DI**: Singleton
- **Methods**: `AcquireHeartbreak(int, int, int)`
- **Source**: 睡眠深度.ERB @素質傷心取得 (F824)

### IClothingPresets (F819)
```csharp
// src/Era.Core/Interfaces/IClothingPresets.cs - ISP sub-interface for clothing preset methods
void PresetNude(CharacterId characterId, int changeUnderwear);
void PresetNightwear(CharacterId characterId, int changeUnderwear);
void PresetNightwearS(CharacterId characterId);
void PresetMale(CharacterId characterId, int changeUnderwear);
void PresetFemale(CharacterId characterId, int changeUnderwear);
void PresetJentle(CharacterId characterId, int changeUnderwear);
void PresetMaid(CharacterId characterId, int changeUnderwear);
void PresetCustom(CharacterId characterId, int changeUnderwear);
void PresetCosplay(CharacterId characterId, int changeUnderwear);
void Preset1(CharacterId characterId, int changeUnderwear);  // 美鈴 + Preset2-Preset13
```
**ISP split from IClothingSystem**. Implemented by `ClothingSystem`.

### IClothingState (F819)
```csharp
// src/Era.Core/Interfaces/IClothingState.cs - ISP sub-interface for clothing state methods
bool IsDressed(CharacterId characterId);
void Save(CharacterId characterId);
void Load(CharacterId characterId, int changeUnderwear);
void Reset(CharacterId characterId, int changeUnderwear);
void SettingTrain(CharacterId characterId);
void ClothesAccessory(CharacterId characterId);
```
**ISP split from IClothingSystem**. Implemented by `ClothingSystem`.

### IClothingEffects (F819)
```csharp
// src/Era.Core/Interfaces/IClothingEffects.cs - ISP sub-interface for underwear/bra selection
void ChangeBra(CharacterId characterId, int changeUnderwear);
int TodaysUnderwear(CharacterId characterId);
int TodaysUnderwearAdult(CharacterId characterId);
```
**ISP split from IClothingSystem**. Stays on `ClothingSystem` (CLOTHES.ERB domain). Distinct from IClothingTrainingEffects.

### IClothingTrainingEffects (F819)
```csharp
// src/Era.Core/Interfaces/IClothingTrainingEffects.cs - Cross-domain CLOTHE_EFFECT.ERB mutations
void ApplyClothingTrainingEffect(CharacterId characterId);
// Additional CLOTHE_EFFECT.ERB methods (EQUIP-state -> TALENT/EXP mutations)
```
**Separate from IClothingEffects**. Implemented by `ClothingEffect` class for TALENT/EXP/SOURCE cross-domain mutations from CLOTHE_EFFECT.ERB.

### INtrQuery (F819)
```csharp
// src/Era.Core/Interfaces/INtrQuery.cs - Narrow interface for NTR-aware accessory logic
bool CheckNtrFavorably(CharacterId character, int threshold);
// Returns true if character's NTR favorability meets the threshold level.
// Maps to NTR_CHK_FAVORABLY(着用者, threshold) in ERB.

// Null implementation (safe default until NTR system migration F825):
// internal sealed class NullNtrQuery : INtrQuery
// => always returns false (no accessories applied when NTR unavailable)
```
**Injected into `ClothingSystem`** for CLOTHES_ACCESSORY ring/choker/collar logic. Full implementation deferred to F825.

### IMultipleBirthService (F822)
```csharp
// src/Era.Core/Interfaces/IMultipleBirthService.cs - Multiple birth operations from 多生児パッチ.ERB
// Runtime implementation deferred to future feature. NullMultipleBirthService provides safe defaults.
int GetBirthCount(int motherId);                                                          // @生まれる人数
void SetMultiBirthFlags(int childId, int multiBirthFlag, int siblingId);                  // @多生児フラグ処理
void DisplayBirthAnnouncement(int childId, int motherId);                                 // @多生児出産口上(0)
void DisplaySubsequentBirthAnnouncement(int childId, int siblingId, int motherId);        // @多生児出産口上(1+)
void ProcessFosterChild(int childId, int motherId);                                       // @里子
void DisplayNonMasterParentAnnouncement(int childId, int motherId, int fatherId);         // @両親がMASTER以外の出産口上
void InitUterusVolumeMultiple(int motherId);                                              // @子宮内体積設定
void SetChildCompatibility(int childId, int motherId, int fatherId);                     // @子供相性設定
```
**Null implementation**: `NullMultipleBirthService` — all births treated as single birth (returns 1 / no-op for all methods). Safe default until runtime implementation.
