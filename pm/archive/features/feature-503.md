# Feature 503: Phase 16 Planning

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

## Created: 2026-01-14

---

## Summary

**Feature を立てる Feature**: Phase 16 Planning

Create sub-features for Phase 16 C# 14 Style Migration:
- F509-F514: Implementation sub-features from Phase 16 tasks (Primary Constructors, Collection Expressions)
- F515: Post-Phase Review Phase 16 (type: infra)
- F516: Phase 17 Planning (type: research)

**Output**: New Feature files as primary deliverables.

**CRITICAL**: Phase 16 applies C# 14 patterns to existing code for simplification (~400 line reduction). Requires careful decomposition into manageable refactoring features following granularity rules (8-15 ACs for engine type).

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity** - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points.

### Problem (Current Issue)

Phase 15 completion requires Phase 16 planning to maintain momentum:
- Phase 16 scope must be defined from architecture.md
- Sub-features must follow granularity rules (8-15 ACs for engine type per feature-template.md)
- C# 14 Style Migration is a code simplification phase
- Dependencies must be documented
- **Transition features must be created** (Post-Phase Review + Next Phase Planning)

### Goal (What to Achieve)

1. **Analyze Phase 16** requirements from full-csharp-architecture.md
2. **Decompose C# 14 Migration** into manageable sub-features
3. **Create implementation sub-features** from Phase 16 tasks
4. **Create transition features** (Post-Phase Review + Phase 17 Planning)
5. **Update index-features.md** with Phase 16 features
6. **Verify sub-feature quality** (Philosophy inheritance, test PASS AC)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 16 analysis documented | file | Grep | contains | "Phase 16 Feature Mapping:" | [x] |
| 2 | C# 14 pattern categorization complete | file | Grep | contains | "Phase 16 Migration Analysis:" | [x] |
| 3 | At least one implementation sub-feature created | file | Grep | contains | "Primary Constructor\\|Collection Expression" | [x] |
| 4 | Post-Phase Review in index | file | Grep | contains | "Post-Phase Review.*Phase 16" | [x] |
| 5 | Phase 17 Planning in index | file | Grep | contains | "Phase 17 Planning" | [x] |
| 6 | Sub-feature Philosophy and test PASS AC verified | file | Grep | contains | "Phase 16: C# 14 Style Migration" | [x] |

### AC Details

**AC#1**: Phase 16 analysis documented in feature-503.md Execution Log
- Test: Grep pattern="Phase 16 Feature Mapping:" in feature-503.md
- Verifies mapping from architecture.md tasks to sub-features

**AC#2**: C# 14 pattern categorization documented
- Test: Grep pattern="Phase 16 Migration Analysis:" in feature-503.md Execution Log
- Must contain explicit Feature ID allocation table
- Shows how architecture.md tasks grouped into implementation features
- Documents decomposition rationale (migration scope, pattern types, granularity compliance)

**AC#3**: At least one implementation sub-feature created
- Test: Grep pattern="Primary Constructor|Collection Expression" path="Game/agents/index-features.md"
- Verifies at least one Phase 16 migration component is registered in index
- Note: "At least one" is intentionally loose - Expected Feature ID Allocation documents F509-F514, but exact count may vary during execution based on decomposition adjustments

**AC#4**: Post-Phase Review in index
- Test: Grep pattern="Post-Phase Review.*Phase 16" in index-features.md
- Type: infra, follows F470/F485/F502 pattern

**AC#5**: Phase 17 Planning in index
- Test: Grep pattern="Phase 17 Planning" in index-features.md
- Type: research, follows F471/F486/F503 pattern

**AC#6**: Sub-feature Philosophy and test PASS AC verified
- Test: Grep pattern="Phase 16: C# 14 Style Migration" in created sub-feature files
- Verifies all created implementation sub-features inherit Philosophy per architecture.md
- Also manually verify each sub-feature includes "dotnet test" or "Test PASS" in AC table per Sub-Feature Requirements #6

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze Phase 16 requirements and document "Phase 16 Feature Mapping:" | [x] |
| 2 | 2 | Document C# 14 pattern categorization as "Phase 16 Migration Analysis:" in Execution Log | [x] |
| 3 | 3 | Create implementation sub-features per Analysis Method (8-15 ACs per feature, grouped by C# 14 pattern, with mandatory handoff tracking per Sub-Feature Requirements) | [x] |
| 4 | 4 | Create Phase 16 Post-Phase Review feature (type: infra) | [x] |
| 5 | 5 | Create Phase 17 Planning feature (type: research) | [x] |
| 6 | 6 | Verify Philosophy inheritance AND test PASS AC presence in created sub-features (Grep "Phase 16: C# 14 Style Migration" and manual check for "dotnet test" AC) | [x] |

<!-- AC:Task 1:1 Rule: 6 ACs = 6 Tasks -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Analysis Method

1. **Read architecture.md Phase 16 section**:
   - `Game/agents/designs/full-csharp-architecture.md`
   - Identify Phase 16 tasks and scope
   - Note C# 14 patterns (Primary Constructors, Collection Expressions, etc.)
   - Review Success Criteria

2. **Decompose into sub-features**:
   - Apply granularity rules (8-15 ACs for engine type)
   - Group migration tasks by C# 14 pattern
   - Assign explicit Feature IDs
   - Identify dependencies between sub-features
   - Document decomposition rationale in Execution Log

   **Expected Feature ID Allocation** (adjust during execution if needed):
   - **F509**: Primary Constructor Migration - Training/ (5 files, ~14 readonly fields)
   - **F510**: Primary Constructor Migration - Character/ (4 files)
   - **F511**: Primary Constructor Migration - Commands/Flow/ (10 files)
   - **F512**: Primary Constructor Migration - Commands/Special/ (16 files)
   - **F513**: Primary Constructor Migration - Commands/System/ + Other (15 files)
   - **F514**: Collection Expression Migration (18 locations)
   - **F515**: Phase 16 Post-Phase Review (Task 8: type: infra)
   - **F516**: Phase 17 Planning (Task 9: type: research)

   **Decomposition Rationale**:
   9 architecture.md tasks → 8 features (net reduction by merging 2 tasks):
   - Primary Constructor: 6 directory tasks (Training, Character, Commands/Flow, Commands/Special, Commands/System, Other) → 5 features (F509-F513, merging Commands/System + Other due to simpler code structure)
   - Collection Expression: 1 task → 1 feature (F514, 18 locations)
   - Transition: 2 tasks → 2 features (F515 Post-Phase Review + F516 Phase 17 Planning)
   - Directory grouping ensures each feature has ~8-15 ACs based on file count

3. **Create feature files**:
   - Follow feature-template.md structure
   - Include Philosophy: "Phase 16: C# 14 Style Migration" per architecture.md
   - Include verification AC (migration completion, test PASS)
   - Reference C# 14 patterns being applied

4. **Create transition features**:
   - Post-Phase Review (type: infra)
   - Phase 17 Planning (type: research)

5. **Update index-features.md**:
   - Add all Phase 16 features atomically
   - Maintain dependency order

### Execution Phases

| Phase | Agent | Input | Output |
|-------|-------|-------|--------|
| 1 | feature-builder | architecture.md Phase 16, feature-template.md | Phase 16 sub-feature files |
| 2 | feature-builder | Sub-feature files | index-features.md update |
| 3 | ac-tester | Sub-feature files | AC verification |

**Execution Order**:
1. Analyze Phase 16 scope and create C# 14 pattern categorization
2. Create all sub-feature files with Philosophy inheritance
3. Update index-features.md with all Phase 16 features
4. Mark Tasks 1-5 complete

### Sub-Feature Requirements

Per architecture.md Phase 16, implementation sub-features MUST include:

| # | Requirement | Applies To | Verification |
|:-:|-------------|------------|--------------|
| 1 | **Philosophy inheritance** - "Phase 16: C# 14 Style Migration" in Philosophy section | All implementation features | Grep |
| 2 | **AC: Migration verification** - AC verifying migration completion | Each implementation feature | Manual inspection |
| 3 | **C# 14 pattern reference** - Specific pattern being applied in Background | Migration features | Manual inspection |
| 4 | **Tasks: Handoff tracking** - Use 引継ぎ先指定 section with concrete tracking IDs | Each implementation feature | 引継ぎ先指定 section check |
| 5 | **AC: No debt markers** - No TODO/FIXME/HACK added during migration (Grep pattern: `TODO|FIXME|HACK`) | Each implementation feature | Grep verification |
| 6 | **AC: Test PASS** - All tests PASS after migration | Each implementation feature | dotnet test verification |

---

## Phase 16 Scope Reference

**Partial snapshot from architecture.md** (line 3648+). See [full-csharp-architecture.md](designs/full-csharp-architecture.md) for complete task list.

**Phase 16: C# 14 Style Migration**

**Goal**: 既存コードへの C# 14 パターン適用によるコード簡潔化

**Background**: Phase 10 で .NET 10 / C# 14 を有効化したが、既存コードは旧スタイルのまま。Primary Constructor 等の新機能を適用することで ~400行のボイラープレートを削減。

**Tasks** (per architecture.md line 3660-3680):
1. Training/ ディレクトリ (5 files) - Primary Constructor
2. Character/ ディレクトリ (4 files) - Primary Constructor
3. Commands/Flow/ ディレクトリ (10 files) - Primary Constructor
4. Commands/Special/ ディレクトリ (16 files) - Primary Constructor
5. Commands/System/ ディレクトリ (3 files) - Primary Constructor
6. その他 (12 files) - Primary Constructor (Common/, Variables/, Functions/)
7. Collection Expression 適用 (18 locations)
8. **Create Phase 16 Post-Phase Review feature** (type: infra)
9. **Create Phase 17 Planning feature** (type: research)

*Note*: File-scoped namespace and Extension member migrations mentioned in architecture.md as potential opportunities but not planned for F503 sub-features (already using file-scoped namespaces, no applicable extension member opportunities identified).

**Success Criteria**:
- [ ] Primary Constructor 適用完了
- [ ] Collection Expression 適用完了
- [ ] ボイラープレート ~400行削減確認
- [ ] 全テスト PASS
- [ ] No functional changes (refactoring only)

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F502 | Phase 15 Post-Phase Review must pass first |
| Successor | F509-F514 | Phase 16 implementation sub-features (created by this feature) |
| Successor | F515 | Phase 16 Post-Phase Review |
| Successor | F516 | Phase 17 Planning |

---

## Links

- [feature-486.md](feature-486.md) - Phase 15 Planning (precedent feature)
- [feature-502.md](feature-502.md) - Phase 15 Post-Phase Review (dependency)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 16 definition
- [feature-template.md](reference/feature-template.md) - Granularity guidelines
- [feature-509.md](feature-509.md) - Primary Constructor Migration - Training
- [feature-510.md](feature-510.md) - Primary Constructor Migration - Character
- [feature-511.md](feature-511.md) - Primary Constructor Migration - Commands/Flow
- [feature-512.md](feature-512.md) - Primary Constructor Migration - Commands/Special
- [feature-513.md](feature-513.md) - Primary Constructor Migration - Commands/System + Other
- [feature-514.md](feature-514.md) - Collection Expression Migration
- [feature-515.md](feature-515.md) - Phase 16 Post-Phase Review
- [feature-516.md](feature-516.md) - Phase 17 Planning

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題なし: セクション空 - 実行時に課題が発生した場合は追記 -->

No deferred tasks identified at PROPOSED stage.

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-16 FL iter1**: AC count (6) exceeds research type guideline (3-5). Phase 16 C# 14 migration complexity with mandatory Sub-Feature Requirements (#4-#6) justifies the count.
- **2026-01-16 FL iter2**: [resolved] Sub-Feature Requirements #4-#5 terminology updated per F507/F508 (replaced '負債の意図的受け入れ/解消' with mandatory handoff tracking).
- **2026-01-16 FL iter6**: [resolved] Sub-Feature Requirements #5 updated with explicit Grep pattern (TODO|FIXME|HACK).
- **2026-01-16 FL iter9**: [skipped] Phase3-Maintainability - Task#3 reference 'per Sub-Feature Requirements' is unambiguous within document. Minor clarity improvement possible but not required.
- **2026-01-16 FL iter11**: [applied] Phase3-Maintainability - Goal#6 'test PASS AC' coverage: AC#6/Task#6 expanded to verify both Philosophy AND test PASS AC presence per user decision.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-14 | create | spec-writer | Created from F486 Phase 15 Planning | PROPOSED |
| 2026-01-16 | START | do | Phase 1 Initialize | READY:503:research |
| 2026-01-16 | START | do | Phase 2 Investigation | verified |
| 2026-01-16 | START | do | Phase 4 Implementation | - |

### Phase 16 Feature Mapping:

| architecture.md Task | Target Feature | Content |
|---------------------|----------------|---------|
| Task 1: Training/ (5 files) | F509 | Primary Constructor - Training directory |
| Task 2: Character/ (4 files) | F510 | Primary Constructor - Character directory |
| Task 3: Commands/Flow/ (10 files) | F511 | Primary Constructor - Commands/Flow directory |
| Task 4: Commands/Special/ (16 files) | F512 | Primary Constructor - Commands/Special directory |
| Task 5+6: Commands/System/ + Other (15 files) | F513 | Primary Constructor - Commands/System + Other |
| Task 7: Collection Expression (18 locations) | F514 | Collection Expression Migration |
| Task 8: Post-Phase Review | F515 | Phase 16 Post-Phase Review (infra) |
| Task 9: Phase 17 Planning | F516 | Phase 17 Planning (research) |

### Phase 16 Migration Analysis:

**Verified File Counts** (from Investigation Phase):
- Training/: 10 files with `private readonly` (architecture.md claims 5)
- Character/: 4 files (matches)
- Commands/Flow/: 11 handler files (architecture.md claims 10)
- Commands/Special/: 16 files (matches)
- Commands/System/: 3 files (matches)
- Other (Common+Variables+Functions): 12 files (matches)
- Collection Expression: 18 occurrences in 4 files (matches)

**Decomposition Rationale**:
- 9 architecture.md tasks → 8 features (net reduction by merging Tasks 5+6)
- Primary Constructor: 6 directory tasks → 5 features (F509-F513)
  - F513 merges Commands/System + Other (simpler code structure, lower complexity)
- Collection Expression: 1 task → 1 feature (F514)
- Transition: 2 tasks → 2 features (F515 Post-Phase Review + F516 Phase 17 Planning)
- Directory grouping ensures each feature has ~8-15 ACs based on file count

**Feature ID Allocation**:
| Feature ID | Type | Content | AC Count (estimated) |
|:----------:|:----:|---------|:--------------------:|
| F509 | engine | Primary Constructor - Training/ | 10 |
| F510 | engine | Primary Constructor - Character/ | 8 (4 files + verification) |
| F511 | engine | Primary Constructor - Commands/Flow/ | 11 |
| F512 | engine | Primary Constructor - Commands/Special/ | 16 |
| F513 | engine | Primary Constructor - Commands/System + Other | 15 |
| F514 | engine | Collection Expression Migration | 8 (4 files + verification) |
| F515 | infra | Post-Phase Review Phase 16 | 8-15 |
| F516 | research | Phase 17 Planning | 3-5 |

| 2026-01-16 | END | feature-builder | F509-F516 created | SUCCESS |
| 2026-01-16 | END | ac-tester | AC verification 6/6 | PASS |
| 2026-01-16 | END | feature-reviewer | post + doc-check | READY |
