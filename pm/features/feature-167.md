# Feature 167: Kojo-Mapper Coverage Verification - All 14 Categories

## Status: [DONE]

## Type: infra

## Background

**Problem**: Feature 166で14カチE��リのカバレチE��刁E��機�Eを実裁E��たが、DAILYカチE��リでカウントバグが発見された。他�EカチE��リにも同様�E問題がある可能性がある、E

**Approach**: 全14カチE��リにつぁE��、grepによる実カウントとkojo-mapper出力を比輁E��、不一致を検�E・修正する、E

### 検証対象カチE��リ

| # | カチE��リ | パターン | 検証方況E|
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
| 10 | RELATION | `@KOJO_MESSAGE_(恋�E\|思�E\|告白).*_K...` | grep vs output |
| 11 | PARAM_CHANGE | `@KOJO_MESSAGE_PALAMCNG_([ABC])_K(\d+)` | grep vs output |
| 12 | MARK_CHANGE | `@KOJO_MESSAGE_MARKCNG_K(\d+)` | grep vs output |
| 13 | FAREWELL | `@KOJO_MESSAGE_K(\d+)_SeeYou` | grep vs output |
| 14 | NTR_SPECIAL | `@NTR_KOJO_K(\d+)_(FAKE_ORGASM\|...)` | grep vs output |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | 全14カチE��リ: grep行数とkojo-mapper出力一致 | output | contains | "VERIFIED: 14/14 categories match" | [x] |

### AC Details

**AC1**: Claudeが各カチE��リにつぁE��grep検証を行い、kojo-mapper --coverage出力と比輁E���E14カチE��リで一致すること、E

**Verification Process**:
1. 吁E��チE��リにつぁE��grepで関数定義数をカウント（�Eースライン�E�E
2. `python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E --coverage` を実衁E
3. grep結果とkojo-mapper出力を比輁E
4. 不一致があれ�E kojo_mapper.py を修正
5. 全カチE��リ一致確認後、検証完亁E��宣言

**Expected Output** (検証完亁E��):
```
VERIFIED: 14/14 categories match
- COM: grep=X, mapper=X ✁E
- NTR_EVENT: grep=X, mapper=X ✁E
... (全14カチE��リ)
```

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | 全14カチE��リのgrep検証 + kojo-mapper比輁E+ 不一致修正 | [O] |

---

## Verification Method

吁E��チE��リにつぁE��以下を実行！E

```bash
# 1. grepでユニ�Eク関数定義をカウンチE
grep -rh "PATTERN" "Game/ERB/口丁E" | grep "^@" | sort -u | wc -l

# 2. K1のみ抽出してカウンチE
grep -rh "PATTERN_K1" "Game/ERB/口丁E" | grep "^@" | sort -u | wc -l

# 3. kojo-mapper出力と比輁E
python src/tools/kojo-mapper/kojo_mapper.py "Game/ERB/口丁E --coverage
```

### 許容差異

- grep結果 vs kojo-mapper: ±10%以冁E�E正常�E�関数参�Evs定義の差�E�E
- 大幁E��差異�E�E倍以丁E半�E以下！E バグの可能性

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
| COM | 1319 | 1319 | ✁E|
| COUNTER | 342 | 342 | ✁E|
| DAILY | 38 | 38 | ✁E|
| EVENT | 64 | 64 | ✁E|
| FAREWELL | 58 | 58 | ✁E|
| MARK_CHANGE | 5 | 5 | ✁E|
| NTR_EVENT | 610 | 610 | ✁E|
| NTR_PRE | 124 | 124 | ✁E|
| NTR_SPECIAL | 10 | 10 | ✁E|
| NTR_WITNESS | 69 | 69 | ✁E|
| PARAM_CHANGE | 80 | 80 | ✁E|
| RELATION | 4 | 4 | ✁E|
| SCOM | 145 | 145 | ✁E|
| WC | 1566 | 1566 | ✁E|

**VERIFIED: 14/14 categories match**

### Additional Verification (Level B-D)

| Level | 検証冁E�� | 結果 |
|:-----:|----------|:----:|
| B | キャラ別冁E�� (K1-K10) | PASS |
| C | 全@行との突合ぁE| PASS (628件未刁E��E |
| D | カチE��リ重褁E���E | PASS (10件重褁E |

### Findings for Future Work

1. **未刁E��KU関数**: 628件 (12.4%) - `_KU`パターンが既存カチE��リに含まれてぁE��ぁE
2. **NTR_EVENT/NTR_SPECIAL重褁E*: 10件 - NTR_SPECIALはNTR_EVENTのサブセチE��

**対忁E*: Feature 168 で KU関数の既存カチE��リ統合を実施予宁E

## Links

- [feature-166.md](feature-166.md) - 親Feature�E�EAILY/RELATIONバグ修正済み�E�E
- [kojo_mapper.py](../../src/tools/kojo-mapper/kojo_mapper.py) - 検証対象
