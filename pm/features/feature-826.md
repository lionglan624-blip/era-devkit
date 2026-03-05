# Feature 826: Post-Phase Review Phase 22

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

## Type: infra

## Background

### Philosophy (Mid-term Vision)

Phase 22: State Systems — Post-phase review ensures all Phase 22 subsystem migrations are complete, quality gates pass, and carry-forward obligations are triaged for Phase 23.

### Problem (Current Issue)

Phase 22 implementation sub-features (F819, F821-F825) must be reviewed holistically after individual completion. CP-2 Step 2c E2E checkpoint verifies cross-subsystem integration. Deferred obligation #1 (N+4 --unit deprecation) requires re-triage. Obligation #32 (ERB file boundary != domain boundary) requires /fc verification step addition.

**Deferred from F819**: SANTA cosplay text output (CLOTHE_EFFECT.ERB @SANTA function PRINT calls) is a UI-layer concern and cannot be migrated to Era.Core (no UI primitives). Must be handled by the engine layer.

**Deferred from F824**: Two cross-cutting concerns discovered during Sleep & Menstrual migration that don't belong to any specific Phase 22 sub-feature:
1. **CVARSET bulk reset**: `CVARSET CFLAG, 317` pattern used in both 睡眠深度.ERB and MOVEMENT.ERB:81. F824 inlined in `SleepDepth.ResetUfufu()` (1 C# call site). When MOVEMENT.ERB migrates, extract `IVariableStore.BulkResetCharacterFlags()` as shared method.
2. **IS_DOUTEI shared utility**: Defined in COMMON.ERB, called from 14 ERB files (22 call sites). F824 inlined in `MenstrualCycle.FormatSimpleStatus` (1 C# call site). When second C# call site appears, extract to `ICharacterUtilities.IsDoutei`.

### Goal (What to Achieve)

Execute CP-2 Step 2c E2E checkpoint for Phase 22 completion. Re-triage obligation #1 (N+4 --unit deprecation). Implement obligation #32 (ERB file boundary != domain boundary /fc verification step).

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning |
| Predecessor | F819 | [DONE] | Clothing System |
| Predecessor | F821 | [DONE] | Weather System |
| Predecessor | F822 | [DRAFT] | Pregnancy System |
| Predecessor | F823 | [DONE] | Room & Stain |
| Predecessor | F824 | [DONE] | Sleep & Menstrual |
| Predecessor | F825 | [DRAFT] | Relationships & DI Integration |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | - | Execute CP-2 Step 2c E2E checkpoint: verify all Phase 22 subsystem E2E tests pass and cross-subsystem integration is verified | | [ ] |
| 2 | - | Re-triage obligation #1 (N+4 --unit deprecation NOT_FEASIBLE re-deferral): evaluate test framework status and decide carry-forward or resolution | | [ ] |
| 3 | - | Implement obligation #32 (ERB file boundary != domain boundary /fc verification step): add verification step to /fc workflow | | [ ] |

---

## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning
- [Predecessor: F819](feature-819.md) - Clothing System
- [Predecessor: F821](feature-821.md) - Weather System
- [Predecessor: F822](feature-822.md) - Pregnancy System
- [Predecessor: F823](feature-823.md) - Room & Stain
- [Predecessor: F824](feature-824.md) - Sleep & Menstrual
- [Predecessor: F825](feature-825.md) - Relationships & DI Integration
