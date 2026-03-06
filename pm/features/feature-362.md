# Feature 362: Test Migration

## Status: [DONE]

> **FL Revision (2026-01-06)**:
> - Iteration 1: test locations corrected (engine.Tests, not Era.Core.Tests), AC#4 clarified, F360 DONE
> - Iteration 3: ERB test file line counts corrected (F358 had ~3,147 → actual ~225)
> - Iteration 4: AC#2-4 clarified with explicit create/enable requirements and binary pass criteria; AC#4 changed from BatchEquivalence to PilotEquivalence (existing F360 tests)

## Type: infra

## Created: 2026-01-05

---

## Summary

Migrate existing ERB test files (~225 lines total, ~150 lines excluding TEST_IMAGE.ERB deferred to Phase 12) to C# MSTest. Ensures test coverage continuity during full C# migration (Phase 3+).

**Output**: C# MSTest test classes in engine.Tests/ and tools/*Tests/ projects

---

## Background

### Philosophy (Mid-term Vision)

**Test-First Migration**: Migrate test infrastructure before game logic to catch regressions. ERB tests must have C# equivalents before ERB removal in Phase 12+.

### Problem (Current Issue)

Test migration requirements (corrected from F358 estimates):

| Current Test Type | Count | Complexity | Status |
|-------------------|------:|:----------:|--------|
| Game/ERB/ TEST_*.ERB files | ~225 lines | LOW | ERB-only (6 files, trivial) |
| Game/tests/ac/kojo/ JSON scenarios | 391 files | MEDIUM | Headless mode |
| Game/tests/regression/ JSON scenarios | 24 files | MEDIUM | Headless mode |

Without migration:
- ERB tests become obsolete when ERB removed
- No C# test coverage for migrated functionality
- Regression detection lost during Phase 3+ migration

### Goal (What to Achieve)

1. Audit existing ERB test files and categorize by migration target
2. Convert critical ERB unit tests to C# MSTest
3. Convert flow/regression tests to C# integration tests
4. Integrate kojo tests with KojoComparer (F360)

**Scope**: Focus on high-value tests first. Full coverage is Phase 12 scope.

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Test audit report created | file | Glob | exists | Game/agents/designs/test-migration-audit.md | [x] |
| 2 | MockRandom tests created | test | dotnet test | succeeds | engine.Tests MockRandomTests pass (≥2 tests) | [x] |
| 3 | FlowRand tests created | test | dotnet test | succeeds | engine.Tests FlowRandTests pass (≥2 tests) | [x] |
| 4 | PilotEquivalence tests enabled | test | dotnet test | succeeds | KojoComparer.Tests PilotEquivalence pass (≥1 test) | [x] |
| 5 | Test count parity report | file | Grep | contains | "Coverage:" in test-migration-audit.md | [x] |
| 6 | F363 reactivated | file | Grep | contains | "Status: \\[PROPOSED\\]" in feature-363.md | [x] |

### AC Details

**AC#1 Test**: Verify audit file exists
```bash
dir Game\agents\designs\test-migration-audit.md
```
Expected: File exists with categorized ERB test inventory

**AC#2 Test**: `dotnet test engine.Tests/ --filter "FullyQualifiedName~MockRandom"`
- Source: TEST_MOCK_RAND.ERB (10 lines) and TEST_MOCK_RAND_EXHAUST.ERB (16 lines) - trivial RAND call tests
- Target: Create engine.Tests/MockRandomTests.cs (no existing coverage found)
- Verify: At least 2 test methods covering: (1) single RAND call with mock injection, (2) RAND exhaustion fallback
- Pass criteria: `dotnet test --filter MockRandom` exits 0 with ≥2 tests passing

**AC#3 Test**: `dotnet test engine.Tests/ --filter "FullyQualifiedName~FlowRand"`
- Source: TEST_FLOW_RAND.ERB (5 lines) and TEST_FLOW_RAND_MULTI.ERB (9 lines) - trivial multi-RAND tests
- Target: Create engine.Tests/FlowRandTests.cs (FlowTestExpectVerificationTests.cs tests expect framework, not RAND behavior)
- Verify: At least 2 test methods covering: (1) single RAND in flow, (2) multi-RAND sequence in flow
- Pass criteria: `dotnet test --filter FlowRand` exits 0 with ≥2 tests passing

**AC#4 Test**: `dotnet test tools/KojoComparer.Tests/ --filter "FullyQualifiedName~PilotEquivalence"`
- Source: PilotEquivalenceTests.cs exists (F360 AC#8, 4 tests with Skip="Requires headless mode")
- Target: Enable PilotEquivalenceTests by removing Skip attributes or creating alternative validation
- Uses F351 pilot YAML at `tools/ErbToYaml.Tests/TestOutput/meirin_com0.yaml`
- Pass criteria: `dotnet test --filter PilotEquivalence` exits 0 with ≥1 test passing (not skipped)

**AC#5 Test**: Verify coverage report in audit file
```bash
grep "Coverage:" Game\agents\designs\test-migration-audit.md
```
Expected: Shows migrated/total test count

**AC#6 Test**: Verify F363 reactivated with PROPOSED status
```bash
grep "Status: \[PROPOSED\]" Game\agents\feature-363.md
```
Expected: F363 reactivated - cancellation block removed, spec revised based on Phase 2 implementation results (see F363 Reactivation Instructions)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Audit Game/ERB/*.ERB test files and Game/tests/ directories, categorize by migration target | [x] |
| 2 | 2 | Create engine.Tests/MockRandomTests.cs with mock RAND injection and exhaustion tests | [x] |
| 3 | 3 | Create engine.Tests/FlowRandTests.cs with single and multi-RAND flow tests | [x] |
| 4 | 4 | Enable PilotEquivalenceTests.cs (remove Skip) or create alternative validation that passes | [x] |
| 5 | 5 | Update test-migration-audit.md with coverage metrics | [x] |
| 6 | 6 | Reactivate F363: review Phase 2 results, revise spec based on implementation experience, update status to PROPOSED | [x] |

---

## ERB Test File Inventory

Verified line counts (F358 estimates were inaccurate):

| File | Lines | Purpose | Migration Target |
|------|------:|---------|------------------|
| test-kojo-k1.ERB | 110 | K1 kojo unit test | Keep as headless scenario reference |
| TEST_FLOW_RAND.ERB | 5 | Single RAND call | engine.Tests/FlowRandTests.cs (Task 3) |
| TEST_FLOW_RAND_MULTI.ERB | 9 | Multi-RAND call | engine.Tests/FlowRandTests.cs (Task 3) |
| TEST_IMAGE.ERB | 75 | Image display test | Unity integration (defer to Phase 12) |
| TEST_MOCK_RAND.ERB | 10 | Mock random test | engine.Tests/MockRandomTests.cs (Task 2) |
| TEST_MOCK_RAND_EXHAUST.ERB | 16 | Exhaustion test | engine.Tests/MockRandomTests.cs (Task 2) |
| **Total** | **~225** | - | Audit and verify coverage |

**Migration Priority**:
1. **HIGH**: MockRandom, Flow tests (core functionality)
2. **MEDIUM**: Kojo integration with KojoComparer
3. **LOW**: TEST_IMAGE.ERB (Unity-specific, defer to Phase 12)

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F358 | Phase 2 Planning defines test migration scope |
| Predecessor | F359 | Test Structure provides Era.Core.Tests foundation |
| Predecessor | F360 | KojoComparer required for kojo test integration (AC#4) |
| Predecessor | F361 | Schema Validator for YAML test fixtures |
| **Reactivates** | F363 | Phase 3 System Infrastructure Planning (Task 6) |

**Dependency Chain**:
```
F358 (Planning) → F359 (Test Structure) → F360 (KojoComparer) → F362 (Test Migration) → [Reactivates F363]
                                        → F361 (Schema Validator) ↗
```

**F360 Predecessor (DONE)**: KojoComparer available for AC#4 integration.

**Reactivates**: F362 Task 6 reactivates F363 (Phase 3 Planning) after Phase 2 Test Infrastructure is complete. Spec must be revised based on actual implementation experience.

---

## Links

- [feature-358.md](feature-358.md) - Phase 2 Planning (defines test migration requirements)
- [feature-359.md](feature-359.md) - Test Structure (provides test project foundation)
- [feature-360.md](feature-360.md) - KojoComparer (required for kojo test integration)
- [feature-361.md](feature-361.md) - Schema Validator (validates test fixtures)
- [feature-363.md](feature-363.md) - Phase 3 Planning (reactivated by Task 6, revise based on Phase 2 results)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 2 Task 4

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | opus | Created from F358 Phase 2 Task 4 | PROPOSED |
| 2026-01-06 05:17 | START | implementer | Task 1 | - |
| 2026-01-06 05:17 | END | implementer | Task 1 | SUCCESS |
| 2026-01-06 05:23 | START | implementer | Task 2 | - |
| 2026-01-06 05:23 | END | implementer | Task 2 | SUCCESS |
| 2026-01-06 05:25 | START | implementer | Task 3 | - |
| 2026-01-06 05:25 | END | implementer | Task 3 | SUCCESS |
| 2026-01-06 05:30 | START | implementer | Task 4 | - |
| 2026-01-06 05:30 | END | implementer | Task 4 | SUCCESS |
| 2026-01-06 05:32 | START | implementer | Task 5 | - |
| 2026-01-06 05:32 | END | implementer | Task 5 | SUCCESS |
| 2026-01-06 05:35 | START | implementer | Task 6 | - |
| 2026-01-06 05:35 | END | implementer | Task 6 | SUCCESS |
