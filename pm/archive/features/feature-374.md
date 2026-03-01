# Feature 374: COMMON_J.ERB Migration - Success Rate Calculator

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate COMMON_J.ERB @GET_SUCCESS_RATE function to C# SuccessRateCalculator.cs. This establishes type-safe success rate calculation based on character traits, abilities, and relationships.

**Context**: F366 successor, Phase 3 from full-csharp-architecture.md. Requires F366 CommonFunctions.cs completion.

---

## Background

### Philosophy (Mid-term Vision)

**Foundation-First Migration**: COMMON_J.ERB contains @GET_SUCCESS_RATE (75 lines) - a core calculation function used for determining success probability based on character abilities (ABL:従順), talents (態度, 度胸, プライド, etc.), relationships (RELATION), and flags (CFLAG:好感度, 館内地位). Migrating to C# enables type-safe calculations and prepares Phase 4+ features.

### Problem (Current Issue)

COMMON_J.ERB (75 lines) contains:
- @GET_SUCCESS_RATE function calculating success probability
- ABL:従順 and PALAM:恭順 contributions
- TALENT modifiers (態度, 度胸, プライド, 目立ちたがり, 自己愛, 抵抗, 即落ち, 性別嗜好)
- PLAYER TALENT bonuses (魅惑, 謎の魅力)
- RELATION range-based adjustments
- CFLAG:好感度 scaling and 恋慕/親愛 bonuses
- 館内地位 difference calculation
- LOCAL clamping (minimum 0)

**Current State**:
- ERB files call @GET_SUCCESS_RATE for success probability
- No C# equivalent for success rate calculation
- Phase 4+ features cannot reference SuccessRateCalculator until migration complete

### Goal (What to Achieve)

1. Analyze @GET_SUCCESS_RATE function logic and dependencies
2. Create Era.Core/Common/SuccessRateCalculator.cs with calculation logic
3. Implement type-safe success rate calculation
4. Create xUnit test cases covering all branches
5. Verify behavior matches ERB legacy implementation
6. Document SuccessRateCalculator API for Phase 4+ reference

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Analysis section exists in feature-374.md | file | Grep | contains | "## GET_SUCCESS_RATE Analysis" | [x] |
| 2 | SuccessRateCalculator.cs created | file | Glob | exists | Era.Core/Common/SuccessRateCalculator.cs | [x] |
| 3 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 4 | SuccessRateCalculator tests created | file | Glob | exists | engine.Tests/Tests/SuccessRateCalculatorTests.cs | [x] |
| 5 | All SuccessRateCalculator tests pass (Pos) | test | dotnet test --filter SuccessRateCalculatorTests | succeeds | - | [x] |
| 6 | Minimum test coverage exists | code | Grep | gte | 10 | [x] |
| 7 | API documentation section exists in feature-374.md | file | Grep | contains | "## SuccessRateCalculator API" | [x] |
| 8 | Boundary value test exists (Neg) | code | Grep | contains | "Clamp" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze @GET_SUCCESS_RATE: document calculation logic, ABL/TALENT/RELATION/CFLAG dependencies in feature-374.md | [x] |
| 2 | 2 | Create Era.Core/Common/SuccessRateCalculator.cs with success rate calculation logic | [x] |
| 3 | 3 | Verify C# build succeeds after SuccessRateCalculator.cs creation | [x] |
| 4 | 4 | Create engine.Tests/Tests/SuccessRateCalculatorTests.cs using xUnit patterns | [x] |
| 5 | 5 | Run all SuccessRateCalculator tests and verify they pass | [x] |
| 6 | 6 | Verify test coverage: ensure minimum 10 test methods exist | [x] |
| 7 | 7 | Document SuccessRateCalculator API with usage examples in feature-374.md | [x] |
| 8 | 8 | Add boundary value test for LOCAL < 0 clamping in SuccessRateCalculatorTests.cs | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F366 | Requires CommonFunctions.cs completion (provides pattern) |

**Runtime Dependencies**:
- ABL array (ABL:従順)
- PALAM array (PALAM:恭順)
- TALENT array (態度, 度胸, プライド, 目立ちたがり, 自己愛, 抵抗, 即落ち, 性別嗜好, 恋慕, PLAYER:魅惑, PLAYER:謎の魅力)
- RELATION array
- CFLAG array (好感度, 館内地位)
- GETPALAMLV (ERB built-in for PALAM level calculation)
- HAS_VAGINA (from CommonFunctions.cs F366)

---

## Links

- [feature-366.md](feature-366.md) - COMMON.ERB Migration (predecessor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 reference
- Game/ERB/COMMON_J.ERB - Source file to migrate (75 lines)

---

## GET_SUCCESS_RATE Analysis

### Function Overview
`@GET_SUCCESS_RATE` calculates success probability for actions based on character traits, abilities, relationships, and status. Returns an integer value (clamped to minimum 0).

### Calculation Components

1. **Base Obedience (ABL:従順)**
   - Formula: `ABL:従順 * 5`
   - Main contributor to success rate

2. **Submission Parameter (PALAM:恭順)**
   - Formula: `GETPALAMLV(PALAM:恭順, 5) * 3`
   - Uses parameter level calculation (0-10 scale)

3. **TALENT Modifiers (Target Character)**
   - `態度` (Attitude): `-5` per level (rebellious)
   - `度胸 > 0` (Courage): `-5` (courageous)
   - `プライド > 0` (High Pride): `-15`
   - `プライド < 0` (Low Pride): `+5`
   - `目立ちたがり` (Attention-seeking): `+2`
   - `自己愛 < 0` (Suppression): `-10`
   - `抵抗` (Resistance): `-10`
   - `即落ち` (Instant Fall): `+10`

4. **Gender Preference Penalty**
   - Formula: `(TALENT:性別嗜好 & 1 && !HAS_VAGINA(PLAYER)) || (TALENT:性別嗜好 & 2 && HAS_VAGINA(PLAYER))` → `-7`
   - Bit 0: Prefers male (性別嗜好 & 1)
   - Bit 1: Prefers female (性別嗜好 & 2)
   - Penalty applied when preference matches player gender (unwanted attention)

5. **Player TALENT Bonuses**
   - `魅惑` (Charm): `+6`
   - `謎の魅力` (Mysterious Charm): `+6`

6. **RELATION Range Adjustments**
   - `0 < RELATION < 30`: `-10`
   - `0 < RELATION < 70`: `-6`
   - `0 < RELATION < 100`: `-3`
   - `100 <= RELATION < 130`: `+3`
   - `100 <= RELATION < 170`: `+6`
   - `RELATION >= 170`: `+10`
   - Note: ERB SELECTCASE with multiple conditions uses AND logic

7. **Favorability (CFLAG:好感度)**
   - Formula: `CFLAG:好感度 / 50`
   - Integer division (rounds down)

8. **Love/Affection Bonuses (TALENT:恋慕)**
   - `恋慕 == 1` (Love): `+20`
   - `恋慕 > 1` (Affection/親愛): `+20 + 40 = +60` total

9. **Status Differential**
   - Formula: `(CFLAG:MASTER:館内地位 - CFLAG:TARGET:館内地位) / 500`
   - Integer division

10. **Clamping**
    - Formula: `MAX(0, result)`
    - No upper limit

### Dependencies
- `ABL:従順` array
- `PALAM:恭順` array (requires GETPALAMLV for level calculation)
- `TALENT` arrays (9 target talents + 2 player talents + 1 love talent)
- `RELATION` array
- `CFLAG` arrays (好感度, 館内地位 for both MASTER and TARGET)
- `HAS_VAGINA()` function from CommonFunctions.cs (F366)

---

## SuccessRateCalculator API

### Namespace
`Era.Core.Common`

### Class
`SuccessRateCalculator` (static)

### Method

```csharp
public static int GetSuccessRate(
    int ablObedience,
    int palamSubmission,
    int talentAttitude,
    int talentCourage,
    int talentPride,
    int talentAttentionSeeking,
    int talentSelfLove,
    int talentResistance,
    int talentInstantFall,
    int talentGenderPreference,
    bool playerHasVagina,
    int talentPlayerCharm,
    int talentPlayerMysteriousCharm,
    int relation,
    int cflagFavorability,
    int talentLove,
    int cflagMasterStatus,
    int cflagTargetStatus)
```

### Returns
`int` - Success rate percentage (0+ with no upper limit, clamped to minimum 0)

### Parameters

| Parameter | Type | Description | ERB Source |
|-----------|------|-------------|------------|
| `ablObedience` | int | Obedience ability (主要な成功率決定要素) | `ABL:従順` |
| `palamSubmission` | int | Submission parameter level (0-10 scale) | `GETPALAMLV(PALAM:恭順, 5)` |
| `talentAttitude` | int | Attitude level (negative = rebellious) | `TALENT:態度` |
| `talentCourage` | int | Courage level (>0 reduces success) | `TALENT:度胸` |
| `talentPride` | int | Pride level (>0 reduces, <0 increases) | `TALENT:プライド` |
| `talentAttentionSeeking` | int | Attention-seeking trait (increases success) | `TALENT:目立ちたがり` |
| `talentSelfLove` | int | Self-love level (<0 reduces success) | `TALENT:自己愛` |
| `talentResistance` | int | Resistance trait (reduces success) | `TALENT:抵抗` |
| `talentInstantFall` | int | Instant fall trait (increases success) | `TALENT:即落ち` |
| `talentGenderPreference` | int | Gender preference bitfield (bit0=male, bit1=female) | `TALENT:性別嗜好` |
| `playerHasVagina` | bool | Player gender (true if female/futanari) | `HAS_VAGINA(PLAYER)` |
| `talentPlayerCharm` | int | Player charm talent (increases success) | `TALENT:PLAYER:魅惑` |
| `talentPlayerMysteriousCharm` | int | Player mysterious charm talent (increases success) | `TALENT:PLAYER:謎の魅力` |
| `relation` | int | Relationship value (0-200+) | `RELATION:(NO:PLAYER)` |
| `cflagFavorability` | int | Favorability value | `CFLAG:好感度` |
| `talentLove` | int | Love/affection level (0=none, 1=love, 2+=affection) | `TALENT:恋慕` |
| `cflagMasterStatus` | int | Master's mansion status | `CFLAG:MASTER:館内地位` |
| `cflagTargetStatus` | int | Target's mansion status | `CFLAG:TARGET:館内地位` |

### Usage Example

```csharp
using Era.Core.Common;

// Calculate success rate for a command
int successRate = SuccessRateCalculator.GetSuccessRate(
    ablObedience: characterObedience,
    palamSubmission: submissionLevel,
    talentAttitude: attitudeValue,
    talentCourage: courageValue,
    talentPride: prideValue,
    talentAttentionSeeking: attentionSeekingValue,
    talentSelfLove: selfLoveValue,
    talentResistance: resistanceValue,
    talentInstantFall: instantFallValue,
    talentGenderPreference: genderPreferenceValue,
    playerHasVagina: playerGender,
    talentPlayerCharm: playerCharmValue,
    talentPlayerMysteriousCharm: playerMysteriousCharmValue,
    relation: relationValue,
    cflagFavorability: favorabilityValue,
    talentLove: loveValue,
    cflagMasterStatus: masterStatusValue,
    cflagTargetStatus: targetStatusValue
);

// Use success rate for probability check
bool success = UnityEngine.Random.Range(0, 100) < successRate;
```

### Test Coverage

21 test methods covering:
- Basic calculation (zero inputs, obedience only, submission only, combined modifiers)
- TALENT modifiers (pride, instant fall, resistance)
- Gender preference logic (bit 0, bit 1 matching)
- RELATION range adjustments (very low, very high, boundary values)
- Love/affection bonuses
- Boundary tests (negative clamping, no upper clamping)
- Edge cases (relation boundaries 99/100)
- Complex scenarios (mixed positive/negative, status differential, favorability division)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | implementer | Created as F366 successor for Japanese localization migration | PROPOSED |
| 2026-01-06 | review | FL | Discovered spec mismatch: COMMON_J.ERB is @GET_SUCCESS_RATE (75 lines), not localization helpers | NEEDS_REVISION |
| 2026-01-06 | fix | FL | Rewrote spec to match actual COMMON_J.ERB content (Success Rate Calculator) | REVISED |
| 2026-01-06 18:00 | START | implementer | Tasks 1-3, 5-8 (Phase 4 Implementation) | - |
| 2026-01-06 18:00 | END | implementer | Tasks 1-3, 5-8 | SUCCESS |
