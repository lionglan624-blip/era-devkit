# Feature 363: Phase 3 System Infrastructure Planning

## Status: [DONE]

> **Reactivation Notes (2026-01-06)**:
> - Phase 2 Test Infrastructure complete (F359-F362 DONE)
> - Test coverage: 100% high-priority ERB tests migrated to C# MSTest
> - KojoComparer operational for ERB==YAML equivalence testing
> - Lessons learned: MSTest structure extensible, headless mode robust, schema validation prevents structural errors
> - Ready to plan Phase 3 with concrete test infrastructure in place

## Type: research

## Created: 2026-01-05

---

## Summary

Plan Phase 3 (System Infrastructure) from full-csharp-architecture.md. Analyze SYSTEM.ERB, COMMON*.ERB, DIM.ERH dependencies and define concrete sub-feature breakdown based on Phase 2 implementation experience.

**Context**: Phase 2 Test Infrastructure (F359-F362) completed successfully, providing robust MSTest foundation, KojoComparer tool, and schema validation for Phase 3 validation.

---

## Background

### Philosophy (Mid-term Vision)

**Foundation-First Migration**: Phase 3 is CRITICAL - all subsequent phases (4-12) depend on System Infrastructure. Thorough planning ensures smooth migration of ~6,200 lines of foundational code. Phase 2 completion validates that test infrastructure is ready to support Phase 3 migration.

### Problem (Current Issue)

full-csharp-architecture.md Phase 3 defines System Infrastructure:
- SYSTEM.ERB migration (game main loop, ~2000 lines)
- COMMON*.ERB migration (shared functions, ~2700 lines)
- DIM.ERH migration (variable definitions, ~800 lines)
- Header files (.ERH) migration (7 files, ~700 lines)

**Phase 2 Completion Context**:
- F359: MSTest structure established with BaseTestClass, TestHelpers, MockGameContext
- F360: KojoComparer operational (ERB==YAML equivalence testing)
- F361: Schema validation integrated (build-time + CI)
- F362: ERB test migration complete (100% high-priority tests → C# MSTest)
- Test coverage baseline documented (Era.Core coverage measured)

**Current Gap**: Phase 3 has no feature breakdown yet. Need to plan migration with concrete understanding of:
1. How to test System Infrastructure components using Phase 2 test framework
2. Migration strategy informed by actual F359-F362 implementation challenges
3. Dependency order based on full-csharp-architecture.md Phase 3 Task priority

### Goal (What to Achieve)

1. Analyze Phase 3 file dependencies and migration order using full-csharp-architecture.md
2. Define DIM.ERH → Constants.cs conversion strategy (CRITICAL - all phases depend on this)
3. Create concrete feature specifications (F364-F371) with test integration plan
4. Update full-csharp-architecture.md Phase 3 with feature references
5. Incorporate Phase 2 lessons: use MSTest for C# migrations, schema validation for data files, KojoComparer for kojo-related migrations

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 3 analysis documented | file | Grep | contains | "## Phase 3 Analysis" in feature-363.md | [x] |
| 2 | Phase 2 lessons documented | file | Grep | contains | "## Phase 2 Lessons" in feature-363.md | [x] |
| 3 | At least 3 sub-features created | file | Glob | gte | 3 files matching feature-36[4-9]*.md | [x] |
| 4 | full-csharp-architecture.md Phase 3 updated | file | Grep | contains | "**Related Features**: F363" in full-csharp-architecture.md | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze full-csharp-architecture.md Phase 3: document file dependencies, migration order, complexity per component | [x] |
| 2 | 2 | Document Phase 2 lessons learned: MSTest patterns, test migration challenges, KojoComparer usage, schema validation insights | [x] |
| 3 | 3 | Create CRITICAL sub-features for Phase 3 (F364-F366: DIM.ERH, SYSTEM.ERB, COMMON.ERB) | [x] |
| 4 | 4 | Update full-csharp-architecture.md Phase 3 with feature references and Phase 2 completion status | [x] |

> **Note**: Remaining sub-features (F367-F371) creation delegated to F366 Task 6 per Feature Progression Protocol.

---

## Output Features (Proposed)

Based on full-csharp-architecture.md Phase 3 Tasks (lines 900-908):

| Feature | Type | Scope | Priority | Lines (est.) | Reference |
|---------|------|-------|:--------:|-------------:|-----------|
| F364 | engine | DIM.ERH → Constants.cs + VariableDefinitions | **CRITICAL** | ~572-800 | Phase 3 Task 6 (first - all phases depend on this) |
| F365 | engine | SYSTEM.ERB → GameInitialization.cs | **CRITICAL** | ~242 | Phase 3 Task 1 |
| F366 | engine | COMMON.ERB → CommonFunctions.cs | **CRITICAL** | ~660 | Phase 3 Task 2 |
| F367-F371 | engine | SYSTEM.ERB external dependencies | HIGH | ~194+ | F365 support features |
| F372 | engine | COMMON_PLACE.ERB → LocationSystem.cs | HIGH | ~318 | Phase 3 Task 5 |
| F373 | engine | INFO.ERB → InfoDisplay.cs | HIGH | ~1490 | Phase 3 Task 8 (NEEDS INVESTIGATION) |
| F374 | engine | COMMON_J.ERB → Localization.cs | HIGH | ~74 | Phase 3 Task 3 |
| F375 | engine | COMMON_KOJO.ERB → KojoCommon.cs | HIGH | ~49 | Phase 3 Task 4 |
| F376 | engine | Header files consolidation | Medium | ~197 | Phase 3 Task 6-7 |

**Note**: Line counts are estimates from full-csharp-architecture.md and may vary. Actual counts to be confirmed during Task 1 analysis.

**Note (F367-F371)**: These features were created for SYSTEM.ERB external dependencies (F365 scope). COMMON.ERB related features are F372-F376.

**Implementation Order** (revised based on Phase 2 experience):
1. **F364 (DIM.ERH)** - FIRST - All subsequent features depend on Constants.cs
2. **F365, F366** - SYSTEM.ERB, COMMON.ERB (parallel possible, both CRITICAL)
3. **F367-F371** - SYSTEM.ERB dependencies (F365 support)
4. **F372-F376** - COMMON.ERB dependencies (F366 support)

**Test Strategy** (based on Phase 2 lessons):
- Use engine.Tests/ MSTest structure (F359 BaseTestClass, TestHelpers)
- For COMMON_KOJO.ERB: validate with KojoComparer (F360)
- Schema validation for any data file conversions (F361)
- Follow F362 migration pattern: create C# tests before removing ERB code

**Note**: Task 2 Phase 2 lessons will inform test planning. Minimum 3 sub-features are required per AC#3.

**Header Files Gap**: Phase 3 defines 5 additional .ERH files (続柄.ERH, NTR_MASTER_3P_SEX.ERH, グラフィック表示/*.ERH, FairyMaids.erh) not listed above. Task 1 analysis will determine whether to create separate features or consolidate with existing ones.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F358 | Phase 2 Planning complete (DONE) |
| Predecessor | F359 | Test Structure established (DONE) - MSTest, BaseTestClass, TestHelpers available |
| Predecessor | F360 | KojoComparer operational (DONE) - validates COMMON_KOJO migration |
| Predecessor | F361 | Schema Validator integrated (DONE) - validates data file conversions |
| Predecessor | F362 | Test Migration complete (DONE) - reactivates F363 (this feature) |
| Successor | F364-F371 | Phase 3 implementation features (to be created by Task 3) |

**Dependency Chain**:
```
F358 (Phase 2 Planning) → F359, F360, F361 → F362 (Test Migration) → [F363 REACTIVATED] → F364-F371 (Phase 3 Implementation)
```

**Phase 2 Complete**: All prerequisites for Phase 3 planning are DONE. Test infrastructure ready to support Phase 3 migration.

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 3 definition (lines 894-951)
- [feature-358.md](feature-358.md) - Phase 2 Planning (DONE - predecessor)
- [feature-359.md](feature-359.md) - Test Structure (DONE - MSTest foundation)
- [feature-360.md](feature-360.md) - KojoComparer (DONE - validates COMMON_KOJO migration)
- [feature-361.md](feature-361.md) - Schema Validator (DONE - validates data file conversions)
- [feature-362.md](feature-362.md) - Test Migration (DONE - reactivates this feature)
- [test-migration-audit.md](designs/test-migration-audit.md) - Phase 2 test coverage report

**Reference to Architecture Doc**:
- Phase 3 Tasks: lines 900-908
- Source Analysis: lines 910-922
- Header Files: lines 923-934
- Deliverables: lines 935-945

---

## Phase 3 Analysis

### File Dependencies and Migration Order

Based on full-csharp-architecture.md Phase 3 and actual source code analysis:

#### Actual Line Counts (Verified)

| File | Lines | Architecture Est. | Variance | Priority |
|------|------:|:-----------------:|:--------:|:--------:|
| DIM.ERH | 572 | 572-800 | ✓ Accurate | **CRITICAL** |
| SYSTEM.ERB | 242 | ~2000 | -88% ERROR | **CRITICAL** |
| COMMON.ERB | 660 | ~1500 | -56% ERROR | **CRITICAL** |
| COMMON_J.ERB | 74 | ~300 | -75% ERROR | HIGH |
| COMMON_KOJO.ERB | 49 | ~500 | -90% ERROR | HIGH |
| COMMON_PLACE.ERB | 318 | ~400 | -21% | HIGH |
| ColorSettings.erh | 45 | ~100 | -55% | Medium |
| INFO.ERB | 1490 | ~600 | **+148% ERROR** | Medium |
| **Total** | **3450** | **~6200** | **-44%** | - |

**CRITICAL FINDING**: full-csharp-architecture.md line count estimates are significantly inaccurate:
- SYSTEM.ERB: 242 lines (NOT 2000) - 88% overestimated
- COMMON.ERB: 660 lines (NOT 1500) - 56% overestimated
- COMMON_KOJO.ERB: 49 lines (NOT 500) - 90% overestimated
- INFO.ERB: 1490 lines (NOT 600) - **148% underestimated** - LARGEST file in Phase 3

**Revised Complexity Assessment**:
- Phase 3 scope is ~3450 lines (NOT ~6200) - 44% smaller than estimated
- INFO.ERB is the largest migration component (1490 lines, 43% of total)
- COMMON* files are much smaller than estimated (total ~1101 lines, NOT ~2700)

#### Header Files (.ERH) - Complete Inventory

| File | Lines | Purpose | Priority | Migration Strategy |
|------|------:|---------|:--------:|-------------------|
| DIM.ERH | 572 | Global variables, constants, #DEFINE | **CRITICAL** | Constants.cs + VariableDefinitions.cs |
| ColorSettings.erh | 45 | Color palette definitions | Medium | ColorConstants.cs |
| 続柄.ERH | 71 | Relationship definitions | HIGH | RelationshipConstants.cs |
| NTR_MASTER_3P_SEX.ERH | 7 | NTR 3P definitions | Medium | NtrConstants.cs |
| グラフィック表示/グラフィック表示.ERH | 28 | Graphics settings | Medium | GraphicsConstants.cs |
| グラフィック表示/立ち絵表示.ERH | 10 | Portrait settings | Medium | PortraitConstants.cs |
| 妖精メイド拡張/FairyMaids.erh | 36 | Fairy maid definitions | Medium | FairyMaidConstants.cs |
| **Total** | **769** | - | - | 7 files |

**Header Migration Strategy**:
- DIM.ERH: Split into Constants.cs (CONST) + VariableDefinitions.cs (SAVEDATA, CHARADATA)
- Other .ERH: Consolidate into single Constants.cs file or separate by domain

#### DIM.ERH Structure Analysis (572 lines)

**Content Categories**:
1. **#DIM CONST** (~200+ lines): Constant definitions
   - Time constants (要睡眠経過時間, 時間_1WEEK, etc.)
   - Character codes (人物_美鈴, 人物_咲夜, etc.)
   - Location codes (場所_正門, 場所_広間, etc.)
   - System constants (人物最大, 精子主_*, etc.)

2. **#DIM SAVEDATA** (~100+ lines): Save data variables
   - Visitor appearance data (訪問者_髪の長さ, etc.)
   - Game state variables
   - Multi-dimensional arrays (親別出産数, 305, 305)

3. **#DIM CHARADATA** (~50+ lines): Character data variables
   - Character-specific arrays (移動ルート, etc.)

4. **#DEFINE** (~20+ lines): Macro aliases
   - 天候値 TIME:1
   - 暦法月 DAY:1
   - 告白成功済 既成事実&1p0

**Migration Strategy**:
```
DIM.ERH (572 lines)
├── Constants.cs (~250 lines)
│   ├── TimeConstants
│   ├── CharacterConstants
│   ├── LocationConstants
│   └── SystemConstants
└── VariableDefinitions.cs (~322 lines)
    ├── SaveDataVariables (SAVEDATA declarations)
    ├── CharacterVariables (CHARADATA declarations)
    └── MacroAliases (#DEFINE → extension methods or properties)
```

**Critical Dependencies**:
- ALL ERB files reference DIM.ERH constants (人物_*, 場所_*, etc.)
- Phase 4+ Variable System depends on VariableDefinitions.cs
- **BLOCKER**: Constants.cs must exist BEFORE any other Phase 3 migration

#### SYSTEM.ERB Structure Analysis (242 lines)

**Content**:
- @EVENTFIRST: New game initialization (mode selection, quick start)
- @DEFAULT_OPTION: Default game options setup
- Mode flags: Futanari mode, busty mode, early visitor, NTR all-in

**Key Observation**: SYSTEM.ERB in era紅魔館protoNTR is **MINIMAL** compared to typical ERA games.
- Typical SYSTEM.ERB: Game main loop, turn processing, event dispatch (~2000 lines)
- This SYSTEM.ERB: Event initialization only (~242 lines)
- Main loop logic likely integrated into engine (uEmuera.Headless, Unity frontend)

**Migration Strategy**:
- SYSTEM.ERB → GameInitialization.cs (NOT GameLoop.cs)
- Main loop already exists in C# engine (headless mode proof)
- Focus on event handlers (@EVENTFIRST, @DEFAULT_OPTION)

#### COMMON.ERB Structure Analysis (660 lines)

**Content**:
- @ITEMSTOCK: Item availability/purchase validation (27 lines)
- @CHOICE: 2-4 choice selection function (38 lines)
- Many other common utility functions

**Migration Strategy**:
- COMMON.ERB → CommonFunctions.cs
- Functions are utilities, NOT core game loop
- Can be migrated incrementally (function-by-function)

#### INFO.ERB Investigation Required

**BLOCKER IDENTIFIED**: INFO.ERB is 1490 lines (148% larger than estimated), but purpose unclear from architecture doc.

**Investigation Needed** (before creating sub-features):
1. Read INFO.ERB to understand content
2. Identify dependencies on SYSTEM/COMMON
3. Determine if it can be split into smaller components
4. Assess whether "Medium" priority is appropriate (may need CRITICAL)

**Risk**: Creating INFO.ERB feature without investigation may result in:
- Underestimated effort
- Missing dependencies
- Incorrect priority assignment

#### Dependency Chain

```
Phase 3 Migration Order (CRITICAL PATH):

1. DIM.ERH (572 lines) - FIRST, ALL others depend on this
   ├── Constants.cs (~250 lines)
   └── VariableDefinitions.cs (~322 lines)

2. COMMON.ERB (660 lines) - utilities used by others
   └── CommonFunctions.cs

3. SYSTEM.ERB (242 lines) - initialization, depends on Constants + Common
   └── GameInitialization.cs

4. COMMON_* files (parallel, after COMMON.ERB)
   ├── COMMON_J.ERB (74 lines) → Localization.cs
   ├── COMMON_KOJO.ERB (49 lines) → KojoCommon.cs
   └── COMMON_PLACE.ERB (318 lines) → LocationSystem.cs

5. INFO.ERB (1490 lines) - LARGEST file, investigate first
   └── InfoDisplay.cs (needs breakdown)

6. ColorSettings.erh (45 lines) + other .ERH (152 lines)
   └── Consolidate into Constants.cs or separate files
```

**Parallelization Opportunities**:
- COMMON_J, COMMON_KOJO, COMMON_PLACE can be migrated in parallel (after COMMON.ERB)
- Header files (.ERH) can be migrated in parallel (after DIM.ERH)

#### Test Strategy (Based on Phase 2 Lessons)

**F359 Infrastructure Available**:
- engine.Tests/ MSTest project established
- Direct state access pattern (GlobalStatic.Reset(), not mocks)
- Coverage measurement via coverlet

**Test Patterns from F362**:
1. **Unit Tests Pattern** (MockRandomTests, FlowRandTests):
   - Create test class per source file
   - Minimum 2 test methods per AC
   - Use MSTest [TestMethod] attribute
   - Coverage target: 85%+ for non-critical, 95%+ for critical paths

2. **Integration Tests Pattern** (PilotEquivalenceTests):
   - Use KojoComparer for COMMON_KOJO migration validation
   - Enable existing Skip tests when functionality ready
   - Headless mode execution for full-stack validation

**Phase 3 Test Plan**:

| Migration Target | Test Project | Test Strategy | Coverage Target |
|------------------|--------------|---------------|:---------------:|
| Constants.cs | engine.Tests/ConstantsTests.cs | Verify all constants exist with correct values | 100% |
| VariableDefinitions.cs | engine.Tests/VariableDefinitionsTests.cs | Verify variable declarations match DIM.ERH | 100% |
| CommonFunctions.cs | engine.Tests/CommonFunctionsTests.cs | Unit test each function (@ITEMSTOCK, @CHOICE, etc.) | 95% |
| GameInitialization.cs | engine.Tests/GameInitializationTests.cs | Test @EVENTFIRST, @DEFAULT_OPTION | 95% |
| Localization.cs | engine.Tests/LocalizationTests.cs | Test Japanese text helpers | 85% |
| KojoCommon.cs | engine.Tests/KojoCommonTests.cs + KojoComparer | Validate ERB==C# equivalence | 95% |
| LocationSystem.cs | engine.Tests/LocationSystemTests.cs | Test place management functions | 95% |
| InfoDisplay.cs | engine.Tests/InfoDisplayTests.cs | (Needs breakdown after investigation) | 85% |

**TDD Workflow** (from F362 experience):
1. Create C# test class BEFORE implementation
2. Write failing tests (RED)
3. Implement minimal C# code (GREEN)
4. Verify ERB removal doesn't break headless tests (REFACTOR)
5. Run full test suite: `dotnet test engine.Tests/`

### Revised Sub-Feature Breakdown

Based on actual line counts and dependency analysis:

#### Recommended Features (Minimum 3 for AC#3)

| Feature | Type | Scope | Priority | Lines (actual) | Complexity | Estimated Effort |
|---------|------|-------|:--------:|---------------:|:----------:|:----------------:|
| **F364** | engine | DIM.ERH → Constants.cs + VariableDefinitions.cs | **CRITICAL** | 572 | HIGH | 2-3 days |
| **F365** | engine | SYSTEM.ERB → GameInitialization.cs | **CRITICAL** | 242 | LOW | 1 day |
| **F366** | engine | COMMON.ERB → CommonFunctions.cs | **CRITICAL** | 660 | MEDIUM | 2-3 days |

**Additional Features (Defer to Task 3)**:
- F367: COMMON_PLACE.ERB → LocationSystem.cs (318 lines)
- F368: INFO.ERB → InfoDisplay.cs (1490 lines) - **NEEDS INVESTIGATION**
- F369: COMMON_J.ERB → Localization.cs (74 lines)
- F370: COMMON_KOJO.ERB → KojoCommon.cs (49 lines)
- F371: Header files consolidation (197 lines)

**Key Changes from Proposed Output Features**:
1. **INFO.ERB investigation** required before creating feature (largest file, 1490 lines)
2. **SYSTEM.ERB** downgraded to HIGH - minimal scope (242 lines, initialization only)
3. Line counts corrected: 3450 total (NOT 6200 from architecture doc)

**Minimum Sub-Features for AC#3** (at least 3 required):
1. F364 (DIM.ERH) - MANDATORY FIRST
2. F365 (SYSTEM.ERB) - MANDATORY
3. F366 (COMMON.ERB) - MANDATORY

**Recommendation**: Start with these 3 CRITICAL features. Create additional features after F364-F366 proven successful.

---

## Phase 2 Lessons Learned

### Testing Framework Patterns (F359)

**MSTest Project Structure - engine.Tests/**:
- **BaseTestClass pattern NOT implemented** - Each test class is self-contained without inheritance hierarchy
- **TestHelpers pattern NOT applicable** - Engine tests use direct xUnit assertions without wrapper utilities
- **MockGameContext NOT applicable** - Engine tests use GlobalStatic.Reset() and direct state injection via KojoTestConfig
- **Coverage infrastructure works well** - coverlet.collector integrates seamlessly with `dotnet test --collect:"XPlat Code Coverage"`

**Key Pattern**: Engine tests favor composition over inheritance. Tests directly manipulate GlobalStatic state rather than using mock abstractions.

**Era.Core.Tests/ Pattern** (distinct from engine.Tests/):
- BaseTestClass, TestHelpers, MockGameContext **ARE** used here for YAML rendering tests
- Demonstrates that test utility patterns depend on component architecture
- YAML rendering benefits from abstraction; engine tests benefit from direct access

**Lesson**: Phase 3 migrations should follow the pattern appropriate to their domain. System infrastructure (SYSTEM.ERB, COMMON.ERB) will likely follow engine.Tests/ pattern (direct state access), while higher-level features may use abstraction.

### Test Migration Challenges (F362)

**ERB Line Count Estimation Risk**:
- F358 Phase 2 planning estimated ~3,147 lines of ERB tests
- Actual audit (F362 Task 1) found **~225 lines** (14x overestimate)
- **Lesson**: Always audit actual files before planning. Glob-based line counting may include comments/whitespace.

**Conditional Compilation (#if HEADLESS_MODE)**:
- MockRandomTests.cs and FlowRandTests.cs use `#if HEADLESS_MODE` guards
- Tests compile successfully in both modes (graceful degradation)
- Placeholder test `MockRand_RequiresHeadlessMode()` ensures test discovery works even when HEADLESS_MODE is undefined
- **Lesson**: Use conditional compilation for headless-specific tests. Include placeholder tests for non-headless builds to avoid test count discrepancies.

**Test Migration Priority Strategy**:
- HIGH priority: MockRandom, FlowRand (core functionality) - migrated first
- MEDIUM priority: Kojo integration (KojoComparer equivalence tests) - enabled PilotEquivalenceTests
- LOW priority: TEST_IMAGE.ERB (Unity graphics) - deferred to Phase 12
- **Lesson**: Prioritize tests that validate core game mechanics. Defer Unity-specific tests until Unity test infrastructure is available.

**Migration Completeness - AC#2-4 Results**:
- AC#2: 2 ERB files → MockRandomTests.cs with 5 test methods (2 required + 3 supplementary)
- AC#3: 2 ERB files → FlowRandTests.cs with 5 test methods (2 required + 3 supplementary)
- AC#4: PilotEquivalenceTests.cs enabled (4 tests for TALENT states: Lover, Yearning, Admiration, None)
- **Coverage**: 4/4 high-priority ERB tests migrated (100%)
- **Lesson**: Implementer agent created comprehensive test coverage beyond minimum AC requirements (supplementary tests for edge cases).

### KojoComparer Usage Insights (F360)

**Architecture - Subprocess vs Project Reference**:
- **ErbRunner**: Uses subprocess (`dotnet run --project engine/uEmuera.Headless.csproj`) to execute ERB
- **YamlRunner**: Uses ProjectReference to Era.Core.KojoEngine for YAML rendering
- **Rationale**: Avoids cross-submodule build dependencies (engine/ is separate git repo)
- **Trade-off**: Subprocess overhead acceptable for equivalence testing (not hot-path operation)

**OutputNormalizer Critical Role**:
- Normalizes whitespace, line endings, color codes, DATAFORM prefixes
- **Without normalization**: ERB output includes formatting artifacts (DATAFORM prefix, CRLF line endings, color codes)
- **With normalization**: Semantic content comparison possible
- **Lesson**: Output normalization is CRITICAL for ERB==YAML equivalence testing. Phase 12 kojo migration will rely heavily on this.

**PilotEquivalenceTests Integration**:
- Initially created with `Skip="Requires headless mode"` attributes (F360 AC#8)
- F362 Task 4 enabled by removing Skip attributes
- Tests verify ERB==YAML for 美鈴 COM_0 across 4 TALENT states
- **Result**: All 4 tests pass - validates pilot conversion workflow
- **Lesson**: KojoComparer is production-ready for Phase 12 batch conversion. Pilot data from F351 provides reliable baseline.

**CLI Test Results (F360 lines 305-322)**:
- AC#6 (PASS case): Expected FAIL because YAML contains multiple dialogue variants while ERB produces single random output
- **Insight**: This is NOT a tool bug - demonstrates diff detection capability
- **Lesson**: For PASS tests, YAML must contain exact dialogue variant that ERB produces (single-variant YAML, not multi-variant pool).

### Schema Validation Integration (F361)

**Build-Time vs Runtime Validation Strategy**:
- **Build-time**: ErbToYaml converter validates output YAML before writing (fail fast)
- **Runtime**: Era.Core.KojoEngine has optional validation (debug mode, disabled by default for performance)
- **CI Integration**: `.github/workflows/test.yml` validates all YAML files with YamlValidator CLI
- **Lesson**: Multi-layer validation catches errors early. Build-time validation prevents invalid YAML from being committed.

**Schema Validator CLI Tool**:
- Standalone tool (`tools/YamlValidator/`) for validating YAML files
- Exit code 0 (valid) / 1 (invalid) for CI integration
- Can validate single file or entire directory (`--validate-all`)
- **Lesson**: CLI tools with exit codes integrate cleanly into CI workflows. Directory validation mode scales to batch operations.

**NJsonSchema Integration**:
- Uses F348 output (`tools/YamlSchemaGen/dialogue-schema.json`)
- Provides detailed validation error messages with line/column information
- **Example error**: "Property 'TALENT' does not match schema pattern '^[A-Z]+:\d+$'"
- **Lesson**: JSON Schema provides precise error reporting. Error messages guide developers to fix structural issues quickly.

**IDE Integration (documented, not implemented)**:
- VS Code: `.vscode/settings.json` with `yaml.schemas` mapping
- JetBrains Rider: Automatic detection via `$schema` property in YAML
- **Status**: Documented but not enforced in F361
- **Lesson**: IDE integration is optional for Phase 2 but recommended for Phase 12 (when kojo authors work directly with YAML).

### Test Infrastructure Readiness for Phase 3

**What's Ready**:
1. **MSTest infrastructure**: engine.Tests/ established with 15 test files (MockRandomTests, FlowRandTests added in F362)
2. **Coverage measurement**: coverlet.collector operational, Cobertura XML output for CI
3. **Headless mode testing**: Conditional compilation pattern proven (`#if HEADLESS_MODE`)
4. **KojoComparer**: Production-ready for kojo migration validation
5. **Schema validation**: Multi-layer validation (build-time, CI, optional runtime)

**What's Not Applicable to Phase 3**:
1. **YAML dialogue testing** - Phase 3 is infrastructure (SYSTEM.ERB, COMMON.ERB), not content
2. **KojoComparer** - Phase 3 has no kojo content
3. **Schema validation** - Phase 3 migrates ERB to C#, not YAML

**Phase 3 Test Strategy Implications**:
- Use engine.Tests/ for C# unit tests (direct state access pattern)
- Use `#if HEADLESS_MODE` for integration tests that require full engine
- Follow MockRandomTests/FlowRandTests naming convention: `{Component}_{Scenario}_{ExpectedResult}`
- Aim for 85-95% line coverage per F359 targets
- Create test fixtures in `engine.Tests/TestData/` if needed

**Migration Workflow Proven in Phase 2**:
1. **Audit first** (F362 Task 1) - actual line counts, not estimates
2. **Create C# tests** (F362 Tasks 2-3) - before removing ERB code
3. **Verify tests pass** (F362 Task 4) - integration with existing infrastructure
4. **Document coverage** (F362 Task 5) - update audit report
5. **Update downstream features** (F362 Task 6) - reactivate blocked features

**Key Takeaway**: Phase 2 validated that full C# migration is feasible. Test infrastructure is robust, extensible, and ready to support Phase 3 System Infrastructure migration.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-05 | create | opus | Created to plan Phase 3 System Infrastructure | PROPOSED |
| 2026-01-05 | cancel | opus | Premature - wait for F362 (Test Migration) | CANCELLED |
| 2026-01-06 | reactivate | implementer | F362 Task 6 - Phase 2 complete, spec revised | PROPOSED |
| 2026-01-06 07:00 | START | implementer | Task 2 (Document Phase 2 lessons) | - |
| 2026-01-06 07:00 | END | implementer | Task 2 (Document Phase 2 lessons) | SUCCESS |
| 2026-01-06 07:01 | START | implementer | Task 3 (Create sub-features F364-F366) | - |
| 2026-01-06 07:01 | END | implementer | Task 3 (Create sub-features F364-F366) | SUCCESS |
| 2026-01-06 07:04 | START | implementer | Task 1 (Phase 3 analysis) | - |
| 2026-01-06 07:04 | END | implementer | Task 1 (Phase 3 analysis) | SUCCESS |
| 2026-01-06 07:05 | START | implementer | Task 4 (Update full-csharp-architecture.md) | - |
| 2026-01-06 07:05 | END | implementer | Task 4 (Update full-csharp-architecture.md) | SUCCESS |
