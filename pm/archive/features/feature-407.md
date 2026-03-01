# Feature 407: Training Integration Tests

## Status: [DONE]

## Phase: 7 (Technical Debt Consolidation)

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

## Created: 2026-01-08

---

## Summary

Add comprehensive integration tests for the Training namespace to verify end-to-end workflows across TrainingProcessor, StateChange hierarchy, and all sub-processors (Equipment, Orgasm, AbilityGrowth, Favor, Juel, Mark).

---

## Background

### Philosophy (Mid-term Vision)

**Phase 7: Technical Debt Consolidation**: Phase 5-6で蓄積した技術負債の解消 + Phase 8以降の基盤確立。

ISP分割(F404)、DI正式化(F405)、Processor完成(F406)後の統合テストにより、Phase 8以降の安定した基盤を保証する。

### Problem (Current Issue)

Phase 7 introduces significant architectural changes:
- IVariableStore interface segregation (F404) creates 4 specialized interfaces
- Callback DI formalization (F405) changes dependency injection patterns
- Equipment/OrgasmProcessor completion (F406) finalizes state change processing
- StateChange hierarchy completion (F402, F403) removes LegacyStateChange

Current TrainingProcessorTests.cs focuses on unit-level interface resolution and basic type checking. No tests verify:
- Multi-processor workflows (TrainingProcessor → EquipmentProcessor → OrgasmProcessor)
- StateChange propagation across processor boundaries
- DI container integration with new interface segregation
- End-to-end training result assembly with all sub-processors

### Goal (What to Achieve)

Create `Era.Core.Tests/TrainingIntegrationTests.cs` covering:
1. Full training workflow integration (all processors)
2. StateChange hierarchy usage across processor boundaries
3. DI container resolution with segregated interfaces
4. Multi-processor result aggregation
5. Equipment + Orgasm + Juel + Mark interaction
6. Ability growth with favor calculation integration
7. BasicChecksProcessor integration with TrainingProcessor
8. Error propagation across processor chain

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | TrainingIntegrationTests.cs exists | file | Glob | exists | TrainingIntegrationTests.cs | [x] |
| 2 | Direct processor sequence test (all processors) | code | Grep | contains | DirectProcessorSequence_AllProcessors_Success | [x] |
| 3 | StateChange propagation test | code | Grep | contains | StateChangePropagation_AcrossProcessors | [x] |
| 4 | DI resolution test (ISP interfaces + IVariableStore) | code | Grep | contains | DIResolution_SegregatedInterfaces | [x] |
| 5 | Multi-processor result aggregation test | code | Grep | contains | ResultAggregation_MultipleProcessors | [x] |
| 6 | Equipment+Orgasm interaction test | code | Grep | contains | EquipmentOrgasmInteraction | [D] |
| 7 | AbilityGrowth+Favor integration test | code | Grep | contains | AbilityGrowthFavorIntegration | [x] |
| 8 | BasicChecks+TrainingProcessor test | code | Grep | contains | BasicChecksTrainingIntegration | [x] |
| 9 | Error propagation test | code | Grep | contains | ErrorPropagation_AcrossChain | [x] |
| 10 | Integration category trait applied | code | Grep | contains | Trait("Category", "Integration") | [x] |
| 11 | C# build succeeds | build | dotnet | succeeds | - | [x] |
| 12 | Integration tests pass | test | dotnet | succeeds | Category=Integration | [x] |

### AC Details

**AC#1**: File exists at `Era.Core.Tests/TrainingIntegrationTests.cs`

**AC#2**: Test method name contains "DirectProcessorSequence_AllProcessors_Success" - verifies all sub-processors (Equipment, Orgasm, AbilityGrowth, Favor, Juel, Mark, BasicChecks) can be called directly in sequence and return success. TrainingProcessor.Process() does NOT orchestrate all processors (Equipment, Orgasm, Favor are commented out); this test calls processors directly to verify integration.

**AC#3**: Test method name contains "StateChangePropagation_AcrossProcessors" - verifies StateChange objects (e.g., SourceChange, NowExChange, ExChange, AbilityChange, TalentChange, ExpChange) are correctly created by one processor and included in TrainingResult.Changes.

**AC#4**: Test method name contains "DIResolution_SegregatedInterfaces" - verifies that (1) original IVariableStore resolves, and (2) the 3 new ISP-compliant interfaces (ITrainingVariables, ICharacterStateVariables, IJuelVariables per F404) resolve from DI container. Note: IVariableStore was not reduced; F404 created additional segregated interfaces.

**AC#5**: Test method name contains "ResultAggregation_MultipleProcessors" - verifies TrainingResult correctly aggregates changes from multiple processors into a single result.

**AC#6**: Test method name contains "EquipmentOrgasmInteraction" - verifies equipment state affects orgasm processing (e.g., TEQUIP values influence orgasm results). **[DEFERRED to Phase 10]**: EquipmentProcessor handler methods are stubs. Full data flow testing deferred to Phase 10 (COM Implementation) when EQUIP_COM42-189 are migrated. See `full-csharp-architecture.md` Phase 10 "F407 Deferred AC" section.

**AC#7**: Test method name contains "AbilityGrowthFavorIntegration" - verifies ability growth triggers favor calculation and both results are included in TrainingResult. **Note**: FavorCalculator integration is also commented out in TrainingProcessor. This test will call AbilityGrowthProcessor.ProcessGrowth() and FavorCalculator.CalculateFavor(target, CharacterId.Reimu, 0, 0) directly in sequence (they are peer processors, not chained).

**AC#8**: Test method name contains "BasicChecksTrainingIntegration" - verifies BasicChecksProcessor provides correct time/favor modifiers to TrainingProcessor.

**AC#9**: Test method name contains "ErrorPropagation_AcrossChain" - verifies failures in sub-processors (e.g., invalid EquipmentProcessor input) propagate correctly to TrainingProcessor.Process() result.

**AC#10**: All test methods in TrainingIntegrationTests.cs have `[Trait("Category", "Integration")]` attribute for filter execution.

**AC#11**: Build verification:
```bash
dotnet build Era.Core.Tests/
```

**AC#12**: Test verification:
```bash
dotnet test Era.Core.Tests/ --filter "Category=Integration"
```

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | Create Era.Core.Tests/TrainingIntegrationTests.cs with 8 integration test methods (AC#2-9) | [x] |
| 2 | 11 | Verify C# build succeeds | [x] |
| 3 | 12 | Verify integration tests pass | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->
<!-- Note: AC#1-10 combined in Task#1 as they represent a single file creation with multiple test methods -->
<!-- AC#11-12 are verification steps after implementation -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F404 | IVariableStore ISP segregation provides interfaces to test |
| Predecessor | F405 | Callback DI formalization provides DI patterns to verify |
| Predecessor | F406 | Equipment/OrgasmProcessor completion provides implementations to test |
| Related | F402 | StateChange Equipment/Orgasm types used in tests |
| Related | F403 | Character namespace StateChange types used in tests |
| Successor | F408 | Phase 7 Post-Phase Review will validate integration test coverage |

---

## Test Structure Reference

Based on existing patterns in `Era.Core.Tests/`:

### Test Class Structure
```csharp
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Era.Core.Training;
using Era.Core.Types;

namespace Era.Core.Tests;

/// <summary>
/// Integration tests for Training namespace workflows.
/// Feature 407: Verifies multi-processor interaction, StateChange propagation,
/// and DI integration after Phase 7 refactoring.
/// </summary>
[Trait("Category", "Integration")]
public class TrainingIntegrationTests : BaseTestClass
{
    // Test methods follow {Method}_{Scenario}_{ExpectedResult} convention
}
```

### Test Method Pattern
```csharp
/// <summary>
/// AC#2: Full workflow test - all processors invoked successfully.
/// </summary>
[Fact]
[Trait("AC", "2")]
public void FullWorkflow_AllProcessors_Success()
{
    // Arrange
    var processor = Services.GetRequiredService<ITrainingProcessor>();
    var character = CharacterId.Meiling;
    var command = new CommandId(100);

    // Act
    var result = processor.Process(character, command);

    // Assert
    Assert.IsType<Result<TrainingResult>.Success>(result);
    var trainingResult = ((Result<TrainingResult>.Success)result).Value;
    Assert.NotEmpty(trainingResult.Changes); // Verify changes from all processors
}
```

### Coverage Requirements

Per `Era.Core.Tests/README.md`:
- New features require minimum 85% line coverage for the code under test (processors being tested)
- Coverage measurement applies to processors, not the test file itself
- Tests must be independent (no shared mutable state)

---

## Implementation Notes

### Test Scenarios

1. **Direct Processor Sequence** (AC#2):
   - Call each sub-processor directly in sequence (not via TrainingProcessor.Process())
   - Verify each processor returns valid result
   - Aggregate results to simulate full workflow integration

2. **StateChange Propagation** (AC#3):
   - Process training command that triggers equipment and orgasm
   - Verify TrainingResult.Changes contains SourceChange, BaseChange, NowExChange, etc.
   - Assert StateChange types match F402 hierarchy

3. **DI Resolution** (AC#4):
   - Resolve IVariableStore, ITrainingVariables, ICharacterStateVariables, IJuelVariables
   - Assert all interfaces return non-null instances
   - Verify all ISP interfaces resolve to VariableStore implementation type

4. **Result Aggregation** (AC#5):
   - Process command triggering multiple processors
   - Verify Changes list length matches expected processor count
   - Assert no duplicate changes

5. **Equipment+Orgasm Interaction** (AC#6) [BLOCKED]:
   - Resolve ITEquipVariables from Services.GetRequiredService<ITEquipVariables>()
   - Set TEQUIP values via ITEquipVariables methods
   - Process orgasm command
   - Verify orgasm result reflects equipment state
   - **Note**: Currently blocked - EquipmentProcessor handlers are stubs

6. **AbilityGrowth+Favor** (AC#7):
   - Process command that grows abilities
   - Verify favor calculation triggered
   - Assert Changes contains both AbilityChange and SourceChange (favor)

7. **BasicChecks+Training** (AC#8):
   - Verify BasicChecksProcessor.GetTimeModifier() called during Process()
   - Assert time increment applied to result
   - Verify favor modifier affects favor calculation

8. **Error Propagation** (AC#9):
   - Pass invalid command to TrainingProcessor
   - Assert Result<TrainingResult>.Failure returned
   - Verify error message propagates from sub-processor

9. **Integration Category** (AC#10):
   - Class-level `[Trait("Category", "Integration")]` applies to ALL methods in the class (xUnit behavior)
   - Individual methods do NOT need `[Trait("Category", "Integration")]` when class has it
   - Enables selective execution: `dotnet test --filter "Category=Integration"`

### Mock Strategy

Use existing `BaseTestClass` infrastructure:
- DI container pre-configured with all services
- TestHelpers for common setup operations
- Mocks/ directory for mock objects if needed

Prefer real implementations over mocks for integration tests to verify actual behavior.

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md#phase-7-technical-debt-consolidation-new) - Phase 7 definition (lines 1782-1932)
- [feature-398.md](feature-398.md) - Phase 7 Planning (parent feature)
- [feature-404.md](feature-404.md) - IVariableStore ISP Segregation (dependency)
- [feature-405.md](feature-405.md) - Callback DI Formalization (dependency)
- [feature-406.md](feature-406.md) - Equipment/OrgasmProcessor Completion (dependency)
- [feature-402.md](feature-402.md) - StateChange Equipment/Orgasm Migration (completed)
- [feature-403.md](feature-403.md) - Character Namespace StateChange Migration
- [Era.Core.Tests/README.md](../../Era.Core.Tests/README.md) - Test conventions and structure
- [Era.Core.Tests/TrainingProcessorTests.cs](../../Era.Core.Tests/TrainingProcessorTests.cs) - Existing unit tests pattern

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-01-09 11:03 | START | implementer | Task 1 | - |
| 2026-01-09 11:03 | END | implementer | Task 1 | SUCCESS |
| 2026-01-09 11:15 | END | feature-reviewer | post (retry) | READY |
| 2026-01-09 11:15 | END | feature-reviewer | doc-check | READY |
| 2026-01-09 11:16 | END | - | AC#6 deferred to Phase 10 | full-csharp-architecture.md updated |
| 2026-01-09 11:16 | END | finalizer | Feature complete | [DONE] |
