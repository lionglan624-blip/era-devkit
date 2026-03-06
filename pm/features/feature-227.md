# Feature 227: /do Workflow Robustness

## Status: [DONE]

## Type: infra

## Keywords (検索用)

`/do`, `workflow`, `phase`, `timeout`, `polling`, `phase-redesign`, `post-review`, `regression-order`, `stop-conditions`, `failure-counter`

---

## Background

### Philosophy (Mid-term Vision)

era プロジェクトにおいて、LLM エージェントが文書化されたワークフローから逸脱しない、自己修正型の CI/CD パイプラインを確立する。**「フローに従う、従えなければ STOP」** を全サブエージェントに徹底させ、STOP 条件の明示により独自判断による継続を防ぐ。

F223 で洗い出された問題のうち、本 Feature は `/do` ワークフロー自体の堅牢性を担保する。

### Problem (Current Issue)

F223 Category 2 (Workflow Ambiguity) で発見された 14 の問題:

| ID | Phase | Problem | Risk |
|:--:|:-----:|---------|------|
| W1 | 4 (kojo) | Polling timeout 上限なし | 無限待機 |
| W2 | 4 (kojo) | Status file path 不整合 | Polling 常時失敗 |
| W3 | 4 (kojo) | Per-writer timeout なし | 1人ハングで全体停止 |
| W6 | 全体 | 3-failure counter 未実装 | STOP 条件無効 |
| W8 | 7-9 | Phase 7-9 の役割が do.md で不明確 | CI委任 vs 手動実行の切り分け必要 |
| W9 | pre-commit | 最終防波堤として AC+Regression+verify 必要 | LLM スキップ対策 |
| W10 | 10 | Phase 10 順序不明確 | Regression→verify→Post-Review→Report順序明記必要 |
| W11 | 7-10 | AC→Regression 順序 | 一般原則はRegression→AC(既存保護優先)。現設計は逆 |
| W12 | 10 | Post-Review 必須ゲート未明記 | READYを返さないとUser Approvalに進めないことが不明確 |
| W13 | 10 | Post-Review 異常時対応なし | UNREACHABLE/IRRELEVANT時の対応未定義 |
| W14 | 10 | Post-Review 判断絶対視未記載 | NEEDS_REVISION無視して進める余地あり |
| W15 | 全体 | Phase番号体系の不整合 | Step小数点が一部のみ、Phase7-9がCI委任で/do内での役割不明確 |

現状の do.md は Phase 1-10 構成だが、Phase 7-9 が CI に委任されており、Phase 10 は Post-Review → Report → Approval → Finalize の 4 ステップを "10.0", "10.1" で表現しており不明瞭。

### Goal (What to Achieve)

1. **Phase 体系の再設計 (W15)**: 小数点廃止、Phase 7-9 統合、Phase 10 を 3 つに分離 → 明確な 9 Phase 体系へ
2. **Timeout & Polling 修正 (W1-W3)**: kojo-writer の polling に上限設定、status file path 統一、per-writer timeout 追加
3. **Failure Counter 実装 (W6)**: 3 連続失敗で STOP する仕組みを明文化
4. **Post-Review ゲート強化 (W12-W14)**: READY 必須、NEEDS_REVISION/UNREACHABLE/IRRELEVANT 時の対応を明記
5. **Regression 順序修正 (W11)**: AC より Regression を先に実行（既存保護優先）
6. **pre-commit Hook 最終防波堤化 (W9)**: verify-logs.py による AC+Regression 検証を明記

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 0 | feature-reviewer.md output enum extended | file | Grep | contains | "READY \| NEEDS_REVISION \| UNREACHABLE \| IRRELEVANT" | [x] |
| 1 | Phase redesign documented | file | Grep | contains | "Phase 1: Initialize" | [x] |
| 2 | Phase redesign documented | file | Grep | contains | "Phase 6: Verification (Regression→AC→Debug)" | [x] |
| 3 | Phase redesign documented | file | Grep | contains | "Phase 7: Post-Review" | [x] |
| 4 | Phase redesign documented | file | Grep | contains | "Phase 8: Report & Approval" | [x] |
| 5 | Phase redesign documented | file | Grep | contains | "Phase 9: Finalize & Commit" | [x] |
| 6 | Polling timeout documented | file | Grep | contains | "max_polling_cycles" | [x] |
| 7 | Status file path documented | file | Grep | contains | "Game/agents/status/{ID}_K{N}.txt" | [x] |
| 8 | Per-writer timeout documented | file | Grep | contains | "per_writer_timeout" | [x] |
| 9 | 3-failure counter documented | file | Grep | contains | "3 consecutive failures" | [x] |
| 10 | Post-Review READY gate documented | file | Grep | contains | "READY required to proceed" | [x] |
| 11 | NEEDS_REVISION handling documented | file | Grep | contains | "NEEDS_REVISION" | [x] |
| 12 | UNREACHABLE handling documented | file | Grep | contains | "UNREACHABLE" | [x] |
| 13 | Regression→AC order documented | file | Grep | contains | "Regression (before AC)" | [x] |
| 14 | verify-logs.py documented | file | Grep | contains | "verify-logs.py" | [x] |
| 15 | Phase 7-9 removed from do.md | file | Grep | not_contains | "Phase 7: AC Verify" | [x] |

### AC Details

**Test**: `Grep("READY | NEEDS_REVISION | UNREACHABLE | IRRELEVANT", path: ".claude/agents/feature-reviewer.md", output_mode: "content")`
**Expected**: Match found (output enum includes all 4 statuses)

**Test**: `Grep("Phase 1: Initialize", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Match found

**Test**: `Grep("Phase 6: Verification", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Match found with description "(Regression→AC→Debug)"

**Test**: `Grep("Phase 7: Post-Review", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Match found

**Test**: `Grep("Phase 8: Report & Approval", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Match found

**Test**: `Grep("Phase 9: Finalize & Commit", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Match found

**Test**: `Grep("max_polling_cycles", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Polling timeout limit documented

**Test**: `Grep("Game/agents/status", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Correct status file path documented

**Test**: `Grep("per_writer_timeout", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Individual writer timeout documented

**Test**: `Grep("3 consecutive failures", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Failure counter behavior documented

**Test**: `Grep("READY required", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Post-Review gate requirement documented

**Test**: `Grep("NEEDS_REVISION", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Handling of NEEDS_REVISION status documented

**Test**: `Grep("UNREACHABLE", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Handling of UNREACHABLE status documented

**Test**: `Grep("Regression.*before AC", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: Regression-first order documented

**Test**: `Grep("verify-logs\\.py", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: verify-logs.py integration documented

**Test**: `Grep("Phase 7: AC Verify", path: ".claude/commands/do.md", output_mode: "content")`
**Expected**: No match (old Phase 7 removed)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | 0 | Extend feature-reviewer.md output enum with UNREACHABLE/IRRELEVANT statuses | [x] |
| 1 | 1-5 | Redesign do.md Phase structure to 9-phase system (eliminate Phase 7-9, split Phase 10) | [x] |
| 2 | 6-8 | Add kojo-writer polling timeout, status path, per-writer timeout to do.md Phase 4 | [x] |
| 3 | 9 | Document 3-failure counter in do.md Error Handling section | [x] |
| 4 | 10-12 | Add Post-Review gate enforcement and abnormal status handling to do.md Phase 7 | [x] |
| 5 | 13 | Reorder Phase 6 to Regression→AC (update do.md Phase 6 description) | [x] |
| 6 | 14 | Document verify-logs.py as final gate in do.md Phase 6 and pre-commit section | [x] |
| 7 | 15 | Remove old Phase 7-9 content from do.md (Phase sections + TodoWrite template references) | [x] |

**AC:Task Grouping Rationale** (deviations from 1:1 rule):
- **Task 1 (AC 1-5)**: Phase redesign is an atomic change; splitting would create inconsistent intermediate states
- **Task 2 (AC 6-8)**: Polling parameters are interdependent; max_polling_cycles, status file path, and per_writer_timeout all configure the same polling loop
- **Task 4 (AC 10-12)**: Post-review statuses (READY, NEEDS_REVISION, UNREACHABLE, IRRELEVANT) must be defined together for gate logic
- **Task 7 (AC 15)**: Cleanup verification as negative AC

---

## Design Details

### Proposed Phase Structure (9 Phases)

Based on F223 W15 Phase Redesign Draft:

| New | Name | Description | 旧 Phase |
|:---:|------|-------------|:--------:|
| 1 | Initialize | initializer dispatch, feature validation, IMPLE_FEATURE_ID creation | 1 |
| 2 | Investigation | explorer dispatch, dependency/constraint check | 2 |
| 3 | Test Creation (TDD) | erb/engine: JSON test creation, RED confirmation | 3 |
| 4 | Implementation | Type routing: kojo (batch), erb/engine (implementer) | 4 |
| 5 | Test Generation | kojo only: kojo_test_gen.py execution | 5 |
| 6 | Verification (Regression→AC→Debug) | Hooks (BOM/Build/Warnings) + Regression + AC + Debug loop (max 3) | 6-9 統合 |
| 7 | Post-Review | feature-reviewer (mode: post) - READY gate enforcement | 10.0 |
| 8 | Report & Approval | Japanese report + verify-logs.py + explicit user approval | 10.1前半 |
| 9 | Finalize & Commit | finalizer dispatch + commit execution | 10.1後半 |

**Key Changes**:
- **Phase 6**: Unified verification phase combining Hooks (6) + Regression (8) + AC (7) + Consistency (9)
- **Phase 7**: Dedicated Post-Review gate (READY required to proceed)
- **Phase 8**: Report & Approval separated from finalization
- **Phase 9**: Finalize & Commit as final phase

**Rationale**:
- Phase 7-9 in old structure were delegated to CI (pre-commit hook), causing confusion
- Phase 10 had too many responsibilities (review, report, approval, finalize)
- New structure makes gates explicit and phases atomic

### Polling Timeout Specification (W1-W3)

Current behavior (do.md L269-283):
```
sleep 360 → Glob → (不足なら sleep 60 繰返し)
```

**Problems**:
- No upper limit on polling cycles (W1)
- Status file path inconsistency between description and usage (W2)
- No per-writer timeout (W3)

**Proposed Fix**:

```markdown
**Polling Parameters**:
- Initial delay: 360 seconds (6 minutes)
- Poll interval: 60 seconds
- Max polling cycles: 60 (total timeout: 1 hour after initial delay)
- Per-writer timeout: 30 minutes (if no file created after this, consider stuck)
- Status file path: `Game/agents/status/{ID}_K{N}.txt` (absolute path)

**Polling Logic**:
1. sleep 360
2. Glob("Game/agents/status/{ID}_K*.txt")
3. If count < 10:
   - Check elapsed time since dispatch
   - If > max_polling_cycles * 60: **STOP** → Report timeout
   - For each missing K{N}, check if > per_writer_timeout: **STOP** → Report stuck writer
   - Else: sleep 60, goto 2
4. If count == 10: Proceed to Phase 5
```

**Status File Path Standard**: `Game/agents/status/{ID}_K{N}.txt` (use absolute path in Glob)

### 3-Failure Counter Implementation (W6)

Current do.md Error Handling mentions "3 failures" but no implementation detail.

**Proposed Addition to do.md**:

```markdown
## Failure Counter

Track consecutive failures across debug loops in Phase 6.

**Counter Scope**: Per Phase 6 entry (reset when Phase 6 starts)

**Increment Conditions**:
- AC test FAIL after debug fix
- Regression test FAIL after debug fix
- Build FAIL after debug fix

**STOP Condition**: 3 consecutive failures of same type

**Action on STOP**:
1. Log: `| {date} | STOP | - | 3 consecutive {type} failures | BLOCKED |`
2. Report to user: "3回連続で{type}失敗しました。別アプローチを試しますか？"
3. Wait for user instruction

**Reset Conditions**:
- Any test PASS
- User provides new instruction
- Phase transition (6→7)
```

### Post-Review Gate Enforcement (W12-W14)

Current do.md Phase 10.0 dispatches feature-reviewer but doesn't enforce gate.

**Proposed Phase 7 (Post-Review) Section**:

```markdown
## Phase 7: Post-Review

**Dispatch feature-reviewer (Mode: post).**

```
Task(subagent_type: "general-purpose", model: "opus", prompt: `
Read .claude/agents/feature-reviewer.md. Mode: post. Feature {ID}.
`)
```

| Result | Action |
|--------|--------|
| READY | **Gate PASS** → Proceed to Phase 8 |
| NEEDS_REVISION | **Gate BLOCK** → Fix issues → re-run Phase 7 |
| UNREACHABLE | **STOP** → Report to user: "Feature {ID} のゴールが達成不可能です。スコープ再定義が必要です。" |
| IRRELEVANT | **STOP** → Report to user: "Feature {ID} の実装内容がゴールと無関係です。Tasks を見直してください。" |

**CRITICAL**:
- **READY required to proceed** - Do NOT bypass this gate
- **NEEDS_REVISION is blocking** - Must fix and re-review
- **UNREACHABLE/IRRELEVANT are fatal** - Require user intervention, cannot auto-fix

**Loop Limit**: Max 3 NEEDS_REVISION cycles. On 4th, **STOP** → Report to user.
```

### Regression→AC Order (W11)

Current do.md has Phase 7 (AC) before Phase 8 (Regression).

**Rationale for Regression-First**:
- Regression tests protect existing functionality
- AC tests verify new functionality
- If regression fails, new functionality is irrelevant (broke existing features)
- Industry best practice: run broader tests before narrower tests

**Proposed Change**: Phase 6 section should document:

```markdown
## Phase 6: Verification (Regression→AC→Debug)

**Execution Order**:
1. **Hooks** (Smoke Test): BOM, Build, Strict Warnings - automatic via PostToolUse
2. **Regression** (before AC): Protect existing functionality
3. **AC**: Verify new functionality
4. **Debug Loop** (if needed): Max 3 iterations

**CI Delegation** (W8 resolution):
- Hooks, Regression, AC tests are executed by pre-commit hook (CI layer)
- /do workflow orchestrates but does NOT manually re-run verified tests
- Debug loop is /do responsibility when CI reports failures

**Rationale**: Regression-first ensures we don't break existing features while adding new ones.
```

### verify-logs.py as Final Gate (W9)

Current do.md mentions verify-logs.py in Phase 10 Report but doesn't emphasize its role as final mechanical gate.

**Proposed Addition**:

In Phase 8 (Report & Approval):

```markdown
### Step 8.1: Verify All Tests (Mechanical Gate)

Before reporting to user, verify that all tests passed using verify-logs.py:

```bash
python tools/verify-logs.py --scope feature:{ID}
```

**Expected Output**:
```
AC:         OK:{N}/{N}
Regression: OK:{M}/{M}
Engine:     OK:{P}/{P}  (if Type=engine)
Result:     OK:{total}/{total}
```

**If any FAIL**:
- **STOP** → This should never happen (pre-commit hook should have blocked)
- Report to user: "verify-logs.py で失敗検出。pre-commit hook が機能していない可能性があります。"
- Do NOT proceed to Step 8.2

**CRITICAL**: This is the **final mechanical gate** before user approval. LLM 主観判断を排除し、機械的検証のみを信頼する。
```

In pre-commit hook section (if not already documented):

```markdown
### Pre-Commit Hook Integration

The pre-commit hook (`.githooks/pre-commit`) runs:
1. All AC tests for feature scope (`IMPLE_FEATURE_ID`)
2. All Regression tests
3. `verify-logs.py` to verify results

**This is the final defense** against:
- LLM skipping test verification
- LLM misreporting test results
- Human error in manual verification

**If pre-commit hook FAILS**:
- Commit is blocked
- Fix issues and retry
- Do NOT use `--no-verify` unless explicitly instructed by user
```

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | Initialization | initializer | Status [PROPOSED]→[WIP] | READY |
| 2025-12-27 | Implementation | implementer | Tasks 0-7 | SUCCESS |
| 2025-12-27 | Post-Review | feature-reviewer | mode: post | NEEDS_REVISION (minor) |
| 2025-12-27 | Fix | opus | Phase 10→8/9 refs | SUCCESS |
| 2025-12-27 | AC Verification | opus | Grep validation | OK:16/16 |

---

## Dependencies

**Depends on**: F224 (Agent STOP Conditions) - **SATISFIED** [DONE]

**Rationale**: Workflow redesign in F227 will reference STOP conditions defined in F224 (e.g., kojo-writer timeout behavior, debugger failure limits).

---

## Links

- [F223](feature-223.md) - Issue Inventory (Category 2: Workflow Ambiguity)
- [F224](feature-224.md) - Agent STOP Conditions (prerequisite)
- [do.md](../../.claude/commands/do.md) - Target file for this feature

---

## Notes

### Implementation Strategy

1. **Read F224 first**: Understand STOP conditions before integrating into workflow
2. **Create backup**: `cp .claude/commands/do.md .claude/commands/do.md.bak`
3. **Incremental rewrite**: Don't replace entire file, update sections:
   - Phase table (top of file)
   - Phase 6 (new unified section)
   - Phase 7-9 (new sections replacing old 7-10)
   - Error Handling (add failure counter)
   - Pre-commit hook section (add verify-logs.py emphasis)
4. **Test references**: Ensure all Grep AC matchers use exact strings from new content
5. **Remove old content**: Verify AC 15 (negative matcher) catches old Phase 7 removal

### W4 (CI Hook Verification) Assignment

W4 ("CI で検証済み" but hook 未確認) is assigned to **F229 (Infrastructure Verification)**, not F227.

**Rationale**: F227 documents workflow; F229 verifies infrastructure works as documented.

### Out of Scope

- W5 (User approval case-insensitivity) → F231 (User Approval UX)
- W7 (Error recovery procedures) → F230 (Recovery Procedures)
- Implementation of verify-logs.py fixes (I5) → F229 (Infrastructure Verification)
- P1, P3 (file placement) → F229 (Infrastructure Verification)

These are intentionally separated to keep F227 focused on workflow structure and logic.
