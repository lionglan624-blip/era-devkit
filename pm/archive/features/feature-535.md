# Feature 535: Content Definition CSVs Migration Part 2 - Phase 17

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

**Secondary Content Definition Blocker**: Following F528 and F534, four additional content definition CSV files require migration to complete the Phase 17 data transformation:
- Mark.csv contains marking/branding definition data used in character customization
- Juel.csv contains jewelry/accessory definition data for equipment systems
- Stain.csv contains stain/condition definition data for visual state tracking
- source.csv contains source definition data for trigger/event identification
- CSV parsing lacks type safety and prevents schema validation
- Current data access patterns lack abstraction for unit testing
- Missing validation prevents early detection of data corruption

### Goal (What to Achieve)

1. **Migrate Mark.csv → Mark.yaml** with IDataLoader<MarkConfig> interface
2. **Migrate Juel.csv → Juel.yaml** with IDataLoader<JuelConfig> interface
3. **Migrate Stain.csv → Stain.yaml** with IDataLoader<StainConfig> interface
4. **Migrate source.csv → SourceConfig.yaml** with IDataLoader<SourceConfig> interface
5. **Implement Phase 4 design compliance** (IDataLoader interface, DI registration, Strongly Typed data models)
6. **Complete content definition migration precedent** established by F534
7. **Verify 100% behavioral equivalence** via integration tests
8. **Remove technical debt** from target implementation files

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Mark.yaml file created | file | Glob | exists | "Game/Data/Mark.yaml" | [ ] |
| 2 | Juel.yaml file created | file | Glob | exists | "Game/Data/Juel.yaml" | [ ] |
| 3 | Stain.yaml file created | file | Glob | exists | "Game/Data/Stain.yaml" | [ ] |
| 4 | SourceConfig.yaml file created | file | Glob | exists | "Game/Data/SourceConfig.yaml" | [ ] |
| 5 | IMarkLoader interface exists | code | Grep | contains | "interface IMarkLoader" | [ ] |
| 6 | IJuelLoader interface exists | code | Grep | contains | "interface IJuelLoader" | [ ] |
| 7 | IStainLoader interface exists | code | Grep | contains | "interface IStainLoader" | [ ] |
| 8 | ISourceConfigLoader interface exists | code | Grep | contains | "interface ISourceConfigLoader" | [ ] |
| 9 | MarkConfig strongly typed model | code | Grep | contains | "public.*class MarkConfig" | [ ] |
| 10 | JuelConfig strongly typed model | code | Grep | contains | "public.*class JuelConfig" | [ ] |
| 11 | StainConfig strongly typed model | code | Grep | contains | "public.*class StainConfig" | [ ] |
| 12 | SourceConfig strongly typed model | code | Grep | contains | "public.*class SourceConfig" | [ ] |
| 13 | DI registration for Mark loader | code | Grep | contains | "AddSingleton.*IMarkLoader.*YamlMarkLoader" | [ ] |
| 14 | DI registration for Juel loader | code | Grep | contains | "AddSingleton.*IJuelLoader.*YamlJuelLoader" | [ ] |
| 15 | DI registration for Stain loader | code | Grep | contains | "AddSingleton.*IStainLoader.*YamlStainLoader" | [ ] |
| 16 | DI registration for SourceConfig loader | code | Grep | contains | "AddSingleton.*ISourceConfigLoader.*YamlSourceConfigLoader" | [ ] |
| 17 | YAML schema validation PASS | test | Bash | succeeds | "dotnet run --project tools/YamlValidator -- Game/Data/Mark.yaml Game/Data/Juel.yaml Game/Data/Stain.yaml Game/Data/SourceConfig.yaml" | [ ] |
| 18 | Data equivalence verification PASS | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~ContentDefinitionPart2Equivalence" | [ ] |
| 19 | Engine integration test PASS | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~ContentDefinitionPart2Integration" | [ ] |
| 20 | Zero technical debt | code | Grep | not_contains | "TODO\\|FIXME\\|HACK" | [ ] |
| 21 | All tests PASS | test | Bash | succeeds | "dotnet test Era.Core.Tests" | [ ] |
| 22 | Engine build succeeds | build | Bash | succeeds | "dotnet build engine/uEmuera.Headless.csproj" | [ ] |

### AC Details

**AC#1-4**: YAML file creation
- Test: Glob pattern for each YAML file in Game/Data/
- Content: CSV data converted to YAML format using CsvToYaml tool
- Schema: Must follow schemas from YamlSchemaGen tool

**AC#5-8**: Interface definitions
- Test: Grep pattern for each interface in Era.Core/Data/
- Pattern: Each interface follows Phase 4 design: `Result<{ConfigType}> Load(string path)`
- XML documentation required per ENGINE quality guide Issue 1

**AC#9-12**: Strongly typed models
- Test: Grep pattern for each model class in Era.Core/Data/Models/
- Requirements: All properties from CSV with proper types, no magic numbers

**AC#13-16**: DI registration
- Test: Grep pattern for DI registration in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- Pattern: Registration follows Phase 4 pattern for data loaders

**AC#17**: YAML schema validation
- Test: YamlValidator CLI against all 4 YAML files
- Expected: 0 validation errors across all content definition files

**AC#18**: Data equivalence verification
- Test: dotnet test with filter "ContentDefinitionPart2Equivalence"
- Verifies CSV→YAML data transformation preserves all values exactly
- Expected: 100% data equivalence PASS for all 4 config types
- **Minimum**: 3 Assert statements per config type (12 total asserts)

**AC#19**: Engine integration test
- Test: dotnet test with filter "ContentDefinitionPart2Integration"
- Verifies GlobalStatic can use new IDataLoader implementations
- Tests actual game initialization flow with YAML data
- **Minimum**: Integration flow from DI container through GlobalStatic

**AC#20**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" in all feature implementation files
- Paths: Era.Core/Data/, Era.Core/Data/Models/, engine/Assets/Scripts/Emuera/Services/
- Expected: 0 matches across all feature files

**AC#21**: All Era.Core tests PASS
- Test: dotnet test Era.Core.Tests
- Ensures no regression in Era.Core functionality
- Expected: 100% test suite PASS

**AC#22**: Engine build succeeds
- Test: dotnet build engine/uEmuera.Headless.csproj
- Verifies engine can compile with new Era.Core data loader integrations
- Expected: Build SUCCESS with 0 errors

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4 | Convert Mark.csv, Juel.csv, Stain.csv, and source.csv to YAML format using CsvToYaml tool | [ ] |
| 2 | 5,6,7,8 | Create IMarkLoader, IJuelLoader, IStainLoader, and ISourceConfigLoader interfaces following Phase 4 design | [ ] |
| 3 | 9,10,11,12 | Implement MarkConfig, JuelConfig, StainConfig, and SourceConfig strongly typed models | [ ] |
| 4 | 13,14,15,16 | Create YAML loader implementations with DI registration for all 4 config types | [ ] |
| 5 | 17 | Validate YAML schema compliance using YamlValidator tool for all content definition files | [ ] |
| 6 | 18 | Implement equivalence verification tests to ensure CSV→YAML data preservation for all config types | [ ] |
| 7 | 19 | Create engine integration tests for GlobalStatic compatibility with content definition loaders | [ ] |
| 8 | 20 | Remove all TODO/FIXME/HACK comments from implementation files | [ ] |
| 9 | 21,22 | Verify all tests PASS and engine build succeeds after integration | [ ] |

<!-- AC:Task alignment: 22 ACs grouped into 9 logical tasks following atomic operations principle -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Locations**:
- `Game/CSV/Mark.csv` - Marking/branding definitions
- `Game/CSV/Juel.csv` - Jewelry/accessory definitions
- `Game/CSV/Stain.csv` - Stain/condition definitions
- `Game/CSV/source.csv` - Source/trigger definitions

### File Structure

| File | Type | Purpose |
|------|------|---------|
| `Era.Core/Data/IMarkLoader.cs` | Interface | Mark data loading contract |
| `Era.Core/Data/IJuelLoader.cs` | Interface | Juel data loading contract |
| `Era.Core/Data/IStainLoader.cs` | Interface | Stain data loading contract |
| `Era.Core/Data/ISourceConfigLoader.cs` | Interface | SourceConfig data loading contract |
| `Era.Core/Data/Models/MarkConfig.cs` | Model | Strongly typed Mark data |
| `Era.Core/Data/Models/JuelConfig.cs` | Model | Strongly typed Juel data |
| `Era.Core/Data/Models/StainConfig.cs` | Model | Strongly typed Stain data |
| `Era.Core/Data/Models/SourceConfig.cs` | Model | Strongly typed SourceConfig data |
| `Era.Core/Data/YamlMarkLoader.cs` | Implementation | YAML loading for Mark |
| `Era.Core/Data/YamlJuelLoader.cs` | Implementation | YAML loading for Juel |
| `Era.Core/Data/YamlStainLoader.cs` | Implementation | YAML loading for Stain |
| `Era.Core/Data/YamlSourceConfigLoader.cs` | Implementation | YAML loading for SourceConfig |
| `Game/Data/Mark.yaml` | Data | Converted from Mark.csv |
| `Game/Data/Juel.yaml` | Data | Converted from Juel.csv |
| `Game/Data/Stain.yaml` | Data | Converted from Stain.csv |
| `Game/Data/SourceConfig.yaml` | Data | Converted from source.csv |

### Interface Design Pattern

All interfaces follow Phase 4 design with Result<T> pattern and XML documentation:

```csharp
using Era.Core.Types;

namespace Era.Core.Data;

/// <summary>Loads {ConfigType} configuration data</summary>
public interface I{ConfigType}Loader
{
    /// <summary>Load {ConfigType} configuration from specified path</summary>
    /// <param name="path">Path to configuration file</param>
    /// <returns>Configuration data or error message</returns>
    Result<{ConfigType}Config> Load(string path);
}
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IMarkLoader, YamlMarkLoader>();
services.AddSingleton<IJuelLoader, YamlJuelLoader>();
services.AddSingleton<IStainLoader, YamlStainLoader>();
services.AddSingleton<ISourceConfigLoader, YamlSourceConfigLoader>();
```

### Error Message Format

When loading fails, use format: `"{LoaderName}: {SpecificError}"`
- Example: `"YamlMarkLoader: File not found at path: Game/Data/Mark.yaml"`
- Example: `"YamlJuelLoader: Invalid YAML format: line 5 column 10"`

### Test Naming Convention

Test methods follow `Test{ConfigType}{TestType}` format:
- `TestMarkEquivalence` - CSV vs YAML data comparison
- `TestJuelEquivalence` - CSV vs YAML data comparison
- `TestStainEquivalence` - CSV vs YAML data comparison
- `TestSourceConfigEquivalence` - CSV vs YAML data comparison
- `TestMarkIntegration` - Engine integration flow
- `TestJuelIntegration` - Engine integration flow
- `TestStainIntegration` - Engine integration flow
- `TestSourceConfigIntegration` - Engine integration flow

This ensures AC filter patterns match correctly.

### Architecture Note

Era.Core cannot reference engine layer. Engine creates service implementations that call GlobalStatic, registered via DI. Era.Core provides stub implementations as fallback.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Prerequisite | tools/CsvToYaml | CSV to YAML conversion tool |
| Prerequisite | tools/YamlValidator | YAML schema validation |
| Prerequisite | tools/YamlSchemaGen | Schema generation for validation |
| Predecessor | F528 | Critical Config Files Migration (VariableSize, GameBase) |
| Predecessor | F534 | Content Definition CSVs Migration Part 1 (Train, Item, Equip, Tequip) |
| Related | F536 | Character Data Migration (may proceed in parallel after F528) |

---

## Links

- [feature-528.md](feature-528.md) - Critical Config Files Migration (dependency)
- [feature-534.md](feature-534.md) - Content Definition CSVs Migration Part 1 (precedent)
- [feature-516.md](feature-516.md) - Phase 17 Planning (parent feature)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 17 definition
- [index-features.md](index-features.md) - Feature index

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Engine service implementation stubs | Era.Core cannot reference engine layer - actual implementations must be in engine project | Feature | F540 (Post-Phase Review will address engine integration pattern) |
| Content definition accessor verification | Need to verify GlobalStatic accessor patterns exist for Mark/Juel/Stain/Source before integration | Feature | F540 (Post-Phase Review will verify engine compatibility) |
| Schema validation dependency | YamlValidator tool must support multiple file validation in single command | Feature | F538 (Tool enhancement if needed) |

---

## Review Notes

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | Created from F516 Phase 17 Planning, Medium priority, depends on F528 | PROPOSED |