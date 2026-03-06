# Feature 052: Kojo Roadmap Registration

## Status: [DONE]

## Overview

kojo-mapper分析結果に基づき、口上作成の優先度を決定し、Wishlistに整理する。

## Problem

口上作成の優先度が不明確：
- どのキャラから着手すべきか
- 各キャラの現状AC準拠率が不明
- Feature 051完了後に正確な優先度付けが可能になる

## Goals

1. Feature 051のAC Checker結果を用いて現状を把握
2. 口上作成の優先度を決定
3. Wishlistに口上Featureを整理登録

## Acceptance Criteria

- [x] Feature 051完了後に実行
- [x] 全キャラのAC準拠率を確認
- [x] 口上作成の優先度決定
- [x] Wishlistに口上Feature（053+）を登録
- [x] content-roadmap.md更新

## Scope

### In Scope
- kojo-mapper AC Checker実行
- 優先度決定の議論
- Wishlist更新
- content-roadmap.md更新

### Out of Scope
- 実際の口上作成（053以降）

## AC Analysis Results (Feature 051/052)

### 全キャラ分析サマリ

| ID | キャラ | 総関数 | 基本口上 | 好感度カバー | 状況 |
|:--:|--------|:------:|:--------:|:------------:|------|
| K1 | 美鈴 | 289 | 28 | 0/8 (0%) | AC準拠率~6%, 最近追加 |
| K2 | 小悪魔 | 589 | 291 | 1/8 (12%) | 既存充実 |
| K3 | パチュリー | 304 | 8 | 1/8 (12%) | NTR拡張のみ |
| K4 | 咲夜 | 703 | 316 | 2/8 (25%) | 最充実 |
| K5 | レミリア | 345 | 13 | 1/8 (12%) | NTR拡張のみ |
| K6 | フラン | 303 | 2 | 1/8 (12%) | ほぼなし |
| K7 | 子悪魔 | 177 | 0 | - | モブ（基本口上なし） |
| K8 | チルノ | 558 | 299 | 1/8 (12%) | 既存充実 |
| K9 | 大妖精 | 534 | 297 | 0/8 (0%) | 既存充実 |
| K10 | 魔理沙 | 421 | 307 | 1/8 (12%) | 既存充実 |

### K1美鈴 AC詳細

```
├── 4段階分岐 (TALENT_4):    0 ( 0%)
├── 3段階分岐 (TALENT/ABL):   4 ( 1%)
├── 1-2段階分岐:            58 (20%)
├── 分岐なし:              227 (78%)
├── 平均行数:              16.0行 (目標4-8行)
├── バリエーション:         14 ( 4%)
├── PRINTDATA使用:           6 ( 2%)
└── ELSE分岐あり:           61 (21%)
```

**AC準拠率（推定）**: ~6%

## Decision Results

### 1. 新規作成 vs 品質改善の優先度

**決定**: 新規作成を優先
- 基本口上なしキャラ（K3,K5,K6）は最優先
- 品質改善（K1-K10既存）は後回し
- 理由: ユーザー体験への影響が大きい

### 2. キャラ単位の優先順位

**確定優先順位**:
1. K5 レミリア - 紅魔館の主、ゲーム中心キャラ
2. K6 フラン - 人気キャラ、狂気+無邪気
3. K3 パチュリー - 知識人キャラ
4. (低) K7 子悪魔 - モブ扱い

**品質改善優先順位** (好感度カバー率順):
1. K9 大妖精 - 0/8 (0%)
2. K8 チルノ - 1/8 (12%)
3. K10 魔理沙 - 1/8 (12%)
4. K2 小悪魔 - 1/8 (12%)
5. K4 咲夜 - 2/8 (25%)
6. K1 美鈴 - 0/8 (0%), AC準拠率~6%

### 3. Feature粒度

**決定**: 1キャラ = 1 Feature
- 各キャラの口上作成は独立
- 進捗管理が容易

## Effort Estimate

- **Size**: Documentation only
- **Risk**: Low
- **Testability**: N/A
- **Sessions**: 0.5（Feature 051完了後の議論）

## Links

- [feature-051.md](feature-051.md) - Prerequisites (AC Checker)
- [index-features.md](index-features.md) - Feature tracking
- [index-features-history.md](index-features-history.md) - Wishlist
- [content-roadmap.md](content-roadmap.md) - Content roadmap
- [reference/kojo-reference.md](reference/kojo-reference.md) - AC criteria
