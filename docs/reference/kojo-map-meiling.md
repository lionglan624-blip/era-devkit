# 1_美鈴 口上マップ

総関数数: **341**

## AC準拠スコア

```
├── 4段階分岐 (TALENT_4):   35 (10%)
├── 3段階分岐 (TALENT/ABL):   0 ( 0%)
├── 1-2段階分岐:            53 (15%)
├── 分岐なし:              238 (69%)
├── 平均口上行数:          1.6行/分岐 (目標4+行)
├── バリエーション:         77 (22%)
├── PRINTDATA使用:          68 (19%)
├── IF RAND使用:             3箇所
└── ELSE分岐あり:          101 (29%)
```

### TALENT段階別カバレッジ

```
├── 恋人分岐あり:   35 (10%)
├── 恋慕分岐あり:   88 (25%)
├── 思慕分岐あり:   35 (10%)
└── ELSE(なし):    101 (29%)
```

**AC準拠率（推定）**: ~14%

## シーンタイプ別

| シーン | 関数数 | 好感度カバー |
|--------|--------|--------------|
| EVENT | 22 | 0/8 (0%) |
| NTR基本 | 32 | 1/8 (12%) |
| WC系 | 96 | 1/8 (12%) |
| お持ち帰り | 18 | 1/8 (12%) |
| セクハラ | 83 | 0/8 (0%) |
| 会話親密 | 34 | 2/8 (25%) |
| 基本口上 | 6 | 0/8 (0%) |
| 愛撫 | 44 | 0/8 (0%) |
| 日常 | 6 | 0/8 (0%) |

## 特殊条件分岐

| 条件 | 出現回数 |
|------|----------|
| 人妻 | 4 |
| 公衆便所 | 22 |
| 処女 | 6 |
| 妊娠 | 1 |
| 恋慕 | 52 |
| 親愛 | 23 |

- soft/hard分岐あり: 0関数

---

## 詳細

> VSCodeの**アウトライン**パネル（左サイドバー）でシーン間を移動できます

### EVENT (22関数)

| 関数名 | 分岐 | 口上行 | 分岐数 | 行/分岐 | RAND | Var | ELSE | 好感度 | 特殊条件 |
|--------|------|--------|--------|---------|------|-----|------|--------|----------|
| `KOJO_K1` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_EVENT_K1_0` | NONE | 7 | 3 | 2.3 |  | ✓ |  | - | - |
| `KOJO_EVENT_K1_1` | NONE | 3 | 1 | 3.0 |  | ✓ |  | - | - |
| `KOJO_EVENT_K1_2` | NONE | 3 | 1 | 3.0 |  | ✓ |  | - | - |
| `KOJO_EVENT_K1_3` | NONE | 2 | 1 | 2.0 |  | ✓ |  | - | - |
| `KOJO_EVENT_K1_4` | NONE | 2 | 1 | 2.0 |  | ✓ |  | - | - |
| `KOJO_EVENT_K1_6` | NONE | 6 | 2 | 3.0 |  | ✓ |  | - | - |
| `KOJO_EVENT_K1_7` | NONE | 7 | 7 | 1.0 |  |  |  | - | - |
| `KOJO_EVENT_K1_8` | NONE | 3 | 1 | 3.0 |  | ✓ |  | - | - |
| `KOJO_EVENT_K1_10` | NONE | 4 | 1 | 4.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COUNTER_K1_29_0` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COUNTER_K1_29_1` | NONE | 5 | 4 | 1.2 |  |  |  | - | - |
| `KOJO_MESSAGE_COUNTER_K1_29_2` | NONE | 2 | 2 | 1.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COUNTER_K1_29_3` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `KOJO_MESSAGE_K1_SeeYou_900_0` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_MESSAGE_K1_SeeYou_900_1` | TALENT_1 | 8 | 6 | 1.3 |  |  | ✓ | - | 恋慕 |
| `KOJO_MESSAGE_K1_SeeYou_900_2` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_MESSAGE_K1_SeeYou_900_3` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `KOJO_MESSAGE_K1_SeeYou_900_4` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `KOJO_MESSAGE_K1_SeeYou_900_5` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `KOJO_MESSAGE_K1_SeeYou_900_9` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `CALLNAME_K1` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |

### 基本口上 (2関数)

| 関数名 | 分岐 | 口上行 | 分岐数 | 行/分岐 | RAND | Var | ELSE | 好感度 | 特殊条件 |
|--------|------|--------|--------|---------|------|-----|------|--------|----------|
| `KOJO_MESSAGE_COM_K1_7` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_7_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |

### 会話親密 (34関数)

| 関数名 | 分岐 | 口上行 | 分岐数 | 行/分岐 | RAND | Var | ELSE | 好感度 | 特殊条件 |
|--------|------|--------|--------|---------|------|-----|------|--------|----------|
| `KOJO_MESSAGE_COM_K1_300` | TALENT_1 | 2 | 1 | 2.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_300_1` | TALENT_4 | 11 | 4 | 2.8 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_301` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_301_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_302` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_302_1` | TALENT_4 | 8 | 5 | 1.6 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_310` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_310_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_311` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_311_1` | TALENT_4 | 8 | 5 | 1.6 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_312` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_312_1` | TALENT_4 | 8 | 5 | 1.6 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_313` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_313_1` | TALENT_4 | 8 | 5 | 1.6 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_314` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_314_1` | TALENT_4 | 8 | 5 | 1.6 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_315` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_315_1` | TALENT_4 | 6 | 5 | 1.2 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_316` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_316_1` | TALENT_4 | 6 | 5 | 1.2 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_300_10_01` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_300_10_...` | NTR_3 | 3 | 3 | 1.0 |  |  | ✓ | FAV_寝取られ寸前 | - |
| `KOJO_MESSAGE_COM_K1_300_10_...` | NTR_3 | 5 | 3 | 1.7 |  |  | ✓ | FAV_寝取られ寸前 | - |
| `KOJO_MESSAGE_COM_K1_300_10_...` | NTR_3 | 6 | 3 | 2.0 |  |  | ✓ | FAV_寝取られ寸前 | - |
| `KOJO_MESSAGE_COM_K1_300_10_02` | NTR_4 | 29 | 27 | 1.1 |  |  | ✓ | FAV_寝取られ寸前 | 妊娠 |
| `KOJO_MESSAGE_COM_K1_300_10_...` | TALENT_1 | 21 | 9 | 2.3 |  |  | ✓ | FAV_寝取られ寸前 | 公衆便所, 親愛, 恋慕 |
| `KOJO_MESSAGE_COM_K1_300_10_...` | NTR_4 | 11 | 11 | 1.0 |  |  | ✓ | FAV_寝取られ寸前 | 公衆便所, 親愛 |
| `KOJO_MESSAGE_COM_K1_300_10_...` | NTR_4 | 30 | 18 | 1.7 |  |  | ✓ | FAV_寝取られ寸前 | 処女, 公衆便所, 親愛 |
| `KOJO_MESSAGE_COM_K1_300_13_01` | TALENT_1 | 15 | 8 | 1.9 |  |  | ✓ | FAV_寝取られ寸前 | 親愛, 恋慕, 人妻 |
| `KOJO_MESSAGE_COM_K1_300_13_02` | TALENT_1 | 10 | 6 | 1.7 |  |  | ✓ | FAV_寝取られ寸前 | 恋慕, 人妻 |
| `KOJO_MESSAGE_COM_K1_300_14_01` | NTR_4 | 17 | 9 | 1.9 |  | ✓ | ✓ | FAV_寝取られ寸前 | 公衆便所, 親愛 |
| `NTR_KOJO_MESSAGE_COM_K1_312` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `NTR_KOJO_MESSAGE_COM_K1_312_1` | NTR_5 | 27 | 12 | 2.2 |  |  | ✓ | FAV_寝取られ | - |
| `NTR_MESSAGE_COM_K1_350_0_3` | TALENT_1 | 7 | 5 | 1.4 |  |  | ✓ | - | 公衆便所, 恋慕, 人妻 |

### 愛撫 (44関数)

| 関数名 | 分岐 | 口上行 | 分岐数 | 行/分岐 | RAND | Var | ELSE | 好感度 | 特殊条件 |
|--------|------|--------|--------|---------|------|-----|------|--------|----------|
| `KOJO_MESSAGE_COM_K1_0` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_0_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_1` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_1_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_2` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_2_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_3` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_3_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_4` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_4_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_5` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_5_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_6` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_6_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_8` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_8_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_9` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_9_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_10` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_10_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_11` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_11_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_20` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_20_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_21` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_21_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_40` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_40_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_41` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_41_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_42` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_42_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_43` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_43_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_44` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_44_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_45` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_45_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_46` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_46_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_47` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_47_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_48` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_48_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |

### 基本口上 (4関数)

| 関数名 | 分岐 | 口上行 | 分岐数 | 行/分岐 | RAND | Var | ELSE | 好感度 | 特殊条件 |
|--------|------|--------|--------|---------|------|-----|------|--------|----------|
| `KOJO_MESSAGE_COM_K1_60` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_60_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |
| `KOJO_MESSAGE_COM_K1_61` | NONE | 0 | 1 | 0.0 |  | ✓ |  | - | - |
| `KOJO_MESSAGE_COM_K1_61_1` | TALENT_4 | 4 | 4 | 1.0 |  | ✓ | ✓ | - | - |

### 日常 (6関数)

| 関数名 | 分岐 | 口上行 | 分岐数 | 行/分岐 | RAND | Var | ELSE | 好感度 | 特殊条件 |
|--------|------|--------|--------|---------|------|-----|------|--------|----------|
| `KOJO_MESSAGE_COM_K1_463_0` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_463_1` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_463_2` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_463_3` | NONE | 4 | 4 | 1.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_463_4` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_MESSAGE_COM_K1_463_5` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |

### NTR基本 (32関数)

| 関数名 | 分岐 | 口上行 | 分岐数 | 行/分岐 | RAND | Var | ELSE | 好感度 | 特殊条件 |
|--------|------|--------|--------|---------|------|-----|------|--------|----------|
| `NTR_KOJO_K1_1` | NTR_7 | 14 | 7 | 2.0 |  |  | ✓ | FAV_寝取られ | 公衆便所, 親愛 |
| `NTR_KOJO_KW1_1` | NTR_7 | 15 | 7 | 2.1 |  |  | ✓ | FAV_寝取られ | - |
| `NTR_KOJO_K1_2_0` | TALENT_1 | 28 | 22 | 1.3 |  | ✓ | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_2_1` | TALENT_1 | 27 | 26 | 1.0 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_2_2` | TALENT_1 | 38 | 30 | 1.3 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_KW1_2_0` | TALENT_1 | 20 | 10 | 2.0 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_KW1_2_1` | TALENT_1 | 18 | 9 | 2.0 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_KW1_2_2` | TALENT_1 | 25 | 10 | 2.5 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_3_0` | NTR_6 | 12 | 6 | 2.0 |  |  | ✓ | FAV_寝取られ | - |
| `NTR_KOJO_K1_3_1` | TALENT_1 | 16 | 10 | 1.6 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_3_2` | TALENT_1 | 17 | 11 | 1.5 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_4_0` | TALENT_1 | 22 | 16 | 1.4 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_4_1` | TALENT_1 | 22 | 16 | 1.4 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_4_2` | TALENT_1 | 22 | 16 | 1.4 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_5_0` | TALENT_1 | 18 | 13 | 1.4 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_5_1` | TALENT_1 | 22 | 13 | 1.7 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_5_2` | TALENT_1 | 16 | 13 | 1.2 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_6_0` | TALENT_1 | 19 | 13 | 1.5 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_6_1` | NTR_5 | 15 | 5 | 3.0 |  |  | ✓ | FAV_寝取られ | - |
| `NTR_KOJO_K1_6_2` | TALENT_1 | 23 | 13 | 1.8 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_7_0` | TALENT_1 | 19 | 13 | 1.5 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_7_1` | TALENT_1 | 19 | 13 | 1.5 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_7_2` | TALENT_1 | 19 | 13 | 1.5 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_11` | NTR_5 | 14 | 13 | 1.1 |  |  | ✓ | FAV_寝取られ | - |
| `NTR_KOJO_K1_12` | TALENT_1 | 9 | 7 | 1.3 |  |  | ✓ | - | 恋慕 |
| `NTR_KOJO_K1_13` | TALENT_1 | 12 | 5 | 2.4 |  |  | ✓ | - | 親愛, 恋慕 |
| `NTR_KOJO_K1_15_0` | TALENT_1 | 18 | 13 | 1.4 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_15_1` | TALENT_1 | 11 | 7 | 1.6 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_15_2` | TALENT_1 | 11 | 7 | 1.6 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_15_3` | TALENT_1 | 11 | 7 | 1.6 |  |  | ✓ | FAV_寝取られ | 恋慕 |
| `NTR_KOJO_K1_16_0` | NONE | 4 | 2 | 2.0 |  |  |  | - | 親愛 |
| `NTR_KOJO_K1_16_1` | NTR_1 | 10 | 5 | 2.0 |  |  | ✓ | FAV_寝取られ | 公衆便所, 親愛 |

### お持ち帰り (18関数)

| 関数名 | 分岐 | 口上行 | 分岐数 | 行/分岐 | RAND | Var | ELSE | 好感度 | 特殊条件 |
|--------|------|--------|--------|---------|------|-----|------|--------|----------|
| `NTR_KOJO_K1_0` | TALENT_1 | 23 | 15 | 1.5 |  | ✓ | ✓ | FAV_寝取られ | 公衆便所, 親愛, 恋慕 |
| `NTR_KOJO_K1_10_0` | TALENT_1 | 25 | 9 | 2.8 |  |  | ✓ | FAV_寝取られ | 公衆便所, 親愛, 恋慕 |
| `NTR_KOJO_K1_10_1` | TALENT_1 | 30 | 19 | 1.6 |  |  | ✓ | FAV_寝取られ | 処女, 公衆便所, 親愛, 恋慕 |
| `NTR_KOJO_K1_10_2` | TALENT_1 | 27 | 17 | 1.6 |  |  | ✓ | FAV_寝取られ | 公衆便所, 親愛, 恋慕 |
| `NTR_KOJO_K1_10_3` | TALENT_1 | 37 | 23 | 1.6 |  |  | ✓ | FAV_寝取られ | 公衆便所, 親愛, 恋慕 |
| `NTR_KOJO_K1_10_4` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `NTR_KOJO_K1_10_4_V` | TALENT_1 | 56 | 34 | 1.6 |  |  | ✓ | FAV_寝取られ | 処女, 公衆便所, 親愛, 恋慕 |
| `NTR_KOJO_K1_10_4_A` | TALENT_1 | 26 | 16 | 1.6 |  |  | ✓ | FAV_寝取られ | 公衆便所, 親愛, 恋慕 |
| `NTR_KOJO_K1_10_5` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `NTR_KOJO_K1_10_5_V` | TALENT_1 | 39 | 22 | 1.8 |  |  | ✓ | FAV_寝取られ | 処女, 公衆便所, 親愛, 恋慕 |
| `NTR_KOJO_K1_10_5_A` | TALENT_1 | 28 | 11 | 2.5 | 1 | ✓ | ✓ | FAV_寝取られ | 公衆便所, 親愛, 恋慕 |
| `NTR_KOJO_K1_14_0` | TALENT_1 | 29 | 19 | 1.5 |  |  | ✓ | FAV_寝取られ | 公衆便所, 親愛, 恋慕 |
| `NTR_KOJO_K1_14_1` | TALENT_1 | 21 | 11 | 1.9 |  |  | ✓ | FAV_寝取られ | 公衆便所, 親愛, 恋慕 |
| `NTR_KOJO_K1_14_2` | TALENT_1 | 21 | 11 | 1.9 |  |  | ✓ | FAV_寝取られ | 公衆便所, 親愛, 恋慕 |
| `NTR_KOJO_K1_19_1` | NONE | 8 | 1 | 8.0 |  |  |  | - | - |
| `NTR_KOJO_K1_19_2` | NONE | 12 | 2 | 6.0 |  |  |  | - | 人妻 |
| `NTR_KOJO_K1_VA_0` | NONE | 8 | 2 | 4.0 |  |  |  | - | 公衆便所, 親愛 |
| `NTR_KOJO_K1_VA_1` | NONE | 3 | 2 | 1.5 |  |  |  | - | 公衆便所, 親愛 |

### セクハラ (83関数)

| 関数名 | 分岐 | 口上行 | 分岐数 | 行/分岐 | RAND | Var | ELSE | 好感度 | 特殊条件 |
|--------|------|--------|--------|---------|------|-----|------|--------|----------|
| `SexHara休憩中_Easy_K1_0` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Easy_K1_1` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Easy_K1_2` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Normal_K1_0` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Normal_K1_1` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Normal_K1_2` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Normal_K1_3` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_0_0` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_0_1` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_0_2` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_0_3` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_0_4` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_1` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_2_0` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_2_1` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_2_2` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_3_0` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_3_1` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_3_2` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_3_3` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_3_4` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_3_5` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Extra_K1_3_6` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Hard_K1_0_0` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Hard_K1_0_1` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `SexHara休憩中_Hard_K1_1_0` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Hard_K1_1_1` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Hard_K1_1_2` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Hard_K1_2_0` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Hard_K1_2_1` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_0_0` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_0_1` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_1_0` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_1_1` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_2_0` | NONE | 5 | 1 | 5.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_2_1` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_3_0` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_3_1` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_4_0` | NONE | 11 | 3 | 3.7 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_4_1` | NONE | 12 | 3 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_4_2` | NONE | 3 | 2 | 1.5 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_5_0` | NONE | 11 | 3 | 3.7 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_5_1` | NONE | 12 | 3 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_90` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_91_0` | NONE | 4 | 2 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_91_1_0` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_91_1_1` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_91_2_0` | NONE | 6 | 3 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_91_2_1` | NONE | 9 | 3 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_92_0_0` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_92_0_1` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_92_0_2` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_92_0_3` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_92_0_5` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_92_0_4` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_92_1_0` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_92_1_1` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_92_1_2` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_92_1_3` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_92_1_4` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_92_1_5` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_0_0` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_0_1` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_0_2` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_0_3` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_0_4` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_0_5` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_1_0` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_1_1` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_1_2` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_1_3` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_1_4` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_1_5` | NONE | 4 | 1 | 4.0 |  |  |  | - | - |
| `SexHara休憩中_Lunatic_K1_93_9` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Phantasm_K1_0_0_0` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Phantasm_K1_0_0_1` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Phantasm_K1_0_0_2` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `SexHara休憩中_Phantasm_K1_0_1_0` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Phantasm_K1_0_1_1` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Phantasm_K1_0_1_2` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Phantasm_K1_0_1_3` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Phantasm_K1_0_1_4` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `SexHara休憩中_Phantasm_K1_0_1_5` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |

### WC系 (96関数)

| 関数名 | 分岐 | 口上行 | 分岐数 | 行/分岐 | RAND | Var | ELSE | 好感度 | 特殊条件 |
|--------|------|--------|--------|---------|------|-----|------|--------|----------|
| `WC_DescriptiveStyle_K1` | TALENT_1 | 0 | 1 | 0.0 |  |  | ✓ | - | 恋慕 |
| `KOJO_WC_COUNTER_com350_K1	;...` | TALENT_1 | 6 | 4 | 1.5 |  |  | ✓ | - | 恋慕 |
| `KOJO_WC_COUNTER_NTRrevelati...` | NONE | 4 | 2 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER_NTRrevelati...` | TALENT_1 | 37 | 12 | 3.1 |  |  | ✓ | FAV_主人より高い | 公衆便所, 恋慕 |
| `KOJO_WC_COUNTER_NTRregain_0...` | NONE | 9 | 4 | 2.2 |  |  |  | - | - |
| `KOJO_WC_COUNTER_NTRregain_1...` | TALENT_1 | 2 | 1 | 2.0 |  |  |  | - | 恋慕 |
| `KOJO_WC_COUNTER_NTRregain_1...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER_NTRregain_1...` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER_NTRregain_2...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER_caretaker_K...` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER10_K1	;;色っぽい仕草` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER12_K1	;;体を摺り寄せる` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER15_K1	;;尻を撫でる` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER16_K1	;;囁く` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER21_1_com301o...` | TALENT_1 | 12 | 2 | 6.0 |  | ✓ | ✓ | - | 恋慕 |
| `KOJO_WC_COUNTER21_1_comOthe...` | TALENT_1 | 23 | 16 | 1.4 | 2 | ✓ | ✓ | - | 恋慕 |
| `KOJO_WC_COUNTER22_0_K1	;;パン...` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER22_1_K1	;;パン...` | TALENT_1 | 2 | 2 | 1.0 |  |  | ✓ | - | 恋慕 |
| `KOJO_WC_COUNTER23_0_com430_...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER23_0_comOthe...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER23_1_K1	;;パン...` | TALENT_1 | 9 | 4 | 2.2 |  |  | ✓ | - | 恋慕 |
| `KOJO_WC_COUNTER24_0_setting...` | NONE | 6 | 3 | 2.0 |  | ✓ |  | - | - |
| `KOJO_WC_COUNTER24_0_setting...` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_0_1stCAP_...` | NONE | 2 | 2 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_1_1stCAP_...` | NONE | 8 | 7 | 1.1 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_0_1stROTO...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_1_1stROTO...` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_0_1stVIB_...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_1_1stVIB_...` | NONE | 8 | 5 | 1.6 |  |  |  | - | 処女 |
| `KOJO_WC_COUNTER24_0_KNOWNs_...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_1_KNOWNs_...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_1_KNOWNs_...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_1_setting...` | NONE | 5 | 5 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_2_setting...` | NONE | 26 | 23 | 1.1 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_3_setting...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_4_setting...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_4_setting...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER24_5_nothing...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_0_setITEM...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_0_stopITE...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_0_stopITE...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_1_stopITE...` | NONE | 3 | 2 | 1.5 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_0_moveITE...` | NONE | 5 | 4 | 1.2 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_1_moveITE...` | NONE | 6 | 5 | 1.2 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_0_VirginR...` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_0_VirginR...` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_1_VirginR...` | NONE | 6 | 3 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_1_VirginR...` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_2_VirginR...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_1_VirginR...` | TALENT_1 | 18 | 9 | 2.0 |  |  | ✓ | - | 恋慕 |
| `KOJO_WC_COUNTER25_0_Chastit...` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER25_0_otherPA...` | NONE | 38 | 27 | 1.4 |  |  |  | - | - |
| `KOJO_WC_COUNTER26_0_change_...` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER26_0_inTOILE...` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER26_1_inTOILE...` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER26_0_inBATHR...` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER26_1_inBATHR...` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER26_2_inBATHR...` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER26_0_ReSET_K...` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER26_0_newSET_...` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER26_1_newSET_...` | NONE | 0 | 1 | 0.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER27_0_K1` | NONE | 3 | 2 | 1.5 |  |  |  | - | - |
| `KOJO_WC_COUNTER27_1_ashamed...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER27_2_ashamed...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER27_3_ashamed...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER27_1_unasham...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER27_4_K1	;;飲尿...` | NONE | 6 | 3 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER27_5_K1	;;飲尿...` | NONE | 5 | 3 | 1.7 |  |  |  | - | 処女 |
| `KOJO_WC_COUNTER41_0_K1	;;強制...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER41_1_K1	;;強制...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER41_2_K1` | NONE | 6 | 3 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER41_3_kiss_K1...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER41_3_gulp_K1...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER42_0_K1	;;強制...` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER43_0_K1	;;強制...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER44_0_K1	;;強制...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER45_0_K1	;;強制...` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER45_1_K1	;;強制...` | NONE | 3 | 2 | 1.5 |  |  |  | - | - |
| `KOJO_WC_COUNTER45_2_milk_K1...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER_ForcedOrgas...` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER40_0_only_K1...` | TALENT_1 | 8 | 2 | 4.0 |  |  | ✓ | - | 恋慕 |
| `KOJO_WC_COUNTER40_1_only_pe...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER40_1_only_sp...` | NONE | 3 | 1 | 3.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER40_0_withOth...` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER40_1_withOth...` | NTR_1 | 17 | 13 | 1.3 |  |  | ✓ | FAV_主人より高い | - |
| `KOJO_WC_COUNTER40_2_withOth...` | NONE | 5 | 1 | 5.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER40_2_item_K1...` | NONE | 2 | 1 | 2.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER40_3_CAMERA_...` | NONE | 3 | 2 | 1.5 |  |  |  | - | - |
| `KOJO_WC_COUNTER40_4_CAMERA_...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER40_5_insert_...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER40_6_item_K1...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER40_7_penis_K...` | NONE | 5 | 2 | 2.5 |  |  |  | - | - |
| `KOJO_WC_COUNTER40_8_penis_K...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER_sexualDesir...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER_sexualDesir...` | NONE | 5 | 1 | 5.0 |  |  |  | - | - |
| `KOJO_WC_COUNTER_sexualDesir...` | NONE | 1 | 1 | 1.0 |  |  |  | - | - |
