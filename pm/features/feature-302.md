# Feature 302: kojo-writer サブエージェント Skill 未読み込み問題

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
サブエージェントは Skill を正しく読み込み、期待される実装を確実に実行する

### Problem (Current Issue)

F289 実行中に発見された問題:

6つの kojo-writer サブエージェントが dispatch されたが、全て以下の動作で終了:

```
[Tool: Read] kojo-writer.md
[Tool: Read] eratw-COM_83.txt
--- RESULT ---
I have read both files. Here is a summary: ...
```

**期待される動作**:
1. `.claude/agents/kojo-writer.md` を読む
2. `Skill(kojo-writing)` を読み込む
3. `Game/agents/feature-289.md` を読む
4. ERB ファイルを編集
5. ステータスファイル `Game/agents/status/289_K{N}.txt` を作成

**実際の動作**:
1. kojo-writer.md を読む
2. eratw-COM_83.txt を読む
3. 両ファイルの要約を返して終了

**結果**:
- ERB 実装: 0/6
- ステータスファイル: 0/6

### Dispatch Prompt

```
Read .claude/agents/kojo-writer.md. 289 K1. COM reference cache: Game/agents/cache/eratw-COM_83.txt
```

**問題点の可能性**:
1. プロンプトが不十分で、エージェントが「読んで要約する」と解釈した
2. Skill ツールの呼び出しが発生しなかった
3. モデルがタスクの意図を理解しなかった

### Goal (What to Achieve)

kojo-writer dispatch プロンプトを改善し、確実に以下を実行させる:
1. Skill(kojo-writing) を読み込む
2. 実際に ERB を編集する
3. ステータスファイルを作成する

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | kojo-writer.md に FIRST ACTION 追加 | code | Grep | contains | "FIRST ACTION" | [x] |
| 2 | F303 feature spec 作成 | code | Glob | exists | Game/agents/feature-303.md | [x] |

**Scope Note**: Minimal targeted fix + follow-up feature creation. Runtime verification via F289 re-execution.

**Removed AC**: do.md 修正は Opus 向けドキュメントであり、subagent に到達しないため削除。kojo-writer.md 修正のみが実効的な fix。

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo-writer.md の MUST edit 文の直後に FIRST ACTION 追加 | [x] |
| 2 | 2 | F303 (dispatch 構造変更) を PROPOSED で作成 | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

**Post-Completion**: F302 完了後、F289 を BLOCKED→PROPOSED に戻し再実行で runtime 検証

---

## Design Notes

### 現在の dispatch プロンプト

```
Read .claude/agents/kojo-writer.md. 289 K1. COM reference cache: Game/agents/cache/eratw-COM_83.txt
```

### Dispatch プロンプト構造

Full dispatch prompt = `Read .claude/agents/{agent}.md. {payload}. Cache: {cache_path}`

| 部分 | 定義場所 | 役割 |
|------|----------|------|
| `Read .claude/agents/{agent}.md` | CLAUDE.md Subagent Strategy | Subagent が自己定義を読む |
| `{payload}` | **do.md Minimal Dispatch Format** | タスク内容 |
| `Cache: {cache_path}` | do.md Phase 4 | eratw-reader 出力パス |

### 修正内容

**Task 1: kojo-writer.md の MUST edit 文の直後に FIRST ACTION 追加**

```markdown
**FIRST ACTION**: Invoke `Skill(kojo-writing)` tool immediately. Do NOT proceed without loading this skill.
```

**挿入位置**: L12 (空行後、## Input の前)

**変更理由**:
- F289 で subagent が Skill を呼び出さなかった
- kojo-writer.md に FIRST ACTION で明示的に Skill 呼び出しを指示

**Note**: do.md 修正は検討したが、do.md は Opus (Orchestrator) 向けドキュメントであり、subagent には到達しない。kojo-writer.md 修正のみが subagent に直接適用される実効的な fix。

### Task 2: F303 feature spec

**F303: Subagent Skill 呼び出し構造改善**

```markdown
## Status: [PROPOSED]

## Type: infra

## Background

### Philosophy
Subagent は dispatch 時に必ず関連 Skill を読み込み、期待される実装を実行する

### Problem
F302 で FIRST ACTION 指示を追加したが、テキスト指示は無視される可能性がある。
より確実な Skill 呼び出し構造が必要。

### Goal
dispatch prompt 構造を変更し、Skill 呼び出しを確実にする

## 調査項目
1. frontmatter `skills:` は自動ロードをトリガーするか？
2. dispatch prompt に explicit Skill() を含める方法
3. 他の agent (ac-tester, implementer) の成功パターン分析

## Acceptance Criteria
TBD (F302 実行結果に基づき具体化)
```

**作成タイミング**: F302 Task 1 完了後、F289 再実行前

### 根本原因分析

| 項目 | 確認結果 |
|------|----------|
| kojo-writer.md L11 | "MUST edit ERB files" **存在** (F293 完了) |
| do.md ERB確認 | "ERB function existence check" **存在** (F293 完了) |
| kojo-writing SKILL L19 | "「Read kojo-writer.md」は読むだけで終わってはいけない" **存在** |
| **真の問題** | Subagent が Skill(kojo-writing) を呼び出さなかった |

**分析**:
- kojo-writing SKILL (L10-17) に Execution Model (AUTONOMOUS) と役割 (ERBコードを書く) が定義されている
- しかし subagent が Skill を invoke しなかったため、これらの指示が読まれなかった
- dispatch プロンプトの `{ID} K{N}` だけでは Skill 呼び出しを誘発できない

**結論**: dispatch プロンプトと kojo-writer.md に明示的な `Invoke Skill()` 指示を追加する。

---

## Investigation

### 調査結果 (完了)

| 確認項目 | 結果 |
|----------|------|
| kojo-writer.md L11 | "MUST edit ERB files" **存在** |
| kojo-writer.md L5-6 | frontmatter `skills: kojo-writing` **存在** |
| kojo-writer.md L72 | "See Skill: kojo-writing" **存在** (ファイル末尾) |
| do.md ERB確認ステップ | "ERB function existence check" **存在** |
| F293 修正 | **有効** (ドキュメントに反映済み) |

### 未解決の疑問 (Out of Scope)

- frontmatter `skills:` は自動ロードをトリガーするか、明示的 `Skill()` 呼び出しが必要か？
- 他の agent (ac-tester, implementer) は `MUST: Skill()` パターンで動作している
- FIRST ACTION は「テキスト指示」であり、無視される可能性は残る
- より根本的な解決は dispatch 構造の変更が必要かもしれない

### 根本原因

dispatch プロンプト `Read .claude/agents/kojo-writer.md. 289 K1...` が問題:
- `Read .claude/agents/kojo-writer.md` は正しい (subagent が自己定義を読む方法)
- 問題は `{ID} K{N}` が actionable でないこと
- kojo-writer.md 内の指示 (MUST edit, See Skill) が無視された
- 明示的な `FIRST ACTION` で Skill 呼び出しを強制する

### 問題の系譜

| Feature | 問題 | 対策 | 結果 |
|:-------:|------|------|:----:|
| F202 | kojo-writer.md 修正 | 複数修正 | ✅ |
| F283 | stub検出・ステータス出力改善 | 空stub判定追加 | ✅ |
| F293 | F288 問題の修正 | MUST edit 追加 | ✅ (ドキュメント修正済み) |
| **F302** | dispatch プロンプト問題 | 明示的実行指示追加 | **本 Feature** |

**結論**: F293 はドキュメント修正として正しい。しかし dispatch プロンプトが「読む」指示のみのため、エージェントが実行しない。dispatch プロンプト自体の改善が必要。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 18:40 | START | implementer | Task 1 | - |
| 2026-01-01 18:40 | END | implementer | Task 1 | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- [kojo-writer.md](../../.claude/agents/kojo-writer.md)
- [do.md](../../.claude/commands/do.md)
- Blocking: [feature-289.md](feature-289.md) - COM_83 口上
- **Prior fix attempts** (調査対象):
  - [feature-202.md](feature-202.md) - kojo-writer.md 修正 (F175/191/200/201統合)
  - [feature-283.md](feature-283.md) - stub検出・ステータス出力改善
  - [feature-293.md](feature-293.md) - kojo-writer 実装確認強化 (同一問題の修正、再発)
- Related: [feature-288.md](feature-288.md) - F293 の発端となった問題発生 Feature
