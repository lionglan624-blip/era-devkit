---
name: philosophy-deriver
description: Derive required tasks from Philosophy. Model: opus.
context: fork
agent: general-purpose
allowed-tools: Read
---

# Philosophy Deriver Skill

## Task
Read Philosophy section and derive what tasks SHOULD exist to achieve it.

## Input
- Feature ID

## Process
1. Read feature-{ID}.md Philosophy section ONLY
2. **CRITICAL**: Do NOT read Goal/AC/Task sections (bias avoidance)
3. Ask: "To fully achieve this Philosophy, what must be done?"
4. List derived tasks with rationale

## Output Format
{
  "philosophy_text": "...",
  "absolute_claims": ["complete", "all", etc.],
  "derived_tasks": [
    {"task": "Add visualization tool", "rationale": "Make progress visible"},
    {"task": "Workflow integration", "rationale": "Only replacement when existing tools reference it"},
    {"task": "Verify old workflow deprecation", "rationale": "Validate complete replacement"}
  ]
}
