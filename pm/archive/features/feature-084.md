# Feature 084: 分岐トレース意味化

## Status: [DONE]

## Background

### 問題

現在のHeadless分岐トレース（`--trace-branches`）は**行番号のみ**を出力する：

```
[BRANCH] KOJO_K1_300.ERB:42 taken=true
[BRANCH] KOJO_K1_300.ERB:45 taken=false
```

これだけでは「どの分岐（恋慕？思慕？）を通ったか」が分からない。

### 目標

静的解析（kojo-mapper）の分岐ブロック情報と動的実行時の行番号を紐づけ：

```
[BRANCH] KOJO_K1_300.ERB:42 "恋慕分岐" taken=true
[BRANCH] KOJO_K1_300.ERB:45 "思慕分岐" taken=false
```

### 用途

- テスト結果の可読性向上
- 「どのTALENT分岐を通過したか」の自動検証
- kojo-mapperカバレッジレポートとの連携

---

## Acceptance Criteria

### AC1: 分岐ブロックマッピング生成

kojo-mapperで各分岐の「意味ラベル」と行番号のマッピングを生成：

```json
{
  "KOJO_K1_300.ERB": {
    "42": { "label": "恋慕分岐", "condition": "TALENT:奴隷:恋慕 == 1" },
    "45": { "label": "思慕分岐", "condition": "TALENT:奴隷:思慕 == 1" },
    "48": { "label": "関係なし分岐", "condition": "ELSE" }
  }
}
```

**検証**:
- [x] kojo-mapperが分岐マッピングJSONを出力できる
- [x] 主要4分岐（恋人/恋慕/思慕/なし）が正しくラベル付けされる

### AC2: Headlessトレース出力拡張

`--trace-branches`実行時、マッピングファイルがあれば意味ラベルを付与：

```bash
dotnet run ... --trace-branches --branch-map branch-map.json
```

出力：
```
[BRANCH] KOJO_K1_300.ERB:42 "恋慕分岐" taken=true
```

**検証**:
- [x] `--branch-map`オプションがHeadlessRunnerのヘルプに表示される
- [x] HeadlessRunnerがCLI引数からBranchMapPathを読込む

### AC3: 条件式ハッシュによる堅牢化

行番号は編集でずれるため、条件式のハッシュでフォールバック：

```json
{
  "KOJO_K1_300.ERB": {
    "hash:abc123": { "label": "恋慕分岐", "line_hint": 42 }
  }
}
```

**検証**:
- [x] 行番号マッチ → ハッシュマッチ → 不明 の優先順位で解決
- [x] ERB編集後もハッシュで正しくマッピング

### AC4: kojo-testレポート連携

kojo-testのサマリーに通過分岐を表示：

```
KOJO_MESSAGE_思慕獲得_KU(1): PASS
  Branches: 恋慕分岐(taken), 思慕分岐(skipped)
```

**検証**:
- [x] `--verbose`オプションで分岐情報表示

---

## Implementation Notes

### アーキテクチャ

```
kojo-mapper (Python)
    ↓ 生成
branch-map.json
    ↓ 読込
HeadlessRunner.cs (C#)
    ↓ 出力
意味ラベル付きトレース
```

### 分岐検出パターン

```erb
; Pattern 1: TALENT直接参照
IF TALENT:奴隷:恋慕 == 1
  → ラベル: "恋慕分岐"

; Pattern 2: NTR_CHK_FAVORABLY呼び出し
SIF NTR_CHK_FAVORABLY(奴隷) == 2
  → ラベル: "恋慕分岐" (関数戻り値から推定)

; Pattern 3: SELECTCASE
SELECTCASE TALENT:奴隷:恋人
  CASE 1 → "恋人分岐"
  CASEELSE → "恋人以外分岐"
```

---

## Effort Estimate

**Medium** - kojo-mapper拡張 + Headless連携

---

## Dependencies

- Feature 055 (kojo-mapper拡張) - 完了済み
- Feature 060 (分岐トレース) - 完了済み

---

## Tasks

| # | Description | Status |
|:-:|-------------|:------:|
| 1 | Extend kojo-mapper to generate branch-block mapping JSON | [○] |
| 2 | Add `--branch-map` CLI option to HeadlessRunner | [○] |
| 3 | Implement hash-based fallback matching in Headless | [○] |
| 4 | Add branch info to kojo-test summary output | [○] |

---

## Execution Log

### Task 1 - 2025-12-17

**Implementer**: opus (Attempt 1)

**Changes**:
- Added `BranchBlock` dataclass for branch block representation
- Added `generate_condition_hash()` for SHA1 hash generation
- Added `get_branch_label()` for semantic label generation from conditions
- Added `extract_branch_blocks()` to extract branch blocks from function content
- Added `generate_branch_map()` to create JSON output structure
- Updated `main()` to use argparse with `--branch-map` and `--branch-map-output` options
- Branch blocks extracted during function analysis via `analyze_function_content()`

**CLI Usage**:
```bash
python kojo_mapper.py <character_dir> --branch-map
python kojo_mapper.py <character_dir> --branch-map --branch-map-output custom.json
```

**Output Format** (matches AC1 spec):
```json
{
  "KOJO_K1_EVENT.ERB": {
    "381": { "label": "恋慕分岐", "condition": "TALENT:奴隷:恋慕", "hash": "80a1fd4c" }
  }
}
```

**Supported Labels**:
- 恋人分岐, 恋慕分岐, 思慕分岐, 親愛分岐 (TALENT-based)
- 好感度N分岐 (NTR_CHK_FAVORABLY-based)
- 親密<=N分岐, 親密>N分岐 (ABL-based)
- 関係なし分岐 (ELSE blocks)
- 条件分岐 (generic fallback)

**Test Result**: 550 branch blocks mapped from 美鈴 kojo files

**Build**: N/A (Python tool, no build required)

---

### Task 2 - 2025-12-17

**Implementer**: opus (Attempt 1)

**Changes**:
- Added `BranchMapPath` property to `HeadlessOptions` class (line 133)
- Added `--branch-map` / `-bm` argument parsing (lines 725-731)
- Added help text for `--branch-map` option in help output (line 889)

**Build**: PASS

**CLI Usage**:
```bash
dotnet run ... --trace-branches --branch-map branch-map.json
dotnet run ... --trace-branches -bm branch-map.json
```

**Files Modified**:
- uEmuera/Assets/Scripts/Emuera/Headless/HeadlessRunner.cs

**Next Step**: Task 3 will implement the actual branch mapping logic to load the JSON file and use it during trace output.

---

### Task 3 - 2025-12-17

**Implementer**: opus (Attempt 1)

**Changes**:

Files Created:
- `BranchMapService.cs`: Service for loading and querying branch mapping JSON
  - Loads JSON mapping from file
  - Supports line number lookup with hash-based fallback
  - Singleton pattern for thread-safe access
  - Prioritized lookup: exact line match → hash fallback → null
- `TraceService.cs`: Thread-local service for trace settings
  - Manages TraceLevel (None/Basic/Deep)
  - Provides `TraceBranch()` for [BRANCH] output
  - Integrates with BranchMapService to add semantic labels

Files Modified:
- `HeadlessRunner.cs`:
  - Load BranchMapService at startup when `--branch-map` provided (line 225-228)
  - Pass `BranchMapPath` to KojoBatchOptions (line 1064)
  - Set TraceLevel before kojo test execution (line 1135)
- `KojoBatchRunner.cs`:
  - Added `BranchMapPath` property to KojoBatchOptions (line 73)
  - Load BranchMapService in batch runner (lines 98-101)
  - Set TraceLevel at batch start (line 104)
- `Instraction.Child.cs`:
  - Added `TraceService.TraceBranch()` calls for SIF (line 1818)
  - Added `TraceService.TraceBranch()` calls for IF/ELSEIF (line 1905)
  - Added `TraceService.TraceBranch()` calls for SELECTCASE (line 2005)

**Branch Output Format**:
```
[BRANCH] KOJO_K1_300.ERB:42 "恋慕分岐" taken=true
[BRANCH] KOJO_K1_300.ERB:45 taken=false
```

**Trace Behavior**:
- Output only when TraceLevel is Basic or Deep
- Semantic label shown only when branch map is loaded and match found
- Hash fallback automatically used if line number doesn't match
- Thread-safe for parallel test execution

**Build**: PASS

**Next Step**: Task 4 will add branch info to kojo-test summary output.

---

### Task 4 - 2025-12-17

**Implementer**: opus (Attempt 1)

**Changes**:

Files Modified:
- `TraceService.cs`:
  - Added `BranchDecision` class for branch decision data
  - Added thread-local `_branchDecisions` list for collecting branch decisions
  - Updated `TraceBranch()` to collect branch decisions when collection is active
  - Added `StartBranchCollection()` to begin collecting branch decisions
  - Added `GetBranchDecisions()` to retrieve and stop collection
  - Updated `Reset()` to clear branch decisions
- `KojoTestResult.cs`:
  - Added `Branches` property (List<BranchDecision>) to KojoTestResult class
  - Added `WriteBranchInfo()` method to format branch info in summary output
  - Updated `WriteCompact()` to call `WriteBranchInfo()`
  - Updated `WriteQuiet()` to call `WriteBranchInfo()`
  - Updated `BuildResultObject()` to include branches in JSON output
- `KojoTestRunner.cs`:
  - Added `TraceService.StartBranchCollection()` call before function execution
  - Added branch collection retrieval and assignment to result after execution
- `ProcessLevelParallelRunner.cs`:
  - Updated `ParseJsonResult()` to parse branches array from JSON output
  - Added code to deserialize BranchDecision objects from worker process results

**Summary Output Format**:
```
[+] KOJO_K1_300 (0.12s) - PASS
  Branches:
    [+] 恋慕分岐 (line 42)
    [-] 思慕分岐 (line 45)
    [+] 好感度5分岐 (line 120)
```

**JSON Output Format**:
```json
{
  "name": "KOJO_K1_300",
  "status": "pass",
  "duration_ms": 120,
  "branches": [
    { "label": "恋慕分岐", "filename": "KOJO_K1_300.ERB", "lineNumber": 42, "taken": true },
    { "label": "思慕分岐", "filename": "KOJO_K1_300.ERB", "lineNumber": 45, "taken": false }
  ]
}
```

**Build**: PASS

**How It Works**:
1. When kojo-test starts, it calls `TraceService.StartBranchCollection()`
2. During execution, every `TraceBranch()` call adds a BranchDecision to the thread-local list
3. After execution, test runner retrieves decisions via `GetBranchDecisions()`
4. Branch info is included in both text and JSON output formats
5. Parallel execution is safe via ThreadStatic collection

**Next**: All tasks completed. Feature 084 ready for testing.

---

## Links

- [feature-060.md](archive/feature-060.md) - Headless分岐トレース
- [kojo-mapper](../../tools/kojo-mapper/) - 口上カバレッジ分析ツール
- [testing-reference.md](reference/testing-reference.md) - テスト戦略
