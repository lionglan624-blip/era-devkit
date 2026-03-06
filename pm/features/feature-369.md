# Feature 369: Clothing System Migration

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate SYSTEM.ERB external dependency: CLOTHES.ERB clothing system functions to C#.

**Context**: Phase 3 Task 4 from full-csharp-architecture.md. Supports F365 (SYSTEM.ERB) migration.

**Note**: CLOTHES.ERB is 1,999 lines - large file requiring careful analysis. Focus on CLOTHES_SETTING function called by SYSTEM.ERB.

**Scope Boundaries**:
- **In-scope (F369)**: CLOTHES_SETTING entry point, CLOTHES_Save, CLOTHES_SETTING_TRAIN, CLOTHES_Preset_NUDE, CLOTHES_Change_Knickers, CLOTHES_Change_Bra, CLOTHES_IsDressed
- **Stub dependencies**:
  - 今日のぱんつ → returns 10 (しましまぱんつ青白 - normal default)
  - 今日のぱんつADULT → returns 31 (Tバック白 - adult default)
  - CLOTHES_ACCESSORY → no-op stub (C# will NOT set accessory flags 結婚指輪/チョーカー/首輪 unlike ERB; acceptable for Phase 3 as SYSTEM.ERB still uses ERB path)
  - CLOTHES_Preset_NIGHTWEAR → calls CLOTHES_Preset_NUDE for 寝間着=0, otherwise no-op (character presets out-of-scope for Phase 3)
- **Out-of-scope (future)**: Character presets (CLOTHES_Preset_1-13), generic presets (MALE/FEMALE/MAID/CUSTOM), display helpers (SHOW_TOP/SHOW_BOTTOM)
- **Engine dependency**: CFLAG/EQUIP array access resolved via CSV column indices

---

## Background

### Philosophy (Mid-term Vision)

**Complete Initialization Stack**: F365 migrates SYSTEM.ERB handlers, but they depend on external functions. F369 migrates clothing setup functions to enable full C# initialization.

### Problem (Current Issue)

SYSTEM.ERB calls:
- CLOTHES_SETTING (CLOTHES.ERB:19) - clothing initialization for characters

CLOTHES.ERB contains (1,999 lines, 40+ functions):
- **Entry point**: CLOTHES_SETTING (line 19) - main initialization function
- **Internal helpers**: CLOTHES_Save, CLOTHES_SETTING_TRAIN (bit flag calculation for TEQUIP)
- **Character presets**: 13 functions (CLOTHES_Preset_1-13) for individual characters
- **Generic presets**: NUDE/MALE/FEMALE/MAID/CUSTOM/jentle/NIGHTWEAR
- **State queries**: IsDressed check, clothing validation
- **Display helpers**: SHOW_TOP/SHOW_BOTTOM, visual state output
- **Randomizers**: 今日のぱんつ (daily underwear selection)

### Goal (What to Achieve)

1. Analyze CLOTHES.ERB structure and identify CLOTHES_SETTING dependencies
2. Create Era.Core/Common/ClothingSystem.cs with clothing logic (consolidate all clothing functionality)
3. Create unit tests (positive and negative cases)
4. Integrate with F365 GameInitialization.cs

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CLOTHES.ERB analysis documented | file | Grep | contains | "## CLOTHES.ERB Analysis" | [x] |
| 2 | ClothingSystem.cs created | file | Glob | exists | Era.Core/Common/ClothingSystem.cs | [x] |
| 3 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 4 | Unit tests created | file | Glob | exists | engine.Tests/Tests/ClothingSystemTests.cs | [x] |
| 5 | Unit tests pass | test | dotnet test | succeeds | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze CLOTHES.ERB: identify CLOTHES_SETTING function, internal dependencies, data structures | [x] |
| 2 | 2 | Create ClothingSystem.cs with consolidated clothing logic (ClothesSetting, Save, SettingTrain, PresetNude, ChangeKnickers, ChangeBra, IsDressed) + stub functions (今日のぱんつ, 今日のぱんつADULT, CLOTHES_ACCESSORY, CLOTHES_Preset_NIGHTWEAR) + update GameInitialization.cs stub (parameter: clothingType → changeUnderwear) | [x] |
| 3 | 3,4,5 | Create unit tests, verify build and tests pass | [x] |
| 4 | 3 | Verify Era.Core build succeeds | [x] |
| 5 | 4,5 | Verify ClothingSystemTests.cs exists and tests pass | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F364 | Constants.cs provides character ID constants (人物_美鈴 etc.); CFLAG/EQUIP column indices resolved from CSV files |
| Predecessor | F365 | GameInitialization.cs structure to integrate with |
| Successor | F365 | Enables full SYSTEM.ERB migration |

**Dependency Chain**:
```
F364 (Constants) → F365 (SYSTEM.ERB) + F369 (Clothing) → Full C# initialization
```

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 System Infrastructure (lines 1086-1150)
- [feature-365.md](feature-365.md) - SYSTEM.ERB Migration (parent)
- [feature-364.md](feature-364.md) - DIM.ERH Migration (prerequisite)
- Game/ERB/CLOTHES.ERB - Source file (1,999 lines)

---

## 残課題 (Deferred Tasks)

| Task | Reason | Target Phase | Tracking |
|------|--------|:------------:|:--------:|
| 今日のぱんつ full implementation | Complex 180-line randomizer | Phase 22 | architecture.md Phase 22 Tasks |
| 今日のぱんつADULT full implementation | Adult underwear selection | Phase 22 | architecture.md Phase 22 Tasks |
| CLOTHES_ACCESSORY full implementation | Accessory flags (結婚指輪/チョーカー/首輪) | Phase 22 | architecture.md Phase 22 Tasks |
| CLOTHES_Preset_NIGHTWEAR full implementation | Character nightwear presets | Phase 22 | architecture.md Phase 22 Tasks |

---

## Implementation Notes

**API Naming**: GameInitialization.cs stub uses `ClothesSetting(characterId, clothingType)` but ERB uses `CLOTHES_SETTING(着用者, 下着変更)` where 下着変更 = "underwear change flag" (1=change, 0=keep). ClothingSystem.cs will use correct parameter name `changeUnderwear`. Task includes updating GameInitialization.cs stub for consistency.

**Stub Verification**: Unit tests should verify stub functions (今日のぱんつ → 10, 今日のぱんつADULT → 31, CLOTHES_ACCESSORY → no-op) return expected default values.

---

## CLOTHES.ERB Analysis

### Function Structure (In-Scope)

| Function | Lines | Purpose |
|----------|:-----:|---------|
| CLOTHES_SETTING | 19-171 | Main dispatcher - SELECTCASE for clothing patterns 0-113, 200 |
| CLOTHES_Save | 658-680 | Backup current EQUIP to CFLAG:服装_* (17 slots) |
| CLOTHES_IsDressed | 718-757 | Boolean check - any of 17 EQUIP slots non-zero |
| CLOTHES_Change_Knickers | 762-792 | Underwear selection (priority: 貞操帯>ノーパン>淫乱>普通) |
| CLOTHES_Change_Bra | 797-815 | Bra selection based on CFLAG option bit 1 and TALENT:バストサイズ |
| CLOTHES_SETTING_TRAIN | 827-900 | EQUIP→TEQUIP bit encoding (lower/upper body state) |
| CLOTHES_Preset_NUDE | 173-179 | Clear all EQUIP slots to 0 |

### Stub Functions (Phase 3)

| Function | Stub Return | Reason |
|----------|:----------:|--------|
| 今日のぱんつ | 10 | Complex 180-line randomizer; default しましまぱんつ青白 |
| 今日のぱんつADULT | 31 | Calls 今日のぱんつ(住人,1); default Tバック白 |
| CLOTHES_ACCESSORY | no-op | Sets 結婚指輪/チョーカー/首輪; SYSTEM.ERB uses ERB path |
| CLOTHES_Preset_NIGHTWEAR | NUDE if 寝間着=0 | Character presets out-of-scope |

### Data Structures

**CFLAG Columns**:
- 服装パターン (col 7) - Preset selector 0-113, 200
- 服装パターンオプション (col 8) - Bit 0=ノーパン, Bit 1=ノーブラ
- 寝間着 - Sleepwear type
- 服装_衣服着用 etc. - 17 backup columns

**EQUIP Slots** (17 total):
- 衣服着用, アクセサリ, 帽子, 靴, 靴下, 下半身下着１, 下半身下着２, 下半身上着１, 下半身上着２, スカート, 上半身下着１, 上半身下着２, 上半身上着１, 上半身上着２, ボディースーツ, ワンピース, 着物

**TEQUIP Encoding** (CLOTHES_SETTING_TRAIN):
- ARG:0 - Lower body bits (スカート/ずらし可/ずらし不可/下半身上着)
- ARG:1 - Upper body bits (ノーブラ/はだけ可/はだけ不可)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | orchestrator | Created as F365 external dependency | PROPOSED |
| 2026-01-06 | init | initializer | Status [PROPOSED]→[WIP] | READY |
| 2026-01-06 | investigate | explorer | CLOTHES.ERB structure analysis | READY |
| 2026-01-06 | tdd | implementer | ClothingSystemTests.cs created (337 lines, 14 tests) | RED |
| 2026-01-06 | impl | implementer | ClothingSystem.cs created (229 lines), GameInitialization.cs updated | GREEN |
| 2026-01-06 | verify | ac-tester | All 5 ACs verified PASS | PASS |
| 2026-01-06 | review | feature-reviewer | NEEDS_REVISION: 4 missing in-scope functions | NEEDS_REVISION |
| 2026-01-06 | fix | implementer | Added ClothesSetting, Save, SettingTrain, ChangeBra, PresetNightwear + 17 tests | GREEN |
| 2026-01-06 | review | feature-reviewer | Mode: post - all 7 in-scope functions verified | READY |
| 2026-01-06 | review | feature-reviewer | Mode: doc-check - SKILL.md updated | READY |
