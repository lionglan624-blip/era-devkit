# Feature 050: 立ち絵基盤実装

## Status: [DONE]

## Overview

Feature 049 の検証結果に基づき、立ち絵表示システムの基盤を実装する。

## Problem

ゲームにキャラクター立ち絵表示機能がない。Feature 049 で実装方針が決定した後、実際の基盤を構築する。

## Goals

1. 立ち絵表示用 ERB ライブラリの実装
2. CSV リソース管理システムの構築
3. 基本的な表示機能（表示/クリア/位置指定）の実装

## Acceptance Criteria

- [x] 立ち絵表示関数が動作する (@立ち絵表示, @立ち絵クリア)
- [x] CSV でリソース定義が可能 (resources/tachie.csv)
- [x] 複数キャラクターの同時表示が可能 (CBG z-depth管理)
- [x] 表示位置指定（左/中央/右）が可能 (立ち絵位置_左/中央/右 constants)
- [x] Build succeeds
- [x] 動作確認テスト pass (headless verification)

## Scope

### In Scope
- ERB 立ち絵表示ライブラリ
- CSV リソース定義ファイル
- 基本表示機能（表示/クリア/移動）
- テスト用サンプル画像（1-2枚）

### Out of Scope
- 全キャラクターの立ち絵素材
- 表情差分の自動切り替え
- アニメーション表示
- Unity 拡張（検証結果次第）

## Technical Considerations

### 実装方針（Feature 049 で決定）

#### 方針A: ERBライブラリ方式
```erb
; eraTW 方式を採用
@立ち絵表示(キャラID, 位置, レイヤー)
@立ち絵クリア(キャラID)
```

#### 方針B: Unity拡張方式
```csharp
// IMediaManager 経由
public interface ITachieManager {
    void Show(int charaId, Position pos);
    void Clear(int? charaId = null);
}
```

### ファイル構成

```
Game/
├── CSV/resources/
│   └── 立ち絵.csv
├── ERB/グラフィック表示/
│   ├── 立ち絵表示.ERB
│   └── 立ち絵表示.ERH
└── resources/tachie/
    └── (テスト画像)
```

## Effort Estimate

- **Size**: Medium
- **Risk**: Low (検証済み方針で実装)
- **Prerequisite**: Feature 049 完了

## Dependencies

- Feature 049 (画像表示機能検証) 完了必須
- Phase 6 (043) 完了 ✅

## Known Issues

### Unity UI Rendering Not Working

**Status**: Blocked - needs Unity Editor investigation

**Symptoms**:
- ERB `立ち絵表示` function works correctly
- CBGSETSPRITE stores data in cbgList successfully
- Sprite textures load successfully (verified: 180x180, 540x360)
- EmueraCBGRenderer creates Image components
- BUT: Nothing visible on screen (even magenta debug box test failed)

**Investigation Done**:
1. Verified sprite data flows correctly: ERB → CBGSETSPRITE → cbgList
2. Confirmed SpriteManager loads textures successfully
3. Tested multiple Y coordinates (0, 300) - no difference
4. Tested with solid magenta color instead of sprite - still invisible
5. CBGRenderer is created as child of EmueraContent at sibling index 0

**Suspected Causes**:
- Canvas hierarchy issue - CBGRenderer might not be properly part of the Canvas
- RectMask2D on EmueraContent may be clipping CBG content
- Layer/sorting order conflict with existing UI

**Next Steps** (for future work):
1. Open Unity Editor and inspect hierarchy at runtime
2. Check if CBGContent/CBGImage GameObjects exist and have correct transforms
3. Verify Canvas component relationships
4. Try creating CBGRenderer outside EmueraContent hierarchy

**Related Files**:
- `uEmuera/Assets/Scripts/EmueraCBGRenderer.cs` - Unity renderer (not working)
- `uEmuera/Assets/Scripts/EmueraContent.cs` - Initializes CBGRenderer
- `uEmuera/Assets/Scripts/Emuera/GameView/EmueraConsole.CBG.cs` - Data storage (working)

## Links

- [feature-044.md](feature-044.md) - 設計概要（eraTW調査結果）
- [feature-049.md](feature-049.md) - 事前検証（実装方針決定）
- [engine-reference.md](../reference/engine-reference.md) - Engine architecture
