# Feature 236: Roadmap-Design SSOT

## Status: [DONE]

## Type: infra

## Keywords (検索用)

`roadmap`, `design`, `ssot`, `next`, `mapping`, `index`, `infra`

---

## Background

### Philosophy (Mid-term Vision)

全スラッシュコマンドが一貫した構造パターンを持つ: 明確な Phase 定義、明示的な subagent 契約、エラーハンドリング、Recovery Procedures。これにより予測可能な実行と保守性の向上を実現する。また、ドキュメント間の SSOT (Single Source of Truth) を確立し、情報の重複と同期漏れを防止する。

### Problem (Current Issue)

SSOT・連携に以下の問題が存在 (F233 Issue Inventory より):

| ID | 問題 | 現状 | リスク |
|:--:|------|------|--------|
| DS2 | Design index 重複 | designs/README.md が content-roadmap.md と Version/Description 重複 | 同期漏れ |
| DS3 | roadmap → Design マッピング非構造 | 自由文埋め込み、Version Roadmap テーブルに Design 列なし | 参照ミス |
| DS4 | /next が roadmap を見ない | 未着手 Version の自動検出不可 | 手動管理必要 |

### Goal (What to Achieve)

Roadmap と Design の SSOT を確立:
1. roadmap の "Design Documents" セクションを削除し、designs/README.md を Design 詳細の SSOT に
2. roadmap Version テーブルに Design 列を追加し、Version → Design の構造化マッピングを実現
3. /next が roadmap を参照するよう更新

**Scope Exclusion**: content-roadmap.md と designs/README.md 間の Version 番号不整合（例: netorase-system が roadmap では v6.x、designs/README.md では v2.0-v2.1）の解消は本 Feature のスコープ外。既存の Version 番号はそのまま維持し、SSOT 構造の確立を優先。Version 整合性は別途対応。

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | roadmap Version テーブルに Design 列あり | code | Grep | contains | "\| Version \| Content \| System \| Design \| Description \| Status \|" | [x] |
| 2 | roadmap Design Documents セクション削除 | code | Grep | not_contains | "## Design Documents" | [x] |
| 3 | /next に roadmap 参照を追加 | code | Grep | matches | "Read.*content-roadmap.md" | [x] |

### AC Details

- AC#1: content-roadmap.md の Version Roadmap テーブルに Design 列を追加。ヘッダ行: "| Version | Content | System | Design | Description | Status |" (6列、System と Description の間に Design 列挿入)
- AC#2: content-roadmap.md から "## Design Documents" セクションを削除。designs/README.md が Design の SSOT となる
- AC#3: next.md に `Read Game/agents/content-roadmap.md` を追加し、未着手 Version の検出を可能に

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add Design column (between System and Description) to roadmap Version table. Extract design links from existing Design Documents section BEFORE Task#2 deletes it. For versions with designs, use markdown links. For versions without designs, use '-' | [x] |
| 2 | 2 | Remove "## Design Documents" section from content-roadmap.md (designs/README.md is SSOT) | [x] |
| 3 | 3 | Add content-roadmap.md reading step to next.md Step 2 Priority 2 (Features) section for Version auto-detection from roadmap | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27T00:00:00Z | Phase 1 Init | initializer | Status [PROPOSED]→[WIP], validate feature | READY |
| 2025-12-27T00:01:00Z | Phase 2 | explorer | Investigated files, structure | READY |
| 2025-12-27T00:02:00Z | Phase 4 | implementer | Task 1-3 implemented | SUCCESS |
| 2025-12-27T00:03:00Z | Phase 6 | - | AC 1-3 verified via Grep | PASS:3/3 |
| 2025-12-27T00:04:00Z | Phase 7 | feature-reviewer | Post-review | READY |
| 2025-12-27T23:59:59Z | Phase 8 | finalizer | Status [WIP]→[DONE], verify completion | READY_TO_COMMIT |

---

## Dependencies

- Parent: F233 (/plan-/next Workflow Audit)
- Related: F237 (Design Granularity Standard) - can be done in parallel

---

## Links

- [F233](feature-233.md) - Parent audit feature
- [content-roadmap.md](content-roadmap.md)
- [designs/README.md](designs/README.md)
- [/next command](../../.claude/commands/next.md)
