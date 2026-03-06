# Feature 058: Headless口上テスト - 口上関数の直接実行

## Status: [DONE]

## Background

Feature 057のHeadless修正で起床後コマンド入力は動作するようになったが、口上デバッグには依然として多くの手順が必要。口上関数を直接呼び出してテストできる専用モードが必要。

### 現状の問題点

| 問題 | 詳細 |
|------|------|
| **テスト手順が煩雑** | タイトル→起床→移動→会話と辿る必要あり |
| **入力を事前列挙** | 全コマンドをテキストファイルに書く必要 |
| **状態注入のタイミング** | 起床処理で位置等がリセットされる |
| **テスト時間** | 1テストに数十秒（ゲーム進行待ち） |

### 想定ユースケース

- **都度ユニットテスト**: 口上作成直後に同セッションでテスト実行
- **回帰テスト**: 蓄積したシナリオを実行し出力を確認

## Goals

1. 口上関数を直接呼び出してテストできる
2. 任意の状態（好感度、フラグ等）を設定してから実行
3. テスト時間を大幅短縮（数秒以内）

## Proposed Solution

### CLI使用例

```bash
# 口上関数を直接呼び出し
uEmuera --unit "KOJO_MESSAGE_COM_K4_300" --char "十六夜咲夜"

# 状態を設定して呼び出し
uEmuera --unit "KOJO_MESSAGE_COM_K4_300" \
        --char "十六夜咲夜" \
        --set "CFLAG:2=5000" \
        --set "TALENT:3=1"

# MASTERを明示的に指定（NPC調教など）
uEmuera --unit "KOJO_MESSAGE_COM_K4_300" \
        --char "十六夜咲夜" \
        --master "レミリア"

# 入力キューを指定
uEmuera --unit "KOJO_MESSAGE_COM_K4_300" \
        --char "十六夜咲夜" \
        --inputs "1,2,3"
```

### 出力例

```
=== KOJO Test: KOJO_MESSAGE_COM_K4_300 ===
Character: TARGET=4 (十六夜咲夜)
State: CFLAG:2=5000, TALENT:3=1

--- OUTPUT ---
何について話そうか……？
[ 1]-他愛も無い雑談
[ 2]-新しいメイドを増やしませんか？
--- END ---

Status: OK
Duration: 0.12s
```

## Scope

### In Scope

| 機能 | 説明 |
|------|------|
| `--unit <func>` | ERB関数を直接実行（ゲームループなし） |
| `--char <name>` | TARGET自動設定 |
| `--master <name>` | MASTER設定（省略時はPLAYER=0） |
| `--set <var>=<val>` | 任意の変数を設定 |
| `--inputs <list>` | 入力キュー（カンマ区切り） |
| WAIT/TWAIT スキップ | 自動スキップ |
| FORCEWAIT スキップ | スキップ+警告 |
| タイムアウト | 30秒デフォルト、`--timeout`で変更可 |
| exit code | 0=成功、1=失敗 |

### Out of Scope (後続Feature)

- シナリオファイル（JSON）読み込み → Feature 059
- expect自動検証 → Feature 059
- バッチテスト → Feature 060
- 分岐トレース → Feature 060
- 乱数制御 → Feature 060
- 複数キャラ設定 → Feature 061
- 対話モード → Feature 061

## Architecture

```
┌─────────────────────────────────────────────────┐
│                  Headless CLI                    │
├─────────────────────────────────────────────────┤
│  --unit <func>    直接関数呼び出し          │
│  --set <var>=<val>     変数設定                  │
│  --char <name>         対象キャラクター          │
│  --master <name>       MASTERキャラクター        │
│  --inputs <list>       入力キュー                │
└─────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────┐
│              KojoTestRunner (新規)               │
├─────────────────────────────────────────────────┤
│  - ゲームループをスキップ                        │
│  - フル初期化（CSV/ERB読み込み）                 │
│  - 変数を直接設定                                │
│  - 指定関数を CALL                               │
│  - PRINTFORM をキャプチャ                        │
└─────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────┐
│              EmueraConsole (既存)                │
│  + InputQueue: 入力キューから自動供給            │
│  + PrintCapture: PRINTFORM 出力をバッファ        │
└─────────────────────────────────────────────────┘
```

### 介入タイミング

**新エントリポイント `RunKojoTest()`**:

```csharp
public class HeadlessRunner
{
    // 既存
    public void Run() { ... }

    // 新規
    public int RunKojoTest(KojoTestConfig config)
    {
        // 1. 初期化（既存と同じ）
        console.Initialize();
        process.Initialize();

        // 2. 状態注入
        StateInjector.Inject(config.State);
        SetupCharacters(config);  // TARGET, MASTER等

        // 3. ゲームループをスキップして直接関数呼び出し
        var label = LabelDictionary.GetNonEventLabel(config.FunctionName);
        if (label == null)
            return Error($"Function not found: {config.FunctionName}");

        // 4. 入力キューをセット
        console.SetInputQueue(config.Inputs);

        // 5. 関数実行
        CalledFunction.CallFunction(process, label, null);
        processState.IntoFunction(calledFunc, args, exm);

        while (processState.HasActiveFunction)
        {
            runScriptProc();
        }

        // 6. 結果出力
        return 0;
    }
}
```

## Technical Investigation Results

### 1. 既存実装の活用可能性

| コンポーネント | ファイル | 活用可能性 |
|---------------|---------|-----------|
| **変数設定API** | `StateInjector.cs` | ✅ 完全実装済み |
| **変数名解決** | `VariableResolver.cs` | ✅ 完全実装済み |
| **出力キャプチャ** | `HeadlessConsole.cs` | ✅ 標準出力へ出力 |
| **キャラクター解決** | `VariableResolver.ResolveCharacterIndex()` | ✅ 実装済み |

### 2. ERB関数の直接呼び出し方法

```
LabelDictionary.GetNonEventLabel(funcName)  // 関数ラベル取得
    ↓
CalledFunction.CallFunction(process, label, null)  // CalledFunction作成
    ↓
ProcessState.IntoFunction(calledFunction, args, exm)  // 関数に入る
    ↓
runScriptProc() // 実行
```

### 3. INPUT/WAIT処理

| 入力命令 | 動作 |
|---------|------|
| `INPUT` (整数) | キューから取得、空なら0 |
| `INPUTS` (文字列) | キューから取得、空なら"" |
| `TINPUT` | タイムアウト値を返す |
| `WAIT` | 自動スキップ |
| `TWAIT` | 自動スキップ |
| `FORCEWAIT` | スキップ+警告 |

### 4. 制御フロー命令の扱い

KojoTestModeフラグで制御:

| 命令 | KojoTestMode時の動作 |
|------|---------------------|
| `GOTO @LABEL` | 同一関数内は許可 |
| `QUIT` | 正常終了（テスト完了扱い） |
| `BEGIN TRAIN/SHOP/...` | 警告+無視 |
| `RESTART` | エラー終了 |
| `SAVEGAME` | 警告+無視 |

### 5. エラーハンドリング

| エラー種別 | 挙動 |
|-----------|------|
| 関数未発見 | 即座に終了、exit 1 |
| 変数名無効 | 警告を出して続行 |
| ERB実行時エラー | スタックトレース出力後終了、exit 1 |
| 無限ループ | タイムアウト（30秒）、exit 1 |
| 入力不足 | デフォルト値使用 + 警告 |

### 6. 起動時間とテスト時間

- 起動時間: 5-10秒（1回のみ、CSV/ERB読み込み）
- テスト時間: 0.1秒/テスト（関数呼び出し）
- フル初期化を受け入れ、並列化で償却

## Implementation Notes

### キャラクター初期化問題（2024-12-14発見・修正）

**問題**: `--char`でキャラクター名を指定しても、CharacterListが空のためキャラクター解決に失敗する。

**原因**: ゲームループを経由しないため、ADDCHARAが呼ばれずCharacterListが空のまま。

**解決策**: KojoTestRunner初期化時にADDDEFCHARAを呼び出してデフォルトキャラクターを登録する。

```csharp
// KojoTestRunner.Run() 内で
// 1. HeadlessWindow.Init() 後にADDDEFCHARA関数を呼び出し
// 2. CharacterListにキャラクターが登録される
// 3. その後でTARGET/MASTER設定が正常に動作
```

## Acceptance Criteria

- [x] `--unit <func>` で口上関数を直接呼び出せる
- [x] `--char <NO>` でTARGETが自動設定される（数値NO指定）
- [x] `--master <NO>` でMASTERが設定される（省略時PLAYER）
- [x] `--set <var>=<val>` で変数を設定できる
- [x] `--inputs <list>` で入力キューを設定できる
- [x] WAIT/TWAITが自動スキップされる
- [x] タイムアウト（30秒）で無限ループを検知できる
- [x] 成功時exit 0、失敗時exit 1が返る

### Known Limitations

- `--char`/`--master`はキャラクター名ではなく数値NOを指定する必要がある
  - 例: `--char "4"` (十六夜咲夜)、`--char "0"` (主人公)
  - 名前解決は後続Featureで対応予定

### AC検証シナリオ

Feature 057で再編成されたK4咲夜口上ファイルの動作確認を兼ねる。

#### Feature 057 ファイルカバレッジ

| ファイル | カテゴリ | テストシナリオ |
|---------|---------|---------------|
| KOJO_K4_会話親密.ERB | COM 300系 | S1, S2, S3 |
| KOJO_K4_愛撫.ERB | COM 0-48 | S4, S5 |
| KOJO_K4_口挿入.ERB | COM 60-144 | S6 |
| KOJO_K4_日常.ERB | COUNTER系 | S7 |
| KOJO_K4_EVENT.ERB | EVENT系 | S8 |

---

#### S1: 会話メニュー（KOJO_K4_会話親密.ERB）
```bash
uEmuera --unit "KOJO_MESSAGE_COM_K4_300" --char "十六夜咲夜"
```
**期待結果**: 出力に「何について話そうか」が含まれる

#### S2: 会話親密・恋慕分岐（KOJO_K4_会話親密.ERB）
```bash
uEmuera --unit "KOJO_MESSAGE_COM_K4_300" --char "十六夜咲夜" \
        --set "CFLAG:2=5000" --set "TALENT:3=1"
```
**期待結果**: 恋慕状態の口上が出力される（選択肢が異なる）

#### S3: 会話選択肢・入力キュー（KOJO_K4_会話親密.ERB）
```bash
uEmuera --unit "KOJO_MESSAGE_COM_K4_300" --char "十六夜咲夜" --inputs "1"
```
**期待結果**: 選択肢1の後続口上が出力される

#### S4: 愛撫コマンド（KOJO_K4_愛撫.ERB）
```bash
uEmuera --unit "KOJO_MESSAGE_COM_K4_0" --char "十六夜咲夜" \
        --set "TFLAG:193=1"
```
**期待結果**: 胸愛撫成功時の口上が出力される

#### S5: 愛撫・絶頂（KOJO_K4_愛撫.ERB）
```bash
uEmuera --unit "KOJO_MESSAGE_COM_K4_40" --char "十六夜咲夜" \
        --set "TFLAG:193=1"
```
**期待結果**: V絶頂時の口上が出力される

#### S6: 口挿入コマンド（KOJO_K4_口挿入.ERB）
```bash
uEmuera --unit "KOJO_MESSAGE_COM_K4_60" --char "十六夜咲夜" \
        --set "TFLAG:193=1"
```
**期待結果**: フェラ時の口上が出力される

#### S7: 日常カウンター（KOJO_K4_日常.ERB）
```bash
uEmuera --unit "KOJO_MESSAGE_COUNTER_K4_10" --char "十六夜咲夜"
```
**期待結果**: カウンター口上が出力される

#### S8: イベント起床（KOJO_K4_EVENT.ERB）
```bash
uEmuera --unit "KOJO_EVENT_K4_0" --char "十六夜咲夜"
```
**期待結果**: 起床イベント口上が出力される

---

#### エラーケース

#### S9: 関数未発見
```bash
uEmuera --unit "NONEXISTENT_FUNCTION" --char "十六夜咲夜"
```
**期待結果**: exit 1、stderr に "Function not found"

#### S10: 変数名無効（警告継続）
```bash
uEmuera --unit "KOJO_MESSAGE_COM_K4_300" --char "十六夜咲夜" \
        --set "INVALID_VAR=100"
```
**期待結果**: stderr に警告、口上は正常出力、exit 0

#### S11: タイムアウト検知
```bash
uEmuera --unit "INFINITE_LOOP_FUNC" --char "十六夜咲夜" --timeout 5000
```
**期待結果**: 5秒後に exit 1、stderr に "Execution timeout"

## Test Plan

```bash
# Feature 057 回帰テスト (S1-S8)
for s in S1 S2 S3 S4 S5 S6 S7 S8; do
  echo "=== $s ===" && run_scenario_$s
done

# エラーケーステスト (S9-S11)
for s in S9 S10 S11; do
  echo "=== $s ===" && run_scenario_$s
done
```

## Effort Estimate

- **Size**: Medium
- **Risk**: High（Emuera内部構造の改修が必要）
- **Dependencies**: Feature 057（Headless基本動作）

## Links

- [feature-057.md](feature-057.md) - Headless起床後コマンド修正（前提）
- [feature-059.md](feature-059.md) - シナリオファイルと自動検証（後続）
- [reference/kojo-reference.md](reference/kojo-reference.md) - 口上システム概要
- [reference/testing-reference.md](reference/testing-reference.md) - テスト戦略
