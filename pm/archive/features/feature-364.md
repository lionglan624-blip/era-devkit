# Feature 364: DIM.ERH Variable Definition Migration

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate DIM.ERH variable definitions to C# Constants.cs and VariableDefinitions. This is the CRITICAL foundation - all subsequent Phase 3-12 features depend on Constants.cs for variable/array declarations.

**Context**: Phase 3 Task 6 from full-csharp-architecture.md. Must be completed FIRST before any other Phase 3 migrations.

---

## Background

### Philosophy (Mid-term Vision)

**Foundation-First Migration**: DIM.ERH defines all global variables and arrays used throughout the game. Converting this to C# Constants.cs establishes type-safe variable access for all subsequent phases. Without this, no other ERB→C# migration can reference variables correctly.

### Problem (Current Issue)

DIM.ERH (572 lines verified per F363 analysis) contains:
- Global variable declarations (#DIM, #DIMS, #DIM CONST, #DIM SAVEDATA, etc.)
- Array declarations with size definitions
- Character attribute arrays (ABL, TALENT, MARK, etc.)
- Game state variables (FLAG, TFLAG, etc.)

**Current State**:
- All ERB files depend on DIM.ERH declarations
- No C# equivalent exists for type-safe variable access
- Phase 4-12 migrations blocked until Constants.cs is available

### Goal (What to Achieve)

1. Analyze DIM.ERH structure and categorize variable types
2. Create Era.Core/Common/Constants.cs with all variable/array definitions
3. Create Era.Core/Common/VariableDefinitions.cs for runtime variable initialization
4. Create MSTest tests for variable access validation
5. Verify all variable declarations accessible from C# code
6. Document migration patterns for subsequent Phase 3 features

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | DIM.ERH migration analysis documented | file | Grep | contains | "## DIM.ERH Migration Analysis" in feature-364.md | [x] |
| 2 | Constants.cs created (#DIM CONST values) | file | Glob | exists | Era.Core/Common/Constants.cs | [x] |
| 3 | VariableDefinitions.cs created (SAVEDATA/CHARADATA/#DEFINE) | file | Glob | exists | Era.Core/Common/VariableDefinitions.cs | [x] |
| 4 | ConstantsTests.cs created | file | Glob | exists | engine.Tests/Tests/ConstantsTests.cs | [x] |
| 5 | C# build succeeds | test | dotnet build engine.Tests/ | succeeds | - | [x] |
| 6 | engine.Tests pass | test | dotnet test | succeeds | engine.Tests/ | [x] |
| 7 | Variable access test | test | dotnet test | contains | "ConstantsTests" | [x] |
| 8 | Migration pattern documented | file | Grep | contains | "## Migration Pattern" in feature-364.md | [x] |
| 9 | Negative test: invalid constant access | test | dotnet test | contains | "InvalidConstant" | [x] |
| 10 | Negative test: boundary value verification | test | dotnet test | contains | "BoundaryValue" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze DIM.ERH: categorize #DIM/#DIMS/#DIM CONST/#DIM SAVEDATA/#DIM CHARADATA/#DEFINE, count variables, document C# type mapping | [x] |
| 2a | 2 | Create Era.Core/Common/ directory and Constants.cs with #DIM CONST values | [x] |
| 2b | 3 | Create VariableDefinitions.cs with SAVEDATA/CHARADATA declarations and #DEFINE alias handling | [x] |
| 3 | 4,5,6,7,9,10 | Create engine.Tests/Tests/ConstantsTests.cs for variable access validation (per F363 Phase 3 Test Plan), verify build and tests pass, include negative tests | [x] |
| 4 | 8 | Document migration pattern for F365-F371 reference (how to access Constants.cs from migrated code) | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F363 | Phase 3 Planning complete (DONE) |
| Predecessor | F359 | MSTest structure available for testing |
| Successor | F365 | SYSTEM.ERB needs Constants.cs for variable access |
| Successor | F366 | COMMON.ERB needs Constants.cs for variable access |
| Successor | All F367-F371 | All Phase 3 features depend on Constants.cs |

**Dependency Chain**:
```
F363 (Planning) → [F364 DIM.ERH FIRST] → F365, F366, F367-F371 (parallel possible after F364)
```

**Critical Path**: F364 MUST complete before ANY other Phase 3 feature can begin.

**Note**: F365 and F366 can begin in parallel after F364 completes (see F363 Phase 3 Analysis).

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 Task 6 (line 954)
- [feature-363.md](feature-363.md) - Phase 3 Planning (parent)
- [feature-359.md](feature-359.md) - Test Structure (MSTest foundation)
- [feature-365.md](feature-365.md) - SYSTEM.ERB Migration (successor, PROPOSED)
- [feature-366.md](feature-366.md) - COMMON.ERB Migration (successor, PROPOSED)
- Game/ERB/DIM.ERH - Source file to migrate
- F367-F371 (planned successors, feature files to be created after F364 completion)

---

## DIM.ERH Migration Analysis

### Variable Categorization

DIM.ERH (572 lines total) contains the following variable declarations:

| Category | Count | ERB Syntax | Purpose |
|----------|------:|------------|---------|
| **#DIM CONST** | 427 | `#DIM CONST name = value` | Immutable game constants (character IDs, location codes, NTR system constants) |
| **#DIM SAVEDATA** | 24 | `#DIM SAVEDATA name [, dim1, dim2]` | Persistent variables saved/loaded with game state |
| **#DIM CHARADATA** | 1 | `#DIM CHARADATA name, size` | Per-character arrays (movement routes) |
| **#DEFINE** | 4 | `#DEFINE alias expression` | Bitwise aliases for existing variables (TIME:1, DAY:1, DAY:2, 既成事実&1p0) |
| **#DIM** | 1 | `#DIM name` | Plain scalar variables (霊夢移動フラグ) |
| **#DIMS** | 1 | `#DIMS name, size` | String arrays (依頼内容) |
| **Total** | **458** | - | - |

### C# Type Mapping Decisions

| ERB Declaration | C# Equivalent | Rationale |
|-----------------|---------------|-----------|
| `#DIM CONST name = value` | `public const int name = value;` | Direct 1:1 mapping. All constants are integers. |
| `#DIM CONST 訪問者宅_牢屋解錠済 = 1p1` | `public const int 訪問者宅_牢屋解錠済 = 1 << 1;` | Bitfield notation `1pN` → `1 << N` |
| `#DIM CONST 人物_美鈴・中黒 = 1` | `public const int 人物_美鈴_中黒 = 1;` | Middle dot `・` → underscore `_` (C# identifier rules) |
| `#DIM SAVEDATA 親別出産数, 305, 305` | `public const string SaveData_BirthCountByParent = "親別出産数";`<br>`public const int SaveData_BirthCountByParent_Dim1 = 305;`<br>`public const int SaveData_BirthCountByParent_Dim2 = 305;` | Document metadata as string constants + size constants. Runtime allocation handled by engine. |
| `#DIM CHARADATA 移動ルート, 移動ルートMAX` | `public const string CharaData_MovementRoute = "移動ルート";`<br>`public const int CharaData_MovementRoute_Size = 13;` | Size resolved from referenced constant. |
| `#DEFINE 天候値 TIME:1` | `// #DEFINE 天候値 TIME:1`<br>`// Bitwise alias: Access TIME array index 1 for weather value` | Documented as comment. Requires runtime array access implementation. |
| `#DEFINE 告白成功済 既成事実&1p0` | `// #DEFINE 告白成功済 既成事実&1p0`<br>`// Bitwise alias: GETBIT(既成事実, 0) for confession success flag` | Documented as comment. Requires GETBIT implementation. |
| `#DIM 霊夢移動フラグ` | `public const string PlainVar_ReimuMovementFlag = "霊夢移動フラグ";` | Document as string constant for runtime allocation. |
| `#DIMS 依頼内容, 8` | `public const string StringArray_RequestContent = "依頼内容";`<br>`public const int StringArray_RequestContent_Size = 8;` | Document metadata. Engine handles string array allocation. |

### Migration Challenges

1. **Bitfield Notation**: ERB `1p0`, `1p1`, etc. → C# `1 << 0`, `1 << 1` (bit shift operators)
2. **Character Restrictions**: Middle dot `・` not allowed in C# identifiers → replaced with underscore `_`
3. **#DEFINE Complexity**: Bitwise aliases like `既成事実&1p0` require runtime evaluation, cannot be const
4. **Array Size References**: `#DIM CHARADATA 移動ルート, 移動ルートMAX` requires resolving constant references
5. **SAVEDATA 2D Arrays**: `親別出産数, 305, 305` represents 305×305 matrix requiring special handling

### Implementation Status

| File | Lines | Status | Notes |
|------|------:|:------:|-------|
| `Era.Core/Common/Constants.cs` | ~572 | ✅ DONE | All 427 #DIM CONST values migrated |
| `Era.Core/Common/VariableDefinitions.cs` | ~120 | ✅ DONE | SAVEDATA (25 vars), CHARADATA (1 array), #DEFINE (4 aliases), #DIM/#DIMS (2 vars) |
| `engine.Tests/Tests/ConstantsTests.cs` | ~140 | ✅ DONE | 18 tests (positive + negative + boundary) |

### File Structure

**Constants.cs**: Pure `public const int` declarations grouped by semantic category (人物, 場所, NTR system, etc.)

**VariableDefinitions.cs**: String constants + metadata for runtime variable allocation. Uses nested classes for logical grouping (e.g., `VisitorAppearance` for 24 visitor-related SAVEDATA variables).

**Test Coverage**: ConstantsTests.cs validates:
- All constant values accessible
- Character ID constants correct
- Location ID constants correct
- NTR system constants correct
- Bitfield constants (1<<N) correct
- Negative tests: Invalid constant access detection
- Boundary tests: Edge case values (e.g., 人物最大=290)

---

## Migration Pattern

### Overview

Phase 3 features (F365-F371) will migrate ERB functions to C#. These functions must access game constants defined in DIM.ERH. This section documents how to access Constants.cs from migrated code.

### Namespace Import

All Phase 3+ C# code that references game constants must include:

```csharp
using Era.Core.Common;
```

### Constant Access Pattern

#### ERB Original

```erb
#DIM CONST 人物_美鈴 = 1

@FUNCTION_EXAMPLE
  IF TARGET == 人物_美鈴
    PRINTFORMW 美鈴との会話です
  ENDIF
```

#### C# Migration

```csharp
using Era.Core.Common;

public class FunctionExample
{
    public void Execute(int target)
    {
        if (target == Constants.人物_美鈴)
        {
            Console.WriteLine("美鈴との会話です");
        }
    }
}
```

### Common Migration Patterns

| ERB Pattern | C# Equivalent | Notes |
|-------------|---------------|-------|
| `人物_美鈴` | `Constants.人物_美鈴` | Direct constant reference |
| `IF TARGET == 人物_美鈴` | `if (target == Constants.人物_美鈴)` | Comparison |
| `CFLAG:人物_美鈴:5` | `cflag[Constants.人物_美鈴, 5]` | Array indexing with constant |
| `SELECTCASE TFLAG:1` | `switch (tflag[1])` | Switch statement |
| `CASE 場所_正門` | `case Constants.場所_正門:` | Case label |
| `場所_訪問者宅` | `Constants.場所_訪問者宅` | Direct reference |
| `NTR_MOOD_性交` | `Constants.NTR_MOOD_性交` | NTR system constant |
| `1p0`, `1p1`, `1p5` | `1 << 0`, `1 << 1`, `1 << 5` | Bitfield notation (already migrated) |

### Location Constant Example (F365 SYSTEM.ERB)

#### ERB Original

```erb
@FUNCTION_NTR_CHECK_LOCATION
  IF CFLAG:TARGET:居場所 == 場所_訪問者宅
    RETURNF 1
  ELSEIF CFLAG:TARGET:居場所 == 場所_売春宿
    RETURNF 1
  ELSE
    RETURNF 0
  ENDIF
```

#### C# Migration

```csharp
using Era.Core.Common;

public static class NtrSystemFunctions
{
    public static int NTR_CHECK_LOCATION(int target, int[,] cflag)
    {
        int location = cflag[target, 居場所Index]; // 居場所 index from CSV

        if (location == Constants.場所_訪問者宅)
        {
            return 1;
        }
        else if (location == Constants.場所_売春宿)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
```

### Character ID Example (F366 COMMON.ERB)

#### ERB Original

```erb
@FUNCTION_IS_MAIN_CHARACTER, ARG
  SELECTCASE ARG
    CASE 人物_美鈴, 人物_小悪魔, 人物_パチュリー, 人物_咲夜
      RETURNF 1
    CASE 人物_レミリア, 人物_フラン
      RETURNF 1
    CASEELSE
      RETURNF 0
  ENDSELECT
```

#### C# Migration

```csharp
using Era.Core.Common;

public static class CharacterFunctions
{
    public static int IS_MAIN_CHARACTER(int characterId)
    {
        switch (characterId)
        {
            case Constants.人物_美鈴:
            case Constants.人物_小悪魔:
            case Constants.人物_パチュリー:
            case Constants.人物_咲夜:
            case Constants.人物_レミリア:
            case Constants.人物_フラン:
                return 1;
            default:
                return 0;
        }
    }
}
```

### Bitfield Constant Example (NTR System)

#### ERB Original

```erb
; DIM.ERH definition
#DIM CONST 訪問者宅_牢屋解錠済 = 1p1
#DIM CONST 訪問者宅_通路解錠済 = 1p2

@FUNCTION_CHECK_UNLOCK_STATUS
  IF FLAG:訪問者宅監禁状況 & 訪問者宅_牢屋解錠済
    PRINTFORMW 牢屋は解錠済みです
  ENDIF
```

#### C# Migration

```csharp
using Era.Core.Common;

public static class UnlockFunctions
{
    public static void CHECK_UNLOCK_STATUS(int[] flag)
    {
        // 訪問者宅監禁状況 is FLAG index (assume index known from CSV)
        int unlockStatus = flag[訪問者宅監禁状況Index];

        if ((unlockStatus & Constants.訪問者宅_牢屋解錠済) != 0)
        {
            Console.WriteLine("牢屋は解錠済みです");
        }
    }
}
```

Note: `1p1` in ERB has been migrated to `1 << 1` in Constants.cs, so bitwise operations work identically.

### Array Size Constant Example

#### ERB Original

```erb
#DIM CONST 移動ルートMAX = 13
#DIM CHARADATA 移動ルート, 移動ルートMAX

@FUNCTION_CLEAR_ROUTE, ARG
  FOR LOCAL, 0, 移動ルートMAX
    CFLAG:ARG:移動ルート:LOCAL = 0
  NEXT
```

#### C# Migration

```csharp
using Era.Core.Common;

public static class RouteFunctions
{
    public static void CLEAR_ROUTE(int characterId, int[][] movementRoute)
    {
        for (int i = 0; i < Constants.移動ルートMAX; i++)
        {
            movementRoute[characterId][i] = 0;
        }
    }
}
```

### Reference Checklist for F365-F371

When migrating ERB functions that use DIM.ERH constants:

1. ✅ Add `using Era.Core.Common;` at top of file
2. ✅ Prefix all constant references with `Constants.`
3. ✅ Verify constant exists in Constants.cs (427 constants available)
4. ✅ For bitfield operations, use `&`, `|`, `^` operators (1p notation already converted to `1 << N`)
5. ✅ For SAVEDATA/CHARADATA variables, check VariableDefinitions.cs for metadata
6. ✅ Write MSTest tests that verify constant values are correct
7. ✅ Use `Constants.移動ルートMAX` for array size loops, not hardcoded `13`

### Known Limitations

1. **#DEFINE Aliases**: `天候値`, `暦法月`, `暦法日`, `告白成功済` require runtime evaluation (TIME:1, DAY:1, DAY:2, 既成事実&1p0). These are documented in VariableDefinitions.cs but not available as const. Migration must use array access syntax.

2. **SAVEDATA/CHARADATA Access**: These define variable names and sizes, but runtime allocation is handled by the engine. Migrated code receives these as parameters (e.g., `int[,] cflag`, `int[] flag`).

3. **CSV Index Resolution**: Constants reference variable *values* (e.g., `人物_美鈴 = 1`), but CSV column indices (e.g., `CFLAG:TARGET:居場所`) must be resolved separately from CSV files.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | implementer | Created as Phase 3 CRITICAL foundation | PROPOSED |
| 2026-01-06 08:41 | task3 | implementer | Created ConstantsTests.cs (RED state) | SUCCESS |
| 2026-01-06 08:47 | task2a | implementer | Created Era.Core/Common/Constants.cs with all 572 #DIM CONST values from DIM.ERH, handled bitfield notation (1pN→1<<N), fixed middle dot chars (・→_), tests pass (18/18) | SUCCESS |
| 2026-01-06 10:21 | task2b | implementer | Completed Era.Core/Common/VariableDefinitions.cs with SAVEDATA (親別出産数 2D array + 24 訪問者_* visitor appearance vars), CHARADATA (移動ルート array), #DEFINE aliases (天候値/暦法月/暦法日/告白成功済 bitwise), plain #DIM/DIMS. Preserved ERB semantics with C# attributes/comments. Build succeeds, Era.Core.Tests pass (19/19) | SUCCESS |
| 2026-01-06 08:54 | task1,task4 | implementer | Documented DIM.ERH Migration Analysis (458 vars categorized: 427 CONST, 24 SAVEDATA, 1 CHARADATA, 4 DEFINE, 1 DIM, 1 DIMS) + Migration Pattern with ERB→C# examples for F365-F371 reference. All 10 ACs complete. Build+tests pass (125/125) | SUCCESS |
