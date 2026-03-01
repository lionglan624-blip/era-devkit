# Feature 168: KU関数の既存カテゴリ統合

## Status: [DONE]

## Type: infra

## Background

**Problem**: Feature 167で発見された課題:
- 628件 (12.4%) のKU関数が未分類
- NTR_EVENT/NTR_SPECIAL間で10件の重複カウント

**Goal**:
1. `_KU`パターンを各カテゴリに統合し、未分類を解消
2. カテゴリ間重複を適切に処理

**Approach**:
- 各カテゴリのパターンを拡張して`_KU`バリアントもマッチ
- NTR_SPECIALをNTR_EVENTから除外するロジック追加

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | 未分類関数が0件 | output | equals | "Uncategorized: 0" | [x] |
| 2 | カテゴリ重複が0件 | output | equals | "Overlaps: 0" | [x] |
| 3 | 全@関数 = 14カテゴリ合計 | output | matches | `\d+ total functions = \d+ category matches` | [x] |

### AC Details

**AC1**: 全@関数が14カテゴリのいずれかに分類されること

**AC2**: 各関数は1カテゴリのみにカウントされること（NTR_SPECIALはNTR_EVENTから除外）

**AC3**: `grep "^@" | wc -l` = Σ(14カテゴリ)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | 各カテゴリパターンにKUマッチ追加し未分類を0に | [x] |
| 2 | 2 | NTR_SPECIAL除外ロジック実装し重複を0に | [x] |
| 3 | 3 | 全@関数数 = 14カテゴリ合計の等式検証を実装 | [x] |

---

## Implementation Notes

### Task Execution Order

1. **Task 1** (prerequisite): Add KU pattern matching to all 14 categories → AC1 passes (Uncategorized: 0)
2. **Task 2** (depends on Task 1): Implement NTR_SPECIAL exclusion logic → AC2 passes (Overlaps: 0)
3. **Task 3** (verify both): Run verification script validating total @ function count equals category sum → AC3 passes

All three tasks must complete successfully and all three ACs must pass for feature completion.

### パターン変更例

```python
# Before
"COM": r"@KOJO_MESSAGE_COM_K(\d+)_(\d+)"

# After
"COM": r"@KOJO_MESSAGE_COM_K(?:U|(\d+))_(\d+)"
```

### 重複除外ロジック

```python
# NTR_EVENT count should exclude NTR_SPECIAL matches
ntr_event_only = ntr_event_matches - ntr_special_matches
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 | COMPLETE | opus | AC Verification | 3/3 PASS |

## Links

- [feature-167.md](feature-167.md) - 親Feature（発見元）
- [kojo_mapper.py](../../tools/kojo-mapper/kojo_mapper.py) - 修正対象
