# Feature 032: WC_SexHara_MESSAGE Split

## Status: [DONE]

## Overview

Split the WC_SexHara_MESSAGE.ERB file (8,557 lines) into smaller, maintainable modules. This is the second ERB refactoring task in Phase 5, continuing the pattern established by Feature 031.

## Problem

The WC_SexHara_MESSAGE.ERB file is a monolithic 8,557-line file containing state machine logic for the WC (washroom/toilet) harassment scene messaging. This size makes the file:
- Difficult to understand and navigate
- Hard to test in isolation
- Prone to merge conflicts when multiple changes occur
- Complex due to embedded state machine patterns

## Goals

1. Split WC_SexHara_MESSAGE.ERB into logical, cohesive modules
2. Maintain exact behavioral compatibility (no gameplay changes)
3. Improve code organization following patterns from Feature 031
4. Enable better testing of individual components

## Acceptance Criteria

- [x] WC_SexHara_MESSAGE.ERB split into 3-5 smaller files
- [x] All original functions callable with same signatures
- [x] State machine logic clearly separated
- [x] Build succeeds (`dotnet build`)
- [x] Headless test passes
- [x] Regression tests pass (Unit tests: pre-existing xUnit package issue, unrelated)
- [x] ErbLinter shows no new errors/warnings (SIF false positives only)

## Scope

### In Scope
- Analysis of WC_SexHara_MESSAGE.ERB structure
- Identification of logical split points
- Creation of new ERB files for separated modules
- Update of CALL/JUMP references if needed
- State machine pattern documentation

### Out of Scope
- Behavior changes to messaging logic
- New features or functionality
- Changes to other ERB files beyond reference updates
- Kojo (dialogue) content changes

## Effort Estimate

- **Size**: ~8,557 lines → 3-5 files
- **Risk**: High (state machine complexity)
- **Testability**: ★★★ (headless scenarios can verify message paths)
- **Sessions**: 2-3 sessions

## Technical Considerations

1. **State Machine Pattern**: Need to map state transitions before splitting
2. **Dependencies**: Check calls from other files using `/impact-check`
3. **Message Categories**: Likely split points by message type/phase
4. **Content並行**: ○ (can work alongside content development)

## Links

- [index-features.md](../index-features.md) - Feature tracking
- [feature-031.md](feature-031.md) - Previous ERB refactor (pattern reference)
- [erb-reference.md](../reference/erb-reference.md) - ERB language reference
- [testing-reference.md](../reference/testing-reference.md) - Testing strategy
