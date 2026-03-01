# COM YAML Guide

**Target Audience**: Content creators and modders who want to create or modify COM (Command) definitions for the ERA game.

**Last Updated**: 2026-01-20

---

## Table of Contents

1. [Introduction](#introduction)
2. [YAML Structure Overview](#yaml-structure-overview)
3. [Effect Types Reference](#effect-types-reference)
4. [Parameter Schema](#parameter-schema)
5. [Examples by Category](#examples-by-category)
6. [Advanced Topics](#advanced-topics)
7. [Validation and Testing](#validation-and-testing)

---

## Introduction

COM (Command) definitions in the ERA game are now defined in YAML format, allowing content creators to add and modify game commands without C# compilation. This guide covers the effect system that determines what happens when a COM is executed.

### Moddability Tiers

- **Tier 1** (Fully Moddable): Adding new COMs with existing effect types, modifying values - **No C# compilation required**
- **Tier 2** (Parameter Adjustment): Tweaking existing parameters - **No C# compilation required**
- **Tier 3** (Engine Extension): Adding new effect types or condition types - **Requires C# compilation**

This guide focuses on Tier 1 and 2 moddability.

---

## YAML Structure Overview

A COM definition consists of the following main sections:

```yaml
id: 0                        # Unique COM identifier
name: "愛撫"                  # Display name (Japanese)
category: "Training/Touch"   # Category for UI grouping
description: "..."           # Optional description
cost:                        # Resource costs
  stamina: 5
  energy: 50
conditions: []               # Prerequisites for execution
effects: []                  # Effects applied when executed
kojo_file: "com000.yaml"    # Optional: dialogue file reference
metadata:                    # Optional: tracking information
  migrated_from_cs: "..."
  migration_date: "..."
```

The **effects** array is the core of COM behavior, defining what changes occur in the game state when the COM executes.

---

## Effect Types Reference

The runtime supports four main effect types, each handled by a specialized effect handler:

### Effect Type Summary Table

| Effect Type | Handler | Purpose | Common Use Cases |
|------------|---------|---------|------------------|
| `source` | SourceEffectHandler | Direct character state modification | Add/subtract pleasure, love, fear, submission, etc. |
| `source_scale` | SourceScaleEffectHandler | Formula-based scaled state modification | Experience-based scaling, level-dependent effects |
| `downbase` | DownbaseEffectHandler | Base stat reduction (costs) | Stamina cost, energy cost, resource consumption |
| `exp` | ExpEffectHandler | Experience/skill increases | Skill training, proficiency improvement |

### 1. source - Direct State Modification

**Purpose**: Directly modifies character SOURCE parameters (emotional/physical states).

**Available Parameters**:

| Parameter | Index | Japanese | Description |
|-----------|-------|----------|-------------|
| `pain` | 0 | 苦痛 | Physical pain level |
| `pleasure` | 1 | 快楽 | Pleasure/arousal level |
| `fear` | 2 | 恐怖 | Fear/terror level |
| `submission` | 3 | 従属 | Submission/obedience level |
| `lust` | 4 | 欲情 | Lust/desire level |
| `love` | 5 | 恋慕 | Love/affection level |
| `shame` | 6 | 羞恥 | Shame/embarrassment level |
| `dependence` | 7 | 依存 | Dependence/addiction level |

**Effect Structure**:

```yaml
effects:
  - type: source
    parameters:
      pleasure: 80      # Increases pleasure by 80
      love: 50          # Increases love by 50
      fear: -20         # Decreases fear by 20 (negative values subtract)
```

**Notes**:
- Values are added directly to current state
- Positive values increase, negative values decrease
- Multiple parameters can be specified in one effect entry

### 2. source_scale - Formula-Based Scaling

**Purpose**: Calculates effect values using formulas that can reference game state, enabling sophisticated scaling behavior.

**Required Fields**:

| Field | Type | Description |
|-------|------|-------------|
| `target` | string | Source parameter name (pain, pleasure, etc.) |
| `formula` | string | Mathematical expression to evaluate |
| `value` (optional) | integer | Base value (default: 100) referenced as 'value' in formula |

**Formula Syntax**:

| Feature | Syntax | Example |
|---------|--------|---------|
| Arithmetic | `+`, `-`, `*`, `/` | `value * 2 + 10` |
| Parentheses | `(`, `)` | `(value + 50) * 2` |
| Base value placeholder | `value` | `value / 2` |
| Palam level function | `getPalamLv(name, threshold)` | `getPalamLv(pleasure, 1000)` |
| Max function | `max(a, b)` | `max(value, 50)` |
| Min function | `min(a, b)` | `min(value * 2, 200)` |

**Effect Structure**:

```yaml
effects:
  - type: source_scale
    parameters:
      target: pleasure
      value: 100              # Base value
    formula: "value * getPalamLv(pleasure, 1000) + 50"
    # Result scales with character's current pleasure level
```

**Formula Evaluation Process**:

1. Replace `value` with the specified base value
2. Evaluate `getPalamLv()` calls using current character state
3. Process `max()` and `min()` functions
4. Evaluate arithmetic expression with proper operator precedence

**getPalamLv Function**:

```
getPalamLv(palamName, threshold)
```

- **palamName**: Parameter name (pain, pleasure, fear, etc.) or Japanese equivalent
- **threshold**: Integer value for level calculation
- **Returns**: `current_palam_value / threshold` (integer division)
- **Example**: If pleasure = 2500 and threshold = 1000, returns 2

### 3. downbase - Base Stat Reduction

**Purpose**: Reduces base stats (typically used for costs or penalties).

**Common Parameters**:

| Parameter | Description | Typical Use |
|-----------|-------------|-------------|
| `stamina` | Stamina cost | Physical action costs |
| `energy` | Energy cost | Mental/general action costs |
| `resistance` | Resistance reduction | Psychological conditioning |

**Effect Structure**:

```yaml
effects:
  - type: downbase
    parameters:
      stamina: 5        # Reduces stamina by 5
      energy: 50        # Reduces energy by 50
```

**Notes**:
- Typically used for resource consumption
- Values are subtracted from current base stats
- Can be combined with cost field (cost is checked before execution, downbase is applied during execution)

### 4. exp - Experience/Skill Increase

**Purpose**: Increases character experience or skill proficiency.

**Effect Structure**:

```yaml
effects:
  - type: exp
    parameters:
      oral_skill: 10        # Increases oral skill experience by 10
      training_level: 5     # Increases training level by 5
```

**Notes**:
- Parameter names depend on your game's EXP index definitions
- Values accumulate over time
- Experience thresholds for level-ups are defined elsewhere in the game engine

---

## Parameter Schema

### Full Effect Definition Schema

Each effect in the `effects` array follows this structure:

```yaml
effects:
  - type: "source" | "source_scale" | "downbase" | "exp" | "flag" | "cflag" | "equipment" | "custom"
    target: string          # For source_scale: target parameter name
    parameters:             # Key-value pairs, content depends on effect type
      [key]: [value]
    value: integer          # For source_scale: base value (optional, default 100)
    formula: string         # For source_scale: calculation expression
    pain: number            # Shorthand for source effects
    pleasure: number        # Shorthand for source effects
    fear: number            # Shorthand for source effects
    submission: number      # Shorthand for source effects
    lust: number            # Shorthand for source effects
    love: number            # Shorthand for source effects
    shame: number           # Shorthand for source effects
    dependence: number      # Shorthand for source effects
```

### Parameter Specification Methods

**Method 1: parameters object** (recommended for clarity):

```yaml
effects:
  - type: source
    parameters:
      pleasure: 100
      love: 50
```

**Method 2: Direct properties** (shorthand for source effects):

```yaml
effects:
  - type: source
    pleasure: 100
    love: 50
```

Both methods are equivalent for `source` type effects.

---

## Examples by Category

### Training/Touch - Basic Physical Intimacy

**Category Characteristics**:
- Primary effects: pleasure, love, fear
- Focus: Physical touch and affection progression
- Typical intensity: Low to medium

**Example: Caress (愛撫)**

```yaml
id: 0
name: "愛撫"
category: "Training/Touch"
description: "Kissing and caressing command, Level 2."
cost:
  stamina: 5
  energy: 50
effects:
  - type: source
    parameters:
      pleasure: 80      # Moderate pleasure increase
      love: 50          # Love increase from intimate touch
  - type: downbase
    parameters:
      stamina: 5        # Physical exertion cost
      energy: 50        # Mental focus cost
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/Training/Touch/Caress.cs"
  migration_date: "2026-01-19"
```

**Example: Breast Caress (Ｂ愛撫)**

```yaml
id: 123
name: "Ｂ愛撫"
category: "Training/Touch"
description: "Breast Caressing - Caressing command, Level 2."
cost:
  stamina: 7
  energy: 70
effects:
  - type: source
    parameters:
      pleasure: 200     # Higher pleasure than general caressing
  - type: downbase
    parameters:
      stamina: 7        # Slightly more effort required
      energy: 70
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/Training/Touch/BreastCaress.cs"
  migration_date: "2026-01-19"
```

### Training/Oral - Advanced Intimacy Skills

**Category Characteristics**:
- Primary effects: pleasure, lust, skill experience
- Focus: Oral techniques and skill development
- Typical intensity: Medium to high

**Example: Cunnilingus (クンニ)**

```yaml
id: 1
name: "クンニ"
category: "Training/Oral"
description: "Oral caressing command, Level 1"
cost:
  stamina: 0
  energy: 0
effects:
  - type: source
    parameters:
      pleasure: 100     # High pleasure from oral stimulation
      lust: 60          # Arousal increase
      love: 40          # Intimacy bonding
  - type: downbase
    parameters:
      stamina: 8        # Physical effort
      energy: 60        # Concentration requirement
  - type: exp
    parameters:
      oral_skill: 5     # Skill proficiency gain
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/Training/Oral/Cunnilingus.cs"
  migration_date: "2026-01-19"
```

### Training/Penetration - Intense Training

**Category Characteristics**:
- Primary effects: pleasure, pain, fear modification
- Focus: Penetrative acts with state changes
- Typical intensity: High

**Example: Vaginal Penetration (膣挿入)**

```yaml
id: 200
name: "膣挿入"
category: "Training/Penetration"
description: "Vaginal penetration with variable pain/pleasure based on experience"
cost:
  stamina: 15
  energy: 100
effects:
  - type: source_scale
    parameters:
      target: pleasure
      value: 150
    formula: "value + getPalamLv(pleasure, 1000) * 20"
    # Pleasure scales with character's training level
  - type: source
    parameters:
      pain: 50          # Initial pain component
      fear: -10         # Fear reduction through experience
  - type: downbase
    parameters:
      stamina: 15
      energy: 100
  - type: exp
    parameters:
      penetration_exp: 10
metadata:
  notes: "Pain decreases and pleasure increases with training"
```

### Training/Equipment - Tool-Based Training

**Category Characteristics**:
- Primary effects: source_scale with experience scaling
- Focus: Equipment usage and progressive intensity
- Typical intensity: Variable (scales with use)

**Example: Anal Beads (アナルビーズ)**

```yaml
id: 46
name: "アナルビーズ"
category: "Training/Equipment"
description: "Anal beads with variable pleasure on insertion/removal, Level 3"
cost:
  stamina: 0
  energy: 0
effects:
  - type: source_scale
    parameters:
      target: pleasure
      value: 80
    formula: "max(value, getPalamLv(pleasure, 800) * 30)"
    # Pleasure scales with experience level
  - type: source
    parameters:
      shame: 60         # Embarrassment from equipment use
  - type: exp
    parameters:
      anal_training: 8  # Specialized training experience
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/Training/Equipment/AnalBeads.cs"
  migration_date: "2026-01-19"
```

### Training/Bondage - Psychological Conditioning

**Category Characteristics**:
- Primary effects: submission, fear, shame
- Focus: Restraint and psychological control
- Typical intensity: Medium (psychological focus)

**Example: Ball Gag (ボールギャグ)**

```yaml
id: 106
name: "ボールギャグ"
category: "Training/Bondage"
description: "Ball Gag equipment with continuous effect, Level 1"
cost:
  stamina: 0
  energy: 0
effects:
  - type: source
    parameters:
      submission: 80    # Helplessness increases submission
      shame: 100        # Embarrassment from inability to speak
      fear: 40          # Slight fear component
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/Training/Bondage/BallGag.cs"
  migration_date: "2026-01-19"
  notes: "Gag equipment increases submission through helplessness and shame"
```

### Masturbation - Solo Activities

**Category Characteristics**:
- Primary effects: pleasure, solo skill experience
- Focus: Self-directed activities
- Typical intensity: Medium

**Example: Self Anal Beads (自慰アナルビーズ)**

```yaml
id: 646
name: "アナルビーズ"
category: "Masturbation"
description: "Self anal beads equipment command for masturbation"
cost:
  stamina: 0
  energy: 0
effects:
  - type: source
    parameters:
      pleasure: 100     # Self-directed pleasure
      shame: 80         # High shame from solo act
  - type: downbase
    parameters:
      stamina: 5
      energy: 40
  - type: exp
    parameters:
      masturbation_skill: 5
      anal_training: 3
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/Masturbation/SelfAnalBeads.cs"
  migration_date: "2026-01-19"
```

### Daily - Routine Activities

**Category Characteristics**:
- Primary effects: mood states, daily skills
- Focus: Non-training routine activities
- Typical intensity: Low

**Example: Anal Caress Daily (日常アナル愛撫)**

```yaml
id: 314
name: "アナル愛撫"
category: "Daily"
description: "Daily anal caressing (lighter than training version)"
cost:
  stamina: 0
  energy: 0
effects:
  - type: source
    parameters:
      pleasure: 60      # Lower intensity than training
      love: 30
  - type: downbase
    parameters:
      stamina: 3
      energy: 30
  - type: exp
    parameters:
      intimacy_level: 2
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/Daily/AnalCaressDaily.cs"
  migration_date: "2026-01-19"
  notes: "Lighter version for relationship maintenance"
```

### Utility - Support and State Management

**Category Characteristics**:
- Primary effects: Various utility functions
- Focus: State changes, location, equipment, etc.
- Typical intensity: Variable

**Example: Clean (掃除)**

```yaml
id: 410
name: "掃除"
category: "Utility"
description: "Cleaning action with skill checks and rewards"
cost:
  stamina: 50
  energy: 100
effects:
  - type: source_scale
    parameters:
      target: pleasure
      value: 20
    formula: "value + getPalamLv(submission, 500) * 10"
    # Pleasure from service scales with submission
  - type: downbase
    parameters:
      stamina: 50
      energy: 100
  - type: exp
    parameters:
      cleaning_exp: 5
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/Utility/Clean.cs"
  migration_date: "2026-01-19"
```

### Visitor - NPC Interaction

**Category Characteristics**:
- Primary effects: Relationship states, location changes
- Focus: External character interactions
- Typical intensity: Varies by context

**Example: Go Out (外出する)**

```yaml
id: 490
name: "外出する"
category: "Visitor"
description: "Travel to external locations outside mansion"
cost:
  stamina: 0
  energy: 0
effects: []
  # Effects depend on destination and events
  # Typically handled by game logic rather than static effects
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/Visitor/GoOut.cs"
  migration_date: "2026-01-19"
  notes: "Location-specific effects applied dynamically"
```

### System - Meta-Game Commands

**Category Characteristics**:
- Primary effects: Game state management
- Focus: System-level operations
- Typical intensity: N/A (meta-game)

**Example: Day End (一日の終了)**

```yaml
id: 888
name: "一日の終了"
category: "System"
description: "System command for ending the day and transitioning to sleep"
cost:
  stamina: 0
  energy: 0
effects: []
  # System commands typically don't use effect handlers
  # Logic is implemented in C# game engine
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/System/DayEnd.cs"
  migration_date: "2026-01-19"
  notes: "Time progression handled by game engine"
```

### Training/Undressing - Clothing Removal

**Category Characteristics**:
- Primary effects: shame, exhibitionism
- Focus: Progressive undressing training
- Typical intensity: Medium (psychological)

**Example: Remove Top (上着を脱ぐ)**

```yaml
id: 520
name: "上着を脱ぐ"
category: "Training/Undressing"
description: "Command to remove upper clothing"
cost:
  stamina: 0
  energy: 20
effects:
  - type: source
    parameters:
      shame: 100        # High shame from exposure
      submission: 40    # Obedience from complying
  - type: exp
    parameters:
      exhibitionism: 5  # Exhibitionism training
metadata:
  notes: "First stage of undressing progression"
```

---

## Advanced Topics

### Combining Multiple Effects

Most COMs combine multiple effect types to create rich, realistic behavior:

```yaml
effects:
  # Primary effect: pleasure with scaling
  - type: source_scale
    parameters:
      target: pleasure
      value: 100
    formula: "value + getPalamLv(pleasure, 1000) * 25"

  # Secondary effects: emotional responses
  - type: source
    parameters:
      shame: 80
      love: 30

  # Resource costs
  - type: downbase
    parameters:
      stamina: 10
      energy: 60

  # Skill progression
  - type: exp
    parameters:
      relevant_skill: 8
```

### Formula Complexity Examples

**Simple arithmetic**:
```yaml
formula: "value * 2"                    # Double the base value
formula: "value + 50"                   # Add fixed amount
formula: "(value + 100) / 2"            # Average with fixed value
```

**Experience-based scaling**:
```yaml
# Linear scaling: +20 per level
formula: "value + getPalamLv(pleasure, 1000) * 20"

# Multiplicative scaling: double at level 5
formula: "value * (1 + getPalamLv(pleasure, 1000) / 5)"

# Capped scaling: max 200
formula: "min(value + getPalamLv(pleasure, 800) * 15, 200)"
```

**Complex formulas**:
```yaml
# Scaling with diminishing returns
formula: "value + min(getPalamLv(pleasure, 500) * 30, 100)"

# Multi-parameter consideration (requires multiple effects)
formula: "max(value * getPalamLv(pleasure, 1000), 50)"
```

### Effect Execution Order

Effects are processed in the order they appear in the array:

1. Effect 1 is applied
2. Effect 2 is applied (can reference state modified by Effect 1)
3. Effect 3 is applied
4. And so on...

This allows for complex interactions where later effects can scale based on earlier modifications.

### Negative Values and State Reduction

All effect types support negative values to reduce states:

```yaml
effects:
  - type: source
    parameters:
      fear: -50         # Reduce fear (comfort)
      pain: -30         # Reduce pain (healing)

  - type: source_scale
    parameters:
      target: resistance
      value: -20        # Reduce resistance progressively
    formula: "value * getPalamLv(submission, 800)"
```

### Conditional Effects (Future)

Currently, effects are applied unconditionally once the COM can execute. Future versions may support conditional effect application:

```yaml
# FUTURE FEATURE (not yet implemented)
effects:
  - type: source
    parameters:
      pleasure: 100
    condition:
      type: talent
      target: "sensitive"
      operator: "eq"
      value: true
```

For now, use COM-level `conditions` to gate entire COM execution.

---

## Validation and Testing

### Schema Validation

All YAML COM files are validated against the JSON schema at:
```
src/tools/schemas/com.schema.json
```

Validation checks:
- Required fields are present
- Effect types are valid (source, source_scale, downbase, exp)
- Parameter structure is correct
- Values are appropriate types (integers, strings)

### Runtime Validation

The game engine performs additional validation at runtime:
- Effect handlers are registered for specified types
- Source/downbase/exp indices are valid
- Formula syntax is parseable
- getPalamLv references valid palam names

### Testing Your COM

1. **Create the YAML file** in the appropriate category directory:
   ```
   Game/data/coms/[Category]/[filename].yaml
   ```

2. **Validate the schema** (if schema validator is available):
   ```bash
   dotnet run --project tools/YamlValidator -- Game/data/coms/[Category]/[filename].yaml
   ```

3. **Test in-game**:
   - Load the game
   - Navigate to the COM menu
   - Execute your COM
   - Verify effects are applied correctly
   - Check game logs for errors

4. **Unit testing** (for developers):
   ```bash
   dotnet test --filter "YamlComExecutor"
   ```

### Common Validation Errors

**Missing required field**:
```
Error: COM definition missing required field 'id'
```
Fix: Add all required fields (id, name, category)

**Invalid effect type**:
```
Error: Unknown effect type 'custom_type'
```
Fix: Use only: source, source_scale, downbase, exp

**Formula syntax error**:
```
Error: Failed to evaluate formula 'value ++ 10'
```
Fix: Check formula syntax (++ is invalid, use + only)

**Invalid parameter name**:
```
Error: Unknown source target: 'invalid_name'
```
Fix: Use valid parameter names (pleasure, pain, fear, etc.)

---

## Appendix: Complete Effect Type Reference

### source Effect Parameters

| Parameter | Type | Range | Description |
|-----------|------|-------|-------------|
| pain | int | -∞ to +∞ | Physical pain level modification |
| pleasure | int | -∞ to +∞ | Pleasure/arousal level modification |
| fear | int | -∞ to +∞ | Fear/terror level modification |
| submission | int | -∞ to +∞ | Submission/obedience level modification |
| lust | int | -∞ to +∞ | Lust/desire level modification |
| love | int | -∞ to +∞ | Love/affection level modification |
| shame | int | -∞ to +∞ | Shame/embarrassment level modification |
| dependence | int | -∞ to +∞ | Dependence/addiction level modification |

### source_scale Required Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| target | string | Yes | Target parameter name (from source list) |
| formula | string | Yes | Mathematical expression to evaluate |
| value | int | No | Base value (default: 100) |

### downbase Effect Parameters

| Parameter | Type | Range | Description |
|-----------|------|-------|-------------|
| stamina | int | 0 to +∞ | Stamina reduction amount |
| energy | int | 0 to +∞ | Energy reduction amount |
| resistance | int | -∞ to +∞ | Resistance modification |

### exp Effect Parameters

Parameters depend on your game's EXP index definitions. Common examples:

| Parameter | Description |
|-----------|-------------|
| oral_skill | Oral technique proficiency |
| penetration_exp | Penetration experience |
| anal_training | Anal training level |
| masturbation_skill | Self-pleasure proficiency |
| cleaning_exp | Cleaning skill experience |
| intimacy_level | Overall intimacy progression |
| exhibitionism | Exhibitionism training level |

---

## Frequently Asked Questions

**Q: Can I add new effect types without C# compilation?**

A: No. Adding new effect types requires implementing IEffectHandler in C# and registering it with the effect handler registry (Tier 3 moddability).

**Q: How do I know if my formula is correct?**

A: Test in-game and check the combat/debug log. Formula evaluation errors are logged. You can also write unit tests using the SourceScaleEffectHandler test suite.

**Q: Can effects reference the results of previous effects?**

A: Not directly in the same COM execution. Effects are applied sequentially, but each effect sees the character state before any effects in the current COM were applied. For progressive effects, use multiple COMs in sequence.

**Q: What happens if I specify an invalid parameter name?**

A: The effect will fail at runtime and an error will be logged. The COM will not execute fully.

**Q: Can I use Japanese characters in parameter names?**

A: getPalamLv accepts Japanese names (苦痛, 快楽, etc.), but parameter keys in the YAML should use English names (pain, pleasure, etc.) for consistency.

**Q: How do I implement random/variable effects?**

A: Currently, use formulas with getPalamLv to create state-dependent variability. True random effects require C# implementation.

---

## Additional Resources

- **JSON Schema**: `src/tools/schemas/com.schema.json` - Full schema definition
- **Example COMs**: `Game/data/coms/` - Browse existing COMs for patterns
- **Effect Handlers Source**: `src/Era.Core/Effects/` - C# implementation details
- **Runtime Executor**: `src/Era.Core/Commands/Com/YamlComExecutor.cs` - Execution logic

---

**Document Version**: 1.0
**Feature**: F565 - COM YAML Runtime Integration
**Created**: 2026-01-20
