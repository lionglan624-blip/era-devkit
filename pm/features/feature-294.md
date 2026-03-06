# Feature 294: erb-duplicate-check.py スタブ判定追加

## Status: [DONE]

## Type: infra

## Background

### Philosophy
重複チェックツールがスタブと実装済みを区別し、/do ワークフローが適切な判断を下せるようにする。

### Problem
F288 で K4, K9 が `OK: 1 definition` を返したため「実装済み」と誤判断する可能性があった。実際は LOCAL=1 でも PRINTFORMW の後が空白の空スタブだった。

### Goal
erb-duplicate-check.py に `--check-stub` オプションを追加し、以下を返す:
- `STUB`: 関数内に DATALIST + DATAFORM (非空テキスト) がない
- `IMPLEMENTED`: DATALIST 内に DATAFORM (非空テキスト) あり
- `NOT_FOUND`: 関数なし

Note: LOCAL パターンは一部関数でのみ使用。IF TALENT 直接分岐も存在するため、出力コマンドの有無で判定。

### Detection Logic

**動作**: `--check-stub` 使用時、ツールはまず `@関数名` を含むファイルを検索し、そのファイル全体から DATAFORM パターンを検索する。

**スコープ**: ファイル単位で検出。関数が存在するファイル全体から DATALIST/DATAFORM を検索。

- **STUB**: 関数が存在するが、ファイル内に内容付き `DATAFORM` がない (DATALIST なし、または DATALIST 内が空の DATAFORM のみ)
- **IMPLEMENTED**: ファイル内に 1行以上の `DATAFORM\s+\S` (DATAFORM + 空白 + 非空白文字) を含む
- **NOT_FOUND**: 関数定義 (`@関数名`) が見つからない

Note:
- 空の `DATAFORM` 行 (スペーシング用) は無視する
- 内容付き `DATAFORM` は `DATAFORM 「テキスト」` 形式
- KOJO 関数は `_1` サフィックス関数に CALL 委譲するため、親関数名で検索してもファイル内の `_1` 関数内容で判定
- PRINTFORMW のみ使用するファイル (DATALIST/DATAFORM なし) は暗黙的に STUB と判定される (DATAFORM 非存在)

### Context
- 発生: F288 重複チェック時
- 現状: 関数定義の有無のみ判定
- 改善: 内容の品質も判定

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | --check-stub オプション追加 | code | Grep(tools/erb-duplicate-check.py) | contains | "--check-stub" | [x] |
| 2 | STUB 判定 (既存スタブ関数) | output | CLI | contains | "STUB:" | [x] |
| 3 | IMPLEMENTED 判定 (実装済み関数) | output | CLI | contains | "IMPLEMENTED:" | [x] |
| 4 | NOT_FOUND 判定 (存在しない関数) | output | CLI | contains | "NOT_FOUND:" | [x] |

### AC Details

**AC#1 Test**: `grep "--check-stub" tools/erb-duplicate-check.py`

**AC#2 Test**: `python tools/erb-duplicate-check.py --check-stub --function KOJO_MESSAGE_COM_KU_60 --path Game/ERB`
- Expected: `STUB: KOJO_MESSAGE_COM_KU_60`
- Note: KOJO_KU_挿入.ERB は PRINTFORMW のみ使用 (DATALIST/DATAFORM なし)。DATAFORM 非存在のため STUB と判定。

**AC#3 Test**: `python tools/erb-duplicate-check.py --check-stub --function KOJO_MESSAGE_COM_K1_80 --path Game/ERB`
- Expected: `IMPLEMENTED: KOJO_MESSAGE_COM_K1_80`
- Note: ファイル単位検出のため、親関数名で検索しても `_1` サフィックス関数の DATAFORM 内容で判定

**AC#4 Test**: `python tools/erb-duplicate-check.py --check-stub --function NONEXISTENT_XYZ --path Game/ERB`
- Expected: `NOT_FOUND: NONEXISTENT_XYZ`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | --check-stub オプションを argparse に追加 | [x] |
| 2 | 2,3,4 | STUB/IMPLEMENTED/NOT_FOUND 判定ロジック実装 (DATAFORM パターン検出) | [x] |
| 3 | 1-4 | 全 AC テスト実行・結果確認 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 16:13 | START | implementer | Task 1 | - |
| 2026-01-01 16:13 | END | implementer | Task 1 | SUCCESS |
| 2026-01-01 16:13 | START | implementer | Task 2 | - |
| 2026-01-01 16:13 | END | implementer | Task 2 | SUCCESS |
| 2026-01-01 16:13 | START | implementer | Task 3 | - |
| 2026-01-01 16:13 | END | implementer | Task 3 | SUCCESS |

---

## 残課題

- ~~`/do ワークフローからの呼び出し`: --check-stub オプションをワークフローで使用する際のガイダンス~~ → **F298 として登録済み**

---

## Links
- [index-features.md](index-features.md)
- 親Feature: [feature-288.md](feature-288.md)
