# Feature 825: Relationships & DI Integration

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

Phase 22: State Systems -- Relationships subsystem migration and DI integration / cross-subsystem wiring. This is the final Phase 22 sub-feature, responsible for migrating 続柄.ERB and completing the DI registration of all Phase 22 subsystem interfaces, including all deferred Null implementations from Phase 21.

### Problem (Current Issue)

続柄.ERB (379 lines) implements the relationships subsystem. As the final Phase 22 sub-feature, F825 is also responsible for DI integration and cross-subsystem wiring: registering IComHandler, completing NullCounterUtilities, NullWcSexHaraService, NullNtrUtilityService, NullTrainingCheckService, NullEjaculationProcessor, NullKojoMessageService, and NullComHandler. IComableUtilities/ICounterUtilities consolidation (TimeProgress/IsAirMaster/GetTargetNum) is also assigned here. All prior Phase 22 subsystems (F819, F821, F822, F823, F824) must be complete before DI wiring can be finalized.

### Goal (What to Achieve)

Migrate 続柄.ERB to C#, complete all deferred Null implementations from Phase 21, perform IComHandler DI registration, consolidate IComableUtilities/ICounterUtilities, and wire all Phase 22 subsystem interfaces into the DI container. CP-2 E2E checkpoint covers DI integration and cross-subsystem wiring.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning |
| Predecessor | F819 | [DRAFT] | Clothing System -- DI integration requires all subsystem interfaces |
| Predecessor | F821 | [DRAFT] | Weather System -- DI integration requires all subsystem interfaces |
| Predecessor | F822 | [DRAFT] | Pregnancy System -- DI integration requires all subsystem interfaces |
| Predecessor | F823 | [DRAFT] | Room & Stain System -- DI integration requires all subsystem interfaces |
| Predecessor | F824 | [DRAFT] | Sleep & Menstrual System -- DI integration requires all subsystem interfaces |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | (stub - to be completed by /fc) | | | | | [ ] |
| 2 | No TODO/FIXME/HACK comments remain in Relationships & DI Integration implementation (zero debt) | file | Grep | matches | No matches | [ ] |
| 3 | CP-2 E2E checkpoint: DI integration and cross-subsystem wiring verified | test | dotnet test | pass | All pass | [ ] |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | - | Remove TODO/FIXME/HACK comments from migrated code | | [ ] |
| 2 | - | Write equivalence tests against ERB baseline | | [ ] |
| 3 | - | IComHandler DI registration strategy (obligation #7) | | [ ] |
| 4 | - | IComableUtilities/ICounterUtilities consolidation (TimeProgress/IsAirMaster/GetTargetNum) (obligation #22) | | [ ] |
| 5 | - | NullCounterUtilities concrete implementation (obligation #24) | | [ ] |
| 6 | - | NullWcSexHaraService concrete implementation (obligation #25) | | [ ] |
| 7 | - | NullNtrUtilityService concrete implementation (obligation #26, GetNWithVisitor returns default 0) | | [ ] |
| 8 | - | NullTrainingCheckService concrete implementation (obligation #27) | | [ ] |
| 9 | - | NullEjaculationProcessor concrete implementation (obligation #29) | | [ ] |
| 10 | - | NullKojoMessageService concrete implementation (obligation #30) | | [ ] |
| 11 | - | NullComHandler concrete implementation (obligation #33) | | [ ] |

---

## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning
- [Predecessor: F819](feature-819.md) - Clothing System
- [Predecessor: F821](feature-821.md) - Weather System
- [Predecessor: F822](feature-822.md) - Pregnancy System
- [Predecessor: F823](feature-823.md) - Room & Stain System
- [Predecessor: F824](feature-824.md) - Sleep & Menstrual System
