# Feature 101: Headless警告抽出機能

## Status: [DONE]

## Type: engine

## Background

### Problem
Headlessモードでは「解釈不可能な識別子」などの警告が出ても、ゲーム実行が継続してしまう。GUI版では起動不可になるエラーが、回帰テスト（flow/inject）で検出できない。

### Current Behavior

| モード | 警告時の挙動 | 検出可否 |
|--------|--------------|:--------:|
| GUI (uEmuera.exe) | 「Emueraを終了します」でエラー停止 | ✅ |
| Headless (flow/inject) | 警告出力のみ、実行継続 | ❌ |

### Goal
Headlessモードで「Now Loading...」から「era紅魔館」の間に出力される警告テキストを抽出し、エラーとして検出可能にする。

### Context
- Feature 100で発覚した問題への対応
- 警告が出ていても回帰テストがPASSしてしまうため、早期検出が困難
- 警告を無視する現行動作はデバッグ向けには有用だが、CI/品質管理には不適

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | --strict-warnings オプション追加 | output | contains | "--strict-warnings" | [x] |
| 2 | 警告検出時に非ゼロ終了 | exit_code | fails | - | [x] |
| 3 | 警告なし時にゼロ終了 | exit_code | succeeds | - | [x] |
| 4 | 警告内容を構造化出力 | output | contains | "[WARNING]" | [x] |

### AC Details

#### AC1: --strict-warnings オプション

**Test Command**:
```bash
cd Game && dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . --help 2>&1 | grep strict
```

**Expected**: "strict-warnings" が表示される

#### AC2: 警告検出時に非ゼロ終了

**Test Command** (警告があるコードで実行):
```bash
cd Game && echo "OP" | dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . --strict-warnings; echo $?
```

**Expected**: 終了コードが0以外

#### AC3: 警告なし時にゼロ終了

**Test Command** (修正後のコードで実行):
```bash
cd Game && echo "OP" | dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . --strict-warnings; echo $?
```

**Expected**: 終了コードが0

#### AC4: 警告内容を構造化出力

**Test Command**:
```bash
cd Game && echo "OP" | dotnet run --project ../uEmuera/uEmuera.Headless.csproj -- . --strict-warnings 2>&1 | grep WARNING
```

**Expected**: `[WARNING] ファイル名:行番号: メッセージ` 形式

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | HeadlessMode.cs に --strict-warnings オプション追加 | [○] |
| 2 | 2 | 警告検出時に非ゼロ終了コード設定 | [○] |
| 3 | 3 | 警告なし時にゼロ終了コード設定 | [○] |
| 4 | 4 | 警告の構造化出力フォーマット実装 | [○] |

---

## Implementation Strategy

### 警告抽出のタイミング

```
[Headless起動]
    ↓
[Now Loading...]  ←── ここから
    ↓
[警告出力] ←───────── 警告収集
    ↓
[era紅魔館protoNTR] ←── ここまで
    ↓
[--strict-warnings時: 警告があればexit(1)]
    ↓
[ゲームループ開始]
```

### 実装箇所

**uEmuera/Headless/HeadlessMode.cs**:
```csharp
// 1. コマンドラインオプション追加
private static bool _strictWarnings = false;

// 2. 警告収集リスト
private static List<string> _loadingWarnings = new();

// 3. Loading phase での警告キャプチャ
// "Now Loading..." から タイトル表示 までのPRINT出力を監視
// "警告Lv" で始まる行を _loadingWarnings に追加

// 4. Loading完了時のチェック
if (_strictWarnings && _loadingWarnings.Count > 0)
{
    foreach (var w in _loadingWarnings)
        Console.WriteLine($"[WARNING] {w}");
    Console.WriteLine($"[FATAL] {_loadingWarnings.Count} warnings during loading");
    Environment.Exit(1);
}
```

### 出力フォーマット

```
[WARNING] 口上\1_美鈴/KOJO_K1_会話親密.ERB:31: "添い寝中"は解釈できない識別子です
[WARNING] 口上\2_小悪魔/KOJO_K2_会話親密.ERB:27: "添い寝中"は解釈できない識別子です
...
[FATAL] 36 warnings during loading. Use --no-strict-warnings to ignore.
```

---

## Alternative Approaches

### Option A: --strict-warnings (Recommended)
- 明示的なオプションで制御
- 既存の動作を壊さない
- CI/回帰テストで有効化

### Option B: --ignore-warnings
- デフォルトでstrictにする
- 開発時に--ignore-warningsで緩和
- 破壊的変更になる

### Option C: 警告出力のみ（exit codeは変えない）
- `grep "WARNING"` でチェック可能
- 最も保守的だが、CI統合が面倒

**選択**: Option A（--strict-warnings）を推奨

---

## Execution State

**Status**: [IN_PROGRESS]
**Current Phase**: Implementation
**Next Step**: AC validation testing

### State Summary
- Feature 101 initialized for engine implementation
- Type: engine (C# Headless mode)
- Target: Add --strict-warnings CLI option to HeadlessMode.cs
- Complexity: Low (config + exit code handling)

### Task Progress
- Task 1: COMPLETED - --strict-warnings option added (HeadlessOptions property, ParseArguments, ShowHelp)
- Task 2: COMPLETED - Warning detection mechanism added after HeadlessWindow.Init()
- Task 3: COMPLETED - Exit with non-zero code (return 1) when warnings exist in strict mode
- Task 4: COMPLETED - Structured warning output with [WARNING] prefix for each warning

### Key Context
- 4 ACs define the feature scope
- 4 Tasks mapped to ACs
- All tasks completed
- No blockers identified
- Task 4 Note: Output format is [WARNING] Filename:LineNo: Message

---

## Execution Log

| Date | Agent | Action | Result |
|------|-------|--------|--------|
| 2025-12-18 | initializer | Status: [PROPOSED] → [APPROVED] | READY |
| 2025-12-18 | implementer | Task 1: Add --strict-warnings option | SUCCESS |
| 2025-12-18 | implementer | Task 2: Warning detection after Init() | SUCCESS |
| 2025-12-18 | implementer | Task 3: Exit with non-zero code on warnings | SUCCESS |
| 2025-12-18 | implementer | Task 4: Structured warning output format | SUCCESS |
| 2025-12-18 | debugger (Attempt 1) | Fix warning detection mechanism | SUCCESS |
| 2025-12-18 | debugger (Attempt 2) | Fix AC4 warning output format | SUCCESS |
| 2025-12-18 | finalizer | Status: [WIP] → [DONE] | VERIFIED |

---

## Discovered Issues

### Issue 1: Warning Detection Mechanism (RESOLVED)

**Discovered**: 2025-12-18 (debugger Attempt 1)
**Type**: Implementation bug
**Root Cause**: WarningCollector.Flush() clears the warning list after printing. By the time HeadlessRunner checks GetWarnings() after Init(), the list is already empty because ErbLoader.LoadErbFiles() calls FlushWarningList() 3 times during loading (lines 76, 83, 91).

**Fix Applied (Attempt 1)**:
1. Added `_totalWarningCount` field to WarningCollector to track cumulative warnings
2. Increment counter in AddWarning() before Flush() clears the list
3. Added GetTotalWarningCount() method for --strict-warnings to use
4. Added ResetTotalWarningCount() called before loading in HeadlessRunner

**Verification**:
- With _TEST_WARNING.ERB: Exit code 1, "[FATAL] 2 warning(s) detected" ✓
- Without warnings: Exit code 0 ✓

---

### Issue 2: AC4 Warning Output Format (RESOLVED)

**Discovered**: 2025-12-18 (debugger Attempt 2)
**Type**: Implementation bug
**Root Cause**: WarningCollector.Flush() clears the `_warnings` list after printing warnings in engine's native format ("警告Lv2:..."). By the time HeadlessRunner checks after Init(), GetWarnings() returns empty list. Only count survived from Attempt 1 fix, but individual warning details (message, filename, line number) were lost.

**AC4 Requirement**: Output each warning in `[WARNING] Filename:LineNo: Message` format before `[FATAL]` summary.

**Fix Applied (Attempt 2)**:
1. Added `_allWarnings` list to WarningCollector that persists after Flush()
2. Modified AddWarning() to store warning in both `_warnings` (temporary) and `_allWarnings` (permanent)
3. Added GetAllWarnings() method to retrieve all warnings including flushed ones
4. Modified ResetTotalWarningCount() to also clear `_allWarnings`
5. Updated HeadlessRunner to use GetAllWarnings() and output each warning in `[WARNING]` format

**Files Modified**:
- uEmuera/Assets/Scripts/Emuera/Sub/WarningCollector.cs (+3 lines in AddWarning, +10 lines for GetAllWarnings, +1 line in Reset)
- uEmuera/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs (replaced GetTotalWarningCount() with GetAllWarnings() loop)

**Verification**:
- With _TEST_WARNING.ERB:
  ```
  [WARNING] 口上/_TEST_WARNING.ERB:9: "これは不正なケース文"は解釈できない識別子です
  [WARNING] 口上/_TEST_WARNING.ERB:12: "絶対に存在しない名前XYZ"は解釈できない識別子です
  [FATAL] 2 warning(s) detected during loading
  ```
- Without _TEST_WARNING.ERB: `[Headless] Game ended.` (clean exit) ✓

| Issue | Type | Priority |
|-------|------|----------|

---

## AC Test Results

### AC2 Verification (2025-12-18) - PASS

**Status**: PASS
**AC**: 2
**Matcher**: exit_code | fails (requires exit code != 0 when warnings exist)

**Initial Issue**: No warnings in clean codebase (Feature 100 fixed all).

**Intentional Error Test (意地悪試験)**:
1. Created `_TEST_WARNING.ERB` with intentional errors (undefined identifiers)
2. Ran: `dotnet run ... --strict-warnings`
3. Output:
   ```
   [WARNING] 口上/_TEST_WARNING.ERB:9: "縺薙Ｌ縺ｯ..."は解釈できない識別子です
   [WARNING] 口上/_TEST_WARNING.ERB:12: "邨ｶ蟇ｾ縺ｫ..."は解釈できない識別子です
   [FATAL] 2 warning(s) detected during loading
   EXIT FAILURE (non-zero)
   ```
4. Test file removed after verification.

**Result**: Exit code is non-zero (1) when warnings detected ✓

---

### AC4 Verification (2025-12-18) - PASS

**Status**: PASS
**AC**: 4
**Matcher**: output | contains "[WARNING]"

**Initial Issue**: Warnings output in engine format (警告Lv2:...) not [WARNING] format.

**Debug Fix (Attempt 2)**:
1. Added `_allWarnings` list to WarningCollector to persist warnings after Flush()
2. Modified HeadlessRunner to use GetAllWarnings() and output each in [WARNING] format

**Intentional Error Test (意地悪試験)**:
1. Created `_TEST_WARNING.ERB` with intentional errors
2. Output:
   ```
   [WARNING] 口上/_TEST_WARNING.ERB:9: "これは不正なケース文"は解釈できない識別子です
   [WARNING] 口上/_TEST_WARNING.ERB:12: "絶対に存在しない名前XYZ"は解釈できない識別子です
   [FATAL] 2 warning(s) detected during loading
   ```
3. Test file removed after verification.

**Result**: Warnings output in [WARNING] format ✓

---

## Design Decision: strict-warnings と CI 思想の整合性 (F213 時点で追記)

### 経緯

F213（Flow Parallel Thread Safety）レビュー時に、pre-commit hook の設計思想について議論が発生。
「strict-warnings は verify-logs.py によるファイルチェック思想に合っていないのでは？」という疑問。

### 背景: CI テスト結果の取得方式

F205 で verify-logs.py を導入し、F212 で pre-commit hook を構築した。

| テスト | 結果取得方式 | verify-logs.py 対象 |
|--------|:------------:|:------------------:|
| dotnet build | exit code | ❌ |
| dotnet test | ファイル (.trx) | ✅ |
| **--strict-warnings** | **exit code** | **❌** |
| --flow | ファイル (.json) | ✅ |
| --unit | ファイル (.json) | ✅ |

strict-warnings だけが「exit code のみ、ファイル出力なし」で、他のテストと異なる。

### 疑問点

1. **思想の不統一**: verify-logs.py は「ファイルを読んで機械的に判定」が目的だが、strict-warnings は対象外
2. **ファイル出力追加すべき？**: `logs/prod/strict/warnings.json` を出力して統一すべきか

### 結論: 現状維持（ファイル出力追加は不要）

**理由1: 実務的なポジションが異なる**

| カテゴリ | テスト | 性質 |
|----------|--------|------|
| **前提条件チェック** | build, test, **strict-warnings** | FAIL 頻度低、即座に停止すべき |
| **テスト実行** | flow, unit | 結果を蓄積して最終判定 |

strict-warnings は「ERB パース時の構文エラー検出」であり、build/test と同列の前提条件チェック。
FAIL したら即座に停止し、後続テストは実行しない。ファイルに蓄積する意味がない。

**理由2: 技術的な依存関係で順序が決まる**

```
[1] build   ← エンジンビルド（必須）
[2] test    ← C# ユニットテスト
[3] strict  ← ERB パース（build 後でないと動かない）
[4] flow    ← 統合テスト
[5] unit    ← AC テスト
[6] verify  ← 最終判定
```

ERB はスクリプトであり、dotnet build では検証されない。ゲーム起動時（dotnet run）に初めてパースされる。
そのため strict-warnings は build の後に配置される。これは TDD 的な「早く失敗」ではなく、技術的な依存関係による。

**理由3: FAIL 頻度と情報量**

| 観点 | strict-warnings |
|------|-----------------|
| FAIL 頻度 | 低い（ERB 編集時のみ） |
| FAIL 時の出力 | 数行〜数十行（冗長ではない） |
| デバッグ性 | 即座にコンソールで原因確認可能 |

FAIL 時はすぐに原因を見て直せばよく、ファイルに残す価値は低い。

### 参照

- [feature-205.md](feature-205.md) - verify-logs.py 導入
- [feature-212.md](feature-212.md) - pre-commit hook 構築
- [feature-213.md](feature-213.md) - 本議論の発生元

---

## Links

- [Feature 100](feature-100.md) - uEmuera起動エラー修正（きっかけ）
- [HeadlessMode.cs](../../uEmuera/Headless/HeadlessMode.cs)
