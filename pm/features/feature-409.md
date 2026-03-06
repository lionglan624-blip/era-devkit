# Feature 409: Phase 8 Planning

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

## Created: 2026-01-08

---

## Summary

**Feature to create Features**: Create Phase 8 sub-features from full-csharp-architecture.md.

Analyze architecture.md Phase 8 (Expression & Function System) and create implementation sub-features.

**Output**: New Feature files (feature-{ID}.md) as primary deliverables.

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity**: Each phase completion triggers next phase planning. This ensures:
- Continuous development pipeline
- Clear phase boundaries
- Documented transition points

### Problem (Current Issue)

Phase 7 completion requires Phase 8 planning to maintain momentum:
- Phase 8 scope must be defined from architecture.md
- Sub-features must follow granularity rules (8-15 ACs for engine type per feature-template.md)
- Dependencies must be documented
- **Transition features must be created** (Post-Phase Review + Next Phase Planning)

Phase 8 involves complex expression evaluation system with 30+ operators and 100+ built-in functions requiring careful decomposition into implementable units.

### Goal (What to Achieve)

1. **Analyze Phase 8** requirements from full-csharp-architecture.md
2. **Create implementation sub-features** (F416-F422):
   - F416: ExpressionParser migration (AST generation)
   - F417: Operator implementation (30+ operators, all categories)
   - F418: Built-in functions Core (Math/Random/Conversion)
   - F419: Built-in functions Data (String/Array)
   - F420: Built-in functions Game (Character/System)
   - F421: Function call mechanism
   - F422: Type conversion & casting
3. **Create transition features** (F423-F424):
   - F423: Phase 8 Post-Phase Review (type: infra)
   - F424: Phase 9 Planning (type: research)
4. **Update index-features.md** with F416-F424
5. **Verify sub-feature quality** (Philosophy, 負債ゼロ AC, 等価性検証 AC)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Feature mapping documented | file | Grep | contains | "Phase 8 Feature Mapping:" | [x] |
| 2 | F416 created (ExpressionParser) | file | Glob | exists | feature-416.md | [x] |
| 3 | F417 created (Operators) | file | Glob | exists | feature-417.md | [x] |
| 4 | F418 created (Functions Core) | file | Glob | exists | feature-418.md | [x] |
| 5 | F419 created (Functions Data) | file | Glob | exists | feature-419.md | [x] |
| 6 | F420 created (Functions Game) | file | Glob | exists | feature-420.md | [x] |
| 7 | F421 created (Call Mechanism) | file | Glob | exists | feature-421.md | [x] |
| 8 | F422 created (Type Conversion) | file | Glob | exists | feature-422.md | [x] |
| 9 | F423 created (Post-Phase Review) | file | Glob | exists | feature-423.md | [x] |
| 10 | F424 created (Phase 9 Planning) | file | Glob | exists | feature-424.md | [x] |
| 11 | index-features.md updated | file | Grep | contains | "| 416 |" | [x] |
| 12 | All sub-features have Philosophy | file | Grep | contains | "Expression & Function" | [x] |
| 13 | All sub-features have 負債ゼロ AC | file | Grep | contains | "not_contains.*TODO" | [x] |
| 14 | All sub-features have 等価性検証 AC | file | Grep | contains | "equivalence" | [x] |

### AC Details

**AC#1**: Grep for "Phase 8 Feature Mapping:" in feature-409.md Execution Log

**AC#2-8**: Each implementation sub-feature file exists (F416-F422)
- F416: ExpressionParser Migration (AST generation)
- F417: Operator Implementation (30+ operators)
- F418: Built-in Functions Core (Math/Random/Conversion)
- F419: Built-in Functions Data (String/Array)
- F420: Built-in Functions Game (Character/System)
- F421: Function Call Mechanism
- F422: Type Conversion & Casting

**AC#9-10**: Transition features exist
- F423: Phase 8 Post-Phase Review (type: infra)
- F424: Phase 9 Planning (type: research)

**AC#11**: Grep for "| 416 |" in index-features.md to verify Phase 8 features added

**AC#12**: Grep for "Expression & Function" in Philosophy section of each F416-F422

**AC#13**: Grep for "not_contains.*TODO" in AC tables of F416-F422. Each engine sub-feature MUST have an AC verifying no TODO/FIXME remains after implementation.

**AC#14**: Grep for "equivalence" in AC tables of F416-F422. Each engine sub-feature MUST have an AC verifying behavior matches legacy ERB implementation.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Document "Phase 8 Feature Mapping:" in Execution Log | [x] |
| 2 | 2 | Create F416: ExpressionParser Migration | [x] |
| 3 | 3 | Create F417: Operator Implementation | [x] |
| 4 | 4 | Create F418: Built-in Functions Core (Math/Random/Conversion) | [x] |
| 5 | 5 | Create F419: Built-in Functions Data (String/Array) | [x] |
| 6 | 6 | Create F420: Built-in Functions Game (Character/System) | [x] |
| 7 | 7 | Create F421: Function Call Mechanism | [x] |
| 8 | 8 | Create F422: Type Conversion & Casting | [x] |
| 9 | 9 | Create F423: Phase 8 Post-Phase Review (type: infra) | [x] |
| 10 | 10 | Create F424: Phase 9 Planning (type: research) | [x] |
| 11 | 11 | Update index-features.md with Phase 8 features | [x] |
| 12 | 12 | Verify all sub-features have "Expression & Function" in Philosophy | [x] |
| 13 | 13 | Verify all sub-features have 負債ゼロ AC (TODO/FIXME not_contains) | [x] |
| 14 | 14 | Verify all sub-features have 等価性検証 AC (legacy/ERB equivalence) | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Sub-Feature Decomposition Strategy

**Rationale**: Phase 8 involves 30+ operators and 100+ functions. Per feature-template.md, engine type features should be ~300 lines with 8-15 ACs. Functions are split by responsibility category.

| Feature | Scope | Estimated Size |
|---------|-------|----------------|
| F416 | ExpressionParser (AST generation) | ~500 lines |
| F417 | All 30+ Operators (Arithmetic/Comparison/Logical/Bitwise/String/Ternary) | ~400 lines |
| F418 | Math(15)/Random(5)/Conversion(10) = 30 functions | ~300 lines |
| F419 | String(20)/Array(15) = 35 functions | ~350 lines |
| F420 | Character(20)/System(5) = 25 functions | ~250 lines |
| F421 | Function Registry, Dispatch, Context | ~200 lines |
| F422 | Type Conversion, Casting System | ~200 lines |

**Function Grouping Rationale** (per architecture.md "カテゴリが異なる責務なら分割"):
- **F418 Core**: Stateless computation (Math, Random, Conversion)
- **F419 Data**: Data manipulation (String, Array)
- **F420 Game**: Game state access (Character, System)

### Sub-Feature Creation Checklist

Each sub-feature created by F409 MUST include the following per architecture.md Sub-Feature Requirements:

| # | Requirement | Verification |
|:-:|-------------|--------------|
| 1 | **Philosophy inheritance** - "Expression & Function System" in Philosophy section | AC#12 (Grep) |
| 2 | **負債解消 Task** - Task to delete TODO/FIXME/HACK comments | Manual check during creation |
| 3 | **等価性検証 AC** - AC verifying legacy implementation equivalence | AC#14 (Grep "equivalence") |
| 4 | **負債ゼロ AC** - AC verifying zero technical debt (not_contains TODO) | AC#13 (Grep "not_contains.*TODO") |

**Execution Order**:
1. Create sub-feature file with all 4 requirements
2. Verify AC#12-14 pass for the created file
3. Mark corresponding Task complete
4. Repeat for all sub-features

---

## Phase 8 Scope Reference

**Snapshot from architecture.md (2026-01-08)**. See [full-csharp-architecture.md](designs/full-csharp-architecture.md) for authoritative version.

**Phase 8: Expression & Function System**

**Goal**: 式評価と組み込み関数の移行

**Tasks** → **Feature Mapping**:

| Task | Feature | Scope |
|------|:-------:|-------|
| 1. ExpressionParser 移行（AST 生成） | F416 | AST generation |
| 2. 演算子実装（30+ 演算子） | F417 | All operators (Arithmetic/Comparison/Logical/Bitwise/String/Ternary) |
| 3. 組み込み関数実装（100+ 関数） | F418-F420 | Split by responsibility |
| ├─ Math/Random/Conversion | F418 | Stateless computation |
| ├─ String/Array | F419 | Data manipulation |
| └─ Character/System | F420 | Game state access |
| 4. 関数呼び出しメカニズム | F421 | Registry, Dispatch, Context |
| 5. 型変換・キャスト | F422 | Casting system |
| 6. Post-Phase Review (type: infra) | F423 | Review F416-F422 |
| 7. Phase 9 Planning (type: research) | F424 | Next phase sub-features |

**Success Criteria**:
- [ ] ExpressionParser migration complete
- [ ] All 30+ operators implemented with Result types
- [ ] All 100+ built-in functions migrated
- [ ] Function call mechanism working
- [ ] Type conversion system functional
- [ ] Expression evaluation tests passing

**Design Requirements**: See [F377 Design Principles](feature-377.md#design-principles) (static class禁止, Strongly Typed ID, Result型)

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F408 | Phase 7 Post-Phase Review must pass first |
| Related | F377 | Phase 4 Design Principles (YAGNI/KISS, static class禁止, Result型) |
| Successor | F416-F422 | Implementation sub-features (created by this feature) |
| Successor | F423 | Phase 8 Post-Phase Review (created by this feature) |
| Successor | F424 | Phase 9 Planning (created by this feature) |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 8 definition
- [feature-408.md](feature-408.md) - Phase 7 Post-Phase Review (dependency)
- [feature-377.md](feature-377.md) - Phase 4 Design Principles (reference)
- [feature-398.md](feature-398.md) - Phase 7 Planning (predecessor pattern)
- [feature-template.md](reference/feature-template.md) - Granularity guidelines
- [feature-416.md](feature-416.md) - F416 ExpressionParser Migration (to be created)
- [feature-417.md](feature-417.md) - F417 Operator Implementation (to be created)
- [feature-418.md](feature-418.md) - F418 Functions Core (to be created)
- [feature-419.md](feature-419.md) - F419 Functions Data (to be created)
- [feature-420.md](feature-420.md) - F420 Functions Game (to be created)
- [feature-421.md](feature-421.md) - F421 Function Call Mechanism (to be created)
- [feature-422.md](feature-422.md) - F422 Type Conversion (to be created)
- [feature-423.md](feature-423.md) - F423 Phase 8 Post-Phase Review (to be created)
- [feature-424.md](feature-424.md) - F424 Phase 9 Planning (to be created)

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-09 FL iter2**: AC count (6) exceeds research type guideline (3-5). Justified by 5+ sub-features + 2 transition features + documentation ACs. Follows F398 precedent.
- **2026-01-09 FL iter3**: AC#6 verifies Philosophy inheritance ("Expression & Function"). Other Sub-Feature Requirements (負債解消 tasks, legacy 等価性検証 AC, 負債ゼロ AC) are validated during sub-feature FL review, not via F409 ACs.
- **2026-01-09 Comprehensive Review Fix**: Major restructure based on Phase philosophy/task/technical debt/maintainability review:
  - AC#2 changed from generic count (>=5) to explicit file checks (F416-F424) per F398 pattern
  - Tasks split to follow AC:Task 1:1 rule (14 Tasks matching 14 ACs)
  - Added Sub-Feature Decomposition Strategy with function grouping rationale
  - Added AC#13 (負債ゼロ verification) and AC#14 (等価性検証) to verify sub-feature requirements within F409
  - Removed FL review delegation for critical requirements - now verified by F409 ACs directly
- **2026-01-09 Validator Fix**: AC#13/AC#14 patterns corrected after parallel validation:
  - AC#13: `"TODO.*not_contains|FIXME.*not_contains"` → `"not_contains.*TODO"` (removed pipe alternation)
  - AC#14: `"legacy equivalence|ERB equivalence"` → `"equivalence"` (simplified to single pattern)
  - Validation: AC Structure ✅, AC:Task 1:1 ✅, Sub-Feature Requirements ✅

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-08 | create | implementer | Created as Phase 8 planning feature per mandatory transition | PROPOSED |
| 2026-01-09 | START | opus | Phase 4 Implementation | - |
| 2026-01-09 | mapping | opus | Phase 8 Feature Mapping: ExpressionParser → F416, Operators → F417, Functions Core (Math/Random/Conversion) → F418, Functions Data (String/Array) → F419, Functions Game (Character/System) → F420, Function Call Mechanism → F421, Type Conversion → F422, Post-Phase Review → F423, Phase 9 Planning → F424 | Task 1 |
| 2026-01-09 13:16 | create | implementer | Created F421: Function Call Mechanism (FunctionRegistry, IFunctionRegistry, IBuiltInFunction, IEvaluationContext) | Task 7 DONE |
| 2026-01-09 13:16 | create | implementer | Created F422: Type Conversion & Casting (TypeConverter, ITypeConverter, Result type conversions) | Task 8 DONE |
| 2026-01-09 13:16 | create | implementer | Created F423: Phase 8 Post-Phase Review (type: infra) following F408 template | Task 9 DONE |
| 2026-01-09 13:17 | create | implementer | Created F424: Phase 9 Planning (type: research) - CommandDispatcher + Mediator Pipeline, 60+ commands + 16 SCOMF, 7 implementation sub-features (F425-F431) + 2 transition features (F432-F433) | Task 10 DONE |
| 2026-01-09 13:18 | create | implementer | Created F418: Built-in Functions Core (Math/Random/Conversion) - 30+ stateless functions (ABS/MAX/MIN/POWER/SQRT/LOG/RAND/TOSTR/TOINT/ISNUMERIC), ~300 lines across 3 files | Task 4 DONE |
| 2026-01-09 13:18 | create | implementer | Created F416: ExpressionParser Migration (AST generation, ~500 lines) | Task 2 DONE |
| 2026-01-09 13:18 | create | implementer | Created F417: Operator Implementation (30+ operators, ~400 lines) | Task 3 DONE |
| 2026-01-09 13:18 | create | implementer | Created F419: Built-in Functions Data (String/Array, ~350 lines) | Task 5 DONE |
| 2026-01-09 13:18 | create | implementer | Created F420: Built-in Functions Game (Character/System, ~250 lines) | Task 6 DONE |
| 2026-01-09 | update | opus | Updated index-features.md with F416-F424, Next Feature number → 425 | Task 11 DONE |
| 2026-01-09 | verify | opus | Verified all F416-F422 have "Expression & Function" Philosophy (7/7) | Task 12 DONE |
| 2026-01-09 | verify | opus | Verified all F416-F422 have 負債ゼロ AC (not_contains TODO) (7/7) | Task 13 DONE |
| 2026-01-09 | verify | opus | Verified all F416-F422 have 等価性検証 AC (equivalence) (7/7) | Task 14 DONE |
| 2026-01-09 | END | opus | Phase 4 Implementation - All 14 tasks complete | SUCCESS |
| 2026-01-09 | review | feature-reviewer | Post-review (mode: post) | READY |
| 2026-01-09 | review | feature-reviewer | Doc-check - planning-validator.md untracked (pre-existing, out-of-scope) | READY |
| 2026-01-09 | verify | opus | SSOT Update Check - No F409-specific SSOT updates required (research type creates docs only) | PASS |
