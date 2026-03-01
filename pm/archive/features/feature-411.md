# Feature 411: ISP Consumer Migration (Remaining)

## Status: [CANCELLED]

## Cancellation Reason

F404 実装中に debugger が BUILD_FAIL を修正する際、本 Feature のスコープである全 consumer も修正完了。F411 の作業は F404 に吸収された。

**経緯**:
1. F404 `/fl` で F411 を分離 (FL iter5: "Task#9 + Missing consumers → Deferred to F411")
2. F404 `/do` Phase 4 で Tasks 1-5 完了後、BUILD_FAIL 発生
3. debugger が全 consumer を修正（F411 スコープ含む）してビルド成功
4. 結果として F404 commit に F411 の全作業が含まれた

**Commit**: `dfb37b1 feat(F404): Complete Character Namespace StateChange Migration`

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

Migrate remaining IVariableStore consumers to use ISP-segregated interfaces defined in F404. This feature covers consumers not addressed in F404: MarkSystem (4 calculator classes), AbilityGrowthProcessor, VirginityManager, ExperienceGrowthCalculator, EquipmentProcessor, OrgasmProcessor, and DI configuration.

---

## Background

### Philosophy (Mid-term Vision)

**Phase 7: Technical Debt Consolidation**: Phase 5-6で蓄積した技術負債の解消 + Phase 8以降の基盤確立。

F404 establishes ISP-compliant interfaces. F411 completes the migration by updating all remaining consumers to use these interfaces.

### Problem (Current Issue)

After F404, the following consumers still use the monolithic IVariableStore:

| Consumer | Required Interfaces | Current Status |
|----------|---------------------|----------------|
| MarkSystem (4 calculators) | ITrainingVariables + ICharacterStateVariables | Uses IVariableStore |
| AbilityGrowthProcessor | IVariableStore + ITrainingVariables | Uses IVariableStore |
| VirginityManager | IVariableStore + ICharacterStateVariables | Uses IVariableStore |
| ExperienceGrowthCalculator | IVariableStore | Uses IVariableStore |
| EquipmentProcessor | TBD (skeleton) | Uses IVariableStore |
| OrgasmProcessor | TBD (skeleton) | Uses IVariableStore |

### Goal (What to Achieve)

Update all remaining consumers to use only the interfaces they need, completing ISP application across Era.Core.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SubmissionMarkCalculator uses ITrainingVariables + ICharacterStateVariables | build | - | succeeds | - | [ ] |
| 2 | PleasureMarkCalculator uses ITrainingVariables + ICharacterStateVariables | build | - | succeeds | - | [ ] |
| 3 | ResistanceMarkCalculator uses ITrainingVariables + ICharacterStateVariables | build | - | succeeds | - | [ ] |
| 4 | PainMarkCalculator uses ITrainingVariables + ICharacterStateVariables | build | - | succeeds | - | [ ] |
| 5 | AbilityGrowthProcessor uses IVariableStore + ITrainingVariables | build | - | succeeds | - | [ ] |
| 6 | VirginityManager uses IVariableStore + ICharacterStateVariables | build | - | succeeds | - | [ ] |
| 7 | ExperienceGrowthCalculator uses IVariableStore | build | - | succeeds | - | [ ] |
| 8 | DI configuration updated for all interfaces | build | - | succeeds | - | [ ] |
| 9 | Era.Core builds with updated consumers | build | - | succeeds | - | [ ] |
| 10 | Era.Core.Tests pass with updated consumers | test | - | succeeds | - | [ ] |

### AC Details

**AC1-4**: MarkSystem calculator migration
- **Test**: `dotnet build Era.Core`
- **Expected**: Each calculator uses segregated interfaces

**AC5-7**: Individual consumer migration
- **Test**: `dotnet build Era.Core`
- **Expected**: Each consumer uses only required interfaces

**AC8**: DI configuration
- **Test**: `dotnet build Era.Core`
- **Expected**: ServiceCollectionExtensions registers all interface bindings

**AC9-10**: Build verification
- **Test**: `dotnet build Era.Core` / `dotnet test Era.Core.Tests`
- **Expected**: All builds and tests pass

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Refactor SubmissionMarkCalculator to use ITrainingVariables + ICharacterStateVariables | [ ] |
| 2 | 2 | Refactor PleasureMarkCalculator to use ITrainingVariables + ICharacterStateVariables | [ ] |
| 3 | 3 | Refactor ResistanceMarkCalculator to use ITrainingVariables + ICharacterStateVariables | [ ] |
| 4 | 4 | Refactor PainMarkCalculator to use ITrainingVariables + ICharacterStateVariables | [ ] |
| 5 | 5 | Refactor AbilityGrowthProcessor to use IVariableStore + ITrainingVariables | [ ] |
| 6 | 6 | Refactor VirginityManager to use IVariableStore + ICharacterStateVariables | [ ] |
| 7 | 7 | Refactor ExperienceGrowthCalculator to use IVariableStore | [ ] |
| 8 | 8 | Update DI configuration in ServiceCollectionExtensions | [ ] |
| 9 | 9 | Verify Era.Core build | [ ] |
| 10 | 10 | Verify Era.Core.Tests pass | [ ] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

**Requires**: F404 (IVariableStore ISP Segregation)

**Rationale**: F404 defines the interfaces that F411 consumers will use.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| - | - | - | - | - |

---

## Links

**Follows**: [F404](feature-404.md) (IVariableStore ISP Segregation)
