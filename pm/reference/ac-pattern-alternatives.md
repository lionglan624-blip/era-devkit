# AC Pattern Alternatives

> **Last Updated:** 2026-01-24
> **Purpose:** AC verification pattern mapping to alternative verification approaches
> **Reference:** [Testing SKILL](../../../.claude/skills/testing/SKILL.md) - AC Definition Format (lines 34-83)

---

## Overview

This document maps Testing SKILL AC verification patterns to alternative verification methods. When standard subprocess execution is not feasible (e.g., slash commands, interactive tools), these alternatives provide actionable verification approaches.

**Use Cases:**
- Slash commands requiring Claude Code session context
- Interactive verification scenarios
- Manual validation of automated tool results
- Fallback approaches when subprocess execution fails

---

## Pattern Type Mappings

The following table maps each of the 7 AC Types documented in Testing SKILL to their standard and alternative verification methods.

| Pattern Type | Matcher | Standard Method | Alternative Method |
|:------------:|---------|-----------------|-------------------|
| **output** | contains | --unit / --flow subprocess execution | Manual test execution with visual output inspection |
| **output** | equals | --unit / --flow subprocess execution | Manual test execution with exact output comparison |
| **output** | not_contains | --unit / --flow subprocess execution | Manual test execution verifying absence of text |
| **variable** | equals | --unit (dump command) subprocess | Manual debug session with dump command verification |
| **variable** | gt/gte/lt/lte | --unit (dump command) subprocess | Manual debug session with numeric comparison |
| **build** | succeeds | dotnet build subprocess | Manual build execution with exit code observation |
| **build** | fails | dotnet build subprocess | Manual build execution expecting compilation errors |
| **exit_code** | equals | Script execution with $? check | Manual script execution with exit code verification |
| **exit_code** | succeeds | Script execution expecting 0 | Manual execution verifying successful completion |
| **exit_code** | fails | Script execution expecting ≠0 | Manual execution verifying error condition |
| **file** | exists | Glob pattern matching | Manual file system inspection (ls/dir) |
| **file** | not_exists | Glob pattern matching | Manual file system inspection verifying absence |
| **file** | contains | Grep content search | Manual file reading with text search |
| **file** | matches | Grep regex search | Manual file reading with pattern verification |
| **code** | contains | Grep content search | Manual code inspection with text editor search |
| **code** | not_contains | Grep content search | Manual code inspection verifying absence |
| **code** | matches | Grep regex search | Manual code inspection with pattern verification |
| **test** | succeeds | dotnet test subprocess | Manual test execution with result observation |
| **test** | fails | dotnet test subprocess | Manual test execution expecting failure |

---

## Alternative Method Details

### When to Use Alternative Methods

Alternative verification methods are required when:

1. **Slash Commands**: Commands starting with `/` (e.g., `/audit`, `/reference-checker`) cannot be executed via subprocess
   - **Reason**: Slash commands are Claude Code CLI features requiring interactive session context
   - **Reference**: Testing SKILL line 79 - Slash command exception documentation

2. **Interactive Tools**: Tools requiring user input or approval
   - **Reason**: Subprocess execution cannot provide interactive responses
   - **Example**: `--debug` mode with manual command input

3. **Environment Constraints**: Subprocess execution is blocked or unavailable
   - **Reason**: Permission restrictions, sandboxing, or execution environment limitations
   - **Example**: Some CI/CD environments restrict subprocess creation

### Exact Verification Steps

#### output Type

**Standard Method:**
```bash
cd Game
dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/ac/kojo/feature-{N}/test.json
```

**Alternative Method:**
1. Execute the test command manually in terminal
2. Visually inspect the output for expected text (contains matcher)
3. Copy output to text file for exact comparison (equals matcher)
4. Verify absence of specific text (not_contains matcher)
5. Document the result in feature file AC Status column

**Example:**
```
AC#1: Screen output contains "最近一緒にいると"
Standard: --unit subprocess with assert_contains
Alternative: Manual --unit execution, visual verification of output text
```

#### variable Type

**Standard Method:**
```bash
cd Game
dotnet run --project ../engine/uEmuera.Headless.csproj -- . --debug --char 1 --input-file tests/test.txt
# Input: {"cmd":"dump","vars":["FLAG:0"]}
```

**Alternative Method:**
1. Execute debug mode manually
2. Send dump command via stdin
3. Observe the JSON response with variable values
4. Compare the value against expected (equals matcher)
5. Verify numeric comparison (gt/gte/lt/lte matchers)

**Example:**
```
AC#2: FLAG:0 equals 12345
Standard: --debug subprocess with dump command assertion
Alternative: Manual --debug session, send dump command, verify JSON response
```

#### build Type

**Standard Method:**
```bash
dotnet build Era.Core
```

**Alternative Method:**
1. Execute `dotnet build` manually in terminal
2. Observe the exit code (`$?` in bash, `$LASTEXITCODE` in PowerShell)
3. Check for "Build succeeded" message (succeeds matcher)
4. Check for compilation errors (fails matcher)
5. Document the result

**Example:**
```
AC#3: Build succeeds
Standard: dotnet build subprocess with exit code check
Alternative: Manual dotnet build execution, observe "Build succeeded" message
```

#### exit_code Type

**Standard Method:**
```bash
python tools/script.py --option value
echo $?  # Check exit code
```

**Alternative Method:**
1. Execute the command manually in terminal
2. Immediately check exit code using `echo $?` (bash) or `echo $LASTEXITCODE` (PowerShell)
3. Verify the code matches expected value (equals matcher)
4. Verify successful completion (succeeds matcher, expecting 0)
5. Verify error condition (fails matcher, expecting ≠0)

**Example:**
```
AC#4: Script exits with code 0
Standard: subprocess.run() with returncode check
Alternative: Manual script execution, echo $? verification
```

#### file Type

**Standard Method:**
```bash
# exists/not_exists matcher
ls pm/features/feature-*.md

# contains/matches matcher
grep "## Status" pm/features/feature-609.md
```

**Alternative Method:**
1. **For exists/not_exists**: Navigate to directory and use `ls` (bash) or `dir` (PowerShell)
2. **For contains/matches**: Open file in text editor and use editor search function
3. Verify file presence/absence or content pattern
4. Document the result

**Example:**
```
AC#5: File pm/reference/ac-pattern-alternatives.md exists
Standard: Glob pattern matching
Alternative: Manual ls/dir command, visual verification of file presence
```

#### code Type

**Standard Method:**
```bash
grep "Skill(testing)" .claude/commands/run.md
```

**Alternative Method:**
1. Open the code file in text editor (VSCode, Vim, etc.)
2. Use editor's search function (Ctrl+F / Cmd+F)
3. Search for the expected pattern (contains matcher)
4. Verify absence of pattern (not_contains matcher)
5. Verify regex match (matches matcher)

**Example:**
```
AC#6: run.md contains "Skill(testing)"
Standard: Grep content search
Alternative: Open file in VSCode, Ctrl+F search for "Skill(testing)"
```

#### test Type

**Standard Method:**
```bash
dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s --logger "trx;LogFileName=test-result.trx"
```

**Alternative Method:**
1. Execute `dotnet test` manually in terminal
2. Observe the test output for pass/fail status
3. Check the summary line (succeeds: "Passed!" count, fails: "Failed!" count)
4. Review .trx file if generated
5. Document the result

**Example:**
```
AC#7: C# unit test succeeds
Standard: dotnet test subprocess with .trx parsing
Alternative: Manual dotnet test execution, observe "Test Run Successful" message
```

### Limitations

Alternative verification methods have inherent limitations compared to automated approaches:

1. **Manual Effort**: Requires human execution time for each verification
2. **Reproducibility**: Manual steps may vary between executions
3. **Scalability**: Cannot be integrated into automated CI/CD pipelines
4. **Error Prone**: Human observation may miss subtle differences
5. **Documentation Overhead**: Results must be manually recorded
6. **No Regression Protection**: Cannot be re-run automatically when code changes

**Recommendation**: Use alternative methods only when standard automated verification is not feasible. Document the use of alternative methods in feature execution logs for auditability.

---

## Slash Command Special Case

Slash commands (e.g., `/audit`, `/commit`, `/reference-checker`) require Claude Code session context and cannot be executed via subprocess.

**Detection**: AC Method column contains slash command pattern (`/command-name`)

**Verification Approach**:
1. Invoke slash command in Claude Code session
2. Wait for command completion
3. Verify expected outcome based on matcher:
   - `succeeds`: Command completes without errors
   - `fails`: Command reports issues or errors
   - `contains`: Command output contains expected text
4. Update AC Status column manually

**Reference**: [ac-static-verifier-manual-verification.md](../../../tools/ac-static-verifier-manual-verification.md) - Comprehensive slash command verification guide

---

## Related Documents

- [Testing SKILL](../../../.claude/skills/testing/SKILL.md) - AC Definition Format, AC Types (lines 34-52)
- [ac-static-verifier-manual-verification.md](../../../tools/ac-static-verifier-manual-verification.md) - Manual verification guide for MANUAL status results
- [INFRA.md](../../../.claude/skills/feature-quality/INFRA.md) Issue 19 - Slash command AC pattern
- [feature-601.md](../feature-601.md) - ac-static-verifier implementation with slash command handling

---

## Summary

This reference document provides structured mappings from Testing SKILL AC patterns to alternative verification approaches. Each of the 7 AC Types (output, variable, build, exit_code, file, code, test) has documented alternative methods for scenarios where subprocess execution is not feasible.

Alternative methods prioritize actionable implementation guidance over theoretical mapping, enabling developers to manually verify ACs when automated verification fails or is unavailable.
