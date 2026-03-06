# Feature 843: Latent GlobalStatic Collection Test Isolation Issues

## Status: [DRAFT]

## Type: engine

## Background

### Problem (Current Issue)
The engine test suite uses a shared `[Collection("GlobalStatic")]` xUnit collection with 16+ test classes. F840 fixed 9 specific test failures (4 VariableDataAccessorTests + 5 ProcessLevelParallelRunnerTests), but other test classes in the same collection may have latent isolation issues due to the same shared-state pattern.

Key observations from F840 investigation:
- `EngineVariablesImplTests` has 33 `GlobalStatic.Reset()` calls
- `GlobalStaticIntegrationTests` has 20+ `GlobalStatic.Reset()` calls
- `GlobalStatic.Reset()` nullifies 10+ fields (VariableData, ConstantData, GameBaseData, etc.)
- Any test class in the collection that reads these fields after a sibling class calls Reset() is vulnerable

### Goal (What to Achieve)
Audit all 16+ test classes in the GlobalStatic collection for shared-state dependencies and apply per-test initialization patterns where needed.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F840 | [DONE] | Fixed 9 specific test failures; established per-test init pattern |

## Links

[Predecessor: F840](feature-840.md) - Established per-test VariableData initialization pattern
