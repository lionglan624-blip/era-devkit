# Feature 162: Test File Migration

## Status: [DONE]

## Type: erb

## Background

Test Infrastructure Reorganization の第3段階、E
既存テストファイルを新フォルダ構造に移動する、E

## Dependencies

- Feature 161 (Folder Structure) - 移動�Eフォルダが忁E��E

## Migration Plan

| 現在 | アクション | 移動�E |
|------|----------|--------|
| `tests/core/feature-XXX/` | 移勁E| `tests/ac/erb/feature-XXX/` |
| `tests/kojo/feature-XXX/` | 移勁E| `tests/ac/kojo/feature-XXX/` |
| `tests/kojo/feature{ID}-*.json` | 移勁Eリネ�Eム | `tests/ac/kojo/feature-{ID}/` |
| `tests/kojo-136-*.json` (root) | 移勁E| `tests/ac/kojo/feature-136/` |
| `tests/core/scenario-*.json` | 移勁E| `tests/regression/` |
| `tests/output/` | 移勁E| `logs/` |
| `tests/ntr/` | 移勁E| `tests/regression/ntr/` |
| `tests/train/` | 移勁E| `tests/regression/train/` |
| `tests/inject/` | 移勁E| `tests/fixtures/inject/` |
| `tests/feature-153/` | 移勁E| `tests/ac/erb/feature-153/` |
| `tests/features/` | **削除** | archive/ |
| `tests/comprehensive/` | **削除** | - |
| `tests/unit/` | **削除** | - |

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 0 | 移動�Eフォルダ作�E完亁E| file | exists | test/fixtures/inject/ | [x] |
| 1 | tests/core/feature-108/ 移動完亁E| file | not_exists | test/core/feature-108/ | [x] |
| 2 | tests/core/feature-154/ 移動完亁E| file | not_exists | test/core/feature-154/ | [x] |
| 3 | tests/core/feature-157/ 移動完亁E| file | not_exists | test/core/feature-157/ | [x] |
| 4 | tests/feature-153/ 移動完亁E| file | not_exists | test/feature-153/ | [x] |
| 5 | tests/kojo/feature-129/ 移動完亁E| file | not_exists | test/kojo/feature-129/ | [x] |
| 6 | tests/kojo/feature109-suite.json 移動完亁E| file | not_exists | test/kojo/feature109-suite.json | [x] |
| 7 | tests/kojo/feature137-K1.json 移動完亁E| file | not_exists | test/kojo/feature137-K1.json | [x] |
| 8 | tests/kojo-136-K1.json 移動完亁E| file | not_exists | test/kojo-136-K1.json | [x] |
| 9 | tests/core/scenario-alice-sameroom.json 移動完亁E| file | not_exists | test/core/scenario-alice-sameroom.json | [x] |
| 10 | tests/core/scenario-sc-001-shiboo-threshold.json 移動完亁E| file | not_exists | test/core/scenario-sc-001-shiboo-threshold.json | [x] |
| 11 | tests/output/ 移動完亁E| file | not_exists | test/output/ | [x] |
| 12 | tests/ntr/ 移動完亁E| file | not_exists | test/ntr/ | [x] |
| 13 | tests/train/ 移動完亁E| file | not_exists | test/train/ | [x] |
| 14 | tests/inject/ 移動完亁E| file | not_exists | test/inject/ | [x] |
| 15 | tests/features/ 削除完亁E| file | not_exists | test/features/ | [x] |
| 16 | tests/comprehensive/ 削除完亁E| file | not_exists | test/comprehensive/ | [x] |
| 17 | tests/unit/ 削除完亁E| file | not_exists | test/unit/ | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | 0 | 移動�Eフォルダ作�E (regression/ntr, regression/train, fixtures/inject) | [O] |
| 1 | 1-3 | tests/core/feature-XXX/ ↁEtests/ac/erb/ 移勁E| [O] |
| 2 | 4 | tests/feature-153/ ↁEtests/ac/erb/feature-153/ 移勁E| [O] |
| 3 | 5 | tests/kojo/feature-XXX/ ↁEtests/ac/kojo/ 移勁E| [O] |
| 4 | 6-7 | tests/kojo/feature{ID}-*.json ↁEtests/ac/kojo/feature-{ID}/ 移勁Eリネ�Eム | [O] |
| 5 | 8 | tests/kojo-136-*.json (root) ↁEtests/ac/kojo/feature-136/ 移勁E| [O] |
| 6 | 9-10 | tests/core/scenario-*.json ↁEtests/regression/ 移勁E| [O] |
| 7 | 11 | tests/output/ ↁElogs/ 移勁E| [O] |
| 8 | 12-13 | tests/ntr/, tests/train/ ↁEtests/regression/ 移勁E| [O] |
| 9 | 14 | tests/inject/ ↁEtests/fixtures/inject/ 移勁E| [O] |
| 10 | 15-17 | tests/features/, tests/comprehensive/, tests/unit/ 削除 | [O] |

## Notes

> **Warning**: Migration完亁E��、Feature 165 (Documentation) が完亁E��るまでドキュメントとファイルパスに不整合が発生する、E
> 162-165は連続実行を推奨、E

> **AC:Task N:1 許容**: ファイル移動�E論理グループ単位で実行するため、褁E��ACめETaskで検証する、E
> 侁E Task 1 (core/feature-XXX移勁E ↁEAC 1-3 で代表3フォルダを検証、E

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 10:00:00 | Finalization | finalizer | Mark all ACs [x], all Tasks [O], Status [DONE] | COMPLETE |

---

## Links

- [feature-161.md](feature-161.md) - Folder Structure (依存�E)
- [feature-163.md](feature-163.md) - Protection Hooks (依存�E)
- [feature-165.md](feature-165.md) - Documentation Update (依存�E)
