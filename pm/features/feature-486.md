# Feature 486: Phase 15 Planning

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

## Created: 2026-01-13

---

## Summary

**Feature を立てる Feature**: Phase 15 Planning

Create sub-features for Phase 15 Architecture Review (F493-F503, 11 features):
- Implementation sub-features from Phase 15 tasks (Code Review, Test Strategy Design, etc.)
- F502: Post-Phase Review Phase 15 (type: infra)
- F503: Phase 16 Planning (type: research)

**Note**: Exact feature IDs may be adjusted during execution. Above allocation follows Analysis Method step 2.

**Output**: New Feature files as primary deliverables.

**CRITICAL**: Phase 15 is an architecture review phase focused on validating Phase 1-14 implementations against Phase 4 design principles (SRP, DIP, Strongly Typed IDs, Result type, DI registration). This phase also establishes Test Strategy (IRandomProvider design, test layer structure, E2E test methodology) before large-scale parallel implementation phases (Phase 19-21). Requires careful decomposition into manageable review-focused features.

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity** - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points.

### Problem (Current Issue)

Phase 14 completion requires Phase 15 planning to maintain momentum:
- Phase 15 scope must be defined from architecture.md
- Sub-features must follow granularity rules (8-15 ACs for infra/engine type per feature-template.md)
- Architecture Review is a validation phase (Phase 1-14 design compliance) + Test Strategy establishment
- Dependencies must be documented
- **Transition features must be created** (Post-Phase Review + Next Phase Planning)

### Goal (What to Achieve)

1. **Analyze Phase 15** requirements from full-csharp-architecture.md
2. **Decompose Architecture Review** into manageable sub-features
3. **Create implementation sub-features** from Phase 15 tasks
4. **Create transition features** (Post-Phase Review + Phase 16 Planning)
5. **Update index-features.md** with Phase 15 features
6. **Verify sub-feature quality** (Philosophy inheritance, test PASS AC)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 15 analysis documented | file | Grep | contains | "Phase 15 Feature Mapping:" | [x] |
| 2 | Review component categorization complete | file | Grep | contains | "Phase 15 Review Analysis:" | [x] |
| 3 | At least one implementation sub-feature created | file | Grep | contains | "Code Review\\|Test Strategy\\|IRandomProvider" | [x] |
| 4 | Post-Phase Review in index | file | Grep | contains | "Post-Phase Review.*Phase 15" | [x] |
| 5 | Phase 16 Planning in index | file | Grep | contains | "Phase 16 Planning" | [x] |
| 6 | 等価性検証 restriction documented | file | Grep | contains | "等価性検証.*ERB migration" | [x] |
| 7 | Sub-feature Philosophy inheritance verified | file | Grep | contains | "Phase 15: Architecture Review" | [x] |

### AC Details

**AC#1**: Phase 15 analysis documented in feature-486.md Execution Log
- Test: Grep pattern="Phase 15 Feature Mapping:" in feature-486.md
- Verifies mapping from architecture.md tasks to sub-features

**AC#2**: Review component categorization documented
- Test: Grep pattern="Phase 15 Review Analysis:" in feature-486.md Execution Log
- Must contain explicit Feature ID allocation table (e.g., "F487: Code Review Phase 1-4")
- Shows how 9 architecture.md tasks grouped into implementation features
- Documents decomposition rationale (review scope, test strategy components, granularity compliance)

**AC#3**: At least one implementation sub-feature created
- Test: Grep pattern="Code Review|Test Strategy|IRandomProvider" path="Game/agents/index-features.md"
- Verifies at least one Phase 15 review component is registered in index

**AC#4**: Post-Phase Review in index
- Test: Grep pattern="Post-Phase Review.*Phase 15" in index-features.md
- Type: infra, follows F470/F485 pattern

**AC#5**: Phase 16 Planning in index
- Test: Grep pattern="Phase 16 Planning" in index-features.md
- Type: research, follows F471/F486 pattern

**AC#6**: 等価性検証 restriction documented
- Test: Grep pattern="等価性検証.*ERB migration" path="Game/agents/designs/full-csharp-architecture.md"
- Add clarification that 等価性検証 requirement applies only to ERB migration features
- Deferred from F485 (Post-Phase Review determined new doc content creation doesn't belong there)

**AC#7**: Sub-feature Philosophy inheritance verified
- Test: Grep pattern="Phase 15: Architecture Review" in created sub-feature files (Game/agents/feature-493.md, etc.)
- Verifies all created implementation sub-features inherit Philosophy per Sub-Feature Requirements #1

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze Phase 15 requirements and document "Phase 15 Feature Mapping:" | [x] |
| 2 | 2 | Document review component categorization as "Phase 15 Review Analysis:" in Execution Log | [x] |
| 3 | 3 | Create implementation sub-features per Analysis Method step 2 (8-15 ACs per feature, grouped by review scope, with 負債解消 tasks and 負債ゼロ ACs per Sub-Feature Requirements) | [x] |
| 4 | 4 | Create Phase 15 Post-Phase Review feature (type: infra) | [x] |
| 5 | 5 | Create Phase 16 Planning feature (type: research) | [x] |
| 6 | 6 | Document 等価性検証 restriction in architecture.md (deferred from F485) | [x] |
| 7 | 7 | Verify Philosophy inheritance in created sub-features (Grep "Phase 15: Architecture Review") | [x] |

<!-- AC:Task 1:1 Rule: 7 ACs = 7 Tasks -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Analysis Method

1. **Read architecture.md Phase 15 section**:
   - `Game/agents/designs/full-csharp-architecture.md`
   - Identify Phase 15 tasks and scope (9 top-level tasks; Task 5 has 10 sub-tasks for Test Strategy Design, totaling 18+ work items)
   - Note review objectives (Phase 4 design compliance, test strategy)
   - Review Success Criteria (SRP/OCP/DIP compliance, Test Strategy documented)

2. **Decompose into sub-features**:
   - Apply granularity rules (8-15 ACs for infra/engine type)
   - Group review tasks by scope (code review phases, test strategy components)
   - Assign explicit Feature IDs (F487+ for review components)
   - Identify dependencies between sub-features
   - Document decomposition rationale in Execution Log

   **Expected Feature ID Allocation** (adjust during execution if needed):
   - **F493-F495**: Code Review by Phase ranges (Phase 1-4, Phase 5-8, Phase 9-12)
   - **F496**: Folder Structure Validation
   - **F497**: Naming Convention Audit
   - **F498**: Testability Assessment
   - **F499-F500**: Test Strategy Design (IRandomProvider design, test layer structure, E2E methodology)
   - **F501**: Refactoring (if needed based on review findings)
   - **F502**: Phase 15 Post-Phase Review (Task 8: type: infra)
   - **F503**: Phase 16 Planning (Task 9: type: research)

   **Decomposition Rationale**:
   9 architecture tasks → 11 features due to granularity splitting:
   - Code Review: 1 task → 3 features (Phase 1-4, 5-8, 9-12) to maintain 8-15 AC limit per feature
   - Test Strategy: 1 task → 2 features (IRandomProvider/test layers vs E2E methodology)
   - +2 transition features (Post-Phase Review + Phase 16 Planning)
   - Separate structural concerns (folder, naming) from behavioral concerns (testability)
   - Refactoring feature created only if Phase 4 deviations found during review

3. **Create feature files**:
   - Follow feature-template.md structure
   - Include Philosophy: "Phase 15: Architecture Review" per architecture.md
   - Include verification AC (review findings documented, compliance verified)
   - Reference Phase 4 design principles

4. **Create transition features**:
   - Post-Phase Review (type: infra)
   - Phase 16 Planning (type: research)

5. **Update index-features.md**:
   - Add all Phase 15 features atomically
   - Maintain dependency order

### Execution Phases

| Phase | Agent | Input | Output |
|-------|-------|-------|--------|
| 1 | spec-writer | architecture.md Phase 15, feature-template.md | Phase 15 sub-feature files |
| 2 | spec-writer | Sub-feature files | index-features.md update |
| 3 | ac-tester | Sub-feature files | AC verification |

**Execution Order**:
1. Analyze Phase 15 scope and create review component categorization
2. Create all sub-feature files with Sub-Feature Requirements
3. Update index-features.md with all Phase 15 features
4. Mark Tasks 1-5 complete

### Sub-Feature Requirements

Per architecture.md Phase 15, implementation sub-features MUST include:

**Note**: test-strategy.md is a deliverable to be created by Test Strategy features, not a pre-existing artifact.

| # | Requirement | Applies To | Verification |
|:-:|-------------|------------|--------------|
| 1 | **Philosophy inheritance** - "Phase 15: Architecture Review" in Philosophy section | All implementation features | Grep |
| 2 | **AC: Review verification** - AC verifying review findings documented | Each implementation feature | Manual inspection |
| 3 | **Phase 4 principles** - Reference to SRP/OCP/DIP/Strongly Typed IDs/Result type in Background | Code review features | Manual inspection |
| 4 | **Tasks: 負債解消** - 負債の意図的受け入れ/解消を文書化 | Each implementation feature | ドキュメント確認 |
| 5 | **AC: 負債ゼロ** - 技術負債ゼロを検証する AC を含む | Each implementation feature | AC 一覧確認 |
| 6 | **Tasks: テスト戦略** - IRandomProvider design, test layer definitions | Test Strategy features | designs/test-strategy.md creation |
| 7 | **Tasks: /do 連携** - Phase 3/6 test execution definitions | Test Strategy features | test-strategy.md section 3 |
| 8 | **Tasks: AC 検証** - AC Type verification flow definitions | Test Strategy features | test-strategy.md section 6 |
| 9 | **AC: テスト戦略** - Test strategy document completion AC | Test Strategy features | AC list check |

---

## Phase 15 Scope Reference

**Partial snapshot from architecture.md (2026-01-13)**. See [full-csharp-architecture.md](designs/full-csharp-architecture.md) for complete task list (9 tasks total).

**Phase 15: Architecture Review**

**Goal**: Phase 1-14 実装経験に基づく構造検証と軽量リファクタリング

**Background**: 150+ COM実装（Phase 12）、DDD基盤（Phase 13）、エンジン基盤（Phase 14）完了後、大規模並列フェーズ（Phase 19-21）前に構造を確定。

**Tasks (summary - see architecture.md for full list)**:
1. コードレビュー（Phase 1-12 全実装）
2. フォルダ構造の妥当性検証
3. 命名規則の一貫性確認
4. テスタビリティ課題の洗い出し
5. **Test Strategy Design（テスト戦略設計）**
   - 5.1. Era.Core 現状調査
   - 5.2. Era.Core.Tests 構造分析
   - 5.3. ERB ランダム/成長パターン抽出
   - 5.4. IRandomProvider 設計
   - 5.5. テストレイヤー構造定義
   - 5.6. E2E テスト方針策定
   - 5.7. 各 Phase の Tasks/AC への反映
6. 必要に応じた構造リファクタリング
7. 設計ドキュメント更新
8. Create Phase 15 Post-Phase Review feature (type: infra)
9. Create Phase 16 Planning feature (type: research)

**Success Criteria**:
- [ ] 全コードが Phase 4 準拠
- [ ] Test Strategy 文書化完了
- [ ] リファクタリング（必要な場合）完了
- [ ] 全テスト PASS

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F485 | Phase 14 Post-Phase Review must pass first |
| Successor | F493-F501 | Phase 15 implementation sub-features (created by this feature) |
| Successor | F502 | Post-Phase Review Phase 15 (created by this feature) |
| Successor | F503 | Phase 16 Planning (created by this feature) |

---

## Links

- [feature-471.md](feature-471.md) - Phase 14 Planning (precedent feature)
- [feature-485.md](feature-485.md) - Phase 14 Post-Phase Review (dependency, AC#6/Task#6 deferred from here)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 15 definition
- [feature-template.md](reference/feature-template.md) - Granularity guidelines

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-14 FL iter2**: [noted] Summary lines 27-29: Feature count (F493-F503, 11 features) slightly exceeds architecture.md 9 tasks. Code review uses 3 features (Phase 1-4, 5-8, 9-12) within AC limits. Test Strategy uses 2 features. Rationale added to Decomposition section.
- **2026-01-14 FL iter2**: [noted] Phase 15 Scope Reference lines 207-240: Omits Deliverables table and test-strategy.md sections. Current reference links to architecture.md for full details.
- **2026-01-14 FL iter2**: [noted] AC count (7) slightly exceeds research type guideline (3-5). F471 precedent had 9 ACs. Phase 15 complexity justifies the count.
- **2026-01-14 FL iter3**: [skipped] Phase2-Validate - AC#6/Task#6 (等価性検証 restriction): User chose to keep as-is (permit research type scope expansion).
- **2026-01-14 FL iter3**: [skipped] Phase2-Validate - AC#3 pattern "Code Review\\|Test Strategy\\|IRandomProvider": User chose to keep as-is. Pattern will be adjusted during /do if needed.
- **2026-01-14 FL iter4**: [applied] Phase3-Maintainability - Goal#6 "Verify sub-feature quality": User chose to add AC#7 for Philosophy inheritance verification.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-13 | create | spec-writer | Created from F471 Phase 14 Planning | PROPOSED |
| 2026-01-14 | START | initializer | Initialize Feature 486 | [WIP] |
| 2026-01-14 | END | opus | Phase 2 Investigation | Phase 15 Feature Mapping complete |

### Phase 15 Feature Mapping:

| architecture.md Task | Mapped To | Type | Rationale |
|---------------------|-----------|------|-----------|
| 1. Code Review (Phase 1-12) | F493, F494, F495 | engine | Split into 3 features (Phase 1-4, 5-8, 9-12) for 8-15 AC limit |
| 2. Folder Structure Validation | F496 | infra | Structural review |
| 3. Naming Convention Audit | F497 | infra | Consistency check |
| 4. Testability Assessment | F498 | engine | Mock injection review |
| 5. Test Strategy Design (5.1-5.10) | F499, F500 | engine | Split: IRandomProvider+layers (F499) vs E2E+/do integration (F500) |
| 6. Structure Refactoring | F501 | engine | Conditional (if deviations found) |
| 7. Design Document Update | (merged) | - | Merged into relevant features |
| 8. Post-Phase Review | F502 | infra | Transition feature |
| 9. Phase 16 Planning | F503 | research | Transition feature |

### Phase 15 Review Analysis:

**Source**: architecture.md Phase 15: Architecture Review (lines 3446-3593)

**Feature ID Allocation**:
| ID | Title | Type | AC Count (est) | Dependencies |
|:--:|-------|------|:--------------:|--------------|
| F493 | Code Review Phase 1-4 | engine | 10 | None |
| F494 | Code Review Phase 5-8 | engine | 10 | F493 |
| F495 | Code Review Phase 9-12 | engine | 10 | F494 |
| F496 | Folder Structure Validation | infra | 8 | F495 |
| F497 | Naming Convention Audit | infra | 8 | F495 |
| F498 | Testability Assessment | engine | 8 | F495 |
| F499 | Test Strategy Design: IRandomProvider and Test Layers | engine | 10 | F498 |
| F500 | Test Strategy Design: E2E and /do Integration | engine | 10 | F499 |
| F501 | Architecture Refactoring | engine | 8-10 | F500 (conditional) |
| F502 | Post-Phase Review Phase 15 | infra | 8 | F501 |
| F503 | Phase 16 Planning | research | 5 | F502 |

**Decomposition Rationale**:
- Code Review: Phase 1-12 has 12 phases worth of code; split into 3 features (4 phases each) to maintain manageable review scope
- Test Strategy: 10 sub-tasks in architecture.md; split into design (F499) and integration (F500)
- Folder/Naming/Testability: Kept separate for single responsibility
- Refactoring (F501): Created as explicit feature; may be skipped if no Phase 4 deviations found
- Total: 11 features (9 tasks + split adjustments for granularity)

| 2026-01-14 | START | implementer | Create F493-F503 sub-features | - |
| 2026-01-14 | END | implementer | All 11 sub-features created | SUCCESS |
| 2026-01-14 | END | opus | Task 6: 等価性検証 restriction added to architecture.md | SUCCESS |
| 2026-01-14 | END | opus | Task 7: Philosophy inheritance verified (10/10 files) | SUCCESS |
