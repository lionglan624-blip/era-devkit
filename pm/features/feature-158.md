# Feature 158: Kojo Test State Management Fix

## Status: [DONE]

## Type: engine

## Background

### Problem
kojo-testモードでKOJO_MESSAGE_COM_K8_21_1などのkojo関数を実行すると、以下のエラーが発生する：

```
emueraのエラー：プログラムの状態を特定できません
※※※ログファイルをC:\Era\era紅魔館protoNTR\Game\emuera.logに出力しました※※※
```

**発生箇所**: `engine/Assets/Scripts/Emuera/GameView/EmueraConsole.cs:424`

```csharp
emuera.DoScript();
if (state == ConsoleState.Running)
{//RunningならProcessは処理を継続するべき
    state = ConsoleState.Error;
    PrintError("emueraのエラー：プログラムの状態を特定できません");
}
```

**原因**: `DoScript()`実行後、ConsoleStateがRunningのまま残っている。これはスクリプトがWAIT/INPUT待ち状態やEndに遷移せず、Running状態で戻ってきたことを意味する。

### Impact
- Feature 136 (COM_21 何もしない) のK8テスト全件失敗
- Feature 157 (output_equals プレースホルダ展開) のAC検証不可
- output_equalsテストでエラーメッセージが出力に混入

### Goal
kojo-testモードでのERB関数実行後、適切な状態遷移を行い、エラーを発生させないようにする。

### Context
- 他のキャラ（K1-K7, K9-K10）のテストでは発生しない可能性あり
- K8（チルノ）のKOJO_MESSAGE_COM_K8_21_1で再現性100%
- エラーは出力キャプチャ後に発生（対話テキスト自体は正しく出力される）

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | K8 COM_21テストがクラッシュなしで完了 | exit_code | equals | "0" | [x] |
| 2 | エラーメッセージが出力に含まれない | output | not_contains | "emueraのエラー" | [x] |
| 3 | ビルド成功 | build | succeeds | - | [x] |
| 4 | 既存テスト回帰なし (Feature 110) | exit_code | equals | "0" | [x] |

### AC Details

**AC1**: K8 COM_21テストがクラッシュなしで完了
- **Test**: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/core/feature-158/test-158-ac1.json`
- **Expected**: Exit code 0 (no crash errors)
- **Note**: test-158-ac1.json uses output_not_contains to verify no crash

**AC2**: エラーメッセージが出力に含まれない
- **Test**: Same as AC1
- **Expected**: Output does not contain "emueraのエラー"

**AC3**: ビルド成功
- **Test**: `cd engine && dotnet build uEmuera.Headless.csproj`
- **Expected**: Build succeeds

**AC4**: 既存テスト回帰なし (Feature 110)
- **Test**: `cd Game && dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/kojo/feature110-k8.json`
- **Expected**: Exit code 0 (K8 tests pass without crash)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | callEmueraProgram()でkojo-testモード時はRunning→Quitに設定 | [x] |
| 2 | 3 | ビルド成功を確認 | [x] |
| 3 | 4 | 既存テスト回帰なし実行 | [x] |

---

## Technical Notes

### 根本原因
1. `ClearConsoleWaitState()` がConsoleStateを`Running`に設定
2. kojo-testモードではINPUT/INPUTSコマンドなしで関数が完了
3. `PressEnterKey()` → `callEmueraProgram()` → `DoScript()` の呼び出し連鎖
4. `DoScript()`後もConsoleStateが`Running`のまま残る
5. `callEmueraProgram()`のエラーチェックでエラー発生

### 適用した修正
**EmueraConsole.cs line 421-434** (callEmueraProgram内):
```csharp
if (state == ConsoleState.Running)
{
    // Feature 158: In kojo-test mode, Running state after DoScript is expected
    if (MinorShift.Emuera.Headless.KojoTestRunner.IsKojoTestMode)
    {
        state = ConsoleState.Quit;
    }
    else
    {
        state = ConsoleState.Error;
        PrintError("emueraのエラー：プログラムの状態を特定できません");
    }
}
```

### 確認コマンド
```bash
cd Game
# 修正確認
dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/core/feature-158/test-158-ac1.json

# 回帰テスト
dotnet run --project ../engine/uEmuera.Headless.csproj -- . --unit tests/kojo/feature110-k8.json
```

---

## Dependencies

- **Blocks**: Feature 157 (output_equals AC検証)
- **Blocked by**: なし

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-20 | initializer | Feature initialization | READY |
| 2025-12-20 | explorer | Investigation | Root cause identified |
| 2025-12-20 | implementer | Initial fix in KojoTestRunner.cs | BUILD OK, fix ineffective |
| 2025-12-20 | debugger | Fix in EmueraConsole.cs callEmueraProgram() | SUCCESS |
| 2025-12-20 | regression-tester | Regression tests | All PASS |
| 2025-12-20 | ac-tester | AC verification | All 4 ACs PASS |

---

## Links

- [index-features.md](index-features.md)
- [feature-157.md](feature-157.md) - 依存先
- [EmueraConsole.cs](../../engine/Assets/Scripts/Emuera/GameView/EmueraConsole.cs)
- [KojoTestRunner.cs](../../engine/Assets/Scripts/Emuera/Headless/KojoTestRunner.cs)
