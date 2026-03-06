# Feature 049: 画像表示機能検証 (Spike)

## Status: [DONE]

## Overview

uEmuera の画像表示機能（HTML_PRINT, PRINT_IMG, G系命令）の対応状況を検証し、立ち絵システム実装方針を決定する。

## Problem

eraTW の立ち絵システムは HTML_PRINT + G系命令を使用しているが、uEmuera での対応状況が不明。実装方針を決定する前に検証が必要。

## Goals

1. uEmuera の画像表示関連命令の対応状況を確認
2. eraTW のグラフィック表示ライブラリが動作するか検証
3. Feature 050 の実装方針を決定（ERBライブラリ方式 or Unity拡張）

## Acceptance Criteria

- [x] HTML_PRINT の画像表示対応を確認 → 定義済、描画未実装
- [x] PRINT_IMG 命令の対応を確認 → 定義済、描画未実装
- [x] G系命令（GCREATE, GDRAWSPRITE等）の対応を確認 → 状態管理のみ動作、描画コメントアウト
- [x] eraTW ライブラリの移植可否を判定 → 不可（描画未実装のため）
- [x] 検証結果をドキュメント化 → WBS-049.md に詳細記載
- [x] Feature 050 の実装方針を決定 → **方針B（限定的ERB + Unity補完）** 推奨

## Scope

### In Scope
- uEmuera ソースコードの画像関連命令調査
- 最小限の動作検証（テストERB作成）
- 検証結果の文書化

### Out of Scope
- 完全な立ち絵システム実装（Feature 050）
- 立ち絵素材の準備
- パフォーマンス最適化

## Technical Considerations

### 検証対象命令

```erb
; HTML系
HTML_PRINT "<img src='resource'>"

; 画像直接表示
PRINT_IMG resource

; グラフィック操作系
GCREATE gid, width, height
GDRAWSPRITE gid, resource, x, y, w, h
GSETCOLOR gid, color_matrix
GDISPOSE gid
SPRITECREATE resource, filename
```

### 検証手順

1. uEmuera ソースで各命令の実装状況を確認
2. 対応している命令でテストERBを作成
3. 実際に動作確認
4. eraTW のライブラリ移植を試行

### 判定基準

| 結果 | 方針 |
|------|------|
| HTML_PRINT + G系命令が動作 | ERBライブラリ方式（Feature 050A） |
| 一部のみ動作 | 限定的ERB + Unity補完（Feature 050B） |
| 非対応 | Unity拡張方式（Feature 050C） |

## Effort Estimate

- **Size**: Small (調査・検証のみ)
- **Risk**: Low
- **Duration**: 1-2 days

## Dependencies

- Phase 6 (043) 完了 ✅

## Links

- [feature-044.md](feature-044.md) - 設計概要（eraTW調査結果）
- [feature-050.md](feature-050.md) - 実装Feature（本検証の結果で方針決定）
- [engine-reference.md](../reference/engine-reference.md) - Engine architecture

## Reference

- **eraTW** - 検証対象のリファレンス実装
  - `ERB/グラフィック表示ライブラリ/グラフィック表示.ERB`
