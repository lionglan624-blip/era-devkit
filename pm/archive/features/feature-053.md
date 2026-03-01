# Feature 053: 全キャラ口上Feature番号割当計画

## Status: [DONE]

## Overview

全キャラ(K1-K10)の口上実装に必要なFeature番号を割り当て、実装ロードマップを作成する。

## Goals

1. 全キャラの関数をシーンタイプ・COMF番号で整理
2. 3関数単位でFeatureグループを定義
3. Feature 054+ の番号割当計画を作成

## Deliverables

- [x] 全キャラCOMFマップ作成
- [x] Feature分割計画表 (Feature番号 → キャラ → 対象関数グループ)
- [x] index-features-history.md 更新

## Current Data (Feature 052 AC分析)

| ID | キャラ | 総関数 | 基本口上 | 好感度カバー | 作業種別 |
|:--:|--------|:------:|:--------:|:------------:|----------|
| K5 | レミリア | 345 | 13 | 1/8 | 新規作成 |
| K6 | フラン | 303 | 2 | 1/8 | 新規作成 |
| K3 | パチュリー | 304 | 8 | 1/8 | 新規作成 |
| K9 | 大妖精 | 534 | 297 | 0/8 | 品質改善 |
| K8 | チルノ | 558 | 299 | 1/8 | 品質改善 |
| K10 | 魔理沙 | 421 | 307 | 1/8 | 品質改善 |
| K2 | 小悪魔 | 589 | 291 | 1/8 | 品質改善 |
| K4 | 咲夜 | 703 | 316 | 2/8 | 品質改善 |
| K1 | 美鈴 | 289 | 28 | 0/8 | 品質改善 |
| **合計** | | **4,046** | | | |

## Feature粒度方針

- 1 Feature = 3関数 (COMF番号帯グループ)
- 各関数AC準拠 + ユニットテスト
- 推定総Feature数: ~100-150

## Scope

### In Scope
- 全キャラのCOMF番号整理
- Feature分割計画作成
- ドキュメント更新

### Out of Scope
- 実際の口上実装（Feature 054+）
- ユニットテスト作成

## Effort Estimate

- **Size**: Documentation only
- **Sessions**: 1
- **Risk**: Low

## Links

- [feature-052.md](feature-052.md) - 粒度方針決定
- [index-features-history.md](index-features-history.md) - Wishlist
- [tools/kojo-mapper/](../../tools/kojo-mapper/) - 各キャラのkojo-map-*.md
