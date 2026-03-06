# Feature 021: Process God Object Split Phase 2

## Status: [DONE]

## Overview

Continue splitting the Process god object (3,593 lines) by extracting ProcessInitializer - the initialization/setup responsibilities from Process class.

## Problem

Process.cs remains a god object despite Feature 020 extracting ErrorHandler:
- Still handles initialization, script execution, and system setup
- ProcessInitializer functionality is intertwined with core processing logic
- Difficult to test initialization separately from execution
- Tight coupling makes changes risky

## Goals

1. Extract initialization responsibilities into ProcessInitializer class
2. Maintain backward compatibility with existing Process consumers
3. Enable independent testing of initialization logic
4. Reduce Process.cs complexity (target: ~500-700 lines reduction)

## Acceptance Criteria

- [x] ProcessInitializer class extracted with clear responsibilities
- [x] IProcessInitializer interface created for DI
- [x] Process.cs delegates to ProcessInitializer for init operations
- [x] Build succeeds
- [x] Regression tests pass
- [x] Documentation updated (engine-reference.md)

## Implementation Summary

### Files Created
- `uEmuera/Assets/Scripts/Emuera/Sub/IProcessInitializer.cs` - Interface with 10 methods
- `uEmuera/Assets/Scripts/Emuera/Sub/ProcessInitializer.cs` - Default implementation (~120 lines)

### Files Modified
- `uEmuera/Assets/Scripts/Emuera/GlobalStatic.cs` - Added DI property
- `uEmuera/Assets/Scripts/Emuera/GameProc/Process.cs` - Refactored Initialize() to delegate

### Process.cs Line Reduction
- Before: ~150 lines of inline initialization code
- After: ~90 lines (delegation to initializer with phase comments)
- Net reduction: ~60 lines in Process.cs

## Scope

### In Scope
- Extract initialization logic from Process.cs
- Create IProcessInitializer interface
- Wire up via GlobalStatic DI
- Integration tests via headless mode

### Out of Scope
- ScriptExecutor extraction (Phase 3)
- ErbLoader refactoring (separate feature)
- Changing Process public API signatures

## Effort Estimate

- **Size**: Medium (1 session - less than estimated)
- **Risk**: 🔴 High - Core system component
- **Testability**: ★★★★★

## Links

- [index-features.md](../index-features.md) - Feature tracking
- [feature-020.md](feature-020.md) - Phase 1 (ErrorHandler extraction)
- [engine-reference.md](../reference/engine-reference.md) - Architecture docs
- [WBS-021.md](WBS-021.md) - Work breakdown
