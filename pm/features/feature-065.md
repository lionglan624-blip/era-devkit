# Feature 065: Other Character COM Integration

## Status: [DONE]

## Background

<!-- Session handoff: Record ALL discussion details here -->
- **Original problem**: Feature 057 established COM category-based file organization for K4 (Sakuya). Other characters still have fragmented file structures.
- **Considered alternatives**:
  - Individual Features per character (065-072) - Too granular for mechanical work
  - **Single Feature for all characters** - Preferred, with subagent automation for 066+
- **Key decisions**:
  - Feature 065: Create tooling + K1 (Meiling) as pilot
  - Subagent `.claude/agents/kojo-refactor.md` for automation
  - 066+ executed via subagent (Sonnet/Haiku)
- **Prerequisites**: Feature 057 complete (COM category pattern established)

## Overview

Extend COM category-based file organization from K4 to all other characters. Create generalized tooling and subagent for automated execution.

## Problem

- Other characters still have fragmented kojo files (KOJO_K{N}.ERB, NTR拡張, 対あなぁE
- No generalized script for COM category reorganization
- Manual process for each character is inefficient

## Goals

1. Generalize `tools/reorganize_k4.py` to support all characters
2. Reorganize K1 (Meiling) as pilot case
3. Create subagent for automated execution of remaining characters
4. Validate with Headless kojo tests

## Target Characters

| Char | Files to Reorganize | Complexity |
|------|---------------------|------------|
| K1 美鈴 | KOJO_K1.ERB (710), 対あなぁE(964) | Low |
| K2 小悪魁E| KOJO_K2.ERB, NTR拡張, 対あなぁE| Medium |
| K3 パチュリー | NTR拡張, 対あなぁE| Low |
| K5 レミリア | NTR拡張, 対あなぁE| Low |
| K6 フラン | NTR拡張, 対あなぁE| Low |
| K7 子悪魁E| 対あなぁEonly | Minimal |
| K8 チルチE| KOJO_K8.ERB, NTR拡張, 対あなぁE| Medium |
| K9 大妖精 | KOJO_K9.ERB, 対あなぁE| Low |
| K10 魔理沁E| KOJO_K10.ERB, NTR拡張 | Medium |
| KU 汎用 | NTR拡張, 対あなぁE| Low |

## Feature 065 Scope (Opus)

### In Scope
- Generalize `tools/reorganize_k4.py` ↁE`tools/reorganize_kojo.py`
- Execute K1 (Meiling) COM integration
- Create `.claude/agents/kojo-refactor.md` subagent
- Document workflow for subagent usage

### Out of Scope (066+, via Subagent)
- K2-K10, KU reorganization (separate execution via subagent)

## Acceptance Criteria

- [x] `tools/reorganize_kojo.py --char K{N}` works for any character
- [x] K1 美鈴 reorganized to COM category files
- [x] Build succeeds (0 errors)
- [x] ErbLinter passes for K1 files
- [x] kojo-mapper function count maintained for K1 (289 functions)
- [x] Headless kojo test validates K1 functions (8 functions tested)
- [x] `.claude/agents/kojo-refactor.md` updated with current workflow (Phase 1 Opus / Phase 2 Subagent)

## Technical Design

### Generalized Script

```python
# tools/reorganize_kojo.py
# Usage: python reorganize_kojo.py --char K1

def main():
    parser.add_argument('--char', required=True, help='K1, K2, ..., K10, KU')
    parser.add_argument('--dry-run', action='store_true')

    # Auto-detect source files per character
    # Apply same COM categorization logic as K4
    # Generate category files
```

### File Organization Pattern (from 057)

```
{N}_{name}/
├── KOJO_K{N}_会話親寁EERB   # COM300-315, 350-352
├── KOJO_K{N}_愛撫.ERB       # COM0-9, 20-21, 40-48
├── KOJO_K{N}_挿入.ERB       # COM60-72
├── KOJO_K{N}_口挿入.ERB     # COM80-148, 180-203
├── KOJO_K{N}_日常.ERB       # COM410-415, 463
├── KOJO_K{N}_EVENT.ERB      # EVENT, COUNTER, etc.
├── NTR口上_*.ERB            # (unchanged)
├── WC系口丁EERB             # (unchanged)
└── SexHara休�E中口丁EERB    # (unchanged)
```

### Verification Steps

1. **Build**: `dotnet build uEmuera/uEmuera.Headless.csproj`
2. **Lint**: `dotnet run --project tools/ErbLinter -- Game/ERB/口丁E{N}_{name}/*.ERB`
3. **Mapper**: `node src/tools/kojo-mapper/src/index.js --char K{N}`
4. **Headless**: `dotnet run --project uEmuera/uEmuera.Headless.csproj -- Game --unit "KOJO_MESSAGE_COM_K{N}_300" --char "{N}"`

## Subagent Workflow (for 066+)

After 065 completion, remaining characters processed via:

```bash
# User or Opus triggers subagent
> Use the kojo-refactor subagent to process K2
```

Subagent executes:
1. Run `reorganize_kojo.py --char K{N}`
2. Verify with ErbLinter
3. Verify with kojo-mapper
4. Run Headless kojo test
5. Report results

## Effort Estimate

- **Size**: Medium (script generalization + K1 pilot)
- **Risk**: Low (pattern proven in 057)
- **Testability**: High (automated verification)

## IMPORTANT: COM File Placement Rules

Feature 190 で以下�E配置ルールが確宁E

| COM Range | Category | File |
|-----------|----------|------|
| 60-72 | 挿入系 (膣/アナル) | `_挿入.ERB` |
| 80-85 | 手技系 (フェラ/パイズリ) | `_口挿入.ERB` |

**SSOT**: `.claude/skills/kojo-writing/SKILL.md`

詳細は Feature 190, 221 を参照、E

## Links

- [feature-057.md](feature-057.md) - K4 COM Integration (reference)
- [reference/testing-reference.md](../reference/testing-reference.md) - Test documentation (kojo-test)
- [index-features.md](../index-features.md) - Feature tracking
