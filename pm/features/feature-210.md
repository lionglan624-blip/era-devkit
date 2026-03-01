# Feature 210: Scenario Input Sequence Fix

## Status: [DONE]

## Type: infra

## Depends: [209, 211]

**Note**: F211 (Empty Line Input Bug Fix) が忁E��。input ファイルの空行が WAIT 確認として機�EしなぁE��題を先に修正する忁E��がある、E

## Background

### Philosophy

**F095/F105/F200-209 の思想を継承**:

1. **回帰チE��ト�Eゲーム全体�E保証として機�Eする**
   - シナリオが「ゲームロジチE��が正しい」ことを検証する
   - exit 0 だけでなく、期征E��た状態変化が起きたかを確認すめE

2. **状態注入 ↁEコマンド実衁EↁE期征E��検証** のフローが完�Eに動作すめE
   - scenario-*.json: 前提状態を注入
   - input-*.txt: コマンドを実行！EOM888 でターン終亁E��ど�E�E
   - expect.var_equals: 狙い通りの値になったか検証

### Problem

F209 で state 遷移修正を実裁E��たが、テストシナリオが正しく動作しなぁE��E

**観察された問顁E*:
1. `100` (起庁E 後に「館に侵入老E��あったよぁE���E�」イベントが発甁E
2. こ�Eイベント�Eエンター征E���E�EAIT 状態！E
3. `888` 入力がエンター確認として消費されめE
4. COM888 が実行されなぁEↁEstate 遷移が発生しなぁE

**根本原因**:
- input file の頁E��が実際のゲームフローと一致してぁE��ぁE
- ゲーム中のイベント（侵入老E��ベント等）による追加の input 消費を老E�EしてぁE��ぁE

### Goal

1. sc-002 (思�E→恋慁E シナリオが正しく COM888 を実衁E
2. sc-004 (NTR陥落) シナリオが正しく COM888 を実衁E
3. 「一日が終わりました」�E力確認！E209 B1�E�E
4. 「[恋�E]を得た」「[NTR]を得た」�E力確認！E209 C1-C2�E�E
5. var_equals: PASS�E�E209 C3�E�E
6. 24/24 PASS�E�E209 D1�E�E

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | sc-002 input 頁E��修正 | output | --flow sc-002 | contains | "一日が終わりました" | [x] |
| 2 | sc-002 恋�E獲征E| output | --flow sc-002 | contains | "[恋�E]を得た" | [x] |
| 3 | sc-004 input 頁E��修正 | output | --flow sc-004 | contains | "一日が終わりました" | [x] |
| 4 | sc-004 NTR陥落 | output | --flow sc-004 | contains | "[NTR]を得た" | [x] |
| 5 | 24/24 PASS | output | --flow | contains | "24/24" | [x] |
| 6 | verify-logs.py 検証成功 | exit_code | verify-logs.py | equals | 0 | [x] |

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | 1,2 | sc-002 input 頁E��修正・チE��チE| tests/regression/input-sc-002*.txt | [O] |
| 2 | 3,4 | sc-004 input 頁E��修正・チE��チE| tests/regression/input-sc-004*.txt | [O] |
| 3 | 5 | 全24シナリオ回帰チE��チE| tests/regression/ | [O] |
| 4 | 6 | verify-logs.py 検証 | src/tools/python/verify-logs.py | [O] |

---

## Technical Details

### 現在の input シーケンス (sc-002)

```
0     ↁEゲーム開姁E
9     ↁEクイチE��スターチE
100   ↁE起庁E
888   ↁE1日の終亁EↁEここで「侵入老E��イベント�Eエンター征E��に消費されめE
100   ↁE確誁E
```

### 修正方釁E

1. ゲームフローを実機で確認し、忁E��な入力シーケンスを特宁E
2. 「侵入老E��イベント等�EWAIT状態に対応する空行（エンター�E�を追加
3. 修正後、E-flow チE��トで期征E�E力を確誁E

### 調査対象

1. `ERB/SYSTEM/EVENTSHOP.ERB` - 起床後�Eイベント�E琁E
2. 「侵入老E��イベント�E発生条件と WAIT 処琁E
3. COM888 実行後�E WAIT 処琁E

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-25 | initializer | Feature 210 initialization completed | WIP |
| 2025-12-25 | - | F209 実裁E��に発見、�E離して作�E | PROPOSED |
| 2025-12-25 | opus | 調査: 空行バグ発見、F211 として刁E�� | BLOCKED on F211 |
| 2025-12-25 | opus | F211完亁E��input file空行追加済み | UNBLOCKED |
| 2025-12-25 | ac-tester | AC検証 | OK:6/6 |
| 2025-12-25 | regression-tester | 回帰チE��チE| OK:24/24 |

---

## Links

- [feature-209.md](feature-209.md) - State 遷移修正�E�本Feature完亁E��に再検証�E�E
- [feature-208.md](feature-208.md) - 旧シナリオ修正�E�本Featureに統合！E
- [feature-211.md](feature-211.md) - Empty Line Input Bug Fix�E�前提！E
