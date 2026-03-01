# Feature 236: Roadmap-Design SSOT

## Status: [DONE]

## Type: infra

## Keywords (検索用)

`roadmap`, `design`, `ssot`, `next`, `mapping`, `index`, `infra`

---

## Background

### Philosophy (Mid-term Vision)

全スラチE��ュコマンドが一貫した構造パターンを持つ: 明確な Phase 定義、�E示皁E�� subagent 契紁E��エラーハンドリング、Recovery Procedures。これにより予測可能な実行と保守性の向上を実現する。また、ドキュメント間の SSOT (Single Source of Truth) を確立し、情報の重褁E��同期漏れを防止する、E

### Problem (Current Issue)

SSOT・連携に以下�E問題が存在 (F233 Issue Inventory より):

| ID | 問顁E| 現状 | リスク |
|:--:|------|------|--------|
| DS2 | Design index 重褁E| designs/README.md ぁEcontent-roadmap.md と Version/Description 重褁E| 同期漏れ |
| DS3 | roadmap ↁEDesign マッピング非構造 | 自由斁E��め込み、Version Roadmap チE�Eブルに Design 列なぁE| 参�Eミス |
| DS4 | /next ぁEroadmap を見なぁE| 未着扁EVersion の自動検�E不可 | 手動管琁E��E��E|

### Goal (What to Achieve)

Roadmap と Design の SSOT を確竁E
1. roadmap の "Design Documents" セクションを削除し、designs/README.md めEDesign 詳細の SSOT に
2. roadmap Version チE�Eブルに Design 列を追加し、Version ↁEDesign の構造化�EチE��ングを実現
3. /next ぁEroadmap を参照するよう更新

**Scope Exclusion**: content-roadmap.md と designs/README.md 間�E Version 番号不整合（侁E netorase-system ぁEroadmap では v6.x、designs/README.md では v2.0-v2.1�E��E解消�E本 Feature のスコープ外。既存�E Version 番号はそ�Eまま維持し、SSOT 構造の確立を優先。Version 整合性は別途対応、E

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | roadmap Version チE�Eブルに Design 列あめE| code | Grep | contains | "\| Version \| Content \| System \| Design \| Description \| Status \|" | [x] |
| 2 | roadmap Design Documents セクション削除 | code | Grep | not_contains | "## Design Documents" | [x] |
| 3 | /next に roadmap 参�Eを追加 | code | Grep | matches | "Read.*content-roadmap.md" | [x] |

### AC Details

- AC#1: content-roadmap.md の Version Roadmap チE�Eブルに Design 列を追加。�EチE��衁E "| Version | Content | System | Design | Description | Status |" (6列、System と Description の間に Design 列挿入)
- AC#2: content-roadmap.md から "## Design Documents" セクションを削除。designs/README.md ぁEDesign の SSOT となめE
- AC#3: next.md に `Read pm/content-roadmap.md` を追加し、未着扁EVersion の検�Eを可能に

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
- [content-roadmap.md](../content-roadmap.md)
- [designs/README.md](../designs/README.md)
- [/next command](../../../archive/claude_legacy_20251230/commands/next.md)
