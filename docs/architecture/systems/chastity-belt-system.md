# 貞操帯システム設計提案

## Status: DRAFT

**Target Version**: v2.6 (S2+)

## 概要

既存の貞操帯システム（NTR公衆便所用/ペニス用）を拡張し、主人公による装着機能、バリエーション、鍵管理システム、NTRシステム連携を統合した包括的な貞操帯システム。

## 背景・動機

### 現行システムの課題

1. **主人公が使えない**: NTR貞操帯は訪問者専用、主人公は鍵を買うだけ
2. **単一タイプ**: バリエーションがない（ペニス用のみ種別あり）
3. **単純な時間制限**: 鍵の有効期限が4時間固定
4. **NTR専用**: 純愛ルートでの活用ができない

### 設計思想

```
貞操帯の役割:
├── 純愛ルート: 防衛・独占・信頼の証
├── NTRルート: 支配・管理・屈辱
├── 寝取らせルート: 演出・制御
└── 復縁ルート: 贖罪・監視・再出発
```

---

## 変数設計

### キャラクター別変数（CFLAG）

```erb
; 貞操帯状態（住人ごと）
500,貞操帯_装着状態        ; 0=なし, 1=装着中
501,貞操帯_タイプ          ; 0=通常, 1=バイブ, 2=電撃, 3=GPS, 4=複合
502,貞操帯_装着者          ; 誰が装着したか（キャラID, -1=自発）
503,貞操帯_装着時刻        ; TIME_PROGRESS()の値
504,貞操帯_解除条件        ; 0=鍵のみ, 1=時間制限, 2=条件付き, 3=解除不可

; 鍵管理（住人ごと）
510,貞操帯_鍵所持者        ; 鍵を持っている人のID（-1=紛失）
511,貞操帯_鍵_複製可能     ; 0=不可, 1=可能
512,貞操帯_鍵_有効期限     ; 0=永続, それ以外=期限（TIME_PROGRESS値）
513,貞操帯_鍵_紛失フラグ   ; 紛失イベント発生済み
514,貞操帯_鍵_複製数       ; 存在する鍵の総数

; バイブ機能（タイプ1の場合）
520,貞操帯_バイブ_強度     ; 0-5 (0=停止)
521,貞操帯_バイブ_パターン ; 0=一定, 1=波状, 2=ランダム
522,貞操帯_バイブ_累計時間 ; バイブ稼働累計

; 電撃機能（タイプ2の場合）
530,貞操帯_電撃_強度       ; 0-3
531,貞操帯_電撃_発動条件   ; 0=手動, 1=興奮時, 2=接近時
532,貞操帯_電撃_累計回数   ; 電撃を受けた回数

; GPS機能（タイプ3の場合）
540,貞操帯_GPS_有効        ; 0=無効, 1=有効
541,貞操帯_GPS_警告範囲    ; 警告を出す範囲（場所ID）
542,貞操帯_GPS_違反回数    ; 範囲外に出た回数

; 心理的影響
550,貞操帯_装着日数        ; 累計装着日数
551,貞操帯_解除願望        ; 0-100（高いほど外したい）
552,貞操帯_受容度          ; 0-100（高いほど受け入れている）
553,貞操帯_依存度          ; 0-100（外すと不安になる）
```

### グローバル変数（FLAG）

```erb
; システム管理
FLAG:貞操帯_ショップ解放   ; ショップでの購入可能フラグ
FLAG:貞操帯_作成可能       ; パチュリー製作可能フラグ
```

### 定数定義

```erb
; 貞操帯タイプ
CONST 貞操帯_通常 = 0
CONST 貞操帯_バイブ = 1
CONST 貞操帯_電撃 = 2
CONST 貞操帯_GPS = 3
CONST 貞操帯_複合 = 4      ; バイブ+電撃+GPS

; 解除条件
CONST 解除_鍵のみ = 0
CONST 解除_時間制限 = 1
CONST 解除_条件付き = 2    ; 特定条件達成で解除
CONST 解除_不可 = 3        ; 破壊以外解除不可

; 装着者識別
CONST 装着者_自発 = -1
CONST 装着者_訪問者 = 0    ; 既存NTRシステムとの互換
```

---

## 貞操帯タイプ

### タイプ別仕様

| タイプ | 名称 | 主な効果 | 入手難易度 | 価格帯 |
|:------:|------|----------|:----------:|:------:|
| 0 | 通常型 | 装着のみ | 低 | 安 |
| 1 | バイブ内蔵型 | 自動快感付与 | 中 | 中 |
| 2 | 電撃型 | リモコンでショック | 中 | 中 |
| 3 | GPS型 | 位置追跡・警告 | 高 | 高 |
| 4 | 複合型 | 全機能搭載 | 最高 | 最高 |

### タイプ0: 通常型

```erb
@CHASTITY_EFFECT_通常(TARGET)
  ; 基本効果のみ
  ; - 性行為ブロック（鍵なし時）
  ; - 自慰不可
  ; - 装着感による羞恥

  IF CFLAG:TARGET:貞操帯_装着日数 > 7
    ; 1週間以上で慣れてくる
    CFLAG:TARGET:貞操帯_受容度 += 1
  ENDIF
```

### タイプ1: バイブ内蔵型

```erb
@CHASTITY_EFFECT_バイブ(TARGET)
  IF CFLAG:TARGET:貞操帯_バイブ_強度 == 0
    RETURN
  ENDIF

  ; 強度に応じた快感付与
  LOCAL 快感 = CFLAG:TARGET:貞操帯_バイブ_強度 * 10
  PALAM:TARGET:快楽 += 快感

  ; パターンによる変化
  SELECTCASE CFLAG:TARGET:貞操帯_バイブ_パターン
    CASE 1  ; 波状
      IF (TIME_PROGRESS() % 10) < 5
        PALAM:TARGET:快楽 += 快感 / 2
      ENDIF
    CASE 2  ; ランダム
      PALAM:TARGET:快楽 += RAND:(快感)
  ENDSELECT

  ; 口上トリガー
  IF PALAM:TARGET:快楽 > 500 && RAND:3 == 0
    CALL KOJO_貞操帯バイブ(TARGET)
  ENDIF

  CFLAG:TARGET:貞操帯_バイブ_累計時間 += 1
```

### タイプ2: 電撃型

```erb
@CHASTITY_SHOCK(TARGET, 強度)
  ; 電撃を発動
  PRINTL ───%CALLNAME:TARGET%の体がびくんと跳ねた。

  SELECTCASE 強度
    CASE 1
      PRINT 「ひっ……！」
      PRINT 軽い刺激が走る。
      PALAM:TARGET:苦痛 += 30
    CASE 2
      PRINT 「あっ……！ やめ……！」
      PRINT 鋭い痛みに表情が歪む。
      PALAM:TARGET:苦痛 += 60
      PALAM:TARGET:恐怖 += 20
    CASE 3
      PRINT 「いやああっ！！」
      PRINT 激しい電流に身体が震える。
      PALAM:TARGET:苦痛 += 100
      PALAM:TARGET:恐怖 += 50
      CFLAG:TARGET:貞操帯_受容度 -= 10
  ENDSELECT

  CFLAG:TARGET:貞操帯_電撃_累計回数 += 1
```

### タイプ3: GPS型

```erb
@CHASTITY_GPS_CHECK(TARGET)
  IF !CFLAG:TARGET:貞操帯_GPS_有効
    RETURN
  ENDIF

  ; 現在位置が許可範囲外かチェック
  LOCAL 現在地 = GET_LOCATION(TARGET)
  LOCAL 許可範囲 = CFLAG:TARGET:貞操帯_GPS_警告範囲

  IF !IS_IN_RANGE(現在地, 許可範囲)
    ; 範囲外警告
    CFLAG:TARGET:貞操帯_GPS_違反回数 += 1
    CALL GPS_違反通知(TARGET, 現在地)
  ENDIF

@GPS_違反通知(TARGET, 場所)
  ; 鍵所持者に通知
  LOCAL 所持者 = CFLAG:TARGET:貞操帯_鍵所持者

  IF 所持者 == MASTER
    PRINTL ───貞操帯のGPSから警告が届いた。
    PRINTL %CALLNAME:TARGET%が許可範囲外にいる。
    PRINTL 現在地: %場所名(場所)%
  ENDIF
```

---

## 装着・解除システム

### 装着コマンド

```erb
;==============================================================================
; COMF480: 貞操帯を装着させる
;==============================================================================
@COM480
  IF !CAN_MOUNT_CHASTITY(TARGET)
    PRINTL この状態では装着できない。
    RETURN 0
  ENDIF

  ; タイプ選択
  PRINTL どの貞操帯を装着させる？
  LOCAL タイプ = SELECT_CHASTITY_TYPE()
  IF タイプ < 0
    RETURN 0
  ENDIF

  ; 解除条件選択
  PRINTL 解除条件は？
  LOCAL 条件 = SELECT_UNLOCK_CONDITION()

  ; 装着処理
  CALL MOUNT_CHASTITY(TARGET, MASTER, タイプ, 条件)

  ; 口上
  CALL KOJO_貞操帯装着(TARGET, タイプ)

  RETURN 1

@MOUNT_CHASTITY(TARGET, 装着者, タイプ, 条件)
  CFLAG:TARGET:貞操帯_装着状態 = 1
  CFLAG:TARGET:貞操帯_タイプ = タイプ
  CFLAG:TARGET:貞操帯_装着者 = 装着者
  CFLAG:TARGET:貞操帯_装着時刻 = TIME_PROGRESS()
  CFLAG:TARGET:貞操帯_解除条件 = 条件

  ; 鍵は装着者が所持
  CFLAG:TARGET:貞操帯_鍵所持者 = 装着者
  CFLAG:TARGET:貞操帯_鍵_複製数 = 1

  ; 衣装システム連動
  EQUIP:TARGET:下半身下着１ = 100  ; 貞操帯

  ; 心理影響初期化
  CFLAG:TARGET:貞操帯_解除願望 = 30
  CFLAG:TARGET:貞操帯_受容度 = 0
```

### 装着可能条件

```erb
@CAN_MOUNT_CHASTITY(TARGET)
  #FUNCTION

  ; 既に装着中
  IF CFLAG:TARGET:貞操帯_装着状態
    RETURNF 0
  ENDIF

  ; 男性キャラ（ペニス用貞操帯は別システム）
  IF TALENT:TARGET:男
    RETURNF 0
  ENDIF

  ; 関係性チェック
  ; - 恋人/人妻なら無条件
  IF TALENT:TARGET:恋人 || TALENT:TARGET:人妻
    RETURNF 1
  ENDIF

  ; - それ以外は従順度または強制
  IF ABL:TARGET:従順 >= 5
    RETURNF 1
  ENDIF

  ; - 馴れ合い強度が高い
  IF TCVAR:TARGET:馴れ合い強度 >= 3
    RETURNF 1
  ENDIF

  RETURNF 0
```

### 解除コマンド

```erb
;==============================================================================
; COMF481: 貞操帯を解除する
;==============================================================================
@COM481
  IF !CFLAG:TARGET:貞操帯_装着状態
    PRINTL 貞操帯を装着していない。
    RETURN 0
  ENDIF

  IF !CAN_UNLOCK_CHASTITY(TARGET, MASTER)
    PRINTL 解除できない。
    RETURN 0
  ENDIF

  CALL DISMOUNT_CHASTITY(TARGET)
  CALL KOJO_貞操帯解除(TARGET)

  RETURN 1

@CAN_UNLOCK_CHASTITY(TARGET, 解除者)
  #FUNCTION

  ; 解除不可タイプ
  IF CFLAG:TARGET:貞操帯_解除条件 == 解除_不可
    RETURNF 0
  ENDIF

  ; 鍵を持っているか
  IF CFLAG:TARGET:貞操帯_鍵所持者 == 解除者
    RETURNF 1
  ENDIF

  ; 時間制限の場合、期限切れか
  IF CFLAG:TARGET:貞操帯_解除条件 == 解除_時間制限
    IF CFLAG:TARGET:貞操帯_鍵_有効期限 <= TIME_PROGRESS()
      RETURNF 1
    ENDIF
  ENDIF

  RETURNF 0

@DISMOUNT_CHASTITY(TARGET)
  ; 依存度が高い場合の影響
  IF CFLAG:TARGET:貞操帯_依存度 > 50
    PRINTL %CALLNAME:TARGET%は少し不安そうな表情を見せた。
    CFLAG:TARGET:貞操帯_依存度 -= 10
  ENDIF

  ; 状態リセット
  CFLAG:TARGET:貞操帯_装着状態 = 0
  CFLAG:TARGET:貞操帯_タイプ = 0
  ; 他の変数は履歴として保持

  ; 衣装システム連動
  EQUIP:TARGET:下半身下着１ = 0
```

---

## 鍵管理システム

### 鍵の操作

```erb
;==============================================================================
; 鍵を渡す
;==============================================================================
@GIVE_CHASTITY_KEY(TARGET, 渡す相手)
  IF CFLAG:TARGET:貞操帯_鍵所持者 != MASTER
    PRINTL 鍵を持っていない。
    RETURN 0
  ENDIF

  CFLAG:TARGET:貞操帯_鍵所持者 = 渡す相手

  IF 渡す相手 == 0  ; 訪問者
    PRINTL %NTR_NAME(0)%に%CALLNAME:TARGET%の貞操帯の鍵を渡した。
    ; NTRシステム連携
    FLAG:貞操帯鍵購入フラグ = TARGET  ; 既存変数との互換
  ELSE
    PRINTL %CALLNAME:渡す相手%に鍵を渡した。
  ENDIF

  RETURN 1

;==============================================================================
; 鍵を複製する
;==============================================================================
@DUPLICATE_CHASTITY_KEY(TARGET)
  IF !CFLAG:TARGET:貞操帯_鍵_複製可能
    PRINTL この貞操帯の鍵は複製できない。
    RETURN 0
  ENDIF

  ; 複製コスト
  IF MONEY < 5000
    PRINTL 複製には5000円必要。
    RETURN 0
  ENDIF

  MONEY -= 5000
  CFLAG:TARGET:貞操帯_鍵_複製数 += 1

  PRINTL 鍵を複製した。（現在%CFLAG:TARGET:貞操帯_鍵_複製数%本）

  RETURN 1

;==============================================================================
; 鍵紛失イベント
;==============================================================================
@KEY_LOST_EVENT(TARGET)
  IF CFLAG:TARGET:貞操帯_鍵_紛失フラグ
    RETURN 0  ; 既に発生済み
  ENDIF

  IF RAND:100 >= 5  ; 5%の確率
    RETURN 0
  ENDIF

  CFLAG:TARGET:貞操帯_鍵_紛失フラグ = 1
  CFLAG:TARGET:貞操帯_鍵所持者 = -1

  PRINTL ───%CALLNAME:TARGET%の貞操帯の鍵を紛失してしまった。
  PRINTL
  PRINT これでは解除できない…

  ; 対処法の提示
  PRINTL
  PRINTL ・パチュリーに相談する（マスターキー作成）
  PRINTL ・訪問者に頼む（屈辱的）
  PRINTL ・破壊する（貞操帯が壊れる）

  RETURN 1
```

### 時間制限システム

```erb
@SET_KEY_EXPIRY(TARGET, 時間)
  ; 時間は分単位
  CFLAG:TARGET:貞操帯_鍵_有効期限 = TIME_PROGRESS() + 時間
  CFLAG:TARGET:貞操帯_解除条件 = 解除_時間制限

@CHECK_KEY_EXPIRY(TARGET)
  IF CFLAG:TARGET:貞操帯_解除条件 != 解除_時間制限
    RETURN
  ENDIF

  IF CFLAG:TARGET:貞操帯_鍵_有効期限 <= TIME_PROGRESS()
    ; 期限切れ
    PRINTL %CALLNAME:TARGET%の貞操帯の鍵が使用可能になった。
    CFLAG:TARGET:貞操帯_解除条件 = 解除_鍵のみ
  ENDIF
```

---

## NTRシステム連携

### 既存システムとの統合

```erb
;==============================================================================
; NTR公衆便所システムとの互換
;==============================================================================
@NTR_CHASTITY_INTEGRATION(TARGET)
  ; 既存のNTR貞操帯フラグとの同期
  IF TALENT:TARGET:公衆便所
    ; 公衆便所は訪問者が管理
    IF !CFLAG:TARGET:貞操帯_装着状態
      CALL MOUNT_CHASTITY(TARGET, 0, 貞操帯_通常, 解除_鍵のみ)
    ENDIF
    CFLAG:TARGET:貞操帯_鍵所持者 = 0  ; 訪問者
  ENDIF

  ; 既存の鍵購入フラグとの連携
  IF FLAG:貞操帯鍵購入フラグ == TARGET
    ; 主人公が一時的に鍵を所持
    ; （有効期限内のみ）
    IF FLAG:貞操帯鍵有効カウンタ > TIME_PROGRESS()
      CFLAG:TARGET:貞操帯_鍵所持者 = MASTER
    ELSE
      CFLAG:TARGET:貞操帯_鍵所持者 = 0  ; 訪問者に戻る
      FLAG:貞操帯鍵購入フラグ = 0
    ENDIF
  ENDIF
```

### 寝取らせシステム連携

```erb
;==============================================================================
; 寝取らせ時の貞操帯活用
;==============================================================================
@NETORASE_CHASTITY_CONTROL(TARGET)
  ; 主人公が恋人に貞操帯を装着し、訪問者に鍵を渡す

  IF !CFLAG:TARGET:貞操帯_装着状態
    RETURN 0
  ENDIF

  ; 鍵を訪問者に渡している場合
  IF CFLAG:TARGET:貞操帯_鍵所持者 == 0
    ; 訪問者が「許可」を持つ
    ; NTRシステムの許可レベルとして機能
    RETURNF 1
  ENDIF

  RETURNF 0

;==============================================================================
; 鍵の所有権変更（NTR展開）
;==============================================================================
@NTR_KEY_TAKEOVER(TARGET)
  ; 訪問者が主人公から鍵を奪う
  IF CFLAG:TARGET:貞操帯_鍵所持者 != MASTER
    RETURN 0
  ENDIF

  PRINTL ───%NTR_NAME(0)%が%CALLNAME:TARGET%の貞操帯の鍵を奪い取った。
  PRINT 「これは俺が預かる」
  PRINT 「お前に%CALLNAME:TARGET%を触らせるのは、俺が許可した時だけだ」

  CFLAG:TARGET:貞操帯_鍵所持者 = 0
  CFLAG:MASTER:屈辱度 += 50

  RETURN 1
```

### 復縁システム連携

```erb
;==============================================================================
; 復縁時の貞操帯活用
;==============================================================================
@RECONCILE_CHASTITY_USE(TARGET)
  ; 復縁ルートでの貞操帯の役割

  ; 贖罪復縁: 監視の証として装着
  IF TALENT:TARGET:贖罪中
    IF !CFLAG:TARGET:貞操帯_装着状態
      ; 贖罪の一環として自発的に装着を提案
      PRINTL 「%CALLNAME:MASTER%…私に、貞操帯を付けてください」
      PRINTL 「もう二度と裏切らないって…証明したいから」

      ; 選択肢
      PRINTBUTTON [0] 装着する, 0
      PRINTBUTTON [1] 必要ない, 1

      IF RESULT == 0
        CALL MOUNT_CHASTITY(TARGET, MASTER, 貞操帯_GPS, 解除_条件付き)
        CFLAG:TARGET:信頼度 += 30
        CFLAG:TARGET:浮気リスク -= 20
      ENDIF
    ENDIF
  ENDIF

  ; 再発防止: GPS型で訪問者との接触を監視
  IF CFLAG:TARGET:貞操帯_タイプ == 貞操帯_GPS
    IF CFLAG:TARGET:貞操帯_GPS_違反回数 > 0
      ; 訪問者との接触を検知
      CFLAG:TARGET:浮気リスク += CFLAG:TARGET:貞操帯_GPS_違反回数 * 5
    ENDIF
  ENDIF
```

---

## 口上トリガー

### 装着時

```erb
@KOJO_貞操帯装着(TARGET, タイプ)
  SELECTCASE タイプ
    CASE 貞操帯_通常
      PRINT 「え…これを、付けるの…？」
      PRINT %CALLNAME:TARGET%は恥ずかしそうに目を逸らした。
    CASE 貞操帯_バイブ
      PRINT 「こ、これ…中に何か入って…」
      PRINT 不安そうな表情の%CALLNAME:TARGET%。
    CASE 貞操帯_電撃
      PRINT 「電気が流れる…？ そんな…」
      PRINT 怯えた目でこちらを見る。
    CASE 貞操帯_GPS
      PRINT 「どこにいても分かる…？」
      PRINT 「逃げられないってこと…？」
    CASE 貞操帯_複合
      PRINT 「全部…付いてるの…？」
      PRINT %CALLNAME:TARGET%は震えている。
  ENDSELECT

  ; TALENT別追加反応
  IF TALENT:TARGET:恋人
    PRINT
    PRINT 「%CALLNAME:MASTER%がそうしたいなら…」
    PRINT 「私は…受け入れるわ」
  ELSEIF TALENT:TARGET:元NTR済み
    PRINT
    PRINT 「…これで、安心してくれる？」
    PRINT どこか自嘲的な笑みを浮かべた。
  ENDIF
```

### 日常トリガー

```erb
@KOJO_貞操帯日常(TARGET)
  ; 装着日数による変化
  LOCAL 日数 = CFLAG:TARGET:貞操帯_装着日数

  IF 日数 < 3
    ; 慣れない
    PRINT 「まだ…慣れない…」
    PRINT 落ち着かない様子の%CALLNAME:TARGET%。
  ELSEIF 日数 < 7
    ; 意識する
    PRINT 歩くたびに貞操帯の存在を意識してしまう。
    PRINT %CALLNAME:TARGET%は時折、腰を気にしている。
  ELSEIF 日数 < 30
    ; 受け入れ始める
    PRINT 「…もう、付けてるのが普通になってきた」
    PRINT 少し複雑そうな表情。
  ELSE
    ; 依存
    IF CFLAG:TARGET:貞操帯_依存度 > 50
      PRINT 「これがないと…不安になる」
      PRINT %CALLNAME:TARGET%は%CALLNAME:MASTER%を見つめた。
      PRINT 「%CALLNAME:MASTER%に守られてるって感じがするから…」
    ENDIF
  ENDIF
```

---

## 実装フェーズ案

### Phase O: 貞操帯基盤

| Phase | 内容 | Feature候補 |
|:-----:|------|:-----------:|
| O1 | 変数定義（CFLAG/FLAG/定数） | 260 |
| O2 | 装着/解除コマンド基本実装 | 261 |
| O3 | 既存NTR貞操帯との統合 | 262 |
| O4 | 貞操帯基本口上 | 263+ |

### Phase P: バリエーション

| Phase | 内容 | Feature候補 |
|:-----:|------|:-----------:|
| P1 | バイブ型実装 | 270 |
| P2 | 電撃型実装 | 271 |
| P3 | GPS型実装 | 272 |
| P4 | 複合型実装 | 273 |
| P5 | バリエーション口上 | 274+ |

### Phase Q: 鍵管理

| Phase | 内容 | Feature候補 |
|:-----:|------|:-----------:|
| Q1 | 鍵複製システム | 280 |
| Q2 | 鍵紛失イベント | 281 |
| Q3 | 時間制限システム | 282 |
| Q4 | 鍵譲渡システム | 283 |
| Q5 | 鍵関連口上 | 284+ |

### Phase R: システム連携

| Phase | 内容 | Feature候補 |
|:-----:|------|:-----------:|
| R1 | 寝取らせ連携 | 290 |
| R2 | 復縁システム連携 | 291 |
| R3 | 妊娠システム連携 | 292 |
| R4 | 連携口上 | 293+ |

---

## バージョン配置案

| バージョン | 内容 | 備考 |
|:----------:|------|------|
| v2.5.3 | 貞操帯基盤（O1-O4） | 既存NTRとの統合 |
| v2.5.4 | バリエーション（P1-P5） | タイプ別機能 |
| v2.5.5 | 鍵管理（Q1-Q5） | 複製・紛失・譲渡 |
| v2.5.6 | システム連携（R1-R4） | 他システムとの統合 |

※ v2.5（NTR演出強化）のサブシステムとして実装

---

## 未解決事項

1. **ペニス用貞操帯との統合**: 既存の`貞操帯管理.ERB`との整合性
2. **入手方法**: ショップ購入 vs パチュリー製作 vs 訪問者から奪う
3. **破壊システム**: 無理やり外す場合のペナルティ
4. **複数キャラ管理**: 複数人に同時装着した場合のUI

---

## 議論ログ

| 日付 | 内容 |
|------|------|
| 2025-12-17 | 初期設計。4拡張方向（装着/バリエ/鍵/NTR連携）の両立性確認 |
| 2025-12-17 | 変数設計、コマンド設計、タイプ別仕様を定義 |
| 2025-12-17 | NTR/寝取らせ/復縁システムとの連携ポイントを設計 |
