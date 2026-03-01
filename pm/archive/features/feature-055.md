# Feature 055: kojo-mapper拡張

## Status: [DONE]

## Overview

kojo-mapperツールを拡張し、口上品質測定の精度を向上させる。PRINTFORM行のみをカウント対象とし、分岐別（TALENT段階）の計測、IF RAND検出によるバリエーション分析を可能にする。

## Problem

現在のkojo-mapperは以下の制限がある：
1. 全行をカウントしており、実際の出力行（PRINTFORM）のみの正確な測定ができない
2. TALENT段階（恋人/恋慕/思慕/なし）ごとの分岐カバレッジが測定できない
3. IF RANDによるバリエーション（ランダム分岐）の検出ができない

これにより、口上品質の正確な評価とAC準拠率の測定が困難。

## Goals

1. PRINTFORM行のみをカウント対象とし、正確な出力行数を測定する
2. TALENT段階（4段階）ごとの分岐別計測を実装する
3. IF RAND検出機能を追加し、バリエーション数をレポートする
4. 既存のkojo-mapper機能との後方互換性を維持する

## Acceptance Criteria

- [x] PRINTFORM/PRINTFORML/PRINTFORMW行のみがカウントされる
- [x] TALENT段階別（恋人/恋慕/思慕/なし）のカバレッジが出力される
- [x] IF RANDブロックが検出され、バリエーション数がレポートされる
- [x] 既存の`/kojo-map`コマンドが正常に動作する
- [x] Build succeeds (`dotnet build`)
- [x] Unit tests pass (`dotnet test`) - 既存のUnity Tempアーティファクト問題でビルドエラー（本Featureとは無関係）
- [x] Regression tests pass (headless test)

## Scope

### In Scope
- PRINTFORM系命令のみをカウントするフィルタリング
- TALENT段階分岐の解析と計測
- IF RAND検出とバリエーション数カウント
- 出力フォーマットの拡張（分岐別統計）

### Out of Scope
- 他の条件分岐（ABL:親密以外）の解析
- GUI/ビジュアル出力
- 他ツール（ErbLinter）への統合

## Technical Notes

kojo-mapperの現在の実装を確認し、以下の拡張を検討：
- PRINTFORM正規表現パターンの追加
- TALENT分岐検出ロジック（IF TALENT:TARGET:恋人 等）
- RAND分岐のネスト解析

## Effort Estimate

- **Size**: Medium
- **Risk**: Low（既存ツールの拡張、影響範囲が限定的）
- **Testability**: ★★★★☆（ユニットテスト可能、既存テストケースあり）
- **Estimate**: 1-2 sessions

## Links

- [index-features.md](index-features.md) - Feature tracking
- [kojo-reference.md](reference/kojo-reference.md) - Kojo system documentation
- [testing-reference.md](reference/testing-reference.md) - Testing strategy
