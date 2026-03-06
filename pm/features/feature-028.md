# Feature 028: GlobalStatic DI Batch 2

## Status: [DONE]

## Overview

Continue GlobalStatic -> DI migration focusing on Variable classes. Added DI infrastructure for IdentifierDictionary and VariableData, following the successful Feature 027 pattern.

## Problem

GlobalStatic still has remaining usages after Batch 1 completion. Variable classes depend on GlobalStatic for IdentifierDictionary and VariableData access, making unit testing difficult and creating tight coupling.

## Goals

1. Add DI infrastructure for Variable-related GlobalStatic dependencies
2. Maintain the DI patterns established in Feature 027
3. Preserve backward compatibility
4. Enable unit testing of Variable classes

## Acceptance Criteria

- [x] IIdentifierDictionary and IVariableData interfaces created
- [x] IdentifierDictionaryInstance and VariableDataInstance DI properties added
- [x] Build succeeds
- [x] Existing unit tests pass (85/85)
- [x] Regression tests pass (headless smoke test)
- [x] Documentation updated

## Scope

### In Scope
- IIdentifierDictionary interface (5 usages)
- IVariableData interface (3 usages)
- DI properties in GlobalStatic
- Implementation on existing classes

### Out of Scope
- EMediator (25 usages) - Already passes as method parameter, not direct GlobalStatic access
- Console/MainWindow/ProcessInstance - Already have interfaces (Feature 027, 014)
- Parser/Analyzer classes (Batch 3)

### Scope Adjustment Note
Original estimate was ~40 usages. Actual new interface coverage is 8 usages because:
- EMediator usages (25) are parameter passing, not direct GlobalStatic property access
- Console (3), MainWindow (1), ProcessInstance (1) already have DI from prior features

## Effort Estimate

- **Size**: 8 new usages covered (~0.5 session)
- **Risk**: Low (Adding interfaces only, no behavior change)
- **Testability**: ★★★★★ (Unit tests can verify injection)

## Technical Notes

### New Interfaces
- `IIdentifierDictionary`: GetVariableToken, getVarTokenIsForbid
- `IVariableData`: GetSystemVariableToken

### Files Changed
- `uEmuera/Assets/Scripts/Emuera/Sub/IIdentifierDictionary.cs` (new)
- `uEmuera/Assets/Scripts/Emuera/Sub/IVariableData.cs` (new)
- `uEmuera/Assets/Scripts/Emuera/GlobalStatic.cs`
- `uEmuera/Assets/Scripts/Emuera/GameData/IdentifierDictionary.cs`
- `uEmuera/Assets/Scripts/Emuera/GameData/Variable/VariableData.cs`

## Links

- [WBS-028.md](WBS-028.md) - Work breakdown
- [feature-027.md](feature-027.md) - Previous batch (reference for pattern)
- [index-features.md](../index-features.md) - Feature tracking
- [engine-reference.md](../reference/engine-reference.md) - Architecture overview
