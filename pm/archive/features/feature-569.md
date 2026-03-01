# Feature 569: Advanced Formula Expressions

## Status: [CANCELLED]

### Cancellation Reason
F565 (COM YAML Runtime Integration) already implemented comprehensive formula parsing including recursive descent parser (ParseExpression, ParseTerm, ParseFactor), Math.Max/Min, getPalamLv integration, and parentheses support. F569's Problem statement claiming "placeholder logic" was outdated - written before F565 implementation. The feature is now redundant. Future formula capabilities (Math.Abs/Sqrt/Pow, named variables, conditional expressions) should be planned as new Features when concrete use cases emerge.

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
COM YAML system should support sophisticated mathematical expressions for effect calculations. Advanced formula expressions enable complex game mechanics through data-driven mathematical operations while maintaining type safety and performance through proper parser implementation.

### Problem (Current Issue)
F565 implements basic formula evaluation with placeholder logic that always returns baseValue unchanged. Complex mathematical expressions beyond basic arithmetic require dedicated parser for proper evaluation of nested operations, variable substitution, and mathematical functions.

### Goal (What to Achieve)
Implement comprehensive formula parser supporting nested mathematical expressions, variable references, and built-in functions for YAML COM effect calculations.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Advanced formula parser implementation | file | Grep "Era.Core/Effects/SourceScaleEffectHandler.cs" | contains | "ParseNestedExpression" | [ ] |
| 2 | Nested expression support | file | Grep "Era.Core/Effects/SourceScaleEffectHandler.cs" | contains | "ParseExpressionRecursive" | [ ] |
| 3 | Variable substitution | file | Grep "Era.Core/Effects/SourceScaleEffectHandler.cs" | contains | "SubstituteVariable" | [ ] |
| 4 | Mathematical functions | file | Grep "Era.Core/Effects/SourceScaleEffectHandler.cs" | contains | "Math\\." | [ ] |
| 5 | Build succeeds | build | dotnet build | succeeds | - | [ ] |
| 6 | All tests pass | test | dotnet test | succeeds | - | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-4 | Enhance SourceScaleEffectHandler with advanced formula parsing capabilities | [ ] |
| 2 | 5-6 | Verify build and test success | [ ] |

---

## Dependencies

| Type | Feature | Relationship | Notes |
|------|---------|--------------|-------|
| Predecessor | F565 | Successor | COM YAML Runtime Integration |

---

## Links

- [index-features.md](index-features.md)
- [feature-565.md](feature-565.md) - COM YAML Runtime Integration

---

## Review Notes

- [resolved] All pending issues resolved by cancellation decision (2026-01-21)
- **Root Cause**: Planning Sequencing Error - F569 was planned before F565 implementation, and documentation wasn't updated after F565 completed comprehensive formula parsing
- **Decision**: CANCELLED per user approval - F565 already achieves F569's goals

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

---