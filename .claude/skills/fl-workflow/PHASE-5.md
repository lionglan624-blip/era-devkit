# Phase 5: Feasibility Check

## Entry Check (MANDATORY)

```
- Previous Phase: 4
- Condition met: Phase 4 applied_fixes == 0
- Expected: Phase 5
- Actual: Phase 5
- Match confirmed → proceed
```

## Goal

Verify technical feasibility, identify blockers, assess implementation complexity.

---

## Step 5.1: Update Task

```
TaskUpdate(subject: "Phase 5: Feasibility Check", status: "in_progress")
```

## Step 5.1.5: Feasibility Skip Check (Pending-Dominated)

**Purpose**: When all open issues are user-pending, feasibility check will only re-detect the same blocked items.

**Evidence**: F779 session — Phase 5 found exact same 2 issues as Phase 2 SEMANTIC (AC-005/AC-006), consuming 56K tokens with 0 new information.

```
# Check if all open issues are already [pending]
unresolved_pending = count [pending] entries in Review Notes (exclude [resolved-*])
recent_non_pending_fixes = count [fix] entries from Phase 3 or Phase 4 in current iteration

IF unresolved_pending > 0 AND recent_non_pending_fixes == 0:
    # All progress is blocked by user-pending items
    # Feasibility check will only re-detect the same blocked issues
    TaskUpdate(subject: "Phase 5: Feasibility Check", status: "completed",
              details: "Skipped — all open issues are [pending], no new fixes from Phase 3-4")
    → GOTO Declare Next Phase (with applied_fixes = 0)
```

## Step 5.2: Dispatch Feasibility Checker

```
feasibility = Task(
  subagent_type: "feasibility-checker",
  model: "sonnet",  # Codebase investigation (Glob/Grep/Read) — sonnet sufficient
  prompt: `Feature {target_id}.

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
)

FOR issue in feasibility.issues:
    IF is_loop(issue):  # Loop detected
        persist_pending(issue, iteration, "Phase5-Feasibility")  # Immediate file write
    ELIF is_orchestrator_decidable(issue):
        # Orchestrator-Decidable: additive fixes auto-applied (see SKILL.md)
        apply_fix(issue)
        persist_fix(issue, iteration, "Phase5-Feasibility")
        applied_fixes++
        CONTINUE
    ELIF issue.severity == "critical" AND (issue.fix is empty OR issue.fix contains "TBD" OR issue.fix contains "investigate"):
        persist_pending(issue, iteration, "Phase5-Feasibility")  # Vague fix, needs user review
    ELIF issue.location NOT found in target_file:
        persist_pending(issue, iteration, "Phase5-Feasibility-InvalidLocation")
    ELSE:
        apply_fix(issue)
        persist_fix(issue, iteration, "Phase5-Feasibility")
        applied_fixes++
```

## Step 5.3: AC Design Constraints Validation

**For all feature types with AC Design Constraints section**:

```
IF feature has "## AC Design Constraints" section:
    FOR each constraint in AC Design Constraints table:
        # Verify constraint still holds
        IF constraint.source is file path:
            content = Read(constraint.source)
            IF content contradicts constraint:
                ISSUE: "Constraint C{N} may be stale - source changed"
                FIX: "Re-verify constraint or update AC Design Constraints"
                applied_fixes++

        # Verify ACs respect constraint
        IF constraint.ac_implication mentions specific AC#:
            ac = find_ac(constraint.ac_implication)
            IF ac does NOT respect constraint:
                ISSUE: "AC#{N} violates constraint C{M}"
                FIX: "Update AC to respect constraint"
                applied_fixes++
```

**Validation Matrix**:

| Constraint Type | Validation |
|-----------------|------------|
| scope_limitation | AC scope <= constraint scope |
| technical_impossibility | No AC for impossible item |
| dependency_requirement | Predecessor in Dependencies |
| baseline_data | AC Expected uses baseline value |

## Step 5.4: Investigation Tag Validation

**For erb/engine types only** (kojo/infra skip this step):

```
FOR each Task in Tasks table:
    IF Task has placeholder Expected (TBD, ???, [PLACEHOLDER]):
        IF Task does NOT have [I] tag:
            ISSUE: "Task#{N} has placeholder Expected but no [I] tag"
            FIX: "Add [I] tag to Task#{N} or provide concrete Expected"
            applied_fixes++

    IF Task has [I] tag:
        IF corresponding AC has concrete (non-placeholder) Expected:
            # OK - [I] tag with concrete Expected is valid
            PASS
        ELSE:
            # OK - [I] tag allows placeholder
            PASS
```

**Validation Logic**:

| Task Tag | AC Expected | Result |
|:--------:|-------------|--------|
| (none) | Concrete | OK |
| (none) | Placeholder | **ISSUE**: Add `[I]` or concrete Expected |
| `[I]` | Concrete | OK |
| `[I]` | Placeholder | OK (resolved at /run Phase 4) |

## Step 5.5: Complete Phase 5

```
TaskUpdate(subject: "Phase 5: Feasibility Check", status: "completed")
```

## Step 5.6: Declare Next Phase (MANDATORY)

**Routing Table**:

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | 2 |
| applied_fixes == 0 | 6 |

**Max Iteration Exception (Forward-Only Mode)**:

When iteration == MAX_ITERATIONS (10):

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | **6** (NOT 2), forward_fixes_total += applied_fixes |
| applied_fixes == 0 | 6 |

**Execute**:
```
## Declare Next Phase
- Current Phase: 5
- Condition: applied_fixes = {count}
- Iteration: {I}/10
- Max Iter Mode: {YES if I == 10, else NO}
- Routing Table → Next: Phase {X}

TaskUpdate(subject: "Iteration {I}/10: Phase {X}", status: "pending")
Read(.claude/skills/fl-workflow/PHASE-{X}.md)
```

**Execute routing mechanically. Do not judge.**
