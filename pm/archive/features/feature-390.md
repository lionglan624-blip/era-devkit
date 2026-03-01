# Feature 390: Phase 6 Planning

## Status: [DONE]

## Type: research

## Created: 2026-01-07

---

## Summary

**Feature を立てる Feature**: Phase 6 の sub-features を作成する計画 Feature。

full-csharp-architecture.md を分析し、Phase 6 実装に必要な Feature 群を作成する。

**Context**: Dedicated planning feature per single-responsibility principle. Separated from F389 review to maintain clear phase transition boundaries.

**Output**: 新規 Feature ファイル (feature-{ID}.md) の作成が主成果物。

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity**: Each phase completion triggers next phase planning. This ensures:
- Continuous development pipeline
- Clear phase boundaries
- Documented transition points

### Problem (Current Issue)

Phase 5 completion requires Phase 6 planning to maintain momentum:
- Phase 6 scope must be defined from architecture.md
- Sub-features must follow granularity rules (8-15 ACs for engine type per feature-template.md)
- Dependencies must be documented

### Goal (What to Achieve)

1. **Analyze Phase 6** requirements from full-csharp-architecture.md
2. **Create sub-features** for Phase 6 Tasks (6 tasks mapping to 9 source files: ABL.ERB, ABL_UP_DATA.ERB, ABLUP.ERB, TRACHECK*.ERB [TRACHECK.ERB, TRACHECK_ABLUP.ERB, TRACHECK_EQUIP.ERB, TRACHECK_ORGASM.ERB], BEFORETRAIN.ERB, AFTERTRA.ERB)
3. **Update index-features.md** with new features
4. **Document dependencies** between Phase 6 features

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Feature grouping strategy documented | file | Grep(feature-390.md) | contains | "Phase 6 Feature Mapping:" | [x] |
| 2 | Sub-features created (>= 2 with F390 as Predecessor) | file | Grep("Predecessor.*F390", Game/agents/) | gte | 2 | [x] |
| 3 | Each created sub-feature has >= 8 ACs | file | Grep(Game/agents/) | gte | 8 | [x] |
| 4 | index-features.md updated | file | Grep(index-features.md) | contains | "Phase 6" | [x] |
| 5 | F390 as Predecessor in created features | file | Grep | contains | "| F390 |" | [x] |

### AC Details

**AC#1**: Grep for "Phase 6 Feature Mapping:" in feature-390.md Execution Log
**AC#2**: Grep for "Predecessor.*F390" in Game/agents/feature-*.md, count >= 2
**AC#3**: For each created feature file with "Predecessor.*F390", count lines matching "^\| *[0-9]+ *\|" in AC table section, verify count >= 8

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Extract Phase 6 Tasks list, determine grouping strategy, document as "Phase 6 Feature Mapping:" in Execution Log | [x] |
| 2 | 2 | Create Phase 6 sub-features per architecture Tasks list | [x] |
| 3 | 3 | Ensure each created feature has at least 8 ACs per feature-template.md | [x] |
| 4 | 4 | Update index-features.md with Phase 6 features | [x] |
| 5 | 5 | Add F390 as Predecessor to each created feature | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F389 | Post-Phase Review must pass first |
| Successor | Phase 6 features (F392+) | Created by this feature |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 6 definition
- [feature-389.md](feature-389.md) - Phase 5 Post-Phase Review
- [feature-template.md](reference/feature-template.md) - Granularity guidelines

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-07 | create | opus | Created as Phase 6 planning feature per SRP | PROPOSED |
| 2026-01-07 13:30 | START | implementer | Task 1-5 | - |
| 2026-01-07 20:06 | END | implementer | Task 1-5 | SUCCESS |
| 2026-01-07 20:08 | END | ac-tester | AC 1-5 | PASS:5/5 |
| 2026-01-07 20:09 | END | feature-reviewer | post | READY |
| 2026-01-07 20:10 | END | feature-reviewer | doc-check | READY (status→[DONE] handled by finalizer) |
| 2026-01-07 22:00 | revision | implementer | Revise F392-F394 with actual measurements, split TRACHECK.ERB into F395-F396 | SUCCESS |

### Phase 6 Feature Mapping:

**Source Analysis**: 6 Tasks → 9 files (~3,700 lines total)

| File | Lines | Purpose | Complexity |
|------|:-----:|---------|:----------:|
| ABL.ERB | ~800 | 能力システムコア | HIGH |
| ABL_UP_DATA.ERB | ~500 | 成長データ定義 | HIGH |
| ABLUP.ERB | ~400 | 能力上昇計算 | Medium |
| TRACHECK.ERB | ~600 | 訓練検証メイン | HIGH |
| TRACHECK_ABLUP.ERB | ~300 | 能力成長チェック | Medium |
| TRACHECK_EQUIP.ERB | ~200 | 装備チェック | Medium |
| TRACHECK_ORGASM.ERB | ~200 | 絶頂チェック | Medium |
| BEFORETRAIN.ERB | ~400 | 訓練前セットアップ | Medium |
| AFTERTRA.ERB | ~300 | 訓練後クリーンアップ | Medium |

**Grouping Strategy**: Functional cohesion + volume limits (per feature-template: engine type ~300 lines, 8-15 ACs)

| Feature | Tasks | Files | Lines | Rationale |
|---------|:-----:|:-----:|:-----:|-----------|
| F392 | 1-3 | ABL.ERB, ABL_UP_DATA.ERB, ABLUP.ERB | ~1,700 | Ability system core: tightly coupled ability/growth logic |
| F393 | 4 | TRACHECK*.ERB (4 files) | ~1,300 | Training validation: cohesive validation subsystem |
| F394 | 5-6 | BEFORETRAIN.ERB, AFTERTRA.ERB | ~700 | Training lifecycle: pre/post processing pair |

**Rationale**:
- F392: Ability system forms single cohesive unit (GetAbility/HasTalent/ApplyGrowth interface)
- F393: TRACHECK*.ERB files share validation responsibility, splitting would break cohesion
- F394: BEFORE/AFTER are logical pair for training lifecycle management

**Volume Check**: All exceed ~300 line guideline but maintain functional cohesion per YAGNI principle (avoid premature splitting)

---

### Revision 2026-01-07 22:00 - Actual Measurements

**Discovery**: Architecture estimates were significantly wrong. Actual line counts:

| File | Estimated | Actual | Notes |
|------|-----------|--------|-------|
| ABL.ERB | ~800 | 123 | Much smaller |
| ABL_UP_DATA.ERB | ~500 | 1983 | 4x larger |
| ABLUP.ERB | ~400 | 205 | ALL COMMENTS - skip |
| TRACHECK.ERB | ~600 | 1239 | Contains 3 distinct domains |
| TRACHECK_ABLUP.ERB | ~300 | 103 | Smaller |
| TRACHECK_EQUIP.ERB | ~200 | 225 | Close |
| TRACHECK_ORGASM.ERB | ~200 | 515 | 2.5x larger |
| BEFORETRAIN.ERB | ~400 | 255 | Smaller |
| AFTERTRA.ERB | ~300 | 138 | Much smaller |

**TRACHECK.ERB Domain Analysis** (1,239 lines):
- 刻印システム (566-853): ~290 lines → F395 Mark System
- 処女管理 (79-220): ~140 lines → F396 Character State Tracking
- 経験成長 (854-950): ~100 lines → F396 Character State Tracking
- 好感度計算 (951-1151): ~200 lines → F393 Training Validation Core
- 訓練修正値 (4-70): ~70 lines → F393 Training Validation Core
- その他: SOURCE_SEX_CHECK, PLAYER_SKILL_CHECK, PAIN_CHECK* → F396

**Revised Feature Mapping**:

| Feature | Scope | Lines | Domain |
|---------|-------|:-----:|--------|
| F392 | ABL.ERB, ABL_UP_DATA.ERB | ~2,106 | Ability System (skip ABLUP.ERB - all comments) |
| F393 | TRACHECK_*.ERB (4 files) + basic checks + FavorCalculator | ~682 | Training Validation Core |
| F394 | BEFORETRAIN.ERB, AFTERTRA.ERB | ~393 | Training Lifecycle |
| F395 | 刻印システム (TRACHECK.ERB lines 566-853) | ~290 | Training Result Domain (Mark System) |
| F396 | 処女管理 + 経験成長 (TRACHECK.ERB other sections) | ~240 | Character State Domain |

**Action Taken**:
- Updated F392 Background: Actual lines, noted ABLUP.ERB skip
- Revised F393 scope: Training Validation Core (TRACHECK_*.ERB + basic checks + FavorCalculator)
- Updated F394 Background: Actual lines (BEFORETRAIN 255, AFTERTRA 138)
- Created F395: Mark System (刻印システム) with 12 ACs
- Created F396: Character State Tracking (処女管理 + 経験成長) with 12 ACs
- Updated index-features.md: Added F395, F396; Next Feature → 397
