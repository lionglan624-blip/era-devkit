# Feature 233: /plan-/next Workflow Audit

## Status: [WIP]

## Type: infra

## Keywords (検索用)

`plan`, `next`, `workflow`, `audit`, `roadmap`, `design`, `SSOT`, `phase`, `subagent`, `infra`

---

## Summary

/plan コマンドの論理性を /do と同等に引き上げ、/next との連携を整理する統括 Feature。
問題を分類・整理し、派生 Feature を起票する。

---

## Background

### Philosophy (Mid-term Vision)

全スラッシュコマンドが一貫した構造パターンを持つ: 明確な Phase 定義、明示的な subagent 契約、エラーハンドリング、Recovery Procedures。これにより予測可能な実行と保守性の向上を実現する。また、新規コマンド作成時のテンプレートとして機能し、開発速度が向上する。

### Session Context

- **Origin**: Following completion of F223 (/do Workflow Comprehensive Audit), similar structural review needed for /plan command
- **Pattern**: Same meta-audit approach as F223 (Issue Inventory → Derived Features → track to completion). Unlike F223 (internal /do workflow), F233 focuses on /plan-/next integration and SSOT
- **Why combined audit**: /plan and /next share roadmap/design dependencies, making joint audit more efficient
- **Scope decision**: Focus on workflow structure (not content/feature quality)

### Problem (Current Issue)

/do コマンドと比較して /plan コマンドに構造的欠陥が存在。また /plan と /next の連携、roadmap と designs の SSOT が不明確。

| 比較項目 | /do | /plan |
|----------|:---:|:-----:|
| Phase 定義 | 9 Phase 明確 | Phase/Step 混在 |
| Subagent 戻り値 | 定義あり | なし |
| Error Handling | あり | なし |
| Recovery Procedures | R1-R5 (複雑な実行workflow用) | なし |
| TodoWrite 統合 | 必須 | 言及なし |
| Approval Gate | 明示的 | 暗黙的 |

### Goal (What to Achieve)

1. 全ての問題をリスト化（Issue Inventory）
2. 派生 Feature を起票
3. 派生 Feature 完了で本 Feature を DONE

**Completion Criteria**:
- **AC completion** ([PROPOSED] → [WIP]): Issue Inventory + Derived Features documented in this file
- **Feature completion** ([WIP] → [DONE]): All derived Features reach [DONE] status (requires manual tracking via Derived Features table Status column)

**Note**: This is a meta-audit feature. Implementation work is documentation only (no code changes). Derived feature files are created via /next command (which dispatches subagents) after ACs pass, not by this feature's implementer agent directly.

---

## Issue Inventory

**Status tracking**: Issue Status column marks `[x]` when the corresponding derived Feature reaches [DONE]. Derived Features table tracks feature progress through workflow.

### Category 1: /plan 構造的欠陥

| ID | 問題 | 現状 | リスク | Status |
|:--:|------|------|--------|:------:|
| PL1 | Phase/Step 混在 | Phase 1-3 と Step 1-4 が混在 | 実行順序不明確 | [ ] |
| PL2 | Subagent 戻り値未定義 | goal-setter 等の成功/失敗条件が不明 | エラー時の対応不明 | [ ] |
| PL3 | Error Handling なし | エラー時の対応セクションなし | 失敗時に継続リスク | [ ] |
| PL4 | Recovery Procedures なし | 中断からの復帰手順なし | 再開不可 | [ ] |
| PL5 | TodoWrite 統合なし | 進捗追跡の仕組みなし | 進捗不可視 | [ ] |
| PL6 | Approval Gate 暗黙的 | DRAFT→APPROVED の承認プロセス不明確 | 未承認で進行リスク | [ ] |
| PL7 | Feature Granularity Guide 内容重複 | plan.md と feature-template.md で同内容重複、変更時に同期必要 | 保守性低下 | [ ] |

### Category 2: /plan 機能不足

| ID | 問題 | 現状 | リスク | Status |
|:--:|------|------|--------|:------:|
| PN1 | Version 自動選択なし | 毎回 `v{X.Y}` を指定する必要 | 操作煩雑 | [ ] |
| PN2 | Feature Breakdown 形式未定義 | spec-writer の出力形式が不統一 | /next 連携失敗 | [ ] |

### Category 3: SSOT・連携問題

| ID | 問題 | 現状 | リスク | Status |
|:--:|------|------|--------|:------:|
| DS1 | 1 Version : N 機能問題 | v2.x-v5.x が1 Designに混在 | 粒度不整合 | [ ] |
| DS2 | Design index 重複 | designs/README.md が content-roadmap.md と Version/Description 重複 | 同期漏れ | [ ] |
| DS3 | roadmap → Design マッピング非構造 | 自由文埋め込み、Version Roadmap テーブルに Design 列なし | 参照ミス | [ ] |
| DS4 | /next が roadmap を見ない | 未着手 Version の自動検出不可 | 手動管理必要 | [ ] |
| DS5 | Architecture Document 未分離 | 上位設計と個別設計が混在 | 責務不明確 | [ ] |

---

## Derived Features

| ID | Name | Scope Issues | Status |
|:--:|------|--------------|:------:|
| 234 | Plan Command Phase Restructure | PL1-PL7 (all Category 1) | [PROPOSED] |
| 235 | Plan Command Version Auto-Select | PN1, PN2 (all Category 2) | [PROPOSED] |
| 236 | Roadmap-Design SSOT | DS2, DS3, DS4 | [PROPOSED] |
| 237 | Design Granularity Standard | DS1, DS5 | [PROPOSED] |

**Note**: Feature IDs 234-237 assigned.

**Issue coverage verified**: All 14 issues mapped to exactly 4 derived features:
- Plan Command Phase Restructure: PL1, PL2, PL3, PL4, PL5, PL6, PL7 (7 issues)
- Plan Command Version Auto-Select: PN1, PN2 (2 issues)
- Roadmap-Design SSOT: DS2, DS3, DS4 (3 issues)
- Design Granularity Standard: DS1, DS5 (2 issues)

**Grouping rationale**:
- Roadmap-Design SSOT: DS2,DS3,DS4 are roadmap-design workflow issues (index duplication, mapping structure, integration)
- Design Granularity Standard: DS1,DS5 are design document scope issues (DS1=version scope per file, DS5=content hierarchy within files)

## Implementation Plan

1. **Step 1: AC Verification**: Verify Issue Inventory (14 issues) and Derived Features (4 planned) tables complete in this file
2. **Step 2: Feature Creation** (manual): After ACs pass, run /next 4 times to create feature-{ID}.md for each derived feature
3. **Step 3: Feature Tracking** (ongoing): Update Derived Features table Status column as each derived feature progresses
4. **Step 4: Completion**: When all 4 derived features reach [DONE], update F233 Status to [DONE]

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Issue Inventory complete | code | Grep | count_equals | 14 | [x] |
| 2 | Derived Features planned | code | Grep | count_equals | 4 | [x] |
| 3 | ビルド成功 | build | dotnet | succeeds | - | [x] |

**AC Details**:
- AC#1: Grep pattern `^\| (PL|PN|DS)[0-9]+ \|` (line-start anchored) in feature-233.md, count = 14
- AC#2: Grep pattern `\| .* \| .* \| \[PROPOSED\] \|` in Derived Features table (3-column format), count = 4

**Note**: ACs verify documentation in this feature file only. Feature completion (Status: [DONE]) requires manual verification that all derived Features reach [DONE] status. This manual tracking is recorded in the Derived Features table Status column.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Investigate and document /plan workflow issues in Issue Inventory | [O] |
| 2 | 2 | Plan and document derived Features based on issue categories | [O] |
| 3 | 3 | Verify build succeeds | [O] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-27 | Phase 1-4 | initializer, explorer | Init, Investigate, Verify | READY |
| 2025-12-27 | Phase 6 | /do | AC verification | PASS:3/3 |
| 2025-12-27 | Phase 7 | feature-reviewer | Post-review | READY |
| 2025-12-27 | Phase 9 | finalizer | Build check + log entry | READY_TO_COMMIT |

---

## Reference Information (事前調査メモ)

<!-- Note: Issue Inventory above is SSOT. This section preserved for design proposals only. -->

**Disclaimer**: The design proposals below are DRAFT references for derived Features. They are NOT binding. Derived feature implementers may refine or revise these approaches based on further investigation. Note: The `designs/` directory referenced below will be created as part of implementing the derived features.

### Design 命名規則・構成 (Option C: roadmap SSOT方式)

**命名規則**: `v{X.Y}-{name}.md`

**ディレクトリ構造**:

```
Game/agents/
├── content-roadmap.md          ← SSOT: Version → Design マッピング
├── index-features.md           ← 実行管理
│
├── designs/                    ← 実装用Design（1 Design = 1 Version）
│   ├── v0.7-kojo-60-90.md
│   ├── v1.7-new-commands.md
│   ├── v1.8-m-orgasm.md
│   ├── v2.0-opp-system.md      ← ntr-core から抽出
│   ├── v2.0-netorase.md        ← 同一verで複数Design OK
│   ├── v2.1-ntr-params.md      ← ntr-core から抽出
│   ├── v2.6-chastity-belt.md
│   ├── v2.7-pregnancy.md       ← 分割 (v2.7のみ)
│   ├── v3.0-phase-management.md
│   └── README.md               ← 廃止 or roadmap から自動生成
│
└── reference/                  ← 参照用ドキュメント（非実行）
    ├── ntr-core-overview.md    ← 旧 ntr-core-system.md（全体像のみ）
    ├── com-map.md
    ├── feature-template.md
    ├── kojo-phases.md
    └── ...
```

**SSOT 対応関係**:

```
roadmap.md (SSOT: Version一覧 + Design列)
    │
    │ Version Roadmap テーブルで Design を直接リンク
    │
    ▼
designs/v{X.Y}-{name}.md
    │
    │ frontmatter: version, status, features[]
    │
    ▼
/next が APPROVED を検出 → feature-{ID}.md 作成
    │
    ▼
index-features.md
```

**designs/ と reference/ の違い**:

| フォルダ | 目的 | /plan が参照 | /next が参照 |
|----------|------|:------------:|:------------:|
| designs/ | 実装用Design（DRAFT→APPROVED→Feature化） | ✅ | ✅ |
| reference/ | 参照ドキュメント（概要、仕様、マップ） | ❌ | ❌ |

**ルール**:

| 要素 | 推奨 |
|------|------|
| 命名規則 | `v{X.Y}-{name}.md` |
| 参照ドキュメント | `reference/` フォルダに分離 |
| SSOT | roadmap.md に Design 列追加 |
| README.md | 廃止 or roadmap から自動生成 |
| 1 Design : 1 Version | 原則維持（同一verで複数Design OK） |

**既存ファイルの移行案**:

| 現在 | 移行先 | アクション |
|------|--------|------------|
| ntr-core-system.md | reference/ntr-core-overview.md | 移動 + 実装詳細を削除 |
| phase-system.md | designs/v3.0-phase-management.md | リネーム |
| new-commands.md | designs/v1.7-new-commands.md | リネーム |
| m-orgasm-system.md | designs/v1.8-m-orgasm.md | リネーム |
| netorase-system.md | designs/v2.0-netorase.md | リネーム |
| chastity-belt-system.md | designs/v2.6-chastity-belt.md | リネーム |
| pregnancy-system.md | designs/v2.7-pregnancy.md | リネーム + v2.8-2.9を分割 |
| reconciliation-system.md | designs/v2.5-reconciliation.md | リネーム |

**roadmap.md の Version Roadmap テーブル（改）**:

```markdown
| Version | Design | Type | Status | Description |
|:-------:|--------|:----:|:------:|-------------|
| v0.7 | [v0.7-kojo-60-90.md](designs/v0.7-kojo-60-90.md) | kojo | DRAFT | 60-90系口上 |
| v1.7 | [v1.7-new-commands.md](designs/v1.7-new-commands.md) | erb | - | 新コマンド +37 |
| v1.8 | [v1.8-m-orgasm.md](designs/v1.8-m-orgasm.md) | erb | - | M絶頂システム |
| v2.0 | [v2.0-opp-system.md](designs/v2.0-opp-system.md), [v2.0-netorase.md](designs/v2.0-netorase.md) | engine | - | OPP + 寝取らせ |
| v2.1 | [v2.1-ntr-params.md](designs/v2.1-ntr-params.md) | engine | - | FAM/DEP分離 |
| v2.6 | [v2.6-chastity-belt.md](designs/v2.6-chastity-belt.md) | erb | - | 貞操帯 |
| v2.7 | [v2.7-pregnancy.md](designs/v2.7-pregnancy.md) | erb | - | 妊娠 |
| v3.0 | [v3.0-phase-management.md](designs/v3.0-phase-management.md) | engine | - | Phase管理 |
```

---

## Dependencies

- F223 (/do Workflow Comprehensive Audit): 同様のアプローチを採用
- No blocking dependencies: Can proceed in parallel with other infra features

---

## Links

- [/plan command](../../.claude/commands/plan.md)
- [/next command](../../.claude/commands/next.md)
- [/do command](../../.claude/commands/do.md)
- [F223](feature-223.md) - 参考: /do Audit Feature
