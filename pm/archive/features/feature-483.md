# Feature 483: MANSETTTING.ERB Migration

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

**Phase 14: Era.Core Engine - MANSETTTING Migration**

Migrate `MANSETTTING.ERB` male character configuration logic from ERB to C# implementation in Era.Core with legacy equivalence testing.

**Migration Source**: `Game/ERB/SYSTEM/MANSETTTING.ERB` (~100 lines)

**Output**:
- `Era.Core/Character/MaleSettings.cs` - Male character configuration service
- `Era.Core/Interfaces/IMaleSettings.cs` - Interface abstraction
- Unit tests in `Era.Core.Tests/Character/MaleSettingsTests.cs`
- Legacy equivalence tests in `Era.Core.Tests/Character/MaleSettingsEquivalenceTests.cs`

**Volume**: ~300 lines (implementation + tests)

---

## Background

### Philosophy (Mid-term Vision)

**Phase 14: Era.Core Engine** - Establish pure C# game engine with headless execution capability. MANSETTTING migration replaces ERB male character configuration with strongly-typed C# implementation, enabling proper DI and testability.

### Problem (Current Issue)

`MANSETTTING.ERB` contains male character configuration logic in ERB scripting language. This creates technical debt:
- No type safety for male-specific properties
- Hard to test configuration sequences
- Cannot leverage DI for dependencies
- Mixed with engine execution logic

### Goal (What to Achieve)

1. **Migrate MANSETTTING logic to C#** - MaleSettings service implementation
2. **Define IMaleSettings interface** - Abstraction for DI
3. **Preserve legacy behavior** - Equivalence tests verify identical results
4. **Test male character configuration** - Unit tests for all configuration scenarios
5. **Register in DI** - ServiceCollectionExtensions integration
6. **Eliminate technical debt** - Zero TODO/FIXME/HACK comments

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | MaleSettings.cs exists | file | Glob | exists | "Era.Core/Character/MaleSettings.cs" | [ ] |
| 2 | IMaleSettings.cs exists | file | Glob | exists | "Era.Core/Interfaces/IMaleSettings.cs" | [ ] |
| 3 | MaleSettings implements interface | code | Grep | contains | "class MaleSettings : IMaleSettings" | [ ] |
| 4 | DI registration | file | Grep | contains | "AddSingleton.*IMaleSettings.*MaleSettings" | [ ] |
| 5 | Configure male character test | test | Bash | succeeds | "dotnet test --filter ConfigureMaleCharacter" | [ ] |
| 6 | Gender-specific settings test | test | Bash | succeeds | "dotnet test --filter GenderSpecificSettings" | [ ] |
| 7 | Legacy equivalence test | test | Bash | succeeds | "dotnet test --filter MaleSettingsEquivalence" | [ ] |
| 8 | Female character rejected (Neg) | test | Bash | succeeds | "dotnet test --filter FemaleCharacterRejected" | [ ] |
| 9 | Invalid settings (Neg) | test | Bash | succeeds | "dotnet test --filter InvalidMaleSettings" | [ ] |
| 10 | Build succeeds | build | Bash | succeeds | "dotnet build Era.Core" | [ ] |
| 11 | Zero technical debt | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [ ] |

### AC Details

**AC#1**: MaleSettings.cs file existence
- Test: Glob pattern="Era.Core/Character/MaleSettings.cs"
- Expected: File exists
- Verifies: Primary implementation file created

**AC#2**: IMaleSettings.cs interface file existence
- Test: Glob pattern="Era.Core/Interfaces/IMaleSettings.cs"
- Expected: File exists
- Verifies: Interface abstraction defined

**AC#3**: MaleSettings implementation
- Test: Grep pattern="class MaleSettings : IMaleSettings" path="Era.Core/Character/MaleSettings.cs"
- Expected: Contains implementation declaration
- Verifies: Interface implemented

**AC#4**: DI registration in ServiceCollectionExtensions
- Test: Grep pattern="AddSingleton.*IMaleSettings.*MaleSettings" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: Contains DI registration
- Verifies: Service registered for injection

**AC#5**: Configure male character (behavior test)
- Test: `dotnet test --filter ConfigureMaleCharacter`
- Test cases: Configure male character with specific settings, Verify BASE/TALENT values set correctly
- Expected: All male configuration tests pass
- Verifies: Basic male character configuration works correctly

**AC#6**: Gender-specific settings handling (behavior test)
- Test: `dotnet test --filter GenderSpecificSettings`
- Test cases: Apply male-only settings, Verify gender-specific BASE values
- Expected: All gender-specific tests pass
- Verifies: Male-specific configuration logic works correctly

**AC#7**: Legacy ERB equivalence verification (equivalence test)
- Test: `dotnet test --filter MaleSettingsEquivalence`
- Test cases: Run same configuration in legacy ERB and new C#, Compare final variable states
- Expected: All equivalence tests pass (minimum 5 assertions per test case)
- Verifies: C# implementation produces identical results to legacy ERB
- **Note**: Uses headless mode to execute legacy ERB, compares BASE/TALENT values for male characters

**AC#8**: Female character rejection (Negative test)
- Test: `dotnet test --filter FemaleCharacterRejected`
- Test cases: Attempt to apply male settings to female character returns Result.Fail
- Expected: All female rejection tests pass
- Verifies: Proper gender validation

**AC#9**: Invalid settings error handling (Negative test)
- Test: `dotnet test --filter InvalidMaleSettings`
- Test cases: Invalid BASE indices, Out-of-range values return Result.Fail
- Expected: All invalid settings tests pass
- Verifies: Proper validation of male settings data

**AC#10**: Build verification
- Test: `dotnet build Era.Core`
- Expected: Build succeeded
- Verifies: No compile errors

**AC#11**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Character/MaleSettings.cs,Era.Core.Tests/Character/MaleSettingsTests.cs"
- Expected: 0 matches
- Verifies: Clean implementation without deferred work

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Create MaleSettings.cs and IMaleSettings.cs files | [ ] |
| 2 | 2 | Define IMaleSettings interface with ConfigureMale/ApplyGenderSettings methods | [ ] |
| 3 | 3 | Implement MaleSettings with IVariableStore dependency | [ ] |
| 4 | 4 | Register IMaleSettings in ServiceCollectionExtensions.cs | [ ] |
| 5 | 5,6 | Write unit tests (male configuration, gender-specific settings) | [ ] |
| 6 | 7 | Write legacy equivalence tests comparing ERB vs C# output | [ ] |
| 7 | 8,9 | Write negative tests (female character, invalid settings) | [ ] |
| 8 | 10 | Verify build succeeds with all changes | [ ] |
| 9 | 11 | Remove all TODO/FIXME/HACK comments from MaleSettings files | [ ] |

<!-- AC:Task 1:1 Rule: 11 ACs = 9 Tasks (Task#1 covers AC#1-2 as file creation, Task#5 covers AC#5-6, Task#6 covers AC#7, Task#7 covers AC#8-9) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Migration Source Reference

**Legacy Location**: `Game/ERB/SYSTEM/MANSETTTING.ERB` (~100 lines)

**Key Logic to Migrate**:

| ERB Section | Purpose | C# Target |
|-------------|---------|-----------|
| @MALE_SETUP | Main male setup entry point | MaleSettings.ConfigureMale |
| @SET_MALE_BASE | Male-specific BASE values | Private SetMaleBase method |
| @SET_MALE_TALENT | Male-specific TALENT values | Private SetMaleTalents method |
| @APPLY_GENDER_DIFF | Gender difference application | Private ApplyGenderDifferences method |

**Note**: Verify actual section names and logic when implementing - MANSETTTING.ERB may use different organization.

### Interface Definition

File: `Era.Core/Interfaces/IMaleSettings.cs`

```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces;

/// <summary>
/// Male character configuration service migrated from MANSETTTING.ERB.
/// </summary>
public interface IMaleSettings
{
    /// <summary>Configure male character with gender-specific settings</summary>
    /// <param name="characterId">Character ID to configure</param>
    /// <param name="settings">Male-specific settings to apply</param>
    /// <returns>Result indicating success or failure</returns>
    Result<Unit> ConfigureMale(CharacterId characterId, MaleCharacterSettings settings);

    /// <summary>Apply gender-specific differences to character</summary>
    /// <param name="characterId">Character ID to modify</param>
    /// <returns>Result indicating success or failure</returns>
    Result<Unit> ApplyGenderDifferences(CharacterId characterId);

    /// <summary>Validate character is male before applying settings</summary>
    /// <param name="characterId">Character ID to check</param>
    Result<bool> IsMaleCharacter(CharacterId characterId);
}

/// <summary>Male character settings data structure</summary>
public record MaleCharacterSettings(Dictionary<int, int> BaseValues, Dictionary<int, int> TalentValues);
```

### Error Message Format

**Error Messages**: Return `Result.Fail` for errors with format:
- Female character: `"キャラクターID {id} は男性キャラクターではありません"`
- Invalid BASE index: `"無効なBASE番号: {index}"`
- Character not found: `"キャラクターID {id} が見つかりません"`

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IMaleSettings, MaleSettings>();
```

### Implementation Requirements

| Requirement | Specification |
|-------------|---------------|
| Thread safety | NOT required - configuration happens during game start only |
| Dependency | Requires IVariableStore for BASE/TALENT writes |
| Gender check | Verify character gender via TALENT:性別 before applying male settings |
| Validation | Validate BASE/TALENT indices are within valid range |

### Equivalence Test Strategy

**Purpose**: Verify C# implementation produces identical variable states to legacy ERB for male characters.

**Approach**:
1. Execute `MANSETTTING.ERB` in headless mode with test male character data
2. Capture final BASE/TALENT values via PRINTV output
3. Execute `MaleSettings.ConfigureMale` with same input
4. Compare all variable values (minimum 5 assertions per character)
5. Verify exact match

**Test Cases**:
- Standard male character with typical settings
- Male character with edge case BASE values (0, max value)
- Male character with minimal TALENT set

**Minimum Assertions**: 5 per test case (BASE values, TALENT values, gender validation)

### Test Naming Convention

Test methods follow `Test{Operation}{Scenario}` format:
- `TestConfigureMaleCharacterBasic` - Single male configuration
- `TestGenderSpecificSettingsApplied` - Gender differences
- `TestMaleSettingsEquivalenceStandard` - Legacy equivalence
- `TestFemaleCharacterRejectedError` - Negative case

This ensures AC filter patterns match correctly.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F471 | Phase 14 Planning must complete first |
| Dependency | F469 | SCOMF Variable Infrastructure provides IVariableStore |
| Sibling | F482 | CHARA_SET.ERB provides base character initialization |

---

## Links

- [feature-471.md](feature-471.md) - Phase 14 Planning (parent feature)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 14 Task 10
- [feature-template.md](reference/feature-template.md) - Feature template

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
