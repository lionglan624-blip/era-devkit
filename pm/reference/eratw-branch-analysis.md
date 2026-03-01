# eraTW 分岐構造 全 COM 比較分析

## 分析概要

**分析対象**: eraTW 4.920 霊夢口上 (M_KOJO_K1_コマンド.ERB)
**検出 COM 数**: 133
**分析日時**: 2026-01-01
**Feature**: F292

本分析は、eraTW における霊夢の全 COM 実装から分岐構造を抽出し、era紅魔館protoNTR の現行実装 (TALENT 4段階分岐のみ) との差分を明確化し、今後の roadmap に反映すべき分岐パターンを提案する。

---

## 分岐タイプ別集計

eraTW 霊夢口上で使用されている分岐条件の全体像:

| 分岐タイプ | 使用 COM 数 | 総出現回数 | 説明 | 優先度 |
|-----------|:----------:|:----------:|------|:------:|
| **TALENT** | 160 | 400+ | 特性分岐（恋人/恋慕等） | **既存** |
| **FLAG** | 137 | 300+ | フラグ分岐（時間停止、FLAG:70等） | 低 |
| **FIRSTTIME** | 88 | 88 | 初回実行判定 | **高** |
| **MARK** | 82 | 150+ | 刻印状態（不埒刻印等） | 中 |
| **CFLAG** | 21 | 50+ | キャラフラグ分岐（添い寝中、デート中等） | 中 |
| **NOWEX** | 12 | 30+ | 射精状態（射精中判定） | **高** |
| **TCVAR** | 11 | 20+ | テンポラリ変数（射精直後、精飲経験等） | **高** |
| **EXP** | 6 | 10+ | 経験値分岐 | 低※ |

**※注**: EXP 分岐は F216-F217 で既に実装済み（Phase 8e で口上追加予定）

---

## COM 系統別分岐パターン

### 口挿入系 (COM_80-85) - 優先分析

口挿入系 COM は **NOWEX, TCVAR 分岐が集中** しており、射精状態に応じた口上の重要性が高い。

| COM | Name | FIRSTTIME | NOWEX:MASTER:11 | TCVAR:104 | TCVAR:精飲経験 | MARK:不埒刻印 | FLAG:時間停止 | 備考 |
|:---:|------|:---------:|:---------------:|:---------:|:--------------:|:-------------:|:-------------:|------|
| 80 | 手淫 | 1 | 2 | - | - | 1 | 1 | 射精中分岐あり |
| **81** | **フェラチオ** | 1 | 1 | **1** | **2** | 1 | 1 | **精飲分岐の代表例** |
| 82 | パイズリ射精 | 1 | 2 | - | - | 1 | 1 | 射精中分岐あり |
| 83 | 素股射精 | 1 | 2 | - | - | 1 | 1 | 射精中分岐あり |
| 84 | Ｖ挿入射精 | 1 | 3 | - | - | 1 | - | 射精中分岐 (最頻) |
| 85 | Ａ挿入射精 | 1 | 2 | - | - | 1 | 1 | 射精中分岐あり |

**パターン分析**:
- **NOWEX:MASTER:11** (射精中): 80-85系全 COM で使用。射精瞬間の反応口上を分岐。
- **TCVAR:104** (射精直後): COM_81 のみ。射精直後の精液処理（精飲/吐き出し）分岐。
- **TCVAR:精飲経験**: COM_81 のみ。精飲経験値による反応変化（初/慣れ）。
- **FIRSTTIME**: 全 COM 共通。初回時特殊口上。

### 挿入系 (COM_0-13, 20-23)

基礎的な訓練 COM。TALENT/MARK/FIRSTTIME が中心。

| COM 範囲 | FIRSTTIME | MARK:不埒刻印 | FLAG:時間停止 | TALENT 分岐 | 特記事項 |
|:--------:|:---------:|:-------------:|:-------------:|:----------:|----------|
| 0-13 | 全て | 全て | ほぼ全て | 恋人/恋慕 | 標準パターン |
| 20-23 | 全て | 一部 | 一部 | 恋人/恋慕 | 親密度重視 |

**パターン**: シンプルな分岐構造。protoNTR の現行 TALENT 分岐で概ねカバー可能。

### 特殊系 (COM_00, COM_88-91)

| COM | Name | 特徴 | 主要分岐 |
|:---:|------|------|----------|
| 00 | SOURCE/NOWEX対応 | NOWEX:MASTER:11, FLAG複合 | 多重フラグ分岐 |
| 88 | 排泄系 | TALENT重視 | 恋慕分岐 |
| 90-91 | その他 | TALENT/MARK基本 | 標準 |

---

## 差分表

### 現行 protoNTR 実装 (v0.6, Phase 8d)

```erb
; 現行: TALENT 4段階分岐のみ
IF TALENT:恋人
    PRINTDATA
        DATALIST × 4  ; 4パターン
    ENDDATA
ELSEIF TALENT:恋慕
    PRINTDATA
        DATALIST × 4
    ENDDATA
ELSEIF TALENT:思慕
    PRINTDATA
        DATALIST × 4
    ENDDATA
ELSE
    PRINTDATA
        DATALIST × 4
    ENDDATA
ENDIF
```

**実装済み分岐**:
- ✅ TALENT 4段階 (恋人/恋慕/思慕/なし)
- ✅ ABL/EXP 分岐関数 (F216-F217, 口上未作成)

**未実装分岐**:
- ❌ FIRSTTIME (初回判定)
- ❌ NOWEX:MASTER:11 (射精中判定)
- ❌ TCVAR:104 (射精直後判定)
- ❌ TCVAR:精飲経験 (精飲経験値分岐)
- ❌ MARK:不埒刻印 (刻印状態分岐)
- ❌ CFLAG 状況分岐 (添い寝中/デート中等)

### eraTW 標準パターン (COM_81 フェラチオ例)

```erb
; eraTW: 多重分岐構造
IF TALENT:恋人
    IF NOWEX:MASTER:11  ; 射精中判定
        IF TCVAR:精飲経験 >= 10  ; 精飲経験あり
            PRINTFORMW 「んむっ……♥　全部、飲んであげる……♥」
        ELSE  ; 精飲初心者
            PRINTFORMW 「ん……っ！？　こ、こんなに……♥」
        ENDIF
    ELSEIF TCVAR:104  ; 射精直後（精液処理）
        IF TCVAR:精飲経験 >= 10
            PRINTFORMW 「……ごくん……♥　美味しかった……」
        ELSE
            PRINTFORMW 「……んっ……ん……ごく……」
        ENDIF
    ELSEIF FIRSTTIME()  ; 初回
        PRINTFORMW 「――%CALLNAME:MASTER%……初めて、だから……優しくね？」
    ELSE  ; 通常時
        PRINTDATA
            DATALIST × 4  ; 通常パターン
        ENDDATA
    ENDIF
ELSEIF TALENT:恋慕
    ; 同様の構造
...
ENDIF
```

### 差分サマリ

| 分岐条件 | protoNTR | eraTW | 差分 | 影響 COM 範囲 |
|---------|:--------:|:-----:|:----:|--------------|
| TALENT 4段階 | ✅ | ✅ | - | 全 COM |
| ABL/EXP | ✅ (関数のみ) | ❌ | protoNTR 独自 | Phase 8e |
| **FIRSTTIME** | ❌ | ✅ | **未実装** | **88 COMs** |
| **NOWEX:MASTER:11** | ❌ | ✅ | **未実装** | **12 COMs (80-85系)** |
| **TCVAR:104** | ❌ | ✅ | **未実装** | **COM_81** |
| **TCVAR:精飲経験** | ❌ | ✅ | **未実装** | **COM_81** |
| MARK:不埒刻印 | ❌ | ✅ | 未実装 | 82 COMs |
| CFLAG 状況 | ❌ | ✅ | 未実装 | 21 COMs |
| FLAG 複合 | ❌ | ✅ | 未実装 | 137 COMs |

**重要度判定**:
- **高**: FIRSTTIME, NOWEX, TCVAR (品質向上に直結)
- **中**: MARK, CFLAG (状況による表現拡張)
- **低**: FLAG 複合 (eraTW 特有システム、protoNTR では不要)

---

## Roadmap 更新提案

### 提案の前提

**既存計画との重複回避**:
- ❌ ABL/TALENT 分岐: Phase 8e (F189, F216-F217) で計画済み
- ❌ Event kojo: Phase 8g で計画済み
- ❌ NTR FAV_* 分岐: Phase 8h で計画済み

**本提案の対象**: eraTW 分析で判明した **未計画の分岐パターン** のみ

### 提案1: FIRSTTIME 分岐追加 (Phase 8f 拡張)

**背景**: eraTW では 88 COMs で FIRSTTIME 判定を使用。初体験時の特別な反応は重要。

**提案内容**:
- Phase 8f ("First experience kojo") の範囲を拡張
- 現行: 特定 COM の初体験のみ (15-30 lines)
- 拡張後: **全 COM に FIRSTTIME 短文追加** (1-3 lines)

**実装例** (COM_81 フェラチオ):
```erb
IF FIRSTTIME()
    PRINTFORMW 「――これが、初めて……緊張するけど、%CALLNAME:MASTER%のためなら……」
    RETURN 1
ENDIF

; 以降、既存の TALENT 4段階分岐
IF TALENT:恋人
    ...
```

**工数見積もり**: 150 COMs × 10 キャラ × 1 FIRSTTIME 口上 = 1,500 行追加

**Roadmap 位置**: Phase 8f (C4) に統合

---

### 提案2: 射精状態分岐追加 (新 Phase 8j)

**背景**: 口挿入系 COM (80-85) で射精中/射精直後の分岐が重要。eraTW の品質の核心。

**提案内容**: 新規 Phase 8j "Ejaculation-state kojo" を追加

**対象 COM**:
- COM_80-85 (手淫/フェラ/パイズリ/素股/Ｖ挿入/Ａ挿入)
- COM_100-107 (対応する挿入系)

**分岐構造**:
```erb
; Phase 8j 構造 (COM_81例)
IF TALENT:恋人
    IF NOWEX:MASTER:11  ; 射精中
        IF TCVAR:精飲経験 >= 10  ; 精飲経験者
            PRINTFORMW 「んむっ……♥　――射精中口上（経験者）」
        ELSE  ; 精飲初心者
            PRINTFORMW 「ん……っ！？　――射精中口上（初心者）」
        ENDIF
        RETURN 1
    ELSEIF TCVAR:104  ; 射精直後（精液処理）
        IF TCVAR:精飲経験 >= 10
            PRINTFORMW 「……ごくん……♥　――射精直後口上（経験者）」
        ELSE
            PRINTFORMW 「……んっ……ごく……――射精直後口上（初心者）」
        ENDIF
        RETURN 1
    ENDIF
    ; 以降、通常時口上
    PRINTDATA
        DATALIST × 4
    ENDDATA
ELSEIF TALENT:恋慕
    ; 同様
...
```

**必要な変数追加**:
- `TCVAR:104` (射精直後フラグ) - エンジン側で COM 終了時にセット
- `TCVAR:精飲経験` (精飲回数カウンタ) - COM_81/82 で INCREMENT

**工数見積もり**:
- COM_81 (フェラチオ): TALENT 4 × (射精中2 + 射精直後2) × 10キャラ = 160 DATALIST
- COM_80, 82-85 (他5 COM): TALENT 4 × 射精中2 × 10キャラ × 5 = 400 DATALIST
- 合計: 約 560 DATALIST 追加

**Roadmap 位置**: v0.9 以降、新規 Phase 8j として追加

---

### 提案3: 状況別口上分岐 (Phase 8k 統合)

**背景**: CFLAG 状況分岐 (添い寝中/デート中/風呂) が 21 COMs で使用。

**提案内容**: Phase 8k "Special situation kojo" に CFLAG 状況分岐を統合

**対象状況**:
- `CFLAG:添い寝中`
- `CFLAG:デート中`
- `BATHROOM()` (風呂)

**実装例** (COM_300 会話):
```erb
IF TALENT:恋人
    PRINTFORM 霊夢は%CALLNAME:MASTER%の言葉に
    IF CFLAG:TARGET:添い寝中
        PRINTFORMW  心底幸せそうな顔をして、%CALLNAME:MASTER%の腕に抱きついてきた。
    ELSEIF CFLAG:TARGET:デート中
        PRINTFORMW  、繋いだ手の指を絡めた。
    ELSEIF BATHROOM()
        PRINTFORMW  、何の恥じらいもなく抱きついてきた。
    ELSE
        PRINTFORMW  幸せそうな笑みを浮かべた。
    ENDIF
    ; 以降通常口上
```

**工数見積もり**: Phase 8k の一環として吸収可能（追加工数少）

**Roadmap 位置**: Phase 8k (C8) に統合

---

### 提案4: 刻印状態分岐 (低優先度)

**背景**: MARK:不埒刻印 が 82 COMs で使用。ただし protoNTR は刻印システムを持たない。

**提案内容**: 将来的な刻印システム実装後に検討

**判断**: v2.x 以降の System Track (S1-S3) で刻印システムが追加される場合に再評価。現時点では roadmap に追加しない。

---

### Roadmap 更新案サマリ

| Phase | 現行 | 提案後 | 追加内容 | Version |
|:-----:|------|--------|----------|:-------:|
| 8f | First experience (特定 COM) | **FIRSTTIME 全 COM 対応** | 全 COM に初回口上追加 (1-3 lines) | v0.9-v1.0 |
| **8j** | **(新規)** | **射精状態分岐** | NOWEX/TCVAR:104/精飲経験 分岐 (COM_80-85, 100-107) | **v1.2-v1.3** |
| 8k | Special situation | **状況別口上統合** | CFLAG 状況分岐 (添い寝/デート/風呂) を統合 | v1.4-v1.6 |

**Version Roadmap への反映**:
```markdown
| v1.0 | C2 Complete | S0 | - | 350-600 series + quality check | - |
| v1.1 | C3 | S0 | - | ABL/TALENT 分岐口上追加 (Phase 8e) | - |
| v1.2 | C4 | S0 | - | First experience + FIRSTTIME 全対応 (Phase 8f) | - |
| **v1.3** | **C_EJ** | **S0** | **-** | **Ejaculation-state kojo (Phase 8j)** | **-** |
| v1.4 | C5 | S0 | - | Event kojo (Phase 8g) | - |
| v1.5 | C6 | S0 | - | NTR kojo depth (Phase 8h) | - |
| v1.6 | C7-C8 | S0 | - | Location/Situation kojo (Phase 8k) | - |
```

---

## 参考: eraTW COM_81 実装詳細

COM_81 (フェラチオ) は eraTW で最も分岐が複雑な COM の一つ。参考として分岐構造を記録:

### 分岐ツリー構造

```
COM_81 (フェラチオ)
├─ TALENT:恋人
│  ├─ NOWEX:MASTER:11 (射精中)
│  │  ├─ TCVAR:精飲経験 >= 10 (経験者)
│  │  └─ TCVAR:精飲経験 < 10 (初心者)
│  ├─ TCVAR:104 (射精直後)
│  │  ├─ TCVAR:精飲経験 >= 10 (経験者)
│  │  └─ TCVAR:精飲経験 < 10 (初心者)
│  ├─ FIRSTTIME() (初回)
│  ├─ MARK:不埒刻印 (刻印あり)
│  ├─ FLAG:時間停止 (時姦)
│  └─ 通常時 (DATALIST × 4)
├─ TALENT:恋慕
│  └─ (同様の構造)
├─ TALENT:思慕
│  └─ (同様の構造)
└─ なし
   └─ (同様の構造)
```

**実装深度**: 最大3段ネスト (TALENT → 射精状態 → 経験値)

**protoNTR での実装難易度**: 中程度（TCVAR 変数追加とエンジン側対応が必要）

---

## 分析結論

### 重要な発見

1. **FIRSTTIME 判定の重要性**: 88 COMs で使用。初体験時の特別な反応は品質向上の鍵。
2. **射精状態分岐の集中**: 口挿入系 COM (80-85) で NOWEX/TCVAR 分岐が集中。eraTW の核心的品質要素。
3. **TALENT × 状態 多重分岐**: eraTW は TALENT 分岐の内側に状態分岐をネスト。protoNTR の TALENT 単独分岐より表現力が高い。

### 実装優先順位

| 優先度 | 分岐パターン | 理由 | Phase | Version |
|:------:|-------------|------|:-----:|:-------:|
| **高** | FIRSTTIME | 実装容易 + 全 COM 適用可 + 品質向上大 | 8f | v1.2 |
| **高** | NOWEX/TCVAR (射精状態) | 口挿入系の品質を劇的向上 | 8j | v1.3 |
| **中** | CFLAG 状況分岐 | 表現拡張、既存 Phase 8k に統合可 | 8k | v1.6 |
| **低** | MARK 刻印 | システム未実装、将来検討 | - | v2.x+ |
| **低** | FLAG 複合 | eraTW 特有、protoNTR 不要 | - | - |

### content-roadmap.md への反映推奨

上記「提案1-3」を content-roadmap.md に追記し、Phase 8j を新設することを推奨する。

---

## 参考資料

- eraTW 4.920: `C:\Users\siihe\OneDrive\同人ゲーム\eraTW4.920`
- eraTW 霊夢口上: `ERB\口上・メッセージ関連\個人口上\001 Reimu [霊夢]\霊夢\M_KOJO_K1_コマンド.ERB`
- protoNTR 現行実装: `Game/ERB/口上/1_美鈴/KOJO_K1_口挿入.ERB` (F282, COM_80)
- Feature 292: [feature-292.md](../feature-292.md)
- Content Roadmap: [content-roadmap.md](../content-roadmap.md)
