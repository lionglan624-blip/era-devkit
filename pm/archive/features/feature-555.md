# Feature 555: Phase 19 Planning

## Status: [DONE]

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

## Created: 2026-01-18

---

## Summary

**Feature を立てる Feature**: Phase 19 Planning

Create sub-features for Phase 19 Kojo Conversion (ERB→YAML migration):
- Kojo conversion tooling improvements (ErbParser PRINTDATA support, batch converter)
- Conversion workflow sub-features (per-character conversion batches)
- Quality validation features (equivalence testing, schema validation)
- F646: Post-Phase Review Phase 19 (type: infra)
- F647: Phase 20 Planning (type: research)

**Output**: New Feature files as primary deliverables.

**CRITICAL**: Phase 19 converts all existing ERB kojo files (117 files across 11 directories: 10 characters + 1 generic) to YAML format using automated conversion tools with manual review workflow. Scope limited to migration only (no new content creation). Follows F344/F352 pilot results and tooling precedents.

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity** - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. Follows F471/F486/F503/F516/F541 pattern.

### Problem (Current Issue)

Phase 18 completion requires Phase 19 planning to maintain momentum:
- Phase 19 scope must be defined from full-csharp-architecture.md
- Sub-features must follow granularity rules (8-15 ACs for erb type, 3-5 for research type per feature-template.md)
- Kojo Conversion is a migration phase requiring tooling improvements, batch processing, and quality validation
- Dependencies must be documented (Tooling before conversion, validation after conversion)
- **Transition features must be created** (Post-Phase Review + Next Phase Planning)

### Goal (What to Achieve)

1. **Analyze Phase 19** requirements from full-csharp-architecture.md
2. **Decompose Kojo Conversion** into manageable sub-features following conversion workflow order
3. **Create tooling sub-features** for ErbParser improvements and batch conversion
4. **Create conversion sub-features** per character or logical grouping
5. **Create validation sub-features** for equivalence testing and schema validation
6. **Create transition features** (Post-Phase Review + Phase 20 Planning)
7. **Update index-features.md** with Phase 19 features
8. **Verify sub-feature quality** (Philosophy inheritance, conversion workflow adherence, quality gates)

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 19 analysis documented | file | Grep(Game/agents/feature-555.md) | contains | "Phase 19 Feature Mapping:" | [x] |
| 2 | Conversion strategy defined | file | Grep(Game/agents/feature-555.md) | contains | "Phase 19 Conversion Strategy:" | [x] |
| 3 | Sub-features created | file | Grep(Game/agents/index-features.md) | contains | "Phase 19" | [x] |
| 4 | Coverage verified | manual | Manual count verification | equals | "15 sub-features F633-F647 created" | [x] |
| 5 | Transition features created | file | Grep(Game/agents/index-features.md) | contains | "Post-Phase Review Phase 19" | [x] |
| 6 | Index updated | file | Grep(Game/agents/index-features.md) | contains | "Phase 19" | [x] |
| 7 | Quality verified | file | Grep(Game/agents/feature-555.md) | contains | "Philosophy inheritance" | [x] |

### AC Details

**AC#1**: Phase 19 analysis documented
- Method: `Grep("Phase 19 Feature Mapping:", "Game/agents/feature-555.md")`
- Expected: Analysis table mapping Phase 19 tasks to features

**AC#2**: Conversion strategy defined
- Method: `Grep("Phase 19 Conversion Strategy:", "Game/agents/feature-555.md")`
- Expected: Conversion workflow (tooling → batch conversion → validation) documented

**AC#3**: Sub-features created in index
- Method: `Grep("Phase 19", "Game/agents/index-features.md")`
- Expected: Multiple Phase 19 feature entries in index

**AC#4**: Coverage verified
- Method: Manual count verification of sub-features F633-F647
- Expected: 15 sub-features created (tooling + conversion batches + validation + transitions)

**AC#5**: Transition features created
- Method: `Grep("Post-Phase Review Phase 19", "Game/agents/index-features.md")`
- Expected: Post-Phase Review and Phase 20 Planning features created

**AC#6**: Index updated
- Method: `Grep("Phase 19", "Game/agents/index-features.md")`
- Expected: index-features.md contains all Phase 19 feature entries

**AC#7**: Quality verified
- Method: `Grep("Philosophy inheritance", "Game/agents/feature-555.md")`
- Expected: Sub-feature quality verification documented

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Read Phase 19 section from full-csharp-architecture.md and create feature mapping | [x] |
| 2 | 2 | Analyze conversion workflow (tooling → conversion → validation) and create strategy | [x] |
| 3 | 3 | Create all 15 Phase 19 sub-features (F633-F647: tooling + conversion + validation + transitions) | [x] |
| 4 | 4 | Verify sub-feature coverage and count (15 features F633-F647 created) | [x] |
| 5 | 5 | Create transition features (Post-Phase Review Phase 19, Phase 20 Planning) | [x] |
| 6 | 6 | Update index-features.md with new Phase 19 features | [x] |
| 7 | 7 | Verify sub-feature quality and Philosophy inheritance | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Analysis Method

1. Read Phase 19 section from full-csharp-architecture.md:
   - Scope Policy (117 files, no empty stubs)
   - Pilot Results (F351 limitations, PRINTDATA parser gap)
   - Conversion strategy (70-80% automatic, 20-30% manual)
   - Integration requirements (IKojoEngine, Strongly Typed IDs)

2. Identify decomposition targets:
   - Tooling improvements → ErbParser PRINTDATA support, batch converter enhancements
   - Conversion batches → Per-character or logical grouping (11 directories: 10 characters + 1 generic)
   - Quality validation → Equivalence testing (ERB vs YAML output), schema validation

3. Group by dependency order:
   - Tooling features before conversion features
   - Conversion features before validation features
   - Validation features before transition features

### Expected Feature ID Allocation

| Phase 19 Component | Feature ID Range | Justification |
|-------------------|------------------|---------------|
| Tooling Improvements | F633-F635 | ErbParser PRINTDATA extension + batch converter + schema validator |
| Conversion Batches | F636-F643 | 8 conversion features (grouped by character or COM series) |
| Quality Validation | F644-F645 | Equivalence testing + schema validation |
| Transition Features | F646-F647 | Post-Phase Review + Phase 20 Planning |

**Total**: ~15 sub-features (F633-F647)

**Decomposition Rationale**: Phase 19 has large scope (117 files) requiring batch processing approach. Conversion features grouped by character to enable parallel execution and incremental progress tracking.

**F354-F357 Relationship**: F633+ features extend upon tooling foundation established by F354-F357 (planned in F352). F354-F357 provide base PRINTDATA parser and conversion framework; F633+ add performance optimization, parallelization, and integration testing capabilities for large-scale Phase 19 conversion.

### Sub-Feature Requirements

Per architecture.md Phase 19 requirements and F344/F352 precedents, sub-features MUST include:

| # | Requirement | Applies To | Verification |
|:-:|-------------|------------|--------------|
| 1 | **Philosophy inheritance** - "Phase 19: Kojo Conversion" in Philosophy section | All sub-features | Grep |
| 2 | **Scope boundary** - Migration only, no new content creation | Conversion features | Summary check |
| 3 | **Quality gate** - Equivalence tests PASS before marking [DONE] | Conversion features | AC with dotnet test |
| 4 | **Schema validation** - YAML files pass schema validation | Conversion features | AC with YAML validator |
| 5 | **Manual review** - Complex conversions flagged for review | Conversion features | AC with manual type |
| 6 | **Zero technical debt** - No TODO/FIXME/HACK in converted YAML | Conversion features | Grep AC |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- [resolved-invalid] Phase1 iter1: Feature ID allocation F556-F570 conflicts (reviewer misunderstood tooling vs planning feature separation)
- [resolved-invalid] Phase1 iter1: AC#4 range incorrect (fix incorrectly referenced F354-F357 tooling features)
- [resolved-invalid] Phase1 iter1: Summary F556+ reference confusing (fix misinterpreted architecture.md tooling vs planning)
- [resolved-invalid] Phase1 iter1: Handoff destinations invalid (fix incorrectly tied to F354-F357 unrelated features)
- [resolved-applied] Phase1 iter2: AC#4 glob pattern mismatch (updated pattern to match F632-F646 range)
- [resolved-invalid] Phase1 iter2: Summary/Architecture mismatch (reviewer confused about phase renumbering history)
- [resolved-applied] Phase1 iter2: Handoff destination validation (added creation tasks for F632, F634, F643)
- [resolved-applied] Phase1 iter2: F354-F357 relationship unclear (added clarification in Implementation Contract)
- [resolved-applied] Phase1 iter3: AC#4 pattern range overflow (fixed pattern to cover F640-F646)
- [resolved-applied] Phase1 iter3: AC:Task 1:1 rule violation (merged Tasks 8-10 into Task#3)
- [resolved-applied] Phase1 iter3: Summary character count error (corrected 9 to 10 characters)
- [resolved-invalid] Phase1 iter3: AC Method column format (short format in table acceptable per testing SKILL.md)
- [resolved-applied] Phase1 iter4: Handoff table format (added Creation Task column per feature-template.md)
- [resolved-applied] Phase1 iter4: Character count inconsistency (updated Implementation Contract to 10 characters)
- [resolved-invalid] Phase1 iter5: Glob pattern invalid syntax (nested character classes in brace expansion work correctly)
- [resolved-applied] Phase1 iter5: AC#4 Expected count mismatch (aligned table with AC Details: 8→15)
- [resolved-invalid] Phase1 iter5: Phase 18 vs 19 reference error (feature correctly says Phase 19, reviewer confused)
- [resolved-invalid] Phase1 iter5: Next Feature number conflict (F632+ allocation correct, index-features.md needs update)
- [resolved-applied] Phase1 iter5: Character count terminology (clarified 11 directories: 10 characters + 1 generic)
- [resolved-invalid] Phase1 iter6: F632-F646 conflicts with F354-F357 (F354-F357 only architecture definitions, not created features)
- [resolved-invalid] Phase1 iter6: AC#4 15 features but F354-F357 created (F354-F357 don't exist as feature files, AC#4 count correct)
- [resolved-invalid] Phase1 iter6: Tooling scope overlap F632-F634 vs F354-F357 (relationship already clarified at line 167)
- [resolved-skipped] Phase2 iter6: Philosophy inheritance architecture.md inconsistency (external document issue - out of scope for feature-555 FL review)
- [resolved-applied] Phase3 iter6: AC Method column format (updated to Grep(path) format per testing SKILL.md)
- [resolved-invalid] Phase3 iter6: AC#4 count_gte matcher verification (count_gte is project-established convention per F450, F463, F471, F541 precedents)
- [resolved-applied] Phase1 iter7: Pending architecture.md issue marked as skipped (external document, out of scope)
- [resolved-applied] Phase1 iter7: Pending count_gte verification resolved as established convention
- [resolved-applied] Phase1 iter8: Feature ID allocation F632 conflict (updated range from F632-F646 to F633-F647)
- [resolved-applied] Phase1 iter8: AC#4 glob pattern F632 conflict (updated pattern to match F633-F647)
- [resolved-applied] Phase1 iter8: Tasks table F632 references (updated to F633-F647 range)
- [resolved-applied] Phase1 iter8: Handoff destinations F632 conflict (updated F632→F633, F634→F635, F643→F644)
- [resolved-applied] Phase1 iter8: Dependencies successor range (updated from F632-F646 to F633-F647)
- [resolved-skipped] Phase5 iter9: Philosophy inheritance mismatch with architecture.md (external document issue, already tracked)
- [resolved-applied] Phase5 iter9: AC#4 glob pattern syntax (split nested brace-character-class into explicit patterns)
- [resolved-skipped] Phase5 iter9: Architecture.md stale phase number (external document, out of scope)
- [resolved-invalid] Phase1 iter10: AC count exceeds research type limit (fix proposal partially incorrect - AC#4/AC#7 serve different purposes)
- [resolved-applied] Phase3 iter10: AC#4 count_gte matcher unsupported (changed to manual verification type)
- [resolved-applied] Phase5 iter10: Task#3 scope clarification (simplified to "all 15 Phase 19 sub-features")
- [resolved-applied] Phase5 iter10: Architecture.md phase discrepancy (added handoff to F646 for phase number correction)
- [resolved-applied] Phase5 iter10: F554 dependency verification (confirmed [DONE] status)
- [resolved-applied] Phase6 iter10: F646 architecture.md correction scope (user confirmed - F646 AC will include explicit architecture.md line 4410 fix)

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

| 課題 | 理由 | 追跡先 | 追跡先ID | Creation Task |
|------|------|--------|----------|-------------|
| Performance benchmarking | Conversion performance measurement deferred from F554 | Feature | F633 | Task#3 |
| Integration test coverage | Engine-level integration tests deferred from F554 | Feature | F644 | Task#3 |
| Batch conversion parallelization | Parallel conversion workflow outside initial scope | Feature | F635 | Task#3 |
| Architecture.md phase number correction | Phase 19 section line 4410 says "Phase 18: Kojo Conversion" (should be "Phase 19") - F646 AC must include explicit architecture.md fix | Feature | F646 | Task#3 |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | feature creation | PENDING |
| 2026-01-27 | init | initializer | status [WIP] | READY |
| 2026-01-27 | impl | orchestrator | Phase 19 analysis | DONE |
| 2026-01-27 | impl | orchestrator | Created F633-F647 DRAFT files | DONE |
| 2026-01-27 | impl | orchestrator | Updated index-features.md | DONE |
| 2026-01-27 | verify | orchestrator | AC verification (7/7 PASS) | PASS |
| 2026-01-27 | DEVIATION | feature-reviewer | post-mode review | NEEDS_REVISION: Status [WIP] but all ACs complete (invalid: [WIP]→[DONE] is Phase 9 finalizer's job) |

---

## Phase 19 Feature Mapping:

### Source Analysis (from full-csharp-architecture.md)

| Component | Architecture.md Reference | Scope |
|-----------|---------------------------|-------|
| PRINTDATA Parser | F354 definition (line 4186, 4204) | Extend ErbParser for PRINTDATA...ENDDATA |
| Batch Converter | F355 definition (line 4205) | Directory processing for ErbToYaml |
| Quality Validator | Line 4287-4291, 4303 | 4x4x4 rule enforcement |
| Migration Tracking | Line 4209, 4232-4250 | migration-status.md |
| KojoComparer | Line 4302, 4328 | Batch verification mode |

### Feature Allocation (F633-F647)

| ID | Name | Type | Scope | Dependency |
|----|------|------|-------|------------|
| F633 | PRINTDATA Parser Extension | engine | ErbParser PRINTDATA...ENDDATA support | - |
| F634 | Batch Conversion Tool | engine | ErbToYaml --batch mode for 117 files | F633 |
| F635 | Conversion Parallelization | engine | Parallel processing for batch conversion | F634 |
| F636 | Meiling Kojo Conversion | erb | 1_美鈴 (11 files) | F634 |
| F637 | Koakuma Kojo Conversion | erb | 2_小悪魔 (11 files) | F634 |
| F638 | Patchouli Kojo Conversion | erb | 3_パチュリー (11 files) | F634 |
| F639 | Sakuya Kojo Conversion | erb | 4_咲夜 (15 files) | F634 |
| F640 | Remilia Kojo Conversion | erb | 5_レミリア (10 files) | F634 |
| F641 | Flandre Kojo Conversion | erb | 6_フラン (10 files) | F634 |
| F642 | Secondary Characters Conversion | erb | 7_子悪魔 + 8_チルノ + 9_大妖精 + 10_魔理沙 (37 files) | F634 |
| F643 | Generic Kojo Conversion | erb | U_汎用 (12 files) | F634 |
| F644 | Equivalence Testing Framework | infra | KojoComparer --all batch verification | F636-F643 |
| F645 | Kojo Quality Validator | engine | 4x4x4 rule validator with --diff, --files modes | F644 |
| F646 | Post-Phase Review Phase 19 | infra | Success Criteria + architecture.md fix (line 4410) | F645 |
| F647 | Phase 20 Planning | research | Equipment & Shop Systems planning | F646 |

### Grouping Rationale

- **F636-F641**: Main characters (individual) - each has 10-15 files with high complexity
- **F642**: Secondary characters (grouped) - smaller file counts, can be batched
- **F643**: Generic - utility/shared kojo (separate scope)

---

## Phase 19 Conversion Strategy:

### Workflow Order

```
1. Tooling (F633-F635)
   └─→ PRINTDATA parser must exist before batch conversion
   └─→ Batch mode must exist before character conversion
   └─→ Parallelization optional for performance

2. Conversion Batches (F636-F643)
   └─→ Can execute in parallel after F634
   └─→ Each creates YAML files + runs KojoComparer

3. Quality Validation (F644-F645)
   └─→ Equivalence testing confirms ERB=YAML
   └─→ Quality validator enforces 4x4x4 rules

4. Transition (F646-F647)
   └─→ Post-Phase Review verifies Success Criteria
   └─→ Phase 20 Planning starts next cycle
```

### Quality Gates

| Gate | Tool | Success Criteria |
|------|------|------------------|
| Equivalence | KojoComparer | 117/117 MATCH |
| Schema | YamlValidator | All YAML pass schema |
| Quality | KojoQualityValidator | 4 branches x 4 variations x 4 lines |
| Debt-free | Grep | No TODO/FIXME/HACK in YAML |

### Philosophy Inheritance

All sub-features MUST include in Philosophy section:
> "Phase 19: Kojo Conversion - Automated migration of 117 ERB kojo files to YAML format"

**Verification**: All 15 sub-features (F633-F647) created with Philosophy section containing Phase 19 reference.

| Feature | Philosophy Verified |
|---------|:-------------------:|
| F633-F645 | Contains "Phase 19: Kojo Conversion" |
| F646 | Contains "Phase 19: Kojo Conversion" |
| F647 | Contains "Pipeline Continuity" (successor planning pattern) |

---

## Dependencies

| Type | ID | Description | Status |
|------|----|-------------|:------:|
| Predecessor | F554 | Post-Phase Review Phase 18 | [DONE] |
| Successor | F633-F647 | Phase 19 implementation features | - |

## Links

- [index-features.md](index-features.md)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 19 section
- [feature-541.md](feature-541.md) - Phase 18 Planning (predecessor pattern)
- [feature-344.md](feature-344.md) - Codebase Analysis (kojo conversion origin)
- [feature-352.md](feature-352.md) - Phase 2 Planning (pilot results reference)
- [feature-template.md](reference/feature-template.md)
