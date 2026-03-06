# Feature 843: GlobalStatic Collection Test Isolation Hardening

## Status: [DONE]
<!-- fl-reviewed: 2026-03-06T09:27:26Z -->

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
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)
The engine test suite is the SSOT for engine correctness. All test classes within a shared xUnit collection must be independently resilient to sibling test execution order, ensuring that no test depends on implicit shared state that another test may destroy.

### Problem (Current Issue)
After F840 fixed 9 specific test failures in VariableDataAccessorTests and ProcessLevelParallelRunnerTests, investigation of all 16 test classes in the `[Collection("GlobalStatic")]` collection confirmed no remaining actively-failing latent issues. However, the structural risk pattern that caused F840's failures persists because `GlobalStatic.Reset()` has dual behavior: it nullifies data fields (VariableData, ConstantData, GameBaseData, Process) but re-creates service instances (ConfigService, FileSystem, CommandDispatcher). This asymmetry means DI-property-based tests are naturally resilient while data-field-based tests are vulnerable. The fixture's `InitializeGlobalStaticForTests()` sets up concrete data fields, but EngineVariablesImplTests (33 Reset() calls at EngineVariablesImplTests.cs:27-511) and GlobalStaticIntegrationTests (22+ Reset() calls) destroy this state as their first action, making the fixture initialization effectively dead code for data fields. Without documented isolation patterns and per-class hardening, any new test class added to the collection that reads data fields will be vulnerable to the same class of failures F840 fixed.

### Goal (What to Achieve)
Harden the GlobalStatic collection test infrastructure by: (1) removing dead fixture initialization code for data fields, (2) refactoring EngineVariablesImplTests to consolidate 33 duplicate Reset() calls, (3) adding try/finally or save/restore patterns to classes with residual isolation risk (InterfaceExtractionTests, CommandDispatcherTests, FileSystemTests), and (4) documenting the Reset() dual-behavior pattern in the fixture to prevent future regression.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why might future tests in the GlobalStatic collection fail non-deterministically? | Because sibling test classes call GlobalStatic.Reset() which nullifies data fields set by the shared fixture | EngineVariablesImplTests.cs:27-511 (33 Reset() calls) |
| 2 | Why does Reset() cause data loss but not service loss? | Because Reset() has dual behavior: it nullifies data fields (VariableData, ConstantData, etc.) but re-creates service instances (ConfigService, FileSystem, etc.) | GlobalStatic.Reset() implementation |
| 3 | Why do test classes call Reset() instead of using fixture state? | Because EngineVariablesImplTests intentionally tests null-VariableData behavior and needs a clean null state per test | EngineVariablesImplTests.cs test design pattern |
| 4 | Why is the fixture initialization not protected against this? | Because GlobalStaticFixture.InitializeGlobalStaticForTests() creates data objects in its constructor but has no mechanism to detect or warn when siblings destroy this state | GlobalStaticCollection.cs fixture constructor |
| 5 | Why (Root)? | Because the isolation contract between fixture initialization and per-test Reset() is undocumented, and no structural pattern (try/finally, save/restore) enforces that tests clean up after themselves | No documentation or enforcement in GlobalStaticCollection.cs |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Future test classes may fail when reading data fields after sibling Reset() | Reset() dual behavior (data nullified, services re-created) is undocumented; fixture init for data fields is dead code |
| Where | Individual test classes in [Collection("GlobalStatic")] | GlobalStaticCollection.cs fixture design + Reset() asymmetry |
| Fix | Add per-test initialization to each new failing class (as F840 did) | Remove dead fixture init, document Reset() contract, harden existing classes with isolation patterns |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F840 | [DONE] | Fixed 9 test failures; established per-test VariableData init pattern; predecessor |
| F835 | [DONE] | IEngineVariables abstract method stubs -- added VariableData delegation |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Code complexity | FEASIBLE | All changes are mechanical refactoring of test code (no production logic) |
| Test safety | FEASIBLE | Changes are test-only in engine repo; no production code affected |
| Scope clarity | FEASIBLE | All 16 classes audited; specific hardening targets identified (3/3 explorers agree) |
| Risk of regression | FEASIBLE | Refactoring existing test patterns; existing tests serve as regression guard |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Test reliability | MEDIUM | Prevents future non-deterministic test failures when new classes join the collection |
| Test maintainability | MEDIUM | Consolidated Reset() calls and documented patterns reduce cognitive load |
| Production code | LOW | No production code changes; test-only modifications |
| Other test collections | LOW | Pattern documentation may inform other shared-state test collections |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Engine repo scope | Cross-repo (C:\Era\engine) | All changes are in engine test project, not devkit |
| xUnit collection semantics | xUnit framework | Classes in same [Collection] run sequentially but share fixture; cannot change to parallel without removing collection |
| Reset() dual behavior is by design | GlobalStatic.Reset() implementation | Cannot change Reset() behavior without affecting all callers; hardening must work around it |
| EngineVariablesImplTests intentionally tests null state | EngineVariablesImplTests.cs test design | Cannot remove Reset() calls; must consolidate or wrap, not eliminate |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Refactoring EngineVariablesImplTests Reset() consolidation breaks test semantics | LOW | MEDIUM | Each test currently calls Reset() as first line; consolidating to setup method preserves same behavior |
| New test classes still skip documentation and create isolation issues | MEDIUM | LOW | Fixture documentation + code comments serve as guardrails; not enforceable at compile time |
| Scope creep into unskipping round-trip tests | LOW | LOW | Explicitly out of scope; track as separate feature if desired |
| GlobalStaticIntegrationTests Reset() consolidation omitted | LOW | LOW | Already partially hardened (uses try/finally in some tests); accepted risk — no further action needed |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Reset() calls in EngineVariablesImplTests | Grep "GlobalStatic.Reset()" in EngineVariablesImplTests.cs | 33 | Target: consolidate to single setup/teardown |
| Test classes in GlobalStatic collection | Grep '[Collection("GlobalStatic")]' in engine test project | 16 | Audit scope |
| All engine tests pass | dotnet test engine.Tests | PASS | Must remain PASS after refactoring |

**Baseline File**: `_out/tmp/baseline-843.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Reset() dual behavior: data fields nullified, service instances re-created | Investigation (3/3 consensus) | ACs must verify documentation of this asymmetry, not attempt to change Reset() behavior |
| C2 | EngineVariablesImplTests intentionally tests null-VariableData scenarios | Investigation (3/3 consensus) | ACs must not require removal of Reset() calls; consolidation or wrapping is the target |
| C3 | InitializeGlobalStaticForTests() is dead code for data fields | Investigation (Explorer 1 primary, 2/3 implicit) | AC should verify removal or deprecation annotation of dead initialization code |
| C4 | InterfaceExtractionTests, CommandDispatcherTests, FileSystemTests have residual isolation risk | Investigation (Explorer 1 enumeration) | ACs should verify try/finally or save/restore patterns added to these classes |
| C5 | All changes are in engine repo test project | Cross-repo constraint | ACs must target engine repo paths, not devkit paths |

### Constraint Details

**C1: Reset() Dual Behavior**
- **Source**: All 3 investigations identified the data-vs-service asymmetry in GlobalStatic.Reset()
- **Verification**: Read GlobalStatic.Reset() implementation; confirm data fields set to null while service properties get new instances
- **AC Impact**: Documentation AC should verify the dual behavior is described in fixture comments

**C2: Intentional Null-State Testing**
- **Source**: EngineVariablesImplTests.cs lines 27-511; tests verify behavior when VariableData is null
- **Verification**: Read test methods that assert null/default behavior after Reset()
- **AC Impact**: Refactoring AC must verify test count and behavior are preserved, not reduced

**C3: Dead Fixture Initialization**
- **Source**: Explorer 1 explicitly identified InitializeGlobalStaticForTests() as dead code; Explorers 2/3 noted fixture state is destroyed by siblings
- **Verification**: Trace fixture constructor calls and verify no test class relies on fixture-initialized data fields surviving past first sibling Reset()
- **AC Impact**: AC should verify the dead initialization code is removed or annotated

**C4: Residual Isolation Risk Classes**
- **Source**: Explorer 1 enumerated specific files; Explorer 3 classified as LOW RISK (self-healing DI)
- **Verification**: Read each class and verify whether Reset() in sibling could affect their test assertions
- **AC Impact**: ACs should verify isolation patterns (try/finally or save/restore) are present in hardened classes

**C5: Engine Repo Scope**
- **Source**: All changes target engine test project at C:\Era\engine
- **Verification**: File paths in ACs must reference engine repo, not devkit
- **AC Impact**: dotnet test commands must target engine test project

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F840 | [DONE] | Fixed 9 specific test failures; established per-test VariableData init pattern |
| Related | F835 | [DONE] | IEngineVariables abstract method stubs; added VariableData delegation used by tests |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block

Example:
| Predecessor | F540 | [DONE] | Era.Core setup required |
| Successor | F567 | [BLOCKED] | F567 depends on this feature |
| Related | F100 | [DONE] | Reference implementation |
-->

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "All test classes within a shared xUnit collection must be independently resilient" | Each hardening-target class must have isolation patterns (try/finally) | AC#4, AC#5, AC#6 |
| "no test depends on implicit shared state that another test may destroy" | Dead fixture initialization (destroyed by siblings) must be removed | AC#1 |
| "no test depends on implicit shared state that another test may destroy" | Duplicate Reset() calls must be consolidated to prevent copy-paste of fragile pattern | AC#2, AC#3 |
| "SSOT for engine correctness" | Reset() dual-behavior must be documented so future authors understand the contract | AC#8 |
| "All test classes within a shared xUnit collection must be independently resilient" | All engine tests must continue to pass after refactoring | AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Dead fixture data-field initialization removed | code | Grep(engine/tests/uEmuera.Tests/Tests/GlobalStaticCollection.cs, pattern="InitializeGlobalStaticForTests") | not_matches | `InitializeGlobalStaticForTests` | [x] |
| 2 | EngineVariablesImplTests Reset() calls consolidated | code | Grep(engine/tests/uEmuera.Tests/Tests/EngineVariablesImplTests.cs, pattern="GlobalStatic\.Reset\(\)") | lte | 2 | [x] |
| 3 | EngineVariablesImplTests test count preserved | code | Grep(engine/tests/uEmuera.Tests/Tests/EngineVariablesImplTests.cs, pattern="\[Fact\]") | gte | 33 | [x] |
| 4 | InterfaceExtractionTests has try/finally isolation | code | Grep(engine/tests/uEmuera.Tests/Tests/InterfaceExtractionTests.cs, pattern="finally") | matches | `finally` | [x] |
| 5 | CommandDispatcherTests has try/finally isolation | code | Grep(engine/tests/uEmuera.Tests/Tests/CommandDispatcherTests.cs, pattern="finally") | matches | `finally` | [x] |
| 6 | FileSystemTests has try/finally isolation | code | Grep(engine/tests/uEmuera.Tests/Tests/FileSystemTests.cs, pattern="finally") | matches | `finally` | [x] |
| 7 | All engine tests pass after refactoring | test | dotnet test engine.Tests --blame-hang-timeout 10s | succeeds | exit code 0 | [x] |
| 8 | Reset() dual-behavior documented in fixture | code | Grep(engine/tests/uEmuera.Tests/Tests/GlobalStaticCollection.cs, pattern="nullif.*data\|re-creat.*service\|data field.*null\|service.*new instance") | matches | `nullif.*data\|re-creat.*service\|data field.*null\|service.*new instance` | [x] |

### AC Details

**AC#2: EngineVariablesImplTests Reset() calls consolidated**
- **Test**: `Grep(engine/tests/uEmuera.Tests/Tests/EngineVariablesImplTests.cs, pattern="GlobalStatic\.Reset\(\)")`
- **Expected**: `lte 2` (1 in constructor/setup + optional 1 in teardown)
- **Derivation**: Baseline is 33 Reset() calls across 33 [Fact] methods (1:1). After consolidation to constructor or IAsyncLifetime.InitializeAsync, per-method calls are removed. lte 2 allows constructor + optional Dispose/teardown. (Constraint C2: calls are consolidated, not removed entirely.)
- **Rationale**: 33 duplicate Reset() calls are copy-paste debt; consolidation prevents future tests from forgetting the call.

**AC#3: EngineVariablesImplTests test count preserved**
- **Test**: `Grep(engine/tests/uEmuera.Tests/Tests/EngineVariablesImplTests.cs, pattern="\[Fact\]")`
- **Expected**: `gte 33` (baseline: 33 test methods)
- **Derivation**: Current count is exactly 33 [Fact] attributes. Refactoring must not delete any tests. gte 33 allows adding new tests but prevents reduction. (Constraint C2: intentional null-state testing must be preserved.)
- **Rationale**: Ensures consolidation refactoring does not accidentally remove test methods.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Removing dead fixture initialization code for data fields | AC#1 |
| 2 | Refactoring EngineVariablesImplTests to consolidate 33 duplicate Reset() calls | AC#2, AC#3 |
| 3 | Adding try/finally or save/restore patterns to InterfaceExtractionTests, CommandDispatcherTests, FileSystemTests | AC#4, AC#5, AC#6 |
| 4 | Documenting the Reset() dual-behavior pattern in the fixture | AC#8 |
| ALL | All engine tests pass after refactoring (regression guard) | AC#7 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The feature performs four mechanical refactoring operations on engine test code, all within `C:\Era\engine\tests\uEmuera.Tests\Tests\`. No production code is modified.

**Operation 1 — Remove dead fixture initialization (AC#1)**
`GlobalStaticFixture.InitializeGlobalStaticForTests()` creates `ConstantData`, `GameBase`, and `VariableData` in the fixture constructor. Because `EngineVariablesImplTests` (33 tests) and `GlobalStaticIntegrationTests` (22+ tests) call `GlobalStatic.Reset()` as their first action, the fixture-initialized data fields are immediately nullified. No test class reads fixture-initialized data fields in a state where they survive. The method body and the call-site in the constructor will be deleted; the constructor retains `Encoding.RegisterProvider()` and the opening `GlobalStatic.Reset()`.

**Operation 2 — Consolidate EngineVariablesImplTests Reset() calls (AC#2, AC#3)**
Currently, each of the 33 `[Fact]` methods begins with `GlobalStatic.Reset()`. The class will implement the xUnit `IDisposable` pattern: a parameterless constructor that calls `GlobalStatic.Reset()` once, and a `Dispose()` method that calls `GlobalStatic.Reset()` once for cleanup. This reduces 33 per-method calls to 2 (constructor + Dispose), satisfying `lte 2`. All 33 `[Fact]` methods are preserved (no deletions), satisfying `gte 33`.

Rationale for constructor + Dispose over `IAsyncLifetime`: xUnit instantiates a new test class instance per `[Fact]`, so a constructor call runs before each test automatically — identical semantics to the current per-method pattern. `IDisposable.Dispose()` runs after each test, providing cleanup. This is the simpler, synchronous approach.

**Operation 3 — Add try/finally isolation to three risk classes (AC#4, AC#5, AC#6)**
The three classes (`InterfaceExtractionTests`, `CommandDispatcherTests`, `FileSystemTests`) contain tests that assign values to `GlobalStatic` data/service properties (e.g., `GameBaseData`, `GameBaseInstance`, `ProcessInstance`, `CommandDispatcher`, `FileSystem`) but rely on inline `// Cleanup GlobalStatic.Reset()` calls which are not exception-safe. The approach wraps the Act + Cleanup sections of affected tests in `try/finally` blocks:

```csharp
// Example pattern for InterfaceExtractionTests
[Fact]
public void GlobalStatic_GameBaseInstance_CanBeOverriddenForTesting()
{
    GlobalStatic.Reset();
    var originalGameBase = new GameBase { ScriptTitle = "Original" };
    GlobalStatic.GameBaseData = originalGameBase;
    var mockGameBase = new MockGameBase { ScriptTitle = "Mock" };

    try
    {
        GlobalStatic.GameBaseInstance = mockGameBase;
        var result = GlobalStatic.GameBaseInstance;

        Assert.Same(mockGameBase, result);
        Assert.Equal("Mock", result.ScriptTitle);
    }
    finally
    {
        GlobalStatic.Reset();
    }
}
```

Only tests that mutate GlobalStatic properties and have a cleanup step need wrapping. Tests that are purely read-only or create local objects (no GlobalStatic mutation) are left unchanged. The `finally` block always calls `GlobalStatic.Reset()`, replacing the unreliable `// Cleanup` comment + bare call.

**Operation 4 — Document Reset() dual-behavior in fixture (AC#8)**
A multi-line XML doc comment is added to `GlobalStaticFixture` (or as an inline block comment within the constructor) explicitly documenting: (a) data fields nullified on Reset() — `VariableData`, `ConstantData`, `GameBaseData`, `Process`; (b) service instances re-created on Reset() — `ConfigService`, `FileSystem`, `CommandDispatcher`; (c) the isolation contract: any test class that sets data fields must do so per-test, not relying on fixture initialization.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Delete `InitializeGlobalStaticForTests()` method body and the `InitializeGlobalStaticForTests()` call-site in the `GlobalStaticFixture` constructor. After change, `Grep(GlobalStaticCollection.cs, "InitializeGlobalStaticForTests")` returns no matches. |
| 2 | Add constructor and `IDisposable.Dispose()` to `EngineVariablesImplTests`, each calling `GlobalStatic.Reset()` once. Remove all 33 per-method `GlobalStatic.Reset()` calls. Result: `Grep(EngineVariablesImplTests.cs, "GlobalStatic\.Reset\(\)")` count = 2 (lte 2). |
| 3 | Preserve all 33 `[Fact]` methods — no deletions. Result: `Grep(EngineVariablesImplTests.cs, "\[Fact\]")` count = 33 (gte 33). |
| 4 | Wrap GlobalStatic-mutating tests in `InterfaceExtractionTests` with `try/finally` blocks. Result: `Grep(InterfaceExtractionTests.cs, "finally")` matches. |
| 5 | Wrap GlobalStatic-mutating tests in `CommandDispatcherTests` with `try/finally` blocks. Result: `Grep(CommandDispatcherTests.cs, "finally")` matches. |
| 6 | Wrap GlobalStatic-mutating tests in `FileSystemTests` with `try/finally` blocks. Result: `Grep(FileSystemTests.cs, "finally")` matches. |
| 7 | Run `dotnet test` against `engine.Tests` — all tests pass (PASS). Refactoring preserves test semantics since constructor Reset() is equivalent to per-method Reset() given xUnit per-instance test class instantiation. |
| 8 | Add documentation comment to `GlobalStaticFixture` containing keywords matching pattern `nullif.*data|re-creat.*service|data field.*null|service.*new instance`. Comment will explicitly state that data fields are nullified and service instances are re-created by `Reset()`. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| EngineVariablesImplTests consolidation mechanism | A: constructor + IDisposable, B: IAsyncLifetime, C: [BeforeAfterTestAttribute] | A: constructor + IDisposable | Synchronous, no async overhead. xUnit creates new instance per [Fact], so constructor semantics match current per-method Reset() exactly. Simpler than IAsyncLifetime. |
| try/finally scope — per-test or class-level | A: per-test try/finally in mutating tests only, B: IDisposable on all three classes | A: per-test try/finally | Surgical — only tests that mutate GlobalStatic need wrapping. Class-level IDisposable would require tracking which state to restore and is heavier for what are largely stateless classes. |
| Dead code handling — delete vs annotate | A: delete InitializeGlobalStaticForTests() entirely, B: keep with [Obsolete] or comment | A: delete entirely | Dead code that actively misleads readers about fixture contract. No external callers. Deleting is clean; annotation leaves confusion. |
| Documentation placement | A: XML doc on GlobalStaticFixture class, B: inline block comment in constructor, C: both | A: XML doc on class + inline in constructor | XML doc is discoverable via IDE hover; inline comment explains the specific constructor flow. Both serve different readers. |

### Interfaces / Data Structures

No new interfaces or data structures are introduced. This feature is a pure test-code refactoring.

The `IDisposable` pattern added to `EngineVariablesImplTests` uses the existing xUnit lifecycle:
- Constructor: called by xUnit before each `[Fact]` — replaces 33 per-method `GlobalStatic.Reset()` calls
- `Dispose()`: called by xUnit after each `[Fact]` — ensures cleanup even on test failure

### Upstream Issues

<!-- No upstream issues discovered. All ACs are well-specified and implementable as designed. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 8 | Remove `InitializeGlobalStaticForTests()` method and call-site from `GlobalStaticFixture`; add XML doc + inline comment documenting Reset() dual-behavior (data fields nullified, service instances re-created) | | [x] |
| 2 | 2, 3 | Refactor `EngineVariablesImplTests` to implement `IDisposable`: add constructor calling `GlobalStatic.Reset()` + `Dispose()` calling `GlobalStatic.Reset()`; remove all 33 per-method `GlobalStatic.Reset()` calls while preserving all 33 `[Fact]` methods | | [x] |
| 3 | 4 | Wrap GlobalStatic-mutating test bodies in `InterfaceExtractionTests` with `try/finally` blocks; `finally` clause calls `GlobalStatic.Reset()` | | [x] |
| 4 | 5 | Wrap GlobalStatic-mutating test bodies in `CommandDispatcherTests` with `try/finally` blocks; `finally` clause calls `GlobalStatic.Reset()` | | [x] |
| 5 | 6 | Wrap GlobalStatic-mutating test bodies in `FileSystemTests` with `try/finally` blocks; `finally` clause calls `GlobalStatic.Reset()` | | [x] |
| 6 | 7 | Run `dotnet test` against `engine.Tests` and confirm all tests pass | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

**When to use `[I]`**:
- AC Expected value cannot be determined before implementation
- Task output depends on investigation or runtime discovery
- Downstream tasks depend on this task's concrete output

**When NOT to use `[I]`**:
- Build/compile tasks (Expected = "succeeds" is always deterministic)
- Tasks with spec-defined outputs (API contracts, schema validation)
- Tasks that verify file existence (deterministic yes/no)
- Tasks where Expected can be calculated from requirements (counts from data, paths from config)
- Standard patterns with known outputs (error messages, status codes)

**Anti-pattern**: Using `[I]` to avoid writing concrete Expected values. `[I]` is for genuine uncertainty, not convenience.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Feature 843 Tasks table + Technical Design | Modified files in `C:\Era\engine\tests\uEmuera.Tests\Tests\` |
| 2 | tester | sonnet | Modified engine test project | `dotnet test` PASS confirmation |

### Pre-conditions

- Engine repo at `C:\Era\engine` is available and `dotnet test` currently passes (all engine tests green)
- All changes target `C:\Era\engine\tests\uEmuera.Tests\Tests\` exclusively — no production code changes

### Execution Order

Execute Tasks in order. Each Task is an independent file edit; no Task depends on the output of a prior Task except Task 6 (which must run after Tasks 1–5).

**Task 1 — GlobalStaticCollection.cs** (`C:\Era\engine\tests\uEmuera.Tests\Tests\GlobalStaticCollection.cs`)
1. Delete the `InitializeGlobalStaticForTests()` method entirely (declaration + body)
2. Delete the `InitializeGlobalStaticForTests()` call from the `GlobalStaticFixture` constructor
3. Add XML doc comment to `GlobalStaticFixture` class documenting Reset() dual behavior:
   - Data fields nullified: `VariableData`, `ConstantData`, `GameBaseData`, `Process`
   - Service instances re-created: `ConfigService`, `FileSystem`, `CommandDispatcher`
   - Isolation contract: any test class that reads data fields must initialize per-test, not rely on fixture state
4. Add inline block comment inside the constructor summarizing the same contract
5. Verify: `Grep(GlobalStaticCollection.cs, "InitializeGlobalStaticForTests")` → no matches

**Task 2 — EngineVariablesImplTests.cs** (`C:\Era\engine\tests\uEmuera.Tests\Tests\EngineVariablesImplTests.cs`)
1. Add `IDisposable` to the class declaration
2. Add a parameterless constructor with single `GlobalStatic.Reset()` call
3. Add `public void Dispose()` with single `GlobalStatic.Reset()` call
4. Remove the `GlobalStatic.Reset()` call from the body of every `[Fact]` method (33 removals)
5. Do NOT delete or modify any `[Fact]` method — preserve all 33 test methods
6. Verify: Reset() call count = 2 (constructor + Dispose); `[Fact]` count ≥ 33

**Task 3 — InterfaceExtractionTests.cs** (`C:\Era\engine\tests\uEmuera.Tests\Tests\InterfaceExtractionTests.cs`)
1. Identify all `[Fact]` methods that mutate `GlobalStatic` properties (e.g., `GameBaseData`, `GameBaseInstance`, `ProcessInstance`)
2. Wrap the mutating section in each such method with `try { ... } finally { GlobalStatic.Reset(); }` — regardless of whether an existing cleanup call is present
3. Read-only tests (no GlobalStatic mutation) are left unchanged
4. Verify: `Grep(InterfaceExtractionTests.cs, "finally")` → matches

**Task 4 — CommandDispatcherTests.cs** (`C:\Era\engine\tests\uEmuera.Tests\Tests\CommandDispatcherTests.cs`)
1. Identify all `[Fact]` methods that mutate `GlobalStatic` properties (e.g., `CommandDispatcher`)
2. Wrap the mutating section in each such method with `try { ... } finally { GlobalStatic.Reset(); }` — regardless of whether an existing cleanup call is present
3. Read-only tests left unchanged
4. Verify: `Grep(CommandDispatcherTests.cs, "finally")` → matches

**Task 5 — FileSystemTests.cs** (`C:\Era\engine\tests\uEmuera.Tests\Tests\FileSystemTests.cs`)
1. Identify all `[Fact]` methods that mutate `GlobalStatic` properties (e.g., `FileSystem`)
2. Wrap the mutating section in each such method with `try { ... } finally { GlobalStatic.Reset(); }` — regardless of whether an existing cleanup call is present
3. Read-only tests left unchanged
4. Verify: `Grep(FileSystemTests.cs, "finally")` → matches

**Task 6 — Test verification**
1. Run: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/engine && /home/siihe/.dotnet/dotnet test tests/uEmuera.Tests/ --blame-hang-timeout 10s'`
2. All tests must PASS — zero failures
3. If any test fails: STOP → report to user with full failure output

### Success Criteria

- AC#1: `Grep(GlobalStaticCollection.cs, "InitializeGlobalStaticForTests")` → no matches
- AC#2: `Grep(EngineVariablesImplTests.cs, "GlobalStatic\.Reset\(\)")` count ≤ 2
- AC#3: `Grep(EngineVariablesImplTests.cs, "\[Fact\]")` count ≥ 33
- AC#4: `Grep(InterfaceExtractionTests.cs, "finally")` → matches
- AC#5: `Grep(CommandDispatcherTests.cs, "finally")` → matches
- AC#6: `Grep(FileSystemTests.cs, "finally")` → matches
- AC#7: `dotnet test engine.Tests` → PASS
- AC#8: `Grep(GlobalStaticCollection.cs, "nullif.*data|re-creat.*service|data field.*null|service.*new instance")` → matches

### Error Handling

| Situation | Action |
|-----------|--------|
| Any test fails after refactoring | STOP → report full failure output to user |
| `InitializeGlobalStaticForTests()` has callers outside fixture | STOP → report caller list to user |
| EngineVariablesImplTests already implements `IDisposable` | Read existing Dispose() and integrate, do not duplicate |
| try/finally pattern breaks test assertion (exception swallowed) | STOP → report affected test name to user |

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

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
| 2026-03-06 | PHASE_START | orchestrator | Phase 1 Initialize | READY:843:engine |
<!-- run-phase-1-completed -->
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 2 Investigation | 5 files analyzed |
<!-- run-phase-2-completed -->
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 3 TDD RED | 6/8 ACs RED confirmed |
<!-- run-phase-3-completed -->
| 2026-03-06 | PHASE_COMPLETE | implementer | Phase 4 Implementation | Tasks 1-5 complete, 619 passed 0 failed |
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Task 6 test verification | 619 passed, 4 skipped, 0 failed |
<!-- run-phase-4-completed -->
| 2026-03-06 | PHASE_COMPLETE | ac-tester | Phase 7 Verification | OK:8/8 ACs PASS |
<!-- run-phase-7-completed -->
| 2026-03-06 | PHASE_COMPLETE | feature-reviewer | Phase 8 Post-Review | READY (spec 5/5, impl clean) |
<!-- run-phase-8-completed -->
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 9 Report & Approval | 8/8 PASS, 0 DEVIATION, user approved |
<!-- run-phase-9-completed -->
| 2026-03-06 | CodeRabbit | 0 findings | - |
| 2026-03-06 | PHASE_COMPLETE | orchestrator | Phase 10 Finalize & Commit | engine:be0ab52, devkit:5494989 |
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase3-Maintainability iter1: Risks table | GlobalStaticIntegrationTests exclusion undocumented — added justification row (already partially hardened with try/finally)
- [fix] Phase3-Maintainability iter1: Implementation Contract Task 1 step 1 | Ambiguous "delete method body" vs "delete method entirely" — aligned with Key Decisions: "delete entirely (declaration + body)"
- [fix] Phase3-Maintainability iter1: Implementation Contract Tasks 3/4/5 step 2 | Instruction only covered tests with existing cleanup — revised to "regardless of whether an existing cleanup call is present"
- [fix] Phase2-Review iter2: Risks table row 4 | Mitigation text referenced "via Mandatory Handoff" but no handoff existed — changed to "accepted risk — no further action needed"
- [fix] Phase4-ACValidation iter3: AC#8 Expected column | Descriptive text "dual-behavior documentation" → actual regex pattern for matches matcher

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

### /imp 843 (2026-03-06)
- [applied] wbs-generator: Key Decisions Selected列とImplementation Contract手順語の整合性チェックルール追加 → `.claude/agents/wbs-generator.md`
- [revised] wbs-generator: Approachスコープ定義条件語（regardless of等）のImplementation Contract転記ルール追加（広範な条件語→スコープ定義条件のみに絞り込み） → `.claude/agents/wbs-generator.md`
- [applied] quality-fixer C34: Risks Mitigation列の"Handoff"参照がMandatory Handoffs行と対応していることを検証するパターン追加 → `.claude/agents/quality-fixer.md`
- [revised] ac-validator: Invalid PatternsテーブルにFmatches/not_matches Expectedはregexパターン必須ルール追加（独立ルール→既存テーブル行として統合） → `.claude/agents/ac-validator.md`
- [rejected] tech-designer Risks除外正当化チェック — Risksテーブルはconsensus-synthesizer生成でありtech-designer管轄外。コンテキスト依存で機械的ルール化困難

---

<!-- fc-phase-6-completed -->
## Links

[Predecessor: F840](feature-840.md) - Established per-test VariableData initialization pattern
[Related: F835](feature-835.md) - IEngineVariables abstract method stubs
