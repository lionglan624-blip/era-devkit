# Feature 370: Body & State Systems Initialization Migration

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate SYSTEM.ERB TRYCALL dependencies: 体設定.ERB, 妊娠処理変更パッチ.ERB, 天候.ERB initialization functions to C#.

**Context**: Phase 3 Task 5 from full-csharp-architecture.md. Supports F365 (SYSTEM.ERB) migration.

**Note**: Target functions within source files: @体詳細初期設定 (lines 6-347, ~340 lines), @子宮内体積設定F (lines 32-52, ~20 lines), @気温耐性取得 (lines 777-819, ~42 lines). Total: ~400 executable lines. Files total 3,168 lines but only these functions are in scope.

---

## Background

### Philosophy (Mid-term Vision)

**Complete Initialization Stack**: F365 migrates SYSTEM.ERB handlers, but they depend on external functions. F370 migrates body and state initialization functions.

### Problem (Current Issue)

SYSTEM.ERB calls these via TRYCALL (optional execution):
- 体詳細初期設定 (体設定.ERB:6) - body detail initialization
- 子宮内体積設定 (妊娠処理変更パッチ.ERB:24) - pregnancy-related initialization
- 気温耐性取得 (天候.ERB:777) - temperature resistance initialization

Total: 体設定.ERB (1,974 lines), 妊娠処理変更パッチ.ERB (357 lines), 天候.ERB (837 lines) = 3,168 lines

**TRYCALL Semantics**: These functions are optional - game runs without them. C# migration must preserve this optional behavior.

### Goal (What to Achieve)

1. Analyze target functions and their dependencies
2. Create Era.Core/State/BodySettings.cs with body initialization
3. Create Era.Core/State/PregnancySettings.cs with pregnancy initialization
4. Create Era.Core/State/WeatherSettings.cs with weather/temperature functions
5. Create MSTest test cases
6. Update F365 GameInitialization.cs stubs to delegate to implementation classes (following F367 pattern)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Function analysis documented | file | Grep | contains | "## Function Analysis" in feature-370.md | [x] |
| 2 | BodySettings.cs created | file | Glob | exists | Era.Core/State/BodySettings.cs | [x] |
| 3 | PregnancySettings.cs created | file | Glob | exists | Era.Core/State/PregnancySettings.cs | [x] |
| 4 | WeatherSettings.cs created | file | Glob | exists | Era.Core/State/WeatherSettings.cs | [x] |
| 5 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 6 | Unit tests created | file | Glob | exists | engine.Tests/Tests/StateSettingsTests.cs | [x] |
| 7 | Unit tests pass | test | dotnet test | succeeds | - | [x] |
| 8a | GameInitialization calls BodySettings | file | Grep | contains | "BodySettings." in GameInitialization.cs | [x] |
| 8b | GameInitialization calls PregnancySettings | file | Grep | contains | "PregnancySettings." in GameInitialization.cs | [x] |
| 8c | GameInitialization calls WeatherSettings | file | Grep | contains | "WeatherSettings." in GameInitialization.cs | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze functions and document: 体詳細初期設定→BodySettings.BodyDetailInit(), 子宮内体積設定→PregnancySettings.UterusVolumeInit(), 気温耐性取得→WeatherSettings.TemperatureResistance() | [x] |
| 2 | 2 | Create BodySettings.cs with BodyDetailInit() implementing 体設定.ERB logic | [x] |
| 3 | 3 | Create PregnancySettings.cs with UterusVolumeInit() implementing 妊娠処理変更パッチ.ERB logic | [x] |
| 4 | 4 | Create WeatherSettings.cs with TemperatureResistance(Random) implementing 天候.ERB logic | [x] |
| 5 | 5 | Build solution and verify success | [x] |
| 6 | 6 | Create StateSettingsTests.cs with unit tests | [x] |
| 7 | 7 | Run tests and verify all pass | [x] |
| 8 | 8a,8b,8c | Update GameInitialization.cs stubs to call BodySettings, PregnancySettings, WeatherSettings | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F364 | Constants.cs required for state constants |
| Successor | F365 | Enables F365 to call implemented methods for full SYSTEM.ERB migration |

**Dependency Chain**:
```
F364 (Constants) → F365 (SYSTEM.ERB) + F370 (State) → Full C# initialization
```

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 (lines 886-908)
- [feature-365.md](feature-365.md) - SYSTEM.ERB Migration (parent)
- [feature-364.md](feature-364.md) - DIM.ERH Migration (prerequisite)
- [feature-367.md](feature-367.md) - Accessor Pattern (referenced for method signatures)
- Game/ERB/体設定.ERB - Source file (1,974 lines)
- Game/ERB/妊娠処理変更パッチ.ERB - Source file (357 lines)
- Game/ERB/天候.ERB - Source file (837 lines)

---

## Function Analysis

### Target Functions

| ERB Function | C# Class | C# Method | Lines | Complexity |
|-------------|----------|-----------|-------|------------|
| @体詳細初期設定 | BodySettings | BodyDetailInit(int charId) | 6-34 (dispatcher), 36-347 (sub-functions) | Character-specific body params |
| @子宮内体積設定F | PregnancySettings | UterusVolumeInit(int charId) | 32-52 | Uterus volume calculation (wrapper @子宮内体積設定 at line 24 calls this) |
| @気温耐性取得 | WeatherSettings | TemperatureResistance(Random rng) | 777-819 | Parent inheritance with RAND |

### Variable Dependencies

| Function | Input Variables | Output Variables (CFLAG indices) |
|----------|-----------------|----------------------------------|
| 体詳細初期設定 | Character ID (ARG), TALENT:性別 (for HAS_VAGINA check, bitwise & 1) | 500(髪の長さ), 501(髪の長さ指定), 502(髪オプション1), 503(髪オプション2), 504(髪原色), 505(髪色), 506(目色右), 507(目色左), 508(目つき), 509(瞳オプション1), 510(瞳オプション2), 511(肌原色), 512(肌色), 513(体オプション1), 514(体オプション2), 515(体オプション3), 516(体オプション4) = 17 CFLAG fields per character |
| 子宮内体積設定F | Character ID (ARG), TALENT:体型 (-3 to 3 range) | 350(子宮内体積) - values: 1500/3000/4000/5000/6000/8000/12000 based on body type |
| 気温耐性取得 | CFLAG:73(父親), CFLAG:74(母親), CHARANUM, RAND:2 for inheritance | CFLAG:370(暑さ耐性), CFLAG:371(寒さ耐性) for characters 0-9 (hardcoded) and 10+ (parent inheritance) |

### Complexity Notes

**体詳細初期設定** (BodyDetailInit):
- @体詳細初期設定 (dispatcher, lines 6-34) calls 14 character-specific sub-functions (chars 0-13)
- Sub-functions: @体詳細初期設定0 through @体詳細初期設定13 (lines 36-347)
- Each sub-function sets 17 CFLAG values for body parameters (see Variable Dependencies)
- Character 0 (player) has gender-based branching (IF HAS_VAGINA checks TALENT:性別 & 1)
  - Female: hair length=400, Male: hair length=100
- Other characters (1-13) have fixed body parameters (no gender branching)
- Implementation approach: Dictionary<int, BodyParams> with 14 character entries
- Total: 14 characters × 17 CFLAG fields = 238 CFLAG assignments

**子宮内体積設定F** (UterusVolumeInit):
- Single CFLAG assignment based on TALENT:体型 value
- 7 body type cases: -3/-2/-1/0/1/2/3 → volumes: 1500/3000/4000/5000/6000/8000/12000
- Default case (TALENT:体型 == 0): 5000
- Simple IF-ELSEIF-ELSE chain, no loops

**気温耐性取得** (TemperatureResistance):
- Hardcoded values for characters 0-9 (lines 781-801)
  - 暑さ耐性 (heat): [30,30,35,25,30,100,100,30,20,25]
  - 寒さ耐性 (cold): [10,5,0,10,10,5,5,0,-50,10]
- Loop over children: `FOR LOCAL, 10, CHARANUM` (line 803)
- Parent inheritance with randomization: `RAND:2` (lines 805-811)
  - 50% chance inherit from father (CFLAG:73), 50% from mother (CFLAG:74)
  - Edge cases: if only one parent exists, inherit from that parent
- Method signature requires `Random` parameter for testability (like F367 ShortCutMode)

### Method Signatures

Following F367 accessor-based pattern. GameInitialization stubs remain parameterless (or single param) for ERB compatibility. Implementation classes take accessor parameters for testability:

```csharp
// BodySettings.cs
public static void BodyDetailInit(
    int characterId,
    Func<int, int, int> getCflag,
    Action<int, int, int> setCflag,
    Func<int, int, int> getTalent);  // Need getTalent for HAS_VAGINA check (TALENT:性別 & 1)

// PregnancySettings.cs
public static void UterusVolumeInit(
    int characterId,
    Func<int, int, int> getCflag,
    Action<int, int, int> setCflag,
    Func<int, int, int> getTalent);

// WeatherSettings.cs
public static void TemperatureResistance(
    int characterCount,
    Func<int, int, int> getCflag,
    Action<int, int, int> setCflag,
    Random rng);
```

### Data Structure Design

**BodySettings.cs**:
```csharp
private class BodyParams
{
    public int HairLength { get; set; }
    public int HairLengthCategory { get; set; }
    public int HairOption1 { get; set; }
    public int HairOption2 { get; set; }
    public int HairBaseColor { get; set; }
    public int HairColor { get; set; }
    public int EyeColorRight { get; set; }
    public int EyeColorLeft { get; set; }
    public int EyeExpression { get; set; }
    public int EyeOption1 { get; set; }
    public int EyeOption2 { get; set; }
    public int SkinBaseColor { get; set; }
    public int SkinColor { get; set; }
    public int BodyOption1 { get; set; }
    public int BodyOption2 { get; set; }
    public int BodyOption3 { get; set; }
    public int BodyOption4 { get; set; }
}

// Character 0 has two configs (male/female)
private static readonly Dictionary<int, BodyParams> FemaleBodySettings = new() { /* char 0-13 */ };
private static readonly Dictionary<int, BodyParams> MaleBodySettings = new() { /* char 0 only */ };
```

**PregnancySettings.cs**:
- No static data needed, just switch statement on TALENT:体型

**WeatherSettings.cs**:
```csharp
private static readonly int[] HeatResistanceBase = { 30, 30, 35, 25, 30, 100, 100, 30, 20, 25 };
private static readonly int[] ColdResistanceBase = { 10, 5, 0, 10, 10, 5, 5, 0, -50, 10 };
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | orchestrator | Created as F365 external dependency (TRYCALL targets) | PROPOSED |
| 2026-01-06 16:11 | START | implementer | Task 1: Function analysis | - |
| 2026-01-06 16:11 | END | implementer | Task 1: Analyzed 3 ERB functions, documented variables, complexity, data structures | SUCCESS |
| 2026-01-06 16:13 | START | implementer | Task 4: Create WeatherSettings.cs | - |
| 2026-01-06 16:13 | END | implementer | Task 4: Created WeatherSettings.cs with TemperatureResistance() - Build SUCCESS | SUCCESS |
| 2026-01-06 16:16 | START | implementer | Task 2: Create BodySettings.cs | - |
| 2026-01-06 16:16 | END | implementer | Task 2: Created BodySettings.cs with BodyDetailInit() implementing 体設定.ERB logic for characters 0-13, fixed signature to match tests, all 281 tests pass | SUCCESS |
| 2026-01-06 16:14 | START | implementer | Task 3: Create PregnancySettings.cs | - |
| 2026-01-06 16:14 | END | implementer | Task 3: Created PregnancySettings.cs with UterusVolumeInit() - Build SUCCESS | SUCCESS |
| 2026-01-06 16:20 | START | implementer | Task 8: Update GameInitialization.cs stubs | - |
| 2026-01-06 16:20 | END | implementer | Task 8: Added Era.Core.State using, updated 3 stubs with TODO accessors calling BodySettings/PregnancySettings/WeatherSettings - Build SUCCESS, AC 8a,8b,8c verified | SUCCESS |
