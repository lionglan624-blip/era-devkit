# Feature 265: 各種口上品質修正

## Status: [DONE]

**Completed**: 2025-12-31

## Type: kojo

## Background

### Philosophy (Mid-term Vision)

F261 全 ERB 完全調査の結果、複数キャラで C (SOURCE 整合) / D (内容品質) NG が検出された。
F263 (K4 口挿入) / F264 (K4 NTR) 以外の残件を本 Feature で修正。

### Problem (Current Issue)

F261 Phase ② 監査で以下の NG が検出:

#### C: SOURCE 整合 NG

| Chara | COM | File | Issue |
|:-----:|:---:|------|-------|
| K2 | 302 | KOJO_K2_会話親密.ERB | SOURCE 不整合 |
| K2 | 313 | KOJO_K2_会話親密.ERB | SOURCE 不整合 |
| K6 | 43 | KOJO_K6_愛撫.ERB | SOURCE:快Ｃ but describes onahole |
| K8 | 314 | KOJO_K8_会話親密.ERB | SOURCE 快Ａ but no keywords |
| K10 | 410 | KOJO_K10_日常.ERB | SOURCE 不整合 (A) |
| K10 | 414 | KOJO_K10_日常.ERB | SOURCE 不整合 (A, B) |

#### D: 内容品質 NG

| Chara | COM | File | Issue |
|:-----:|:---:|------|-------|
| K2 | 302, 313 | KOJO_K2_会話親密.ERB | (C と重複) |
| K8 | 314 | KOJO_K8_会話親密.ERB | No meaningful content (C と重複) |

#### Removed (F261 False Positive)

| Chara | COM | File | Reason |
|:-----:|:---:|------|--------|
| K1 | 64 | KOJO_K1_挿入.ERB | Already implemented (騎乗位描写あり) |
| K1 | 65 | KOJO_K1_挿入.ERB | Already implemented (騎乗位描写あり) |
| K8 | 313 | KOJO_K8_会話親密.ERB | Already implemented (TALENT 4分岐実装済み) |
| K4 | 00 | KOJO_K4_愛撫.ERB | `_00` は汎用フォールバック関数であり COM 0 口上ではない (空スタブは正常) |

### Goal (What to Achieve)

各関数の口上内容を修正し、SOURCE 整合性と品質を保証。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | K2 COM 302, 313 検証 | verify | --unit | - | F261 false positive 確認 | [x] |
| 2 | K6 COM 43 主客関係修正 | output | --unit | contains | PLAYER がオナホールを TARGET に使用 | [x] |
| 3 | K8 COM 314 内容実装 | output | --unit | contains | アナル関連キーワード | [x] |
| 4 | K10 COM 410, 414 内容充実 | output | --unit | contains | 歓楽 SOURCE 対応描写 | [x] |
| 5 | ビルド成功 | build | Bash | succeeds | dotnet build | [x] |
| 6 | 回帰テスト PASS | test | --flow | succeeds | 24/24 scenarios | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | K2 COM 302, 313 検証 (F261 FP 確認) | [x] |
| 2 | 2 | K6 COM 43 主客関係修正 (PLAYER→TARGET) | [x] |
| 3 | 3 | K8 COM 314 アナル明示口上実装 | [x] |
| 4 | 4 | K10 COM 410, 414 内容充実 | [x] |
| 5 | 5 | ビルド確認 | [x] |
| 6 | 6 | 回帰テスト実行 | [x] |

---

## Scope Note

本 Feature は複数キャラにまたがる残件を統合。
Volume が大きい場合はキャラ単位で分割を検討。

---

## Links

- [feature-261.md](feature-261.md) - 全ERB完全調査 (本 Feature の入力)
- [feature-262.md](feature-262.md) - ファイル配置修正 (A カテゴリ)
- [feature-266.md](feature-266.md) - K4 口挿入 SOURCE 修正
- [feature-267.md](feature-267.md) - K4 NTR 口上スタブ実装
