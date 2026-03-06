# Feature 273: ac-tester Task ステータス自動更新

## Status: [DONE]

## Type: infra

## Background

### Problem
ac-tester が AC PASS 時に feature-{ID}.md の AC ステータスのみ更新し、対応する Task ステータスを更新しない。
結果、post-review で NEEDS_REVISION となり手動修正が必要になった（F244 で発生）。

### Goal
ac-tester が AC PASS 時に対応する Task も `[x]` に更新する。

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ac-tester.md に Task 更新手順追加 | code | Grep(.claude/agents/ac-tester.md) | contains | "## Task ステータス更新" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | ac-tester.md に Task 更新セクション追加 | [x] |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-31 | - | - | ac-tester.md 更新 | OK |

## Links
- [index-features.md](index-features.md)
- 発生元: [feature-244.md](feature-244.md) Issues Found
