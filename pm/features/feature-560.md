# Feature 560: Fix /run Progressive Disclosure Bypass

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

## Created: 2026-01-18

---

## Summary

Remove `requires:` frontmatter from run-workflow/SKILL.md to restore progressive phase loading behavior.

---

## Background

### Philosophy (Mid-term Vision)

**Progressive Disclosure** prevents orchestrator context dilution by loading rules incrementally. When all phases are loaded at once, the orchestrator's attention becomes diffused across 9 phases worth of instructions, causing it to miss or deprioritize critical phase-specific rules.

### Problem (Current Issue)

`run-workflow/SKILL.md` contains a YAML frontmatter `requires:` block that lists all 9 phases:

```yaml
requires:
  - run-workflow/PHASE-1
  - run-workflow/PHASE-2
  ...
  - run-workflow/PHASE-9
```

If the Skill loader processes this `requires:` directive, ALL phases are loaded when `Skill(run-workflow)` is called. This defeats the progressive disclosure design where each phase file ends with `Read(.claude/skills/run-workflow/PHASE-N.md)` to load the next phase incrementally.

### Goal (What to Achieve)

1. Ensure run-workflow skill loads phases progressively (one phase at a time) rather than all at once
2. If yes, remove `requires:` frontmatter to restore progressive loading
3. Ensure each phase file's "Next" section properly chains to the next phase
4. Document the correct Progressive Disclosure pattern for future skill creators

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SKILL.md has no requires frontmatter | code | Grep(.claude/skills/run-workflow/SKILL.md) | not_contains | "requires:" | [x] |
| 2 | SKILL.md instructs to read PHASE-1 only | code | Grep(.claude/skills/run-workflow/SKILL.md) | matches | "Read.*PHASE-1" | [x] |
| 3 | PHASE-1 chains to PHASE-2 | code | Grep(.claude/skills/run-workflow/PHASE-1.md) | matches | "Read.*PHASE-2" | [x] |
| 4 | PHASE-8 chains to PHASE-9 | code | Grep(.claude/skills/run-workflow/PHASE-8.md) | matches | "Read.*PHASE-9" | [x] |
| 5 | PHASE-1 through PHASE-8 have Next section | code | Grep(.claude/skills/run-workflow/PHASE-*.md) | count_equals | 8 | [x] |
| 6 | requires anti-pattern documented in CLAUDE.md | code | Grep(CLAUDE.md) | contains | "requires: frontmatter defeats Progressive Disclosure" | [x] |

### AC Details

**AC#1**: `requires:` frontmatter removed from SKILL.md
- Test: `Grep "requires:" .claude/skills/run-workflow/SKILL.md`
- Expected: No matches

**AC#2**: SKILL.md only instructs reading PHASE-1, not all phases
- Test: `Grep "Read.*PHASE-1" .claude/skills/run-workflow/SKILL.md`
- Expected: Contains instruction

**AC#3-4**: Phase chaining verified
- Test: Each PHASE-N.md has "Read PHASE-(N+1).md" instruction
- Expected: Proper chain from 1→2→...→9

**AC#5**: PHASE-1 through PHASE-8 have Next section for chaining
- Test: Count `## Next` sections across PHASE-1.md through PHASE-8.md files in .claude/skills/run-workflow/
- Expected: 8 matches (one per file, PHASE-9 excluded as final phase)

**AC#6**: CLAUDE.md documents the anti-pattern
- Test: `Grep "requires: frontmatter defeats Progressive Disclosure" CLAUDE.md`
- Expected: Contains the exact warning text "requires: frontmatter defeats Progressive Disclosure"

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Remove `requires:` frontmatter from SKILL.md (includes verification that Read PHASE-1 instruction exists) | [x] |
| 2 | 3-5 | Verify all phase files chain correctly | [x] |
| 3 | 6 | Document pattern in CLAUDE.md Skills section | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | SKILL.md | Modified SKILL.md without requires |
| 2 | implementer | sonnet | CLAUDE.md | Updated Skills documentation |
| 3 | reference-checker | haiku | All modified files | Link validation |

---

## Review Notes

[applied] Handoff inconsistency: Created F574 for fl-workflow requires: removal. Updated handoff destination from F561 to F574.

[applied] Investigation task: Clarified Task 1 description - removed "after investigation", added "(includes verification that Read PHASE-1 instruction exists)".

[applied] AC#6 pattern: Changed from regex 'requires.*Progressive Disclosure' to concrete text 'requires: frontmatter defeats Progressive Disclosure'.

---

## Dependencies

| Type | Feature | Description |
|------|---------|-------------|
| - | - | - |

---

## Links

- [run-workflow/SKILL.md](../../.claude/skills/run-workflow/SKILL.md) - Target file
- [feature-557.md](feature-557.md) - Related: FL Progressive Disclosure (different command)
- [feature-540.md](feature-540.md) - Originally tracked in 引継ぎ先指定
- [feature-574.md](feature-574.md) - Handoff destination for fl-workflow requires: removal

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 詳細 | 種別 | 引継先 |
|------|------|------|--------|
| fl-workflow requires: removal | Same `requires:` pattern exists in fl-workflow/SKILL.md lines 6-16. Same Progressive Disclosure issue | Feature | F574 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | Opus | Created from F540 handoff analysis | PROPOSED |
| 2026-01-20 12:09 | END | implementer | Task 3: Document pattern in CLAUDE.md | SUCCESS |
| 2026-01-20 12:10 | DEVIATION | reference-checker | AC#6 backtick mismatch detected | NEEDS_REVISION |
| 2026-01-20 12:10 | FIX | Opus | Removed backticks from CLAUDE.md line 225 | SUCCESS |
| 2026-01-20 12:15 | DEVIATION | feature-reviewer | Task status inconsistency | NEEDS_REVISION |
| 2026-01-20 12:15 | FIX | Opus | Updated Task 1,2 status to [x] | SUCCESS |
