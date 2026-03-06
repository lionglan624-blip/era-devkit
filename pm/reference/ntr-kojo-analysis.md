# NTR Kojo Analysis

Cross-character NTR kojo branch statistics for K1-K6, K8-K10, and U_汎用. K7 (子悪魔) is excluded — zero NTR kojo files confirmed. K4 (咲夜) data is referenced from the architecture doc pre-analysis; not re-derived.

---

## Universal NTR Infrastructure (NTR_UTIL.ERB)

The function `@NTR_CHK_FAVORABLY(奴隷, 好感度LV)` in `NTR_UTIL.ERB:1040-1099` implements the universal 11-level FAV system. All per-character kojo files call NTR_CHK_FAVORABLY — they share this infrastructure. The 11 levels are ordered from highest to lowest commitment:

| Level | Constant | Condition Summary |
|:-----:|----------|-------------------|
| 1 | FAV_寝取り返し寸前 | NTR talent + 好感度 > 900 |
| 2 | FAV_寝取り返し中 | NTR talent + 好感度 > 500 |
| 3 | FAV_寝取られ | NTR talent flag only |
| 4 | FAV_寝取られ寸前 | 屈服度 vs 好感度 threshold with 浮気公認/浮気癖 modifiers |
| 5 | FAV_寝取られそう | Softer 屈服度 threshold, same modifiers |
| 6 | FAV_主人より高い | 屈服度 > 好感度 and 屈服度 > 1000 |
| 7 | FAV_うふふする程度2 | 屈服度 > 好感度*5/6 and 屈服度 > 700 |
| 8 | FAV_うふふする程度 | 屈服度 > 好感度*3/4 and 屈服度 > 500 |
| 9 | FAV_奉仕する程度 | 屈服度 > 好感度/2 and 屈服度 > 400 |
| 10 | FAV_体を触らせる程度 | 屈服度 > 好感度/3 and 屈服度 > 150 |
| 11 | FAV_キスする程度 | 屈服度 > 好感度/4 and 屈服度 > 100 |

This 11-level framework is universal — defined once in NTR_UTIL.ERB and referenced universally by all character kojo files. Individual characters implement varying subsets of these levels in their kojo branches. The infrastructure also defines TALENT:奴隷: as the universal slave-talent condition framework.

---

## Per-Character Statistics

### K1 (美鈴)

| Metric | Count |
|--------|------:|
| FAV_ occurrences | 223 |
| TALENT:奴隷: occurrences | 232 |
| NTR_CHK calls | 241 |
| CHK_NTR_SATISFACTORY calls | 3 |
| NTR lines (total) | 3,055 |

**Files**: NTR口上.ERB, NTR口上_お持ち帰り.ERB (2-file standard pattern)

**Notes**: K1 follows the standard 2-file pattern. No character-specific helper abstractions identified. FAV count (223) is among the lower end of the character range, reflecting relatively moderate NTR kojo scope.

---

### K2 (小悪魔)

| Metric | Count |
|--------|------:|
| FAV_ occurrences | 337 |
| TALENT:奴隷: occurrences | 462 |
| NTR_CHK calls | 359 |
| CHK_NTR_SATISFACTORY calls | 3 |
| NTR lines (total) | 5,417 |

**Files**: NTR口上.ERB, NTR口上_お持ち帰り.ERB (2-file standard pattern)

**Notes**: K2 has a higher TALENT:奴隷: density relative to FAV_ count compared to K1, indicating heavier use of TALENT branching conditions alongside FAV-level branching.

---

### K3 (パチュリー)

| Metric | Count |
|--------|------:|
| FAV_ occurrences | 350 |
| TALENT:奴隷: occurrences | 532 |
| NTR_CHK calls | 363 |
| CHK_NTR_SATISFACTORY calls | 4 |
| NTR lines (total) | 6,282 |

**Files**: NTR口上.ERB, NTR口上_お持ち帰り.ERB (2-file standard pattern)

**Notes**: K3 has a high TALENT:奴隷: count relative to FAV_, similar to K2. No character-specific helper abstractions identified. CHK_NTR_SATISFACTORY count of 4 is slightly above average for non-unique characters.

---

### K4 (咲夜) — Reference

K4 pre-analysis data is referenced from `docs/architecture/migration/phase-20-27-game-systems.md:395-541` (not re-derived per F827 Key Decision). K4 is the statistical outlier: 662 FAV occurrences, 16,146 NTR lines across 6 scenario-split files — the only character with this file decomposition pattern.

K4 serves as the baseline calibration reference for the 8h/8m/8n gap analysis (8h=80%, 8m=10%, 8n=0%).

---

### K5 (レミリア)

| Metric | Count |
|--------|------:|
| FAV_ occurrences | 433 |
| TALENT:奴隷: occurrences | 729 |
| NTR_CHK calls | 477 |
| CHK_NTR_SATISFACTORY calls | 13 |
| NTR lines (total) | 9,404 |

**Files**: NTR口上.ERB, NTR口上_お持ち帰り.ERB (2-file standard pattern)

**Notes**: K5 has the highest CHK_NTR_SATISFACTORY count (13) among the 2-file characters, indicating elaborate satisfaction-state branching. **UNIQUE**: NTR性交拒否 CFLAG state machine (13 occurrences) — a sex refusal subsystem that abstracts FAV branching. This per-character subsystem is not present in any other character and means K5's raw FAV_ count (433) may undercount effective branching: the NTR性交拒否 state machine introduces additional branch points not visible in FAV_ grep counts.

---

### K6 (フラン)

| Metric | Count |
|--------|------:|
| FAV_ occurrences | 360 |
| TALENT:奴隷: occurrences | 679 |
| NTR_CHK calls | 374 |
| CHK_NTR_SATISFACTORY calls | 6 |
| NTR lines (total) | 9,255 |

**Files**: NTR口上.ERB, NTR口上_お持ち帰り.ERB (2-file standard pattern)

**Notes**: K6 has a high TALENT:奴隷: count (679) relative to FAV_ (360), suggesting dense TALENT-gated branching. No character-specific helper abstractions identified.

---

### K7 (子悪魔) — Excluded (zero NTR kojo files)

K7 is excluded from NTR kojo analysis. Confirmation: zero NTR kojo files exist for K7. The only NTR-string matches found in K7's directory are in WC系口上.ERB (32 occurrences), which consist of commented-out dead code and NTR revelation witness dialogue — not active NTR kojo. K7 zero NTR kojo files means no branching statistics are derivable. This exclusion is consistent across all three investigation methods used during Task 1.

---

### K8 (チルノ)

| Metric | Count |
|--------|------:|
| FAV_ occurrences | 296 |
| TALENT:奴隷: occurrences | 423 |
| NTR_CHK calls | 309 |
| CHK_NTR_SATISFACTORY calls | 3 |
| NTR lines (total) | 5,494 |

**Files**: NTR口上.ERB, NTR口上_お持ち帰り.ERB (2-file standard pattern)

**Notes**: K8 contains dead code at lines 18/25 of NTR口上.ERB. The dead code does not affect active branching counts but signals incomplete implementation or authoring in progress at those lines. No character-specific helper abstractions identified.

---

### K9 (大妖精)

| Metric | Count |
|--------|------:|
| FAV_ occurrences | 221 |
| TALENT:奴隷: occurrences | 230 |
| NTR_CHK calls | 216 |
| CHK_NTR_SATISFACTORY calls | 4 |
| NTR lines (total) | 2,985 |

**Files**: NTR口上.ERB, NTR口上_お持ち帰り.ERB (2-file standard pattern)

**Notes**: K9 has the lowest FAV_ count (221) and NTR line count (2,985) among all analyzed characters. TALENT:奴隷: count (230) closely tracks FAV_ count, suggesting a relatively 1:1 relationship without additional helper abstractions. K9 represents the lower bound of NTR kojo complexity.

---

### K10 (魔理沙)

| Metric | Count |
|--------|------:|
| FAV_ occurrences | 393 |
| TALENT:奴隷: occurrences | 731 |
| NTR_CHK calls | 445 |
| CHK_NTR_SATISFACTORY calls | 38 |
| NTR lines (total) | 11,387 |

**Files**: NTR口上.ERB, NTR口上_お持ち帰り.ERB (2-file standard pattern)

**Notes**: K10 has the highest CHK_NTR_SATISFACTORY count (38) by a large margin — over 3x K5's count (13). **UNIQUE**: MSG_NTR_SEX_10 helper function with ~50 call sites and approximately 5 FAV branches in the helper body. This per-character helper abstracts branching that is not visible in the raw FAV_ grep count (393). The effective branching point count is approximately 393 + (50 × 5 implicit branches) = ~643 effective branch-points, making K10's actual branching complexity comparable to K4 (662 FAV occurrences). Raw FAV_ count alone significantly undercounts K10's effective branching.

---

## U_汎用 (Generic Templates)

U_汎用 is architecturally distinct from character-specific kojo files. It provides generic NTR dialogue templates using a slot-17 naming convention.

| Metric | Count |
|--------|------:|
| FAV_ occurrences | 566 |
| TALENT:奴隷: occurrences | 778 |
| NTR_CHK calls | 627 |
| CHK_NTR_SATISFACTORY calls | 0 |
| NTR lines (total) | 9,330 |

**Files**: NTR口上.ERB, NTR口上_お持ち帰り.ERB, NTR口上_野外調教.ERB (3 files — unique third file not present in character-specific kojo)

**Naming convention**:
- NTR口上.ERB uses `@NTR_KOJO_K_{N}` and `@NTR_KOJO_KW_{N}` — standard and wife-mode variants
- NTR口上_野外調教.ERB uses `@NTR_KOJO_K_17_{N}` — slot 17 is the generic character slot

**CHK_NTR_SATISFACTORY = 0**: Satisfaction checking is handled differently in generic templates. U_汎用 defers satisfaction state management to the calling context rather than implementing it inline.

**Key differences from character-specific kojo**:
- Highest raw FAV_ count (566) among all analyzed targets, but this reflects template coverage breadth (must handle all characters generically) rather than depth
- Wife-mode (`KW`) variants are unique to U_汎用
- Outdoor training (`野外調教`) file has no equivalent in any character-specific directory
- Zero CHK_NTR_SATISFACTORY calls, contrasting with character-specific kojo (K10: 38, K5: 13)

---

## Content-Roadmap Gap Analysis (8h/8m/8n)

Gap analysis calibrated against K4 baseline: 8h=80%, 8m=10%, 8n=0%.

- **8h** (NTR kojo depth): Proportion of FAV levels covered, branching complexity relative to K4 (662 FAV occurrences = 80% baseline)
- **8m** (MC interaction): Presence of MC-perspective NTR dialogue. Most characters have minimal MC interaction content in NTR kojo files
- **8n** (Netorase): Explicit 寝取らせ content. Separate from NTR (寝取り/寝取られ); most characters have 0%

| Character | 8h (NTR kojo depth) | 8m (MC interaction) | 8n (Netorase) |
|-----------|:-------------------:|:-------------------:|:-------------:|
| K1 (美鈴) | 27% | 5% | 0% |
| K2 (小悪魔) | 41% | 5% | 0% |
| K3 (パチュリー) | 42% | 5% | 0% |
| K4 (咲夜) [baseline] | 80% | 10% | 0% |
| K5 (レミリア) | 70%* | 5% | 0% |
| K6 (フラン) | 43% | 5% | 0% |
| K8 (チルノ) | 36% | 5% | 0% |
| K9 (大妖精) | 27% | 5% | 0% |
| K10 (魔理沙) | 77%** | 5% | 0% |
| U_汎用 | 68%*** | 5% | 0% |

*K5 8h adjusted upward from raw FAV ratio (433/662 = 65%) to ~70% to reflect that the NTR性交拒否 state machine adds branching depth not visible in FAV_ grep count. The 13 CHK_NTR_SATISFACTORY occurrences further suggest substantial satisfaction-state coverage beyond raw FAV conditions.

**K10 8h adjusted upward from raw FAV ratio (393/662 = 59%) to account for MSG_NTR_SEX_10 helper with ~50 call sites (~5 branches each). Effective branch-point count (~643) places K10 near K4 baseline. Estimate: ~77%.

***U_汎用 8h reflects template breadth (566/662 = 85% raw ratio) discounted for generic-template nature — U_汎用 covers many situations shallowly rather than specific situations deeply. Estimate: ~68%.

**Gap summary**:
- 8h gaps: K1 (美鈴) and K9 (大妖精) are the lowest at ~27%, indicating the largest NTR kojo depth gaps. K10 (魔理沙) and K4 (咲夜) are near-complete.
- 8m gaps: All characters score 5-10% — MC-perspective NTR interaction dialogue is universally thin. Only K4 (baseline) has documented 10% coverage.
- 8n gaps: All characters score 0%. Netorase content (寝取らせ) is a separate content category not present in any current NTR kojo file.

---

## Cross-Character Pattern Summary

### Universal Patterns (Infrastructure-Defined)

The following patterns are universal — defined in NTR_UTIL.ERB and applied consistently across all characters:

- **11-level FAV system**: The `@NTR_CHK_FAVORABLY` function and its 11 FAV constants (FAV_キスする程度 through FAV_寝取り返し寸前) are universal infrastructure. Every per-character kojo file uses these constants; none define their own FAV levels.
- **TALENT:奴隷: condition framework**: The slave-talent condition is universal infrastructure applied universally across all characters as a branching gate for NTR content access.
- **NTR_CHK call pattern**: Universal infrastructure function calls are present in all character kojo files (K1: 241, K2: 359, ... K10: 445). The NTR_CHK infrastructure is shared.
- **2-file base structure**: Most characters follow the 2-file pattern (NTR口上.ERB + NTR口上_お持ち帰り.ERB). This is the universal structural convention.

### Character-Specific Patterns (Per-Character Implementations)

The following patterns are character-specific — per-character author decisions not replicated elsewhere:

- **K10 MSG_NTR_SEX_10 helper**: A character-specific helper with ~50 call sites that abstracts approximately 5 FAV branches per call. This per-character optimization concentrates branching logic in one helper, causing raw FAV_ counts to significantly undercount effective branching.
- **K5 NTR性交拒否 state machine**: A per-character CFLAG-based sex refusal subsystem (13 occurrences) that abstracts FAV branching into a dedicated state machine. Not present in any other character.
- **K4 scenario-split file structure**: 6 scenario-split files instead of the universal 2-file pattern. K4 is the only character with this decomposition — a per-character architectural decision by that character's author.
- **K8 dead code at lines 18/25**: Character-specific implementation artifact not present in other characters.

### Per-Character Variance

FAV_ occurrence range: 221 (K9) to 662 (K4), with U_汎用 at 566. Excluding K4 and U_汎用, the per-character range is 221 (K9) to 433 (K5), with a median of approximately 350. This substantial variance reflects the multi-author nature of the NTR kojo codebase — no single character's branching depth can be treated as representative of all others.

### U_汎用 Architectural Distinctness

U_汎用 is architecturally distinct from all per-character kojo:
- Uses slot-17 naming (`@NTR_KOJO_K_17_{N}`, `@NTR_KOJO_KW_{N}`) rather than the character-number convention
- Includes wife-mode variants (`KW`) absent from all character-specific kojo
- Has a third file (NTR口上_野外調教.ERB) absent from all character-specific directories
- CHK_NTR_SATISFACTORY = 0 — satisfaction state is handled externally (character-specific kojo handles it inline)

U_汎用 should be treated as a separate category in DDD design — it represents template coverage breadth across all generic NTR scenarios, not per-character depth.

### File Structure Summary

| Pattern | Characters |
|---------|-----------|
| 2-file (universal) | K1, K2, K3, K5, K6, K8, K9, K10 |
| 6-file scenario-split (per-character unique) | K4 only |
| 3-file (template-specific) | U_汎用 only |
| 0 NTR files (excluded) | K7 |
