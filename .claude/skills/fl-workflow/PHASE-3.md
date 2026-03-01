# Phase 3: Maintainability Review

## Entry Check (MANDATORY)

```
- Previous Phase: 2
- Condition met: Phase 2 (issues.count == 0) or (applied_fixes == 0)
- Expected: Phase 3
- Actual: Phase 3
- Match confirmed → proceed
```

## FORBIDDEN Suggestions (Zero Debt Upfront - Anti-YAGNI)

**CRITICAL**: This project does NOT follow YAGNI. The following suggestions are **FORBIDDEN**:

| Forbidden Pattern | Why |
|-------------------|-----|
| "Match existing pattern" | Existing may be debt; new design may be correct |
| "Keep it simple/minimal" | Causes future cost increase |
| "YAGNI" / "Not needed now" | Violates Zero Debt Upfront principle |
| "Over-engineering" | Proper design investment is not over-engineering |

**Correct Approach**:
- Research future extension patterns before design
- Reference similar systems / prior implementations
- Choose technically correct implementation from the start

## Goal

Check responsibility clarity, philosophy coverage, task coverage, technical debt elimination, and long-term maintainability.

**Scope by Feature Type**:
- All types: Responsibility Clarity, Philosophy Coverage, Task Coverage
- engine/erb/infra only: Technical Debt, Maintainability, Extensibility (code-related)

---

## Step 3.1: Update Task

```
TaskUpdate(subject: "Phase 3: Maintainability Review", status: "in_progress")
```

## Step 3.2: Dispatch Maintainability Reviewer

**For features only.**

```
IF target_type == "feature":
    maintainability = Task(
      subagent_type: "feature-reviewer",
      prompt: `Feature {target_id}. [mode: maintainability]

OUTPUT RULE: Your ENTIRE response must be a single JSON object. Any text outside the JSON (analysis, reasoning, "Let me", explanations) is a protocol violation.`
    )

    FOR issue in maintainability.issues:
        IF is_loop(issue):
            persist_pending(issue, iteration, "Phase3-Maintainability")  # Immediate file write
        ELIF is_orchestrator_decidable(issue):
            # Orchestrator-Decidable: additive fixes auto-applied (see SKILL.md)
            apply_fix(issue)
            persist_fix(issue, iteration, "Phase3-Maintainability")
            applied_fixes++
            CONTINUE
        ELIF issue.severity == "critical" AND (issue.fix is empty OR issue.fix contains "TBD" OR issue.fix contains "investigate"):
            persist_pending(issue, iteration, "Phase3-Maintainability")  # Vague fix, needs user review
        ELIF issue.location NOT found in target_file:
            persist_pending(issue, iteration, "Phase3-Maintainability-InvalidLocation")
        ELSE:
            apply_fix(issue)
            persist_fix(issue, iteration, "Phase3-Maintainability")
            applied_fixes++
```

## Step 3.3: Complete Phase 3

```
TaskUpdate(subject: "Phase 3: Maintainability Review", status: "completed")
```

## Step 3.4: Declare Next Phase (MANDATORY)

**Routing Table**:

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | 2 |
| applied_fixes == 0 | 4 |

**Max Iteration Exception (Forward-Only Mode)**:

When iteration == MAX_ITERATIONS (10):

| Condition | Next Phase |
|-----------|:----------:|
| applied_fixes > 0 | **4** (NOT 2), forward_fixes_total += applied_fixes |
| applied_fixes == 0 | 4 |

**Execute**:
```
## Declare Next Phase
- Current Phase: 3
- Condition: applied_fixes = {count}
- Iteration: {I}/10
- Max Iter Mode: {YES if I == 10, else NO}
- Routing Table → Next: Phase {X}

TaskUpdate(subject: "Iteration {I}/10: Phase {X}", status: "pending")
Read(.claude/skills/fl-workflow/PHASE-{X}.md)
```

**Execute routing mechanically. Do not judge.**
