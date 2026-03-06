# Feature 235: Plan Command Version Auto-Select

## Status: [DONE]

## Type: infra

## Keywords (検索用)

`plan`, `version`, `auto-select`, `feature-breakdown`, `spec-writer`, `infra`

---

## Background

### Philosophy (Mid-term Vision)

全スラッシュコマンドが一貫した構造パターンを持つ: 明確な Phase 定義、明示的な subagent 契約、エラーハンドリング、Recovery Procedures。これにより予測可能な実行と保守性の向上を実現する。

### Problem (Current Issue)

/plan コマンドに以下の機能不足が存在 (F233 Issue Inventory より):

| ID | 問題 | 現状 | リスク |
|:--:|------|------|--------|
| PN1 | Version 自動選択なし | 毎回 `v{X.Y}` を指定する必要 | 操作煩雑 |
| PN2 | Feature Breakdown 形式未定義 | spec-writer の出力形式が不統一 | /next 連携失敗 |

**Gap Analysis for PN2**:
- plan.md has "Feature Granularity Guide" section which defines CRITERIA (what to split)
- MISSING: FORMAT SCHEMA in plan.md that spec-writer follows when outputting to designs/*.md:
  - Required columns for Feature Breakdown table (Feature#, Type, Name, AC Count)
  - Status field values that trigger /next processing
  - Linkage between design Feature# and index-features.md "next Feature number"

### Goal (What to Achieve)

/plan コマンドの使いやすさを向上:
1. 引数なしで次の未着手 Version を自動選択
2. spec-writer の Feature Breakdown 出力形式を定義し、/next との連携を確実化

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Version Auto-Select section | code | Grep | contains | "Version Auto-Select" | [x] |
| 2 | Feature Breakdown Format Schema | code | Grep | matches | "Feature#.*Type.*Name.*AC Count" | [x] |
| 3 | spec-writer format reference | code | Grep | contains | "See plan.md" | [x] |
| 4 | ビルド成功 | build | dotnet | succeeds | - | [x] |

### AC Details

- AC#1: plan.md に "Version Auto-Select" セクション追加。以下を定義:
  - Auto-select algorithm: content-roadmap.md Version Roadmap の `Status: -` (unstarted) を検出
  - Selection criteria: 最初の未着手 Version を選択
  - Fallback: すべて完了なら "all complete" 報告
  - Test: Grep `"Version Auto-Select"` in plan.md to verify section exists
- AC#2: plan.md に "## Feature Breakdown Output Format" セクション追加。スキーマ定義:
  - Required columns: Feature#, Type, Name, AC Count
  - APPROVED status trigger for /next processing
  - Test: Grep regex `"Feature#.*Type.*Name.*AC Count"` to verify table header structure
- AC#3: spec-writer.md の「Feature分割案の記載」セクションに SSOT 参照を追加:
  - 既存の口上/システム分割基準はそのまま維持
  - "See plan.md ### Feature Breakdown Output Format" の参照を追加
  - 出力時に必須カラム (Feature#, Type, Name, AC Count) を含むこと
  - Test: Grep `"See plan.md"` in spec-writer.md to verify SSOT reference
- AC#4: `dotnet build` 成功

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add "### Version Auto-Select" section to plan.md with algorithm definition | [x] |
| 2 | 2 | Add "### Feature Breakdown Output Format" section to plan.md with schema | [x] |
| 3 | 3 | Add SSOT reference to spec-writer.md Feature分割案 section (plan.md format, required columns) | [x] |
| 4 | 4 | Verify build succeeds | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | Initialization | initializer | Status PROPOSED→WIP | READY |
| 2025-12-27 | Investigation | Explore | Discovered ACs already satisfied by F234 | READY |
| 2025-12-27 | AC Verification | manual | All 4 ACs pass (Grep + Build) | PASS:4/4 |
| 2025-12-27 | Post-Review | feature-reviewer | Mode: post, philosophy alignment check | READY |
| 2025-12-27 | Finalization | finalizer | Status WIP→DONE, pre-check pass (no new failures) | READY_TO_COMMIT |

---

## Dependencies

- Parent: F233 (/plan-/next Workflow Audit)
- Blocking: F234 (Plan Command Phase Restructure)
  - Reason: F234 restructures plan.md Phase definitions. F235 adds new sections within this structure.
  - Coordination: F235 should be implemented AFTER F234's Phase restructure is complete to avoid rework.

---

## Links

- [F233](feature-233.md) - Parent audit feature
- [/plan command](../../.claude/commands/plan.md)
- [spec-writer agent](../../.claude/agents/spec-writer.md)
