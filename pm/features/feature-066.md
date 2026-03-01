# Feature 066: K2 蟆乗が鬲・COM邨ｱ蜷・

## Status: [DONE]

## Background

- **Original problem**: K2 (蟆乗が鬲・ still has fragmented kojo file structure
- **Solution**: Apply COM category-based reorganization pattern from Feature 057/065
- **Execution**: Via kojo-refactor subagent

## Overview

Reorganize K2 (蟆乗が鬲・ kojo files into COM category-based structure using `tools/reorganize_kojo.py`.

## Scope

### In Scope
- Run `reorganize_kojo.py --char K2`
- Verify with ErbLinter, kojo-mapper, Headless test
- Delete original files after verification

### Out of Scope
- Content changes to kojo text
- NTR蜿｣荳・files (keep as-is)

## Acceptance Criteria

- [x] `reorganize_kojo.py --char K2` succeeds
- [x] Build succeeds (0 errors)
- [x] ErbLinter passes for K2 files
- [x] kojo-mapper function count maintained (332=332)
- [x] Headless kojo test passes

## Subagent Execution

```
Use the kojo-refactor subagent to process K2
```

## Discovered Issues

During K2 execution, the following pre-existing bugs were found:

### 1. Missing RETURN in KOJO_K2_諢帶牒.ERB
- **Function**: `@KOJO_MESSAGE_COM_K2_00` (placeholder function)
- **Origin**: Pre-existing in original KOJO_K2.ERB (line 4763)
- **Status**: Fixed (added `RETURN 0`)

### 2. Missing RETURN in EVENT_MESSAGE_COM.ERB
- **Function**: `@MESSAGE_COM0` (and all 127 MESSAGE_COM functions)
- **Origin**: Pre-existing since initial commit
- **Impact**: Causes "莠域悄縺励↑縺・せ繧ｯ繝ｪ繝励ヨ邨らｫｯ縺ｧ縺・ error in emuera.log
- **Status**: Fixed (128 RETURN statements added)

### 3. Headless Test Gap
- **Problem**: `Status: OK` reported even when errors logged to emuera.log
- **Cause**: Direct function tests don't check emuera.log
- **Workaround**: Use JSON scenarios with `"expect": {"no_errors": true}`
- **Recorded**: Wishlist - Headless error detection enhancement

## Improvement Recommendations

### reorganize_kojo.py Enhancement
Add RETURN statement detection/auto-fix:
```python
# 讀懷・譎ゅ↓隴ｦ蜻・+ 閾ｪ蜍穂ｿｮ豁｣
笞・・Warning: Function @KOJO_MESSAGE_COM_K2_00 has no RETURN statement
   竊・Auto-added: RETURN 0
```

## Links

- [feature-065.md](feature-065.md) - Tooling reference
- [kojo-refactor.md](../../../archive/claude_legacy_20251230/agents/kojo-refactor.md) - Subagent
