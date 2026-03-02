# Phase 28-34: Integration & Completion

**Parent**: [full-csharp-architecture.md](../full-csharp-architecture.md)

---

### Phase 28: Domain Events統合 (was Phase 27)

**Phase Status**: TODO

**Goal**: クロスシステムイベント駆動連携の完成

**Background**: Phase 13 (DDD Foundation) で IDomainEvent, AggregateRoot, IDomainEventPublisher, EventHandlerRegistry の基盤を確立済み。Phase 24 (NTR Bounded Context) でドメインイベントの具体的活用パターンを設計済み。本Phaseではこれらの基盤を全システムに統合し、クロスシステム連携をイベント駆動で実現する。

> **⚠️ Phase 13 との責務分担**
>
> - **Phase 13 (DDD Foundation)**: IDomainEvent, DomainEventBase, IDomainEventPublisher, DomainEventPublisher, EventHandlerRegistry, Character AggregateRoot 統合
> - **Phase 28 (本Phase)**: クロスシステムハンドラの実装・登録、イベントフロー検証

> **⚠️ Event-Driven Architecture原則（このPhaseの本質）**
>
> **イベント発行**:
> - 全Aggregateはドメインイベントを発行可能
> - イベントは不変（immutable）
> - 発行者は購読者を知らない
>
> **イベント購読**:
> - ハンドラは1イベント=1責務
> - 非同期処理可能
> - 失敗時のリトライ戦略定義
>
> **クロスシステム連携**:
> - NTR進行 -> 口上分岐更新
> - 訓練完了 -> 能力成長 -> UI更新
> - 状態変化 -> ログ記録

**Tasks** (基盤 Task 1-3 は Phase 13 に移動済み):
1. ~~IDomainEvent基盤インターフェース確立~~ -> **Phase 13 に移動**
2. ~~DomainEventPublisher実装~~ -> **Phase 13 に移動**
3. ~~EventHandlerRegistry実装~~ -> **Phase 13 に移動**
4. イベントディスパッチ機構（同期/非同期対応）
5. ~~Character Aggregateへのイベント発行統合~~ -> **Phase 13 に移動**
6. NtrProgression Aggregateへのイベント発行統合
7. KojoSystemイベントハンドラ（口上分岐更新）
8. UIイベントハンドラ（画面更新通知）
9. LoggingEventHandler（監査ログ）
10. Integration Tests（イベントフロー検証）
11. **Create Phase 28 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 28 セクションとの整合確認必須
12. **Create Phase 29 Planning feature** (type: research, include transition feature tasks)

**Core Event Infrastructure** (Phase 13 で実装。Phase 28 は参照のみ):

```csharp
// src/Era.Core/Events/IDomainEvent.cs (Phase 13 で実装)
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
    string EventType { get; }
}

// src/Era.Core/Events/DomainEventBase.cs
public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}

// src/Era.Core/Events/IDomainEventPublisher.cs
public interface IDomainEventPublisher
{
    void Publish<T>(T @event) where T : IDomainEvent;
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : IDomainEvent;
}

// src/Era.Core/Events/IDomainEventHandler.cs
public interface IDomainEventHandler<T> where T : IDomainEvent
{
    Task Handle(T @event, CancellationToken ct = default);
}
```

**Event Handler Registry**:

```csharp
// src/Era.Core/Events/EventHandlerRegistry.cs
public class EventHandlerRegistry
{
    private readonly IServiceProvider _services;
    private readonly Dictionary<Type, List<Type>> _handlers = new();

    public void Register<TEvent, THandler>()
        where TEvent : IDomainEvent
        where THandler : IDomainEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = new List<Type>();
        _handlers[eventType].Add(typeof(THandler));
    }

    public IEnumerable<IDomainEventHandler<T>> GetHandlers<T>() where T : IDomainEvent
    {
        if (!_handlers.TryGetValue(typeof(T), out var handlerTypes))
            yield break;

        foreach (var handlerType in handlerTypes)
            yield return (IDomainEventHandler<T>)_services.GetRequiredService(handlerType);
    }
}
```

**Cross-System Event Examples**:

```csharp
// NTR -> Kojo連携
public class NtrPhaseAdvancedHandler : IDomainEventHandler<NtrPhaseAdvanced>
{
    private readonly IKojoEngine _kojo;

    public async Task Handle(NtrPhaseAdvanced @event, CancellationToken ct)
    {
        // NTRフェーズ進行時に口上キャッシュを更新
        await _kojo.InvalidateCache(@event.CharacterId);
    }
}

// Training -> UI連携
public class TrainingCompletedHandler : IDomainEventHandler<TrainingCompleted>
{
    private readonly IUINotifier _ui;

    public async Task Handle(TrainingCompleted @event, CancellationToken ct)
    {
        await _ui.NotifyGrowth(@event.CharacterId, @event.Growth);
    }
}

// 監査ログ
public class AuditLoggingHandler<T> : IDomainEventHandler<T> where T : IDomainEvent
{
    private readonly ILogger<AuditLoggingHandler<T>> _logger;

    public Task Handle(T @event, CancellationToken ct)
    {
        _logger.LogInformation("Domain Event: {EventType} {EventId} at {OccurredOn}",
            @event.EventType, @event.EventId, @event.OccurredOn);
        return Task.CompletedTask;
    }
}
```

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/Events/IDomainEvent.cs` | イベント基盤インターフェース |
| `src/Era.Core/Events/DomainEventBase.cs` | イベント基底クラス |
| `src/Era.Core/Events/IDomainEventPublisher.cs` | 発行インターフェース |
| `src/Era.Core/Events/DomainEventPublisher.cs` | 発行実装 |
| `src/Era.Core/Events/EventHandlerRegistry.cs` | ハンドラ登録 |
| `src/Era.Core/Events/Handlers/*.cs` | クロスシステムハンドラ |
| `src/Era.Core/DependencyInjection/EventsExtensions.cs` | DI統合 |

**DI Registration**:

```csharp
// src/Era.Core/DependencyInjection/EventsExtensions.cs
public static IServiceCollection AddDomainEvents(this IServiceCollection services)
{
    services.AddSingleton<IDomainEventPublisher, DomainEventPublisher>();
    services.AddSingleton<EventHandlerRegistry>();

    // Handler registrations
    services.AddTransient<IDomainEventHandler<NtrPhaseAdvanced>, NtrPhaseAdvancedHandler>();
    services.AddTransient<IDomainEventHandler<TrainingCompleted>, TrainingCompletedHandler>();
    services.AddTransient(typeof(IDomainEventHandler<>), typeof(AuditLoggingHandler<>));

    return services;
}
```

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=DomainEvents"
# Event publishing verified
# Handler invocation verified
# Cross-system integration verified
# Async handling verified
```

**Success Criteria**:
- [ ] ドメインイベント基盤確立 (Phase 13 で完了済み前提)
- [ ] クロスシステム連携機能 (NTR->口上、訓練->UI、状態->ログ)
- [ ] 全テスト PASS

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 28: Domain Events」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |
| **前提確認** | Phase 13 で IDomainEvent/Publisher/Registry 実装済みであること | ビルド確認 |

**Next**: Create Phase 29 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 29: WPF UI (was Phase 28, revised from Unity UI)

**Phase Status**: TODO

**Goal**: WPF-based UI implementation（双方向インタラクション + グラフィック表示統合）

> **⚠️ プラットフォーム変更 (2026-03-02)**
>
> Unity UI -> WPF に変更。理由:
> - Era.Core (.NET 10 / C# 14) との直接プロジェクト参照が可能（DLL互換性問題ゼロ）
> - テキスト主体のeraゲームに WPF DirectWrite が最適
> - 配布先は Windows 直接配布のみ（成人向けコンテンツでストア審査不可）
> - Unity の .NET Standard 2.1 制約と CoreCLR 対応待ちのリスクを排除

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **UI層はEra.Coreに依存、逆は禁止**:
> ```
> Era.Core (Logic Layer) <- WPF (Presentation Layer)
>     |                         |
> Pure C#, no WPF        WPF-specific code
> ```
>
> **DIコンテナ**: WPF 側で `AddEraCore()` を呼び出し、WPF 実装を登録

> **⚠️ AA マップ等価性制約（必須）**
>
> ゲーム内マップは全角文字によるアスキーアート（AA）で描画される。テキストレイアウトのピクセル等価性が必須。
>
> **制約1: ピクセル吸着モード**
> ```xml
> <!-- 全テキスト表示領域に適用必須 -->
> <RichTextBox TextOptions.TextFormattingMode="Display"
>              FontFamily="MS Gothic" FontSize="18" />
> ```
> `TextFormattingMode="Display"` は DirectWrite のサブピクセル配置を無効化し、GDI 互換のピクセルグリッド吸着を有効にする。
>
> **制約2: エンジン互換レイアウト計算**
> ```csharp
> // uEmuera の Utils.GetDisplayLength を Era.Core に移植
> // WPF の DirectWrite 文字計測には依存しない
> public static int GetDisplayLength(string s, float fontSize)
> {
>     float x = 0;
>     foreach (char c in s)
>         x += IsHalfWidth(c) ? fontSize / 2 : fontSize;
>     return (int)x;
> }
> // IsHalfWidth: c < 0x127 → half, else → full（エンジン同等のバイナリ判定）
> ```
>
> **制約3: 精密描画フォールバック**
>
> FlowDocument の自動レイアウトで AA がずれる場合、マップ等の精密描画部分のみ `DrawingVisual` + `FormattedText` でピクセル座標指定描画に切り替える。
>
> **検証方法**: Headless 出力と WPF 出力の文字位置を MAP_PRINT で比較。1px 以上のずれがあれば FAIL。
>
> **背景**: 元 Emuera は WinForms + GDI（ピクセルグリッド描画）。uEmuera で GDI が使えなくなり `GetDisplayLength` のバイナリ判定に簡略化。WPF はこのバイナリ判定を移植し、`TextFormattingMode="Display"` でピクセル吸着することで同等の描画を実現する。

---

#### UI Interaction Model（重要: 表示だけではない）

WPF UI は「流れてきた API を表示するだけ」ではない。**双方向通信 + 状態管理**が必要。

**通信フロー**:
```
Era.Core ──Print/Button/Bar/Wait──→ WPF 表示
Era.Core ←─Click/Enter/Input──────── WPF 入力
Era.Core ←→ 状態同期（WaitInput? InputType? ButtonGeneration?）
```

##### 1. Era.Core 接続インターフェース（既存・片方向）

Era.Core の UI 境界インターフェースはプラットフォーム非依存。WPF は同じインターフェースを実装する。

| Era.Core Interface | WPF 実装 | 責務 |
|-------------------|----------|------|
| `IConsoleOutput` | `WpfConsoleOutput` | Print*, DrawLine, Bar, Wait -> FlowDocument |
| `IInputHandler` | (Era.Core 提供の `InputHandler` をそのまま使用) | `ProvideInput()` を WPF から呼ぶ |
| `IStyleManager` | `WpfStyleManager` | SetColor/Font/Alignment -> WPF Brush/FontFamily |
| `IGameState` | `WpfGameState` | Save/Load Dialog -> WPF OpenFileDialog |
| `ITextFormatting` | `WpfTextFormatting` | 服装/体型テキスト生成 |
| `IEngineVariables` | `WpfEngineVariables` | ゲーム変数ブリッジ |

##### 2. WPF 側が追加で担う責務（既存インターフェースでカバーされない）

現エンジン (`EmueraConsole`, `EmueraThread`, `EmueraContent`) にハードコードされている UI 挙動を WPF 側で再実装する必要がある。

**ボタンインタラクション**:

| 挙動 | 詳細 | 現エンジン実装 |
|------|------|---------------|
| **ボタンクリック→入力** | PRINTBUTTON クリック時に値をゲームスレッドに返す | `EmueraThread.Input(code, true)` → 共有変数 |
| **ボタン世代管理** | 前の INPUT の古いボタンを無効化し、現在の INPUT のボタンのみクリック可能にする | `button_generation` カウンタ。`ConsoleButtonString.Generation >= lastButtonGeneration` でフィルタ |
| **QuickButtons パネル** | 現在有効なボタンを一覧表示するパネル | `EmueraContent.SetLastButtonGeneration()` で抽出 |

**入力制御**:

| 挙動 | 詳細 | 現エンジン実装 |
|------|------|---------------|
| **入力ボックス表示切替** | 数値/文字列入力時 → 表示、WAIT/EnterKey時 → 非表示 | `InputType` に応じて `Inputpad` を show/hide |
| **@コマンド** | `@REBOOT`, `@CONFIG`, `@QUIT` 等のシステムコマンド | 入力文字列先頭 `@` を検出 → `doSystemCommand()` |
| **ONEINPUT** | 単一キー押下で即座に入力確定 | `InputType.OneInput` で1文字バリデーション |

**メッセージ送り・スキップ**:

| 挙動 | 詳細 | 現エンジン実装 |
|------|------|---------------|
| **背景クリック / Enter** | WAIT/EnterKey 待ちを進める | `PressEnterKey(false, "", false)` |
| **ダブルクリックスキップ** | 200ms以内の連打で `MesSkip` フラグ ON → WAIT を自動通過 | `(nowtick - last_click_tic < 200)` で検出 |
| **SKIPDISP** | ゲームスクリプトが表示を抑制 | `SetSkipPrint(true)` → Print 出力を破棄 |

**タイムアウト入力 (TINPUT)**:

| 挙動 | 詳細 | 現エンジン実装 |
|------|------|---------------|
| **カウントダウン表示** | 「残り X.X」秒を100ms間隔で更新 | 10ms Timer + 一時行（Temporary Line）の上書き |
| **一時行** | 最終表示行を上書き可能にする概念 | カウントダウン表示・デフォルト値表示で使用 |
| **タイムアウト時** | 空入力をゲームスレッドに送信 | `endTimer()` → `callEmueraProgram("")` |

**スクロール**:

| 挙動 | 詳細 | 現エンジン実装 |
|------|------|---------------|
| **自動スクロール** | 新しい行の追加時にビューポートを最下部に移動 | `drag_delta.y += LineHeight * 1.5f` |
| **手動スクロールバック** | ユーザーが上にスクロールして履歴を閲覧 | ドラッグ + 慣性スクロール |
| **仮想レンダリング** | ビューポート内の行のみ描画（MaxLog リングバッファ） | viewport culling + オブジェクトプール |
| **スクロール位置によるボタン制御** | スクロールバック中はボタン無効化の判定に影響 | `ScrollBar.Value != Maximum` で判定 |

**スレッド間通信**:

| 挙動 | 詳細 | 現エンジン実装 |
|------|------|---------------|
| **ゲームスレッド** | バックグラウンドで ERB/C# ロジックを実行 | `ThreadPool.QueueUserWorkItem` |
| **UI→ゲーム** | 入力値をスレッドセーフに渡す | 共有 `string input` フィールド + 1ms ポーリング |
| **ゲーム→UI** | 表示更新を UI スレッドに通知 | 直接 UI オブジェクト操作（Unity は thread-unsafe を許容） |

> **⚠️ WPF スレッドモデル**
>
> WPF は `Dispatcher` による厳密な UI スレッドアフィニティを持つ。現エンジンの「共有変数 + ポーリング」は WPF でも使えるが、ゲームスレッド→UI更新は必ず `Dispatcher.InvokeAsync()` 経由にする必要がある。
>
> **推奨**: 入力は `SemaphoreSlim` + `ConcurrentQueue<string>` でポーリングを排除。表示は `IProgress<DisplayUpdate>` パターンで Dispatcher マーシャリング。

**その他**:

| 挙動 | 詳細 |
|------|------|
| **ウィンドウタイトル** | `SetWindowTitle(str)` — ゲーム名・状態を反映 |
| **処理中インジケータ** | ゲーム処理が1.5秒以上続いた場合にスピナー表示 |
| **オプション画面** | 解像度・言語・タイトルへ戻る・再起動・ログ保存・終了 |
| **CBG ボタンマップ** | ピクセル色ベースのクリッカブル領域（稀、グラフィック表示と連動） |

---

**Tasks**:
1. Create WPF project (`Era.WPF`) with .NET 10 TargetFramework
2. Implement game thread infrastructure（`SemaphoreSlim` ベースのスレッド間通信、`Dispatcher` マーシャリング）
3. Implement `WpfConsoleOutput`（FlowDocument ベーステキスト描画 + ボタン世代管理）
4. Implement virtual scroll system（仮想レンダリング + 自動スクロール + 手動スクロールバック）
5. Implement button interaction system（クリック→入力、世代フィルタ、QuickButtons パネル）
6. Implement input system（TextBox + 表示切替 + @コマンド + ONEINPUT）
7. Implement TINPUT countdown（DispatcherTimer + 一時行 + タイムアウト処理）
8. Implement message skip（ダブルクリック検出 + MesSkip フラグ + SKIPDISP）
9. Implement `WpfStyleManager`（color, font, alignment）
10. Implement `WpfGameState`（save/load dialogs, quit, restart, option screen）
11. Implement menu system（MVVM pattern）
12. **グラフィック表示統合**（画像・立ち絵の WPF Image 表示 + CBG ボタンマップ）
13. **Create Phase 29 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 29 セクションとの整合確認必須
14. **Create Phase 30 Planning feature** (type: research, include transition feature tasks)
15. **(Optional) GUI DisplayMode Visualization** - デバッグ用にdisplayMode（newline/wait/key-wait）を視覚化するトグル機能。F678で実装したDisplayModeCaptureをGUI側に拡張。

**WPF Architecture**:

```
Era.WPF/
├── App.xaml                    # Application entry + DI container setup
├── MainWindow.xaml             # Main game window
├── ViewModels/
│   ├── MainViewModel.cs        # MVVM root
│   ├── ConsoleViewModel.cs     # Text display + scroll state
│   ├── InputViewModel.cs       # Input state + box visibility
│   └── ButtonPanelViewModel.cs # QuickButtons panel
├── Views/
│   ├── ConsoleView.xaml        # FlowDocument-based text area (virtual scroll)
│   ├── InputView.xaml          # Command input area (show/hide by InputType)
│   ├── ButtonPanelView.xaml    # QuickButtons panel
│   └── MenuView.xaml           # Option/system menu
├── Services/
│   ├── WpfConsoleOutput.cs     # IConsoleOutput -> FlowDocument + button generation
│   ├── WpfStyleManager.cs      # IStyleManager -> WPF resources
│   ├── WpfGameState.cs         # IGameState -> WPF dialogs
│   ├── WpfTextFormatting.cs    # ITextFormatting
│   └── GameThreadHost.cs       # Game thread lifecycle + SemaphoreSlim input handoff
├── Interaction/
│   ├── ButtonGenerationTracker.cs  # Button generation counter + stale button filter
│   ├── MessageSkipController.cs    # Double-click detection + MesSkip flag
│   ├── TimedInputController.cs     # TINPUT countdown + temporary line
│   └── SystemCommandParser.cs      # @REBOOT, @CONFIG, @QUIT dispatch
├── Graphics/
│   ├── GraphicsManager.cs      # 画像表示管理 (WPF Image)
│   ├── PortraitDisplay.cs      # 立ち絵表示 (WPF Canvas + Image)
│   └── CbgButtonMap.cs         # CBG pixel-color clickable regions
└── Converters/                 # MVVM value converters
```

**Graphics Display Migration** (from Phase 27 Extensions):

| ERB File | WPF Component | Purpose |
|----------|--------------|---------|
| `グラフィック表示.ERB` | `GraphicsManager.cs` | 画像表示管理 (WPF Image) |
| `グラフィック表示.ERH` | (設定) | 設定定義 |
| `立ち絵表示.ERB` | `PortraitDisplay.cs` | 立ち絵表示 (WPF Canvas + Image) |
| `立ち絵表示.ERH` | (設定) | 立ち絵設定 |

**DI Registration (WPF Host)**:

```csharp
// App.xaml.cs
var services = new ServiceCollection();

// Era.Core の全サービスを登録
services.AddEraCore();

// WPF 実装で Era.Core のスタブを上書き
services.AddSingleton<IConsoleOutput, WpfConsoleOutput>();
services.AddSingleton<IStyleManager, WpfStyleManager>();
services.AddSingleton<IGameState, WpfGameState>();
services.AddSingleton<ITextFormatting, WpfTextFormatting>();
services.AddSingleton<IEngineVariables, WpfEngineVariables>();

// WPF 固有サービス（Era.Core に存在しない UI 挙動）
services.AddSingleton<GameThreadHost>();
services.AddSingleton<ButtonGenerationTracker>();
services.AddSingleton<MessageSkipController>();
services.AddSingleton<TimedInputController>();
services.AddSingleton<SystemCommandParser>();
```

**Key Design Decisions**:

| 項目 | 決定 | 理由 |
|------|------|------|
| テキスト描画 | FlowDocument + RichTextBox + `TextFormattingMode="Display"` | DirectWrite ピクセル吸着モードで AA マップ等価性を保証 |
| AA レイアウト | `GetDisplayLength` 移植 + `DrawingVisual` 併用 | エンジン同等のバイナリ半角/全角判定でピクセル座標計算。精密描画が必要な箇所は `FormattedText` + 座標指定 |
| スレッド間通信 | `SemaphoreSlim` + `IProgress<T>` | ポーリング排除、WPF Dispatcher 準拠 |
| ボタン管理 | `ButtonGenerationTracker` | 世代カウンタで古いボタンを無効化 |
| スクロール | VirtualizingStackPanel or custom | 大量行で OOM 防止 |
| MVVM | CommunityToolkit.Mvvm | .NET 公式、軽量 |
| DI | Microsoft.Extensions.DependencyInjection | Era.Core と同じ |
| 画像 | WPF Image + BitmapImage | 立ち絵・CBG に十分 |
| レイアウト | Grid + DockPanel | テキストゲーム向きの固定レイアウト |

**Verification**:
```bash
# WPF build test
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && /home/siihe/.dotnet/dotnet build src/Era.WPF/'

# Era.Core integration test
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/devkit && /home/siihe/.dotnet/dotnet test src/Era.WPF.Tests/ --blame-hang-timeout 10s'
```

**Success Criteria**:
- [ ] WPF プロジェクト作成 + ビルド成功
- [ ] Era.Core プロジェクト参照 + DI 統合
- [ ] テキスト表示（FlowDocument + 仮想スクロール）
- [ ] ボタンクリック→入力（世代管理付き）
- [ ] 入力ボックス表示切替 + @コマンド
- [ ] TINPUT カウントダウン + タイムアウト
- [ ] ダブルクリックスキップ + SKIPDISP
- [ ] グラフィック表示統合
- [ ] AA マップ等価性（MAP_PRINT の Headless 出力と WPF 出力でレイアウト一致）
- [ ] Headless モードとの共存（Era.Core のインターフェースが同一であること）

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 29: WPF UI」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |
| **AC: インタラクション** | ボタン・入力・スキップ・TINPUT の動作検証 AC を含む | AC 一覧確認 |
| **AC: AA 等価性** | MAP_PRINT レイアウト等価性テストを含む | Headless vs WPF 出力比較 |

**Next**: Create Phase 30 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 30: Integration (was Phase 29)

**Phase Status**: TODO

**Goal**: Full game running on new stack

> **⚠️ Phase 4 Design Requirements（最終確認）**
>
> **全システム統合時のDI確認**:
> - すべてのインターフェースがDIコンテナに登録されていること
> - 循環依存がないこと
> - Strongly Typed IDsが全モジュールで一貫していること
>
> **統合テスト**:
> - Mock注入による単体テスト可能性を確認
> - E2Eテストでの全システム連携確認

**Tasks**:
1. Wire up all systems (Phase 3-28)
2. Implement game flow (title -> game -> save/load)
3. SHOP system integration (Phase 20)
4. TRAIN system integration (Phase 6, 12)
5. **Miscellaneous systems integration**
6. **E2E Test Planning & Implementation** (see E2E Test Strategy below)
7. Final ERB dependency check
8. Archive ERB files to `Game/ERB/_archived/`
9. **Create Phase 30 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 30 セクションとの整合確認必須
10. **Create Phase 31 Planning feature** (type: research, include transition feature tasks)
11. **BenchmarkDotNet パフォーマンスベースライン**: Full Integration 後に COM評価、YAML解析、コマンドディスパッチ等のホットパスを計測し初回ベースラインを取得。以降のリグレッション検出に使用。参考: `<PackageReference Include="BenchmarkDotNet" Version="0.14.*" />`

**E2E Test Strategy (Task 6)**:

> **⚠️ User Discussion Required**
>
> Before implementation, discuss with user:
> - Test scenario granularity (per-feature? per-flow? per-system?)
> - Coverage targets (critical paths only? comprehensive?)
> - Direction (Golden Master? Assertion-based? Hybrid?)
> - Technical debt policy (zero debt? acceptable debt with tracking?)

**E2E Test Planning Subtasks**:
| Subtask | Description | Output |
|---------|-------------|--------|
| 6.1 | User discussion: scope, granularity, coverage, direction | Decision document |
| 6.2 | Define E2E test scenarios based on user decisions | `test/e2e/scenarios.md` |
| 6.3 | Define test data fixtures (seeded state, characters, items) | `test/e2e/fixtures/` |
| 6.4 | Define Golden Master baselines (if applicable) | `test/e2e/golden/` |
| 6.5 | Implement E2E test framework (headless mode integration) | `src/Era.Core.Tests/E2E/` |
| 6.6 | Implement E2E test cases per scenario list | Test files |
| 6.7 | Execute E2E tests, verify PASS | CI integration |
| 6.8 | Fix failures, iterate until all PASS | Iteration loop |

**E2E Test Scope Candidates** (to be finalized in 6.1):
| Category | Example Scenarios | Priority |
|----------|-------------------|:--------:|
| Game Flow | Title -> New Game -> First Day -> Save -> Load | Critical |
| Training | COM execution -> State change -> Kojo display | Critical |
| NTR | NTR trigger -> Mark progression -> Ending branch | High |
| Shop | Purchase -> Inventory -> Equipment | Medium |
| Character | Stat growth -> Ability unlock -> Relationship | Medium |

**Technical Debt Policy Options** (to be decided in 6.1):
| Option | Description | Implication |
|--------|-------------|-------------|
| A | Zero debt - all E2E tests PASS before Phase 30 complete | Strict, may delay |
| B | Critical paths only - non-critical can be tracked | Pragmatic |
| C | Coverage threshold (e.g., 80%) with debt tracking | Measurable |

**Implementation Loop (Task 6.6-6.8)**:
```
WHILE (E2E tests have failures) DO
  1. Identify failing test
  2. Debug root cause (test issue? implementation issue?)
  3. Fix (test or implementation)
  4. Re-run E2E suite
  5. IF new failures introduced THEN continue loop
DONE
```

**Exit Criteria for Task 6**:
- [ ] User decisions documented (6.1)
- [ ] All planned E2E scenarios implemented (6.6)
- [ ] E2E test pass rate meets agreed threshold (6.7)
- [ ] Technical debt (if any) tracked per CLAUDE.md protocol (6.8)

**ERB Archive Structure** (from Phase 7 Review):

> **⚠️ Phase 7 引き継ぎ事項**: 292 root-level ERB files の整理アーカイブ

**Pre-Archive Checklist**:
- [ ] 292 root-level ERB files confirmed fully migrated to C#
- [ ] 口上/ directory content migrated to YAML (Phase 19)
- [ ] NTR/, 会話拡張/, 外出拡張/, 妖精メイド拡張/ migrated
- [ ] All ERB->C# equivalence tests PASS

**Archive Directory Structure**:
```
Game/ERB/_archived/
├── System/           # SYSTEM.ERB, OPTION.ERB, OPTION_2.ERB, LOCK.ERB, VERSION_UP.ERB
├── Training/         # BEFORETRAIN.ERB, AFTERTRA.ERB, TRACHECK*.ERB, TRAIN*.ERB
├── Content/          # COMF*.ERB (150+ files), SCOMF*.ERB (16 files)
├── Character/        # ABL*.ERB, ABLUP.ERB, SOURCE*.ERB, 名称*.ERB
├── Clothes/          # CLOTHES*.ERB, BODY*.ERB
├── Shop/             # SHOP*.ERB
├── Location/         # RESTROOM.ERB, MAP*.ERB, MOVEMENT.ERB
├── UI/               # PRINT*.ERB, SEL_ARRAY.ERB
├── Extensions/       # 会話拡張/, 外出拡張/, 妖精メイド拡張/, グラフィック表示/
├── NTR/              # NTR/*.ERB (14 files)
├── Kojo/             # 口上/**/*.ERB (already YAML-converted in Phase 19)
└── Legacy/           # その他未分類
```

**Current File Distribution** (verified):
| Location | Count | Status |
|----------|:-----:|--------|
| Root (`Game/ERB/*.ERB`) | 292 | To be archived |
| `口上/` | 117+ | Phase 19: YAML conversion |
| `NTR/` | 14 | Phase 24-25: C# migration |
| Other subdirs | 46+ | Various phases |
| **Total** | ~469 | |

**Verification**:
```bash
# Build succeeds without ERB/
dotnet build Era.Core

# All tests pass with C#-only stack
dotnet test Era.Core.Tests

# No runtime ERB loader dependencies
grep -r "\.ERB" src/Era.Core/ --include="*.cs" | wc -l  # Should be 0
```

**Core Systems Integration**:

| System | Source Phase | Target |
|--------|:------------:|--------|
| System Infrastructure | Phase 3 | Era.Core.System |
| Variables | Phase 5 | Era.Core.Variables |
| Ability/Training | Phase 6 | Era.Core.Ability |
| Expressions | Phase 8 | Era.Core.Expressions |
| Commands | Phase 9 | Era.Core.Commands |
| COM Implementation | Phase 12 | Era.Core.Commands.Com |
| DDD Foundation | Phase 13 | Era.Core.Domain |
| Game Engine | Phase 14 | Era.Core.GameEngine |
| KojoEngine SRP | Phase 18 | Era.Core.Dialogue |
| Equipment/Shop | Phase 20 | Era.Core.Shop |
| Counter | Phase 21 | Era.Core.Counter |
| State | Phase 22 | Era.Core.State |
| NTR Bounded Context | Phase 24 | Era.Core.NTR |
| AI/Visitor | Phase 25 | Era.Core.AI |
| Modes/Messaging | Phase 26 | Era.Core.Modes |
| Extensions | Phase 27 | Era.Core.Extensions |
| Domain Events | Phase 28 | Era.Core.Events |

**Miscellaneous Systems** (integrated during Phase 30):

| File | Target | Notes |
|------|--------|-------|
| MOVEMENT.ERB | Era.Core.Movement | 移動システム |
| MAP_PRINT.ERB | Era.Core.UI.MapDisplay | マップ表示 |
| RESTROOM.ERB | Era.Core.Location | トイレ/休憩所 |
| PRINT_STATE.ERB | Era.Core.UI.StateDisplay | 状態表示 |
| SEL_ARRAY.ERB | Era.Core.UI.ArraySelector | 配列選択UI |
| COMORDER.ERB | Era.Core.Commands.Com | コマンド順序 |
| OPTION.ERB, OPTION_2.ERB | Era.Core.Config | オプション画面 |
| LOCK.ERB | Era.Core.System | ロック機構 |
| VERSION_UP.ERB | Era.Core.Migration | バージョンアップ処理 |
| USERCOM.ERB | Era.Core.Commands | ユーザーカスタムコマンド |
| ~~アイテム管理.ERB~~ | ~~Era.Core.Items~~ | **ファイル不在 -- 削除** |
| ~~設定.ERB~~ | ~~Era.Core.Config~~ | **ファイル不在 -- 削除** |

**Verification**:
```bash
# Full game loop test
dotnet run --project Era.Game -- --demo
# Expected: Title -> NewGame -> Train -> Save -> Load -> Exit
```

**Success Criteria**:
- [ ] 全システム統合完了
- [ ] ゲームフロー動作確認
- [ ] ERB ファイルアーカイブ

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 30: Integration」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **Tasks: 等価性検証** | legacy 実装との等価性テストを含む | AC にテスト存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 31 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 31: Directory Structure Refactoring (was Phase 30)

**Phase Status**: TODO

**Goal**: ゼロベースでディレクトリ構造を根本的に見直し、わかりやすく要素単位で明確に分離

> **⚠️ 重要原則**
>
> **ゼロベース設計**: 現在の構造は参考情報のみ。歴史的経緯を無視して最適な構造を設計する。
> **参照リンク完全保持**: 全ての内部参照（ドキュメント間リンク、import文、設定ファイルパス）を移行後も完全に機能させる。

**Prerequisites**:
- Repository 統合完了（engine submodule 吸収、F424 Pre-Phase Infrastructure）
- Phase 30 (Integration) 完了

**Current Structure Issues** (参考情報):

| 問題点 | 現状 | 理想 |
|--------|------|------|
| 開発ドキュメントの配置 | `pm/` | ゲームコンテンツと分離すべき |
| テストプロジェクト散在 | `src/Era.Core.Tests/`, `src/engine.Tests/` がルート | 統一的な配置 |
| tools の位置づけ | `tools/` がルート | 役割に応じた整理 |
| 設定ファイル | `.claude/`, 各種 `*.md` | 明確なカテゴリ分け |

**Design Principles**:

| 原則 | 説明 |
|------|------|
| **Single Responsibility** | 各ディレクトリは1つの責務のみ |
| **Discoverability** | 初見でも構造が理解できる命名 |
| **Consistency** | 同種の要素は同じ階層に配置 |
| **Minimal Nesting** | 不要な深い階層を避ける |
| **Reference Integrity** | 全てのパス参照を追跡・更新 |

**Tasks**:
1. 現在の全ディレクトリ・ファイル構造の棚卸し
2. 新ディレクトリ構造の設計（ゼロベース）
3. 移行計画の策定（参照リンク一覧作成）
4. ディレクトリ移行実行
5. 全参照リンクの更新（ドキュメント、import、設定ファイル）
6. 参照整合性検証（/audit + grep による完全チェック）
7. Feature Dashboard の engine/ git 監視を除去（`server.js` health endpoint の `countDirty(path.join(PROJECT_ROOT, 'engine'))` を削除）
8. **Create Phase 31 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 31 セクションとの整合確認必須
9. **Create Phase 32 Planning feature** (type: research, include transition feature tasks)

**Reference Link Categories** (移行時に更新必須):

| カテゴリ | 例 | 検証方法 |
|----------|---|----------|
| Markdown リンク | `[feature-xxx](../feature-xxx.md)` | Grep + /audit |
| C# using/namespace | `using Era.Core.Functions;` | dotnet build |
| .csproj 参照 | `<ProjectReference Include="..\Era.Core\">` | dotnet build |
| CLAUDE.md パス | `pm/`, `engine/` | 手動確認 |
| Skills/Commands パス | `.claude/skills/`, `.claude/commands/` | Skill 読み込みテスト |
| Test 設定 | `test/`, フィルタパス | dotnet test |
| Git hooks | `.githooks/` 内のパス参照 | pre-commit 実行 |

**Verification**:
```bash
# 1. ビルド成功（C# 参照整合性）
dotnet build

# 2. テスト成功（テストパス整合性）
dotnet test

# 3. ドキュメントリンク検証
/audit

# 4. 壊れたリンク検出
grep -r "](.*\.md)" --include="*.md" | grep -v "http" | # 各リンク存在確認

# 5. pre-commit hook 動作確認
git commit --allow-empty -m "test" --dry-run
```

**Success Criteria**:
- [ ] 新ディレクトリ構造設計書作成
- [ ] 全ファイル移行完了
- [ ] dotnet build 成功
- [ ] dotnet test 全 PASS
- [ ] /audit 整合性検証 PASS
- [ ] 壊れた参照リンク = 0

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 31: Directory Structure」を継承 | Grep |
| **Tasks: 参照更新** | 全参照リンク更新タスクを含む | タスク一覧確認 |
| **AC: 参照整合性** | 壊れた参照 = 0 を検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 32 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 32: Documentation (was Phase 31)

**Phase Status**: TODO

**Goal**: ドキュメント/ワークフローをC#/YAML前提に更新

> **⚠️ Phase 4 Design Guidelines 文書化**
>
> **Type Design Guidelinesを開発者向けに整備**:
> - `docs/architecture/type-design.md` - 型設計ガイドライン
> - `docs/architecture/di-patterns.md` - DI パターン集
> - `docs/development/adding-new-feature.md` - 新機能追加手順（Phase 4パターン準拠）

**⚠️ Infrastructure Prerequisites**:
- `docs/` directory does not exist yet - needs to be created from scratch
- `src/Era.Core/Resources/` does not exist yet - Japanese localization infrastructure (Task 13) requires building resource management from scratch

**Tasks**:
1. Skills更新
2. CLAUDE.md更新
3. Agent定義更新
4. Commands更新
5. /audit で整合性検証
6. **testing SKILL Quality 階層化** (F264 残課題) - Required/NG 構造の適用判断
7. **品質統合レポート** (F300 残課題) - 構造的品質 + 意味的品質の統合
8. **SKILL vs 既存パターン矛盾解決** (F310 残課題) - 矛盾発生時の解決手順
9. **Create Phase 32 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 32 セクションとの整合確認必須
10. **Create Phase 33 Planning feature** (type: research, include transition feature tasks)
11. **日本語コントリビューターガイド整備** (docs/ja/)
    - `contributing.md` - 貢献方法概要（パチュロダ経由のファイル送付想定）
    - `yaml-structure.md` - YAMLファイル分割ルール（いつ新規ファイル vs 既存追加）
    - `com-guide.md` - COM追加ガイド（ディレクトリ構造、命名規則、配置場所）
    - `kojo-guide.md` - 口上追加ガイド（キャラ別/COM別配置、TALENT分岐）
    - `tools-guide.md` - ツール利用ガイド（F611 Linter、kojo-mapper）
    - `validation-guide.md` - 検証手順（Linter実行、よくあるエラーと対処）
12. **開発者向けガイド整備** (docs/ja/dev/)
    - `csharp-extension.md` - C#拡張ガイド（Era.Core拡張、新COM実装）
    - `integration-checklist.md` - 統合チェックリスト（パチュロダ投稿->diff確認->マージ）
    - `review-guide.md` - レビュー観点（スキーマ準拠、キャラ設定整合性）
13. **エラーメッセージ日本語化** - F598基盤上での実メッセージ追加（src/Era.Core/Resources/）
14. **IDE設定ガイド日本語版** - F599の日本語版（docs/ja/ide-setup.md）

**Affected Documents**:

| Category | Files | Action |
|----------|-------|--------|
| **Skills** | `erb-syntax.md` | Archive (legacy reference) |
| | `kojo-writing.md` | Rewrite for YAML |
| | `testing.md` | Update to dotnet test |
| | `engine-dev.md` | Update for WPF |
| **Agents** | `implementer.md` | ERB->C#/YAML |
| | `kojo-writer.md` | ERB->YAML |
| | `ac-tester.md` | Update matchers |
| **Commands** | `/do`, `/commit` etc. | Update test procedures |
| **Root** | `CLAUDE.md` | Full revision (incl. Project Structure for Game/content/) |

**Verification**:
```bash
# Run audit to check consistency
/audit
# Expected: No broken references, all paths valid
```

**Success Criteria**:
- [ ] ドキュメント更新完了
- [ ] /audit 整合性検証 PASS

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 32: Documentation」を継承 | Grep |
| **Tasks: 負債解消** | 古い参照の削除を含む | /audit 検証 |
| **AC: 負債ゼロ** | ドキュメント負債ゼロを検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 33 planning feature when this phase completes [OPTIONAL]

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 33: Validation (was Phase 32)

**Phase Status**: TODO

**Goal**: Confirm behavioral equivalence

> **⚠️ Phase 4 最終検証**
>
> **アーキテクチャ検証項目**:
> - [ ] 全インターフェースがMock可能（テスタビリティ）
> - [ ] 循環依存なし（DIコンテナ起動成功）
> - [ ] Strongly Typed IDsで型安全性確保
> - [ ] Result型で明示的エラーハンドリング
> - [ ] 技術的負債 = 0（意図的受け入れは文書化済み）

**Tasks**:
1. Run parallel: old (uEmuera) vs new (WPF)
2. Compare outputs for same inputs
3. Fix any discrepancies
4. Final regression test suite (20+ scenarios)
5. Kojo equivalence verification (all 117 files)
6. **⚠️ BaseIndex.Satisfaction 既存バグ修正** (F399 残課題) - Base.csv: 満足=13, 怒り=12 の誤り検証・修正
   > **WARNING**: `BaseIndex.Satisfaction = new(12)` が怒りインデックス12と衝突。Phase 33まで先送りは危険 -- 後続Phaseで満足度を参照するコードが誤った値を読む可能性がある。**Phase 20 開始前に単独 Feature として早期修正を推奨**。
7. **Create Phase 33 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 33 セクションとの整合確認必須
8. **Create Phase 34 Planning feature** (type: research, include transition feature tasks) [OPTIONAL]

**Validation Matrix**:

| System | Test Method | Coverage |
|--------|-------------|:--------:|
| Variables | Unit tests | 100% |
| Expressions | Unit tests | 100% |
| Commands | Integration tests | 100% |
| Kojo | KojoComparer | 117 files |
| Counter | Flow tests | 24 scenarios |
| State | Integration tests | 100% |
| Full Game | Parallel run | 100% |

**Verification**:
```bash
# Parallel comparison test
dotnet run --project tools/ParallelRunner -- --compare
# Expected: 100% match on all test scenarios
```

> **⚠️ Note**: `tools/ParallelRunner/` does not exist yet and needs to be built from scratch as part of this phase.

**Success Criteria**:
- [ ] 全テスト 100% PASS
- [ ] Kojo 117 ファイル等価確認
- [ ] 技術負債 = 0

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 33: Validation」を継承 | Grep |
| **Tasks: 等価性検証** | 全システム等価性テストを含む | ParallelRunner 100% MATCH |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 34 planning feature when this phase completes (OPTIONAL - only if save migration needed)

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。 (OPTIONAL for final phase)

### Phase 34: Save Migration (OPTIONAL) (was Phase 33)

**Phase Status**: TODO

**Goal**: Legacy uEmuera saves -> new format

**Execution Condition**: This phase requires explicit user request AND confirmation that legacy save compatibility is truly needed.

> **⚠️ Phase 4 Design Requirements（適用する場合）**
>
> **セーブデータ読み込みインターフェース**:
> ```csharp
> public interface ILegacySaveReader
> {
>     Result<GameState> ReadLegacySave(string path);
> }
>
> public interface ISaveConverter
> {
>     Result<Unit> Convert(string sourcePath, string destinationPath);
> }
> ```

| Condition | Action |
|-----------|--------|
| New deployment, no existing users | **SKIP** this phase |
| Existing users with save data | Execute after explicit confirmation |

**Background**: 既存ユーザーのセーブデータ保護が目的。新規デプロイや既存ユーザーがいない場合は不要。

**Tasks** (if executed):
1. Analyze old save format (SAV files)
2. Build save converter tool
3. Test with sample saves
4. Document migration guide for users
5. Integrate converter into first-run experience
6. **Create Phase 34 Post-Phase Review feature** (type: infra) [FINAL] - 本ドキュメント Phase 34 セクションとの整合確認必須

**Source Analysis**:

| Component | Lines | Purpose |
|-----------|:-----:|---------|
| `EraBinaryDataReader.cs` | 733 | 旧形式読込 |
| `EraBinaryDataWriter.cs` | 423 | 旧形式書込 |

**SAV Format Details**:
- Custom binary format (format version 1808)
- Zero-run compression for space optimization
- Multi-dimensional array serialization (1D/2D/3D)
- Int64 and string arrays support

**Deliverables**:

| Tool | Purpose |
|------|---------|
| `tools/SaveConverter/` | SAV -> JSON converter |
| `tools/SaveConverter/FormatAnalyzer.cs` | 旧形式解析 |
| `docs/save-migration.md` | User guide |
| `src/Era.Core/SaveMigration/LegacySaveReader.cs` | 統合読込 |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=SaveMigration"
# All legacy saves convert correctly
```

**Success Criteria**:
- [ ] SAV->JSON 変換成功
- [ ] 全レガシーセーブ読込可能

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 34: Save Migration」を継承 | Grep |
| **Tasks: 等価性検証** | セーブデータ等価性テストを含む | SaveMigration テスト PASS |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

---

## Risk Mitigation

### Foundation Risks (Phase 3-13)

| Risk | Probability | Impact | Mitigation |
|------|:-----------:|:------:|------------|
| Variable system semantic mismatch | Medium | Critical | Comprehensive unit tests for all 500+ variable codes |
| Expression evaluation edge cases | Medium | High | Port existing expression tests, add fuzzing |
| Command behavior differences | Medium | High | Side-by-side comparison testing for all 60+ commands |
| Built-in function parity | Low | High | Automated function output comparison |

### Content Migration Risks (Phase 16-18)

| Risk | Probability | Impact | Mitigation |
|------|:-----------:|:------:|------------|
| ERB->YAML conversion incomplete | Medium | High | Build comprehensive test suite, manual review |
| Kojo branching logic loss | Medium | High | KojoComparer for all 117 files |
| DATALIST pattern variations | Medium | Medium | Pattern catalog, edge case logging |

### Game Logic Risks (Phase 19-24)

| Risk | Probability | Impact | Mitigation |
|------|:-----------:|:------:|------------|
| Counter system complexity | High | High | Incremental migration with parallel testing |
| State system inconsistency | Medium | High | Snapshot comparison at each turn |
| AI behavior divergence | Medium | Medium | Deterministic seeding for reproducibility |
| Extension compatibility | Low | Low | Modular design, optional loading |

### Platform Risks (Phase 28-32)

| Risk | Probability | Impact | Mitigation |
|------|:-----------:|:------:|------------|
| WPF text rendering complexity | Low | Medium | DirectWrite proven for CJK text, FlowDocument well-documented |
| Performance regression | Low | Medium | Profile early, optimize hot paths |
| Feature parity gaps | Medium | High | Detailed ERB analysis, acceptance tests |
| Save compatibility | Medium | Medium | Converter tool for old saves |

### Overall Risk Summary

| Phase Group | Risk Level | Critical Path |
|-------------|:----------:|:-------------:|
| Foundation (3-11) | High | Yes |
| Content (12-18) | Medium | Yes |
| Game Logic (19-27) | High | Parallel OK |
| Platform (28-33) | Low | Sequential |

---

## Technology Stack

| Component | Technology | Rationale |
|-----------|------------|-----------|
| Content | YAML | Human-readable, widely supported |
| Logic | C# 14 / .NET 10 | Modern features, strong typing |
| UI | WPF (.NET 10) | DirectWrite text rendering, native .NET integration |
| YAML Parser | YamlDotNet | Mature, well-documented |
| Testing | MSTest + Moq | .NET standard |
| CI | GitHub Actions | Existing infrastructure |

---

## Resolved Design Decisions

> All design questions resolved during Phase 0-3 implementation.

| Question | Decision | Rationale | Resolved |
|----------|----------|-----------|:--------:|
| Variable syntax | `{master}` | Simpler, matches existing ERB patterns | 2026-01 |
| Weighted selection | Pure random | Simpler implementation, matches legacy behavior | 2026-01 |
| Save format | JSON | Human-readable, debuggable, standard tooling | 2026-01 |
| UI platform | WPF (.NET 10) | Native .NET 10, DirectWrite text, Windows-only distribution | 2026-03 |

---

## Links

- [feature-341.md](../../feature-341.md) - Architecture Research
- [feature-343.md](../../feature-343.md) - This design's feature
- [content-roadmap.md](../../content-roadmap.md) - Version roadmap
- [ntr-core-overview.md](../../reference/ntr-core-overview.md) - NTR system reference
