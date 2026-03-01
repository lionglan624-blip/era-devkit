# Feature 533: Character Data Migration Part 2 - Sub NPCs Additional Characters

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

### Philosophy (思想・上位目標)

**Phase 17: Data Migration** - Establish YAML/JSON as the single source of truth for character data definitions, ensuring consistent loading behavior and simplified maintenance. This feature continues the data migration strategy by converting sub-NPC and additional character CSV files to YAML format while maintaining exact equivalence with legacy CSV behavior.

### Problem (現状の問題)

Phase 17 Character Data Migration is split into two parts for granularity compliance. After F532 handles main characters (Chara0-Chara13), 5 additional character files remain in legacy CSV format:
- Chara28.csv, Chara29.csv (Sub characters)
- Chara99.csv (Special NPC)
- Chara148.csv, Chara149.csv (Additional characters)

These files use the same CSV→YAML migration pattern as F532 but represent lower-priority characters in the game system.

### Goal (このFeatureで達成すること)

1. **Migrate 5 character CSV files to YAML** following Phase 4 ICharacterDataLoader interface design
2. **Implement ICharacterDataLoader for additional characters** with DI registration and strongly typed models
3. **Verify CSV→YAML equivalence** using automated equivalence testing
4. **Eliminate technical debt** by removing TODO/FIXME/HACK markers during migration
5. **Ensure schema validation** using SchemaValidator CLI tool

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Character YAML files exist | file | Glob | exists | "Game/Data/Characters/Character*.yaml" | [ ] |
| 2 | ICharacterDataLoader implementation | code | Grep | contains | "class.*CharacterDataLoader.*ICharacterDataLoader" | [ ] |
| 3 | DI registration verification | file | Grep | contains | "AddSingleton.*ICharacterDataLoader.*CharacterDataLoader" | [ ] |
| 4 | Character28 data equivalence | test | Bash | succeeds | "dotnet test --filter Character28Equivalence" | [ ] |
| 5 | Character29 data equivalence | test | Bash | succeeds | "dotnet test --filter Character29Equivalence" | [ ] |
| 6 | Character99 data equivalence | test | Bash | succeeds | "dotnet test --filter Character99Equivalence" | [ ] |
| 7 | Character148 data equivalence | test | Bash | succeeds | "dotnet test --filter Character148Equivalence" | [ ] |
| 8 | Character149 data equivalence | test | Bash | succeeds | "dotnet test --filter Character149Equivalence" | [ ] |
| 9 | Schema validation PASS | test | Bash | succeeds | "dotnet run --project tools/YamlValidator -- Game/Data/Characters/" | [ ] |
| 10 | Zero technical debt | code | Grep | not_contains | "TODO\\|FIXME\\|HACK" | [ ] |
| 11 | All tests PASS | test | Bash | succeeds | "dotnet test" | [ ] |
| 12 | Phase 4 interface compliance | code | Grep | contains | "Result<CharacterData> Load" | [ ] |

### AC Details

**AC#1**: Character YAML files created
- Test: Glob pattern="Game/Data/Characters/Character*.yaml"
- Expected: 5 files (Character28.yaml, Character29.yaml, Character99.yaml, Character148.yaml, Character149.yaml)
- Verifies output file creation per migration

**AC#2**: ICharacterDataLoader implementation exists
- Test: Grep pattern="class.*CharacterDataLoader.*ICharacterDataLoader" path="Era.Core/Characters/"
- Verifies interface implementation following Phase 4 design requirements

**AC#3**: DI registration verification
- Test: Grep pattern="AddSingleton.*ICharacterDataLoader.*CharacterDataLoader" path="Era.Core/DependencyInjection/"
- Verifies service registration per Phase 4 requirements

**AC#4-8**: CSV→YAML equivalence verification
- Test naming convention: Test{CharacterNumber}Equivalence (e.g., TestCharacter28Equivalence)
- Each test verifies that YAML data produces identical results to legacy CSV data
- Minimum 3 Assert statements per equivalence test

**AC#9**: Schema validation PASS
- Test: SchemaValidator CLI tool validates all character YAML files
- Ensures YAML structure compliance with defined schema

**AC#10**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" across all feature files
- Scope: Era.Core/Characters/, Game/Data/Characters/
- Expected: 0 matches (clean implementation)

**AC#11**: All tests PASS
- Test: dotnet test (full test suite)
- Ensures no regression after migration

**AC#12**: Phase 4 interface compliance
- Test: Grep pattern="Result<CharacterData> Load" path="Era.Core/Characters/"
- Verifies interface method signature compliance

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create Character YAML files from CSV source (5 files) | [ ] |
| 2 | 2,3,12 | Implement ICharacterDataLoader interface with DI registration | [ ] |
| 3 | 4,5,6,7,8 | Create CSV→YAML equivalence tests (5 tests) | [ ] |
| 4 | 9 | Verify schema validation using YamlValidator tool | [ ] |
| 5 | 10 | Remove technical debt markers during implementation | [ ] |
| 6 | 11 | Ensure full test suite passes after migration | [ ] |

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Migration Source Reference

**Legacy Location**: `Game/CSV/Chara{28,29,99,148,149}.csv`

| File | Character Role | Notes |
|------|---------------|-------|
| Chara28.csv | Sub character | Secondary character data |
| Chara29.csv | Sub character | Secondary character data |
| Chara99.csv | Special NPC | Special system character |
| Chara148.csv | Additional character | Extended character set |
| Chara149.csv | Additional character | Extended character set |

### Phase 4 Design Requirements

**Interface Contract**:
```csharp
// Era.Core/Characters/ICharacterDataLoader.cs
using Era.Core.Types;

namespace Era.Core.Characters;

public interface ICharacterDataLoader
{
    /// <summary>Load character data by ID</summary>
    Result<CharacterData> Load(int characterId);

    /// <summary>Get all available character IDs</summary>
    Result<IEnumerable<int>> GetAvailableIds();
}
```

**Implementation Stub**:
```csharp
// Era.Core/Characters/CharacterDataLoader.cs
using Era.Core.Types;

namespace Era.Core.Characters;

public class CharacterDataLoader : ICharacterDataLoader
{
    public Result<CharacterData> Load(int characterId)
        => Result<CharacterData>.Fail("Character data loading not implemented - awaiting Phase 18");

    public Result<IEnumerable<int>> GetAvailableIds()
        => Result<IEnumerable<int>>.Ok(new[] { 28, 29, 99, 148, 149 });
}
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<ICharacterDataLoader, CharacterDataLoader>();
```

### Test Naming Convention

Test methods follow `Test{CharacterNumber}Equivalence` format:
- `TestCharacter28Equivalence`
- `TestCharacter29Equivalence`
- `TestCharacter99Equivalence`
- `TestCharacter148Equivalence`
- `TestCharacter149Equivalence`

This ensures AC filter patterns match correctly.

### Error Message Format

Character loading errors use format: `"キャラクター{ID}のデータが見つかりません"` (e.g., `"キャラクター28のデータが見つかりません"`).

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F532 | Character Data Migration Part 1 must complete first |
| Interface | Phase 4 | ICharacterDataLoader interface design |
| Tool | F538 | CsvToYaml converter tool |
| Tool | F539 | SchemaValidator CLI for validation |

## Links

- [feature-532.md](feature-532.md) - Character Data Migration Part 1 (dependency)
- [feature-516.md](feature-516.md) - Phase 17 Planning
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 17 definition
- [index-features.md](index-features.md)

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Engine integration implementation | Era.Core provides stubs, actual loading requires engine integration | Feature | F542+ (Phase 18 Engine Integration) |

## Review Notes

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|