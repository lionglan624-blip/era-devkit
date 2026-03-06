# Feature 074: KU 汎用 COM統合

## Status: [DONE]

## Background

- **Original problem**: KU (汎用) still has fragmented kojo file structure
- **Solution**: Apply COM category-based reorganization pattern from Feature 057/065
- **Execution**: Via kojo-refactor subagent

## Overview

Reorganize KU (汎用) kojo files into COM category-based structure using `tools/reorganize_kojo.py`.

## Scope

### In Scope
- Run `reorganize_kojo.py --char KU` (auto-fixes missing RETURN in reorganized files)
- Run `reorganize_kojo.py --char KU --fix-preserved` (fixes missing RETURN in NTR口上, SexHara, WC系)
- Run `reorganize_kojo.py --char KU --verify` (headless test with error detection)
- Verify with ErbLinter, kojo-mapper
- Delete original files after verification

### Out of Scope
- Content changes to kojo text

## Acceptance Criteria

- [x] `reorganize_kojo.py --char KU` succeeds
- [x] `reorganize_kojo.py --char KU --fix-preserved` scans preserved files
- [x] Build succeeds (0 errors)
- [x] ErbLinter passes for KU files
- [x] kojo-mapper function count maintained (325 functions)
- [x] `reorganize_kojo.py --char KU --verify` passes (✓ PASS)

## Subagent Execution

```
Use the kojo-refactor subagent to process KU
```

## Links

- [feature-065.md](feature-065.md) - Tooling reference
- [kojo-refactor.md](../../.claude/agents/kojo-refactor.md) - Subagent
