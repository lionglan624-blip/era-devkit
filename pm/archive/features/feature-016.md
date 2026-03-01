# Feature 016: LexicalAnalyzer Split

## Status: [DONE]

## Overview

LexicalAnalyzer.cs（1,301行）を責務ごとに分割し、テスト可能性と保守性を向上させる。TokenReader、ExpressionAnalyzer、MacroExpanderの3つのクラスに分離。

## Problem

現状の `LexicalAnalyzer.cs`:
- 1,301行の大規模クラス
- 複数の責務が混在（トークン読み取り、式解析、マクロ展開）
- 個別機能のユニットテストが困難
- 変更時の影響範囲が広い

## Goals

1. **責務分離**: 単一責任原則に従った3クラスに分割
2. **テスト可能性**: 各クラスを独立してテスト可能に
3. **保守性向上**: 変更影響範囲の限定化
4. **後方互換性**: 既存の呼び出し元に影響を与えない

## Proposed Structure

```
LexicalAnalyzer.cs (1,301行)
    ↓ 分割
├── TokenReader.cs        - トークン読み取り（数値、識別子、文字列、演算子）
│   - ReadInt64, ReadDouble
│   - ReadFirstIdentifierWord, ReadSingleIdentifierWord, ReadSingleIdentifier
│   - ReadString, ReadOperator, ReadAssignmentOperator
│   - SkipAllSpace, SkipWhiteSpace, SkipHalfSpace, IsWhiteSpace
│
├── ExpressionAnalyzer.cs - 式の解析
│   - Analyse (main entry)
│   - AnalyseFormattedString
│   - AnalyseYenAt
│
└── MacroExpander.cs      - マクロ展開
    - expandMacro
    - expandFunctionlikeMacro
```

## Acceptance Criteria

- [x] TokenReader.cs 作成
- [x] ExpressionAnalyzer.cs 作成
- [x] MacroExpander.cs 作成
- [x] LexicalAnalyzer.cs がファサードとして機能
- [x] 既存の呼び出し元が変更不要
- [x] Build succeeds
- [x] Regression tests pass
- [x] Unit tests for each new class

## Scope

### In Scope
- LexicalAnalyzer.cs の3クラス分割
- 各クラスへのインターフェース追加（テスト用）
- 単体テスト作成

### Out of Scope
- 他のパーサー関連クラスの変更
- パフォーマンス最適化（別Feature）

## Effort Estimate

- **Size**: Medium (1,301行)
- **Risk**: 🟡 Medium（パーサーは広範囲で使用）
- **Testability**: ★★★★☆
- **Files**: 3 create, 1 modify

## Dependencies

- Feature 012-015 (DI基盤) - 参照パターン

## Links

- [LexicalAnalyzer.cs](../../../uEmuera/Assets/Scripts/Emuera/Sub/LexicalAnalyzer.cs)
- [index-features.md](../index-features.md) - Feature tracking
- [engine-reference.md](../reference/engine-reference.md) - Architecture docs
