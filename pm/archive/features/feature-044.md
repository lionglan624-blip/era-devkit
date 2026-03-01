# Feature 044: 立ち絵表示システム（設計文書）

## Status: [ARCHIVED]

> **Note**: この Feature は設計調査文書としてアーカイブ。実装は以下に分割:
> - [Feature 049](feature-049.md): 画像表示機能検証 (Spike)
> - [Feature 050](feature-050.md): 立ち絵基盤実装

## Overview

立ち絵（キャラクター画像）表示システムの設計概要。eraTW の実装調査結果を含む。

## Problem

現在のゲームはテキストのみで進行し、視覚的なキャラクター表現がない。立ち絵表示により、没入感とユーザー体験が大幅に向上する。

## Implementation Features

| Feature | 内容 | Status |
|---------|------|--------|
| [049](feature-049.md) | 画像表示機能検証 (Spike) | [PROPOSED] |
| [050](feature-050.md) | 立ち絵基盤実装 | [PROPOSED] |

## Technical Considerations

### eraTW 実装調査結果

eraTW の立ち絵システムを調査した結果、以下の実装パターンが判明:

#### 1. CSVベースリソース管理

リソースIDとスプライトシート座標をCSVで定義:

```csv
; resources/立ち絵.csv
リソースID,ファイル名,SourceX,SourceY,Width,Height
紅魔館_霊夢_通常_17,17.png,0,0,180,180
紅魔館_霊夢_悲観_17,17.png,180,0,180,180
紅魔館_霊夢_本気_17,17.png,360,0,180,180
```

**メリット**: 1枚のスプライトシートに複数ポーズを格納、メモリ効率向上

#### 2. レイヤー構成

eraTWは3層構成:
1. **Body Layer** (`立ち絵.csv`) - ポーズ: 通常/悲観/本気
2. **Face Layer** (`顔.csv`) - 表情差分
3. **Accessory Layer** (`kaoru_set.csv`) - 装飾、エモーション

```
顔G_紅_通常_17,17_顔.png,0,0,180,180    ; Face overlay
CN00_他_装飾_涙,kaoru_set.png,856,1,30,11  ; Accessory
```

#### 3. HTML_PRINT による表示

Emuera組み込みのHTML_PRINT命令を使用:

```erb
@画像表示(画像ID, 表示位置, 縦位置, 横位置, 拡大率)
  ; HTML生成
  TEMP_HTML = "<img src='%画像ID%' height='%高さ%' width='%幅%' ypos='%位置%'>"
  HTML_PRINT TEMP_HTML
```

**重要**: 新規命令追加不要、既存のHTML_PRINTで実現可能

#### 4. カラーマトリックス（色変更）

5x5行列で肌色・髪色等を動的変更:

```erb
; 18種類のプリセット色定義
カラーマトリックス_ベース:0 = 赤 (R=256,G=0,B=0)
カラーマトリックス_ベース:6 = グレー (128,128,128)

GDRAWSPRITE gid, resource, x, y, w, h, カラーマトリックス
```

#### 5. グラフィック表示ライブラリ構成

```
ERB/グラフィック表示ライブラリ/
├── グラフィック表示.ERB   ; 表示ロジック
├── グラフィック表示.ERH   ; 変数定義
└── グラフィック合成.ERB   ; 合成処理
```

**主要関数**:
- `@画像表示(画像ID, 位置...)` - 単体画像表示
- `@画像セット(画像ID, 縦位置...)` - 複数画像配置
- `@画像作成(gid, リソース名, x, y, w, h)` - スプライト生成
- `@リソース登録(リソース名, gid)` - リソース登録

### 本プロジェクト実装方針

eraTWの調査結果を踏まえ、2段階で実装:

#### Phase 1: ERBライブラリ方式（HTML_PRINT活用）

既存のEmuera機能のみで実装:

```erb
; ERB/SYSTEM/立ち絵表示.ERB
@立ち絵表示(キャラID, 位置=CENTER, レイヤー=0)
  LOCAL リソース名 = @立ち絵リソース取得(キャラID)
  HTML_PRINT "<img src='%リソース名%' ypos='%位置%'>"
  RETURN 1
```

#### Phase 2: Unity拡張（将来）

Phase 1で機能不足の場合のみ:
- Unity Canvas + Image コンポーネント
- IMediaManager インターフェース導入
- 独自命令追加 (DRAWSPRITE等)

### ERB 命令案（Phase 1）

```erb
; 既存命令を活用したラッパー関数
@立ち絵表示 キャラID, 位置, [レイヤー]
@立ち絵クリア [キャラID]  ; 省略時は全クリア
@立ち絵移動 キャラID, 新位置
```

### ファイル配置

```
Game/
├── CSV/
│   └── resources/
│       ├── 立ち絵.csv        ; リソースID定義
│       ├── 顔.csv            ; 表情レイヤー定義
│       └── アクセサリ.csv    ; 装飾定義
├── ERB/
│   └── グラフィック表示/
│       ├── 立ち絵表示.ERB    ; メイン表示ライブラリ
│       └── 立ち絵表示.ERH    ; 変数定義
└── resources/
    └── tachie/
        ├── {CharaID}.png      ; ボディスプライトシート
        └── {CharaID}_顔.png   ; 表情スプライトシート
```

## Effort Estimate

- **Size**: Medium → **Small-Medium** (Phase 1のみなら既存機能活用)
- **Risk**: Medium → **Low** (eraTW実績あり、新規命令不要)
- **Testability**: ★★★☆☆ → **★★★★☆** (HTML出力はヘッドレスでも検証可能)

## Dependencies

- Phase 6 (043) 完了 ✅

## Alternatives

Phase 5 並行可の Feature もあり：
- **037**: Gender filter (★★★, Low effort)
- **041**: Casino system import (★★★★, Low effort)

## Links

- [index-features.md](../index-features.md) - Feature tracking
- [engine-reference.md](../reference/engine-reference.md) - Engine architecture
- [content-roadmap.md](../content-roadmap.md) - Media System roadmap

## Reference

- **eraTW** (`C:\Users\siihe\OneDrive\同人ゲーム\eraTW`) - 立ち絵システム実装の参考元
  - `ERB/グラフィック表示ライブラリ/` - 表示ライブラリ実装
  - `resources/立ち絵.csv`, `顔.csv` - リソース定義形式
  - HTML_PRINT + CSVリソース管理方式を採用
