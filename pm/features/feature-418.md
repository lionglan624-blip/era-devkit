# Feature 418: Built-in Functions Core (Math/Random/Conversion)

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

Migrate 18 built-in functions from legacy `Creator.Method.cs` to C# with stateless computation focus.

Categories: Math (11 functions), Random (1 function), Conversion (6 functions).

**Note**: F409 estimated 30 functions but actual count is 18 after detailed analysis. Remaining functions are game-state-dependent and belong to F420 (Game Functions).

**Function Grouping Rationale**: Stateless computation - no game state access required.

---

## Background

### Philosophy (Mid-term Vision)

**Expression & Function System**: Phase 8 establishes a modern, testable expression evaluation system replacing legacy ERB function handling. Built-in functions are migrated to C# with proper type safety, DI support, and Result-based error handling.

**This feature's contribution**: Core stateless functions (Math/Random/Conversion) provide the foundation for expression evaluation without game state dependencies, enabling pure computation with minimal coupling.

### Problem (Current Issue)

Legacy `Creator.Method.cs` (~800 lines) contains stateless functions mixed with game-state-dependent functions. This feature extracts 18 core stateless functions:
- Math functions (ABS, MAX, MIN, POWER, SQRT, LOG, CBRT, EXPONENT, SIGN, LIMIT) scattered in monolithic file
- Random functions (RAND) use global state without DI
- Conversion functions (TOSTR, TOINT, ISNUMERIC, CONVERT, UNICODE) lack type safety
- No unit tests for individual function behavior
- Function registry hard-coded in static dictionary

### Goal (What to Achieve)

1. **Extract 18 Core functions** from `Creator.Method.cs` into Era.Core
2. **Implement DI-compatible function classes**: Math, Random, Conversion categories
3. **Prepare for registry integration**: Function classes ready for F421 (Function Call Mechanism) registration
4. **Zero technical debt**: Remove all TODO/FIXME/HACK comments
5. **Legacy equivalence**: All functions match legacy ERB behavior exactly

**Deliverables** (approximate line counts):
- `Era.Core/Functions/MathFunctions.cs` (~150-200 lines)
- `Era.Core/Functions/RandomFunctions.cs` (~80-100 lines)
- `Era.Core/Functions/ConversionFunctions.cs` (~70-100 lines)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | MathFunctions.cs created | file | Glob | exists | Era.Core/Functions/MathFunctions.cs | [x] |
| 2 | RandomFunctions.cs created | file | Glob | exists | Era.Core/Functions/RandomFunctions.cs | [x] |
| 3 | ConversionFunctions.cs created | file | Glob | exists | Era.Core/Functions/ConversionFunctions.cs | [x] |
| 4 | Math functions build success | build | dotnet build | succeeds | Era.Core | [x] |
| 5 | ABS function equivalence | unit | dotnet test --filter TestMathAbs | succeeds | - | [x] |
| 6 | MAX/MIN function equivalence | unit | dotnet test --filter TestMathMaxMin | succeeds | - | [x] |
| 7 | POWER/SQRT function equivalence | unit | dotnet test --filter TestMathPowerSqrt | succeeds | - | [x] |
| 8 | LOG function equivalence | unit | dotnet test --filter TestMathLog | succeeds | - | [x] |
| 9 | CBRT/EXPONENT function equivalence | unit | dotnet test --filter TestMathCbrtExponent | succeeds | - | [x] |
| 10 | SIGN/LIMIT function equivalence | unit | dotnet test --filter TestMathSignLimit | succeeds | - | [x] |
| 11 | RAND function equivalence | unit | dotnet test --filter TestRandomRand | succeeds | - | [x] |
| 12 | Conversion TOINT/TOSTR equivalence | unit | dotnet test --filter TestConversionToIntToStr | succeeds | - | [x] |
| 13 | ISNUMERIC function equivalence | unit | dotnet test --filter TestConversionIsNumeric | succeeds | - | [x] |
| 14 | CONVERT/UNICODE function equivalence | unit | dotnet test --filter TestConversionConvertUnicode | succeeds | - | [x] |
| 15 | No technical debt remains | file | Grep | not_contains | "TODO|FIXME|HACK" | [x] |

### AC Details

**AC#1-3**: Core function files exist in Era.Core/Functions/
- `MathFunctions.cs`: ABS, MAX, MIN, POWER, SQRT, CBRT, LOG, LOG10, EXPONENT, SIGN, LIMIT (11 functions)
- `RandomFunctions.cs`: RAND (1 function, with DI-based random number generator)
- `ConversionFunctions.cs`: TOSTR, TOINT, ISNUMERIC, CONVERT, UNICODE, UNICODEBYTE (6 functions)

**Test**: Glob pattern `Era.Core/Functions/*.cs`

**AC#4**: C# build succeeds with no errors or warnings

**Test**: `dotnet build Era.Core/Era.Core.csproj`

**AC#5**: ABS function returns absolute value matching legacy behavior

**Test**: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TestMathAbs"`
**Expected**: Test verifies `ABS(-5)` → `5`, `ABS(10)` → `10`

**AC#6**: MAX/MIN functions return correct values for variable-length arguments

**Test**: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TestMathMaxMin"`
**Expected**: `MAX(1, 5, 3)` → `5`, `MIN(1, 5, 3)` → `1`

**AC#7**: POWER/SQRT functions with error handling (overflow, negative sqrt)

**Test**: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TestMathPowerSqrt"`
**Expected**: `SQRT(-1)` → Result.Fail("SQRT関数の引数に負の値が指定されました"), `POWER(2, 3)` → `8`

**AC#8**: LOG function with natural/base-10 variants

**Test**: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TestMathLog"`
**Expected**: `LOG(1)` → `0`, `LOG10(100)` → `2`

**AC#9**: CBRT/EXPONENT functions with error handling

**Test**: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TestMathCbrtExponent"`
**Expected**: `CBRT(8)` → `2`, `CBRT(-8)` → Result.Fail("CBRT関数の引数に負の値が指定されました"), `EXPONENT(10)` → `22026` (EXPONENT uses Math.Exp, so EXPONENT(x) = e^x truncated to integer)

**AC#10**: SIGN/LIMIT (clamp) functions

**Test**: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TestMathSignLimit"`
**Expected**: `SIGN(-5)` → `-1`, `SIGN(0)` → `0`, `SIGN(5)` → `1`, `LIMIT(15, 0, 10)` → `10`

**AC#11**: RAND function with DI-based random generator

**Test**: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TestRandomRand"`
**Expected**: `RAND(max)` → [0, max), `RAND(min, max)` → [min, max), error validation for invalid range

**AC#12**: TOINT/TOSTR conversion functions

**Test**: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TestConversionToIntToStr"`
**Expected**: `TOINT("123")` → `123`, `TOSTR(123)` → `"123"`, `TOSTR(255, "X")` → `"FF"`, `TOSTR(10, "InvalidFormat")` → Result.Fail("TOSTR関数の書式指定が間違っています")

**AC#13**: ISNUMERIC validation function

**Test**: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TestConversionIsNumeric"`
**Expected**: `ISNUMERIC("123")` → `1`, `ISNUMERIC("abc")` → `0`

**AC#14**: CONVERT/UNICODE/UNICODEBYTE conversion functions

**Test**: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TestConversionConvertUnicode"`
**Expected**: `UNICODE(65)` → `"A"`, `UNICODEBYTE("A")` → `65`, `CONVERT(255, 16)` → `"ff"`, `CONVERT(10, 3)` → Result.Fail("CONVERT関数の第２引数は2, 8, 10, 16のいずれかでなければなりません"), `UNICODE(0x10000)` → Result.Fail("UNICODE関数に範囲外の値が渡されました") (range: 0-0xFFFF), `UNICODE(0x01)` → empty string with warning (control character)

**AC#15**: Technical debt verification - no TODO/FIXME/HACK in Era.Core/Functions/

**Test**: `rg "TODO|FIXME|HACK" Era.Core/Functions/`
**Expected**: No matches (empty output)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create MathFunctions.cs with 11 math functions (ABS/MAX/MIN/POWER/SQRT/CBRT/LOG/LOG10/EXPONENT/SIGN/LIMIT) | [x] |
| 2 | 2 | Create RandomFunctions.cs with RAND function and IRandom DI interface | [x] |
| 3 | 3 | Create ConversionFunctions.cs with TOSTR/TOINT/ISNUMERIC/CONVERT/UNICODE/UNICODEBYTE | [x] |
| 4 | 5-10 | Write equivalence tests for Math functions (ABS/MAX/MIN/POWER/SQRT/LOG/CBRT/EXPONENT/SIGN/LIMIT) | [x] |
| 5 | 11 | Write equivalence tests for RAND function with mock random generator | [x] |
| 6 | 12-14 | Write equivalence tests for Conversion functions (TOINT/TOSTR/ISNUMERIC/CONVERT/UNICODE/UNICODEBYTE) | [x] |
| 7 | 4 | Verify build success and fix any compilation errors | [x] |
| 8 | 15 | Remove all TODO/FIXME/HACK comments from function implementations | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Function Implementation Pattern

**MUST follow Phase 4 Design Principles** ([F377](feature-377.md)):

```csharp
// Math Functions - stateless, pure computation
public interface IMathFunctions
{
    Result<long> Abs(long value);
    Result<long> Max(params long[] values);
    Result<long> Min(params long[] values);
    Result<long> Power(long baseValue, long exponent);
    Result<long> Sqrt(long value);
    // ... other math functions
}

// Random Functions - DI-based random generator
public interface IRandom
{
    long Next(long max);
    long Next(long min, long max);
}

public interface IRandomFunctions
{
    Result<long> Rand(long max);
    Result<long> Rand(long min, long max);
}

public sealed class RandomFunctions : IRandomFunctions
{
    private readonly IRandom _random;

    public RandomFunctions(IRandom random)
    {
        ArgumentNullException.ThrowIfNull(random);
        _random = random;
    }

    // Implementation...
}

// Conversion Functions - type conversion utilities
public interface IConversionFunctions
{
    Result<string> ToStr(long value);
    Result<string> ToStr(long value, string format);  // Optional format (e.g., "X" for hex)
    Result<long> ToInt(string value);
    Result<long> IsNumeric(string value);
    Result<string> Convert(long value, long toBase);  // Base 2, 8, 10, 16 only
    Result<string> Unicode(long codePoint);           // Code point to character
    Result<long> UnicodeByte(string character);       // Character to code point
}
```

**Error Handling Pattern** (per architecture.md Exception vs Result):

| Scenario | Approach | Example |
|----------|----------|---------|
| Invalid input (recoverable) | `Result<T>.Fail()` | SQRT(-1) → Fail("Negative value") |
| Programmer error | Exception | null argument → ArgumentNullException |
| Overflow/underflow | `Result<T>.Fail()` | POWER overflow → Fail("Overflow") |

**Legacy Behavior Preservation**:
- CBRT: Legacy throws error for negative values despite being mathematically valid (cbrt(-8) = -2). Preserve this behavior for equivalence.

**DI Registration** (in `ServiceCollectionExtensions.cs`):

```csharp
services.AddSingleton<IMathFunctions, MathFunctions>();
services.AddSingleton<IRandom, SystemRandom>();
services.AddSingleton<IRandomFunctions, RandomFunctions>();
services.AddSingleton<IConversionFunctions, ConversionFunctions>();
```

**Function Registry Integration**:

Functions will be registered in `IFunctionRegistry` (F421) via:
```csharp
registry.Register("ABS", context => mathFunctions.Abs(context.GetArg<long>(0)));
registry.Register("MAX", context => mathFunctions.Max(context.GetArgs<long>()));
// ... etc
```

### Source Migration Reference

**Legacy Location**: `engine/Assets/Scripts/Emuera/GameData/Function/Creator.Method.cs`

| Function | Line Range | Notes |
|----------|------------|-------|
| RandMethod | 923-975 | RAND function with min/max variants |
| MaxMethod | 977-1027 | MAX/MIN with variable arguments |
| AbsMethod | 1029-1042 | Simple absolute value |
| PowerMethod | 1044-1065 | Power with overflow checks |
| SqrtMethod | 1067-1082 | Square root with negative check |
| CbrtMethod | 1084-1099 | Cube root |
| LogMethod | 1101-1138 | LOG/LOG10 variants |
| ExpMethod | 1140-1161 | Exponent function |
| SignMethod | 1163-1177 | Sign function |
| GetLimitMethod | 1179-1201 | LIMIT (clamp) function |

**Conversion Functions** (Creator.Method.cs lines 2301-2578):
- TOSTR, TOINT, ISNUMERIC, CONVERT, UNICODE, UNICODEBYTE

### Equivalence Test Requirements

**Test Location**: `Era.Core.Tests/Functions/CoreFunctionsTests.cs`

Each test MUST verify:
1. **Exact output match** with legacy function for same input
2. **Error case handling** matches legacy error messages
3. **Edge cases**: overflow, underflow, negative values, zero, max/min values

**Example Test Structure**:

```csharp
[Fact]
public void TestMathAbs()
{
    var mathFunctions = new MathFunctions();

    // Positive case
    var result = mathFunctions.Abs(-5);
    var success = Assert.IsType<Result<long>.Success>(result);
    Assert.Equal(5, success.Value);

    // Already positive
    result = mathFunctions.Abs(10);
    success = Assert.IsType<Result<long>.Success>(result);
    Assert.Equal(10, success.Value);

    // Zero
    result = mathFunctions.Abs(0);
    success = Assert.IsType<Result<long>.Success>(result);
    Assert.Equal(0, success.Value);
}
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F409 | Phase 8 Planning (this feature created by F409 Task 4) |
| Related | F377 | Phase 4 Design Principles (Result型, DI, static class禁止) |
| Related | F404 | IVariableStore ISP (interface segregation pattern reference) |
| Successor | F421 | Function Call Mechanism (registry integration) |

---

## Links

- [feature-409.md](feature-409.md) - Phase 8 Planning (parent feature)
- [feature-377.md](feature-377.md) - Phase 4 Design Principles
- [feature-404.md](feature-404.md) - IVariableStore ISP (interface segregation pattern reference)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 8 definition
- [feature-421.md](feature-421.md) - Function Call Mechanism (to be created)

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-09 FL iter3**: [resolved] Result<T> usage for non-failing functions - Validated that ABS(long.MinValue) CAN throw OverflowException, so Result<T> is appropriate. Functions that truly cannot fail (MAX/MIN/SIGN/LIMIT) still benefit from uniform API for consistency.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 | create | implementer | Created F418 per F409 Task 4 requirement | PROPOSED |
| 2026-01-09 16:16 | START | implementer | Task 1 | - |
| 2026-01-09 16:16 | END | implementer | Task 1 | SUCCESS |
| 2026-01-09 16:16 | START | implementer | Task 2 | - |
| 2026-01-09 16:16 | END | implementer | Task 2 | SUCCESS |
| 2026-01-09 16:16 | START | implementer | Task 3 | - |
| 2026-01-09 16:16 | END | implementer | Task 3 | SUCCESS |
| 2026-01-09 16:16 | START | implementer | Task 4 (Math tests) | - |
| 2026-01-09 16:16 | END | implementer | Task 4 (Math tests) | SUCCESS |
| 2026-01-09 16:16 | START | implementer | Task 5 (Random tests) | - |
| 2026-01-09 16:16 | END | implementer | Task 5 (Random tests) | SUCCESS |
| 2026-01-09 16:16 | START | implementer | Task 6 (Conversion tests) | - |
| 2026-01-09 16:16 | END | implementer | Task 6 (Conversion tests) | SUCCESS |
| 2026-01-09 16:19 | DEVIATION | debugger | TestMathLog: Expected 1, Actual 0 (test expected wrong value) | FIXED |
| 2026-01-09 16:19 | DEVIATION | debugger | TestConversionToIntToStr: .NET format handling differs from legacy | FIXED |
| 2026-01-09 16:20 | VERIFY | ac-tester | All 10 function tests pass | SUCCESS |
| 2026-01-09 16:20 | START | implementer | Task 7 (Build verification) | - |
| 2026-01-09 16:20 | END | implementer | Task 7 (Build verification) | SUCCESS |
| 2026-01-09 16:20 | START | implementer | Task 8 (No technical debt) | - |
| 2026-01-09 16:20 | END | implementer | Task 8 (No technical debt) | SUCCESS |
| 2026-01-09 16:21 | VERIFY | regression-tester | Era.Core.Tests 354/354 pass | SUCCESS |
