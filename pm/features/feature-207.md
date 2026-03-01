# Feature 207: Flow Test Verification Framework



## Status: [DONE]



## Type: infra + engine



## Depends: [206]



## Background



### Problem



F095/F105/F200-205 の思想:

1. **シナリオがゲーム全体�E保証をできるか検討�E判断**

2. **シナリオが試したぁE���Eが特定されており、状態注入→コマンド実行�Eoutput が期征E��一致するぁE*

3. **ログに結果が残り、異常時�E検証できる**



**現状の問顁E*:

- 24シナリオは「exit 0 = PASS」だが、「ゲームロジチE��が正しい」保証ではなぁE
- 侁E sc-001 は「好感度999で恋�EにならなぁE��を検証すべきだが、クラチE��ュしなければ PASS

- output はログに残るが、E*期征E��との比輁E��なぁE*

- FAIL 時に「期征Evs 実際」が出力されなぁE


### F206 調査結果 (前提条件)



- input-*.txt 24件復允E��み (`tests/regression/`)

- 回帰チE��チE24/24 PASS (glob + parallel 使用晁E ↁE**exit 0 のみで判宁E*

- FLOW.md に実行経路・制限事頁E��追記済み

- 単一ファイル + parallel なぁEↁEinput ファイルが読み込まれなぁE��題を発要E


### Goal



**回帰チE��トが「ゲーム全体�E保証」として機�Eする状態にする、E*



具体的には:

1. 吁E��ナリオに「何を検証するか、Eexpected output) を定義

2. チE��ト実行時に output を期征E��と比輁E
3. PASS 時�E簡潔に報告、FAIL 時�E「期征Evs 実際」を詳細出劁E
4. verify-logs.py で output 検証を�E動化

5. サブエージェントが Skills 参�Eで迷わず一発実衁E


---



## Acceptance Criteria



### Part A: シナリオ検証定義



| AC# | Description | Type | Matcher | Expected | Status |

|:---:|-------------|------|---------|----------|:------:|

| A1 | scenario JSON に expect フィールド定義追加 | file | contains | `"expect":` | [x] |

| A2 | 24シナリオ全てに期征E��定義 | exit_code | equals | 0 | [x] |

| A3 | sc-001: 恋�EにならなぁE��証 | file | contains | `"TALENT:1:18": 0` | [x] |



### Part B: Engine 検証機�E



| AC# | Description | Type | Matcher | Expected | Status |

|:---:|-------------|------|---------|----------|:------:|

| B1 | expect 検証機�E実裁E| output | contains | "[Expect]" | [x] |

| B2 | PASS 晁E 簡潔�E劁E| output | contains | "PASS: expectation met" | [x] |

| B3 | FAIL 晁E 期征Evs 実際出劁E| output | contains | "Expected:" | [x] |

| B4 | 単一ファイル時も input ペアリング | output | matches | `Buffered.*input` | [-] |

| B5 | 期征E��なしシナリオで従来動作維持E(ネガ) | output | not_contains | "[Expect]" | [x] |

| B6 | --parallel 子�Eロセス適刁E��亁E| process | equals | 0 残存�Eロセス | [x] |

| B7 | 子�Eロセスがvar_equals検証を実衁E| output | contains | "[ExpectResult]" | [x] |

| B8 | 親プロセスがExpectResult解极E| output | contains | "var_equals: PASS" | [x] |



### Part C: 運用フロー



| AC# | Description | Type | Matcher | Expected | Status |

|:---:|-------------|------|---------|----------|:------:|

| C1 | FLOW.md に正規コマンド�E訁E| file | contains | "regression-tester MUST" | [x] |

| C2 | FLOW.md に検証基準�E訁E| file | contains | "expect フィールチE | [x] |

| C3 | verify-logs.py で expect 検証 | exit_code | equals | 0 | [x] |

| C4 | regression-tester.md ぁEFLOW.md 参�E | file | contains | "FLOW.md" | [x] |



### Part D: ビルド�EチE��チE


| AC# | Description | Type | Matcher | Expected | Status |

|:---:|-------------|------|---------|----------|:------:|

| D1 | dotnet build 成功 | build | succeeds | - | [x] |

| D2 | dotnet test 成功 | exit_code | equals | 0 | [x] |

| D3 | 24/24 PASS (検証込み) | output | contains | "24/24 passed" | [x] |



---



## Tasks



| Task# | AC# | Description | Target | Status |

|:-----:|:---:|-------------|--------|:------:|

| 1 | A1 | scenario JSON に expect フィールド定義追加 | tests/regression/*.json | [x] |

| 2 | A2 | 24シナリオ全てに期征E��設宁E| tests/regression/*.json | [x] |

| 3 | A3 | sc-001 期征E��: TALENT:1:18=0 | scenario-sc-001-*.json | [x] |

| 4 | B1 | Engine: expect 検証機�E実裁E| ProcessLevelParallelRunner.cs | [x] |

| 5 | B2 | Engine: PASS 時簡潔�E劁E| ProcessLevelParallelRunner.cs | [x] |

| 6 | B3 | Engine: FAIL 時詳細出劁E| ProcessLevelParallelRunner.cs | [x] |

| 7 | B4 | Engine: 単一ファイル input ペアリング | HeadlessRunner.cs | [-] |

| 8 | B5 | 期征E��なしシナリオの従来動作確誁E| tests/regression/ | [x] |

| 16 | B6 | Engine: 子�Eロセス終亁E��E���E回収 | ProcessLevelParallelRunner.cs | [x] |

| 9 | C1 | FLOW.md に正規コマンド�E訁E| .claude/skills/testing/FLOW.md | [x] |

| 10 | C2 | FLOW.md に検証基準�E訁E| .claude/skills/testing/FLOW.md | [x] |

| 11 | C3 | verify-logs.py: expect 検証対忁E| src/tools/python/verify-logs.py | [x] |

| 12 | C4 | regression-tester.md 更新 | .claude/agents/regression-tester.md | [x] |

| 13 | D1 | dotnet build 確誁E| - | [x] |

| 14 | D2 | dotnet test 確誁E| - | [x] |

| 17 | B7 | HeadlessRunner: --expect-json処琁E| HeadlessRunner.cs | [x] |

| 18 | B7 | InteractiveRunner: EOF時var_equals検証 | InteractiveRunner.cs | [x] |

| 19 | B8 | ProcessLevelParallelRunner: ExpectResult解极E| ProcessLevelParallelRunner.cs | [x] |

| 20 | D3 | 24/24 PASS確誁E| - | [x] |



**凡侁E*: [x]=完亁E [ ]=未完亁E [-]=スキチE�E(--parallel使用で回避可)



---



## Technical Details



### Task 1: scenario JSON expect フィールチE


**現状**:

```json

{

  "name": "sc-001: 思�E→恋慁E閾値未満",

  "description": "好感度999で恋�EにならなぁE��とを確誁E,

  "characters": { ... }

}

```



**修正征E*:

```json

{

  "name": "sc-001: 思�E→恋慁E閾値未満",

  "description": "好感度999で恋�EにならなぁE��とを確誁E,

  "characters": { ... },

  "expect": {

    "output_contains": ["[100]", "日常メニュー"],

    "output_not_contains": ["恋�Eを獲征E],

    "variables": {

      "TALENT:1:18": 0

    }

  }

}

```



### Task 3: Engine expect 検証



**ProcessLevelParallelRunner.cs 修正**:

```csharp

// シナリオ実行征E
if (scenario.Expect != null)

{

    var result = VerifyExpectations(scenario.Expect, output);

    if (result.Passed)

    {

        Console.WriteLine($"[Expect] PASS: expectation met");

    }

    else

    {

        Console.WriteLine($"[Expect] FAIL:");

        Console.WriteLine($"  Expected: {result.Expected}");

        Console.WriteLine($"  Actual: {result.Actual}");

        passed = false;

    }

}

```



### Task 4: 単一ファイル input ペアリング



**HeadlessRunner.cs 修正** (紁Eline 1281):

```csharp

// 現状

if (options_.InjectFiles.Count > 1 || (options_.InjectFiles.Count == 1 && options_.Parallel))



// 修正: --flow 使用時�E常に FlowTestScenario を使用

if (options_.FlowMode || options_.InjectFiles.Count > 1 || ...)

```



### Task 6: verify-logs.py expect 検証



```python

def verify_expectation(result_json):

    """expect フィールド�E検証結果を確誁E""

    if "expectation" not in result_json:

        return True  # 期征E��なぁE= 従来の exit 0 判宁E


    return result_json["expectation"]["passed"]

```



---



## Scenario Verification Mapping (from F095)



| シナリオ | 検証冁E�� | expect 定義 |

|----------|----------|-------------|

| sc-001 | 好感度999で恋�EにならなぁE| `TALENT:1:18 == 0` |

| sc-002 | 好感度1500+従頁Eで恋�E昁E�� | `TALENT:1:18 == 1` |

| sc-003 | 好感度9999で親愛にならなぁE| `TALENT:1:19 == 0` |

| sc-004 | NTR陥落 通常 | `TALENT:1:101 == 1` |

| sc-005 | NTR陥落 親愛保護 | `TALENT:1:101 == 0` |

| sc-006 | セーチEローチE| 全状態復允E��誁E|

| ... | ... | ... |



---



## Execution Log



| Date | Agent | Action | Result |

|------|-------|--------|--------|

| 2025-12-24 | - | F204 から刁E��して作�E | PROPOSED |

| 2025-12-24 | - | 調査/実裁E�E離のため再構�E | 実裁E��用に変更、F206依孁E|

| 2025-12-25 | - | F206 完亁E 回帰チE��チE24/24 PASS (glob + parallel) | - |

| 2025-12-25 | - | 再設訁E 検証フレームワークにスコープ拡大 | AC/Task 再定義 |

| 2025-12-25 | implementer | Tasks 1-3: 24シナリオにexpect追加 | SUCCESS |

| 2025-12-25 | implementer | Tasks 4-6: Engine expect検証実裁E| SUCCESS |

| 2025-12-25 | implementer | Tasks 9-12: ドキュメント更新 | SUCCESS |

| 2025-12-25 | debugger | FlowTestResult.ExpectResults追加 | SUCCESS |

| 2025-12-25 | debugger | Task 7: 単一ファイルペアリング修正試衁E| DLL LOCK |

| 2025-12-25 | - | チE��ト結果: 14/24 PASS (glob+parallel) | BLOCKED |

| 2025-12-25 | implementer | Task 19: var_equals: PASS 出力追加 | SUCCESS |

| 2025-12-25 | - | sc-002, sc-004 問題発要EↁEF208 として刁E�� | - |

| 2025-12-25 | ac-tester | AC検証: OK:23/24 (B4 SKIPPED) | SUCCESS |

| 2025-12-25 | regression-tester | 24/24 PASS, verify-logs OK:934/934 | SUCCESS |

| 2025-12-25 | feature-reviewer | Post-review 完亁E| DONE |



## 完亁E��メモ (2025-12-25)



**全タスク完亁E*:

- A1-A3: シナリオexpect定義 ✁E
- B1-B3, B5-B8: Engine検証機�E ✁E
- B4: 単一ファイルinputペアリング [-] (--parallel使用で回避可)

- C1-C4: ドキュメント更新 ✁E
- D1-D3: ビルチEチE��チE24/24 PASS ✁E


**シナリオ期征E��問顁EↁEF208 へ刁E��**:

- sc-002, sc-004 の var_equals 検証で TALENT 番号誤りと状態注入不足を発要E
- 回帰チE��トシナリオは hook 編雁E��止のため、別 Feature (F208) として修正予宁E


---



### 主要問顁E var_equals検証が機�EしなぁE(10シナリオ失敗�E原因)



**根本原因**:

ProcessLevelParallelRunner.cs L543:

```csharp

varName => null  // getVariable (not available in flow tests)

```



Flow Testでは子�Eロセスでゲームが実行されるため、親プロセスからゲーム変数を読み取れなぁE��E
`var_equals` 検証時に `getVariable` が常に `null` を返すため、�Eて FAIL になる、E


**失敗シナリオ** (10/24):

- sc-001�E�E05: TALENT検証

- sc-006: saveload状態検証

- sc-011, sc-012: 状態検証

- wakeup, alice-sameroom: 状態検証



**解決方釁E*: 子�Eロセス側でvar_equals検証を実裁E(推奨)



| 方況E| 説昁E| 採用 |

|------|------|:----:|

| A. 子�Eロセス側で検証 | 子�Eロセスがexpect JSONを読み、変数を直接チェチE�� | ✁E|

| B. 子�Eロセスが変数dump | 終亁E��にJSON出力、親が検証 | - |

| C. output_containsに変換 | DEBUGPRINTで出力検証 | - |



**実裁E��釁E(方法A)**:

1. 子�Eロセス起動時に `--expect-json` オプションでexpectを渡ぁE
2. 子�Eロセス終亁E��に `KojoExpectValidator.Validate()` を実衁E
3. 結果めEstdout に `[ExpectResult] {"passed":true/false, ...}` 形式で出劁E
4. 親プロセスぁEstdout からパ�Eス



**実裁E��E��**:

- `ProcessLevelParallelRunner.cs`: 子�Eロセス引数にexpect追加

- `HeadlessRunner.cs`: --expect-json オプション処琁E��終亁E��検証

- `InteractiveRunner.cs`: EOF時に検証実衁E


---



### 過去の調査メモ (参老E



**問顁E** (解消済み): HeadlessRunner.cs でFlowTestパスに到達しなぁE
- 修正済み: L720-724, L1198-1202



**問顁E** (B6、現在解涁E: 子�Eロセス残孁E
- 以前�E500+プロセスがDLLロチE��

- 現在はビルチEチE��ト正常動佁E


**変更ファイル一覧**:

- `engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs` (Expect, ExpectResults追加)

- `engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs` (FlowTestパス修正)

- `test/regression/scenario-*.json` (24ファイル全てにexpect追加)

- `.claude/skills/testing/FLOW.md` (検証ルール追訁E

- `.claude/agents/regression-tester.md` (FLOW.md参�E追加)

- `src/tools/python/verify-logs.py` (expectResults検証追加)

- `engine.Tests/Tests/FlowTestExpectVerificationTests.cs` (TDDチE��チE件)



**次回作業** (/imple 207 で続行可能):

1. Task 17: HeadlessRunner --expect-json処琁E
2. Task 18: InteractiveRunner EOF時var_equals検証

3. Task 19: ProcessLevelParallelRunner ExpectResult解极E
4. Task 20: 24/24 PASS確誁E
5. finalizer で完亁E


---



## Links



- [feature-206.md](feature-206.md) - 調査 Feature (完亁E

- [feature-095.md](feature-095.md) - シナリオ設計�E

- [feature-205.md](feature-205.md) - verify-logs.py 設訁E
- [ProcessLevelParallelRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs) - expect 検証実裁E
- [HeadlessRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs) - 単一ファイル修正

- [FLOW.md](../../../archive/claude_legacy_20251230/skills/testing/FLOW.md) - 運用ドキュメンチE
- [regression-tester.md](../../../archive/claude_legacy_20251230/agents/regression-tester.md) - サブエージェンチE
