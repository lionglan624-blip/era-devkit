# Feature 166: Kojo-Mapper Extension - Comprehensive Coverage & Quality Analysis

## Status: [DONE]

## Type: qw

## Implementation Note

**技術的に単純な実装**:
- 既存kojo_mapper.pyにパターン辞書を追加するだけ
- 新規アルゴリズムや複雑なロジックは不要
- 正規表現マッチ → カウント → 表示の繰り返し

## Background

**Problem**: 現行kojo-mapperは品質分析ツール（分岐パターン、対話行数、バリエーション検出）として機能しているが、**全口上システムの実装状況を網羅的に検証する機能がない**。

**Approach**: 既存のkojo-mapper機能を**拡張**し、品質メトリクスを維持しつつカバレッジ検証機能を追加する。

### 既存kojo-mapper機能（維持・拡張対象）

| 機能 | 説明 | 維持 |
|------|------|:----:|
| **口上行数計測** | PRINTFORM/DATAFORM のみカウント（システム文除外） | ✅ |
| **分岐タイプ分析** | TALENT_4/3/1、ABL、NTR分岐を個別集計 | ✅ |
| **行/分岐計算** | `dialogue_text_lines / kojo_block_count` | ✅ |
| **AC準拠スコア** | 分岐品質の加重スコア計算 | ✅ |
| **バリエーション検出** | PRINTDATA/IF RAND パターン検出 | ✅ |
| **特殊条件分岐** | 処女/妊娠/人妻等の条件検出 | ✅ |

### 口上システム全体像（5,500+関数）

| カテゴリ | 関数パターン | 規模 | 現状 |
|----------|-------------|------|------|
| **COM系** | `@KOJO_MESSAGE_COM_K{N}_{COM}` | 1,337+ | 未追跡 |
| **NTRイベント系** | `@NTR_KOJO_K{N}_{ID}` | 597+ | 未追跡 |
| **NTR Pre系** | `@NTR_KOJO_PRE_K{N}_{COM}_{SCENARIO}` | 145+ | 未追跡 |
| **EVENT系** | `@KOJO_EVENT_K{N}_{SCENE}` | 66+ | 未追跡 |
| **会話親密系** | `@KOJO_MESSAGE_COUNTER_K{N}_{ID}` | 318+ | 未追跡 |
| **日常系** | `@KOJO_MESSAGE_COM_KU_4XX` | 18 | 未追跡 |
| **WC系** | `@SexHara*` / `@WC_*` / `@KOJO_WC_*` | 483+ | 未追跡 |
| **SCOM系** | `@KOJO_MESSAGE_SCOM_K{N}_{COM}` | 174+ | 未追跡 |
| **関係性系** | `@KOJO_MESSAGE_*獲得` | 4 | 未追跡 |
| **Parameter系** | `@KOJO_MESSAGE_PALAMCNG_*` | 42 | 未追跡 |
| **Mark系** | `@KOJO_MESSAGE_MARKCNG_*` | 6 | 未追跡 |
| **Farewell系** | `@KOJO_MESSAGE_K{N}_SeeYou` | 63+ | 未追跡 |
| **Special NTR** | `@NTR_KOJO_K{N}_FAKE/MARK/*` | 17 | 未追跡 |

**影響**:
- 特定COMシリーズの部分実装を検出不可
- NTRイベント（お持ち帰り、野外調教等）の実装状況不明
- 初対面・日常・イベント系の網羅率不明
- キャラ別カバレッジの偏り可視化不可

**Goal**: kojo-mapperを拡張し、既存の品質メトリクスを維持しつつ全13カテゴリのカバレッジ検証機能を追加

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | 14カテゴリ検出 | output | contains | "Total: 14 categories" | [x] |
| 2 | キャラ別集計 | output | contains | "K1:" | [x] |
| 3 | 既存メトリクス維持 | output | contains | "行/分岐" | [x] |

### AC Details

**AC1**: 14カテゴリのパターン辞書を追加し、全カテゴリを検出（NTR_EVENTとNTR_WITNESSを別カテゴリとして計上）
```
Category Summary:
Total: 14 categories
```

**AC2**: キャラクター別（K1-K10）の実装数を集計・表示

**AC3**: 既存の品質メトリクス（行/分岐、分岐タイプ）が引き続き動作することを確認

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | KOJO_PATTERNSに14カテゴリの正規表現を追加 | [x] |
| 2 | 2 | キャラ別（K1-K10）集計ロジック追加 | [x] |
| 3 | 3 | 既存メトリクス出力の動作確認 | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Technical Design

> **Note**: 以下は参照用のパターン定義。実装は既存kojo_mapper.pyに辞書追加するだけ。

### 1. カテゴリ別関数パターン定義（コピペ用）

```python
KOJO_PATTERNS = {
    # === 主要カテゴリ ===
    "COM": {
        "pattern": r"@KOJO_MESSAGE_COM_K(\d+)_(\d+)",
        "description": "調教コマンド口上",
        "scope": "150 COM × 11 chars",
    },
    "NTR_EVENT": {
        "pattern": r"@NTR_KOJO_K(\d+)_(.+)",
        "description": "NTRイベント（主人不在）",
        "scope": "80+ triggers × 11 chars",
    },
    "NTR_WITNESS": {
        "pattern": r"@NTR_KOJO_KW(\d+)_(.+)",
        "description": "NTRイベント（主人在宅=見せつけ）",
        "scope": "80+ triggers × 11 chars",
    },
    "NTR_PRE": {
        "pattern": r"@NTR_KOJO_PRE_KW?(\d+)_(\d+)_(\d+)_(\d+)",
        "description": "NTR Pre-scene variants",
        "scope": "COM × scenario × pattern",
    },
    "EVENT": {
        "pattern": r"@KOJO_EVENT_K(\d+)_(\d+)",
        "description": "イベント口上（初対面、部屋入室等）",
        "scope": "12 scenes × 11 chars",
    },
    "COUNTER": {
        "pattern": r"@KOJO_MESSAGE_COUNTER_K(\d+)_(\d+)",
        "description": "会話親密系カウンター",
        "scope": "45+ counters × 11 chars",
    },
    "DAILY": {
        "pattern": r"@KOJO_MESSAGE_COM_KU_(4\d\d)",
        "description": "日常系（訪問者対話等）",
        "scope": "COM 400-463",
    },
    "WC": {
        "pattern": r"@(SexHara|WC_|KOJO_WC_).*_K(\d+)",
        "description": "WC系/肉便器系",
        "scope": "6 difficulty × 11 chars",
    },
    "SCOM": {
        "pattern": r"@KOJO_MESSAGE_SCOM_K(\d+)_(\d+)",
        "description": "特殊コマンド口上",
        "scope": "variable",
    },

    # === 状態変化系 ===
    "RELATION": {
        "pattern": r"@KOJO_MESSAGE_(恋慕|思慕|告白).*_K(\d+|U)",
        "description": "関係性獲得（恋慕獲得、告白成功等）",
        "scope": "4 types × 11 chars",
    },
    "PARAM_CHANGE": {
        "pattern": r"@KOJO_MESSAGE_PALAMCNG_([ABC])_K(\d+)",
        "description": "パラメータ変化通知",
        "scope": "3 params × 11 chars",
    },
    "MARK_CHANGE": {
        "pattern": r"@KOJO_MESSAGE_MARKCNG_K(\d+)",
        "description": "マーク変化通知",
        "scope": "11 chars",
    },
    "FAREWELL": {
        "pattern": r"@KOJO_MESSAGE_K(\d+)_SeeYou",
        "description": "別れ口上",
        "scope": "11 chars",
    },

    # === 特殊系 ===
    "NTR_SPECIAL": {
        "pattern": r"@NTR_KOJO_K(\d+)_(FAKE_ORGASM|GET_ORDER|MAKE_CLIENT|MARK)",
        "description": "特殊NTR（偽絶頂、命令、マーキング）",
        "scope": "4 types × 11 chars",
    },
}
```

### 2. 期待値定義（content-roadmap.mdより）

```python
EXPECTED_COVERAGE = {
    "COM": {
        # Series 0: 愛撫系 (12)
        0: "愛撫", 1: "クンニ", 2: "アナル愛撫", 3: "乳愛撫",
        4: "素股", 5: "愛撫される", 6: "ペッティング", 7: "焦らす",
        8: "乳首愛撫", 9: "アナル愛撫される", 10: "射精見せ", 11: "Dキス",
        # Series 20: コミュ系 (2)
        20: "キスする", 21: "何もしない",
        # Series 40: 道具使用 (9)
        40: "ローター", 41: "Eマッサージャ", 42: "クリキャップ",
        43: "スカトロ放尿", 44: "バイブ", 45: "クリローター",
        46: "アナルプラグ", 47: "ポンプ・搾乳機", 48: "アナルバイブ",
        # ... 全150 COM
    },
    "NTR_EVENT": {
        "_0": "お持ち帰り帰宅",
        "_1": "訪問者会話", "_2": "キス", "_3": "身体接触",
        "_10_0": "お持ち帰り開始", "_10_1": "Lv1", "_10_2": "Lv2",
        "_10_3": "Lv3", "_10_4": "Lv4", "_10_5": "Lv5",
        "_11": "訪問者誘い",
        "_14_0": "事後0", "_14_1": "事後1", "_14_2": "事後2",
        "_15_*": "目撃/発覚",
        "_17_*": "野外調教開始",
        "_18_*": "野外調教進行",
        "_21": "観察1", "_22": "観察2",
    },
    "EVENT": {
        0: "部屋入室", 1: "別れ", 2: "すれ違い",
        3: "カウンター出会い", 4: "対話開始",
        # ... 12 scenes
    },
    "RELATION": {
        "恋慕獲得": "恋慕獲得時",
        "思慕獲得": "思慕獲得時",
        "告白成功": "告白成功時",
        "告白失敗": "告白失敗時",
    },
}
```

### 3. CLI拡張

```bash
# 全カテゴリサマリー
python tools/kojo-mapper/kojo_mapper.py --coverage all

# 特定カテゴリのみ
python tools/kojo-mapper/kojo_mapper.py --coverage com
python tools/kojo-mapper/kojo_mapper.py --coverage ntr
python tools/kojo-mapper/kojo_mapper.py --coverage event
python tools/kojo-mapper/kojo_mapper.py --coverage relation

# キャラ別レポート
python tools/kojo-mapper/kojo_mapper.py --coverage all --by-character

# Markdownファイル出力
python tools/kojo-mapper/kojo_mapper.py --coverage all --output coverage/

# 欠落のみ表示
python tools/kojo-mapper/kojo_mapper.py --coverage all --missing-only
```

### 4. 出力形式

**サマリー出力**（カバレッジ + 品質メトリクス統合）:
```
=== Kojo Coverage & Quality Report ===

Category Summary:
┌─────────────┬──────────┬──────────┬─────────┬─────────┬──────────┐
│ Category    │ Expected │ Actual   │ Coverage│ 行/分岐 │ AC準拠率 │
├─────────────┼──────────┼──────────┼─────────┼─────────┼──────────┤
│ COM         │ 1,650    │ 1,337    │ 81.0%   │ 3.2行   │ 45%      │
│ NTR_EVENT   │ 880      │ 597      │ 67.8%   │ 4.1行   │ 62%      │
│ NTR_WITNESS │ 880      │ 145      │ 16.5%   │ 2.8行   │ 38%      │
│ EVENT       │ 132      │ 66       │ 50.0%   │ 5.2行   │ 71%      │
│ COUNTER     │ 495      │ 318      │ 64.2%   │ 2.1行   │ 25%      │
│ RELATION    │ 44       │ 4        │ 9.1%    │ 6.0行   │ 100%     │
│ FAREWELL    │ 11       │ 63       │ 100%+   │ 1.5行   │ 15%      │
├─────────────┼──────────┼──────────┼─────────┼─────────┼──────────┤
│ TOTAL       │ 6,200    │ 5,557    │ 89.6%   │ 3.4行   │ 48%      │
└─────────────┴──────────┴──────────┴─────────┴─────────┴──────────┘

Branch Type Distribution:
├── TALENT_4 (4段階): 512 (9.2%)
├── TALENT_3 (3段階): 834 (15.0%)
├── ABL_3+:           203 (3.7%)
├── TALENT_1/ABL_1-2: 1,205 (21.7%)
└── NONE (分岐なし):  2,803 (50.4%)

Missing (Priority):
- COM_41 (Eマッサージャ): 0/10 chars
- RELATION_恋慕獲得: 0/10 chars (KU only)
- EVENT_0 (部屋入室): K3, K5, K6, K7 missing
```

**キャラ別出力**（カバレッジ + 品質）:
```
=== Character Coverage & Quality ===

K1 (美鈴):
  Coverage:
    COM:      65/150  (43.3%)
    NTR:      45/80   (56.3%)
    EVENT:     6/12   (50.0%)
    COUNTER:  28/45   (62.2%)
    RELATION:  0/4    (0.0%)  ← Missing!
    TOTAL:   312/500  (62.4%)

  Quality:
    行/分岐:    3.1行 (目標: 4+行)
    AC準拠率:   42%
    TALENT_4:   28 (9%)
    TALENT_3:   45 (14%)
    ELSE分岐:   89 (29%)
    バリエーション: 56 (18%)

K2 (小悪魔):
  Coverage:
    COM:     201/150  (134.0%)  ← 超過実装
    ...
```

**口上行数計測ルール**（既存ロジック維持）:
```
カウント対象（純粋な口上）:
  ✅ PRINTFORM, PRINTFORML, PRINTFORMW
  ✅ DATAFORM（PRINTDATA内）
  ✅ PRINTDATAブロック → 1行としてカウント

除外対象（システム文）:
  ❌ IF, SIF, ELSEIF, ELSE, ENDIF
  ❌ CALL, GOTO, RETURN
  ❌ ;コメント行
  ❌ 空行
  ❌ PRINT（PRINTFORMなし）
```

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-21 | FIX | debugger | Fixed single capture group handling - added else clause to count invalid char numbers for all K1-K10 | FIXED |
| 2025-12-21 | FIX | debugger | Fixed DAILY regex pattern from `4\d\d` to `4\d*` to match 0-3 digits | FIXED |
| 2025-12-21 | FIX | debugger | Fixed RELATION regex to use non-capturing group `K(?:(\d+)|U)` | FIXED |
| 2025-12-21 | FIX | debugger | Added _KU generic function handler (counts for all K1-K10) | FIXED |
| 2025-12-21 | INFO | debugger | 14 categories detected (not 13) - NTR_EVENT and NTR_WITNESS are separate | REPORTED |
| 2025-12-21 | IMPL | implementer | Added KOJO_PATTERNS dict (14 categories) to kojo_mapper.py | SUCCESS |
| 2025-12-21 | IMPL | implementer | Added count_category_matches() and generate_coverage_report() | SUCCESS |
| 2025-12-21 | IMPL | implementer | Added --coverage CLI argument | SUCCESS |
| 2025-12-21 | TEST | ac-tester | Verified AC1 (Total: 14 categories) | PASS |
| 2025-12-21 | TEST | ac-tester | Verified AC2 (K1: ...) | PASS |
| 2025-12-21 | TEST | ac-tester | Verified AC3 (行/分岐 preserved) | PASS |

## Links
- [kojo_mapper.py](../../tools/kojo-mapper/kojo_mapper.py) - 拡張対象の既存実装
- [kojo-mapper README](../../tools/kojo-mapper/README.md) - 既存ツールドキュメント
- [content-roadmap.md](content-roadmap.md) - 全カテゴリ定義元
- [kojo-phases.md](reference/kojo-phases.md) - Phase 8a-8k詳細仕様
