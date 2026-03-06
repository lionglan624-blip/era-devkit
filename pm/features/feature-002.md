# Feature 002: State Injection for Headless Testing

## Status: [DONE]

## Overview

Extend headless mode to support direct game state injection, enabling rapid testing of dialogue (口上) and other game features without playing through the normal game flow.

## Use Case

```
開発者が口上ERBを作成
    ↓
状態設定ファイルで好感度等を指定
    ↓
Headlessで実行、コマンド入力
    ↓
出力ログで口上を確認
```

---

## Quick Start

### 1. シナリオファイルを作成

```json
{
  "name": "咲夜口上テスト - 高好感度",
  "variables": {
    "FLAG:5": 0,
    "FLAG:26": 0
  },
  "characters": {
    "咲夜": {
      "CFLAG:2": 5000,
      "CFLAG:6": 3,
      "CFLAG:21": 100
    }
  }
}
```

### 2. 入力ファイルを作成

```
0
0
0
テスト
テスト
1000
1000
999
0
0
```

### 3. テスト実行

```bash
dotnet uEmuera.Headless.dll --inject scenario.json Game < input.txt > output.log
```

---

## Scenario File Format

### JSON Schema

```json
{
  "name": "シナリオ名（任意）",
  "description": "説明（任意）",
  "variables": {
    "FLAG:インデックスまたは名前": 値
  },
  "characters": {
    "キャラ名": {
      "CFLAG:インデックスまたは名前": 値,
      "BASE:インデックスまたは名前": 値,
      "TALENT:インデックスまたは名前": 値,
      "ABL:インデックスまたは名前": 値
    }
  },
  "copy": [
    {"from": "コピー元キャラ", "to": "コピー先キャラ", "var": "CFLAG:インデックス"}
  ]
}
```

### Copy機能

あるキャラクターの変数値を別のキャラクターにコピーできます。

**使用例**: 咲夜の現在位置をプレイヤーにコピー（プレイヤーを咲夜と同じ場所に移動）

```json
{
  "name": "咲夜と同室テスト",
  "characters": {
    "十六夜咲夜": {
      "CFLAG:21": 100
    }
  },
  "copy": [
    {"from": "十六夜咲夜", "to": "MASTER", "var": "CFLAG:300"}
  ]
}
```

**copy配列の各要素**:
| フィールド | 説明 | 例 |
|-----------|------|-----|
| `from` | コピー元キャラクター名 | `"十六夜咲夜"` |
| `to` | コピー先キャラクター名 | `"MASTER"` |
| `var` | コピーする変数 | `"CFLAG:300"` |

### 特殊キャラクター参照

以下の特殊名がキャラクター名として使えます：

| 参照名 | 説明 |
|--------|------|
| `MASTER` | プレイヤーキャラクター |
| `TARGET` | 現在のターゲット |
| `ASSI` | アシスタント |
| `PLAYER` | プレイヤー（MASTERと同義） |

**使用例**:
- `"to": "MASTER"` → プレイヤーに値をコピー
- `"from": "TARGET"` → ターゲットから値を読み取り

### 変数の指定方法

| 変数タイプ | 形式 | 例 |
|-----------|------|-----|
| FLAG | `FLAG:インデックス` | `"FLAG:5": 0` |
| CFLAG | `CFLAG:インデックス` | `"CFLAG:2": 5000` |
| BASE | `BASE:インデックス` | `"BASE:0": 500` |
| TALENT | `TALENT:インデックス` | `"TALENT:0": 1` |
| ABL | `ABL:インデックス` | `"ABL:0": 100` |

### 主要な変数インデックス

**FLAG (グローバル変数)**
| Index | 名前 | 説明 |
|-------|------|------|
| 5 | ゲームモード | - |
| 11 | 侵入者の現在位置 | - |
| 21 | 訪問者の現在位置 | - |
| 26 | ＮＴＲパッチ設定 | - |

**CFLAG (キャラ変数)**
| Index | 名前 | 説明 |
|-------|------|------|
| 2 | 好感度 | 0-10000+ |
| 6 | 馴れ合い強度度 | 1-3+ |
| 21 | 屈服度 | 0-100+ |
| 300 | 現在位置 | マップ番号（下記参照） |

**マップ番号 (CFLAG:300)**
| Index | 場所名 |
|-------|--------|
| 1 | 正門 |
| 13 | 二階廊下 |
| 15 | あなた私室 |

※その他のマップ番号はゲーム内で確認

### キャラクター名

| 番号 | 名前 | CALLNAME |
|-----|------|----------|
| 0 | あなた | あなた |
| 1 | 紅美鈴 | 美鈴 |
| 2 | 小悪魔 | 小悪魔 |
| 3 | パチュリー・ノーレッジ | パチュリー |
| 4 | 十六夜咲夜 | 咲夜 |
| 5 | レミリア | レミリア |
| 6 | フランドール | フラン |

※NAMEまたはCALLNAMEどちらでも指定可能

---

## CLI Options

```bash
dotnet uEmuera.Headless.dll [options] <game-path>

Options:
  -h, --help                 ヘルプを表示
  -i, --inject <file>        状態注入用JSONファイル
  -o, --output <file>        出力をファイルに保存
  -l, --load <slot>          セーブスロット番号からロード (0-99)
  -f, --load-file <file>     任意のパスからセーブファイルをロード
  -e, --export-slot <slot> [path]
                             セーブスロットをファイルにエクスポート
```

### 使用例

```bash
# 基本実行
echo "0" | dotnet uEmuera.Headless.dll Game/

# 状態注入付き
dotnet uEmuera.Headless.dll --inject scenario.json Game/ < input.txt

# 出力をファイルに保存
dotnet uEmuera.Headless.dll --inject scenario.json --output result.log Game/ < input.txt

# セーブスロットからロード
dotnet uEmuera.Headless.dll --load 5 Game/ < input.txt

# 任意のセーブファイルをロード + 状態注入
dotnet uEmuera.Headless.dll --load-file tests/base-wakeup.sav --inject tests/scenario.json Game/ < input.txt

# セーブスロットをエクスポート（テスト用に退避）
dotnet uEmuera.Headless.dll --export-slot 5 tests/base-wakeup.sav Game/
```

---

## Injection Timing

```
HeadlessRunner.Run()
    ↓
SetupDirectories()
    ↓
ConfigData.LoadConfig()
    ↓
SetupLoadFile()  ← --load-file使用時
    ↓
window_.Init()  ← EmueraConsole作成
    ↓
★ FLAG注入（即座）
    ↓
RunGameLoop()
    ↓
    ...ゲームがキャラをロード...
    ↓
★ CFLAG/BASE/etc注入（遅延）← NPCがCharacterListに追加された時点
    ↓
★ Copy処理（遅延）← キャラ変数注入完了後
```

**重要**:
- NPCの変数は`ADDCHARA`でキャラクターがロードされるまで注入できません
- 遅延注入機能により、キャラがロードされた時点で自動的に適用されます
- Copy処理はキャラクター変数注入が完了した後に実行されます

### テスト用セーブファイルの準備

繰り返しデバッグを行う場合、**適切な状態のセーブファイル**を用意することが重要です：

| テスト目的 | 必要なセーブ状態 |
|-----------|-----------------|
| 口上テスト | 起床済み・マップ画面 |
| 同室テスト | 起床済み・対象キャラと会話可能な状態 |
| 初回イベント | 新規ゲーム直後 |

**注意**:
- NTRオプションメニューは**初回起床時のみ**表示される
- 起床前のセーブをロードすると毎回NTRメニューが出現し、テストが複雑化する
- **推奨**: 起床済み・NTR設定済みのセーブファイルを`tests/`に用意しておく

**セーブファイル作成手順**:
1. 通常のEmueraでゲームを起動
2. テストしたい状態まで進める（起床、NTR設定完了など）
3. セーブして`--export-slot`で`tests/`にエクスポート

---

## Test Results

### Confirmed Working (2024-12 Test)

**Setup Input Sequence** (`setup-full.txt`):
```
0           # Start new game
0           # START mode
0           # Not キャラなりきり
Player      # NAME
Player      # CALLNAME
1000        # Character confirm
1000        # Final confirm
999         # Skip character settings
0           # Confirm
0           # Keep gender
0           # Virgin defaults
2           # Skip shortcut (4 times needed due to game flow)
2
2
2
999         # Skip 寝間着設定
0           # Confirm
999         # Skip 体設定
0           # Confirm
999         # Skip 服装設定
0           # Confirm
100         # 起床 (SHOP menu)
0           # Wait after wake
100         # NTR menu return
400         # 移動 command
13          # 咲夜私室
300         # 会話 command
```

**Verified Injection**:
- `好感度:5000` (CFLAG:2 injected)
- `信頼度:100` (CFLAG:21 injected)
- Character located at 咲夜私室

**Execution Command**:
```bash
cd uEmuera/bin/Release/net8.0
cat Game/tests/setup-full.txt | timeout 60 dotnet uEmuera.Headless.dll \
  --inject Game/tests/test-scenario-sakuya.json \
  "Game" > output.log 2>&1
```

### Known Issues

**ERB Encoding Problem** - **RESOLVED**:
- Issue: Japanese variable names failed to parse (`TCVAR:?z??:??b?s?\`)
- Cause: ERB files use Shift-JIS, Config.Encode defaulted to UTF-8
- Fix: Added `CodePagesEncodingProvider` and set `Config.Encode = Encoding.GetEncoding(932)`
- Commit: `uEmuera@3b7ccbc`
- Verified: No TCVAR errors in re-test

### Original Test Log

```
[Scenario] Applying: 咲夜口上テスト - 高好感度
[Scenario] Character injection pending (will apply when characters load)
[Scenario] Applied 2 variables              ← FLAG注入成功
[Headless] Starting game loop...
...
[Scenario] Applied 6 pending character variables  ← CFLAG注入成功
```

---

## Files Created

| File | Description |
|------|-------------|
| `Headless/VariableResolver.cs` | 変数名→インデックス解決、特殊キャラ参照 |
| `Headless/StateInjector.cs` | 変数値の注入、読み取り |
| `Headless/ScenarioParser.cs` | JSONシナリオ解析、Copy処理 |
| `tests/test-scenario-sakuya.json` | テスト用シナリオ |
| `tests/test-input-sakuya.txt` | テスト用入力 |
| `tests/scenario-sakuya-sameroom.json` | 同室テスト用シナリオ (Copy機能) |
| `tests/base-wakeup.sav` | 起床後セーブデータ |

---

## Links

- [feature-001.md](feature-001.md) - Headless Mode spec
- [WBS-002.md](WBS-002.md) - Work breakdown
- [agents.md](agents.md) - Workflow rules
