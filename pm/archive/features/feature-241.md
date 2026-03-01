# Feature 241: COM_64 逆レイプ 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)
kojo Feature ファイルは SSOT 準拠で他ワークフロー（/do, /fl, ac-tester）と連携する

### Problem
COM_64 (逆レイプ) lacks Phase 8d quality dialogue for all characters.

### Goal
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8d: 全COM網羅 + 品質改修
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character
- File: KOJO_K{N}_挿入.ERB (COM 60-72 range)

---

## Acceptance Criteria

| AC# | Char | Type | Method | Matcher | Expected | MockRand | Status |
|:---:|------|------|--------|---------|----------|:--------:|:------:|
| 1 | K1美鈴 | output | --unit | contains | "{auto}" | [0] | [x] |
| 2 | K2小悪魔 | output | --unit | contains | "{auto}" | [0] | [x] |
| 3 | K3パチュリー | output | --unit | contains | "{auto}" | [0] | [x] |
| 4 | K4咲夜 | output | --unit | contains | "{auto}" | [0] | [x] |
| 5 | K5レミリア | output | --unit | contains | "{auto}" | [0] | [x] |
| 6 | K6フラン | output | --unit | contains | "{auto}" | [0] | [x] |
| 7 | K7子悪魔 | output | --unit | contains | "{auto}" | [0] | [x] |
| 8 | K8チルノ | output | --unit | contains | "{auto}" | [0] | [x] |
| 9 | K9大妖精 | output | --unit | contains | "{auto}" | [0] | [x] |
| 10 | K10魔理沙 | output | --unit | contains | "{auto}" | [0] | [x] |
| 11 | Build | build | - | succeeds | - | - | [x] |
| 12 | Regression | output | - | contains | "passed (100%)" | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_64 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-28 | Phase 1 | initializer | Feature init | READY |
| 2025-12-28 | Phase 2 | explorer | Investigation | READY |
| 2025-12-28 | Phase 4 | kojo-writer×10 | K1-K10 batch dispatch | OK |
| 2025-12-28 | Phase 5 | kojo_test_gen.py | Test generation | 160 tests |
| 2025-12-28 | Phase 6 | regression-tester | Regression test | PASS:24/24 |
| 2025-12-28 | Phase 6 | ac-tester | AC test (1st) | FAIL:112/160 |
| 2025-12-28 | Phase 6 | debugger | Fix duplicate funcs | Fixed |
| 2025-12-28 | Phase 6 | ac-tester | AC test (2nd) | PASS:160/160 |
| 2025-12-28 | Phase 7 | feature-reviewer | Post-review | NEEDS_REVISION |
| 2025-12-28 | Phase 7 | debugger | Remove parent stubs | Fixed |
| 2025-12-28 | Phase 7 | feature-reviewer | Post-review (2nd) | READY |
| 2025-12-28 | Phase 8 | finalizer | Feature completion | DONE |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
