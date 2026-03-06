# Feature 013: FontManager Service Extraction

## Status: [DONE]

## Overview

Config.csのフォント管理機能（キャッシュ、取得、クリア）を独立したサービスとして抽出し、テスト可能性と単一責任原則を向上させる。

## Current State

Config.cs内のフォント管理コード (L201-243, 約43行):

```csharp
static readonly Dictionary<string, Dictionary<FontStyle, Font>> fontDic = ...;
public static Font Font { get; }
public static Font GetFont(string theFontname, FontStyle style) { ... }
public static void ClearFont() { ... }
```

**問題点**:
- Configクラスが設定以外の責務（フォントキャッシュ管理）を持つ
- フォント取得ロジックのユニットテストが困難
- 14ファイルがConfig.Font/GetFont/ClearFontに依存

## Proposed Solution

### New Components

| File | Type | Description |
|------|------|-------------|
| `Sub/IFontManager.cs` | Interface | フォント取得API |
| `Sub/FontManager.cs` | Class | デフォルト実装（キャッシュ付き） |

### Interface Design

```csharp
internal interface IFontManager
{
    Font DefaultFont { get; }
    Font GetFont(string fontName, FontStyle style);
    void ClearCache();
}
```

### Migration Strategy

```csharp
// Config.cs - 後方互換性維持
private static IFontManager _fontManager = new FontManager();
public static IFontManager FontManager { get => _fontManager; set => _fontManager = value ?? new FontManager(); }

// 既存のstaticメソッドは委譲
public static Font Font => _fontManager.DefaultFont;
public static Font GetFont(string name, FontStyle style) => _fontManager.GetFont(name, style);
public static void ClearFont() => _fontManager.ClearCache();
```

## Goals

1. **単一責任原則**: フォント管理をConfigから分離
2. **テスト可能性**: IFontManagerをモック可能に
3. **後方互換性**: 既存の14ファイルは変更不要

## Acceptance Criteria

- [ ] IFontManager interfaceが定義されている
- [ ] FontManager classがIFontManagerを実装
- [ ] Config.csが FontManagerに委譲
- [ ] 既存の Config.Font/GetFont/ClearFont が動作
- [ ] ビルド成功
- [ ] 単体テスト: FontManager動作確認
- [ ] 単体テスト: DI注入確認

## Scope

### In Scope
- IFontManager interface作成
- FontManager実装
- Config.cs修正（委譲パターン）
- 単体テスト

### Out of Scope
- 既存14ファイルのリファクタリング（後方互換性で不要）
- フォントロジック自体の変更

## Effort Estimate

- **Size**: Small (1セッション)
- **Risk**: 🟢 Low
- **Files**: 3 create, 1 modify

## Dependencies

- Feature 012 (WarningCollector) - 同じパターンを適用

---

## Links

- [Config.cs](../../uEmuera/Assets/Scripts/Emuera/Config/Config.cs) - Current implementation (L201-243)
- [index-features.md](index-features.md) - Feature tracking
- [agents.md](agents.md) - Workflow rules
