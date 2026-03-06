# Feature 228: Subagent I/O Optimization

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)

Subagent communication should follow the **Minimal Context** principle: "OK時は簡潔、ERR時のみ詳細". This minimizes context window consumption and focuses on actionable information only. Information already stored in files should be referenced, not duplicated in agent output.

### Problem (Current Issue)

F223 Category 7 (Subagent I/O) identified 6 issues with excessive information exchange:

| ID | Agent | Problem | Risk |
|:--:|-------|---------|------|
| O1 | initializer | OK時にBackground/Tasks/ACs等を全返却 | Context肥大化 |
| O2 | implementer | OK時にFiles/Changes/Docs等を全返却 | Context肥大化 |
| O3 | debugger | 入力形式未定義 | 過剰情報渡し |
| O4 | ac-tester | AC table全体が入力？ | 不要情報 |
| O5 | kojo-writer | status file を Read している | Glob件数確認のみで十分 |
| O6 | kojo-writer | 失敗検出を Phase 4 で行う | Phase 5 test gen に委任可能 |

Current agent output patterns are inconsistent:
- **Good**: eratw-reader, ac-tester, regression-tester, debugger, finalizer, feature-reviewer (concise OK, detailed ERR)
- **Bad**: initializer, implementer (return full content even on success)

### Goal (What to Achieve)

Standardize subagent I/O to:
1. **Unified output format**: `STATUS` (OK) or `STATUS:{count}|{detail}` (ERR)
2. **Reduced polling overhead**: Remove redundant Read operations in Phase 4 kojo polling
3. **Defined input contracts**: Document minimum required input for each agent
4. **Delegation**: Move failure detection to appropriate phases (kojo-writer errors → Phase 5 test gen)

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | initializer output format | file | Grep | matches | `READY:\d+:(kojo|erb|engine|infra)` | [x] |
| 2 | implementer output format | file | Grep | matches | `SUCCESS$` | [x] |
| 2a | implementer verbose removal | file | Grep | not_contains | "Files," | [x] |
| 3 | debugger input contract | file | Grep | contains | "## Input Contract" | [x] |
| 4 | ac-tester input minimized | file | Grep | contains | "Receives AC# and Target only" | [x] |
| 5 | do.md Phase 4 Read removal | file | Grep | not_contains | `Read("Game/agents/status/` | [x] |
| 6 | do.md Phase 5 failure delegation | file | Grep | contains | "Missing functions" | [x] |
| 7 | kojo-writer status optional | file | Grep | contains | "optional" | [x] |
| 8 | I/O format guide documented | file | Glob | exists | Game/agents/reference/io-format-guide.md | [x] |

### AC Details

**AC1**: initializer.md Output section
- Should return: `READY:{ID}:{Type}` format (e.g., `READY:228:infra`)
- Regex pattern: `READY:\d+:(kojo|erb|engine|infra)`
- Details available in: `feature-{ID}.md`

**AC2**: implementer.md Output section
- Should return: `SUCCESS` only (no Files/Build/Changes/Docs)
- Details available in: feature.md Execution Log

**AC2a**: implementer.md verbose removal verification
- Verify "Files," does NOT appear in Output section (verbose fields removed)

**AC3**: debugger.md new Input Contract section
- Add section header: `## Input Contract`
- Define minimum required fields
- Example: Error type, file path, line number, error message

**AC4**: ac-tester.md Input section
- Add text: "Receives AC# and Target only" (NOT full AC table)
- Reads feature.md internally for matcher/expected

**AC5**: .claude/commands/do.md Phase 4
- Remove: `Read("Game/agents/status/{ID}_K*.txt")`
- Keep: `Glob("Game/agents/status/{ID}_K*.txt")` for count check

**AC6**: .claude/commands/do.md Phase 5 [ALREADY SATISFIED]
- Text "Missing functions" already exists in Phase 5 (line 311)
- No additional implementation required

**AC7**: kojo-writer.md Status File section
- Update: "Optional. /do polling uses Glob only, does not Read content."

**AC8**: Create Game/agents/reference/io-format-guide.md
- Document unified I/O format principle
- List each agent's input/output contract
- Provide examples of good/bad patterns

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Update `.claude/agents/initializer.md` output format to READY:{ID}:{Type} | [x] |
| 2 | 2,2a | Update `.claude/agents/implementer.md` output format to SUCCESS only (remove verbose) | [x] |
| 3 | 3 | Add Input Contract section to `.claude/agents/debugger.md` | [x] |
| 4 | 4 | Clarify `.claude/agents/ac-tester.md` input is minimal (AC#/Target) | [x] |
| 5 | 5 | Remove status file Read from `.claude/commands/do.md` Phase 4 | [x] |
| 6 | 6 | ~~Document Phase 5 failure detection~~ (Already satisfied) | [x] |
| 7 | 7 | Mark status file as optional in `.claude/agents/kojo-writer.md` | [x] |
| 8 | 8 | Create `Game/agents/reference/io-format-guide.md` reference document | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 13:22 | START | initializer | Feature 228 init | READY:228:infra |
| 2025-12-27 13:22 | - | explorer | Investigation | READY |
| 2025-12-27 13:22 | - | implementer | Tasks 1-5,7,8 | SUCCESS |
| 2025-12-27 13:22 | - | - | Static AC verification | PASS:8/8 |
| 2025-12-27 13:22 | END | feature-reviewer | Post review | READY |

---

## Dependencies

- F224 (Agent STOP Conditions) [DONE]: Defines BLOCKED output formats. F228 complements with SUCCESS format standardization

---

## Links

- [F223](feature-223.md) - /do Workflow Comprehensive Audit (Category 7: Subagent I/O)
- [F224](feature-224.md) - Agent STOP Conditions

---

## Notes

### Unified Output Format Principle

```
OK時:  STATUS (1語)
ERR時: STATUS:{count}|{detail}
       - 詳細は必要最小限
       - ファイル参照で済む情報は含めない
```

**Exception**: Reviewers (feature-reviewer, doc-reviewer) return JSON for structured issue reporting.

### Agent Classification (F223 Review)

**Good Examples** (already following principle):
- eratw-reader: `OK:cached` / `ERR:{reason}`
- ac-tester: `OK:{n}/{m}` / `ERR:{count}|{ACs}`
- regression-tester: `Regression: OK:24/24`
- debugger: `FIXED` / `UNFIXABLE` / `QUICK_WIN` / `BLOCKED`
- finalizer: `READY_TO_COMMIT` / `REVIEW_NEEDED` / `NOT_READY`
- feature-reviewer: JSON `{status, issues[]}`

**Needs Improvement**:
- initializer: Returns Background/Tasks/ACs even on success
- implementer: Returns Files/Build/Changes/Docs even on success

### Kojo-Writer Polling Changes (O5-O6)

**Current Flow**:
```
1. sleep 360
2. Glob("status/{ID}_K*.txt") → 件数確認
3. 10件揃うまで sleep 60 繰り返し
4. 全ファイル Read して OK/ERROR 判定  ← 削除
5. ERROR あれば STOP
6. 全部 OK なら Phase 5 へ
```

**Optimized Flow**:
```
1. sleep 360
2. Glob("status/{ID}_K*.txt") → 件数確認
3. 10件揃うまで sleep 60 繰り返し
4. Phase 5 へ (Read しない)  ← 簡素化
5. test gen が Missing functions で失敗検出
```

**Rationale**:
1. kojo-writer が「自覚できる失敗」は稀（ファイル書き込み失敗のみ）
2. ほとんどの失敗は後続フェーズで機械的に検出:
   - Phase 5 (test gen): `Missing functions` で未作成検出
   - Phase 6 (Hooks): `--strict-warnings` で構文エラー検出
   - Phase 7 (AC): テスト FAIL で内容品質問題検出
3. Read 10回 → コンテキスト消費
4. Glob 1回 → ファイル名リストのみ（最小限）

**Changes**:

| 項目 | 現状 | 変更案 |
|------|------|--------|
| 完了確認 | Glob | **維持** |
| 内容確認 | Read 10回 | **削除** |
| 失敗検出 | Phase 4 で ERROR 判定 | Phase 5 test gen に委任 |
| kojo-writer 出力 | status file 書く | **書かなくてよい（optional）** |

### Implementation Scope

**In Scope**:
- Update agent .md files (initializer, implementer, debugger, ac-tester, kojo-writer)
- Update do.md Phase 4 and Phase 5
- Create io-format-guide.md reference

**Out of Scope**:
- Runtime code changes (C#/Python/PowerShell) - only .md documentation updates
- Breaking changes to existing agent output (maintain backward compatibility where needed)

### File Locations

- Agent definitions: `.claude/agents/*.md`
- Workflow: `.claude/commands/do.md`
- Reference: `Game/agents/reference/io-format-guide.md` (new)
