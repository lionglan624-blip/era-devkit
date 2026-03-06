# Feature 031: NTR_SEX family refactor

## Status: [DONE]

## Overview

NTR.ERB内のNTR_SEX_0~NTR_SEX_6関数群（7関数、~447行）の統合リファクタリング。Phase 5の最初の練習用Featureとして、後続の大規模分割（032-034）に向けた手法確立を目的とする。

## Problem

- NTR_SEX_0~6は90%以上同一パターンの重複コード（各関数60-70行）
- 差異はSOURCE値（快Ｖ、情愛、苦痛、露出、反感、逸脱等）とFLAG値のみ
- 修正時に7箇所同時変更が必要で保守性が低い

## Goals

1. NTR_SEX_0~6の共通パターンを1つの共通関数に統合
2. ポジション固有のパラメータをデータ駆動で管理
3. Phase 5リファクタリング手法の確立（後続Feature 032-034の参考）
4. 既存動作の完全維持（同一の出力・状態変更）

## Acceptance Criteria

- [x] NTR_SEX_0~6 の現状分析完了
- [x] リファクタリング計画策定
- [x] コード統合・整理実施
- [x] Build succeeds
- [x] Regression tests pass (Unit tests 85/85)
- [x] Headless test pass

## Scope

### In Scope
- NTR.ERB内のNTR_SEX_0~6関数（lines 2415-2886）のリファクタリング
- 共通関数@NTR_SEX_COMMONの作成
- ポジション設定テーブルの作成
- 呼び出し元の更新不要（既存関数名を維持）

### Out of Scope
- NTR_A_SEX系関数（アナル系、別タスク）
- 他のNTR関連関数の変更
- ゲームロジックの変更
- 新機能追加

## Effort Estimate

- **Size**: ~500 lines (7ファイル)
- **Risk**: Medium
- **Testability**: ★★★ (Training Mode + manual verification)
- **Sessions**: 1-2

## Technical Notes

Phase 5リファクタリングの順序:
1. **031** (this): NTR_SEX family - 練習用、小規模
2. 032: WC_SexHara_MESSAGE split - 8,557行、High risk
3. 033: TOILET_COUNTER_MESSAGE split - 10,953行、High risk
4. 034: NTR.ERB split - 4,898行、VHigh risk、61 dependencies

## Links

- [index-features.md](index-features.md) - Feature tracking
- [index-features-history.md](index-features-history.md) - Full history
- [content-roadmap.md](content-roadmap.md) - Content/gameplay roadmap
