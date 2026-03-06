# Feature 847: Phase 23 NTR Kojo Reference Analysis

## Status: [DRAFT]

## Type: research

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity — each phase produces analysis documents that feed the next phase's design. Research phases generate empirical reference documents that serve as direct input to subsequent design and implementation phases, ensuring each phase is grounded in concrete data rather than assumptions.

### Problem (Current Issue)

Phase 23 requires NTR kojo analysis across all 10 characters to establish empirical foundation for Phase 24 DDD design and content-roadmap 8h/8m/8n gap analysis. K4 (Sakuya) pre-analysis exists in the architecture doc but covers only 1 of 10 characters. Without full-character NTR kojo analysis, Phase 24 Value Object and Aggregate design would lack empirical grounding, and content-roadmap gap analysis for phases 8h/8m/8n would remain incomplete.

### Goal (What to Achieve)

Produce `pm/reference/ntr-kojo-analysis.md` (all-character NTR branch statistics) and `pm/reference/ntr-ddd-input.md` (Phase 24 Value Object/Aggregate design input) by performing NTR kojo analysis across all 10 NTR characters (K1-K10 where files exist). Also perform content-roadmap 8h/8m/8n Gap 分析 update based on full character analysis.

Methodology: NTR branch pattern analysis — read ERB kojo files in `C:\Era\game\ERB\口上\`, count FAV/TALENT/situation condition occurrences per character. K4 (Sakuya) pre-analysis at `docs/architecture/migration/phase-20-27-game-systems.md:395-541` is the baseline; this feature extends to K1-K3, K5-K10.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F827 | [DONE] | Phase 23 Planning — must be [DONE] before F847 proceeds |

## Links

- [Predecessor: F827](feature-827.md) - Phase 23 Planning
