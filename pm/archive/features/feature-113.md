# Feature 113: COM_313 胸愛撫 口上 (Phase 8)

## Status: [DONE]

## Type: kojo

## Background

### Problem
COM_313 (胸愛撫) lacks Phase 8 quality dialogue for all characters.

### Goal
Create Phase 8 quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8: 4分岐 × 4パターン = 160 DATALIST
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character (160 DATALIST total)

---

## Acceptance Criteria

| AC# | Char | Type | Matcher | Expected | MockRand | Status |
|:---:|------|------|---------|----------|:--------:|:------:|
| 1 | K1美鈴 | output | contains | "そこ、気持ちいい" | [0] | [x] |
| 2 | K2小悪魔 | output | contains | "悪魔の体、" | [0] | [x] |
| 3 | K3パチュリー | output | contains | "あなたの手、暖かいわね" | [0] | [x] |
| 4 | K4咲夜 | output | contains | "そんなに優しく触られると" | [0] | [x] |
| 5 | K5レミリア | output | contains | "私の体は貴方だけのものよ" | [0] | [x] |
| 6 | K6フラン | output | contains | "フランの体、" | [0] | [x] |
| 7 | K7子悪魔 | output | contains | "あたしの胸、小さいけど" | [0] | [x] |
| 8 | K8チルノ | output | contains | "最強のあたいが、" | [0] | [x] |
| 9 | K9大妖精 | output | contains | "小さな胸が" | [0] | [x] |
| 10 | K10魔理沙 | output | contains | "私の、全部、お前のものだからな" | [0] | [x] |
| 11 | Build | build | succeeds | - | - | [x] |
| 12 | Regression | output | contains | "10/10 passed" | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_313 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-18 | ac-tester | AC1-12 verification | 12/12 PASS |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)

