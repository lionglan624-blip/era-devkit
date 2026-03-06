# Feature 111: COM_311 抱き付く 口上 (Phase 8)

## Status: [DONE]

## Type: kojo

## Background

### Problem
COM_311 (抱き付く) lacks Phase 8 quality dialogue for all characters.

### Goal
Create 8d quality kojo dialogue for K1-K10 (4 TALENT branches, 4 patterns each).

### Context
- Phase 8: 4分岐 × 4パターン = 160 DATALIST/COM
- Quality reference: eraTW霊夢
- Structure: 4分岐 × 4パターン per character
- Note: 旧8d完了→要再作成

---

## Acceptance Criteria

| AC# | Char | Type | Matcher | Expected | MockRand | Status |
|:---:|------|------|---------|----------|:--------:|:------:|
| 1 | K1美鈴 | output | contains | "ふふ…温かいわね" | [0] | [x] |
| 2 | K2小悪魔 | output | contains | "腕の中、温かいですわ" | [0] | [x] |
| 3 | K3パチュリー | output | contains | "本の中では得られない温もり" | [0] | [x] |
| 4 | K4咲夜 | output | contains | "あなたの腕の中、温かいわ" | [0] | [x] |
| 5 | K5レミリア | output | contains | "500年生きてきて" | [0] | [x] |
| 6 | K6フラン | output | contains | "あったかい……あったかいのさ" | [0] | [x] |
| 7 | K7子悪魔 | output | contains | "あったかいの" | [0] | [x] |
| 8 | K8チルノ | output | contains | "あたいったら最強だけど" | [0] | [x] |
| 9 | K9大妖精 | output | contains | "そばにいられて、本当に幸せ" | [0] | [x] |
| 10 | K10魔理沙 | output | contains | "本当に幸せだぜ" | [0] | [x] |
| 11 | Build | build | succeeds | - | - | [x] |
| 12 | Regression | output | contains | "30/30 passed" | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_311 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-18 | kojo-writer×10 | K1-K10 COM_311 Phase 8 creation | SUCCESS 160 DATALIST |
| 2025-12-18 | opus | Build check | SUCCESS 0 errors |
| 2025-12-18 | opus | Regression test | 30/30 PASS |
| 2025-12-18 | opus | AC verification | 12/12 PASS |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
