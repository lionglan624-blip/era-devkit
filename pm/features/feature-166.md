# Feature 166: Kojo-Mapper Extension - Comprehensive Coverage & Quality Analysis

## Status: [DONE]

## Type: qw

## Implementation Note

**技術的に単純な実裁E*:
- 既存kojo_mapper.pyにパターン辞書を追加するだぁE
- 新規アルゴリズムめE��E��なロジチE��は不要E
- 正規表現マッチEↁEカウンチEↁE表示の繰り返し

## Background

**Problem**: 現行kojo-mapperは品質刁E��チE�Eル�E��E岐パターン、対話行数、バリエーション検�E�E�として機�EしてぁE��が、E*全口上シスチE��の実裁E��況を網羁E��に検証する機�EがなぁE*、E

**Approach**: 既存�Ekojo-mapper機�EめE*拡張**し、品質メトリクスを維持しつつカバレチE��検証機�Eを追加する、E

### 既存kojo-mapper機�E�E�維持�E拡張対象�E�E

| 機�E | 説昁E| 維持E|
|------|------|:----:|
| **口上行数計測** | PRINTFORM/DATAFORM のみカウント（シスチE��斁E��外！E| ✁E|
| **刁E��タイプ�E极E* | TALENT_4/3/1、ABL、NTR刁E��を個別雁E��E| ✁E|
| **衁E刁E��計箁E* | `dialogue_text_lines / kojo_block_count` | ✁E|
| **AC準拠スコア** | 刁E��品質の加重スコア計箁E| ✁E|
| **バリエーション検�E** | PRINTDATA/IF RAND パターン検�E | ✁E|
| **特殊条件刁E��E* | 処女/妊娠/人妻等�E条件検�E | ✁E|

### 口上シスチE��全体像�E�E,500+関数�E�E

| カチE��リ | 関数パターン | 規模 | 現状 |
|----------|-------------|------|------|
| **COM系** | `@KOJO_MESSAGE_COM_K{N}_{COM}` | 1,337+ | 未追跡 |
| **NTRイベント系** | `@NTR_KOJO_K{N}_{ID}` | 597+ | 未追跡 |
| **NTR Pre系** | `@NTR_KOJO_PRE_K{N}_{COM}_{SCENARIO}` | 145+ | 未追跡 |
| **EVENT系** | `@KOJO_EVENT_K{N}_{SCENE}` | 66+ | 未追跡 |
| **会話親寁E��** | `@KOJO_MESSAGE_COUNTER_K{N}_{ID}` | 318+ | 未追跡 |
| **日常系** | `@KOJO_MESSAGE_COM_KU_4XX` | 18 | 未追跡 |
| **WC系** | `@SexHara*` / `@WC_*` / `@KOJO_WC_*` | 483+ | 未追跡 |
| **SCOM系** | `@KOJO_MESSAGE_SCOM_K{N}_{COM}` | 174+ | 未追跡 |
| **関係性系** | `@KOJO_MESSAGE_*獲得` | 4 | 未追跡 |
| **Parameter系** | `@KOJO_MESSAGE_PALAMCNG_*` | 42 | 未追跡 |
| **Mark系** | `@KOJO_MESSAGE_MARKCNG_*` | 6 | 未追跡 |
| **Farewell系** | `@KOJO_MESSAGE_K{N}_SeeYou` | 63+ | 未追跡 |
| **Special NTR** | `@NTR_KOJO_K{N}_FAKE/MARK/*` | 17 | 未追跡 |

**影響**:
- 特定COMシリーズの部刁E��裁E��検�E不可
- NTRイベント（お持ち帰り、E��外調教等）�E実裁E��況不�E
- 初対面・日常・イベント系の網羁E��不�E
- キャラ別カバレチE��の偏り可視化不可

**Goal**: kojo-mapperを拡張し、既存�E品質メトリクスを維持しつつ全13カチE��リのカバレチE��検証機�Eを追加

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | 14カチE��リ検�E | output | contains | "Total: 14 categories" | [x] |
| 2 | キャラ別雁E��E| output | contains | "K1:" | [x] |
| 3 | 既存メトリクス維持E| output | contains | "衁E刁E��E | [x] |

### AC Details

**AC1**: 14カチE��リのパターン辞書を追加し、�EカチE��リを検�E�E�ETR_EVENTとNTR_WITNESSを別カチE��リとして計上！E
```
Category Summary:
Total: 14 categories
```

**AC2**: キャラクター別�E�E1-K10�E��E実裁E��を集計�E表示

**AC3**: 既存�E品質メトリクス�E�衁E刁E��、�E岐タイプ）が引き続き動作することを確誁E

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | KOJO_PATTERNSに14カチE��リの正規表現を追加 | [x] |
| 2 | 2 | キャラ別�E�E1-K10�E�集計ロジチE��追加 | [x] |
| 3 | 3 | 既存メトリクス出力�E動作確誁E| [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Technical Design

> **Note**: 以下�E参�E用のパターン定義。実裁E�E既存kojo_mapper.pyに辞書追加するだけ、E

### 1. カチE��リ別関数パターン定義�E�コピ�E用�E�E

```python
KOJO_PATTERNS = {
    # === 主要カチE��リ ===
    "COM": {
        "pattern": r"@KOJO_MESSAGE_COM_K(\d+)_(\d+)",
        "description": "調教コマンド口丁E,
        "scope": "150 COM ÁE11 chars",
    },
    "NTR_EVENT": {
        "pattern": r"@NTR_KOJO_K(\d+)_(.+)",
        "description": "NTRイベント（主人不在�E�E,
        "scope": "80+ triggers ÁE11 chars",
    },
    "NTR_WITNESS": {
        "pattern": r"@NTR_KOJO_KW(\d+)_(.+)",
        "description": "NTRイベント（主人在宁E見せつけ！E,
        "scope": "80+ triggers ÁE11 chars",
    },
    "NTR_PRE": {
        "pattern": r"@NTR_KOJO_PRE_KW?(\d+)_(\d+)_(\d+)_(\d+)",
        "description": "NTR Pre-scene variants",
        "scope": "COM ÁEscenario ÁEpattern",
    },
    "EVENT": {
        "pattern": r"@KOJO_EVENT_K(\d+)_(\d+)",
        "description": "イベント口上（�E対面、E��屋�E室等！E,
        "scope": "12 scenes ÁE11 chars",
    },
    "COUNTER": {
        "pattern": r"@KOJO_MESSAGE_COUNTER_K(\d+)_(\d+)",
        "description": "会話親寁E��カウンター",
        "scope": "45+ counters ÁE11 chars",
    },
    "DAILY": {
        "pattern": r"@KOJO_MESSAGE_COM_KU_(4\d\d)",
        "description": "日常系�E�訪問老E��話等！E,
        "scope": "COM 400-463",
    },
    "WC": {
        "pattern": r"@(SexHara|WC_|KOJO_WC_).*_K(\d+)",
        "description": "WC系/肉便器系",
        "scope": "6 difficulty ÁE11 chars",
    },
    "SCOM": {
        "pattern": r"@KOJO_MESSAGE_SCOM_K(\d+)_(\d+)",
        "description": "特殊コマンド口丁E,
        "scope": "variable",
    },

    # === 状態変化系 ===
    "RELATION": {
        "pattern": r"@KOJO_MESSAGE_(恋�E|思�E|告白).*_K(\d+|U)",
        "description": "関係性獲得（恋慕獲得、告白成功等！E,
        "scope": "4 types ÁE11 chars",
    },
    "PARAM_CHANGE": {
        "pattern": r"@KOJO_MESSAGE_PALAMCNG_([ABC])_K(\d+)",
        "description": "パラメータ変化通知",
        "scope": "3 params ÁE11 chars",
    },
    "MARK_CHANGE": {
        "pattern": r"@KOJO_MESSAGE_MARKCNG_K(\d+)",
        "description": "マ�Eク変化通知",
        "scope": "11 chars",
    },
    "FAREWELL": {
        "pattern": r"@KOJO_MESSAGE_K(\d+)_SeeYou",
        "description": "別れ口丁E,
        "scope": "11 chars",
    },

    # === 特殊系 ===
    "NTR_SPECIAL": {
        "pattern": r"@NTR_KOJO_K(\d+)_(FAKE_ORGASM|GET_ORDER|MAKE_CLIENT|MARK)",
        "description": "特殊NTR�E�偽絶頂、命令、�Eーキング�E�E,
        "scope": "4 types ÁE11 chars",
    },
}
```

### 2. 期征E��定義�E�Eontent-roadmap.mdより�E�E

```python
EXPECTED_COVERAGE = {
    "COM": {
        # Series 0: 愛撫系 (12)
        0: "愛撫", 1: "クンチE, 2: "アナル愛撫", 3: "乳愛撫",
        4: "素股", 5: "愛撫されめE, 6: "ペッチE��ング", 7: "焦らす",
        8: "乳首�E撫", 9: "アナル愛撫されめE, 10: "封E��見せ", 11: "Dキス",
        # Series 20: コミュ系 (2)
        20: "キスする", 21: "何もしなぁE,
        # Series 40: 道�E使用 (9)
        40: "ローター", 41: "Eマッサージャ", 42: "クリキャチE�E",
        43: "スカトロ放尿", 44: "バイチE, 45: "クリローター",
        46: "アナルプラグ", 47: "ポンプ�E搾乳橁E, 48: "アナルバイチE,
        # ... 全150 COM
    },
    "NTR_EVENT": {
        "_0": "お持ち帰り帰宁E,
        "_1": "訪問老E��話", "_2": "キス", "_3": "身体接触",
        "_10_0": "お持ち帰り開姁E, "_10_1": "Lv1", "_10_2": "Lv2",
        "_10_3": "Lv3", "_10_4": "Lv4", "_10_5": "Lv5",
        "_11": "訪問老E��い",
        "_14_0": "事征E", "_14_1": "事征E", "_14_2": "事征E",
        "_15_*": "目撁E発要E,
        "_17_*": "野外調教開姁E,
        "_18_*": "野外調教進衁E,
        "_21": "観寁E", "_22": "観寁E",
    },
    "EVENT": {
        0: "部屋�E室", 1: "別めE, 2: "すれ違い",
        3: "カウンター出会い", 4: "対話開姁E,
        # ... 12 scenes
    },
    "RELATION": {
        "恋�E獲征E: "恋�E獲得時",
        "思�E獲征E: "思�E獲得時",
        "告白成功": "告白成功晁E,
        "告白失敁E: "告白失敗時",
    },
}
```

### 3. CLI拡張

```bash
# 全カチE��リサマリー
python src/tools/kojo-mapper/kojo_mapper.py --coverage all

# 特定カチE��リのみ
python src/tools/kojo-mapper/kojo_mapper.py --coverage com
python src/tools/kojo-mapper/kojo_mapper.py --coverage ntr
python src/tools/kojo-mapper/kojo_mapper.py --coverage event
python src/tools/kojo-mapper/kojo_mapper.py --coverage relation

# キャラ別レポ�EチE
python src/tools/kojo-mapper/kojo_mapper.py --coverage all --by-character

# Markdownファイル出劁E
python src/tools/kojo-mapper/kojo_mapper.py --coverage all --output coverage/

# 欠落のみ表示
python src/tools/kojo-mapper/kojo_mapper.py --coverage all --missing-only
```

### 4. 出力形弁E

**サマリー出劁E*�E�カバレチE�� + 品質メトリクス統合！E
```
=== Kojo Coverage & Quality Report ===

Category Summary:
┌─────────────┬──────────┬──────────┬─────────┬─────────┬──────────━E
━ECategory    ━EExpected ━EActual   ━ECoverage━E衁E刁E��E━EAC準拠玁E━E
├─────────────┼──────────┼──────────┼─────────┼─────────┼──────────┤
━ECOM         ━E1,650    ━E1,337    ━E81.0%   ━E3.2衁E  ━E45%      ━E
━ENTR_EVENT   ━E880      ━E597      ━E67.8%   ━E4.1衁E  ━E62%      ━E
━ENTR_WITNESS ━E880      ━E145      ━E16.5%   ━E2.8衁E  ━E38%      ━E
━EEVENT       ━E132      ━E66       ━E50.0%   ━E5.2衁E  ━E71%      ━E
━ECOUNTER     ━E495      ━E318      ━E64.2%   ━E2.1衁E  ━E25%      ━E
━ERELATION    ━E44       ━E4        ━E9.1%    ━E6.0衁E  ━E100%     ━E
━EFAREWELL    ━E11       ━E63       ━E100%+   ━E1.5衁E  ━E15%      ━E
├─────────────┼──────────┼──────────┼─────────┼─────────┼──────────┤
━ETOTAL       ━E6,200    ━E5,557    ━E89.6%   ━E3.4衁E  ━E48%      ━E
└─────────────┴──────────┴──────────┴─────────┴─────────┴──────────━E

Branch Type Distribution:
├── TALENT_4 (4段隁E: 512 (9.2%)
├── TALENT_3 (3段隁E: 834 (15.0%)
├── ABL_3+:           203 (3.7%)
├── TALENT_1/ABL_1-2: 1,205 (21.7%)
└── NONE (刁E��なぁE:  2,803 (50.4%)

Missing (Priority):
- COM_41 (Eマッサージャ): 0/10 chars
- RELATION_恋�E獲征E 0/10 chars (KU only)
- EVENT_0 (部屋�E室): K3, K5, K6, K7 missing
```

**キャラ別出劁E*�E�カバレチE�� + 品質�E�E
```
=== Character Coverage & Quality ===

K1 (美鈴):
  Coverage:
    COM:      65/150  (43.3%)
    NTR:      45/80   (56.3%)
    EVENT:     6/12   (50.0%)
    COUNTER:  28/45   (62.2%)
    RELATION:  0/4    (0.0%)  ↁEMissing!
    TOTAL:   312/500  (62.4%)

  Quality:
    衁E刁E��E    3.1衁E(目樁E 4+衁E
    AC準拠玁E   42%
    TALENT_4:   28 (9%)
    TALENT_3:   45 (14%)
    ELSE刁E��E   89 (29%)
    バリエーション: 56 (18%)

K2 (小悪魁E:
  Coverage:
    COM:     201/150  (134.0%)  ↁE趁E��実裁E
    ...
```

**口上行数計測ルール**�E�既存ロジチE��維持E��E
```
カウント対象�E�純粋な口上！E
  ✁EPRINTFORM, PRINTFORML, PRINTFORMW
  ✁EDATAFORM�E�ERINTDATA冁E��E
  ✁EPRINTDATAブロチE�� ↁE1行としてカウンチE

除外対象�E�シスチE��斁E��E
  ❁EIF, SIF, ELSEIF, ELSE, ENDIF
  ❁ECALL, GOTO, RETURN
  ❁E;コメント衁E
  ❁E空衁E
  ❁EPRINT�E�ERINTFORMなし！E
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
| 2025-12-21 | TEST | ac-tester | Verified AC3 (衁E刁E��Epreserved) | PASS |

## Links
- [kojo_mapper.py](../../src/tools/kojo-mapper/kojo_mapper.py) - 拡張対象の既存実裁E
- [kojo-mapper README](../../src/tools/kojo-mapper/README.md) - 既存ツールドキュメンチE
- [content-roadmap.md](../content-roadmap.md) - 全カチE��リ定義允E
- [kojo-phases.md](../reference/kojo-phases.md) - Phase 8a-8k詳細仕槁E
