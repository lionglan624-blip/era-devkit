# Feature 121: COM_4 アナル舐め 口上 (Phase 8)

## Status: [DONE]

## Type: kojo

## Background

### Problem
COM_4 (アナル舐め) lacks Phase 8 quality dialogue for all characters.

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
| 1 | K1美鈴 | output | contains | "後ろ、舐められてる" | [0] | [x] |
| 2 | K2小悪魔 | output | contains | "舌……あったかい" | [0] | [x] |
| 3 | K3パチュリー | output | contains | "本にも載っていない背徳行為" | [0] | [x] |
| 4 | K4咲夜 | output | contains | "舐めるなんて……恥ずかしすぎますわ" | [0] | [x] |
| 5 | K5レミリア | output | contains | "こんな恥ずかしいこと……許すのは" | [0] | [x] |
| 6 | K6フラン | output | contains | "私の恥ずかしいところ、全部" | [0] | [x] |
| 7 | K7子悪魔 | output | contains | "そこ……舐めてるの" | [0] | [x] |
| 8 | K8チルノ | output | contains | "あたいのおしり……舐めて気持ちいいの" | [0] | [x] |
| 9 | K9大妖精 | output | contains | "後ろ、舐められてる……なのに……嬉しいの" | [0] | [x] |
| 10 | K10魔理沙 | output | contains | "そこ、舐めるのかよ" | [0] | [x] |
| 11 | Build | build | succeeds | - | - | [x] |
| 12 | Regression | output | contains | "passed (100%)" | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_4 口上作成 (4分岐×4パターン) | [ ] |
| 2 | 11 | ビルド確認 | [ ] |
| 3 | 12 | 回帰テスト | [ ] |
| 4 | 1-10 | AC検証 | [ ] |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-19 | finalizer | Feature completed | 12/12 AC passed |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
