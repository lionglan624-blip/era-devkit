# Feature 067: K3 パチュリー COM統合

## Status: [DONE]

## Background

- **Original problem**: K3 (パチュリー) still has fragmented kojo file structure
- **Solution**: Apply COM category-based reorganization pattern from Feature 057/065
- **Execution**: Via kojo-refactor subagent

## Overview

Reorganize K3 (パチュリー) kojo files into COM category-based structure using `tools/reorganize_kojo.py`.

## Scope

### In Scope
- Run `reorganize_kojo.py --char K3`
- Verify with ErbLinter, kojo-mapper, Headless test
- Delete original files after verification

### Out of Scope
- Content changes to kojo text
- NTR口上 files (keep as-is)

## Acceptance Criteria

- [x] `reorganize_kojo.py --char K3` succeeds
- [x] Build succeeds (0 errors)
- [x] ErbLinter passes for K3 files (304 functions, 0 errors)
- [x] kojo-mapper function count maintained (43 = 43)
- [x] Headless kojo test passes (KOJO_MESSAGE_COM_K3_312)

## Subagent Execution

```
Use the kojo-refactor subagent to process K3
```

## Implementation Notes

K3 has unique structure compared to K1/K4:
- No `KOJO_K3.ERB` - uses `対あなた口上.ERB` + `KOJO_K3_NTR拡張.ERB`
- Has 日常 category (COM 463) - not present in K1/K4
- No 愛撫 category (COM 0-9, 20-21, 40-48 don't exist)

Files created:
- KOJO_K3_EVENT.ERB (328 lines, 12 functions)
- KOJO_K3_会話親密.ERB (1240 lines, 25 functions)
- KOJO_K3_日常.ERB (111 lines, 6 functions)

Files deleted:
- 対あなた口上.ERB
- KOJO_K3_NTR拡張.ERB

## Links

- [feature-065.md](feature-065.md) - Tooling reference
- [kojo-refactor.md](../../.claude/agents/kojo-refactor.md) - Subagent
