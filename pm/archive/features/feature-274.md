# Feature 274: kojo-writer ステータスファイル出力

## Status: [DONE]

## Type: infra

## Background

### Problem
kojo-writer が完了時にステータスファイルを出力しない。
do.md は `Game/agents/status/{ID}_K{N}.txt` をポーリングする設計だが、kojo-writer.md に出力手順がない。
結果、ポーリングがタイムアウトし、grep で ERB 直接確認に回避策を取った（F244 で発生）。

### Goal
kojo-writer.md に完了時のステータスファイル出力手順を追加。

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | kojo-writer.md にステータス出力手順追加 | code | contains | "Game/agents/status/" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo-writer.md に Status File Output セクション追加 | [x] |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-31 | - | - | kojo-writer.md 更新 | OK |

## Links
- [index-features.md](index-features.md)
- 発生元: [feature-244.md](feature-244.md) Issues Found
