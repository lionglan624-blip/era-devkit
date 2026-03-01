# Feature 051: kojo-mapper AC Checker

## Status: [DONE]

## Overview

kojo-mapperを拡張し、AC基準（関係性分岐・文量・バリエーション）の準拠度を測定する機能を追加する。

## Problem

現在のkojo-mapperは関数数とキーワード出現回数を測定するが、AC基準の準拠度は測定できない：
- 関係性4段階分岐（TALENT or ABL:親密）の有無
- 関数内の文量（目標4-8行）
- PRINTDATA/DATALISTによるバリエーション

結果、「kojo-mapper 100%」と「AC準拠100%」に大きな乖離がある。

## Goals

1. AC準拠度を定量的に測定可能にする
2. キャラ/関数単位で改善優先度を可視化
3. 継続的な品質モニタリングの基盤を作る

## Acceptance Criteria

- [x] TALENT方式分岐（恋人/恋慕/思慕/なし）の検出
- [x] ABL:親密方式分岐の検出
- [x] 分岐段階数（4段階/3段階/なし）のカウント
- [x] 関数内行数カウント（コメント/空行除外）
- [x] PRINTDATA/DATALIST使用の検出
- [x] ELSEブロック有無の検出
- [x] ダッシュボードにAC準拠スコア表示
- [x] **検証**: 特定ファイル（例: KOJO_K1.ERB）でClaude手動チェックとmapper結果が一致
- [x] Build succeeds
- [x] 既存テスト pass

## Scope

### In Scope
- KojoFunction dataclassへのAC指標追加
- analyze_function_content()の拡張
- kojo-dashboard.htmlへのAC準拠セクション追加
- キャラ別AC準拠率サマリー

### Out of Scope
- ELSEブロックの「距離感」品質チェック（主観的、Phase 2）
- 自動修正機能
- CI統合

## Technical Design

### New Metrics in KojoFunction

```python
@dataclass
class KojoFunction:
    # Existing fields...

    # AC Metrics (new)
    branch_type: str = ""       # "TALENT_4", "TALENT_3", "ABL_3", "NONE"
    branch_depth: int = 0       # Number of relationship branches
    content_lines: int = 0      # Non-blank, non-comment lines
    has_printdata: bool = False # Uses PRINTDATA/DATALIST
    has_else: bool = False      # Has ELSE block for low-relationship
```

### Branch Detection Patterns

```python
TALENT_PATTERNS = [
    r'IF\s+TALENT:[^:]+:恋人',
    r'ELSEIF\s+TALENT:[^:]+:恋慕',
    r'ELSEIF\s+TALENT:[^:]+:思慕',
]

ABL_PATTERNS = [
    r'IF\s+ABL:[^:]+:親密\s*[<>=]',
    r'ELSEIF\s+ABL:[^:]+:親密\s*[<>=]',
]
```

### Dashboard Output Example

```
=== 咲夜 AC準拠スコア ===
関数数: 316
├── 4段階分岐:   52 (16%)
├── 3段階分岐:   84 (27%)
├── 分岐なし:   180 (57%)
├── 平均行数:   3.2行 (目標4-8行)
├── PRINTDATA:  45 (14%)
└── AC準拠率:  ~16%
```

## Effort Estimate

- **Size**: Small-Medium
- **Risk**: Low（既存パース基盤あり）
- **Testability**: ★★★★☆（実データで検証可能）
- **Sessions**: 2

## Links

- [index-features.md](index-features.md) - Feature tracking
- [reference/kojo-reference.md](reference/kojo-reference.md) - AC criteria definition
- [kojo-mapper source](../../tools/kojo-mapper/) - Implementation target
