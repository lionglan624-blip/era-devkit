# Feature 585: FL Workflow pending Status Auto-Update Rules

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
FL workflow automation should provide Phase-internal resolution tracking of pending issues through systematic status management during iterative fix-apply cycles. POST-LOOP user confirmation handled separately in F586.

### Problem (Current Issue)
FL workflow lacks clear rules for updating [pending] status during loop execution. Currently POST-LOOP only defines user confirmation updates, but Phase-internal fix applications and validation invalidations should also trigger status updates. The persist_pending() function writes [pending] tags to Review Notes, but there's no corresponding resolve_pending() function to update these tags when issues are resolved within the loop.

### Goal (What to Achieve)
Add resolve_pending() function and auto-update rules to PHASE-1.md Step C that automatically update [pending] status tags in Review Notes when fixes are applied or validation results change, providing complete pending issue lifecycle management.

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | resolve_pending function defined | file | Grep(.claude/skills/fl-workflow/SKILL.md) | contains | "resolve_pending Definition" | [x] |
| 2 | Function updates Review Notes | file | Grep(.claude/skills/fl-workflow/SKILL.md) | matches | "resolve_pending.*(?:resolved-applied|resolved-invalid)" | [x] |
| 3 | Auto-update in Phase 1 apply_fix | file | Grep(.claude/skills/fl-workflow/PHASE-1.md) | contains | "resolve_pending" | [x] |
| 4 | Auto-update in validation invalidation | file | Grep(.claude/skills/fl-workflow/PHASE-1.md) | contains | "resolve_pending" | [x] |
| 5 | resolved-applied transition documented | file | Grep(.claude/skills/fl-workflow/SKILL.md) | matches | "resolve_pending.*resolved-applied" | [x] |
| 6 | resolved-invalid transition documented | file | Grep(.claude/skills/fl-workflow/SKILL.md) | matches | "resolve_pending.*resolved-invalid" | [x] |
| 7 | POST-LOOP boundary preserved | file | Grep(.claude/skills/fl-workflow/POST-LOOP.md) | not_contains | "resolve_pending" | [x] |

## AC Details

**AC#1**: resolve_pending function definition added to SKILL.md
- Test: Grep "resolve_pending Definition" in .claude/skills/fl-workflow/SKILL.md
- Expected: Function definition section similar to persist_pending Definition

**AC#2**: Function updates Review Notes by changing pending tags
- Test: Grep pattern for resolve_pending function with resolved status codes
- Expected: Documentation shows resolve_pending function with resolved-applied or resolved-invalid status codes

**AC#3**: Phase 1 Step C calls resolve_pending when applying fixes
- Test: Grep for resolve_pending call in PHASE-1.md
- Expected: resolve_pending(issue, iteration, "resolved-applied") call added to Step C.1 after fix application logic

**AC#4**: Phase 1 Step C calls resolve_pending for validation invalidations
- Test: Grep for resolve_pending call in PHASE-1.md
- Expected: resolve_pending(issue, iteration, "resolved-invalid") call added to Step C.1 when validation marks issues as invalid

**AC#5**: resolved-applied transition documented
- Test: Grep for resolve_pending function with resolved-applied status codes in SKILL.md
- Expected: Documentation includes "resolved-applied" status for successful fix applications

**AC#6**: resolved-invalid transition documented
- Test: Grep for resolve_pending function with resolved-invalid status codes in SKILL.md
- Expected: Documentation includes "resolved-invalid" status for validation failures

**AC#7**: POST-LOOP boundary preserved
- Test: Grep for resolve_pending in POST-LOOP.md (should not contain)
- Expected: resolve_pending function remains within in-loop phases only

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add resolve_pending function definition to SKILL.md | [x] |
| 2 | 2 | Document function updates Review Notes behavior | [x] |
| 3 | 3 | Add resolve_pending calls to PHASE-1.md Step C apply_fix branches | [x] |
| 4 | 4 | Add resolve_pending calls to PHASE-1.md Step C validation invalidation | [x] |
| 5 | 5 | Verify resolved-applied transition documented (AC#5) | [x] |
| 6 | 6 | Verify resolved-invalid transition documented (AC#6) | [x] |
| 7 | 7 | Verify POST-LOOP boundary preserved (AC#7) | [x] |

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Component | Change | Details |
|-------|-----------|--------|---------|
| 1 | SKILL.md | Add resolve_pending function | After ### persist_pending Definition header, matching persist_pending parameter signature and Review Notes format pattern |
| 2 | PHASE-1.md Step C.1 | Insert resolve_pending call after fix completion | Add resolve_pending(issue, iteration, "resolved-applied") call immediately AFTER fix_log.append({...issue, iteration, phase: "review"}) line in Step C.1 ELSE branch |
| 3 | PHASE-1.md Step C.1 | Add resolve_pending for validation.invalid | Add resolve_pending(issue, iteration, "resolved-invalid") call in validation.invalid branch before the CONTINUE statement |
| 4 | Verification | Run audit and reference-checker | Ensure documentation consistency |

### Rollback Plan

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| .claude/skills/fl-workflow/SKILL.md | Add resolve_pending Definition | All FL workflow phases can reference function |
| .claude/skills/fl-workflow/PHASE-1.md | Add resolve_pending calls in Step C | Automatic pending status updates during fix application |
| Review Notes in feature files | (behavioral change) | [pending] tags automatically updated to [resolved] |

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Successor | F586 | [PROPOSED] | F586 depends on F585 resolve_pending function for POST-LOOP integration |

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->

### Resolved in FL iter10
- [resolved] Implementation Contract Phase 3 CONTINUE placement - Changed to "before CONTINUE" per user decision
- [resolved] AC#5/6 AC Details vs AC table mismatch - AC Details updated to match Grep verification
- [resolved] Philosophy AC Coverage gap (resolved-invalid) - AC#10 added for resolved-invalid pattern
- [resolved] Implementation Contract Phase 2a/2b clarity - Consolidated into single Phase 2 with (a)(b) locations
- [resolved] Phase0-RefCheck iter1: Non-existent Handoff Destination - F586 created

### Design Decisions (not SSOT violations)
- [resolved-design] TDD circular logic - AC tests expected outcome after implementation (standard TDD RED→GREEN)
- [resolved-design] Review Notes [pending] vs Mandatory Handoffs - Different purposes (FL tracking vs deferred tasks)
- [resolved-design] AC#5/6 Type validity - file|Grep|contains is SSOT-compliant
- [resolved-design] AC#9 pattern specificity - Pattern reasonable for TDD testing

### Resolved in FL iter2
- [resolved] AC#5/6 replaced with resolve_pending behavior verification instead of unrelated file content checks
- [resolved] AC#7/8 removed as pre-condition checks not testing F585 functionality
- [resolved] AC renumbering: AC#9→7, AC#10→8, AC#11→9 after AC#7/8 deletion
- [resolved] Tasks table updated to match new AC numbers and descriptions

### Resolved Design Decisions
- [resolved-design] Phase1-Uncertain iter1: AC#2 regex pattern validity - Depends on implementation documentation format (acceptable TDD approach)
- [resolved-design] Phase1-Uncertain iter3: Lifecycle states documentation - AC#5/6 provide sufficient coverage without additional AC#10
- [resolved-design] Phase1-Uncertain iter6: AC#5/6 regex documentation style - Current AC Details sufficient per SSOT
- [resolved-design] Phase1-Uncertain iter6: Review Notes pending entries - Keep for FL tracking history
- [resolved-design] Phase1-Uncertain iter7: Task#5/6 specificity - AC:Task 1:1 mapping provides sufficient traceability

### Remaining Issues (resolved by user decision)
- [resolved-user] Task description style: Keep 'Verify X' for infra features (検証が目的)
- [resolved-user] AC#2 Expected format: Changed to regex pattern for composite status support
- [pending] Phase1-Uncertain iter9: Implementation Contract Phase 2 specificity - Whether to add explicit line reference clarification
- [resolved-user] AC#9/10 pattern specificity: Current pattern sufficient for TDD testing
- [resolved-user] Goal vs Philosophy scope: resolve_pending implements Philosophy, F586 handles POST-LOOP
- [resolved-user] Leak Prevention distinction: Review Notes [pending] = FL tracking, Mandatory Handoffs = Deferred Task Protocol (if feature creation needed from pending, tracking destination specified at that time)

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| POST-LOOP pending handling | Different lifecycle pattern | Feature | F586 [DONE] |
| ac-static-verifier matches support | Tool lacks `matches` matcher | Feature | F593 |

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-21 21:03 | START | implementer | Task 1-4 | - |
| 2026-01-21 21:03 | END | implementer | Task 1-4 | SUCCESS |
| 2026-01-21 21:06 | DEVIATION | Bash | ac-static-verifier | exit code 1: AC#2,5,6 FAIL (Unknown matcher: matches) |
| 2026-01-21 21:09 | DEBUG | debugger | Fix AC#2,5,6 | Added usage examples to SKILL.md for regex pattern match |
| 2026-01-21 21:10 | VERIFY | Grep | AC#1-7 | All PASS |

## Links
- [index-features.md](index-features.md)
- [fl-workflow SKILL.md](../../.claude/skills/fl-workflow/SKILL.md)
- [fl-workflow PHASE-1.md](../../.claude/skills/fl-workflow/PHASE-1.md)
- [feature-586.md](feature-586.md)