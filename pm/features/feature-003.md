# Feature 003: ERB Linter / Validator

## Status: [DONE]

## Overview

ERBスクリプトの静的解析ツール。構文エラー、未定義変数、共通のミスを検出し、開発時の品質向上とデバッグ時間短縮を実現する。

## Goals

1. **構文チェック**: ERBの基本構文エラーを検出
2. **変数チェック**: 未定義変数、タイポの検出
3. **関数チェック**: 未定義関数呼び出し、引数ミスマッチ
4. **スタイルチェック**: 一貫性のない命名、非推奨パターン
5. **レポート出力**: JSON/テキスト形式での警告・エラー出力

## Use Case

```
開発者がERBを編集
    ↓
linter実行
    ↓
警告・エラーリスト取得
    ↓
修正
    ↓
再実行（クリーン確認）
```

---

## Scope

### In Scope

| カテゴリ | チェック内容 |
|---------|-------------|
| 構文 | IF/ENDIF対応、FOR/NEXT対応、括弧の対応 |
| 構文 | 関数定義 @FUNCNAME の形式 |
| 変数 | 未定義FLAGインデックス使用 |
| 変数 | 未定義CFLAGインデックス使用 |
| 関数 | CALL/TRYCALLの対象関数存在確認 |
| 関数 | CALLFORM動的呼び出しのパターン警告 |
| 命名 | 口上関数命名規則 (KOJO_*) 準拠確認 |

### Out of Scope (v1)

- 実行時エラーの検出（型不一致等）
- 自動修正（Auto-fix）
- IDEプラグイン連携
- パフォーマンス分析

---

## Technical Design

### Architecture

```
┌─────────────────┐
│  CLI Entry      │  erb-linter [options] <path>
└────────┬────────┘
         │
┌────────▼────────┐
│  File Scanner   │  ERBファイル列挙
└────────┬────────┘
         │
┌────────▼────────┐
│  ERB Parser     │  トークン化、AST構築（簡易）
└────────┬────────┘
         │
┌────────▼────────┐
│  Analyzer       │  各種チェック実行
│  ├─ SyntaxCheck │
│  ├─ VarCheck    │
│  ├─ FuncCheck   │
│  └─ StyleCheck  │
└────────┬────────┘
         │
┌────────▼────────┐
│  Reporter       │  結果出力（JSON/Text）
└─────────────────┘
```

### Technology

| 項目 | 選択 | 理由 |
|------|------|------|
| 言語 | C# (.NET 8) | uEmueraと同一環境、参照活用可能 |
| 形式 | コンソールアプリ | CI/CD統合容易 |
| 出力 | JSON + Text | 機械処理 + 人間可読 |

### Data Sources

| データ | ソース | 用途 |
|--------|--------|------|
| FLAG定義 | `CSV/FLAG.CSV` | FLAGインデックス検証 |
| CFLAG定義 | `CSV/CFLAG.csv` | CFLAGインデックス検証 |
| ABL定義 | `CSV/Abl.csv` | ABLインデックス検証 |
| TALENT定義 | `CSV/Talent.csv` | TALENTインデックス検証 |
| キャラ定義 | `CSV/Chara*.csv` | キャラ名検証 |
| 口上関数 | `kojo-reference.md` | 命名規則検証 |

---

## CLI Interface

```bash
erb-linter [options] <path>

Options:
  -h, --help              ヘルプ表示
  -o, --output <file>     結果をファイルに出力
  -f, --format <fmt>      出力形式: text (default), json
  -l, --level <level>     最小レベル: error, warning, info
  -c, --config <file>     設定ファイル（無視ルール等）
  --no-color              色付き出力を無効化

Examples:
  erb-linter Game/ERB/
  erb-linter --format json -o report.json Game/ERB/口上/
  erb-linter --level error Game/ERB/SYSTEM.ERB
```

---

## Output Format

### Text (Default)

```
Game/ERB/口上/4_咲夜/KOJO_K4.ERB:142:5: error: Unmatched IF without ENDIF
Game/ERB/口上/4_咲夜/KOJO_K4.ERB:256:12: warning: Undefined FLAG index: 999
Game/ERB/SYSTEM.ERB:89:1: info: Function @DEPRECATED_FUNC is never called

Summary:
  Errors:   1
  Warnings: 1
  Info:     1
  Files:    3
```

### JSON

```json
{
  "summary": {
    "errors": 1,
    "warnings": 1,
    "info": 1,
    "files_scanned": 3
  },
  "issues": [
    {
      "file": "Game/ERB/口上/4_咲夜/KOJO_K4.ERB",
      "line": 142,
      "column": 5,
      "level": "error",
      "code": "ERB001",
      "message": "Unmatched IF without ENDIF"
    }
  ]
}
```

---

## Error Codes

| Code | Level | Description |
|------|-------|-------------|
| ERB001 | error | IF/ENDIF mismatch |
| ERB002 | error | FOR/NEXT mismatch |
| ERB003 | error | 括弧の対応不一致 |
| ERB004 | error | 関数定義の構文エラー |
| VAR001 | warning | 未定義FLAG使用 |
| VAR002 | warning | 未定義CFLAG使用 |
| VAR003 | warning | 未定義変数名（タイポの可能性） |
| FUNC001 | warning | CALL対象関数が見つからない |
| FUNC002 | info | TRYCALL対象関数が見つからない（意図的かも） |
| FUNC003 | info | 定義されているが呼ばれない関数 |
| STYLE001 | info | 口上関数命名規則不一致 |

---

## Acceptance Criteria

- [x] ERBファイルをパースしてトークン化できる
- [x] IF/ENDIF、FOR/NEXTの対応チェックが動作する
- [x] CSV定義を読み込んで変数インデックスを検証できる
- [x] 結果をJSON/テキスト形式で出力できる
- [x] Game/ERB/ 全体をスキャンして結果を得られる

---

## Dependencies

- None (standalone tool)
- Optional: uEmuera codebase reference for ERB parsing logic

---

## Risks

| リスク | 対策 |
|--------|------|
| ERB構文が複雑で完全なパーサーが困難 | 簡易パーサーで主要構文のみ対応、段階的拡張 |
| 偽陽性（false positive）が多すぎる | レベル分け、設定ファイルで無視可能に |
| CALLFORM動的呼び出しの追跡困難 | 警告のみ、完全な検出は不可能と明記 |

---

## Links

- [WBS-003.md](WBS-003.md) - Work breakdown
- [kojo-reference.md](../reference/kojo-reference.md) - 口上関数インデックス
- [erb-reference.md](../reference/erb-reference.md) - ERB言語リファレンス
- [agents.md](../agents.md) - Workflow rules
