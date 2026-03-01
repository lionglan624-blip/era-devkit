# Feature 600: ac-static-verifier Slash Command Matcher Support

## Status: [CANCELLED]

**Cancellation Reason**: Slash commands cannot be executed via subprocess per Testing SKILL line 79. Design is fundamentally impossible. Alternative approach implemented in F601.

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

### Philosophy (思想・上位目標)
Test-Driven Development (TDD) - Acceptance Criteria verification should be comprehensive and automated. ac-static-verifier.py serves as a critical testing infrastructure component that must support all documented AC patterns in Testing SKILL, including slash command verification patterns, to ensure complete AC coverage and reliable TDD workflows.

### Problem (現状の問題)
ac-static-verifier.py does not support the 'file | /command | succeeds' pattern documented in Testing SKILL for slash commands. This causes verify-logs to report ERR for ACs using slash command verification (e.g., AC#13 in F590 using /audit). The tool currently only supports code/build/file types with traditional matchers but lacks support for slash command execution verification.

### Goal (このFeatureで達成すること)
Add slash command matcher support to ac-static-verifier so that ACs with 'file | /command | succeeds' pattern are properly verified and reported as PASS when the command succeeds. This will complete the AC verification infrastructure for infra-type features that rely on slash commands for documentation consistency checking.

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Slash command pattern recognized | code | Grep(tools/ac-static-verifier.py) | contains | "slash command" | [ ] |
| 2 | File type handler modified | code | Grep(tools/ac-static-verifier.py) | contains | "verify_file_ac" | [ ] |
| 3 | Command execution logic added | code | Grep(tools/ac-static-verifier.py) | contains | "_execute_slash_command" | [ ] |
| 4 | Pattern detection logic | code | Grep(tools/ac-static-verifier.py) | matches | "^/[a-z-]+$" | [ ] |
| 5 | Succeeds matcher for slash commands | code | Grep(tools/ac-static-verifier.py) | contains | "succeeds" | [ ] |
| 6 | F590 AC#13 verification passes | exit_code | python tools/ac-static-verifier.py --feature 590 --ac-type file | succeeds | - | [ ] |
| 7 | Build verification passes | build | dotnet build | succeeds | - | [ ] |
| 8 | No technical debt markers | code | Grep(tools/ac-static-verifier.py) | not_contains | "TODO|FIXME|HACK" | [ ] |
| 9 | Error handling for failed slash commands | code | Grep(tools/ac-static-verifier.py) | contains | "error_details" | [ ] |
| 10 | Unit test for slash command pattern | file | Glob | exists | tools/tests/test_ac_static_verifier_slash.py | [ ] |
| 11 | Unit test passes | exit_code | python -m pytest tools/tests/test_ac_static_verifier_slash.py | succeeds | - | [ ] |
| 12 | Documentation updated | code | Grep(tools/ac-static-verifier.py) | matches | "Execute slash command.*docstring" | [ ] |

### AC Details

**AC#1**: Slash command pattern recognition
- Test: Grep for slash command pattern handling in ac-static-verifier.py
- Expected: Code recognizes slash command patterns (starting with /)

**AC#2**: File type handler modification
- Test: Grep for slash command handling in verify_file_ac method
- Expected: verify_file_ac method includes slash command execution path

**AC#3**: Command execution logic
- Test: Grep for _execute_slash_command method implementation
- Expected: New method handles slash command subprocess execution

**AC#4**: Pattern detection logic
- Test: Grep for regex pattern that matches slash commands
- Expected: Pattern correctly identifies commands like /audit, /commit, etc.

**AC#5**: Succeeds matcher support
- Test: Grep for succeeds matcher handling with slash commands
- Expected: Code properly handles succeeds matcher for slash command exit codes

**AC#6**: F590 AC#13 passes verification
- Test: Run ac-static-verifier on F590 file type ACs
- Expected: AC#13 with /audit command reports PASS instead of ERR

**AC#7**: Build verification
- Test: dotnet build succeeds
- Expected: No compilation errors after changes

**AC#8**: Clean code without technical debt
- Test: Grep for TODO/FIXME/HACK markers
- Expected: Zero matches (no technical debt introduced)

**AC#9**: Error handling for command failures
- Test: Grep for error handling when slash commands fail
- Expected: Proper error details returned when command fails

**AC#10**: Unit test file exists
- Test: Check for dedicated test file for slash command functionality
- Expected: test_ac_static_verifier_slash.py exists

**AC#11**: Unit tests pass
- Test: Run pytest on the slash command test file
- Expected: All tests pass

**AC#12**: Documentation updated
- Test: Grep for slash command documentation in docstrings
- Expected: Code includes documentation for slash command support

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add slash command pattern recognition to file type handler | [ ] |
| 2 | 2 | Modify verify_file_ac method for slash command detection | [ ] |
| 3 | 3 | Implement _execute_slash_command method | [ ] |
| 4 | 4 | Add regex pattern for slash command detection | [ ] |
| 5 | 5 | Support succeeds matcher for slash command exit codes | [ ] |
| 6 | 6 | Test F590 AC#13 verification with updated tool | [ ] |
| 7 | 7 | Verify build passes after implementation | [ ] |
| 8 | 8 | Ensure no technical debt markers remain | [ ] |
| 9 | 9 | Add error handling for failed slash commands | [ ] |
| 10 | 10 | Create unit test file for slash command functionality | [ ] |
| 11 | 11 | Implement and verify unit tests pass | [ ] |
| 12 | 12 | Update docstrings and inline documentation | [ ] |

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Slash Command Detection Logic

The implementation must detect slash commands in the Method field:

1. Check if Method field contains pattern matching `^/[a-z-]+$` (slash followed by lowercase letters and hyphens only - Claude Code slash commands use hyphen-delimited format)
2. For detected slash commands, use subprocess execution instead of file content verification
3. Support only `succeeds` and `fails` matchers for slash command execution

### Command Execution Implementation

```python
def _execute_slash_command(self, command: str) -> Dict[str, Any]:
    """Execute slash command and return result.

    Args:
        command: Slash command (e.g., "/audit")

    Returns:
        Dict with command_result, exit_code, stdout, stderr
    """
```

### Integration Point

Modify `verify_file_ac` method to:

1. Detect slash command pattern in Method field
2. Delegate to `_execute_slash_command` for execution
3. Apply succeeds/fails matcher logic to exit code
4. Return standard result format compatible with existing JSON output

### Test Requirements

Create `tools/tests/test_ac_static_verifier_slash.py` with:

1. Test slash command pattern detection
2. Test successful command execution (mock /audit success)
3. Test failed command execution (mock command failure)
4. Test matcher logic for succeeds/fails
5. Test JSON output format consistency

### Error Handling Requirements

1. Handle command not found scenarios
2. Handle command execution timeouts (30 second limit)
3. Return appropriate error details in result JSON
4. Maintain consistent error format with existing code

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Related | F590 | [WIP] | YAML Schema Validation Tools - Contains AC#13 that requires slash command support |

## Review Notes
- [resolved-invalid] Phase1 iter1: AC#6 clarification: Type is 'exit_code' but F590 AC#13 uses 'file | /audit | succeeds' pattern. Consider clarifying Method description to explain this AC verifies F590's file type AC#13 passes via verifier tool exit code.
- [pending] Phase1 iter2: Implementation Contract execution mechanism: _execute_slash_command method signature does not specify how slash commands are executed. Slash commands like '/audit' are Claude Code internal commands, not shell commands. Clarify execution mechanism.
- [pending] Phase1 iter3: Slash commands cannot be executed via subprocess: Testing SKILL line 79 explicitly states slash commands are not shell commands. The current design is impossible to implement. Alternative verification mechanism required.
- [pending] Phase1 iter3: AC#4 pattern detection logic: AC expects regex pattern '^/[a-z-]+$' to be found in code using matches matcher. The design may be flawed - unclear if this verifies code presence or runtime behavior.
- [resolved-invalid] Phase1 iter4: ACs designed for impossible implementation: AC#3 is impossible but AC#4 and AC#12 are valid code presence checks.
- [pending] Phase1 iter4: CRITICAL: Fundamental design flaw - entire feature premise is impossible. Slash commands cannot be executed via subprocess. Testing SKILL line 79 confirms this. Feature requires complete redesign with alternative approach.
- [pending] Phase1 iter4: AC#6 verification logic: F590 AC#13 uses slash command pattern but ac-static-verifier cannot execute slash commands. Clarify what "verification passes" means.
- [pending] Phase1 iter4: Pattern naming restrictions: Pattern '^/[a-z-]+$' may be overly restrictive for Claude Code slash command naming conventions.
- [pending] Phase2 iter4: CRITICAL Philosophy violation: Philosophy requires all documented AC patterns but Testing SKILL confirms slash commands cannot be executed via subprocess. Complete redesign required.
- [pending] Phase2 iter4: AC#3 and AC#6 impossible: Both assume subprocess execution which is impossible. Must be redesigned or removed.
- [pending] Phase2 iter4: Implementation Contract invalid: _execute_slash_command cannot work as specified. Contract must be rewritten for alternative approach.
- [pending] Phase2 iter4: Unresolved pending issues: Four pending issues identify fundamental flaw but feature was not updated before FL review.
- [pending] Phase2 iter4: Goal vs Implementation mismatch: Goal requires PASS for slash commands but this is impossible with current approach.
- [pending] Phase3 iter4: AC#3 impossible implementation: _execute_slash_command cannot be implemented via subprocess per Testing SKILL line 79.
- [pending] Phase3 iter4: AC#6 impossible verification: F590 AC#13 uses slash command but subprocess execution is impossible.
- [pending] Phase3 iter4: AC#4 pattern semantics unclear: Expected contains pattern to find, not pattern to match against.
- [pending] Phase3 iter4: AC#12 overly specific pattern: 'Execute slash command.*docstring' may not match actual docstring format.
- [pending] Phase3 iter4: Implementation Contract subprocess impossible: Method signature specifies impossible subprocess execution.
- [pending] Phase4 iter4: CRITICAL FEASIBILITY: Slash commands cannot be executed via subprocess - feature is impossible to implement as designed per Testing SKILL line 79.
- [pending] Phase4 iter4: AC#4 feasibility: Expected pattern search is semantically incorrect - searches for pattern itself, not regex implementation.
- [pending] Phase4 iter4: AC#12 feasibility: Expected docstring pattern may not match actual ac-static-verifier.py docstring format.
- [pending] Phase4 iter4: Goal vs Implementation feasibility: Goal requires F590 AC#13 PASS but ac-static-verifier cannot execute slash commands.
- [applied] Phase6 iter4: CRITICAL Design contradiction: Feature documents impossibility of slash command execution in Review Notes but Goal and ACs still assume subprocess execution. Complete redesign required. → USER DECISION: Status changed to [BLOCKED], follow-up feature required for complete redesign.
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

## Mandatory Handoffs
<!-- If issues exist → All rows MUST have tracking destination. Empty/TBD/未定 forbidden. FL will FAIL -->

| Issue | Description | Tracking Destination | Notes |
|-------|-------------|---------------------|--------|
| Complete redesign required | Slash command execution impossible via subprocess per Testing SKILL line 79. Alternative verification mechanism needed. | F601 | User selected option A: Complete redesign with alternative approach |

## 引継ぎ先指定 (Philosophy Gate Deferrals)

Following tasks derived from Philosophy analysis but technically impossible in current feature due to [BLOCKED] status:

1. **Survey Testing SKILL for all documented AC patterns** - Follow-up feature must enumerate all patterns before implementation
2. **Analyze gap between current ac-static-verifier capabilities and documented patterns** - Prerequisite analysis required
3. **Research slash command execution mechanism** - Critical investigative phase missing
4. **Determine if slash commands are executable or require alternative verification approach** - Feasibility determination required before design

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

## Links
[index-features.md](index-features.md)
[feature-590.md](feature-590.md) - Related: YAML Schema Validation Tools