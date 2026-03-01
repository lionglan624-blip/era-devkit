# Feature 051: kojo-mapper AC Checker

## Status: [DONE]

## Overview

kojo-mapperを拡張し、AC基準（関係性刁E���E斁E��・バリエーション�E��E準拠度を測定する機�Eを追加する、E

## Problem

現在のkojo-mapperは関数数とキーワード�E現回数を測定するが、AC基準�E準拠度は測定できなぁE��E
- 関係性4段階�E岐！EALENT or ABL:親寁E���E有無
- 関数冁E�E斁E���E�目樁E-8行！E
- PRINTDATA/DATALISTによるバリエーション

結果、「kojo-mapper 100%」と「AC準拠100%」に大きな乖離がある、E

## Goals

1. AC準拠度を定量皁E��測定可能にする
2. キャラ/関数単位で改喁E��先度を可視化
3. 継続的な品質モニタリングの基盤を作る

## Acceptance Criteria

- [x] TALENT方式�E岐（恋人/恋�E/思�E/なし）�E検�E
- [x] ABL:親寁E��式�E岐�E検�E
- [x] 刁E��段階数�E�E段隁E3段隁Eなし）�EカウンチE
- [x] 関数冁E��数カウント（コメンチE空行除外！E
- [x] PRINTDATA/DATALIST使用の検�E
- [x] ELSEブロチE��有無の検�E
- [x] ダチE��ュボ�EドにAC準拠スコア表示
- [x] **検証**: 特定ファイル�E�侁E KOJO_K1.ERB�E�でClaude手動チェチE��とmapper結果が一致
- [x] Build succeeds
- [x] 既存テスチEpass

## Scope

### In Scope
- KojoFunction dataclassへのAC持E��追加
- analyze_function_content()の拡張
- kojo-dashboard.htmlへのAC準拠セクション追加
- キャラ別AC準拠玁E��マリー

### Out of Scope
- ELSEブロチE��の「距離感」品質チェチE���E�主観皁E��Phase 2�E�E
- 自動修正機�E
- CI統吁E

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
    r'ELSEIF\s+TALENT:[^:]+:恋�E',
    r'ELSEIF\s+TALENT:[^:]+:思�E',
]

ABL_PATTERNS = [
    r'IF\s+ABL:[^:]+:親寁Es*[<>=]',
    r'ELSEIF\s+ABL:[^:]+:親寁Es*[<>=]',
]
```

### Dashboard Output Example

```
=== 咲夁EAC準拠スコア ===
関数数: 316
├── 4段階�E岁E   52 (16%)
├── 3段階�E岁E   84 (27%)
├── 刁E��なぁE   180 (57%)
├── 平坁E��数:   3.2衁E(目樁E-8衁E
├── PRINTDATA:  45 (14%)
└── AC準拠玁E  ~16%
```

## Effort Estimate

- **Size**: Small-Medium
- **Risk**: Low�E�既存パース基盤あり�E�E
- **Testability**: ☁E�E☁E�E☁E��実データで検証可能�E�E
- **Sessions**: 2

## Links

- [index-features.md](../index-features.md) - Feature tracking
- [reference/kojo-reference.md](../reference/kojo-reference.md) - AC criteria definition
- [kojo-mapper source](../../src/tools/kojo-mapper/) - Implementation target
