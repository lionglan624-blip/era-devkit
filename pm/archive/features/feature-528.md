# Feature 528: Critical Config Files Migration

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

## Background

### Philosophy (Mid-term Vision)

**Phase 17: Data Migration** - Establish YAML/JSON as the single source of truth for all ERA configuration data, ensuring type safety through strongly typed data models and consistent data access via IDataLoader interface pattern. This migration eliminates magic number dependencies and enables automated schema validation while maintaining exact behavioral equivalence with CSV-based legacy data.

### Problem (Current Issue)

**Critical Configuration Blocker**: VariableSize.csv and GameBase.csv are fundamental dependencies that MUST be migrated before any other data migration can proceed:
- VariableSize.csv defines array size constants referenced by all variable definition files (FLAG, CFLAG, TFLAG, etc.)
- GameBase.csv contains core game initialization parameters
- CSV parsing is fragile, lacks type safety, and prevents schema validation
- Current GlobalStatic direct access pattern lacks abstraction for unit testing

### Goal (What to Achieve)

1. **Migrate VariableSize.csv → VariableSize.yaml** with IDataLoader<VariableSizeConfig> interface
2. **Migrate GameBase.csv → GameBase.yaml** with IDataLoader<GameBaseConfig> interface
3. **Implement Phase 4 design compliance** (IDataLoader interface, DI registration, Strongly Typed data models)
4. **Establish migration precedent** for remaining Phase 17 data files
5. **Verify 100% behavioral equivalence** via integration tests
6. **Remove technical debt** from target implementation files

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | VariableSize.yaml file created | file | Glob | exists | Game/config/variable_sizes.yaml | [x] |
| 2 | GameBase.yaml file created | file | Glob | exists | Game/config/game_base.yaml | [x] |
| 3 | IVariableSizeLoader interface exists | code | Grep | contains | "interface IVariableSizeLoader" | [x] |
| 4 | IGameBaseLoader interface exists | code | Grep | contains | "interface IGameBaseLoader" | [x] |
| 5 | VariableSizeConfig strongly typed model | code | Grep | contains | "public.*class VariableSizeConfig" | [x] |
| 6 | GameBaseConfig strongly typed model | code | Grep | contains | "public.*class GameBaseConfig" | [x] |
| 7 | DI registration for VariableSize loader | code | Grep | contains | "AddSingleton.*IVariableSizeLoader.*YamlVariableSizeLoader" | [x] |
| 8 | DI registration for GameBase loader | code | Grep | contains | "AddSingleton.*IGameBaseLoader.*YamlGameBaseLoader" | [x] |
| 9 | VariableSize schema created | file | Glob | exists | tools/schemas/VariableSize.schema.json | [x] |
| 10 | GameBase schema created | file | Glob | exists | tools/schemas/GameBase.schema.json | [x] |
| 11 | VariableSize YAML schema validation PASS | test | Bash | succeeds | - | [x] |
| 12 | GameBase YAML schema validation PASS | test | Bash | succeeds | - | [x] |
| 13 | Data equivalence verification PASS (Pos) | test | Bash | succeeds | - | [x] |
| 14 | Invalid YAML format rejection (Neg) | test | Bash | succeeds | - | [x] |
| 15 | File not found error handling (Neg) | test | Bash | succeeds | - | [x] |
| 16 | Engine integration test PASS (Pos) | test | Bash | succeeds | - | [x] |
| 17 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [x] |
| 18 | All tests PASS | test | Bash | succeeds | - | [x] |
| 19 | Engine build succeeds | build | Bash | succeeds | - | [x] |

### AC Details

**AC#1**: VariableSize.yaml file creation
- Test: Glob pattern="Game/config/variable_sizes.yaml"
- Content: Array size constants from VariableSize.csv converted to YAML format
- Schema: Must follow handwritten VariableSize.schema.json (YamlSchemaGen only supports dialogue schemas)

**AC#2**: GameBase.yaml file creation
- Test: Glob pattern="Game/config/game_base.yaml"
- Content: Game initialization parameters from GameBase.csv converted to YAML format
- Schema: Must follow handwritten GameBase.schema.json (YamlSchemaGen only supports dialogue schemas)

**AC#3**: IVariableSizeLoader interface definition
- Test: Grep pattern="interface IVariableSizeLoader" in Era.Core/Data/IVariableSizeLoader.cs
- Interface must follow Phase 4 design: `Result<VariableSizeConfig> Load(string path)`
- XML documentation required per ENGINE quality guide Issue 1
- Note: File existence is implicitly verified by Grep success. Failure indicates file missing or pattern not found.

**AC#4**: IGameBaseLoader interface definition
- Test: Grep pattern="interface IGameBaseLoader" in Era.Core/Data/IGameBaseLoader.cs
- Interface must follow Phase 4 design: `Result<GameBaseConfig> Load(string path)`
- XML documentation required per ENGINE quality guide Issue 1
- Note: File existence is implicitly verified by Grep success.

**AC#5**: VariableSizeConfig strongly typed model
- Test: Grep pattern="public.*class VariableSizeConfig" in Era.Core/Data/Models/
- Must contain all array size properties from CSV with proper types
- No magic numbers - use constants or enums per Phase 4 design

**AC#6**: GameBaseConfig strongly typed model
- Test: Grep pattern="public.*class GameBaseConfig" in Era.Core/Data/Models/
- Must contain all game initialization properties from CSV with proper types
- No magic numbers - use constants or enums per Phase 4 design

**AC#7**: DI registration for VariableSize loader
- Test: Grep pattern="AddSingleton.*IVariableSizeLoader.*YamlVariableSizeLoader" in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- Expected format: `services.AddSingleton<IVariableSizeLoader, YamlVariableSizeLoader>();`
- Registration follows Phase 4 pattern for data loaders

**AC#8**: DI registration for GameBase loader
- Test: Grep pattern="AddSingleton.*IGameBaseLoader.*YamlGameBaseLoader" in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- Expected format: `services.AddSingleton<IGameBaseLoader, YamlGameBaseLoader>();`
- Registration follows Phase 4 pattern for data loaders

**AC#9**: VariableSize schema created
- Test: Glob pattern="tools/schemas/VariableSize.schema.json"
- Verifies: Schema file exists before YAML validation

**AC#10**: GameBase schema created
- Test: Glob pattern="tools/schemas/GameBase.schema.json"
- Verifies: Schema file exists before YAML validation

**AC#11**: VariableSize YAML schema validation
- Test command: `dotnet run --project tools/YamlValidator -- --schema tools/schemas/VariableSize.schema.json --yaml Game/config/variable_sizes.yaml`
- Requires: tools/schemas/VariableSize.schema.json created by Task#5 (handwritten - YamlSchemaGen only supports dialogue schemas)
- Expected: 0 validation errors

**AC#12**: GameBase YAML schema validation
- Test command: `dotnet run --project tools/YamlValidator -- --schema tools/schemas/GameBase.schema.json --yaml Game/config/game_base.yaml`
- Requires: tools/schemas/GameBase.schema.json created by Task#5 (handwritten - YamlSchemaGen only supports dialogue schemas)
- Expected: 0 validation errors

**AC#13**: Data equivalence verification (Positive test)
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~CriticalConfigEquivalence`
- Test file: `Era.Core.Tests/Data/CriticalConfigEquivalenceTests.cs`
- Verifies CSV→YAML data transformation preserves all values exactly
- Expected: 100% data equivalence PASS
- **Minimum entry coverage**: 104 VariableSize entries (78 active variables with size≥1, plus 26 forbidden A-Z variables with -1→0 conversion) + 7 GameBase entries = 111 distinct entries verified. Note: VariableSizeConfig model has 104 properties (one per variable entry). Multi-dimensional arrays (e.g., DA:305,305) store multiple size values in an int[] property but count as 1 entry. Use parameterized tests - assertion count may be fewer if using data-driven approach.

**AC#14**: Invalid YAML format rejection (Negative test)
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~InvalidYamlRejection`
- Test file: `Era.Core.Tests/Data/CriticalConfigEquivalenceTests.cs` (negative test methods in same file)
- Verifies loader returns Result.Fail() with appropriate error message when YAML format is invalid
- Test cases: malformed YAML syntax, missing required fields, type mismatch (string where int expected)
- Expected: Result.Fail() with error message following format from Implementation Contract

**AC#15**: File not found error handling (Negative test)
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~FileNotFound`
- Test file: `Era.Core.Tests/Data/CriticalConfigIntegrationTests.cs` (negative test methods in same file)
- Verifies loader returns Result.Fail() when file does not exist
- Test cases: non-existent path, empty path, null path
- Expected: Result.Fail() with error message "YamlVariableSizeLoader: File not found at path: {path}" or similar

**AC#16**: Engine integration test (Positive test)
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~CriticalConfigIntegration`
- Test file: `Era.Core.Tests/Data/CriticalConfigIntegrationTests.cs`
- Tests verify DI container can resolve IVariableSizeLoader and IGameBaseLoader, and that loaders successfully load YAML files
- GlobalStatic integration (connecting loaders to GlobalStatic.VariableData) is deferred to dedicated engine feature
- **Minimum**: DI container resolution + loader functionality verification

**AC#17**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" in all feature production files (test files excluded - may contain legitimate TODOs for future coverage)
- Paths: Era.Core/Data/IVariableSizeLoader.cs, Era.Core/Data/IGameBaseLoader.cs, Era.Core/Data/Models/VariableSizeConfig.cs, Era.Core/Data/Models/GameBaseConfig.cs, Era.Core/Data/YamlVariableSizeLoader.cs, Era.Core/Data/YamlGameBaseLoader.cs
- Expected: 0 matches across all production files

**AC#18**: All Era.Core tests PASS
- Test: dotnet test Era.Core.Tests
- Ensures no regression in Era.Core functionality
- Includes: All positive tests (AC#13, AC#16) and negative tests (AC#14, AC#15)
- Expected: 100% test suite PASS

**AC#19**: Engine build succeeds
- Test: dotnet build engine/uEmuera.Headless.csproj
- Verifies engine can compile with new Era.Core data loader integrations
- Expected: Build SUCCESS with 0 errors

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Convert VariableSize.csv and GameBase.csv to YAML format per Implementation Contract Type Mapping and Key Mapping tables | [x] |
| 2 | 3,4 | Create IVariableSizeLoader and IGameBaseLoader interfaces following Phase 4 design | [x] |
| 3 | 5,6 | Implement VariableSizeConfig and GameBaseConfig strongly typed models | [x] |
| 4 | 7,8 | Create YamlVariableSizeLoader and YamlGameBaseLoader implementations with DI registration | [x] |
| 5 | 9,10,11,12 | Create tools/schemas/ directory, write JSON schemas manually for VariableSize and GameBase, validate YAML files using YamlValidator | [x] |
| 6 | 13,14 | Implement equivalence verification tests including positive (CSV→YAML preservation) and negative (invalid YAML rejection) tests | [x] |
| 7 | 15,16 | Create DI container integration tests including positive (loader success) and negative (file not found) tests | [x] |
| 8 | 17 | Remove all TODO/FIXME/HACK comments from implementation files | [x] |
| 9 | 18,19 | Verify all tests PASS and engine build succeeds after integration | [x] |

<!-- AC:Task alignment: 19 ACs grouped into 9 logical tasks following atomic operations principle -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Locations**:
- `Game/CSV/VariableSize.csv` - Array size definitions (104 entries total: 78 active variables + 26 forbidden A-Z variables with -1 values)
- `Game/CSV/GameBase.csv` - Game initialization parameters (7 key-value pairs: コード, バージョン, タイトル, 作者, 製作年, バージョン違い認める, 追加情報)

**Current GlobalStatic Usage Pattern** (reference for integration):
```csharp
// Existing pattern in engine code uses GlobalStatic.VariableData (not Variables)
// GlobalStatic.Variables accessor does NOT exist - F540 will address engine integration
// Actual integration pattern will be established during engine service implementation
```

**Note**: The GlobalStatic.Variables/GameBase accessor pattern in the original design is a target pattern, not current state. Current engine uses GlobalStatic.VariableData. Integration pattern will be finalized in F540 Post-Phase Review.

### File Structure

**Note**: Era.Core/Data/ and Era.Core/Data/Models/ directories do not exist and must be created as part of implementation. Game/config/ directory also needs to be created.

| File | Type | Purpose |
|------|------|---------|
| `Era.Core/Data/IVariableSizeLoader.cs` | Interface | VariableSize data loading contract |
| `Era.Core/Data/IGameBaseLoader.cs` | Interface | GameBase data loading contract |
| `Era.Core/Data/Models/VariableSizeConfig.cs` | Model | Strongly typed VariableSize data |
| `Era.Core/Data/Models/GameBaseConfig.cs` | Model | Strongly typed GameBase data |
| `Era.Core/Data/YamlVariableSizeLoader.cs` | Implementation | YAML loading for VariableSize |
| `Era.Core/Data/YamlGameBaseLoader.cs` | Implementation | YAML loading for GameBase |
| `Game/config/variable_sizes.yaml` | Data | Converted from VariableSize.csv |
| `Game/config/game_base.yaml` | Data | Converted from GameBase.csv |
| `tools/schemas/VariableSize.schema.json` | Schema | JSON schema for VariableSize.yaml validation |
| `tools/schemas/GameBase.schema.json` | Schema | JSON schema for GameBase.yaml validation |
| `Era.Core.Tests/Data/CriticalConfigEquivalenceTests.cs` | Test | CSV→YAML equivalence verification |
| `Era.Core.Tests/Data/CriticalConfigIntegrationTests.cs` | Test | DI container + loader integration |

### Interface Design

**IVariableSizeLoader**:
```csharp
using Era.Core.Types;

namespace Era.Core.Data;

/// <summary>Loads variable size configuration data</summary>
public interface IVariableSizeLoader
{
    /// <summary>Load variable size configuration from specified path</summary>
    /// <param name="path">Path to configuration file</param>
    /// <returns>Configuration data or error message</returns>
    Result<VariableSizeConfig> Load(string path);
}
```

**IGameBaseLoader**:
```csharp
using Era.Core.Types;

namespace Era.Core.Data;

/// <summary>Loads game base configuration data</summary>
public interface IGameBaseLoader
{
    /// <summary>Load game base configuration from specified path</summary>
    /// <param name="path">Path to configuration file</param>
    /// <returns>Configuration data or error message</returns>
    Result<GameBaseConfig> Load(string path);
}
```

### Model Design Note

**VariableSizeConfig** and **GameBaseConfig** models must be placed in `Era.Core/Data/Models/` with proper namespace declaration.

**VariableSizeConfig example structure** (abbreviated - full 104 properties from CSV):
```csharp
namespace Era.Core.Data.Models;

public class VariableSizeConfig
{
    // 1D arrays (int)
    public int DAY { get; init; }
    public int MONEY { get; init; }
    public int FLAG { get; init; }
    public int A { get; init; }  // Forbidden: -1 in CSV → 0 in YAML
    // ... other 1D variables

    // Multi-dimensional arrays (int[])
    // Array length indicates dimensionality: 2 elements = 2D, 3 elements = 3D
    public int[] DA { get; init; } = [];  // [305, 305] = 2D array 305x305
    public int[] DB { get; init; } = [];  // [100, 100] = 2D array 100x100
    // ... other 2D variables

    public int[] TA { get; init; } = [];  // [305, 305, 10] = 3D array 305x305x10
    public int[] TB { get; init; } = [];  // [305, 305, 10] = 3D array 305x305x10
}
```

**GameBaseConfig structure**:
```csharp
namespace Era.Core.Data.Models;

public class GameBaseConfig
{
    public string Code { get; init; } = "";
    public string Version { get; init; } = "";
    public string Title { get; init; } = "";
    public string Author { get; init; } = "";
    public string Year { get; init; } = "";
    public string AllowVersionMismatch { get; init; } = "";
    public string AdditionalInfo { get; init; } = "";
}
```

**Note**: Property names should match CSV variable names exactly (uppercase for VariableSize: DAY, MONEY, etc.; PascalCase for GameBase per Key Mapping table).

**Type Mapping (VariableSize)**:

| CSV Pattern | YAML Format | C# Property Type | Example |
|-------------|-------------|------------------|---------|
| `VAR,N` (N≥0) | `VAR: N` | `int Var { get; init; }` | `DAY: 1000` → `int DAY` |
| `VAR,-1` | `VAR: 0` | `int Var { get; init; }` | `A: 0` → `int A` (forbidden) |
| `VAR,N,M` | `VAR: [N, M]` | `int[] Var { get; init; }` | `DA: [305, 305]` → `int[] DA` (2D) |
| `VAR,N,M,K` | `VAR: [N, M, K]` | `int[] Var { get; init; }` | `TA: [305, 305, 10]` → `int[] TA` (3D) |

**Note on multi-dimensional arrays**: YAML stores dimension sizes as a flat array `[dim1, dim2, dim3]`. The C# property `int[] Var` holds the size values per dimension. The number of elements indicates dimensionality (2 elements = 2D array, 3 elements = 3D array). This preserves the semantic meaning while using simple YAML syntax.

**Key Mapping (GameBase)**:

| CSV Key (Japanese) | YAML Key (English) |
|--------------------|-------------------|
| コード | code |
| バージョン | version |
| タイトル | title |
| 作者 | author |
| 製作年 | year |
| バージョン違い認める | allowVersionMismatch |
| 追加情報 | additionalInfo |

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IVariableSizeLoader, YamlVariableSizeLoader>();
services.AddSingleton<IGameBaseLoader, YamlGameBaseLoader>();
```

### Error Message Format

When loading fails, use format: `"{LoaderName}: {SpecificError}"`
- Example: `"YamlVariableSizeLoader: File not found at path: Game/config/variable_sizes.yaml"`
- Example: `"YamlGameBaseLoader: Invalid YAML format: line 5 column 10"`

### YAML Structure Examples

**VariableSize.yaml expected format** (using inline flow style for consistency):
```yaml
# 1D arrays (single size value)
DAY: 1000
MONEY: 10000
FLAG: 10000
A: 0  # -1 in CSV means variable FORBIDDEN (size=0 per ConstantData.cs)

# 2D arrays - inline flow style [dim1, dim2]
DA: [305, 305]
DB: [100, 100]
DC: [100, 100]
DD: [100, 100]
DE: [100, 100]

# 3D arrays - inline flow style [dim1, dim2, dim3]
TA: [305, 305, 10]
TB: [305, 305, 10]
```

**Note on -1 semantics**: Per Emuera ConstantData.cs (lines 265-282), negative values in VariableSize.csv mean the variable is **forbidden from use** and its size is set to 0. Variables like A-Z with -1 values should be converted to size=0 in YAML.

**Loader type detection**: YamlVariableSizeLoader must detect property type (int vs int[]) to handle 1D vs multi-dimensional arrays.

**GameBase.yaml expected format**:
```yaml
code: "7153"
version: "0040"
title: "era紅魔館protoNTR"
author: "寝の人"
year: "2013-2021"
allowVersionMismatch: "0016"
additionalInfo: "※これは..."
```

### Engine Integration Pattern

**Note**: Engine service implementations are OUT OF SCOPE for this feature. This feature focuses on Era.Core interfaces and YAML data loaders. Engine integration (connecting YamlVariableSizeLoader to GlobalStatic.VariableData) is explicitly deferred to F540 (Post-Phase Review).

**Architecture Rationale**: Era.Core cannot reference engine layer. Era.Core defines interfaces and implementations. Engine layer provides integration services that populate GlobalStatic using Era.Core loaders. This separation is addressed in F540.

### Test Naming Convention

Test methods follow `Test{ConfigType}{TestType}` format:
- `TestVariableSizeEquivalence` - CSV vs YAML data comparison
- `TestGameBaseEquivalence` - CSV vs YAML data comparison
- `TestVariableSizeIntegration` - Engine integration flow
- `TestGameBaseIntegration` - Engine integration flow

This ensures AC filter patterns match correctly.

### Migration Dependency Chain

**CRITICAL ORDER**:
1. **F528 MUST complete first** - VariableSize.yaml defines array sizes for all subsequent migrations
2. **Next**: Variable Definition CSVs (F529, F530) depend on VariableSize array sizes
3. **Then**: Character/Content CSVs can proceed in parallel

**Blocking Impact**: If F528 fails, ALL Phase 17 data migration features are BLOCKED.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Exists | tools/YamlValidator | YAML schema validation (existing tool, uses NJsonSchema) |
| Predecessor | F516 | Phase 17 Planning feature must be completed |
| Successor | F529 | Variable Definition CSVs migration (depends on VariableSize) |
| Successor | F530 | Variable Definition CSVs migration Part 2 |
| Related | F538 | CsvToYaml tool creation (may proceed in parallel, not required - manual conversion documented) |

---

## Links

- [feature-516.md](feature-516.md) - Phase 17 Planning (parent feature)
- [feature-529.md](feature-529.md) - Variable Definition CSVs migration (successor)
- [feature-530.md](feature-530.md) - Variable Definition CSVs migration Part 2 (successor)
- [feature-538.md](feature-538.md) - CsvToYaml Converter Tool (related, parallel)
- [feature-540.md](feature-540.md) - Post-Phase Review Phase 17
- [feature-558.md](feature-558.md) - Engine Integration Services for Critical Config (successor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 17 definition
- [index-features.md](index-features.md) - Feature index

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Engine service implementation | Era.Core cannot reference engine layer. F558 must: (1) Create bridge service connecting Era.Core loaders to GlobalStatic.VariableData, (2) Register in engine DI container | Feature | F558 |
| GlobalStatic accessor verification | F558 must verify: (1) GlobalStatic has properties to receive loaded config data, (2) Integration pattern works with DI-resolved loaders | Feature | F558 |
| F538 conversion rules reference | F528's manual YAML format establishes the source of truth for conversion rules. If F538 (CsvToYaml tool) is created, it should reference F528's YAML format (Game/config/*.yaml) to ensure consistency with subsequent migrations (F529+). | Feature | F538 |

---

## Review Notes

- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - AC#1,2 Expected quotes: Quotes removed from Expected column in iter2.
- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - Task#2/Task#3 merge: Keeping current split for AC:Task 1:1 alignment. Interface and model creation are logically separate file creations.
- **2026-01-18 FL iter5**: [resolved] Phase2-Validate - YAML -1 semantics: Verified via ConstantData.cs lines 265-282. -1 means variable FORBIDDEN (size=0), not size=1. YAML examples updated.
- **2026-01-18 FL iter5**: [resolved] Phase2-Validate - 引継ぎ先指定 specificity: Handoffs updated with explicit verification items for F540.
- **2026-01-18 FL iter6**: [resolved] Phase2-Validate - F540 scope mismatch CRITICAL: User selected option A. Created F558 (Engine Integration Services for Critical Config) with Status:[BLOCKED] on F528. Updated F528 引継ぎ先指定 to reference F558 instead of F540.
- **2026-01-18 FL iter7**: [resolved] Phase2-Validate - AC#10-16 Expected column format: For test|Bash|succeeds pattern, Expected should be '-'. Fixed in iter4 - all test ACs now show '-' in Expected column. AC#10 (file|Glob|exists) correctly uses path as Expected value.
- **2026-01-18 FL iter9**: [resolved] Phase3-Maintainability - Path mismatch with architecture.md: User selected Option C (Game/config/). Updated all AC paths, File Structure, Error Message examples to use `Game/config/variable_sizes.yaml` and `Game/config/game_base.yaml`. Aligns with architecture.md intent while keeping data under Game/.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | Created from F516 Phase 17 Planning, CRITICAL priority | PROPOSED |
| 2026-01-18 20:02 | START | implementer | Task 1 | - |
| 2026-01-18 20:02 | END | implementer | Task 1 | SUCCESS |
| 2026-01-18 20:04 | START | implementer | Task 2 | - |
| 2026-01-18 20:04 | END | implementer | Task 2 | SUCCESS |
| 2026-01-18 20:06 | START | implementer | Task 3 | - |
| 2026-01-18 20:06 | END | implementer | Task 3 | SUCCESS |
| 2026-01-18 20:09 | START | implementer | Task 4 | - |
| 2026-01-18 20:09 | END | implementer | Task 4 | SUCCESS |
| 2026-01-18 20:46 | START | implementer | Task 9 | - |
| 2026-01-18 20:46 | END | implementer | Task 9 | SUCCESS |