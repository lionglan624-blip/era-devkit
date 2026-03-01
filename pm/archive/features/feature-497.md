# Feature 497: Naming Convention Audit

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

## Type: infra

## Created: 2026-01-14

---

## Summary

Audit Era.Core naming conventions for consistency across Phase 1-12 implementations.

**Audit Scope**:
- Interface naming (IFoo vs IFooService)
- Class naming (Foo vs FooImpl vs FooService)
- Method naming (PascalCase consistency)
- Strongly Typed ID naming (FooId conventions)
- COM class naming (Com{N} vs Com{N}{Feature})
- Generic type parameter naming (T, TEntity, etc.) if applicable
- Record naming (if any records exist)
- Extension method naming (if any extension methods exist)

**Excluded from Scope** (with rationale):
- Namespace naming: Follows folder structure per F496
- File naming: Covered by F496 Folder Structure Validation

**Conditional Audit Items** (included in report only if present in codebase):
- Generic type parameter naming (T, TEntity, etc.)
- Record naming
- Extension method naming
- Test class naming (documented if encountered during audit, no separate AC)

**Output**: Naming convention audit report with standardization recommendations.

---

## Background

### Philosophy (Mid-term Vision)

**Phase 15: Architecture Review** - Validate Phase 1-14 implementations against Phase 4 design principles (SRP, DIP, Strongly Typed IDs, Result type, DI registration) before large-scale parallel implementation phases (Phase 19-21). Ensures architectural consistency and identifies technical debt requiring resolution.

### Problem (Current Issue)

Naming conventions evolved across Phase 1-12 without systematic consistency enforcement:
- Mixed interface naming patterns (ICharacterStore vs IOperatorRegistry vs IFunctionRegistry)
- Inconsistent implementation class naming
- Strongly Typed IDs follow conventions but not formally documented
- COM naming conventions need verification

### Goal (What to Achieve)

1. **Document current naming patterns** across codebase
2. **Identify inconsistencies** in naming conventions
3. **Define standard conventions** for Phase 16+ implementations
4. **Recommend renames** if inconsistencies found (or ratify current conventions)
5. **Track refactoring needs** for F501 if changes required

### Impact Analysis

| Component | Impact | Description |
|-----------|--------|-------------|
| Game/agents/designs/naming-conventions-15.md | Create | New audit report file |
| F501 | Input | May receive rename tasks if inconsistencies found |
| Phase 16+ | Reference | Defines naming standards for future implementations |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Audit report exists | file | Glob | exists | "Game/agents/designs/naming-conventions-15.md" | [x] |
| 2 | Interface naming audited | file | Grep(naming-conventions-15.md) | contains | "Interface Naming Patterns:" | [x] |
| 3 | Class naming audited | file | Grep(naming-conventions-15.md) | contains | "Class Naming Patterns:" | [x] |
| 4 | Method naming audited | file | Grep(naming-conventions-15.md) | contains | "Method Naming Conventions:" | [x] |
| 5 | Strongly Typed ID naming | file | Grep(naming-conventions-15.md) | contains | "Strongly Typed ID Conventions:" | [x] |
| 6 | COM naming audited | file | Grep(naming-conventions-15.md) | contains | "COM Class Naming:" | [x] |
| 7 | Inconsistencies documented | file | Grep(naming-conventions-15.md) | contains | "Naming Inconsistencies:" | [x] |
| 8 | Standard conventions defined | file | Grep(naming-conventions-15.md) | contains | "Standard Conventions:" | [x] |
| 9 | 負債ゼロ | file | Grep(naming-conventions-15.md) | not_contains | "TODO\\|FIXME\\|HACK" | [x] |

### AC Details

**AC#1**: Audit report exists
- Test: Glob pattern="Game/agents/designs/naming-conventions-15.md"
- Expected: File exists

**AC#2**: Interface naming audited
- Test: Grep("Interface Naming Patterns:", "Game/agents/designs/naming-conventions-15.md")
- Expected: Lists interface naming patterns found (IFoo, IFooService, IFooRegistry, etc.)

**AC#3**: Class naming audited
- Test: Grep("Class Naming Patterns:", "Game/agents/designs/naming-conventions-15.md")
- Expected: Lists implementation class patterns (Foo, FooImpl, FooService, etc.)

**AC#4**: Method naming audited
- Test: Grep("Method Naming Conventions:", "Game/agents/designs/naming-conventions-15.md")
- Expected: Verifies PascalCase consistency, identifies deviations

**AC#5**: Strongly Typed ID naming audited
- Test: Grep("Strongly Typed ID Conventions:", "Game/agents/designs/naming-conventions-15.md")
- Expected: Documents CharacterId, MaxBaseIndex, etc. naming patterns

**AC#6**: COM naming audited
- Test: Grep("COM Class Naming:", "Game/agents/designs/naming-conventions-15.md")
- Expected: Verifies COM class naming (Com{N}, base class suffixes)

**AC#7**: Inconsistencies documented
- Test: Grep("Naming Inconsistencies:", "Game/agents/designs/naming-conventions-15.md")
- Expected: Lists inconsistencies with severity

**AC#8**: Standard conventions defined
- Test: Grep("Standard Conventions:", "Game/agents/designs/naming-conventions-15.md")
- Expected: Defines recommended naming standards for Phase 16+

**AC#9**: Zero technical debt in audit documentation
- Test: Grep("TODO|FIXME|HACK", "Game/agents/designs/naming-conventions-15.md")
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create naming-conventions-15.md audit report | [x] |
| 2 | 2,3,4,5,6 | Audit naming patterns across Era.Core | [x] |
| 3 | 7,8 | Document inconsistencies and define standard conventions | [x] |
| 4 | 9 | Verify zero technical debt in audit documentation (負債解消) | [x] |

<!-- AC:Task 1:1 Rule: 9 ACs = 4 Tasks (batch waiver for Task 2: single audit pass produces multiple related sections) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Audit Report Structure

`Game/agents/designs/naming-conventions-15.md` must include:

```markdown
# Naming Convention Audit Phase 15

## Interface Naming Patterns:
| Pattern | Examples | Count | Consistency |
|---------|----------|-------|-------------|
| IFoo | ICharacterStore, IMaxBaseStore | N | ✓/✗ |
| IFooService | ... | N | ✓/✗ |
| IFooRegistry | IOperatorRegistry, IFunctionRegistry | N | ✓/✗ |

## Class Naming Patterns:
| Pattern | Examples | Count | Consistency |
|---------|----------|-------|-------------|
| Foo | CharacterStore, MaxBaseStore | N | ✓/✗ |
| FooImpl | ... | N | ✓/✗ |
| FooService | ... | N | ✓/✗ |

## Method Naming Conventions:
- **PascalCase**: [✓ Consistent / ✗ Inconsistencies Found]
- **Verb prefixes**: Get/Set/Add/Remove/Create/Delete
- **Deviations**: (list any camelCase or other deviations)

## Strongly Typed ID Conventions:
| Type | Naming | Consistency |
|------|--------|-------------|
| Character IDs | CharacterId | ✓/✗ |
| Index types | MaxBaseIndex, TalentIndex | ✓/✗ |
| Enum-based IDs | ... | ✓/✗ |

## COM Class Naming:
| Pattern | Examples | Count | Consistency |
|---------|----------|-------|-------------|
| Com{N} | Com311, Com312 | N | ✓/✗ |
| Com{N}Base | OrgasmComBase, EquipmentComBase | N | ✓/✗ |

## Generic Type Parameter Conventions:
(Include if applicable)
| Convention | Examples | Consistency |
|------------|----------|-------------|
| Single T | T | ✓/✗ |
| Descriptive | TEntity, TResult | ✓/✗ |

## Record Naming:
(Include if applicable)
| Pattern | Examples | Count | Consistency |
|---------|----------|-------|-------------|
| ... | ... | N | ✓/✗ |

## Extension Method Naming:
(Include if applicable)
| Pattern | Examples | Count | Consistency |
|---------|----------|-------|-------------|
| ... | ... | N | ✓/✗ |

## Naming Inconsistencies:
| Location | Current Name | Inconsistency | Recommendation |
|----------|--------------|---------------|----------------|
| ... | ... | ... | Rename to ... / Accept as-is |

## Standard Conventions:
(Define recommended standards for Phase 16+)

- **Interfaces**: IFoo (no Service/Registry suffix unless ambiguous)
- **Classes**: Foo (no Impl suffix)
- **Methods**: PascalCase, verb prefixes
- **Strongly Typed IDs**: FooId pattern
- **COM Classes**: Com{N} with base class suffix

## 負債の意図的受け入れ:
(Document any naming debt accepted with justification)
```

### Audit Method

1. **Grep interface definitions**: `public interface I*` across Era.Core
2. **Grep class definitions**: `public class *` across Era.Core
3. **Analyze naming patterns**: Group by suffix, prefix, conventions
4. **Identify inconsistencies**: Compare against Phase 4 expectations
5. **Define standards**: Based on majority pattern or best practice

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F495 | Code Review Phase 9-12 must complete first |
| Sibling | F496 | Folder Structure Validation (parallel review) |
| Sibling | F498 | Testability Assessment (parallel review) |
| Successor | F499 | Test Strategy Design (after all reviews complete) |
| Related | F501 | Architecture Refactoring (implements renames if recommended) |

---

## Links

- [feature-486.md](feature-486.md) - Phase 15 Planning (parent feature)
- [feature-495.md](feature-495.md) - Code Review Phase 9-12 (predecessor)
- [feature-496.md](feature-496.md) - Folder Structure Validation (sibling)
- [feature-498.md](feature-498.md) - Testability Assessment (sibling)
- [feature-499.md](feature-499.md) - Test Strategy Design (successor)
- [feature-501.md](feature-501.md) - Architecture Refactoring (related)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 15 naming review requirements

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-15 FL iter2**: [resolved] Phase2-Validate - Impact Analysis: Validated as complete. F499 is Type=engine (IRandomProvider), has no data dependency on F497 (naming audit). Successor indicates execution order, not data flow.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-14 | create | spec-writer | Created from F486 Phase 15 Planning | PROPOSED |
| 2026-01-15 06:58 | START | implementer | Task 1-4 | - |
| 2026-01-15 06:58 | END | implementer | Task 1-4 | SUCCESS |
| 2026-01-15 06:59 | START | ac-tester | AC 1-9 verification | - |
| 2026-01-15 06:59 | END | ac-tester | AC 1-9 verification | PASS 9/9 |
| 2026-01-15 07:05 | END | finalizer | Feature 497 | [DONE] |
