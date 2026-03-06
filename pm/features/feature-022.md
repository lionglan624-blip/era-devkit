# Feature 022: ErbLoader Refactor

## Status: [DONE]

## Overview

Split the 1,478-line ErbLoader.cs god object into focused, single-responsibility classes for improved testability and maintainability. Target classes: NestValidator, JumpResolver (ErbFileReader kept as orchestrator).

## Problem

ErbLoader.cs handles multiple responsibilities:
1. File reading and parsing
2. Nesting structure validation (IF/ELSE/ENDIF, SELECTCASE, etc.)
3. Jump/label resolution
4. Error reporting during load

This violates SRP and makes unit testing difficult. The class is tightly coupled and hard to modify.

## Goals

1. ~~Extract ErbFileReader - file I/O and line parsing~~ (kept in ErbLoader as orchestrator)
2. Extract NestValidator - control structure validation
3. Extract JumpResolver - label and jump resolution
4. Define interfaces for testability (INestValidator, IJumpResolver, etc.)
5. Maintain backward compatibility with existing callers

## Acceptance Criteria

- [x] ErbLoader.cs split into 3+ focused classes (NestValidator, JumpResolver, ErbLoaderContext)
- [x] Interfaces defined for each extracted class (INestValidator, IJumpResolver, IErbFileReader)
- [x] Existing functionality preserved (headless test passes)
- [x] Build succeeds without errors
- [x] Regression tests pass (72/73, 1 pre-existing failure)
- [x] engine-reference.md updated with new interfaces

## Scope

### In Scope
- ErbLoader.cs refactoring
- Interface extraction
- Unit test enablement
- Documentation updates

### Out of Scope
- ERB parsing logic changes
- New ERB features
- Performance optimization
- Other god object splits

## Effort Estimate

- **Size**: Large (1,478 lines, 3 target classes)
- **Risk**: High (core loading functionality)
- **Testability**: (enables unit tests for ERB loading)
- **Sessions**: 3-4 sessions estimated

## Technical Notes

Current ErbLoader responsibilities to extract:
1. **ErbFileReader**: `loadErbs()`, `loadErbFile()`, encoding detection
2. **NestValidator**: `CheckNest()`, SELECTCASE/IF validation
3. **JumpResolver**: `resolveLabels()`, `setJumpTo()`, GOTO/CALL resolution

## Links

- [index-features.md](../index-features.md) - Feature tracking
- [engine-reference.md](../reference/engine-reference.md) - Architecture docs
