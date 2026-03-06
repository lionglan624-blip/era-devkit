# Feature 840: Engine Test Isolation Failures (GlobalStatic Shared State)

## Status: [DRAFT]

## Type: engine

## Background

### Problem (Current Issue)
Engine test suite has 9 PRE-EXISTING test isolation failures when running the full suite:
- `ProcessLevelParallelRunnerTests`: 5 failures
- `VariableDataAccessorTests`: 4 failures

All tests pass when run in isolation. The root cause is `GlobalStatic` shared state between test collections — tests modify shared static state without cleanup, causing cross-contamination when run in parallel or sequentially within the same process.

### Goal (What to Achieve)
Add proper test isolation for the affected test classes using `[Collection]` attributes or `IDisposable` cleanup patterns to ensure all 9 tests pass reliably in full-suite execution.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F838 | [DONE] | Parent feature that created this handoff |
| Related | F833 | [DONE] | Original discovery of engine test isolation failures |

## Links

[Related: F838](feature-838.md) - Parent feature (cross-repo verifier path resolution)
[Related: F833](feature-833.md) - Original discovery during IEngineVariables implementation
