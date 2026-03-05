# Feature 832: ac-static-verifier Numeric Expected Parsing Fix

## Status: [DRAFT]

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

### Problem (Current Issue)
ac-static-verifier.py fails when processing ACs with `gte` matcher and bare numeric Expected values (e.g., `12`, `1`, `4`). The error message is: "Expected value must be in '`pattern` = N' or 'Pattern (N)' format for content-type gte matcher". Bug confirmed to reproduce (2026-03-06) during F829 execution. The Format C code at lines 880-890 of ac-static-verifier.py exists but does not handle all cases correctly when the Grep pattern is specified in the Method column.

### Goal (What to Achieve)
Fix the numeric Expected parsing in ac-static-verifier.py so that bare numeric Expected values (e.g., `12`) work correctly with count-type matchers (gte, gt, lte, lt, count_equals) when the pattern is specified in the Method column via Grep(path, pattern="...") format. Include regression tests.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F829 | [WIP] | Phase 22 Deferred Obligations Consolidation; routing origin |

## Links
- [Related: F829](feature-829.md) - Phase 22 Deferred Obligations Consolidation (routing origin)
