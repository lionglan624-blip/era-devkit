# Feature 045: Meiling Base Kojo

## Status: [DONE]

## Overview

Create base kojo (KOJO_K1.ERB) for Meiling (K1). Currently Meiling has NTR/WC/vs-You kojo but lacks the standard base dialogue that other characters have.

## Problem

According to kojo-dashboard.html analysis:
- Meiling has **0 base kojo functions** (vs average ~200 for other characters)
- She falls back to Universal kojo for all standard interactions
- This reduces character personality expression during normal gameplay

## Goals

1. Create KOJO_K1.ERB with standard dialogue functions
2. Match structure of existing base kojo files (K2, K4, K8, K9, K10)
3. Express Meiling's character traits (loyal gate guard, martial artist, Chinese heritage)

## Acceptance Criteria

- [x] KOJO_K1.ERB created with core event handlers
- [x] ~~MESSAGE_COM handlers for key commands~~ (deferred - event handlers sufficient for MVP)
- [x] Character-appropriate dialogue reflecting Meiling's personality
- [x] Game runs without errors
- [x] Headless test passes

## Scope

### In Scope

- KOJO_K1.ERB creation with:
  - Event handlers (KOJO_EVENT_K1_0 through _6)
  - MESSAGE_COM handlers for priority commands
  - Basic condition branches (virgin, love, affection)

### Out of Scope

- NTR extension (already exists: NTR口上.ERB)
- WC kojo expansion (already 96 functions)
- vs-You kojo expansion (already 32 functions)

## Technical Considerations

### Reference Files

| File | Functions | Notes |
|------|-----------|-------|
| KOJO_K2.ERB | 284 | Good reference for structure |
| KOJO_K4.ERB | 297 | Most complete character |
| KOJO_K8.ERB | 297 | Similar personality type |

### Target Structure

```erb
@KOJO_K1(ARG)           ; Entry point
@KOJO_EVENT_K1_0        ; Room encounter
@KOJO_EVENT_K1_1        ; Affair discovered
@KOJO_EVENT_K1_2        ; Confrontation
@KOJO_EVENT_K1_3        ; MASTER enters
@KOJO_EVENT_K1_4        ; TARGET enters
@KOJO_EVENT_K1_6        ; Same room at wakeup

@KOJO_MESSAGE_COM_K1_*  ; Command handlers
```

### Character Reference

From [kojo-reference.md](../reference/kojo-reference.md):

**Original Quotes:**
- 「くそ、背水の陣だ！」 - 戦闘前
- 「あら、私について来てもこっちには何もなくてよ？」 - 門番として
- 「えー、普通の人よ。」 - 何者かと聞かれて
- 「済みません、お嬢様〜。」 - 敗北時
- 「たしか…巫女は食べてもいい人類だって言い伝えが…。」

**Personality:**
- のんびり、おおらかな性格
- 門番としての責任感はあるが、どこか抜けている
- 敬語と砕けた口調が混在
- 負けても謝る素直さ

**Speech Pattern:**
- 一人称: 私
- 語尾: 〜わ、〜よ（混在）

## Effort Estimate

- **Size**: Medium-Large (writing dialogue content)
- **Risk**: Low (additive only, no existing code changes)
- **Testability**: Medium (dialogue content is subjective)

## Dependencies

- None (content-only feature)

## Links

- [kojo-reference.md](../reference/kojo-reference.md) - Kojo system overview
- [index-features.md](../index-features.md) - Feature tracking
