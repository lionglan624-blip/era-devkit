# Feature 011: Debug Entry to Training Mode

## Status: [DONE]

> **Completed**: 2025-12-11 - 2段階デバッグコマンド（888+889）実装。889でTEQUIP設定追加。挿入状態表示[V:ペニス][A:ペニス]のヘッドレステスト確認済み。回帰テスト9/9 PASS。

## Overview

Headless テストでうふふモード（調教ループ）に直接入るためのデバッグコマンドを追加。Feature 009（挿入状態表示）等の調教中UI機能をテスト可能にする。

## Problem

現状の課題:
- `BEGIN TRAIN` はERBシステムコマンドで、State Injection では制御できない
- `BEGIN TRAIN` は `BEFORETRAIN`（日替わり処理）を実行し、State Injectionで設定した状態がリセットされる
- 調教モードに入るには長い入力シーケンスが必要
- Headless テストで調教中の機能検証が困難

## Goals

1. **2段階デバッグコマンド**: BEFORETRAIN後にうふふモードを設定
2. **自動状態設定**: 同室キャラをTARGETに自動設定、CFLAG:うふふ を設定
3. **テストフロー簡素化**: State Injection + 2コマンドで調教画面へ

## Technical Design

### コマンド888: TRAINループ開始（SHOP）

```erb
; SHOP.ERB - 隠しコマンド
ELSEIF RESULT == 888
    CALL DEBUG_ENTER_UFUFU
    RETURN 1

@DEBUG_ENTER_UFUFU
    ; DEBUG用: SHOPからTRAINループを開始
    ; うふふモード設定はTRAINループ内でコマンド889を使用
    BEGIN TRAIN
```

### コマンド889: うふふモード設定 + 挿入パターンサイクル（TRAIN内）

コマンド889は呼び出すたびに異なる挿入パターンを設定する：

| Call # | Pattern | V | A |
|--------|---------|---|---|
| 1 | 0 | ペニス | ペニス |
| 2 | 1 | バイブ | アナルバイブ |
| 3 | 2 | なし | アナルビーズ |
| 4 | 3 | バイブ | なし |
| 5 | 4 | なし | なし |

```erb
; USERCOM.ERB - 隠しコマンド（TRAINループ内で使用）
ELSEIF RESULT == 889
    ; 同室キャラ検索、いなければ咲夜を呼び寄せる
    ; + 挿入パターンサイクル（TFLAG:889でカウント）
    ...
    LOCAL = TFLAG:889 % 5
    TFLAG:889 += 1
    ; Reset all TEQUIP, then set pattern
    IF LOCAL == 0
        TEQUIP:(TARGET:0):Ｖセックス = 1
        TEQUIP:(TARGET:0):Ａセックス = 1
    ELSEIF LOCAL == 1
        TEQUIP:(TARGET:0):バイブ = 1
        TEQUIP:(TARGET:0):アナルバイブ = 1
    ...
```

### テストフロー

```
1. Headless起動 + セーブロード
   dotnet run ... --load-file base.sav --inject scenario.json

2. State Injection (scenario.json)
   - TEQUIP:キャラ:50 = 1 (Vセックス)
   - TEQUIP:キャラ:51 = 1 (Aセックス)
   - copy: MASTERの位置をキャラにコピー（同室にする）

3. Input: 888
   → BEGIN TRAIN実行
   → BEFORETRAIN（日替わり処理）
   → TRAINループに入る

4. Input: 889
   → 同室キャラをTARGETに設定
   → CFLAG:うふふ = 1
   → うふふモード有効化

5. Input: (任意の調教コマンド)
   → INFO.ERB表示（挿入状態表示含む）
```

## Implementation Location

| File | Change |
|------|--------|
| `ERB/SHOP.ERB` | @USERSHOP に分岐追加（隠しコマンド888） |
| `ERB/SHOP.ERB` | @DEBUG_ENTER_UFUFU 関数（BEGIN TRAINのみ） |
| `ERB/USERCOM.ERB` | @USERCOM に分岐追加（隠しコマンド889） |

## Acceptance Criteria

- [x] コマンド888が隠しコマンドとして動作（メニュー非表示）
- [x] 888入力で調教ループに入る
- [x] コマンド889がTRAINループ内で動作
- [x] 889入力で同室キャラがTARGETに設定される
- [x] 889入力でCFLAG:うふふ が正しく設定される
- [x] 調教中 UI（INFO.ERB）でうふふモード表示確認
- [x] Feature 009（挿入状態表示）のテストが可能

## Test Scenarios

| # | Scenario | Input | Expected |
|---|----------|-------|----------|
| 1 | TRAINループ開始 | 888 | 調教ループに入る |
| 2 | うふふモード設定 | 888→889 | うふふフラグ設定、TARGET設定 |
| 3 | 挿入状態表示 | inject TEQUIP+copy → 888→889→0 | [V:ペニス][A:ペニス]表示 |

## Risks

| Risk | Mitigation |
|------|------------|
| 通常ゲームフローに影響 | 888/889は隠しコマンド、メニュー非表示 |
| BEFORETRAINで状態リセット | 889はBEFORETRAIN後に実行されるため影響なし |

## Links

- [index-features.md](index-features.md) - Feature index
- [feature-009.md](feature-009.md) - 挿入状態表示（本機能でテスト可能に）
- [SHOP.ERB](../ERB/SHOP.ERB) - コマンド888実装
- [USERCOM.ERB](../ERB/USERCOM.ERB) - コマンド889実装
