# Feature 412: TEQUIP/CDOWN Variable Accessor Addition

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

Add TEQUIP and CDOWN variable accessor methods to IVariableStore interfaces. This unblocks F406 (Equipment/OrgasmProcessor Completion) which requires these methods for processor implementation.

---

## Background

### Philosophy (Mid-term Vision)

**Phase 7: Technical Debt Consolidation**: Complete variable accessor coverage to enable processor implementations.

F404 established ISP segregation for IVariableStore. This feature extends the pattern to cover TEQUIP (equipment flags) and CDOWN (pleasure reduction) variables required by Equipment/Orgasm processors.

### Problem (Current Issue)

F406 (Equipment/OrgasmProcessor Completion) is blocked because IVariableStore lacks:

| Variable | Purpose | Required By |
|----------|---------|-------------|
| TEQUIP | Equipment flag checks | EquipmentProcessor (AC#1-4) |
| CDOWN | Post-orgasm pleasure reduction | OrgasmProcessor (AC#7) |
| EX | Orgasm counters | OrgasmProcessor (AC#10) |

Current state:
- IVariableStore has GetExp/SetExp (EXP array) but no GetEx/SetEx (EX array)
- No GetTEquip/SetTEquip methods exist
- No GetCDown/SetCDown methods exist
- TEquipIndex type does not exist (need to create or use raw int)
- CDOWN uses same indices as PALAM (per Game/CSV/Palam.csv)

### Goal (What to Achieve)

1. Add ITEquipVariables interface with GetTEquip/SetTEquip methods
2. Add GetCDown/SetCDown to appropriate interface (uses PalamIndex)
3. Add GetEx/SetEx to appropriate interface (uses ExIndex)
4. Implement methods in VariableStore
5. Unblock F406 for processor completion

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ITEquipVariables interface exists | code | Grep Era.Core/Interfaces/ | contains | interface ITEquipVariables | [x] |
| 2 | GetTEquip method defined | code | Grep Era.Core/Interfaces/ | contains | GetTEquip(CharacterId | [x] |
| 3 | SetTEquip method defined | code | Grep Era.Core/Interfaces/ | contains | SetTEquip(CharacterId | [x] |
| 4 | GetCDown method defined | code | Grep Era.Core/Interfaces/ | contains | GetCDown(CharacterId | [x] |
| 5 | SetCDown method defined | code | Grep Era.Core/Interfaces/ | contains | SetCDown(CharacterId | [x] |
| 6 | GetEx method defined | code | Grep Era.Core/Interfaces/ | contains | GetEx(CharacterId | [x] |
| 7 | SetEx method defined | code | Grep Era.Core/Interfaces/ | contains | SetEx(CharacterId | [x] |
| 8 | VariableStore implements GetTEquip | code | Grep Era.Core/Variables/VariableStore.cs | contains | public Result<int> GetTEquip | [x] |
| 9 | VariableStore implements GetCDown | code | Grep Era.Core/Variables/VariableStore.cs | contains | public Result<int> GetCDown | [x] |
| 10 | VariableStore implements GetEx | code | Grep Era.Core/Variables/VariableStore.cs | contains | public Result<int> GetEx | [x] |
| 11 | C# build succeeds | build | dotnet | succeeds | - | [x] |
| 12 | All Era.Core tests pass | test | dotnet | succeeds | Era.Core.Tests | [x] |

### AC Details

**AC#1-3**: ITEquipVariables interface for equipment flag access:
```csharp
public interface ITEquipVariables
{
    Result<int> GetTEquip(CharacterId character, int equipmentIndex);
    void SetTEquip(CharacterId character, int equipmentIndex, int value);
}
```

Note: Uses raw int for equipmentIndex as TEQUIP indices are defined in Game/CSV/Tequip.csv and there are many equipment types.

**AC#4-5**: CDOWN methods for pleasure reduction:
```csharp
// Add to ICharacterStateVariables
Result<int> GetCDown(CharacterId character, PalamIndex index);
void SetCDown(CharacterId character, PalamIndex index, int value);
```

Note: CDOWN uses same indices as PALAM (0=PleasureC, 1=PleasureV, 2=PleasureA, 3=PleasureB).

**AC#6-7**: EX methods for orgasm counters:
```csharp
// Add to ICharacterStateVariables
Result<int> GetEx(CharacterId character, ExIndex index);
void SetEx(CharacterId character, ExIndex index, int value);
```

Note: ExIndex already exists in Era.Core/Types/ExIndex.cs.

**AC#8-10**: VariableStore implementation (follows existing pattern in VariableStore.cs):
```csharp
public Result<int> GetTEquip(CharacterId character, int equipmentIndex)
{
    if (!_characterVariables.TryGetValue(character, out var charVars))
    {
        return Result<int>.Fail($"Invalid character: {character.Value}");
    }
    return Result<int>.Ok(charVars.GetTEquip(equipmentIndex));
}

public Result<int> GetCDown(CharacterId character, PalamIndex index)
{
    if (!_characterVariables.TryGetValue(character, out var charVars))
    {
        return Result<int>.Fail($"Invalid character: {character.Value}");
    }
    return Result<int>.Ok(charVars.GetCDown(index));
}

public Result<int> GetEx(CharacterId character, ExIndex index)
{
    if (!_characterVariables.TryGetValue(character, out var charVars))
    {
        return Result<int>.Fail($"Invalid character: {character.Value}");
    }
    return Result<int>.Ok(charVars.GetEx(index));
}
```

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-3 | Create ITEquipVariables interface with GetTEquip/SetTEquip | [x] |
| 2 | 4-5 | Add GetCDown/SetCDown to ICharacterStateVariables | [x] |
| 3 | 6-7 | Add GetEx/SetEx to ICharacterStateVariables | [x] |
| 4 | 8 | Implement GetTEquip/SetTEquip in VariableStore | [x] |
| 5 | 9 | Implement GetCDown/SetCDown in VariableStore | [x] |
| 6 | 10 | Implement GetEx/SetEx in VariableStore | [x] |
| 7 | 11 | Verify C# build succeeds | [x] |
| 8 | 12 | Verify Era.Core tests pass | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->
<!-- Note: T1-3 interface, T4-6 implementation, T7-8 verification -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Implementation Steps

| Step | Component | Action |
|:----:|-----------|--------|
| 1 | Era.Core/Interfaces/ | Create ITEquipVariables.cs with GetTEquip/SetTEquip |
| 2 | ICharacterStateVariables.cs | Add GetCDown/SetCDown and GetEx/SetEx methods |
| 3 | VariableStore.cs | Implement all new methods using TryGetValue/charVars pattern |
| 4 | CharacterVariables.cs | Add _tequip, _cdown, _ex arrays and implement Get/Set methods |
| 5 | Verification | Build and test |
| 6 | SSOT Update | Update engine-dev SKILL.md with ITEquipVariables, GetCDown/SetCDown, GetEx/SetEx |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-08 FL**: [resolved] CDOWN index type - F406 updated to use PalamIndex (same as F412)

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 01:53 | START | implementer | Task 1-6 | - |
| 2026-01-09 01:53 | END | implementer | Task 1-6 | SUCCESS |

## Links
- [F406: Equipment/OrgasmProcessor Completion](feature-406.md) - Unblocks F406 with required accessor methods
- [F404: IVariableStore ISP Segregation](feature-404.md) - Related: interface pattern
- [F398: Phase 7 Planning](feature-398.md) - Parent planning feature
