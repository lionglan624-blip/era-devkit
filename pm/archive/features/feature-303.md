# Feature 303: Subagent Skill 呼び出し構造改善

## Status: [DONE]

## Type: infra

## Background

### Philosophy
Subagent は dispatch 時に必ず関連 Skill を読み込み、期待される実装を実行する

### Problem
F302 で FIRST ACTION 指示を追加したが、テキスト指示は無視される可能性がある。
F289 再実行で発覚: `subagent_type: "general-purpose"` + `prompt: "Read .claude/agents/kojo-writer.md..."` 形式だと、agents が "Read" を文字通り解釈し、サマリーだけ返して Skill を呼び出さなかった。

### Root Cause
1. 「Read」という単語への過剰反応 - 「読んで報告せよ」と誤解
2. Agent Identity 未確立 - kojo-writer.md を読んでも kojo-writer として振る舞うべきと認識できず
3. 文脈依存性の誤解 - 「.md を読め」= 「その agent として行動せよ」と理解できなかった

### Historical Context (Why general-purpose was used)

最初の subagent-strategy.md (0cb6dfb, 2025-12-17) に設計理由が記載されていた:

```markdown
Custom agents use `general-purpose` with prompt referencing `.claude/agents/*.md`
Built-in agents (`Explore`, `debugger`) are used directly
```

**当時の設計意図**:
- Custom agents (kojo-writer 等): `general-purpose` + prompt で参照
- Built-in agents (Explore, debugger): 直接 subagent_type 指定

**現状との差異**:
- 当時は `kojo-writer` が Claude Code の built-in ではなかった
- 現在は Task ツールの available agent types に `kojo-writer` が登録済み
- よって `subagent_type: "kojo-writer"` を直接使用可能

**結論**: 当時の Claude Code の制限による設計であり、現在は直接指定に変更して問題なし。

### Solution
`subagent_type: "kojo-writer"` を直接指定し、`.claude/agents/kojo-writer.md` を自動適用させる。
これにより:
- Agent が自動的に kojo-writer として Identity を確立
- "Read" prefix が不要になり誤解を排除
- prompt は `{ID} K{N}` のみで簡潔

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | CLAUDE.md kojo-writer type 変更 | file | contains | "kojo-writer \| kojo-writer \| opus" | [x] |
| 2 | do.md kojo-writer type 変更 | file | contains | "kojo-writer \| kojo-writer \| opus" | [x] |
| 3 | do.md Phase 4 dispatch 例追加 | file | contains | "NEVER use `Read .claude/agents/kojo-writer.md...` prefix" | [x] |
| 4 | CLAUDE.md Dispatch 説明更新 | file | contains | "kojo-writer: `subagent_type: \"kojo-writer\"`" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,4 | CLAUDE.md Subagent Strategy 更新 | [x] |
| 2 | 2,3 | do.md Phase 4 kojo + Subagent Dispatch 更新 | [x] |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 | analysis | - | F289 kojo-writer 失敗原因分析 | "Read" 誤解発見 |
| 2026-01-01 | research | claude-code-guide | subagent_type 動作確認 | 直接指定で自動適用確認 |
| 2026-01-01 | implement | - | CLAUDE.md 更新 | kojo-writer type 変更 |
| 2026-01-01 | implement | - | do.md 更新 | type 変更 + dispatch 例追加 |

## Links
- [index-features.md](index-features.md)
- 親Feature: [feature-302.md](feature-302.md)
- 検証対象: [feature-289.md](feature-289.md)
