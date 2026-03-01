# COM YAML Modding Guide

**Target Audience**: Content creators and modders who want to create or modify COM (Command) definitions for the ERA game.

**Last Updated**: 2026-01-25

---

## Table of Contents

1. [Introduction](#introduction)
2. [Moddability Tiers](#moddability-tiers)
3. [Tier 1: Parameter-Level Moddability](#tier-1-parameter-level-moddability)
4. [Tier 2: Structure-Level Moddability](#tier-2-structure-level-moddability)
5. [Tier 3: ERB Domain (Out of Scope)](#tier-3-erb-domain-out-of-scope)
6. [YAML Structure Overview](#yaml-structure-overview)
7. [Effect Types Reference](#effect-types-reference)
8. [Parameter Schema](#parameter-schema)
9. [Examples by Category](#examples-by-category)
10. [Advanced Topics](#advanced-topics)
11. [Validation and Testing](#validation-and-testing)

---

## Introduction

COM (Command) definitions in the ERA game are now defined in YAML format, allowing content creators to add and modify game commands without C# compilation. This guide covers the effect system that determines what happens when a COM is executed.

The YAML-based COM system provides three tiers of moddability, ranging from simple parameter tweaks (Tier 1) to structural modifications (Tier 2) and engine-level extensions (Tier 3). This guide focuses on Tier 1 and 2 moddability, which require no C# compilation.

---

## Moddability Tiers

The COM YAML system is designed with a tiered moddability approach, allowing modders to customize the game at different levels of complexity:

| Tier | Name | Description | C# Compilation Required | Typical Use Cases |
|:----:|------|-------------|:-----------------------:|-------------------|
| **Tier 1** | Parameter-Level | Modifying numeric/string values in existing COMs | ❌ No | Balance tweaks, intensity adjustments, cost changes |
| **Tier 2** | Structure-Level | Adding new COMs, combining effects, modifying effect flow | ❌ No | New training commands, custom content, effect combinations |
| **Tier 3** | ERB Domain | Adding new effect types, condition types, or custom game logic | ✅ Yes | New game mechanics, custom effect handlers, engine extensions |

**This guide covers Tier 1 and Tier 2 only.** For Tier 3 modifications, refer to the engine development documentation.

---

## Tier 1: Parameter-Level Moddability

**Tier 1 moddability** allows you to modify existing COM definitions by changing numeric values, thresholds, and timing parameters. This is the simplest form of modding and requires no programming knowledge—just edit YAML files and reload the game.

### What You Can Modify

1. **Effect intensity** - Change how much pleasure, pain, love, etc. a COM produces
2. **Resource costs** - Adjust stamina and energy consumption
3. **Experience gains** - Modify skill progression rates
4. **Formula parameters** - Tweak scaling coefficients and base values

### Example 1: Adjusting Pleasure Intensity

**Original File**: `Game/data/coms/training/touch/caress.yaml`

```yaml
# Original: Basic caress with moderate pleasure
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
      pleasure: 80      # Original pleasure value
      love: 50
  - type: downbase
    parameters:
      stamina: 5
      energy: 50
```

**Modified: Increased pleasure for faster progression**

```yaml
# Game/data/coms/training/touch/caress.yaml
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
      pleasure: 120     # Increased from 80 to 120 (50% boost)
      love: 50
  - type: downbase
    parameters:
      stamina: 5
      energy: 50
```

**Impact**: The caress command now provides 50% more pleasure, making training progression faster.

### Example 2: Reducing Resource Costs

**Original File**: `Game/data/coms/training/equipment/anal-beads.yaml`

```yaml
# Original: Anal beads with moderate intensity
id: 46
name: "アナルビーズ"
category: "Training/Equipment"
description: "Anal beads with variable pleasure on insertion/removal, Level 3."
cost:
  stamina: 0
  energy: 0
effects:
  - type: source
    parameters:
      pleasure: 380
      pain: 60          # Original pain value
      submission: 280
  - type: downbase
    parameters:
      stamina: 0
      energy: 0
```

**Modified: Reduced pain for gentler training**

```yaml
# Game/data/coms/training/equipment/anal-beads.yaml
id: 46
name: "アナルビーズ"
category: "Training/Equipment"
description: "Anal beads with variable pleasure on insertion/removal, Level 3."
cost:
  stamina: 0
  energy: 0
effects:
  - type: source
    parameters:
      pleasure: 380
      pain: 30          # Reduced from 60 to 30 (50% reduction)
      submission: 280
  - type: downbase
    parameters:
      stamina: 0
      energy: 0
```

**Impact**: Equipment use is now less painful, making it more suitable for characters with lower pain tolerance.

### Example 3: Tweaking Formula Coefficients

**Original File**: `Game/data/coms/training/equipment/aphrodisiac.yaml`

```yaml
# Original: Aphrodisiac with experience-based scaling
id: 187
name: "媚薬"
category: "Training/Equipment"
description: "Aphrodisiac - Equipment command with continuous effect."
cost:
  stamina: 0
  energy: 0
effects:
  - type: source
    parameters:
      lust: 600
      pleasure: 250
  - type: source_scale
    parameters:
      pleasure: 400
    formula: "baseValue * (1 + getPalamLv(lust) * 0.15)"  # Original: 15% per level
  - type: downbase
    parameters:
      stamina: 0
      energy: 0
```

**Modified: Stronger scaling for experienced characters**

```yaml
# Game/data/coms/training/equipment/aphrodisiac.yaml
id: 187
name: "媚薬"
category: "Training/Equipment"
description: "Aphrodisiac - Equipment command with continuous effect."
cost:
  stamina: 0
  energy: 0
effects:
  - type: source
    parameters:
      lust: 600
      pleasure: 250
  - type: source_scale
    parameters:
      pleasure: 400
    formula: "baseValue * (1 + getPalamLv(lust) * 0.25)"  # Increased from 0.15 to 0.25
  - type: downbase
    parameters:
      stamina: 0
      energy: 0
```

**Impact**: The aphrodisiac now scales more aggressively with character lust level (25% per level instead of 15%), rewarding progression.

### Tier 1 Best Practices

1. **Start small** - Make one change at a time and test thoroughly
2. **Document changes** - Use YAML comments to note your modifications
3. **Keep backups** - Save original files before modifying
4. **Balance carefully** - Extreme values can break game balance or progression
5. **Test in-game** - Always verify your changes work as expected

### Common Tier 1 Modifications

| Modification Type | Files to Edit | Parameters to Change |
|-------------------|---------------|---------------------|
| Training intensity | `training/**/*.yaml` | `pleasure`, `pain`, `love`, `fear` |
| Resource economy | All COM files | `stamina`, `energy` in `cost` and `downbase` |
| Skill progression | Files with `exp` effects | Experience parameter values |
| Scaling behavior | Files with `source_scale` | Formula coefficients (e.g., `0.15` → `0.25`) |

---

## Tier 2: Structure-Level Moddability

**Tier 2 moddability** allows you to create entirely new COM definitions, combine multiple effects in novel ways, and modify the structure of effect flow. This requires understanding YAML syntax and the effect system, but still no C# compilation.

### What You Can Do

1. **Create new COMs** - Define entirely new training commands with custom effect combinations
2. **Combine effects** - Mix multiple effect types (source, source_scale, downbase, exp) for complex behavior
3. **Add conditional scaling** - Use formulas to create dynamic, state-dependent effects
4. **Modify effect flow** - Change the order and combination of effects

### Example 1: Creating a New Custom COM

**New File**: `Game/data/coms/training/touch/gentle-embrace.yaml`

```yaml
# Game/data/coms/training/touch/gentle-embrace.yaml
# Custom COM: Gentle Embrace - Low intensity, high love gain
id: 9001  # Use high ID to avoid conflicts
name: "優しい抱擁"
category: "Training/Touch"
description: "A gentle, comforting embrace that builds trust and affection."
cost:
  stamina: 2
  energy: 20
effects:
  # Primary effect: Build love without overwhelming pleasure
  - type: source
    parameters:
      love: 120         # High love gain
      pleasure: 30      # Low pleasure (comfort, not arousal)
      fear: -40         # Reduces fear (reassuring)

  # Resource costs: Very light
  - type: downbase
    parameters:
      stamina: 2
      energy: 20

  # Experience: Build intimacy skill
  - type: exp
    parameters:
      intimacy_level: 8
metadata:
  custom_content: true
  author: "Your Name"
  creation_date: "2026-01-25"
  notes: "Designed for building trust with fearful characters"
```

**Use Case**: This custom COM is ideal for characters with high fear or low trust. It provides a gentler alternative to standard caressing commands.

### Example 2: Advanced Multi-Stage Effect Combination

**New File**: `Game/data/coms/training/penetration/progressive-training.yaml`

```yaml
# Game/data/coms/training/penetration/progressive-training.yaml
# Custom COM: Progressive intensity based on character experience
id: 9002
name: "段階的調教"
category: "Training/Penetration"
description: "Progressive training that adapts intensity to character experience."
cost:
  stamina: 20
  energy: 120
effects:
  # Stage 1: Base pleasure with experience scaling
  - type: source_scale
    parameters:
      pleasure: 200
    formula: "baseValue * (1 + getPalamLv(pleasure) * 0.2)"

  # Stage 2: Pain that decreases with experience
  - type: source_scale
    parameters:
      pain: 100
    formula: "max(baseValue - getPalamLv(pleasure) * 15, 10)"  # Min 10 pain

  # Stage 3: Submission gain that increases with pleasure level
  - type: source_scale
    parameters:
      submission: 80
    formula: "baseValue + getPalamLv(pleasure) * 10"

  # Stage 4: Fear reduction (comfort through familiarity)
  - type: source
    parameters:
      fear: -20

  # Stage 5: Resource costs
  - type: downbase
    parameters:
      stamina: 20
      energy: 120

  # Stage 6: Experience gain
  - type: exp
    parameters:
      penetration_exp: 15
      training_level: 10
metadata:
  custom_content: true
  author: "Your Name"
  creation_date: "2026-01-25"
  notes: "Multi-stage progressive training with dynamic intensity"
```

**Advanced Technique**: This COM uses multiple `source_scale` effects to create a progressive training experience that adapts to character state. Pain decreases as pleasure experience increases, creating realistic progression.

### Example 3: Conditional Effect Scaling with Multiple Variables

**New File**: `Game/data/coms/training/equipment/advanced-sensitivity-device.yaml`

```yaml
# Game/data/coms/training/equipment/advanced-sensitivity-device.yaml
# Custom COM: Advanced device with complex multi-variable scaling
id: 9003
name: "高度感度増幅装置"
category: "Training/Equipment"
description: "Advanced equipment that scales with both pleasure and lust."
cost:
  stamina: 0
  energy: 80
effects:
  # Complex scaling: Considers both pleasure and lust levels
  - type: source_scale
    parameters:
      pleasure: 600
      lust: 400
    formula: "baseValue * (1 + (getPalamLv(lust) + getPalamLv(pleasure)) * 0.12)"

  # Secondary effect: Shame with diminishing returns
  - type: source_scale
    parameters:
      shame: 200
    formula: "min(baseValue + getPalamLv(submission) * 20, 400)"  # Capped at 400

  # Submission boost for advanced users
  - type: source
    parameters:
      submission: 150

  # Resource cost
  - type: downbase
    parameters:
      energy: 80

  # Experience: Multiple skill gains
  - type: exp
    parameters:
      equipment_mastery: 12
      sensitivity_training: 8
metadata:
  custom_content: true
  author: "Your Name"
  creation_date: "2026-01-25"
  notes: "Multi-variable scaling with capped shame for advanced training"
```

**Advanced Technique**: This COM demonstrates:
- Multi-variable formulas (`getPalamLv(lust) + getPalamLv(pleasure)`)
- Capped scaling with `min()` function
- Multiple experience gains for different skills

### Example 4: Adding Effects to Existing COMs

You can extend existing COMs by adding new effects to their effect arrays:

**Original File**: `Game/data/coms/training/touch/caress.yaml`

```yaml
# Original: Simple caress
id: 0
name: "愛撫"
category: "Training/Touch"
effects:
  - type: source
    parameters:
      pleasure: 80
      love: 50
  - type: downbase
    parameters:
      stamina: 5
      energy: 50
```

**Modified: Added submission gain and experience tracking**

```yaml
# Game/data/coms/training/touch/caress.yaml
id: 0
name: "愛撫"
category: "Training/Touch"
effects:
  - type: source
    parameters:
      pleasure: 80
      love: 50
      submission: 20    # NEW: Added submission gain

  - type: downbase
    parameters:
      stamina: 5
      energy: 50

  # NEW: Added experience tracking
  - type: exp
    parameters:
      caress_skill: 5
      intimacy_level: 3
```

**Impact**: The basic caress now also builds submission and tracks skill progression.

### Tier 2 Best Practices

1. **Use high COM IDs** - Start custom COMs at ID 9000+ to avoid conflicts
2. **Test formulas carefully** - Complex formulas can have unexpected edge cases
3. **Document thoroughly** - Use metadata fields to explain custom content
4. **Validate with tools** - Use YamlValidator to catch structural errors (see [Validation and Testing](#validation-and-testing))
5. **Consider balance** - Ensure custom COMs fit the game's progression curve

### Formula Design Patterns

#### Pattern 1: Linear Scaling
```yaml
formula: "baseValue + getPalamLv(pleasure) * 20"
# Effect increases by 20 per pleasure level
```

#### Pattern 2: Multiplicative Scaling
```yaml
formula: "baseValue * (1 + getPalamLv(pleasure) * 0.15)"
# Effect increases by 15% per pleasure level
```

#### Pattern 3: Diminishing Returns (Capped)
```yaml
formula: "min(baseValue + getPalamLv(pleasure) * 30, 500)"
# Scales up to maximum of 500
```

#### Pattern 4: Inverse Scaling (Decreasing)
```yaml
formula: "max(baseValue - getPalamLv(pleasure) * 10, 20)"
# Decreases by 10 per level, minimum 20
```

#### Pattern 5: Multi-Variable Combination
```yaml
formula: "baseValue * (1 + (getPalamLv(lust) + getPalamLv(pleasure)) * 0.08)"
# Scales based on sum of two variables
```

### Validation

**IMPORTANT**: Before using custom COMs, validate them with the YAML validator:

```bash
dotnet run --project src/tools/dotnet/YamlValidator -- \
  --schema src/tools/schemas/com.schema.json \
  --yaml Game/data/coms/training/touch/gentle-embrace.yaml
```

See [Validation and Testing](#validation-and-testing) section for detailed validation workflow.

### Schema References

For automatic IDE validation and autocomplete, add this comment to the top of your YAML files:

```yaml
# yaml-language-server: $schema=../../../../schemas/com.schema.json
```

This enables real-time validation in editors that support the YAML Language Server (VS Code, IntelliJ, etc.).

### Cross-References

- **Schema Validation**: [src/tools/dotnet/YamlValidator/README.md](../../../src/tools/dotnet/YamlValidator/README.md)
- **Schema Generation**: [src/tools/dotnet/YamlSchemaGen/README.md](../../../src/tools/dotnet/YamlSchemaGen/README.md)
- **Data Format Documentation**: [docs/game/data-formats/CSV-YAML-Mapping.md](../data-formats/CSV-YAML-Mapping.md)

---

## Tier 3: ERB Domain (Out of Scope)

**Tier 3 modifications** require C# compilation and are beyond the scope of this modding guide. Tier 3 includes:

- Adding new effect types (requires implementing `IEffectHandler` in C#)
- Creating custom condition types
- Modifying core game engine logic
- Adding new variable types or game systems

**Rationale**: Per the F562/F563 architecture decision, these modifications fall into the ERB domain and require engine-level development. The YAML system provides Tier 1 and Tier 2 moddability to cover the vast majority of content creation use cases without requiring C# knowledge.

For information on why certain features are Tier 3, see:
- [CSV-YAML Mapping Documentation](../data-formats/CSV-YAML-Mapping.md#tier-3-rationale)
- [System Architecture Overview](../architecture/System-Overview.md)

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
| `target` | string | Source parameter name (pain, pleasure, etc.) - **Note**: Not used in current implementation, specify parameters directly |
| `formula` | string | Mathematical expression to evaluate |
| `parameters` | object | Parameter base values (referenced as `baseValue` in formula) |

**Formula Syntax**:

| Feature | Syntax | Example |
|---------|--------|---------|
| Arithmetic | `+`, `-`, `*`, `/` | `baseValue * 2 + 10` |
| Parentheses | `(`, `)` | `(baseValue + 50) * 2` |
| Base value placeholder | `baseValue` | `baseValue / 2` |
| Palam level function | `getPalamLv(name)` | `getPalamLv(pleasure)` |
| Max function | `max(a, b)` | `max(baseValue, 50)` |
| Min function | `min(a, b)` | `min(baseValue * 2, 200)` |

**Effect Structure**:

```yaml
effects:
  - type: source_scale
    parameters:
      pleasure: 100     # Base value for pleasure
    formula: "baseValue * (1 + getPalamLv(pleasure) * 0.15)"
    # Result scales with character's current pleasure level
```

**getPalamLv Function**:

```
getPalamLv(palamName)
```

- **palamName**: Parameter name (pain, pleasure, fear, etc.) or Japanese equivalent
- **Returns**: Current palam value / 1000 (integer division)
- **Example**: If pleasure = 2500, `getPalamLv(pleasure)` returns 2

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
    target: string          # For source_scale: target parameter name (deprecated, use parameters)
    parameters:             # Key-value pairs, content depends on effect type
      [key]: [value]
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
# Game/data/coms/training/touch/caress.yaml
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

### Training/Equipment - Tool-Based Training

**Category Characteristics**:
- Primary effects: source_scale with experience scaling
- Focus: Equipment usage and progressive intensity
- Typical intensity: Variable (scales with use)

**Example: Anal Beads (アナルビーズ)**

```yaml
# Game/data/coms/training/equipment/anal-beads.yaml
id: 46
name: "アナルビーズ"
category: "Training/Equipment"
description: "Anal beads with variable pleasure on insertion/removal, Level 3"
cost:
  stamina: 0
  energy: 0
effects:
  - type: source
    parameters:
      pleasure: 380
      pain: 60
      submission: 280
  - type: downbase
    parameters:
      stamina: 0
      energy: 0
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/Training/Equipment/AnalBeads.cs"
  migration_date: "2026-01-19"
```

**Example: Aphrodisiac (媚薬) with Scaling**

```yaml
# Game/data/coms/training/equipment/aphrodisiac.yaml
id: 187
name: "媚薬"
category: "Training/Equipment"
description: "Aphrodisiac - Equipment command with continuous effect."
cost:
  stamina: 0
  energy: 0
effects:
  - type: source
    parameters:
      lust: 600
      pleasure: 250
  - type: source_scale
    parameters:
      pleasure: 400
    formula: "baseValue * (1 + getPalamLv(lust) * 0.15)"
    # Pleasure scales by 15% per lust level
  - type: downbase
    parameters:
      stamina: 0
      energy: 0
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/Training/Equipment/Aphrodisiac.cs"
  migration_date: "2026-01-19"
```

**Example: Sensitivity Amplification (感度増幅) with Multi-Variable Scaling**

```yaml
# Game/data/coms/training/equipment/sensitivity-amplification.yaml
id: 189
name: "感度増幅"
category: "Training/Equipment"
description: "Sensitivity Amplification - Equipment command with continuous effect."
cost:
  stamina: 0
  energy: 0
effects:
  - type: source_scale
    parameters:
      pleasure: 800
      lust: 500
    formula: "baseValue * (1 + (getPalamLv(lust) + getPalamLv(pleasure)) * 0.08)"
    # Scales based on combined lust and pleasure levels
  - type: downbase
    parameters:
      stamina: 0
      energy: 0
metadata:
  migrated_from_cs: "src/Era.Core/Commands/Com/Training/Equipment/SensitivityAmplification.cs"
  migration_date: "2026-01-19"
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
      pleasure: 100
    formula: "baseValue * (1 + getPalamLv(pleasure) * 0.25)"

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
formula: "baseValue * 2"                    # Double the base value
formula: "baseValue + 50"                   # Add fixed amount
formula: "(baseValue + 100) / 2"            # Average with fixed value
```

**Experience-based scaling**:
```yaml
# Linear scaling: +20 per level
formula: "baseValue + getPalamLv(pleasure) * 20"

# Multiplicative scaling: 15% increase per level
formula: "baseValue * (1 + getPalamLv(pleasure) * 0.15)"

# Capped scaling: max 200
formula: "min(baseValue + getPalamLv(pleasure) * 15, 200)"
```

**Complex formulas**:
```yaml
# Scaling with diminishing returns
formula: "baseValue + min(getPalamLv(pleasure) * 30, 100)"

# Multi-parameter consideration
formula: "baseValue * (1 + (getPalamLv(pleasure) + getPalamLv(lust)) * 0.08)"
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
      resistance: -20   # Reduce resistance progressively
    formula: "baseValue * getPalamLv(submission)"
```

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

### Using YamlValidator

Validate your custom COM files before using them in-game:

```bash
# Single file validation
dotnet run --project src/tools/dotnet/YamlValidator -- \
  --schema src/tools/schemas/com.schema.json \
  --yaml Game/data/coms/training/touch/gentle-embrace.yaml

# Directory validation (all COMs)
dotnet run --project src/tools/dotnet/YamlValidator -- \
  --schema src/tools/schemas/com.schema.json \
  --validate-all Game/data/coms/
```

**Example output on success**:
```
PASS: gentle-embrace.yaml is valid
```

**Example output on failure**:
```
FAIL: gentle-embrace.yaml
Error at effects[0].parameters: Required property 'pleasure' is missing
```

For detailed YamlValidator usage, see [src/tools/dotnet/YamlValidator/README.md](../../../src/tools/dotnet/YamlValidator/README.md).

### Schema Generation

If you need to regenerate the COM schema (e.g., after engine updates):

```bash
dotnet run --project src/tools/dotnet/YamlSchemaGen/
```

For detailed schema generation information, see [src/tools/dotnet/YamlSchemaGen/README.md](../../../src/tools/dotnet/YamlSchemaGen/README.md).

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

2. **Validate the schema**:
   ```bash
   dotnet run --project src/tools/dotnet/YamlValidator -- \
     --schema src/tools/schemas/com.schema.json \
     --yaml Game/data/coms/[Category]/[filename].yaml
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
Error: Failed to evaluate formula 'baseValue ++ 10'
```
Fix: Check formula syntax (++ is invalid, use + only)

**Invalid parameter name**:
```
Error: Unknown source target: 'invalid_name'
```
Fix: Use valid parameter names (pleasure, pain, fear, etc.)

---

## Additional Resources

- **JSON Schema**: `src/tools/schemas/com.schema.json` - Full schema definition
- **Example COMs**: `Game/data/coms/` - Browse existing COMs for patterns
- **Effect Handlers Source**: `src/Era.Core/Effects/` - C# implementation details
- **Runtime Executor**: `src/Era.Core/Commands/Com/YamlComExecutor.cs` - Execution logic
- **Validation Tools**:
  - [YamlValidator README](../../../src/tools/dotnet/YamlValidator/README.md)
  - [YamlSchemaGen README](../../../src/tools/dotnet/YamlSchemaGen/README.md)
- **Data Format Documentation**: [CSV-YAML Mapping](../data-formats/CSV-YAML-Mapping.md)
- **System Architecture**: [System Overview](../architecture/System-Overview.md)

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

**Q: What ID should I use for custom COMs?**

A: Use IDs starting from 9000 to avoid conflicts with existing and future official COMs.

**Q: How do I share my custom COMs with others?**

A: Place your YAML files in `Game/data/coms/` subdirectories and share the files. Other users can copy them to the same location. Always validate with YamlValidator before sharing.

---

**Document Version**: 2.0
**Feature**: F564 - Documentation Consolidation (COM YAML + Phase 17)
**Created**: 2026-01-20
**Updated**: 2026-01-25 - Added Tier 1 and Tier 2 moddability sections
