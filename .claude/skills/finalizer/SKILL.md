---
name: finalizer
description: Feature completion agent. MUST BE USED at end of /run to finalize status. Requires haiku model.
context: fork
agent: general-purpose
allowed-tools: Read, Edit, Bash, Glob, Grep
---

# Finalizer Skill

Feature completion specialist. Verifies objectives, updates status, unblocks dependents, prepares commit.

## Input

- `feature-{ID}.md`: Tasks, ACs, Background, Dependencies
- `index-features.md`: Feature tracking
- `index-features-history.md`: History overflow (when needed)

## Output

| Status | Meaning |
|--------|---------|
| READY_TO_COMMIT | All complete, dependents unblocked |
| BLOCKED | [B] ACs exist, feature-{ID}.md + index-features.md updated atomically |
| REVIEW_NEEDED | Objective gap detected |
| NOT_READY | Incomplete tasks/ACs |
| CANCELLED | Feature cancelled |

---

## Step 1: Pre-Finalization Checks

Before proceeding:

- Check Tasks and ACs status:
  - All `[x]` → **Normal path** (Step 2 → [DONE])
  - Any `[B]` (and no `[-]` or `[ ]`) → **Blocked path** (Step 2B → [BLOCKED])
  - Any `[-]` or `[ ]` → return `NOT_READY` immediately
- Build passes, no new warnings
- **Log verification**: `_out/logs/debug/failed/` should not have new entries
  - Check timestamps: old failures OK, new failures BLOCK

---

## Step 2: Objective Verification

| Check | Status |
|-------|--------|
| All goals have ACs | ACHIEVED |
| Some goals not explicit | PARTIAL → REVIEW_NEEDED |
| Major goal missing | GAP → REVIEW_NEEDED |

---

## Step 2B: Blocked Path ([B] ACs exist)

**Trigger**: Step 1 detected `[B]` ACs (no `[-]` or `[ ]`).

```bash
python src/tools/python/feature-status.py set {ID} BLOCKED
```

The tool atomically updates feature-{ID}.md and index-features.md.

→ Skip Steps 3-7 → Go to Step 8 (Stage Files) → Return `BLOCKED`

---

## Step 3: Status Update + Cascade (Normal Path)

Run the status management tool (handles status update, index sync, dependency cascade, history rotation):

```bash
# For DONE:
python src/tools/python/feature-status.py set {ID} DONE
# For CANCELLED:
python src/tools/python/feature-status.py set {ID} CANCELLED
```

The tool performs all of the following atomically:
- Updates `feature-{ID}.md` Status line
- Moves row from Active → Recently Completed in `index-features.md`
- Unbolds `**F{ID}**` → `F{ID}` in Depends On columns
- Updates dependency Status in all dependent features' Dependencies tables
- Unblocks `[BLOCKED]` → `[PROPOSED]` when all Predecessor/Blocker deps satisfied
- Rotates history if Recently Completed exceeds 6

**Output**: The tool prints all actions taken and lists Modified files.

### Cascade Rules (implemented by tool)

- Only `[BLOCKED]` → `[PROPOSED]` transitions occur. `[DRAFT]` stays `[DRAFT]`.
- Cascade propagates until no more features can be unblocked (PROPOSED ≠ DONE = natural termination).
- Only unblocks when ALL Predecessor/Blocker dependencies are [DONE], not just this one.

---

## Step 4: AC Log Cleanup (log cleanup)

**Guard**: Only execute on [DONE] transition. If status is [BLOCKED] or [CANCELLED], skip this step.

Remove feature-scoped AC logs on [DONE] transition (log cleanup):

```bash
# Remove feature-scoped AC result directories across all AC types
rm -rf _out/logs/prod/ac/*/feature-{ID}
# Remove feature-scoped engine TRX files (root-level)
rm -f _out/logs/prod/ac/engine/feature-{ID}*.trx
```

**Note**: `{ID}` must be expanded to the actual feature ID at runtime. These are agent instructions — the implementing agent substitutes the real feature number.

**Error handling**: Non-blocking. If cleanup fails, log a warning but continue to next steps. Cleanup failure must NOT prevent dependent feature unblocking or commit.

**Report**: Include cleanup result (success/warning/skipped) in the Final Report.

---

## Step 5: Stage Files

Stage all files listed in the tool's "Modified:" output:

```bash
git add <files from feature-status.py Modified output>
```

---

## Final Report

```
=== Finalizer Complete ===
Feature: {ID}
Status: [WIP] → [DONE]

Log Cleanup: {Success/Warning/Skipped}

Unblocked Features:
- F{dep_ID}: {name} ([BLOCKED] → [PROPOSED] or Dependencies updated)
- ...
(or "None" if no dependents unblocked)

History Rotation: {Yes/No}

Result: READY_TO_COMMIT
```

---

## Decision Criteria

- Verify completion before status change
- DO NOT commit (Opus does after user approval)
- Commit message format: `feat(F{ID}): {summary}`

---

## Notes

- **Philosophy Gate**: Runs BEFORE finalizer (in FL POST-LOOP or run-workflow). Finalizer assumes Philosophy validation already passed.
- **Commit**: Executed by orchestrator (Opus) in PHASE-9, not by finalizer.
