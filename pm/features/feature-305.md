# Feature 305: F289 実行時の問題分析と改善

## Status: [DONE]

## Type: infra

## Background

### Problem
F289 (COM_83 kojo) 実行時に複数の問題が発生した。根本原因を分析し、ワークフロー改善が必要。

### Context
- F289 で kojo-writer を 10 並列 dispatch
- 最初の dispatch が失敗（エージェントが kojo-writer.md を読んで要約しただけ）
- resume で回復したが、プロセスに複数の問題があった

### Goal
do.md workflow を改善し F289 型の dispatch 失敗を防止する：
1. OK/NG dispatch 例の明示
2. polling での早期失敗検出
3. 構造化された failure investigation 手順

---

## 議論ポイント

### 1. Dispatch Format の誤り（部分的に F304 で対応済み）

**発生した問題**:
- do.md に `{ID} K{N} COM={X} Cache:{path}` と明記されていた
- 実際の dispatch: `"Read .claude/agents/kojo-writer.md. 289 K1"`
- `**EXACT format. NO additions. NO exceptions.**` と強く書いてあったのに無視

**原因分析**:
- 他の agent (initializer) の dispatch パターン `"Read .claude/agents/{agent}.md..."` を流用
- do.md の指示より自分の判断を優先

**F304 での対応**:
- kojo-writer.md に Workflow と Input format 明記
- kojo-writer 側で INVALID_FORMAT validation 追加
- do.md format を `{ID} K{N}` に簡略化

**未解決の問題**:
- do.md に書いてあっても違うことをする LLM の特性
- Notes がテーブルの Format 列から離れている
- 実際の dispatch 例（コピペ可能）がない

**検討中の改善案**:
- OK/NG 例をテーブル直下に配置
- dispatch 例をコピペ可能な形式で提供
- 全 agent で dispatch format を統一

---

### 2. ポーリング時の早期状態確認

**発生した問題**:
- 6分（初回 delay）経過後、status file が 0/10
- その後 60 秒ごとに polling を繰り返した（18 回以上）
- エージェントが実際に動作しているか確認しなかった

**あるべき動作**:
- 1回目の polling（6分後）で 0 件なら、すぐにエージェント状態を確認
- TaskOutput (block=false) で 1-2 個のエージェントの進捗確認
- 問題があれば早期に検出・対応

**改善案**:
- do.md の polling ロジックに追加:
  ```
  If count == 0 after initial delay:
    Check 1-2 agents with TaskOutput(block=false)
    If no progress (0 tools used): STOP and investigate
  ```

---

### 3. リカバリー手順を守らなかった

**発生した問題**:
- 失敗を検出後、すぐに全 10 エージェントを resume
- **原因を十分に分析せずに**再実行
- do.md の Failure Recovery を機械的に適用

**あるべき動作**:
1. まず 1 つのエージェントで原因を調査
2. 原因を特定・報告
3. 修正方法をユーザーと確認
4. 他のエージェントに適用

**根本原因**:
- do.md の Recovery フローを「チェックリスト」として扱った
- 各ステップの**目的**を考えなかった
- 「まず調べる」より「早く解決する」を優先

**改善案**:
- do.md の Failure Recovery セクションに明記:
  ```
  1. Investigate ONE agent first (TaskOutput or resume with investigation prompt)
  2. Identify root cause
  3. Report to user
  4. Apply fix to remaining agents
  ```

---

### 4. Resume で確認する内容

**発生した問題**:
- resume 時に `"Invoke Skill(kojo-writing) first. Then implement."` と指示
- これで動作はしたが、**なぜ最初の dispatch が失敗したか**の原因分析ではなかった

**あるべき動作**:
- resume 時にまず原因を確認:
  ```
  "Why did you not invoke Skill(kojo-writing)?
   What did you interpret the prompt to mean?"
  ```
- 原因を理解してから修正方法を決定

**改善案**:
- do.md に「原因調査用 resume prompt」を追加:
  ```markdown
  ## Failure Investigation (before Recovery)

  Resume ONE failed agent with:
  "Explain why you did not complete the task.
   What was your interpretation of the prompt?"

  After understanding the cause, proceed to Recovery.
  ```

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | do.md に OK/NG dispatch 例追加 | code | contains | "### Dispatch Examples" | [x] |
| 2 | do.md polling に早期状態確認追加 | code | contains | "If count == 0" | [x] |
| 3 | do.md Failure Recovery に調査ステップ追加 | code | contains | "Failure Investigation" | [x] |
| 4 | do.md に原因調査用 resume prompt 追加 | code | contains | "Explain why you did not complete the task" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | dispatch 例セクション追加 | [○] |
| 2 | 2 | polling ロジック修正 | [○] |
| 3 | 3 | Failure Investigation セクション追加 | [○] |
| 4 | 4 | 原因調査用 resume prompt 追加 | [○] |

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 | create | - | F289 議論から派生 | PROPOSED |
| 2026-01-02 | /do | initializer | Feature init | READY |
| 2026-01-02 | /do | explorer | Investigation | All AC already in do.md |
| 2026-01-02 | /do | - | AC verification | 4/4 PASS |
| 2026-01-02 | /do | feature-reviewer | post + doc-check | READY |
| 2026-01-02 | /do | finalizer | Status update | DONE |

## Links

- [index-features.md](index-features.md)
- Related: [feature-289.md](feature-289.md) - 発生源
- Related: [feature-304.md](feature-304.md) - 部分対応（kojo-writer Workflow）
