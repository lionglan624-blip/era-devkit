# Feature 127: COM_7 乳首責め 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Problem
COM_7 (乳首責め) lacks Phase 8d quality dialogue for all characters.

### Goal
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8d: 全COM網羅 + 品質改修
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character

---

## Acceptance Criteria

| AC# | Char | Type | Matcher | Expected | MockRand | Status |
|:---:|------|------|---------|----------|:--------:|:------:|
| 1 | K1美鈴 | output | contains | "そこ、敏感なの……" | [0] | [x] |
| 2 | K2小悪魔 | output | contains | "乳首ばかり……いじわるですわ" | [0] | [x] |
| 3 | K3パチュリー | output | contains | "そこ、敏感なの……知ってるでしょう……もっと、して" | [0] | [x] |
| 4 | K4咲夜 | output | contains | "弱いって……知っているくせに" | [0] | [x] |
| 5 | K5レミリア | output | contains | "私のこと、好きにしていいわよ" | [0] | [x] |
| 6 | K6フラン | output | contains | "そこ、すごく……気持ちいいのさ" | [0] | [x] |
| 7 | K7子悪魔 | output | contains | "乳首……いじられてる" | [0] | [x] |
| 8 | K8チルノ | output | contains | "そこ、敏感なんだから" | [0] | [x] |
| 9 | K9大妖精 | output | contains | "そこ、弱いの" | [0] | [x] |
| 10 | K10魔理沙 | output | contains | "そこ、弱いって知ってるくせに" | [0] | [x] |
| 11 | Build | build | succeeds | - | - | [x] |
| 12 | Regression | output | contains | "passed (100%)" | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_7 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-19 | eratw-reader | COM_7 cache | OK:cached |
| 2025-12-19 | kojo-writer x10 | K1-K10 dialogue | OK:10/10 |
| 2025-12-19 | regression-tester | Full suite | OK:109/109 |
| 2025-12-19 | ac-tester | AC verification | PASS:10/10 |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
