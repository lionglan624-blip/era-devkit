# Feature 010: State Injection拡張 (TFLAG/TEQUIP/TARGET)

## Status: [DONE]

## Summary

HeadlessモードのState Injection機能を拡張し、TFLAG（調教フラグ）、TEQUIP（調教装備）、TARGET（ターゲットキャラ）の注入をサポートする。これにより、うふふモード（調教モード）の状態を直接設定してテスト可能になる。

## Motivation

Feature 009（挿入状態表示）のテストにおいて、以下の課題が判明：
- うふふモードに入るには複雑なゲーム状態が必要
- TEQUIP（挿入状態）を直接注入できないため、手動でモードに入る必要がある
- Headlessテストで調教モード機能の自動テストが困難
- **追加発見**: TFLAG:COMABLE管理はゲームフロー中にCFLAG:うふふに基づいて再設定される
- **追加発見**: CFLAG:うふふの評価にはTARGET設定が必要

## Requirements

### Must Have
1. TFLAG注入サポート（グローバル調教フラグ）✓
2. TEQUIP注入サポート（キャラ紐付け調教装備）✓
3. TARGET注入サポート（ターゲットキャラインデックス）← NEW
4. 既存のシナリオファイル形式との互換性

### Nice to Have
- TFLAG/TEQUIP/TARGET の読み取り（ReadVariable）サポート

## Technical Design

### 対象変数

| 変数 | 型 | 説明 | 用途例 |
|------|-----|------|--------|
| TFLAG | グローバル1D配列 | 調教セッションフラグ | TFLAG:102=2 (うふふON) |
| TEQUIP | キャラ紐付け1D配列 | 装備・挿入状態 | TEQUIP:50=1 (Vセックス) |
| TARGET | グローバル1D配列 | ターゲットキャラインデックス | TARGET:0=5 (キャラNo.5) |

### 主要なTFLAGインデックス

```
100: 調教中COMABLE管理
102: COMABLE管理 (1=日常ON, 2=ウフフON)
104: 現在のTARGET
```

### 主要なTEQUIPインデックス

```
13: バイブ
14: アナルバイブ
15: アナルビーズ
50: Ｖセックス
51: Ａセックス
```

### 実装箇所

| ファイル | 変更内容 |
|----------|----------|
| VariableResolver.cs | `ResolveTflagIndex()`, `ResolveTequipIndex()` 追加 |
| StateInjector.cs | `SetTflag()`, `SetTequip()`, `GetTflag()`, `GetTequip()` 追加 |
| StateInjector.cs | `SetTarget()`, `GetTarget()` 追加 ← NEW |
| StateInjector.cs | `InjectVariable()`, `ReadVariable()` のswitch拡張 |

### TARGET変数の仕組み

```
TARGET:0 → 現在のターゲットキャラのインデックス（キャラ番号）
奴隷 = TARGET:0  ← ERB内でこのように使用される
CFLAG:奴隷:うふふ == 1 → TFLAG:COMABLE管理 = 2
```

キャラ名からインデックスへの解決は`VariableResolver.ResolveCharacterIndex()`を使用。

### シナリオファイル形式

```json
{
  "name": "ufufu-insertion-test",
  "variables": {
    "TARGET:0": "大妖精"
  },
  "characters": {
    "大妖精": {
      "CFLAG:317": 1,
      "TEQUIP:50": 1,
      "TEQUIP:51": 1
    }
  },
  "copy": [
    {"from": "大妖精", "to": "MASTER", "var": "CFLAG:300"}
  ]
}
```

**注**: `TARGET:0`にキャラ名を指定すると、自動的にキャラインデックスに解決される。

## Estimated Effort

- コード追加: 約60行
- 工数: 低（既存パターンの踏襲）

## Dependencies

- Feature 002 (State Injection) - 基盤

## Success Criteria

1. TFLAG注入が動作する ✓
2. TEQUIP注入が動作する ✓
3. TARGET注入が動作する（キャラ名→インデックス解決含む）
4. Feature 009のテストシナリオで挿入状態表示が確認できる

## Links

- [feature-002.md](feature-002.md) - 元のState Injection
- [feature-009.md](feature-009.md) - 挿入状態表示（テスト対象）
- [index-features.md](index-features.md)
