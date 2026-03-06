# NTR DDD Design Input — Phase 24

This document provides empirically-grounded DDD (Domain-Driven Design) input for Phase 24 NTR Bounded Context design, derived from cross-character NTR kojo analysis (F847).

---

## Aggregate Candidates

### NtrProgression Aggregate

The NTR progression system tracks a character's journey from initial contact to full NTR state.

**Empirical basis**:
- Universal infrastructure: NTR_UTIL.ERB defines the 11-level FAV system via `@NTR_CHK_FAVORABLY(奴隷, 好感度LV)`. All 9 characters + U_汎用 share this progression model.
- Cross-character evidence: FAV_ usage ranges from 221 (K9) to 662 (K4), median approximately 350 across all characters. All characters branch on the same 11 FAV constants defined in NTR_UTIL.ERB.
- TALENT:奴隷: usage is universal: 230 (K9) to 731 (K10) occurrences per character.

**Aggregate boundary rationale**: All characters' NTR kojo branch on the same state (FAV level + TALENT:奴隷: conditions), making this the natural Aggregate boundary. The NtrProgression Aggregate would encapsulate:
- Current FAV level (from the 11-level FAV system)
- 屈服度/好感度 balance state
- TALENT flags (奴隷 and related)
- Progression history

**Cross-character validation**: The Aggregate boundary is grounded in universal infrastructure — not character-specific implementations. All 9 characters + U_汎用 use the same FAV system defined once in NTR_UTIL.ERB.

---

## Empirically Derived Value Object Candidates

These Value Object candidates were identified from K4 (Sakuya) pre-analysis (`phase-20-27-game-systems.md:497-514`) and validated as universal across all 9 characters + U_汎用 in F847 cross-character analysis.

### FavLevel Value Object

Represents one of the 11 FAV levels from the `@NTR_CHK_FAVORABLY` function in NTR_UTIL.ERB.

**Cross-character evidence**: All 9 characters use FAV_ conditions (221 to 662 occurrences per character). Every character branches on at least a subset of the 11 FAV constants. The FAV constants themselves are defined in NTR_UTIL.ERB — they are universal infrastructure, not per-character definitions.

**Validation status**: Validated — universal pattern across all characters without exception.

### AffairPermission Value Object

Represents the 浮気公認/浮気癖 modifier state that affects FAV level thresholds.

**Cross-character evidence**: TALENT:奴隷: usage is universal (230 to 731 occurrences per character) and includes 浮気公認 checks. The 浮気公認/浮気癖 conditions affect FAV_寝取られ寸前 and FAV_寝取られそう thresholds in NTR_CHK_FAVORABLY — defined in universal infrastructure, applied by all characters.

**Validation status**: Validated — the condition modifiers are part of the universal NTR_UTIL.ERB infrastructure.

### CorruptionState Value Object

Represents the 屈服度 vs 好感度 balance — the fundamental axis of the FAV system evaluation.

**Cross-character evidence**: NTR_CHK calls (216 to 627 per character) evaluate this state. The 11-level FAV system is fundamentally a CorruptionState evaluator: every FAV level threshold is defined in terms of the 屈服度/好感度 ratio. All characters call the same NTR_CHK infrastructure that evaluates this state.

**Validation status**: Validated — the 屈服度/好感度 balance is evaluated universally via NTR_UTIL.ERB infrastructure.

### PeepingContext Value Object

Represents the お持ち帰り (take-home/peeping) scenario context.

**Cross-character evidence**: All 9 characters + U_汎用 have a dedicated `NTR口上_お持ち帰り.ERB` file. This scenario type is universal — every character implements it as a separate file, making it a distinct scenario context in the domain model. No character omits this file.

**Validation status**: Validated — universal structural pattern (1 dedicated file per character across all 9 characters + U_汎用).

---

## Architecture-Designed Concepts (Empirical Grounding)

These are new concepts introduced in `phase-20-27-game-systems.md:575` for the C# DDD migration. They do not exist directly as named constructs in ERB source, but are grounded in empirically observed patterns from the cross-character analysis.

### NtrRoute Value Object (R0-R6)

Route classification for NTR progression paths.

**Architecture intent**: Provides a typed route identifier (R0-R6) to classify which NTR progression pathway a character is on.

**Empirical grounding**: Cross-character branching analysis reveals distinct progression pathways visible in FAV level distribution patterns. Characters with lower FAV counts (K1 at 223, K9 at 221) implement earlier-level FAV branches more densely; characters with higher counts (K4 at 662, K10 effective ~643) implement deep progression paths. The 11 FAV levels naturally cluster into route segments — early approach levels, mid corruption levels, and deep NTR completion levels. The SELECTCASE branching in NTR口上.ERB files implements route-like evaluation patterns across all characters.

**Grounding status**: Grounded — requires Phase 24 design decisions to formalize route boundaries from FAV-level clusters.

### NtrPhase Value Object (0-7)

Phase tracking within NTR progression.

**Architecture intent**: Provides sequential phase numbering (0-7) within an NTR route.

**Empirical grounding**: FAV levels map to progression phases; the SELECTCASE branching in NTR口上.ERB files shows sequential phase-like evaluation patterns across all characters. K5's NTR性交拒否 CFLAG state machine (13 occurrences) is a concrete example of explicit phase tracking within NTR progression — a per-character implementation of phase state that Phase 24 DDD would generalize. The universal FAV level ordering in NTR_UTIL.ERB (FAV_キスする程度 through FAV_寝取り返し寸前) provides the empirical basis for 8 progression phases (Phase 0 = no FAV, Phases 1-7 = FAV levels 11 through FAV_寝取られ).

**Grounding status**: Grounded — requires Phase 24 design decisions to map FAV levels to phase numbers.

### NtrParameters Value Object

Encapsulates condition parameters for NTR evaluation.

**Architecture intent**: Provides a typed container for the parameters passed to NTR evaluation functions, replacing loose parameter passing.

**Empirical grounding**: `@NTR_CHK_FAVORABLY(奴隷, 好感度LV)` takes (奴隷, 好感度LV) as explicit parameters — the empirical basis for the NtrParameters Value Object. Per-character kojo adds CFLAG conditions on top of these base parameters. K10's MSG_NTR_SEX_10 helper (approximately 50 call sites) demonstrates parameter-passing patterns at scale — this helper encapsulates parameter bundles for repeated NTR sex evaluation. CHK_NTR_SATISFACTORY usage varies dramatically across characters (0 for U_汎用, 3 for K1/K2/K8, 38 for K10), suggesting optional parameter inclusion in the NtrParameters object for satisfaction-state tracking.

**Grounding status**: Grounded — requires Phase 24 design decisions to finalize which parameters are mandatory vs optional in NtrParameters.

---

## Cross-Character Evidence Summary

The following table summarizes the empirical data from `pm/reference/ntr-kojo-analysis.md` that grounds each DDD candidate.

| DDD Candidate | Type | Cross-Character Evidence | K4 Baseline | Validation Status |
|---------------|------|------------------------|-------------|-------------------|
| NtrProgression | Aggregate | All 9+U_汎用 share FAV/TALENT branching model | 662 FAV_, 16,146 lines | Validated: universal pattern |
| FavLevel | Value Object | 221-662 FAV_ occurrences across all characters | 662 FAV_ | Validated: universal |
| AffairPermission | Value Object | 230-731 TALENT:奴隷: across all characters | — | Validated: universal |
| CorruptionState | Value Object | 216-627 NTR_CHK calls across all characters | — | Validated: universal |
| PeepingContext | Value Object | All chars have NTR口上_お持ち帰り.ERB | 1 of 6 files | Validated: universal |
| NtrRoute (R0-R6) | Value Object | FAV level distribution patterns per character | New concept | Grounded: requires Phase 24 design |
| NtrPhase (0-7) | Value Object | Sequential FAV evaluation + K5 state machine | New concept | Grounded: requires Phase 24 design |
| NtrParameters | Value Object | NTR_CHK params + K10 helper + CHK_SAT variance | New concept | Grounded: requires Phase 24 design |

**Distinction**:
- **Validated**: Directly observable in ERB source across all characters. The pattern exists in the codebase right now.
- **Grounded**: New concept with empirical support from cross-character analysis, but requiring Phase 24 design decisions to formalize as a typed C# construct.

**Data sources**: All counts derived from Task 1 grep analysis (F847). K4 data from `docs/architecture/migration/phase-20-27-game-systems.md:395-541`. Architecture-designed concepts from `phase-20-27-game-systems.md:575`.
