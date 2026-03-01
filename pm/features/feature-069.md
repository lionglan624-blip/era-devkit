# Feature 069: K6 フラン COM統吁E

## Status: [DONE]

## Background

- **Original problem**: K6 (フラン) still has fragmented kojo file structure
- **Solution**: Apply COM category-based reorganization pattern from Feature 057/065
- **Execution**: Via kojo-refactor subagent

## Overview

Reorganize K6 (フラン) kojo files into COM category-based structure using `tools/reorganize_kojo.py`.

## Scope

### In Scope
- Run `reorganize_kojo.py --char K6` (auto-fixes missing RETURN in reorganized files)
- Run `reorganize_kojo.py --char K6 --fix-preserved` (fixes missing RETURN in NTR口丁E SexHara, WC系)
- Run `reorganize_kojo.py --char K6 --verify` (headless test with error detection)
- Verify with ErbLinter, kojo-mapper
- Delete original files after verification

### Out of Scope
- Content changes to kojo text

## Acceptance Criteria

- [x] `reorganize_kojo.py --char K6` succeeds
- [x] `reorganize_kojo.py --char K6 --fix-preserved` scans preserved files
- [x] Build succeeds (0 errors)
- [x] ErbLinter passes for K6 files
- [x] kojo-mapper function count maintained
- [x] `reorganize_kojo.py --char K6 --verify` passes (PASS)

## Results

### Files Created
- `KOJO_K6_EVENT.ERB` (299 lines, 12 functions)
- `KOJO_K6_会話親寁EERB` (958 lines, 17 functions)
- `KOJO_K6_日常.ERB` (107 lines, 6 functions)

### Files Deleted
- `KOJO_K6_NTR拡張.ERB` (172 lines)
- `対あなた口丁EERB` (1,205 lines)

### Bug Fixes Applied
- 6 missing RETURNs added to `NTR口丁EERB`
- Function name typo fixed: `K7_463_1` ↁE`K6_463_1`
- Tool fix: reorganize_kojo.py Windows encoding issue

## Links

- [feature-065.md](feature-065.md) - Tooling reference
