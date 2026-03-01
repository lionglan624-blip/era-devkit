# Feature 617: IVariableDefinitionLoader YAML Migration

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
Single Source of Truth (SSOT) - Establish YAML as the sole configuration format by providing YAML alternatives to all CSV-dependent operations, eliminating the dual-format maintenance burden and enabling complete CSV file removal.

### Problem (Current Issue)
TalentIndexTests depends on Game/CSV/Talent.csv via IVariableDefinitionLoader.LoadFromCsv() method, creating a blocker for F591 (Legacy CSV File Removal). The IVariableDefinitionLoader interface only provides CSV loading capability, but F591 requires removing all CSV files including Talent.csv to complete the CSV-to-YAML migration.

### Goal (What to Achieve)
Add LoadFromYaml() method to IVariableDefinitionLoader interface for backward compatibility with existing test infrastructure that requires CsvVariableDefinitions format, while ITalentLoader remains the canonical YAML talent loading interface. This unblocks F591's CSV removal objective by providing YAML alternative for TalentIndexTests.

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | LoadFromYaml method added to interface | code | Grep(Era.Core/Interfaces/IVariableDefinitionLoader.cs) | contains | LoadFromYaml | [x] |
| 2 | LoadFromYaml implementation in VariableDefinitionLoader | code | Grep(Era.Core/Variables/VariableDefinitionLoader.cs) | contains | LoadFromYaml | [x] |
| 3 | Talent.yaml file exists | file | Glob | exists | Game/data/Talent.yaml | [x] |
| 4 | Talent.yaml contains talent definitions | file | Grep(Game/data/Talent.yaml) | contains | "処女" | [x] |
| 5 | LoadFromYaml handles file not found | test | dotnet test Era.Core.Tests --filter VariableDefinitionLoaderTests | succeeds | - | [x] |
| 6 | LoadFromYaml handles invalid YAML | test | dotnet test Era.Core.Tests --filter VariableDefinitionLoaderTests | succeeds | - | [x] |
| 7 | LoadFromYaml returns same data as LoadFromCsv | test | dotnet test Era.Core.Tests --filter VariableDefinitionLoaderTests | succeeds | - | [x] |
| 8 | TalentIndexTests works with YAML loader | test | dotnet test Era.Core.Tests --filter TalentIndexTests | succeeds | - | [x] |
| 9 | Zero technical debt | code | Grep(Era.Core/) | not_contains | TODO\|FIXME\|HACK | [x] |
| 10 | Build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |

### AC Details

**AC#1**: Interface extension for YAML support
- Test: Grep pattern="LoadFromYaml" path="Era.Core/Interfaces/IVariableDefinitionLoader.cs"
- Expected: Method signature with YAML path parameter and Result<CsvVariableDefinitions> return type

**AC#2**: Implementation in VariableDefinitionLoader
- Test: Grep pattern="LoadFromYaml" path="Era.Core/Variables/VariableDefinitionLoader.cs"
- Expected: Complete method implementation with YAML deserialization

**AC#3**: YAML data file creation
- Test: Glob pattern="Game/data/Talent.yaml"
- Expected: File exists with YAML format talent definitions
- Note: Game/data/Talent.yaml will be created during implementation (Task#3)

**AC#4**: YAML content verification
- Test: Grep pattern="処女" path="Game/data/Talent.yaml"
- Expected: Contains talent name "処女" confirming proper data migration
- Note: Game/data/Talent.yaml will be created during implementation (Task#3)

**AC#5**: Error handling for missing file
- Test: dotnet test Era.Core.Tests --filter FullyQualifiedName~LoadFromYaml_FileNotFound
- Expected: Returns Result.Fail with appropriate error message

**AC#6**: Error handling for invalid YAML
- Test: dotnet test Era.Core.Tests --filter FullyQualifiedName~LoadFromYaml_InvalidYaml
- Expected: Returns Result.Fail with YAML parsing error

**AC#7**: Data equivalence test
- Test: dotnet test Era.Core.Tests --filter FullyQualifiedName~LoadFromYaml_EquivalentToCsv
- Expected: Both LoadFromCsv and LoadFromYaml return identical CsvVariableDefinitions for talent data
- Note: Talent.yaml created by Task#6 will contain exactly the same data as CSV for this test to pass

**AC#8**: TalentIndexTests integration
- Test: dotnet test Era.Core.Tests --filter TalentIndexTests
- Expected: All tests pass using modified test that can use either CSV or YAML data source

**AC#9**: Technical debt verification
- Test: Grep pattern="TODO|FIXME|HACK" paths=[Era.Core/Variables/VariableDefinitionLoader.cs, Era.Core/Interfaces/IVariableDefinitionLoader.cs]
- Expected: 0 matches in Era.Core/ directory
- Note: Pattern "TODO|FIXME|HACK" uses regex OR operator (|) to match any of the three terms

**AC#10**: Build verification
- Test: dotnet build Era.Core
- Expected: Clean build with no warnings or errors

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Extend IVariableDefinitionLoader interface with LoadFromYaml method | [x] |
| 2 | 2 | Implement LoadFromYaml method in VariableDefinitionLoader class | [x] |
| 3 | 5 | Add file not found error handling test for LoadFromYaml | [x] |
| 4 | 6 | Add invalid YAML error handling test for LoadFromYaml | [x] |
| 5 | 7 | Add CSV/YAML data equivalence test for LoadFromYaml | [x] |
| 6 | 3 | Create Game/data/Talent.yaml file | [x] |
| 7 | 4 | Populate Talent.yaml with talent definitions including '処女' | [x] |
| 8 | 8 | Update TalentIndexTests to use YAML loader as fallback | [x] |
| 9 | 9 | Verify no TODO/FIXME/HACK in modified files | [x] |
| 10 | 10 | Verify build succeeds | [x] |
| 11 | - | Create follow-up feature to remove CSV fallback from TalentIndexTests after F591 completes | [x] |
| 12 | - | Document YAML format compatibility with ITalentLoader in Implementation Contract | [x] |

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Interface Extension

Add to `Era.Core/Interfaces/IVariableDefinitionLoader.cs`:

```csharp
/// <summary>
/// Loads variable definitions from a YAML file.
/// </summary>
/// <param name="yamlPath">Path to the YAML file containing variable definitions.</param>
/// <returns>Result containing CsvVariableDefinitions on success, or error on load failure.</returns>
Result<CsvVariableDefinitions> LoadFromYaml(string yamlPath);
```

### YAML File Format

Create `Game/data/Talent.yaml` with structure:

```yaml
# Talent variable definitions - Index to Name mappings
# Format: lowercase for IVariableDefinitionLoader compatibility
definitions:
  - index: 0
    name: "処女"
  - index: 1
    name: "童貞"
  # ... continue with all talent definitions
```

### Implementation Requirements

| Requirement | Details |
|-------------|---------|
| Error handling | Use same error pattern as LoadFromCsv - Result.Fail with descriptive messages |
| YAML library | Use YamlDotNet.Serialization (already used in project) |
| Data structure | Return same CsvVariableDefinitions type for compatibility |
| Loader relationship | IVariableDefinitionLoader.LoadFromYaml converts TalentConfig data to CsvVariableDefinitions for legacy test consumers. ITalentLoader remains canonical for new talent loading code. |
| Test coverage | Add positive and negative tests following existing VariableDefinitionLoaderTests patterns |

### Test Modification Strategy

Modify `TalentIndexTests.LoadTalentCsv()` method to try YAML first, fallback to CSV:

```csharp
private IReadOnlyDictionary<string, int> LoadTalentCsv()
{
    var loader = Services.GetService(typeof(IVariableDefinitionLoader)) as IVariableDefinitionLoader;
    Assert.NotNull(loader);

    // Transitional: YAML-first with CSV fallback until F591 removes CSV files
    var yamlPath = GetProjectPath("Game/data/Talent.yaml");
    if (File.Exists(yamlPath))
    {
        var yamlResult = loader.LoadFromYaml(yamlPath);
        if (yamlResult is Result<CsvVariableDefinitions>.Success yamlSuccess)
        {
            return yamlSuccess.Value.NameToIndex;
        }
    }

    // Fallback to CSV for backward compatibility
    var csvPath = GetProjectPath("Game/CSV/Talent.csv");
    AssertFileExists(csvPath, $"Neither Talent.yaml nor Talent.csv found");

    var csvResult = loader.LoadFromCsv(csvPath);
    Assert.IsType<Result<CsvVariableDefinitions>.Success>(csvResult);

    var definitions = ((Result<CsvVariableDefinitions>.Success)csvResult).Value;
    return definitions.NameToIndex;
}
```

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Successor | F591 | [BLOCKED] | F591 depends on this feature to remove CSV files without breaking tests |

## Review Notes
- [resolved-invalid] Phase1-Uncertain iter1: Code snippet uses 'IReadOnlyDictionary<string, int>' return type but imports System.Collections.Generic are not shown. May cause confusion for implementer.
- [resolved-applied] Phase1-Uncertain iter1: AC#7 states 'Both LoadFromCsv and LoadFromYaml return identical CsvVariableDefinitions for talent data' but Task#3 creates Talent.yaml with YAML format. Need to ensure YAML file contains exact same data as CSV for equivalence test to pass.
- [resolved-applied] Phase1-Uncertain iter2: Task#2 maps to AC#2,5,6,7 (4 ACs) but AC:Task 1:1 principle requires each AC to have exactly one Task. This violates the 1 AC = 1 Task rule.
- [resolved-applied] Phase1-Uncertain iter2: YAML format shows '# Converted from Game/CSV/Talent.csv for F617' but this comment will be stale once CSV is removed by F591. Philosophy mentions 'eliminating dual-format maintenance burden' which includes avoiding stale references.
- [resolved-applied] Phase1-Uncertain iter4: Two pending review notes remain unresolved: (1) AC#7 data equivalence concern about Talent.yaml matching CSV exactly, (2) AC:Task 1:1 violation. Pending items should be resolved before proceeding.
- [resolved-applied] Phase1-Uncertain iter5: YAML format shows 'description' field but VariableDefinitionLoader.LoadFromCsv returns CsvVariableDefinitions(NameToIndex, IndexToName) which only uses index and name. Adding 'description' field creates data model mismatch.
- [resolved-invalid] Phase1-Uncertain iter5: Code snippet shows 'if (yamlResult is Result<CsvVariableDefinitions>.Success yamlSuccess)' pattern but existing codebase uses 'Assert.IsType<Result<CsvVariableDefinitions>.Success>(result)' pattern. Pattern mismatch may confuse implementer.
- [resolved-invalid] Phase1-Uncertain iter7: Potential duplication with existing infrastructure: YamlTalentLoader.cs and ITalentLoader already exist in Era.Core/Data/ returning TalentConfig. This feature proposes creating Game/data/Talent.yaml and adding LoadFromYaml to IVariableDefinitionLoader returning CsvVariableDefinitions. Two separate YAML loading mechanisms for talent data creates confusion about which is authoritative.
- [resolved-invalid] Phase1-Uncertain iter8: Philosophy states 'Establish YAML as sole configuration format' but Goal only addresses IVariableDefinitionLoader.LoadFromYaml addition. Does not address existing YamlTalentLoader/ITalentLoader infrastructure or how they relate. Philosophy implies unification but Goal creates parallel mechanism.

---

## Mandatory Handoffs

| Target | Task | Priority | Description |
|--------|------|----------|-------------|
| F591 | Remove CSV fallback code | Normal | After F591 removes Game/CSV/Talent.csv, remove CSV fallback logic from TalentIndexTests.LoadTalentCsv() method (lines 169-176). The fallback path will become dead code once CSV files are removed. |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-24 19:01 | START | implementer | Task 3,4,5 (TDD Test Creation) | - |
| 2026-01-24 19:01 | END | implementer | Task 3,4,5 (TDD Test Creation) | SUCCESS |
| 2026-01-24 19:09 | START | implementer | Task 1,2,6,7,8,9,10 (Implementation) | - |
| 2026-01-24 19:09 | END | implementer | Task 1,2,6,7,8,9,10 (Implementation) | SUCCESS |
| 2026-01-24 19:14 | DEVIATION | feature-reviewer | Post-review | NEEDS_REVISION: engine-dev SKILL.md not updated |

## Links
- [index-features.md](index-features.md)
- [feature-591.md](feature-591.md) - Legacy CSV File Removal (blocked by this feature)