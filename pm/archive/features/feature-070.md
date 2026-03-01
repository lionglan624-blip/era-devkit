# Feature 070: K7 子悪魔 COM統合

## Status: [DONE]

## Background

- **Original problem**: K7 (子悪魔) still has fragmented kojo file structure
- **Solution**: Apply COM category-based reorganization pattern from Feature 057/065
- **Execution**: Via kojo-refactor subagent

## Overview

Reorganize K7 (子悪魔) kojo files into COM category-based structure using `tools/reorganize_kojo.py`.

## Scope

### In Scope
- Run `reorganize_kojo.py --char K7` (auto-fixes missing RETURN in reorganized files)
- Run `reorganize_kojo.py --char K7 --fix-preserved` (fixes missing RETURN in NTR口上, SexHara, WC系)
- Run `reorganize_kojo.py --char K7 --verify` (headless test with error detection)
- Verify with ErbLinter, kojo-mapper
- Delete original files after verification

### Out of Scope
- Content changes to kojo text

## Acceptance Criteria

- [x] `reorganize_kojo.py --char K7` succeeds
- [x] `reorganize_kojo.py --char K7 --fix-preserved` scans preserved files
- [x] Build succeeds (0 errors)
- [x] ErbLinter passes for K7 files
- [x] kojo-mapper function count maintained
- [x] `reorganize_kojo.py --char K7 --verify` passes (✓ PASS)

## Subagent Execution

```
Use the kojo-refactor subagent to process K7
```

## Links

- [feature-065.md](feature-065.md) - Tooling reference
- [kojo-refactor.md](../../.claude/agents/kojo-refactor.md) - Subagent
