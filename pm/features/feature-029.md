# Feature 029: GlobalStatic DI Batch 3

## Status: [DONE]

## Overview

GlobalStatic の残り使用箇所を Dependency Injection に移行するバッチ処理の第3弾。Parser/Analyzer エリアを中心に DI インフラストラクチャを追加。

Phase 4 (GlobalStatic -> DI Phase 2) の3番目のFeature。

## Problem

- GlobalStatic に依存した static アクセスパターンが残存
- テストでのモック化が困難
- コンポーネント間の依存関係が暗黙的
- Parser/Analyzer 周りのコードがグローバル状態に強く結合

## Goals

1. Parser/Analyzer エリアの GlobalStatic 使用箇所に対する DI インフラ追加
2. IIdentifierDictionary インターフェースの拡張 (10メソッド追加)
3. ILabelDictionary, IConstantData インターフェースの新規作成
4. Feature 027/028 で確立したパターンを踏襲

## Acceptance Criteria

- [x] IIdentifierDictionary に 10 メソッド追加
- [x] ILabelDictionary インターフェース作成 (2 methods + 1 property)
- [x] IConstantData インターフェース作成 (1 method)
- [x] GlobalStatic に DI プロパティ追加 (LabelDictionaryInstance, ConstantDataInstance)
- [x] Build succeeds (dotnet build) - 0 errors
- [x] 全ユニットテスト合格 (85/85 passed)
- [x] ヘッドレステスト合格 (game loads to title screen)

## Scope

### In Scope
- IIdentifierDictionary インターフェース拡張
- ILabelDictionary, IConstantData 新規インターフェース作成
- GlobalStatic への DI プロパティ追加
- 既存クラスへのインターフェース実装

### Out of Scope
- 新規機能追加
- パフォーマンス最適化
- 他エリア (Feature 030 対象) の移行

## Technical Notes

### New Interfaces
- `ILabelDictionary`: Initialized, GetNonEventLabel, GetLabelDollar
- `IConstantData`: isDefined

### Extended Interface
- `IIdentifierDictionary`: Added 10 methods for Parser/Analyzer operations
  - CheckUserLabelName, CheckUserVarName, CheckUserPrivateVarName
  - GetFunctionIdentifier, GetRefMethod, GetFunctionMethod
  - ThrowException, resizeLocalVars, getLocalDefaultSize, getLocalIsForbid

### Files Changed
- `uEmuera/Assets/Scripts/Emuera/Sub/ILabelDictionary.cs` (new)
- `uEmuera/Assets/Scripts/Emuera/Sub/IConstantData.cs` (new)
- `uEmuera/Assets/Scripts/Emuera/Sub/IIdentifierDictionary.cs` (extended)
- `uEmuera/Assets/Scripts/Emuera/GlobalStatic.cs` (DI properties)
- `uEmuera/Assets/Scripts/Emuera/GameProc/LabelDictionary.cs` (implements)
- `uEmuera/Assets/Scripts/Emuera/GameData/ConstantData.cs` (implements)

## Effort Estimate

- **Size**: 13 interface methods (Medium)
- **Risk**: Low - Interface additions only, no behavior change
- **Testability**: ★★★★★ (5/5) - 既存テスト基盤あり
- **Sessions**: 1 session

## Links

- [WBS-029.md](WBS-029.md) - Work breakdown
- [index-features.md](index-features.md) - Feature tracking
- [index-features-history.md](index-features-history.md) - Full history
- [feature-027.md](feature-027.md) - Batch 1 (参考)
- [feature-028.md](feature-028.md) - Batch 2 (参考)
