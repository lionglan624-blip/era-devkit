# Feature 026: ERB Structure Validator

## Status: [DONE]

## Overview

ErbLinter拡張: SELECTCASE/IFネスト構造の検証機能追加。構文エラーや不適切なネストを静的解析で検出する。

**実装ノート**: SyntaxAnalyzer.csに既存のブロック構造検証が実装済みだった。本Featureでは、テストプロジェクト追加とネスト深度警告機能（ERB004）の追加を実施。

## Problem

現在のErbLinterは関数定義・呼び出し・変数を解析できるが、制御構文（SELECTCASE/IF/REPEAT等）のネスト構造検証がない。不完全なネストや不正な構文はEmuera実行時まで発見できず、大規模ERBリファクタリング（Phase 5）の安全性が低下する。

## Goals

1. SELECTCASE/CASE/CASEELSE/ENDSELECT の整合性検証
2. IF/ELSEIF/ELSE/ENDIF の整合性検証
3. REPEAT/REND, FOR/NEXT, WHILE/WEND, DO-LOOP の整合性検証
4. ネスト深度の警告（設定可能な閾値超過時）
5. ErbLinterの既存アーキテクチャ（Visitor/Command）を活用

## Acceptance Criteria

- [x] SELECTCASE構文のネスト検証が動作する
- [x] IF構文のネスト検証が動作する
- [x] ループ構文（REPEAT/FOR/WHILE/DO）の検証が動作する
- [x] 不正なネストでエラーを報告する
- [x] デフォルトlintコマンドで自動実行（既存動作）
- [x] Build succeeds
- [x] Regression tests pass (uEmuera: 85, ErbLinter: 31)
- [x] Unit tests for structure validation added (31 tests)

## Scope

### In Scope
- 制御構文のスタックベース検証
- 開始/終了の対応チェック
- ネスト深度警告（オプション、デフォルト閾値10）
- 既存ErbLinter Commandパターンへの統合

### Out of Scope
- 意味解析（到達不能コード検出等）
- 実行時のパス解析
- SIF等の単一行条件文（構文上問題なし）

## Technical Approach

既存実装を発見・活用:
1. SyntaxAnalyzer.cs に Stack<BlockInfo> によるブロック追跡が既存
2. ERB001/ERB002/ERB003 エラーコードで報告
3. ERB004 を追加: ネスト深度警告 (NestingThreshold プロパティ)
4. ErbLinter.Tests プロジェクト追加

## Effort Estimate

- **Size**: Low (既存パーサー活用可能)
- **Risk**: 🟢Low (新機能追加、既存機能に影響なし)
- **Testability**: ★★★☆☆ (構造エラーのあるテストファイルで検証可能)
- **Estimate**: 1セッション
- **Actual**: 1セッション

## Dependencies

- ErbLinter基盤 (Feature 003, 004)
- ERBパーサー既存実装

## Links

- [index-features.md](index-features.md) - Feature tracking
- [WBS-026.md](WBS-026.md) - Work breakdown
- [feature-024.md](feature-024.md) - Function Call Graph (同じPhase 3)
- [feature-025.md](feature-025.md) - Change Impact Analyzer (同じPhase 3)
