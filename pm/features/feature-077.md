# Feature 077: 関係性コマンド

## Status: [DONE]

## Background

- **Original problem**: Phase 8a完了後の次ステップとして、キャラクターとの関係性を深めるコマンド群が必要
- **Considered alternatives**:
  - ❌ Phase 8b (コンテンツ実装) を先に進める - 関係性コマンドがないとTALENT分岐の検証が困難
  - ❌ 思慕と恋慕を独立した状態として管理 - eraTW同様の段階制が自然
  - ✅ eraTW同様の段階制（思慕→恋慕→恋人）を採用 - ゲームバランスと整合性確保
- **Key decisions**:
  - eraTW同様の3段階遷移を採用: `なし → 思慕 → 恋慕 → 恋人`
  - 思慕/恋慕は1日の終わりに自動付与（条件を満たした場合）
  - 告白コマンド(COM352)で恋慕→恋人への遷移を確定
  - 逆告白はPhase 1では実装しない（将来拡張）
- **Constraints**: eraTWの設計思想を参考にするが、コードは独自実装

## Overview

プレイヤーとキャラクター間の関係性を段階的に深めるシステムを実装する。

### 状態遷移図（eraTW準拠）

```
┌─────────────────────────────────────────────────────────┐
│                    関係性状態遷移                        │
├─────────────────────────────────────────────────────────┤
│                                                         │
│   なし ─────→ 思慕 ─────→ 恋慕 ─────→ 恋人            │
│         (自動)      (自動)      (告白)                  │
│                                                         │
│   条件:        条件:        条件:                       │
│   好感度≥500   好感度≥1000  恋慕状態                    │
│   親密≥3       従順≥3       告白コマンド成功            │
│                奉仕経験≥30                              │
│                思慕状態                                 │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### TALENT定義（既存）

| ID | 名前 | 説明 |
|:--:|------|------|
| 17 | 思慕 | 好意的な感情を抱いている状態 |
| 3 | 恋慕 | 愛情に似た感情を抱いている状態 |
| 16 | 恋人 | 最高の親愛度。恋人関係 |

## Technical Design

### 1. 思慕獲得（自動・1日終了時）

**実装場所**: `EVENTTURNEND.ERB`

**新規関数**: `@CHK_ADMIRATION_GET(奴隷)`

```erb
;-------------------------------------------------------------------------------
;思慕獲得判定
@CHK_ADMIRATION_GET(奴隷)
    #DIM 奴隷

    ;すでに思慕/恋慕/恋人を持っていたらスキップ
    IF TALENT:奴隷:思慕 || TALENT:奴隷:恋慕 || TALENT:奴隷:恋人
        RETURN 0
    ENDIF

    ;条件: 好感度≥500, 親密≥3
    IF CFLAG:奴隷:好感度 >= 500 && ABL:奴隷:親密 >= 3
        PRINTFORML %CALLNAME:奴隷%は%CALLNAME:MASTER%に好意を抱き始めたようだ…
        PRINTFORMW %CALLNAME:奴隷%は[思慕]を得た
        TALENT:奴隷:思慕 = 1
        ;思慕獲得口上（キャラ別）
        TRYCALLFORM KOJO_MESSAGE_思慕獲得_K{奴隷}
    ENDIF
RETURN 0
```

**呼び出し位置**: `@EVENTTURNEND` のFORループ内、`CHK_FALL_IN_LOVE`の前

### 2. 恋慕獲得（自動・1日終了時）

**実装場所**: `EVENTTURNEND.ERB` (既存の`@CHK_FALL_IN_LOVE`を修正)

**変更点**: 思慕を前提条件に追加

```erb
;-------------------------------------------------------------------------------
;恋慕獲得判定
@CHK_FALL_IN_LOVE(奴隷)
    #DIM 奴隷
    #DIM 閾値

    ;★追加: 思慕がなければスキップ（段階制）
    IF !TALENT:奴隷:思慕
        RETURN 0
    ENDIF

    ;恋慕獲得閾値（既存ロジック維持）
    閾値 = (1000+(500*TALENT:奴隷:淫乱)+(2000*TALENT:MASTER:肉便器))*(100+EXP:奴隷:NTR陥落経験*5)/100
    IF TALENT:奴隷:浮気癖
        閾値 /= 3
    ENDIF

    ;恋慕獲得条件（既存）
    IF CFLAG:奴隷:好感度 > 閾値 && EXP:奴隷:奉仕快楽経験 >= 30 && ABL:奴隷:従順 >= 3 && !TALENT:奴隷:恋慕
        ;★追加: 思慕→恋慕への昇格メッセージ
        PRINTFORML %CALLNAME:奴隷%の%CALLNAME:MASTER%への想いは、好意から恋心へと変わったようだ…
        PRINTFORML %CALLNAME:奴隷%は[思慕]を失い、[恋慕]を得た
        TALENT:奴隷:思慕 = 0  ;★追加: 思慕をクリア
        TALENT:奴隷:恋慕 = 1
        ;恋慕獲得口上（キャラ別）
        TRYCALLFORM KOJO_MESSAGE_恋慕獲得_K{奴隷}
        ;（以下、既存のNTR解除処理等は維持）
        ...
    ENDIF
RETURN 0
```

### 3. 告白コマンド（プレイヤー操作）

**実装場所**: `COMF352.ERB` (既存を大幅修正)

#### 3.1 告白可否判定（新規追加）

```erb
;-------------------------------------------------------------------------------
;告白コマンド可否判定
@COM_ABLE352
    ;恋慕状態でなければ実行不可
    IF !TALENT:TARGET:恋慕
        RETURN 0
    ENDIF
    ;すでに恋人なら実行不可
    IF TALENT:TARGET:恋人
        RETURN 0
    ENDIF
    ;二人きりでなければ実行不可（eraTW準拠）
    IF GET_TARGETNUM() > 1
        RETURN 0
    ENDIF
    RETURN 1
```

#### 3.2 告白コマンド本体（修正）

```erb
;-------------------------------------------------------------------------------
;告白する
@COM352
    ;成功判定（既存ロジックベース）
    LOCAL = MARK:屈服刻印 * 3 + MARK:快楽刻印 * 3 + MIN(ABL:TARGET:親密 * 10, 50) + MIN(ABL:TARGET:欲望 * 10, 50) + GETPALAMLV(PALAM:欲情,5) * 5 + GETPALAMLV(PALAM:好意,5) * 5 + BASE:ムード / 50 + (1000 - BASE:理性) / 30

    IF LOCAL > 100 + TALENT:TARGET:一線越えない * 20
        TFLAG:コマンド成功度 = 成功度_成功
    ELSE
        TFLAG:コマンド成功度 = 成功度_失敗
    ENDIF

    IF TFLAG:コマンド成功度 == 成功度_成功
        ;★恋人状態を設定
        TALENT:TARGET:恋人 = 1
        TALENT:TARGET:恋慕 = 0  ;恋慕をクリア（恋人に昇格）
        SETBIT CFLAG:TARGET:既成事実, 2  ;告白成功フラグ

        ;告白成功口上
        TRYCALLFORM KOJO_MESSAGE_告白成功_K{NO:TARGET}

        PRINTFORML %CALLNAME:TARGET%と恋人になりました

        ;SOURCE設定（既存）
        SOURCE:歓楽 = 1000
        SOURCE:受動 = 500 + 100 * ABL:TARGET:従順
        SOURCE:征服 = 500 + 100 * ABL:TARGET:サドっ気
    ELSE
        ;告白失敗口上
        TRYCALLFORM KOJO_MESSAGE_告白失敗_K{NO:TARGET}

        PRINTFORML %CALLNAME:TARGET%に告白を断られてしまった…
    ENDIF
RETURN 1
```

### 4. 必要な口上関数

| 関数名 | トリガー | 説明 |
|--------|----------|------|
| `KOJO_MESSAGE_思慕獲得_K{N}` | 思慕獲得時 | キャラ別の思慕獲得メッセージ |
| `KOJO_MESSAGE_恋慕獲得_K{N}` | 恋慕獲得時 | キャラ別の恋慕獲得メッセージ |
| `KOJO_MESSAGE_告白成功_K{N}` | 告白成功時 | キャラ別の告白成功メッセージ |
| `KOJO_MESSAGE_告白失敗_K{N}` | 告白失敗時 | キャラ別の告白失敗メッセージ |

### 5. 閾値定数（将来的にDIM.ERHへ移動検討）

| 定数名 | 値 | 説明 |
|--------|:--:|------|
| 思慕獲得_好感度閾値 | 500 | 思慕獲得に必要な好感度 |
| 思慕獲得_親密閾値 | 3 | 思慕獲得に必要な親密レベル |
| 恋慕獲得_好感度閾値 | 1000 | 恋慕獲得の基本好感度閾値 |
| 恋慕獲得_従順閾値 | 3 | 恋慕獲得に必要な従順レベル |
| 恋慕獲得_奉仕経験閾値 | 30 | 恋慕獲得に必要な奉仕快楽経験 |

## Implementation Phases

### Phase 1: Core Mechanics (このFeature)

1. 思慕獲得の自動判定実装
2. 恋慕獲得の前提条件修正（思慕必須化）
3. 告白コマンドの完成（恋人状態確定）
4. 基本口上の作成（汎用テンプレート）
5. kojo-testでの検証

### Phase 2: Content (将来Feature)

- 全キャラ別の思慕/恋慕/告白口上作成
- TALENT:思慕/恋慕/恋人 分岐の既存口上への反映

### Phase 3: Advanced (将来Feature)

- 逆告白イベントの実装
- デートシステムとの連携

## Goals

1. 思慕→恋慕→恋人の段階的遷移システムの実装
2. 告白コマンド(COM352)の完成
3. 基本的な口上テンプレートの作成
4. kojo-testでの関係性遷移の自動検証

## Acceptance Criteria

### 機能実装
- [x] 思慕が1日終了時に自動付与される（条件: 好感度≥500, 親密≥3）
- [x] 恋慕獲得に思慕が前提条件となっている
- [x] 告白コマンド(COM352)で恋慕→恋人の遷移が行われる
- [x] 告白コマンドは恋慕状態でのみ実行可能
- [x] 告白成功/失敗時に適切な口上が表示される

### データ
- [x] TALENT:思慕(17)が正しく設定/クリアされる
- [x] TALENT:恋慕(3)が正しく設定/クリアされる
- [x] TALENT:恋人(16)が告白成功時に設定される
- [x] セーブ/ロード後も状態が保持される（TALENT変数は既存システムで保存対象）

### テスト
- [x] コードレビューで思慕獲得ロジック確認
- [x] コードレビューで恋慕獲得の段階制（思慕必須）確認
- [x] コードレビューで告白成功時の恋人化確認
- [x] コードレビューで告白失敗ロジック確認
- [x] TALENT変数の変更はコード上で確認済み
- [x] Build succeeds (0 errors)
- [x] Regression tests pass (85/85)
- [x] インタラクティブモードでの動作確認 → Feature 081で全AC検証完了

## Test Scenarios

### Scenario 1: 思慕獲得

```json
{
  "name": "思慕獲得テスト",
  "inject_state": {
    "target": 1,
    "cflag": { "好感度": 600 },
    "abl": { "親密": 4 }
  },
  "commands": ["CALL EVENTTURNEND"],
  "expect": ["思慕"]
}
```

### Scenario 2: 恋慕獲得

```json
{
  "name": "恋慕獲得テスト",
  "inject_state": {
    "target": 1,
    "talent": { "思慕": 1 },
    "cflag": { "好感度": 1500 },
    "abl": { "従順": 4 },
    "exp": { "奉仕快楽経験": 50 }
  },
  "commands": ["CALL EVENTTURNEND"],
  "expect": ["恋慕"]
}
```

### Scenario 3: 告白成功

```json
{
  "name": "告白成功テスト",
  "inject_state": {
    "target": 1,
    "talent": { "恋慕": 1 },
    "cflag": { "好感度": 2000 },
    "abl": { "親密": 5 },
    "mark": { "快楽刻印": 2 }
  },
  "commands": ["352"],
  "expect": ["恋人になりました"]
}
```

## Files to Modify

| File | Change Type | Description |
|------|-------------|-------------|
| `EVENTTURNEND.ERB` | Modify | 思慕獲得判定追加、恋慕獲得条件修正 |
| `COMF352.ERB` | Modify | 告白コマンド完成、COM_ABLE352追加 |
| `口上/EVENT_関係性.ERB` | New | 思慕/恋慕/告白の口上テンプレート |

## Links

- [index-features.md](index-features.md) - Feature tracking
- [content-roadmap.md](content-roadmap.md) - Content/gameplay roadmap
- [reference/kojo-reference.md](reference/kojo-reference.md) - Kojo system reference (TALENT分岐)

## Notes: Interactive Mode Testing Issues

### 遭遇した問題

#### 1. JSON Protocol形式
- ❌ `{"command":"..."}` - 動作しない
- ✅ `{"cmd":"..."}` - 正しい形式
- ❌ `{"cmd":"call","function":"..."}` - 動作しない
- ✅ `{"cmd":"call","func":"..."}` - 正しい形式

#### 2. 変数名指定
- ❌ `CFLAG:TARGET:好感度` - 日本語名では設定不可
- ✅ `CFLAG:TARGET:2` - 数値インデックスが必要
- CSV定義から確認: CFLAG:2=好感度, ABL:9=親密, TALENT:17=思慕

#### 3. キャラクター指定
- `--char 4` オプションは `AddCharacterFromCsvNo(4)` でキャラを追加
- しかし CharacterList への登録インデックスは CSV番号と異なる
- ❌ `CFLAG:4:2` - "Character not found: 4" エラー
- ✅ `CFLAG:TARGET:2` - TARGET経由でアクセス

#### 4. TARGET初期化
- `--char 4` は kojo読み込みとキャラ追加を行うが、TARGET変数は自動設定されない場合あり
- `{"cmd":"set","var":"TARGET","value":4}` は "Invalid variable format" エラー
- TARGET は VariableCode型の特殊変数で、直接setできない

#### 5. ABL設定の問題
- `{"cmd":"set","var":"ABL:TARGET:9","value":4}` → status:ok だが dump すると null
- ABL配列の初期化タイミングの問題の可能性

### 推奨対処法

1. kojo-test モードを使用（より安定）
2. インタラクティブモードでは character name (NAME/CALLNAME) で指定
3. 複雑な状態設定は ERB側でテスト用関数を作成

### 根本原因（2025-12-15確認）

インタラクティブモードのStateInjector.csでキャラクタ変数を設定する際:
1. `set` コマンドは `status:ok` を返すが、実際には値が設定されない場合がある
2. 原因: `VariableResolver.GetCharacter()` がCharacterListのインデックス（登録順）でキャラを検索
3. `--char 4` で追加されたキャラはCharacterList[1]に入るが、"4"や"咲夜"では見つからない
4. FLAG等のグローバル変数は正常に動作する

### 今後の改善案

1. StateInjectorにCsvNo→CharacterListインデックスのマッピングを追加
2. もしくはテスト用にキャラデータを初期化するERB関数を作成

## Reference: eraTW Implementation

調査結果の要約（参照元: `C:\Users\siihe\OneDrive\同人ゲーム\eraTW4.920`）

| 項目 | eraTW | 本実装 |
|------|-------|--------|
| 思慕閾値 | 好感度≥1500, 信頼≥250, 親密≥5 | 好感度≥500, 親密≥3（緩め） |
| 恋慕閾値 | 好感度≥3000, 信頼≥700, 親密≥10 | 既存ロジック維持（好感度≥1000基準） |
| 告白成功条件 | 馴れ合い強度≥2, 合意判定>350 | 既存ロジック維持（刻印/親密/欲望等） |
| 逆告白 | DATE_CMN.ERBでデート終了時 | Phase 3で検討 |
