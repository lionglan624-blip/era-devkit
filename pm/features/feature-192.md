# Feature 192: COM_10 乳首吸い 口上 (Phase 8d)

## Status: [DONE]

## Type: kojo

## Background

### Problem
COM_10 (乳首吸い) lacks Phase 8d quality dialogue for all characters.

### Goal
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8d: 全COM網羅 + 品質改修
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character
- COM_10: 主人公がキャラの乳首を吸う行為

---

## Acceptance Criteria

| AC# | Char | Type | Matcher | Expected | MockRand | Status |
|:---:|------|------|---------|----------|:--------:|:------:|
| 1 | K1美鈴 | output | contains | "んっ……あなた、そこ……気持ちいい……" | [0] | [x] |
| 2 | K2小悪魔 | output | contains | "悪魔の乳首……特別に味わわせてあげますわ" | [0] | [x] |
| 3 | K3パチュリー | output | contains | "本には書いていない感覚" | [0] | [x] |
| 4 | K4咲夜 | output | contains | "貴方になら……全てを見せられますわ" | [0] | [x] |
| 5 | K5レミリア | output | contains | "あ……んっ……そこ、敏感なのよ" | [0] | [x] |
| 6 | K6フラン | output | contains | "ふふ……あなた、そこ好きなのさ" | [0] | [x] |
| 7 | K7子悪魔 | output | contains | "ふわぁ……あなた、そこ、敏感なとこなの" | [0] | [x] |
| 8 | K8チルノ | output | contains | "あたいの乳首、気持ちいい？" | [0] | [x] |
| 9 | K9大妖精 | output | contains | "わたしの乳首……%CALLNAME:MASTER%のものだよ" | [0] | [x] |
| 10 | K10魔理沙 | output | contains | "お前の乳首、やわらかくて吸いやすいぜ" | [0] | [x] |
| 11 | Build | build | succeeds | - | - | [x] |
| 12 | Regression | output | contains | "passed (100%)" | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_10 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-24 | ac-tester | Verified AC 1-10 (character dialogue) | PASS: 160/160 tests |
| 2025-12-24 | ac-tester | Verified AC 11 (Build) | PASS: Build succeeded (0 warnings, 0 errors) |
| 2025-12-24 | ac-tester | Verified AC 12 (Regression) | PASS: Smoke test (Feature 180) 160/160 passed |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
