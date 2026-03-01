# Feature 231: User Approval UX

## Status: [DONE]

## Type: infra

## Keywords (検索用)

`/do`, `user approval`, `case-insensitive`, `UX`, `Phase 8`

---

## Summary

User approval prompt in `/do` Phase 8 should accept both uppercase and lowercase variants of "yes"/"y" to improve user experience.

---

## Background

### Philosophy (Mid-term Vision)

User-facing workflows should be robust against common input variations. Workflows should not fail due to trivial input format differences when user intent is clear.

### Problem (Current Issue)

**Issue W5 from F223 Issue Inventory**:

Phase 8 user approval may reject uppercase "Y" or "YES", accepting only lowercase variants. This creates unnecessary friction when users naturally type uppercase confirmation.

**Current behavior (from do.md "Phase 8: Report & Approval" section)**:
```markdown
Finalize と Commit? (y/n)

**CRITICAL: Wait for EXPLICIT user approval before proceeding.**
- User MUST respond with "y", "yes", "OK", or equivalent
```

The current documentation accepts "y", "yes", "OK", or equivalent, but does not explicitly specify case-insensitivity. This creates ambiguity about whether uppercase inputs like "Y" or "YES" are accepted.

### Goal (What to Achieve)

Accept both uppercase and lowercase variants of approval responses ("y", "Y", "yes", "YES", "ok", "OK") to reduce user friction and prevent accidental rejection of valid confirmations.

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Case-insensitive acceptance documented | code | Grep | contains | "case-insensitive" | [x] |

### AC Details

**Test 1**: Verify do.md Phase 8 approval section explicitly mentions case-insensitive acceptance.

```bash
Grep(pattern: "case-insensitive", path: ".claude/commands/do.md", output_mode: "content")
```

**Expected**: "case-insensitive" appears in user approval section of do.md.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Update do.md "Update Rules" section and "Phase 8: Report & Approval" section to specify case-insensitive matching for user approval | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | init | - | Feature created from F223 Task 7 (W5) | - |
| 2025-12-27 | initialize | initializer | Status [PROPOSED]→[WIP], ready for implementation | READY |
| 2025-12-27 | implement | implementer | Added case-insensitive to do.md (3 locations) | SUCCESS |
| 2025-12-27 | verify | AC1 | Grep "case-insensitive" found 3 matches | PASS |
| 2025-12-27 | post-review | feature-reviewer | Implementation achieves Philosophy goal | READY |
| 2025-12-27 | finalize | finalizer | Status [WIP]→[DONE], moved to Recently Completed | READY_TO_COMMIT |

---

## Dependencies

- F227 (/do Workflow Robustness) - **SATISFIED** [DONE]

---

## Links

- [F223](feature-223.md) - /do Workflow Comprehensive Audit (parent feature)
- [F227](feature-227.md) - /do Workflow Robustness (dependency)

---

## Notes

### Implementation Guidance

The approval logic should be modified to:

1. **Convert user input to lowercase** before comparison
2. **Accept these variants**: y, yes, ok (and uppercase/mixed case equivalents)
3. **Reject everything else**: including partial matches, silence, topic changes, questions

### Current Approval Gate Documentation (do.md "Update Rules" section)

```markdown
- **NEVER** mark approval gates as completed until user explicitly says "y" or "yes"
- **CRITICAL**: User approval requires EXPLICIT confirmation. Do NOT assume approval from:
  - Silence or lack of response
  - Moving on to discuss other topics
  - Asking questions about next steps
  - Any response that is not clearly "y", "yes", "OK", or equivalent
```

### Proposed Update (do.md "Update Rules" section)

**Before**:
```markdown
- **NEVER** mark approval gates as completed until user explicitly says "y" or "yes"
- **CRITICAL**: User approval requires EXPLICIT confirmation. Do NOT assume approval from:
  - Silence or lack of response
  - Moving on to discuss other topics
  - Asking questions about next steps
  - Any response that is not clearly "y", "yes", "OK", or equivalent
```

**After**:
```markdown
- **NEVER** mark approval gates as completed until user explicitly says "y" or "yes" (case-insensitive)
- **CRITICAL**: User approval requires EXPLICIT confirmation. Do NOT assume approval from:
  - Silence or lack of response
  - Moving on to discuss other topics
  - Asking questions about next steps
  - Any response that is not clearly "y", "yes", "OK", or equivalent (case-insensitive matching)
```

### Proposed Update (do.md "Phase 8: Report & Approval" section)

**Before**:
```markdown
Finalize と Commit? (y/n)

**CRITICAL: Wait for EXPLICIT user approval before proceeding.**
- User MUST respond with "y", "yes", "OK", or equivalent
```

**After**:
```markdown
Finalize と Commit? (y/n)

**CRITICAL: Wait for EXPLICIT user approval before proceeding.**
- User MUST respond with "y", "yes", "OK", or equivalent (case-insensitive matching)
- Rejected: Any response that is not clearly approval
```

### Test Case Examples

| Input | Expected Result | Reason |
|-------|----------------|--------|
| "y" | Approved | Standard lowercase |
| "Y" | Approved | Standard uppercase |
| "yes" | Approved | Standard lowercase |
| "YES" | Approved | Standard uppercase |
| "Yes" | Approved | Mixed case |
| "ok" | Approved | Alternative approval |
| "OK" | Approved | Alternative approval (uppercase) |
| "n" | Rejected | Explicit rejection |
| "no" | Rejected | Explicit rejection |
| "maybe" | Rejected | Ambiguous |
| "" (silence) | Rejected | No response |
| "What's next?" | Rejected | Question, not approval |

---

## Priority Justification

**P2 (Nice-to-have)** - This is a UX improvement, not a blocking issue. Current workflow functions correctly with lowercase input. However, it reduces friction and prevents confusion when users naturally type uppercase confirmations.

Dependency on F227 ensures this UX polish is applied to the final, stable workflow design rather than an interim version.
