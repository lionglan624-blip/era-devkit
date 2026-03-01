# Feature 027: GlobalStatic DI Batch 1

## Status: [DONE]

## Overview

GlobalStatic -> DI Phase 2の最初のバッチ。GameBase/Process関連の約40箇所のGlobalStatic使用箇所をDIパターンに移行する。

## Problem

Feature 019で開始したGlobalStatic -> DI移行は、Phase 1で主要な基盤を構築したが、168箇所の使用箇所が残っている。これらの静的依存関係は：
- テスト容易性を低下させる
- コンポーネント間の結合度を高める
- 依存関係の可視性を損なう

## Goals

1. GameBase/Process領域の約40箇所のGlobalStatic使用を解消
2. 既存テストのパス維持
3. DIパターンの一貫した適用

## Acceptance Criteria

- [x] GameBase関連のGlobalStatic DI infrastructure追加 (IEmueraConsole, IMainWindow)
- [x] Process関連のGlobalStatic DI infrastructure追加 (ConsoleInstance, MainWindowInstance)
- [x] 全ての既存ユニットテストがパス (85/85)
- [x] ビルドが成功
- [x] リグレッションテストがパス (Headless smoke test)

## Scope

### In Scope
- GameBase関連クラスのGlobalStatic使用箇所の移行
- Process関連クラスのGlobalStatic使用箇所の移行
- 必要なコンストラクタインジェクションの追加
- ServiceCollection登録の更新

### Out of Scope
- Variable classes (Batch 2)
- Parser/Analyzer (Batch 3)
- その他の領域 (Batch 4)

## Effort Estimate

- **Size**: ~40 usages
- **Risk**: Medium
- **Testability**: ★★★★★ (5 stars)
- **Sessions**: 1-2

## Technical Notes

Phase 1 (Feature 019) で確立されたパターン:
- コンストラクタインジェクション
- ServiceCollection登録
- nullable警告対応

## Links

- [index-features.md](../index-features.md) - Feature tracking
- [index-features-history.md](../index-features-history.md) - Full history
- [engine-reference.md](../reference/engine-reference.md) - Engine architecture
