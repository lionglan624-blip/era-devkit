# Feature 627: Dialogue-Specific IEvaluationContext Extension

## Status: [CANCELLED]

## Type: engine

## Created: 2026-01-26

---

## Background

### Philosophy (Mid-term Vision)

Phase 18: KojoEngine SRP - The dialogue selection system requires an evaluation context that provides direct access to character state (Talents, ABL, EXP) for condition evaluation. This extends the base IEvaluationContext with dialogue-specific methods.

### Problem (Current Issue)

Era.Core.Functions.IEvaluationContext provides generic evaluation context but lacks HasTalent and GetAbl/GetExp convenience methods needed for dialogue selection conditions. Current KojoEngine uses indirect access via ctx.Variables.GetTalent() which is verbose and lacks type safety.

### Goal (What to Achieve)

Define or extend IEvaluationContext with dialogue-specific evaluation methods:
- HasTalent(TalentIndex) method for direct TALENT presence checking
- GetAbl(AbilityIndex) method for ABL values
- GetExp(ExpIndex) method for EXP values

---

## Origin

Created from F545 (IDialogueSelector Interface Extraction) Task#10 per Handoff Option A.

The IDialogueSelector.Select method requires IEvaluationContext parameter, but the existing IEvaluationContext in Era.Core.Functions lacks the Talents/GetAbl methods documented in F545's Implementation Contract design rationale.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. **Why**: F547 (Concrete Specifications) TalentSpecification and AblSpecification cannot compile
2. **Why**: TalentSpecification calls `entity.Talents.Contains(_talent)` and AblSpecification calls `entity.GetAbl(_abl)`, but IEvaluationContext lacks these methods
3. **Why**: Era.Core.Functions.IEvaluationContext was designed for built-in function execution (F421), not dialogue condition evaluation
4. **Why**: Dialogue selection (Phase 18) requires character state access patterns different from function call context
5. **Why**: The base IEvaluationContext provides generic function arguments and IVariableStore, but dialogue specifications need direct typed access to TALENT/ABL/EXP values without character ID indirection

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| F547 TalentSpecification cannot call `entity.Talents` | IEvaluationContext interface lacks Talents property |
| F547 AblSpecification cannot call `entity.GetAbl()` | IEvaluationContext interface lacks GetAbl method |
| Verbose access via ctx.Variables.GetTalent() | IEvaluationContext designed for function execution, not dialogue conditions |

### Conclusion

The root cause is an **interface responsibility mismatch**: Era.Core.Functions.IEvaluationContext was designed for built-in function execution (F421) with IVariableStore access, but Phase 18 dialogue specifications require a different access pattern. The existing interface has:
- `IVariableStore Variables` - requires `CharacterId` parameter for all TALENT/ABL/EXP access
- `CharacterId? CurrentCharacter` - provides context character but no convenience methods

Dialogue specifications need:
- `IReadOnlySet<TalentType> Talents` - direct TALENT set access (scoped to context character)
- `int GetAbl(AblType)` - direct ABL value access (scoped to context character)
- `int GetExp(ExpIndex)` - direct EXP value access (scoped to context character)

The solution is to extend IEvaluationContext with these dialogue-specific members, providing a character-scoped facade over IVariableStore.

---

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F545 | [DONE] | Origin | Created F627 via Task#10 Handoff Option A; IDialogueSelector.Select uses IEvaluationContext |
| F546 | [DONE] | Foundation | Specification Pattern Infrastructure; ISpecification<IEvaluationContext> |
| F547 | [WIP] | Consumer | Concrete Specifications (TalentSpecification, AblSpecification) - Can benefit from this feature (optional enhancement) |
| F550 | [BLOCKED] | Consumer | ConditionEvaluator Implementation - uses IEvaluationContext for condition evaluation |
| F552 | [BLOCKED] | Consumer | PriorityDialogueSelector Implementation - passes IEvaluationContext to condition evaluation |
| F543 | [DONE] | Related | IConditionEvaluator Interface - uses IEvaluationContext parameter |
| F421 | [DONE] | Origin | Function Call Mechanism - original IEvaluationContext definition |

### Pattern Analysis

This is **not a recurring pattern** but rather an **intentional architectural extension**. The original IEvaluationContext (F421) was designed for built-in function execution where:
- Functions need to access arbitrary character variables via explicit CharacterId
- Function arguments are retrieved via GetArg<T>
- No assumptions about "current character" scope

Phase 18 dialogue selection introduces a different pattern:
- Conditions always evaluate against a specific target character
- Character ID is implicit (context-scoped)
- TALENT checks need set membership, not value retrieval

The solution is extension, not replacement - the base interface remains valid for function execution.

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Interface extension is straightforward; IVariableStore already has GetTalent/GetAbility/GetExp methods |
| Scope is realistic | YES | Adding 3 members (Talents, GetAbl, GetExp) to interface + implementation in context classes |
| No blocking constraints | YES | Interface extension is additive; existing implementations can add new members without breaking changes |

**Verdict**: FEASIBLE

The solution requires:
1. Add Talents property, GetAbl method, GetExp method to IEvaluationContext interface
2. Update existing IEvaluationContext implementations (e.g., ComEvaluationContext) to provide these members

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F625 | [DONE] | Post-Phase Review Phase 17 - Layer 1 gate |
| Successor | F547 | [WIP] | Concrete Specifications - BLOCKED until F627 provides Talents/GetAbl |
| Successor | F550 | [BLOCKED] | ConditionEvaluator Implementation - uses extended IEvaluationContext |
| Successor | F552 | [BLOCKED] | PriorityDialogueSelector Implementation - passes IEvaluationContext |
| Related | F545 | [DONE] | Origin feature (IDialogueSelector uses IEvaluationContext) |
| Related | F543 | [DONE] | IConditionEvaluator Interface (uses IEvaluationContext parameter) |
| Related | F546 | [DONE] | Specification Pattern Infrastructure (ISpecification<IEvaluationContext>) |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| Era.Core.Functions | Runtime | Low | Interface extension is additive |
| Era.Core.Types | Runtime | Low | TalentType, AbilityIndex, ExpIndex already exist |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| Era.Core/Dialogue/Specifications/TalentSpecification.cs | HIGH | Calls entity.Talents.Contains() |
| Era.Core/Dialogue/Specifications/AblSpecification.cs | HIGH | Calls entity.GetAbl() |
| Era.Core/Commands/Com/ComEvaluationContext.cs | MEDIUM | Existing IEvaluationContext implementation - needs update |
| Era.Core/KojoEngine.cs | LOW | May use extended context for dialogue evaluation |

---

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| Era.Core/Functions/IEvaluationContext.cs | Update | Add Talents property, GetAbl method, GetExp method |
| Era.Core/Commands/Com/ComEvaluationContext.cs | Update | Implement new interface members |
| Era.Core.Tests/ | Create | Unit tests for new interface members |

---

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| CurrentCharacter may be null | IEvaluationContext design | Methods must handle null case (return empty set / 0 / error) |
| IVariableStore requires CharacterId | Existing API | Implementation must extract CharacterId from CurrentCharacter |
| Result<int> return type in IVariableStore | Error handling pattern | GetAbl/GetExp must unwrap Result or return default on failure |
| TalentType vs TalentIndex mismatch | Type system | May need type conversion or new typed index |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Null CurrentCharacter causes NullReferenceException | Medium | Medium | Return empty Talents set / 0 for GetAbl/GetExp when CurrentCharacter is null |
| Result<int> unwrapping adds complexity | Low | Low | Use pattern matching to extract value with default fallback |
| Breaking change to existing IEvaluationContext implementations | Low | Medium | Document required member additions in Implementation Contract |
| TalentType/TalentIndex type confusion | Low | Low | Use consistent typing (TalentIndex for IVariableStore, expose TalentType set) |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Extend existing IEvaluationContext interface with HasTalent method | [ ] |
| 2 | 2 | Extend existing IEvaluationContext interface with GetAbl method | [ ] |
| 3 | 3 | Extend existing IEvaluationContext interface with GetExp method | [ ] |
| 4 | 4 | Implement HasTalent in ComEvaluationContext | [ ] |
| 5 | 5 | Implement GetAbl in ComEvaluationContext | [ ] |
| 6 | 6 | Implement GetExp in ComEvaluationContext | [ ] |
| 7 | 7 | Create unit test for HasTalent null character handling | [ ] |
| 8 | 8 | Create unit test for GetAbl null character handling | [ ] |
| 9 | 9 | Create unit test for GetExp null character handling | [ ] |
| 10 | 10 | Create unit test for HasTalent IVariableStore delegation | [ ] |
| 11 | 11 | Create unit test for GetAbl IVariableStore delegation | [ ] |
| 12 | 12 | Create unit test for GetExp IVariableStore delegation | [ ] |
| 13 | 13 | Verify build succeeds | [ ] |
| 14 | 14 | Verify zero technical debt (interface) | [ ] |
| 15 | 15 | Verify zero technical debt (implementation) | [ ] |

<!-- AC:Task 1:1 Rule: Each AC maps to exactly one Task for implementation and verification -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Tasks 1-3 | Interface extension (IEvaluationContext.cs) |
| 2 | implementer | sonnet | Tasks 4-6 | Implementation (ComEvaluationContext.cs) |
| 3 | implementer | sonnet | Tasks 7-12 | Unit tests (Era.Core.Tests/Functions/ComEvaluationContextTests.cs) |
| 4 | ac-tester | haiku | Task 13 | Build verification |
| 5 | ac-tester | haiku | Task 14 | Technical debt verification |

**Constraints** (from Technical Design):

1. **Null safety**: All methods must return safe defaults (false/0) when `CurrentCharacter` is null (for interface contract compliance; unreachable in ComEvaluationContext since IComContext.Target is non-nullable)
2. **Result unwrapping**: Use `Match()` pattern for extracting values from `Result<int>` with 0 fallback on error
3. **TALENT semantics**: HasTalent returns `true` when TALENT value is non-zero (TALENT uses 0/1 encoding)
4. **CurrentCharacter delegation**: All methods must use `CurrentCharacter.Value` when calling `IVariableStore` methods
5. **Type consistency**: Use TalentIndex, AbilityIndex, ExpIndex from Era.Core.Types (not string/int primitives)

**Pre-conditions**:

- Era.Core/Functions/IEvaluationContext.cs exists with base members (from F421)
- Era.Core/Commands/Com/ComEvaluationContext.cs exists as existing implementation
- Era.Core.Types defines TalentIndex, AbilityIndex, ExpIndex
- IVariableStore has GetTalent, GetAbility, GetExp methods

**Success Criteria**:

- All 14 ACs pass verification
- `dotnet build Era.Core` succeeds
- `dotnet test Era.Core.Tests --filter FullyQualifiedName~ComEvaluationContext` succeeds
- No TODO/FIXME/HACK comments in modified files
- F547 can compile TalentSpecification and AblSpecification using new interface members

**Interface Extension Pattern** (Era.Core/Functions/IEvaluationContext.cs):

Add three methods to existing interface:

```csharp
public interface IEvaluationContext
{
    // Existing members (F421)
    T GetArg<T>(int index);
    T[] GetArgs<T>();
    int ArgCount { get; }
    CharacterId? CurrentCharacter { get; }
    IVariableStore Variables { get; }

    // NEW: Dialogue-specific convenience methods (F627)
    /// <summary>
    /// Check if current character has specified talent.
    /// Returns false if CurrentCharacter is null or talent value is 0.
    /// </summary>
    bool HasTalent(TalentIndex talent);

    /// <summary>
    /// Get ability value for current character.
    /// Returns 0 if CurrentCharacter is null or ability lookup fails.
    /// </summary>
    int GetAbl(AbilityIndex ability);

    /// <summary>
    /// Get experience value for current character.
    /// Returns 0 if CurrentCharacter is null or exp lookup fails.
    /// </summary>
    int GetExp(ExpIndex index);
}
```

**Implementation Pattern** (Era.Core/Commands/Com/ComEvaluationContext.cs):

Add three method implementations to existing class:

```csharp
public bool HasTalent(TalentIndex talent)
    => CurrentCharacter == null
        ? false
        : Variables.GetTalent(CurrentCharacter.Value, talent).Match(
            v => v != 0,  // TALENT value is 0 or 1; non-zero means talent acquired
            _ => false    // On error, treat as talent not present
        );

public int GetAbl(AbilityIndex ability)
    => CurrentCharacter == null
        ? 0
        : Variables.GetAbility(CurrentCharacter.Value, ability).Match(
            v => v,       // Extract value from Success
            _ => 0        // On error, return 0 (safe default for ABL)
        );

public int GetExp(ExpIndex index)
    => CurrentCharacter == null
        ? 0
        : Variables.GetExp(CurrentCharacter.Value, index).Match(
            v => v,       // Extract value from Success
            _ => 0        // On error, return 0 (safe default for EXP)
        );
```

**Test Structure** (Era.Core.Tests/Functions/ComEvaluationContextTests.cs):

Create or extend test class with 6 test methods:

1. **Null Character Tests** (ACs 7-9):
   - Create test-only IEvaluationContext implementation that allows null CurrentCharacter OR use direct mock of IEvaluationContext interface (IComContext.Target is non-nullable, making ComEvaluationContext.CurrentCharacter never null)
   - Verify HasTalent returns false, GetAbl returns 0, GetExp returns 0

2. **IVariableStore Delegation Tests** (ACs 10-12):
   - Create ComEvaluationContext with mock IVariableStore
   - Configure mock to return Success(value)
   - Verify methods return correct values
   - Verify IVariableStore was called with correct parameters

**Test Naming Convention**: Test methods follow `MethodName_Scenario_ExpectedBehavior` format:
- `HasTalent_NullCharacter_ReturnsFalse`
- `HasTalent_ValidCharacter_DelegatesToVariableStore`
- `GetAbl_NullCharacter_ReturnsZero`
- `GetAbl_ValidCharacter_DelegatesToVariableStore`
- `GetExp_NullCharacter_ReturnsZero`
- `GetExp_ValidCharacter_DelegatesToVariableStore`

**Error Handling**:

- `Result<int>.Match()` pattern handles both Success and Failure cases
- Failure case returns safe default (false for HasTalent, 0 for GetAbl/GetExp)
- No exceptions thrown - silent fallback consistent with missing character semantics

---

## Links

- [feature-545.md](feature-545.md) - Origin feature (IDialogueSelector Interface Extraction)
- [feature-550.md](feature-550.md) - ConditionEvaluator Implementation (related consumer)
- [feature-552.md](feature-552.md) - PriorityDialogueSelector Implementation (related consumer)
- [feature-546.md](feature-546.md) - Specification Pattern Infrastructure (related foundation)
- [feature-547.md](feature-547.md) - Concrete Specifications (blocked successor)
- [feature-543.md](feature-543.md) - IConditionEvaluator Interface (related)

---

## Review Notes
- [resolved-applied] Phase1-Uncertain iter2: IComContext.Target is CharacterId (non-nullable type), so ComEvaluationContext.CurrentCharacter cannot be null when backed by IComContext. Null tests require direct IEvaluationContext mock or dedicated test implementation.
- [resolved-applied] Phase1-Uncertain iter4: Null check in ComEvaluationContext implementation is provided for IEvaluationContext interface contract compliance, ensuring other implementations can rely on null safety. The check is unreachable in ComEvaluationContext but required for interface semantics.
- [pending] Phase1-Pending iter5: Root Cause Analysis is factually incorrect - F547 TalentSpecification/AblSpecification compile successfully using entity.Variables.GetTalent() pattern. F547 is [WIP] with all ACs checked, not blocked. Original problem was solved differently.
- [pending] Phase1-Pending iter5: F547 relationship claims "Can benefit from this feature" but F547 already works without F627 using IVariableStore pattern. F547 chose alternative approach.
- [pending] Phase1-Pending iter5: Dependencies lists F547 as "Successor BLOCKED" but F547 is [WIP] not blocked. F547 Dependencies shows F627 as future enhancement only.
- [pending] Phase1-Pending iter5: Problem statement claims IVariableStore pattern "verbose and lacks type safety" but F547 uses this pattern successfully.
- [pending] Phase1-Pending iter5: Philosophy Derivation treats convenience methods as requirement-satisfying but F547 already provides direct access via IVariableStore.
- [pending] Phase2-Maintainability iter5: Feature justification depends on F547 being blocked, but F547 is [WIP] not blocked. May need re-evaluation of necessity or re-scoping as optional convenience enhancement.
- [pending] Phase1-Pending iter6: F547 is [DONE] not [WIP]. Root Cause Analysis factually incorrect - F547 compiles successfully using IVariableStore pattern. Feature needs re-scoping as optional enhancement or closure as unnecessary.
- [pending] Phase2-Maintainability iter6: Multiple [pending] review notes remain unaddressed. Philosophy Coverage and Dependencies require fundamental updates to acknowledge F547 [DONE] status and alternative approach. Feature purpose unclear with F547 complete.

## Cancellation Reason

Feature cancelled due to invalid premise. Original problem statement claimed F547 "cannot compile" but F547 is [DONE] and works successfully using IVariableStore pattern. The convenience methods proposed by F627 are not needed since F547 demonstrates the existing IVariableStore approach is functional and sufficient.

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "direct access to character state (Talents, ABL, EXP)" | IEvaluationContext must expose Talents property, GetAbl method, GetExp method | AC#1, AC#2, AC#3 |
| "Talents property for direct TALENT access" | HasTalent(TalentIndex) method for talent presence check | AC#1 |
| "GetAbl(index) method for ABL values" | GetAbl(AbilityIndex) method returning int | AC#2 |
| "GetExp(index) method for EXP values" | GetExp(ExpIndex) method returning int | AC#3 |
| "character-scoped facade over IVariableStore" | Implementation delegates to IVariableStore with CurrentCharacter | AC#4 |
| "CurrentCharacter may be null" | Methods must handle null case gracefully | AC#5, AC#6 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IEvaluationContext.HasTalent method signature | code | Grep(Era.Core/Functions/IEvaluationContext.cs) | contains | `bool HasTalent(TalentIndex talent)` | [ ] |
| 2 | IEvaluationContext.GetAbl method signature | code | Grep(Era.Core/Functions/IEvaluationContext.cs) | contains | `int GetAbl(AbilityIndex ability)` | [ ] |
| 3 | IEvaluationContext.GetExp method signature | code | Grep(Era.Core/Functions/IEvaluationContext.cs) | contains | `int GetExp(ExpIndex index)` | [ ] |
| 4 | ComEvaluationContext implements HasTalent | code | Grep(Era.Core/Commands/Com/ComEvaluationContext.cs) | contains | `public bool HasTalent` | [ ] |
| 5 | ComEvaluationContext implements GetAbl | code | Grep(Era.Core/Commands/Com/ComEvaluationContext.cs) | contains | `public int GetAbl` | [ ] |
| 6 | ComEvaluationContext implements GetExp | code | Grep(Era.Core/Commands/Com/ComEvaluationContext.cs) | contains | `public int GetExp` | [ ] |
| 7 | HasTalent returns false when CurrentCharacter null | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~HasTalent_NullCharacter | succeeds | - | [ ] |
| 8 | GetAbl returns 0 when CurrentCharacter null | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~GetAbl_NullCharacter | succeeds | - | [ ] |
| 9 | GetExp returns 0 when CurrentCharacter null | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~GetExp_NullCharacter | succeeds | - | [ ] |
| 10 | HasTalent delegates to IVariableStore.GetTalent | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~HasTalent_ValidCharacter | succeeds | - | [ ] |
| 11 | GetAbl delegates to IVariableStore.GetAbility | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~GetAbl_ValidCharacter | succeeds | - | [ ] |
| 12 | GetExp delegates to IVariableStore.GetExp | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~GetExp_ValidCharacter | succeeds | - | [ ] |
| 13 | Build succeeds | build | dotnet build Era.Core | succeeds | - | [ ] |
| 14 | Zero technical debt (interface) | code | Grep(Era.Core/Functions/IEvaluationContext.cs) | not_contains | `TODO` | [ ] |
| 15 | Zero technical debt (implementation) | code | Grep(Era.Core/Commands/Com/ComEvaluationContext.cs) | not_contains | `TODO` | [ ] |

### AC Details

**AC#1: IEvaluationContext.HasTalent method signature**
- Verifies new HasTalent(TalentIndex) method added to IEvaluationContext interface
- Returns bool instead of exposing Talents set directly (encapsulation, matches TalentSpecification.IsSatisfiedBy pattern)
- TalentIndex is existing type from Era.Core.Types

**AC#2: IEvaluationContext.GetAbl method signature**
- Verifies new GetAbl(AbilityIndex) method added to IEvaluationContext interface
- Returns int directly (not Result<int>) for simplified condition evaluation
- AbilityIndex is existing type from Era.Core.Types

**AC#3: IEvaluationContext.GetExp method signature**
- Verifies new GetExp(ExpIndex) method added to IEvaluationContext interface
- Returns int directly for simplified condition evaluation
- ExpIndex is existing type from Era.Core.Types

**AC#4: ComEvaluationContext implements HasTalent**
- Verifies ComEvaluationContext provides HasTalent implementation
- Implementation pattern: CurrentCharacter null check, then IVariableStore.GetTalent delegation
- Returns false if CurrentCharacter is null (safe fallback)

**AC#5: ComEvaluationContext implements GetAbl**
- Verifies ComEvaluationContext provides GetAbl implementation
- Implementation pattern: CurrentCharacter null check, then IVariableStore.GetAbility delegation
- Returns 0 if CurrentCharacter is null (safe fallback)

**AC#6: ComEvaluationContext implements GetExp**
- Verifies ComEvaluationContext provides GetExp implementation
- Implementation pattern: CurrentCharacter null check, then IVariableStore.GetExp delegation
- Returns 0 if CurrentCharacter is null (safe fallback)

**AC#7: HasTalent returns false when CurrentCharacter null**
- Unit test: Test interface contract compliance - Create mock IEvaluationContext with null CurrentCharacter (ComEvaluationContext cannot have null CurrentCharacter, so this tests interface design)
- Call HasTalent(TalentIndex.Affection)
- Assert returns false (no exception thrown)

**AC#8: GetAbl returns 0 when CurrentCharacter null**
- Unit test: Test interface contract compliance - Create mock IEvaluationContext with null CurrentCharacter (ComEvaluationContext cannot have null CurrentCharacter, so this tests interface design)
- Call GetAbl(new AbilityIndex(0))
- Assert returns 0 (no exception thrown)

**AC#9: GetExp returns 0 when CurrentCharacter null**
- Unit test: Test interface contract compliance - Create mock IEvaluationContext with null CurrentCharacter (ComEvaluationContext cannot have null CurrentCharacter, so this tests interface design)
- Call GetExp(ExpIndex.OrgasmExperience)
- Assert returns 0 (no exception thrown)

**AC#10: HasTalent delegates to IVariableStore.GetTalent**
- Unit test: Create ComEvaluationContext with mock IVariableStore
- Configure mock to return Success(1) for specific TalentIndex
- Call HasTalent, assert returns true
- Configure mock to return Success(0), assert returns false
- Verify IVariableStore.GetTalent was called with correct CharacterId and TalentIndex

**AC#11: GetAbl delegates to IVariableStore.GetAbility**
- Unit test: Create ComEvaluationContext with mock IVariableStore
- Configure mock to return Success(42) for specific AbilityIndex
- Call GetAbl, assert returns 42
- Verify IVariableStore.GetAbility was called with correct CharacterId and AbilityIndex

**AC#12: GetExp delegates to IVariableStore.GetExp**
- Unit test: Create ComEvaluationContext with mock IVariableStore
- Configure mock to return Success(100) for specific ExpIndex
- Call GetExp, assert returns 100
- Verify IVariableStore.GetExp was called with correct CharacterId and ExpIndex

**AC#13: Build succeeds**
- Verify Era.Core project builds without compilation errors after interface extension
- All existing code using IEvaluationContext should continue to compile

**AC#14: Zero technical debt**
- Test: Grep pattern="TODO|FIXME|HACK" paths=[Era.Core/Functions/IEvaluationContext.cs, Era.Core/Commands/Com/ComEvaluationContext.cs]
- Expected: 0 matches

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Extend `IEvaluationContext` interface with three character-scoped convenience methods that delegate to `IVariableStore`:
1. `bool HasTalent(TalentIndex talent)` - Returns true if TALENT value is non-zero
2. `int GetAbl(AbilityIndex ability)` - Returns ABL value or 0
3. `int GetExp(ExpIndex index)` - Returns EXP value or 0

Implementation pattern in `ComEvaluationContext`:
- Null safety: Return false/0 when `CurrentCharacter` is null
- Result unwrapping: Use `Match()` to extract value with 0 fallback on error
- Delegation: Call `Variables.GetTalent/GetAbility/GetExp` with `CurrentCharacter` as CharacterId

This design provides type-safe character-scoped access while preserving the existing `IVariableStore` API for cases requiring explicit character ID control.

**Rationale**: Dialogue specifications always evaluate against a single target character (from `IComContext.Target`). These convenience methods eliminate the boilerplate of null-checking `CurrentCharacter` and unwrapping `Result<int>` in every specification class. The base `IEvaluationContext` interface remains valid for function execution (F421), while dialogue selection (Phase 18) benefits from the extension.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add `bool HasTalent(TalentIndex talent);` method signature to `IEvaluationContext` interface |
| 2 | Add `int GetAbl(AbilityIndex ability);` method signature to `IEvaluationContext` interface |
| 3 | Add `int GetExp(ExpIndex index);` method signature to `IEvaluationContext` interface |
| 4 | Implement `HasTalent` in `ComEvaluationContext`: return `CurrentCharacter == null ? false : Variables.GetTalent(CurrentCharacter.Value, talent).Match(v => v != 0, _ => false)` |
| 5 | Implement `GetAbl` in `ComEvaluationContext`: return `CurrentCharacter == null ? 0 : Variables.GetAbility(CurrentCharacter.Value, ability).Match(v => v, _ => 0)` |
| 6 | Implement `GetExp` in `ComEvaluationContext`: return `CurrentCharacter == null ? 0 : Variables.GetExp(CurrentCharacter.Value, index).Match(v => v, _ => 0)` |
| 7 | Create test `HasTalent_NullCharacter`: Create context with null `CurrentCharacter`, call `HasTalent`, assert false |
| 8 | Create test `GetAbl_NullCharacter`: Create context with null `CurrentCharacter`, call `GetAbl`, assert 0 |
| 9 | Create test `GetExp_NullCharacter`: Create context with null `CurrentCharacter`, call `GetExp`, assert 0 |
| 10 | Create test `HasTalent_ValidCharacter`: Mock `IVariableStore.GetTalent` to return Success(1), call `HasTalent`, assert true; mock Success(0), assert false |
| 11 | Create test `GetAbl_ValidCharacter`: Mock `IVariableStore.GetAbility` to return Success(42), call `GetAbl`, assert 42 |
| 12 | Create test `GetExp_ValidCharacter`: Mock `IVariableStore.GetExp` to return Success(100), call `GetExp`, assert 100 |
| 13 | Verify `dotnet build Era.Core` succeeds after changes |
| 14 | Verify no TODO/FIXME/HACK comments in modified files |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Interface member type | A. `IReadOnlySet<TalentType> Talents` property<br>B. `bool HasTalent(TalentIndex)` method<br>C. `Result<bool> HasTalent(TalentIndex)` | B | Matches TalentSpecification.IsSatisfiedBy pattern; encapsulates null handling; simpler than Result monad for boolean check |
| Return type for GetAbl/GetExp | A. `Result<int>` (propagate errors)<br>B. `int` (default to 0 on error)<br>C. `int?` (nullable) | B | Dialogue conditions need simple int comparison (e.g., `GetAbl(Attack) > 5`); 0 is safe fallback for missing character; avoids Result unwrapping in every specification |
| Null handling strategy | A. Throw exception<br>B. Return default (false/0)<br>C. Return Result.Failure | B | Silent fallback prevents NullReferenceException; dialogue conditions can safely evaluate even with missing context; consistent with TALENT=0 semantics (talent not acquired) |
| Result unwrapping pattern | A. Pattern matching (`Match`)<br>B. `IsSuccess` check + cast<br>C. Extension method `GetValueOrDefault` | A | `Match()` is functional, concise, and handles both Success/Failure cases in single expression; avoids conditional logic |
| Implementation location | A. Extension methods on IEvaluationContext<br>B. Default interface members (C# 8)<br>C. Instance methods in implementations | C | Default interface members require `IVariableStore Variables` access, which is instance state; extension methods cannot access instance state; instance methods provide cleanest implementation |

### Interfaces / Data Structures

**Extended IEvaluationContext Interface** (Era.Core/Functions/IEvaluationContext.cs):

```csharp
public interface IEvaluationContext
{
    // Existing members (F421)
    T GetArg<T>(int index);
    T[] GetArgs<T>();
    int ArgCount { get; }
    CharacterId? CurrentCharacter { get; }
    IVariableStore Variables { get; }

    // NEW: Dialogue-specific convenience methods (F627)
    /// <summary>
    /// Check if current character has specified talent.
    /// Returns false if CurrentCharacter is null or talent value is 0.
    /// </summary>
    bool HasTalent(TalentIndex talent);

    /// <summary>
    /// Get ability value for current character.
    /// Returns 0 if CurrentCharacter is null or ability lookup fails.
    /// </summary>
    int GetAbl(AbilityIndex ability);

    /// <summary>
    /// Get experience value for current character.
    /// Returns 0 if CurrentCharacter is null or exp lookup fails.
    /// </summary>
    int GetExp(ExpIndex index);
}
```

**Implementation Pattern** (ComEvaluationContext.cs):

```csharp
public bool HasTalent(TalentIndex talent)
    => CurrentCharacter == null
        ? false
        : Variables.GetTalent(CurrentCharacter.Value, talent).Match(
            v => v != 0,  // TALENT value is 0 or 1; non-zero means talent acquired
            _ => false    // On error, treat as talent not present
        );

public int GetAbl(AbilityIndex ability)
    => CurrentCharacter == null
        ? 0
        : Variables.GetAbility(CurrentCharacter.Value, ability).Match(
            v => v,       // Extract value from Success
            _ => 0        // On error, return 0 (safe default for ABL)
        );

public int GetExp(ExpIndex index)
    => CurrentCharacter == null
        ? 0
        : Variables.GetExp(CurrentCharacter.Value, index).Match(
            v => v,       // Extract value from Success
            _ => 0        // On error, return 0 (safe default for EXP)
        );
```

**Test Structure** (Era.Core.Tests/Functions/ComEvaluationContextTests.cs):

```csharp
[Theory]
[InlineData(TalentIndex.Affection)]
public void HasTalent_NullCharacter_ReturnsFalse(TalentIndex talent)
{
    var mockContext = new Mock<IEvaluationContext>();
    mockContext.Setup(c => c.CurrentCharacter).Returns((CharacterId?)null);
    mockContext.Setup(c => c.HasTalent(talent)).Returns(false);

    Assert.False(mockContext.Object.HasTalent(talent));
}

[Theory]
[InlineData(TalentIndex.Affection, 1, true)]  // TALENT=1 => has talent
[InlineData(TalentIndex.Affection, 0, false)] // TALENT=0 => no talent
public void HasTalent_ValidCharacter_DelegatesToVariableStore(
    TalentIndex talent, int talentValue, bool expected)
{
    var mockVariables = new Mock<IVariableStore>();
    var mockComContext = new Mock<IComContext>();
    var characterId = new CharacterId(1);
    mockComContext.Setup(c => c.Target).Returns(characterId);
    mockVariables.Setup(v => v.GetTalent(characterId, talent))
        .Returns(Result<int>.Ok(talentValue));

    var context = new ComEvaluationContext(mockComContext.Object, mockVariables.Object);

    Assert.Equal(expected, context.HasTalent(talent));
    mockVariables.Verify(v => v.GetTalent(characterId, talent), Times.Once);
}
```

---
