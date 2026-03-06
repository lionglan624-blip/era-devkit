# Feature 463: Phase 13 Planning

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

**Feature を立てる Feature**: Phase 13 Planning

Create sub-features for Phase 13 DDD Foundation (expected F465-F471):
- F465: Aggregate Root + Character Aggregate (Tasks 1-2)
- F466: Repository Pattern (Tasks 3,5)
- F467: UnitOfWork Pattern (Tasks 4,6,7)
- F468: Legacy Bridge + DI Integration (Tasks 8-9)
- F469: SCOMF Full Implementation (Task 10)
- F470: Post-Phase Review Phase 13 (type: infra)
- F471: Phase 14 Planning (type: research)

**Note**: Exact feature IDs may be adjusted during execution. Above allocation follows Analysis Method step 2.

**Output**: New Feature files as primary deliverables.

**CRITICAL**: Phase 13 introduces DDD patterns (Aggregate Root, Repository, UnitOfWork) requiring careful decomposition into manageable features following granularity rules (8-15 ACs for engine type).

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity** - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points.

### Problem (Current Issue)

Phase 12 completion requires Phase 13 planning to maintain momentum:
- Phase 13 scope must be defined from architecture.md
- Sub-features must follow granularity rules (8-15 ACs for engine type per feature-template.md)
- DDD Foundation is a foundational phase (domain model patterns)
- Dependencies must be documented
- **Transition features must be created** (Post-Phase Review + Next Phase Planning)

### Goal (What to Achieve)

1. **Analyze Phase 13** requirements from full-csharp-architecture.md
2. **Decompose DDD Foundation** into manageable sub-features
3. **Create implementation sub-features** from Phase 13 tasks
4. **Create transition features** (Post-Phase Review + Phase 14 Planning)
5. **Update index-features.md** with Phase 13 features
6. **Verify sub-feature quality** (Philosophy inheritance, test PASS AC)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 13 analysis documented | file | Grep | contains | "Phase 13 Feature Mapping:" | [x] |
| 2 | DDD pattern categorization complete | file | Grep | contains | "Phase 13 DDD Analysis:" | [x] |
| 3 | At least one implementation sub-feature created | file | Grep | contains | "DDD.*Foundation" | [x] |
| 4 | Post-Phase Review in index | file | Grep | contains | "Post-Phase Review.*Phase 13" | [x] |
| 5 | Phase 14 Planning in index | file | Grep | contains | "Phase 14 Planning" | [x] |
| 6 | index-features.md updated | file | Grep | contains | "\\| 46[5-9] \\||\\| 47[0-9] \\|" | [x] |
| 7 | Implementation sub-feature has Philosophy | file | Grep | contains | "Phase 13: DDD Foundation" | [x] |
| 8 | Minimum sub-feature coverage (5+) | file | Grep | count_gte | 5 | [x] |
| 9 | Next Feature number updated | file | Grep | matches | "Next Feature number.*47[0-9]" | [x] |

### AC Details

**AC#1**: Phase 13 analysis documented in feature-463.md Execution Log
- Test: Grep pattern="Phase 13 Feature Mapping:" in feature-463.md
- Verifies mapping from architecture.md tasks to sub-features

**AC#2**: DDD pattern categorization documented
- Test: Grep pattern="Phase 13 DDD Analysis:" in feature-463.md Execution Log
- Must contain explicit Feature ID allocation table (e.g., "F465: Aggregate Root + Character Aggregate")
- Shows how 12 architecture.md tasks grouped into 5+ implementation features
- Documents decomposition rationale (DDD pattern responsibility, granularity compliance)

**AC#3**: At least one implementation sub-feature created
- Test: Grep pattern="DDD.*Foundation" in index-features.md
- Verifies implementation sub-features are registered

**AC#4**: Post-Phase Review in index
- Test: Grep pattern="Post-Phase Review.*Phase 13" in index-features.md
- Type: infra, follows F462 pattern

**AC#5**: Phase 14 Planning in index
- Test: Grep pattern="Phase 14 Planning" in index-features.md
- Type: research, follows F463 pattern

**AC#6**: index-features.md updated with Phase 13 features
- Test: Grep pattern="\\| 46[5-9] \\||\\| 47[0-9] \\|" in index-features.md
- Verifies at least one sub-feature ID in 465-479 range registered (F463, F464 already taken)
- Pattern covers expected 5-8 sub-features

**AC#7**: Implementation sub-feature has Philosophy
- Test: Grep pattern="Phase 13: DDD Foundation" in created sub-feature files
- Per architecture.md Sub-Feature Requirements

**AC#8**: Minimum sub-feature coverage (5+)
- Test: Grep pattern="\\| 46[5-9] \\||\\| 47[0-9] \\|" in index-features.md, count >= 5
- Verifies at least 5 Phase 13 sub-features created
- Note: Actual count may vary based on DDD pattern decomposition

**AC#9**: Next Feature number updated
- Test: Grep pattern="Next Feature number.*47[0-9]" in index-features.md
- Verifies Next Feature number incremented to 470-479 range after sub-feature creation
- Note: Pattern uses `.*` to accommodate markdown bold formatting in index-features.md

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze Phase 13 requirements and document "Phase 13 Feature Mapping:" | [x] |
| 2 | 2 | Document DDD pattern categorization as "Phase 13 DDD Analysis:" in Execution Log | [x] |
| 3 | 3 | Create implementation sub-features per Analysis Method step 2 (8-15 ACs per feature, grouped by DDD pattern responsibility, with 負債解消 tasks and 負債ゼロ ACs per Sub-Feature Requirements #4-5) | [x] |
| 4 | 4 | Create Phase 13 Post-Phase Review feature (type: infra) | [x] |
| 5 | 5 | Create Phase 14 Planning feature (type: research) | [x] |
| 6 | 6 | Update index-features.md with all Phase 13 features | [x] |
| 7 | 7 | Verify Philosophy inheritance in implementation sub-features | [x] |
| 8 | 8 | Verify minimum sub-feature coverage (5+ features created) | [x] |
| 9 | 9 | Update Next Feature number in index-features.md after sub-feature creation | [x] |

<!-- AC:Task 1:1 Rule: 9 ACs = 9 Tasks -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Analysis Method

1. **Read architecture.md Phase 13 section**:
   - `Game/agents/designs/full-csharp-architecture.md`
   - Identify Phase 13 tasks and scope
   - Note DDD pattern requirements (Aggregate Root, Repository, UnitOfWork)
   - Review Phase 13 design requirements

2. **Decompose into sub-features**:
   - Apply granularity rules (8-15 ACs for engine type)
   - Group DDD patterns by responsibility
   - Assign explicit Feature IDs (F465+ for DDD patterns)
   - Identify dependencies between sub-features
   - Document decomposition rationale in Execution Log

   **Expected Feature ID Allocation** (adjust during execution if needed):
   - **F465**: Aggregate Root + Character Aggregate (Tasks 1-2: 基盤クラス + Character実装)
   - **F466**: Repository Pattern (Tasks 3,5: IRepository定義 + InMemory実装)
   - **F467**: UnitOfWork Pattern (Tasks 4,6,7: IUnitOfWork定義 + 実装 + TransactionBehavior完全化)
   - **F468**: Legacy Bridge + DI Integration (Tasks 8-9: IVariableStore橋渡し + DI統合)
   - **F469**: SCOMF Full Implementation (Task 10: F435 stubs → full logic)
   - **F470**: Phase 13 Post-Phase Review (Task 11: type: infra)
   - **F471**: Phase 14 Planning (Task 12: type: research)

   **Decomposition Rationale**:
   - Group by DDD pattern responsibility (F465-F467: Core patterns, F468: Integration, F469: SCOMF)
   - Each feature targets 8-15 ACs per granularity rules
   - SCOMF separated due to distinct scope (stub completion vs new DDD implementation)
   - Transition features follow standard pattern (F470: Review, F471: Planning)

3. **Create feature files**:
   - Follow feature-template.md structure
   - Include Philosophy: "Phase 13: DDD Foundation" per architecture.md
   - Include test PASS verification AC (per Sub-Feature Requirements)
   - Reference DDD design patterns

4. **Create transition features**:
   - Post-Phase Review (type: infra)
   - Phase 14 Planning (type: research)

5. **Update index-features.md**:
   - Add all Phase 13 features atomically
   - Maintain dependency order

### Execution Phases

| Phase | Agent | Input | Output |
|-------|-------|-------|--------|
| 1 | spec-writer | architecture.md Phase 13, feature-template.md | Phase 13 sub-feature files |
| 2 | spec-writer | Sub-feature files | index-features.md update |
| 3 | ac-tester | Sub-feature files | AC#7 PASS |

**Execution Order**:
1. Analyze Phase 13 scope and create DDD pattern categorization
2. Create all sub-feature files with Sub-Feature Requirements
3. Update index-features.md with all Phase 13 features
4. Verify AC#7 passes for implementation sub-features
5. Mark Tasks 1-8 complete

### Sub-Feature Requirements

Per architecture.md Phase 13, implementation sub-features MUST include:

| # | Requirement | Applies To | Verification |
|:-:|-------------|------------|--------------|
| 1 | **Philosophy inheritance** - "Phase 13: DDD Foundation" in Philosophy section | All implementation features | AC#7 Grep |
| 2 | **AC: Test verification** - AC verifying tests pass after DDD implementation | Each implementation feature | Manual inspection |
| 3 | **DDD patterns** - Reference to Aggregate Root/Repository/UnitOfWork in Background | All implementation features | Manual inspection |
| 4 | **Tasks: 負債解消** - TODO/FIXME/HACK コメント削除タスクを含む | Each implementation feature | AC に not_contains |
| 5 | **AC: 負債ゼロ** - 技術負債ゼロを検証する AC を含む | Each implementation feature | AC 一覧確認 |

---

## Phase 13 Scope Reference

**Partial snapshot from architecture.md (2026-01-11)**. See [full-csharp-architecture.md](designs/full-csharp-architecture.md) for complete task list (12 tasks total).

**Phase 13: DDD Foundation**

**Goal**: Domain-Driven Design基盤の確立（Aggregate Root, Repository, UnitOfWork patterns）

**Background**: Phase 12までで基本機能は揃うが、ゲーム状態がフラット配列（ERBレガシー）のまま。Phase 13でドメインモデルを導入し、ビジネスルールのカプセル化と不変条件の保証を実現する。

**Tasks (summary - see architecture.md for full list)**:
1. Aggregate Root基盤クラス定義
2. Character Aggregate実装
3. IRepository<T>インターフェース定義
4. IUnitOfWork定義
5. InMemoryRepository実装
6. UnitOfWork実装
7. TransactionBehavior UoW完全実装
8. 既存IVariableStoreとの橋渡しアダプター
9. DI統合
10. SCOMF full logic implementation (F435 stubs → full SOURCE/STAIN/EXP/TCVAR logic)
11. Create Phase 13 Post-Phase Review feature (type: infra)
12. Create Phase 14 Planning feature (type: research)

**Success Criteria**:
- [ ] Aggregate Root パターン確立
- [ ] Repository パターン実装
- [ ] Domain Event 基盤動作

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F462 | Phase 12 Post-Phase Review must pass first |
| Successor | (TBD) | Phase 13 implementation sub-features (created by this feature) |
| Successor | (TBD) | Post-Phase Review Phase 13 (created by this feature) |
| Successor | (TBD) | Phase 14 Planning (created by this feature) |

---

## Links

- [feature-450.md](feature-450.md) - Phase 12 Planning (precedent feature)
- [feature-462.md](feature-462.md) - Phase 12 Post-Phase Review (dependency)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 13 definition
- [feature-template.md](reference/feature-template.md) - Granularity guidelines

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-12 FL iter1**: [resolved] Phase2-Validate - Task 3: Scope unclear - fixed by adding reference to Analysis Method step 2.
- **2026-01-12 FL iter1**: [resolved] Phase2-Validate - Missing AC: Added AC#9 for Next Feature number verification.
- **2026-01-12 FL iter3**: [resolved] AC count justification: 9 ACs acceptable per RESEARCH.md flexibility clause - Phase Planning features require verification of analysis documentation (AC#1-2), sub-feature creation/quality (AC#3,7,8), transition features (AC#4-5), and index updates (AC#6,9). Similar to F424/F437 precedent.
- **2026-01-12 FL iter3**: [accepted] Phase2-Validate - AC#6 regex pattern: Pattern follows F450 precedent which successfully passed FL. Acceptable complexity for research type feature.
- **2026-01-12 FL iter4**: [accepted] Phase2-Validate - AC#9 pattern 'Next Feature number: 47[0-9]' covers 470-479 range. Acceptable for expected 5-8 sub-feature range (F465-F471 = 7 features, Next Feature = 472).
- **2026-01-12 FL iter6**: [accepted] Phase2-Validate - AC#8 count_gte matcher kept as-is. Project-established convention per F450 precedent (10+ DONE features track record).
- **2026-01-12 FL iter7**: [accepted] Phase3-Maintainability - Task#3 reference to "Analysis Method step 2" is sufficient; that section contains the Expected Feature ID Allocation table. No change needed.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 20:51 | create | feature-builder | Created from F450 Phase 12 Planning | PROPOSED |
| 2026-01-12 | START | initializer | Initialize Feature 463 | READY |
| 2026-01-12 | START | opus | Phase 2: Investigation | - |

## Phase 13 Feature Mapping:

| architecture.md Task | Description | Target Feature |
|:--------------------:|-------------|:--------------:|
| 1 | Aggregate Root基盤クラス定義 | F465 |
| 2 | Character Aggregate実装 | F465 |
| 3 | IRepository<T>インターフェース定義 | F466 |
| 4 | IUnitOfWork定義 | F467 |
| 5 | InMemoryRepository実装 | F466 |
| 6 | UnitOfWork実装 | F467 |
| 7 | TransactionBehavior UoW完全実装 | F467 |
| 8 | 既存IVariableStoreとの橋渡しアダプター | F468 |
| 9 | DI統合 | F468 |
| 10 | SCOMF full logic implementation | F469 |
| 11 | Create Phase 13 Post-Phase Review feature | F470 |
| 12 | Create Phase 14 Planning feature | F471 |

## Phase 13 DDD Analysis:

**Decomposition Rationale**:
- **Group by DDD pattern responsibility**: Core patterns (F465-F467), Integration (F468), SCOMF (F469)
- **Granularity compliance**: Each feature targets 8-15 ACs per feature-template.md engine type
- **SCOMF separated**: Distinct scope (stub completion vs new DDD implementation)
- **Transition features**: Standard pattern (F470: Review, F471: Planning)

**Feature ID Allocation Table**:

| Feature ID | Name | Tasks | Type | AC Target |
|:----------:|------|:-----:|:----:|:---------:|
| F465 | Aggregate Root + Character Aggregate | 1-2 | engine | 8-12 |
| F466 | Repository Pattern | 3,5 | engine | 8-12 |
| F467 | UnitOfWork Pattern | 4,6,7 | engine | 10-15 |
| F468 | Legacy Bridge + DI Integration | 8-9 | engine | 8-12 |
| F469 | SCOMF Full Implementation | 10 | engine | 8-12 |
| F470 | Phase 13 Post-Phase Review | 11 | infra | 8-15 |
| F471 | Phase 14 Planning | 12 | research | 3-5 |

**DDD Pattern Categories**:
1. **Core Domain Patterns** (F465-F467): Aggregate Root, Repository, UnitOfWork
2. **Integration Layer** (F468): Bridge existing IVariableStore + DI container setup
3. **Feature Completion** (F469): Phase 9 SCOMF stubs → full implementation
4. **Transition** (F470-F471): Review + Planning

| 2026-01-12 | END | opus | Phase 2: Investigation | Task 1-2 documented |
| 2026-01-12 | START | feature-builder | Create F465-F469 implementation sub-features | - |
| 2026-01-12 | SKILL | feature-builder | Skill(feature-quality) | LOADED |
| 2026-01-12 | SKILL | feature-builder | ENGINE.md | LOADED |
| 2026-01-12 | CREATE | feature-builder | feature-465.md (Aggregate Root + Character Aggregate) | SUCCESS |
| 2026-01-12 | CREATE | feature-builder | feature-466.md (Repository Pattern) | SUCCESS |
| 2026-01-12 | CREATE | feature-builder | feature-467.md (UnitOfWork Pattern) | SUCCESS |
| 2026-01-12 | CREATE | feature-builder | feature-468.md (Legacy Bridge + DI Integration) | SUCCESS |
| 2026-01-12 | CREATE | feature-builder | feature-469.md (SCOMF Full Implementation) | SUCCESS |
| 2026-01-12 | UPDATE | feature-builder | index-features.md (registered F465-F469, Next Feature: 470) | SUCCESS |
| 2026-01-12 | VERIFY | feature-builder | Philosophy inheritance in all 5 features | PASS |
| 2026-01-12 | VERIFY | feature-builder | Minimum coverage (5 features) | PASS (5 created) |
| 2026-01-12 | END | feature-builder | Create F465-F469 | SUCCESS:5_FEATURES_CREATED |
| 2026-01-12 | START | feature-builder | Create F470-F471 transition features | - |
| 2026-01-12 | CREATE | feature-builder | feature-470.md (Phase 13 Post-Phase Review) | SUCCESS |
| 2026-01-12 | CREATE | feature-builder | feature-471.md (Phase 14 Planning) | SUCCESS |
| 2026-01-12 | UPDATE | feature-builder | index-features.md (registered F470-F471, Next Feature: 472) | SUCCESS |
| 2026-01-12 | END | feature-builder | Create F470-F471 | SUCCESS:2_FEATURES_CREATED |
| 2026-01-12 | AC-TEST | ac-tester | Verify Feature 463 Acceptance Criteria | 8/9 PASS, 1 pattern issue |
| 2026-01-12 | FIX | opus | AC#9 pattern updated to use .* for markdown compatibility | PASS |
| 2026-01-12 | END | opus | Phase 6 Verification | 9/9 PASS |
| 2026-01-12 | START | feature-reviewer | Post-Review (mode: post) | - |
| 2026-01-12 | END | feature-reviewer | Post-Review | READY |
| 2026-01-12 | START | feature-reviewer | Doc-Check (mode: doc-check) | - |
| 2026-01-12 | END | feature-reviewer | Doc-Check | READY |
| 2026-01-12 | VERIFY | opus | SSOT Update Check | PASS (no new types/commands/agents) |
| 2026-01-12 | END | opus | Phase 7 Post-Review | Gate PASS |
