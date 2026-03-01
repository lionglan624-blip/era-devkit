# Feature 456: Main Training Commands Part 2 (Com4xx 420-499)

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

Migrate Com4xx main training commands Part 2 (COM_420-499, 10 files).

**Scope**: COMF420-499 main training commands (second half of Com4xx category)

**Output**: `Era.Core/Commands/Com/Com4xx/*.cs` - Main training implementations Part 2 (10 files)

**Total Volume**: 10 COM implementations, ~2,000 lines

---

## Background

### Philosophy (Mid-term Vision)

**Phase 12: COM Implementation** - Migrate all 150+ COMF*.ERB training command implementations to C# using Phase 4 design patterns (Strongly Typed IDs, DI, SRP), ensuring legacy equivalence and establishing single source of truth for game training logic.

### Problem (Current Issue)

F455 migrated Com4xx Part 1 (COM_400-419). Part 2 covers COM_420-499 to complete Com4xx category.

### Goal (What to Achieve)

1. **Migrate Com4xx Part 2** main training commands (COM_420-499, 10 files)
2. **Verify legacy equivalence** for all migrated COMs
3. **Zero technical debt** in migrated code

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Com4xx Part 2 implementation count | file | Glob | count_equals | 10 | [x] |
| 2 | COM unit tests pass | test | Bash | succeeds | "dotnet test --filter Category=Com" | [x] |
| 3 | Zero technical debt | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |

### AC Details

**AC#1**: Com4xx Part 2 implementation count
- Test: Glob pattern="Era.Core/Commands/Com/Com4xx/Com4[2-9][0-9].cs", count == 10
- Verifies exactly 10 Com420-499 implementations
- Pattern allows 420-499 range; only 10 files exist at specific IDs (421, 430, 446, 450-452, 463-465, 490). count_equals 10 verifies exact expected file count

**AC#2**: COM unit tests pass
- Test: Bash command="dotnet test --filter Category=Com"
- All Com implementations match legacy behavior

**AC#3**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Com/Com4xx/"
- Expected: 0 matches
- Note: Checks entire Com4xx directory; Part 1 (F455) already verified tech-debt-free

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Migrate Com4xx Part 2 main training commands (COM_420-499, 10 files) | [x] |
| 2 | 2 | Add COM unit tests for Part 2 (COM_420-499) and verify legacy equivalence | [x] |
| 3 | 3 | Verify zero technical debt | [x] |

<!-- AC:Task 1:1 Rule: 3 ACs = 3 Tasks -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Location**: `Game/ERB/COMF421.ERB` - `COMF490.ERB` (3-digit IDs only)
- Note: COM 42-48 (2-digit IDs) are in Com0xx/ per F452

| ERB File | Lines | Functions | Description |
|----------|:-----:|----------|-------------|
| COMF421.ERB | 13 | COM421 | 身じろぎする (Struggle) |
| COMF430.ERB | 1151 | COM430 | 着替える (Change clothes) |
| COMF446.ERB | 512 | COM446 | 撮影 (Photography) |
| COMF450.ERB | 19 | COM450 | カメラ設置 (Camera setup) |
| COMF451.ERB | 10 | COM451 | カメラ撤去 (Camera removal) |
| COMF452.ERB | 5 | COM452 | 映像解析 (Video analysis) |
| COMF463.ERB | 127 | COM463 | 訪問者を誘う (Invite visitor) |
| COMF464.ERB | 24 | COM464 | 訪問者を案内する (Guide visitor) |
| COMF465.ERB | 23 | COM465 | 訪問者と別行動する (Separate from visitor) |
| COMF490.ERB | 119 | COM490 | 外出する (Go out) |

**Total**: 10 files, 2,003 lines

**Implementation Pattern**: Follow ComBase pattern established in F452

### Test Naming Convention

Test methods follow `TestCom4xx{Number}` format (e.g., `TestCom4xx421`, `TestCom4xx430`), using `[Trait("Category", "Com")]` attribute per F452/F454/F455 convention.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F452 | COM foundation (ComBase, IComRegistry) |
| Predecessor | F455 | Main Training Commands Part 1 |
| Successor | F457 | Additional Training Migration (Com5xx) |

---

## Links

- [feature-452.md](feature-452.md) - COM foundation
- [feature-450.md](feature-450.md) - Phase 12 Planning
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 12 definition

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-12 FL iter2**: [resolved] Volume waiver granted: Com4xx Part 2 batch migration (~2,000 lines) requires atomicity per Phase 12 design, following F452/F453/F454/F455 precedent.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 20:50 | create | feature-builder | Created from F450 Phase 12 Planning | PROPOSED |
| 2026-01-12 09:43 | START | implementer | Task 1 | - |
| 2026-01-12 09:43 | END | implementer | Task 1 | SUCCESS |
