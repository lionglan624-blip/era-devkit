# Feature 398: Phase 7 Planning

## Status: [DONE]

## Type: research

## Created: 2026-01-07

---

## Summary

**Feature to create Features**: Create Phase 7 sub-features from full-csharp-architecture.md.

Analyze architecture.md Phase 7 (Technical Debt Consolidation) and create implementation sub-features.

**Output**: New Feature files (feature-{ID}.md) as primary deliverables.

---

## Background

### Philosophy (Mid-term Vision)

**Pipeline Continuity**: Each phase completion triggers next phase planning. This ensures:
- Continuous development pipeline
- Clear phase boundaries
- Documented transition points

### Problem (Current Issue)

Phase 6 completion requires Phase 7 planning to maintain momentum:
- Phase 7 scope must be defined from architecture.md
- Sub-features must follow granularity rules (5-12 ACs for engine type per feature-template.md)
- Dependencies must be documented
- **Transition features must be created** (Post-Phase Review + Next Phase Planning)

### Goal (What to Achieve)

1. **Analyze Phase 7** requirements from full-csharp-architecture.md
2. **Create sub-features** for Phase 7 Tasks:
   - F404: IVariableStore ISP Segregation
   - F405: Callback DI Formalization
   - F406: Equipment/OrgasmProcessor Completion
   - F407: Training Integration Tests
3. **Update index-features.md** with new features
4. **Document dependencies** between Phase 7 features
5. **Create transition features** (F408: Post-Phase Review + F409: Phase 8 Planning)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Feature mapping documented | file | Grep | contains | "Phase 7 Feature Mapping:" | [x] |
| 2 | F404 created (ISP) | file | Glob | exists | feature-404.md | [x] |
| 3 | F405 created (Callback DI) | file | Glob | exists | feature-405.md | [x] |
| 4 | F406 created (Processor Completion) | file | Glob | exists | feature-406.md | [x] |
| 5 | F407 created (Integration Tests) | file | Glob | exists | feature-407.md | [x] |
| 6 | F408 created (Post-Phase Review) | file | Glob | exists | feature-408.md | [x] |
| 7 | F409 created (Phase 8 Planning) | file | Glob | exists | feature-409.md | [x] |
| 8 | index-features.md updated | file | Grep | contains | "| 404 |" | [x] |

### AC Details

**AC#1**: Grep for "Phase 7 Feature Mapping:" in feature-398.md Execution Log (file path: Game/agents/feature-398.md)
**AC#2-7**: Each Phase 7 sub-feature file exists (F402, F403 already exist, so excluded). Verify each file individually: `feature-404.md`, `feature-405.md`, `feature-406.md`, `feature-407.md`, `feature-408.md`, `feature-409.md`
**AC#8**: Grep for "| 404 |" in index-features.md to verify F404 entry was added (file path: Game/agents/index-features.md)

**Note**: AC count verification (>= 5 ACs per created feature) is enforced during feature creation per feature-template.md granularity rules.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Document "Phase 7 Feature Mapping:" in Execution Log | [x] |
| 2 | 2 | Create F404: IVariableStore ISP Segregation | [x] |
| 3 | 3 | Create F405: Callback DI Formalization | [x] |
| 4 | 4 | Create F406: Equipment/OrgasmProcessor Completion | [x] |
| 5 | 5 | Create F407: Training Integration Tests | [x] |
| 6 | 6 | Create F408: Phase 7 Post-Phase Review (type: infra) | [x] |
| 7 | 7 | Create F409: Phase 8 Planning (type: research) | [x] |
| 8 | 8 | Update index-features.md with Phase 7 features | [x] |

---

## Phase 7 Scope Reference

**Snapshot from architecture.md (2026-01-08)**. See [full-csharp-architecture.md](designs/full-csharp-architecture.md) for authoritative version.

From full-csharp-architecture.md:

**Phase 7: Technical Debt Consolidation (NEW)**

**Goal**: Phase 5-6で蓄積した技術負債の解消 + Phase 8以降の基盤確立

**Tasks**:
1. IVariableStore Interface Segregation（ISP準拠分割）→ F404
2. Callback Injection DI Formalization（Factory登録）→ F405
3. StateChange Hierarchy Completion:
   - F402: Equipment/Orgasm Migration (existing, Phase 7, DONE)
   - F403: Character Namespace StateChange Migration (existing)
4. Equipment/OrgasmProcessor 完成 → F406
5. Training Integration Tests 追加 → F407
6. Create Phase 7 Post-Phase Review feature (type: infra) → F408
7. Create Phase 8 Planning feature (type: research) → F409

**Deliverables**:
| Component | Responsibility | Feature |
|-----------|----------------|:-------:|
| `Era.Core/Variables/ITrainingVariables.cs` | Training変数インターフェース | F404 |
| `Era.Core/Variables/ICharacterStateVariables.cs` | State変数インターフェース | F404 |
| `Era.Core/Variables/IJuelVariables.cs` | Juel変数インターフェース | F404 |
| `Era.Core/DependencyInjection/CallbackFactories.cs` | Callback DI登録 | F405 |
| `Era.Core/Training/StateChange.cs` | Training namespace完成 | F402 |
| `Era.Core/Character/*.cs` | Character namespace StateChange | F403 |
| `Era.Core/Training/EquipmentProcessor.cs` | 装備処理完成 | F406 |
| `Era.Core/Training/OrgasmProcessor.cs` | 絶頂処理完成 | F406 |
| `Era.Core.Tests/Training/TrainingIntegrationTests.cs` | 統合テスト | F407 |

**Success Criteria** (from architecture.md):
- [ ] IVariableStore が ISP 準拠（4インターフェース分割）
- [ ] Callback injection が DI 正式登録
- [ ] StateChange 13サブタイプ完成（Training + Character）
- [ ] LegacyStateChange 完全削除
- [ ] OrgasmProcessor/EquipmentProcessor 100% 実装
- [ ] Training 統合テスト追加

**Existing Features**: F402 (Equipment/Orgasm Migration) and F403 (Character Namespace StateChange Migration) already created.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F397 | Phase 6 Post-Phase Review must pass first |
| Related | F402 | StateChange Equipment/Orgasm Migration (Phase 7, DONE) - part of Phase 7 scope |
| Related | F403 | Character Namespace StateChange Migration (already created) - part of Phase 7 scope |
| Successor | F404 | IVariableStore ISP分割 (created by this feature) |
| Successor | F405 | Callback DI正式化 (created by this feature) |
| Successor | F406 | Equipment/OrgasmProcessor完成 (created by this feature) |
| Successor | F407 | Training Integration Tests (created by this feature) |
| Successor | F408 | Phase 7 Post-Phase Review (created by this feature) |
| Successor | F409 | Phase 8 Planning (created by this feature) |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 7 definition
- [feature-397.md](feature-397.md) - Phase 6 Post-Phase Review
- [feature-402.md](feature-402.md) - F402 StateChange Equipment/Orgasm Migration (Phase 7, DONE)
- [feature-403.md](feature-403.md) - F403 Character Namespace StateChange Migration (existing)
- [feature-390.md](feature-390.md) - Phase 6 Planning (predecessor pattern)
- [feature-template.md](reference/feature-template.md) - Granularity guidelines
- [feature-404.md](feature-404.md) - F404 ISP Segregation (created)
- [feature-405.md](feature-405.md) - F405 Callback DI (created)
- [feature-406.md](feature-406.md) - F406 Processor Completion (created)
- [feature-407.md](feature-407.md) - F407 Integration Tests (created)
- [feature-408.md](feature-408.md) - F408 Post-Phase Review (created)
- [feature-409.md](feature-409.md) - F409 Phase 8 Planning (created)

---

## Review Notes

- **2026-01-08 FL iter3**: AC count (8) exceeds research type guideline (3-5). Justified by 6 new feature files (F404-F409) + 2 documentation ACs (mapping log, index update).

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-07 | create | opus | Created as Phase 7 planning feature per mandatory transition | PROPOSED |
| 2026-01-08 | update | opus | Updated for new Phase 7 (Technical Debt Consolidation) per architecture revision #11 | PROPOSED |
| 2026-01-08 | mapping | implementer | Phase 7 Feature Mapping: IVariableStore ISP → F404, Callback DI → F405, Equipment/OrgasmProcessor → F406, Training Integration Tests → F407, Post-Phase Review → F408, Phase 8 Planning → F409 | - |
| 2026-01-08 | create | implementer | Created F408 (Phase 7 Post-Phase Review) with 12 ACs covering ISP compliance, DI formalization, StateChange hierarchy, legacy deletion, processor completion, integration tests | Task 6 complete |
| 2026-01-08 | create | implementer | Created F404 (IVariableStore ISP Segregation) with 14 ACs covering interface segregation (4 interfaces: IVariableStore core 12, ITrainingVariables 6, ICharacterStateVariables 6, IJuelVariables 6), GlobalStaticAdapter implementation, consumer refactoring, builds | Task 2 complete |
| 2026-01-08 | update | opus | Updated F404-F407 Philosophy with Phase 7 統一思想 (Technical Debt Consolidation) | - |
| 2026-01-08 | update | opus | Added F405 AC#13-14, Task 5-6 (ad-hoc callback migration); F406 AC#13-16, Task 9-12 (FIXME cleanup, ERB equivalence) | 技術負債ゼロ強化 |
| 2026-01-08 | update | opus | Added Sub-Feature Requirements to architecture.md Phase 7-29 (Checklist for Planning Feature) | ワークフロー改善 |
