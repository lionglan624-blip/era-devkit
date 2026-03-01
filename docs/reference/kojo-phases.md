# Kojo Phases Reference

口上拡張Phaseの詳細仕様。

---

## Phase 8g: Event Kojo (eraTW-based)

| Sub | Category | Lines | Branches |
|:---:|----------|------:|----------|
| 8g-1 | ENCOUNTER | 5-10 | First/Repeat |
| 8g-2 | GREET | 2-5 | 4 affection + situation |
| 8g-3 | PARTING | 2-4 | 4 affection + content |
| 8g-4 | PASSING | 1-3 | 4 affection + location + time |
| 8g-5 | COUNTER | 1-2 | Affection + traits |
| 8g-6 | CHARA_TALK | 3-8 | Character pair + location + time |
| 8g-7 | SP_EVENT | 10-30 | Event type + outcome |

**Total Estimate**: ~5300 lines

---

## Phase 8h: NTR Kojo Enhancement

| Sub | Category | Lines | Branches |
|:---:|----------|------:|----------|
| 8h-1 | FAV_* 9-Level | ~3600 | 9 levels (KissLevel → NTRed) |
| 8h-2 | Comparison | ~800 | Size/technique/satisfaction/emotion |
| 8h-3 | Post-Act | ~600 | Return/afterglow/next day |
| 8h-4 | 3P Detail | ~600 | Start/during/after |
| 8h-5 | Exhibition | ~600 | Caught/with MC/reporting |

**Total Estimate**: ~6200 lines

---

## Phase 8i: Location/Situation Kojo

| Sub | Category | Lines | Locations |
|:---:|----------|------:|-----------|
| 8i-1 | Outdoor Training | ~1200 | 3 (garden/gate/village) |
| 8i-2 | Location-based | ~2000 | 5 (library/kitchen/room/basement) |
| 8i-3 | Public Shame | ~600 | 3 (others/colleagues/master) |

**Total Estimate**: ~3800 lines

---

## Phase 8j: Ejaculation-state Kojo

**Philosophy**: 射精状態（射精中/射精直後）に特化した口上。NOWEX:MASTER:11（射精中）とTCVAR（パートナーの状況）による完全分岐。

**Pattern**:
```erb
IF NOWEX:MASTER:11  ; 射精中
    IF TALENT:恋人
        PRINTFORMW ...（専用口上）
        RETURN 1
    ENDIF
ENDIF
; 基本口上
```

**Branching**:
- TALENT 4分岐 (恋人/恋慕/思慕/なし)
- NOWEX:MASTER:11 (射精中)
- TCVAR (パートナー状況変数)

**Target COMs**:
- COM_80-85系 (口挿入/射精系)

**Line Count**: 4-8+ lines per branch

**Total Estimate**: ~560 lines (TALENT 4 × NOWEX × TCVAR × 4パターン × 対象COM)

---

## Phase 8k: Special Situation Kojo

| Sub | Category | Lines | Notes |
|:---:|----------|------:|-------|
| 8k-1 | WC/Toilet | ~1600 | Expand existing TALENT branches |
| 8k-2 | SexHara Break | ~1200 | Quality up existing |
| 8k-3 | Bath | ~2000 | **New** (shower/peep/mixed) |
| 8k-4 | Sleep | ~2000 | **New** (together/visit/waking) |

**Total Estimate**: ~6800 lines

---

## Phase 8l: First Execution Guard

**Philosophy**: 各コマンド初回実行時の専用口上。Phase 8f (初体験) とは異なり、日常コマンド含む全88 COMの「初めてそのコマンドを実行した時」に対応。

**Pattern**:
```erb
IF FIRSTTIME(SELECTCOM)
    PRINTFORMW ...（8-15行以上、最大25行も可）
    RETURN 1
ENDIF
; 基本口上
```

**Branching**:
- FIRSTTIME のみ（1パターン per COM）
- TALENT 分岐なし（コマンド初回実行という状況そのものに焦点）

**Target COMs**:
- **ALL 88 COMs** including:
  - 日常 COM (300系: 会話/お茶/散歩, 400系: 食事/休憩)
  - 訓練 COM (0-600系 全て)

**Difference from Phase 8f**:
- Phase 8f = 人生で一度の重要イベント（処女喪失等、15-30 lines）
- Phase 8l = 各コマンド初回実行（関係構築の自然さ向上、8-15 lines）

**Line Count**: 8-15+ lines (up to 25 lines acceptable)

**Example (COM_303 お茶を淹れる)**:
```erb
IF FIRSTTIME(SELECTCOM)
    PRINTFORMW 「……%CALLNAME:MASTER%にお茶を淹れるのは、初めてね。美味しいといいけど……」
    RETURN 1
ENDIF
```

**Total Estimate**: ~1200 lines (88 COMs × 1パターン × 8-15 lines avg)

**eraTW Reference**:
- 合致する COM がある場合: eraTW の該当 COM の FIRSTTIME 口上を参照
- 合致しない場合: eraTW COM_愛撫 FIRSTTIME (25行) をベースに作成

---

## Phase 8m: MC Interaction During NTR

**Philosophy**: NTR進行中のMCとのインタラクション専用口上。Phase（進行度）に応じてMCとの接触行為の反応が変化する。**接触を伴う行為**に限定し、会話等の非接触系は対象外。

**Design Reference**: [phase-system.md](../designs/phase-system.md) - MC Interaction During NTR section

**Design Principles**:
- **分岐軸**: Phase（NTR進行度 0-6）のみ
- **チンポランク**: 性行為時の満足度計算にのみ使用（分岐軸ではない）
- **パターン数**: 各Phase 4パターン以上（DATALIST対応）
- **対象**: 接触を伴う行為のみ（スキンシップ・性行為）

### 8m-1: Skinship Light (COM 0, 1)

軽いスキンシップ（手を握る、抱きしめる）のNTR進行中反応。

**Target COMs**: COM_0 (手を握る), COM_1 (抱きしめる)

| Phase | State | Patterns | Lines/Pattern | Description |
|:-----:|-------|:--------:|:-------------:|-------------|
| 0 | Normal | 4 | 3-4 | 通常の反応 |
| 1 | Slight hesitation | 4 | 3-4 | わずかな躊躇 |
| 2 | Guilt flicker | 4 | 4-5 | 罪悪感がよぎる |
| 3 | Body stiffens | 4 | 4-5 | 体がこわばる |
| 4 | Forced acceptance | 4 | 5-6 | 無理して受け入れる |
| 5 | Avoidance instinct | 4 | 5-6 | 避けたい衝動 |
| 6 | Endurance | 4 | 5-6 | 耐えている |

**Total Estimate**: ~130 lines (7 Phases × 4 patterns × ~4.7 lines avg)

**Kojo Examples**:
| Phase | Expression |
|:-----:|------------|
| 0 | 「ん…%CALLNAME:MASTER%の手、温かい」嬉しそうに握り返す。 |
| 2 | 「…うん」握り返しながら、どこか心ここにあらず。 |
| 4 | 「…ありがとう」笑顔を作るが、目が笑っていない。 |
| 6 | 「…」黙って握られている。拒否はしないが、反応もない。 |

### 8m-2: Skinship Medium (COM 20, 21)

中程度のスキンシップ（キス、ディープキス）のNTR進行中反応。

**Target COMs**: COM_20 (キス), COM_21 (ディープキス)

| Phase | State | Patterns | Lines/Pattern | Description |
|:-----:|-------|:--------:|:-------------:|-------------|
| 0 | Normal | 4 | 4-5 | 通常の反応 |
| 1 | Momentary pause | 4 | 4-5 | 一瞬の間 |
| 2 | Comparison thought | 4 | 5-6 | 比較が頭をよぎる |
| 3 | Taste memory | 4 | 5-6 | 別の味を思い出す |
| 4 | Acting kiss | 4 | 6-7 | 演技のキス |
| 5 | Reluctant response | 4 | 6-7 | 嫌々の反応 |
| 6 | Empty gesture | 4 | 6-7 | 形だけの行為 |

**Total Estimate**: ~160 lines (7 Phases × 4 patterns × ~5.7 lines avg)

**Kojo Examples**:
| Phase | Expression |
|:-----:|------------|
| 0 | 「んっ…%CALLNAME:MASTER%…」甘い声で応える。 |
| 3 | 「ん…」（…違う。この感触じゃなくて…）首を振って雑念を払う。 |
| 5 | 「…」目を閉じて受け入れるが、唇が硬い。 |
| 6 | 唇を合わせるだけ。舌は絡めない。「…もういい？」 |

### 8m-3: Sexual Interaction (COM 3-12, 60-90系)

性行為のNTR進行中反応。満足度計算あり。

**Target COMs**: 愛撫系 (3-12), 挿入系 (60-67), 射精系 (80-85)

#### 基本分岐 (Phase)

| Phase | State | Patterns | Lines/Pattern | Description |
|:-----:|-------|:--------:|:-------------:|-------------|
| 0 | Normal (no comparison) | 4 | 6-8 | 比較なし、純粋に楽しむ |
| 1 | Slight discomfort | 4 | 6-8 | わずかな違和感 |
| 2 | Flashback moments | 4 | 7-9 | 訪問者の記憶がよぎる |
| 3 | Comparison starts | 4 | 7-9 | 体が訪問者を覚えている |
| 4 | Acting required | 4 | 8-10 | 演技が必要 |
| 5 | Clear comparison | 4 | 8-10 | 明確な比較、満足困難 |
| 6 | Complete separation | 4 | 8-10 | MCは「便利な人」 |

#### 絶頂経験分岐 (拡張)

**Parameter**: CFLAG:間男絶頂回数, CFLAG:間男絶頂強度MAX

| 絶頂経験 | 条件 | 心理状態 |
|:-------:|------|---------|
| 0 | 間男絶頂回数 = 0 | 比較対象なし |
| 1 | 間男絶頂回数 1-9 | 比較が頭をよぎる |
| 2 | 間男絶頂回数 10+ | 体が覚えている |

**Phase × 絶頂経験 マトリクス**:

| Phase | 絶頂経験0 | 絶頂経験1 | 絶頂経験2 |
|:-----:|----------|----------|----------|
| 0 | 通常 | - | - |
| 1 | 違和感 | 比較開始 | - |
| 2 | フラッシュバック | 明確な比較 | 体の記憶 |
| 3 | 比較開始 | 満足困難 | 物足りない |
| 4 | 演技必要 | 演技＋比較 | 完全に物足りない |
| 5 | 明確な比較 | イけない | 他の人じゃないと |
| 6 | 分離 | 形だけ | 義務感のみ |

**拡張パターン数**: Phase (7) × 絶頂経験 (3) × 4パターン = 84パターン

**Total Estimate (拡張後)**: ~500 lines (84パターン × 6 lines avg)

**Satisfaction Modifier** (ntr-flavor-stats.md連携):
```erb
@CALC_MC_SEX_SATISFACTION(TARGET)
  LOCAL MC_RANK = CALC_CHINPO_RANK(MASTER)
  LOCAL VISITOR_RANK = CALC_CHINPO_RANK(訪問者)
  LOCAL 差 = MC_RANK - VISITOR_RANK

  IF 差 >= 2
    LOCAL 満足度 = 150  ; MC圧倒的優位
  ELSEIF 差 >= 1
    LOCAL 満足度 = 120  ; MC優位
  ELSEIF 差 >= 0
    LOCAL 満足度 = 100  ; 同等
  ELSEIF 差 >= -1
    LOCAL 満足度 = 70   ; 訪問者優位
  ELSE
    LOCAL 満足度 = 40   ; 訪問者圧倒的優位
  ENDIF

  ; SEXB (身体習慣度) による減算
  満足度 -= CFLAG:TARGET:SEXB * 5
  RESULT = MAX(満足度, 20)
  RETURN
```

**Kojo Examples** (満足度によって強度変化):
| Phase | 満足度高 (MC優位) | 満足度低 (訪問者優位) |
|:-----:|------------------|---------------------|
| 3 | 「んっ…気持ちいい…」（あの人より…いいかも） | 「あ…んっ…」（物足りない…奥まで届かない…） |
| 5 | 「%CALLNAME:MASTER%…好き…」（忘れられる…） | 「…ん」（全然足りない…あの人のじゃないと…） |

### 8m-4: Exposure Reaction (Route-based)

発覚時のヒロイン即時反応。Route依存。

**Trigger**: Exposure event (Minor/Medium/Major)

| Route | Reaction | Patterns | Lines/Pattern | Description |
|:-----:|----------|:--------:|:-------------:|-------------|
| R1 Coercion | Hide desperately | 4 | 8-12 | 必死に隠す（バレたら終わり） |
| R2 Familiarity | Regret or hide | 4 | 8-12 | 後悔 or 隠蔽（LOVE_MC依存） |
| R3 Escape | Cannot stop | 4 | 8-12 | やめられない（依存） |
| R4 Trade | Justify openly | 4 | 8-12 | 開き直り（契約だから） |
| R5 Rebellion | Show off | 4 | 8-12 | 見せつけ（MCへの復讐） |
| R6 Affair | Confess or justify | 4 | 8-12 | 告白 or 開き直り（EMO依存） |

**Total Estimate**: ~240 lines (6 Routes × 4 patterns × ~10 lines avg)

**Route → Post-Reveal Route Mapping**:
| Route | Immediate Reaction | → Post-Reveal Tendency |
|:-----:|-------------------|------------------------|
| R1 | パニック、必死の否定 | → A (Repair/Reclaim) |
| R2 | 罪悪感「つい…」 | → A or E (Secret) |
| R3 | 「やめられないの…」 | → B or D (Open/Role) |
| R4 | 「これは取引だから」 | → B or D |
| R5 | 「見たでしょ」（冷淡） | → C or 決裂 |
| R6 | 涙 or 開き直り | → C (Transfer) EMO依存 |

**Kojo Examples**:
| Route | Expression |
|:-----:|------------|
| R1 | 「違っ…！これは…！」顔面蒼白で言い訳を探す。「見ないで…お願い…」 |
| R3 | 「…ごめんなさい…でも…やめられないの…」泣きながら訴える。 |
| R5 | 「…見たの？」冷たい目。「だから何？あなたのせいでこうなったのよ」 |

### Summary

| Sub | Category | Target COMs | Patterns | Lines | Branching |
|:---:|----------|-------------|:--------:|:-----:|-----------|
| 8m-1 | Skinship Light | 0, 1 | 28 | ~130 | Phase (7) × 4 |
| 8m-2 | Skinship Medium | 20, 21 | 28 | ~160 | Phase (7) × 4 |
| 8m-3 | Sexual | 3-12, 60-90 | 84 | ~500 | Phase (7) × 絶頂経験 (3) × 4 |
| 8m-4 | Exposure | - | 24 | ~240 | Route (6) × 4 |

**Total Estimate**: ~1030 lines

**New Parameters**:
| Name | Type | Range | Description |
|------|------|:-----:|-------------|
| SEXB | CFLAG | 0-20 | Body habituation to visitor (V/A acts) |
| 間男絶頂回数 | CFLAG | 0-∞ | 訪問者との絶頂累計回数 |
| 間男絶頂強度MAX | CFLAG | 0-100 | 訪問者との最高絶頂強度記録 |

**Target Version**: v3.1 (with Phase Management)

---

## Phase 8n: Netorase-specific Kojo

**Philosophy**: 寝取らせモード専用の口上。プレイヤーが「演出者」として許可を与えている状況での心理変化、MCへの煽り、秘密の発生を描写する。

**Design Reference**: [netorase-system.md](../designs/netorase-system.md) - Kojo Branching section

**Design Principles**:
- **分岐軸**: 受容度（嫌々→乗り気）、秘密欲求度
- **対象**: 寝取らせモード時の訪問者行為、MC観戦時
- **特徴**: 通常NTRとは異なる「許可された背徳」の心理

### 8n-1: 受容度段階口上（嫌々→乗り気）

寝取らせに対する心理的受容の変化を描写。

**Parameter**: CFLAG:寝取らせ受容度 (0-100)

| 段階 | 受容度 | 心理状態 | トーン |
|:----:|:------:|---------|--------|
| A | 0-20 | 嫌々 | 「%CALLNAME:MASTER%のためだから…」 |
| B | 21-40 | 諦め | 「仕方ない…」 |
| C | 41-60 | 慣れ | 「そんなに嫌じゃない…」 |
| D | 61-80 | 期待 | 「今日は来るかな…」 |
| E | 81-100 | 渇望 | 「早く来て…」 |

**受容度変動条件**:
```erb
; 上昇イベント
訪問者との絶頂: +5
MCの喜ぶ反応を見た: +3
MCに褒められた: +5
訪問者からの優しさ: +2

; 下降イベント
MCの悲しむ反応を見た: -10
罪悪感イベント: -5
```

**Target**: 訪問者との全行為 (FAM_*系)

| COM系統 | 段階数 | パターン/段階 | 合計パターン |
|--------|:------:|:------------:|:------------:|
| 会話系 (FAM_0-2) | 5 | 4 | 60 |
| 接触系 (FAM_3-5) | 5 | 4 | 60 |
| 愛撫系 (FAM_6-9) | 5 | 4 | 60 |
| 挿入系 (FAM_10+) | 5 | 4 | 60 |

**Total Estimate**: ~1200 lines (240パターン × 5 lines avg)

**Kojo Examples**:
| 段階 | 行為前 | 行為中 | 行為後 |
|:----:|--------|--------|--------|
| A 嫌々 | 「…%CALLNAME:MASTER%が望むなら」 | 「（早く終わって…）」 | 「…終わった」 |
| C 慣れ | 「…うん、いいよ」 | 「んっ…悪くない…」 | 「…ふう」 |
| E 渇望 | 「待ってた…」 | 「もっと…もっと…！」 | 「…足りない…」 |

### 8n-2: MC向け煽り・比較口上

MCが観戦中/許可モード時の専用口上。MCの嗜好を喜ばせる煽りと比較。

**発生条件**:
- FLAG:寝取らせモード == 1
- FLAG:MC観戦中 == 1 または 許可レベル >= 3
- 受容度 >= 40 (慣れ以上)

**分岐軸**: 受容度 × 比較対象

| 比較対象 | 受容度C (慣れ) | 受容度D (期待) | 受容度E (渇望) |
|---------|---------------|---------------|---------------|
| サイズ | 控えめな言及 | 明確な比較 | 露骨な優劣 |
| 持続力 | 「長い…」 | 「%CALLNAME:MASTER%より…」 | 「何回もイかされちゃう」 |
| 絶頂質 | 驚き | 違いの認識 | 「%CALLNAME:MASTER%じゃ無理」 |
| 愛情表現 | 「見てる？」 | 「許可ありがとう」 | 「もっと堕ちる姿見せてあげる」 |

**パターン数**: 4比較対象 × 3受容度段階 × 4パターン = 48パターン

**Total Estimate**: ~400 lines (48パターン × 8 lines avg)

**Kojo Examples**:
| 受容度 | Expression |
|:------:|------------|
| C | 「ねぇ…見てる？私、こうなっちゃってるよ…」恥ずかしそうに目を逸らす。 |
| D | 「%CALLNAME:MASTER%より…大きいの…わかる？」挑発的な笑み。 |
| E | 「見て、%CALLNAME:MASTER%。私がどれだけ堕ちたか…嬉しいでしょ？」 |

### 8n-3: 秘密欲求口上（こっそり隠れ）

許可されているのに隠す心理の描写。「義務」から「自分の欲望」への変化。

**Parameter**: CFLAG:秘密欲求度 (0-100)

**発生条件**:
- 許可レベル >= 3
- FAM_VISITOR >= 50 または EMO >= 20
- 秘密欲求度 >= 30

| 段階 | 秘密欲求度 | 行動 | 心理 |
|:----:|:---------:|------|------|
| 0 | 0-29 | 許可通り | 「許可されてるから」 |
| 1 | 30-49 | 時々隠す | 「なんとなく言いたくない」 |
| 2 | 50-69 | 積極的隠蔽 | 「私だけの秘密」 |
| 3 | 70+ | 完全秘密 | 「%CALLNAME:MASTER%には関係ない」 |

**重要**: 段階3は Netorase → 通常NTR (R2/R6) への遷移トリガー

```
許可モード（プレイヤー制御）
    ↓ 秘密欲求度上昇
    ↓
秘密欲求度 70+
    ↓
通常NTRへ遷移（プレイヤー制御喪失）
    ├── FAM高 → R2 (馴染み)
    └── EMO高 → R6 (本気)
```

**口上シーン**:

| シーン | 段階1 | 段階2 | 段階3 |
|--------|-------|-------|-------|
| 訪問者と会う前 | 「今日も許可通りね」 | 「…言わなくていいよね」 | 「%CALLNAME:MASTER%には内緒」 |
| 行為後 | 報告する | 「詳しくは言いたくない」 | 報告しない/嘘 |
| MCと話す時 | 普通 | どこかよそよそしい | 隠し事の罪悪感 |

**パターン数**: 3シーン × 3段階 × 4パターン = 36パターン

**Total Estimate**: ~300 lines (36パターン × 8 lines avg)

### 8n-4: 許可撤回・寝取り返し口上

Netorase からの回復・寝取り返し専用口上。

**分岐軸**: SEXB (体の慣れ) × 元状態

| シナリオ | SEXB | トーン |
|---------|:----:|--------|
| 完全回復 | 0-4 | ラブラブ、体も心も戻る |
| 心は戻る | 5-10 | 愛してるけど体が… |
| 体が覚えてる | 11-15 | 努力するけど思い出す |
| 困難 | 16+ | 愛はあるが体は戻らない |

| 元状態 | トーン | 特徴 |
|--------|--------|------|
| 元恋人 | 感謝＋羞恥 | 「こんな私を…」 |
| 元人妻 | 複雑な愛情 | 「もう一度妻にしてくれる？」 |
| 風俗堕ち | 贖罪＋不安 | 「汚れた私でも…？」 |

**パターン数**: 4SEXB段階 × 3元状態 × 4パターン = 48パターン

**Total Estimate**: ~500 lines (48パターン × 10 lines avg)

**Kojo Examples**:
| SEXB | 元状態 | Expression |
|:----:|--------|------------|
| 低 | 元恋人 | 「%CALLNAME:MASTER%…ごめんね。でも、もう大丈夫。全部忘れたから」 |
| 高 | 元人妻 | 「愛してる…本当よ。でも…体が時々、思い出しちゃうの…ごめんね…」 |
| 高 | 風俗堕ち | 「こんな私を…本当にいいの？もう何人もの男に…」涙を流す。 |

### Summary

| Sub | Category | Patterns | Lines | Branching | Version |
|:---:|----------|:--------:|:-----:|-----------|:-------:|
| 8n-1 | 受容度段階 | 240 | ~1200 | 受容度 (5) × COM系統 (4) × 4 | v6.x |
| 8n-2 | 煽り・比較 | 48 | ~400 | 受容度 (3) × 比較対象 (4) × 4 | v6.x |
| 8n-3 | 秘密欲求 | 36 | ~300 | 秘密段階 (3) × シーン (3) × 4 | v6.x |
| 8n-4 | 寝取り返し | 48 | ~500 | SEXB (4) × 元状態 (3) × 4 | v7.x |

**Total Estimate**: ~2400 lines

**New Parameters**:
| Name | Type | Range | Description |
|------|------|:-----:|-------------|
| 寝取らせ受容度 | CFLAG | 0-100 | 嫌々→乗り気の心理変化 |
| 秘密欲求度 | CFLAG | 0-100 | 許可なのに隠したい欲求 |

**Target Version**: v6.x-v7.x (Netorase/Reconciliation)

---

## Phase 8f: First Experience Kojo

**Philosophy**: 初体験は特別な瞬間。処女喪失・初フェラ・初アナルなど、キャラクターの「初めて」を丁寧に描写する。

**Pattern**:
```erb
IF FIRSTTIME(SELECTCOM)
    ; 15-30 lines special first experience
ELSE
    ; 5-10 lines normal
ENDIF
```

### 8f-1: Social First Experience

| Type | TALENT/条件 | 対応COM | Lines |
|------|------------|---------|------:|
| First meeting | - | ENCOUNTER | 20-30 |
| First kiss | - | COM_20 | 15-25 |
| First caress | - | COM_0-12 | 20-30 |

### 8f-2: Sexual First Experience (処女喪失系)

| Type | TALENT | 対応COM | Lines | Notes |
|------|--------|---------|------:|-------|
| First V sex | 処女 | COM_60,61,64,65,66,67 | 30-40 | 膣挿入全般 |
| First A sex | Ａ処女 | COM_62,63 | 25-35 | アナル挿入 |
| First oral | 口の処女 | COM_81,82 | 20-30 | フェラ系 |
| First paizuri | - | COM_83 | 15-25 | パイズリ |

### 8f-3: Receiving First Experience

| Type | 条件 | 対応COM | Lines | Notes |
|------|------|---------|------:|-------|
| First cum in mouth | EXP:口内射精 == 0 | 射精系 | 15-25 | 初めての口内射精 |
| First cum inside | EXP:膣内射精 == 0 | 射精系 | 20-30 | 初めての中出し |
| First cum in ass | EXP:アナル射精 == 0 | 射精系 | 15-25 | 初めてのアナル射精 |

### Summary

| Sub | Category | Lines | Branches |
|:---:|----------|------:|----------|
| 8f-1 | Social | ~600 | First/Repeat |
| 8f-2 | Sexual | ~1200 | TALENT:処女系 |
| 8f-3 | Receiving | ~600 | EXP:射精系 |

**Total Estimate**: ~2400 lines

**Targets**: Major COMs × All characters

---

## Links

- [content-roadmap.md](../content-roadmap.md)
- [index-features.md](../index-features.md)
