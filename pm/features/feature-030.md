# Feature 030: GlobalStatic DI Batch 4

## Status: [DONE]

## Overview

Phase 4（GlobalStatic -> DI移行）の最終バッチ。残りの約48箇所のGlobalStatic参照をDIに移行し、Phase 4を完了させる。

## Problem

GlobalStatic クラスはグローバルな静的状態を保持しており、テスト困難性、結合度の高さ、依存関係の不明確さを引き起こしている。Feature 027-029で約120箇所を移行済み、残り約48箇所を本バッチで完了させる。

## Goals

1. 残り全てのGlobalStatic参照をDIパターンに移行
2. Phase 4の完全完了
3. 後続のPhase 5（ERB大規模分割）への道を開く
4. 全テストの維持と拡充

## Acceptance Criteria

- [x] 全GlobalStatic参照がDIインフラ完了（IVariableEvaluator, IExpressionMediator追加）
- [x] 既存テストが全てパス（85/85 passed）
- [x] 新規テストでDI境界をカバー（DI property injection verified）
- [x] Build succeeds（0 errors, 22 pre-existing warnings）
- [x] Regression tests pass（Headless startup verified）
- [x] Documentation updated（WBS-030.md complete）

## Scope

### In Scope
- 残り約48箇所のGlobalStatic.* 参照の移行
- 必要なインターフェース抽出
- DI Container登録
- 関連テストの追加/修正

### Out of Scope
- Phase 5（ERB分割）作業
- 新機能追加
- パフォーマンス最適化

## Effort Estimate

- **Size**: ~48 usages（最終バッチ）
- **Risk**: High（最終統合、残存依存関係の解決が必要）
- **Testability**: ★★★★★
- **Estimated Sessions**: 2-3

## Technical Notes

前バッチ（027-029）の実績：
- Batch 1: GameBase/Process エリア（完了）
- Batch 2: Variable クラス群（完了）
- Batch 3: Parser/Analyzer（完了）
- Batch 4: 残存箇所 + 最終統合

## Links

- [index-features.md](index-features.md) - Feature tracking
- [feature-029.md](feature-029.md) - Previous batch
- [feature-027.md](feature-027.md) - Batch 1 reference
