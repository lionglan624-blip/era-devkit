# Feature 209: Flow Mode State Transition Fix



## Status: [DONE]



## Type: engine



## Depends: [208]



**相互依存�E整琁E*:

- F208 ↁEF209: シナリオ修正は state 遷移が動作しなぁE��検証不可

- F209 ↁEF208: state 遷移修正後、C1-C4 の検証に F208 のシナリオ修正が忁E��E


**実行頁E��E*: F209 (state 遷移) を�Eに実裁EↁEF208 (シナリオ期征E��) で完�E



## Background



### Philosophy



**F095/F105/F200-207 の思想を継承**:



1. **回帰チE��ト�Eゲーム全体�E保証として機�Eする**

   - シナリオが「ゲームロジチE��が正しい」ことを検証する

   - exit 0 だけでなく、期征E��た状態変化が起きたかを確認すめE


2. **状態注入 ↁEコマンド実衁EↁE期征E��検証** のフローが完�Eに動作すめE
   - scenario-*.json: 前提状態を注入

   - input-*.txt: コマンドを実行！EOM888 でターン終亁E��ど�E�E
   - expect.var_equals: 狙い通りの値になったか検証



3. **成功なら簡潔に、失敗なら詳細に** 出力すめE
   - PASS: `var_equals: PASS`, `PASS: expectation met`

   - FAIL: `Expected: X, Actual: Y` で差刁E��明示



**本 Feature の位置づぁE*:

Flow mode は「状態注入 + input コマンチE+ expect 検証」を一貫して実行する、E
state 遷移が途中で止まると、EVENTTURNEND による TALENT 判定が実行されず、var_equals 検証が意味をなさなぁE��E
本 Feature で state 遷移を修正し、「状態注入→コマンド�E期征E��」フローを完�Eに機�Eさせる、E


### Problem



F208 実裁E��に発見された問題。`--flow` モードで COM888�E�E日の終亁E��を実行しても、`BEGIN AFTERTRAIN` 後�E state 遷移が正しく処琁E��れなぁE��E


**観察された動佁E*:

1. コマンチE88入劁E
2. 時刻ぁE7:00のまま変化しなぁE
3. @EVENTEND の出力！E一日が終わりました"�E�が表示されなぁE
4. @EVENTTURNEND が呼ばれなぁEↁETALENT変更が�E琁E��れなぁE
5. ↁEvar_equals 検証ぁEFAIL�E�期征E��ぁETALENT が付与されてぁE��ぁE��E


**期征E��れる動佁E*:

1. COM888 ↁE`BEGIN AFTERTRAIN`

2. AFTERTRAIN ↁE@EVENTEND�E�E一日が終わりました"�E�E
3. @EVENTEND ↁE`BEGIN ABLUP`

4. ABLUP ↁE@EVENTTURNEND�E�EALENT/恋�E/NTR判定！E
5. @EVENTTURNEND ↁE`BEGIN SHOP`

6. ↁEvar_equals 検証ぁEPASS�E�EALENT が正しく付与される�E�E


### Root Cause



`--flow` モード�E game loop ぁE`BEGIN` statement による state 遷移を正しくハンドリングしてぁE��ぁE��能性、E
Input exhausted 時点でループを終亁E��、state 遷移の完亁E��征E��てぁE��ぁE��能性、E


### Impact



- sc-002�E�思�E→恋慕）�E var_equals 検証失敁E
- sc-004�E�ETR陥落�E��E var_equals 検証失敁E
- ターン終亁E��伴ぁE��べての回帰チE��トで「状態注入→コマンド�E期征E��」フローが破綻



### Goal



1. `--flow` モードで `BEGIN AFTERTRAIN` ↁE`TURNEND` ↁE`SHOP` の遷移を正しく処琁E
2. sc-002: 状態注入 ↁECOM888 ↁEEVENTTURNEND ↁETALENT:3(恋�E)=1 めEvar_equals で検証 PASS

3. sc-004: 状態注入 ↁECOM888 ↁEEVENTTURNEND ↁETALENT:6(NTR)=1 めEvar_equals で検証 PASS

4. 成功晁E `var_equals: PASS` の簡潔�E劁E
5. 失敗時: `Expected: X, Actual: Y` の詳細出力！E207 で実裁E��み�E�E


---



## Acceptance Criteria



### Part A: 調査



| AC# | Description | Type | Method | Matcher | Expected | Status |

|:---:|-------------|------|--------|---------|----------|:------:|

| A1 | ProcessLevelParallelRunner調査完亁E| doc | manual | contains | "### ProcessLevelParallelRunner 調査結果" | [x] |

| A2 | HeadlessRunner調査完亁E| doc | manual | contains | "### HeadlessRunner 調査結果" | [x] |



### Part B: State 遷移修正�E��Eジ�E�E


| AC# | Description | Type | Method | Matcher | Expected | Status |

|:---:|-------------|------|--------|---------|----------|:------:|

| B1 | AFTERTRAIN 実衁E| output | --flow | contains | "一日が終わりました" | [x] |

| B2 | SHOP state 到達（メニュー表示�E�E| output | --flow | contains | "[100] - 起庁E | [x] |



### Part C: EVENTTURNEND 実行確認（�Eジ�E�E


**Note**: C1/C2 は state 遷移が正しく動作し、EVENTTURNEND 冁E�E判定ロジチE��が実行された証拠、E208 でシナリオ期征E��を修正した上で PASS となる、E


| AC# | Description | Type | Method | Matcher | Expected | Status |

|:---:|-------------|------|--------|---------|----------|:------:|

| C1 | sc-002: 恋�E獲得メチE��ージ | output | --flow | contains | "[恋�E]を得た" | [x] |

| C2 | sc-004: NTR陥落メチE��ージ | output | --flow | contains | "[NTR]を得た" | [x] |

| C3 | var_equals: PASS 出劁E| output | --flow | contains | "var_equals: PASS" | [x] |

| C4 | 成功時�E簡潔�E劁E| output | --flow | contains | "PASS: expectation met" | [x] |



### Part D: 回帰チE��ト�E件



| AC# | Description | Type | Method | Matcher | Expected | Status |

|:---:|-------------|------|--------|---------|----------|:------:|

| D1 | 24/24 PASS | output | --flow | contains | "24/24" | [x] |

| D2 | verify-logs.py 検証成功 | exit_code | python | equals | 0 | [x] |



### Part E: ビルチE


| AC# | Description | Type | Method | Matcher | Expected | Status |

|:---:|-------------|------|--------|---------|----------|:------:|

| E1 | dotnet build 成功 | build | dotnet | succeeds | - | [x] |

| E2 | dotnet test 成功 | exit_code | dotnet | equals | 0 | [x] |



### AC Details



**A1-A2**: 調査結果めETechnical Details セクションに、E## ProcessLevelParallelRunner 調査結果」、E## HeadlessRunner 調査結果」サブセクションとして斁E��化、E


**B1-B2**: state 遷移が正しく動作する証拠

```bash

cd Game

dotnet run --project ../engine/uEmuera.Headless.csproj -- . --flow "tests/regression/scenario-sc-002*.json" --parallel 1

```

- B1: "一日が終わりました" ↁEAFTERTRAIN が実行された

- B2: "春季2日" ↁESHOP state に到達し日付が進んだ



**C1-C2**: EVENTTURNEND 冁E�E判定ロジチE��が実行された証拠

- C1: "[恋�E]を得た" ↁECHK_FALL_IN_LOVE が実行され条件を満たしぁE
- C2: "[NTR]を得た" ↁECHK_NTR_CHANGE が実行され条件を満たしぁE
- **前提**: F208 でシナリオ期征E���E�EXP:40, TALENT番号�E�を修正済み



**C3-C4**: F207 expect フレームワークの出力確誁E
- C3: `var_equals: PASS` ↁE変数検証成功

- C4: `PASS: expectation met` ↁE全期征E��クリア�E��E功なら簡潔！E


**D1-D2**: 全件回帰チE��チE+ verify-logs.py 照吁E
```bash

cd Game

dotnet run --project ../engine/uEmuera.Headless.csproj -- . --flow "tests/regression/*.json" --parallel 4

python src/tools/python/verify-logs.py --dir _out/logs/prod

```



**E1-E2**: `dotnet build engine/` / `dotnet test engine.Tests/`



---



## Tasks



| Task# | AC# | Description | Target | Status |

|:-----:|:---:|-------------|--------|:------:|

| 1 | A1 | ProcessLevelParallelRunner の state 遷移調査 | ProcessLevelParallelRunner.cs | [O] |

| 2 | A2 | HeadlessRunner の BEGIN 処琁E��査 | HeadlessRunner.cs | [O] |

| 3 | B1-B2 | state 遷移修正実裁E��調査結果に基づぁETarget 決定！E| HeadlessWindow.cs | [O] |

| 4 | C1 | sc-002 恋�E獲得メチE��ージ確誁E| scenario-sc-002*.json | [O] |

| 5 | C2 | sc-004 NTR陥落メチE��ージ確誁E| scenario-sc-004*.json | [O] |

| 6 | C3 | var_equals: PASS 出力確誁E| - | [O] |

| 7 | C4 | 成功時簡潔�E力確誁E| - | [O] |

| 8 | D1 | 24/24 PASS 確誁E| tests/regression/ | [O] |

| 9 | D2 | verify-logs.py 検証 | logs/prod/ | [O] |

| 10 | E1 | dotnet build 確誁E| engine/ | [O] |

| 11 | E2 | dotnet test 確誁E| engine.Tests/ | [O] |



**Note**:

- Task 3 の Target は Task 1-2 の調査完亁E��に具体化する

- Task 4-7 (C1-C4) は F208 でシナリオ期征E��を修正した状態で確誁E
- F209 = state 遷移修正、F208 = シナリオ期征E��修正。両方完亁E�� 24/24 PASS



---



## Technical Details



### 調査対象ファイル



1. `engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs`

   - FlowTest の実行ロジチE��

   - game loop のハンドリング



2. `engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs`

   - `--flow` モード�EエントリポインチE
   - state machine の処琁E


3. `engine/Assets/Scripts/Emuera/GameProc/Process.State.cs`

   - `AfterTrain_Begin`, `Turnend_Begin` 等�E state 定義



### ProcessLevelParallelRunner 調査結果



**ファイル**: `engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs`



#### Flow Test 実行メカニズム



Flow mode では worker process を使用して個別チE��トを実衁E

- scenario-{name}.json ↁEinput-{name}.txt のペアを検�E

- Parent (ProcessLevelParallelRunner) がコマンドを絁E��立て

- Child process は `--input-file` パラメータを受け取り、E��Flow mode で実衁E


#### Input Processing in Child (HeadlessWindow.cs, lines 184-208)



```csharp

private void HandleInput()

{

    if (inputQueue_.Count == 0)

    {

        Console.WriteLine("[Headless] Input queue exhausted");

        HeadlessRunner.Stop();  // ↁECRITICAL: Stops game loop immediately

        return;

    }

    string input = inputQueue_.Dequeue();

    console_.PressEnterKey(false, input, false);

}

```



**問顁E*: Input file の最後�Eコマンド（例！E88�E�を処琁E��、`Stop()` が呼ばれ、state transition が完亁E��る前に process が終亁E��る可能性、E


### HeadlessRunner 調査結果



**ファイル**: `engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs`



#### 根本原因の特宁E


**Process.DoScript() (lines 219-229)**:

```csharp

while (true)

{

    while (state.ScriptEnd && console.IsRunning)

        runSystemProc();  // ↁEState machine processor



    if (!console.IsRunning)  // ↁEInput exhaustion でここぁEtrue

        break;



    runScriptProc();

}

```



**Key Issue**:

- Input exhaustion で `console.IsRunning = false` ぁEset されると loop めEbreak

- `state.ScriptEnd && console.IsRunning` を�E確認する前に break

- Pending state transitions ぁEskip されめE


### 修正方釁E


**優先度1: HeadlessWindow.HandleInput()**

- Input exhaustion の後も state transitions めEwait する

- 即座に Stop() を呼ばず、state ぁENormal に戻るまで征E��E


**優先度2: Process.DoScript()**

- `console.IsRunning` check 改喁E
- State transitions 完亁E��に break



### チE��チE��方況E


```bash

# 詳細ログ付きで実衁E
cd Game

dotnet run --project ../engine/uEmuera.Headless.csproj -- . \

  --flow tests/regression/scenario-sc-002-shiboo-promotion.json \

  --trace 2>&1 | tee debug.log

```



---



## Execution Log



| Date | Agent | Action | Result |

|------|-------|--------|--------|

| 2025-12-25 | - | F208 実裁E��に問題発見、�E離して作�E | PROPOSED |

| 2025-12-25 | explorer | Tasks 1-2: state 遷移調査 | READY (root cause: HeadlessWindow.HandleInput) |

| 2025-12-25 | implementer | Task 3: HeadlessWindow.cs 修正 | SUCCESS (inputExhaustedCount_ 追加) |

| 2025-12-25 | opus | AC B1, C1 確誁E| B1 PASS ↁE後に FAIL (シナリオ input 頁E��問顁E |

| 2025-12-25 | regression-tester | 全件チE��チE| 22/24 (2 FAIL は F208 スコーチE |

| 2025-12-25 | opus | verify-logs.py | OK:816/816 AC, 94/94 Engine, ERR:2|24 Regression |

| 2025-12-25 | opus | 再検証 | シナリオ input 頁E��問題発要EↁEF210/F211 で解決 |

| 2025-12-25 | opus | F210/F211完亁E���E検証 | OK: A1-A2, B1-B2, C1-C4, D1-D2, E1-E2 全PASS |



---



## Links



- [feature-208.md](feature-208.md) - シナリオ期征E��修正�E�本Feature完亁E��に再開�E�E
- [ProcessLevelParallelRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs)

- [HeadlessRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs)

- [Process.State.cs](../../engine/Assets/Scripts/Emuera/GameProc/Process.State.cs)

