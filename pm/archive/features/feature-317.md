# Feature 317: COM_92 Ａ正常位される 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem
COM_92 (Ａ正常位される) lacks Phase 8d quality dialogue for all characters.

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
| 1 | K1美鈴 COM_92 口上出力 | output | --unit tests/ac/kojo/feature-317/test-317-K1.json | contains | DATALIST output | [x] |
| 2 | K2小悪魔 COM_92 口上出力 | output | --unit tests/ac/kojo/feature-317/test-317-K2.json | contains | DATALIST output | [x] |
| 3 | K3パチュリー COM_92 口上出力 | output | --unit tests/ac/kojo/feature-317/test-317-K3.json | contains | DATALIST output | [x] |
| 4 | K4咲夜 COM_92 口上出力 | output | --unit tests/ac/kojo/feature-317/test-317-K4.json | contains | DATALIST output | [x] |
| 5 | K5レミリア COM_92 口上出力 | output | --unit tests/ac/kojo/feature-317/test-317-K5.json | contains | DATALIST output | [x] |
| 6 | K6フラン COM_92 口上出力 | output | --unit tests/ac/kojo/feature-317/test-317-K6.json | contains | DATALIST output | [x] |
| 7 | K7子悪魔 COM_92 口上出力 | output | --unit tests/ac/kojo/feature-317/test-317-K7.json | contains | DATALIST output | [x] |
| 8 | K8チルノ COM_92 口上出力 | output | --unit tests/ac/kojo/feature-317/test-317-K8.json | contains | DATALIST output | [x] |
| 9 | K9大妖精 COM_92 口上出力 | output | --unit tests/ac/kojo/feature-317/test-317-K9.json | contains | DATALIST output | [x] |
| 10 | K10魔理沙 COM_92 口上出力 | output | --unit tests/ac/kojo/feature-317/test-317-K10.json | contains | DATALIST output | [x] |
| 11 | Build succeeds | build | - | succeeds | - | [x] |
| 12 | Regression tests pass | output | --flow tests/regression/ | contains | "passed (100%)" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_92 口上作成 (4分岐×4パターン) | [O] |
| 2 | 11 | ビルド確認 | [O] |
| 3 | 12 | 回帰テスト | [O] |
| 4 | 1-10 | AC検証 | [O] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 | Phase 1 | initializer | Initialize Feature 317 | READY |
| 2026-01-02 | Phase 4 | kojo-writer x10 | K1-K10 batch dispatch | 10/10 complete |
| 2026-01-02 | Phase 4.1 | - | ERB function existence check | 10/10 exist |
| 2026-01-02 | Phase 4.2 | - | MODIFIER_PRE_COMMON check | K1-K10 confirmed |
| 2026-01-02 | Phase 4.2 | - | MODIFIER_POST_COMMON check | K1-K10 confirmed |
| 2026-01-02 | Phase 4.3 | - | RETURN check | All functions have RETURN RESULT |
| 2026-01-02 | Phase 5 | - | kojo_test_gen.py | 10/10 test JSON generated (160 tests) |
| 2026-01-02 | Phase 6 | - | BOM verification | 117/117 OK |
| 2026-01-02 | Phase 6 | - | ErbLinter | 0 errors, 0 warnings |
| 2026-01-02 | Phase 6 | - | Regression tests | 24/24 passed (100%) |
| 2026-01-02 | Phase 6 | - | AC tests | 160/160 passed |
| 2026-01-02 | Phase 7 | feature-reviewer | Post-review (mode: post) | READY |
| 2026-01-02 | Phase 7 | feature-reviewer | Doc-check | READY |

---

## Session Notes

### セッション中に実施したインフラ改善 (前セッション)
1. **do.md Polling Procedure**: While ループ形式に書き換え (フロー明確化)
2. **kojo-writing SKILL**: 「File Operations」セクション追加
   - 末尾への関数追加には Write を使う (Edit ではなく)
   - 理由: Edit は old_string の一意性確保が困難、リトライ発生

### K6 agent の Edit 苦戦から得た知見
- Edit ツールは「置換」向け、末尾「追記」には不向き
- Write で Read→追記→Write が確実
- kojo-writer SKILL に反映済み

### kojo_test_gen.py 注意点 (今セッション)
- COM_92 は `_挿入.ERB` ファイルに配置 (全キャラ共通)
- ツールの COM_FILE_MAP は `_口挿入.ERB` を期待するが実際は異なる
- 個別ファイル指定で生成: `--function KOJO_MESSAGE_COM_K{N}_92_1` + 直接パス指定

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
