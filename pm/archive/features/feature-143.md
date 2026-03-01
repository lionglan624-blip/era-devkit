# Feature 143: Agent MD転換（入出力 + 判断基準のみ）

## Status: [DONE]

## Type: infra

## Background

### Problem
- .claude/agents/ 配下の12ファイルが合計2,937行
- Step列挙、複数の例、重複内容が多い
- エージェントの判断を制限している

### Goal
- 各agent.md を「入出力 + 判断基準 + 例1つ」のみに削減
- 2,937行 → ~600行（各50行程度）

### Context
- [anthropic-recommended-transition.md](designs/anthropic-recommended-transition.md) Phase B
- Anthropic推奨原則#4: 最小ルール + エージェント判断

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | ac-tester.md行数 | output | lte | 80 | [x] |
| 2 | kojo-writer.md行数 | output | lte | 80 | [x] |
| 3 | regression-tester.md行数 | output | lte | 60 | [x] |
| 4 | unit-tester.md行数 | output | lte | 60 | [x] |
| 5 | implementer.md行数 | output | lte | 60 | [x] |
| 6 | 全agent.md合計行数 | output | lte | 700 | [x] |
| 7 | /imple正常動作 | build | succeeds | - | [x] |

### AC Details

#### AC1-5: 各agent.md行数

**Test Command**:
```bash
wc -l .claude/agents/{agent-name}.md | awk '{print $1}'
```

#### AC6: 全agent.md合計行数

**Test Command**:
```bash
wc -l .claude/agents/*.md | tail -1 | awk '{print $1}'
```

**Expected Output**: ≤ 700

#### AC7: /imple正常動作

**Test Command**: 手動で `/imple {feature-id}` を実行

**Expected**: 正常に動作し、エラーなく完了する（exit code 0）

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | ac-tester.md を最小化（431→76行） | [○] |
| 2 | 2 | kojo-writer.md を最小化（420→75行） | [○] |
| 3 | 3 | regression-tester.md を最小化（327→60行） | [○] |
| 4 | 4 | unit-tester.md を最小化（262→49行） | [○] |
| 5 | 5 | implementer.md を最小化（170→56行） | [○] |
| 6 | 6 | 残り8件を最小化（合計684行） | [○] |
| 7 | 7 | ビルド・テスト確認 | [○] |

---

## Target Files (Results)

| # | File | Before | After | 削減率 |
|:-:|------|-------:|------:|:------:|
| 1 | ac-tester.md | 431 | 76 | 82% |
| 2 | kojo-writer.md | 420 | 75 | 82% |
| 3 | regression-tester.md | 327 | 60 | 82% |
| 4 | unit-tester.md | 262 | 49 | 81% |
| 5 | finalizer.md | 214 | 51 | 76% |
| 6 | ac-validator.md | 187 | 49 | 74% |
| 7 | ac-task-aligner.md | 180 | 45 | 75% |
| 8 | debugger.md | 174 | 47 | 73% |
| 9 | doc-reviewer.md | 171 | 48 | 72% |
| 10 | implementer.md | 170 | 56 | 67% |
| 11 | feasibility-checker.md | 159 | 40 | 75% |
| 12 | eratw-reader.md | 142 | 47 | 67% |
| 13 | initializer.md | 100 | 41 | 59% |
| **合計** | | **2,937** | **684** | **77%** |

---

## Transformation Template

### Before (現状)

```markdown
# Agent Name

## Overview
長い説明...

## Step 1: ...
1. ...
2. ...

## Step 2: ...
...

## Examples
### Example 1
...
### Example 2
...
### Example 3
...

## Error Handling
長いエラーケース列挙...

## Notes
...
```

### After (目標)

```markdown
# Agent Name

## Purpose
1文で説明

## Input
- feature.md path
- target character (optional)

## Output
- Modified file path
- Status (SUCCESS/FAIL)

## Decision Criteria
- 条件A: アクションX
- 条件B: アクションY

## Example
[1つだけ]
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-20 11:00 | START | opus | Feature 143 | - |
| 2025-12-20 11:05 | END | opus | Task 1-5 | 5 agents minimized |
| 2025-12-20 11:15 | END | opus | Task 6 | 8 agents minimized |
| 2025-12-20 11:20 | END | opus | Task 7 | Build PASS, 85 tests PASS |
| 2025-12-20 19:55 | END | finalizer | Feature 143 | DONE |

---

## Discovered Issues

| Issue | Type | Priority |
|-------|------|----------|
| | | |

---

## Links

- [anthropic-recommended-transition.md](designs/anthropic-recommended-transition.md)
- [.claude/agents/](../../.claude/agents/)
