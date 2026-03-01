# Feature 770: /fc Section Ordering Alignment with Feature Template

## Status: [CANCELLED]
<!-- Cancelled: 2026-02-10. fc.md and feature-template.md Section Ownership tables already match. No ordering mismatch exists. -->

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

## Review Context (FL POST-LOOP Step 6.3)

### Origin

| Field | Value |
|-------|-------|
| Parent Feature | F766 (Paren-Stripping Guard Refinement) |
| Discovery Point | FL Phase 2 Review, iter 3 |
| Timestamp | F766 FL review |

### Identified Gap

The `/fc` command generates sections in phase order (Phase 1 → Background, Phase 2 → Root Cause/Related Features/AC Design Constraints, Phase 3 → Acceptance Criteria, etc.), but this ordering does not match the feature-template.md SSOT which places AC Design Constraints after Baseline Measurement. Moving sections across `<!-- fc-phase-X-completed -->` markers would break phase semantics. The root cause is the `/fc` command's section generation order, not individual features.

### Review Evidence

| Field | Value |
|-------|-------|
| Gap Source | F766 FL [FMT-002] AC Design Constraints section ordering |
| Derived Task | Align /fc section generation order with feature-template.md |
| Comparison Result | All recent features (F760-F769) have the same ordering mismatch |
| DEFER Reason | Systemic /fc issue, not fixable per-feature |

### Parent Review Observations

F766 FL review identified that AC Design Constraints appears before Acceptance Criteria in all /fc-generated features, contrary to the template SSOT ordering.

## Background

### Philosophy (Mid-term Vision)
Ensure `/fc` command generates feature files with section ordering that matches the feature-template.md SSOT, eliminating recurring FL [FMT-002] findings and reducing FL iteration count.

### Problem (Current Issue)


### Goal (What to Achieve)


---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| (none identified) | - | - | - | - |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| | | | | |

---

## Links
- [feature-766.md](feature-766.md) - Parent feature (discovery source)
