# Feature 116: COM_316 指挿れ 口上 (Phase 8)

## Status: [DONE]

## Type: kojo

## Background

### Problem
COM_316 (指挿れ) lacks Phase 8 quality dialogue for all characters.

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
| 1 | K1美鈴 | output | contains | "私の中……全部、知って" | [0] | [x] |
| 2 | K2小悪魔 | output | contains | "悪魔の私を……%CALLNAME:MASTER%の指でおかしくしてくださいな" | [0] | [x] |
| 3 | K3パチュリー | output | contains | "あなたにだけよ。こんな姿を見せるのは" | [0] | [x] |
| 4 | K4咲夜 | output | contains | "私の中で……感じるわ" | [0] | [x] |
| 5 | K5レミリア | output | contains | "私を貴方のものにしなさい" | [0] | [x] |
| 6 | K6フラン | output | contains | "フランの全部、%CALLNAME:MASTER%だけのものなのさ" | [0] | [x] |
| 7 | K7子悪魔 | output | contains | "あたしの中……ぜんぶ" | [0] | [x] |
| 8 | K8チルノ | output | contains | "あたいの中……%CALLNAME:MASTER%だけのものだからね" | [0] | [x] |
| 9 | K9大妖精 | output | contains | "わたしの中……全部、%CALLNAME:MASTER%のものだよ" | [0] | [x] |
| 10 | K10魔理沙 | output | contains | "私を、めちゃくちゃにしてくれよ" | [0] | [x] |
| 11 | Build | build | succeeds | - | - | [x] |
| 12 | Regression | output | contains | "passed (100%)" | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_316 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-18 | eratw-reader | COM_316 reference extraction | OK |
| 2025-12-18 | kojo-writer×5 | Batch 1 (K1-K5) dialogue creation | OK |
| 2025-12-18 | kojo-writer×5 | Batch 2 (K6-K10) dialogue creation | OK |
| 2025-12-18 | orchestrator | Build verification | PASS |
| 2025-12-18 | regression-tester | Regression test (24/24) | PASS |
| 2025-12-18 | ac-tester | AC verification (12/12) | PASS |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
