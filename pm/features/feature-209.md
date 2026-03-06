# Feature 209: Flow Mode State Transition Fix

## Status: [DONE]

## Type: engine

## Depends: [208]

**相互依存の整理**:
- F208 → F209: シナリオ修正は state 遷移が動作しないと検証不可
- F209 → F208: state 遷移修正後、C1-C4 の検証に F208 のシナリオ修正が必要

**実行順序**: F209 (state 遷移) を先に実装 → F208 (シナリオ期待値) で完成

## Background

### Philosophy

**F095/F105/F200-207 の思想を継承**:

1. **回帰テストはゲーム全体の保証として機能する**
   - シナリオが「ゲームロジックが正しい」ことを検証する
   - exit 0 だけでなく、期待した状態変化が起きたかを確認する

2. **状態注入 → コマンド実行 → 期待値検証** のフローが完全に動作する
   - scenario-*.json: 前提状態を注入
   - input-*.txt: コマンドを実行（COM888 でターン終了など）
   - expect.var_equals: 狙い通りの値になったか検証

3. **成功なら簡潔に、失敗なら詳細に** 出力する
   - PASS: `var_equals: PASS`, `PASS: expectation met`
   - FAIL: `Expected: X, Actual: Y` で差分を明示

**本 Feature の位置づけ**:
Flow mode は「状態注入 + input コマンド + expect 検証」を一貫して実行する。
state 遷移が途中で止まると、EVENTTURNEND による TALENT 判定が実行されず、var_equals 検証が意味をなさない。
本 Feature で state 遷移を修正し、「状態注入→コマンド→期待値」フローを完全に機能させる。

### Problem

F208 実装中に発見された問題。`--flow` モードで COM888（1日の終了）を実行しても、`BEGIN AFTERTRAIN` 後の state 遷移が正しく処理されない。

**観察された動作**:
1. コマンド888入力
2. 時刻が07:00のまま変化しない
3. @EVENTEND の出力（"一日が終わりました"）が表示されない
4. @EVENTTURNEND が呼ばれない → TALENT変更が処理されない
5. → var_equals 検証が FAIL（期待した TALENT が付与されていない）

**期待される動作**:
1. COM888 → `BEGIN AFTERTRAIN`
2. AFTERTRAIN → @EVENTEND（"一日が終わりました"）
3. @EVENTEND → `BEGIN ABLUP`
4. ABLUP → @EVENTTURNEND（TALENT/恋慕/NTR判定）
5. @EVENTTURNEND → `BEGIN SHOP`
6. → var_equals 検証が PASS（TALENT が正しく付与される）

### Root Cause

`--flow` モードの game loop が `BEGIN` statement による state 遷移を正しくハンドリングしていない可能性。
Input exhausted 時点でループを終了し、state 遷移の完了を待っていない可能性。

### Impact

- sc-002（思慕→恋慕）の var_equals 検証失敗
- sc-004（NTR陥落）の var_equals 検証失敗
- ターン終了を伴うすべての回帰テストで「状態注入→コマンド→期待値」フローが破綻

### Goal

1. `--flow` モードで `BEGIN AFTERTRAIN` → `TURNEND` → `SHOP` の遷移を正しく処理
2. sc-002: 状態注入 → COM888 → EVENTTURNEND → TALENT:3(恋慕)=1 を var_equals で検証 PASS
3. sc-004: 状態注入 → COM888 → EVENTTURNEND → TALENT:6(NTR)=1 を var_equals で検証 PASS
4. 成功時: `var_equals: PASS` の簡潔出力
5. 失敗時: `Expected: X, Actual: Y` の詳細出力（F207 で実装済み）

---

## Acceptance Criteria

### Part A: 調査

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| A1 | ProcessLevelParallelRunner調査完了 | doc | manual | contains | "### ProcessLevelParallelRunner 調査結果" | [x] |
| A2 | HeadlessRunner調査完了 | doc | manual | contains | "### HeadlessRunner 調査結果" | [x] |

### Part B: State 遷移修正（ポジ）

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| B1 | AFTERTRAIN 実行 | output | --flow | contains | "一日が終わりました" | [x] |
| B2 | SHOP state 到達（メニュー表示） | output | --flow | contains | "[100] - 起床" | [x] |

### Part C: EVENTTURNEND 実行確認（ポジ）

**Note**: C1/C2 は state 遷移が正しく動作し、EVENTTURNEND 内の判定ロジックが実行された証拠。F208 でシナリオ期待値を修正した上で PASS となる。

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| C1 | sc-002: 恋慕獲得メッセージ | output | --flow | contains | "[恋慕]を得た" | [x] |
| C2 | sc-004: NTR陥落メッセージ | output | --flow | contains | "[NTR]を得た" | [x] |
| C3 | var_equals: PASS 出力 | output | --flow | contains | "var_equals: PASS" | [x] |
| C4 | 成功時の簡潔出力 | output | --flow | contains | "PASS: expectation met" | [x] |

### Part D: 回帰テスト全件

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| D1 | 24/24 PASS | output | --flow | contains | "24/24" | [x] |
| D2 | verify-logs.py 検証成功 | exit_code | python | equals | 0 | [x] |

### Part E: ビルド

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| E1 | dotnet build 成功 | build | dotnet | succeeds | - | [x] |
| E2 | dotnet test 成功 | exit_code | dotnet | equals | 0 | [x] |

### AC Details

**A1-A2**: 調査結果を Technical Details セクションに「### ProcessLevelParallelRunner 調査結果」「### HeadlessRunner 調査結果」サブセクションとして文書化。

**B1-B2**: state 遷移が正しく動作する証拠
```bash
cd Game
dotnet run --project ../engine/uEmuera.Headless.csproj -- . --flow "tests/regression/scenario-sc-002*.json" --parallel 1
```
- B1: "一日が終わりました" → AFTERTRAIN が実行された
- B2: "春季2日" → SHOP state に到達し日付が進んだ

**C1-C2**: EVENTTURNEND 内の判定ロジックが実行された証拠
- C1: "[恋慕]を得た" → CHK_FALL_IN_LOVE が実行され条件を満たした
- C2: "[NTR]を得た" → CHK_NTR_CHANGE が実行され条件を満たした
- **前提**: F208 でシナリオ期待値（EXP:40, TALENT番号）を修正済み

**C3-C4**: F207 expect フレームワークの出力確認
- C3: `var_equals: PASS` → 変数検証成功
- C4: `PASS: expectation met` → 全期待値クリア（成功なら簡潔）

**D1-D2**: 全件回帰テスト + verify-logs.py 照合
```bash
cd Game
dotnet run --project ../engine/uEmuera.Headless.csproj -- . --flow "tests/regression/*.json" --parallel 4
python tools/verify-logs.py --dir Game/logs/prod
```

**E1-E2**: `dotnet build engine/` / `dotnet test engine.Tests/`

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | A1 | ProcessLevelParallelRunner の state 遷移調査 | ProcessLevelParallelRunner.cs | [O] |
| 2 | A2 | HeadlessRunner の BEGIN 処理調査 | HeadlessRunner.cs | [O] |
| 3 | B1-B2 | state 遷移修正実装（調査結果に基づき Target 決定） | HeadlessWindow.cs | [O] |
| 4 | C1 | sc-002 恋慕獲得メッセージ確認 | scenario-sc-002*.json | [O] |
| 5 | C2 | sc-004 NTR陥落メッセージ確認 | scenario-sc-004*.json | [O] |
| 6 | C3 | var_equals: PASS 出力確認 | - | [O] |
| 7 | C4 | 成功時簡潔出力確認 | - | [O] |
| 8 | D1 | 24/24 PASS 確認 | tests/regression/ | [O] |
| 9 | D2 | verify-logs.py 検証 | logs/prod/ | [O] |
| 10 | E1 | dotnet build 確認 | engine/ | [O] |
| 11 | E2 | dotnet test 確認 | engine.Tests/ | [O] |

**Note**:
- Task 3 の Target は Task 1-2 の調査完了後に具体化する
- Task 4-7 (C1-C4) は F208 でシナリオ期待値を修正した状態で確認
- F209 = state 遷移修正、F208 = シナリオ期待値修正。両方完了で 24/24 PASS

---

## Technical Details

### 調査対象ファイル

1. `engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs`
   - FlowTest の実行ロジック
   - game loop のハンドリング

2. `engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs`
   - `--flow` モードのエントリポイント
   - state machine の処理

3. `engine/Assets/Scripts/Emuera/GameProc/Process.State.cs`
   - `AfterTrain_Begin`, `Turnend_Begin` 等の state 定義

### ProcessLevelParallelRunner 調査結果

**ファイル**: `engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs`

#### Flow Test 実行メカニズム

Flow mode では worker process を使用して個別テストを実行:
- scenario-{name}.json → input-{name}.txt のペアを検出
- Parent (ProcessLevelParallelRunner) がコマンドを組み立て
- Child process は `--input-file` パラメータを受け取り、非Flow mode で実行

#### Input Processing in Child (HeadlessWindow.cs, lines 184-208)

```csharp
private void HandleInput()
{
    if (inputQueue_.Count == 0)
    {
        Console.WriteLine("[Headless] Input queue exhausted");
        HeadlessRunner.Stop();  // ← CRITICAL: Stops game loop immediately
        return;
    }
    string input = inputQueue_.Dequeue();
    console_.PressEnterKey(false, input, false);
}
```

**問題**: Input file の最後のコマンド（例：888）を処理後、`Stop()` が呼ばれ、state transition が完了する前に process が終了する可能性。

### HeadlessRunner 調査結果

**ファイル**: `engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs`

#### 根本原因の特定

**Process.DoScript() (lines 219-229)**:
```csharp
while (true)
{
    while (state.ScriptEnd && console.IsRunning)
        runSystemProc();  // ← State machine processor

    if (!console.IsRunning)  // ← Input exhaustion でここが true
        break;

    runScriptProc();
}
```

**Key Issue**:
- Input exhaustion で `console.IsRunning = false` が set されると loop を break
- `state.ScriptEnd && console.IsRunning` を再確認する前に break
- Pending state transitions が skip される

### 修正方針

**優先度1: HeadlessWindow.HandleInput()**
- Input exhaustion の後も state transitions を wait する
- 即座に Stop() を呼ばず、state が Normal に戻るまで待機

**優先度2: Process.DoScript()**
- `console.IsRunning` check 改善
- State transitions 完了後に break

### デバッグ方法

```bash
# 詳細ログ付きで実行
cd Game
dotnet run --project ../engine/uEmuera.Headless.csproj -- . \
  --flow tests/regression/scenario-sc-002-shiboo-promotion.json \
  --trace 2>&1 | tee debug.log
```

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-25 | - | F208 実装中に問題発見、分離して作成 | PROPOSED |
| 2025-12-25 | explorer | Tasks 1-2: state 遷移調査 | READY (root cause: HeadlessWindow.HandleInput) |
| 2025-12-25 | implementer | Task 3: HeadlessWindow.cs 修正 | SUCCESS (inputExhaustedCount_ 追加) |
| 2025-12-25 | opus | AC B1, C1 確認 | B1 PASS → 後に FAIL (シナリオ input 順序問題) |
| 2025-12-25 | regression-tester | 全件テスト | 22/24 (2 FAIL は F208 スコープ) |
| 2025-12-25 | opus | verify-logs.py | OK:816/816 AC, 94/94 Engine, ERR:2|24 Regression |
| 2025-12-25 | opus | 再検証 | シナリオ input 順序問題発見 → F210/F211 で解決 |
| 2025-12-25 | opus | F210/F211完了後再検証 | OK: A1-A2, B1-B2, C1-C4, D1-D2, E1-E2 全PASS |

---

## Links

- [feature-208.md](feature-208.md) - シナリオ期待値修正（本Feature完了後に再開）
- [ProcessLevelParallelRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs)
- [HeadlessRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs)
- [Process.State.cs](../../engine/Assets/Scripts/Emuera/GameProc/Process.State.cs)
