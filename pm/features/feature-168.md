# Feature 168: KU関数の既存カチE��リ統吁E

## Status: [DONE]

## Type: infra

## Background

**Problem**: Feature 167で発見された課顁E
- 628件 (12.4%) のKU関数が未刁E��E
- NTR_EVENT/NTR_SPECIAL間で10件の重褁E��ウンチE

**Goal**:
1. `_KU`パターンを各カチE��リに統合し、未刁E��を解涁E
2. カチE��リ間重褁E��適刁E��処琁E

**Approach**:
- 吁E��チE��リのパターンを拡張して`_KU`バリアントもマッチE
- NTR_SPECIALをNTR_EVENTから除外するロジチE��追加

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | 未刁E��関数ぁE件 | output | equals | "Uncategorized: 0" | [x] |
| 2 | カチE��リ重褁E��0件 | output | equals | "Overlaps: 0" | [x] |
| 3 | 全@関数 = 14カチE��リ合訁E| output | matches | `\d+ total functions = \d+ category matches` | [x] |

### AC Details

**AC1**: 全@関数ぁE4カチE��リのぁE��れかに刁E��されること

**AC2**: 吁E��数は1カチE��リのみにカウントされること�E�ETR_SPECIALはNTR_EVENTから除外！E

**AC3**: `grep "^@" | wc -l` = Σ(14カチE��リ)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | 吁E��チE��リパターンにKUマッチ追加し未刁E��を0に | [x] |
| 2 | 2 | NTR_SPECIAL除外ロジチE��実裁E��重褁E��0に | [x] |
| 3 | 3 | 全@関数数 = 14カチE��リ合計�E等式検証を実裁E| [x] |

---

## Implementation Notes

### Task Execution Order

1. **Task 1** (prerequisite): Add KU pattern matching to all 14 categories ↁEAC1 passes (Uncategorized: 0)
2. **Task 2** (depends on Task 1): Implement NTR_SPECIAL exclusion logic ↁEAC2 passes (Overlaps: 0)
3. **Task 3** (verify both): Run verification script validating total @ function count equals category sum ↁEAC3 passes

All three tasks must complete successfully and all three ACs must pass for feature completion.

### パターン変更侁E

```python
# Before
"COM": r"@KOJO_MESSAGE_COM_K(\d+)_(\d+)"

# After
"COM": r"@KOJO_MESSAGE_COM_K(?:U|(\d+))_(\d+)"
```

### 重褁E��外ロジチE��

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

- [feature-167.md](feature-167.md) - 親Feature�E�発見�E�E�E
- [kojo_mapper.py](../../src/tools/kojo-mapper/kojo_mapper.py) - 修正対象
