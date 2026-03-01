# Feature 304: kojo-writer Workflow 明記

## Status: [DONE]

## Type: infra

## Background

### Problem
kojo-writer.md に Input format と Workflow が明記されておらず、dispatch 時にエージェントが何をすべきか不明確だった。F289 実行時に dispatch format の誤りが発生。

### Goal
kojo-writer.md に Input format と Workflow を明記し、自律的に動作できるようにする。

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | Input format 明記 | code | contains | `{ID} K{N}` | [x] |
| 2 | Workflow セクション追加 | code | exists | Workflow section | [x] |
| 3 | INVALID_FORMAT STOP 追加 | code | contains | `BLOCKED:INVALID_FORMAT` | [x] |
| 4 | do.md dispatch format 簡略化 | code | contains | `{ID} K{N}` | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3 | kojo-writer.md 修正 | [x] |
| 2 | 4 | do.md dispatch format 更新 | [x] |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 | create | - | F289 議論から派生 | DONE |

## Links

- [index-features.md](index-features.md)
- Related: [feature-289.md](feature-289.md) - 発生源
