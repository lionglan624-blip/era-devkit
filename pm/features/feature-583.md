# Feature 583: Complete CSV Elimination (Remaining File Types)

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
Single Source of Truth (SSOT) - Establish YAML as the unified configuration format across Era.Core, eliminating CSV dependency beyond VariableSize/GameBase to enable complete modernization of game data management and improved developer experience with type-safe configuration loading.

**Scope**: F583 creates loader infrastructure only. Engine integration and actual CSV file elimination handled by F591.

### Problem (Current Issue)
Game/CSV contains 44 CSV files of which ~20+ file types lack YAML loader implementations. Current YAML conversion coverage is limited to VariableSize, GameBase, and COM definitions, leaving data types like Talent, Abl, Base, CFLAG, CSTR, Equip, Item, Juel, Mark, Palam, Stain, Str, TCVAR, TFLAG, Train, TSTR, and others dependent on legacy CSV parsing.

### Goal (What to Achieve)
Create comprehensive YAML loader infrastructure for all remaining CSV file types, establishing Era.Core as the complete alternative to engine CSV loading and enabling future CSV elimination beyond the baseline VariableSize/GameBase implementations.

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Interface files created | file | Glob(Era.Core/Data/I*Loader.cs) | count_equals | 26 | [x] |
| 2 | YAML loader implementations | file | Glob(Era.Core/Data/Yaml*Loader.cs) | count_equals | 26 | [x] |
| 3 | Data model classes defined | file | Glob(Era.Core/Data/Models/*Config.cs) | count_equals | 25 | [x] |
| 4 | Talent loader interface | file | Glob | exists | Era.Core/Data/ITalentLoader.cs | [x] |
| 5 | Talent YAML loader | file | Glob | exists | Era.Core/Data/YamlTalentLoader.cs | [x] |
| 6 | Talent data model | file | Glob | exists | Era.Core/Data/Models/TalentConfig.cs | [x] |
| 7 | Abl loader interface | file | Glob | exists | Era.Core/Data/IAblLoader.cs | [x] |
| 8 | Abl YAML loader | file | Glob | exists | Era.Core/Data/YamlAblLoader.cs | [x] |
| 9 | Abl data model | file | Glob | exists | Era.Core/Data/Models/AblConfig.cs | [x] |
| 10 | DI registration complete | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | count_equals | 26 | [x] |
| 11 | Error handling consistency | code | Grep(Era.Core/Data/Yaml*Loader.cs) | count_equals | 26 | [x] |
| 12 | Unit tests created | file | Glob(Era.Core.Tests/Data/*LoaderTests.cs) | count_equals | 23 | [x] |
| 13 | TalentConfig has Index property | code | Grep(Era.Core/Data/Models/TalentConfig.cs) | matches | public int Index \\{ get; init; \\} | [x] |
| 14 | TalentConfig has Name property | code | Grep(Era.Core/Data/Models/TalentConfig.cs) | matches | public string Name \\{ get; init; \\} | [x] |
| 15 | Zero technical debt | code | Grep(Era.Core/Data/) | not_contains | TODO\|FIXME\|HACK | [x] |

### AC Details

**AC#1-3**: Comprehensive loader infrastructure
- Test: Count interface files (I*Loader.cs), implementation files (Yaml*Loader.cs), model files (*Config.cs)
- Expected: Total counts including pre-existing: AC#1=26 (23 new + 3 existing), AC#2=26 (23 new + 3 existing), AC#3=25 (23 new + 2 existing: VariableSizeConfig, GameBaseConfig; ComDefinition is not in Models/)
- Note: Excludes Chara*.csv (F589)

**AC#4-6**: Talent loader example (pattern for other types)
- Test: Verify specific Talent implementation files exist
- Expected: Complete interface + implementation + model for Talent CSV type

**AC#7-9**: Abl loader example (validate pattern replication)
- Test: Verify Abl implementation follows same pattern as Talent
- Expected: Complete interface + implementation + model for Abl CSV type

**AC#10**: DI registration verification
- Test: Grep pattern="AddSingleton.*I.*Loader.*Yaml.*Loader" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs" | count
- Expected: 26 DI registrations (23 new + 3 existing: VariableSize, GameBase, Com)

**AC#11**: Error handling consistency across loaders
- Test: Grep pattern="Invalid YAML format:" path="Era.Core/Data/Yaml*Loader.cs" | count
- Expected: 26 loaders (23 new + 3 existing) all use consistent error message "Invalid YAML format:" in YamlException catch block

**AC#12**: Unit test coverage
- Test: Count test files matching pattern "*LoaderTests.cs" in Era.Core.Tests/Data/
- Expected: 23 unit test files, one for each loader implementation
- Requirements: Each test file must include positive test (valid YAML loads successfully) and negative test (invalid path/format returns Result.Fail)

**AC#13-14**: TalentConfig model structure verification
- Test: Verify TalentConfig model has Index and Name properties matching CSV structure
- Expected: Properties match Talent.csv column structure (index, name, description)

**AC#15**: Technical debt verification
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Data/" type=cs | count
- Expected: 0 matches (no technical debt in loader implementations)

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create 23 loader interfaces following IVariableSizeLoader pattern (batch task: scaffolding) | [x] |
| 2 | 2,10 | Implement 23 YAML loader classes with DI registration (batch: boilerplate, AC#2+10 interdependent, requires Task#1 completion) | [x] |
| 3 | 3 | Create 23 data model classes following VariableSizeConfig pattern (batch task: scaffolding) | [x] |
| 4 | 4,5,6 | Implement Talent loader (interface + YAML loader + model) - example pattern (batch waiver: AC#4,5,6 verify single Talent implementation as demonstration) | [x] |
| 5 | 7,8,9 | Implement Abl loader (interface + YAML loader + model) - pattern validation (batch waiver: AC#7,8,9 verify single Abl implementation as validation) | [x] |
| 6 | 11,15 | Standardize error handling and eliminate technical debt | [x] |
| 7 | 12 | Create comprehensive unit tests for all loaders | [x] |
| 8 | 13,14 | Verify data model property mapping consistency | [x] |

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### CSV File Type Analysis

Based on Game/CSV directory scan, target file types for YAML loader creation:

| CSV File | Interface | YAML Loader | Data Model | Notes |
|----------|-----------|-------------|------------|-------|
| Talent.csv | ITalentLoader | YamlTalentLoader | TalentConfig | Index, Name, Description |
| Abl.csv | IAblLoader | YamlAblLoader | AblConfig | Ability definitions |
| Base.csv | IBaseLoader | YamlBaseLoader | BaseConfig | Base stats |
| CFLAG.csv | ICFlagLoader | YamlCFlagLoader | CFlagConfig | Character flags |
| CSTR.csv | ICStrLoader | YamlCStrLoader | CStrConfig | Character strings |
| Equip.csv | IEquipLoader | YamlEquipLoader | EquipConfig | Equipment data |
| ex.csv | IExLoader | YamlExLoader | ExConfig | Extended data |
| exp.csv | IExpLoader | YamlExpLoader | ExpConfig | Experience data |
| Item.csv | IItemLoader | YamlItemLoader | ItemConfig | Item definitions |
| Juel.csv | IJuelLoader | YamlJuelLoader | JuelConfig | Juel/jewelry data |
| Mark.csv | IMarkLoader | YamlMarkLoader | MarkConfig | Mark definitions |
| Palam.csv | IPalamLoader | YamlPalamLoader | PalamConfig | Parameter data |
| source.csv | ISourceLoader | YamlSourceLoader | SourceConfig | Source definitions |
| Stain.csv | IStainLoader | YamlStainLoader | StainConfig | Stain data |
| Str.csv | IStrLoader | YamlStrLoader | StrConfig | String definitions |
| TCVAR.csv | ITCVarLoader | YamlTCVarLoader | TCVarConfig | Temporary character variables |
| TFLAG.csv | ITFlagLoader | YamlTFlagLoader | TFlagConfig | Temporary flags |
| Train.csv | ITrainLoader | YamlTrainLoader | TrainConfig | Training data |
| TSTR.csv | ITStrLoader | YamlTStrLoader | TStrConfig | Temporary strings |
| _Rename.csv | IRenameLoader | YamlRenameLoader | RenameConfig | Rename operations |
| Tequip.csv | ITequipLoader | YamlTequipLoader | TequipConfig | Equipment templates |
| _Replace.csv | IReplaceLoader | YamlReplaceLoader | ReplaceConfig | Replacement operations |
| FLAG.CSV | IFlagLoader | YamlFlagLoader | FlagConfig | Game flags |

**Total**: 23 CSV types requiring YAML loader infrastructure

### Implementation Pattern

**Naming Convention**: Loader naming follows CSV file name with proper casing: ex.csv → IExLoader, _Rename.csv → IRenameLoader

**Design Decision**: 23 separate loader classes chosen over generic YamlLoader<T> pattern for:
1. Type-safe configuration handling (each CSV has different structure/columns)
2. Independent evolution (future loader-specific features without affecting others)
3. Clear file-level encapsulation for debugging and maintenance
4. Consistency with existing VariableSizeLoader/GameBaseLoader pattern

Follow YamlVariableSizeLoader pattern for all implementations:

```csharp
// Era.Core/Data/I{Type}Loader.cs
using Era.Core.Data.Models;
using Era.Core.Types;

namespace Era.Core.Data;

public interface I{Type}Loader
{
    Result<{Type}Config> Load(string path);
}

// Era.Core/Data/Yaml{Type}Loader.cs
using Era.Core.Data.Models;
using Era.Core.Types;
using YamlDotNet.Serialization;

namespace Era.Core.Data;

public class Yaml{Type}Loader : I{Type}Loader
{
    public Result<{Type}Config> Load(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return Result<{Type}Config>.Fail("Yaml{Type}Loader: Path cannot be null or empty");
            }

            if (!File.Exists(path))
            {
                return Result<{Type}Config>.Fail($"Yaml{Type}Loader: File not found at path: {path}");
            }

            var yamlContent = File.ReadAllText(path);
            var deserializer = new DeserializerBuilder().Build();
            var config = deserializer.Deserialize<{Type}Config>(yamlContent);

            return Result<{Type}Config>.Ok(config);
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            return Result<{Type}Config>.Fail($"Yaml{Type}Loader: Invalid YAML format: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<{Type}Config>.Fail($"Yaml{Type}Loader: {ex.Message}");
        }
    }
}

// Era.Core/Data/Models/{Type}Config.cs
namespace Era.Core.Data.Models;

public class {Type}Config
{
    // Properties based on CSV structure analysis
    public int Index { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
```

### DI Registration

Register all loaders in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// Data loaders
services.AddSingleton<ITalentLoader, YamlTalentLoader>();
services.AddSingleton<IAblLoader, YamlAblLoader>();
// ... repeat for all 23 types
```

### Unit Test Pattern

**Rationale**: Separate test files chosen for:
1. Independent failure isolation - single loader failures don't affect other tests
2. Clear file-level coverage visibility per CSV type
3. Consistency with existing VariableSizeLoaderTests pattern

Create test files in `Era.Core.Tests/Data/` following existing pattern:

```csharp
// Era.Core.Tests/Data/{Type}LoaderTests.cs
using Era.Core.Data;
using Era.Core.Data.Models;
using Xunit;

namespace Era.Core.Tests.Data;

public class {Type}LoaderTests
{
    [Fact]
    public void Load_ValidYamlFile_ReturnsSuccess()
    {
        // Test implementation
    }

    [Fact]
    public void Load_InvalidPath_ReturnsFailure()
    {
        // Test implementation
    }

    [Fact]
    public void Load_InvalidYaml_ReturnsFailure()
    {
        // Test implementation
    }
}
```

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F558 | [DONE] | Engine Integration Services for Critical Config |
| Related | F575 | [DONE] | CSV Complete Elimination - complementary feature |
| Related | F572 | [DONE] | COM YAML Rapid Iteration Tooling - provides YAML deserialization patterns used in loader implementations |

## Review Notes
- [applied] Phase1-Uncertain iter2: AC#11 design suggestion - changed from 'contains' to 'count_equals' matcher for error handling verification across all 22 loaders
- [skipped] Phase1-Uncertain iter2: Unit test pattern stub implementation - accepted as adequate template guidance for implementer
- [applied] Phase1-Uncertain iter4: AC#1-3 glob pattern path specification - will add path to Method column per issue #5
- [resolved] Phase1-Uncertain iter4: Review Notes resolution convention - established precedent for marking status before implementation
- [applied] Phase1-Uncertain iter5: Philosophy coverage gap - clarified scope in Philosophy section that F583 creates loader infrastructure only, F591 handles engine integration
- [applied] Phase1-Uncertain iter5: Extensibility concern - added explicit design decision for 23 separate loaders with rationale per Zero Debt Upfront principle
- [applied] Phase1-Uncertain iter6: AC Details accuracy - updated AC#11 description to specify "YamlException catch block" for precision
- [applied] Phase1-Uncertain iter6: Test design tradeoff - added rationale for separate test files approach to Unit Test Pattern section
- [applied] Phase1-Uncertain iter8: Method column format - AC#1-3 'Glob(path)' format accepted per feature-575 precedent and feature-584 standardization
- [applied] Phase1-Uncertain iter9: AC#13-14 regex specificity - updated patterns to precise format 'public int Index \\{ get; init; \\}'
- [applied] Phase1-Uncertain iter9: Task sequencing documentation - added Task#1 dependency to Task#2 description
- [applied] Phase1-Uncertain iter10: F572 dependency relevance - clarified YAML deserialization patterns value in Dependencies table
- [applied] Phase2-Maintainability: CSV count discrepancy - updated Problem section from 43 to 44 CSV files
- [resolved] Phase2-Maintainability: FLAG.CSV inclusion ambiguity - confirmed FLAG.CSV correctly included in 23 types scope
- [skipped] Phase2-Maintainability: Task#8 duplication concern - kept as independent post-implementation validation step
- [applied] Phase2-Maintainability: AC#11 pattern precision - updated AC Details to match pattern with colon
- [applied] Phase3-ACValidation: AC#12 test coverage - added positive/negative test requirements to AC Details
- [resolved] Phase3-ACValidation: Review Notes pending resolution - all items resolved via user decisions

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| Character CSV files (Chara*.csv) excluded | Individual character files require separate analysis and potentially different loader pattern | Feature | F589 |
| YAML schema validation not included | YAML schema generation and validation tools separate concern | Feature | F590 |
| Legacy CSV removal not included | Actual CSV file removal requires engine compatibility verification | Feature | F591 |

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-21 21:45 | START | implementer | Task 1 | - |
| 2026-01-21 21:45 | END | implementer | Task 1 | SUCCESS |
| 2026-01-21 21:45 | START | implementer | Task 2 | - |
| 2026-01-21 21:45 | END | implementer | Task 2 | SUCCESS |
| 2026-01-21 21:45 | START | implementer | Task 3 | - |
| 2026-01-21 21:45 | END | implementer | Task 3 | SUCCESS |
| 2026-01-21 21:56 | START | implementer | Task 4-8 | - |
| 2026-01-21 21:56 | END | implementer | Task 4-8 | SUCCESS |
| 2026-01-21 22:15 | DEVIATION | feature-reviewer | post review | AC#1-3 glob includes pre-existing files |

## Links
[index-features.md](index-features.md)
[feature-575.md](feature-575.md)
[feature-572.md](feature-572.md)
[feature-558.md](feature-558.md)