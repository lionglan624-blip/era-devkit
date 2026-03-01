# Feature 417: Operator Implementation

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

## Created: 2026-01-09

---

## Summary

Migrate 30+ operators from `engine/Assets/Scripts/Emuera/GameData/Expression/OperatorMethod.cs` (~832 lines) to `Era.Core/Expressions/Operators.cs` with full category coverage.

**Output**: `Era.Core/Expressions/Operators.cs` as primary deliverable.

---

## Background

### Philosophy (Mid-term Vision)

**Expression & Function System** - Migrate expression evaluation and built-in functions from legacy engine to Era.Core with strong typing and Result-based error handling.

This feature focuses on implementing all operator categories (Arithmetic, Comparison, Logical, Bitwise, String, Ternary) as the foundation for expression evaluation, contributing to the systematic migration of the 30+ operators currently implemented in OperatorMethod.cs.

### Problem (Current Issue)

Phase 8 requires operator migration from legacy engine to Era.Core:
- `engine/Assets/Scripts/Emuera/GameData/Expression/OperatorMethod.cs` contains 30+ operator implementations (~832 lines)
- Operators span 7 categories: Arithmetic (5), Comparison (6), Logical (5), Bitwise (5), String (2), Ternary (1), Unary (8)
- Legacy implementation uses static dictionaries and lacks Result-based error handling
- No interface abstraction for DI compatibility (violates Phase 4 design principles)

### Goal (What to Achieve)

1. **Migrate all 30+ operators** from OperatorMethod.cs to Era.Core/Expressions/Operators.cs
2. **Apply Phase 4 design principles**:
   - Interface abstraction (`IOperatorRegistry`, `IOperator`)
   - Result type for error handling
   - DI registration via ServiceCollectionExtensions
3. **Implement all 7 operator categories**:
   - Arithmetic: `+`, `-`, `*`, `/`, `%` (5 binary operators)
   - Comparison: `==`, `!=`, `<`, `>`, `<=`, `>=` (6 operators)
   - Logical: `&&`, `||`, `^^`, `!&`, `!|` (5 binary operators including NAND/NOR)
   - Bitwise: `&`, `|`, `^`, `<<`, `>>` (5 binary operators)
   - String: `+` (concat), `*` (repeat) (2 operators)
   - Ternary: `? :` (1 operator using `#` as separator)
   - Unary: prefix `+`, `-`, `!`, `~`, `++`, `--` and postfix `++`, `--` (8 operator variants)
4. **Remove technical debt**: Eliminate all TODO/FIXME/HACK comments
5. **Verify legacy equivalence**: Ensure operator behavior matches legacy implementation

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Operators.cs created | file | Glob | exists | Era.Core/Expressions/Operators.cs | [x] |
| 2 | IOperatorRegistry interface | code | Grep | contains | "interface IOperatorRegistry" | [x] |
| 3 | Arithmetic operators (5) | test | dotnet test | succeeds | OperatorsArithmeticTests | [x] |
| 4 | Comparison operators (6) | test | dotnet test | succeeds | OperatorsComparisonTests | [x] |
| 5 | Logical operators (5) | test | dotnet test | succeeds | OperatorsLogicalTests | [x] |
| 6 | Bitwise operators (5) | test | dotnet test | succeeds | OperatorsBitwiseTests | [x] |
| 7 | String operators (2) | test | dotnet test | succeeds | OperatorsStringTests | [x] |
| 8 | Ternary operator (1) | test | dotnet test | succeeds | OperatorsTernaryTests | [x] |
| 9 | Unary operators (8) | test | dotnet test | succeeds | OperatorsUnaryTests | [x] |
| 10 | DI registration | test | dotnet test | succeeds | Era.Core.Tests DI category | [x] |
| 11 | Result type usage | code | Grep | gt | 10 | [x] |
| 12 | Legacy equivalence (Pos) | test | dotnet test | succeeds | Era.Core.Tests Equivalence category | [x] |
| 13 | Error handling (Neg) | test | dotnet test | succeeds | Era.Core.Tests ErrorHandling category | [x] |
| 14 | Technical debt zero | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |

### AC Details

**AC#1**: Verify `Era.Core/Expressions/Operators.cs` exists
- Test: `Glob("Era.Core/Expressions/Operators.cs")`

**AC#2**: Verify IOperatorRegistry interface definition
- Test: `Grep("interface IOperatorRegistry", "Era.Core/Expressions/Operators.cs")`

**AC#3-9**: Category-specific unit tests
- Test: `dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~Operators{Category}Tests"`
- Categories: Arithmetic, Comparison, Logical, Bitwise, String, Ternary, Unary
- Each category tests all operators with positive cases
- Use `[Trait("Category", "Operators")]` attribute for category filtering

**AC#10**: DI registration test
- Test: `dotnet test Era.Core.Tests/ --filter "Category=Operators&Subcategory=DI"`
- Verifies `IOperatorRegistry` can be resolved from DI container

**AC#11**: Result type usage verification
- Test: `Grep("Result<", "Era.Core/Expressions/Operators.cs", output_mode: "count")`
- Expected: count > 10 (using `gt` matcher)
- Verification: Confirms Result<T> pattern is used throughout operator implementations

**AC#12**: Legacy equivalence (Positive)
- Test: `dotnet test Era.Core.Tests/ --filter "Category=Operators&Subcategory=Equivalence"`
- Verifies operator behavior matches OperatorMethod.cs for valid inputs

**AC#13**: Error handling (Negative)
- Test: `dotnet test Era.Core.Tests/ --filter "Category=Operators&Subcategory=ErrorHandling"`
- Verifies operators return Result.Fail for:
  - Division by zero
  - Invalid type combinations
  - Overflow conditions

**AC#14**: Technical debt zero
- Test: `Grep("TODO|FIXME|HACK", "Era.Core/Expressions/Operators.cs")` (expect 0 matches)
- Ensures no technical debt comments remain

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create Operators.cs file structure | [x] |
| 2 | 2 | Define IOperatorRegistry and IOperator interfaces | [x] |
| 3 | 3 | Implement Arithmetic operators (5) | [x] |
| 4 | 4 | Implement Comparison operators (6) | [x] |
| 5 | 5 | Implement Logical operators (5) | [x] |
| 6 | 6 | Implement Bitwise operators (5) | [x] |
| 7 | 7 | Implement String operators (2) | [x] |
| 8 | 8 | Implement Ternary operator (1) | [x] |
| 9 | 9 | Implement Unary operators (8) | [x] |
| 10 | 10 | Add DI registration to ServiceCollectionExtensions | [x] |
| 11 | 11 | Ensure all operator methods return Result<object> per Interface Specifications | [x] |
| 12 | 12 | Write legacy equivalence tests (Pos) | [x] |
| 13 | 13 | Write error handling tests (Neg) | [x] |
| 14 | 14 | Verify no TODO/FIXME/HACK comments in implementation | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Design Requirements (Phase 4)

**MUST Follow**: [F377 Design Principles](feature-377.md#design-principles)

| Requirement | Implementation |
|-------------|----------------|
| **No static class** | Use `IOperatorRegistry` interface + DI |
| **Result type** | All operations return `Result<T>` for error handling |
| **Interface abstraction** | Define `IOperator<T>` for operator implementations |
| **DI registration** | Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` |

### Interface Specifications

```csharp
public enum OperatorCategory
{
    Arithmetic, Comparison, Logical, Bitwise, String, Ternary, Unary
}

public interface IOperatorRegistry
{
    Result<object> EvaluateBinary(string symbol, object left, object right);
    Result<object> EvaluateUnary(string symbol, object operand, bool isPrefix);
    Result<object> EvaluateTernary(object condition, object trueValue, object falseValue);
}

public interface IOperator
{
    string Symbol { get; }
    OperatorCategory Category { get; }
}

public interface IBinaryOperator : IOperator
{
    Result<object> Evaluate(object left, object right);
}

public interface IUnaryOperator : IOperator
{
    Result<object> Evaluate(object operand);
}
```

**Design Decision: Result<object>**
The interfaces use `Result<object>` rather than generic `Result<T>` for the following reasons:
1. **Runtime type dispatch**: Operator types (Int64, String) are determined at runtime based on operand types
2. **Complexity avoidance**: Generic methods would require type combinations (IntInt, StrStr, IntStr) in interface signatures
3. **Legacy compatibility**: Original OperatorMethod.cs uses object-based dispatch via dictionaries
4. **Acceptable overhead**: Boxing overhead is minimal for expression evaluation use cases

This is an intentional tradeoff prioritizing implementation simplicity over compile-time type safety.

### Known Legacy Issues

**Note**: During legacy equivalence testing, verify the following potential bugs:
- `GreaterEqualStrStr` (lines 484-498): May have inverted logic (`c < 0` returns 1L instead of `c >= 0`)
- `LessEqualStrStr` (lines 515-529): May have inverted logic (`c < 0` returns 1L instead of `c <= 0`)

Determine correct behavior during implementation and document any intentional deviations from legacy.

### Operator Coverage Reference

**Source**: `engine/Assets/Scripts/Emuera/GameData/Expression/OperatorMethod.cs` (~832 lines)

| Category | Operators | Count |
|----------|-----------|:-----:|
| Arithmetic | `+`, `-`, `*`, `/`, `%` | 5 |
| Comparison | `==`, `!=`, `<`, `>`, `<=`, `>=` (handles both Int64 and String types) | 6 |
| Logical | `&&`, `||`, `^^`, `!&`, `!|` | 5 |
| Bitwise | `&`, `|`, `^`, `<<`, `>>` | 5 |
| String | `+` (concat), `*` (repeat) | 2 |
| Ternary | `? :` (using `#` separator) - 2 type variants (Int64/String return) | 1 |
| Unary | `+`, `-`, `!`, `~`, `++`, `--` (prefix), `++`, `--` (postfix) | 8 |

**Total**: 32 unique operator symbols with type-specific implementations (IntInt, StrStr variants total ~45 method classes in legacy)

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F409 | Phase 8 Planning feature that created F417 |
| Predecessor | F377 | Phase 4 Design Principles (static class禁止, Result型) |
| Related | F416 | ExpressionParser Migration (AST generation, consumes operators) |
| Successor | F418 | Built-in Functions Core (uses operator infrastructure) |
| Successor | F423 | Phase 8 Post-Phase Review (verifies F417 completion) |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 8 definition
- [feature-409.md](feature-409.md) - Phase 8 Planning (predecessor)
- [feature-377.md](feature-377.md) - Phase 4 Design Principles (YAGNI/KISS, Result型)
- [feature-416.md](feature-416.md) - F416 ExpressionParser Migration (related)
- [feature-418.md](feature-418.md) - F418 Built-in Functions Core (successor)
- [feature-423.md](feature-423.md) - F423 Phase 8 Post-Phase Review (successor)
- [feature-template.md](reference/feature-template.md) - Feature specification template

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 | create | implementer | Created F417 per F409 Task 3 requirements | PROPOSED |
| 2026-01-09 | verify | ac-tester | Verified all 14 ACs (File: Operators.cs, Interface: IOperatorRegistry, All operator categories, DI, Result types, Equivalence, ErrorHandling, Zero technical debt) | PASSED |
