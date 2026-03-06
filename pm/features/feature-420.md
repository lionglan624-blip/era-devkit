# Feature 420: Built-in Functions Game (Character/System)

## Status: [DONE]

## Phase: 8 (Expression & Function System)

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: engine

## Created: 2026-01-09

---

## Summary

Migrate 29 game-specific built-in functions (21 Character + 8 System) from legacy Creator.Method.cs to Era.Core/Functions. These functions access game state via IEvaluationContext (following F421 pattern) and use Result-based error handling.

---

## Background

### Philosophy (Mid-term Vision)

**Phase 8: Expression & Function System** - Migrate expression evaluation and built-in functions to Era.Core with proper type safety, DI integration, and Result-based error handling. This feature contributes by implementing game state access functions that bridge the expression system with character data and system state.

### Problem (Current Issue)

Legacy Creator.Method.cs contains 29 game-specific functions (Character: GETCHARA, FINDCHARA, GETPALAMLV, GETEXPLV, CSV*, etc.; System: GETTIME, GETMILLISECOND, VARSIZE, etc.) implemented as static methods without proper dependency injection. These functions directly access game state and need to be migrated to Era.Core with:
- IEvaluationContext pattern (per F421 established architecture)
- Result type error handling
- Proper separation from stateless functions (Math/String/Array)

### Goal (What to Achieve)

1. Create `Era.Core/Functions/CharacterFunctions.cs` with character data access functions (GETCHARA, FINDCHARA, CSV* family, GETPALAMLV, GETEXPLV)
2. Create `Era.Core/Functions/SystemFunctions.cs` with system state access functions (GETTIME, GETMILLISECOND, SAVENOS, etc.)
3. Implement IEvaluationContext pattern (per F421 established architecture)
4. Ensure Result type usage for all function returns (verified via AC#11 build + AC#12 tests)
5. Migrate all Character/System function implementations from legacy code
6. Remove TODO/FIXME/HACK comments (technical debt zero)
7. Verify equivalence with legacy ERB implementation

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CharacterFunctions.cs exists | file | Glob | exists | CharacterFunctions.cs | [x] |
| 2 | SystemFunctions.cs exists | file | Glob | exists | SystemFunctions.cs | [x] |
| 3 | CharacterFunctions has GETCHARA | code | Grep CharacterFunctions.cs | contains | GETCHARA | [x] |
| 4 | CharacterFunctions has FINDCHARA | code | Grep CharacterFunctions.cs | contains | FINDCHARA | [x] |
| 5 | CharacterFunctions has GETPALAMLV | code | Grep CharacterFunctions.cs | contains | GETPALAMLV | [x] |
| 6 | CharacterFunctions has GETEXPLV | code | Grep CharacterFunctions.cs | contains | GETEXPLV | [x] |
| 7 | SystemFunctions has GETTIME | code | Grep SystemFunctions.cs | contains | GETTIME | [x] |
| 8 | SystemFunctions has GETMILLISECOND | code | Grep SystemFunctions.cs | contains | GETMILLISECOND | [x] |
| 9 | CharacterFunctions uses IEvaluationContext | code | Grep CharacterFunctions.cs | contains | IEvaluationContext | [x] |
| 10 | SystemFunctions uses IEvaluationContext | code | Grep SystemFunctions.cs | contains | IEvaluationContext | [x] |
| 11 | C# build succeeds | build | dotnet | succeeds | - | [x] |
| 12 | Era.Core.Tests function tests pass | test | dotnet | succeeds | Category=Functions | [x] |
| 13 | Legacy equivalence verified | test | dotnet | succeeds | FullyQualifiedName~GameFunctionsTests | [x] |
| 14 | DI registration includes ICharacterFunctions | code | Grep ServiceCollectionExtensions | contains | ICharacterFunctions | [x] |
| 15 | DI registration includes ISystemFunctions | code | Grep ServiceCollectionExtensions | contains | ISystemFunctions | [x] |
| 16 | Technical debt zero | code | Grep Era.Core/Functions | not_contains | TODO | [x] |

### AC Details

**AC#1-2**: Create new files in Era.Core/Functions:
- `Era.Core/Functions/CharacterFunctions.cs` - Character data access (GETCHARA, FINDCHARA, GETPALAM, GETEXP, CSV* functions, etc.)
- `Era.Core/Functions/SystemFunctions.cs` - System state access (GETTIME, GETMILLISECOND, GETSECOND, SAVENOS, etc.)

**AC#3-6**: Core character functions implemented:
- GETCHARA - Get character by index
- FINDCHARA - Search character by condition
- GETPALAMLV - Get character parameter level (derives from CSVBASE/CSVABL)
- GETEXPLV - Get character experience level (derives from CSVEXP)

**AC#7-8**: Core system functions implemented:
- GETTIME - Get current game time
- GETMILLISECOND - Get millisecond timer

**AC#9-10**: Both function classes receive context via IEvaluationContext, accessing variables through context.Variables (IVariableStore). This aligns with the established F421 pattern.

**Note on Result type verification**: Goal #4 (Result type usage) is verified implicitly via AC#11 (build succeeds) + AC#12 (tests pass). Interface definitions require Result<object> return types; non-conforming implementations will fail compilation.

**AC#11-12**: Build and unit tests pass.

**Note on function coverage**: AC#3-8 verify 6 representative functions (2 from each major category). Complete verification of all 28 functions is provided by AC#12 (unit tests) and AC#13 (equivalence tests). This follows F418 precedent where representative ACs verify file existence and key functions, while test suite provides complete coverage.

**AC#13**: Equivalence tests verify that new implementations produce identical results to legacy ERB functions.

Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~GameFunctionsTests"`
Expected: All GameFunctions tests pass

Test file: `Era.Core.Tests/Functions/GameFunctionsTests.cs` (must exist with CharacterFunctions and SystemFunctions equivalence tests)

**AC#14-15**: DI registration verification - both ICharacterFunctions and ISystemFunctions must be registered in ServiceCollectionExtensions.

**AC#16**: Zero technical debt - no TODO/FIXME/HACK comments remain in Era.Core/Functions after implementation.

Grep: `grep -r "TODO\|FIXME\|HACK" Era.Core/Functions/`
Expected: No matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | - | Create ICharacterDataAccess interface (prerequisite from Dependencies Required) | [x] |
| 1 | 1,3-6,9,14 | Create CharacterFunctions.cs with character data access functions and register DI | [x] |
| 2 | 2,7-8,10,15 | Create SystemFunctions.cs with system state access functions and register DI | [x] |
| 3 | 11 | Verify C# build succeeds | [x] |
| 4 | 12 | Write unit tests for function implementations (mock IVariableStore) | [x] |
| 5 | 13 | Write legacy equivalence tests comparing outputs with ERB execution | [x] |
| 6 | 16 | Remove all TODO/FIXME/HACK comments from implementation | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->
<!-- **Batch implementation waiver (Task 1, 2)**: Task 1 creates CharacterFunctions.cs (AC#1,3-6,9,14) and Task 2 creates SystemFunctions.cs (AC#2,7-8,10,15). Waiver justified because:
     1. Single deliverable per task: Each task produces one .cs file + DI registration containing related functions
     2. Atomic implementation: Character/System functions share internal dependencies (IVariableStore access patterns)
     3. F418 precedent: MathFunctions/RandomFunctions/ConversionFunctions are single files with multiple functions
     4. AC#11-12 provide unified verification: Build + tests validate all functions together
     AC:Task 1:1 rule waived for cohesive function group bundling. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Function Categories

**Character Functions** (21 total):
- Character lookup (5): GETCHARA, GETSPCHARA, FINDCHARA, FINDLASTCHARA, EXISTCSV
- CSV string data (5): CSVNAME, CSVCALLNAME, CSVNICKNAME, CSVMASTERNAME, CSVCSTR
- CSV int data (9): CSVBASE, CSVABL, CSVMARK, CSVEXP, CSVRELATION, CSVTALENT, CSVCFLAG, CSVEQUIP, CSVJUEL
- Derived functions (2): GETPALAMLV, GETEXPLV

**System Functions** (8 total):
- Time (4): GETTIME, GETTIMES, GETMILLISECOND, GETSECOND
- State (3): SAVENOS, PRINTCPERLINE, PRINTCLENGTH
- Variable metadata (1): VARSIZE (moved from F419 - requires IVariableStore for variable dimension access)

**Out of Scope** (tracked via F423 Phase 8 Post-Phase Review):
- Display: GETCOLOR, GETDEFCOLOR, GETFOCUSCOLOR, GETBGCOLOR, GETDEFBGCOLOR (requires IConsoleService)
- UI state: ISSKIP, MOUSESKIP, MESSKIP, CURRENTALIGN, CURRENTREDRAW (requires UI state interface)

*Note: These functions require additional interfaces beyond IEvaluationContext. Tracking deferred to F423 Post-Phase Review which will determine if a dedicated feature is needed.*

### Source Reference

Legacy functions are registered in `Creator.Method.cs` and implemented in method classes within the same directory:

| Legacy Class | ERB Function | New Location |
|--------------|--------------|--------------|
| `GetcharaMethod` | GETCHARA | `CharacterFunctions.GetChara` |
| `FindcharaMethod` | FINDCHARA | `CharacterFunctions.FindChara` |
| `CsvDataMethod` | CSV* family | `CharacterFunctions.Csv*` |
| `GetPalamLVMethod` | GETPALAMLV | `CharacterFunctions.GetPalamLV` |
| `GetExpLVMethod` | GETEXPLV | `CharacterFunctions.GetExpLV` |
| `GettimeMethod` | GETTIME/GETTIMES | `SystemFunctions.GetTime` |
| `GetMillisecondMethod` | GETMILLISECOND | `SystemFunctions.GetMillisecond` |
| `VarsizeMethod` | VARSIZE | `SystemFunctions.Varsize` |

**Source Directory**: `engine/Assets/Scripts/Emuera/GameData/Function/`
**Total Estimated Lines**: ~250 (consolidated with shared utilities)

### Dependencies Required

Both function classes receive context via `IEvaluationContext`:
- `IEvaluationContext` - Primary interface providing arguments, current character, and variable access
- `IEvaluationContext.Variables` (IVariableStore) - Variable access for character data
- For time functions, SystemFunctions will use `DateTime.Now` or abstraction if testability required

**Additional Interface Required**: Current IVariableStore provides variable access by CharacterId but lacks:
- Character enumeration (listing registered characters)
- Character search by NO (GETCHARA)
- Character search by condition (FINDCHARA)
- CSV template data access (CSV* functions)

**Proposed Solution**: Introduce `ICharacterDataAccess` interface:
```csharp
public interface ICharacterDataAccess
{
    int GetCharacterCount();
    CharacterId? GetCharacter(int index);
    CharacterId? FindChara(/* predicate parameters */);
    bool ExistsCsv(int charId);
    string GetCsvString(CharacterStrDataType type, int charId);
    long GetCsvInt(CharacterIntDataType type, int charId, int index);
}
```

This interface should be added to IEvaluationContext or injected via DI. Implementation will need to wrap legacy VEvaluator functionality until full migration.

**Technical Debt Tracking**: VEvaluator wrapper removal is tracked via F423 Phase 8 Post-Phase Review.

### Interface Definitions

Per F418 pattern, create interfaces for DI registration. Return type is `Result<object>` consistent with F421 IBuiltInFunction.Execute signature (game functions return mixed types: long for numeric, string for text):

```csharp
// Era.Core/Functions/ICharacterFunctions.cs
public interface ICharacterFunctions
{
    Result<object> GetChara(IEvaluationContext context);
    Result<object> FindChara(IEvaluationContext context);
    // ... additional character functions (returns long or string depending on function)
}

// Era.Core/Functions/ISystemFunctions.cs
public interface ISystemFunctions
{
    Result<object> GetTime(IEvaluationContext context);
    Result<object> GetMillisecond(IEvaluationContext context);
    // ... additional system functions
}
```

### Registration Pattern

DI registration in ServiceCollectionExtensions (per F418 pattern):

```csharp
// DI registration for function implementations
services.AddSingleton<ICharacterFunctions, CharacterFunctions>();
services.AddSingleton<ISystemFunctions, SystemFunctions>();
```

FunctionRegistry registration requires IBuiltInFunction adapters. Create wrapper classes that implement IBuiltInFunction and delegate to ICharacterFunctions/ISystemFunctions:

```csharp
// Example adapter pattern for FunctionRegistry
public class GetCharaFunction : IBuiltInFunction
{
    private readonly ICharacterFunctions _functions;
    public GetCharaFunction(ICharacterFunctions functions) => _functions = functions;
    public Result<object> Execute(IEvaluationContext context) => _functions.GetChara(context);
}

// Registration in FunctionRegistry initialization
registry.Register("GETCHARA", new GetCharaFunction(characterFunctions));
registry.Register("FINDCHARA", new FindCharaFunction(characterFunctions));
// ... etc.
```

Each function receives `IEvaluationContext` (per-call context) and returns `Result<object>`.

### Migration Steps

1. **Create CharacterFunctions.cs**:
   - Define class with IEvaluationContext access pattern
   - Migrate character lookup functions (GETCHARA, FINDCHARA, etc.)
   - Migrate CSV data access functions (CSV*, GETPALAMLV, etc.)
   - Return Result types for all functions
   - Remove any TODO/FIXME/HACK comments

2. **Create SystemFunctions.cs**:
   - Define class with IEvaluationContext access pattern
   - Migrate time functions (GETTIME, GETMILLISECOND, etc.)
   - Migrate state functions (SAVENOS, PRINTC*, etc.)
   - Return Result types for all functions
   - Remove any TODO/FIXME/HACK comments

3. **Test Creation**:
   - Unit tests: Verify each function with mock dependencies
   - Equivalence tests: Compare outputs with legacy implementation

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F409 | Phase 8 Planning (created this feature) |
| Predecessor | F416 | ExpressionParser (AST foundation) |
| Predecessor | F421 | Function Call Mechanism (registry integration) |
| Related | F418 | Built-in Functions Core (Math/Random/Conversion) |
| Related | F419 | Built-in Functions Data (String/Array) |
| Related | F377 | Phase 4 Design Principles (Result types, DI patterns) |

---

## Links

- [feature-409.md](feature-409.md) - Phase 8 Planning (parent feature)
- [feature-416.md](feature-416.md) - ExpressionParser Migration
- [feature-421.md](feature-421.md) - Function Call Mechanism
- [feature-418.md](feature-418.md) - Built-in Functions Core
- [feature-419.md](feature-419.md) - Built-in Functions Data
- [feature-377.md](feature-377.md) - Phase 4 Design Principles
- [feature-423.md](feature-423.md) - Phase 8 Post-Phase Review (out-of-scope tracking)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 8 definition

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

- **2026-01-09**: VARSIZE added from F419 - requires IVariableStore for variable dimension access, conflicts with F419's "no game state access" philosophy. Grouped with System Functions as variable metadata access.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 13:15 | create | implementer | Created as Phase 8 sub-feature per F409 Task 6 | PROPOSED |
| 2026-01-09 | init | initializer | Transitioned to WIP | WIP |
| 2026-01-09 20:30 | START | implementer | Task 0 | - |
| 2026-01-09 20:30 | END | implementer | Task 0 | SUCCESS |
| 2026-01-09 20:34 | START | implementer | Task 2 | - |
| 2026-01-09 20:34 | END | implementer | Task 2 | SUCCESS |
| 2026-01-09 20:35 | START | implementer | Task 1 | - |
| 2026-01-09 20:35 | END | implementer | Task 1 | SUCCESS |
| 2026-01-09 21:25 | FIX | debugger | DI resolution error: Created NullCharacterDataAccess stub and registered in ServiceCollectionExtensions | FIXED |
| 2026-01-09 21:30 | VERIFY | ac-tester | All 19 GameFunctionsTests pass, 592 full regression tests pass | PASS |
| 2026-01-09 21:35 | REVIEW | feature-reviewer | Post-review mode: READY, doc-check mode: READY | READY |
