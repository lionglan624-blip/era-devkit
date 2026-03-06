# Feature 161: Test Folder Structure

## Status: [DONE]

## Type: erb

## Background

Test Infrastructure Reorganization の第2段階。
Migration前にフォルダ構造を作成する必要がある。

## Dependencies

- Feature 160 (file_exists matcher) - AC検証に必要

## New Folder Structure

```
tests/
├── ac/                    # ACテスト (Phase 2.5作成、編集禁止)
│   ├── erb/               # --inject で実行
│   └── kojo/              # --unit で実行
│
├── regression/            # 回帰テスト (固定、編集禁止)
│
├── fixtures/              # テストデータ (維持)
│
├── scripts/               # テストスクリプト (維持)
│
└── debug/                 # デバッグ用 (自由編集可)

logs/                      # テスト結果出力 (Engine自動生成)
├── ac/
│   ├── erb/
│   └── kojo/
├── regression/
└── debug/
```

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | tests/ac/erb/ 作成 | file | exists | Game/tests/ac/erb/ | [x] |
| 2 | tests/ac/kojo/ 作成 | file | exists | Game/tests/ac/kojo/ | [x] |
| 3 | tests/regression/ 維持確認 | file | exists | Game/tests/regression/ | [x] |
| 4 | tests/debug/ 作成 | file | exists | Game/tests/debug/ | [x] |
| 5 | logs/ac/erb/ 作成 | file | exists | Game/logs/ac/erb/ | [x] |
| 6 | logs/ac/kojo/ 作成 | file | exists | Game/logs/ac/kojo/ | [x] |
| 7 | logs/regression/ 作成 | file | exists | Game/logs/regression/ | [x] |
| 8 | logs/debug/ 作成 | file | exists | Game/logs/debug/ | [x] |
| 9 | .gitignore に logs/ 追加 | file | contains | "logs/" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | tests/ac/erb/ ディレクトリ作成 | [O] |
| 2 | 2 | tests/ac/kojo/ ディレクトリ作成 | [O] |
| 3 | 3 | tests/regression/ 存在確認 | [O] |
| 4 | 4 | tests/debug/ ディレクトリ作成 | [O] |
| 5 | 5 | logs/ac/erb/ ディレクトリ作成 | [O] |
| 6 | 6 | logs/ac/kojo/ ディレクトリ作成 | [O] |
| 7 | 7 | logs/regression/ ディレクトリ作成 | [O] |
| 8 | 8 | logs/debug/ ディレクトリ作成 | [O] |
| 9 | 9 | .gitignore 更新 | [O] |

## Naming Convention

| 要素 | ルール | 例 |
|------|--------|-----|
| Feature ID | `feature-{ID}` (ハイフン必須) | `feature-135` |
| キャラ番号 | `-K{N}` (大文字K) | `-K1`, `-K10` |
| ACテスト | `test-{ID}-ac{N}.json` | `test-135-ac1.json` |
| Kojoテスト | `feature-{ID}-K{N}.json` | `feature-135-K1.json` |

## Test File Format Reference

| Prefix | Format | CLI Option | Location | Description |
|--------|--------|------------|----------|-------------|
| `feature-*` | KojoTestScenario | `--unit` | `tests/ac/kojo/` | Kojo関数テスト |
| `test-*-ac*` | KojoTestScenario | `--unit` | `tests/ac/erb/` | ERB ACテスト |
| `kojo-*` | KojoTestScenario | `--unit` | `tests/regression/` | Kojo回帰テスト |
| `scenario-*` | InjectScenario | `--inject` | `tests/regression/` | フロー回帰テスト |

**重要**: `--unit` は KojoTestScenario 形式のみ対応。`scenario-*.json` は `--inject` で実行する。

### KojoTestScenario 形式 (--unit)

```json
{
  "name": "テスト名",
  "tests": [
    { "name": "K1", "character": "1", "call": "FUNC_NAME", ... }
  ]
}
```

### InjectScenario 形式 (--inject)

```json
{
  "name": "シナリオ名",
  "description": "説明"
}
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 08:40 | START | initializer | Feature init | READY |
| 2025-12-21 08:40 | INVESTIGATE | explorer | Check folder structure | DONE |
| 2025-12-21 08:41 | IMPLEMENT | implementer | Create directories | SUCCESS |
| 2025-12-21 08:42 | REGRESSION | regression-tester | Run regression tests | PASS |
| 2025-12-21 08:43 | AC_VERIFY | ac-tester | Verify AC1-9 | ALL PASS |
| 2025-12-21 08:44 | FINALIZE | finalizer | Status update | READY_TO_COMMIT |

---

## Links

- [feature-160.md](feature-160.md) - File Exists Matcher (依存元)
- [feature-162.md](feature-162.md) - Migration (依存先)
