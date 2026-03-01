# Feature 453: Special Actions Migration (Com2xx)

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

Migrate Com2xx clothing removal commands (4 files).

**Scope**: COMF200-203 clothing removal commands (脱衣系: 上半身脱衣, 下半身脱衣, ブラ脱衣, パンツ脱衣)

**Output**: `Era.Core/Commands/Com/Com2xx/*.cs` - Clothing removal implementations (4 files)

**Total Volume**: 4 COM implementations, estimated 400-600 lines

---

## Background

### Philosophy (Mid-term Vision)

**Phase 12: COM Implementation** - Migrate all 150+ COMF*.ERB training command implementations to C# using Phase 4 design patterns (Strongly Typed IDs, DI, SRP), ensuring legacy equivalence and establishing single source of truth for game training logic.

### Problem (Current Issue)

F452 establishes COM foundation (Com0xx/Com1xx). Com2xx special actions require migration.

### Goal (What to Achieve)

1. **Migrate Com2xx** clothing removal commands (4 files)
2. **Verify legacy equivalence** for all migrated COMs
3. **Zero technical debt** in migrated code

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Com2xx directory exists | file | Glob | exists | Era.Core/Commands/Com/Com2xx/ | [x] |
| 2 | Com2xx implementation count | file | Glob | count_gte | 4 | [x] |
| 3 | COM unit tests pass | test | Bash | succeeds | "dotnet test --filter Category=Com" | [x] |
| 4 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [x] |

### AC Details

**AC#1**: Com2xx directory exists
- Test: Glob pattern="Era.Core/Commands/Com/Com2xx/"
- Category subdirectory for special actions

**AC#2**: Com2xx implementation count
- Test: Glob pattern="Era.Core/Commands/Com/Com2xx/*.cs", count >= 4
- Verifies all 4 Com2xx implementations (COMF200-203)

**AC#3**: COM unit tests pass
- Test: Bash command="dotnet test --filter Category=Com"
- All Com2xx implementations match legacy behavior
- Uses F452 pattern: `[Trait("Category", "Com")]` with TestClothingRemovalCom{Number} naming
- Note: PALAM coefficient differences (x10 for COM200/201, x5 for COM202/203) verified implicitly via legacy equivalence testing per F452 precedent

**AC#4**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Com/Com2xx/"
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Migrate Com2xx clothing removal commands (4 files) | [x] |
| 2 | 3 | Implement COM unit tests and verify legacy equivalence | [x] |
| 3 | 4 | Verify zero technical debt | [x] |

<!-- AC:Task 1:1 Rule: 4 ACs = 3 Tasks (AC#1,2 same edit) -->
<!-- **Batch verification waiver (Task 1)**: Com2xx implementations are 4 related clothing removal commands created atomically. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

| ERB File | Lines | Functions | Description |
|----------|:-----:|-----------|-------------|
| COMF200.ERB | 116 | COM200, CAN_COM200 | 上半身脱衣 (Upper body undress) |
| COMF201.ERB | 115 | COM201, CAN_COM201 | 下半身脱衣 (Lower body undress) |
| COMF202.ERB | 112 | COM202, CAN_COM202 | ブラ脱衣 (Bra removal) |
| COMF203.ERB | 115 | COM203, CAN_COM203 | パンツ脱衣 (Panties removal) |

**Legacy Location**: `Game/ERB/COMF200.ERB, COMF201.ERB, COMF202.ERB, COMF203.ERB`

**Implementation Pattern**: Follow ComBase pattern established in F452. Com2xx commands are simple ComBase implementations (not EquipmentComBase). Reference Com100 pattern in F452 Implementation Contract.

```csharp
// Example: Com200.cs (Upper body undress)
// Note: COM200/201 use PALAM:欲情 x10, COM202/203 use x5
public class Com200 : ComBase
{
    public override ComId Id => new(200);
    public override string Name => "上半身脱衣";

    public override Result<ComResult> Execute(IComContext context)
    {
        // Implement CAN_COM200 validation logic:
        // - Check execution value threshold (>= 40)
        // - Calculate from ABL:露出癖, MARK:快楽刻印, PALAM:欲情 (x10 for COM200)
        // - Apply TALENT:羞恥心, 恋慕, 親愛 modifiers
        // - Check 貞操帯の鍵 (FLAG:貞操帯鍵購入フラグ == TARGET)
        // Actual execution delegated to EVENTCOMEND per ERB comment
        return Result<ComResult>.Ok(new ComResult { Success = true });
    }
}
```

**PALAM Coefficient Note**: COM200/201 (upper/lower body) use PALAM:欲情 x10 multiplier, while COM202/203 (bra/panties) use x5.

### Test Naming Convention

Test methods follow `TestClothingRemovalCom{Number}` format (e.g., `TestClothingRemovalCom200`, `TestClothingRemovalCom201`), using `[Trait("Category", "Com")]` attribute per F452 convention.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F452 | COM foundation (ComBase, IComRegistry) |
| Successor | F454 | Conversation Systems Migration (Com3xx) |

---

## Links

- [feature-452.md](feature-452.md) - COM foundation
- [feature-450.md](feature-450.md) - Phase 12 Planning
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 12 definition

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-12 FL iter2**: [resolved] Phase2-Validate - Test naming convention: Use TestClothingRemovalCom{Number} following domain semantics (what the COM does), matching F452 pattern (TestCaressingCom100 = domain category)
- **2026-01-12 FL iter2**: [resolved] Phase2-Validate - Positive/negative test AC: F452 precedent does not have explicit pos/neg AC. Defer to Phase 12 convention (implicit in "COM unit tests pass")
- **2026-01-12 FL iter2**: [resolved] Phase2-Validate - Code snippets: Reference F452 Com100 pattern (simple ComBase implementation)
- **2026-01-12 FL iter3**: [resolved] Volume waiver: Com2xx follows F452 batch migration pattern. 4 related clothing removal commands require atomic implementation per Phase 12 design (~400-600 lines exceeds ~300 limit)
- **2026-01-12 FL iter4**: [resolved] Phase2-Validate - AC#3 equivalence criteria: F452 precedent applies for Phase 12 consistency. COM tests use Category filter without specific input/output pairs in AC. ENGINE.md Issue 6 guidance applies to new patterns, not established precedents.
- **2026-01-12 FL iter4**: [resolved] Phase3-Maintainability - Task 1 CAN_COM mention: CAN_COM inclusion is implicit per F452 pattern. Migration includes both COM and CAN_COM functions per ERB file structure, as documented in Implementation Contract Source Migration Reference table.
- **2026-01-12 FL iter6**: [resolved] Phase3-Maintainability - Code snippet structure: Code snippet comment mentions 'CAN_COM200 validation logic' within Execute() method. ERB separates COM/CAN_COM but C# consolidates into Execute(). This is architectural choice per F452 precedent (all logic within Execute() without separate CanExecute()).
- **2026-01-12 FL iter7**: [resolved] Phase2-Validate - AC#4 Details format: Current format (pattern in Expected column, path in AC Details) matches ENGINE.md Issue 15 requirements. AC Details explicitly states path="Era.Core/Commands/Com/Com2xx/" and Expected="0 matches".

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 20:50 | create | feature-builder | Created from F450 Phase 12 Planning | PROPOSED |
| 2026-01-12 06:24 | START | implementer | Task 1 | - |
| 2026-01-12 06:24 | END | implementer | Task 1 | SUCCESS |
| 2026-01-12 06:27 | START | implementer | Task 2 | - |
| 2026-01-12 06:28 | END | implementer | Task 2 | SUCCESS |
| 2026-01-12 06:35 | DEVIATION | Opus | Phase 3 TDD | Opus wrote tests directly (do.md lacked dispatch instruction) |
| 2026-01-12 06:35 | DEVIATION | Opus | Phase 3 TDD | --no-build test run, required build then retry |
| 2026-01-12 06:40 | END | Opus | do.md fix | Added Phase 3 dispatch + Step 8.0.5 verification |
