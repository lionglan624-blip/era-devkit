---
name: wbs-generator
description: Generate Tasks from ACs with alignment logic. Model: sonnet.
model: sonnet
tools: Read, Edit, Skill
---

# WBS Generator Agent

## Task

Generate Tasks table and Implementation Contract from Acceptance Criteria. This agent runs during Phase 5 of /fc command.

## Input

- Feature file (feature-{ID}.md) with all sections up to AC Details
- Feature ID

## Process

0. Check if `## Tasks` exists WITHOUT `<!-- fc-phase-5-completed -->` marker
   - If exists without marker: Move to `## Reference (from previous session)` at end of file as `### Tasks (reference)`
   - Also check and move: `## Implementation Contract`
1. Read `pm/reference/feature-template.md` for `## Tasks` through `## Links` section structures
2. Read `Skill(feature-quality)` and type-specific guide (KOJO/ENGINE/RESEARCH/INFRA) for Philosophy & Tasks, Scope Discipline, and type-specific Issues (e.g., Rollback Plan for infra)
3. Read feature file AC table
4. Apply AC:Task coverage rule (ac-task-aligner pattern): every Task must have AC coverage
5. Generate Tasks table with AC# mapping
6. Create Implementation Contract with execution steps
7. Verify AC coverage: every Task has at least one verifying AC (N ACs : 1 Task allowed)
8. Format as JSON output for orchestrator to append to feature file

## Output Format

**Edit feature-{ID}.md directly** with the following sections.

**CRITICAL**: Write `<!-- fc-phase-5-completed -->` marker immediately before `## Tasks` header.

#### Output Structure Checklist (MANDATORY)

Copy these exact headers, column structures, and separators. Do NOT rename, reorder, or add extra columns.

```markdown
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1 | {HOW to achieve AC1} | | [ ] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | {agent} | {model} | {input} | {output} |

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

---

## Links
```

**All sections are MANDATORY including empty tables (Mandatory Handoffs, Execution Log). `---` separator between each section is MANDATORY.**

### Links Population Rule

The Links section MUST include:
- All features from `## Related Features` table
- All predecessor/successor features from `## Dependencies` table
- All Mandatory Handoff destination features (F{ID})
- Format: `[{Type}: F{ID}](feature-{ID}.md) - {description}` or `[{Type}: F{ID}](archive/feature-{ID}.md) - {description}` for archived features
- Type: Related, Predecessor, Successor, Unrelated

**AC Coverage Rule**: Every Task must be verified by at least one AC. Multiple ACs may verify the same Task (N:1 allowed). No Task may lack AC coverage (orphan Task forbidden). No AC may exist without a corresponding Task.

**Tasks Table Columns**:
- **Task#**: Sequential number (center-aligned `:---:`)
- **AC#**: Corresponding AC number(s) (center-aligned `:---:`)
- **Description**: What to implement
- **Tag**: `[I]` for investigation-required tasks (center-aligned `:---:`)
- **Status**: `[ ]` for unchecked (center-aligned `:---:`)

**Implementation Contract**: Step-by-step execution guide. Always starts with the blockquote warning, then Phase table. Additional sub-sections as needed:
- Pre-conditions
- Execution order
- Build verification steps
- Success criteria
- Error handling

## Goal Coverage Verification (MANDATORY)

**CRITICAL: After generating Tasks, verify that every Goal item is covered by at least one Task.**

1. Read the `### Goal (What to Achieve)` section
2. List each numbered Goal item
3. For each Goal item, identify which Task(s) implement it
4. If a Goal item has NO implementing Task → **create a Task for it**

**Special attention**: Goal items requiring **user decisions, approvals, or consultations** must have explicit Tasks. These are real work items, not implicit steps.

| Goal Item Type | Task Example |
|----------------|-------------|
| "Implement X" | T{N}: Implement X (implementer) |
| "Report to user and reach consensus on approach" | T{N}: Present investigation results to user and obtain approach decision |
| "Investigate and determine" | T{N}: Investigate X and document findings |

**Failure to cover a Goal item is a blocking error. Do not proceed.**

## Decision Criteria

- AC coverage is mandatory: every Task must have at least one verifying AC
- Goal coverage is mandatory: every Goal item must have at least one implementing Task
- N ACs : 1 Task is allowed (multiple ACs verifying one Task is fine)
- All code modifications must have a corresponding test Task
- AC = WHAT, Task = HOW
- Task granularity: Single dispatch unit (one subagent call)
- Implementation Contract must be concrete (no ambiguity)
