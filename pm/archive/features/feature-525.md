# Feature 525: Rename and update feature-builder rules file

## Status: [WIP]

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
Single Source of Truth (SSOT) principle ensures that documentation accurately reflects current implementation patterns and references correct agents/skills. When implementation patterns change (feature-builder agent → Skill(feature-creator)), all documentation must be updated to maintain consistency and prevent confusion.

### Problem (Current Issue)
The file is named `feature-builder.md` with title "Feature Builder Quality Rules" but its purpose is feature-quality validation rules. The naming is confusing and inconsistent with the current pattern where feature-builder is deprecated.

### Goal (What to Achieve)
Rename the file to `.claude/rules/feature-quality.md` and update the title to "Feature Quality Rules". Ensure naming consistency across documentation.

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| .claude/rules/feature-builder.md | Renamed to feature-quality.md | Rules auto-load triggers update |
| .claude/rules/feature-quality.md | Title updated | Title changed from "Feature Builder" to "Feature Quality" |
| .claude/agents/_archived/feature-builder.md | Contains old path ref (line 137) | Exempt - archived file preserves historical context |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Old file removed | file | Glob | not_exists | .claude/rules/feature-builder.md | [x] |
| 2 | New file created | file | Glob | exists | .claude/rules/feature-quality.md | [x] |
| 3 | Title updated | file | Grep(.claude/rules/feature-quality.md) | contains | "# Feature Quality Rules" | [x] |
| 4 | Old title removed | file | Grep(.claude/rules/feature-quality.md) | not_contains | "Feature Builder" | [x] |
| 5 | YAML frontmatter preserved | file | Grep(.claude/rules/feature-quality.md) | contains | "---" | [x] |
| 6 | Paths configuration preserved | file | Grep(.claude/rules/feature-quality.md) | contains | "paths:" | [x] |
| 7 | No stale references in docs | file | Grep(.claude/) | not_contains | "rules/feature-builder.md" | [x] |

### AC Details

**AC#1-2**: File rename operation
- Test: `Glob` verification for old/new file existence
- Verifies: Complete migration from old to new filename

**AC#3**: Title reflects current purpose
- Test: `Grep "# Feature Quality Rules"` in new file
- Verifies: Title heading updated from "Feature Builder" to "Feature Quality"

**AC#4**: Old title removed
- Test: `Grep "Feature Builder"` (expect 0 matches in new file)
- Verifies: Title no longer contains "Feature Builder" (changed to "Feature Quality")

**AC#5-6**: Configuration preserved
- Test: `Grep` for YAML frontmatter structure
- Verifies: File automation settings maintained

**AC#7**: Stale reference check
- Test: `Grep "rules/feature-builder.md"` in active documentation
- Scope: `.claude/` (excluding `_archived/`) and `Game/agents/*.md` (excluding `feature-*.md` historical context)
- Target: CLAUDE.md, skills/, commands/, rules/, index-features.md
- Exempt: Predecessor features (F524) preserve historical handoff state; archived files preserve historical context
- Verifies: No stale path references in active documentation after rename

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-6 | Rename file and update title (atomic edit operation) | [x] |
| 2 | 7 | Verify no stale path references remain | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | File rename and title update | Updated feature-quality.md |
| 2 | ac-tester | haiku | AC verification | All ACs pass |

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback

---

## Links

- [index-features.md](index-features.md)
- [feature-524.md](feature-524.md) - F524 handoff (引継ぎ先)
- [.claude/rules/feature-quality.md](../../.claude/rules/feature-quality.md) - Target file (post-implementation)

---

## Dependencies

| Type | Feature | Relationship |
|------|---------|--------------|
| Predecessor | F524 | Handoff source |

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| (none at creation time) | - | - | - |

## Review Notes
- **2026-01-18 FL iter2**: [resolved] Phase2-Validate - AC#5/Task#3: File has "Feature Builder" in title, not "feature-builder" references. AC#5 pattern may need adjustment.
- **2026-01-18 FL iter2**: [resolved] Phase2-Validate - Problem description: Claims "content referencing deprecated feature-builder agent" but file only has title reference.
- **2026-01-18 FL iter2**: [applied] Phase2-Validate - AC#4: Whether Skill(feature-creator) reference should be added is debatable.
- **2026-01-18 FL iter2**: [applied] Phase3-Maintainability - Confirms above: rules file never referenced agent. AC#4 and Task#3 solve non-existent problem.
- **2026-01-18 FL iter3**: [applied] Phase2-Validate - AC#4: CRITICAL - Remove AC#4 (adding Skill reference). Rules file is implementation-agnostic.
- **2026-01-18 FL iter3**: [applied] Phase2-Validate - Task#3: CRITICAL - Remove Task#3 (replace agent refs). No agent references exist to replace.
- **2026-01-18 FL iter3**: [skipped] Phase2-Validate - AC#5: uncertain - Description 'Old title removed' is imprecise but matcher correct. Current acceptable.
- **2026-01-18 FL iter3**: [skipped] Phase2-Validate - Impact Analysis: uncertain - Statement technically correct but slightly confusing.
- **2026-01-18 FL iter5**: [applied] Phase2-Validate - AC#7: uncertain - 'manual' type not in testing SKILL, Expected '-' ambiguous.
- **2026-01-18 FL iter5**: [skipped] Phase2-Validate - AC#5: uncertain - 'contains ---' pattern weak but adequate for rules file.