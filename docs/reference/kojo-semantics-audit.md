# Kojo Semantics Audit Report

**Feature**: F336 全口上セマンティクス監査・修正・恒久対策
**Created**: 2026-01-04
**Scope**: All kojo implementations with TCVAR:116 = TARGET semantics

---

## Audit Summary

**全19 COM を com-auditor (opus) で監査完了。**

| 区分 | 件数 | 備考 |
|------|:----:|------|
| PASS | 128 | +40 (F338修正) |
| **INCONSISTENT** | **0** | F338で全件修正 |
| NOT_IMPLEMENTED | 62 | - |

**Fixed by**: [Feature 338](../feature-338.md) - 口上セマンティクス不整合修正 (2026-01-04)

---

## COMF Comment/Code Fixes

COMF fixes applied: COMF97 line 67, COMF98 line 66

### COMF97.ERB
- **Line 67**: Changed comment from `;奴隷のＰ⇔調教者のＡの汚れが移動` to `;奴隷のＰ⇔調教者のＶの汚れが移動`
- **Reason**: Code processes `STAIN:PLAYER:Ｖ`, not `STAIN:PLAYER:Ａ`

### COMF98.ERB
- **Line 66**: Changed comment from `;奴隷のＰ⇔調教者のＡの汚れが移動` to `;奴隷のＰ⇔調教者のＶの汚れが移動`
- **Reason**: Code processes `STAIN:PLAYER:Ｖ`, not `STAIN:PLAYER:Ａ`

---

## COM Semantics Analysis

All COM commands where `TCVAR:116 = TARGET` (行為者 = TARGET/キャラクター):

| COM# | Command | TCVAR:116 | Actor | Semantics | STAIN Direction |
|:----:|---------|:---------:|-------|-----------|-----------------|
| 8 | 秘貝開帳 | TARGET | TARGET | TARGET exposes self | - |
| 9 | 自慰 | TARGET | TARGET | TARGET masturbates | - |
| 11 | 乳首吸わせ | TARGET | TARGET | TARGET sucks PLAYER nipple | TARGET:口 ← PLAYER:乳首 |
| 65 | 騎乗位 | TARGET | TARGET | TARGET rides PLAYER (V insertion) | TARGET:Ｖ ← PLAYER:Ｐ |
| 90 | アナル愛撫される | TARGET | TARGET | TARGET caresses PLAYER anus | - |
| 91 | アナル舐めされる | TARGET | TARGET | TARGET licks PLAYER anus | TARGET:口 ← PLAYER:Ａ |
| 92 | Ａ正常位される | TARGET | TARGET | TARGET inserts into PLAYER (A missionary) | TARGET:Ｐ ← PLAYER:Ａ |
| 93 | Ａ後背位される | TARGET | TARGET | TARGET inserts into PLAYER (A doggy) | TARGET:Ｐ ← PLAYER:Ａ |
| 94 | Ａ騎乗位する | TARGET | TARGET | TARGET rides PLAYER (A insertion) | TARGET:Ａ ← PLAYER:Ｐ |
| 95 | 愛撫される | TARGET | TARGET | TARGET caresses PLAYER | - |
| 96 | クンニされる | TARGET | TARGET | TARGET performs cunnilingus on PLAYER | TARGET:口 ← PLAYER:Ｖ |
| 97 | 正常位させる | TARGET | TARGET | TARGET inserts into PLAYER (V missionary) | TARGET:Ｐ ← PLAYER:Ｖ |
| 98 | 後背位させる | TARGET | TARGET | TARGET inserts into PLAYER (V doggy) | TARGET:Ｐ ← PLAYER:Ｖ |
| 99 | 騎乗位する | TARGET | TARGET | TARGET rides PLAYER (V insertion) | TARGET:Ｐ ← PLAYER:Ｖ |
| 120 | クンニ強制 | TARGET | TARGET | TARGET forced to perform cunnilingus | TARGET:口 ← PLAYER:Ｖ |
| 122 | 助手を犯させる | TARGET | TARGET | TARGET penetrates ASSISTANT | TARGET:Ｐ → ASSI:Ｖ |
| 126 | 乳の揉み合い | TARGET | TARGET | TARGET massages PLAYER breasts mutually | - |
| 140 | イラマチオ | TARGET | TARGET | TARGET facefucked by PLAYER | TARGET:口 ← PLAYER:Ｐ |
| 145 | アナル奉仕 | TARGET | TARGET | TARGET services PLAYER anus with tongue | TARGET:口 ← PLAYER:Ａ |
| 160 | 膣内観察 | TARGET | TARGET | TARGET observes PLAYER vagina | - |

---

## Full Audit Results (com-auditor opus)

| COM | Command | PASS | INCONSISTENT | NOT_IMPL |
|:---:|---------|:----:|:------------:|:--------:|
| 8 | 秘貝開帳 | 8 | 0 | 2 |
| 9 | 自慰 | 10 | 0 | 0 |
| **11** | **乳首吸わせ** | 1 | **9** | 0 |
| 65 | 騎乗位 | 10 | 0 | 0 |
| **90** | **アナル愛撫される** | 4 | **6** | 0 |
| 91 | アナル舐めされる | 10 | 0 | 0 |
| **92** | **Ａ正常位される** | 3 | **7** | 0 |
| **93** | **Ａ後背位される** | 3 | **7** | 0 |
| **94** | **Ａ騎乗位する** | 9 | **1** | 0 |
| 95 | 愛撫される | 10 | 0 | 0 |
| **96** | **クンニされる** | 9 | **1** | 0 |
| **97** | **正常位させる** | 1 | **9** | 0 |
| 99 | 騎乗位する | 0 | 0 | 10 |
| 120 | クンニ強制 | 0 | 0 | 10 |
| 122 | 助手を犯させる | 0 | 0 | 10 |
| 126 | 乳の揉み合い | 0 | 0 | 10 |
| 140 | イラマチオ | 0 | 0 | 10 |
| 145 | アナル奉仕 | 0 | 0 | 10 |
| 160 | 膣内観察 | 0 | 0 | 10 |
| **Total** | - | **88** | **40** | **62** |

---

## Fix Candidates (40件)

### COM 11 (乳首吸わせ) - 9件

TARGET sucks PLAYER's nipple. K7 のみ PASS。

| COM# | Character | File | Issue |
|:----:|-----------|------|-------|
| 11 | K1 (美鈴) | KOJO_K1_愛撫.ERB | Inverted: PLAYER sucks TARGET's nipple |
| 11 | K2 (小悪魔) | KOJO_K2_愛撫.ERB | Inverted: PLAYER sucks TARGET's nipple |
| 11 | K3 (パチュリー) | KOJO_K3_愛撫.ERB | Inverted: PLAYER sucks TARGET's nipple |
| 11 | K4 (咲夜) | KOJO_K4_愛撫.ERB | Inverted: PLAYER sucks TARGET's nipple |
| 11 | K5 (レミリア) | KOJO_K5_愛撫.ERB | Inverted: PLAYER sucks TARGET's nipple |
| 11 | K6 (フランドール) | KOJO_K6_愛撫.ERB | Inverted: PLAYER sucks TARGET's nipple |
| 11 | K8 (チルノ) | KOJO_K8_愛撫.ERB | Inverted: PLAYER sucks TARGET's nipple |
| 11 | K9 (大妖精) | KOJO_K9_愛撫.ERB | Inverted: PLAYER sucks TARGET's nipple |
| 11 | K10 (魔理沙) | KOJO_K10_愛撫.ERB | Inverted: PLAYER sucks TARGET's nipple |

### COM 90 (アナル愛撫される) - 6件

TARGET caresses PLAYER's anus. K2, K5, K6, K10 は PASS。

| COM# | Character | File | Issue |
|:----:|-----------|------|-------|
| 90 | K1 (美鈴) | KOJO_K1_口挿入.ERB | Inverted: MASTER caresses TARGET's anus |
| 90 | K3 (パチュリー) | KOJO_K3_口挿入.ERB | Inverted: MASTER caresses TARGET's anus |
| 90 | K4 (咲夜) | KOJO_K4_口挿入.ERB | Inverted: MASTER caresses TARGET's anus |
| 90 | K7 (妖精メイド) | KOJO_K7_口挿入.ERB | Inverted: MASTER caresses TARGET's anus |
| 90 | K8 (チルノ) | KOJO_K8_口挿入.ERB | Inverted: MASTER caresses TARGET's anus |
| 90 | K9 (大妖精) | KOJO_K9_口挿入.ERB | Inverted: MASTER caresses TARGET's anus |

### COM 92 (Ａ正常位される) - 7件

TARGET inserts into PLAYER's anus. K3, K6, K7 は PASS。

| COM# | Character | File | Issue |
|:----:|-----------|------|-------|
| 92 | K1 (美鈴) | KOJO_K1_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |
| 92 | K2 (小悪魔) | KOJO_K2_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |
| 92 | K4 (咲夜) | KOJO_K4_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |
| 92 | K5 (レミリア) | KOJO_K5_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |
| 92 | K8 (チルノ) | KOJO_K8_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |
| 92 | K9 (大妖精) | KOJO_K9_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |
| 92 | K10 (魔理沙) | KOJO_K10_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |

### COM 93 (Ａ後背位される) - 7件

TARGET inserts into PLAYER's anus (doggy). K3, K6, K7 は PASS。

| COM# | Character | File | Issue |
|:----:|-----------|------|-------|
| 93 | K1 (美鈴) | KOJO_K1_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |
| 93 | K2 (小悪魔) | KOJO_K2_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |
| 93 | K4 (咲夜) | KOJO_K4_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |
| 93 | K5 (レミリア) | KOJO_K5_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |
| 93 | K8 (チルノ) | KOJO_K8_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |
| 93 | K9 (大妖精) | KOJO_K9_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |
| 93 | K10 (魔理沙) | KOJO_K10_口挿入.ERB | Inverted: MASTER inserts into TARGET's anus |

### COM 94 (Ａ騎乗位する) - 1件

TARGET rides PLAYER (anal). K6 のみ INCONSISTENT。

| COM# | Character | File | Issue |
|:----:|-----------|------|-------|
| 94 | K6 (フランドール) | KOJO_K6_口挿入.ERB | Inverted: Describes TARGET inserting instead of receiving |

### COM 96 (クンニされる) - 1件

TARGET performs cunnilingus on PLAYER. K3 のみ INCONSISTENT。

| COM# | Character | File | Issue |
|:----:|-----------|------|-------|
| 96 | K3 (パチュリー) | KOJO_K3_口挿入.ERB | Inverted: MASTER performs on TARGET |

### COM 97 (正常位させる) - 9件

TARGET inserts into PLAYER. K1 のみ PASS。

| COM# | Character | File | Issue |
|:----:|-----------|------|-------|
| 97 | K2 (小悪魔) | KOJO_K2_口挿入.ERB | Inverted: TARGET receiving instead of inserting |
| 97 | K3 (パチュリー) | KOJO_K3_口挿入.ERB | Inverted: TARGET receiving instead of inserting |
| 97 | K4 (咲夜) | KOJO_K4_口挿入.ERB | Inverted: TARGET receiving instead of inserting |
| 97 | K5 (レミリア) | KOJO_K5_口挿入.ERB | Inverted: TARGET receiving instead of inserting |
| 97 | K6 (フランドール) | KOJO_K6_口挿入.ERB | Inverted: TARGET receiving instead of inserting |
| 97 | K7 (妖精メイド) | KOJO_K7_口挿入.ERB | Inverted: TARGET receiving instead of inserting |
| 97 | K8 (チルノ) | KOJO_K8_口挿入.ERB | Inverted: TARGET receiving instead of inserting |
| 97 | K9 (大妖精) | KOJO_K9_口挿入.ERB | Inverted: TARGET receiving instead of inserting |
| 97 | K10 (魔理沙) | KOJO_K10_口挿入.ERB | Inverted: TARGET receiving instead of inserting |

---

## Already Fixed

### F327 で修正済み (COM 98)

| COM# | Character | Status |
|:----:|-----------|:------:|
| 98 | K2, K4, K5, K6, K9 | FIXED |

---

## Root Cause Analysis

### Primary Causes

1. **Missing COMF Reading Step in kojo-writer.md Workflow**
2. **eraTW Cache Absence** for newer COMs
3. **Command Name Ambiguity** (「させる」「される」の罠)
4. **COMF Comment Errors** (A↔V コピペミス)

### Affected Patterns

| Pattern | Issue | Affected COMs |
|---------|-------|---------------|
| 「させる」系 | 命名が MASTER 視点、実際は TARGET が行為者 | 97 |
| 「される」系 | 受動態に見えるが TARGET が行為者 | 90, 92, 93, 96 |
| 「吸わせ」系 | 使役形だが TARGET が吸う側 | 11 |
| 騎乗位系 | TARGET が上、だが挿入/被挿入が混同 | 94 |

---

## Action Required

**Follow-up Feature**: [feature-338.md](../feature-338.md)

40件の INCONSISTENT を kojo-writer で再作成する。

---

## Audit Metadata

- **Auditor**: com-auditor (opus)
- **Date**: 2026-01-04
- **Feature**: F336
- **Scope**: 全19 COM × 10キャラ = 190 監査ポイント
- **Status**: Complete
- **Follow-up**: F338 (40件修正)
- **JSON Output**: `_out/logs/audit/com-*.json`
