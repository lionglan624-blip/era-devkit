# Feature 626: ac-static-verifier Matcher Enhancement

## Status: [DONE]

## Type: infra

## Background

### Philosophy (思想・上位目標)

AC 検証の完全自動化。手動検証への fallback は非効率であり、ac-static-verifier が全 AC タイプを正しく処理できることで /run ワークフローの信頼性が向上する。F621 でパターン解析を修正したが、matcher 別の処理ロジックにまだ改善の余地がある。

### Problem (現状の問題)

F625 実行時に ac-static-verifier が以下の問題を報告：

1. **`contains` matcher + regex pattern**: AC#1-7 で `"Status: \\[DONE\\]"` を使用したが、ツールは「Pattern contains regex patterns. Use 'matches' matcher for regex patterns.」エラーを返した。F621 でエラーメッセージを追加したが、根本的には **AC 定義側の問題を示すガイダンス** が必要。

2. **`exists` matcher + Expected=`-`**: AC#10-12 で `Glob(tools/YamlSchemaGen/*.cs)` を Method に、Expected を `-` としたが、ツールは `file_path: "-"` を存在チェックしようとして失敗。**Method 列の Glob パターンを解析** すべき。

3. **`build` matcher + Expected=`-`**: AC#14 で Method=`dotnet build`、Expected=`-` としたが、ツールは Expected 列からコマンドを取得しようとして失敗。**Method 列からコマンドを取得** すべき。

### Goal (このFeatureで達成すること)

1. `exists` matcher: Method 列の `Glob(pattern)` からパターンを抽出してファイル存在チェック
2. `build` matcher: Method 列からビルドコマンドを取得して実行
3. `contains` matcher + regex escape: 現在のエラーメッセージを維持（F621 設計通り）、ただし AC 定義ガイドへのリンクを追加
4. テストケース追加で regression 防止

### Session Context

- **Trigger**: F625 Phase 6 で ac-static-verifier が 10/14 FAIL を報告（手動検証は 16/16 PASS）
- **Related**: F621 (ac-static-verifier Pattern Parsing Fundamental Fix) - パターン解析修正済み
- **Related**: F618 (ac-static-verifier MANUAL Status Counting Fix) - MANUAL カウント修正済み
- **Tool location**: `tools/ac-static-verifier.py`

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. **Why**: F625 Phase 6 で ac-static-verifier が 10/14 FAIL を報告したが、手動検証では 16/16 PASS だった
2. **Why**: ツールが `exists` matcher (AC#10-12) と `build` matcher (AC#14) の AC 定義を正しく処理できなかった
3. **Why**: `verify_file_ac()` 関数内の file existence check で `file_path = ac.expected` と Expected 列からパス取得、`verify_build_ac()` 関数内で `build_command = ac.expected.strip()` と Expected 列からコマンド取得する設計
4. **Why**: AC テーブル定義の進化：当初は Expected 列にデータを置く設計だったが、F625 等で Method 列に `Glob(pattern)` や `dotnet build` を記載し Expected を `-` とするパターンが出現
5. **Why**: F621 で Pattern 解析は修正されたが、matcher タイプ別の列使用規則（Method vs Expected）の多様性は考慮されていなかった

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| `exists` matcher で `file_path: "-"` を存在チェック → FAIL | `verify_file_ac()` has glob support when pattern is in Expected column, but not when Method=`Glob(pattern)` and Expected=`-`. Need to extract pattern from Method column in latter case |
| `build` matcher で `command: "-"` を実行 → FAIL | `verify_build_ac()` の command extraction 部が Expected 列から取得。Method 列の `dotnet build` を解析していない |
| `contains` + `\\[DONE\\]` で regex error | F621 設計通りの正しい動作（`_contains_regex_metacharacters()` が `\\[` を検出）。AC 定義側で `matches` を使うべき |

### Conclusion

根本原因は **matcher タイプごとの列使用規則が硬直的**:

| Matcher | 現在の動作 | 期待される動作 |
|---------|-----------|---------------|
| `contains/matches` | Expected 列からパターン取得 | ✅ 正しく動作 |
| `exists` | Expected 列からファイルパス取得 | ❌ Method 列の `Glob(pattern)` から取得すべき |
| `build` | Expected 列からコマンド取得 | ❌ Method 列からコマンド取得すべき |

**Code Evidence**:
- `verify_file_ac()` 関数: Grep check 条件分岐後の file existence check 部で `file_path = ac.expected` を使用
- `verify_build_ac()` 関数: command extraction 部で `build_command = ac.expected.strip()` を固定で使用

**Fix Direction**:
1. `verify_file_ac()`: Method 列が `Glob(pattern)` 形式の場合、pattern を抽出して存在チェック
2. `verify_build_ac()`: Expected が `-` の場合、Method 列からコマンド取得
3. `contains` + regex: 現状維持（F621 設計通り）、ただしエラーメッセージに AC 定義ガイドへのリンク追加を検討

---

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F621 | [DONE] | Predecessor | Pattern Parsing Fundamental Fix - regex escape, Method format 等を修正。基盤整備済み |
| F618 | [CANCELLED] | Investigation | MANUAL Status Counting Fix - 調査の結果バグなしと判明 |
| F625 | [DONE] | Trigger | Post-Phase Review Phase 17 で問題発見。手動検証で全 PASS 確認済み |
| F619 | [DONE] | Prior issue | Feature Creation Workflow - F621 の trigger となった問題発見 |

### Pattern Analysis

**Recurring Pattern Identified**: ac-static-verifier は複数回の修正を経ている：
- F618: MANUAL status counting → 調査の結果バグなし
- F619: Pattern parsing issues → F621 で修正
- F625: Method/Expected 列使用パターン → 本 Feature で対応

**Root Pattern**: AC 定義フォーマットの進化（新パターン追加）に対してツールが追従していない。

**Prevention Strategy**: 新しい AC 定義パターンを導入する際は、ac-static-verifier の対応を同時に確認・実装する。

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | **YES** | コード変更箇所明確（`verify_file_ac()`, `verify_build_ac()` 関数）、条件分岐追加で対応可能 |
| Scope is realistic | **YES** | 2 箇所の条件分岐追加 + テスト追加、F621 と同規模 |
| No blocking constraints | **YES** | F621 完了済み、JSON schema/exit code 互換性要件は F621 で確立済み |

**Verdict**: FEASIBLE

**Implementation Estimate**:
- `verify_file_ac()`: Method 列の `Glob(pattern)` 解析追加 (~15 lines)
- `verify_build_ac()`: Expected=`-` 時に Method 列からコマンド取得 (~10 lines)
- Test files: 2 new test files (method_glob.py, method_build.py)
- Total: ~50 lines code change + 2 test files

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F621 | [DONE] | Pattern Parsing Fundamental Fix - 本 Feature の基盤 |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| Python 3 stdlib | Runtime | None | re, subprocess, pathlib, glob |
| ripgrep (rg) | Optional | Low | grep fallback あり |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| `.claude/skills/run-workflow/PHASE-6.md` | **CRITICAL** | AC 検証の中核ツール |
| `tools/verify-logs.py` | **HIGH** | JSON output schema 依存（`summary.failed`, `summary.total`） |
| `tools/tests/test_ac_verifier_*.py` | **MEDIUM** | 既存テストが PASS し続ける必要あり |

**Critical Constraint**: JSON output schema 変更禁止（F621 で確立）

---

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| `tools/ac-static-verifier.py` | **Update** | `verify_file_ac()` と `verify_build_ac()` に Method 列解析追加 |
| `tools/tests/test_ac_verifier_method_glob.py` | **Create** | `exists` matcher + Method=`Glob(pattern)` のテスト |
| `tools/tests/test_ac_verifier_method_build.py` | **Create** | `build` matcher + Method=`dotnet build` のテスト |

---

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| JSON schema stability | F621, verify-logs.py | **HIGH** - `summary.total`, `summary.passed`, `summary.manual`, `summary.failed` 維持必須 |
| Exit code semantics | PHASE-6.md, testing SKILL | **HIGH** - 0=pass, 1=fail 維持必須 |
| Backward compatibility | Existing ACs | **HIGH** - 既存 AC が引き続き PASS する必要あり |
| F621 fixes preserved | `_contains_regex_metacharacters()` function | **MEDIUM** - F621 の修正を壊さない |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Method 列解析が既存 AC を壊す | Low | High | 新規パターンのみ追加、fallback で既存動作維持 |
| Glob pattern extraction が不正確 | Low | Medium | F625 の AC#10-12 でテスト検証 |
| Build command 解析が危険なコマンドを許可 | Low | Low | 既存の信頼モデル維持（feature file は信頼される） |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "完全自動化" (complete automation) | All AC types handled without manual fallback | AC#1-4, AC#9 |
| "全 AC タイプを正しく処理" (correctly process ALL types) | `exists` matcher parses `Glob(pattern)` from Method | AC#1, AC#2 |
| "全 AC タイプを正しく処理" (correctly process ALL types) | `build` matcher parses command from Method when Expected=`-` | AC#3, AC#4 |
| "信頼性が向上" (reliability improvement) | Regression tests prevent future breakage | AC#5, AC#6, AC#7 |
| "F621 設計通り" (per F621 design) | `contains` + regex behavior preserved | AC#8, AC#9 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Glob pattern extraction code exists | code | Grep(tools/ac-static-verifier.py) | contains | `Glob\(` | [x] |
| 2 | exists matcher test passes | test | pytest tools/tests/test_ac_verifier_method_glob.py | succeeds | - | [x] |
| 3 | Method column parsing code exists | code | Grep(tools/ac-static-verifier.py) | contains | `ac.method` | [x] |
| 4 | build matcher test passes | test | pytest tools/tests/test_ac_verifier_method_build.py | succeeds | - | [x] |
| 5 | Test file for exists+Glob created | file | Glob(tools/tests/test_ac_verifier_method_glob.py) | exists | - | [x] |
| 6 | Test file for build+Method created | file | Glob(tools/tests/test_ac_verifier_method_build.py) | exists | - | [x] |
| 7 | Existing tests still pass | test | pytest tools/tests/test_ac_verifier*.py | succeeds | - | [x] |
| 8 | F621 regex detection preserved | code | Grep(tools/ac-static-verifier.py) | contains | `_contains_regex_metacharacters` | [x] |
| 9 | F621 error message preserved | code | Grep(tools/ac-static-verifier.py) | contains | `Use 'matches' matcher` | [x] |
| 10 | JSON schema unchanged | code | Grep(tools/ac-static-verifier.py) | contains | `"summary": {` | [x] |
| 11 | Exit code semantics preserved | code | Grep(tools/ac-static-verifier.py) | contains | `return 0 if failed == 0 else 1` | [x] |

**Note**: 11 ACs is within infra feature range (8-15).

### AC Details

**AC#1: Glob pattern extraction code exists**
- Method: Grep(tools/ac-static-verifier.py) for `Glob\(`
- Verifies: Code path to parse `Glob(pattern)` syntax from Method column exists
- Rationale: Enables `exists` matcher to extract file pattern from Method column

**AC#2: exists matcher test passes**
- Method: pytest tools/tests/test_ac_verifier_method_glob.py
- Test cases: (1) Glob pattern matches files → PASS, (2) Glob pattern no match → FAIL, (3) Backward compat with Expected column
- Verifies: End-to-end behavior of `exists` matcher with Method=`Glob(pattern)`

**AC#3: Method column parsing code exists**
- Method: Grep(tools/ac-static-verifier.py) for `ac.method`
- Verifies: Code path exists in verify_build_ac to use Method column for build command
- Rationale: Enables `build` matcher to take command from Method when Expected=`-`

**AC#4: build matcher test passes**
- Method: pytest tools/tests/test_ac_verifier_method_build.py
- Test cases: (1) Method=`echo test`, Expected=`-` → PASS, (2) Method=`exit 1`, Expected=`-` → FAIL, (3) Backward compat with Expected column
- Verifies: End-to-end behavior of `build` matcher with Method column

**AC#5: Test file for exists+Glob created**
- Method: Glob(tools/tests/test_ac_verifier_method_glob.py)
- Verifies: Regression test file exists for exists matcher enhancement

**AC#6: Test file for build+Method created**
- Method: Glob(tools/tests/test_ac_verifier_method_build.py)
- Verifies: Regression test file exists for build matcher enhancement

**AC#7: Existing tests still pass**
- Method: pytest tools/tests/test_ac_verifier*.py
- Verifies: Backward compatibility - no existing tests broken by changes
- Critical: All test_ac_verifier_*.py files must pass

**AC#8: F621 regex detection preserved**
- Method: Grep for `_contains_regex_metacharacters` function
- Verifies: F621 fix for regex pattern detection is not accidentally removed
- Rationale: This function enables proper error messages for regex in `contains` matcher

**AC#9: F621 error message preserved**
- Method: Grep for `Use 'matches' matcher` string
- Verifies: User-facing error message from F621 still exists
- Rationale: Users need guidance when using regex with `contains` matcher

**AC#10: JSON schema unchanged**
- Method: Grep for `"summary": {` pattern
- Verifies: JSON output schema structure exists (Critical Constraint per Dependencies section)
- Consumer: tools/verify-logs.py depends on this schema

**AC#11: Exit code semantics preserved**
- Method: Grep for `return 0 if failed == 0 else 1` pattern
- Verifies: Exit code behavior unchanged (0=pass, 1=fail)
- Consumer: PHASE-6.md workflow depends on exit code semantics

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Column-Aware Parsing Strategy with Fallback**: Extend `verify_file_ac()` and `verify_build_ac()` functions to intelligently extract parameters from either Method or Expected columns based on AC definition patterns, with fallback to preserve backward compatibility.

**Core Changes**:
1. **`verify_file_ac()`**: Add Glob pattern extraction from Method column when `Glob(pattern)` syntax detected
2. **`verify_build_ac()`**: Add Method column command extraction when Expected is `-`
3. **F621 preservation**: Zero changes to `_contains_regex_metacharacters()` and related error messages

**Why this approach satisfies the Philosophy**:
- **"完全自動化" (complete automation)**: Enables ac-static-verifier to handle `exists` and `build` matchers with Method column patterns without manual intervention
- **"全 AC タイプを正しく処理" (correctly process ALL types)**: Method column parsing supports evolved AC definition patterns while maintaining backward compatibility
- **"信頼性が向上" (reliability improvement)**: Fallback pattern ensures existing ACs continue to work, regression tests prevent future breakage

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add regex pattern `Glob\((.*?)\)` in `verify_file_ac()` function to extract pattern from Method column |
| 2 | Create `test_ac_verifier_method_glob.py` with test cases: (1) Positive: `Glob(tools/*.py)` matches files → PASS, (2) Negative: `Glob(nonexistent/*.foo)` no match → FAIL, (3) Backward compat: Expected column path → PASS |
| 3 | Add conditional `if ac.expected.strip() == "-": build_command = ac.method.strip()` in `verify_build_ac()` function |
| 4 | Create `test_ac_verifier_method_build.py` with test cases: (1) Positive: Method=`echo test`, Expected=`-` → PASS, (2) Negative: Method=`exit 1`, Expected=`-` → FAIL, (3) Backward compat: Expected column command → PASS |
| 5 | Glob test file created as `tools/tests/test_ac_verifier_method_glob.py` |
| 6 | Build test file created as `tools/tests/test_ac_verifier_method_build.py` |
| 7 | Run `pytest tools/tests/test_ac_verifier*.py -v` to verify all existing tests still pass (backward compatibility check) |
| 8 | Grep verification confirms `_contains_regex_metacharacters` function unchanged (F621 preservation) |
| 9 | Grep verification confirms error message "Use 'matches' matcher" unchanged (F621 preservation) |
| 10 | Grep verification confirms JSON schema `"summary": {` structure unchanged |
| 11 | Grep verification confirms exit code logic `return 0 if failed == 0 else 1` unchanged |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **Glob pattern source** | A) Method only<br>B) Expected only<br>C) Method with Expected fallback | **C** | Backward compatibility: existing ACs use Expected column (file existence check section), new ACs use Method. Fallback ensures zero breakage. |
| **Glob extraction method** | A) Regex `Glob\((.*?)\)`<br>B) String split on `(` and `)`<br>C) Full AST parser | **A** | Regex is robust for simple `Glob(pattern)` syntax. String split fragile for nested parens. AST parser overkill. |
| **Build command trigger** | A) Expected=`-` triggers Method<br>B) Method always precedence<br>C) Expected always precedence | **A** | Explicit marker avoids ambiguity. Current code uses Expected column (command extraction section), so Method precedence would break existing ACs. |
| **F621 code handling** | A) Refactor for consistency<br>B) Leave completely untouched | **B** | F621 fixes are stable and tested. Any change risks regression. Zero-touch approach eliminates risk. |
| **Test organization** | A) Add to existing test files<br>B) Create separate test files | **B** | Separation of concerns. Each enhancement (Glob pattern, Method command) has dedicated test file for clear regression coverage. |

### Implementation Details

#### 1. `verify_file_ac()` Enhancement - Glob Pattern Extraction

**Target Function**: `verify_file_ac()` in `tools/ac-static-verifier.py`

**Current Logic** (existing code behavior):
```python
# After slash command check and Grep content check
file_path = ac.expected  # Always uses Expected column
```

**Enhanced Logic** (new behavior with fallback):
```python
# Check if Method contains Glob(pattern) syntax
if "Glob(" in ac.method:
    # Extract pattern from Glob(pattern) using regex
    import re
    match = re.search(r'Glob\((.*?)\)', ac.method)
    if match:
        file_path = match.group(1)  # Use extracted pattern
    else:
        file_path = ac.expected  # Fallback if regex fails
else:
    file_path = ac.expected  # Backward compatibility: traditional behavior
```

**Integration Point**: Replace line 567 `file_path = ac.expected` with conditional logic that first checks if Method contains `Glob(pattern)`. The glob pattern detection at line 574 already handles glob syntax in the path, so extracted pattern from Method will flow through correctly

**Rationale**:
- String check `"Glob(" in ac.method` is fast initial filter
- Regex `Glob\((.*?)\)` extracts content between first `(` and matching `)`
- Non-greedy `.*?` handles patterns like `Glob(tools/*.py)` correctly
- Fallback to `ac.expected` preserves existing AC behavior when Method has no Glob syntax

#### 2. `verify_build_ac()` Enhancement - Method Column Command

**Target Function**: `verify_build_ac()` in `tools/ac-static-verifier.py`

**Current Logic** (existing code behavior):
```python
# Command extraction section - always extracts command from Expected column
build_command = ac.expected.strip()
```

**Enhanced Logic** (new behavior with fallback):
```python
# If Expected is "-", use Method column for command
if ac.expected.strip() == "-":
    build_command = ac.method.strip()
else:
    build_command = ac.expected.strip()  # Backward compatibility: traditional behavior
```

**Integration Point**: Replace line 325 command extraction statement `build_command = ac.expected.strip()` with conditional logic

**Rationale**:
- Explicit `-` marker in Expected indicates "use Method for command"
- Clear semantics: either Expected has command (traditional) OR Expected is `-` (new pattern)
- No ambiguity when both columns have content
- Backward compatible: existing ACs with `Expected=dotnet build` continue to work

#### 3. Test Files - Regression Coverage

**File 1**: `tools/tests/test_ac_verifier_method_glob.py`

**Purpose**: Verify `exists` matcher handles `Glob(pattern)` in Method column

**Test Cases**:
1. **Positive**: Method=`Glob(tools/*.py)`, Expected=`-`, matcher=`exists` → PASS (files exist)
2. **Negative**: Method=`Glob(nonexistent/*.foo)`, Expected=`-`, matcher=`exists` → FAIL (no files)
3. **Backward Compat**: Method=`(empty)`, Expected=`tools/ac-static-verifier.py`, matcher=`exists` → PASS (traditional format)

**File 2**: `tools/tests/test_ac_verifier_method_build.py`

**Purpose**: Verify `build` matcher handles command in Method column

**Test Cases**:
1. **Positive**: Method=`echo test`, Expected=`-`, matcher=`succeeds` → PASS (exit code 0)
2. **Negative**: Method=`exit 1`, Expected=`-`, matcher=`succeeds` → FAIL (exit code 1)
3. **Backward Compat**: Method=`(empty)`, Expected=`echo test`, matcher=`succeeds` → PASS (traditional format)

#### 4. F621 Preservation - Zero Changes

**No code modifications** to these sections:

**Function**: `_contains_regex_metacharacters()`
- Grep AC#8 verifies function name exists unchanged
- Purpose: Detects regex patterns in `contains` matcher

**Error Message**: "Use 'matches' matcher for regex patterns."
- Grep AC#9 verifies message string exists unchanged
- Purpose: Guides users to correct matcher when regex detected in `contains`

#### 5. Backward Compatibility - Schema and Exit Code Preservation

**JSON Output Schema** (no changes):
```python
# summary dict structure unchanged
summary = {
    "total": ...,
    "passed": ...,
    "manual": ...,
    "failed": ...
}
```
- Grep AC#10 verifies schema keys unchanged
- Consumer: `tools/verify-logs.py` depends on this structure

**Exit Code Logic** (no changes):
```python
# Exit code behavior unchanged
return 0 if failed == 0 else 1
```
- Grep AC#11 verifies exit logic unchanged
- Consumer: PHASE-6.md workflow relies on exit codes (0=pass, 1=fail)

### Data Structures

**No new data structures required**. Existing `ACDefinition` class already supports Method and Expected columns.

**ACDefinition class** (unchanged):
```python
class ACDefinition:
    def __init__(self, ac_number: int, description: str, ac_type: str,
                 method: str, matcher: str, expected: str):
        self.ac_number = ac_number
        self.description = description
        self.ac_type = ac_type
        self.method = method      # ← Can contain Glob(pattern) or build command
        self.matcher = matcher
        self.expected = expected  # ← Can be "-" to trigger Method column usage
```

### Risk Mitigation

| Risk | Likelihood | Impact | Mitigation Strategy |
|------|:----------:|:------:|---------------------|
| Glob regex extraction fails on edge cases | Low | Medium | Fallback to Expected column preserved. Test negative case with invalid syntax. |
| Method command parsing breaks existing ACs | Very Low | High | Conditional on Expected=`-` only. All existing ACs have Expected!=`-` so unchanged. AC#7 verifies existing tests pass. |
| F621 fixes accidentally modified | Very Low | High | Zero-touch approach: no code changes to F621 sections. AC#8-9 verify via grep. |
| JSON schema change breaks verify-logs.py | Very Low | Critical | No modifications to summary dict structure. AC#10 verifies schema unchanged. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Implement Glob pattern extraction in verify_file_ac() | [x] |
| 2 | 5 | Create test_ac_verifier_method_glob.py | [x] |
| 3 | 2 | Run exists matcher test (method_glob.py) | [x] |
| 4 | 3 | Implement Method column parsing in verify_build_ac() | [x] |
| 5 | 6 | Create test_ac_verifier_method_build.py | [x] |
| 6 | 4 | Run build matcher test (method_build.py) | [x] |
| 7 | 8 | Verify F621 regex detection preserved | [x] |
| 8 | 9 | Verify F621 error message preserved | [x] |
| 9 | 10 | Verify JSON schema unchanged | [x] |
| 10 | 11 | Verify exit code semantics preserved | [x] |
| 11 | 7 | Run all existing tests for backward compatibility | [x] |

<!-- AC:Task 1:1 Rule: Each AC maps to specific Tasks for implementation and verification -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-26 10:25 | START | implementer | Task 1: Implement Glob pattern extraction | - |
| 2026-01-26 10:25 | END | implementer | Task 1: Implement Glob pattern extraction | SUCCESS |
| 2026-01-26 10:28 | START | implementer | Task 2: Create test_ac_verifier_method_glob.py | - |
| 2026-01-26 10:28 | END | implementer | Task 2: Create test_ac_verifier_method_glob.py | SUCCESS |
| 2026-01-26 10:30 | START | implementer | Task 4: Implement Method column parsing | - |
| 2026-01-26 10:30 | END | implementer | Task 4: Implement Method column parsing | SUCCESS |
| 2026-01-26 10:39 | START | implementer | Task 5: Create test_ac_verifier_method_build.py | - |
| 2026-01-26 10:39 | END | implementer | Task 5: Create test_ac_verifier_method_build.py | SUCCESS |
| 2026-01-26 10:50 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION: testing SKILL.md not updated |
| 2026-01-26 10:55 | DEVIATION | Bash | verify-logs.py | exit code 1: Reference section old AC#10 regex pattern |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Execution Phases

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Task 1 | Glob pattern extraction in verify_file_ac() |
| 2 | implementer | sonnet | Task 2 | Test file test_ac_verifier_method_glob.py |
| 3 | ac-tester | haiku | Task 3 | AC#2 verification (exists test passes) |
| 4 | implementer | sonnet | Task 4 | Method parsing in verify_build_ac() |
| 5 | implementer | sonnet | Task 5 | Test file test_ac_verifier_method_build.py |
| 6 | ac-tester | haiku | Task 6 | AC#4 verification (build test passes) |
| 7 | ac-tester | haiku | Task 7 | AC#8 verification (F621 regex detection) |
| 8 | ac-tester | haiku | Task 8 | AC#9 verification (F621 error message) |
| 9 | ac-tester | haiku | Task 9 | AC#10 verification (JSON schema) |
| 10 | ac-tester | haiku | Task 10 | AC#11 verification (exit code semantics) |
| 11 | ac-tester | haiku | Task 11 | AC#7 verification (regression tests) |

### Constraints

From Technical Design:

1. **Backward Compatibility**: All changes use fallback pattern (new behavior only when special markers present)
2. **JSON Schema Stability**: No changes to `summary.total/passed/manual/failed` structure (Critical Constraint from Dependencies section)
3. **Exit Code Semantics**: No changes to `sys.exit(1)` conditions (0=pass, 1=fail)
4. **F621 Preservation**: Zero changes to `_contains_regex_metacharacters()` function and related error messages
5. **Method Column Fallback**: verify_file_ac() falls back to Expected column if Glob pattern extraction fails
6. **Expected Column Fallback**: verify_build_ac() uses Expected column when Expected != "-"

### Pre-conditions

- F621 completed (Pattern Parsing Fundamental Fix - Predecessor dependency)
- Python 3 environment available with pytest
- `tools/ac-static-verifier.py` exists with current implementation
- `tools/tests/` directory exists for test files
- All existing ac-static-verifier tests passing (baseline)

### Implementation Steps

#### Phase 1: Glob Pattern Extraction (Task 1, AC#1)

**Target**: `verify_file_ac()` function in `tools/ac-static-verifier.py`

**Location**: File existence check section (after Grep content check branch, before glob pattern detection statement)

**Implementation**:
```python
# Check if Method contains Glob(pattern) syntax
if "Glob(" in ac.method:
    import re
    match = re.search(r'Glob\((.*?)\)', ac.method)
    if match:
        file_path = match.group(1)
    else:
        file_path = ac.expected  # Fallback
else:
    file_path = ac.expected  # Traditional behavior
```

**Verification**: Grep for `Glob\(` pattern in ac-static-verifier.py (AC#1)

#### Phase 2: Create Glob Test File (Task 2, AC#5)

**Target**: Create `tools/tests/test_ac_verifier_method_glob.py`

**Test Cases Required**:
1. **Positive**: Method=`Glob(tools/*.py)`, Expected=`-`, matcher=`exists` → PASS (files exist)
2. **Negative**: Method=`Glob(nonexistent/*.foo)`, Expected=`-`, matcher=`exists` → FAIL (no files)
3. **Backward Compat**: Method=`(empty)`, Expected=`tools/ac-static-verifier.py`, matcher=`exists` → PASS (traditional format)

**Verification**: Glob for test_ac_verifier_method_glob.py (AC#5)

#### Phase 3: Run Glob Test (Task 3, AC#2)

**Target**: Execute pytest for new glob test file

**Command**: `pytest tools/tests/test_ac_verifier_method_glob.py -v`

**Expected**: All test cases pass

**Verification**: AC#2 succeeds matcher

#### Phase 4: Method Column Parsing (Task 4, AC#3)

**Target**: `verify_build_ac()` function in `tools/ac-static-verifier.py`

**Location**: Command extraction section (line 325, where `build_command` is assigned)

**Implementation**:
```python
# If Expected is "-", use Method column for command
if ac.expected.strip() == "-":
    build_command = ac.method.strip()
else:
    build_command = ac.expected.strip()  # Traditional behavior
```

**Verification**: Grep for `ac.method` in ac-static-verifier.py (AC#3)

#### Phase 5: Create Build Test File (Task 5, AC#6)

**Target**: Create `tools/tests/test_ac_verifier_method_build.py`

**Test Cases Required**:
1. **Positive**: Method=`echo test`, Expected=`-`, matcher=`succeeds` → PASS (exit code 0)
2. **Negative**: Method=`exit 1`, Expected=`-`, matcher=`succeeds` → FAIL (exit code 1)
3. **Backward Compat**: Method=`(empty)`, Expected=`echo test`, matcher=`succeeds` → PASS (traditional format)

**Verification**: Glob for test_ac_verifier_method_build.py (AC#6)

#### Phase 6: Run Build Test (Task 6, AC#4)

**Target**: Execute pytest for new build test file

**Command**: `pytest tools/tests/test_ac_verifier_method_build.py -v`

**Expected**: All test cases pass

**Verification**: AC#4 succeeds matcher

#### Phase 7-10: F621 Preservation and Schema Verification (Tasks 7-10, AC#8-11)

**Target**: Verify F621 fixes and schema stability unchanged

**Verification Steps**:
1. Grep `_contains_regex_metacharacters` in `tools/ac-static-verifier.py` → Must exist (AC#8)
2. Grep `Use 'matches' matcher` in `tools/ac-static-verifier.py` → Must exist (AC#9)
3. Grep `summary.*total.*passed.*manual.*failed` in `tools/ac-static-verifier.py` → Must exist (AC#10)
4. Grep `return 0 if failed == 0 else 1` in `tools/ac-static-verifier.py` → Must exist (AC#11)

**Expected**: All 4 grep patterns found

#### Phase 11: Regression Test (Task 11, AC#7)

**Target**: Verify all existing tests still pass

**Command**: `pytest tools/tests/test_ac_verifier*.py -v`

**Expected**: All existing tests pass (no breakage)

**Verification**: AC#7 succeeds matcher

### Build Verification

**After Each Implementation Phase**:
- Phase 1 (Task 1): No build required (Python script)
- Phase 2 (Task 2): Run `pytest tools/tests/test_ac_verifier_method_glob.py -v` to verify test structure
- Phase 4 (Task 4): No build required (Python script)
- Phase 5 (Task 5): Run `pytest tools/tests/test_ac_verifier_method_build.py -v` to verify test structure

**Final Verification**:
- Run `pytest tools/tests/test_ac_verifier*.py -v` to verify all tests pass

### Success Criteria

1. All 8 Tasks completed
2. All 11 ACs verified:
   - AC#1: Glob pattern extraction code exists
   - AC#2: exists matcher test passes
   - AC#3: Method column parsing code exists
   - AC#4: build matcher test passes
   - AC#5: test_ac_verifier_method_glob.py created
   - AC#6: test_ac_verifier_method_build.py created
   - AC#7: Existing tests still pass
   - AC#8-11: F621 preservation and schema stability confirmed
3. New test files created and passing
4. Existing tests still passing (backward compatibility maintained)
5. F621 fixes preserved
6. JSON schema and exit code unchanged

### Error Handling

| Error | Action |
|-------|--------|
| Glob regex extraction fails | STOP → Fallback to Expected column is already in code, investigate edge case |
| Build command parsing breaks existing ACs | STOP → Report to user, do not proceed (Critical Constraint violation) |
| New tests fail after creation | STOP → Report to user, debug test case logic |
| Existing tests fail (regression) | STOP → Report to user, revert changes immediately |
| F621 code accidentally modified | STOP → Report to user, revert changes immediately |
| JSON schema or exit code changed | STOP → Report to user, revert changes (Critical Constraint violation) |

### Rollback Plan

**If issues arise after implementation**:

1. **Immediate Rollback Trigger**: Any of these conditions:
   - Existing tests fail (regression detected)
   - F621 fixes broken (`_contains_regex_metacharacters()` or error messages changed)
   - JSON schema changed (verify-logs.py breaks)
   - Exit code semantics changed (PHASE-6.md workflow breaks)

2. **Rollback Steps**:
   ```bash
   # Revert the commit
   git revert HEAD

   # Verify rollback successful
   pytest tools/tests/test_ac_verifier*.py -v

   # Check F621 fixes intact
   grep "_contains_regex_metacharacters" tools/ac-static-verifier.py
   grep "Use 'matches' matcher" tools/ac-static-verifier.py
   ```

3. **Post-Rollback Actions**:
   - Create follow-up feature for root cause investigation
   - Document rollback reason in feature-626.md
   - Notify user of rollback and follow-up feature ID

4. **Partial Rollback** (if only one matcher affected):
   - Rollback specific function change (`verify_file_ac()` OR `verify_build_ac()`)
   - Keep passing enhancements
   - Document limitation in feature-626.md

---

## Review Notes
- [resolved-applied] Phase1-Uncertain iter1: AC#7 Method `pytest tools/tests/test_ac_verifier*.py` uses glob pattern which may not expand correctly in all environments when passed to subprocess
- [resolved-applied] Phase1-Uncertain iter1: Task#7 covers AC#8,9,10,11 (4 ACs) which violates AC:Task 1:1 principle stated in comments
- [resolved-applied] Phase1-Uncertain iter4: Implementation Contract Phase 7 naming inconsistency - 'Implementation Steps' section has 'Phase 7' covering AC#8-11 together but 'Execution Phases' correctly shows Phase 7-10 separately
- [resolved-applied] Phase1-Uncertain iter5: verify_build_ac() integration point lacks line number specification compared to verify_file_ac()
- [resolved-applied] Phase1-Uncertain iter7: AC#10 pattern only verifies summary dict exists but not individual total/passed/manual/failed keys

## Reference (from previous session)

<details>
<summary>Acceptance Criteria, Root Cause Analysis, Related Features, Feasibility, Dependencies (reference)</summary>

### Tasks (reference)

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 2 | Implement Glob pattern extraction in verify_file_ac() | [ ] |
| 2 | 1,7 | Create test_ac_verifier_method_glob.py with test cases | [ ] |
| 3 | 4 | Implement Method column parsing in verify_build_ac() | [ ] |
| 4 | 3,8 | Create test_ac_verifier_method_build.py with test cases | [ ] |
| 5 | 5,6,10,11 | Verify F621 preservation and schema stability | [ ] |
| 6 | 9 | Run all existing tests to verify backward compatibility | [ ] |

### Implementation Contract (reference)

**Execution Phases**:

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Task 1 | Glob pattern extraction logic in verify_file_ac() |
| 2 | implementer | sonnet | Task 2 | Test file test_ac_verifier_method_glob.py |
| 3 | implementer | sonnet | Task 3 | Method parsing logic in verify_build_ac() |
| 4 | implementer | sonnet | Task 4 | Test file test_ac_verifier_method_build.py |
| 5 | ac-tester | haiku | Task 5 | Verification of F621 preservation (AC#5-6, 10-11) |
| 6 | ac-tester | haiku | Task 6 | All existing tests pass (AC#9) |

**Constraints**: Backward Compatibility, JSON Schema Stability, Exit Code Semantics, F621 Preservation

**Pre-conditions**: F621 completed, Python 3 environment, tools/ac-static-verifier.py exists

**Build Verification**: pytest after Tasks 2,4; all tests after completion

**Success Criteria**: All 6 Tasks completed, All 11 ACs verified, F621 fixes preserved

### Technical Design (reference)

**Note**: This Technical Design section was from a previous session and has been superseded by the new `<!-- fc-phase-4-completed -->` marked section above.

#### Approach (reference)

**Column-Aware Parsing Strategy**: Extend `verify_file_ac()` and `verify_build_ac()` to intelligently extract parameters from either Method or Expected columns based on AC definition patterns.

This approach maintains backward compatibility (existing ACs continue to work) while supporting new patterns where Method column contains the primary data (e.g., `Glob(pattern)`, `dotnet build`) and Expected is `-`.

**Why this approach**:
1. **Non-breaking**: Falls back to Expected column when Method doesn't contain special patterns
2. **Pattern-driven**: Uses regex to detect `Glob(...)` syntax in Method column
3. **Consistent with F621**: Preserves all F621 fixes (regex metacharacter detection, error messages)
4. **Testable**: Clear input/output behavior for unit tests

#### AC Coverage (reference)

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `test_ac_verifier_method_glob.py` with test case: Method=`Glob(tools/*.py)`, Expected=`-`, matcher=`exists` → expect PASS when files exist |
| 2 | Add `Glob(` pattern detection in `verify_file_ac()` file existence check section: `if "Glob(" in ac.method: file_path = extract_glob_pattern(ac.method) else: file_path = ac.expected` |
| 3 | Create `test_ac_verifier_method_build.py` with test case: Method=`echo test`, Expected=`-`, matcher=`succeeds` → expect PASS |
| 4 | Modify `verify_build_ac()` command extraction section: `if ac.expected.strip() == "-": build_command = ac.method.strip() else: build_command = ac.expected.strip()` |
| 5 | No change needed - `_contains_regex_metacharacters()` function remains untouched (F621 code preserved) |
| 6 | No change needed - error message "Use 'matches' matcher for regex patterns." remains in code |
| 7 | Create new file `tools/tests/test_ac_verifier_method_glob.py` with pytest test cases |
| 8 | Create new file `tools/tests/test_ac_verifier_method_build.py` with pytest test cases |
| 9 | Run `pytest tools/tests/test_ac_verifier*.py -v` in AC verification - all existing tests must pass |
| 10 | No change to JSON output code - grep verification confirms `summary.total/passed/manual/failed` unchanged |
| 11 | No change to exit code logic - grep verification confirms `sys.exit(1) if summary['failed'] > 0` unchanged |

#### Key Decisions (reference)

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **Where to parse Glob pattern** | A) Method column only<br>B) Expected column only<br>C) Both with fallback | **C** | Backward compatibility: existing ACs use Expected, new ACs use Method. Fallback ensures no breakage. |
| **Glob pattern extraction** | A) Regex `Glob\((.*?)\)`<br>B) String split on `(` and `)`<br>C) Full parser | **A** | Regex is simple, robust for `Glob(pattern)` format. No need for complex parser. |
| **Build command fallback** | A) Expected=`-` triggers Method<br>B) Method always takes precedence<br>C) Expected always takes precedence | **A** | Explicit marker (`-`) indicates intention. Avoids ambiguity when both columns have content. |
| **F621 preservation** | A) Refactor regex detection<br>B) Leave F621 code untouched | **B** | F621 is stable and tested. Zero risk approach. |
| **Test structure** | A) Add to existing test files<br>B) Create new test files | **B** | Clear separation of concerns. Each enhancement has dedicated regression test file. |

#### Implementation Details (reference)

##### 1. Glob Pattern Extraction (AC#1-2, AC#7)

**Location**: `verify_file_ac()` function, file existence check section (after Grep check branch)

**Current Code**:
```python
file_path = ac.expected
```

**Enhanced Code**:
```python
# Check if Method contains Glob(pattern) syntax
if "Glob(" in ac.method:
    # Extract pattern from Glob(pattern)
    import re
    match = re.search(r'Glob\((.*?)\)', ac.method)
    if match:
        file_path = match.group(1)
    else:
        # Fallback to Expected if extraction fails
        file_path = ac.expected
else:
    # Traditional behavior: use Expected column
    file_path = ac.expected
```

**Rationale**:
- Simple string check `"Glob(" in ac.method` for performance
- Regex `Glob\((.*?)\)` captures pattern between parentheses
- Fallback to Expected preserves backward compatibility
- Non-greedy `.*?` handles edge cases like `Glob(foo/*.cs)`

##### 2. Build Command from Method (AC#3-4, AC#8)

**Location**: `verify_build_ac()` function, command extraction section (near top of function)

**Current Code**:
```python
build_command = ac.expected.strip()
```

**Enhanced Code**:
```python
# If Expected is "-", use Method column for command
if ac.expected.strip() == "-":
    build_command = ac.method.strip()
else:
    # Traditional behavior: use Expected column
    build_command = ac.expected.strip()
```

**Rationale**:
- Explicit marker `-` indicates "use Method column"
- No ambiguity: either Expected has command OR Method has command
- Backward compatible: existing ACs with Expected=`dotnet build` continue to work

##### 3. Test Files (AC#7, AC#8)

**File**: `tools/tests/test_ac_verifier_method_glob.py`
```python
"""
Test cases for exists matcher with Glob(pattern) in Method column
"""
# Positive: Glob pattern matches files → PASS
# Negative: Glob pattern matches no files → FAIL
# Edge: Invalid Glob syntax → FAIL with error
```

**File**: `tools/tests/test_ac_verifier_method_build.py`
```python
"""
Test cases for build matcher with command in Method column
"""
# Positive: Method=`echo test`, Expected=`-` → PASS (exit code 0)
# Negative: Method=`exit 1`, Expected=`-` → FAIL (exit code 1)
# Backward compat: Expected=`echo test` (old format) → PASS
```

##### 4. F621 Preservation (AC#5-6)

**No code changes required**. Grep verification confirms:
- AC#5: `_contains_regex_metacharacters` function exists and unchanged
- AC#6: Error message "Use 'matches' matcher for regex patterns." exists

##### 5. Backward Compatibility (AC#9-11)

**Strategy**:
- All changes use fallback pattern (new behavior only when special markers present)
- JSON output schema untouched (no changes to `summary` dict structure)
- Exit code logic untouched (no changes to `sys.exit(1)` conditions)
- Existing tests run without modification

#### Data Structures / Interfaces (reference)

**No new interfaces required**. Changes are internal to existing functions.

**AC dataclass** (already exists, no changes):
```python
@dataclass
class AC:
    ac_number: int
    description: str
    type: str
    method: str      # ← Now can contain Glob(pattern) or build command
    matcher: str
    expected: str    # ← Can be "-" to indicate "use Method"
    status: str
```

#### Risk Mitigation (reference)

| Risk | Mitigation |
|------|-----------|
| Glob extraction regex fails on edge cases | Fallback to Expected column + add negative test case |
| Build command from Method allows dangerous commands | No change to security model (feature files are trusted per F621) |
| Existing ACs break | All changes use fallback pattern + AC#9 verifies all existing tests pass |
| F621 fixes accidentally broken | Zero changes to F621 code + AC#5-6 verify via grep |

### Acceptance Criteria (reference)

**Note**: This AC section was from a previous session and has been superseded by the new `<!-- fc-phase-3-completed -->` marked section above.

#### Philosophy Derivation (reference)

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "AC 検証の完全自動化" (complete automation) | All supported AC types must be handled without manual fallback | AC#1, AC#2, AC#3, AC#4, AC#5, AC#6 |
| "ac-static-verifier が全 AC タイプを正しく処理" (must correctly process all AC types) | `exists` matcher must parse `Glob(pattern)` from Method column | AC#1, AC#2, AC#7 |
| "ac-static-verifier が全 AC タイプを正しく処理" (must correctly process all AC types) | `build` matcher must parse command from Method column when Expected=`-` | AC#3, AC#4, AC#8 |
| "/run ワークフローの信頼性が向上" (reliability improvement) | Regression tests must prevent future breakage | AC#7, AC#8, AC#9 |
| "F621 設計通り" (per F621 design) | `contains` + regex pattern behavior must be preserved | AC#5, AC#6 |

#### AC Definition Table (reference - ARCHIVED, not for verification)

<!-- ARCHIVED: This table is from a previous session and should NOT be parsed by ac-static-verifier.
The current AC Definition Table is in the main section above marked with fc-phase-3-completed.
| AC# | Description | Type | Method | Matcher | Expected | Status |
| 1 | exists matcher parses Glob(pattern) from Method (Pos) | test | pytest | succeeds | - | [ ] |
| 2 | exists matcher file found returns PASS | code | Grep(tools/ac-static-verifier.py) | contains | "Glob\\(" | [ ] |
... (truncated for brevity)
-->

### Root Cause Analysis (reference)

#### 5 Whys

1. **Why**: F625 Phase 6 で ac-static-verifier が 10/14 FAIL を報告したが手動検証は全 PASS
2. **Why**: ツールが `exists` と `build` matcher の AC 定義を正しく解析できなかった
3. **Why**: ツールは Expected 列からファイルパス/コマンドを取得する設計だが、AC 定義は Method 列にパターン/コマンドを記載
4. **Why**: AC 定義フォーマットのバリエーション（Method 列 vs Expected 列の使い分け）が増えた
5. **Why**: 元の設計時に全 matcher タイプの列使用パターンを網羅していなかった

#### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| `exists` matcher で `file_path: "-"` | Expected=`-` から取得しているが、`Glob(pattern)` は Method 列にある |
| `build` matcher で `command: "-"` | Expected=`-` から取得しているが、コマンド `dotnet build` は Method 列にある |
| `contains` + `\\[DONE\\]` で regex error | 正しい動作（F621 設計）だが、ユーザーへのガイダンスが不十分 |

#### Conclusion

根本原因は **matcher タイプごとの列使用規則の不統一**：
- `contains/matches`: Expected 列にパターン（正しく動作）
- `exists`: Method 列に `Glob(pattern)`、Expected は `-`（未対応）
- `build`: Method 列にコマンド、Expected は `-`（未対応）

ツールを修正して Method 列からも情報を抽出できるようにする。

### Related Features (reference)

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F621 | [DONE] | Predecessor | Pattern Parsing Fundamental Fix - regex escape 処理等を修正 |
| F618 | [DONE] | Related | MANUAL Status Counting Fix - MANUAL カウント修正 |
| F625 | [WIP] | Trigger | Post-Phase Review で問題発見 |

### Feasibility Assessment (reference)

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | パターンは明確、列解析ロジックの拡張で対応可能 |
| Scope is realistic | YES | 2-3 箇所の条件分岐追加 + テスト追加 |
| No blocking constraints | YES | F621 完了済み、基盤は整備済み |

**Verdict**: FEASIBLE

### Dependencies (reference)

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F621 | [DONE] | Pattern Parsing Fundamental Fix |

</details>
