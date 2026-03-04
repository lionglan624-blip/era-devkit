# Feature 819: Clothing System Migration (Merged)

## Status: [DRAFT]

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

## Type: erb

## Background

### Philosophy (Mid-term Vision)

Phase 22: State Systems -- Clothing subsystem migration covering base clothing presets, effects, clothing system menu, and cosplay presets (CLOTHES.ERB, CLOTHE_EFFECT.ERB, CLOTHES_SYSTEM.ERB, CLOTHES_Cosplay.ERB). Absorbs former F820 scope (Roslynator investigation, CLOTHES_SYSTEM.ERB, CLOTHES_Cosplay.ERB) due to F819+F820 merge.

### Problem (Current Issue)

Four ERB files totalling 3,665 lines implement the clothing subsystem: CLOTHES.ERB (1999 lines), CLOTHE_EFFECT.ERB (285 lines), CLOTHES_SYSTEM.ERB (964 lines), CLOTHES_Cosplay.ERB (417 lines). ClothingSystem.cs exists as a Phase 3 stub (628 lines) requiring completion. IKnickersSystem was explicitly deferred to Phase 22 from F811 and requires full implementation.

### Goal (What to Achieve)

Migrate all four clothing ERB files to C#, complete ClothingSystem.cs Phase 3 stubs, implement IKnickersSystem, NullKnickersSystem, CFlagIndex typed struct, EquipIndex typed struct, and perform Roslynator investigation. Achieve zero-debt implementation with equivalence tests against ERB baseline.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | (stub - to be completed by /fc) | | | | | [ ] |
| 2 | No TODO/FIXME/HACK comments remain in Clothing implementation (zero debt) | file | Grep | matches | No matches | [ ] |
| 3 | CP-2 E2E checkpoint: Clothing integration verified | test | dotnet test | pass | All pass | [ ] |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | - | Remove TODO/FIXME/HACK comments from migrated code | | [ ] |
| 2 | - | Write equivalence tests against ERB baseline | | [ ] |
| 3 | - | IKnickersSystem full implementation (obligation C9) | | [ ] |
| 4 | - | ClothingSystem.cs stub completion (obligation C13) | | [ ] |
| 5 | - | CFlagIndex typed struct implementation (obligation #20) | | [ ] |
| 6 | - | EquipIndex typed struct implementation (obligation #21) | | [ ] |
| 7 | - | NullKnickersSystem concrete implementation (obligation #28) | | [ ] |
| 8 | - | Roslynator investigation (obligation C8, absorbed from former F820) | | [ ] |

---

## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning

**Note**: F820 is 欠番 (skipped). F819 and F820 were merged into this single feature. F820 content (Roslynator, CLOTHES_SYSTEM.ERB, CLOTHES_Cosplay.ERB) is absorbed here.
