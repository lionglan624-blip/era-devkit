# Feature 110: COM_310 尻を撫でる 口上 (Phase 8)

## Status: [DONE]

## Type: kojo

## Background

### Problem
COM_310 (尻を撫でる) lacks Phase 8 quality dialogue for all characters.

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
| 1 | K1美鈴 | output | contains | "もっと、触ってもいいのよ" | [0] | [x] |
| 2 | K2小悪魔 | output | contains | "悪魔のお尻、お気に召しましたか" | [0] | [x] |
| 3 | K3パチュリー | output | contains | "その手付き……いやらしいわ" | [0] | [x] |
| 4 | K4咲夜 | output | contains | "瀟洒なメイドとは思えぬ、熱を帯びた吐息が漏れた" | [0] | [x] |
| 5 | K5レミリア | output | contains | "紅魔館の主の身体を、こんな風に触れるのはお前だけよ" | [0] | [x] |
| 6 | K6フラン | output | contains | "私のお尻、%CALLNAME:MASTER%専用だからね" | [0] | [x] |
| 7 | K7子悪魔 | output | contains | "あたしのお尻で喜んでくれるなら、いいの" | [0] | [x] |
| 8 | K8チルノ | output | contains | "最強のあたいが許可してあげるんだから" | [0] | [x] |
| 9 | K9大妖精 | output | contains | "でも、%CALLNAME:MASTER%になら…いいよ" | [0] | [x] |
| 10 | K10魔理沙 | output | contains | "私の全部、お前のものだからな" | [0] | [x] |
| 11 | Build | build | succeeds | - | - | [x] |
| 12 | Regression | output | contains | "passed (100%)" | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-10 | K1-K10 COM_310 口上作成 (4分岐×4パターン) | [x] |
| 2 | 11 | ビルド確認 | [x] |
| 3 | 12 | 回帰テスト | [x] |
| 4 | 1-10 | AC検証 | [x] |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-18 20:30 | finalizer | Feature 110 | DONE (completion verified) |

---

## Links

- [index-features.md](index-features.md)
- [kojo-reference.md](reference/kojo-reference.md)
