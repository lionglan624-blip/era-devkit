---
name: run-workflow
description: Progressive Disclosure version of Feature implementation workflow. Standard /run command phases.
---

# run-workflow Skill

Progressive Disclosure version of Feature implementation workflow.

## Overview

| Phase | Name | Skill |
|:-----:|------|-------|
| 1 | Initialize | Read(.claude/skills/run-workflow/PHASE-1.md) |
| 2 | Investigation | Read(.claude/skills/run-workflow/PHASE-2.md) |
| 3 | Test Creation (TDD RED) | Read(.claude/skills/run-workflow/PHASE-3.md) |
| 4 | Implementation (TDD GREEN) | Read(.claude/skills/run-workflow/PHASE-4.md) |
| 5 | Refactoring (TDD REFACTOR) | Read(.claude/skills/run-workflow/PHASE-5.md) |
| 6 | Test Generation (kojo) | Read(.claude/skills/run-workflow/PHASE-6.md) |
| 7 | Verification | Read(.claude/skills/run-workflow/PHASE-7.md) |
| 8 | Post-Review | Read(.claude/skills/run-workflow/PHASE-8.md) |
| 9 | Report & Approval | Read(.claude/skills/run-workflow/PHASE-9.md) |
| 10 | Finalize, Commit & CodeRabbit Review | Read(.claude/skills/run-workflow/PHASE-10.md) |

## Global Rules (Always Applied)

### Workflow Artifact Immutability (F814 Lesson)

**CRITICAL: Orchestrator MUST NOT modify workflow artifacts during /run execution.**

| Protected | Examples |
|-----------|---------|
| `.claude/skills/**/*.md` | SKILL.md, PHASE-*.md |
| `.claude/commands/*.md` | fc.md, fl.md, run.md |
| `.claude/agents/*.md` | Agent definitions |

Modifying these files to bypass validation gates is **self-hacking** — the orchestrator rewriting its own rules to pass checks that would otherwise fail.

**If a gate blocks progress**: STOP → Report to user. Do not alter the gate.

### DEVIATION Recording

**CRITICAL: When Bash exit ≠ 0 or Agent ERR occurs, MUST record before next action**

```markdown
// Append to feature-{ID}.md Execution Log
| {timestamp} | DEVIATION | {source} | {action} | {detail} |
```

**DEVIATION Patterns**:
- Bash exit ≠ 0
- Agent ERR:*
- NEEDS_REVISION
- retry
- timeout
- Manual intervention

### TDD RED Filtering

Phase 3 TDD RED (expected test failure) is NOT a DEVIATION. The deviation-log.txt hook records ALL exit != 0 events mechanically. The Phase終了チェック in Phase 3 MUST filter TDD RED entries when comparing hook log vs feature.md counts. See Phase 3 DEVIATION Check for details.

### DEVIATION Definition (F575 Lesson)

**What DEVIATION is**:
- A **factual record** of events that occurred during execution
- **Irrelevant** whether the cause is F{ID} implementation bug or not
- "Environment issue" and "PRE-EXISTING" **are still DEVIATION**
- Judgment, interpretation, and exemption are **not allowed**

**Prohibited patterns**:
- ❌ "No need to record because it's an environment issue"
- ❌ "Exclude because it's not F{ID}'s problem"
- ❌ "Not a DEVIATION because it was expected"

**Correct response**:
- ✅ exit ≠ 0 occurs → Record immediately → Root cause analysis in Phase 9.8

### Track What You Skip (F696 Lesson)

**CRITICAL: All discovered issues MUST be tracked with concrete destination**

**Principle**: Defer is OK, forget is not.

**PRE-EXISTING issues are NOT exempt**:
- PRE-EXISTING = 発見時点で既に存在していた問題
- PRE-EXISTING でも **Action (A/B/C) と Destination (F{ID}) は必須**
- 「PRE-EXISTINGだから追跡不要」は **禁止**

**Prohibited patterns**:
- ❌ "PRE-EXISTING なので Action/Destination なし"
- ❌ "F{ID} のスコープ外なので追跡不要"
- ❌ Mandatory Handoffs テーブルに Destination 空欄

**Correct response**:
- ✅ PRE-EXISTING 発見 → Record DEVIATION → Phase 9.8 で A/B/C 決定 → Destination 作成/指定

### Subagent Dispatch

| Agent | type | model | Role |
|-------|------|:-----:|------|
| initializer | general-purpose | haiku | Feature state |
| explorer | Explore | - | Code investigation |
| feature-reviewer | general-purpose | opus | Pre/Post review |
| implementer | general-purpose | sonnet | ERB/Engine code |
| ac-tester | general-purpose | haiku | AC verification |
| debugger | debugger | sonnet→opus | Error fix |
| finalizer | finalizer | haiku | Commit prep |

**CRITICAL: Do NOT use `run_in_background: true` for ANY Task dispatch. Background tasks cause premature session exit in `-p` mode (the parent process terminates before background agents complete, losing all results). All Tasks MUST be synchronous.**

### Type Routing

| Type | Agent | Test Mode |
|------|-------|-----------|
| kojo | kojo-writer | --unit |
| erb | implementer | --unit |
| engine | implementer | dotnet test + --unit |
| infra | implementer | Static verification |
| research | DRAFT creation + /fc | Static verification |

### Context Pressure Gate (/run)

**Purpose**: Prevent context exhaustion during long /run sessions by checking context usage at critical phase boundaries.

**Check Points**: Before Phase 7 (Verification) and Phase 9 (Report & Approval) — the most context-intensive phases.

**Mechanism**: Same as FL workflow — reads `_out/tmp/claude-ctx-f{ID}.txt` written by statusline.

```
CONTEXT_PRESSURE_THRESHOLD = 80

check_run_context_pressure(feature_id):
    pct_file = "_out/tmp/claude-ctx-f{feature_id}.txt"
    IF file exists:
        context_pct = int(Read(pct_file).strip())
        RETURN context_pct
    RETURN -1  # Unknown

# At Phase 7 / Phase 9 entry:
context_pct = check_run_context_pressure(feature_id)
IF context_pct >= CONTEXT_PRESSURE_THRESHOLD:
    # Record in Execution Log
    Edit(feature-{ID}.md, append to Execution Log:
        "| {timestamp} | CONTEXT_PRESSURE | orchestrator | {context_pct}% at Phase {N} entry | Graceful exit |")
    # Phase markers (see below) enable resume
    Report to user: "Context pressure {context_pct}% ≥ 80%. Execution Log + phase markers saved. Re-run `/run {ID}` to resume from Phase {N}."
    EXIT
```

**Fallback**: If file not found (`context_pct == -1`), proceed normally.

### Phase Completion Markers (Resume Support)

**Purpose**: Enable `/run` resume after context compression or session crash by marking completed phases in the Execution Log.

**Marker Format**: Appended to feature-{ID}.md Execution Log after each Phase completes:
```html
<!-- run-phase-{N}-completed -->
```

**Write Timing**: Each Phase's "Declare Next Phase" step appends the marker to the Execution Log immediately after the Phase completion TaskUpdate.

**Resume Detection** (Phase 1, Step 1.0.5): When status is `[WIP]`, scan Execution Log for markers in reverse order to determine `resume_from`:
```
IF status == [WIP]:
    markers = Grep("<!-- run-phase-\\d+-completed -->", feature_file)
    IF markers found:
        last_completed = max(extract_phase_numbers(markers))
        resume_from = last_completed + 1
        Log: "Resuming /run from Phase {resume_from} (Phase {last_completed} completed)"
    ELSE:
        resume_from = 1  # No markers = start from beginning
```

**Cleanup**: Markers are NOT deleted after /run completion. They serve as audit trail.

## Start

**Now read Phase 1**:
```
Read(.claude/skills/run-workflow/PHASE-1.md)
```
