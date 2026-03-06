# Feature 567: Claude Code Hooks Cleanup

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

Hooks must provide value without blocking legitimate operations.
Dead hooks create noise ("hook error" messages) and false confidence. However, hooks protecting active directories serve valid purposes and should be preserved or updated, not eliminated.

TDD protection is consolidated under single authority (F568) per SSOT principle.

### Problem (Current Issue)

Current `.claude/hooks/` contains obsolete and harmful hooks:

| Hook | Problem |
|------|---------|
| `pre-ac-write.ps1` | Protects `tests/ac/` (ACTIVE, valid), `tests/regression/` (non-existent, dead code) - removing noise is cleanup, not critical |
| `pre-bash-ac.ps1` | Protects `tests/ac/` (ACTIVE, valid), `tests/regression/` (non-existent, dead code) - removing noise is cleanup, not critical |
| `pre-workflow-write.ps1` | Blocks `.claude/commands/`, `.claude/agents/` edits - originally intended to protect workflow definitions during execution, but too restrictive (blocks documentation updates and bug fixes) |
| `post-code-write.ps1` | Targets `engine/` C# files, doesn't target `Era.Core/` C# files; ERB BOM check still useful but scope wrong |

Additional issues:
- "PreToolUse:Edit hook error" messages appear on every edit (noise)
- BOM check targets ERB but YAML (new standard) doesn't need BOM

### Goal (What to Achieve)

1. Remove hooks protecting unused directories
2. Remove hook blocking legitimate workflow edits
3. Update post-write hook for current architecture (Era.Core/, no ERB focus)
4. Eliminate "hook error" noise

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | pre-ac-write.ps1 modified | file | Grep | not_contains | "tests/regression" | [x] |
| 2 | pre-bash-ac.ps1 keeps tests/ac protection | file | Grep | contains | "tests/ac" | [x] |
| 3 | pre-workflow-write.ps1 removed | file | Glob | not_exists | `.claude/hooks/pre-workflow-write.ps1` | [x] |
| 4 | settings.json PreToolUse hooks removed | file | Grep | not_contains | "pre-workflow-write" | [x] |
| 5 | post-code-write.ps1 updated for Era.Core | file | Grep | contains | "Era\.Core[/\\].*\.cs$" | [x] |
| 6 | post-code-write.ps1 removes ERB BOM logic | file | Grep | not_contains | "BOM added" | [x] |
| 7 | .claude/agents directory accessible | file | Glob | exists | `.claude/agents/*.md` | [x] |
| 8 | pre-ac-write.ps1 removes tests/ac/ protection | file | Grep | not_contains | "tests/ac" | [x] |
| 9 | pre-bash-ac.ps1 removes tests/regression/ only | file | Grep | not_contains | "regression" | [x] |
| 10 | post-code-write.ps1 removes TDD protection block | file | Grep | not_contains | "TDD Protection" | [x] |

### AC Details

**AC#1**: Remove tests/regression from pre-ac-write.ps1
- pre-ac-write.ps1 removes tests/regression pattern (dead code - directory doesn't exist)
- Combined with AC#8: pre-ac-write.ps1 will have ZERO test directory protection after F567 (F568 covers Write|Edit for both paths)

**AC#2**: Verify pre-bash-ac.ps1 KEEPS tests/ac protection (post-modification)
- Verified AFTER Task#9 modifies pre-bash-ac.ps1 to remove regression patterns
- pre-bash-ac.ps1 must still contain tests/ac pattern after modification
- Rationale: F568 only covers Write|Edit, not Bash commands; Bash TDD protection must remain

**AC#3**: Remove workflow-blocking hook
- Delete file: `pre-workflow-write.ps1`

**AC#4**: Update settings.json
- Remove PreToolUse entry for pre-workflow-write.ps1 (deleted)
- Keep PreToolUse entry for pre-ac-write.ps1 (modified)
- Keep PostToolUse for post-code-write.ps1

**AC#5-6**: Update post-code-write.ps1
- AC#5: Line 43 - Replace `$isCS = $path -match 'engine[/\\].*\.cs$'` with `$isCS = $path -match 'Era\.Core[/\\].*\.cs$'`
  - Note: `.*` allows recursive match for all C# files under Era.Core/ (correct for build+test trigger)
- AC#6: Lines 60-74 - Remove entire ERB BOM logic block (YAML doesn't need BOM, cleans up technical debt)
- Keep dotnet build + dotnet test for C# files

**AC#7**: Workflow directory accessible
- Verify .claude/agents/ directory has markdown files (implicitly confirms edit operations possible after hook removal)

**AC#8**: Remove tests/ac protection from pre-ac-write.ps1
- pre-ac-write.ps1 tests/ac protection removed (F568 pre-tdd-protection.ps1 covers Write|Edit)

**AC#9**: Remove tests/regression only from pre-bash-ac.ps1
- KEEP tests/ac protection (F568 doesn't cover Bash commands - protection gap prevention)
- Remove only tests/regression pattern (dead code - directory doesn't exist)
- Implementation note: Requires regex surgery on 6 patterns (lines 23, 30, 37, 44, 51, 58)
  - Change `tests[/\\](ac|regression)[/\\]` to `tests[/\\]ac[/\\]` in each pattern

**AC#10**: Remove TDD protection block from PostToolUse hook
- Remove lines 35-39 (entire TDD protection block including tests/ac and tests/regression)
- Rationale: F568 pre-tdd-protection.ps1 provides unified TDD protection; post-code-write.ps1 TDD blocking is redundant and violates SSOT
- Note: tests/regression/ protection also removed from PostToolUse (dead path, no impact)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Remove tests/regression reference from pre-ac-write.ps1 | [x] |
| 2 | 2 | Verify pre-bash-ac.ps1 keeps tests/ac protection | [x] |
| 3 | 3 | Delete pre-workflow-write.ps1 | [x] |
| 4 | 4 | Update settings.json to remove pre-workflow-write reference | [x] |
| 5 | 5 | Update post-code-write.ps1 path pattern to Era.Core | [x] |
| 6 | 6 | Remove ERB BOM logic from post-code-write.ps1 | [x] |
| 7 | 7 | Verify .claude/agents directory accessible | [x] |
| 8 | 8 | Remove tests/ac protection from pre-ac-write.ps1 | [x] |
| 9 | 9 | Modify pre-bash-ac.ps1: split OR groups to remove regression, keep ac only | [x] |
| 10 | 10 | Remove TDD protection block (lines 35-39) from post-code-write.ps1 | [x] |

---

## Implementation Contract

### Impact Analysis

| File/Component | Change | Impact |
|----------------|--------|--------|
| `.claude/hooks/pre-workflow-write.ps1` | Delete 1 file (Task#3) | Workflow files (.claude/agents/, .claude/commands/) now editable |
| `.claude/hooks/pre-ac-write.ps1` | Remove tests/regression and tests/ac patterns (Task#1, Task#8) | Cleanup dead path protection (F568 consolidates TDD) |
| `.claude/hooks/pre-bash-ac.ps1` | Remove tests/regression only (Task#9), verify tests/ac kept (Task#2) | Remove dead code; keep Bash TDD protection (F568 only covers Write|Edit) |
| `.claude/settings.json` | Remove pre-workflow-write.ps1 line (Task#4) | Cleaner hook config |
| `.claude/hooks/post-code-write.ps1` | Update path to Era.Core, remove ERB BOM, remove TDD block (Task#5, Task#6, Task#10) | C# edits trigger build+test, TDD protection delegated to F568 |

### Rollback Plan

If issues arise after deployment:
1. `git revert` to restore hooks
2. Re-evaluate which hooks are actually needed
3. Create follow-up feature for refined approach

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F568 | [DONE] | TDD AC protection for Era.Core.Tests/ + tests/ac/ (must be DONE before F567 to avoid protection gap) |
| Related | F566 | [DONE] | Pre-commit CI Modernization (parallel cleanup effort) |

---

## Review Notes

### F568 Handoff (2026-01-20)

F568のFL reviewで、TDD保護の責任境界について決定:
- **決定**: F568がTDD保護を一元化（Era.Core.Tests/ + tests/ac/）
- **F567への影響**: pre-ac-write.ps1からtests/ac/保護を除去（当初は保持予定だった）
- **理由**: SSOT原則に基づき、TDD保護は単一の権威（F568 hook）で管理

AC#9, Task#2, Task#5, Impact Analysisを更新済。

### FL Review Phase 7 (resolved)

- [applied] Phase7 iter10: F578 created as stub (user decision: create now)

---

## Mandatory Handoffs

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| TDD AC protection for Era.Core.Tests/ | Current hooks protect unused paths; new protection needed for C# test files | Feature | F568 |
| Bash TDD protection consolidation | pre-bash-ac.ps1 still protects tests/ac/ for Bash commands; F568 only covers Write|Edit. Future consolidation needed. | Feature (create after F567) | F578 |

**Context**: Original hooks prevented TDD violations (editing test criteria during flow execution). The principle is sound, but target directory changed from `tests/ac/` to `Era.Core.Tests/`. F568 must be [DONE] before F567 execution begins to avoid protection gap - this is a hard prerequisite for transition safety.

**Bash TDD Gap**: F568's pre-tdd-protection.ps1 registers for Write|Edit only. pre-bash-ac.ps1 provides Bash command protection for tests/ac/. F567 preserves this protection to avoid gap. Future work should consolidate Bash TDD protection under F568 or dedicated hook.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-21 08:48 | START | implementer | Task 3 | - |
| 2026-01-21 08:48 | END | implementer | Task 3 | SUCCESS |
| 2026-01-21 08:48 | START | implementer | Task 4 | - |
| 2026-01-21 08:48 | END | implementer | Task 4 | SUCCESS |
| 2026-01-21 08:48 | START | implementer | Task 9 | - |
| 2026-01-21 08:48 | END | implementer | Task 9 | SUCCESS |
| 2026-01-21 08:48 | START | implementer | Task 2 | - |
| 2026-01-21 08:48 | END | implementer | Task 2 | SUCCESS |
| 2026-01-21 08:48 | START | implementer | Task 1 | - |
| 2026-01-21 08:48 | END | implementer | Task 1 | SUCCESS |
| 2026-01-21 08:50 | DEVIATION | Verification | AC#9 | pre-bash-ac.ps1 still contains "regression" in error messages |
| 2026-01-21 08:51 | FIX | Opus | AC#9 | Removed "regression" from comments and error messages |
| 2026-01-21 08:49 | START | implementer | Task 8 | - |
| 2026-01-21 08:49 | END | implementer | Task 8 | SUCCESS |
| 2026-01-21 08:49 | START | implementer | Task 5 | - |
| 2026-01-21 08:49 | END | implementer | Task 5 | SUCCESS |
| 2026-01-21 08:49 | START | implementer | Task 10 | - |
| 2026-01-21 08:49 | END | implementer | Task 10 | SUCCESS |
| 2026-01-21 08:49 | START | implementer | Task 6 | - |
| 2026-01-21 08:49 | END | implementer | Task 6 | SUCCESS |
| 2026-01-21 08:49 | START | implementer | Task 7 | - |
| 2026-01-21 08:50 | END | implementer | Task 7 | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- [feature-566.md](feature-566.md) - Related: Pre-commit CI Modernization
- [feature-568.md](feature-568.md) - Predecessor: TDD protection consolidation (Era.Core.Tests/ + tests/ac/)
