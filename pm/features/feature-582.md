# Feature 582: FL Workflow persist_pending Definition Guidance

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

Progressive Disclosure principle requires each phase to load only the information needed at that moment. However, function definitions used across phases (like `persist_pending`) must have clear guidance to their definitions to prevent incorrect usage.

### Problem (Current Issue)

FL workflow Phase files (PHASE-1.md, PHASE-2.md, etc.) use `persist_pending()` function without explicit Read instruction to SKILL.md where the function is defined. This led to incorrect implementation (creating `pending_user.txt` file instead of writing to feature.md Review Notes section).

Root cause: SKILL.md lines 196-205 defines `persist_pending` but Phase files don't instruct to read SKILL.md before first use.

### Goal (What to Achieve)

1. Add persist_pending definition guidance in SKILL.md Global Rules section, ensuring all phases have access before first use
2. Ensure orchestrator understands `persist_pending` writes to feature.md, not separate files

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| `.claude/skills/fl-workflow/SKILL.md` | Add persist_pending cross-reference in Global Rules | Accessible to all phases before first use |
| `.claude/skills/fl-workflow/PHASE-0.md` | No change | Still uses persist_pending |

### Rollback Plan

If issues arise:
1. Revert commit via `git revert`
2. Original Phase files restored

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SKILL.md contains persist_pending guidance | code | Grep(.claude/skills/fl-workflow/SKILL.md) | contains | "persist_pending Usage Guidance" | [x] |
| 2 | Guidance references correct section | code | Grep(.claude/skills/fl-workflow/SKILL.md) | contains | "Review Notes" | [x] |
| 3 | SKILL.md persist_pending definition exists | code | Grep(.claude/skills/fl-workflow/SKILL.md) | contains | "persist_pending Definition" | [x] |
| 4 | All links valid | tool | reference-checker | succeeds | - | [x] |
| 5 | Phase loading pattern verified in SKILL.md | code | Grep(.claude/skills/fl-workflow/SKILL.md) | contains | "Read(.claude/skills/fl-workflow/PHASE" | [x] |

### AC Details

**AC#1**: SKILL.md must contain persist_pending guidance subsection
- Test: `Grep "persist_pending Usage Guidance" .claude/skills/fl-workflow/SKILL.md`
- Expected: Contains guidance subsection header

**AC#2**: Guidance must clarify that persist_pending writes to Review Notes section
- Test: `Grep "Review Notes" .claude/skills/fl-workflow/SKILL.md`
- Expected: Contains reference to Review Notes

**AC#3**: Verify SKILL.md already has persist_pending definition (prerequisite)
- Test: `Grep "persist_pending Definition" .claude/skills/fl-workflow/SKILL.md`
- Expected: Definition section exists

**AC#4**: All references valid
- Test: Run reference-checker
- Expected: No broken links

**AC#5**: Verify Progressive Disclosure pattern ensures SKILL.md loads before Phase files
- Test: `Grep "Read(.claude/skills/fl-workflow/PHASE" .claude/skills/fl-workflow/SKILL.md`
- Expected: SKILL.md shows Phase file loading pattern

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add persist_pending guidance to SKILL.md Global Rules section | [x] |
| 2 | 2 | Ensure guidance mentions Review Notes | [x] |
| 3 | 3 | Verify SKILL.md definition exists | [x] |
| 4 | 4 | Run reference-checker | [x] |
| 5 | 5 | Verify Progressive Disclosure pattern | [x] |

---

## Implementation Contract

### Change Details

Add the following guidance to SKILL.md Global Rules section (near line 190, before persist_pending Definition):

```markdown
### persist_pending Usage Guidance

**persist_pending Definition**: See section "persist_pending Definition" below. Writes to **feature.md Review Notes section** with `[pending]` tag. Do NOT create separate files like `pending_user.txt`.

This function is used across multiple Phase files. Always read this SKILL.md before proceeding to Phase files.
```

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F568 | [DONE] | Origin of this handoff issue |
| Related | F574 | [DONE] | FL Progressive Disclosure fix |

---

## Links

- [index-features.md](index-features.md)
- [feature-568.md](feature-568.md) - Origin: Handoff from F568
- [feature-574.md](feature-574.md) - Related: FL Progressive Disclosure
- [feature-584.md](feature-584.md) - Deferred: SSOT Method column format inconsistency
- [feature-587.md](feature-587.md) - Handoff: ac-static-verifier quote stripping bug

---

## Review Notes
- [applied] Phase1-Uncertain iter3: AC#3 is a prerequisite check (verifying existing state) not implementation work. However, AC:Task 1:1 rule in feature-template.md requires each AC to have corresponding Task. The AC itself may be questionable, but Task mapping follows template rules. → User decision: Keep AC#3 as prerequisite verification.
- [applied] Phase1-Uncertain iter3: Current Goal wording 'Add cross-reference in PHASE-0.md to read SKILL.md definition before first persist_pending() use' is consistent with Implementation Contract 'before Step 0.2'. Additional specificity may help but current wording is not incorrect. → Resolved: Goal changed to SKILL.md target.
- [deferred:F584] Phase1-Uncertain iter3: Testing SKILL.md AC Type Requirements table (line 352) shows 'Grep(path)' format for Method column. Feature-582 uses 'Grep(.claude/skills/fl-workflow/SKILL.md)' which matches this pattern. However, Method Column Usage examples (lines 56-61) show different format. SSOT has inconsistency. → User decision: Fix in separate Feature F584.
- [applied] Phase2-Maintainability iter4: Goal specifies 'Add cross-reference in PHASE-0.md' but persist_pending is first used in PHASE-0.md Step 0.2 line 40 AND PHASE-1.md line 182/185. PHASE-0 guidance alone does not cover PHASE-1 first use. Philosophy says 'function definitions used across phases must have clear guidance' but Goal only addresses PHASE-0. → Resolved: Goal changed to SKILL.md (covers all phases).
- [applied] Phase2-Maintainability iter4: Implementation Contract adds guidance 'before Step 0.2' in PHASE-0.md but this does NOT solve the root cause. When orchestrator enters Phase 1 directly (e.g., after Phase 0 completes), they read PHASE-1.md which has persist_pending at line 182. The guidance in PHASE-0.md is not visible. Progressive Disclosure principle means SKILL.md is the only common entry point loaded before all phases. → Resolved: Implementation Contract changed to SKILL.md.
- [applied] Phase2-Maintainability iter4: Philosophy states 'function definitions used across phases must have clear guidance to their definitions'. AC#1 only checks PHASE-0.md contains guidance, but does not verify that 'all phases' can access the guidance. The AC does not validate the philosophy's 'across phases' requirement. → Resolved: AC#1 now checks SKILL.md.
- [applied] Phase1-Uncertain iter5: AC#3 does verify existing state as a prerequisite. However, Review Notes line 133 already acknowledges this as [pending] and justifies it via AC:Task 1:1 rule from feature-template.md. The issue was already identified and tracked. Whether to remove it or keep it is a design decision. → User decision: Keep AC#3.
- [skipped] Phase1-Invalid iter6: Goal line 33 states 'Add persist_pending definition guidance in SKILL.md Global Rules section' and Implementation Contract lines 103-111 adds exactly that to SKILL.md. These are consistent, not inconsistent. The reviewer misread - there is no inconsistency between Goal and Implementation Contract.
- [skipped] Phase1-Invalid iter6: AC#1 Expected 'persist_pending Usage Guidance' will correctly match Implementation Contract header '### persist_pending Usage Guidance' via Grep contains matcher. The Grep pattern does not require quotes around multi-word strings. This is not a real issue.
- [skipped] Phase1-Invalid iter6: AC:Task 1:1 rule from feature-template.md line 121 means '1 AC = 1 Test = 1 Task' but does NOT prohibit prerequisite verification ACs. Feature-582 Review Notes line 133 already justifies AC#3 via AC:Task 1:1 rule. The AC verifies a prerequisite exists, which is valid for implementation confidence.
- [skipped] Phase1-Invalid iter6: AC#5 verifies that Progressive Disclosure pattern exists in SKILL.md (Phase loading via Read instructions). This is a prerequisite verification ensuring the architecture supports the solution. While not 'new implementation', it validates the assumption that SKILL.md loads before Phase files. Removing it would weaken verification.
