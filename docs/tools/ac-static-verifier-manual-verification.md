# AC Static Verifier - Manual Verification Guide

## Purpose

This document provides guidance for manually verifying Acceptance Criteria (ACs) that return `MANUAL` status from ac-static-verifier.py.

## When MANUAL Status is Returned

The ac-static-verifier tool returns `MANUAL` status when it encounters verification patterns that cannot be executed automatically. The primary case is **slash commands**.

### Slash Commands

Slash commands (commands starting with `/`, such as `/audit`, `/commit`, `/reference-checker`) are Claude Code CLI commands, not shell commands. Per Testing SKILL line 79, these cannot be executed via subprocess.

**Example AC patterns that return MANUAL status:**

```markdown
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 8 | Documentation consistency | file | /audit | succeeds | - | [ ] |
| 15 | All links valid | file | /reference-checker | succeeds | - | [ ] |
```

## Why Slash Commands Cannot Be Executed Automatically

Slash commands are Claude Code features that:

1. Require interactive Claude Code session context
2. Cannot be invoked as standalone subprocess commands
3. May require user interaction or approval
4. Operate within Claude's agent framework

Attempting to execute them via `subprocess.run()` would fail because they are not executable shell commands.

## How to Perform Manual Verification

When you encounter a `MANUAL` status result in the verification logs, follow these steps:

### Step 1: Identify the Slash Command

Check the JSON output for the slash command details:

```json
{
  "ac_number": 8,
  "result": "MANUAL",
  "details": {
    "slash_command": "/audit",
    "matcher": "succeeds",
    "expected": "-",
    "manual_verification": "Slash commands require manual verification - execute the command and verify results manually"
  }
}
```

### Step 2: Execute the Slash Command

In your Claude Code session:

1. Invoke the slash command directly
   - Example: Type `/audit` in the Claude Code interface
2. Wait for the command to complete
3. Observe the output

### Step 3: Verify Against Expected Outcome

Check the `matcher` field to determine the expected outcome:

| Matcher | Verification Method |
|---------|---------------------|
| `succeeds` | Command should complete without errors (exit code 0 equivalent) |
| `fails` | Command should fail or report issues (exit code ≠ 0 equivalent) |
| `contains` | Command output should contain the expected string |
| `not_contains` | Command output should NOT contain the expected string |

### Step 4: Record the Result

After manual verification:

1. Update the feature file's AC Status column
   - `[x]` if verification passed
   - `[-]` if verification failed
2. Document any issues in the feature file's execution log or review notes

## Common Slash Commands

| Command | Purpose | Expected Behavior |
|---------|---------|-------------------|
| `/audit` | Documentation consistency check | Succeeds if no SSOT violations found |
| `/commit` | Create git commit with logical grouping | Succeeds if commit created successfully |
| `/reference-checker` | Validate markdown links and references | Succeeds if all links resolve correctly |
| `/fl` | Feature review-fix loop | Succeeds if feature passes all review checks |
| `/next` | Propose next work item | Succeeds if valid feature proposal created |
| `/run` | Execute feature implementation | Succeeds if feature implementation completes |

## Testing SKILL Reference

For comprehensive AC verification patterns and testing methodology, refer to:

- [Testing SKILL](../.claude/skills/testing/SKILL.md) - AC Definition Format (line 34+)
- Testing SKILL line 79 - Slash command exception documentation
- [INFRA.md](../pm/reference/feature-quality/INFRA.md) Issue 19 - Slash command AC pattern

## Troubleshooting

### Q: Can I automate slash command verification?

A: No. Slash commands require Claude Code's interactive session context and cannot be executed as standalone subprocess commands. Manual verification is the only supported approach.

### Q: What if the slash command doesn't exist?

A: This indicates a potential error in the AC definition. Verify that:
1. The slash command name is correct
2. The command is available in your Claude Code version
3. The AC Definition references the correct verification pattern

### Q: Should I report MANUAL status as a failure?

A: No. `MANUAL` status is distinct from `FAIL` status:
- `FAIL` = Automated verification detected a problem
- `MANUAL` = Automated verification is not possible, manual action required
- The AC may still pass after manual verification

## Related Documents

- [ac-static-verifier.py](../tools/ac-static-verifier.py) - Static verifier implementation
- [test_ac_static_verifier_manual.py](../tools/tests/test_ac_static_verifier_manual.py) - Unit tests for manual verification functionality
- [Feature 601](../pm/features/feature-601.md) - Implementation details for slash command handling

## Exit Code Behavior

When ACs return `MANUAL` status, the ac-static-verifier exit code behavior is:

- Exit code 0: All ACs passed OR all ACs are MANUAL (no failures)
- Exit code 1: One or more ACs failed

`MANUAL` status does not cause exit code 1 because it requires human judgment, not automated failure detection.

## Summary Statistics

The JSON output includes summary statistics for MANUAL status:

```json
{
  "summary": {
    "total": 15,
    "passed": 10,
    "manual": 3,
    "failed": 2
  }
}
```

This helps you identify how many ACs require manual attention versus automated verification.
