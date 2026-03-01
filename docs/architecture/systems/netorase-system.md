# 寝取らせシステム設計提案

## Status: DRAFT

**Target Version**: v6.x (S4)

## 概要

プレイヤーが「演出者」として、交際/結婚相手を訪問者に寝取らせることを能動的に制御できるシステム。

## 背景・動機

### 現行NTRシステムの課題

1. **能動性の欠如**: プレイヤーは基本「見守る」だけ
2. **ルートの単線性**: 寝取られる/防ぐの二択のみ

### 寝取らせの位置付け

- 「寝取られ」= 受動的（訪問者主導）
- 「寝取らせ」= 能動的（プレイヤー主導）

プレイヤーが「どこまで許すか」「どう攻めさせるか」を制御する演出者的プレイスタイル。

---

## 解放条件

### 前提: 交際または結婚状態

```
思慕 (TALENT:17) → 恋慕 (TALENT:3) → 恋人 (TALENT:16) → 人妻 (TALENT:15)
                                          ↑                    ↑
                                       交際状態             結婚状態
                                     (告白成功)          (結婚指輪使用)
```

| TALENT | ID | 意味 | 寝取らせ解放 |
|--------|:--:|------|:------------:|
| 思慕 | 17 | 好意的 | ❌ |
| 恋慕 | 3 | 愛情に近い | ❌ |
| **恋人** | 16 | 交際中 | ✅ |
| **人妻** | 15 | 結婚済 | ✅ |

### 判定関数

```erb
@IS_NETORASE_ENABLED(TARGET)
  IF TALENT:TARGET:恋人 || TALENT:TARGET:人妻
    RETURN 1
  ENDIF
  RETURN 0
```

---

## 変数設計

### 新規CFLAG（住人ごと）

```erb
; CSV/CFLAG.csv に追加
109,寝取らせ許可レベル    ; 0-5 (ムード段階に対応)
110,寝取らせ積極度        ; 0:受身 1:普通 2:誘惑 3:逆レイプ
111,寝取らせ対象指定      ; 訪問者ID (0=誰でも)
```

#### 許可レベル

| 値 | 意味 | 対応ムード |
|:--:|------|:----------:|
| 0 | 禁止（デフォルト） | - |
| 1 | 会話まで | NTR_MOOD_会話 |
| 2 | キスまで | NTR_MOOD_キス |
| 3 | 愛撫まで | NTR_MOOD_愛撫 |
| 4 | 奉仕まで | NTR_MOOD_奉仕 |
| 5 | 全許可 | NTR_MOOD_性交以上 |

#### 積極度

| 値 | 意味 | 効果 |
|:--:|------|------|
| 0 | 受身 | 訪問者主導のみ |
| 1 | 普通 | 通常反応 |
| 2 | 誘惑 | 住人から誘惑行動 |
| 3 | 逆レイプ | 住人が主導権 |

### 新規FLAG（グローバル）

```erb
; DIM.ERH に追加
#DIM SAVEDATA FLAG:寝取らせモード = 0        ; 0:OFF 1:ON
#DIM SAVEDATA FLAG:寝取らせターゲット = 0    ; 対象キャラID
```

### 新規CFLAG（心構え）

**Design Change**: 「訪問者への指示」→「ヒロインへのお願い」に変更。
MCが訪問者を操る（監督者視点）のではなく、パートナーに心構えを伝える（恋人/夫視点）方が自然なため。

詳細は [visitor-type-system.md](visitor-type-system.md) Layer 2 参照。

```erb
; CSV/CFLAG.csv に追加
112,寝取らせ心構え    ; 0:自然体 1:LOVE_MC優先 2:快楽解放 3:依存許可 4:EMO抑制

; Constants
#DIM CONST MINDSET_NATURAL = 0     ; 何も言わない
#DIM CONST MINDSET_LOVE_MC = 1     ; 「私だけを愛して」
#DIM CONST MINDSET_ENJOY = 2       ; 「楽しんで」
#DIM CONST MINDSET_ESCAPE = 3      ; 「逃げ場にして」
#DIM CONST MINDSET_NO_HEART = 4    ; 「心は渡さないで」
```

### 心構えの効果

| 心構え | ヒロイン反応 | ルート傾向 |
|--------|-------------|-----------|
| 自然体 | 状況依存 | Emergent |
| LOVE_MC優先 | 罪悪感↑、EMO抑制 | R1-R4 |
| 快楽解放 | FAM/TEM↑ | R2 |
| 依存許可 | DEP↑ | R3 |
| EMO抑制 | 距離維持 | R6ブロック |

---

## コマンド設計

### 新規コマンド

| COM番号 | 名前 | 対象 | 効果 |
|:-------:|------|------|------|
| 470 | 寝取らせ設定 | 住人 | 許可レベル + 心構え（統合） |
| 472 | 訪問者を招待 | - | 特定訪問者を呼ぶ |
| 473 | 寝取らせ観戦 | - | 覗きの上位版 |

**設計原則**:
- **訪問者タイプ** → NTR_OPTION（訪問者の設定）
- **許可レベル・心構え** → COM470（ヒロインへの指示/お願い）

### COMF470: 寝取らせ設定（統合）

許可レベルと心構えを一画面で設定。詳細は [visitor-type-system.md](visitor-type-system.md) 参照。

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

@COM_ABLE470
  SIF !TFLAG:100
    RETURN 0
  SIF !IS_NETORASE_ENABLED(TARGET)
    RETURN 0
  SIF GET_CHARA_NUM() != 2  ; 二人きりの時のみ
    RETURN 0
  RETURN 1
```

---

## 既存システムとの統合

### ムード上限判定への介入

```erb
; NTR_FRIENDSHIP.ERB @JUDGE_VISITOR_MOOD_MAX 修正

@JUDGE_VISITOR_MOOD_MAX(TARGET)
  ; === 寝取らせ許可チェック追加 ===
  IF FLAG:寝取らせモード == 1
    LOCAL 許可上限 = CFLAG:TARGET:寝取らせ許可レベル
    IF 許可上限 > 0
      RESULT = 許可上限
      RETURN
    ENDIF
  ENDIF

  ; === 既存の判定ロジック ===
  ; (現行コードそのまま)
```

### 心構えによるパラメータ蓄積修正

```erb
; NTR_UTIL.ERB or visitor-type-system 参照

@APPLY_MINDSET_MODIFIER(HEROINE, PARAM, BASE_GAIN)
  LOCAL MINDSET = CFLAG:HEROINE:寝取らせ心構え
  LOCAL MOD = 1.0

  SELECTCASE MINDSET
    CASE MINDSET_LOVE_MC  ; 「私だけを愛して」
      IF PARAM == PARAM_EMO
        MOD = 0.3  ; EMO上昇を抑制
      ENDIF
      ; 罪悪感も蓄積
      CFLAG:HEROINE:GUILT += BASE_GAIN * 0.5

    CASE MINDSET_ENJOY  ; 「楽しんで」
      IF PARAM == PARAM_FAM || PARAM == PARAM_TEM
        MOD = 1.5  ; FAM/TEMブースト
      ENDIF

    CASE MINDSET_ESCAPE  ; 「逃げ場にして」
      IF PARAM == PARAM_DEP
        MOD = 1.8  ; DEPブースト
      ENDIF

    CASE MINDSET_NO_HEART  ; 「心は渡さないで」
      IF PARAM == PARAM_EMO
        MOD = 0.1  ; EMOほぼブロック
      ENDIF

    CASE MINDSET_NATURAL  ; 何も言わない
      MOD = 1.0  ; 修正なし

  ENDSELECT

  RETURN BASE_GAIN * MOD
```

### ルート傾向への影響

| 心構え | 主効果 | ルート傾向 |
|--------|--------|-----------|
| LOVE_MC優先 | EMO↓↓, GUILT↑ | R1-R4維持（愛してるけど体は…） |
| 快楽解放 | FAM↑, TEM↑ | R2（馴染み/流れ） |
| 依存許可 | DEP↑↑ | R3（逃避/麻酔） |
| EMO抑制 | EMO→0 | R6ブロック |
| 自然体 | 修正なし | Emergent（訪問者タイプ依存） |

---

## 交際 vs 結婚の差別化

| 要素 | 交際（恋人） | 結婚（人妻） |
|------|:------------:|:------------:|
| 寝取らせ解放 | ✅ | ✅ |
| 許可レベル上限 | 4（奉仕まで） | 5（全許可） |
| 積極度上限 | 2（誘惑） | 3（逆レイプ） |
| 専用口上 | 恋人寝取らせ | 人妻寝取らせ |
| 屈服度ボーナス | ×1.0 | ×1.5 |

```erb
@GET_NETORASE_PERMIT_MAX(TARGET)
  IF TALENT:TARGET:人妻
    RETURN 5
  ELSEIF TALENT:TARGET:恋人
    RETURN 4
  ENDIF
  RETURN 0
```

---

## 寝取らせ売春システム

### コンセプト

既存の「公衆便所」システムを拡張し、プレイヤーが能動的に風俗店へ送り出す形。

```
現行: 公衆便所素質 → 自動で売春宿へ（受動的）

拡張: プレイヤーが指示 → 風俗店選択 → 送り出す（能動的）
```

### 風俗店タイプ

| 店タイプ | サービス内容 | 客層 | 収入 | 堕落度 |
|----------|-------------|------|:----:|:------:|
| **キャバクラ** | 接客・会話のみ | 一般～富裕 | 低 | 最低 |
| **ガールズバー** | 軽い接客 | 一般人 | 最低 | 最低 |
| **ピンサロ** | フェラのみ | 一般人 | 低 | 低 |
| **ヘルス** | 本番以外 | 一般人 | 中 | 中 |
| **ソープ** | 本番あり | 一般～富裕 | 高 | 高 |
| **デリヘル** | 出張、本番あり | 様々 | 中～高 | 高 |
| **高級クラブ** | 接待、本番あり | 富裕層 | 最高 | 中 |

### 解放条件

```erb
@IS_FUZOKU_ENABLED(TARGET)
  ; 寝取らせモードON かつ 交際/結婚
  IF FLAG:寝取らせモード == 0
    RETURN 0
  ENDIF
  IF !IS_NETORASE_ENABLED(TARGET)
    RETURN 0
  ENDIF
  ; 許可レベルに応じて店タイプ解放
  RETURN CFLAG:TARGET:寝取らせ許可レベル
```

| 許可レベル | 解放される店 |
|:----------:|-------------|
| 3（愛撫まで） | ピンサロ |
| 4（奉仕まで） | ピンサロ、ヘルス |
| 5（全許可） | 全店舗 |

### 新規コマンド

| COM番号 | 名前 | 効果 |
|:-------:|------|------|
| 474 | 風俗店に送り出す | 店タイプ選択 → 勤務開始 |
| 475 | 風俗店の報告を聞く | 勤務結果の確認 |

### COMF474: 風俗店に送り出す

```erb
@COMF474
  PRINTL どの店に送り出す？

  LOCAL 許可 = CFLAG:TARGET:寝取らせ許可レベル

  IF 許可 >= 3
    PRINTBUTTON [0] ピンサロ（フェラのみ）, 0
  ENDIF
  IF 許可 >= 4
    PRINTBUTTON [1] ヘルス（本番以外）, 1
  ENDIF
  IF 許可 >= 5
    PRINTBUTTON [2] ソープ（本番あり）, 2
    PRINTBUTTON [3] デリヘル（出張）, 3
    PRINTBUTTON [4] 高級クラブ, 4
  ENDIF

  CFLAG:TARGET:風俗勤務先 = RESULT
  CFLAG:TARGET:風俗勤務中 = 1

  CALL FUZOKU_SEND_KOJO(TARGET, RESULT)
  TIME += 10
  RETURN 1
```

### 風俗勤務処理

```erb
; 時間経過で自動処理
@FUZOKU_WORK(TARGET)
  LOCAL 店 = CFLAG:TARGET:風俗勤務先
  LOCAL 客数 = 0

  SELECTCASE 店
    CASE 店_ピンサロ
      客数 = RAND:3 + 1  ; 1～3人
      CALL FUZOKU_PINSARO(TARGET, 客数)
    CASE 店_ヘルス
      客数 = RAND:2 + 1  ; 1～2人
      CALL FUZOKU_HEALTH(TARGET, 客数)
    CASE 店_ソープ
      客数 = RAND:2 + 1
      CALL FUZOKU_SOAP(TARGET, 客数)
    CASE 店_デリヘル
      客数 = 1
      CALL FUZOKU_DELI(TARGET, 客数)
    CASE 店_高級クラブ
      客数 = RAND:3 + 1
      CALL FUZOKU_CLUB(TARGET, 客数)
  ENDSELECT

  ; 経験・収入の記録
  CFLAG:TARGET:風俗客数累計 += 客数
  CFLAG:TARGET:風俗収入累計 += GET_FUZOKU_INCOME(店, 客数)
```

### 客ステータスとの連動

**ntr-flavor-stats.md 参照**

```erb
; 風俗客のステータス生成
@GENERATE_FUZOKU_CLIENT(店タイプ)
  SELECTCASE 店タイプ
    CASE 店_ピンサロ
      ; 一般的な客
      TB:客:チンポランク = RAND:3  ; E～C
      TB:客:顔ランク = RAND:3 - 1   ; E～C
    CASE 店_ソープ
      ; やや裕福
      TB:客:チンポランク = RAND:4  ; E～B
      TB:客:顔ランク = RAND:3      ; D～B
    CASE 店_高級クラブ
      ; 富裕層（ただし年配多め）
      TB:客:チンポランク = RAND:3  ; E～C
      TB:客:顔ランク = RAND:2 - 1   ; E～D
      TB:客:金払い = ランク_A
  ENDSELECT
```

### 報告システム

```erb
@COMF475  ; 報告を聞く
  IF CFLAG:TARGET:風俗勤務中 == 0
    PRINT 勤務中ではありません。
    RETURN 0
  ENDIF

  ; 勤務結果表示
  PRINTL 〇〇での勤務報告:
  PRINTFORML 客数: %CFLAG:TARGET:今回客数%人
  PRINTFORML 収入: %CFLAG:TARGET:今回収入%円
  PRINTFORML 中出し: %CFLAG:TARGET:今回中出し%回

  ; 口上表示
  CALL FUZOKU_REPORT_KOJO(TARGET)

  CFLAG:TARGET:風俗勤務中 = 0
  TIME += 5
  RETURN 1
```

### 変数追加

```erb
; CFLAG追加
112,風俗勤務先         ; 0:なし 1:ガールズバー 2:キャバクラ 3:ピンサロ 4:ヘルス 5:ソープ 6:デリヘル 7:高級クラブ
113,風俗勤務中         ; 0/1
114,風俗客数累計       ; 累計客数
115,風俗収入累計       ; 累計収入
```

---

## 外部マップシステム

### コンセプト

紅魔館の外に「繁華街」エリアを追加。主人公が利用したり、寝取らせを観戦できる場所。

```
紅魔館 ──── 正門 ──── 人里 ──── 繁華街
                        │
                   訪問者の家
```

### 外部マップ一覧

| 場所 | 説明 | 主人公利用 | NTR観戦 |
|------|------|:----------:|:-------:|
| **訪問者の家** | お持ち帰り先（既存） | ❌ | ✅ |
| **売春宿** | 公衆便所先（既存） | ❌ | ✅ |
| **ラブホテル** | 短時間利用 | ✅ | ✅ |
| **繁華街** | 風俗店街エリア | ✅ | ✅ |
| **キャバクラ** | 繁華街内 | ✅ | ✅ |
| **ガールズバー** | 繁華街内 | ✅ | ✅ |
| **ピンサロ** | 繁華街内 | ❌ | ✅ |
| **ヘルス** | 繁華街内 | ❌ | ✅ |
| **ソープ** | 繁華街内 | ❌ | ✅ |

### 場所定数追加

```erb
; DIM.ERH 追加
#DIM CONST 場所_訪問者宅 = 900      ; 既存
#DIM CONST 場所_売春宿 = 901        ; 既存
#DIM CONST 場所_ラブホテル = 902
#DIM CONST 場所_繁華街 = 903
#DIM CONST 場所_キャバクラ = 904
#DIM CONST 場所_ガールズバー = 905
#DIM CONST 場所_ピンサロ = 906
#DIM CONST 場所_ヘルス = 907
#DIM CONST 場所_ソープ = 908
#DIM CONST 場所_高級クラブ = 909
```

### 主人公の利用

#### ラブホテル

```erb
@COMF480  ; ラブホテルに連れ込む
  ; 交際/結婚相手と二人で利用
  ; 紅魔館外での行為 → 特別な口上
  ; 主人公成長にも影響（性交経験）
```

#### キャバクラ/ガールズバー

```erb
@COMF481  ; 繁華街で遊ぶ
  ; 主人公が客として利用
  ; 女性キャラを同伴可能
  ; 「他の男に見せつける」プレイ
```

### NTR観戦システム

#### 観戦モード

```erb
@COMF482  ; 寝取らせ観戦
  ; 風俗店で働いている住人を観戦
  ; 覗きの上位版
  ; 店タイプに応じた観戦内容

  SELECTCASE CFLAG:TARGET:風俗勤務先
    CASE 店_キャバクラ
      ; 客と楽しそうに話す姿を見る
      CALL KANSEN_CABARET(TARGET)
    CASE 店_ソープ
      ; マジックミラー越しに行為を見る
      CALL KANSEN_SOAP(TARGET)
  ENDSELECT
```

#### 観戦の効果

| 状況 | プレイヤーへの効果 | 住人への効果 |
|------|-------------------|--------------|
| 接客を見る | 嫉妬/興奮 | 羞恥/背徳感 |
| 行為を見る | NTR快感 | 屈服度上昇 |
| 目が合う | 特別演出 | 比較認識上昇 |

### ラブホテル詳細

#### プレイヤー利用時

```erb
@COMF483  ; ラブホに誘う
  ; 条件: 交際/結婚相手
  PRINTL どこに行く？
  PRINTBUTTON [0] 普通の部屋, 0
  PRINTBUTTON [1] SM部屋, 1
  PRINTBUTTON [2] コスプレ部屋, 2

  ; 部屋タイプで口上・効果変化
  ; 紅魔館内より親密度上昇しやすい
```

#### NTR利用時

```erb
; 訪問者がラブホに連れ込む
@NTR_LOVEHOTEL(TARGET)
  ; お持ち帰りの一種
  ; 紅魔館外 → より背徳感が高い
  ; 主人公は尾行して観戦可能

  IF FLAG:主人公尾行中
    CALL LOVEHOTEL_PEEP(TARGET)  ; 覗き
  ENDIF
```

### マップ移動コマンド

```erb
@COMF490  ; 外出する
  PRINTL どこに行く？
  PRINTBUTTON [0] 人里, 0
  PRINTBUTTON [1] 繁華街, 1
  PRINTBUTTON [2] 訪問者の家を探す, 2

  SELECTCASE RESULT
    CASE 0
      ; 人里へ移動
    CASE 1
      ; 繁華街へ移動 → 店選択
    CASE 2
      ; 訪問者宅へ尾行（住人がお持ち帰り中のみ）
  ENDSELECT
```

### 変数追加

```erb
; FLAG追加
FLAG:主人公現在地         ; 0:紅魔館 1:人里 2:繁華街 3:ラブホ 4:訪問者宅付近
FLAG:主人公尾行中         ; 0/1
FLAG:尾行対象             ; キャラID

; CFLAG追加（住人）
116,現在外出先             ; 場所ID
117,外出同伴者             ; 同伴キャラID（主人公or訪問者）
```

---

## NTR後ルート：堕落展開

### コンセプト

NTR完了後（TALENT:NTR取得）に発生する追加展開。訪問者が住人を風俗店で働かせ、主人公が客として利用する。

```
NTR完了
    │
    ▼
訪問者が風俗店勤務を指示
    │
    ▼
主人公の選択肢:
├── 客として利用（知ってて行く）
├── 偶然遭遇（知らずに行く）
├── 身請け/救出を試みる
└── 放置/容認
```

### 発生条件

```erb
@IS_NTR_FUZOKU_ROUTE(TARGET)
  ; NTR完了状態
  IF !TALENT:TARGET:NTR
    RETURN 0
  ENDIF
  ; 元恋人/元人妻
  IF !CFLAG:TARGET:元恋人フラグ && !CFLAG:TARGET:元人妻フラグ
    RETURN 0
  ENDIF
  RETURN 1
```

### 変数追加

```erb
; CFLAG追加（住人）
118,寝取らせ受容度       ; (新規)
119,秘密欲求度           ; (新規)
120,風俗堕ち状態       ; 0:なし 1:訪問者に働かされ中 2:自発的
121,主人公利用済み     ; 主人公が客として利用した回数
```

### 展開パターン

#### 1. 知ってて利用

```erb
@COMF491  ; 風俗店で元カノ/元妻を指名
  ; 条件: NTR後ルート発動中 & 風俗堕ち状態
  IF !IS_NTR_FUZOKU_ROUTE(TARGET)
    RETURN 0
  ENDIF

  PRINTL %CALLNAME:TARGET%を指名しますか？
  ; 金額は通常の2倍（指名料）

  ; 行為開始 → 特別口上
  CALL NTR_FUZOKU_REUNION(TARGET)
```

#### 2. 偶然遭遇

```erb
@NTR_FUZOKU_ENCOUNTER()
  ; 繁華街をうろついていると…
  ; ランダムでNTR風俗堕ちキャラと遭遇

  PRINTL ───その店の前を通りかかった時。
  PRINTL 見覚えのある姿が目に入った。

  ; 選択肢
  PRINTBUTTON [0] 声をかける, 0
  PRINTBUTTON [1] 客として入る, 1
  PRINTBUTTON [2] 見なかったことにする, 2
```

#### 3. 身請け/救出

```erb
@COMF492  ; 身請けを申し出る
  ; 高額な金銭が必要
  ; 訪問者との交渉イベント
  ; 成功すると風俗堕ち解除、関係再構築の可能性

  LOCAL 身請け金額 = 1000000  ; 100万円
  IF MONEY < 身請け金額
    PRINT お金が足りない…
    RETURN 0
  ENDIF

  ; 訪問者との交渉
  CALL MIUKE_NEGOTIATION(TARGET)
```

### 口上パターン

#### 再会時（客として）

```erb
@NTR_FUZOKU_REUNION_KOJO(TARGET)
  IF CFLAG:TARGET:元人妻フラグ
    ; 元妻バージョン
    PRINT 「あ…あなた…？」
    PRINT 驚いた顔の%CALLNAME:TARGET%。
    PRINT 「まさか、お客として来るなんて…」
    PRINT
    PRINT 「…ふふ、でも嬉しい」
    PRINT 「私がどれだけ堕ちたか、見て欲しかったの」
  ELSEIF CFLAG:TARGET:元恋人フラグ
    ; 元カノバージョン
    PRINT 「え…%CALLNAME:MASTER%…？」
    PRINT 「どうして…ここに…」
    PRINT
    PRINT 「…見ないでよ、こんな姿」
    PRINT 「でも…お金払ってくれるなら、やるけど」
  ENDIF
```

#### 行為中

```erb
@NTR_FUZOKU_ACT_KOJO(TARGET, ACT_TYPE)
  ; 過去の思い出との対比
  PRINT （昔は二人きりで愛し合っていたのに…）
  PRINT （今は金を払わないと抱けない）

  IF CFLAG:TARGET:主人公利用済み >= 3
    ; 慣れてきた場合
    PRINT 「ふふ、%CALLNAME:MASTER%のも久しぶり」
    PRINT 「でも最近は毎日違う人のを咥えてるから…」
    PRINT 「%CALLNAME:MASTER%のサイズ、忘れちゃった」
  ENDIF
```

#### 身請け成功時

```erb
@MIUKE_SUCCESS_KOJO(TARGET)
  PRINT %CALLNAME:TARGET%を身請けした。

  IF CFLAG:TARGET:元人妻フラグ
    PRINT 「…本当にいいの？」
    PRINT 「私、もうあんなに汚れてるのに」
    PRINT
    PRINT 「…ありがとう」
    PRINT 「でも、体は忘れられないかも…」
    ; 浮気癖が残る可能性
  ENDIF
```

### ルート分岐

| 選択 | 展開 | エンディング候補 |
|------|------|------------------|
| **客として通う** | 関係が「元恋人→客と風俗嬢」に固定 | 堕落容認ED |
| **偶然遭遇→放置** | NTR完了、以降関わらない | 別離ED |
| **身請け成功** | 関係再構築の可能性 | 再生ED（条件付き） |
| **身請け失敗** | 訪問者に阻まれる | 絶望ED |

### 訪問者との関係

```erb
; 訪問者が風俗堕ちを指示する条件
@VISITOR_FUZOKU_ORDER(TARGET)
  ; NTR完了
  IF !TALENT:TARGET:NTR
    RETURN 0
  ENDIF
  ; 屈服度が非常に高い
  IF CFLAG:TARGET:屈服度 < 3000
    RETURN 0
  ENDIF
  ; 訪問者の嗜好（金銭欲など）
  IF FLAG:訪問者タイプ == 訪問者_金持ち || RAND:100 < 30
    ; 風俗堕ち指示
    CFLAG:TARGET:風俗堕ち状態 = 1
    RETURN 1
  ENDIF
  RETURN 0
```

### 実装フェーズ追加

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| E1 | NTR後ルート変数定義 | feature-130 |
| E2 | 風俗堕ち判定・遷移 | feature-131 |
| E3 | 偶然遭遇イベント | feature-132 |
| E4 | 客として利用コマンド | feature-133 |
| E5 | 身請けシステム | feature-134 |
| E6 | NTR後ルート専用口上 | feature-135+ |

---

## 主人公成長システム

### コンセプト

プレイヤー（主人公）のチンポ/顔ステータスを成長させ、NTRの「比較」要素を強化。

```
初期状態: プレイヤー C ランク vs 訪問者 A～S ランク
    ↓
成長後: プレイヤー A ランク vs 訪問者 A～S ランク
    ↓
比較演出の変化、寝取られにくさの変化
```

### 成長対象ステータス

| カテゴリ | ステータス | 初期値 | 上限 |
|----------|-----------|:------:|:----:|
| **チンポ** | サイズ | C | A |
| | 硬度 | C | A |
| | 持続力 | C | S |
| | 精液量 | C | A |
| | 回復力 | C | S |
| **顔** | 顔立ち | C | B |
| | フェロモン | C | A |
| | 威圧感 | C | A |

※ 訪問者は A～S 固定のため、完全に上回ることは困難

### 成長方法

#### 1. アイテム使用

| アイテム | 効果 | 入手方法 |
|----------|------|----------|
| **増大サプリ** | サイズ +1 | ショップ（高額） |
| **精力剤** | 回復力 +1 | ショップ |
| **持続スプレー** | 持続力 +1 | ショップ |
| **媚薬（自己使用）** | フェロモン +1 | パチュリー調合 |
| **禁断の秘薬** | 全ステ +1 | レアイベント |

#### 2. トレーニング

| トレーニング | 効果 | 条件 |
|--------------|------|------|
| **オナ禁** | 精液量 +1 | 7日間射精なし |
| **寸止め訓練** | 持続力 +1 | 特定コマンド使用 |
| **性交経験** | 各種 +1 | 累計回数に応じて |

#### 3. イベント

| イベント | 効果 |
|----------|------|
| **パチュリーの実験台** | ランダムで +1 または -1 |
| **永琳の診察（将来）** | 確実に +1（高コスト） |

### 実装案

```erb
; 成長処理
@GROW_MASTER_STAT(STAT_ID, AMOUNT)
  LOCAL 現在値 = CFLAG:MASTER:STAT_ID
  LOCAL 上限 = GET_MASTER_STAT_CAP(STAT_ID)

  IF 現在値 + AMOUNT > 上限
    CFLAG:MASTER:STAT_ID = 上限
  ELSE
    CFLAG:MASTER:STAT_ID += AMOUNT
  ENDIF

  ; 成長演出
  CALL GROWTH_EFFECT_KOJO(STAT_ID, AMOUNT)

; 上限取得
@GET_MASTER_STAT_CAP(STAT_ID)
  SELECTCASE STAT_ID
    CASE チンポ_サイズ, チンポ_硬度, チンポ_精液量
      RESULT = ランク_A  ; 最大A
    CASE チンポ_持続力, チンポ_回復力
      RESULT = ランク_S  ; 最大S（努力次第）
    CASE 顔_顔立ち
      RESULT = ランク_B  ; 顔は変えにくい
    CASE 顔_フェロモン, 顔_威圧感
      RESULT = ランク_A
  ENDSELECT
  RETURN
```

### NTRバランスへの影響

```erb
; 屈服度計算での比較
@CALC_NTR_COMPARISON(住人, 訪問者)
  LOCAL 主人ランク = CALC_CHINPO_RANK(MASTER) + CFLAG:MASTER:顔ランク
  LOCAL 訪問者ランク = CALC_CHINPO_RANK(訪問者) + CFLAG:訪問者:顔ランク

  LOCAL 差 = 訪問者ランク - 主人ランク

  ; 差が大きいほど屈服しやすい
  ; 差が0以下なら屈服しにくい
  IF 差 > 0
    屈服度補正 = 1.0 + 差 * 0.15
  ELSE
    屈服度補正 = 1.0 + 差 * 0.1  ; マイナス時は緩やか
  ENDIF

  RESULT = 屈服度補正
```

### 口上への反映

```erb
; 成長後の比較演出
IF CFLAG:MASTER:チンポ_サイズ >= CFLAG:訪問者:チンポ_サイズ
  PRINT （%CALLNAME:MASTER%も負けてない…）
ELSE
  PRINT （やっぱり%CALLNAME:MASTER%より大きい…）
ENDIF

; 主人公が成長すると
IF CALC_CHINPO_RANK(MASTER) >= ランク_A
  ; 寝取らせ時の優越感演出
  PRINT 「ふふ、私の旦那様も立派なのよ？」
  PRINT 「でも…他の男も知りたいの」
ENDIF
```

### 成長イベント案

| イベント | 内容 | 効果 |
|----------|------|------|
| **パチュリーの秘薬** | 実験台として協力 | ランダム成長 |
| **小悪魔の特訓** | サキュバス流技術 | 持続力・回復力 |
| **咲夜の激励** | メイド長の期待 | フェロモン |
| **レミリアの血** | 吸血鬼の力（リスク大） | 全ステ大幅上昇 |

---

## 実装フェーズ案

### 寝取らせ基盤

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| A1 | 変数定義（CFLAG/FLAG/定数） | feature-095 |
| A2 | COMF470: 許可設定コマンド | feature-096 |
| A3 | ムード上限判定の修正 | feature-096 |
| A4 | COMF471: 訪問者指示コマンド | feature-097 |
| A5 | 訪問者AIへの統合 | feature-097 |
| A6 | 積極度システム | feature-098 |
| A7 | 口上追加（寝取らせ専用） | feature-099+ |

### 風俗システム

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| B1 | 風俗店定数・変数定義 | feature-100 |
| B2 | COMF474: 風俗店に送り出す | feature-101 |
| B3 | 風俗勤務処理（時間経過） | feature-101 |
| B4 | COMF475: 報告を聞く | feature-102 |
| B5 | 客生成・ステータス連動 | feature-102 |
| B6 | 風俗専用口上 | feature-103+ |

### 外部マップ

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| C1 | 場所定数追加（900番台） | feature-110 |
| C2 | COMF490: 外出コマンド | feature-111 |
| C3 | ラブホテルシステム | feature-112 |
| C4 | キャバクラ/ガールズバー | feature-113 |
| C5 | NTR観戦システム | feature-114 |
| C6 | 尾行システム | feature-115 |
| C7 | 外部マップ専用口上 | feature-116+ |

### 主人公成長

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| D1 | 主人公ステータス変数定義 | feature-120 |
| D2 | 成長アイテム追加 | feature-121 |
| D3 | トレーニングシステム | feature-122 |
| D4 | 成長イベント（パチュリー等） | feature-123 |
| D5 | NTR計算式への統合 | feature-124 |
| D6 | 比較演出口上 | feature-125+ |

### NTR後ルート

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| E1 | NTR後ルート変数定義 | feature-130 |
| E2 | 風俗堕ち判定・遷移 | feature-131 |
| E3 | 偶然遭遇イベント | feature-132 |
| E4 | 客として利用コマンド | feature-133 |
| E5 | 身請けシステム | feature-134 |
| E6 | NTR後ルート専用口上 | feature-135+ |

---

## 既存システムとの統合

### 既存NTRシステム概要

現行システムは `Game/ERB/NTR/` に実装済み。主要コンポーネント:

| ファイル | 機能 |
|----------|------|
| `NTR.ERB` | メイン処理、訪問者出現、お持ち帰り |
| `NTR_FRIENDSHIP.ERB` | ムード進行、密室度、施錠 |
| `NTR_VISITOR.ERB` | 訪問者移動AI |
| `NTR_SEX.ERB` | 性行為処理 |
| `NTR_TAKEOUT.ERB` | お持ち帰り処理 |

### 活用する既存機能

#### 1. 密室度システム (`GET_ROOM_SECURITY`)

```erb
; 既存定義 (NTR_FRIENDSHIP.ERB:154)
; 戻り値: 0=堂々, 1=普通, 2=個室, 3=私室

密室度0: 正門, 広間, 食堂, 二階踊り場, 妖精メイド詰め所, 3Fテラス
密室度1: 二階廊下, あなた私室, 大浴場, 魔法の森内部
密室度2: 図書室, 厨房, 納屋, 庭, 湖, トイレ
密室度3: 各キャラ私室, 守衛小屋, 地下室
```

**統合方法**: 寝取らせ許可レベルと密室度を組み合わせてムード上限を決定

#### 2. ムード上限判定 (`JUDGE_VISITOR_MOOD_MAX`)

```erb
; 既存ロジック (NTR_FRIENDSHIP.ERB:184)
; 密室度 + 屈服度 + 浮気公認 → ムード上限値

; 寝取らせ許可による介入点
@JUDGE_VISITOR_MOOD_MAX(奴隷, 密室度, 同席人数)
  ; ... 既存処理 ...

  ; === 寝取らせ許可チェック追加 ===
  IF FLAG:寝取らせモード == 1 && IS_NETORASE_ENABLED(奴隷)
    LOCAL 許可上限 = CFLAG:奴隷:寝取らせ許可レベル
    IF 許可上限 > 0
      ムード上限値 = MIN(ムード上限値, 許可上限)
    ENDIF
  ENDIF
```

#### 3. 施錠システム

```erb
; 既存関数 (NTR_FRIENDSHIP.ERB:70付近)
tryLock(キャラ, 場所)      ; 通常施錠
tryLockBolt(キャラ, 場所)  ; 閂施錠（開錠困難）
isLocked(場所)             ; 施錠状態確認
IS_CAN_LOCK(場所)          ; 施錠可能判定
```

**統合方法**: 寝取らせ時は浮気公認状態で施錠行動を変化

#### 4. 訪問者お気に入り (`FLAG:訪問者のお気に入り`)

```erb
; 既存変数 (NTR_VISITOR.ERB)
FLAG:訪問者のお気に入り    ; ターゲットキャラID (999=未設定)
FLAG:訪問者の嫌いな相手    ; 回避キャラID

; 統合方法: 寝取らせ指示でお気に入りを設定
@COMF471  ; 訪問者に指示
  IF RESULT == 指示_特定キャラ狙え
    FLAG:訪問者のお気に入り = FLAG:寝取らせターゲット
  ENDIF
```

#### 5. 浮気公認 (`TALENT:浮気公認`)

```erb
; 既存段階
浮気_なし = 0
浮気_キス公認 = 1
浮気_愛撫公認 = 2
浮気_奉仕公認 = 3
浮気_性交公認 = 4

; 統合方法: 寝取らせ許可レベルと同期
; TALENT:浮気公認 >= 寝取らせ許可レベル の場合のみ許可有効
```

#### 6. 覗き発覚 (`CFLAG:覗き発覚回数`)

```erb
; 既存処理 (NTR_FRIENDSHIP.ERB:52-62)
; 覗きバレ後は行為エスカレート
IF CFLAG:奴隷:覗き発覚回数 && FLAG:訪問者のムード < 2
    FLAG:訪問者のムード += 1
ENDIF

; 統合方法: 寝取らせモードでは覗きを「観戦」として扱う
; 発覚しても関係悪化しない（むしろ興奮ボーナス）
```

### 新規追加が必要な変数

```erb
; === CFLAG追加 (住人ごと) ===
CFLAG:寝取らせ許可レベル     ; 0-5 (既存の浮気公認と連動)
CFLAG:寝取らせ積極度         ; 0-3 (受身/普通/誘惑/逆レイプ)
CFLAG:寝取らせ対象指定       ; 訪問者ID (0=誰でも)

; === FLAG追加 (グローバル) ===
FLAG:寝取らせモード          ; 0:OFF 1:ON
FLAG:訪問者への指示          ; 指示コード
FLAG:寝取らせターゲット      ; 対象キャラID
```

### 統合実装の優先順位

| # | 統合内容 | 工数 | 既存活用度 |
|:-:|----------|:----:|:----------:|
| 1 | `JUDGE_VISITOR_MOOD_MAX` への許可レベル介入 | 低 | 高 |
| 2 | `FLAG:訪問者のお気に入り` を指示で設定 | 低 | 高 |
| 3 | `TALENT:浮気公認` との同期処理 | 低 | 高 |
| 4 | 施錠システムの寝取らせ対応 | 低 | 高 |
| 5 | 覗き→観戦モード変換 | 中 | 中 |

### 紅魔館マップ NTR特性

既存 `GET_ROOM_SECURITY` に加え、NTR向け特性を追加:

| 場所 | 密室度 | NTR特性 | 備考 |
|------|:------:|---------|------|
| 守衛小屋 | 3 | 隔離・長時間 | 訪問者最初の接触点 |
| 図書室 | 2 | 死角多い | 覗きやすい |
| 地下室 | 3 | 秘匿性最高 | 背徳感ボーナス |
| 大広間 | 0 | 目撃リスク高 | スリル感 |
| 納屋 | 2 | 隠れ場所 | 特殊シチュ |

```erb
; NTR特性取得関数（新規）
@GET_NTR_LOCATION_TRAIT(場所)
  SELECTCASE 場所
    CASE 場所_守衛小屋
      RESULT:隔離度 = 3
      RESULT:背徳感 = 1
    CASE 場所_図書室
      RESULT:隔離度 = 1
      RESULT:覗き適性 = 3
    CASE 場所_地下室
      RESULT:隔離度 = 3
      RESULT:背徳感 = 3
    CASE 場所_大広間
      RESULT:隔離度 = 0
      RESULT:スリル = 3
  ENDSELECT
```

---

## Phase System Integration

本システムは [phase-system.md](phase-system.md) と連携して動作する。

### 関係性

| システム | 役割 | 主導権 |
|---------|------|:------:|
| Phase System | NTR進行の自動管理 | 訪問者 |
| Netorase System | NTR進行のプレイヤー制御 | プレイヤー |

```
通常モード:
  Phase進行 → 訪問者行動 → パラメータ変化 → Phase遷移

寝取らせモード:
  許可レベル設定 → Phase進行に介入 → 訪問者行動を制御
```

### 許可レベルと Phase の関係

| 許可レベル | Phase上限 | 対応Route | 備考 |
|:----------:|:---------:|-----------|------|
| 0 (禁止) | Phase 0 | - | NTR進行をブロック |
| 1 (会話) | Phase 1-2 | - | 外圧/馴染みまで |
| 2 (キス) | Phase 2-3 | R2候補 | 身体接触開始 |
| 3 (愛撫) | Phase 3 | R2, R3候補 | 境界侵食 |
| 4 (奉仕) | Phase 3-4 | R1-R4候補 | Point of No Return手前 |
| 5 (全許可) | Phase 4+ | R1-R6全て | 制限なし |

### Route分岐への影響

寝取らせモードでは、プレイヤーの許可がRoute分岐条件に影響:

| Route | 通常条件 | 寝取らせ時の追加条件 |
|:-----:|---------|---------------------|
| R1 強制 | LEV high | 許可レベル低い場合も発生 |
| R2 馴染み | FAM high + TEM mid | 許可レベル段階的上昇で促進 |
| R3 逃避 | DEP high + SAT_MC low | 許可による心理的逃避 |
| R4 交換 | DEP mid + 条件 | 許可=取引条件として機能 |
| R5 反逆 | TRUST_MC low | 寝取らせ=MC承認のため発生しにくい |
| R6 本気 | EMO high | 許可があっても EMO 条件必須 |

**設計意図**: 寝取らせモードでは R5 (反逆) が発生しにくく、R2/R4 が発生しやすい。

### パラメータ連携

```erb
; 寝取らせモード時のPhaseパラメータ修正
@NETORASE_PHASE_MODIFIER(TARGET)
  IF FLAG:寝取らせモード == 0
    RETURN
  ENDIF

  LOCAL 許可 = CFLAG:TARGET:寝取らせ許可レベル

  ; 許可による心理的変化
  IF 許可 >= 3
    ; 愛撫以上許可 → 罪悪感低下、抵抗低下
    CFLAG:TARGET:RES -= 許可 * 5
  ENDIF

  ; R5抑制（MCが許可しているため反逆理由がない）
  IF 許可 >= 1
    ; TRUST_MC低下を抑制
    TRUST_MC_DECAY_RATE = 0.5
  ENDIF
```

### Post-Reveal Route との関係

Phase System の Post-Reveal (A-E) と Netorase の風俗堕ちルートの関係:

| Phase Post-Reveal | Netorase対応 | 条件 |
|-------------------|-------------|------|
| A: Repair/Reclaim | 身請け成功 | TRUST回復行動 |
| B: Open Coexistence | 継続的寝取らせ | 許可維持 |
| C: Transfer | 訪問者への完全移行 | EMO high, 許可撤回不可 |
| D: Role Fixation | 風俗堕ち | 屈服度 ≥ 3000 |
| E: Secret Reconnection | 客として利用 + 秘密の絆 | BOND_MC_SECRET成長 |

### 依存関係

```
v3.x Phase System (S2)
         ↓
    [必須前提]
         ↓
v6.x Netorase System (S4)
```

---

## Kojo Branching Design

寝取らせ専用口上の分岐設計。

**Detailed specification**: See [kojo-phases.md](../reference/kojo-phases.md) Phase 8n section.

**Summary**:
| Category | Patterns | Lines | Version |
|----------|:--------:|:-----:|:-------:|
| 受容度段階 (8n-1) | 240 | ~1200 | v6.x |
| 煽り・比較 (8n-2) | 48 | ~400 | v6.x |
| 秘密欲求 (8n-3) | 36 | ~300 | v6.x |
| 寝取り返し (8n-4) | 48 | ~500 | v7.x |
| **Total** | 372 | ~2400 | - |

**New Parameters** (defined in this document):
- 寝取らせ受容度 (CFLAG 0-100): 嫌々→乗り気の心理変化
- 秘密欲求度 (CFLAG 0-100): 許可なのに隠したい欲求

---

## 未解決事項

1. **結婚システムの現状確認**: 結婚指輪使用フローの実装状況
2. **NTR後の関係性変化**: 寝取られ後にTALENT:恋人/人妻が解除されるか
3. **浮気公認との統合**: 既存のTALENT:浮気公認との関係整理 → **上記で方針決定**
4. **口上の優先度**: システム実装と口上作成のどちらを先行するか

---

## 議論ログ

| 日付 | 内容 |
|------|------|
| 2025-12-17 | 初期設計議論。能動性・ルート分岐の課題整理 |
| 2025-12-17 | 解放条件を「交際/結婚」に決定 |
| 2025-12-17 | 変数設計・コマンド設計の初版作成 |
| 2025-12-17 | 寝取らせ売春システム追加（風俗店タイプ5種） |
| 2025-12-17 | 主人公成長システム追加（アイテム/トレーニング/イベント） |
| 2025-12-17 | 風俗店にキャバクラ/ガールズバー追加（計7種） |
| 2025-12-17 | 外部マップシステム追加（繁華街/ラブホ/観戦/尾行） |
| 2025-12-17 | NTR後ルート追加（風俗堕ち/客として利用/身請け） |
| 2025-12-17 | 既存NTRシステム調査完了、統合セクション追加 |
| 2025-12-17 | 密室度/ムード上限/施錠/浮気公認との統合方法を文書化 |
| 2025-12-17 | 紅魔館マップNTR特性（隔離度/背徳感/覗き適性）を定義 |
