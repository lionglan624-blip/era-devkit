# Feature 457: Additional Training Migration (Com5xx)

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

## Created: 2026-01-11

---

## Summary

Migrate Com5xx additional training command (1 file: COMF511.ERB).

**Scope**: COMF511 (COM511: 二本フェラ)

**Output**: `Era.Core/Commands/Com/Com5xx/Com511.cs`

**Total Volume**: 1 COM implementation, ~265 lines

---

## Background

### Philosophy (Mid-term Vision)

**Phase 12: COM Implementation** - Migrate all 150+ COMF*.ERB training command implementations to C# using Phase 4 design patterns (Strongly Typed IDs, DI, SRP), ensuring legacy equivalence and establishing single source of truth for game training logic.

### Problem (Current Issue)

F455-F456 completed main training commands. Com5xx additional training commands require migration.

**Note**: COM 5 (1-digit ID) is already migrated as Com05.cs in Com0xx/ per F452. This feature covers only 3-digit COM IDs (5xx range).

### Goal (What to Achieve)

1. **Migrate Com5xx** additional training command (1 file: COMF511.ERB)
2. **Verify legacy equivalence** for COM511
3. **Zero technical debt** in migrated code

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Com5xx directory exists | file | Glob | exists | Era.Core/Commands/Com/Com5xx/ | [x] |
| 2 | Com5xx implementation count | file | Glob | count_equals | 1 | [x] |
| 3 | COM unit tests pass | test | Bash | succeeds | "dotnet test --filter Category=Com" | [x] |
| 4 | Zero technical debt | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |

### AC Details

**AC#1**: Com5xx directory exists
- Test: Glob pattern="Era.Core/Commands/Com/Com5xx/"
- Category subdirectory for additional training

**AC#2**: Com5xx implementation count
- Test: Glob pattern="Era.Core/Commands/Com/Com5xx/*.cs", count == 1
- Verifies exactly 1 Com5xx implementation (Com511.cs)

**AC#3**: COM unit tests pass
- Test: Bash command="dotnet test --filter Category=Com"
- All COM implementations match legacy behavior (consistent with F452/F456)

**AC#4**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Com/Com5xx/"
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Migrate Com5xx additional training command (COMF511.ERB) | [x] |
| 2 | 3 | Implement COM511 unit test and verify legacy equivalence | [x] |
| 3 | 4 | Verify zero technical debt | [x] |

<!-- AC:Task 1:1 Rule: 4 ACs = 3 Tasks (AC#1,2 share Task#1 as same file creation) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Location**: `Game/ERB/COMF511.ERB` (COM511: 二本フェラ, ~265 lines)

| ERB File | Lines | Functions | Description |
|----------|:-----:|-----------|-------------|
| COMF511.ERB | 265 | COM511 | 二本フェラ (Double fellatio) |

**Note**: COMF511.ERB is the only file in Com5xx range (500-599).

**Implementation Pattern**: Follow ComBase pattern established in F452

### Test Naming Convention

Test methods follow `TestCom5xx{Number}` format (e.g., `TestCom5xx511`).

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F452 | COM foundation (ComBase, IComRegistry) |
| Predecessor | F456 | Main Training Commands Part 2 |
| Successor | F458 | Extended Actions Migration (Com6xx) |

---

## Links

- [feature-450.md](feature-450.md) - Phase 12 Planning
- [feature-452.md](feature-452.md) - COM foundation
- [feature-455.md](feature-455.md) - Main Training Commands Part 1
- [feature-456.md](feature-456.md) - Main Training Commands Part 2
- [feature-458.md](feature-458.md) - Extended Actions Migration (Successor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 12 definition

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-12 FL iter1**: [applied] Scope corrected from ~10 files to 1 file (only COMF511.ERB exists in Com5xx range)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 20:51 | create | feature-builder | Created from F450 Phase 12 Planning | PROPOSED |
| 2026-01-12 10:41 | START | implementer | Task 1 | - |
| 2026-01-12 10:41 | END | implementer | Task 1 | SUCCESS |
| 2026-01-12 11:25 | START | implementer | Task 2 | - |
| 2026-01-12 11:25 | END | implementer | Task 2 | SUCCESS |
| 2026-01-12 11:26 | START | finalizer | Task 3 | - |
| 2026-01-12 11:26 | END | finalizer | Task 3 | SUCCESS |
| 2026-01-12 11:26 | FINALIZE | finalizer | Mark [DONE] | SUCCESS |
