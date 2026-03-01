# 異変戦争システム設計提案

## Status: DRAFT

**Target Version**: v3.5-v4.5 (S5-S6)

## 概要

幻想郷の「異変」を発端とした勢力間戦争（弾幕ごっこ）システム。
スペルカードルールに則り、勝者が敗者に「要求」できる仕組みを通じて、
**寝取り（能動的NTR）** と **寝取られ（受動的NTR）** の両立を実現する。

## 背景・動機

### 参考ゲーム調査

| ゲーム | 主要システム | NTR要素 |
|--------|-------------|---------|
| [戦国ランス](https://rance.ken-shin.net/contents/captive.html) | 地域制圧、捕虜→勧誘、好感度 | 女武将を寝取り、島津兄弟NTRリスク |
| [魔剣士リーネ2](https://www.makuracover.com/leane2/top.htm) | 女囚/ハーレムシステム、忠誠度 | 高忠誠キャラが優先NTR対象 |
| [eratohoK](https://wiki.eragames.rip/index.php/EraTohoK) | 勢力外交、捕虜奴隷化、特殊勢力 | サキュバス/カルト勢力による永続NTR |

### 現行システムの課題

1. **NTRの一方向性**: 現状は「寝取られ」のみ（受動的）
2. **プレイスタイル制限**: 攻めのNTR（寝取り）ができない
3. **他勢力不在**: 紅魔館の外との関係性がない

### 異変戦争の位置付け

```
現行: 紅魔館内 → 訪問者NTR（受動）

拡張: 勢力間戦争 → 勝利=寝取り（能動）、敗北=寝取られ（受動）
```

東方Projectの「スペルカードルール」:
- 勝者は敗者に「要求」できる（原作設定）
- 殺し合いではなく「決闘」
- NTR展開の自然な理由付け

---

## システム構成

```
┌─────────────────────────────────────────────────────────┐
│  1. 勢力システム                                         │
│     - 紅魔館（主人公勢力）+ 他勢力                       │
│     - 勢力間好感度 + リーダー個人好感度                  │
├─────────────────────────────────────────────────────────┤
│  2. 異変トリガー                                         │
│     - ランダム発生 or 意図的発動                         │
│     - 異変の種類で対戦相手・条件が変化                   │
├─────────────────────────────────────────────────────────┤
│  3. 弾幕ごっこ（戦闘）                                   │
│     - スペルカードルール準拠                             │
│     - 武将（キャラ）の能力で勝敗                         │
│     - 相性システム（種族/属性）                          │
├─────────────────────────────────────────────────────────┤
│  4. 戦後処理                                             │
│     勝利時 → 敗者への「要求」（キャラ獲得、調教権）      │
│     敗北時 → 自キャラが「奪われる」（NTR展開）           │
├─────────────────────────────────────────────────────────┤
│  5. 外交システム                                         │
│     - 同盟/停戦/敵対                                     │
│     - キャラを「貸す」（NTR外交）                        │
│     - 強国からの「要求」                                 │
├─────────────────────────────────────────────────────────┤
│  6. 忠誠度システム                                       │
│     - 高忠誠 = NTR対象になりやすい（魔剣士リーネ式）     │
│     - 低忠誠 = 反乱/離脱リスク                           │
│     - 中間が最も安定                                     │
└─────────────────────────────────────────────────────────┘
```

---

## 1. 勢力システム

### 勢力一覧（竿役付き）

| ID | 勢力名 | リーダー | 竿役 | 主要キャラ | 特性 |
|:--:|--------|----------|------|-----------|------|
| 0 | **紅魔館** | レミリア | **主人公** | 咲夜,美鈴,パチュリー,小悪魔,フラン | プレイヤー拠点 |
| 1 | 博麗神社 | 霊夢 | **霖之助**（香霖堂店主） | 魔理沙（訪問） | 異変解決者 |
| 2 | 永遠亭 | 輝夜 | **月の使者**（男性） | 永琳,鈴仙,てゐ,妹紅 | 薬/不老不死 |
| 3 | 白玉楼 | 幽々子 | **冥界の門番**（男性霊） | 妖夢 | 冥界/剣術 |
| 4 | 地霊殿 | さとり | **地獄鴉の雄**（獣人） | こいし,燐,空 | 読心/核融合 |
| 5 | 守矢神社 | 神奈子 | **人里の信者頭**（信仰狂） | 諏訪子,早苗 | 信仰/風祝 |
| 6 | 命蓮寺 | 白蓮 | **雲山**（入道、人型化可） | 一輪,ムラサ,ナズーリン,星,ぬえ | 仏教/妖怪寺 |
| 7 | 旧地獄 | 勇儀 | **鬼の若頭**（力自慢） | パルスィ,キスメ,ヤマメ | 力/嫉妬 |
| **8** | **訪問者一族** | **訪問者** | **訪問者本人** | 訪問者の親族女性 | 既存NTR統合 |

### 竿役詳細

各勢力の「竿役」は、捕虜を堕落させる役割を担う。

| 勢力 | 竿役名 | 調教スタイル | 得意プレイ | 堕落タイプ |
|------|--------|-------------|-----------|-----------|
| 紅魔館 | 主人公 | プレイヤー依存 | 全般 | - |
| 博麗神社 | 霖之助 | 知的・誘導型 | 言葉責め | 精神堕落 |
| 永遠亭 | 月の使者 | 薬物・技術型 | 媚薬、拘束 | 依存堕落 |
| 白玉楼 | 冥界の門番 | 幽玄・持久型 | 焦らし、長時間 | 快楽堕落 |
| 地霊殿 | 地獄鴉の雄 | 野性・本能型 | 激しい交尾 | 肉体堕落 |
| 守矢神社 | 人里の信者頭 | 洗脳・信仰型 | 儀式、奉仕強制 | 信仰堕落 |
| 命蓮寺 | 雲山 | 力・変幻型 | 触手的、拘束 | 屈服堕落 |
| 旧地獄 | 鬼の若頭 | 暴力・支配型 | 力による屈服 | 恐怖堕落 |
| 訪問者一族 | 訪問者 | 既存NTRシステム | 既存コマンド | 通常堕落 |

### 訪問者一族（勢力ID:8）

既存の「訪問者」システムを勢力として統合。

```
勢力ID: 8
勢力名: 訪問者一族
リーダー/竿役: 訪問者（既存キャラ）
拠点: 人里 / 訪問者の家
所属キャラ: 訪問者の妻、娘、姉妹（新規追加候補）
```

**統合メリット**:
- 既存NTRシステムとの完全互換
- 訪問者に敗北 = 現行NTRの自然な延長
- 訪問者に勝利 = 訪問者の女性親族を寝取る（逆NTR）

### 特殊勢力（NTR専用）

eratohoKを参考に、NTR特化の特殊勢力を設定。

| ID | 勢力名 | 特殊効果 | NTRメカニズム |
|:--:|--------|----------|---------------|
| 90 | **サキュバス集団** | 10日ごとにステ吸収 | 精力吸い取り、堕落 |
| 91 | **邪仙道** | 洗脳/精神支配 | 人格改変、従属化 |
| 92 | **月の都** | 技術力で記憶操作 | NTR記憶植え付け |
| 93 | **旧支配者** | 狂気/SAN値削り | 触手、異形NTR |

**特殊勢力に敗北した場合**:
- キャラが「永続NTR状態」に
- 奪還戦で取り返すか、特殊条件で解放

### 変数設計

```erb
; === 勢力定数 (DIM.ERH) ===
#DIM CONST 勢力_紅魔館 = 0
#DIM CONST 勢力_博麗神社 = 1
#DIM CONST 勢力_永遠亭 = 2
#DIM CONST 勢力_白玉楼 = 3
#DIM CONST 勢力_地霊殿 = 4
#DIM CONST 勢力_守矢神社 = 5
#DIM CONST 勢力_命蓮寺 = 6
#DIM CONST 勢力_旧地獄 = 7

#DIM CONST 特殊勢力_サキュバス = 90
#DIM CONST 特殊勢力_邪仙道 = 91
#DIM CONST 特殊勢力_月の都 = 92
#DIM CONST 特殊勢力_旧支配者 = 93

; === 勢力ステータス (FLAG配列) ===
#DIM SAVEDATA 勢力好感度, 100     ; 勢力ID → 好感度 (-1000 ~ 1000)
#DIM SAVEDATA 勢力状態, 100       ; 0:中立 1:同盟 2:停戦 3:敵対 4:従属 5:支配
#DIM SAVEDATA 勢力戦力, 100       ; 総合戦力値
#DIM SAVEDATA 同盟残りターン, 100 ; 同盟/停戦の残りターン数

; === リーダー好感度 (CFLAG) ===
; 既存のCFLAG:好感度を使用（勢力好感度とは別管理）
```

### 勢力好感度 vs リーダー好感度

eratohoK方式を採用: 勢力全体の態度とリーダー個人の感情は別。

```erb
@GET_FACTION_RELATION(勢力ID)
  ; 勢力好感度: 外交に影響
  ; リーダー好感度: 個人イベントに影響

  LOCAL 勢力 = 勢力好感度:勢力ID
  LOCAL リーダー = CFLAG:(GET_FACTION_LEADER(勢力ID)):好感度

  ; 両方高い → 友好的
  ; 勢力高+リーダー低 → 表面上は友好、個人的には冷淡
  ; 勢力低+リーダー高 → 公的には敵対、私的には好意

@GET_FACTION_LEADER(勢力ID)
  SELECTCASE 勢力ID
    CASE 勢力_博麗神社
      RESULT = CHARA_霊夢
    CASE 勢力_永遠亭
      RESULT = CHARA_輝夜
    ; ...
  ENDSELECT
```

---

## 2. 異変トリガー

### 異変タイプ

| ID | 異変名 | 発動条件 | 対戦相手 | 期間 |
|:--:|--------|----------|----------|:----:|
| 1 | 紅霧異変 | レミリア発動 | 博麗神社 | 7日 |
| 2 | 永夜異変 | 永遠亭発動 | 全勢力 | 1夜 |
| 3 | 花映異変 | 季節イベント | ランダム | 14日 |
| 4 | 地霊異変 | 地霊殿発動 | 地上勢力 | 10日 |
| 5 | 信仰戦争 | 守矢神社発動 | 博麗神社 | 30日 |
| 6 | 邪仙襲来 | 特殊勢力イベント | 全勢力 | ∞ |

### 発動方式

```erb
; === 異変発動コマンド ===
@COMF_INCIDENT_START
  PRINTL どの異変を起こす？

  ; 条件を満たす異変のみ表示
  FOR 異変ID = 1 TO 異変_MAX
    IF CAN_START_INCIDENT(異変ID)
      PRINTBUTTON [%異変ID%] %GET_INCIDENT_NAME(異変ID)%, 異変ID
    ENDIF
  NEXT

  IF RESULT > 0
    CALL START_INCIDENT(RESULT)
  ENDIF

; === 異変発動条件 ===
@CAN_START_INCIDENT(異変ID)
  SELECTCASE 異変ID
    CASE 異変_紅霧
      ; レミリアがリーダー、戦力十分、博麗との関係が中立以下
      RESULT = (勢力状態:勢力_博麗神社 != 状態_同盟)
    CASE 異変_永夜
      ; 永遠亭との敵対状態
      RESULT = (勢力状態:勢力_永遠亭 == 状態_敵対)
    ; ...
  ENDSELECT

; === ランダム異変発生 ===
@CHECK_RANDOM_INCIDENT
  ; 30日ごとに5%の確率で異変発生
  IF DAY % 30 == 0 && RAND:100 < 5
    LOCAL 異変 = GET_RANDOM_INCIDENT()
    IF 異変 > 0
      CALL START_INCIDENT(異変)
    ENDIF
  ENDIF
```

### 異変進行

```erb
; === 異変状態管理 ===
#DIM SAVEDATA FLAG:現在異変 = 0          ; 発生中の異変ID
#DIM SAVEDATA FLAG:異変残り日数 = 0      ; 残り日数
#DIM SAVEDATA FLAG:異変敵勢力 = 0        ; 敵対勢力ID
#DIM SAVEDATA FLAG:異変戦闘回数 = 0      ; 戦闘発生回数

@INCIDENT_DAILY_UPDATE
  IF FLAG:現在異変 == 0
    RETURN
  ENDIF

  FLAG:異変残り日数 -= 1

  ; 戦闘イベント発生判定
  IF RAND:100 < GET_BATTLE_CHANCE(FLAG:現在異変)
    CALL TRIGGER_BATTLE()
  ENDIF

  ; 異変終了判定
  IF FLAG:異変残り日数 <= 0
    CALL END_INCIDENT()
  ENDIF
```

---

## 3. 弾幕ごっこ（戦闘）

### 戦闘システム概要

戦国ランスを参考にしつつ、東方のスペルカードルールを反映。

```
┌─────────────────────────────────────────────┐
│  戦闘フェーズ                                │
│                                             │
│  1. 出撃キャラ選択（最大5人）                │
│  2. スペルカード選択                         │
│  3. 自動戦闘（ステータス＋乱数）             │
│  4. 勝敗判定                                 │
│  5. 戦後処理（要求/被要求）                  │
└─────────────────────────────────────────────┘
```

### キャラステータス（戦闘用）

```erb
; === 戦闘ステータス (CFLAG追加) ===
; 既存の能力値に加えて戦闘用ステータスを追加

CFLAG:弾幕力        ; 攻撃力相当 (0-1000)
CFLAG:回避力        ; 防御力相当 (0-1000)
CFLAG:スペル数      ; 使用可能スペルカード数
CFLAG:霊力          ; HP相当、0で敗北

; === スペルカード (CSTR) ===
CSTR:スペル1        ; スペルカード名
CSTR:スペル2
CSTR:スペル3

; === 種族相性 ===
#DIM CONST 種族_人間 = 0
#DIM CONST 種族_妖怪 = 1
#DIM CONST 種族_吸血鬼 = 2
#DIM CONST 種族_妖精 = 3
#DIM CONST 種族_幽霊 = 4
#DIM CONST 種族_神 = 5

; 相性テーブル (攻撃側, 防御側) → 倍率
@GET_SPECIES_MODIFIER(攻撃種族, 防御種族)
  ; 例: 人間 vs 妖怪 = 0.8 (不利)
  ;     神 vs 妖怪 = 1.2 (有利)
```

### 戦闘処理

```erb
@BATTLE_MAIN(自軍リスト, 敵軍リスト)
  LOCAL 自軍HP = 0
  LOCAL 敵軍HP = 0

  ; 初期HP計算
  FOR I = 0 TO 4
    IF 自軍リスト:I > 0
      自軍HP += CFLAG:(自軍リスト:I):霊力
    ENDIF
    IF 敵軍リスト:I > 0
      敵軍HP += CFLAG:(敵軍リスト:I):霊力
    ENDIF
  NEXT

  ; ターン制戦闘
  LOCAL ターン = 1
  WHILE 自軍HP > 0 && 敵軍HP > 0 && ターン <= 10
    CALL BATTLE_TURN(自軍リスト, 敵軍リスト, 自軍HP, 敵軍HP)
    ターン += 1
  WEND

  ; 勝敗判定
  IF 自軍HP > 敵軍HP
    RESULT = 1  ; 勝利
  ELSEIF 自軍HP < 敵軍HP
    RESULT = -1 ; 敗北
  ELSE
    RESULT = 0  ; 引き分け
  ENDIF

@BATTLE_TURN(自軍, 敵軍, REF 自軍HP, REF 敵軍HP)
  ; 自軍攻撃
  FOR I = 0 TO 4
    IF 自軍:I > 0 && CFLAG:(自軍:I):霊力 > 0
      LOCAL ダメージ = CALC_DAMAGE(自軍:I, 敵軍)
      敵軍HP -= ダメージ
      CALL BATTLE_LOG(自軍:I, ダメージ, "攻撃")
    ENDIF
  NEXT

  ; 敵軍攻撃
  FOR I = 0 TO 4
    IF 敵軍:I > 0 && CFLAG:(敵軍:I):霊力 > 0
      LOCAL ダメージ = CALC_DAMAGE(敵軍:I, 自軍)
      自軍HP -= ダメージ
      CALL BATTLE_LOG(敵軍:I, ダメージ, "攻撃")
    ENDIF
  NEXT

@CALC_DAMAGE(攻撃者, 防御側リスト)
  LOCAL 基本ダメージ = CFLAG:攻撃者:弾幕力
  LOCAL 相性補正 = 1.0

  ; 種族相性計算
  LOCAL 攻撃種族 = CFLAG:攻撃者:種族
  FOR I = 0 TO 4
    IF 防御側リスト:I > 0
      LOCAL 防御種族 = CFLAG:(防御側リスト:I):種族
      相性補正 += GET_SPECIES_MODIFIER(攻撃種族, 防御種族) - 1.0
    ENDIF
  NEXT
  相性補正 /= 5

  ; 乱数要素
  LOCAL 乱数 = 0.8 + RAND:40 / 100.0

  RESULT = 基本ダメージ * 相性補正 * 乱数
```

### スペルカード発動

```erb
@USE_SPELLCARD(キャラ, スペルID)
  ; スペルカードは1戦闘で1回のみ使用可能
  ; 強力な効果だが、使用後はそのキャラの攻撃力低下

  LOCAL スペル名 = CSTR:キャラ:(スペル1 + スペルID)
  LOCAL 効果 = GET_SPELLCARD_EFFECT(キャラ, スペルID)

  PRINTFORML %CALLNAME:キャラ%「%スペル名%！」

  SELECTCASE 効果:タイプ
    CASE スペル_攻撃
      ; 敵全体に大ダメージ
      RESULT = 効果:威力 * 3
    CASE スペル_回復
      ; 味方HP回復
      CALL HEAL_PARTY(効果:回復量)
    CASE スペル_強化
      ; 味方攻撃力上昇
      CALL BUFF_PARTY(効果:上昇量)
    CASE スペル_妨害
      ; 敵攻撃力低下
      CALL DEBUFF_ENEMY(効果:低下量)
  ENDSELECT

  ; スペル使用済みフラグ
  CFLAG:キャラ:スペル使用済み:スペルID = 1
```

---

## 4. 戦後処理

### 勝利時: 要求システム

戦国ランス/魔剣士リーネの「捕虜」概念と、eratohoKの「奴隷化」を組み合わせ。

```erb
@POST_BATTLE_WIN(敵勢力ID)
  PRINTL ───勝利！───
  PRINTL スペルカードルールに則り、敗者に要求を出せる。

  PRINTBUTTON [0] 賠償金を要求, 0
  PRINTBUTTON [1] キャラクターを要求, 1
  PRINTBUTTON [2] 停戦協定, 2
  PRINTBUTTON [3] 従属を要求, 3
  PRINTBUTTON [4] 何も要求しない, 4

  SELECTCASE RESULT
    CASE 0
      CALL DEMAND_MONEY(敵勢力ID)
    CASE 1
      CALL DEMAND_CHARACTER(敵勢力ID)
    CASE 2
      CALL ESTABLISH_CEASEFIRE(敵勢力ID)
    CASE 3
      CALL DEMAND_SUBMISSION(敵勢力ID)
  ENDSELECT
```

### キャラクター要求（寝取り）

```erb
@DEMAND_CHARACTER(敵勢力ID)
  PRINTL 誰を要求する？

  ; 敵勢力のキャラ一覧表示
  LOCAL 候補リスト
  CALL GET_FACTION_CHARACTERS(敵勢力ID, 候補リスト)

  FOR I = 0 TO 候補リスト:LENGTH - 1
    LOCAL キャラ = 候補リスト:I
    PRINTBUTTON [%I%] %CALLNAME:キャラ%, I
  NEXT

  LOCAL 選択キャラ = 候補リスト:RESULT

  ; 要求タイプ選択
  PRINTL %CALLNAME:選択キャラ%に何を要求する？

  PRINTBUTTON [0] 紅魔館への移籍（完全獲得）, 0
  PRINTBUTTON [1] 一時的な調教権（貸与）, 1
  PRINTBUTTON [2] 性的奉仕のみ（1回）, 2

  SELECTCASE RESULT
    CASE 0
      ; 完全獲得 → 自勢力に編入
      CALL TRANSFER_CHARACTER(選択キャラ, 勢力_紅魔館)
      CALL CAPTURE_CHARACTER_EVENT(選択キャラ, "獲得")
    CASE 1
      ; 調教権 → 一定期間調教可能
      CFLAG:選択キャラ:調教権所有者 = 勢力_紅魔館
      CFLAG:選択キャラ:調教権残り日数 = 30
      CALL CAPTURE_CHARACTER_EVENT(選択キャラ, "調教権")
    CASE 2
      ; 1回のみ
      CALL ONE_TIME_SERVICE_EVENT(選択キャラ)
  ENDSELECT
```

### 敗北時: 被要求システム（寝取られ）

```erb
@POST_BATTLE_LOSE(敵勢力ID)
  PRINTL ───敗北...───
  PRINTL スペルカードルールに則り、敗者として要求を受ける。

  ; AIによる要求決定
  LOCAL 要求タイプ = DECIDE_AI_DEMAND(敵勢力ID)
  LOCAL 要求対象 = DECIDE_AI_TARGET(敵勢力ID)

  SELECTCASE 要求タイプ
    CASE 要求_賠償金
      CALL LOSE_MONEY(敵勢力ID)
    CASE 要求_キャラ
      CALL LOSE_CHARACTER(敵勢力ID, 要求対象)
    CASE 要求_従属
      CALL BECOME_SUBORDINATE(敵勢力ID)
  ENDSELECT

@LOSE_CHARACTER(敵勢力ID, 対象キャラ)
  ; 忠誠度が高いキャラが優先的に狙われる（魔剣士リーネ式）

  PRINTL %GET_FACTION_NAME(敵勢力ID)%は%CALLNAME:対象キャラ%を要求してきた！

  ; 抵抗の選択肢
  PRINTBUTTON [0] 要求を受け入れる, 0
  PRINTBUTTON [1] 身代わりを提案する, 1
  PRINTBUTTON [2] 再戦を申し込む（リスク大）, 2

  SELECTCASE RESULT
    CASE 0
      ; 受け入れ → NTR展開
      CALL TRANSFER_CHARACTER(対象キャラ, 敵勢力ID)
      CALL NTR_BY_FACTION_EVENT(対象キャラ, 敵勢力ID)
    CASE 1
      ; 身代わり → 別キャラを差し出す
      CALL OFFER_SUBSTITUTE(敵勢力ID)
    CASE 2
      ; 再戦 → 負けたらさらに悪い条件
      CALL REMATCH(敵勢力ID)
  ENDSELECT
```

### 捕虜状態管理

```erb
; === 捕虜/獲得キャラの状態 ===
CFLAG:所属勢力          ; 現在の所属勢力ID
CFLAG:元勢力            ; 元の所属勢力ID
CFLAG:捕虜状態          ; 0:通常 1:捕虜 2:奴隷 3:調教中
CFLAG:調教権所有者      ; 調教権を持つ勢力ID
CFLAG:調教権残り日数    ; 調教権の残り日数
CFLAG:NTR元恋人フラグ   ; 主人公と交際していた場合

; === 捕虜からの遷移 ===
@UPDATE_CAPTIVE_STATE(キャラ)
  IF CFLAG:キャラ:捕虜状態 == 状態_捕虜
    ; 精神力低下 → 奴隷化判定
    IF CFLAG:キャラ:精神力 <= 0
      CFLAG:キャラ:捕虜状態 = 状態_奴隷
      CALL ENSLAVEMENT_EVENT(キャラ)
    ENDIF
  ENDIF
```

### 捕虜堕落システム（2軸）

捕虜が段階的に堕落していくシステム。**肉体陥落（ちんぽ）が先、心の陥落（好感度）が後**。

#### 堕落の流れ

```
捕虜化
  │
  ▼
【フェーズ1: 抵抗期】(0-7日)
  │ この期間に救出 → 「潔癖」維持、後遺症なし
  │
  ▼
【フェーズ2: 性的接触期】(8-21日)
  │ 愛撫、口淫等 → 肉体陥落度↑
  │ まだ心は主人公に → 敵好感度は低い
  │
  ▼
【フェーズ3: 性的陥落期】(22-45日)
  │ 本番、快楽調教 → 肉体陥落度MAX
  │ 「体は感じるけど心は…」状態
  │
  ▼
【フェーズ4: 心の陥落期】(46日-)
  │ 肉体が完全に屈服 → 敵好感度↑↑
  │ 主人公への感情が薄れる
  │
  ▼
【フェーズ5: 完全陥落】
  │ 敵好感度MAX → 敵の「将軍」化
  │ 主人公が恋人/結婚相手でも敵を愛する
  └─→ 敵勢力のリーダーユニットとして活動
```

#### 2軸ステータス

| 軸 | ステータス名 | 範囲 | 意味 |
|:--:|-------------|:----:|------|
| **肉体** | 肉体陥落度 | 0-1000 | 竿役のちんぽへの依存 |
| **精神** | 敵好感度 | -500~1000 | 竿役/敵勢力への感情 |

**進行順序**: 肉体陥落度 → 敵好感度（体が先、心が後）

```erb
; === 2軸ステータス変数 ===
CFLAG:肉体陥落度:竿役ID      ; 特定の竿役への肉体依存 (0-1000)
CFLAG:敵好感度:勢力ID        ; 敵勢力への感情 (-500 ~ 1000)
CFLAG:敵好感度:竿役ID        ; 竿役個人への感情 (-500 ~ 1000)

; === 陥落閾値 ===
#DIM CONST 肉体陥落_軽度 = 200     ; 体が反応し始める
#DIM CONST 肉体陥落_中度 = 500     ; 快楽を隠せない
#DIM CONST 肉体陥落_重度 = 800     ; 自ら求める
#DIM CONST 肉体陥落_完全 = 1000    ; 心の陥落開始条件

#DIM CONST 敵好感度_無関心 = 0
#DIM CONST 敵好感度_好意 = 300     ; 好意を持ち始める
#DIM CONST 敵好感度_恋慕 = 600     ; 恋愛感情
#DIM CONST 敵好感度_愛情 = 900     ; 愛している
#DIM CONST 敵好感度_狂愛 = 1000    ; 完全陥落、将軍化
```

#### 段階別詳細

| フェーズ | 期間 | 肉体陥落度 | 敵好感度 | 救出時の状態 |
|:--------:|:----:|:----------:|:--------:|-------------|
| 1.抵抗期 | 0-7日 | 0-100 | -500~-300 | **潔癖**（後遺症なし） |
| 2.性的接触期 | 8-21日 | 100-400 | -300~0 | **軽度汚染**（すぐ回復） |
| 3.性的陥落期 | 22-45日 | 400-1000 | 0~300 | **中度汚染**（ケア必要） |
| 4.心の陥落期 | 46日- | 1000(固定) | 300-900 | **重度汚染**（長期ケア） |
| 5.完全陥落 | - | 1000 | 1000 | **不可逆**（敵の将軍に） |

#### 処理フロー

```erb
; === 毎日の堕落進行 ===
@DAILY_CORRUPTION_UPDATE(キャラ, 竿役)
  LOCAL 日数 = CFLAG:キャラ:捕虜日数

  ; フェーズ判定
  LOCAL フェーズ = GET_CORRUPTION_PHASE(キャラ)

  SELECTCASE フェーズ
    CASE フェーズ_抵抗期
      ; 抵抗イベント、精神力消耗
      CALL RESISTANCE_EVENT(キャラ)
      CFLAG:キャラ:精神力 -= RAND:10 + 5

    CASE フェーズ_性的接触期
      ; 愛撫・口淫による肉体陥落
      IF 竿役 != 0
        CALL SEXUAL_CONTACT_EVENT(キャラ, 竿役)
        CFLAG:キャラ:肉体陥落度:竿役 += RAND:30 + 10
      ENDIF

    CASE フェーズ_性的陥落期
      ; 本番による肉体完全陥落
      IF 竿役 != 0
        CALL SEXUAL_FALL_EVENT(キャラ, 竿役)
        CFLAG:キャラ:肉体陥落度:竿役 += RAND:50 + 30
        ; 肉体がMAXに近づくと敵好感度も上がり始める
        IF CFLAG:キャラ:肉体陥落度:竿役 >= 肉体陥落_重度
          CFLAG:キャラ:敵好感度:竿役 += RAND:20 + 5
        ENDIF
      ENDIF

    CASE フェーズ_心の陥落期
      ; 心の陥落（肉体は既にMAX）
      CALL HEART_FALL_EVENT(キャラ, 竿役)
      CFLAG:キャラ:敵好感度:竿役 += RAND:40 + 20
      ; 主人公への好感度低下
      CFLAG:キャラ:好感度 -= RAND:30 + 10

    CASE フェーズ_完全陥落
      ; 将軍化処理
      CALL GENERAL_CONVERSION(キャラ, 竿役)

  ENDSELECT

  ; 捕虜日数更新
  CFLAG:キャラ:捕虜日数 += 1

; === フェーズ判定 ===
@GET_CORRUPTION_PHASE(キャラ)
  ; 完全陥落チェック（日数に関係なく）
  IF CFLAG:キャラ:敵好感度:竿役 >= 敵好感度_狂愛
    RESULT = フェーズ_完全陥落
    RETURN
  ENDIF

  ; 心の陥落チェック（肉体がMAXかつ一定日数経過）
  IF CFLAG:キャラ:肉体陥落度:竿役 >= 肉体陥落_完全
    RESULT = フェーズ_心の陥落期
    RETURN
  ENDIF

  ; 日数ベースのフェーズ
  LOCAL 日数 = CFLAG:キャラ:捕虜日数
  IF 日数 <= 7
    RESULT = フェーズ_抵抗期
  ELSEIF 日数 <= 21
    RESULT = フェーズ_性的接触期
  ELSE
    RESULT = フェーズ_性的陥落期
  ENDIF
```

#### 将軍化（完全陥落）

```erb
@GENERAL_CONVERSION(キャラ, 竿役)
  ; 敵勢力のリーダーユニットに変化
  LOCAL 敵勢力 = CFLAG:キャラ:捕虜元勢力

  PRINTL ───%CALLNAME:キャラ%は完全に堕ちた───

  ; 元の関係を記録
  IF CFLAG:キャラ:恋人フラグ
    CFLAG:キャラ:元恋人フラグ = 1
    CFLAG:キャラ:恋人フラグ = 0
  ENDIF
  IF CFLAG:キャラ:人妻フラグ
    CFLAG:キャラ:元人妻フラグ = 1
    ; 人妻フラグは維持（NTR人妻として）
  ENDIF

  ; 所属変更
  CFLAG:キャラ:所属勢力 = 敵勢力
  CFLAG:キャラ:捕虜状態 = 状態_将軍  ; 新状態

  ; 敵の将軍として戦闘に参加可能に
  CFLAG:キャラ:敵将軍フラグ = 1
  CFLAG:キャラ:担当竿役 = 竿役

  ; 専用口上
  CALL GENERAL_CONVERSION_KOJO(キャラ, 竿役)

  ; 今後は敵勢力として主人公と対峙
  ; 戦闘で倒しても「奪還」は困難（心が敵にある）
```

#### 敵の子妊娠システム

将軍化後、敵の子を産むことで**敵勢力が増強**される。

```erb
; === 将軍化後の妊娠判定 ===
@GENERAL_PREGNANCY_CHECK(キャラ, 竿役)
  ; 将軍化したキャラは毎日妊娠判定
  IF CFLAG:キャラ:敵将軍フラグ == 0
    RETURN
  ENDIF

  ; 既に妊娠中なら判定しない
  IF CFLAG:キャラ:妊娠フラグ
    RETURN
  ENDIF

  ; 妊娠確率（愛情度が高いほど上昇）
  LOCAL 確率 = 5 + CFLAG:キャラ:敵好感度:竿役 / 100
  IF RAND:100 < 確率
    CALL ENEMY_PREGNANCY_START(キャラ, 竿役)
  ENDIF

@ENEMY_PREGNANCY_START(キャラ, 竿役)
  CFLAG:キャラ:妊娠フラグ = 1
  CFLAG:キャラ:妊娠日数 = 0
  CFLAG:キャラ:胎児の父 = 竿役
  CFLAG:キャラ:胎児の勢力 = CFLAG:キャラ:所属勢力

  PRINTL %CALLNAME:キャラ%は%GET_SAOKYAKU_NAME(竿役)%の子を身籠った...

  ; 主人公への報告イベント（NTR演出）
  IF CFLAG:キャラ:元恋人フラグ || CFLAG:キャラ:元人妻フラグ
    CALL ENEMY_PREGNANCY_NTR_EVENT(キャラ, 竿役)
  ENDIF

; === 敵の子出産 ===
@ENEMY_CHILD_BIRTH(キャラ)
  LOCAL 竿役 = CFLAG:キャラ:胎児の父
  LOCAL 敵勢力 = CFLAG:キャラ:胎児の勢力

  PRINTL %CALLNAME:キャラ%は%GET_SAOKYAKU_NAME(竿役)%の子を産んだ。

  ; 子供を敵勢力のユニットとして追加
  LOCAL 子供 = CREATE_ENEMY_CHILD(キャラ, 竿役)
  CFLAG:子供:所属勢力 = 敵勢力

  ; 敵勢力の戦力増加
  勢力戦力:敵勢力 += 50

  ; 母親との絆（敵への忠誠強化）
  CFLAG:キャラ:敵好感度:竿役 += 100  ; 子供の父への愛情増加

  ; NTR口上
  IF CFLAG:キャラ:元恋人フラグ
    PRINTL 「この子は私と%GET_SAOKYAKU_NAME(竿役)%様の...愛の結晶です」
    PRINTL 「...あなた？ もう関係ないでしょう」
  ENDIF

  ; 妊娠状態リセット
  CFLAG:キャラ:妊娠フラグ = 0
  CFLAG:キャラ:妊娠日数 = 0

; === 托卵の可能性 ===
; 主人公が父親だと思っていた子が実は敵の子だった
@CHECK_CUCKOO_CHILD(キャラ)
  ; 心の陥落期（フェーズ4）で妊娠した場合
  ; 救出後に出産しても、実は敵の子という展開
  IF CFLAG:キャラ:隠し父親 != 0
    LOCAL 真の父 = CFLAG:キャラ:隠し父親
    ; 成長後に発覚するイベント
    CALL CUCKOO_REVELATION_EVENT(キャラ, 真の父)
  ENDIF
```

#### 偽りの帰還（完全陥落後の救出）

完全陥落後に「救出」しても、**心は敵のまま**。形だけの帰還となる。

```erb
; === 偽りの帰還システム ===
#DIM CONST 帰還状態_正常 = 0
#DIM CONST 帰還状態_偽り = 1      ; 心は敵にある
#DIM CONST 帰還状態_スパイ = 2    ; 積極的に情報漏洩

@FALSE_RETURN(キャラ, 竿役)
  ; 完全陥落後の「救出」
  CFLAG:キャラ:帰還状態 = 帰還状態_偽り
  CFLAG:キャラ:所属勢力 = 勢力_紅魔館  ; 表面上は紅魔館所属
  CFLAG:キャラ:真の忠誠先 = CFLAG:キャラ:捕虜元勢力

  PRINTL %CALLNAME:キャラ%は紅魔館に「戻った」...

  ; 表向きは普通に振る舞う
  ; しかし口上で本心が漏れる

; === 偽り帰還者の日常 ===
@DAILY_FALSE_RETURN_CHECK(キャラ)
  IF CFLAG:キャラ:帰還状態 != 帰還状態_偽り
    RETURN
  ENDIF

  LOCAL 竿役 = CFLAG:キャラ:担当竿役

  ; 夜に敵を想う
  IF TIME == 夜
    IF RAND:100 < 30
      CALL FALSE_RETURN_NIGHT_KOJO(キャラ, 竿役)
    ENDIF
  ENDIF

  ; 行為中に敵の名前を呼ぶ
  ; (口上側で CFLAG:帰還状態 をチェック)

  ; 密かに連絡を取っている可能性
  IF RAND:100 < 5
    CFLAG:キャラ:帰還状態 = 帰還状態_スパイ  ; エスカレート
  ENDIF

; === スパイ化 ===
@SPY_ACTIVITY(キャラ)
  IF CFLAG:キャラ:帰還状態 != 帰還状態_スパイ
    RETURN
  ENDIF

  LOCAL 敵勢力 = CFLAG:キャラ:真の忠誠先

  ; 情報漏洩
  IF RAND:100 < 20
    勢力好感度:敵勢力 += 10  ; 敵が有利に
    ; 主人公にはバレない
  ENDIF

  ; 発覚判定
  IF RAND:100 < 3
    CALL SPY_DISCOVERY_EVENT(キャラ)
  ENDIF
```

#### 偽り帰還者の口上パターン

| シチュエーション | 口上例（美鈴の場合） |
|:----------------:|---------------------|
| 日常会話 | 「...あ、はい、ご主人様」（心ここにあらず） |
| 夜の独り言 | 「...会いたい...あの人に...」 |
| 行為中 | 「あ...っ...（名前を呼びかけて飲み込む）」 |
| 行為中（漏れ） | 「...○○様...あ、いえ、何でも...」 |
| 妊娠発覚時 | 「この子は...誰の子かしら...」（曖昧な笑み） |
| スパイ発覚 | 「...バレちゃいました？ でも、後悔はしていません」 |

#### 偽り帰還者の分岐

偽りの帰還状態から3つのルートに分岐する。

```
偽りの帰還
    │
    ├──→【復活ルート】主人公のちんぽ > 敵 & 好感度回復
    │       └─→ 心が戻る、自軍ユニット完全復活
    │
    ├──→【捕虜ルート】プレイヤー選択で「捕虜」扱いに
    │       └─→ 慰み者として支配、愛ではなく服従
    │
    └──→【放置ルート】何もしない
            └─→ スパイ化、最終的に敵へ帰還
```

#### 真の復活システム

**条件**: 主人公の「男性ステータス（チンポ総合ランク）」が敵の竿役を上回る + 時間をかけて好感度回復

> **参照**: [ntr-flavor-stats.md](ntr-flavor-stats.md) - 男性ステータス詳細設計
> - `CALC_CHINPO_RANK()` で総合ランク算出
> - 長さ/太さ/硬度/持続力/精液量/回復力の6軸

#### 竿役のデフォルト男性ステータス

| 竿役 | 長さ | 太さ | 硬度 | 持続 | 精液 | 回復 | **総合** |
|------|:----:|:----:|:----:|:----:|:----:|:----:|:--------:|
| 主人公（初期） | D | D | D | D | C | C | **D** |
| 霖之助 | B | C | B | A | B | B | **B** |
| 月の使者 | A | B | A | S | A | A | **A** |
| 冥界の門番 | B | B | B | S | B | B | **B** |
| 地獄鴉の雄 | A | A | A | B | A | A | **A** |
| 信者頭 | B | B | B | B | B | B | **B** |
| 雲山 | A | S | A | A | S | A | **A** |
| 鬼の若頭 | S | S | S | A | A | S | **S** |
| 訪問者 | B | B | A | B | A | B | **B** |

**設計意図**: 主人公はDランクスタート → 成長でA~Sに到達可能（ntr-flavor-stats.md参照）

```erb
; === 男性ステータス比較（上書き判定）===
; 既存の CALC_CHINPO_RANK() を活用
#DIM CONST 上書き必要回数_偽り = 50    ; 完全陥落からの回復は困難

@CHECK_OVERWRITE_PROGRESS(キャラ)
  IF CFLAG:キャラ:帰還状態 != 帰還状態_偽り
    RETURN
  ENDIF

  LOCAL 竿役 = CFLAG:キャラ:担当竿役

  ; ntr-flavor-stats.md の CALC_CHINPO_RANK() を使用
  LOCAL 主人公ランク = CALC_CHINPO_RANK(MASTER)
  LOCAL 敵ランク = CALC_CHINPO_RANK(竿役)

  ; 総合ランク比較
  IF 主人公ランク > 敵ランク
    ; 上書き可能状態
    CFLAG:キャラ:上書き進行中フラグ = 1
  ELSE
    ; 敵の方が上 → 上書き進まない、むしろ敵を思い出す
    IF RAND:100 < 20
      PRINTL （%CALLNAME:キャラ%は物足りなさそうにしている...）
      CFLAG:キャラ:敵思慕度 += 10
    ENDIF
    RETURN
  ENDIF

; === 行為による上書き処理 ===
@PROCESS_OVERWRITE(キャラ)
  IF CFLAG:キャラ:上書き進行中フラグ == 0
    RETURN
  ENDIF

  ; 行為ごとに上書きカウント
  CFLAG:キャラ:上書き回数 += 1

  ; 上書き進行の口上
  LOCAL 進捗 = CFLAG:キャラ:上書き回数 * 100 / 上書き必要回数_偽り
  IF 進捗 < 30
    PRINTL 「...（まだ、あの人の方が...）」
  ELSEIF 進捗 < 60
    PRINTL 「...あれ...？ ご主人様...？」
  ELSEIF 進捗 < 90
    PRINTL 「ご主人様...私...何を...」
  ELSE
    PRINTL 「ご主人様...！ 私...ずっと...！」
  ENDIF

  ; 好感度回復
  CFLAG:キャラ:好感度 += 5

  ; 完全復活判定
  IF CFLAG:キャラ:上書き回数 >= 上書き必要回数_偽り
    CALL TRUE_REVIVAL(キャラ)
  ENDIF

; === 真の復活 ===
@TRUE_REVIVAL(キャラ)
  PRINTL ───%CALLNAME:キャラ%の心が、ついに戻った───

  ; 状態リセット
  CFLAG:キャラ:帰還状態 = 帰還状態_正常
  CFLAG:キャラ:敵好感度:担当竿役 = 0
  CFLAG:キャラ:肉体陥落度:担当竿役 = 0
  CFLAG:キャラ:真の忠誠先 = 0

  ; 復活口上
  PRINTL 「ご主人様...ごめんなさい...私...」
  PRINTL 「でも...もう大丈夫です。私の心は...あなただけのもの」

  ; 完全復活ボーナス
  CFLAG:キャラ:忠誠度 = 100     ; 絶対忠誠
  CFLAG:キャラ:復活フラグ = 1   ; 特別な絆

  ; 復活キャラは「二度と堕ちない」特性を得る可能性
  IF CFLAG:キャラ:好感度 >= 1000
    TALENT:キャラ:不堕 = 1       ; 特殊素質「不堕」獲得
  ENDIF
```

#### 捕虜ルート（慰み者化）

偽り帰還者を「愛で取り戻す」のではなく、**力で支配**する選択肢。

```erb
; === 捕虜化選択 ===
@CONVERT_TO_CAPTIVE(キャラ)
  ; プレイヤーが偽り帰還者を「捕虜」として扱うことを選択

  PRINTL %CALLNAME:キャラ%を捕虜として扱う？
  PRINTL （愛ではなく支配。二度と対等な関係には戻れない）

  PRINTBUTTON [0] 捕虜にする, 0
  PRINTBUTTON [1] やめる, 1

  IF RESULT == 0
    CALL PROCESS_CAPTIVE_CONVERSION(キャラ)
  ENDIF

@PROCESS_CAPTIVE_CONVERSION(キャラ)
  PRINTL ───%CALLNAME:キャラ%は捕虜となった───

  ; 状態変更
  CFLAG:キャラ:帰還状態 = 帰還状態_正常  ; 偽り帰還解除
  CFLAG:キャラ:捕虜状態 = 状態_慰み者     ; 新状態

  ; 関係性の変化
  CFLAG:キャラ:恋人フラグ = 0
  CFLAG:キャラ:人妻フラグ = 0
  CFLAG:キャラ:慰み者フラグ = 1

  ; スパイリスク消失（完全支配下）
  CFLAG:キャラ:真の忠誠先 = 0

  ; 戦闘ユニットとしては使用可能
  ; ただし口上が変化（愛ではなく服従）

  PRINTL 「...分かりました。私は...もう、あなたの物です」
  PRINTL （その目に愛はない。ただ、諦めがあるだけだった）

; === 慰み者の特性 ===
; - 命令には従う（恐怖/諦め）
; - 愛情表現なし
; - 敵への思慕は残る（口上で漏れる）
; - 戦闘参加可能だが士気低い
; - 逃亡リスクあり

@DAILY_CAPTIVE_CHECK(キャラ)
  IF CFLAG:キャラ:捕虜状態 != 状態_慰み者
    RETURN
  ENDIF

  ; 逃亡判定
  IF RAND:100 < 3
    LOCAL 敵勢力 = CFLAG:キャラ:元捕虜勢力
    IF 敵勢力 > 0
      ; 元の敵勢力への逃亡試行
      CALL ESCAPE_ATTEMPT(キャラ, 敵勢力)
    ENDIF
  ENDIF

  ; 敵を想う口上（低確率）
  IF RAND:100 < 10
    LOCAL 竿役 = CFLAG:キャラ:担当竿役
    PRINTL （%CALLNAME:キャラ%は窓の外を見つめている...）
    PRINTL 「...%GET_SAOKYAKU_NAME(竿役)%様...」
  ENDIF
```

#### 慰み者の口上パターン

| シチュエーション | 口上例（美鈴の場合） |
|:----------------:|---------------------|
| 命令時 | 「...はい」（感情のない声） |
| 行為中 | 「...」（反応はするが、心がない） |
| 行為後 | 「...もう、いいですか？」 |
| 敵を想う | 「（...あの人なら...もっと...）」 |
| 逃亡失敗 | 「...逃げられると、思ったのに...」 |

#### ルート比較

| ルート | 条件 | 結果 | リスク |
|:------:|------|------|--------|
| **復活** | ちんぽ>敵 & 50回行為 | 完全復活、絶対忠誠 | 時間がかかる |
| **捕虜** | プレイヤー選択 | 支配下、戦闘可 | 逃亡リスク、愛なし |
| **放置** | 何もしない | スパイ化→敵帰還 | 敵戦力増、情報漏洩 |

#### 救出時の処理

```erb
@RESCUE_CAPTIVE(キャラ)
  LOCAL フェーズ = GET_CORRUPTION_PHASE(キャラ)
  LOCAL 竿役 = GET_CAPTIVE_SAOKYAKU(キャラ)

  SELECTCASE フェーズ
    CASE フェーズ_抵抗期
      ; 潔癖維持
      PRINTL 間に合った...%CALLNAME:キャラ%は無事だ。
      CFLAG:キャラ:肉体陥落度:竿役 = 0
      CFLAG:キャラ:敵好感度:竿役 = -500
      ; 後遺症なし

    CASE フェーズ_性的接触期
      ; 軽度汚染
      PRINTL %CALLNAME:キャラ%を救出した...だが、何かされたようだ。
      ; 肉体陥落度は残る（数日で自然回復）
      CFLAG:キャラ:汚染状態 = 汚染_軽度
      CFLAG:キャラ:汚染回復日数 = 7

    CASE フェーズ_性的陥落期
      ; 中度汚染
      PRINTL %CALLNAME:キャラ%を救出した...しかし、体が...
      CFLAG:キャラ:汚染状態 = 汚染_中度
      ; 主人公との行為で「上書き」が必要
      CFLAG:キャラ:上書き必要回数 = 10 + CFLAG:キャラ:肉体陥落度:竿役 / 100

    CASE フェーズ_心の陥落期
      ; 重度汚染
      PRINTL %CALLNAME:キャラ%を救出したが...心がここにない。
      CFLAG:キャラ:汚染状態 = 汚染_重度
      ; 長期ケア必要、敵を思い出す口上が発生
      CFLAG:キャラ:敵思慕フラグ = 1

    CASE フェーズ_完全陥落
      ; 完全陥落 → 「偽りの帰還」
      ; 物理的には連れ戻せるが、心は敵にある
      PRINTL %CALLNAME:キャラ%を連れ戻した...だが...
      PRINTL 「...」
      PRINTL その目は、もうこちらを見ていなかった。

      ; 偽りの帰還状態へ
      LOCAL 竿役 = GET_CAPTIVE_SAOKYAKU(キャラ)
      CALL FALSE_RETURN(キャラ, 竿役)

      ; 形式上は救出成功だが...
      CFLAG:キャラ:捕虜状態 = 状態_通常
      CFLAG:キャラ:所属勢力 = 勢力_紅魔館
      RESULT = 1  ; 「成功」だが実質的には失敗
      RETURN

  ENDSELECT

  ; 捕虜状態解除
  CFLAG:キャラ:捕虜状態 = 状態_通常
  CFLAG:キャラ:所属勢力 = 勢力_紅魔館
  RESULT = 1
```

#### 口上パターン

| フェーズ | 口上例（美鈴の場合） |
|:--------:|---------------------|
| 抵抗期 | 「離して...私には...ご主人様がいるの...」 |
| 性的接触期 | 「やめて...体が...でも、心は...」 |
| 性的陥落期 | 「あ...また...体が勝手に...ご主人様、ごめんなさい...」 |
| 心の陥落期 | 「ご主人様って...誰のこと？ 私の主人は...」 |
| 完全陥落 | 「あなた様の為なら何でもします...紅魔館？ 敵ですね」 |

---

## 5. 外交システム

### 外交アクション

eratohoKを参考に、初期は外交制限を設ける。

```erb
; === 外交制限 ===
#DIM CONST 外交解禁ターン = 6    ; 最初6ターンは宣戦不可
#DIM CONST 停戦最低期間 = 10     ; 停戦は最低10ターン

@CAN_DECLARE_WAR(対象勢力)
  IF FLAG:ゲームターン < 外交解禁ターン
    RESULT = 0
    RETURN
  ENDIF
  IF 同盟残りターン:対象勢力 > 0
    RESULT = 0
    RETURN
  ENDIF
  RESULT = 1

; === 外交コマンド ===
@DIPLOMACY_MENU(対象勢力)
  PRINTL %GET_FACTION_NAME(対象勢力)%との外交

  PRINTBUTTON [0] 関係を深める, 0
  PRINTBUTTON [1] 停戦を申し込む, 1
  PRINTBUTTON [2] 同盟を申し込む, 2
  PRINTBUTTON [3] 宣戦布告, 3
  PRINTBUTTON [4] キャラを貸す, 4
  PRINTBUTTON [5] キャラを要求する, 5

  SELECTCASE RESULT
    CASE 0
      CALL DEEPEN_FRIENDSHIP(対象勢力)
    CASE 1
      CALL PROPOSE_CEASEFIRE(対象勢力)
    CASE 2
      CALL PROPOSE_ALLIANCE(対象勢力)
    CASE 3
      CALL DECLARE_WAR(対象勢力)
    CASE 4
      CALL LEND_CHARACTER(対象勢力)
    CASE 5
      CALL REQUEST_CHARACTER(対象勢力)
  ENDSELECT
```

### キャラを貸す（NTR外交）

eratohoKの「Discuss」オプションを発展。

```erb
@LEND_CHARACTER(対象勢力)
  PRINTL 誰を貸す？（関係向上の代償）

  ; 自勢力のキャラ一覧
  FOR I = 0 TO 自勢力キャラ:LENGTH - 1
    LOCAL キャラ = 自勢力キャラ:I
    IF キャラ != MASTER  ; 主人公は除外
      PRINTBUTTON [%I%] %CALLNAME:キャラ%, I
    ENDIF
  NEXT

  LOCAL 貸すキャラ = 自勢力キャラ:RESULT

  ; 貸す期間
  PRINTL 何日間貸す？
  PRINTBUTTON [0] 1日（関係+10）, 1
  PRINTBUTTON [1] 7日（関係+50）, 7
  PRINTBUTTON [2] 30日（関係+150）, 30

  LOCAL 期間 = RESULT

  ; 貸し出し処理
  CFLAG:貸すキャラ:貸出先 = 対象勢力
  CFLAG:貸すキャラ:貸出残り日数 = 期間

  ; 関係向上
  勢力好感度:対象勢力 += 期間 * 5

  ; NTRイベント（貸出中に発生）
  IF 期間 >= 7
    CFLAG:貸すキャラ:貸出NTRフラグ = 1
  ENDIF

  CALL LEND_CHARACTER_EVENT(貸すキャラ, 対象勢力, 期間)
```

### 強国からの要求

eratohoK: 強国リーダーが金銭やキャラを要求してくる。

```erb
@CHECK_STRONG_FACTION_DEMAND
  FOR 勢力 = 1 TO 勢力_MAX
    IF 勢力戦力:勢力 > 勢力戦力:勢力_紅魔館 * 1.5
      ; 強国判定
      IF RAND:100 < 10  ; 10%の確率で要求
        CALL STRONG_FACTION_DEMAND(勢力)
      ENDIF
    ENDIF
  NEXT

@STRONG_FACTION_DEMAND(勢力)
  LOCAL リーダー = GET_FACTION_LEADER(勢力)

  PRINTL %CALLNAME:リーダー%からの使者が来た。

  LOCAL 要求タイプ = RAND:3

  SELECTCASE 要求タイプ
    CASE 0
      ; 金銭要求
      PRINTL 「貢ぎ物を寄越しなさい」
      CALL TRIBUTE_DEMAND(勢力)
    CASE 1
      ; キャラ要求（調教目的）
      PRINTL 「あなたの部下を一人、こちらで預かりたいのだけど」
      CALL CHARACTER_DEMAND(勢力)
    CASE 2
      ; 主人公の「奉仕」要求
      PRINTL 「あなた自身に来てもらいたいの」
      CALL PERSONAL_SERVICE_DEMAND(勢力, リーダー)
  ENDSELECT
```

---

## 6. 忠誠度システム

### 魔剣士リーネ式の逆説

**高忠誠 = NTR対象になりやすい**

```erb
; === NTR対象選定（敵AI） ===
@SELECT_NTR_TARGET(自勢力キャラリスト)
  ; 忠誠度でソート（高い順）
  CALL SORT_BY_LOYALTY(自勢力キャラリスト, DESC)

  ; 最も忠誠度が高いキャラを優先
  ; 「愛されているほど奪いたい」というNTR心理

  LOCAL 候補 = 自勢力キャラリスト:0

  ; ただし忠誠度100（絶対忠誠）なら回避可能
  IF CFLAG:候補:忠誠度 >= 100
    ; 選択肢を与える
    PRINTL %CALLNAME:候補%は絶対的な忠誠を示している。
    PRINTBUTTON [0] それでも要求する, 0
    PRINTBUTTON [1] 別のキャラを選ぶ, 1

    IF RESULT == 1
      候補 = 自勢力キャラリスト:1
    ENDIF
  ENDIF

  RESULT = 候補
```

### 忠誠度の変動

```erb
; === 忠誠度上昇 ===
@RAISE_LOYALTY(キャラ, 量)
  ; 戦闘参加で上昇
  ; 贈り物で上昇
  ; 主人公との交流で上昇

  CFLAG:キャラ:忠誠度 += 量
  CFLAG:キャラ:忠誠度 = MIN(CFLAG:キャラ:忠誠度, 100)

; === 忠誠度低下 ===
@LOWER_LOYALTY(キャラ, 量)
  ; 放置で低下
  ; NTR被害で低下
  ; 敗戦で低下

  CFLAG:キャラ:忠誠度 -= 量
  CFLAG:キャラ:忠誠度 = MAX(CFLAG:キャラ:忠誠度, 0)

  ; 低忠誠での反乱判定
  IF CFLAG:キャラ:忠誠度 < 20
    CALL CHECK_REBELLION(キャラ)
  ENDIF
```

### 反乱システム

eratohoKの反乱メカニズムを参考。

```erb
; === 反乱判定 ===
@CHECK_REBELLION(キャラ)
  ; 反乱スコア計算
  LOCAL スコア = 0

  ; 性格特性による加算
  IF TALENT:キャラ:傲慢
    スコア += 20
  ENDIF
  IF TALENT:キャラ:生意気
    スコア += 15
  ENDIF
  IF TALENT:キャラ:反抗的
    スコア += 25
  ENDIF

  ; 性格特性による減算
  IF TALENT:キャラ:従順
    スコア -= 20
  ENDIF
  IF TALENT:キャラ:臆病
    スコア -= 15
  ENDIF

  ; 野心度による倍率
  LOCAL 野心倍率 = 1.0
  IF CFLAG:キャラ:野心 == 野心_C
    野心倍率 = 1.1
  ELSEIF CFLAG:キャラ:野心 == 野心_B
    野心倍率 = 1.25
  ELSEIF CFLAG:キャラ:野心 == 野心_A
    野心倍率 = 1.5
  ENDIF

  スコア *= 野心倍率

  ; 忠誠度が低いほど反乱確率上昇
  LOCAL 反乱確率 = スコア * (100 - CFLAG:キャラ:忠誠度) / 100

  IF RAND:100 < 反乱確率
    CALL REBELLION_EVENT(キャラ)
  ENDIF

@REBELLION_EVENT(キャラ)
  PRINTL %CALLNAME:キャラ%が反乱を起こした！

  ; 反乱の規模
  IF CFLAG:キャラ:野心 >= 野心_A
    ; 大規模反乱：他のキャラも引き連れる
    CALL MAJOR_REBELLION(キャラ)
  ELSE
    ; 小規模：単独離脱
    CALL MINOR_REBELLION(キャラ)
  ENDIF
```

---

## 特殊勢力詳細

### サキュバス集団（特殊勢力90）

```erb
@SUCCUBUS_FACTION_EFFECT
  ; 10日ごとに捕虜のステータスを吸収

  IF DAY % 10 == 0
    FOR キャラ IN サキュバス捕虜リスト
      ; 精力吸収
      CFLAG:キャラ:精力 -= 100
      CFLAG:キャラ:体力 -= 50

      ; 快楽耐性低下
      CFLAG:キャラ:快楽耐性 -= 10

      ; 堕落進行
      CFLAG:キャラ:サキュバス堕落度 += 50

      IF CFLAG:キャラ:サキュバス堕落度 >= 1000
        ; 完全堕落 → サキュバス化
        CALL SUCCUBUS_CONVERSION(キャラ)
      ENDIF
    NEXT
  ENDIF
```

### 旧支配者（特殊勢力93）

```erb
@OLD_ONES_FACTION_EFFECT
  ; 触手、異形NTR
  ; SAN値システム

  FOR キャラ IN 旧支配者捕虜リスト
    ; SAN値減少
    CFLAG:キャラ:SAN値 -= RAND:20 + 10

    IF CFLAG:キャラ:SAN値 <= 0
      ; 狂気
      CALL MADNESS_EVENT(キャラ)
    ELSEIF CFLAG:キャラ:SAN値 <= 30
      ; 狂気の兆候
      CALL MADNESS_SIGN_EVENT(キャラ)
    ENDIF

    ; 触手妊娠判定
    IF RAND:100 < 5
      CALL TENTACLE_PREGNANCY(キャラ)
    ENDIF
  NEXT
```

---

## 実装フェーズ

### Phase S5: 異変・勢力基盤 (v3.5)

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| S5-1 | 勢力定数・変数定義 | feature-300 |
| S5-2 | 勢力ステータス管理 | feature-301 |
| S5-3 | 異変タイプ定義 | feature-302 |
| S5-4 | 異変発動コマンド | feature-303 |
| S5-5 | 異変進行処理 | feature-304 |
| S5-6 | 勢力キャラ初期配置 | feature-305 |

### Phase S6: 弾幕ごっこ (v4.0)

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| S6-1 | 戦闘ステータス定義 | feature-310 |
| S6-2 | 戦闘システム基盤 | feature-311 |
| S6-3 | スペルカード処理 | feature-312 |
| S6-4 | 種族相性システム | feature-313 |
| S6-5 | 戦闘AI（敵軍） | feature-314 |
| S6-6 | 戦闘UI・演出 | feature-315 |

### Phase S6+: 戦後・外交 (v4.2)

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| S6+-1 | 戦後処理（勝利）| feature-320 |
| S6+-2 | 戦後処理（敗北）| feature-321 |
| S6+-3 | 捕虜管理システム | feature-322 |
| S6+-4 | 外交コマンド | feature-323 |
| S6+-5 | キャラ貸出システム | feature-324 |
| S6+-6 | 強国要求システム | feature-325 |

### Phase S6++: 忠誠・反乱 (v4.3)

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| S6++-1 | 忠誠度システム | feature-330 |
| S6++-2 | 反乱判定 | feature-331 |
| S6++-3 | 反乱イベント | feature-332 |
| S6++-4 | NTR対象選定AI | feature-333 |

### Phase S7: 特殊勢力 (v4.5)

| Phase | 内容 | Feature候補 |
|:-----:|------|-------------|
| S7-1 | 特殊勢力定義 | feature-340 |
| S7-2 | サキュバス勢力 | feature-341 |
| S7-3 | 邪仙道勢力 | feature-342 |
| S7-4 | 月の都勢力 | feature-343 |
| S7-5 | 旧支配者勢力 | feature-344 |
| S7-6 | 特殊NTRイベント | feature-345+ |

---

## 依存関係

```
v2.2 外部マップ ───┐
                   │
v2.7 妊娠システム ─┼─→ v3.5 異変・勢力基盤
                   │         │
v3.0 メディア演出 ─┘         ▼
                       v4.0 弾幕ごっこ
                             │
                             ▼
                       v4.2 戦後・外交
                             │
                             ▼
                       v4.3 忠誠・反乱
                             │
                             ▼
                       v4.5 特殊勢力・勢力間NTR
```

---

## 口上要件

### 勢力別口上

各勢力リーダー・キャラの個性を反映した口上が必要。

| 勢力 | 口上トーン | 例 |
|------|-----------|-----|
| 永遠亭 | 優雅・余裕 | 「月の民には敵わないわよ」 |
| 白玉楼 | 儚げ・冥界風 | 「冥界にお連れしましょうか」 |
| 地霊殿 | 読心ネタ | 「あなたの考えは全部お見通し」 |
| 守矢神社 | 信仰アピール | 「信仰を集めるのです！」 |

### NTR口上

| シチュエーション | 必要口上 |
|-----------------|---------|
| 敗北→キャラ奪われ | 別れの口上、連行口上 |
| 貸出中NTR | 帰還報告、状態変化口上 |
| 特殊勢力NTR | 堕落進行、異形化口上 |
| 奪還成功 | 再会口上、後遺症口上 |

---

## 参考資料

### 調査ゲーム

- [戦国ランス](https://rance.ken-shin.net/contents/captive.html) - 捕虜システム、好感度
- [魔剣士リーネ2](https://www.makuracover.com/leane2/top.htm) - 女囚/ハーレムシステム、忠誠度
- [eratohoK](https://wiki.eragames.rip/index.php/EraTohoK) - 勢力外交、特殊勢力、奴隷化
- [eratohoK Endings](https://wiki.eragames.rip/index.php/EraTohoK/Endings) - エンディング分岐

### 参考資料

- [戦国ランス NTRレビュー](https://h-otoku.com/review/netori/netori-rekisi/sengokuransu/) - NTR要素分析
- [NTR BLOG - 魔剣士リーネ2](https://ntrblog.com/archives/1064565091.html) - システム解説

---

## 未解決事項

1. **戦闘バランス**: 戦力差が大きすぎると一方的になる調整
2. **キャラ追加工数**: 他勢力キャラの実装コスト
3. **口上量**: 勢力数×シチュエーション数の口上作成
4. **既存NTRとの整合**: 訪問者NTRと勢力NTRの関係
5. **プレイ時間**: 異変・戦闘がゲームテンポに与える影響

---

## 議論ログ

| 日付 | 内容 |
|------|------|
| 2025-12-19 | 初期提案。勢力間戦争によるNTR両立の構想 |
| 2025-12-19 | 参考ゲーム調査（戦国ランス、魔剣士リーネ2、eratohoK） |
| 2025-12-19 | コア要素6点を決定（勢力/異変/戦闘/戦後/外交/忠誠度） |
| 2025-12-19 | 設計ドキュメント初版作成 |
| 2025-12-19 | 各勢力に竿役を追加（霖之助、月の使者、冥界の門番など） |
| 2025-12-19 | 訪問者一族（勢力ID:8）を追加、既存NTRシステムとの統合 |
| 2025-12-19 | 2軸捕虜堕落システム追加（肉体陥落度→敵好感度の順で進行） |
| 2025-12-19 | 5フェーズ堕落進行（抵抗期→性的接触期→性的陥落期→心の陥落期→完全陥落） |
| 2025-12-19 | 将軍化システム追加（完全陥落で敵勢力のリーダーユニット化） |
| 2025-12-19 | 敵の子妊娠システム追加（出産で敵勢力戦力増強、托卵要素） |
| 2025-12-19 | 偽りの帰還システム追加（完全陥落後の救出でも心は敵のまま、スパイ化可能）|
| 2025-12-19 | 偽り帰還者の分岐追加（復活/捕虜/放置の3ルート） |
| 2025-12-19 | 真の復活システム追加（ちんぽステータス比較、上書き50回で完全復活）|
| 2025-12-19 | 捕虜ルート追加（慰み者化、逃亡リスク、愛なき服従）|
| 2025-12-19 | 男性ステータスをntr-flavor-stats.mdの既存設計に統合（CALC_CHINPO_RANK参照）|
| 2025-12-19 | 各竿役のデフォルト6軸ステータス表を追加（D～S）|
