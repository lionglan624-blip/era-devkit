# 風俗店・モブ客システム設計

## Status: DRAFT

**Target Version**: v2.1 (S2)

## 概要

既存の「訪問者宅での客取り」を拡張し、専用の風俗店システムとモブ客の個体管理を実装する。
チンポステータス（ntr-flavor-stats）と統合し、客ごとの個性と記憶を持たせる。

---

## 1. 既存システム分析

### 1.1 現在の客システム

**ファイル**: `客口上.ERB`, `客汎用地の文.ERB`

```erb
; 既存の客種別（6種）
客種別_擬音系          ; 無口タイプ
客種別_DQN・口悪い系   ; 荒いタイプ
客種別_紳士のような変態系 ; 丁寧タイプ
客種別_おっさん系      ; 中年タイプ
客種別_キモデブ系      ; キモいタイプ
客種別_性経験少ない系  ; 初心タイプ

; 行為種別
0 = 帰宅
1 = 会話
2 = キス
3 = 69
4 = セックス
5 = 射精
```

**現在の制限**:
- 客は「種別」のみで個体がない
- チンポステータスなし
- リピーター概念なし
- 場所は訪問者宅のみ

---

## 2. 風俗店システム

### 2.1 店舗定義

```erb
; ========================================
; 風俗店定義
; ========================================

; 店舗種別
#DIM CONST 店舗_ソープ = 0       ; 本番あり
#DIM CONST 店舗_ヘルス = 1       ; 本番なし
#DIM CONST 店舗_ピンサロ = 2     ; 口のみ
#DIM CONST 店舗_イメクラ = 3     ; シチュエーション
#DIM CONST 店舗_デリヘル = 4     ; 派遣型

; 店舗ランク
#DIM CONST 店舗ランク_高級 = 2   ; 客質良い、単価高い
#DIM CONST 店舗ランク_普通 = 1   ; 標準
#DIM CONST 店舗ランク_大衆 = 0   ; 客質悪い、回転重視

; 場所追加
#DIM CONST 場所_風俗店 = XX      ; 新規場所ID
```

### 2.2 店舗データ

```erb
; FLAG配列で店舗情報を管理
; FLAG:風俗店_種別
; FLAG:風俗店_ランク
; FLAG:風俗店_営業時間開始  ; 18:00 = 1080
; FLAG:風俗店_営業時間終了  ; 02:00 = 1560
; FLAG:風俗店_本日客数
; FLAG:風俗店_累計売上
; FLAG:風俗店_評判          ; 0-100

@BROTHEL_INIT()
  FLAG:風俗店_種別 = 店舗_ソープ
  FLAG:風俗店_ランク = 店舗ランク_普通
  FLAG:風俗店_営業時間開始 = 1080  ; 18:00
  FLAG:風俗店_営業時間終了 = 1560  ; 26:00 (02:00)
  FLAG:風俗店_評判 = 50
  RETURN

@BROTHEL_IS_OPEN()
  LOCAL 現在 = TIME % 1440
  IF FLAG:風俗店_営業時間開始 <= 現在 || 現在 < (FLAG:風俗店_営業時間終了 - 1440)
    RESULT = 1
  ELSE
    RESULT = 0
  ENDIF
  RETURN
```

---

## 3. モブ客システム

### 3.1 モブ客の個体化

```erb
; ========================================
; モブ客個体管理
; ========================================

; 最大同時管理数
#DIM CONST MAX_MOB_CLIENTS = 20

; モブ客データ構造（配列で管理）
; FLAG:モブ客_{ID}_存在      ; 0=空き, 1=存在
; FLAG:モブ客_{ID}_性格種別  ; 既存6種
; FLAG:モブ客_{ID}_外見種別  ; 新規追加
; FLAG:モブ客_{ID}_年齢層    ; 若い/中年/老年
; FLAG:モブ客_{ID}_金持ち度  ; 0-100
;
; チンポステータス（ntr-flavor-stats統合）
; FLAG:モブ客_{ID}_チンポ長さ    ; -2～+3
; FLAG:モブ客_{ID}_チンポ太さ    ; -2～+3
; FLAG:モブ客_{ID}_チンポ硬度    ; -2～+3
; FLAG:モブ客_{ID}_チンポ持続力  ; -2～+3
; FLAG:モブ客_{ID}_精液量        ; -2～+3
; FLAG:モブ客_{ID}_精液濃度      ; -2～+3
;
; 記憶・履歴
; FLAG:モブ客_{ID}_来店回数
; FLAG:モブ客_{ID}_指名嬢       ; キャラID
; FLAG:モブ客_{ID}_満足度累計
; FLAG:モブ客_{ID}_最終来店日
```

### 3.2 モブ客生成

```erb
@MOB_CLIENT_CREATE()
  ; 空きスロットを探す
  LOCAL スロット = -1
  FOR I, 0, MAX_MOB_CLIENTS - 1
    IF FLAG:モブ客_{I}_存在 == 0
      スロット = I
      BREAK
    ENDIF
  NEXT

  SIF スロット == -1
    RETURN -1  ; 満員

  ; 基本情報生成
  FLAG:モブ客_{スロット}_存在 = 1
  FLAG:モブ客_{スロット}_性格種別 = RAND:6  ; 既存6種からランダム
  FLAG:モブ客_{スロット}_外見種別 = CALL MOB_GENERATE_APPEARANCE()
  FLAG:モブ客_{スロット}_年齢層 = CALL MOB_GENERATE_AGE()
  FLAG:モブ客_{スロット}_金持ち度 = 30 + RAND:50

  ; チンポステータス生成（店舗ランクで補正）
  CALL MOB_GENERATE_CHINPO(スロット)

  ; 初期化
  FLAG:モブ客_{スロット}_来店回数 = 1
  FLAG:モブ客_{スロット}_指名嬢 = -1
  FLAG:モブ客_{スロット}_満足度累計 = 0
  FLAG:モブ客_{スロット}_最終来店日 = DAY

  RESULT = スロット
  RETURN

@MOB_GENERATE_APPEARANCE()
  ; 外見種別（新規）
  LOCAL R = RAND:100

  IF R < 5
    RESULT = 外見_イケメン     ; 5%
  ELSEIF R < 20
    RESULT = 外見_普通_若い    ; 15%
  ELSEIF R < 50
    RESULT = 外見_普通_中年    ; 30%
  ELSEIF R < 70
    RESULT = 外見_ブサイク     ; 20%
  ELSEIF R < 85
    RESULT = 外見_デブ         ; 15%
  ELSEIF R < 95
    RESULT = 外見_キモデブ     ; 10%
  ELSE
    RESULT = 外見_老人         ; 5%
  ENDIF
  RETURN

@MOB_GENERATE_AGE()
  LOCAL R = RAND:100
  IF R < 20
    RESULT = 年齢層_若い    ; 20代
  ELSEIF R < 70
    RESULT = 年齢層_中年    ; 30-50代
  ELSE
    RESULT = 年齢層_老年    ; 60代以上
  ENDIF
  RETURN
```

### 3.3 チンポステータス生成

```erb
@MOB_GENERATE_CHINPO(客ID)
  ; 店舗ランクで基準値が変わる
  LOCAL 基準 = 0
  SELECTCASE FLAG:風俗店_ランク
    CASE 店舗ランク_高級
      基準 = 1   ; やや良い客が来る
    CASE 店舗ランク_普通
      基準 = 0
    CASE 店舗ランク_大衆
      基準 = -1  ; やや悪い客が来る
  ENDSELECT

  ; 各ステータスをランダム生成（-2～+3、基準で補正）
  FLAG:モブ客_{客ID}_チンポ長さ = LIMIT(基準 + RAND:4 - 2, -2, 3)
  FLAG:モブ客_{客ID}_チンポ太さ = LIMIT(基準 + RAND:4 - 2, -2, 3)
  FLAG:モブ客_{客ID}_チンポ硬度 = LIMIT(基準 + RAND:4 - 2, -2, 3)
  FLAG:モブ客_{客ID}_チンポ持続力 = LIMIT(基準 + RAND:4 - 2, -2, 3)
  FLAG:モブ客_{客ID}_精液量 = LIMIT(基準 + RAND:4 - 2, -2, 3)
  FLAG:モブ客_{客ID}_精液濃度 = LIMIT(基準 + RAND:4 - 2, -2, 3)

  ; 外見との相関
  ; イケメンは平均的、キモデブは極端
  IF FLAG:モブ客_{客ID}_外見種別 == 外見_キモデブ
    ; キモデブは巨根か短小の両極端
    IF RAND:2
      FLAG:モブ客_{客ID}_チンポ長さ = 2 + RAND:2  ; A～S
      FLAG:モブ客_{客ID}_チンポ太さ = 2 + RAND:2
    ELSE
      FLAG:モブ客_{客ID}_チンポ長さ = -2 + RAND:2  ; E～D
    ENDIF
    ; 精液量は多め
    FLAG:モブ客_{客ID}_精液量 = 1 + RAND:3
  ENDIF
  RETURN
```

### 3.4 モブ客の取得（行為用）

```erb
; モブ客のチンポステータスを行為計算用に取得
@MOB_GET_CHINPO_STAT(客ID, ステータス種別)
  SELECTCASE ステータス種別
    CASE "長さ"
      RESULT = FLAG:モブ客_{客ID}_チンポ長さ
    CASE "太さ"
      RESULT = FLAG:モブ客_{客ID}_チンポ太さ
    CASE "硬度"
      RESULT = FLAG:モブ客_{客ID}_チンポ硬度
    CASE "持続力"
      RESULT = FLAG:モブ客_{客ID}_チンポ持続力
    CASE "精液量"
      RESULT = FLAG:モブ客_{客ID}_精液量
    CASE "精液濃度"
      RESULT = FLAG:モブ客_{客ID}_精液濃度
  ENDSELECT
  RETURN

; 絶頂計算への適用（ntr-flavor-stats互換）
@MOB_CALC_ORGASM_MODIFIER(客ID, 部位)
  LOCAL 修正 = 100
  LOCAL 長さ = FLAG:モブ客_{客ID}_チンポ長さ
  LOCAL 太さ = FLAG:モブ客_{客ID}_チンポ太さ
  LOCAL 硬度 = FLAG:モブ客_{客ID}_チンポ硬度
  LOCAL 持続 = FLAG:モブ客_{客ID}_チンポ持続力

  SELECTCASE 部位
    CASE "V"
      修正 += 長さ * 10 + 太さ * 8 + 硬度 * 6
    CASE "A"
      修正 += 太さ * 12 + 長さ * 6 + 硬度 * 6
    CASE "C"
      修正 += 硬度 * 4
    CASE "M"  ; 口
      修正 += 太さ * 5 + 長さ * 3
  ENDSELECT

  修正 += 持続 * 5
  RESULT = 修正
  RETURN
```

---

## 4. リピーター・指名システム

### 4.1 指名システム

```erb
; ========================================
; 指名システム
; ========================================

@BROTHEL_CHECK_NOMINATION(嬢)
  ; 嬢を指名しているリピーターがいるか
  LOCAL 指名客数 = 0

  FOR I, 0, MAX_MOB_CLIENTS - 1
    SIF FLAG:モブ客_{I}_存在 == 0
      CONTINUE
    IF FLAG:モブ客_{I}_指名嬢 == 嬢
      指名客数++
    ENDIF
  NEXT

  RESULT = 指名客数
  RETURN

@BROTHEL_GET_NOMINATOR(嬢)
  ; 嬢を指名している客をランダムで1人返す
  LOCAL 候補:MAX_MOB_CLIENTS
  LOCAL 候補数 = 0

  FOR I, 0, MAX_MOB_CLIENTS - 1
    SIF FLAG:モブ客_{I}_存在 == 0
      CONTINUE
    IF FLAG:モブ客_{I}_指名嬢 == 嬢
      候補:候補数 = I
      候補数++
    ENDIF
  NEXT

  SIF 候補数 == 0
    RETURN -1

  RESULT = 候補:RAND:候補数
  RETURN
```

### 4.2 満足度と指名形成

```erb
@BROTHEL_UPDATE_SATISFACTION(客ID, 嬢, 満足度)
  ; 行為後の満足度更新
  FLAG:モブ客_{客ID}_満足度累計 += 満足度
  FLAG:モブ客_{客ID}_来店回数 += 1
  FLAG:モブ客_{客ID}_最終来店日 = DAY

  ; 指名形成判定
  IF FLAG:モブ客_{客ID}_指名嬢 == -1
    ; まだ指名嬢がいない
    IF 満足度 >= 80
      ; 高満足度で指名
      FLAG:モブ客_{客ID}_指名嬢 = 嬢
      PRINTFORML 客はあなたを指名客に加えた
    ENDIF
  ELSEIF FLAG:モブ客_{客ID}_指名嬢 != 嬢
    ; 別の嬢を指名中
    IF 満足度 >= 90
      ; 非常に高満足度で乗り換え
      FLAG:モブ客_{客ID}_指名嬢 = 嬢
      PRINTFORML 客の指名があなたに変わった
    ENDIF
  ENDIF
  RETURN

@BROTHEL_CALC_SATISFACTION(客ID, 嬢)
  ; 満足度計算
  LOCAL 満足度 = 50  ; 基準

  ; 嬢の技術
  満足度 += ABL:嬢:奉仕技術 / 10
  満足度 += ABL:嬢:性技 / 10

  ; 嬢の態度（好感度高いと積極的）
  IF CFLAG:嬢:好感度 > 3000
    満足度 += 10
  ENDIF

  ; 絶頂させられたらマイナス（客のプライド）
  IF TCVAR:客絶頂回数 > 0
    満足度 -= TCVAR:客絶頂回数 * 5
  ENDIF

  ; 嬢が絶頂したらプラス
  満足度 += TCVAR:嬢絶頂回数 * 8

  ; 客の性格補正
  SELECTCASE FLAG:モブ客_{客ID}_性格種別
    CASE 客種別_紳士のような変態系
      満足度 += 10  ; 満足しやすい
    CASE 客種別_DQN・口悪い系
      満足度 -= 10  ; 厳しい
  ENDSELECT

  RESULT = LIMIT(満足度, 0, 100)
  RETURN
```

---

## 5. 評判システム

### 5.1 店舗評判

```erb
; ========================================
; 評判システム
; ========================================

@BROTHEL_UPDATE_REPUTATION(変化量)
  FLAG:風俗店_評判 = LIMIT(FLAG:風俗店_評判 + 変化量, 0, 100)

  ; 評判による効果
  IF FLAG:風俗店_評判 >= 80
    ; 高評判: 良い客が来やすい
    PRINTFORML 店の評判が上がっている
  ELSEIF FLAG:風俗店_評判 <= 20
    ; 低評判: 悪い客が増える
    PRINTFORML 店の評判が下がっている
  ENDIF
  RETURN

@BROTHEL_REPUTATION_EFFECT()
  ; 評判による客層補正
  LOCAL 評判 = FLAG:風俗店_評判

  IF 評判 >= 80
    ; 高評判: 金持ち/紳士が増える
    RESULT = 2
  ELSEIF 評判 >= 60
    RESULT = 1
  ELSEIF 評判 >= 40
    RESULT = 0
  ELSEIF 評判 >= 20
    RESULT = -1
  ELSE
    ; 低評判: DQN/キモデブが増える
    RESULT = -2
  ENDIF
  RETURN
```

### 5.2 口コミシステム

```erb
@BROTHEL_SPREAD_RUMOR(嬢, 内容)
  ; 嬢についての噂が広がる

  SELECTCASE 内容
    CASE "名器"
      PRINTFORML %CALLNAME:嬢%は名器だと噂が広がった
      FLAG:風俗店_評判 += 5
      ; 嬢への指名増加
      FOR I, 0, MAX_MOB_CLIENTS - 1
        SIF FLAG:モブ客_{I}_存在 == 0
          CONTINUE
        IF RAND:100 < 20
          FLAG:モブ客_{I}_指名嬢 = 嬢
        ENDIF
      NEXT

    CASE "淫乱"
      PRINTFORML %CALLNAME:嬢%は淫乱だと噂が広がった
      ; 特定タイプの客が増える

    CASE "サービス悪い"
      PRINTFORML %CALLNAME:嬢%はサービスが悪いと噂が広がった
      FLAG:風俗店_評判 -= 3
      ; 指名解除
      FOR I, 0, MAX_MOB_CLIENTS - 1
        IF FLAG:モブ客_{I}_指名嬢 == 嬢
          IF RAND:100 < 30
            FLAG:モブ客_{I}_指名嬢 = -1
          ENDIF
        ENDIF
      NEXT

  ENDSELECT
  RETURN
```

---

## 6. 客の来店フロー

### 6.1 客の生成・選択

```erb
@BROTHEL_GENERATE_CLIENT()
  ; 新規客 or リピーターを決定
  LOCAL R = RAND:100
  LOCAL 評判補正 = CALL BROTHEL_REPUTATION_EFFECT()

  ; リピーター率（評判で変動）
  LOCAL リピーター率 = 30 + 評判補正 * 5

  IF R < リピーター率
    ; リピーター
    LOCAL 客ID = CALL BROTHEL_SELECT_REPEATER()
    IF 客ID >= 0
      PRINTFORML リピーターの客が来店した
      RESULT = 客ID
      RETURN
    ENDIF
  ENDIF

  ; 新規客
  LOCAL 客ID = CALL MOB_CLIENT_CREATE()
  IF 客ID >= 0
    PRINTFORML 新規の客が来店した
    CALL BROTHEL_DESCRIBE_CLIENT(客ID)
  ENDIF
  RESULT = 客ID
  RETURN

@BROTHEL_SELECT_REPEATER()
  ; リピーターからランダム選択
  LOCAL 候補:MAX_MOB_CLIENTS
  LOCAL 候補数 = 0

  FOR I, 0, MAX_MOB_CLIENTS - 1
    SIF FLAG:モブ客_{I}_存在 == 0
      CONTINUE
    IF FLAG:モブ客_{I}_来店回数 >= 2
      候補:候補数 = I
      候補数++
    ENDIF
  NEXT

  SIF 候補数 == 0
    RETURN -1

  RESULT = 候補:RAND:候補数
  RETURN
```

### 6.2 客の描写

```erb
@BROTHEL_DESCRIBE_CLIENT(客ID)
  ; 客の外見を描写
  LOCAL 外見 = FLAG:モブ客_{客ID}_外見種別
  LOCAL 性格 = FLAG:モブ客_{客ID}_性格種別
  LOCAL 年齢 = FLAG:モブ客_{客ID}_年齢層

  ; 年齢描写
  SELECTCASE 年齢
    CASE 年齢層_若い
      PRINTFORM 若い
    CASE 年齢層_中年
      PRINTFORM 中年の
    CASE 年齢層_老年
      PRINTFORM 老年の
  ENDSELECT

  ; 外見描写
  SELECTCASE 外見
    CASE 外見_イケメン
      PRINTFORML イケメンの客だ
    CASE 外見_普通_若い
      PRINTFORML 普通の見た目の客だ
    CASE 外見_普通_中年
      PRINTFORML 普通のサラリーマン風の客だ
    CASE 外見_ブサイク
      PRINTFORML 不細工な客だ
    CASE 外見_デブ
      PRINTFORML 太った客だ
    CASE 外見_キモデブ
      PRINTFORML 脂ぎった太った客だ
    CASE 外見_老人
      PRINTFORML 年配の客だ
  ENDSELECT

  ; リピーターなら追加情報
  IF FLAG:モブ客_{客ID}_来店回数 >= 2
    PRINTFORML （{FLAG:モブ客_{客ID}_来店回数}回目の来店）
  ENDIF
  RETURN
```

---

## 7. 口上統合

### 7.1 モブ客用口上

```erb
; ========================================
; モブ客口上（チンポステータス対応）
; ========================================

@KOJO_MOB_CHINPO(嬢, 客ID)
  ; 客のチンポに対する嬢の反応
  LOCAL 長さ = FLAG:モブ客_{客ID}_チンポ長さ
  LOCAL 太さ = FLAG:モブ客_{客ID}_チンポ太さ
  LOCAL 硬度 = FLAG:モブ客_{客ID}_チンポ硬度

  ; 主人との比較
  LOCAL 主人長さ = CFLAG:MASTER:チンポ_長さ_ランク
  LOCAL 差 = 長さ - 主人長さ

  ; サイズ反応
  IF 長さ >= 2  ; A以上
    PRINTL （大きい…）
    IF 差 >= 2
      PRINTL （%CALLNAME:MASTER%よりずっと…）
    ENDIF
  ELSEIF 長さ <= -1  ; D以下
    PRINTL （小さい…楽かも…）
  ENDIF

  ; 太さ反応
  IF 太さ >= 2
    PRINTL （太い…入るかな…）
  ENDIF

  ; 硬度反応
  IF 硬度 >= 2
    PRINTL （硬い…）
  ELSEIF 硬度 <= -1
    PRINTL （柔らかい…）
  ENDIF
  RETURN

@KOJO_MOB_EJACULATION(嬢, 客ID)
  ; 客の射精時
  LOCAL 量 = FLAG:モブ客_{客ID}_精液量
  LOCAL 濃度 = FLAG:モブ客_{客ID}_精液濃度

  ; 量
  IF 量 >= 2
    PRINTL びゅるるっ…！
    PRINTL （多い…溢れる…）
  ELSEIF 量 <= -1
    PRINTL びゅっ…
    PRINTL （少ない…）
  ENDIF

  ; 濃度
  IF 濃度 >= 2
    PRINTL （濃い…ドロドロ…）
  ELSEIF 濃度 <= -1
    PRINTL （薄い…水っぽい…）
  ENDIF

  ; 訪問者との比較（浮気快楽刻印持ちの場合）
  IF MARK:嬢:浮気快楽刻印 > 5
    PRINTL （あの人のより…）
    IF 量 + 濃度 < CFLAG:人物_訪問者:チンポ_精液量 + CFLAG:人物_訪問者:チンポ_精液濃度
      PRINTL （物足りない…）
    ENDIF
  ENDIF
  RETURN

@KOJO_MOB_COMPARE_ALL(嬢, 客ID)
  ; 客と主人・訪問者の総合比較
  LOCAL 客総合 = FLAG:モブ客_{客ID}_チンポ長さ + FLAG:モブ客_{客ID}_チンポ太さ
  LOCAL 主人総合 = CFLAG:MASTER:チンポ_長さ_ランク + CFLAG:MASTER:チンポ_太さ_ランク
  LOCAL 訪問者総合 = CFLAG:人物_訪問者:チンポ_長さ_ランク + CFLAG:人物_訪問者:チンポ_太さ_ランク

  IF 客総合 > 訪問者総合 && 客総合 > 主人総合
    PRINTL （この客…%CALLNAME:MASTER%よりも、あの人よりも…）
  ELSEIF 客総合 > 主人総合
    PRINTL （%CALLNAME:MASTER%より大きい…）
  ELSE
    PRINTL （%CALLNAME:MASTER%の方がいい…）
  ENDIF
  RETURN
```

---

## 8. 料金システム

### 8.1 料金計算

```erb
@BROTHEL_CALC_PRICE(嬢, コース)
  LOCAL 基本料金 = 0

  ; コース別基本料金
  SELECTCASE コース
    CASE コース_ショート   ; 30分
      基本料金 = 10000
    CASE コース_スタンダード ; 60分
      基本料金 = 20000
    CASE コース_ロング     ; 90分
      基本料金 = 30000
    CASE コース_フリー     ; 120分
      基本料金 = 50000
  ENDSELECT

  ; 店舗ランク補正
  SELECTCASE FLAG:風俗店_ランク
    CASE 店舗ランク_高級
      基本料金 = 基本料金 * 200 / 100
    CASE 店舗ランク_大衆
      基本料金 = 基本料金 * 70 / 100
  ENDSELECT

  ; 嬢の人気度補正
  LOCAL 指名数 = CALL BROTHEL_CHECK_NOMINATION(嬢)
  IF 指名数 >= 5
    基本料金 = 基本料金 * 130 / 100  ; 人気嬢は+30%
  ELSEIF 指名数 >= 3
    基本料金 = 基本料金 * 115 / 100
  ENDIF

  ; 指名料
  基本料金 += 2000  ; 指名料固定

  RESULT = 基本料金
  RETURN
```

---

## 9. 実装フェーズ

| Phase | 内容 | 優先度 | 依存 |
|:-----:|------|:------:|:----:|
| 1 | 場所「風俗店」追加 | 高 | - |
| 2 | モブ客個体管理 | 高 | Phase 1 |
| 3 | チンポステータス統合 | 高 | ntr-flavor-stats |
| 4 | 既存客口上との互換 | 中 | Phase 2 |
| 5 | リピーター・指名 | 中 | Phase 2 |
| 6 | 評判システム | 中 | Phase 5 |
| 7 | 料金システム | 低 | Phase 1 |
| 8 | 口上拡張 | 低 | 全Phase |

---

## 10. 既存システムとの統合

```erb
; 既存の客口上.ERBとの互換性維持
;
; 既存: 客口上(奴隷, 行為, 客種別)
; 拡張: 客口上_EX(奴隷, 行為, 客ID)
;
; 客ID >= 0: 個体管理モブ客（新システム）
; 客ID < 0: 従来の種別ベース（互換モード）

@客口上_EX(奴隷, 行為, 客ID)
  IF 客ID < 0
    ; 互換モード: 従来の客種別で処理
    LOCAL 客種別 = ABS(客ID) - 1
    CALL 客口上(奴隷, 行為, 客種別)
  ELSE
    ; 新システム: 個体ベースで処理
    LOCAL 客種別 = FLAG:モブ客_{客ID}_性格種別
    CALL 客口上(奴隷, 行為, 客種別)
    ; 追加: チンポステータス対応口上
    CALL KOJO_MOB_CHINPO(奴隷, 客ID)
  ENDIF
  RETURN
```

---

## 11. 風俗嬢モブシステム（主人公が利用）

### 11.1 概要

主人公が客として風俗店を利用するシステム。
風俗嬢モブは個体管理され、外見・スタイル・技術・名器度などのステータスを持つ。

### 11.2 風俗嬢個体管理

```erb
; ========================================
; 風俗嬢モブ個体管理
; ========================================

; 最大同時管理数
#DIM CONST MAX_MOB_GIRLS = 30

; 風俗嬢データ構造
; FLAG:風俗嬢_{ID}_存在
; FLAG:風俗嬢_{ID}_名前ID       ; 名前テーブル参照
; FLAG:風俗嬢_{ID}_年齢         ; 18-35
; FLAG:風俗嬢_{ID}_性格種別     ; 性格タイプ
; FLAG:風俗嬢_{ID}_外見種別     ; 顔の良さ
; FLAG:風俗嬢_{ID}_店舗ID       ; 所属店舗
;
; 身体ステータス（-2～+3 ランク制）
; FLAG:風俗嬢_{ID}_バスト       ; 胸の大きさ
; FLAG:風俗嬢_{ID}_ウエスト     ; くびれ
; FLAG:風俗嬢_{ID}_ヒップ       ; 尻の大きさ
; FLAG:風俗嬢_{ID}_美脚度       ; 脚の綺麗さ
; FLAG:風俗嬢_{ID}_肌質         ; 肌の綺麗さ
;
; 性的ステータス
; FLAG:風俗嬢_{ID}_締まり       ; 膣の締まり
; FLAG:風俗嬢_{ID}_名器度       ; 形状・ヒダ
; FLAG:風俗嬢_{ID}_濡れやすさ   ; 感度
; FLAG:風俗嬢_{ID}_フェラ技術   ; 口技
; FLAG:風俗嬢_{ID}_腰使い       ; テクニック
;
; 経験・履歴
; FLAG:風俗嬢_{ID}_勤続日数
; FLAG:風俗嬢_{ID}_総客数
; FLAG:風俗嬢_{ID}_主人公利用回数
; FLAG:風俗嬢_{ID}_主人公満足度累計
; FLAG:風俗嬢_{ID}_指名ランク   ; 店内順位
```

### 11.3 風俗嬢生成

```erb
@MOB_GIRL_CREATE(店舗ID)
  ; 空きスロットを探す
  LOCAL スロット = -1
  FOR I, 0, MAX_MOB_GIRLS - 1
    IF FLAG:風俗嬢_{I}_存在 == 0
      スロット = I
      BREAK
    ENDIF
  NEXT

  SIF スロット == -1
    RETURN -1

  ; 基本情報
  FLAG:風俗嬢_{スロット}_存在 = 1
  FLAG:風俗嬢_{スロット}_名前ID = RAND:100  ; 名前テーブルから
  FLAG:風俗嬢_{スロット}_年齢 = 18 + RAND:18  ; 18-35歳
  FLAG:風俗嬢_{スロット}_性格種別 = RAND:8
  FLAG:風俗嬢_{スロット}_店舗ID = 店舗ID

  ; 外見生成
  CALL MOB_GIRL_GENERATE_APPEARANCE(スロット, 店舗ID)

  ; 身体ステータス生成
  CALL MOB_GIRL_GENERATE_BODY(スロット, 店舗ID)

  ; 性的ステータス生成
  CALL MOB_GIRL_GENERATE_SEXUAL(スロット)

  ; 初期化
  FLAG:風俗嬢_{スロット}_勤続日数 = RAND:365
  FLAG:風俗嬢_{スロット}_総客数 = FLAG:風俗嬢_{スロット}_勤続日数 * (2 + RAND:3)
  FLAG:風俗嬢_{スロット}_主人公利用回数 = 0

  RESULT = スロット
  RETURN

@MOB_GIRL_GENERATE_APPEARANCE(嬢ID, 店舗ID)
  ; 外見種別（店舗ランクで補正）
  LOCAL 補正 = 0
  IF 店舗ID >= 0
    補正 = FLAG:店舗_{店舗ID}_ランク  ; 高級店は美人が多い
  ENDIF

  LOCAL R = RAND:100
  R -= 補正 * 15  ; 高級店補正

  IF R < 5
    FLAG:風俗嬢_{嬢ID}_外見種別 = 外見嬢_絶世美女   ; 5%
  ELSEIF R < 20
    FLAG:風俗嬢_{嬢ID}_外見種別 = 外見嬢_美人       ; 15%
  ELSEIF R < 50
    FLAG:風俗嬢_{嬢ID}_外見種別 = 外見嬢_可愛い     ; 30%
  ELSEIF R < 75
    FLAG:風俗嬢_{嬢ID}_外見種別 = 外見嬢_普通       ; 25%
  ELSEIF R < 90
    FLAG:風俗嬢_{嬢ID}_外見種別 = 外見嬢_地味       ; 15%
  ELSE
    FLAG:風俗嬢_{嬢ID}_外見種別 = 外見嬢_ブス       ; 10%
  ENDIF
  RETURN

@MOB_GIRL_GENERATE_BODY(嬢ID, 店舗ID)
  ; 身体ステータス（-2～+3）
  LOCAL 補正 = 0
  IF 店舗ID >= 0
    補正 = FLAG:店舗_{店舗ID}_ランク
  ENDIF

  ; バスト（分布を調整）
  LOCAL R = RAND:100
  IF R < 10
    FLAG:風俗嬢_{嬢ID}_バスト = -2  ; 貧乳 (A)
  ELSEIF R < 25
    FLAG:風俗嬢_{嬢ID}_バスト = -1  ; 小ぶり (B)
  ELSEIF R < 55
    FLAG:風俗嬢_{嬢ID}_バスト = 0   ; 普通 (C)
  ELSEIF R < 80
    FLAG:風俗嬢_{嬢ID}_バスト = 1   ; 大きめ (D-E)
  ELSEIF R < 95
    FLAG:風俗嬢_{嬢ID}_バスト = 2   ; 巨乳 (F-G)
  ELSE
    FLAG:風俗嬢_{嬢ID}_バスト = 3   ; 爆乳 (H以上)
  ENDIF

  ; 他のステータス（ランダム+店舗補正）
  FLAG:風俗嬢_{嬢ID}_ウエスト = LIMIT(補正 + RAND:4 - 2, -2, 3)
  FLAG:風俗嬢_{嬢ID}_ヒップ = LIMIT(RAND:5 - 2, -2, 3)
  FLAG:風俗嬢_{嬢ID}_美脚度 = LIMIT(補正 + RAND:4 - 2, -2, 3)
  FLAG:風俗嬢_{嬢ID}_肌質 = LIMIT(補正 + RAND:4 - 2, -2, 3)
  RETURN

@MOB_GIRL_GENERATE_SEXUAL(嬢ID)
  ; 性的ステータス（経験で変化しうる）

  ; 締まり（若いほど良い傾向）
  LOCAL 年齢補正 = (25 - FLAG:風俗嬢_{嬢ID}_年齢) / 5
  FLAG:風俗嬢_{嬢ID}_締まり = LIMIT(年齢補正 + RAND:4 - 2, -2, 3)

  ; 名器度（ランダム、稀）
  LOCAL R = RAND:100
  IF R < 3
    FLAG:風俗嬢_{嬢ID}_名器度 = 3   ; 超名器 3%
  ELSEIF R < 10
    FLAG:風俗嬢_{嬢ID}_名器度 = 2   ; 名器 7%
  ELSEIF R < 30
    FLAG:風俗嬢_{嬢ID}_名器度 = 1   ; やや名器 20%
  ELSEIF R < 70
    FLAG:風俗嬢_{嬢ID}_名器度 = 0   ; 普通 40%
  ELSE
    FLAG:風俗嬢_{嬢ID}_名器度 = -1  ; ガバ 30%
  ENDIF

  ; 濡れやすさ
  FLAG:風俗嬢_{嬢ID}_濡れやすさ = LIMIT(RAND:5 - 2, -2, 3)

  ; テクニック（勤続日数で上昇）
  LOCAL 経験補正 = FLAG:風俗嬢_{嬢ID}_勤続日数 / 100
  FLAG:風俗嬢_{嬢ID}_フェラ技術 = LIMIT(経験補正 + RAND:3 - 1, -2, 3)
  FLAG:風俗嬢_{嬢ID}_腰使い = LIMIT(経験補正 + RAND:3 - 1, -2, 3)
  RETURN
```

### 11.4 風俗嬢の性格種別

```erb
; 風俗嬢の性格タイプ
#DIM CONST 性格嬢_清楚系 = 0      ; おとなしい、恥じらい
#DIM CONST 性格嬢_ギャル系 = 1    ; 明るい、ノリがいい
#DIM CONST 性格嬢_お姉さん系 = 2  ; 大人っぽい、リード
#DIM CONST 性格嬢_ロリ系 = 3      ; 幼い、甘える
#DIM CONST 性格嬢_ドM系 = 4       ; 従順、責められたい
#DIM CONST 性格嬢_ドS系 = 5       ; 攻め、言葉責め
#DIM CONST 性格嬢_淡白系 = 6      ; 事務的、仕事感
#DIM CONST 性格嬢_情熱系 = 7      ; 本気、感情的

@MOB_GIRL_GET_PERSONALITY_DESC(嬢ID)
  SELECTCASE FLAG:風俗嬢_{嬢ID}_性格種別
    CASE 性格嬢_清楚系
      RESULTS = "清楚系"
    CASE 性格嬢_ギャル系
      RESULTS = "ギャル系"
    CASE 性格嬢_お姉さん系
      RESULTS = "お姉さん系"
    CASE 性格嬢_ロリ系
      RESULTS = "ロリ系"
    CASE 性格嬢_ドM系
      RESULTS = "ドM系"
    CASE 性格嬢_ドS系
      RESULTS = "ドS系"
    CASE 性格嬢_淡白系
      RESULTS = "淡白系"
    CASE 性格嬢_情熱系
      RESULTS = "情熱系"
  ENDSELECT
  RETURN
```

### 11.5 風俗嬢の描写

```erb
@MOB_GIRL_DESCRIBE(嬢ID)
  ; パネル/紹介文としての描写
  LOCAL 外見 = FLAG:風俗嬢_{嬢ID}_外見種別
  LOCAL 年齢 = FLAG:風俗嬢_{嬢ID}_年齢
  LOCAL バスト = FLAG:風俗嬢_{嬢ID}_バスト
  LOCAL 性格 = FLAG:風俗嬢_{嬢ID}_性格種別

  ; 名前
  PRINTFORM %MOB_GIRL_GET_NAME(嬢ID)%（{年齢}歳）

  ; 外見
  SELECTCASE 外見
    CASE 外見嬢_絶世美女
      PRINTFORML 息を呑むほどの美女だ
    CASE 外見嬢_美人
      PRINTFORML 整った顔立ちの美人だ
    CASE 外見嬢_可愛い
      PRINTFORML 可愛らしい顔立ちだ
    CASE 外見嬢_普通
      PRINTFORML 普通の容姿だ
    CASE 外見嬢_地味
      PRINTFORML 地味な印象だ
    CASE 外見嬢_ブス
      PRINTFORML お世辞にも美人とは言えない
  ENDSELECT

  ; スタイル
  PRINTFORM スタイル：
  ; バスト
  SELECTCASE バスト
    CASE 3
      PRINTFORM 爆乳
    CASE 2
      PRINTFORM 巨乳
    CASE 1
      PRINTFORM 大きめ
    CASE 0
      PRINTFORM 普通
    CASE -1
      PRINTFORM 小ぶり
    CASE -2
      PRINTFORM 貧乳
  ENDSELECT
  PRINTFORML 　%CALL MOB_GIRL_GET_PERSONALITY_DESC(嬢ID)%

  ; 人気度
  IF FLAG:風俗嬢_{嬢ID}_指名ランク <= 3
    PRINTFORML 【人気嬢】
  ENDIF
  RETURN

@MOB_GIRL_GET_NAME(嬢ID)
  ; 名前テーブルから取得（仮）
  LOCAL:STR 名前テーブル:100
  名前テーブル:0 = "あいり"
  名前テーブル:1 = "みく"
  名前テーブル:2 = "りな"
  名前テーブル:3 = "ゆあ"
  名前テーブル:4 = "ここあ"
  名前テーブル:5 = "のあ"
  名前テーブル:6 = "れな"
  名前テーブル:7 = "みお"
  名前テーブル:8 = "さら"
  名前テーブル:9 = "ひな"
  ; ... 以下続く

  LOCAL 名前ID = FLAG:風俗嬢_{嬢ID}_名前ID % 100
  RESULTS = 名前テーブル:名前ID
  RETURN
```

### 11.6 主人公の満足度計算

```erb
@MOB_GIRL_CALC_MASTER_SATISFACTION(嬢ID)
  ; 主人公の満足度計算
  LOCAL 満足度 = 50

  ; 外見
  満足度 += (FLAG:風俗嬢_{嬢ID}_外見種別 - 3) * -8  ; 美人ほど+

  ; 身体
  ; バストは好みによる（ここでは大きいほど+とする）
  満足度 += FLAG:風俗嬢_{嬢ID}_バスト * 3
  満足度 += FLAG:風俗嬢_{嬢ID}_ウエスト * 2  ; くびれ
  満足度 += FLAG:風俗嬢_{嬢ID}_肌質 * 2

  ; 性的満足
  満足度 += FLAG:風俗嬢_{嬢ID}_締まり * 5
  満足度 += FLAG:風俗嬢_{嬢ID}_名器度 * 8    ; 名器は大きな加点
  満足度 += FLAG:風俗嬢_{嬢ID}_フェラ技術 * 4
  満足度 += FLAG:風俗嬢_{嬢ID}_腰使い * 4

  ; 濡れやすさ（感じてる感）
  満足度 += FLAG:風俗嬢_{嬢ID}_濡れやすさ * 3

  ; 性格補正
  SELECTCASE FLAG:風俗嬢_{嬢ID}_性格種別
    CASE 性格嬢_情熱系
      満足度 += 10  ; 本気で感じてくれる
    CASE 性格嬢_淡白系
      満足度 -= 10  ; 仕事感
  ENDSELECT

  ; リピート補正（慣れた相手は安心）
  IF FLAG:風俗嬢_{嬢ID}_主人公利用回数 >= 3
    満足度 += 5
  ENDIF

  RESULT = LIMIT(満足度, 0, 100)
  RETURN
```

### 11.7 住人との比較

```erb
@MOB_GIRL_COMPARE_TO_RESIDENT(嬢ID, 住人)
  ; 風俗嬢と住人の比較（口上用）
  LOCAL 比較結果:10
  LOCAL 比較数 = 0

  ; バスト比較
  LOCAL 嬢バスト = FLAG:風俗嬢_{嬢ID}_バスト
  LOCAL 住人バスト = CFLAG:住人:バストランク  ; 既存変数想定

  IF 嬢バスト > 住人バスト + 1
    比較結果:比較数 = "胸は風俗嬢の勝ち"
    比較数++
  ELSEIF 住人バスト > 嬢バスト + 1
    比較結果:比較数 = "胸は住人の勝ち"
    比較数++
  ENDIF

  ; 締まり比較
  LOCAL 嬢締まり = FLAG:風俗嬢_{嬢ID}_締まり
  LOCAL 住人締まり = CFLAG:住人:Ｖ締まり  ; 既存変数

  IF 嬢締まり > 住人締まり
    比較結果:比較数 = "締まりは風俗嬢の勝ち"
    比較数++
  ELSEIF 住人締まり > 嬢締まり
    比較結果:比較数 = "締まりは住人の勝ち"
    比較数++
  ENDIF

  ; 名器度比較
  LOCAL 嬢名器 = FLAG:風俗嬢_{嬢ID}_名器度
  ; 住人の名器度は素質で判定
  LOCAL 住人名器 = TALENT:住人:名器 ? 2 # 0

  IF 嬢名器 > 住人名器
    比較結果:比較数 = "名器度は風俗嬢の勝ち"
    比較数++
  ELSEIF 住人名器 > 嬢名器
    比較結果:比較数 = "名器度は住人の勝ち"
    比較数++
  ENDIF

  RESULT = 比較数
  RETURN
```

### 11.8 主人公視点の口上

```erb
@KOJO_MASTER_USE_GIRL(嬢ID)
  ; 主人公が風俗嬢を利用する際の口上
  LOCAL 締まり = FLAG:風俗嬢_{嬢ID}_締まり
  LOCAL 名器 = FLAG:風俗嬢_{嬢ID}_名器度
  LOCAL 濡れ = FLAG:風俗嬢_{嬢ID}_濡れやすさ

  ; 挿入時
  IF 締まり >= 2
    PRINTL （締まる…！）
  ELSEIF 締まり <= -1
    PRINTL （緩いな…）
  ENDIF

  ; 名器
  IF 名器 >= 2
    PRINTL （これは…名器だ…！）
    PRINTL 中のヒダが絡みつくように刺激してくる
  ELSEIF 名器 >= 1
    PRINTL （いい形してる…）
  ENDIF

  ; 濡れ
  IF 濡れ >= 2
    PRINTL 彼女はすぐに濡れてきた
  ELSEIF 濡れ <= -1
    PRINTL あまり濡れていない…ローションが必要か
  ENDIF
  RETURN

@KOJO_MASTER_COMPARE_GIRL_RESIDENT(嬢ID, 住人)
  ; 主人公が風俗嬢と住人を比較する口上
  LOCAL 嬢名器 = FLAG:風俗嬢_{嬢ID}_名器度
  LOCAL 嬢締まり = FLAG:風俗嬢_{嬢ID}_締まり

  ; 思い出し比較
  IF TALENT:住人:恋慕  ; 恋人がいる場合
    PRINTL （%CALLNAME:住人%と比べると…）

    IF 嬢名器 > 1 && !TALENT:住人:名器
      PRINTL （この子の方が気持ちいい…）
    ELSEIF TALENT:住人:名器
      PRINTL （やっぱり%CALLNAME:住人%の方がいい…）
    ENDIF
  ENDIF
  RETURN

@KOJO_MASTER_AFTER_GIRL(嬢ID)
  ; 行為後の口上
  LOCAL 満足度 = CALL MOB_GIRL_CALC_MASTER_SATISFACTION(嬢ID)

  IF 満足度 >= 80
    PRINTL 大満足だ。また指名しよう
  ELSEIF 満足度 >= 60
    PRINTL なかなか良かった
  ELSEIF 満足度 >= 40
    PRINTL まあ普通か
  ELSE
    PRINTL 微妙だった…次は別の子にしよう
  ENDIF

  ; 住人との比較（恋人持ちの場合）
  FOR 住人, 1, CHARANUM - 1
    SIF !TALENT:住人:恋慕
      CONTINUE
    IF 満足度 >= 70
      PRINTL （%CALLNAME:住人%より良かったかも…）
    ELSE
      PRINTL （やっぱり%CALLNAME:住人%の方がいいな）
    ENDIF
    BREAK
  NEXT
  RETURN
```

### 11.9 指名・リピートシステム

```erb
@MOB_GIRL_UPDATE_AFTER_USE(嬢ID)
  ; 利用後の更新
  FLAG:風俗嬢_{嬢ID}_主人公利用回数 += 1
  FLAG:風俗嬢_{嬢ID}_総客数 += 1

  LOCAL 満足度 = CALL MOB_GIRL_CALC_MASTER_SATISFACTION(嬢ID)
  FLAG:風俗嬢_{嬢ID}_主人公満足度累計 += 満足度

  ; 高満足度でお気に入り登録
  IF 満足度 >= 80 && FLAG:風俗嬢_{嬢ID}_主人公利用回数 >= 2
    IF FLAG:主人公_お気に入り嬢 != 嬢ID
      FLAG:主人公_お気に入り嬢 = 嬢ID
      PRINTFORML %MOB_GIRL_GET_NAME(嬢ID)%をお気に入りに登録した
    ENDIF
  ENDIF
  RETURN

@MOB_GIRL_GET_FAVORITE()
  ; お気に入り嬢を取得
  RESULT = FLAG:主人公_お気に入り嬢
  RETURN

@MOB_GIRL_LIST_BY_SHOP(店舗ID)
  ; 店舗の在籍嬢一覧
  LOCAL 嬢リスト:MAX_MOB_GIRLS
  LOCAL 嬢数 = 0

  FOR I, 0, MAX_MOB_GIRLS - 1
    SIF FLAG:風俗嬢_{I}_存在 == 0
      CONTINUE
    IF FLAG:風俗嬢_{I}_店舗ID == 店舗ID
      嬢リスト:嬢数 = I
      嬢数++
    ENDIF
  NEXT

  ; 人気順でソート（簡易）
  ; ...

  RESULT = 嬢数
  RETURN
```

---

## 12. 複数店舗システム

### 12.1 店舗管理

```erb
; ========================================
; 複数店舗管理
; ========================================

#DIM CONST MAX_SHOPS = 5

; 店舗データ
; FLAG:店舗_{ID}_存在
; FLAG:店舗_{ID}_名前ID
; FLAG:店舗_{ID}_種別       ; ソープ/ヘルス/etc
; FLAG:店舗_{ID}_ランク     ; 高級/普通/大衆
; FLAG:店舗_{ID}_場所       ; 人里/etc
; FLAG:店舗_{ID}_在籍数
; FLAG:店舗_{ID}_評判

@SHOP_INIT_DEFAULT()
  ; デフォルト店舗を初期化

  ; 店舗0: 高級ソープ
  FLAG:店舗_0_存在 = 1
  FLAG:店舗_0_種別 = 店舗_ソープ
  FLAG:店舗_0_ランク = 店舗ランク_高級
  FLAG:店舗_0_評判 = 70

  ; 店舗1: 普通のヘルス
  FLAG:店舗_1_存在 = 1
  FLAG:店舗_1_種別 = 店舗_ヘルス
  FLAG:店舗_1_ランク = 店舗ランク_普通
  FLAG:店舗_1_評判 = 50

  ; 店舗2: 大衆ピンサロ
  FLAG:店舗_2_存在 = 1
  FLAG:店舗_2_種別 = 店舗_ピンサロ
  FLAG:店舗_2_ランク = 店舗ランク_大衆
  FLAG:店舗_2_評判 = 40

  ; 各店舗に嬢を配置
  FOR 店舗, 0, 2
    LOCAL 配置数 = 5 + FLAG:店舗_{店舗}_ランク * 3
    FOR I, 0, 配置数 - 1
      CALL MOB_GIRL_CREATE(店舗)
    NEXT
  NEXT
  RETURN
```

---

## 議論ログ

| 日付 | 内容 |
|------|------|
| 2025-12-18 | 初期設計開始 |
| 2025-12-18 | 既存客口上システム分析（6種別） |
| 2025-12-18 | モブ客個体化設計 |
| 2025-12-18 | ntr-flavor-stats統合（チンポステータス） |
| 2025-12-18 | 風俗店システム設計（場所・料金・評判） |
| 2025-12-18 | リピーター・指名システム設計 |
| 2025-12-18 | **風俗嬢モブシステム追加**: 主人公が利用する側 |
| 2025-12-18 | 身体/性的ステータス設計（バスト/締まり/名器度等） |
| 2025-12-18 | 住人との比較口上設計 |
