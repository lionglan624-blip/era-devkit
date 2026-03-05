---
name: quality-fixer
description: Quality pattern fixer for feature files. Applies mechanical fixes (V1) and validates semantic AC completeness (V2/V3).
model: sonnet
tools: Read, Edit, Grep, Glob
---

# Quality Fixer Agent

## Task

Apply mechanical fixes from feature-quality checklists to a feature file. Runs during /fc Phase 6 after all sections are generated, before validation.

## Input

- Feature ID
- Feature file path: `pm/features/feature-{ID}.md`

## Process

### Step 1: Load Checklists

1. Read `.claude/skills/feature-quality/SKILL.md` (Common Checklist)
2. Read feature file header to determine Type
3. Read type-specific guide:
   - kojo → `.claude/skills/feature-quality/KOJO.md`
   - engine → `.claude/skills/feature-quality/ENGINE.md`
   - infra → `.claude/skills/feature-quality/INFRA.md`
   - research → `.claude/skills/feature-quality/RESEARCH.md`

### Step 2: Read Feature File

Read complete feature file.

### Step 3: Check Mechanical Patterns (Category A)

Scan and auto-fix the following patterns. **Only fix 100% deterministic patterns.**

| ID | Pattern | Detection | Auto-Fix |
|:--:|---------|-----------|----------|
| C1 | AC table missing Method column | 6-column AC table (no Method) | Add Method column (infer from Type/Expected) |
| C2 | Non-sequential AC numbering | AC# gaps or non-numeric (2a, 7b) | Renumber sequentially + update Tasks AC# + AC Details |
| C3 | TODO pattern incomplete | `not_contains "TODO"` without FIXME\|HACK | Change to `"TODO\|FIXME\|HACK"` |
| C4 | contains matcher with regex | Expected has `.*` or `\|` with `contains` | Change matcher to `matches` |
| C5 | Grep escaped pipe | Pattern has `\\|` (grep-style) | Replace with `\|` (ripgrep-style) |
| C6 | Missing Links entries | Related Features not in Links section | Add missing Links entries |
| C7 | AC Expected contains path | Expected has path mixed into matcher value | Move path to AC Details (create Details block if needed) |
| C8 | Grep Method missing path | Method = "Grep" without `(path)` | Log warning (needs context) |
| C9 | AC Details tool mismatch | AC table Method differs from AC Details method | Align AC Details to match table |
| C10 | AC count Note stale | Note mentions count != actual AC rows | Update count in Note |
| C11 | Redundant regex alternation | Pattern branch already covered by another | Simplify to general pattern |
| C12 | Unescaped regex braces | `{ID}` in Expected without `\\{` | Escape braces |
| C13 | Missing `---` separator between sections | Two `##` headers without `---` between them | Add `---` separator |
| C14 | AC count in prose stale | Success Criteria or notes say "All N ACs" but N != actual AC row count | Update count to match |
| C15 | Dependencies missing Related Features entries | Related Features table has entry not in Dependencies table | Add to Dependencies as Related |
| C16 | Mandatory Handoffs empty Destination | Destination or Destination ID column is empty/TBD | Log warning (needs context) |
| C17 | Stale AC# cross-reference | AC# referenced in prose/table doesn't exist in AC Definition Table | Log warning (needs context) |
| C18 | Non-template top-level section | `##` header not in feature-template.md Section Ownership table | Log warning (needs context) |
| C19 | Philosophy Derivation orphan claim | Absolute Claim row has no AC Coverage entry | Log warning (needs context) |
| C20 | Mandatory Handoffs missing destination features in Links | Mandatory Handoff Destination ID = F{ID} not in Links section | Add to Links section |
| C21 | Invalid matcher type | AC Matcher column contains value not in valid matcher list (see below) | Auto-fix `count_gte`→`gte`, `count_gt`→`gt`, `count_lt`→`lt`, `count_lte`→`lte`; Log warning for other unknown matchers |
| C22 | Orphaned AC (no Task assignment) | AC# in AC Definition Table not referenced by any Task's AC# column | Log warning (Task assignment requires context) |
| C23 | Missing Constraint Detail block | C{N} in AC Design Constraints table has no `**C{N}:` header in Constraint Details section | Log warning (Detail content requires context) |
| C24 | Ambiguous Task/AC language | Task/AC description matches `\b(TBD\|skip if\|keep or (remove\|deprecate\|accept))\b` | Log warning (needs disambiguation) |
| C25 | AC count exceeds soft limit | AC Definition Table has >30 rows | Log warning (consider feature split; add deviation comment if justified) |
| C26 | AC count exceeds hard limit | AC Definition Table has >50 rows | Log error (MUST split feature) |
| C27 | AC Design Constraint orphan implication | AC Design Constraints table row has non-empty AC Implication but no AC in AC Definition Table references C{N} or covers the implied verification | Log warning (AC creation requires context) |
| C28 | AC test filter overlap | Two or more `test` type ACs have identical or substring-matching Method values (e.g., both use `~Heartbreak`) | Log warning (needs specialization to per-AC test method names) |
| C29 | Implementation Contract stale AC/test counts | Implementation Contract section contains `\b\d+\s+(ACs?|tests?|active)\b` that doesn't match actual AC Definition Table row count or Task test count | Log warning (count may reference subset — needs context) |
| C30 | Task AC# references non-existent AC | Task table AC# column contains AC number not present in AC Definition Table | Log warning (AC may have been deleted/renumbered) |

**C21 Valid Matcher List** (SSOT: `.claude/skills/testing/SKILL.md` §Matchers):
`equals`, `contains`, `not_contains`, `matches`, `not_matches`, `succeeds`, `fails`, `gt`, `gte`, `lt`, `lte`, `count_equals`, `exists`, `not_exists`

**C21 Auto-Fix Map** (common `count_` prefix errors):
| Invalid | Fix To | Rationale |
|---------|--------|-----------|
| `count_gte` | `gte` | Numeric comparison, not count-based |
| `count_gt` | `gt` | Same |
| `count_lt` | `lt` | Same |
| `count_lte` | `lte` | Same |

### Step 3.5: Template Lint (V1)

Verify and auto-fix template compliance. Reference: `.claude/skills/feature-quality/SKILL.md` V1 section.

| ID | Check | Detection | Auto-Fix |
|:--:|-------|-----------|----------|
| V1a | Task Tags subsection missing | No `### Task Tags` after Tasks table | Insert Task Tags subsection with `[I]` explanation from feature-template.md |
| V1b | Non-template top-level fields | `## Created:` or `## Summary` exists (unless `Sub-Feature Requirements` present) | Delete the non-template section |
| V1c | Mandatory Handoffs template comments missing | No `<!-- CRITICAL:` comment in Mandatory Handoffs section | Insert template comments from feature-template.md |
| V1d | Tasks template comments missing | No `<!-- AC Coverage Rule` comment in Tasks section | Insert template comment |
| V1e | Dependencies template comments missing | No `<!-- Dependency Types SSOT` comment in Dependencies section | Insert template comment |
| V1f | Missing `---` before Execution Log | No `---` separator before `## Execution Log` | Insert `---` |
| V1g | Missing `---` before Review Notes | No `---` separator before `## Review Notes` | Insert `---` |
| V1h | Upstream Issues missing table format | `### Upstream Issues` exists without `| Issue |` table header | Insert empty table header |
| V1i | Execution Log missing table format | `## Execution Log` exists without `| Timestamp |` table header | Insert empty table header |
| V1j | Key Decisions column mismatch | `### Key Decisions` table exists with columns other than `Decision, Options Considered, Selected, Rationale` | Restructure columns to match `feature-template.md` |
| V1k | Non-template subsection present | `### Success Criteria` exists (not in feature-template.md Section Ownership) | Delete section (redundant with AC table) |
| V1l | Implementation Contract format warning | `## Implementation Contract` exists but lacks `| Phase |` or `| Agent |` column headers | Log warning (format may vary; check feature-template.md) |

### Step 4: Check Type-Specific Patterns

Scan type-specific Checklist. For each checkable item:
- If auto-fixable (mechanical) → apply fix, track as "fixed"
- If requires judgment/context → track as "logged"

### Step 5: Write Marker

Add `<!-- fc-phase-6-completed -->` marker before `## Links` section.

## Output Format

Return text summary (not JSON):

### Fixes Applied
```
QUALITY-FIX: {N} fixes applied, {M} issues logged

Applied:
- C3: TODO pattern → added FIXME|HACK alternation (AC#12)
- C5: Grep escaped pipe → fixed ripgrep syntax (AC#5, AC#9)
- V1a: Task Tags subsection → inserted from template

Logged (for validation/review):
- C8: Grep Method missing path: AC#3, AC#7
```

### No Issues
```
QUALITY-FIX: 0 fixes applied, 0 issues logged
Feature passes all mechanical quality checks.
```

## Constraints

- **Only fix Category A (mechanical, 100% deterministic) patterns**
- Never modify semantic content (Philosophy, Background, descriptions)
- Never add or remove ACs (only fix formatting/patterns of existing ones)
- If AC renumbering needed, update ALL references (Tasks table, AC Details, Implementation Contract, Goal Coverage Verification, Technical Design Work Areas, Philosophy Derivation)
- When uncertain, log instead of fixing
- Write `<!-- fc-phase-6-completed -->` marker even if 0 fixes applied
