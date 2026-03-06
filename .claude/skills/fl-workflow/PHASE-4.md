# Phase 4: AC Validation

## Entry Check (MANDATORY)

```
- Previous Phase: 3
- Condition met: Phase 3 applied_fixes == 0
- Expected: Phase 4
- Actual: Phase 4
- Match confirmed → proceed
```

## Goal

Validate Acceptance Criteria format, matcher correctness, and TDD feasibility.

---

## Step 4.1: Update Task

```
TaskUpdate(subject: "Phase 4: AC Validation", status: "in_progress")
```

## Step 4.2: Check Target Type

```
IF target_type != "feature":
    TaskUpdate(subject: "Phase 4: AC Validation", status: "completed", details: "Phase 4: AC Validation (skipped - non-feature)")
    # BREAK → POST-LOOP (non-feature targets complete here)
    TaskUpdate(subject: "Iteration {I}/10: POST-LOOP", status: "pending")
    Read(.claude/skills/fl-workflow/POST-LOOP.md)
    # Exit this phase - do not proceed to Step 4.3
```

## Step 4.2.5: AC Structural Lint Gate

**CRITICAL: Run before ac-validator. Catches structural errors that AI agents misinterpret.**

```bash
python src/tools/python/ac_ops.py ac-check {target_id}
python src/tools/python/ac_ops.py ac-renumber {target_id}
```

| Result | Action |
|--------|--------|
| ac-check Exit 0 | Proceed to Step 4.3 |
| ac-check Exit 1 | Report raw output verbatim. Each issue = 1 applied_fix after auto-fix attempt. If `ac_ops.py ac-fix` or manual Edit resolves it, persist_fix and applied_fixes++. If not resolvable, persist_pending. |

ac-renumber always runs (idempotent — no-op when no gaps exist). Closes numbering gaps left by previous AC modifications.

> **Available fix tools**: `ac-fix` (update Expected/Description), `ac-renumber` (close gaps), `ac-insert`, `ac-delete`. Run `python src/tools/python/ac_ops.py --help` for all subcommands.

## Step 4.3: Dispatch AC Validator

```
ac_validation = Task(
  subagent_type: "ac-validator",
  prompt: `Feature {target_id}.

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
)

FOR issue in ac_validation.issues:
    IF is_loop(issue):
        persist_pending(issue, iteration, "Phase4-ACValidation")  # Immediate file write
    ELSE:
        apply_fix(issue)
        persist_fix(issue, iteration, "Phase4-ACValidation")
        applied_fixes++
```

## Step 4.3.5: Dispatch AC-Task Aligner

```
alignment = Task(
  subagent_type: "ac-task-aligner",
  prompt: `Check AC:Task alignment for Feature {target_id}.

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
)

IF alignment.status == "BLOCKED":
    persist_pending({
        severity: "critical",
        location: "AC-Task alignment",
        issue: alignment.reason,
        fix: "Manual AC-Task alignment required"
    }, iteration, "Phase4-ACAlignment")
ELIF alignment.status == "FIXED":
    # ac-task-aligner auto-fixed alignment (Orchestrator-Decidable per SKILL.md)
    persist_fix({location: "AC/Tasks tables", issue: "AC-Task alignment corrected"}, iteration, "Phase4-ACAlignment")
    applied_fixes++
# ALIGNED = no action needed
```

## Step 4.3.7: AC Count Threshold Check

**Purpose**: Detect AC count growth beyond documented limits during FL iterations.

```
ac_count = count_rows(AC_Definition_Table)

IF ac_count > 50:
    persist_pending({
        severity: "critical",
        location: "AC Definition Table",
        issue: "AC count ({ac_count}) exceeds hard limit (50). Feature MUST be split.",
        fix: "Split feature into smaller sub-features"
    }, iteration, "Phase4-ACCount")

ELIF ac_count > 30:
    # Check if deviation comment already exists
    IF NOT Grep("Deviation.*AC.*exceed", target_path):
        persist_pending({
            severity: "major",
            location: "AC Definition Table",
            issue: "AC count ({ac_count}) exceeds soft limit (30). Add deviation comment with justification or split feature.",
            fix: "Add <!-- Deviation: {ac_count} ACs exceed ... --> comment above AC Definition Table"
        }, iteration, "Phase4-ACCount")
    # If deviation comment exists, the count is acknowledged — no action needed
```

**Rationale**: F813 grew from ~13 ACs (FC) to 37 ACs (FL end) without warning. quality-fixer C25/C26 catch this at FC time; this step catches growth during FL iterations.

## Step 4.4: Complete Phase 4

```
TaskUpdate(subject: "Phase 4: AC Validation", status: "completed")
```

## Step 4.5: Declare Next Phase (MANDATORY)

**Routing Table** (default):

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | 2 |
| applied_fixes == 0 | 5 |

**Feature Type Fast-Path Override** (see SKILL.md for details):

| Feature Type | applied_fixes == 0 | Rationale |
|--------------|:------------------:|-----------|
| kojo | → **7** (skip 5, 6) | Fixed structure, feasibility is content-only |
| research | → **6** (skip 5) | Outputs are features, not code |
| erb/engine/infra | → 5 (default) | Full validation required |

```
# Apply fast-path routing
IF applied_fixes == 0:
    IF feature.type == "kojo":
        next_phase = 7  # Skip Phase 5 (Feasibility), Phase 6 (Planning)
    ELIF feature.type == "research":
        next_phase = 6  # Skip Phase 5 (Feasibility)
    ELSE:
        next_phase = 5  # Default routing
```

**Max Iteration Exception (Forward-Only Mode)**:

When iteration == MAX_ITERATIONS (10):

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | **5** (NOT 2), forward_fixes_total += applied_fixes |
| applied_fixes == 0 | 5 |

**Execute**:
```
## Declare Next Phase
- Current Phase: 4
- Condition: applied_fixes = {count}
- Iteration: {I}/10
- Max Iter Mode: {YES if I == 10, else NO}
- Routing Table → Next: Phase {X}

TaskUpdate(subject: "Iteration {I}/10: Phase {X}", status: "pending")
Read(.claude/skills/fl-workflow/PHASE-{X}.md)
```

**Execute routing mechanically. Do not judge.**
