# Feature 237: Design Granularity Standard

## Status: [DONE]

## Type: infra

## Keywords (検索用)

`design`, `granularity`, `version`, `architecture`, `reference`, `infra`

---

## Background

### Philosophy (Mid-term Vision)

全スラチE��ュコマンドが一貫した構造パターンを持つ: 明確な Phase 定義、�E示皁E�� subagent 契紁E��エラーハンドリング、Recovery Procedures。これにより予測可能な実行と保守性の向上を実現する。また、Design ドキュメント�E明確な粒度基準を持ち、E Version = 1 Design の原則に従う、E

### Problem (Current Issue)

Design 粒度に以下�E問題が存在 (F233 Issue Inventory より):

| ID | 問顁E| 現状 | リスク |
|:--:|------|------|--------|
| DS1 | 1 Version : N 機�E問顁E| v2.x-v5.x ぁE Designに混在 (ntr-core-system.md) | 粒度不整吁E|
| DS5 | Architecture Document 未刁E�� | 上位設計と個別設計が混在 | 責務不�E確 |

### Goal (What to Achieve)

Design ドキュメント�E粒度標準を確竁E
1. 1 Design = 1 Version の原則を文書匁E
2. designs/ と reference/ の役割刁E��を�E確匁E
3. designs/ntr-core-system.md めEreference/ntr-core-overview.md として移動！Eersion Roadmap セクションのみ削除、他�Eセクションは保持�E�E

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | 粒度基準文書匁E| code | Grep | contains | "1 Design = 1 Version" | [x] |
| 2 | designs/reference 役割定義 | code | Grep | contains | "実裁E��訁E | [x] |
| 3 | ntr-core ↁEreference移勁E| file | Glob | exists | pm/reference/ntr-core-overview.md | [x] |
| 4 | Version詳細削除確誁E| code | Grep | not_contains | "Version Roadmap" | [x] |
| 5 | ビルド�E劁E| build | dotnet | succeeds | - | [x] |

### AC Details

**AC#1 Test**: `Grep pattern="1 Design = 1 Version" path="docs/architecture/README.md"`
**AC#1 Expected**: Pattern found in designs/README.md (file will be updated by Task#1)

**AC#2 Test**: `Grep pattern="実裁E��訁E path="docs/architecture/README.md"`
**AC#2 Expected**: Role distinction pattern found (file will be updated by Task#2)

**AC#3 Test**: `Glob("pm/reference/ntr-core-overview.md")`
**AC#3 Expected**: File exists

**AC#4 Test**: `Grep pattern="Version Roadmap" path="pm/reference/ntr-core-overview.md"`
**AC#4 Expected**: Pattern NOT found (verification that version-specific details were removed)

**AC#5 Test**: `dotnet build`
**AC#5 Expected**: Exit code 0

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Document "1 Design = 1 Version" principle in designs/README.md | [x] |
| 2 | 2 | Define designs/ vs reference/ directory roles in designs/README.md | [x] |
| 3 | 3 | Move designs/ntr-core-system.md to reference/ntr-core-overview.md | [x] |
| 4 | 4 | Remove Version Roadmap section from ntr-core-overview.md | [x] |
| 5 | 5 | Verify build succeeds | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | Phase 1 | initializer | Feature validation | READY |
| 2025-12-27 | Phase 2 | explorer | Code investigation | READY |
| 2025-12-27 | Phase 4 | implementer | Task#1-4 execution | SUCCESS |
| 2025-12-27 | Phase 6 | - | AC verification | 5/5 PASS |
| 2025-12-27 | Phase 7 | feature-reviewer | Post-review | NEEDS_REVISION (broken links) |
| 2025-12-27 | Phase 7 | implementer | Fix broken links | SUCCESS |
| 2025-12-27 | Phase 7 | feature-reviewer | Re-review | READY |
| 2025-12-27 | Phase 8 | finalizer | Status finalization | COMPLETE |

---

## Dependencies

- Parent: F233 (/plan-/next Workflow Audit)
- Related: F236 (Roadmap-Design SSOT) - can be done in parallel

---

## Links

- [F233](feature-233.md) - Parent audit feature
- [ntr-core-overview.md](../reference/ntr-core-overview.md) - NTR core system overview (migrated from designs/)
- [designs/README.md](../designs/README.md)
