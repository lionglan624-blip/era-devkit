# Feature 561: FL Parallel Execution Mode

## Status: [DONE]

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: infra

## Background

### Philosophy (Mid-term Vision)
FL workflow should support batch processing of multiple features. Efficiency gains are inherent in parallel execution - no explicit metric required.

### Problem (Current Issue)
1. `/fl` can only process one feature at a time
2. Wave batch review requires sequential `/fl` calls
3. No aggregation of pending_user issues across multiple features

### Goal (What to Achieve)
1. Add parallel execution mode for multiple feature IDs
2. Support `wave1` syntax for Wave batch processing
3. Aggregate pending_user issues from all parallel Tasks

**Note**: This feature was split from [Feature 557](feature-557.md) (Progressive Disclosure).

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| `.claude/skills/fl-workflow/SKILL.md` | Add parallel mode documentation | Skill content |
| `.claude/commands/fl.md` | Add wave syntax to Usage table | Minimal (example only) |

**Note**: fl.md remains a thin wrapper per F557. Only Usage table example is added; all parallel mode logic is documented in SKILL.md and executed by Claude Code.

### Rollback Plan

If issues arise after deployment:
1. Revert commit via `git revert HEAD`
2. Remove parallel mode logic from SKILL.md
3. `/fl` returns to single-ID mode

---

## Dependencies

| Type | Feature ID | Name | Status |
|------|------------|------|--------|
| Predecessor | F557 | FL Workflow Progressive Disclosure | [DONE] |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Parallel mode branch logic exists | file | Grep(SKILL.md) | contains | "Parallel Mode" | [x] |
| 2 | Single ID inline mode documented | file | Grep(SKILL.md) | contains | "Inline Mode" | [x] |
| 3 | Task spawn for parallel execution | file | Grep(SKILL.md) | contains | "run_in_background: true" | [x] |
| 4 | pending_user aggregation documented | file | Grep(SKILL.md) | contains | "Aggregate" | [x] |
| 5 | wave1 syntax documented | file | Grep(SKILL.md) | contains | "wave1" | [x] |
| 6 | wave expansion logic | file | Grep(SKILL.md) | contains | "wave expansion" | [x] |
| 7 | Mode selection logic documented | file | Grep(SKILL.md) | contains | "ID count" | [x] |
| 8 | Wave prerequisite error documented | file | Grep(SKILL.md) | contains | "Wave data not found" | [x] |
| 9 | fl.md Usage includes wave syntax | file | Grep(fl.md) | contains | "wave" | [x] |
| 10 | All links valid | file | reference-checker | succeeds | - | [x] |
| 11 | Invalid wave number error documented | file | Grep(SKILL.md) | contains | "Invalid wave" | [x] |

### AC Details

**AC#1-2**: Parallel/Inline mode branch logic
- This Feature introduces "Parallel Mode" and "Inline Mode" terminology to SKILL.md (new terminology)
- SKILL.md must document the branching logic based on argument count
- Single ID (|IDs|==1) → Inline Mode: orchestrator calls subagents directly
- Multiple IDs (|IDs|>1) → Parallel Mode: spawn Task() for each ID

**AC#3**: Task spawn pattern for parallel execution
- Each ID spawned via `Task(prompt: "Read .claude/skills/fl-workflow/SKILL.md and execute FL for Feature {ID}", run_in_background: true)`
- Enables Wave-level batch processing

**AC#4**: pending_user aggregation
- Parallel mode must collect pending_user from all spawned Tasks
- Present unified summary to user for decision

**AC#5-6**: wave1 syntax support
- `/fl wave1` → orchestrator expands to Wave 1 feature IDs from queue context
- Requires `/queue` to have been run in same conversation (Claude Code extracts wave data from conversation context)
- Expansion: `wave1` → `[wave1 IDs from /queue output in conversation]`
- **Syntax normalization**: Both `/fl wave1` and `/fl wave 1` (with space) are supported; orchestrator normalizes to canonical form. AC#5 verifies "wave1" presence which covers both input formats.

**AC#7**: Mode selection logic
- SKILL.md must document the ID count check for mode selection
- Test: Grep "ID count" to verify argument count logic is documented

**AC#8**: Wave prerequisite error
- SKILL.md must document the /queue dependency for wave syntax
- Error: "Wave data not found. Run /queue first."

**AC#9**: fl.md Usage table
- fl.md Usage table includes wave syntax example (e.g., `/fl wave1`)

**AC#10**: Links validation
- All markdown links in modified SKILL.md are valid
- Verified by reference-checker agent

**AC#11**: Invalid wave number error
- SKILL.md must document error handling for invalid wave numbers
- Covers: non-existent wave (wave99), zero/negative (wave0), malformed (waveX)
- Error: "Invalid wave: {input}. Valid waves: wave1, wave2, ..."

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add "Parallel Mode" section to SKILL.md | [x] |
| 2 | 2 | Add "Inline Mode" section to SKILL.md | [x] |
| 3 | 3 | Document Task spawn with run_in_background: true | [x] |
| 4 | 4 | Document pending_user aggregation | [x] |
| 5 | 5 | Document wave1 syntax formats (wave1, wave 1, wave2) | [x] |
| 6 | 6 | Document wave-to-IDs expansion algorithm | [x] |
| 7 | 7 | Document mode selection based on ID count | [x] |
| 8 | 8 | Document wave prerequisite (/queue dependency) | [x] |
| 9 | 9 | Add wave syntax example to fl.md Usage table | [x] |
| 10 | 10 | Run reference-checker for link validation | [x] |
| 11 | 11 | Document invalid wave number error handling | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Tasks 1-4,7 | Parallel/Inline mode, Task spawn, aggregation, mode selection |
| 2 | implementer | sonnet | Tasks 5-6,8,9,11 | wave syntax, wave expansion, prerequisite error, invalid wave error, fl.md Usage |
| 3 | reference-checker | haiku | Task 10 | Links validation |

**Note**: Integration test (actual /fl with multiple IDs) is post-implementation verification, not a pre-defined AC task.

### Parallel Mode Architecture

**Note**: Polling and aggregation are orchestrator behaviors (fl.md command logic), not SKILL.md content. ACs verify that SKILL.md **documents** what the orchestrator should do (e.g., AC#4 verifies aggregation is documented), but implementation logic lives in Claude Code.

```
/fl 529 531 534          ← Orchestrator parses: |IDs|=3 → Parallel Mode
  │
  ├→ Task(prompt: "Read .claude/skills/fl-workflow/SKILL.md and execute FL for Feature 529", run_in_background: true)
  │     └→ |IDs|=1 → Inline Mode → feature-reviewer, ac-validator...
  │     └→ Returns: {status: "REVIEWED"} or {status: "pending_user", issues: [...]}
  │
  ├→ Task(prompt: "Read .claude/skills/fl-workflow/SKILL.md and execute FL for Feature 531", run_in_background: true)
  │     └→ ... same pattern
  │
  └→ Task(prompt: "Read .claude/skills/fl-workflow/SKILL.md and execute FL for Feature 534", run_in_background: true)
        └→ ... same pattern

Orchestrator:
  - Poll output files for completion
  - Aggregate all pending_user issues
  - Present unified summary to user
```

### Wave Syntax Expansion

```
/fl wave1
  │
  ├→ Orchestrator checks context for /queue results
  ├→ Finds: Wave 1 = [IDs from /queue]
  ├→ Expands to: /fl [wave1 IDs]
  └→ Continues with Parallel Mode (|IDs| > 1)

Supported syntax:
  /fl wave1     → Wave 1 IDs
  /fl wave 1    → Wave 1 IDs (space allowed)
  /fl wave2     → Wave 2 IDs

Prerequisite: /queue must have been run in same conversation
Context mechanism: Claude Code extracts wave data from /queue output in conversation context
Error: "Wave data not found. Run /queue first."
```

---

## Review Notes

- **2026-01-18**: Created by splitting from F557 (AC count exceeded guideline)
- **2026-01-19**: [resolved] Wave context mechanism: B) Conversation context extraction (user must run /queue first in same conversation)
- **2026-01-19**: [resolved] Method column format: Keep `Grep(SKILL.md)` (user decision - explicit file path in AC table improves readability)
- **2026-01-19**: [resolved] Invalid wave number error: Added AC#11 and Task#11 (user decision - Zero Debt Upfront compliance)

---

## 引継ぎ先指定 (Mandatory Handoffs)

<!-- セクション削除: 課題なし -->

---

## Links

- [feature-557.md](feature-557.md) - FL Workflow Progressive Disclosure (predecessor)
- [fl.md](../../.claude/commands/fl.md) - FL command
- [fl-workflow](../../.claude/skills/fl-workflow/) - FL workflow skill (created by F557)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-01-19 02:44 | START | implementer | Phase 1: Tasks 1,2,3,4,7 | - |
| 2026-01-19 02:44 | END | implementer | Phase 1: Tasks 1,2,3,4,7 | SUCCESS |
| 2026-01-19 02:46 | START | implementer | Phase 2: Tasks 5,6,8,9,11 | - |
| 2026-01-19 02:46 | END | implementer | Phase 2: Tasks 5,6,8,9,11 | SUCCESS |
| 2026-01-19 02:48 | DEVIATION | Grep | AC#4 verification | "Aggregation" not contains "Aggregate" - fixed to "Aggregate pending_user" |
