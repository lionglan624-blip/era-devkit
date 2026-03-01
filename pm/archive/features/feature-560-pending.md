# Feature 560 Pending Issues

## Phase 0 - Reference Check (Iteration 0)

### Critical Issue: Phase0-RefCheck

**Type**: unverified_claim
**Location**: Background/Problem section
**Issue**: Feature 560 claims that requires: frontmatter causes bulk loading, but does not verify this claim against actual Skill loader behavior
**Fix Required**: Task 1 (Investigate Skill loader behavior) must be completed before removal. This is a research feature that should verify the assumption before acting.

**User Decision Required**: Should this feature proceed with Task 1 investigation first, or should the Background/Problem claim be revised based on existing evidence?

## Phase 1 - Review (Iteration 0)

### Critical Issue: Problem Statement Unverified

**Type**: unverified_assumption
**Location**: Background/Problem section (lines 37-47)
**Issue**: Problem statement is speculative, not verified. Task 1 says 'Investigate Skill loader behavior' but Philosophy and Goal sections assume the `requires:` directive causes bulk loading without verification. If `requires:` is actually ignored by the Skill loader, the entire Feature premise is invalid.
**Fix Required**: Move Task 1 (Skill loader behavior investigation) to a prerequisite phase. Either: (A) Verify the assumption BEFORE creating this Feature and document the evidence in Background, or (B) Change Feature type to 'research' if the investigation itself is the deliverable.

**User Decision Required**: Should this feature be converted to 'research' type, or should the investigation be completed first to verify the premise?

### Major Issue: Scope Inconsistency

**Type**: scope_incomplete
**Location**: Target scope vs. available scope
**Issue**: fl-workflow/SKILL.md has the SAME `requires:` anti-pattern (lines 6-16). Feature 560 only targets run-workflow but the same issue exists in fl-workflow which was just created in Feature 557. This creates inconsistency.
**Fix Options**: (A) Expand scope to include fl-workflow/SKILL.md (add Tasks and ACs for both workflows), or (B) Create a separate Feature for fl-workflow `requires:` removal and link it in Dependencies.

**User Decision Required**: Should Feature 560 expand scope to cover both workflows, or create separate Feature for fl-workflow?

## Phase 1 - Final Review (Iteration 0)

### Critical Issue: Invalid Handoff Reference

**Type**: invalid_reference
**Location**: Handoff Section
**Issue**: Handoff references F561 for 'fl-workflow requires: removal' but F561 is [DONE] and was about Parallel Execution Mode, not requires: removal. The handoff destination is incorrect - F561 does not address this issue.
**Fix Options**: (A) Create a new Feature F562 specifically for 'fl-workflow requires: removal' and update handoff to reference F562, OR (B) Include fl-workflow requires: removal in THIS feature (F560) scope since both files have the same issue and same fix pattern.

**User Decision Required**: Should Feature 560 expand scope to include both run-workflow AND fl-workflow, or create new Feature F562 for fl-workflow?

## Phase 3 - Maintainability (Iteration 0)

### Critical Issue: Unverified Problem Statement

**Type**: unverified_assumption
**Location**: Background/Problem section
**Issue**: Philosophy/Goal assumes `requires:` causes batch loading without verification. Problem states 'IF the Skill loader processes this directive' - this is speculation, not confirmed behavior. Feature may be solving a non-existent problem.
**Fix Required**: Add AC#0: Verify `requires:` frontmatter causes batch loading. If `requires:` is ignored by Skill loader, feature becomes unnecessary and should be closed as IRRELEVANT.

**User Decision Required**: Should this feature proceed with investigation AC, or should the assumption be verified outside the feature first?

## Phase 8 - Handoff Validation (Iteration 0)

### Critical Issue: Invalid Handoff Reference Confirmed

**Type**: invalid_handoff_tracking
**Location**: Handoff Tracking section (line 144)
**Issue**: Referenced feature F561 exists but is [DONE] and covers "FL Parallel Execution Mode", not "fl-workflow requires: removal". The handoff tracking destination is incorrect.
**Fix Required**: Either: (A) Create new Feature F562 for "fl-workflow requires: removal" and update handoff to F562, OR (B) Expand Feature 560 scope to include both run-workflow AND fl-workflow requires: removal.

**User Decision Required**: Should Feature 560 expand scope to include fl-workflow, or create new Feature F562?