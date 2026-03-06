<!-- fc-phase-1-completed -->

# Feature 847: Phase 23 NTR Kojo Reference Analysis

## Status: [DONE]
<!-- fl-reviewed: 2026-03-06T21:29:37Z -->

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When you discover an issue that is OUT OF SCOPE but needs attention:
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - If user approves, create a new feature-{ID}.md
> 4. **LINK** - Reference the new feature in the Links section below
>
> **Rationale**: Skipping is acceptable. Forgetting is not. All discovered issues must be tracked.

## Type: research

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity — each phase produces analysis documents that feed the next phase's design. Research phases generate empirical reference documents that serve as direct input to subsequent design and implementation phases, ensuring each phase is grounded in concrete data rather than assumptions.

### Problem (Current Issue)

The K4 (Sakuya) pre-analysis in `docs/architecture/migration/phase-20-27-game-systems.md:395-541` covers only 1 of 10 characters, and K4 is a statistical outlier (16,146 NTR lines, 6 scenario-split files) compared to other characters (median 2,000-5,000 lines, 2 files each). Because the NTR kojo codebase was built by multiple authors over time with character-specific variations (K10 helper functions like `MSG_NTR_SEX_10`, K5 sex refusal subsystem `NTR性交拒否`, U_汎用 different naming convention `@NTR_KOJO_K_17_*`), the K4-only analysis risks producing DDD Value Object and Aggregate boundaries that over-specify for simpler characters or miss character-specific patterns entirely. The shared NTR infrastructure in `C:\Era\game\ERB\NTR\NTR_UTIL.ERB:1040` defines the universal 11-level FAV system, but individual characters implement varying subsets of these levels, meaning only empirical cross-character analysis can distinguish universal patterns from character-specific ones.

### Goal (What to Achieve)

Produce `pm/reference/ntr-kojo-analysis.md` (all-character NTR branch statistics) and `pm/reference/ntr-ddd-input.md` (Phase 24 Value Object/Aggregate design input) by performing NTR kojo analysis across all NTR characters (K1-K6, K8-K10, U_汎用; K7 excluded due to zero NTR files). Also perform content-roadmap 8h/8m/8n gap analysis update based on full-character findings. Distinguish universal NTR infrastructure patterns (NTR_UTIL.ERB) from character-specific kojo patterns to ground DDD design in empirical cross-character data.

Methodology: NTR branch pattern analysis -- read ERB kojo files in `C:\Era\game\ERB\口上\`, count FAV/TALENT/situation condition occurrences per character. K4 (Sakuya) pre-analysis at `docs/architecture/migration/phase-20-27-game-systems.md:395-541` is the baseline reference (not re-derived); this feature extends the same methodology to all other characters.

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is Phase 24 DDD design at risk? | The empirical foundation covers only K4 (Sakuya) out of 10 characters | `phase-20-27-game-systems.md:396` (K4 pre-analysis) |
| 2 | Why does only K4 have analysis? | K4 was used as the reference implementation because she has the largest NTR codebase (16,146 lines, 6 files) | `C:\Era\game\ERB\口上\4_咲夜\NTR口上_シナリオ*.ERB` |
| 3 | Why is K4 insufficient as sole reference? | K4 is a statistical outlier: 662 FAV occurrences vs K1's 129, K8's 209; unique scenario-split file structure not replicated in any other character | Grep counts across character NTR ERB files |
| 4 | Why do characters have such different branching complexity? | Different authors wrote each character's NTR kojo with varying coding styles, helper abstractions, and FAV/TALENT subset coverage | K10 `MSG_NTR_SEX_10` at `NTR口上.ERB:37`; K5 `NTR性交拒否` at `NTR口上.ERB:53` |
| 5 | Why (Root)? | Multi-author NTR kojo codebase has no enforced conventions; character-specific abstractions and branching patterns vary significantly atop the shared NTR infrastructure (`NTR_UTIL.ERB:1040`), making single-character analysis non-representative | `NTR_UTIL.ERB:1040-1099` (universal 11 FAV levels); character dirs show 129-662 FAV range |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Only K4 has NTR kojo analysis; other 9 characters unanalyzed | Multi-author codebase created character-specific variations that cannot be inferred from any single character's analysis |
| Where | `phase-20-27-game-systems.md:395-541` (K4 section only) | `C:\Era\game\ERB\口上\` (9 character dirs + U_汎用, each with different author conventions) |
| Fix | Assume K4 patterns apply to all characters | Empirically analyze all characters to distinguish universal patterns (shared NTR infra) from character-specific ones (per-author kojo) |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F827 | [DONE] | Predecessor: Phase 23 Planning, created this DRAFT, defined methodology and deliverables |
| F848 | [DRAFT] | Successor: Post-Phase Review Phase 23, depends on F847 completion |
| F849 | [DRAFT] | Successor: Phase 24 Planning, depends on F848 |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Source data accessible | FEASIBLE | All NTR ERB files exist in `C:\Era\game\ERB\口上\` for K1-K6, K8-K10, U_汎用 |
| K4 methodology replicable | FEASIBLE | Same FAV_*/TALENT:奴隷:*/NTR_CHK constructs used across all characters (CHK_NTR_SATISFACTORY: 96 occurrences across 12 files in 9 dirs) |
| K7 exclusion justified | FEASIBLE | Zero NTR files confirmed by glob |
| Deliverable paths clear | FEASIBLE | `pm/reference/ntr-kojo-analysis.md` and `pm/reference/ntr-ddd-input.md` defined in architecture doc |
| No code changes required | FEASIBLE | Type: research; output is markdown documents only |
| Cross-character variations manageable | FEASIBLE | Variations (K10 helpers, K5 sex refusal, U_汎用 naming) are documentable without methodology change |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Phase 24 DDD Design | HIGH | Provides empirical foundation for Value Object and Aggregate boundary decisions; without this, DDD design is speculative |
| Content Roadmap | MEDIUM | 8h/8m/8n gap analysis per character enables accurate content planning across all NTR characters |
| Phase 23 Completion | HIGH | F847 is the sole remaining Phase 23 task; blocking F848 (Post-Phase Review) and F849 (Phase 24 Planning) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| K7 (子悪魔) has zero NTR kojo files | Glob result (confirmed by all 3 investigations) | Analysis covers 9 characters + U_汎用, not 10; K7 exclusion must be documented |
| K4 pre-analysis must not be re-derived | `feature-827.md` Key Decision | F847 references K4 data from architecture doc; extends methodology to other characters |
| Character-specific helpers abstract branching | K10 `MSG_NTR_SEX_10`, K5 `NTR性交拒否` | Raw grep counts may undercount actual branching; analysis must note abstracted branching per character |
| U_汎用 uses different naming convention | `@NTR_KOJO_K_17_*` vs `@NTR_KOJO_K{N}_*` | Must treat U_汎用 as separate category (generic templates) in analysis |
| ERB is source of truth for branching | YAML files exist alongside ERB but contain migrated dialogue | Analyze ERB branching patterns, not YAML content |
| Game repo is external | 5-repo split | Use Read/Grep on `C:\Era\game\`, not lsp.py |
| Shared NTR infrastructure defines universal patterns | `C:\Era\game\ERB\NTR\NTR_UTIL.ERB:1040` (11 FAV levels) | Analysis must distinguish infrastructure-defined (universal) vs kojo-implemented (per-character) conditions |
| K4 unique file structure | Only K4 has `NTR口上_シナリオ*.ERB` scenario-based decomposition | Document K4 structural divergence; other characters use 2-file pattern |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| FAV/TALENT grep counts overcount (commented-out code, non-NTR usage) | MEDIUM | LOW | Sample validation: compare grep count vs manual inspection for 1-2 characters |
| Character-specific abstractions cause undercounting | MEDIUM | MEDIUM | Document helper functions per character; analyze helper definitions separately |
| U_汎用 analysis complicates DDD model (generic vs character-specific) | LOW | MEDIUM | Separate U_汎用 statistics from character-specific ones in analysis doc |
| Content-roadmap gap analysis requires domain judgment beyond pattern counting | LOW | MEDIUM | Use K4 pre-analysis (8h: 80%, 8m: 10%, 8n: 0%) as calibration template |
| K4 outlier inflates DDD expectations for simpler characters | HIGH | MEDIUM | Include per-character stats with variance metrics; note K4 as upper bound |
| DDD Value Object candidates may change when full data is available | LOW | LOW | DDD input doc is advisory; Phase 24 will refine based on actual design |
| K8 contains dead code at lines 18/25 | LOW | LOW | Document in analysis; does not affect methodology |
| K3 partial YAML migration may contain NTR content | LOW | LOW | Check both ERB and YAML; note discrepancies if found |

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Output is 2 markdown files, no code | `phase-20-27-game-systems.md:551-556` | ACs use exists + content matchers (Grep contains/matches), not build/test |
| C2 | All NTR characters must be analyzed (K1-K6, K8-K10, U_汎用) | `phase-20-27-game-systems.md:559` | AC must verify presence of statistics for 9 characters + U_汎用 (K7 excluded) |
| C3 | K4 pre-analysis is referenced, not re-derived | `feature-827.md` Key Decision | AC should verify K4 section references architecture doc, not duplicate data |
| C4 | DDD Value Object candidates required in ntr-ddd-input.md | `phase-20-27-game-systems.md:560` | AC must verify Value Object / Aggregate section exists with empirical derivation |
| C5 | Content-roadmap 8h/8m/8n gap analysis required | `phase-20-27-game-systems.md:561` | AC must verify gap assessment for all three content phases per character |
| C6 | K7 exclusion must be documented | 0 NTR files (confirmed by all investigations) | AC should verify K7 is explicitly noted as excluded with reason |
| C7 | Character-specific variations must be documented | K10 helpers, K5 sex refusal, U_汎用 naming | AC should verify analysis notes per-character unique patterns |
| C8 | U_汎用 analyzed separately from character-specific kojo | `@NTR_KOJO_K_17_*` naming (generic templates) | AC should verify U_汎用 has separate section in analysis |
| C9 | ERB is branching source of truth | YAML contains migrated dialogue, not branching logic | ACs must target ERB files, not YAML |
| C10 | Shared NTR infrastructure provides universal domain model | `NTR_UTIL.ERB:1040-1099` (11 FAV levels) | AC should verify analysis distinguishes universal vs character-specific patterns |

### Constraint Details

**C1: Research-only output**
- **Source**: Architecture doc Phase 23 task definition
- **Verification**: `pm/reference/ntr-kojo-analysis.md` and `pm/reference/ntr-ddd-input.md` exist after completion
- **AC Impact**: All ACs use exists and Grep matchers; no dotnet_test or build verification needed

**C2: Full character coverage**
- **Source**: Phase 23 scope definition requiring all-character analysis
- **Verification**: Each of K1, K2, K3, K4, K5, K6, K8, K9, K10, U_汎用 has a statistics section in ntr-kojo-analysis.md
- **AC Impact**: AC must verify presence of all 10 analysis targets (9 characters + U_汎用)
- **Collection Members** (MANDATORY): K1 (美鈴), K2 (小悪魔), K3 (パチュリー), K4 (咲夜), K5 (レミリア), K6 (フラン), K7 (子悪魔, excluded), K8 (チルノ), K9 (大妖精), K10 (魔理沙), U_汎用 (generic templates)

**C4: DDD Value Object candidates**
- **Source**: Architecture doc Phase 23 pre-analysis (`phase-20-27-game-systems.md:497-514`) derived empirical VO candidates: FavLevel, AffairPermission, CorruptionState, PeepingContext + NtrProgression Aggregate. Phase 24 (`phase-20-27-game-systems.md:575`) introduces new design concepts (NtrRoute R0-R6, NtrPhase 0-7) that don't exist in ERB source.
- **Verification**: ntr-ddd-input.md contains VO/Aggregate candidate section distinguishing (a) empirically derived VOs validated across all characters and (b) architecture-designed concepts requiring empirical grounding
- **AC Impact**: AC should verify VO candidates are grounded in cross-character data, not just K4

**C5: 8h/8m/8n gap analysis**
- **Source**: Content-roadmap phases 8h (NTR kojo depth), 8m (MC interaction), 8n (Netorase)
- **Verification**: Gap assessment per character for all 3 content phases exists
- **AC Impact**: AC must verify all 3 phases are assessed, with per-character breakdown

**C3: K4 pre-analysis referenced, not re-derived**
- **Source**: `feature-827.md` Key Decision
- **Verification**: ntr-kojo-analysis.md K4 section references `phase-20-27-game-systems.md:395-541` instead of re-counting
- **AC Impact**: AC#3 verifies K4 presence; implementation must import K4 data by reference

**C6: K7 exclusion must be documented**
- **Source**: K7 (子悪魔) has zero NTR files (confirmed by glob across all investigations)
- **Verification**: ntr-kojo-analysis.md contains explicit K7 exclusion statement with reason
- **AC Impact**: AC#6 verifies K7 exclusion via regex pattern match

**C7: Character-specific variations documented**
- **Source**: K10 `MSG_NTR_SEX_10` helpers, K5 `NTR性交拒否` subsystem, U_汎用 `@NTR_KOJO_K_17_*` naming
- **Verification**: Per-character sections in ntr-kojo-analysis.md note unique patterns where applicable
- **AC Impact**: Covered implicitly by AC#3 (full coverage); specific variation documentation is a quality concern

**C8: U_汎用 analyzed separately**
- **Source**: U_汎用 uses `@NTR_KOJO_K_17_*` naming convention (generic templates, not character-specific)
- **Verification**: ntr-kojo-analysis.md has a dedicated U_汎用 section separate from per-character statistics
- **AC Impact**: AC#3 includes U_汎用 in the 10-target coverage pattern

**C9: ERB is branching source of truth**
- **Source**: YAML files contain migrated dialogue text, not IF/ELSEIF branching logic
- **Verification**: Analysis methodology targets ERB files for condition counting
- **AC Impact**: All AC Methods reference ERB-derived content in the analysis documents

**C10: Universal vs character-specific distinction**
- **Source**: `NTR_UTIL.ERB:1040-1099` defines 11 FAV levels; 14 shared NTR system files in `C:\Era\game\ERB\NTR\`
- **Verification**: Analysis document has section distinguishing shared infrastructure patterns from per-character kojo patterns
- **AC Impact**: AC should verify this distinction exists in the analysis output

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F827 | [DONE] | Phase 23 Planning -- created this DRAFT, defined methodology and deliverables |
| Successor | F848 | [DRAFT] | Post-Phase Review Phase 23, blocked on F847 completion |
| Successor | F849 | [DRAFT] | Phase 24 Planning, blocked on F848 |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block

Example:
| Predecessor | F540 | [DONE] | Era.Core setup required |
| Successor | F567 | [BLOCKED] | F567 depends on this feature |
| Related | F100 | [DONE] | Reference implementation |
-->

---

<!-- fc-phase-3-completed -->

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "each phase produces analysis documents" | Both deliverable files (ntr-kojo-analysis.md, ntr-ddd-input.md) must be produced | AC#1, AC#2 |
| "direct input to subsequent design and implementation phases" | ntr-ddd-input.md must contain DDD Value Object/Aggregate candidates usable by Phase 24 | AC#4 |
| "ensuring each phase is grounded in concrete data rather than assumptions" | Analysis must cover all NTR characters empirically (not infer from K4 alone) | AC#3 |
| "concrete data rather than assumptions" | Gap analysis must provide per-character 8h/8m/8n assessment grounded in ERB branch data | AC#5 |
| "concrete data rather than assumptions" | Analysis must distinguish universal NTR infrastructure patterns from per-character kojo patterns (C10) | AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ntr-kojo-analysis.md exists | file | Glob(pm/reference/ntr-kojo-analysis.md) | exists | 1 | [x] |
| 2 | ntr-ddd-input.md exists | file | Glob(pm/reference/ntr-ddd-input.md) | exists | 1 | [x] |
| 3 | All 10 analysis targets covered in ntr-kojo-analysis.md | file | Grep(pm/reference/ntr-kojo-analysis.md, pattern="K1[^0-9]|K2[^0-9]|K3[^0-9]|K4[^0-9]|K5[^0-9]|K6[^0-9]|K8[^0-9]|K9[^0-9]|K10|U_汎用") | gte | 20 | [x] |
| 4 | DDD Value Object/Aggregate candidates in ntr-ddd-input.md | file | Grep(pm/reference/ntr-ddd-input.md, pattern="Value Object|Aggregate") | gte | 4 | [x] |
| 5 | 8h/8m/8n gap analysis present in ntr-kojo-analysis.md | file | Grep(pm/reference/ntr-kojo-analysis.md, pattern="8h|8m|8n") | gte | 10 | [x] |
| 6 | K7 exclusion documented with reason | file | Grep(pm/reference/ntr-kojo-analysis.md, pattern="K7.*excluded|K7.*zero|K7.*0.*NTR") | matches | K7.*excluded|K7.*zero|K7.*0.*NTR | [x] |
| 7 | Universal vs character-specific pattern distinction | file | Grep(pm/reference/ntr-kojo-analysis.md, pattern="universal|infrastructure|character-specific|per-character") | gte | 3 | [x] |

### AC Details

**AC#3: All 10 analysis targets covered in ntr-kojo-analysis.md**
- **Test**: `Grep(pm/reference/ntr-kojo-analysis.md, pattern="K1[^0-9]|K2[^0-9]|K3[^0-9]|K4[^0-9]|K5[^0-9]|K6[^0-9]|K8[^0-9]|K9[^0-9]|K10|U_汎用")`
- **Expected**: gte 20 (9 characters K1-K6, K8-K10 + U_汎用 = 10 analysis targets; K7 excluded due to zero NTR files; each target appears in section heading + at least 1 statistics row = 2 minimum mentions × 10 = 20; K1-K9 patterns use `[^0-9]` suffix to prevent K1 matching K10 etc.)
- **Rationale**: C2 constraint requires all NTR characters analyzed. Each character must have a dedicated section with actual data, not just a mention in a summary sentence. Threshold 20 ensures each target appears in both structural (heading) and content (data) contexts.
- **Derivation**: K1 (美鈴), K2 (小悪魔), K3 (パチュリー), K4 (咲夜), K5 (レミリア), K6 (フラン), K8 (チルノ), K9 (大妖精), K10 (魔理沙), U_汎用 = 10 targets × 2 minimum mentions = 20

**AC#4: DDD Value Object/Aggregate candidates in ntr-ddd-input.md**
- **Test**: `Grep(pm/reference/ntr-ddd-input.md, pattern="Value Object|Aggregate")`
- **Expected**: gte 4 (Aggregate Candidates section heading + Empirically Derived Value Object Candidates section heading + at least 2 candidate descriptions mentioning "Value Object" or "Aggregate" = 4 minimum)
- **Rationale**: C4 constraint requires DDD VO/Aggregate section with empirical derivation. Phase 24 design depends on this input. Threshold 4 ensures both section headings exist AND at least 2 candidate descriptions reference the concepts.
- **Derivation**: 2 section headings ("Aggregate Candidates", "Value Object Candidates") + 2 candidate descriptions = 4 minimum.

**AC#5: 8h/8m/8n gap analysis present in ntr-kojo-analysis.md**
- **Test**: `Grep(pm/reference/ntr-kojo-analysis.md, pattern="8h|8m|8n")`
- **Expected**: gte 10 (C5 constraint: gap assessment for all 3 content phases must exist with per-character coverage)
- **Rationale**: Content-roadmap phases 8h (NTR kojo depth), 8m (MC interaction), 8n (Netorase) each need per-character gap assessment. Threshold 10 ensures content beyond just section headers (3 labels × section title + table header = 6 minimum; per-character data rows add more).
- **Derivation**: 3 content phases appear in: section title (3), table header (3), plus at least 4 data rows mentioning phase labels = 10 minimum.

**AC#7: Universal vs character-specific pattern distinction**
- **Test**: `Grep(pm/reference/ntr-kojo-analysis.md, pattern="universal|infrastructure|character-specific|per-character")`
- **Expected**: gte 3 (C10 constraint: analysis must distinguish universal NTR infrastructure patterns from per-character kojo patterns; the Cross-Character Pattern Summary section uses these terms)
- **Rationale**: C10 constraint and Philosophy claim "concrete data rather than assumptions" require distinguishing shared NTR infrastructure (NTR_UTIL.ERB) from per-character implementations. Minimum 3 ensures the distinction section exists with meaningful content.
- **Derivation**: Cross-Character Pattern Summary section uses at least: "universal" (1), "infrastructure" or "character-specific" (1), "per-character" (1) = 3 minimum

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Produce pm/reference/ntr-kojo-analysis.md (all-character NTR branch statistics) | AC#1, AC#3 |
| 2 | Produce pm/reference/ntr-ddd-input.md (Phase 24 Value Object/Aggregate design input) | AC#2, AC#4 |
| 3 | Perform NTR kojo analysis across all NTR characters (K1-K6, K8-K10, U_汎用; K7 excluded) | AC#3, AC#6 |
| 4 | Perform content-roadmap 8h/8m/8n gap analysis update based on full-character findings | AC#5 |
| 5 | Distinguish universal NTR infrastructure patterns from character-specific kojo patterns | AC#7 |

---

<!-- fc-phase-4-completed -->

## Technical Design

### Approach

This is a research feature producing two markdown analysis documents. The implementation approach is: systematic ERB grep analysis across all 9 NTR character directories plus U_汎用, followed by authoring two structured documents with findings.

**Document 1: `pm/reference/ntr-kojo-analysis.md`**
- Per-character statistics sections for K1-K6, K8-K10, U_汎用 (K7 explicitly excluded)
- FAV occurrence counts, TALENT condition counts, NTR_CHK/CHK_NTR_SATISFACTORY usage per character
- Per-character unique patterns (K10 helper abstraction, K5 sex refusal, U_汎用 naming)
- U_汎用 analyzed in a separate section (not mixed with character-specific kojo)
- Cross-character comparison: universal (NTR_UTIL.ERB infrastructure) vs per-character branching
- Content-roadmap 8h/8m/8n gap assessment per character (calibrated against K4 baseline: 8h=80%, 8m=10%, 8n=0%)
- K7 exclusion documented with reason (zero NTR kojo files confirmed)

**Document 2: `pm/reference/ntr-ddd-input.md`**
- Value Object and Aggregate candidate section derived from cross-character empirical data
- Candidates expected: NtrProgression Aggregate, FavLevel VO, NtrRoute VO, NtrPhase VO, NtrParameters VO
- Each candidate grounded in cross-character evidence (not K4 extrapolation alone)

**Analysis methodology** (replicating K4 baseline methodology from `phase-20-27-game-systems.md:395-541`):
1. For each character directory: Grep ERB files for `FAV_`, `TALENT:奴隷:`, `NTR_CHK`, `CHK_NTR_SATISFACTORY`
2. Document raw counts and note abstracted branching (helper functions that may undercount)
3. Identify character-specific constructs (K10 `MSG_NTR_SEX_10`, K5 `NTR性交拒否`, U_汎用 `@NTR_KOJO_K_17_*`)
4. Reference K4 pre-analysis from architecture doc (not re-derived)
5. Synthesize DDD candidates from cross-character patterns in ntr-ddd-input.md

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `pm/reference/ntr-kojo-analysis.md` file during Task execution |
| 2 | Create `pm/reference/ntr-ddd-input.md` file during Task execution |
| 3 | Include a dedicated statistics section for each of K1, K2, K3, K4, K5, K6, K8, K9, K10, U_汎用 in ntr-kojo-analysis.md — using character identifiers K1 through K10 and "U_汎用" as section headings or within section headings, ensuring all 10 identifiers appear at least once |
| 4 | Include a "Value Object" candidates section and an "Aggregate" candidates section in ntr-ddd-input.md — both terms will appear multiple times given the 5 expected candidates (1 Aggregate + 4 VOs) |
| 5 | Include a content-roadmap gap analysis section in ntr-kojo-analysis.md that references phases 8h, 8m, and 8n — all three strings will appear in the gap analysis per-character table |
| 6 | Include an explicit K7 exclusion statement in ntr-kojo-analysis.md using language like "K7 excluded" or "K7: zero NTR files" that matches the AC#6 regex pattern `K7.*excluded\|K7.*zero\|K7.*0.*NTR` |
| 7 | Include cross-character pattern distinction in ntr-kojo-analysis.md using terms like "universal", "infrastructure", "character-specific", "per-character" to distinguish shared NTR infrastructure patterns from per-character kojo patterns |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| K4 data handling | A: Re-derive K4 stats; B: Reference architecture doc pre-analysis | B: Reference pre-analysis | C3 constraint: F827 Key Decision mandates no re-derivation; architecture doc `phase-20-27-game-systems.md:395-541` is authoritative baseline |
| U_汎用 placement | A: Merge with character sections; B: Separate section at end | B: Separate section | C8 constraint: U_汎用 uses different naming convention (`@NTR_KOJO_K_17_*`); generic templates are architecturally distinct from character-specific kojo |
| Abstracted branching documentation | A: Count only raw grep hits; B: Note abstractions separately | B: Note abstractions | Risk mitigation: K10's `MSG_NTR_SEX_10` and K5's `NTR性交拒否` abstract branching not visible in FAV grep counts — must document to prevent undercounting |
| 8h/8m/8n calibration reference | A: Derive from scratch per character; B: Use K4 baseline percentages as calibration | B: K4 as calibration | K4 pre-analysis provides concrete calibration (8h=80%, 8m=10%, 8n=0%); applying same framework cross-character ensures comparable assessments |
| Universal vs character-specific distinction | A: Embed in per-character sections; B: Dedicate a cross-character comparison section | B: Dedicated section | C10 constraint: `NTR_UTIL.ERB:1040-1099` defines shared 11-level FAV system — universal patterns must be distinguished from per-character implementations; cross-character section satisfies this clearly |

### Interfaces / Data Structures

<!-- No code interfaces. This is a research feature producing markdown documents only. -->

Document structure for `ntr-kojo-analysis.md`:
```
# NTR Kojo Analysis

## Universal NTR Infrastructure (NTR_UTIL.ERB)
- 11 FAV levels, shared CHK functions, universal patterns

## Per-Character Statistics
### K1 (美鈴)
### K2 (小悪魔)
### K3 (パチュリー)
### K4 (咲夜) [Reference — see phase-20-27-game-systems.md:395-541]
### K5 (レミリア)
### K6 (フラン)
### K8 (チルノ)
### K9 (大妖精)
### K10 (魔理沙)
### K7 (子悪魔) — Excluded (zero NTR kojo files)

## U_汎用 (Generic Templates)

## Content-Roadmap Gap Analysis (8h/8m/8n)
| Character | 8h | 8m | 8n |
...

## Cross-Character Pattern Summary
```

Document structure for `ntr-ddd-input.md`:
```
# NTR DDD Design Input — Phase 24

## Aggregate Candidates
- NtrProgression Aggregate (empirical basis: cross-character data)

## Empirically Derived Value Object Candidates
- FavLevel VO (K4-derived, validated cross-character)
- AffairPermission VO (K4-derived, validated cross-character)
- CorruptionState VO (K4-derived, validated cross-character)
- PeepingContext VO (K4-derived, validated cross-character)

## Architecture-Designed Concepts (Empirical Grounding)
- NtrRoute VO (R0-R6) — new concept for C# migration, not in ERB; empirical grounding from FAV/branching analysis
- NtrPhase VO (0-7) — new concept; empirical grounding from progression pattern analysis
- NtrParameters VO — new concept; empirical grounding from condition parameter patterns

## Cross-Character Evidence Summary
```

### Upstream Issues

<!-- No upstream issues found during Technical Design. AC patterns are grounded in concrete document structures designed here. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

<!-- fc-phase-5-completed -->

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 3, 5, 6 | Grep ERB files in `C:\Era\game\ERB\口上\` for each character directory (K1-K3, K5-K6, K8-K10, U_汎用): count FAV_, TALENT:奴隷:, NTR_CHK, CHK_NTR_SATISFACTORY occurrences; identify character-specific constructs (K10 MSG_NTR_SEX_10, K5 NTR性交拒否, U_汎用 @NTR_KOJO_K_17_*); K4 data is referenced from architecture doc, not re-derived | [I] | [x] |
| 2 | 1, 3, 5, 6, 7 | Author `pm/reference/ntr-kojo-analysis.md` with: universal NTR infrastructure section (NTR_UTIL.ERB 11 FAV levels), per-character statistics sections for K1-K6, K8-K10 (K4 as reference to architecture doc), K7 exclusion statement (zero NTR kojo files), U_汎用 separate section, content-roadmap 8h/8m/8n gap analysis table, cross-character pattern summary distinguishing universal vs character-specific patterns | | [x] |
| 3 | 2, 4 | Author `pm/reference/ntr-ddd-input.md` with: Aggregate candidates section (NtrProgression), empirically derived VO candidates section (FavLevel, AffairPermission, CorruptionState, PeepingContext — validated cross-character), architecture-designed concepts section (NtrRoute, NtrPhase, NtrParameters — empirical grounding from branching analysis), cross-character evidence summary | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | researcher | sonnet | `C:\Era\game\ERB\口上\` (all character dirs), `docs/architecture/migration/phase-20-27-game-systems.md:395-541` (K4 baseline) | grep count data per character (Task 1) |
| 2 | researcher | sonnet | Phase 1 grep data, K4 pre-analysis baseline | `pm/reference/ntr-kojo-analysis.md` (Task 2) |
| 3 | researcher | sonnet | Phase 1 grep data, Phase 2 ntr-kojo-analysis.md | `pm/reference/ntr-ddd-input.md` (Task 3) |

### Pre-conditions

- `C:\Era\game\ERB\口上\` is accessible with character subdirectories for K1-K6, K8-K10, and U_汎用
- K7 (子悪魔) has zero NTR kojo files (confirmed; excluded from analysis)
- `docs/architecture/migration/phase-20-27-game-systems.md:395-541` is readable (K4 pre-analysis baseline)
- `pm/reference/` directory exists and is writable

### Execution Order

**Phase 1 — ERB Grep Analysis (Task 1)**

1. For each character directory in `C:\Era\game\ERB\口上\`: K1 (美鈴), K2 (小悪魔), K3 (パチュリー), K5 (レミリア), K6 (フラン), K8 (チルノ), K9 (大妖精), K10 (魔理沙), U_汎用 — grep all `.ERB` files for:
   - `FAV_` (FAV condition occurrences)
   - `TALENT:奴隷:` (slave TALENT conditions)
   - `NTR_CHK` (NTR check calls)
   - `CHK_NTR_SATISFACTORY` (satisfaction check calls)
2. Record raw counts per character. Note helper abstractions that may undercount raw FAV_ hits:
   - K10: `MSG_NTR_SEX_10` helper function abstracts branching
   - K5: `NTR性交拒否` sex refusal subsystem abstracts branching
   - U_汎用: uses `@NTR_KOJO_K_17_*` naming convention (not `@NTR_KOJO_K{N}_*`)
3. Do NOT re-derive K4 data — reference `docs/architecture/migration/phase-20-27-game-systems.md:395-541` for K4 (咲夜) statistics

**Phase 2 — Author ntr-kojo-analysis.md (Task 2)**

4. Create `pm/reference/ntr-kojo-analysis.md` with the following sections in order:
   a. `## Universal NTR Infrastructure (NTR_UTIL.ERB)` — document the 11-level FAV system from `NTR_UTIL.ERB:1040-1099`; these are the universal patterns shared across all characters
   b. `## Per-Character Statistics` — subsections for each character:
      - `### K1 (美鈴)` through `### K10 (魔理沙)` (excluding K7), each with FAV_, TALENT:奴隷:, NTR_CHK, CHK_NTR_SATISFACTORY counts and any character-specific construct notes
      - `### K4 (咲夜)` — reference section only: "See `docs/architecture/migration/phase-20-27-game-systems.md:395-541`"; do NOT re-derive
      - `### K7 (子悪魔) — Excluded (zero NTR kojo files)` — explicit exclusion statement matching pattern `K7.*excluded|K7.*zero|K7.*0.*NTR`
   c. `## U_汎用 (Generic Templates)` — separate section (NOT inside Per-Character Statistics) with U_汎用 grep counts and naming convention notes (`@NTR_KOJO_K_17_*`)
   d. `## Content-Roadmap Gap Analysis (8h/8m/8n)` — table with columns: Character | 8h (NTR kojo depth) | 8m (MC interaction) | 8n (Netorase), one row per character; calibrate against K4 baseline (8h=80%, 8m=10%, 8n=0%); all three phase labels `8h`, `8m`, `8n` must appear in this section
   e. `## Cross-Character Pattern Summary` — distinguish universal patterns (NTR_UTIL.ERB infrastructure-defined) from per-character kojo patterns (per-author implementations)

**Phase 3 — Author ntr-ddd-input.md (Task 3)**

5. Create `pm/reference/ntr-ddd-input.md` with the following sections:
   a. `## Aggregate Candidates` — document NtrProgression Aggregate with cross-character empirical evidence (not K4 extrapolation alone)
   b. `## Empirically Derived Value Object Candidates` — document FavLevel VO, AffairPermission VO, CorruptionState VO, PeepingContext VO (K4-derived candidates validated across all characters)
   c. `## Architecture-Designed Concepts (Empirical Grounding)` — document NtrRoute VO (R0-R6), NtrPhase VO (0-7), NtrParameters VO (new concepts per `phase-20-27-game-systems.md:575`) with empirical grounding from cross-character branching analysis
   d. `## Cross-Character Evidence Summary` — summarize the data from ntr-kojo-analysis.md that grounds each DDD candidate

### Success Criteria

- `pm/reference/ntr-kojo-analysis.md` exists with all 10 analysis targets present (K1-K6, K8-K10, U_汎用) and K7 explicitly excluded
- `pm/reference/ntr-ddd-input.md` exists with both "Value Object" and "Aggregate" sections containing empirically grounded candidates
- 8h, 8m, and 8n all appear in the gap analysis section of ntr-kojo-analysis.md
- K7 exclusion statement matches regex `K7.*excluded|K7.*zero|K7.*0.*NTR`

### Error Handling

- If a character directory is missing or ERB files cannot be read: STOP → Report to user with exact path
- If K4 pre-analysis section is not found at `phase-20-27-game-systems.md:395-541`: STOP → Report to user (do not re-derive)
- If helper function definitions (K10 `MSG_NTR_SEX_10`, K5 `NTR性交拒否`) cannot be located: Document as "helper definition not found" and note raw grep count may undercount; do NOT skip the character

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK
- Missing Task for Option A → FL FAIL
-->

<!-- DRAFT Creation Checklist (Option A):
When a Task creates a new feature-{ID}.md [DRAFT], it MUST complete ALL of:
1. Create feature-{ID}.md file
2. Register in index-features.md (add row to Active Features table)
3. Update "Next Feature number" in index-features.md
AC for DRAFT creation MUST verify BOTH file existence AND index registration.
-->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-07T00:00 | START | initializer | [REVIEWED] → [WIP] | READY |
<!-- run-phase-1-completed -->
| 2026-03-07T00:01 | PHASE2 | orchestrator | Artifact confirmation | All 3 artifacts confirmed |
<!-- run-phase-2-completed -->
| 2026-03-07T00:05 | PHASE4 | orchestrator | Tasks 1-3 complete | SUCCESS (all 3 tasks) |
<!-- run-phase-4-completed -->
| 2026-03-07T00:10 | PHASE7 | ac-tester | AC verification 7/7 PASS | All ACs [x] |
<!-- run-phase-7-completed -->
| 2026-03-07T00:12 | DEVIATION | feature-reviewer | NEEDS_REVISION | K5 8h footnote contradictory (says "adjusted upward" but 52% < 65% raw) |
<!-- run-phase-8-completed -->
| 2026-03-07T00:15 | PHASE9 | orchestrator | Report & Approval | 7/7 PASS, 1 DEVIATION (D: 修正済み) |
<!-- run-phase-9-completed -->
| 2026-03-07T00:16 | PHASE10 | finalizer | [WIP] → [DONE] | READY_TO_COMMIT |
| 2026-03-07T00:16 | CodeRabbit | Skip (research) | - |
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase2-Review iter1: C5/AC#5/Implementation Contract | 8h/8m/8n labels corrected (hot spring→NTR kojo depth, modding support→MC interaction, NTR content→Netorase)
- [fix] Phase2-Uncertain iter1: AC#3 | threshold increased gte 10→gte 20 (heading + data = 2 per target × 10)
- [fix] Phase2-Review iter1: C10/Philosophy | AC#7 added for universal vs character-specific pattern distinction
- [fix] Phase2-Review iter2: AC#3 | pattern fixed K1[^0-9] etc. to prevent K1 matching K10
- [fix] Phase2-Review iter2: C4/Tech Design | VO candidates aligned with architecture doc (empirical vs architecture-designed distinction)
- [fix] Phase2-Uncertain iter2: AC#5 | threshold increased gte 3→gte 10 (per-character coverage)
- [fix] Phase2-Review iter3: Goal | added universal vs character-specific distinction text to Goal section (Goal Item 5)
- [fix] Phase2-Uncertain iter3: AC#4 | threshold increased gte 2→gte 4 (section headings + candidate descriptions)

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F827](feature-827.md) - Phase 23 Planning
- [Successor: F848](feature-848.md) - Post-Phase Review Phase 23
- [Successor: F849](feature-849.md) - Phase 24 Planning
