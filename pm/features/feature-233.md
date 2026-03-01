# Feature 233: /plan-/next Workflow Audit

## Status: [WIP]

## Type: infra

## Keywords (検索用)

`plan`, `next`, `workflow`, `audit`, `roadmap`, `design`, `SSOT`, `phase`, `subagent`, `infra`

---

## Summary

/plan コマンド�E論理性めE/do と同等に引き上げ、Enext との連携を整琁E��る統括 Feature、E
問題を刁E���E整琁E��、派甁EFeature を起票する、E

---

## Background

### Philosophy (Mid-term Vision)

全スラチE��ュコマンドが一貫した構造パターンを持つ: 明確な Phase 定義、�E示皁E�� subagent 契紁E��エラーハンドリング、Recovery Procedures。これにより予測可能な実行と保守性の向上を実現する。また、新規コマンド作�E時�EチE��プレートとして機�Eし、E��発速度が向上する、E

### Session Context

- **Origin**: Following completion of F223 (/do Workflow Comprehensive Audit), similar structural review needed for /plan command
- **Pattern**: Same meta-audit approach as F223 (Issue Inventory ↁEDerived Features ↁEtrack to completion). Unlike F223 (internal /do workflow), F233 focuses on /plan-/next integration and SSOT
- **Why combined audit**: /plan and /next share roadmap/design dependencies, making joint audit more efficient
- **Scope decision**: Focus on workflow structure (not content/feature quality)

### Problem (Current Issue)

/do コマンドと比輁E��て /plan コマンドに構造皁E��陥が存在。まぁE/plan と /next の連携、roadmap と designs の SSOT が不�E確、E

| 比輁E��E�� | /do | /plan |
|----------|:---:|:-----:|
| Phase 定義 | 9 Phase 明確 | Phase/Step 混在 |
| Subagent 戻り値 | 定義あり | なぁE|
| Error Handling | あり | なぁE|
| Recovery Procedures | R1-R5 (褁E��な実行workflow用) | なぁE|
| TodoWrite 統吁E| 忁E��E| 言及なぁE|
| Approval Gate | 明示皁E| 暗黙的 |

### Goal (What to Achieve)

1. 全ての問題をリスト化�E�Essue Inventory�E�E
2. 派甁EFeature を起票
3. 派甁EFeature 完亁E��本 Feature めEDONE

**Completion Criteria**:
- **AC completion** ([PROPOSED] ↁE[WIP]): Issue Inventory + Derived Features documented in this file
- **Feature completion** ([WIP] ↁE[DONE]): All derived Features reach [DONE] status (requires manual tracking via Derived Features table Status column)

**Note**: This is a meta-audit feature. Implementation work is documentation only (no code changes). Derived feature files are created via /next command (which dispatches subagents) after ACs pass, not by this feature's implementer agent directly.

---

## Issue Inventory

**Status tracking**: Issue Status column marks `[x]` when the corresponding derived Feature reaches [DONE]. Derived Features table tracks feature progress through workflow.

### Category 1: /plan 構造皁E��陥

| ID | 問顁E| 現状 | リスク | Status |
|:--:|------|------|--------|:------:|
| PL1 | Phase/Step 混在 | Phase 1-3 と Step 1-4 が混在 | 実行頁E��不�E確 | [ ] |
| PL2 | Subagent 戻り値未定義 | goal-setter 等�E成功/失敗条件が不�E | エラー時�E対応不�E | [ ] |
| PL3 | Error Handling なぁE| エラー時�E対応セクションなぁE| 失敗時に継続リスク | [ ] |
| PL4 | Recovery Procedures なぁE| 中断からの復帰手頁E��ぁE| 再開不可 | [ ] |
| PL5 | TodoWrite 統合なぁE| 進捗追跡の仕絁E��なぁE| 進捗不可要E| [ ] |
| PL6 | Approval Gate 暗黙的 | DRAFT→APPROVED の承認�Eロセス不�E確 | 未承認で進行リスク | [ ] |
| PL7 | Feature Granularity Guide 冁E��重褁E| plan.md と feature-template.md で同�E容重褁E��変更時に同期忁E��E| 保守性低丁E| [ ] |

### Category 2: /plan 機�E不足

| ID | 問顁E| 現状 | リスク | Status |
|:--:|------|------|--------|:------:|
| PN1 | Version 自動選択なぁE| 毎回 `v{X.Y}` を指定する忁E��E| 操作�E雁E| [ ] |
| PN2 | Feature Breakdown 形式未定義 | spec-writer の出力形式が不統一 | /next 連携失敁E| [ ] |

### Category 3: SSOT・連携問顁E

| ID | 問顁E| 現状 | リスク | Status |
|:--:|------|------|--------|:------:|
| DS1 | 1 Version : N 機�E問顁E| v2.x-v5.x ぁE Designに混在 | 粒度不整吁E| [ ] |
| DS2 | Design index 重褁E| designs/README.md ぁEcontent-roadmap.md と Version/Description 重褁E| 同期漏れ | [ ] |
| DS3 | roadmap ↁEDesign マッピング非構造 | 自由斁E��め込み、Version Roadmap チE�Eブルに Design 列なぁE| 参�Eミス | [ ] |
| DS4 | /next ぁEroadmap を見なぁE| 未着扁EVersion の自動検�E不可 | 手動管琁E��E��E| [ ] |
| DS5 | Architecture Document 未刁E�� | 上位設計と個別設計が混在 | 責務不�E確 | [ ] |

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
| 3 | ビルド�E劁E| build | dotnet | succeeds | - | [x] |

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

### Design 命名規則・構�E (Option C: roadmap SSOT方弁E

**命名規則**: `v{X.Y}-{name}.md`

**チE��レクトリ構造**:

```
pm/
├── content-roadmap.md          ↁESSOT: Version ↁEDesign マッピング
├── index-features.md           ↁE実行管琁E
━E
├── designs/                    ↁE実裁E��Design�E�E Design = 1 Version�E�E
━E  ├── v0.7-kojo-60-90.md
━E  ├── v1.7-new-commands.md
━E  ├── v1.8-m-orgasm.md
━E  ├── v2.0-opp-system.md      ↁEntr-core から抽出
━E  ├── v2.0-netorase.md        ↁE同一verで褁E��Design OK
━E  ├── v2.1-ntr-params.md      ↁEntr-core から抽出
━E  ├── v2.6-chastity-belt.md
━E  ├── v2.7-pregnancy.md       ↁE刁E�� (v2.7のみ)
━E  ├── v3.0-phase-management.md
━E  └── README.md               ↁE廁E�� or roadmap から自動生戁E
━E
└── reference/                  ↁE参�E用ドキュメント（非実行！E
    ├── ntr-core-overview.md    ↁE旧 ntr-core-system.md�E��E体像のみ�E�E
    ├── com-map.md
    ├── feature-template.md
    ├── kojo-phases.md
    └── ...
```

**SSOT 対応関俁E*:

```
roadmap.md (SSOT: Version一覧 + Design刁E
    ━E
    ━EVersion Roadmap チE�Eブルで Design を直接リンク
    ━E
    ▼
designs/v{X.Y}-{name}.md
    ━E
    ━Efrontmatter: version, status, features[]
    ━E
    ▼
/next ぁEAPPROVED を検�E ↁEfeature-{ID}.md 作�E
    ━E
    ▼
index-features.md
```

**designs/ と reference/ の違い**:

| フォルダ | 目皁E| /plan が参照 | /next が参照 |
|----------|------|:------------:|:------------:|
| designs/ | 実裁E��Design�E�ERAFT→APPROVED→Feature化！E| ✁E| ✁E|
| reference/ | 参�Eドキュメント（概要、仕様、�EチE�E�E�E| ❁E| ❁E|

**ルール**:

| 要素 | 推奨 |
|------|------|
| 命名規則 | `v{X.Y}-{name}.md` |
| 参�EドキュメンチE| `reference/` フォルダに刁E�� |
| SSOT | roadmap.md に Design 列追加 |
| README.md | 廁E�� or roadmap から自動生戁E|
| 1 Design : 1 Version | 原則維持E��同一verで褁E��Design OK�E�E|

**既存ファイルの移行桁E*:

| 現在 | 移行�E | アクション |
|------|--------|------------|
| ntr-core-system.md | reference/ntr-core-overview.md | 移勁E+ 実裁E��細を削除 |
| phase-system.md | designs/v3.0-phase-management.md | リネ�Eム |
| new-commands.md | designs/v1.7-new-commands.md | リネ�Eム |
| m-orgasm-system.md | designs/v1.8-m-orgasm.md | リネ�Eム |
| netorase-system.md | designs/v2.0-netorase.md | リネ�Eム |
| chastity-belt-system.md | designs/v2.6-chastity-belt.md | リネ�Eム |
| pregnancy-system.md | designs/v2.7-pregnancy.md | リネ�Eム + v2.8-2.9を�E割 |
| reconciliation-system.md | designs/v2.5-reconciliation.md | リネ�Eム |

**roadmap.md の Version Roadmap チE�Eブル�E�改�E�E*:

```markdown
| Version | Design | Type | Status | Description |
|:-------:|--------|:----:|:------:|-------------|
| v0.7 | [v0.7-kojo-60-90.md](../designs/v0.7-kojo-60-90.md) | kojo | DRAFT | 60-90系口丁E|
| v1.7 | [v1.7-new-commands.md](../designs/v1.7-new-commands.md) | erb | - | 新コマンチE+37 |
| v1.8 | [v1.8-m-orgasm.md](../designs/v1.8-m-orgasm.md) | erb | - | M絶頂シスチE�� |
| v2.0 | [v2.0-opp-system.md](../designs/v2.0-opp-system.md), [v2.0-netorase.md](../designs/v2.0-netorase.md) | engine | - | OPP + 寝取らせ |
| v2.1 | [v2.1-ntr-params.md](../designs/v2.1-ntr-params.md) | engine | - | FAM/DEP刁E�� |
| v2.6 | [v2.6-chastity-belt.md](../designs/v2.6-chastity-belt.md) | erb | - | 貞操帯 |
| v2.7 | [v2.7-pregnancy.md](../designs/v2.7-pregnancy.md) | erb | - | 妊娠 |
| v3.0 | [v3.0-phase-management.md](../designs/v3.0-phase-management.md) | engine | - | Phase管琁E|
```

---

## Dependencies

- F223 (/do Workflow Comprehensive Audit): 同様�Eアプローチを採用
- No blocking dependencies: Can proceed in parallel with other infra features

---

## Links

- [/plan command](../../../archive/claude_legacy_20251230/commands/plan.md)
- [/next command](../../../archive/claude_legacy_20251230/commands/next.md)
- [/do command](../../../archive/claude_legacy_20251230/commands/do.md)
- [F223](feature-223.md) - 参老E /do Audit Feature
