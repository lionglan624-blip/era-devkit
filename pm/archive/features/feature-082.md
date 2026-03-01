# Feature 082: Headless Test Mode Clarification

## Status: [DONE]

## Background

### 問題の発見

Feature 079-081でdebugモード(旧Interactive)を実装したが、現状の仕様は以下の問題がある：
1. unitモード(旧kojo-test)と機能的に重複しており、差別化ができていない
2. debugモードの本来の目的（リアルタイムデバッグ）から外れている
3. モード名が歴史的経緯に基づいており、用途がわかりにくい → **ドキュメント更新で解決**

### 元々の想定

**debugモード**の本来の目的：
- flow等でバグが起きたとき、毎回テストコードを書き直すのが大変
- 標準入出力を使ってClaudeとuEmueraが直接会話する
- リアルタイムでデバッグ・探索的テスト
- GUIに近い挙動

### モード定義（ドキュメント更新で新名称に統一）

| 新名 | 旧名 | 用途 |
|------|------|------|
| **unit** | kojo-test | 口上単体テスト、CI向けバッチテスト |
| **flow** | Headless | 統合テスト、状態注入 → ゲームコマンド実行 |
| **debug** | Interactive | デバッグ・探索、動的調査、stdin/stdout対話 |

### 重要な原則

**ERB解析が基本**：
- ナビゲーション作成時は、まずERBを解析して構造を理解する
- debugモードは**ERB解析だけでは不明な場合の補助手段**
- 先にERBを参照することを強く推奨

---

## Acceptance Criteria

### AC1: 口上期待値の事前定義

全キャラ(1-10) + 汎用 × 4口上の期待値：

#### 思慕獲得 (`KOJO_MESSAGE_思慕獲得_KU`)

| NO | キャラ | 期待値 |
|:--:|--------|--------|
| 1 | 美鈴 | `最近一緒にいると` |
| 2 | 小悪魔 | `ちょっと気になって` |
| 3 | パチュリー | `安らぎを感じる` |
| 4 | 咲夜 | `傍にいると心が安らぐ` |
| 5 | レミリア | `退屈しないわね` |
| 6 | フラン | `もっと一緒にいたい` |
| 7 | 妖精メイド | `一緒にいると楽しい` |
| 8 | チルノ | `なんか楽しいわね` |
| 9 | 大妖精 | `なんだか嬉しい` |
| 10 | 魔理沙 | `調子いいぜ` |
| - | 汎用 | `嬉しそうな表情` |

#### 恋慕獲得 (`KOJO_MESSAGE_恋慕獲得_KU`)

| NO | キャラ | 期待値 |
|:--:|--------|--------|
| 1 | 美鈴 | `好きなんだと思います` |
| 2 | 小悪魔 | `悪魔なのに` |
| 3 | パチュリー | `胸の高鳴り` |
| 4 | 咲夜 | `お仕えすることが` |
| 5 | レミリア | `私のものよ` |
| 6 | フラン | `大好き` |
| 7 | 妖精メイド | `お傍にいても` |
| 8 | チルノ | `一番に思ってる` |
| 9 | 大妖精 | `大切に思ってる` |
| 10 | 魔理沙 | `特別な気がする` |
| - | 汎用 | `特別な感情` |

#### 告白成功 (`KOJO_MESSAGE_告白成功_KU`)

| NO | キャラ | 期待値 |
|:--:|--------|--------|
| 1 | 美鈴 | `ずっと傍に` |
| 2 | 小悪魔 | `悪魔と契約` |
| 3 | パチュリー | `共に歩む` |
| 4 | 咲夜 | `全ては、あなたのもの` |
| 5 | レミリア | `伴侶として` |
| 6 | フラン | `やったー` |
| 7 | 妖精メイド | `私なんかでいいの` |
| 8 | チルノ | `最強のコンビ` |
| 9 | 大妖精 | `とっても嬉しい` |
| 10 | 魔理沙 | `嫌いじゃないぜ` |
| - | 汎用 | `幸せそうに微笑み` |

#### 告白失敗 (`KOJO_MESSAGE_告白失敗_KU`)

| NO | キャラ | 期待値 |
|:--:|--------|--------|
| 1 | 美鈴 | `ごめんなさい` |
| 2 | 小悪魔 | `急すぎます` |
| 3 | パチュリー | `そういう気分ではない` |
| 4 | 咲夜 | `申し訳ありません` |
| 5 | レミリア | `まだ足りない` |
| 6 | フラン | `そういう気分じゃない` |
| 7 | 妖精メイド | `ごめんなさい` |
| 8 | チルノ | `そんな気分じゃない` |
| 9 | 大妖精 | `ごめんね` |
| 10 | 魔理沙 | `まだ早いぜ` |
| - | 汎用 | `申し訳なさそうに` |

**汎用(KU)関数**: `KOJO_MESSAGE_*_KU(奴隷)` でキャラ別分岐（SELECTCASE NO:奴隷）

---

### AC2: unit単体モードで--mock-rand対応

**問題**: `--mock-rand`オプション使用時、HeadlessRunner.csがバッチモードを強制する

**修正箇所**: `uEmuera/uEmuera.Headless/HeadlessRunner.cs:960`

```csharp
// 現在（--mock-randでバッチモード強制）
var isBatchMode = testPath.EndsWith(".json") ||
                  Directory.Exists(testPath) ||
                  options_.Parallel ||
                  !string.IsNullOrEmpty(options_.Preset) ||
                  options_.MockRand.Count > 0;  // ← 削除

// 修正後
var isBatchMode = testPath.EndsWith(".json") ||
                  Directory.Exists(testPath) ||
                  options_.Parallel ||
                  !string.IsNullOrEmpty(options_.Preset);
```

**検証**:
- [x] 単体モード: `dotnet run ... --unit KOJO_MESSAGE_思慕獲得_KU --char 1 --mock-rand 0`
  - Expected: 直接テキスト出力（JSON結果ではない）✅
- [x] バッチモード: `dotnet run ... --unit k4-basic.json --mock-rand 0`
  - Expected: サマリー形式の結果 ✅

---

### AC3: unit（単体テスト）全キャラ+汎用 4口上パラレル実行 ✅

| キャラ | 思慕獲得 | 恋慕獲得 | 告白成功 | 告白失敗 |
|:------:|:--------:|:--------:|:--------:|:--------:|
| 1 美鈴 | [x] | [x] | [x] | [x] |
| 2 小悪魔 | [x] | [x] | [x] | [x] |
| 3 パチュリー | [x] | [x] | [x] | [x] |
| 4 咲夜 | [x] | [x] | [x] | [x] |
| 5 レミリア | [x] | [x] | [x] | [x] |
| 6 フラン | [x] | [x] | [x] | [x] |
| 7 妖精メイド | [x] | [x] | [x] | [x] |
| 8 チルノ | [x] | [x] | [x] | [x] |
| 9 大妖精 | [x] | [x] | [x] | [x] |
| 10 魔理沙 | [x] | [x] | [x] | [x] |
| 99 汎用 | [x] | [x] | [x] | [x] |

**合計**: 11キャラ × 4関数 = **44テスト**

**汎用テスト用ダミーキャラ**: `CSV/Chara99.csv` (NO=99, 名前=テストダミー) を作成。
SELECTCASE NO:奴隷 の CASEELSE分岐をテストするため。

```bash
# バッチモード実行結果 (2025-12-16)
dotnet run ... --unit tests/k4-comprehensive-44.json --parallel 4
# ==> 44/44 passed (67.93s)
```

---

### AC4: flow（統合テスト）- 設計変更 ✅

**注記**: 関係性口上（思慕獲得、恋慕獲得、告白成功、告白失敗）は**イベントトリガー型**であり、
EVENTTURNENDから呼び出される。コマンド352（告白）を実行しても直接発動しない。

**判定**: AC3のunitで直接関数テストが完了しているため、AC4は以下の理由で**N/A**とする：
- 4関数はイベント駆動（ターン終了時の状態チェック）
- コマンドフロー経由でのテストは不可能
- unit（AC3）で十分なカバレッジを達成

**将来の統合テスト対象**（本Featureスコープ外）:
- コマンド実行型口上（例：COM_K{N}_463_0）
- 状態注入 → コマンド実行 → 口上確認のフロー

---

### AC5: debug（デバッグ）動作確認

**目的**: Claudeがstdinでリアルタイム対話し、動的にデバッグできることを確認

**検証手順**:
```
1. 起動: dotnet run ... --interactive
2. 状態設定: set CFLAG:0 1
3. 確認: dump CFLAG:0 → Expected: "1"
4. 関数呼出: call KOJO_MESSAGE_思慕獲得_KU 1
5. 出力確認: output contains "最近一緒にいると"
6. リセット: reset
7. 状態確認: dump CFLAG:0 → Expected: "0"
```

**検証項目**:
- [x] `set` コマンドで変数設定 → `dump` で値確認 ✅ (FLAG:100=12345)
- [x] `call` コマンドで関数呼出 → 期待出力確認 ✅ (「最近一緒にいると」出力)
- [x] `reset` コマンドで状態リセット ✅ (status: ok)
- [x] 代表キャラ（美鈴）で思慕獲得口上動作確認 ✅

---

### AC6: エラーハンドリング・追加テスト ✅

**エラーケース**:
- [x] 不正関数名: `call INVALID_FUNC` → "Function not found" ✅
- [x] 不正パラメータ: 引数なし関数呼び出しはデフォルト動作（エラーなし）✅
- [x] 不正変数: `dump INVALID:VAR` → null返却 ✅

**追加検証**:
- [x] セッション内キャラ切替: setup char1 → call → setup char2 → call ✅ (InteractiveRunner.cs修正で解決)
- [x] set/dump整合性: `set FLAG:999 123` → `dump FLAG:999` → 123 ✅

---

### AC7: ドキュメント整備

**testing-reference.mdに追記する内容**:

```markdown
## Test Mode Comparison

| Mode | CLI Option | 用途 | 実行形式 |
|------|------------|------|----------|
| **unit** | `--unit` | 口上単体テスト | バッチ/CI |
| **flow** | (default) | 統合テスト | バッチ/CI |
| **debug** | `--interactive` | デバッグ | 対話式(stdin/stdout) |

### ERB解析優先の原則

**重要**: ナビゲーション作成時は、まずERBを解析して構造を理解する。
debugモードは**ERB解析だけでは不明な場合の補助手段**。

### 各モードの推奨フロー

1. **新規口上追加**: ERB作成 → unit単体 → flow統合
2. **バグ調査**: unit失敗 → debug調査 → 修正 → 再テスト
3. **ナビゲーション作成**: ERB解析 → (不明点あれば)debug確認
```

**検証項目**:
- [x] 「Test Mode Comparison」セクションをtesting-reference.mdに追記 ✅
- [x] 「ERB解析優先の原則」セクションをtesting-reference.mdに追記 ✅
- [x] 「各モードの推奨フロー」セクションをtesting-reference.mdに追記 ✅
- [x] 既存の「kojo-test」説明と矛盾がないことを確認 ✅
- [x] 既存の「Headless」説明と矛盾がないことを確認 ✅

---

## Test Target Functions

| カテゴリ | 関数名 | パラメータ |
|----------|--------|-----------|
| 関係性 | `KOJO_MESSAGE_思慕獲得_KU` | `(奴隷)` |
| 関係性 | `KOJO_MESSAGE_恋慕獲得_KU` | `(奴隷)` |
| 関係性 | `KOJO_MESSAGE_告白成功_KU` | `(奴隷)` |
| 関係性 | `KOJO_MESSAGE_告白失敗_KU` | `(奴隷)` |
| 日常 | `KOJO_MESSAGE_COM_K{N}_463_0` または `COM_KU_463_0` | `(奴隷)` |

---

## Implementation Notes

### モード名変更（ドキュメント更新で完了）

| 旧名 | 新名 |
|------|------|
| kojo-test | **unit** |
| Headless | **flow** |
| Interactive | **debug** |

### debugモードの使用場面

1. **unitが失敗したとき**: 関数呼び出し前後の状態を動的に確認
2. **flowが失敗したとき**: ゲームフローのどこで問題が起きたか追跡
3. **ナビゲーション作成**: ERB解析だけではわからない「どのコマンドでどの口上が出るか」を実機確認

---

## Links

- [feature-079.md](feature-079.md) - debug Mode実装
- [feature-081.md](feature-081.md) - debug Mode検証
- [testing-reference.md](reference/testing-reference.md) - テスト戦略
- [KOJO_KU_関係性.ERB](../ERB/口上/U_汎用/KOJO_KU_関係性.ERB) - 関係性口上ソース
