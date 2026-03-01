# Phase 1-4: Foundation

**Parent**: [full-csharp-architecture.md](../full-csharp-architecture.md)

---

## Phase 1: Tools

**Phase Status**: DONE

**Goal**: ERB→YAML conversion pipeline

**Features**: F346-F353 (see [feature-345.md](../../feature-345.md) for breakdown)

| Feature | Component | Project/Location |
|:-------:|-----------|------------------|
| F346 | ERB Parser | `tools/ErbParser/` |
| F347 | TALENT Branching Extractor | `tools/ErbParser/` (extension) |
| F348 | YAML Schema Generator | `tools/YamlSchemaGen/` |
| F349 | DATALIST→YAML Converter | `tools/ErbToYaml/` |
| F350 | YAML Dialogue Renderer | `src/Era.Core/KojoEngine.cs` (**runtime**) |
| F351 | Pilot Conversion (美鈴 COM_0) | Integration test |
| F353 | CFLAG/Function Condition Extractor | `tools/ErbParser/` (extension) |

**Tool Projects** (Phase 1 deliverables):
| Project | Purpose |
|---------|---------|
| `tools/ErbParser/` | ERB→AST parsing + condition extraction (F346, F347, F353) |
| `tools/YamlSchemaGen/` | JSON Schema generation for YAML validation (F348) |
| `tools/ErbToYaml/` | AST→YAML conversion (F349) |
| `src/Era.Core/` | Runtime library - KojoEngine for dialogue rendering (F350) |

**Note**: F353 extends F347 to handle CFLAG references (e.g., `CFLAG:TARGET:好感度`) and function calls (e.g., `HAS_VAGINA()`, `GETBIT()`). Required for complete kojo conversion.

**Scope Boundaries** (tools created in later phases):
| Tool | Phase | Purpose |
|------|:-----:|---------|
| `tools/KojoComparer/` | Phase 2 | Automated ERB==YAML equivalence testing |
| `tools/CsvToYaml/` | Phase 11 | CSV→YAML data migration |
| `tools/ErbToYaml/` batch mode | Phase 12 | Batch kojo conversion (220+ files) |

**Verification** (Pilot - manual comparison for 4 representative variants):
```bash
# Pilot: 美鈴 COM_0 変換確認
# File: Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB (@KOJO_MESSAGE_COM_K1_0)
dotnet run --project tools/ErbToYaml -- "Game/ERB/口上/1_美鈴/KOJO_K1_愛撫.ERB" --function KOJO_MESSAGE_COM_K1_0
# Output: COM_0.yaml generated, 16 variants render, 4 representative manually verified
# Full automated comparison: Phase 2 (KojoComparer)
```

**Next**: Create Phase 1 Transition Features when implementation completes

> **Mandatory Transition Features** (separate from implementation features):
> - **Phase 1 Post-Phase Review** (infra): F346-F353 の品質検証
> - **Phase 2 Planning** (research): **Feature を立てる Feature** - Phase 2 sub-features 作成

---

## Phase 2: Test Infrastructure

<!-- Related Features: F358, F359, F360, F361 -->
**Related Features**: F358 (Planning), F359 (Test Structure), F360 (KojoComparer), F361 (Schema Validator)

**Phase Status**: DONE

**Goal**: テスト基盤の構築 + 既存テスト資産の移行

**Tasks**:
1. MSTest プロジェクト構成（Era.Core.Tests）
2. KojoComparer ツール（ERB出力 == YAML出力 比較）
3. Schema Validator 統合（F348出力を使用）
4. 既存テスト資産の移行
5. CI設定（GitHub Actions `dotnet test` 統合）
6. Headless モード維持・拡張

**Deliverables**:

| 成果物 | 用途 |
|--------|------|
| `src/Era.Core.Tests/` | C# 単体テスト |
| `tools/KojoComparer/` | ERB==YAML 等価性検証 |
| `schemas/kojo_schema.json` | YAML スキーマ検証 |
| `.github/workflows/test.yml` | CI 統合 |

**Test Layers**:

| Layer | Technology | Scope |
|-------|------------|-------|
| Unit | MSTest + Moq | Era.Core classes |
| Schema | JSON Schema | YAML validation |
| Integration | HeadlessUI | Full pipeline |
| Equivalence | KojoComparer | ERB == YAML output |

**Existing Test Migration**:

| Current | Count | Action |
|---------|:-----:|--------|
| `test/unit/*.erb` | 160+ | Convert to MSTest unit tests |
| `test/regression/*.erb` | 20+ | Convert to MSTest integration tests |
| `test/flow/*.erb` | 24 | Convert to C# integration tests |
| `test/kojo/*.erb` | ~50 | Migrate to KojoComparer |
| `src/engine.Tests/*.cs` | 12 | Preserve and extend |

**ERB Test Files in Game/ERB/** (Root level - preserve for validation):

| File | Lines | Purpose | Action |
|------|------:|---------|--------|
| `test-kojo-k1.ERB` | 109 | K1口上ユニットテスト (F045) | Reference for KojoComparer |
| `TEST_FLOW_RAND.ERB` | ~66 | ランダムフローテスト | Convert to C# |
| `TEST_FLOW_RAND_MULTI.ERB` | ~151 | マルチフローテスト | Convert to C# |
| `TEST_IMAGE.ERB` | ~2000 | 画像表示テスト | Unity integration test |
| `TEST_MOCK_RAND.ERB` | ~284 | モックランダムテスト | Convert to C# (MockRandom) |
| `TEST_MOCK_RAND_EXHAUST.ERB` | ~537 | 網羅テスト | Convert to C# |

**Headless Mode Preservation**:

| Component | Action |
|-----------|--------|
| `HeadlessRunner.cs` | Preserve API, extend for new tests |
| `ScenarioParser.cs` | Preserve scenario format compatibility |
| `StateInjector.cs` | Extend for new variable system |

**Verification**:
```bash
dotnet test Era.Core.Tests
# 100% pass on all unit tests
# Existing test coverage maintained
```

**Next**: Create Phase 3 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)

---

## Phase 3: System Infrastructure

<!-- Related Features: F363 -->
**Related Features**: F363 (Planning)

**Phase Status**: DONE

> **Phase 2 Status (2026-01-06)**: Complete (F359-F362 DONE)
> - MSTest structure established (BaseTestClass, TestHelpers, MockGameContext)
> - KojoComparer operational for ERB==YAML equivalence testing
> - Schema validation integrated (build-time + CI)
> - ERB test migration complete (100% high-priority tests → C# MSTest)
> - Test infrastructure ready to support Phase 3 migration

**Goal**: 全システムが依存する共通基盤の移行（CRITICAL - Phase 4以降の前提条件）

**Background**: SYSTEM.ERB, COMMON*.ERB, DIM.ERH は全ERBファイルから参照される基盤。これらなしに後続Phaseは成立しない。

**Tasks**:
1. SYSTEM.ERB 移行（ゲームメインループ）
2. COMMON.ERB 移行（共通関数・マクロ）
3. COMMON_J.ERB 移行（成功率計算）
4. COMMON_KOJO.ERB 移行（口上共通関数）
5. COMMON_PLACE.ERB 移行（場所・ロケーションシステム）
6. DIM.ERH 移行（変数定義ヘッダ）
7. ColorSettings.erh 移行（カラーパレット定義）
8. INFO.ERB 移行（情報表示システム）

**Source Analysis**:

| Current File | Lines | Purpose | Priority |
|--------------|:-----:|---------|:--------:|
| `SYSTEM.ERB` | 242 | ゲーム初期化ハンドラ（メインループはC#エンジン側） | **CRITICAL** |
| `COMMON.ERB` | ~660 | 共通関数、マクロ定義 | **CRITICAL** |
| `COMMON_J.ERB` | 75 | 成功率計算 (@GET_SUCCESS_RATE) | HIGH |
| `COMMON_KOJO.ERB` | ~50 | 口上システム共通関数 | HIGH |
| `COMMON_PLACE.ERB` | ~400 | 場所管理システム | HIGH |
| `DIM.ERH` | ~800 | 変数・配列宣言 | **CRITICAL** |
| `ColorSettings.erh` | ~100 | カラー定数 | Medium |
| `INFO.ERB` | ~600 | 情報表示 | Medium |

**Header Files (.ERH)** - All 7 files requiring special migration:

| File | Lines | Purpose | Priority |
|------|------:|---------|:--------:|
| `DIM.ERH` | 572 | グローバル変数宣言 | **CRITICAL** |
| `ColorSettings.erh` | 45 | カラー設定 | Medium |
| `続柄.ERH` | 71 | 続柄（関係性）定義 | HIGH |
| `NTR_MASTER_3P_SEX.ERH` | 7 | NTR 3P定義 | Medium |
| `グラフィック表示/グラフィック表示.ERH` | 28 | グラフィック設定 | Medium |
| `グラフィック表示/立ち絵表示.ERH` | 10 | 立ち絵設定 | Medium |
| `妖精メイド拡張/FairyMaids.erh` | 36 | 妖精メイド定義 | Medium |

**Feature Breakdown** (2026-01-06):

| Feature | Scope | Status |
|---------|-------|:------:|
| F363 | Phase 3 Planning | ✅ DONE |
| F364 | DIM.ERH → Constants.cs (572行) | ✅ DONE |
| F365 | SYSTEM.ERB → GameInitialization.cs (242行) | ✅ DONE |
| F366 | COMMON.ERB → CommonFunctions.cs (660行) | ✅ DONE |
| F367 | Options/Utilities (OPTION_2, VERSION_UP, ショートカット: 194行) | ✅ DONE |
| F368 | Character Setup (SHOP_CUSTOM, CHARA_CUSTUM, TALENTCOPY: 610行) | ✅ DONE |
| F369 | Clothing System (CLOTHES.ERB: 1,999行) | ✅ DONE |
| F370 | Body/State Systems (体設定, 妊娠処理, 天候: 3,168行) | ✅ DONE |
| F371 | NTR Initialization (NTR_TAKEOUT partial: ~50行) | ✅ DONE |
| F372 | Location System (移動関連関数: 230行) | ✅ DONE |
| F373 | InfoPrint Display Utilities | ✅ DONE |
| F374 | COMMON_J.ERB → SuccessRateCalculator.cs (48行) | ✅ DONE |
| F375 | COMMON_KOJO.ERB → KojoCommon.cs (49行) | ✅ DONE |
| F376 | Header Files Consolidation (.ERH Migration: 100 constants) | ✅ DONE |
| F378 | INFO.ERB Event Handling (InfoEvent.cs) | ✅ DONE |
| F379 | INFO.ERB Equipment Display (InfoEquip.cs) | ✅ DONE |
| F380 | INFO.ERB SHOW_STATUS Orchestration (StatusOrchestrator.cs) | ✅ DONE |
| F381 | INFO.ERB State Management (InfoState.cs) | ✅ DONE |
| F382 | INFO.ERB TrainMode Display (InfoTrainModeDisplay.cs) | ✅ DONE |

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/System/GameLoop.cs` | メインゲームループ |
| `src/Era.Core/Common/CommonFunctions.cs` | 共通関数 |
| `src/Era.Core/Common/Localization.cs` | ローカライズヘルパー |
| `src/Era.Core/Common/KojoCommon.cs` | 口上共通処理 |
| `src/Era.Core/Common/LocationSystem.cs` | 場所管理 |
| `src/Era.Core/Common/Constants.cs` | 定数定義 |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=SystemInfrastructure"
# All common functions match legacy behavior
```

**Next**: Create Phase 4 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)

---

## Phase 4: Architecture Refactoring (NEW)

**Phase Status**: DONE

**Goal**: Phase 3成果物のDI対応化 + Phase 5以降のパターン確立

**Background**: Phase 3は1:1移行でstatic classを作成。Phase 4で全static classをインターフェース抽出し、DI可能な設計に変換する。

**Scope**: Phase 3で作成した25ファイル（Era.Core配下）

> **⚠️ 設計原則（Phase 4の本質）**
>
> **やるべきこと（本質的問題の解決）**:
> - `static class` → インターフェース抽出 + インスタンスクラス化（DIP/ISP修正）
> - `int characterId` → `CharacterId` Strongly Typed ID（型安全）
> - 多パラメータメソッド → Parameter Object（凝集度向上）
>
> **やらないこと（YAGNI/KISS）**:
> - 行数が多いだけでの分割（607行でも単一責務なら分割不要）
> - switch文の機械的置換（将来拡張がなければそのまま）
> - 純粋定数クラスのインターフェース化（Constants.cs等は static のまま）

**Tasks**:
1. Strongly Typed IDs 導入（`CharacterId`, `LocationId`）※ `CommandId`, `ItemId` は Phase 8 に延期
2. Result型 導入（`Result<T>` 基盤クラス）
3. DI基盤構築（`Microsoft.Extensions.DependencyInjection`）
4. 19個のstatic classにインターフェース抽出（詳細は[F377](../../feature-377.md)参照）
5. Era.Core.Tests 新規作成（Architecture Tests）
6. engine.Tests DI対応更新（[F383](../../feature-383.md)）← 静的呼び出し→インスタンス呼び出し

**インターフェース抽出対象**（19クラス）:

| static class | Interface | 備考 |
|--------------|-----------|------|
| `GameInitialization.cs` | `IGameInitializer` | エントリポイント |
| `GameOptions.cs` | `IGameOptions` | 設定 |
| `CommonFunctions.cs` | `ICommonFunctions` | ユーティリティ |
| `LocationSystem.cs` | `ILocationService` | + LocationId適用 |
| `ClothingSystem.cs` | `IClothingSystem` | 607行だが単一責務 |
| `CharacterSetup.cs` | `ICharacterSetup` | キャラ設定 |
| `SuccessRateCalculator.cs` | `ISuccessRateCalculator` | + SuccessRateParams record |
| (他11クラス) | (詳細はF377) | - |

**純粋定数クラス**（インターフェース不要、staticのまま）:
- `Constants.cs`, `VariableDefinitions.cs`, `ColorSettings.cs`, `RelationshipTypes.cs`

**Strongly Typed IDs導入**:

| 対象 | 型名 | 用途 | Phase |
|------|------|------|:-----:|
| キャラクターID | `CharacterId` | `人物_美鈴` 等 | 4 ✅ |
| 場所ID | `LocationId` | `場所_正門` 等 | 4 ✅ |
| アイテムID | `ItemId` | アイテム参照 | 8 |
| コマンドID | `CommandId` | COM番号 | 8 |

**DI構成**:

```csharp
// src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEraCore(this IServiceCollection services)
    {
        services.AddSingleton<IGameState, GameState>();
        services.AddSingleton<ICharacterRepository, CharacterRepository>();
        services.AddTransient<INtrEngine, NtrEngine>();
        // ...
        return services;
    }
}
```

**Deliverables**:

| Component | Responsibility |
|-----------|----------------|
| `src/Era.Core/Types/CharacterId.cs` | Strongly Typed ID |
| `src/Era.Core/Types/LocationId.cs` | Strongly Typed ID |
| `src/Era.Core/Types/Result.cs` | Result型 |
| `src/Era.Core/Interfaces/IGameState.cs` | ゲーム状態インターフェース |
| `src/Era.Core/Interfaces/ICharacterRepository.cs` | キャラクターリポジトリ |
| `src/Era.Core/DependencyInjection/` | DI設定 |

**Verification**:
```bash
dotnet test Era.Core.Tests --filter "Category=Architecture"
# All refactored components maintain behavior
# DI container resolves all dependencies
# Strongly Typed IDs prevent misuse at compile time
```

**Success Criteria**:
- [x] 19個のstatic classがインターフェース抽出済み（F377 Task 5）
- [x] Strongly Typed IDs (`CharacterId`, `LocationId`) がコンパイル時型安全を保証（F377 Task 1）
- [x] DIコンテナが全依存関係を解決（F377 Task 4）
- [x] Era.Core.Tests Architecture Tests がパス（F377 Task 6）
- [ ] engine.Tests が全てパス（F383）← **未完了**
- [x] Phase 5以降が参照するパターンドキュメント完成（[F377](../../feature-377.md)）

**Next**: Create Phase 5 planning feature when this phase completes

> **Mandatory Transition Features**: See "Phase Progression Rules" - Review (infra) + Planning (research: Feature を立てる Feature)
