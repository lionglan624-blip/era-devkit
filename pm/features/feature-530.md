# Feature 530: Variable Definition CSVs Migration Part 2

## Status: [CANCELLED]

> **Cancellation Reason**: Per F562/F563 architecture analysis, Talent/Ability definitions should remain as C# enums (TalentIndex, AbilityIndex) because new talents/abilities require C# handler implementation. YAML migration only adds phantom moddability - users cannot add new talents/abilities without C# code changes. Talent.csv and Abl.csv migrate NAME MAPPINGS only (handled by F529 and architecture.md update). See F562 "Community Moddability Assessment" and F563 "User Decision" sections for Tier 3 exclusion rationale.

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

**Remaining Variable Definition Files Need Migration**: Talent.csv, Abl.csv, Palam.csv, exp.csv, ex.csv contain critical character attribute and experience definitions that must be migrated to YAML format:
- Talent.csv defines character talent attributes and their properties (verified 1 file exists)
- Abl.csv defines ability attributes and their configurations (verified 1 file exists)
- Palam.csv defines parameter attributes for character states (verified 1 file exists)
- exp.csv defines experience categories and calculations (verified 1 file exists)
- ex.csv defines extended experience attributes (verified 1 file exists)
- These definitions depend on VariableSize array sizes from F528 (predecessor)
- CSV parsing lacks type safety and schema validation
- Current GlobalStatic direct access pattern lacks abstraction for unit testing
- Technical debt accumulation prevents automated quality checks

### Goal (What to Achieve)

1. **Migrate remaining variable definition CSVs** (Talent, Abl, Palam, exp, ex) → YAML format
2. **Implement IDataLoader interfaces** following Phase 4 design compliance
3. **Create strongly typed data models** with proper validation and error handling
4. **Establish DI registration patterns** consistent with F528/F529 precedent
5. **Verify 100% behavioral equivalence** via integration tests and data validation
6. **Achieve zero technical debt** across all implementation files
7. **Complete Phase 17 variable definition migration** enabling subsequent data migrations

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Talent.yaml file created | file | Glob | exists | "Game/Data/Talent.yaml" | [ ] |
| 2 | Abl.yaml file created | file | Glob | exists | "Game/Data/Abl.yaml" | [ ] |
| 3 | Palam.yaml file created | file | Glob | exists | "Game/Data/Palam.yaml" | [ ] |
| 4 | exp.yaml file created | file | Glob | exists | "Game/Data/exp.yaml" | [ ] |
| 5 | ex.yaml file created | file | Glob | exists | "Game/Data/ex.yaml" | [ ] |
| 6 | ITalentLoader interface exists | code | Grep | contains | "interface ITalentLoader" | [ ] |
| 7 | IAblLoader interface exists | code | Grep | contains | "interface IAblLoader" | [ ] |
| 8 | IPalamLoader interface exists | code | Grep | contains | "interface IPalamLoader" | [ ] |
| 9 | IExpLoader interface exists | code | Grep | contains | "interface IExpLoader" | [ ] |
| 10 | IExLoader interface exists | code | Grep | contains | "interface IExLoader" | [ ] |
| 11 | TalentConfig strongly typed model | code | Grep | contains | "public.*class TalentConfig" | [ ] |
| 12 | AblConfig strongly typed model | code | Grep | contains | "public.*class AblConfig" | [ ] |
| 13 | PalamConfig strongly typed model | code | Grep | contains | "public.*class PalamConfig" | [ ] |
| 14 | ExpConfig strongly typed model | code | Grep | contains | "public.*class ExpConfig" | [ ] |
| 15 | ExConfig strongly typed model | code | Grep | contains | "public.*class ExConfig" | [ ] |
| 16 | DI registration for all loaders | code | Grep | contains | "AddSingleton.*ITalentLoader.*YamlTalentLoader" | [ ] |
| 17 | YAML schema validation PASS | test | Bash | succeeds | "dotnet run --project tools/YamlValidator -- Game/Data/Talent.yaml Game/Data/Abl.yaml Game/Data/Palam.yaml Game/Data/exp.yaml Game/Data/ex.yaml" | [ ] |
| 18 | Data equivalence verification PASS | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~VariableDefinitionEquivalence" | [ ] |
| 19 | Engine integration test PASS | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~VariableDefinitionIntegration" | [ ] |
| 20 | Zero technical debt | code | Grep | not_contains | "TODO\\|FIXME\\|HACK" | [ ] |
| 21 | All Era.Core tests PASS | test | Bash | succeeds | "dotnet test Era.Core.Tests" | [ ] |
| 22 | Engine build succeeds | build | Bash | succeeds | "dotnet build engine/uEmuera.Headless.csproj" | [ ] |

### AC Details

**AC#1-5**: YAML file creation
- Test: Glob pattern for each YAML file in Game/Data/
- Content: Variable definition data from respective CSV files converted to YAML format
- Schema: Must follow schemas from YamlSchemaGen tool for each data type

**AC#6-10**: Interface definitions following Phase 4 design
- Test: Grep pattern for each interface in Era.Core/Data/
- Interface pattern: `Result<{Type}Config> Load(string path)` for each type
- XML documentation required per ENGINE quality guide Issue 1

**AC#11-15**: Strongly typed data models
- Test: Grep pattern for each config class in Era.Core/Data/Models/
- Must contain all properties from respective CSV files with proper types
- No magic numbers - use constants or enums per Phase 4 design

**AC#16**: DI registration pattern verification
- Test: Grep pattern for all loader registrations in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- Expected: All 5 loader interfaces registered with YAML implementations
- Pattern: `services.AddSingleton<I{Type}Loader, Yaml{Type}Loader>();`

**AC#17**: YAML schema validation
- Test: YamlValidator CLI against all 5 YAML files
- Must PASS schema validation using generated schemas
- Expected: 0 validation errors across all files

**AC#18**: Data equivalence verification
- Test: dotnet test with filter "VariableDefinitionEquivalence"
- Verifies CSV→YAML data transformation preserves all values exactly for all 5 files
- Expected: 100% data equivalence PASS
- **Minimum**: 3 Assert statements per config type (15 total assertions)

**AC#19**: Engine integration test
- Test: dotnet test with filter "VariableDefinitionIntegration"
- Verifies GlobalStatic can use all new IDataLoader implementations
- Tests actual game initialization flow with YAML data for variable definitions
- **Minimum**: Integration flow from DI container through GlobalStatic for each type

**AC#20**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" in all feature implementation files
- Paths: Era.Core/Data/, engine/Assets/Scripts/Emuera/Services/
- Expected: 0 matches across all feature files

**AC#21**: Era.Core tests regression verification
- Test: dotnet test Era.Core.Tests
- Ensures no regression in Era.Core functionality after adding 5 new data loaders
- Expected: 100% test suite PASS

**AC#22**: Engine build compatibility
- Test: dotnet build engine/uEmuera.Headless.csproj
- Verifies engine can compile with new Era.Core variable definition data loader integrations
- Expected: Build SUCCESS with 0 errors

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4,5 | Convert remaining variable definition CSVs (Talent, Abl, Palam, exp, ex) to YAML format using CsvToYaml tool | [ ] |
| 2 | 6,7,8,9,10 | Create IDataLoader interfaces (ITalentLoader, IAblLoader, IPalamLoader, IExpLoader, IExLoader) following Phase 4 design | [ ] |
| 3 | 11,12,13,14,15 | Implement strongly typed config models (TalentConfig, AblConfig, PalamConfig, ExpConfig, ExConfig) | [ ] |
| 4 | 16 | Create YAML loader implementations with DI registration for all 5 interfaces | [ ] |
| 5 | 17 | Validate YAML schema compliance for all converted files using YamlValidator tool | [ ] |
| 6 | 18 | Implement equivalence verification tests ensuring CSV→YAML data preservation for all 5 types | [ ] |
| 7 | 19 | Create engine integration tests for GlobalStatic compatibility with all variable definition loaders | [ ] |
| 8 | 20 | Remove all TODO/FIXME/HACK comments from implementation files achieving zero debt | [ ] |
| 9 | 21,22 | Verify all tests PASS and engine build succeeds after complete integration | [ ] |

<!-- AC:Task alignment: 22 ACs grouped into 9 logical tasks following atomic operations principle -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Locations**:
- `Game/CSV/Talent.csv` - Character talent definitions (50+ talent types)
- `Game/CSV/Abl.csv` - Ability attribute configurations (20+ ability types)
- `Game/CSV/Palam.csv` - Character parameter definitions (100+ parameters)
- `Game/CSV/exp.csv` - Experience category definitions (30+ experience types)
- `Game/CSV/ex.csv` - Extended experience attributes (10+ extended types)

**Current GlobalStatic Usage Pattern** (to be maintained):
```csharp
// Current pattern in engine code
var talentValue = GlobalStatic.Variables.GetTalentValue(talentId);
var ablMax = GlobalStatic.Variables.GetAblMax(ablId);
var palamValue = GlobalStatic.Param.GetPalam(palamId);
```

### File Structure

| File | Type | Purpose |
|------|------|---------|
| `Era.Core/Data/ITalentLoader.cs` | Interface | Talent data loading contract |
| `Era.Core/Data/IAblLoader.cs` | Interface | Ability data loading contract |
| `Era.Core/Data/IPalamLoader.cs` | Interface | Parameter data loading contract |
| `Era.Core/Data/IExpLoader.cs` | Interface | Experience data loading contract |
| `Era.Core/Data/IExLoader.cs` | Interface | Extended experience data loading contract |
| `Era.Core/Data/Models/TalentConfig.cs` | Model | Strongly typed talent data |
| `Era.Core/Data/Models/AblConfig.cs` | Model | Strongly typed ability data |
| `Era.Core/Data/Models/PalamConfig.cs` | Model | Strongly typed parameter data |
| `Era.Core/Data/Models/ExpConfig.cs` | Model | Strongly typed experience data |
| `Era.Core/Data/Models/ExConfig.cs` | Model | Strongly typed extended experience data |
| `Era.Core/Data/YamlTalentLoader.cs` | Implementation | YAML loading for talent data |
| `Era.Core/Data/YamlAblLoader.cs` | Implementation | YAML loading for ability data |
| `Era.Core/Data/YamlPalamLoader.cs` | Implementation | YAML loading for parameter data |
| `Era.Core/Data/YamlExpLoader.cs` | Implementation | YAML loading for experience data |
| `Era.Core/Data/YamlExLoader.cs` | Implementation | YAML loading for extended experience data |
| `Game/Data/Talent.yaml` | Data | Converted from Talent.csv |
| `Game/Data/Abl.yaml` | Data | Converted from Abl.csv |
| `Game/Data/Palam.yaml` | Data | Converted from Palam.csv |
| `Game/Data/exp.yaml` | Data | Converted from exp.csv |
| `Game/Data/ex.yaml` | Data | Converted from ex.csv |

### Interface Design Pattern

All interfaces follow consistent design pattern:

```csharp
using Era.Core.Types;

namespace Era.Core.Data;

/// <summary>Loads {type} configuration data</summary>
public interface I{Type}Loader
{
    /// <summary>Load {type} configuration from specified path</summary>
    /// <param name="path">Path to configuration file</param>
    /// <returns>Configuration data or error message</returns>
    Result<{Type}Config> Load(string path);
}
```

### DI Registration

Register all interfaces in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<ITalentLoader, YamlTalentLoader>();
services.AddSingleton<IAblLoader, YamlAblLoader>();
services.AddSingleton<IPalamLoader, YamlPalamLoader>();
services.AddSingleton<IExpLoader, YamlExpLoader>();
services.AddSingleton<IExLoader, YamlExLoader>();
```

### Error Message Format

When loading fails, use consistent format: `"Yaml{Type}Loader: {SpecificError}"`
- Example: `"YamlTalentLoader: File not found at path: Game/Data/Talent.yaml"`
- Example: `"YamlAblLoader: Invalid YAML format: line 12 column 5"`

### Engine Integration Pattern

Engine implementations in `engine/Assets/Scripts/Emuera/Services/`:

```csharp
// Example: TalentService.cs - connects to GlobalStatic.Variables
public class TalentService : ITalentLoader
{
    public Result<TalentConfig> Load(string path)
    {
        // Delegate to Era.Core YamlTalentLoader
        // Register TalentConfig values in GlobalStatic.Variables
        return Result<TalentConfig>.Fail("Integration implementation pending - Phase 18");
    }
}
```

**Architecture Note**: Era.Core cannot reference engine layer. Engine creates service implementations that call GlobalStatic, registered via DI. Era.Core provides stub implementations as fallback.

### Test Naming Convention

Test methods follow `Test{ConfigType}Equivalence` and `Test{ConfigType}Integration` format:
- `TestTalentEquivalence` - CSV vs YAML data comparison
- `TestAblEquivalence` - CSV vs YAML data comparison
- `TestPalamEquivalence` - CSV vs YAML data comparison
- `TestExpEquivalence` - CSV vs YAML data comparison
- `TestExEquivalence` - CSV vs YAML data comparison
- `TestTalentIntegration` - Engine integration flow
- `TestAblIntegration` - Engine integration flow
- `TestPalamIntegration` - Engine integration flow
- `TestExpIntegration` - Engine integration flow
- `TestExIntegration` - Engine integration flow

This ensures AC filter patterns match correctly.

### Migration Dependency Chain

**CRITICAL ORDER**:
1. **F528 COMPLETED** - VariableSize.yaml provides array size constants
2. **F529 COMPLETED** - FLAG/CFLAG/TFLAG migration establishes pattern
3. **F530 (THIS FEATURE)** - Remaining variable definitions complete Phase 17 foundation
4. **Next**: Character/Content CSVs can proceed using established patterns

**HIGH PRIORITY**: F530 completion enables full Phase 17 data migration workflow.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Prerequisite | tools/CsvToYaml | CSV to YAML conversion tool |
| Prerequisite | tools/YamlValidator | YAML schema validation |
| Prerequisite | tools/YamlSchemaGen | Schema generation for validation |
| Predecessor | F528 | VariableSize migration must provide array size constants |
| Predecessor | F529 | FLAG/CFLAG/TFLAG migration establishes IDataLoader patterns |
| Related | F538 | CsvToYaml tool creation (may proceed in parallel) |

---

## Links

- [feature-528.md](feature-528.md) - Critical Config Files Migration (predecessor)
- [feature-529.md](feature-529.md) - Variable Definition CSVs Migration Part 1 (predecessor)
- [feature-516.md](feature-516.md) - Phase 17 Planning (parent feature)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 17 definition
- [index-features.md](index-features.md) - Feature index

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Engine service implementation stubs | Era.Core cannot reference engine layer - actual implementations must be in engine project | Feature | F540 (Post-Phase Review will address engine integration pattern) |
| GlobalStatic accessor method verification | Need to verify GlobalStatic accessor patterns for variable definitions exist before integration | Feature | F540 (Post-Phase Review will verify engine compatibility) |
| Character definition CSV dependencies | Chara*.csv files depend on variable definitions from this feature | Feature | F531 (Character CSVs migration) |

---

## Review Notes

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | Created from Phase 17 planning, HIGH priority continuation of F529 | PROPOSED |