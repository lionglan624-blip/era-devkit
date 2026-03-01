# Feature 150: Skills導入 + `skills:` フィールド実験

## Status: [DONE]

## Type: infra

## Background

Feature 149 の公式ドキュメント検証結果に基づき、Claude Code の Skills 機能を導入する。

**経緯:**
- Anthropic公式ドキュメントで Skills は「Progressive Disclosure（段階的開示）」として推奨
- reference/*.md を Skills に統合することでメンテコスト削減
- `skills:` フィールドを subagent に適用した場合の効果は公式ドキュメントで未明記 → 実験が必要

**Goals:**
1. reference/*.md を Skills に統合（メンテコスト削減）
2. `skills:` フィールドの subagent 効果を検証

**Scope:**

```
.claude/skills/
├── erb-syntax/
│   └── SKILL.md           # erb-reference.md から移行
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

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | erb-syntax Skill ファイル存在 | file | exists | `.claude/skills/erb-syntax/SKILL.md` | [x] |
| 2 | kojo-writing Skill ファイル存在 | file | exists | `.claude/skills/kojo-writing/SKILL.md` | [x] |
| 3 | testing Skill ファイル存在 | file | exists | `.claude/skills/testing/SKILL.md` | [x] |
| 4 | engine-dev Skill ファイル存在 | file | exists | `.claude/skills/engine-dev/SKILL.md` | [x] |
| 5 | skills: フィールド構文検証 | file | exists | `.claude/agents/implementer.md` | [x] |
| 6 | ビルド成功 | build | succeeds | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | erb-reference.md を .claude/skills/erb-syntax/SKILL.md に移行 | [x] |
| 2 | 2 | kojo-reference.md を .claude/skills/kojo-writing/SKILL.md に移行（canon-lines.md も配置） | [x] |
| 3 | 3 | testing-reference.md を .claude/skills/testing/SKILL.md に移行 | [x] |
| 4 | 4 | engine-reference.md を .claude/skills/engine-dev/SKILL.md に移行 | [x] |
| 5 | 5 | implementer.md に skills: フィールドを追加（構文検証） | [x] |
| 6 | 6 | dotnet build 実行確認 | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-20 13:43 | START | Opus | Feature 150 implementation | - |
| 2025-12-20 13:43 | DISPATCH | implementer×4 | Tasks 1-4 (parallel) | SUCCESS |
| 2025-12-20 13:44 | DISPATCH | implementer | Task 5 | SUCCESS |
| 2025-12-20 13:44 | VERIFY | Opus | Build + AC verification | ALL PASS |
| 2025-12-20 13:44 | END | Opus | Feature 150 complete | DONE |
| 2025-12-20 13:50 | EXPERIMENT | Opus | skills: subagent effect test | NEGATIVE |

---

## Experiment Results: Skills for Subagents

### 実験1: `skills:` フィールド（自動注入）

**仮説**: `skills:` フィールドが subagent に Skills コンテンツを自動注入するか

| シナリオ | Skills 自動ロード | 結果 |
|----------|:-----------------:|:----:|
| 質問形式（ERB構文について質問） | ❌ | Read 必要 |
| 明示的指示（"use your erb-syntax skill"） | ❌ | Read 必要 |
| 実装タスク（ERB関数を書く） | ❌ | Read 必要 |

**結論**: `skills:` フィールドは subagent には無効（メタデータのみ）

---

### 実験2: `tools: Skill`（Skill ツール追加）

**仮説**: `tools:` に `Skill` を追加すれば subagent が Skill ツールを使えるか

| 設定 | Skill ツール使用 | 結果 |
|------|:----------------:|:----:|
| `tools:` に Skill なし | ❌ | 使えない |
| `tools:` に Skill あり | ✅ | **使える！** |

**結論**: `tools: Skill` を追加すれば subagent も Skills を使える

---

### 最終結論

| 方法 | Subagent で有効 | 備考 |
|------|:---------------:|------|
| `skills:` フィールド | ❌ | 自動注入されない |
| `tools: Skill` | ✅ | **推奨** |

**推奨設定**:
```yaml
---
name: implementer
tools: Read, Write, Edit, Bash, Glob, Grep, Skill
---
```

### 必要条件

1. **SKILL.md に YAML frontmatter が必須**
   ```yaml
   ---
   name: erb-syntax
   description: ERB scripting syntax reference. Use when...
   ---
   ```

2. **`tools:` に `Skill` を追加**

3. **description にトリガーキーワードを含める**

---

## Links

- [feature-149.md](feature-149.md) - 親Feature（公式ドキュメント検証）
- [reference/erb-reference.md](reference/erb-reference.md) - 移行元
- [reference/kojo-reference.md](reference/kojo-reference.md) - 移行元
- [reference/testing-reference.md](reference/testing-reference.md) - 移行元
- [reference/engine-reference.md](reference/engine-reference.md) - 移行元
