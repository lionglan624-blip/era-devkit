# Feature 419: Built-in Functions Data (String/Array)

## Status: [DONE]

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

Migrate data manipulation built-in functions (String and Array categories) from legacy FunctionMethod.cs to Era.Core.

Implements 22 functions for string operations (SUBSTRING, STRFIND, REPLACE, etc.) and array expression functions (SUMARRAY, MAXARRAY, FINDELEMENT, MATCH, etc.).

**Output**: `Era.Core/Functions/StringFunctions.cs`, `Era.Core/Functions/ArrayFunctions.cs`

---

## Background

### Philosophy (Mid-term Vision)

**Expression & Function System**: Migrate legacy ERB expression evaluation and built-in functions to Era.Core with proper separation of concerns. Data manipulation functions (String/Array) form a cohesive responsibility category - they operate on data without accessing game state, making them ideal for isolated testing and reuse.

This feature contributes to Phase 8 by delivering the "Data" function category - stateless string and array operations that depend only on their inputs.

### Problem (Current Issue)

FunctionMethod.cs (~800 lines) contains all 100+ built-in functions in a monolithic structure. String and array expression functions (22 functions, ~700 lines) need to be:
- Extracted to dedicated classes following SRP
- Migrated to Result type error handling
- Made testable without game state dependencies
- Registered in FunctionRegistry with DI

Legacy implementation issues:
- Mixed responsibilities (data manipulation + game state access)
- Exception-based error handling
- No dependency injection
- Difficult to test in isolation

### Goal (What to Achieve)

1. **Extract string functions** (10 functions) to `StringFunctions.cs`:
   - SUBSTRING, SUBSTRINGU (Unicode-aware substring)
   - STRFIND, STRFINDU (string search, Unicode-aware)
   - STRLENS, STRLENSU (string length variants)
   - REPLACE (string replacement)
   - TOUPPER, TOLOWER (string transformation)
   - ESCAPE (escape sequence processing)

   **Note**: TOSTR is implemented in F418 ConversionFunctions, not F419.

2. **Extract array expression functions** (12 functions) to `ArrayFunctions.cs`:
   - SUMARRAY, SUMCARRAY (array summation)
   - MAXARRAY, MAXCARRAY, MINARRAY, MINCARRAY (array extrema)
   - FINDELEMENT, FINDLASTELEMENT (element search)
   - MATCH, CMATCH (pattern matching)
   - INRANGEARRAY, INRANGECARRAY (range checking)

   **Note**: VARSIZE moved to F420 (requires IVariableStore for variable metadata access).

3. **Implement with Phase 4 design principles**:
   - Result type for error handling (no exceptions)
   - Strongly Typed IDs where applicable
   - DI registration in FunctionRegistry
   - No static classes

4. **Ensure legacy equivalence**:
   - All functions produce identical output to legacy implementation
   - Edge cases handled consistently (null, empty, out-of-bounds)

5. **Zero technical debt**:
   - No TODO/FIXME/HACK comments
   - Complete implementations only

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | StringFunctions.cs exists | file | Glob | exists | "Era.Core/Functions/StringFunctions.cs" | [x] |
| 2 | ArrayFunctions.cs exists | file | Glob | exists | "Era.Core/Functions/ArrayFunctions.cs" | [x] |
| 3 | C# build succeeds | build | Bash | succeeds | "dotnet build Era.Core" | [x] |
| 4 | SUBSTRING equivalence | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestSubstring" | [x] |
| 5 | STRFIND equivalence | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestStrfind" | [x] |
| 6 | STRLENS equivalence | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestStrlens" | [x] |
| 7 | REPLACE equivalence | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestReplace" | [x] |
| 8 | SUMARRAY equivalence | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestSumarray" | [x] |
| 9 | MAXARRAY equivalence | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestMaxarray" | [x] |
| 10 | FINDELEMENT equivalence | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestFindelement" | [x] |
| 11 | MATCH equivalence | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestMatch" | [x] |
| 12 | FunctionRegistry registration | test | Bash | succeeds | "dotnet test --filter Category=FunctionRegistry" | [x] |
| 13 | Zero technical debt | file | Grep | not_contains | "TODO|FIXME|HACK" | [x] |
| 14 | Full legacy equivalence | test | Bash | succeeds | "dotnet test --filter Category=DataFunctions" | [x] |

### AC Details

**AC#1-2**: File existence verification
- Test: `dir Era.Core\Functions\StringFunctions.cs` and `dir Era.Core\Functions\ArrayFunctions.cs`
- Expected: Files exist

**AC#3**: Build verification
- Test: `dotnet build Era.Core`
- Expected: Build succeeds with no errors

**AC#4-7**: String function equivalence tests
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~Test{FunctionName}"`
- Example: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TestSubstring"`
- Expected: Each function's tests pass (SUBSTRING, STRFIND, STRLENS, REPLACE)
- Verifies: Legacy behavior equivalence for each string operation individually

**AC#8-11**: Array expression function equivalence tests
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~Test{FunctionName}"`
- Example: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TestSumarray"`
- Expected: Each function's tests pass (SUMARRAY, MAXARRAY, FINDELEMENT, MATCH)
- Verifies: Legacy behavior equivalence for each array expression function individually

**AC#12**: FunctionRegistry registration
- Test: `dotnet test Era.Core.Tests --filter "Category=FunctionRegistry"`
- Expected: All 22 functions (10 string + 12 array) successfully registered and retrievable
- Verifies: DI integration works correctly

**AC Coverage Strategy**:
- AC#4-11 are **representative sample tests** for key functions (4 string + 4 array)
- AC#14 provides **comprehensive coverage** via `Category=DataFunctions` test category
- The Category=DataFunctions test suite includes 200+ test cases covering all 22 functions
- This sampling approach balances explicit verification with comprehensive coverage

**AC#13**: Zero technical debt
- Test: Grep tool with pattern `TODO|FIXME|HACK` on Era.Core/Functions/StringFunctions.cs and Era.Core/Functions/ArrayFunctions.cs
- Expected: No matches (zero technical debt)
- Verifies: Complete implementations only, no deferred work

**AC#14**: Comprehensive legacy equivalence
- Test: `dotnet test Era.Core.Tests --filter "Category=DataFunctions"`
- Expected: Full test suite passes (200+ test cases covering edge cases)
- Verifies: All string/array functions match legacy behavior exactly

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create Era.Core/Functions/StringFunctions.cs with 10 string functions | [x] |
| 2 | 2 | Create Era.Core/Functions/ArrayFunctions.cs with 12 array expression functions | [x] |
| 3 | 3 | Verify C# build succeeds | [x] |
| 4 | 4 | Implement and test SUBSTRING function equivalence | [x] |
| 5 | 5 | Implement and test STRFIND function equivalence | [x] |
| 6 | 6 | Implement and test STRLENS function equivalence | [x] |
| 7 | 7 | Implement and test REPLACE function equivalence | [x] |
| 8 | 8 | Implement and test SUMARRAY function equivalence | [x] |
| 9 | 9 | Implement and test MAXARRAY function equivalence | [x] |
| 10 | 10 | Implement and test FINDELEMENT function equivalence | [x] |
| 11 | 11 | Implement and test MATCH function equivalence | [x] |
| 12 | 12 | Register all functions in FunctionRegistry and verify | [x] |
| 13 | 13 | Verify no TODO/FIXME/HACK comments in new implementations | [x] |
| 14 | 14 | Verify full legacy equivalence with comprehensive test suite | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Interface Definitions

Following F418 pattern for consistency and DI registration:

```csharp
public interface IStringFunctions
{
    Result<string> Substring(string str, long start, long length);
    Result<string> SubstringU(string str, long start, long length);
    Result<long> Strfind(string str, string search, long start);
    Result<long> StrfindU(string str, string search, long start);
    Result<long> Strlens(string str);
    Result<long> StrlensU(string str);
    Result<string> Replace(string str, string oldValue, string newValue);
    Result<string> ToUpper(string str);
    Result<string> ToLower(string str);
    Result<string> Escape(string str);
}

public interface IArrayFunctions
{
    Result<long> Sumarray(long[] array, long start, long end);
    Result<long> Sumcarray(string[] array, long start, long end);
    Result<long> Maxarray(long[] array, long start, long end);
    Result<long> Maxcarray(string[] array, long start, long end);
    Result<long> Minarray(long[] array, long start, long end);
    Result<long> Mincarray(string[] array, long start, long end);
    Result<long> Findelement(long[] array, long value, long start, long end);
    Result<long> Findlastelement(long[] array, long value, long start, long end);
    Result<long> Match(long[] array, long start, params long[] values);
    Result<long> Cmatch(string[] array, long start, params string[] values);
    Result<long> Inrangearray(long[] array, long start, long end, long min, long max);
    Result<long> Inrangecarray(string[] array, long start, long end, long min, long max);
}
```

### DI Registration

```csharp
services.AddSingleton<IStringFunctions, StringFunctions>();
services.AddSingleton<IArrayFunctions, ArrayFunctions>();
```

### String Functions (10 functions)

**Source (registration)**: `Creator.cs` lines 117-130
**Source (implementation)**: `Creator.Method.cs` string-related lines

**Note**: TOSTR is in F418 ConversionFunctions, not F419. STRLEN does not exist in Creator.cs (only STRLENS/STRLENSU).

| Function | Purpose | Return Type | Source Lines |
|----------|---------|-------------|:------------:|
| SUBSTRING | Extract substring by byte offset | string | ~50 |
| SUBSTRINGU | Extract substring by Unicode char offset | string | ~50 |
| STRFIND | Find substring position (byte) | int | ~40 |
| STRFINDU | Find substring position (Unicode) | int | ~40 |
| STRLENS | String length (shift-jis byte) | int | ~20 |
| STRLENSU | String length (Unicode char) | int | ~20 |
| REPLACE | Replace substring | string | ~30 |
| TOUPPER | Convert to uppercase | string | ~15 |
| TOLOWER | Convert to lowercase | string | ~15 |
| ESCAPE | Process escape sequences | string | ~30 |

### Array Expression Functions (12 functions)

**Source (registration)**: `Creator.cs` lines 89-112
**Source (implementation)**: `Creator.Method.cs` array-related lines

**Note**: ARRAYSIZE, ARRAYSEARCH, ARRAYCOPY, ARRAYSHIFT, ARRAYREMOVE, ARRAYMSORT are **commands** (FunctionCode), not expression functions. They are out of scope for this feature.

**Note**: VARSIZE moved to F420 - requires IVariableStore for variable metadata access, conflicting with F419's "no game state access" philosophy.

| Function | Purpose | Return Type | Source Lines |
|----------|---------|-------------|:------------:|
| SUMARRAY | Sum array elements | int | ~30 |
| SUMCARRAY | Sum string array lengths | int | ~30 |
| MAXARRAY | Find maximum value (int array) | int | ~25 |
| MAXCARRAY | Find maximum value (string array) | int | ~25 |
| MINARRAY | Find minimum value (int array) | int | ~25 |
| MINCARRAY | Find minimum value (string array) | int | ~25 |
| FINDELEMENT | Find element index | int | ~40 |
| FINDLASTELEMENT | Find element from end | int | ~40 |
| MATCH | Pattern match (simple) | int | ~25 |
| CMATCH | Pattern match (regex) | int | ~35 |
| INRANGEARRAY | Count elements in range | int | ~30 |
| INRANGECARRAY | Count string elements in range | int | ~30 |

### Migration Steps

1. **Create StringFunctions.cs** (~350 lines total)
   - Implement all 10 string functions
   - Use Result type for error handling
   - Add XML documentation for each function
   - Follow Phase 4 design principles (no static classes)

2. **Create ArrayFunctions.cs** (~350 lines total)
   - Implement all 12 array expression functions
   - Use Result type for error handling
   - Handle edge cases (empty arrays, out-of-bounds)
   - Add XML documentation

3. **Create comprehensive tests** (Era.Core.Tests/Functions/)
   - `StringFunctionsTests.cs` (~400 lines, 100+ test cases)
   - `ArrayFunctionsTests.cs` (~300 lines, 80+ test cases)
   - Cover edge cases: null, empty, Unicode, out-of-bounds

4. **Register in FunctionRegistry**
   - Add string functions to registry
   - Add array functions to registry
   - Verify DI resolution works

5. **Remove technical debt**
   - Delete all TODO/FIXME/HACK comments
   - Ensure all implementations are complete
   - Verify AC#13 passes

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F409 | Phase 8 Planning must create this feature first |
| Predecessor | F421 | Function call mechanism (FunctionRegistry) must exist |
| Related | F377 | Phase 4 Design Principles (Result type, SRP, DI) |
| Related | F418 | Functions Core (Math/Random/Conversion) - parallel implementation |
| Related | F420 | Functions Game (Character/System) - parallel implementation |
| Successor | F423 | Phase 8 Post-Phase Review (will verify this feature) |

---

## Out-of-Scope Note

| Issue | Description | Disposition |
|-------|-------------|-------------|
| HalfwidthChars duplication | HalfwidthChars HashSet duplicated in StringFunctions.cs and ArrayFunctions.cs. DRY improvement for future refactor. | → F427 |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD FL iter{N}**: [{status}] {location} - {issue summary} -->

- **2026-01-09 FL iter5**: [resolved] STRLEN removed - does not exist in Creator.cs (only STRLENS/STRLENSU)
- **2026-01-09 FL iter5**: [resolved] MAXCARRAY/MINCARRAY added - exist in Creator.cs lines 98, 100
- **2026-01-09 FL iter5**: [resolved] String functions out-of-scope → deferred to F425 (String Functions Extended)
- **2026-01-09 FL iter5**: [resolved] Value comparison functions out-of-scope → deferred to F426 (Value Comparison Functions)
- **2026-01-09 FL iter7**: [resolved] ARRAYMSORT added to out-of-scope commands list
- **2026-01-09 FL iter7**: [resolved] Interface definitions - added explicit method signatures following F418 pattern
- **2026-01-09 FL iter7**: [resolved] AC coverage documentation - added explicit sampling strategy documentation
- **2026-01-09 FL iter8**: [resolved] VARSIZE moved to F420 - requires IVariableStore, conflicts with "no game state access" philosophy

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 | create | implementer | Created as Phase 8 sub-feature per F409 Task 5 | PROPOSED |
| 2026-01-09 20:32 | START | implementer | Task 1-14 | - |
| 2026-01-09 20:32 | END | implementer | Task 1-14 | SUCCESS |

---

## Links

- [feature-409.md](feature-409.md) - Phase 8 Planning (parent feature)
- [feature-377.md](feature-377.md) - Phase 4 Design Principles (Result type, SRP, DI)
- [feature-418.md](feature-418.md) - Functions Core (Math/Random/Conversion)
- [feature-420.md](feature-420.md) - Functions Game (Character/System)
- [feature-421.md](feature-421.md) - Function Call Mechanism (FunctionRegistry)
- [feature-423.md](feature-423.md) - Phase 8 Post-Phase Review
- [feature-425.md](feature-425.md) - String Functions Extended (deferred from F419)
- [feature-426.md](feature-426.md) - Value Comparison Functions (deferred from F419)
- [feature-427.md](feature-427.md) - ShiftJisHelper共通化 (follow-up from F419)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 8 definition
- [FunctionMethod.cs](../../engine/Assets/Scripts/Emuera/GameData/Function/FunctionMethod.cs) - Legacy implementation source
- [Creator.cs](../../engine/Assets/Scripts/Emuera/GameData/Function/Creator.cs) - Function registration source
