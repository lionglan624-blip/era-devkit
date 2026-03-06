# Feature 447: Phase 11 Planning

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

**Feature を立てる Feature**: Phase 11 Planning

Create sub-features for Phase 11 xUnit v3 Migration:
- F448: xUnit v3 Migration (package updates, API changes, test verification)
- F449: Phase 11 Post-Phase Review (type: infra)
- F450: Phase 12 Planning (type: research) - plans next phase per architecture.md phase ordering

**Output**: New Feature files (feature-448.md through feature-450.md) as primary deliverables.

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity**: Each phase completion triggers next phase planning. This ensures:
- Continuous development pipeline
- Clear phase boundaries
- Documented transition points

### Problem (Current Issue)

Phase 10 completion requires Phase 11 planning to maintain momentum:
- Phase 11 scope must be defined from architecture.md
- Sub-features must follow granularity rules (8-15 ACs for engine type per feature-template.md)
- Dependencies must be documented
- **Transition features must be created** (Post-Phase Review + Next Phase Planning)

### Goal (What to Achieve)

1. **Analyze Phase 11** requirements from full-csharp-architecture.md
2. **Create implementation sub-features** from Phase 11 tasks
3. **Create transition features** (Post-Phase Review + Next Phase Planning)
4. **Update index-features.md** with Phase 11 features
5. **Verify sub-feature quality** (Philosophy inheritance, test PASS AC)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Feature mapping documented | file | Grep | contains | "Phase 11 Feature Mapping:" | [x] |
| 2 | F448 created (xUnit v3 Migration) | file | Glob | exists | "feature-448.md" | [x] |
| 3 | F449 created (Post-Phase Review) | file | Glob | exists | "feature-449.md" | [x] |
| 4 | F450 created (Phase 12 Planning) | file | Glob | exists | "feature-450.md" | [x] |
| 5 | index-features.md updated | file | Grep | contains | "\\| 448 \\|" | [x] |
| 6 | F448 has Philosophy | file | Grep | contains | "Phase 11: xUnit v3 Migration" | [x] |
| 7 | F448 has test PASS AC | file | Grep | contains | "dotnet test.*succeeds" | [x] |

### AC Details

**AC#1**: Grep for "Phase 11 Feature Mapping:" in feature-447.md Execution Log

**AC#2-4**: Sub-feature files exist
- F448: xUnit v3 Migration (package updates, API changes, test verification)
- F449: Phase 11 Post-Phase Review (type: infra)
- F450: Phase 12 Planning (type: research)

**AC#5**: Grep for "| 448 |" in index-features.md to verify Phase 11 features added. F448 entry implies F449-F450 were added atomically.

**AC#6**: Grep for "Phase 11: xUnit v3 Migration" in F448 feature file. Per architecture.md Sub-Feature Requirements.

**Verification**: `grep -l "Phase 11: xUnit v3 Migration" Game/agents/feature-448.md | wc -l` → expect 1.

**AC#7**: Grep for "dotnet test.*succeeds" in F448 feature file. Per architecture.md Sub-Feature Requirements for test verification AC.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Document "Phase 11 Feature Mapping:" in Execution Log | [x] |
| 2 | 2 | Create F448: xUnit v3 Migration (package updates, API changes, test verification) | [x] |
| 3 | 3 | Create F449: Phase 11 Post-Phase Review (type: infra) | [x] |
| 4 | 4 | Create F450: Phase 12 Planning (type: research) | [x] |
| 5 | 5 | Update index-features.md with F448-F450 | [x] |
| 6 | 6 | Verify F448 has "Phase 11: xUnit v3 Migration" in Philosophy | [x] |
| 7 | 7 | Verify F448 has test PASS AC | [x] |

<!-- AC:Task 1:1 Rule: 7 ACs = 7 Tasks -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Analysis Method

1. **Read architecture.md Phase 11 section**:
   - `Game/agents/designs/full-csharp-architecture.md`
   - Identify Phase 11 tasks and scope
   - Note Success Criteria

2. **Decompose into sub-features**:
   - Apply granularity rules (8-15 ACs for engine type)
   - Group related tasks by responsibility
   - Identify dependencies between sub-features

3. **Create feature files**:
   - Follow feature-template.md structure
   - Include Philosophy: "Phase 11: xUnit v3 Migration" per architecture.md
   - Include test PASS verification AC (per Sub-Feature Requirements)

4. **Create transition features**:
   - Post-Phase Review (type: infra)
   - Phase 12 Planning (type: research)

5. **Update index-features.md**:
   - Add all Phase 11 features atomically
   - Maintain dependency order

### Execution Phases

| Phase | Agent | Input | Output |
|-------|-------|-------|--------|
| 1 | spec-writer | architecture.md Phase 11, feature-template.md | F448-F450 feature files |
| 2 | spec-writer | F448-F450 files | index-features.md update |
| 3 | ac-tester | F448 file | AC#6-7 PASS |

**Execution Order**:
1. Create all sub-feature files (F448-F450) with Sub-Feature Requirements
2. Update index-features.md with all Phase 11 features
3. Verify AC#6-7 pass for implementation sub-feature (F448)
4. Mark Tasks 1-7 complete

### Sub-Feature Requirements

Per architecture.md Phase 11, implementation sub-features MUST include:

| # | Requirement | Applies To | Verification |
|:-:|-------------|------------|--------------|
| 1 | **Philosophy inheritance** - "Phase 11: xUnit v3 Migration" in Philosophy section | F448 | AC#6 Grep |
| 2 | **AC: Test verification** - AC verifying all tests pass after migration | F448 | AC#7 Grep "dotnet test" |

---

## Phase 11 Scope Reference

**Snapshot from architecture.md (2026-01-10)**. See [full-csharp-architecture.md](designs/full-csharp-architecture.md) for authoritative version.

**Phase 11: xUnit v3 Migration**

**Goal**: xUnit v2 → v3 migration (breaking changes)

**Scope**: All test projects

**Tasks** → **Feature Mapping**:

| Task | Feature | Scope |
|------|:-------:|-------|
| 1. xUnit v3 migration guide analysis | xUnit v3 Migration | Investigation |
| 2. Package reference change (`xunit` → `xunit.v3`) | xUnit v3 Migration | 7 test projects |
| 3. xunit.runner.visualstudio 3.x update | xUnit v3 Migration | 7 test projects |
| 4. Test code modification (API changes) | xUnit v3 Migration | Code updates |
| 5. MTP (Microsoft Test Platform) v2 verification | xUnit v3 Migration | Compatibility |
| 6. All tests run PASS verification | xUnit v3 Migration | Test execution |
| 7. Post-Phase Review creation | Post-Phase Review | type: infra |
| 8. Phase 12 Planning creation | Phase 12 Planning | type: research |

**Sub-Feature Decomposition**:
- **xUnit v3 Migration (Tasks 1-6)**: Package updates + API changes + test verification (simpler than Phase 10, expected 8-12 ACs)
- **Post-Phase Review (Task 7)**: Phase 11 completion review (type: infra)
- **Phase 12 Planning (Task 8)**: Next phase planning (type: research)

**Success Criteria**:
- [ ] All test projects use xUnit v3 (xunit.v3 package)
- [ ] xunit.runner.visualstudio 3.x installed
- [ ] All tests pass with xUnit v3
- [ ] MTP v2 compatibility verified

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F446 | Phase 10 Post-Phase Review must pass first |
| Successor | F448 | xUnit v3 Migration (created by this feature) |
| Successor | F449 | Phase 11 Post-Phase Review (created by this feature) |
| Successor | F450 | Phase 12 Planning (created by this feature) |

---

## Links

- [feature-437.md](feature-437.md) - Phase 10 Planning (precedent feature)
- [feature-446.md](feature-446.md) - Phase 10 Post-Phase Review (dependency)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 11 definition
- [feature-template.md](reference/feature-template.md) - Granularity guidelines
- [feature-448.md](feature-448.md) - F448 xUnit v3 Migration (to be created)
- [feature-449.md](feature-449.md) - F449 Phase 11 Post-Phase Review (to be created)
- [feature-450.md](feature-450.md) - F450 Phase 12 Planning (to be created)

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

- **2026-01-11 FL iter1**: AC count (7) exceeds research type guideline (3-5). Justified breakdown: 3 deliverable existence ACs (F448-F450) + 1 mapping documentation AC + 1 index update AC + 2 quality verification ACs (Philosophy, test PASS AC). Each AC maps 1:1 to Task. Precedent: F437 (9 ACs for 4 sub-features). Pattern follows F437 exactly.
- **2026-01-11 FL iter1 (post-mode)**: Fixed sub-feature quality issues per FL post responsibility:
  - F450 AC#3-5: Changed Glob→Grep patterns (non-standard naming fixed)
  - F450 AC#2: Changed marker to "Phase 12 COM Analysis:" (avoid conflict with Scope Reference)
  - F450 AC#7: Clarified description to "Implementation sub-feature has Philosophy"
  - F449 AC#4-5: Added re-verification intent notes (Post-Phase Review pattern)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 | create | implementer | Created from F437 Phase 10 Planning | PROPOSED |
| 2026-01-11 | start | spec-writer | Tasks 1-5 | - |

### Phase 11 Feature Mapping:

**From architecture.md Phase 11 Tasks → Sub-Features**:

| Task | Feature | Scope |
|------|:-------:|-------|
| 1. xUnit v3 migration guide analysis | F448 | Investigation |
| 2. Package reference change (`xunit` → `xunit.v3`) | F448 | 7 test projects |
| 3. xunit.runner.visualstudio 3.x update | F448 | 7 test projects |
| 4. Test code modification (API changes) | F448 | Code updates |
| 5. MTP (Microsoft Test Platform) v2 verification | F448 | Compatibility |
| 6. All tests run PASS verification | F448 | Test execution |
| 7. Post-Phase Review creation | F449 | type: infra |
| 8. Phase 12 Planning creation | F450 | type: research |

**Sub-Features Created**:
- **F448: xUnit v3 Migration** (Tasks 1-6) - Type: engine, 11 ACs
  - Package updates for 7 test projects
  - Runner upgrade to 3.x
  - API breaking changes handling
  - Build + Test PASS verification
  - Philosophy: "Phase 11: xUnit v3 Migration"

- **F449: Phase 11 Post-Phase Review** (Task 7) - Type: infra, 8 ACs
  - F448 completion verification
  - Success Criteria verification (xUnit v3, tests PASS, MTP v2)
  - Architecture.md update with Implementation Notes
  - Phase 12 transition approval

- **F450: Phase 12 Planning** (Task 8) - Type: research, 7 ACs
  - Phase 12 COM Implementation planning
  - 150+ COMF file categorization
  - Sub-feature creation (COM migration by category)
  - Post-Phase Review + Phase 13 Planning features

**Verification**:
- ✅ F448 has Philosophy inheritance (AC#6)
- ✅ F448 has test PASS AC (AC#7)
- ✅ index-features.md updated with F448-F450 (AC#5)

| 2026-01-11 | complete | spec-writer | Tasks 1-5 | SUCCESS |
