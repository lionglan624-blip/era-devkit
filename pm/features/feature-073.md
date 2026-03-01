# Feature 073: K10 魔理沁ECOM統吁E

## Status: [DONE]

## Background

- **Original problem**: K10 (魔理沁E still has fragmented kojo file structure
- **Solution**: Apply COM category-based reorganization pattern from Feature 057/065
- **Execution**: Via kojo-refactor subagent

## Overview

Reorganize K10 (魔理沁E kojo files into COM category-based structure using `tools/reorganize_kojo.py`.

## Scope

### In Scope
- Run `reorganize_kojo.py --char K10` (auto-fixes missing RETURN in reorganized files)
- Run `reorganize_kojo.py --char K10 --fix-preserved` (fixes missing RETURN in NTR口丁E SexHara, WC系)
- Run `reorganize_kojo.py --char K10 --verify` (headless test with error detection)
- Verify with ErbLinter, kojo-mapper
- Delete original files after verification

### Out of Scope
- Content changes to kojo text

## Acceptance Criteria

- [x] `reorganize_kojo.py --char K10` succeeds
- [x] `reorganize_kojo.py --char K10 --fix-preserved` scans preserved files
- [x] Build succeeds (0 errors)
- [x] ErbLinter passes for K10 files
- [x] kojo-mapper function count maintained
- [x] `reorganize_kojo.py --char K10 --verify` passes (✁EPASS)

## Subagent Execution

```
Use the kojo-refactor subagent to process K10
```

## Links

- [feature-065.md](feature-065.md) - Tooling reference
