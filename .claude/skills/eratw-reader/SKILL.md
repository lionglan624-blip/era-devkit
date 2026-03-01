---
name: eratw-reader
description: eraTW reference extractor. Extracts COM section and caches for kojo-writers.
context: fork
agent: general-purpose
allowed-tools: Read, Write, Grep, Bash
---

# eraTW Reader Skill

Extracts COM sections from eraTW Reimu and saves to cache.

## Input

COM number from Opus

## Output

| Status | Format |
|--------|--------|
| Success | `OK:cached` |
| Error | `ERR:{reason}` |

## eraTW Path

**CRITICAL**: Use configurable path, NOT hardcoded.

**Priority order**:
1. Environment variable: `ERATW_PATH`
2. CLAUDE.md: External Dependencies > eraTW Reference Repository
3. Fallback (hardcoded): `C:\Users\siihe\OneDrive\同人ゲーム\eraTW4.920`

**File path within eraTW**:
```
{ERATW_PATH}\ERB\口上・メッセージ関連\個人口上\001 Reimu [霊夢]\霊夢\M_KOJO_K1_コマンド.ERB
```

**Verification**: Before reading, verify path exists. If not found, STOP and report.

## Process

0. **Bash** to get environment variable `ERATW_PATH`
   ```bash
   echo $ERATW_PATH
   ```
   - If set → Use that path
   - If not set → Use CLAUDE.md fallback

1. **Read** eraTW file first (read 1 line for verification) → If failure, return `ERR:file_not_found`
2. **Grep** for `@M_KOJO_MESSAGE_COM_K1_{NUM}` (note: eraTW uses `M_` prefix) → If no result, return `ERR:section_not_found`
3. **Read** section (~200 lines from grep result line number)
4. **Write** to `pm/cache/eratw-COM_{NUM}.txt`

## Cache Format

```erb
; eraTW霊夢 COM_{NUM} reference
; Extracted: {date}
{extracted ERB code}
```

## Phase-specific Extraction

**Input parameter extension**: `{COM: 80, Phase: "8l"}`

### Output File Naming

| Phase | Extraction Target | Output |
|:-----:|-------------------|--------|
| 8d | TALENT branch patterns | pm/cache/eratw-COM_{X}.txt |
| 8j | NOWEX/TCVAR branches | pm/cache/eratw-COM_{X}-8j.txt |
| 8l | FIRSTTIME blocks | pm/cache/eratw-COM_{X}-8l.txt |

**Default**: When Phase is not specified, assume Phase 8d (existing behavior).

### Phase 8l: FIRSTTIME Block Extraction

**Purpose**: Extract FIRSTTIME(SELECTCOM) block for initial execution context.

**Extraction Logic**:
1. **When eraTW has matching COM**: Extract FIRSTTIME block
   - Grep pattern: `IF FIRSTTIME\(SELECTCOM\)`
   - Block range: From `IF FIRSTTIME` to corresponding `ENDIF`
   - Output: pm/cache/eratw-COM_{X}-8l.txt
2. **When no match**: Return `ERR:section_not_found`
   - kojo-writer will use kojo-phases.md as reference instead

**FIRSTTIME Block Identification**:
- Grep within the COM section (~200 lines)
- Look for `IF FIRSTTIME(SELECTCOM)` or `IF FIRSTTIME(SELECTCOM:対象番号)`
- Extract from `IF FIRSTTIME` to matching `ENDIF` (respect nesting)

### Phase 8j: NOWEX/TCVAR Branch Extraction

**Purpose**: Extract NOWEX/TCVAR conditional branches for variation patterns.

**Extraction Logic**:
1. Grep within COM section for NOWEX/TCVAR patterns
2. Extract relevant branches
3. Output: pm/cache/eratw-COM_{X}-8j.txt

**Note**: Phase 8j follows the same extraction logic as 8l but targets NOWEX/TCVAR conditional blocks instead of FIRSTTIME blocks.

## Error Handling

| Situation | Action |
|-----------|--------|
| eraTW file inaccessible | Return `ERR:file_not_found` |
| Grep returns no results | Return `ERR:section_not_found` |

**CRITICAL**: Do not perform unspecified actions. No inference or alternative processing. Return ERR and terminate.

## Caller Fallback Procedure (for /run workflow)

When eratw-reader returns an error, the caller (Opus in /run workflow) must follow this fallback procedure:

### ERR:section_not_found

**Meaning**: The specified COM reference does not exist in eraTW Reimu.

**Caller Action**:
1. **Record DEVIATION** in feature-{ID}.md Execution Log:
   ```
   | {timestamp} | DEVIATION | eratw-reader | COM_{X} cache | ERR:section_not_found |
   ```
2. **Proceed with original content creation**: kojo-writer creates original content
3. **No cache file**: kojo-writer operates without cache (following SKILL quality standards)

**Rationale**: eraTW is a reference, not mandatory. Phase 8d quality can be achieved without reference.

### ERR:file_not_found

**Meaning**: eraTW repository path is invalid, or file does not exist.

**Caller Action**:
1. **STOP** → Report to user: "eraTW file not found. Please verify ERATW_PATH."
2. **Do NOT proceed**: This is an environment configuration issue requiring user intervention

## Decision Criteria

- NEVER use Bash for file read/write (use Read/Write tools instead)
- Output 1行のみ
- Overwrite existing cache
