# Feature 579: Remove IMPLE_FEATURE_ID Reference Cleanup

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
Documentation consistency (SSOT) - Reference documentation should not contain obsolete implementation details. Dead code references in reference docs confuse future maintainers and violate the principle that documentation should reflect current reality. This applies specifically to Game/agents/reference/ directory which contains active reference documentation.

### Problem (Current Issue)
Line 120 of `io-format-guide.md` contains a reference to `IMPLE_FEATURE_ID` which was identified as dead code. F577 (Workflow Dead Code Cleanup) was scoped to workflow documentation only, leaving reference documentation untouched. This creates inconsistency where dead code has been cleaned up from workflow files but remains in reference documentation.

### Goal (What to Achieve)
Remove all dead code references `IMPLE_FEATURE_ID` from reference documentation in `Game/agents/reference/` directory, completing the cleanup initiated by F577 for reference documentation.

**Out of scope**: Historical feature files (feature-*.md) in Game/agents/ and Game/agents/features/ which document past implementation details as historical records, not active reference documentation.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IMPLE_FEATURE_ID removed from line 120 | file | Grep(Game/agents/reference/io-format-guide.md) | not_contains | "IMPLE_FEATURE_ID" | [x] |
| 2 | No remaining IMPLE_FEATURE_ID references in reference docs | file | Grep(Game/agents/reference/) | not_contains | "IMPLE_FEATURE_ID" | [x] |

### AC Details

**AC#1**: Dead code reference removed from io-format-guide.md
- Test: `Grep("IMPLE_FEATURE_ID", "Game/agents/reference/io-format-guide.md")`
- Expected: No matches (reference completely removed)

**AC#2**: No remaining IMPLE_FEATURE_ID references in reference documentation
- Test: `Grep("IMPLE_FEATURE_ID", "Game/agents/reference/")`
- Expected: No matches (all dead references removed from reference docs)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Change line 120 from 'Scope (full or feature-scoped via `.git/IMPLE_FEATURE_ID`)' to 'Scope (full or feature-scoped)' | [x] |
| 2 | 2 | Remove any additional IMPLE_FEATURE_ID references found in reference docs | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Step | Action | Method |
|------|--------|--------|
| 1 | Read current line 120 context | Read(io-format-guide.md, offset=115, limit=10) |
| 2 | Edit line to remove dead reference | Edit(remove IMPLE_FEATURE_ID reference) |
| 3 | Verify edit preserves meaning | Grep verification of context |
| 4 | Run documentation audit | /audit command |

### Rollback Plan

If issues arise after implementation:
1. Revert commit with `git revert`
2. Restore original line 120 content
3. Create follow-up feature for alternative approach

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F577 | [DONE] | Workflow Dead Code Cleanup (established dead code identification) |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- [resolved] Previous AC validation issues resolved through AC restructuring: Removed redundant smoke test ACs (AC#2,AC#3), fixed AC#2 Type/Method mismatch by changing to file/Grep verification approach.

---

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| (none currently) | | | |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-21 | DEVIATION | feature-reviewer | post review | NEEDS_REVISION: Task status not updated |

## Links
- [index-features.md](index-features.md)
- [feature-577.md](feature-577.md) - Predecessor Workflow Dead Code Cleanup