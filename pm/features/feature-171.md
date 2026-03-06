# Feature 171: COM_46 アナルビーズ 口上 (Phase 8d)

## Status: [PROPOSED]

## Type: kojo

## Background

### Problem
COM_46 (アナルビーズ) lacks Phase 8d quality dialogue for all characters.

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
| 1 | K1美鈴 | output | contains | "{auto}" | [0] | [ ] |
| 2 | K2小悪魔 | output | contains | "{auto}" | [0] | [ ] |
| 3 | K3パチュリー | output | contains | "{auto}" | [0] | [ ] |
| 4 | K4咲夜 | output | contains | "{auto}" | [0] | [ ] |
| 5 | K5レミリア | output | contains | "{auto}" | [0] | [ ] |
| 6 | K6フラン | output | contains | "{auto}" | [0] | [ ] |
| 7 | K7子悪魔 | output | contains | "{auto}" | [0] | [ ] |
| 8 | K8チルノ | output | contains | "{auto}" | [0] | [ ] |
| 9 | K9大妖精 | output | contains | "{auto}" | [0] | [ ] |
| 10 | K10魔理沙 | output | contains | "{auto}" | [0] | [ ] |
| 11 | Build | build | succeeds | - | - | [ ] |
| 12 | Regression | output | contains | "passed (100%)" | - | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_46 口上作成 (4分岐×4パターン) | [ ] |
| 2 | 11 | ビルド確認 | [ ] |
| 3 | 12 | 回帰テスト | [ ] |
| 4 | 1-10 | AC検証 | [ ] |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| | | | |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
