# Feature 005: Process.ScriptProc Command Pattern Refactoring

## Status: [DONE]

## Overview

uEmuera エンジンの `Process.ScriptProc.cs` にある巨大な switch 文（約790行）をコマンドパターンにリファクタリング。保守性・テスタビリティ・拡張性を向上させる。

## Problem

現状の `Process.ScriptProc.cs`:
- **doNormalFunction()**: 660行の switch 文、47 case
- **doFlowControlFunction()**: 130行の switch 文、7 case
- 単一責任原則違反（全命令が1メソッドに集約）
- 個別命令のユニットテスト困難
- 新命令追加時に巨大ファイル編集が必要

## Goals

1. **コマンドパターン導入**: 各 FunctionCode を個別クラスに分離
2. **テスタビリティ向上**: 命令ごとにユニットテスト可能に
3. **拡張性向上**: 新命令追加が容易に
4. **段階的移行**: 既存動作を壊さず漸進的にリファクタ

## Technical Design

### 現状アーキテクチャ

```
Process.ScriptProc.cs (955行)
├── runScriptProc()           # メインループ (lines 17-89)
├── doNormalFunction()        # 660行 switch (lines 102-764)
│   ├── Print系: PRINTBUTTON, PRINTBUTTONC/LC, PRINTPLAIN, DRAWLINE, CUSTOMDRAWLINE
│   ├── Print系: PRINT_ABL/TALENT/MARK/EXP, PRINT_PALAM, PRINT_ITEM, PRINT_SHOPITEM
│   ├── Character系: UPCHECK, CUPCHECK, DELALLCHARA, PICKUPCHARA, ADDDEFCHARA
│   ├── Save/Config: PUTFORM, QUIT, VARSIZE, SAVEDATA, PRINTCPERLINE, SAVENOS
│   ├── Math/Data: POWER, SWAP, GETTIME, SPLIT, ENCODETOUNI, STRDATA
│   ├── Style系: SETCOLOR, SETCOLORBYNAME, SETBGCOLOR, SETBGCOLORBYNAME, FONTSTYLE, SETFONT, ALIGNMENT
│   ├── Control: REDRAW, RESET_STAIN, FORCEKANA, SKIPDISP, NOSKIP, ENDNOSKIP
│   ├── Array系: ARRAYSHIFT, ARRAYREMOVE, ARRAYSORT, ARRAYCOPY
│   └── Other: OUTPUTLOG, ASSERT, THROW, CLEARTEXTBOX
└── doFlowControlFunction()   # 130行 switch (lines 773-903)
    ├── LOADDATA
    ├── TRYCALLLIST/TRYJUMPLIST
    ├── TRYGOTOLIST
    ├── CALLTRAIN
    ├── STOPCALLTRAIN
    └── DOTRAIN
```

### 依存関係

```
Process.ScriptProc
├── ExpressionMediator exm     # 式評価
│   ├── Console               # 出力
│   └── VEvaluator            # 変数評価
├── VariableEvaluator vEvaluator  # 直接参照
├── ProcessState state         # 実行状態
├── EmueraConsole console      # 直接参照
├── bool skipPrint            # スキップ状態
├── bool saveSkip             # NOSKIP用保存
└── bool userDefinedSkip      # ユーザー定義スキップ
```

### 目標アーキテクチャ

```
GameProc/
├── Commands/
│   ├── IScriptCommand.cs              # 通常コマンド用インターフェース
│   ├── IFlowCommand.cs                # フロー制御用インターフェース
│   ├── ScriptCommandContext.cs        # 実行コンテキスト（依存オブジェクトを集約）
│   ├── CommandRegistry.cs             # FunctionCode → Command マッピング
│   ├── Print/                         # 12コマンド
│   │   ├── PrintButtonCommand.cs      # PRINTBUTTON
│   │   ├── PrintButtonCCommand.cs     # PRINTBUTTONC, PRINTBUTTONLC
│   │   ├── PrintPlainCommand.cs       # PRINTPLAIN, PRINTPLAINFORM
│   │   ├── DrawLineCommand.cs         # DRAWLINE
│   │   ├── CustomDrawLineCommand.cs   # CUSTOMDRAWLINE, DRAWLINEFORM
│   │   ├── PrintCharDataCommand.cs    # PRINT_ABL/TALENT/MARK/EXP
│   │   ├── PrintPalamCommand.cs       # PRINT_PALAM
│   │   ├── PrintItemCommand.cs        # PRINT_ITEM
│   │   └── PrintShopItemCommand.cs    # PRINT_SHOPITEM
│   ├── Character/                     # 5コマンド
│   │   ├── UpcheckCommand.cs          # UPCHECK
│   │   ├── CupcheckCommand.cs         # CUPCHECK
│   │   ├── DelAllCharaCommand.cs      # DELALLCHARA
│   │   ├── PickupCharaCommand.cs      # PICKUPCHARA
│   │   └── AddDefCharaCommand.cs      # ADDDEFCHARA
│   ├── Style/                         # 8コマンド
│   │   ├── SetColorCommand.cs         # SETCOLOR
│   │   ├── SetColorByNameCommand.cs   # SETCOLORBYNAME
│   │   ├── SetBgColorCommand.cs       # SETBGCOLOR
│   │   ├── SetBgColorByNameCommand.cs # SETBGCOLORBYNAME
│   │   ├── FontStyleCommand.cs        # FONTSTYLE
│   │   ├── SetFontCommand.cs          # SETFONT
│   │   └── AlignmentCommand.cs        # ALIGNMENT
│   ├── Array/                         # 4コマンド
│   │   ├── ArrayShiftCommand.cs       # ARRAYSHIFT
│   │   ├── ArrayRemoveCommand.cs      # ARRAYREMOVE
│   │   ├── ArraySortCommand.cs        # ARRAYSORT
│   │   └── ArrayCopyCommand.cs        # ARRAYCOPY
│   ├── Control/                       # 7コマンド
│   │   ├── RedrawCommand.cs           # REDRAW
│   │   ├── ResetStainCommand.cs       # RESET_STAIN
│   │   ├── ForceKanaCommand.cs        # FORCEKANA
│   │   ├── SkipDispCommand.cs         # SKIPDISP
│   │   └── NoSkipCommand.cs           # NOSKIP, ENDNOSKIP
│   ├── Data/                          # 6コマンド
│   │   ├── PowerCommand.cs            # POWER
│   │   ├── SwapCommand.cs             # SWAP
│   │   ├── GetTimeCommand.cs          # GETTIME
│   │   ├── SplitCommand.cs            # SPLIT
│   │   ├── EncodeToUniCommand.cs      # ENCODETOUNI
│   │   └── StrDataCommand.cs          # STRDATA
│   ├── System/                        # 5コマンド
│   │   ├── PutFormCommand.cs          # PUTFORM
│   │   ├── QuitCommand.cs             # QUIT
│   │   ├── VarSizeCommand.cs          # VARSIZE
│   │   ├── SaveDataCommand.cs         # SAVEDATA
│   │   └── OutputLogCommand.cs        # OUTPUTLOG
│   ├── Config/                        # 2コマンド
│   │   ├── PrintCPerLineCommand.cs    # PRINTCPERLINE
│   │   └── SaveNosCommand.cs          # SAVENOS
│   ├── Debug/                         # 3コマンド
│   │   ├── AssertCommand.cs           # ASSERT
│   │   ├── ThrowCommand.cs            # THROW
│   │   └── ClearTextBoxCommand.cs     # CLEARTEXTBOX
│   └── Flow/                          # 7コマンド
│       ├── LoadDataCommand.cs         # LOADDATA
│       ├── TryCallListCommand.cs      # TRYCALLLIST, TRYJUMPLIST
│       ├── TryGotoListCommand.cs      # TRYGOTOLIST
│       ├── CallTrainCommand.cs        # CALLTRAIN
│       ├── StopCallTrainCommand.cs    # STOPCALLTRAIN
│       └── DoTrainCommand.cs          # DOTRAIN
└── Process.ScriptProc.cs              # 簡略化されたディスパッチャ
```

### インターフェース設計

```csharp
/// <summary>実行コンテキスト - コマンドが必要とする全依存関係を集約</summary>
public class ScriptCommandContext
{
    public ExpressionMediator Exm { get; }
    public VariableEvaluator VEvaluator { get; }
    public EmueraConsole Console { get; }
    public ProcessState State { get; }

    // スキップ状態は Process から参照（コマンドが変更する可能性あり）
    public Func<bool> GetSkipPrint { get; }
    public Action<bool> SetSkipPrint { get; }
    public Action<bool> SetUserDefinedSkip { get; }

    // NOSKIP用の状態保存
    public Func<bool> GetSaveSkip { get; }
    public Action<bool> SetSaveSkip { get; }
}

/// <summary>通常コマンド用インターフェース</summary>
public interface IScriptCommand
{
    FunctionCode[] SupportedCodes { get; }  // 複数コードを1クラスで処理可能に
    void Execute(ScriptCommandContext ctx, InstructionLine func);
}

/// <summary>フロー制御コマンド用インターフェース</summary>
public interface IFlowCommand
{
    FunctionCode[] SupportedCodes { get; }
    /// <returns>true: 次の行へ継続, false: 処理を中断して呼び出し元へ戻る</returns>
    bool Execute(ScriptCommandContext ctx, InstructionLine func);
}

/// <summary>コマンドレジストリ - FunctionCode から適切なコマンドへディスパッチ</summary>
public class CommandRegistry
{
    private readonly Dictionary<FunctionCode, IScriptCommand> _normalCommands = new();
    private readonly Dictionary<FunctionCode, IFlowCommand> _flowCommands = new();

    public void Register(IScriptCommand command)
    {
        foreach (var code in command.SupportedCodes)
            _normalCommands[code] = command;
    }

    public void Register(IFlowCommand command)
    {
        foreach (var code in command.SupportedCodes)
            _flowCommands[code] = command;
    }

    public bool TryExecute(ScriptCommandContext ctx, InstructionLine func)
    {
        if (_normalCommands.TryGetValue(func.FunctionCode, out var cmd))
        {
            cmd.Execute(ctx, func);
            return true;
        }
        return false;  // フォールバックへ
    }

    public (bool found, bool continueExec) TryExecuteFlow(ScriptCommandContext ctx, InstructionLine func)
    {
        if (_flowCommands.TryGetValue(func.FunctionCode, out var cmd))
            return (true, cmd.Execute(ctx, func));
        return (false, true);  // フォールバックへ
    }
}
```

### コマンド実装例

```csharp
/// <summary>PRINTBUTTON コマンド</summary>
public class PrintButtonCommand : IScriptCommand
{
    public FunctionCode[] SupportedCodes => new[] { FunctionCode.PRINTBUTTON };

    public void Execute(ScriptCommandContext ctx, InstructionLine func)
    {
        if (ctx.GetSkipPrint()) return;

        ctx.Console.UseUserStyle = true;
        ctx.Console.UseSetColorStyle = true;

        var bArg = (SpButtonArgument)func.Argument;
        var str = bArg.PrintStrTerm.GetStrValue(ctx.Exm).Replace("\n", "");

        if (bArg.ButtonWord.GetOperandType() == typeof(long))
            ctx.Console.PrintButton(str, bArg.ButtonWord.GetIntValue(ctx.Exm));
        else
            ctx.Console.PrintButton(str, bArg.ButtonWord.GetStrValue(ctx.Exm));
    }
}

/// <summary>PRINTBUTTONC, PRINTBUTTONLC コマンド（複数コード対応例）</summary>
public class PrintButtonCCommand : IScriptCommand
{
    public FunctionCode[] SupportedCodes => new[] {
        FunctionCode.PRINTBUTTONC,
        FunctionCode.PRINTBUTTONLC
    };

    public void Execute(ScriptCommandContext ctx, InstructionLine func)
    {
        if (ctx.GetSkipPrint()) return;

        ctx.Console.UseUserStyle = true;
        ctx.Console.UseSetColorStyle = true;

        var bArg = (SpButtonArgument)func.Argument;
        var str = bArg.PrintStrTerm.GetStrValue(ctx.Exm).Replace("\n", "");
        bool isRight = func.FunctionCode == FunctionCode.PRINTBUTTONC;

        if (bArg.ButtonWord.GetOperandType() == typeof(long))
            ctx.Console.PrintButtonC(str, bArg.ButtonWord.GetIntValue(ctx.Exm), isRight);
        else
            ctx.Console.PrintButtonC(str, bArg.ButtonWord.GetStrValue(ctx.Exm), isRight);
    }
}

/// <summary>LOADDATA フローコマンド（戻り値あり例）</summary>
public class LoadDataCommand : IFlowCommand
{
    public FunctionCode[] SupportedCodes => new[] { FunctionCode.LOADDATA };

    public bool Execute(ScriptCommandContext ctx, InstructionLine func)
    {
        var intExpArg = (ExpressionArgument)func.Argument;
        long target = intExpArg.Term.GetIntValue(ctx.Exm);

        if (target < 0)
            throw new CodeEE($"LOADDATAの引数に負の値({target})が指定されました");
        if (target > int.MaxValue)
            throw new CodeEE($"LOADDATAの引数({target})が大きすぎます");

        var result = ctx.VEvaluator.CheckData((int)target, EraSaveFileType.Normal);
        if (result.State != EraDataState.OK)
            throw new CodeEE("不正なデータをロードしようとしました");

        if (!ctx.VEvaluator.LoadFrom((int)target))
            throw new ExeEE("ファイルのロード中に予期しないエラーが発生しました");

        ctx.State.ClearFunctionList();
        ctx.State.SystemState = SystemStateCode.LoadData_DataLoaded;
        return false;  // 処理中断
    }
}
```

### 移行戦略

#### Phase 1: 基盤構築 (4ファイル)
- `ScriptCommandContext.cs` - 依存関係を集約するコンテキストクラス
- `IScriptCommand.cs` - 通常コマンド用インターフェース
- `IFlowCommand.cs` - フロー制御用インターフェース
- `CommandRegistry.cs` - コマンド登録・ディスパッチ
- `Process.ScriptProc.cs` - レジストリ統合、フォールバック維持

#### Phase 2: Print系コマンド移行 (9クラス, 12コマンド)
- PrintButtonCommand (PRINTBUTTON)
- PrintButtonCCommand (PRINTBUTTONC, PRINTBUTTONLC)
- PrintPlainCommand (PRINTPLAIN, PRINTPLAINFORM)
- DrawLineCommand (DRAWLINE)
- CustomDrawLineCommand (CUSTOMDRAWLINE, DRAWLINEFORM)
- PrintCharDataCommand (PRINT_ABL, PRINT_TALENT, PRINT_MARK, PRINT_EXP)
- PrintPalamCommand (PRINT_PALAM)
- PrintItemCommand (PRINT_ITEM)
- PrintShopItemCommand (PRINT_SHOPITEM)

#### Phase 3: Flow系コマンド移行 (6クラス, 7コマンド)
- LoadDataCommand (LOADDATA)
- TryCallListCommand (TRYCALLLIST, TRYJUMPLIST)
- TryGotoListCommand (TRYGOTOLIST)
- CallTrainCommand (CALLTRAIN)
- StopCallTrainCommand (STOPCALLTRAIN)
- DoTrainCommand (DOTRAIN)

#### Phase 4: 残りコマンド移行 (27クラス, 35コマンド)
- **Character系** (5): UPCHECK, CUPCHECK, DELALLCHARA, PICKUPCHARA, ADDDEFCHARA
- **Style系** (7): SETCOLOR, SETCOLORBYNAME, SETBGCOLOR, SETBGCOLORBYNAME, FONTSTYLE, SETFONT, ALIGNMENT
- **Array系** (4): ARRAYSHIFT, ARRAYREMOVE, ARRAYSORT, ARRAYCOPY
- **Control系** (6): REDRAW, RESET_STAIN, FORCEKANA, SKIPDISP, NOSKIP, ENDNOSKIP
- **Data系** (6): POWER, SWAP, GETTIME, SPLIT, ENCODETOUNI, STRDATA
- **System系** (4): PUTFORM, QUIT, VARSIZE, SAVEDATA
- **Config系** (2): PRINTCPERLINE, SAVENOS
- **Debug系** (3): ASSERT, THROW, CLEARTEXTBOX
- **Other**: OUTPUTLOG

#### Phase 5: クリーンアップ
- switch 文から移行済みcaseを削除
- Process.ScriptProc.cs の簡略化
- 未使用コードの削除
- ドキュメント整備

## File Changes

| File | Change |
|------|--------|
| `GameProc/Commands/ScriptCommandContext.cs` | 新規: 実行コンテキスト |
| `GameProc/Commands/IScriptCommand.cs` | 新規: 通常コマンドIF |
| `GameProc/Commands/IFlowCommand.cs` | 新規: フロー制御IF |
| `GameProc/Commands/CommandRegistry.cs` | 新規: コマンド登録・ディスパッチ |
| `GameProc/Commands/Print/*.cs` | 新規: Print系コマンド (9クラス) |
| `GameProc/Commands/Character/*.cs` | 新規: Character系コマンド (5クラス) |
| `GameProc/Commands/Style/*.cs` | 新規: Style系コマンド (7クラス) |
| `GameProc/Commands/Array/*.cs` | 新規: Array系コマンド (4クラス) |
| `GameProc/Commands/Control/*.cs` | 新規: Control系コマンド (5クラス) |
| `GameProc/Commands/Data/*.cs` | 新規: Data系コマンド (6クラス) |
| `GameProc/Commands/System/*.cs` | 新規: System系コマンド (5クラス) |
| `GameProc/Commands/Config/*.cs` | 新規: Config系コマンド (2クラス) |
| `GameProc/Commands/Debug/*.cs` | 新規: Debug系コマンド (3クラス) |
| `GameProc/Commands/Flow/*.cs` | 新規: Flow系コマンド (6クラス) |
| `GameProc/Process.ScriptProc.cs` | 変更: レジストリ統合、フォールバック |
| `GameProc/Process.cs` | 変更: CommandRegistry初期化 |

## Acceptance Criteria

### Phase 1 ✓
- [x] ScriptCommandContext クラス定義
- [x] IScriptCommand インターフェース定義
- [x] IFlowCommand インターフェース定義
- [x] CommandRegistry 実装
- [x] Process.ScriptProc.cs にフォールバック付きレジストリ統合

### Phase 2 ✓
- [x] Print系コマンド 9クラス実装
- [x] 既存動作維持確認（フォールバック維持、段階的削除可能）

### Phase 3 ✓
- [x] Flow系コマンド 6クラス実装
- [x] フロー制御の戻り値動作確認

### Phase 4 ✓
- [x] 残り 37クラス実装（8カテゴリ：Character, Style, Array, Control, Data, System, Config, Debug）

### Phase 5 ✓
- [x] ドキュメント更新
- [ ] switch文削除（フォールバック維持のためオプショナル）

### 全体 ✓
- [x] パフォーマンス劣化なし（Dictionary lookup は O(1)）
- [x] 既存テスト（E2Eヘッドレステスト）がすべてパス

## Risks

| Risk | Mitigation |
|------|------------|
| パフォーマンス劣化 | Dictionary lookup は switch の O(log n) より高速、実測で確認 |
| 既存動作の破壊 | 各Phase完了時にE2Eテスト実行、フォールバック維持 |
| 大規模変更による混乱 | Phase分割、各Phase完了後にコミット |
| 依存関係の複雑化 | ScriptCommandContext で依存を明示的に管理 |

## Dependencies

- uEmuera コードベースへの理解 ✓
- 既存の `Instruction` パターン（`FunctionMethodCreator.cs` 参照）
- Process.ScriptProc.cs の内部状態（skipPrint, saveSkip 等）

## Estimated Effort

| Phase | Tasks | Files | Estimate |
|-------|-------|-------|----------|
| Phase 1 | 基盤構築 | 5 | Small |
| Phase 2 | Print系移行 | 9 | Med |
| Phase 3 | Flow系移行 | 6 | Med |
| Phase 4 | 残り移行 | 27 | Large |
| Phase 5 | クリーンアップ | 2 | Small |
| **Total** | | **49** | **Large** |

## Links

- [index-features.md](../index-features.md) - Feature index
- [WBS-005.md](WBS-005.md) - Work breakdown structure
- [engine-reference.md](../reference/engine-reference.md) - Engine architecture
- [Process.ScriptProc.cs](../../../uEmuera/Assets/Scripts/Emuera/GameProc/Process.ScriptProc.cs) - Target file
