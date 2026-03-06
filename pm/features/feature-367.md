# Feature 367: Options & Utility Functions Migration

## Status: [DONE]

## Type: engine

## Created: 2026-01-06

---

## Summary

Migrate SYSTEM.ERB external dependencies: OPTION_2.ERB, ショートカットモード.ERB, VERSION_UP.ERB to C#. These are small utility functions called during game initialization.

**Context**: Phase 3 Task 2 from full-csharp-architecture.md. Supports F365 (SYSTEM.ERB) migration.

**Note**: Combined ~197 lines total (including comments/blanks, ~100 executable lines) - small utility functions that can be migrated together.

---

## Background

### Philosophy (Mid-term Vision)

**Complete Initialization Stack**: F365 migrates SYSTEM.ERB handlers, but they depend on external functions. F367 migrates the smaller utility functions to enable full C# initialization.

### Problem (Current Issue)

SYSTEM.ERB calls these external functions:
- DEFAULT_OPTION (OPTION_2.ERB, function at line 2) - 55 lines total
- SHORT_CUT_MODE (ショートカットモード.ERB, function at line 4) - 129 lines total
- VERSION_UP (VERSION_UP.ERB, function at line 2) - 13 lines total

**Design Decisions**:
1. **State Management**: GameOptions methods remain side-effect based (modify global arrays via engine). Tests verify behavior via integration testing with mock game state.
2. **SHORT_CUT_MODE Interactivity**: SHORT_CUT_MODE contains INPUT prompts. For C# migration, extract state modification logic only; keep UI interaction in ERB layer. ERB handles INPUT, then calls GameInitialization.ShortCutMode(int mode). Mode values: 0=shortcut, -1=NTR, 1/2=normal (1 and 2 are equivalent skip modes). Method signature includes accessors for all modified arrays: CFLAG, ABL, TALENT, TA, RELATION, EXP, plus Random. Exact signature determined in Task 1 analysis.
3. **VERSION_UP Dependency**: VERSION_UP calls HAS_PENIS(), defined in COMMON.ERB (F366). HAS_PENIS checks `(TALENT:charId:性別 & 2) != 0`. If F366 incomplete, VERSION_UP uses local stub `HasPenisStub(charId, talentAccessor)` with same logic. VERSION_UP also requires FLAG:1000, MAXBASE, BASE array accessors. Exact signature determined in Task 1 analysis.
4. **Testing Strategy for CHARANUM Loops**: SHORT_CUT_MODE and VERSION_UP iterate over CHARANUM characters. Testing strategy: (a) Parameterized character count for unit tests, (b) Integration tests for full character iteration logic. Note: VERSION_UP starts at index 0 (includes MASTER), SHORT_CUT_MODE starts at index 1 (excludes MASTER).
5. **ERB Integration Scope**: F367 creates C# GameOptions methods callable from engine. F365 stubs (ShortCutMode(), etc.) remain parameterless. F367 implements parameterized methods in GameOptions.cs. ERB layer updates (ショートカットモード.ERB calling C#) are out of scope; will be addressed in Phase 10 (Era.Core Engine Integration).
6. **Array Access Strategy**: Functions requiring runtime arrays receive array accessor delegates as method parameters. Character arrays (2D): `Func<int, int, T>`/`Action<int, int, T>` for CFLAG, ABL, TALENT, RELATION, EXP, FLAG, MAXBASE, BASE. TA is 3D: `Func<int, int, int, T>`/`Action<int, int, int, T>`. Global 1D arrays: `Action<int, T>` for PALAMLV, EXPLV, ITEMSALES. This enables unit testing with mock data.
7. **TA Array Handling**: ShortCutMode modifies inter-character relationships via TA 3D array (TA:charA:charB:index).
8. **RAND Handling for Testability**: SHORT_CUT_MODE uses RAND:26 for randomization. C# method receives `Random` instance as parameter for deterministic unit testing (inject seeded Random).

Without C# equivalents, F365 cannot fully migrate @EVENTFIRST and @EVENTLOAD handlers.

### Goal (What to Achieve)

1. Analyze target functions and their dependencies
2. Create Era.Core/Common/GameOptions.cs with DefaultOption(), ShortCutMode(), VersionUp() methods
3. Create xUnit test cases (GameOptionsTests.cs)
4. Update GameInitialization.cs stub methods to delegate to GameOptions methods

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1a | Function analysis heading exists | file | Grep feature-367.md | contains | "## Function Analysis" | [x] |
| 1b | Variable dependencies documented | file | Grep feature-367.md | contains | "Variable Dependencies" | [x] |
| 2 | GameOptions.cs created | file | Glob | exists | Era.Core/Common/GameOptions.cs | [x] |
| 3 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 4 | Unit tests created | file | Glob | exists | Era.Core.Tests/GameOptionsTests.cs | [x] |
| 5 | Unit tests pass | test | dotnet test | succeeds | - | [x] |
| 6 | GameInitialization.cs stubs updated | file | Grep | contains | "GameOptions." in Era.Core/Common/GameInitialization.cs | [x] |
| 7 | Null accessor parameter rejected | test | dotnet test | succeeds | ArgumentNullException test case in GameOptionsTests.cs | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1a,1b | Analyze DEFAULT_OPTION, SHORT_CUT_MODE, VERSION_UP: output must include (1) Variable Dependencies table, (2) CHARANUM iteration strategy, (3) RAND handling approach, (4) Exact method signatures | [x] |
| 2 | 2 | Create GameOptions.cs with methods (signatures per Task 1 analysis) | [x] |
| 3 | 3 | Build verification | [x] |
| 4 | 4 | Create GameOptionsTests.cs unit test structure | [x] |
| 5 | 5,7 | Implement and verify unit tests pass (includes negative test cases) | [x] |
| 6 | 6 | Update GameInitialization.cs stubs to call GameOptions methods | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F364 | Constants.cs required for variable access |
| Predecessor | F365 | Stub interfaces defined in GameInitialization.cs (F367 implements DefaultOption, ShortCutMode, VersionUp stubs) |
| Soft Predecessor | F366 | HAS_PENIS() for VERSION_UP; can proceed with HasPenisStub() if F366 incomplete |

**Dependency Chain**:
```
F364 (Constants) → F365 (SYSTEM.ERB) + F366 (COMMON.ERB) → F367 (Options) → Full C# initialization
```

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 (lines 886-908)
- [feature-365.md](feature-365.md) - SYSTEM.ERB Migration (parent)
- [feature-364.md](feature-364.md) - DIM.ERH Migration (prerequisite)
- [feature-366.md](feature-366.md) - COMMON.ERB Migration (HAS_PENIS dependency)
- Game/ERB/OPTION_2.ERB - Source file (55 lines)
- Game/ERB/ショートカットモード.ERB - Source file (129 lines)
- Game/ERB/VERSION_UP.ERB - Source file (13 lines)

---

## Function Analysis

### Variable Dependencies

| Variable | Type | Dimensions | Usage | Source |
|----------|------|------------|-------|--------|
| **FLAG:難易度** | 1D array | 1 | Difficulty setting | OPTION_2 line 4 |
| **FLAG:情景テキスト設定** | 1D array | 1 | Scene text flag | OPTION_2 line 7 |
| **FLAG:口上テキスト設定** | 1D array | 1 | Dialogue text flag | OPTION_2 line 8 |
| **FLAG:1003, 1004** | 1D array | 2 indices | Character list display flags | OPTION_2 lines 11-12 |
| **FLAG:1000** | 1D array | 1 | Auto-purchase flag; VERSION_UP line 3 check | OPTION_2 line 21; VERSION_UP line 3 |
| **FLAG:1001** | 1D array | 1 | Random/custom character mode | OPTION_2 line 24 |
| **ITEMSALES:50...100** | 1D array | 51 values | Item sales table (50=10, 70-100=-2) | OPTION_2 lines 15-18 |
| **PALAMLV:0...15** | 1D array | 16 values | Parameter level thresholds | OPTION_2 lines 26-41 |
| **EXPLV:0...12+** | 1D array | 13+ values | Experience level thresholds | OPTION_2 lines 43-55 |
| **CFLAG:charId:2** | 2D array | 1200 (好感度) | Character affection | SHORT_CUT_MODE lines 25, 79 |
| **CFLAG:charId:21** | 2D array | 1000 (屈服度) | Character submission level | SHORT_CUT_MODE line 79 |
| **ABL:charId:親密** | 2D array | [3,5] or [4,6] | Character intimacy | SHORT_CUT_MODE lines 29-32, 83-86 |
| **ABL:charId:従順** | 2D array | [3,5] or [4,6] | Character obedience | SHORT_CUT_MODE lines 35-38, 89-92 |
| **ABL:charId:欲望** | 2D array | [2,4] or [3,5] | Character desire | SHORT_CUT_MODE lines 41-44, 95-98 |
| **ABL:charId:技巧** | 2D array | [2,4] or [3,5] | Character technique | SHORT_CUT_MODE lines 47-50, 101-104 |
| **EXP:charId:奉仕快楽経験** | 2D array | +=15 | Character service-pleasure experience | SHORT_CUT_MODE line 54 |
| **RELATION:charA:charB** | 2D array | Affinity modifier | Character-to-character affinity base | SHORT_CUT_MODE lines 58, 109 |
| **TA:charA:charB:TA_好感度** | 3D array | index=5 | Inter-character affection (cap 500) | SHORT_CUT_MODE lines 59, 63, 66, 110, 112 |
| **RAND:26** | Random | [0,26) | Random offset for affection | SHORT_CUT_MODE lines 59, 63, 66, 110, 112 |
| **TALENT:charId:6** | 2D array | 0/1 | NTR talent flag | SHORT_CUT_MODE line 76 |
| **TALENT:charId:性別** | 2D array | & 2 mask | Gender (bit 1: penis) | VERSION_UP line 6 (HAS_PENIS) |
| **MAXBASE:charId:精力** | 2D array | 1000 | Max stamina | VERSION_UP lines 7-8 |
| **BASE:charId:精力** | 2D array | set to MAXBASE | Current stamina | VERSION_UP line 8 |

### CHARANUM Iteration Strategy

**SHORT_CUT_MODE nested loops** (excludes MASTER at index 0):
```
FOR LOCAL,1,CHARANUM          # Inner loop over CHARANUM-1
  FOR LOCAL:1,1,CHARANUM     # Nested loop: each character's affinity with others
    TA:charA:charB:5 = calc  # Relationship matrix operation
  NEXT
NEXT
```

**VERSION_UP loop** (includes MASTER at index 0):
```
FOR LOCAL,0,CHARANUM         # Loop starting at 0
  IF HAS_PENIS(LOCAL)
    MAXBASE:LOCAL:5 = 1000
    BASE:LOCAL:5 = MAXBASE:LOCAL:5
  ENDIF
NEXT
```

### RAND Handling Approach

`RAND:26` in ERB generates random [0,26). C# method receives `Random` instance for deterministic testing:
- Unit tests: `new Random(seed)` for reproducibility
- Production: standard `Random` instance

### Method Signatures

**DEFAULT_OPTION**:
```csharp
public static void DefaultOption(
    Action<int, int> setFlag,
    Action<int, int> setPalamLv,
    Action<int, int> setExpLv,
    Action<int, int> setItemSales)
```

**SHORT_CUT_MODE**:
```csharp
public static void ShortCutMode(
    int mode,
    int charanum,
    Func<int, int, int> getCflag,
    Action<int, int, int> setCflag,
    Func<int, int, int> getAbl,
    Action<int, int, int> setAbl,
    Action<int, int, int> setExp,
    Action<int, int, int> setTalent,
    Func<int, int, int> getRelation,
    Func<int, int, int, int> getTa,
    Action<int, int, int, int> setTa,
    Random random)
```

**VERSION_UP**:
```csharp
public static void VersionUp(
    Func<int, int> getFlag,
    Action<int, int> setFlag,
    int charanum,
    Func<int, int, int> getTalent,
    Action<int, int, int> setMaxBase,
    Action<int, int, int> setBase)
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | orchestrator | Created as F365 external dependency | PROPOSED |
| 2026-01-06 | init | initializer | Status → [WIP] | READY |
| 2026-01-06 | Phase 2 | explorer | Function analysis, variable dependencies | READY |
| 2026-01-06 14:02 | Task 4 | implementer | Create GameOptionsTests.cs (TDD RED state) | SUCCESS |
| 2026-01-06 14:30 | Task 2 | implementer | Create GameOptions.cs (DefaultOption, ShortCutMode, VersionUp) | SUCCESS |
| 2026-01-06 14:09 | Task 6 | implementer | Update GameInitialization.cs stubs with GameOptions references | SUCCESS |
| 2026-01-06 | Phase 6 | orchestrator | AC verification: all 8 ACs PASS, build OK, tests 33/33 | PASS |
| 2026-01-06 | Phase 7 | feature-reviewer | Post-review (mode: post) | READY |
| 2026-01-06 | Phase 7 | feature-reviewer | Doc-check (mode: doc-check) | READY |
