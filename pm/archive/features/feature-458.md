# Feature 458: Masturbation Commands Migration (COM600-699)

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

Migrate Masturbation commands (17 files) using F464 semantic naming.

**Scope**: COMF600-699 masturbation commands (self-play actions)

**Output**: `Era.Core/Commands/Com/Masturbation/*.cs` - Semantically named implementations (17 files)

**Total Volume**: 17 COM implementations, estimated 1,700-2,550 lines

**Volume Waiver**: Granted per F452 precedent for atomic category migration

---

## Background

### Philosophy (Mid-term Vision)

**Phase 12: COM Implementation** - Migrate all 150+ COMF*.ERB training command implementations to C# using Phase 4 design patterns (Strongly Typed IDs, DI, SRP), ensuring legacy equivalence and establishing single source of truth for game training logic.

### Problem (Current Issue)

F457 completed additional training commands. Com6xx extended actions require migration.

### Goal (What to Achieve)

1. **Migrate Masturbation commands** (17 files) using F464 semantic naming
2. **Verify legacy equivalence** for all migrated COMs
3. **Zero technical debt** in migrated code

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Masturbation directory exists | file | Glob | exists | Era.Core/Commands/Com/Masturbation/ | [x] |
| 2 | Masturbation implementation count | file | Glob | count_equals | 17 | [x] |
| 3 | COM unit tests pass | test | Bash | succeeds | "dotnet test --filter Category=Com" | [x] |
| 4 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [x] |

### AC Details

**AC#1**: Masturbation directory exists
- Test: Glob pattern="Era.Core/Commands/Com/Masturbation/"
- Category directory for self-play commands per F464 semantic naming

**AC#2**: Masturbation implementation count
- Test: Glob pattern="Era.Core/Commands/Com/Masturbation/*.cs", count == 17
- Verifies all 17 Masturbation implementations exist

**AC#3**: COM unit tests pass
- Test: Bash command="dotnet test --filter Category=Com"
- All Masturbation implementations match legacy behavior
- Uses `[Trait("Category", "Com")]` attribute per F452-F457 precedent

**AC#4**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Com/Masturbation/"
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Migrate Masturbation commands (17 files) using F464 semantic naming | [x] |
| 2 | 3 | Implement COM unit tests and verify legacy equivalence | [x] |
| 3 | 4 | Verify zero technical debt | [x] |

<!-- AC:Task 1:1 Rule: 4 ACs = 3 Tasks (AC#1,2 same edit) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Location**: COMF600.ERB, COMF602-603.ERB, COMF605-609.ERB, COMF640-648.ERB (17 files total)
- Files: 600, 602, 603, 605, 606, 607, 608, 609, 640, 641, 642, 643, 644, 645, 646, 647, 648
- Note: COM 6, 60-69 (1-2 digit IDs) are in Training/ per F464

**Implementation Pattern**: Follow ComBase pattern established in F452 with F464 semantic naming

### Semantic Naming Mapping (per F464)

| COM ID | Japanese | Semantic Name | File |
|--------|----------|---------------|------|
| 600 | 愛撫 | SelfCaress | SelfCaress.cs |
| 602 | セルフフェラ | SelfFellatio | SelfFellatio.cs |
| 603 | 指挿れ | SelfFingerInsertion | SelfFingerInsertion.cs |
| 605 | アナル愛撫 | SelfAnalCaress | SelfAnalCaress.cs |
| 606 | 胸愛撫 | SelfBreastCaress | SelfBreastCaress.cs |
| 607 | 乳首責め | SelfNippleTorture | SelfNippleTorture.cs |
| 608 | 秘貝開帳 | SelfClitExposure | SelfClitExposure.cs |
| 609 | 手淫 | SelfMasturbation | SelfMasturbation.cs |
| 640 | ローター | SelfRotor | SelfRotor.cs |
| 641 | Eマッサージャ | SelfMassager | SelfMassager.cs |
| 642 | クリキャップ | SelfClitoralCap | SelfClitoralCap.cs |
| 643 | オナホール | SelfOnahole | SelfOnahole.cs |
| 644 | バイブ | SelfVibrator | SelfVibrator.cs |
| 645 | アナルバイブ | SelfAnalVibrator | SelfAnalVibrator.cs |
| 646 | アナルビーズ | SelfAnalBeads | SelfAnalBeads.cs |
| 647 | ニプルキャップ | SelfNippleCap | SelfNippleCap.cs |
| 648 | 搾乳機 | SelfMilkingMachine | SelfMilkingMachine.cs |

### Test Naming Convention

Test methods follow `Test{SemanticName}` format (e.g., `TestSelfCaress`, `TestSelfVibrator`) per F464 convention.
- This naming convention is for code organization and readability
- AC#3 verification uses `[Trait("Category", "Com")]` attribute filter per F452-F457 precedent

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F452 | COM foundation (ComBase, IComRegistry) |
| Predecessor | F457 | Additional Training Migration |
| Predecessor | F464 | COM Semantic Naming (use new naming convention) |
| Successor | F459 | Special Processing Migration (Com888, Com999) |

---

## Links

- [feature-452.md](feature-452.md) - COM foundation
- [feature-457.md](feature-457.md) - Additional Training Migration
- [feature-459.md](feature-459.md) - Special Processing Migration
- [feature-464.md](feature-464.md) - COM Semantic Naming
- [feature-450.md](feature-450.md) - Phase 12 Planning
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 12 definition

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->

- **2026-01-12 FL iter1**: Volume waiver granted: COM batch migration follows F452 precedent for atomic category migration (17 files, ~1,700-2,550 lines)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 20:51 | create | feature-builder | Created from F450 Phase 12 Planning | PROPOSED |
| 2026-01-12 12:49 | START | implementer | Task 1 | - |
| 2026-01-12 12:49 | END | implementer | Task 1 | SUCCESS |
| 2026-01-12 12:52 | START | ac-tester | Task 2 (unit tests) | - |
| 2026-01-12 12:52 | END | ac-tester | Task 2 | SUCCESS (161 tests pass) |
| 2026-01-12 12:52 | START | ac-tester | Task 3 (tech debt) | - |
| 2026-01-12 12:52 | END | ac-tester | Task 3 | SUCCESS (0 matches) |
