# Feature 482: CHARA_SET.ERB Migration

## Status: [PROPOSED]

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

**Phase 14: Era.Core Engine - CHARA_SET Migration**

Migrate `CHARA_SET.ERB` character initialization logic from ERB to C# implementation in Era.Core with legacy equivalence testing.

**Migration Source**: `Game/ERB/SYSTEM/CHARA_SET.ERB` (164 lines)

**Output**:
- `Era.Core/Character/CharacterSetup.cs` - Character initialization service
- `Era.Core/Interfaces/ICharacterSetup.cs` - Interface abstraction
- Unit tests in `Era.Core.Tests/Character/CharacterSetupTests.cs`
- Legacy equivalence tests in `Era.Core.Tests/Character/CharacterSetupEquivalenceTests.cs`

**Volume**: ~300 lines (implementation + tests)

---

## Background

### Philosophy (Mid-term Vision)

**Phase 14: Era.Core Engine** - Establish pure C# game engine with headless execution capability. CHARA_SET migration replaces ERB character initialization with strongly-typed C# implementation, enabling proper DI and testability.

### Problem (Current Issue)

`CHARA_SET.ERB` contains character initialization logic in ERB scripting language. This creates technical debt:
- No type safety for character properties
- Hard to test initialization sequences
- Cannot leverage DI for dependencies
- Mixed with engine execution logic

### Goal (What to Achieve)

1. **Migrate CHARA_SET logic to C#** - CharacterSetup service implementation
2. **Define ICharacterSetup interface** - Abstraction for DI
3. **Preserve legacy behavior** - Equivalence tests verify identical results
4. **Test character initialization** - Unit tests for all character types
5. **Register in DI** - ServiceCollectionExtensions integration
6. **Eliminate technical debt** - Zero TODO/FIXME/HACK comments

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CharacterSetup.cs exists | file | Glob | exists | "Era.Core/Character/CharacterSetup.cs" | [ ] |
| 2 | ICharacterSetup.cs exists | file | Glob | exists | "Era.Core/Interfaces/ICharacterSetup.cs" | [ ] |
| 3 | CharacterSetup implements interface | code | Grep | contains | "class CharacterSetup : ICharacterSetup" | [ ] |
| 4 | DI registration | file | Grep | contains | "AddSingleton.*ICharacterSetup.*CharacterSetup" | [ ] |
| 5 | Initialize character test | test | Bash | succeeds | "dotnet test --filter InitializeCharacter" | [ ] |
| 6 | Multiple characters test | test | Bash | succeeds | "dotnet test --filter MultipleCharacters" | [ ] |
| 7 | Legacy equivalence test | test | Bash | succeeds | "dotnet test --filter CharacterSetupEquivalence" | [ ] |
| 8 | Duplicate ID (Neg) | test | Bash | succeeds | "dotnet test --filter DuplicateCharacterId" | [ ] |
| 9 | Invalid character data (Neg) | test | Bash | succeeds | "dotnet test --filter InvalidCharacterData" | [ ] |
| 10 | Build succeeds | build | Bash | succeeds | "dotnet build Era.Core" | [ ] |
| 11 | Zero technical debt | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [ ] |

### AC Details

**AC#1**: CharacterSetup.cs file existence
- Test: Glob pattern="Era.Core/Character/CharacterSetup.cs"
- Expected: File exists
- Verifies: Primary implementation file created

**AC#2**: ICharacterSetup.cs interface file existence
- Test: Glob pattern="Era.Core/Interfaces/ICharacterSetup.cs"
- Expected: File exists
- Verifies: Interface abstraction defined

**AC#3**: CharacterSetup implementation
- Test: Grep pattern="class CharacterSetup : ICharacterSetup" path="Era.Core/Character/CharacterSetup.cs"
- Expected: Contains implementation declaration
- Verifies: Interface implemented

**AC#4**: DI registration in ServiceCollectionExtensions
- Test: Grep pattern="AddSingleton.*ICharacterSetup.*CharacterSetup" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: Contains DI registration
- Verifies: Service registered for injection

**AC#5**: Initialize single character (behavior test)
- Test: `dotnet test --filter InitializeCharacter`
- Test cases: Initialize character with ID/name/TALENT values, Verify variables set correctly
- Expected: All initialization tests pass
- Verifies: Basic character setup works correctly

**AC#6**: Multiple characters handling (behavior test)
- Test: `dotnet test --filter MultipleCharacters`
- Test cases: Initialize 10+ characters, Verify each character has independent state
- Expected: All multi-character tests pass
- Verifies: Batch initialization works correctly

**AC#7**: Legacy ERB equivalence verification (equivalence test)
- Test: `dotnet test --filter CharacterSetupEquivalence`
- Test cases: Run same initialization in legacy ERB and new C#, Compare final variable states
- Expected: All equivalence tests pass (minimum 5 assertions per test case)
- Verifies: C# implementation produces identical results to legacy ERB
- **Note**: Uses headless mode to execute legacy ERB, compares TALENT/ABL/BASE values

**AC#8**: Duplicate character ID error handling (Negative test)
- Test: `dotnet test --filter DuplicateCharacterId`
- Test cases: Attempt to initialize same ID twice returns Result.Fail
- Expected: All duplicate ID tests pass
- Verifies: Proper error handling for duplicate IDs

**AC#9**: Invalid character data error handling (Negative test)
- Test: `dotnet test --filter InvalidCharacterData`
- Test cases: Missing required fields, Invalid TALENT values return Result.Fail
- Expected: All invalid data tests pass
- Verifies: Proper validation of character data

**AC#10**: Build verification
- Test: `dotnet build Era.Core`
- Expected: Build succeeded
- Verifies: No compile errors

**AC#11**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Character/CharacterSetup.cs,Era.Core.Tests/Character/CharacterSetupTests.cs"
- Expected: 0 matches
- Verifies: Clean implementation without deferred work

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Create CharacterSetup.cs and ICharacterSetup.cs files | [ ] |
| 2 | 2 | Define ICharacterSetup interface with InitializeCharacter/InitializeBatch methods | [ ] |
| 3 | 3 | Implement CharacterSetup with IVariableStore dependency | [ ] |
| 4 | 4 | Register ICharacterSetup in ServiceCollectionExtensions.cs | [ ] |
| 5 | 5,6 | Write unit tests (single character, batch initialization) | [ ] |
| 6 | 7 | Write legacy equivalence tests comparing ERB vs C# output | [ ] |
| 7 | 8,9 | Write negative tests (duplicate ID, invalid data) | [ ] |
| 8 | 10 | Verify build succeeds with all changes | [ ] |
| 9 | 11 | Remove all TODO/FIXME/HACK comments from CharacterSetup files | [ ] |

<!-- AC:Task 1:1 Rule: 11 ACs = 9 Tasks (Task#1 covers AC#1-2 as file creation, Task#5 covers AC#5-6, Task#6 covers AC#7, Task#7 covers AC#8-9) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Migration Source Reference

**Legacy Location**: `Game/ERB/SYSTEM/CHARA_SET.ERB` (164 lines)

**Key Logic to Migrate**:

| ERB Section | Line Range | Purpose | C# Target |
|-------------|------------|---------|-----------|
| @CHARA_SETUP | 1-30 | Main setup entry point | CharacterSetup.InitializeCharacter |
| @SET_TALENT | 35-70 | TALENT value initialization | Private SetTalents method |
| @SET_ABL | 75-110 | ABL value initialization | Private SetAbilities method |
| @SET_BASE | 115-150 | BASE value initialization | Private SetBaseStats method |
| @SET_EXP | 155-164 | EXP value initialization | Private SetExperience method |

**Note**: Line ranges are approximate - verify actual ranges when implementing.

### Interface Definition

File: `Era.Core/Interfaces/ICharacterSetup.cs`

```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces;

/// <summary>
/// Character initialization service migrated from CHARA_SET.ERB.
/// </summary>
public interface ICharacterSetup
{
    /// <summary>Initialize single character with specified ID and properties</summary>
    /// <param name="characterId">Character ID to initialize</param>
    /// <param name="name">Character name</param>
    /// <param name="talents">TALENT values to set</param>
    /// <returns>Result indicating success or failure</returns>
    Result<Unit> InitializeCharacter(CharacterId characterId, string name, Dictionary<int, int> talents);

    /// <summary>Initialize multiple characters in batch</summary>
    /// <param name="characters">Collection of character initialization data</param>
    /// <returns>Result indicating success or failure with count of initialized characters</returns>
    Result<int> InitializeBatch(IEnumerable<CharacterInitData> characters);

    /// <summary>Check if character ID is already initialized</summary>
    /// <param name="characterId">Character ID to check</param>
    bool IsInitialized(CharacterId characterId);
}

/// <summary>Character initialization data structure</summary>
public record CharacterInitData(CharacterId Id, string Name, Dictionary<int, int> Talents);
```

### Error Message Format

**Error Messages**: Return `Result.Fail` for errors with format:
- Duplicate ID: `"キャラクターID {id} は既に初期化されています"`
- Invalid talent index: `"無効な才能番号: {index}"`
- Missing required field: `"{fieldName} は必須項目です"`

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<ICharacterSetup, CharacterSetup>();
```

### Implementation Requirements

| Requirement | Specification |
|-------------|---------------|
| Thread safety | NOT required - initialization happens during game start only |
| Dependency | Requires IVariableStore for TALENT/ABL/BASE/EXP writes |
| Validation | Validate TALENT indices are within valid range (0-299 per ERA spec) |
| Duplicate check | Track initialized character IDs - reject duplicates |

### Equivalence Test Strategy

**Purpose**: Verify C# implementation produces identical variable states to legacy ERB.

**Approach**:
1. Execute `CHARA_SET.ERB` in headless mode with test character data
2. Capture final TALENT/ABL/BASE/EXP values via PRINTV output
3. Execute `CharacterSetup.InitializeCharacter` with same input
4. Compare all variable values (minimum 5 assertions per character)
5. Verify exact match

**Test Cases**:
- Standard character (Reimu) with typical TALENT values
- Character with edge case TALENT values (0, max value)
- Character with sparse TALENT set (only 3 talents)

**Minimum Assertions**: 5 per test case (TALENT count >= 3, ABL values, BASE values)

### Test Naming Convention

Test methods follow `Test{Operation}{Scenario}` format:
- `TestInitializeCharacterBasic` - Single character setup
- `TestMultipleCharactersIndependent` - Batch initialization
- `TestCharacterSetupEquivalenceReimu` - Legacy equivalence for Reimu
- `TestDuplicateCharacterIdReturnsError` - Negative case

This ensures AC filter patterns match correctly.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F471 | Phase 14 Planning must complete first |
| Dependency | F469 | SCOMF Variable Infrastructure provides IVariableStore |

---

## Links

- [feature-471.md](feature-471.md) - Phase 14 Planning (parent feature)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 14 Task 9
- [feature-template.md](reference/feature-template.md) - Feature template
- [CHARA_SET.ERB](../../ERB/SYSTEM/CHARA_SET.ERB) - Legacy implementation source

---

## 残課題 (Deferred Tasks)

None.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-13 | create | spec-writer | Created from F471 Phase 14 Planning | PROPOSED |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
