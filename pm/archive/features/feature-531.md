# Feature 531: Config Files Migration - Phase 17

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

**Phase 17: Data Migration** - Establish YAML as the single source of truth for UI rendering settings (_default.config: 14 properties), engine behavior flags (_fixed.config: 6 properties), and comprehensive game configuration (emuera.config: 67 properties), replacing legacy Shift-JIS config format with type-safe YAML models that enable schema validation and eliminate encoding issues. This migration ensures type safety through strongly typed data models and consistent data access via IDataLoader interface pattern while maintaining exact behavioral equivalence with legacy configuration.

### Problem (Current Issue)

**Config Files Need Migration**: Three core configuration files (_default.config, _fixed.config, emuera.config) contain essential game settings and rendering parameters that need to be migrated to YAML format:
- _default.config: Contains UI rendering settings (window size, font, colors) - 14 key-value pairs
- _fixed.config: Contains engine behavior flags (case sensitivity, file search) - 6 key-value pairs
- emuera.config: Contains comprehensive game configuration (67 settings including debug, save, input)
- Current config format lacks type safety and schema validation
- Configuration loading is hardcoded without proper abstraction layer

### Goal (What to Achieve)

1. **Migrate _default.config → DefaultConfig.yaml** with IDefaultConfigLoader interface
2. **Migrate _fixed.config → FixedConfig.yaml** with IFixedConfigLoader interface
3. **Migrate emuera.config → EmueraConfig.yaml** with IEmueraConfigLoader interface
4. **Implement Phase 4 design compliance** (specific loader interfaces per F528 pattern, DI registration, Strongly Typed data models)
5. **Verify 100% behavioral equivalence** via integration tests (87 total properties: 14+6+67)
6. **Remove technical debt** from target implementation files

### Source Migration Reference

**Legacy Locations** (verified 2026-01-19):

| Source File | Location | Properties | Encoding |
|-------------|----------|:----------:|----------|
| _default.config | Game/CSV/_default.config | 14 | UTF-8 with BOM |
| _fixed.config | Game/CSV/_fixed.config | 6 | UTF-8 with BOM |
| emuera.config | Game/emuera.config | 67 | Shift-JIS |

**Note**: emuera.config uses Shift-JIS encoding. Migration parser MUST specify Shift-JIS encoding when reading to preserve Japanese property names.

---

## Dependencies

| Feature | Relationship | Description |
|---------|-------------|-------------|
| F528 | Predecessor | Critical Config Files Migration (VariableSize.csv, GameBase.csv) must complete first |
| F516 | Parent | Phase 17 Planning - this is a sub-feature |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | DefaultConfig.yaml file created | file | Glob | exists | Game/config/DefaultConfig.yaml | [ ] |
| 2 | FixedConfig.yaml file created | file | Glob | exists | Game/config/FixedConfig.yaml | [ ] |
| 3 | EmueraConfig.yaml file created | file | Glob | exists | Game/config/EmueraConfig.yaml | [ ] |
| 4 | YamlDefaultConfigLoader implementation exists | code | Grep | contains | "class.*YamlDefaultConfigLoader.*IDefaultConfigLoader" | [ ] |
| 5 | YamlFixedConfigLoader implementation exists | code | Grep | contains | "class.*YamlFixedConfigLoader.*IFixedConfigLoader" | [ ] |
| 6 | YamlEmueraConfigLoader implementation exists | code | Grep | contains | "class.*YamlEmueraConfigLoader.*IEmueraConfigLoader" | [ ] |
| 7 | DefaultConfigModel class exists | code | Grep | contains | "class.*DefaultConfigModel" | [ ] |
| 8 | FixedConfigModel class exists | code | Grep | contains | "class.*FixedConfigModel" | [ ] |
| 9 | EmueraConfigModel class exists | code | Grep | contains | "class.*EmueraConfigModel" | [ ] |
| 10 | DI registration for config loaders | code | Grep | count_equals | "AddSingleton.*I(Default\|Fixed\|Emuera)ConfigLoader" (3) | [ ] |
| 11 | Equivalence tests pass (Pos) | test | Bash | succeeds | - | [ ] |
| 12 | Error handling tests pass (Neg) | test | Bash | succeeds | - | [ ] |
| 13 | Build succeeds | build | Bash | succeeds | "dotnet build" | [ ] |
| 14 | Zero technical debt | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [ ] |

### AC Details

**AC#1-3**: YAML configuration files created
- Test: Glob pattern for each YAML file in Game/config/
- Expected: Files exist with proper YAML structure
- Note: Files contain equivalent data to original config files with proper typing
- Output directory matches F528 precedent (Game/config/)

**AC#4-6**: Config loader implementation classes exist
- Test: Grep pattern for loader class definitions in Era.Core/Data/
- Expected: Classes implement specific loader interface pattern per F528 precedent
- Pattern: "class.*YamlDefaultConfigLoader.*IDefaultConfigLoader" matches "public class YamlDefaultConfigLoader : IDefaultConfigLoader"
- Naming follows F528 convention: Yaml prefix + config type + Loader (e.g., YamlDefaultConfigLoader)
- Loaders include Load(string path) method returning Result&lt;T&gt;

**AC#7-9**: Strongly typed data models
- Test: Grep pattern for model classes in Era.Core/Data/Models/
- Expected: Models contain all properties from original config files (14+6+67=87 total)
- Properties have correct C# types (bool, int, string, Color)
- All properties have XML doc comments

**AC#10**: DI registration verification
- Test: Grep pattern in Era.Core/DependencyInjection/ServiceCollectionExtensions.cs with count_equals matcher
- Expected: Exactly 3 matches for specific config loader interfaces
- Pattern: "AddSingleton.*I(Default\|Fixed\|Emuera)ConfigLoader"
- Matches: IDefaultConfigLoader, IFixedConfigLoader, IEmueraConfigLoader registrations

**AC#11**: Behavioral equivalence tests (Positive)
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~ConfigEquivalence`
- Expected: 100% equivalence for all 87 config values (14+6+67)
- Tests verify: (1) Legacy parser with correct encoding, (2) YAML loader, (3) 1:1 property match
- Minimum 3 specific assertions per config file (e.g., window width=940, case-insensitive=YES, debug=NO)

**AC#12**: Error handling tests (Negative)
- Test: `dotnet test Era.Core.Tests --filter FullyQualifiedName~ConfigErrorHandling`
- Expected: Tests pass, error messages follow format from Implementation Contract
- Tests verify: (1) Invalid YAML format → "Configuration file format error", (2) Missing file → "Configuration file not found", (3) Type conversion error → "Invalid configuration value type"
- Minimum 3 error cases per loader type

**AC#13**: Build verification
- Test: Run dotnet build to verify compilation
- Expected: No build errors, successful compilation

**AC#14**: Technical debt cleanup
- Test: Grep for TODO/FIXME/HACK in all feature-created files
- Paths: Era.Core/Data/, Era.Core/Data/Models/, Game/config/
- Expected: 0 matches across all feature-created files
- Ensures clean implementation without deferred issues

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | Create YAML configuration files from legacy config files | [ ] |
| 2 | 4,5,6 | Implement config loader interfaces and classes | [ ] |
| 3 | 7,8,9 | Implement strongly typed data models | [ ] |
| 4 | 10 | Register config loaders in DI container | [ ] |
| 5 | 11,12 | Create and verify equivalence tests (positive) and error handling tests (negative) | [ ] |
| 6 | 13,14 | Verify build and cleanup technical debt | [ ] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### File Structure

| Target File | Description |
|-------------|-------------|
| `Game/config/DefaultConfig.yaml` | _default.config equivalent with typed YAML (14 properties) |
| `Game/config/FixedConfig.yaml` | _fixed.config equivalent with typed YAML (6 properties) |
| `Game/config/EmueraConfig.yaml` | emuera.config equivalent with typed YAML (67 properties) |
| `Era.Core/Data/IDefaultConfigLoader.cs` | DefaultConfig loader interface |
| `Era.Core/Data/YamlDefaultConfigLoader.cs` | DefaultConfig loader implementing IDefaultConfigLoader |
| `Era.Core/Data/IFixedConfigLoader.cs` | FixedConfig loader interface |
| `Era.Core/Data/YamlFixedConfigLoader.cs` | FixedConfig loader implementing IFixedConfigLoader |
| `Era.Core/Data/IEmueraConfigLoader.cs` | EmueraConfig loader interface |
| `Era.Core/Data/YamlEmueraConfigLoader.cs` | EmueraConfig loader implementing IEmueraConfigLoader |
| `Era.Core/Data/Models/DefaultConfigModel.cs` | DefaultConfig strongly typed model (14 properties) |
| `Era.Core/Data/Models/FixedConfigModel.cs` | FixedConfig strongly typed model (6 properties) |
| `Era.Core/Data/Models/EmueraConfigModel.cs` | EmueraConfig strongly typed model (67 properties) |
| `Era.Core.Tests/Data/ConfigEquivalenceTests.cs` | Config migration equivalence tests |
| `Era.Core.Tests/Data/ConfigErrorHandlingTests.cs` | Config loader error handling tests |

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IDefaultConfigLoader, YamlDefaultConfigLoader>();
services.AddSingleton<IFixedConfigLoader, YamlFixedConfigLoader>();
services.AddSingleton<IEmueraConfigLoader, YamlEmueraConfigLoader>();
```

**Note**: Uses specific loader interfaces following F528 precedent (IVariableSizeLoader, IGameBaseLoader pattern).

### Data Equivalence Requirements

**Critical**: Each YAML file must contain exactly the same configuration values as its legacy counterpart:
- _default.config: 14 properties (ウィンドウ幅, ウィンドウ高さ, etc.)
- _fixed.config: 6 properties (大文字小文字の違いを無視する, etc.)
- emuera.config: 67 properties (complete engine configuration)

### Equivalence Verification Requirements

**Test Methodology** (AC#11):
1. **Parse legacy config files** with correct encoding (UTF-8 with BOM for _default/_fixed, Shift-JIS for emuera)
2. **Load YAML equivalents** via IDataLoader implementations
3. **Assert 1:1 property match** for all 87 values (14+6+67)

**Minimum Test Cases** (3+ per config file):
- DefaultConfig: Window width = 940, Window height = 480, Font name = "ＭＳ ゴシック"
- FixedConfig: Case insensitive = YES, Use subdirectories = YES, Sort by filename = YES
- EmueraConfig: Use mouse = YES, Debug commands = NO, Auto-save = YES

### Encoding Handling

**CRITICAL**: emuera.config uses Shift-JIS encoding.

- _default.config: UTF-8 with BOM (verified readable)
- _fixed.config: UTF-8 with BOM (verified readable)
- emuera.config: Shift-JIS (migration parser MUST specify encoding to preserve Japanese property names)

### Documentation Requirements

**All interface members and model properties MUST have XML doc comments**:
- Interface members: Describe purpose and return type
- Model properties: Describe purpose, data type constraints, and default values where applicable

### Error Handling Pattern

Use Result&lt;T&gt; pattern from Phase 4 design with F528 error message format:

```
{LoaderName}: {SpecificError}
```

Examples:
- Invalid YAML → Result&lt;T&gt;.Fail("YamlDefaultConfigLoader: Invalid YAML format at line {line}")
- Missing file → Result&lt;T&gt;.Fail("YamlDefaultConfigLoader: File not found at path: {path}")
- Type conversion error → Result&lt;T&gt;.Fail("YamlDefaultConfigLoader: Invalid value type for property {name}")

### Test Naming Convention

Test methods follow `Test{ConfigType}{TestType}` format to ensure AC filter patterns match correctly:
- `TestDefaultConfigEquivalence`, `TestFixedConfigEquivalence`, `TestEmueraConfigEquivalence`
- `TestDefaultConfigErrorHandling`, `TestFixedConfigErrorHandling`, `TestEmueraConfigErrorHandling`

### Interface Design

```csharp
using Era.Core.Types;

namespace Era.Core.Data;

/// <summary>
/// Loads default configuration settings from YAML format.
/// </summary>
public interface IDefaultConfigLoader
{
    /// <summary>
    /// Loads the default configuration from the specified path.
    /// </summary>
    /// <param name="path">Path to the YAML configuration file.</param>
    /// <returns>Result containing the configuration model or error.</returns>
    Result<DefaultConfigModel> Load(string path);
}

/// <summary>
/// Loads fixed configuration settings from YAML format.
/// </summary>
public interface IFixedConfigLoader
{
    /// <summary>
    /// Loads the fixed configuration from the specified path.
    /// </summary>
    /// <param name="path">Path to the YAML configuration file.</param>
    /// <returns>Result containing the configuration model or error.</returns>
    Result<FixedConfigModel> Load(string path);
}

/// <summary>
/// Loads emuera configuration settings from YAML format.
/// </summary>
public interface IEmueraConfigLoader
{
    /// <summary>
    /// Loads the emuera configuration from the specified path.
    /// </summary>
    /// <param name="path">Path to the YAML configuration file.</param>
    /// <returns>Result containing the configuration model or error.</returns>
    Result<EmueraConfigModel> Load(string path);
}
```

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Legacy config file cleanup | Original files should remain during transition period | Feature | F540 |

---

## Links

- [feature-516.md](feature-516.md) - Phase 17 Planning (Parent)
- [feature-528.md](feature-528.md) - Critical Config Files Migration (Predecessor)
- [feature-540.md](feature-540.md) - Legacy Config File Cleanup (Handoff)
- [index-features.md](index-features.md) - Feature Index