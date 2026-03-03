# Feature 814: Phase 22 Planning

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

## Type: research

## Summary

Feature to create Features: Phase 22 Planning. Decompose Phase 22 (Clothing/State Systems) into implementation sub-features following the F647/F783 planning pattern. Phase 22 must run alone per design-reference.md:537.

---

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity — Each phase completion triggers next phase planning. Phase 22 (Clothing) cannot run concurrently with other phases per design constraints.

### Problem (Current Issue)

Phase 21 (F783) decomposition used file-prefix grouping only, resulting in all sub-features having only F783 as Predecessor. Inter-feature call-chain dependencies (F803→F801, F805→F803/F804, F806-F808→F805, F810→F809, F811→F801/F812) were missing and required manual correction. Phase 22 decomposition MUST follow the "Sub-Feature Dependency Analysis" procedure in `full-csharp-architecture.md` to derive inter-feature Predecessors from CALL/TRYCALL/CALLFORM analysis at DRAFT creation time.

---

## Deferred Obligations from F813

The following 35 obligations were transferred from F813 (Post-Phase Review Phase 21) and must be addressed during F814 planning or implementation:

| # | Issue | Origin |
|:-:|-------|--------|
| 1 | N+4 --unit deprecation NOT_FEASIBLE re-deferral | F782→F783→F813 |
| 2 | IShrinkageSystem runtime implementation | F803 |
| 3 | IEngineVariables GetTime/SetTime behavioral override | F806 |
| 4 | WcCounterMessageSex constructor complexity reduction (16 deps) | F806 |
| 5 | CFlag/Cflag naming normalization | F807 |
| 6 | WC_VA_FITTING caller documentation | F806 |
| 7 | IComHandler DI registration strategy | F811 |
| 8 | WcCounterMessage constructor bloat (12 params) | F807 |
| 9 | WcCounterMessageTease behavioral test coverage | F807 |
| 10 | NOITEM photography bug (ERB original bug) | F806 |
| 11 | KOJO 3-param overload verification | F806 |
| 12 | WcCounterMessageSex duplicate constant names | F806 |
| 13 | Dispatch() dual offender convention unification | F806 |
| 14 | IWcCounterMessageTease interface extraction | F807 |
| 15 | Character ID constant consolidation | F807 |
| 16 | F807 AC#34 local function enforcement gap | F807 |
| 17 | ICharacterStringVariables VariableStore implementation | F803 |
| 18 | EXP_UP logic duplication | F803 |
| 19 | ICounterSourceHandler ISP violation | F803 |
| 20 | CFlagIndex typed struct | F803 |
| 21 | EquipIndex typed struct | F803 |
| 22 | IComableUtilities/ICounterUtilities TimeProgress/IsAirMaster/GetTargetNum consolidation | F810 |
| 23 | NtrReversalSource/NtrAgreeSource REGRESSION fix if found | F813 Task 14 |
| 24 | NullCounterUtilities concrete implementation | F801/F804 |
| 25 | NullWcSexHaraService concrete implementation | F811 |
| 26 | NullNtrUtilityService concrete implementation | F811 |
| 27 | NullTrainingCheckService concrete implementation | F811 |
| 28 | NullKnickersSystem concrete implementation | F811 |
| 29 | NullEjaculationProcessor concrete implementation | F811 |
| 30 | NullKojoMessageService concrete implementation | F811 |
| 31 | RotorOut IWcCounterMessageItem migration consideration | F813 Task 13 |
| 32 | ERB file boundary != domain boundary /fc verification step | F808 |
| 33 | NullComHandler concrete implementation | F811 |
| 34 | IEngineVariables GetTime/SetTime NuGet version bump | F806 |
| 35 | WcCounterMessageNtr class-level split | F813 |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (Mandatory Handoff origin) |
| Predecessor | F813 | [DONE] | Post-Phase Review Phase 21 |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | - | Push all commits to remote | | [ ] |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|

---

## Links

- [Predecessor: F813](feature-813.md) - Post-Phase Review Phase 21
