# Feature 834: ac-static-verifier Format C Guard DRY Consolidation and unescape() Investigation

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

### Philosophy (Mid-term Vision)
ac-static-verifier.py should have a single implementation of the Format C guard logic (Grep-count-matcher pattern-to-expected resolution) shared across both `verify_code_ac` and `verify_file_ac`. Additionally, `unescape()` behavior on regex metacharacters from Method column patterns should be verified and corrected if necessary.

### Problem (Current Issue)
1. Duplicated Format C guard logic exists at two sites: `verify_code_ac` (lines ~880-890) and `verify_file_ac` (lines ~1032-1040). Both sites contain identical pattern-to-expected resolution logic that could diverge during future modifications.
2. `unescape()` (line ~464) may corrupt regex metacharacters like `\|`, `\[` when applied to patterns extracted from the Method column. This risk was identified during F832 (likelihood MEDIUM) but not investigated.

### Goal (What to Achieve)
Extract shared Format C guard logic into a helper method (e.g., `_resolve_count_expected(ac, pattern)`) called from both verification entry points. Investigate and fix `unescape()` behavior on regex patterns from Method column.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F832 | [DONE] | Parser fix that makes positional Grep format work; this feature consolidates the guard logic |

---

## Links
- [Predecessor: F832](feature-832.md) - ac-static-verifier Numeric Expected Parsing Fix
