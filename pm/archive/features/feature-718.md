# Feature 718: Era.Core Data Loader Test Coverage

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
Test coverage should grow alongside implementation. Data loaders are the bridge between YAML/CSV definitions and runtime game state — correctness here prevents hard-to-diagnose runtime bugs. Inherits philosophy from F716 (Era.Core Test Coverage Hardening).

### Problem (Current Issue)
Era.Core/Data/ has 55 implementation files but only 28 test files (~50% coverage). (Note: corrected to 6 testable files in Root Cause Analysis below.) Phase 17 (Data Migration) completed CSV-to-YAML migration but did not require unit tests for all loaders. The untested loaders include:

- Several constant-type loaders (YAML-based)
- Legacy CSV loaders still in use
- Data transformation/mapping utilities

Additionally, the IO/Encoding/DI namespace has 5 implementation files with 0 test files (100% uncovered). These system-level utilities (encoding detection, file I/O helpers, dependency injection configuration) are foundational infrastructure that other namespaces depend on.

Without tests, loader bugs (wrong index, missing default, encoding issue) and IO/Encoding/DI utility bugs would only surface at runtime.

### Goal (What to Achieve)
Add unit tests for the ~27 untested Data/ loaders to bring namespace coverage above 80%.

**Scope**: Era.Core/Data/ and Era.Core.Tests/Data/ only. Training/Character/Ability tests are F716. Tool tests are F717.

**Volume estimate**: ~15-25 new test files, ~100-150 test methods.

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Era.Core/Data/ has untested YAML loaders and IO/Encoding/DI utilities have zero test coverage.
2. Why: F716 (parent initiative) identified the gap but scoped it out — F716 focused on Training/Character/Ability namespaces only, deferring Data/ and IO/Encoding/DI to a successor feature.
3. Why: The original Data/ migration (Phase 17, F583) created loader tests for most constant-type loaders but skipped 3 loaders: YamlComLoader, YamlGameBaseLoader, and YamlVariableSizeLoader.
4. Why: YamlComLoader was complex (caching, directory scanning) and tested indirectly through ComDefinitionCacheTests and integration tests. YamlGameBaseLoader and YamlVariableSizeLoader were added later (F713) without accompanying unit tests.
5. Why: No coverage enforcement gate existed during F583/F713 — loaders were manually tracked, and gaps accumulated without automated detection.

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| Feature spec claims "55 impl files, only 28 test files (~50% coverage)" | The 55 count includes 27 interfaces (I*.cs) and 27 model classes (Models/*.cs) that are not unit-testable. The actual testable implementation files are 28 (27 Yaml loaders + 1 ComDefinitionCache). Of these, 25 already have tests. Only 3 YAML loaders lack dedicated test files. |
| IO/Encoding/DI claimed "5 implementation files with 0 test files" | Actual count is 4 files: 1 interface (IFileSystem.cs — untestable), 2 DI classes (ServiceCollectionExtensions.cs, CallbackFactories.cs), 1 encoding helper (ShiftJisHelper.cs). Only 3 are testable, and all 3 lack tests. |

### Conclusion

The root cause is **scope deferral without precise measurement**: F716 identified the Data/ and IO/Encoding/DI gaps by file count but did not verify which files were testable vs. interfaces/models. The actual untested coverage gap is **6 files** (3 YAML loaders + 3 utilities), not ~27 as originally estimated.

**Corrected inventory**:

**Data/ namespace** (Era.Core/Data/):
- 27 YAML loaders (YamlXxxLoader.cs) — of these, **24 have tests**, **3 untested**
- 27 interfaces (IXxxLoader.cs) — not testable (pure interfaces)
- 27 model classes (Models/XxxConfig.cs) — not testable (pure data classes, no business logic)
- 1 ComDefinitionCache.cs — **already tested** (ComDefinitionCacheTests.cs)
- Test files: 28 (24 loader tests + CdflagVariableSizeTests + ComDefinitionCacheTests + CriticalConfigEquivalenceTests + CriticalConfigIntegrationTests)

**Untested YAML loaders** (3 files):
| File | Complexity | Notes |
|------|:----------:|-------|
| YamlComLoader.cs | HIGH | Caching, directory scanning (LoadAll), cache invalidation. Dependencies: ComDefinitionCache, ILogger |
| YamlGameBaseLoader.cs | LOW | Simple single-file YAML loader, same pattern as tested loaders |
| YamlVariableSizeLoader.cs | LOW | Simple single-file YAML loader, same pattern as tested loaders |

**Untested IO/Encoding/DI** (3 files):
| File | Complexity | Notes |
|------|:----------:|-------|
| ShiftJisHelper.cs | LOW | Static utility class (IsHalfwidth, GetByteCount), no dependencies |
| ServiceCollectionExtensions.cs | MEDIUM | DI registration (~150 service registrations), verifiable via container resolution |
| CallbackFactories.cs | MEDIUM | Factory delegates for training variable access, requires mocking ITrainingVariables/ITEquipVariables |

**IFileSystem.cs** is a pure interface (no implementation in Era.Core/IO/) — not testable.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F716 | [DONE] | Parent initiative | Completed Training/Character/Ability test coverage; deferred Data/ and IO/Encoding/DI to this feature |
| F713 | [DONE] | Related | Added YAML variable definitions for remaining constant types (added YamlGameBaseLoader, YamlVariableSizeLoader without tests) |
| F583 | [DONE] | Origin | Phase 17 Data Migration — created 24 of 27 loader tests, skipped ComLoader/GameBaseLoader/VariableSizeLoader |
| F580 | [DONE] | Related | COM Loader Performance Optimization — added caching to YamlComLoader, ComDefinitionCache (cache tested, loader not) |
| F499 | [DONE] | Foundation | Test strategy design — established test patterns used by existing loader tests |
| F427 | [DONE] | Related | ShiftJisHelper extraction — created Encoding/ShiftJisHelper.cs without dedicated tests |
| F717 | [DRAFT] | Sibling | Tool test coverage — parallel successor of F716 for tool tests |

### Pattern Analysis

The gap follows a consistent pattern: when loaders were batch-created in F583, most got tests, but a few edge cases (YamlComLoader with complex caching, late additions in F713) were missed. IO/Encoding/DI utilities were extracted as part of refactoring features (F427, F377, F405) that focused on extraction correctness, not test coverage. The test-strategy.md (F499) was designed after these extractions, so they predated the coverage enforcement process.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | All 6 untested files follow established patterns. Existing tests (e.g., AblLoaderTests.cs) provide templates. |
| Scope is realistic | YES | 6 files to test (reduced from claimed ~27). Estimated ~6 new test files, ~30-50 test methods. |
| No blocking constraints | YES | F716 is [DONE]. No technical blockers — all dependencies (YamlDotNet, DI container, xUnit) are already in the test project. |

**Verdict**: FEASIBLE

**Note**: The Background section numbers are significantly overstated. The actual scope is ~6 test files, not 15-25. The Background should be corrected before AC design:
- "55 implementation files" → 28 testable (27 loaders + 1 cache), of which 25 already tested
- "~27 untested" → 3 untested loaders + 3 untested utilities = **6 untested**
- "5 IO/Encoding/DI files" → 4 files (1 interface not testable), **3 testable and untested**
- Volume estimate "15-25 test files, 100-150 methods" → **~6 test files, ~30-50 methods**

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F716 | [DONE] | Era.Core test coverage hardening (parent initiative) — completed, no blocker |
| Related | F713 | [DONE] | YAML variable definitions — created untested YamlGameBaseLoader/YamlVariableSizeLoader |
| Related | F583 | [DONE] | Phase 17 Data Migration — origin of most loader tests |
| Related | F499 | [DONE] | Test strategy design — patterns to follow |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| YamlDotNet | NuGet | Low | Already referenced by Era.Core.Tests, used by all existing loader tests |
| Microsoft.Extensions.DependencyInjection | NuGet | Low | Already referenced, needed for ServiceCollectionExtensions tests |
| xUnit / coverlet.collector | NuGet | Low | Already in test project |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | LOW | Registers all loaders; tests would verify DI wiring |
| Era.Core/Common/VariableDefinitions.cs | LOW | Consumes YamlVariableSizeLoader output |
| Era.Core/Common/GameInitialization.cs | LOW | Consumes YamlGameBaseLoader output |
| Commands that use COM system | LOW | Depend on YamlComLoader through DI |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core.Tests/Data/ComLoaderTests.cs | Create | Tests for YamlComLoader (Load, LoadAll, caching, cache invalidation) |
| Era.Core.Tests/Data/GameBaseLoaderTests.cs | Create | Tests for YamlGameBaseLoader (positive/negative path) |
| Era.Core.Tests/Data/VariableSizeLoaderTests.cs | Create | Tests for YamlVariableSizeLoader (positive/negative path) |
| Era.Core.Tests/Encoding/ShiftJisHelperTests.cs | Create | Tests for IsHalfwidth, GetByteCount |
| Era.Core.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs | Create | Tests for AddEraCore service registration |
| Era.Core.Tests/DependencyInjection/CallbackFactoriesTests.cs | Create | Tests for AddTrainingCallbacks factory registration |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| TreatWarningsAsErrors=true | Directory.Build.props (F708) | LOW — new test files must compile cleanly |
| Existing BaseTestClass pattern | Era.Core.Tests/BaseTestClass.cs | LOW — loader tests must inherit BaseTestClass for DI setup |
| YamlComLoader requires ComDefinitionCache | Constructor dependency | LOW — test needs to instantiate or mock ComDefinitionCache |
| ServiceCollectionExtensions registers ~150 services | DI container test complexity | MEDIUM — verification test needs to resolve many services; may need partial verification strategy |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| YamlComLoader tests require real file system (caching uses File.GetLastWriteTime) | High | Low | Use temp files in test, clean up in finally blocks (matches existing pattern in AblLoaderTests) |
| ServiceCollectionExtensions test may fail if any registered service has unsatisfied dependencies | Medium | Medium | Test service registration count or use IServiceCollection inspection instead of full resolution |
| ShiftJisHelper halfwidth character set correctness is hard to verify exhaustively | Low | Low | Test boundary cases (ASCII, known halfwidth, known fullwidth) rather than exhaustive set |
| Scope creep: Models might be claimed to need tests | Low | Low | Models are pure data classes with no logic — explicitly exclude from scope |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Test coverage should grow alongside implementation" (inherited from F716) | New test files must be created for all 6 untested files | AC#1-AC#6 |
| "Data loaders are the bridge between YAML/CSV definitions and runtime game state" | Loader tests must verify correct data loading behavior | AC#1-AC#3 |
| "correctness here prevents hard-to-diagnose runtime bugs" | All tests must pass to confirm correctness | AC#7 |
| "correctness here prevents hard-to-diagnose runtime bugs" | Build must succeed with zero warnings (TreatWarningsAsErrors) | AC#8 |
| "Add unit tests for the ~27 untested Data/ loaders" (Goal, corrected to 6) | Each new test file must have non-trivial test coverage (>= 3 methods) | AC#9 |
| "infrastructure without tests is hidden technical debt" (inherited) | No technical debt markers in new test files | AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ComLoaderTests.cs exists | file | Glob(Era.Core.Tests/Data/ComLoaderTests.cs) | exists | - | [x] |
| 2 | GameBaseLoaderTests.cs exists | file | Glob(Era.Core.Tests/Data/GameBaseLoaderTests.cs) | exists | - | [x] |
| 3 | VariableSizeLoaderTests.cs exists | file | Glob(Era.Core.Tests/Data/VariableSizeLoaderTests.cs) | exists | - | [x] |
| 4 | ShiftJisHelperTests.cs exists | file | Glob(Era.Core.Tests/Encoding/ShiftJisHelperTests.cs) | exists | - | [x] |
| 5 | ServiceCollectionExtensionsTests.cs exists | file | Glob(Era.Core.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs) | exists | - | [x] |
| 6 | CallbackFactoriesTests.cs exists | file | Glob(Era.Core.Tests/DependencyInjection/CallbackFactoriesTests.cs) | exists | - | [x] |
| 7 | All Era.Core.Tests pass | test | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 8 | Build succeeds with zero warnings | build | dotnet build Era.Core.Tests | succeeds | - | [x] |
| 9 | Each new test file has >= 3 test methods | code | Grep(per file, see AC Details for 6 files) | gte | 3 | [x] |
| 10 | No technical debt markers in new test files | code | Grep(6 files per AC Details, "TODO|FIXME|HACK") | not_contains | - | [x] |

**Note**: 10 ACs is within the infra range (8-15). AC#1-6 verify file creation for each target class. AC#7-8 verify build/test quality. AC#9 verifies each new test file has non-trivial coverage. AC#10 verifies code quality.

### AC Details

**AC#1: ComLoaderTests.cs exists**
- Verifies unit test file created for `Era.Core/Data/YamlComLoader.cs`
- HIGH complexity: Tests must cover Load (single COM), LoadAll (directory scanning), caching behavior, and cache invalidation
- Dependencies: ComDefinitionCache (mock or real), ILogger
- Test: `Glob("Era.Core.Tests/Data/ComLoaderTests.cs")`

**AC#2: GameBaseLoaderTests.cs exists**
- Verifies unit test file created for `Era.Core/Data/YamlGameBaseLoader.cs`
- LOW complexity: Simple single-file YAML loader following same pattern as existing tested loaders (e.g., AblLoaderTests.cs)
- Test: `Glob("Era.Core.Tests/Data/GameBaseLoaderTests.cs")`

**AC#3: VariableSizeLoaderTests.cs exists**
- Verifies unit test file created for `Era.Core/Data/YamlVariableSizeLoader.cs`
- LOW complexity: Simple single-file YAML loader following same pattern as existing tested loaders
- Test: `Glob("Era.Core.Tests/Data/VariableSizeLoaderTests.cs")`

**AC#4: ShiftJisHelperTests.cs exists**
- Verifies unit test file created for `Era.Core/Encoding/ShiftJisHelper.cs`
- LOW complexity: Static utility class with IsHalfwidth and GetByteCount methods, no dependencies
- Tests should cover ASCII, known halfwidth katakana, known fullwidth characters, and boundary cases
- Test: `Glob("Era.Core.Tests/Encoding/ShiftJisHelperTests.cs")`

**AC#5: ServiceCollectionExtensionsTests.cs exists**
- Verifies unit test file created for `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`
- MEDIUM complexity: DI registration with ~150 service registrations
- Tests should verify service registration count or key service resolution (not exhaustive resolution of all 150 services)
- Test: `Glob("Era.Core.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs")`

**AC#6: CallbackFactoriesTests.cs exists**
- Verifies unit test file created for `Era.Core/DependencyInjection/CallbackFactories.cs`
- MEDIUM complexity: Factory delegates for training variable access
- Tests require mocking ITrainingVariables / ITEquipVariables interfaces
- Test: `Glob("Era.Core.Tests/DependencyInjection/CallbackFactoriesTests.cs")`

**AC#7: All Era.Core.Tests pass**
- Verifies all existing and new tests pass together
- Command: `dotnet test Era.Core.Tests/`
- Ensures new tests do not break existing tests and all new tests are GREEN

**AC#8: Build succeeds with zero warnings**
- Verifies new test files compile cleanly under TreatWarningsAsErrors=true (F708)
- Command: `dotnet build Era.Core.Tests/`
- Any warning in new test files would cause build failure

**AC#9: Each new test file has >= 3 test methods**
- Verifies non-trivial test coverage per file
- Files to check (6 total):
  - `Era.Core.Tests/Data/ComLoaderTests.cs` — expected >= 5 (Load, LoadAll, caching, invalidation, error handling)
  - `Era.Core.Tests/Data/GameBaseLoaderTests.cs` — expected >= 3 (positive path, missing file, malformed YAML)
  - `Era.Core.Tests/Data/VariableSizeLoaderTests.cs` — expected >= 3 (positive path, missing file, malformed YAML)
  - `Era.Core.Tests/Encoding/ShiftJisHelperTests.cs` — expected >= 3 (halfwidth, fullwidth, boundary)
  - `Era.Core.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs` — expected >= 3 (registration, resolution, count)
  - `Era.Core.Tests/DependencyInjection/CallbackFactoriesTests.cs` — expected >= 3 (factory creation, invocation, error)
- Method: Grep each file for `\[Fact\]|\[Theory\]` and verify count >= 3

**AC#10: No technical debt markers in new test files**
- Verifies no TODO, FIXME, or HACK comments in new test files
- Files to check: same 6 files as AC#9
- Method: `Grep("TODO|FIXME|HACK")` across all 6 new test files, expect zero matches

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature creates 6 new test files following the established testing patterns from existing loader tests (e.g., AblLoaderTests.cs). The approach is organized into three testing strategies based on component complexity:

**Strategy 1: Simple YAML Loader Tests** (GameBaseLoader, VariableSizeLoader)
- Follow the AblLoaderTests.cs pattern exactly
- Positive tests: Valid YAML file parsing with field validation
- Negative tests: Null/empty path, missing file, invalid YAML format
- DI integration tests: Service resolution and singleton verification
- Use temp file creation/cleanup in try-finally blocks

**Strategy 2: Complex YAML Loader with Caching** (ComLoader)
- Extend Strategy 1 pattern with cache-specific tests
- Test Load (single file) with cache hit/miss scenarios
- Test LoadAll (directory scanning) with multiple files
- Test cache invalidation via InvalidateCache method
- Verify integration with ComDefinitionCache (real instance, not mock)
- Test file modification time-based cache staleness detection

**Strategy 3: Utility and DI Tests** (ShiftJisHelper, ServiceCollectionExtensions, CallbackFactories)
- **ShiftJisHelper**: Pure static utility tests with boundary cases (ASCII < 0x127, halfwidth chars in HashSet, fullwidth chars)
- **ServiceCollectionExtensions**: DI registration verification via service count or selective resolution (avoid exhaustive resolution of all 150+ services)
- **CallbackFactories**: Factory registration and invocation tests with mock ITrainingVariables/ITEquipVariables

**New Directory Creation**:
- `Era.Core.Tests/Encoding/` (new directory for ShiftJisHelperTests.cs)
- `Era.Core.Tests/DependencyInjection/` (new directory for ServiceCollectionExtensionsTests.cs and CallbackFactoriesTests.cs)

**Test Quality Standards**:
- All tests inherit from BaseTestClass for DI container setup
- Use ResultAssert.AssertSuccess/AssertFailure for Result<T> validation
- Each test file has >= 3 test methods (AC#9 requirement)
- No TODO/FIXME/HACK markers (AC#10 requirement)
- All tests must pass (AC#7) and build with zero warnings (AC#8)

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core.Tests/Data/ComLoaderTests.cs` with tests for Load, LoadAll, cache hit/miss, cache invalidation, and error handling. Use real ComDefinitionCache instance (not mock) to test integration. Include DI resolution tests. Minimum 5 test methods covering: (1) Load valid YAML, (2) Load with cache hit, (3) LoadAll directory scanning, (4) InvalidateCache behavior, (5) error handling (null/missing file). |
| 2 | Create `Era.Core.Tests/Data/GameBaseLoaderTests.cs` following AblLoaderTests.cs pattern. Test positive path (valid YAML with required fields: code, version, title), negative paths (null/empty path, missing file, invalid YAML), and DI resolution. Minimum 3 test methods. |
| 3 | Create `Era.Core.Tests/Data/VariableSizeLoaderTests.cs` following AblLoaderTests.cs pattern. Test positive path (valid YAML with UPPERCASE properties like DAY, MONEY, FLAG), negative paths (null/empty path, missing file, invalid YAML), and DI resolution. Minimum 3 test methods. |
| 4 | Create `Era.Core.Tests/Encoding/ShiftJisHelperTests.cs` (new directory). Test IsHalfwidth method with: (1) ASCII characters (< 0x127), (2) known halfwidth chars from HashSet, (3) fullwidth characters. Test GetByteCount method with empty string, mixed halfwidth/fullwidth strings. Minimum 3 test methods. No DI needed (static utility class). |
| 5 | Create `Era.Core.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs` (new directory). Test AddEraCore method by: (1) verifying service collection is not empty after registration, (2) spot-checking key service resolution (e.g., IGameInitializer, IAblLoader, IVariableStore), (3) verifying singleton lifetime for key services. Avoid exhaustive resolution of all 150+ services. Minimum 3 test methods. |
| 6 | Create `Era.Core.Tests/DependencyInjection/CallbackFactoriesTests.cs` (new directory). Test AddTrainingCallbacks method by: (1) verifying factory registration (Func<CharacterId, CupIndex, int>, Func<CharacterId, int, bool>, Func<CharacterId, JuelIndex, int>), (2) testing factory invocation with mock ITrainingVariables/ITEquipVariables/IJuelVariables, (3) verifying Result unwrapping behavior (Success → value, Failure → default). Minimum 3 test methods. |
| 7 | Verify with `dotnet test Era.Core.Tests/` after all test files are created. All existing tests + 6 new test files must pass (GREEN). If any test fails, fix until GREEN. |
| 8 | Verify with `dotnet build Era.Core.Tests/`. Build must succeed with zero warnings. TreatWarningsAsErrors=true (F708) will fail build on any warning in new test files. |
| 9 | Each of the 6 new test files must contain >= 3 test methods (xUnit `[Fact]` or `[Theory]` attributes). Verified by grepping each file for `\[Fact\]|\[Theory\]` and counting matches. ComLoaderTests.cs expected >= 5 methods due to complexity. |
| 10 | Verified by grepping all 6 new test files for pattern `TODO|FIXME|HACK`. Zero matches required. No deferred work markers allowed in final test code. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **ComLoader cache testing** | (A) Mock ComDefinitionCache, (B) Use real ComDefinitionCache instance, (C) Test cache separately only | B - Use real ComDefinitionCache | ComDefinitionCacheTests.cs already tests cache in isolation. ComLoaderTests.cs should verify integration between loader and cache (cache hit/miss behavior with real file I/O). Mocking would not verify the integration contract. |
| **ServiceCollectionExtensions verification strategy** | (A) Resolve all 150+ services exhaustively, (B) Service count verification, (C) Spot-check key services | C - Spot-check key services | Exhaustive resolution (A) is brittle (fails if any service has unsatisfied dependency) and slow. Service count verification (B) is fragile (changes with every service addition). Spot-checking key services verifies DI wiring works without brittleness. |
| **ShiftJisHelper test scope** | (A) Exhaustive character set validation, (B) Boundary case testing (ASCII, known halfwidth, known fullwidth) | B - Boundary case testing | Exhaustive validation (A) would require testing all ~150 HalfwidthChars entries individually (low value, high maintenance). Boundary case testing (B) verifies the logic (< 0x127 check, HashSet membership) without exhaustive enumeration. |
| **Test data location** | (A) Inline YAML strings in tests, (B) External YAML files in TestData/, (C) Combination | A - Inline YAML strings | Existing loader tests (AblLoaderTests.cs) use inline YAML strings written to temp files. This approach keeps tests self-contained and readable. External files (B) would spread test logic across multiple files. |
| **Directory creation timing** | (A) Pre-create Encoding/ and DependencyInjection/ directories, (B) Create during test file creation | B - Create during test file creation | Visual Studio and dotnet test automatically create missing directories when files are written. No manual pre-creation needed. Directories will be created as part of file write operations. |
| **CallbackFactories mocking strategy** | (A) Mock all 3 interfaces (ITrainingVariables, ITEquipVariables, IJuelVariables), (B) Use real VariableStore, (C) Manual stub implementation | A - Mock all 3 interfaces | Callback factory tests should verify factory registration and invocation logic, not VariableStore behavior (already tested in VariableStoreTests.cs). Mocking (A) isolates the factory logic. Using real VariableStore (B) would require complex setup. Manual stubs (C) are more verbose than mocks. Use NSubstitute (already in test project) or Moq. |

### Test File Structure

Each test file follows this structure (adapted from AblLoaderTests.cs):

```csharp
using Era.Core.Data;
using Era.Core.Data.Models;
using Era.Core.Tests.Assertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Era.Core.Tests.{Namespace}; // Data, Encoding, or DependencyInjection

/// <summary>
/// Unit tests for {ClassName} (Feature 718 AC#{N}).
/// {Brief description of what is being tested}
/// </summary>
public class {ClassName}Tests : BaseTestClass
{
    #region Positive Tests
    [Fact]
    public void {Method}_ValidInput_ReturnsSuccess() { ... }
    #endregion

    #region Negative Tests
    [Fact]
    public void {Method}_NullPath_ReturnsFailure() { ... }

    [Fact]
    public void {Method}_MissingFile_ReturnsFailure() { ... }

    [Fact]
    public void {Method}_InvalidYaml_ReturnsFailure() { ... }
    #endregion

    #region DI Integration Tests (if applicable)
    [Fact]
    public void DIContainer_CanResolve{Interface}() { ... }

    [Fact]
    public void DIContainer_ReturnsSingletonInstance() { ... }
    #endregion
}
```

**File-specific notes**:

1. **ComLoaderTests.cs**: Add `#region Cache Behavior Tests` with tests for cache hit, cache miss, cache staleness (file modification time), and InvalidateCache method.

2. **ShiftJisHelperTests.cs**: No BaseTestClass inheritance needed (static utility class, no DI). Use direct `Assert` calls.

3. **ServiceCollectionExtensionsTests.cs**: Test uses `new ServiceCollection()` pattern (not BaseTestClass.Services) to verify registration method directly.

4. **CallbackFactoriesTests.cs**: Create mock instances of ITrainingVariables/ITEquipVariables/IJuelVariables. Verify factory functions are registered as Func<CharacterId, CupIndex, int>, Func<CharacterId, int, bool>, Func<CharacterId, JuelIndex, int> respectively.

### Implementation Notes

**Test Data Cleanup**:
All tests that create temp files must use try-finally blocks for cleanup:
```csharp
var testYamlPath = GetTestDataPath("test.yaml");
File.WriteAllText(testYamlPath, yamlContent);
try
{
    // Act & Assert
}
finally
{
    if (File.Exists(testYamlPath))
        File.Delete(testYamlPath);
}
```

**ComLoader LoadAll Test Pattern**:
```csharp
// Create temp directory with multiple YAML files
var testDir = Path.Combine(TestDataPath, "com_test_dir");
Directory.CreateDirectory(testDir);
try
{
    File.WriteAllText(Path.Combine(testDir, "com1.yaml"), yaml1);
    File.WriteAllText(Path.Combine(testDir, "com2.yaml"), yaml2);

    var loader = Services.GetRequiredService<IComLoader>();
    var result = loader.LoadAll(testDir);

    var definitions = ResultAssert.AssertSuccess(result);
    Assert.Equal(2, definitions.Count);
}
finally
{
    if (Directory.Exists(testDir))
        Directory.Delete(testDir, recursive: true);
}
```

**VariableSizeConfig Test YAML Example**:
```yaml
DAY: 1
MONEY: 1
FLAG: 100
SAVESTR: 50
BASE: 10
DITEMTYPE: [10, 10]
```

**GameBaseConfig Test YAML Example**:
```yaml
code: "eraKoumakan"
version: "1.0.0"
title: "era紅魔館"
author: "TestAuthor"
year: "2024"
```

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create Era.Core.Tests/Data/ComLoaderTests.cs with tests for Load, LoadAll, cache hit/miss, cache invalidation, and error handling (>=5 test methods) | [x] |
| 2 | 2 | Create Era.Core.Tests/Data/GameBaseLoaderTests.cs following AblLoaderTests.cs pattern (>=3 test methods) | [x] |
| 3 | 3 | Create Era.Core.Tests/Data/VariableSizeLoaderTests.cs following AblLoaderTests.cs pattern (>=3 test methods) | [x] |
| 4 | 4 | Create Era.Core.Tests/Encoding/ShiftJisHelperTests.cs for static utility tests (>=3 test methods) | [x] |
| 5 | 5 | Create Era.Core.Tests/DependencyInjection/ServiceCollectionExtensionsTests.cs for DI registration verification (>=3 test methods) | [x] |
| 6 | 6 | Create Era.Core.Tests/DependencyInjection/CallbackFactoriesTests.cs for factory registration and invocation tests (>=3 test methods) | [x] |
| 7 | 7 | Verify all Era.Core.Tests pass (all existing tests + 6 new test files must pass GREEN) | [x] |
| 8 | 8 | Verify build succeeds with zero warnings (TreatWarningsAsErrors enforcement) | [x] |
| 9 | 9 | Verify each new test file has >= 3 test methods (Grep for [Fact] or [Theory] attributes in 6 files) | [x] |
| 10 | 10 | Verify no technical debt markers in new test files (Grep for TODO|FIXME|HACK in 6 files) | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Tasks | Input | Output |
|-------|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T6 | Technical Design test file specifications | 6 new test files created |
| 2 | ac-tester | haiku | T7 | `dotnet test Era.Core.Tests/` | All tests pass (GREEN) |
| 3 | ac-tester | haiku | T8 | `dotnet build Era.Core.Tests/` | Build succeeds with zero warnings |
| 4 | ac-tester | haiku | T9 | Grep each of 6 files for `\[Fact\]|\[Theory\]` | Each file has >= 3 test methods |
| 5 | ac-tester | haiku | T10 | Grep 6 files for `TODO|FIXME|HACK` | Zero matches (no technical debt markers) |

**Constraints** (from Technical Design):

1. **Test Pattern Adherence**: All loader tests (T1-T3) must follow AblLoaderTests.cs pattern exactly (positive/negative/DI tests, try-finally cleanup)
2. **Cache Integration**: ComLoaderTests.cs (T1) must use real ComDefinitionCache instance (not mock) to verify integration behavior
3. **Directory Creation**: New directories (Era.Core.Tests/Encoding/, Era.Core.Tests/DependencyInjection/) will be created automatically during file write operations
4. **DI Test Strategy**: ServiceCollectionExtensionsTests.cs (T5) must spot-check key services (IGameInitializer, IAblLoader, IVariableStore) rather than exhaustive resolution of all 150+ services
5. **Mocking Strategy**: CallbackFactoriesTests.cs (T6) must use NSubstitute or Moq to mock ITrainingVariables/ITEquipVariables/IJuelVariables interfaces
6. **Test Quality Standards**: All new tests inherit from BaseTestClass (except ShiftJisHelperTests.cs which is static utility), use ResultAssert.AssertSuccess/AssertFailure for Result<T> validation
7. **Minimum Coverage**: Each new test file must have >= 3 test methods (AC#9); ComLoaderTests.cs expected >= 5 due to complexity

**Pre-conditions**:

- F716 (Era.Core Test Coverage Hardening) is [DONE] — no blocking dependencies
- Existing test infrastructure (BaseTestClass, ResultAssert, xUnit, coverlet.collector) is in place
- AblLoaderTests.cs exists as reference pattern for loader tests
- YamlDotNet, Microsoft.Extensions.DependencyInjection, NSubstitute/Moq are already referenced in Era.Core.Tests

**Success Criteria**:

1. All 6 new test files created with correct namespaces (Data, Encoding, DependencyInjection)
2. All existing tests + 6 new test files pass (AC#7: `dotnet test Era.Core.Tests/` succeeds)
3. Build succeeds with zero warnings (AC#8: TreatWarningsAsErrors enforcement)
4. Each new test file has >= 3 test methods (AC#9: minimum coverage threshold)
5. No technical debt markers in new test files (AC#10: zero TODO/FIXME/HACK)
6. Test coverage for 6 untested files achieved:
   - YamlComLoader.cs (HIGH complexity: Load, LoadAll, caching, invalidation)
   - YamlGameBaseLoader.cs (LOW complexity: positive/negative/DI tests)
   - YamlVariableSizeLoader.cs (LOW complexity: positive/negative/DI tests)
   - ShiftJisHelper.cs (LOW complexity: boundary case tests)
   - ServiceCollectionExtensions.cs (MEDIUM complexity: spot-check key services)
   - CallbackFactories.cs (MEDIUM complexity: factory registration/invocation with mocks)

**Rollback Plan**:

If issues arise after deployment:

1. **Revert commit**: `git revert <commit-hash>` to remove all 6 new test files
2. **Notify user**: Report the specific issue encountered (test failures, build errors, integration problems)
3. **Create follow-up feature**: New feature-{ID}.md with additional investigation including:
   - Root cause analysis of why tests failed or caused issues
   - Review of test strategy (e.g., ComLoader cache integration vs. mocking)
   - Review of DI test approach (spot-checking vs. alternative verification)
   - Re-assessment of test data patterns and cleanup strategies
4. **Preserve evidence**: Save test failure logs to `Game/logs/debug/` for investigation
5. **Dependencies**: No downstream features depend on these tests, so rollback has minimal impact

**Execution Order**:

Phase 1 (T1-T6) creates all test files in dependency order:
1. T4 first (Encoding/ directory creation, no dependencies on other tests)
2. T5-T6 next (DependencyInjection/ directory creation, no dependencies)
3. T2-T3 next (Data/ directory exists, simple loader tests)
4. T1 last (Data/ directory exists, complex loader test, depends on understanding from T2-T3 patterns)

Phase 2-5 (T7-T10) verify quality in strict sequence:
1. T7: All tests pass (prerequisite for all other verifications)
2. T8: Build succeeds (TreatWarningsAsErrors enforcement)
3. T9: Minimum test method count per file (coverage threshold)
4. T10: No technical debt markers (code quality)

**Error Handling**:

- **Phase 1 (T1-T6) failures**: If test file creation fails, STOP → report to user → investigate before proceeding
- **Phase 2 (T7) failures**: If any test fails, fix until GREEN before proceeding to T8-T10
- **Phase 3 (T8) failures**: If build warnings occur, fix warnings until zero before proceeding to T9-T10
- **Phase 4 (T9) failures**: If any test file has < 3 test methods, add tests until threshold met
- **Phase 5 (T10) failures**: If technical debt markers found, remove or resolve before completion
- **3 consecutive failures**: STOP → escalate to user (Escalation Policy)

---

## 残課題 (Deferred Items)

| Item | Destination | Note |
|------|-------------|------|
| (none identified) | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Review Notes

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|

---

## Execution Log

---

## Links
- [feature-716.md](feature-716.md) - Parent initiative
- [feature-713.md](feature-713.md) - YAML variable definitions — created untested YamlGameBaseLoader/YamlVariableSizeLoader
- [feature-583.md](feature-583.md) - Phase 17 Data Migration — origin of most loader tests
- [feature-580.md](feature-580.md) - COM Loader Performance Optimization — added caching to YamlComLoader
- [feature-499.md](feature-499.md) - Test strategy design — patterns to follow
- [feature-427.md](feature-427.md) - ShiftJisHelper extraction — created Encoding/ShiftJisHelper.cs without dedicated tests
- [feature-717.md](feature-717.md) - Tool test coverage — parallel successor of F716 for tool tests
- [CLAUDE.md Test Coverage Policy](../../CLAUDE.md#test-coverage-policy)
