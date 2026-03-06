# Feature 514: Collection Expression Migration

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

## Created: 2026-01-16

---

## Summary

Migrate 18 occurrences of `new List<T>()` and `new List<T> { ... }` to C# 14 Collection Expression syntax (`[]` and `[...]`) across 4 files in Era.Core. This simplification removes boilerplate initialization code while maintaining identical runtime behavior.

**Migration Pattern**:
- `new List<T>()` → `[]`
- `new List<T> { ... }` → `[...]`
- `var list = new List<T>()` → `List<T> list = []`

**Target Files and Counts**:
- Era.Core/Common/InfoEquip.cs: 6 occurrences
- Era.Core/Expressions/ExpressionParser.cs: 5 occurrences
- Era.Core/Training/MarkSystem.cs: 6 occurrences
- Era.Core/Functions/ArrayFunctions.cs: 1 occurrence

**Scope**: Code simplification only. No functional changes, no new features. Files listed here that also require Primary Constructor migration are handled by F509-F513 (both migrations may be applied to the same file independently).

---

## Background

### Philosophy (Mid-term Vision)

**Phase 16: C# 14 Style Migration** - Establish Collection Expression (`[]`) as the standard list initialization pattern across Era.Core, replacing verbose `new List<T>()` syntax. This migration creates a single recognizable initialization pattern that reduces cognitive load during code review, improves codebase consistency, and aligns with modern C# idioms.

### Problem (Current Issue)

Era.Core codebase uses legacy C# 12 collection initialization syntax (`new List<T>()`). C# 14 Collection Expressions provide a more concise syntax (`[]`) that reduces visual noise and improves code clarity without changing runtime behavior.

### Goal (What to Achieve)

1. Replace all 18 `new List<T>()` instances with Collection Expression syntax
2. Verify all tests pass after migration (no functional changes)
3. Ensure zero technical debt markers introduced during refactoring
4. Document migration completion for Phase 16 scope

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | InfoEquip.cs migrated | code | Grep | not_contains | "new List<" | [x] |
| 2 | ExpressionParser.cs migrated | code | Grep | not_contains | "new List<" | [x] |
| 3 | MarkSystem.cs migrated | code | Grep | not_contains | "new List<" | [x] |
| 4 | ArrayFunctions.cs migrated | code | Grep | not_contains | "new List<" | [x] |
| 5 | InfoEquip.cs uses Collection Expression | code | Grep | contains | "List<.*>=.*\\[" | [x] |
| 6 | ExpressionParser.cs uses Collection Expression | code | Grep | contains | "List<.*>=.*\\[" | [x] |
| 7 | MarkSystem.cs uses Collection Expression | code | Grep | contains | "List<.*>=.*\\[" | [x] |
| 8 | ArrayFunctions.cs uses Collection Expression | code | Grep | contains | "List<.*>=.*\\[" | [x] |
| 9 | No tech debt introduced | code | Grep | not_contains | "TODO|FIXME|HACK" | [x] |
| 10 | All tests pass | test | Bash | succeeds | - | [x] |

### AC Details

**AC#1-4**: Legacy List initialization removed
- Test: Grep pattern="new List<" path="Era.Core/{file}"
- Expected: 0 matches (all instances migrated)

**AC#5-8**: Collection Expression syntax adopted
- Test: Grep pattern="List<.*>=.*\\[" path="Era.Core/{file}"
- Expected: Contains pattern (verifies new syntax, e.g., `List<string> parts = []`)
- Note: Pattern matches `[]` and `[...]` variants on single-line variable declarations. MarkSystem.cs: 4 constructor argument instances (lines 33-34, 41-42) migrate to `[]` within new expressions and won't match this variable declaration pattern; its 2 variable declarations (lines 45-46) will match.

**AC#9**: No technical debt introduced
- Paths:
  - Era.Core/Common/InfoEquip.cs
  - Era.Core/Expressions/ExpressionParser.cs
  - Era.Core/Training/MarkSystem.cs
  - Era.Core/Functions/ArrayFunctions.cs
- Test: Grep pattern="TODO|FIXME|HACK" for each file listed above
- Expected: 0 matches in all 4 files (defensive check - verifies migration does not introduce debt markers)

**AC#10**: All tests pass after migration
- Test: `dotnet test` at repository root
- Expected: All tests pass (no functional changes)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,5 | Migrate InfoEquip.cs (6 instances) | [x] |
| 2 | 2,6 | Migrate ExpressionParser.cs (5 instances) | [x] |
| 3 | 3,7 | Migrate MarkSystem.cs (6 instances) | [x] |
| 4 | 4,8 | Migrate ArrayFunctions.cs (1 instance) | [x] |
| 5 | 9,10 | Run full test suite, verify PASS, and confirm zero technical debt | [x] |

<!-- AC:Task 1:1 Rule: 10 ACs = 5 Tasks (Tasks 1-4 batch 2 ACs each per .claude/skills/feature-quality/ENGINE.md Issue 7) -->

<!-- **Batch verification waiver (Tasks 1-4)**: Following F384 precedent for related file migration verification. Each task handles both removal (AC#1-4) and adoption (AC#5-8) verification atomically per file. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Migration Source Reference

**Legacy Pattern**: `new List<T>()` and `new List<T> { ... }`

**Target Pattern**: C# 14 Collection Expression

| File | Line Ranges | Instance Count |
|------|----------------------|:--------------:|
| Era.Core/Common/InfoEquip.cs | 49, 103, 106, 196, 295, 330 | 6 |
| Era.Core/Expressions/ExpressionParser.cs | 44, 69, 115, 471, 504 | 5 |
| Era.Core/Training/MarkSystem.cs | 33, 34, 41, 42, 45, 46 | 6 |
| Era.Core/Functions/ArrayFunctions.cs | 277 | 1 |

**MarkSystem.cs Note**: Lines 33-34 and 41-42 are constructor arguments (inside `new MarkAcquisitionResult(...)` calls), migrating `new List<T>()` → `[]` directly. Lines 45-46 are variable declarations requiring explicit type: `var x = new List<T>()` → `List<T> x = []`.

### C# 14 Pattern Reference

See `.claude/skills/csharp-14/SKILL.md` for Collection Expression syntax details.

**Conversion Examples**:

```csharp
// Before (C# 12 style)
var parts = new List<string>();
List<Regex> regexes = new List<Regex>();
var bodyParts = new List<(string label, int index, bool shouldDisplay)>
{
    ("口：", 0, true),
    ("手：", 1, true)
};

// After (C# 14 Collection Expression)
List<string> parts = [];
List<Regex> regexes = [];
List<(string label, int index, bool shouldDisplay)> bodyParts =
[
    ("口：", 0, true),
    ("手：", 1, true)
];
```

**Type Inference Requirement**: Collection Expression requires explicit type declaration on the left-hand side. Change `var` to explicit `List<T>` when migrating.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F503 | Phase 16 Planning defines migration scope |
| Related | F509-F513 | Primary Constructor migration features (parallel execution) |
| Successor | F515 | Post-Phase Review Phase 16 verifies completion |

---

## Links

- [feature-503.md](feature-503.md) - Phase 16 Planning
- [feature-509.md](feature-509.md) - Primary Constructor Migration - Training Directory
- [feature-510.md](feature-510.md) - Primary Constructor Migration - Character Directory
- [feature-511.md](feature-511.md) - Primary Constructor Migration - Commands/Flow Directory
- [feature-512.md](feature-512.md) - Primary Constructor Migration - Commands/Special Directory
- [feature-513.md](feature-513.md) - Primary Constructor Migration - Commands/System + Other
- [feature-515.md](feature-515.md) - Post-Phase Review Phase 16
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 16 definition (Task 7)
- [.claude/skills/csharp-14/SKILL.md](../../.claude/skills/csharp-14/SKILL.md) - C# 14 syntax reference
- [.claude/skills/feature-quality/ENGINE.md](../../.claude/skills/feature-quality/ENGINE.md) - Engine type quality guide

---

## 引継ぎ先指定 (Mandatory Handoffs)

No deferred tasks identified at PROPOSED stage.

---

## Review Notes
<!-- Optional: Add review feedback here. /fl command shows this section. -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-16 | create | feature-builder | Created from F503 Phase 16 Planning | PROPOSED |
| 2026-01-16 15:26 | START | implementer | Tasks 1-5 | - |
| 2026-01-16 15:26 | END | implementer | Tasks 1-5 | SUCCESS |
