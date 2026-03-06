# Feature 558: Engine Integration Services for Critical Config

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

---

## Summary

Create engine integration services that connect Era.Core data loaders (IVariableSizeLoader, IGameBaseLoader) to GlobalStatic runtime properties. Services map Era.Core config classes (VariableSizeConfig, GameBaseConfig) to engine ConstantData arrays, which are then used to construct runtime objects (VariableData, GameBase) for GlobalStatic. This bridges the Era.Core data layer with the Emuera engine layer.

---

## Background

### Philosophy (Mid-term Vision)

**Phase 17: Data Migration** - Establish YAML/JSON as primary source of truth for configurable ERA parameters, with YAML taking precedence over CSV for values that can be overridden. This is an intermediate state towards eliminating CSV dependency entirely. Engine integration services complete the migration pipeline by populating runtime GlobalStatic properties from YAML data after CSV loading.

### Problem (Current Issue)

F528 (Critical Config Files Migration) creates Era.Core data loaders (YamlVariableSizeLoader, YamlGameBaseLoader) with strongly typed models, but cannot connect them to the engine layer:
- Era.Core cannot reference engine assemblies (layer separation)
- GlobalStatic.VariableData requires populated data at runtime
- No bridge service exists to call Era.Core loaders and populate GlobalStatic

### Goal (What to Achieve)

1. **Create VariableSizeService** - Engine-layer service that calls IVariableSizeLoader and populates GlobalStatic.VariableData
2. **Create GameBaseService** - Engine-layer service that calls IGameBaseLoader and populates GlobalStatic.GameBase
3. **Create singleton service instances in GlobalStatic** - Store services as static properties, instantiate during engine startup, retrieve from ProcessInitializer
4. **Verify YAML-to-ConstantData integration** - Confirm runtime data matches YAML source

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| <!-- AC ordering: File existence (1,2,15,16), Class/loader (3-6), GlobalStatic (7-10), Service tests (11,12), Build (13,14), Negative (17,18), Interface (19,20), Reset (21,22), Error tests (23,24,26), Mapping (25) -->
| 1 | VariableSizeService.cs exists | file | Glob | exists | engine\Assets\Scripts\Emuera\Services\VariableSizeService.cs | [x] |
| 2 | GameBaseService.cs exists | file | Glob | exists | engine\Assets\Scripts\Emuera\Services\GameBaseService.cs | [x] |
| 15 | IVariableSizeService.cs exists | file | Glob | exists | engine\Assets\Scripts\Emuera\Services\IVariableSizeService.cs | [x] |
| 16 | IGameBaseService.cs exists | file | Glob | exists | engine\Assets\Scripts\Emuera\Services\IGameBaseService.cs | [x] |
| 3 | VariableSizeService class defined | code | Grep | contains | "class VariableSizeService" | [x] |
| 4 | GameBaseService class defined | code | Grep | contains | "class GameBaseService" | [x] |
| 5 | VariableSizeService uses IVariableSizeLoader | code | Grep | contains | "_loader.Load" | [x] |
| 6 | GameBaseService uses IGameBaseLoader | code | Grep | contains | "_loader.Load" | [x] |
| 7 | GlobalStatic exposes VariableSizeService | code | Grep | contains | "VariableSizeService" | [x] |
| 8 | GlobalStatic exposes GameBaseService | code | Grep | contains | "GameBaseService" | [x] |
| 9 | ProcessInitializer invokes VariableSizeService | code | Grep | contains | "VariableSizeService" | [x] |
| 10 | ProcessInitializer invokes GameBaseService | code | Grep | contains | "GameBaseService" | [x] |
| 11 | VariableSizeService populates ConstantData arrays from YAML | test | Bash | succeeds | dotnet test --filter FullyQualifiedName~GlobalStaticIntegration_VariableSizeService | [x] |
| 12 | GameBaseService populates GameBase properties from YAML | test | Bash | succeeds | dotnet test --filter FullyQualifiedName~GlobalStaticIntegration_GameBaseService | [x] |
| 13 | All tests PASS | test | Bash | succeeds | dotnet test engine.Tests | [x] |
| 14 | Engine build succeeds | build | Bash | succeeds | dotnet build engine/uEmuera.Headless.csproj | [x] |
| 17 | Process.cs not assigning ConstantData | code | Grep | not_contains | "GlobalStatic\\.ConstantData\\s*=" | [x] |
| 18 | Process.cs not assigning GameBaseData | code | Grep | not_contains | "GlobalStatic\\.GameBaseData\\s*=" | [x] |
| 19 | IVariableSizeService interface defined | code | Grep | contains | "public interface IVariableSizeService" | [x] |
| 20 | IGameBaseService interface defined | code | Grep | contains | "public interface IGameBaseService" | [x] |
| 21 | GlobalStatic.Reset() sets services to null | code | Grep | contains | "_variableSizeService = null" | [x] |
| 22 | GlobalStatic.Reset() sets GameBaseService to null | code | Grep | contains | "_gameBaseService = null" | [x] |
| 23 | YAML load failure degrades gracefully | test | Bash | succeeds | dotnet test --filter FullyQualifiedName~GlobalStaticIntegration_YamlLoadFailure | [x] |
| 24 | VariableSizeService getter throws when not initialized | test | Bash | succeeds | dotnet test --filter FullyQualifiedName~GlobalStaticIntegration_ServiceThrowsBehavior | [x] |
| 26 | GameBaseService getter throws when not initialized | test | Bash | succeeds | dotnet test --filter FullyQualifiedName~GlobalStaticIntegration_GameBaseServiceThrowsBehavior | [x] |
| 25 | All VariableSizeConfig properties mapped | test | Bash | succeeds | dotnet test --filter FullyQualifiedName~GlobalStaticIntegration_AllVariableSizeProperties | [x] |

### AC Details

**AC#1-2**: Service file existence
- Test: `Glob pattern="engine/Assets/Scripts/Emuera/Services/VariableSizeService.cs"`
- Test: `Glob pattern="engine/Assets/Scripts/Emuera/Services/GameBaseService.cs"`
- Verifies: Service files created at expected locations

**AC#3-4**: Service class definition
- Test: `Grep pattern="class VariableSizeService" path="engine/Assets/Scripts/Emuera/Services/VariableSizeService.cs" type=cs`
- Test: `Grep pattern="class GameBaseService" path="engine/Assets/Scripts/Emuera/Services/GameBaseService.cs" type=cs`
- Verifies: Engine-layer service classes defined in correct files

**AC#5-6**: Era.Core loader integration
- Test: `Grep pattern="_loader.Load" path="engine/Assets/Scripts/Emuera/Services/VariableSizeService.cs" type=cs`
- Test: `Grep pattern="_loader.Load" path="engine/Assets/Scripts/Emuera/Services/GameBaseService.cs" type=cs`
- Verifies: Services actually invoke Era.Core loader Load() method

**AC#7-8**: GlobalStatic property registration
- Test: `Grep pattern="VariableSizeService" path="engine/Assets/Scripts/Emuera/GlobalStatic.cs" type=cs`
- Test: `Grep pattern="GameBaseService" path="engine/Assets/Scripts/Emuera/GlobalStatic.cs" type=cs`
- Verifies: Services exposed via GlobalStatic static properties

**AC#9-10**: ProcessInitializer integration
- Test: `Grep pattern="VariableSizeService" path="engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs" type=cs`
- Test: `Grep pattern="GameBaseService" path="engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs" type=cs`
- Verifies: ProcessInitializer calls services after CSV loading

**AC#11-12**: GlobalStatic integration
- Test file: `engine.Tests/Tests/GlobalStaticIntegrationTests.cs`
- Test naming: `GlobalStaticIntegration_{ServiceName}` (e.g., `GlobalStaticIntegration_VariableSizeService`)
- Test: `dotnet test engine.Tests --filter FullyQualifiedName~GlobalStaticIntegration`
- AC#11: Test setup requirements (isolated test, differs from production): (1) Call GlobalStatic.Reset(), (2) Create ConstantData via ProcessInitializer.LoadConstantData() or equivalent test harness that populates GlobalStatic.ConstantData with CSV defaults (arrays initialized with ArraySizeLarge defaults except FLAG which uses ArraySizeXLarge). **Test Setup**: engine.Tests can access ProcessInitializer via GlobalStatic.ProcessInitializer (internal class accessible within engine assembly). Use GlobalStatic.ProcessInitializer.LoadConstantData(csvDir, console, false) with mock EmueraConsole. (3) **CRITICAL: Create test YAML with FLAG value DIFFERENT from default (ArraySizeXLarge = 10000 from ConstantData.cs, test YAML should use 9999) to verify YAML overwrites CSV defaults**, (4) Instantiate VariableSizeService with YamlVariableSizeLoader, (5) Call Initialize(testYamlPath) and assert result is Result<Unit>.Success, (6) Verify GlobalStatic.ConstantData.VariableIntArrayLength[FLAG_index] equals YAML value (NOT ArraySizeXLarge default) - confirms YAML value overwrites CSV-loaded default
- AC#12: Test setup requirements: (1) Create test YAML with known title value, (2) Instantiate GameBaseService with YamlGameBaseLoader, (3) Call Initialize(testYamlPath) after GlobalStatic.GameBase exists and assert result is Result<Unit>.Success, (4) Verify GlobalStatic.GameBase.ScriptTitle equals YAML title value (verifies YAML precedence over CSV)
- Verifies: YAML values appear correctly in ConstantData/GameBase properties

**AC#13**: Test suite
- Test: `dotnet test engine.Tests`
- Verifies: No regression from integration

**AC#14**: Engine build
- Test: `dotnet build engine/uEmuera.Headless.csproj`
- Verifies: Engine compiles with new services

**AC#17**: Process.cs ConstantData assignment removal
- Test: `Grep pattern="GlobalStatic\.ConstantData\\s*=" path="engine/Assets/Scripts/Emuera/GameProc/Process.cs" type=cs`
- Verifies: Process.cs no longer assigns ConstantData to GlobalStatic (ProcessInitializer now handles assignment)
- NOTE: AC#17 uses not_contains matcher - FAILS (pattern found) until Task#7 removes assignments, then PASSES (pattern not found). Test is executable throughout but result interpretation changes.

**AC#18**: Process.cs GameBaseData assignment removal
- Test: `Grep pattern="GlobalStatic\.GameBaseData\\s*=" path="engine/Assets/Scripts/Emuera/GameProc/Process.cs" type=cs`
- Verifies: Process.cs no longer assigns GameBaseData to GlobalStatic (ProcessInitializer now handles assignment)
- NOTE: AC#18 uses not_contains matcher - FAILS (pattern found) until Task#7 removes assignments, then PASSES (pattern not found). Test is executable throughout but result interpretation changes.

**AC#19**: IVariableSizeService interface definition
- Test: `Grep pattern="public interface IVariableSizeService" path="engine/Assets/Scripts/Emuera/Services/IVariableSizeService.cs" type=cs`
- Verifies: IVariableSizeService interface is properly defined in interface file

**AC#20**: IGameBaseService interface definition
- Test: `Grep pattern="public interface IGameBaseService" path="engine/Assets/Scripts/Emuera/Services/IGameBaseService.cs" type=cs`
- Verifies: IGameBaseService interface is properly defined in interface file

**AC#21**: GlobalStatic.Reset() service reset
- Test: `Grep pattern="_variableSizeService = null" path="engine/Assets/Scripts/Emuera/GlobalStatic.cs" type=cs`
- Verifies: Reset() method properly clears VariableSizeService (service has no default constructor)
- NOTE: AC#21 is verified only after Task#3 completes. During Task#1a-2b, this AC will show current state (not yet modified).

**AC#22**: GlobalStatic.Reset() GameBaseService reset
- Test: `Grep pattern="_gameBaseService = null" path="engine/Assets/Scripts/Emuera/GlobalStatic.cs" type=cs`
- Verifies: Reset() method properly clears GameBaseService (service has no default constructor)
- NOTE: AC#22 is verified only after Task#3 completes. During Task#1a-2b, this AC will show current state (not yet modified).

**AC#23**: YAML load failure graceful degradation
- Test setup: Create engine.Tests integration test method 'GlobalStaticIntegration_YamlLoadFailureGracefulness' that: (1) Deletes or renames variable_sizes.yaml, (2) Calls VariableSizeService.Initialize() with missing path, (3) Asserts result is Result<Unit>.Failure AND GlobalStatic.ConstantData still has CSV default values
- Test command: `dotnet test engine.Tests --filter FullyQualifiedName~GlobalStaticIntegration_YamlLoadFailure`
- Verifies: Error handling logs failure and continues with CSV fallback values per Implementation Contract error handling

**AC#24**: VariableSizeService getter throws when not initialized
- Test setup: Create engine.Tests integration test method 'GlobalStaticIntegration_ServiceThrowsBehavior' that: (1) Calls GlobalStatic.Reset() to clear services, (2) Attempts to access GlobalStatic.VariableSizeService, (3) Asserts InvalidOperationException is thrown
- Test command: `dotnet test engine.Tests --filter FullyQualifiedName~GlobalStaticIntegration_ServiceThrowsBehavior`
- Verifies: Pattern deviation from F461 fallback behavior is intentional and working as designed

**AC#26**: GameBaseService getter throws when not initialized
- Test setup: Create engine.Tests integration test method 'GlobalStaticIntegration_GameBaseServiceThrowsBehavior' that: (1) Calls GlobalStatic.Reset() to clear services, (2) Attempts to access GlobalStatic.GameBaseService, (3) Asserts InvalidOperationException is thrown
- Test command: `dotnet test engine.Tests --filter FullyQualifiedName~GlobalStaticIntegration_GameBaseServiceThrowsBehavior`
- Verifies: Consistent throw behavior between VariableSizeService and GameBaseService

**AC#25**: All VariableSizeConfig properties mapped
- Test setup: Create engine.Tests integration test method 'GlobalStaticIntegration_AllVariableSizeProperties' that uses reflection to get all properties of VariableSizeConfig class and verifies each one is correctly mapped to corresponding ConstantData array elements. Focus on verifying all 104 VariableSizeConfig properties are verified (one per property)
- Test command: `dotnet test engine.Tests --filter FullyQualifiedName~GlobalStaticIntegration_AllVariableSizeProperties`
- Verifies: All 104 VariableSizeConfig properties (1D int, 1D str, 2D, 3D, Character categories) are correctly mapped per F528 Type Mapping categorization, ensuring complete Task#1 coverage


---

## Implementation Contract

### File Structure

| File | Purpose |
|------|---------|
| `engine/Assets/Scripts/Emuera/Services/IVariableSizeService.cs` | Service interface for variable size configuration |
| `engine/Assets/Scripts/Emuera/Services/IGameBaseService.cs` | Service interface for game base configuration |
| `engine/Assets/Scripts/Emuera/Services/VariableSizeService.cs` | Service bridging IVariableSizeLoader to GlobalStatic |
| `engine/Assets/Scripts/Emuera/Services/GameBaseService.cs` | Service bridging IGameBaseLoader to GlobalStatic |
| `engine.Tests/Tests/GlobalStaticIntegrationTests.cs` | Integration tests for GlobalStatic population |

### Test Naming Convention

Test methods follow `GlobalStaticIntegration_{ServiceName}` format (engine.Tests convention differs from Era.Core.Tests `Test{ConfigType}{TestType}` pattern). This ensures AC filter patterns match correctly:
- `GlobalStaticIntegration_VariableSizeService` - tests VariableSizeService integration
- `GlobalStaticIntegration_GameBaseService` - tests GameBaseService integration

### Service Interface Pattern

**Required assembly references**: VariableSizeService.cs and GameBaseService.cs require ProjectReference to Era.Core for IVariableSizeLoader/IGameBaseLoader interfaces and Era.Core.Types for Result<T>/Unit types. The engine.csproj already references Era.Core (verify existing reference).

```csharp
// Engine-layer service interfaces (following existing DI pattern)
// NOTE: These interfaces are engine-layer only (namespace MinorShift.Emuera.Services), not Era.Core interfaces.
// This differs from F461's pattern but is correct for layer separation - F558 services cannot be in Era.Core.
namespace MinorShift.Emuera.Services;

public interface IVariableSizeService
{
    Result<Unit> Initialize(string yamlPath);
}

public interface IGameBaseService
{
    Result<Unit> Initialize(string yamlPath);
}

// Engine-layer service (can access GlobalStatic)
public class VariableSizeService : IVariableSizeService
{
    private readonly IVariableSizeLoader _loader;

    public VariableSizeService(IVariableSizeLoader loader)
    {
        _loader = loader;
    }

    public Result<Unit> Initialize(string yamlPath)
    {
        // Design choice: Path resolution happens in ProcessInitializer (caller) and Initialize() receives the resolved path.
        // This enables testability by allowing test files to be passed in instead of hardcoded paths.
        // PRECONDITION: GlobalStatic.ConstantData must be assigned before calling Initialize()
        // Initialize() reads GlobalStatic.ConstantData (precondition: must be assigned before call) and modifies its arrays in-place. Returns Result<Unit>.Ok on success.
        if (GlobalStatic.ConstantData == null) return Result<Unit>.Fail("ConstantData not initialized");
        var result = _loader.Load(yamlPath);
        return result.Match(
            onSuccess: config => {
                // Integration point: Call VariableSizeService.Initialize() AFTER constant.LoadData(csvDir, console, displayReport)
                // returns but BEFORE returning constant from LoadConstantData() (ProcessInitializer.cs lines 118-123).
                // Services populate ConstantData.VariableIntArrayLength etc. arrays.
                // VariableData construction happens AFTER ConstantData population in engine startup (Process.cs line 98).

                // COMPLETE PROPERTY MAPPING - All 104 VariableSizeConfig properties
                // Authoritative source: Era.Core/Data/Models/VariableSizeConfig.cs
                // Target arrays: ConstantData.cs lines 156-191

                // === (1) 1D Int Variables → VariableIntArrayLength ===
                // DAY, MONEY, TIME, ITEM, ITEMSALES, NOITEM, BOUGHT, PBAND, FLAG, TFLAG,
                // TARGET, MASTER, PLAYER, ASSI, ASSIPLAY, UP, DOWN, LOSEBASE, PALAMLV, EXPLV,
                // EJAC, PREVCOM, SELECTCOM, NEXTCOM, RESULT, COUNT
                // Note: A-Z (26 single-letter vars) are forbidden in YAML config

                // === (2) NAME Variables → MaxDataList[index] ===
                // Index constants from ConstantData.cs lines 50-76:
                // ITEMNAME    → MaxDataList[itemIndex]     (index = 14)
                // ABLNAME     → MaxDataList[ablIndex]      (index = 0)
                // TALENTNAME  → MaxDataList[talentIndex]   (index = 1)
                // EXPNAME     → MaxDataList[expIndex]      (index = 2)
                // MARKNAME    → MaxDataList[markIndex]     (index = 3)
                // PALAMNAME   → MaxDataList[palamIndex]    (index = 4)
                // TRAINNAME   → MaxDataList[trainIndex]    (index = 5)
                // BASENAME    → MaxDataList[baseIndex]     (index = 6)
                // SOURCENAME  → MaxDataList[sourceIndex]   (index = 7)
                // EXNAME      → MaxDataList[exIndex]       (index = 8)
                // EQUIPNAME   → MaxDataList[equipIndex]    (index = 9)
                // TEQUIPNAME  → MaxDataList[tequipIndex]   (index = 10)
                // FLAGNAME    → MaxDataList[flagIndex]     (index = 11)
                // CFLAGNAME   → MaxDataList[cflagIndex]    (index = 12)
                // TFLAGNAME   → MaxDataList[tflagIndex]    (index = 13)

                // === (3) 1D String Variables → VariableStrArrayLength ===
                // SAVESTR, RESULTS, TSTR, STR

                // === (4) Character Int Variables → CharacterIntArrayLength ===
                // BASE, MAXBASE, ABL, TALENT, EXP, MARK, PALAM, SOURCE, EX, CFLAG,
                // JUEL, RELATION, EQUIP, TEQUIP, STAIN, GOTJUEL, NOWEX, TCVAR

                // === (5) Character String Variables → CharacterStrArrayLength ===
                // CSTR

                // === (6) Special Variables ===
                // LOCAL, LOCALS, ARG, ARGS, GLOBAL, GLOBALS

                // === (7) 2D Int Arrays → VariableIntArray2DLength ===
                // Encoding: ((Int64)dim1 << 32) + dim2
                // DITEMTYPE, DA, DB, DC, DD, DE

                // === (8) 3D Int Arrays → VariableIntArray3DLength ===
                // Encoding: ((Int64)dim1 << 40) + ((Int64)dim2 << 20) + dim3
                // TA, TB

                // Access GlobalStatic.ConstantData to populate arrays (assigned before Initialize() call)
                // CRITICAL: All VariableCode values have type flags in upper bits - MUST use __LOWERCASE__ mask
                // Pattern: (int)(VariableCode.__LOWERCASE__ & VariableCode.XXX) extracts base index (lower 16 bits)
                // Example mapping implementation:
                GlobalStatic.ConstantData.VariableIntArrayLength[(int)(VariableCode.__LOWERCASE__ & VariableCode.FLAG)] = config.FLAG;
                GlobalStatic.ConstantData.VariableIntArrayLength[(int)(VariableCode.__LOWERCASE__ & VariableCode.DAY)] = config.DAY;
                // NAME variables - same mask pattern
                GlobalStatic.ConstantData.MaxDataList[(int)(VariableCode.__LOWERCASE__ & VariableCode.ABLNAME)] = config.ABLNAME;
                GlobalStatic.ConstantData.MaxDataList[(int)(VariableCode.__LOWERCASE__ & VariableCode.ITEMNAME)] = config.ITEMNAME;
                GlobalStatic.ConstantData.VariableStrArrayLength[(int)(VariableCode.__LOWERCASE__ & VariableCode.STR)] = config.STR;
                GlobalStatic.ConstantData.CharacterIntArrayLength[(int)(VariableCode.__LOWERCASE__ & VariableCode.ABL)] = config.ABL;
                // ... (all remaining properties per categories above)
                return Result<Unit>.Ok(Unit.Value);
            },
            onFailure: error => Result<Unit>.Fail(error)
        );
    }
}
```

### GlobalStatic Registration Pattern

```csharp
// In GlobalStatic.cs - static property pattern
// NOTE: Pattern deviation from existing GlobalStatic properties - uses throw instead of fallback
// REASON: These services have no parameterless constructors (require loader injection),
// so fallback pattern (get => _service ?? new FallbackType()) is not applicable.
// Verified in AC#21-22: "(service has no default constructor)"
public static class GlobalStatic
{
    private static IVariableSizeService _variableSizeService;
    private static IGameBaseService _gameBaseService;

    public static IVariableSizeService VariableSizeService {
        get => _variableSizeService ?? throw new InvalidOperationException("VariableSizeService not initialized");
        set => _variableSizeService = value;
    }
    public static IGameBaseService GameBaseService {
        get => _gameBaseService ?? throw new InvalidOperationException("GameBaseService not initialized");
        set => _gameBaseService = value;
    }

    // In Reset() method - clear services during reset (matches existing DI pattern for service instances)
    // NOTE: Insert service resets alongside existing F461 DI service resets (CharacterManager, StyleManager, GameState) for consistency. Reset order should match property declaration order.
    public static void Reset()
    {
        // ... existing resets ...
        _variableSizeService = null;  // No default constructor - set to null
        _gameBaseService = null;     // No default constructor - set to null
    }
}
```

**Pattern Comparison: F461 vs F558 GlobalStatic Services**

| Service Type | Example | Getter Pattern | Rationale |
|--------------|---------|----------------|-----------|
| Era.Core DI (F461) | CharacterManagerInstance, StyleManagerInstance, GameStateInstance | `?? new FallbackType()` | Has parameterless stub constructor for fallback |
| Engine-only bridge (F558) | VariableSizeService, GameBaseService | `?? throw InvalidOperationException` | Requires loader injection, no meaningful default |

### ProcessInitializer Integration

Services are instantiated and called within ProcessInitializer after CSV loading:

**Helper method for shared path resolution**:
```csharp
private Result<string> ResolveConfigPath(string csvDir, string fileName, EmueraConsole console)
{
    if (string.IsNullOrEmpty(csvDir)) {
        console.PrintError("csvDir is null or empty, cannot resolve config directory");
        return Result<string>.Fail("Invalid csvDir");
    }
    string parentDir = Path.GetDirectoryName(csvDir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    if (parentDir == null) {
        console.PrintError($"Cannot determine parent directory for csvDir: {csvDir}");
        return Result<string>.Fail("Cannot determine parent directory");
    }
    string configDir = Path.Combine(parentDir, "config");
    return Result<string>.Ok(Path.Combine(configDir, fileName));
}
```

```csharp
// In ProcessInitializer.LoadConstantData() - after constant.LoadData() returns
// **BREAKING CHANGE**: This changes existing pattern where Process.cs handles GlobalStatic.ConstantData assignment
// Rationale: Centralize YAML service calls within ProcessInitializer for consistency with GameBaseService
// Required: Remove GlobalStatic.ConstantData assignment from Process.cs (Task#7)
// NOTE: LoadConstantData() return type remains ConstantData, but the method NOW handles GlobalStatic assignment internally
public ConstantData LoadConstantData(string csvDir, EmueraConsole console, bool displayReport)
{
    var constant = new ConstantData();
    constant.LoadData(csvDir, console, displayReport);

    // F558: Instantiate and call VariableSizeService after CSV loading
    // Path resolution: Use shared helper method for cross-platform compatibility
    var variableSizePath = ResolveConfigPath(csvDir, "variable_sizes.yaml", console).Match(
        onSuccess: path => path,
        onFailure: error => {
            console.PrintError($"Failed to resolve variable_sizes.yaml path: {error}");
            return null;
        });
    if (variableSizePath == null) return constant;

    var variableSizeLoader = new YamlVariableSizeLoader(); // No constructor parameters
    var variableSizeService = new VariableSizeService(variableSizeLoader);
    GlobalStatic.VariableSizeService = variableSizeService;

    // Assign ConstantData to GlobalStatic BEFORE calling service (service reads from GlobalStatic.ConstantData)
    GlobalStatic.ConstantData = constant;
    // NOTE: VariableSizeService.Initialize() reads GlobalStatic.ConstantData (assigned above) and modifies its arrays in-place
    var result = variableSizeService.Initialize(variableSizePath); // Service modifies GlobalStatic.ConstantData in-place
    if (result is Result<Unit>.Failure rf) {
        // ERROR HANDLING: During migration (F558): Log error, continue with CSV values (graceful degradation)
        // After migration (F575): YAML failure is fatal, engine cannot start
        console.PrintError($"Failed to initialize VariableSize from YAML: {rf.Error}");
    }

    return constant;
}

// LoadGameBase() modification - CHOSEN: Option B (Modify ProcessInitializer pattern)
// **BREAKING CHANGE**: This changes existing pattern where Process.cs handles GlobalStatic assignment
// Rationale: Centralize YAML service calls within ProcessInitializer for consistency with VariableSizeService
// Required: Remove GlobalStatic.GameBaseData assignment from Process.cs (Task#7)
// NOTE: LoadGameBase() return type remains GameBase, but the method NOW handles GlobalStatic assignment internally
public GameBase LoadGameBase(string csvDir, EmueraConsole console)
{
    var gamebase = new GameBase();
    if (!gamebase.LoadGameBaseCsv(csvDir + "GAMEBASE.CSV")) {
        console.PrintError("Failed to load GAMEBASE.CSV");
    }

    // F558 Option B: Assign to GlobalStatic inside ProcessInitializer (pattern change)
    GlobalStatic.GameBaseData = gamebase;

    // F558: Instantiate and call GameBaseService after GameBase is available in GlobalStatic
    // Path resolution: Use shared helper method for cross-platform compatibility
    var gameBasePath = ResolveConfigPath(csvDir, "game_base.yaml", console).Match(
        onSuccess: path => path,
        onFailure: error => {
            console.PrintError($"Failed to resolve game_base.yaml path: {error}");
            return null;
        });
    if (gameBasePath == null) return gamebase;

    var gameBaseLoader = new YamlGameBaseLoader(); // No constructor parameters
    var gameBaseService = new GameBaseService(gameBaseLoader);
    GlobalStatic.GameBaseService = gameBaseService;
    var result = gameBaseService.Initialize(gameBasePath); // Initialize() loads YAML from gameBasePath and applies values to GlobalStatic.GameBaseData (already assigned above from CSV)
    if (result is Result<Unit>.Failure rf) {
        // ERROR HANDLING: During migration (F558): Log error, continue with CSV values (graceful degradation)
        // After migration (F575): YAML failure is fatal, engine cannot start
        console.PrintError($"Failed to initialize GameBase from YAML: {rf.Error}");
    }

    return gamebase;
}
```

### Architecture Note

Era.Core cannot reference GlobalStatic (engine layer). Integration uses this pattern:
- **Era.Core**: Defines IVariableSizeLoader, IGameBaseLoader interfaces + YAML implementations
- **engine**: Creates VariableSizeService, GameBaseService that call loaders and populate GlobalStatic
- **Registration**: Engine startup creates services and assigns to GlobalStatic static properties
- **CSV Integration**: YAML takes precedence over CSV. GameBaseService runs AFTER LoadGameBaseCsv() completes and replaces any property present in YAML. CSV provides initial values that are fully overwritten by YAML values when present. ScriptTitle and other configurable properties in game_base.yaml completely replace CSV values.

### GameBaseService Property Mapping

GameBaseService maps GameBaseConfig (Era.Core strings) to GameBase (engine types):
- `GameBaseConfig.Code` (string) → `GameBase.ScriptUniqueCode` (Int64) - Convert.ToInt64() or 0 if parse fails
- `GameBaseConfig.Version` (string) → `GameBase.ScriptVersion` (Int64) - Convert.ToInt64() or 0 if parse fails
- `GameBaseConfig.Title` (string) → `GameBase.ScriptTitle` (string) - Direct assignment
- `GameBaseConfig.Author` (string) → `GameBase.ScriptAutherName` (string) - Direct assignment
- `GameBaseConfig.Year` (string) → `GameBase.ScriptYear` (string) - Direct assignment
- `GameBaseConfig.AllowVersionMismatch` (bool) → Not mapped (GameBase has no AllowVersionMismatch property)
- `GameBaseConfig.AdditionalInfo` (string) → `GameBase.ScriptDetail` (string) - Direct assignment
- Error handling: Parse failures for numeric conversions should use console.PrintWarning and use fallback value
- **Note**: 6 of 7 GameBaseConfig properties are mapped (Code, Version, Title, Author, Year, AdditionalInfo). AllowVersionMismatch is not mapped since GameBase has no corresponding property.

**Pattern Distinction**: Unlike F461 services (CharacterManager etc.) which delegate to Era.Core stub implementations as fallback, F558 services have no meaningful stub behavior - calling Initialize() without a path would be invalid. Therefore InvalidOperationException on access before initialization is the correct pattern. Era.Core interface implementations (IGameState, ICharacterManager) delegate to GlobalStatic and provide interface contracts. Engine-only services (VariableSizeService, GameBaseService) bridge layers with IVariableSizeService, IGameBaseService interfaces following existing GlobalStatic DI pattern - they take Era.Core loaders as dependencies and populate GlobalStatic directly.

### Data Mapping Note

VariableSizeService maps Era.Core config to engine runtime:
- `VariableSizeConfig` (Era.Core) → `ConstantData[]` arrays with specific mappings:
  - 1D int vars (FLAG, DAY, etc.) → VariableIntArrayLength
  - 1D str vars (STR, SAVESTR) → VariableStrArrayLength
  - Character int vars (ABL, CFLAG) → CharacterIntArrayLength
  - 2D vars (DA, DB) → VariableIntArray2DLength with encoding: ((Int64)dim1 << 32) + dim2
- `ConstantData[]` arrays → `VariableData` constructor → `GlobalStatic.VariableData`

---

## Tasks

**Note**: AC:Task 1:1 principle requires task restructuring. Current tasks violate this principle - Task#1 covers 6 ACs, Task#2 covers 5 ACs, Task#5 covers 5 ACs.

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1a | 1,15 | Create IVariableSizeService interface file | [x] |
| 1b | 3,5,19 | Create VariableSizeService implementation with IVariableSizeLoader dependency | [x] |
| 1c | 25 | Implement all 104 VariableSizeConfig property mappings in VariableSizeService.Initialize() | [x] |
| 2a | 2,16 | Create IGameBaseService interface file | [x] |
| 2b | 4,6,20 | Create GameBaseService implementation with IGameBaseLoader dependency | [x] |
| 3 | 7,8,21,22 | Add VariableSizeService and GameBaseService properties to GlobalStatic.cs and Reset() method <!-- Batch waiver (Task 3): GlobalStatic property addition requires corresponding Reset() update for consistency --> | [x] |
| 4 | 9,10 | Modify ProcessInitializer.LoadConstantData() and LoadGameBase() to instantiate services | [x] |
| 7 | 17,18 | Remove GlobalStatic.ConstantData and GameBaseData assignments from Process.cs (ProcessInitializer now handles assignment) | [x] |
| 5a | 11,24 | Create VariableSizeService integration tests | [x] |
| 5b | 12,26 | Create GameBaseService integration tests | [x] |
| 5c | 23 | Create YAML load failure graceful degradation test | [x] |
| 6 | 13,14 | Verify all tests PASS and engine builds | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F528 [DONE] | Critical Config Files Migration must complete first (creates Era.Core loaders) |
| Successor | F576 [PROPOSED] | Character 2D Array Support (receives handoff - informational tracking only) |

---

## Links

- [feature-461.md](feature-461.md) - Implementation Contract pattern reference
- [feature-528.md](feature-528.md) - Critical Config Files Migration (predecessor)
- [feature-540.md](feature-540.md) - Related infra feature
- [feature-575.md](feature-575.md) - CSV完全廃止 handoff destination
- [feature-576.md](feature-576.md) - Character 2D Array Support Extension
- [index-features.md](index-features.md) - Feature index

---

## Deferred Acceptance Criteria

| AC# | Description | Deferred to | Reason |
|:---:|-------------|-------------|--------|
| 24 | Post-F575 YAML load failure is fatal | F575 verification | Test scenario requires F575 (CSV completely eliminated) to be [DONE]. After F575 implementation, missing YAML file should cause engine startup failure instead of graceful degradation |

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| CSV完全廃止 | F558 maintains CSV fallback intentionally during migration. F575 will change error handling from graceful degradation to fatal failure to eliminate CSV loading entirely | feature-{ID}.md | F575 |
| Character 2D Arrays | ConstantData has CharacterIntArray2DLength and CharacterStrArray2DLength but VariableSizeConfig model lacks character 2D array properties. Extension needed for completeness | feature-{ID}.md | F576 |

---

## Review Notes

- **2026-01-18**: Created to resolve F528 handoff scope mismatch. F540 (Type:infra) cannot implement engine services.
- [resolved] Phase1-Uncertain iter1: F576 exists but is incomplete stub (AC section says 'To be defined during implementation'). F558 handoff delegates Character 2D arrays to F576 but F576 needs proper spec before F558 can be considered complete. Leak prevention requires concrete tracking. **Resolution**: F576 added to index-features.md Active Features, Dependencies updated to [PROPOSED].
- [resolved] Phase1-Uncertain iter2: F576 does exist with AC 'To be defined during implementation'. Leak prevention concern is valid per CLAUDE.md. However, F576 is not in index-features.md Active Features which may indicate incomplete creation. Fix option (B) is reasonable but requires verifying if F576's stub status is acceptable per workflow. **Resolution**: F576 added to index-features.md Active Features.
- [resolved] Phase1-Uncertain iter2: Implementation Contract line 247 already references 'Era.Core/Data/Models/VariableSizeConfig.cs which defines all properties to map'. The fix option (B) is already partially addressed. Whether complete mapping table is required depends on implementer guidance standards. **Resolution**: No additional mapping table required - existing reference is sufficient.
- [resolved] Phase1-Uncertain iter2: AC:Task 1:1 principle is noted in F558 line 434 ('Note: AC:Task 1:1 principle requires task restructuring'). The current grouping may be acceptable if class definition necessarily includes related items. Fix suggests documenting rationale which is already partially done. **Resolution**: Current grouping is acceptable - interface+class creation in same task is logical.
- [resolved] Phase1-Uncertain iter2: Not_contains matcher with '-' in Expected is functional. The fix is stylistic improvement but not required for correctness. **Resolution**: Current format follows testing SKILL.md requirements.
- [resolved] Phase1-Uncertain iter3: F558 already tracks F575 in Links section (line 467) and 引継ぎ先指定 section (line 484). Adding F575 as Successor in Dependencies table would improve bidirectional traceability but is not strictly required since Successor type is 'Informational' per feature-template.md. The existing references provide adequate tracking. **Resolution**: Current tracking is adequate.
- [resolved] Phase1-Uncertain iter5: AC numbers are non-sequential (1,2,15,16,3,4,5,...) creating readability issues. ENGINE.md Issue 23 shows sequential numbering preference but specifically addresses lettered suffixes (2a, 2b) not integer order. Current grouping logic preserves functional structure (file existence, interface definition, integration). While improvement is possible, no explicit SSOT mandate exists for sequential numbering. **Resolution**: Added AC ordering comment explaining grouping rationale.
- [resolved] Phase1-Uncertain iter5: AC#11 test setup description is extremely long (8 numbered steps). No SSOT specifies AC Details length limits. Moving test setup to Implementation Contract separates verification requirements from AC, potentially causing confusion. Stylistic preference without SSOT backing. **Resolution**: Current AC Details format is acceptable per feature-template.md.
- [resolved] Phase1-Uncertain iter5: ResolveConfigPath helper method ResolveConfigPath is already defined (lines 300-313). The verbose pattern matching for Result<T> follows ENGINE.md Issue 2 guidance. While DRY improvement is valid, the suggested fix (Match() method) doesn't exist in current Result<T> implementation. The current pattern is consistent with project conventions. **Resolution**: Current pattern matches project conventions.
- [resolved] Phase1-Uncertain iter7: F576 Predecessor status shows [PROPOSED] but F558 Dependencies shows F576 as Successor not Predecessor. This is correct but note that F576's Dependencies table shows F558 as Predecessor with status [PROPOSED] which needs update when F558 status changes. **Resolution**: Added reminder note to F576 Review Notes per user decision.
- [resolved] Phase1-Uncertain iter7: AC ordering comment on line 53 references AC numbers but the actual ordering in table differs from comment. **Resolution**: Comment updated to reflect actual AC ordering per user decision.
- [skipped] Phase1-Uncertain iter9: Result<Unit>.Fail() vs pattern matching inconsistency observation - both pattern matching and Match() are used in the feature. ENGINE.md Issue 2 documents both patterns as valid. **Resolution**: User confirmed both patterns should be maintained as per ENGINE.md guidance.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | FL orchestrator | Created from F528 pending issue resolution | PROPOSED |
| 2026-01-21 08:46 | START | implementer | Task 2a | - |
| 2026-01-21 08:46 | END | implementer | Task 2a | SUCCESS |
| 2026-01-21 08:46 | START | implementer | Task 1a | - |
| 2026-01-21 08:46 | END | implementer | Task 1a | SUCCESS |
| 2026-01-21 08:49 | START | implementer | Task 1b | - |
| 2026-01-21 08:49 | START | implementer | Task 2b | - |
| 2026-01-21 08:49 | END | implementer | Task 2b | SUCCESS |
| 2026-01-21 08:49 | END | implementer | Task 1b | SUCCESS |
| 2026-01-21 08:51 | START | implementer | Task 1c | - |
| 2026-01-21 08:51 | END | implementer | Task 1c | SUCCESS |
| 2026-01-21 08:54 | START | implementer | Task 3 | - |
| 2026-01-21 08:54 | END | implementer | Task 3 | SUCCESS |
| 2026-01-21 08:56 | START | implementer | Task 4 | - |
| 2026-01-21 08:56 | END | implementer | Task 4 | SUCCESS |
| 2026-01-21 09:01 | START | implementer | Task 5a-5c | - |
| 2026-01-21 09:01 | END | implementer | Task 5a-5c | SUCCESS |
| 2026-01-21 09:02 | START | implementer | Task 6 | - |
| 2026-01-21 09:02 | END | implementer | Task 6 | SUCCESS |
