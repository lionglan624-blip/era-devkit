# Feature 285: tools/kojo-mapper/kojo_test_gen.py COM_FILE_MAP 完全同期

## Status: [DONE]

## Type: infra

## Background

### Philosophy
ツールとドキュメントの整合性を維持し、新規 COM 追加時の抜け漏れを防止する

### Problem
Feature 282 で COM 80 のテスト生成時に、`tools/kojo-mapper/kojo_test_gen.py` の `COM_FILE_MAP` に複数の COM 範囲が欠落していることが判明した。具体的には COM 20-21, 40-48, 300-316, 350-352, 410-415, 463 が未登録。

現状の COM_FILE_MAP:
- 0-11 愛撫系: 完了 (0-6, 8-9 → `_愛撫.ERB`、7,10,11 → `_乳首責め.ERB`)
- 20-21 キス系: **欠落** → `_愛撫.ERB` に追加必要
- 40-48 道具系: **欠落** → `_愛撫.ERB` に追加必要
- 60-72 挿入系: 完了
- 80-203 口挿入系: 完了 (既存)
- 300-316, 350-352 会話親密系: **欠落** → `_会話親密.ERB` に追加必要
- 410-415, 463 日常系: **欠落** → `_日常.ERB` に追加必要 (K7 は stub 作成必要)

### Goal
1. COM_FILE_MAP に欠落している COM 範囲を追加
2. COM_FILE_MAP → ERB ファイル存在検証メカニズム導入

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | COM 20-21 (キス系) マッピング追加 | code | Grep(kojo_test_gen.py) | contains | `range(20, 22)` | [x] |
| 2 | COM 40-48 (道具系) マッピング追加 | code | Grep(kojo_test_gen.py) | contains | `range(40, 49)` | [x] |
| 3 | COM 300-316, 350-352 (会話親密) マッピング追加 | code | Grep(kojo_test_gen.py) | contains | `range(300, 317)` + `range(350, 353)` | [x] |
| 4 | COM 410-415, 463 (日常系) マッピング追加 | code | Grep(kojo_test_gen.py) | contains | `range(410, 416)` | [x] |
| 5 | ERB ファイル存在検証スクリプト作成 | exit_code | verify_com_map.py | succeeds | exit 0 | [x] |
| 6 | pre-commit hook で COM_FILE_MAP 整合性チェック | code | Grep(pre-commit) | contains | "verify_com_map" | [x] |

### AC Details

**AC 1**: `Grep(pattern='range(20, 22)', path="tools/kojo-mapper/kojo_test_gen.py")` → matches
**AC 2**: `Grep(pattern='range(40, 49)', path="tools/kojo-mapper/kojo_test_gen.py")` → matches
**AC 3**: `Grep(pattern='range(300, 317)', path="tools/kojo-mapper/kojo_test_gen.py")` → matches
**AC 3b**: `Grep(pattern='range(350, 353)', path="tools/kojo-mapper/kojo_test_gen.py")` → matches
**AC 4**: `Grep(pattern='range(410, 416)', path="tools/kojo-mapper/kojo_test_gen.py")` → matches

**AC 5**: `python tools/kojo-mapper/verify_com_map.py` → exit 0
- Verifies: For each erb_suffix in COM_FILE_MAP, check K1-K10 all have corresponding ERB file
- Note: File-level verification only (does not check function existence within files)
- Skip conditions (documented known missing files):
  ```python
  SKIP_COMBINATIONS = {
      ("K7", "_日常.ERB"),        # K7 lacks KOJO_K7_日常.ERB
      ("K5", "_乳首責め.ERB"),    # K5 lacks KOJO_K5_乳首責め.ERB
      ("K6", "_乳首責め.ERB"),    # K6 lacks KOJO_K6_乳首責め.ERB
      ("K7", "_乳首責め.ERB"),    # K7 lacks KOJO_K7_乳首責め.ERB
      ("K8", "_乳首責め.ERB"),    # K8 lacks KOJO_K8_乳首責め.ERB
      ("K10", "_乳首責め.ERB"),   # K10 lacks KOJO_K10_乳首責め.ERB
  }
  ```
- On error: Prints "ERROR: COM {N} K{X} → {path} (missing)" and exits 1
- On success: Prints "OK: All {N} COM mappings verified" and exits 0

**AC 6**: `Grep(pattern="verify_com_map", path=".githooks/pre-commit")` → matches
- Hook order: New step 3 of 4 (between ErbLinter and verify-logs.py)
- Invocation: `python tools/kojo-mapper/verify_com_map.py`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | COM_FILE_MAP に COM 20-21 (キス系) を追加 | [O] |
| 2 | 2 | COM_FILE_MAP に COM 40-48 (道具系) を追加 | [O] |
| 3 | 3 | COM_FILE_MAP に COM 300-316, 350-352 (会話親密) を追加 | [O] |
| 4 | 4 | COM_FILE_MAP に COM 410-415, 463 (日常系) を追加 | [O] |
| 5 | 5 | tools/kojo-mapper/verify_com_map.py 作成 (COM_FILE_MAP → ERB ファイル存在検証) | [O] |
| 6 | 6 | pre-commit に COM マッピング検証追加 (Task 5 完了後、ステップ番号更新: [1/3]→[1/4]等) | [O] |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-01 09:49 | START | implementer | Task 1-6 | - |
| 2026-01-01 09:49 | END | implementer | Task 1-6 | SUCCESS |

---

## Links
- [index-features.md](index-features.md)
- [kojo-writing SKILL](../../.claude/skills/kojo-writing/SKILL.md)
- 発見契機: [feature-282.md](feature-282.md) (COM 80 実装時に問題発見)
