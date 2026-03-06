# Feature 358: Phase 2 Test Infrastructure Planning

## Status: [DONE]

## Type: research

## Created: 2026-01-05

> **Revision (2026-01-05 Review)**:
> 1. **Circular dependency removed**: F352 predecessor deleted (F352 depends on F358, not vice versa)
> 2. **F359 scope adjusted**: Era.Core.Tests already exists (created by F350 Task 0); changed to "Test Structure & Coverage Extension"
> 3. **AC#2 clarified**: Core sub-features F359-F361 required; F362-F363 optional per Task 1 analysis
> 4. **Phase 1 status updated**: F346-F353 all DONE (was "partially complete")

---

## Summary

Plan Phase 2 (Test Infrastructure) from full-csharp-architecture.md. Define MSTest setup, KojoComparer tool, CI integration, and test migration strategy.

---

## Background

### Philosophy (Mid-term Vision)

**Test-First Migration**: Before migrating game logic (Phase 3+), establish robust test infrastructure to catch regressions. KojoComparer ensures ERB==YAML equivalence throughout the migration.

### Problem (Current Issue)

full-csharp-architecture.md Phase 2 defines Test Infrastructure:
- MSTest project setup (Era.Core.Tests)
- KojoComparer tool (ERB==YAML equivalence)
- Schema Validator integration
- Existing test asset migration (160+ unit, 20+ regression, 24 flow)
- CI integration (GitHub Actions)
- Headless mode maintenance

Phase 1 (Tools) is complete (F346-F353 all DONE), but Phase 2 has no feature breakdown yet.

### Goal (What to Achieve)

1. Analyze Phase 2 requirements from full-csharp-architecture.md
2. Create concrete feature specifications for Phase 2 components
3. Define test migration strategy for existing ERB tests
4. Plan KojoComparer architecture

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 2 analysis documented | file | Grep | contains | "## Phase 2 Analysis" | [x] |
| 2a | F359 feature created | file | Glob | exists | Game/agents/feature-359.md | [x] |
| 2b | F360 feature created | file | Glob | exists | Game/agents/feature-360.md | [x] |
| 2c | F361 feature created | file | Glob | exists | Game/agents/feature-361.md | [x] |
| 3 | full-csharp-architecture.md Phase 2 updated | file | Grep | contains | "Related Features: F358" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze full-csharp-architecture.md Phase 2: document test counts, migration complexity, KojoComparer requirements from F351 pilot | [x] |
| 2 | 2a,2b,2c | Create F359 (Test Structure), F360 (KojoComparer), F361 (Schema Validator); F362/F363 optional based on Task 1 analysis | [x] |
| 3 | 3 | Update full-csharp-architecture.md Phase 2 with feature references | [x] |

---

## Output Features (Proposed)

Based on full-csharp-architecture.md Phase 2 Tasks (lines 818-824):

| Feature | Type | Scope | Reference |
|---------|------|-------|-----------|
| F359 | infra | Era.Core.Tests Test Structure & Coverage Extension (extends existing) | Phase 2 Task 1 |
| F360 | engine | KojoComparer Tool - ERB==YAML equivalence testing | Phase 2 Task 2 |
| F361 | infra | Schema Validator Integration - JSON Schema for YAML | Phase 2 Task 3 |
| F362 | infra | Test Migration - convert 160+ ERB unit tests to MSTest | Phase 2 Task 4 |
| F363 | infra | CI Integration - GitHub Actions test workflow | Phase 2 Task 5 |

**Note**: Features may be consolidated or split based on Task 1 analysis.

**F359 Scope Clarification**: Era.Core.Tests project already exists (created by F350 Task 0). F359 focuses on:
- Test organization and naming conventions
- Coverage targets and measurement
- Shared test utilities (fixtures, mocks, helpers)
- NOT project creation (already done)

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F346-F353 | Phase 1 Tools (all DONE) - provides conversion pipeline |
| Predecessor | F351 | Pilot Conversion results inform KojoComparer design |
| Successor | F352 | Phase 12 Planning depends on F358 (KojoComparer required for batch conversion)

**Dependency Note**: F352 was previously listed as predecessor but this created circular dependency. F352 (Phase 12 Kojo Conversion Planning) actually depends ON F358, not vice versa. F358 uses F351 pilot results (not F352) for KojoComparer requirements.

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 2 definition (lines 814-878)
- [feature-351.md](feature-351.md) - Pilot Conversion (Phase 1 completion, informs KojoComparer design)
- [feature-352.md](feature-352.md) - Phase 12 Kojo Conversion Planning (successor, blocked by F358)
- [feature-350.md](feature-350.md) - YAML Dialogue Renderer (created Era.Core.Tests)
- [feature-346.md](feature-346.md) - ERB Parser Foundation (Phase 1)
- [feature-347.md](feature-347.md) - TALENT Branching Extractor (Phase 1)
- [feature-348.md](feature-348.md) - YAML Schema Generator (Phase 1)
- [feature-349.md](feature-349.md) - DATALIST→YAML Converter (Phase 1)

**Terminology Note**:
- full-csharp-architecture.md "Phase 2" = Test Infrastructure (this feature)
- F344/F352 "Phase 2" = Core Migration = full-csharp-architecture.md Phase 12

**Reference to Architecture Doc**:
- Phase 2 Tasks: lines 818-824
- Deliverables: lines 826-833
- Test Layers: lines 835-842
- Existing Test Migration: lines 844-852

---

## Phase 2 Analysis

### Current Test Infrastructure State

**C# Unit Tests** (MSTest/xUnit):

| Project | Test Count | Status | Notes |
|---------|:----------:|--------|-------|
| Era.Core.Tests | 6 | Active | Created by F350 Task 0; YAML renderer tests |
| ErbParser.Tests | 65 | Active | Phase 1 parser foundation (F346) |
| ErbToYaml.Tests | 7 | Active | DATALIST converter + pilot tests (F349, F351) |
| engine.Tests (uEmuera.Tests) | 97 | Active | Existing uEmuera engine tests |
| **Total** | **175** | - | All MSTest/xUnit C# tests |

**JSON Scenario Tests** (Headless mode):

| Directory | Test Count | Format | Notes |
|-----------|:----------:|--------|-------|
| Game/tests/regression/ | 24 | JSON | Regression test scenarios |
| Game/tests/ac/kojo/ | 391 | JSON | Kojo AC tests (20+ features) |
| **Total** | **415** | - | Headless scenario tests |

**ERB Test Files** (Game/ERB/):

| File | Lines | Purpose | Migration Target |
|------|------:|---------|------------------|
| test-kojo-k1.ERB | 109 | K1 kojo unit test (F045) | KojoComparer reference |
| TEST_FLOW_RAND.ERB | 66 | Random flow test | C# integration test |
| TEST_FLOW_RAND_MULTI.ERB | 151 | Multi-flow test | C# integration test |
| TEST_IMAGE.ERB | 2000 | Image display test | Unity integration test |
| TEST_MOCK_RAND.ERB | 284 | Mock random test | C# (MockRandom) |
| TEST_MOCK_RAND_EXHAUST.ERB | 537 | Exhaustive test | C# unit test |
| **Total** | **3147** | - | Preserve for validation |

**Summary**: 175 C# tests + 415 JSON scenarios + 3147 lines of ERB tests = comprehensive test coverage foundation. Era.Core.Tests project already exists; F359 focuses on structure extension, not project creation.

### Migration Complexity Assessment

**Phase 2 Task 4 Analysis** (Existing Test Migration):

full-csharp-architecture.md lines 844-852 outline migration targets:

| Current Test Type | Count | Conversion Complexity | Action |
|-------------------|:-----:|:---------------------:|--------|
| `Game/tests/unit/*.erb` | 160+ | **HIGH** | Convert to MSTest unit tests (requires ERB→C# logic translation) |
| `Game/tests/regression/*.erb` | 20+ | **MEDIUM** | Convert to MSTest integration tests (scenario-based) |
| `Game/tests/flow/*.erb` | 24 | **MEDIUM** | Convert to C# integration tests (flow control validation) |
| `Game/tests/kojo/*.erb` | ~50 | **HIGH** | Migrate to KojoComparer (requires equivalence testing framework) |
| `engine.Tests/*.cs` | 12 | **LOW** | Preserve and extend (already C#) |

**Complexity Notes**:
- **unit/*.erb**: 160+ tests = largest migration task. Requires ERB→C# logic rewrite for each test case.
- **kojo/*.erb**: ~50 tests listed in architecture doc, but actual count = 391 JSON files in Game/tests/ac/kojo/. Discrepancy suggests doc refers to ERB test files, not JSON scenario files.
- **Actual kojo test files**: 391 JSON files are already headless-compatible. Migration to KojoComparer = enhance with ERB==YAML equivalence checks, not full rewrite.

**Revised Complexity**: Phase 2 Task 4 focuses on:
1. **ERB test files** in Game/ERB/ (3147 lines) → C# unit/integration tests
2. **JSON kojo scenarios** (391 files) → extend with KojoComparer equivalence validation
3. **Existing C# tests** (175 tests) → preserve, extend coverage

**Estimated Effort**:
- Task 1 (MSTest project setup): **DONE** (Era.Core.Tests created by F350)
- Task 2 (KojoComparer tool): **3-5 days** (new tool, ERB==YAML output comparison)
- Task 3 (Schema Validator integration): **1-2 days** (integrate F348 output)
- Task 4 (Existing test migration): **2-3 weeks** (160+ unit tests, 391 kojo scenarios)
- Task 5 (CI integration): **1 day** (GitHub Actions workflow)
- Task 6 (Headless mode maintenance): **Ongoing** (preserve API compatibility)

### KojoComparer Requirements from F351 Pilot

**F351 Findings**:

F351 execution log (line 123) documents critical limitation:
> ErbParser は PRINTDATA...ENDDATA 構造を未対応。F351 テストコード内で regex workaround 使用。Phase 2 (F352) で Parser 拡張を検討すべき

**PRINTDATA Parser Gap**:

tools/ErbToYaml.Tests/PilotConversionTests.cs lines 225-249 show manual regex parsing:
```csharp
/// <summary>
/// Find PRINTDATA node in function body
/// NOTE: Current parser doesn't parse PRINTDATA blocks.
/// This method manually parses the raw ERB text to extract ALL PRINTDATA blocks
/// with their IF/ELSEIF/ELSE conditions.
/// </summary>
private string? FindPrintDataNode(List<ErbParser.Ast.AstNode> functionBody)
{
    // Since parser doesn't handle PRINTDATA, we manually extract from source file
    var erbContent = File.ReadAllText(_erbFilePath);
    // ... regex extraction ...
}
```

**Impact on KojoComparer**:
- **ERB output generation**: KojoComparer must run ERB kojo functions in headless mode to capture PRINTDATA output
- **YAML output generation**: KojoComparer must render YAML through YamlRenderer (Era.Core.KojoEngine)
- **Output comparison**: String-based diff with normalization (whitespace, formatting)
- **PRINTDATA parsing**: F351 workaround acceptable for pilot, but NOT for production KojoComparer
  - **Dependency**: F354 PRINTDATA Parser Extension (Phase 12) required for automated batch conversion
  - **Workaround**: KojoComparer can compare outputs without parsing PRINTDATA (run both, compare results)

**KojoComparer Architecture** (derived from F351):

| Component | Purpose | Implementation |
|-----------|---------|----------------|
| **ErbRunner** | Execute ERB kojo function in headless mode | Use HeadlessRunner.cs (existing) |
| **YamlRunner** | Render YAML through Era.Core.KojoEngine | Use YamlRenderer (F350) |
| **OutputNormalizer** | Normalize whitespace, formatting for comparison | Regex-based text cleanup |
| **DiffEngine** | Compare normalized outputs (string diff) | Line-by-line comparison with mismatch reporting |
| **BatchProcessor** | Run equivalence tests on multiple kojo files | Iterate kojo directory, report pass/fail |

**Equivalence Test Flow**:
1. Load ERB kojo file (e.g., KOJO_K1_愛撫.ERB)
2. Execute ERB function @KOJO_MESSAGE_COM_K1_0 → capture output A
3. Load YAML kojo file (converted in Phase 12)
4. Render YAML with same TALENT/CFLAG state → capture output B
5. Normalize both outputs (strip whitespace, formatting)
6. Compare A == B → report PASS/FAIL with diff on failure

**Test Coverage Strategy**:
- **Unit tests**: KojoEngine rendering (already covered by Era.Core.Tests 6 tests)
- **Integration tests**: Full ERB==YAML equivalence on 391 kojo scenarios
- **Schema validation**: YAML structure validation (F348 output)
- **Regression tests**: Existing 24 regression scenarios preserved + extended

**F351 Pilot Success Metrics**:
- AC#1: Conversion succeeded (美鈴 COM_0 ERB → YAML)
- AC#2: All 4 TALENT states rendered (4/4 pass)
- AC#3: Manual comparison confirmed distinct output per TALENT state

**KojoComparer Goal**: Automate AC#3 validation for all 391+ kojo files, eliminating manual comparison bottleneck.

**Blockers**:
- F354 PRINTDATA Parser Extension (Phase 12) NOT required for KojoComparer
  - KojoComparer compares outputs (black-box), not AST structure
  - F354 required for batch conversion automation (F355), not equivalence testing
- F348 YAML Schema Generator output (dialogue-schema.json) required
  - F348 status: [DONE] → schema available

**Dependencies for KojoComparer (F360)**:
- ✅ F350 (YAML Renderer) - DONE
- ✅ F348 (YAML Schema) - DONE
- ✅ F351 (Pilot results) - DONE
- ❌ F354 (PRINTDATA Parser) - NOT required (outputs compared, not AST)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | - | Created to plan Phase 2 Test Infrastructure | PROPOSED |
| 2026-01-05 | review | opus | Reviewed against Phase 1/2 and F34x-F35x | 4 issues found |
| 2026-01-05 | revise | opus | Fixed circular dep, F359 scope, AC#2, Phase 1 status | PROPOSED |
| 2026-01-05 | START | implementer | Task 1 | Phase 2 analysis |
| 2026-01-05 | END | implementer | Task 1 | SUCCESS |
| 2026-01-05 | START | implementer | Task 2 | Create F359-F361 specs |
| 2026-01-05 | END | implementer | Task 2 | SUCCESS |
| 2026-01-05 | START | implementer | Task 3 | Update full-csharp-architecture.md Phase 2 |
| 2026-01-05 | END | implementer | Task 3 | SUCCESS |
