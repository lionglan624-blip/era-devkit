# Feature 618: ac-static-verifier MANUAL Status Counting Fix

## Status: [CANCELLED]

**Rationale**: Investigation confirmed no bug exists - MANUAL status is correctly excluded from failed count (line 586: `failed = total - passed - manual`). The described problem was based on incorrect understanding of the code logic.

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **RECORD** - Add to Handoff section with concrete destination
> 3. **CONTINUE** - Return to in-scope work
>
> **Definition**: This feature fixes MANUAL status exit code logic only. Additional ac-static-verifier enhancements or workflow changes are out of scope.

## Type: infra

## Background

### Philosophy
Tool Reliability - Testing tools should provide accurate status reporting that reflects actual verification state. MANUAL ACs require human verification but should not be treated as failures when the tool cannot execute them automatically.

### Problem
**INVESTIGATION COMPLETE**: F609 discovery was based on misunderstanding. Current ac-static-verifier.py correctly handles MANUAL status:
- Line 586: `failed = total - passed - manual` EXCLUDES MANUAL from failed count
- Line 612: `return 0 if failed == 0 else 1` returns success when only MANUAL ACs remain
- MANUAL ACs do NOT cause exit code 1 - the math explicitly subtracts manual from failed count

**CONCLUSION**: No bug exists. This feature is unnecessary as described.

### Goal
Fix ac-static-verifier.py exit code logic to treat MANUAL status ACs appropriately - either as neutral (not contributing to failure count) or as requiring separate handling, ensuring exit code 0 when only MANUAL ACs remain unverified.

## Acceptance Criteria

**OBSOLETE**: All ACs removed due to incorrect problem premise. The bug these ACs were designed to fix does not exist in the current code.

## AC Details

**AC#1**: Exit code logic modification
- Test: `Grep(tools/ac-static-verifier.py)` for revised exit code calculation
- Expected: Logic ignores MANUAL count when determining failure

**AC#2**: MANUAL-only scenario returns success
- Test: Run ac-static-verifier on feature with only MANUAL ACs
- Expected: Exit code 0 (success)

**AC#3**: Console output clarity
- Test: `Grep(tools/ac-static-verifier.py)` for console message
- Expected: Clear indication of MANUAL verification requirement

**AC#4**: JSON summary structure preserved
- Test: `Grep(tools/ac-static-verifier.py)` for JSON structure
- Expected: "manual" field remains in summary output

**AC#5**: Failed count calculation accuracy
- Test: `Grep(tools/ac-static-verifier.py)` for calculation logic
- Expected: `failed = total - passed` (excluding manual from denominator)

**AC#6**: Documentation consistency
- Test: `Grep(tools/ac-static-verifier.py)` for docstring/comments
- Expected: MANUAL status exit code behavior documented

**AC#7**: Integration test with MANUAL AC
- Test: Execute ac-static-verifier.py with feature containing slash command AC
- Expected: Exit code 0 when only MANUAL ACs present

**AC#8**: Regression test for actual failures
- Test: Execute ac-static-verifier.py with feature containing actual failed AC
- Expected: Exit code 1 maintained for genuine failures

## Tasks

**OBSOLETE**: All tasks removed since no work is required. The described bug does not exist in the current code.

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Step | Action | Details |
|:----:|--------|---------|
| 1 | Analyze current logic | Review tools/ac-static-verifier.py lines 586, 612 for exit code calculation |
| 2 | Modify calculation | Change line 586: `failed = total - passed` (remove `- manual`) |
| 3 | Update console output | Enhance line 609 message to clarify MANUAL requirement |
| 4 | Update documentation | Add MANUAL exit code behavior to docstring (lines 8-9) |
| 5 | Create test scenarios | Use existing feature with slash command AC for testing |
| 6 | Verify regression | Ensure actual failed ACs still return exit code 1 |

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F609 | [DONE] | Discovered the MANUAL status counting issue |

## Review Notes

## Mandatory Handoffs

| Issue | Destination | Description |
|-------|-------------|-------------|
| Documentation gap | Future feature | Document MANUAL status behavior for tool users - current behavior is correct but undocumented |
| Testing gap | Future feature | Add regression test for genuine failures to ensure MANUAL handling doesn't mask real problems |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|

## Links

[index-features.md](index-features.md)
[feature-609.md](feature-609.md) - Predecessor: Discovered the issue