# Feature 592: Engine Fatal Error Exit Handling

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

## Background

### Philosophy (Mid-term Vision)
Phase 17 Data Migration - Proper error handling for YAML-only configuration ensures automated testing reliability and fail-fast behavior during migration phase. Engine should provide clear feedback to automation systems by exiting with appropriate exit codes when encountering fatal initialization errors.

### Problem (Current Issue)
When engine encounters fatal initialization errors (YAML load failure, missing configuration, etc.), F575 throws InvalidOperationException but this may not result in proper exit codes for automation. Automated testing and CI/CD pipelines require explicit exit codes (not just exceptions) to reliably detect failures during Phase 17 data migration.

### Goal (What to Achieve)
Implement proper fatal error exit handling where engine immediately exits with non-zero exit code (1) upon encountering fatal initialization errors, enabling reliable automated testing during Phase 17 data migration.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Fatal error enum exists | file | Glob | exists | "engine/Assets/Scripts/Emuera/Services/FatalErrorType.cs" | [x] |
| 2 | Fatal error handler interface | code | Grep(engine/Assets/Scripts/Emuera/Services/IFatalErrorHandler.cs) | contains | "public interface IFatalErrorHandler" | [x] |
| 3 | YAML load failure handling | code | Grep(engine/Assets/Scripts/Emuera/) | contains | "FatalErrorType.YamlLoadFailure" | [x] |
| 4 | Configuration missing handling | code | Grep(engine/Assets/Scripts/Emuera/) | contains | "FatalErrorType.ConfigurationMissing" | [x] |
| 5 | Exit code constant defined | code | Grep(engine/Assets/Scripts/Emuera/Services/) | contains | "public const int FATAL_ERROR_EXIT_CODE = 1" | [x] |
| 6 | FatalErrorHandler tests exist | test | Bash | succeeds | "dotnet test engine.Tests --filter FatalErrorHandlingTests" | [x] |
| 7 | Fatal error logging before exit | code | Grep(engine/Assets/Scripts/Emuera/Services/) | contains | "LogFatalErrorAndExit" | [x] |
| 8 | DI registration for handler | code | Grep(engine/Assets/Scripts/Emuera/GlobalStatic.cs) | contains | "public static IFatalErrorHandler FatalErrorHandler" | [x] |
| 9 | No TODO markers in FatalErrorType | code | Grep(engine/Assets/Scripts/Emuera/Services/FatalErrorType.cs) | not_contains | "TODO" | [x] |
| 10 | No FIXME markers in FatalErrorHandler | code | Grep(engine/Assets/Scripts/Emuera/Services/FatalErrorHandler.cs) | not_contains | "FIXME" | [x] |
| 11 | No HACK markers in IFatalErrorHandler | code | Grep(engine/Assets/Scripts/Emuera/Services/IFatalErrorHandler.cs) | not_contains | "HACK" | [x] |
| 12 | GlobalStatic Reset integration | code | Grep(engine/Assets/Scripts/Emuera/GlobalStatic.cs) | contains | "_fatalErrorHandler = new FatalErrorHandler()" | [x] |
| 13 | ProcessInitializer integration | code | Grep(engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs) | contains | "FatalErrorHandler.HandleFatalError" | [x] |
| 14 | IEnvironmentExitHandler interface exists | file | Glob | exists | "engine/Assets/Scripts/Emuera/Services/IEnvironmentExitHandler.cs" | [x] |
| 15 | EnvironmentExitHandler implementation exists | file | Glob | exists | "engine/Assets/Scripts/Emuera/Services/EnvironmentExitHandler.cs" | [x] |
| 16 | Exit code verification via external process | process | Bash | equals | "1" | [x] |

### AC Details

**AC#1**: Fatal error type enumeration
- Test: Glob pattern="engine/Assets/Scripts/Emuera/Services/FatalErrorType.cs"
- Expected: File exists with enumeration of fatal error types

**AC#2**: Fatal error handler interface definition
- Test: Grep pattern="public interface IFatalErrorHandler" path="engine/Assets/Scripts/Emuera/Services/"
- Expected: Interface with HandleFatalError method

**AC#3**: YAML load failure error type
- Test: Grep pattern="FatalErrorType.YamlLoadFailure" path="engine/Assets/Scripts/Emuera/"
- Expected: Enumeration value for YAML loading failures

**AC#4**: Configuration missing error type
- Test: Grep pattern="FatalErrorType.ConfigurationMissing" path="engine/Assets/Scripts/Emuera/"
- Expected: Enumeration value for missing configuration files

**AC#5**: Exit code constant definition
- Test: Grep pattern="public const int FATAL_ERROR_EXIT_CODE = 1" path="engine/Assets/Scripts/Emuera/Services/FatalErrorType.cs"
- Expected: Standard exit code constant for fatal errors

**AC#6**: Headless mode fatal error tests
- Test: dotnet test --filter FatalErrorHandlingTests
- Expected: Tests verify headless mode exits with code 1 on fatal errors

**AC#7**: Fatal error logging implementation
- Test: Grep pattern="LogFatalErrorAndExit" path="engine/Assets/Scripts/Emuera/"
- Expected: Method that logs error message and calls Environment.Exit(1)

**AC#8**: DI registration for handler
- Test: Grep pattern="public static IFatalErrorHandler FatalErrorHandler" path="engine/Assets/Scripts/Emuera/GlobalStatic.cs"
- Expected: Static property for dependency injection of fatal error handler

**AC#9**: No TODO markers in new files
- Test: Grep pattern="TODO" path="engine/Assets/Scripts/Emuera/Services/FatalError*.cs"
- Expected: 0 matches in new fatal error handling files

**AC#10**: No FIXME markers in new files
- Test: Grep pattern="FIXME" path="engine/Assets/Scripts/Emuera/Services/FatalError*.cs"
- Expected: 0 matches in new fatal error handling files

**AC#11**: No HACK markers in new files
- Test: Grep pattern="HACK" path="engine/Assets/Scripts/Emuera/Services/FatalError*.cs"
- Expected: 0 matches in new fatal error handling files

**AC#12**: GlobalStatic Reset integration
- Test: Grep pattern="_fatalErrorHandler = new FatalErrorHandler()" path="engine/Assets/Scripts/Emuera/GlobalStatic.cs"
- Expected: Reset method reinitializes fatal error handler

**AC#13**: ProcessInitializer integration
- Test: Grep pattern="FatalErrorHandler.HandleFatalError" path="engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs"
- Expected: ProcessInitializer uses FatalErrorHandler for fatal errors instead of InvalidOperationException

**AC#14**: IEnvironmentExitHandler interface exists
- Test: Glob pattern="engine/Assets/Scripts/Emuera/Services/IEnvironmentExitHandler.cs"
- Expected: Interface file exists for dependency injection

**AC#15**: EnvironmentExitHandler implementation exists
- Test: Glob pattern="engine/Assets/Scripts/Emuera/Services/EnvironmentExitHandler.cs"
- Expected: Default implementation file exists for production use

**AC#16**: Exit code verification via external process
- Test: Bash command that creates test scenario and verifies exit code
- Expected: External process receives exit code 1 on fatal error

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,3,4,5 | Create fatal error types and constants | [x] |
<!-- **Batch waiver (Task 1)**: AC#1,3,4,5 all define fatal error types and constants in same file FatalErrorType.cs -->
| 2 | 2,7 | Implement fatal error handler interface and service | [x] |
<!-- **Batch waiver (Task 2)**: AC#2,7 both relate to handler interface and logging method in same service -->
| 3 | 8 | Register fatal error handler in DI container | [x] |
| 4 | 9,10,11 | Verify zero technical debt in new files | [x] |
<!-- **Batch waiver (Task 4)**: AC#9,10,11 all verify zero technical debt markers in same directory, verification is atomic -->
| 5 | 6 | Create fatal error handling tests | [x] |
| 6 | 12 | Verify GlobalStatic Reset integration | [x] |
| 7 | 13 | Integrate FatalErrorHandler into ProcessInitializer | [x] |
| 8 | 14,15 | Create Environment Exit Handler interfaces | [x] |
| 9 | 16 | Verify external process exit code | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Fatal Error Types

Create enumeration in `engine/Assets/Scripts/Emuera/Services/FatalErrorType.cs`:

```csharp
using System;

namespace MinorShift.Emuera.Services;

/// <summary>Types of fatal errors that require immediate application exit</summary>
public enum FatalErrorType
{
    /// <summary>YAML configuration file failed to load</summary>
    YamlLoadFailure,

    /// <summary>Required configuration files are missing</summary>
    ConfigurationMissing,

    /// <summary>Critical system initialization failed</summary>
    InitializationFailure
}

/// <summary>Fatal error exit codes</summary>
public static class FatalErrorExitCodes
{
    /// <summary>Standard exit code for fatal errors</summary>
    public const int FATAL_ERROR_EXIT_CODE = 1;
}
```

### Fatal Error Handler Interface

Create interface in `engine/Assets/Scripts/Emuera/Services/IFatalErrorHandler.cs`:

```csharp
using System;

namespace MinorShift.Emuera.Services;

/// <summary>Handles fatal errors that require immediate application exit</summary>
public interface IFatalErrorHandler
{
    /// <summary>Log fatal error and exit application with appropriate exit code</summary>
    /// <param name="errorType">Type of fatal error</param>
    /// <param name="message">Error message</param>
    /// <param name="exception">Optional exception details</param>
    void HandleFatalError(FatalErrorType errorType, string message, Exception? exception = null);
}
```

### Fatal Error Handler Implementation

Create service in `engine/Assets/Scripts/Emuera/Services/FatalErrorHandler.cs`:

```csharp
using System;

namespace MinorShift.Emuera.Services;

public class FatalErrorHandler : IFatalErrorHandler
{
    private readonly IEnvironmentExitHandler _exitHandler;

    // Default constructor for production use
    public FatalErrorHandler() : this(new EnvironmentExitHandler()) { }

    // Constructor for dependency injection (testing)
    public FatalErrorHandler(IEnvironmentExitHandler exitHandler)
    {
        _exitHandler = exitHandler;
    }

    public void HandleFatalError(FatalErrorType errorType, string message, Exception? exception = null)
    {
        LogFatalErrorAndExit(errorType, message, exception);
    }

    private void LogFatalErrorAndExit(FatalErrorType errorType, string message, Exception? exception)
    {
        Console.Error.WriteLine($"FATAL ERROR [{errorType}]: {message}");
        if (exception != null)
        {
            Console.Error.WriteLine($"Exception: {exception}");
        }

        _exitHandler.Exit(FatalErrorExitCodes.FATAL_ERROR_EXIT_CODE);
    }
}
```

### Environment Exit Handler Interface

Create interface in `engine/Assets/Scripts/Emuera/Services/IEnvironmentExitHandler.cs`:

```csharp
using System;

namespace MinorShift.Emuera.Services;

/// <summary>Interface for dependency injection of Environment.Exit behavior</summary>
public interface IEnvironmentExitHandler
{
    /// <summary>Exits the application with specified exit code</summary>
    void Exit(int exitCode);
}
```

Create implementation in `engine/Assets/Scripts/Emuera/Services/EnvironmentExitHandler.cs`:

```csharp
using System;

namespace MinorShift.Emuera.Services;

/// <summary>Default implementation for production use</summary>
public class EnvironmentExitHandler : IEnvironmentExitHandler
{
    /// <summary>Exits the application using Environment.Exit</summary>
    public void Exit(int exitCode) => Environment.Exit(exitCode);
}
```

### DI Registration

Add to GlobalStatic.cs:

```csharp
private static IFatalErrorHandler _fatalErrorHandler = new FatalErrorHandler();
public static IFatalErrorHandler FatalErrorHandler
{
    get => _fatalErrorHandler;
    set => _fatalErrorHandler = value ?? new FatalErrorHandler();
}
```

**Update GlobalStatic.Reset() method**:
*Note: This adds a new line to the existing Reset() method*
```csharp
public static void Reset()
{
    // ... existing reset code ...
    _fatalErrorHandler = new FatalErrorHandler(); // ADD this line - consistent with other service resets
}
```

### Error Handling Integration

Replace existing error handling patterns that wait for user input with fatal error handler calls.

**Current State**: F575 implementation throws InvalidOperationException on YAML load failure. The exceptions propagate up but may not result in proper exit codes for automation.

**Required Changes**:
1. Update ProcessInitializer.cs LoadGameBase() method (line 147)
   - Replace `throw new System.InvalidOperationException(...)` with `GlobalStatic.FatalErrorHandler.HandleFatalError(FatalErrorType.YamlLoadFailure, ...)`
2. Update ProcessInitializer.cs LoadConstantData() method (line 177)
   - Replace `throw new System.InvalidOperationException(...)` with `GlobalStatic.FatalErrorHandler.HandleFatalError(FatalErrorType.ConfigurationMissing, ...)`

**Files to modify**:
- engine/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs (lines 147, 177)

```csharp
// OLD: Direct exception throw (ProcessInitializer lines 147, 177)
throw new System.InvalidOperationException("GameBase YAML loading failed");

// NEW: Fatal error handler call with exit codes
GlobalStatic.FatalErrorHandler.HandleFatalError(FatalErrorType.YamlLoadFailure, "GameBase YAML loading failed", null);
```

### Test Implementation

Create test file at `engine.Tests/Tests/FatalErrorHandlingTests.cs`:

*Note: Tests use IEnvironmentExitHandler interface to avoid actual process termination*

```csharp
using Xunit;
using MinorShift.Emuera.Services;

namespace MinorShift.Emuera.Tests
{
    public class FatalErrorHandlingTests
    {
        private MockEnvironmentExitHandler mockExitHandler;
        private FatalErrorHandler handler;

        public FatalErrorHandlingTests()
        {
            mockExitHandler = new MockEnvironmentExitHandler();
            handler = new FatalErrorHandler(mockExitHandler);
        }

        [Fact]
        public void HandleFatalError_YamlLoadFailure_CallsExitWithCode1()
        {
            // Test that YamlLoadFailure triggers Exit(1) via mock
            handler.HandleFatalError(FatalErrorType.YamlLoadFailure, "Test error", null);
            Assert.Equal(1, mockExitHandler.LastExitCode);
            Assert.True(mockExitHandler.ExitCalled);
        }

        [Fact]
        public void HandleFatalError_ConfigurationMissing_CallsExitWithCode1()
        {
            // Test that ConfigurationMissing triggers Exit(1) via mock
            handler.HandleFatalError(FatalErrorType.ConfigurationMissing, "Test error", null);
            Assert.Equal(1, mockExitHandler.LastExitCode);
            Assert.True(mockExitHandler.ExitCalled);
        }

        [Fact]
        public void LogFatalErrorAndExit_LogsMessageBeforeExit()
        {
            // Test that error is logged before Exit() call
            // Verify logging occurs, then Exit() called
        }
    }

    // Manual mock implementation (no external dependencies)
    public class MockEnvironmentExitHandler : IEnvironmentExitHandler
    {
        public bool ExitCalled { get; private set; }
        public int LastExitCode { get; private set; }

        public void Exit(int exitCode)
        {
            ExitCalled = true;
            LastExitCode = exitCode;
        }
    }

    // IEnvironmentExitHandler is created in Services/ directory per implementation contract
}
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F575 [DONE] | Era.Core YAML configuration setup required |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} → This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This → F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block

Example:
| Predecessor | F540 | [DONE] | Era.Core setup required |
| Successor | F567 | [BLOCKED] | F567 depends on this feature |
| Related | F100 | [DONE] | Reference implementation |
-->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- [resolved] Phase1-Uncertain iter1: Core directory doesn't exist - resolved, using Services/ structure per Implementation Contract
- [resolved] Phase1-Uncertain iter3: AC#10 path specification complexity - resolved, AC#10 replaced with split ACs for TODO/FIXME/HACK
- [resolved] Phase1-Uncertain iter4: AC#8 regex pattern issue - resolved, AC#8 updated to DI registration check (no regex)
- [resolved] Phase1-Uncertain iter5: AC#10 path array format - resolved, split into individual ACs #9,10,11 with single paths
- [resolved] Phase1-Uncertain iter5: Task#1 maps to 4 ACs - accepted, valid batch waiver per ENGINE.md
- [resolved] Phase1-Pending iter6: F594 tracking destination does not exist - feature-594.md created successfully
- [resolved-applied] Phase2-Maintainability iter10: IEnvironmentExitHandler interface in test file should be in Services namespace - moved to Services, added AC#14,15
- [resolved-applied] Phase2-Maintainability iter10: Test path structure needs verification for AC#6 test filter - updated AC#6 with explicit project path
- [resolved-applied] Phase2-Maintainability iter10: Missing CI/CD integration AC for external process exit code verification - added AC#16
- [resolved-applied] Phase2-Maintainability iter10: GlobalStatic Reset pattern inconsistency - user decided to keep new instance pattern
- [resolved-applied] Phase3-ACValidation iter10: AC#6 Type/Method inconsistency - updated to explicit test project path
- [resolved-applied] Phase3-ACValidation iter10: AC#9-11 use Grep with glob wildcards - changed to specific file paths
- [resolved-applied] Phase3-ACValidation iter10: Missing negative test coverage for engine type feature - addressed by AC#16 process verification
- [resolved-applied] Phase4-Feasibility iter10: AC#6 test command needs explicit project path - updated to engine.Tests
- [resolved-applied] Phase4-Feasibility iter10: IEnvironmentExitHandler interfaces must be in production Services directory - moved with AC#14,15
- [resolved-applied] Phase4-Feasibility iter10: AC#9-11 Grep path does not support glob wildcards - changed to specific files

---

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| GUI mode error handling | GUI applications may need different user experience | Feature | F594 |

<!-- Note: "TBD" removed from example per CLAUDE.md TBD Prohibition -->
<!-- Tracking destination options (CLAUDE.md Deferred Task Protocol):
- Feature: Create new Feature → Destination ID = F{ID}
- Task: Add to existing Feature Tasks → Destination ID = F{ID}#T{N}
- Phase: Add to architecture.md Phase Tasks → Destination ID = Phase {N}
-->

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-22 06:50 | START | implementer | Task 5 | - |
| 2026-01-22 06:50 | END | implementer | Task 5 | SUCCESS |
| 2026-01-22 06:53 | START | implementer | Task 1 | - |
| 2026-01-22 06:53 | END | implementer | Task 1 | SUCCESS |
| 2026-01-22 06:53 | START | implementer | Task 2 | - |
| 2026-01-22 06:53 | END | implementer | Task 2 | SUCCESS |
| 2026-01-22 06:53 | START | implementer | Task 8 | - |
| 2026-01-22 06:53 | END | implementer | Task 8 | SUCCESS |
| 2026-01-22 06:56 | START | implementer | Task 3 | - |
| 2026-01-22 06:56 | END | implementer | Task 3 | SUCCESS |
| 2026-01-22 06:57 | START | implementer | Task 7 | - |
| 2026-01-22 06:57 | END | implementer | Task 7 | SUCCESS |
| 2026-01-22 13:07 | START | implementer | Task 4 | - |
| 2026-01-22 13:07 | END | implementer | Task 4 | SUCCESS |
| 2026-01-22 13:07 | START | implementer | Task 6 | - |
| 2026-01-22 13:07 | END | implementer | Task 6 | SUCCESS |
| 2026-01-22 07:02 | START | implementer | Task 9 | - |
| 2026-01-22 07:02 | END | implementer | Task 9 | SUCCESS |
| 2026-01-22 13:17 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION: engine-dev SKILL.md missing FatalErrorHandler |

## Links
- [index-features.md](index-features.md)
- [F575: Era.Core YAML Data Loading](feature-575.md)
- [F594: GUI Mode Error Handling](feature-594.md)