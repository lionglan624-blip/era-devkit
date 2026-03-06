# Feature 046: Meiling Conversation Kojo

## Status: [DONE]

## Overview

Add conversation command kojo (MESSAGE_COM handlers) for Meiling (K1). Feature 045 established base event handlers; this feature adds dialogue for conversation commands (300-315).

## Problem

- Meiling currently falls back to Universal kojo for all conversation commands
- This reduces character personality expression during conversation interactions
- Other characters (K2, K4, K8, K9, K10) have character-specific conversation kojo

## Goals

1. Add MESSAGE_COM handlers for conversation commands to KOJO_K1.ERB
2. Express Meiling's character traits in dialogue:
   - Laid-back, easy-going personality
   - Gate guard responsibility (but sometimes careless)
   - Mix of polite and casual speech
   - Honest, apologizes when wrong
3. Include condition branches (love level, affection, time of day)

## Acceptance Criteria

### Implementation
- [x] MESSAGE_COM_K1_300 (conversation) implemented
- [x] MESSAGE_COM_K1_301 (serve tea) implemented
- [x] MESSAGE_COM_K1_302 (skinship) implemented
- [x] MESSAGE_COM_K1_310-315 (intimate commands) implemented
- [x] Character-appropriate dialogue reflecting Meiling's personality

### Intimacy Branching (v2 - Added 2025-12)
- [x] CMD 302 (スキンシップ): ABL:親密 branch added
- [x] CMD 311 (抱き付く): Low intimacy shows confusion
- [x] CMD 312 (キス): ABL:親密 branch required
- [x] CMD 313-315 (性的愛撫): ABL:親密 check added
- [x] Dialogue matches system effects (low intimacy = reluctance)

### Testing (Acceptance-based)
- [x] Game runs without errors (Headless起動確認)
- [x] Smoke test passes (debug 893 or direct call)
- [ ] E2E test (恋慕/好感度 branches) - RECOMMENDED (skipped - content is subjective)
- [x] kojo-mapper updated (`tools/kojo-mapper/kojo-map-美鈴.md`)

## Scope

### In Scope

- KOJO_K1.ERB additions:
  - @KOJO_MESSAGE_COM_K1_300 / _300_1 - Basic conversation
  - @KOJO_MESSAGE_COM_K1_301 / _301_1 - Serve tea
  - @KOJO_MESSAGE_COM_K1_302 / _302_1 - Skinship (general)
  - @KOJO_MESSAGE_COM_K1_310 / _310_1 - Pet buttocks
  - @KOJO_MESSAGE_COM_K1_311 / _311_1 - Hug
  - @KOJO_MESSAGE_COM_K1_312 / _312_1 - Kiss
  - @KOJO_MESSAGE_COM_K1_313 / _313_1 - Breast fondling
  - @KOJO_MESSAGE_COM_K1_314 / _314_1 - Anal fondling
  - @KOJO_MESSAGE_COM_K1_315 / _315_1 - Clitoral fondling

### Out of Scope

- NTR extension kojo (already exists)
- vs-You kojo (対あなた口上.ERB - separate feature if needed)
- WC kojo expansion

## Technical Considerations

### Reference Files

| File | Notes |
|------|-------|
| KOJO_K4.ERB | Most complete conversation handlers |
| KOJO_K2.ERB | Good balance of content |
| KOJO_K8.ERB | Similar personality type (laid-back) |

### Command Reference

| CMD | Name | Description |
|-----|------|-------------|
| 300 | 会話 | Basic conversation |
| 301 | お茶を淹れる | Serve tea |
| 302 | スキンシップ | Physical affection (general) |
| 310 | 尻を撫でる | Pet buttocks |
| 311 | 抱き付く | Hug |
| 312 | キスする | Kiss |
| 313 | 胸愛撫 | Breast fondling |
| 314 | アナル愛撫 | Anal fondling |
| 315 | クリ愛撫 | Clitoral fondling |

### Character Reference

From feature-045.md:

**Original Quotes:**
- 「くそ、背水の陣だ！」 - Before battle
- 「あら、私について来てもこっちには何もなくてよ？」 - As gate guard
- 「えー、普通の人よ。」 - When asked who she is
- 「済みません、お嬢様〜。」 - After defeat
- 「たしか…巫女は食べてもいい人類だって言い伝えが…。」

**Personality:**
- Laid-back, easy-going
- Has responsibility as gate guard but sometimes careless
- Mix of polite and casual speech
- Honest, apologizes readily

**Speech Pattern:**
- First person: 私
- Endings: 〜わ、〜よ (mixed)

### Unique Meiling Themes

- Gate guard duties and breaks
- Chinese martial arts references
- Sleeping on the job jokes
- Sakuya's discipline/knives
- Loyalty to Remilia despite being treated poorly

## Effort Estimate

- **Size**: Medium (dialogue writing)
- **Risk**: Low (additive only)
- **Testability**: Medium (dialogue content is subjective)

## Dependencies

- Feature 045: Meiling Base Kojo [DONE] - provides KOJO_K1.ERB base

## Links

- [feature-045.md](feature-045.md) - Meiling Base Kojo (prerequisite)
- [kojo-reference.md](../reference/kojo-reference.md) - Kojo system overview
- [index-features.md](../index-features.md) - Feature tracking

---

## Revision History

| Date | Change |
|------|--------|
| 2025-12-13 | v2: Reopened - Add ABL:親密 branching for intimate commands (302, 311-315). Original implementation lacked intimacy checks, making Meiling appear too permissive. Updated kojo-reference.md with Branching Guidelines. |
| 2025-12-XX | v1: Initial implementation completed |
