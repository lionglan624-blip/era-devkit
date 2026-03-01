---
name: kojo-writing
description: Kojo (口上) dialogue writing reference. Use when creating character dialogue, writing kojo functions, TALENT branching, character speech patterns, NTR implementation.
---

# Kojo Reference

## Execution Model

**Agents reading this SKILL are AUTONOMOUS (self-executing).**

| Item | Description |
|------|-------------|
| Role | **Write** ERB code (not just read) |
| Parent Opus | Delegating side following CLAUDE.md "Opus NEVER writes..." |
| Expected behavior | Read input → implement kojo → output status file |
| STOP applies to | **Specification ambiguity** only. "Should I implement?" is NOT ambiguous (always implement) |

**Important**: "Read .claude/agents/kojo-writer.md" means "read and implement". Do not stop at reading.

## File Operations

**Use Write for appending functions to end** (not Edit)

| Operation | Tool | Reason |
|-----------|------|--------|
| Append new function to end | **Write** | Edit struggles with unique old_string, causes retry loops |
| Modify existing function | Edit | Only when replacement target is clear |

**Write append procedure**:
1. Read entire file
2. Create content with new function appended
3. Write entire file

**Reason**: Edit appending to end cannot guarantee unique old_string anchor, causing multiple retries. Write completes in single operation.

## Directory Structure

`ERB/口上/{1-10}_キャラ名/` (K1-K10, 94-620 functions/char) + `ERB/口上/U_汎用/` (KU, 481 functions)

## File Types

| Type | Pattern | Purpose |
|------|---------|---------|
| COM Category | `KOJO_K{N}_{Category}.ERB` | 会話親密, 愛撫, 口挿入, 日常 |
| NTR | `NTR口上.ERB` (split >5,000 lines) | Scenario-based |
| Event | `KOJO_K{N}_EVENT.ERB` | 思慕/恋慕/告白獲得 |
| Special | `WC系口上.ERB`, `SexHara休憩中口上.ERB` | System-specific |

## COM→File Mapping

**SSOT**: [`src/tools/kojo-mapper/com_file_map.json`](../../../src/tools/kojo-mapper/com_file_map.json)

### JSON Reading Method

1. Get `file` from `ranges` matching COM number
2. If `character_overrides` has K{N} + COM combination, **override**
3. Skip ranges with `implemented: false` (future implementation)

**Example: Determining COM 92 placement**

```
ranges: {"start": 90, "end": 93, "file": "_口挿入.ERB", ...}
character_overrides: {"K10": {"92": "_愛撫.ERB"}}
```

| Character | Result | Reason |
|-----------|--------|--------|
| K1-K9 | `_口挿入.ERB` | ranges default |
| K10 | `_愛撫.ERB` | character_overrides override |

**Do not hardcode individual definitions in this file. Always reference JSON.**

## Function Naming

```erb
@KOJO_MESSAGE_COM_K{CharID}_{CommandID}_{Variant}
@NTR_KOJO_K{N}_{ID}           ; Master not present
@NTR_KOJO_KW{N}_{ID}          ; Master present
@KOJO_MESSAGE_{Event}_K{N}    ; 思慕獲得, 恋慕獲得, 告白成功/失敗
```

### Special Function Suffixes

| Suffix | Purpose | Example |
|--------|---------|---------|
| `_{N}` | **Main kojo for COM N** | `@KOJO_MESSAGE_COM_K4_0` = COM 0 |
| `_00` | **Common additional processing for all COMs** (universal fallback) | `@KOJO_MESSAGE_COM_K4_00` |

**Note**: `_0` and `_00` serve **different purposes**. `_00` is a universal handler called after any COM success. Empty stub is valid. (Discovered in F261)

## Trigger System

```erb
TRYCALLLIST
    FUNC NTR_KOJO_MESSAGE_COM_K{N}_{CMD}  ; 1. NTR specific
    FUNC KOJO_MESSAGE_COM_K{N}_{CMD}      ; 2. Character specific
    FUNC KOJO_MESSAGE_COM_KU_{CMD}        ; 3. Universal
    FUNC TRAIN_MESSAGE                     ; 4. Default
ENDFUNC
```

## Phase Definitions

Kojo dialogue is categorized into implementation phases based on branching complexity, line count, and target COMs.

### Phase 8d: 基本口上 (Basic Dialogue)

**Purpose**: Standard COM dialogue with modular structure and TALENT-based branching.

**Branching**: Nested (TALENT 4分岐)
- 恋人 / 恋慕 / 思慕 / なし

**Pattern**:
```erb
@KOJO_MESSAGE_COM_K{N}_{X}
CALL TRAIN_MESSAGE
CALLF KOJO_MODIFIER_PRE_COMMON
CALL KOJO_MESSAGE_COM_K{N}_{X}_1
CALLF KOJO_MODIFIER_POST_COMMON
RETURN RESULT
```

**Line Count**: 4-8 lines per branch

**Patterns**: 4+ variations (DATALIST) per branch

**Target COMs**: All COMs

**Quality Standards**:
- TALENT 4-way branching required
- 4+ lines per branch (situation + dialogue + reaction)
- 4+ DATALIST variations per branch

### Phase 8j: 射精状態 (Ejaculation-state Dialogue)

**Purpose**: Dialogue specialized for ejaculation states (during/immediately after).

**Branching**: Complete branching (immediate RETURN)
- NOWEX:MASTER:11 (射精中)
- TALENT 4分岐 (恋人/恋慕/思慕/なし)
- TCVAR (partner situation variables)

**Pattern**:
```erb
;=== Phase 8j: 射精状態 ===
IF NOWEX:MASTER:11  ; 射精中
    IF TALENT:恋人
        PRINTFORMW ...（専用口上）
        RETURN 1
    ELSEIF TALENT:恋慕
        PRINTFORMW ...
        RETURN 1
    ELSEIF TALENT:思慕
        PRINTFORMW ...
        RETURN 1
    ELSE
        PRINTFORMW ...
        RETURN 1
    ENDIF
ENDIF

;=== Phase 8d: 基本口上 ===
; (fallback)
```

**Line Count**: 4-8+ lines per branch

**Patterns**: 4+ variations per branch

**Target COMs**: COM_80-85 (口挿入/射精系)

**Quality Standards**:
- Complete branching (RETURN 1 in each branch)
- 4-8行以上 per branch
- 4+ DATALIST variations per branch
- Must check NOWEX:MASTER:11 first

### Phase 8l: 初回実行 (First Execution Guard)

**Purpose**: Specialized dialogue for first-time execution of any COM (including daily COMs).

**Branching**: Complete branching (immediate RETURN)
- FIRSTTIME(SELECTCOM) only (no TALENT branching)

**Pattern**:
```erb
;=== Phase 8l: FIRSTTIME（最優先判定） ===
IF FIRSTTIME(SELECTCOM)
    PRINTFORMW 「セリフ」
    PRINTFORMW 地の文（8-15行以上）
    PRINTFORMW 地の文
    ...
    RETURN 1
ENDIF

;=== Phase 8d: 基本口上 ===
; (fallback)
```

**Line Count**: 8-15+ lines (up to 25 lines acceptable)

**Patterns**: 1 pattern per COM (no TALENT branching needed)

**Target COMs**: **ALL 88 COMs** including:
- Daily COMs (300 series: conversation/tea/walk, 400 series: meal/rest)
- Training COMs (0-600 series all)

**Quality Standards**:
- Complete branching (RETURN 1 after FIRSTTIME block)
- 8-15+ lines (up to 25 lines acceptable)
- Focus on "first time doing this command" moment
- NO TALENT branching (situation itself is the focus)

**Difference from Phase 8f** (First Experience):
- Phase 8f = Once-in-lifetime major events (virginity loss, 15-30 lines)
- Phase 8l = First execution of each command (natural relationship building, 8-15 lines)

### Phase Priority Order

When multiple phases apply to the same COM, implement in this order:

```erb
;=== Phase 8l: FIRSTTIME (highest priority check) ===
IF FIRSTTIME(SELECTCOM)
    PRINTFORMW ...
    RETURN 1
ENDIF

;=== Phase 8j: Ejaculation state ===
IF NOWEX:MASTER:11
    ; TALENT branching with RETURN 1
ENDIF

;=== Phase 8d: Basic kojo (modular) ===
CALL TRAIN_MESSAGE
CALLF KOJO_MODIFIER_PRE_COMMON
CALL KOJO_MESSAGE_COM_K{N}_{X}_1
CALLF KOJO_MODIFIER_POST_COMMON
RETURN RESULT
```

## Branching

### TALENT Method (Required)

```erb
IF TALENT:恋人
    ;After lover established
ELSEIF TALENT:恋慕
    ;Has romantic feelings
ELSEIF TALENT:思慕
    ;Has affection/admiration
ELSE
    ;Distant/cautious
ENDIF
```

### Other Branches

| Branch | Values | Tone |
|--------|--------|------|
| TFLAG:Command success level | Great success/Success/Failure/NTR failure | Enthusiastic/Positive/Refusal/Hiding |
| ABL:Intimacy | 0-2/3-5/6-8/9+ | Reject/Reluctant/Willing/Eager |
| NTR_CHK_FAVORABLY | Cuckolded/Higher than master/Giggly/Service/Allow touching/Kiss/ELSE | Choose visitor/Intimate/Friendly/Obedient/Endure/Cautious/Reject |
| IS_NTR_SOFT() | TRUE/FALSE | Softer/Explicit |

## Character Speech Patterns

| Char | Pronoun | Suffix | Differentiation |
|------|---------|--------|-----------------|
| Meiling(K1) | 私 | 〜わ、〜よ | Relaxed, mixed politeness ≠Sakuya(perfect) |
| Koakuma(K2) | 私/あたし | 〜ですわ | Playful polite ≠Patchouli(intellectual) |
| Patchouli(K3) | 私 | 〜わ(flat) | Intellectual, book-obsessed ≠Koakuma(whimsical) |
| Sakuya(K4) | 私 | 〜わ、〜のよ | Perfect politeness, pride ≠Meiling(careless) |
| Remilia(K5) | 私 | 〜わ、〜よ | Haughty, charisma+childlike ≠Flan(innocent) |
| Flandre(K6) | 私 | 〜のさ、〜よ | Innocent, madness+purity mix ≠Remilia(dignity) |
| Fairy Maid(K7) | わたし/あたし | 〜です、〜なの | Clumsy politeness ≠Sakuya(perfect) |
| Cirno(K8) | あたい | 〜ね、〜わよ | Boastful, baseless confidence ≠Daiyousei(modest) |
| Daiyousei(K9) | わたし | 〜だよ、〜なの | Modest, protective ≠Cirno(boastful) |
| Marisa(K10) | 私 | 〜だぜ、〜ぜ | Casual speech, hardworking ≠Patchouli(intellectual) |

## COMF Verification (before writing)

Before writing dialogue for COM N, read `Game/ERB/COMF{N}.ERB`:

| Item | Purpose | Consequence if ignored |
|------|---------|------------------------|
| **PLAYER/TARGET** | Identify actor/recipient | Actor/recipient reversal error |
| **Conditions** | Activation conditions (TALENT:futanari etc.) | Context inconsistency |
| SOURCE | Pleasure source (reference) | - |
| COM name | Action content (reference) | - |

**PLAYER/TARGET and Conditions are mandatory checks. SOURCE/COM name are reference info.** (Importance confirmed in F261/F269)

## TCVAR:116 Verification (CRITICAL)

**TCVAR:116 determines who is the ACTOR**:

| TCVAR:116 | Meaning | Expression in kojo |
|-----------|---------|-------------------|
| PLAYER | MASTER/PLAYER is actor | "Do X to you", "I do X" |
| TARGET | TARGET/Character is actor | "Be done X", "You do X to me" |

**Warning about "させる" (make do) commands** (Discovered in F336):
- Command name "Xさせる" is MASTER perspective naming
- Actual actor determined by COMF's TCVAR:116
- Example: "正常位させる" (make do missionary) → TCVAR:116=TARGET → TARGET penetrates MASTER
- When eraTW absent, COMF is SSOT

**Beware COMF comment/code divergence**:
- Comments (`;奴隷のＰ⇔調教者のＡ` etc.) may be incorrect
- **Prioritize code (STAIN:X, EXP:X)**
- F336: Fixed COMF97/98 comment "Ａ"→actual "Ｖ" inconsistency

## Existing Pattern Reference

**Purpose**: Ensure consistency with existing codebase by referencing actual implementations before writing new kojo.

**Priority Order** (highest to lowest):

1. **同一ファイル内の他 COM 実装** - Same character's other COM implementations in the same file
2. **他キャラの同 COM 実装** - Other characters' implementations of the same COM
3. **SKILL テンプレート** - This SKILL.md template

**Reference Method**: Use Grep to extract existing patterns from codebase.

### Pattern Extraction Procedure

Before writing new kojo, search for existing patterns:

```bash
# 1. Same file, other COMs (highest priority)
# Example: Writing COM_85 for K4 → check K4's COM_65, COM_80, etc.
Grep "KOJO_MESSAGE_COM_K4_" Game/ERB/口上/4_十六夜咲夜/KOJO_K4_口挿入.ERB -output_mode content -A 10

# 2. Same COM, other characters (second priority)
# Example: Writing COM_85 for K4 → check K1-K3's COM_85
Grep "KOJO_MESSAGE_COM_K[0-9]_85" Game/ERB/口上/ -output_mode content -A 10

# 3. SKILL template (fallback)
# If no existing patterns found, follow ERB Template section
```

### Pattern Types to Reference

| Pattern Type | Purpose | Example |
|--------------|---------|---------|
| **KOJO_MODIFIER** | Pre/post dialogue modifiers | `CALLF KOJO_MODIFIER_PRE_COMMON` |
| **DATALIST** | Variation count and structure | 4+ DATALIST blocks per branch |
| **Branching** | TALENT/TCVAR/NOWEX logic | Phase 8j ejaculation branching |
| **Line Count** | Length per branch | 4-8 lines per TALENT branch |
| **Phase Structure** | FIRSTTIME/射精/基本 order | Phase Priority Order |

**Critical**: If existing patterns use `KOJO_MODIFIER_PRE_COMMON`/`POST_COMMON`, **always include them** in new implementations. F291 K4 failure caused by omitting these calls despite same-file precedent (COM_65).

### Conflict Resolution

If SKILL template contradicts existing patterns:

1. **STOP** and report to user
2. Include both patterns in report
3. Wait for user decision

**Example**:
```
BLOCKED:DOC_MISMATCH
SKILL template: 4 DATALIST per branch
Existing K4 COMs: 6 DATALIST per branch
```

## Procedure

1. Read `pm/features/feature-{ID}.md` for COM number and character (K{N})
2. Read `pm/cache/eratw-COM_{X}.txt` for eraTW patterns
3. **Read `Game/ERB/COMF{N}.ERB` for PLAYER/TARGET and Conditions**
4. **Reference existing patterns using Grep** (see Existing Pattern Reference section)
5. **Determine target file using JSON**:
   - Read `src/tools/kojo-mapper/com_file_map.json`
   - Find COM in `ranges` → get default `file`
   - Check `character_overrides` for K{N} + COM → override if exists
6. Check for existing stubs using Grep
7. Write dialogue following existing patterns + ERB Template structure
8. Validate output against Quality Standards

## /run Phase 4 Procedure (Orchestrator Reference)

**Note**: This section for Opus orchestrator during `/run` execution. kojo-writer agents read Procedure section above.

### 4.0 Pre-Implementation Check

**Stub Check**: For each K{N}:
```bash
python src/tools/python/erb-duplicate-check.py --check-stub --function "KOJO_MESSAGE_COM_K{N}_{COM}" --path "Game/ERB/口上"
```

| Result | Action |
|--------|--------|
| NOT_FOUND | New implementation |
| STUB | Remove stub first |
| IMPLEMENTED | Quality check → skip if GOOD |
| DUPLICATE | **STOP** |

**eraTW Cache**: Dispatch eratw-reader → pass cache path to kojo-writer.

### 4.1 Batch Dispatch

Dispatch all K1-K10 with `run_in_background: true`:
```
prompt: "{ID} K{N}"   // Example: "289 K1"
```

**Polling**: Glob("pm/status/{ID}_K*.txt") → complete when count == 10

| Param | Value |
|-------|-------|
| Initial delay | 360s |
| Poll interval | 60s |
| Timeout | 1h |

### 4.2 Post-Dispatch Checks

After all 10 complete:

1. **Function existence**: Grep `@KOJO_MESSAGE_COM_K{N}_{COM}` (parallel 10x)
2. **MODIFIER calls**: Grep `KOJO_MODIFIER_PRE_COMMON` and `POST_COMMON`
3. **RETURN statement**: Grep `RETURN 0`

If any missing → **STOP** or re-dispatch affected K{N}.

### 4.3 Failure Recovery

1. Resume failed agent: "Explain why you did not complete"
2. Root cause analysis → report to user
3. Apply fix → re-dispatch

---

## ERB Template

```erb
@KOJO_MESSAGE_COM_K{N}_{X}
CALL TRAIN_MESSAGE
CALLF KOJO_MODIFIER_PRE_COMMON
CALL KOJO_MESSAGE_COM_K{N}_{X}_1
CALLF KOJO_MODIFIER_POST_COMMON
RETURN RESULT

@KOJO_MESSAGE_COM_K{N}_{X}_1
IF TALENT:Lover
    PRINTDATA
        DATALIST
        	DATAFORM
        	DATAFORM "Dialogue"
        	DATAFORM Narration
        ENDLIST
        DATALIST
        ...pattern2...
        ENDLIST
        DATALIST
        ...pattern3...
        ENDLIST
        DATALIST
        ...pattern4...
        ENDLIST
    ENDDATA
    PRINTFORMW
ELSEIF TALENT:Affection
    PRINTDATA
        DATALIST...ENDLIST (×4)
    ENDDATA
    PRINTFORMW
ELSEIF TALENT:Fondness
    PRINTDATA
        DATALIST...ENDLIST (×4)
    ENDDATA
    PRINTFORMW
ELSE
    PRINTDATA
        DATALIST...ENDLIST (×4)
    ENDDATA
    PRINTFORMW
ENDIF
RETURN 1
```

## Quality

### Scene Description Principle

**Depict scenes, not just dialogue lists. Include actions, psychology, narration.**

### Required Items

- [ ] TALENT method (lover/affection/fondness/none) implemented
- [ ] Distant response in low relationship (ELSE)
- [ ] **4+ lines per branch** (situation+dialogue+reaction)
- [ ] Speech differentiated from other characters

### Recommended Items

- [ ] Multiple variations via PRINTDATA/DATALIST
- [ ] Psychology/action descriptions included

### NG Items

| Situation | NG Expression |
|-----------|---------------|
| Low relationship | Friendly expressions, sharing personal topics |
| General | Copying other character's speech, contradicting canon |

## Constraints

- UTF-8 with BOM required (hook auto-processes)
- eraTW forbidden: `CFLAG:Sleeping together`, `Place_*` (not implemented)
- eraTW allowed: TALENT, ABL, TFLAG

## NTR Implementation AC

### Required Items

- [ ] NTR_CHK_FAVORABLY 6-level branching
- [ ] Both master absent/present versions
- [ ] IS_NTR_SOFT() support

### NG Items

| Situation | NG Expression |
|-----------|---------------|
| Low FAV | Active affection toward visitor |
| High FAV | Devoted attitude toward master |

## File Encoding

**ALL ERB files MUST use UTF-8 with BOM.** Files without BOM cause mojibake.

### Verification & Fix

```powershell
$bytes = [System.IO.File]::ReadAllBytes("file.ERB")
$hasBOM = ($bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
if (-not $hasBOM) {
    $content = Get-Content "file.ERB" -Raw
    [System.IO.File]::WriteAllText("file.ERB", $content, [System.Text.UTF8Encoding]::new($true))
}
```

## eraTW Reference Constraints

**Forbidden**: `CFLAG:TARGET:Sleeping together`, `Place_*` (not implemented)
**Allowed**: `TALENT:Fondness/Affection/Lover/Marriage`, `ABL:Intimacy`, `TFLAG:Command success level`
