# Feature 161: Test Folder Structure

## Status: [DONE]

## Type: erb

## Background

Test Infrastructure Reorganization の第2段階、E
Migration前にフォルダ構造を作�Eする忁E��がある、E

## Dependencies

- Feature 160 (file_exists matcher) - AC検証に忁E��E

## New Folder Structure

```
tests/
├── ac/                    # ACチE��チE(Phase 2.5作�E、編雁E��止)
━E  ├── erb/               # --inject で実衁E
━E  └── kojo/              # --unit で実衁E
━E
├── regression/            # 回帰チE��チE(固定、編雁E��止)
━E
├── fixtures/              # チE��トデータ (維持E
━E
├── scripts/               # チE��トスクリプト (維持E
━E
└── debug/                 # チE��チE��用 (自由編雁E��)

logs/                      # チE��ト結果出劁E(Engine自動生戁E
├── ac/
━E  ├── erb/
━E  └── kojo/
├── regression/
└── debug/
```

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | tests/ac/erb/ 作�E | file | exists | test/ac/erb/ | [x] |
| 2 | tests/ac/kojo/ 作�E | file | exists | test/ac/kojo/ | [x] |
| 3 | tests/regression/ 維持確誁E| file | exists | test/regression/ | [x] |
| 4 | tests/debug/ 作�E | file | exists | test/debug/ | [x] |
| 5 | logs/ac/erb/ 作�E | file | exists | _out/logs/ac/erb/ | [x] |
| 6 | logs/ac/kojo/ 作�E | file | exists | _out/logs/ac/kojo/ | [x] |
| 7 | logs/regression/ 作�E | file | exists | _out/logs/regression/ | [x] |
| 8 | logs/debug/ 作�E | file | exists | _out/logs/debug/ | [x] |
| 9 | .gitignore に logs/ 追加 | file | contains | "logs/" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | tests/ac/erb/ チE��レクトリ作�E | [O] |
| 2 | 2 | tests/ac/kojo/ チE��レクトリ作�E | [O] |
| 3 | 3 | tests/regression/ 存在確誁E| [O] |
| 4 | 4 | tests/debug/ チE��レクトリ作�E | [O] |
| 5 | 5 | logs/ac/erb/ チE��レクトリ作�E | [O] |
| 6 | 6 | logs/ac/kojo/ チE��レクトリ作�E | [O] |
| 7 | 7 | logs/regression/ チE��レクトリ作�E | [O] |
| 8 | 8 | logs/debug/ チE��レクトリ作�E | [O] |
| 9 | 9 | .gitignore 更新 | [O] |

## Naming Convention

| 要素 | ルール | 侁E|
|------|--------|-----|
| Feature ID | `feature-{ID}` (ハイフン忁E��E | `feature-135` |
| キャラ番号 | `-K{N}` (大斁E��K) | `-K1`, `-K10` |
| ACチE��チE| `test-{ID}-ac{N}.json` | `test-135-ac1.json` |
| KojoチE��チE| `feature-{ID}-K{N}.json` | `feature-135-K1.json` |

## Test File Format Reference

| Prefix | Format | CLI Option | Location | Description |
|--------|--------|------------|----------|-------------|
| `feature-*` | KojoTestScenario | `--unit` | `tests/ac/kojo/` | Kojo関数チE��チE|
| `test-*-ac*` | KojoTestScenario | `--unit` | `tests/ac/erb/` | ERB ACチE��チE|
| `kojo-*` | KojoTestScenario | `--unit` | `tests/regression/` | Kojo回帰チE��チE|
| `scenario-*` | InjectScenario | `--inject` | `tests/regression/` | フロー回帰チE��チE|

**重要E*: `--unit` は KojoTestScenario 形式�Eみ対応。`scenario-*.json` は `--inject` で実行する、E

### KojoTestScenario 形弁E(--unit)

```json
{
  "name": "チE��ト名",
  "tests": [
    { "name": "K1", "character": "1", "call": "FUNC_NAME", ... }
  ]
}
```

### InjectScenario 形弁E(--inject)

```json
{
  "name": "シナリオ吁E,
  "description": "説昁E
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

- [feature-160.md](feature-160.md) - File Exists Matcher (依存�E)
- [feature-162.md](feature-162.md) - Migration (依存�E)
