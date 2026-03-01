# Feature 729: Game Runtime YAML Character Loading

## Status: [DONE]

## Type: engine

## Background

### Philosophy (Mid-term Vision)
F727 updated KojoTestRunner to load character data from YAML files in --unit test mode. However, the game runtime still relies on CSV-based character loading (which no longer works since F591 removed CSV files). For complete CSV elimination, the game runtime needs YAML character loading integration.

### Problem (Current Issue)
Game runtime character loading path still expects CSV files that were removed by F591. Only the test mode (KojoTestRunner) was updated by F727 to use YAML. The game runtime requires integration with the save/load system and full game initialization flow.

### Goal (What to Achieve)
1. Populate `ConstantData.CharacterTmplList` from YAML character files during `ProcessInitializer.LoadConstantData()`, following the VariableSizeService/GameBaseService pattern
2. Add a public method to `ConstantData` for populating `CharacterTmplList` from external data (bridge method)
3. Map `CharacterConfig` fields to `CharacterTemplate` fields (bridge: Name, CallName, BaseStats, Abilities, Talents, Flags, Experience, Relation; unmapped fields Nickname, Mastername, Mark, Equip, Juel, CStr default to empty/zero)
4. Sort `CharacterTmplList` after population (required for binary search in `GetCharacterTemplate`)
5. Handle errors gracefully: missing/invalid YAML files log warnings but do not crash initialization
6. Verify all 18 character YAML files are loaded (chara0 through chara149)

Note: Save/load is NOT affected by this change. Save files contain fully serialized CharacterData and bypass CharacterTemplate entirely.

## Links
- [feature-727.md](feature-727.md) - Parent feature (test mode YAML loading)
- [feature-589.md](feature-589.md) - YamlCharacterLoader infrastructure
- [feature-591.md](feature-591.md) - Legacy CSV File Removal
- [feature-728.md](feature-728.md) - Character Config Model Extension (extended fields)
- [feature-731.md](feature-731.md) - Character Data Structure Encapsulation

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ConstantData has public PopulateCharacterTemplates method | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | matches | `public void PopulateCharacterTemplates\(` | [x] |
| 2 | PopulateCharacterTemplates adds to CharacterTmplList and sorts | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs) | contains | `CharacterTmplList.Add` | [x] |
| 3 | ProcessInitializer loads character YAML in LoadConstantData | code | Grep(engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs) | contains | `PopulateCharacterTemplates` | [x] |
| 4 | ProcessInitializer uses YamlCharacterLoader for character loading | code | Grep(engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs) | contains | `YamlCharacterLoader` | [x] |
| 5 | Character directory path resolved from data dir | code | Grep(engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs) | contains | `"characters"` | [x] |
| 6 | All 18 YAML files loaded via directory scan | test | dotnet test engine.Tests --filter FullyQualifiedName~CharacterTemplatePopulation | succeeds | - | [x] |
| 7 | Unmapped CharacterTemplate fields default to empty/zero | test | dotnet test engine.Tests --filter FullyQualifiedName~CharacterTemplateDefaults | succeeds | - | [x] |
| 8 | PopulateCharacterTemplates handles empty config list gracefully | test | dotnet test engine.Tests --filter FullyQualifiedName~CharacterLoadMissingDirectory | succeeds | - | [x] |
| 9 | CharacterTmplList sorted after population (binary search precondition) | test | dotnet test engine.Tests --filter FullyQualifiedName~CharacterTemplateSorted | succeeds | - | [x] |
| 10 | CharacterConfig fields mapped to CharacterTemplate correctly | test | dotnet test engine.Tests --filter FullyQualifiedName~CharacterConfigToTemplate | succeeds | - | [x] |
| 11 | Invalid YAML file logs warning and skips without crashing | test | dotnet test engine.Tests --filter FullyQualifiedName~CharacterLoadInvalidYaml | succeeds | - | [x] |
| 12 | Engine builds successfully | build | dotnet build engine/uEmuera.Headless.csproj | succeeds | - | [x] |
| 13 | Zero technical debt | code | Grep(engine/Assets/Scripts/Emuera/GameData/ConstantData.cs,engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs) | not_matches | `TODO|FIXME|HACK` | [x] |

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F727 | [DONE] | Created YAML character data files in `Game/data/characters/` and established YAML loading pattern in KojoTestRunner |
| Predecessor | F589 | [DONE] | YamlCharacterLoader infrastructure (ICharacterLoader, CharacterConfig model) |
| Predecessor | F591 | [DONE] | Legacy CSV File Removal - created the need for YAML-based CharacterTmplList population |
| Related | F728 | [DONE] | CharacterConfig Model Extension for Experience and Relation fields |
| Related | F731 | [WIP] | Character Data Structure Encapsulation - related refactoring |
| Related | F558 | [DONE] | Engine Integration Services - established YAML service integration pattern in ProcessInitializer |
| Successor | F706 | [PROPOSED] | KojoComparer Full Equivalence Verification - depends on working game runtime |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Add PopulateCharacterTemplates and GetCharacterTemplateCount methods to ConstantData | [x] |
| 2 | 3,4,5 | Integrate YAML character loading into ProcessInitializer.LoadConstantData | [x] |
| 3 | 6,7,9,10 | Write unit tests for CharacterTemplate population and field mapping | [x] |
| 4 | 8,11 | Write unit tests for error handling (missing directory, invalid YAML) | [x] |
| 5 | 12 | Verify engine build succeeds | [x] |
| 6 | 13 | Verify zero technical debt | [x] |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| `engine/Assets/Scripts/Emuera/GameData/ConstantData.cs` | Modified | Added PopulateCharacterTemplates() and GetCharacterTemplateCount() methods |
| `engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs` | Modified | Added YAML character loading to LoadConstantData() |
| `engine.Tests/Tests/CharacterTemplatePopulationTests.cs` | New | 6 unit tests (4 positive, 2 negative scenarios) |

## Execution Log

| Timestamp | Event | Agent | Action | Detail |
|-----------|-------|-------|--------|--------|
| 2026-02-01 | START | orchestrator | /run 729 | Phase 1: Initialize |
| 2026-02-01 | INIT | initializer | Status update | [REVIEWED] → [WIP] |
| 2026-02-01 | INVESTIGATE | explorer | Codebase analysis | All patterns verified |
| 2026-02-01 | RED | implementer | Test creation | 6 tests, 9 compile errors (expected) |
| 2026-02-01 | GREEN | implementer | Tasks 1-2 | PopulateCharacterTemplates + ProcessInitializer integration |
| 2026-02-01 | VERIFY | ac-tester | AC verification | 13/13 PASS |
| 2026-02-01 | DEVIATION | ac-tester | Feature file overwrite | ac-tester rewrote feature-729.md removing all sections below Links. Restored from context |

## Handoff

No deferred items. All tasks completed within F729 scope.
