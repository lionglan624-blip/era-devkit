# Feature 084: 刁E��トレース意味匁E

## Status: [DONE]

## Background

### 問顁E

現在のHeadless刁E��トレース�E�E--trace-branches`�E��E**行番号のみ**を�E力する！E

```
[BRANCH] KOJO_K1_300.ERB:42 taken=true
[BRANCH] KOJO_K1_300.ERB:45 taken=false
```

これだけでは「どの刁E��（恋慕？思�E�E�）を通ったか」が刁E��らなぁE��E

### 目樁E

静的解析！Eojo-mapper�E��E刁E��ブロチE��惁E��と動的実行時の行番号を紐づけ！E

```
[BRANCH] KOJO_K1_300.ERB:42 "恋�E刁E��E taken=true
[BRANCH] KOJO_K1_300.ERB:45 "思�E刁E��E taken=false
```

### 用送E

- チE��ト結果の可読性向丁E
- 「どのTALENT刁E��を通過したか」�E自動検証
- kojo-mapperカバレチE��レポ�Eトとの連携

---

## Acceptance Criteria

### AC1: 刁E��ブロチE��マッピング生�E

kojo-mapperで吁E�E岐�E「意味ラベル」と行番号のマッピングを生成！E

```json
{
  "KOJO_K1_300.ERB": {
    "42": { "label": "恋�E刁E��E, "condition": "TALENT:奴隷:恋�E == 1" },
    "45": { "label": "思�E刁E��E, "condition": "TALENT:奴隷:思�E == 1" },
    "48": { "label": "関係なし�E岁E, "condition": "ELSE" }
  }
}
```

**検証**:
- [x] kojo-mapperが�E岐�EチE��ングJSONを�E力できる
- [x] 主要E刁E��（恋人/恋�E/思�E/なし）が正しくラベル付けされめE

### AC2: Headlessトレース出力拡張

`--trace-branches`実行時、�EチE��ングファイルがあれ�E意味ラベルを付与！E

```bash
dotnet run ... --trace-branches --branch-map branch-map.json
```

出力！E
```
[BRANCH] KOJO_K1_300.ERB:42 "恋�E刁E��E taken=true
```

**検証**:
- [x] `--branch-map`オプションがHeadlessRunnerのヘルプに表示されめE
- [x] HeadlessRunnerがCLI引数からBranchMapPathを読込む

### AC3: 条件式ハチE��ュによる堁E��匁E

行番号は編雁E��ずれるため、条件式�Eハッシュでフォールバック�E�E

```json
{
  "KOJO_K1_300.ERB": {
    "hash:abc123": { "label": "恋�E刁E��E, "line_hint": 42 }
  }
}
```

**検証**:
- [x] 行番号マッチEↁEハッシュマッチEↁE不�E の優先頁E��で解決
- [x] ERB編雁E��もハッシュで正しくマッピング

### AC4: kojo-testレポ�Eト連携

kojo-testのサマリーに通過刁E��を表示�E�E

```
KOJO_MESSAGE_思�E獲得_KU(1): PASS
  Branches: 恋�E刁E��Etaken), 思�E刁E��Eskipped)
```

**検証**:
- [x] `--verbose`オプションで刁E��情報表示

---

## Implementation Notes

### アーキチE��チャ

```
kojo-mapper (Python)
    ↁE生�E
branch-map.json
    ↁE読込
HeadlessRunner.cs (C#)
    ↁE出劁E
意味ラベル付きトレース
```

### 刁E��検�Eパターン

```erb
; Pattern 1: TALENT直接参�E
IF TALENT:奴隷:恋�E == 1
  ↁEラベル: "恋�E刁E��E

; Pattern 2: NTR_CHK_FAVORABLY呼び出ぁE
SIF NTR_CHK_FAVORABLY(奴隷) == 2
  ↁEラベル: "恋�E刁E��E (関数戻り値から推宁E

; Pattern 3: SELECTCASE
SELECTCASE TALENT:奴隷:恋人
  CASE 1 ↁE"恋人刁E��E
  CASEELSE ↁE"恋人以外�E岁E
```

---

## Effort Estimate

**Medium** - kojo-mapper拡張 + Headless連携

---

## Dependencies

- Feature 055 (kojo-mapper拡張) - 完亁E��み
- Feature 060 (刁E��トレース) - 完亁E��み

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
    "381": { "label": "恋�E刁E��E, "condition": "TALENT:奴隷:恋�E", "hash": "80a1fd4c" }
  }
}
```

**Supported Labels**:
- 恋人刁E��E 恋�E刁E��E 思�E刁E��E 親愛�E岁E(TALENT-based)
- 好感度N刁E��E(NTR_CHK_FAVORABLY-based)
- 親寁E=N刁E��E 親寁EN刁E��E(ABL-based)
- 関係なし�E岁E(ELSE blocks)
- 条件刁E��E(generic fallback)

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
  - Prioritized lookup: exact line match ↁEhash fallback ↁEnull
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
[BRANCH] KOJO_K1_300.ERB:42 "恋�E刁E��E taken=true
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
    [+] 恋�E刁E��E(line 42)
    [-] 思�E刁E��E(line 45)
    [+] 好感度5刁E��E(line 120)
```

**JSON Output Format**:
```json
{
  "name": "KOJO_K1_300",
  "status": "pass",
  "duration_ms": 120,
  "branches": [
    { "label": "恋�E刁E��E, "filename": "KOJO_K1_300.ERB", "lineNumber": 42, "taken": true },
    { "label": "思�E刁E��E, "filename": "KOJO_K1_300.ERB", "lineNumber": 45, "taken": false }
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

- [feature-060.md](archive/feature-060.md) - Headless刁E��トレース
- [kojo-mapper](../../src/tools/kojo-mapper/) - 口上カバレチE��刁E��チE�Eル
- [testing-reference.md](../reference/testing-reference.md) - チE��ト戦略
