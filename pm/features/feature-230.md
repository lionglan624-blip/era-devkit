# Feature 230: Recovery Procedures

## Status: [DONE]

## Type: infra

## Keywords (検索用)

`recovery`, `crash`, `timeout`, `rollback`, `failure`, `/do`, `kojo-writer`, `AC`, `hook`, `Phase`

---

## Background

### Philosophy (Mid-term Vision)

era プロジェクトにおいて、LLM エージェントワークフローが予期しなぁE��断めE��敗から復旧できる明確な手頁E��確立する、E*「失敗�E起きる前提で設計し、復旧手頁E��斁E��化、E* により、E��発中断を最小化し、継続可能性を向上させる、E

### Problem (Current Issue)

F223 Issue Inventory Category 6 で発見された通り、Edo ワークフローにおいて以下�E復旧シナリオが未斁E��匁E

| ID | Scenario | Current Status |
|:--:|----------|------|
| R1 | Phase 1 途中でクラチE��ュ | 再開方法不�E |
| R2 | kojo-writer ぁE6時間経過 | キャンセル方法不�E |
| R3 | AC Test 3回失敁E(異なる理由) | カウント方法不�E |
| R4 | User approval スキチE�E | ロールバック方法不�E |
| R5 | Hook サイレント失敁E| 検�E方法なぁE|

また、W7 (Error recovery 手頁E��ぁEↁE途中再開不可) により、ワークフロー全体で recovery procedures が欠落してぁE��、E

現状では、失敗発生時にユーザーが手動で状態を確認し、試行錯誤で復旧を試みるしかなぁE��これにより開発効玁E��低下し、データ不整合�Eリスクが増大する、E

### Goal (What to Achieve)

/do ワークフロー実行中に発生しぁE�� 5 種類�E異常シナリオにつぁE��、以下を斁E��匁E

1. **検�E方況E*: 異常状態をどぁE��定するか
2. **復旧手頁E*: どのファイル/状態を修正し、どのコマンドを実行するか
3. **防止筁E*: 異常発生を最小化する設計改喁E��

これにより、E��発老E��異常発生時に迷わず対処でき、ワークフローの中断時間を最小化する、E

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | R1 header | file | Grep | contains | "R1: Phase Crash Recovery" | [x] |
| 2 | R2 header | file | Grep | contains | "R2: kojo-writer Timeout" | [x] |
| 3 | R3 header | file | Grep | contains | "R3: AC Test Failure Counter" | [x] |
| 4 | R4 header | file | Grep | contains | "R4: User Approval Rollback" | [x] |
| 5 | R5 header | file | Grep | contains | "R5: Hook Silent Failure" | [x] |
| 6 | Has Scenario markers | file | Grep | contains | "**Scenario**:" | [x] |
| 7 | Has Detection markers | file | Grep | contains | "**Detection**:" | [x] |
| 8 | Has Recovery markers | file | Grep | contains | "**Recovery**:" | [x] |
| 9 | Has Prevention markers | file | Grep | contains | "**Prevention**:" | [x] |

### AC Details

**Target**: `.claude/commands/do.md`

**AC1 Test**:
```
Grep("R1: Phase Crash Recovery", path: ".claude/commands/do.md", output_mode: "content")
```
**Expected**: Match found

**AC2 Test**:
```
Grep("R2: kojo-writer Timeout", path: ".claude/commands/do.md", output_mode: "content")
```
**Expected**: Match found

**AC3 Test**:
```
Grep("R3: AC Test Failure Counter", path: ".claude/commands/do.md", output_mode: "content")
```
**Expected**: Match found

**AC4 Test**:
```
Grep("R4: User Approval Rollback", path: ".claude/commands/do.md", output_mode: "content")
```
**Expected**: Match found

**AC5 Test**:
```
Grep("R5: Hook Silent Failure", path: ".claude/commands/do.md", output_mode: "content")
```
**Expected**: Match found

**Scope**: Each recovery section should include:
- Scenario description (what went wrong)
- Detection method (how to identify the issue)
- Recovery steps (concrete commands/actions)
- Prevention strategy (how to avoid in future)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,6-9 | Document R1: Phase crash recovery with all 4 subsections (Scenario/Detection/Recovery/Prevention) | [x] |
| 2 | 2,6-9 | Document R2: kojo-writer timeout with all 4 subsections | [x] |
| 3 | 3,6-9 | Document R3: AC Test 3-failure recovery with all 4 subsections (Note: references F227 Failure Counter) | [x] |
| 4 | 4,6-9 | Document R4: User approval rollback with all 4 subsections | [x] |
| 5 | 5,6-9 | Document R5: Hook silent failure with all 4 subsections | [x] |

<!-- Note: AC6-9 verify subsection markers exist across all R sections. Each Task implements one R section with all required subsections. -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Dependencies

- **F227**: /do Workflow Robustness (**SATISFIED** ✁E - Phase structure and Failure Counter already implemented in do.md

---

## Implementation Notes

### Target Document

Primary: `.claude/commands/do.md` - Add "Recovery Procedures" section

**Hook Bypass Required**: do.md is protected by F226 `pre-workflow-write.ps1` hook. Implementer must request user approval for temporary hook bypass during implementation.

### Section Structure (Proposed)

```markdown
## Recovery Procedures

### R1: Phase Crash Recovery

**Scenario**: Opus crashes during Phase execution
**Detection**: Task tool shows incomplete execution, feature status unchanged
**Recovery**:
1. Check `pm/features/feature-{ID}.md` Execution Log for last completed Phase
2. Check TodoWrite status for current Phase progress
3. Re-run `/do {ID}` - initializer agent will determine resume point based on feature status (returns: NO_FEATURE, Background empty, All done, or READY)
**Prevention**: Idempotent Phase design (F227 scope)

### R2: kojo-writer Timeout

**Scenario**: kojo-writer exceeds 6 hours without completion
**Detection**: `Glob("pm/status/{ID}_K*.txt")` count < 10 after timeout
**Recovery**:
1. Cancel long-running Task (if accessible)
2. Check `Game/ERB/口丁E` for partial kojo files
3. Verify which K# completed, which failed
4. Manual intervention: complete remaining kojo OR re-run Phase 4
**Prevention**: Per-writer timeout (F227 W3)

### R3: AC Test Failure Counter

**Scenario**: AC tests fail 3 times with different reasons
**Detection**: Execution Log in feature-{ID}.md shows 3 FAIL entries for same type within Phase 6 (tracked by /do workflow per do.md Failure Counter section)
**Recovery**:
1. Review test logs: `.tmp/ac/{ID}/`
2. Identify root cause (test issue vs implementation issue)
3. If test issue ↁEescalate to user (STOP)
4. If implementation issue ↁEre-run debug loop
**Prevention**: Counter Scope: Per Phase 6 entry (reset on PASS, user instruction, or Phase transition) - per do.md Failure Counter section

### R4: User Approval Rollback

**Scenario**: User realizes approval was mistaken after Phase 10
**Detection**: User inspection of committed changes
**Recovery**:
1. `git log -1` - verify commit hash
2. `git reset --soft HEAD~1` - undo commit, keep changes
3. Edit `pm/features/feature-{ID}.md` - update Tasks/ACs
4. Re-run verification (Phase 6-9)
5. Re-run approval (Phase 10)
**Prevention**: Better approval prompt (F231), Post-Review enforcement (F227 W12-W14)

### R5: Hook Silent Failure

**Scenario**: pre-commit hook fails silently, allowing invalid commits
**Detection**: None (silent failure by definition)
**Recovery**:
1. Manual verification: `git log -1 --stat` - check if AC/test files modified
2. Check hook logs: `.tmp/hook-*.log` (if logging implemented)
3. If violation detected: `git reset --soft HEAD~1`, fix violation, re-commit
**Prevention**: Manual verification recommended, exit code verification (F229 I1)
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | Feature Completion | finalizer | Mark all ACs [x], Tasks [O], Status [DONE] | SUCCESS |

---

## Links

- [F223](feature-223.md) - /do Workflow Comprehensive Audit (Issue Inventory Category 6)
- [F227](feature-227.md) - /do Workflow Robustness (dependency)
- [F226](feature-226.md) - Hook Protection Hardening (related)
- [F229](feature-229.md) - Infrastructure Verification (related)
- [F231](feature-231.md) - User Approval UX (related)

---

## Notes

- This feature documents recovery procedures only. Implementation of automated recovery (e.g., failure counters, idempotent Phases) is out of scope and belongs to F227.
- R3 failure counter mechanism is documented in F227 (do.md "Failure Counter" section). F230 Task 3 adds recovery-specific documentation (detection, recovery steps, prevention) that the Failure Counter section lacks.
- Hook logging (R5 prevention) is F226 scope (dynamic log path added).
- Post-implementation, test each recovery scenario manually to verify documentation accuracy.

---

## Priority

**P2** (Nice-to-have)

**Rationale**: Recovery procedures improve developer experience and reduce debugging time, but are not blocking for basic workflow execution. Should be implemented after core workflow robustness (F227) is complete.
