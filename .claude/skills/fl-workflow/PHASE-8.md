# Phase 8: Handoff Validation

## Entry Check (MANDATORY)

```
- Previous Phase: 7
- Condition met: Phase 7 applied_fixes == 0
- Expected: Phase 8
- Actual: Phase 8
- Match confirmed → proceed
```

## Goal

Ensure all deferred tasks have concrete tracking destinations. No TBD allowed.

---

## Step 8.1: Update Task

```
TaskUpdate(subject: "Phase 8: Handoff Validation", status: "in_progress")
```

## Step 8.2: Validate Handoff Section

**For features only.**

```
IF target_type == "feature":
    # Check Handoff Tracking section
    handoff_section = Grep("Mandatory Handoffs", target_path)

    IF handoff_section exists AND not "Section Deleted":
        # Extract handoff tracking table rows
        handoff_table = parse_table(handoff_section)
        tasks_table = parse_table(Tasks_section)

        FOR row in handoff_table:
            # Check for empty/TBD patterns
            IF row.destination_ID is empty OR row.destination_ID in ["TBD", "Undecided", "Decide later"]:
                persist_pending({
                    severity: "critical",
                    location: "Handoff Tracking section",
                    issue: "Empty or TBD tracking destination: {row.destination_ID}",
                    fix: "Specify concrete tracking destination (Feature ID, Task ID, or Phase number)"
                }, iteration, "Phase8-Handoff")

            # Option A: New Feature - validate creation Task exists
            ELIF row.destination == "Feature" AND NOT exists("pm/features/feature-{ID}.md"):
                creation_task = find_task_containing("Create.*{row.destination_ID}", tasks_table)
                IF creation_task is None:
                    persist_pending({
                        severity: "critical",
                        location: "Handoff Tracking section",
                        issue: "Follow-up Feature {row.destination_ID} has no creation Task",
                        fix: "Add a Task to create {row.destination_ID} in Tasks table"
                    }, iteration, "Phase8-Handoff")
                # If creation Task exists → OK, file will be created during /run

            # Option B: Existing Feature - validate Feature exists
            ELIF row.destination == "Feature" AND exists("pm/features/feature-{ID}.md"):
                # OK - referenced Feature exists
                PASS

            # Option C: Phase - validate Phase exists in architecture.md
            ELIF row.destination == "Phase":
                IF NOT Grep("Phase {N}", "docs/architecture/migration/full-csharp-architecture.md"):
                    persist_pending({
                        severity: "critical",
                        location: "Handoff Tracking section",
                        issue: "Referenced Phase does not exist: {row.destination_ID}",
                        fix: "Verify Phase number in architecture.md or update tracking ID"
                    }, iteration, "Phase8-Handoff")
ELSE:
    TaskUpdate(subject: "Phase 8: Handoff Validation", status: "completed", details: "Phase 8: Handoff Validation (skipped - non-feature)")
```

## Step 8.3: Complete Phase 8

```
TaskUpdate(subject: "Phase 8: Handoff Validation", status: "completed")
```

## Step 8.4: Declare Next Phase (MANDATORY)

**Routing Table**:

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | 2 |
| applied_fixes == 0 | POST-LOOP |

**Max Iteration Exception (Forward-Only Mode)**:

When iteration == MAX_ITERATIONS (10):

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | **POST-LOOP** (NOT 2), forward_fixes_total += applied_fixes |
| applied_fixes == 0 | POST-LOOP |

**Execute**:
```
## Declare Next Phase
- Current Phase: 8
- Condition: applied_fixes = {count}
- Iteration: {I}/10
- Max Iter Mode: {YES if I == 10, else NO}
- Routing Table → Next: {POST-LOOP or Phase 2}

TaskUpdate(subject: "Iteration {I}/10: {Next}", status: "pending")
Read(.claude/skills/fl-workflow/{Next}.md)
```

**Execute routing mechanically. Do not judge.**
