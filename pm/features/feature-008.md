# Feature 008: Game State Dump

## Status: [DONE]

## Overview

SaveAnalyzer（Feature 007）を拡張し、セーブファイルから完全なゲーム状態（変数、キャラクターデータ）をJSON出力する機能を追加する。

## Problem

Feature 007で実装したSaveAnalyzerはヘッダー情報のみ解析:
- FLAG, CFLAG, TALENT, ABL等の変数値が取得できない
- キャラクター別のデータ抽出ができない
- テストシナリオ作成時に手動でセーブファイルを解析する必要

## Goals

1. **完全な変数解析**: グローバル変数（FLAG等）とキャラクター変数（CFLAG, TALENT, ABL等）
2. **キャラクターフィルタ**: 特定キャラクターのデータのみ抽出
3. **変数フィルタ**: 特定変数のみ抽出（例: `--filter "CFLAG:297"`）
4. **非ゼロ値のみ出力**: デフォルトで0以外の値のみ出力（ノイズ削減）

## Technical Analysis

### セーブファイル構造（EraDataStream.cs参照）

```
[Header]
  GameCode (7153)
  Version (40)
  Timestamp + GameName
  CharacterCount

[Global Arrays - Eramaker format]
  プレイヤー名
  各種配列 (values...\n__FINISHED)

[Emuera Extended Section]
  __EMUERA_1808_STRAT__
  key:value (単一値)
  __EMU_SEPARATOR__
  key\nvalues...\n__FINISHED (配列)
  __EMU_SEPARATOR__
  ...
```

### 実装方針

既存の`SaveReader.cs`を拡張:
1. `SkipToEmueraSection` → `ReadGlobalArrays`に変更
2. `ReadExtendedSection`を強化（配列、2D配列対応）
3. キャラクターデータのパース（CharacterCount分ループ）

### CLI拡張

```bash
# 全データ出力
dotnet run --project tools/SaveAnalyzer -- save.sav

# ヘッダーのみ（既存機能）
dotnet run --project tools/SaveAnalyzer -- --header save.sav

# キャラクターフィルタ
dotnet run --project tools/SaveAnalyzer -- --character "咲夜" save.sav

# 変数フィルタ
dotnet run --project tools/SaveAnalyzer -- --filter "CFLAG:2" save.sav

# 非ゼロ値のみ（デフォルト）
dotnet run --project tools/SaveAnalyzer -- --non-zero save.sav

# 全値出力
dotnet run --project tools/SaveAnalyzer -- --all save.sav
```

### 出力JSON構造

```json
{
  "file": "save.sav",
  "header": { ... },
  "emueraVersion": 1808,
  "globals": {
    "FLAG": {"26": 128, "100": 1},
    "TFLAG": {"0": 5}
  },
  "characters": [
    {
      "id": 0,
      "name": "咲夜",
      "CFLAG": {"2": 5000, "297": 100},
      "TALENT": {"10": 1, "20": 1},
      "ABL": {"0": 50, "1": 30}
    }
  ]
}
```

## Acceptance Criteria

- [x] グローバル変数（FLAG, TFLAG等）をJSON出力
- [x] キャラクター変数（CFLAG, TALENT, ABL等）をJSON出力
- [x] `--character`オプションでキャラクターフィルタ
- [x] `--filter`オプションで変数フィルタ
- [x] 既存の`--header`オプションとの互換性維持
- [x] base-wakeup.savで動作確認

## File Changes

| File | Change |
|------|--------|
| `tools/SaveAnalyzer/SaveReader.cs` | 拡張: 変数・キャラクター解析 |
| `tools/SaveAnalyzer/Program.cs` | 拡張: 新CLIオプション |

## Risks

| Risk | Mitigation |
|------|------------|
| セーブ形式の複雑さ | EraDataStream.csのロジックを忠実に再現 |
| 出力サイズ | 非ゼロ値のみデフォルト出力、フィルタオプション |

## Estimated Effort

**Low** - 既存SaveAnalyzerの拡張

| Task | Estimate |
|------|----------|
| グローバル変数解析 | Small |
| キャラクターデータ解析 | Small |
| フィルタ機能 | Small |
| テスト | Small |
| **Total** | **Low** |

## Links

- [feature-007.md](feature-007.md) - SaveAnalyzer (base implementation)
- [testing-reference.md](../reference/testing-reference.md) - Testing strategy
- [EraDataStream.cs](../../../uEmuera/Assets/Scripts/Emuera/Sub/EraDataStream.cs) - Save format reference
