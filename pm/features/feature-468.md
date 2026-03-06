# Feature 468: Legacy Bridge + DI Integration

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

## Created: 2026-01-12

---

## Summary

**Phase 13: DDD Foundation - Legacy Integration and DI Configuration**

Implement adapter layer bridging DDD domain model with existing IVariableStore (ERB legacy flat arrays) and complete DI container configuration for all Phase 13 components. This enables gradual migration from legacy storage while maintaining backward compatibility.

**Output**:
- `Era.Core/Infrastructure/VariableStoreAdapter.cs` - Adapter between domain model and legacy IVariableStore
- `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` - Updated DI registration for DDD components
- Unit tests in `Era.Core.Tests/Infrastructure/`

**Volume**: ~250 lines (adapter + DI updates + tests)

---

## Background

### Philosophy (Mid-term Vision)

**Phase 13: DDD Foundation** - Establish Domain-Driven Design foundation through Aggregate Root, Repository, and UnitOfWork patterns. Legacy bridge enables coexistence of new DDD domain model with existing ERB flat array storage, allowing incremental migration without breaking existing game logic.

### Problem (Current Issue)

With DDD patterns established (F465-F467), we need:
- Adapter layer translating between Character aggregate and IVariableStore flat arrays
- DI container configuration for all DDD components (AggregateRoot, Repository, UnitOfWork)
- Backward compatibility with existing ERB code using IVariableStore
- Migration path from legacy to DDD without big-bang rewrite

### Goal (What to Achieve)

1. **Create VariableStoreAdapter** bridging Character aggregate and IVariableStore
2. **Update ServiceCollectionExtensions** with DDD component registrations
3. **Verify DI registration** through Grep checks and unit tests
4. **Test adapter bidirectional translation** (aggregate ↔ IVariableStore)
5. **Establish migration strategy** in documentation
6. **Eliminate technical debt** (no TODO/FIXME/HACK comments)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | VariableStoreAdapter.cs exists | file | Glob | exists | "Era.Core/Infrastructure/VariableStoreAdapter.cs" | [x] |
| 2 | Adapter has ToAggregate method | code | Grep | contains | "Character ToAggregate.*IVariableStore" | [x] |
| 3 | Adapter has FromAggregate method | code | Grep | contains | "void FromAggregate.*Character.*IVariableStore" | [x] |
| 4 | ServiceCollectionExtensions updated | file | Glob | exists | "Era.Core/DependencyInjection/ServiceCollectionExtensions.cs" | [x] |
| 5 | IRepository DI registration | code | Grep | contains | "AddScoped.*IRepository.*Character.*CharacterRepository" | [x] |
| 6 | UnitOfWork DI registration | code | Grep | contains | "AddScoped.*IUnitOfWork.*UnitOfWork" | [x] |
| 7 | TransactionBehavior DI registration | code | Grep | contains | "AddScoped.*IPipelineBehavior.*TransactionBehavior" | [x] |
| 8 | VariableStoreAdapter unit tests exist | file | Glob | exists | "Era.Core.Tests/Infrastructure/VariableStoreAdapterTests.cs" | [x] |
| 9 | ToAggregate test passes | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestToAggregate" | [x] |
| 10 | FromAggregate test passes | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestFromAggregate" | [x] |
| 11 | All adapter tests pass | test | Bash | succeeds | "dotnet test Era.Core.Tests --filter Category=Adapter" | [x] |
| 12 | Zero technical debt | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 13 | Migration strategy documented | file | Grep | contains | "Migration Strategy" | [x] |
| 14 | VariableStoreAdapter DI registration | code | Grep | contains | "AddScoped.*VariableStoreAdapter" | [x] |

### AC Details

**AC#1**: VariableStoreAdapter file existence
- Test: Glob pattern="Era.Core/Infrastructure/VariableStoreAdapter.cs"
- Expected: File exists

**AC#2**: Adapter ToAggregate method
- Test: Grep pattern="Character ToAggregate.*IVariableStore" path="Era.Core/Infrastructure/VariableStoreAdapter.cs"
- Expected: Contains method converting IVariableStore to Character aggregate
- Verifies: Legacy → Domain translation

**AC#3**: Adapter FromAggregate method
- Test: Grep pattern="void FromAggregate.*Character.*IVariableStore" path="Era.Core/Infrastructure/VariableStoreAdapter.cs"
- Expected: Contains method converting Character aggregate to IVariableStore
- Verifies: Domain → Legacy translation

**AC#4**: ServiceCollectionExtensions file existence
- Test: Glob pattern="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: File exists
- Verifies: DI configuration location

**AC#5**: IRepository<Character, CharacterId> registration
- Test: Grep pattern="AddScoped.*IRepository.*Character.*CharacterRepository" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: Contains scoped registration for IRepository<Character, CharacterId> mapped to CharacterRepository
- Verifies: Repository interface available in DI container for UnitOfWork dependency

**AC#6**: UnitOfWork registration
- Test: Grep pattern="AddScoped.*IUnitOfWork.*UnitOfWork" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: Contains scoped registration mapping IUnitOfWork to UnitOfWork
- Verifies: Transaction boundary management available

**AC#7**: TransactionBehavior registration
- Test: Grep pattern="AddScoped.*IPipelineBehavior.*TransactionBehavior" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: Contains scoped registration for MediatR pipeline (must be scoped to match IUnitOfWork lifetime)
- Verifies: Automatic transaction management in MediatR

**AC#8**: Unit test file existence
- Test: Glob pattern="Era.Core.Tests/Infrastructure/VariableStoreAdapterTests.cs"
- Expected: File exists

**AC#9**: ToAggregate translation test
- Test: `dotnet test --filter FullyQualifiedName~TestToAggregate`
- Expected: PASS
- Verifies: IVariableStore → Character conversion correct

**AC#10**: FromAggregate translation test
- Test: `dotnet test --filter FullyQualifiedName~TestFromAggregate`
- Expected: PASS
- Verifies: Character → IVariableStore conversion correct

**AC#11**: All adapter tests pass
- Test: `dotnet test Era.Core.Tests --filter Category=Adapter`
- Expected: PASS with all Adapter category tests
- Verifies: Bidirectional translation correctness

**AC#12**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Infrastructure/VariableStoreAdapter.cs" and "Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: 0 matches in new F468 code (no TODO/FIXME/HACK comments)
- Verifies: Clean implementation across all files modified by F468

**AC#13**: Migration strategy documentation
- Test: Grep pattern="Migration Strategy" path="Era.Core/Infrastructure/VariableStoreAdapter.cs"
- Expected: Contains comment block describing migration approach
- Verifies: Future maintainers understand gradual migration plan

**AC#14**: VariableStoreAdapter DI registration
- Test: Grep pattern="AddScoped.*VariableStoreAdapter" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: Contains scoped registration for VariableStoreAdapter
- Verifies: Adapter available in DI container

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | Create VariableStoreAdapter with ToAggregate and FromAggregate bidirectional translation | [x] |
| 2 | 4,5,14 | Update ServiceCollectionExtensions with IRepository<Character, CharacterId> and VariableStoreAdapter DI registrations | [x] |
| 2b | 6,7 | Verify existing UnitOfWork and TransactionBehavior DI registrations from F467 | [x] |
| 3 | 8,9,10 | Write unit tests for ToAggregate and FromAggregate translation correctness | [x] |
| 4 | 11 | Verify all adapter tests pass with Category=Adapter | [x] |
| 5 | 12 | Remove all TODO/FIXME/HACK comments from adapter and DI configuration files | [x] |
| 6 | 13 | Document migration strategy in VariableStoreAdapter comments | [x] |

<!-- AC:Task 1:1 Rule: 14 ACs = 7 Tasks (batch waivers: Task 1 for AC 1-3 related adapter methods, Task 2 for AC 4,5,14 DI registrations, Task 2b for AC 6,7 verification, Task 3 for AC 8-10 test creation) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Adapter Pattern Requirements

**Adapter Pattern**:
- Translates between incompatible interfaces (Character aggregate ↔ IVariableStore)
- Bidirectional translation (ToAggregate and FromAggregate)
- Maintains data integrity across translation boundary
- No business logic (pure translation only)
- Documented migration strategy for gradual legacy replacement

### VariableStoreAdapter Implementation

```csharp
// Era.Core/Infrastructure/VariableStoreAdapter.cs
using Era.Core.Domain.Aggregates;
using Era.Core.Domain.ValueObjects;
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Infrastructure;

/// <summary>
/// Adapter bridging Character aggregate (DDD) with IVariableStore (legacy ERB flat arrays).
///
/// Migration Strategy:
/// 1. Phase 13: Adapter enables coexistence (new code uses Character, legacy uses IVariableStore)
/// 2. Phase 14+: Gradually migrate ERB commands to use Character aggregate
/// 3. Long-term: Deprecate IVariableStore when all ERB code migrated
///
/// Translation Mappings (using IVariableStore strongly-typed API):
/// - CharacterId.Value ↔ character index in IVariableStore
/// - CharacterStats.Health ↔ GetMaxBase(character, MaxBaseIndex.Mood)
/// - CharacterStats.Stamina ↔ GetMaxBase(character, MaxBaseIndex.Reason)
/// - CharacterStats.Frustration ↔ GetJuel(character, 100) (100 = 否定 index)
/// - CharacterStats.Loyalty ↔ GetCharacterFlag(character, CharacterFlagIndex.Favor)
///
/// IMPORTANT: Phase 13 Placeholder Mapping
/// The CharacterStats properties (Health, Stamina) do not have direct semantic equivalents in
/// IVariableStore. Current mapping uses Mood/Reason as placeholders for demonstration purposes.
/// This semantic mismatch is tracked as 残課題 for resolution in a future feature where either:
/// 1. CharacterStats properties are renamed to match IVariableStore semantics (Mood, Reason)
/// 2. Proper Health/Stamina indices are added to IVariableStore
///
/// Note: CharacterName is not stored in IVariableStore (stored separately in character table).
/// This adapter focuses on numeric stat translation only.
/// </summary>
public class VariableStoreAdapter
{
    /// <summary>
    /// Translates IVariableStore data to Character aggregate.
    /// Uses graceful degradation with default values for missing/invalid data.
    /// </summary>
    public Character ToAggregate(int characterIndex, IVariableStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var id = new CharacterId(characterIndex);
        // CharacterName not available in IVariableStore - use placeholder
        // SAFE: CharacterName.Create never fails for non-empty constant prefix + integer
        var name = CharacterName.Create("Character_" + characterIndex).Value;

        // Extract values with graceful fallback to defaults using pattern matching
        // Result<T> is discriminated union - use 'is Success s' pattern to extract value
        var healthResult = store.GetMaxBase(id, MaxBaseIndex.Mood);
        var staminaResult = store.GetMaxBase(id, MaxBaseIndex.Reason);
        var frustrationResult = store.GetJuel(id, 100); // 100 = 否定
        var loyaltyResult = store.GetCharacterFlag(id, CharacterFlagIndex.Favor);

        var stats = new CharacterStats(
            Health: healthResult is Result<int>.Success hs ? hs.Value : 0,
            Stamina: staminaResult is Result<int>.Success ss ? ss.Value : 0,
            Frustration: frustrationResult is Result<int>.Success fs ? fs.Value : 0,
            Loyalty: loyaltyResult is Result<int>.Success ls ? ls.Value : 0
        );

        return Character.Create(id, name, stats);
    }

    /// <summary>
    /// Writes Character aggregate data back to IVariableStore.
    /// Invalid character IDs are silently ignored per IVariableStore contract.
    /// </summary>
    public void FromAggregate(Character character, IVariableStore store)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(store);

        var id = character.Id;
        // CharacterName not stored in IVariableStore - skip
        store.SetMaxBase(id, MaxBaseIndex.Mood, character.Stats.Health);
        store.SetMaxBase(id, MaxBaseIndex.Reason, character.Stats.Stamina);
        store.SetJuel(id, 100, character.Stats.Frustration); // 100 = 否定
        store.SetCharacterFlag(id, CharacterFlagIndex.Favor, character.Stats.Loyalty);
    }
}
```

### DI Registration Updates

Current state (already registered in F467):
- `services.AddScoped<IUnitOfWork, UnitOfWork>()` (line 66)
- `services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>))` (line 146)

**Required additions for F468**:

```csharp
// Add using directive at top of ServiceCollectionExtensions.cs
// (Era.Core.Infrastructure already exists; only Aggregates namespace needed)
using Era.Core.Domain.Aggregates;

// Add to ServiceCollectionExtensions.cs in DDD Infrastructure section (after line 66)

// DDD Infrastructure (Phase 13) - F468 additions
services.AddScoped<IRepository<Character, CharacterId>, CharacterRepository>();
services.AddScoped<VariableStoreAdapter>();
```

**Final state after F468**:
```csharp
// DDD Infrastructure (Phase 13)
services.AddScoped<IUnitOfWork, UnitOfWork>();  // F467
services.AddScoped<IRepository<Character, CharacterId>, CharacterRepository>();  // F468
services.AddScoped<VariableStoreAdapter>();  // F468
```

Note: TransactionBehavior is already correctly registered as Scoped (line 146).

### Test Requirements

Tests must verify:
1. ToAggregate creates Character with correct values from IVariableStore
2. FromAggregate writes Character values back to IVariableStore correctly
3. Round-trip translation preserves all data (ToAggregate → FromAggregate → verify equality)
4. Invalid IVariableStore data handled gracefully (empty names, negative values)
5. DI container resolves all registered types (CharacterRepository, IUnitOfWork, VariableStoreAdapter)

**Test Category**: All tests must be marked with `[Trait("Category", "Adapter")]`

**Test Naming Convention**: Test methods follow `Test{Operation}` format (e.g., `TestToAggregate`, `TestFromAggregate`, `TestRoundTrip`). This ensures AC filter patterns match correctly.

### Error Handling

| Scenario | Use |
|----------|-----|
| Invalid character index | Return Character with default values (graceful degradation) |
| Null IVariableStore argument | `ArgumentNullException` |
| Missing variable in store | Use default value (0 for int, "Unknown" for name) |

### Migration Source Reference

**Legacy Location**: Direct IVariableStore usage throughout ERB code and engine

| Component | Current State | Migration Target |
|-----------|---------------|------------------|
| ERB Commands | Use IVariableStore directly | Gradually migrate to Character aggregate |
| Training System | MAXBASE direct access | Use Character.UpdateStats |
| NTR System | CFLAG/JUEL direct access | Use Character aggregate properties |

**Gradual Migration Approach**:
1. New features use Character aggregate exclusively
2. Existing features continue using IVariableStore (no breaking changes)
3. Adapter synchronizes state between both representations
4. Future features refactor one ERB command at a time to use Character

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F467 | UnitOfWork Pattern - DI registration depends on UnitOfWork completion |
| Predecessor | F466 | Repository Pattern - DI registration depends on CharacterRepository |
| Predecessor | F465 | Aggregate Root + Character Aggregate - Adapter translates to/from Character |
| Predecessor | F463 | Phase 13 Planning - defines DDD Foundation scope |
| Successor | F469 | SCOMF Full Implementation - May use Character aggregate through adapter |

---

## Links

- [feature-463.md](feature-463.md) - Phase 13 Planning (parent feature)
- [feature-467.md](feature-467.md) - UnitOfWork Pattern (dependency)
- [feature-466.md](feature-466.md) - Repository Pattern (dependency)
- [feature-465.md](feature-465.md) - Aggregate Root + Character Aggregate (dependency)
- [feature-469.md](feature-469.md) - SCOMF Full Implementation (successor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 13 Tasks 8-9

---

## 残課題

| Item | Description | Resolution Path |
|------|-------------|-----------------|
| CharacterStats semantic mismatch | Health/Stamina properties mapped to Mood/Reason indices which are semantically different | Future feature to either rename CharacterStats properties or add proper Health/Stamina indices to IVariableStore |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->

- **2026-01-13 FL iter2**: [resolved] Phase2-Validate - CharacterName.Create .Value access: Code uses .Value without IsSuccess check, but guaranteed to succeed for "Character_N". Code quality concern rather than defect.
- **2026-01-13 FL iter2**: [resolved] Phase2-Validate - AC#5-#7 DI patterns: Grep patterns may be fragile with different formatting. Patterns use .* which should handle most cases.
- **2026-01-13 FL iter2**: [resolved] Phase2-Validate - AC#12 pattern syntax: Table shows escaped pipes but AC Details shows correct unescaped pipes. AC Details is authoritative.
- **2026-01-13 FL iter3**: [pending] Phase2-Validate - AC#13 precision: AC Details could clarify "Contains XML comment block with Migration Strategy section". Minor observation.
- **2026-01-13 FL iter3**: [resolved] Phase2-Validate - using directive verification: Verified - using Era.Core.Domain.Aggregates is required since Character is in Domain.Aggregates namespace, not covered by existing Era.Core.Domain using.
- **2026-01-13 FL iter4**: [pending] Phase2-Validate - ToAggregate signature: Design opinion on int vs CharacterId parameter. Current int is defensible for legacy bridge purposes.
- **2026-01-13 FL iter4**: [pending] Phase2-Validate - AC#5 namespace concern: Pattern may fail if qualified namespace used. Speculative - should work with using directive.
- **2026-01-13 FL iter5**: [pending] Phase3-Maintainability - F469 dependency is speculative: "May use" phrasing is appropriately hedged, but could change to "Available for use if needed". Style preference.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-12 | create | feature-builder | Created from F463 Phase 13 Planning | PROPOSED |
| 2026-01-13 09:23 | START | implementer | Phase 3 TDD - Task 1 | - |
| 2026-01-13 09:23 | END | implementer | Task 1-6 Complete | SUCCESS |
| 2026-01-13 09:24 | START | ac-tester | Phase 6 - AC Verification | - |
| 2026-01-13 09:24 | END | ac-tester | All 14 ACs PASS | SUCCESS |
| 2026-01-13 09:25 | START | feature-reviewer | Phase 7.1 - Post Review (spec mode) | - |
| 2026-01-13 09:25 | END | feature-reviewer | All ACs verified, maintainability OK | READY |
| 2026-01-13 09:25 | START | feature-reviewer | Phase 7.2 - Doc-check | - |
| 2026-01-13 09:25 | DEVIATION | feature-reviewer | engine-dev SKILL.md missing F468 updates | NEEDS_REVISION |
| 2026-01-13 09:26 | END | Opus | Fixed engine-dev SKILL.md | SUCCESS |
| 2026-01-13 09:26 | END | - | Phase 7.3 - SSOT Update Check | SUCCESS |
