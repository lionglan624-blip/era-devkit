# Feature 450: Phase 12 Planning

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

## Created: 2026-01-11

---

## Summary

**Feature を立てる Feature**: Phase 12 Planning

Create sub-features for Phase 12 COM Implementation:
- Implementation sub-features (COM migration by category: Com0xx, Com1xx, etc.)
- Technical debt resolution sub-features (TrainingProcessor, callbacks, TODO cleanup)
- Post-Phase Review feature (type: infra)
- Phase 13 Planning feature (type: research)

**Output**: New Feature files as primary deliverables.

**CRITICAL**: Phase 12 is the largest phase (150+ COMF files, 50,000+ lines). Requires careful decomposition into manageable features following granularity rules (8-15 ACs for engine type).

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity**: Each phase completion triggers next phase planning. This ensures:
- Continuous development pipeline
- Clear phase boundaries
- Documented transition points

### Problem (Current Issue)

Phase 11 completion requires Phase 12 planning to maintain momentum:
- Phase 12 scope must be defined from architecture.md
- Sub-features must follow granularity rules (8-15 ACs for engine type per feature-template.md)
- COM Implementation is the largest migration (150+ files)
- Dependencies must be documented
- **Transition features must be created** (Post-Phase Review + Next Phase Planning)

### Goal (What to Achieve)

1. **Analyze Phase 12** requirements from full-csharp-architecture.md
2. **Decompose COM Implementation** into manageable sub-features by category
3. **Create implementation sub-features** from Phase 12 tasks
4. **Create transition features** (Post-Phase Review + Phase 13 Planning)
5. **Update index-features.md** with Phase 12 features
6. **Verify sub-feature quality** (Philosophy inheritance, test PASS AC)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 12 analysis documented | file | Grep | contains | "Phase 12 Feature Mapping:" | [x] |
| 2 | COM categorization complete | file | Grep | contains | "Phase 12 COM Analysis:" | [x] |
| 3 | At least one implementation sub-feature created | file | Grep | contains | "COM.*Migration" | [x] |
| 4 | Post-Phase Review in index | file | Grep | contains | "Post-Phase Review.*Phase 12" | [x] |
| 5 | Phase 13 Planning in index | file | Grep | contains | "Phase 13 Planning" | [x] |
| 6 | index-features.md updated | file | Grep | contains | "\\| 45[2-9] \\||\\| 46[0-9] \\|" | [x] |
| 7 | Implementation sub-feature has Philosophy | file | Grep | contains | "Phase 12: COM Implementation" | [x] |
| 8 | Minimum sub-feature coverage | file | Grep | count_gte | 8 | [x] |

### AC Details

**AC#1**: Phase 12 analysis documented in feature-450.md Execution Log
- Test: Grep pattern="Phase 12 Feature Mapping:" in feature-450.md
- Verifies mapping from architecture.md tasks to sub-features

**AC#2**: COM categorization documented
- Test: Grep pattern="Phase 12 COM Analysis:" in feature-450.md Execution Log
- Shows how 150+ COMF files are grouped into features
- Note: Uses unique marker "Phase 12 COM Analysis:" (not "COM Category Breakdown:" which exists in Scope Reference)

**AC#3**: At least one implementation sub-feature created
- Test: Grep pattern="COM.*Migration" in index-features.md
- Verifies implementation sub-features are registered

**AC#4**: Post-Phase Review in index
- Test: Grep pattern="Post-Phase Review.*Phase 12" in index-features.md
- Type: infra, follows F449 pattern

**AC#5**: Phase 13 Planning in index
- Test: Grep pattern="Phase 13 Planning" in index-features.md
- Type: research, follows F450 pattern

**AC#6**: index-features.md updated with Phase 12 features
- Test: Grep pattern="\\| 45[2-9] \\||\\| 46[0-9] \\|" in index-features.md
- Verifies at least one sub-feature ID in 452-469 range registered (F451 already taken)
- Pattern covers expected 10-12 sub-features (8 COM categories + 2 transition + tech debt)

**AC#7**: Implementation sub-feature has Philosophy
- Test: Grep pattern="Phase 12: COM Implementation" in created sub-feature files
- Per architecture.md Sub-Feature Requirements

**AC#8**: Minimum sub-feature coverage
- Test: Grep pattern="\\| 45[2-9] \\||\\| 46[0-9] \\|" in index-features.md, count >= 8
- Verifies at least 8 Phase 12 sub-features created (8 COM categories minimum)
- Note: Actual count may exceed 8 with technical debt consolidation features

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze Phase 12 requirements and document "Phase 12 Feature Mapping:" | [x] |
| 2 | 2 | Document COM categorization as "Phase 12 COM Analysis:" in Execution Log | [x] |
| 3 | 3 | Create implementation sub-features (COM migration by category) | [x] |
| 4 | 4 | Create Phase 12 Post-Phase Review feature (type: infra) | [x] |
| 5 | 5 | Create Phase 13 Planning feature (type: research) | [x] |
| 6 | 6 | Update index-features.md with all Phase 12 features | [x] |
| 7 | 7 | Verify Philosophy inheritance in implementation sub-features | [x] |
| 8 | 8 | Verify minimum sub-feature coverage (8+ features created) | [x] |

<!-- AC:Task 1:1 Rule: 8 ACs = 8 Tasks -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Analysis Method

1. **Read architecture.md Phase 12 section**:
   - `Game/agents/designs/full-csharp-architecture.md`
   - Identify Phase 12 tasks and scope
   - Note COM categorization (Com0xx, Com1xx, etc.)
   - Review Phase 4 design requirements (Strongly Typed IDs, DI)
   - Review Technical Debt Deferred to Phase 12:
     * TrainingProcessor Integration (17 lines dead code)
     * Well-Known Index Additions (CharacterFlagIndex.Favor, TCVarIndex.Actor)
     * Callback Implementations (JUEL/TEQUIP accessors)
     * TODO Comment Cleanup (14 outdated comments)
     * GlobalStatic accessor migration (3 TODOs)
     * System Commands engine integration
   - Review F406 Deferred Items:
     * EQUIP_COM42-189 (18 equipment handlers)
     * Video recording / TSTR accessor migration

2. **Decompose into sub-features**:
   - Apply granularity rules (8-15 ACs for engine type)
   - Group COM implementations by category
   - Assign explicit Feature IDs (F452+ for COM categories)
   - Consider F406 deferred items (EquipmentProcessor integration)
   - Consider Phase 7 deferred items (TrainingProcessor integration, callbacks, TODO cleanup)
   - Identify dependencies between sub-features
   - Document decomposition rationale in Execution Log

3. **Create feature files**:
   - Follow feature-template.md structure
   - Include Philosophy: "Phase 12: COM Implementation" per architecture.md
   - Include test PASS verification AC (per Sub-Feature Requirements)
   - Reference Phase 4 design patterns (Strongly Typed IDs, DI, SRP)

4. **Create transition features**:
   - Post-Phase Review (type: infra)
   - Phase 13 Planning (type: research)

5. **Update index-features.md**:
   - Add all Phase 12 features atomically
   - Maintain dependency order

### Execution Phases

| Phase | Agent | Input | Output |
|-------|-------|-------|--------|
| 1 | spec-writer | architecture.md Phase 12, feature-template.md | Phase 12 sub-feature files |
| 2 | spec-writer | Sub-feature files | index-features.md update |
| 3 | ac-tester | Sub-feature files | AC#7 PASS |

**Execution Order**:
1. Analyze Phase 12 scope and create COM categorization
2. Create all sub-feature files with Sub-Feature Requirements
3. Update index-features.md with all Phase 12 features
4. Verify AC#7 passes for implementation sub-features
5. Mark Tasks 1-7 complete

### Sub-Feature Requirements

Per architecture.md Phase 12, implementation sub-features MUST include:

| # | Requirement | Applies To | Verification |
|:-:|-------------|------------|--------------|
| 1 | **Philosophy inheritance** - "Phase 12: COM Implementation" in Philosophy section | All implementation features | AC#7 Grep |
| 2 | **AC: Test verification** - AC verifying tests pass after COM migration | Each implementation feature | Manual inspection |
| 3 | **Phase 4 patterns** - Reference to Strongly Typed IDs, DI, SRP in Background | All implementation features | Manual inspection |

---

## Phase 12 Scope Reference

**Snapshot from architecture.md (2026-01-11)**. See [full-csharp-architecture.md](designs/full-csharp-architecture.md) for authoritative version.

**Phase 12: COM Implementation**

**Goal**: COM コマンド実装の移行（150+ COMF ファイル）

**Background**: COMF*.ERB はゲームの訓練コマンド（COM_0〜COM_999）の実装本体。Phase 9 のコマンド基盤上で動作する。

**CRITICAL**: これがゲームロジックの最大部分。150+ ファイル、推定 50,000+ 行。

**COM Category Breakdown** (from architecture.md):

| Range | Files | Category |
|-------|:-----:|----------|
| COMF0-99 | ~20 | 基本行動（会話、移動等） |
| COMF100-199 | ~30 | 愛撫系（キス、タッチ等） |
| COMF200-299 | ~15 | 特殊行動 |
| COMF300-399 | ~20 | 会話系（会話拡張含む） |
| COMF400-499 | ~40 | 訓練系（メイン訓練） |
| COMF500-599 | ~10 | 追加訓練 |
| COMF600-699 | ~10 | 拡張行動 |
| COMF888, 999 | 2 | 特殊処理 |
| **Total** | **~150** | |

**Design Requirements** (from Phase 4):
- Strongly Typed IDs: `ComId`, `ExpType`, `PalamType`
- DI interfaces: `ICom`, `IComContext`, `IComRegistry`
- SRP: 1 COM = 1 class = 1 file
- Category subdirectories: `Com0xx/`, `Com1xx/`, etc.

**Equipment Handler Integration** (from F406 Deferred):
- EquipmentProcessor.ProcessEquipment calls EQUIP_COM*
- Implementation in COMF*.ERB must be migrated
- See F406 Links section for deferred item details

**Tasks** (from architecture.md):
1. COMF ファイル分析・分類
2. COM 実装パターン抽出
3. C# クラス設計（継承/コンポジション）
4. 150+ COM の逐次移行
5. 各 COM の単体テスト
5.5. TrainingProcessor integration (17 lines dead code)
5.6. Well-Known Index Additions (CharacterFlagIndex.Favor, TCVarIndex.Actor)
5.7. JUEL/TEQUIP callback implementation (F405 pattern)
5.8. TODO comment cleanup (14 outdated comments)
5.9. GlobalStatic accessor migration (3 TODOs in GameInitialization.cs)
5.10. System Commands engine integration (CharacterManager/StyleManager/GameState)
5.11. Additional system commands (SETCOLORBYNAME, BEGIN, DRAWLINE)
6. Create Phase 12 Post-Phase Review feature (type: infra)
7. Create Phase 13 Planning feature (type: research)

**Success Criteria**:
- [ ] 150+ COM implementations migrated to C#
- [ ] All COM unit tests pass
- [ ] Phase 4 design patterns applied (Strongly Typed IDs, DI, SRP)
- [ ] Equipment handlers integrated (F406 deferred items resolved)

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F449 | Phase 11 Post-Phase Review must pass first |
| Successor | F452-F459 | Phase 12 COM implementation features (created by this feature) |
| Successor | F460-F461 | Phase 12 technical debt consolidation features (created by this feature) |
| Successor | F462 | Post-Phase Review Phase 12 (created by this feature) |
| Successor | F463 | Phase 13 Planning (created by this feature) |
| Reference | F406 | Equipment handler deferred items |

---

## Links

- [feature-447.md](feature-447.md) - Phase 11 Planning (precedent feature)
- [feature-449.md](feature-449.md) - Phase 11 Post-Phase Review (dependency)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 12 definition
- [feature-template.md](reference/feature-template.md) - Granularity guidelines
- [feature-406.md](feature-406.md) - Equipment handler (deferred items)

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-11 FL pre**: AC count (8) exceeds research type guideline (3-5). Justified: 2 documentation + 1 impl sub-feature + 2 transition features + 1 index update + 1 quality verification + 1 coverage. Follows F447 precedent pattern with coverage AC added per maintainability review.
- **2026-01-11 FL iter1**: [resolved] Phase2-Validate - Philosophy section: F447 (precedent) has identical Philosophy text and passed FL review without SSOT claim. Validator confirmed precedent is valid; no SSOT claim required for research-type Planning Features.
- **2026-01-11 FL iter2**: [applied] Phase6-PlanningValidation - Updated Analysis Method and Scope Reference to include complete architecture.md Task mapping (Tasks 5.5-5.11: deferred technical debt items). AC structure retained as-is; Feature IDs are outputs of analysis, not inputs at PROPOSED stage.
- **2026-01-11 FL iter4**: [applied] Phase3-Maintainability - (1) AC#6 pattern expanded to 452-469 range (F451 already taken). (2) Added AC#8 minimum coverage verification (count_gte 8).
- **2026-01-11 FL iter5**: [applied] Phase2-Validate - Fixed AC#6 regex: `\\|\\|` → `\\||` (regex OR instead of literal double-pipe).
- **2026-01-11 FL iter5**: [applied] Phase2-Validate - AC#6/AC#8 pattern asymmetry fixed to symmetric pattern `\\| 45[2-9] \\||\\| 46[0-9] \\|`.
- **2026-01-11 FL iter6**: [skipped] Phase2-Validate - AC#3 pattern `COM.*Migration` kept as-is. Pattern is sufficiently robust; naming convention documented in Implementation Contract.
- **2026-01-11 FL iter6**: [skipped] Phase4-ACValidation - AC#8 `count_gte` matcher kept as-is. Project-established convention with 10+ DONE features track record.
- **2026-01-11 FL post iter1**: [applied] Phase2-Validate - F462 AC#6 changed from Bash "/audit" to manual /audit (slash command is not Bash command).
- **2026-01-11 FL post iter1**: [applied] Phase2-Validate - Dependencies TBD updated to actual created feature IDs (F452-F463).
- **2026-01-11 FL post iter2**: [applied] Phase2-Validate - F452 Task comment improved to standard format (13 ACs = 6 Tasks with grouping explanation).
- **2026-01-11 FL post iter2**: [applied] Phase2-Validate - F452 equipment handler scope updated to include EQUIP_COM183-189 (18 total handlers).
- **2026-01-11 FL post iter2**: [applied] Phase2-Validate - F450 Dependencies split F452-F461 into F452-F459 (COM impl) + F460-F461 (tech debt) for clarity.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 | create | spec-writer | Created from F447 Phase 11 Planning | PROPOSED |
| 2026-01-11 19:55 | START | implementer | Task 1-8 | - |

### Phase 12 Feature Mapping:

**Source**: architecture.md Phase 12 section (lines 2832-3099)

**Tasks from architecture.md**:
1. COMF file analysis and classification → F452 (analysis + Com0xx/Com1xx)
2. COM implementation pattern extraction → F452 (design)
3. C# class design (inheritance/composition) → F452 (architecture)
4. 150+ COM migration → F453-F459 (by category)
5. COM unit tests → Integrated into F452-F459 (each feature has test AC)
5.5. TrainingProcessor integration (17 lines) → F460 (technical debt consolidation)
5.6. Well-Known Index additions (CharacterFlagIndex.Favor, TCVarIndex.Actor) → F460
5.7. JUEL/TEQUIP callback implementation (F405 pattern) → F460
5.8. TODO comment cleanup (14 outdated comments) → F460
5.9. GlobalStatic accessor migration (3 TODOs in GameInitialization.cs) → F461 (system integration)
5.10. System Commands engine integration (CharacterManager/StyleManager/GameState) → F461
5.11. Additional system commands (SETCOLORBYNAME, BEGIN, DRAWLINE) → F461
6. Create Phase 12 Post-Phase Review feature → F462 (type: infra)
7. Create Phase 13 Planning feature → F463 (type: research)

**Sub-Feature Decomposition Rationale**:
- COM categories grouped to meet 8-15 AC granularity (engine type)
- F452 combines analysis + Com0xx/Com1xx (~50 files, 12 ACs: 1 design + 10 impl + 1 test)
- F453-F459 handle remaining categories (Com2xx through Special)
- F460 consolidates Phase 7 deferred technical debt (4 items)
- F461 consolidates Phase 9 deferred system integration (3 items)
- F462/F463 are mandatory transition features per Phase Progression Rules
- Equipment handler migration integrated into respective COM category features (F406 deferred items)

### Phase 12 COM Analysis:

**Total COMF Files**: 104 files identified via Glob (actual count may vary from architecture.md estimate)

**Category Distribution** (from architecture.md):
- Com0xx (0-99): ~20 files - Basic actions (conversation, movement)
- Com1xx (100-199): ~30 files - Caressing (kiss, touch)
- Com2xx (200-299): ~15 files - Special actions
- Com3xx (300-399): ~20 files - Conversation systems
- Com4xx (400-499): ~40 files - Training commands (main training)
- Com5xx (500-599): ~10 files - Additional training
- Com6xx (600-699): ~10 files - Extended actions
- Com888, Com999: 2 files - Special processing

**Equipment Handler Distribution** (from F406 deferred):
- Com0xx range: EQUIP_COM42-48 (7 equipment handlers)
- Com1xx range: EQUIP_COM104-106, EQUIP_COM146-148 (6 equipment handlers)
- Com1xx range: EQUIP_COM183-189 (5 equipment handlers, includes video recording)
- Total: 18 equipment handlers integrated into COM migration

**Grouping Strategy**:
- F452: Analysis + Com0xx + Com1xx (~50 files, includes 13 equipment handlers)
- F453: Com2xx (15 files)
- F454: Com3xx (20 files)
- F455: Com4xx Part 1 (20 files, COM_400-419)
- F456: Com4xx Part 2 (20 files, COM_420-499)
- F457: Com5xx (10 files)
- F458: Com6xx (10 files)
- F459: Special COMs (Com888, Com999)

**Feature Count**: 12 sub-features total
- 8 COM implementation features (F452-F459)
- 2 technical debt consolidation features (F460-F461)
- 2 transition features (F462-F463)

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 20:01 | END | implementer | Task 1-8 | SUCCESS |
| 2026-01-11 20:30 | DEVIATION | - | quality review | implementer skipped Skill(feature-quality) |
| 2026-01-11 20:35 | - | - | delete F452-F463 | Recreate with feature-builder |
| 2026-01-11 20:35 | - | - | create feature-builder.md | New specialized agent |
| 2026-01-11 20:45 | START | feature-builder | Create F452-F463 | - |
| 2026-01-11 20:45 | SKILL | feature-builder | Skill(feature-quality) | LOADED |
| 2026-01-11 20:46 | SKILL | feature-builder | ENGINE.md | LOADED |
| 2026-01-11 20:46 | SKILL | feature-builder | RESEARCH.md | LOADED |
| 2026-01-11 20:46 | SKILL | feature-builder | INFRA.md | LOADED |
| 2026-01-11 20:50 | CREATE | feature-builder | F452 COM Analysis and Basic/Caressing Actions | PROPOSED |
| 2026-01-11 20:50 | CREATE | feature-builder | F453 Special Actions Migration (Com2xx) | PROPOSED |
| 2026-01-11 20:50 | CREATE | feature-builder | F454 Conversation Systems Migration (Com3xx) | PROPOSED |
| 2026-01-11 20:50 | CREATE | feature-builder | F455 Main Training Commands Part 1 (Com4xx 400-419) | PROPOSED |
| 2026-01-11 20:50 | CREATE | feature-builder | F456 Main Training Commands Part 2 (Com4xx 420-499) | PROPOSED |
| 2026-01-11 20:51 | CREATE | feature-builder | F457 Additional Training Migration (Com5xx) | PROPOSED |
| 2026-01-11 20:51 | CREATE | feature-builder | F458 Extended Actions Migration (Com6xx) | PROPOSED |
| 2026-01-11 20:51 | CREATE | feature-builder | F459 Special Processing Migration (Com888, Com999) | PROPOSED |
| 2026-01-11 20:51 | CREATE | feature-builder | F460 Phase 7 Technical Debt Resolution | PROPOSED |
| 2026-01-11 20:51 | CREATE | feature-builder | F461 Phase 9 System Integration | PROPOSED |
| 2026-01-11 20:51 | CREATE | feature-builder | F462 Phase 12 Post-Phase Review | PROPOSED |
| 2026-01-11 20:51 | CREATE | feature-builder | F463 Phase 13 Planning | PROPOSED |
| 2026-01-11 20:52 | UPDATE | feature-builder | index-features.md | Registered F452-F463 |
| 2026-01-11 20:52 | END | feature-builder | Create F452-F463 | SUCCESS: 12 features created |
