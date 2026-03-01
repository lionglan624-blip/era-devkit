# Feature 522: Context:Fork Dispatch Pattern Standardization

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

Skills are the Single Source of Truth. F519 established that Skills with context:fork eliminate lazy loading by guaranteeing knowledge auto-load at dispatch time. All critical workflow subagents should use the Skill() pattern to ensure consistent behavior and eliminate subagent execution failures due to missing knowledge.

### Problem (Current Issue)

PHASE-1.md and PHASE-9.md in run-workflow use inconsistent Task() dispatch pattern instead of the standardized Skill() pattern established in do.md. PHASE-1.md uses `Task(subagent_type: "general-purpose", model: "haiku", prompt: "Read .claude/agents/initializer.md")` and PHASE-9.md uses `Task(subagent_type: "finalizer", model: "haiku", prompt: "Read .claude/agents/finalizer.md")`. This creates workflow inconsistency and contradicts the F519 standardization.

### Goal (What to Achieve)

Standardize context:fork dispatch to Skill() pattern in PHASE-1.md and PHASE-9.md to match the pattern established in do.md. Replace Task() calls with Skill(initializer) and Skill(finalizer) to ensure consistent workflow behavior across all Phase files.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | PHASE-1 uses Skill pattern | file | Grep | contains | "Skill(initializer" | [x] |
| 2 | PHASE-9 uses Skill pattern | file | Grep | contains | "Skill(finalizer" | [x] |
| 3 | PHASE-1 Task pattern removed | file | Grep | not_contains | "Task.*general-purpose.*haiku" | [x] |
| 4 | PHASE-9 Task pattern removed | file | Grep | not_contains | "Task.*finalizer.*haiku" | [x] |
| 5 | PHASE-1 syntax valid | file | Grep | matches | "Skill\\(initializer,.*args.*\\{ID\\}" | [x] |
| 6 | PHASE-9 syntax valid | file | Grep | matches | "Skill\\(finalizer,.*args.*\\{ID\\}" | [x] |
| 7 | Exactly 1 initializer Skill dispatch | file | Grep | count_equals | 1 | [x] |
| 8 | Exactly 1 finalizer Skill dispatch | file | Grep | count_equals | 1 | [x] |
| 9 | Documentation consistency | manual | /audit | succeeds | - | [x] |

### AC Details

**AC#1**: PHASE-1.md Step 1.1 uses Skill(initializer) pattern
- Test: `Grep "Skill(initializer" .claude/skills/run-workflow/PHASE-1.md`
- Expected: Match found in dispatch section

**AC#2**: PHASE-9.md Step 9.1 uses Skill(finalizer) pattern
- Test: `Grep "Skill(finalizer" .claude/skills/run-workflow/PHASE-9.md`
- Expected: Match found in dispatch section

**AC#3**: Old Task pattern removed from PHASE-1
- Test: `Grep "Task.*general-purpose.*haiku" .claude/skills/run-workflow/PHASE-1.md`
- Expected: No matches found

**AC#4**: Old Task pattern removed from PHASE-9
- Test: `Grep "Task.*finalizer.*haiku" .claude/skills/run-workflow/PHASE-9.md`
- Expected: No matches found

**AC#5**: PHASE-1 syntax matches do.md pattern
- Test: `Grep "Skill\\(initializer,.*args.*\\{ID\\}" .claude/skills/run-workflow/PHASE-1.md`
- Expected: Correct syntax with args parameter

**AC#6**: PHASE-9 syntax matches do.md pattern
- Test: `Grep "Skill\\(finalizer,.*args.*\\{ID\\}" .claude/skills/run-workflow/PHASE-9.md`
- Expected: Correct syntax with args parameter

**AC#7**: Exactly one Skill(initializer, dispatch call in PHASE-1.md
- Test: `Grep "Skill(initializer," .claude/skills/run-workflow/PHASE-1.md` (output_mode: count)
- Expected: Count equals 1

**AC#8**: Exactly one Skill(finalizer, dispatch call in PHASE-9.md
- Test: `Grep "Skill(finalizer," .claude/skills/run-workflow/PHASE-9.md` (output_mode: count)
- Expected: Count equals 1

**AC#9**: All documentation cross-references valid
- Test: `/audit` command (manual verification)
- Expected: No broken links or inconsistencies

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,3,5,7 | Update PHASE-1.md Step 1.1 to use Skill(initializer) pattern | [x] |
| 2 | 2,4,6,8 | Update PHASE-9.md Step 9.1 to use Skill(finalizer) pattern | [x] |
| 3 | 9 | Verify documentation consistency with /audit (manual verification) | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Update PHASE-1.md dispatch pattern | Skill() syntax in Step 1.1 |
| 2 | implementer | sonnet | Update PHASE-9.md dispatch pattern | Skill() syntax in Step 9.1 |
| 3 | ac-tester | haiku | Verify AC#1-8 criteria | PASS/FAIL status |
| 4 | opus | - | Verify AC#9 (/audit) manually | Documentation consistency confirmed |

### Rollback Plan

Trivial documentation change (2 file text replacements in PHASE-1.md and PHASE-9.md). Per F521 precedent, git revert is sufficient for trivial doc changes.

---

## Impact Analysis

| File | Change | Impact |
|------|--------|--------|
| .claude/skills/run-workflow/PHASE-1.md | Replace Task() with Skill(initializer) | /run workflow Phase 1 uses context:fork dispatch |
| .claude/skills/run-workflow/PHASE-9.md | Replace Task() with Skill(finalizer) | /run workflow Phase 9 uses context:fork dispatch |

---

## Review Notes

- **2026-01-17 FL iter1**: [resolved] Phase2-Validate - Implementation Contract Phase 3: ac-tester cannot execute /audit (AC#9). Resolution: Split Phase 3 to AC#1-8, added Phase 4 for manual /audit verification.
- **2026-01-17 FL iter2**: [resolved] Phase2-Validate - Rollback Plan: Added to Implementation Contract per INFRA.md Issue 5. Documented F521 precedent: git revert sufficient for trivial doc changes.
- **2026-01-17 FL iter3**: [resolved] Phase3-Maintainability - AC#5/AC#6 regex pattern: `{ID}` braces escaped to `\\{ID\\}` for ripgrep compatibility.
- **2026-01-17 FL iter5**: [resolved] Phase2-Validate - AC#7/AC#8 pattern: Added comma `Skill(initializer,` to ensure only dispatch calls (not prose) are counted.
- **2026-01-17 FL iter5**: [resolved] Phase2-Validate - Dependencies table: F519 in Links is sufficient for traceability. feature-template.md doesn't require Dependencies table; adding it would be duplicative.
- **2026-01-17 FL iter6**: [resolved] Phase3-Maintainability - fl.md scope: F519 handoff mentions fl.md but fl.md has NO initializer/finalizer dispatch (Grep verified). F522 scope correctly limited to PHASE-1/9.md. Other agents in fl.md (reference-checker, etc.) are out of scope.

---

## Links

- [index-features.md](index-features.md)
- [feature-519.md](feature-519.md) - Context:fork Skills conversion
- [feature-521.md](feature-521.md) - Skill Documentation Completion (precedent for trivial changes)
- [PHASE-1.md](../../.claude/skills/run-workflow/PHASE-1.md) - Target file 1
- [PHASE-9.md](../../.claude/skills/run-workflow/PHASE-9.md) - Target file 2
- [do.md](../../.claude/commands/do.md) - Reference pattern