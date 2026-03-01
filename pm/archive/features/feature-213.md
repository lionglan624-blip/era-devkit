# Feature 213: Flow Parallel Thread Safety

## Status: [DONE]

## Type: engine

## Depends: [212]

## Background

### Problem

F212 (CI Integration) 実装中に発見。`--flow` テストを並列実行（`--parallel N`, N > 1）すると、テスト結果がフレーキー（不安定）になる。

**観察された動作**:
- `--parallel 20` で 24 テスト実行 → 22-24/24 PASS（実行ごとに異なるテストが FAIL）
- `--parallel 1` で同じテスト実行 → 24/24 PASS（常に成功）
- FAIL するテストは毎回異なる（sc-002, sc-004 など）

### Root Cause (REVISED)

当初は `[ThreadStatic]` 問題と仮定したが、詳細調査により**真の根本原因**を特定:

**AutoSave ファイル競合問題**

全ての子プロセスが同じ `save99.sav` ファイルにオートセーブを試みる:

```csharp
// Process.SystemProc.cs
const int AutoSaveIndex = 99;

void endAutoSaveCallSaveInfo()
{
    if (saveTarget == AutoSaveIndex)
    {
        if (!vEvaluator.SaveTo(saveTarget, vEvaluator.SAVEDATA_TEXT))
        {
            console.PrintError("オートセーブ中に予期しないエラーが発生しました");
            console.PrintError("オートセーブをスキップします");
            console.ReadAnyKey();  // <-- 入力を消費！
        }
    }
    endAutoSave();
}
```

**競合シナリオ**:
1. 20個の子プロセスが同時に起動
2. 各プロセスがショップ画面遷移時に `save99.sav` への書き込みを試みる
3. ファイルロックにより一部プロセスの書き込みが失敗
4. エラーメッセージ後の `ReadAnyKey()` が入力キューから値を消費
5. 後続の入力シーケンスがずれ、ゲームが期待通りに進まない
6. テスト期待値（例: `[恋慕]を得た`）が出力されず FAIL

**証拠**:
- 失敗テストの結果JSONに "オートセーブ中に予期しないエラーが発生しました" 出力あり
- 子プロセス単体実行では常に成功
- シェルからの手動並列実行では成功（タイミングが異なる）

### Impact

- 並列テスト実行が信頼できない
- F212 で `--parallel 1` の暫定回避が必要（実行時間増加: 4秒→38秒）

### Goal

1. ~~`ScenarioParser` と `PostWakeupInjector` をスレッドセーフにする~~ (不要と判明)
2. AutoSave を headless モードで無効化
3. `--parallel N` (N > 1) で安定した結果を得る
4. pre-commit hook を並列実行に戻す（高速化）

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | HeadlessRunner: AutoSave = false 設定追加 | code | grep | contains | AutoSave = false | [x] |
| 2 | --parallel 20 で 24/24 PASS x10安定 (ポジ) | test | --flow | equals | 24/24 | [x] |
| 3 | --parallel 1 で 24/24 PASS (基準確認) | test | --flow | equals | 24/24 | [x] |
| 4 | dotnet build 成功 | build | dotnet | succeeds | - | [x] |
| 5 | pre-commit hook を auto-parallel に変更 | file | grep | not_contains | "--parallel 1" | [x] |

### AC Details

**AC2/AC3 Test**:
```bash
# 並列実行 (10回連続で安定確認)
for i in 1 2 3 4 5 6 7 8 9 10; do
  dotnet run --project ../engine/uEmuera.Headless.csproj -- . \
    --flow "tests/regression/scenario-*.json" --parallel 20
done
# Result: 24/24 passed x 10 consecutive runs

# 直列実行 (基準確認)
dotnet run --project ../engine/uEmuera.Headless.csproj -- . \
  --flow "tests/regression/scenario-*.json" --parallel 1
# Result: 24/24 passed
```

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | HeadlessRunner: AutoSave = false 追加 | [x] |
| 2 | 2 | 並列テスト (--parallel 20) 10回連続実行で安定性確認 | [x] |
| 3 | 3 | 直列テスト (--parallel 1) 正常動作確認 | [x] |
| 4 | 4 | dotnet build 確認 | [x] |
| 5 | 5 | pre-commit hook: --parallel 1 → auto-parallel に変更 | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Technical Details

### Task 1: HeadlessRunner AutoSave 無効化

**修正箇所**: `HeadlessRunner.cs` の `Run()` メソッド

**Before**:
```csharp
// Load configuration
ConfigData.Instance.LoadConfig();

// Feature 101: Reset warning counter before loading in strict mode
```

**After**:
```csharp
// Load configuration
ConfigData.Instance.LoadConfig();

// Feature 213: Disable auto-save in headless mode to prevent file contention
// when multiple child processes run in parallel. Auto-save writes to save99.sav
// which causes file lock conflicts and disrupts input queue synchronization.
GlobalStatic.ConfigServiceInstance.AutoSave = false;

// Feature 101: Reset warning counter before loading in strict mode
```

### 調査過程で検討したが不要と判明した対策

当初の仮説では `[ThreadStatic]` が必要と考えたが、以下の理由で不要:

1. `ProcessLevelParallelRunner` は各テストを**別プロセス**として起動
2. 各プロセスは独立したメモリ空間を持つ
3. `[ThreadStatic]` は同一プロセス内のスレッド間共有を防ぐもの
4. プロセス間ではそもそも static フィールドは共有されない

真の問題は**ファイルシステムの共有**（save99.sav）であった。

### 補足: ThreadStatic が必要になるケース

将来的にプロセスレベルではなくスレッドレベルの並列化を検討する場合は、以下の対策が必要:

- `ScenarioParser`: `pendingScenario_`, `characterInjectionDone_` 等
- `PostWakeupInjector`: `pendingAssignments_`, `injectionDone_` 等
- `KojoTestRunner`: `isKojoTestMode_`, `inputQueue_` 等

現在の実装ではプロセス分離により問題は発生しない。

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-25 | - | F212 実装中に並列テストのフレーキー問題を発見 | PROPOSED |
| 2025-12-25 | debugger-opus | 深掘り調査: ThreadStatic仮説を否定、AutoSave競合を特定 | ROOT_CAUSE |
| 2025-12-25 | debugger-opus | HeadlessRunner.cs: AutoSave = false 追加 | FIXED |
| 2025-12-25 | debugger-opus | 20回連続テスト: 24/24 x 20 = 100% 安定 | VERIFIED |

---

## Links

- [feature-212.md](feature-212.md) - CI Integration（問題発見元）
- [feature-088.md](feature-088.md) - Regression Test Infrastructure（関連）
- [HeadlessRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs) - 修正箇所
- [Process.SystemProc.cs](../../engine/Assets/Scripts/Emuera/GameProc/Process.SystemProc.cs) - AutoSave処理
- [pre-commit](../../.githooks/pre-commit) - CI hook (Task 5で変更済み)
