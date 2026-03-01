# Feature 481: InputHandler Input Processing

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

## Created: 2026-01-13

---

## Summary

**Phase 14: Era.Core Engine - InputHandler Implementation**

Implement InputHandler system for processing INPUT/INPUTS commands with validation, request management, and input waiting state.

**Output**:
- `Era.Core/Input/IInputHandler.cs` - Input handler interface
- `Era.Core/Input/InputHandler.cs` - Input command processing
- `Era.Core/Input/InputRequest.cs` - Input request representation (numeric/string)
- `Era.Core/Input/InputValidator.cs` - Input validation logic
- Unit tests in `Era.Core.Tests/Input/InputHandlerTests.cs`

**Volume**: ~300 lines total

---

## Background

### Philosophy (Mid-term Vision)

**Phase 14: Era.Core Engine** - Establish pure C# game engine with headless execution capability. InputHandler forms the input processing foundation, enabling INPUT/INPUTS commands to request user input during game execution.

### Problem (Current Issue)

ERA scripts use INPUT (numeric) and INPUTS (string) commands to request user input, but no C# implementation exists to process these requests. Without InputHandler, the engine cannot:
- Process INPUT/INPUTS commands
- Validate numeric input ranges
- Manage input waiting state
- Queue multiple input requests

### Goal (What to Achieve)

1. **Implement IInputHandler interface** - Input processing abstraction
2. **Implement InputHandler** - Command processing and request management
3. **Implement InputRequest** - Input request value object
4. **Implement InputValidator** - Validation logic for numeric ranges
5. **Register in DI** - ServiceCollectionExtensions integration
6. **Test input processing** - Unit tests for INPUT/INPUTS flow
7. **Eliminate technical debt** - Zero TODO/FIXME/HACK comments

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IInputHandler.cs exists | file | Glob | exists | "Era.Core/Input/IInputHandler.cs" | [x] |
| 2 | InputHandler.cs exists | file | Glob | exists | "Era.Core/Input/InputHandler.cs" | [x] |
| 3 | InputRequest.cs exists | file | Glob | exists | "Era.Core/Input/InputRequest.cs" | [x] |
| 4 | InputValidator.cs exists | file | Glob | exists | "Era.Core/Input/InputValidator.cs" | [x] |
| 5 | IInputHandler interface exists | code | Grep | contains | "public interface IInputHandler" | [x] |
| 6 | InputHandler implements interface | code | Grep | contains | "class InputHandler : IInputHandler" | [x] |
| 7 | DI registration | file | Grep | contains | "AddSingleton.*IInputHandler.*InputHandler" | [x] |
| 8 | INPUT numeric test | test | Bash | succeeds | "dotnet test --filter InputNumeric" | [x] |
| 9 | INPUTS string test | test | Bash | succeeds | "dotnet test --filter InputString" | [x] |
| 10 | Validation test | test | Bash | succeeds | "dotnet test --filter InputValidation" | [x] |
| 11 | Invalid input (Neg) | test | Bash | succeeds | "dotnet test --filter InvalidInput" | [x] |
| 12 | Duplicate request rejected | test | Bash | succeeds | "dotnet test --filter DuplicateRequest" | [x] |
| 13 | Build succeeds | build | Bash | succeeds | "dotnet build Era.Core" | [x] |
| 14 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [x] |

### AC Details

**AC#1**: IInputHandler.cs file existence
- Test: Glob pattern="Era.Core/Input/IInputHandler.cs"
- Expected: File exists
- Verifies: Interface file created

**AC#2**: InputHandler.cs file existence
- Test: Glob pattern="Era.Core/Input/InputHandler.cs"
- Expected: File exists
- Verifies: Primary implementation file created

**AC#3**: InputRequest.cs file existence
- Test: Glob pattern="Era.Core/Input/InputRequest.cs"
- Expected: File exists
- Verifies: Input request value object created

**AC#4**: InputValidator.cs file existence
- Test: Glob pattern="Era.Core/Input/InputValidator.cs"
- Expected: File exists
- Verifies: Validation logic component created

**AC#5**: IInputHandler interface definition
- Test: Grep pattern="public interface IInputHandler" path="Era.Core/Input/IInputHandler.cs"
- Expected: Contains interface definition
- Verifies: Abstraction defined for DI

**AC#6**: InputHandler implementation
- Test: Grep pattern="class InputHandler : IInputHandler" path="Era.Core/Input/InputHandler.cs"
- Expected: Contains implementation declaration
- Verifies: Interface implemented

**AC#7**: DI registration in ServiceCollectionExtensions
- Test: Grep pattern="AddSingleton.*IInputHandler.*InputHandler" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: Contains DI registration
- Verifies: Service registered for injection

**AC#8**: INPUT numeric input handling (behavior test)
- Test: `dotnet test --filter InputNumeric`
- Test cases: Request numeric input, Provide valid number, Verify parsed correctly, Request returns Result.Ok
- Expected: All numeric input tests pass
- Verifies: INPUT command processes numbers correctly

**AC#9**: INPUTS string input handling (behavior test)
- Test: `dotnet test --filter InputString`
- Test cases: Request string input, Provide text, Verify returned as-is, Request returns Result.Ok
- Expected: All string input tests pass
- Verifies: INPUTS command processes strings correctly

**AC#10**: Input validation logic (behavior test)
- Test: `dotnet test --filter InputValidation`
- Test cases: Validate numeric range (min/max), Reject out-of-range, Accept in-range
- Expected: All validation tests pass
- Verifies: Validator correctly checks ranges

**AC#11**: Invalid input error handling (Negative test)
- Test: `dotnet test --filter InvalidInput`
- Test cases: Non-numeric for INPUT returns Result.Fail, Out-of-range returns Result.Fail
- Expected: All invalid input tests pass
- Verifies: Proper error handling for malformed input

**AC#12**: Duplicate request rejection (Negative test)
- Test: `dotnet test --filter DuplicateRequest`
- Test cases: Request input while already waiting returns Result.Fail with message "入力待ち状態で新しい入力要求はできません"
- Expected: All duplicate request tests pass
- Verifies: Single pending request only, reject new requests while waiting

**AC#13**: Build verification
- Test: `dotnet build Era.Core`
- Expected: Build succeeded
- Verifies: No compile errors

**AC#14**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Input/" -i (case-insensitive, regex OR)
- Expected: 0 matches
- Verifies: Clean implementation without deferred work

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4 | Create Input/ directory and component files (IInputHandler, InputHandler, InputRequest, InputValidator) | [x] |
| 2 | 5 | Define IInputHandler interface with RequestInput/ProvideInput methods | [x] |
| 3 | 6 | Implement InputHandler with request queue and state management | [x] |
| 4 | 8 | Implement InputRequest value object (verified via INPUT numeric test) | [x] |
| 5 | 10 | Implement InputValidator with range checking logic (verified via validation test) | [x] |
| 6 | 7 | Register IInputHandler in ServiceCollectionExtensions.cs | [x] |
| 7 | 9,11,12 | Write unit tests (INPUTS, invalid input, duplicate request) | [x] |
| 8 | 13 | Verify build succeeds with all changes | [x] |
| 9 | 14 | Remove all TODO/FIXME/HACK comments from Input/ directory | [x] |

<!-- AC:Task 1:1 Rule: 14 ACs = 9 Tasks (Task#1 covers file existence AC#1-4, behavioral verification via Task#4/AC#8 and Task#5/AC#10) -->
<!-- **Batch file creation waiver (Task 1)**: Following F480 precedent for related component file creation in same namespace. -->
<!-- **Batch test waiver (Task 7)**: Related negative case tests (INPUTS, invalid input, duplicate request) can be implemented together. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Interface Definition

File: `Era.Core/Input/IInputHandler.cs`

```csharp
using Era.Core.Types;

namespace Era.Core.Input;

/// <summary>
/// Input handler for processing INPUT/INPUTS commands with validation.
/// </summary>
public interface IInputHandler
{
    /// <summary>Request numeric input with optional min/max validation</summary>
    /// <param name="prompt">User prompt message</param>
    /// <param name="min">Minimum allowed value (optional)</param>
    /// <param name="max">Maximum allowed value (optional)</param>
    Result<Unit> RequestNumericInput(string prompt, int? min = null, int? max = null);

    /// <summary>Request string input</summary>
    /// <param name="prompt">User prompt message</param>
    Result<Unit> RequestStringInput(string prompt);

    /// <summary>Provide input response to pending request</summary>
    /// <param name="input">User-provided input string</param>
    /// <returns>Result containing: int (boxed) for numeric input, string for string input.
    /// Note: Uses object for simplicity; boxing cost acceptable for game I/O path.</returns>
    Result<object> ProvideInput(string input);

    /// <summary>Check if input request is pending</summary>
    bool IsWaitingForInput { get; }
}
```

### Error Message Format

**Error Messages**: Return `Result.Fail` for validation errors with format:
- Non-numeric input: `"数値の入力が必要です"`
- Out of range (both min and max): `"入力値は{min}～{max}の範囲で指定してください"`
- Out of range (min only): `"入力値は{min}以上で指定してください"`
- Out of range (max only): `"入力値は{max}以下で指定してください"`
- No pending request: `"入力待ち状態ではありません"`
- Duplicate request: `"入力待ち状態で新しい入力要求はできません"`

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// Input Handler (Singleton) - Feature 481
services.AddSingleton<IInputHandler, InputHandler>();
```

Place after Process State section (line 60), following the established feature grouping pattern.

### Implementation Requirements

| Requirement | Specification |
|-------------|---------------|
| Thread safety | NOT required - single-threaded execution per game instance |
| Request queue | Single pending request only - reject new requests while waiting |
| String validation | None - INPUTS accepts any string |
| Numeric parsing | Use `int.TryParse` - reject non-integers |

### ProcessState Integration

InputHandler is self-contained - it does NOT directly depend on IProcessState. The integration happens at GameEngine orchestration level:

- InputHandler provides `IsWaitingForInput` property
- GameEngine queries `IsWaitingForInput` and sets `ProcessState.ExecutionState = ExecutionState.WaitingInput` when true
- This decoupling allows InputHandler to be tested independently while maintaining state consistency at runtime

**Note**: Integration verification is F474 (GameEngine) responsibility. This feature verifies InputHandler in isolation.

### Test Naming Convention

Test methods follow `Test{CommandType}{Scenario}` format:
- `TestInputNumericBasic` - Basic INPUT processing
- `TestInputStringBasic` - Basic INPUTS processing
- `TestInputValidationRange` - Range validation
- `TestInvalidInputReturnsError` - Negative case
- `TestDuplicateRequestRejected` - Duplicate request rejection

This ensures AC filter patterns match correctly (substring matching).

### InputHandler Implementation

File: `Era.Core/Input/InputHandler.cs`

```csharp
using Era.Core.Types;

namespace Era.Core.Input;

/// <summary>
/// Processes INPUT/INPUTS commands, managing request state and validation.
/// </summary>
public sealed class InputHandler : IInputHandler
{
    private InputRequest? _pendingRequest;

    public bool IsWaitingForInput => _pendingRequest is not null;

    public Result<Unit> RequestNumericInput(string prompt, int? min = null, int? max = null)
    {
        if (_pendingRequest is not null)
            return Result<Unit>.Fail("入力待ち状態で新しい入力要求はできません");

        _pendingRequest = InputRequest.Numeric(prompt, min, max);
        return Result<Unit>.Ok(Unit.Value);
    }

    public Result<Unit> RequestStringInput(string prompt)
    {
        if (_pendingRequest is not null)
            return Result<Unit>.Fail("入力待ち状態で新しい入力要求はできません");

        _pendingRequest = InputRequest.String(prompt);
        return Result<Unit>.Ok(Unit.Value);
    }

    public Result<object> ProvideInput(string input)
    {
        if (_pendingRequest is null)
            return Result<object>.Fail("入力待ち状態ではありません");

        var request = _pendingRequest;
        _pendingRequest = null;

        return request.Type switch
        {
            InputType.Numeric => InputValidator.ValidateNumeric(input, request.Min, request.Max)
                .Match(
                    v => Result<object>.Ok(v),
                    e => Result<object>.Fail(e)),
            InputType.String => InputValidator.ValidateString(input)
                .Match(
                    v => Result<object>.Ok(v),
                    e => Result<object>.Fail(e)),
            _ => Result<object>.Fail("不明な入力タイプ")
        };
    }
}
```

### InputRequest Definition

File: `Era.Core/Input/InputRequest.cs`

```csharp
namespace Era.Core.Input;

/// <summary>
/// Value object representing a pending input request.
/// </summary>
public sealed class InputRequest
{
    public InputType Type { get; }
    public string Prompt { get; }
    public int? Min { get; }
    public int? Max { get; }

    private InputRequest(InputType type, string prompt, int? min, int? max)
    {
        Type = type;
        Prompt = prompt;
        Min = min;
        Max = max;
    }

    public static InputRequest Numeric(string prompt, int? min = null, int? max = null)
        => new(InputType.Numeric, prompt, min, max);

    public static InputRequest String(string prompt)
        => new(InputType.String, prompt, null, null);
}

public enum InputType { Numeric, String }
```

### InputValidator Definition

File: `Era.Core/Input/InputValidator.cs`

```csharp
using Era.Core.Types;

namespace Era.Core.Input;

/// <summary>
/// Validates input against request constraints.
/// </summary>
public static class InputValidator
{
    /// <summary>Validate numeric input against range constraints</summary>
    public static Result<int> ValidateNumeric(string input, int? min, int? max)
    {
        if (!int.TryParse(input, out var value))
            return Result<int>.Fail("数値の入力が必要です");

        if (min.HasValue && value < min.Value)
            return Result<int>.Fail(max.HasValue
                ? $"入力値は{min}～{max}の範囲で指定してください"
                : $"入力値は{min}以上で指定してください");

        if (max.HasValue && value > max.Value)
            return Result<int>.Fail(min.HasValue
                ? $"入力値は{min}～{max}の範囲で指定してください"
                : $"入力値は{max}以下で指定してください");

        return Result<int>.Ok(value);
    }

    /// <summary>Validate string input (always succeeds)</summary>
    public static Result<string> ValidateString(string input)
        => Result<string>.Ok(input);
}
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F471 | Phase 14 Planning must complete first |
| Predecessor | F480 | ProcessState defines ExecutionState.WaitingInput (design precedent, not runtime dependency - integration at GameEngine level) |

---

## Links

- [feature-471.md](feature-471.md) - Phase 14 Planning (parent feature)
- [feature-474.md](feature-474.md) - GameEngine (orchestration for ProcessState integration)
- [feature-480.md](feature-480.md) - ProcessState (prerequisite for input waiting state)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 14 Task 8
- [feature-template.md](reference/feature-template.md) - Feature template

---

## 残課題 (Deferred Tasks)

None.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-13 | create | spec-writer | Created from F471 Phase 14 Planning | PROPOSED |
| 2026-01-13 21:23 | START | implementer | Task 1-9: InputHandler implementation | - |
| 2026-01-13 21:23 | END | implementer | Task 1-9: All components created, tests passing (20/20) | SUCCESS |
| 2026-01-13 21:45 | VERIFY | ac-tester | All 14 ACs verified PASS | REVIEWED |
| 2026-01-13 21:50 | DEVIATION | feature-reviewer | Step 7.1 NEEDS_REVISION: engine-dev SKILL.md update missing | Fixed |
| 2026-01-13 21:51 | END | feature-reviewer | Step 7.1/7.2 READY after SSOT fix | SUCCESS |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
