# Feature 508: Remove '負債の意図的受け入れ' from Remaining Design Docs

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

**"Write and forget" must be impossible.** Every deferred task must have a concrete tracking destination before a Feature can be completed. The workflow itself must enforce this - not just documentation guidelines.

This Feature is a follow-up to F507, extending the abolition of '負債の意図的受け入れ' sections to all design documents.

### Problem (Current Issue)

F507 targets only architecture-review-15.md, but '負債の意図的受け入れ' sections exist in 5 additional design documents:

| File | Line | Type |
|------|:----:|------|
| testability-assessment-15.md | 173 | Section header |
| folder-structure-15.md | 262 | Section header |
| naming-conventions-15.md | 358 | Section header |
| test-strategy.md | 279 | Section header |
| test-strategy.md | 380 | Section header |

These sections create alternative tracking mechanisms outside CLAUDE.md Deferred Task Protocol.

### Goal (What to Achieve)

Remove all '負債の意図的受け入れ' sections from the 5 remaining design documents, ensuring consistent enforcement of Deferred Task Protocol across the codebase.

---

## Impact Analysis

### Affected Documents
| File | Change |
|------|--------|
| testability-assessment-15.md | Remove '負債の意図的受け入れ' section |
| folder-structure-15.md | Remove '負債の意図的受け入れ' section |
| naming-conventions-15.md | Remove '負債の意図的受け入れ' section |
| test-strategy.md | Remove 2 '負債の意図的受け入れ' sections |

### Breaking Changes
- Agents can no longer use '負債の意図的受け入れ' as a tracking mechanism in these files (following F507's pattern for architecture-review-15.md)

### Migration Path
- No migration needed - sections are being removed, not replaced

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | testability-assessment section removed | file | Grep(Game/agents/designs/testability-assessment-15.md) | not_contains | "負債の意図的受け入れ" | [x] |
| 2 | folder-structure section removed | file | Grep(Game/agents/designs/folder-structure-15.md) | not_contains | "負債の意図的受け入れ" | [x] |
| 3 | naming-conventions section removed | file | Grep(Game/agents/designs/naming-conventions-15.md) | not_contains | "負債の意図的受け入れ" | [x] |
| 4 | test-strategy sections removed | file | Grep(Game/agents/designs/test-strategy.md) | not_contains | "負債の意図的受け入れ" | [x] |

### AC Details

**AC#1-4**: Remove all '負債の意図的受け入れ' section occurrences from each design document.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Remove '負債の意図的受け入れ' section from testability-assessment-15.md | [x] |
| 2 | 2 | Remove '負債の意図的受け入れ' section from folder-structure-15.md | [x] |
| 3 | 3 | Remove '負債の意図的受け入れ' section from naming-conventions-15.md | [x] |
| 4 | 4 | Remove both '負債の意図的受け入れ' sections from test-strategy.md | [x] |

<!-- AC#1-4 are verified via Grep not_contains checks -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

**Note**: Line numbers are approximate. Use Grep to locate actual sections.

### Part 1: testability-assessment-15.md (line ~173)
Remove entire '負債の意図的受け入れ' section including header and content.

### Part 2: folder-structure-15.md (line ~262)
Remove entire '負債の意図的受け入れ' section including header and content.

### Part 3: naming-conventions-15.md (line ~358)
Remove entire '負債の意図的受け入れ' section including header and content.

### Part 4: test-strategy.md (lines ~279, ~380)
Remove both '負債の意図的受け入れ' sections including headers and content.

### Rollback Plan

If issues arise:
1. `git revert` the commit
2. Report to user
3. Create follow-up feature for fix

---

## Review Notes
<!-- Optional: Add review feedback here. -->

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-15 18:56 | START | implementer | Task 1-4 | - |
| 2026-01-15 18:56 | END | implementer | Task 1-4 | SUCCESS |
| 2026-01-15 | DEVIATION | initializer | Status update | Status not changed to [WIP] - manually fixed |

## Links

**Context**:
- [F507](feature-507.md) - Parent feature (architecture-review-15.md scope)
- [CLAUDE.md](../../CLAUDE.md) - Deferred Task Protocol (SSOT)

**Target Files**:
- [testability-assessment-15.md](designs/testability-assessment-15.md)
- [folder-structure-15.md](designs/folder-structure-15.md)
- [naming-conventions-15.md](designs/naming-conventions-15.md)
- [test-strategy.md](designs/test-strategy.md)
