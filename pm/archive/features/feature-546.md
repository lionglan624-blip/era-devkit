# Feature 546: Specification Pattern Infrastructure

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

## Background

### Philosophy (Mid-term Vision)

Phase 18: KojoEngine SRP分割 - Specification Pattern enables type-safe, composable condition logic for TALENT/ABL/EXP branching, replacing magic number comparisons with domain-specific specifications. This establishes the foundation for maintainable, testable dialogue condition evaluation.

### Problem (Current Issue)

KojoEngine mixes condition evaluation logic (TALENT checks, ABL thresholds) with dialogue selection and rendering, violating Single Responsibility Principle. Condition logic is scattered across methods using magic numbers and hard-to-test boolean expressions.

### Goal (What to Achieve)

Create the Specification Pattern infrastructure (ISpecification<T> interface and base classes) that enables composable, type-safe condition evaluation for TALENT/ABL/EXP checks. This provides the foundation for subsequent concrete specification implementations (F547) and composite specifications (F548).

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ISpecification.cs exists | file | Glob | exists | "Era.Core/Dialogue/Specifications/ISpecification.cs" | [x] |
| 2 | SpecificationBase.cs exists | file | Glob | exists | "Era.Core/Dialogue/Specifications/SpecificationBase.cs" | [x] |
| 3 | AndSpecification.cs exists | file | Glob | exists | "Era.Core/Dialogue/Specifications/AndSpecification.cs" | [x] |
| 4 | OrSpecification.cs exists | file | Glob | exists | "Era.Core/Dialogue/Specifications/OrSpecification.cs" | [x] |
| 5 | NotSpecification.cs exists | file | Glob | exists | "Era.Core/Dialogue/Specifications/NotSpecification.cs" | [x] |
| 6 | IsSatisfiedBy method signature | code | Grep(Era.Core/Dialogue/Specifications/ISpecification.cs) | contains | "bool IsSatisfiedBy\\(T entity\\)" | [x] |
| 7 | And method signature | code | Grep(Era.Core/Dialogue/Specifications/ISpecification.cs) | contains | "ISpecification<T> And\\(ISpecification<T> other\\)" | [x] |
| 8 | Or method signature | code | Grep(Era.Core/Dialogue/Specifications/ISpecification.cs) | contains | "ISpecification<T> Or\\(ISpecification<T> other\\)" | [x] |
| 9 | Not method signature | code | Grep(Era.Core/Dialogue/Specifications/ISpecification.cs) | contains | "ISpecification<T> Not\\(\\)" | [x] |
| 10 | SpecificationBase provides default combinators | code | Grep(Era.Core/Dialogue/Specifications/SpecificationBase.cs) | contains | "public virtual ISpecification<T> And" | [x] |
| 11 | Unit tests succeed | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~SpecificationTests" | [x] |
| 12 | Zero technical debt in Specifications/ | code | Grep | not_contains | "TODO\\|FIXME\\|HACK" | [x] |
| 13 | All tests PASS | test | Bash | succeeds | "dotnet test" | [x] |

### AC Details

**AC#1**: ISpecification.cs file creation
- Test: Glob pattern="Era.Core/Dialogue/Specifications/ISpecification.cs"
- Expected: File exists

**AC#2**: SpecificationBase.cs file creation
- Test: Glob pattern="Era.Core/Dialogue/Specifications/SpecificationBase.cs"
- Expected: File exists

**AC#3**: AndSpecification.cs file creation
- Test: Glob pattern="Era.Core/Dialogue/Specifications/AndSpecification.cs"
- Expected: File exists

**AC#4**: OrSpecification.cs file creation
- Test: Glob pattern="Era.Core/Dialogue/Specifications/OrSpecification.cs"
- Expected: File exists

**AC#5**: NotSpecification.cs file creation
- Test: Glob pattern="Era.Core/Dialogue/Specifications/NotSpecification.cs"
- Expected: File exists

**AC#6**: IsSatisfiedBy method in ISpecification
- Test: Grep pattern="bool IsSatisfiedBy\\(T entity\\)" path="Era.Core/Dialogue/Specifications/ISpecification.cs" type=cs
- Expected: Method signature found

**AC#7**: And method in ISpecification
- Test: Grep pattern="ISpecification<T> And\\(ISpecification<T> other\\)" path="Era.Core/Dialogue/Specifications/ISpecification.cs" type=cs
- Expected: Method signature found

**AC#8**: Or method in ISpecification
- Test: Grep pattern="ISpecification<T> Or\\(ISpecification<T> other\\)" path="Era.Core/Dialogue/Specifications/ISpecification.cs" type=cs
- Expected: Method signature found

**AC#9**: Not method in ISpecification
- Test: Grep pattern="ISpecification<T> Not\\(\\)" path="Era.Core/Dialogue/Specifications/ISpecification.cs" type=cs
- Expected: Method signature found

**AC#10**: SpecificationBase provides default And/Or/Not implementations
- Test: Grep pattern="public virtual ISpecification<T> And" path="Era.Core/Dialogue/Specifications/SpecificationBase.cs" type=cs
- Expected: Default combinator implementation found

**AC#11**: Unit tests for specification infrastructure
- Test: `dotnet test --filter FullyQualifiedName~SpecificationTests`
- Expected: Tests PASS
- Minimum: 4 tests (IsSatisfiedBy, And composition, Not negation, Null argument exception)

**AC#12**: Zero technical debt in Specifications/
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Dialogue/Specifications/" type=cs
- Expected: 0 matches

**AC#13**: All project tests PASS
- Test: `dotnet test`
- Expected: Exit code 0

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Create ISpecification.cs and SpecificationBase.cs with interface and base implementations | [x] |
| 2 | 3,4,5 | Create composite specification classes (And/Or/Not) | [x] |
| 3 | 6,7,8,9,10 | Verify all interface method signatures (batch verification waiver) | [x] |
| 4 | 11 | Create and run unit tests for specification infrastructure | [x] |
| 5 | 12,13 | Run full test suite and verify zero technical debt | [x] |

<!-- **Batch task waivers**: Following F384/F465 precedent patterns.
     Task 1 (AC 1-2): Interface and base class implementation are tightly coupled atomic operation.
     Task 2 (AC 3-5): Composite specification classes follow identical pattern (And/Or/Not).
     Task 3 (AC 6-10): Interface method signature verification (batch verification).
     Task 5 (AC 12-13): Technical debt verification and test execution typically run together.
     AC:Task 1:1 rule waived. If any AC fails, the entire task fails (atomic success/failure). -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Interface Definition

**File**: `Era.Core/Dialogue/Specifications/ISpecification.cs`

```csharp
using System;

namespace Era.Core.Dialogue.Specifications;

/// <summary>
/// Specification Pattern interface for composable, type-safe condition evaluation.
/// Enables chaining conditions with And/Or/Not operators.
/// </summary>
/// <typeparam name="T">The entity type to evaluate specifications against</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Evaluates whether the given entity satisfies this specification.
    /// </summary>
    /// <param name="entity">The entity to evaluate</param>
    /// <returns>True if the entity satisfies this specification, false otherwise</returns>
    bool IsSatisfiedBy(T entity);

    /// <summary>
    /// Creates a composite specification that requires both this and another specification to be satisfied.
    /// </summary>
    /// <param name="other">The other specification to combine with</param>
    /// <returns>A new specification representing the AND combination</returns>
    ISpecification<T> And(ISpecification<T> other);

    /// <summary>
    /// Creates a composite specification that requires either this or another specification to be satisfied.
    /// </summary>
    /// <param name="other">The other specification to combine with</param>
    /// <returns>A new specification representing the OR combination</returns>
    ISpecification<T> Or(ISpecification<T> other);

    /// <summary>
    /// Creates a specification that negates this specification.
    /// </summary>
    /// <returns>A new specification representing the NOT of this specification</returns>
    ISpecification<T> Not();
}
```

### Base Class Definition

**File**: `Era.Core/Dialogue/Specifications/SpecificationBase.cs`

```csharp
using System;

namespace Era.Core.Dialogue.Specifications;

/// <summary>
/// Base class providing default implementations for specification combinator methods.
/// Concrete specifications can inherit from this class to avoid reimplementing And/Or/Not.
/// </summary>
/// <typeparam name="T">The entity type to evaluate specifications against</typeparam>
public abstract class SpecificationBase<T> : ISpecification<T>
{
    /// <summary>
    /// Evaluates whether the given entity satisfies this specification.
    /// Must be implemented by concrete specifications.
    /// </summary>
    public abstract bool IsSatisfiedBy(T entity);

    /// <summary>
    /// Creates a composite specification that requires both this and another specification to be satisfied.
    /// </summary>
    public virtual ISpecification<T> And(ISpecification<T> other) =>
        new AndSpecification<T>(this, other);

    /// <summary>
    /// Creates a composite specification that requires either this or another specification to be satisfied.
    /// </summary>
    public virtual ISpecification<T> Or(ISpecification<T> other) =>
        new OrSpecification<T>(this, other);

    /// <summary>
    /// Creates a specification that negates this specification.
    /// </summary>
    public virtual ISpecification<T> Not() =>
        new NotSpecification<T>(this);
}
```

### Composite Specification Stubs

**File**: `Era.Core/Dialogue/Specifications/AndSpecification.cs`

```csharp
using System;

namespace Era.Core.Dialogue.Specifications;

/// <summary>
/// Composite specification that requires both left and right specifications to be satisfied.
/// Internal implementation for F546 testing compatibility.
/// F548 (Composite Specifications) provides public version inheriting from SpecificationBase.
/// </summary>
/// <typeparam name="T">The entity type to evaluate specifications against</typeparam>
internal class AndSpecification<T> : ISpecification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
    }

    public bool IsSatisfiedBy(T entity) => _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);

    public ISpecification<T> And(ISpecification<T> other) => new AndSpecification<T>(this, other);
    public ISpecification<T> Or(ISpecification<T> other) => new OrSpecification<T>(this, other);
    public ISpecification<T> Not() => new NotSpecification<T>(this);
}
```

**File**: `Era.Core/Dialogue/Specifications/OrSpecification.cs`

```csharp
using System;

namespace Era.Core.Dialogue.Specifications;

/// <summary>
/// Composite specification that requires either left or right specification to be satisfied.
/// Internal implementation for F546 testing compatibility.
/// F548 (Composite Specifications) provides public version inheriting from SpecificationBase.
/// </summary>
/// <typeparam name="T">The entity type to evaluate specifications against</typeparam>
internal class OrSpecification<T> : ISpecification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
    }

    public bool IsSatisfiedBy(T entity) => _left.IsSatisfiedBy(entity) || _right.IsSatisfiedBy(entity);

    public ISpecification<T> And(ISpecification<T> other) => new AndSpecification<T>(this, other);
    public ISpecification<T> Or(ISpecification<T> other) => new OrSpecification<T>(this, other);
    public ISpecification<T> Not() => new NotSpecification<T>(this);
}
```

**File**: `Era.Core/Dialogue/Specifications/NotSpecification.cs`

```csharp
using System;

namespace Era.Core.Dialogue.Specifications;

/// <summary>
/// Composite specification that negates another specification.
/// Internal implementation for F546 testing compatibility.
/// F548 (Composite Specifications) provides public version inheriting from SpecificationBase.
/// </summary>
/// <typeparam name="T">The entity type to evaluate specifications against</typeparam>
internal class NotSpecification<T> : ISpecification<T>
{
    private readonly ISpecification<T> _spec;

    public NotSpecification(ISpecification<T> spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
    }

    public bool IsSatisfiedBy(T entity) => !_spec.IsSatisfiedBy(entity);

    public ISpecification<T> And(ISpecification<T> other) => new AndSpecification<T>(this, other);
    public ISpecification<T> Or(ISpecification<T> other) => new OrSpecification<T>(this, other);
    public ISpecification<T> Not() => new NotSpecification<T>(this);
}
```

**Note**: These are functional implementations with `internal` visibility to enable F546 testing and compilation. F548 (Composite Specifications) will upgrade these to `public` classes inheriting from `SpecificationBase<T>` with enhanced features.

### Test Requirements

**File**: `Era.Core.Tests/Dialogue/Specifications/SpecificationTests.cs`

Create unit tests verifying:
1. IsSatisfiedBy returns expected boolean result
2. And combinator creates correct composite specification
3. Not negates specification correctly
4. Null argument to composite specifications throws ArgumentNullException

**Test Naming Convention**: Test methods follow `Test{ClassName}{Method}` format (e.g., `TestSpecificationBaseAnd`, `TestSpecificationBaseNot`). This ensures AC filter patterns match correctly.

### Design Rationale

**Why Specification Pattern?**
- **Type Safety**: Replace magic number comparisons with strongly-typed specification classes
- **Composability**: Enable complex conditions via And/Or/Not chaining
- **Testability**: Each specification is a discrete, testable unit
- **Maintainability**: Condition logic is centralized in specification classes, not scattered across KojoEngine

**Generic Type Parameter**: ISpecification<T> allows specifications to evaluate any entity type (IEvaluationContext for dialogue conditions, CharacterState for character conditions, etc.).

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F625 | [DONE] | Post-Phase Review Phase 17 (Data Migration) |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- [resolved-already-present] Phase1-Uncertain iter4: Task#1 and Task#2 violate AC:Task 1:1 principle - batch waivers already documented in HTML comment lines 122-127

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

**解決済み** - F546 provides full implementation with stub composite specifications. No pending handoffs.

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-26 06:47 | START | implementer | Task 1-5 (atomic implementation) | - |
| 2026-01-26 06:47 | END | implementer | Task 1-5 (atomic implementation) | SUCCESS |
| 2026-01-26 06:52 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION: ISpecification<T> not in engine-dev SKILL |
| 2026-01-26 06:53 | FIX | opus | SSOT update | Added ISpecification to engine-dev SKILL.md |

## Links

- [index-features.md](index-features.md)
- [Feature 541: Phase 18 Planning](feature-541.md)
- [Feature 547: Concrete Specifications](feature-547.md)
- [Feature 548: Composite Specifications](feature-548.md)
- [Feature 625: Post-Phase Review Phase 17](feature-625.md)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md)
