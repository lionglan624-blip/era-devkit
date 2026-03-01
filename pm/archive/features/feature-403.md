# Feature 403: Character Namespace StateChange Migration

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

Complete StateChange type safety migration for Era.Core/Character/ namespace. Migrate VirginityManager, VirginityState, and ExperienceState to typed StateChange. Extend ExpIndex for orgasm experience patterns. Delete LegacyStateChange after all migrations complete.

---

## Background

### Philosophy (Mid-term Vision)

**Full C# Architecture**: Type safety enables compile-time checking, self-documenting code, and safer refactoring.

### Problem (Current Issue)

F402 completed Training namespace StateChange migration but deferred Character namespace:

| File | Issue | Blocked By |
|------|-------|------------|
| VirginityManager.cs | TCVAR, TALENT, SOURCE, EXP string patterns | Out of F402 scope |
| VirginityState.cs | List<LegacyStateChange> | F402 focused on Training/ only |
| ExperienceState.cs | List<LegacyStateChange> | F402 focused on Training/ only |
| OrgasmProcessor.cs | EXP:* patterns in UpdateOrgasmExperience | Missing ExpIndex well-known values |
| LegacyStateChange.cs | Temporary struct in Era.Core/Types/ | Character namespace still using it |

### Goal (What to Achieve)

1. Migrate VirginityManager to typed StateChange (TCVarChange, TalentChange, SourceChange, ExpChange)
2. Migrate VirginityState to typed StateChange
3. Migrate ExperienceState to typed StateChange
4. Extend ExpIndex with orgasm experience well-known values
5. Migrate OrgasmProcessor.UpdateOrgasmExperience to ExpChange
6. Delete Era.Core/Types/LegacyStateChange.cs

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | VirginityManager no string AddChange | code | Grep VirginityManager.cs | not_contains | AddChange(" | [x] |
| 2 | VirginityState uses typed StateChange | code | Grep VirginityState.cs | not_contains | LegacyStateChange | [x] |
| 3 | ExperienceState uses typed StateChange | code | Grep ExperienceState.cs | not_contains | LegacyStateChange | [x] |
| 4 | ExpIndex has orgasm experience values | code | Grep ExpIndex.cs | contains | OrgasmExperience | [x] |
| 5 | OrgasmProcessor EXP uses ExpChange | code | Grep OrgasmProcessor.cs | contains | ExpChange( | [x] |
| 6 | LegacyStateChange deleted from Era.Core | code | Glob | not_exists | LegacyStateChange.cs | [x] |
| 7 | No LegacyStateChange references in Era.Core | code | Grep Era.Core/ | not_contains | LegacyStateChange | [x] |
| 8 | C# build succeeds | build | dotnet | succeeds | - | [x] |
| 9 | All Training tests pass | test | dotnet | succeeds | Category=Training | [x] |
| 10 | SSOT updated (ExpIndex values) | code | Grep SKILL.md | contains | OrgasmExperience | [x] |
| 11 | TCVarIndex has Defloration | code | Grep TCVarIndex.cs | contains | Defloration = new(15) | [x] |
| 12 | TalentIndex has Virginity | code | Grep TalentIndex.cs | contains | Virginity = new(0) | [x] |
| 13 | VirginityManager no local TCVarIndex | code | Grep VirginityManager.cs | not_contains | private static readonly TCVarIndex | [x] |

### AC Details

**AC#1**: VirginityManager.cs migration - Convert all string-based AddChange calls to typed:
- ProcessVirginity: `AddChange("TCVAR:破瓜", ...)` → `AddChange(new TCVarChange(TCVarIndex.Defloration, ...))` (index 15)
- ProcessVirginity: `AddChange("TALENT:処女", ...)` → `AddChange(new TalentChange(TalentIndex.Virginity, ...))` (index 0)
- ProcessChastityEffects: `AddChange("SOURCE:情愛", ...)` → `AddChange(new SourceChange(SourceIndex.Love, ...))`
- ProcessChastityEffects: `AddChange("SOURCE:反感", ...)` → `AddChange(new SourceChange(SourceIndex.Antipathy, ...))`
- ProcessAnalVirginity: `AddChange("EXP:Ａ経験", ...)` → `AddChange(new ExpChange(ExpIndex.AExp, ...))`
- ProcessKissVirginity: `AddChange("EXP:口淫経験", ...)` → `AddChange(new ExpChange(ExpIndex.OralExp, ...))`
- Update VirginityChange to inherit from StateChange base or accept StateChange list
- **Note**: VirginityManager has local `TCVarDefloration = new(8)` which is incorrect (should be 15). Fix as part of migration.
- **Note**: After migration, remove VirginityManager local constants and use centralized Types (TCVarIndex, TalentIndex, etc.)

**AC#2**: VirginityState.cs migration:
- VirginityState.cs contains VirginityChange class (lines 45-107) which has the AddChange method (line 93)
- Change `List<LegacyStateChange>` property to `List<StateChange>`
- Update `AddChange` helper to accept typed StateChange objects
- Update `AddChanges` helper signature

**AC#3**: ExperienceState.cs migration:
- Change `List<LegacyStateChange>` to `List<StateChange>`
- Update `AddChange` helper signature

**AC#4**: ExpIndex extension:
- Add to Era.Core/Types/ExpIndex.cs well-known values (from Game/CSV/exp.csv):
```csharp
// Orgasm experience (for OrgasmProcessor)
public static readonly ExpIndex OrgasmExperience = new(10);    // 絶頂経験
public static readonly ExpIndex OrgasmExperienceC = new(5);    // Ｃ絶頂経験
public static readonly ExpIndex OrgasmExperienceV = new(6);    // Ｖ絶頂経験
public static readonly ExpIndex OrgasmExperienceA = new(7);    // Ａ絶頂経験
public static readonly ExpIndex OrgasmExperienceB = new(8);    // Ｂ絶頂経験

// Additional for VirginityManager
public static readonly ExpIndex AExp = new(2);                  // Ａ経験
public static readonly ExpIndex OralExp = new(25);              // 口淫経験
```

**AC#5**: OrgasmProcessor.UpdateOrgasmExperience migration:
- The UpdateOrgasmExperience method body is a TODO stub with commented code (F402 deferral). The method exists but does nothing.
- Implement the method body using the new ExpIndex values defined in Task 2:
  - `ExpChange(ExpIndex.OrgasmExperience, totalExp)`
  - `ExpChange(ExpIndex.OrgasmExperienceC, c)`
  - `ExpChange(ExpIndex.OrgasmExperienceV, v)`
  - `ExpChange(ExpIndex.OrgasmExperienceA, a)`
  - `ExpChange(ExpIndex.OrgasmExperienceB, b)`

**AC#6-7**: LegacyStateChange deletion:
- After all Character namespace migrations complete
- Verify no remaining references in Era.Core/
- Delete Era.Core/Types/LegacyStateChange.cs

**AC#8-9**: Verify no regression.

**AC#10**: Update engine-dev SKILL.md with new ExpIndex well-known values (OrgasmExperience*, AExp, OralExp). One representative value check (OrgasmExperience) is sufficient for SSOT compliance verification.

**AC#11-12**: Add missing well-known values to centralized Types files:
- TCVarIndex.Defloration = new(15) for 破瓜 (from TCVAR.csv)
- TalentIndex.Virginity = new(0) for 処女 (from Talent.csv)

**AC#13**: Verify VirginityManager's local TCVarIndex constant is removed in favor of centralized TCVarIndex.Defloration.
- Remove `private static readonly TCVarIndex TCVarDefloration = new(8);` (line 25)
- Use `TCVarIndex.Defloration` from centralized Types instead

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 11-12 | Add missing well-known values: TCVarIndex.Defloration(15), TalentIndex.Virginity(0) | [x] |
| 2 | 4 | Extend ExpIndex with orgasm + virginity experience well-known values | [x] |
| 3 | 1,13 | Migrate VirginityManager AddChange calls and remove local TCVarIndex constant | [x] |
| 4 | 2 | Migrate VirginityState/VirginityChange to typed StateChange | [x] |
| 5 | 3 | Migrate ExperienceState to typed StateChange | [x] |
| 6 | 5 | Migrate OrgasmProcessor.UpdateOrgasmExperience to ExpChange (uncomment method body) | [x] |
| 7 | 6-7 | Delete LegacyStateChange.cs after all migrations | [x] |
| 8 | 8-9 | Verify build and tests pass | [x] |
| 9 | 10 | Update engine-dev SKILL.md with ExpIndex values | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F402 | Provides SourceChange, StateChange hierarchy, moved LegacyStateChange |
| Predecessor | F401 | Provides ExpChange subtype |

---

## Links

- [feature-402.md](feature-402.md) - Parent feature (F402 残課題 source)
- [feature-401.md](feature-401.md) - F401 StateChange Type Safety Migration
- [feature-410.md](feature-410.md) - F410 VirginityManager Local Constant Consolidation (残課題)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 7 definition

---

## Review Notes

- **2026-01-08 FL iter3**: [resolved] AC#1 Details bug fix (TCVarDefloration index) - covered by AC#11 (TCVarIndex.Defloration verification)
- **2026-01-08**: 残課題 (VirginityManager local constants) → Tracked in F410

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-08 | create | opus | Follow-up from F402 残課題 | PROPOSED |
| 2026-01-08 20:20 | START | implementer | Task 1 | - |
| 2026-01-08 20:20 | END | implementer | Task 1 | SUCCESS |
| 2026-01-08 20:22 | START | implementer | Task 2 | - |
| 2026-01-08 20:22 | END | implementer | Task 2 | SUCCESS |
| 2026-01-08 20:24 | START | implementer | Task 3 | - |
| 2026-01-08 20:27 | END | implementer | Task 3 | SUCCESS |
| 2026-01-08 20:28 | START | implementer | Task 4 | - |
| 2026-01-08 20:29 | END | implementer | Task 4 | SUCCESS |
| 2026-01-08 20:34 | START | implementer | Task 5 | - |
| 2026-01-08 20:34 | END | implementer | Task 5 | SUCCESS |
| 2026-01-08 20:35 | START | implementer | Task 6 | - |
| 2026-01-08 20:36 | END | implementer | Task 6 | SUCCESS |
| 2026-01-08 20:36 | START | implementer | Task 7 | - |
| 2026-01-08 20:38 | END | implementer | Task 7 | SUCCESS |
| 2026-01-08 20:39 | START | implementer | Task 8 | - |
| 2026-01-08 20:39 | END | implementer | Task 8 | SUCCESS |
| 2026-01-08 20:41 | START | implementer | Task 9 | - |
| 2026-01-08 20:41 | END | implementer | Task 9 | SUCCESS |
