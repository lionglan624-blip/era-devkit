# Feature 375: COMMON_KOJO.ERB Migration - Dialogue System Common Functions

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate COMMON_KOJO.ERB branch determination functions to C# KojoCommon.cs. This establishes type-safe branch level determination functions for kojo dialogue variation.

**Context**: F366 successor, Phase 3 from full-csharp-architecture.md. Phase 3 ordering (no code dependency on CommonFunctions.cs).

---

## Background

### Philosophy (Mid-term Vision)

**Foundation-First Migration**: COMMON_KOJO.ERB contains branch determination functions (49 lines) used by kojo scripts for dialogue variation - ABL/TALENT/EXP threshold checks that return branch levels. Migrating these to C# enables type-safe branch determination and prepares Phase 4+ features.

### Problem (Current Issue)

COMMON_KOJO.ERB (49 lines) contains:
- GET_ABL_BRANCH: Returns 0/1/2 based on ABL thresholds (0, 1, 3)
- GET_TALENT_BRANCH: Returns 0/1 based on TALENT value (==1 check, returns 1 if talent active)
- GET_EXP_BRANCH: Returns 0/1/2 based on EXP thresholds (0, 10, 100)
- TEST wrapper functions for each branch function (ERB-only, not migrated to C#)

**Current State**:
- Kojo scripts use these functions for dialogue variation branching
- No C# equivalent for branch determination
- Phase 4+ features cannot reference KojoCommon until migration complete

**Migration Strategy**: Functions will accept primitive values (int) rather than direct array access. Callers extract values from GlobalStatic arrays (ABL[target][index]) before calling KojoCommon methods. This follows the CommonFunctions.cs pattern established in F366.

### Goal (What to Achieve)

1. Analyze COMMON_KOJO.ERB branch functions (GET_ABL_BRANCH, GET_TALENT_BRANCH, GET_EXP_BRANCH)
2. Create Era.Core/Common/KojoCommon.cs with GetAblBranch, GetTalentBranch, GetExpBranch methods
3. Implement type-safe branch determination with threshold constants
4. Create xUnit test cases covering threshold boundaries (low/mid/high for ABL/EXP, normal/sensitive for TALENT)
5. Verify behavior matches ERB legacy implementation
6. Document KojoCommon API for Phase 4+ reference

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | COMMON_KOJO.ERB analysis documented | file | Grep "## COMMON_KOJO.ERB Analysis" in Game/agents/feature-375.md | contains | "## COMMON_KOJO.ERB Analysis" | [x] |
| 2 | KojoCommon.cs created | file | Glob Era.Core/Common/KojoCommon.cs | exists | Era.Core/Common/KojoCommon.cs | [x] |
| 3 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 4 | KojoCommon tests created | file | Glob engine.Tests/Tests/KojoCommonTests.cs | exists | engine.Tests/Tests/KojoCommonTests.cs | [x] |
| 5 | All kojo tests pass (Pos) | test | dotnet test --filter KojoCommonTests | succeeds | - | [x] |
| 6 | Minimum test coverage exists | code | Grep "\\[Fact\\]" in engine.Tests/Tests/KojoCommonTests.cs | gte | 6 | [x] |
| 7 | API documentation created | file | Grep "## KojoCommon API" in Game/agents/feature-375.md | contains | "## KojoCommon API" | [x] |
| 8 | Invalid input value test exists (Neg) | code | Grep "InvalidInput" in engine.Tests/Tests/KojoCommonTests.cs | contains | "InvalidInput" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze COMMON_KOJO.ERB branch functions: GET_ABL_BRANCH (3 thresholds), GET_TALENT_BRANCH (binary), GET_EXP_BRANCH (3 thresholds), document in feature-375.md | [x] |
| 2 | 2 | Create Era.Core/Common/KojoCommon.cs with dialogue functions | [x] |
| 3 | 3 | Verify C# build succeeds after KojoCommon.cs creation | [x] |
| 4 | 4 | Create engine.Tests/Tests/KojoCommonTests.cs using xUnit patterns (per F366 CommonFunctionsTests.cs) | [x] |
| 5 | 5 | Run all KojoCommon tests and verify they pass | [x] |
| 6 | 6 | Verify test coverage: ensure minimum 6 test methods exist | [x] |
| 7 | 7 | Document KojoCommon API with usage examples in feature-375.md | [x] |
| 8 | 8 | Add negative test for invalid input value handling in KojoCommonTests.cs | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F366 | Phase 3 architectural ordering (no code dependency) |
| Predecessor | F364 | Phase 3 architectural ordering (no code dependency) |

---

## Links

- [feature-366.md](feature-366.md) - COMMON.ERB Migration (predecessor)
- [feature-364.md](feature-364.md) - Constants.cs (prerequisite)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 reference
- Game/ERB/COMMON_KOJO.ERB - Source file to migrate

---

## COMMON_KOJO.ERB Analysis

### Source File Structure

**File**: Game/ERB/COMMON_KOJO.ERB (50 lines total)

**Function Groups**:
1. Branch determination functions (lines 6-29): Core logic for kojo dialogue variation
2. TEST wrapper functions (lines 35-48): ERB-only test support (not migrated to C#)

### Branch Determination Functions

#### 1. GET_ABL_BRANCH (lines 6-13)

**Purpose**: Returns branch level based on ABL (ability) value thresholds.

**ERB Signature**: `@GET_ABL_BRANCH(ARG, ARG:1)`
- ARG = TARGET番号 (target character index)
- ARG:1 = ABL番号 (ability type index)
- RESULT = 0:低 / 1:中 / 2:高 (low/mid/high)

**Logic**:
```erb
SIF ABL:(ARG):(ARG:1) >= 3
    RETURN 2
SIF ABL:(ARG):(ARG:1) >= 1
    RETURN 1
RETURN 0
```

**Thresholds**:
- 0: ABL < 1 (low)
- 1: 1 <= ABL < 3 (mid)
- 2: ABL >= 3 (high)

**Migration Strategy**: Accept primitive `int ablValue` parameter. Caller extracts `ABL[target][ablType]` from GlobalStatic before calling C# method.

#### 2. GET_TALENT_BRANCH (lines 15-20)

**Purpose**: Returns branch level based on TALENT (character trait) activation.

**ERB Signature**: `@GET_TALENT_BRANCH(ARG, ARG:1)`
- ARG = TARGET番号 (target character index)
- ARG:1 = TALENT番号 (talent type index)
- RESULT = 0:通常 / 1:敏感 (normal/sensitive)

**Logic**:
```erb
SIF TALENT:(ARG):(ARG:1) == 1
    RETURN 1
RETURN 0
```

**Thresholds**:
- 0: TALENT != 1 (normal, talent inactive)
- 1: TALENT == 1 (sensitive, talent active)

**Migration Strategy**: Accept primitive `int talentValue` parameter. Caller extracts `TALENT[target][talentType]` from GlobalStatic before calling C# method.

#### 3. GET_EXP_BRANCH (lines 22-29)

**Purpose**: Returns branch level based on EXP (experience) value thresholds.

**ERB Signature**: `@GET_EXP_BRANCH(ARG, ARG:1)`
- ARG = TARGET番号 (target character index)
- ARG:1 = EXP番号 (experience type index)
- RESULT = 0:未経験 / 1:経験少 / 2:経験豊富 (inexperienced/some experience/experienced)

**Logic**:
```erb
SIF EXP:(ARG):(ARG:1) >= 100
    RETURN 2
SIF EXP:(ARG):(ARG:1) >= 10
    RETURN 1
RETURN 0
```

**Thresholds**:
- 0: EXP < 10 (inexperienced)
- 1: 10 <= EXP < 100 (some experience)
- 2: EXP >= 100 (experienced)

**Migration Strategy**: Accept primitive `int expValue` parameter. Caller extracts `EXP[target][expType]` from GlobalStatic before calling C# method.

### TEST Wrapper Functions (Not Migrated)

**Functions**: `@TEST_GET_ABL_BRANCH`, `@TEST_GET_TALENT_BRANCH`, `@TEST_GET_EXP_BRANCH` (lines 35-48)

**Purpose**: ERB test scaffolding that assumes TARGET is already set. Calls core branch functions and prints RESULT.

**Migration Note**: These are ERB-only test helpers. C# tests will use xUnit patterns directly (per F366 CommonFunctionsTests.cs). No C# equivalent needed.

### Summary

**Total Lines**: 50
**Functions to Migrate**: 3 (GET_ABL_BRANCH, GET_TALENT_BRANCH, GET_EXP_BRANCH)
**Test Functions (ERB-only)**: 3 (not migrated)

**C# Method Signatures** (planned):
```csharp
public static int GetAblBranch(int ablValue)
public static int GetTalentBranch(int talentValue)
public static int GetExpBranch(int expValue)
```

**Pattern Consistency**: Follows F366 CommonFunctions.cs pattern - accept primitive values, caller extracts from GlobalStatic arrays.

---

## KojoCommon API

### Overview

KojoCommon.cs provides type-safe branch determination functions for kojo dialogue variation. All methods accept primitive `int` values - callers must extract values from GlobalStatic arrays (ABL, TALENT, EXP) before calling these methods.

**Namespace**: `Era.Core.Common.KojoCommon`

**Pattern**: Callers extract array values → pass to KojoCommon method → receive branch level

### Method Signatures

#### GetAblBranch

```csharp
public static int GetAblBranch(int ablValue)
```

**Purpose**: Determine dialogue branch level based on character ability value.

**Parameters**:
- `ablValue`: Ability value (extracted from `ABL[target][ablType]`)

**Returns**: Branch level
- `0`: Low (ablValue < 1)
- `1`: Mid (1 <= ablValue < 3)
- `2`: High (ablValue >= 3)

**Thresholds**:
- Low threshold: 1
- High threshold: 3

**Usage Example**:
```csharp
// Extract ability value from GlobalStatic
int targetIndex = 5;  // Character index
int ablType = 2;      // Ability type (e.g., 欲望)
int ablValue = GlobalStatic.ABL[targetIndex][ablType];

// Get branch level for dialogue variation
int branchLevel = KojoCommon.GetAblBranch(ablValue);

// Use branch level in kojo dialogue logic
switch (branchLevel)
{
    case 0: // Low ability - innocent/inexperienced dialogue
        break;
    case 1: // Mid ability - developing dialogue
        break;
    case 2: // High ability - experienced dialogue
        break;
}
```

---

#### GetTalentBranch

```csharp
public static int GetTalentBranch(int talentValue)
```

**Purpose**: Determine dialogue branch level based on character talent activation.

**Parameters**:
- `talentValue`: Talent value (extracted from `TALENT[target][talentType]`)

**Returns**: Branch level
- `0`: Normal (talentValue != 1, talent inactive)
- `1`: Sensitive (talentValue == 1, talent active)

**Thresholds**:
- Active threshold: 1 (exact match)

**Usage Example**:
```csharp
// Extract talent value from GlobalStatic
int targetIndex = 5;   // Character index
int talentType = 12;   // Talent type (e.g., 敏感体質)
int talentValue = GlobalStatic.TALENT[targetIndex][talentType];

// Get branch level for dialogue variation
int branchLevel = KojoCommon.GetTalentBranch(talentValue);

// Use branch level in kojo dialogue logic
switch (branchLevel)
{
    case 0: // Normal - standard dialogue
        break;
    case 1: // Sensitive talent active - special dialogue variation
        break;
}
```

---

#### GetExpBranch

```csharp
public static int GetExpBranch(int expValue)
```

**Purpose**: Determine dialogue branch level based on character experience value.

**Parameters**:
- `expValue`: Experience value (extracted from `EXP[target][expType]`)

**Returns**: Branch level
- `0`: Inexperienced (expValue < 10)
- `1`: Some experience (10 <= expValue < 100)
- `2`: Experienced (expValue >= 100)

**Thresholds**:
- Some experience threshold: 10
- Experienced threshold: 100

**Usage Example**:
```csharp
// Extract experience value from GlobalStatic
int targetIndex = 5;  // Character index
int expType = 3;      // Experience type (e.g., Ｃ経験)
int expValue = GlobalStatic.EXP[targetIndex][expType];

// Get branch level for dialogue variation
int branchLevel = KojoCommon.GetExpBranch(expValue);

// Use branch level in kojo dialogue logic
switch (branchLevel)
{
    case 0: // Inexperienced - first-time/virgin dialogue
        break;
    case 1: // Some experience - developing dialogue
        break;
    case 2: // Experienced - veteran dialogue
        break;
}
```

---

### Migration Notes

**ERB → C# Mapping**:

| ERB Function | C# Method | Migration Strategy |
|--------------|-----------|-------------------|
| `@GET_ABL_BRANCH(ARG, ARG:1)` | `GetAblBranch(int ablValue)` | Caller extracts `ABL[target][ablType]` before calling |
| `@GET_TALENT_BRANCH(ARG, ARG:1)` | `GetTalentBranch(int talentValue)` | Caller extracts `TALENT[target][talentType]` before calling |
| `@GET_EXP_BRANCH(ARG, ARG:1)` | `GetExpBranch(int expValue)` | Caller extracts `EXP[target][expType]` before calling |

**Key Differences**:
- ERB functions accept target/index parameters and access arrays directly
- C# methods accept primitive values only - caller responsible for array extraction
- This pattern matches F366 CommonFunctions.cs migration strategy

**Backward Compatibility**: ERB functions remain in COMMON_KOJO.ERB for legacy kojo scripts. C# methods provide type-safe alternative for Phase 4+ features.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | implementer | Created as F366 successor for dialogue system migration | PROPOSED |
| 2026-01-06 11:42 | START | implementer | Task 1 | - |
| 2026-01-06 11:43 | END | implementer | Task 1 | SUCCESS |
| 2026-01-06 18:34 | START | implementer | Task 2 | - |
| 2026-01-06 18:35 | END | implementer | Task 2 | SUCCESS |
| 2026-01-06 18:36 | START | implementer | Tasks 4-6 verification | - |
| 2026-01-06 18:37 | END | implementer | Tasks 4-6 verification | SUCCESS |
| 2026-01-06 19:08 | START | implementer | Task 7 | - |
| 2026-01-06 19:09 | END | implementer | Task 7 | SUCCESS |
