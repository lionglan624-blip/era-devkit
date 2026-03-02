# Phase 7: Final Reference Check

## Entry Check (MANDATORY)

```
- Previous Phase: 6
- Condition met: Phase 6 applied_fixes == 0
- Expected: Phase 7
- Actual: Phase 7
- Match confirmed → proceed
```

## Goal

Re-validate all references after all fixes have been applied. Ensures no broken references introduced during fix iterations.

---

## Step 7.1: Update Task

```
TaskUpdate(subject: "Phase 7: Final Reference Check", status: "in_progress")
```

## Step 7.2: Dispatch Reference Checker

**For features only.**

```
IF target_type == "feature":
    final_ref_check = Task(
      subagent_type: "general-purpose",
      model: "sonnet",
      prompt: `Read .claude/skills/reference-checker/SKILL.md and execute for Feature {target_id}

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
    )

    IF final_ref_check.status == "NEEDS_REVISION":
        FOR issue in final_ref_check.issues:
            IF issue.severity == "critical":
                persist_pending(issue, iteration, "Phase7-FinalRefCheck")  # Immediate file write
            ELIF issue.fix is empty OR issue.fix contains "TBD" OR issue.fix contains "investigate":
                persist_pending(issue, iteration, "Phase7-FinalRefCheck")  # Vague fix, needs user review
            ELIF issue.location NOT found in target_file:
                persist_pending(issue, iteration, "Phase7-FinalRefCheck-InvalidLocation")
            ELSE:
                apply_fix(issue)
                persist_fix(issue, iteration, "Phase7-FinalRefCheck")
                applied_fixes++

        IF applied_fixes > 0:
            CONTINUE  # Re-review from Phase 2 after fixes
ELSE:
    TaskUpdate(subject: "Phase 7: Final Reference Check", status: "completed", details: "Phase 7: Final Reference Check (skipped - non-feature)")
```

## Step 7.3: Complete Phase 7

```
TaskUpdate(subject: "Phase 7: Final Reference Check", status: "completed")
```

## Step 7.4: Declare Next Phase (MANDATORY)

**Routing Table**:

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | 2 |
| applied_fixes == 0 | 8 |

**Max Iteration Exception (Forward-Only Mode)**:

When iteration == MAX_ITERATIONS (10):

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | **8** (NOT 2), forward_fixes_total += applied_fixes |
| applied_fixes == 0 | 8 |

**Execute**:
```
## Declare Next Phase
- Current Phase: 7
- Condition: applied_fixes = {count}
- Iteration: {I}/10
- Max Iter Mode: {YES if I == 10, else NO}
- Routing Table → Next: Phase {X}

TaskUpdate(subject: "Iteration {I}/10: Phase {X}", status: "pending")
Read(.claude/skills/fl-workflow/PHASE-{X}.md)
```

**Execute routing mechanically. Do not judge.**
