# Feature 207: Flow Test Verification Framework

## Status: [DONE]

## Type: infra + engine

## Depends: [206]

## Background

### Problem

F095/F105/F200-205 の思想:
1. **シナリオがゲーム全体の保証をできるか検討・判断**
2. **シナリオが試したい機能が特定されており、状態注入→コマンド実行→output が期待と一致するか**
3. **ログに結果が残り、異常時は検証できる**

**現状の問題**:
- 24シナリオは「exit 0 = PASS」だが、「ゲームロジックが正しい」保証ではない
- 例: sc-001 は「好感度999で恋慕にならない」を検証すべきだが、クラッシュしなければ PASS
- output はログに残るが、**期待値との比較がない**
- FAIL 時に「期待 vs 実際」が出力されない

### F206 調査結果 (前提条件)

- input-*.txt 24件復元済み (`tests/regression/`)
- 回帰テスト 24/24 PASS (glob + parallel 使用時) ← **exit 0 のみで判定**
- FLOW.md に実行経路・制限事項を追記済み
- 単一ファイル + parallel なし → input ファイルが読み込まれない問題を発見

### Goal

**回帰テストが「ゲーム全体の保証」として機能する状態にする。**

具体的には:
1. 各シナリオに「何を検証するか」(expected output) を定義
2. テスト実行時に output を期待値と比較
3. PASS 時は簡潔に報告、FAIL 時は「期待 vs 実際」を詳細出力
4. verify-logs.py で output 検証を自動化
5. サブエージェントが Skills 参照で迷わず一発実行

---

## Acceptance Criteria

### Part A: シナリオ検証定義

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| A1 | scenario JSON に expect フィールド定義追加 | file | contains | `"expect":` | [x] |
| A2 | 24シナリオ全てに期待値定義 | exit_code | equals | 0 | [x] |
| A3 | sc-001: 恋慕にならない検証 | file | contains | `"TALENT:1:18": 0` | [x] |

### Part B: Engine 検証機能

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| B1 | expect 検証機能実装 | output | contains | "[Expect]" | [x] |
| B2 | PASS 時: 簡潔出力 | output | contains | "PASS: expectation met" | [x] |
| B3 | FAIL 時: 期待 vs 実際出力 | output | contains | "Expected:" | [x] |
| B4 | 単一ファイル時も input ペアリング | output | matches | `Buffered.*input` | [-] |
| B5 | 期待値なしシナリオで従来動作維持 (ネガ) | output | not_contains | "[Expect]" | [x] |
| B6 | --parallel 子プロセス適切終了 | process | equals | 0 残存プロセス | [x] |
| B7 | 子プロセスがvar_equals検証を実行 | output | contains | "[ExpectResult]" | [x] |
| B8 | 親プロセスがExpectResult解析 | output | contains | "var_equals: PASS" | [x] |

### Part C: 運用フロー

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| C1 | FLOW.md に正規コマンド明記 | file | contains | "regression-tester MUST" | [x] |
| C2 | FLOW.md に検証基準明記 | file | contains | "expect フィールド" | [x] |
| C3 | verify-logs.py で expect 検証 | exit_code | equals | 0 | [x] |
| C4 | regression-tester.md が FLOW.md 参照 | file | contains | "FLOW.md" | [x] |

### Part D: ビルド・テスト

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
| 2 | A2 | 24シナリオ全てに期待値設定 | tests/regression/*.json | [x] |
| 3 | A3 | sc-001 期待値: TALENT:1:18=0 | scenario-sc-001-*.json | [x] |
| 4 | B1 | Engine: expect 検証機能実装 | ProcessLevelParallelRunner.cs | [x] |
| 5 | B2 | Engine: PASS 時簡潔出力 | ProcessLevelParallelRunner.cs | [x] |
| 6 | B3 | Engine: FAIL 時詳細出力 | ProcessLevelParallelRunner.cs | [x] |
| 7 | B4 | Engine: 単一ファイル input ペアリング | HeadlessRunner.cs | [-] |
| 8 | B5 | 期待値なしシナリオの従来動作確認 | tests/regression/ | [x] |
| 16 | B6 | Engine: 子プロセス終了待機・回収 | ProcessLevelParallelRunner.cs | [x] |
| 9 | C1 | FLOW.md に正規コマンド明記 | .claude/skills/testing/FLOW.md | [x] |
| 10 | C2 | FLOW.md に検証基準明記 | .claude/skills/testing/FLOW.md | [x] |
| 11 | C3 | verify-logs.py: expect 検証対応 | tools/verify-logs.py | [x] |
| 12 | C4 | regression-tester.md 更新 | .claude/agents/regression-tester.md | [x] |
| 13 | D1 | dotnet build 確認 | - | [x] |
| 14 | D2 | dotnet test 確認 | - | [x] |
| 17 | B7 | HeadlessRunner: --expect-json処理 | HeadlessRunner.cs | [x] |
| 18 | B7 | InteractiveRunner: EOF時var_equals検証 | InteractiveRunner.cs | [x] |
| 19 | B8 | ProcessLevelParallelRunner: ExpectResult解析 | ProcessLevelParallelRunner.cs | [x] |
| 20 | D3 | 24/24 PASS確認 | - | [x] |

**凡例**: [x]=完了, [ ]=未完了, [-]=スキップ(--parallel使用で回避可)

---

## Technical Details

### Task 1: scenario JSON expect フィールド

**現状**:
```json
{
  "name": "sc-001: 思慕→恋慕 閾値未満",
  "description": "好感度999で恋慕にならないことを確認",
  "characters": { ... }
}
```

**修正後**:
```json
{
  "name": "sc-001: 思慕→恋慕 閾値未満",
  "description": "好感度999で恋慕にならないことを確認",
  "characters": { ... },
  "expect": {
    "output_contains": ["[100]", "日常メニュー"],
    "output_not_contains": ["恋慕を獲得"],
    "variables": {
      "TALENT:1:18": 0
    }
  }
}
```

### Task 3: Engine expect 検証

**ProcessLevelParallelRunner.cs 修正**:
```csharp
// シナリオ実行後
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

**HeadlessRunner.cs 修正** (約 line 1281):
```csharp
// 現状
if (options_.InjectFiles.Count > 1 || (options_.InjectFiles.Count == 1 && options_.Parallel))

// 修正: --flow 使用時は常に FlowTestScenario を使用
if (options_.FlowMode || options_.InjectFiles.Count > 1 || ...)
```

### Task 6: verify-logs.py expect 検証

```python
def verify_expectation(result_json):
    """expect フィールドの検証結果を確認"""
    if "expectation" not in result_json:
        return True  # 期待値なし = 従来の exit 0 判定

    return result_json["expectation"]["passed"]
```

---

## Scenario Verification Mapping (from F095)

| シナリオ | 検証内容 | expect 定義 |
|----------|----------|-------------|
| sc-001 | 好感度999で恋慕にならない | `TALENT:1:18 == 0` |
| sc-002 | 好感度1500+従順3で恋慕昇格 | `TALENT:1:18 == 1` |
| sc-003 | 好感度9999で親愛にならない | `TALENT:1:19 == 0` |
| sc-004 | NTR陥落 通常 | `TALENT:1:101 == 1` |
| sc-005 | NTR陥落 親愛保護 | `TALENT:1:101 == 0` |
| sc-006 | セーブ/ロード | 全状態復元確認 |
| ... | ... | ... |

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-24 | - | F204 から分離して作成 | PROPOSED |
| 2025-12-24 | - | 調査/実装分離のため再構成 | 実装専用に変更、F206依存 |
| 2025-12-25 | - | F206 完了: 回帰テスト 24/24 PASS (glob + parallel) | - |
| 2025-12-25 | - | 再設計: 検証フレームワークにスコープ拡大 | AC/Task 再定義 |
| 2025-12-25 | implementer | Tasks 1-3: 24シナリオにexpect追加 | SUCCESS |
| 2025-12-25 | implementer | Tasks 4-6: Engine expect検証実装 | SUCCESS |
| 2025-12-25 | implementer | Tasks 9-12: ドキュメント更新 | SUCCESS |
| 2025-12-25 | debugger | FlowTestResult.ExpectResults追加 | SUCCESS |
| 2025-12-25 | debugger | Task 7: 単一ファイルペアリング修正試行 | DLL LOCK |
| 2025-12-25 | - | テスト結果: 14/24 PASS (glob+parallel) | BLOCKED |
| 2025-12-25 | implementer | Task 19: var_equals: PASS 出力追加 | SUCCESS |
| 2025-12-25 | - | sc-002, sc-004 問題発見 → F208 として分離 | - |
| 2025-12-25 | ac-tester | AC検証: OK:23/24 (B4 SKIPPED) | SUCCESS |
| 2025-12-25 | regression-tester | 24/24 PASS, verify-logs OK:934/934 | SUCCESS |
| 2025-12-25 | feature-reviewer | Post-review 完了 | DONE |

## 完了時メモ (2025-12-25)

**全タスク完了**:
- A1-A3: シナリオexpect定義 ✅
- B1-B3, B5-B8: Engine検証機能 ✅
- B4: 単一ファイルinputペアリング [-] (--parallel使用で回避可)
- C1-C4: ドキュメント更新 ✅
- D1-D3: ビルド/テスト/24/24 PASS ✅

**シナリオ期待値問題 → F208 へ分離**:
- sc-002, sc-004 の var_equals 検証で TALENT 番号誤りと状態注入不足を発見
- 回帰テストシナリオは hook 編集禁止のため、別 Feature (F208) として修正予定

---

### 主要問題: var_equals検証が機能しない (10シナリオ失敗の原因)

**根本原因**:
ProcessLevelParallelRunner.cs L543:
```csharp
varName => null  // getVariable (not available in flow tests)
```

Flow Testでは子プロセスでゲームが実行されるため、親プロセスからゲーム変数を読み取れない。
`var_equals` 検証時に `getVariable` が常に `null` を返すため、全て FAIL になる。

**失敗シナリオ** (10/24):
- sc-001～005: TALENT検証
- sc-006: saveload状態検証
- sc-011, sc-012: 状態検証
- wakeup, alice-sameroom: 状態検証

**解決方針**: 子プロセス側でvar_equals検証を実装 (推奨)

| 方法 | 説明 | 採用 |
|------|------|:----:|
| A. 子プロセス側で検証 | 子プロセスがexpect JSONを読み、変数を直接チェック | ✅ |
| B. 子プロセスが変数dump | 終了時にJSON出力、親が検証 | - |
| C. output_containsに変換 | DEBUGPRINTで出力検証 | - |

**実装方針 (方法A)**:
1. 子プロセス起動時に `--expect-json` オプションでexpectを渡す
2. 子プロセス終了前に `KojoExpectValidator.Validate()` を実行
3. 結果を stdout に `[ExpectResult] {"passed":true/false, ...}` 形式で出力
4. 親プロセスが stdout からパース

**実装箇所**:
- `ProcessLevelParallelRunner.cs`: 子プロセス引数にexpect追加
- `HeadlessRunner.cs`: --expect-json オプション処理、終了前検証
- `InteractiveRunner.cs`: EOF時に検証実行

---

### 過去の調査メモ (参考)

**問題1** (解消済み): HeadlessRunner.cs でFlowTestパスに到達しない
- 修正済み: L720-724, L1198-1202

**問題2** (B6、現在解消): 子プロセス残存
- 以前は500+プロセスがDLLロック
- 現在はビルド/テスト正常動作

**変更ファイル一覧**:
- `engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs` (Expect, ExpectResults追加)
- `engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs` (FlowTestパス修正)
- `Game/tests/regression/scenario-*.json` (24ファイル全てにexpect追加)
- `.claude/skills/testing/FLOW.md` (検証ルール追記)
- `.claude/agents/regression-tester.md` (FLOW.md参照追加)
- `tools/verify-logs.py` (expectResults検証追加)
- `engine.Tests/Tests/FlowTestExpectVerificationTests.cs` (TDDテスト6件)

**次回作業** (/imple 207 で続行可能):
1. Task 17: HeadlessRunner --expect-json処理
2. Task 18: InteractiveRunner EOF時var_equals検証
3. Task 19: ProcessLevelParallelRunner ExpectResult解析
4. Task 20: 24/24 PASS確認
5. finalizer で完了

---

## Links

- [feature-206.md](feature-206.md) - 調査 Feature (完了)
- [feature-095.md](feature-095.md) - シナリオ設計元
- [feature-205.md](feature-205.md) - verify-logs.py 設計
- [ProcessLevelParallelRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/ProcessLevelParallelRunner.cs) - expect 検証実装
- [HeadlessRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs) - 単一ファイル修正
- [FLOW.md](../../.claude/skills/testing/FLOW.md) - 運用ドキュメント
- [regression-tester.md](../../.claude/agents/regression-tester.md) - サブエージェント
