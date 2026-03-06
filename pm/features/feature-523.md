# Feature 523: Update run-workflow to use Skill(feature-creator) instead of deprecated feature-builder

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
Subagent workflow consistency across all skills - skills should reference the current agent dispatch patterns and avoid deprecated methods. This ensures SSOT (Single Source of Truth) for agent dispatch mechanisms and prevents confusion between old and new patterns.

### Problem (Current Issue)
The run-workflow skill contains references to the deprecated `feature-builder` subagent in two locations:
1. Type Routing table (line 71: `| research | feature-builder | Static verification |`) maps research type to deprecated feature-builder
2. Phase 4 documentation (line 53: `For research creating sub-features, use feature-builder instead.`) suggests using deprecated feature-builder

feature-builder is deprecated per CLAUDE.md Subagent Strategy (replaced by `Skill(feature-creator)` with context:fork isolation). This feature completes the migration to Skill(feature-creator) in run-workflow.

### Goal (What to Achieve)
Update run-workflow skill documentation to replace feature-builder references with Skill(feature-creator), ensuring consistency with F522's context:fork standardization and current best practices.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Type routing table updated | file | Grep | contains | "research.*Skill(feature-creator)" | [x] |
| 2 | No feature-builder references in Type table | file | Grep | not_contains | "research.*feature-builder" | [x] |
| 3 | Phase 4 documentation updated | file | Grep | contains | "Skill(feature-creator)" | [x] |
| 4 | No feature-builder references in Phase 4 | file | Grep | not_contains | "feature-builder instead" | [x] |
| 5 | SKILL.md requires block intact | file | Grep | contains | "requires:" | [x] |
| 6 | All markdown links valid | manual | reference-checker | succeeds | - | [x] |
| 7 | Documentation consistency verified | manual | /audit | succeeds | - | [x] |

### AC Details

**AC#1**: Type Routing table uses Skill(feature-creator)
- Test: `Grep "research.*Skill(feature-creator)" .claude/skills/run-workflow/SKILL.md`
- Expected: Type Routing table shows `| research | Skill(feature-creator) | Static verification |`

**AC#2**: No deprecated feature-builder in Type table
- Test: `Grep "research.*feature-builder" .claude/skills/run-workflow/SKILL.md`
- Expected: No matches (deprecated pattern removed)

**AC#3**: Phase 4 suggests Skill(feature-creator)
- Test: `Grep "Skill(feature-creator)" .claude/skills/run-workflow/PHASE-4.md`
- Expected: Type: infra/research section mentions Skill(feature-creator) for research sub-features

**AC#4**: No deprecated feature-builder in Phase 4
- Test: `Grep "feature-builder instead" .claude/skills/run-workflow/PHASE-4.md`
- Expected: No matches (deprecated pattern removed)

**AC#5**: SKILL.md requires block intact
- Test: `Grep "requires:" .claude/skills/run-workflow/SKILL.md`
- Ensures skill file structure remains valid after edits

**AC#6**: All internal and external links resolve correctly
- Verifies no broken links introduced by documentation changes

**AC#7**: SSOT consistency across all documentation
- Ensures changes are consistent with overall documentation

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,5 | Update Type Routing table in SKILL.md: research row to use Skill(feature-creator) | [x] |
| 2 | 3,4 | Update Type: infra/research section in PHASE-4.md: use Skill(feature-creator) | [x] |
| 3 | 6,7 | Verify documentation consistency and link integrity | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Update SKILL.md Type table and PHASE-4.md text | Updated files |
| 2 | reference-checker | haiku | Validate all links in modified files | Link validation report |
| 3 | - | - | Run /audit for SSOT consistency | Audit results |

---

## Dependencies

| Type | Feature | Relationship |
|------|---------|--------------|
| Predecessor | F522 | Context:fork standardization (PHASE-1/9.md patterns) |

---

## Links

- [index-features.md](index-features.md)
- [feature-522.md](feature-522.md) - Context:fork standardization
- [feature-524.md](feature-524.md) - Follow-up feature for template/agent cleanup
- [run-workflow skill](../../.claude/skills/run-workflow/) - Target files for update

---

## Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| run-workflow/SKILL.md | Type table line 71 | /run command Type routing for research features |
| run-workflow/PHASE-4.md | Documentation line 53 | Phase 4 agent selection guidance |
| All /run executions | Indirect | Research features will use Skill(feature-creator) instead of feature-builder |

---

## Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Restore feature-builder references in both files
3. Notify user of rollback
4. Create follow-up feature for proper fix

---

## 引継ぎ先指定 (Mandatory Handoffs)

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| feature-template.md also references deprecated feature-builder (lines 52, 57) | Out of scope - different file | Feature | F524 |
| implementer.md references deprecated feature-builder (lines 70, 74) | Out of scope - agent file not skill | Feature | F524 |
| .claude/agents/feature-builder.md deprecated agent file removal | Out of scope - file deletion | Feature | F524 |

---

## Review Notes

- **2026-01-17 FL iter1**: [resolved] Removed AC#5/AC#6 - SKILL.md has no YAML frontmatter, changed AC#5 to check requires: block
- **2026-01-17 FL iter1**: [resolved] Added feature-template.md to 引継ぎ先指定 (F524)
- **2026-01-17 FL iter2**: [resolved] Fixed Background - F522 claim incorrect, updated to reference CLAUDE.md deprecation
- **2026-01-17 FL iter2**: [resolved] AC#1/AC#3 patterns escaped for regex compatibility
- **2026-01-17 FL iter2**: [resolved] AC#6/AC#7 Type changed to manual (not file)
- **2026-01-17 FL iter2**: [resolved] 引継ぎ先指定 table added 理由 column
- **2026-01-17 FL iter2**: [note] AC count 7 acceptable for simple doc update (close to 8 minimum)
- **2026-01-17 FL iter3**: [resolved] Added implementer.md (lines 70, 74) to 引継ぎ先指定 (F524)
- **2026-01-17 FL iter4**: [resolved] Fixed Dependencies table - F522 did not deprecate feature-builder
- **2026-01-17 FL iter5**: [resolved] AC#3 pattern simplified to "Skill\\(feature-creator\\)" for flexibility
- **2026-01-17 FL iter5**: [resolved] Problem section line references made self-documenting with actual text
- **2026-01-17 FL iter5**: [resolved] 引継ぎ先指定 F524 created (TBD Prohibition compliance)
- **2026-01-17 FL iter5**: [resolved] Maintainability: Added feature-builder.md removal to 引継ぎ先指定 and F524 scope
- **2026-01-17 FL iter6**: [note] Historical feature files (F450-F521) contain references to feature-builder as documentation of past decisions. These are not updated per historical record preservation principle.
- **2026-01-17 FL iter6**: [resolved] AC#1 pattern simplified from regex to literal string
- **2026-01-17 FL iter6**: [resolved] Task#1 AC mapping corrected: AC#5 moved from Task#3 to Task#1 (file structure verification belongs with edit task)
- **2026-01-17 FL iter6**: [resolved] Maintainability: F524 AC#5 pattern made specific to avoid duplication with AC#1
- **2026-01-17 FL iter6**: [resolved] AC Validation: AC#3 Expected simplified from escaped to literal "Skill(feature-creator)"
- **2026-01-17 FL iter7**: [resolved] AC#1 pattern changed from literal pipe to regex "research.*Skill(feature-creator)"
- **2026-01-17 FL iter7**: [resolved] F524 Problem section updated to include implementer.md line 74

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 06:47 | START | implementer | Task 1 | - |
| 2026-01-18 06:47 | END | implementer | Task 1 | SUCCESS |
| 2026-01-18 06:47 | START | implementer | Task 2 | - |
| 2026-01-18 06:47 | END | implementer | Task 2 | SUCCESS |
| 2026-01-18 06:48 | START | reference-checker | Link validation | - |
| 2026-01-18 06:48 | DEVIATION | reference-checker | F524 missing from Links | NEEDS_REVISION |
| 2026-01-18 06:48 | END | opus | Add F524 to Links | SUCCESS |
| 2026-01-18 06:48 | START | reference-checker | Re-validation | - |
| 2026-01-18 06:48 | END | reference-checker | All links valid | SUCCESS |
| 2026-01-18 06:48 | START | audit | Doc consistency | - |
| 2026-01-18 06:49 | END | audit | Task 3 complete | SUCCESS |
