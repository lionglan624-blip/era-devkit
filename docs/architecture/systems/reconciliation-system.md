# 復縁システム設計提案

## Status: DRAFT

**Target Version**: v7.x (S5)

**Depends on**: v5.x (NTR Completion), v6.x (Netorase) optional

## 概要

NTR完了後のキャラクターとの関係再構築システム。単純な「好感度上げて告白」ではなく、プラトニック/肉欲の二軸好感度と主人公成長システムを組み合わせた、戦略性とドラマ性のある復縁プロセス。

## 背景・動機

### 現行システムの課題

1. **復縁が軽い**: 好感度500で告白可能、NTR経験が無視される
2. **プロセスがない**: 告白1回で完了、ドラマがない
3. **主人公の成長が無関係**: NTRで「比較」されたのにステータスが影響しない
4. **再発リスクがない**: 一度復縁したら安泰

### 設計思想

```
NTR完了後の状態:
├── 精神的には完堕ち（主人公を邪険にする）
├── 肉体的には訪問者に調教済み（比較基準ができた）
└── 主人公への態度は冷淡だが、完全な拒絶ではない

復縁に必要なもの:
├── プラトニック好感度の回復（地道なコミュニケーション）
├── 肉欲的好感度の獲得（主人公のステータス向上）
└── 信頼度の回復（時間と行動）
```

---

## 二軸好感度システム

### 概念

```
            肉欲的好感度 高
                 │
          ┌──────┼──────┐
          │ 依存 │ 完全 │
          │ 関係 │ 復縁 │
プラトニック低 ─┼──────┼─ プラトニック高
          │ 拒絶 │ 友人 │
          │ 状態 │ 止まり│
          └──────┼──────┘
                 │
            肉欲的好感度 低
```

### 変数設計

```erb
; 新規CFLAG（住人ごと）
122,プラトニック好感度    ; 0-1000 (精神的な好意)
123,肉欲的好感度          ; 0-1000 (肉体的な満足度)
124,信頼度                ; -500～1000 (NTR後はマイナススタート)
125,NTR経験回数           ; 累計NTR完了回数
126,復縁回数              ; 復縁した回数
127,浮気リスク            ; 0-100 (再発確率ベース値)
128,比較基準_サイズ       ; 訪問者のチンポサイズ（最高値を記録）
129,比較基準_持続力       ; 訪問者の持続力（最高値を記録）
130,比較基準_顔           ; 訪問者の顔ランク（最高値を記録）
131,最終NTR相手           ; 最後にNTRした訪問者ID
132,刻印封印度            ; 0-100 (刻印の封印状態)
133,誘惑段階              ; 0-3 (再発イベント進行段階)
134,訪問者干渉度          ; 0-100 (訪問者の復縁妨害度)
135,元恋人フラグ          ; NTR前に恋人だった
136,元人妻フラグ          ; NTR前に人妻だった
137,成長を感じたフラグ    ; 主人公成長による満足度変化検知
```

### NTR完了時の変化

```erb
@ON_NTR_COMPLETE(TARGET, 訪問者)
  ; プラトニック好感度は維持（情はある）
  ; CFLAG:TARGET:プラトニック好感度 は変更なし

  ; 肉欲的好感度は訪問者基準にリセット
  CFLAG:TARGET:肉欲的好感度 = 0  ; 主人公への肉欲はゼロに

  ; 信頼度は大幅マイナス
  IF CFLAG:TARGET:NTR経験回数 == 0
    CFLAG:TARGET:信頼度 -= 500  ; 初回NTR
  ELSE
    CFLAG:TARGET:信頼度 -= 300  ; 2回目以降
  ENDIF

  ; 恋人/人妻状態でのNTRは追加ペナルティ
  IF TALENT:TARGET:恋人
    CFLAG:TARGET:信頼度 -= 200
    CFLAG:TARGET:元恋人フラグ = 1
    TALENT:TARGET:恋人 = 0  ; 恋人解除
  ENDIF
  IF TALENT:TARGET:人妻
    CFLAG:TARGET:信頼度 -= 400
    CFLAG:TARGET:元人妻フラグ = 1
    TALENT:TARGET:人妻 = 0  ; 人妻解除
  ENDIF

  ; 比較基準の記録（訪問者の最高値を保存）
  CFLAG:TARGET:比較基準_サイズ = MAX(CFLAG:TARGET:比較基準_サイズ, CFLAG:訪問者:チンポ_サイズ)
  CFLAG:TARGET:比較基準_持続力 = MAX(CFLAG:TARGET:比較基準_持続力, CFLAG:訪問者:チンポ_持続力)
  CFLAG:TARGET:比較基準_顔 = MAX(CFLAG:TARGET:比較基準_顔, CFLAG:訪問者:顔ランク)
  CFLAG:TARGET:最終NTR相手 = 訪問者

  ; NTR経験回数カウント
  CFLAG:TARGET:NTR経験回数 += 1

  ; 浮気リスク設定
  CFLAG:TARGET:浮気リスク = 30 + CFLAG:TARGET:NTR経験回数 * 10
```

---

## プラトニック好感度

### 上昇方法

| 行動 | 上昇量 | 条件 |
|------|:------:|------|
| 会話 | +1~3 | 毎日の積み重ね |
| プレゼント | +5~15 | 好みに合うもの |
| 助ける | +10~30 | イベント時 |
| 謝罪を聞く | +20~50 | 特殊イベント |
| デート（プラトニック） | +15~25 | 性的行為なし |
| 記念日を覚えている | +30 | 特定日 |

### NTR完了後の特殊反応

```erb
@PLATONIC_INTERACTION(TARGET)
  ; NTR完了後は邪険にされる
  IF TALENT:TARGET:NTR
    IF CFLAG:TARGET:信頼度 < 0
      ; 冷淡な反応
      PRINTL 「……何か用？」
      PRINTL %CALLNAME:TARGET%は冷たい目でこちらを見た。
      ; 上昇量は通常の1/3
      RESULT = RESULT / 3
    ELSEIF CFLAG:TARGET:信頼度 < 300
      ; 警戒した反応
      PRINTL 「……また何か企んでるの？」
      ; 上昇量は通常の1/2
      RESULT = RESULT / 2
    ENDIF
  ENDIF
```

---

## 肉欲的好感度

### 主人公ステータスとの連動

```erb
@CALC_CARNAL_SATISFACTION(TARGET)
  ; 主人公と訪問者の比較
  LOCAL 主人公スコア = 0
  LOCAL 比較スコア = 0

  ; サイズ比較
  主人公スコア += CFLAG:MASTER:チンポ_サイズ * 20
  比較スコア += CFLAG:TARGET:比較基準_サイズ * 20

  ; 持続力比較
  主人公スコア += CFLAG:MASTER:チンポ_持続力 * 15
  比較スコア += CFLAG:TARGET:比較基準_持続力 * 15

  ; 顔比較
  主人公スコア += CFLAG:MASTER:顔ランク * 10
  比較スコア += CFLAG:TARGET:比較基準_顔 * 10

  ; 比較結果による満足度係数
  IF 主人公スコア >= 比較スコア
    ; 訪問者以上 → 高満足
    RESULT = 150  ; 1.5倍
  ELSEIF 主人公スコア >= 比較スコア * 0.8
    ; 8割程度 → 普通
    RESULT = 100  ; 1.0倍
  ELSEIF 主人公スコア >= 比較スコア * 0.5
    ; 半分程度 → 低満足
    RESULT = 50   ; 0.5倍
  ELSE
    ; 半分以下 → ほぼ満足できない
    RESULT = 20   ; 0.2倍
  ENDIF
```

### 性行為時の肉欲的好感度変動

```erb
@UPDATE_CARNAL_AFFECTION(TARGET, 行為タイプ)
  LOCAL 基本値 = 0

  SELECTCASE 行為タイプ
    CASE 行為_愛撫
      基本値 = 5
    CASE 行為_奉仕
      基本値 = 10
    CASE 行為_性交
      基本値 = 20
    CASE 行為_中出し
      基本値 = 30
  ENDSELECT

  ; 満足度係数を適用
  LOCAL 満足度 = CALC_CARNAL_SATISFACTION(TARGET)
  LOCAL 実際上昇 = 基本値 * 満足度 / 100

  ; NTR経験者は比較口上
  IF TALENT:TARGET:NTR && 満足度 < 80
    CALL COMPARISON_KOJO(TARGET, 満足度)
  ENDIF

  CFLAG:TARGET:肉欲的好感度 += 実際上昇
```

### 比較口上例

```erb
@COMPARISON_KOJO(TARGET, 満足度)
  IF 満足度 <= 20
    ; 全く満足できない
    PRINT 「……ん」
    PRINT %CALLNAME:TARGET%の反応は薄い。
    PRINT （%NTR_NAME(CFLAG:TARGET:最終NTR相手)%の時と比べると…）
    PRINT %CALLNAME:TARGET%は心の中で比較していた。
  ELSEIF 満足度 <= 50
    ; あまり満足できない
    PRINT 「あ、ん……」
    PRINT 控えめな喘ぎ声。
    PRINT （前はもっと……いえ、考えちゃダメ）
    PRINT %CALLNAME:TARGET%は首を振って邪念を払おうとした。
  ELSEIF 満足度 <= 80
    ; そこそこ
    PRINT 「んっ……%CALLNAME:MASTER%……」
    PRINT 声を出しながらも、どこか物足りなさそうな表情。
  ELSE
    ; 満足
    PRINT 「あっ、いい……！」
    PRINT %CALLNAME:TARGET%は素直に感じている。
    IF CFLAG:TARGET:NTR経験回数 > 0
      PRINT （%CALLNAME:MASTER%も、負けてない……）
    ENDIF
  ENDIF
```

---

## 信頼度システム

### 信頼度段階

| 信頼度 | 段階 | 状態 |
|:------:|------|------|
| -500～-200 | 絶望 | 会話すら拒否、復縁不可 |
| -199～0 | 拒絶 | 邪険にされる、基本拒否 |
| 1～200 | 警戒 | 最低限の会話のみ |
| 201～500 | 疑念 | 会話可能、行為は拒否 |
| 501～700 | 許容 | 行為可能、告白は要条件 |
| 701～900 | 回復 | 告白可能 |
| 901～1000 | 信頼 | 完全復縁可能 |

### 信頼度の自然回復

```erb
@DAILY_TRUST_RECOVERY(TARGET)
  IF !TALENT:TARGET:NTR
    RETURN
  ENDIF

  ; 時間経過で少しずつ回復（マイナス時のみ）
  IF CFLAG:TARGET:信頼度 < 0
    CFLAG:TARGET:信頼度 += 1  ; 1日+1
  ENDIF

  ; プラトニック好感度が高いと回復しやすい
  IF CFLAG:TARGET:プラトニック好感度 > 500
    CFLAG:TARGET:信頼度 += 1
  ENDIF

  ; 監視/贖罪状態で回復ボーナス
  IF TALENT:TARGET:監視対象
    CFLAG:TARGET:信頼度 += 2
  ENDIF
```

---

## 復縁ルート分岐

### 復縁条件

```erb
@CAN_RECONCILE(TARGET)
  ; 基本条件
  IF !TALENT:TARGET:NTR
    RETURN 0  ; NTR経験なし
  ENDIF

  IF CFLAG:TARGET:信頼度 < 500
    RETURN 0  ; 信頼度不足
  ENDIF

  ; プラトニック好感度チェック
  IF CFLAG:TARGET:プラトニック好感度 < 300
    RETURN 0
  ENDIF

  ; 肉欲的好感度チェック（条件付き）
  IF CFLAG:TARGET:肉欲的好感度 < 200
    ; 低くても贖罪ルートなら可能
    IF TALENT:TARGET:贖罪中
      RETURN 2  ; 贖罪復縁
    ENDIF
    RETURN 0
  ENDIF

  ; 完全復縁条件
  IF CFLAG:TARGET:プラトニック好感度 >= 700 && CFLAG:TARGET:肉欲的好感度 >= 500
    IF CFLAG:TARGET:信頼度 >= 800
      RETURN 1  ; 完全復縁可能
    ENDIF
  ENDIF

  RETURN 3  ; 条件付き復縁
```

### ルート種類

| ルート | 条件 | 特徴 |
|--------|------|------|
| **完全復縁** | プラトニック700+, 肉欲500+, 信頼800+ | 元通りに近い関係、再発リスク低 |
| **贖罪復縁** | 贖罪TALENT, 信頼500+ | 主従関係として復縁、監視下 |
| **条件付き復縁** | 信頼500+, 他条件不足 | オープン関係、再発リスク高 |
| **依存復縁** | 肉欲700+, プラトニック低 | 体だけの関係、不安定 |

---

## 再発リスクシステム

### リスク計算

```erb
@CALC_CHEATING_RISK(TARGET)
  IF !TALENT:TARGET:元NTR済み
    RETURN 0
  ENDIF

  LOCAL リスク = CFLAG:TARGET:浮気リスク

  ; NTR経験回数
  リスク += CFLAG:TARGET:NTR経験回数 * 10

  ; 復縁回数（繰り返すほど高リスク）
  リスク += CFLAG:TARGET:復縁回数 * 15

  ; 肉欲的好感度が低いとリスク上昇
  IF CFLAG:TARGET:肉欲的好感度 < 300
    リスク += 30
  ELSEIF CFLAG:TARGET:肉欲的好感度 < 500
    リスク += 15
  ENDIF

  ; 主人公ステータスが低いとリスク上昇
  LOCAL 満足度 = CALC_CARNAL_SATISFACTION(TARGET)
  IF 満足度 < 50
    リスク += 25
  ELSEIF 満足度 < 80
    リスク += 10
  ENDIF

  ; 監視状態でリスク低下
  IF TALENT:TARGET:監視対象
    リスク -= 30
  ENDIF

  ; 贖罪状態でリスク低下
  IF TALENT:TARGET:贖罪中
    リスク -= 20
  ENDIF

  ; 完全復縁ならリスク低下
  IF CFLAG:TARGET:信頼度 >= 900
    リスク -= 25
  ENDIF

  RESULT = LIMIT(リスク, 0, 95)  ; 0-95%
```

### 再発判定

```erb
@CHECK_CHEATING_TEMPTATION(TARGET)
  ; 訪問者と同室時に判定
  IF CFLAG:TARGET:現在位置 != FLAG:訪問者の現在位置
    RETURN 0
  ENDIF

  LOCAL リスク = CALC_CHEATING_RISK(TARGET)

  ; 毎日1回判定
  IF RAND:100 < リスク
    ; 再発の兆候イベント
    CALL CHEATING_TEMPTATION_EVENT(TARGET)
    RETURN 1
  ENDIF

  RETURN 0
```

### 再発イベント段階

| 段階 | イベント | 効果 |
|:----:|----------|------|
| 1 | 誘惑される | 警告、信頼度-10 |
| 2 | 揺らぐ | 選択肢、失敗で信頼度-50 |
| 3 | 一線を越える | NTR再発、関係リセット |

```erb
@CHEATING_TEMPTATION_EVENT(TARGET)
  CFLAG:TARGET:誘惑段階 += 1

  SELECTCASE CFLAG:TARGET:誘惑段階
    CASE 1
      ; 誘惑される
      PRINTL ───ふと、%CALLNAME:TARGET%の様子がおかしい。
      PRINTL %NTR_NAME(0)%と話している時、どこか懐かしそうな表情をしている。
      PRINTL
      PRINT 「……別に、何でもないわよ」
      CFLAG:TARGET:信頼度 -= 10

    CASE 2
      ; 揺らぐ
      PRINTL ───%CALLNAME:TARGET%が%NTR_NAME(0)%と二人きりでいるところを見かけた。
      PRINTL 何やら親しげに話している。
      PRINTL
      ; プレイヤー選択肢
      PRINTBUTTON [0] 声をかける, 0
      PRINTBUTTON [1] 様子を見る, 1
      PRINTBUTTON [2] 見なかったことにする, 2

      SELECTCASE RESULT
        CASE 0
          CALL INTERRUPT_TEMPTATION(TARGET)
          CFLAG:TARGET:誘惑段階 = 0
        CASE 1
          ; 何も起きない（今回は）
          CFLAG:TARGET:信頼度 -= 30
        CASE 2
          CFLAG:TARGET:信頼度 -= 50
      ENDSELECT

    CASE 3
      ; 一線を越える
      PRINTL ───嫌な予感がして%CALLNAME:TARGET%の部屋に向かうと…
      PRINTL 鍵がかかっていた。
      PRINTL 中から聞こえてくる声。
      PRINTL
      PRINT （また…また裏切られた…）
      ; NTR再発処理
      CALL ON_NTR_COMPLETE(TARGET, FLAG:訪問者ID)
      CFLAG:TARGET:誘惑段階 = 0
  ENDSELECT
```

---

## 訪問者の反応システム

### 概念

復縁は主人公とヒロインだけの問題ではない。訪問者がどう反応するかによって、復縁の難易度と展開が大きく変わる。

### 反応タイプ

```erb
; 訪問者の復縁への姿勢（FLAG管理）
; FLAG:訪問者_復縁姿勢 に設定

CONST 姿勢_興味喪失 = 0      ; もう興味がない
CONST 姿勢_一時貸与 = 1      ; 少しだけ貸してやる
CONST 姿勢_絶対阻止 = 2      ; 絶対に許さない、すぐ取り返す
CONST 姿勢_再寝取り楽 = 3    ; 再び寝取るのが楽しい
CONST 姿勢_無自覚 = 4        ; 気づいていない
```

### 1. もう興味がない（興味喪失）

```erb
@VISITOR_REACTION_興味喪失(TARGET)
  ; 訪問者はすでに次のターゲットに移行済み
  ; 復縁への妨害なし、ただしヒロインが未練を持つ可能性

  ; 発生条件
  ; - 訪問者が別キャラを攻略中
  ; - NTR完了から一定期間経過
  ; - 訪問者の性格: 飽き性

  ; 復縁への影響
  CFLAG:TARGET:訪問者干渉度 = 0  ; 干渉なし

  ; ただしヒロインの心理的影響
  IF CFLAG:TARGET:訪問者への依存度 > 50
    ; ヒロインが捨てられたショックで不安定に
    CFLAG:TARGET:精神不安定 = 1
    PRINTL （%NTR_NAME(0)%は……私のことなんて……）
    PRINTL %CALLNAME:TARGET%は複雑な表情をしている。
  ENDIF
```

**特徴**:
- 復縁は比較的容易
- しかしヒロインが「捨てられた」と感じる心理的ダメージ
- 主人公への依存が高まりやすい（反動）
- 再発リスクは低い（訪問者が来ない）

### 2. 少しだけ貸してやる（一時貸与）

```erb
@VISITOR_REACTION_一時貸与(TARGET)
  ; 訪問者は余裕を持ってヒロインを「貸す」
  ; 主人公を見下している状態

  ; 発生条件
  ; - 訪問者の優位が圧倒的（ステータス差大）
  ; - 訪問者の性格: サディスト、支配的
  ; - 主人公の成長が訪問者に及ばない

  ; 復縁中の特殊イベント
  IF FLAG:復縁進行中
    LOCAL 確率 = 20 + (CFLAG:訪問者:支配欲 * 5)
    IF RAND:100 < 確率
      CALL 一時貸与イベント(TARGET)
    ENDIF
  ENDIF

@一時貸与イベント(TARGET)
  PRINTL ───%NTR_NAME(0)%が主人公に近づいてきた。
  PRINTL
  PRINT 「%CALLNAME:TARGET%を返してやってもいいぜ」
  PRINT %NTR_NAME(0)%は余裕の笑みを浮かべる。
  PRINT 「どうせお前じゃ満足させられないだろうからな」
  PRINT
  PRINT 「そのうち、また俺のところに戻ってくるさ」
  ; 主人公の屈辱度上昇
  CFLAG:MASTER:屈辱度 += 30
```

**特徴**:
- 復縁は可能だが、訪問者が「許可」している形
- 主人公は屈辱を感じる
- 訪問者はいつでも取り返せると思っている
- 再発リスク高（訪問者が定期的に干渉）
- 主人公がステータスで訪問者を超えると状況が変化

### 3. 絶対に許さない、すぐ取り返す（絶対阻止）

```erb
@VISITOR_REACTION_絶対阻止(TARGET)
  ; 訪問者はヒロインを所有物と見なし、復縁を妨害
  ; 最も困難な復縁ルート

  ; 発生条件
  ; - 訪問者の独占欲が高い
  ; - ヒロインへの執着が強い
  ; - 訪問者の性格: 独占的、執着型

  ; 復縁判定への干渉
  CFLAG:TARGET:訪問者干渉度 = 50  ; 高干渉

  ; 毎日の妨害判定
  IF FLAG:復縁進行中
    IF RAND:100 < 40  ; 40%で妨害イベント
      CALL 復縁妨害イベント(TARGET)
    ENDIF
  ENDIF

@復縁妨害イベント(TARGET)
  LOCAL イベント = RAND:3

  SELECTCASE イベント
    CASE 0
      ; 直接干渉
      PRINTL %NTR_NAME(0)%が%CALLNAME:TARGET%を呼び出した。
      PRINTL 二人きりで何か話している……
      CFLAG:TARGET:プラトニック好感度 -= 20
      CFLAG:TARGET:信頼度 -= 10

    CASE 1
      ; 主人公への威嚇
      PRINTL ───%NTR_NAME(0)%が主人公の前に立ちはだかる。
      PRINT 「%CALLNAME:TARGET%に近づくな」
      PRINT 低い声で威嚇される。
      PRINT 「あいつは俺のものだ」
      CFLAG:MASTER:恐怖度 += 10

    CASE 2
      ; ヒロインの前で見せつけ
      PRINTL %NTR_NAME(0)%が%CALLNAME:TARGET%の前で
      PRINTL 主人公を嘲笑している。
      CFLAG:TARGET:肉欲的好感度 -= 15  ; 比較で劣等感
  ENDSELECT
```

**特徴**:
- 復縁が非常に困難
- 訪問者を何とかしないと進まない
- 主人公の成長 or 訪問者排除が必要
- 最もドラマチックな展開
- 成功すれば最も達成感がある

### 4. 再び寝取るのが楽しい（再寝取り楽）

```erb
@VISITOR_REACTION_再寝取り楽(TARGET)
  ; 訪問者は復縁を「楽しみ」として歓迎
  ; わざと復縁させてから再度寝取る

  ; 発生条件
  ; - 訪問者の性格: サディスト、愉悦型
  ; - 過去に複数回NTR経験あり
  ; - 訪問者が主人公を嬲ることを楽しんでいる

  ; 復縁を邪魔しない（わざと）
  CFLAG:TARGET:訪問者干渉度 = 0

  ; しかし復縁後の再発リスクを大幅上昇
  IF TALENT:TARGET:復縁済み
    CFLAG:TARGET:浮気リスク += 30  ; ベース上昇
    ; 再発イベントの追加条件
    FLAG:訪問者_狙撃中 = 1
  ENDIF

@再寝取り計画(TARGET)
  ; 復縁が成功したら訪問者が動き出す
  IF TALENT:TARGET:恋人 && FLAG:訪問者_狙撃中
    PRINTL ───%NTR_NAME(0)%の視線を感じる。
    PRINTL こちらを見て、にやりと笑っている。
    PRINTL
    PRINT （また……始まるのか……）
    ; 誘惑イベントの発生率上昇
    CFLAG:TARGET:誘惑段階発生率 = 50
  ENDIF
```

**特徴**:
- 復縁自体は比較的容易
- しかし復縁後が本番（罠）
- 再発リスクが極めて高い
- 「何度でも奪える」という訪問者の自信
- 主人公が気づいて対策を取るか、永遠にループ

### 5. 気づいていない（無自覚）

```erb
@VISITOR_REACTION_無自覚(TARGET)
  ; 訪問者は復縁の動きに気づいていない
  ; または興味がない

  ; 発生条件
  ; - 訪問者が多忙
  ; - 訪問者が別件に集中
  ; - 復縁が秘密裏に進行

  ; 干渉なし
  CFLAG:TARGET:訪問者干渉度 = 0

  ; ただし発覚リスク
  IF FLAG:復縁進行中
    ; 復縁行動のたびに発覚判定
    IF RAND:100 < 10  ; 10%で発覚
      CALL 復縁発覚イベント(TARGET)
    ENDIF
  ENDIF

@復縁発覚イベント(TARGET)
  PRINTL ───%NTR_NAME(0)%が二人の様子に気づいたようだ。
  PRINTL
  ; 訪問者の反応を再決定
  LOCAL 新反応 = RAND:4  ; 0-3（無自覚以外）
  FLAG:訪問者_復縁姿勢 = 新反応

  SELECTCASE 新反応
    CASE 姿勢_興味喪失
      PRINT 「ふーん……まあ、好きにすれば」
    CASE 姿勢_一時貸与
      PRINT 「へえ……復縁か。まあいいぜ、どうせ戻ってくる」
    CASE 姿勢_絶対阻止
      PRINT 「何やってんだ……%CALLNAME:TARGET%は俺のものだろ」
    CASE 姿勢_再寝取り楽
      PRINT 「……面白いな。楽しませてもらうか」
  ENDSELECT
```

**特徴**:
- 最も復縁しやすい状態
- ただし発覚リスクがある
- 発覚すると他の反応タイプに移行
- 秘密裏に進めるスリル

### 反応タイプ決定要因

```erb
@DETERMINE_VISITOR_STANCE(TARGET)
  ; NTR完了時に訪問者の姿勢を決定
  LOCAL 独占欲 = CFLAG:訪問者:独占欲
  LOCAL サディズム = CFLAG:訪問者:サディズム
  LOCAL 飽き性 = CFLAG:訪問者:飽き性
  LOCAL 執着度 = CFLAG:訪問者:TARGET:執着度

  ; 基本判定
  IF 執着度 > 80
    IF サディズム > 50
      FLAG:訪問者_復縁姿勢 = 姿勢_再寝取り楽
    ELSE
      FLAG:訪問者_復縁姿勢 = 姿勢_絶対阻止
    ENDIF
  ELSEIF 独占欲 > 70
    FLAG:訪問者_復縁姿勢 = 姿勢_絶対阻止
  ELSEIF サディズム > 70
    IF RAND:2
      FLAG:訪問者_復縁姿勢 = 姿勢_一時貸与
    ELSE
      FLAG:訪問者_復縁姿勢 = 姿勢_再寝取り楽
    ENDIF
  ELSEIF 飽き性 > 60
    FLAG:訪問者_復縁姿勢 = 姿勢_興味喪失
  ELSE
    FLAG:訪問者_復縁姿勢 = 姿勢_無自覚
  ENDIF
```

---

## NTR刻印の扱い

### 概要

NTR完了時にヒロインに付与されるMARK（快楽刻印、屈服刻印など）は、復縁後どうなるのか？

### 既存MARKシステム

```erb
; 現行システムのMARK（ヒロインに付与）
MARK:快楽刻印    ; 快楽で屈服した証
MARK:屈服刻印    ; 精神的に屈服した証
; ※その他、支配刻印、依存刻印など追加可能
```

### 刻印の永続性

```erb
; 基本方針: NTR刻印は「消えない」

; 復縁しても刻印は残る
; → これが復縁の難しさと再発リスクの根源

@ON_RECONCILE_SUCCESS(TARGET)
  ; 復縁成功時、刻印は消えない
  ; MARK:快楽刻印, MARK:屈服刻印 は維持

  ; ただし「封印」状態にできる
  IF CFLAG:TARGET:信頼度 >= 900 && CALC_CARNAL_SATISFACTION(TARGET) >= 100
    ; 完全復縁の場合、刻印を封印
    CFLAG:TARGET:刻印封印度 = 100
    PRINTL %CALLNAME:TARGET%の中で、あの記憶が薄れていく。
    PRINTL 今は目の前の%CALLNAME:MASTER%だけが見える。
  ELSE
    ; 不完全な復縁では刻印が疼く
    CFLAG:TARGET:刻印封印度 = 50
  ENDIF
```

### 刻印の影響

```erb
; 刻印封印度による効果

@MARK_EFFECT_CHECK(TARGET)
  IF !TALENT:TARGET:元NTR済み
    RETURN
  ENDIF

  LOCAL 封印度 = CFLAG:TARGET:刻印封印度

  ; 性行為時の比較口上発生率
  IF 封印度 >= 100
    ; 完全封印：比較口上なし
    CFLAG:TARGET:比較口上率 = 0
  ELSEIF 封印度 >= 70
    ; ほぼ封印：10%で発生
    CFLAG:TARGET:比較口上率 = 10
  ELSEIF 封印度 >= 50
    ; 半封印：30%で発生
    CFLAG:TARGET:比較口上率 = 30
  ELSE
    ; 封印弱い：50%で発生
    CFLAG:TARGET:比較口上率 = 50
  ENDIF

  ; 再発リスクへの影響
  LOCAL 刻印リスク = (100 - 封印度) / 2
  CFLAG:TARGET:浮気リスク += 刻印リスク
```

### 刻印関連口上

```erb
@KOJO_刻印疼き(TARGET)
  ; 訪問者を見かけた時、刻印が反応
  IF CFLAG:TARGET:刻印封印度 < 80
    IF MARK:TARGET:快楽刻印
      PRINTL %CALLNAME:TARGET%の体がぴくりと震えた。
      PRINT （体が……覚えてる……）
      PRINT 無意識に%NTR_NAME(CFLAG:TARGET:最終NTR相手)%を目で追ってしまう。
    ENDIF
    IF MARK:TARGET:屈服刻印
      PRINTL %CALLNAME:TARGET%の視線が自然と下がる。
      PRINT （この人の前だと……逆らえない……）
      PRINT 体が勝手に従順な姿勢を取ってしまう。
    ENDIF
  ELSE
    ; 封印が効いている
    PRINTL %CALLNAME:TARGET%は%NTR_NAME(0)%を見ても、
    PRINTL 特に動揺を見せなかった。
    PRINT （もう、あの人は関係ない）
    PRINT %CALLNAME:MASTER%の隣で、そう心に誓う。
  ENDIF

@KOJO_刻印上書き(TARGET)
  ; 主人公が訪問者を超えた時の特殊口上
  IF CALC_CARNAL_SATISFACTION(TARGET) >= 120
    IF MARK:TARGET:快楽刻印
      PRINTL 「あっ……！ %CALLNAME:MASTER%……！」
      PRINT %CALLNAME:TARGET%が今までにない声を上げる。
      PRINT
      PRINT （%CALLNAME:MASTER%の方が……すごい……）
      PRINT 体に刻まれた記憶が、塗り替えられていく。
      PRINT
      PRINT 「もう……%CALLNAME:MASTER%じゃないとダメ……」
      ; 刻印の効果を弱める
      CFLAG:TARGET:刻印封印度 += 20
    ENDIF
  ENDIF
```

### 刻印解除条件

```erb
; 刻印は「消える」のではなく「封印される」
; 完全解除は非常に困難

@CHECK_MARK_SEAL_PROGRESS(TARGET)
  ; 刻印封印度の上昇条件

  ; 1. 主人公との性行為で満足
  IF CALC_CARNAL_SATISFACTION(TARGET) >= 100
    CFLAG:TARGET:刻印封印度 += 2  ; 少しずつ
  ENDIF

  ; 2. 主人公ステータスが訪問者を大幅に超過
  IF CALC_CARNAL_SATISFACTION(TARGET) >= 150
    CFLAG:TARGET:刻印封印度 += 5
  ENDIF

  ; 3. 信頼度が最大に近い
  IF CFLAG:TARGET:信頼度 >= 950
    CFLAG:TARGET:刻印封印度 += 3
  ENDIF

  ; 4. 長期間訪問者と接触なし
  IF CFLAG:TARGET:訪問者非接触日数 > 30
    CFLAG:TARGET:刻印封印度 += 10
  ENDIF

  ; 上限
  CFLAG:TARGET:刻印封印度 = MIN(CFLAG:TARGET:刻印封印度, 100)

  ; 完全封印達成
  IF CFLAG:TARGET:刻印封印度 >= 100
    PRINTL ───%CALLNAME:TARGET%の瞳が澄んでいる。
    PRINT もう過去の影は、どこにもなかった。
    TALENT:TARGET:刻印克服 = 1
  ENDIF
```

### 刻印再活性化

```erb
; 封印された刻印も、条件次第で再活性化する

@CHECK_MARK_REACTIVATION(TARGET)
  IF !TALENT:TARGET:元NTR済み
    RETURN
  ENDIF

  ; 再活性化条件
  LOCAL 再活性化 = 0

  ; 1. 訪問者との接触
  IF CFLAG:TARGET:現在位置 == FLAG:訪問者の現在位置
    IF RAND:100 < (100 - CFLAG:TARGET:刻印封印度)
      再活性化 = 1
    ENDIF
  ENDIF

  ; 2. 主人公との性行為で不満
  IF CALC_CARNAL_SATISFACTION(TARGET) < 50
    IF RAND:100 < 30
      再活性化 = 1
    ENDIF
  ENDIF

  ; 3. 訪問者からの接触（絶対阻止/再寝取り楽タイプ）
  IF FLAG:訪問者_復縁姿勢 == 姿勢_絶対阻止 || FLAG:訪問者_復縁姿勢 == 姿勢_再寝取り楽
    IF RAND:100 < 20
      再活性化 = 1
    ENDIF
  ENDIF

  IF 再活性化
    CFLAG:TARGET:刻印封印度 -= 15
    CFLAG:TARGET:刻印封印度 = MAX(CFLAG:TARGET:刻印封印度, 0)
    PRINTL ───ふと、%CALLNAME:TARGET%の表情が曇った。
    PRINT （……思い出しちゃダメ……）
    PRINT 体の奥で、何かが疼いている。
  ENDIF
```

---

## 復縁専用口上

### 告白（再告白）

```erb
@KOJO_MESSAGE_告白成功_復縁(TARGET)
  IF !TALENT:TARGET:元NTR済み
    RETURN 0  ; 通常の告白口上へ
  ENDIF

  LOCAL 満足度 = CALC_CARNAL_SATISFACTION(TARGET)

  IF 満足度 >= 100
    ; 主人公が訪問者を超えた
    PRINT 「%CALLNAME:MASTER%……」
    PRINT %CALLNAME:TARGET%の目に涙が浮かぶ。
    PRINT 「私…あなたを裏切ったのに…」
    PRINT 「でも、あなたは…私を取り戻してくれた」
    PRINT
    PRINT 「もう、他の誰も見ない。約束する」
    PRINT 「だって…%CALLNAME:MASTER%が一番なんだもの」
  ELSEIF 満足度 >= 70
    ; 同等程度
    PRINT 「……本当にいいの？」
    PRINT %CALLNAME:TARGET%は不安そうに聞く。
    PRINT 「私、あんなことしたのに…」
    PRINT
    PRINT 「……ありがとう」
    PRINT 「今度こそ、裏切らない…努力する」
    PRINT 小さな声で付け加えた。
  ELSE
    ; 主人公が下回っている
    PRINT 「……わかった」
    PRINT %CALLNAME:TARGET%の返事は淡白だ。
    PRINT 「でも…正直に言うわね」
    PRINT
    PRINT 「%CALLNAME:MASTER%のこと、嫌いじゃない」
    PRINT 「でも……体は、まだ覚えてるの」
    PRINT 目を伏せる%CALLNAME:TARGET%。
    PRINT
    PRINT 「それでもいいなら……」
    ; 条件付き復縁フラグ
    TALENT:TARGET:条件付き復縁 = 1
  ENDIF

  RETURN 1
```

### 日常会話（復縁後）

```erb
@KOJO_会話_復縁後(TARGET)
  IF !TALENT:TARGET:元NTR済み || !TALENT:TARGET:恋人
    RETURN 0
  ENDIF

  LOCAL 満足度 = CALC_CARNAL_SATISFACTION(TARGET)
  LOCAL 信頼 = CFLAG:TARGET:信頼度

  IF 信頼 >= 900 && 満足度 >= 100
    ; 完全回復
    PRINT 「%CALLNAME:MASTER%」
    PRINT %CALLNAME:TARGET%が甘えるように寄り添ってきた。
    PRINT 「あの時は…本当にごめんなさい」
    PRINT 「でも今は、%CALLNAME:MASTER%だけよ」
  ELSEIF 信頼 >= 700
    ; 回復途上
    PRINT 「……今日も、一緒にいてくれるの？」
    PRINT 少し不安そうな%CALLNAME:TARGET%。
    PRINT 「嬉しい…ありがとう」
  ELSE
    ; まだ不安定
    PRINT 「……」
    PRINT %CALLNAME:TARGET%はどこか上の空だ。
    IF 満足度 < 50
      PRINT （物足りない…でも、そんなこと考えちゃダメ…）
    ENDIF
  ENDIF

  RETURN 1
```

---

## 実装フェーズ案

### 復縁基盤

| Phase | 内容 | Feature候補 |
|:-----:|------|:-----------:|
| K1 | 二軸好感度変数定義 | 210 |
| K2 | NTR完了時の状態変更処理 | 211 |
| K3 | プラトニック好感度システム | 212 |
| K4 | 肉欲的好感度・比較システム | 213 |
| K5 | 信頼度システム | 214 |

### 復縁プロセス

| Phase | 内容 | Feature候補 |
|:-----:|------|:-----------:|
| K6 | 復縁条件・ルート分岐 | 215 |
| K7 | 復縁専用イベント | 216-218 |
| K8 | 告白コマンド修正（復縁対応） | 219 |
| K9 | 復縁専用口上 | 220+ |

### 再発システム

| Phase | 内容 | Feature候補 |
|:-----:|------|:-----------:|
| L1 | 再発リスク計算 | 230 |
| L2 | 誘惑イベント（3段階） | 231 |
| L3 | 監視・対策システム | 232 |
| L4 | 再発専用口上 | 233+ |

### 訪問者反応システム

| Phase | 内容 | Feature候補 |
|:-----:|------|:-----------:|
| M1 | 訪問者姿勢決定システム | 240 |
| M2 | 興味喪失・無自覚ルート | 241 |
| M3 | 一時貸与ルート | 242 |
| M4 | 絶対阻止ルート | 243 |
| M5 | 再寝取り楽ルート | 244 |
| M6 | 訪問者反応口上 | 245+ |

### NTR刻印システム

| Phase | 内容 | Feature候補 |
|:-----:|------|:-----------:|
| N1 | 刻印封印度変数・基本処理 | 250 |
| N2 | 刻印の影響（比較口上率等） | 251 |
| N3 | 刻印解除条件・進行 | 252 |
| N4 | 刻印再活性化システム | 253 |
| N5 | 刻印関連口上（疼き・上書き） | 254+ |

---

## 主人公成長システムとの連携

### netorase-system.md Phase D との統合

```erb
; 主人公が成長すると復縁が有利に
@ON_MASTER_STAT_GROW(STAT_ID)
  ; 主人公成長時、全キャラの満足度を再計算
  FOR TARGET, 1, CHARANUM
    IF TALENT:TARGET:元NTR済み
      ; 肉欲的好感度の上限が上がる
      LOCAL 新上限 = CALC_CARNAL_MAX(TARGET)
      ; 成長を感じさせる口上フラグ
      IF CALC_CARNAL_SATISFACTION(TARGET) > CFLAG:TARGET:前回満足度
        CFLAG:TARGET:成長を感じたフラグ = 1
      ENDIF
    ENDIF
  NEXT
```

### 成長を感じる口上

```erb
@KOJO_成長を感じる(TARGET)
  IF !CFLAG:TARGET:成長を感じたフラグ
    RETURN 0
  ENDIF

  PRINT 「あっ……」
  PRINT %CALLNAME:TARGET%が驚いたように声を上げる。
  PRINT 「%CALLNAME:MASTER%……前より……」
  PRINT
  IF CALC_CARNAL_SATISFACTION(TARGET) >= 100
    PRINT 「……すごい」
    PRINT 頬を染めて%CALLNAME:TARGET%がつぶやく。
    PRINT （%NTR_NAME(CFLAG:TARGET:最終NTR相手)%より……）
  ELSE
    PRINT 「……変わった？」
    PRINT 少し驚いた様子の%CALLNAME:TARGET%。
  ENDIF

  CFLAG:TARGET:成長を感じたフラグ = 0
  RETURN 1
```

---

## バージョン配置案

| バージョン | 内容 | 備考 |
|:----------:|------|------|
| v2.4.1 | 復縁基盤（K1-K5） | NTR後ルートの拡張 |
| v2.4.2 | 復縁プロセス（K6-K9） | 告白システム改修含む |
| v2.4.3 | 再発システム（L1-L4） | 長期プレイ要素 |
| v2.4.4 | 訪問者反応（M1-M6） | 三角関係ドラマ |
| v2.4.5 | NTR刻印システム（N1-N5） | 永続的影響・克服 |

※ 主人公成長システム（v2.3）との連携が前提

---

## 未解決事項

1. **既存好感度との関係**: 現行の「好感度」をどう扱うか（プラトニックに統合？）
2. **復縁後のTALENT管理**: 元NTR済み + 恋人 の両立
3. **複数回NTRのバランス**: 何度もNTR→復縁を繰り返すケースの処理
4. **訪問者ステータスの保存**: 比較基準として記録するタイミング
5. **訪問者の複数人対応**: 複数の訪問者がいた場合の姿勢決定
6. **刻印の種類拡張**: 快楽刻印、屈服刻印以外の刻印タイプ（支配、依存など）
7. **訪問者排除システム**: 絶対阻止ルートで訪問者を追い出す手段

---

## 議論ログ

| 日付 | 内容 |
|------|------|
| 2025-12-17 | 初期設計。二軸好感度、主人公成長連携を中心に構成 |
| 2025-12-17 | 比較システム追加: 訪問者ステータスを基準に満足度計算 |
| 2025-12-17 | 再発リスクシステム: 3段階イベント、主人公ステータス影響 |
| 2025-12-17 | 訪問者反応システム追加: 5タイプ（興味喪失/一時貸与/絶対阻止/再寝取り楽/無自覚） |
| 2025-12-17 | NTR刻印システム追加: 封印度による影響、克服条件、再活性化 |
