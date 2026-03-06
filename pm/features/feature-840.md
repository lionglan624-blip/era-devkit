# Feature 840: Engine Test Isolation Failures (GlobalStatic Shared State + Stale Paths)

## Status: [DONE]
<!-- fl-reviewed: 2026-03-06T07:45:00Z -->

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: engine

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)
Engine test suite must be deterministic regardless of execution order. Every test class must own its state setup and not depend on shared fixture state surviving sibling test modifications. SSOT for test isolation patterns is the xUnit collection fixture documentation; scope is the 9 currently-failing tests in uEmuera.Tests.

### Problem (Current Issue)
The engine test suite has 9 PRE-EXISTING test failures when running the full suite, caused by TWO distinct root causes:

1. **VariableDataAccessorTests (4 failures)**: `GlobalStaticFixture` initializes `GlobalStatic.VariableData` once in its constructor (GlobalStaticCollection.cs:35-61), but sibling test classes in the same `[Collection("GlobalStatic")]` — specifically `EngineVariablesImplTests` (33 `Reset()` calls) and `GlobalStaticIntegrationTests` (20+ `Reset()` calls) — invoke `GlobalStatic.Reset()` (GlobalStatic.cs:296) which sets `VariableData = null`. The 4 failing tests (`TrySetIntegerArray_ValidCode`, `TrySetIntegerArray_OutOfBounds`, `TryGetIntegerArray_ValidCode`, `TryGetIntegerArray_OutOfBounds`) dereference `GlobalStatic.VariableData.DataIntegerArray` causing NullReferenceException. Other VariableData tests pass because they handle null gracefully or test null/negative-index paths.

2. **ProcessLevelParallelRunnerTests (5 failures)**: This class has ZERO references to GlobalStatic. The 5 tests reference stale monorepo paths (`dev/tests/configs/unit-test-detection.json` and `repoRoot/Game`) that no longer exist after the 5-repo split. The actual file lives at `C:\Era\devkit\test\configs\unit-test-detection.json`. Tests fail deterministically at `Assert.True(File.Exists(scenarioPath))`.

### Goal (What to Achieve)
Fix all 9 test failures so the full engine test suite passes reliably: (1) add per-test VariableData initialization in VariableDataAccessorTests to survive sibling Reset() calls, and (2) update stale monorepo paths in ProcessLevelParallelRunnerTests to post-split locations using environment variables.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do 9 engine tests fail in full-suite? | Two distinct causes: VariableData nullification (4) and stale paths (5) | ProcessLevelParallelRunnerTests.cs has zero GlobalStatic refs |
| 2 | Why is VariableData null during VariableDataAccessorTests? | Sibling tests call GlobalStatic.Reset() which sets VariableData = null | GlobalStatic.cs:296 — Reset() nullifies VariableData |
| 3 | Why does Reset() affect VariableDataAccessorTests? | GlobalStaticFixture constructor runs once per collection, not per-test | GlobalStaticCollection.cs:35-61 — ctor initializes once |
| 4 | Why do ProcessLevelParallelRunnerTests fail? | Paths reference pre-split monorepo structure (dev/tests/configs/) | ProcessLevelParallelRunnerTests.cs:113,185,220,254,297 |
| 5 | Why (Root)? | Tests were written for monorepo layout and never updated after 5-repo split; VariableData tests assumed fixture state would persist across sibling test execution | xunit.runner.json:4 — parallelizeTestCollections: true |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 9 tests fail in full-suite but pass in isolation | VariableData: fixture state destroyed by sibling Reset(); ProcessLevel: stale monorepo paths |
| Where | VariableDataAccessorTests, ProcessLevelParallelRunnerTests | GlobalStatic.Reset() (GlobalStatic.cs:296), stale path literals (ProcessLevelParallelRunnerTests.cs:113+) |
| Fix | Re-run tests individually | Per-test VariableData init; update paths to post-split locations |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F838 | [DONE] | Parent feature (cross-repo verifier) that created this handoff |
| F833 | [DONE] | Original discovery of engine test isolation failures during IEngineVariables implementation |
| F841 | [WIP] | Sibling handoff from F838 |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Code accessibility | FEASIBLE | All affected files are in engine repo tests directory |
| Fix complexity | FEASIBLE | Both fixes are mechanical: per-test init pattern and path string updates |
| Risk of regression | FEASIBLE | Changes are test-only; no production code modifications |
| Testing approach | FEASIBLE | Full-suite dotnet test verifies all 9 fixes |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Engine test reliability | HIGH | 9 tests will pass in full-suite execution |
| CI pipeline | MEDIUM | Eliminates known false-failure noise in engine test runs |
| Other GlobalStatic collection tests | LOW | Per-test init pattern may reveal latent issues in other tests (out of scope) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Changes target engine repo (C:\Era\engine), not devkit | 5-repo split | Commits go to engine repo |
| xUnit collection fixture ctor runs once per collection | xUnit framework | Cannot rely on per-test fixture re-init for VariableData |
| GlobalStatic.Reset() is destructive (nullifies 10+ fields) | GlobalStatic.cs:289-337 | Tests must be self-sufficient, not depend on fixture survival |
| Cross-repo paths need environment variables | 5-repo architecture | DEVKIT_ROOT env var needed for test config path resolution |
| WSL path compatibility required | CLAUDE.md build pattern | Paths must work under /mnt/c/ mount |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Other GlobalStatic collection tests have latent isolation issues | MEDIUM | MEDIUM | Out of scope; tracked in F843 [DRAFT] via Mandatory Handoff |
| ProcessLevelParallelRunnerTests path fix reveals deeper test logic issues | LOW | MEDIUM | First 2 tests (DLL path tests) currently pass, confirming DLL exists |
| Per-test VariableData init adds overhead | LOW | LOW | VariableData init is lightweight (in-memory arrays only) |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| VariableDataAccessorTests failures | `dotnet test --filter "FullyQualifiedName~VariableDataAccessorTests"` (full-suite) | 4 failures | TrySet/GetIntegerArray Valid/OutOfBounds |
| ProcessLevelParallelRunnerTests failures | `dotnet test --filter "FullyQualifiedName~ProcessLevelParallelRunnerTests"` (full-suite) | 5 failures | Path-dependent tests |
| Total engine test failures | `dotnet test` (full engine test suite) | 9 failures | Combined count |

**Baseline File**: Captured during `/run` Phase 3 execution.

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | All 9 tests must pass in full-suite execution | Feature goal | Verify via `dotnet test` on full engine test project |
| C2 | ProcessLevelParallelRunnerTests root cause is stale paths, NOT GlobalStatic | Investigation (3/3 agreement) | ACs must address path fix, not collection attribute changes |
| C3 | Cross-repo paths need DEVKIT_ROOT env var | 5-repo architecture | Test must resolve paths via environment variable |
| C4 | VariableDataAccessorTests must survive sibling Reset() calls | Collection fixture design | Per-test VariableData initialization required |
| C5 | Existing passing tests must not regress | Backward compatibility | Full-suite verification AC required |
| C6 | WSL path compatibility | CLAUDE.md build pattern | Paths must work under WSL mount |

### Constraint Details

**C1: Full-Suite Pass**
- **Source**: Feature goal — all 9 identified failures must be resolved
- **Verification**: Run `dotnet test` on full engine test project, confirm 0 failures
- **AC Impact**: Need dotnet_test AC covering full suite

**C2: Dual Root Cause**
- **Source**: 3/3 investigator agreement that ProcessLevelParallelRunnerTests has zero GlobalStatic references
- **Verification**: Grep ProcessLevelParallelRunnerTests.cs for "GlobalStatic" — zero matches
- **AC Impact**: ACs must have separate verification for path fix (Grep for updated paths) and state fix (per-test init pattern)

**C3: Environment Variable Paths**
- **Source**: 5-repo split moved test configs from `dev/tests/configs/` to `C:\Era\devkit\test\configs/`
- **Verification**: Confirm DEVKIT_ROOT or equivalent env var usage in updated tests
- **AC Impact**: Grep AC for env var usage in ProcessLevelParallelRunnerTests

**C4: Per-Test VariableData Init**
- **Source**: GlobalStaticFixture ctor runs once (GlobalStaticCollection.cs:35-61); Reset() destroys state
- **Verification**: Confirm VariableData initialization in test method or setup, not just fixture
- **AC Impact**: Grep AC for VariableData initialization pattern in VariableDataAccessorTests

**C5: No Regression**
- **Source**: Backward compatibility requirement
- **Verification**: Full-suite dotnet test with 0 failures
- **AC Impact**: dotnet_test AC on full suite (overlaps with C1)

**C6: WSL Compatibility**
- **Source**: CLAUDE.md mandates WSL for dotnet commands
- **Verification**: Run tests via WSL bash pattern
- **AC Impact**: dotnet_test AC must use WSL execution pattern

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F838 | [DONE] | Parent feature (cross-repo verifier) that created this handoff |
| Related | F833 | [DONE] | Original discovery of engine test isolation failures |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block
-->

<!-- fc-phase-2-completed -->

<!-- fc-phase-3-completed -->

---

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Engine test suite must be deterministic regardless of execution order" | All 9 tests pass in full-suite (order-independent) | AC#1, AC#2, AC#9 |
| "Every test class must own its state setup" | VariableDataAccessorTests initializes VariableData per-test, not relying on fixture | AC#4, AC#8 |
| "not depend on shared fixture state surviving sibling test modifications" | Per-test init replaces GlobalStatic.VariableData dependency; paths resolved from env vars not relative traversal | AC#4, AC#5, AC#6, AC#7 |
| "scope is the 9 currently-failing tests" | Stale paths removed, env vars adopted, no unrelated changes | AC#3, AC#5, AC#6, AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | VariableDataAccessorTests pass in full-suite | test | dotnet test uEmuera.Tests --filter "FullyQualifiedName~VariableDataAccessorTests.TrySetIntegerArray_VariableData_ValidCode\|FullyQualifiedName~VariableDataAccessorTests.TrySetIntegerArray_VariableData_OutOfBounds\|FullyQualifiedName~VariableDataAccessorTests.TryGetIntegerArray_VariableData_ValidCode\|FullyQualifiedName~VariableDataAccessorTests.TryGetIntegerArray_VariableData_OutOfBounds" | succeeds | - | [x] |
| 2 | ProcessLevelParallelRunnerTests pass in full-suite | test | dotnet test uEmuera.Tests --filter "FullyQualifiedName~ProcessLevelParallelRunnerTests" | succeeds | - | [x] |
| 3 | Stale monorepo paths removed from ProcessLevelParallelRunnerTests | code | Grep(tests/uEmuera.Tests/Tests/ProcessLevelParallelRunnerTests.cs, pattern="dev.*tests.*configs") | not_matches | `dev.*tests.*configs` | [x] |
| 4 | VariableDataAccessorTests no longer reads GlobalStatic.VariableData | code | Grep(tests/uEmuera.Tests/Tests/VariableDataAccessorTests.cs, pattern="GlobalStatic\.VariableData") | not_matches | `GlobalStatic\.VariableData` | [x] |
| 5 | ProcessLevelParallelRunnerTests uses env var for devkit path | code | Grep(tests/uEmuera.Tests/Tests/ProcessLevelParallelRunnerTests.cs, pattern="DEVKIT_ROOT|Environment.GetEnvironmentVariable") | matches | `DEVKIT_ROOT|Environment.GetEnvironmentVariable` | [x] |
| 6 | ProcessLevelParallelRunnerTests uses env var for game path | code | Grep(tests/uEmuera.Tests/Tests/ProcessLevelParallelRunnerTests.cs, pattern="GAME_PATH") | matches | `GAME_PATH` | [x] |
| 7 | All gamePath assignments use GetGamePath() helper | code | Grep(tests/uEmuera.Tests/Tests/ProcessLevelParallelRunnerTests.cs, pattern="GetGamePath\\(\\)") | gte | 5 | [x] |
| 8 | Per-test VariableData initialization via helper in all VariableData test methods | code | Grep(tests/uEmuera.Tests/Tests/VariableDataAccessorTests.cs, pattern="CreateTestVariableData") | gte | 6 | [x] |
| 9 | Full engine test suite passes with 0 failures | test | dotnet test uEmuera.Tests | succeeds | - | [x] |
| 10 | Handoff feature F843 file exists | file | Glob(pm/features/feature-843.md) | exists | - | [x] |
| 11 | Handoff feature F843 registered in index | code | Grep(pm/index-features.md, pattern="F843") | matches | `F843` | [x] |

### AC Details

**AC#3: Stale monorepo paths removed from ProcessLevelParallelRunnerTests**
- **Test**: `Grep(tests/uEmuera.Tests/Tests/ProcessLevelParallelRunnerTests.cs, pattern="dev.*tests.*configs")`
- **Expected**: No matches (currently 5 occurrences of stale `dev/tests/configs/unit-test-detection.json` path)
- **Rationale**: C2 constraint -- root cause is stale paths from pre-split monorepo. All 5 references must be updated.

**AC#4: VariableDataAccessorTests no longer reads GlobalStatic.VariableData in failing tests**
- **Test**: `Grep(tests/uEmuera.Tests/Tests/VariableDataAccessorTests.cs, pattern="GlobalStatic\.VariableData")`
- **Expected**: No matches (currently 6 occurrences). All VariableData-using tests must create local instances.
- **Rationale**: C4 constraint -- GlobalStatic.VariableData is nullified by sibling Reset() calls. Per-test local initialization eliminates shared state dependency.

**AC#7: All gamePath assignments use GetGamePath() helper**
- **Test**: `Grep(tests/uEmuera.Tests/Tests/ProcessLevelParallelRunnerTests.cs, pattern="GetGamePath\\(\\)")`
- **Expected**: `>= 5` (1 helper definition + 4 test method call sites + 1 FindHeadlessProjectPath call site = 6 minimum, gte 5 is conservative)
- **Rationale**: C2/C3 constraints -- game path must come from GAME_PATH env var via `GetGamePath()` helper, not computed from monorepo root. All `Path.Combine(repoRoot, "Game")` assignments including FindHeadlessProjectPath() are updated for consistency. Only line 112 `NonExistentGamePath_` (intentional invalid path) is excluded.

**AC#8: Per-test VariableData initialization via helper in all VariableData test methods**
- **Test**: `Grep(tests/uEmuera.Tests/Tests/VariableDataAccessorTests.cs, pattern="CreateTestVariableData")`
- **Expected**: `>= 6` (1 definition of helper method + 6 call sites in all 6 VariableData tests = 7 minimum, gte 6 is conservative)
- **Rationale**: C4 constraint -- Key Decision #1 chose Option A (extract `CreateTestVariableData()` helper). All 6 tests that referenced `GlobalStatic.VariableData` (4 failing + 2 NegativeIndex) are updated to call this helper. Grep for `CreateTestVariableData` verifies the helper exists and all 6 tests use it.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Fix all 9 test failures so full engine test suite passes reliably | AC#1, AC#2, AC#9 |
| 2 | Add per-test VariableData initialization in VariableDataAccessorTests to survive sibling Reset() calls | AC#4, AC#8 |
| 3 | Update stale monorepo paths in ProcessLevelParallelRunnerTests to post-split locations using environment variables | AC#3, AC#5, AC#6, AC#7 |
| 4 | Track latent GlobalStatic collection test isolation issues via handoff | AC#10, AC#11 |

<!-- fc-phase-4-completed -->

---

## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

Two independent mechanical fixes to test-only files in the engine repo (`C:\Era\engine\tests\uEmuera.Tests\Tests\`). No production code changes.

**Fix 1 — VariableDataAccessorTests (6 VariableData tests: 4 failing + 2 NegativeIndex)**

All 6 tests that reference `GlobalStatic.VariableData` (`TrySetIntegerArray_VariableData_ValidCode_ReturnsTrue`, `TrySetIntegerArray_VariableData_OutOfBounds_ReturnsFalse`, `TrySetIntegerArray_VariableData_NegativeIndex_ReturnsFalse`, `TryGetIntegerArray_VariableData_ValidCode_ReturnsTrue`, `TryGetIntegerArray_VariableData_OutOfBounds_ReturnsFalse`, `TryGetIntegerArray_VariableData_NegativeIndex_ReturnsFalse`) each read `GlobalStatic.VariableData` directly. After sibling tests in the same `[Collection("GlobalStatic")]` invoke `GlobalStatic.Reset()`, `VariableData` becomes null. The 4 ValidCode/OutOfBounds tests fail with `NullReferenceException` on `.DataIntegerArray`; the 2 NegativeIndex tests currently pass (TrySet/GetIntegerArray handles null gracefully) but share the same fragile dependency.

The fix is to construct a local `VariableData` instance via a `CreateTestVariableData()` helper in all 6 tests, using the same initialization sequence already present in `GlobalStaticFixture.InitializeGlobalStaticForTests()` (GlobalStaticCollection.cs:38-61) and `CreateTestCharacterData()` (VariableDataAccessorTests.cs:22-42). Each test gets its own `ConstantData` + `GameBase` + `VariableData` trio rather than borrowing from `GlobalStatic`.

Pattern to apply in each of the 6 VariableData tests (replacing `var varData = GlobalStatic.VariableData;`):
```csharp
var configService = GlobalStatic.ConfigServiceInstance;
if (configService.PalamLvDef == null)
    configService.PalamLvDef = new System.Collections.Generic.List<long> { 0, 100, 500, 3000, 10000, 30000, 60000, 100000, 150000, 250000 };
if (configService.ExpLvDef == null)
    configService.ExpLvDef = new System.Collections.Generic.List<long> { 0, 1, 4, 20, 50, 100, 200, 400 };
var constant = new ConstantData();
constant.AllocateDataArrays();
var gameBase = new GameBase();
var varData = new VariableData(gameBase, constant);
```

After this change: `GlobalStatic.VariableData` will no longer appear in any test in this file (AC#4 satisfied), and `CreateTestVariableData` will appear at least 7 times: 1 definition + 6 call sites in all VariableData tests (AC#8 satisfied).

**Fix 2 — ProcessLevelParallelRunnerTests (5 failing tests)**

The 5 tests (`ParallelRunner_InvalidPathReportsError`, `ParallelRunner_OutputContainsErbResult`, `ParallelRunner_StructuredOutputNotNull`, `ParallelRunner_EquivalenceWithSequential`, `ParallelRunner_ConcurrentWorkersNoCorruption`) construct paths using stale monorepo patterns:
- `Path.Combine(repoRoot, "dev", "tests", "configs", "unit-test-detection.json")` — pre-split location
- `Path.Combine(repoRoot, "Game")` — monorepo game path

Replace both with environment variable resolution:
```csharp
// devkit config path
string devkitRoot = Environment.GetEnvironmentVariable("DEVKIT_ROOT")
    ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "devkit"));
string scenarioPath = Path.Combine(devkitRoot, "test", "configs", "unit-test-detection.json");

// game path
string gamePath = Environment.GetEnvironmentVariable("GAME_PATH")
    ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "game"));
```

The fallback paths use `AppContext.BaseDirectory` traversal to reach sibling repo directories when env vars are absent, preserving WSL compatibility. The `DEVKIT_ROOT` and `GAME_PATH` env vars are the canonical cross-repo path mechanism documented in CLAUDE.md.

Note: `FindHeadlessProjectPath()` (line 497) also computes `gamePath = Path.Combine(repoRoot, "Game")` — this is updated to use `GetGamePath()` as well to maintain a consistent path resolution pattern throughout the file and eliminate stale monorepo path debt.

This approach satisfies all 11 ACs: structural grep ACs (3, 4, 5, 6, 7, 8) verify the textual changes; dotnet_test ACs (1, 2, 9) verify the tests pass in full-suite execution.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Replace `GlobalStatic.VariableData` with local `VariableData` construction in all 4 failing test methods; tests will no longer throw NullReferenceException when sibling Reset() runs first |
| 2 | Replace `dev/tests/configs/` with `DEVKIT_ROOT`-based path and `repoRoot/Game` with `GAME_PATH`-based path in the 5 failing integration tests |
| 3 | Replacing `dev/tests/configs/` with env-var-based `test/configs/` eliminates all matches for `dev.*tests.*configs` in ProcessLevelParallelRunnerTests.cs |
| 4 | Replacing `GlobalStatic.VariableData` with local `var varData = new VariableData(...)` in each test eliminates all `GlobalStatic\.VariableData` references in VariableDataAccessorTests.cs |
| 5 | Each of the 5 affected tests will contain `Environment.GetEnvironmentVariable` call for `DEVKIT_ROOT` |
| 6 | Each of the 5 affected tests will contain `Environment.GetEnvironmentVariable` call for `GAME_PATH` |
| 7 | All gamePath assignments (5 test methods + FindHeadlessProjectPath) call `GetGamePath()` helper, providing ≥5 matches for the pattern |
| 8 | 1 `CreateTestVariableData()` helper definition + 6 call sites (one per VariableData test) = 7 total matches for `CreateTestVariableData` pattern (gte 6 satisfied) |
| 9 | Both fixes together enable all 9 previously-failing tests to pass; full-suite run produces 0 failures |
| 10 | Task 4 creates feature-842.md [DRAFT] file; file_exists verifies creation |
| 11 | Task 4 registers F843 in index-features.md; Grep verifies registration |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| VariableData fix scope: helper method vs inline per-test | A: Extract `CreateTestVariableData()` helper; B: Inline init in each test | A: Extract helper | DRY — 4 tests repeat identical init. Helper also signals intent clearly. AC#8 `gte 5` is satisfied either way (helper itself counts as 1 occurrence of `new VariableData`) |
| VariableData helper: private instance method vs private static | A: private static; B: private instance | A: private static | Consistent with existing `CreateTestCharacterData()` which is also `private static` |
| Path resolution: env var with fallback vs env var only (no fallback) | A: Env var only (throw if absent); B: Env var with AppContext fallback | B: Env var with fallback | Tests must work in both CI (env vars set) and local-no-config environments. Fallback mirrors existing `repoRoot` traversal pattern in tests |
| Fallback path: AppContext traversal vs relative Directory.GetCurrentDirectory | A: `AppContext.BaseDirectory` traversal; B: `Directory.GetCurrentDirectory()` | A: `AppContext.BaseDirectory` | More reliable — `AppContext.BaseDirectory` is the test assembly dir (fixed), whereas cwd can vary |
| Where to place `DEVKIT_ROOT`/`GAME_PATH` resolution | A: At top of each test method; B: In a shared private helper; C: In a static field | B: Private helper methods `GetDevkitConfigPath()` and `GetGamePath()` | Consolidates env var logic in one place; each test calls the helper once, keeping test bodies readable |

### Interfaces / Data Structures

No new interfaces or data structures. Both fixes are mechanical test code edits:

- `VariableDataAccessorTests.cs`: Add `private static VariableData CreateTestVariableData()` helper + update 4 test methods to call it instead of reading `GlobalStatic.VariableData`
- `ProcessLevelParallelRunnerTests.cs`: Add `private static string GetScenarioPath()` and `private static string GetGamePath()` private helper methods; update 5 test method `Arrange` sections to call them

### Upstream Issues

<!-- No upstream issues found during Technical Design. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 4, 8 | Add `CreateTestVariableData()` private static helper and update all 6 test methods that reference `GlobalStatic.VariableData` (`TrySetIntegerArray_VariableData_ValidCode_ReturnsTrue`, `TrySetIntegerArray_VariableData_OutOfBounds_ReturnsFalse`, `TrySetIntegerArray_VariableData_NegativeIndex_ReturnsFalse`, `TryGetIntegerArray_VariableData_ValidCode_ReturnsTrue`, `TryGetIntegerArray_VariableData_OutOfBounds_ReturnsFalse`, `TryGetIntegerArray_VariableData_NegativeIndex_ReturnsFalse`) in `VariableDataAccessorTests.cs` to call the helper instead of reading `GlobalStatic.VariableData` | | [x] |
| 2 | 2, 3, 5, 6, 7 | Add `GetScenarioPath()` and `GetGamePath()` private static helper methods and update the 5 failing test method Arrange sections + `FindHeadlessProjectPath()` in `ProcessLevelParallelRunnerTests.cs` to resolve paths via `DEVKIT_ROOT` and `GAME_PATH` environment variables with `AppContext.BaseDirectory` fallbacks | | [x] |
| 3 | 9 | Run full engine test suite (`dotnet test uEmuera.Tests`) and verify 0 failures | | [x] |
| 4 | 10, 11 | Create feature-843.md [DRAFT] for latent GlobalStatic collection test isolation issues (Mandatory Handoff) and register in index-features.md | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | `C:\Era\engine\tests\uEmuera.Tests\Tests\VariableDataAccessorTests.cs` | Updated file with `CreateTestVariableData()` helper and 6 fixed test methods (Task 1) |
| 2 | implementer | sonnet | `C:\Era\engine\tests\uEmuera.Tests\Tests\ProcessLevelParallelRunnerTests.cs` | Updated file with `GetScenarioPath()` / `GetGamePath()` helpers, 5 fixed test Arrange sections + FindHeadlessProjectPath, dead var cleanup (Task 2) |
| 3 | tester | sonnet | Engine test project | Full-suite pass verification (Task 3) |

### Pre-conditions

- Engine repo accessible at `C:\Era\engine`
- `DEVKIT_ROOT` env var points to `C:\Era\devkit` (or AppContext fallback resolves correctly)
- `GAME_PATH` env var points to `C:\Era\game` (or AppContext fallback resolves correctly)

### Task 1 — VariableDataAccessorTests.cs

**File**: `C:\Era\engine\tests\uEmuera.Tests\Tests\VariableDataAccessorTests.cs`

1. Add a new `private static VariableData CreateTestVariableData()` helper method following the same pattern as `CreateTestCharacterData()`:
   ```csharp
   private static VariableData CreateTestVariableData()
   {
       var configService = GlobalStatic.ConfigServiceInstance;
       if (configService.PalamLvDef == null)
           configService.PalamLvDef = new System.Collections.Generic.List<long> { 0, 100, 500, 3000, 10000, 30000, 60000, 100000, 150000, 250000 };
       if (configService.ExpLvDef == null)
           configService.ExpLvDef = new System.Collections.Generic.List<long> { 0, 1, 4, 20, 50, 100, 200, 400 };
       var constant = new ConstantData();
       constant.AllocateDataArrays();
       var gameBase = new GameBase();
       return new VariableData(gameBase, constant);
   }
   ```
2. In all 6 test methods that reference `GlobalStatic.VariableData` (4 failing: TrySet/GetIntegerArray ValidCode/OutOfBounds + 2 non-failing: TrySet/GetIntegerArray NegativeIndex), replace the line `var varData = GlobalStatic.VariableData;` with `var varData = CreateTestVariableData();`
3. Verify: `Grep VariableDataAccessorTests.cs "GlobalStatic\.VariableData"` → 0 matches
4. Verify: `Grep VariableDataAccessorTests.cs "CreateTestVariableData"` → ≥ 6 matches

### Task 2 — ProcessLevelParallelRunnerTests.cs

**File**: `C:\Era\engine\tests\uEmuera.Tests\Tests\ProcessLevelParallelRunnerTests.cs`

1. Add two private static path helper methods near the top of the class (after existing field declarations):
   ```csharp
   private static string GetScenarioPath()
   {
       string devkitRoot = Environment.GetEnvironmentVariable("DEVKIT_ROOT")
           ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "devkit"));
       return Path.Combine(devkitRoot, "test", "configs", "unit-test-detection.json");
   }

   private static string GetGamePath()
   {
       return Environment.GetEnvironmentVariable("GAME_PATH")
           ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "game"));
   }
   ```
2. In each of the 5 failing test Arrange sections, replace:
   - `Path.Combine(repoRoot, "dev", "tests", "configs", "unit-test-detection.json")` → `GetScenarioPath()`
   - `Path.Combine(repoRoot, "Game")` → `GetGamePath()`
3. In `FindHeadlessProjectPath()` (line ~497), replace `Path.Combine(repoRoot, "Game")` → `GetGamePath()`
4. Remove dead `repoRoot` declarations: In the 4 test methods where `repoRoot` is no longer used after helper replacement (`ParallelRunner_OutputContainsErbResult`, `ParallelRunner_StructuredOutputNotNull`, `ParallelRunner_EquivalenceWithSequential`, `ParallelRunner_ConcurrentWorkersNoCorruption`), delete the `string repoRoot = ...` line. In `FindHeadlessProjectPath()`, delete both `string testBaseDir = ...` and `string repoRoot = ...` lines. Note: `ParallelRunner_InvalidPathReportsError` retains `repoRoot` (still used for `invalidPath`).
5. Verify: `Grep ProcessLevelParallelRunnerTests.cs "dev.*tests.*configs"` → 0 matches
6. Verify: `Grep ProcessLevelParallelRunnerTests.cs "DEVKIT_ROOT"` → ≥ 1 match
7. Verify: `Grep ProcessLevelParallelRunnerTests.cs "GAME_PATH"` → ≥ 1 match

### Task 3 — Full-Suite Verification

Run via WSL:
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/engine && DEVKIT_ROOT=/mnt/c/Era/devkit GAME_PATH=/mnt/c/Era/game /home/siihe/.dotnet/dotnet test tests/uEmuera.Tests --blame-hang-timeout 10s'
```

**Success criteria**: 0 failures, all 9 previously-failing tests now pass.

### Error Handling

- If `CreateTestVariableData()` throws due to missing `ConfigServiceInstance`: confirm `GlobalStaticFixture` still runs and `ConfigServiceInstance` is initialized (it is not nullified by `Reset()`).
- If ProcessLevelParallelRunnerTests still fail after path fix: check if the `unit-test-detection.json` file exists at `C:\Era\devkit\test\configs\` — run `ls C:\Era\devkit\test\configs\` to confirm.
- On 3 consecutive test failures: STOP → report to user.

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| Other GlobalStatic collection tests have latent isolation issues | Out of scope — F840 fixes 9 specific tests only | New Feature [DRAFT] | F843 | Task 4 | [x] | 作成済み(A) |

<!-- Transferred + Result columns (F811/F805 Lesson):
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: 作成済み(A), 追記済み(B), 記載済み(C), 確認済み(既存)
- Phase 9.4.1 で転記実行・Result記入。Phase 10.0.2 で検証のみ
- Prevents "Destination filled but content never transferred" gap
-->

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK
- Missing Task for Option A → FL FAIL
-->

<!-- DRAFT Creation Checklist (Option A):
When a Task creates a new feature-{ID}.md [DRAFT], it MUST complete ALL of:
1. Create feature-{ID}.md file
2. Register in index-features.md (add row to Active Features table)
3. Update "Next Feature number" in index-features.md
AC for DRAFT creation MUST verify BOTH file existence AND index registration.
-->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-06T08:00 | Phase 1 | initializer | Status [REVIEWED]->[WIP], build check passed | READY |
| 2026-03-06T08:02 | Phase 2 | explorer | Investigation complete, all findings match spec | OK |
| 2026-03-06T08:05 | Phase 3 | orchestrator | RED confirmed: 5 ProcessLevel failures (stale paths), VariableData pass-in-isolation (shared-state issue) | OK |
| 2026-03-06 08:15 | START | implementer | Task 2 | - |
| 2026-03-06 08:17 | END | implementer | Task 2 | SUCCESS |
| 2026-03-06T08:10 | Task 1 | implementer | Add CreateTestVariableData() + replace 6 GlobalStatic.VariableData refs in VariableDataAccessorTests.cs | SUCCESS |
| 2026-03-06T08:20 | Task 3 | orchestrator | Full engine test suite: 0 failures, 619 passed, 4 skipped | SUCCESS |
| 2026-03-06T08:22 | Task 4 | orchestrator | Created F843 [DRAFT] (was F842, ID conflict), registered in index | SUCCESS |
| 2026-03-06T08:22 | DEVIATION | orchestrator | F842 ID conflict — spec referenced F842 but already taken by ac-static-verifier feature | Reallocated to F843 |
| 2026-03-06T08:25 | DEVIATION | ac-tester | AC#4 FAIL: doc comment contained GlobalStatic.VariableData text | Debug iter 1 |
| 2026-03-06T08:26 | Phase 7 | orchestrator | Fixed doc comment, AC#4 now PASS. All 11/11 ACs [x] | OK |
| 2026-03-06T08:30 | DEVIATION | feature-reviewer | NEEDS_REVISION: Links section F843 pointed to feature-842.md | Fixed |
| 2026-03-06T08:30 | Phase 8 | orchestrator | Quality review fix applied; 8.2 skipped (no extensibility); 8.3 N/A | OK |
<!-- run-phase-1-completed -->
<!-- run-phase-2-completed -->
<!-- run-phase-3-completed -->
<!-- run-phase-4-completed -->
<!-- run-phase-7-completed -->
<!-- run-phase-8-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase2-Review iter1: AC#8 vs Key Decision #1 | AC#8 expected value inconsistent with helper approach — changed pattern from `new VariableData` gte 5 to `CreateTestVariableData` gte 4
- [fix] Phase2-Review iter2: AC#7 vs Technical Design | AC#7 pattern `repoRoot.*Game` too broad (matches line 112 NonExistentGamePath_ and line 497 FindHeadlessProjectPath) — changed to `GetGamePath()` gte 4
- [fix] Phase2-Review iter2: Task 1 vs AC#4 | Task 1 scoped to 4 failing tests but AC#4 requires 0 GlobalStatic.VariableData across entire file (6 occurrences) — expanded Task 1 to all 6 tests, updated AC#8 to gte 6
- [fix] Phase3-Maintainability iter3: FindHeadlessProjectPath stale path | FindHeadlessProjectPath() uses same stale repoRoot/Game pattern — expanded Task 2 to include it, updated AC#7 gte 4→5, updated design note
- [fix] Phase3-Maintainability iter4: Dead repoRoot variables | After helper replacement, 4 test methods + FindHeadlessProjectPath have dead repoRoot/testBaseDir vars — added cleanup step to Task 2
- [fix] Phase3-Maintainability iter5: index-features.md SSOT inconsistency | F838 listed in Depends On column but feature spec has it as Related — removed from Depends On in index
- [fix] Phase3-Maintainability iter6: Stale "4 tests" references | Approach Fix 1 heading/body and Implementation Contract still said "4" after Task 1 expanded to 6 — updated all to 6
- [fix] Phase3-Maintainability iter7: Leak Prevention | Risk "latent GlobalStatic issues" had no tracking destination — added Mandatory Handoff to F843 [DRAFT] with creation Task 4
- [fix] Phase2-Review iter8: Task 4 orphan | Task 4 had no ACs — added AC#10 (file_exists) and AC#11 (Grep index for F843) per DRAFT Creation Checklist
- [fix] Phase4-ACValidation iter9: AC#10 invalid type | Changed type from 'artifact' to 'file', method from file_exists to Glob
- [fix] Phase7-FinalRefCheck iter10: Missing F843 link | F843 referenced in body but not in Links section — added forward reference
- [fix] Phase1-RefCheck iter1: Related Features table | F841 status stale [DRAFT] → [WIP]
- [fix] Phase2-Review iter1: Task 2 steps | Duplicate step numbering (two '5.') — renumbered to 5/6/7

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

---

<!-- fc-phase-6-completed -->

## Links

[Related: F838](feature-838.md) - Parent feature (cross-repo verifier path resolution)
[Related: F833](feature-833.md) - Original discovery during IEngineVariables implementation
[Related: F841](feature-841.md) - Sibling handoff from F838
[Related: F843](feature-843.md) - Latent GlobalStatic collection test isolation issues (Mandatory Handoff)
