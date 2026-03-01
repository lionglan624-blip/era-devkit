# Feature 020: Process God Object Split - Phase 1

## Status: [DONE]

## Overview

Split the monolithic Process class (3,593 lines) into focused, single-responsibility classes. **Phase 1** focuses on extracting error handling into an injectable `IProcessErrorHandler` service.

## Problem

The Process class is a "god object" anti-pattern:
- **3,593 lines** across 5 partial class files
- Handles initialization, script execution, error handling, and state management
- Difficult to test individual concerns in isolation
- High coupling makes changes risky
- Violates Single Responsibility Principle

## Goals

### Phase 1 Goals (This Feature)
1. Extract error handling into `IProcessErrorHandler` interface
2. Create `ProcessErrorHandler` implementation
3. Register in GlobalStatic for DI
4. Maintain backward compatibility

### Future Phases
- Phase 2: ProcessInitializer extraction
- Phase 3: Evaluate ScriptExecutor extraction (CommandRegistry already exists)

## Acceptance Criteria

- [x] Analysis of Process class structure complete
- [x] IProcessErrorHandler interface created
- [x] ProcessErrorHandler implementation created
- [x] Process.cs delegates to new service
- [x] GlobalStatic DI property added
- [-] Unit tests for ProcessErrorHandler (deferred - verified via integration test)
- [x] Build succeeds
- [x] Regression tests pass (headless mode)
- [x] No functional changes to game behavior
- [x] Architecture documentation updated

## Scope

### In Scope (Phase 1)
- Extract `handleException()` and `handleExceptionInSystemProc()` to service
- Create `IProcessErrorHandler` interface
- Create `ProcessErrorHandler` implementation
- DI integration via GlobalStatic

### Out of Scope
- ProcessInitializer extraction (Phase 2)
- ScriptExecutor extraction (Phase 3)
- Changes to ERB script behavior
- Performance optimization
- UI changes

## Technical Approach

### Methods to Extract
From Process.cs:
- `handleException(Exception exc, LogicalLine current, bool playSound)` (lines 444-516)
- `handleExceptionInSystemProc(Exception exc, LogicalLine current, bool playSound)` (lines 419-442)
- `printRawLine(ScriptPosition position)` (lines 518-523)
- `getRawTextFormFilewithLine(ScriptPosition position)` (lines 525-542)

### Interface Design
```csharp
internal interface IProcessErrorHandler
{
    void HandleException(Exception exc, LogicalLine current, bool playSound,
        EmueraConsole console, Func<int, LogicalLine> getReturnAddress);
    void HandleExceptionInSystemProc(Exception exc, LogicalLine current,
        bool playSound, EmueraConsole console);
    void PrintRawLine(ScriptPosition position, EmueraConsole console);
    string GetRawTextFromFileWithLine(ScriptPosition position);
}
```

### DI Pattern
Follow existing pattern from Feature 012-019:
```csharp
// In GlobalStatic.cs
private static IProcessErrorHandler _processErrorHandler = new ProcessErrorHandler();
public static IProcessErrorHandler ProcessErrorHandler
{
    get => _processErrorHandler;
    set => _processErrorHandler = value ?? new ProcessErrorHandler();
}
```

## Implementation Summary

### Files Created
- `uEmuera/Assets/Scripts/Emuera/Sub/IProcessErrorHandler.cs` - Interface definition
- `uEmuera/Assets/Scripts/Emuera/Sub/ProcessErrorHandler.cs` - Implementation (~160 lines)

### Files Modified
- `uEmuera/Assets/Scripts/Emuera/GlobalStatic.cs` - Added DI property
- `uEmuera/Assets/Scripts/Emuera/GameProc/Process.cs` - Replaced ~100 lines with 4 delegation methods

### Process.cs Line Reduction
- Before: ~100 lines of error handling code
- After: 4 thin delegation methods (~10 lines)
- Net reduction: ~90 lines in Process.cs

## Effort Estimate

- **Size**: Medium (Phase 1 only: ~100 lines extracted)
- **Risk**: 🟡 Medium (error paths, but isolated from main logic)
- **Testability**: ★★★★☆
- **Sessions**: 1

## Dependencies

- Feature 019 (GlobalStatic → DI Phase 1) - Completed
- Existing DI infrastructure

## Links

- [index-features.md](../index-features.md) - Feature tracking
- [engine-reference.md](../reference/engine-reference.md) - Architecture docs
- [WBS-020.md](WBS-020.md) - Work breakdown
