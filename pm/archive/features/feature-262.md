# Feature 262: F261 監査結果 - 口上品質修正

## Status: [DONE]

## Type: erb

## Background

### Philosophy (Mid-term Vision)

F261 で実施した全 ERB 完全調査 (1506 関数監査) の結果、46 件の NG が検出された。
これらを修正し、口上システムの品質を保証する。

**F261 Philosophy の継承**:
> 「全ERBを調査済み」と断言できる状態を作る。

本 Feature は F261 の調査結果を実装に反映する第一段階であり、A カテゴリ (ファイル配置修正) を担当する。C/D カテゴリは F265/F266/F267 で対応する。

### Problem (Current Issue)

F261 Phase ② 監査で以下の問題が検出された:

| Category | Issue | Count |
|:--------:|-------|------:|
| A | ファイル配置 NG (正規ファイル以外に配置) | 13 |
| C | SOURCE 整合 NG (SOURCE と内容が不一致) | 28 |
| D | 内容品質 NG (スタブ/空実装/内容不備) | 9 |
| **Total** | | **46** |

**Note**: 一部の関数は複数カテゴリで NG (C+D が重複する 4 件あり)

### Goal (What to Achieve)

1. **A: ファイル配置修正** - 13 関数を正規ファイルに移動
2. **C: SOURCE 整合修正** - 28 関数の内容を SOURCE 定義に合わせて修正
3. **D: 内容品質修正** - 9 関数にコンテンツを実装

---

## NG Items Detail

### A: ファイル配置 NG (13 functions)

| # | File | Function | COM | Chara | Issue | Fix |
|:-:|------|----------|:---:|:-----:|-------|-----|
| A1 | KOJO_K1_乳首責め.ERB | @KOJO_MESSAGE_COM_K1_7 | 7 | K1 | COM 7 は愛撫系 | → _愛撫.ERB |
| A2 | KOJO_K1_乳首責め.ERB | @KOJO_MESSAGE_COM_K1_7_1 | 7 | K1 | COM 7 は愛撫系 | → _愛撫.ERB |
| A3 | KOJO_K2_乳首責め.ERB | @KOJO_MESSAGE_COM_K2_7 | 7 | K2 | COM 7 は愛撫系 | → _愛撫.ERB |
| A4 | KOJO_K2_乳首責め.ERB | @KOJO_MESSAGE_COM_K2_7_1 | 7 | K2 | COM 7 は愛撫系 | → _愛撫.ERB |
| A5 | KOJO_K3_乳首責め.ERB | @KOJO_MESSAGE_COM_K3_7 | 7 | K3 | COM 7 は愛撫系 | → _愛撫.ERB |
| A6 | KOJO_K3_乳首責め.ERB | @KOJO_MESSAGE_COM_K3_7_1 | 7 | K3 | COM 7 は愛撫系 | → _愛撫.ERB |
| A7 | KOJO_K2_口挿入.ERB | @KOJO_MESSAGE_COM_K2_65 | 65 | K2 | COM 65 は挿入系 | → _挿入.ERB |
| A8 | KOJO_K2_口挿入.ERB | @KOJO_MESSAGE_COM_K2_65_1 | 65 | K2 | COM 65 は挿入系 | → _挿入.ERB |
| A9 | KOJO_K2_口挿入.ERB | @KOJO_MESSAGE_COM_K2_66 | 66 | K2 | COM 66 は挿入系 | → _挿入.ERB |
| A10 | KOJO_K2_口挿入.ERB | @KOJO_MESSAGE_COM_K2_66_1 | 66 | K2 | COM 66 は挿入系 | → _挿入.ERB |
| A11 | KOJO_K2_口挿入.ERB | @KOJO_MESSAGE_COM_K2_67 | 67 | K2 | COM 67 は挿入系 | → _挿入.ERB |
| A12 | KOJO_K2_口挿入.ERB | @KOJO_MESSAGE_COM_K2_67_1 | 67 | K2 | COM 67 は挿入系 | → _挿入.ERB |
| A13 | KOJO_K4_会話親密.ERB | @NTR_KOJO_MESSAGE_COM_K4_314 | 314 | K4 | COM 314 は愛撫系 | → _愛撫.ERB |

### C: SOURCE 整合 NG (28 functions)

| # | File | Function | COM | Chara | Issue |
|:-:|------|----------|:---:|:-----:|-------|
| C1-2 | KOJO_K10_日常.ERB | @KOJO_MESSAGE_COM_K10_410, _1 | 410 | K10 | SOURCE 不整合 (A) |
| C3-4 | KOJO_K10_日常.ERB | @KOJO_MESSAGE_COM_K10_414, _1 | 414 | K10 | SOURCE 不整合 (A, B) |
| C5 | KOJO_K1_挿入.ERB | @KOJO_MESSAGE_COM_K1_64 | 64 | K1 | Missing cowgirl description |
| C6 | KOJO_K1_挿入.ERB | @KOJO_MESSAGE_COM_K1_65 | 65 | K1 | Missing cowgirl description |
| C7 | KOJO_K2_会話親密.ERB | @KOJO_MESSAGE_COM_K2_302 | 302 | K2 | SOURCE 不整合 |
| C8 | KOJO_K2_会話親密.ERB | @KOJO_MESSAGE_COM_K2_313 | 313 | K2 | SOURCE 不整合 |
| C9-26 | KOJO_K4_口挿入.ERB | @KOJO_MESSAGE_COM_K4_140-145, 180-183 (×2 each) | 140-183 | K4 | SOURCE 要件未充足 (18 funcs) |
| C27 | KOJO_K6_愛撫.ERB | @KOJO_MESSAGE_COM_K6_43_1 | 43 | K6 | SOURCE:快Ｃ but describes onahole |
| C28 | KOJO_K8_会話親密.ERB | @KOJO_MESSAGE_COM_K8_314 | 314 | K8 | SOURCE 快Ａ but no keywords |

### D: 内容品質 NG (9 functions)

| # | File | Function | COM | Chara | Issue |
|:-:|------|----------|:---:|:-----:|-------|
| D1 | KOJO_K2_会話親密.ERB | @KOJO_MESSAGE_COM_K2_302 | 302 | K2 | C+D overlap |
| D2 | KOJO_K2_会話親密.ERB | @KOJO_MESSAGE_COM_K2_313 | 313 | K2 | C+D overlap |
| D3 | KOJO_K4_会話親密.ERB | @NTR_KOJO_MESSAGE_COM_K4_302 | 302 | K4 | No dialogue (CALL TRAIN_MESSAGE only) |
| D4 | KOJO_K4_会話親密.ERB | @NTR_KOJO_MESSAGE_COM_K4_311 | 311 | K4 | No dialogue (CALL TRAIN_MESSAGE only) |
| D5 | KOJO_K4_会話親密.ERB | @NTR_KOJO_MESSAGE_COM_K4_312 | 312 | K4 | No dialogue (CALL TRAIN_MESSAGE only) |
| D6 | KOJO_K4_会話親密.ERB | @NTR_KOJO_MESSAGE_COM_K4_314 | 314 | K4 | No anal caress description + A overlap |
| D7 | KOJO_K4_愛撫.ERB | @KOJO_MESSAGE_COM_K4_00 | 0 | K4 | Empty stub |
| D8 | KOJO_K8_会話親密.ERB | @KOJO_MESSAGE_COM_K8_313 | 313 | K8 | No meaningful content |
| D9 | KOJO_K8_会話親密.ERB | @KOJO_MESSAGE_COM_K8_314 | 314 | K8 | No meaningful content + C overlap |

---

## Scope Decision

**Volume Analysis**:

| Category | Functions | Effort |
|:--------:|----------:|:------:|
| A (移動) | 13 | Low (mechanical) |
| C (内容修正) | 28 | High (kojo rewrite) |
| D (新規実装) | 9 | High (kojo creation) |

**Recommendation**: A カテゴリ (ファイル配置修正) のみを本 Feature で実施。
C/D カテゴリ (内容修正/新規実装) は別 Feature として分離することを推奨。

**Reason**:
- A は erb-type (コード移動) で完結
- C/D は kojo-type (口上作成) で別の専門性が必要
- Volume limit: erb type は ~500 lines、kojo type は ~2,500 lines

---

## Acceptance Criteria

### AC Definition Table (A Category Only)

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | K1 COM 7 が愛撫.ERB に存在 | code | Grep | exists | @KOJO_MESSAGE_COM_K1_7 in KOJO_K1_愛撫.ERB | [ ] |
| 2 | K1 COM 7 が乳首責め.ERB に非存在 | code | Grep | not_exists | @KOJO_MESSAGE_COM_K1_7 in KOJO_K1_乳首責め.ERB | [ ] |
| 3 | K2 COM 7 が愛撫.ERB に存在 | code | Grep | exists | @KOJO_MESSAGE_COM_K2_7 in KOJO_K2_愛撫.ERB | [ ] |
| 4 | K2 COM 7 が乳首責め.ERB に非存在 | code | Grep | not_exists | @KOJO_MESSAGE_COM_K2_7 in KOJO_K2_乳首責め.ERB | [ ] |
| 5 | K3 COM 7 が愛撫.ERB に存在 | code | Grep | exists | @KOJO_MESSAGE_COM_K3_7 in KOJO_K3_愛撫.ERB | [ ] |
| 6 | K3 COM 7 が乳首責め.ERB に非存在 | code | Grep | not_exists | @KOJO_MESSAGE_COM_K3_7 in KOJO_K3_乳首責め.ERB | [ ] |
| 7 | K2 COM 65-67 が挿入.ERB に存在 | code | Grep | exists | @KOJO_MESSAGE_COM_K2_6[567] in KOJO_K2_挿入.ERB | [ ] |
| 8 | K2 COM 65-67 が口挿入.ERB に非存在 | code | Grep | not_exists | @KOJO_MESSAGE_COM_K2_6[567] in KOJO_K2_口挿入.ERB | [ ] |
| 9 | K4 COM 314 NTR が愛撫.ERB に存在 | code | Grep | exists | @NTR_KOJO_MESSAGE_COM_K4_314 in KOJO_K4_愛撫.ERB | [ ] |
| 10 | K4 COM 314 NTR が会話親密.ERB に非存在 | code | Grep | not_exists | @NTR_KOJO_MESSAGE_COM_K4_314 in KOJO_K4_会話親密.ERB | [ ] |
| 11 | ビルド成功 | build | Bash | succeeds | dotnet build | [ ] |
| 12 | 回帰テスト PASS | test | --flow | succeeds | 24/24 scenarios | [ ] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | K1 COM 7 を乳首責め.ERB → 愛撫.ERB に移動 | [ ] |
| 2 | 3,4 | K2 COM 7 を乳首責め.ERB → 愛撫.ERB に移動 | [ ] |
| 3 | 5,6 | K3 COM 7 を乳首責め.ERB → 愛撫.ERB に移動 | [ ] |
| 4 | 7,8 | K2 COM 65-67 を口挿入.ERB → 挿入.ERB に移動 | [ ] |
| 5 | 9,10 | K4 COM 314 NTR を会話親密.ERB → 愛撫.ERB に移動 | [ ] |
| 6 | 11 | ビルド確認 | [ ] |
| 7 | 12 | 回帰テスト実行 | [ ] |

---

## Deferred to Future Features

以下は本 Feature のスコープ外。別 Feature として作成済み:

### F265: 各種口上品質修正 (kojo)

その他の C/D issues (複数キャラ):
- K1 COM 64-65 cowgirl description
- K2 COM 302, 313
- K6 COM 43 SOURCE 修正
- K8 COM 313, 314
- K10 COM 410, 414
- K4 COM 0 実装

### F266: K4 口挿入 SOURCE 修正 (kojo)

18 functions in KOJO_K4_口挿入.ERB with SOURCE requirement issues:
- COM 140-145 (イラマチオ系)
- COM 180-183 (特殊系)

### F267: K4 NTR 口上スタブ実装 (kojo)

K4 NTR 会話親密スタブ実装:
- @NTR_KOJO_MESSAGE_COM_K4_302, 311, 312, 314
- **依存**: F262 (COM 314 配置修正) 完了後
- **Note**: F262 完了後、COM 314 NTR は `KOJO_K4_愛撫.ERB` に配置される

---

## Input Data

**Source**: `.tmp/f261-fix-candidates.jsonl` (F261 Phase ③ output)

---

## Links

- [feature-261.md](feature-261.md) - 全ERB完全調査 (本 Feature の入力)
- [feature-057.md](feature-057.md) - K4 COM統合 (元の分類設計)
- [feature-190.md](feature-190.md) - COM_60 重複解消
- [feature-221.md](feature-221.md) - 挿入/口挿入混乱解消

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-29 | create | opus | F261 Phase ④ として F262 作成 | - |
| 2025-12-29 | init | initializer | Status: PROPOSED → WIP | READY:262:erb |
