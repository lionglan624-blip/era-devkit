# Phase 20-27: Game Systems Migration

**Parent**: [full-csharp-architecture.md](../full-csharp-architecture.md)

---

### Phase 20: Equipment & Shop Systems (was Phase 19)

**Phase Status**: DONE

**Goal**: 装備・アイテム・ショップシステムの移行

**Background**: ショップとキャラカスタマイズは独立した複雑なサブシステム。Counter System と State Systems の前に移行推奨。

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **Strongly Typed IDs**:
> - `ItemId` - アイテム識別
> - `ShopId` - ショップ識別
> - `EquipmentSlot` - 装備スロット（enum）
>
> **インターフェース定義**:
> ```csharp
> public interface IShopSystem
> {
>     Result<IReadOnlyList<ShopItem>> GetAvailableItems(ShopId shop);
>     Result<PurchaseResult> Purchase(CharacterId buyer, ItemId item);
> }
>
> public interface IInventoryManager
> {
>     Result<bool> HasItem(CharacterId owner, ItemId item);
>     Result<Unit> AddItem(CharacterId owner, ItemId item, int count);
> }
> ```
>
> **DI登録**: `IShopSystem`, `IInventoryManager`, `IBodySettings`

**Tasks**:
1. SHOP*.ERB 移行（ショップシステム 5種）
2. アイテム管理システム
3. コレクション追跡
4. **CHARA_CUSTUM.ERB 移行（キャラカスタマイズ）**
5. **TALENTCOPY.ERB 移行（素質コピー）**
6. **体設定.ERB 移行（ボディ設定 - 1,974行）**
7. **アイテム説明.ERB 移行（アイテム説明文）**
8. **Create Phase 20 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 20 セクションとの整合確認必須
9. **Create Phase 21 Planning feature** (type: research, include transition feature tasks)

**Source Analysis**:

| File | Lines | Purpose | Complexity |
|------|:-----:|---------|:----------:|
| `SHOP.ERB` | 197 | ショップメイン（分岐ルーター） | Medium |
| `SHOP2.ERB` | 246 | 第2ショップ | Medium |
| `SHOP_COLLECTION.ERB` | 353 | コレクション | Medium |
| `SHOP_CUSTOM.ERB` | 472 | カスタマイズ | Medium |
| `SHOP_ITEM.ERB` | 559 | アイテム選択 | Medium |
| `CHARA_CUSTUM.ERB` | 28 | キャラカスタマイズ | Low |
| `TALENTCOPY.ERB` | 110 | 素質コピー | Low |
| `体設定.ERB` | 1,976 | ボディ設定（**大規模**） | **HIGH** |
| `アイテム説明.ERB` | 232 | アイテム説明テキスト | Low |
| `CHARA_SET.ERB` | 189 | キャラクター設定 | Medium |

**Note**: 装備システムはSHOP系ファイルとCSVで管理。独立したEQUIP.ERBは存在しない。

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/Shop/ShopSystem.cs` | ショップメイン |
| `src/Era.Core/Shop/CollectionTracker.cs` | コレクション |
| `src/Era.Core/Shop/ItemPurchase.cs` | アイテム購入ロジック |
| `src/Era.Core/Shop/InventoryManager.cs` | アイテム管理 |
| `src/Era.Core/Shop/ItemDescriptions.cs` | **アイテム説明** |
| `src/Era.Core/Interfaces/IItemVariables.cs` | アイテム変数インターフェース |
| `src/Era.Core/Character/CharacterCustomizer.cs` | **キャラカスタマイズ** |
| `src/Era.Core/Character/TalentCopier.cs` | **素質コピー** |
| `src/Era.Core/Character/BodySettings.cs` | **ボディ設定** |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=Shop"
# All shop and customization operations match legacy behavior
```

**Success Criteria**:
- [ ] ショップシステム移行完了
- [ ] ボディ設定移行完了
- [ ] 全テスト PASS

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 20: Equipment & Shop」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **Tasks: 等価性検証** | legacy 実装との等価性テストを含む | AC にテスト存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 21 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 21: Counter System (was Phase 20)

> **⚠️ 粒度警告**: 本Phaseは ~25,862行・~30ファイルと Phase 20 の6倍。Planning Feature では作業を複数 sub-feature に分割すること (Main Counter / WC Counter / COMABLE+SOURCE)。

**Phase Status**: DONE

**Goal**: 行動選択システムの移行（メイン + WC + COMABLE + SOURCE）

**Background**: ゲーム中の行動選択ロジック。メインカウンターと WC カウンターの2系統 + コマンド可否判定 + ソース管理。

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **インターフェース定義**:
> ```csharp
> public interface ICounterSystem
> {
>     Result<IReadOnlyList<ActionOption>> GetAvailableActions(IGameState state);
>     Result<ActionResult> ExecuteAction(ActionId action);
> }
>
> public interface IComAvailabilityChecker
> {
>     Result<bool> IsAvailable(ComId com, CharacterId target, IGameState state);
>     Result<string> GetUnavailableReason(ComId com, CharacterId target);
> }
> ```
>
> **SRP分割**:
> - COUNTER_*.ERB 8ファイル → 責務別クラス
> - TOILET_COUNTER_*.ERB → 別サブシステムとして分離
>
> **Strategy Pattern適用**:
> - 行動選択ロジック → `IActionSelector` 実装クラス群

**Tasks**:
1. COUNTER_*.ERB 移行（8+ ファイル）
2. TOILET_COUNTER_*.ERB 移行（13 ファイル）
3. 行動可否判定ロジック
4. メッセージ生成システム
5. ポーズ・リアクション処理
6. **COMABLE*.ERB 移行（4ファイル）**
7. **SOURCE*.ERB 移行（5ファイル）**
8. **Create Phase 21 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 21 セクションとの整合確認必須
9. **Create Phase 22 Planning feature** (type: research, include transition feature tasks)
10. **E2E Checkpoint CP-2 Step 2a** - E2E基盤構築 + DI全解決 + Training->Counter cross-system。詳細は [architecture.md CP-2](../full-csharp-architecture.md#incremental-e2e-test-strategy) 参照

**Main Counter Files**:

| File | Purpose | Complexity |
|------|---------|:----------:|
| COUNTER_SELECT.ERB | 行動選択 | High |
| COUNTER_ACTABLE.ERB | 実行可能行動フィルタ | High |
| COUNTER_COMBINATION.ERB | 行動組み合わせ (10行スタブ) | Low |
| COUNTER_MESSAGE.ERB | メッセージ生成 | High |
| COUNTER_POSE.ERB | ポーズシステム | Medium |
| COUNTER_REACTION.ERB | リアクション | Medium |
| COUNTER_PUNISHMENT.ERB | 罰則行動 (44行) | Low |
| COUNTER_SOURCE.ERB | ソース管理（920行） | **HIGH** |

> **Note**: `COUNTER_COMBINATION.ERB` の実ファイル名は `COUNTER_CONBINATION.ERB`（typo）。C# 移行時に正しい綴り `Combination` に修正すること。

**WC Counter Files**:

| File | Purpose | Complexity |
|------|---------|:----------:|
| TOILET_COUNTER.ERB | WC 行動選択 | High |
| TOILET_COUNTER_ACTABLE.ERB | WC 行動可否 | High |
| TOILET_COUNTER_COMBINATION.ERB | WC 組み合わせ | Medium |
| TOILET_COUNTER_MESSAGE.ERB | WC メッセージ | High |
| TOILET_COUNTER_MESSAGE_ITEM.ERB | 専用メッセージ (ITEM) | Medium |
| TOILET_COUNTER_MESSAGE_NTR.ERB | 専用メッセージ (NTR) | Medium |
| TOILET_COUNTER_MESSAGE_SEX.ERB | 専用メッセージ (SEX, 3,517行) | **HIGH** |
| TOILET_COUNTER_MESSAGE_TEASE.ERB | 専用メッセージ (TEASE, 3,482行) | **HIGH** |
| TOILET_COUNTER_POSE.ERB | WC ポーズ | Medium |
| TOILET_COUNTER_PUNISHMENT.ERB | WC 罰則 | Medium |
| TOILET_COUNTER_REACTION.ERB | WC リアクション | Medium |
| TOILET_COUNTER_SOURCE.ERB | WC ソース処理（1,411行） | **HIGH** |
| TOILET_EVENT_KOJO.ERB | WC イベント口上 | Medium |

**COMABLE Files** (Previously undocumented):

| File | Purpose | Complexity |
|------|---------|:----------:|
| COMABLE.ERB | コマンド可否判定メイン | **HIGH** |
| COMABLE2.ERB | 追加判定ロジック（149行） | Medium |
| COMABLE_300.ERB | 会話系コマンド可否 | Medium |
| COMABLE_400.ERB | 訓練系コマンド可否 | Medium |

**SOURCE Files** (Previously undocumented):

| File | Purpose | Complexity |
|------|---------|:----------:|
| SOURCE.ERB | ソース（体液）システムコア | **HIGH** |
| SOURCE1.ERB | 追加ソース処理（1,959行） | **HIGH** |
| SOURCE_CALLCOM.ERB | ソース→COM連携 | Medium |
| SOURCE_POSE.ERB | ソースポーズ処理 | Medium |
| SOURCE_SHOOT.ERB | 射出処理 | Medium |

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/Counter/ActionSelector.cs` | 行動選択 |
| `src/Era.Core/Counter/ActionValidator.cs` | 行動可否判定 |
| `src/Era.Core/Counter/MessageGenerator.cs` | メッセージ生成 |
| `src/Era.Core/Counter/PoseManager.cs` | ポーズ管理 |
| `src/Era.Core/Counter/Wc*.cs` | WC 専用サブシステム（F801 で確立されたフラット構造に従う） |
| `src/Era.Core/Counter/Comable/ComableChecker.cs` | **コマンド可否判定** |
| `src/Era.Core/Counter/Comable/ComableRules.cs` | **可否ルール定義** |
| `src/Era.Core/Source/SourceSystem.cs` | **ソースシステム** |
| `src/Era.Core/Source/SourceCalculator.cs` | **ソース計算** |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=Counter"
dotnet test Era.Core.Tests --filter "Category=Comable"
dotnet test Era.Core.Tests --filter "Category=Source"
# All action selection matches legacy behavior
```

**Success Criteria**:
- [ ] Counter システム移行完了
- [ ] Comable 判定移行完了
- [ ] Source システム移行完了
- [ ] 全テスト PASS
- [x] **CP-2 Step 2a PASS**: E2E基盤 + DI全解決 + Training->Counter cross-system

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 21: Counter System」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **Tasks: 等価性検証** | legacy 実装との等価性テストを含む | AC にテスト存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |
| **AC: CP-2 Step 2a** | E2E基盤 + DI全解決テストを含む | E2E テスト PASS |

**Next**: Create Phase 22 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 22: State Systems (was Phase 21)

**Phase Status**: DONE

**Goal**: 状態管理システムの移行（服装・妊娠・汚れ・部屋・環境）

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **インターフェース定義**:
> ```csharp
> public interface IClothingSystem
> {
>     Result<ClothingState> GetState(CharacterId character);
>     Result<Unit> ChangeClothes(CharacterId character, ClothingId clothing);
> }
>
> public interface IPregnancySystem
> {
>     Result<PregnancyState> GetState(CharacterId character);
>     Result<Unit> AdvanceDay(CharacterId character);
> }
>
> public interface IEnvironmentSystem
> {
>     GameTime CurrentTime { get; }
>     Weather CurrentWeather { get; }
> }
> ```
>
> **Value Objects**:
> - `PregnancyState` - 妊娠状態（不変レコード）
> - `ClothingState` - 服装状態
> - `Weather` - 天候（enum）

**Deferred Obligations (from F811)**:
- IKnickersSystem full implementation (CLOTHES_Change_Knickers) - clothing system scope
- GetNWithVisitor (INtrUtilityService) - NTR_VISITOR.ERB visitor location count

**Deferred Obligations (from Phase 22 Consolidation F829)**:
- 12 obligations routed to Phase 25, standalone features (F830-F833), or NOT_FEASIBLE — see [feature-829.md](../../../pm/features/feature-829.md) for full routing table

**Tasks**:
1. 服装システム移行（CLOTHES*.ERB）
2. 妊娠システム移行（PREGNACY_S*.ERB）
3. 汚れ管理移行（STAIN.ERB）
4. 部屋状態管理（ROOM_SMELL.ERB）
5. **生理機能追加パッチ.ERB 移行（生理周期システム）**
6. **睡眠深度.ERB 移行（睡眠状態管理）**
7. **天候.ERB 移行（天候システム）**
8. **続柄.ERB 移行（関係性定義）**
9. **Create Phase 22 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 22 セクションとの整合確認必須
10. **Create Phase 23 Planning feature** (type: research, include transition feature tasks)
11. **E2E Checkpoint CP-2 Step 2c** (完成) - Phase 20-22 全統合検証 + Step 2a-2b 退行なし。詳細は [architecture.md CP-2](../full-csharp-architecture.md#incremental-e2e-test-strategy) 参照
12. **Roslynator Analyzers 導入調査**: `Roslynator.Analyzers` パッケージ追加の可否を調査。500+ルールのうちプロジェクトに適合するものを選定、不要ルールのNoWarn追加含めて Planning sub-feature で検証。参考: `<PackageReference Include="Roslynator.Analyzers" Version="4.*" PrivateAssets="all" />`

**Clothing System**:

| File | Purpose | Complexity |
|------|---------|:----------:|
| CLOTHES.ERB | 服装管理 | High |
| CLOTHES_SYSTEM.ERB | 服装メカニクス | High |
| CLOTHES_Cosplay.ERB | コスプレ対応 | Medium |
| CLOTHE_EFFECT.ERB | 効果計算 | Medium |

**Pregnancy System**:

| File | Purpose | Complexity |
|------|---------|:----------:|
| PREGNACY_S.ERB | 妊娠システム | High |
| PREGNACY_S_EVENT.ERB | 妊娠イベント（28行） | Low |
| PREGNACY_S_EVENT0.ERB | 初期イベント（16行） | Low |
| PREGNACY_S_CHILD_MOVEMENT.ERB | 子供移動 | Medium |

**Environment & Relationship Systems**:

| File | Lines | Purpose | Complexity |
|------|------:|---------|:----------:|
| `生理機能追加パッチ.ERB` | 163 | 生理周期システム | Medium |
| `睡眠深度.ERB` | 244 | 睡眠深度管理 | Medium |
| `天候.ERB` | 839 | 天候システム（暦法+気温+異常気象の統合） | **HIGH** |
| `続柄.ERB` | 379 | 関係性（親子、配偶者等） | Medium |
| `STAIN.ERB` | - | 汚れ管理 | Medium |
| `ROOM_SMELL.ERB` | 1,140 | 部屋の匂い管理 | **HIGH** |

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/State/ClothingSystem.cs` | 服装管理 |
| `src/Era.Core/State/PregnancySystem.cs` | 妊娠管理 |
| `src/Era.Core/State/StainManager.cs` | 汚れ管理 |
| `src/Era.Core/State/RoomState.cs` | 部屋状態 |
| `src/Era.Core/State/MenstrualCycle.cs` | **生理周期** |
| `src/Era.Core/State/SleepDepth.cs` | **睡眠深度** |
| `src/Era.Core/State/WeatherSystem.cs` | **天候** |
| `src/Era.Core/Character/Relationships.cs` | **関係性** |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=StateSystem"
```

**Success Criteria**:
- [x] 服装システム移行完了
- [x] 妊娠システム移行完了
- [x] 環境システム移行完了
- [x] 全テスト PASS
- [x] **CP-2 Step 2c PASS**: Phase 20-22 全統合 + Step 2a-2b 退行なし

**E2E Checkpoint CP-2 Step 2c** (Post-Phase Review 必須検証項目):

> **参照**: [architecture.md Incremental E2E Test Strategy](../full-csharp-architecture.md#incremental-e2e-test-strategy)

| 検証項目 | 内容 | 期待結果 |
|----------|------|----------|
| DI統合解決 | Phase 20-22 の全サービスをDIコンテナに登録 | 例外なく解決 |
| Cross-system フロー | Shop購入->Counter更新->State変化 | 一連の状態遷移が正しく伝播 |
| Headless再現性 | seeded IRandomProvider で決定的実行 | 同一seed = 同一結果 |
| Step 2a 退行 | Phase 21 で作成した E2E が全 PASS | 退行なし |

**実装先**: `src/Era.Core.Tests/E2E/Phase22IntegrationTests.cs`

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 22: State Systems」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **Tasks: 等価性検証** | legacy 実装との等価性テストを含む | AC にテスト存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |
| **AC: CP-2 E2E** | Phase 20-22 統合E2E検証を含む | E2E テスト PASS |

**Next**: Create Phase 23 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 23: NTR Kojo Reference Analysis (was Phase 22)

**Phase Status**: DONE

**Goal**: 既存NTR口上の分岐パターンを分析し、Phase 24 (NTR Bounded Context設計) および content-roadmap Phase 8h/8m/8n の実装ベースラインを確立

**Background**: 咲夜は最大NTR口上量（16,146行）を持つリファレンス実装。DDD設計前に既存分岐パターンを定量分析し、Value Object・Aggregate設計の入力とする。

**Reference Character**: 咲夜 (K4)

#### Existing NTR Branch Statistics

**FAV (好感度段階) 分岐**:

| FAV Level | 出現回数 | content-roadmap 8h 対応 |
|-----------|-------:|:------------:|
| FAV_寝取られ | 177 | Level 9 |
| FAV_寝取られ寸前 | 177 | Level 8 |
| FAV_寝取られそう | 149 | Level 7 |
| FAV_主人より高い | 105 | Level 6 |
| FAV_うふふする程度 | 33 | Level 3 |
| FAV_うふふする程度2 | 9 | Level 4 |
| FAV_奉仕する程度 | 8 | Level 5 |
| FAV_体を触らせる程度 | 2 | Level 2 |
| FAV_キスする程度 | 2 | Level 1 |
| **合計** | **662** | 9段階全対応済 |

> **注意**: FAVシステムは実際には **11段階**（上記9段階 + `FAV_寝取り返し寸前`, `FAV_寝取り返し中`）。上記統計では2段階が未カウント。

**評価**: content-roadmap Phase 8h で計画している9段階FAV分岐は既に咲夜で実装済み。他キャラへの展開が8hの主要タスク。

**TALENT (状態) 分岐**:

| TALENT | 出現回数 | 分類 |
|--------|-------:|------|
| TALENT:奴隷:恋慕 | 516 | 感情状態 |
| TALENT:奴隷:浮気公認 | 192 | 許可状態 |
| TALENT:奴隷:処女 | 164 | 身体状態 |
| TALENT:奴隷:公衆便所 | 92 | 堕落状態 |
| TALENT:奴隷:親愛 | 40 | 感情状態 |
| TALENT:奴隷:危険日 | 23 | 身体状態 |
| TALENT:奴隷:人妻 | 15 | 関係状態 |
| TALENT:奴隷:傷心 | 3 | 感情状態 |
| TALENT:奴隷:淫乱 | 3 | 性格状態 |

**分岐軸の複合パターン**:
```
FAV段階 (9) x 感情状態 (恋慕/親愛/傷心) x 身体状態 (処女/危険日) x 許可状態 (浮気公認)
```

**状況分岐**:

| 条件 | 出現回数 | 説明 |
|------|-------:|------|
| NTR_CHK_PEEP_CONT \|\| 主人同席 | 102 | 主人の存在 |
| NTR_CHK_PEEP_CONT | 53 | 覗き中 |
| CHK_NTR_SATISFACTORY | 32 | 満足度判定 |
| RAND:N | 124+ | ランダム分岐 |

#### File Structure

| ファイル | 行数 | 関数数 | 主要内容 |
|----------|-----:|-------:|----------|
| NTR口上_シナリオ9.ERB | 3,697 | 38 | 完全寝取られルート |
| NTR口上_シナリオ8.ERB | 3,553 | 28 | 寝取られ寸前ルート |
| NTR口上_シナリオ1-7.ERB | 3,496 | 33 | 初期〜中期ルート |
| NTR口上_お持ち帰り.ERB | 2,411 | 24 | お持ち帰りシーン |
| NTR口上_シナリオ11-22.ERB | 1,905 | 22 | 後期バリエーション |
| NTR口上_野外調教.ERB | 1,084 | 19 | 野外シーン |
| **合計** | **16,146** | **164** | |

#### Roadmap Phase Mapping

**Phase 8h (NTR Kojo Enhancement) との対応**:

| 8h Sub | 計画内容 | 咲夜既存 | Gap |
|--------|----------|:--------:|-----|
| 8h-1 FAV 9段階 | ~3600行 | **16,146行** | 既存が4.5倍超過 |
| 8h-2 比較口上 | ~800行 | 一部実装 | 追加必要 |
| 8h-3 事後口上 | ~600行 | 一部実装 | 追加必要 |
| 8h-4 3P詳細 | ~600行 | 未実装 | 新規必要 |
| 8h-5 発覚口上 | ~600行 | 未実装 | 新規必要 |

**結論**: 咲夜は Phase 8h の FAV分岐を先取り実装。8h-2〜8h-5 の追加で完成。

**Phase 8m (MC Interaction) との対応**:

| 8m Sub | 計画内容 | 咲夜既存 | 対応 |
|--------|----------|:--------:|------|
| 8m-1 軽スキンシップ | ~130行 | 未実装 | 新規 |
| 8m-2 キス系 | ~160行 | 未実装 | 新規 |
| 8m-3 性行為 | ~500行 | 一部(CHK_NTR_SATISFACTORY) | 拡張 |
| 8m-4 発覚反応 | ~240行 | 未実装 | 新規 |

**結論**: 咲夜既存は訪問者視点中心。Phase 8m は MC視点の追加が必要。

**Phase 8n (Netorase) との対応**:

| 8n Sub | 計画内容 | 咲夜既存 | 対応 |
|--------|----------|:--------:|------|
| 8n-1 受容度段階 | ~1200行 | 未実装 | 新規 |
| 8n-2 煽り・比較 | ~400行 | 未実装 | 新規 |
| 8n-3 秘密欲求 | ~300行 | 未実装 | 新規 |
| 8n-4 寝取り返し | ~500行 | 未実装 | 新規 |

**結論**: Netorase は新システム。咲夜既存とは別軸で新規作成。

#### DDD Design Implications (Phase 24 Input)

**Value Object 候補** (既存分岐から抽出):

| Value Object | 根拠 | 値域 |
|--------------|------|------|
| `FavLevel` | FAV_* 11段階分岐 (寝取り返し寸前/中 含む) | Level 1-11 |
| `AffairPermission` | TALENT:浮気公認 分岐 | 0-3 段階 |
| `CorruptionState` | TALENT:公衆便所 等 | enum |
| `PeepingContext` | NTR_CHK_PEEP_CONT | bool + 主人同席 |

**Aggregate 境界**:
```
NtrProgression (集約ルート)
├── FavLevel (11段階)
├── EmotionalState (恋慕/親愛/傷心)
├── PhysicalState (処女/危険日)
├── PermissionState (浮気公認)
└── SituationContext (覗き/同席)
```

**YAML Migration Pattern**:

```yaml
ntr_kojo:
  character: sakuya
  scene: scenario_9
  conditions:
    fav_level: [寝取られ, 寝取られ寸前]
    talent:
      恋慕: true
      浮気公認: [1, 2, 3]
    situation:
      peeping: false
      master_present: false
  variants:
    - weight: 1
      lines: ["台詞1", "台詞2"]
```

#### Summary

| 指標 | 咲夜既存 | Roadmap計画 | 評価 |
|------|-------:|------------:|------|
| FAV分岐 | 662回 | 9段階 | 先取り実装 |
| NTR行数 | 16,146行 | 6,200行/10キャラ | 2.6倍超過 |
| Phase 8h | 80%実装済 | - | 8h-4,5 追加のみ |
| Phase 8m | 10%実装済 | - | MC視点新規必要 |
| Phase 8n | 0%実装済 | - | 新システム |

**Tasks**:
1. 全10キャラのNTR口上分岐パターン分析
2. FAV/TALENT/状況 分岐の統計レポート作成
3. Phase 24 DDD設計への入力ドキュメント作成
4. content-roadmap Phase 8h/8m/8n との Gap 分析更新

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `pm/reference/ntr-kojo-analysis.md` | 全キャラ分岐統計 |
| `pm/reference/ntr-ddd-input.md` | Phase 24 設計入力 |

**Success Criteria**:
- [x] 全10キャラ NTR分岐統計完了
- [x] Phase 24 Value Object 候補リスト確定
- [x] content-roadmap 8h/8m/8n Gap 分析完了

**Next**: Create Phase 24 planning feature when this phase completes

> **咲夜は NTR 口上のリファレンス実装**として、他キャラ展開・Phase 24 DDD設計のベースラインとなる。

### Phase 24: NTR Bounded Context設計 (was Phase 23)

**Phase Status**: DONE

**Goal**: NTRシステムの独立Bounded Context化

**Background**: NTRシステムは本ゲームの最複雑部分（14ファイル、14,069行）。Phase 25でのNTR実装移行前に、独立したドメインモデルを設計し、システム全体の疎結合化を図る。

> **⚠️ 新規設計概念の注意**: Route R0-R6 および NtrPhase 0-7 は **C#移行で新たに導入する概念**。既存ERBにはこの区分は存在せず、FAVレベル（11段階: FAV_寝取り返し寸前, FAV_寝取り返し中 を含む）+ TALENTフラグの組み合わせで状態管理している。Aggregate境界も既存ERBはキャラ単体管理（単一訪問者前提）であり、キャラ×訪問者ペア設計は新規。

> **⚠️ Bounded Context原則（このPhaseの本質）**
>
> **独立ドメイン**:
> - NTRシステムは他システムから独立したBounded Contextとして設計
> - 外部とはAnti-Corruption Layerを介して通信
> - NTR固有のUbiquitous Language（ルート、フェーズ、FAV等）を定義
>
> **イベント駆動設計**:
> - NTR状態変化をドメインイベントとして発行
> - 口上システム、UI、その他システムはイベント購読で反応
>
> **Aggregate設計**:
> - NtrProgression（キャラ×訪問者ペアの進行状態）が集約ルート

**Tasks**:
1. NTR Ubiquitous Language定義（用語辞書）
2. NtrProgression Aggregate設計・実装
3. NtrRoute Value Object実装（R0-R6）
4. NtrPhase Value Object実装（Phase 0-7）
5. NtrParameters Value Object実装（FAV_*, 露出度等）
6. NTR Domain Events定義（PhaseAdvanced, RouteChanged等）
7. INtrCalculator Domain Service設計
8. NTR Application Services設計
9. Anti-Corruption Layer設計（既存システムとの橋渡し）
10. **Create Phase 24 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 24 セクションとの整合確認必須
11. **Create Phase 25 Planning feature** (type: research, include transition feature tasks)

**NTR Mark Integration (from F406 Deferred)**:

> **⚠️ F406 引き継ぎ事項**: OrgasmProcessor.CalculateOrgasmRequirementCoefficient (lines 136-148) で
> NTR mark checking がスタブのまま。Phase 25-26 で実装すること。

**ERB Reference** (TRACHECK_ORGASM.ERB lines 40-45):
```erb
;訪問者以外の調教で、快楽刻印よりも浮気快楽刻印のレベルが高い場合、絶頂しづらくなる
IF 調教者 != 998 && GET_MARK_LEVEL(奴隷,刻印番号_快楽刻印,調教者) < MARK:奴隷:浮気快楽刻印
    絶頂要求係数 = MAX( 100 * MARK:奴隷:浮気快楽刻印 / MAX(1,GET_MARK_LEVEL(奴隷,刻印番号_快楽刻印,調教者)) , 100 )
ELSE
    絶頂要求係数 = 100
ENDIF
```

**Required Components**:

| Component | Description | Dependency |
|-----------|-------------|------------|
| GET_MARK_LEVEL | マーク所有者別レベル取得関数 | F366 Deferred -> Phase 25 |
| 浮気快楽刻印 (MarkIndex.AffairPleasure) | F399で定義済み (index 5) | - |
| INtrCalculator.CalculateOrgasmCoefficient | NTR絶頂要求係数計算 | Phase 25 Task 7 |

**C# Integration Pattern**:
```csharp
// src/Era.Core/Training/OrgasmProcessor.cs (F406で実装済み - TODO stub)
private int CalculateOrgasmRequirementCoefficient(CharacterId target, CharacterId trainer)
{
    // Phase 25-26 で実装:
    // 1. INtrCalculator を DI で注入
    // 2. _ntrCalculator.CalculateOrgasmCoefficient(target, trainer) を呼び出す
    return 100; // Current stub
}
```

**NTR Domain Model Structure**:

```
src/Era.Core/
└── NTR/                              # NTR Bounded Context
    ├── Domain/
    │   ├── Aggregates/
    │   │   └── NtrProgression.cs     # 集約ルート
    │   ├── ValueObjects/
    │   │   ├── NtrRoute.cs           # R0-R6
    │   │   ├── NtrPhase.cs           # Phase 0-7
    │   │   ├── NtrParameters.cs      # FAV系パラメータ
    │   │   └── Susceptibility.cs     # 耐性/脆弱性
    │   ├── Events/
    │   │   ├── NtrPhaseAdvanced.cs
    │   │   ├── NtrRouteChanged.cs
    │   │   ├── NtrExposureLevelChanged.cs
    │   │   └── NtrCorrupted.cs
    │   ├── Services/
    │   │   └── INtrCalculator.cs     # ドメインサービス
    │   └── Repositories/
    │       └── INtrProgressionRepository.cs
    ├── Application/
    │   ├── Commands/
    │   │   ├── AdvanceNtrPhaseCommand.cs
    │   │   └── ChangeNtrRouteCommand.cs
    │   ├── Queries/
    │   │   ├── GetNtrStatusQuery.cs
    │   │   └── GetSusceptibilityQuery.cs
    │   └── EventHandlers/
    │       └── NtrPhaseAdvancedHandler.cs
    └── Infrastructure/
        ├── NtrProgressionRepository.cs
        └── NtrQueryAcl.cs
```

**Core Aggregate**:

```csharp
// src/Era.Core/NTR/Domain/Aggregates/NtrProgression.cs
public class NtrProgression : AggregateRoot<NtrProgressionId>
{
    public CharacterId TargetCharacter { get; private set; }
    public CharacterId Visitor { get; private set; }
    public NtrRoute CurrentRoute { get; private set; }
    public NtrPhase CurrentPhase { get; private set; }
    public NtrParameters Parameters { get; private set; }

    // ドメインメソッド
    public Result<Unit> AdvancePhase()
    {
        if (!calculator.CanAdvance(this))
            return Result<Unit>.Fail("Conditions not met for phase advancement");

        var newPhase = CurrentPhase.Next();
        AddDomainEvent(new NtrPhaseAdvanced(Id, CurrentPhase, newPhase));
        CurrentPhase = newPhase;
        return Result<Unit>.Ok(Unit.Value);
    }

    public Result<Unit> ChangeRoute(NtrRoute newRoute, INtrCalculator calculator)
    {
        if (!calculator.CanChangeRoute(this, newRoute))
            return Result<Unit>.Fail($"Cannot change to route {newRoute}");

        AddDomainEvent(new NtrRouteChanged(Id, CurrentRoute, newRoute));
        CurrentRoute = newRoute;
        return Result<Unit>.Ok(Unit.Value);
    }
}
```

**Value Objects**:

```csharp
// src/Era.Core/NTR/Domain/ValueObjects/NtrRoute.cs
public readonly record struct NtrRoute
{
    public int Value { get; }

    public static readonly NtrRoute R0 = new(0); // 未接触
    public static readonly NtrRoute R1 = new(1); // 強制/調教
    public static readonly NtrRoute R2 = new(2); // 懐柔
    public static readonly NtrRoute R3 = new(3); // 欲情
    public static readonly NtrRoute R4 = new(4); // 恋愛
    public static readonly NtrRoute R5 = new(5); // 混合
    public static readonly NtrRoute R6 = new(6); // 純愛堕ち

    private NtrRoute(int value) => Value = value;

    public bool IsCorrupted => Value >= 4;
}

// src/Era.Core/NTR/Domain/ValueObjects/NtrPhase.cs
public readonly record struct NtrPhase
{
    public int Value { get; }

    public static readonly NtrPhase Phase0 = new(0); // 無関心
    public static readonly NtrPhase Phase1 = new(1); // 接触
    public static readonly NtrPhase Phase2 = new(2); // 動揺
    public static readonly NtrPhase Phase3 = new(3); // 葛藤
    public static readonly NtrPhase Phase4 = new(4); // 受容
    public static readonly NtrPhase Phase5 = new(5); // 依存
    public static readonly NtrPhase Phase6 = new(6); // 堕落
    public static readonly NtrPhase Phase7 = new(7); // 完堕ち

    private NtrPhase(int value) => Value = value;

    public NtrPhase Next() => new(Math.Min(Value + 1, 7));
    public bool IsComplete => Value >= 7;
}
```

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/NTR/Domain/Aggregates/NtrProgression.cs` | NTR進行状態集約 |
| `src/Era.Core/NTR/Domain/ValueObjects/*.cs` | Route, Phase, Parameters等 |
| `src/Era.Core/NTR/Domain/Events/*.cs` | ドメインイベント |
| `src/Era.Core/NTR/Domain/Services/INtrCalculator.cs` | 計算ドメインサービス |
| `src/Era.Core/NTR/Domain/Repositories/INtrProgressionRepository.cs` | リポジトリインターフェース（DIP） |
| `src/Era.Core/NTR/Application/Commands/*.cs` | アプリケーションコマンド |
| `src/Era.Core/NTR/Application/Queries/*.cs` | クエリ |
| `src/Era.Core/NTR/Infrastructure/*.cs` | リポジトリ、ACL |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=NtrDomain"
# Aggregate invariants verified
# Route/Phase transitions validated
# Domain events correctly raised
```

**Success Criteria**:
- [x] NTR Bounded Context 確立
- [x] Aggregate Root パターン確立
- [x] Domain Events 発行機能
- [x] 全テスト PASS

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 24: NTR Bounded Context」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 25 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 25: AI & Visitor Systems (was Phase 24)

> **⚠️ 粒度警告**: 本Phaseは ~24,000行・38+ファイルと巨大。Planning Feature では以下の3分割を推奨:
> 1. **NTR Implementation** (14ファイル, ~14,069行) - NTRサブシステム移行
> 2. **Visitor & Event** (7ファイル, ~7,164行 + INTRUDER 324行) - イベント・訪問者
> 3. **Location Extensions** (16ファイル, ~2,509行 + AFFAIR_DISCLOSURE ~103行) - 訪問先拡張

**Phase Status**: TODO

**Deferred Obligations (from F829)**:
- OB-02: SANTA cosplay text output (CLOTHE_EFFECT.ERB @SANTA PRINT calls) — requires engine-layer UI primitives
- OB-03: CLOTHES_ACCESSORY/INtrQuery wiring — trigger: when NTR system migrates
- OB-04: IVariableStore 2D SAVEDATA stubs (GetBirthCountByParent/SetBirthCountByParent)
- OB-05: NullMultipleBirthService runtime implementation (8-method stub)
- OB-08: I3DArrayVariables GetDa/SetDa DA gap (ROOM_SMELL_WHOSE_SAMEN)
- OB-10: CP-2 Step 2c behavioral flow test (Shop→Counter→State)

**Goal**: AI・訪問者・NTRサブシステムの移行（拡張版）

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **Strongly Typed IDs**:
> - `VisitorId` - 訪問者識別
> - `NtrEventId` - NTRイベント識別
>
> **インターフェース定義**:
> ```csharp
> public interface IVisitorSystem
> {
>     Result<VisitorState> GetCurrentVisitor();
>     Result<Unit> ProcessVisit(VisitorId visitor, IGameState state);
> }
>
> // INtrEngine は Phase 12 で定義済み - ここで実装
> ```
>
> **SRP分割（NTR 14ファイル -> 機能別）**:
> - Core: `NtrEngine.cs` (システム統合)
> - Actions: `NtrActionProcessor.cs`
> - Messages: `NtrMessageGenerator.cs`
> - Events: `NtrEventHandler.cs`

**Tasks**:
1. 訪問者システム移行（INTRUDER.ERB, NTR_VISITOR.ERB）
2. イベントシステム移行（EVENT_*.ERB）
3. ターン終了処理（EVENTTURNEND.ERB）
4. **NTRサブシステム移行**（5ファイル追加）
5. 訪問先拡張（訪問者宅拡張/ 16+ files）
6. **AFFAIR_DISCLOSURE 移行** (INFO.ERB:693-795 -> src/Era.Core/Events/AffairDisclosure.cs) - Phase 3 F378 から延期
7. **Create Phase 25 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 25 セクションとの整合確認必須
8. **Create Phase 26 Planning feature** (type: research, include transition feature tasks)

**AFFAIR_DISCLOSURE Migration** (deferred from F378):

| Item | Details |
|------|---------|
| Source | INFO.ERB lines 693-795 (~103 lines) |
| Target | `src/Era.Core/Events/AffairDisclosure.cs` |
| C# Name | `CheckAffairDisclosure()` |
| Dependencies | IN_ROOM, CAN_MOVE, KOJO_EVENT, TOUCH_SET, 天候, CFLAG/TALENT |
| Requires | IEventContext (defined in this Phase) |
| Post-migration | Update F380 StatusOrchestrator.cs to pure C# |

See [feature-378.md](../../feature-378.md) "AFFAIR_DISCLOSURE Deferral Plan" for technical details.

**Visitor System**:

| File | Purpose | Complexity |
|------|---------|:----------:|
| INTRUDER.ERB | 訪問者システム（324行） | Medium |
| NTR_VISITOR.ERB | NTR 訪問者 | High |
| 訪問者宅拡張/*.ERB | 訪問先拡張 | Medium |

**NTR Subsystems** (14 files in NTR/ folder):

| File | Purpose | Complexity |
|------|---------|:----------:|
| `NTR.ERB` | NTRシステムコア | **HIGH** |
| `NTR_ACTION.ERB` | NTRアクション処理 | HIGH |
| `NTR_COMF416.ERB` | COM416連携処理 | Medium |
| `NTR_EXHIBITION.ERB` | 展示メカニクス（470行, NPC目撃ロジック） | **HIGH** |
| `NTR_FRIENDSHIP.ERB` | NTR友好度管理 | **HIGH** |
| `NTR_MASTER_3P_SEX.ERB` | 3Pシーン処理 | **HIGH** |
| `NTR_MASTER_SEX.ERB` | マスターセックス処理 | **HIGH** |
| `NTR_MESSAGE.ERB` | NTRメッセージ生成 | HIGH |
| `NTR_OPTION.ERB` | NTRオプション設定（505行） | Medium |
| `NTR_SEX.ERB` | セックス処理コア | **HIGH** |
| `NTR_TAKEOUT.ERB` | デート外出 | HIGH |
| `NTR_UTIL.ERB` | NTRユーティリティ（1,501行） | **HIGH** |
| `NTR_VISITOR.ERB` | 訪問者システム | HIGH |
| `NTR陥落イベント.ERB` | 陥落イベント（207行） | Medium |

**訪問者宅拡張 (Location Extension)**:

| File Pattern | Purpose | Count |
|--------------|---------|:-----:|
| `COMF460ex*.ERB` | 場所A訪問 | 5+ |
| `COMF461ex*.ERB` | 場所B訪問 | 5+ |
| `COMF466ex*.ERB` | 場所C訪問 | 5+ |
| `COMF467ex*.ERB` | 場所D訪問 | 5+ |
| `PLACEex.ERB` | 場所定義 | 1 |

**Event System**:

| File | Purpose | Complexity |
|------|---------|:----------:|
| EVENT_KOJO.ERB | イベント口上 | Medium |
| EVENT_MESSAGE.ERB | イベントメッセージ | Medium |
| EVENT_MESSAGE_COM.ERB | COM イベント（4,599行） | **HIGH** |
| EVENT_MESSAGE_ORGASM.ERB | 絶頂イベント | Medium |
| EVENTCOMEND.ERB | COM 終了イベント | Low |
| EVENTTURNEND.ERB | ターン終了イベント | Medium |
| `情事中に踏み込み.ERB` | 侵入イベント | Medium |

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/AI/VisitorAI.cs` | 訪問者 AI |
| `src/Era.Core/AI/ScheduleManager.cs` | スケジュール管理 |
| `src/Era.Core/NTR/FriendshipSystem.cs` | NTR友好度 |
| `src/Era.Core/NTR/ThreesomeHandler.cs` | 3Pシーン |
| `src/Era.Core/NTR/DateOutSystem.cs` | 外出デート |
| `src/Era.Core/NTR/CorruptionEvents.cs` | 陥落イベント |
| `src/Era.Core/Events/EventDispatcher.cs` | イベント発火 |
| `src/Era.Core/Events/EventHandlers.cs` | イベントハンドラ |
| `src/Era.Core/Location/VisitorHomeExtension.cs` | 訪問先拡張 |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=AI"
dotnet test Era.Core.Tests --filter "Category=NTR"
```

**Success Criteria**:
- [ ] 訪問者 AI 移行完了
- [ ] NTR サブシステム移行完了
- [ ] イベント発火機能
- [ ] 全テスト PASS

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 25: AI & Visitor」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **Tasks: 等価性検証** | legacy 実装との等価性テストを含む | AC にテスト存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 26 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 26: Special Modes & Messaging (was Phase 25)

> **⚠️ 粒度警告**: 本Phaseは ~18,600行・21ファイルと巨大。Planning Feature では以下の2分割を推奨:
> 1. **SexHara Mode** (9ファイル, ~13,215行) - セクハラモード関連
> 2. **Social & Message** (6ファイル メッセージ ~1,260行 + 6ファイル 交流 ~4,131行) - 交流・メッセージ・従者

**Phase Status**: TODO

**Goal**: 特殊モード・メッセージ・交流システムの移行（拡張版）

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **インターフェース定義**:
> ```csharp
> public interface IMessageGenerator
> {
>     Result<string> Generate(MessageContext context);
> }
>
> public interface ISpecialModeManager
> {
>     Result<bool> IsActive(SpecialMode mode);
>     Result<Unit> Activate(SpecialMode mode);
> }
> ```
>
> **SRP分割（大規模ファイル）**:
> - `WC_SexHara_MESSAGE.ERB` (1,778行) -> 状況別クラス群
> - `WC_SexHara_MESSAGE_RAPE.ERB` (2,343行) -> シーン別クラス
> - `WC_SexHara_MESSAGE_VILLAGE.ERB` (3,436行) -> 村人別クラス

**Tasks**:
1. SexHara モード移行（5ファイル）
2. メッセージ生成システム（MSG_FUNC.ERB + 関連）
3. 住人交流システム
4. 従者・子供イベント
5. 射精・フィルムシステム
6. **Create Phase 26 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 26 セクションとの整合確認必須
7. **Create Phase 27 Planning feature** (type: research, include transition feature tasks)

**SexHara Mode** (7 WC_SexHara files):

| File | Lines | Purpose | Complexity |
|------|------:|---------|:----------:|
| SexHara.ERB | ~500 | セクハラモード | High |
| WC_SexHara.ERB | ~300 | WC 拡張 | Medium |
| WC_SexHara_ACTABLE.ERB | 614 | 行動可否 | **HIGH** |
| WC_SexHara_MESSAGE.ERB | 1,778 | メッセージ | **HIGH** |
| WC_SexHara_MESSAGE_CARESS.ERB | 1,025 | 愛撫メッセージ | **HIGH** |
| WC_SexHara_MESSAGE_RAPE.ERB | 2,343 | 強姦メッセージ | **HIGH** |
| WC_SexHara_MESSAGE_VILLAGE.ERB | 3,436 | 村人メッセージ | **HIGH** |
| WC_SexHara_SOURCE.ERB | 1,520 | ソース処理 | HIGH |
| SexHara休憩中.ERB | 1,246 | 休憩処理 | **HIGH** |

**Message Generation System**:

| File | Lines | Purpose | Complexity |
|------|------:|---------|:----------:|
| `MSG_FUNC.ERB` | 63 | メッセージ関数（コア） | Low |
| `EJACULATION.ERB` | ~300 | 射精メッセージ | Medium |
| `MAKEFILM.ERB` | ~200 | 撮影システム | Medium |
| `FILMMASSAGE.ERB` | ~100 | フィルムメッセージ | Low |
| `VIDEO.ERB` | ~150 | ビデオ視聴 | Low |
| `ショートカットモード.ERB` | ~100 | ショートカットモード | Low |

**Social & Interaction Systems**:

| File | Purpose | Complexity |
|------|---------|:----------:|
| `住人同士の交流.ERB` | 住人交流（~40KB） | **HIGH** |
| `住人同士の交流設定.ERB` | 交流設定 | Low |
| `従者イベント.ERB` | 従者イベント（1,129行） | **HIGH** |
| `子供の訪問関係.ERB` | 子供訪問 | Medium |
| `GIFT.ERB` | ギフトシステム（3行, dead code - 移行不要の可能性） | Low |
| `貞操帯管理.ERB` | 貞操帯管理 | Medium |

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/Modes/SexHaraMode.cs` | セクハラモード |
| `src/Era.Core/Message/MessageBuilder.cs` | メッセージ生成 |
| `src/Era.Core/Message/EjaculationHandler.cs` | 射精処理 |
| `src/Era.Core/Media/FilmSystem.cs` | 撮影・再生 |
| `src/Era.Core/Interaction/ResidentInteraction.cs` | 住人交流 |
| `src/Era.Core/Interaction/ServantEvents.cs` | 従者イベント |
| `src/Era.Core/Items/GiftSystem.cs` | ギフト管理 |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=SpecialModes"
dotnet test Era.Core.Tests --filter "Category=Messaging"
```

**Success Criteria**:
- [ ] SexHara モード移行完了
- [ ] メッセージ生成システム確立
- [ ] 住人交流移行完了
- [ ] 全テスト PASS

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 26: Special Modes」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **Tasks: 等価性検証** | legacy 実装との等価性テストを含む | AC にテスト存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |

**Next**: Create Phase 27 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。

### Phase 27: Extensions (was Phase 26)

**Phase Status**: TODO

**Goal**: 拡張システムの移行（詳細ブレークダウン）

> **⚠️ Phase 4 Design Requirements（必須）**
>
> **拡張システムのモジュール化**:
> ```csharp
> public interface IExtensionModule
> {
>     string ModuleId { get; }
>     bool IsEnabled { get; }
>     Result<Unit> Initialize(IServiceProvider services);
> }
>
> // 各拡張は独立したモジュールとして実装
> public class FairyMaidExtension : IExtensionModule { }
> public class ConversationExtension : IExtensionModule { }
> ```
>
> **DI登録**: 拡張モジュールは条件付き登録（有効時のみ）

**Tasks**:
1. 経歴拡張移行（memorial_base.ERB）
2. 会話拡張移行（COMF300ex* 10ファイル - トピック別）
3. 外出拡張移行
4. 妖精メイド拡張移行（COMF420ex* 6 files - 独自キャラシステム）
5. **CORE8666.ERB 移行（キャラ固有移動ロジック - 子悪魔専用）**
6. **Empty stub files (COMF421-428) - 将来拡張用に保持**
7. **Create Phase 27 Post-Phase Review feature** (type: infra) - 本ドキュメント Phase 27 セクションとの整合確認必須
8. **Create Phase 28 Planning feature** (type: research, include transition feature tasks)
9. **E2E Checkpoint CP-3 実装** (System E2E) - Phase 20-27 全統合検証。詳細は [architecture.md Incremental E2E Test Strategy](../full-csharp-architecture.md#incremental-e2e-test-strategy) 参照
10. **NetArchTest アーキテクチャテスト導入**: Phase 20-27 で全サブシステム完成後、レイヤー依存関係を自動テスト化。`Era.Core.Domain` → `Era.Core.Infrastructure` 依存禁止、Handler internal 強制等。参考: `<PackageReference Include="NetArchTest.Rules" Version="1.*" />`

**Note**: グラフィック表示は Phase 29 (WPF UI) に統合

**1. 経歴拡張 (History/Memorial)**:

| File | Purpose | Complexity |
|------|---------|:----------:|
| `memorial_base.ERB` | 経歴・記念システム | Medium |

**2. 会話拡張 (Conversation Topics)** - 10 files:

| File | Topic | Complexity |
|------|-------|:----------:|
| `COMF300ex.ERB` | 会話拡張メイン | Medium |
| `COMF300ex01_通常会話.ERB` | 通常会話 | Medium |
| `COMF300ex02_妖精メイド増員.ERB` | 妖精メイド増員 | Medium |
| `COMF300ex10_浮気について.ERB` | **浮気話題** | HIGH |
| `ComF300ex11_好きなプレイについて.ERB` | 好きなプレイ | Medium |
| `ComF300ex12_勃たなくなりました.ERB` | ED話題 | Medium |
| `ComF300ex13_売春について.ERB` | 売春（74行） | Medium |
| `ComF300ex14_結婚指輪について.ERB` | 結婚指輪 | Medium |
| `ComF300ex15_今日のぱんつについて.ERB` | 下着話題 | Low |
| `ComF300ex16_最近おまんこ緩くなったよね？.ERB` | 性的話題 | Medium |

**3. 外出拡張 (Outing)** - 2 files:

| File | Purpose | Complexity |
|------|---------|:----------:|
| `GoOut.ERB` | 外出システムメイン | Medium |
| `GoOut900_訪問者の家へ.ERB` | 訪問者宅外出 | Medium |

**4. 妖精メイド拡張 (Fairy Maid)** - 独自キャラシステム:

| File Pattern | Purpose | Count |
|--------------|---------|:-----:|
| `COMF420ex*.ERB` | 妖精メイド COM | 5 |
| `COMF421ex*.ERB` | 追加 COM | 0 |
| `FairyMaid*.ERB` | 専用処理 | 1 |

**Note**: 妖精メイドは独自のキャラクター進行・パーティ管理を持つ複雑なサブシステム

**Patch Files** (Phase 17 で対応):

| File | Purpose | Complexity |
|------|---------|:----------:|
| `多生児パッチ.ERB` | 多胎児対応 | Low |
| `妊娠処理変更パッチ.ERB` | 妊娠処理修正 | Low |
| `追加パッチverup.ERB` | バージョンアップ対応 | Low |

**Semantic Naming for Extension Files (F464 方針)**:

ERB の "ex" サフィックスは C# では不要。機能に応じたセマンティック名を付け、親カテゴリ直下に配置する。

| ERB File | C# Semantic Name | Directory |
|----------|------------------|-----------|
| `COMF300ex.ERB` | `ConversationMain.cs` | `Daily/` |
| `COMF300ex01_通常会話.ERB` | `NormalConversation.cs` | `Daily/` |
| `COMF300ex02_妖精メイド増員.ERB` | `FairyMaidRecruit.cs` | `Daily/` |
| `COMF300ex10_浮気について.ERB` | `AffairTopic.cs` | `Daily/` |
| `ComF300ex11_好きなプレイについて.ERB` | `FavoritePlayTopic.cs` | `Daily/` |
| `ComF300ex12_勃たなくなりました.ERB` | `EdTopic.cs` | `Daily/` |
| `ComF300ex13_売春について.ERB` | `ProstitutionTopic.cs` | `Daily/` |
| `ComF300ex14_結婚指輪について.ERB` | `WeddingRingTopic.cs` | `Daily/` |
| `ComF300ex15_今日のぱんつについて.ERB` | `UnderwearTopic.cs` | `Daily/` |
| `ComF300ex16_最近おまんこ緩くなったよね？.ERB` | `LooseningTopic.cs` | `Daily/` |
| `COMF420ex*.ERB` | `FairyMaid*.cs` | `Utility/FairyMaid/` |
| `COMF46xex*.ERB` | `Visitor*.cs` | `Visitor/` |

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/Extensions/HistoryExtension.cs` | 経歴拡張 |
| `src/Era.Core/Extensions/ConversationTopics/` | 会話トピック (16クラス) |
| `src/Era.Core/Extensions/OutingExtension.cs` | 外出拡張 |
| `src/Era.Core/Extensions/FairyMaid/` | 妖精メイドサブシステム |
| `src/Era.Core/Extensions/FairyMaid/FairyMaidManager.cs` | 妖精メイド管理 |
| `src/Era.Core/Extensions/FairyMaid/FairyMaidProgression.cs` | 進行システム |
| `src/Era.Core/Extensions/CharacterSpecific/Core8666.cs` | **子悪魔固有移動** |

**Miscellaneous Files**:

| File | Lines | Purpose |
|------|------:|---------|
| `CORE8666.ERB` | 414 | 子悪魔キャラ専用移動ロジック |
| `COMF421.ERB` - `COMF428.ERB` | 0 | 空スタブ（将来拡張用） |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=Extensions"
dotnet test Era.Core.Tests --filter "Category=FairyMaid"
```

**Success Criteria**:
- [ ] 拡張モジュール基盤確立
- [ ] 妖精メイドサブシステム移行完了
- [ ] 全テスト PASS
- [ ] **CP-3 E2E PASS**: Phase 20-27 全サブシステム統合DI解決 + Cross-system連鎖フロー検証

**E2E Checkpoint CP-3** (Post-Phase Review 必須検証項目):

> **参照**: [architecture.md Incremental E2E Test Strategy](../full-csharp-architecture.md#incremental-e2e-test-strategy)

| 検証項目 | 内容 | 期待結果 |
|----------|------|----------|
| DI全統合 | Phase 20-27 全サービス + 拡張モジュールをDIコンテナに登録 | 例外なく解決 |
| 訓練連鎖 | 訓練->能力成長->口上分岐 | 一連の連鎖が正しく伝播 |
| NTRフロー | NTR trigger->Mark進行->口上変化 | Phase 23-25 成果物が統合動作 |
| 拡張モジュール | 条件付き登録（有効/無効切替） | 無効時に影響なし |

**実装先**: `src/Era.Core.Tests/E2E/Phase27IntegrationTests.cs`

**Sub-Feature Requirements** (Planning Feature がこのセクションを読んで sub-feature に反映):

| 項目 | 要件 | 検証方法 |
|------|------|----------|
| **Philosophy** | 全 sub-feature に「Phase 27: Extensions」を継承 | Grep |
| **Tasks: 負債解消** | TODO/FIXME/HACK コメント削除タスクを含む | AC に not_contains |
| **Tasks: 等価性検証** | legacy 実装との等価性テストを含む | AC にテスト存在確認 |
| **AC: 負債ゼロ** | 技術負債ゼロを検証する AC を含む | AC 一覧確認 |
| **AC: CP-3 E2E** | Phase 20-27 全統合E2E検証を含む | E2E テスト PASS |

**Next**: Create Phase 28 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
>
> **Post-Phase Review 必須**: 本ドキュメントの該当 Phase セクションと実装の整合性を確認し、Success Criteria を更新、差異があれば本ドキュメントを修正すること。**残課題発生時は Redux Pattern 適用必須** (Feature Progression Protocol 参照)。
