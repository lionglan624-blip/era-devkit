# Feature 359: Test Structure & Coverage Extension

## Status: [DONE]

## Type: infra

## Created: 2026-01-05

---

## Summary

Extend Era.Core.Tests project (created by F350 Task 0) with test organization, coverage targets, shared utilities, and best practices. Establishes foundation for Phase 2 test migration and KojoComparer integration.

**Note**: Era.Core.Tests project already exists with 6 KojoEngine tests. This feature focuses on structure extension, NOT project creation.

---

## Background

### Philosophy (Mid-term Vision)

**Test-First Migration**: Before migrating game logic (Phase 3+), establish robust test infrastructure to catch regressions. Clear test organization and shared utilities ensure maintainability as test count grows from 175 C# tests to 400+ with Phase 2 migration.

### Problem (Current Issue)

Phase 2 Test Infrastructure Planning (F358) identified:
- Era.Core.Tests has 6 tests (created by F350)
- Test migration will add 160+ ERB unit tests → C# tests
- 391 kojo scenario tests need KojoComparer integration
- No shared test utilities (fixtures, mocks, helpers)
- No coverage targets defined
- No test naming conventions documented

Without structure:
- Duplicate test setup code across test classes
- Inconsistent naming hampers filtering/categorization
- Coverage measurement undefined
- Maintenance burden increases with scale

### Goal (What to Achieve)

Establish test structure foundation:
1. Define test organization and naming conventions
2. Create shared test utilities (BaseTestClass, TestHelpers, MockGameContext)
3. Set coverage targets for Era.Core and tools
4. Document testing best practices for future migrations

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 0 | Baseline coverage documented | code | Grep(Era.Core.Tests/README.md) | contains | Coverage Baseline | [x] |
| 1 | BaseTestClass created | file | Glob | exists | Era.Core.Tests/BaseTestClass.cs | [x] |
| 2 | TestHelpers utility created | file | Glob | exists | Era.Core.Tests/TestHelpers.cs | [x] |
| 3 | MockGameContext created | file | Glob | exists | Era.Core.Tests/Mocks/MockGameContext.cs | [x] |
| 4 | Test organization doc | file | Glob | exists | Era.Core.Tests/README.md | [x] |
| 5 | Coverage target config | code | Grep(Era.Core.Tests.csproj) | contains | coverlet.collector | [x] |
| 6 | All tests pass | test | dotnet test | succeeds | - | [x] |
| 7 | Coverage command succeeds | test | dotnet test --collect | succeeds | - | [x] |
| 8 | Coverage report file exists | file | Glob | exists | Era.Core.Tests/TestResults/**/coverage.cobertura.xml | [x] |

### AC Details

**AC#0 Test**: Verify `Era.Core.Tests/README.md` contains "Coverage Baseline" section with measured coverage values. Task 0 performs the measurement; this AC verifies documentation of results.

**AC#1 Test**: Verify `Era.Core.Tests/BaseTestClass.cs` exists with common setup/teardown methods for derived test classes.

**AC#2 Test**: Verify `Era.Core.Tests/TestHelpers.cs` exists with utility methods for YAML fixture loading, assertion helpers, and test-specific string normalization (distinct from F360 OutputNormalizer which handles ERB/YAML comparison normalization).

**AC#3 Test**: Verify `Era.Core.Tests/Mocks/MockGameContext.cs` exists providing configurable mock for testing condition evaluation without full game state. MockGameContext wraps the existing `Dictionary<string, object>` context pattern used by KojoEngine (see KojoEngineTests.cs lines 58-60), NOT the IGameContext interface (which exists in Era.Core but is not used by KojoEngine).

**AC#4 Test**: Verify `Era.Core.Tests/README.md` documents:
- Test organization structure
- Naming conventions (e.g., `{Class}_{Method}_{Scenario}_Test`)
- Test categories (Unit/Integration/Schema)
- How to run tests with filters

**AC#5 Test**: Verify `coverlet.collector` package (version 6.0.2 or latest stable) added to Era.Core.Tests.csproj for code coverage measurement.

**AC#6 Test**: `dotnet test Era.Core.Tests/` succeeds (all 6 existing tests + new structure tests pass).

**AC#7 Test**: `dotnet test Era.Core.Tests/ --collect:"XPlat Code Coverage"` command succeeds.

**AC#8 Test**: Coverage report file (coverage.cobertura.xml) exists in TestResults/ directory after running coverage command. (Depends on AC#7)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | 0 | Measure current coverage baseline for Era.Core (KojoEngine.cs) and document in README.md | [x] |
| 1 | 1 | Create BaseTestClass with common setup/teardown (TestData path initialization, common assertions) | [x] |
| 2 | 2 | Create TestHelpers with YAML loading, string normalization, assertion helpers | [x] |
| 3 | 3 | Create MockGameContext in Era.Core.Tests/Mocks/ with TALENT/CFLAG state configuration | [x] |
| 4 | 5 | Add coverlet.collector package to Era.Core.Tests.csproj, configure coverage thresholds | [x] |
| 5 | 4 | Write Era.Core.Tests/README.md documenting structure, naming conventions, coverage targets | [x] |
| 6 | 6 | Run full test suite, verify all tests pass | [x] |
| 7 | 7 | Run test suite with coverage collection enabled | [x] |
| 8 | 8 | Verify coverage report file (coverage.cobertura.xml) generated | [x] |

---

## Test Coverage Targets

### F359 Scope (Era.Core)

| Component | Current Coverage | Phase 2 Target | Notes |
|-----------|:---------------:|:--------------:|-------|
| Era.Core/KojoEngine.cs | 70.1% line / 55.76% branch | 95% | 6 tests cover core paths; EvaluateRequirement is 20% (gap) |
| Era.Core/KojoEngine.cs (condition methods) | 70.1% line | 95% | EvaluateConditionDict (85.18%), EvaluateRequirement (20%) |

### Future Reference (F362 Test Migration scope)

| Component | Current Coverage | Phase 2 Target | Notes |
|-----------|:---------------:|:--------------:|-------|
| tools/ErbParser/ | ~70% | 85% | Parser foundation (F346) |
| tools/ErbToYaml/ | ~60% | 85% | Converter + pilot tests (F349, F351) |

**Measurement**: Use coverlet with xUnit integration. Report format: Cobertura XML for CI integration (F363).

---

## Test Organization

### Directory Structure

```
Era.Core.Tests/
├── BaseTestClass.cs              # Common test infrastructure
├── TestHelpers.cs                # Utility methods
├── Mocks/
│   ├── MockGameContext.cs        # Mock game state
│   └── MockConditionEvaluator.cs # Mock condition evaluator (future)
├── KojoEngineTests.cs            # Existing renderer tests (6 tests)
├── (future) ConditionEvaluatorTests.cs  # Phase 2+ when condition logic is extracted
├── IntegrationTests/             # Placeholder - created when F362 approved
│   └── PipelineTests.cs          # Placeholder - created when F362 approved
├── TestData/                     # Test fixtures (existing)
│   └── sample-dialogue.yaml
└── README.md                     # Documentation
```

### Naming Conventions

**Test Class**: `{ClassName}Tests.cs` (e.g., `KojoEngineTests.cs`)

**Test Method**: `{Method}_{Scenario}_{ExpectedResult}` (e.g., `LoadYaml_ValidFile_Succeeds`)

**Test Categories** (via xUnit Trait):
- `[Trait("Category", "Unit")]` - Unit tests (isolated component)
- `[Trait("Category", "Integration")]` - Integration tests (multiple components)
- `[Trait("Category", "Schema")]` - Schema validation tests

**Filter Examples**:
```bash
# Run only unit tests
dotnet test --filter "Category=Unit"

# Run specific class
dotnet test --filter "FullyQualifiedName~KojoEngineTests"

# Run coverage on unit tests only
dotnet test --filter "Category=Unit" --collect:"XPlat Code Coverage"
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F350 | Created Era.Core.Tests project (Task 0) |
| Predecessor | F358 | Phase 2 planning, defines structure requirements |
| Successor | F360 | KojoComparer will use MockGameContext and TestHelpers |
| Successor | F361 | Schema Validator will use TestHelpers for YAML fixtures |
| Successor | F362 | Test Migration will follow conventions from this feature (not yet created) |

---

## Links

- [feature-350.md](feature-350.md) - YAML Dialogue Renderer (created Era.Core.Tests)
- [feature-358.md](feature-358.md) - Phase 2 Planning (defines test infrastructure needs)
- [feature-360.md](feature-360.md) - KojoComparer Tool (uses test utilities)
- [feature-361.md](feature-361.md) - Schema Validator Integration (uses test utilities)
- F362 - Test Migration (not yet created, will follow conventions from this feature)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 2 Tasks 1-2 (lines 819-820)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | implementer | Task 2 from F358 | PROPOSED |
| 2026-01-05 20:49 | START | implementer | Task 4 | - |
| 2026-01-05 20:49 | END | implementer | Task 4 | SUCCESS |
| 2026-01-05 20:51 | START | implementer | Task 1 | - |
| 2026-01-05 20:51 | END | implementer | Task 1 | SUCCESS |
| 2026-01-05 20:52 | START | implementer | Task 2 | - |
| 2026-01-05 20:52 | END | implementer | Task 2 | SUCCESS |
| 2026-01-05 20:51 | START | implementer | Task 3 | - |
| 2026-01-05 20:51 | END | implementer | Task 3 | SUCCESS |
| 2026-01-05 20:52 | START | implementer | Task 0 | - |
| 2026-01-05 20:52 | END | implementer | Task 0 | SUCCESS |
