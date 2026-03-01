# Deferred Task Protocol

> **Purpose**: Rules for deferring tasks to prevent documentation leaks.
>
> **Referenced by**: CLAUDE.md (pointer), fl-workflow/POST-LOOP.md, feature-quality/INFRA.md

---

## Destination Selection

**When deferring a task, choose ONE concrete destination with actionable Task:**

| Option | Destination | Requirement | When to use |
|:------:|-------------|-------------|-------------|
| A | Create new Feature | Task to create F{ID} in current Feature | Independent issue |
| B | Add to existing Feature | Task to add content OR direct Edit | Related Feature exists **AND is NOT [DONE]** |
| C | Add to architecture.md Phase | Phase exists **AND Phase Status ≠ WIP/Done** AND **not self-referencing** (see Option C Guards) | Phase-level planning (pre-feature-creation only) |

**THEN record in Handoff section AND add corresponding Task (for A/B).**

**Rationale**: Handoff without actionable Task = documented but never executed = leak.

---

## TBD Prohibition

**"TBD" is NEVER allowed. Handoff without execution means is also TBD.**

Every deferred task MUST have:
1. **Concrete destination** (Feature ID, Task ID, or Phase number)
2. **Actionable means** (Task in Tasks table for A/B, Phase existence for C)

| Option | Destination | Validation |
|:------:|-------------|------------|
| A | `F{ID}` (new) | **DRAFT file exists** OR Creation Task exists in Tasks table. `/fc {ID}` → `/fl` → `/run` is executed by the user. Claude's responsibility ends at DRAFT creation |
| B | `F{ID}#T{N}` (existing) | Referenced Feature exists **AND Status is NOT [DONE]**. Adding tasks to [DONE] Features is forbidden |
| C | `Phase N` | Phase exists in architecture.md **AND passes Option C Guards** |

---

## Option C Guards

**Problem**: Option C ("add to Phase tasks") is only actionable when the Phase has not yet been broken down into features. Once features are created (Phase Status = WIP), Phase tasks are "consumed" — no agent reads the Phase doc to pick up new items.

### Guard 1: Phase Status Gate

```
Phase Status in architecture.md:
  "Not Started" / "Planning" → Option C allowed
  "WIP"  → BLOCKED: "Phase {N} already has active features.
            Phase tasks are consumed. Use Option A (new Feature)
            or Option B (add to existing non-DONE feature in this Phase)."
  "Done" → BLOCKED: "Phase {N} completed. Use Option A."
```

### Guard 2: Self-Referencing Phase Detection

```
1. Resolve current feature's Phase:
   - Check Dependencies table for Predecessor with "Phase {N} Planning"
   - OR check Philosophy section for Phase reference
   - OR trace through architecture.md Phase → Feature mapping

2. IF handoff destination Phase == current feature's Phase:
   BLOCKED: "Self-referencing: Feature belongs to Phase {N}.
             Cannot hand off to own Phase. Use Option A."
```

**Rationale**: A Phase 20 feature handing off to Phase 20 is circular — the Phase's features are already defined. The handoff would be orphaned.

### Guard Summary

| Check | Condition | Result |
|-------|-----------|--------|
| Phase exists | Phase NOT in architecture.md | → FAIL (existing rule) |
| Phase Status | Phase Status == WIP or Done | → FAIL: use Option A or B |
| Self-reference | Handoff Phase == Feature's own Phase | → FAIL: use Option A |
| All pass | Phase exists, not WIP/Done, not self-referencing | → Option C allowed |

---

## Forbidden Patterns

- `F{ID}` without creation Task → Forbidden (documented but not actionable)
- `F{ID} (TBD)` → Forbidden
- `Destination: Undetermined` → Forbidden
- `Decide later` → Forbidden
- `F{ID}` where Status = `[DONE]` → Forbidden (tasks added to completed Features will never be executed)

**Enforcement**: FL PHASE-7 validates Task existence. /run AC validates file creation.
