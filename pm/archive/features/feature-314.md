# Feature 314: finalizer dispatch を専用 subagent_type に変更

## Status: [DONE]

## Type: infra

## Background

### Philosophy
Subagent dispatch は専用 type を使用して信頼性を確保する

### Problem
finalizer が `general-purpose` + `Read .claude/agents/finalizer.md...` 形式で dispatch されており、haiku が agent.md を読んで解釈する必要があった。これにより:
- 実行せず要約のみ返す
- 実装ファイルを commit に含めない
などの問題が F313 で発生。

### Goal
finalizer を専用 subagent_type で dispatch し、kojo-writer と同様の信頼性を確保する

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | do.md Subagent Dispatch テーブルで finalizer の type が finalizer | code | contains | "finalizer \| finalizer \| haiku" | [x] |
| 2 | Step 9.1 の dispatch が subagent_type: "finalizer" を使用 | code | contains | 'subagent_type: "finalizer"' | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | do.md の finalizer dispatch を専用 type に変更 | [O] |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 17:15 | START | Opus | Direct fix | - |
| 2026-01-02 17:15 | END | Opus | Direct fix | SUCCESS |

## Links
- [index-features.md](index-features.md)
- 親Feature: [feature-313.md](feature-313.md)
