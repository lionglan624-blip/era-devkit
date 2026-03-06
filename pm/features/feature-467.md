# Feature 467: UnitOfWork Pattern

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

**Phase 13: DDD Foundation - UnitOfWork Pattern Implementation**

Implement Unit of Work pattern to coordinate multiple repository changes within a single transaction boundary. This establishes atomic commit semantics and completes Phase 9 TransactionBehavior stub implementation with full BeginTransaction/Commit/Rollback logic.

**Output**:
- `Era.Core/Domain/IUnitOfWork.cs` - Unit of Work interface
- `Era.Core/Infrastructure/UnitOfWork.cs` - In-memory UnitOfWork implementation
- `Era.Core/Commands/Behaviors/TransactionBehavior.cs` - Full implementation replacing F430 stub
- Unit tests in `Era.Core.Tests/Infrastructure/`

**Volume**: ~300 lines (interface + implementation + transaction behavior + tests)

---

## Background

### Philosophy (Mid-term Vision)

**Phase 13: DDD Foundation** - Establish Domain-Driven Design foundation through Aggregate Root, Repository, and UnitOfWork patterns. Unit of Work coordinates changes across multiple aggregates within transaction boundaries, ensuring atomic commits and providing rollback capability for maintaining consistency.

### Problem (Current Issue)

With Repository pattern established (F466), we need:
- Transaction boundary definition for multi-aggregate operations
- Atomic commit semantics across multiple repositories
- Rollback capability when business rules fail
- Phase 9 TransactionBehavior stub (F430) completion with full BeginTransaction/Commit/Rollback implementation

### Goal (What to Achieve)

1. **Define IUnitOfWork interface** with repository accessors and CommitAsync/Rollback methods
2. **Implement UnitOfWork** with in-memory transaction tracking
3. **Complete TransactionBehavior** with IUnitOfWork integration (replaces F430 pass-through stub)
4. **Verify transaction interface** through unit tests (commit success, rollback callable - in-memory stub semantics; full rollback deferred to database implementation)
5. **Eliminate technical debt** (no TODO/FIXME/HACK comments)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IUnitOfWork.cs exists | file | Glob | exists | "Era.Core/Domain/IUnitOfWork.cs" | [x] |
| 2 | IUnitOfWork has CommitAsync | code | Grep | contains | "Task<int> CommitAsync.*CancellationToken" | [x] |
| 3 | IUnitOfWork has Rollback | code | Grep | contains | "void Rollback" | [x] |
| 4 | IUnitOfWork has repository accessor | code | Grep | contains | "IRepository<.*Character.*CharacterId> Characters" | [x] |
| 5 | UnitOfWork.cs exists | file | Glob | exists | "Era.Core/Infrastructure/UnitOfWork.cs" | [x] |
| 6 | UnitOfWork implements IUnitOfWork | code | Grep | contains | "class UnitOfWork.*IUnitOfWork" | [x] |
| 7 | TransactionBehavior has UnitOfWork dependency | code | Grep | contains | "IUnitOfWork.*_unitOfWork" | [x] |
| 8 | TransactionBehavior calls CommitAsync | code | Grep | contains | "CommitAsync" | [x] |
| 9 | UnitOfWork unit tests exist | file | Glob | exists | "Era.Core.Tests/Infrastructure/UnitOfWorkTests.cs" | [x] |
| 10 | Commit test passes | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestCommit" | [x] |
| 11 | Rollback test passes | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestRollback" | [x] |
| 12 | All UnitOfWork tests pass | test | Bash | succeeds | "dotnet test Era.Core.Tests --filter Category=UnitOfWork" | [x] |
| 13 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [x] |
| 14 | Namespace follows convention | code | Grep | contains | "namespace Era.Core.Domain|namespace Era.Core.Infrastructure|namespace Era.Core.Commands.Behaviors" | [x] |
| 15 | TransactionBehavior registered as Scoped | code | Grep | contains | "AddScoped.*TransactionBehavior" | [x] |

### AC Details

**AC#1**: IUnitOfWork interface file existence
- Test: Glob pattern="Era.Core/Domain/IUnitOfWork.cs"
- Expected: File exists

**AC#2**: IUnitOfWork CommitAsync method
- Test: Grep pattern="Task<int> CommitAsync.*CancellationToken" path="Era.Core/Domain/IUnitOfWork.cs"
- Expected: Contains async commit signature with cancellation support
- Verifies: Standard UnitOfWork commit pattern

**AC#3**: IUnitOfWork Rollback method
- Test: Grep pattern="void Rollback" path="Era.Core/Domain/IUnitOfWork.cs"
- Expected: Contains rollback signature
- Verifies: Transaction abort capability

**AC#4**: IUnitOfWork repository accessor
- Test: Grep pattern="IRepository<.*Character.*CharacterId> Characters" path="Era.Core/Domain/IUnitOfWork.cs"
- Expected: Contains Characters repository property (pattern allows namespace prefix like Aggregates.Character)
- Verifies: Repository coordination through UnitOfWork

**AC#5**: UnitOfWork implementation file
- Test: Glob pattern="Era.Core/Infrastructure/UnitOfWork.cs"
- Expected: File exists

**AC#6**: UnitOfWork implements interface
- Test: Grep pattern="class UnitOfWork.*IUnitOfWork" path="Era.Core/Infrastructure/UnitOfWork.cs"
- Expected: Implements IUnitOfWork
- Verifies: Concrete implementation provided

**AC#7**: TransactionBehavior has UnitOfWork dependency
- Test: Grep pattern="IUnitOfWork.*_unitOfWork" path="Era.Core/Commands/Behaviors/TransactionBehavior.cs"
- Expected: Contains dependency injection field (pattern allows 'readonly' modifier)
- Verifies: TransactionBehavior integrated with UnitOfWork

**AC#8**: TransactionBehavior calls CommitAsync
- Test: Grep pattern="CommitAsync" path="Era.Core/Commands/Behaviors/TransactionBehavior.cs"
- Expected: Contains method call
- Verifies: Transaction commit logic present

**AC#9**: Unit test file existence
- Test: Glob pattern="Era.Core.Tests/Infrastructure/UnitOfWorkTests.cs"
- Expected: File exists

**AC#10**: Commit operation test
- Test: `dotnet test --filter FullyQualifiedName~TestCommit`
- Expected: PASS
- Verifies: Multi-repository changes committed atomically

**AC#11**: Rollback operation test
- Test: `dotnet test --filter FullyQualifiedName~TestRollback`
- Expected: PASS
- Verifies: Rollback can be called safely (no exception). Actual rollback semantics deferred to database implementation.

**AC#12**: All UnitOfWork tests pass
- Test: `dotnet test Era.Core.Tests --filter Category=UnitOfWork`
- Expected: PASS with all UnitOfWork category tests
- Verifies: Complete transaction semantics correctness
- Prerequisite: Tests must have `[Trait("Category", "UnitOfWork")]` annotation (see Test Requirements)

**AC#13**: Zero technical debt
- Test: Run Grep with pattern="TODO|FIXME|HACK" on each file: Era.Core/Domain/IUnitOfWork.cs, Era.Core/Infrastructure/UnitOfWork.cs, Era.Core/Commands/Behaviors/TransactionBehavior.cs, Era.Core.Tests/Infrastructure/UnitOfWorkTests.cs
- Expected: 0 matches across all files
- Verifies: Clean implementation
- Note: Tester runs multiple Grep invocations, one per file

**AC#14**: Namespace convention
- Test: Sanity check - Grep pattern="namespace Era.Core.Domain|namespace Era.Core.Infrastructure|namespace Era.Core.Commands.Behaviors" on created files
- Expected: Each file uses its appropriate namespace (Domain files use Era.Core.Domain, Infrastructure files use Era.Core.Infrastructure, Behaviors use Era.Core.Commands.Behaviors)
- Verifies: Project structure follows conventions (sanity check only - compiler enforces actual correctness)

**AC#15**: TransactionBehavior registered as Scoped
- Test: Grep pattern="AddScoped.*TransactionBehavior" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: Contains Scoped registration
- Verifies: DI lifetime compatible with Scoped IUnitOfWork dependency (Singleton cannot inject Scoped)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4 | Define IUnitOfWork interface with CommitAsync, Rollback, and Characters repository accessor | [x] |
| 2 | 5,6 | Implement UnitOfWork with in-memory transaction tracking | [x] |
| 3 | 7,8 | Add IUnitOfWork dependency to TransactionBehavior and implement CommitAsync/Rollback calls | [x] |
| 4 | 9,10,11 | Write unit tests for Commit and Rollback transaction semantics | [x] |
| 5 | 12 | Verify all UnitOfWork tests pass with Category=UnitOfWork | [x] |
| 6 | 13 | Remove all TODO/FIXME/HACK comments and Phase 9/Phase 11 legacy comments from TransactionBehavior.cs | [x] |
| 7 | 14 | Verify namespace consistency across UnitOfWork interface and implementations | [x] |
| 8 | 15 | Update TransactionBehavior DI registration from Singleton to Scoped | [x] |

<!-- AC:Task 1:1 Rule: 15 ACs = 8 Tasks (batch waivers: Task 1 for AC 1-4 related interface definition, Task 3 for AC 7-8 stub completion, Task 4 for AC 9-11 test creation) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Unit of Work Pattern Requirements

**Unit of Work Pattern**:
- Coordinates changes across multiple repositories
- Tracks changes in memory until CommitAsync called
- Provides atomic commit (all or nothing)
- Rollback discards uncommitted changes
- Implements IDisposable for resource cleanup

### Interface Definition

```csharp
// Era.Core/Domain/IUnitOfWork.cs
using Era.Core.Domain.Aggregates;
using Era.Core.Types;

namespace Era.Core.Domain;

public interface IUnitOfWork : IDisposable
{
    IRepository<Character, CharacterId> Characters { get; }
    Task<int> CommitAsync(CancellationToken ct = default);
    void Rollback();
}
```

### UnitOfWork Implementation

```csharp
// Era.Core/Infrastructure/UnitOfWork.cs
using Era.Core.Domain;
using Era.Core.Domain.Aggregates;
using Era.Core.Types;

namespace Era.Core.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly IRepository<Character, CharacterId> _characterRepository;
    private bool _disposed;

    public IRepository<Character, CharacterId> Characters => _characterRepository;

    public UnitOfWork(IRepository<Character, CharacterId> characterRepository)
    {
        ArgumentNullException.ThrowIfNull(characterRepository);
        _characterRepository = characterRepository;
    }

    public Task<int> CommitAsync(CancellationToken ct = default)
    {
        // In-memory implementation: changes already applied to repositories
        // For future database implementation: call SaveChangesAsync here
        return Task.FromResult(1); // In-memory stub: returns 1 (actual count deferred to database implementation)
    }

    public void Rollback()
    {
        // In-memory implementation: no-op (would reload from DB in real implementation)
        // For now, caller must track original state
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
    }
}
```

### TransactionBehavior Full Implementation

**Note**: TransactionBehavior already exists at `Era.Core/Commands/Behaviors/TransactionBehavior.cs` (F430 stub). This feature updates the existing stub with full transaction logic.

**Breaking Change**: Existing stub (parameterless) will be replaced with constructor injection pattern. DI container will automatically resolve IUnitOfWork dependency.

```csharp
// Era.Core/Commands/Behaviors/TransactionBehavior.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Era.Core.Types;
using Era.Core.Domain;

namespace Era.Core.Commands.Behaviors;

public class TransactionBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TResult>> Handle(TCommand request,
        Func<Task<Result<TResult>>> next, CancellationToken ct)
    {
        try
        {
            // Execute command handler
            var result = await next();

            // Commit changes if successful
            if (result.IsSuccess)
            {
                await _unitOfWork.CommitAsync(ct);
            }

            return result;
        }
        catch
        {
            // Rollback on any exception
            _unitOfWork.Rollback();
            throw;
        }
    }
}
```

### DI Registration

Update `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// Add UnitOfWork registration
services.AddScoped<IUnitOfWork, UnitOfWork>();

// Update TransactionBehavior from Singleton to Scoped (required for IUnitOfWork dependency)
// Change: services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
// To:     services.AddScoped(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
```

**Note**: TransactionBehavior registration must be changed from `AddSingleton` to `AddScoped`. A Singleton cannot inject Scoped dependencies - this is a .NET DI constraint. Since IUnitOfWork is Scoped, TransactionBehavior must also be Scoped.

### Test Requirements

Tests must verify:
1. CommitAsync persists changes from multiple repositories
2. Rollback can be called without exception (no-op for in-memory; actual semantics deferred to database implementation)
3. Dispose releases resources properly
4. TransactionBehavior commits on handler success
5. TransactionBehavior rolls back on handler exception
6. Multiple aggregate changes committed atomically

**Note**: TestCommit and TestRollback tests cover the essential scenarios (items 1-2, 4-5). Items 3 and 6 are covered by the comprehensive Category=UnitOfWork filter.

**Test Category**: All tests must be marked with `[Trait("Category", "UnitOfWork")]`

**Test Naming Convention**: Test methods follow `Test{Operation}` format (e.g., `TestCommit`, `TestRollback`). This ensures AC filter patterns match correctly.

### Error Handling

| Scenario | Use |
|----------|-----|
| Commit failure | Propagate exception from repository |
| Rollback after dispose | No-op (idempotent) |
| Null repository argument | `ArgumentNullException` |

### Migration Source Reference

**Legacy Location**: `Era.Core/Commands/Behaviors/TransactionBehavior.cs` (F430 stub)

| Component | Status | Notes |
|-----------|--------|-------|
| TransactionBehavior | STUB | Replace pass-through with full transaction logic |
| CommitAsync | MISSING | Add call to _unitOfWork.CommitAsync on success |
| Rollback | MISSING | Add call to _unitOfWork.Rollback in catch block |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F466 | Repository Pattern - UnitOfWork coordinates repositories |
| Predecessor | F465 | Aggregate Root + Character Aggregate - UnitOfWork operates on aggregates |
| Predecessor | F430 | TransactionBehavior stub creation (Phase 9) |
| Predecessor | F463 | Phase 13 Planning - defines DDD Foundation scope |
| Successor | F468 | Legacy Bridge + DI Integration - DI registration uses UnitOfWork |

---

## Links

- [feature-463.md](feature-463.md) - Phase 13 Planning (parent feature)
- [feature-466.md](feature-466.md) - Repository Pattern implementation (dependency)
- [feature-430.md](feature-430.md) - Phase 9 TransactionBehavior stub creation (created Era.Core/Commands/Behaviors/TransactionBehavior.cs)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 13 Tasks 4,6,7

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-12 | create | feature-builder | Created from F463 Phase 13 Planning | PROPOSED |
| 2026-01-13 07:21 | START | implementer | Task 4 TDD Phase 3 - Create unit tests | - |
| 2026-01-13 07:21 | END | implementer | Task 4 TDD Phase 3 - Create unit tests | RED |
| 2026-01-13 07:24 | START | implementer | Tasks 1-8 UnitOfWork implementation | - |
| 2026-01-13 07:24 | END | implementer | Tasks 1-8 UnitOfWork implementation | SUCCESS |
