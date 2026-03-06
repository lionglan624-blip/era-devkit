---
name: task-comparator
description: Compare derived tasks with current tasks. Model: sonnet.
context: fork
agent: general-purpose
allowed-tools: Read
---

# Task Comparator Skill

## Task
Compare philosophy-derived tasks with current feature tasks.

## Input
- Feature ID
- Derived tasks (from philosophy-deriver)

## Process
1. Read feature-{ID}.md Tasks section
2. Map each derived task to current task (semantic matching)
3. Identify gaps (derived but not in current)
4. Identify extras (in current but not derived - OK, just note)

## Output Format
{
  "mappings": [
    {"derived": "Add visualization tool", "current_task": "Task 1-5", "status": "covered"},
    {"derived": "Workflow integration", "current_task": null, "status": "gap"},
    {"derived": "Verify old workflow deprecation", "current_task": null, "status": "gap"}
  ],
  "gaps": [
    {"task": "Workflow integration", "rationale": "..."},
    {"task": "Verify old workflow deprecation", "rationale": "..."}
  ],
  "recommendation": "PARTIAL - 2 gaps detected"
}

## Zero Debt Upfront: Gap Handling

**CRITICAL**: When gaps are detected, the orchestrator (Opus) decides ADOPT/DEFER/REJECT.

This agent (task-comparator) does NOT recommend DEFER. It only reports gaps.

**Orchestrator DEFER Constraint** (for Opus reference):

DEFER is ONLY valid when:
- Predecessor feature required but not [DONE]
- External dependency unavailable
- Technical conflict with this feature

DEFER is INVALID when:
- "Scope would be too large" → ADOPT
- "Cost/effort is high" → ADOPT
- "Better to do in separate feature" → ADOPT (unless technical reason)
