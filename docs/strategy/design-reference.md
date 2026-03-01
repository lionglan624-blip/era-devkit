# Design Reference

**Parent**: [full-csharp-architecture.md](../full-csharp-architecture.md)

Cross-cutting design guidelines and architecture patterns used across all phases.

---

## Type Design Guidelines

**Purpose**: Phase 4以降で適用する型設計パターン。ERA特有の課題に対応し、技術的負債0を目指す。

### Strongly Typed IDs（必須）

生`int`の混同を防ぎ、コンパイル時に誤用を検出する。

```csharp
// 定義
public readonly record struct CharacterId(int Value);
public readonly record struct LocationId(int Value);
public readonly record struct ItemId(int Value);

// 使用例：コンパイルエラーで誤用を防ぐ
void Move(CharacterId who, LocationId where);
// Move(locationId, characterId); // コンパイルエラー
```

| ERB現状 | 問題 | C#対策 |
|---------|------|--------|
| `int`が全てを表現 | CharID=1, LocationID=1 が混同可能 | Strongly Typed ID |
| `TARGET`, `MASTER` | 役割不明 | `CharacterId target`, `CharacterId master` |

### Flag/State ラッパー（必須）

マジックナンバーを排除し、意味を明示する。

```csharp
// enum で意味を明示
public enum CharacterFlag { 居場所 = 0, 好感度 = 1, 体力 = 2, ... }

// 型安全アクセス
public sealed class CharacterState
{
    public int GetFlag(CharacterFlag flag) => _flags[(int)flag];
    public void SetFlag(CharacterFlag flag, int value) { ... }
}
```

| ERB現状 | 問題 | C#対策 |
|---------|------|--------|
| `FLAG:N`, `TFLAG:N` | 生配列、意味不明 | 型付きラッパー + enum index |
| `CFLAG:CharID:Index` | 2D配列、マジックナンバー | `Character.GetFlag(FlagType)` |

### Result型（推奨）

失敗を明示的に扱い、暗黙の失敗を防ぐ。

```csharp
// 軽量自作版
public readonly record struct Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    public static Result<T> Ok(T value) => new(true, value, null);
    public static Result<T> Fail(string error) => new(false, default, error);
}

// 使用例
public Result<CharacterState> LoadCharacter(CharacterId id)
{
    if (!_characters.TryGetValue(id, out var state))
        return Result<CharacterState>.Fail($"Character {id.Value} not found");
    return Result<CharacterState>.Ok(state);
}
```

### Exception vs Result<T> 使い分け（必須）

エラーハンドリング方針を統一し、一貫した処理を保証する。

| シナリオ | 推奨 | 理由 |
|----------|------|------|
| 回復可能な失敗（変数未定義、キャラ不在等） | `Result<T>.Fail()` | 呼び出し元で判断可能 |
| プログラマエラー（null参照、引数不正等） | `ArgumentNullException` | バグとして即座に検出 |
| 致命的エラー（ファイル破損、DB接続不可等） | 例外 + ログ | 上位でキャッチして終了 |
| 外部API/IO操作 | `Result<T>` でラップ | 失敗は想定内 |

```csharp
// 回復可能 -> Result<T>
public Result<CharacterState> GetCharacter(CharacterId id)
{
    if (!_characters.TryGetValue(id, out var state))
        return Result<CharacterState>.Fail($"Character {id.Value} not found");
    return Result<CharacterState>.Ok(state);
}

// プログラマエラー -> 例外（早期発見）
public void UpdateCharacter(CharacterId id, CharacterState state)
{
    ArgumentNullException.ThrowIfNull(state);
    // ...
}

// 致命的エラー -> 例外 + ログ
public Result<GameState> LoadSave(string path)
{
    try
    {
        var json = File.ReadAllText(path);
        return Result<GameState>.Ok(JsonSerializer.Deserialize<GameState>(json)!);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to load save file: {Path}", path);
        return Result<GameState>.Fail($"Save file corrupted: {ex.Message}");
    }
}
```

### DI対応設計（必須）

グローバル状態を排除し、テスタビリティを確保する。

```csharp
// インターフェース定義
public interface IGameState
{
    IReadOnlyDictionary<CharacterId, CharacterState> Characters { get; }
    int GetFlag(int index);
    void SetFlag(int index, int value);
}

// DI注入
public class NtrEngine
{
    private readonly IGameState _state;

    public NtrEngine(IGameState state) => _state = state;
}
```

| 原則 | ERA適用 |
|------|---------|
| **SRP** | 責務別クラス分割（ERB 1ファイル -> 複数クラス） |
| **ISP** | 小さなインターフェース（`IGameState`, `ICharacterRepository`） |
| **DIP** | 抽象に依存（`IGameState` 注入、グローバル変数排除） |

### Domain Value Objects（推奨）

ビジネスルールを型で表現する。

```csharp
// 時間（ERA独自の時間システム）
public readonly record struct GameTime(int Hour, int Day, int Month);

// 好感度（範囲制約付き）
public readonly record struct Affection
{
    public int Value { get; }
    public Affection(int value) => Value = Math.Clamp(value, 0, 1000);
}

// 不変レコード（ゲーム状態スナップショット）
public sealed record GameSnapshot(
    ImmutableDictionary<CharacterId, CharacterState> Characters,
    ImmutableArray<int> Flags,
    GameTime CurrentTime
);
```

### 推奨ライブラリ

| 用途 | 推奨 | 理由 |
|------|------|------|
| DI | `Microsoft.Extensions.DependencyInjection` | 標準、軽量 |
| Result型 | 自作 or `OneOf` | 軽量、学習コスト低 |
| Immutable | `System.Collections.Immutable` | 標準ライブラリ |
| Validation | `FluentValidation` | 宣言的、テスト容易 |
| JSON | `System.Text.Json` | 標準、高速 |

### プロジェクト設定

```xml
<!-- Era.Core.csproj -->
<PropertyGroup>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>
```

---

## C# 14 Patterns

### Primary Constructor with DI

**Pattern**: Use primary constructors for dependency injection in command handlers and processors.

```csharp
// Primary Constructor with DI - Command Handler pattern
public class MarkSystemProcessor(
    IVariableStore variables,
    ICharacterStateVariables stateVariables,
    ITrainingVariables trainingVariables) : IMarkSystem
{
    public Result<MarkChange> CalculateMarkGrowth(CharacterId target, MarkIndex markType)
    {
        // Use injected dependencies directly (no field declarations needed)
        var resistTalent = variables.GetTalent(target, TalentIndex.Resistant);
        var tcvar = trainingVariables.GetTCVar(target, TCVarIndex.PleasureIntensity);

        return resistTalent.Bind(resist =>
            tcvar.Bind(intensity =>
                Result<MarkChange>.Success(new MarkChange(markType, intensity - resist))));
    }
}
```

**Benefits**:
- Eliminates boilerplate field declarations and constructor assignments
- Dependencies visible at class declaration (improved readability)
- Works seamlessly with ASP.NET Core DI and generic host
- Parameters become private readonly fields automatically

**When to Use**:
- Command handlers (ICommandHandler<TCommand, TResult>)
- Domain processors (ITrainingProcessor, IMarkSystem, etc.)
- Service classes with constructor injection
- Any class requiring 2+ injected dependencies

**Constraints**:
- Parameters become private fields (cannot be made public)
- Cannot mix primary constructor with explicit field initializers for same parameters
- Base class constructor calls must use primary constructor syntax

### Extension Members for Result<T>

**Pattern**: Add composition methods to Result<T> using extension members for fluent monadic operations.

```csharp
// src/Era.Core/Types/ResultExtensions.cs
public static class ResultExtensions
{
    // Bind for monadic composition
    public static Result<U> Bind<T, U>(this Result<T> result, Func<T, Result<U>> binder)
    {
        return result.Match(
            onSuccess: binder,
            onFailure: error => Result<U>.Fail(error)
        );
    }

    // Map for value transformation
    public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> mapper)
    {
        return result.Match(
            onSuccess: value => Result<U>.Success(mapper(value)),
            onFailure: error => Result<U>.Fail(error)
        );
    }

    // Flatten nested Result<Result<T>>
    public static Result<T> Flatten<T>(this Result<Result<T>> nestedResult)
    {
        return nestedResult.Match(
            onSuccess: inner => inner,
            onFailure: error => Result<T>.Fail(error)
        );
    }
}

// Usage: Chain operations without explicit error checking
public Result<TrainingResult> ProcessTraining(CharacterId target, CommandId command)
{
    return variableStore.GetAbility(target, AbilityIndex.Obedient)
        .Bind(ability => trainingVariables.GetBase(target, BaseIndex.Mood)
        .Bind(mood => CalculateResult(ability, mood)));
}
```

### Collection Expressions for Initialization

**Pattern**: Use `[..]` syntax for concise collection initialization, especially for StateChange lists and test data.

```csharp
// StateChange collection initialization
public TrainingResult ProcessVirginity(CharacterId target)
{
    List<StateChange> changes = [
        new TalentChange(TalentIndex.Virginity, -1),
        new ExpChange(ExpIndex.VExp, 100),
        new TCVarChange(TCVarIndex.Defloration, 1),
        new MarkHistoryChange(MarkIndex.ResistanceHistory, 1)
    ];

    return changes.ToTrainingResult(target);
}

// Character group definitions
CharacterId[] mainCharacters = [
    CharacterId.Meiling,
    CharacterId.Sakuya,
    CharacterId.Patchouli,
    CharacterId.Remilia
];

// Spread operator for combining collections
CharacterId[] allCharacters = [
    ..mainCharacters,
    CharacterId.Flandre,
    CharacterId.Koakuma
];
```

---

## Architecture Layers

```
+-----------------------------------------------------------------+
|                    Content Layer (YAML)                          |
|  Game/content/                                                  |
|  |- kojo/           # Character dialogue                        |
|  |- characters/     # Character definitions                     |
|  |- commands/       # COM definitions                           |
|  +- config/         # Game configuration                        |
+-----------------------------------------------------------------+
                              |
                              | Load & Parse
                              v
+-----------------------------------------------------------------+
|                    Logic Layer (C#)                              |
|  src/Era.Core/                                                      |
|  |- GameEngine.cs        # Main game loop                       |
|  |- StateManager.cs      # Game state (save/load)               |
|  |- CommandProcessor.cs  # COM execution                        |
|  |- KojoEngine.cs        # Dialogue selection                   |
|  +- NtrEngine.cs         # NTR parameter calculations           |
+-----------------------------------------------------------------+
                              |
                              | Events & Commands
                              v
+-----------------------------------------------------------------+
|                    UI Layer (Unity)                              |
|  UnityUI/                                                       |
|  |- TextRenderer.cs      # Text display                         |
|  |- InputHandler.cs      # User input                           |
|  |- MenuSystem.cs        # Menu navigation                      |
|  +- SceneManager.cs      # Scene transitions                    |
+-----------------------------------------------------------------+
```

### Layer Responsibilities

| Layer | Responsibility | Technology | Testability |
|-------|----------------|------------|:-----------:|
| **Content** | Data definition (what) | YAML | Schema validation |
| **Logic** | Business rules (how) | Pure C# | Unit tests |
| **UI** | Presentation (show) | Unity | Integration tests |

### Dependency Rule

```
Content -> Logic -> UI
   |        |       |
 YAML    Pure C#  Unity

Logic layer has NO Unity dependencies.
Can run headless for testing.
```

---

## Content Layer (YAML)

### Directory Structure

```
Game/content/
|- kojo/
|   |- K1_Meiling/
|   |   |- Training/            # Game-loop based (F464)
|   |   |   |- Touch/
|   |   |   |   |- Caress.yaml
|   |   |   |   +- Kiss.yaml
|   |   |   |- Oral/
|   |   |   |   +- Cunnilingus.yaml
|   |   |   +- Equipment/
|   |   |       +- Vibrator.yaml
|   |   |- Daily/
|   |   |   +- Conversation.yaml
|   |   +- ...
|   |- K2_Patchouli/
|   +- ...
|- characters/
|   |- K1_Meiling.yaml
|   |- K2_Patchouli.yaml
|   +- ...
|- commands/
|   |- command_definitions.yaml  # Semantic command metadata
|   +- command_categories.yaml   # Game-loop based categories
+- config/
    |- game_settings.yaml
    |- ntr_parameters.yaml
    +- talent_definitions.yaml
```

### YAML Kojo Schema

See [full-csharp-architecture.md](../full-csharp-architecture.md) for the complete schema definitions.

### Schema Versioning Strategy

YAMLスキーマの後方互換性と破壊的変更の管理。

**バージョン形式**: `major.minor`
- **major**: 破壊的変更（既存ファイルが読めなくなる）
- **minor**: 後方互換の追加（新フィールド追加等）

**マイグレーション管理**:

| From | To | Breaking Change | Migrator |
|:----:|:--:|-----------------|----------|
| 1.0 | 1.1 | なし（`weight` フィールド追加） | 不要 |
| 1.1 | 2.0 | `when.talent` -> `when.talents[]` | `MigrateV1ToV2.cs` |
| 2.0 | 2.1 | なし（`priority` フィールド追加） | 不要 |

---

## Logic Layer (C#)

### Core Classes

For complete class definitions, see [full-csharp-architecture.md](../full-csharp-architecture.md).

Key classes:
- `GameEngine` - Main game loop orchestration
- `StateManager` - Game state persistence (save/load)
- `KojoEngine` - Dialogue selection with weighted random, variable expansion
- `NtrEngine` - NTR parameter calculations (temptation, resistance, progress)

### Testability Example

```csharp
// src/Era.Core.Tests/KojoEngineTests.cs
[TestClass]
public class KojoEngineTests
{
    [TestMethod]
    public void GetDialogue_WithLoverTalent_ReturnsLoverDialogue()
    {
        var mockRandom = new MockRandom(0);
        var engine = new KojoEngine(mockRandom);
        var context = new EvaluationContext
        {
            Talents = new[] { "恋人" },
            MasterName = "レミリア"
        };

        var result = engine.GetDialogue("K1_Meiling", "COM_0", context);

        Assert.IsTrue(result.Contains("レミリア様"));
    }
}
```

---

## UI Layer (Unity)

### UnityUI Interface

```csharp
// src/Era.Core/IGameUI.cs (in Logic layer, implemented by Unity)
public interface IGameUI
{
    void ShowMenu(MenuData menu);
    void ShowResult(CommandResult result);
    void ShowDialogue(string speaker, string text);
    Task<UserInput> GetInputAsync();
    void PlaySound(string soundId);
    void ShowPortrait(string characterId, string expression);
}
```

### Headless Mode (Testing)

```csharp
// src/Era.Core.Tests/HeadlessUI.cs
public class HeadlessUI : IGameUI
{
    public List<string> OutputLog { get; } = new();
    public Queue<UserInput> InputQueue { get; } = new();

    public void ShowDialogue(string speaker, string text)
    {
        OutputLog.Add($"[{speaker}] {text}");
    }

    public Task<UserInput> GetInputAsync()
    {
        return Task.FromResult(InputQueue.Dequeue());
    }

    // ... other members
}
```

---

## Concurrency Design Guidelines (Phase 20-22)

Phase 20-22 は並列実行可能だが、共有状態へのアクセス制御が必要。

**共有状態の分類**:

| 状態 | アクセスパターン | 制御方式 |
|------|------------------|----------|
| `IGameState` | 読み取り多、書き込み少 | `ReaderWriterLockSlim` |
| `CharacterState` | キャラ単位で独立 | キャラ別ロック（細粒度） |
| `IVariableStore` | 頻繁な読み書き | スレッドローカルコピー + マージ |
| 設定値（読み取り専用） | 読み取りのみ | ロック不要（イミュータブル） |

**Phase 20-22 の独立性保証**:

| Phase | 主要対象 | 他Phaseとの競合 |
|:-----:|----------|-----------------|
| 20 | Equipment, Shop | なし（独立サブシステム） |
| 21 | Counter, COMABLE | Phase 12 COM と読み取り共有 |
| 22 | State Systems | CharacterState 書き込み |

**競合回避ルール**:

1. **Phase 22 (State) は他と同時実行禁止** - CharacterState への排他書き込み
2. **Phase 20, 21 は完全並列可** - 独立サブシステム
3. **Phase 23 (NTR Kojo Analysis) は Phase 22 完了後** - 分析はState完了後に実施
4. **Phase 24 (NTR Context) は Phase 23 完了後** - DDD設計は分析結果に依存
