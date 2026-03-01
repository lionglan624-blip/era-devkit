# Feature 024: Function Call Graph

## Status: [DONE]

## Overview

Feature 004 (Dead Code Detection) で構築したERB関数解析基盤を拡張し、関数間の呼び出し関係をDOT形式やインタラクティブな形式で可視化する。これにより、コードベースの構造理解とリファクタリング計画が容易になる。

## Problem

1. **構造把握の困難**: 800+関数、2万行超のERBコードベースで、関数間の依存関係を把握するのが難しい
2. **リファクタリングリスク**: 関数変更時の影響範囲が不明確
3. **デッドコード検出の限界**: Feature 004は未使用関数を検出するが、依存の「深さ」や「方向」は可視化していない

## Goals

1. ERB関数の呼び出しグラフをDOT形式で出力
2. オプションでフィルタリング機能（ファイル別、関数別、深さ制限）を提供
3. 既存のErbLinter解析基盤を再利用し、実装を最小化

## Acceptance Criteria

- [x] `dotnet run --project tools/ErbLinter -- callgraph Game/ERB` で呼び出しグラフをDOT出力
- [x] 出力DOTファイルがGraphvizで正常にレンダリング可能
- [x] `--root <function>` オプションで特定関数を起点としたサブグラフ出力
- [x] `--depth <N>` オプションで深さ制限
- [x] Build succeeds
- [x] ErbLinter builds and runs correctly
- [x] CLI integration tests pass

## Scope

### In Scope
- ErbLinterへの`callgraph`サブコマンド追加
- DOT形式出力（Graphviz互換）
- 関数名/ファイル名によるフィルタリング
- 深さ制限オプション

### Out of Scope
- インタラクティブUI（将来拡張として検討）
- リアルタイム更新
- 変数依存の追跡（関数呼び出しのみ）

## Technical Approach

1. **既存基盤の活用**: ErbLinter.Analyzer の関数定義/呼び出し解析を再利用
2. **グラフ構築**: Dictionary<string, HashSet<string>> で呼び出し関係を構築
3. **DOT出力**: シンプルなテキスト出力（digraph形式）
4. **フィルタリング**: BFS/DFSで指定ノードからの到達可能ノードを抽出

## Effort Estimate

- **Size**: Medium (2-3 sessions)
- **Risk**: Low - 既存解析基盤の拡張
- **Testability**: ★★★☆☆

## Links

- [index-features.md](index-features.md) - Feature tracking
- [feature-004.md](feature-004.md) - Dead Code Detection (基盤)
- [tools/ErbLinter](../../tools/ErbLinter) - ErbLinter source
