# Feature 376: Header Files Consolidation - .ERH Migration

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate header files (.ERH) to C# constants and configuration classes. This consolidates ColorSettings.erh, 続柄.ERH, and other header files into type-safe C# equivalents.

**Context**: F364 successor, Phase 3 from full-csharp-architecture.md. Follows Constants.cs patterns established in F364.

---

## Background

### Philosophy (Mid-term Vision)

**Foundation-First Migration**: Header files (.ERH) contain constant definitions and configuration data used throughout the codebase. Migrating these to C# enables compile-time validation and prepares Phase 4+ features.

### Problem (Current Issue)

Header files requiring migration (categorized by content type):

**Pure Constants (#DIM CONST only)** - Migrate to C# constants:
- `ColorSettings.erh` - Color palette definitions (37 `#DIM CONST`)
- `NTR_MASTER_3P_SEX.ERH` - NTR 3P configuration (4 `#DIM CONST`)
- `グラフィック表示/立ち絵表示.ERH` - Character sprite settings (4 `#DIM CONST`)

**Mixed Content** - Constants portion to C#, #DEFINE aliases out of scope:
- `続柄.ERH` - Relationship type definitions (55 `#DIM CONST` + 12 `#DEFINE` aliases)

**Runtime Variables** - Out of scope for this feature (not constants):
- `グラフィック表示/グラフィック表示.ERH` (28 lines) - `#DIMS`, `#DIM`, `#DIM SAVEDATA` (runtime variables, not constants)
- `妖精メイド拡張/FairyMaids.erh` (36 lines) - `#DIM CONST`, `#DIM SAVEDATA`, `#DIMS SAVEDATA`, `#DEFINE` (mixed, primarily runtime)

**Current State**:
- All ERB files depend on header files for constants
- No C# equivalent for header constants
- Phase 4+ features cannot reference header constants until migration complete

### Goal (What to Achieve)

1. Analyze all header files and categorize constants
2. Create appropriate C# classes for each header category
3. Implement type-safe constant definitions
4. Create xUnit test cases for constant validation
5. Verify constants match ERB legacy values
6. Document header migration API for Phase 4+ reference

**File Organization**: Large constant groups (30+) get dedicated files (ColorSettings.cs, RelationshipTypes.cs). Small groups (≤10) are added to existing Constants.cs to avoid file proliferation.

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Header files analysis documented | file | Grep "## Header Files Analysis" in Game/agents/feature-376.md | contains | "## Header Files Analysis" | [x] |
| 2 | ColorSettings.cs created with all color constants | file | Glob Era.Core/Common/ColorSettings.cs | exists | Era.Core/Common/ColorSettings.cs | [x] |
| 3 | RelationshipTypes.cs created with relationship constants | file | Glob Era.Core/Common/RelationshipTypes.cs | exists | Era.Core/Common/RelationshipTypes.cs | [x] |
| 4 | NTR3P constants added to Constants.cs | code | Grep "挿入部位3P" in Era.Core/Common/Constants.cs | contains | "挿入部位3P" | [x] |
| 5 | Tachie constants added to Constants.cs | code | Grep "立ち絵" in Era.Core/Common/Constants.cs | contains | "立ち絵" | [x] |
| 6 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 7 | Header constants tests created | file | Glob engine.Tests/Tests/HeaderConstantsTests.cs | exists | engine.Tests/Tests/HeaderConstantsTests.cs | [x] |
| 8 | All constant tests pass | test | dotnet test --filter HeaderConstantsTests | succeeds | - | [x] |
| 9 | Minimum test coverage (5+ test methods) | code | Grep "\\[Fact\\]" in engine.Tests/Tests/HeaderConstantsTests.cs | gte | 5 | [x] |
| 10 | API documentation created | file | Grep "## Header Constants API" in Game/agents/feature-376.md | contains | "## Header Constants API" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze all header files: categorize constants, identify dependencies, document in feature-376.md | [x] |
| 2 | 2 | Create Era.Core/Common/ColorSettings.cs with color palette constants from ColorSettings.erh (hex 0xRRGGBB as int) | [x] |
| 3 | 3 | Create Era.Core/Common/RelationshipTypes.cs with relationship constants from 続柄.ERH (note: 続柄_祖先, 続柄_子孫 use const expressions) | [x] |
| 4 | 4 | Add Ntr3P constants to Era.Core/Common/Constants.cs from NTR_MASTER_3P_SEX.ERH (use #region marker) | [x] |
| 5 | 5 | Add Tachie constants to Era.Core/Common/Constants.cs from 立ち絵表示.ERH (use #region marker) | [x] |
| 6 | 6 | Verify C# build succeeds after header constants migration | [x] |
| 7 | 7 | Create engine.Tests/Tests/HeaderConstantsTests.cs using xUnit patterns | [x] |
| 8 | 8 | Run all header constant tests and verify they pass | [x] |
| 9 | 9 | Verify test coverage: ensure minimum 5 test methods exist | [x] |
| 10 | 10 | Document header constants API with usage examples in feature-376.md | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F364 | Requires Constants.cs patterns |

---

## Links

- [feature-364.md](feature-364.md) - Constants.cs (prerequisite)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 reference
- Game/ERB/ColorSettings.erh - Source file (pure constants)
- Game/ERB/続柄.ERH - Source file (constants portion only)
- Game/ERB/NTR_MASTER_3P_SEX.ERH - Source file (pure constants)
- Game/ERB/グラフィック表示/立ち絵表示.ERH - Source file (pure constants)

---

## Header Files Analysis

### ColorSettings.erh
- **Type**: Pure constants (37 `#DIM CONST`)
- **Content**: Color palette definitions for UI elements
- **Categories**:
  - 清潔度 (Cleanliness levels): 7 colors (0x39E639 to 0xFF4040)
  - 日時 (Time): 2 colors (day/night)
  - 状態 (Status): 9 colors (warnings, sleep, emotions, NTR)
  - 設定 (Settings): 3 colors (positive/negative/disabled)
  - テキスト (Text): 4 colors (fallen, remarks, enabled/disabled)
  - コマンド (Commands): 3 colors (derived, new, disabled)
  - マップ (Map): 6 colors (self, others, lover, caress, sex, chat)
  - 兄弟 (Siblings): 1 color (self)
- **Migration Target**: Era.Core/Common/ColorSettings.cs (dedicated file due to 37 constants)

### 続柄.ERH
- **Type**: Mixed content (55 `#DIM CONST` + 12 `#DEFINE` aliases)
- **Content**: Relationship type definitions for family tree system
- **Constants** (55):
  - Search limits: 3 (尊属探索_上限, 卑属探索_上限, 傍系探索_上限)
  - Base categories: 5 (尊属_父, 尊属_母, 卑属, 傍系, 傍系尊属, 傍系卑属)
  - Ascending relatives: 20 (父 to 十世の祖母)
  - Descending relatives: 10 (子 to 雲孫の孫)
  - Collateral relatives: 15 (兄姉 to 叔父母, 甥姪)
  - Computed: 2 (祖先, 子孫 - use const expressions)
- **#DEFINE aliases** (12): 間柄_* macros for CFLAG access (out of scope)
- **Migration Target**: Era.Core/Common/RelationshipTypes.cs (dedicated file due to 55 constants)
- **Special Note**: 続柄_祖先 and 続柄_子孫 use const expressions: `続柄_尊属_父 + 尊属探索_上限 + 1` and `続柄_卑属 + 卑属探索_上限 + 1`

### NTR_MASTER_3P_SEX.ERH
- **Type**: Pure constants (4 `#DIM CONST`)
- **Content**: NTR 3P configuration
- **Constants**:
  - Array indices: 2 (挿入部位3P_あなた, 挿入部位3P_奴隷)
  - Insertion types: 2 (挿入部位3P_V挿入, 挿入部位3P_A挿入)
- **Migration Target**: Era.Core/Common/Constants.cs (small group, use #region marker)

### 立ち絵表示.ERH
- **Type**: Pure constants (4 `#DIM CONST`)
- **Content**: Character sprite display settings
- **Constants**:
  - Position: 3 (立ち絵位置_左, 立ち絵位置_中央, 立ち絵位置_右)
  - Z-depth: 1 (立ち絵_DEFAULT_ZDEPTH)
- **Migration Target**: Era.Core/Common/Constants.cs (small group, use #region marker)

### Summary
- **Total constants to migrate**: 100 (37 + 55 + 4 + 4)
- **Dedicated files**: 2 (ColorSettings.cs, RelationshipTypes.cs)
- **Additions to Constants.cs**: 2 regions (Ntr3P, Tachie)
- **Out of scope**: #DEFINE aliases in 続柄.ERH (12 macros)

---

## Header Constants API

### ColorSettings Class
**Namespace**: `Era.Core.Common`
**File**: `Era.Core/Common/ColorSettings.cs`
**Purpose**: Color palette definitions for UI elements (0xRRGGBB format)

#### Usage Example
```csharp
using Era.Core.Common;

// Access cleanliness level colors
int cleanLevel1Color = ColorSettings.色設定_清潔度１; // 0x39E639 (Green)
int cleanLevel7Color = ColorSettings.色設定_清潔度７; // 0xFF4040 (Red)

// Access NTR status color
int ntrColor = ColorSettings.色設定_状態_ＮＴＲ; // 0xE6399B

// Access map colors
int mapSelfColor = ColorSettings.色設定_マップ_自分;   // 0x00FF00 (Green)
int mapLoverColor = ColorSettings.色設定_マップ_恋人; // 0xFF9696 (Pink)
```

#### Categories
- **清潔度** (Cleanliness): 7 levels (色設定_清潔度１ to 色設定_清潔度７)
- **日時** (Time): 昼間/夜間 (day/night)
- **状態** (Status): 注意/警告/睡眠可/要睡眠/睡眠中/同行中/激おこ/発情中/理性的/恋慕/ＮＴＲ
- **設定** (Settings): 肯定的/否定的/無効化
- **テキスト** (Text): 陥落/備考/無効/有効
- **コマンド** (Commands): 派生/新規/無効
- **マップ** (Map): 自分/他人/恋人/愛撫/性交/会話
- **兄弟** (Siblings): 自分

**Total**: 37 constants

---

### RelationshipTypes Class
**Namespace**: `Era.Core.Common`
**File**: `Era.Core/Common/RelationshipTypes.cs`
**Purpose**: Family tree relationship type definitions

#### Usage Example
```csharp
using Era.Core.Common;

// Access base categories
int paternalBase = RelationshipTypes.続柄_尊属_父;  // 0
int maternalBase = RelationshipTypes.続柄_尊属_母;  // 10
int descendantBase = RelationshipTypes.続柄_卑属;   // 20

// Access specific relatives
int father = RelationshipTypes.続柄_父;      // 1
int mother = RelationshipTypes.続柄_母;      // 11
int child = RelationshipTypes.続柄_子;       // 21
int sibling = RelationshipTypes.続柄_兄姉;   // 31

// Access computed constants (using const expressions)
int ancestor = RelationshipTypes.続柄_祖先;   // 7 (= 0 + 6 + 1)
int descendant = RelationshipTypes.続柄_子孫; // 29 (= 20 + 8 + 1)

// Access search limits
int ancestorLimit = RelationshipTypes.尊属探索_上限;  // 6
int descendantLimit = RelationshipTypes.卑属探索_上限; // 8
```

#### Categories
- **探索上限** (Search Limits): 尊属探索_上限/卑属探索_上限/傍系探索_上限
- **続柄基礎** (Base Categories): 尊属_父/尊属_母/卑属/傍系/傍系尊属/傍系卑属
- **尊属** (Ascending): 本人/父/祖父/曽祖父/高祖父... (20 constants)
- **卑属** (Descending): 子/孫/曽孫/玄孫/来孫... (10 constants)
- **傍系** (Collateral): 兄姉/弟妹/従兄姉/従弟妹/伯父母/叔父母/甥姪... (15 constants)
- **祖先/子孫** (Computed): 祖先/子孫 (2 constants using const expressions)

**Total**: 55 constants

**Note**: #DEFINE aliases (間柄_*) were not migrated (out of scope, CFLAG macros).

---

### Constants Class Additions
**Namespace**: `Era.Core.Common`
**File**: `Era.Core/Common/Constants.cs`
**Purpose**: Small constant groups added to existing Constants.cs

#### NTR 3P Constants (from NTR_MASTER_3P_SEX.ERH)
```csharp
using Era.Core.Common;

// Array indices for 3P insertion positions
int youIndex = Constants.挿入部位3P_あなた;   // 0
int slaveIndex = Constants.挿入部位3P_奴隷;   // 1

// Insertion type values
int vaginalInsertion = Constants.挿入部位3P_V挿入;  // 0
int analInsertion = Constants.挿入部位3P_A挿入;     // 1
```

**Total**: 4 constants

#### Tachie (Character Sprite) Constants (from 立ち絵表示.ERH)
```csharp
using Era.Core.Common;

// Position constants for tachie placement
int leftPos = Constants.立ち絵位置_左;      // 0
int centerPos = Constants.立ち絵位置_中央;  // 1
int rightPos = Constants.立ち絵位置_右;     // 2

// Default z-depth (higher = further back)
int defaultZDepth = Constants.立ち絵_DEFAULT_ZDEPTH;  // 100
```

**Total**: 4 constants

---

### Migration Summary
- **Total Constants Migrated**: 100 (37 ColorSettings + 55 RelationshipTypes + 4 NTR3P + 4 Tachie)
- **New Files**: 2 (ColorSettings.cs, RelationshipTypes.cs)
- **Modified Files**: 1 (Constants.cs - added 2 regions)
- **Test Coverage**: 23 xUnit tests (100% coverage of all constant categories)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | implementer | Created as F366 successor for header files consolidation | PROPOSED |
| 2026-01-06 | review | fl | FL Iterations 1-7: Fixed dependency (F366→F364), categorized ERH files, updated AC/Task (8→10), naming convention (日本語維持), const expressions note | REVISED |
| 2026-01-06 18:25 | implement | implementer | Tasks 1-10: Analyzed headers, created ColorSettings.cs (37), RelationshipTypes.cs (55), added NTR3P/Tachie to Constants.cs (8), fixed test, all 23 tests pass | SUCCESS |
