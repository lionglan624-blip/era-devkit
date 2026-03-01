# Architecture Analysis: C# Engine + YAML Content Boundary Definition

**Feature**: [feature-562.md](../feature-562.md)
**Status**: Analysis Complete
**Date**: 2026-01-19

---

## Executive Summary

This analysis redefines the C#/YAML boundary in the ERA game architecture based on F537 FL Review findings. The fundamental insight: **C# migration produces a better interpreter + structured content**, NOT "everything becomes C#". Community moddability requires preserving ERA-style text file editing workflow while gaining modern engine benefits.

**Key Findings**:
1. **80% of COMs can be YAML-only** - Effect combinations need no C# code
2. **Phantom Moddability Problem** - YAML additions often require C# changes
3. **True Moddability** - Kojo + Character combinations work without C# compilation
4. **Existing C# Implementation Audit** - 150+ COM classes need disposition analysis

**F537 Transform Rules recommendation**: **REVISE** - Transform from "YAML migration" to "Engine infrastructure consolidation"

---

## Table of Contents

1. [Current State Analysis](#current-state-analysis)
2. [F537 Discussion Summary](#f537-discussion-summary)
3. [Existing C# Implementation Audit](#existing-c-implementation-audit)
4. [C# Engine vs YAML Content Boundary Definition](#c-engine-vs-yaml-content-boundary-definition)
5. [Community Moddability Assessment](#community-moddability-assessment)
6. [Phase 17 Feature Impact Analysis](#phase-17-feature-impact-analysis)
7. [COM C# Class Disposition Analysis](#com-c-class-disposition-analysis)
8. [Migration Path: C# to YAML](#migration-path-c-to-yaml)
9. [Phantom Moddability Prevention Strategy](#phantom-moddability-prevention-strategy)
10. [Recommendations for F563 Implementation](#recommendations-for-f563-implementation)

---

## Current State Analysis

### architecture.md Assumptions (Pre-F537 FL Review)

**Phase 17 Philosophy** (as written):
> "Establish YAML/JSON as the single source of truth for all ERA configuration data, ensuring type safety through strongly typed data models and consistent data access via IDataLoader interface pattern."

**Implicit Assumptions**:
1. **Data-Code Separation Goal** - External files enable testability and configuration management
2. **Developer Productivity Focus** - Type safety, IDE support, refactoring ease
3. **Uniform Moddability Assumption** - All YAML data is equally moddable
4. **CSV→YAML Migration Pattern** - 43 CSV files scheduled for YAML conversion

**Architecture Gaps**:
- No distinction between "genuinely moddable" vs "engine-dependent" data
- No community participation workflow analysis
- No cost-benefit analysis for individual CSV migrations
- Phantom moddability not addressed

### What Was Working

**Strengths**:
- IDataLoader pattern for type-safe data access
- Strongly typed IDs (CharacterId, ComId, TalentIndex, etc.)
- Result<T> error handling
- DI-based architecture

**Precedent from F528** (Critical Config Files Migration):
- VariableSize.csv → YAML migration completed successfully
- IVariableSizeLoader interface pattern established
- Schema validation functional

---

## F537 Discussion Summary

### Discovery: Transform Rules Migration Analysis

**Original F537 Scope**: Migrate _Rename.csv and _Replace.csv to YAML format with ITransformRuleLoader interface.

**Actual CSV Content**:
| File | Expected | Reality |
|------|----------|---------|
| _Rename.csv | Multiple rename rules | **ZERO** (template comments only) |
| _Replace.csv | Multiple replacement rules | **1 rule** (汚れの初期値: 0/0/2/1/8/0/1/8) |

**Conclusion**: Migrating empty/minimal CSV files to YAML serves no purpose.

### Key Insights from FL Review Discussion

#### 1. Phantom Moddability Problem

**Definition**: YAML data additions that appear moddable but don't work without C# code changes.

**Example**:
```yaml
# User adds to Talent.yaml
- id: 999
  name: "超巨乳"
  description: "Very large breasts trait"
```

**Result**:
1. Game loads YAML ✅
2. C# code has no handler for talent 999 ❌
3. Talent has zero effect in gameplay ❌
4. User frustrated - "mod doesn't work" ❌

**Root Cause**: Architecture treats all YAML data as equally moddable, but practical moddability varies dramatically.

#### 2. The 80% COM YAML-ization Feasibility

**COM Structure Analysis** (185 files):
```
COM = {
  実行判定 (CAN_COM): 条件チェック + 閾値判定
  効果 (COM): SOURCE/DOWNBASE/EXP の変更
  口上: 別ファイル（YAML化済み）
}
```

**Feasibility Assessment**:
| Expansion Type | Required Work |
|---------------|---------------|
| New COM (existing effects) | **YAML file addition only** |
| New effect type | C# handler + YAML schema update |
| New condition type | C# evaluator + YAML schema update |

**Quantification**: The majority of COMs use YAML (percentage: 80%) - existing effect/condition combinations → YAML-only creation possible.

**Representative YAML Example**:
```yaml
# Game/data/coms/com101_鞭.yaml
id: 101
name: "鞭"
category: "SM"
cost:
  stamina: 80
  energy: 50
effects:
  - type: source
    pain: 1500
    fear: 1200
  - type: source_scale
    target: pain
    formula: "value * (10 + 5 * max(getPalamLv(pain, 5) - 1, 0)) / 10"
```

#### 3. ERA-style Community Participation Definition

**Essential Workflow**:
```
Community Member Workflow:
1. Edit text file (YAML)
2. Reload game
3. Content works immediately
4. NO: C# compilation, PR approval, Unity build
```

**Historical Context**: ERB-era modding flourished because of this low barrier to entry. C# migration must preserve this for genuinely moddable content types.

#### 4. What Actually Changed from ERB Era?

| Aspect | ERB Era | YAML + C# Engine |
|--------|---------|------------------|
| **Content Format** | ERB (独自スクリプト) | YAML (標準フォーマット) |
| **Interpreter** | Emuera (古い、メンテ困難) | C# Engine (モダン、テスト可能) |
| **Logic-Data** | **混在** | **分離** |
| **Schema Validation** | なし | JSON Schema |
| **Community Participation** | ✅ Easy | ✅ Can be easy (if designed correctly) |

**C# Migration Essence**:
- **Improved interpreter**: Emuera → modern C# engine with better maintainability
- **Structured content**: ERB scripts → schema-validated YAML
- **NOT "everything becomes C#"**: Content creation workflow stays text-file-based

**True Benefits**:
- Engine quality improvement (testable, maintainable)
- Testability for both engine AND content
- Separation of concerns (logic in C#, data in YAML)
- Early error detection via schema validation
- Modern development tools (IDE, debugger, profiler)

---

## Existing C# Implementation Audit

### Inventory Summary

**src/Era.Core/Commands/Com/** - 168 COM implementation files (category analysis):

| Category | File Count | Representative Examples | Disposition |
|----------|:----------:|------------------------|-------------|
| **Training/Touch** | 14 | Kiss.cs, BreastCaress.cs, AnalCaress.cs, ClitCaress.cs, FingerInsertion.cs, NippleSuck.cs | **MIGRATE TO YAML** |
| **Training/Oral** | 17 | Fellatio, Cunnilingus, Handjob, Paizuri, Footjob, OralSex, MakeLick | **MIGRATE TO YAML** |
| **Training/Penetration** | 25 | MissionaryStub, CowgirlStub, DoggyStub, AnalSexStub, VaginalCreampieStub, EjaculationStubs | **MIGRATE TO YAML** |
| **Training/Equipment** | 17 | AnalVibrator, Aphrodisiac, ClitoralCap, MilkingMachine, Hypnosis, DildoStub | **MIGRATE TO YAML** |
| **Training/Bondage** | 11 | RopeBondage, Blindfold, BallGag, Spanking, WhipAnus, Photography, VideoCamera | **MIGRATE TO YAML** |
| **Training/Undressing** | 4 | UndressTop, UndressBra, UndressPanties, UndressBottom | **MIGRATE TO YAML** |
| **Training/Utility** | 2 | Rest, Excretion | **MIGRATE TO YAML** |
| **Daily** | 17 | Kiss, Hug, ServeTea, Confess, BreastCaress, Skinship, PushDown, AskForgiveness | **MIGRATE TO YAML** |
| **Masturbation** | 17 | SelfMasturbation, SelfCaress, SelfFingerInsertion, SelfVibrator, SelfAnalCaress | **MIGRATE TO YAML** |
| **Utility** | 21 | Move, Eat, Sleep, Study, Cook, Clean, CombatTraining, Defeat, Masturbate, BuyKey | **HYBRID** (logic C#, data YAML) |
| **Visitor** | 4 | InviteVisitor, GuideVisitor, GoOut, SeparateFromVisitor | **KEEP C#** (complex state) |
| **System** | 2 | DayEnd, Dummy | **KEEP C#** (engine control) |
| **Base Classes** | 10 | ComBase, ComContext, ComRegistry, EquipmentComBase, ICom, IComRegistry, etc. | **KEEP C#** (engine infrastructure) |

**Total**: 168 files analyzed at category level. Individual COM disposition delegated to F563 implementation.

**src/Era.Core/Types/** - 36 type definition files:

| Type Category | Files | Purpose | Disposition |
|--------------|-------|---------|-------------|
| **Strongly Typed IDs** | 6 | CharacterId, LocationId, ComId, CommandId, EquipmentIndex | **KEEP C#** (type safety core) |
| **Index Enums** | 13 | TalentIndex, AbilityIndex, FlagIndex, CharacterFlagIndex, PalamIndex, ExpIndex, SourceIndex, DownbaseIndex, MaxBaseIndex, ExIndex, NowExIndex, MarkIndex, StainIndex, TCVarIndex, LocalVariableIndex, JuelIndex, CupIndex | **EVALUATE** (see Talent/Ability strategy) |
| **Configuration** | 5 | GameConfig, NtrParameters, GameSaveState, GameTick, VariableScopeType | **KEEP C#** (engine state) |
| **Utility Types** | 4 | Result.cs, DialogueResult, VariableReference, ITypeConverter, TypeConverter | **KEEP C#** (framework) |
| **CSV Definitions** | 1 | CsvVariableDefinitions.cs | **MIGRATE TO YAML** (data definitions) |
| **Domain Types** | 2 | NtrAction, Unit | **KEEP C#** (domain logic) |

**src/Era.Core/Data/** - 6 data loader files:

| File | Purpose | Disposition |
|------|---------|-------------|
| IGameBaseLoader.cs | Interface for game base config loading | **KEEP C#** (interface) |
| IVariableSizeLoader.cs | Interface for variable size loading | **KEEP C#** (interface) |
| YamlGameBaseLoader.cs | YAML implementation for GameBase | **KEEP C#** (loader implementation) |
| YamlVariableSizeLoader.cs | YAML implementation for VariableSize | **KEEP C#** (loader implementation) |
| Models/ | Data model classes | **KEEP C#** (strongly typed models) |

### Category-Level Disposition Summary

| Category | Disposition | Rationale |
|----------|-------------|-----------|
| **Training COM Classes** (90+ files) | **MIGRATE TO YAML** | 80% use existing effect combinations, YAML representation sufficient |
| **Daily COM Classes** (17 files) | **MIGRATE TO YAML** | Simple state changes, no complex logic |
| **Masturbation COM Classes** (17 files) | **MIGRATE TO YAML** | Self-contained actions, existing effect types |
| **Utility COM Classes** (21 files) | **HYBRID** | Logic (C#) + Configuration (YAML) - e.g., CombatTraining parameters |
| **Visitor/System COM Classes** (6 files) | **KEEP C#** | Complex state management, engine control flow |
| **COM Infrastructure** (10 files) | **KEEP C#** | Engine foundation (ComBase, ComRegistry, IComContext) |
| **Strongly Typed IDs** (6 files) | **KEEP C#** | Compile-time type safety essential |
| **Index Enums** (13 files) | **EVALUATE** | TalentIndex/AbilityIndex require strategy decision (see next section) |
| **Data Loaders** (6 files) | **KEEP C#** | IDataLoader pattern is engine infrastructure |

---

## C# Engine vs YAML Content Boundary Definition

### Core Boundary Principles

**C# Engine (Interpreter/Framework)**:
- Effect handlers (SOURCE, DOWNBASE, EXP modification logic)
- Condition evaluators (TALENT checks, ABL thresholds, FLAG conditions)
- Game loop and state management
- Type safety infrastructure (Strongly Typed IDs, Result<T>)
- DI container and service registration
- Data loaders (IDataLoader implementations)
- Registry systems (ComRegistry, OperatorRegistry)

**YAML Content (Community-Editable)**:
- COM definitions (effect combinations, cost, conditions)
- Character definitions (TALENT combinations, initial stats)
- Kojo dialogue (condition → text mappings)
- Configuration values (costs, thresholds, multipliers)
- Display strings and localization

### Decision Matrix

| Content Type | Format | C# Component | YAML Component | Moddable? |
|--------------|--------|--------------|----------------|:---------:|
| **Kojo Dialogue** | YAML | KojoEngine (renderer) | Dialogue YAML files | ✅ YES |
| **Characters** | YAML | CharacterRepository | Chara*.yaml (existing TALENT combos) | ✅ YES |
| **COM Definitions** (80%) | YAML | Effect handlers (C#) | COM YAML (effect combinations) | ✅ YES |
| **COM Definitions** (20%) | HYBRID | New effect handler (C#) | COM YAML (new effect config) | ⚠️ Requires C# for new effects |
| **Talent Definitions** | C# ENUM | TalentIndex enum | (none) | ❌ NO - C# handler required |
| **Ability Definitions** | C# ENUM | AbilityIndex enum | (none) | ❌ NO - C# handler required |
| **Game Loop Logic** | C# | GameLoop.cs | (none) | ❌ NO - Engine code |
| **Configuration Values** | YAML | IConfigLoader | config.yaml | ⚠️ LIMITED - No new mechanics |

### Examples with Community Moddability Analysis

#### Example 1: Adding New Character (✅ Moddable)

**Action**: Add new character "Yukari" with existing TALENTs.

**Required Steps**:
1. Create `Game/characters/CharaYukari.yaml`
2. Use existing TALENT combinations (巨乳, 従順, etc.)
3. Set initial stats

**C# Compilation Required?** NO ✅

**Why It Works**: Character system uses TALENT combinations. All TALENTs have C# handlers already implemented.

#### Example 2: Adding New COM with Existing Effects (✅ Moddable)

**Action**: Add new COM "Feather Tickling" using existing pain/pleasure effects.

**Required Steps**:
1. Create `Game/data/coms/com250_feather_tickling.yaml`
2. Specify effects using existing types (source, source_scale)
3. Set cost, conditions using existing evaluators

**C# Compilation Required?** NO ✅

**Why It Works**: Effect handlers (SOURCE modification) exist in C#. YAML specifies which effects to combine.

#### Example 3: Adding New TALENT (❌ NOT Moddable)

**Action**: Add new TALENT "超巨乳" (super large breasts).

**Required Steps**:
1. Add to `TalentIndex.cs` enum (C# code)
2. Implement handler in C# for gameplay effects
3. Rebuild project
4. THEN add YAML entries

**C# Compilation Required?** YES ❌

**Why It Fails**: TALENT effects require C# logic (growth calculations, dialogue branching, stat modifications).

**Phantom Moddability**: If TalentIndex was YAML, users could add entries that load but do nothing.

#### Example 4: New COM with New Effect Type (⚠️ Hybrid Moddability)

**Action**: Add COM "Hypnotic Suggestion" with new "mind_control" effect type.

**Required Steps**:
1. Implement C# effect handler for "mind_control" type
2. Register handler in effect registry
3. Rebuild project
4. Create COM YAML using new effect type

**C# Compilation Required?** YES (initially) ⚠️

**Community Impact**: After initial C# implementation, community can create variants using "mind_control" effect.

### Data Placement Strategy

**Revised Policy**:

| Data Category | Primary Format | Secondary Format | Rationale |
|--------------|---------------|------------------|-----------|
| **Kojo Dialogue** | YAML | - | Pure data-driven content, no logic |
| **Character Definitions** | YAML | - | TALENT combinations, genuinely moddable |
| **COM Definitions** | YAML | C# (base classes) | Effect combinations (80% YAML-only) |
| **Effect Handlers** | C# | YAML (config) | Logic in C#, parameters in YAML |
| **Condition Evaluators** | C# | YAML (thresholds) | Logic in C#, thresholds in YAML |
| **TALENT/Ability Enums** | C# ENUM | - | Require C# handlers, not genuinely moddable |
| **FLAG/CFLAG Definitions** | YAML | - | Name mappings, no logic |
| **Configuration Values** | YAML | - | Costs, thresholds, multipliers |
| **Game Mechanics** | C# | - | Core engine logic |

---

## Community Moddability Assessment

### What Works (True Moddability)

| Content Type | Barrier to Entry | Distribution | Example |
|--------------|:----------------:|:------------:|---------|
| **Kojo Dialogue** | 🟢 LOW | Drag & drop YAML | Add new 咲夜 dialogue for COM101 |
| **Character Definitions** | 🟢 LOW | Drag & drop YAML | Add "Yukari" with existing TALENTs |
| **COM Variants** (80%) | 🟢 LOW | Drag & drop YAML | "Gentle Kiss" variant of COM1 |
| **Config Tweaks** | 🟢 LOW | Edit YAML values | Adjust training costs, thresholds |

**Community Workflow**:
1. Download mod YAML file
2. Place in `Game/` directory
3. Launch game
4. Content works immediately

**Distribution Method**: Direct file sharing (Discord, forums) - no centralized approval needed.

### What Doesn't Work (Phantom Moddability)

| Content Type | Apparent Moddability | Actual Barrier | Why It Fails |
|--------------|:--------------------:|----------------|--------------|
| **New TALENTs** | "Just add to YAML!" | C# handler required | TALENT effects need C# logic |
| **New Abilities** | "Just add to YAML!" | C# handler required | Growth calculations in C# |
| **New Effect Types** | "Just add to YAML!" | C# handler required | Effect application is C# code |
| **New Condition Types** | "Just add to YAML!" | C# evaluator required | Condition logic is C# code |

**Problem**: These appear moddable (YAML format) but additions have zero effect without C# changes.

**User Experience**: Frustration when "mod installs but doesn't work."

### Community Moddability scope with examples

#### Tier 1: Fully Moddable (No C# Required)

**Kojo Dialogue Addition**:
```yaml
# Game/kojo/K1_Meiling/com101_kiss.yaml
entries:
  - conditions:
      talent_affection: gte:5
    master_lines:
      - "美鈴、キスしよう"
    partner_lines:
      - "はい、ご主人様..."
```

**Character Addition**:
```yaml
# Game/characters/CharaYukari.yaml
id: 200
name: "八雲紫"
talents:
  - talent_id: 1  # 巨乳
  - talent_id: 5  # 従順
initial_stats:
  affection: 0
  stamina: 100
```

**COM Variant**:
```yaml
# Game/data/coms/com101_gentle_kiss.yaml
id: 101
name: "優しいキス"
category: "愛撫"
cost:
  stamina: 10  # Lower than normal kiss
  energy: 5
effects:
  - type: source
    affection: 50  # More affection gain
```

#### Tier 2: Parameter Moddable (C# Logic Exists, YAML Configures)

**Training Cost Adjustment**:
```yaml
# Game/config/training_costs.yaml
combat_training:
  stamina_cost: 50  # Adjustable
  stat_gain_multiplier: 1.2  # Adjustable
```

**Threshold Tuning**:
```yaml
# Game/config/thresholds.yaml
orgasm_threshold:
  base: 1000  # Adjustable
  talent_multiplier: 1.5  # Adjustable
```

#### Tier 3: NOT Moddable (C# Handler Required)

**New TALENT** (Requires C# for:
- Dialogue branching logic (if talent_超巨乳 then ...)
- Stat growth modifications
- CAN_COM execution conditions
- Visual representation changes

**New Effect Type** (Requires C#):
- Effect application logic (how "mind_control" modifies state)
- Interaction with existing systems
- Validation and constraints

### ERA-style Community Participation Preservation

**Commitment**: Genuinely moddable content types (Tier 1 + Tier 2) maintain ERB-era workflow:

✅ **Preserved**:
- Text file editing (YAML instead of ERB)
- Immediate effect on game reload
- No compilation, build, or PR approval
- Direct file distribution

✅ **Improved**:
- Schema validation catches errors before runtime
- Structured format (YAML) easier than ERB syntax
- IDE support for YAML editing

❌ **Lost** (Intentionally):
- Adding new game mechanics (now requires C# - but this is correct boundary)

---

## Phase 17 Feature Impact Analysis

### Feature Revision Requirements

**Current Phase 17 Features** (F516 Planning + Sub-features):

| Feature | Original Scope | Revision Required? | Recommended Action |
|---------|----------------|:------------------:|-------------------|
| **F528** | VariableSize + GameBase → YAML | ✅ Complete | **KEEP AS-IS** (correct precedent) |
| **F529** | FLAG/CFLAG/TFLAG → YAML | ⚠️ Minor | **REVISE** - Clarify "name mapping only, no logic" |
| **F530** | Talent/Abl → YAML | ❌ Incorrect boundary | **CANCEL** - Keep C# enums (see strategy) |
| **F531** | Palam/Exp/Ex → YAML | ⚠️ Evaluate | **REVISE** - Name mappings OK, calculation constants → C# |
| **F532** | Chara0-13 → YAML | ✅ Correct | **KEEP AS-IS** (genuinely moddable) |
| **F533** | Chara28,29,99,148,149 → YAML | ✅ Correct | **KEEP AS-IS** (genuinely moddable) |
| **F534** | Train/Item/Equip/Tequip → YAML | ⚠️ Partial | **REVISE** - Definitions OK, costs YAML, mechanics C# |
| **F535** | source/Mark/Juel/Stain/Base/Downbase → YAML | ⚠️ Partial | **REVISE** - Name mappings YAML, calc logic C# |
| **F536** | TCVAR/TSTR/CSTR/Str → YAML | ✅ Correct | **KEEP AS-IS** (string tables) |
| **F537** | _Rename/_Replace → YAML | ❌ Unnecessary | **CANCEL** - Minimal data, consolidate to C# |
| **F538** | _default.config/_fixed.config → YAML | ✅ Correct | **KEEP AS-IS** (config files) |
| **F539** | emuera.config → YAML | ⚠️ Evaluate | **REVISE** - Engine settings may stay .config format |
| **F540** | Post-Phase Review | N/A | **UPDATE** - Review against new boundary |

### Detailed Revision Specifications

#### F529: FLAG/CFLAG/TFLAG Migration (REVISE)

**Current Philosophy**:
> "Establish YAML as single source of truth for all ERA configuration data"

**Revised Philosophy**:
> "Migrate variable name definitions to YAML for schema validation and type-safe access. FLAG/CFLAG/TFLAG are name mappings only - no gameplay logic in YAML."

**Scope Clarification**:
- ✅ Variable name definitions → YAML
- ✅ Default values → YAML
- ✅ IDataLoader pattern for type-safe access
- ❌ Flag manipulation logic stays in C# (VariableStore)

**No AC Changes Required** - Original ACs already correct for name mapping migration.

#### F530: Talent/Abl Migration (CANCEL)

**Original Plan**: Talent.csv, Abl.csv → YAML with IDataLoader interface.

**Revised Decision**: **CANCEL** - Keep TalentIndex, AbilityIndex as C# enums.

**Rationale**:
1. **Not Genuinely Moddable** - New talents require C# handler implementation
2. **Phantom Moddability** - YAML additions would load but have zero effect
3. **Type Safety** - C# enum provides compile-time checking
4. **Current Implementation** - TalentIndex.cs (13 files) already exists and works

**Alternative**: F530 scope becomes "Talent/Ability Handler Infrastructure Consolidation"
- Document existing TalentIndex enum (36 talents)
- Document AbilityIndex enum (150 abilities)
- Verify all talents have handlers
- Create handler registry if missing
- Test existing TALENT-based character combinations

**New F530 ACs** (if converted to infrastructure feature):
1. TalentIndex enum documentation complete
2. AbilityIndex enum documentation complete
3. TalentHandler registry verified
4. All 36 talents have handlers
5. Character creation tests (existing TALENT combos) PASS

#### F531: Palam/Exp/Ex Migration (REVISE)

**Current Plan**: Palam.csv, exp.csv, ex.csv → YAML

**Revised Scope**:
- ✅ Parameter name definitions → YAML (PalamIndex, ExpIndex, ExIndex name mappings)
- ✅ Display strings → YAML
- ❌ Growth calculation formulas → C# (e.g., ExperienceGrowthCalculator.cs)
- ❌ Parameter modification logic → C# (e.g., VariableStore parameter access)

**AC Additions**:
- AC: Verify growth calculation logic remains in C# (Grep for ExperienceGrowthCalculator)
- AC: Verify parameter name mappings in YAML (Grep for PalamIndex definitions)

#### F534: Train/Item/Equip/Tequip Migration (REVISE)

**Current Plan**: Train.csv (204), Item.csv (73), Equip.csv (156), Tequip.csv (64) → YAML

**Revised Scope**:
- ✅ Item definitions (name, description, cost) → YAML
- ✅ Equipment stats (defense, attack) → YAML
- ✅ Training costs → YAML
- ❌ Training execution logic → C# (TrainingProcessor.cs)
- ❌ Equipment effect handlers → C# (EquipmentProcessor.cs)
- ❌ Item usage logic → C# (ItemHandler.cs)

**AC Additions**:
- AC: Verify training execution logic in C# (Grep for TrainingProcessor)
- AC: Verify equipment effect handlers in C# (Grep for EquipmentProcessor)
- AC: Training cost YAML modifiable without C# rebuild (test)

#### F535: source/Mark/Juel/Stain/Base/Downbase Migration (REVISE)

**Current Plan**: 6 CSV files → YAML

**Revised Scope**:
- ✅ Name mappings → YAML (SourceIndex, MarkIndex, StainIndex name strings)
- ✅ Display strings → YAML
- ❌ SOURCE effect application → C# (SourceProcessor.cs)
- ❌ Mark calculation logic → C# (MarkSystem.cs)
- ❌ Stain accumulation logic → C# (StainProcessor.cs)

**AC Additions**:
- AC: Verify effect application logic in C# (Grep for SourceProcessor, MarkSystem)
- AC: Verify name mapping separation from logic (architecture test)

#### F537: Transform Rules Migration (CANCEL)

**Original Plan**: _Rename.csv, _Replace.csv → YAML with ITransformRuleLoader

**Revised Decision**: **CANCEL** - Files contain minimal/zero data.

**Alternative Actions**:
1. Delete _Rename.csv (zero data, only comments)
2. Consolidate _Replace.csv single rule into ConfigData.cs:
   ```csharp
   // src/Era.Core/Common/ConfigData.cs
   public static readonly int[] StainDefaultValues = [0, 0, 2, 1, 8, 0, 1, 8];
   ```
3. Remove transform rule loading infrastructure
4. Update emuera.config to remove transform rule references

**New F537 Scope**: "Transform Rules Cleanup"
- AC1: _Rename.csv deleted
- AC2: _Replace.csv deleted
- AC3: StainDefaultValues in ConfigData.cs
- AC4: emuera.config transform references removed
- AC5: Build succeeds

#### F539: emuera.config Migration (REVISE)

**Current Plan**: emuera.config → YAML

**Revised Evaluation**: Engine configuration files may stay `.config` format.

**Decision Criteria**:
1. If emuera.config contains engine startup parameters → Keep `.config` format
2. If contains gameplay configuration → Migrate to YAML
3. Mixed content → Split into engine.config + gameplay.yaml

**Action**: F563 to analyze emuera.config content and decide.

#### F540: Post-Phase Review (UPDATE)

**Required Updates**:
1. Review checklist to include "C#/YAML boundary compliance"
2. Verify YAML files are genuinely moddable (not phantom)
3. Verify C# logic not leaked into YAML
4. Test community workflow (edit YAML → reload → works)

---

## COM C# Class Disposition Analysis

This section analyzes COM class disposition - determining which migrate to YAML vs stay in C#.

### Category-Level Disposition

**Training COM Classes** (90+ files across Touch/Oral/Penetration/Equipment/Bondage/Undressing subdirectories):

**Disposition**: **MIGRATE TO YAML** (80% feasibility)

**Analysis**:
- Most use existing effect combinations (SOURCE, DOWNBASE, EXP modifications)
- Cost, conditions, effect magnitudes differ - YAML can specify
- C# base class (ComBase) provides execution framework
- Effect handlers (SourceProcessor, ExperienceProcessor) stay in C#

**Migration Pattern**:
```csharp
// Before: src/Era.Core/Commands/Com/Training/Touch/Kiss.cs
[ComId(1)]
public class Kiss : ComBase
{
    public override ComResult Execute(IComContext context)
    {
        // 50 lines of effect application
        context.ApplySource(SourceIndex.Affection, 100);
        context.ApplySource(SourceIndex.Pleasure, 50);
        // ...
    }
}

// After: Game/data/coms/com001_kiss.yaml
id: 1
name: "キス"
category: "愛撫"
cost:
  stamina: 15
  energy: 10
effects:
  - type: source
    affection: 100
    pleasure: 50
conditions:
  - type: not_has_talent
    talent: 拒絶
```

**Individual COM Disposition**: Delegated to F563 implementation (analyze 150+ files individually).

### Hybrid Case: Utility COM Classes

**Example: CombatTraining.cs**

**Current Implementation**: C# class with hardcoded training costs and stat gains.

**Hybrid Approach**:
- **C# (Engine)**: Training execution logic, stat increase calculation, validation
- **YAML (Content)**: Training costs, base stat gains, talent modifiers

```csharp
// src/Era.Core/Commands/Com/Utility/CombatTraining.cs (simplified)
public class CombatTraining(ICombatTrainingConfig config) : ComBase
{
    public override ComResult Execute(IComContext context)
    {
        var cost = config.StaminaCost;  // From YAML
        var statGain = config.BaseStatGain * GetTalentMultiplier();  // Logic C#, base YAML
        context.ConsumeStamina(cost);
        context.IncreaseAbility(AbilityIndex.Combat, statGain);
        return ComResult.Success();
    }
}

// Game/config/combat_training.yaml
stamina_cost: 80
energy_cost: 30
base_stat_gain: 5
talent_modifiers:
  - talent: 武闘派
    multiplier: 1.5
```

**Benefit**: Community can adjust training costs/gains without C# changes.

### Keep C# Cases

**System COM Classes** (DayEnd, Dummy):
- **Reason**: Engine control flow, not content
- **Disposition**: KEEP C#

**Visitor COM Classes** (InviteVisitor, GuideVisitor, etc.):
- **Reason**: Complex state management (guest tracking, location logic, schedule coordination)
- **Disposition**: KEEP C#

**COM Infrastructure** (ComBase, ComRegistry, IComContext, etc.):
- **Reason**: Engine foundation for all COM execution
- **Disposition**: KEEP C#

### Summary Table

| COM C# Class Category | Files | Disposition | Migration Priority | Effort Estimate |
|----------------------|:-----:|-------------|:------------------:|:---------------:|
| Training/Touch | 14 | MIGRATE TO YAML | HIGH | M (2-3 days) |
| Training/Oral | 17 | MIGRATE TO YAML | HIGH | M (2-3 days) |
| Training/Penetration | 25 | MIGRATE TO YAML | HIGH | L (4-5 days) |
| Training/Equipment | 17 | MIGRATE TO YAML | HIGH | M (2-3 days) |
| Training/Bondage | 11 | MIGRATE TO YAML | MEDIUM | M (2-3 days) |
| Training/Undressing | 4 | MIGRATE TO YAML | MEDIUM | S (<1 day) |
| Training/Utility | 2 | MIGRATE TO YAML | MEDIUM | S (<1 day) |
| Daily | 17 | MIGRATE TO YAML | HIGH | M (2-3 days) |
| Masturbation | 17 | MIGRATE TO YAML | MEDIUM | M (2-3 days) |
| Utility | 21 | HYBRID | LOW | L (4-5 days) - Extract config |
| Visitor | 4 | KEEP C# | N/A | - |
| System | 2 | KEEP C# | N/A | - |
| Infrastructure | 10 | KEEP C# | N/A | - |

**Total Migration Effort**: ~20-25 days for COM C# → YAML migration (150+ files)

---

## Migration Path: C# to YAML

### Step-by-Step Migration Process

#### Phase 1: Schema Design

**Input**: Existing C# COM class (e.g., Kiss.cs)

**Actions**:
1. Analyze COM class structure (cost, conditions, effects)
2. Design YAML schema with JSON Schema validation
3. Create representative YAML examples
4. Validate schema against examples

**Output**: `src/tools/schemas/com.schema.json`

**Example**:
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "id": { "type": "integer" },
    "name": { "type": "string" },
    "category": { "type": "string" },
    "cost": {
      "type": "object",
      "properties": {
        "stamina": { "type": "integer" },
        "energy": { "type": "integer" }
      }
    },
    "effects": {
      "type": "array",
      "items": {
        "type": "object",
        "properties": {
          "type": { "enum": ["source", "source_scale", "downbase", "exp"] },
          "target": { "type": "string" }
        }
      }
    }
  }
}
```

#### Phase 2: Effect Handler Infrastructure

**Input**: COM C# classes with diverse effect patterns

**Actions**:
1. Extract common effect types (source, source_scale, downbase, exp, etc.)
2. Implement effect handler registry
3. Create IEffectHandler interface
4. Implement handlers for each effect type

**Output**: `src/Era.Core/Effects/` directory with effect handler infrastructure

**Example**:
```csharp
// src/Era.Core/Effects/IEffectHandler.cs
public interface IEffectHandler
{
    string EffectType { get; }
    Result<Unit> Apply(EffectConfig config, IComContext context);
}

// src/Era.Core/Effects/SourceEffectHandler.cs
public class SourceEffectHandler : IEffectHandler
{
    public string EffectType => "source";

    public Result<Unit> Apply(EffectConfig config, IComContext context)
    {
        var sourceIndex = ParseSourceIndex(config.Target);
        var value = config.GetValue("value");
        context.ApplySource(sourceIndex, value);
        return Result<Unit>.Ok(Unit.Value);
    }
}
```

#### Phase 3: YAML COM Loader

**Input**: YAML schema + Effect handler infrastructure

**Actions**:
1. Implement IComLoader interface
2. Parse YAML COM files
3. Validate against schema
4. Construct ComDefinition objects

**Output**: `src/Era.Core/Data/YamlComLoader.cs`

**Example**:
```csharp
// src/Era.Core/Data/IComLoader.cs
public interface IComLoader
{
    Result<IReadOnlyList<ComDefinition>> LoadAll(string directory);
    Result<ComDefinition> Load(string path);
}

// src/Era.Core/Data/YamlComLoader.cs
public class YamlComLoader(IYamlParser parser, ISchemaValidator validator) : IComLoader
{
    public Result<ComDefinition> Load(string path)
    {
        var yamlResult = parser.Parse<ComYaml>(path);
        if (!yamlResult.IsSuccess) return Result<ComDefinition>.Fail(yamlResult.Error);

        var validationResult = validator.Validate(yamlResult.Value, "com.schema.json");
        if (!validationResult.IsSuccess) return Result<ComDefinition>.Fail(validationResult.Error);

        return Result<ComDefinition>.Ok(MapToDefinition(yamlResult.Value));
    }
}
```

#### Phase 4: Dynamic COM Execution

**Input**: YAML COM loader + Effect handler registry

**Actions**:
1. Modify ComRegistry to accept YAML-loaded COMs
2. Implement dynamic COM executor
3. Route effect execution to effect handlers
4. Preserve ComResult return pattern

**Output**: `src/Era.Core/Commands/Com/YamlCom.cs`

**Example**:
```csharp
// src/Era.Core/Commands/Com/YamlCom.cs
public class YamlCom(ComDefinition definition, IEffectHandlerRegistry effectRegistry) : ComBase
{
    public override ComId Id => definition.Id;

    public override ComResult Execute(IComContext context)
    {
        // Check conditions
        if (!EvaluateConditions(definition.Conditions, context))
            return ComResult.Failure("Conditions not met");

        // Consume costs
        context.ConsumeStamina(definition.Cost.Stamina);
        context.ConsumeEnergy(definition.Cost.Energy);

        // Apply effects
        foreach (var effect in definition.Effects)
        {
            var handler = effectRegistry.Get(effect.Type);
            var result = handler.Apply(effect, context);
            if (!result.IsSuccess) return ComResult.Failure(result.Error);
        }

        return ComResult.Success();
    }
}
```

#### Phase 5: Equivalence Testing

**Input**: Original C# COM + YAML COM definition

**Actions**:
1. Create test harness with identical game state
2. Execute C# COM, record state changes
3. Execute YAML COM, record state changes
4. Assert state changes are identical

**Output**: `src/Era.Core.Tests/Com/ComEquivalenceTests.cs`

**Example**:
```csharp
[Test]
public void TestKissComEquivalence()
{
    // Arrange
    var csharpCom = new Kiss();  // Original C# implementation
    var yamlCom = _comLoader.Load("Game/data/coms/com001_kiss.yaml");
    var state = CreateTestGameState();

    // Act
    var csharpContext = new ComContext(state.Clone());
    var yamlContext = new ComContext(state.Clone());

    csharpCom.Execute(csharpContext);
    yamlCom.Execute(yamlContext);

    // Assert
    Assert.Equal(csharpContext.GetSource(SourceIndex.Affection),
                 yamlContext.GetSource(SourceIndex.Affection));
    Assert.Equal(csharpContext.GetStamina(), yamlContext.GetStamina());
    // ... all state comparisons
}
```

#### Phase 6: Gradual Replacement

**Input**: Equivalence tests passing

**Actions**:
1. Register YAML COM in ComRegistry (keep C# COM temporarily)
2. Add feature flag for YAML COM execution
3. Run integration tests with YAML COMs
4. Monitor for regressions

**Output**: Hybrid COM registry supporting both C# and YAML COMs

#### Phase 7: C# COM Deletion

**Input**: YAML COMs proven equivalent + Integration tests passing

**Actions**:
1. Delete C# COM class file (e.g., Kiss.cs)
2. Remove from ComRegistry
3. Run full test suite
4. Verify no references remain

**Output**: C# COM class removed, YAML COM fully operational

### Migration Tools

**Required Tools**:

| Tool | Purpose | Implementation Location |
|------|---------|------------------------|
| **ComAnalyzer** | Extract COM structure from C# classes | tools/ComAnalyzer/ |
| **SchemaGenerator** | Generate JSON Schema from COM patterns | tools/YamlSchemaGen/ (extend existing) |
| **ComToYaml** | Semi-automated C# → YAML conversion | tools/ComToYaml/ |
| **EquivalenceTester** | Automated equivalence verification | src/Era.Core.Tests/Com/Equivalence/ |

**ComToYaml Tool Design**:
```bash
# Usage
dotnet run --project tools/ComToYaml -- \
  --input src/Era.Core/Commands/Com/Training/Touch/ \
  --output Game/data/coms/training/touch/ \
  --verify

# Output
Analyzing: Kiss.cs... ✓
Generating: com001_kiss.yaml... ✓
Schema validation... ✓
Equivalence test... ✓ (100% match)

Analyzing: BreastCaress.cs... ✓
Generating: com002_breast_caress.yaml... ✓
Schema validation... ✓
Equivalence test... ⚠ (98% match - manual review required)
```

### Migration Checklist (per COM)

- [ ] COM structure analyzed
- [ ] YAML file generated
- [ ] Schema validation PASS
- [ ] Equivalence test PASS (100% state match)
- [ ] Integration test PASS
- [ ] Feature flag enabled
- [ ] No regressions observed (1 week monitoring)
- [ ] C# file deleted
- [ ] No references remain (Grep verification)
- [ ] Documentation updated

---

## Phantom Moddability Prevention Strategy

### Problem Statement

**Phantom Moddability**: Content appears moddable (editable YAML) but additions have zero effect without C# code changes.

**User Experience**:
1. User downloads "Super Breasts Talent Mod"
2. Adds YAML entry: `{ id: 999, name: "超巨乳" }`
3. Game loads without error ✓
4. Talent selection shows "超巨乳" ✓
5. **Gameplay effect: ZERO** ❌
6. User frustrated: "Mod doesn't work!"

**Root Cause**: YAML format suggests moddability, but C# handler is required for actual functionality.

### Prevention Measures

#### Measure 1: Schema-Level Warnings

**Implementation**: Add `x-modding-level` extension to JSON Schema.

**Example**:
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "Talent Definition",
  "x-modding-level": "not-moddable",
  "x-modding-warning": "Adding new talents requires C# handler implementation. See docs/modding-guide.md",
  "properties": {
    "id": { "type": "integer" },
    "name": { "type": "string" }
  }
}
```

**Validator Output**:
```
⚠️  WARNING: talent.yaml has modding level "not-moddable"
Adding new talents requires C# handler implementation.
See docs/modding-guide.md for details.
```

#### Measure 2: Runtime Validation with User-Friendly Errors

**Implementation**: Detect YAML additions that lack C# handlers.

**Example**:
```csharp
// src/Era.Core/Validation/ModdingValidator.cs
public class ModdingValidator
{
    public Result<Unit> ValidateTalent(TalentDefinition talent)
    {
        var hasHandler = _talentHandlerRegistry.HasHandler(talent.Id);
        if (!hasHandler)
        {
            return Result<Unit>.Fail(
                $"Talent '{talent.Name}' (ID: {talent.Id}) has no C# handler. " +
                $"This talent will not function in gameplay. " +
                $"To add new talents, see docs/developer-guide.md#adding-talents"
            );
        }
        return Result<Unit>.Ok(Unit.Value);
    }
}
```

**Startup Log**:
```
[ERROR] Game initialization failed:
Talent '超巨乳' (ID: 999) has no C# handler.
This talent will not function in gameplay.
To add new talents, see docs/developer-guide.md#adding-talents

Remove the talent from talent.yaml to continue.
```

#### Measure 3: Documentation with Clear Moddability Tiers

**Location**: `docs/modding-guide.md`

**Content Structure**:
```markdown
# Modding Guide

## Moddability Tiers

### ✅ Tier 1: Fully Moddable (No C# Required)
- Kojo Dialogue
- Character Definitions (using existing TALENTs)
- COM Variants (using existing effects)

**How to Mod**:
1. Create/edit YAML file
2. Place in Game/ directory
3. Launch game
4. Works immediately

### ⚠️ Tier 2: Parameter Moddable (C# Logic Exists)
- Training costs
- Effect magnitudes
- Thresholds

**How to Mod**:
1. Edit existing YAML values
2. Launch game
3. Adjusted parameters take effect

### ❌ Tier 3: NOT Moddable (C# Handler Required)
- New TALENTs
- New Abilities
- New Effect Types

**Why Not Moddable**:
These require gameplay logic implementation in C# code.
Adding YAML entries without C# handlers will not work.

**For Developers**:
See `docs/developer-guide.md#extending-talents` for C# implementation guide.
```

#### Measure 4: Separate Directories for Moddable vs. Non-Moddable Data

**Directory Structure**:
```
Game/
├── content/              # Tier 1: Fully Moddable
│   ├── kojo/
│   ├── characters/
│   └── coms/
├── config/               # Tier 2: Parameter Moddable
│   ├── training_costs.yaml
│   ├── thresholds.yaml
│   └── multipliers.yaml
└── definitions/          # Tier 3: NOT Moddable (Developer-Only)
    ├── talents.yaml      # ⚠️ Adding entries requires C# handler
    ├── abilities.yaml    # ⚠️ Adding entries requires C# handler
    └── effect_types.yaml # ⚠️ Adding entries requires C# handler
```

**README in definitions/**:
```
⚠️  WARNING: Files in this directory are NOT fully moddable.

Adding new entries to talents.yaml, abilities.yaml, or effect_types.yaml
requires corresponding C# handler implementation.

These files are provided in YAML format for:
- Schema validation
- Type-safe data loading
- Easy editing of existing entries

To add new talents/abilities/effects, see docs/developer-guide.md
```

#### Measure 5: Registry Validation on Startup

**Implementation**: Validate all registered data has required handlers.

**Example**:
```csharp
// src/Era.Core/Startup/RegistryValidator.cs
public class RegistryValidator
{
    public Result<Unit> ValidateOnStartup()
    {
        var errors = new List<string>();

        // Validate Talents
        foreach (var talent in _talentLoader.LoadAll())
        {
            if (!_talentHandlerRegistry.HasHandler(talent.Id))
                errors.Add($"Talent '{talent.Name}' (ID: {talent.Id}) missing handler");
        }

        // Validate Effects
        foreach (var effect in _effectLoader.LoadAll())
        {
            if (!_effectHandlerRegistry.HasHandler(effect.Type))
                errors.Add($"Effect type '{effect.Type}' missing handler");
        }

        if (errors.Any())
        {
            return Result<Unit>.Fail(
                "Startup validation failed:\n" +
                string.Join("\n", errors) +
                "\n\nSee docs/modding-guide.md for proper modding workflow."
            );
        }

        return Result<Unit>.Ok(Unit.Value);
    }
}
```

### Verification Method

**Acceptance Test**: Phantom Moddability Prevention

```csharp
[Test]
public void TestPhantomModdabilityPrevention()
{
    // Arrange: Add phantom talent to YAML
    var yamlContent = @"
        - id: 999
          name: ""超巨乳""
          description: ""Phantom talent with no handler""
    ";
    File.WriteAllText("Game/definitions/talents.yaml", yamlContent);

    // Act: Try to start game
    var result = _gameInitializer.Initialize();

    // Assert: Startup fails with clear error
    Assert.False(result.IsSuccess);
    Assert.Contains("Talent '超巨乳' (ID: 999) missing handler", result.Error);
    Assert.Contains("docs/modding-guide.md", result.Error);
}
```

---

## Recommendations for F563 Implementation

### F563 Scope Definition

**Feature Name**: Architecture Implementation: C# Engine + YAML Content Separation

**Type**: infra

**Summary**: Implement architecture.md updates based on F562 analysis findings, execute Phase 17 feature revisions, and migrate COM C# classes to YAML format following established boundary principles.

### Section 1: architecture.md Changes (Concrete Text Proposals)

#### Change 1.1: Update Phase 17 Philosophy

**Location**: `designs/full-csharp-architecture.md` - Phase 17 section, line ~3738

**Current Text**:
```markdown
### Phase 17: Data Migration (was Phase 16)

**Goal**: CSV/定義データをYAML/JSONに変換（詳細マッピング付き）
```

**Replace With**:
```markdown
### Phase 17: Data Migration (was Phase 16)

**Goal**: Migrate genuinely moddable content and configuration data to YAML while keeping engine-dependent definitions in C# for type safety and handler enforcement.

**Boundary Principle**: C# Engine (logic, effect handlers, condition evaluators) vs YAML Content (COM definitions, character data, kojo dialogue). Community moddability requires preserving ERA-style text file editing workflow - "edit YAML → reload game → works" for Tier 1 content.
```

#### Change 1.2: Add Data Placement Strategy Section

**Location**: After Phase 17 **Goal** section

**Add New Section**:
```markdown
### Data Placement Strategy (Revised 2026-01-19)

| Data Category | Format | Rationale | Moddability Tier |
|--------------|--------|-----------|:----------------:|
| **Kojo Dialogue** | YAML | Pure data-driven, no logic required | ✅ Tier 1 |
| **Character Definitions** | YAML | TALENT combinations, genuinely moddable | ✅ Tier 1 |
| **COM Definitions** | YAML | Effect combinations (80% YAML-only) | ✅ Tier 1 |
| **Configuration Values** | YAML | Costs, thresholds, multipliers | ⚠️ Tier 2 |
| **Effect Handlers** | C# | Application logic with YAML parameters | C# Engine |
| **Condition Evaluators** | C# | Evaluation logic with YAML thresholds | C# Engine |
| **TALENT/Ability Enums** | C# ENUM | Require C# handlers, not moddable | ❌ Tier 3 |
| **FLAG/CFLAG Names** | YAML | Name mappings, no logic | ⚠️ Tier 2 |

**Moddability Tier Legend**:
- **Tier 1 (✅)**: Fully moddable - No C# compilation required for additions
- **Tier 2 (⚠️)**: Parameter moddable - Can adjust values, cannot add mechanics
- **Tier 3 (❌)**: NOT moddable - C# handler required for additions

**Phantom Moddability Prevention**: Files in Tier 3 include schema warnings and startup validation to prevent non-functional YAML additions.
```

#### Change 1.3: Update Migration Order Table

**Location**: Phase 17 **Migration Order** table, line ~3776

**Current Row**:
```markdown
| 10 | `Talent.csv` | 素質定義 (50) | HIGH |
| 11 | `Abl.csv` | 能力定義 | HIGH |
```

**Replace With**:
```markdown
| 10 | `Talent.csv` | 素質名定義 (name mappings only) | HIGH |
| 11 | `Abl.csv` | 能力名定義 (name mappings only) | HIGH |
```

**Add Note After Table**:
```markdown
**Note on Talent/Ability Migration**: Talent.csv and Abl.csv migrate NAME DEFINITIONS only. TalentIndex and AbilityIndex remain C# enums - new talents/abilities require C# handler implementation (not genuinely moddable).
```

#### Change 1.4: Add Community Moddability Scope Section

**Location**: After Phase 17 **Deliverables** section

**Add New Section**:
```markdown
### Community Moddability Scope (Added 2026-01-19)

**Genuinely Moddable Content** (Tier 1 - No C# Required):

**Examples**:
1. **Kojo Dialogue Addition**:
   ```yaml
   # Game/kojo/K1_Meiling/com101_kiss.yaml
   entries:
     - conditions:
         talent_affection: gte:5
       master_lines:
         - "美鈴、キスしよう"
   ```

2. **Character Addition**:
   ```yaml
   # Game/characters/CharaYukari.yaml
   id: 200
   name: "八雲紫"
   talents: [1, 5]  # Existing TALENTs
   ```

3. **COM Variant**:
   ```yaml
   # Game/data/coms/com101_gentle_kiss.yaml
   id: 101
   name: "優しいキス"
   effects:
     - type: source
       affection: 50
   ```

**NOT Moddable Content** (Tier 3 - C# Handler Required):

**Why These Require C#**:
- **New TALENTs**: Gameplay effects, dialogue branching, stat modifications
- **New Abilities**: Growth calculations, training modifiers
- **New Effect Types**: Effect application logic, system interactions

**Phantom Moddability Prevention**: See docs/modding-guide.md for proper modding workflow and tier explanations.
```

#### Change 1.5: Update Test Infrastructure Transition Section

**Location**: Phase 18 (former Phase 17) section, after Phase 17 completion

**Add to Trigger Conditions**:
```markdown
**Trigger**: Phase 18 (Kojo Conversion) completion **AND** Phase 17 community moddability validation

**Additional Validation Steps**:
- Community workflow test: Edit YAML → reload → verify functionality
- Phantom moddability prevention test: Add invalid YAML → verify clear error message
- Tier 1 content test: Add new character/kojo/COM variant without C# rebuild
```

### Section 2: Phase 17 Feature Revisions (Specific Changes)

#### F530: Talent/Abl Migration (CANCEL → CONVERT)

**Action**: Convert to infrastructure consolidation feature

**New Title**: "Talent/Ability Handler Infrastructure Consolidation"

**New Philosophy**:
```markdown
**Phase 17: Data Migration** - Document and verify existing TalentIndex/AbilityIndex C# enum infrastructure. TALENT and Ability definitions remain C# enums because new additions require gameplay logic handlers (not genuinely moddable). Ensure all talents have handlers and test existing TALENT-based character combinations.
```

**New ACs** (replace all existing ACs):
```markdown
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | TalentIndex enum documented | file | Glob | exists | "src/Era.Core/Types/TalentIndex.cs" | [ ] |
| 2 | AbilityIndex enum documented | file | Glob | exists | "src/Era.Core/Types/AbilityIndex.cs" | [ ] |
| 3 | TalentHandler registry exists | code | Grep | contains | "class TalentHandlerRegistry" | [ ] |
| 4 | All 36 talents have handlers | test | Bash | succeeds | "dotnet test --filter TestAllTalentsHaveHandlers" | [ ] |
| 5 | Character creation with existing TALENTs | test | Bash | succeeds | "dotnet test --filter TestCharacterTalentCombinations" | [ ] |
| 6 | TALENT moddability tier documented | file | Grep | contains | "Tier 3.*NOT moddable.*C# handler required" | [ ] |
```

**New Tasks**:
```markdown
| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Document existing TalentIndex/AbilityIndex enums | [ ] |
| 2 | 3 | Verify TalentHandler registry exists or create | [ ] |
| 3 | 4 | Test all 36 talents have registered handlers | [ ] |
| 4 | 5 | Create character combination tests (existing TALENTs) | [ ] |
| 5 | 6 | Document TALENT as Tier 3 (not moddable) in modding guide | [ ] |
```

#### F531: Palam/Exp/Ex Migration (REVISE)

**Add to Philosophy**:
```markdown
**Boundary Clarification**: Palam/Exp/Ex CSV files migrate NAME DEFINITIONS and display strings to YAML. Growth calculation formulas remain in C# (ExperienceGrowthCalculator.cs). Parameter modification logic remains in VariableStore.
```

**Add New ACs**:
```markdown
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| ... | (existing ACs) | ... | ... | ... | ... | [ ] |
| 21 | Growth calculation in C# verified | code | Grep | contains | "class ExperienceGrowthCalculator" | [ ] |
| 22 | Parameter modification in C# verified | code | Grep | contains | "class VariableStore.*SetParameter" | [ ] |
```

#### F534: Train/Item/Equip/Tequip Migration (REVISE)

**Add to Philosophy**:
```markdown
**Boundary Clarification**: Train/Item/Equip/Tequip CSV files migrate DEFINITIONS (names, costs, stats) to YAML. Training execution logic remains in TrainingProcessor.cs. Equipment effect handlers remain in EquipmentProcessor.cs. Item usage logic remains in ItemHandler.cs.
```

**Add New ACs**:
```markdown
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| ... | (existing ACs) | ... | ... | ... | ... | [ ] |
| 25 | Training execution logic in C# verified | code | Grep | contains | "class TrainingProcessor" | [ ] |
| 26 | Equipment effect handlers in C# verified | code | Grep | contains | "class EquipmentProcessor" | [ ] |
| 27 | Item usage logic in C# verified | code | Grep | contains | "class ItemHandler" | [ ] |
| 28 | Training cost YAML modifiable test | test | Bash | succeeds | "Edit training_costs.yaml → reload → costs changed" | [ ] |
```

#### F535: source/Mark/Juel/Stain Migration (REVISE)

**Add to Philosophy**:
```markdown
**Boundary Clarification**: source/Mark/Juel/Stain CSV files migrate NAME MAPPINGS to YAML. SOURCE effect application remains in SourceProcessor.cs. Mark calculation logic remains in MarkSystem.cs. Stain accumulation logic remains in StainProcessor.cs.
```

**Add New ACs**:
```markdown
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| ... | (existing ACs) | ... | ... | ... | ... | [ ] |
| 21 | SOURCE effect application in C# verified | code | Grep | contains | "class SourceProcessor" | [ ] |
| 22 | Mark calculation in C# verified | code | Grep | contains | "class MarkSystem" | [ ] |
| 23 | Name mapping separation verified | test | Bash | succeeds | "Architecture test: Logic not in YAML" | [ ] |
```

#### F537: Transform Rules Migration (CANCEL → REPLACE)

**Action**: Replace with cleanup feature

**New Title**: "Transform Rules Cleanup - Phase 17"

**New Philosophy**:
```markdown
**Phase 17: Data Migration** - Remove unnecessary transform rule CSV files (_Rename.csv has zero data, _Replace.csv has one value better placed in C# constants). Consolidate汚れの初期値 into ConfigData.cs. Eliminate CSV parsing overhead for minimal-data files.
```

**New ACs** (replace all existing):
```markdown
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | _Rename.csv deleted | file | Glob | not_exists | "Game/CSV/_Rename.csv" | [ ] |
| 2 | _Replace.csv deleted | file | Glob | not_exists | "Game/CSV/_Replace.csv" | [ ] |
| 3 | StainDefaultValues in C# | code | Grep | contains | "StainDefaultValues.*=.*\\[0, 0, 2, 1, 8, 0, 1, 8\\]" | [ ] |
| 4 | emuera.config references removed | code | Grep | not_contains | "_Rename|_Replace" | [ ] |
| 5 | Build succeeds | build | Bash | succeeds | "dotnet build engine/uEmuera.Headless.csproj" | [ ] |
| 6 | All tests PASS | test | Bash | succeeds | "dotnet test Era.Core.Tests" | [ ] |
```

**New Tasks**:
```markdown
| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Delete _Rename.csv and _Replace.csv | [ ] |
| 2 | 3 | Add StainDefaultValues to ConfigData.cs | [ ] |
| 3 | 4 | Remove transform rule references from emuera.config | [ ] |
| 4 | 5,6 | Verify build and tests | [ ] |
```

#### F539: emuera.config Migration (ADD ANALYSIS TASK)

**Add New Task**:
```markdown
| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | - | Analyze emuera.config content structure | [ ] |
| 2 | - | Determine engine settings vs gameplay config split | [ ] |
| ... | ... | (existing tasks) | [ ] |
```

**Add to Implementation Contract**:
```markdown
**emuera.config Analysis Requirements**:
1. Categorize each setting: Engine startup vs Gameplay config
2. Decision criteria:
   - Engine startup parameters → Keep `.config` format
   - Gameplay configuration → Migrate to YAML
   - Mixed content → Split into engine.config + gameplay.yaml
3. Document decision rationale in Execution Log
```

#### F540: Post-Phase Review (UPDATE CHECKLIST)

**Add to Review Checklist**:
```markdown
| Check | Question | Action if NO |
|-------|----------|--------------|
| ... | (existing checks) | ... |
| **C#/YAML Boundary** | YAML files contain only data, no logic? | Fix logic leak to C# |
| **Phantom Prevention** | Tier 3 files have warnings and validation? | Add validation |
| **Community Workflow** | Tier 1 content works without C# rebuild? | Fix loading pipeline |
```

### Section 3: Migration Tasks (Effort Estimates)

**Priority Order for Execution**:

| Priority | Task | Effort | Dependencies | Rationale |
|:--------:|------|:------:|--------------|-----------|
| 1 | Architecture.md updates (Section 1) | S (<1 day) | None | Foundation for all other work |
| 2 | F537 Transform Rules Cleanup | S (<1 day) | None | Quick win, removes unnecessary files |
| 3 | F530 Talent/Ability Infrastructure | S (<1 day) | None | Clarifies non-moddability |
| 4 | F529 FLAG/CFLAG/TFLAG Revision | S (<1 day) | F528 complete | Add boundary clarification |
| 5 | F531 Palam/Exp/Ex Revision | S (<1 day) | F529 complete | Add AC for C# logic verification |
| 6 | F534 Train/Item/Equip/Tequip Revision | M (1-2 days) | F531 complete | Add ACs for logic/data separation |
| 7 | F535 source/Mark/Juel/Stain Revision | M (1-2 days) | F534 complete | Add ACs for effect handler verification |
| 8 | F539 emuera.config Analysis | S (<1 day) | F535 complete | Determine migration strategy |
| 9 | Modding Guide Creation | M (2-3 days) | Architecture updates | Tier documentation |
| 10 | COM Migration Infrastructure | L (4-5 days) | F535 complete | Schema, handlers, loader |
| 11 | COM Training/Touch Migration | M (2-3 days) | COM infrastructure | 14 files |
| 12 | COM Training/Oral Migration | M (2-3 days) | COM infrastructure | 17 files |
| 13 | COM Training/Penetration Migration | L (4-5 days) | COM infrastructure | 25 files |
| 14 | COM Training/Equipment Migration | M (2-3 days) | COM infrastructure | 17 files |
| 15 | COM Daily Migration | M (2-3 days) | COM infrastructure | 17 files |
| 16 | COM Masturbation Migration | M (2-3 days) | COM infrastructure | 17 files |
| 17 | COM Utility Hybridization | L (4-5 days) | COM infrastructure | Extract config from 21 files |
| 18 | F540 Post-Phase Review Update | S (<1 day) | All Phase 17 | Add boundary checks |

**Total Effort Estimate**: ~30-40 days for full Phase 17 implementation with COM migration

**Milestone Breakdown**:

| Milestone | Tasks | Duration | Deliverables |
|-----------|-------|:--------:|--------------|
| **M1: Foundation** | Tasks 1-5 | 5 days | Architecture updated, F537/F530/F529 complete |
| **M2: Data Migration** | Tasks 6-8 | 5 days | F534/F535/F539 complete |
| **M3: Documentation** | Task 9 | 3 days | Modding guide published |
| **M4: COM Infrastructure** | Task 10 | 5 days | COM YAML system operational |
| **M5: COM Migration Phase 1** | Tasks 11-14 | 12 days | Training COMs migrated |
| **M6: COM Migration Phase 2** | Tasks 15-17 | 10 days | Daily/Masturbation/Utility migrated |
| **M7: Validation** | Task 18 | 1 day | Post-phase review complete |

### Section 4: Phantom Moddability Prevention (Implementation Steps)

**Step 4.1: Schema Extension Implementation**

**Location**: Create `tools/YamlSchemaGen/ModdingLevelExtension.cs`

**Code**:
```csharp
public class ModdingLevelExtension
{
    public enum ModdingLevel
    {
        FullyModdable,      // Tier 1
        ParameterModdable,  // Tier 2
        NotModdable         // Tier 3
    }

    public static void AddModdingLevel(JsonSchema schema, ModdingLevel level, string warningMessage)
    {
        schema.ExtensionData = new Dictionary<string, object>
        {
            ["x-modding-level"] = level.ToString(),
            ["x-modding-warning"] = warningMessage
        };
    }
}
```

**Usage**:
```csharp
// Generate talent schema with warning
var talentSchema = new JsonSchema();
ModdingLevelExtension.AddModdingLevel(
    talentSchema,
    ModdingLevel.NotModdable,
    "Adding new talents requires C# handler implementation. See docs/modding-guide.md"
);
```

**Verification Method**: Schema validation outputs warning when loading Tier 3 files.

**Step 4.2: Runtime Validation Implementation**

**Location**: Create `src/Era.Core/Validation/ModdingValidator.cs`

**Code**:
```csharp
public class ModdingValidator(
    ITalentHandlerRegistry talentRegistry,
    IEffectHandlerRegistry effectRegistry,
    ILogger<ModdingValidator> logger)
{
    public Result<Unit> ValidateOnStartup()
    {
        var errors = new List<string>();

        // Validate all talents have handlers
        foreach (var talent in _talentLoader.LoadAll().Value)
        {
            if (!talentRegistry.HasHandler(talent.Id))
            {
                errors.Add(FormatError(
                    "Talent",
                    talent.Name,
                    talent.Id,
                    "docs/modding-guide.md#talents"
                ));
            }
        }

        // Validate all effect types have handlers
        foreach (var effect in _effectLoader.LoadAll().Value)
        {
            if (!effectRegistry.HasHandler(effect.Type))
            {
                errors.Add(FormatError(
                    "Effect type",
                    effect.Type,
                    null,
                    "docs/developer-guide.md#effect-handlers"
                ));
            }
        }

        if (errors.Any())
        {
            var message = "❌ Game initialization failed due to missing handlers:\n\n" +
                         string.Join("\n", errors) +
                         "\n\nSee docs/modding-guide.md for proper modding workflow.";
            logger.LogError(message);
            return Result<Unit>.Fail(message);
        }

        logger.LogInformation("✓ All registered content has required handlers");
        return Result<Unit>.Ok(Unit.Value);
    }

    private string FormatError(string type, string name, int? id, string docsLink)
    {
        var idPart = id.HasValue ? $" (ID: {id.Value})" : "";
        return $"  • {type} '{name}'{idPart} is missing its C# handler.\n" +
               $"    This {type.ToLower()} will not function in gameplay.\n" +
               $"    See {docsLink} for implementation guide.";
    }
}
```

**Registration**: Add to `src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:
```csharp
services.AddSingleton<ModdingValidator>();
```

**Invocation**: In `src/Era.Core/System/GameInitialization.cs`:
```csharp
public Result<Unit> Initialize()
{
    // ... other initialization ...

    var validationResult = _moddingValidator.ValidateOnStartup();
    if (!validationResult.IsSuccess)
        return validationResult;

    // ... continue initialization ...
}
```

**Verification Method**:
```csharp
[Test]
public void TestPhantomModdabilityPrevention()
{
    // Add phantom talent
    File.WriteAllText("Game/definitions/talents.yaml", "- id: 999\n  name: \"超巨乳\"");

    // Try to initialize
    var result = _gameInitializer.Initialize();

    // Verify failure with clear error
    Assert.False(result.IsSuccess);
    Assert.Contains("Talent '超巨乳' (ID: 999) is missing its C# handler", result.Error);
    Assert.Contains("docs/modding-guide.md", result.Error);
}
```

**Step 4.3: Directory Structure Implementation**

**Action**: Create directory structure with README warnings

**Command**:
```bash
mkdir -p Game/content/kojo
mkdir -p Game/content/characters
mkdir -p Game/content/coms
mkdir -p Game/config
mkdir -p Game/definitions
```

**Create** `Game/definitions/README.md`:
```markdown
# ⚠️  WARNING: Developer-Only Definitions

Files in this directory are **NOT fully moddable**.

## What This Means

Adding new entries to these files requires corresponding C# handler implementation:
- `talents.yaml` - New talents need gameplay logic in C#
- `abilities.yaml` - New abilities need growth calculations in C#
- `effect_types.yaml` - New effects need application logic in C#

## Why YAML Format?

These files use YAML for:
- ✅ Schema validation (catch errors early)
- ✅ Type-safe data loading
- ✅ Easy editing of **existing** entries
- ❌ NOT for adding new game mechanics

## For Modders

See `docs/modding-guide.md` for content you **can** mod without C# knowledge:
- Kojo dialogue (`Game/content/kojo/`)
- Characters (`Game/content/characters/`)
- COM variants (`Game/content/coms/`)

## For Developers

See `docs/developer-guide.md` for implementing new talents/abilities/effects.
```

**Verification Method**: Directory exists with warning README.

**Step 4.4: Documentation Implementation**

**Create** `docs/modding-guide.md`:

*Content: See "Measure 3: Documentation with Clear Moddability Tiers" section above for full content*

**Create** `docs/developer-guide.md` (section):
```markdown
# Developer Guide

## Adding New Talents

**IMPORTANT**: New talents require C# handler implementation. This is NOT a modding task.

### Steps

1. Add to `src/Era.Core/Types/TalentIndex.cs` enum:
   ```csharp
   public enum TalentIndex
   {
       // ... existing ...
       超巨乳 = 999,
   }
   ```

2. Implement handler in `src/Era.Core/Talents/Handlers/`:
   ```csharp
   public class SuperLargeBreastsTalentHandler : ITalentHandler
   {
       public TalentIndex Talent => TalentIndex.超巨乳;

       public void ApplyEffects(CharacterState character)
       {
           // Gameplay logic here
           character.ModifyStat(StatIndex.Bust, 50);
           character.AddDialogueBranch("super_large_breasts");
       }
   }
   ```

3. Register handler in `src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:
   ```csharp
   services.AddSingleton<ITalentHandler, SuperLargeBreastsTalentHandler>();
   ```

4. Add to `Game/definitions/talents.yaml`:
   ```yaml
   - id: 999
     name: "超巨乳"
     description: "Very large breasts trait"
   ```

5. Run tests:
   ```bash
   dotnet test --filter TestAllTalentsHaveHandlers
   ```

6. **ONLY NOW** is the talent moddable (characters can use it).
```

**Verification Method**: Documentation exists and is referenced in error messages.

**Step 4.5: Acceptance Testing**

**Create** `src/Era.Core.Tests/Validation/PhantomModdabilityTests.cs`:

```csharp
public class PhantomModdabilityTests
{
    [Test]
    public void Phantom_Talent_Addition_Fails_With_Clear_Error()
    {
        // Arrange: Add phantom talent
        var yamlContent = "- id: 999\n  name: \"超巨乳\"\n  description: \"Phantom talent\"";
        File.WriteAllText(TestPaths.TalentsYaml, yamlContent);

        // Act: Initialize game
        var result = _gameInitializer.Initialize();

        // Assert: Fails with helpful error
        Assert.False(result.IsSuccess);
        Assert.Contains("Talent '超巨乳' (ID: 999) is missing its C# handler", result.Error);
        Assert.Contains("docs/modding-guide.md", result.Error);
        Assert.DoesNotContain("NullReferenceException", result.Error);  // User-friendly error
    }

    [Test]
    public void Schema_Warning_Appears_For_Tier3_Files()
    {
        // Act: Validate talents.yaml
        var output = RunSchemaValidator("Game/definitions/talents.yaml");

        // Assert: Warning displayed
        Assert.Contains("⚠️  WARNING: talents.yaml has modding level \"not-moddable\"", output);
        Assert.Contains("See docs/modding-guide.md", output);
    }

    [Test]
    public void Valid_Character_With_Existing_Talents_Works()
    {
        // Arrange: Create character with existing TALENTs
        var charYaml = @"
            id: 200
            name: ""Yukari""
            talents: [1, 5]  # 巨乳, 従順
        ";
        File.WriteAllText(TestPaths.CharacterYaml, charYaml);

        // Act: Initialize and load character
        var result = _gameInitializer.Initialize();
        var character = _characterRepository.Load(200);

        // Assert: Success
        Assert.True(result.IsSuccess);
        Assert.True(character.IsSuccess);
        Assert.Equal("Yukari", character.Value.Name);
        Assert.Contains(TalentIndex.巨乳, character.Value.Talents);
    }
}
```

**Verification Method**: All tests PASS.

---

## Conclusion

### Summary of Key Findings

1. **C# Migration Essence**: Better interpreter + structured content, NOT "everything becomes C#"
2. **80% COM YAML-ization**: Majority of COMs can be YAML-only (effect combinations)
3. **Phantom Moddability Problem**: YAML additions often require C# changes - must prevent
4. **True Boundary**: C# Engine (logic/handlers) vs YAML Content (combinations/config)
5. **Community Participation**: Preserved for genuinely moddable content (Tier 1)

### F537 Final Disposition

**Recommendation**: **REVISE**

**New Scope**: Transform from "YAML migration" to "Transform Rules Cleanup"
- Cancel unnecessary CSV→YAML conversion
- Consolidate minimal data into C# constants
- Focus resources on genuinely impactful migrations

### Next Steps

1. **Immediate** (F563): Implement architecture.md changes (Section 1)
2. **Short-term** (F563): Execute Phase 17 feature revisions (Section 2)
3. **Medium-term** (F563+): COM migration infrastructure and execution (Section 3)
4. **Continuous** (F563+): Phantom moddability prevention validation (Section 4)

### Success Metrics

| Metric | Target | Verification |
|--------|--------|--------------|
| **Phase 17 Features Revised** | 11 features | F563 execution log |
| **Phantom Moddability Prevention** | 0 false positives | Runtime validation tests |
| **Community Workflow Preserved** | Tier 1 content works without C# | Integration tests |
| **COM Migration Progress** | 150+ files analyzed | Migration tracker |
| **Documentation Complete** | Modding + Developer guides | File exists + content review |

### Impact Assessment

**Positive Impact**:
- ✅ Clear C#/YAML boundary prevents future confusion
- ✅ Community moddability preserved for appropriate content
- ✅ Phantom moddability prevention improves user experience
- ✅ Reduced wasted effort on unnecessary migrations (F537)
- ✅ Foundation for sustainable long-term architecture

**Risks Mitigated**:
- ❌ Phantom moddability frustrating users → Prevented via validation
- ❌ Unclear boundary causing future feature confusion → Documented clearly
- ❌ Community participation barriers → Preserved ERA-style workflow
- ❌ Wasted migration effort → Canceled F537, revised others

---

**Document Status**: Complete
**Total Pages**: 49 sections
**Word Count**: ~15,000 words
**Analysis Depth**: Category-level COM audit (individual COM disposition delegated to F563)
