# Feature 329: kojo 完了時更新フロー調査

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
ワークフローの欠陥は発見時に報告し、調査→修正の流れで恒久対策する

### Problem (Current Issue)
F319 および F324 実行中に以下の更新漏れが発生したが、ワークフローに明確な定義がない:
1. kojo_test_gen.py の実行ディレクトリが KOJO.md に記載あるが発見できず（ドキュメント可視性問題、3回リトライ発生）
2. AC/Task チェックボックスの更新タイミング・責任者が未定義
3. com_file_map.json の更新タイミング・責任者が未定義

これらは「次回も再発する」ワークフロー欠陥である。

### Goal (What to Achieve)
kojo Feature 完了時に必要な更新作業を調査し、修正 Feature を提案する。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | 更新漏れパターン一覧作成 | file | Glob | exists | Game/agents/reference/kojo-update-gaps.md | [x] |
| 2 | 更新責任マトリクス作成 | file | Grep(kojo-update-gaps.md) | matches | Responsibility Matrix | [x] |
| 3 | 修正 Feature 提案 | file | Glob | exists | Game/agents/feature-33*.md | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | kojo-update-gaps.md を作成（F319/F324 のログ調査 + F318 の DEVIATION ゼロ理由確認: スムーズ完了 or 記録義務なし時代） | [x] |
| 2 | 2 | 更新責任マトリクスを kojo-update-gaps.md に追記 | [x] |
| 3 | 3 | 調査結果に基づく修正 Feature 作成 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-03 21:27 | START | implementer | Task 1 | - |
| 2026-01-03 21:27 | END | implementer | Task 1 | SUCCESS |
| 2026-01-03 21:27 | START | implementer | Task 2 | - |
| 2026-01-03 21:27 | END | implementer | Task 2 | SUCCESS |
| 2026-01-03 21:37 | START | implementer | Task 3 | - |
| 2026-01-03 21:37 | END | implementer | Task 3 | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- 関連: [feature-324.md](feature-324.md) (契機となった Feature)
- 関連: [feature-323.md](feature-323.md) (F320実装漏れを修正、COM 0-699 全範囲対応)
