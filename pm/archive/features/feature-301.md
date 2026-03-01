# Feature 301: erb-duplicate-check.py --check-stub 空スタブ誤判定修正

## Status: [DONE]

## Type: infra

## Background

### Philosophy (Mid-term Vision)
ツールは正確な判定を行い、ワークフローが誤った前提で進行することを防ぐ

### Problem (Current Issue)

F289 実行中に発見された問題:

`erb-duplicate-check.py --check-stub` が以下のパターンを `IMPLEMENTED` と誤判定:

```erb
@KOJO_MESSAGE_COM_K2_83_1
LOCAL = 0          ; または LOCAL = 1
IF LOCAL
    IF TALENT:恋慕
        PRINTFORMW   ; ← 空欄（台詞なし）
        RETURN 0
    ENDIF
ENDIF
RETURN 1
```

**判定結果**:
| キャラ | LOCAL | PRINTFORMW | --check-stub | 正しい判定 | AC |
|:------:|:-----:|:----------:|:------------:|:----------:|:--:|
| K2 | 0 | 空 | IMPLEMENTED | STUB | AC1 |
| K3 | 0 | 空 | IMPLEMENTED | STUB | AC3 |
| K4 | 1 | 空 | IMPLEMENTED | STUB | AC4 |
| K9 | 1 | 空 | IMPLEMENTED | STUB | (AC4でカバー) |

Note: K9 は K4 と同じ LOCAL=1 パターンのため、AC4 で代表してテスト。

**根本原因**: 現在の `--check-stub` は**ファイル全体**を検索対象としている:
1. 関数名でファイルを特定
2. ファイル全体から `DATAFORM\s+\S` パターンを検索
3. 同一ファイル内の**別関数**に DATAFORM があると IMPLEMENTED と誤判定

K2_83 (STUB) と K2_81 (IMPLEMENTED) が同一ファイル `KOJO_K2_口挿入.ERB` に存在するため、
K2_81 の DATAFORM が検出されて K2_83 も IMPLEMENTED と判定されている。

### Goal (What to Achieve)

`--check-stub` が以下を正しく判定する:
- **STUB**: 関数は存在するが、PRINTFORMW が空または存在しない
- **IMPLEMENTED**: 関数が存在し、PRINTFORMW に実際の台詞テキストがある

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | 空 PRINTFORMW を STUB と判定 | output | CLI | equals | "STUB: KOJO_MESSAGE_COM_K2_83" | [x] |
| 2 | 台詞あり を IMPLEMENTED と判定 (DATAFORM回帰) | output | CLI | equals | "IMPLEMENTED: KOJO_MESSAGE_COM_K2_81" | [x] |
| 3 | LOCAL=0 + 空 PRINTFORMW を STUB と判定 | output | CLI | equals | "STUB: KOJO_MESSAGE_COM_K3_83" | [x] |
| 4 | LOCAL=1 + 空 PRINTFORMW を STUB と判定 | output | CLI | equals | "STUB: KOJO_MESSAGE_COM_K4_83" | [x] |

### AC Details

**AC1 Test**:
```bash
python tools/erb-duplicate-check.py --function "KOJO_MESSAGE_COM_K2_83" --path "Game/ERB/口上" --check-stub
# Expected: STUB (現在は IMPLEMENTED と誤判定)
```

**AC2 Test**:
```bash
# 同一ファイル内の実装済み関数で確認 (DATAFORM回帰テスト)
# K2_81 は K2_83 と同じファイル (KOJO_K2_口挿入.ERB) に存在
python tools/erb-duplicate-check.py --function "KOJO_MESSAGE_COM_K2_81" --path "Game/ERB/口上" --check-stub
# Expected: IMPLEMENTED (関数スコープ分離の検証も兼ねる)
```

**AC3 Test** (LOCAL=0 パターン):
```bash
# K3_83 も空スタブ (LOCAL=0)
python tools/erb-duplicate-check.py --function "KOJO_MESSAGE_COM_K3_83" --path "Game/ERB/口上" --check-stub
# Expected: STUB
```

**AC4 Test** (LOCAL=1 パターン):
```bash
# K4_83 は空スタブ (LOCAL=1)
python tools/erb-duplicate-check.py --function "KOJO_MESSAGE_COM_K4_83" --path "Game/ERB/口上" --check-stub
# Expected: STUB
```

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-4 | 関数スコープ抽出を実装し、対象関数内のみでコンテンツ判定を行う | [x] |
| 2 | 1-4 | テストケース作成・検証 | [x] |

<!-- AC:Task bundling rationale: infra type feature where single code change (function scope extraction) satisfies all 4 ACs. F294 precedent. -->

---

## Design Notes

### 判定ロジック案

**修正の核心**: ファイル全体ではなく、親関数 + _1 サフィックス関数のスコープ内のみを検索する

**KOJO 関数の構造**:
- 親関数 (例: K2_83): `CALL TRAIN_MESSAGE` + `CALL K2_83_1` のラッパー
- _1 関数 (例: K2_83_1): 実際の口上ロジック (PRINTDATA/DATALIST/DATAFORM または PRINTFORMW)
- 親関数名で検索しても、_1 関数の内容で判定する必要がある

```python
def extract_function_scope(file_content: str, function_name: str) -> str:
    """
    親関数 + _1 サフィックス関数のコンテンツを抽出

    例: function_name = "KOJO_MESSAGE_COM_K2_83"
        → @KOJO_MESSAGE_COM_K2_83 ～ @KOJO_MESSAGE_COM_K2_83_1 の終わりまで抽出

    注意: 関数名が他の関数名の部分文字列になる場合を考慮
    """
    # 親関数 + _1 関数の両方を含むスコープを抽出
    # @FUNC から @FUNC_1 の終わり（次の無関係な @ まで）を取得
    parent_pattern = rf'^@{re.escape(function_name)}(?:\s|\(|$)'
    suffix_pattern = rf'^@{re.escape(function_name)}_1(?:\s|\(|$)'

    # Step 1: 親関数の開始位置を特定
    parent_match = re.search(parent_pattern, file_content, re.MULTILINE)
    if not parent_match:
        return ""

    # Step 2: _1 関数の終了位置を特定 (次の無関係な @ まで)
    # _1 関数の開始を探し、そこから次の @ (または EOF) まで
    search_start = parent_match.start()
    remaining = file_content[search_start:]

    # _1 を含む全スコープを抽出
    # 親関数から、_1 関数の終わりまで
    end_pattern = rf'^@(?!{re.escape(function_name)}(_1)?(\s|\(|$))'
    end_match = re.search(end_pattern, remaining, re.MULTILINE)
    if end_match:
        return remaining[:end_match.start()]
    else:
        return remaining  # EOF まで

def is_stub(function_content: str) -> bool:
    """
    関数スコープ内でのスタブ判定:

    IMPLEMENTED判定:
    1. DATAFORM + コンテンツあり → IMPLEMENTED
    2. PRINTFORMW + テキストあり → IMPLEMENTED

    STUB判定:
    - 上記以外すべて (関数定義のみ、空PRINTFORMW、LOCAL=0のみ等)
    """
    # Pattern 1: DATAFORM with content (F294 established)
    if re.search(r'DATAFORM\s+\S', function_content):
        return False  # IMPLEMENTED

    # Pattern 2: PRINTFORMW with content (F301 extension)
    # PRINTFORMW\s+\S matches PRINTFORMW followed by whitespace then non-whitespace
    # Empty PRINTFORMW (no text or whitespace only) will NOT match → returns True (STUB)
    if re.search(r'PRINTFORMW\s+\S', function_content):
        return False  # IMPLEMENTED

    return True  # STUB - no content detected
```

**判定フロー**:
1. 関数スコープを抽出 (`@FUNC` から次の `@` まで)
2. スコープ内で DATAFORM + コンテンツ → IMPLEMENTED
3. スコープ内で PRINTFORMW + テキスト → IMPLEMENTED
4. それ以外 → STUB

**実装上の注意**:
- コメント行 (`;` で始まる行) の PRINTFORMW は無視する
- 例: `;PRINTFORMW 「テキスト」` は IMPLEMENTED とカウントしない

### 空スタブパターン

```erb
; パターン1: LOCAL=0 (表示無効)
LOCAL = 0
IF LOCAL
    PRINTFORMW   ; 空
ENDIF

; パターン2: LOCAL=1 だが PRINTFORMW 空
LOCAL = 1
IF LOCAL
    PRINTFORMW   ; 空欄
ENDIF

; パターン3: DATALIST なし + PRINTFORMW 空
IF TALENT:恋慕
    PRINTFORMW   ; 空
ENDIF
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 20:08 | START | implementer | Task 1 | - |
| 2026-01-01 20:08 | END | implementer | Task 1 | SUCCESS |

---

## Links

- [index-features.md](index-features.md)
- [erb-duplicate-check.py](../../tools/erb-duplicate-check.py)
- Blocking: [feature-289.md](feature-289.md) - COM_83 口上
- Related: [feature-294.md](feature-294.md) - --check-stub 初期実装
