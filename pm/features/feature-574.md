# Feature 574: Fix /fl Progressive Disclosure Bypass

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

## Created: 2026-01-20

---

## Summary

Remove `requires:` frontmatter from fl-workflow/SKILL.md and verify existing phase chain integrity to restore progressive phase loading behavior.

---

## Background

### Philosophy (Mid-term Vision)

**Progressive Disclosure** prevents orchestrator context dilution by loading rules incrementally. When all phases are loaded at once, the orchestrator's attention becomes diffused across multiple phases worth of instructions.

### Problem (Current Issue)

`fl-workflow/SKILL.md` contains a YAML frontmatter `requires:` block that lists all phases. This defeats the progressive disclosure design where each phase file ends with `Read(.claude/skills/fl-workflow/PHASE-N.md)` to load the next phase incrementally.

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| fl-workflow/SKILL.md | Remove requires: frontmatter | Skill tool loads only SKILL.md initially, phases load on demand |

**Note**: SKILL.md Start section already contains Read instruction for PHASE-0. No additions needed, only removal of requires: block.

### Goal (What to Achieve)

1. Remove `requires:` frontmatter from fl-workflow/SKILL.md
2. Ensure phase chaining works correctly (each phase loads next)

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SKILL.md YAML frontmatter has no requires | code | Grep(.claude/skills/fl-workflow/SKILL.md) | count_equals | 0 (pattern: ^requires:) | [x] |
| 2 | SKILL.md Start section instructs to read PHASE-0 (not other phases) | code | Grep(.claude/skills/fl-workflow/SKILL.md) | contains | "Read(.claude/skills/fl-workflow/PHASE-0.md)" | [x] |
| 3 | All 8 phase files (0-7) exist | file | Glob(.claude/skills/fl-workflow/PHASE-*.md) | count_equals | 8 | [x] |
| 4 | All links valid | tool | reference-checker | succeeds | - | [x] |
| 5 | PHASE-7 has terminal state | code | Grep(.claude/skills/fl-workflow/PHASE-7.md) | contains | "POST-LOOP" | [x] |
| 6 | Progressive disclosure documented | code | Grep(.claude/skills/fl-workflow/SKILL.md) | contains | "Progressive Disclosure" | [x] |
| 7 | fl-workflow dynamic routing preserved | code | Grep(.claude/skills/fl-workflow/SKILL.md) | contains | "Routing Table" | [x] |
| 8 | POST-LOOP.md file exists for terminal state handling | file | Glob | exists | ".claude/skills/fl-workflow/POST-LOOP.md" | [x] |
| 9 | PHASE-0 through PHASE-6 contain Read instructions for phase chaining | code | Grep(.claude/skills/fl-workflow/PHASE-*.md) | count_equals | 7 | [x] |

### AC Details

**AC#1**: `requires:` frontmatter removed from SKILL.md
- Test: `Grep "^requires:" .claude/skills/fl-workflow/SKILL.md` (regex start-of-line)
- Expected: No matches (comment containing "requires:" is allowed, only YAML key is checked)

**AC#2**: SKILL.md only instructs reading PHASE-0, not all phases
- Test: `Grep "Read(.claude/skills/fl-workflow/PHASE-0.md)" .claude/skills/fl-workflow/SKILL.md`
- Expected: Contains instruction to read PHASE-0.md only

**AC#3**: All 8 phase files (0-7) exist
- Test: `Glob .claude/skills/fl-workflow/PHASE-*.md`
- Expected: 8 files (PHASE-0.md through PHASE-7.md)

**AC#4**: All links valid
- Test: reference-checker execution on all modified files
- Expected: No broken links

**AC#5**: PHASE-7 has terminal state
- Test: `Grep "POST-LOOP" .claude/skills/fl-workflow/PHASE-7.md`
- Expected: Contains POST-LOOP reference for clean chain termination

**AC#6**: Progressive disclosure documented
- Test: `Grep "Progressive Disclosure" .claude/skills/fl-workflow/SKILL.md`
- Expected: Contains documentation of the pattern

**AC#7**: fl-workflow dynamic routing preserved
- Test: `Grep "Routing Table" .claude/skills/fl-workflow/SKILL.md`
- Expected: Contains routing table for conditional phase transitions

**AC#8**: POST-LOOP terminal state exists
- Test: `Glob .claude/skills/fl-workflow/POST-LOOP.md`
- Expected: File exists

**AC#9**: PHASE-0 through PHASE-6 chain to next phase
- Test: `Grep "Read\\(.claude/skills/fl-workflow/PHASE-" .claude/skills/fl-workflow/PHASE-*.md`
- Expected: 7 matches (PHASE-0 through PHASE-6 contain Read instructions for next PHASE)


---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Remove `requires:` frontmatter from fl-workflow/SKILL.md | [x] |
| 2 | 2 | Verify SKILL.md PHASE-0 Read instruction preserved | [x] |
| 3 | 3 | Verify all 8 phase files exist | [x] |
| 4 | 4 | Run reference-checker to validate all links | [x] |
| 5 | 5 | Verify PHASE-7 has terminal state (POST-LOOP reference) | [x] |
| 6 | 6 | Verify progressive disclosure documented in SKILL.md | [x] |
| 7 | 7 | Verify fl-workflow dynamic routing preserved | [x] |
| 8 | 8 | Verify POST-LOOP terminal state file exists | [x] |
| 9 | 9 | Verify PHASE-0 through PHASE-6 chain to next phase | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 (impl) | implementer | sonnet | SKILL.md | Modified SKILL.md without requires |
| 2 (verify) | reference-checker | haiku | All modified files | Link validation |

---

## Rollback Plan

1. git revert to restore requires: frontmatter
2. Verify Skill tool still works

---

## Review Notes

- [resolved] Phase2-Maintainability: Pre-verification semantics for Task#2/Task#3 not explicitly defined in feature-template.md. Task#2/Task#3 reset to [ ] for consistency with standard workflow where tasks are verified during implementation.
- [skipped] Phase2-Maintainability iter5: AC#9 semantic concern - reviewer suggests phase files use dynamic placeholders {X} not explicit numbers, making count_equals 7 misleading. However, AC#9 technically validates that 7 phase files contain Read instruction pattern. AC#5 already covers PHASE-7→POST-LOOP chain. User decision: Skip (stylistic concern, AC is technically correct).

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F560 | [DONE] | Handoff source: Progressive Disclosure fix for fl-workflow |

---

## Links

- [fl-workflow/SKILL.md](../../.claude/skills/fl-workflow/SKILL.md) - Target file
- [feature-560.md](feature-560.md) - Related: run-workflow Progressive Disclosure fix

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-20 | create | Opus | Created from F560 FL review handoff resolution | PROPOSED |
| 2026-01-20 | note | Opus | Pre-verified PHASE-0 Read instruction exists in SKILL.md (will be formally verified during implementation) | - |
| 2026-01-21 | init | initializer | WIP transition | [WIP] |
| 2026-01-21 | verify | Opus | Found uncommitted changes from prior session; AC#1-9 all PASS | READY |
| 2026-01-21 | complete | Opus | All verifications passed; committed 3ab7073 | [DONE] |
