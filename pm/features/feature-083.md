# Feature 083: 起床後の状態注入

## Status: [DONE]

## Background

### 問題

現在のHeadlessテストでは、状態注入（`--set`オプション）が**起床処理の前**に実行される。
起床処理（`BEGIN TRAIN` → `@EVENTTRAIN`）で以下がリセットされるため、注入した状態が無効になる：

- キャラクター移動 (`CHARA_MOVEMENT`関数が30分刻みで起床時刻まで移動)
- 移動ルートリセット (`RESET_ROUTE`)
- 霊夢の位置リセット (`CFLAG:人物_霊夢:現在位置 = CFLAG:人物_霊夢:開始位置`)

### 起床フロー詳細 (ERB調査結果)

```
SYSTEM.ERB (@EVENTFIRST)
    └─ CFLAG:LOCAL:現在位置 = CFLAG:LOCAL:311 (開始位置)
    └─ BEGIN SHOP
           ↓
SHOP.ERB (@SHOW_SHOP)
    └─ [100] 起床 → BEGIN TRAIN
           ↓
BEFORETRAIN.ERB (@EVENTTRAIN)  ← ここでリセットが発生
    └─ Line 26: CALLF RESET_ROUTE(キャラ番号, 1)
    └─ Line 32-37: 催眠救済 (人間の里/民家→正門)
    └─ Line 51-66: CHARA_MOVEMENT (時間経過分の移動)
    └─ Line 79-80: 霊夢位置リセット
           ↓
@EVENTTRAIN 完了 → コマンド入力待ち  ← ここで注入したい
```

### 影響

- 「キャラXが場所Yにいる状態でテスト」ができない
- Headless統合テストで特定シナリオの再現が困難
- kojo-testは直接関数呼び出しなので影響なし

### 解決策

`@EVENTTRAIN`完了後（最初のコマンド入力待ち）に状態を再注入する機能を追加。

---

## Acceptance Criteria

### AC1: `--set-after-wakeup`オプション追加

新しいCLIオプションで起床後に状態を注入する。

**テストコマンド**:
```bash
# 現行（起床前注入 - CHARA_MOVEMENTでリセットされる可能性）
dotnet run ... --set "CFLAG:4:現在位置=5"

# 新規（起床後注入 - 維持される）
dotnet run ... --set-after-wakeup "CFLAG:4:現在位置=5"
```

**検証シナリオ**:
| Mode | Input | Expected | Status | Evidence |
|------|-------|----------|:------:|----------|
| Headless | `--set-after-wakeup "CFLAG:4:現在位置=5" --dump-vars "CFLAG:4:現在位置"` | output contains: `[Headless] Post-wakeup set:` + dump contains: `"CFLAG:4:現在位置":5` | [x] | `[Headless] Post-wakeup set: CFLAG:4:現在位置=5` 出力 |
| Headless | 通常の`--set "FLAG:0=999"` | output NOT contains: `Post-wakeup` + dump contains: `"FLAG:0":999` | [x] | Post-wakeupログなし + dump正常 |

### AC2: 起床完了検出

`@EVENTTRAIN`関数終了を検出して状態注入をトリガーする。

**検出条件**:
- `BEGIN TRAIN` 発行後の `@EVENTTRAIN` 完了時点
- 具体的には: `EraProcess.CurrentState == ProgramState.Normal` && 直前に `@EVENTTRAIN` が実行された

**検証シナリオ**:
| Mode | Scenario | Expected | Status | Evidence |
|------|----------|----------|:------:|----------|
| Headless | ニューゲーム → [9]クイックスタート → [888]TRAIN | output contains: `[Headless] Post-wakeup injection: applying` | [x] | `Train_CallEventTrain -> Train_WaitInput` 遷移で注入実行確認 |
| Headless | セーブロード → [100]起床 | output contains: `[Headless] Post-wakeup injection: applying` | [x] | `[Headless] Post-wakeup injection: applying 1 variable(s)` 出力確認 |
| Headless | ニューゲーム → SHOP止まり | output NOT contains: `Post-wakeup injection: applying` | [x] | `queued`のみ、注入実行なし確認 |

### AC3: Interactive モード対応

Interactiveモードでも起床後注入をサポート。

**テストシナリオ**:
```json
{"cmd": "set-after-wakeup", "var": "CFLAG:4:現在位置", "value": 5}
// (起床完了まで待機)
// Response:
{"status": "ok", "var": "CFLAG:4:現在位置", "value": 5}
```

**検証シナリオ**:
| Mode | Input | Expected | Status | Evidence |
|------|-------|----------|:------:|----------|
| Interactive | `{"cmd": "set-after-wakeup", "var": "CFLAG:4:300", "value": 5}` | `{"status": "ok"}` かつ `Post-wakeup injection queued` | [x] | `{"status":"ok"}` + `queued: 1 variable(s)` 確認 |

### AC4: ドキュメント更新

**検証シナリオ**:
| Item | Check | Status | Evidence |
|------|-------|:------:|----------|
| testing-reference.md | Section 4.2.1に`--set-after-wakeup`コマンド例とcontains: `Post-Wakeup Injection` | [x] | Section 4.2.1 (L173-211) 存在確認 |
| testing-reference.md | 起床フロー図に`★ --set-after-wakeup 注入 ★`の記載 | [x] | フロー図 (L190-205) に記載確認 |

---

## Implementation Notes

### 既存コード参照

- `HeadlessRunner.cs` L199-206: 現行の状態注入処理
- `ScenarioParser.cs`: 遅延注入パターン (`HasPendingCharacterInjections`)
- `BEFORETRAIN.ERB`: 起床処理 (`@EVENTTRAIN`)

### 実装方針

1. `HeadlessOptions`に`SetAfterWakeup`プロパティ追加
2. `@EVENTTRAIN`完了検出フック実装
   - `Process.CurrentFunction`が`EVENTTRAIN`で、状態が`Normal`に戻った時点
3. 検出時に`StateInjector.InjectVariable()`を呼び出し

### エッジケース

- クイックスタート未使用時は`@EVENTTRAIN`が呼ばれるタイミングが異なる
- セーブロード時も`BEGIN TRAIN`は発生する

---

## Effort Estimate

**Medium** - 起床完了検出のタイミング制御が必要

---

## Links

- [feature-078.md](feature-078.md) - Headless State Injection修正（関連）
- [testing-reference.md](reference/testing-reference.md) - テスト戦略
