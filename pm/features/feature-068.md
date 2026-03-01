# Feature 068: K5 レミリア COM統吁E

## Status: [DONE]

## Background

- **Original problem**: K5 (レミリア) still has fragmented kojo file structure
- **Solution**: Apply COM category-based reorganization pattern from Feature 057/065
- **Execution**: Via kojo-refactor subagent

## Overview

Reorganize K5 (レミリア) kojo files into COM category-based structure using `tools/reorganize_kojo.py`.

## Scope

### In Scope
- Run `reorganize_kojo.py --char K5`
- Verify with ErbLinter, kojo-mapper, Headless test
- Delete original files after verification

### Out of Scope
- Content changes to kojo text
- NTR口丁Efiles (keep as-is)

## Acceptance Criteria

- [x] `reorganize_kojo.py --char K5` succeeds
- [x] Build succeeds (0 errors)
- [x] ErbLinter passes for K5 files
- [x] kojo-mapper function count maintained (54=54)
- [x] Headless kojo test passes

## Subagent Execution

```
Use the kojo-refactor subagent to process K5
```

## Discovered Issues

During K5 execution, the following pre-existing bugs were found:

### Missing RETURN in NTR口丁EERB
- **Functions**: 5 functions without RETURN statement
  - `@NTR_KOJO_K5_15_0` (underwear removal)
  - `@NTR_KOJO_K5_15_1` (rotator V insertion)
  - `@NTR_KOJO_K5_15_2` (rotator A insertion)
  - `@NTR_KOJO_K5_15_3` (rotator AV insertion)
  - `@NTR_KOJO_K5_16_1` (chastity belt key - other)
- **Origin**: Pre-existing in original NTR口丁EERB
- **Impact**: Causes "予期しなぁE��クリプト終端でぁE error in emuera.log
- **Status**: Fixed (5 RETURN statements added)

## Links

- [feature-065.md](feature-065.md) - Tooling reference
- [kojo-refactor.md](../../../archive/claude_legacy_20251230/agents/kojo-refactor.md) - Subagent
