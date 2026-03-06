# Feature 410: VirginityManager Local Constant Consolidation

## Status: [DONE]

## Phase: 7 (Technical Debt Consolidation)

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

## Type: engine

## Created: 2026-01-08

---

## Summary

VirginityManager.cs currently has 4 remaining local constants (TalentAffection, TalentChastity, SourceLove, SourceAntipathy) that should use centralized Types. This feature consolidates them by adding missing TalentIndex well-known values and replacing all local constants with centralized references.

---

## Background

### Philosophy (Mid-term Vision)

**Full C# Architecture**: Type safety enables compile-time checking, self-documenting code, and safer refactoring. Centralized well-known values ensure consistency across the codebase.

### Problem (Current Issue)

F403 completed the primary StateChange migration but left 4 local constants in VirginityManager.cs:

| Constant | Current Location | Value | Should Be |
|----------|------------------|-------|-----------|
| TalentAffection | VirginityManager line 19 | new(3) | TalentIndex.Affection |
| TalentChastity | VirginityManager line 20 | new(30) | TalentIndex.Chastity |
| SourceLove | VirginityManager line 23 | new(10) | Already exists: SourceIndex.Love |
| SourceAntipathy | VirginityManager line 24 | new(33) | Already exists: SourceIndex.Antipathy |

Note: SourceLove and SourceAntipathy already exist in SourceIndex (F399), so they just need replacement.

### Goal (What to Achieve)

1. Add TalentIndex.Affection = new(3) to centralized Types
2. Add TalentIndex.Chastity = new(30) to centralized Types
3. Replace VirginityManager local constants with centralized Types
4. Remove all local TalentIndex/SourceIndex constants from VirginityManager
5. Update SSOT documentation

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | TalentIndex has Affection | code | Grep TalentIndex.cs | contains | Affection = new(3) | [x] |
| 2 | TalentIndex has Chastity | code | Grep TalentIndex.cs | contains | Chastity = new(30) | [x] |
| 3 | VirginityManager no local TalentIndex | code | Grep VirginityManager.cs | not_contains | private static readonly TalentIndex | [x] |
| 4 | VirginityManager no local SourceIndex | code | Grep VirginityManager.cs | not_contains | private static readonly SourceIndex | [x] |
| 5 | VirginityManager uses TalentIndex.Affection | code | Grep VirginityManager.cs | contains | TalentIndex.Affection | [x] |
| 6 | VirginityManager uses TalentIndex.Chastity | code | Grep VirginityManager.cs | contains | TalentIndex.Chastity | [x] |
| 7 | VirginityManager uses SourceIndex.Love | code | Grep VirginityManager.cs | contains | SourceIndex.Love | [x] |
| 8 | VirginityManager uses SourceIndex.Antipathy | code | Grep VirginityManager.cs | contains | SourceIndex.Antipathy | [x] |
| 9 | C# build succeeds | build | dotnet | succeeds | - | [x] |
| 10 | All Training tests pass | test | dotnet | succeeds | Category=Training | [x] |
| 11 | SSOT updated (TalentIndex.Affection) | code | Grep engine-dev/SKILL.md | contains | TalentIndex.Affection | [x] |
| 12 | SSOT updated (TalentIndex.Chastity) | code | Grep engine-dev/SKILL.md | contains | TalentIndex.Chastity | [x] |

### AC Details

**AC#1-2**: Add missing well-known values to TalentIndex.cs (from Talent.csv):
- TalentIndex.Affection = new(3) for 恋慕 (line 7: "3,恋慕")
- TalentIndex.Chastity = new(30) for 貞操 (line 47: "30,貞操")

**AC#3-4**: Remove all local constants from VirginityManager.cs:
- Remove `private static readonly TalentIndex TalentAffection = new(3);`
- Remove `private static readonly TalentIndex TalentChastity = new(30);`
- Remove `private static readonly SourceIndex SourceLove = new(10);` (use SourceIndex.Love)
- Remove `private static readonly SourceIndex SourceAntipathy = new(33);` (use SourceIndex.Antipathy)

**AC#5-8**: Update VirginityManager to use centralized Types:
- Replace all `TalentAffection` with `TalentIndex.Affection`
- Replace all `TalentChastity` with `TalentIndex.Chastity`
- Replace all `SourceLove` with `SourceIndex.Love`
- Replace all `SourceAntipathy` with `SourceIndex.Antipathy` (verify F403 migration and complete any remaining usages)

**AC#9-10**: Verify no regression.

**AC#11-12**: Update engine-dev SKILL.md with new TalentIndex well-known values (both Affection and Chastity).

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-2 | Add TalentIndex.Affection(3), TalentIndex.Chastity(30) to centralized Types | [x] |
| 2 | 3-8 | Replace VirginityManager local constants with centralized Types | [x] |
| 3 | 9-10 | Verify build and tests pass | [x] |
| 4 | 11-12 | Update engine-dev SKILL.md with TalentIndex values | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F403 | Parent feature (created this as 残課題) |
| Predecessor | F399 | Provides SourceIndex.Love, SourceIndex.Antipathy |

---

## Links

- [feature-403.md](feature-403.md) - Parent feature (F403 残課題 source)
- [feature-399.md](feature-399.md) - F399 SourceIndex well-known values
- [feature-413.md](feature-413.md) - Follow-up: AbilityGrowthProcessor consolidation
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 7 definition

**Out-of-Scope Note**: AbilityGrowthProcessor.cs has 7 similar local TalentIndex constants (lines 49-55). → Tracked in F413.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-08 | create | opus | Follow-up from F403 残課題 | PROPOSED |
| 2026-01-08 22:20 | START | implementer | Task 1 | - |
| 2026-01-08 22:20 | END | implementer | Task 1 | SUCCESS |
| 2026-01-09 | AC-TEST | ac-tester | All 12 ACs | PASS |
| 2026-01-09 | FINALIZE | finalizer | Status update to DONE | SUCCESS |
