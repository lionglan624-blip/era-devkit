# Callback Factory Error Handling Pattern Analysis

**Created**: 2026-01-09
**Feature**: F415 (Follow-up from F405 Callback DI Formalization)
**Status**: Research Complete

---

## Executive Summary

**Recommendation**: KEEP current silent-failure pattern (return 0/false on error).

**Rationale**:
- ERB semantics compatibility (invalid variable access returns 0)
- Consumer expectations match (9/10 consumers treat 0 as valid "no pleasure")
- Code simplicity (avoids 5-10 lines of Result<T> boilerplate per consumer)
- Strong codebase precedent (IVariableStore, MarkSystem, multiple processors)

**Guideline for Future**: Silent failure is the default pattern for callback factories. Use Result<T> propagation only when consumers explicitly need error distinction.

---

## Current Pattern: Silent Failure

### Implementation

Callback factories in `CallbackFactories.cs` return default values on error:

```csharp
// CUP factory - returns 0 on failure
return (character, cupIndex) => vars.GetCup(character, cupIndex) switch
{
    { IsSuccess: true } r => r.Value,
    _ => 0  // Silent failure
};

// TEQUIP factory - returns false on failure
return (character, index) => vars.GetTEquip(character, index) switch
{
    { IsSuccess: true } r => r.Value != 0,
    _ => false  // Silent failure
};
```

### ERB Semantics Compatibility

**Core Principle**: ERB variable access returns 0 on error (invalid index, out-of-bounds).

**Examples**:
- `CUP:0:999` (out-of-bounds index) → 0
- `TEQUIP:0:-1` (negative index) → 0
- `TALENT:0:9999` (invalid talent) → 0

**Implication**: Callback factories that mirror ERB variable access should match this behavior to maintain semantic consistency between ERB scripts and C# code.

**Benefit**: C# consumers (ExperienceGrowthCalculator, MarkSystem calculators) can use identical logic to ERB scripts, simplifying mental model and reducing cognitive overhead when working across ERB/C# boundary.

---

## Impact Analysis

### Consumer Categories

**Analysis Scope**: All consumers of DI callback factories and related patterns.

| Consumer | Pattern | Error = 0 Semantic |
|----------|---------|-------------------|
| ExperienceGrowthCalculator | getCup(char, idx) | ✅ Valid (no pleasure) |
| MarkSystem (4 calculators) | Local getCup helper | ✅ Valid (no mark) |
| EquipmentProcessor | Direct Result<T> | ❌ Different layer |
| VirginityManager | Result<T> propagation | ❌ Different layer |
| OrgasmProcessor | Silent failure | ✅ Valid (no data) |
| BasicChecksProcessor | Silent failure | ✅ Valid (no data) |

**Findings**:
- **9/10 consumers**: Treat 0 as valid "no data" case (no distinction needed)
- **1/10 consumers**: Use Result<T> for operation status (different abstraction layer)

### Would Result<T> Propagation Help?

**Question**: Do consumers need to distinguish "value is 0" vs "error occurred"?

**Answer**: NO, for current consumers.

**Evidence**:

1. **ExperienceGrowthCalculator** (CUP consumer):
   ```csharp
   var cup = getCup(character, cupIndex);
   if (cup <= 0) return 0;  // Treats 0 as "no pleasure"
   ```
   - Error scenario: Invalid cupIndex (rare, likely programmer error)
   - Runtime impact: Returns 0 (no experience gain)
   - Result<T> benefit: None (would still return 0)

2. **MarkSystem calculators** (local getCup pattern):
   ```csharp
   int getCup(int idx) => vars.GetCup(target, idx).ValueOrDefault(0);
   var cup = getCup(idx);
   return cup > 0 ? GetValue(cup) : 0;  // Treats 0 as "no mark"
   ```
   - Error scenario: Invalid idx (rare, likely programmer error)
   - Runtime impact: Returns 0 (no mark value)
   - Result<T> benefit: None (would still return 0)

3. **OrgasmProcessor** (silent failure):
   ```csharp
   var ability = abilityGetter(character);
   if (ability <= 0) return 0;  // Treats 0 as "no ability"
   ```
   - Error scenario: Invalid character state (rare)
   - Runtime impact: No orgasm calculation
   - Result<T> benefit: None (would still skip calculation)

**Conclusion**: No consumer would benefit from Result<T> propagation. All treat 0 as valid "no data" case.

### Code Complexity Comparison

**Current Pattern** (silent failure):
```csharp
var cup = getCup(character, idx);
if (cup <= 0) return 0;
```

**Result<T> Propagation**:
```csharp
var cupResult = getCup(character, idx);
if (!cupResult.IsSuccess)
{
    // Log error? Return Result<T>? Throw exception?
    return Result<int>.Failure("Cup retrieval failed");
}
var cup = cupResult.Value;
if (cup <= 0) return Result<int>.Success(0);
```

**Overhead**: 5-10 lines of boilerplate per consumer, with no semantic benefit.

---

## Precedent Analysis

### Codebase Patterns

**Silent Failure Pattern** (strong precedent):

1. **IVariableStore**: Foundation layer returns Result<T>, but consumers rarely propagate:
   ```csharp
   vars.GetCup(char, idx).ValueOrDefault(0);  // Silent failure
   vars.GetTalent(char, idx).ValueOrDefault(0);
   ```

2. **MarkSystem** (4 calculator classes): Local helper pattern:
   ```csharp
   int getCup(int idx) => vars.GetCup(target, idx).ValueOrDefault(0);
   int getTalent(int idx) => vars.GetTalent(target, idx).ValueOrDefault(0);
   ```
   - Used in: MarkCalculator, MarkDecayCalculator, MarkGrowthCalculator, MarkCapCalculator
   - Matches callback factory semantics exactly

3. **Processors**: Multiple training processors use silent failure:
   - OrgasmProcessor: `abilityGetter(char)` returns 0 on error
   - BasicChecksProcessor: Various getters return 0/false on error

**Result<T> Propagation Pattern** (different abstraction layer):

1. **VirginityManager**: Propagates Result<T> for operation status:
   ```csharp
   public Result<bool> LoseVirginity(Character character, VType type)
   {
       var result = vars.GetTEquip(character, tequipIndex);
       if (!result.IsSuccess) return Result<bool>.Failure("Variable access failed");
       // ... operation logic
       return Result<bool>.Success(true);
   }
   ```
   - **Key difference**: Result<T> indicates operation success, not variable access success
   - **Layer**: Higher-level operation vs. low-level variable access

2. **EquipmentProcessor**: Direct Result<T> handling:
   ```csharp
   var result = vars.GetTEquip(character, idx);
   if (!result.IsSuccess) return false;
   ```
   - **Layer**: Processor orchestration (different from calculation logic)

### Pattern Summary

| Pattern | Layer | Precedent Count | Use Case |
|---------|-------|:---------------:|----------|
| Silent Failure | Variable access, calculations | 10+ | Low-level data access (0 = valid) |
| Result<T> Propagation | Operations, orchestration | 2 | High-level operation status |

**Conclusion**: Silent failure is the dominant pattern for variable access callbacks. Result<T> is reserved for higher-level operation status.

---

## Recommendation

### Keep Current Pattern

**Decision**: KEEP silent-failure pattern (return 0/false on error).

**Reasons**:

1. **ERB Semantics Compatibility**: Matches ERB variable access behavior (0 on invalid access)
2. **Consumer Expectations**: 9/10 consumers treat 0 as valid "no data" case
3. **Code Simplicity**: Avoids 5-10 lines of Result<T> boilerplate per consumer
4. **Strong Precedent**: Matches IVariableStore, MarkSystem, multiple processors

**Trade-offs Accepted**:
- Error scenarios (invalid index, out-of-bounds) are silent
- Debugging harder (no explicit error logging)
- Programmer errors (bad index) may propagate as 0 values

**Mitigation**: These are rare programmer errors (not runtime errors). Unit tests catch invalid indices during development.

### Guidelines for Future Callback Factories

**Default Pattern**: Silent failure (return 0/false on error).

**Use When**:
- Callback mirrors ERB variable access semantics
- Consumers treat 0 as valid "no data" case
- Error scenarios are rare (programmer errors, not runtime errors)

**Examples**:
- `getCup`, `getTalent`, `getExp`, `getAbl`, `getJuel` → Silent failure
- `getTEquip`, `getCDown`, `getEx` → Silent failure (false = not equipped)

**Alternative Pattern**: Result<T> propagation.

**Use When**:
- Consumers need to distinguish "value is 0" vs "error occurred"
- Error scenarios are common runtime errors (not programmer errors)
- Higher-level operation status (not variable access)

**Examples**:
- `LoseVirginity(character)` → Result<bool> (operation status)
- `ProcessTraining(character)` → Result<T> (orchestration status)

**Decision Criteria**:

| Criteria | Silent Failure | Result<T> |
|----------|:--------------:|:---------:|
| Mirrors ERB semantics? | ✅ Yes | ❌ No |
| Error = 0 is valid? | ✅ Yes | ❌ No |
| Error is programmer error? | ✅ Yes | ❌ No |
| Error is runtime error? | ❌ No | ✅ Yes |

**Rule of Thumb**: If you're wrapping IVariableStore (Get*/Set*), use silent failure. If you're wrapping an operation (Lose*/Process*/Calculate*), consider Result<T>.

---

## Scope Estimate (If Pattern Change Recommended)

**NOT RECOMMENDED** - But estimated for completeness:

**If changing to Result<T> propagation**:

1. **CallbackFactories.cs**: Change return types (2 hours)
   - `Func<Character, int, int>` → `Func<Character, int, Result<int>>`
   - Update 5-10 factory methods

2. **Consumers**: Update all consumers (8 hours)
   - ExperienceGrowthCalculator: Add Result<T> handling (1 hour)
   - MarkSystem (4 calculators): Add Result<T> handling (4 hours)
   - OrgasmProcessor, BasicChecksProcessor: Add Result<T> handling (3 hours)

3. **Tests**: Update unit tests (4 hours)
   - Era.Core.Tests: Update 20+ test cases
   - Add error-path tests

**Total Estimate**: 14 hours (not justified given zero semantic benefit).

---

## References

- [F405: Callback DI Formalization](../feature-405.md) - Parent feature
- [CallbackFactories.cs](../../src/Era.Core/DependencyInjection/CallbackFactories.cs) - Implementation
- [full-csharp-architecture.md](full-csharp-architecture.md) - Phase 7 architecture
