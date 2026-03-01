# Feature 512: Primary Constructor Migration - Commands/Special Directory

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

Migrate 16 SCOMF (Special COM Form) command handler files in `Era.Core/Commands/Special/` directory from traditional constructor pattern to C# 14 Primary Constructor pattern.

**Scope**: Era.Core/Commands/Special/ directory only (Scomf1Handler.cs through Scomf16Handler.cs)

**Pattern**: Convert `private readonly` field declarations with constructor assignment to primary constructor parameter initialization.

**Expected Reduction**: ~80 lines (5 lines per file: field declaration, empty line, constructor signature, null check assignment, closing brace)

---

## Background

### Philosophy (Mid-term Vision)

Phase 16: C# 14 Style Migration - Apply C# 14 patterns to existing code for simplification. Primary Constructor and Collection Expression reduce ~400 lines of boilerplate.

### Problem (Current Issue)

Special command handlers in `Era.Core/Commands/Special/` use traditional constructor pattern with boilerplate:
- Explicit `private readonly` field declarations
- Constructor parameter → field assignment
- Null check in constructor body

This pattern adds ~5 lines per handler (16 files = ~80 lines of boilerplate).

### Goal (What to Achieve)

1. Migrate all 16 Scomf{N}Handler.cs files to primary constructor pattern
2. Eliminate field declarations and constructor bodies
3. Maintain identical behavior (refactoring only, no functional changes)
4. Verify all tests PASS after migration

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Scomf1Handler.cs migrated | code | Grep | contains | "Scomf1Handler.*ISpecialTraining specialTraining" | [x] |
| 2 | Scomf2Handler.cs migrated | code | Grep | contains | "Scomf2Handler.*ISpecialTraining specialTraining" | [x] |
| 3 | Scomf3Handler.cs migrated | code | Grep | contains | "Scomf3Handler.*ISpecialTraining specialTraining" | [x] |
| 4 | Scomf4Handler.cs migrated | code | Grep | contains | "Scomf4Handler.*ISpecialTraining specialTraining" | [x] |
| 5 | Scomf5Handler.cs migrated | code | Grep | contains | "Scomf5Handler.*ISpecialTraining specialTraining" | [x] |
| 6 | Scomf6Handler.cs migrated | code | Grep | contains | "Scomf6Handler.*ISpecialTraining specialTraining" | [x] |
| 7 | Scomf7Handler.cs migrated | code | Grep | contains | "Scomf7Handler.*ISpecialTraining specialTraining" | [x] |
| 8 | Scomf8Handler.cs migrated | code | Grep | contains | "Scomf8Handler.*ISpecialTraining specialTraining" | [x] |
| 9 | Scomf9Handler.cs migrated | code | Grep | contains | "Scomf9Handler.*ISpecialTraining specialTraining" | [x] |
| 10 | Scomf10Handler.cs migrated | code | Grep | contains | "Scomf10Handler.*ISpecialTraining specialTraining" | [x] |
| 11 | Scomf11Handler.cs migrated | code | Grep | contains | "Scomf11Handler.*ISpecialTraining specialTraining" | [x] |
| 12 | Scomf12Handler.cs migrated | code | Grep | contains | "Scomf12Handler.*ISpecialTraining specialTraining" | [x] |
| 13 | Scomf13Handler.cs migrated | code | Grep | contains | "Scomf13Handler.*ISpecialTraining specialTraining" | [x] |
| 14 | Scomf14Handler.cs migrated | code | Grep | contains | "Scomf14Handler.*ISpecialTraining specialTraining" | [x] |
| 15 | Scomf15Handler.cs migrated | code | Grep | contains | "Scomf15Handler.*ISpecialTraining specialTraining" | [x] |
| 16 | Scomf16Handler.cs migrated | code | Grep | contains | "Scomf16Handler.*ISpecialTraining specialTraining" | [x] |
| 17 | No private readonly in Special/ | code | Grep | not_contains | "private readonly" | [x] |
| 18 | No technical debt | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 19 | Tests PASS | test | Bash | succeeds | "dotnet test" | [x] |

### AC Details

**AC#1-16**: Primary constructor migration pattern
- Test: Grep pattern="Scomf{N}Handler.*ISpecialTraining specialTraining" path="Era.Core/Commands/Special/"
- Expected: Class declaration with primary constructor parameter (C# 14 syntax)
- Verifies constructor parameter-to-field pattern replaced with primary constructor

**AC#17**: No private readonly fields remain
- Test: Grep pattern="private readonly" path="Era.Core/Commands/Special/"
- Expected: 0 matches (all handlers use primary constructor)

**AC#18**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Special/"
- Expected: 0 matches across all handler files

**AC#19**: Test PASS verification
- Test: `dotnet test`
- Expected: All tests PASS (no functional changes, refactoring only)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Migrate Scomf1Handler.cs to primary constructor | [x] |
| 2 | 2 | Migrate Scomf2Handler.cs to primary constructor | [x] |
| 3 | 3 | Migrate Scomf3Handler.cs to primary constructor | [x] |
| 4 | 4 | Migrate Scomf4Handler.cs to primary constructor | [x] |
| 5 | 5 | Migrate Scomf5Handler.cs to primary constructor | [x] |
| 6 | 6 | Migrate Scomf6Handler.cs to primary constructor | [x] |
| 7 | 7 | Migrate Scomf7Handler.cs to primary constructor | [x] |
| 8 | 8 | Migrate Scomf8Handler.cs to primary constructor | [x] |
| 9 | 9 | Migrate Scomf9Handler.cs to primary constructor | [x] |
| 10 | 10 | Migrate Scomf10Handler.cs to primary constructor | [x] |
| 11 | 11 | Migrate Scomf11Handler.cs to primary constructor | [x] |
| 12 | 12 | Migrate Scomf12Handler.cs to primary constructor | [x] |
| 13 | 13 | Migrate Scomf13Handler.cs to primary constructor | [x] |
| 14 | 14 | Migrate Scomf14Handler.cs to primary constructor | [x] |
| 15 | 15 | Migrate Scomf15Handler.cs to primary constructor | [x] |
| 16 | 16 | Migrate Scomf16Handler.cs to primary constructor | [x] |
| 17 | 17,18,19 | Verify migration completion, zero debt, test PASS | [x] |

<!-- AC:Task 1:1 Rule: 19 ACs = 17 Tasks (16 migration tasks + 1 batch verification task per F384 precedent) -->

<!-- **Batch verification waiver (Task 17)**: Following F384 precedent for verification tasks (AC#17-19). -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Migration Pattern

**Before** (Traditional Constructor):
```csharp
public class Scomf1Handler : ICommandHandler<Scomf1Command, TrainingResult>
{
    private readonly ISpecialTraining _specialTraining;

    public Scomf1Handler(ISpecialTraining specialTraining)
    {
        _specialTraining = specialTraining ?? throw new ArgumentNullException(nameof(specialTraining));
    }

    public Task<Result<TrainingResult>> Handle(Scomf1Command command, CancellationToken ct)
    {
        return Task.FromResult(Result<TrainingResult>.Fail("NotImplemented: SCOMF1"));
    }
}
```

**After** (Primary Constructor):
```csharp
public class Scomf1Handler(ISpecialTraining specialTraining) : ICommandHandler<Scomf1Command, TrainingResult>
{
    public Task<Result<TrainingResult>> Handle(Scomf1Command command, CancellationToken ct)
    {
        return Task.FromResult(Result<TrainingResult>.Fail("NotImplemented: SCOMF1"));
    }
}
```

**Changes**:
1. Move constructor parameter to class declaration: `class Scomf1Handler(ISpecialTraining specialTraining)`
2. Remove `private readonly` field declaration
3. Remove explicit constructor body
4. Update field references from `_specialTraining` to `specialTraining` (primary constructor parameter is accessible throughout class). Note: Current stub implementations do not reference `_specialTraining` in Handle method body. This step is included for completeness in case future implementations use the field.

**Null Check**: Primary constructor parameters do NOT include automatic null checks. Since current handlers use ArgumentNullException, this migration removes null checking. This is acceptable for DI-injected dependencies (DI container ensures non-null).

---

## Source Migration Reference

**Legacy Pattern**: Current implementation uses traditional constructor pattern (established in Phase 9)

**Target Files**: 16 handler files in `Era.Core/Commands/Special/`
- Scomf1Handler.cs through Scomf16Handler.cs
- All follow identical pattern with `ISpecialTraining` dependency

**File Verification**: Glob `Era.Core/Commands/Special/Scomf*Handler.cs` confirms 16 handler files. ScomfCommands.cs (record definitions) is excluded from migration scope.

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

No deferred tasks identified at PROPOSED stage.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F503 | Phase 16 Planning feature defines migration scope |
| Related | F509 | Primary Constructor Migration - Training/ directory |
| Related | F510 | Primary Constructor Migration - Character/ directory |
| Related | F511 | Primary Constructor Migration - Commands/Flow/ directory |
| Related | F513 | Primary Constructor Migration - Commands/System/ + Other |
| Successor | F515 | Phase 16 Post-Phase Review (verifies all migrations) |

---

## Links

- [feature-503.md](feature-503.md) - Phase 16 Planning
- [feature-509.md](feature-509.md) - Primary Constructor Migration - Training/ directory
- [feature-510.md](feature-510.md) - Primary Constructor Migration - Character/ directory
- [feature-511.md](feature-511.md) - Primary Constructor Migration - Commands/Flow/ directory
- [feature-513.md](feature-513.md) - Primary Constructor Migration - Commands/System/ + Other
- [feature-515.md](feature-515.md) - Phase 16 Post-Phase Review
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 16 definition
- [csharp-14 SKILL](../../.claude/skills/csharp-14/SKILL.md) - Primary Constructor reference

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-16 | create | feature-builder | Created from F503 Phase 16 Planning | PROPOSED |
| 2026-01-16 15:12 | START | implementer | Tasks 1-16: Batch migration | - |
| 2026-01-16 15:12 | END | implementer | Tasks 1-16: Batch migration | SUCCESS |
| 2026-01-16 | AC verify | ac-tester | AC#1-16: Grep primary constructor pattern | OK:16/16 |
| 2026-01-16 | AC verify | ac-tester | AC#17: Grep "private readonly" → 0 matches | OK:AC17 |
| 2026-01-16 | AC verify | ac-tester | AC#18: Grep "TODO\|FIXME\|HACK" → 0 matches | OK:AC18 |
| 2026-01-16 | AC verify | ac-tester | AC#19: dotnet test Era.Core.Tests → 1167/1167 PASS | OK:AC19 |
