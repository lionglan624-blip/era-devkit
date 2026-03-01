# Feature 541: Phase 18 Planning

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

**Feature を立てる Feature**: Phase 18 Planning

Create sub-features for Phase 18 KojoEngine SRP分割:
- IDialogueLoader interface and YamlDialogueLoader implementation
- IConditionEvaluator interface and ConditionEvaluator implementation
- Specification Pattern基盤実装 (ISpecification, TalentSpecification, etc.)
- IDialogueRenderer interface and TemplateDialogueRenderer implementation
- IDialogueSelector interface and PriorityDialogueSelector implementation
- KojoEngine Facade再構成
- F554: Post-Phase Review Phase 18 (type: infra)
- F555: Phase 19 Planning (type: research)

**Output**: New Feature files as primary deliverables.

**CRITICAL**: Phase 18 splits KojoEngine (391-line monolith) into 4 Single Responsibility classes: Loading, Validation, Evaluation, Rendering. Introduces Specification Pattern for complex condition logic (TALENT/ABL/EXP branching). Requires interface extraction, implementation creation, DI registration, and Facade pattern for backward compatibility.

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity** - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. Follows F471/F486/F503/F516 pattern.

### Problem (Current Issue)

Phase 17 completion requires Phase 18 planning to maintain momentum:
- Phase 18 scope must be defined from full-csharp-architecture.md
- Sub-features must follow granularity rules (8-15 ACs for engine type per feature-template.md)
- KojoEngine SRP分割 is a refactoring phase with strict SOLID principles adherence
- Dependencies must be documented (Interface extraction before implementation)
- **Transition features must be created** (Post-Phase Review + Next Phase Planning)

### Goal (What to Achieve)

1. **Analyze Phase 18** requirements from full-csharp-architecture.md
2. **Decompose KojoEngine SRP分割** into manageable sub-features following SRP decomposition order
3. **Create implementation sub-features** from Phase 18 tasks
4. **Create transition features** (Post-Phase Review + Phase 19 Planning)
5. **Update index-features.md** with Phase 18 features
6. **Verify sub-feature quality** (Philosophy inheritance, SRP adherence, interface-first design)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 18 analysis documented | file | Grep | contains | "Phase 18 Feature Mapping:" | [x] |
| 2 | SRP responsibility categorization complete | file | Grep | contains | "Phase 18 SRP Analysis:" | [x] |
| 3 | Interface extraction sub-features created | file | Grep | contains | "IDialogueLoader\\|IConditionEvaluator\\|IDialogueRenderer\\|IDialogueSelector" | [x] |
| 4 | Post-Phase Review in index | file | Grep | contains | "Post-Phase Review.*Phase 18" | [x] |
| 5 | Phase 19 Planning in index | file | Grep | contains | "Phase 19 Planning" | [x] |
| 6 | Sub-feature Philosophy verified | file | Grep | contains | "Philosophy.*Phase 18.*KojoEngine.*SRP" | [x] |
| 7 | Sub-feature count verified | file | Glob | count_gte | 14 | [x] |

### AC Details

**AC#1**: Phase 18 analysis documented
- Method: `Grep("Phase 18 Feature Mapping:", "Game/agents/feature-541.md")`
- Expected: Analysis table mapping Phase 18 tasks to features

**AC#2**: SRP responsibility categorization complete
- Method: `Grep("Phase 18 SRP Analysis:", "Game/agents/feature-541.md")`
- Expected: Four responsibilities identified and grouped for feature creation

**AC#3**: Interface extraction sub-features created
- Method: `Grep("IDialogueLoader\\|IConditionEvaluator\\|IDialogueRenderer\\|IDialogueSelector", "Game/agents/index-features.md")`
- Expected: At least one interface-related feature name in index (OR pattern matches any of 4 interfaces)

**AC#4**: Post-Phase Review in index
- Method: `Grep("Post-Phase Review.*Phase 18", "Game/agents/index-features.md")`
- Expected: F590 Post-Phase Review Phase 18 entry

**AC#5**: Phase 19 Planning in index
- Method: `Grep("Phase 19 Planning", "Game/agents/index-features.md")`
- Expected: F591 Phase 19 Planning entry

**AC#6**: Sub-feature Philosophy verified
- Method: `Grep("Philosophy.*Phase 18.*KojoEngine.*SRP", "Game/agents/feature-54*.md")` and `Grep("Philosophy.*Phase 18.*KojoEngine.*SRP", "Game/agents/feature-55[0-5].md")`
- Expected: Philosophy inheritance in created sub-features F542-F555
- Note: Both Grep commands must return matches for AC to PASS. Sub-features MUST use corrected "Phase 18" (not stale "Phase 17" from architecture.md line 4055)

**AC#7**: Sub-feature count verified
- Method: `Glob("Game/agents/feature-54[2-9].md")` + `Glob("Game/agents/feature-55[0-5].md")` (run both)
- Expected: At least 14 sub-features created (F542-F555 = 8 + 6 files)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Read Phase 18 section from full-csharp-architecture.md and create feature mapping | [x] |
| 2 | 2 | Analyze KojoEngine responsibilities (Loading, Validation, Evaluation, Rendering) and group into features | [x] |
| 3 | 3,7 | Create interface extraction sub-features following dependency order and update index-features.md | [x] |
| 4 | 4,5 | Create transition features (Post-Phase Review + Phase 19 Planning) | [x] |
| 5 | 6 | Verify sub-feature quality and Philosophy inheritance | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Analysis Method

1. Read Phase 18 section from full-csharp-architecture.md:
   - Tasks list (11 SRP tasks + 2 transition tasks = 13 total)
   - Core Interfaces specifications
   - Specification Pattern examples
   - KojoEngine Facade refactoring requirements

2. Identify decomposition targets:
   - Interface extractions → Implementation features
   - Specification Pattern → Base infrastructure + concrete implementations
   - Facade refactoring → Integration feature

3. Group by dependency order:
   - Interface definitions before implementations
   - Base patterns before concrete classes
   - Individual components before facade integration

### Expected Feature ID Allocation

| Phase 18 Component | Feature ID Range | Justification |
|-------------------|------------------|---------------|
| Interface Extractions | F542-F545 | 4 main interfaces (Loader, Evaluator, Renderer, Selector) |
| Specification Pattern | F546-F548 | Base pattern + Talent/Abl specifications + composite specs |
| Implementation Classes | F549-F552 | Concrete implementations for 4 interfaces |
| Facade Refactoring | F553 | KojoEngine integration and DI registration |
| Transition Features | F554-F555 | Post-Phase Review + Phase 19 Planning |

**Total**: ~14 sub-features (F542-F555)

**Decomposition Rationale**: architecture.md lists 13 tasks (11 SRP + 2 transition) but allocation shows 14 features because:
- Specification Pattern (architecture Tasks 5-6) expanded into 3 features (F546-F548) for proper granularity:
  - F546: Specification Pattern base infrastructure (ISpecification interface, base classes)
  - F547: Concrete specifications (TalentSpecification, AblSpecification)
  - F548: Composite specifications (AndSpecification, OrSpecification)
- This ensures each feature follows 8-15 AC guideline per feature-template.md

### Sub-Feature Requirements

Per architecture.md Phase 18 lines 4051-4057, implementation sub-features MUST include:

| # | Requirement | Applies To | Verification |
|:-:|-------------|------------|--------------|
| 1 | **Philosophy inheritance** - "Phase 18: KojoEngine SRP分割" in Philosophy section | All implementation features | Grep |
| 2 | **Tasks: 負債解消** - TODO/FIXME/HACK comment removal tasks | Each implementation feature | AC with not_contains |
| 3 | **AC: 負債ゼロ** - No debt markers added during refactoring | Each implementation feature | Grep verification |
| 4 | **AC: Test PASS** - All tests PASS after refactoring | Each implementation feature | dotnet test verification |
| 5 | **AC: SRP Compliance** - Each class has single responsibility | Interface/Impl features | Manual inspection |
| 6 | **AC: DI Registration** - Services registered in ServiceCollectionExtensions | F553 Facade feature | Grep verification |
| 7 | **Tasks: Handoff tracking** - Use 引継ぎ先指定 section with concrete tracking IDs | Each implementation feature | 引継ぎ先指定 section check |

**NOTE**: architecture.md line 4055 shows stale phase number "Phase 17: KojoEngine SRP" instead of "Phase 18". This is tracked in 引継ぎ先指定 for F554 (Post-Phase Review) to fix.

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - AC#4/AC#5: Patterns search for text ("Post-Phase Review.*Phase 18", "Phase 19 Planning") not specific IDs - validated as OK
- **2026-01-18 FL iter1**: [resolved] Phase2-Validate - 引継ぎ先指定: architecture.md stale phase number handoff added (F555)
- **2026-01-18 FL iter2**: [resolved] Phase2-Validate - F554 Testing Infrastructure: Removed from allocation (not in architecture.md Phase 18 tasks)
- **2026-01-18 FL iter4**: [resolved] Phase2-Validate - AC:Task 1:1: 7 ACs with 5 Tasks - per RESEARCH.md line 210 "3-5 AC count guideline (flexible: more ACs acceptable with justification)" and logical Task grouping (Task#3=create+count, Task#4=transitions)
- **2026-01-18 FL iter5**: [resolved] Phase2-Validate - AC#3 pattern updated to include IDialogueSelector; AC#6/AC#7 paths narrowed to F542-F555 range
- **2026-01-18 FL iter7**: [resolved] Phase3-Maintainability - AC#6 Details clarified (both Grep must pass); AC#3 description/details aligned

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| Interface documentation | Out of scope for planning feature | Feature | F542-F545 |
| Unit test coverage | Implementation phase concern | Feature | F549-F553 |
| architecture.md Phase 18 stale phase numbers | line 3912, 4055 say Phase 15/17 instead of Phase 18/19 | Feature | F554 |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | feature creation | PENDING |
| 2026-01-18 | Task#1 | Opus | Phase 18 Feature Mapping documentation | SUCCESS |
| 2026-01-18 | Task#2 | Opus | Phase 18 SRP Analysis documentation | SUCCESS |
| 2026-01-18 | Task#3 | feature-creator x4 | Created F542-F555 (14 sub-features) | SUCCESS |
| 2026-01-18 | Task#4 | feature-creator | Created F554, F555 (transition features) | SUCCESS |
| 2026-01-18 | Task#5 | Opus | Verified Philosophy inheritance (13/14 files have KojoEngine SRP) | SUCCESS |
| 2026-01-18 | Phase 6 | Opus | AC verification - all 7 ACs PASS | SUCCESS |
| 2026-01-18 | Phase 7.1 | feature-reviewer | Post review - status update deferred to Phase 9 | READY |
| 2026-01-18 | Phase 7.2 | feature-reviewer | Doc-check - all links valid | READY |
| 2026-01-18 | Phase 7.3 | Opus | SSOT update check - N/A (no code changes) | N/A |
| 2026-01-18 | Phase 9 | finalizer | Status update [WIP] → [DONE] | SUCCESS |

---

### Phase 18 Feature Mapping:

**Source**: architecture.md Phase 18 section (lines 3890-4064)

| architecture.md Task | Feature ID | Feature Name |
|---------------------|:----------:|--------------|
| Task 1: IDialogueLoader interface | F542 | IDialogueLoader Interface Extraction |
| Task 2: YamlDialogueLoader implementation | F549 | YamlDialogueLoader Implementation |
| Task 3: IConditionEvaluator interface | F543 | IConditionEvaluator Interface Extraction |
| Task 4: ConditionEvaluator implementation | F550 | ConditionEvaluator Implementation |
| Task 5: Specification Pattern base | F546 | Specification Pattern Infrastructure |
| Task 6: TalentSpecification, AblSpecification | F547, F548 | Concrete + Composite Specifications |
| Task 7: IDialogueRenderer interface | F544 | IDialogueRenderer Interface Extraction |
| Task 8: TemplateDialogueRenderer impl | F551 | TemplateDialogueRenderer Implementation |
| Task 9: IDialogueSelector interface | F545 | IDialogueSelector Interface Extraction |
| Task 10: PriorityDialogueSelector impl | F552 | PriorityDialogueSelector Implementation |
| Task 11: KojoEngine Facade refactoring | F553 | KojoEngine Facade Refactoring |
| Phase Progression: Post-Phase Review | F554 | Post-Phase Review Phase 18 |
| Phase Progression: Next Phase Planning | F555 | Phase 19 Planning |

**Total**: 14 features (F542-F555)

---

### Phase 18 SRP Analysis:

**Responsibility Decomposition** (per SOLID SRP principle):

| Responsibility | Interface | Implementation | Feature IDs |
|----------------|-----------|----------------|-------------|
| **Loading** | IDialogueLoader | YamlDialogueLoader | F542, F549 |
| **Evaluation** | IConditionEvaluator | ConditionEvaluator | F543, F550 |
| **Rendering** | IDialogueRenderer | TemplateDialogueRenderer | F544, F551 |
| **Selection** | IDialogueSelector | PriorityDialogueSelector | F545, F552 |
| **Specification** | ISpecification<T> | TalentSpec, AblSpec, Composites | F546, F547, F548 |
| **Facade** | IKojoEngine | KojoEngine (refactored) | F553 |

**Dependency Order**:
1. F542-F545: Interface extractions (parallel, no dependencies)
2. F546: Specification Pattern infrastructure (parallel with interfaces)
3. F547-F548: Concrete + composite specifications (depends on F546)
4. F549-F552: Implementation classes (depend on respective interfaces)
5. F553: Facade refactoring (depends on all F549-F552)
6. F554-F555: Transition features (depend on F553)

**Sub-Feature Requirements Checklist** (per architecture.md line 4051-4057):
- [x] Philosophy: "Phase 18: KojoEngine SRP分割"
- [x] Tasks: 負債解消 (TODO/FIXME/HACK removal)
- [x] AC: 負債ゼロ (Grep `TODO|FIXME|HACK` not_contains)
- [x] AC: Test PASS (dotnet test)
- [x] AC: SRP Compliance (single responsibility per class)
- [x] AC: DI Registration (F553 only)

## Dependencies

| Type | ID | Description | Status |
|------|----|-------------|:------:|
| Predecessor | F540 | Post-Phase Review Phase 17 | [PROPOSED] |
| Successor | F542-F555 | Phase 18 implementation features | - |

## Links

- [index-features.md](index-features.md)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md)
- [F516: Phase 17 Planning](feature-516.md) - predecessor research feature
- [F540: Post-Phase Review Phase 17](feature-540.md) - predecessor review
- [feature-template.md](reference/feature-template.md)