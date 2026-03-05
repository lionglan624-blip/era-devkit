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

**Deferred from F823**: ROOM_SMELL_WHOSE_SAMEN function (ROOM_SMELL.ERB:1108-1140) was not migrated because I3DArrayVariables lacks GetDa/SetDa for DA variable access. This DA interface gap is a cross-cutting concern that must be resolved before WHOSE_SAMEN can be implemented. See F823 Mandatory Handoffs and AC#13.

**Deferred from F824**: DI registration of ISleepDepth/SleepDepth, IMenstrualCycle/MenstrualCycle, IHeartbreakService/HeartbreakService is done in F824 (own services only). F825 owns the full Phase 22 DI integration batch including relationship services and cross-subsystem wiring.

**Deferred from F819**: (1) CLOTHES_ACCESSORY full implementation requires NTR_CHK_FAVORABLY via INtrQuery — current NullNtrQuery returns false as safe default. (2) Remaining 服装_* CFLAG backup slot indices not confirmed in CFLAG.yaml — Save/Load only covers 5 of ~19 backup pairs; remaining slots must be resolved from full CFLAG.yaml scan.

**Deferred from F821**: IEngineVariables indexed methods (GetDay(int)/SetDay(int,int)/GetTime(int)/SetTime(int,int)) were added as default interface methods with no-op stubs. Engine repo implementation must override these with real DAY/TIME array access after Era.Core NuGet bump.

### Goal (What to Achieve)

Migrate 続柄.ERB to C#, complete all deferred Null implementations from Phase 21, perform IComHandler DI registration, consolidate IComableUtilities/ICounterUtilities, and wire all Phase 22 subsystem interfaces into the DI container. CP-2 E2E checkpoint covers DI integration and cross-subsystem wiring.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning |
| Predecessor | F819 | [DONE] | Clothing System -- DI integration requires all subsystem interfaces |
| Predecessor | F821 | [DONE] | Weather System -- DI integration requires all subsystem interfaces |
| Predecessor | F822 | [DONE] | Pregnancy System -- DI integration requires all subsystem interfaces |
| Predecessor | F823 | [DONE] | Room & Stain System -- DI integration requires all subsystem interfaces |
| Predecessor | F824 | [DONE] | Sleep & Menstrual System -- DI integration requires all subsystem interfaces |

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
