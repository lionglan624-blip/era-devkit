# Feature 014: Interface Extraction (IGameBase, IProcess)

## Status: [DONE]

## Overview

GameBaseとProcessクラスにインターフェースを追加し、テスト可能性と依存性注入を改善する。Feature 012/013と同じパターンを適用。

## Current State

### GameBase (GameData/GameBase.cs)

```csharp
internal sealed class GameBase
{
    public string ScriptTitle = "";
    public Int64 ScriptVersion = 0;
    public Int64 DefaultCharacter = -1;
    // ... 設定プロパティ群
}
```

**依存ファイル**: 9ファイル, 19箇所

### Process (GameProc/Process.cs)

```csharp
internal sealed partial class Process
{
    public LabelDictionary LabelDictionary { get; }
    public VariableEvaluator VEvaluator { get; }
    public bool Initialize() { ... }
    // ... 4つのpartialファイルに分散
}
```

**問題点**:
- 具象クラスへの直接依存でモック不可
- ユニットテストでゲーム全体の初期化が必要
- GlobalStaticを通じた暗黙的依存

## Proposed Solution

### Phase 1: IGameBase (低リスク)

```csharp
// Sub/IGameBase.cs
internal interface IGameBase
{
    string ScriptTitle { get; }
    string ScriptAutherName { get; }
    Int64 ScriptVersion { get; }
    Int64 ScriptUniqueCode { get; }
    Int64 DefaultCharacter { get; }
    bool UniqueCodeEqualTo(Int64 target);
    bool CheckVersion(Int64 target);
}
```

### Phase 2: IProcess (中リスク、最小インターフェース)

```csharp
// Sub/IProcess.cs
internal interface IProcess
{
    LabelDictionary LabelDictionary { get; }
    VariableEvaluator VEvaluator { get; }
    LogicalLine getCurrentLine { get; }
}
```

### Migration Strategy

Feature 012/013と同じ委譲パターン:

```csharp
// GlobalStatic.cs
private static IGameBase _gameBase;
public static IGameBase GameBaseInstance
{
    get => _gameBase;
    set => _gameBase = value;
}
// 既存コード互換
public static GameBase GameBase => (GameBase)_gameBase;
```

## Goals

1. **テスト可能性**: GameBase/Processをモック可能に
2. **段階的移行**: 既存コードは変更不要
3. **Feature 012/013との一貫性**: 同じDIパターン

## Acceptance Criteria

- [x] IGameBase interfaceが定義されている
- [x] GameBaseがIGameBaseを実装
- [x] IProcess interfaceが定義されている（最小限）
- [x] ProcessがIProcessを実装
- [x] GlobalStaticにDI用プロパティ追加
- [x] ビルド成功
- [x] 既存コードが動作（回帰テスト）
- [x] 単体テスト: モック注入確認
- [x] engine-reference.md に文書化

## Scope

### In Scope
- IGameBase interface作成
- IProcess interface作成（読み取り専用プロパティのみ）
- GlobalStatic.cs修正（DI用プロパティ）
- 単体テスト

### Out of Scope
- 既存9ファイルのリファクタリング（後方互換性で不要）
- Process全メソッドのインターフェース化（大きすぎる）
- VariableEvaluator/LabelDictionaryのインターフェース化（別Feature）

## Effort Estimate

- **Size**: Small-Medium (1-2セッション)
- **Risk**: 🟡 Medium（GlobalStatic変更あり）
- **Files**: 2 create, 3 modify

## Dependencies

- Feature 012 (WarningCollector) - 同パターン参照
- Feature 013 (FontManager) - 同パターン参照

## Risks

| Risk | Mitigation |
|------|------------|
| GlobalStatic変更による副作用 | 既存プロパティは維持、新プロパティ追加のみ |
| partial classの複雑さ | IProcessは最小限のプロパティのみ |

---

## Links

- [GameBase.cs](../../uEmuera/Assets/Scripts/Emuera/GameData/GameBase.cs)
- [Process.cs](../../uEmuera/Assets/Scripts/Emuera/GameProc/Process.cs)
- [GlobalStatic.cs](../../uEmuera/Assets/Scripts/Emuera/GlobalStatic.cs)
- [index-features.md](index-features.md) - Feature tracking
- [feature-012.md](feature-012.md) - Reference pattern
- [feature-013.md](feature-013.md) - Reference pattern
