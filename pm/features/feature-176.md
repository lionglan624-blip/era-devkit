# Feature 176: Fix parallel execution expect passing bug

## Status: [DONE]

## Type: engine

## Background

### Problem
Feature 170 introduced default parallel execution for directory mode in `--unit` tests. However, when running tests in parallel via `ProcessLevelParallelRunner`, the `expect` field from test scenarios is not being passed to worker subprocesses. This causes all tests to fail immediately (0.8s for 160 tests) because expectations cannot be validated.

### Evidence
```bash
# Directory mode (parallel, default) - FAIL
dotnet run ... --unit tests/ac/kojo/feature-156/
# Result: 0/160 passed, 160 failed (0.80s)

# Directory mode + --sequential - PASS
dotnet run ... --unit tests/ac/kojo/feature-156/ --sequential
# Result: 10/10 passed (240.46s)

# Single file (no parallel) - PASS
dotnet run ... --unit tests/ac/kojo/feature-156/feature-156-K1.json
# Result: 16/16 passed (24.38s)
```

### Root Cause
In `KojoBatchRunner.cs:138-141`:
```csharp
// Feature 170: Default parallel execution for directory mode
if (isDirectoryMode && !options.Sequential && !options.Parallel)
{
    options.Parallel = true;  // Auto-enable parallel mode
}
```

When parallel mode is auto-enabled, `ProcessLevelParallelRunner.Run()` is called. The actual bug is in the parent-worker communication:

1. **Worker subprocess** runs test and validates expectations correctly
2. **Worker outputs JSON** with `status: "pass"` or `status: "fail"` but `BuildResultObject` (KojoTestResult.cs:387-438) does NOT include `expect_results` in output
3. **Parent's `ParseJsonResult`** (ProcessLevelParallelRunner.cs:720-829) only parses `status`, not `output` content
4. **Parent's re-validation** (KojoBatchRunner.cs:412-431) fails because `result.Output` is empty/incomplete

### Goal
Fix the parallel execution pipeline so that `expect` fields are correctly passed to and validated by worker subprocesses.

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Directory parallel test passes | output | --unit dir/ | contains | "160/160 passed" | [x] |
| 2 | Single file still works | output | --unit file.json | contains | "16/16 passed" | [x] |
| 3 | --sequential still works | output | --unit dir/ --sequential | contains | "10/10 passed" | [x] |
| 4 | Build succeeds | build | dotnet build | succeeds | - | [x] |

### AC Details

**AC1 Test**:
```bash
cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-156/
```
**Expected**: `160/160 passed`

**AC2 Test**:
```bash
cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-156/feature-156-K1.json
```
**Expected**: `16/16 passed`

**AC3 Test**:
```bash
cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-156/ --sequential
```
**Expected**: `10/10 passed`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add `expect_results` to BuildResultObject (KojoTestResult.cs:387-438) and ParseJsonResult (ProcessLevelParallelRunner.cs:720-829) | [O] |
| 2 | 2 | Verify single file mode test passes | [O] |
| 3 | 3 | Verify sequential mode test passes | [O] |
| 4 | 4 | Verify build succeeds after changes | [O] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Investigation Notes

**Fix locations confirmed:**
- `engine/Assets/Scripts/Emuera/Headless/KojoTestResult.cs:387-438` - `BuildResultObject()` needs to serialize `expect_results`
- `engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs:720-829` - `ParseJsonResult()` needs to deserialize `expect_results`

**Verified by feasibility-checker:**
- Worker subprocess correctly receives `expect` field via temp JSON
- Build system works (`dotnet build` succeeds)
- Test commands work (single file: 16/16, sequential: 10/10)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2025-12-22 | Initialization | initializer | Status [PROPOSED]→[WIP], verified background/tasks/ACs | READY |
| 2025-12-22 | Investigation | explorer | Analyzed BuildResultObject and ParseJsonResult | READY |
| 2025-12-22 | Implementation | implementer | Added expect_results serialization/deserialization | SUCCESS |
| 2025-12-22 | Bug Fix | opus | Fixed NullReferenceException (JsonIgnore on IsTestSuite) | SUCCESS |
| 2025-12-22 | Bug Fix | opus | Fixed FindHeadlessProjectPath (engine/ path support) | SUCCESS |
| 2025-12-22 | AC Verification | ac-tester | 160/160 passed (parallel), 16/16 (single), 10/10 (sequential), build OK | PASS |
| 2025-12-22 | Regression | regression-tester | 127/127 tests passed (build, C# unit, flow) | PASS |
| 2025-12-22 | Finalization | finalizer | Status [WIP]→[DONE], staged for commit | READY_TO_COMMIT |

---

## Links

- [Feature 170](feature-170.md) - Introduced the bug
- [Feature 064](feature-064.md) - Original ProcessLevelParallelRunner
