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
| B | Add to existing Feature | Task to add content OR direct Edit | Passes ALL Option B Guards (Semantic Routing Gates) |
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
| B | `F{ID}#T{N}` (existing) | Passes ALL Option B Guards. Adding to [DONE]/[WIP]/[BLOCKED]/[REVIEWED] Features is forbidden |
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

## Option B Guards (Semantic Routing)

**Problem**: Option B ("add to existing Feature") was the most common misrouting path.
Obligations were sent to features that exist but lack the scope, status, or execution
path to act on them. This resulted in obligations being "confirmed" but never executed.

**Principle**: A (new Feature) is the default. B is the exception requiring proof.

### Gate 1: Status Gate
| Status | Allowed? | Reason |
|--------|:--------:|--------|
| [DRAFT] | Yes | /fc will incorporate obligation into ACs and Tasks |
| [PROPOSED] | Yes* | /fc re-run can incorporate; must add [pending] Review Note |
| [REVIEWED] | No | Review complete; adding requirements invalidates review |
| [WIP] | No | Implementation in progress; scope creep |
| [BLOCKED] | No | May resume without re-reading new handoffs |
| [DONE] | No | Tasks will never execute |

*When routing to [PROPOSED], append to destination's Review Notes:
`- [pending] Handoff from F{source}: {summary} — /fc re-run required`

### Gate 2: Type/Scope Compatibility Gate
The obligation's domain must match the destination Feature's Type and scope.

| Obligation Domain | Compatible Destination Types |
|-------------------|-----------------------------|
| C# code migration stubs | erb, engine (same subsystem) |
| Engine/runtime issues | engine |
| Tooling/workflow bugs | infra |
| Documentation gaps | infra, research |
| UI-layer concerns | engine (engine-layer) |
| Test infrastructure | infra, engine (test project) |

**Cross-domain routing is ALWAYS A** (create new Feature).
**Unlisted domain is ALWAYS A** (if obligation domain not in table above, default to A).

### Gate 3: Execution Guarantee Gate (mechanized)
Verify the obligation aligns with the destination's existing scope.

**Step 1 — Keyword Extraction (mechanical)**:
Extract 2 keywords from the obligation:
1. **Type tag**: from Gate 2 classification (e.g., "engine", "erb", "infra")
2. **Subject noun**: first proper noun or technical term in the obligation description
   (e.g., "IVariableStore", "INtrQuery", "ac-static-verifier", "SANTA cosplay")

**Step 2 — Destination Scope Check**:
| Destination State | Check | Result |
|-------------------|-------|--------|
| [DRAFT] with no Goal section populated | Auto-pass | /fc will define scope and can incorporate |
| Has Goal/Philosophy/Background populated | grep each keyword against Goal + Philosophy + Background | ANY match → pass; ZERO matches → FAIL |

**No speculative judgment**: only grep-verifiable keyword matches count.
**Keyword extraction is mechanical**: type tag from Gate 2 + first technical term from description.

### Gate 4: Sole Candidate Gate
| Candidates Passing Gates 1-3 | Result |
|:-----------------------------:|--------|
| 0 | A (no valid destination) |
| 1 | B (unambiguous ownership) |
| 2+ | A (ambiguous → create new Feature for explicit ownership) |

**Rationale**: Multiple valid candidates create race conditions (F812→F803 lesson).

### Guard Summary
| Check | Condition | Result |
|-------|-----------|--------|
| Status | destination.status not in {[DRAFT], [PROPOSED]} | → FAIL: use Option A |
| Type/Scope | obligation.domain ∉ destination.scope | → FAIL: use Option A |
| Execution | obligation keywords not in destination Goal/Philosophy/Background | → FAIL: use Option A |
| Sole Candidate | multiple destinations pass Gates 1-3 | → FAIL: use Option A |
| All pass | status OK + scope match + keywords match + sole candidate | → Option B allowed |

---

## Historical Misrouting Patterns (Prohibited)

| Pattern | Example | Why Wrong | Correct Action |
|---------|---------|-----------|----------------|
| Code stubs → research feature | F826→F827 (migration stubs to Phase 23 research) | Research features don't implement code | A: dedicated obligations feature |
| Engine obligation → erb feature | F821→F825 (IEngineVariables to Relationships) | Domain mismatch | A: engine-scoped feature |
| Obligation → [WIP] feature | F809→F811 (obligations to [BLOCKED] feature) | Cannot incorporate new requirements mid-execution | A: new [DRAFT] |
| Obligation → [DONE] feature | F793→F782 (obligations to completed post-review) | Tasks will never execute | A: new feature |
| Phase doc without executor | F814→architecture.md Phase tasks | No agent reads Phase doc after features created | A: new feature |
| "確認済み" without Task | F806→F813 (confirmed exists, no Task created) | Confirmation ≠ execution; Action B must always write (追記済み) | A: new [DRAFT] |
| Conditional → Post-Phase Review | F826→F827 (conditional triggers to planning) | Planning feature won't implement triggers | A: trigger-specific feature |
| Race condition routing | F812→F803 ("whoever runs first") | Non-deterministic execution; ambiguous ownership | A: explicit single-owner feature |
| Multiple valid candidates | 2+ features could plausibly own the obligation | Ambiguous → obligation falls through cracks | A: new feature (Sole Candidate Gate) |

---

## Forbidden Patterns

- `F{ID}` without creation Task → Forbidden (documented but not actionable)
- `F{ID} (TBD)` → Forbidden
- `Destination: Undetermined` → Forbidden
- `Decide later` → Forbidden
- `F{ID}` where Status = `[DONE]` → Forbidden (tasks added to completed Features will never be executed)

**Enforcement**: FL PHASE-7 validates Task existence. /run AC validates file creation.
