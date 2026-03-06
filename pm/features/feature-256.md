# Feature 256: Philosophy Gate for FL

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
FL (/fl) は Feature の品質を保証する最終ゲートとして機能すべき。AC/Task の形式的整合性だけでなく、**Philosophy の意味的達成** も検証する必要がある。

### Problem (Current Issue)
F254 で発生した問題:
- Philosophy: 「Phase 8 Summary を **完全代替** すべき」
- 実装: `--progress` オプション（可視化ツール）のみ
- 結果: Philosophy の一部しか達成していないのに「完了」と判定

**根本原因**:
- feature-reviewer は AC/Task を基準にレビュー
- 現在の AC/Task から逆算すると「これで十分」と錯覚
- Philosophy との乖離を検出する仕組みがない

### Goal (What to Achieve)
FL ループ完了後に **Philosophy Gate** を追加:
1. Philosophy から「導出すべきタスク」を正算（現在の AC/Task に引きずられない）
2. 導出タスクと現在タスクを突き合わせて Gap を検出
3. Opus が Gap に対し ADOPT/DEFER/REJECT を判断
4. ADOPT の場合はループ最初に戻り再検証

### Session Context
- **Origin**: F254 完了時のレビューで Philosophy 未達成を発見
- **Lesson**: AC/Task からの逆算ではなく、Philosophy からの正算が必要

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | philosophy-deriver.md 作成 | file | Glob | exists | .claude/agents/philosophy-deriver.md | [x] |
| 2 | deriver が opus モデル指定 | file | Grep | contains | "model: opus" | [x] |
| 3 | task-comparator.md 作成 | file | Glob | exists | .claude/agents/task-comparator.md | [x] |
| 4 | comparator が haiku モデル指定 | file | Grep | contains | "model: haiku" | [x] |
| 5 | fl.md にセクション 6 追加 | file | Grep | contains | "Philosophy Gate" | [x] |
| 6 | セクション 6 が deriver 呼び出し | file | Grep | contains | "philosophy-deriver" | [x] |
| 7 | セクション 6 が comparator 呼び出し | file | Grep | contains | "task-comparator" | [x] |
| 8 | ADOPT 時 GOTO WHILE 再開 | file | Grep | contains | "GOTO WHILE" | [x] |
| 9 | CLAUDE.md に philosophy-deriver 追加 | file | Grep | contains | "philosophy-deriver" | [x] |
| 10 | CLAUDE.md に task-comparator 追加 | file | Grep | contains | "task-comparator" | [x] |

### AC Details

**AC1-2**: philosophy-deriver.md
- Model: opus（高度な抽象推論が必要）
- 責務: Philosophy → 導出すべきタスクを列挙
- 現在の AC/Task を見ない（バイアス回避）

**AC3-4**: task-comparator.md
- Model: haiku（単純な突き合わせ）
- 責務: 導出タスク vs 現在タスク → Gap 検出

**AC5-8**: fl.md セクション 6 (Philosophy Gate)
- ループ完了後（zero issues 後）に実行
- Features のみ（command/agent/skill/doc はスキップ）
- ADOPT → GOTO WHILE でループ再開
- DEFER → 残課題として記録、FL 完了
- REJECT → Gap は対象外、FL 完了

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | philosophy-deriver.md 作成 | [x] |
| 2 | 3,4 | task-comparator.md 作成 | [x] |
| 3 | 5,6,7,8 | fl.md にセクション 6 追加 | [x] |
| 4 | 9,10 | CLAUDE.md Subagent Strategy に新エージェント追加 | [x] |

<!-- AC:Task Consolidation Rationale:
- Task 1 (AC1+2): Agent file creation includes frontmatter content (single file operation)
- Task 2 (AC3+4): Same rationale as Task 1
- Task 3 (AC5-8): fl.md セクション 6 is a single logical section with interdependent components
-->

---

## Technical Notes

### philosophy-deriver.md 設計

```markdown
---
name: philosophy-deriver
description: Derive required tasks from Philosophy. Model: opus.
model: opus
tools: Read
---

# Philosophy Deriver Agent

## Task
Read Philosophy section and derive what tasks SHOULD exist to achieve it.

## Input
- Feature ID

## Process
1. Read feature-{ID}.md Philosophy section ONLY
2. **CRITICAL**: Do NOT read Goal/AC/Task sections (bias avoidance)
3. Ask: "To fully achieve this Philosophy, what must be done?"
4. List derived tasks with rationale

## Output Format
{
  "philosophy_text": "...",
  "absolute_claims": ["完全", "すべて", etc.],
  "derived_tasks": [
    {"task": "可視化ツール追加", "rationale": "進捗を見えるようにする"},
    {"task": "ワークフロー統合", "rationale": "既存ツールが参照して初めて代替"},
    {"task": "旧ワークフロー廃止確認", "rationale": "完全代替の検証"}
  ]
}
```

### task-comparator.md 設計

```markdown
---
name: task-comparator
description: Compare derived tasks with current tasks. Model: haiku.
model: haiku
tools: Read
---

# Task Comparator Agent

## Task
Compare philosophy-derived tasks with current feature tasks.

## Input
- Feature ID
- Derived tasks (from philosophy-deriver)

## Process
1. Read feature-{ID}.md Tasks section
2. Map each derived task to current task (semantic matching)
3. Identify gaps (derived but not in current)
4. Identify extras (in current but not derived - OK, just note)

## Output Format
{
  "mappings": [
    {"derived": "可視化ツール追加", "current_task": "Task 1-5", "status": "covered"},
    {"derived": "ワークフロー統合", "current_task": null, "status": "gap"},
    {"derived": "旧ワークフロー廃止確認", "current_task": null, "status": "gap"}
  ],
  "gaps": [
    {"task": "ワークフロー統合", "rationale": "..."},
    {"task": "旧ワークフロー廃止確認", "rationale": "..."}
  ],
  "recommendation": "PARTIAL - 2 gaps detected"
}
```

### fl.md セクション 6 設計

```markdown
## 6. Philosophy Gate (Post-loop, Features Only)

**Integration Point**: Insert between Section 5 (Report) and post-loop processing. Runs after WHILE loop exits with zero issues, BEFORE Report generation.

After loop completes with zero issues, run Philosophy Gate:

### Skip Conditions
- target_type != "feature" → Skip
- Feature has no Philosophy section → Skip with WARNING

### 6.1 Derive Tasks from Philosophy

derived = Task(
  subagent_type: "general-purpose",
  model: "opus",
  prompt: `Read .claude/agents/philosophy-deriver.md and execute for Feature {target_id}`
)

### 6.2 Compare with Current Tasks

comparison = Task(
  subagent_type: "general-purpose",
  model: "haiku",
  prompt: `Read .claude/agents/task-comparator.md and execute for Feature {target_id}. Derived tasks: {derived.derived_tasks}`
)

### 6.3 Opus Decision

IF comparison.gaps is empty:
    → FL Complete (Philosophy fully covered)

IF comparison.gaps is not empty:
    Display gaps to Opus (parent):

    FOR each gap in comparison.gaps:
        decision = Opus evaluates:

        | Decision | Criteria | Action |
        |----------|----------|--------|
        | ADOPT | Essential for Philosophy, feasible in this Feature | Add as new AC/Task → GOTO WHILE |
        | DEFER | Essential but too large for this Feature | Add to "残課題" section → Continue |
        | REJECT | Not actually required (deriver overreached) | Skip → Continue |

    IF any ADOPT decisions:
        Apply AC/Task additions
        GOTO WHILE  # Restart loop for re-verification
    ELSE:
        IF any DEFER decisions:
            Append to feature-{ID}.md "残課題" section
        FL Complete

### 6.4 Loop Limit

Philosophy Gate can trigger at most 1 re-loop.
If Phase 6 runs a second time and still has gaps → Force DEFER all remaining gaps.
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 | Feature completion | finalizer | Mark [DONE], all ACs passed | READY_TO_COMMIT |

---

## Links

- [feature-254.md](feature-254.md) - Philosophy 未達成の発端
- [feature-255.md](feature-255.md) - F254 残課題
- [fl.md](../../.claude/commands/fl.md) - 修正対象
