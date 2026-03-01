# Feature 215: Kojo ABL/TALENT/EXP Branch Research

**Status**: [DONE]
**Type**: infra
**Priority**: Medium
**Version**: v0.7+

---

## Background

### Philosophy (思想・上位目樁E

口上�E単なるTALENT刁E��（思�E/恋�E/恋人�E�だけでなく、キャラクターの感覚�E感度・中毒�E経験値を反映した多層皁E��刁E��を持つべき。シスチE��が蓄積する変数は口上にも反映されるべきである、E

### Problem (現状の問顁E

1. KOJO_KX.ERB に「ABL/TALENT刁E��ガイド」がコメントとして存在するが、体系皁E��整琁E��されてぁE��ぁE
2. 全COMカチE��リに対してどの変数で刁E��すべきか明確でなぁE
3. 刁E��閾値�E�EBL >= 3 で高、等）が標準化されてぁE��ぁE

### Goal (こ�EFeatureで達�Eすること)

1. 全COMカチE��リの刁E��候補を調査・斁E��化すめE
2. KOJO_KX.ERB の既存ガイドコメントを整琁E�E拡允E��めE
3. 実裁E��様（関数設計）を確定させる ↁEF216 で実裁E

---

## 刁E��候補�Eトリクス�E�確定版�E�E

### ABL�E�感要E中毁E性技�E�E

| ABL | 名前 | 対象COM | 刁E���E容 |
|:---:|------|---------|----------|
| 0 | �E�感要E| 愛撫系(0番台) | 高LVで敏感描�E |
| 1 | �E�感要E| 挿入系(60番台) | 高LVで快感描冁E|
| 2 | �E�感要E| アナル系 | 高LVで快感描冁E|
| 3 | �E�感要E| 胸系 | 高LVで敏感描�E |
| **31** | **精液中毁E* | **口系(81,82,83,85)** | **LV3+で渁E��描�E** |
| 51 | 舁E| 口系(81,82) | 高LVで技巧描�E |
| 54 | 膣 | 挿入系(60番台) | 高LVで締め付け描�E |
| 55 | アナル | アナル系 | 高LVで締め付け描�E |

### TALENT�E�感度�E�E

| TALENT | 名前 | 対象COM | 刁E���E容 |
|:------:|------|---------|----------|
| 101 | �E�感度 | 愛撫系 | 敏感/鈍感で反応変化 |
| 102 | �E�感度 | 挿入系 | 敏感/鈍感で反応変化 |
| 103 | �E�感度 | アナル系 | 敏感/鈍感で反応変化 |
| 104 | �E�感度 | 胸系/道�E系(40番台) | 敏感/鈍感で反応変化 |
| **109** | **�E�感度** | **口系(81,82,85)** | **敏感/鈍感で反応変化** |
| 110 | �E�感度 | 乳首系 | 敏感/鈍感で反応変化 |
| 111 | �E�感度 | 挿入系�E�深部�E�E| ポルチオ快感描冁E|

### EXP�E�経験値�E�E

| EXP | 名前 | 対象COM | 刁E���E容 |
|:---:|------|---------|----------|
| 25 | 口淫経騁E| 口系 | 経験豊富で余裕描冁E|
| 20 | �E�性交経騁E| 挿入系 | 経験豊富で余裕描冁E|
| 21 | �E�性交経騁E| アナル系 | 経験豊富で余裕描冁E|
| 27 | キス経騁E| キス(20) | 経験豊富で積極描�E |

---

## COMカチE��リ別刁E��提桁E

### 優先度: 髁E

| 番台 | カチE��リ | 追加刁E��E|
|:----:|----------|----------|
| 80 | 奉仕系 | ABL:精液中毁E TALENT:�E�感度, EXP:口淫経騁E|
| 60 | 挿入系 | ABL:�E�感要E TALENT:�E�感度/�E�感度, EXP:�E�性交経騁E|
| 40 | 道�E系 | TALENT:対応部位感度 |

### 優先度: 中

| 番台 | カチE��リ | 追加刁E��E|
|:----:|----------|----------|
| 0 | 愛撫系 | ABL:�E�感要E�E�感要E TALENT:�E�感度/�E�感度 |
| 90 | される系 | ABL:吁E��要E TALENT:吁E��度 |
| 100 | SM系 | ABL:マゾっ氁E TALENT:苦痛耐性 |

### 優先度: 佁E

| 番台 | カチE��リ | 追加刁E��E|
|:----:|----------|----------|
| 20 | コミュ系 | EXP:キス経験（キスのみ�E�E|
| 300 | 対あなた系 | 既存で十�E |

---

## Technical Details

### 刁E��判定関数の設計！E216 で実裁E��E

```erb
;============================================
; ABL/TALENT/EXP 刁E��判定関数
;============================================

@GET_ABL_BRANCH(ARG, ARG:1)
;ARG = TARGET番号, ARG:1 = ABL番号
;RESULT = 0:佁E/ 1:中 / 2:髁E
;閾値: 0 < 1-2 < 3+

@GET_TALENT_BRANCH(ARG, ARG:1)
;ARG = TARGET番号, ARG:1 = TALENT番号
;RESULT = 0:通常 / 1:敏感
;TALENT は 0/1 のバイナリ

@GET_EXP_BRANCH(ARG, ARG:1)
;ARG = TARGET番号, ARG:1 = EXP番号
;RESULT = 0:未経騁E/ 1:経験封E/ 2:経験豊寁E
;閾値: 0-9 < 10-99 < 100+
```

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | 刁E��候補�Eトリクス完�E | file | grep | contains | "刁E��候補�Eトリクス�E�確定版�E�E | [x] |
| 2 | KOJO_KX.ERB ガイド整琁E| code | grep | contains | "ABL/TALENT刁E��ガイチE | [x] |

### AC Details

**AC1 Test**: `grep "刁E��候補�Eトリクス�E�確定版�E�E pm/features/feature-215.md`
**Expected**: Section header found

**AC2 Test**: `grep "ABL/TALENT刁E��ガイチE Game/templates/KOJO_KX.ERB`
**Expected**: Guide section found and organized

---

## Tasks

| Task# | AC# | Description | Target | Status |
|:-----:|:---:|-------------|--------|:------:|
| 1 | 1 | 刁E��候補�Eトリクスを完�EさせめE| pm/features/feature-215.md | [O] |
| 2 | 2 | KOJO_KX.ERB の既存ガイドを整琁E| Game/templates/KOJO_KX.ERB | [O] |

---

## Review Notes

<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2025-12-25**: FL loop により Feature 189 から ID 215 に移動。調査・斁E��化に縮小、実裁E�E F216 に刁E��、E

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

- [feature-216.md](feature-216.md) - 刁E��判定関数の実裁E
- [m-orgasm-system.md](../designs/m-orgasm-system.md) - v1.8 M絶頂シスチE��
- [v0.8-kojo-80-90.md](../designs/v0.8-kojo-80-90.md) - 80-90番台口上計画

---

## Notes

- 本Featureは「調査・斁E��化」がスコーチE
- 実裁E�E Feature 216 で実施
- Feature 189 から ID 競合�Eため移動（旧189はCOM_71用に予紁E��E
