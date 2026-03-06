# Feature 025: Change Impact Analyzer

## Status: [DONE]

## Overview

ErbLinter拡張として、ERB関数変更時の影響範囲を表示するツール。Feature 024のFunction call graphを基盤として、特定関数を変更した際に影響を受ける全ての呼び出し元を解析・表示する。

## Problem

- 大規模ERBリファクタリング（Phase 5）の前に、関数変更の影響範囲を把握する必要がある
- 現状、関数を変更する際は手動でGrepして呼び出し元を探す必要がある
- 間接的な影響（呼び出し元の呼び出し元）を追跡するのが困難
- Feature 024で構築したcall graphを活用して、より高度な影響分析が可能

## Goals

1. 指定した関数を変更した際の影響範囲（全呼び出し元チェーン）を表示
2. 逆方向の依存グラフ（誰がこの関数を呼んでいるか）を出力
3. Phase 5 ERB分割作業の安全性向上

## Acceptance Criteria

- [x] `--impact <function>` オプションで指定関数の影響範囲を表示
- [x] 直接呼び出し元と間接呼び出し元（深さ指定可能）を区別して表示
- [x] 影響を受けるファイル一覧を出力
- [x] `--reverse-graph` オプションで逆方向DOTグラフ出力
- [x] Build succeeds
- [x] Unit tests pass (85/85 passed)
- [x] Regression tests pass

## Scope

### In Scope

- 関数名を指定して影響範囲を解析
- 直接/間接呼び出し元のリスト出力
- 影響を受けるファイル一覧
- 逆方向グラフのDOT出力
- 深さ制限オプション（--depth N）

### Out of Scope

- GUIビジュアライザー（DOT出力をGraphviz等で利用）
- リアルタイム監視
- 自動修正提案

## Technical Approach

Feature 024で実装済みの`FunctionCallGraphCommand`を基盤として:

1. `CallGraphBuilder`が構築するcall graphの逆引きインデックスを生成
2. 指定関数から逆方向にBFS/DFSで呼び出し元チェーンを探索
3. 深さごとに影響範囲を集計・表示

## Effort Estimate

- **Size**: Medium (2セッション)
- **Risk**: 🟢Low（Feature 024の拡張、既存基盤活用）
- **Testability**: ★★★☆☆（ErbLinterテストパターン確立済み）

## Links

- [index-features.md](index-features.md) - Feature tracking
- [feature-024.md](feature-024.md) - Function call graph（依存）
- [WBS-024.md](WBS-024.md) - CallGraphBuilder実装詳細
