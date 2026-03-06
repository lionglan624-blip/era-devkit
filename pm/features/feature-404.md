# Feature 404: IVariableStore ISP Segregation

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

Refactor IVariableStore (currently 34 methods) into ISP-compliant specialized interfaces. Split monolithic interface into 4 domain-specific interfaces: IVariableStore (core 14 methods), ITrainingVariables (6 methods), ICharacterStateVariables (8 methods), IJuelVariables (6 methods). This segregation follows Interface Segregation Principle, allowing consumers to depend only on methods they actually use.

---

## Background

### Philosophy (Mid-term Vision)

**Phase 7: Technical Debt Consolidation**: Phase 5-6で蓄積した技術負債の解消 + Phase 8以降の基盤確立。

Phase 6 (Training) で IVariableStore が34メソッドに肥大化し、ISP違反が発生。本Featureは Interface Segregation Principle (ISP) 適用により技術負債ゼロ原則に貢献する。

### Problem (Current Issue)

Current IVariableStore interface violates ISP with 34 methods (17 Get/Set pairs) mixed together:

| Category | Methods | Current Issue |
|----------|---------|---------------|
| Core variables | FLAG, TFLAG, CFLAG, ABL, TALENT, PALAM, EXP (14 methods) | Mixed with specialized methods |
| Training-specific | BASE, TCVAR, CUP (6 methods) | Training processors forced to see all 34 methods |
| Character state | SOURCE, MARK, NOWEX, MAXBASE (8 methods) | State trackers forced to see all 34 methods |
| Juel system | JUEL, GOTJUEL, PALAMLV (6 methods) | AFTERTRA.ERB processors forced to see all 34 methods |

**Consequences**:
- Processors depend on methods they never use
- Interface changes affect unrelated consumers
- Harder to understand which variables a processor actually needs
- Violates ISP: "No client should be forced to depend on methods it does not use"

### Goal (What to Achieve)

Split IVariableStore into 4 ISP-compliant interfaces:

1. **IVariableStore** (core, 14 methods): FLAG, TFLAG, CFLAG, ABL, TALENT, PALAM, EXP
2. **ITrainingVariables** (training, 6 methods): BASE, TCVAR, CUP
3. **ICharacterStateVariables** (state tracking, 8 methods): SOURCE, MARK, NOWEX, MAXBASE
4. **IJuelVariables** (juel system, 6 methods): JUEL, GOTJUEL, PALAMLV

Each processor depends only on interfaces it needs, improving clarity and reducing coupling.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IVariableStore has 14 core methods | build | - | succeeds | - | [x] |
| 2 | ITrainingVariables interface exists | build | - | succeeds | - | [x] |
| 3 | ITrainingVariables has 6 methods | build | - | succeeds | - | [x] |
| 4 | ICharacterStateVariables interface exists | build | - | succeeds | - | [x] |
| 5 | ICharacterStateVariables has 8 methods | build | - | succeeds | - | [x] |
| 6 | IJuelVariables interface exists | build | - | succeeds | - | [x] |
| 7 | IJuelVariables has 6 methods | build | - | succeeds | - | [x] |
| 8 | VariableStore implements all 4 interfaces | build | - | succeeds | - | [x] |
| 9 | BasicChecksProcessor uses IVariableStore + ITrainingVariables | build | - | succeeds | - | [x] |
| 10 | JuelProcessor uses IVariableStore + IJuelVariables | build | - | succeeds | - | [x] |
| 11 | Era.Core builds with new interfaces | build | - | succeeds | - | [x] |
| 12 | Era.Core.Tests pass with new interfaces | test | - | succeeds | - | [x] |
| 13 | uEmuera.Headless builds with new interfaces | build | - | succeeds | - | [x] |

### AC Details

**AC1-7**: Interface structure validation
- **Test**: `dotnet build Era.Core`
- **Expected**: Build succeeds, 4 interfaces with correct method counts

**AC8**: Implementation verification
- **Test**: `dotnet build Era.Core`
- **Expected**: VariableStore implements IVariableStore, ITrainingVariables, ICharacterStateVariables, IJuelVariables

**AC9-10**: Consumer refactoring verification
- **Test**: `dotnet build Era.Core`
- **Expected**: BasicChecksProcessor and JuelProcessor use segregated interfaces

**AC11**: Era.Core build verification
- **Test**: `dotnet build Era.Core`
- **Expected**: No build errors after interface segregation

**AC12**: Unit test verification
- **Test**: `dotnet test Era.Core.Tests`
- **Expected**: All tests pass with new interface structure

**AC13**: Engine build verification
- **Test**: `dotnet build engine/uEmuera.Headless.csproj`
- **Expected**: No build errors in headless engine

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 2,3 | Create Era.Core/Interfaces/ITrainingVariables.cs (BASE, TCVAR, CUP) | [x] |
| 2 | 4,5 | Create Era.Core/Interfaces/ICharacterStateVariables.cs (SOURCE, MARK, NOWEX, MAXBASE) | [x] |
| 3 | 6,7 | Create Era.Core/Interfaces/IJuelVariables.cs (JUEL, GOTJUEL, PALAMLV) | [x] |
| 4 | 1 | Refactor IVariableStore to contain only core 14 methods | [x] |
| 5 | 8 | Update VariableStore to implement all 4 interfaces | [x] |
| 6 | 9 | Refactor BasicChecksProcessor to use IVariableStore + ITrainingVariables | [x] |
| 7 | 10 | Refactor JuelProcessor to use IVariableStore + IJuelVariables | [x] |
| 8 | 11 | Verify Era.Core build | [x] |
| 9 | 12 | Verify Era.Core.Tests pass | [x] |
| 10 | 13 | Verify uEmuera.Headless build | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Details

### Interface Structure

**IVariableStore** (core 14 methods):
```csharp
public interface IVariableStore
{
    // 1D arrays
    int GetFlag(FlagIndex index);
    void SetFlag(FlagIndex index, int value);
    int GetTFlag(FlagIndex index);
    void SetTFlag(FlagIndex index, int value);

    // 2D character arrays (core variables)
    Result<int> GetCharacterFlag(CharacterId character, CharacterFlagIndex flag);
    void SetCharacterFlag(CharacterId character, CharacterFlagIndex flag, int value);
    Result<int> GetAbility(CharacterId character, AbilityIndex ability);
    void SetAbility(CharacterId character, AbilityIndex ability, int value);
    Result<int> GetTalent(CharacterId character, TalentIndex talent);
    void SetTalent(CharacterId character, TalentIndex talent, int value);
    Result<int> GetPalam(CharacterId character, PalamIndex index);
    void SetPalam(CharacterId character, PalamIndex index, int value);
    Result<int> GetExp(CharacterId character, ExpIndex index);
    void SetExp(CharacterId character, ExpIndex index, int value);
}
```

**ITrainingVariables** (training-specific 6 methods):
```csharp
public interface ITrainingVariables
{
    Result<int> GetBase(CharacterId character, BaseIndex index);
    void SetBase(CharacterId character, BaseIndex index, int value);
    Result<int> GetTCVar(CharacterId character, TCVarIndex index);
    void SetTCVar(CharacterId character, TCVarIndex index, int value);
    Result<int> GetCup(CharacterId character, CupIndex index);
    void SetCup(CharacterId character, CupIndex index, int value);
}
```

**ICharacterStateVariables** (state tracking 8 methods):
```csharp
public interface ICharacterStateVariables
{
    Result<int> GetSource(CharacterId character, SourceIndex index);
    void SetSource(CharacterId character, SourceIndex index, int value);
    Result<int> GetMark(CharacterId character, MarkIndex index);
    void SetMark(CharacterId character, MarkIndex index, int value);
    Result<int> GetNowEx(CharacterId character, NowExIndex index);
    void SetNowEx(CharacterId character, NowExIndex index, int value);
    Result<int> GetMaxBase(CharacterId character, MaxBaseIndex index);
    void SetMaxBase(CharacterId character, MaxBaseIndex index, int value);
}
```

**IJuelVariables** (juel system 6 methods):
```csharp
public interface IJuelVariables
{
    // Character-scoped juel arrays
    Result<int> GetJuel(CharacterId character, int index);
    void SetJuel(CharacterId character, int index, int value);
    Result<int> GetGotJuel(CharacterId character, int index);
    void SetGotJuel(CharacterId character, int index, int value);

    // Global palam level array (1D, indices 0-15)
    // Note: PALAMLV is non-character-scoped but grouped with juel for processing cohesion
    Result<int> GetPalamLv(int index);
    void SetPalamLv(int index, int value);
}
```

### Consumer Updates

**BasicChecksProcessor** (F393):
```csharp
// Before: IVariableStore (34 methods visible)
public BasicChecksProcessor(IVariableStore variables) { ... }

// After: IVariableStore (core) + ITrainingVariables (training)
// Note: Uses GetCharacterFlag, GetAbility (core) and GetBase (training only).
// TCVAR and CUP in ITrainingVariables are unused by this processor but
// included for interface cohesion - still better ISP than 34-method interface.
public BasicChecksProcessor(IVariableStore coreVariables, ITrainingVariables trainingVariables) { ... }
```

**JuelProcessor** (F400):
```csharp
// Before: IVariableStore (34 methods visible)
public JuelProcessor(IVariableStore variables) { ... }

// After: IVariableStore (core) + IJuelVariables (juel)
// Note: JuelProcessor uses GetPalam and GetExp from core interface
// in addition to JUEL, GOTJUEL, PALAMLV from IJuelVariables
public JuelProcessor(IVariableStore coreVariables, IJuelVariables juelVariables) { ... }
```

### Implementation Adapter

**VariableStore** implements all 4 interfaces:
```csharp
public class VariableStore :
    IVariableStore,
    ITrainingVariables,
    ICharacterStateVariables,
    IJuelVariables
{
    // All 26 methods implemented once
    // Each interface exposes only its subset
}
```

---

## Dependencies

**Requires**: None (independent refactoring)

**Rationale**: F404 is a pure interface restructuring that doesn't depend on StateChange migrations. The interface changes affect only Era.Core consumers, not ERA game logic.

---

## Review Notes

- **2026-01-08 FL iter5**: [resolved] AC#10/Task#7 - CharacterStateTracker does NOT use IVariableStore → **Removed** (user decision)
- **2026-01-08 FL iter5**: [resolved] Task#9 + Missing consumers → **Deferred to F411** (user decision: split feature)
- **2026-01-08 FL iter5**: [resolved] Implementation Details - CharacterStateTracker example removed

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-08 22:26 | END | ac-tester | AC verification | OK:13/13 |
| 2026-01-08 22:25 | END | regression-tester | Regression tests | OK:24/24 |
| 2026-01-08 22:20 | END | debugger | Fix FavorCalculatorTests ISP | SUCCESS |
| 2026-01-08 22:18 | END | debugger | Fix JuelProcessorTests ISP | SUCCESS |
| 2026-01-08 22:14 | END | debugger | BUILD_FAIL fix (all consumers) | SUCCESS |
| 2026-01-08 21:51 | END | implementer | Task 4 (IVariableStore refactor) | SUCCESS |
| 2026-01-08 21:49 | END | implementer | Tasks 1-3 (new interfaces) | SUCCESS |
| 2026-01-08 21:47 | START | initializer | Initialize Feature 404 | READY |

---

## Links

**Related**: [F393](feature-393.md) (BasicChecksProcessor), [F400](feature-400.md) (JuelProcessor)
**Absorbed**: [F411](feature-411.md) (CANCELLED - debugger が BUILD_FAIL 修正時に全 consumer を移行完了)
