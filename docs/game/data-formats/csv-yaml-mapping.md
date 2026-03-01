# CSV to YAML Migration Decisions

## Overview

This document summarizes the architectural decisions and implementation history of the CSV to YAML migration effort, based on the Architecture Analysis (F562) and subsequent implementation features (F563-F593). The migration establishes YAML as the Single Source of Truth (SSOT) for game configuration while maintaining a clear separation between C# engine code and community-editable content.

### Architecture Decision: Tier-based Moddability

The F562 Architecture Analysis established a tiered approach to moddability that determines which game elements reside in YAML (community-editable) versus C# (engine domain):

| Tier | Scope | C# Compilation Required | Format |
|:----:|-------|:-----------------------:|--------|
| **1** | New content creation (kojo, character definitions, COM derivatives) | No | YAML |
| **2** | Existing content adjustment (COM parameters, character stats) | No | YAML |
| **3** | New game mechanics (Talents, Abilities, effect types) | Yes | C# enums |

This three-tier model preserves "ERA-style community participation" (edit text file → game works immediately) for Tier 1+2 content while acknowledging that fundamental game mechanics (Tier 3) require engine-level C# changes.

## Current Data Format Structure

### YAML Loaders Implemented

As of F583 (Complete CSV Elimination) and F589 (Character CSV Files YAML Migration), the following 23 YAML loaders provide comprehensive configuration loading infrastructure:

#### Variable Definition Loaders (F528, F558, F575)
- **VariableSizeConfig**: Array size definitions for game variables
- **GameBaseConfig**: Base game configuration parameters

#### CSV Type Loaders (F583)
1. **TalentConfig**: Character talent definitions (index, name, description)
2. **AblConfig**: Ability system definitions
3. **BaseConfig**: Base character statistics
4. **CFlagConfig**: Character flag definitions
5. **CStrConfig**: Character string data
6. **EquipConfig**: Equipment item definitions
7. **TequipConfig**: Equipment templates
8. **ExConfig**: Extended data arrays
9. **ExpConfig**: Experience point data
10. **ItemConfig**: Game item definitions
11. **JuelConfig**: Jewelry/Juel data
12. **MarkConfig**: Mark system definitions
13. **PalamConfig**: Parameter array definitions
14. **SourceConfig**: Source point definitions
15. **StainConfig**: Stain system data
16. **StrConfig**: String table definitions
17. **TStrConfig**: Temporary string data
18. **TCVarConfig**: Temporary character variables
19. **TFlagConfig**: Temporary flag definitions
20. **TrainConfig**: Training command data
21. **RenameConfig**: Character rename operations
22. **ReplaceConfig**: Text replacement rules
23. **FlagConfig**: Global game flags

#### Character Loaders (F589)
24. **CharacterConfig**: Character initialization data (19 character files: Chara0-13, Chara28-29, Chara99, Chara148-149)
  - Character ID, Name, CallName
  - Base stats collection (Dictionary<int, int>)
  - Abilities collection (Dictionary<int, int>)
  - Talents collection (Dictionary<int, int>)
  - Flags collection (Dictionary<int, int>)

#### COM Loaders (F563, F565)
25. **ComDefinition**: COM (command) YAML definitions (152 COM files)
  - All COMs migrated from C# classes to YAML format
  - Effect handlers implemented for Source, Downbase, Exp, SourceScale
  - Dynamic COM execution via YamlComExecutor

### File Type Mapping

The following table shows the mapping from legacy CSV filenames to YAML loader implementations:

| CSV Filename | YAML Loader | Data Model | YAML Path | Notes |
|--------------|-------------|------------|-----------|-------|
| VariableSize.csv | YamlVariableSizeLoader | VariableSizeConfig | Game/data/variable_sizes.yaml | F528, F558, F575 |
| GAMEBASE.CSV | YamlGameBaseLoader | GameBaseConfig | Game/data/gamebase.yaml | F528, F558, F575 |
| Talent.csv | YamlTalentLoader | TalentConfig | Game/data/talent.yaml | F583 |
| Abl.csv | YamlAblLoader | AblConfig | Game/data/abl.yaml | F583 |
| Base.csv | YamlBaseLoader | BaseConfig | Game/data/base.yaml | F583 |
| CFLAG.csv | YamlCFlagLoader | CFlagConfig | Game/data/cflag.yaml | F583 |
| CSTR.csv | YamlCStrLoader | CStrConfig | Game/data/cstr.yaml | F583 |
| Equip.csv | YamlEquipLoader | EquipConfig | Game/data/equip.yaml | F583 |
| Tequip.csv | YamlTequipLoader | TequipConfig | Game/data/tequip.yaml | F583 |
| ex.csv | YamlExLoader | ExConfig | Game/data/ex.yaml | F583 |
| exp.csv | YamlExpLoader | ExpConfig | Game/data/exp.yaml | F583 |
| Item.csv | YamlItemLoader | ItemConfig | Game/data/item.yaml | F583 |
| Juel.csv | YamlJuelLoader | JuelConfig | Game/data/juel.yaml | F583 |
| Mark.csv | YamlMarkLoader | MarkConfig | Game/data/mark.yaml | F583 |
| Palam.csv | YamlPalamLoader | PalamConfig | Game/data/palam.yaml | F583 |
| source.csv | YamlSourceLoader | SourceConfig | Game/data/source.yaml | F583 |
| Stain.csv | YamlStainLoader | StainConfig | Game/data/stain.yaml | F583 |
| Str.csv | YamlStrLoader | StrConfig | Game/data/str.yaml | F583 |
| TSTR.csv | YamlTStrLoader | TStrConfig | Game/data/tstr.yaml | F583 |
| TCVAR.csv | YamlTCVarLoader | TCVarConfig | Game/data/tcvar.yaml | F583 |
| TFLAG.csv | YamlTFlagLoader | TFlagConfig | Game/data/tflag.yaml | F583 |
| Train.csv | YamlTrainLoader | TrainConfig | Game/data/train.yaml | F583 |
| _Rename.csv | YamlRenameLoader | RenameConfig | Game/data/rename.yaml | F583 |
| _Replace.csv | YamlReplaceLoader | ReplaceConfig | Game/data/replace.yaml | F583 |
| FLAG.CSV | YamlFlagLoader | FlagConfig | Game/data/flag.yaml | F583 |
| Chara*.csv (19 files) | YamlCharacterLoader | CharacterConfig | Game/data/characters/*.yaml | F589 |
| COM*.cs (152 classes) | YamlComLoader | ComDefinition | Game/data/coms/**/*.yaml | F563, F565 |

### Schema Locations

YAML schema validation is provided by F590 (YAML Schema Validation Tools):

- **Schema Generation Tool**: `tools/YamlSchemaGen/` - Generates JSON Schema from C# models
- **Schema Validation Tool**: `tools/YamlValidator/` - Validates YAML files against schemas
- **Community Validator**: `src/tools/go/com-validator/` - Japanese-language YAML validation for modders
- **COM Schema**: `src/tools/schemas/com.schema.json` - JSON Schema for COM YAML validation (F563)

For detailed usage of validation tools, see:
- [YamlSchemaGen README](../../../tools/YamlSchemaGen/README.md)
- [YamlValidator README](../../../tools/YamlValidator/README.md)
- [COM YAML Modding Guide](../modding/COM-YAML-Guide.md)

## Migration History

### Phase 17 Original Plan (F529-F540)

Phase 17 was initially planned as a comprehensive CSV to YAML migration covering 12 features:

- **F529**: Variable Definition CSVs Migration Part 1
- **F530**: Variable Definition CSVs Migration Part 2 (Talent/Abl) - **CANCELLED** (Tier 3 exclusion)
- **F531**: Config Files Migration
- **F532**: Character Data Migration Part 1 - Chara0-Chara13
- **F533**: Character Data Migration Part 2 - Sub NPCs Additional
- **F534**: Content Definition CSVs Migration Part 1 - Train.csv, Item.csv, Equip.csv, Tequip.csv
- **F535**: Content Definition CSVs Migration Part 2 - Mark.csv, Juel.csv, Stain.csv, source.csv
- **F536**: String Tables Migration - Str.csv, CSTR.csv, TSTR.csv, TCVAR.csv
- **F537**: Transform Rules Migration - **CANCELLED** (per F562/F563 decision)
- **F538**: CsvToYaml Converter Tool
- **F539**: SchemaValidator with Era.Core.Validation Layer
- **F540**: Post-Phase Review Phase 17

### Architecture Revision (F562)

F562 (Architecture Analysis: C#/YAML Boundary Definition) fundamentally reconsidered the migration approach:

**Key Findings**:
1. **Phantom Moddability Issue**: YAML data additions don't work without corresponding C# handler code for many content types
2. **COM YAML-ization Feasibility**: 80% of COMs (152 commands) can be implemented with YAML-only definitions
3. **Talent/Ability C# Requirement**: New talent/ability definitions require C# enum changes (Tier 3)
4. **Community Participation Preservation**: ERA-style "edit text file → game works" workflow must be maintained for genuinely moddable content

**Architectural Decision**: Establish Tier 1/2/3 moddability boundaries rather than attempting complete YAML migration of all data types.

### Actual Implementation (F575-F592)

Following the F562 analysis, the migration was restructured around Tier 1+2 capabilities:

#### Infrastructure Establishment (F558, F575, F576)
- **F558**: Engine Integration Services for Critical Config - Created VariableSizeService and GameBaseService YAML integration
- **F575**: CSV Partial Elimination (VariableSize/GameBase) - Removed CSV loading for VariableSize.csv and GAMEBASE.CSV, established YAML-first pattern
- **F576**: Character 2D Array Support Extension - Added CDFLAG property to VariableSizeConfig for character 2D integer arrays

#### COM YAML Migration (F563, F565)
- **F563**: Architecture Implementation: Full COM YAML Migration
  - Migrated all 152 COM C# classes to YAML format in single atomic operation
  - Created Effect Handler infrastructure (IEffectHandler, SourceEffectHandler, DownbaseEffectHandler, ExpEffectHandler, SourceScaleEffectHandler)
  - Implemented YamlComLoader and YamlComExecutor for dynamic COM execution
  - Deleted 152 C# COM implementation files, retained 12 infrastructure files
  - **User Decision**: "Tier 1+2 Method A (全COM YAML化)" - All COMs migrated at once during migration phase
- **F565**: COM YAML Runtime Integration
  - Populated 152 YAML COM definitions with actual effect data (no longer stubs)
  - Implemented CreateEffectContext for runtime execution
  - Completed SourceScaleEffectHandler formula implementation
  - Achieved full COM YAML test coverage (159 tests pass, 0 skipped)

#### Complete CSV Elimination (F583, F589, F591, F592)
- **F583**: Complete CSV Elimination (Remaining File Types)
  - Created 23 YAML loader interfaces and implementations for all remaining CSV types
  - Established comprehensive loader infrastructure with Result<T> error handling pattern
  - Created 23 data model classes (*Config) and 23 unit test files
  - Registered all loaders in DI container (src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs)
- **F589**: Character CSV Files YAML Migration (Chara*.csv)
  - Created CharacterConfig loader for 19 character files
  - Supports multi-property initialization data (CharacterId, Name, CallName, BaseStats, Abilities, Talents, Flags)
  - Established YAML format for character definitions distinct from simple key-value loaders
- **F591**: Legacy CSV File Removal
  - Removed all 44 legacy CSV files from Game/CSV directory in atomic operation
  - Updated VariableResolver.cs to use Game/data YAML paths instead of Game/CSV
  - Updated all documentation references from CSV to YAML paths
  - Established YAML as unambiguous Single Source of Truth
- **F592**: Engine Fatal Error Exit Handling
  - Implemented proper exit codes (1) for YAML load failures
  - Created FatalErrorHandler infrastructure for automated testing reliability
  - Enabled fail-fast behavior during migration phase

### Cancelled Features Rationale

Two Phase 17 features were cancelled based on F562 Architecture Analysis:

#### F530: Variable Definition CSVs Migration Part 2 (Talent/Abl)
**Cancelled Reason**: Tier 3 exclusion. New Talent and Ability definitions require C# enum changes to add handler code. While existing talent/ability *values* can be stored in YAML (and are, via TalentConfig/AblConfig loaders from F583), adding *new* talent/ability types requires modifying TalentIndex and AbilityIndex C# enums plus implementing corresponding effect handlers in the engine.

**Current State**: TalentConfig and AblConfig loaders (F583) provide YAML-based storage for talent and ability *data* (index, name, description values), but new talent/ability *types* remain in C# enums as Tier 3 content.

#### F537: Transform Rules Migration
**Cancelled Reason**: Per F562/F563 decision, transform rules are part of the engine's data transformation pipeline and do not benefit from YAML externalization. Transform rules define how the engine processes data internally, not community-editable content.

**Current State**: Transform rules remain as engine implementation details in C# code.

## Tier 3 Rationale

### Why CSV Files (Now YAML) Remain ERB Domain

The Tier 3 boundary acknowledges a fundamental distinction in modding:

**Tier 1+2 (YAML Moddability)**:
- Modifying existing content parameters (COM thresholds, character stats, item costs)
- Adding new content instances using existing types (new characters, new COM derivatives, new kojo dialogue)
- **Works without C# compilation**: Edit YAML → reload game → immediate effect

**Tier 3 (C# Engine Domain)**:
- Adding new game mechanics that require handler code (new talent types, new ability types, new effect types)
- Defining new data structures or variable types
- Implementing new command categories beyond existing COM infrastructure
- **Requires C# compilation**: Edit C# enum → add handler → compile engine → deploy

### ERB-Level Moddability Explanation

In the original ERB system, Tier 3 content required modifying ERB scripts and potentially the Emuera interpreter itself. The C# migration preserves this boundary:

| Content Type | Original ERB | Current C# | Community Access |
|--------------|--------------|------------|------------------|
| New kojo dialogue | Edit .ERB file | Edit .yaml file | Tier 1 - Full access |
| Adjust COM parameters | Edit .ERB file | Edit .yaml file | Tier 2 - Full access |
| New talent type | Modify TALENT enum + .ERB handlers | Modify TalentIndex enum + C# handlers | Tier 3 - Requires engine PR |
| New COM category | Modify COM infrastructure | Modify COM infrastructure | Tier 3 - Requires engine PR |

The key insight from F562 Architecture Analysis: **Not all data externalization creates genuine moddability**. Externalizing talent enum values to YAML without providing corresponding C# handler code would create "phantom moddability" - users could add YAML entries that appear to work but fail at runtime.

The Tier 1/2/3 distinction makes the boundary explicit: Tier 1+2 content is genuinely community-editable, while Tier 3 content requires engine-level contributions through pull requests.

### Architecture Decision Summary

Per F562 and F563, the C#/YAML boundary is defined by:

1. **C# Engine Domain** (Tier 3):
   - Interpreters and execution frameworks
   - Effect handlers and condition evaluators
   - Type definitions (enums for Talent, Ability, Flag indices)
   - Registry systems and infrastructure

2. **YAML Content Domain** (Tier 1+2):
   - COM definitions (152 commands)
   - Character initialization data (19 characters)
   - Game configuration (variable sizes, base stats, items, equipment)
   - String tables and localization data
   - Kojo dialogue rendering data (via COM YAML integration)

This boundary enables "ERA-style community participation" for Tier 1+2 content (edit YAML → reload → works) while acknowledging that fundamental game mechanics (Tier 3) require engine-level C# development.

## Cross-References

For complete feature dependency graph and system architecture, see:

- [Feature Dependencies](../architecture/Feature-Dependencies.md) - Visual dependency graph for F563-F593 migration chain
- [System Overview](../architecture/System-Overview.md) - COM YAML infrastructure and development workflow
- [COM YAML Modding Guide](../modding/COM-YAML-Guide.md) - Tier 1+2 moddability examples and validation workflow

For migration implementation details, see individual feature files:

- [Feature 562: Architecture Analysis](../../agents/feature-562.md) - C#/YAML boundary definition
- [Feature 563: Full COM YAML Migration](../../agents/feature-563.md) - 152 COM migration
- [Feature 575: CSV Partial Elimination](../../agents/feature-575.md) - VariableSize/GameBase
- [Feature 576: Character 2D Array Support](../../agents/feature-576.md) - CDFLAG extension
- [Feature 583: Complete CSV Elimination](../../agents/feature-583.md) - 23 YAML loaders
- [Feature 589: Character CSV Files YAML Migration](../../agents/feature-589.md) - Character loaders
- [Feature 591: Legacy CSV File Removal](../../agents/feature-591.md) - Final CSV cleanup
- [Feature 592: Engine Fatal Error Exit Handling](../../agents/feature-592.md) - YAML error handling
