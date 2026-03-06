# Feature 529: Variable Definition CSVs Migration Part 1

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

**Variable Definition Files Require Migration**: FLAG.CSV, CFLAG.csv, TFLAG.csv contain critical game variable definitions that must be migrated to YAML format:
- FLAG.CSV defines player flags and their default values (verified 1 file exists)
- CFLAG.csv defines character-specific flags (verified 1 file exists)
- TFLAG.csv defines temporary flags for scene management (verified 1 file exists)
- These definitions depend on VariableSize array sizes from F528 (predecessor)
- CSV parsing lacks type safety and schema validation
- Current GlobalStatic direct access pattern lacks abstraction for unit testing

### Goal (What to Achieve)

1. **Migrate FLAG.CSV → FLAG.yaml** with IDataLoader<FlagConfig> interface following Phase 4 design
2. **Migrate CFLAG.csv → CFLAG.yaml** with IDataLoader<CFlagConfig> interface
3. **Migrate TFLAG.csv → TFLAG.yaml** with IDataLoader<TFlagConfig> interface using constants or enums per Phase 4 design (eliminate magic number dependencies)
4. **Implement Phase 4 design compliance** (IDataLoader interface pattern, DI registration, strongly typed models)
5. **Verify 100% behavioral equivalence** via integration tests
6. **Remove technical debt** from all implementation files
7. **Establish precedent** for remaining variable definition files in Part 2

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | FLAG.yaml file created | file | Glob | exists | "Game/Data/FLAG.yaml" | [ ] |
| 2 | CFLAG.yaml file created | file | Glob | exists | "Game/Data/CFLAG.yaml" | [ ] |
| 3 | TFLAG.yaml file created | file | Glob | exists | "Game/Data/TFLAG.yaml" | [ ] |
| 4 | IFlagLoader interface exists | code | Grep | contains | "interface IFlagLoader" | [ ] |
| 5 | ICFlagLoader interface exists | code | Grep | contains | "interface ICFlagLoader" | [ ] |
| 6 | ITFlagLoader interface exists | code | Grep | contains | "interface ITFlagLoader" | [ ] |
| 7 | FlagConfig strongly typed model | code | Grep | contains | "public.*class FlagConfig" | [ ] |
| 8 | CFlagConfig strongly typed model | code | Grep | contains | "public.*class CFlagConfig" | [ ] |
| 9 | TFlagConfig strongly typed model | code | Grep | contains | "public.*class TFlagConfig" | [ ] |
| 10 | DI registration for FLAG loader | code | Grep | contains | "AddSingleton.*IFlagLoader.*YamlFlagLoader" | [ ] |
| 11 | DI registration for CFLAG loader | code | Grep | contains | "AddSingleton.*ICFlagLoader.*YamlCFlagLoader" | [ ] |
| 12 | DI registration for TFLAG loader | code | Grep | contains | "AddSingleton.*ITFlagLoader.*YamlTFlagLoader" | [ ] |
| 13 | YAML schema validation PASS | test | Bash | succeeds | "dotnet run --project tools/YamlValidator -- Game/Data/FLAG.yaml Game/Data/CFLAG.yaml Game/Data/TFLAG.yaml" | [ ] |
| 14 | FLAG equivalence verification PASS | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestFlagEquivalence" | [ ] |
| 15 | CFLAG equivalence verification PASS | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestCFlagEquivalence" | [ ] |
| 16 | TFLAG equivalence verification PASS | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestTFlagEquivalence" | [ ] |
| 17 | Engine integration test PASS | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~VariableDefinitionIntegration" | [ ] |
| 18 | Zero technical debt | code | Grep | not_contains | "TODO\\|FIXME\\|HACK" | [ ] |
| 19 | All tests PASS | test | Bash | succeeds | "dotnet test Era.Core.Tests" | [ ] |
| 20 | Engine build succeeds | build | Bash | succeeds | "dotnet build engine/uEmuera.Headless.csproj" | [ ] |

### AC Details

**AC#1**: FLAG.yaml file creation
- Test: Glob pattern="Game/Data/FLAG.yaml"
- Content: Player flag definitions from FLAG.CSV converted to YAML format
- Schema: Must follow FLAG schema from YamlSchemaGen tool
- Schema generation: Run tools/YamlSchemaGen to generate schema before validation. Output: Game/Data/schemas/FLAG.schema.json (or similar location per tool design).

**AC#2**: CFLAG.yaml file creation
- Test: Glob pattern="Game/Data/CFLAG.yaml"
- Content: Character flag definitions from CFLAG.csv converted to YAML format
- Schema: Must follow CFLAG schema from YamlSchemaGen tool
- Schema generation: Run tools/YamlSchemaGen to generate schema before validation. Output: Game/Data/schemas/CFLAG.schema.json (or similar location per tool design).

**AC#3**: TFLAG.yaml file creation
- Test: Glob pattern="Game/Data/TFLAG.yaml"
- Content: Temporary flag definitions from TFLAG.csv converted to YAML format
- Schema: Must follow TFLAG schema from YamlSchemaGen tool
- Schema generation: Run tools/YamlSchemaGen to generate schema before validation. Output: Game/Data/schemas/TFLAG.schema.json (or similar location per tool design).

**AC#4**: IFlagLoader interface definition
- Test: Grep pattern="interface IFlagLoader" in Era.Core/Data/
- Interface must follow Phase 4 design: `Result<FlagConfig> Load(string path)`
- XML documentation required per ENGINE quality guide Issue 1

**AC#5**: ICFlagLoader interface definition
- Test: Grep pattern="interface ICFlagLoader" in Era.Core/Data/
- Interface must follow Phase 4 design: `Result<CFlagConfig> Load(string path)`
- XML documentation required per ENGINE quality guide Issue 1

**AC#6**: ITFlagLoader interface definition
- Test: Grep pattern="interface ITFlagLoader" in Era.Core/Data/
- Interface must follow Phase 4 design: `Result<TFlagConfig> Load(string path)`
- XML documentation required per ENGINE quality guide Issue 1

**AC#7**: FlagConfig strongly typed model
- Test: Grep pattern="public.*class FlagConfig" in Era.Core/Data/Models/
- Must contain all flag definitions from FLAG.CSV with proper types
- No magic numbers - use constants or enums per Phase 4 design

**AC#8**: CFlagConfig strongly typed model
- Test: Grep pattern="public.*class CFlagConfig" in Era.Core/Data/Models/
- Must contain all character flag definitions from CFLAG.csv with proper types
- No magic numbers - use constants or enums per Phase 4 design

**AC#9**: TFlagConfig strongly typed model
- Test: Grep pattern="public.*class TFlagConfig" in Era.Core/Data/Models/
- Must contain all temporary flag definitions from TFLAG.csv with proper types
- No magic numbers - use constants or enums per Phase 4 design

**AC#10**: DI registration for FLAG loader
- Test: Grep pattern="AddSingleton.*IFlagLoader.*YamlFlagLoader" in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- Registration follows Phase 4 pattern for data loaders

**AC#11**: DI registration for CFLAG loader
- Test: Grep pattern="AddSingleton.*ICFlagLoader.*YamlCFlagLoader" in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- Registration follows Phase 4 pattern for data loaders

**AC#12**: DI registration for TFLAG loader
- Test: Grep pattern="AddSingleton.*ITFlagLoader.*YamlTFlagLoader" in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- Registration follows Phase 4 pattern for data loaders

**AC#13**: YAML schema validation
- Test: YamlValidator CLI against all three YAML files
- Must PASS schema validation using generated schemas
- Expected: 0 validation errors across all files

**AC#14**: FLAG data equivalence verification
- Test: dotnet test with filter "TestFlagEquivalence"
- Verifies FLAG.CSV→FLAG.yaml data transformation preserves all values exactly
- Expected: 100% data equivalence PASS
- **Minimum**: 3 Assert statements per flag type covering: (1) flag count, (2) flag name preservation, (3) default value equivalence

**AC#15**: CFLAG data equivalence verification
- Test: dotnet test with filter "TestCFlagEquivalence"
- Verifies CFLAG.csv→CFLAG.yaml data transformation preserves all values exactly
- Expected: 100% data equivalence PASS
- **Minimum**: 3 Assert statements per flag type covering: (1) flag count, (2) flag name preservation, (3) default value equivalence

**AC#16**: TFLAG data equivalence verification
- Test: dotnet test with filter "TestTFlagEquivalence"
- Verifies TFLAG.csv→TFLAG.yaml data transformation preserves all values exactly
- Expected: 100% data equivalence PASS
- **Minimum**: 3 Assert statements per flag type covering: (1) flag count, (2) flag name preservation, (3) default value equivalence

**AC#17**: Engine integration test
- Test: dotnet test with filter "VariableDefinitionIntegration"
- Verifies GlobalStatic can use new IDataLoader implementations for all three flag types
- Tests actual game variable initialization flow with YAML data
- **Minimum**: Integration flow from DI container through GlobalStatic for FLAGS/CFLAGS/TFLAGS

**AC#18**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" in all feature implementation files
- Paths: Era.Core/Data/, Era.Core/Data/Models/, engine/Assets/Scripts/Emuera/Services/
- Expected: 0 matches across all feature files

**AC#19**: All Era.Core tests PASS
- Test: dotnet test Era.Core.Tests
- Ensures no regression in Era.Core functionality
- Expected: 100% test suite PASS

**AC#20**: Engine build succeeds
- Test: dotnet build engine/uEmuera.Headless.csproj
- Verifies engine can compile with new Era.Core data loader integrations
- Expected: Build SUCCESS with 0 errors

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | Convert FLAG.CSV, CFLAG.csv, TFLAG.csv to YAML format using CsvToYaml tool | [ ] |
| 2 | 4,5,6 | Create IFlagLoader, ICFlagLoader, ITFlagLoader interfaces following Phase 4 design | [ ] |
| 3 | 7,8,9 | Implement FlagConfig, CFlagConfig, TFlagConfig strongly typed models | [ ] |
| 4 | 10,11,12 | Create YAML loader implementations with DI registration for all three flag types | [ ] |
| 5 | 13 | Validate YAML schema compliance using YamlValidator tool | [ ] |
| 6 | 14,15,16 | Implement equivalence verification tests to ensure CSV→YAML data preservation for all flag types | [ ] |
| 7 | 17 | Create engine integration tests for GlobalStatic variable definition compatibility | [ ] |
| 8 | 18 | Remove all TODO/FIXME/HACK comments from implementation files | [ ] |
| 9 | 19,20 | Verify all tests PASS and engine build succeeds after integration | [ ] |

<!-- AC:Task alignment: 20 ACs grouped into 9 logical tasks following atomic operations principle -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Locations**:
- `Game/CSV/FLAG.CSV` - Player flag definitions (verified file exists)
- `Game/CSV/CFLAG.csv` - Character flag definitions (verified file exists)
- `Game/CSV/TFLAG.csv` - Temporary flag definitions (verified file exists)

**Current GlobalStatic Usage Pattern** (to be maintained):
```csharp
// Current pattern in engine code
var playerFlag = GlobalStatic.Variables.GetFlag(flagIndex);
var characterFlag = GlobalStatic.Variables.GetCFlag(characterId, flagIndex);
var tempFlag = GlobalStatic.Variables.GetTFlag(flagIndex);
```

### File Structure

| File | Type | Purpose |
|------|------|---------|
| `Era.Core/Data/IFlagLoader.cs` | Interface | FLAG data loading contract |
| `Era.Core/Data/ICFlagLoader.cs` | Interface | CFLAG data loading contract |
| `Era.Core/Data/ITFlagLoader.cs` | Interface | TFLAG data loading contract |
| `Era.Core/Data/Models/FlagConfig.cs` | Model | Strongly typed FLAG data |
| `Era.Core/Data/Models/CFlagConfig.cs` | Model | Strongly typed CFLAG data |
| `Era.Core/Data/Models/TFlagConfig.cs` | Model | Strongly typed TFLAG data |
| `Era.Core/Data/YamlFlagLoader.cs` | Implementation | YAML loading for FLAG |
| `Era.Core/Data/YamlCFlagLoader.cs` | Implementation | YAML loading for CFLAG |
| `Era.Core/Data/YamlTFlagLoader.cs` | Implementation | YAML loading for TFLAG |
| `Game/Data/FLAG.yaml` | Data | Converted from FLAG.CSV |
| `Game/Data/CFLAG.yaml` | Data | Converted from CFLAG.csv |
| `Game/Data/TFLAG.yaml` | Data | Converted from TFLAG.csv |

### Interface Design

**IFlagLoader**:
```csharp
using Era.Core.Types;

namespace Era.Core.Data;

/// <summary>Provides data loading contract for player flag definitions from YAML sources</summary>
public interface IFlagLoader
{
    /// <summary>Load player flag configuration from specified path</summary>
    /// <param name="path">Path to configuration file</param>
    /// <returns>Configuration data or error message</returns>
    Result<FlagConfig> Load(string path);
}
```

**ICFlagLoader**:
```csharp
using Era.Core.Types;

namespace Era.Core.Data;

/// <summary>Provides data loading contract for character flag definitions from YAML sources</summary>
public interface ICFlagLoader
{
    /// <summary>Load character flag configuration from specified path</summary>
    /// <param name="path">Path to configuration file</param>
    /// <returns>Configuration data or error message</returns>
    Result<CFlagConfig> Load(string path);
}
```

**ITFlagLoader**:
```csharp
using Era.Core.Types;

namespace Era.Core.Data;

/// <summary>Provides data loading contract for temporary flag definitions from YAML sources</summary>
public interface ITFlagLoader
{
    /// <summary>Load temporary flag configuration from specified path</summary>
    /// <param name="path">Path to configuration file</param>
    /// <returns>Configuration data or error message</returns>
    Result<TFlagConfig> Load(string path);
}
```

### Model Structure

**FlagConfig, CFlagConfig, TFlagConfig** strongly typed models map CSV columns to C# properties:

```csharp
// Era.Core/Data/Models/FlagConfig.cs
public class FlagConfig
{
    public List<FlagDefinition> Flags { get; set; } = new();
}

public class FlagDefinition
{
    public int Index { get; set; }           // CSV column 0 (flag number)
    public string Name { get; set; } = "";   // CSV column 1 (flag name)
    public int DefaultValue { get; set; }    // CSV column 2 (default value)
    public string Comment { get; set; } = "";// CSV column 3 (comment/description)
}
```

**CSV→Model Mapping**:
- FLAG.CSV: `Index,Name,DefaultValue,Comment` → `FlagDefinition` list
- CFLAG.csv: Same structure as FLAG, character-specific
- TFLAG.csv: Same structure as FLAG, temporary scope

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IFlagLoader, YamlFlagLoader>();
services.AddSingleton<ICFlagLoader, YamlCFlagLoader>();
services.AddSingleton<ITFlagLoader, YamlTFlagLoader>();
```

### Error Message Format

**Template**: `"{LoaderName}: {SpecificError}"` where LoaderName is the concrete class name (YamlFlagLoader, YamlCFlagLoader, YamlTFlagLoader) and SpecificError describes the failure reason.

Examples:
- `"YamlFlagLoader: File not found at path: Game/Data/FLAG.yaml"`
- `"YamlCFlagLoader: Invalid YAML format: line 5 column 10"`
- `"YamlTFlagLoader: Schema validation failed: missing required field 'defaultValue'"`

### Engine Integration Pattern

Engine implementations in `engine/Assets/Scripts/Emuera/Services/`:

```csharp
// FlagService.cs - connects to GlobalStatic.Variables flag access
public class FlagService : IFlagLoader
{
    public Result<FlagConfig> Load(string path)
    {
        // Delegate to Era.Core YamlFlagLoader
        // Register FlagConfig values in GlobalStatic.Variables
        return Result<FlagConfig>.Fail("Integration implementation deferred to Post-Phase Review");
    }
}
```

**Architecture Note**: Era.Core cannot reference engine layer. Engine creates service implementations that call GlobalStatic, registered via DI. Era.Core provides stub implementations as fallback.

### Test Naming Convention

Test methods follow `Test{FlagType}Equivalence` format:
- `TestFlagEquivalence` - FLAG.CSV vs FLAG.yaml data comparison
- `TestCFlagEquivalence` - CFLAG.csv vs CFLAG.yaml data comparison
- `TestTFlagEquivalence` - TFLAG.csv vs TFLAG.yaml data comparison
- `VariableDefinitionIntegration` - Engine integration flow for all flag types

This ensures AC filter patterns match correctly.

### Migration Dependency Requirements

**CRITICAL DEPENDENCY**: F528 (VariableSize.yaml) MUST be completed first:
- VariableSize.yaml defines array sizes used by FLAG/CFLAG/TFLAG definitions
- Without VariableSize data, flag array bounds cannot be validated
- F529 implementation must reference VariableSize array bounds for validation

**Phase 17 Priority**: HIGH priority due to dependency chain blocking effect on remaining variable definition files.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Prerequisite | F528 | VariableSize.yaml defines array sizes used by flag definitions |
| Prerequisite | tools/CsvToYaml | CSV to YAML conversion tool |
| Prerequisite | tools/YamlValidator | YAML schema validation |
| Prerequisite | tools/YamlSchemaGen | Schema generation for validation |
| Predecessor | F516 | Phase 17 Planning feature must be completed |
| Successor | F530 | Variable Definition CSVs migration Part 2 (remaining variable files) |
| Prerequisite | F538 | CsvToYaml tool creation (required for CSV→YAML conversion) |
| Indirect | F539 | SchemaValidator dependency (blocks F538, affects conversion tool availability) |

---

## Links

- [feature-528.md](feature-528.md) - Critical Config Files Migration (predecessor)
- [feature-516.md](feature-516.md) - Phase 17 Planning (parent feature)
- [feature-530.md](feature-530.md) - Variable Definition CSVs Migration Part 2 (successor)
- [feature-531.md](feature-531.md) - Character CSVs Migration (Character definition CSV dependencies)
- [feature-538.md](feature-538.md) - CsvToYaml Converter Tool (dependency)
- [feature-539.md](feature-539.md) - SchemaValidator dependency (blocks F538)
- [feature-540.md](feature-540.md) - Post-Phase Review Phase 17 (handoff destination)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 17 definition
- [index-features.md](index-features.md) - Feature index

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Part 2 variable files migration | Scope limitation - remaining variable definition CSV files not included | Feature | F530 (Variable Definition CSVs Migration Part 2) |
| Engine service implementation stubs | Era.Core cannot reference engine layer - actual implementations must be in engine project | Feature | F540 (Post-Phase Review will address engine integration pattern) |
| CsvToYaml tool dependency | If tool doesn't exist, manual conversion required | Feature | F538 (CsvToYaml tool creation feature) |

---

## Review Notes

**2026-01-19 FL iter0**: [pending_user] CsvToYaml tool dependency (F538) is [BLOCKED] by F539. F529 implementation requires F538 [DONE] first. Alternative: Manual CSV→YAML conversion for F529 (3 small files ~20KB total), document conversion process in Implementation Contract. User decision required: (A) Wait for F538, (B) Manual conversion, or (C) Create minimal converter.

**2026-01-19 FL iter0**: [resolved] Interface XML doc updated - added interface-level summary for IFlagLoader, ICFlagLoader, ITFlagLoader (ENGINE Issue 1).

**2026-01-19 FL iter0**: [resolved] Error message format template specified - added template string with LoaderName and SpecificError placeholders (ENGINE Issue 20).

**2026-01-19 FL iter0**: [resolved] AC#18 path scope expanded - added Era.Core/Data/Models/ to tech debt check paths (ENGINE Issue 24).

**2026-01-19 FL iter0**: [resolved] Model Structure section added - documented FlagConfig/CFlagConfig/TFlagConfig with CSV column mappings.

**2026-01-19 FL iter0**: [resolved] Schema generation workflow clarified - AC#1-3 Details now specify YamlSchemaGen usage and output location.

**2026-01-19 FL iter0**: [resolved] Stub message consistency - updated 'Phase 18' to 'Post-Phase Review' in Engine Integration Pattern.

**2026-01-19 FL iter0**: [resolved] Philosophy-Goal alignment - Goal item 3 now explicitly mentions magic number elimination.

**2026-01-19 FL iter0**: [resolved] AC#14-16 assertion criteria - specified 3 coverage areas: flag count, name preservation, default value equivalence.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | Created Feature 529 from F528 dependency chain, HIGH priority engine type | PROPOSED |
