# Feature 222: Subagent Skills Auto-Load (Anthropic Best Practice)

## Status: [DONE]

## Type: infra

## Background

### Problem

サブエージェントが Skills を確実に参照しない問題が F186-F187 で発生。
現状は agent.md 本文で「Skill(X) を参照」と記載しているが、呼び忘れリスクがあった。

**発生した問題**:
- F186: kojo-writer が kojo-writing SKILL の COM 配置ルールを参照せず、誤ったファイルに配置
- F187: Expected 値の不一致、TDD 違反

### Root Cause

Anthropic 公式ドキュメントの推奨パターンに従っていなかった。

### Solution

YAML frontmatter の `skills:` フィールドで宣言 → エージェント起動時に自動ロード。

---

## Anthropic Official Documentation

### 1. Subagent Skills Field

**Source**: Claude Code Subagents Documentation
**URL**: https://docs.anthropic.com/en/docs/claude-code/sub-agents

> **Skills field (Optional)**: Comma-separated list of skill names to auto-load when the subagent starts. Skills are loaded into the subagent's context automatically.

**Example from documentation**:
```yaml
---
name: your-subagent-name
skills: skill1, skill2, skill3
---
```

### 2. Skills as Single Source of Truth

**Source**: Claude Code Skills Documentation
**URL**: https://docs.anthropic.com/en/docs/claude-code/skills

> Skills are the Single Source of Truth for commands. Agents MUST reference Skills, not hardcode commands.

### 3. Progressive Disclosure Architecture

**Source**: Agent SDK Skills Overview
**URL**: https://docs.anthropic.com/en/docs/agents-and-tools/agent-skills/overview

> Skills leverage Claude's VM environment to provide capabilities beyond what's possible with prompts alone... This filesystem-based architecture enables **progressive disclosure**: Claude loads information in stages as needed, rather than consuming context upfront.

### 4. Subagent Configuration Pattern

**Source**: Agent SDK Skills Best Practices
**URL**: https://docs.anthropic.com/en/docs/agents-and-tools/agent-skills/best-practices

> Keep Skills focused: One Skill should address one capability.

Multiple subagents can reference the same Skill, ensuring they use identical logic rather than duplicating instructions.

---

## Changes

| Agent | Before | After |
|-------|--------|-------|
| kojo-writer | `tools: ..., Skill` + 本文で参照指示 | `skills: kojo-writing, erb-syntax` |
| implementer | 本文で `Skill(testing)`, `erb-syntax` 参照指示 | `skills: erb-syntax, testing` |
| ac-tester | 本文で `Skill(testing)` 参照指示 | `skills: testing` |

**Note**: `Skill` は `tools:` から削除しない（動的に追加 Skill をロードする可能性を残す）。

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | kojo-writer.md に skills: フィールド | file | contains | "skills:" | [x] |
| 2 | implementer.md に skills: フィールド | file | contains | "skills:" | [x] |
| 3 | ac-tester.md に skills: フィールド | file | contains | "skills:" | [x] |

---

## Execution Log

| Date | Action | Result |
|------|--------|--------|
| 2025-12-26 | Feature 222 作成、公式ドキュメント引用 | DONE |
| 2025-12-26 | kojo-writer.md に skills: 追加 | DONE |
| 2025-12-26 | implementer.md に skills: 追加 | DONE |
| 2025-12-26 | ac-tester.md に skills: 追加 | DONE |

---

## Notes

- この変更は既存の動作に影響しない（Skills は追加でロードされるのみ）
- `Skill` tool は tools: に残す（将来の動的ロード用）
- 本文の「Skill(X) を参照」記述は残す（二重の安全策）
