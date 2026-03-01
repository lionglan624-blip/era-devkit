# Feature 034: NTR.ERB split

## Status: [DONE]

## Overview

Split NTR.ERB (4,748 lines, 97 functions) into logical modules. This is the final and most complex item in Phase 5 ERB Refactoring, with 61 file dependencies.

## Problem

NTR.ERB is a monolithic file containing multiple unrelated systems:
- Visitor movement/appearance logic
- Room security calculations
- Friendship progression system
- Various NTR action handlers (kiss, pet, sex, etc.)
- Kojo message dispatch functions
- Photo/takeout functions

The mixed responsibilities make the code hard to navigate and maintain. Functions that belong together logically are scattered throughout the file.

## Goals

1. Split NTR.ERB into cohesive modules by functional area
2. Maintain 100% backward compatibility (all 61 dependent files must work unchanged)
3. Improve code navigability and maintainability
4. Apply lessons learned from Feature 031-033

## Acceptance Criteria

- [x] NTR.ERB split into 4-6 logical modules (split into 6 modules)
- [x] All 61 dependent files work without modification
- [x] Build succeeds (`dotnet build`)
- [x] Headless test passes
- [x] Regression tests pass (headless smoke test)
- [x] Documentation updated (WBS-034.md, feature-034.md)

## Scope

### In Scope

Based on function analysis, proposed split:

| New File | Functions | Lines (est.) | Description |
|----------|-----------|--------------|-------------|
| NTR_VISITOR.ERB | VISITER_*, CHK_*, JUDGE_*, GET_* | ~900 | Visitor movement/position |
| NTR_FRIENDSHIP.ERB | *_FRIENDSHIP_* | ~500 | Friendship interaction system |
| NTR_ACTION.ERB | NTR_KISS, NTR_PET, NTR_FELLATIO, NTR_69, NTR_THUG | ~400 | NTR action handlers |
| NTR_SEX.ERB | NTR_SEX*, NTR_A_SEX*, NTR_SEX_COMMON | ~700 | Sex system (already refactored in 031) |
| NTR_KOJO_DISPATCH.ERB | *_MSG_KOJO*, *_KOJO_EX | ~400 | Kojo message dispatch |
| NTR.ERB | NTR_CLEAR_FLG, NTR_HARAM*, NTR_TAKEOUT* | ~200 | Core/utility functions |

### Out of Scope

- Modifying any of the 61 dependent files
- Refactoring function internals
- Changing function signatures
- Adding new functionality

## Technical Notes

### High-Risk Areas

1. **Visitor System**: VISITER_* functions are heavily interconnected
2. **State Machine**: Friendship progression has implicit state dependencies
3. **Kojo Integration**: Message dispatch functions are called from many character files

### Mitigation Strategy

1. Use `/impact-check` for each major function before moving
2. Create comprehensive test scenario covering all NTR paths
3. Split in order of least dependencies first
4. Validate after each file extraction

### Dependency Analysis (61 files)

Key dependent areas:
- `Game/ERB/口上/*/NTR*.ERB` - Character dialogue (20+ files)
- `Game/ERB/訪問者宅拡張/COMF*.ERB` - Visitor house expansion
- `Game/ERB/外出拡張/` - Outing expansion
- `Game/ERB/NTR/` - NTR subsystem files
- Core files: SOURCE.ERB, SYSTEM.ERB, MOVEMENT.ERB

## Effort Estimate

- **Size**: Large (~4,748 lines, 97 functions)
- **Risk**: Very High (61 dependencies, complex state machine)
- **Testability**: ★★☆☆☆ (needs comprehensive scenario)
- **Sessions**: 2-3 sessions (careful incremental approach required)

## Links

- [index-features.md](../index-features.md) - Feature tracking
- [feature-031.md](feature-031.md) - NTR_SEX refactor (practice, lessons learned)
- [feature-032.md](feature-032.md) - WC_SexHara split
- [feature-033.md](feature-033.md) - TOILET_COUNTER split
- [kojo-reference.md](../reference/kojo-reference.md) - Kojo system overview
