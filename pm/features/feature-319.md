# Feature 319: COM_94 Ａ騎乗位する 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem
COM_94 (Ａ騎乗位する) lacks Phase 8d quality dialogue for all characters.

### Goal
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8d: 全COM網羅 + 品質改修
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | K1美鈴 COM_94 口上出力 | output | --unit tests/ac/kojo/feature-319/test-319-K1.json | contains | DATALIST output | [x] |
| 2 | K2小悪魔 COM_94 口上出力 | output | --unit tests/ac/kojo/feature-319/test-319-K2.json | contains | DATALIST output | [x] |
| 3 | K3パチュリー COM_94 口上出力 | output | --unit tests/ac/kojo/feature-319/test-319-K3.json | contains | DATALIST output | [x] |
| 4 | K4咲夜 COM_94 口上出力 | output | --unit tests/ac/kojo/feature-319/test-319-K4.json | contains | DATALIST output | [x] |
| 5 | K5レミリア COM_94 口上出力 | output | --unit tests/ac/kojo/feature-319/test-319-K5.json | contains | DATALIST output | [x] |
| 6 | K6フラン COM_94 口上出力 | output | --unit tests/ac/kojo/feature-319/test-319-K6.json | contains | DATALIST output | [x] |
| 7 | K7子悪魔 COM_94 口上出力 | output | --unit tests/ac/kojo/feature-319/test-319-K7.json | contains | DATALIST output | [x] |
| 8 | K8チルノ COM_94 口上出力 | output | --unit tests/ac/kojo/feature-319/test-319-K8.json | contains | DATALIST output | [x] |
| 9 | K9大妖精 COM_94 口上出力 | output | --unit tests/ac/kojo/feature-319/test-319-K9.json | contains | DATALIST output | [x] |
| 10 | K10魔理沙 COM_94 口上出力 | output | --unit tests/ac/kojo/feature-319/test-319-K10.json | contains | DATALIST output | [x] |
| 11 | Build succeeds | build | - | succeeds | - | [x] |
| 12 | Regression tests pass | output | --flow tests/regression/ | contains | "passed (100%)" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_94 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-03 | DEVIATION | eratw-reader | COM_94 抽出 | ERR:file_not_found - 環境変数読み取り失敗 |
| 2026-01-03 | WORKAROUND | eratw-reader | パス明示で再dispatch | OK:cached |
| 2026-01-03 | ROOT_CAUSE | eratw-reader | 調査完了 | Tool Capability Mismatch: tools に Bash がないため環境変数取得不可 |
| 2026-01-03 | DEVIATION | Bash | kojo_test_gen.py | exit code 1: --output-dir required |
| 2026-01-03 | DEVIATION | Bash | kojo_test_gen.py | exit code 1: Unsupported COM: 94 |
| 2026-01-03 | ADHOC_FIX | - | COM_FILE_MAP.json 編集 | COM_94 追加 |
| 2026-01-03 | ROOT_CAUSE | - | 調査完了 | F320 実装漏れ: 思想は「将来COM全て含める」だが ranges が 90-93 で止まっていた |
| 2026-01-03 | ADHOC_FIX | - | SKILL.md 編集 | ファイル名参照を小文字→大文字に修正 (誤修正) |
| 2026-01-03 | ROOT_CAUSE | - | 調査完了 | Glob が COM_FILE_MAP.json (大文字) を返したが Git 追跡名は com_file_map.json (小文字) |
| 2026-01-03 | REVERT | - | SKILL.md 編集 | 元の小文字 com_file_map.json に戻した |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)

### Follow-up Features (発見された課題)

| ID | 課題 | Root Cause |
|:--:|------|-----------|
| 322 | eratw-reader Bash 追加 | tools に Bash がなく環境変数取得不可 |
| 323 | COM_FILE_MAP 全範囲対応 | F320 実装漏れで 90-93 止まり |
