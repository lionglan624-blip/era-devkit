# Feature 454: Touch and Interaction Migration (Com3xx)

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

Migrate Com3xx touch and interaction commands (17 files) + COM 3 (1 file in Com0xx).

**Scope**: COMF301-363 touch and interaction commands (caressing, intimate touching, position changes, social interactions)

**Output**:
- `Era.Core/Commands/Com/Com3xx/*.cs` - Touch and interaction implementations (17 files: 301-363)
- `Era.Core/Commands/Com/Com0xx/Com03.cs` - COM 3 指挿入れ (COM 3 ≠ COM 300)

**Total Volume**: 18 COM implementations, estimated 1,500-2,000 lines

---

## Background

### Philosophy (Mid-term Vision)

**Phase 12: COM Implementation** - Migrate all 150+ COMF*.ERB training command implementations to C# using Phase 4 design patterns (Strongly Typed IDs, DI, SRP), ensuring legacy equivalence and establishing single source of truth for game training logic.

### Problem (Current Issue)

F452-F453 established foundation and special actions. Com3xx touch and interaction commands require migration.

### Goal (What to Achieve)

1. **Migrate Com3xx** touch and interaction commands (18 files)
2. **Verify legacy equivalence** for all migrated COMs
3. **Zero technical debt** in migrated code

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Com3xx directory exists | file | Glob | exists | Era.Core/Commands/Com/Com3xx/ | [x] |
| 2 | Com3xx implementation count | file | Glob | count_gte | 17 | [x] |
| 3 | COM unit tests pass | test | Bash | succeeds | "dotnet test --filter Category=Com" | [x] |
| 4 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [x] |

### AC Details

**AC#1**: Com3xx directory exists
- Test: Glob pattern="Era.Core/Commands/Com/Com3xx/"
- Category subdirectory for touch and interaction commands

**AC#2**: Com3xx implementation count
- Test: Glob pattern="Era.Core/Commands/Com/Com3xx/*.cs", count >= 17
- Verifies 17 Com3xx implementations (COMF301-302, COMF310-316, COMF350-355, COMF360, COMF363)
- Note: COM 3 (COMF3.ERB) is in Com0xx/Com03.cs (COM ID 3 ≠ COM ID 300)

**AC#3**: COM unit tests pass
- Test: Bash command="dotnet test --filter Category=Com"
- All Com3xx implementations match legacy behavior
- Uses F452/F453 pattern: `[Trait("Category", "Com")]` with TestCom3xx{Number} naming

**AC#4**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Com/Com3xx/"
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Migrate Com3xx touch and interaction commands (18 files) | [x] |
| 2 | 3 | Implement COM unit tests and verify legacy equivalence | [x] |
| 3 | 4 | Verify zero technical debt | [x] |

<!-- AC:Task 1:1 Rule: 4 ACs = 3 Tasks (AC#1,2 same edit) -->
<!-- **Batch verification waiver (Task 1)**: Com3xx implementations are 18 related touch/interaction commands created atomically. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

| ERB File | Lines | Functions | Description |
|----------|:-----:|-----------|-------------|
| COMF3.ERB | 70 | COM3, CAN_COM3 | 指挿入れ (Finger insertion - caress) |
| COMF301.ERB | 98 | COM301 | お茶を淹れる (Make tea) |
| COMF302.ERB | 107 | COM302 | スキンシップ (Skinship) |
| COMF310.ERB | 118 | COM310 | 尻を撫でる (Buttock caressing) |
| COMF311.ERB | 104 | COM311 | 抱き付く (Hugging) |
| COMF312.ERB | 150 | COM312 | キスする (Kissing) |
| COMF313.ERB | 57 | COM313 | 胸愛撫 (Breast caressing) |
| COMF314.ERB | 99 | COM314 | アナル愛撫 (Anal caressing) |
| COMF315.ERB | 81 | COM315 | クリ愛撫 (Clitoris caressing) |
| COMF316.ERB | 92 | COM316 | 指挿入れ (Finger insertion) |
| COMF350.ERB | 125 | COM350 | 押し倒す (Push down) |
| COMF351.ERB | 174 | COM351 | 連れ出す (Take out) |
| COMF352.ERB | 50 | COM352 | 告白する (Confess) |
| COMF353.ERB | 5 | COM353 | 許しを乞う (Beg forgiveness) |
| COMF354.ERB | 67 | COM354 | 邪魔しない (Don't interfere) |
| COMF355.ERB | 5 | COM355 | なすがまま (Let it be) |
| COMF360.ERB | 5 | COM360 | なすがまま (Let it be) |
| COMF363.ERB | 13 | COM363 | 身じろぎする (Squirm) |

**Legacy Location**: `Game/ERB/COMF3*.ERB` (18 files, ~1,420 lines total)

**Implementation Pattern**: Follow ComBase pattern established in F452. Most Com3xx commands are simple ComBase implementations. Reference Com100 pattern in F452 Implementation Contract.

```csharp
// Example: Com310.cs (Buttock caressing)
namespace Era.Core.Commands.Com.Com3xx;

public class Com310 : ComBase
{
    public override ComId Id => new(310);
    public override string Name => "尻を撫でる";

    public override Result<ComResult> Execute(IComContext context)
    {
        // Check COM417_is_Protected for 娼婦紋_左尻
        // Implement caress logic with affection/sensation calculations
        return Result<ComResult>.Ok(new ComResult { Success = true });
    }
}
```

### Test Naming Convention

Test methods follow `TestCom3xx{Number}` format (e.g., `TestCom3xx310`, `TestCom3xx350`), using `[Trait("Category", "Com")]` attribute per F452/F453 convention.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F452 | COM foundation (ComBase, IComRegistry) |
| Predecessor | F453 | Special Actions Migration |
| Successor | F455 | Main Training Commands Part 1 |

---

## Links

- [feature-452.md](feature-452.md) - COM foundation
- [feature-453.md](feature-453.md) - Special Actions Migration
- [feature-450.md](feature-450.md) - Phase 12 Planning
- [feature-455.md](feature-455.md) - Main Training Commands Part 1 (Successor)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 12 definition

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-12 FL iter1**: [applied] Phase2-Validate - Title renamed: 'Conversation Systems Migration' → 'Touch and Interaction Migration' per user decision.
- **2026-01-12 FL iter1**: [resolved] Volume waiver granted: Com3xx batch migration requires atomicity per Phase 12 design, following F452/F453 precedent. 18 related touch/interaction commands (~1,500-2,000 lines exceeds ~300 limit).
- **2026-01-12 FL iter2**: [resolved] Phase4-ACValidation - AC#3 quoting: Changed to unescaped 'Category=Com' for F453 consistency.
- **2026-01-12 FL iter2**: [applied] Phase2-Validate - Test naming: Changed to TestCom3xx{Number} per user decision.
- **2026-01-12 FL iter2**: [resolved] AC#4 pattern: Changed 'TODO\\|FIXME\\|HACK' to 'TODO|FIXME|HACK' for consistency with F453 precedent.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 20:50 | create | feature-builder | Created from F450 Phase 12 Planning | PROPOSED |
| 2026-01-12 | START | /do | Initialize Feature 454 | WIP |
| 2026-01-12 | END | implementer | Task 1: Migrate Com3xx (17 files) + Com0xx/Com03.cs | SUCCESS |
| 2026-01-12 | DEVIATION | opus | COM 3 misplaced in Com3xx (Windows reserved name) | FIX APPLIED |
| 2026-01-12 | END | opus | Moved COM 3 to Com0xx/Com03.cs (COM 3 ≠ COM 300) | SUCCESS |
| 2026-01-12 | END | ac-tester | Task 2: COM unit tests (62 passed) | SUCCESS |
| 2026-01-12 | END | opus | Task 3: Zero technical debt (0 matches) | SUCCESS |
| 2026-01-12 | END | ac-tester | AC verification (4/4 passed) | SUCCESS |
