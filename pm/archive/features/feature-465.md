# Feature 465: Aggregate Root + Character Aggregate

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

**Phase 13: DDD Foundation - Core Aggregate Pattern Implementation**

Implement Aggregate Root base class and Character aggregate as the foundation of Domain-Driven Design patterns. This establishes encapsulation of business rules and invariant enforcement within domain entities.

**Output**:
- `Era.Core/Domain/AggregateRoot.cs` - Base class for all aggregates with domain event support
- `Era.Core/Domain/Aggregates/Character.cs` - Character aggregate root implementation
- `Era.Core/Domain/ValueObjects/CharacterStats.cs` - Character statistics value object
- `Era.Core/Domain/ValueObjects/CharacterName.cs` - Character name value object
- `Era.Core/Domain/Events/IDomainEvent.cs` - Domain event interface
- `Era.Core/Domain/Events/CharacterCreatedEvent.cs` - Example domain event (demonstrates pattern)
- Unit tests in `Era.Core.Tests/Domain/`

**Note**: Uses existing `Era.Core/Types/CharacterId.cs` (not creating new CharacterId).

**Volume**: ~250 lines (base class + aggregate + 3 value objects + tests)

---

## Background

### Philosophy (Mid-term Vision)

**Phase 13: DDD Foundation** - Establish Domain-Driven Design foundation through Aggregate Root, Repository, and UnitOfWork patterns. This phase encapsulates business rules within domain entities, enforces invariants, and provides abstraction layer for future persistence migration from flat ERB arrays to structured domain models.

### Problem (Current Issue)

Phase 12 completion leaves game state as flat ERB-style arrays without:
- Business rule encapsulation within domain entities
- Invariant enforcement at aggregate boundaries
- Domain event tracking for state changes
- Clear aggregate root pattern for future Repository/UnitOfWork implementation

### Goal (What to Achieve)

1. **Define AggregateRoot base class** with domain event support
2. **Implement Character aggregate** with business logic encapsulation
3. **Create value objects** for CharacterStats, CharacterName (uses existing CharacterId)
4. **Establish IDomainEvent interface** for domain event pattern
5. **Verify invariant enforcement** through unit tests
6. **Eliminate technical debt** (no TODO/FIXME/HACK comments)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | AggregateRoot.cs exists | file | Glob | exists | "Era.Core/Domain/AggregateRoot.cs" | [x] |
| 2 | AggregateRoot has domain event support | code | Grep | contains | "DomainEvents" | [x] |
| 3 | Character.cs exists | file | Glob | exists | "Era.Core/Domain/Aggregates/Character.cs" | [x] |
| 4 | Character inherits AggregateRoot | code | Grep | contains | "class Character : AggregateRoot<CharacterId>" | [x] |
| 5 | CharacterStats value object exists | file | Glob | exists | "Era.Core/Domain/ValueObjects/CharacterStats.cs" | [x] |
| 6 | CharacterName value object exists | file | Glob | exists | "Era.Core/Domain/ValueObjects/CharacterName.cs" | [x] |
| 7 | CharacterId exists (use existing) | file | Glob | exists | "Era.Core/Types/CharacterId.cs" | [x] |
| 8 | IDomainEvent interface exists | file | Glob | exists | "Era.Core/Domain/Events/IDomainEvent.cs" | [x] |
| 9 | Unit tests exist | file | Glob | exists | "Era.Core.Tests/Domain/AggregateRootTests.cs" | [x] |
| 10 | Unit tests pass | test | Bash | succeeds | "dotnet test Era.Core.Tests --filter Category=DDD" | [x] |
| 11 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [x] |
| 12 | Namespace follows convention | code | Grep | contains | "namespace Era.Core.Domain" | [x] |
| 13 | CharacterCreatedEvent exists | file | Glob | exists | "Era.Core/Domain/Events/CharacterCreatedEvent.cs" | [x] |

### AC Details

**AC#1**: AggregateRoot.cs file existence
- Test: Glob pattern="Era.Core/Domain/AggregateRoot.cs"
- Expected: File exists

**AC#2**: Domain event support in AggregateRoot
- Test: Grep pattern="DomainEvents" path="Era.Core/Domain/AggregateRoot.cs"
- Expected: Contains DomainEvents property (full signature: `IReadOnlyList<IDomainEvent> DomainEvents`)
- Verifies: Base class provides event collection management

**AC#3**: Character aggregate file existence
- Test: Glob pattern="Era.Core/Domain/Aggregates/Character.cs"
- Expected: File exists

**AC#4**: Character inherits AggregateRoot pattern
- Test: Grep pattern="class Character : AggregateRoot<CharacterId>" path="Era.Core/Domain/Aggregates/Character.cs"
- Expected: Character aggregate uses strongly-typed ID
- Verifies: Aggregate Root pattern correctly applied

**AC#5**: CharacterStats value object
- Test: Glob pattern="Era.Core/Domain/ValueObjects/CharacterStats.cs"
- Expected: File exists
- Verifies: Character state encapsulated in value object

**AC#6**: CharacterName value object
- Test: Glob pattern="Era.Core/Domain/ValueObjects/CharacterName.cs"
- Expected: File exists
- Verifies: Name identity encapsulated

**AC#7**: CharacterId exists (use existing Era.Core/Types/CharacterId.cs)
- Test: Glob pattern="Era.Core/Types/CharacterId.cs"
- Expected: File exists
- Verifies: Existing strongly-typed ID is reused (no duplication)

**AC#8**: IDomainEvent interface
- Test: Glob pattern="Era.Core/Domain/Events/IDomainEvent.cs"
- Expected: File exists
- Verifies: Domain event base contract defined

**AC#9**: Unit test file existence
- Test: Glob pattern="Era.Core.Tests/Domain/AggregateRootTests.cs"
- Expected: File exists containing 7 test cases per Test Requirements section
- Verifies: Test coverage for aggregate behavior (domain events, validation, immutability)

**AC#10**: All tests pass
- Test: `dotnet test Era.Core.Tests --filter Category=DDD`
- Expected: PASS with all DDD category tests
- Verifies: Aggregate invariant enforcement and domain event tracking work correctly

**AC#11**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Domain/" (recursive, ripgrep syntax)
- Expected: 0 matches
- Verifies: Clean implementation with no deferred work

**AC#12**: Namespace convention
- Test: Grep pattern="namespace Era.Core.Domain" path="Era.Core/Domain/"
- Expected: All files use consistent namespace
- Verifies: Project structure follows conventions

**AC#13**: CharacterCreatedEvent file existence
- Test: Glob pattern="Era.Core/Domain/Events/CharacterCreatedEvent.cs"
- Expected: File exists
- Verifies: Example domain event implementing IDomainEvent pattern

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Create AggregateRoot base class with domain event support | [x] |
| 2 | 3,4 | Implement Character aggregate inheriting AggregateRoot | [x] |
| 3 | 5 | Create CharacterStats value object (readonly record struct) | [x] |
| 4 | 6 | Create CharacterName value object with validation | [x] |
| 5 | 7 | Verify existing CharacterId in Era.Core/Types/ meets DDD requirements | [x] |
| 6 | 8,13 | Define IDomainEvent interface and CharacterCreatedEvent for event pattern | [x] |
| 7 | 9,10 | Write unit tests per Test Requirements section (7 test cases) | [x] |
| 8 | 11 | Verify zero TODO/FIXME/HACK comments in Domain/ directory | [x] |
| 9 | 12 | Verify namespace consistency across all domain files | [x] |

<!-- AC:Task 1:1 Rule: 13 ACs = 9 Tasks (batch waivers: Task 1 for AC 1-2 related base class, Task 2 for AC 3-4 related aggregate, Task 6 for AC 8,13 domain event pattern, Task 7 for AC 9-10 test creation+execution) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### DDD Pattern Requirements

**Aggregate Root Pattern**:
- All aggregate state changes go through aggregate root methods
- Aggregates maintain their own invariants
- External access only via aggregate root interface
- Domain events raised for state changes requiring external notification

**Value Object Pattern**:
- Immutable state (readonly record struct or class)
- Equality based on value, not identity
- No setters - use `with` expressions for modifications

### Core Interface Definition

```csharp
// Era.Core/Domain/AggregateRoot.cs
using Era.Core.Domain.Events;

namespace Era.Core.Domain;

// Deviation from architecture.md: Using 'protected init' instead of 'protected set'
// for compile-time immutability. This prevents accidental ID mutation after creation,
// which is a DDD best practice for aggregate identity invariants.
public abstract class AggregateRoot<TId> where TId : struct
{
    public TId Id { get; protected init; }
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    protected void AddDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
    public void ClearDomainEvents() => _domainEvents.Clear();
}
```

### Domain Event Interface

```csharp
// Era.Core/Domain/Events/IDomainEvent.cs
namespace Era.Core.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
```

### Example Domain Event

```csharp
// Era.Core/Domain/Events/CharacterCreatedEvent.cs
using Era.Core.Types;

namespace Era.Core.Domain.Events;

public record CharacterCreatedEvent(CharacterId CharacterId) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
```

### Character Aggregate Example

```csharp
// Era.Core/Domain/Aggregates/Character.cs
// Note: AbilitySet, TalentSet properties and ApplyTraining method deferred to follow-up
// feature per 残課題 section (architecture.md Task 2 scope reduction).
using Era.Core.Domain.ValueObjects;
using Era.Core.Types;

namespace Era.Core.Domain.Aggregates;

public class Character : AggregateRoot<CharacterId>
{
    public CharacterName Name { get; private set; }
    public CharacterStats Stats { get; private set; }

    private Character() { } // Private constructor for factory pattern

    // Factory method: Takes pre-validated inputs (CharacterName already validated via Result<T>)
    // Returns Character directly, not Result<Character>, because inputs are guaranteed valid
    public static Character Create(CharacterId id, CharacterName name, CharacterStats stats)
    {
        var character = new Character
        {
            Id = id,
            Name = name,
            Stats = stats
        };
        character.AddDomainEvent(new CharacterCreatedEvent(id));
        return character;
    }
}
```

### Value Object Examples

**Note**: CharacterId already exists at `Era.Core/Types/CharacterId.cs`. Use existing type, do not create new.

```csharp
// Era.Core/Domain/ValueObjects/CharacterName.cs
using Era.Core.Types;

namespace Era.Core.Domain.ValueObjects;

public readonly record struct CharacterName
{
    public string Value { get; }

    private CharacterName(string value) => Value = value;

    public static Result<CharacterName> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<CharacterName>.Fail("Character name cannot be empty");

        return Result<CharacterName>.Ok(new CharacterName(name));
    }
}
```

```csharp
// Era.Core/Domain/ValueObjects/CharacterStats.cs
namespace Era.Core.Domain.ValueObjects;

public readonly record struct CharacterStats(
    int Health,
    int Stamina,
    int Frustration,
    int Loyalty
)
{
    public CharacterStats ConsumeStamina(int amount) =>
        this with { Stamina = Math.Max(0, Stamina - amount) };
}
```

### Test Requirements

Tests must verify:
1. AggregateRoot domain event collection and clearing
2. Character aggregate creation
3. CharacterName validation (empty string rejection)
4. CharacterStats immutability and with-expressions
5. Domain event raising when state changes
6. CharacterName.Create with empty string returns Result.Failure (negative test)
7. CharacterName.Create with whitespace-only string returns Result.Failure (negative test)

**Test Category**: All tests must be marked with `[Trait("Category", "DDD")]`

### Error Handling

| Scenario | Use |
|----------|-----|
| Invalid character name | `Result<CharacterName>.Fail()` |
| Null argument | `ArgumentNullException` |
| Business rule violation | `Result<T>.Fail()` with descriptive message |

---

## 残課題

| ID | Description | Deferred To | Rationale |
|----|-------------|-------------|-----------|
| 1 | AbilitySet value object | F472 | Scope reduction for Phase 13 foundation - add when Character business logic needed |
| 2 | TalentSet value object | F472 | Same as above |
| 3 | ApplyTraining domain method | F472 | Requires AbilitySet/TalentSet dependencies |

**Note**: Per architecture.md Phase 13 Task 2, these were originally in scope. User decision (FL iter4) deferred to follow-up feature for incremental implementation.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F463 | Phase 13 Planning - defines DDD Foundation scope |
| Predecessor | F462 | Phase 12 Post-Phase Review must pass first |
| Successor | F466 | Repository Pattern - depends on Aggregate Root |
| Successor | F467 | UnitOfWork Pattern - depends on Character aggregate |

---

## Links

- [feature-462.md](feature-462.md) - Phase 12 Post-Phase Review (predecessor)
- [feature-463.md](feature-463.md) - Phase 13 Planning (parent feature)
- [feature-466.md](feature-466.md) - Repository Pattern (successor)
- [feature-467.md](feature-467.md) - UnitOfWork Pattern (successor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 13 Tasks 1-2

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-12 FL iter1**: [resolved] Phase2-Validate - Implementation Contract Character.Create: Character.Create returns Character directly (not Result<Character>) because it takes pre-validated inputs (CharacterName is already Result<T> validated). Documented in Implementation Contract.
- **2026-01-12 FL iter1**: [resolved] Phase2-Validate - AC#12 namespace pattern: Pattern 'namespace Era.Core.Domain' matches sub-namespaces as prefix. Grep works correctly.
- **2026-01-12 FL iter2**: [resolved] Phase2-Validate - UpdateStats method scope: Verified - UpdateStats method does NOT exist in Implementation Contract. CharacterStats.ConsumeStamina demonstrates value object immutability pattern. No change needed.
- **2026-01-12 FL iter3**: [resolved] Phase2-Validate - AC#11 directory dependency: Task 8 reworded from "Remove" to "Verify zero" for new directory semantics. Task ordering (1-6 before 8) ensures directory exists.
- **2026-01-12 FL iter3**: [resolved] Phase2-Validate - AggregateRoot.Id immutability: Changed 'protected set' to 'protected init' for DDD ID immutability.
- **2026-01-12 FL iter3**: [resolved] Phase2-Validate - AC#10 filter syntax: Both quoted and unquoted '--filter Category=DDD' work. Current syntax is valid.
- **2026-01-12 FL iter4**: [applied] Phase2-Validate - SCOPE: AbilitySet/TalentSet missing from F465. User decision: Create new feature for remaining value objects (AbilitySet, TalentSet, ApplyTraining). Update dependencies and references.
- **2026-01-12 FL iter4**: [applied] Phase2-Validate - SCOPE: ApplyTraining domain method. User decision: Same as above - defer to new feature with full value objects.
- **2026-01-12 FL iter4**: [applied] Phase2-Validate - CharacterCreatedEvent AC. User decision: Added AC#13 for explicit file existence verification. Task 6 updated to include AC#13.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-12 | create | feature-builder | Created from F463 Phase 13 Planning | PROPOSED |
| 2026-01-12 21:32 | START | implementer | Tasks 1-9 implementation | - |
| 2026-01-12 21:32 | END | implementer | Tasks 1-9 implementation | SUCCESS |
