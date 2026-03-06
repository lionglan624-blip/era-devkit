# Feature 167: Kojo-Mapper Coverage Verification - All 14 Categories

## Status: [DONE]

## Type: infra

## Background

**Problem**: Feature 166で14カテゴリのカバレッジ分析機能を実装したが、DAILYカテゴリでカウントバグが発見された。他のカテゴリにも同様の問題がある可能性がある。

**Approach**: 全14カテゴリについて、grepによる実カウントとkojo-mapper出力を比較し、不一致を検出・修正する。

### 検証対象カテゴリ

| # | カテゴリ | パターン | 検証方法 |
|:-:|----------|----------|----------|
| 1 | COM | `@KOJO_MESSAGE_COM_K(\d+)_(\d+)` | grep vs output |
| 2 | NTR_EVENT | `@NTR_KOJO_K(\d+)_(.+)` | grep vs output |
| 3 | NTR_WITNESS | `@NTR_KOJO_KW(\d+)_(.+)` | grep vs output |
| 4 | NTR_PRE | `@NTR_KOJO_PRE_KW?(\d+)_...` | grep vs output |
| 5 | EVENT | `@KOJO_EVENT_K(\d+)_(\d+)` | grep vs output |
| 6 | COUNTER | `@KOJO_MESSAGE_COUNTER_K(\d+)_(\d+)` | grep vs output |
| 7 | DAILY | `@KOJO_MESSAGE_COM_KU_4(\d*)` | grep vs output |
| 8 | WC | `@(SexHara\|WC_\|KOJO_WC_).*_K(\d+)` | grep vs output |
| 9 | SCOM | `@KOJO_MESSAGE_SCOM_K(\d+)_(\d+)` | grep vs output |
| 10 | RELATION | `@KOJO_MESSAGE_(恋慕\|思慕\|告白).*_K...` | grep vs output |
| 11 | PARAM_CHANGE | `@KOJO_MESSAGE_PALAMCNG_([ABC])_K(\d+)` | grep vs output |
| 12 | MARK_CHANGE | `@KOJO_MESSAGE_MARKCNG_K(\d+)` | grep vs output |
| 13 | FAREWELL | `@KOJO_MESSAGE_K(\d+)_SeeYou` | grep vs output |
| 14 | NTR_SPECIAL | `@NTR_KOJO_K(\d+)_(FAKE_ORGASM\|...)` | grep vs output |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | 全14カテゴリ: grep行数とkojo-mapper出力一致 | output | contains | "VERIFIED: 14/14 categories match" | [x] |

### AC Details

**AC1**: Claudeが各カテゴリについてgrep検証を行い、kojo-mapper --coverage出力と比較。全14カテゴリで一致すること。

**Verification Process**:
1. 各カテゴリについてgrepで関数定義数をカウント（ベースライン）
2. `python tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" --coverage` を実行
3. grep結果とkojo-mapper出力を比較
4. 不一致があれば kojo_mapper.py を修正
5. 全カテゴリ一致確認後、検証完了を宣言

**Expected Output** (検証完了時):
```
VERIFIED: 14/14 categories match
- COM: grep=X, mapper=X ✓
- NTR_EVENT: grep=X, mapper=X ✓
... (全14カテゴリ)
```

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | 全14カテゴリのgrep検証 + kojo-mapper比較 + 不一致修正 | [O] |

---

## Verification Method

各カテゴリについて以下を実行：

```bash
# 1. grepでユニーク関数定義をカウント
grep -rh "PATTERN" "Game/ERB/口上/" | grep "^@" | sort -u | wc -l

# 2. K1のみ抽出してカウント
grep -rh "PATTERN_K1" "Game/ERB/口上/" | grep "^@" | sort -u | wc -l

# 3. kojo-mapper出力と比較
python tools/kojo-mapper/kojo_mapper.py "Game/ERB/口上" --coverage
```

### 許容差異

- grep結果 vs kojo-mapper: ±10%以内は正常（関数参照vs定義の差）
- 大幅な差異（2倍以上/半分以下）: バグの可能性

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 | INIT | initializer | Feature initialization | READY |
| 2025-12-21 | INVESTIGATE | explorer | kojo-mapper analysis | 14 categories identified |
| 2025-12-21 | IMPLEMENT | implementer | 14-category verification | 4 bugs fixed, all match |
| 2025-12-21 | REGRESSION | regression-tester | Build + Unit tests | PASS (85/85) |
| 2025-12-21 | AC_VERIFY | ac-tester | Verify AC1 | PASS |

### Bugs Fixed in kojo_mapper.py

1. **Function Definition vs Reference Counting**: Changed to line-by-line parsing, only matching lines starting with `@`
2. **Global Deduplication**: Added global_coverage dict with sets to deduplicate function names globally
3. **Japanese Character Support**: Added Unicode ranges for Hiragana, Katakana, Kanji
4. **Whitespace/Comment Handling**: Split on whitespace first before regex matching

### Final Verification Results

| Category | Grep | Mapper | Status |
|----------|------|--------|--------|
| COM | 1319 | 1319 | ✓ |
| COUNTER | 342 | 342 | ✓ |
| DAILY | 38 | 38 | ✓ |
| EVENT | 64 | 64 | ✓ |
| FAREWELL | 58 | 58 | ✓ |
| MARK_CHANGE | 5 | 5 | ✓ |
| NTR_EVENT | 610 | 610 | ✓ |
| NTR_PRE | 124 | 124 | ✓ |
| NTR_SPECIAL | 10 | 10 | ✓ |
| NTR_WITNESS | 69 | 69 | ✓ |
| PARAM_CHANGE | 80 | 80 | ✓ |
| RELATION | 4 | 4 | ✓ |
| SCOM | 145 | 145 | ✓ |
| WC | 1566 | 1566 | ✓ |

**VERIFIED: 14/14 categories match**

### Additional Verification (Level B-D)

| Level | 検証内容 | 結果 |
|:-----:|----------|:----:|
| B | キャラ別内訳 (K1-K10) | PASS |
| C | 全@行との突合せ | PASS (628件未分類) |
| D | カテゴリ重複検出 | PASS (10件重複) |

### Findings for Future Work

1. **未分類KU関数**: 628件 (12.4%) - `_KU`パターンが既存カテゴリに含まれていない
2. **NTR_EVENT/NTR_SPECIAL重複**: 10件 - NTR_SPECIALはNTR_EVENTのサブセット

**対応**: Feature 168 で KU関数の既存カテゴリ統合を実施予定

## Links

- [feature-166.md](feature-166.md) - 親Feature（DAILY/RELATIONバグ修正済み）
- [kojo_mapper.py](../../tools/kojo-mapper/kojo_mapper.py) - 検証対象
