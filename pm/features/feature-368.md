# Feature 368: Character Setup Functions Migration

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate SYSTEM.ERB external dependencies: SHOP_CUSTOM.ERB, CHARA_CUSTUM.ERB, TALENTCOPY.ERB character setup functions to C#.

**Context**: Phase 3 Task 3 from full-csharp-architecture.md. Supports F365 (SYSTEM.ERB) migration.

**Note**: Combined 613 lines ERB source. In-scope extractable logic: ~50 lines (NormalizeCharacterState ~47 lines + CopyTalents loop ~3 lines). Interactive UI remains in ERB.

---

## Background

### Philosophy (Mid-term Vision)

**Complete Initialization Stack**: F365 migrates SYSTEM.ERB handlers, but they depend on external functions. F368 migrates character setup functions to enable full C# initialization.

### Problem (Current Issue)

SYSTEM.ERB calls these external functions:
- CUSTOM_CHARAMAKE (SHOP_CUSTOM.ERB:1) - creates custom character
- VIRGIN_CUSTOM (SHOP_CUSTOM.ERB:448) - virgin customization
- REVERSEMODE_1 (TALENTCOPY.ERB:6) - talent reversal
- COPY_CUSTOM (TALENTCOPY.ERB:72) - copies talents between characters
- CHARA_CUSTUM (CHARA_CUSTUM.ERB:4) - character customization

**Helper functions** (called internally by CUSTOM_CHARAMAKE):
- CUSTOM_TERMINAL, NAME_CUSTOM, BASE_CUSTOM, TALENT_CUSTOM, ABL_CUSTOM, EXP_CUSTOM, CLOTHES_CUSTOM

**External display functions** (called by CUSTOM_TERMINAL, out of scope - remain in ERB):
- PRINT_STATE_ABL, PRINT_STATE_TALENT, PRINT_STATE_EXP (defined in PRINT_STATE.ERB)

Total: SHOP_CUSTOM.ERB (473 lines), CHARA_CUSTUM.ERB (29 lines), TALENTCOPY.ERB (111 lines) = 613 lines

### Scope Boundaries

**In-scope** (pure logic extractable to C#):
- NormalizeCharacterState(): CUSTOM_TERMINAL lines 21-67 (gender-based ability clearing, virgin status, conflicting talents)
- CopyTalents(): COPY_CUSTOM core logic lines 94-96 (TALENT array copying loop)
- Implementation classes (CharacterSetup.cs, TalentManager.cs)

**Out-of-scope** (interactive UI remains in ERB):
- CUSTOM_TERMINAL menu loop (PRINT/INPUT/RESTART at lines 68-233)
- Helper UI functions: NAME_CUSTOM, BASE_CUSTOM, TALENT_CUSTOM, ABL_CUSTOM, EXP_CUSTOM, CLOTHES_CUSTOM
- VIRGIN_CUSTOM interactive prompts
- COPY_CUSTOM/REVERSEMODE_1 user confirmation dialogs

### Goal (What to Achieve)

1. Analyze target functions and their dependencies
2. Create Era.Core/Common/CharacterSetup.cs with setup functions
3. Create Era.Core/Common/TalentManager.cs with talent functions
4. Create xUnit test cases (CharacterSetupTests.cs)

---

## Function Analysis

### Overview

Based on test requirements in CharacterSetupTests.cs and source ERB files, the following functions need C# migration:

**CharacterSetup.cs** - Character state normalization and validation (Era.Core/Common/)
**TalentManager.cs** - Talent array operations (Era.Core/Common/)

### CharacterSetup.cs

#### Gender Helper Methods

Three bitwise AND helper methods for gender checking:

| Method | Logic | Purpose |
|--------|-------|---------|
| `HasPenis(genderValue)` | `(genderValue & 2) != 0` | Check if character has penis (male=2, hermaphrodite=3) |
| `HasVagina(genderValue)` | `(genderValue & 1) != 0` | Check if character has vagina (female=1, hermaphrodite=3) |
| `IsFemale(genderValue)` | `genderValue == 1` | Check if character is pure female (excludes hermaphrodites) |

**Source**: SHOP_CUSTOM.ERB CUSTOM_TERMINAL lines 21-67 use these checks for conditional attribute clearing.

#### NormalizeCharacterState(CharacterState state)

**Purpose**: Applies gender-based attribute clearing, stamina initialization, virgin consistency checks, talent conflict resolution.

**Returns**: `CharacterState` (modified state object)

**Logic Flow**:
1. **Gender-based clearing** (lines 21-39 in CUSTOM_TERMINAL):
   - If `HasVagina()`: Clear homo-related attributes (`ホモっ気`, `ゲイ中毒`, `ゲイ経験`)
   - If `!HasPenis()`: Clear ejaculation experience (`射精経験`)
   - If `!HasVagina()`: Clear vagina-related attributes (abilities: `Ｖ感覚`, `レズっ気`, `レズ中毒`; experiences: `レズ経験`, `噴乳経験`, `Ｖ経験`, `パイズリ経験`, `Ｖ性交経験`, `Ｖ拡張経験`; talents: `Ｖ感度`, `処女`, `母乳体質`, `バストサイズ`)

2. **Stamina initialization** (lines 41-47):
   - If `HasPenis()`: `MaxBase精力 = Base精力 = 1000`
   - Else: `MaxBase精力 = Base精力 = 0`, clear `童貞` talent

3. **Virgin consistency** (lines 49-60):
   - If `処女` talent set: Clear `Ｖ感覚`, `Ｖ経験`, `Ｖ性交経験`, `Ｖ拡張経験`
   - If `処女` not set AND `Ｖ経験 == 0`: Set `Ｖ経験 = 1` (minimal experience for consistency)

4. **Talent conflict resolution** (lines 62-67):
   - If `肉便器` AND `公衆便所`: Clear `公衆便所` (肉便器 takes priority)
   - If `肉便器` AND `NTR`: Clear `NTR` (肉便器 takes priority)

**Dependencies**: Requires `CharacterState` class with methods `GetTalent()`, `SetTalent()`, `GetAbility()`, `SetAbility()`, `GetExperience()`, `SetExperience()`, properties `MaxBase精力`, `Base精力`.

#### SetVirginStatus(CharacterState state)

**Purpose**: Reset character to virgin state (VIRGIN_CUSTOM logic from SHOP_CUSTOM.ERB:448).

**Returns**: `CharacterState` (modified state)

**Logic**:
- If `IsFemale()` (pure female, excludes hermaphrodites):
  - Set `処女 = 1`
  - Clear `Ｖ経験`, `Ｖ性交経験`, `Ａ性交経験`
  - Clear `Ｖ感覚`, `Ａ感覚` abilities
- Else: No changes (males and hermaphrodites unaffected)

**Source**: SHOP_CUSTOM.ERB VIRGIN_CUSTOM function (lines 448-464).

#### ValidateCharacterId(int characterId)

**Purpose**: Defensive validation for character ID range.

**Logic**:
- If `characterId < 0` OR `characterId > 290` (人物最大 from DIM.ERH): Throw `ArgumentOutOfRangeException`
- Valid range: 0 (あなた) to 290

**Exception**: `ArgumentOutOfRangeException` with `ParamName = "characterId"`

**Usage**: Called at entry points of CharacterSetup methods to prevent invalid array access.

### TalentManager.cs

#### CopyTalents(int[] source, int[] target)

**Purpose**: Copies all 192 talents from source character to target character.

**Parameters**:
- `source`: Source talent array (length 192)
- `target`: Target talent array (length 192)

**Logic**:
```csharp
for (int i = 0; i < 192; i++)
{
    target[i] = source[i];
}
```

**Source**: TALENTCOPY.ERB COPY_CUSTOM lines 94-96 (core loop only, excluding interactive UI).

**Invariants**:
- Source array remains unchanged
- Target array is overwritten completely
- Array length assumed to be 192 (talent count from DIM.ERH)

### CharacterState Class Requirement

**Location**: Era.Core/Common/CharacterState.cs (must be created)

**Purpose**: Encapsulates character state data for manipulation by CharacterSetup methods.

**Required Interface**:
- `int GetTalent(string talentName)` - Get talent value by name
- `void SetTalent(string talentName, int value)` - Set talent value
- `int GetAbility(string abilityName)` - Get ability value by name
- `void SetAbility(string abilityName, int value)` - Set ability value
- `int GetExperience(string expName)` - Get experience value by name
- `void SetExperience(string expName, int value)` - Set experience value
- `int MaxBase精力` - Property for max stamina
- `int Base精力` - Property for current stamina

**Note**: This class acts as a data transfer object (DTO) for character state operations. It does not directly map to ERB global arrays but provides a typed interface for C# manipulation.

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Function analysis documented | file | Grep feature-368.md | contains | "## Function Analysis" | [x] |
| 2 | CharacterSetup.cs created | file | Glob | exists | Era.Core/Common/CharacterSetup.cs | [x] |
| 3 | TalentManager.cs created | file | Glob | exists | Era.Core/Common/TalentManager.cs | [x] |
| 4 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 5 | Unit tests created (covers CharacterSetup + TalentManager) | file | Glob | exists | engine.Tests/Tests/CharacterSetupTests.cs | [x] |
| 6 | Unit tests pass | test | dotnet test | succeeds | - | [x] |
| 7 | Invalid character ID handling (Neg): characterId < 0 or > 人物最大 | test | dotnet test --filter ValidateCharacterId | succeeds | - | [x] |
| 8 | Invalid ID range checking (Neg) | code | Grep CharacterSetup.cs | contains | "ArgumentOutOfRangeException" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze CUSTOM_CHARAMAKE, VIRGIN_CUSTOM, REVERSEMODE_1, COPY_CUSTOM, CHARA_CUSTUM: document behavior, dependencies in "## Function Analysis" section | [x] |
| 2 | 2 | Create CharacterSetup.cs with stub methods + NormalizeCharacterState() (pure logic from CUSTOM_TERMINAL lines 21-67) | [x] |
| 3 | 3 | Create TalentManager.cs with CopyTalents() method (TALENTCOPY.ERB core logic only, excluding interactive UI) | [x] |
| 4 | 4 | Verify C# build succeeds | [x] |
| 5 | 5 | Create unit tests in CharacterSetupTests.cs (covers both CharacterSetup.cs and TalentManager.cs) | [x] |
| 6 | 6 | Verify unit tests pass | [x] |
| 7 | 7 | Add negative test for invalid character ID handling | [x] |
| 8 | 8 | Add defensive guards/null checks in CharacterSetup.cs | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F364 | Constants.cs required for character constants |
| Predecessor | F365 | GameInitialization.cs provides stub interfaces (CustomCharaMake, CharaCustum, etc.) that F368 will implement |

**Dependency Chain**:
```
F364 (Constants) → F365 (SYSTEM.ERB) + F368 (Character) → Full C# initialization
```

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 (lines 886-908)
- [feature-365.md](feature-365.md) - SYSTEM.ERB Migration (parent)
- [feature-364.md](feature-364.md) - DIM.ERH Migration (prerequisite)
- Game/ERB/SHOP_CUSTOM.ERB - Source file (473 lines)
- Game/ERB/CHARA_CUSTUM.ERB - Source file (29 lines)
- Game/ERB/TALENTCOPY.ERB - Source file (111 lines)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | orchestrator | Created as F365 external dependency | PROPOSED |
| 2026-01-06 14:02 | START | implementer | Task 1 | - |
| 2026-01-06 14:08 | START | implementer | Task 2 | - |
| 2026-01-06 14:08 | END | implementer | Task 2 | SUCCESS |
| 2026-01-06 14:02 | END | implementer | Task 1 | SUCCESS |
| 2026-01-06 14:10 | END | orchestrator | Tasks 3-8 | SUCCESS (TalentManager moved to separate file, all tests pass) |
| 2026-01-06 14:15 | REVIEW | feature-reviewer | Post review (mode: post) | NEEDS_REVISION (AC7 filter name, spec alignment) |
| 2026-01-06 14:16 | FIX | orchestrator | AC7 filter, Goal 5, SetVirginStatus spec | FIXED |
| 2026-01-06 14:17 | REVIEW | feature-reviewer | Post review (retry) | NEEDS_REVISION (ReverseMode reference cleanup) |
| 2026-01-06 14:18 | FIX | orchestrator | Removed ReverseMode from scope and Task 3 | FIXED |
