# Feature 086: Subagent Format Standardization

## Status: [DONE]

## Type: engine

## Background

### Problem
- subagent mdファイルの `## Model` セクションに強制力がない
- implement.md と agent md で二重管理、不整合リスク
- Opusがimplement.mdを読み飛ばすとモデル指定を忘れる可能性
- implementerのモデルが `sonnet→opus` で不明確

### Goal
- Claude公式ドキュメントに準拠したsubagentフォーマット導入
- implementerモデルをopusに統一
- Dispatcherへの明確な指示追加

### Context
- Claude Agent SDK公式ドキュメントの推奨パターン:
  - Front matter (`---` ブロック) で name/description/model/tools を宣言
  - description に "MUST BE USED" 等のフレーズで自動委譲を促進
  - "When Invoked" セクションで実行ステップを明示

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | implement.md implementer model修正 | file | contains | ".claude/commands/implement.md":"implementer \| general-purpose \| opus" | [x] |
| 2 | 全agent mdにfront matter追加 | file | matches | ".claude/agents/*.md":"/^---\\nname:/" | [x] |
| 3 | 全agent mdにDISPATCHER INSTRUCTION追加 | file | contains | ".claude/agents/*.md":"DISPATCHER INSTRUCTION" | [x] |
| 4 | 全agent mdにWhen Invokedセクション追加 | file | contains | ".claude/agents/*.md":"## When Invoked" | [x] |

---

## AC Verification Log (2025-12-17)

| AC# | File(s) | Evidence | Result |
|:---:|---------|----------|:------:|
| 1 | .claude/commands/implement.md | Line 23: `\| implementer \| general-purpose \| opus \|` | PASS |
| 2 | All .claude/agents/*.md (12 files) | All 12 files start with `---\nname:` pattern | PASS |
| 3 | All .claude/agents/*.md (12 files) | 13 occurrences found across 12 files (doc-reviewer.md has 2) | PASS |
| 4 | All .claude/agents/*.md (12 files) | 14 occurrences found across 12 files (doc-reviewer.md has 3) | PASS |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | implement.md implementerモデルを opus に修正 | [○] |
| 2 | 2,3,4 | initializer.md 公式フォーマット変更 | [○] |
| 3 | 2,3,4 | kojo-writer.md 公式フォーマット変更 | [○] |
| 4 | 2,3,4 | implementer.md 公式フォーマット変更 | [○] |
| 5 | 2,3,4 | ac-validator.md 公式フォーマット変更 | [○] |
| 6 | 2,3,4 | debugger.md 公式フォーマット変更 | [○] |
| 7 | 2,3,4 | unit-tester.md 公式フォーマット変更 | [○] |
| 8 | 2,3,4 | regression-tester.md 公式フォーマット変更 | [○] |
| 9 | 2,3,4 | ac-tester.md 公式フォーマット変更 | [○] |
| 10 | 2,3,4 | finalizer.md 公式フォーマット変更 | [○] |

---

## Design Notes

### 新フォーマット構造

```markdown
---
name: {agent-name}
description: {役割}. MUST BE USED for {条件}. Requires {model} model.
model: {haiku|sonnet|opus}
tools: {tool list}
---

# {Agent Name}

**DISPATCHER INSTRUCTION**: Call with `model: "{model}"` - {理由}.

{persona/role description}

## When Invoked

1. {Step 1}
2. {Step 2}
...
```

### モデル統一方針

| Agent | Model | 理由 |
|-------|:-----:|------|
| initializer | haiku | 軽量な状態抽出 |
| kojo-writer | opus | 創作には高い言語能力必要 |
| implementer | **opus** | 実装には高い推論能力必要 |
| ac-validator | sonnet | 分析的推論 |
| debugger | sonnet→opus | 3回目でescalation |
| unit-tester | haiku | 軽量なテスト実行 |
| regression-tester | haiku | 軽量なテスト実行 |
| ac-tester | haiku | 軽量なテスト実行 |
| finalizer | haiku | 軽量なステータス更新 |

### 変更ファイル一覧

| File | Changes |
|------|---------|
| `.claude/commands/implement.md` | implementer model: opus |
| `.claude/agents/initializer.md` | 公式フォーマット |
| `.claude/agents/kojo-writer.md` | 公式フォーマット |
| `.claude/agents/implementer.md` | 公式フォーマット |
| `.claude/agents/ac-validator.md` | 公式フォーマット |
| `.claude/agents/debugger.md` | 公式フォーマット + escalationテーブル |
| `.claude/agents/unit-tester.md` | 公式フォーマット |
| `.claude/agents/regression-tester.md` | 公式フォーマット |
| `.claude/agents/ac-tester.md` | 公式フォーマット |
| `.claude/agents/finalizer.md` | 公式フォーマット |

---

## Execution State

| Component | Status | Ready | Notes |
|-----------|:------:|:-----:|-------|
| implement.md | DONE | ✅ | implementer model → opus |
| initializer.md | DONE | ✅ | 公式フォーマット完了 |
| kojo-writer.md | DONE | ✅ | 公式フォーマット完了 |
| implementer.md | DONE | ✅ | 公式フォーマット完了 |
| ac-validator.md | DONE | ✅ | 公式フォーマット完了 |
| debugger.md | DONE | ✅ | 公式フォーマット + escalation表 |
| unit-tester.md | DONE | ✅ | 公式フォーマット完了 |
| regression-tester.md | DONE | ✅ | 公式フォーマット完了 |
| ac-tester.md | DONE | ✅ | 公式フォーマット完了 |
| finalizer.md | DONE | ✅ | 公式フォーマット完了 |
| explorer.md | DONE | ✅ | 削除完了 |
| AC Verification | DONE | ✅ | 全4 AC PASS |

**Completed**: 2025-12-17 by Finalizer

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-17 | Opus | Claude公式ドキュメント調査 | Task tool best practices確認 |
| 2025-12-17 | Opus | implement.md修正 | implementer model → opus |
| 2025-12-17 | Opus | 全agent md (10ファイル) 公式フォーマット変更 | 完了 |
| 2025-12-17 | Opus | explorer.md削除 | Built-in Explore agent使用のため不要 |
| 2025-12-17 | Opus | AC実ファイル検証 | 全4 AC PASS |
| 2025-12-17 | Initializer | Feature initialization | Status updated to [WIP] |

---

## Discovered Issues

| Issue | Type | Priority |
|-------|------|----------|
| ~~explorer.mdはbuilt-in agent補足のため変更不要~~ | ~~Note~~ | - |
| explorer.md削除 - Built-in Explore使用のため.mdファイル不要 | Resolved | - |

---

## Links

- [imple.md](../../.claude/commands/imple.md)
- [Claude Agent SDK - Subagents](https://platform.claude.com/docs/en/agent-sdk/subagents)
- [Claude Code - Subagents](https://code.claude.com/docs/en/sub-agents)
