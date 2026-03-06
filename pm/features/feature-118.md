# Feature 118: COM_1 クンニ 口上 (Phase 8)

## Status: [DONE]

## Type: kojo

## Background

### Problem
COM_1 (クンニ) lacks Phase 8 quality dialogue for all characters.

### Goal
Create Phase 8 quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8: 全COM網羅 + 品質改修
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character

---

## Acceptance Criteria

| AC# | Char | Type | Matcher | Expected | MockRand | Status |
|:---:|------|------|---------|----------|:--------:|:------:|
| 1 | K1美鈴 | output | contains | "そこ……気持ちいい……もっと、して……" | [0] | [x] |
| 2 | K2小悪魔 | output | contains | "悪魔の弱点を知ってしまうなんて" | [0] | [x] |
| 3 | K3パチュリー | output | contains | "動かない大図書館が、あなたにだけは動かされてしまうのね" | [0] | [x] |
| 4 | K4咲夜 | output | contains | "メイドがこんな姿を見せるなんて" | [0] | [x] |
| 5 | K5レミリア | output | contains | "私の下僕になったつもり" | [0] | [x] |
| 6 | K6フラン | output | contains | "私……壊れちゃいそう……でも、嬉しい壊れ方だよ" | [0] | [x] |
| 7 | K7子悪魔 | output | contains | "えへへ、こうしてるの、幸せ……もっと、なめて" | [0] | [x] |
| 8 | K8チルノ | output | contains | "あたいのこと、もっと好きにして" | [0] | [x] |
| 9 | K9大妖精 | output | contains | "そこ……気持ちいいよ" | [0] | [x] |
| 10 | K10魔理沙 | output | contains | "お前の舌、熱いぜ" | [0] | [x] |
| 11 | Build | build | succeeds | - | - | [x] |
| 12 | Regression | output | contains | "passed (100%)" | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_1 口上作成 (4分岐×4パターン) | [○] |
| 2 | 11 | ビルド確認 | [○] |
| 3 | 12 | 回帰テスト | [○] |
| 4 | 1-10 | AC検証 | [○] |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-19 | finalizer | Feature 118 completion | DONE (all AC passed) |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
