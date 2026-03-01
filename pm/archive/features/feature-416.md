# Feature 416: ExpressionParser Migration

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

**ExpressionParser Migration**: Migrate legacy ExpressionParser.cs from engine to Era.Core with AST generation, replacing procedural parsing with structured Abstract Syntax Tree construction.

**Scope**: AST generation only. Operator and function implementations are separate features (F417-F422).

---

## Background

### Philosophy (Mid-term Vision)

**Expression & Function System**: Establish a clean, testable expression evaluation system in C# that eliminates static dependencies, supports DI, and provides clear AST representation for all ERB expressions.

This feature migrates the parser component, enabling type-safe expression representation that subsequent features (F417-F422) will evaluate.

### Problem (Current Issue)

Legacy ExpressionParser.cs (~626 lines) uses procedural parsing with static methods and global state:
- Static class prevents DI and testing
- No structured AST representation (returns IOperandTerm interface)
- Coupled with expression evaluation logic
- Cannot verify parsing correctness independently

### Goal (What to Achieve)

1. **Migrate ExpressionParser** from engine/Assets/Scripts/Emuera/GameData/Expression/ExpressionParser.cs to Era.Core/Expressions/
2. **Generate structured AST** replacing IOperandTerm with explicit AST node types
3. **Eliminate static methods** with DI-compatible IExpressionParser interface
4. **Separate parsing from evaluation** - parser produces AST only, evaluation is F417-F422 responsibility
5. **Achieve legacy equivalence** - evaluation of AST produces same results as legacy IOperandTerm evaluation (verified via AC#7)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IExpressionParser interface defined | file | Grep | contains | "interface IExpressionParser" | [x] |
| 2 | ExpressionParser implementation exists | file | Glob | exists | Era.Core/Expressions/ExpressionParser.cs | [x] |
| 3 | AST node types defined | file | Grep | contains | "class ExpressionNode" | [x] |
| 4 | DI registration complete | file | Grep | contains | "AddSingleton<IExpressionParser>" | [x] |
| 5 | ArgsEndWith enum migrated | file | Glob | exists | Era.Core/Expressions/ArgsEndWith.cs | [x] |
| 6 | TermEndWith enum migrated | file | Glob | exists | Era.Core/Expressions/TermEndWith.cs | [x] |
| 7 | Legacy static methods removed | file | Grep | not_contains | "static.*ReduceArguments" | [x] |
| 8 | Unit tests for AST generation | test | dotnet test | succeeds | Era.Core.Tests/Expressions/ExpressionParserTests.cs | [x] |
| 9 | Equivalence tests pass | test | dotnet test | succeeds | --filter "Category=ExpressionEquivalence" | [x] |
| 10 | Build succeeds | build | dotnet build | succeeds | - | [x] |
| 11 | No ERB warnings | test | headless | not_contains | "[WARN]" | [x] |
| 12 | Technical debt zero (TODO) | file | Grep | not_contains | "TODO" | [x] |
| 13 | Technical debt zero (FIXME) | file | Grep | not_contains | "FIXME" | [x] |
| 14 | Technical debt zero (HACK) | file | Grep | not_contains | "HACK" | [x] |

### AC Details

**AC#1**: Grep for "interface IExpressionParser" in Era.Core/Expressions/
- Verifies interface follows Phase 4 DI design

**AC#2**: Glob for Era.Core/Expressions/ExpressionParser.cs
- Confirms migration from engine to Era.Core

**AC#3**: Grep for "class ExpressionNode" in Era.Core/Expressions/
- Verifies structured AST node types exist (BinaryNode, UnaryNode, LiteralIntNode, LiteralStringNode, etc.)

**AC#4**: Grep for "AddSingleton<IExpressionParser>" in DependencyInjection/
- Confirms DI registration per Phase 4 requirements

**AC#5**: Glob for Era.Core/Expressions/ArgsEndWith.cs
- Verifies ArgsEndWith enum is migrated from legacy ExpressionParser.cs

**AC#6**: Glob for Era.Core/Expressions/TermEndWith.cs
- Verifies TermEndWith enum is migrated from legacy ExpressionParser.cs

**AC#7**: Grep for "static.*ReduceArguments" with not_contains matcher in Era.Core/Expressions/
- Verifies no legacy static methods remain after migration

**AC#8**: Build succeeds for Era.Core.Tests/Expressions/ExpressionParserTests.cs
- Unit tests verify AST generation correctness

**AC#9**: Test filter for Category=ExpressionEquivalence
- Equivalence tests verify AST semantic equivalence with legacy IOperandTerm
- Uses minimal evaluator implemented within F416 scope for testing

**AC#10**: dotnet build succeeds
- Verifies no compilation errors

**AC#11**: Headless mode produces no [WARN] output
- Verifies no ERB loading warnings

**AC#12-14**: Grep for TODO/FIXME/HACK with not_contains matcher in Era.Core/Expressions/
- Verifies zero technical debt in migrated code
- Scope: Era.Core/Expressions/*.cs only

**Test Command (AC#9)**:
```bash
cd C:/Era/era紅魔館protoNTR
dotnet test Era.Core.Tests --filter "Category=ExpressionEquivalence"
```

**Expected**: All equivalence tests pass, confirming AST represents same semantics as legacy parser.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Define IExpressionParser interface with Parse() method | [x] |
| 2 | 3 | Define AST node types (ExpressionNode hierarchy) | [x] |
| 3 | 2 | Migrate ExpressionParser.cs from engine to Era.Core/Expressions/ | [x] |
| 4 | 2 | Refactor ReduceArguments to instance method returning AST | [x] |
| 5 | 2 | Refactor ReduceExpressionTerm to instance method returning AST | [x] |
| 6 | 4 | Register IExpressionParser in DI container | [x] |
| 7 | 5 | Migrate ArgsEndWith enum to Era.Core/Expressions/ | [x] |
| 8 | 6 | Migrate TermEndWith enum to Era.Core/Expressions/ | [x] |
| 9 | 12,13,14 | Remove all TODO/FIXME/HACK comments from migrated code | [x] |
| 10 | 8 | Write ExpressionParserTests.cs with AST generation unit tests | [x] |
| 11 | 9 | Write ExpressionEquivalenceTests.cs with minimal evaluator implementation | [x] |
| 12 | 10 | Verify dotnet build succeeds | [x] |
| 13 | 11 | Verify headless mode has no ERB warnings | [x] |
| 14 | 7 | Verify no legacy static methods remain | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Migration Strategy

**Source**: `engine/Assets/Scripts/Emuera/GameData/Expression/ExpressionParser.cs` (~626 lines)

**Destination**: `Era.Core/Expressions/ExpressionParser.cs`

**Key Changes**:
1. **Static → Instance**: Convert static class to DI-compatible instance class
2. **IOperandTerm → AST**: Replace interface returns with concrete AST node types
3. **Separation**: Parser produces AST only, does not evaluate expressions
4. **Error Handling**: Use Result<T> for parse failures per Phase 4 design

### AST Node Structure

```csharp
// Base node
public abstract record ExpressionNode;

// Literals (type-safe, no object boxing)
public record LiteralIntNode(long Value) : ExpressionNode;
public record LiteralStringNode(string Value) : ExpressionNode;

// Binary operations
public record BinaryNode(ExpressionNode Left, string Operator, ExpressionNode Right) : ExpressionNode;

// Unary operations
public record UnaryNode(string Operator, ExpressionNode Operand) : ExpressionNode;

// Function calls
public record FunctionCallNode(string Name, ImmutableArray<ExpressionNode> Arguments) : ExpressionNode;

// Variable references
public record VariableNode(string Name, ImmutableArray<ExpressionNode>? Indices) : ExpressionNode;

// Ternary operator
public record TernaryNode(ExpressionNode Condition, ExpressionNode TrueExpr, ExpressionNode FalseExpr) : ExpressionNode;
```

### Interface Definition

```csharp
public interface IExpressionParser
{
    Result<ExpressionNode> Parse(string expression);
    Result<ImmutableArray<ExpressionNode>> ParseArguments(string arguments, ArgsEndWith endWith);
}

public class ExpressionParser : IExpressionParser
{
    public Result<ExpressionNode> Parse(string expression) { ... }
    public Result<ImmutableArray<ExpressionNode>> ParseArguments(string arguments, ArgsEndWith endWith) { ... }
}
```

**Notes**:
- Migrate `ArgsEndWith` enum from legacy ExpressionParser.cs to `Era.Core/Expressions/ArgsEndWith.cs`
- Migrate `TermEndWith` enum from legacy ExpressionParser.cs to `Era.Core/Expressions/TermEndWith.cs`
- `ImmutableArray<T>` is .NET BCL standard (System.Collections.Immutable included in .NET 8)
- `Parse(string expression)` handles internal tokenization (no external WordCollection dependency)
- `string Operator` in AST nodes is intentional - F417's IOperatorRegistry uses string-based operator lookup
- `OperatorCode` enum migration is F417 scope (Operator Implementation)

### Equivalence Testing Strategy

**Purpose**: Verify AST semantic equivalence with legacy IOperandTerm

**Approach**: Implement minimal evaluator within F416 scope for testing. This evaluator supports basic operations needed for equivalence verification without duplicating F417's full operator implementation.

**Method**:
1. Parse same expression with both legacy and new parser
2. Evaluate both using F416's minimal evaluator (supports basic arithmetic, comparison, string operations)
3. Compare results for 100+ test cases covering:
   - All operator types (arithmetic, comparison, logical, bitwise, string, ternary)
   - Function calls with various argument counts
   - Variable references with/without indices
   - Nested expressions
   - Edge cases (empty strings, division by zero conditions, etc.)

**Test Data Sources**:
- Existing ERB expressions from Game/ERB/
- Synthetic expressions covering operator precedence
- Error cases (malformed expressions)

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F409 | Phase 8 Planning feature |
| Related | F377 | Phase 4 Design Principles (DI, Result<T>, Strongly Typed IDs) |
| Successor | F417 | Operator Implementation (evaluates AST nodes) |
| Successor | F418 | Built-in Functions Core (evaluates FunctionCallNode) |
| Related | Era.Core.Variables | VariableCode type reference (LiteralIntNode/LiteralStringNode AST) |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 8 definition
- [feature-409.md](feature-409.md) - Phase 8 Planning (parent feature)
- [feature-377.md](feature-377.md) - Phase 4 Design Principles
- [feature-417.md](feature-417.md) - Operator Implementation (successor)
- [feature-418.md](feature-418.md) - Built-in Functions Core (successor)
- [feature-422.md](feature-422.md) - Type Conversion & Casting (successor)
- [ExpressionParser.cs](../../engine/Assets/Scripts/Emuera/GameData/Expression/ExpressionParser.cs) - Legacy source

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 | create | implementer | Created as Phase 8 sub-feature per F409 Task 2 | PROPOSED |
| 2026-01-09 19:06 | START | implementer | Task 1-14 | - |
| 2026-01-09 19:06 | END | implementer | Task 1-14 | SUCCESS |
| 2026-01-09 20:34 | AC VERIFY | ac-tester | Verify all 14 ACs | OK:14/14 |
