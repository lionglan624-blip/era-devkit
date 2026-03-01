# 妊娠・出産・子育てシステム設計提案

## Status: DRAFT

**Target Version**: v2.7-v2.9 (S3)

## 概要

妊娠から出産、子育て、そして子の成長までを扱う包括的なシステム。NTRシステムとの連携により「誰の子か」という托卵要素を中心に、長期的なゲームプレイの動機付けと関係性の深化を実現する。

## 背景・動機

### 現行システムの課題

1. **長期目標の欠如**: 関係性を深めた先のコンテンツが限定的
2. **NTRの帰結不足**: 寝取られ/寝取らせの「結果」が見えにくい
3. **家族概念の不在**: 結婚しても家族としての展開がない

### 妊娠システムの位置付け

- NTRの「結果」を可視化（托卵、誰の子か問題）
- 長期プレイの動機付け（子の成長を見届ける）
- 新キャラクター追加のメカニズム
- 関係性の不可逆的変化（子を持つことで変わる関係）

---

## 変数設計

### 新規CFLAG（住人ごと）

```erb
; CSV/CFLAG.csv に追加
100,妊娠状態           ; 0:なし 1:妊娠初期 2:妊娠中期 3:妊娠後期 4:臨月
101,妊娠日数           ; 妊娠からの経過日数
102,胎児の父親         ; キャラID (0=主人公, >0=訪問者/他キャラ)
103,妊娠回数           ; 累計妊娠回数
104,出産回数           ; 累計出産回数
105,浮気妊娠回数       ; 主人公以外による妊娠回数
106,中出し回数_主人公   ; 主人公による中出し累計
107,中出し回数_他者     ; 主人公以外による中出し累計
108,妊娠願望度         ; 0-1000 (妊娠への欲求)
109,妊娠依存度         ; 0-1000 (妊娠中毒・ボテ腹依存)
110,母性               ; 0-1000 (子への愛着)
```

### 経産婦関連TALENT

```erb
; CSV/TALENT.csv に追加
200,経産婦             ; 出産経験あり
201,多産               ; 3回以上出産
202,孕み体質           ; 妊娠しやすい体質
203,ボテ腹好き         ; 妊娠状態を好む
204,托卵願望           ; 他の男の子を孕みたい
205,種付け済み         ; 現在妊娠中（表示用）
```

### 子供関連CFLAG

```erb
; 子供は新規キャラとして生成
; CFLAG追加
120,父親ID             ; 父親のキャラID
121,母親ID             ; 母親のキャラID
122,誕生日             ; ゲーム内日付
123,成長段階           ; 0:胎児 1:乳児 2:幼児 3:子供 4:少年少女 5:成人
124,血統タイプ         ; 0:主人公の子 1:訪問者の子 2:不明
125,容姿継承元         ; 父/母のどちらに似ているか
```

### グローバルFLAG

```erb
; DIM.ERH に追加
#DIM SAVEDATA FLAG:妊娠システム有効 = 0     ; 0:OFF 1:ON
#DIM SAVEDATA FLAG:托卵モード = 0           ; 0:OFF 1:ON (父親不明演出)
#DIM SAVEDATA FLAG:子供の成長速度 = 1       ; 1:通常 2:加速 3:即成長
#DIM SAVEDATA FLAG:出産済み子供数 = 0       ; 総子供数
```

---

## 妊娠判定システム

### 妊娠確率計算

```erb
@CALC_PREGNANCY_CHANCE(母親, 父親, 行為タイプ)
  LOCAL 基本確率 = 0

  ; 行為タイプによる基本確率
  SELECTCASE 行為タイプ
    CASE 行為_膣内射精
      基本確率 = 15  ; 15%
    CASE 行為_中出し連続
      基本確率 = 25  ; 25%
    CASE 行為_排卵日中出し
      基本確率 = 40  ; 40%
  ENDSELECT

  ; 体質補正
  IF TALENT:母親:孕み体質
    基本確率 *= 1.5
  ENDIF

  ; 経産婦補正（妊娠しやすくなる）
  IF TALENT:母親:経産婦
    基本確率 *= 1.2
  ENDIF

  ; 妊娠願望補正（心理的要因）
  基本確率 += CFLAG:母親:妊娠願望度 / 100

  ; 避妊具・薬の影響（将来実装）
  ; IF FLAG:避妊中
  ;   基本確率 *= 0.1
  ; ENDIF

  RESULT = MIN(基本確率, 80)  ; 上限80%
  RETURN

@TRY_PREGNANCY(母親, 父親, 行為タイプ)
  ; 既に妊娠中なら判定しない
  IF CFLAG:母親:妊娠状態 > 0
    RETURN 0
  ENDIF

  LOCAL 確率 = CALC_PREGNANCY_CHANCE(母親, 父親, 行為タイプ)

  IF RAND:100 < 確率
    ; 妊娠成立
    CALL START_PREGNANCY(母親, 父親)
    RETURN 1
  ENDIF

  RETURN 0
```

### 妊娠開始処理

```erb
@START_PREGNANCY(母親, 父親)
  CFLAG:母親:妊娠状態 = 1      ; 妊娠初期
  CFLAG:母親:妊娠日数 = 0
  CFLAG:母親:胎児の父親 = 父親
  CFLAG:母親:妊娠回数 += 1
  TALENT:母親:種付け済み = 1

  ; 浮気妊娠判定
  IF 父親 != MASTER && (TALENT:母親:恋人 || TALENT:母親:人妻)
    CFLAG:母親:浮気妊娠回数 += 1
  ENDIF

  ; 妊娠イベント発火
  CALL PREGNANCY_START_EVENT(母親, 父親)
```

### 父親判定（複数候補時）

```erb
@DETERMINE_FATHER(母親)
  ; 直近の中出し記録から確率的に父親を決定
  ; 簡易版: 最後の中出し相手を父親とする

  LOCAL 主人公確率 = CFLAG:母親:中出し回数_主人公
  LOCAL 他者確率 = CFLAG:母親:中出し回数_他者
  LOCAL 総確率 = 主人公確率 + 他者確率

  IF 総確率 == 0
    RESULT = MASTER  ; デフォルトは主人公
    RETURN
  ENDIF

  IF RAND:総確率 < 主人公確率
    RESULT = MASTER
  ELSE
    RESULT = FLAG:最後の中出し相手  ; 訪問者ID
  ENDIF

  RETURN
```

---

## 妊娠進行システム

### 妊娠段階

| 段階 | 日数 | 状態 | 外見変化 | 行動制限 |
|:----:|:----:|------|----------|----------|
| 1 | 0-30 | 妊娠初期 | なし | なし |
| 2 | 31-90 | 妊娠中期 | お腹が膨らみ始め | 激しい運動× |
| 3 | 91-180 | 妊娠後期 | 明らかなボテ腹 | 外出制限 |
| 4 | 181-270 | 臨月 | 大きなお腹 | 安静推奨 |
| 5 | 270+ | 出産 | - | 出産イベント |

### 日次進行処理

```erb
@DAILY_PREGNANCY_UPDATE(TARGET)
  IF CFLAG:TARGET:妊娠状態 == 0
    RETURN
  ENDIF

  CFLAG:TARGET:妊娠日数 += 1

  ; 段階更新
  LOCAL 日数 = CFLAG:TARGET:妊娠日数
  SELECTCASE 日数
    CASE 31
      CFLAG:TARGET:妊娠状態 = 2  ; 中期へ
      CALL PREGNANCY_STAGE_EVENT(TARGET, 2)
    CASE 91
      CFLAG:TARGET:妊娠状態 = 3  ; 後期へ
      CALL PREGNANCY_STAGE_EVENT(TARGET, 3)
    CASE 181
      CFLAG:TARGET:妊娠状態 = 4  ; 臨月へ
      CALL PREGNANCY_STAGE_EVENT(TARGET, 4)
    CASE 270
      CALL TRIGGER_BIRTH(TARGET)  ; 出産
  ENDSELECT

  ; つわり判定（初期〜中期）
  IF 日数 >= 14 && 日数 <= 60
    IF RAND:100 < 30
      CALL MORNING_SICKNESS_EVENT(TARGET)
    ENDIF
  ENDIF

  ; 妊娠依存度上昇（ボテ腹好きの場合）
  IF TALENT:TARGET:ボテ腹好き
    CFLAG:TARGET:妊娠依存度 += RAND:3
  ENDIF
```

---

## 出産システム

### 出産処理

```erb
@TRIGGER_BIRTH(母親)
  LOCAL 父親 = CFLAG:母親:胎児の父親

  ; 子キャラ生成
  LOCAL 子ID = CREATE_CHILD(母親, 父親)

  ; 母親ステータス更新
  CFLAG:母親:妊娠状態 = 0
  CFLAG:母親:妊娠日数 = 0
  CFLAG:母親:出産回数 += 1
  TALENT:母親:種付け済み = 0
  TALENT:母親:経産婦 = 1

  IF CFLAG:母親:出産回数 >= 3
    TALENT:母親:多産 = 1
  ENDIF

  ; 母性上昇
  CFLAG:母親:母性 += 200

  ; 出産イベント
  CALL BIRTH_EVENT(母親, 子ID, 父親)

  FLAG:出産済み子供数 += 1
```

### 子キャラ生成

```erb
@CREATE_CHILD(母親, 父親)
  ; 新規キャラIDを取得
  LOCAL 子ID = GET_NEW_CHARA_ID()

  ; 基本情報設定
  CFLAG:子ID:父親ID = 父親
  CFLAG:子ID:母親ID = 母親
  CFLAG:子ID:誕生日 = GAMEDAY
  CFLAG:子ID:成長段階 = 1  ; 乳児

  ; 血統タイプ判定
  IF 父親 == MASTER
    CFLAG:子ID:血統タイプ = 0  ; 主人公の子
  ELSEIF 父親 >= 訪問者ID開始
    CFLAG:子ID:血統タイプ = 1  ; 訪問者の子
  ELSE
    CFLAG:子ID:血統タイプ = 2  ; 不明
  ENDIF

  ; 容姿継承（ランダム）
  IF RAND:100 < 60
    CFLAG:子ID:容姿継承元 = 母親  ; 母親似
  ELSE
    CFLAG:子ID:容姿継承元 = 父親  ; 父親似
  ENDIF

  ; ステータス継承
  CALL INHERIT_STATS(子ID, 母親, 父親)

  RESULT = 子ID
  RETURN
```

---

## 托卵システム

### NTR連携

```erb
@CHECK_CUCKOLD_SITUATION(母親)
  ; 托卵状況の判定
  ; 条件: 恋人/人妻 かつ 父親が主人公以外

  IF !TALENT:母親:恋人 && !TALENT:母親:人妻
    RETURN 0  ; 交際/結婚していない
  ENDIF

  IF CFLAG:母親:胎児の父親 == MASTER
    RETURN 0  ; 主人公の子
  ENDIF

  RETURN 1  ; 托卵状態
```

### 父親判明イベント

```erb
@FATHER_REVELATION_EVENT(母親, 子ID)
  LOCAL 真の父親 = CFLAG:子ID:父親ID
  LOCAL 容姿元 = CFLAG:子ID:容姿継承元

  ; 托卵モードがONの場合、判明を遅らせる
  IF FLAG:托卵モード
    ; 子の容姿で徐々にバレる
    IF 容姿元 == 真の父親 && 真の父親 != MASTER
      ; 訪問者似の場合、成長とともに判明
      IF CFLAG:子ID:成長段階 >= 3  ; 子供以上
        CALL FATHER_REVEALED_KOJO(母親, 子ID, 真の父親)
      ENDIF
    ENDIF
  ELSE
    ; 即判明
    IF 真の父親 != MASTER
      CALL FATHER_REVEALED_KOJO(母親, 子ID, 真の父親)
    ENDIF
  ENDIF
```

### DNA検査システム（オプション）

```erb
@DNA_TEST(子ID)
  ; 高額アイテム使用で父親を確定
  LOCAL 真の父親 = CFLAG:子ID:父親ID

  PRINTL DNA検査の結果…

  IF 真の父親 == MASTER
    PRINT この子は間違いなくあなたの子です。
  ELSE
    PRINTFORML この子の父親は%CALLNAME:真の父親%です。
    ; 修羅場イベントトリガー
    CALL SHURABA_EVENT(CFLAG:子ID:母親ID, 真の父親)
  ENDIF
```

---

## 子の成長システム

### 成長段階

| 段階 | 名称 | 必要日数 | 特徴 |
|:----:|------|:--------:|------|
| 1 | 乳児 | 0 | 世話が必要、イベント少 |
| 2 | 幼児 | 365 | 言葉を覚える、かわいい盛り |
| 3 | 子供 | 1095 | 性格形成、親との会話 |
| 4 | 少年少女 | 2555 | 思春期、反抗期 |
| 5 | 成人 | 5475 | **攻略対象化可能** |

### 成長処理

```erb
@DAILY_CHILD_GROWTH(子ID)
  LOCAL 経過日数 = GAMEDAY - CFLAG:子ID:誕生日

  ; 成長速度補正
  経過日数 *= FLAG:子供の成長速度

  ; 段階判定
  LOCAL 新段階 = 1
  IF 経過日数 >= 5475
    新段階 = 5
  ELSEIF 経過日数 >= 2555
    新段階 = 4
  ELSEIF 経過日数 >= 1095
    新段階 = 3
  ELSEIF 経過日数 >= 365
    新段階 = 2
  ENDIF

  IF 新段階 > CFLAG:子ID:成長段階
    CFLAG:子ID:成長段階 = 新段階
    CALL GROWTH_STAGE_EVENT(子ID, 新段階)
  ENDIF
```

---

## 攻略対象化システム

### 成人した子の攻略対象化

```erb
@ENABLE_CHILD_AS_TARGET(子ID)
  ; 条件: 成人 (段階5)
  IF CFLAG:子ID:成長段階 < 5
    RETURN 0
  ENDIF

  ; キャラタイプを攻略対象に変更
  CFLAG:子ID:攻略対象フラグ = 1

  ; 血統による特殊属性
  IF CFLAG:子ID:血統タイプ == 0
    ; 主人公の娘 → 近親相姦ルート
    TALENT:子ID:主人公の娘 = 1
  ELSEIF CFLAG:子ID:血統タイプ == 1
    ; 訪問者の子 → NTR復讐ルート可能
    TALENT:子ID:訪問者の血筋 = 1
  ENDIF

  ; 容姿継承に基づくステータス生成
  CALL GENERATE_ADULT_STATS(子ID)

  RETURN 1
```

### 血統別ルート

| 血統 | 攻略ルート | 特殊要素 |
|------|-----------|----------|
| 主人公の娘 | 近親ルート | 背徳感、母親との三角関係 |
| 訪問者の子 | 復讐NTRルート | 母を寝取った男の娘を寝取り返す |
| 不明 | 謎解きルート | 真実を探る、DNA検査 |

### 世代間NTR

```erb
; 母娘丼、親子NTRなどの展開
@GENERATION_NTR_CHECK(母親, 娘)
  ; 母親が訪問者に寝取られている状態で
  ; 娘も同じ訪問者に狙われる/狙う

  IF TALENT:母親:NTR && CFLAG:娘:母親ID == 母親
    ; 母娘共有NTRルート解放
    RETURN 1
  ENDIF
  RETURN 0
```

---

## 妊娠依存システム

### 妊娠中毒（ボテ腹依存）

```erb
@UPDATE_PREGNANCY_ADDICTION(TARGET)
  ; 妊娠中は依存度上昇
  IF CFLAG:TARGET:妊娠状態 > 0
    IF TALENT:TARGET:ボテ腹好き
      CFLAG:TARGET:妊娠依存度 += RAND:5 + 2
    ELSE
      CFLAG:TARGET:妊娠依存度 += RAND:2
    ENDIF
  ELSE
    ; 非妊娠時は徐々に低下（依存度高いと下がりにくい）
    IF CFLAG:TARGET:妊娠依存度 > 500
      ; 高依存: 禁断症状
      CALL PREGNANCY_WITHDRAWAL(TARGET)
    ELSE
      CFLAG:TARGET:妊娠依存度 -= 1
    ENDIF
  ENDIF

  ; 依存度による行動変化
  IF CFLAG:TARGET:妊娠依存度 >= 800
    TALENT:TARGET:ボテ腹好き = 1  ; 自動取得
    ; 妊娠を求める行動
  ENDIF
```

### 依存段階

| 依存度 | 段階 | 症状 |
|:------:|------|------|
| 0-200 | 正常 | 特になし |
| 201-500 | 軽度 | 妊娠を意識する発言 |
| 501-700 | 中度 | 妊娠願望を隠さない |
| 701-900 | 重度 | 積極的に種付けを求める |
| 901-1000 | 中毒 | 常に妊娠していたい |

---

## 口上例

### 妊娠発覚

```erb
@PREGNANCY_DISCOVERY_KOJO(母親, 父親)
  IF 父親 == MASTER
    PRINT 「あの…%CALLNAME:MASTER%」
    PRINT %CALLNAME:母親%が恥ずかしそうに切り出す。
    PRINT 「私…赤ちゃんができたみたい」
  ELSE
    ; 浮気妊娠の場合
    IF TALENT:母親:恋人 || TALENT:母親:人妻
      PRINT 「……」
      PRINT %CALLNAME:母親%が何かを言いかけて、口をつぐむ。
      PRINT その表情には、喜びと…罪悪感が入り混じっていた。
    ENDIF
  ENDIF
```

### ボテ腹性交

```erb
@BOTEBARA_SEX_KOJO(母親, 段階)
  IF 段階 >= 3  ; 後期以降
    PRINT 大きく膨らんだお腹が、行為の度に揺れる。
    PRINT 「んっ…お腹の子に…見られてる気がする…」
    IF TALENT:母親:ボテ腹好き
      PRINT 「でも…こんな姿で抱かれるの…好き…」
    ENDIF
  ENDIF
```

### 托卵告白

```erb
@CUCKOLD_CONFESSION_KOJO(母親, 子ID)
  LOCAL 真の父親 = CFLAG:子ID:父親ID

  PRINT 「%CALLNAME:MASTER%…話があるの」
  PRINT %CALLNAME:母親%が真剣な表情で切り出す。
  PRINT
  PRINT 「この子…あなたの子じゃないの」
  PRINT 「%CALLNAME:真の父親%の…」
  PRINT
  PRINT 震える声で告げる%CALLNAME:母親%。
  PRINT その目には涙が溜まっていた。
```

---

## 実装フェーズ案

### 妊娠基盤

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| F1 | 変数定義（CFLAG/FLAG/TALENT） | feature-150 |
| F2 | 妊娠判定・確率計算 | feature-151 |
| F3 | 妊娠進行・段階管理 | feature-152 |
| F4 | つわり・体調イベント | feature-153 |
| F5 | 妊娠専用口上 | feature-154+ |

### 出産・子供

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| G1 | 出産イベント・処理 | feature-160 |
| G2 | 子キャラ生成 | feature-161 |
| G3 | 子の成長システム | feature-162 |
| G4 | 子育てイベント | feature-163 |
| G5 | 出産・子育て口上 | feature-164+ |

### 托卵・NTR連携

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| H1 | 父親判定・托卵システム | feature-170 |
| H2 | DNA検査・判明イベント | feature-171 |
| H3 | 修羅場イベント | feature-172 |
| H4 | 托卵専用口上 | feature-173+ |

### 攻略対象化

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| I1 | 成人判定・攻略対象化 | feature-180 |
| I2 | 血統別ルート分岐 | feature-181 |
| I3 | 世代間NTR | feature-182 |
| I4 | 娘攻略口上 | feature-183+ |

### 依存システム

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| J1 | 妊娠依存度管理 | feature-190 |
| J2 | 禁断症状・行動変化 | feature-191 |
| J3 | 依存専用口上 | feature-192+ |

---

## NTRシステムとの統合

### 統合ポイント

| 既存機能 | 統合方法 |
|----------|----------|
| 中出し処理 | `TRY_PREGNANCY` を呼び出し |
| 訪問者行為 | 父親候補として記録 |
| 浮気公認 | 妊娠許可にも影響 |
| NTR完了 | 托卵確定フラグ |

### 寝取らせシステムとの連携

```erb
; netorase-system.md との統合
@NETORASE_PREGNANCY_CHECK(住人, 訪問者)
  ; 寝取らせモードで妊娠した場合
  IF FLAG:寝取らせモード && CFLAG:住人:妊娠状態 == 1
    IF CFLAG:住人:胎児の父親 == 訪問者
      ; 寝取らせ妊娠成功 → 特殊演出
      CALL NETORASE_PREGNANCY_SUCCESS(住人, 訪問者)
    ENDIF
  ENDIF
```

---

## バージョン配置案

| バージョン | 内容 | 備考 |
|:----------:|------|------|
| v2.5 | 妊娠基盤（F1-F5） | NTR演出強化と同時期 |
| v2.6 | 出産・子供（G1-G5） | 大規模拡張の一部 |
| v2.7 | 托卵・NTR連携（H1-H4） | NTRシステム完成後 |
| v3.x | 攻略対象化（I1-I4） | 長期目標 |
| v3.x | 依存システム（J1-J3） | オプション機能 |

---

## 未解決事項

1. **妊娠期間のバランス**: 270日は長すぎる？加速オプションの調整
2. **子の上限数**: 何人まで生成可能にするか
3. **攻略対象化の倫理**: 実娘攻略の扱い（オプション化？）
4. **既存キャラとの整合性**: 東方キャラの妊娠設定
5. **メモリ/セーブデータ**: 子キャラ増加によるデータ肥大化

---

## 議論ログ

| 日付 | 内容 |
|------|------|
| 2025-12-17 | 初期設計。NTR連携、托卵要素を中心に構成 |
| 2025-12-17 | ステータス追加: 妊娠回数、浮気妊娠回数、妊娠依存度、経産婦 |
| 2025-12-17 | 攻略対象化設計: 主人公の娘、訪問者の血筋ルート |
