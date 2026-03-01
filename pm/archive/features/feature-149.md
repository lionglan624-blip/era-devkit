# Feature 149: Claude Code Architecture Refactoring (Anthropic BP)

## Status: [DONE] ← 設計完了、実装は150+で

## Type: infra

## Background

現在のClaude Code構成（hooks, subagents, commands, docs, reference）はプロジェクト固有の進化を遂げてきたが、Anthropicの公式ベストプラクティスとの整合性を検証し、最適化の余地を特定する。

---

## 公式ドキュメント検証結果（2025-12-20）

### 1. Subagent によるコンテキスト分離 ✅ 公式推奨

**Agent SDK Subagents ドキュメントより:**

> "Subagents maintain separate context from the main agent, preventing information overload and keeping interactions focused. **This isolation ensures that specialized tasks don't pollute the main conversation context with irrelevant details.**"

> "A `research-assistant` subagent can explore dozens of files and documentation pages **without cluttering the main conversation** with all the intermediate search results, returning only the relevant findings."

**→ 本プロジェクトの「Opus は意思決定のみ、実装は subagent へ委譲」戦略は公式に合致**

### 2. モデル選択 ⚠️ 公式は Sonnet を orchestration 向けと記載

**Model Selection ドキュメントより:**

| モデル | 公式推奨用途 |
|--------|-------------|
| **Sonnet 4.5** | "Best model for complex agents and coding, **superior tool orchestration** for long-running autonomous tasks" |
| **Opus 4.5** | "Maximum intelligence with practical performance for **complex specialized tasks**" |
| **Haiku 4.5** | "Near-frontier performance with **lightning-fast speed**" |

**→ 公式は「Opus = orchestration」とは言っていない。ただし本プロジェクトでは Opus のコンテキスト保護（意思決定に集中）を優先し、現行戦略を維持する**

### 3. Skills による統合 ✅ 公式推奨

**Skills Best Practices ドキュメントより:**

> "Skills leverage Claude's VM environment to provide capabilities beyond what's possible with prompts alone... This filesystem-based architecture enables **progressive disclosure**: Claude loads information in stages as needed, **rather than consuming context upfront**."

**公式メリット:**
> "- **Specialize Claude**: Tailor capabilities for domain-specific tasks
> - **Reduce repetition**: Create once, use automatically
> - **Compose capabilities**: Combine Skills to build complex workflows"

**Progressive Disclosure（段階的開示）:**

| Level | ロードタイミング | トークンコスト |
|-------|-----------------|---------------|
| Level 1: Metadata | 常時（起動時） | ~100 tokens/Skill |
| Level 2: Instructions | Skill トリガー時 | <5k tokens |
| Level 3+: Resources | 必要時のみ | 実質無制限 |

**→ reference/*.md を Skills に統合することで、メンテコスト削減と Progressive Disclosure の両方を実現可能**

### 4. Subagent への Skills 適用 ⚠️ 要検証

**Subagent Frontmatter 公式6フィールド:**

```yaml
name: agent-name              # Required
description: "..."            # Required
tools: tool1, tool2           # Optional
model: sonnet|opus|haiku      # Optional
permissionMode: default       # Optional
skills: skill1, skill2        # Optional ← 存在するが効果の説明なし
```

**→ `skills:` フィールドは公式に存在するが、subagent への効果は明示されていない。Feature 150 で実験**

---

## 結論

### 現行アーキテクチャの評価

| 項目 | 公式サポート | 本プロジェクト | 評価 |
|------|:-----------:|:-------------:|:----:|
| Subagent でコンテキスト分離 | ✅ 明示的推奨 | 採用済み | ◎ |
| Opus を意思決定に使用 | ⚠️ Sonnet 推奨だが禁止ではない | 採用済み | ○ |
| Skills でリファレンス統合 | ✅ 明示的推奨 | 未採用 | 要導入 |
| Skills を Subagent に適用 | ⚠️ 機能存在、効果未明 | 未採用 | 要実験 |

### 最終判断

1. **Subagent 分離戦略** → **維持**（公式推奨に合致）
2. **Opus 使用** → **維持**（コンテキスト保護の観点から有効）
3. **Skills 導入** → **Feature 150 で実験**（公式推奨、メンテコスト削減）
4. **Rules 導入** → **Feature 150 で導入**（path-based reminders）

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | 現状分析完了 | file | exists | `designs/claude-code-current.md` | [x] |
| 2 | 公式ドキュメント検証完了 | file | contains | "公式ドキュメント" | [x] |
| 3 | 公式引用に基づく結論 | file | contains | "Agent SDK Subagents" | [x] |
| 4 | 150以降計画の定義 | file | contains | "Feature 150" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | 現状分析（claude-code-current.md）| [x] |
| 2 | 2 | 公式ドキュメント検証 | [x] |
| 3 | 3 | 公式引用に基づく結論文書化 | [x] |
| 4 | 4 | 150以降の計画定義 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-20 12:43 | START | Opus | Feature 149 implementation | - |
| 2025-12-20 12:45 | DISPATCH | implementer×4 | Tasks 1-4 (parallel) | SUCCESS |
| 2025-12-20 12:51 | DISPATCH | ac-tester | AC verification | PASS |
| 2025-12-20 12:52 | REVISION | Opus | 公式ドキュメント再検証 | 設計修正 |
| 2025-12-20 xx:xx | REVISION | Opus | ゼロベースレビュー | 最終結論 |

---

## 次期計画

### Feature 150: Skills 導入 + `skills:` フィールド実験

**Goals:**
1. reference/*.md を Skills に統合（メンテコスト削減）
2. `skills:` フィールドの subagent 効果を検証

**Scope:**

```
.claude/skills/
├── erb-syntax/
│   ├── SKILL.md           # erb-reference.md から移行
│   └── functions.md       # 詳細リファレンス
├── kojo-writing/
│   ├── SKILL.md           # kojo-reference.md から移行
│   └── canon-lines.md     # キャラ口調リファレンス
├── testing/
│   └── SKILL.md           # testing-reference.md から移行
└── engine-dev/
    └── SKILL.md           # engine-reference.md から移行

.claude/rules/
├── 00-always.md           # 常時ロード（言語、コミット規約）
├── erb-files.md           # paths: "**/*.erb" → ERB編集時
└── kojo-files.md          # paths: "**/口上/**" → kojo編集時
```

**Experiment:**
- 1つの subagent に `skills: erb-syntax` を追加
- Skills 内容が subagent に読み込まれるか検証
- 結果に基づいて全面移行の要否を判断

### Feature 151: Skills 全面展開（150の結果次第）

**If `skills:` effective:**
- 全 subagent に `skills:` フィールド追加
- reference/*.md を Skills に完全移行
- 重複ファイル削除

**If `skills:` ineffective:**
- Skills はメインコンテキスト用のみ
- reference/*.md は subagent 用に維持
- subagent prompts は現行 Read 参照パターン維持

---

## 公式ドキュメント引用元

1. **Agent SDK Subagents**: Context isolation, parallelization benefits
2. **Model Selection**: Sonnet for orchestration, model recommendations
3. **Skills Best Practices**: Progressive disclosure, reduce repetition
4. **Claude Code Subagents**: Frontmatter fields including `skills:`

---

## Links

- [designs/claude-code-current.md](designs/claude-code-current.md) - 現状分析
- [designs/claude-code-target.md](designs/claude-code-target.md) - 目標設計
- [designs/claude-code-gap-analysis.md](designs/claude-code-gap-analysis.md) - ギャップ分析
- [CLAUDE.md](../../CLAUDE.md) - 現行プロジェクト設定
