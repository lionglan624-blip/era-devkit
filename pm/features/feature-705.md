# Feature 705: Eliminate Build Warnings and Review Test Skips

## Status: [DONE]

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

## Type: infra

## Background

### Philosophy (Mid-term Vision)
Build output should be clean: zero warnings, zero errors. Abnormalities must not become normalized. Every warning is either fixed or explicitly suppressed with documented justification.

### Problem (Current Issue)
`dotnet build` produces ~80 warnings across all projects. `dotnet test` shows 11 skipped tests without clear documentation of intent. The warnings span multiple categories:

| Category | Code | Count | Location |
|----------|------|------:|----------|
| Switch exhaustiveness | CS8524 | 2 | Production code (HeadlessUI, DisplayModeConsumer) |
| Obsolete API usage | CS0618 | ~15 | Production (1) + Test (~14) |
| Platform compatibility | CA1416 | 3 | Production (DashboardService) |
| Unused variable | CS0219 | 1 | Test code |
| Nullable reference | CS8625/CS8600/CS8604/CS8603 | ~20 | Test code |
| xUnit CancellationToken | xUnit1051 | ~30 | Test code |
| xUnit Assert.Single | xUnit2013 | 4 | Test code |
| xUnit blocking task | xUnit1031 | 4 | Test code |
| Test skips | - | 11 | KojoComparer.Tests integration tests |

### Goal (What to Achieve)
1. **Zero warnings** on `dotnet build` for all projects
2. **All test skips documented** with clear rationale
3. Establish foundation for future `TreatWarningsAsErrors` enablement (follow-up feature)

---

## Warning Details

### Production Code (4 files, 6 warnings)

**CS8524 - Switch exhaustiveness (2)**:
- `Era.Core/HeadlessUI.cs:22` - DisplayMode switch covers all 9 values but lacks discard pattern
- `engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs:30` - Same pattern
- Fix: Add `_ => throw new UnreachableException()`

**CS0618 - Obsolete DialogueResult.Lines (1)**:
- `tools/KojoComparer/YamlRunner.cs:31` - Uses deprecated `.Lines` property
- Fix: Replace with `.DialogueLines.Select(dl => dl.Text)`
- Note: `[Obsolete]` defined in `Era.Core/Types/DialogueResult.cs:25`, migration started in F683

**CA1416 - Platform compatibility (3)**:
- `engine/Assets/Scripts/Emuera/Services/DashboardService.cs:38,39,172` - `PerformanceCounter` is Windows-only
- Fix: Add `[SupportedOSPlatform("windows")]` to containing methods
- Note: Code already has null-check fallback for non-Windows

### Test Code (~15 files, ~60 warnings)

**CS0618 - Obsolete usage (~14)**: Test code using deprecated `.Lines` property
- Files: `Era.Core.Tests/Dialogue/DisplayModeTests.cs`, `KojoEngineTests.cs`, `KojoEngineFacadeTests.cs`, `MultiEntrySelectionTests.cs`, `KojoComparer.Tests/YamlRunnerTests.cs`, `PilotEquivalenceTests.cs`, `DisplayModeEquivalenceTests.cs`
- Fix: Migrate to `.DialogueLines`

**CS8625/CS8600/CS8604/CS8603 - Nullable warnings (~20)**: Intentional null-passing in tests
- Files: `InfoStateTests.cs`, `InfoTrainModeDisplayTests.cs`, `OperatorsErrorHandlingTests.cs`, `TypeConverterTests.cs`, `PerformanceMetricsServiceTests.cs`, `StateManagerTests.cs`, `DataFunctionsTests.cs`
- Fix: Use `null!` suppression for intentional null tests

**xUnit1051 - CancellationToken (~30)**: Missing `TestContext.Current.CancellationToken`
- Files: `CommandDispatcherTests.cs`, `DispatchEquivalenceTests.cs`, `PipelineOrderingTests.cs`, `SystemCommandTests.cs`, `ScomfCommandTests.cs`, `BehaviorEquivalenceTests.cs`, `PipelineIntegrationTest.cs`, `InMemoryRepositoryTests.cs`, `UnitOfWorkTests.cs`, `ComDefinitionCacheTests.cs`, `DashboardPerformanceTests.cs`, `SchemaValidationTests.cs`, `ConverterTests.cs`
- Fix: Pass `TestContext.Current.CancellationToken` to async methods

**xUnit2013 - Assert.Single (4)**: `Assert.Equal(1, collection.Count)` should use `Assert.Single`
- File: `PerformanceMetricsServiceTests.cs:351-354`

**xUnit1031 - Blocking task (4)**: `.Result`/`.Wait()` in test methods
- Files: `ComDefinitionCacheTests.cs:374`, `FunctionRegistryTests.cs:263,268`, `InMemoryRepositoryTests.cs:166`
- Fix: Convert to `async`/`await`

**CS0219 - Unused variable (1)**:
- `engine.Tests/Tests/CdflagMappingTests.cs:277` - `defaultValue` assigned but never used
- Fix: Remove or add assertion

### Test Skips (11) - Intentional

All 11 skips are integration tests in KojoComparer.Tests requiring headless mode subprocess execution and game data:
- `ErbRunnerTests` (2): "Requires headless mode subprocess execution"
- `YamlRunnerTests` (2): "Requires headless mode for game data context"
- `PilotEquivalenceTests` (4): "Requires headless mode and game data"
- `DisplayModeEquivalenceTests` (3): "Requires headless mode execution with game data"

**Verdict**: Skips are intentional and properly documented with `Skip` reasons. No action needed.

---

## Out of Scope

- `TreatWarningsAsErrors` enablement (follow-up feature after zero-warning baseline)
- Fixing skipped integration tests (require headless mode infrastructure changes)
- Removing the `[Obsolete]` attribute from `DialogueResult.Lines` (separate migration completion)

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: `dotnet build` produces warnings across all projects and test skips lack documentation review.
2. Why: Warnings accumulated organically as features were added - each feature addressed its own functionality but not cross-cutting code quality concerns.
3. Why: No automated enforcement mechanism (TreatWarningsAsErrors) exists to prevent warning introduction.
4. Why: The project prioritized feature delivery over build hygiene during rapid development phases.
5. Why: Build warnings were treated as informational rather than actionable defects, allowing normalization of abnormal output.

### Symptom vs Root Cause

| Symptom | Root Cause |
|---------|------------|
| ~80+ build warnings in `dotnet build --no-incremental` output | No enforcement mechanism (TreatWarningsAsErrors) and incremental build caching masks warnings in normal workflow |
| Warnings appear to be 0 on incremental builds | MSBuild incremental compilation skips unchanged files, hiding warnings that only appear on clean/non-incremental builds |
| 11 test skips in KojoComparer.Tests | Integration tests require headless mode subprocess - intentional and documented |

### Conclusion

The root cause is the absence of build warning enforcement (TreatWarningsAsErrors) combined with incremental build caching that masks existing warnings during normal development workflow. Developers running `dotnet build` see "0 warnings" because MSBuild caches successful compilation results, only revealing warnings on `--no-incremental` or clean builds. This creates a false sense of cleanliness and allows new warnings to accumulate unnoticed.

**Critical finding**: Standard `dotnet build` shows 0 warnings due to incremental caching. Only `dotnet build --no-incremental` reveals the actual warnings. This must be accounted for in AC verification.

**Additional finding**: CS0618 count is higher than originally estimated. KojoComparer.Tests also has 7 warnings for deprecated `YamlRunner.RenderAsync` method (not just `.Lines`), and `KojoComparer/Program.cs:66` has an additional `.Lines` usage not originally counted in production warnings.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F683 | [DONE] | Introduced [Obsolete] | Introduced `[Obsolete]` on `DialogueResult.Lines` - source of CS0618 warnings |
| F700 | [DONE] | Introduced DisplayMode variants | Added PRINTDATAW/K/D DisplayMode - relevant to CS8524 switch exhaustiveness |
| F688 | [DONE] | Related to DisplayMode | InteractiveRunner DisplayMode JSON Response |

### Pattern Analysis

Warning accumulation follows a predictable pattern: new features introduce code that triggers warnings in existing analyzer rules, but since warnings don't block builds, they accumulate silently. The incremental build caching further hides this accumulation from developers. This is a systemic issue that will recur until TreatWarningsAsErrors is enabled (noted as follow-up feature in Out of Scope).

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | All warning categories have known, mechanical fixes (discard patterns, null!, CancellationToken, async/await) |
| Scope is realistic | YES | ~80+ warnings across ~20 files, all with straightforward fixes |
| No blocking constraints | YES | No predecessor dependencies; F683 (introduced [Obsolete]) is already [DONE] |

**Verdict**: FEASIBLE

All fixes are mechanical and well-understood. No design decisions or architectural changes required. The main risk is regression in test behavior when converting blocking tasks to async, but this is mitigatable with test execution verification.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F683 | [DONE] | DialogueResult.Lines Obsolete Deprecation (introduced the [Obsolete]) |
| Related | F700 | [DONE] | PRINTDATAW/K/D DisplayMode Variants (DisplayMode enum expansion) |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| xunit.analyzers 1.27.0 | Build-time analyzer | Low | xUnit1051/2013/1031 rules bundled via xunit.v3 3.2.2 transitive dependency |
| .NET 10 SDK (10.0.102) | Build toolchain | Low | CA1416 platform compatibility analyzer included by default |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| All .csproj projects | LOW | Warning fix changes are localized to each file; no API changes |
| `DialogueResult.Lines` callers | MEDIUM | CS0618 fixes migrate callers to `.DialogueLines` - functional equivalence must be verified |
| Async test methods | LOW | xUnit1031 fixes convert sync to async - test behavior preserved |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core/HeadlessUI.cs | Update | Add discard pattern to DisplayMode switch (CS8524) |
| engine/.../DisplayModeConsumer.cs | Update | Add discard pattern to DisplayMode switch (CS8524) |
| tools/KojoComparer/YamlRunner.cs | Update | Replace `.Lines` with `.DialogueLines` (CS0618) |
| tools/KojoComparer/Program.cs | Update | Replace `.Lines` with `.DialogueLines` (CS0618) |
| engine/.../DashboardService.cs | Update | Add `[SupportedOSPlatform("windows")]` (CA1416) |
| Era.Core.Tests/ (7+ files) | Update | Migrate `.Lines` to `.DialogueLines` (CS0618) |
| Era.Core.Tests/ (7 files) | Update | Add `null!` suppression (CS8625/CS8600/CS8604/CS8603) |
| Era.Core.Tests/ + tools/*Tests/ (13+ files) | Update | Pass `TestContext.Current.CancellationToken` (xUnit1051) |
| Era.Core.Tests/Monitoring/PerformanceMetricsServiceTests.cs | Update | Replace `Assert.Equal(1,...)` with `Assert.Single` (xUnit2013) |
| Era.Core.Tests/ (3 files) | Update | Convert `.Result`/`.Wait()` to async/await (xUnit1031) |
| engine.Tests/Tests/CdflagMappingTests.cs | Update | Remove unused variable (CS0219) |
| KojoComparer.Tests/ (4 files) | Update | Migrate `RenderAsync` to `Render` and `.Lines` to `.DialogueLines` (CS0618) |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Incremental build caching masks warnings | MSBuild behavior | MEDIUM - ACs must use `--no-incremental` to verify zero warnings |
| `Nullable` disabled in engine project | uEmuera.Headless.csproj | LOW - CS8625 etc. only affect projects with `<Nullable>enable</Nullable>` |
| xUnit.v3 + xunit.analyzers 1.27.0 | Package dependency | LOW - xUnit rules are current and well-supported |
| `DialogueResult.Lines` internal self-reference | Era.Core/Types/DialogueResult.cs:13 | MEDIUM - The `Create()` method internally uses `.Lines` causing CS0618 in Era.Core itself; suppression or refactor needed |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Test behavior change from async conversion | Low | Medium | Run full test suite after xUnit1031 fixes |
| `.DialogueLines` migration breaks test assertions | Low | Low | `.DialogueLines[i].Text` is equivalent to `.Lines[i]` by design |
| New warnings introduced between fix and verification | Low | Low | Use `--no-incremental` for final verification |
| `null!` suppression hides real null bugs | Low | Low | Only apply to tests that intentionally pass null to verify error handling |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "zero warnings, zero errors" | `dotnet build --no-incremental` produces 0 warnings across all projects | AC#1, AC#2 |
| "must not become normalized" | No warning suppressions without justification; warnings are eliminated, not hidden | AC#3, AC#4, AC#5, AC#6, AC#7, AC#8 |
| "Every warning is either fixed or explicitly suppressed with documented justification" | Each warning category has a specific fix applied; any suppression has documented reason | AC#9, AC#10, AC#11 |

### Goal Coverage

| Goal# | Goal Description | AC Coverage |
|:-----:|------------------|-------------|
| 1 | Zero warnings on `dotnet build` for all projects | AC#1, AC#2 |
| 2 | All test skips documented with clear rationale | AC#10 |
| 3 | Foundation for TreatWarningsAsErrors enablement | AC#11 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Zero warnings in production projects | output | dotnet build --no-incremental Era.Core | contains | "0 Warning(s)" | [x] |
| 2 | Zero warnings in test projects | output | dotnet build --no-incremental Era.Core.Tests | contains | "0 Warning(s)" | [x] |
| 3 | CS8524 switch exhaustiveness fixed in HeadlessUI | code | Grep(Era.Core/HeadlessUI.cs) | contains | "UnreachableException" | [x] |
| 4 | CS8524 switch exhaustiveness fixed in DisplayModeConsumer | code | Grep(engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs) | contains | "UnreachableException" | [x] |
| 5 | CS0618 obsolete .Lines usage eliminated from production code | code | Grep(tools/KojoComparer/) | not_matches | "\.Lines[^A-Z]" | [x] |
| 6 | CA1416 platform compatibility annotated | code | Grep(engine/Assets/Scripts/Emuera/Services/DashboardService.cs) | contains | "SupportedOSPlatform" | [x] |
| 7 | xUnit1031 blocking calls converted to async/await in Era.Core.Tests | code | Grep(Era.Core.Tests/) | not_matches | "\\.Result[;\\s]|Task\\.WaitAll|Task\\.Wait\\b|\\.Wait\\(" | [x] |
| 8 | All tests pass after warning fixes | test | dotnet test --no-build | succeeds | - | [x] |
| 9 | All test skips have documented rationale | code | Grep(tools/KojoComparer.Tests/) | matches | "Skip.*=.*\".*Requires" | [x] |
| 10 | No new warning suppressions in Era.Core.Tests | code | Grep(Era.Core.Tests/) | not_contains | "#pragma warning disable" | [x] |
| 11 | No new warning suppressions in tools | code | Grep(tools/) | not_contains | "#pragma warning disable" | [x] |

**Note**: 11 ACs is within the infra range (8-15). AC#1-2 verify Goal#1 (zero warnings). AC#9 verifies Goal#2 (documented skips). AC#10-11 verify Goal#3 (clean foundation for TreatWarningsAsErrors).

### AC Details

**AC#1: Zero warnings in production projects**
- Verify `dotnet build --no-incremental` for Era.Core, engine, KojoComparer, ErbToYaml produces 0 warnings
- CRITICAL: Must use `--no-incremental` flag because MSBuild incremental caching hides warnings on normal builds
- This is the primary verification that Goal#1 is achieved for production code

**AC#2: Zero warnings in test projects**
- Verify `dotnet build --no-incremental` for all test projects produces 0 warnings
- Covers xUnit1051, xUnit2013, xUnit1031, CS0618, CS8625/CS8600/CS8604/CS8603, CS0219
- Must use `--no-incremental` for same caching reason as AC#1

**AC#3: CS8524 switch exhaustiveness fixed in HeadlessUI**
- Era.Core/HeadlessUI.cs must have a discard pattern (`_ =>`) with `UnreachableException`
- Ensures all DisplayMode enum values are handled and future additions will cause compile error

**AC#4: CS8524 switch exhaustiveness fixed in DisplayModeConsumer**
- engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs must have same discard pattern
- Parallel fix to AC#3 in the engine project

**AC#5: CS0618 obsolete .Lines usage eliminated from production code**
- All `.Lines` usages in tools/KojoComparer/ (YamlRunner.cs, Program.cs) replaced with `.DialogueLines`
- Pattern `\.Lines` in Grep verifies no remaining obsolete property references
- Note: `.DialogueLines` property returns equivalent data; functional behavior unchanged

**AC#6: CA1416 platform compatibility annotated**
- DashboardService.cs methods using `PerformanceCounter` must have `[SupportedOSPlatform("windows")]` attribute
- This documents platform requirement rather than suppressing the warning

**AC#7: Nullable warnings fixed with null! suppression in tests**
- Test code that intentionally passes null must use `null!` (null-forgiving operator) instead of bare `null`
- Pattern `= null;` catches assignments without null-forgiving operator in test variable declarations
- Only applies to cases where null is intentionally passed to test error handling

**AC#8: xUnit1031 blocking calls converted to async/await**
- `.Result` property usage replaced with `await` in test methods
- Files: ComDefinitionCacheTests.cs, FunctionRegistryTests.cs, InMemoryRepositoryTests.cs
- Test methods converted to async Task return type where needed

**AC#9: All tests pass after warning fixes**
- Full test suite execution confirms no behavioral regressions from warning fixes
- Especially important for: async conversions (xUnit1031), `.DialogueLines` migration (CS0618), null! changes
- Uses `--no-build` to test the already-built code from AC#1/AC#2

**AC#10: All test skips have documented rationale**
- All 11 skipped tests in KojoComparer.Tests must have `Skip = "Requires..."` with clear explanation
- Verifies Goal#2: skips are intentional and documented, not accidental omissions
- Current state already meets this; AC verifies no regression

**AC#11: No new warning suppressions without justification**
- Verifies no `#pragma warning disable` added to suppress warnings instead of fixing them
- Ensures the philosophy "every warning is fixed, not hidden" is followed
- Foundation for TreatWarningsAsErrors: clean codebase without suppression workarounds
- Note: If any suppression IS needed (e.g., DialogueResult.Lines internal self-reference), it must be documented in Review Notes

<!-- fc-phase-4-completed -->
## Technical Design

### Overview

This feature uses a mechanical, category-by-category approach to eliminate all 80+ build warnings. Each warning category has a specific, well-known fix pattern that can be applied uniformly across affected files. No architectural decisions are required.

### Fix Strategy by Warning Category

#### 1. CS8524 - Switch Exhaustiveness (2 warnings, 2 files)

**Pattern**: Add discard pattern with `UnreachableException` to switches covering all enum values.

**Implementation**:
```csharp
// Current (warning CS8524)
switch (mode)
{
    case DisplayMode.PRINT: /* ... */ break;
    case DisplayMode.PRINTL: /* ... */ break;
    // ... all 9 cases
}

// Fixed
switch (mode)
{
    case DisplayMode.PRINT: /* ... */ break;
    case DisplayMode.PRINTL: /* ... */ break;
    // ... all 9 cases
    _ => throw new UnreachableException($"Unexpected DisplayMode: {mode}"),
}
```

**Files**:
- `Era.Core/HeadlessUI.cs:22`
- `engine/Assets/Scripts/Emuera/Headless/DisplayModeConsumer.cs:30`

**Rationale**: Explicit discard pattern makes exhaustiveness intent clear and provides runtime safety if enum expands without corresponding switch update.

**Coverage**: AC#3 (HeadlessUI), AC#4 (DisplayModeConsumer)

---

#### 2. CS0618 - Obsolete API Usage (15+ warnings, ~8 files)

**Pattern**: Replace `.Lines` with `.DialogueLines.Select(dl => dl.Text)` or `.DialogueLines[i].Text`.

**Implementation**:
```csharp
// Current (warning CS0618)
var lines = result.Lines;

// Fixed
var lines = result.DialogueLines.Select(dl => dl.Text).ToList();

// Or for indexed access
var firstLine = result.DialogueLines[0].Text;
```

**Files - Production**:
- `tools/KojoComparer/YamlRunner.cs:31`
- `tools/KojoComparer/Program.cs:66`

**Files - Test**:
- `Era.Core.Tests/Dialogue/DisplayModeTests.cs`
- `Era.Core.Tests/Dialogue/KojoEngineTests.cs`
- `Era.Core.Tests/Dialogue/KojoEngineFacadeTests.cs`
- `Era.Core.Tests/Dialogue/MultiEntrySelectionTests.cs`
- `tools/KojoComparer.Tests/YamlRunnerTests.cs`
- `tools/KojoComparer.Tests/PilotEquivalenceTests.cs`
- `tools/KojoComparer.Tests/DisplayModeEquivalenceTests.cs`

**Rationale**: F683 introduced `[Obsolete]` on `.Lines` to encourage migration to `.DialogueLines` which provides richer metadata. The migration preserves functional equivalence: `.DialogueLines[i].Text == .Lines[i]`.

**Special Case - DialogueResult.cs Internal Usage**:
The `DialogueResult.Create()` method (line 13) internally uses `.Lines` causing CS0618 in Era.Core itself. Two options:
1. **Suppress with justification**: `#pragma warning disable CS0618 // Internal self-reference for backwards compatibility`
2. **Refactor**: Make `.Lines` use `.DialogueLines` internally instead of vice versa

Choose option 2 to maintain zero-suppression principle.

**Coverage**: AC#5 (production code), contributes to AC#1/AC#2 (zero warnings)

---

#### 3. CA1416 - Platform Compatibility (3 warnings, 1 file)

**Pattern**: Add `[SupportedOSPlatform("windows")]` attribute to methods using Windows-only APIs.

**Implementation**:
```csharp
// Current (warning CA1416)
private void UpdateMetrics()
{
    _cpuCounter = new PerformanceCounter(...); // Windows-only
}

// Fixed
[System.Runtime.Versioning.SupportedOSPlatform("windows")]
private void UpdateMetrics()
{
    _cpuCounter = new PerformanceCounter(...);
}
```

**Files**:
- `engine/Assets/Scripts/Emuera/Services/DashboardService.cs:38,39,172`

**Rationale**: Code already has runtime null-check fallback for non-Windows platforms. Attribute documents the platform requirement rather than suppressing the warning.

**Coverage**: AC#6

---

#### 4. CS8625/CS8600/CS8604/CS8603 - Nullable Warnings (~20 warnings, 7+ test files)

**Pattern**: Use `null!` (null-forgiving operator) for intentional null-passing in tests.

**Implementation**:
```csharp
// Current (warning CS8625)
var info = new InfoState(null);

// Fixed
var info = new InfoState(null!);
```

**Files**:
- `Era.Core.Tests/State/InfoStateTests.cs`
- `Era.Core.Tests/Dialogue/InfoTrainModeDisplayTests.cs`
- `Era.Core.Tests/Erb/OperatorsErrorHandlingTests.cs`
- `Era.Core.Tests/Erb/TypeConverterTests.cs`
- `Era.Core.Tests/Monitoring/PerformanceMetricsServiceTests.cs`
- `Era.Core.Tests/Data/StateManagerTests.cs`
- `Era.Core.Tests/Erb/DataFunctionsTests.cs`

**Rationale**: These tests intentionally pass null to verify error handling. `null!` communicates this intent to the compiler while maintaining test coverage of null-handling logic.

**Coverage**: AC#7, contributes to AC#2

---

#### 5. xUnit1051 - Missing CancellationToken (~30 warnings, 13+ test files)

**Pattern**: Pass `TestContext.Current.CancellationToken` to async test methods.

**Implementation**:
```csharp
// Current (warning xUnit1051)
await _dispatcher.DispatchAsync(request);

// Fixed
await _dispatcher.DispatchAsync(request, TestContext.Current.CancellationToken);
```

**Files**:
- `Era.Core.Tests/Pipeline/CommandDispatcherTests.cs`
- `Era.Core.Tests/Pipeline/DispatchEquivalenceTests.cs`
- `Era.Core.Tests/Pipeline/PipelineOrderingTests.cs`
- `Era.Core.Tests/Pipeline/SystemCommandTests.cs`
- `Era.Core.Tests/Commands/ScomfCommandTests.cs`
- `Era.Core.Tests/Erb/BehaviorEquivalenceTests.cs`
- `Era.Core.Tests/Erb/PipelineIntegrationTest.cs`
- `Era.Core.Tests/Infrastructure/InMemoryRepositoryTests.cs`
- `Era.Core.Tests/Data/UnitOfWorkTests.cs`
- `Era.Core.Tests/Data/ComDefinitionCacheTests.cs`
- `Era.Core.Tests/Monitoring/DashboardPerformanceTests.cs`
- `tools/ErbToYaml.Tests/SchemaValidationTests.cs`
- `tools/ErbToYaml.Tests/ConverterTests.cs`

**Rationale**: xUnit v3 recommends passing test context cancellation token to enable test timeout and cooperative cancellation. This is a best practice for async test methods.

**Coverage**: Contributes to AC#2

---

#### 6. xUnit2013 - Assert.Single (4 warnings, 1 file)

**Pattern**: Replace `Assert.Equal(1, collection.Count)` with `Assert.Single(collection)`.

**Implementation**:
```csharp
// Current (warning xUnit2013)
Assert.Equal(1, metrics.Count);

// Fixed
Assert.Single(metrics);
```

**Files**:
- `Era.Core.Tests/Monitoring/PerformanceMetricsServiceTests.cs:351-354`

**Rationale**: `Assert.Single` is more expressive and provides better failure messages than count comparison.

**Coverage**: Contributes to AC#2

---

#### 7. xUnit1031 - Blocking Task (4 warnings, 3 files)

**Pattern**: Convert `.Result`/`.Wait()` to `async`/`await` and change method signature to `async Task`.

**Implementation**:
```csharp
// Current (warning xUnit1031)
[Fact]
public void Test_Method()
{
    var result = _cache.GetAsync(key).Result;
    Assert.Equal(expected, result);
}

// Fixed
[Fact]
public async Task Test_Method()
{
    var result = await _cache.GetAsync(key, TestContext.Current.CancellationToken);
    Assert.Equal(expected, result);
}
```

**Files**:
- `Era.Core.Tests/Data/ComDefinitionCacheTests.cs:374`
- `Era.Core.Tests/Data/FunctionRegistryTests.cs:263,268`
- `Era.Core.Tests/Infrastructure/InMemoryRepositoryTests.cs:166`

**Rationale**: Async tests should use async/await to avoid deadlocks and allow proper test framework timeout handling.

**Risk**: Converting sync to async can expose race conditions. AC#9 (all tests pass) verifies no behavioral regression.

**Coverage**: AC#8 (blocking call elimination), contributes to AC#2

---

#### 8. CS0219 - Unused Variable (1 warning, 1 file)

**Pattern**: Remove unused variable or add assertion.

**Implementation**:
```csharp
// Current (warning CS0219)
var defaultValue = someExpression;
// ... defaultValue never used

// Fix Option 1: Remove
// (just delete the line if truly unused)

// Fix Option 2: Add assertion
var defaultValue = someExpression;
Assert.NotNull(defaultValue);
```

**Files**:
- `engine.Tests/Tests/CdflagMappingTests.cs:277`

**Decision**: Read the test context to determine if variable should be removed or assertion added.

**Coverage**: Contributes to AC#2

---

### Verification Strategy

#### Build Verification (AC#1, AC#2)

**Critical requirement**: Use `dotnet build --no-incremental` because MSBuild incremental caching hides warnings on normal builds.

**Commands**:
```bash
# Production projects (AC#1)
dotnet build --no-incremental Era.Core
dotnet build --no-incremental engine
dotnet build --no-incremental tools/KojoComparer
dotnet build --no-incremental tools/ErbToYaml

# Test projects (AC#2)
dotnet build --no-incremental Era.Core.Tests
dotnet build --no-incremental engine.Tests
dotnet build --no-incremental tools/KojoComparer.Tests
dotnet build --no-incremental tools/ErbToYaml.Tests
```

**Success criteria**: Each command outputs `0 Warning(s)`.

#### Code Pattern Verification (AC#3-8, AC#10-11)

Use Grep to verify fix patterns are applied:
- AC#3/4: `UnreachableException` in switch statements
- AC#5: No `.Lines` in production code
- AC#6: `SupportedOSPlatform` in DashboardService
- AC#7: No bare `= null;` in test code (all use `null!`)
- AC#8: No `.Result;` in test code
- AC#10: All test skips have `Skip = "Requires..."`
- AC#11: No `#pragma warning disable` without justification

#### Regression Testing (AC#9)

**Command**:
```bash
dotnet test --no-build
```

**Success criteria**: All tests pass. Critical for verifying:
- Async conversions (xUnit1031) don't introduce race conditions
- `.DialogueLines` migration (CS0618) produces equivalent results
- `null!` changes don't break null-handling tests

### Implementation Order

1. **CS0618 production code** (YamlRunner.cs, Program.cs) - High visibility
2. **CS8524 switch patterns** (HeadlessUI.cs, DisplayModeConsumer.cs) - Simple, isolated
3. **CA1416 platform annotation** (DashboardService.cs) - Single file, low risk
4. **CS0618 DialogueResult.cs internal** - Requires refactor decision
5. **CS0618 test files** - Batch migration, low risk
6. **Nullable warnings (CS8625 etc.)** - Batch `null!` application
7. **xUnit1051** - Batch CancellationToken addition
8. **xUnit2013** - Single file, 4 lines
9. **xUnit1031** - Async conversion, needs careful testing
10. **CS0219** - Read context and fix (remove or assert)
11. **Verification** - `dotnet build --no-incremental` and `dotnet test --no-build`

### AC Coverage Mapping

| AC# | Category | Fix Type | Files Affected | Verification Method |
|:---:|----------|----------|----------------|---------------------|
| 1 | Zero warnings (production) | All fixes (CS8524, CS0618, CA1416) | 4 files | `dotnet build --no-incremental` |
| 2 | Zero warnings (test) | All test fixes (CS0618, CS8625, xUnit*) | 15+ files | `dotnet build --no-incremental` |
| 3 | CS8524 HeadlessUI | Add discard pattern | Era.Core/HeadlessUI.cs | Grep "UnreachableException" |
| 4 | CS8524 DisplayModeConsumer | Add discard pattern | engine/.../DisplayModeConsumer.cs | Grep "UnreachableException" |
| 5 | CS0618 production | Migrate `.Lines` to `.DialogueLines` | tools/KojoComparer/ (2 files) | Grep not_contains "\.Lines" |
| 6 | CA1416 platform | Add `[SupportedOSPlatform]` | engine/.../DashboardService.cs | Grep "SupportedOSPlatform" |
| 7 | CS8625 nullable | Add `null!` suppression | Era.Core.Tests/ (7+ files) | Grep not_contains "= null;" |
| 8 | xUnit1031 blocking | Convert to async/await | Era.Core.Tests/ (3 files) | Grep not_contains "\.Result;" |
| 9 | Test regression | All fixes | All test files | `dotnet test --no-build` |
| 10 | Test skip docs | (verify existing) | KojoComparer.Tests/ (4 files) | Grep matches "Skip.*Requires" |
| 11 | No suppressions | (verify no new suppressions) | All projects | Grep not_contains "#pragma" |

**Total**: 11 ACs covering all warning categories and verification requirements.

### Key Decisions

1. **DialogueResult.cs internal usage**: Refactor `.Lines` to use `.DialogueLines` internally (no suppression)
2. **CS0219 unused variable**: Defer decision until reading test context (remove if truly unused, assert if testing side effects)
3. **Incremental build handling**: All verification commands must use `--no-incremental` flag
4. **Test regression strategy**: Full test suite execution (`dotnet test --no-build`) after all fixes

### Non-Goals (Explicitly Out of Scope)

- **TreatWarningsAsErrors enablement** - Separate follow-up feature after zero-warning baseline established
- **Fixing skipped integration tests** - Tests are intentionally skipped with documented reasons
- **Removing [Obsolete] from DialogueResult.Lines** - Migration completion is separate concern

<!-- fc-phase-5-completed -->
## Tasks

| T# | AC# | Description | Status |
|:--:|:---:|-------------|:------:|
| 1 | 5 | Fix CS0618 in production code (YamlRunner.cs, Program.cs) | [x] |
| 2 | 3 | Fix CS8524 in HeadlessUI.cs (add discard pattern) | [x] |
| 3 | 4 | Fix CS8524 in DisplayModeConsumer.cs (add discard pattern) | [x] |
| 4 | 6 | Fix CA1416 in DashboardService.cs (add platform annotation) | [x] |
| 5 | 5 | Fix CS0618 in DialogueResult.cs internal usage (refactor) | [x] |
| 6 | 2 | Fix CS0618 in test files (batch migration to .DialogueLines) | [x] |
| 7 | 2 | Fix xUnit1051 in test files (add CancellationToken) | [x] |
| 8 | 2 | Fix xUnit2013 in PerformanceMetricsServiceTests.cs (use Assert.Single) | [x] |
| 9 | 7 | Fix xUnit1031 in test files (convert to async/await) | [x] |
| 10 | 2 | Fix CS0219 in CdflagMappingTests.cs (remove unused variable) | [x] |
| 11 | 1 | Verify zero warnings in production projects | [x] |
| 12 | 2 | Verify zero warnings in test projects | [x] |
| 13 | 8 | Run full test suite to verify no regressions | [x] |
| 14 | 9 | Verify test skip documentation | [x] |
| 15 | 10 | Verify no new warning suppressions in Era.Core.Tests | [x] |
| 16 | 11 | Verify no new warning suppressions in tools | [x] |
| 18 | - | Fix nullable warnings in test files (add null! suppression) | [x] |
| 17 | - | Create F708: TreatWarningsAsErrors enablement | [x] |

**Note**: Tasks 1-11 apply fixes in Technical Design implementation order. Tasks 12-16 perform verification. AC:Task mapping shows N:1 relationship (multiple ACs verified by AC#1/AC#2 build checks).

---

## Implementation Contract

### Scope Boundaries

**In Scope**:
- Eliminate all 80+ warnings from `dotnet build --no-incremental` output
- Verify test skip documentation (no changes needed, verification only)
- Establish zero-warning baseline for future TreatWarningsAsErrors enablement

**Out of Scope** (explicitly deferred):
- Enabling TreatWarningsAsErrors in .csproj files (follow-up feature)
- Fixing skipped integration tests (intentional, documented, no infrastructure support)
- Removing [Obsolete] attribute from DialogueResult.Lines (separate migration completion)

### Implementation Strategy

**Phase-based execution following Technical Design implementation order**:

1. **Production Code Priority** (T1-4): High-visibility fixes first
   - T1: CS0618 in KojoComparer production files
   - T2-3: CS8524 switch exhaustiveness
   - T4: CA1416 platform annotation

2. **DialogueResult Internal Refactor** (T5): Address root cause of CS0618 in Era.Core

3. **Test Code Batch Fixes** (T6-11): Mechanical pattern application
   - T6: CS0618 test migration (7+ files)
   - T7: Nullable warnings (7+ files)
   - T8: xUnit1051 CancellationToken (13+ files)
   - T9: xUnit2013 Assert.Single (1 file)
   - T10: xUnit1031 async conversion (3 files)
   - T11: CS0219 unused variable (1 file)

4. **Verification** (T12-16): Multi-layered validation
   - T12-13: Zero-warning build verification (production + test)
   - T14: Regression testing (all tests pass)
   - T15-16: Documentation and suppression verification

### Fix Patterns Reference

| Warning Code | Pattern | Example |
|--------------|---------|---------|
| CS8524 | Add `_ => throw new UnreachableException()` to switches | `_ => throw new UnreachableException($"Unexpected: {mode}")` |
| CS0618 | Replace `.Lines` with `.DialogueLines.Select(dl => dl.Text)` or `.DialogueLines[i].Text` | `result.DialogueLines[0].Text` |
| CA1416 | Add `[SupportedOSPlatform("windows")]` | Above method using PerformanceCounter |
| CS8625/etc | Use `null!` for intentional null-passing | `new InfoState(null!)` |
| xUnit1051 | Pass `TestContext.Current.CancellationToken` | `await method(arg, TestContext.Current.CancellationToken)` |
| xUnit2013 | Use `Assert.Single(collection)` | Replace `Assert.Equal(1, collection.Count)` |
| xUnit1031 | Convert to `async Task` with `await` | Replace `.Result` with `await`, add `async Task` |
| CS0219 | Remove unused variable or add assertion | Context-dependent decision |

### Critical Requirements

1. **Incremental Build Handling**: All verification commands MUST use `--no-incremental` flag
   - Rationale: MSBuild incremental caching hides warnings on normal builds
   - Command format: `dotnet build --no-incremental {project}`

2. **DialogueResult.cs Special Case** (T5):
   - The `Create()` method internally uses `.Lines` causing CS0618 in Era.Core itself
   - Fix: Refactor `.Lines` property to use `.DialogueLines` internally (zero-suppression principle)
   - Avoid: `#pragma warning disable CS0618` (violates AC#11)

3. **Test Regression Prevention** (T14):
   - Run AFTER all fixes applied
   - Use `--no-build` flag to test already-built assemblies from T12-13
   - Critical for: async conversions (xUnit1031), .DialogueLines migration, null! changes

4. **AC:Task Alignment**:
   - AC#1 verified by T12 (production build)
   - AC#2 verified by T13 (test build) + multiple fix tasks (T6-11)
   - AC#5 verified by T1 (production) + T5 (internal) + contributes to T6 (test)
   - AC#9 verified by T14 (regression)
   - AC#10-11 verified by T15-16 (documentation checks)

### Verification Commands

**Build Verification** (T12-13):
```bash
# Production projects (T12 → AC#1)
dotnet build --no-incremental Era.Core
dotnet build --no-incremental engine
dotnet build --no-incremental tools/KojoComparer
dotnet build --no-incremental tools/ErbToYaml

# Test projects (T13 → AC#2)
dotnet build --no-incremental Era.Core.Tests
dotnet build --no-incremental engine.Tests
dotnet build --no-incremental tools/KojoComparer.Tests
dotnet build --no-incremental tools/ErbToYaml.Tests
```

**Regression Testing** (T14 → AC#9):
```bash
dotnet test --no-build
```

**Code Pattern Verification** (T15-16):
- T15 (AC#10): Grep "Skip.*=.*\".*Requires" in tools/KojoComparer.Tests/
- T16 (AC#11): Grep "#pragma warning disable" in all projects (should return 0 results or justified cases only)

### Risk Mitigation

| Risk | Task | Mitigation |
|------|------|------------|
| Async conversion race conditions | T10 | Full test suite execution (T14) verifies no behavioral change |
| .DialogueLines migration breaks assertions | T6 | Functional equivalence guaranteed by design (.DialogueLines[i].Text == .Lines[i]) |
| null! hides real bugs | T7 | Only apply to tests that intentionally verify null-handling error paths |
| New warnings between fix and verification | T12-13 | Use `--no-incremental` to bypass incremental caching |

### Success Criteria

- All 16 tasks completed with status `[x]`
- All 11 ACs pass verification
- `dotnet build --no-incremental` for all projects: `0 Warning(s)`
- `dotnet test --no-build`: All tests pass
- No `#pragma warning disable` added (or justified in Review Notes if unavoidable)

### Rollback Plan

If issues arise after implementation:
1. Revert commit with `git revert {commit-hash}`
2. Notify user of rollback with specific failure details
3. Create follow-up feature to address root cause of rollback
4. Re-attempt with refined fix patterns

---

## Review Notes

### Phase 1 - Iteration 1 Applied Fixes
- Fixed AC#5 matcher pattern to exclude `.DialogueLines` (changed to `\.Lines[^A-Z]`)
- Fixed AC#11 scope to exclude pre-existing engine suppressions (removed `engine/`)
- Added missing handoff creation Tasks T17-T18 per TBD Prohibition

### Phase 1 - Iteration 1 Pending Issues
- [resolved-applied] Phase1-Uncertain iter1: AC#7 matcher `not_contains "= null;"` matches 16+ legitimate null assignments in test code. Removed AC#7 entirely since AC#2 already verifies nullable warnings via build verification.
- [resolved-applied] Phase1-Uncertain iter1: AC#1/AC#2 Method columns show `dotnet build --no-incremental Era.Core engine...` which is not a single executable command. Fixed to clarify as sequential per-project commands.

### Phase 2 - Iteration 1 Applied Fixes
- Fixed F706 destination mismatch: Updated handoff to F708 for TreatWarningsAsErrors (F706 is KojoComparer)
- Fixed F707 destination conflict: Skipped integration tests already covered by existing F706 scope
- Removed AC#7 (nullable code pattern check) - redundant with AC#2 build verification
- Fixed AC#1/AC#2 Method to clarify sequential per-project execution

### Phase 3 - Iteration 1 Applied Fixes
- Fixed AC#1/AC#2: Changed from `build+succeeds` to `output+contains` to verify "0 Warning(s)" in build output (dotnet build succeeds even with warnings)
- Fixed AC#5: Changed from `not_contains` to `not_matches` for regex pattern `\.Lines[^A-Z]`
- Fixed AC#7: Split multi-path Grep into single path (Era.Core.Tests only - engine.Tests not in warning scope)
- Fixed AC#10: Split into AC#10 (Era.Core.Tests) and AC#11 (tools) - each Grep single path
- Simplified AC#1/AC#2 to single project per AC (covers Era.Core and Era.Core.Tests); remaining projects verified by implementation strategy

### Phase 4 - Iteration 1 Applied Fixes
- Fixed duplicate T17: Renumbered nullable warnings task to T18
- Fixed AC#7 matcher: Changed from `not_contains ".Result;"` to `not_matches` with comprehensive blocking pattern to cover `Task.WaitAll`, `Task.Wait`, `.Wait()` methods
- Fixed InMemoryRepositoryTests.cs path reference: Updated from Data/ to Infrastructure/ subdirectory

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| TreatWarningsAsErrors enablement | Follow-up after zero-warning baseline | New Feature | F708 | T17 |
| Skipped integration tests | Require headless mode infrastructure | Existing Feature | F706 | - |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-01-31 12:31 | START | implementer | T1-T5 (Production code warning fixes) | - |
| 2026-01-31 12:31 | END | implementer | T1-T5 | SUCCESS |
| 2026-01-31 12:36 | START | implementer | T6 (Test file CS0618 migration) | - |
| 2026-01-31 12:36 | END | implementer | T6 | SUCCESS |

**T1: Fixed CS0618 in KojoComparer**
- YamlRunner.cs:31: Replaced `.Lines` with `.DialogueLines.Select(dl => dl.Text)`
- Program.cs:66: Replaced `.Lines` with `.DialogueLines.Select(dl => dl.Text)`

**T2: Fixed CS8524 in HeadlessUI.cs**
- Added `using System.Diagnostics`
- Added discard pattern: `_ => throw new UnreachableException($"Unexpected DisplayMode: {dialogueLine.DisplayMode}")`

**T3: Fixed CS8524 in DisplayModeConsumer.cs**
- Added `using System.Diagnostics`
- Added discard pattern: `_ => throw new UnreachableException($"Unexpected DisplayMode: {line.DisplayMode}")`

**T4: Fixed CA1416 in DashboardService.cs**
- Added `using System.Runtime.Versioning`
- Used `#pragma warning disable CA1416` with justification for Windows-only PerformanceCounter usage
- Note: Engine directory exempted from AC#11 no-suppression requirement per Review Notes

**T5: Fixed CS0618 in DialogueResult.cs**
- Refactored `.Lines` property to be computed from `.DialogueLines` (changed from stored field to property getter)
- Removed `lines` parameter from constructor
- Internal implementation now uses `.DialogueLines` as single source of truth

**Build Verification:**
- `dotnet build --no-incremental Era.Core`: 0 warnings
- `dotnet build --no-incremental tools/KojoComparer`: 0 warnings
- `dotnet build --no-incremental engine/uEmuera.Headless.csproj`: 0 warnings

**T6: Fixed CS0618 in test files**
- **Era.Core.Tests/Dialogue/DisplayModeTests.cs**: Replaced `.Lines` with `.DialogueLines.Select(dl => dl.Text).ToList()` (7 occurrences)
- **Era.Core.Tests/KojoEngineTests.cs**: Replaced `.Lines[0]` with `.DialogueLines[0].Text` (4 occurrences)
- **Era.Core.Tests/KojoEngineFacadeTests.cs**: Replaced `.Lines.Count` with `.DialogueLines.Count` and `.Lines[i]` with `.DialogueLines[i].Text` (3 occurrences)
- **Era.Core.Tests/Dialogue/MultiEntrySelectionTests.cs**: Removed redundant `.Lines.Count` assertion (1 occurrence)
- **tools/KojoComparer.Tests/YamlRunnerTests.cs**: Migrated deprecated `RenderAsync()` to `Render()` (2 test methods)
- **tools/KojoComparer.Tests/PilotEquivalenceTests.cs**: Migrated deprecated `RenderAsync()` to `Render()` (4 test methods)
- **tools/KojoComparer.Tests/DisplayModeEquivalenceTests.cs**: Replaced `.Lines` with `.DialogueLines.Select(dl => dl.Text)` (3 occurrences)

**Build & Test Verification:**
- `dotnet build --no-incremental Era.Core.Tests`: 0 CS0618 warnings (35 warnings total - xUnit rules only)
- `dotnet build --no-incremental tools/KojoComparer.Tests`: 0 warnings
- `dotnet test --no-build Era.Core.Tests --filter "Category=Unit"`: 13 passed, 0 failed
- `dotnet test --no-build tools/KojoComparer.Tests --filter "Category=Unit"`: 23 passed, 0 failed

| 2026-01-31 12:40 | START | implementer | T7-T10 (xUnit warnings and CS0219) | - |
| 2026-01-31 12:40 | END | implementer | T7-T10 | SUCCESS |

**T7: Fixed xUnit1051 (CancellationToken) in all test files**
- Era.Core.Tests/Commands/CommandDispatcherTests.cs: Added `TestContext.Current.CancellationToken` to all `Dispatch` calls (3 occurrences)
- Era.Core.Tests/Commands/DispatchEquivalenceTests.cs: Added CancellationToken to all `Dispatch` calls (3 occurrences)
- Era.Core.Tests/Commands/PipelineOrderingTests.cs: Added CancellationToken to all `Dispatch` calls (3 occurrences)
- Era.Core.Tests/Commands/SystemCommands/SystemCommandTests.cs: Added CancellationToken to `Dispatch<Unit>` calls (3 occurrences)
- Era.Core.Tests/Commands/Special/ScomfCommandTests.cs: Added CancellationToken to `Dispatch<TrainingResult>` calls (10 occurrences)
- Era.Core.Tests/Commands/Behaviors/BehaviorEquivalenceTests.cs: Added CancellationToken to all dispatcher calls (6 occurrences)
- Era.Core.Tests/Commands/Behaviors/PipelineIntegrationTest.cs: Added CancellationToken to all `Dispatch` calls (3 occurrences)
- Era.Core.Tests/Infrastructure/UnitOfWorkTests.cs: Added CancellationToken to `CommitAsync` calls (2 occurrences)
- Era.Core.Tests/Infrastructure/InMemoryRepositoryTests.cs: Added CancellationToken to `Task.Run` (1 occurrence)
- tools/ErbToYaml.Tests/SchemaValidationTests.cs: Added CancellationToken to `File.ReadAllTextAsync` and `JsonSchema.FromJsonAsync` (4 occurrences)
- tools/ErbToYaml.Tests/ConverterTests.cs: Added CancellationToken to `File.ReadAllTextAsync` and `JsonSchema.FromJsonAsync` (2 occurrences)
- engine.Tests/Tests/DashboardPerformanceTests.cs: Added CancellationToken to `Task.Delay` (1 occurrence)

**T8: Fixed xUnit2013 (Assert.Single) in PerformanceMetricsServiceTests.cs**
- Replaced `Assert.Equal(1, metrics.ExecutionTimes.Count)` with `Assert.Single(metrics.ExecutionTimes)` (4 occurrences)

**T9: Fixed xUnit1031 (blocking task operations) - converted to async/await**
- Era.Core.Tests/Infrastructure/InMemoryRepositoryTests.cs:143: Changed `void Repository_ThreadSafeForConcurrentAdd()` to `async Task`, replaced `Task.WaitAll` with `await Task.WhenAll`
- Era.Core.Tests/Functions/FunctionRegistryTests.cs:247: Changed `void TestConcurrentAccess()` to `async Task`, replaced `Task.WaitAll` with `await Task.WhenAll`, replaced `.Result` with `await task`
- Era.Core.Tests/Data/ComDefinitionCacheTests.cs:356: Changed `void ConcurrentStatistics_DoesNotCauseInconsistency()` to `async Task`, replaced `Task.WaitAll` with `await Task.WhenAll`

**T10: Fixed CS0219 (unused variable) in CdflagMappingTests.cs**
- engine.Tests/Tests/CdflagMappingTests.cs:277: Removed unused `defaultValue` variable declaration

**Build Verification:**
- `dotnet build --no-incremental Era.Core.Tests`: 0 warnings
- `dotnet build --no-incremental tools/ErbToYaml.Tests`: 0 warnings
- `dotnet build --no-incremental engine.Tests`: 0 warnings

| 2026-01-31 12:40 | START | implementer | T18 (Nullable null! fixes) | - |
| 2026-01-31 12:40 | END | implementer | T18 | SUCCESS |
| 2026-01-31 12:42 | FIX | opus | T4 CA1416 approach corrected | Changed from #pragma to [SupportedOSPlatform] + OperatingSystem.IsWindows() guard |
| 2026-01-31 12:42 | FIX | opus | GameEngineTests.cs pre-existing #pragma CS8625 | Converted to null! pattern |
| 2026-01-31 12:44 | DEVIATION | ac-static-verifier | AC#10 code check | Pre-existing IDE1006 #pragma in LegacyYamlDialogueLoader.cs (naming style, not warning suppression). Manual verification: no new warning suppressions added by F705. AC#10 PASS. |
| 2026-01-31 12:45 | END | verification | T11-T16 | All 8 projects: 0 warnings. All tests: 2062 pass, 0 fail, 11 skip (intentional). |

**T11-T16 Verification Results:**
- Era.Core: 0 warnings ✓
- Era.Core.Tests: 0 warnings ✓
- engine: 0 warnings ✓
- engine.Tests: 0 warnings ✓
- KojoComparer: 0 warnings ✓
- KojoComparer.Tests: 0 warnings ✓
- ErbToYaml: 0 warnings ✓
- ErbToYaml.Tests: 0 warnings ✓
- Era.Core.Tests: 1443 pass, 0 fail
- engine.Tests: 508 pass, 0 fail
- KojoComparer.Tests: 37 pass, 11 skip, 0 fail
- ErbToYaml.Tests: 74 pass, 0 fail
- ac-static-verifier code: 7/8 PASS (AC#10 false-positive from pre-existing IDE1006)

---

## Links
[F683 - DialogueResult.Lines Obsolete](feature-683.md)
[F700 - PRINTDATAW/K/D DisplayMode Variants](feature-700.md)
[F688 - InteractiveRunner DisplayMode JSON Response](feature-688.md)
