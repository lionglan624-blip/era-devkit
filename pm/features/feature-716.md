# Feature 716: Era.Core Test Coverage Hardening

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
Test coverage should grow alongside implementation. Completed C# migration phases should have proportional test coverage — infrastructure without tests is hidden technical debt. The test-strategy.md (F499) established the design; this Feature fills the concrete gaps.

### Problem (Current Issue)
Several Era.Core namespaces completed in earlier migration phases have significant test coverage gaps:

| Namespace | Total Files | Impl Files | Test Files | Uncovered | Impl Gap |
|-----------|:-----------:|:----------:|:----------:|:---------:|:--------:|
| Training/ | 26 | 16 | 9 | 3 | 19% |
| Character/ | 10 | 4 | 2 | 4 | 50% |
| Ability/ | 4 | 2 | 1 | 1 | 50% |
| Data/ (loaders) | 55 | 55 | 28 | 27 | 50% |
| IO/Encoding/DI | 5 | 5 | 0 | 5 | 100% |

**Note**: Training gap corrected per Root Cause Analysis — actual uncovered classes: AbilityGrowthProcessor, BasicChecksProcessor, EquipmentProcessor (3 of 16).

These gaps were not tracked in any migration Phase or Feature. Phase 7 (F407) added Training integration tests but not unit tests for individual processors. Phase 15 (Architecture Review) designed the test strategy but did not include gap remediation tasks.

Additionally, 4 tools have zero test coverage: com-validator, SaveAnalyzer, YamlValidator, kojo-mapper.

### Goal (What to Achieve)
Add unit tests for the highest-gap Era.Core namespaces (Training, Character, Ability) to bring them to adequate coverage. Data loader and tool test gaps are tracked but deferred to separate features if volume exceeds this feature's scope.

**Scope**: Era.Core unit tests only. Tool tests and engine.Tests are out of scope. GrowthData.cs excluded — pure data classes with no business logic beyond property storage (no test value).

**Volume estimate**: ~7-10 test files, ~60-100 test methods.

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. Why: Several Era.Core namespaces (Training, Character, Ability) have significant test coverage gaps.
2. Why: Migration phases (Phase 6-7, Phase 9) focused on porting ERB logic to C# and verifying integration behavior, not writing granular unit tests for every processor class.
3. Why: The migration workflow prioritized "does it produce the same output as ERB?" (equivalence tests) over "does each class behave correctly in isolation?" (unit tests).
4. Why: Test strategy (F499) was designed after most migration phases were complete, so earlier phases lacked unit test requirements in their ACs.
5. Why: There was no systematic coverage enforcement mechanism during migration — coverage gaps accumulated silently without tracking.

### Symptom vs Root Cause

| Symptom | Root Cause |
|---------|-----------|
| Training has 26 files but only ~9 test files; Character has 10 files but only 2 test files; Ability has 4 files but only 1 test file | Migration phases prioritized equivalence testing over unit testing, and no coverage tracking was in place until F499 (test strategy) was completed |

### Conclusion

The root cause is a **process gap**: migration phases did not include unit test ACs for individual processor/manager classes. Integration and equivalence tests were written (e.g., TrainingIntegrationTests, OrgasmProcessorEquivalenceTests), but dedicated unit tests for classes like BasicChecksProcessor, EquipmentProcessor, AbilityGrowthProcessor, VirginityManager, PainStateChecker, ExperienceGrowthCalculator, and AbilityGrowth were never created. The test-strategy.md (F499) identified the pattern but did not include remediation tasks.

**Correction to Problem section**: The Problem table overstates the Training gap. Investigation found 9 test files (not 2) covering Training classes: TrainingProcessorTests, FavorCalculatorTests, JuelProcessorTests, MarkSystemTests, OrgasmProcessorEquivalenceTests, SpecialTrainingTests, TrainingSetupTests, TrainingIntegrationTests, ScenarioPrerequisiteTests. The actual uncovered Training implementation classes are: AbilityGrowthProcessor, BasicChecksProcessor, EquipmentProcessor (3 of 16 impl classes). Character and Ability numbers are accurate.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F499 | [DONE] | Foundation | Test strategy design — established patterns this feature follows |
| F407 | [DONE] | Partial coverage | Phase 7 Training integration tests — added TrainingIntegrationTests but not per-processor unit tests |
| F392 | [DONE] | Migration origin (Ability) | Phase 6 Ability System Core — created AbilitySystem, AbilityGrowth; only AbilitySystemTests exists |
| F393 | [DONE] | Migration origin (Training) | Phase 6 Training Processing Core — created processors; TrainingProcessorTests exists but not all |
| F396 | [DONE] | Migration origin (Character) | Phase 6 Character State Tracking — created CharacterStateTracker etc.; CharacterStateTests exists |
| F717 | [DRAFT] | Successor | Tool test coverage — deferred from this feature |
| F718 | [DRAFT] | Successor | Data loader test coverage — deferred from this feature |
| F708 | [DONE] | Build quality | TreatWarningsAsErrors — new test files must compile warning-free |

### Pattern Analysis

This is not a recurring problem but an expected consequence of the migration strategy: migration phases prioritized functional equivalence over unit test density. Now that migration is largely complete, this is the appropriate time to backfill unit tests. The pattern of "test gap remediation as a separate feature" is established by F499's design and should be followed for Data/ (F718) and tools (F717).

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | All target classes have interfaces, are DI-registered, and follow testable patterns (constructor injection, IVariableStore dependency) |
| Scope is realistic | YES | Actual gap is ~7 new test files (3 Training + 4 Character + 0-1 Ability), not 15-20. Volume estimate in Goal should be revised down to ~7-10 test files, ~60-100 test methods |
| No blocking constraints | YES | All dependencies (F499, F407, F708) are [DONE]. No external blockers. Test infrastructure (xUnit 3.x, Moq, BaseTestClass, DI container) is mature and ready |

**Verdict**: FEASIBLE

The feature is implementable with no blockers. The scope is actually smaller than estimated in the Problem/Goal sections because many Training classes already have test coverage. The revised scope focuses on:

- **Training** (3 uncovered impl classes): AbilityGrowthProcessor, BasicChecksProcessor, EquipmentProcessor
- **Character** (4 uncovered impl classes): CharacterStateTracker, ExperienceGrowthCalculator, PainStateChecker, VirginityManager
- **Ability** (1-2 uncovered impl classes): AbilityGrowth, GrowthData (data class, may not need dedicated tests)

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core.Tests/Training/AbilityGrowthProcessorTests.cs | Create | Unit tests for ability growth during training |
| Era.Core.Tests/Training/BasicChecksProcessorTests.cs | Create | Unit tests for basic modifier checks |
| Era.Core.Tests/Training/EquipmentProcessorTests.cs | Create | Unit tests for equipment processing logic |
| Era.Core.Tests/CharacterStateTests.cs | Expand | Add behavioral unit tests for CharacterStateTracker delegation methods (TrackVirginityLoss, ProcessExperienceGrowth, CheckPainState) beyond existing DI resolution tests |
| Era.Core.Tests/Character/ExperienceGrowthCalculatorTests.cs | Create | Unit tests for experience growth calculation |
| Era.Core.Tests/Character/PainStateCheckerTests.cs | Create | Unit tests for pain state checking |
| Era.Core.Tests/Character/VirginityManagerTests.cs | Create | Unit tests for virginity management |
| Era.Core.Tests/Ability/AbilityGrowthTests.cs | Create | Unit tests for AbilityGrowth (if complex enough) |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| TreatWarningsAsErrors (F708) | Directory.Build.props | MEDIUM — All new test files must compile with zero warnings |
| DI-based test pattern | BaseTestClass, existing conventions | LOW — Must follow established pattern: inherit BaseTestClass, use `Services.GetRequiredService<T>()` |
| IVariableStore dependency | Most classes depend on IVariableStore for game state | MEDIUM — Tests need proper variable store setup; may require Moq or the real DI container |
| xUnit 3.x conventions | Era.Core.Tests.csproj uses xunit.v3 3.2.2 | LOW — Must use v3 attribute format ([Fact], [Theory], [Trait]) |
| net10.0 target | Era.Core.Tests.csproj | LOW — Must be compatible with .NET 10 |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Some processors have complex internal dependencies (e.g., TrainingProcessor calls sub-processors) making unit isolation difficult | Medium | Medium | Use Moq to mock interfaces; the DI registration already uses interfaces for all dependencies |
| Test setup complexity — VariableStore requires initialized character data | Medium | Low | Follow existing patterns in FavorCalculatorTests/JuelProcessorTests which already handle this |
| Scope creep — discovering untested edge cases in existing code while writing tests | Low | Medium | Strict adherence to Scope Discipline: log issues, defer to follow-up features |
| Build time increase from ~60-100 new test methods | Low | Low | Negligible impact; xUnit parallel execution handles this well |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Test coverage should grow alongside implementation" | New test files must be created for uncovered classes | AC#1-AC#8 |
| "Completed C# migration phases should have proportional test coverage" | Training (3), Character (4), Ability (1) namespaces must all gain tests | AC#1-AC#8 |
| "infrastructure without tests is hidden technical debt" | No technical debt markers (TODO/FIXME/HACK) in new test files | AC#12 |
| "fills the concrete gaps" (Goal) | All tests must pass | AC#9 |
| "fills the concrete gaps" (Goal) | Build must succeed with zero warnings (F708) | AC#10 |
| "proportional test coverage" | Each test file must have non-trivial coverage (minimum test method count) | AC#11 |
| "Data loader and tool test gaps are tracked but deferred" | F717 and F718 DRAFT files exist | AC#13, AC#14 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | AbilityGrowthProcessorTests.cs exists | file | Glob(Era.Core.Tests/Training/AbilityGrowthProcessorTests.cs) | exists | - | [x] |
| 2 | BasicChecksProcessorTests.cs exists | file | Glob(Era.Core.Tests/Training/BasicChecksProcessorTests.cs) | exists | - | [x] |
| 3 | EquipmentProcessorTests.cs exists | file | Glob(Era.Core.Tests/Training/EquipmentProcessorTests.cs) | exists | - | [x] |
| 4 | CharacterStateTests.cs expanded with CharacterStateTracker tests | code | Grep(Era.Core.Tests/CharacterStateTests.cs, "TrackVirginityLoss|ProcessExperienceGrowth|CheckPainState") | gte | 3 | [x] |
| 5 | ExperienceGrowthCalculatorTests.cs exists | file | Glob(Era.Core.Tests/Character/ExperienceGrowthCalculatorTests.cs) | exists | - | [x] |
| 6 | PainStateCheckerTests.cs exists | file | Glob(Era.Core.Tests/Character/PainStateCheckerTests.cs) | exists | - | [x] |
| 7 | VirginityManagerTests.cs exists | file | Glob(Era.Core.Tests/Character/VirginityManagerTests.cs) | exists | - | [x] |
| 8 | AbilityGrowthTests.cs exists | file | Glob(Era.Core.Tests/Ability/AbilityGrowthTests.cs) | exists | - | [x] |
| 9 | All Era.Core.Tests pass | test | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 10 | Build succeeds with zero warnings | build | dotnet build Era.Core.Tests | succeeds | - | [x] |
| 11 | Each NEW test file has >= 3 test methods | code | Grep(per file, see AC Details for 7 files) | gte | 3 | [x] |
| 12 | No technical debt markers in new/modified test files | code | Grep(8 files per AC Details, "TODO|FIXME|HACK") | not_contains | - | [x] |
| 13 | F717 DRAFT file exists | file | Glob(Game/agents/feature-717.md) | exists | - | [x] |
| 14 | F718 DRAFT file exists | file | Glob(Game/agents/feature-718.md) | exists | - | [x] |
| 15 | F718 includes IO/Encoding/DI namespace in scope | code | Grep(Game/agents/feature-718.md, "IO.*Encoding|Encoding.*DI") | contains | - | [x] |
| 16 | F717 registered in index-features.md | code | Grep(Game/agents/index-features.md, "717.*DRAFT") | contains | - | [x] |
| 17 | F718 registered in index-features.md | code | Grep(Game/agents/index-features.md, "718.*DRAFT") | contains | - | [x] |

**Note**: 17 ACs is above the infra range (8-15) but justified by DRAFT Creation Checklist requirements. AC#1-8 verify file creation for each target class. AC#9-10 verify build/test quality. AC#11 verifies each new test file has >= 3 test methods (per-file verification described in AC Details). AC#12 verifies code quality. AC#13-14 verify deferred scope file existence. AC#15 verifies F718 scope expansion. AC#16-17 verify index registration.

### AC Details

**AC#1: AbilityGrowthProcessorTests.cs exists**
- Verifies unit test file created for `Era.Core/Training/AbilityGrowthProcessor.cs`
- This processor handles ability stat growth during training sessions
- Test: `Glob("Era.Core.Tests/Training/AbilityGrowthProcessorTests.cs")`

**AC#2: BasicChecksProcessorTests.cs exists**
- Verifies unit test file created for `Era.Core/Training/BasicChecksProcessor.cs`
- This processor handles basic modifier checks during training
- Test: `Glob("Era.Core.Tests/Training/BasicChecksProcessorTests.cs")`

**AC#3: EquipmentProcessorTests.cs exists**
- Verifies unit test file created for `Era.Core/Training/EquipmentProcessor.cs`
- This processor handles equipment-related logic during training
- **Scope**: Tests cover only ProcessEquipment orchestration and GetEquipmentFlag method; 18 stub handler methods are excluded (no testable behavior). Stub replacement tests will be added when methods gain real logic
- Test: `Glob("Era.Core.Tests/Training/EquipmentProcessorTests.cs")`

**AC#4: CharacterStateTests.cs expanded with CharacterStateTracker tests**
- Verifies existing test file `Era.Core.Tests/CharacterStateTests.cs` is expanded with CharacterStateTracker-specific tests
- This is the facade class for tracking character state changes
- Test: `Grep("Era.Core.Tests/CharacterStateTests.cs", "TrackVirginityLoss|ProcessExperienceGrowth|CheckPainState")` with count >= 3 (verifies methods for delegation testing)

**AC#5: ExperienceGrowthCalculatorTests.cs exists**
- Verifies unit test file created for `Era.Core/Character/ExperienceGrowthCalculator.cs`
- This class calculates experience growth for characters
- Test: `Glob("Era.Core.Tests/Character/ExperienceGrowthCalculatorTests.cs")`

**AC#6: PainStateCheckerTests.cs exists**
- Verifies unit test file created for `Era.Core/Character/PainStateChecker.cs`
- This class checks and manages pain state for characters
- Test: `Glob("Era.Core.Tests/Character/PainStateCheckerTests.cs")`

**AC#7: VirginityManagerTests.cs exists**
- Verifies unit test file created for `Era.Core/Character/VirginityManager.cs`
- This class manages virginity state tracking
- Test: `Glob("Era.Core.Tests/Character/VirginityManagerTests.cs")`

**AC#8: AbilityGrowthTests.cs exists**
- Verifies unit test file created for `Era.Core/Ability/AbilityGrowth.cs`
- This class handles ability growth calculations
- Test: `Glob("Era.Core.Tests/Ability/AbilityGrowthTests.cs")`

**AC#9: All Era.Core.Tests pass**
- All existing and new tests must pass together
- Ensures new tests do not break existing functionality
- Test: `dotnet test Era.Core.Tests/Era.Core.Tests.csproj`
- Expected: exit code 0, all tests pass

**AC#10: Build succeeds with zero warnings**
- F708 enforces TreatWarningsAsErrors in Directory.Build.props
- New test files must compile cleanly with zero warnings
- Test: `dotnet build Era.Core.Tests/Era.Core.Tests.csproj`
- Expected: exit code 0, "Build succeeded. 0 Warning(s)"

**AC#11: Each NEW test file has >= 3 test methods**
- Ensures non-trivial coverage per new test file (not just a single placeholder test)
- Manual verification during implementation: Grep each of the 7 new test files for `[Fact]` and `[Theory]` attributes with count >= 3:
  - `Era.Core.Tests/Training/AbilityGrowthProcessorTests.cs`
  - `Era.Core.Tests/Training/BasicChecksProcessorTests.cs`
  - `Era.Core.Tests/Training/EquipmentProcessorTests.cs`
  - `Era.Core.Tests/Character/ExperienceGrowthCalculatorTests.cs`
  - `Era.Core.Tests/Character/PainStateCheckerTests.cs`
  - `Era.Core.Tests/Character/VirginityManagerTests.cs`
  - `Era.Core.Tests/Ability/AbilityGrowthTests.cs`
- Note: CharacterStateTests.cs excluded (covered by AC#4 requirement of >= 3 specific method names)
- Rationale: 3 is the minimum for meaningful coverage (happy path + edge case + error case)

**AC#12: No technical debt markers in new/modified test files**
- New and modified test code must not contain TODO, FIXME, or HACK markers
- Test: `Grep` each of the 7 new test files and 1 expanded test file for pattern `TODO|FIXME|HACK`
- Expected: zero matches across all target test files

**AC#13: F717 DRAFT file exists**
- Tool test coverage was deferred from this feature's scope
- The DRAFT file must exist to track the deferred work
- Test: `Glob("Game/agents/feature-717.md")`

**AC#14: F718 DRAFT file exists**
- Data loader test coverage was deferred from this feature's scope
- The DRAFT file must exist to track the deferred work
- Test: `Glob("Game/agents/feature-718.md")`

**AC#15: F718 includes IO/Encoding/DI namespace in scope**
- IO/Encoding/DI namespace test gap was added to F718's scope (5 files, 100% uncovered)
- Verifies that F718's Problem section includes these system-level utilities alongside Data/ loaders
- Test: `Grep("Game/agents/feature-718.md", "IO.*Encoding|Encoding.*DI")`
- Expected: Pattern found in F718's scope or problem description

**AC#16: F717 registered in index-features.md**
- Per DRAFT Creation Checklist: DRAFT creation must verify BOTH file existence AND index registration
- Verifies F717 is properly registered in index-features.md with [DRAFT] status
- Test: `Grep("Game/agents/index-features.md", "717.*DRAFT")`

**AC#17: F718 registered in index-features.md**
- Per DRAFT Creation Checklist: DRAFT creation must verify BOTH file existence AND index registration
- Verifies F718 is properly registered in index-features.md with [DRAFT] status
- Test: `Grep("Game/agents/index-features.md", "718.*DRAFT")`

---

<!-- fc-phase-4-completed -->
## Technical Design

### Overview

This feature creates unit tests for 7 target classes across 3 namespaces following the established test infrastructure patterns. All target classes already have DI registration and follow testable patterns (constructor injection, interface-based dependencies, Result monad for error handling). The test files will use xUnit 3.x with the existing BaseTestClass infrastructure.

### Test Infrastructure Analysis

**Existing Patterns (from BaseTestClass, FavorCalculatorTests, TrainingProcessorTests, CharacterStateTests):**

1. **Base Class Pattern**: All test classes inherit from `BaseTestClass`
   - Provides `Services` property (IServiceProvider) with full DI container
   - Container is initialized with `services.AddEraCore()` in constructor
   - Auto-disposes container in `Dispose()` method

2. **Test Method Pattern**: Tests use xUnit 3.x conventions
   - `[Fact]` for simple tests, `[Theory]` for parameterized tests
   - `[Trait("AC", "N")]` for AC traceability
   - `[Trait("Category", "NamespaceName")]` for test categorization

3. **Dependency Resolution Pattern**: Two approaches depending on test needs
   - **DI Resolution**: `Services.GetRequiredService<IInterface>()` for integration-style tests
   - **Mock-Free**: Existing tests primarily use DI container, not Moq
   - Tests rely on the real DI container with properly initialized VariableStore

4. **Assertion Pattern**: Direct xUnit assertions
   - `Assert.NotNull()`, `Assert.Equal()`, `Assert.IsType<T>()`
   - Pattern matching with Result monad: `result.Match(onSuccess: _, onFailure: _)`
   - No custom assertion libraries beyond xUnit

### Target Class Analysis

#### Training Namespace (3 classes)

**1. AbilityGrowthProcessor** (530 lines)
- **Constructor Dependencies**: `IVariableStore`, `ITrainingVariables`
- **Key Methods to Test**:
  - `ProcessGrowth(CharacterId)` - orchestrates 12 sub-processors
  - `ProcessCombatAbility()`, `ProcessConversationSkill()`, etc. - private methods (test via ProcessGrowth)
  - `GetExpLv(int)` - EXPLV table lookup (boundary conditions)
  - `CheckExpUp(CharacterId, int)` - EXP_UP check logic
- **Test Strategy**:
  - Use DI container to get real instance
  - Setup test data in VariableStore (ABL, EXP, TALENT values)
  - Verify GrowthResult contains expected ability/talent changes
  - Test edge cases: max level caps (20, 10, 6), threshold boundaries, EXP_UP conditions
- **Mock Requirements**: None (use real VariableStore from DI)
- **Estimated Tests**: 5-8 (ProcessGrowth happy path, threshold boundaries, level caps, EXP_UP edge cases)

**2. BasicChecksProcessor** (212 lines)
- **Constructor Dependencies**: `IVariableStore`
- **Key Methods to Test**:
  - `GetTimeModifier(CommandId)` - simple range check (< 200 || >= 600)
  - `GetFavorModifier(CharacterId, int)` - SELECTCASE with 5 branches
  - `GetTechniqueModifier(CharacterId, int)` - 2 branches (commandType 0-3 vs else)
  - `GetMoodModifier(CharacterId, int)` - 3 branches (special cases + default)
  - `GetReasonModifier(CharacterId, int)` - 3 branches (special cases + default)
  - `GetRevision(int, int, int)` - private asymptotic formula (test via public methods)
- **Test Strategy**:
  - Each method has distinct input ranges (commandId, commandType)
  - Setup VariableStore with known CFLAG/ABL/BASE values
  - Verify correct modifier calculation for each branch
  - Test boundary conditions (commandType 0, 3, 10, 30, 33)
- **Mock Requirements**: None (use real VariableStore)
- **Estimated Tests**: 8-12 (1-2 per method + boundary cases)

**3. EquipmentProcessor** (267 lines, mostly stubs)
- **Constructor Dependencies**: `IVariableStore`, `ICharacterStateVariables`, `ITEquipVariables`
- **Key Methods to Test**:
  - `ProcessEquipment(CharacterId)` - orchestrates 18 equipment checks + 2 insertions
  - `GetEquipmentFlag(CharacterId, int)` - wrapper method
  - Equipment handler methods are currently stubs (no logic to test)
- **Test Strategy**:
  - Focus on ProcessEquipment orchestration logic only
  - Verify equipment flag checks are executed (via TEQUIP values)
  - Test V/A insertion partner detection logic
  - **Skip stub methods** (no tests for placeholder logic; tests will be added when stubs are replaced with real implementations)
- **Mock Requirements**: None (use real ITEquipVariables)
- **Estimated Tests**: 3-5 (ProcessEquipment success, equipment flag detection, insertion checks)

#### Character Namespace (4 classes)

**4. CharacterStateTracker** (48 lines) - Facade/Orchestrator
- **Constructor Dependencies**: `IVirginityManager`, `IExperienceGrowthCalculator`, `IPainStateChecker`
- **Key Methods to Test**:
  - `TrackVirginityLoss()` - delegates to VirginityManager
  - `ProcessExperienceGrowth()` - validates character + delegates to calculator
  - `CheckPainState()` - delegates to pain checker
- **Test Strategy**:
  - This is a thin facade - tests verify delegation, not business logic
  - Use DI container (dependencies are already tested separately)
  - Verify invalid character validation in ProcessExperienceGrowth
  - Verify successful delegation to sub-components
- **Mock Requirements**: None (use real dependencies from DI)
- **Estimated Tests**: 3-4 (delegation verification, invalid input handling)

**5. ExperienceGrowthCalculator** (387 lines) - Complex Logic
- **Constructor Dependencies**: `IVariableStore`, `ITrainingVariables`, `Func<CharacterId, CupIndex, int>`, `Func<CharacterId, JuelIndex, int>`, `IJuelVariables`
- **Key Methods to Test**:
  - `CheckObedienceGrowth(CharacterId)` - ABL:従順 >= 3 && TALENT:抵抗 check
  - `CheckDesireGrowth(CharacterId)` - ABL:欲望 >= 3 && TALENT:自己愛 < 0 check
  - `ProcessExperienceGain(CharacterId)` - orchestrates service/pain pleasure
  - `ProcessServicePleasureExperience()` - 6 thresholds (1000-12000), CUP multipliers
  - `ProcessPainPleasureExperience()` - 6 thresholds (lust/pain combos)
  - `ProcessSadisticPleasureExperience()` - PLAYER sadism + target masochism check
- **Test Strategy**:
  - Setup VariableStore with ABL/TALENT/EXP/CUP values
  - Verify state changes (TALENT:抵抗 = 0, JUEL:100 /= 2)
  - Test threshold boundaries (e.g., totalPleasure = 999, 1000, 1001)
  - Verify CUP multiplier application (fear, antipathy)
- **Mock Requirements**: Need to mock `Func<CharacterId, CupIndex, int>` and `Func<CharacterId, JuelIndex, int>` to return controlled values
- **Estimated Tests**: 10-15 (obedience/desire checks, service pleasure thresholds, pain pleasure thresholds, sadistic checks)

**6. PainStateChecker** (227 lines)
- **Constructor Dependencies**: `IVariableStore`
- **Key Methods to Test**:
  - `CheckPain(CharacterId, PainType, Func<PalamIndex, int>)` - 2 pain types × 2 modifier tables
  - `GetExpLv(int)` - EXPLV table lookup
  - `GetPalamLv(int)` - PALAMLV table lookup
- **Test Strategy**:
  - Setup VariableStore with EXP:Ｖ経験 or EXP:Ａ経験 at various levels
  - Mock `getPalam` Func to return controlled lubrication values
  - Verify correct multipliers for each experience/lubrication combination
  - Test boundary conditions (EXPLV thresholds, PALAMLV thresholds)
- **Mock Requirements**: Mock `Func<PalamIndex, int>` parameter
- **Estimated Tests**: 8-12 (vaginal pain checks, anal pain checks, threshold boundaries, table edge cases)

**7. VirginityManager** (237 lines)
- **Constructor Dependencies**: `IVariableStore`, `ITEquipVariables`
- **Key Methods to Test**:
  - `CheckLostVirginity()` - routes to 3 virginity types
  - `ProcessVaginalVirginity()` - TALENT:処女 levels (0/1/2), chastity effects
  - `ProcessAnalVirginity()` - simple tracking
  - `ProcessKissVirginity()` - simple tracking
  - `ProcessChastityEffects()` - 3 branches (chastity+affection combos)
  - `DetermineVirginityLossMethod()` - TEQUIP checks (vibrator, VSex)
- **Test Strategy**:
  - Setup VariableStore with TALENT:処女, TALENT:貞操, TALENT:恋慕, SOURCE values
  - Setup ITEquipVariables with equipment flags
  - Verify TCVAR:破瓜 values (0/1/2)
  - Verify SOURCE multipliers (love ×0.60/×1.20, antipathy ×0.30-10.00)
  - Test invalid character handling
- **Mock Requirements**: None (use real ITEquipVariables)
- **Estimated Tests**: 8-12 (virginity types, chastity combos, equipment detection, invalid input)

#### Ability Namespace (1 class)

**8. AbilityGrowth** (106 lines) - Data Structures
- **Type**: 3 readonly record structs + 1 result class
  - `AbilityGrowth` (index, amount)
  - `TalentGrowth` (index, amount)
  - `ExperienceGrowth` (index, amount)
  - `GrowthResult` (lists + Add methods)
- **Key Behaviors to Test**:
  - Record struct construction and property access
  - GrowthResult.AddAbility/AddTalent/AddExperience methods
  - List initialization in constructor
- **Test Strategy**:
  - Simple instantiation and equality tests
  - Verify Add methods populate lists correctly
  - Test constructor with null/non-null collections
- **Mock Requirements**: None (pure data structures)
- **Estimated Tests**: 3-5 (struct construction, GrowthResult methods, edge cases)

### Test Dependency Strategy

**General Principle**: Prefer real DI container for all registered services.

**Where Test Arguments are Required**:
1. **ExperienceGrowthCalculator**: Use registered DI factories for `Func<CharacterId, CupIndex, int>` and `Func<CharacterId, JuelIndex, int>` delegates
   - These Func delegates are registered in DI via `CallbackFactories.AddTrainingCallbacks()` (called from `AddEraCore()`)
   - **Implementation approach**: Use standard DI resolution: `Services.GetRequiredService<IExperienceGrowthCalculator>()`
   - For test isolation, use the real factories or inject test-controlled instances via DI

2. **PainStateChecker**: Provide test `Func<PalamIndex, int>` parameter for CheckPain method
   - Method parameter requiring test lambda: `(index) => testLubricationValue`
   - Use DI resolution for the instance: `Services.GetRequiredService<PainStateChecker>()`

**Where Standard DI Resolution Applies**:
- All other classes use real DI container
- VariableStore, TrainingVariables, etc. are properly initialized in BaseTestClass
- No mocking required for any registered services or interfaces

### VariableStore Setup Pattern

**Challenge**: Tests need to set up game state (ABL, EXP, TALENT, SOURCE, etc.) before executing test logic.

**Solution** (observed in existing tests):
1. Get IVariableStore from DI container
2. Use `SetAbility()`, `SetExp()`, `SetTalent()`, `SetSource()`, etc. to initialize state
3. Execute test method
4. Verify Result or output state

**Example Pattern** (from FavorCalculatorTests):
```csharp
var variables = Services.GetRequiredService<IVariableStore>();
var target = CharacterId.Meiling;

// Setup
variables.SetAbility(target, AbilityIndex.Obedience, 5);
variables.SetTalent(target, TalentIndex.Resistance, 1);

// Act
var result = calculator.CheckObedienceGrowth(target);

// Assert
Assert.True(result.LostResistance);
```

### File Structure Design

All new test files follow this structure:

```csharp
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Era.Core.Training; // or Era.Core.Character, Era.Core.Ability
using Era.Core.Types;
using Era.Core.Interfaces;

namespace Era.Core.Tests.Training; // or Character, Ability

/// <summary>
/// Unit tests for [ClassName].
/// Feature 716 - Era.Core Test Coverage Hardening.
/// Tests verify [key behaviors].
/// </summary>
[Trait("Category", "Training")] // or Character, Ability
public class [ClassName]Tests : BaseTestClass
{
    [Fact]
    [Trait("Category", "Training")]
    public void MethodName_ValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var instance = Services.GetRequiredService<IInterface>();
        var variables = Services.GetRequiredService<IVariableStore>();
        // Setup test data

        // Act
        var result = instance.Method(args);

        // Assert
        Assert.NotNull(result);
        // Specific assertions
    }

    [Fact]
    public void MethodName_BoundaryCondition_HandlesCorrectly()
    {
        // ...
    }

    [Fact]
    public void MethodName_InvalidInput_ReturnsFailure()
    {
        // ...
    }
}
```

**Directory Structure**:
- `Era.Core.Tests/Training/AbilityGrowthProcessorTests.cs`
- `Era.Core.Tests/Training/BasicChecksProcessorTests.cs`
- `Era.Core.Tests/Training/EquipmentProcessorTests.cs`
- `Era.Core.Tests/CharacterStateTests.cs` (note: existing file tests DI resolution - expand with CharacterStateTracker-specific tests)
- `Era.Core.Tests/Character/ExperienceGrowthCalculatorTests.cs`
- `Era.Core.Tests/Character/PainStateCheckerTests.cs`
- `Era.Core.Tests/Character/VirginityManagerTests.cs`
- `Era.Core.Tests/Ability/AbilityGrowthTests.cs`

**Note**: CharacterStateTests.cs already exists (244 lines) but only tests DI resolution and delegation. New CharacterStateTracker-specific tests will be ADDED to existing file, not created from scratch. This changes AC#4 from "file creation" to "file expansion".

### Test Method Naming Convention

**Pattern**: `MethodName_Scenario_ExpectedBehavior`

**Examples**:
- `ProcessGrowth_CombatAbilityThresholdMet_IncreasesAbility`
- `GetTimeModifier_CommandBelow200_Returns5`
- `CheckObedienceGrowth_ObedienceLevel3WithResistance_RemovesResistance`
- `CheckPain_VaginalWithLowExperience_AppliesHighMultiplier`

### AC Coverage Matrix

| Design Decision | AC Satisfied | Rationale |
|----------------|--------------|-----------|
| Create AbilityGrowthProcessorTests.cs | AC#1 | File existence verification |
| Create BasicChecksProcessorTests.cs | AC#2 | File existence verification |
| Create EquipmentProcessorTests.cs | AC#3 | File existence verification |
| Expand CharacterStateTests.cs | AC#4 | Add CharacterStateTracker-specific tests to existing file |
| Create ExperienceGrowthCalculatorTests.cs | AC#5 | File existence verification |
| Create PainStateCheckerTests.cs | AC#6 | File existence verification |
| Create VirginityManagerTests.cs | AC#7 | File existence verification |
| Create AbilityGrowthTests.cs | AC#8 | File existence verification |
| All test files compile warning-free | AC#10 | F708 TreatWarningsAsErrors enforcement |
| Each file has >= 3 test methods (estimated 3-15 per file) | AC#11 | Non-trivial coverage requirement |
| No TODO/FIXME/HACK markers in new code | AC#12 | Code quality gate |
| Tests use xUnit assertions, follow BaseTestClass pattern | AC#9 | Integration with existing test suite |
| Create F717/F718 DRAFT files | AC#13, AC#14 | Deferred scope tracking |

### Implementation Order

**Phase 1: Simple Data Structures** (AC#8)
1. AbilityGrowthTests.cs - Pure data structures, no dependencies

**Phase 2: Simple Processors** (AC#2, AC#3)
2. BasicChecksProcessorTests.cs - Simple calculations, minimal state
3. EquipmentProcessorTests.cs - Orchestration logic, mostly stubs

**Phase 3: Character State** (AC#4, AC#6, AC#7)
4. CharacterStateTrackerTests.cs - Add tracker-specific tests to existing file
5. PainStateCheckerTests.cs - Calculation logic with table lookups
6. VirginityManagerTests.cs - Complex state transitions

**Phase 4: Complex Logic** (AC#1, AC#5)
7. AbilityGrowthProcessorTests.cs - Most complex processor (12 sub-processors)
8. ExperienceGrowthCalculatorTests.cs - Complex thresholds and state changes

**Rationale**: Start with simple, build confidence, tackle complex last. This order minimizes risk of getting blocked on hard problems early.

### Build and Test Commands

**Build Verification** (AC#10):
```bash
dotnet build Era.Core.Tests/Era.Core.Tests.csproj
```
Expected: Exit code 0, "Build succeeded. 0 Warning(s)"

**Test Execution** (AC#9):
```bash
dotnet test Era.Core.Tests/Era.Core.Tests.csproj
```
Expected: All tests pass

**Coverage Report** (optional, not in ACs):
```bash
dotnet test Era.Core.Tests/Era.Core.Tests.csproj /p:CollectCoverage=true /p:CoverageOutputFormat=cobertura
```

### Risks and Mitigations

| Risk | Mitigation |
|------|------------|
| VariableStore state pollution between tests | BaseTestClass creates new DI container per test - no shared state |
| Mock complexity for Func parameters | Use simple lambda expressions instead of Moq setup |
| AC#4 file already exists - implementation conflict | Add new tests to existing file, verify >= 3 NEW methods |
| Test execution time with 60-100 new tests | xUnit parallel execution handles this automatically |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create `Era.Core.Tests/Training/AbilityGrowthProcessorTests.cs` with xUnit 3.x tests for AbilityGrowthProcessor (ProcessGrowth, GetExpLv, CheckExpUp) | [x] |
| 2 | 2 | Create `Era.Core.Tests/Training/BasicChecksProcessorTests.cs` with xUnit 3.x tests for BasicChecksProcessor (GetTimeModifier, GetFavorModifier, GetTechniqueModifier, GetMoodModifier, GetReasonModifier) | [x] |
| 3 | 3 | Create `Era.Core.Tests/Training/EquipmentProcessorTests.cs` with xUnit 3.x tests for EquipmentProcessor (orchestration dispatch only — 3 tests max, stub methods excluded) | [x] |
| 4 | 4 | Expand existing `Era.Core.Tests/CharacterStateTests.cs` with CharacterStateTracker-specific tests (add >= 3 new test methods) | [x] |
| 5 | 5 | Create `Era.Core.Tests/Character/ExperienceGrowthCalculatorTests.cs` with xUnit 3.x tests for ExperienceGrowthCalculator (CheckObedienceGrowth, CheckDesireGrowth, ProcessExperienceGain, ProcessServicePleasureExperience, ProcessPainPleasureExperience, ProcessSadisticPleasureExperience) | [x] |
| 6 | 6 | Create `Era.Core.Tests/Character/PainStateCheckerTests.cs` with xUnit 3.x tests for PainStateChecker (CheckPain, GetExpLv, GetPalamLv) | [x] |
| 7 | 7 | Create `Era.Core.Tests/Character/VirginityManagerTests.cs` with xUnit 3.x tests for VirginityManager (CheckLostVirginity, ProcessVaginalVirginity, ProcessAnalVirginity, ProcessKissVirginity, ProcessChastityEffects, DetermineVirginityLossMethod) | [x] |
| 8 | 8 | Create `Era.Core.Tests/Ability/AbilityGrowthTests.cs` with xUnit 3.x tests for AbilityGrowth record structs and GrowthResult class | [x] |
| 9 | 9 | Run `dotnet test Era.Core.Tests/Era.Core.Tests.csproj` and verify all tests pass | [x] |
| 10 | 10 | Run `dotnet build Era.Core.Tests/Era.Core.Tests.csproj` and verify zero warnings (F708 TreatWarningsAsErrors enforcement) | [x] |
| 11 | 11 | Run Grep on each of 7 new test files to verify >= 3 test methods (`[Fact]` or `[Theory]` attributes) per file | [x] |
| 12 | 12 | Run Grep on all new test files to verify zero technical debt markers (TODO/FIXME/HACK) | [x] |
| 13 | 13 | Verify `Game/agents/feature-717.md` DRAFT file exists (use Glob) | [x] |
| 14 | 14 | Verify `Game/agents/feature-718.md` DRAFT file exists (use Glob) | [x] |
| 15 | 15 | Add IO/Encoding/DI namespace test gap (5 files, 100% uncovered) to F718 scope by updating F718's Problem section to include these system-level utilities alongside Data/ loaders | [x] |
| 16 | 16 | Verify F717 is registered in `Game/agents/index-features.md` (use Grep for "717.*DRAFT") | [x] |
| 17 | 17 | Verify F718 is registered in `Game/agents/index-features.md` (use Grep for "718.*DRAFT") | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**

**Agent Dispatch Plan**:

| Phase | Tasks | Agent | Model | Notes |
|:-----:|:-----:|-------|:-----:|-------|
| 1a | T8, T2, T3 | implementer | sonnet | Simple files: AbilityGrowthTests (data structures), BasicChecksProcessorTests (calculations), EquipmentProcessorTests (orchestration only). Use BaseTestClass pattern, DI container. |
| 1b | T4, T6, T7 | implementer | sonnet | Character files: Expand CharacterStateTests (delegation), PainStateCheckerTests (table lookups), VirginityManagerTests (state tracking). Use DI container for resolution. |
| 1c | T1, T5 | implementer | sonnet | Complex files: AbilityGrowthProcessorTests (12 sub-processors), ExperienceGrowthCalculatorTests (thresholds, DI-registered factories). Use standard DI resolution for both classes. |
| 2 | T9 | ac-tester | haiku | Run `dotnet test Era.Core.Tests/Era.Core.Tests.csproj` to verify AC#9 (all tests pass) |
| 3 | T10 | ac-tester | haiku | Run `dotnet build Era.Core.Tests/Era.Core.Tests.csproj` to verify AC#10 (zero warnings) |
| 4 | T11 | ac-tester | haiku | Run Grep on each of 7 new test files to verify AC#11 (>= 3 test methods per file): `Grep(Era.Core.Tests/Training/AbilityGrowthProcessorTests.cs, "\[Fact\]|\[Theory\]", output_mode: "count")` and 6 others |
| 5 | T12 | ac-tester | haiku | Run Grep on all 7 new test files + 1 expanded to verify AC#12 (no TODO/FIXME/HACK): Run 8 individual Grep commands per AC Details (target files only, not all *Tests.cs) expecting zero matches |
| 6 | T13 | ac-tester | haiku | Run `Glob("Game/agents/feature-717.md")` to verify AC#13 (F717 DRAFT exists) |
| 7 | T14 | ac-tester | haiku | Run `Glob("Game/agents/feature-718.md")` to verify AC#14 (F718 DRAFT exists) |
| 8 | T15 | implementer | sonnet | Add IO/Encoding/DI namespace test gap (5 files, 100% uncovered) to F718 scope by updating F718's Problem section |
| 9 | T16 | ac-tester | haiku | Run `Grep("Game/agents/index-features.md", "717.*DRAFT")` to verify AC#16 (F717 index registration) |
| 10 | T17 | ac-tester | haiku | Run `Grep("Game/agents/index-features.md", "718.*DRAFT")` to verify AC#17 (F718 index registration) |

**Quality Gates**:
- After Phase 1: All test files must compile with zero warnings (enforced by AC#10/T10)
- After Phase 2: All tests must pass (enforced by AC#9/T9)
- After Phase 4: All files must have >= 3 test methods (enforced by AC#11/T11)
- After Phase 5: No technical debt markers in new code (enforced by AC#12/T12)

**Rationale**: Implementation uses single implementer agent for all test file creation to maintain consistency in test patterns and code style. The ac-tester agent handles all verification tasks (T9-T14) as these are binary pass/fail checks requiring no code changes. The implementation order follows Technical Design Phase 1-4 (simple to complex) to minimize risk and build confidence progressively.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F499 | [DONE] | Test strategy design (test-strategy.md) |
| Related | F407 | [DONE] | Training integration tests |
| Related | F708 | [DONE] | TreatWarningsAsErrors (build quality) |
| Related | F392 | [DONE] | Ability System Core migration (created target classes) |
| Related | F393 | [DONE] | Training Processing Core migration (created target classes) |
| Related | F396 | [DONE] | Character State Tracking migration (created target classes) |
| Successor | F717 | [DRAFT] | Tool test coverage (deferred scope) |
| Successor | F718 | [DRAFT] | Data loader test coverage (deferred scope) |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| xunit.v3 3.2.2 | Test framework | Low | Already in use, no version change needed |
| Moq 4.20.72 | Mocking library | Low | Already in use, available if needed for test isolation |
| coverlet.collector 6.0.2 | Coverage tool | Low | Already in use for coverage reporting |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| CI/CD pipeline (dotnet test) | LOW | New tests will be picked up automatically |
| F717 (Tool Tests) | LOW | May follow patterns established here |
| F718 (Data Loader Tests) | LOW | May follow patterns established here |

---

## Review Notes

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Tool test coverage (com-validator, SaveAnalyzer, YamlValidator, kojo-mapper) | Volume exceeds scope, different project types (Python/C#) | Feature | F717 | T13 (verify pre-created) |
| Data loader test gap (~27 loaders) | Volume exceeds scope | Feature | F718 | T14 (verify pre-created) |
| IO/Encoding/DI namespace test gap (5 files, 100% uncovered) | Volume exceeds scope, different concern area (system-level utilities vs business logic) | Feature | F718 | T15 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
| 2026-02-01 11:20 | START | implementer | Task 4, 6, 7 | - |
| 2026-02-01 11:20 | END | implementer | Task 4, 6, 7 | SUCCESS |
| 2026-02-01 11:21 | START | implementer | Task 1, 5 | - |
| 2026-02-01 11:21 | END | implementer | Task 1, 5 | SUCCESS |

---

## Links
- [test-strategy.md](../../Game/agents/designs/test-strategy.md)
- [CLAUDE.md Test Coverage Policy](../../CLAUDE.md#test-coverage-policy)
- [feature-499.md](feature-499.md) - Test strategy design
- [feature-407.md](feature-407.md) - Training integration tests
- [feature-708.md](feature-708.md) - TreatWarningsAsErrors
- [feature-392.md](feature-392.md) - Ability System Core migration
- [feature-393.md](feature-393.md) - Training Processing Core migration
- [feature-396.md](feature-396.md) - Character State Tracking migration
- [feature-717.md](feature-717.md) - Tool test coverage
- [feature-718.md](feature-718.md) - Data loader test coverage
