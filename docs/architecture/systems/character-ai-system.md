# 商業ゲーム級キャラクターAIシステム設計

## Status: DRAFT

**Target Version**: v1.5 (S1)

## 概要

女性キャラ（住人）と訪問者に商業フルプライスゲーム以上のAIアルゴリズムを適用する設計。
単純な条件分岐から、**動的意思決定・感情モデル・記憶システム**を備えた自律的キャラクターへ。

## 現状分析

### 実装済み（中級レベル）

| 要素 | 現状 | 課題 |
|------|------|------|
| **訪問者移動** | 確率ベース（部屋魅力度） | 戦略性なし |
| **エスカレート判定** | 屈服度/好感度の比較 | 固定ロジック |
| **住人反応** | 受動的（訪問者主導） | 自律行動なし |
| **記憶** | お気に入り設定のみ | 長期記憶なし |

### 目標

```
現状: 条件分岐 + 確率 → 反応的AI
目標: Utility AI + 感情モデル + 記憶 → 自律的AI
```

---

## 1. Utility AI（効用関数システム）

### 1.1 基本概念

**Utility AI**は各行動候補にスコアを計算し、最高スコアの行動を選択する。
固定の条件分岐より柔軟で、複数要因を総合的に評価できる。

```
Score = Σ (Factor × Weight × Curve)
```

### 1.2 女性キャラの行動決定

```erb
; ========================================
; 住人のUtility AI: 行動決定
; ========================================

@RESIDENT_DECIDE_ACTION(住人)
  ; 行動候補と効用スコア
  LOCAL:STR 行動候補:10
  LOCAL 効用スコア:10

  ; 候補リスト
  行動候補:0 = "日常業務"
  行動候補:1 = "主人に会いに行く"
  行動候補:2 = "訪問者を探す"
  行動候補:3 = "一人で休む"
  行動候補:4 = "他の住人と交流"
  行動候補:5 = "訪問者を避ける"
  行動候補:6 = "主人を避ける"
  行動候補:7 = "自慰"

  ; 各行動の効用計算
  効用スコア:0 = CALL UTILITY_日常業務(住人)
  効用スコア:1 = CALL UTILITY_主人接近(住人)
  効用スコア:2 = CALL UTILITY_訪問者探索(住人)
  効用スコア:3 = CALL UTILITY_休息(住人)
  効用スコア:4 = CALL UTILITY_住人交流(住人)
  効用スコア:5 = CALL UTILITY_訪問者回避(住人)
  効用スコア:6 = CALL UTILITY_主人回避(住人)
  効用スコア:7 = CALL UTILITY_自慰(住人)

  ; 最高スコアの行動を選択
  LOCAL 最高スコア = 0
  LOCAL 選択行動 = 0
  FOR I, 0, 7
    IF 効用スコア:I > 最高スコア
      最高スコア = 効用スコア:I
      選択行動 = I
    ENDIF
  NEXT

  RESULTS = 行動候補:選択行動
  RETURN

; ----------------------------------------
; 個別効用関数
; ----------------------------------------

@UTILITY_主人接近(住人)
  LOCAL スコア = 0

  ; 好感度が高いほど会いたい
  スコア += CFLAG:住人:好感度 / 100

  ; 思慕/恋慕があると強化
  IF TALENT:住人:思慕
    スコア += 30
  ENDIF
  IF TALENT:住人:恋慕
    スコア += 50
  ENDIF

  ; 屈服度が高いと抑制（訪問者優先になる）
  スコア -= CFLAG:住人:屈服度 / 200

  ; 最近主人と会っていないと上昇（寂しさ）
  LOCAL 未接触時間 = 現在時刻 - CFLAG:住人:最終主人接触
  スコア += MIN(未接触時間 / 60, 30)  ; 最大+30

  ; 感情状態の影響
  スコア += CALL EMOTION_MODIFIER(住人, "愛情")

  RESULT = MAX(スコア, 0)
  RETURN

@UTILITY_訪問者探索(住人)
  LOCAL スコア = 0

  ; 屈服度が高いほど探したい
  スコア += CFLAG:住人:屈服度 / 50

  ; 浮気素質があると強化
  IF TALENT:住人:浮気な蜜壷
    スコア += 40
  ENDIF
  IF TALENT:住人:NTR
    スコア += 60
  ENDIF

  ; 性欲が高いと上昇
  スコア += CFLAG:住人:性欲 / 10

  ; 快楽刻印の影響
  スコア += MARK:住人:浮気快楽刻印 * 5

  ; 好感度が高いと抑制（罪悪感）
  スコア -= CFLAG:住人:好感度 / 300

  ; 記憶: 訪問者との良い体験があると上昇
  スコア += CALL MEMORY_POSITIVE_SCORE(住人, "訪問者")

  RESULT = MAX(スコア, 0)
  RETURN

@UTILITY_訪問者回避(住人)
  LOCAL スコア = 0

  ; 好感度が高く、屈服度が低いと回避したい
  IF CFLAG:住人:好感度 > CFLAG:住人:屈服度 * 2
    スコア += 50
  ENDIF

  ; 恐怖感情があると強化
  スコア += CALL EMOTION_GET(住人, "恐怖") * 2

  ; 最近嫌な体験をした
  スコア += CALL MEMORY_NEGATIVE_SCORE(住人, "訪問者")

  ; 羞恥心が残っていると回避傾向
  スコア += CFLAG:住人:羞恥心残量 / 20

  RESULT = MAX(スコア, 0)
  RETURN

@UTILITY_自慰(住人)
  LOCAL スコア = 0

  ; 性欲が高いと上昇
  スコア += CFLAG:住人:性欲 / 5

  ; 快楽依存があると上昇
  スコア += CFLAG:住人:快楽依存 * 3

  ; 浮気快楽刻印が高いと「あの快感」を求める
  IF MARK:住人:浮気快楽刻印 > MARK:住人:快楽刻印
    スコア += (MARK:住人:浮気快楽刻印 - MARK:住人:快楽刻印) * 8
  ENDIF

  ; 一人の時間があると上昇
  IF CALL IS_ALONE(住人)
    スコア += 20
  ENDIF

  RESULT = MAX(スコア, 0)
  RETURN
```

### 1.3 訪問者の行動決定

```erb
; ========================================
; 訪問者のUtility AI: ターゲット選定
; ========================================

@VISITOR_SELECT_TARGET()
  LOCAL 最高スコア = 0
  LOCAL ターゲット = -1

  ; 全住人をスコアリング
  FOR 住人, 0, CHARANUM - 1
    IF CALL IS_VALID_TARGET(住人)
      LOCAL スコア = CALL VISITOR_TARGET_UTILITY(住人)
      IF スコア > 最高スコア
        最高スコア = スコア
        ターゲット = 住人
      ENDIF
    ENDIF
  NEXT

  RESULT = ターゲット
  RETURN

@VISITOR_TARGET_UTILITY(住人)
  LOCAL スコア = 0

  ; === 攻略しやすさ ===
  ; 屈服度が高いほど狙いやすい
  スコア += CFLAG:住人:屈服度 / 10

  ; 好感度が低いほど狙いやすい（主人への忠誠が弱い）
  スコア += (10000 - CFLAG:住人:好感度) / 500

  ; 浮気素質持ちは優先
  IF TALENT:住人:浮気な蜜壷
    スコア += 30
  ENDIF

  ; === 報酬期待値 ===
  ; 高嶺の花ほど落とし甲斐がある
  IF CFLAG:住人:好感度 > 5000 && CFLAG:住人:屈服度 < 1000
    スコア += 50  ; チャレンジボーナス
  ENDIF

  ; まだ陥落していないキャラは価値が高い
  IF !TALENT:住人:NTR
    スコア += 40
  ENDIF

  ; === リスク ===
  ; 主人の監視が強いエリアは減点
  スコア -= CALL GET_AREA_SURVEILLANCE(CFLAG:住人:現在位置) * 5

  ; === 個人的好み（記憶から） ===
  スコア += CALL VISITOR_PREFERENCE(住人)

  RESULT = MAX(スコア, 0)
  RETURN
```

---

## 2. 感情モデル

### 2.1 PADモデル（3軸感情）

商業ゲームでよく使われる**Pleasure-Arousal-Dominance**モデルを採用。

| 軸 | 説明 | 範囲 | このゲームでの意味 |
|:--:|------|:----:|-------------------|
| **P** (Pleasure) | 快・不快 | -100～+100 | 幸福感、満足度 |
| **A** (Arousal) | 興奮・沈静 | -100～+100 | 性的興奮、緊張 |
| **D** (Dominance) | 支配・服従 | -100～+100 | 主導権、自己決定感 |

```erb
; ========================================
; PAD感情モデル
; ========================================

; 変数定義 (CFLAGに追加)
; CFLAG:キャラ:感情_P  ; Pleasure
; CFLAG:キャラ:感情_A  ; Arousal
; CFLAG:キャラ:感情_D  ; Dominance

@EMOTION_UPDATE(キャラ, イベント種別, 相手)
  ; イベントに応じてPAD値を更新

  SELECTCASE イベント種別
    ; ======== 主人関連 ========
    CASE "主人_愛撫"
      CFLAG:キャラ:感情_P += 10
      CFLAG:キャラ:感情_A += 15
      CFLAG:キャラ:感情_D += 5   ; 主人には安心感

    CASE "主人_絶頂"
      CFLAG:キャラ:感情_P += 30
      CFLAG:キャラ:感情_A += 40
      CFLAG:キャラ:感情_D += 10

    CASE "主人_放置"
      CFLAG:キャラ:感情_P -= 5
      CFLAG:キャラ:感情_A -= 10
      CFLAG:キャラ:感情_D -= 5   ; 寂しさ

    ; ======== 訪問者関連 ========
    CASE "訪問者_接近"
      CFLAG:キャラ:感情_P -= 10  ; 初期は不快
      CFLAG:キャラ:感情_A += 20  ; 緊張
      CFLAG:キャラ:感情_D -= 15  ; 支配される感覚

    CASE "訪問者_強制"
      CFLAG:キャラ:感情_P -= 30
      CFLAG:キャラ:感情_A += 50
      CFLAG:キャラ:感情_D -= 40  ; 支配される

    CASE "訪問者_絶頂"
      ; 屈服度によってP値が変化
      IF CFLAG:キャラ:屈服度 > CFLAG:キャラ:好感度 / 2
        CFLAG:キャラ:感情_P += 20  ; 快感を受け入れている
      ELSE
        CFLAG:キャラ:感情_P -= 10  ; 罪悪感
      ENDIF
      CFLAG:キャラ:感情_A += 60
      CFLAG:キャラ:感情_D -= 30

    CASE "訪問者_快楽堕ち"
      CFLAG:キャラ:感情_P += 50   ; 快楽に溺れる
      CFLAG:キャラ:感情_A += 80
      CFLAG:キャラ:感情_D -= 60   ; 完全服従

    ; ======== 自発行動 ========
    CASE "自慰_訪問者想起"
      CFLAG:キャラ:感情_P += 15
      CFLAG:キャラ:感情_A += 30
      CFLAG:キャラ:感情_D -= 20   ; 自己嫌悪も

  ENDSELECT

  ; クランプ
  CFLAG:キャラ:感情_P = LIMIT(CFLAG:キャラ:感情_P, -100, 100)
  CFLAG:キャラ:感情_A = LIMIT(CFLAG:キャラ:感情_A, -100, 100)
  CFLAG:キャラ:感情_D = LIMIT(CFLAG:キャラ:感情_D, -100, 100)
  RETURN
```

### 2.2 派生感情

PADの組み合わせから具体的な感情を導出。

| 感情 | P | A | D | このゲームでの状況 |
|------|:-:|:-:|:-:|-------------------|
| **幸福** | + | 0 | + | 主人との幸せな時間 |
| **恐怖** | - | + | - | 訪問者の強制 |
| **怒り** | - | + | + | 主人の浮気発覚時 |
| **悲しみ** | - | - | - | 放置、無視 |
| **興奮** | + | + | 0 | 性行為中 |
| **屈辱** | - | + | - | 強制絶頂 |
| **背徳快感** | + | + | - | 訪問者との快楽 |
| **依存** | + | - | - | 訪問者への執着 |

```erb
@EMOTION_GET_DERIVED(キャラ)
  LOCAL P = CFLAG:キャラ:感情_P
  LOCAL A = CFLAG:キャラ:感情_A
  LOCAL D = CFLAG:キャラ:感情_D

  ; 派生感情を計算
  LOCAL:STR 主感情 = ""
  LOCAL 強度 = 0

  IF P > 30 && A > 30 && D < -30
    主感情 = "背徳快感"
    強度 = (P + A - D) / 3
  ELSEIF P < -30 && A > 30 && D < -30
    主感情 = "屈辱"
    強度 = (-P + A - D) / 3
  ELSEIF P < -30 && A > 30 && D > 30
    主感情 = "怒り"
    強度 = (-P + A + D) / 3
  ELSEIF P > 30 && A < -30 && D < -30
    主感情 = "依存"
    強度 = (P - A - D) / 3
  ELSEIF P > 30 && A > 30
    主感情 = "興奮"
    強度 = (P + A) / 2
  ELSEIF P > 30 && D > 30
    主感情 = "幸福"
    強度 = (P + D) / 2
  ELSEIF P < -30 && A > 30
    主感情 = "恐怖"
    強度 = (-P + A) / 2
  ELSEIF P < -30 && D < -30
    主感情 = "悲しみ"
    強度 = (-P - D) / 2
  ELSE
    主感情 = "平静"
    強度 = 0
  ENDIF

  RESULTS = 主感情
  RESULT = 強度
  RETURN
```

### 2.3 感情の時間減衰

```erb
@EMOTION_DECAY(キャラ)
  ; 1時間ごとに呼び出し
  ; 感情は徐々に中立に戻る

  ; 減衰率（基本5%）
  LOCAL 減衰率 = 5

  ; 刻印があると減衰しにくい（体に刻まれた記憶）
  IF MARK:キャラ:浮気快楽刻印 > 5
    減衰率 = 3
  ENDIF

  CFLAG:キャラ:感情_P = CFLAG:キャラ:感情_P * (100 - 減衰率) / 100
  CFLAG:キャラ:感情_A = CFLAG:キャラ:感情_A * (100 - 減衰率) / 100
  CFLAG:キャラ:感情_D = CFLAG:キャラ:感情_D * (100 - 減衰率) / 100
  RETURN
```

---

## 3. 記憶システム

### 3.1 短期記憶（最近の出来事）

```erb
; ========================================
; 短期記憶システム
; ========================================

; 構造: CFLAG:キャラ:短期記憶_X_種別, _相手, _強度, _時刻
; 最大5件の短期記憶

@MEMORY_ADD_SHORT(キャラ, 種別, 相手, 強度)
  ; 最も古い記憶を探す
  LOCAL 最古スロット = 0
  LOCAL 最古時刻 = CFLAG:キャラ:短期記憶_0_時刻

  FOR I, 1, 4
    IF CFLAG:キャラ:短期記憶_{I}_時刻 < 最古時刻
      最古時刻 = CFLAG:キャラ:短期記憶_{I}_時刻
      最古スロット = I
    ENDIF
  NEXT

  ; 上書き
  CFLAGS:キャラ:短期記憶_{最古スロット}_種別 = 種別
  CFLAG:キャラ:短期記憶_{最古スロット}_相手 = 相手
  CFLAG:キャラ:短期記憶_{最古スロット}_強度 = 強度
  CFLAG:キャラ:短期記憶_{最古スロット}_時刻 = 現在時刻
  RETURN
```

### 3.2 長期記憶（重要イベント）

```erb
; ========================================
; 長期記憶システム
; ========================================

; 長期記憶は特定条件で形成される
; - 初めての体験
; - 強烈な感情を伴う体験
; - 繰り返しによる定着

@MEMORY_TRY_CONSOLIDATE(キャラ, 種別, 相手, 強度)
  ; 長期記憶への移行判定

  LOCAL 移行閾値 = 80

  ; 初体験は必ず記憶
  IF CALL IS_FIRST_EXPERIENCE(キャラ, 種別, 相手)
    CALL MEMORY_ADD_LONG(キャラ, 種別, 相手, 強度, "初体験")
    RETURN
  ENDIF

  ; 強度が高ければ記憶
  IF 強度 >= 移行閾値
    CALL MEMORY_ADD_LONG(キャラ, 種別, 相手, 強度, "強烈")
    RETURN
  ENDIF

  ; 同種の短期記憶が3回以上あれば定着
  LOCAL 回数 = CALL COUNT_SHORT_MEMORY(キャラ, 種別, 相手)
  IF 回数 >= 3
    CALL MEMORY_ADD_LONG(キャラ, 種別, 相手, 強度, "反復")
    RETURN
  ENDIF
  RETURN

; 長期記憶の例
; "訪問者との初絶頂" → 強度90, タグ"初体験"
; "訪問者に何度も絶頂させられた" → 強度60, タグ"反復"
; "主人に無視された日" → 強度75, タグ"強烈"
```

### 3.3 記憶が行動に与える影響

```erb
@MEMORY_POSITIVE_SCORE(キャラ, 対象種別)
  ; 対象（主人/訪問者）との肯定的記憶のスコア
  LOCAL スコア = 0

  ; 長期記憶をスキャン
  FOR I, 0, 長期記憶数 - 1
    IF CFLAGS:キャラ:長期記憶_{I}_相手種別 == 対象種別
      IF CFLAG:キャラ:長期記憶_{I}_感情 > 0  ; 肯定的
        スコア += CFLAG:キャラ:長期記憶_{I}_強度 / 10
      ENDIF
    ENDIF
  NEXT

  ; 刻印による記憶の強化
  IF 対象種別 == "訪問者"
    スコア += MARK:キャラ:浮気快楽刻印 * 3
  ELSEIF 対象種別 == "主人"
    スコア += MARK:キャラ:快楽刻印 * 3
  ENDIF

  RESULT = スコア
  RETURN

@MEMORY_NEGATIVE_SCORE(キャラ, 対象種別)
  ; 対象との否定的記憶のスコア
  LOCAL スコア = 0

  FOR I, 0, 長期記憶数 - 1
    IF CFLAGS:キャラ:長期記憶_{I}_相手種別 == 対象種別
      IF CFLAG:キャラ:長期記憶_{I}_感情 < 0  ; 否定的
        スコア += ABS(CFLAG:キャラ:長期記憶_{I}_強度) / 10
      ENDIF
    ENDIF
  NEXT

  RESULT = スコア
  RETURN
```

---

## 4. 訪問者のGOAP（目標指向行動計画）

### 4.1 目標設定

```erb
; ========================================
; 訪問者のGOAP: 目標指向行動計画
; ========================================

@VISITOR_SET_GOAL()
  ; 現在の状況から最適な目標を選択

  LOCAL:STR 目標候補:5
  LOCAL 目標優先度:5

  目標候補:0 = "新規ターゲット開拓"
  目標候補:1 = "既存ターゲット攻略進行"
  目標候補:2 = "陥落済みキャラ享受"
  目標候補:3 = "ハーレム構築"
  目標候補:4 = "主人の面前NTR"

  ; 各目標の優先度計算
  目標優先度:0 = CALL GOAL_PRIORITY_新規開拓()
  目標優先度:1 = CALL GOAL_PRIORITY_攻略進行()
  目標優先度:2 = CALL GOAL_PRIORITY_享受()
  目標優先度:3 = CALL GOAL_PRIORITY_ハーレム()
  目標優先度:4 = CALL GOAL_PRIORITY_面前NTR()

  ; 最高優先度の目標を選択
  LOCAL 最高 = 0
  LOCAL 選択 = 0
  FOR I, 0, 4
    IF 目標優先度:I > 最高
      最高 = 目標優先度:I
      選択 = I
    ENDIF
  NEXT

  FLAG:訪問者_現在目標 = 選択
  RETURN

@GOAL_PRIORITY_攻略進行()
  ; 攻略中のターゲットがいる場合
  LOCAL スコア = 0
  LOCAL ターゲット = FLAG:訪問者のお気に入り

  IF ターゲット >= 0
    ; 攻略進行度（屈服度/好感度比）
    LOCAL 進行度 = CFLAG:ターゲット:屈服度 * 100 / MAX(CFLAG:ターゲット:好感度, 1)

    IF 進行度 > 20 && 進行度 < 100
      スコア = 80  ; 攻略中は継続優先
    ENDIF

    ; あと少しで陥落なら最優先
    IF 進行度 > 80
      スコア = 100
    ENDIF
  ENDIF

  RESULT = スコア
  RETURN
```

### 4.2 行動プランニング

```erb
@VISITOR_PLAN_ACTIONS(目標)
  ; 目標達成のための行動計画を生成

  LOCAL:STR 計画:10
  LOCAL 計画数 = 0

  SELECTCASE 目標
    CASE "既存ターゲット攻略進行"
      LOCAL ターゲット = FLAG:訪問者のお気に入り
      LOCAL 屈服 = CFLAG:ターゲット:屈服度
      LOCAL 好感 = CFLAG:ターゲット:好感度
      LOCAL 位置 = CFLAG:ターゲット:現在位置

      ; Step 1: ターゲットの近くに移動
      IF FLAG:訪問者の現在位置 != 位置
        計画:計画数 = "移動:" + TOSTR(位置)
        計画数++
      ENDIF

      ; Step 2: 現在の攻略段階に応じた行動
      IF 屈服 < 好感 / 4
        ; 初期段階: 会話で好感度を下げる/屈服度を上げる
        計画:計画数 = "会話:誘惑"
        計画数++
      ELSEIF 屈服 < 好感 / 2
        ; 中期: スキンシップ
        計画:計画数 = "接触:キス"
        計画数++
      ELSEIF 屈服 < 好感 * 3 / 4
        ; 後期: 愛撫
        計画:計画数 = "愛撫"
        計画数++
      ELSE
        ; 最終段階: 性行為
        計画:計画数 = "性交"
        計画数++
      ENDIF

      ; Step 3: 密室確保（可能なら）
      IF CALL GET_密室度(位置) < 2 && 屈服 > 好感 / 2
        計画:計画数 = "誘導:密室"
        計画数++
      ENDIF

    CASE "主人の面前NTR"
      ; 主人がいる部屋で行為
      LOCAL 主人位置 = FLAG:主人位置
      計画:計画数 = "移動:" + TOSTR(主人位置)
      計画数++
      計画:計画数 = "呼び出し:陥落済みキャラ"
      計画数++
      計画:計画数 = "性交:見せつけ"
      計画数++

  ENDSELECT

  ; 計画をFLAGに保存
  FLAG:訪問者_計画数 = 計画数
  FOR I, 0, 計画数 - 1
    FLAGS:訪問者_計画_{I} = 計画:I
  NEXT
  RETURN
```

---

## 5. 関係性グラフ

### 5.1 キャラクター間関係

```erb
; ========================================
; 関係性マトリクス
; ========================================

; CFLAG2D:キャラA:キャラB:関係種別 = 値
; 関係種別: 好意, 信頼, 嫉妬, 依存, 敵意

@RELATION_UPDATE(キャラA, キャラB, 種別, 変化量)
  CFLAG2D:キャラA:キャラB:種別 += 変化量
  CFLAG2D:キャラA:キャラB:種別 = LIMIT(CFLAG2D:キャラA:キャラB:種別, -100, 100)
  RETURN

@RELATION_GET(キャラA, キャラB, 種別)
  RESULT = CFLAG2D:キャラA:キャラB:種別
  RETURN
```

### 5.2 嫉妬システム

```erb
@JEALOUSY_CHECK(観察者, 行為者, 対象)
  ; 観察者が行為者と対象の行為を見た時

  ; 観察者が対象に好意を持っている場合
  LOCAL 好意 = CALL RELATION_GET(観察者, 対象, "好意")
  IF 好意 > 30
    ; 嫉妬発生
    LOCAL 嫉妬量 = 好意 / 3 + RAND:20
    CALL RELATION_UPDATE(観察者, 行為者, "嫉妬", 嫉妬量)
    CALL RELATION_UPDATE(観察者, 行為者, "敵意", 嫉妬量 / 2)

    ; 感情への影響
    CALL EMOTION_UPDATE(観察者, "嫉妬目撃", 行為者)
  ENDIF
  RETURN
```

---

## 6. 口上への統合

### 6.1 感情状態の口上反映

```erb
@KOJO_EMOTION_PREFIX(キャラ)
  ; 口上の前に感情状態を表現

  LOCAL:STR 感情 = CALL EMOTION_GET_DERIVED(キャラ)
  LOCAL 強度 = RESULT

  SELECTCASE 感情
    CASE "背徳快感"
      IF 強度 > 60
        PRINTL （いけない…でも…気持ちいい…っ）
      ELSE
        PRINTL （こんなの…ダメなのに…）
      ENDIF

    CASE "屈辱"
      PRINTL （悔しい…なのに体が…）

    CASE "依存"
      PRINTL （また…あの人に会いたい…）

    CASE "恐怖"
      PRINTL （怖い…でも逆らえない…）

    CASE "幸福"
      PRINTL （%CALLNAME:MASTER%と一緒で幸せ…）

  ENDSELECT
  RETURN

@KOJO_MEMORY_REFERENCE(キャラ, 状況)
  ; 記憶を参照した口上

  ; 比較記憶
  IF 状況 == "性交中" && CALL HAS_MEMORY(キャラ, "訪問者_絶頂")
    LOCAL 訪問者刻印 = MARK:キャラ:浮気快楽刻印
    LOCAL 主人刻印 = MARK:キャラ:快楽刻印

    IF 訪問者刻印 > 主人刻印 + 3
      PRINTL （%CALLNAME:MASTER%じゃ…物足りない…）
      PRINTL （あの人の方が…ずっと…）
    ENDIF
  ENDIF
  RETURN
```

---

## 7. 実装フェーズ

| Phase | 内容 | 優先度 | 依存 |
|:-----:|------|:------:|:----:|
| 1 | PAD感情モデル | 高 | - |
| 2 | Utility AI（住人基本） | 高 | Phase 1 |
| 3 | 短期記憶システム | 中 | Phase 1 |
| 4 | Utility AI（訪問者） | 中 | Phase 2 |
| 5 | 長期記憶システム | 中 | Phase 3 |
| 6 | GOAP（訪問者目標計画） | 低 | Phase 4 |
| 7 | 関係性グラフ | 低 | Phase 5 |
| 8 | 口上統合 | 低 | 全Phase |

---

## 8. 商業ゲームとの比較

| 要素 | 一般的な商業ゲーム | 本設計 |
|------|-------------------|--------|
| **行動決定** | FSM / BT / Utility | Utility AI |
| **感情モデル** | 独自実装 / OCC | PAD + 派生感情 |
| **記憶** | 限定的 | 短期+長期記憶 |
| **目標計画** | GOAP（一部タイトル） | GOAP（訪問者） |
| **関係性** | 好感度のみが多い | 多軸関係グラフ |
| **学習** | 稀 | 記憶による行動変化 |

**商業ゲーム以上の点**:
- NTRという特殊テーマに最適化された感情モデル
- 刻印システムとの連携（体に刻まれた記憶）
- 比較システム（主人 vs 訪問者）の深化

---

## 9. 参考文献

- [Game AI Planning: GOAP, Utility, and Behavior Trees](https://tonogameconsultants.com/game-ai-planning/)
- [GOBT: Goal-Oriented Behavior Tree Framework (2024)](https://www.jmis.org/archive/view_article?pid=jmis-10-4-321)
- [Behavior Selection Algorithms Overview](https://www.gameaipro.com/GameAIPro/GameAIPro_Chapter04_Behavior_Selection_Algorithms.pdf)

---

## 議論ログ

| 日付 | 内容 |
|------|------|
| 2025-12-18 | 初期設計開始 |
| 2025-12-18 | 現状分析: 訪問者AI中級、住人AI未実装 |
| 2025-12-18 | Utility AI + PAD感情モデル + 記憶システムの採用決定 |
