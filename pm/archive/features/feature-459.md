# Feature 459: System Commands Migration (Com888, Com999)

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

Migrate system commands Com888 (Day End) and Com999 (Dummy) to complete Phase 12 COM implementation.

**Scope**: COMF888.ERB, COMF999.ERB (system commands)

**Output**: `Era.Core/Commands/Com/System/*.cs` - System command implementations (2 files: DayEnd.cs, Dummy.cs)

**Total Volume**: 2 COM implementations. DayEnd has external CALL dependencies (stubs until future migration). Dummy is trivial.

---

## Background

### Philosophy (Mid-term Vision)

**Phase 12: COM Implementation** - Migrate all 150+ COMF*.ERB training command implementations to C# using Phase 4 design patterns (Strongly Typed IDs, DI, SRP), ensuring legacy equivalence and establishing single source of truth for game training logic.

### Problem (Current Issue)

F452-F458 completed standard COM categories. Com888 and Com999 special processing require migration to complete Phase 12 COM implementation.

### Goal (What to Achieve)

1. **Migrate Com888 (DayEnd) and Com999 (Dummy)** system commands using F464 semantic naming
2. **Verify legacy equivalence** for migrated COMs
3. **Zero technical debt** in migrated code
4. **Complete Phase 12 COM migration** (final 2 of 150+ COMs)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | System directory exists | file | Glob | exists | Era.Core/Commands/Com/System/ | [x] |
| 2 | DayEnd implementation | file | Glob | exists | Era.Core/Commands/Com/System/DayEnd.cs | [x] |
| 3 | Dummy implementation | file | Glob | exists | Era.Core/Commands/Com/System/Dummy.cs | [x] |
| 4 | COM unit tests pass | test | Bash | succeeds | "dotnet test Era.Core.Tests" | [x] |
| 5 | Zero technical debt | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |

### AC Details

**AC#1**: System directory exists
- Test: Glob pattern="Era.Core/Commands/Com/System/"
- Per F464 directory structure for system commands (COM888, COM999)

**AC#2**: DayEnd implementation (Com888)
- Test: Glob pattern="Era.Core/Commands/Com/System/DayEnd.cs"
- Semantic name for Com888 (一日の終了)
- Uses `[ComId(888)]` attribute per F464

**AC#3**: Dummy implementation (Com999)
- Test: Glob pattern="Era.Core/Commands/Com/System/Dummy.cs"
- Semantic name for Com999 (ダミー)
- Uses `[ComId(999)]` attribute per F464

**AC#4**: COM unit tests pass
- Test: Bash command="dotnet test Era.Core.Tests"
- All Era.Core tests pass (validates COM implementations and related components)
- Expected tests: TestDayEnd (Com888 structure), TestDummy (Com999 returns success)
- Note: [ComId] attribute verification is implicit in build success and ComRegistry lookup

**AC#5**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Com/System/"
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | Migrate DayEnd (Com888) and Dummy (Com999) to System/ directory with semantic names | [x] |
| 2 | 4 | Implement COM unit tests and verify legacy equivalence | [x] |
| 3 | 5 | Verify zero technical debt | [x] |

<!-- AC:Task 1:1 Rule: All 5 ACs = 3 Tasks (AC#1,2,3 same edit) -->

<!-- AC:Task 1:1 Rule: 5 ACs = 3 Tasks (AC#1,2,3 same edit) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Location**: `Game/ERB/COMF888.ERB`, `Game/ERB/COMF999.ERB`

**Implementation Pattern**: Follow F464 semantic naming convention with `[ComId]` attribute

### Semantic Name Mapping

| Legacy | Semantic Name | Description |
|--------|---------------|-------------|
| Com888 | DayEnd | 一日の終了 (Day End Processing) |
| Com999 | Dummy | ダミー (Placeholder command) |

### Test Naming Convention

Test methods follow `Test{SemanticName}` format (e.g., `TestDayEnd`, `TestDummy`).

### DayEnd Implementation Notes

COMF888.ERB contains external CALL dependencies:
- `CHARA_MOVEMENT` - Character movement logic
- `INHABITANT_DO` - Resident interaction logic
- `VISITER_DO` - Visitor action logic
- `EVENTCOMEND2` - End-of-command event processing

**Strategy**: These external calls are placeholder stubs returning 1 (success) to not block execution flow. Actual implementation deferred to when their containing ERB files are migrated.

**SKIPDISP handling**: COMF888.ERB uses `SKIPDISP 1/0` to suppress display during sleep loop. Stub implementation can omit display handling for now (future Phase will add IComContext display suppression flag if needed).

**Core DayEnd scope** (this Feature):
1. Time loop until wake time (`TIME += 30` increments)
2. Position management (temp relocation during sleep, restore based on SELECTCASE)
3. Visitor flag reset (`FLAG:訪問者の現在位置`, `FLAG:訪問者同行フラグ`)
4. `BEGIN AFTERTRAIN` flow transition (transition to AFTERTRAIN game state)

**Dummy (Com999)**: Trivial RETURN 1 stub, no external dependencies.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F452 | COM foundation (ComBase, IComRegistry) |
| Predecessor | F458 | Extended Actions Migration |
| Predecessor | F464 | COM Semantic Naming (use new naming convention) |
| Successor | F460 | Phase 7 Technical Debt Resolution |

---

## Links

- [feature-452.md](feature-452.md) - COM foundation
- [feature-450.md](feature-450.md) - Phase 12 Planning
- [feature-458.md](feature-458.md) - Extended Actions Migration
- [feature-464.md](feature-464.md) - COM Semantic Naming
- [feature-460.md](feature-460.md) - Phase 7 Technical Debt Resolution
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 12 definition

**Deferred Items (tracked for future Phases)**:
- DayEnd external CALLs (CHARA_MOVEMENT, INHABITANT_DO, VISITER_DO, EVENTCOMEND2): Stubs returning 1. Full implementation when containing ERB files are migrated.
- SKIPDISP handling: Out of scope for Phase 12 stub implementations.

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 20:51 | create | feature-builder | Created from F450 Phase 12 Planning | PROPOSED |
| 2026-01-12 14:40 | START | implementer | Task 1 | - |
| 2026-01-12 14:40 | END | implementer | Task 1 | SUCCESS |
| 2026-01-12 14:42 | VERIFY | ac-tester | Verify all 5 ACs | OK:5/5 |
