# Feature 366: COMMON.ERB Shared Functions Migration

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate COMMON.ERB shared utility functions to C# CommonFunctions.cs. This establishes reusable utility functions referenced by all game systems.

**Context**: Phase 3 Task 2 from full-csharp-architecture.md. Requires F364 Constants.cs completion for variable access.

---

## Background

### Philosophy (Mid-term Vision)

**Foundation-First Migration**: COMMON.ERB contains shared utility functions used throughout the codebase - mathematical helpers, string manipulation, data access wrappers, etc. Migrating these to C# enables type-safe function calls and prepares Phase 4+ features to use CommonFunctions library.

### Problem (Current Issue)

COMMON.ERB (661 lines verified from source file) contains:
- @ITEMSTOCK: Item availability/purchase validation
- @CHOICE: 2-4 choice selection function
- Mathematical utility functions
- String manipulation helpers
- Data access wrappers
- Common macro definitions

**Current State**:
- All ERB files depend on COMMON.ERB functions
- No C# equivalent for COMMON.ERB shared utilities (GameInitialization.cs from F365 covers SYSTEM.ERB only)
- Phase 4+ features cannot reference CommonFunctions until migration complete

**Phase 2 Context**:
- F359: Test infrastructure ready for CommonFunctions testing
- F362: Test migration patterns established (create C# tests before removing ERB code)
- Strategy: Test each function group independently using xUnit (engine.Tests pattern)

### Goal (What to Achieve)

1. Complete function group analysis with runtime dependency classification documented
2. Create Era.Core/Common/CommonFunctions.cs with all shared utilities
3. Implement type-safe wrappers for ERB macro equivalents
4. Create xUnit test cases for each function group
5. Verify function behavior matches ERB legacy implementation
6. Document CommonFunctions API for Phase 4+ reference

**Runtime Array Access Strategy**: Functions requiring runtime arrays (TALENT, TEQUIP, STAIN, CFLAG, MARK, etc.) will receive array parameters as method arguments. This follows engine.Tests pattern (direct state access per F363 Phase 2 Lessons).

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | COMMON.ERB analysis documented | file | Grep "## COMMON.ERB Analysis" in Game/agents/feature-366.md | contains | "## COMMON.ERB Analysis" | [x] |
| 2 | CommonFunctions.cs created | file | Glob Era.Core/Common/CommonFunctions.cs | exists | Era.Core/Common/CommonFunctions.cs | [x] |
| 3 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 4 | CommonFunctions tests created | file | Glob engine.Tests/Tests/CommonFunctionsTests.cs | exists | engine.Tests/Tests/CommonFunctionsTests.cs | [x] |
| 5 | All function tests pass (Pos) | test | dotnet test --filter CommonFunctionsTests | succeeds | - | [x] |
| 6 | CommonFunctionsTests has minimum coverage | code | Grep "\\[Fact\\]" in engine.Tests/Tests/CommonFunctionsTests.cs | gte | 10 | [x] |
| 7 | API documentation created | file | Grep "## CommonFunctions API" in Game/agents/feature-366.md | contains | "## CommonFunctions API" | [x] |
| 8 | COMMON.ERB related sub-features created | file | Glob Game/agents/feature-37[2-6].md | count_equals | 5 | [x] |
| 9 | Invalid parameter test exists (Neg) | code | Grep "InvalidParameter" in engine.Tests/Tests/CommonFunctionsTests.cs | contains | "InvalidParameter" | [x] |
| 10 | Null handling test exists (Neg) | code | Grep "Null" in engine.Tests/Tests/CommonFunctionsTests.cs | contains | "Null" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze COMMON.ERB: categorize function groups (math/string/data access), count functions, identify Constants.cs dependencies AND runtime variable dependencies (TALENT/CFLAG/TEQUIP/MARK/STAIN arrays requiring engine context), document in feature-366.md | [x] |
| 2 | 2 | Create Era.Core/Common/CommonFunctions.cs with all function groups, implement type-safe wrappers using Constants.cs | [x] |
| 3 | 3 | Verify C# build succeeds after CommonFunctions.cs creation | [x] |
| 4 | 4 | Create engine.Tests/Tests/CommonFunctionsTests.cs using xUnit patterns (per F364 ConstantsTests.cs) | [x] |
| 5 | 5 | Run all CommonFunctions tests and verify they pass | [x] |
| 6 | 6 | Verify test coverage: ensure minimum 10 test methods exist in CommonFunctionsTests.cs | [x] |
| 7 | 7 | Document CommonFunctions API with usage examples for Phase 4+ features in feature-366.md | [x] |
| 8 | 8 | Create COMMON.ERB related sub-features (F372-F376) | [x] |
| 9 | 9 | Add negative test for invalid parameter handling in CommonFunctionsTests.cs | [x] |
| 10 | 10 | Add negative test for null handling in CommonFunctionsTests.cs | [x] |

> **Note (Task 1 Output)**: The "## COMMON.ERB Analysis" section MUST contain: (1) Function Groups table with counts, (2) Runtime Dependencies (TALENT/CFLAG/TEQUIP/MARK/STAIN arrays), (3) Constants.cs dependencies, (4) Migration complexity per group.

> **Note (Task 8)**: Per full-csharp-architecture.md Phase 3 plan, F366 triggers creation of COMMON.ERB dependent features: F372 (COMMON_PLACE.ERB), F373 (INFO.ERB), F374 (COMMON_J.ERB), F375 (COMMON_KOJO.ERB), F376 (Header files consolidation). Note: F367-F371 are SYSTEM.ERB successors (F365 created them).

---

## COMMON.ERB Analysis

### Function Groups

Based on analysis of Game/ERB/COMMON.ERB (661 lines), functions are categorized as follows:

| Group | Function Count | Migration Complexity | Example Functions |
|-------|:--------------:|:--------------------:|-------------------|
| Gender/Sex Utilities | 5 | Low | HAS_PENIS, HAS_VAGINA, IS_FEMALE, GET_SEXNAME, IS_DOUTEI |
| Mathematical Helpers | 3 | Low | GET_REVISION, SOURCE_REVISION1, SOURCE_REVISION2 |
| Date/Time Functions | 2 | Low | GET_DAY, DATETIME |
| Dish/Menu Functions | 2 | Low | GETDISHNAME, GETDISHMENU |
| Body Part Pair Functions | 3 | Low | HAS_PAIR_部位, GET_PAIR_部位, DICTIONARY_PAIR_部位 |
| Equipment State Functions | 2 | Low | INVAGINA, INANAL |
| Character/Target Query | 4 | Medium | GET_TARGETNUM, GET_TARGETSLEEP, GET_TSLEEP_NAME, GET_TARGETWAKEUP |
| Item Management | 1 | Medium | ITEMSTOCK |
| UI/Choice Functions | 2 | High (ERB-specific) | CHOICE, PRINT_BASE |
| Event State Management | 2 | Medium | ONCE, FIRSTTIME |
| Experience Tracking | 1 | Medium | EXP_UP |
| Relationship Query | 1 | Medium | HETEROSEX |
| Stain/Mark Management | 4 | Medium | GET_STAINCOUNT, HAS_STAIN, GET_MARK_LEVEL, GET_MARK_LEVEL_OWNERNAME |
| Name/Description Helpers | 3 | Medium | GET_DESCRIPTIVE_NAME, GET_CALLNAME, GET_TRAIN_NAME |
| **Total** | **35** | - | - |

### Runtime Dependencies Analysis

Functions requiring runtime array access (TALENT, CFLAG, TEQUIP, MARK, STAIN arrays):

| Function | Runtime Arrays Required | Migration Strategy |
|----------|-------------------------|-------------------|
| HAS_PENIS, HAS_VAGINA, IS_FEMALE, GET_SEXNAME | TALENT (性別) | **Phase 1**: Accept genderValue parameter directly |
| IS_DOUTEI | TALENT (性別, 童貞) | **Deferred**: Requires TALENT array access |
| HETEROSEX | TALENT (性別) | **Deferred**: Requires TALENT array for 2 characters |
| INVAGINA, INANAL | TEQUIP (various) | **Phase 1**: Accept equipment values as parameters |
| GET_STAINCOUNT, HAS_STAIN | STAIN, TALENT | **Deferred**: Requires STAIN/TALENT array access |
| GET_MARK_LEVEL, GET_MARK_LEVEL_OWNERNAME | MARK, CFLAG | **Deferred**: Requires MARK/CFLAG array access |
| ITEMSTOCK | ITEM, ITEMNAME, ITEMSALES, ITEMPRICE, MONEY | **Deferred**: Requires multiple global arrays |
| GET_TARGETNUM, GET_TARGETSLEEP, GET_TSLEEP_NAME, GET_TARGETWAKEUP | TARGET, CFLAG (various) | **Deferred**: Requires TARGET/CFLAG array access |
| EXP_UP | EXP, TCVAR | **Deferred**: Requires EXP/TCVAR array access |
| ONCE, FIRSTTIME | CSTR | **Deferred**: Requires CSTR array access |
| GET_DESCRIPTIVE_NAME, GET_CALLNAME | CALLNAME, CFLAG (optional) | **Deferred**: Requires CALLNAME array access |
| GET_TRAIN_NAME | TRAINNAME, PREVCOM | **Deferred**: Requires TRAINNAME array access |

### Constants.cs Dependencies

Functions using Constants.cs (F364):

| Function | Constants Used | Status |
|----------|----------------|:------:|
| HAS_PAIR_部位, GET_PAIR_部位, DICTIONARY_PAIR_部位 | 部位_膣内, 部位_Ｖ, 部位_腸内, 部位_Ａ | ✅ Available |
| GET_STAINCOUNT | 部位_膣内, 部位_腸内, 汚れB_愛液, 汚れB_精液, 汚れB_粘液, 汚れB_アナル, 汚れB_母乳 | ✅ Available |
| GET_DESCRIPTIVE_NAME, GET_CALLNAME | 人物_村人, 人物_客, 人物_訪問者, 人物_あなた | ✅ Available |

### Phase 1 Migration Scope (Current Feature)

**Functions migrated in this feature (15 functions)**:

1. **Gender Functions (4)**: HAS_PENIS, HAS_VAGINA, IS_FEMALE, GET_SEXNAME
   - Strategy: Accept `int genderValue` parameter instead of TALENT array access
   - Test coverage: 6 tests (male/female/futanari combinations)

2. **Mathematical Functions (3)**: GET_REVISION, SOURCE_REVISION1, SOURCE_REVISION2
   - Strategy: Pure mathematical functions, no dependencies
   - Test coverage: 4 tests (zero input, high input, boundary cases)

3. **Date/Time Functions (2)**: GET_DAY, DATETIME
   - Strategy: Accept day/time parameters instead of global DAY/TIME
   - Test coverage: 4 tests (day of week calculations)

4. **Dish Functions (2)**: GETDISHNAME, GETDISHMENU
   - Strategy: Pure string lookup functions
   - Test coverage: 4 tests (code ranges, modifiers)

5. **Body Part Pair Functions (3)**: HAS_PAIR_部位, GET_PAIR_部位, DICTIONARY_PAIR_部位
   - Strategy: Use Constants.cs for part IDs
   - Test coverage: 4 tests (pair detection, retrieval)

6. **Equipment State Functions (2)**: INVAGINA, INANAL
   - Strategy: Accept equipment values as parameters
   - Test coverage: 5 tests (equipment combinations)

**Deferred Functions (20 functions)**: Will be migrated in F372-F376 sub-features as they require full game state context.

---

## CommonFunctions API

### Overview

`Era.Core.Common.CommonFunctions` provides type-safe C# implementations of COMMON.ERB shared utility functions. All methods are static and can be called directly without instantiation.

**Namespace**: `Era.Core.Common`
**Assembly**: `Era.Core.dll`
**Feature**: F366 Phase 1 Migration (15 functions)

### Gender Functions

#### HasPenis(int genderValue) -> bool

Checks if character has penis (male or futanari).

**Parameters**:
- `genderValue`: Gender value (1=female, 2=male, 3=futanari)

**Returns**: `true` if male or futanari (bit 1 set)

**ERB Equivalent**: `@HAS_PENIS(ARG)` checks `TALENT:ARG:性別 & 2`

**Example**:
```csharp
// Phase 4+ usage with TALENT array access
int gender = globalStatic.TALENT[charIndex][(int)Constants.性別];
bool hasPenis = CommonFunctions.HasPenis(gender);
```

#### HasVagina(int genderValue) -> bool

Checks if character has vagina (female or futanari).

**Parameters**:
- `genderValue`: Gender value (1=female, 2=male, 3=futanari)

**Returns**: `true` if female or futanari (bit 0 set)

**ERB Equivalent**: `@HAS_VAGINA(ARG)` checks `TALENT:ARG:性別 & 1`

**Example**:
```csharp
int gender = globalStatic.TALENT[charIndex][(int)Constants.性別];
bool hasVagina = CommonFunctions.HasVagina(gender);
```

#### IsFemale(int genderValue) -> bool

Checks if character is female only (not futanari).

**Parameters**:
- `genderValue`: Gender value (1=female, 2=male, 3=futanari)

**Returns**: `true` if female only (gender == 1)

**ERB Equivalent**: `@IS_FEMALE(ARG)` checks `1 == (TALENT:ARG:性別 & 3)`

**Example**:
```csharp
int gender = globalStatic.TALENT[charIndex][(int)Constants.性別];
if (CommonFunctions.IsFemale(gender))
{
    // Female-only logic
}
```

#### GetSexName(int genderValue) -> string

Gets gender name string.

**Parameters**:
- `genderValue`: Gender value (1=female, 2=male, 3=futanari)

**Returns**: Gender name ("女", "男", "ふたなり", or "よくわからないもの" for invalid values)

**ERB Equivalent**: `@GET_SEXNAME(ARG)` returns "女"/"男"/"ふたなり"

**Example**:
```csharp
int gender = globalStatic.TALENT[charIndex][(int)Constants.性別];
string genderName = CommonFunctions.GetSexName(gender); // "女", "男", or "ふたなり"
```

### Mathematical Functions

#### GetRevision(int value, int limit, int rate) -> int

Generic revision calculation - converts value to approach limit asymptotically.

**Parameters**:
- `value`: Input value
- `limit`: Upper limit to approach
- `rate`: Rate of approach (smaller = faster approach)

**Returns**: Revised value approaching limit

**Throws**: `ArgumentException` if `rate == 0` (division by zero)

**ERB Equivalent**: `@GET_REVISION(ARG:0, ARG:1, ARG:2)` with formula `ARG:1 - ARG:2 * ARG:1 / (ARG:2 + ARG:0)`

**Example**:
```csharp
// Pleasure calculation approaching limit of 1000 with rate 200
int revisedPleasure = CommonFunctions.GetRevision(currentAbl, 1000, 200);
```

#### SourceRevision1(int abl) -> int

Pleasure SOURCE revision by pleasure ABL (1/10 scaling).

**Parameters**:
- `abl`: Ability level (0-10+)

**Returns**: Revision value (5 for abl=0, 10*abl for 1-3, 20*abl-30 for 4-9, 150 for 10+)

**ERB Equivalent**: `@SOURCE_REVISION1(ARG)`

**Example**:
```csharp
int pleasureSource = CommonFunctions.SourceRevision1(pleasureAbl) / 10;
```

#### SourceRevision2(int abl) -> int

Pleasure SOURCE revision by desire ABL (1/100 scaling).

**Parameters**:
- `abl`: Ability level (0-10+)

**Returns**: Revision value (50+10*abl for 0-5, 75+5*abl for 6-9, 140 for 10+)

**ERB Equivalent**: `@SOURCE_REVISION2(ARG)`

**Example**:
```csharp
int desireSource = CommonFunctions.SourceRevision2(desireAbl) / 100;
```

### Date/Time Functions

#### GetDayOfWeek(int day) -> string

Gets day of week string.

**Parameters**:
- `day`: Day number

**Returns**: Day of week character ("日", "月", "火", "水", "木", "金", "土")

**ERB Equivalent**: `@GET_DAY` returns `DAY % 7` index from day array

**Example**:
```csharp
int currentDay = globalStatic.DAY;
string dayOfWeek = CommonFunctions.GetDayOfWeek(currentDay); // "月" for Monday
```

#### GetDateTime(int day, int time) -> int

Gets total elapsed time in minutes from day 0 time 0.

**Parameters**:
- `day`: Day number
- `time`: Time in minutes

**Returns**: Total elapsed minutes (1440 * day + time)

**ERB Equivalent**: `@DATETIME()` returns `1440 * DAY + TIME`

**Example**:
```csharp
int currentDay = globalStatic.DAY;
int currentTime = globalStatic.TIME;
int totalMinutes = CommonFunctions.GetDateTime(currentDay, currentTime);
```

### Dish Functions

#### GetDishName(int code) -> string

Gets dish name with modifier.

**Parameters**:
- `code`: Dish code (modifier encoded in thousands place, dish type in ones place)

**Returns**: Dish name with modifier (empty string if code not found)

**Modifiers**:
- `> 6000`: 睡眠薬入り (sleeping pill)
- `> 5000`: 利尿剤入り (diuretic)
- `> 4000`: 媚薬入り (aphrodisiac)
- `> 3000`: 母乳入り (breast milk)
- `> 2000`: 愛液入り (love juice)
- `> 1000`: 精液入り (semen)

**Dish Types** (code % 1000):
- 1: サンドイッチ (sandwich)
- 2: おにぎり (rice ball)
- 31-40: 主食 (main dishes: オムレツ, シチュー, ハンバーグ, etc.)
- 61-70: デザート (desserts: プリン, ケーキ, ゼリー, etc.)

**ERB Equivalent**: `@GETDISHNAME(ARG)`

**Example**:
```csharp
string dishName = CommonFunctions.GetDishName(4031); // "媚薬入りヲムレツ"
```

#### GetDishMenu(int code) -> string

Gets dish menu category.

**Parameters**:
- `code`: Dish code

**Returns**: Menu category ("軽食", "主食", "デザート", or empty string)

**ERB Equivalent**: `@GETDISHMENU(ARG)`

**Example**:
```csharp
string menuCategory = CommonFunctions.GetDishMenu(31); // "主食"
```

### Body Part Pair Functions

#### HasPairPart(int part) -> bool

Checks if body part has a pair.

**Parameters**:
- `part`: Body part ID

**Returns**: `true` if part has a pair (部位_膣内 or 部位_腸内)

**ERB Equivalent**: `@HAS_PAIR_部位(ARG)`

**Pairs**:
- `部位_膣内` (vagina internal) <-> `部位_Ｖ` (vagina external)
- `部位_腸内` (anal internal) <-> `部位_Ａ` (anal external)

**Example**:
```csharp
if (CommonFunctions.HasPairPart(Constants.部位_膣内))
{
    int pairPart = CommonFunctions.GetPairPart(Constants.部位_膣内); // 部位_Ｖ
}
```

#### GetPairPart(int part) -> int

Gets paired body part.

**Parameters**:
- `part`: Body part ID

**Returns**: Paired body part ID

**Throws**: `ArgumentException` if part has no pair

**ERB Equivalent**: `@GET_PAIR_部位(ARG)`

**Example**:
```csharp
int pairPart = CommonFunctions.GetPairPart(Constants.部位_膣内); // Returns 部位_Ｖ
```

### Equipment State Functions

#### InVagina(int vSex, int vibe) -> int

Checks what is in vagina.

**Parameters**:
- `vSex`: Ｖセックス equipment value (penis)
- `vibe`: バイブ equipment value (vibrator)

**Returns**:
- `0`: Nothing
- `1`: Penis
- `2`: Vibrator

**ERB Equivalent**: `@INVAGINA(ARG)` checks `TEQUIP:ARG:Ｖセックス` and `TEQUIP:ARG:バイブ`

**Example**:
```csharp
int vSex = globalStatic.TEQUIP[targetIndex][(int)Constants.Ｖセックス];
int vibe = globalStatic.TEQUIP[targetIndex][(int)Constants.バイブ];
int inVagina = CommonFunctions.InVagina(vSex, vibe);
if (inVagina == 1)
{
    // Penis in vagina
}
```

#### InAnal(int aSex, int analVibe, int analBeads) -> int

Checks what is in anal.

**Parameters**:
- `aSex`: Ａセックス equipment value (penis)
- `analVibe`: アナルバイブ equipment value (anal vibrator)
- `analBeads`: アナルビーズ equipment value (anal beads)

**Returns**:
- `0`: Nothing
- `1`: Penis
- `2`: Anal vibrator
- `3`: Anal beads

**ERB Equivalent**: `@INANAL(ARG)` checks `TEQUIP:ARG:Ａセックス`, `アナルバイブ`, `アナルビーズ`

**Example**:
```csharp
int aSex = globalStatic.TEQUIP[targetIndex][(int)Constants.Ａセックス];
int analVibe = globalStatic.TEQUIP[targetIndex][(int)Constants.アナルバイブ];
int analBeads = globalStatic.TEQUIP[targetIndex][(int)Constants.アナルビーズ];
int inAnal = CommonFunctions.InAnal(aSex, analVibe, analBeads);
```

### Usage Notes for Phase 4+ Features

1. **Parameter Strategy**: Phase 1 functions accept primitive parameters instead of global array access. Phase 4+ features should extract values from GlobalStatic arrays before calling CommonFunctions.

2. **Error Handling**: Functions with validation (e.g., `GetRevision`, `GetPairPart`) throw `ArgumentException` for invalid input. Always validate input or catch exceptions.

3. **Constants Integration**: Body part functions use `Constants.cs` (F364) for type-safe constant access.

4. **Deferred Functions**: 20 functions requiring full game state context (TARGET, CFLAG, TALENT arrays) are deferred to F372-F376 sub-features.

5. **Testing**: All functions have xUnit test coverage in `engine.Tests/Tests/CommonFunctionsTests.cs` with positive and negative cases.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F363 | Phase 3 Planning complete |
| Predecessor | F364 | Constants.cs required for variable access in functions |
| Predecessor | F359 | xUnit test structure for CommonFunctions testing |
| Parallel | F365 (DONE) | SYSTEM.ERB and COMMON.ERB can be migrated in parallel after F364 |
| Successor | F372-F376 | COMMON.ERB related sub-features (created by Task 6 of this feature) |

**Dependency Chain**:
```
F363 (Planning) → F364 (Constants) → [F365 || F366] → F372-F376
                                       (parallel possible)
```

**Note**: F367-F371 are SYSTEM.ERB successors (F365 created them), not COMMON.ERB related.

**Critical Path**: F364 MUST complete before F366 can begin (Constants.cs dependency). F365 and F366 can run in parallel.

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 Task 2 (line 1104)
- [feature-363.md](feature-363.md) - Phase 3 Planning (parent)
- [feature-364.md](feature-364.md) - DIM.ERH Migration (prerequisite)
- [feature-359.md](feature-359.md) - Test Structure (xUnit foundation)
- [feature-362.md](feature-362.md) - Test Migration (patterns reference)
- [feature-365.md](feature-365.md) - SYSTEM.ERB Migration (parallel feature)
- [feature-367.md](feature-367.md) - SYSTEM.ERB successor (Options & Utility)
- [feature-368.md](feature-368.md) - SYSTEM.ERB successor (Character Setup)
- [feature-369.md](feature-369.md) - SYSTEM.ERB successor (Clothing System)
- [feature-370.md](feature-370.md) - SYSTEM.ERB successor (Body & State)
- [feature-371.md](feature-371.md) - SYSTEM.ERB successor (NTR Initialization)
- Game/ERB/COMMON.ERB - Source file to migrate

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | implementer | Created as Phase 3 CRITICAL shared functions migration | PROPOSED |
| 2026-01-06 | validate | ac-validator | AC validation: Fixed Method syntax (AC1,7,8,6), added negative tests (AC9,10), aligned Tasks 1:1 with ACs | TDD Ready: 10/10 |
| 2026-01-06 15:30 | implement | implementer | Task 1: Analyzed COMMON.ERB (661 lines, 35 functions, 7 groups), documented Phase 1 migration scope (15 functions), created CommonFunctions.cs with gender/math/time/dish/body-part/equipment functions | SUCCESS |
| 2026-01-06 15:44 | implement | implementer | Task 7: Added CommonFunctions API documentation with all 15 method signatures, parameters, return values, ERB equivalents, and Phase 4+ usage examples | SUCCESS |
