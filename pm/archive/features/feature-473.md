# Feature 473: SCOMF Full Implementation

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

## Created: 2026-01-13

---

## Summary

**Phase 13: DDD Foundation - SCOMF Scenario Implementation**

Implement full logic for SpecialTraining service (ISpecialTraining), replacing Phase 9 stubs (F435) for SCOMF1-16 special training scenarios. Uses STAIN/DOWNBASE infrastructure from F469.

**Prerequisite**: F469 (SCOMF Variable Infrastructure) must be completed first.

**Output**:
- `Era.Core/Training/SpecialTraining.cs` - Full implementation replacing stub
- Unit tests in `Era.Core.Tests/Training/SpecialTrainingTests.cs`

**Volume**: ~3000 lines (16 scenarios + tests)

---

## Background

### Philosophy (Mid-term Vision)

**Phase 13: DDD Foundation** - Establish Domain-Driven Design foundation through Aggregate Root, Repository, and UnitOfWork patterns. SCOMF completion applies these patterns to special training scenarios, enabling proper variable manipulation through IVariableStore abstraction.

### Problem (Current Issue)

Phase 9 SCOMF implementation (F435) created stub handlers for SCOMF1-16 special training commands. The core `SpecialTraining.ExecuteScenario` method returns `Result.Fail("NotImplemented")` for all scenarios.

F469 established the variable infrastructure (STAIN/DOWNBASE in IVariableStore), but SpecialTraining still contains stub implementation.

### Goal (What to Achieve)

1. **Implement SpecialTraining** full logic for SCOMF1-16 scenarios
2. **Enable IsScenarioAvailable** with proper prerequisite checks
3. **Remove stub responses** (no "NotImplemented" returns)
4. **Test scenario execution** through unit tests
5. **Eliminate technical debt** (no TODO/FIXME/HACK comments)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SpecialTraining stub removed | code | Grep | not_contains | "NotImplemented.*SCOMF" | [x] |
| 2 | SpecialTraining uses IVariableStore | code | Grep | contains | "IVariableStore" | [x] |
| 3 | IsScenarioAvailable validates range | test | Bash | succeeds | "dotnet test --filter IsScenarioAvailable" | [x] |
| 4 | SCOMF1 scenario implemented | code | Grep | matches | "ExecuteScomf1\\s*\\(" | [x] |
| 5 | SCOMF16 scenario implemented | code | Grep | matches | "ExecuteScomf16\\s*\\(" | [x] |
| 6 | SpecialTraining tests pass | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~SpecialTrainingTests" | [x] |
| 7 | Build succeeds | build | Bash | succeeds | "dotnet build Era.Core" | [x] |
| 8 | Zero technical debt | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 9 | All 16 scenarios implemented | code | Grep | count_equals(16) | "ExecuteScomf\\d+" | [x] |
| 10 | Invalid scenario rejected (Neg) | test | Bash | succeeds | "dotnet test --filter InvalidScenario" | [x] |

### AC Details

**AC#1**: SpecialTraining stub removal
- Test: Grep pattern="NotImplemented.*SCOMF" path="Era.Core/Training/SpecialTraining.cs"
- Expected: 0 matches
- Verifies: F435 stub replaced with full implementation

**AC#2**: SpecialTraining uses IVariableStore
- Test: Grep pattern="IVariableStore" path="Era.Core/Training/SpecialTraining.cs"
- Expected: Contains IVariableStore dependency injection
- Verifies: Proper variable access pattern through abstraction

**AC#3**: IsScenarioAvailable range validation (behavior test)
- Test: `dotnet test --filter IsScenarioAvailable`
- Test cases: IsScenarioAvailable(0)=false, (1)=true, (16)=true, (17)=false
- Expected: All boundary tests pass
- Verifies: IsScenarioAvailable correctly validates 1-16 range

**AC#4**: SCOMF1 scenario method exists
- Test: Grep pattern="ExecuteScomf1\\s*\\(" path="Era.Core/Training/SpecialTraining.cs"
- Expected: Contains ExecuteScomf1 method definition
- Verifies: First scenario implemented (シックスナイン)

**AC#5**: SCOMF16 scenario method exists
- Test: Grep pattern="ExecuteScomf16\\s*\\(" path="Era.Core/Training/SpecialTraining.cs"
- Expected: Contains ExecuteScomf16 method definition
- Verifies: Last scenario implemented (授乳手コキ)

**AC#6**: SpecialTraining unit tests pass
- Test: `dotnet test --filter FullyQualifiedName~SpecialTrainingTests`
- Expected: PASS
- Verifies: Scenario execution works correctly

**AC#7**: Build succeeds
- Test: `dotnet build Era.Core`
- Expected: Build succeeded
- Verifies: No compile errors from changes

**AC#8**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" paths="Era.Core/Training/SpecialTraining.cs, Era.Core.Tests/Training/SpecialTrainingTests.cs"
- Note: Pipe `|` is regex alternation (matches TODO OR FIXME OR HACK)
- Expected: 0 matches
- Verifies: Clean implementation

**AC#9**: All 16 scenarios implemented
- Test: Grep pattern="ExecuteScomf\\d+" path="Era.Core/Training/SpecialTraining.cs" count
- Expected: 16 matches (ExecuteScomf1 through ExecuteScomf16)
- Verifies: All 16 SCOMF scenarios have handlers

**AC#10**: Invalid scenario rejected (Negative test)
- Test: `dotnet test --filter InvalidScenario`
- Test cases: ExecuteScenario(0), ExecuteScenario(17), ExecuteScenario(-1) all return Result.Fail
- Expected: All invalid scenario tests pass
- Verifies: Proper error handling for out-of-range scenarios

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Inject IVariableStore into SpecialTraining constructor | [x] |
| 2 | 3 | Implement IsScenarioAvailable with range validation | [x] |
| 3 | 1,4 | Implement ExecuteScenario switch expression with SCOMF1-4 cases | [x] |
| 4 | 1 | Implement SCOMF5-8 scenario cases | [x] |
| 5 | 1 | Implement SCOMF9-12 scenario cases | [x] |
| 6 | 1,5 | Implement SCOMF13-16 scenario cases | [x] |
| 7 | 3,6,10 | Write unit tests (range validation + scenario execution + negative tests) | [x] |
| 8 | 7 | Verify build succeeds with all changes | [x] |
| 9 | 8 | Ensure zero TODO/FIXME/HACK comments | [x] |
| 10 | 9 | Final sanity check: grep count of ExecuteScomf\\d+ == 16 | [x] |

<!-- AC:Task 1:1 Rule: 10 ACs = 10 Tasks (Task#7 covers AC#3, AC#6, AC#10 as unified test suite) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Design Principle

**SCOMF scenarios are implemented as ideal C# domain logic, NOT ERB ports.**

- ERB files are **reference only** for understanding scenario intent
- Implementation uses **available IVariableStore operations** for state management
- No ERB legacy constraints (TEQUIP, CSTR, external ERB functions are **not required**)
- Design for **testability and maintainability** over ERB parity
- Zero technical debt: complete implementations, no TODO/FIXME/HACK

### SCOMF Scenario Reference

| ID | Name (JP) | Description | Base Command |
|:--:|-----------|-------------|--------------|
| 1 | シックスナイン | 69 position - oral mutual | Oral |
| 2 | 岩清水 | Caress variant | Caress |
| 3 | Gスポット刺激 | G-spot stimulation | Caress |
| 4 | 乱れ牡丹 | Intercourse variant | Intercourse |
| 5 | 手淫フェラ | Handjob + fellatio | Service |
| 6 | 挿入Ｇスポ責め | G-spot during intercourse | Intercourse |
| 7 | 挿入子宮口責め | Cervix stimulation | Intercourse |
| 8 | ６９パイズリ | 69 + paizuri | Service |
| 9 | ダブルフェラ | Double fellatio | Assistant/Lesbian |
| 10 | ダブル素股 | Double intercrural | Service/Lesbian |
| 11 | ダブルパイズリ | Double paizuri | Assistant/Lesbian |
| 12 | パイズリフェラ | Paizuri + fellatio | Service |
| 13 | 交互挿入 | Alternating insertion | Intercourse |
| 14 | 母乳飲み | Breast milk drinking | Caress |
| 15 | 授乳する | Breastfeeding (assistant) | Assistant/Lesbian |
| 16 | 授乳手コキ | Breastfeeding + handjob | Caress |

### SpecialTraining Implementation Pattern

```csharp
// Era.Core/Training/SpecialTraining.cs
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Training;

public class SpecialTraining : ISpecialTraining
{
    private readonly IVariableStore _variableStore;

    public SpecialTraining(IVariableStore variableStore)
    {
        _variableStore = variableStore ?? throw new ArgumentNullException(nameof(variableStore));
    }

    public Result<TrainingResult> ExecuteScenario(int scenarioId, CharacterId target)
    {
        if (!IsScenarioAvailable(scenarioId, target))
        {
            return Result<TrainingResult>.Fail($"Scenario {scenarioId} not available for character {target.Value}");
        }

        return scenarioId switch
        {
            1 => ExecuteScomf1(target),   // シックスナイン
            2 => ExecuteScomf2(target),   // 岩清水
            3 => ExecuteScomf3(target),   // Gスポット刺激
            // ... SCOMF4-16 ...
            16 => ExecuteScomf16(target), // 授乳手コキ
            _ => Result<TrainingResult>.Fail($"Unknown scenario: {scenarioId}")
        };
    }

    public bool IsScenarioAvailable(int scenarioId, CharacterId target)
    {
        // Range check - prerequisite checks deferred to future feature
        return scenarioId >= 1 && scenarioId <= 16;
    }

    private Result<TrainingResult> ExecuteScomf1(CharacterId target)
    {
        // SCOMF1: シックスナイン - Reference: Game/ERB/SCOMF1.ERB
        // Example IVariableStore usage (indices are integers, see CSV/VARIABLESIZE.CSV):
        var stain = _variableStore.GetStain(target, new StainIndex(0));  // STAIN:0 = 口
        var stamina = _variableStore.GetDownbase(target, DownbaseIndex.Stamina);
        _variableStore.SetDownbase(target, DownbaseIndex.Stamina, stamina - 20);

        return Result<TrainingResult>.Ok(new TrainingResult());
    }

    // ... ExecuteScomf2-16 (each method follows same pattern: read ERB, use IVariableStore) ...
}
```

### ERB Source Reference

Each SCOMF scenario should reference the corresponding ERB file:
- `Game/ERB/SCOMF1.ERB` - シックスナイン (~333 lines)
- `Game/ERB/SCOMF2.ERB` - 岩清水 (~61 lines)
- ... etc.

### Scenario Implementation Requirements

Each ExecuteScomfN method MUST:
1. **Follow naming convention** - Method name MUST be `ExecuteScomf{N}` (e.g., ExecuteScomf1, ExecuteScomf16)
2. **Reference ERB for intent** - Use `Game/ERB/SCOMF{N}.ERB` to understand scenario purpose (not for exact port)
3. **Use IVariableStore** - Access SOURCE, STAIN, EXP, DOWNBASE via injected interface
4. **Return Result<TrainingResult>** - Success with result data or Fail with error message
5. **Complete implementation** - No TODO/FIXME/HACK; design clean C# logic

Key IVariableStore operations used in SCOMF:
| Operation | ERB Equivalent | Example |
|-----------|----------------|---------|
| GetStain | `STAIN:TARGET:idx` | `_variableStore.GetStain(target, new StainIndex(idx))` |
| SetStain | `STAIN:TARGET:idx = val` | `_variableStore.SetStain(target, new StainIndex(idx), val)` |
| GetDownbase | `DOWNBASE:TARGET:idx` | `_variableStore.GetDownbase(target, DownbaseIndex.Stamina)` |
| SetDownbase | `DOWNBASE:TARGET:idx = val` | `_variableStore.SetDownbase(target, DownbaseIndex.Stamina, val)` |
| GetSource | `SOURCE:TARGET:idx` | `_variableStore.GetSource(target, new SourceIndex(idx))` |
| SetSource | `SOURCE:TARGET:idx = val` | `_variableStore.SetSource(target, new SourceIndex(idx), val)` |
| GetExp | `EXP:TARGET:idx` | `_variableStore.GetExp(target, new ExpIndex(idx))` |
| SetExp | `EXP:TARGET:idx = val` | `_variableStore.SetExp(target, new ExpIndex(idx), val)` |
| GetTCVar | `TCVAR:TARGET:idx` | `_variableStore.GetTCVar(target, new TCVarIndex(idx))` |
| SetTCVar | `TCVAR:TARGET:idx = val` | `_variableStore.SetTCVar(target, new TCVarIndex(idx), val)` |
| GetTalent | `TALENT:TARGET:idx` | `_variableStore.GetTalent(target, new TalentIndex(idx))` |

### DI Registration

SpecialTraining is already registered via F435 DI setup. Adding IVariableStore constructor parameter is sufficient - the DI container auto-resolves registered dependencies. No DI registration code change needed.
```csharp
// F435 existing registration remains:
services.AddSingleton<ISpecialTraining, SpecialTraining>();
// IVariableStore is already registered (via F468) - DI auto-resolves
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F469 | SCOMF Variable Infrastructure - provides GetStain/GetDownbase in IVariableStore |
| Predecessor | F435 | SCOMF stub implementation (Phase 9) - provides Scomf1-16Handlers + ISpecialTraining interface |
| Predecessor | F468 | Legacy Bridge + DI Integration - establishes VariableStoreAdapter pattern and IVariableStore DI registration |

---

## Links

- [feature-469.md](feature-469.md) - SCOMF Variable Infrastructure (direct predecessor)
- [feature-435.md](feature-435.md) - SCOMF stub implementation (Phase 9)
- [feature-463.md](feature-463.md) - Phase 13 Planning (parent feature)
- [feature-468.md](feature-468.md) - Legacy Bridge + DI Integration
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 13 Task 10

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-13**: Created from F469 scope split (FL review determined SCOMF1-16 implementation too large for single feature)
- **2026-01-13 FL iter1**: [resolved] AC#3 pattern → Changed to unit test based verification (behavior test, not pattern match)
- **2026-01-13**: **Volume waiver**: ~3000 lines exceeds engine limit (~300), justified because 16 SCOMF scenarios form atomic functional unit per F435 stub structure. Splitting would break scenario cohesion.
- **2026-01-13 FL iter4**: [resolved] Scope clarification → **Ideal C# design** (not ERB port). ERB is reference for intent only. Full implementation, no stub-plus.
- **2026-01-13 FL iter6**: [resolved] AC#8/Limitations conflict → Limitations removed. Zero debt maintained. Complete implementations required.
- **2026-01-13 FL iter6**: [resolved] ERB→C# guidance → Design Principle added: "ideal C# domain logic, NOT ERB ports". No TEQUIP/CSTR/external functions needed.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-13 | create | opus | Created from F469 残課題 | PROPOSED |
| 2026-01-13 14:24 | START | implementer | Task 1-10 | - |
| 2026-01-13 14:24 | END | implementer | Task 1-10 | SUCCESS |
| 2026-01-13 14:25 | DEVIATION | debugger | ScomfStubTests.cs build errors | FIXED |
| 2026-01-13 14:25 | - | ac-tester | All ACs verified | PASS |
