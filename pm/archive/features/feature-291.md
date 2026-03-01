# Feature 291: COM_85 足扱き 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem
COM_85 (足扱き) lacks Phase 8d quality dialogue for all characters.

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
| 1 | K1美鈴 COM_85 口上出力 | output | --unit tests/ac/kojo/feature-291/test-291-K1.json | contains | DATALIST output | [x] |
| 2 | K2小悪魔 COM_85 口上出力 | output | --unit tests/ac/kojo/feature-291/test-291-K2.json | contains | DATALIST output | [x] |
| 3 | K3パチュリー COM_85 口上出力 | output | --unit tests/ac/kojo/feature-291/test-291-K3.json | contains | DATALIST output | [x] |
| 4 | K4咲夜 COM_85 口上出力 | output | --unit tests/ac/kojo/feature-291/test-291-K4.json | contains | DATALIST output | [x] |
| 5 | K5レミリア COM_85 口上出力 | output | --unit tests/ac/kojo/feature-291/test-291-K5.json | contains | DATALIST output | [x] |
| 6 | K6フラン COM_85 口上出力 | output | --unit tests/ac/kojo/feature-291/test-291-K6.json | contains | DATALIST output | [x] |
| 7 | K7子悪魔 COM_85 口上出力 | output | --unit tests/ac/kojo/feature-291/test-291-K7.json | contains | DATALIST output | [x] |
| 8 | K8チルノ COM_85 口上出力 | output | --unit tests/ac/kojo/feature-291/test-291-K8.json | contains | DATALIST output | [x] |
| 9 | K9大妖精 COM_85 口上出力 | output | --unit tests/ac/kojo/feature-291/test-291-K9.json | contains | DATALIST output | [x] |
| 10 | K10魔理沙 COM_85 口上出力 | output | --unit tests/ac/kojo/feature-291/test-291-K10.json | contains | DATALIST output | [x] |
| 11 | Build succeeds | build | - | succeeds | - | [x] |
| 12 | Regression tests pass | output | --flow tests/regression/ | contains | "passed (100%)" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_85 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-02 | Init | initializer | Feature initialized | READY |
| 2026-01-02 | Phase 4 | kojo-writer x10 | K1-K10 batch dispatch | 10/10 complete |
| 2026-01-02 | DEVIATION | kojo_test_gen.py | K4 test generation | 0 DATALIST: K4 missing DATAFORM prefixes |
| 2026-01-02 | Analysis | kojo-writer K4 | Resume: 原因確認 | SKILLテンプレート参照、既存コード未参照 |
| 2026-01-02 | Handoff | - | Session handoff | .tmp/session-handoff-F291.md 作成 |
| 2026-01-02 17:12 | Fix | implementer | K4 DATAFORM prefix fix | 16 DATALIST blocks corrected |
| 2026-01-02 17:12 | Verify | implementer | --strict-warnings | PASS (0 warnings) |
| 2026-01-02 | Phase 5-6 | - | Test gen, BOM, Linter, Regression, AC | All PASS |
| 2026-01-02 | Fix | - | K4 KOJO_MODIFIER calls | Added PRE/POST COMMON |
| 2026-01-02 | Verify | - | K4 AC test after MODIFIER fix | 16/16 PASS |
| 2026-01-02 | Follow-up | - | Feature 309-312 作成 | 改善 Feature 4件登録 |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
- Related: [feature-299.md](feature-299.md) - do.md 品質チェック手順 (残課題: complete-feature 統合)

**Note**: F299 残課題 (complete-feature品質チェック統合) 未判断
