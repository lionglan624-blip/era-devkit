# Feature 548: Composite Specifications

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

Phase 18: KojoEngine SRP分割 - Specification Pattern enables type-safe, composable condition logic for TALENT/ABL/EXP branching, replacing magic number comparisons with domain-specific specifications. Composite specifications implement Boolean logic composition, enabling complex conditions via And/Or/Not chaining.

### Problem (Current Issue)

While concrete specifications (F547) handle individual TALENT/ABL checks, KojoEngine requires combining multiple conditions using Boolean logic (e.g., "TALENT:恋慕 AND ABL:V感覚 >= 3"). Without composite specifications, condition composition requires nested if-statements and repeated IsSatisfiedBy calls, reducing code clarity and testability.

### Goal (What to Achieve)

Upgrade AndSpecification, OrSpecification, and NotSpecification from internal to public visibility and inherit from SpecificationBase<T>. These enable fluent, chainable Boolean logic composition, allowing complex conditions to be expressed as `new TalentSpecification(恋慕).And(new AblSpecification(V感覚, 3))` instead of nested boolean expressions.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | AndSpecification is public class inheriting SpecificationBase | code | Grep | contains | "public class AndSpecification<T> : SpecificationBase<T>" | [x] |
| 2 | OrSpecification is public class inheriting SpecificationBase | code | Grep | contains | "public class OrSpecification<T> : SpecificationBase<T>" | [x] |
| 3 | NotSpecification is public class inheriting SpecificationBase | code | Grep | contains | "public class NotSpecification<T> : SpecificationBase<T>" | [x] |
| 4 | AndSpecification constructor accepts two specifications | code | Grep | contains | "AndSpecification\\(ISpecification<T> left, ISpecification<T> right\\)" | [x] |
| 5 | OrSpecification constructor accepts two specifications | code | Grep | contains | "OrSpecification\\(ISpecification<T> left, ISpecification<T> right\\)" | [x] |
| 6 | NotSpecification constructor accepts one specification | code | Grep | contains | "NotSpecification\\(ISpecification<T> spec\\)" | [x] |
| 7 | AndSpecification IsSatisfiedBy implements AND logic | code | Grep | contains | "_left.IsSatisfiedBy.*&&.*_right.IsSatisfiedBy" | [x] |
| 8 | OrSpecification IsSatisfiedBy implements OR logic | code | Grep | contains | "_left.IsSatisfiedBy.*\\|\\|.*_right.IsSatisfiedBy" | [x] |
| 9 | NotSpecification IsSatisfiedBy implements NOT logic | code | Grep | contains | "!.*_spec.IsSatisfiedBy" | [x] |
| 10 | Chainable API integration test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~SpecificationChainTests" | [x] |
| 11 | Zero technical debt in composite specifications | code | Grep | not_contains | "TODO\\|FIXME\\|HACK" | [x] |
| 12 | All tests PASS | test | Bash | succeeds | "dotnet test" | [x] |

### AC Details

**AC#1**: AndSpecification is public class inheriting SpecificationBase
- Test: Grep pattern="public class AndSpecification<T> : SpecificationBase<T>" path="Era.Core/Dialogue/Specifications/AndSpecification.cs" type=cs
- Expected: Class is public and inherits from SpecificationBase<T>

**AC#2**: OrSpecification is public class inheriting SpecificationBase
- Test: Grep pattern="public class OrSpecification<T> : SpecificationBase<T>" path="Era.Core/Dialogue/Specifications/OrSpecification.cs" type=cs
- Expected: Class is public and inherits from SpecificationBase<T>

**AC#3**: NotSpecification is public class inheriting SpecificationBase
- Test: Grep pattern="public class NotSpecification<T> : SpecificationBase<T>" path="Era.Core/Dialogue/Specifications/NotSpecification.cs" type=cs
- Expected: Class is public and inherits from SpecificationBase<T>

**AC#4**: AndSpecification constructor
- Test: Grep pattern="AndSpecification\\(ISpecification<T> left, ISpecification<T> right\\)" path="Era.Core/Dialogue/Specifications/AndSpecification.cs" type=cs
- Expected: Constructor accepts two ISpecification<T> parameters

**AC#5**: OrSpecification constructor
- Test: Grep pattern="OrSpecification\\(ISpecification<T> left, ISpecification<T> right\\)" path="Era.Core/Dialogue/Specifications/OrSpecification.cs" type=cs
- Expected: Constructor accepts two ISpecification<T> parameters

**AC#6**: NotSpecification constructor
- Test: Grep pattern="NotSpecification\\(ISpecification<T> spec\\)" path="Era.Core/Dialogue/Specifications/NotSpecification.cs" type=cs
- Expected: Constructor accepts one ISpecification<T> parameter

**AC#7**: AndSpecification AND logic
- Test: Grep pattern="_left.IsSatisfiedBy.*&&.*_right.IsSatisfiedBy" path="Era.Core/Dialogue/Specifications/AndSpecification.cs" type=cs
- Expected: IsSatisfiedBy implementation uses && operator

**AC#8**: OrSpecification OR logic
- Test: Grep pattern="_left.IsSatisfiedBy.*\\|\\|.*_right.IsSatisfiedBy" path="Era.Core/Dialogue/Specifications/OrSpecification.cs" type=cs
- Expected: IsSatisfiedBy implementation uses || operator

**AC#9**: NotSpecification NOT logic
- Test: Grep pattern="!.*_spec.IsSatisfiedBy" path="Era.Core/Dialogue/Specifications/NotSpecification.cs" type=cs
- Expected: IsSatisfiedBy implementation uses ! operator

**AC#10**: Chainable API integration test
- Test: `dotnet test --filter FullyQualifiedName~SpecificationChainTests`
- Expected: Tests PASS
- Verifies: `spec1.And(spec2).Or(spec3).Not()` chains correctly

**AC#11**: Zero technical debt in composite specifications
- Test: Grep pattern="TODO|FIXME|HACK" paths=["Era.Core/Dialogue/Specifications/AndSpecification.cs", "Era.Core/Dialogue/Specifications/OrSpecification.cs", "Era.Core/Dialogue/Specifications/NotSpecification.cs"] type=cs
- Expected: 0 matches

**AC#12**: All project tests PASS
- Test: `dotnet test`
- Expected: Exit code 0

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,4,7 | Upgrade AndSpecification.cs to public visibility and SpecificationBase<T> inheritance with constructor and AND logic implementation | [x] |
| 2 | 2,5,8 | Upgrade OrSpecification.cs to public visibility and SpecificationBase<T> inheritance with constructor and OR logic implementation | [x] |
| 3 | 3,6,9 | Upgrade NotSpecification.cs to public visibility and SpecificationBase<T> inheritance with constructor and NOT logic implementation | [x] |
| 4 | 10 | Create and run chainable API integration test | [x] |
| 5 | 11 | Verify zero technical debt in composite specification files | [x] |
| 6 | 12 | Run full test suite and verify PASS | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### AndSpecification Implementation

**File**: `Era.Core/Dialogue/Specifications/AndSpecification.cs`

**Upgrade**: Change from `internal class AndSpecification<T> : ISpecification<T>` to `public class AndSpecification<T> : SpecificationBase<T>`

```csharp
using System;

namespace Era.Core.Dialogue.Specifications;

/// <summary>
/// Composite specification that combines two specifications with AND logic.
/// </summary>
/// <typeparam name="T">The entity type to evaluate specifications against</typeparam>
public class AndSpecification<T> : SpecificationBase<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    /// <summary>
    /// Creates a composite specification that requires both specifications to be satisfied.
    /// </summary>
    /// <param name="left">The first specification</param>
    /// <param name="right">The second specification</param>
    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
    }

    /// <summary>
    /// Evaluates whether the entity satisfies both specifications.
    /// </summary>
    /// <param name="entity">The entity to evaluate</param>
    /// <returns>True if both specifications are satisfied, false otherwise</returns>
    public override bool IsSatisfiedBy(T entity) =>
        _left.IsSatisfiedBy(entity) && _right.IsSatisfiedBy(entity);
}
```

### OrSpecification Implementation

**File**: `Era.Core/Dialogue/Specifications/OrSpecification.cs`

**Upgrade**: Change from `internal class OrSpecification<T> : ISpecification<T>` to `public class OrSpecification<T> : SpecificationBase<T>`

```csharp
using System;

namespace Era.Core.Dialogue.Specifications;

/// <summary>
/// Composite specification that combines two specifications with OR logic.
/// </summary>
/// <typeparam name="T">The entity type to evaluate specifications against</typeparam>
public class OrSpecification<T> : SpecificationBase<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    /// <summary>
    /// Creates a composite specification that requires either specification to be satisfied.
    /// </summary>
    /// <param name="left">The first specification</param>
    /// <param name="right">The second specification</param>
    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
    }

    /// <summary>
    /// Evaluates whether the entity satisfies either specification.
    /// </summary>
    /// <param name="entity">The entity to evaluate</param>
    /// <returns>True if at least one specification is satisfied, false otherwise</returns>
    public override bool IsSatisfiedBy(T entity) =>
        _left.IsSatisfiedBy(entity) || _right.IsSatisfiedBy(entity);
}
```

### NotSpecification Implementation

**File**: `Era.Core/Dialogue/Specifications/NotSpecification.cs`

**Upgrade**: Change from `internal class NotSpecification<T> : ISpecification<T>` to `public class NotSpecification<T> : SpecificationBase<T>` and align field naming with existing code

```csharp
using System;

namespace Era.Core.Dialogue.Specifications;

/// <summary>
/// Composite specification that negates another specification.
/// </summary>
/// <typeparam name="T">The entity type to evaluate specifications against</typeparam>
public class NotSpecification<T> : SpecificationBase<T>
{
    private readonly ISpecification<T> _spec;

    /// <summary>
    /// Creates a specification that negates the given specification.
    /// </summary>
    /// <param name="spec">The specification to negate</param>
    public NotSpecification(ISpecification<T> spec)
    {
        _spec = spec ?? throw new ArgumentNullException(nameof(spec));
    }

    /// <summary>
    /// Evaluates whether the entity does NOT satisfy the specification.
    /// </summary>
    /// <param name="entity">The entity to evaluate</param>
    /// <returns>True if the specification is not satisfied, false otherwise</returns>
    public override bool IsSatisfiedBy(T entity) =>
        !_spec.IsSatisfiedBy(entity);
}
```

### Test Requirements

**File**: `Era.Core.Tests/Dialogue/Specifications/SpecificationChainTests.cs`

Create integration test verifying:
1. Chainable API: `spec1.And(spec2).Or(spec3).Not()` evaluates correctly
2. Complex condition: `new AlwaysTrueSpecification().And(new AlwaysFalseSpecification())` evaluates correctly

**Test Naming Convention**: Test methods follow `Test{ClassName}{Scenario}` format (e.g., `TestSpecificationChainBothTrue`). This ensures AC filter patterns match correctly.

**Test Coverage Rationale**: Comprehensive And/Or/Not unit tests already exist in `SpecificationTests.cs` (F546) and provide complete coverage for composite specification logic. F548 focuses on upgrading visibility and inheritance; the chainable API integration test verifies the public upgrade worked correctly.

### Design Rationale

**Binary Composition**: AndSpecification and OrSpecification accept exactly two specifications (left/right) rather than a collection. This follows the Composite Pattern design where complex compositions are built via chaining (e.g., `spec1.And(spec2).And(spec3)`) rather than constructor arrays.

**Null Validation**: All composite specifications validate constructor parameters to prevent NullReferenceException during composition.

**Short-Circuit Evaluation**: C# && and || operators provide short-circuit evaluation automatically. AndSpecification stops evaluating if left is false; OrSpecification stops if left is true. This optimization is built-in without explicit code.

**Fluent API**: SpecificationBase.And/Or/Not methods (F546) return composite specifications, enabling fluent chaining: `spec.And(spec2).Or(spec3)`.

**Redundant Method Removal**: Existing composite specification classes implement And/Or/Not methods directly. After inheriting from SpecificationBase<T>, these methods become redundant and should be removed as they are provided by the base class.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F625 | [DONE] | Post-Phase Review Phase 17 (Data Migration) |
| Predecessor | F546 | [DONE] | ISpecification<T> and SpecificationBase<T> infrastructure required |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- [resolved-applied] Phase1-Uncertain iter1: Contract shows classes inheriting SpecificationBase with different field names (_left/_right vs left/right) and using ArgumentNullException.ThrowIfNull vs ?? throw pattern. Misaligned with existing code
- [resolved-applied] Phase1-Uncertain iter3: Existing SpecificationTests.cs already tests And/Or/Not functionality via SpecificationBase. New separate test classes may be redundant. Resolution: Removed redundant AC#10-12 and Tasks#4-6 (AndSpecificationTests, OrSpecificationTests, NotSpecificationTests). Retained AC#10 and Task#4 (SpecificationChainTests) to verify public chainable API upgrade.

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| - | - | - | - |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-26 08:03 | START | implementer | Task 4 (Create chainable API integration test) | - |
| 2026-01-26 08:03 | END | implementer | Task 4 | SUCCESS (Tests PASS - GREEN state due to internal classes being accessible via SpecificationBase) |
| 2026-01-26 | DEVIATION | Bash | dotnet test | exit code 1 - PRE-EXISTING build errors in TalentSpecificationTests, AblSpecificationTests, TemplateDialogueRendererTests (F547/F544 related, not F548) |
| 2026-01-26 | DEVIATION | feature-reviewer | post | NEEDS_REVISION - Stale build cache caused false BLOCKED status. Tests pass (1373/1373) |
| 2026-01-26 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION - engine-dev SKILL.md line 222-223 needed update for F548 completion |

## Links

- [index-features.md](index-features.md)
- [Feature 541: Phase 18 Planning](feature-541.md)
- [Feature 546: Specification Pattern Infrastructure](feature-546.md)
- [Feature 547: Concrete Specifications](feature-547.md)
- [Feature 625: Post-Phase Review Phase 17](feature-625.md)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md)
