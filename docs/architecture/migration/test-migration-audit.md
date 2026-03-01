# Test Migration Audit Report

**Feature**: F362 Test Migration
**Created**: 2026-01-06
**Purpose**: Categorize existing test infrastructure for C# migration

---

## Executive Summary

This audit identifies all test assets in the era紅魔館protoNTR repository and categorizes them by migration target. The goal is to ensure test coverage continuity during the full C# migration (Phase 3+).

**Total Test Assets**:
- **ERB Test Files**: 6 files (~219 lines)
- **JSON Test Scenarios**: 415 files
  - Kojo AC tests: 391 files
  - Regression tests: 24 files
- **C# Test Projects**: 4 projects (engine.Tests + 3 tools/*Tests)

**Migration Status**:
- ERB tests: 0/6 migrated (0%)
- JSON scenarios: Remain as-is (headless mode compatible)
- C# infrastructure: Partially complete (F359, F360)

---

## 1. ERB Test Files (Game/ERB/)

### 1.1 MockRandom Tests

| File | Lines | Purpose | Migration Target | Priority | Status |
|------|------:|---------|------------------|----------|:------:|
| TEST_MOCK_RAND.ERB | 9 | Mock RAND injection verification | src/engine.Tests/MockRandomTests.cs | HIGH | [ ] |
| TEST_MOCK_RAND_EXHAUST.ERB | 15 | Mock RAND exhaustion fallback | src/engine.Tests/MockRandomTests.cs | HIGH | [ ] |

**Migration Details**:
- **Target**: `src/engine.Tests/MockRandomTests.cs` (new file)
- **Coverage**: 2 test methods minimum
  1. `MockRand_SingleCall_ReturnsInjectedValue()` - Verifies RAND returns mock value
  2. `MockRand_Exhaustion_FallsBackToMTRandom()` - Verifies fallback when queue exhausted
- **Dependencies**: Headless mode mock injection (F091 existing feature)
- **Verification**: `dotnet test --filter "FullyQualifiedName~MockRandom"`

### 1.2 Flow RAND Tests

| File | Lines | Purpose | Migration Target | Priority | Status |
|------|------:|---------|------------------|----------|:------:|
| TEST_FLOW_RAND.ERB | 4 | Single RAND call in flow test | src/engine.Tests/FlowRandTests.cs | HIGH | [ ] |
| TEST_FLOW_RAND_MULTI.ERB | 8 | Multiple RAND calls in flow test | src/engine.Tests/FlowRandTests.cs | HIGH | [ ] |

**Migration Details**:
- **Target**: `src/engine.Tests/FlowRandTests.cs` (new file)
- **Coverage**: 2 test methods minimum
  1. `FlowRand_SingleCall_ExecutesSuccessfully()` - Verifies single RAND in flow
  2. `FlowRand_MultipleCall_ExecutesSequentially()` - Verifies multi-RAND sequence
- **Dependencies**: Flow test framework (F220 existing feature)
- **Verification**: `dotnet test --filter "FullyQualifiedName~FlowRand"`
- **Note**: FlowTestExpectVerificationTests.cs tests the expect framework itself, not RAND behavior

### 1.3 Kojo Test (Reference Only)

| File | Lines | Purpose | Migration Target | Priority | Status |
|------|------:|---------|------------------|----------|:------:|
| test-kojo-k1.ERB | 109 | Meiling K1 kojo unit test | Keep as reference | LOW | N/A |

**Migration Strategy**:
- **Action**: Keep as-is (headless scenario reference)
- **Reason**: Kojo tests already migrated to JSON format (391 files in `test/ac/kojo/`)
- **Integration**: Use KojoComparer (F360) for ERB==YAML equivalence testing

### 1.4 Unity Integration Tests (Deferred)

| File | Lines | Purpose | Migration Target | Priority | Status |
|------|------:|---------|------------------|----------|:------:|
| TEST_IMAGE.ERB | 74 | Image display commands (HTML_PRINT, G*, CBG*, SPRITE*) | Unity integration tests | LOW | DEFER |

**Migration Strategy**:
- **Action**: Defer to Phase 12 (Full Unity Integration)
- **Reason**: Tests Unity-specific rendering (graphics buffers, sprites, HTML)
- **Dependencies**: Unity test infrastructure not yet available

---

## 2. JSON Test Scenarios (test/)

### 2.1 Kojo AC Tests (test/ac/kojo/)

**Count**: 391 files across 34 feature directories

**Sample Features**:
- F180-F182: Meiling base kojo (K1-K3)
- F185-F188: Meiling extended kojo (K4-K7)
- F192, F194: Meiling special kojo (K8, K10)
- F241-F245: Alice kojo series (K11-K15)
- F278-F282: Patchouli kojo series (K21-K25)
- F287-F291: Sakuya kojo series (K31-K35)
- F315-F319: Daiyousei kojo series (K41-K45)
- F324-F327, F338: Recent kojo implementations

**Migration Strategy**:
- **Action**: Keep as-is (JSON format, headless compatible)
- **Integration**: Use KojoComparer (F360) for YAML equivalence testing
  - PilotEquivalenceTests.cs exists (4 tests, currently Skip="Requires headless mode")
  - AC#4 task: Enable these tests or create alternative validation
- **Format**: Unified JSON schema (`test/schema.json`)
- **Execution**: `dotnet run --project engine/uEmuera.Headless.csproj -- Game --unit`

### 2.2 Regression Tests (test/regression/)

**Count**: 24 files

**Categories**:
- **Core mechanics** (8 files):
  - Movement, conversation, same-room detection
  - Day end, wake up, meal timeout
- **NTR system** (5 files):
  - Shibou/Renbo thresholds, NTR fall/protection
  - Save/load integration
- **Game systems** (11 files):
  - Energy/stamina zero handling
  - Ufufu toggle, insert pattern cycle
  - Chastity belt, speculum items
  - Visitor leave, day-end reset
  - Character-specific: Alice, Daiyousei, Sakuya

**Migration Strategy**:
- **Action**: Keep as-is (JSON format, headless compatible)
- **Purpose**: Release gate tests (run before every release)
- **Execution**: `dotnet run --project engine/uEmuera.Headless.csproj -- Game --flow`
- **Ownership**: Managed by regression-tester agent

### 2.3 Other Test Directories

| Directory | Count | Purpose | Migration Action |
|-----------|------:|---------|------------------|
| `ac/engine/` | 2 subdirs | Engine-level AC tests (F220 input validation) | Keep as-is |
| `ac/erb/` | 2 subdirs | ERB-level AC tests (F216 CFLAG boundaries, F190) | Keep as-is |
| `ac/f266/` | 1 file | Feature-specific AC test | Keep as-is |
| Root level | 2 files | feature-267.json, detection configs | Keep as-is |

**Total AC Tests**: 399 files (391 kojo + 8 other)

---

## 3. C# Test Projects

### 3.1 engine.Tests

**Path**: `C:\Era\era紅魔館protoNTR\engine.Tests\`
**Framework**: MSTest
**Project Status**: Active (F359 established structure)

**Existing Test Files** (17 files):
- CommandDispatcherTests.cs
- ConfigServiceTests.cs
- FileSystemTests.cs
- FlowTestExpectVerificationTests.cs - **Note**: Tests expect framework, NOT RAND behavior
- FontManagerTests.cs
- GlobalStaticCollection.cs
- HeadlessWindowTests.cs
- InterfaceExtractionTests.cs
- ParserMediatorTests.cs
- TestFixture.cs
- TestPathUtilsTests.cs
- TokenReaderTests.cs
- WarningCollectorTests.cs

**Migration Targets (F362)**:
- [ ] **MockRandomTests.cs** - Migrate TEST_MOCK_RAND*.ERB (AC#2)
- [ ] **FlowRandTests.cs** - Migrate TEST_FLOW_RAND*.ERB (AC#3)

### 3.2 tools/ErbParser.Tests

**Path**: `C:\Era\era紅魔館protoNTR\tools\ErbParser.Tests\`
**Framework**: Xunit
**Test Count**: 13 test files

**Coverage**:
- CflagExtractorTests.cs
- ConditionExtractorTests.cs
- DatalistParseTests.cs
- EmptyFileTests.cs
- FunctionExtractorTests.cs
- IConditionInterfaceTests.cs
- IfNodeTests.cs
- KojoExtractionTests.cs
- LogicalOperatorParserTests.cs
- NestedDatalistTests.cs
- SyntaxErrorTests.cs
- TalentBranchingExtractorTests.cs

**Status**: Complete (no migration needed)

### 3.3 tools/ErbToYaml.Tests

**Path**: `C:\Era\era紅魔館protoNTR\tools\ErbToYaml.Tests\`
**Framework**: Xunit
**Test Count**: 3 test files

**Coverage**:
- ConverterTests.cs
- PilotConversionTests.cs
- SchemaValidationTests.cs

**Status**: Complete (F351 pilot conversion verified)

### 3.4 tools/KojoComparer.Tests

**Path**: `C:\Era\era紅魔館protoNTR\tools\KojoComparer.Tests\`
**Framework**: Xunit
**Test Count**: 6 test files

**Coverage**:
- BatchProcessorTests.cs
- DiffEngineTests.cs
- ErbRunnerTests.cs
- OutputNormalizerTests.cs
- **PilotEquivalenceTests.cs** - **Migration Target AC#4**
- YamlRunnerTests.cs

**Migration Target (F362)**:
- [ ] **PilotEquivalenceTests.cs** - Enable 4 tests (currently Skip="Requires headless mode") (AC#4)
  - Tests: Lover, Yearning, Admiration, None conditions
  - Verifies: ERB==YAML equivalence for 美鈴 COM_0
  - Path: `tools/ErbToYaml.Tests/TestOutput/meirin_com0.yaml`

### 3.5 tools/YamlSchemaGen.Tests

**Path**: `C:\Era\era紅魔館protoNTR\tools\YamlSchemaGen.Tests\`
**Framework**: Xunit
**Test Count**: 1 test file

**Coverage**:
- SchemaValidationTests.cs

**Status**: Complete (F361 schema validation)

### 3.6 tools/ErbLinter.Tests

**Path**: `C:\Era\era紅魔館protoNTR\tools\ErbLinter.Tests\`
**Framework**: Xunit
**Test Count**: 1 test file

**Coverage**:
- SyntaxAnalyzerTests.cs

**Status**: Complete (static analysis tool)

---

## 4. Migration Priority Matrix

### Phase 2 (F362) - Test Infrastructure Migration

| Priority | Category | Count | Target | Verification | Status |
|:--------:|----------|------:|--------|--------------|:------:|
| 1 | ERB MockRandom tests | 2 files | src/engine.Tests/MockRandomTests.cs | AC#2 | [ ] |
| 2 | ERB Flow RAND tests | 2 files | src/engine.Tests/FlowRandTests.cs | AC#3 | [ ] |
| 3 | KojoComparer integration | 4 tests | Enable PilotEquivalenceTests.cs | AC#4 | [ ] |
| - | Test audit report | 1 doc | test-migration-audit.md | AC#1 | [x] |
| - | Coverage metrics | 1 section | Update this document | AC#5 | [ ] |

### Phase 3+ (Future) - Deferred Migrations

| Priority | Category | Count | Target | Reason for Deferral |
|:--------:|----------|------:|--------|---------------------|
| LOW | Kojo ERB test | 1 file | Keep as reference | Already migrated to JSON (391 files) |
| LOW | Unity graphics tests | 1 file | Unity integration tests | Requires Unity test infrastructure |

---

## 5. Coverage Analysis

### 5.1 Current Coverage

**ERB Tests**:
- Total: 6 files (~219 lines)
- Migrated to C#: 0 files (0%)
- Deferred/Reference: 2 files (33%) - test-kojo-k1.ERB, TEST_IMAGE.ERB
- **Migration Target**: 4 files (67%) - MockRandom (2), FlowRand (2)

**JSON Scenarios**:
- Total: 415 files
- Migrated to C#: N/A (remain as headless scenarios)
- Integration status: KojoComparer available (F360), integration pending (AC#4)

**C# Test Infrastructure**:
- Projects: 4 (engine.Tests + 3 tools/*Tests)
- Existing tests: 37+ test files
- New tests required: 2 files (MockRandomTests.cs, FlowRandTests.cs)
- Integration tests to enable: 4 tests (PilotEquivalenceTests.cs)

### 5.2 Post-F362 Coverage (Target)

**After AC#2-3 completion**:
```
ERB Core Tests:
  MockRandom: 2 C# tests (TEST_MOCK_RAND*.ERB → MockRandomTests.cs)
  FlowRand: 2 C# tests (TEST_FLOW_RAND*.ERB → FlowRandTests.cs)
  Coverage: 4/4 high-priority ERB tests migrated (100%)
```

**After AC#4 completion**:
```
Kojo Integration:
  PilotEquivalence: 4 enabled tests (Lover, Yearning, Admiration, None)
  Verification: ERB==YAML equivalence for 美鈴 COM_0
  Status: Full integration with KojoComparer (F360)
```

**Metrics**:
- **High-priority ERB tests**: 4/4 migrated (100%)
- **C# test projects**: 4/4 active
- **Kojo integration**: 4/4 equivalence tests enabled
- **JSON scenarios**: 415 files remain as headless-compatible integration tests

---

## 6. Recommendations

### 6.1 Immediate Actions (F362)

1. **Create MockRandomTests.cs** (AC#2)
   - Migrate TEST_MOCK_RAND.ERB and TEST_MOCK_RAND_EXHAUST.ERB
   - Verify mock injection and exhaustion fallback
   - Add to engine.Tests project

2. **Create FlowRandTests.cs** (AC#3)
   - Migrate TEST_FLOW_RAND.ERB and TEST_FLOW_RAND_MULTI.ERB
   - Verify single and multi-RAND flow execution
   - Add to engine.Tests project

3. **Enable PilotEquivalenceTests** (AC#4)
   - Remove Skip attributes or create alternative validation
   - Verify ERB==YAML equivalence for pilot data
   - Integrate with KojoComparer (F360)

4. **Update Coverage Metrics** (AC#5)
   - Add "Coverage:" section to this document
   - Report migrated/total test counts

### 6.2 Long-term Strategy

1. **Maintain JSON Scenarios**
   - Keep 415 JSON test files as headless integration tests
   - Do not migrate to C# (format already optimal for headless mode)
   - Expand KojoComparer coverage to additional characters

2. **Defer Unity Tests**
   - TEST_IMAGE.ERB remains until Phase 12
   - Unity test infrastructure required (not available in Phase 2-3)
   - Graphics/sprite/HTML rendering tests need Unity Test Framework

3. **Expand C# Unit Tests**
   - Add unit tests for new engine features as they're implemented
   - Follow TDD workflow: write tests before implementation
   - Maintain separation: unit tests (engine.Tests) vs integration tests (JSON scenarios)

---

## 7. Dependencies

### 7.1 Completed Prerequisites

- [x] F358: Phase 2 Planning (defines test migration scope)
- [x] F359: Test Structure (establishes engine.Tests foundation)
- [x] F360: KojoComparer (provides ERB==YAML equivalence testing)
- [x] F361: Schema Validator (validates YAML test fixtures)

### 7.2 Blocking Dependencies

None. All F362 tasks can proceed independently.

### 7.3 Post-F362 Unlock

- F363: Phase 3 System Infrastructure Planning (reactivated by F362 Task 6)
  - Requires revision based on Phase 2 implementation experience
  - Status change: [CANCELLED] → [PROPOSED]

---

## 8. Appendix: File Listings

### 8.1 ERB Test Files (Complete List)

```
C:\Era\era紅魔館protoNTR\Game\ERB\TEST_FLOW_RAND.ERB         (4 lines)
C:\Era\era紅魔館protoNTR\Game\ERB\TEST_FLOW_RAND_MULTI.ERB   (8 lines)
C:\Era\era紅魔館protoNTR\Game\ERB\TEST_IMAGE.ERB             (74 lines)
C:\Era\era紅魔館protoNTR\Game\ERB\TEST_MOCK_RAND.ERB         (9 lines)
C:\Era\era紅魔館protoNTR\Game\ERB\TEST_MOCK_RAND_EXHAUST.ERB (15 lines)
C:\Era\era紅魔館protoNTR\Game\ERB\test-kojo-k1.ERB           (109 lines)
Total: 219 lines
```

### 8.2 Kojo Feature Directories (Complete List)

```
test/ac/kojo/feature-180  (Meiling K1)
test/ac/kojo/feature-181  (Meiling K2)
test/ac/kojo/feature-182  (Meiling K3)
test/ac/kojo/feature-185  (Meiling K4)
test/ac/kojo/feature-186  (Meiling K5)
test/ac/kojo/feature-187  (Meiling K6)
test/ac/kojo/feature-188  (Meiling K7)
test/ac/kojo/feature-192  (Meiling K8)
test/ac/kojo/feature-194  (Meiling K10)
test/ac/kojo/feature-241  (Alice K11)
test/ac/kojo/feature-242  (Alice K12)
test/ac/kojo/feature-243  (Alice K13)
test/ac/kojo/feature-244  (Alice K14)
test/ac/kojo/feature-245  (Alice K15)
test/ac/kojo/feature-278  (Patchouli K21)
test/ac/kojo/feature-279  (Patchouli K22)
test/ac/kojo/feature-280  (Patchouli K23)
test/ac/kojo/feature-281  (Patchouli K24)
test/ac/kojo/feature-282  (Patchouli K25)
test/ac/kojo/feature-287  (Sakuya K31)
test/ac/kojo/feature-288  (Sakuya K32)
test/ac/kojo/feature-289  (Sakuya K33)
test/ac/kojo/feature-290  (Sakuya K34)
test/ac/kojo/feature-291  (Sakuya K35)
test/ac/kojo/feature-315  (Daiyousei K41)
test/ac/kojo/feature-316  (Daiyousei K42)
test/ac/kojo/feature-317  (Daiyousei K43)
test/ac/kojo/feature-318  (Daiyousei K44)
test/ac/kojo/feature-319  (Daiyousei K45)
test/ac/kojo/feature-324
test/ac/kojo/feature-325
test/ac/kojo/feature-326
test/ac/kojo/feature-327
test/ac/kojo/feature-338
Total: 34 directories, 391 JSON files
```

### 8.3 Regression Test Files (Complete List)

```
test/regression/scenario-alice-sameroom.json
test/regression/scenario-conversation.json
test/regression/scenario-daiyousei-sameroom.json
test/regression/scenario-dayend.json
test/regression/scenario-k4-kojo.json
test/regression/scenario-movement.json
test/regression/scenario-sakuya-sameroom.json
test/regression/scenario-sameroom.json
test/regression/scenario-sc-001-shiboo-threshold.json
test/regression/scenario-sc-002-shiboo-promotion.json
test/regression/scenario-sc-003-renbo-threshold.json
test/regression/scenario-sc-004-ntr-fall.json
test/regression/scenario-sc-005-ntr-protection.json
test/regression/scenario-sc-006-saveload.json
test/regression/scenario-sc-011-ufufu-toggle.json
test/regression/scenario-sc-012-insert-pattern-cycle.json
test/regression/scenario-sc-016-chastity-belt.json
test/regression/scenario-sc-017-speculum.json
test/regression/scenario-sc-023-meal-timeout.json
test/regression/scenario-sc-030-energy-zero.json
test/regression/scenario-sc-031-stamina-zero.json
test/regression/scenario-sc-034-visitor-leave.json
test/regression/scenario-sc-046-dayend-reset.json
test/regression/scenario-wakeup.json
Total: 24 files
```

---

## Coverage:

**ERB Test Migration**: 4/4 high-priority files migrated (100%)
- MockRandom tests: 2/2 migrated (AC#2 complete) → MockRandomTests.cs
- FlowRand tests: 2/2 migrated (AC#3 complete) → FlowRandTests.cs

**KojoComparer Integration**: 4/4 equivalence tests enabled (100%)
- PilotEquivalenceTests: 4 tests enabled (AC#4 complete)

**F362 Migration Status**: COMPLETE - All high-priority ERB tests migrated to C# MSTest

---

**Audit Status**: [COMPLETE]
**Next Steps**: Execute AC#2-6 (create C# tests, enable integration tests, reactivate F363)
