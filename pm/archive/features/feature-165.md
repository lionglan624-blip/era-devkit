# Feature 165: Documentation Path Update

## Status: [DONE]

## Type: erb

## Background

Test Infrastructure Reorganization の最終段階。
Feature 162 (Migration) で移動したファイルパスに合わせてドキュメントを更新。

## Dependencies

- Feature 163 (Protection Hooks) - Hook実装完了後
- Feature 164 (Engine Log Path) - Engine変更完了後

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | imple.md テストパス更新 | code | contains | "tests/ac/" | [x] |
| 2 | smoke-tester.md テストパス更新 | code | contains | "tests/ac/kojo/feature-{ID}/" | [x] |
| 3 | ac-tester.md テストパス更新 | code | contains | "tests/ac/" | [x] |
| 4 | regression-tester.md テストパス更新 | code | contains | "tests/regression/" | [x] |
| 5 | kojo-writer.md テストパス更新 | code | contains | "tests/ac/kojo/feature-{ID}/" | [x] |
| 6 | feature-template.md に Method列追加 | code | contains | "| Method |" | [x] |

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | imple.md: テストパス更新 | [O] |
| 2 | 2 | smoke-tester.md: テストパス更新 | [O] |
| 3 | 3 | ac-tester.md: テストパス更新 | [O] |
| 4 | 4 | regression-tester.md: テストパス更新 | [O] |
| 5 | 5 | kojo-writer.md: テストパス更新 | [O] |
| 6 | 6 | feature-template.md: AC Table に Method列追加 | [O] |

## Path Update Mapping

### imple.md 変更箇所

| 行 | 現在 | 変更後 |
|----|------|--------|
| 206 | `tests/kojo-{ID}-K{N}.json` | `tests/ac/kojo/feature-{ID}/feature-{ID}-K{N}.json` |
| 207 | `tests/core/scenario-{ID}-{N}.json` | `tests/ac/erb/feature-{ID}/test-{ID}-ac{N}.json` |
| 284 | `tests/core/*.json` | `tests/regression/*.json` |
| 307 | `tests/kojo-{ID}-K{N}.json` | `tests/ac/kojo/feature-{ID}/` |
| 308 | `tests/core/scenario-{ID}-{N}.json` | `tests/ac/erb/feature-{ID}/` |

### subagent MD 変更箇所

| ファイル | 現在 | 変更後 |
|----------|------|--------|
| ac-tester.md | `tests/kojo/feature{ID}/k{N}-*.json` | `tests/ac/kojo/feature-{ID}/` |
| ac-tester.md | `tests/core/scenario-{ID}-*.json` | `tests/ac/erb/feature-{ID}/` |
| smoke-tester.md | `tests/kojo/feature{ID}/k{N}-*.json` | `tests/ac/kojo/feature-{ID}/` |
| smoke-tester.md | `tests/core/scenario-*.json` | `tests/ac/erb/` or `tests/debug/` |
| regression-tester.md | `tests/core/scenario-*.json` | `tests/regression/*.json` |
| kojo-writer.md | `tests/kojo-{ID}-K{N}.json` | `tests/ac/kojo/feature-{ID}/feature-{ID}-K{N}.json` |

## Post-Implementation: imple Workflow Changes

### Phase 2.5 (Test Creation) 変更

| Type | 現在 | 変更後 |
|------|------|--------|
| kojo | `tests/kojo-{ID}-K{N}.json` | `tests/ac/kojo/feature-{ID}/feature-{ID}-K{N}.json` |
| erb | `tests/core/scenario-{ID}-{N}.json` | `tests/ac/erb/feature-{ID}/test-{ID}-ac{N}.json` |

**保護動作**:
- 新規作成: ✅ 許可 (Phase 2.5 で作成)
- 既存編集: ❌ ブロック (Phase 3以降で改ざん防止)

### Phase 7 (AC Verification) 変更

| Type | コマンド | ログ出力先 |
|------|---------|-----------|
| kojo | `--unit tests/ac/kojo/feature-{ID}/` | `logs/ac/kojo/feature-{ID}/` (自動) |
| erb | `--inject tests/ac/erb/feature-{ID}/` | `logs/ac/erb/feature-{ID}/` (自動) |

### Phase 6 (Regression) 変更

```bash
# 変更前
--inject "tests/core/scenario-*.json"

# 変更後
--inject "tests/regression/*.json"
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 | START | initializer | Feature init | READY |
| 2025-12-21 10:27 | START | implementer | All 6 tasks | - |
| 2025-12-21 10:27 | END | implementer | All 6 tasks | SUCCESS |
| 2025-12-21 10:28 | FINALIZE | finalizer | Status update + index move | COMPLETE |

---

## Links

- [feature-163.md](feature-163.md) - Protection Hooks (依存元)
- [feature-164.md](feature-164.md) - Engine Log Path (依存元)
