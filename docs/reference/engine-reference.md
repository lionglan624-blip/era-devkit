# Engine Reference (uEmuera)

> **Last Updated:** 2025-12-20
> **Purpose:** Essential engine knowledge for C# modifications

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

---

## Key Directories

- `Assets/Scripts/Emuera/` - Pure C# (GameProc, GameData, GameView, Sub)
- `uEmuera.Headless/` - CLI project

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

**CLI Options**: `--flow <json>`, `--unit <func>`, `--debug`, `--parallel [N]`, `--fail-fast`, `--diff`

**Build**: `dotnet build engine/uEmuera.Headless.csproj`

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

## Additional Interfaces

### IWarningOutput
Warning message output interface for display/logging.
```csharp
void PrintWarning(string message, ScriptPosition position, int level);
void PrintSystemLine(string line);
```

### IFontManager
Font caching and retrieval for rendering.
```csharp
Font GetFont(string fontName, int fontSize, FontStyle style);
void ClearCache();
```

### IGameBase
Game script metadata (title, author, version).
```csharp
string ScriptTitle { get; }
string ScriptAutherName { get; }
Int64 ScriptVersion { get; }
bool UniqueCodeEqualTo(Int64 target);
bool CheckVersion(Int64 target);
```

### ITokenReader
Lexical analysis operations for ERB parsing.
```csharp
Int64 ReadInt64(StringStream st, bool retZero);
IdentifierWord ReadFirstIdentifierWord(StringStream st);
string ReadString(StringStream st, StrEndWith endWith);
OperatorCode ReadOperator(StringStream st, bool allowAssignment);
int SkipAllSpace(StringStream st);
```

### IProcessInitializer
Process initialization with testable loading operations.
```csharp
bool InitializeParserAndCheckConfig(EmueraConsole console);
bool LoadResources(EmueraConsole console);
GameBase LoadGameBase(string csvDir, EmueraConsole console);
ConstantData LoadConstantData(string csvDir, EmueraConsole console, bool displayReport);
bool LoadErbFiles(string erbDir, bool displayReport, EmueraConsole console, ...);
```

### IErbFileReader
ERB file loading and parsing.
```csharp
bool LoadErbFiles(string erbDir, bool displayReport, LabelDictionary labelDictionary);
bool LoadErbs(List<string> paths, LabelDictionary labelDictionary);
```

### INestValidator
Control structure nesting validation (IF/REPEAT/SELECTCASE).
```csharp
void NestCheck(FunctionLabelLine label);
```

### IJumpResolver
GOTO/CALL/JUMP destination resolution.
```csharp
bool UseCallForm { get; set; }
void SetJumpTo(FunctionLabelLine label);
```

### IMainWindow
Main window operations for display updates.
```csharp
void Update();
void clear_richText();
string InternalEmueraVer { get; }
string EmueraVerText { get; }
```

### ILabelDictionary
Label/function name resolution.
```csharp
bool Initialized { get; set; }
FunctionLabelLine GetNonEventLabel(string key);
GotoLabelLine GetLabelDollar(string key, FunctionLabelLine labelAtLine);
```

### IConstantData
Constant value definitions from CSV.
```csharp
bool isDefined(VariableCode varCode, string str);
```

### IExpressionMediator
Expression evaluation and console output.
```csharp
VariableEvaluator VEvaluator { get; }
Process Process { get; }
void ForceKana(Int64 flag);
string ConvertStringType(string str);
void OutputToConsole(string str, FunctionIdentifier func);
string CreateBar(Int64 var, Int64 max, Int64 length);
```

### IIdentifierDictionary
Identifier (variable/function) name resolution.
```csharp
VariableToken GetVariableToken(string key, string subKey, bool allowPrivate);
bool getVarTokenIsForbid(string key);
FunctionIdentifier GetFunctionIdentifier(string str);
void CheckUserLabelName(ref string errMes, ref int warnLevel, bool isFunction, string labelName);
void resizeLocalVars(string key, string subKey, int newSize);
```

### IVariableData
Variable data storage access.
```csharp
VariableToken GetSystemVariableToken(string str);
```

### IVariableEvaluator
Variable evaluation and runtime state.
```csharp
VariableData VariableData { get; }
ConstantData Constant { get; }
Int64 GetNextRand(Int64 max);
Int64 CHARANUM { get; }
Int64 TARGET { get; set; }
Int64 RESULT { get; set; }
string RESULTS { get; set; }
Int64 GetChara(Int64 charaNo);
```

### IProcess
Core process state (extended from Core Interfaces section).
```csharp
LabelDictionary LabelDictionary { get; }
VariableEvaluator VEvaluator { get; }
LogicalLine getCurrentLine { get; }
bool inInitializeing { get; }
LogicalLine GetScaningLine();
bool SkipPrint { get; set; }
int MethodStack();
```
