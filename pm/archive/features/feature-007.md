# Feature 007: Save File Analyzer

## Status: [DONE]

## Overview

セーブファイル（.sav）を解析し、人間可読形式で変数値を出力するCLIツール。デバッグとテスト効率化を目的とする。

## Problem

現状のデバッグ課題:
- セーブファイルはテキストベースだが構造が複雑（`__FINISHED`マーカー、配列、キャラクターデータ）
- 特定変数の値を確認するには手動でファイルを解析する必要
- テストシナリオ作成時に既存セーブの状態把握が困難

## Goals

1. **セーブファイル解析**: .savファイルを読み込み、構造を解析
2. **人間可読出力**: JSON形式で変数値を出力
3. **フィルタ機能**: 特定変数・キャラクターのみ抽出
4. **Headless連携**: テストシナリオ作成支援

## Technical Analysis

### セーブファイル形式

```
行1: ゲームコード (7153)
行2: バージョン (40)
行3: 日時 + ゲーム名
行4: キャラクター数
行5-N: プレイヤー名、各種変数
...
__FINISHED: 配列終端マーカー
__EMUERA_1808_STRAT__: Emueraセクション開始
```

### 既存コード参照

- `EraDataReader` (`Sub/EraDataStream.cs`): セーブ読み込みロジック
- `Process.State.cs`: セーブ/ロード処理

### 実装案

```
tools/SaveAnalyzer/
├── SaveAnalyzer.csproj
├── Program.cs
├── SaveReader.cs      # EraDataReaderベースの解析
├── SaveStructure.cs   # 構造定義
└── JsonExporter.cs    # JSON出力
```

### CLI仕様

```bash
# 全体出力
dotnet run --project tools/SaveAnalyzer -- Game/tests/base-wakeup.sav

# 特定変数フィルタ
dotnet run --project tools/SaveAnalyzer -- --filter "CFLAG:297" save.sav

# キャラクターフィルタ
dotnet run --project tools/SaveAnalyzer -- --character "咲夜" save.sav

# 比較モード（差分抽出）
dotnet run --project tools/SaveAnalyzer -- --diff save1.sav save2.sav
```

## Acceptance Criteria

- [x] .savファイルを正常に読み込み
- [x] ヘッダー情報（ゲームコード、バージョン、日時、キャラ数）をJSON出力
- [ ] 主要変数（FLAG, CFLAG, TALENT, ABL等）をJSON出力（将来拡張）
- [ ] キャラクター名でフィルタ可能（将来拡張）
- [ ] 変数名でフィルタ可能（将来拡張）
- [x] 既存テストセーブ（base-wakeup.sav）で動作確認

## File Changes

| File | Change |
|------|--------|
| `tools/SaveAnalyzer/` | 新規ディレクトリ |
| `tools/SaveAnalyzer/SaveAnalyzer.csproj` | 新規 |
| `tools/SaveAnalyzer/Program.cs` | 新規: CLIエントリポイント |
| `tools/SaveAnalyzer/SaveReader.cs` | 新規: 解析ロジック |

## Risks

| Risk | Mitigation |
|------|------------|
| セーブ形式の複雑さ | EraDataReaderのロジックを参考に段階的実装 |
| バージョン互換性 | 主要バージョン（1808）に集中、他は警告出力 |

## Estimated Effort

**Low** - 既存のEraDataReaderロジックを参考に実装

| Task | Estimate |
|------|----------|
| セーブ形式分析 | Small |
| SaveReader実装 | Small |
| JSON出力実装 | Small |
| CLI/フィルタ実装 | Small |
| テスト | Small |
| **Total** | **Low** |

## Links

- [index-features.md](index-features.md) - Feature index
- [EraDataStream.cs](../../uEmuera/Assets/Scripts/Emuera/Sub/EraDataStream.cs) - Save format reference
