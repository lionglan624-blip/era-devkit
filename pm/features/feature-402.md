# Feature 402: StateChange Equipment/Orgasm Migration

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

Complete StateChange type safety migration for Equipment/Orgasm processors. Add new StateChange subtypes (SourceChange, DownbaseChange, NowExChange, ExChange, TimeChange) and migrate remaining string-based patterns from F401.

---

## Background

### Philosophy (Mid-term Vision)

**Full C# Architecture**: Type safety enables compile-time checking, self-documenting code, and safer refactoring.

### Problem (Current Issue)

F401 established the abstract StateChange hierarchy but deferred several migrations:

| Processor | String Patterns | Blocked By |
|-----------|-----------------|------------|
| EquipmentProcessor | SOURCE:*, DOWNBASE:* | Missing SourceChange, DownbaseChange |
| OrgasmProcessor | SOURCE:*, DOWNBASE:*, NOWEX:*, EX:* | Missing NowExChange, ExChange |
| TrainingProcessor | "TIME", "CFLAG:好感度" | Missing TimeChange, CharacterFlagIndex mapping |

`LegacyStateChange` temporary struct remains in TrainingResult.cs for compatibility.

### Goal (What to Achieve)

1. Add new StateChange subtypes: SourceChange, DownbaseChange, NowExChange, ExChange, TimeChange
2. Migrate EquipmentProcessor to typed StateChange
3. Migrate OrgasmProcessor to typed StateChange
4. Complete TrainingProcessor TIME migration (CFLAG deferred to follow-up)
5. Remove LegacyStateChange after all migrations complete

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SourceChange subtype exists | code | Grep | contains | record SourceChange | [x] |
| 2 | DownbaseChange subtype exists | code | Grep | contains | record DownbaseChange | [x] |
| 3 | NowExChange subtype exists | code | Grep | contains | record NowExChange | [x] |
| 3b | NowExIndex has multi-orgasm well-known values | code | Grep NowExIndex.cs | contains | DoubleOrgasm | [x] |
| 4 | ExChange subtype exists | code | Grep | contains | record ExChange | [x] |
| 5 | TimeChange subtype exists | code | Grep | contains | record TimeChange | [x] |
| 6 | EquipmentResult.cs uses typed StateChange | code | Grep | not_contains | LegacyStateChange | [x] |
| 7 | OrgasmProcessor uses typed StateChange | code | Grep | not_contains | LegacyStateChange | [x] |
| 8 | TrainingProcessor TIME uses TimeChange (uncomment and convert) | code | Grep | contains | TimeChange( | [x] |
| 9 | LegacyStateChange removed from Era.Core/Training/ | code | Grep Era.Core/Training/ | not_contains | LegacyStateChange | [x] |
| 10 | C# build succeeds | build | dotnet | succeeds | - | [x] |
| 11 | All Training tests pass | test | dotnet | succeeds | Category=Training | [x] |
| 12 | All Mark tests pass | test | dotnet | succeeds | Category=Mark | [x] |
| 13 | SSOT updated (DownbaseIndex) | code | Grep | contains | DownbaseIndex | [x] |
| 14 | SSOT updated (ExIndex) | code | Grep | contains | ExIndex | [x] |

### AC Details

**AC#1-5**: Add new StateChange subtypes to Era.Core/Training/StateChange.cs:
```csharp
// Era.Core/Training/StateChange.cs (additions)
public record SourceChange(SourceIndex Index, int Delta) : StateChange;  // Emotional state: 快V, 快A, 情愛, 苦痛
public record DownbaseChange(DownbaseIndex Index, int Delta) : StateChange;  // Stamina/mood decreases
public record NowExChange(NowExIndex Index, int Value) : StateChange;  // Current orgasm flags
public record ExChange(ExIndex Index, int Delta) : StateChange;  // Orgasm counters
public record TimeChange(int Delta) : StateChange;  // Training time modifier
```

**AC#6**: EquipmentResult.cs type conversion:
- **Migration steps**:
  1. Change `List<LegacyStateChange> Changes` to `List<StateChange> Changes`
  2. Update `AddChange` helper to accept `StateChange` parameter instead of string-based construction
  3. Update `AddChanges` helper to accept `IEnumerable<StateChange>`
- **Note**: EquipmentProcessor is currently a skeleton (ProcessEquipment returns empty result). Processor migration patterns for future reference:
  - `AddChange("SOURCE:快Ｖ", ...)` → `AddChange(new SourceChange(SourceIndex.PleasureV, ...))`
  - `AddChange("DOWNBASE:体力", ...)` → `AddChange(new DownbaseChange(DownbaseIndex.Stamina, ...))`

**AC#7**: OrgasmProcessor.cs migration:
- **Scope**: OrgasmProcessor.cs has AddChange calls in four methods:
  - `ApplySourceChanges` (lines 287-328): SOURCE:*, DOWNBASE:* patterns
  - `UpdateNowexFlags` (lines 344-361): NOWEX:* patterns (includes multi-orgasm)
  - `UpdateOrgasmCounters` (lines 377-380): EX:* patterns (basic orgasm counters only)
  - `UpdateOrgasmExperience` (lines 400-405): EXP:* patterns (already have ExpChange from F401, out of scope)
- **Design Decision**: NowExChange/ExChange records do NOT include CharacterId. CharacterId is derived from parent OrgasmResult.CharacterId container. All Result types already carry CharacterId, so records omit it for simplicity.
- Convert SOURCE/DOWNBASE patterns (same as AC#6). See SourceIndex.cs for well-known values (Lust/欲情, Obedience/恭順, Exposure/露出, Submission/屈従, Antipathy/反感).
- Convert `AddChange($"NOWEX:{target}:Ｃ絶頂", ...)` → `AddChange(new NowExChange(NowExIndex.OrgasmC, ...))`
- Convert `AddChange($"EX:{target}:Ｃ絶頂", ...)` → `AddChange(new ExChange(ExIndex.OrgasmC, ...))`
- Update OrgasmResult.cs to use `List<StateChange>` instead of `List<LegacyStateChange>`

**AC#8**: TrainingProcessor.cs TIME migration:
- **Semantics**: `TimeChange(int Delta)` follows additive semantics like `AbilityChange`. The time modifier from `_basicChecks.GetTimeModifier()` is a delta value that modifies the training time.
- **Note**: TrainingProcessor TIME logic is currently commented out (TODO blocks). This AC requires uncommenting and converting to TimeChange:
  - The time modifier comes from `_basicChecks.GetTimeModifier(command)`
  - Uncommented code pattern:
    ```csharp
    int timeModifier = _basicChecks.GetTimeModifier(command);
    if (timeModifier != 0)
    {
        result.AddChange(new TimeChange(timeModifier));
    }
    ```
- **CFLAG migration deferred**: CFLAG:好感度 migration requires CharacterFlagIndex.Favor well-known value which does not exist. Deferred to follow-up feature.

**AC#9**: After all Era.Core/Training/ migrations complete, remove `LegacyStateChange` from TrainingResult.cs. Note: VirginityState.cs and ExperienceState.cs in Era.Core/Character/ also use LegacyStateChange - deferred to follow-up feature.

**AC#10-12**: Verify no regression.

**AC#13-14**: Update engine-dev SKILL.md with new index types. Grep path: `.claude/skills/engine-dev/SKILL.md`

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1-5 | Add new StateChange subtypes (SourceChange, DownbaseChange, NowExChange, ExChange, TimeChange) | [x] |
| 2 | 1-5 | Add index types to Era.Core/Types: SourceIndex (exists), DownbaseIndex (NEW: Stamina/体力=0, Willpower/気力=1 - from ERB patterns), NowExIndex (extend: add DoubleOrgasm/二重絶頂=4, TripleOrgasm/三重絶頂=5, QuadOrgasm/四重絶頂=6), ExIndex (NEW: OrgasmC=0, OrgasmV=1, OrgasmA=2, OrgasmB=3 - from Game/CSV/EX.CSV), BaseIndex (exists) | [x] |
| 3 | 6 | Update EquipmentResult.cs to use List<StateChange> (EquipmentProcessor is skeleton - no actual AddChange calls to migrate) | [x] |
| 4 | 7 | Migrate OrgasmProcessor to typed StateChange | [x] |
| 5 | 7 | Update OrgasmResult.cs to use List<StateChange> | [x] |
| 6 | 8 | Complete TrainingProcessor TIME migration (CFLAG deferred) | [x] |
| 7 | 9 | Remove LegacyStateChange from TrainingResult.cs (moved to Era.Core/Types/) | [x] |
| 8 | 10-12 | Verify build and tests pass | [x] |
| 9 | 13-14 | Update engine-dev SKILL.md with DownbaseIndex, ExIndex types | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F401 | Provides abstract StateChange hierarchy |
| Predecessor | F393 | Original EquipmentProcessor/OrgasmProcessor implementation |

---

## Links

- [feature-401.md](feature-401.md) - Parent feature (F401 StateChange Type Safety Migration)
- [feature-393.md](feature-393.md) - F393 Training Processing Core
- [feature-403.md](feature-403.md) - Follow-up: Character Namespace StateChange Migration
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 7 definition (Technical Debt Consolidation)

---

## Review Notes
- **2026-01-08 FL iter1**: [resolved] AC Details AC#7 - Design decision: Option (B) selected. CharacterId is derived from parent OrgasmResult.CharacterId container. Records omit CharacterId for simplicity.
- **2026-01-08 FL iter4**: [resolved] AC#1-5 BaseChange - Removed from scope. No processor uses it, no planned use in any future phase. YAGNI applied.
- **2026-01-08 FL iter6**: [resolved] AC#7 SOURCE mapping - Rely on SourceIndex.cs as SSOT. Japanese comments in SourceIndex.cs (欲情, 恭順, 露出, 屈従, 反感) provide sufficient mapping for implementers.

---

## 残課題

| Issue | Description | Resolution |
|-------|-------------|------------|
| VirginityManager migration | VirginityManager.cs lines 170-171 use string-based AddChange for SOURCE:情愛, SOURCE:反感. Out of scope for F402 (focuses on OrgasmProcessor). | → Tracked in F403 |
| Era.Core/Character/ LegacyStateChange | VirginityState.cs and ExperienceState.cs in Era.Core/Character/ use LegacyStateChange. F402 scope is Era.Core/Training/ only. | → Tracked in F403 |
| LegacyStateChange location | LegacyStateChange moved from Era.Core/Training/TrainingResult.cs to Era.Core/Types/LegacyStateChange.cs for Character namespace compatibility. Delete when Character namespace migration completes. | → Tracked in F403 |
| OrgasmProcessor EXP migration | UpdateOrgasmExperience method's EXP:* patterns not migrated (ExpIndex well-known values for orgasm experience not defined). | → Tracked in F403 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-08 | create | opus | Follow-up from F401 残課題 | PROPOSED |
| 2026-01-08 13:47 | START | implementer | Task 1-2 | - |
| 2026-01-08 13:48 | END | implementer | Task 1-2 | SUCCESS |
| 2026-01-08 13:48 | START | implementer | Task 3-5 | - |
| 2026-01-08 13:52 | END | implementer | Task 3-5 | SUCCESS |
| 2026-01-08 13:53 | START | implementer | Task 6 | - |
| 2026-01-08 13:54 | END | implementer | Task 6 | SUCCESS |
| 2026-01-08 13:54 | START | opus | Task 7 fix | LegacyStateChange moved to Era.Core/Types/ |
| 2026-01-08 13:55 | END | opus | Task 7 fix | SUCCESS |
| 2026-01-08 13:55 | START | implementer | Task 9 | - |
| 2026-01-08 13:56 | END | implementer | Task 9 | SUCCESS |
| 2026-01-08 13:57 | START | ac-tester | AC verification | - |
| 2026-01-08 13:58 | END | ac-tester | AC verification | 14/14 PASS |
| 2026-01-08 14:00 | START | feature-reviewer | post review | - |
| 2026-01-08 14:01 | END | feature-reviewer | post review | NEEDS_REVISION (3 doc issues) |
| 2026-01-08 14:02 | START | opus | doc fix | TrainingProcessor.cs, SKILL.md comments |
| 2026-01-08 14:02 | END | opus | doc fix | SUCCESS |
| 2026-01-08 14:03 | START | feature-reviewer | doc-check | - |
| 2026-01-08 14:04 | END | feature-reviewer | doc-check | NEEDS_REVISION (5 SSOT issues) |
| 2026-01-08 14:05 | START | opus | SSOT fix | SKILL.md well-known values |
| 2026-01-08 14:05 | END | opus | SSOT fix | SUCCESS |
