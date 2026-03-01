# eraTW口上システム構造調査レポート

**調査日**: 2025-12-18
**対象**: eraTW4.920 口上・メッセージ関連システム
**フォーカス**: COM以外のイベント・状況ベース口上

---

## 1. 口上カテゴリ全体構造

eraTWの口上システムは中央管理関数 `@KOJO_MESSAGE_SEND` で統括されている。

### 1.1 主要口上種別（KOJO_MESSAGE.ERB）

```
主要: ENCOUNTER, SP_EVENT, EVENT, COMMAND, COUNTER
追加: PALAM, MARK, DIRECT, SUCCESS, ENDING
特殊: ONABARE, PERMISSION, LOST_VIRGIN_STOP, CHILD, DANMAKU,
      IRAI, GIFT, DAILY, DIARY, MUSHI_BATTLE, GRAVITY, SUIKA,
      ODEKAKE, SEX_FRIEND, BEFORETRAIN
```

**COM以外の主要イベント系口上は以下の5つ：**

| 種別 | 日本語名 | 呼び出しパターン | 用途 |
|------|--------|-----------------|------|
| ENCOUNTER | ファーストインプレッション | `M_KOJO_ENCOUNTER_K{NO}` | 初対面・挨拶タイミング |
| SP_EVENT | スペシャルイベント | `M_KOJO_SPEVENT_K{NO}_{ID}` | デート、告白など主要イベント |
| EVENT | イベント系 | `M_KOJO_EVENT_K{NO}_{ID}` | 時間停止、なりきり、その他日常イベント |
| COMMAND | コマンド系 | `M_KOJO_MESSAGE_COM_K{NO}_{ID}` | プレイヤーの行動選択（会話、スキンシップなど） |
| COUNTER | カウンター（自動発動） | `M_KOJO_MESSAGE_COUNTER_K{NO}_{ID}` | 距離が近い、チラ見、笑顔など自動トリガー |

---

## 2. COM以外のイベント系口上の詳細

### 2.1 ENCOUNTER（初対面・挨拶系）

**ファイル**: M_KOJO_K1_イベント.ERB（例：霊夢）

**特徴**:
- 最初の出会いシーン専用
- 1つのラベル: `@M_KOJO_ENCOUNTER_K{NO}`
- TALENTの初期化も可能

**分岐条件**:
- CFLAG:31のビット判定で、複数回目以降の判定
- 初回と複数回目で異なる内容

**参考実装（霊夢 ENCOUNTER）**:
```
- 初回: 初対面の警戒的な態度
- 複数回以上: 親密さが増した挨拶
- 内容バリエーション: 2種類（初回vs複数回以降）
```

---

### 2.2 SP_EVENT（スペシャルイベント）

**ファイル**: M_KOJO_K1_イベント.ERB

**特徴**:
- 主要な分岐ストーリーイベント
- 2つのメソッド構成:
  - `@M_KOJO_SPEVENT_MESSAGECHECK_K{NO}_{ID}`: 描写スキップ判定
  - `@M_KOJO_SPEVENT_K{NO}_{ID}`: 実際の口上テキスト

**呼び出しパターン**:
```
KOJO_MESSAGE_SEND("SP_EVENT", イベント番号, キャラNo, ARG:3, ARG:4)
```

**分岐条件の返値**（MESSAGECHECK）:
- `0`: 描写＋口上表示
- `1`: 描写非表示＋口上表示
- `2`: 描写表示＋口上非表示
- `3`: 両方非表示

**参考実装（霊夢 SP_EVENT）**:
- `_1`: デート帰りのファーストキス（合意取得）
- `_2`: デート帰りの告白（恋人フラグ取得）
  - 告白内容分岐 (ARG==0: 告白, ARG==1: 承認)

---

### 2.3 EVENT（日常イベント系）

**ファイル**: M_KOJO_K1_イベント.ERB

**特徴**:
- 最も拡張性が高いイベント基盤
- 通常イベント・時間停止・なりきり対応
- 個別キャラ口上がない場合は共通口上へフォールバック

**呼び出しパターン**:
```
KOJO_MESSAGE_SEND("EVENT", イベント番号, キャラNo, ARG:3, ARG:4)
または
KOJO_MESSAGE_SEND("EVENT", ARGS:1="名前", キャラNo, ARG:3, ARG:4)
```

**ラベルパターン**:
```
@M_KOJO_EVENT_K{NO}_{ID}          # 数値ID版
@M_KOJO_EVENT_K{NO}_{文字列ID}    # 文字列ID版（ARGS:1使用）
```

**条件フラグ**:
- `FLAG:70`: 時間停止中フラグ
- `CFLAG:キャラ:時間停止口上有`: 時間停止対応フラグ
- `FLAG:なりきり`: なりきり中フラグ
- `CFLAG:キャラ:なりきり口上有`: なりきり対応フラグ

---

### 2.4 COUNTER（自動カウンター口上）

**ファイル**: M_KOJO_K1_COUNTER.ERB

**特徴**:
- プレイヤーの行動に対して自動発動
- 定型的なしぐさやリアクション
- TALENT等の愛情度で分岐

**実装カテゴリ**（霊夢の例）:

#### コミュニケーション系
| ID | 名称 | 分岐条件 | バリエーション数 |
|:--:|------|--------|:----------------:|
| 1 | 距離が近い | TALENT: 恋慕時特殊 | 2-3 |
| 2 | チラ見 | TALENT: 恋慕で異分岐 | 2-4 |
| 3 | おはなし | - | 1 |
| 4 | いい匂い | - | 2 |

#### ベリーソフト系（親密度高時）
| ID | 名称 | TALENT分岐 | 特徴 |
|:--:|------|:--------:|--------|
| 11 | 近寄ってくる | 恋慕時 | 距離縮小 |
| 12 | 笑顔を浮かべる | - | ポジティブ反応 |
| 13 | 構って欲しがる | - | 甘えぐさ |
| 14 | 身をすり寄せる | 恋慕時 | 接触増加 |

**TALENT分岐パターン**:
- `TALENT:恋人`: 最高愛情度（付き合った状態）
- `TALENT:恋慕`: 高愛情度（好意を抱いている）
- `TALENT:思慕`: 中愛情度（好意あり）
- その他: 低親密度

**実装特徴**:
```erb
@M_KOJO_MESSAGE_COUNTER_K1_{ID}
CALL M_KOJO_MESSAGE_COUNTER_K1_{ID}_1
RETURN RESULT

@M_KOJO_MESSAGE_COUNTER_K1_{ID}_1
PRINTDATA
  DATALIST
    DATAFORM 「バリエーション1」
    ...
  ENDLIST
  DATALIST
    DATAFORM 「バリエーション2」
    ...
  ENDLIST
ENDDATA
```

---

## 3. COMMAND（コマンド系口上）詳細

**ファイル**: M_KOJO_K1_コマンド.ERB

### 3.1 構造

```
@M_KOJO_MESSAGE_COM_K1_{ID}         # 中継ラベル
  ↓
CALL M_KOJO_MESSAGE_COM_K1_{ID}_1   # 実装ラベル（バリエーション処理）
```

### 3.2 COMカテゴリ一覧（霊夢の例）

| ID | 名称 | 記述有無 | TALENT分岐 | バリエーション | 特記 |
|:--:|------|:-------:|:--------:|:------------:|------|
| 300 | 会話 | ✓ | 4段階 | 複数 | PRINTDATA多用 |
| 301 | お茶を淹れる | ✓ | 3段階 | 複数 | - |
| 302 | スキンシップ | ✓ | 4段階 | 6-7 | 物理接触描写 |
| 303 | 謝る | ✓ | 3段階 | TFLAG:193分岐 | 喧嘩和解系 |
| 304 | 仕事を手伝う | ✓ | 4段階 | CFLAG:350による分岐 | 掃除判定有 |
| 305 | 膝枕してもらう | ✓ | 4段階 | 2-3 | 親密度強制分岐 |
| 306 | お腹を撫でる | ✓ | 複雑 | 妊娠判定含む | TFLAG:193参照 |

### 3.3 TALENT分岐パターン（重要）

**4段階分岐** (300, 301, 302, 305の基本):
```
IF TALENT:恋人
  [最高愛情度用の台詞]
ELSEIF TALENT:恋慕
  [高愛情度用の台詞]
ELSEIF TALENT:思慕
  [中愛情度用の台詞]
ELSE
  [低親密度用の台詞]
ENDIF
```

### 3.4 バリエーション実装手法

**PRINTDATA + DATALIST**:
```erb
PRINTDATA
  DATALIST
    DATAFORM 「バリエーション1」
    DATAFORM 「文字列」
  ENDLIST
  DATALIST
    DATAFORM 「バリエーション2」
    DATAFORM 「文字列」
  ENDLIST
ENDDATA
WAIT
```

**用途**: 複数の短い台詞バリエーションをランダム選択

**条件フラグ活用**:
- `CFLAG:TARGET:添い寝中`: 就寝状態
- `CFLAG:TARGET:デート中`: デート中
- `BATHROOM()`: 風呂シーン判定
- `FLAG:時間停止`: 時間停止フラグ
- `TALENT:TARGET:妊娠`: 妊娠状態（306で重要）

---

## 4. ファイルの統合構造

```
個人口上フォルダ (ERB/口上・メッセージ関連/個人口上/001 Reimu [霊夢]/霊夢/)
├── M_KOJO_K1_イベント.ERB
│   ├── @M_KOJO_K1                        (存在判定)
│   ├── @M_KOJO_FLAGSETTING_K1             (初期化)
│   ├── @M_KOJO_COLOR_K1                   (色設定)
│   ├── @M_KOJO_ENCOUNTER_K1               (初対面)
│   ├── @M_KOJO_SPEVENT_MESSAGECHECK_K1_{ID}
│   └── @M_KOJO_SPEVENT_K1_{ID}           (特別イベント)
│
├── M_KOJO_K1_コマンド.ERB
│   ├── @M_KOJO_MESSAGE_COM_K1_300      (会話)
│   ├── @M_KOJO_MESSAGE_COM_K1_301      (お茶)
│   ├── @M_KOJO_MESSAGE_COM_K1_302      (スキンシップ)
│   ├── @M_KOJO_MESSAGE_COM_K1_303      (謝る)
│   ├── @M_KOJO_MESSAGE_COM_K1_304      (仕事手伝い)
│   ├── @M_KOJO_MESSAGE_COM_K1_305      (膝枕)
│   └── @M_KOJO_MESSAGE_COM_K1_306      (お腹撫でる)
│       (その他COMコマンド...)
│
└── M_KOJO_K1_COUNTER.ERB
    ├── @M_KOJO_MESSAGE_COUNTER_K1_{ID}
    └── (コミュニケーション・ベリーソフトシーン)
```

---

## 5. 実装パターンリファレンス

### 5.1 単純な愛情度分岐 + バリエーション

**用途**: 会話、お茶など日常系

```erb
@M_KOJO_MESSAGE_COM_K1_300_1
LOCAL = 1  ; 記入チェック
IF LOCAL && !FLAG:時間停止
  IF TALENT:恋人
    PRINTDATA
      DATALIST
        DATAFORM 「バリエーション1」
      ENDLIST
      DATALIST
        DATAFORM 「バリエーション2」
      ENDLIST
    ENDDATA
    WAIT
    RETURN 1
  ELSEIF TALENT:恋慕
    ; 別パターン
  ELSE
    ; その他パターン
  ENDIF
ENDIF
RETURN 1
```

### 5.2 複数の条件フラグ組み合わせ

**用途**: 状況依存型（仕事手伝い、お腹撫でる）

```erb
@M_KOJO_MESSAGE_COM_K1_304_1
LOCAL = 1
IF LOCAL && !FLAG:時間停止
  IF CFLAG:1:350 == 40  ; 掃除判定
    IF FLAG:時間停止
      ; 時間停止時の対応
    ELSEIF TALENT:恋人
      ; 恋人時の対応
    ENDIF
  ENDIF
ENDIF
```

### 5.3 イベントID参照 + PRINTDATA

**用途**: スペシャルイベント分岐

```erb
@M_KOJO_SPEVENT_K1_2  ; 告白イベント
LOCAL:1 = 1  ; 記入チェック
IF LOCAL:1 && ARG == 0
  PRINTFORMW ――それは、神社がもうすぐ見えるであろうと思われた時のことだ。
  ...
ENDIF
IF LOCAL:1 && ARG == 1
  PRINTW
  PRINTFORMW 「――本当に？」
  ...
ENDIF
```

### 5.4 COUNTER自動発動パターン

**用途**: チラ見、距離感など

```erb
@M_KOJO_MESSAGE_COUNTER_K1_2_1
IF TALENT:恋慕
  PRINTDATA
    DATALIST
      DATAFORM 霊夢はちらちらとこちらを見ている。
      DATAFORM %CALLNAME:MASTER%が霊夢と目を合わせると、
      DATAFORM 「……えへへ……」
    ENDLIST
  ENDDATA
ELSE
  PRINTDATA
    DATALIST
      DATAFORM 霊夢はちらちらと...
    ENDLIST
  ENDDATA
ENDIF
PRINTFORMW
RETURN 1
```

---

## 6. 参考になる設計パターン

### 6.1 愛情度レイヤリング

eraTWの設計では、各コマンド・イベントについて**統一した4段階愛情度分岐**を採用：

```
恋人 (最高) > 恋慕 (高) > 思慕 (中) > その他 (低)
```

これにより：
- キャラの感情状態が一貫性を保つ
- 新しいコマンド追加時のテンプレが明確
- プレイヤーの選択が直感的

### 6.2 条件フラグの活用

複数の動作状態を同時に考慮：

```
FLAG:時間停止             (グローバル: 時間停止中)
CFLAG:キャラ:添い寝中      (個別: キャラが寝ている)
CFLAG:キャラ:デート中      (個別: 現在デート中)
FLAG:なりきり             (グローバル: なりきりモード)
TALENT:キャラ:妊娠         (個別: 妊娠状態)
```

### 6.3 PRINTDATA + DATALISTの活用

ランダム選択を含むバリエーション：

```erb
PRINTDATA
  DATALIST
    DATAFORM 「パターンA」
  ENDLIST
  DATALIST
    DATAFORM 「パターンB」
  ENDLIST
ENDDATA
WAIT
```

WAITにより、ランダムに1つが選択される。

### 6.4 共通口上へのフォールバック

キャラ個別の口上がない場合：

```erb
CALL KOJO_ACTIVE_INFO(TARGET)
IF !RESULT
  TRYCALLFORM M_KOJO_EVENT_{ARG:1}(ARG:3,ARG:4)
  TARGET = LOCAL
  RETURN 0
ENDIF
```

グレースフルな降格処理でゲーム進行を保証。

---

## 7. COM以外のイベント系口上サマリー

| カテゴリ | 呼び出し頻度 | 分岐数 | 実装難度 | 拡張性 |
|---------|:----------:|:-----:|:------:|:------:|
| ENCOUNTER | 低（初回のみ） | 1-2 | 低 | 低 |
| SP_EVENT | 低（メインシーン） | 2-4 | 中 | 中 |
| EVENT | 中（各シーン） | 4+ | 低 | 高 |
| COMMAND (COM) | 高（毎ターン） | 4-7 | 中 | 高 |
| COUNTER | 高（自動） | 2-4 | 低 | 中 |

---

## 8. 実装上の注意点

### 8.1 ファイル名規則

```
M_KOJO_K{キャラNo}_{カテゴリ}.ERB
M_KOJO_K1_コマンド.ERB    (COM系)
M_KOJO_K1_イベント.ERB    (EVENT/SP_EVENT/ENCOUNTER)
M_KOJO_K1_COUNTER.ERB     (COUNTER)
```

### 8.2 ラベル命名規則

```
@M_KOJO_MESSAGE_COM_K{No}_{ID}
  COM系: 300-400番台など
  数値ID: エディタ内で定義

@M_KOJO_MESSAGE_COUNTER_K{No}_{ID}
  COUNTER: 1-20番台程度

@M_KOJO_SPEVENT_K{No}_{ID}
  SP_EVENT: 1, 2等の主要イベント
```

### 8.3 マジックナンバー

- `CFLAG:1:350`: 掃除状態フラグ（304コマンドで参照）
- `TFLAG:193`: 怒られ度（303謝るで使用）
- `TFLAG:50`: カウンターコマンド判定
- 各キャラ独自のCFLAG領域あり

### 8.4 記述ガイドライン

各ラベルの先頭に**記入チェックコメント**：

```erb
;-------------------------------------------------
;記入チェック（=0, 非表示、1, 表示）
LOCAL = 1
;-------------------------------------------------
IF LOCAL
  ; 実装コード
ENDIF
```

LOCAL = 0で一括無効化可能。

---

## 結論

eraTWの口上システムは、**5つの層状イベント体系**により柔軟性を確保しつつ、各レイヤーで一貫した愛情度分岐を採用している。

COM以外のイベント系（ENCOUNTER, SP_EVENT, EVENT, COUNTER）は：
- **ENCOUNTER**: 初回フラグベース
- **SP_EVENT**: メインストーリー向けの複数分岐
- **EVENT**: 最も拡張性高い汎用フレームワーク
- **COUNTER**: 自動発動による世界観表現

各カテゴリは独立しながらも、フレームワーク側で統一的に管理され、共通口上へのフォールバックで堅牢性を確保している設計。
