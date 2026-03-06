# Feature 460: Phase 7 Technical Debt Resolution

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

## Type: engine

## Created: 2026-01-11

---

## Summary

Resolve Phase 7 deferred technical debt items (TrainingProcessor integration, well-known indices, callbacks, TODO cleanup).

**Scope** (from architecture.md Phase 12):
1. TrainingProcessor integration (uncomment Equipment/Orgasm/Favor code)
2. CharacterFlagIndex.Favor well-known index addition
3. ExperienceGrowthCalculator: Replace placeholder with TCVarIndex.Actor reference
4. JUEL callback implementation (F405 pattern)
5. ITEquipVariables injection for VirginityManager
6. TODO comment cleanup

**Output**:
- `Era.Core/Training/TrainingProcessor.cs` - Uncommented integration code
- `Era.Core/Types/CharacterFlagIndex.cs` - Favor well-known index added
- `Era.Core/Character/ExperienceGrowthCalculator.cs` - TCVarIndex.Actor reference + JUEL callback
- `Era.Core/Character/VirginityManager.cs` - ITEquipVariables injected
- `Era.Core.Tests/OrgasmProcessorEquivalenceTests.cs` - TODO comments removed

---

## Background

### Philosophy (Mid-term Vision)

**Phase 12: COM Implementation** - Migrate all 150+ COMF*.ERB training command implementations to C# using Phase 4 design patterns (Strongly Typed IDs, DI, SRP), ensuring legacy equivalence and establishing single source of truth for game training logic.

### Problem (Current Issue)

Phase 7 deferred technical debt to Phase 12:
- TrainingProcessor has 17 lines of dead code (equipment/orgasm integration commented out)
- Well-known indices (Favor, Actor) not added to index enums
- JUEL/TEQUIP callbacks not implemented (F405 pattern available)
- 14 outdated TODO comments in test files

**Rationale**: F406 completed Equipment/OrgasmProcessor implementations. Phase 12 provides COM context needed for full integration.

### Goal (What to Achieve)

1. **Uncomment TrainingProcessor integration code** (Equipment/Orgasm/Favor)
2. **Add CharacterFlagIndex.Favor** well-known index
3. **Replace ExperienceGrowthCalculator placeholder** with TCVarIndex.Actor reference
4. **Implement JUEL callback** using F405 pattern
5. **Inject ITEquipVariables** into VirginityManager
6. **Remove outdated TODO comments**
7. **Verify build and tests pass** after integration

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | TrainingProcessor Equipment call uncommented | code | Grep | contains | "ProcessEquipment\(target\)" | [x] |
| 2 | TrainingProcessor Orgasm call uncommented | code | Grep | contains | "ProcessOrgasm\(target," | [x] |
| 3 | TrainingProcessor Favor calculation uncommented | code | Grep | contains | "CalculateFavor\(target" | [x] |
| 4 | CharacterFlagIndex.Favor well-known index exists | code | Grep | contains | "public static readonly CharacterFlagIndex Favor" | [x] |
| 5 | ExperienceGrowthCalculator placeholder Actor replaced | code | Grep | not_contains | "Actor = new(999)" | [x] |
| 6 | JuelIndex strongly-typed ID exists | code | Grep | contains | "public readonly record struct JuelIndex" | [x] |
| 7 | JUEL callback implemented | code | Grep | contains | "Func\<CharacterId, JuelIndex, int\>" | [x] |
| 8 | TEQUIP callback ITEquipVariables injected | code | Grep | contains | "ITEquipVariables" | [x] |
| 9 | OrgasmProcessorEquivalenceTests TODO cleanup | code | Grep | not_contains | "TODO: When IJuelVariables interface is implemented" | [x] |
| 10 | ExperienceGrowthCalculator TODO cleanup | code | Grep | not_contains | "TODO: Add CupChange" | [x] |
| 11 | Build succeeds after integration | build | Bash | succeeds | "dotnet build Era.Core" | [x] |
| 12 | All tests pass | test | Bash | succeeds | "dotnet test Era.Core.Tests" | [x] |

### AC Details

**AC#1**: TrainingProcessor Equipment call uncommented
- Test: Grep pattern="ProcessEquipment\(target\)" path="Era.Core/Training/TrainingProcessor.cs"
- Verifies commented Equipment processor integration code is uncommented

**AC#2**: TrainingProcessor Orgasm call uncommented
- Test: Grep pattern="ProcessOrgasm\(target," path="Era.Core/Training/TrainingProcessor.cs"
- Verifies commented Orgasm processor integration code is uncommented (ProcessOrgasm takes two arguments: target, target)

**AC#3**: TrainingProcessor Favor calculation uncommented
- Test: Grep pattern="CalculateFavor\(target" path="Era.Core/Training/TrainingProcessor.cs"
- Verifies commented Favor calculation code is uncommented

**AC#4**: CharacterFlagIndex.Favor well-known index exists
- Test: Grep pattern="public static readonly CharacterFlagIndex Favor" path="Era.Core/Types/CharacterFlagIndex.cs"
- Adds `public static readonly CharacterFlagIndex Favor = new(2);` (CFLAG:好感度 index is 2 per Game/CSV/CFLAG.csv)

**AC#5**: ExperienceGrowthCalculator placeholder Actor replaced
- Test: Grep pattern="Actor = new(999)" path="Era.Core/Character/ExperienceGrowthCalculator.cs" (not_contains)
- Replace placeholder `Actor = new(999)` at L51 with reference to `TCVarIndex.Actor` (already exists at TCVarIndex.cs L31 with correct value 116)
- Also remove the L50 TODO comment "Add well-known index once available" since TCVarIndex.Actor IS the well-known index

**AC#6**: JuelIndex strongly-typed ID exists
- Test: Grep pattern="public readonly record struct JuelIndex" path="Era.Core/Types/JuelIndex.cs"
- Create JuelIndex following TCVarIndex pattern (required before JUEL callback can be implemented)

**AC#7**: JUEL callback implemented
- Test: Grep pattern="Func\<CharacterId, JuelIndex, int\>" path="Era.Core/Character/ExperienceGrowthCalculator.cs" (escaped angle brackets)
- Apply F405 callback pattern for JUEL accessor to resolve TODO at L106

**AC#8**: TEQUIP callback ITEquipVariables injected
- Test: Grep pattern="ITEquipVariables" path="Era.Core/Character/VirginityManager.cs"
- Inject ITEquipVariables dependency to resolve TODO at L60 (determine VirginityLossMethod from TEQUIP)
- Additional verification: Grep pattern="TODO.*TEQUIP" path="Era.Core/Character/VirginityManager.cs" should return 0 matches after implementation

**AC#9**: OrgasmProcessorEquivalenceTests TODO cleanup
- Test: Grep pattern="TODO: When IJuelVariables interface is implemented" path="Era.Core.Tests/OrgasmProcessorEquivalenceTests.cs"
- Expected: 0 matches (remove 12 outdated "When IJuelVariables interface is implemented" comments - IJuelVariables now exists)

**AC#10**: ExperienceGrowthCalculator TODO cleanup
- Test: Grep pattern="TODO: Add CupChange" path="Era.Core/Character/ExperienceGrowthCalculator.cs"
- Expected: 0 matches (remove CupChange TODO at L392 - CUP is session-temporary, no StateChange needed)

**AC#11**: Build succeeds after integration
- Test: Bash command="dotnet build Era.Core"
- Verifies all changes compile successfully

**AC#12**: All tests pass
- Test: Bash command="dotnet test Era.Core.Tests"
- Verifies Equipment/Orgasm/Favor integration works correctly and no regressions

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Uncomment TrainingProcessor Equipment and Orgasm calls (L85-97) | [x] |
| 2 | 4 | Add CharacterFlagIndex.Favor = new(2) following TCVarIndex pattern | [x] |
| 3 | 3 | Uncomment TrainingProcessor Favor calculation (L103-107) - depends on Task 2 | [x] |
| 4 | 5 | Replace placeholder Actor definition in ExperienceGrowthCalculator with TCVarIndex.Actor reference and remove L50 TODO comment | [x] |
| 5 | 6 | Create JuelIndex strongly-typed ID in Era.Core/Types/JuelIndex.cs | [x] |
| 6 | 7 | Add JUEL callback to ExperienceGrowthCalculator using F405 pattern (resolves L106 TODO) - depends on Task 5 | [x] |
| 7 | 8 | Inject ITEquipVariables dependency into VirginityManager | [x] |
| 8 | 9 | Remove outdated "When IJuelVariables" TODO comments from OrgasmProcessorEquivalenceTests | [x] |
| 9 | 10 | Remove CupChange TODO from ExperienceGrowthCalculator (L392) | [x] |
| 10 | 11,12 | Verify build succeeds and all tests pass | [x] |

<!-- 12 ACs = 10 Tasks (grouped by logical operations where multiple ACs are related) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**1. TrainingProcessor Integration**:

| File | Action |
|------|--------|
| `Era.Core/Training/TrainingProcessor.cs` | Uncomment Equipment processor call |
| `Era.Core/Training/TrainingProcessor.cs` | Uncomment Orgasm processor call |
| `Era.Core/Training/TrainingProcessor.cs` | Uncomment Favor calculation |

**2. Well-Known Index Additions**:

| File | Index | Pattern | Purpose |
|------|-------|---------|---------|
| `Era.Core/Types/CharacterFlagIndex.cs` | `Favor` | `public static readonly CharacterFlagIndex Favor = new(2);` | CFLAG:好感度 index is 2 per Game/CSV/CFLAG.csv |

**Note**: TCVarIndex.Actor already exists at TCVarIndex.cs L31. ExperienceGrowthCalculator L51 should reference it instead of placeholder.

**3. Strongly-Typed ID Creation**:

| File | Type | Pattern |
|------|------|---------|
| `Era.Core/Types/JuelIndex.cs` | `JuelIndex` | Create following TCVarIndex pattern (public readonly record struct) |

**4. Callback/Dependency Implementations**:

| File | Location | Change |
|------|----------|--------|
| `Era.Core/Character/ExperienceGrowthCalculator.cs` | L50-51, L106 | Replace placeholder `Actor = new(999)` with `TCVarIndex.Actor`, add JUEL callback using JuelIndex |
| `Era.Core/Character/VirginityManager.cs` | L60 | Inject `ITEquipVariables` to determine VirginityLossMethod |

**5. TODO Comment Cleanup**:

| File | Pattern | Action |
|------|---------|--------|
| `Era.Core.Tests/OrgasmProcessorEquivalenceTests.cs` | "TODO: When IJuelVariables" | Remove outdated comments (IJuelVariables is now implemented) |
| `Era.Core/Character/ExperienceGrowthCalculator.cs` | "TODO: Add CupChange" at L392 | Remove (CUP is session-temporary) |

**Note**: L50 TODO ("Add well-known index once available") will be resolved by Task 4 (TCVarIndex.Actor reference). L106 TODO ("Add JUEL callback") will be resolved by Task 6 (JUEL callback implementation).

### F405 Callback Pattern Reference

```csharp
// F405 established callback pattern
public class ExperienceGrowthCalculator
{
    private readonly Func<CharacterId, JuelIndex, int> _getJuel;

    public ExperienceGrowthCalculator(Func<CharacterId, JuelIndex, int> getJuel)
    {
        _getJuel = getJuel;
    }

    private int GetJuel(CharacterId id, JuelIndex index)
        => _getJuel(id, index);
}
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F406 | EquipmentProcessor/OrgasmProcessor implementations |
| Predecessor | F405 | Callback implementation pattern |
| Predecessor | F459 | COM implementations complete (provides context) |
| Successor | F461 | Phase 9 System Integration |

---

## Links

- [feature-406.md](feature-406.md) - EquipmentProcessor (Equipment/Orgasm integration)
- [feature-405.md](feature-405.md) - Callback pattern reference
- [feature-450.md](feature-450.md) - Phase 12 Planning
- [feature-459.md](feature-459.md) - System Commands Migration (provides COM context)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 12 technical debt section

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->

- **2026-01-12 FL iter1**: [resolved] Phase2-Validate - AC#4 CharacterFlagIndex design: Added specific implementation pattern matching TCVarIndex
- **2026-01-12 FL iter1**: [resolved] Phase2-Validate - AC#5: Corrected to use TCVarIndex.Actor reference instead of claiming it needs to be added
- **2026-01-12 FL iter1**: [resolved] Phase2-Validate - AC#7: Changed from Func callback to ITEquipVariables injection approach
- **2026-01-12 FL iter1**: [resolved] Phase2-Validate - AC#8/9: Updated regex patterns to match actual TODO comment format
- **2026-01-12 FL iter1**: [resolved] Phase2-Validate - AC#10/11: Changed to standard build/test verification (removed non-existent TrainingIntegrationTests)
- **2026-01-12 FL iter2**: [resolved] Phase2-Validate - AC#2: Fixed ProcessOrgasm pattern to match 2-arg signature
- **2026-01-12 FL iter2**: [resolved] Phase2-Validate - AC#4: Specified Favor = new(2) per CFLAG.csv
- **2026-01-12 FL iter2**: [resolved] Phase2-Validate - Task 1: Added note about hardcoded CharacterId.Reimu
- **2026-01-12 FL iter2**: [resolved] Phase2-Validate - Added clarification that L50/L106 TODOs resolved by Tasks 3/4
- **2026-01-12 FL iter3**: [resolved] Phase2-Validate - Added AC#6 JuelIndex type creation (prerequisite for JUEL callback)
- **2026-01-12 FL iter4**: [resolved] Phase2-Validate - AC#5: Clarified as removing duplicate definition and using TCVarIndex.Actor
- **2026-01-12 FL iter5**: [resolved] Phase2-Validate - AC#7: Added note about angle bracket escaping in Grep pattern
- **2026-01-12 FL iter5**: [resolved] Phase2-Validate - Task split: Separated Equipment/Orgasm (Task 1) from Favor (Task 3) due to CharacterFlagIndex.Favor dependency
- **2026-01-12 FL iter6**: [resolved] Phase2-Validate - Implementation Contract Note: Fixed task number references (Task 4 and Task 6, not Task 3 and Task 5)
- **2026-01-12 FL iter7**: [resolved] Phase3-Maintainability - AC#5/Task 4: Changed 'duplicate' to 'placeholder' for terminology accuracy
- **2026-01-12 FL iter7**: [resolved] Phase3-Maintainability - AC#8: Added TODO removal verification
- **2026-01-12 FL iter8**: [resolved] Phase2-Validate - Task 4/AC#5: Added explicit L50 TODO removal to task description
- **2026-01-12 FL iter9**: [resolved] Phase2-Validate - AC#7: Fixed Grep pattern to properly escape angle brackets
- **2026-01-12 FL iter9**: [resolved] Phase2-Validate - Task comment: Revised to remove misleading 1:1 claim

### 残課題 (Out-of-Scope)
- TrainingProcessor L102 CalculateFavor uses hardcoded CharacterId.Reimu (player=Reimu). Should use IComContext.Actor from training context. → **Phase 12 Task 5.9** ([architecture.md](designs/full-csharp-architecture.md#phase-12-com-implementation-was-phase-11))

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 20:51 | create | feature-builder | Created from F450 Phase 12 Planning | PROPOSED |
| 2026-01-12 15:55 | START | implementer | Task 1 | - |
| 2026-01-12 15:55 | END | implementer | Task 1 | SUCCESS |
| 2026-01-12 15:56 | START | implementer | Task 2 | - |
| 2026-01-12 15:56 | END | implementer | Task 2 | SUCCESS |
| 2026-01-12 15:57 | START | implementer | Task 3 | - |
| 2026-01-12 15:57 | END | implementer | Task 3 | SUCCESS |
| 2026-01-12 15:59 | START | implementer | Task 4 | - |
| 2026-01-12 15:59 | END | implementer | Task 4 | SUCCESS |
| 2026-01-12 16:01 | START | implementer | Task 5 | - |
| 2026-01-12 16:01 | END | implementer | Task 5 | SUCCESS |
| 2026-01-12 16:02 | START | implementer | Task 6 | - |
| 2026-01-12 16:02 | END | implementer | Task 6 | SUCCESS |
| 2026-01-12 16:05 | START | implementer | Task 7 | - |
| 2026-01-12 16:05 | END | implementer | Task 7 | SUCCESS |
| 2026-01-12 16:10 | START | implementer | Task 8 | - |
| 2026-01-12 16:10 | END | implementer | Task 8 | SUCCESS |
| 2026-01-12 16:11 | START | implementer | Task 9 | - |
| 2026-01-12 16:11 | END | implementer | Task 9 | SUCCESS |
| 2026-01-12 16:12 | START | opus | Task 10 | - |
| 2026-01-12 16:12 | END | opus | Task 10 | SUCCESS (build 0 errors, 951 tests passed) |
