# Feature 823: Room & Stain System Migration

## Status: [DRAFT]

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

## Type: erb

## Background

### Philosophy (Mid-term Vision)

Phase 22: State Systems -- Room smell and stain subsystem migration covering room odor tracking and stain state management (ROOM_SMELL.ERB, STAIN.ERB).

### Problem (Current Issue)

Two ERB files totalling 1,440 lines implement room and stain subsystems: ROOM_SMELL.ERB (1140 lines) and STAIN.ERB (300 lines). NTR functions internally SET room smell state; no external NTR CALL is required, keeping this subsystem independent.

### Goal (What to Achieve)

Migrate ROOM_SMELL.ERB and STAIN.ERB to C# implementing IStainLoader and related room/stain interfaces. Achieve zero-debt implementation with equivalence tests against ERB baseline.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | (stub - to be completed by /fc) | | | | | [ ] |
| 2 | No TODO/FIXME/HACK comments remain in Room & Stain implementation (zero debt) | file | Grep | matches | No matches | [ ] |
| 3 | CP-2 E2E checkpoint: Room & Stain integration verified | test | dotnet test | pass | All pass | [ ] |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | - | Remove TODO/FIXME/HACK comments from migrated code | | [ ] |
| 2 | - | Write equivalence tests against ERB baseline | | [ ] |

---

## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning
