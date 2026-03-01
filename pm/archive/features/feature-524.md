# Feature 524: Update feature-template.md to reference Skill(feature-creator)

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
Documentation consistency ensures Single Source of Truth (SSOT) across the codebase. When workflow changes occur, all references must be updated to maintain accuracy and prevent confusion for future development. Removing deprecated code and references aligns with the Zero Debt Upfront principle - paying the cleanup cost now eliminates future technical debt and prevents developers from accidentally using outdated patterns.

### Problem (Current Issue)
`feature-template.md` still references the deprecated `feature-builder` agent:
- Line 52: "Feature ファイル作成には feature-builder を使用すること"
- Line 57: Agent Selection table shows `feature-{ID}.md | **feature-builder**`

Note: `implementer.md` and `.claude/agents/feature-builder.md` have already been updated/removed in prior work. Only `feature-template.md` requires updates.

### Goal (What to Achieve)
Update `feature-template.md` to reference `Skill(feature-creator)` instead of the deprecated `feature-builder` agent, completing SSOT cleanup for the feature-builder deprecation.

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| feature-template.md | Update to Skill(feature-creator) | All future feature creation follows new pattern |
| implementer.md | (already done) | - |
| .claude/agents/feature-builder.md | (already removed) | - |
| CLAUDE.md | (no change) | Already updated |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | feature-template.md updated | file | Grep | contains | "Skill\\(feature-creator\\)" | [x] |
| 2 | feature-template.md no feature-builder | file | Grep | not_contains | "feature-builder" | [x] |
| 3 | Documentation consistency verified | manual | /audit | succeeds | - | [x] |
| 4 | All links valid | manual | Skill(reference-checker) | succeeds | - | [x] |

### AC Details

**AC#1**: feature-template.md contains Skill(feature-creator) reference
- Test: `Grep "Skill\\(feature-creator\\)" Game/agents/reference/feature-template.md`
- Verifies: Template shows correct modern workflow

**AC#2**: feature-template.md has no feature-builder references
- Test: `Grep "feature-builder" Game/agents/reference/feature-template.md`
- Expected: No matches (not_contains)

**AC#3**: Documentation consistency verified across codebase
- Method: `/audit` command to check SSOT consistency
- Expected: Command succeeds (exit code 0)

**AC#4**: All markdown links resolve correctly
- Method: `Skill(reference-checker)` (manual verification)
- Expected: Tool reports success with no broken links

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Update feature-template.md Agent Selection section | [x] |
| 2 | 3 | Run documentation audit for consistency | [x] |
| 3 | 4 | Validate all links still resolve | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Update feature-template.md | File updated |
| 2 | reference-checker | haiku | Validate all links | Link validation report |

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Rename and update `.claude/rules/feature-builder.md` | Rules file naming/content inconsistent with deprecated agent | Feature | F525 |

---

## Review Notes
- **2026-01-18 FL iter1**: [applied] Phase2-Validate - AC#1: Parentheses escaped per user approval.
- **2026-01-18 FL iter1**: [applied] Phase2-Validate - AC#3: Parentheses escaped per user approval.
- **2026-01-18 FL iter2**: [applied] Phase2-Validate - 引継ぎ先指定 updated to include content update scope.
- **2026-01-18 FL iter2**: [applied] Phase3-Maintainability - Added Zero Debt Upfront principle to Philosophy.
- **2026-01-18 FL iter2**: [resolved] Phase8-Handoff - F525 created. Handoff tracking satisfied.
- **2026-01-18 FL iter3**: [applied] Phase1-Review - Scope reduced: implementer.md and feature-builder.md already done. Focus on feature-template.md only.
- **2026-01-18 FL iter4**: [applied] Post-loop - AC#3 removed per user approval (redundant with AC#1+AC#2). ACs renumbered.
- **2026-01-18 FL iter4**: [applied] Phase4-ACValidation - AC#4 Expected changed to "-" (binary judgment by exit code).

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 08:55 | START | implementer | Task 1 | - |
| 2026-01-18 08:55 | END | implementer | Task 1 | SUCCESS |
| 2026-01-18 09:00 | START | /audit | Task 2 | - |
| 2026-01-18 09:00 | END | /audit | Task 2 | SUCCESS |
| 2026-01-18 09:00 | START | reference-checker | Task 3 | - |
| 2026-01-18 09:00 | END | reference-checker | Task 3 | SUCCESS |

## Links
- [index-features.md](index-features.md)
- [feature-template.md](reference/feature-template.md)
- [feature-525.md](feature-525.md) - Handoff: rename rules file