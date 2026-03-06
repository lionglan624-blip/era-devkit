# Feature 568: TDD AC Protection Hook

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

TDD principle: Tests created in Phase 3 are immutable during Phase 4 implementation.
Implementation MUST conform to tests, NOT the other way around.

### Problem (Current Issue)

Current protection gap for new test architecture:
- No TDD protection exists for new test locations
- `Era.Core.Tests/` needs protection (engine type)
- `Game/tests/ac/` needs protection (erb type, if used)
- Risk of TDD violations during /run workflow

### Goal (What to Achieve)

Create Claude Code hook that:
1. Allows new test file creation (Phase 3 RED)
2. Blocks modification of existing test files (Phase 4 GREEN)
3. Covers both `Era.Core.Tests/` and `Game/tests/ac/`

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Hook script exists | file | Glob | exists | `.claude/hooks/pre-tdd-protection.ps1` | [x] |
| 2 | settings.json has PreToolUse entry | file | Grep | contains | "pre-tdd-protection" | [x] |
| 3 | New test file creation allowed | exit_code | Script | succeeds | exit 0 | [x] |
| 4a | Existing test modification blocked (Edit) | exit_code | Script | fails | exit 2 | [x] |
| 4b | Existing test modification blocked (Write) | exit_code | Script | fails | exit 2 | [x] |
| 5 | Era.Core.Tests/ covered | code | Grep | contains | "Era\.Core\.Tests" | [x] |
| 6 | tests/ac/ covered | code | Grep | contains | "tests[/\\\\]ac[/\\\\].*\\.json" | [x] |
| 7 | Non-test files unaffected | exit_code | Script | succeeds | exit 0 | [x] |
| 8 | Clear error message on block | code | Grep | contains | "TDD PRINCIPLE" | [x] |

### AC Details

**AC#1-2**: Hook infrastructure
- Script at `.claude/hooks/pre-tdd-protection.ps1`
- Registered in `.claude/settings.json` PreToolUse for Write|Edit

**AC#3-4**: Core TDD protection
- New file (not exists) → Allow
- Existing file → Block with error
- Setup: Before running AC#4 tests, create empty test files: `Era.Core.Tests/ExistingTest.cs` and `Game/tests/ac/ExistingTest.json`
- Test commands (simulate Claude tool input):
  - AC#3: `$json = @'{"tool": "Write", "tool_input": {"file_path": "Era.Core.Tests/NewTest.cs"}}'@; $json | pwsh -File .claude/hooks/pre-tdd-protection.ps1; $LASTEXITCODE` → exit 0
  - AC#3 (tests/ac/): `$json = @'{"tool": "Write", "tool_input": {"file_path": "Game/tests/ac/NewTest.json"}}'@; $json | pwsh -File .claude/hooks/pre-tdd-protection.ps1; $LASTEXITCODE` → exit 0
  - AC#4a-Edit: `$json = @'{"tool": "Edit", "tool_input": {"file_path": "Era.Core.Tests/ExistingTest.cs"}}'@; $json | pwsh -File .claude/hooks/pre-tdd-protection.ps1; $LASTEXITCODE` → exit 2
  - AC#4a-Edit (tests/ac/): `$json = @'{"tool": "Edit", "tool_input": {"file_path": "Game/tests/ac/ExistingTest.json"}}'@; $json | pwsh -File .claude/hooks/pre-tdd-protection.ps1; $LASTEXITCODE` → exit 2
  - AC#4b-Write: `$json = @'{"tool": "Write", "tool_input": {"file_path": "Era.Core.Tests/ExistingTest.cs"}}'@; $json | pwsh -File .claude/hooks/pre-tdd-protection.ps1; $LASTEXITCODE` → exit 2
  - AC#4b-Write (tests/ac/): `$json = @'{"tool": "Write", "tool_input": {"file_path": "Game/tests/ac/ExistingTest.json"}}'@; $json | pwsh -File .claude/hooks/pre-tdd-protection.ps1; $LASTEXITCODE` → exit 2

**AC#5**: Era.Core.Tests coverage verification
- C# test directory protected
- Test command: `Select-String "Era\\.Core\\.Tests" .claude/hooks/pre-tdd-protection.ps1`
- Note: AC table shows `Era\.Core\.Tests` (regex notation) while test command uses `Era\\.Core\\.Tests` (PowerShell escaping). Actual hook regex uses `Era\.Core\.Tests[/\\].*\.cs`
- **Task Coverage**: AC#5 is verified by inspecting Task#1 output (hook script contains correct regex)

**AC#6**: tests/ac/ coverage verification
- JSON test scenarios protected
- Test command: `Select-String "tests[/\\\\]ac[/\\\\].*\\.json" .claude/hooks/pre-tdd-protection.ps1`
- Note: AC table shows regex notation while test command uses PowerShell escaping
- **Task Coverage**: AC#6 is verified by inspecting Task#1 output (hook script contains correct regex)

**AC#7**: Non-interference
- Only test directories affected
- Normal code editing works
- Test command: `@'{"tool": "Edit", "tool_input": {"file_path": "Game/ERB/SYSTEM.ERB"}}' | pwsh -File .claude/hooks/pre-tdd-protection.ps1; $LASTEXITCODE` → exit 0

**AC#8**: Developer experience
- Clear message explaining why edit was blocked
- Reference to TDD principle
- Test command (code check): `Select-String "TDD PRINCIPLE" .claude/hooks/pre-tdd-protection.ps1` (verify script contains error message)
- Test command (behavior check): `$err = @'{"tool": "Edit", "tool_input": {"file_path": "Era.Core.Tests/ExistingTest.cs"}}' | pwsh -File .claude/hooks/pre-tdd-protection.ps1 2>&1; echo $err | Select-String 'TDD PRINCIPLE'`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create pre-tdd-protection.ps1 hook script | [x] |
| 2 | 2 | Update settings.json PreToolUse | [x] |
| 3 | 3 | Test: new file creation allowed | [x] |
| 4 | 4a | Test: existing file edit blocked | [x] |
| 5 | 4b | Test: existing file write blocked | [x] |
| 6 | 5 | Verify: Era.Core.Tests/ regex in hook script | [x] |
| 7 | 6 | Verify: tests/ac/ regex in hook script | [x] |
| 8 | 7 | Test: non-test files unaffected | [x] |
| 9 | 8 | Test: error message verification | [x] |

---

## Implementation Contract

### Hook Logic

```
IF ((path matches Era.Core.Tests[/\\].*\.cs) OR (path matches tests[/\\]ac[/\\].*\.json)) AND NOT (path matches obj[/\\])
  IF file does NOT exist
    → ALLOW (exit 0) (Phase 3: create new test)
  ELSE
    → BLOCK (exit 2) (Phase 4: cannot modify existing test)
      Error: "TDD PRINCIPLE: Fix implementation, not the test. If test definition is incorrect, escalate to user for manual correction."
ELSE
  → ALLOW (exit 0) (not a test file OR build artifact)
```

**tests/regression/ Handling**: Explicitly excluded from F568 scope. Current state (before F567): pre-ac-write.ps1 blocks tests/regression/ + tests/ac/. Post-F567 state: pre-ac-write.ps1 removes both tests/regression/ and tests/ac/, F568 assumes full TDD protection responsibility.

**Transition Coordination**: F567 modifies pre-ac-write.ps1 to remove all test directory protection, transferring TDD protection authority to F568. F568's pre-tdd-protection.ps1 provides unified TDD protection for Era.Core.Tests/ + tests/ac/.

**Responsibility Boundary**: pre-tdd-protection.ps1 = TDD protection (Era.Core.Tests/ + tests/ac/). pre-ac-write.ps1 = deprecated (no active protection, retained for hook chain compatibility). After F567, pre-ac-write.ps1 removes tests/ac/ protection to avoid duplication with F568. Rationale: TDD principle requires unified test protection under single authority. F568 consolidates all TDD-related test protection for consistency.

**Build Artifact Exclusion**: obj/ directories contain auto-generated files (GlobalUsings.g.cs, AssemblyInfo.cs) that should not be TDD protected.

**File Type Coverage**: Era.Core.Tests/ protects all .cs files (test source code). tests/ac/ protects only .json files (AC scenario definitions) since .md files in tests/ac/ are documentation, not immutable test artifacts.

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| `.claude/hooks/pre-tdd-protection.ps1` | New file | TDD enforcement |
| `.claude/settings.json` | Add PreToolUse | Hook registration |

### Rollback Plan

If issues arise:
1. Remove hook from settings.json
2. Delete script file
3. TDD protection disabled (manual discipline required)

### Transition Gap Protection

**Dependency Note**: F567 has completed modifications to pre-ac-write.ps1, removing tests/ac/ protection. F568 was implemented first to ensure continuous TDD protection without gaps. The transition completed successfully with F568 providing consolidated TDD protection for both Era.Core.Tests/ and tests/ac/.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Blocked by (this) | F567 | [BLOCKED] | F567 is blocked by this feature |
| Blocked by (this) | F540 | [BLOCKED] | F540 is blocked by this feature |

---

## Review Notes

### [done] FL Review Issues (Iteration 1-9)

### [pending] Phase 2 Maintainability Issues (Iteration 1)

- [applied] Phase2-Maintainability iter1: Handoff tracking destination mismatch - Fixed: Updated Mandatory Handoffs table to remove invalid F576 reference, placeholder set for new persist_pending improvement feature
- [applied] Phase2-Maintainability iter1: Links section label mismatch - Fixed: Removed invalid F576 link from Links section

**Issue 1** [skipped]: AC:Task 1:1 - Task#1 covers AC#1,5,6. User decided to keep current structure (AC#5,6 are quality requirements of Task#1 output). Note: AC#5,6 are structural verification of Task#1 output. Tasks#6,7 are verification-only tasks with no implementation action.

**Issue 2** [applied]: Links section label - Changed "Predecessor" to "Successor" for F567.

**Issue 3** [skipped]: AC Details escaping note - Already documented intentionally.

**Issue 4** [done]: Duplicate protection - F568 and pre-ac-write.ps1 both protect tests/ac/. Intentional during transition: F568 adds protection first, F567 (currently [BLOCKED]) will remove duplicate after F568 completion. Tracked in Mandatory Handoffs.

**Issue 5** [skipped]: AC#2 precision - "pre-tdd-protection" is sufficiently unique.

**Issue 6-7** [skipped]: Escaping in AC table vs commands - Documentation style choice, not functional.

**Issue 8** [skipped]: Task#3 split - Current structure is contextually 1:1.

**Issue 9** [skipped]: AC#8 stderr capture - Implicit verification via exit code is adequate.

**Issue 10** [done]: PowerShell stdin piping syntax - Verified: direct pipe `echo '...' | pwsh -File` works in Git Bash. PowerShell here-string syntax not needed.

**Issue 11** [skipped]: AC#8 dual-test - Current approach covers both code and behavior.

**Issue 12** [applied]: Dependency type - Changed to standard "Successor" type.

---

## Mandatory Handoffs

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| persist_pending定義の導線不足 | SKILL.mdの定義(行189)を読まずPhase実行→誤ってpending_user.txt作成 | Feature | F582 |
| F567 coordination (remove tests/ac/ protection from pre-ac-write.ps1) | User decided: F568 consolidates TDD protection, F567 should remove tests/ac/ from pre-ac-write.ps1 | Feature | F567(existing) |

**引継ぎ詳細**: FL Phase-1等で`persist_pending()`初回使用時、SKILL.md定義への明示的Read指示がない。改善案: (A)コメント追加 (B)Prerequisites追加 (C)インライン化 (D)初回のみRead指示。ユーザと議論後、PHASE-1.md等に1-2行追加。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-20 21:53 | START | implementer | Task 1 | - |
| 2026-01-20 21:53 | END | implementer | Task 1 | SUCCESS |
| 2026-01-20 22:05 | START | opus | Task 2 (settings.json) | - |
| 2026-01-20 22:05 | END | opus | Task 2 | SUCCESS |
| 2026-01-20 22:06 | START | opus | AC Verification | - |
| 2026-01-20 22:08 | END | opus | AC Verification (all) | PASS (9/9) |

---

## Links

- [index-features.md](index-features.md)
- [feature-567.md](feature-567.md) - Successor: Hooks Cleanup
- [feature-540.md](feature-540.md) - Blocked by this feature
- [feature-582.md](feature-582.md) - Handoff: persist_pending Definition Guidance
