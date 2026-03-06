# Feature 534: Content Definition CSVs Migration Part 1 - Phase 17, Train.csv Item.csv Equip.csv Tequip.csv

## Status: [PROPOSED]

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

**Content Definition Data Fragility**: Four critical content definition CSV files contain game content constants that are currently parsed via fragile CSV reading without type safety:
- Train.csv (204 training commands) - Core gameplay command definitions
- Item.csv (73 items) - Game item database
- Equip.csv (156 equipment) - Character equipment definitions
- Tequip.csv (64 target equipment) - Equipment targeting system
- Current CSV parsing lacks schema validation and type safety
- Magic number dependencies in game code accessing these definitions
- No automated verification of data consistency across CSV updates

### Goal (What to Achieve)

1. **Migrate content definition CSVs → YAML** with strongly typed data models
2. **Implement IDataLoader pattern compliance** following F528 precedent
3. **Establish type-safe content access** via ITrainLoader, IItemLoader, IEquipLoader, ITequipLoader interfaces
4. **Enable schema validation** for all content definition data
5. **Verify 100% behavioral equivalence** via content equivalence tests
6. **Remove technical debt** from implementation files
7. **Establish migration pattern** for remaining content CSV files in Phase 17

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | training.yaml file created | file | Glob | exists | "Game/content/training.yaml" | [ ] |
| 2 | items.yaml file created | file | Glob | exists | "Game/content/items.yaml" | [ ] |
| 3 | equipment.yaml file created | file | Glob | exists | "Game/content/equipment.yaml" | [ ] |
| 4 | target_equipment.yaml file created | file | Glob | exists | "Game/content/target_equipment.yaml" | [ ] |
| 5 | ITrainLoader interface exists | code | Grep | contains | "interface ITrainLoader" | [ ] |
| 6 | IItemLoader interface exists | code | Grep | contains | "interface IItemLoader" | [ ] |
| 7 | IEquipLoader interface exists | code | Grep | contains | "interface IEquipLoader" | [ ] |
| 8 | ITequipLoader interface exists | code | Grep | contains | "interface ITequipLoader" | [ ] |
| 9 | TrainConfig strongly typed model | code | Grep | contains | "public.*class TrainConfig" | [ ] |
| 10 | ItemConfig strongly typed model | code | Grep | contains | "public.*class ItemConfig" | [ ] |
| 11 | EquipConfig strongly typed model | code | Grep | contains | "public.*class EquipConfig" | [ ] |
| 12 | TequipConfig strongly typed model | code | Grep | contains | "public.*class TequipConfig" | [ ] |
| 13 | DI registration for Train loader | code | Grep | contains | "AddSingleton.*ITrainLoader.*YamlTrainLoader" | [ ] |
| 14 | DI registration for Item loader | code | Grep | contains | "AddSingleton.*IItemLoader.*YamlItemLoader" | [ ] |
| 15 | DI registration for Equip loader | code | Grep | contains | "AddSingleton.*IEquipLoader.*YamlEquipLoader" | [ ] |
| 16 | DI registration for Tequip loader | code | Grep | contains | "AddSingleton.*ITequipLoader.*YamlTequipLoader" | [ ] |
| 17 | YAML schema validation PASS | test | Bash | succeeds | - | [ ] |
| 18 | Content equivalence verification PASS (Pos) | test | Bash | succeeds | - | [ ] |
| 19 | Invalid YAML format rejection (Neg) | test | Bash | succeeds | - | [ ] |
| 20 | File not found error handling (Neg) | test | Bash | succeeds | - | [ ] |
| 21 | Engine integration test PASS (Pos) | test | Bash | succeeds | - | [ ] |
| 22 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [ ] |
| 23 | All Era.Core tests PASS | test | Bash | succeeds | - | [ ] |
| 24 | Engine build succeeds | build | Bash | succeeds | - | [ ] |

### AC Details

**AC#1**: training.yaml file creation
- Test: Glob pattern="Game/content/training.yaml"
- Content: 204 training command definitions from Train.csv converted to YAML format
- Schema: Must follow handwritten training.schema.json (per F528 pattern)

**AC#2**: items.yaml file creation
- Test: Glob pattern="Game/content/items.yaml"
- Content: 73 item definitions from Item.csv converted to YAML format
- Schema: Must follow handwritten items.schema.json (per F528 pattern)

**AC#3**: equipment.yaml file creation
- Test: Glob pattern="Game/content/equipment.yaml"
- Content: 156 equipment definitions from Equip.csv converted to YAML format
- Schema: Must follow handwritten equipment.schema.json (per F528 pattern)

**AC#4**: target_equipment.yaml file creation
- Test: Glob pattern="Game/content/target_equipment.yaml"
- Content: 64 target equipment definitions from Tequip.csv converted to YAML format
- Schema: Must follow handwritten target_equipment.schema.json (per F528 pattern)

**AC#5**: ITrainLoader interface definition
- Test: Grep pattern="interface ITrainLoader" in Era.Core/Data/
- Interface must follow Phase 4 design: `Result<TrainConfig> Load(string path)`
- XML documentation required per ENGINE quality guide Issue 1

**AC#6**: IItemLoader interface definition
- Test: Grep pattern="interface IItemLoader" in Era.Core/Data/
- Interface must follow Phase 4 design: `Result<ItemConfig> Load(string path)`
- XML documentation required per ENGINE quality guide Issue 1

**AC#7**: IEquipLoader interface definition
- Test: Grep pattern="interface IEquipLoader" in Era.Core/Data/
- Interface must follow Phase 4 design: `Result<EquipConfig> Load(string path)`
- XML documentation required per ENGINE quality guide Issue 1

**AC#8**: ITequipLoader interface definition
- Test: Grep pattern="interface ITequipLoader" in Era.Core/Data/
- Interface must follow Phase 4 design: `Result<TequipConfig> Load(string path)`
- XML documentation required per ENGINE quality guide Issue 1

**AC#9**: TrainConfig strongly typed model
- Test: Grep pattern="public.*class TrainConfig" in Era.Core/Data/Models/
- Must contain all training command properties from CSV with proper types
- No magic numbers - use constants or enums per Phase 4 design

**AC#10**: ItemConfig strongly typed model
- Test: Grep pattern="public.*class ItemConfig" in Era.Core/Data/Models/
- Must contain all item properties from CSV with proper types
- No magic numbers - use constants or enums per Phase 4 design

**AC#11**: EquipConfig strongly typed model
- Test: Grep pattern="public.*class EquipConfig" in Era.Core/Data/Models/
- Must contain all equipment properties from CSV with proper types
- No magic numbers - use constants or enums per Phase 4 design

**AC#12**: TequipConfig strongly typed model
- Test: Grep pattern="public.*class TequipConfig" in Era.Core/Data/Models/
- Must contain all target equipment properties from CSV with proper types
- No magic numbers - use constants or enums per Phase 4 design

**AC#13**: DI registration for Train loader
- Test: Grep pattern="AddSingleton.*ITrainLoader.*YamlTrainLoader" in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- Registration follows Phase 4 pattern for data loaders

**AC#14**: DI registration for Item loader
- Test: Grep pattern="AddSingleton.*IItemLoader.*YamlItemLoader" in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- Registration follows Phase 4 pattern for data loaders

**AC#15**: DI registration for Equip loader
- Test: Grep pattern="AddSingleton.*IEquipLoader.*YamlEquipLoader" in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- Registration follows Phase 4 pattern for data loaders

**AC#16**: DI registration for Tequip loader
- Test: Grep pattern="AddSingleton.*ITequipLoader.*YamlTequipLoader" in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- Registration follows Phase 4 pattern for data loaders

**AC#17**: YAML schema validation
- Test: `dotnet run --project tools/YamlValidator -- Game/content/training.yaml Game/content/items.yaml Game/content/equipment.yaml Game/content/target_equipment.yaml`
- Must PASS schema validation using handwritten schemas (training.schema.json, items.schema.json, equipment.schema.json, target_equipment.schema.json)
- Expected: 0 validation errors across all content files

**AC#18**: Content equivalence verification (Positive)
- Test: `dotnet test --filter "FullyQualifiedName~TrainEquivalence|FullyQualifiedName~ItemEquivalence|FullyQualifiedName~EquipEquivalence|FullyQualifiedName~TequipEquivalence"`
- Verifies CSV→YAML data transformation preserves all values exactly
- Expected: 100% data equivalence PASS for all 4 content types
- **Minimum**: 3 Assert statements per content type (12 total asserts)
- Tests in ContentEquivalenceTests class with individual methods per content type

**AC#19**: Invalid YAML format rejection (Negative)
- Test: `dotnet test --filter "FullyQualifiedName~InvalidYamlRejection"`
- Verifies loaders return Result.Fail() for malformed YAML input
- Expected: YamlTrainLoader, YamlItemLoader, YamlEquipLoader, YamlTequipLoader all reject invalid YAML
- **Minimum**: 1 negative test per loader (4 total tests)

**AC#20**: File not found error handling (Negative)
- Test: `dotnet test --filter "FullyQualifiedName~FileNotFound"`
- Verifies loaders return Result.Fail() when file path does not exist
- Expected: All loaders return descriptive error message for missing files
- **Minimum**: 1 negative test per loader (4 total tests)

**AC#21**: Engine integration test (Positive)
- Test: `dotnet test --filter "FullyQualifiedName~TrainIntegration|FullyQualifiedName~ItemIntegration|FullyQualifiedName~EquipIntegration|FullyQualifiedName~TequipIntegration"`
- Verifies GlobalStatic can use new IDataLoader implementations for content access
- Tests actual game content loading flow with YAML data
- **Minimum**: Integration flow from DI container through content access APIs
- Tests in ContentIntegrationTests class with individual methods per content type

**AC#22**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" in all feature implementation files
- Paths: Era.Core/Data/, Era.Core/Data/Models/, engine/Assets/Scripts/Emuera/Services/
- Expected: 0 matches across all feature files

**AC#23**: All Era.Core tests PASS
- Test: dotnet test Era.Core.Tests
- Ensures no regression in Era.Core functionality after content loader integration
- Expected: 100% test suite PASS

**AC#24**: Engine build succeeds
- Test: dotnet build engine/uEmuera.Headless.csproj
- Verifies engine can compile with new Era.Core content loader integrations
- Expected: Build SUCCESS with 0 errors

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4 | Convert Train.csv, Item.csv, Equip.csv, Tequip.csv to YAML format via manual conversion (per F528 pattern) | [ ] |
| 2 | - | Create handwritten JSON schema files (training.schema.json, items.schema.json, equipment.schema.json, target_equipment.schema.json) per F528 pattern | [ ] |
| 3 | 5,6,7,8 | Create ITrainLoader, IItemLoader, IEquipLoader, ITequipLoader interfaces following Phase 4 design | [ ] |
| 4 | 9,10,11,12 | Implement TrainConfig, ItemConfig, EquipConfig, TequipConfig strongly typed models | [ ] |
| 5 | 13,14,15,16 | Create YAML loader implementations with DI registration for all content types | [ ] |
| 6 | 17 | Validate YAML schema compliance using YamlValidator tool | [ ] |
| 7 | 18 | Implement equivalence verification tests (positive) to ensure CSV→YAML content preservation | [ ] |
| 8 | 19,20 | Implement negative tests for invalid YAML rejection and file not found error handling | [ ] |
| 9 | 21 | Create engine integration tests for GlobalStatic content loading compatibility | [ ] |
| 10 | 22 | Remove all TODO/FIXME/HACK comments from implementation files | [ ] |
| 11 | 23,24 | Verify all tests PASS and engine build succeeds after integration | [ ] |

<!-- AC:Task alignment: 24 ACs grouped into 11 logical tasks following atomic operations principle -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Locations**:
- `Game/CSV/Train.csv` - Training command definitions (204 commands)
- `Game/CSV/Item.csv` - Item definitions (73 items)
- `Game/CSV/Equip.csv` - Equipment definitions (156 equipment)
- `Game/CSV/Tequip.csv` - Target equipment definitions (64 target equipment)

### File Structure

| File | Type | Purpose |
|------|------|---------|
| `Era.Core/Data/ITrainLoader.cs` | Interface | Training data loading contract |
| `Era.Core/Data/IItemLoader.cs` | Interface | Item data loading contract |
| `Era.Core/Data/IEquipLoader.cs` | Interface | Equipment data loading contract |
| `Era.Core/Data/ITequipLoader.cs` | Interface | Target equipment data loading contract |
| `Era.Core/Data/Models/TrainConfig.cs` | Model | Strongly typed training data |
| `Era.Core/Data/Models/ItemConfig.cs` | Model | Strongly typed item data |
| `Era.Core/Data/Models/EquipConfig.cs` | Model | Strongly typed equipment data |
| `Era.Core/Data/Models/TequipConfig.cs` | Model | Strongly typed target equipment data |
| `Era.Core/Data/YamlTrainLoader.cs` | Implementation | YAML loading for training data |
| `Era.Core/Data/YamlItemLoader.cs` | Implementation | YAML loading for item data |
| `Era.Core/Data/YamlEquipLoader.cs` | Implementation | YAML loading for equipment data |
| `Era.Core/Data/YamlTequipLoader.cs` | Implementation | YAML loading for target equipment data |
| `Game/content/training.yaml` | Data | Converted from Train.csv |
| `Game/content/items.yaml` | Data | Converted from Item.csv |
| `Game/content/equipment.yaml` | Data | Converted from Equip.csv |
| `Game/content/target_equipment.yaml` | Data | Converted from Tequip.csv |
| `tools/schemas/training.schema.json` | Schema | Handwritten JSON schema for training.yaml |
| `tools/schemas/items.schema.json` | Schema | Handwritten JSON schema for items.yaml |
| `tools/schemas/equipment.schema.json` | Schema | Handwritten JSON schema for equipment.yaml |
| `tools/schemas/target_equipment.schema.json` | Schema | Handwritten JSON schema for target_equipment.yaml |

### Interface Design

**Content Loader Interfaces** (following F528 pattern):

```csharp
using Era.Core.Data.Models;
using Era.Core.Types;

namespace Era.Core.Data;

/// <summary>Loads training command configuration data</summary>
public interface ITrainLoader
{
    /// <summary>Load training command configuration from specified path</summary>
    /// <param name="path">Path to configuration file</param>
    /// <returns>Configuration data or error message</returns>
    Result<TrainConfig> Load(string path);
}

/// <summary>Loads item configuration data</summary>
public interface IItemLoader
{
    /// <summary>Load item configuration from specified path</summary>
    /// <param name="path">Path to configuration file</param>
    /// <returns>Configuration data or error message</returns>
    Result<ItemConfig> Load(string path);
}

/// <summary>Loads equipment configuration data</summary>
public interface IEquipLoader
{
    /// <summary>Load equipment configuration from specified path</summary>
    /// <param name="path">Path to configuration file</param>
    /// <returns>Configuration data or error message</returns>
    Result<EquipConfig> Load(string path);
}

/// <summary>Loads target equipment configuration data</summary>
public interface ITequipLoader
{
    /// <summary>Load target equipment configuration from specified path</summary>
    /// <param name="path">Path to configuration file</param>
    /// <returns>Configuration data or error message</returns>
    Result<TequipConfig> Load(string path);
}
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// Content definition data loaders
services.AddSingleton<ITrainLoader, YamlTrainLoader>();
services.AddSingleton<IItemLoader, YamlItemLoader>();
services.AddSingleton<IEquipLoader, YamlEquipLoader>();
services.AddSingleton<ITequipLoader, YamlTequipLoader>();
```

### Error Message Format

When loading fails, use format: `"{LoaderName}: {SpecificError}"`
- Example: `"YamlTrainLoader: File not found at path: Game/content/training.yaml"`
- Example: `"YamlItemLoader: Invalid YAML format: line 15 column 3"`

### Engine Integration Pattern

Engine implementations in `engine/Assets/Scripts/Emuera/Services/`:

```csharp
// ContentDefinitionServices.cs - connects to GlobalStatic content access
public class TrainService : ITrainLoader
{
    public Result<TrainConfig> Load(string path)
    {
        // Delegate to Era.Core YamlTrainLoader
        // Register TrainConfig values in GlobalStatic training system
        return Result<TrainConfig>.Fail("Integration implementation pending - Phase 18");
    }
}
```

**Architecture Note**: Era.Core cannot reference engine layer. Engine creates service implementations that call GlobalStatic content systems, registered via DI. Era.Core provides stub implementations as fallback.

### Test Naming Convention

Test methods follow `Test{ContentType}Equivalence` and `Test{ContentType}Integration` format:
- `TestTrainEquivalence` - Train CSV vs YAML data comparison
- `TestItemEquivalence` - Item CSV vs YAML data comparison
- `TestEquipEquivalence` - Equipment CSV vs YAML data comparison
- `TestTequipEquivalence` - Target equipment CSV vs YAML data comparison
- `TestTrainIntegration` - Engine integration flow for training data
- `TestItemIntegration` - Engine integration flow for item data
- `TestEquipIntegration` - Engine integration flow for equipment data
- `TestTequipIntegration` - Engine integration flow for target equipment data

This ensures AC filter patterns match correctly.

### Migration Dependency Chain

**CRITICAL ORDER**:
1. **F528 MUST complete first** - VariableSize.yaml establishes IDataLoader pattern
2. **F534 follows F528** - Content definition migration using established pattern
3. **Parallel execution possible** with other content CSV migrations after F528

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Prerequisite | tools/YamlValidator | YAML schema validation |
| Predecessor | F528 | Critical Config Files Migration (establishes IDataLoader pattern) |
| Related | F529 | Variable Definition CSVs migration Part 1 |
| Related | F530 | Variable Definition CSVs migration Part 2 |
| Related | F516 | Phase 17 Planning feature |

---

## Links

- [feature-528.md](feature-528.md) - Critical Config Files Migration (prerequisite)
- [feature-516.md](feature-516.md) - Phase 17 Planning (parent feature)
- [feature-529.md](feature-529.md) - Variable Definition CSVs migration Part 1
- [feature-530.md](feature-530.md) - Variable Definition CSVs migration Part 2
- [feature-535.md](feature-535.md) - Content Definition CSVs Migration Part 2
- [feature-540.md](feature-540.md) - Post-Phase Review
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 17 definition
- [index-features.md](index-features.md) - Feature index

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Engine service implementation stubs | Era.Core cannot reference engine layer - actual implementations must be in engine project | Feature | F540 (Post-Phase Review will address engine integration pattern) |
| Content access API compatibility | Need to verify existing content access patterns in engine before full integration | Feature | F540 (Post-Phase Review will verify engine content API compatibility) |
| Additional content CSV migrations | Other CSV files need similar migration but excluded from this feature scope | Feature | F535 (Content Definition CSVs Migration Part 2) |

---

## Review Notes

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | Created from requirements, engine type, depends on F528, medium priority | PROPOSED |