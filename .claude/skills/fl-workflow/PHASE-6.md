# Phase 6: Planning Validation (Research Type Only)

## Entry Check (MANDATORY)

```
- Previous Phase: 5
- Condition met: Phase 5 applied_fixes == 0
- Expected: Phase 6
- Actual: Phase 6
- Match confirmed → proceed
```

## Goal

**Pre-mode**: Validate design before/during `/run`.
**Post-mode**: Validate sub-feature quality after `/run`.

---

## Post-mode Responsibilities (Status == [DONE])

When research feature is [DONE], validate:

1. **Sub-Feature Existence** - All files exist
2. **Architecture Coverage** - All Tasks are covered
3. **Philosophy Inheritance** - Each sub-feature has Philosophy
4. **AC Pattern Quality** - Zero-debt AC, Build/Test AC are correct
5. **Transition Features** - Post-Phase Review + Next Planning exist
6. **Index Update** - All sub-features are registered in index

**IMPORTANT**: Sub-feature quality issues (e.g., AC pattern errors) are fixed HERE, not in each sub-feature's FL.

---

## Step 6.1: Update Task

```
TaskUpdate(subject: "Phase 6: Planning Validation (research only)", status: "in_progress")
```

## Step 6.2: Check Feature Type

```
IF target_type == "feature":
    feature_type = read_feature_type(target_path)  # "## Type: research" etc.
    IF feature_type == "research":
        # Detect execution state by status (pre vs post)
        feature_status = read_feature_status(target_path)  # "## Status: [DONE]" etc.

        IF feature_status == "[DONE]":
            planning_mode = "post"  # Sub-features created, validate quality
        ELSE:  # [PROPOSED], [REVIEWED], [WIP]
            planning_mode = "pre"   # Validate design before/during execution

        planning_val = Task(
          subagent_type: "general-purpose",
          model: "sonnet",
          prompt: `Read .claude/agents/planning-validator.md and validate Feature {target_id} [mode: {planning_mode}]

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
        )

        IF planning_val.status == "NEEDS_REVISION":
            FOR issue in planning_val.issues:
                IF is_loop(issue):
                    persist_pending(issue, iteration, "Phase6-PlanningValidation")  # Immediate file write
                ELIF issue.severity == "critical" AND (issue.fix is empty OR issue.fix contains "TBD" OR issue.fix contains "investigate"):
                    persist_pending(issue, iteration, "Phase6-PlanningValidation")  # Vague fix, needs user review
                ELIF issue.location NOT found in target_file:
                    persist_pending(issue, iteration, "Phase6-PlanningValidation-InvalidLocation")
                ELSE:
                    apply_fix(issue)
                    persist_fix(issue, iteration, "Phase6-PlanningValidation-{planning_mode}")
                    applied_fixes++

            IF applied_fixes > 0:
                CONTINUE  # Re-review from Phase 2 after fixes
    ELSE:
        TaskUpdate(subject: "Phase 6: Planning Validation (research only)", status: "completed", details: "Phase 6: Planning Validation (skipped - not research type)")
ELSE:
    TaskUpdate(subject: "Phase 6: Planning Validation (research only)", status: "completed", details: "Phase 6: Planning Validation (skipped - non-feature)")
```

## Step 6.3: Complete Phase 6

```
TaskUpdate(subject: "Phase 6: Planning Validation (research only)", status: "completed")
```

## Step 6.4: Declare Next Phase (MANDATORY)

**Routing Table**:

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | 2 |
| applied_fixes == 0 | 7 |

**Max Iteration Exception (Forward-Only Mode)**:

When iteration == MAX_ITERATIONS (10):

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | **7** (NOT 2), forward_fixes_total += applied_fixes |
| applied_fixes == 0 | 7 |

**Execute**:
```
## Declare Next Phase
- Current Phase: 6
- Condition: applied_fixes = {count}
- Iteration: {I}/10
- Max Iter Mode: {YES if I == 10, else NO}
- Routing Table → Next: Phase {X}

TaskUpdate(subject: "Iteration {I}/10: Phase {X}", status: "pending")
Read(.claude/skills/fl-workflow/PHASE-{X}.md)
```

**Execute routing mechanically. Do not judge.**
