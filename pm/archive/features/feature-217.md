# Feature 217: Kojo ABL/TALENT/EXP Branch Integration

## Status: [DONE]

## Type: erb

## Depends: F216

## Background

### Philosophy (思想・上位目標)

口上は単なるTALENT分岐（思慕/恋慕/恋人）だけでなく、キャラクターの感覚・感度・中毒・経験値を反映した多層的な分岐を持つべき。システムが蓄積する変数は口上にも反映されるべきである。

### Problem (現状の問題)

1. F216 で GET_ABL_BRANCH / GET_TALENT_BRANCH / GET_EXP_BRANCH を実装したが、実際の口上で使用されていない
2. 口上は現在 TALENT 分岐（思慕/恋慕/恋人）のみで、ABL/EXP による分岐がない
3. キャラクターの成長・経験が口上に反映されない

### Goal (このFeatureで達成すること)

1. 既存口上ファイルに ABL/EXP 分岐関数の **呼び出しコード** を追加
2. 対象COM: 愛撫系（感覚分岐に適する）
3. 口上テキスト作成は別Feature（本Featureはコード構造のみ）

---

## Scope

### 対象COM

| COM | 名称 | 分岐タイプ | 対象キャラ | 理由 |
|-----|------|-----------|-----------|------|
| COM_6 | 胸愛撫 | ABL:3 (Ｂ感覚) | K1-K10 | 胸感度に応じた反応 |
| COM_3 | 指挿れ | ABL:1 (Ｖ感覚), EXP:1 (Ｖ経験) | K1のみ | 膣の感度・経験 |
| COM_5 | アナル愛撫 | ABL:2 (Ａ感覚) | K1のみ | 肛門感度に応じた反応 |

> **ABL番号**: 0=Ｃ感覚, 1=Ｖ感覚, 2=Ａ感覚, 3=Ｂ感覚
> **EXP番号**: 0=Ｃ経験, 1=Ｖ経験, 2=Ａ経験, 3=Ｂ経験
> **Note**: COM_3/COM_5 は K1 を代表として先行実装。K2-K10 への拡大は後続 Feature で対応。

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | K1 COM_6 にGET_ABL_BRANCH呼び出し | code | grep | contains | GET_ABL_BRANCH(MASTER, 3) | [x] |
| 2 | K1 COM_3 にGET_ABL_BRANCH呼び出し | code | grep | contains | GET_ABL_BRANCH(MASTER, 1) | [x] |
| 3 | K1 COM_3 にGET_EXP_BRANCH呼び出し | code | grep | contains | GET_EXP_BRANCH(MASTER, 1) | [x] |
| 4 | K1 COM_5 にGET_ABL_BRANCH呼び出し | code | grep | contains | GET_ABL_BRANCH(MASTER, 2) | [x] |
| 5 | K2-K10 COM_6 分岐コード (9ファイル) | code | grep -l | equals | 9 | [x] |
| 6 | ErbLinter エラーなし | exit_code | lint | succeeds | - | [x] |
| 7 | Regression PASS | exit_code | --flow | succeeds | - | [x] |

<!-- AC5 1:N Exception: K2-K10 は同一パターン適用のため集約検証。個別失敗時は Task5 で対応。 -->

### AC Details

**AC1**: `grep "GET_ABL_BRANCH(MASTER, 3)" Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB` (COM_6関数内)
**AC2**: `grep "GET_ABL_BRANCH(MASTER, 1)" Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB` (COM_3関数内)
**AC3**: `grep "GET_EXP_BRANCH(MASTER, 1)" Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB` (COM_3関数内)
**AC4**: `grep "GET_ABL_BRANCH(MASTER, 2)" Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB` (COM_5関数内)
**AC5**: `grep -rl "GET_ABL_BRANCH" Game/ERB/口上/[2-9]_*/KOJO_*_愛撫.ERB Game/ERB/口上/10_*/KOJO_*_愛撫.ERB | wc -l` → 出力が "9" であること
**AC6**: `dotnet run --project tools/ErbLinter -- Game/ERB` (exit 0 = PASS)
**AC7**: `dotnet run --project ../engine/uEmuera.Headless.csproj -- Game --flow tests/` (exit 0 = PASS)

> **Note**: 静的コード検証 (grep) はコード存在確認のため Pos-only で十分。erb type の Neg 要件は実行時動作検証に適用されるが、本 Feature はコード構造追加のみのため対象外。

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | 1 | K1 COM_6 ABL分岐コード追加 | ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB | [x] |
| 2 | 2 | K1 COM_3 ABL分岐コード追加 | ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB | [x] |
| 3 | 3 | K1 COM_3 EXP分岐コード追加 | ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB | [x] |
| 4 | 4 | K1 COM_5 ABL分岐コード追加 | ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB | [x] |
| 5 | 5 | K2-K10 COM_6 分岐コード追加 | ERB/口上/{2-10}_*/KOJO_K*_愛撫.ERB (9 files) | [x] |
| 6 | 6 | ErbLinter 構文検証 実行 | - | [x] |
| 7 | 7 | Regression テスト 実行 | - | [x] |

---

## Review Notes

- **2025-12-26**: F216 の残課題として作成。分岐関数を実際の口上で使用。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-26 | All ACs verified | finalizer | Mark DONE | READY_TO_COMMIT |

---

## Links

- [feature-216.md](feature-216.md) - 分岐関数実装（前提）
- [feature-215.md](feature-215.md) - 分岐候補マトリクス調査

---

## Notes

- F216 で実装した関数を使用
- 本Featureは分岐コード構造のみ追加（口上テキストは別Feature）
- implementer が実装担当（erb type）
