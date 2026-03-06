# Feature 033: TOILET_COUNTER_MESSAGE split

## Status: [DONE]

## Overview

10,953行のTOILET_COUNTER_MESSAGE関数群を複数ファイルに分割する。Phase 5 ERBリファクタリングの3番目の対象。

## Problem

- TOILET_COUNTER_MESSAGE.ERB is 10,953 lines - extremely large single file
- Many kojo (dialogue) dependencies make changes risky
- Difficult to navigate and maintain
- Similar to WC_SexHara_MESSAGE (Feature 032), likely contains state machine complexity

## Goals

1. Split TOILET_COUNTER_MESSAGE into logically cohesive files
2. Maintain all existing functionality
3. Preserve kojo call dependencies
4. Follow patterns established in Feature 031 and 032

## Acceptance Criteria

- [x] TOILET_COUNTER_MESSAGE.ERB split into 5 files (93 functions preserved)
- [x] All kojo dependencies preserved (TRYCALLFORM dynamic dispatch)
- [x] Build succeeds (0 errors, 0 warnings)
- [x] Regression tests pass (headless OK, unit tests: pre-existing xUnit issue)
- [x] File structure documented in WBS-033.md

## Scope

### In Scope
- Analyze TOILET_COUNTER_MESSAGE.ERB structure
- Identify logical split points
- Create new files with split content
- Update any callers if needed

### Out of Scope
- Functional changes to logic
- Performance optimization
- Refactoring kojo files

## Effort Estimate

- **Size**: 10,953 lines (High)
- **Risk**: High (many kojo dependencies)
- **Testability**: ★★★☆☆ (manual + regression scenarios)

## Links

- [index-features.md](index-features.md) - Feature tracking
- [feature-031.md](feature-031.md) - NTR_SEX family refactor (Phase 5 pattern)
- [feature-032.md](feature-032.md) - WC_SexHara_MESSAGE split (similar approach)
