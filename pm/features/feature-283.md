# Feature 283: kojo-writer stub検出・ステータス出力の改善

## Status: [DONE]

## Type: infra

## Background

### Philosophy
kojo-writerワークフローは信頼性高く、最小限の手動介入で動作すべき。

### Problem
Feature 280実行時に以下の問題が発生：

| Deviation | 根本原因 | 影響 |
|-----------|----------|------|
| K2 BLOCKED despite LOCAL=0 | kojo-writerが「空stub」と「実装済み」を区別しない | 不要なBLOCK |
| K3 BLOCKED after implementation | 同上 | 再dispatchが必要に |
| K2, K3 status file未作成 | BLOCKED時のstatus file作成ルールが曖昧 | polling延長 |
| 手動status file作成 | ↑の結果 | 手動介入 |
| K2再dispatch | ↑の結果 | 手動介入 |
| AC#12 path欠落 | kojo-init.md AC#12テンプレートにpath未記載 | 調査完了→修正予定 |
| Duplicate check後の対処曖昧 | do.mdの「Remove stub first」が具体的手順なし | 曖昧 |

### Goal
1. LOCAL=0 stubを「置換対象」として識別
2. BLOCKED時もstatus fileを作成
3. do.md Phase 4.0にstub検出時の具体的手順を追加
4. AC#12 path問題を調査・修正

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | kojo-writer.md に空stub判定ルール追加 | code | .claude/agents/kojo-writer.md | contains | "LOCAL=0のみ" | [x] |
| 2 | kojo-writer.md にBLOCKED時status file作成ルール追加 | code | .claude/agents/kojo-writer.md | contains | "BLOCKED時もstatus file" | [x] |
| 3 | do.md Phase 4.0にstub削除手順追加 | code | .claude/commands/do.md | contains | "Remove stub" | [x] |
| 4 | kojo-init.md AC#12にpath追加 | code | .claude/commands/kojo-init.md | contains | "--flow tests/regression/" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo-writer.md に空stub(LOCAL=0のみ)vs実装済み(DATALIST有)判定ロジック追加 | [O] |
| 2 | 2 | kojo-writer.md にBLOCKED時もstatus file作成ルール追加 | [O] |
| 3 | 3 | do.md Phase 4.0にstub削除手順を追加 | [O] |
| 4 | 4 | kojo-init.md AC#12テンプレートにpath追加 | [O] |

---

## Investigation Notes

### AC#12 Path問題調査（調査完了）

**調査対象**:
- `.claude/commands/kojo-init.md` - kojo feature初期化
- `Game/agents/reference/feature-template.md` - feature templateのAC定義
- `tools/kojo-mapper/kojo_test_gen.py` - テスト生成スクリプト

**調査結果**:
- **kojo-init.md AC#12**: `--flow` のみでpath指定なし（line 112）
- **feature-280.md AC#12**: `--flow tests/regression/` でpath指定あり
- **原因**: kojo-init.md template が path を省略している
- **修正方針**: kojo-init.md の AC#12 テンプレートに path 追加が必要

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-31 22:00 | START | implementer | Task 1 | - |
| 2025-12-31 22:00 | END | implementer | Task 1 | SUCCESS |
| 2025-12-31 22:00 | START | implementer | Task 2 | - |
| 2025-12-31 22:00 | END | implementer | Task 2 | SUCCESS |
| 2025-12-31 22:00 | START | implementer | Task 3 | - |
| 2025-12-31 22:00 | END | implementer | Task 3 | SUCCESS |
| 2025-12-31 22:00 | START | implementer | Task 4 | - |
| 2025-12-31 22:00 | END | implementer | Task 4 | SUCCESS |

## Links
- 発端: [feature-280.md](feature-280.md)
