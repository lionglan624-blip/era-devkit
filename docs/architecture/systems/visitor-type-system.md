# Visitor Type System Design

## Status: DRAFT

**Target Version**: v4.x (S3)

**Parent**: [phase-system.md](phase-system.md)

---

## Overview

Visitor type selection at game start determines route tendency. Player chooses play style through visitor type.

---

## Design Philosophy

### Problem Statement

1. **Determinism Chain**: Heroine traits → Reaction pattern → Visitor adapts → Route fixed
2. **Character Gacha**: Route determined by heroine characteristics, not player agency
3. **Visitor Gacha**: If visitor behavior is fixed, route is predetermined

### Solution: Visitor Type = Route Tendency

- Player selects visitor type at game start (Option)
- Visitor type determines primary parameter accumulation pattern
- Route emerges from visitor type × heroine characteristics interaction

---

## Visitor Type Definitions

| Type ID | Name (JP) | Name (EN) | Primary Param | Route Tendency | Behavior Pattern |
|:-------:|-----------|-----------|:-------------:|:--------------:|------------------|
| 1 | 強制型 | Coercion | LEV | R1 | Gather evidence, exploit weakness, threaten |
| 2 | 誘惑型 | Seduction | FAM | R2 | Close distance, create atmosphere, normalize |
| 3 | 依存型 | Dependency | DEP | R3/R4 | Help, protect, create debt, offer escape |
| 4 | 情熱型 | Passion | EMO | R6 | Court, romance, genuine affection |
| 5 | ランダム | Random | Mixed | Emergent | Adapts to heroine reactions |

### Type Details

#### Type 1: Coercion (強制型)

```
Primary: LEV (Leverage/Evidence)
Secondary: TEM (through fear/submission)
Route: R1 (Coercion/Sacrifice)

Behavior:
- Seeks compromising situations
- Gathers evidence (photos, witnesses)
- Exploits weakness/debt
- Threatens exposure
- "No escape" narrative
```

#### Type 2: Seduction (誘惑型)

```
Primary: FAM (Familiarity)
Secondary: TEM (through comfort/attraction)
Route: R2 (Familiarity/Flow)

Behavior:
- Spends time together
- Creates comfortable atmosphere
- Normalizes physical contact
- Erodes boundaries gradually
- "It just happened" narrative
```

#### Type 3: Dependency (依存型)

```
Primary: DEP (Dependency/Debt)
Secondary: TEM (through gratitude/need)
Route: R3 (Escape) or R4 (Trade)

Behavior:
- Offers help/protection
- Creates situations requiring rescue
- Builds emotional debt
- Provides escape from stress
- "I need you" narrative
```

#### Type 4: Passion (情熱型)

```
Primary: EMO (Romanticization)
Secondary: FAM + TEM
Route: R6 (Real Affair) - strict unlock

Behavior:
- Genuine courtship
- Romantic gestures
- Emotional connection
- Competes with MC for love
- "I fell for you" narrative

Note: R6 requires MC failure accumulation + EMO high
```

#### Type 5: Random (ランダム)

```
Primary: Adapts based on heroine reaction
Route: Emergent from interaction

Behavior:
- Observes heroine responses
- Switches approach based on success
- Most "realistic" but least predictable
- Recommended for replay variety
```

---

## Parameter Accumulation by Type

### Base Accumulation Rate (per interaction)

| Action Type | Type 1 | Type 2 | Type 3 | Type 4 | Type 5 |
|-------------|:------:|:------:|:------:|:------:|:------:|
| LEV gain | **+3** | +1 | +1 | +0 | Variable |
| FAM gain | +1 | **+3** | +1 | +2 | Variable |
| DEP gain | +1 | +1 | **+3** | +1 | Variable |
| TEM gain | +2 | +2 | +2 | +2 | Variable |
| EMO gain | +0 | +1 | +1 | **+3** | Variable |

### Type 5 (Random) Adaptation Logic

```erb
@VISITOR_ADAPT_TYPE(HEROINE)
  ; Check heroine's recent reactions
  IF HEROINE_RESISTED_LAST_ACTION
    ; Resistance → try coercion
    CURRENT_APPROACH = TYPE_COERCION
  ELSEIF HEROINE_ENJOYED_LAST_ACTION
    ; Enjoyment → try seduction
    CURRENT_APPROACH = TYPE_SEDUCTION
  ELSEIF HEROINE_STRESSED
    ; Stressed → try dependency
    CURRENT_APPROACH = TYPE_DEPENDENCY
  ELSEIF HEROINE_EMOTIONALLY_OPEN
    ; Open → try passion
    CURRENT_APPROACH = TYPE_PASSION
  ENDIF
```

---

## Heroine Reaction Influence

### Reaction Modifier to Parameter Gain

| Heroine Reaction | LEV Mod | FAM Mod | DEP Mod | EMO Mod |
|-----------------|:-------:|:-------:|:-------:|:-------:|
| Resists strongly | +2 | -1 | +0 | -1 |
| Resists weakly | +1 | +0 | +1 | +0 |
| Accepts passively | +0 | +2 | +1 | +1 |
| Enjoys actively | -1 | +2 | +0 | +2 |
| Seeks actively | -1 | +1 | -1 | +3 |

### Trait Influence on Reaction

| Trait | Likely Reaction | Parameter Tendency |
|-------|-----------------|-------------------|
| 貞操観念高 | Resists strongly | LEV accumulates faster |
| 素直 | Accepts passively | FAM accumulates faster |
| 自制心 | Resists weakly | Slow accumulation overall |
| 好奇心 | Enjoys actively | FAM/EMO accumulates faster |
| 依存傾向 | Seeks actively | DEP/EMO accumulates faster |

---

## Player Intervention by Mode

### NTR Mode (Passive)

| Intervention | Effect | Limitation |
|--------------|--------|------------|
| 好感度維持 | Slower progression | Route direction unchanged |
| 放置回避 | Reduce OPP | Route direction unchanged |
| 覗き回避 | Reduce SUS | Route direction unchanged |

**Route is determined by Visitor Type × Heroine Traits**

### NTRase Mode (Active) - v6.x

Two-layer control system for natural narrative framing.

#### Layer 1: 許可レベル（行為段階）

**「どこまで許すか」** - Controls act depth.

| Level | Meaning | Mood Limit |
|:-----:|---------|:----------:|
| 0 | 禁止 | - |
| 1 | 会話まで | NTR_MOOD_会話 |
| 2 | キスまで | NTR_MOOD_キス |
| 3 | 愛撫まで | NTR_MOOD_愛撫 |
| 4 | 奉仕まで | NTR_MOOD_奉仕 |
| 5 | 全許可 | NTR_MOOD_性交以上 |

#### Layer 2: ヒロインへのお願い（心構え）

**「どう向き合うか」** - Affects heroine mindset, indirectly influences route.

**Design Philosophy**: MC talks to partner, not orchestrates visitor. More intimate/natural than "訪問者への指示".

| Request (JP) | Request (EN) | Heroine Mindset | Reaction Tendency | Route Influence |
|--------------|--------------|-----------------|-------------------|-----------------|
| 「私だけを愛して」 | "Love only me" | LOVE_MC優先 | Guilt↑, EMO suppressed | R1-R4 (loves but yields) |
| 「楽しんで」 | "Enjoy yourself" | 快楽解放 | Pleasure acceptance | R2 (FAM↑) |
| 「逃げ場にして」 | "Use it as escape" | 依存許可 | Comfort seeking | R3 (DEP↑) |
| 「心は渡さないで」 | "Don't give your heart" | EMO抑制 | Emotional distance | R6 blocked |
| (何も言わない) | (Say nothing) | 自然体 | Situation dependent | Emergent |

#### Mindset Effect Implementation

```erb
@APPLY_MINDSET_MODIFIER(HEROINE, PARAM, BASE_GAIN)
  LOCAL MINDSET = CFLAG:HEROINE:寝取らせ心構え
  LOCAL MOD = 1.0

  SELECTCASE MINDSET
    CASE MINDSET_LOVE_MC  ; 「私だけを愛して」
      IF PARAM == PARAM_EMO
        MOD = 0.3  ; EMO gain suppressed
      ENDIF
      CFLAG:HEROINE:GUILT += BASE_GAIN * 0.5

    CASE MINDSET_ENJOY  ; 「楽しんで」
      IF PARAM == PARAM_FAM || PARAM == PARAM_TEM
        MOD = 1.5  ; FAM/TEM boosted
      ENDIF

    CASE MINDSET_ESCAPE  ; 「逃げ場にして」
      IF PARAM == PARAM_DEP
        MOD = 1.8  ; DEP boosted
      ENDIF

    CASE MINDSET_NO_HEART  ; 「心は渡さないで」
      IF PARAM == PARAM_EMO
        MOD = 0.1  ; EMO almost blocked
      ENDIF

    CASE MINDSET_NATURAL  ; 何も言わない
      MOD = 1.0  ; No modification

  ENDSELECT

  RETURN BASE_GAIN * MOD

; Effect chain:
; Request → Mindset → Reaction modifier → Parameter gain → Route
```

#### UI Design

```
┌─────────────────────────────────────┐
│ %NAME%に何を伝える？                │
├─────────────────────────────────────┤
│ [1] 「好きにしていい」              │
│     → 許可レベル5、心構え:自然体    │
│                                     │
│ [2] 「でも、私だけを愛して」        │
│     → 心構え:LOVE_MC優先            │
│     → R1-R4傾向（愛してるけど…）   │
│                                     │
│ [3] 「楽しんでおいで」              │
│     → 心構え:快楽解放               │
│     → R2傾向（馴染み/流れ）         │
│                                     │
│ [4] 「辛いときの逃げ場にして」      │
│     → 心構え:依存許可               │
│     → R3傾向（逃避/麻酔）           │
│                                     │
│ [5] 「心だけは渡さないで」          │
│     → 心構え:EMO抑制                │
│     → R6ブロック                    │
│                                     │
│ [6] （何も言わない）                │
│     → 心構え:自然体                 │
│     → ルートはemergent              │
└─────────────────────────────────────┘
```

#### Variable Definition

```erb
; CFLAG addition
112,寝取らせ心構え    ; 0:自然体 1:LOVE_MC優先 2:快楽解放 3:依存許可 4:EMO抑制

; Constants
#DIM CONST MINDSET_NATURAL = 0
#DIM CONST MINDSET_LOVE_MC = 1
#DIM CONST MINDSET_ENJOY = 2
#DIM CONST MINDSET_ESCAPE = 3
#DIM CONST MINDSET_NO_HEART = 4
```

---

## Setting Locations

### 設計原則

| 設定対象 | 場所 | 理由 |
|---------|------|------|
| 訪問者タイプ | **OPTION** | 訪問者の設定 |
| 許可レベル | **COM** | ヒロインへの指示 |
| 心構え | **COM** | ヒロインへのお願い |

---

## Visitor Type: OPTION (NTR_OPTION.ERB)

### UI Design

```erb
; NTR_OPTION.ERB に追加

PRINTFORML [40] - 訪問者のタイプ: %GET_VISITOR_TYPE_NAME(FLAG:訪問者タイプ)%

; ...

ELSEIF RESULT == 40
  PRINTFORML 訪問者のタイプを選んでください
  PRINTL
  PRINTFORML [1] 強制型 - 弱みを握って支配する
  PRINTFORML     → R1（強制/犠牲）傾向
  PRINTFORML [2] 誘惑型 - 雰囲気で流させる
  PRINTFORML     → R2（馴染み/流れ）傾向
  PRINTFORML [3] 依存型 - 恩を売って縛る
  PRINTFORML     → R3/R4（逃避/取引）傾向
  PRINTFORML [4] 情熱型 - 本気で口説く
  PRINTFORML     → R6（本気浮気）傾向 ※厳格条件
  PRINTFORML [5] ランダム - 状況で変化
  PRINTFORML     → 予測不能、リプレイ向け
  PRINTFORML [0] 戻る
  $INPUT_VISITOR_TYPE
  INPUT
  IF RESULT >= 1 && RESULT <= 5
    FLAG:訪問者タイプ = RESULT
    PRINTFORMW 訪問者タイプを%GET_VISITOR_TYPE_NAME(RESULT)%に設定しました
    GOTO INPUT_LOOP_NTR_OPTION_TOP
  ELSEIF RESULT == 0
    GOTO INPUT_LOOP_NTR_OPTION_TOP
  ELSE
    GOTO INPUT_VISITOR_TYPE
  ENDIF
```

### Variable Storage

```erb
; FLAG definition (DIM.ERH)
#DIM SAVEDATA FLAG:訪問者タイプ = 5  ; デフォルト: ランダム

; Constants
#DIM CONST VISITOR_TYPE_COERCION = 1    ; 強制型
#DIM CONST VISITOR_TYPE_SEDUCTION = 2   ; 誘惑型
#DIM CONST VISITOR_TYPE_DEPENDENCY = 3  ; 依存型
#DIM CONST VISITOR_TYPE_PASSION = 4     ; 情熱型
#DIM CONST VISITOR_TYPE_RANDOM = 5      ; ランダム
```

### Helper Function

```erb
@GET_VISITOR_TYPE_NAME(TYPE)
#FUNCTIONS
  SELECTCASE TYPE
    CASE VISITOR_TYPE_COERCION
      RETURNF "強制型"
    CASE VISITOR_TYPE_SEDUCTION
      RETURNF "誘惑型"
    CASE VISITOR_TYPE_DEPENDENCY
      RETURNF "依存型"
    CASE VISITOR_TYPE_PASSION
      RETURNF "情熱型"
    CASE VISITOR_TYPE_RANDOM
      RETURNF "ランダム"
    CASEELSE
      RETURNF "未設定"
  ENDSELECT
```

---

## Permission & Mindset: COM (対ヒロイン)

### COM470: 寝取らせ設定

統合コマンド - 許可レベルと心構えを一画面で設定。

```erb
@COMF470
  PRINTL ■ 寝取らせ設定 - %CALLNAME:TARGET%
  PRINTL

  ; === 現在の設定表示 ===
  PRINTL 【現在の設定】
  PRINTL  許可レベル: %GET_PERMIT_LEVEL_NAME(CFLAG:TARGET:寝取らせ許可レベル)%
  PRINTL  心構え: %GET_MINDSET_NAME(CFLAG:TARGET:寝取らせ心構え)%
  PRINTL

  ; === 許可レベル変更 ===
  PRINTL 【どこまで許すか】
  PRINTBUTTON [1] 禁止, 1
  PRINTBUTTON [2] 会話まで, 2
  PRINTBUTTON [3] キスまで, 3
  PRINTBUTTON [4] 愛撫まで, 4
  PRINTBUTTON [5] 奉仕まで, 5
  PRINTBUTTON [6] 好きにして, 6
  PRINTL

  ; === 心構え変更 ===
  PRINTL 【どう向き合うか】
  PRINTBUTTON [11] 「私だけを愛して」, 11
  PRINTBUTTON [12] 「楽しんで」, 12
  PRINTBUTTON [13] 「逃げ場にして」, 13
  PRINTBUTTON [14] 「心は渡さないで」, 14
  PRINTBUTTON [15] （何も言わない）, 15
  PRINTL

  PRINTBUTTON [0] 戻る, 0

  INPUT

  SELECTCASE RESULT
    ; 許可レベル
    CASE 1
      CFLAG:TARGET:寝取らせ許可レベル = 0
      PRINTL 「誰にも触らせない」と伝えた。
    CASE 2
      CFLAG:TARGET:寝取らせ許可レベル = 1
      PRINTL 「会話までなら…」と許可した。
    CASE 3
      CFLAG:TARGET:寝取らせ許可レベル = 2
      PRINTL 「キスまでなら…」と許可した。
    CASE 4
      CFLAG:TARGET:寝取らせ許可レベル = 3
      PRINTL 「愛撫までなら…」と許可した。
    CASE 5
      CFLAG:TARGET:寝取らせ許可レベル = 4
      PRINTL 「奉仕までなら…」と許可した。
    CASE 6
      CFLAG:TARGET:寝取らせ許可レベル = 5
      PRINTL 「好きにしていい」と告げた。

    ; 心構え
    CASE 11
      CFLAG:TARGET:寝取らせ心構え = MINDSET_LOVE_MC
      PRINTL 「でも、私だけを愛して」と伝えた。
      PRINTL %CALLNAME:TARGET%は頷いた。
    CASE 12
      CFLAG:TARGET:寝取らせ心構え = MINDSET_ENJOY
      PRINTL 「楽しんでおいで」と告げた。
      PRINTL %CALLNAME:TARGET%は驚いた表情を見せた。
    CASE 13
      CFLAG:TARGET:寝取らせ心構え = MINDSET_ESCAPE
      PRINTL 「辛いときの逃げ場にして」と伝えた。
      PRINTL %CALLNAME:TARGET%は少し悲しそうな顔をした。
    CASE 14
      CFLAG:TARGET:寝取らせ心構え = MINDSET_NO_HEART
      PRINTL 「心だけは渡さないで」とお願いした。
      PRINTL %CALLNAME:TARGET%は静かに頷いた。
    CASE 15
      CFLAG:TARGET:寝取らせ心構え = MINDSET_NATURAL
      ; 何も言わない

    CASE 0
      RETURN 0
  ENDSELECT

  TIME += 5
  RETURN 1
```

---

## Current System Analysis

### Current (v0.6): Single Visitor

```erb
; Current FLAG-based single visitor
FLAG:訪問者の現在位置
FLAG:訪問者のムード
FLAG:訪問者との行為
FLAG:訪問者のお気に入り
FLAG:訪問者の嫌いな相手

; Single visitor name
NTR_NAME(0)  ; Always index 0
```

### Future (v4.x+): Typed Single Visitor

```erb
; Add visitor type
FLAG:訪問者タイプ  ; 1-5

; Parameter accumulation uses type
@GET_PARAM_GAIN(PARAM, ACTION)
  LOCAL BASE = ACTION_BASE_GAIN(ACTION)
  LOCAL TYPE_MOD = TYPE_MODIFIER(FLAG:訪問者タイプ, PARAM)
  LOCAL HEROINE_MOD = HEROINE_REACTION_MOD(TARGET, PARAM)
  RETURN BASE * TYPE_MOD * HEROINE_MOD
```

---

## Route Determination Formula

### Phase 4 Route Selection

```erb
@DETERMINE_ROUTE(HEROINE)
  ; Get accumulated parameters
  LOCAL LEV = GET_PARAM(HEROINE, PARAM_LEV)
  LOCAL FAM = GET_PARAM(HEROINE, PARAM_FAM)
  LOCAL DEP = GET_PARAM(HEROINE, PARAM_DEP)
  LOCAL EMO = GET_PARAM(HEROINE, PARAM_EMO)
  LOCAL TRUST_MC = GET_PARAM(HEROINE, PARAM_TRUST_MC)

  ; R6 check first (strict conditions)
  IF EMO >= THRESHOLD_EMO_HIGH && TRUST_MC < THRESHOLD_TRUST_LOW
    RETURN ROUTE_R6_AFFAIR
  ENDIF

  ; R5 check (MC failure)
  IF TRUST_MC < THRESHOLD_TRUST_CRITICAL && MC_FAILURE_COUNT >= 3
    RETURN ROUTE_R5_REBELLION
  ENDIF

  ; Standard routes by max parameter
  LOCAL MAX_PARAM = MAX(LEV, FAM, DEP)

  IF MAX_PARAM == LEV
    RETURN ROUTE_R1_COERCION
  ELSEIF MAX_PARAM == FAM
    RETURN ROUTE_R2_FAMILIARITY
  ELSEIF MAX_PARAM == DEP
    IF TRADE_CONDITIONS_MET
      RETURN ROUTE_R4_TRADE
    ELSE
      RETURN ROUTE_R3_ESCAPE
    ENDIF
  ENDIF
```

---

## Implementation Roadmap

### v4.0: Visitor Type Foundation

| Feature | Content | Dependencies |
|---------|---------|--------------|
| Visitor type option | Game start selection | - |
| Type-based param rates | Accumulation modifiers | S1 params |
| Type storage | FLAG:訪問者タイプ | - |

### v4.1: Heroine Reaction System

| Feature | Content | Dependencies |
|---------|---------|--------------|
| Reaction tracking | Last action response | v4.0 |
| Trait → Reaction mapping | Deterministic base | v4.0 |
| Reaction → Param modifiers | Dynamic accumulation | v4.0 |

### v4.2: Type 5 Random

| Feature | Content | Dependencies |
|---------|---------|--------------|
| Adaptation logic | Response-based switching | v4.1 |
| Behavior variety | Multiple approaches | v4.1 |

---

## Links

- [phase-system.md](phase-system.md) - Route definitions
- [netorase-system.md](netorase-system.md) - Active intervention (v6.x)
- [character-ai-system.md](character-ai-system.md) - Heroine reaction AI
- [content-roadmap.md](../content-roadmap.md) - Master roadmap
