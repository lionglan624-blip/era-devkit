# Feature 532: Character Data Migration Part 1 - Phase 17, Chara0-Chara13

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

**Phase 17: Data Migration** - Establish YAML/JSON as the single source of truth for all ERA character configuration data, ensuring type safety through strongly typed data models and consistent data access via ICharacterDataLoader interface pattern. This migration eliminates magic number dependencies and enables automated schema validation while maintaining exact behavioral equivalence with CSV-based legacy character data across the complete character roster.

### Problem (Current Issue)

**Core Character Data Migration Dependency**: The player character (Chara0) and main Touhou characters (Chara1-Chara13) represent the foundational character definitions that ALL other game systems depend on:
- Player character (Chara0.csv) contains base template for character attributes and initialization
- Main characters (Chara1-13: Reimu, Marisa, Sakuya, Meiling, Patchouli, Koakuma, Remilia, Flandre, Alice, Yukari, Yuyuko, Youmu, Ran) define core game characters
- Character data includes attributes, stats, relationships, and game state initialization
- CSV parsing is fragile, lacks type safety, and prevents automated validation
- Current GlobalStatic direct access pattern lacks abstraction for unit testing
- Technical debt accumulation prevents quality assurance automation
- Remaining 180+ characters cannot be migrated until core character infrastructure is established

### Goal (What to Achieve)

1. **Migrate Chara0-Chara13 CSVs → YAML format** (14 files: player + main characters)
2. **Implement ICharacterDataLoader interface** following Phase 4 design compliance
3. **Create CharacterData strongly typed model** with proper validation and error handling
4. **Establish DI registration pattern** consistent with F528-F530 precedent
5. **Verify 100% behavioral equivalence** via integration tests comparing CSV vs YAML data loading
6. **Achieve zero technical debt** across all character data implementation files
7. **Establish migration precedent** for remaining character files (Chara14+) in future features

---

## Dependencies

| Dependency | Type | Reason | Status |
|------------|------|--------|:------:|
| F528 | predecessor | VariableSize.yaml provides array size constants for character attributes | [PROPOSED] |
| F529 | predecessor | Variable definition YAMLs provide attribute type definitions | [PROPOSED] |
| F530 | predecessor | Complete variable definition migration provides character attribute schemas | [PROPOSED] |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Character YAML files created | file | Glob | count_equals | "Game/Data/Characters/Chara*.yaml" (14) | [ ] |
| 2 | ICharacterDataLoader interface exists | code | Grep | contains | "interface ICharacterDataLoader" | [ ] |
| 3 | CharacterData model exists | code | Grep | contains | "class CharacterData" | [ ] |
| 4 | Character data loader implementation | code | Grep | contains | "class CharacterDataLoader.*ICharacterDataLoader" | [ ] |
| 5 | DI registration exists | code | Grep | contains | "AddSingleton.*ICharacterDataLoader.*CharacterDataLoader" | [ ] |
| 6 | Character data validation tests | test | Bash | succeeds | "dotnet test --filter CharacterDataValidation" | [ ] |
| 7 | CSV-YAML equivalence tests | test | Bash | succeeds | "dotnet test --filter CharacterDataEquivalence" | [ ] |
| 8 | Zero technical debt in Era.Core/Characters/ | code | Grep | not_contains | "TODO\\|FIXME\\|HACK" | [ ] |
| 9 | Zero technical debt in interface file | code | Grep | not_contains | "TODO\\|FIXME\\|HACK" | [ ] |
| 10 | Character loader unit tests | test | Bash | succeeds | "dotnet test --filter CharacterDataLoaderTests" | [ ] |
| 11 | Result<T> error handling tests | test | Bash | succeeds | "dotnet test --filter CharacterDataLoaderErrorHandling" | [ ] |
| 12 | Thread safety verification | code | Grep | contains | "ConcurrentDictionary\\|lock\\|readonly" | [ ] |

### AC Details

**AC#1**: Character YAML files created (14 files)
- Test: Glob pattern="Game/Data/Characters/Chara*.yaml"
- Expected: 14 files (Chara0.yaml through Chara13.yaml)
- Files: Chara0 (player), Chara1-13 (main Touhou characters)

**AC#2**: ICharacterDataLoader interface exists
- Test: Grep pattern="interface ICharacterDataLoader" path="Era.Core/Data/"
- Expected: Interface definition with LoadCharacter method signature

**AC#3**: CharacterData model exists
- Test: Grep pattern="class CharacterData" path="Era.Core/Models/"
- Expected: Strongly typed model with character attributes, validation

**AC#4**: Character data loader implementation
- Test: Grep pattern="class CharacterDataLoader.*ICharacterDataLoader" path="Era.Core/Data/"
- Expected: Implementation class implementing interface

**AC#5**: DI registration exists
- Test: Grep pattern="AddSingleton.*ICharacterDataLoader.*CharacterDataLoader" path="Era.Core/DependencyInjection/"
- Expected: Service registration in ServiceCollectionExtensions.cs

**AC#6**: Character data validation tests
- Test: dotnet test --filter "FullyQualifiedName~CharacterDataValidation"
- Expected: Tests pass validating character data integrity and type constraints

**AC#7**: CSV-YAML equivalence tests
- Test: dotnet test --filter "FullyQualifiedName~CharacterDataEquivalence"
- Expected: Tests pass verifying identical data between CSV and YAML formats

**AC#8**: Zero technical debt in Era.Core/Characters/
- Test: Grep pattern="TODO\\|FIXME\\|HACK" path="Era.Core/Characters/" type="cs"
- Expected: 0 matches across all character implementation files

**AC#9**: Zero technical debt in interface file
- Test: Grep pattern="TODO\\|FIXME\\|HACK" path="Era.Core/Data/ICharacterDataLoader.cs"
- Expected: 0 matches in interface definition

**AC#10**: Character loader unit tests
- Test: dotnet test --filter "FullyQualifiedName~CharacterDataLoaderTests"
- Expected: Comprehensive unit tests for loader functionality

**AC#11**: Result<T> error handling tests
- Test: dotnet test --filter "FullyQualifiedName~CharacterDataLoaderErrorHandling"
- Expected: Tests verify proper Result.Fail patterns for invalid data

**AC#12**: Thread safety verification
- Test: Grep pattern="ConcurrentDictionary\\|lock\\|readonly" path="Era.Core/Data/CharacterDataLoader.cs"
- Expected: Thread-safe implementation patterns detected

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create Game/Data/Characters/ directory and migrate Chara0-13.csv to .yaml format | [ ] |
| 2 | 2,3 | Design and implement ICharacterDataLoader interface and CharacterData model | [ ] |
| 3 | 4,12 | Implement CharacterDataLoader class with thread-safe data loading | [ ] |
| 4 | 5 | Add DI registration for character data services | [ ] |
| 5 | 6,7 | Create data validation and CSV-YAML equivalence tests | [ ] |
| 6 | 10,11 | Implement comprehensive unit tests for loader and error handling | [ ] |
| 7 | 8,9 | Remove technical debt and finalize implementation | [ ] |

---

## Implementation Contract

### Migration Source Reference

**Legacy Location**: `Game/CSV/Chara*.csv`

| File | Character | Notes |
|------|-----------|-------|
| Chara0.csv | Player Character | Base template for all character attributes |
| Chara1.csv | Hakurei Reimu | Shrine maiden protagonist |
| Chara2.csv | Kirisame Marisa | Magician protagonist |
| Chara3.csv | Izayoi Sakuya | Perfect and Elegant Maid |
| Chara4.csv | Hong Meiling | Gatekeeper |
| Chara5.csv | Patchouli Knowledge | Great Magician |
| Chara6.csv | Koakuma | Library Assistant |
| Chara7.csv | Remilia Scarlet | Scarlet Devil |
| Chara8.csv | Flandre Scarlet | Sister of the Devil |
| Chara9.csv | Alice Margatroid | Seven-Colored Puppeteer |
| Chara10.csv | Yakumo Yukari | Gap Youkai |
| Chara11.csv | Saigyouji Yuyuko | Ghostly Butterfly |
| Chara12.csv | Konpaku Youmu | Half-Human Half-Phantom |
| Chara13.csv | Yakumo Ran | Shikigami of Yukari |

### Interface Design

```csharp
// Era.Core/Data/ICharacterDataLoader.cs
using Era.Core.Models;
using Era.Core.Types;

namespace Era.Core.Data;

/// <summary>
/// Loads character configuration data from YAML sources
/// </summary>
public interface ICharacterDataLoader
{
    /// <summary>
    /// Load character data by character ID
    /// </summary>
    /// <param name="characterId">Character identifier (0-13 for main characters)</param>
    /// <returns>Character data or error if not found/invalid</returns>
    Result<CharacterData> LoadCharacter(int characterId);

    /// <summary>
    /// Load all main character data (Chara0-13)
    /// </summary>
    /// <returns>Dictionary of character data or error if any load fails</returns>
    Result<Dictionary<int, CharacterData>> LoadMainCharacters();
}
```

### Data Model Design

```csharp
// Era.Core/Models/CharacterData.cs
namespace Era.Core.Models;

/// <summary>
/// Strongly typed character data model
/// </summary>
public class CharacterData
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required string CallName { get; init; }
    public required string NickName { get; init; }
    public required Dictionary<string, int> Attributes { get; init; }
    public required Dictionary<string, int> Abilities { get; init; }
    public required Dictionary<string, int> Parameters { get; init; }
    public required Dictionary<string, int> Talents { get; init; }

    // Validation method
    public Result<Unit> Validate()
    {
        if (Id < 0) return Result<Unit>.Fail($"Character ID must be non-negative: {Id}");
        if (string.IsNullOrEmpty(Name)) return Result<Unit>.Fail("Character name is required");
        return Result<Unit>.Ok(Unit.Value);
    }
}
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<ICharacterDataLoader, CharacterDataLoader>();
```

### Error Message Format

Character data loading errors should follow this format:
- File not found: `"Character data file not found for ID {id}: {filepath}"`
- Invalid YAML: `"Invalid character data format for ID {id}: {yamlError}"`
- Validation failure: `"Character data validation failed for ID {id}: {validationError}"`

### Test Naming Convention

Test methods follow `Test{MethodName}{Scenario}` format:
- `TestLoadCharacterSuccess`
- `TestLoadCharacterNotFound`
- `TestLoadMainCharactersEquivalence`

### Thread Safety Requirements

| Requirement | Verification |
|-------------|--------------|
| Concurrent data loading | Use ConcurrentDictionary or immutable collections |
| File system access safety | Ensure YAML file reading is thread-safe |
| Caching strategy | Implement thread-safe caching if performance critical |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Remaining characters migration (Chara14+) | Volume limit: 14 files priority focus on main characters | Feature | F533 |
| Character relationship data migration | Complex dependency graph requires separate analysis | Feature | F534 |
| Character event/state migration | Depends on character base data completion | Feature | F535 |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| | | | | |

## Links

- [index-features.md](index-features.md)
- [feature-528.md](feature-528.md) - Critical Config Files Migration (VariableSize)
- [feature-529.md](feature-529.md) - Variable Definition CSVs Migration Part 1
- [feature-530.md](feature-530.md) - Variable Definition CSVs Migration Part 2