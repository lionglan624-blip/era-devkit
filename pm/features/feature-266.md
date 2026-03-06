# Feature 266: K4 COM 140-145, 180-183 口上新規実装

## Status: [DONE]

## Type: kojo

## Background

### Philosophy (Mid-term Vision)

F261 全 ERB 完全調査の結果、K4 (咲夜) の口挿入口上 18 関数が空スタブ状態であり、SOURCE 要件を満たす内容が存在しないことが判明した。
SOURCE 定義に合致する口上内容を新規実装し、Phase 8d 品質を保証する。

### Problem (Current Issue)

F261 Phase ② 監査で以下の COM が「空スタブにより SOURCE 要件未充足」と検出:

| COM | Function | Issue |
|:---:|----------|-------|
| 140 | @KOJO_MESSAGE_COM_K4_140_1 | SOURCE GET_STAINCOUNT 要件未充足 |
| 141 | @KOJO_MESSAGE_COM_K4_141, _1 | SOURCE 1200 要件未充足 |
| 142 | @KOJO_MESSAGE_COM_K4_142, _1 | SOURCE 1200 要件未充足 |
| 143 | @KOJO_MESSAGE_COM_K4_143, _1 | SOURCE 2000 要件未充足 |
| 144 | @KOJO_MESSAGE_COM_K4_144, _1 | SOURCE 700 要件未充足 |
| 145 | @KOJO_MESSAGE_COM_K4_145, _1 | SOURCE GET_STAINCOUNT 要件未充足 |
| 180 | @KOJO_MESSAGE_COM_K4_180, _1 | SOURCE 100 要件未充足 |
| 181 | @KOJO_MESSAGE_COM_K4_181, _1 | SOURCE 1000 要件未充足 |
| 182 | @KOJO_MESSAGE_COM_K4_182, _1 | SOURCE 150 要件未充足 |
| 183 | @KOJO_MESSAGE_COM_K4_183 | SOURCE 恐怖/逸脱/反感 要件未充足 |

**Total**: 18 functions

### Goal (What to Achieve)

各関数に Phase 8d 品質の口上を新規実装し、COMF 定義の SOURCE に合致する内容を作成。

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | COM 140 イラマチオ 口上出力 | output | --unit | matches | (不潔\|汚\|臭) | [x] |
| 2 | COM 141 フィストファック 口上出力 | output | --unit | matches | (膣\|挿入\|拡) | [x] |
| 3 | COM 142 アナルフィスト 口上出力 | output | --unit | matches | (アナル\|尻\|拡) | [x] |
| 4 | COM 143 両穴フィスト 口上出力 | output | --unit | matches | (両穴\|膣.*尻) | [x] |
| 5 | COM 144 放尿 口上出力 | output | --unit | matches | (羞恥\|晒\|逸脱) | [x] |
| 6 | COM 145 アナル奉仕 口上出力 | output | --unit | matches | (不潔\|汚\|臭) | [x] |
| 7 | COM 180 ローション 口上出力 | output | --unit | matches | (ローション\|塗\|滑) | [x] |
| 8 | COM 181 媚薬 口上出力 | output | --unit | matches | (媚薬\|薬\|異常) | [x] |
| 9 | COM 182 利尿剤 口上出力 | output | --unit | matches | (利尿\|薬\|尿) | [x] |
| 10 | COM 183 ビデオカメラ 口上出力 | output | --unit | matches | (カメラ\|撮影\|恐怖) | [x] |
| 11 | ビルド成功 | build | - | succeeds | dotnet build | [x] |
| 12 | 回帰テスト PASS | test | --flow | succeeds | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | COM 140 イラマチオ 口上新規実装 (TALENT 4分岐, 各4行以上, DATALIST 4パターン) | [○] |
| 2 | 2 | COM 141 フィストファック 口上新規実装 (TALENT 4分岐, 各4行以上, DATALIST 4パターン) | [○] |
| 3 | 3 | COM 142 アナルフィスト 口上新規実装 (TALENT 4分岐, 各4行以上, DATALIST 4パターン) | [○] |
| 4 | 4 | COM 143 両穴フィスト 口上新規実装 (TALENT 4分岐, 各4行以上, DATALIST 4パターン) | [○] |
| 5 | 5 | COM 144 放尿 口上新規実装 (TALENT 4分岐, 各4行以上, DATALIST 4パターン) | [○] |
| 6 | 6 | COM 145 アナル奉仕 口上新規実装 (TALENT 4分岐, 各4行以上, DATALIST 4パターン) | [○] |
| 7 | 7 | COM 180 ローション 口上新規実装 (TALENT 4分岐, 各4行以上, DATALIST 4パターン) | [○] |
| 8 | 8 | COM 181 媚薬 口上新規実装 (TALENT 4分岐, 各4行以上, DATALIST 4パターン) | [○] |
| 9 | 9 | COM 182 利尿剤 口上新規実装 (TALENT 4分岐, 各4行以上, DATALIST 4パターン) | [○] |
| 10 | 10 | COM 183 ビデオカメラ 口上新規実装 (TALENT 4分岐, 各4行以上, DATALIST 4パターン) | [○] |
| 11 | 11 | ビルド確認 | [○] |
| 12 | 12 | 回帰テスト実行 | [○] |

---

## Pre-work

COM 140-145, 180-183 の COMF 定義を確認し、SOURCE 要件を把握すること。

### PLAYER/TARGET 情報 (必須)

| COM | コマンド名 | TCVAR:116 | 備考 |
|:---:|----------|-----------|------|
| 140 | イラマチオ | TARGET | K4 が奉仕側 (行為者=K4) |
| 141 | フィストファック | PLAYER | 調教者が挿入側 (行為者=調教者) |
| 142 | アナルフィスト | PLAYER | 調教者が挿入側 (行為者=調教者) |
| 143 | 両穴フィスト | PLAYER | 調教者が挿入側 (行為者=調教者) |
| 144 | 放尿 | PLAYER | 調教者指示 (行為者=調教者) |
| 145 | アナル奉仕 | TARGET | K4 が奉仕側 (行為者=K4) |
| 180 | ローション | PLAYER | 調教者が塗布 (行為者=調教者) |
| 181 | 媚薬 | PLAYER | 調教者が投与 (行為者=調教者) |
| 182 | 利尿剤 | PLAYER | 調教者が投与 (行為者=調教者) |
| 183 | ビデオカメラ | PLAYER | 調教者が撮影 (行為者=調教者) |

**NOTE**: TCVAR:116 = TARGET の場合、K4 が口上の主語 (K4 視点)。TCVAR:116 = PLAYER の場合、調教者が行為者 (K4 は被行為者)。

### SOURCE 情報 (参考)

| SOURCE | 口上での表現 | 備考 |
|--------|-------------|------|
| 不潔 (GET_STAINCOUNT) | 汚れ・臭いの描写 | 参考情報 |
| 快V/快A | 快感描写 | 参考情報 |
| 露出/逸脱 | 羞恥・異常描写 | 参考情報 |
| 液体 | ローション/濡れ描写 | 参考情報 |
| 恐怖/反感 | 恐怖・嫌悪描写 | 参考情報 |

**NOTE**: PLAYER/TARGET は必須確認。SOURCE/COM名は参考情報 (kojo-writing SKILL)。

### COM別SOURCE要件

| COM | コマンド名 | 主要SOURCE |
|:---:|----------|-----------|
| 140 | イラマチオ | 不潔 (GET_STAINCOUNT) |
| 141 | フィストファック | 快V = 200, 屈従 = 1200 |
| 142 | アナルフィスト | 快A = 200, 屈従 = 1200 |
| 143 | 両穴フィスト | 快V = 200, 快A = 200, 屈従 = 2000 |
| 144 | 放尿 | 露出 = 800, 逸脱 = 700 (快SOURCE なし) |
| 145 | アナル奉仕 | 不潔 (GET_STAINCOUNT) |
| 180 | ローション | 液体 = 5000, 露出 = 100 |
| 181 | 媚薬 | 逸脱 = 1000 |
| 182 | 利尿剤 | 逸脱 = 800, 反感 = 150 |
| 183 | ビデオカメラ | 恐怖 = 1000, 逸脱 = 400, 反感 = 700 |

---

## Links

- [feature-261.md](feature-261.md) - 全ERB完全調査 (本 Feature の入力)
- [feature-262.md](feature-262.md) - ファイル配置修正 (A カテゴリ)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md) - 口上実装手順
- [testing SKILL](../../.claude/skills/testing/SKILL.md) - AC検証手順
