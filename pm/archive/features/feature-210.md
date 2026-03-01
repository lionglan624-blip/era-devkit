# Feature 210: Scenario Input Sequence Fix

## Status: [DONE]

## Type: infra

## Depends: [209, 211]

**Note**: F211 (Empty Line Input Bug Fix) が必須。input ファイルの空行が WAIT 確認として機能しない問題を先に修正する必要がある。

## Background

### Philosophy

**F095/F105/F200-209 の思想を継承**:

1. **回帰テストはゲーム全体の保証として機能する**
   - シナリオが「ゲームロジックが正しい」ことを検証する
   - exit 0 だけでなく、期待した状態変化が起きたかを確認する

2. **状態注入 → コマンド実行 → 期待値検証** のフローが完全に動作する
   - scenario-*.json: 前提状態を注入
   - input-*.txt: コマンドを実行（COM888 でターン終了など）
   - expect.var_equals: 狙い通りの値になったか検証

### Problem

F209 で state 遷移修正を実装したが、テストシナリオが正しく動作しない。

**観察された問題**:
1. `100` (起床) 後に「館に侵入者があったようだ！」イベントが発生
2. このイベントはエンター待ち（WAIT 状態）
3. `888` 入力がエンター確認として消費される
4. COM888 が実行されない → state 遷移が発生しない

**根本原因**:
- input file の順序が実際のゲームフローと一致していない
- ゲーム中のイベント（侵入者イベント等）による追加の input 消費を考慮していない

### Goal

1. sc-002 (思慕→恋慕) シナリオが正しく COM888 を実行
2. sc-004 (NTR陥落) シナリオが正しく COM888 を実行
3. 「一日が終わりました」出力確認（F209 B1）
4. 「[恋慕]を得た」「[NTR]を得た」出力確認（F209 C1-C2）
5. var_equals: PASS（F209 C3）
6. 24/24 PASS（F209 D1）

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | sc-002 input 順序修正 | output | --flow sc-002 | contains | "一日が終わりました" | [x] |
| 2 | sc-002 恋慕獲得 | output | --flow sc-002 | contains | "[恋慕]を得た" | [x] |
| 3 | sc-004 input 順序修正 | output | --flow sc-004 | contains | "一日が終わりました" | [x] |
| 4 | sc-004 NTR陥落 | output | --flow sc-004 | contains | "[NTR]を得た" | [x] |
| 5 | 24/24 PASS | output | --flow | contains | "24/24" | [x] |
| 6 | verify-logs.py 検証成功 | exit_code | verify-logs.py | equals | 0 | [x] |

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | 1,2 | sc-002 input 順序修正・テスト | tests/regression/input-sc-002*.txt | [O] |
| 2 | 3,4 | sc-004 input 順序修正・テスト | tests/regression/input-sc-004*.txt | [O] |
| 3 | 5 | 全24シナリオ回帰テスト | tests/regression/ | [O] |
| 4 | 6 | verify-logs.py 検証 | tools/verify-logs.py | [O] |

---

## Technical Details

### 現在の input シーケンス (sc-002)

```
0     ← ゲーム開始
9     ← クイックスタート
100   ← 起床
888   ← 1日の終了 ← ここで「侵入者」イベントのエンター待ちに消費される
100   ← 確認
```

### 修正方針

1. ゲームフローを実機で確認し、必要な入力シーケンスを特定
2. 「侵入者」イベント等のWAIT状態に対応する空行（エンター）を追加
3. 修正後、--flow テストで期待出力を確認

### 調査対象

1. `ERB/SYSTEM/EVENTSHOP.ERB` - 起床後のイベント処理
2. 「侵入者」イベントの発生条件と WAIT 処理
3. COM888 実行後の WAIT 処理

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-25 | initializer | Feature 210 initialization completed | WIP |
| 2025-12-25 | - | F209 実装中に発見、分離して作成 | PROPOSED |
| 2025-12-25 | opus | 調査: 空行バグ発見、F211 として分離 | BLOCKED on F211 |
| 2025-12-25 | opus | F211完了、input file空行追加済み | UNBLOCKED |
| 2025-12-25 | ac-tester | AC検証 | OK:6/6 |
| 2025-12-25 | regression-tester | 回帰テスト | OK:24/24 |

---

## Links

- [feature-209.md](feature-209.md) - State 遷移修正（本Feature完了後に再検証）
- [feature-208.md](feature-208.md) - 旧シナリオ修正（本Featureに統合）
- [feature-211.md](feature-211.md) - Empty Line Input Bug Fix（前提）
