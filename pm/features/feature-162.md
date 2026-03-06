# Feature 162: Test File Migration

## Status: [DONE]

## Type: erb

## Background

Test Infrastructure Reorganization の第3段階。
既存テストファイルを新フォルダ構造に移動する。

## Dependencies

- Feature 161 (Folder Structure) - 移動先フォルダが必要

## Migration Plan

| 現在 | アクション | 移動先 |
|------|----------|--------|
| `tests/core/feature-XXX/` | 移動 | `tests/ac/erb/feature-XXX/` |
| `tests/kojo/feature-XXX/` | 移動 | `tests/ac/kojo/feature-XXX/` |
| `tests/kojo/feature{ID}-*.json` | 移動+リネーム | `tests/ac/kojo/feature-{ID}/` |
| `tests/kojo-136-*.json` (root) | 移動 | `tests/ac/kojo/feature-136/` |
| `tests/core/scenario-*.json` | 移動 | `tests/regression/` |
| `tests/output/` | 移動 | `logs/` |
| `tests/ntr/` | 移動 | `tests/regression/ntr/` |
| `tests/train/` | 移動 | `tests/regression/train/` |
| `tests/inject/` | 移動 | `tests/fixtures/inject/` |
| `tests/feature-153/` | 移動 | `tests/ac/erb/feature-153/` |
| `tests/features/` | **削除** | archive/ |
| `tests/comprehensive/` | **削除** | - |
| `tests/unit/` | **削除** | - |

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 0 | 移動先フォルダ作成完了 | file | exists | Game/tests/fixtures/inject/ | [x] |
| 1 | tests/core/feature-108/ 移動完了 | file | not_exists | Game/tests/core/feature-108/ | [x] |
| 2 | tests/core/feature-154/ 移動完了 | file | not_exists | Game/tests/core/feature-154/ | [x] |
| 3 | tests/core/feature-157/ 移動完了 | file | not_exists | Game/tests/core/feature-157/ | [x] |
| 4 | tests/feature-153/ 移動完了 | file | not_exists | Game/tests/feature-153/ | [x] |
| 5 | tests/kojo/feature-129/ 移動完了 | file | not_exists | Game/tests/kojo/feature-129/ | [x] |
| 6 | tests/kojo/feature109-suite.json 移動完了 | file | not_exists | Game/tests/kojo/feature109-suite.json | [x] |
| 7 | tests/kojo/feature137-K1.json 移動完了 | file | not_exists | Game/tests/kojo/feature137-K1.json | [x] |
| 8 | tests/kojo-136-K1.json 移動完了 | file | not_exists | Game/tests/kojo-136-K1.json | [x] |
| 9 | tests/core/scenario-alice-sameroom.json 移動完了 | file | not_exists | Game/tests/core/scenario-alice-sameroom.json | [x] |
| 10 | tests/core/scenario-sc-001-shiboo-threshold.json 移動完了 | file | not_exists | Game/tests/core/scenario-sc-001-shiboo-threshold.json | [x] |
| 11 | tests/output/ 移動完了 | file | not_exists | Game/tests/output/ | [x] |
| 12 | tests/ntr/ 移動完了 | file | not_exists | Game/tests/ntr/ | [x] |
| 13 | tests/train/ 移動完了 | file | not_exists | Game/tests/train/ | [x] |
| 14 | tests/inject/ 移動完了 | file | not_exists | Game/tests/inject/ | [x] |
| 15 | tests/features/ 削除完了 | file | not_exists | Game/tests/features/ | [x] |
| 16 | tests/comprehensive/ 削除完了 | file | not_exists | Game/tests/comprehensive/ | [x] |
| 17 | tests/unit/ 削除完了 | file | not_exists | Game/tests/unit/ | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | 0 | 移動先フォルダ作成 (regression/ntr, regression/train, fixtures/inject) | [O] |
| 1 | 1-3 | tests/core/feature-XXX/ → tests/ac/erb/ 移動 | [O] |
| 2 | 4 | tests/feature-153/ → tests/ac/erb/feature-153/ 移動 | [O] |
| 3 | 5 | tests/kojo/feature-XXX/ → tests/ac/kojo/ 移動 | [O] |
| 4 | 6-7 | tests/kojo/feature{ID}-*.json → tests/ac/kojo/feature-{ID}/ 移動+リネーム | [O] |
| 5 | 8 | tests/kojo-136-*.json (root) → tests/ac/kojo/feature-136/ 移動 | [O] |
| 6 | 9-10 | tests/core/scenario-*.json → tests/regression/ 移動 | [O] |
| 7 | 11 | tests/output/ → logs/ 移動 | [O] |
| 8 | 12-13 | tests/ntr/, tests/train/ → tests/regression/ 移動 | [O] |
| 9 | 14 | tests/inject/ → tests/fixtures/inject/ 移動 | [O] |
| 10 | 15-17 | tests/features/, tests/comprehensive/, tests/unit/ 削除 | [O] |

## Notes

> **Warning**: Migration完了後、Feature 165 (Documentation) が完了するまでドキュメントとファイルパスに不整合が発生する。
> 162-165は連続実行を推奨。

> **AC:Task N:1 許容**: ファイル移動は論理グループ単位で実行するため、複数ACを1Taskで検証する。
> 例: Task 1 (core/feature-XXX移動) → AC 1-3 で代表3フォルダを検証。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 10:00:00 | Finalization | finalizer | Mark all ACs [x], all Tasks [O], Status [DONE] | COMPLETE |

---

## Links

- [feature-161.md](feature-161.md) - Folder Structure (依存元)
- [feature-163.md](feature-163.md) - Protection Hooks (依存先)
- [feature-165.md](feature-165.md) - Documentation Update (依存先)
