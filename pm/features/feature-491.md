# Feature 491: ResultAssert Test Helper for Result<T> Type Verification

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

## Created: 2026-01-14

---

## Summary

Create a custom assertion helper class `ResultAssert` for type-safe verification of `Result<T>` discriminated unions, providing informative error messages and idiomatic xUnit patterns.

**Output**: `Era.Core.Tests/Assertions/ResultAssert.cs`

**Volume**: ~50 lines (within ~300 line limit for engine type).

---

## Background

### Philosophy (Mid-term Vision)

**Test Clarity** - Unit test failures should provide immediate diagnostic value. Assertion error messages must clearly indicate what was expected vs what was received, enabling rapid debugging without source code inspection.

### Problem (Current Issue)

F490 fixed Assert.IsType failures by replacing with `Assert.True(result is T)`. This works but:

| Issue | Impact |
|-------|--------|
| Generic error message | "Assert.True() Failure" - no type information |
| Lost return value | Assert.IsType returns cast value; Assert.True does not |
| Non-idiomatic | Hides intent - reader doesn't know it's type checking |
| Scattered pattern | Same workaround repeated across test files |

### Goal (What to Achieve)

1. Single source of truth for Result<T> assertions
2. Informative error messages showing expected vs actual type
3. Idiomatic API returning extracted value for chaining
4. Zero technical debt in implementation

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ResultAssert.cs exists | file | Glob | exists | Era.Core.Tests/Assertions/ResultAssert.cs | [x] |
| 2 | AssertSuccess method exists | code | Grep(Era.Core.Tests/Assertions/ResultAssert.cs) | contains | public static T AssertSuccess | [x] |
| 3 | AssertFailure method exists | code | Grep(Era.Core.Tests/Assertions/ResultAssert.cs) | contains | public static string AssertFailure | [x] |
| 4 | Unit tests pass | test | dotnet test | succeeds | exit code 0 | [x] |
| 5 | Build succeeds | build | dotnet build | succeeds | exit code 0 | [x] |
| 6 | Zero technical debt | code | Grep(Era.Core.Tests/Assertions) | not_contains | TODO\|FIXME\|HACK | [x] |

### AC Details

**AC#1**: ResultAssert.cs exists
- Test: Glob pattern="Era.Core.Tests/Assertions/ResultAssert.cs"
- Expected: File exists
- Note: On Windows, Glob may fail with forward-slash patterns. Use `dir Era.Core.Tests\Assertions\ResultAssert.cs` as backup verification per CLAUDE.md.

**AC#2**: AssertSuccess method signature
- Test: Grep pattern="public static T AssertSuccess" path="Era.Core.Tests/Assertions/ResultAssert.cs"
- Expected: Contains generic method returning T

**AC#3**: AssertFailure method signature
- Test: Grep pattern="public static string AssertFailure" path="Era.Core.Tests/Assertions/ResultAssert.cs"
- Expected: Contains method returning error string

**AC#4**: Unit tests pass
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~ResultAssertTests"`
- Expected: Exit code 0

**AC#5**: Build succeeds
- Test: `dotnet build Era.Core.Tests`
- Expected: Exit code 0

**AC#6**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core.Tests/Assertions"
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | Create ResultAssert.cs with AssertSuccess and AssertFailure methods | [x] |
| 2 | 4 | Create ResultAssertTests.cs with positive/negative test cases | [x] |
| 3 | 5,6 | Verify build and zero technical debt | [x] |
<!-- **Batch verification waiver (Task 1)**: AC#1,2,3 all verify same file creation/content. Split unnecessary per ENGINE.md Issue 7. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### File Structure

| File | Purpose |
|------|---------|
| `Era.Core.Tests/Assertions/ResultAssert.cs` | Static assertion helper class |
| `Era.Core.Tests/Assertions/ResultAssertTests.cs` | Unit tests for ResultAssert |

### ResultAssert Interface

```csharp
// Era.Core.Tests/Assertions/ResultAssert.cs
using Era.Core.Types;
using Xunit;

namespace Era.Core.Tests.Assertions;

/// <summary>
/// Custom assertion helpers for Result&lt;T&gt; discriminated union type verification.
/// Provides informative error messages and returns extracted values for assertion chaining.
/// </summary>
public static class ResultAssert
{
    /// <summary>
    /// Asserts that result is Success and returns the extracted value.
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="result">The result to verify</param>
    /// <param name="message">Optional custom message on failure</param>
    /// <returns>The extracted success value</returns>
    /// <exception cref="Xunit.Sdk.XunitException">If result is not Success</exception>
    public static T AssertSuccess<T>(Result<T> result, string? message = null)
    {
        Assert.True(
            result is Result<T>.Success,
            message ?? $"Expected Result<{typeof(T).Name}>.Success but got {result.GetType().Name}");
        return ((Result<T>.Success)result).Value;
    }

    /// <summary>
    /// Asserts that result is Failure and returns the error message.
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="result">The result to verify</param>
    /// <param name="message">Optional custom message on failure</param>
    /// <returns>The extracted error message</returns>
    /// <exception cref="Xunit.Sdk.XunitException">If result is not Failure</exception>
    public static string AssertFailure<T>(Result<T> result, string? message = null)
    {
        Assert.True(
            result is Result<T>.Failure,
            message ?? $"Expected Result<{typeof(T).Name}>.Failure but got {result.GetType().Name}");
        return ((Result<T>.Failure)result).Error;
    }
}
```

### Test Cases (Positive/Negative)

```csharp
// Era.Core.Tests/Assertions/ResultAssertTests.cs
using Era.Core.Types;
using Xunit;
using Xunit.Sdk;

namespace Era.Core.Tests.Assertions;

public class ResultAssertTests
{
    // Positive tests
    [Fact]
    public void AssertSuccess_WithSuccess_ReturnsValue()
    {
        var result = Result<int>.Ok(42);
        var value = ResultAssert.AssertSuccess(result);
        Assert.Equal(42, value);
    }

    [Fact]
    public void AssertFailure_WithFailure_ReturnsError()
    {
        var result = Result<int>.Fail("test error");
        var error = ResultAssert.AssertFailure(result);
        Assert.Equal("test error", error);
    }

    // Negative tests
    [Fact]
    public void AssertSuccess_WithFailure_ThrowsWithMessage()
    {
        var result = Result<int>.Fail("error");
        var ex = Assert.ThrowsAny<XunitException>(
            () => ResultAssert.AssertSuccess(result));
        Assert.Contains("Expected Result<Int32>.Success", ex.Message);
    }

    [Fact]
    public void AssertFailure_WithSuccess_ThrowsWithMessage()
    {
        var result = Result<int>.Ok(42);
        var ex = Assert.ThrowsAny<XunitException>(
            () => ResultAssert.AssertFailure(result));
        Assert.Contains("Expected Result<Int32>.Failure", ex.Message);
    }
}
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| After | F490 | Assert.IsType workaround identified need for this helper |
| Uses | Result<T> | Era.Core/Types/Result.cs discriminated union |

---

## Links

- [feature-490.md](feature-490.md) - Assert.IsType fix (predecessor, identified technical debt)
- [Era.Core/Types/Result.cs](../../Era.Core/Types/Result.cs) - Result<T> discriminated union

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-14 FL iter1**: [resolved] Phase2-Validate - Tasks section Task#1: AC:Task 1:1 mapping (Task#1 covers AC#1,2,3). Added waiver comment per ENGINE.md Issue 7.
- **2026-01-14 FL iter1**: [resolved] Phase2-Validate - Summary Output path: Forward slash usage is standard for documentation. Added Windows workaround note to AC#1 Details.
- **2026-01-14 FL iter2**: [resolved] Phase2-Validate - AC#6 Grep path: Removed trailing slash for consistency.
- **2026-01-14 FL iter3**: [resolved] Phase2-Validate - xUnit v3 exception type: Changed TrueException to XunitException for compatibility.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-14 | create | opus | Created from F490 technical debt | PROPOSED |
| 2026-01-14 12:37 | START | implementer | Task 1 | - |
| 2026-01-14 12:37 | END | implementer | Task 1 | SUCCESS |
| 2026-01-14 | AC test | ac-tester | Verify all 6 ACs | OK:6/6 |
| 2026-01-14 | FINALIZE | finalizer | Verify completion and update status | READY_TO_COMMIT |
