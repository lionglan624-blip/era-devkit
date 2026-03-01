# Feature 072: K9 大妖精 COM統吁E

## Status: [DONE]

## Background

- **Original problem**: K9 (大妖精) still has fragmented kojo file structure
- **Solution**: Apply COM category-based reorganization pattern from Feature 057/065
- **Execution**: Via kojo-refactor subagent

## Overview

Reorganize K9 (大妖精) kojo files into COM category-based structure using `tools/reorganize_kojo.py`.

## Scope

### In Scope
- Run `reorganize_kojo.py --char K9` (auto-fixes missing RETURN in reorganized files)
- Run `reorganize_kojo.py --char K9 --fix-preserved` (fixes missing RETURN in NTR口丁E SexHara, WC系)
- Run `reorganize_kojo.py --char K9 --verify` (headless test with error detection)
- Verify with ErbLinter, kojo-mapper
- Delete original files after verification

### Out of Scope
- Content changes to kojo text

## Acceptance Criteria

- [x] `reorganize_kojo.py --char K9` succeeds
- [x] `reorganize_kojo.py --char K9 --fix-preserved` scans preserved files
- [x] Build succeeds (0 errors)
- [x] ErbLinter passes for K9 files
- [x] kojo-mapper function count maintained (326 functions)
- [x] `reorganize_kojo.py --char K9 --verify` passes (PASS)

## Subagent Execution

```
Use the kojo-refactor subagent to process K9
```

## Links

- [feature-065.md](feature-065.md) - Tooling reference
- [kojo-refactor.md](../../../archive/claude_legacy_20251230/agents/kojo-refactor.md) - Subagent
