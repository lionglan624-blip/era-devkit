# Feature 455: Main Training Commands Part 1 (Com4xx 400-419)

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

Migrate Com4xx main training commands Part 1 (COM_400-419, ~16 files).

**Scope**: COMF400-419 main training commands (first half of Com4xx category)

**Output**: `Era.Core/Commands/Com/Com4xx/*.cs` - Main training implementations Part 1 (~16 files)

**Total Volume**: ~16 COM implementations, ~4,389 ERB lines total

---

## Background

### Philosophy (Mid-term Vision)

**Phase 12: COM Implementation** - Migrate all 150+ COMF*.ERB training command implementations to C# using Phase 4 design patterns (Strongly Typed IDs, DI, SRP), ensuring legacy equivalence and establishing single source of truth for game training logic.

### Problem (Current Issue)

F454 completed Com3xx touch/interaction commands. F455 continues with Com4xx main training commands.

Com4xx is the largest category (~40 files). Split into two features for granularity. Part 1 covers COM_400-419.

### Goal (What to Achieve)

1. **Migrate Com4xx Part 1** main training commands (COM_400-419, ~16 files)
2. **Verify legacy equivalence** for all migrated COMs
3. **Zero technical debt** in migrated code

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Com4xx directory exists | file | Glob | exists | Era.Core/Commands/Com/Com4xx/ | [x] |
| 2 | Com4xx Part 1 implementation count | file | Glob | count_equals | 16 | [x] |
| 3 | COM unit tests pass | test | Bash | succeeds | "dotnet test --filter Category=Com" | [x] |
| 4 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [x] |

### AC Details

**AC#1**: Com4xx directory exists
- Test: Glob pattern="Era.Core/Commands/Com/Com4xx/"
- Category subdirectory for main training commands

**AC#2**: Com4xx Part 1 implementation count
- Test: Glob pattern="Era.Core/Commands/Com/Com4xx/Com4[01][0-9].cs", count == 16
- Verifies all 16 Com400-419 implementations exist
- Pattern allows 400-419 (20 possible), but only 16 files exist due to 406-409 gap. count_equals 16 verifies exact expected file count.

**AC#3**: COM unit tests pass
- Test: Bash command="dotnet test --filter Category=Com"
- All COM implementations match legacy behavior (consistent with F452/F454 test trait)

**AC#4**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Com/Com4xx/" output_mode="count"
- Expected: not_contains (0 matches) - checks Com4xx directory for Part 1 files; F456 will run same check for Part 2

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Migrate Com4xx Part 1 main training commands (COM_400-419, ~16 files) | [x] |
| 2 | 3 | Implement COM unit tests and verify legacy equivalence | [x] |
| 3 | 4 | Verify zero technical debt | [x] |

<!-- AC:Task 1:1 Rule: 4 ACs = 3 Tasks (AC#1,2 same edit) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

| ERB File | Lines | Functions | Description |
|----------|:-----:|-----------|-------------|
| COMF400.ERB | 935 | COM400 | 移動 (Movement) |
| COMF401.ERB | 8 | COM401 | コレクション (Collection) |
| COMF402.ERB | 9 | COM402 | 就寝 (Sleep) |
| COMF403.ERB | 48 | COM403 | 休憩 (Rest) |
| COMF404.ERB | 776 | COM404 | 覗く (Peeping) |
| COMF405.ERB | 118 | COM405 | においを嗅ぐ (Smell) |
| COMF410.ERB | 114 | COM410 | 掃除 (Cleaning) |
| COMF411.ERB | 42 | COM411 | 戦闘訓練 (Combat training) |
| COMF412.ERB | 42 | COM412 | 勉強 (Study) |
| COMF413.ERB | 321 | COM413 | 料理を作る (Cooking) |
| COMF414.ERB | 135 | COM414 | 食事 (Eating) |
| COMF415.ERB | 106 | COM415 | 食事をふるまう (Serve food) |
| COMF416.ERB | 7 | COM416 | 自慰する (Masturbation) |
| COMF417.ERB | 1518 | COM417 | 撃破する (Defeat) |
| COMF418.ERB | 161 | COM418 | 鍵を購入する (Buy key) |
| COMF419.ERB | 49 | COM419 | 貞操帯を外してもらう (Remove chastity belt) |

**Legacy Location**: `Game/ERB/COMF400.ERB` - `COMF419.ERB` (16 files, ~4,389 lines total)
- Note: COM 4, 40-48 (1-2 digit IDs) are in Com0xx/ per F452
- Note: COMF406-409 do not exist (gap in COM sequence)

**Implementation Pattern**: Follow ComBase pattern established in F452. Most Com4xx commands are main training implementations. Reference Com100 pattern in F452 Implementation Contract.

```csharp
// Example: Com400.cs (Movement)
namespace Era.Core.Commands.Com.Com4xx;

public class Com400 : ComBase
{
    public override ComId Id => new(400);
    public override string Name => "移動";

    public override Result<ComResult> Execute(IComContext context)
    {
        // Implement movement logic from COMF400.ERB
        // Handle room selection, time passage, NPC interactions
        return Result<ComResult>.Ok(new ComResult { Success = true });
    }
}
```

### Test Naming Convention

Test methods follow `TestCom4xx{Number}` format (e.g., `TestCom4xx400`, `TestCom4xx410`), using `[Trait("Category", "Com")]` attribute per F452/F454 convention.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F452 | COM foundation (ComBase, IComRegistry) |
| Predecessor | F454 | Touch and Interaction Migration (Com3xx) |
| Successor | F456 | Main Training Commands Part 2 (COM_420-499) |

---

## Links

- [feature-452.md](feature-452.md) - COM foundation
- [feature-454.md](feature-454.md) - Touch and Interaction Migration (Predecessor)
- [feature-456.md](feature-456.md) - Main Training Commands Part 2 (Successor)
- [feature-450.md](feature-450.md) - Phase 12 Planning
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 12 definition

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-12 FL iter1**: [resolved] Volume waiver granted: Com4xx Part 1 batch migration requires atomicity per Phase 12 design, following F452/F453/F454 precedent.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 20:50 | create | feature-builder | Created from F450 Phase 12 Planning | PROPOSED |
| 2026-01-12 08:41 | START | implementer | Task 1 | - |
| 2026-01-12 08:56 | END | implementer | Task 1 | SUCCESS |
| 2026-01-12 09:05 | END | ac-tester | Task 2 | SUCCESS (21 tests pass) |
| 2026-01-12 09:06 | END | ac-tester | Task 3 | SUCCESS (0 tech debt) |
