# Feature 527: Fix run-workflow phase skill references

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
Progressive Disclosure workflow should work seamlessly without errors. Each Phase transition should use a consistent, working mechanism that doesn't require workarounds.

### Problem (Current Issue)
PHASE-N.md files reference `Skill(run-workflow/PHASE-N)` for next phase navigation, but this syntax returns "Unknown skill" error. The Skill tool doesn't support nested paths with `/`. Users must manually invoke `Read` tool with the phase file path as a workaround, breaking the Progressive Disclosure promise.

### Goal (What to Achieve)
Update all PHASE-N.md files to use `Read` tool syntax instead of non-working `Skill()` syntax for phase navigation. Document the correct pattern in run-workflow SKILL.md.

### Impact Analysis

| File/Component | Change | Refs | Impact |
|----------------|--------|:----:|--------|
| .claude/skills/run-workflow/SKILL.md | Update overview table + Start section | 10 | Clearer instructions |
| .claude/skills/run-workflow/PHASE-1.md | Update "Next" section | 1 | Working navigation |
| .claude/skills/run-workflow/PHASE-2.md | Update "Type Routing" + "Next" sections | 4 | Working navigation (kojo skip + type routing) |
| .claude/skills/run-workflow/PHASE-3.md | Update "Type Routing" + "Next" sections | 3 | Working navigation (kojo/infra skip) |
| .claude/skills/run-workflow/PHASE-4.md | Update "Next" section (2 paths) | 2 | Working navigation (kojo→5, others→6) |
| .claude/skills/run-workflow/PHASE-5.md | Update "Applicability" + "Next" sections | 2 | Working navigation (other→skip) |
| .claude/skills/run-workflow/PHASE-6.md | Update "Next" section | 1 | Working navigation |
| .claude/skills/run-workflow/PHASE-7.md | Update "Next" section | 1 | Working navigation |
| .claude/skills/run-workflow/PHASE-8.md | Update "Next" section | 1 | Working navigation |
| .claude/skills/run-workflow/PHASE-9.md | No change needed | 0 | Final phase (no navigation) |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SKILL.md uses Read syntax | file | Grep | contains | "Read.*PHASE-1" | [x] |
| 2 | No broken Skill() in SKILL.md | file | Grep | not_contains | "Skill(run-workflow/" | [x] |
| 3 | PHASE-1 uses Read for next | file | Grep | contains | "Read.*PHASE-2" | [x] |
| 4 | PHASE-2 uses Read for next | file | Grep | contains | "Read.*PHASE-3" | [x] |
| 5 | PHASE-3 uses Read for next | file | Grep | contains | "Read.*PHASE-4" | [x] |
| 6a | PHASE-4 kojo path uses Read for next | file | Grep | contains | "Read.*PHASE-5" | [x] |
| 6b | PHASE-4 non-kojo path uses Read for next | file | Grep | contains | "Read.*PHASE-6" | [x] |
| 7 | PHASE-5 uses Read for next | file | Grep | contains | "Read.*PHASE-6" | [x] |
| 8 | PHASE-6 uses Read for next | file | Grep | contains | "Read.*PHASE-7" | [x] |
| 9 | PHASE-7 uses Read for next | file | Grep | contains | "Read.*PHASE-8" | [x] |
| 10 | PHASE-8 uses Read for next | file | Grep | contains | "Read.*PHASE-9" | [x] |
| 11 | No broken Skill() in any PHASE file | file | Grep | not_contains | "Skill(run-workflow/PHASE-" | [x] |

### AC Details

**AC#1-2**: Main skill file updated
- Test: `Grep` in SKILL.md
- Verifies: Entry point uses correct Read syntax
- Note: SKILL.md overview table contains `Skill(run-workflow/PHASE-N)` references as documentation. These should also be updated to Read syntax to match AC#2.

**AC#3-10**: Individual phase files updated
- Test: `Grep` in each PHASE-N.md
- Verifies: Next phase navigation uses Read

**AC#11**: No broken syntax remains
- Test: `Grep "Skill(run-workflow/PHASE-"` across all files
- Verifies: All broken `Skill()` calls removed

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-2 | Update SKILL.md navigation instructions | [x] |
| 2 | 3-11 | Update all PHASE-N.md navigation sections to use Read (incl. Type Routing/Applicability inline refs) | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Update all files | Updated files |
| 2 | ac-tester | haiku | AC verification | All ACs pass |

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback

---

## Review Notes

- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - AC#6: PHASE-4.md has two destinations (PHASE-5 for kojo, PHASE-6 for others). Pattern `Read.*PHASE-` may be too vague - should both destinations be verified separately? → Split into AC#6a and AC#6b
- **2026-01-18 FL iter2**: [resolved] Phase2-Validate - AC#3-10 patterns: 'Read.*PHASE-N' are broad. More specific patterns like 'Read.*run-workflow/PHASE-N' would be precise but current patterns work in context (each AC verifies within specific files). → Single-file scope acceptable; no change needed.
- **2026-01-18 FL iter3**: [resolved] Phase2-Validate - Task#2 detailed listing: Reviewer suggests explicit listing of all inline ref locations in Task#2. However, Impact Analysis table (lines 32-43) already provides file-by-file detail. Duplicating in Task#2 may violate DRY. → Accepted: Impact Analysis is sufficient, no duplication in Task#2 needed.
- **2026-01-18 FL iter4**: [resolved] Phase2-Validate - Task#1 description: Could explicitly mention 'Overview table (9 refs) + Start section (1 ref)' but Impact Analysis already provides detail. Judgment call on DRY vs explicit. → Accepted: Impact Analysis table is SSOT for ref counts; no duplication in Task#1 needed.
- **2026-01-18 FL iter4**: [resolved] Phase2-Validate - AC#2 and AC#11 overlap: AC#2 checks SKILL.md, AC#11 says 'across all files' but contextually targets PHASE files. Overlap exists but is harmless (double-verification). → Accepted: Defense-in-depth; minimal overlap, different target files.

---

## Links

- [index-features.md](index-features.md)
- [feature-525.md](feature-525.md) - Predecessor (run-workflow review source)
- [run-workflow SKILL](../../.claude/skills/run-workflow/SKILL.md) - Target files

---

## Dependencies

| Type | Feature | Relationship |
|------|---------|--------------|
| Predecessor | F525 | Review feedback source |

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| (none at creation time) | - | - | - |
