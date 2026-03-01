# Feature 577: Workflow Dead Code Cleanup

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

Documentation must reflect actual implementation. Dead code references create confusion and maintenance debt.
Per SSOT principle: skills and commands must accurately represent current workflow state.

### Problem (Current Issue)

IMPLE_FEATURE_ID is no longer used by pre-commit (removed in F566) but workflow documentation still references it:
1. `.claude/skills/run-workflow/PHASE-1.md` creates `.git/IMPLE_FEATURE_ID`
2. `.claude/skills/run-workflow/PHASE-9.md` removes `.git/IMPLE_FEATURE_ID`
3. `.claude/commands/do.md` references IMPLE_FEATURE_ID creation and usage

This creates dead code that serves no functional purpose but adds maintenance overhead.

### Goal (What to Achieve)

Remove all references to IMPLE_FEATURE_ID from workflow documentation to eliminate dead code and ensure SSOT consistency.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | PHASE-1.md IMPLE_FEATURE_ID creation removed | file | Grep(.claude/skills/run-workflow/PHASE-1.md) | not_contains | "IMPLE_FEATURE_ID" | [x] |
| 2 | PHASE-9.md IMPLE_FEATURE_ID cleanup removed | file | Grep(.claude/skills/run-workflow/PHASE-9.md) | not_contains | "IMPLE_FEATURE_ID" | [x] |
| 3 | do.md IMPLE_FEATURE_ID references removed | file | Grep(.claude/commands/do.md) | not_contains | "IMPLE_FEATURE_ID" | [x] |
| 4 | PHASE-1.md retains valid phase structure | file | Grep(.claude/skills/run-workflow/PHASE-1.md) | contains | "# Phase 1" | [x] |
| 5 | do.md retains valid command structure | file | Grep(.claude/commands/do.md) | contains | "# /do Command" | [x] |

### AC Details

**AC#1**: Remove IMPLE_FEATURE_ID creation from PHASE-1.md
- File: `.claude/skills/run-workflow/PHASE-1.md`
- Line 71: `echo "{ID}" > "$(git rev-parse --show-toplevel)/.git/IMPLE_FEATURE_ID"`
- Action: Remove this command and any related context

**AC#2**: Remove IMPLE_FEATURE_ID cleanup from PHASE-9.md
- File: `.claude/skills/run-workflow/PHASE-9.md`
- Line 61: `rm -f "$(git rev-parse --show-toplevel)/.git/IMPLE_FEATURE_ID"`
- Action: Remove this command and any related context

**AC#3**: Remove IMPLE_FEATURE_ID references from do.md
- File: `.claude/commands/do.md`
- Line 260: Table entry mentioning "IMPLE_FEATURE_ID creation"
- Lines 323-326: IMPLE_FEATURE_ID creation documentation
- Line 329: Pre-commit signal explanation (dead documentation: "This file signals to pre-commit...")
- Action: Remove entire Step 1.2 subsection (lines 321-329) and table entry

**AC#4**: Phase structure preservation verification
- Verify PHASE-1.md retains its core phase structure ("# Phase 1") after IMPLE_FEATURE_ID removal
- Ensures cleanup doesn't accidentally remove essential documentation

**AC#5**: Command structure preservation verification
- Verify do.md retains its command header ("# /do Command") after IMPLE_FEATURE_ID removal
- Ensures cleanup doesn't accidentally remove essential command documentation

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Remove IMPLE_FEATURE_ID creation from PHASE-1.md | [x] |
| 2 | 2 | Remove IMPLE_FEATURE_ID cleanup from PHASE-9.md | [x] |
| 3 | 3 | Remove IMPLE_FEATURE_ID references from do.md | [x] |
| 4 | 4 | Verify PHASE-1.md retains valid phase structure after cleanup | [x] |
| 5 | 5 | Verify do.md retains valid command structure after cleanup | [x] |

---

## Implementation Contract

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| `.claude/skills/run-workflow/PHASE-1.md` | Remove IMPLE_FEATURE_ID creation | No functional impact (file not used by pre-commit) |
| `.claude/skills/run-workflow/PHASE-9.md` | Remove IMPLE_FEATURE_ID cleanup | No functional impact (file not created anymore) |
| `.claude/commands/do.md` | Remove IMPLE_FEATURE_ID documentation | Documentation accuracy improvement |

### Scope Boundary

**In scope**: Only PHASE-1.md, PHASE-9.md, and do.md are targeted for IMPLE_FEATURE_ID removal.

**Pre-conditions verified**: SKILL.md and run.md do NOT contain IMPLE_FEATURE_ID (confirmed via Grep).

**Out-of-scope handling**: If other files reference IMPLE_FEATURE_ID during implementation, report as out-of-scope issue per Scope Discipline and create follow-up feature.

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Workflow documentation restored to previous state
3. Create follow-up feature for proper fix

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F566 | [DONE] | Pre-commit CI Modernization (removed IMPLE_FEATURE_ID usage) |

---

## Review Notes

- [resolved] Phase1-Uncertain iter2: Sanity check issue identified (iter2-3).
- [resolved] Phase2-Maintainability iter4: Original AC#4/AC#5/Task#4/Task#5 (SKILL.md/run.md sanity checks) removed per AC:Task 1:1 principle. Moved to Implementation Contract as pre-conditions. Old AC#6/AC#7 renumbered to new AC#4/AC#5. Scope boundary and out-of-scope handling added.
- [skipped] Phase2-Maintainability iter8 (loop): Reviewer claims 'file' Type invalid, but validated as correct in iter5 per testing/SKILL.md AC Types table. User skipped (false positive).

---

## Mandatory Handoffs

<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Description | Tracking Destination |
|-------|-------------|---------------------|
| Out-of-scope IMPLE_FEATURE_ID reference | `Game/agents/reference/io-format-guide.md` line 120 still references IMPLE_FEATURE_ID. Per Implementation Contract, only workflow docs (PHASE-1.md, PHASE-9.md, do.md) are in scope. Reference docs are out-of-scope. | F579 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-21 08:48 | START | implementer | Task 1-3 | - |
| 2026-01-21 08:48 | END | implementer | Task 1-3 | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- [feature-566.md](feature-566.md) - Predecessor: Pre-commit CI Modernization