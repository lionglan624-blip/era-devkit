# Feature 215: Kojo ABL/TALENT/EXP Branch Research

**Status**: [DONE]
**Type**: infra
**Priority**: Medium
**Version**: v0.7+

---

## Background

### Philosophy (思想・上位目標)

口上は単なるTALENT分岐（思慕/恋慕/恋人）だけでなく、キャラクターの感覚・感度・中毒・経験値を反映した多層的な分岐を持つべき。システムが蓄積する変数は口上にも反映されるべきである。

### Problem (現状の問題)

1. KOJO_KX.ERB に「ABL/TALENT分岐ガイド」がコメントとして存在するが、体系的な整理がされていない
2. 全COMカテゴリに対してどの変数で分岐すべきか明確でない
3. 分岐閾値（ABL >= 3 で高、等）が標準化されていない

### Goal (このFeatureで達成すること)

1. 全COMカテゴリの分岐候補を調査・文書化する
2. KOJO_KX.ERB の既存ガイドコメントを整理・拡充する
3. 実装仕様（関数設計）を確定させる → F216 で実装

---

## 分岐候補マトリクス（確定版）

### ABL（感覚/中毒/性技）

| ABL | 名前 | 対象COM | 分岐内容 |
|:---:|------|---------|----------|
| 0 | Ｃ感覚 | 愛撫系(0番台) | 高LVで敏感描写 |
| 1 | Ｖ感覚 | 挿入系(60番台) | 高LVで快感描写 |
| 2 | Ａ感覚 | アナル系 | 高LVで快感描写 |
| 3 | Ｂ感覚 | 胸系 | 高LVで敏感描写 |
| **31** | **精液中毒** | **口系(81,82,83,85)** | **LV3+で渇望描写** |
| 51 | 舌 | 口系(81,82) | 高LVで技巧描写 |
| 54 | 膣 | 挿入系(60番台) | 高LVで締め付け描写 |
| 55 | アナル | アナル系 | 高LVで締め付け描写 |

### TALENT（感度）

| TALENT | 名前 | 対象COM | 分岐内容 |
|:------:|------|---------|----------|
| 101 | Ｃ感度 | 愛撫系 | 敏感/鈍感で反応変化 |
| 102 | Ｖ感度 | 挿入系 | 敏感/鈍感で反応変化 |
| 103 | Ａ感度 | アナル系 | 敏感/鈍感で反応変化 |
| 104 | Ｂ感度 | 胸系/道具系(40番台) | 敏感/鈍感で反応変化 |
| **109** | **Ｍ感度** | **口系(81,82,85)** | **敏感/鈍感で反応変化** |
| 110 | Ｎ感度 | 乳首系 | 敏感/鈍感で反応変化 |
| 111 | Ｐ感度 | 挿入系（深部） | ポルチオ快感描写 |

### EXP（経験値）

| EXP | 名前 | 対象COM | 分岐内容 |
|:---:|------|---------|----------|
| 25 | 口淫経験 | 口系 | 経験豊富で余裕描写 |
| 20 | Ｖ性交経験 | 挿入系 | 経験豊富で余裕描写 |
| 21 | Ａ性交経験 | アナル系 | 経験豊富で余裕描写 |
| 27 | キス経験 | キス(20) | 経験豊富で積極描写 |

---

## COMカテゴリ別分岐提案

### 優先度: 高

| 番台 | カテゴリ | 追加分岐 |
|:----:|----------|----------|
| 80 | 奉仕系 | ABL:精液中毒, TALENT:Ｍ感度, EXP:口淫経験 |
| 60 | 挿入系 | ABL:Ｖ感覚, TALENT:Ｖ感度/Ｐ感度, EXP:Ｖ性交経験 |
| 40 | 道具系 | TALENT:対応部位感度 |

### 優先度: 中

| 番台 | カテゴリ | 追加分岐 |
|:----:|----------|----------|
| 0 | 愛撫系 | ABL:Ｃ感覚/Ｂ感覚, TALENT:Ｃ感度/Ｂ感度 |
| 90 | される系 | ABL:各感覚, TALENT:各感度 |
| 100 | SM系 | ABL:マゾっ気, TALENT:苦痛耐性 |

### 優先度: 低

| 番台 | カテゴリ | 追加分岐 |
|:----:|----------|----------|
| 20 | コミュ系 | EXP:キス経験（キスのみ） |
| 300 | 対あなた系 | 既存で十分 |

---

## Technical Details

### 分岐判定関数の設計（F216 で実装）

```erb
;============================================
; ABL/TALENT/EXP 分岐判定関数
;============================================

@GET_ABL_BRANCH(ARG, ARG:1)
;ARG = TARGET番号, ARG:1 = ABL番号
;RESULT = 0:低 / 1:中 / 2:高
;閾値: 0 < 1-2 < 3+

@GET_TALENT_BRANCH(ARG, ARG:1)
;ARG = TARGET番号, ARG:1 = TALENT番号
;RESULT = 0:通常 / 1:敏感
;TALENT は 0/1 のバイナリ

@GET_EXP_BRANCH(ARG, ARG:1)
;ARG = TARGET番号, ARG:1 = EXP番号
;RESULT = 0:未経験 / 1:経験少 / 2:経験豊富
;閾値: 0-9 < 10-99 < 100+
```

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | 分岐候補マトリクス完成 | file | grep | contains | "分岐候補マトリクス（確定版）" | [x] |
| 2 | KOJO_KX.ERB ガイド整理 | code | grep | contains | "ABL/TALENT分岐ガイド" | [x] |

### AC Details

**AC1 Test**: `grep "分岐候補マトリクス（確定版）" Game/agents/feature-215.md`
**Expected**: Section header found

**AC2 Test**: `grep "ABL/TALENT分岐ガイド" Game/templates/KOJO_KX.ERB`
**Expected**: Guide section found and organized

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | 1 | 分岐候補マトリクスを完成させる | Game/agents/feature-215.md | [O] |
| 2 | 2 | KOJO_KX.ERB の既存ガイドを整理 | Game/templates/KOJO_KX.ERB | [O] |

---

## Review Notes

<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2025-12-25**: FL loop により Feature 189 から ID 215 に移動。調査・文書化に縮小、実装は F216 に分離。

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2025-12-25 | init | initializer | Status check | READY |
| 2025-12-25 | investigate | explorer | KOJO_KX.ERB analysis | Guide exists, needs update |
| 2025-12-25 | implement | implementer | Update guide comments | SUCCESS |
| 2025-12-25 | verify | ac-tester | AC1+AC2 grep | PASS:2/2 |
| 2025-12-25 | regression | regression-tester | Flow tests | OK:24/24 |
| 2025-12-25 | review | feature-reviewer | Post mode | READY |

---

## Links

- [feature-216.md](feature-216.md) - 分岐判定関数の実装
- [m-orgasm-system.md](designs/m-orgasm-system.md) - v1.8 M絶頂システム
- [v0.8-kojo-80-90.md](designs/v0.8-kojo-80-90.md) - 80-90番台口上計画

---

## Notes

- 本Featureは「調査・文書化」がスコープ
- 実装は Feature 216 で実施
- Feature 189 から ID 競合のため移動（旧189はCOM_71用に予約）
