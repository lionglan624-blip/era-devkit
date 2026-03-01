# Feature 325: COM_96 クンニされる 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem
COM_96 (クンニされる) lacks Phase 8d quality dialogue for all characters.

### Goal
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8d: 全COM網羅 + 品質改修
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character
- COM特性: TCVAR:116 = TARGET (相手が行為者), MASTER がクンニを受ける側

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | K1美鈴 COM_96 口上出力 | output | --unit tests/ac/kojo/feature-325/test-325-K1.json | contains | DATALIST output | [x] |
| 2 | K2小悪魔 COM_96 口上出力 | output | --unit tests/ac/kojo/feature-325/test-325-K2.json | contains | DATALIST output | [x] |
| 3 | K3パチュリー COM_96 口上出力 | output | --unit tests/ac/kojo/feature-325/test-325-K3.json | contains | DATALIST output | [x] |
| 4 | K4咲夜 COM_96 口上出力 | output | --unit tests/ac/kojo/feature-325/test-325-K4.json | contains | DATALIST output | [x] |
| 5 | K5レミリア COM_96 口上出力 | output | --unit tests/ac/kojo/feature-325/test-325-K5.json | contains | DATALIST output | [x] |
| 6 | K6フラン COM_96 口上出力 | output | --unit tests/ac/kojo/feature-325/test-325-K6.json | contains | DATALIST output | [x] |
| 7 | K7子悪魔 COM_96 口上出力 | output | --unit tests/ac/kojo/feature-325/test-325-K7.json | contains | DATALIST output | [x] |
| 8 | K8チルノ COM_96 口上出力 | output | --unit tests/ac/kojo/feature-325/test-325-K8.json | contains | DATALIST output | [x] |
| 9 | K9大妖精 COM_96 口上出力 | output | --unit tests/ac/kojo/feature-325/test-325-K9.json | contains | DATALIST output | [x] |
| 10 | K10魔理沙 COM_96 口上出力 | output | --unit tests/ac/kojo/feature-325/test-325-K10.json | contains | DATALIST output | [x] |
| 11 | Build succeeds | build | - | succeeds | - | [x] |
| 12 | Regression tests pass | output | --flow tests/regression/ | contains | "passed (100%)" | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | K1美鈴 COM_96 口上作成 | [x] |
| 2 | 2 | K2小悪魔 COM_96 口上作成 | [x] |
| 3 | 3 | K3パチュリー COM_96 口上作成 | [x] |
| 4 | 4 | K4咲夜 COM_96 口上作成 | [x] |
| 5 | 5 | K5レミリア COM_96 口上作成 | [x] |
| 6 | 6 | K6フラン COM_96 口上作成 | [x] |
| 7 | 7 | K7子悪魔 COM_96 口上作成 | [x] |
| 8 | 8 | K8チルノ COM_96 口上作成 | [x] |
| 9 | 9 | K9大妖精 COM_96 口上作成 | [x] |
| 10 | 10 | K10魔理沙 COM_96 口上作成 | [x] |
| 11 | 11 | ビルド確認 | [x] |
| 12 | 12 | 回帰テスト | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-04 | DEVIATION | eratw-reader | COM_96 cache | ERR:section_not_found → **System fix**: eratw-reader.md Fallback追加 |
| 2026-01-04 | Implementation | kojo-writer x10 | K1-K10 COM_96 parallel | SUCCESS |
| 2026-01-04 | DEVIATION | Grep | RETURN 0 count check | 曖昧結果 → do.md記載済み、記録忘れ |
| 2026-01-04 | Test Gen | kojo_test_gen.py | 160 tests generated | SUCCESS |
| 2026-01-04 | Verification | ac-tester | 160/160 passed | SUCCESS |
| 2026-01-04 | Regression | regression-tester | 24/24 passed | SUCCESS |
| 2026-01-04 | DEVIATION | feature-reviewer | post + doc-check | NEEDS_REVISION x2 (正常レビューフロー) |
| 2026-01-04 | System fix | do.md | Deviation Resolution Principle | 追加完了 |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
