# Feature 578: Bash TDD Protection Extension

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

TDD protection for each directory should be comprehensive while maintaining tool-type separation.
Currently F568's pre-tdd-protection.ps1 covers Write|Edit tools for both tests/ac/ and Era.Core.Tests/. pre-bash-ac.ps1 covers Bash commands only for tests/ac/, creating a coverage gap for Era.Core.Tests/ Bash operations. This architecture maintains tool-type boundaries (Write|Edit vs Bash) while ensuring complete directory protection.

### Problem (Current Issue)

TDD protection is split across:
- F568 pre-tdd-protection.ps1: Write|Edit tools
- pre-bash-ac.ps1: Bash commands (sed -i, rm, mv, redirects, git operations)

This violates SSOT and creates maintenance burden.

### Goal (What to Achieve)

Extend Bash TDD protection by extending pre-bash-ac.ps1 to cover Era.Core.Tests/, eliminating coverage gap while maintaining separation of Write|Edit vs Bash hook responsibilities.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | sed -i pattern covers Era.Core.Tests | file | Grep(.claude/hooks/pre-bash-ac.ps1) | contains | "Era\\.Core\\.Tests" | [x] |
| 2 | rm/mv pattern covers Era.Core.Tests | file | Grep(.claude/hooks/pre-bash-ac.ps1) | contains | "Era\\.Core\\.Tests" | [x] |
| 3 | redirect pattern covers Era.Core.Tests | file | Grep(.claude/hooks/pre-bash-ac.ps1) | contains | "Era\\.Core\\.Tests" | [x] |
| 4 | rmdir pattern covers Era.Core.Tests | file | Grep(.claude/hooks/pre-bash-ac.ps1) | contains | "Era\\.Core\\.Tests" | [x] |
| 5 | rm -rf pattern covers Era.Core.Tests | file | Grep(.claude/hooks/pre-bash-ac.ps1) | contains | "Era\\.Core\\.Tests" | [x] |
| 6 | git operation pattern covers Era.Core.Tests | file | Grep(.claude/hooks/pre-bash-ac.ps1) | contains | "Era\\.Core\\.Tests" | [x] |
| 7 | git error message includes TDD PRINCIPLE | file | Grep(.claude/hooks/pre-bash-ac.ps1) | contains | "TDD PRINCIPLE" | [x] |
| 8 | Hook blocks sed -i on Era.Core.Tests | exit_code | Script(.claude/hooks/pre-bash-ac.ps1) | equals | 2 | [x] |
| 9 | Hook allows sed -i on non-protected path | exit_code | Script(.claude/hooks/pre-bash-ac.ps1) | equals | 0 | [x] |

### AC Details

**AC#1-6**: Extend all Bash operation patterns to cover Era.Core.Tests/
- AC#1: sed -i operations protected
- AC#2: rm/mv operations protected
- AC#3: redirect operations protected
- AC#4: rmdir operations protected
- AC#5: rm -rf operations protected
- AC#6: git operations protected

**AC#7**: Git error message includes TDD PRINCIPLE
- Currently git operation error message is missing "TDD PRINCIPLE" text
- Ensures consistent error message format across all TDD protection patterns

**AC#8-9**: Hook functional testing
- AC#8: Script test with mock JSON input targeting Era.Core.Tests, expects exit code 1 (blocked)
  ```
  $json = '{"tool_input": {"command": "sed -i s/foo/bar/ Era.Core.Tests/test.cs"}}'
  $json | pwsh -File .claude/hooks/pre-bash-ac.ps1 ; $LASTEXITCODE -eq 1
  ```
- AC#9: Script test with mock JSON input targeting non-protected path, expects exit code 0 (allowed)
  ```
  $json = '{"tool_input": {"command": "sed -i s/foo/bar/ src/test.cs"}}'
  $json | pwsh -File .claude/hooks/pre-bash-ac.ps1 ; $LASTEXITCODE -eq 0
  ```
- Note: AC#8-9 test sed -i operation as representative sample. Other operations (rm/mv, redirect, rmdir, rm -rf, git) follow same pattern logic in pre-bash-ac.ps1

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add Era.Core.Tests/ pattern to sed -i operation check | [x] |
| 2 | 2 | Add Era.Core.Tests/ pattern to rm/mv operation check | [x] |
| 3 | 3 | Add Era.Core.Tests/ pattern to redirect operation check | [x] |
| 4 | 4 | Add Era.Core.Tests/ pattern to rmdir operation check | [x] |
| 5 | 5 | Add Era.Core.Tests/ pattern to rm -rf operation check | [x] |
| 6 | 6 | Add Era.Core.Tests/ pattern to git operation check | [x] |
| 7 | 7 | Fix git operation error message to include TDD PRINCIPLE | [x] |
| 8 | 8 | Test hook blocks Era.Core.Tests operations | [x] |
| 9 | 9 | Test hook allows non-protected path operations | [x] |

---

## Implementation Contract

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| `.claude/hooks/pre-bash-ac.ps1` patterns 1-6 | Add Era.Core.Tests/ patterns to: sed -i, rm/mv, redirect, rmdir, rm -rf, git operation checks | Extends Bash TDD protection to C# test files (Era.Core.Tests/), closes coverage gap |
| Git operation error message | Update to include "TDD PRINCIPLE" text | Consistent user experience with F568 pre-tdd-protection.ps1 and other operation patterns |
| Hook behavioral testing | AC#8-9 use Script test method with mock JSON input to verify blocking behavior | Ensures hook actually blocks Era.Core.Tests operations, not just pattern presence verification |
| Hook selection mechanism | pre-bash-ac.ps1 triggers on Bash tool type, pre-tdd-protection.ps1 triggers on Write|Edit tool types | Maintains clear tool-type boundaries preventing hook overlap/conflict |

### Rollback Plan

If issues arise after deployment:
1. `git revert` to restore original pre-bash-ac.ps1
2. Re-evaluate hook consolidation strategy
3. Create follow-up feature for alternative approach

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F567 | [DONE] | Claude Code Hooks Cleanup (must complete first to reveal Bash TDD requirements) |

---

## Review Notes

- [resolved-applied] Phase1 iter6: Expected patterns verification approach clarified - ACs verify that Era.Core.Tests appears in each pattern (coverage presence), not full regex structure
- [resolved-applied] Phase1 iter8: Hook functional testing method specified - AC#8-9 use Script method with mock JSON input to test blocking behavior
- [resolved-applied] Phase2 iter1: Pattern modification strategy specified in Implementation Contract - extends existing patterns following pre-bash-ac.ps1 structure

### F567 Handoff (2026-01-21)

Created as handoff destination from F567 for Bash TDD protection consolidation.
Full specification pending F567 completion.

---

## Mandatory Handoffs

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-22 | DEVIATION | ac-static-verifier | AC#1-6 file verification | FAIL: Pattern "Era.Core.Tests" not found - file has escaped "Era\\.Core\\.Tests" (regex) |
| 2026-01-22 | DEVIATION | Bash | AC#8 exit_code verification | Actual exit code is 2 (not 1) - AC definition error, hook uses exit 2 |
| 2026-01-22 | DEVIATION | feature-reviewer | Post review | NEEDS_REVISION: Status [WIP] but all AC/Tasks complete - expected, Phase 9 updates |
| 2026-01-22 | DEVIATION | feature-reviewer | Doc-check | NEEDS_REVISION: testing SKILL.md missing Era.Core.Tests/ protection doc |

---

## Links

- [index-features.md](index-features.md)
- [feature-567.md](feature-567.md) - Predecessor: Claude Code Hooks Cleanup
- [feature-568.md](feature-568.md) - Related: TDD AC Protection Hook
