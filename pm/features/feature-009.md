# Feature 009: 挿入状態表示

## Status: [DONE]

> **Completed**: 2025-12-11 - INVAGINA/INANAL関数とUI実装完了。Feature 011 (コマンド889) でヘッドレステスト確認済み。[V:ペニス][A:ペニス]表示動作確認。

## Overview

V（膣）とA（アナル）に何が挿入されているかを表示する機能を復活させる。オリジナルソースの`INVAGINA`/`INANAL`関数を参考に実装。

## Problem

現状の課題:
- 調教中にV/Aに何が挿入されているか視覚的に確認できない
- プレイヤーが現在の状態を把握しづらい
- オリジナルソースには存在していた機能が欠落

## Goals

1. **INVAGINA関数復活**: V（膣）の挿入状態を返す
2. **INANAL関数復活**: A（アナル）の挿入状態を返す
3. **状態表示UI**: 調教画面でV/A状態を表示

## Technical Analysis

### オリジナル実装（COMMON.ERB）

**@INVAGINA(ARG)** - V（膣）の状態
```erb
@INVAGINA(ARG)
  #FUNCTION
  IF TEQUIP:ARG:Ｖセックス
    RETURNF 1  ; ペニス
  ELSEIF TEQUIP:ARG:バイブ
    RETURNF 2  ; バイブ
  ENDIF
  ; 戻り値 0 = なし
```

**@INANAL(ARG)** - A（アナル）の状態
```erb
@INANAL(ARG)
  #FUNCTION
  IF TEQUIP:ARG:Ａセックス
    RETURNF 1  ; ペニス
  ELSEIF TEQUIP:ARG:アナルバイブ
    RETURNF 2  ; アナルバイブ
  ELSEIF TEQUIP:ARG:アナルビーズ
    RETURNF 3  ; アナルビーズ
  ENDIF
  ; 戻り値 0 = なし
```

### TEQUIP インデックス

| Index | Name | 用途 |
|-------|------|------|
| 13 | バイブ | V挿入判定 |
| 14 | アナルバイブ | A挿入判定 |
| 15 | アナルビーズ | A挿入判定 |
| 50 | Ｖセックス | V挿入判定 |
| 51 | Ａセックス | A挿入判定 |

### 表示形式案

```
[V:ペニス A:バイブ]
[V:なし A:ビーズ]
[V:バイブ A:なし]
```

### 実装箇所

| File | Change |
|------|--------|
| `ERB/COMMON.ERB` | INVAGINA, INANAL関数追加 |
| `ERB/SHOW_STATUS.ERB` or similar | 表示UI追加 |

## Acceptance Criteria

- [x] INVAGINA関数が正常動作（戻り値 0/1/2）
- [x] INANAL関数が正常動作（戻り値 0/1/2/3）
- [x] 調教画面でV/A状態が表示される（INFO.ERB @SHOW_EQUIP_1）
- [x] V挿入テスト（ペニス、バイブ）
- [x] A挿入テスト（ペニス、アナルバイブ、アナルビーズ）
- [x] 複合挿入テスト（V+A同時）
- [x] 回帰テスト通過（9/9 PASS）

## Test Scenarios

| # | Scenario | TEQUIP設定 | Expected |
|---|----------|-----------|----------|
| 1 | V:ペニス | Ｖセックス=1 | INVAGINA=1 |
| 2 | V:バイブ | バイブ=1 | INVAGINA=2 |
| 3 | A:ペニス | Ａセックス=1 | INANAL=1 |
| 4 | A:アナルバイブ | アナルバイブ=1 | INANAL=2 |
| 5 | A:アナルビーズ | アナルビーズ=1 | INANAL=3 |
| 6 | V+A同時 | Ｖセックス=1, Ａセックス=1 | 両方表示 |
| 7 | 何もなし | 全て0 | V:なし A:なし |

## Risks

| Risk | Mitigation |
|------|------------|
| 表示位置の競合 | 既存UIを調査してから実装 |
| TEQUIP仕様変更 | オリジナルのTEQUIP.CSVを参照 |

## Estimated Effort

**Low** - オリジナル実装を参考にERB追加のみ

| Task | Estimate |
|------|----------|
| 関数実装 | Small |
| 表示UI実装 | Small |
| テストシナリオ作成 | Small |
| テスト実行 | Small |
| **Total** | **Low** |

## Headless Testing

### コマンド889 パターンサイクル

コマンド889を複数回呼び出すことで、異なる挿入パターンをテストできる：

| Call # | Pattern | V | A | 表示例 |
|--------|---------|---|---|--------|
| 1 | 0 | ペニス | ペニス | `[V:ペニス][A:ペニス]` |
| 2 | 1 | バイブ | アナルバイブ | `[V:バイブ][A:アナルバイブ]` |
| 3 | 2 | なし | アナルビーズ | `[A:アナルビーズ]` |
| 4 | 3 | バイブ | なし | `[V:バイブ]` |
| 5 | 4 | なし | なし | (表示なし) |

### テストコマンド例

```bash
# Pattern cycling test
dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . \
  --load-file tests/train/regression-base.sav \
  < tests/train/input-test-cycle2.txt 2>&1 \
  | grep -E "(DEBUG 889|挿入状態)"
```

### テスト入力ファイル例 (`tests/train/input-test-cycle2.txt`)

```
100       # 起床
0         # (shop menu)
0         # (shop menu)
0         # (shop menu)
0         # (shop menu)
889       # Pattern 0: V:ペニス, A:ペニス
0         # 愛撫 → 挿入状態表示
0
0
0
0
889       # Pattern 1: V:バイブ, A:アナルバイブ
0         # 愛撫 → 挿入状態表示
...
300       # Exit
```

**Note**: 愛撫(command 0)はWAITがあり、複数の入力を消費する。各889の後に5-10個の0を入れることを推奨。

## Links

- [index-features.md](index-features.md) - Feature index
- [feature-011.md](feature-011.md) - Debug Entry (command 889)
- [originalSource COMMON.ERB](../archive/original-source/ERB/COMMON.ERB) - Original implementation
- [TEQUIP.CSV](../CSV/TEQUIP.CSV) - Equipment indices
