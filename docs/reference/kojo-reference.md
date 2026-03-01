# Kojo Reference

## Directory Structure

`ERB/口上/{1-10}_キャラ名/` (K1-K10, 94-620 functions/char) + `ERB/口上/U_汎用/` (KU, 481 functions)

## File Types

| Type | Pattern | Purpose |
|------|---------|---------|
| COM Category | `KOJO_K{N}_{Category}.ERB` | Conversation/Intimacy, Caress, Oral, Daily life |
| NTR | `NTR口上.ERB` (split >5,000 lines) | Scenario-based |
| Event | `KOJO_K{N}_EVENT.ERB` | Admiration/Love/Confession acquisition |
| Special | `WC系口上.ERB`, `SexHara休憩中口上.ERB` | System-specific |

## Function Naming

```erb
@KOJO_MESSAGE_COM_K{CharID}_{CommandID}_{Variant}
@NTR_KOJO_K{N}_{ID}           ; Master not present
@NTR_KOJO_KW{N}_{ID}          ; Master present
@KOJO_MESSAGE_{Event}_K{N}    ; Admiration acquired, Love acquired, Confession success/failure
```

## Trigger System

```erb
TRYCALLLIST
    FUNC NTR_KOJO_MESSAGE_COM_K{N}_{CMD}  ; 1. NTR specific
    FUNC KOJO_MESSAGE_COM_K{N}_{CMD}      ; 2. Character specific
    FUNC KOJO_MESSAGE_COM_KU_{CMD}        ; 3. Universal
    FUNC TRAIN_MESSAGE                     ; 4. Default
ENDFUNC
```

## Branching

### TALENT-based branching (Required)

```erb
IF TALENT:恋人
    ;After becoming lovers
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
| TFLAG:コマンド成功度 | Great success/Success/Failure/Failure NTR | Enthusiastic/Positive/Refusal/Hiding |
| ABL:親密 | 0-2/3-5/6-8/9+ | Reject/Reluctant/Willing/Eager |
| NTR_CHK_FAVORABLY | Cuckolded/Higher than master/Flirty/Obedient/Allows touching/Kissing/ELSE | Choose visitor/Intimate/Friendly/Obedient/Endure/Cautious/Reject |
| IS_NTR_SOFT() | TRUE/FALSE | Softer/Explicit |

## Character Speech Patterns

| Character | 1st Person | Sentence Ending Examples | Differentiation Points |
|--------|--------|--------|----------------|
| 美鈴(K1) | 私 | 〜わ、〜よ | Laid-back, mixed polite/casual ≠ Sakuya (perfect) |
| 小悪魔(K2) | 私/あたし | 〜ですわ | Mischievous polite speech ≠ Patchouli (intellectual) |
| パチュリー(K3) | 私 | 〜わ (flat tone) | Intellectual, obsessed with books ≠ Koakuma (whimsical) |
| 咲夜(K4) | 私 | 〜わ、〜のよ | Perfect polite speech, pride ≠ Meiling (careless) |
| レミリア(K5) | 私 | 〜わ、〜よ | Haughty, charisma + childishness ≠ Flan (innocent) |
| フラン(K6) | 私 | 〜のさ、〜よ | Innocent, madness + purity mix ≠ Remilia (dignity) |
| 妖精メイド(K7) | わたし/あたし | 〜です、〜なの | Poor polite speech, clumsy ≠ Sakuya (perfect) |
| チルノ(K8) | あたい | 〜ね、〜わよ | Bold, unfounded confidence ≠ Daiyousei (modest) |
| 大妖精(K9) | わたし | 〜だよ、〜なの | Modest, protective ≠ Cirno (bold) |
| 魔理沙(K10) | 私 | 〜だぜ、〜ぜ | Casual speech, hard worker ≠ Patchouli (intellectual) |

## Implementation AC

### Scene Description Principles

**Depict the scene, not just list dialogue (actions/psychology/narrative).**

### Required Items

- [ ] TALENT-based branching (Lovers/Love/Admiration/None) implemented
- [ ] Distant reaction in low relationship (ELSE)
- [ ] **4+ lines per branch** (situation description + dialogue + reaction)
- [ ] Differentiated from other characters' speech patterns

### Recommended Items

- [ ] Multiple variations using PRINTDATA/DATALIST
- [ ] Include psychological/action descriptions

### NG Items

| Situation | NG Expression |
|------|--------|
| Low relationship | Friendly expressions, sharing personal topics |
| General | Copying other characters' speech, contradicting source material |

## NTR Implementation AC

### Required Items

- [ ] NTR_CHK_FAVORABLY 6-level branching
- [ ] Both versions: Master absent/Master present
- [ ] IS_NTR_SOFT() support

### NG Items

| Situation | NG Expression |
|------|--------|
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

## Constraints when referencing eraTW

**Prohibited**: `CFLAG:TARGET:添い寝中`, `場所_*` (not implemented)
**Allowed**: `TALENT:思慕/恋慕/恋人/結婚`, `ABL:親密`, `TFLAG:コマンド成功度`
