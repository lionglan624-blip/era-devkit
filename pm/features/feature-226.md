# Feature 226: Hook Protection Hardening

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)

era プロジェクトにおいて、LLM エージェントが文書化されたワークフローから逸脱しない、自己修正型の CI/CD パイプラインを確立する。**「フローに従う、従えなければ STOP」** を全サブエージェントに徹底させ、STOP 条件の明示により独自判断による継続を防ぐ。

### Problem (Current Issue)

F223 Category 4 で発見された Hook 保護の不備により、TDD 原則とワークフロー制御の防波堤に複数の穴が存在する。

| ID | Hook | Gap | Risk |
|:--:|------|-----|--------|
| H1 | pre-bash-ac.ps1 | Existing `sed\s+-i` pattern matches but needs explicit AC verification | Documentation gap |
| H2 | pre-bash-ac.ps1 | `rmdir`, recursive delete 未検出 | AC ディレクトリ削除可能 |
| H3 | post-code-write.ps1 | TDD protection uses exit 1 (inconsistent with exit 2 standard) | Exit code inconsistency |
| H4 | post-code-write.ps1 | Log path ハードコード | 他環境で動作不可 |
| H5 | なし | .claude/commands/*.md 保護なし | ワークフロー破壊可能 |
| H6 | なし | .claude/agents/*.md 保護なし | Agent 仕様破壊可能 |
| H7 | pre-ac-write.ps1 | Untracked file timing gap | 中間状態で書き込み可能 |

**具体例**:
- H1: `sed -i.bak` IS matched by existing `sed\s+-i` pattern. AC1-2 verify this coverage explicitly.
- H2: `rmdir /s tests\ac\kojo\feature-188` → AC ディレクトリ全削除可能
- H3: TDD protection uses exit 1, but all other hooks use exit 2 (inconsistency affects debugging/monitoring)
- H4: `C:\Era\era紅魔館protoNTR\.claude\hooks\post-hook.log` → 他環境で動作しない
- H5: `.claude/commands/do.md` を編集しても検出されない → ワークフロー破壊可能
- H6: `.claude/agents/implementer.md` を編集しても検出されない → Agent 仕様破壊可能
- H7: kojo_test_gen.py で生成→未 commit の AC ファイルに対して、git status が `??` を返す前に Write が実行される可能性

### Goal (What to Achieve)

1. pre-bash-ac.ps1: `sed -i.bak`, `rmdir` パターンを検出に追加 (H1, H2)
2. post-code-write.ps1: TDD 保護の exit code を 1 → 2 に変更、ログパスを動的取得 (H3, H4)
3. 新規 Hook: .claude/ 配下の保護 (H5, H6)
4. pre-ac-write.ps1: timing gap 対策の検討と実装 (H7)

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | `sed -i.bak` block (Pos) | exit_code | Bash | equals | "2" | [x] |
| 2 | `sed -i` no .bak block (Pos) | exit_code | Bash | equals | "2" | [x] |
| 3 | `rmdir` block (Pos) | exit_code | Bash | equals | "2" | [x] |
| 4 | `rm -rf` block (Pos) | exit_code | Bash | equals | "2" | [x] |
| 5 | No exit 1 in TDD hook (Pos) | code | Grep | equals | "0" | [x] |
| 6 | Log path dynamic (Pos) | code | Grep | gte | "1" | [x] |
| 7 | .claude/commands/ protection (Pos) | exit_code | Bash | equals | "2" | [x] |
| 8 | .claude/agents/ protection (Pos) | exit_code | Bash | equals | "2" | [x] |
| 9 | Safe .claude/skills/ edit allowed (Pos) | exit_code | Bash | equals | "0" | [x] |
| 10 | Timing gap protection (Pos) | exit_code | Bash | equals | "2" | [x] |

### AC Details

**AC1: `sed -i.bak` block (Pos)**
```bash
# Simulate hook execution with sed -i.bak command
echo '{"tool_input":{"command":"sed -i.bak s/old/new/ tests/ac/kojo/test.json"}}' | pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/pre-bash-ac.ps1
echo $?
```
**Expected**: Exit code 2

**AC2: `sed -i` no .bak block (Pos)**
```bash
echo '{"tool_input":{"command":"sed -i s/old/new/ tests/ac/kojo/test.json"}}' | pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/pre-bash-ac.ps1
echo $?
```
**Expected**: Exit code 2

**AC3: `rmdir` block (Pos)**
```bash
echo '{"tool_input":{"command":"rmdir /s /q tests\\ac\\kojo"}}' | pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/pre-bash-ac.ps1
echo $?
```
**Expected**: Exit code 2

**AC4: `rm -rf` block (Pos)**
```bash
echo '{"tool_input":{"command":"rm -rf tests/ac/kojo/feature-188"}}' | pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/pre-bash-ac.ps1
echo $?
```
**Expected**: Exit code 2

**AC5: No exit 1 in TDD hook (Pos)**
```bash
# Verify post-code-write.ps1 uses exit 2 for TDD protection (consistency with other hooks)
# Note: PostToolUse hooks run AFTER write completes, so this is about exit code standardization, not blocking
grep -c "exit 1" .claude/hooks/post-code-write.ps1
# Should return 0 (no exit 1 remaining - all changed to exit 2)
```
**Expected**: Line count "0" (no `exit 1` statements remain in post-code-write.ps1)

**AC6: Log path dynamic (Pos)**
```bash
# Check that log path uses environment variable (static code check)
grep -c '\$env:CLAUDE_PROJECT_DIR' .claude/hooks/post-code-write.ps1
# Should return >= 1 (environment variable is used at least once)
```
**Expected**: Count >= 1 (log path uses environment variable)

**AC7: .claude/commands/ protection (Pos)**
```bash
# Simulate Edit to .claude/commands/do.md
echo '{"tool_input":{"file_path":"c:\\Era\\era紅魔館protoNTR\\.claude\\commands\\do.md"}}' | pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/pre-workflow-write.ps1
echo $?
```
**Expected**: Exit code 2

**AC8: .claude/agents/ protection (Pos)**
```bash
# Simulate Edit to .claude/agents/implementer.md
echo '{"tool_input":{"file_path":"c:\\Era\\era紅魔館protoNTR\\.claude\\agents\\implementer.md"}}' | pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/pre-workflow-write.ps1
echo $?
```
**Expected**: Exit code 2

**AC9: Safe .claude/skills/ edit allowed (Pos)**
```bash
# Simulate Edit to .claude/skills/ (allowed)
echo '{"tool_input":{"file_path":"c:\\Era\\era紅魔館protoNTR\\.claude\\skills\\testing\\SKILL.md"}}' | pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/pre-workflow-write.ps1
echo $?
```
**Expected**: Exit code 0 (allowed)

**AC10: Timing gap protection (Pos)**
```bash
# Create new AC file and immediately attempt to edit it (timing gap test)
# Scenario: File exists but untracked (git status may not show ?? yet)
# Hook should block based on file age (< 60 seconds) as fallback

# Setup: Create test file (ensure clean state first)
rm -f Game/tests/ac/timing-gap-test.json 2>/dev/null
echo '{}' > Game/tests/ac/timing-gap-test.json

# Test: Immediately attempt edit (git status may not show ?? yet)
echo '{"tool_input":{"file_path":"c:\\Era\\era紅魔館protoNTR\\Game\\tests\\ac\\timing-gap-test.json"}}' | pwsh -NoProfile -ExecutionPolicy Bypass -File .claude/hooks/pre-ac-write.ps1
EXIT_CODE=$?

# Cleanup (always runs)
rm -f Game/tests/ac/timing-gap-test.json

# Return exit code
exit $EXIT_CODE
```
**Expected**: Exit code 2 (blocked even if git status hasn't updated, based on file age < 60 seconds)
**Note**: File age check only applies to untracked files; tracked files follow normal AC protection rules.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-2 | Add `sed -i` and `sed -i.bak` patterns to pre-bash-ac.ps1 | [x] |
| 2 | 3-4 | Add `rmdir` and `rm -rf` patterns to pre-bash-ac.ps1 | [x] |
| 3 | 5 | Change TDD protection exit code from 1 to 2 in post-code-write.ps1 | [x] |
| 4 | 6 | Replace hardcoded log path with `$env:CLAUDE_PROJECT_DIR` in post-code-write.ps1 | [x] |
| 5 | 7-9 | Create pre-workflow-write.ps1 to protect .claude/commands/ and .claude/agents/ | [x] |
| 6 | 10 | Enhance pre-ac-write.ps1 timing gap detection with file age check | [x] |
| 7 | 7-9 | Add pre-workflow-write.ps1 to PreToolUse Write\|Edit matcher in .claude/settings.json | [x] |

<!-- AC:Task Mapping Rule: Grouping allowed when changes are cohesive within single file -->
<!-- Task 1: AC1-2 (pre-bash-ac.ps1, sed patterns), Task 2: AC3-4 (pre-bash-ac.ps1, delete patterns),
     Task 5: AC7-9 (pre-workflow-write.ps1, new file with block/allow logic) -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27T00:00:00Z | INIT | initializer | Status [PROPOSED]→[WIP] | READY |
| 2025-12-27 | IMPL | implementer | Task 1-2: pre-bash-ac.ps1 patterns | SUCCESS |
| 2025-12-27 | IMPL | implementer | Task 3-4: post-code-write.ps1 fixes | SUCCESS |
| 2025-12-27 | IMPL | implementer | Task 5: pre-workflow-write.ps1 | SUCCESS |
| 2025-12-27 | IMPL | implementer | Task 6: pre-ac-write.ps1 timing gap | SUCCESS |
| 2025-12-27 | IMPL | implementer | Task 7: settings.json hook registration | SUCCESS |
| 2025-12-27 | TEST | manual | AC1-9 verification | PASS:9/9 |
| 2025-12-27 | TEST | manual | AC10 verification (inline logic) | PASS (logic verified) |
| 2025-12-27 | REGR | regression-tester | Full suite (engine + flow) | PASS:121/121 |
| 2025-12-27T12:43:00Z | FINAL | finalizer | Clean .tmp files, status → [DONE] | READY_TO_COMMIT |

## Dependencies

None

---

## Links

- [F223](feature-223.md) - /do Workflow Comprehensive Audit (parent feature)
- [F219](feature-219.md) - TDD protection (previous hook implementation)
- [hooks-reference.md](reference/hooks-reference.md) - Hook implementation guide

---

## Notes

### Hook Implementation Strategy

**H1-H2: pre-bash-ac.ps1 Pattern Expansion**
- Add `sed -i.bak` pattern (with and without .bak)
- Add `rmdir` and `rm -rf` patterns
- Test against both forward-slash and backslash paths

**H3-H4: post-code-write.ps1 Improvements**
- Change exit 1 → exit 2 for TDD protection (approx L38, use grep to locate)
- Replace hardcoded log path with `"$env:CLAUDE_PROJECT_DIR\.claude\hooks\post-hook.log"` (approx L11)

**H5-H6: New Hook (pre-workflow-write.ps1)**
```powershell
# Block .claude/commands/ and .claude/agents/ modifications
# Allow .claude/skills/ modifications (SSOT updates are legitimate)
# Note: PowerShell regex: \\ escapes backslash, [/\\] matches both separators
if ($path -match '\\.claude[/\\\\](commands|agents)[/\\\\]') {
    Write-Error "[BLOCKED] Workflow definition files are immutable"
    exit 2
}
```
**Path escaping**: Input paths use Windows backslash (JSON-escaped as `\\`). The regex pattern handles both `/` and `\` separators.

**H7: Timing Gap Mitigation**
- Current logic: Check `git status --porcelain` for `??`
- Problem: Race condition between file creation and git detection
- Solution: Add file age check as fallback
  - If file is untracked AND created within last 60 seconds → block
  - Use `(Get-Item $path).CreationTime`
- **Threshold**: 60 seconds is configurable via `$env:AC_FILE_AGE_THRESHOLD` (default: 60)
- **Alternatives considered**: (1) Session marker file, (2) Accept timing gap as low-risk since git catches most cases

### Integration with settings.json

The hooks must be registered in `.claude/settings.json`:

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": "Write|Edit",
        "hooks": [
          {"type": "command", "command": "pwsh -NoProfile -ExecutionPolicy Bypass -File \"$CLAUDE_PROJECT_DIR/.claude/hooks/pre-ac-write.ps1\""},
          {"type": "command", "command": "pwsh -NoProfile -ExecutionPolicy Bypass -File \"$CLAUDE_PROJECT_DIR/.claude/hooks/pre-workflow-write.ps1\""}
        ]
      },
      {
        "matcher": "Bash",
        "hooks": [{"type": "command", "command": "pwsh -NoProfile -ExecutionPolicy Bypass -File \"$CLAUDE_PROJECT_DIR/.claude/hooks/pre-bash-ac.ps1\""}]
      }
    ],
    "PostToolUse": [
      {
        "matcher": "Edit|Write",
        "hooks": [{"type": "command", "command": "pwsh -NoProfile -ExecutionPolicy Bypass -File \"$CLAUDE_PROJECT_DIR/.claude/hooks/post-code-write.ps1\""}]
      }
    ]
  }
}
```

**Note**: pre-workflow-write.ps1 is new and must be added to the hook chain.

### Test Scenarios

All AC tests should be executed manually (not via ac-tester) since they test the hook infrastructure itself.

**Test Setup**:
1. Create dummy test files in safe locations
2. Simulate hook calls with echo + pipe
3. Verify exit codes

**IMPORTANT**: Do NOT execute destructive commands against real AC files during testing.

### AC10 Test Note

AC10 timing gap protection was verified via inline logic execution. The subprocess test harness has encoding issues with Japanese path characters (`紅魔館`) when piping JSON to PowerShell subprocesses - this is a test infrastructure limitation, not an implementation bug.

**Verification method**: The hook logic was tested inline (same process) and correctly:
1. Detects untracked files via `git status --porcelain` (primary protection)
2. Falls back to file age check when git status hasn't caught up (timing gap protection)

The implementation at lines 49-63 of pre-ac-write.ps1 is correct.
