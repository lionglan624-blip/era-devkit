# Feature 401: StateChange Type Safety Migration

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

## Type: engine

## Created: 2026-01-08

---

## Summary

Migrate F393's string-based StateChange to F395's abstract hierarchy for type safety and consistency across training subsystem.

---

## Background

### Philosophy (Mid-term Vision)

**Full C# Architecture**: Type safety enables compile-time checking, self-documenting code, and safer refactoring.

### Problem (Current Issue)

Two incompatible StateChange patterns exist in Era.Core/Training/:

| Feature | Pattern | Location |
|---------|---------|----------|
| F393 | `StateChange(string variable, int delta)` | TrainingResult.cs |
| F395 | `abstract record StateChange` + concrete subtypes | MarkAcquisitionResult |

This inconsistency creates:
- Confusion about which pattern to use
- Risk of typos in string-based pattern
- Harder refactoring (no IDE support for strings)
- Technical debt if left unfixed

### Goal (What to Achieve)

1. Replace F393's string-based StateChange with F395's abstract hierarchy
2. Update TrainingResult to use List<StateChange> with concrete subtypes
3. Update all usages in training processors
4. Ensure all tests pass with new type-safe pattern

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | StateChange abstract type in common location | file | Glob | exists | Era.Core/Training/StateChange.cs | [ ] |
| 2 | TrainingResult uses abstract StateChange | code | Grep | contains | List<StateChange> | [ ] |
| 3 | LegacyStateChange compatibility struct created | code | Grep | contains | LegacyStateChange | [ ] |
| 4 | TrainingProcessor uses typed StateChange construction | code | Grep | contains | AbilityChange( | [ ] |
| 5 | MarkStateChange prefix removed from hierarchy | code | Grep | not_contains | MarkStateChange | [ ] |
| 6 | C# build succeeds | build | dotnet | succeeds | - | [ ] |
| 7 | All Training tests pass | test | dotnet | succeeds | Category=Training | [ ] |
| 8 | All Mark tests pass | test | dotnet | succeeds | Category=Mark | [ ] |

### AC Details

**AC#1**: Rename existing MarkStateChange hierarchy (from IMarkSystem.cs) to StateChange and move to dedicated file. Migrate F395's existing types and add TalentChange for TrainingProcessor. Other new types (SourceChange, DownbaseChange, etc.) are deferred to follow-up feature.
```csharp
// Era.Core/Training/StateChange.cs
public abstract record StateChange;
// Delta = additive change (growth operations add to existing values)
public record AbilityChange(AbilityIndex Index, int Delta) : StateChange;
public record TalentChange(TalentIndex Index, int Delta) : StateChange;  // NEW: additive like AbilityChange
// Value = absolute set or context-dependent (flags use assignment semantics, EXP uses additive in growth context)
public record CFlagChange(CharacterFlagIndex Index, int Value) : StateChange;
public record TCVarChange(TCVarIndex Index, int Value) : StateChange;
public record TFlagChange(FlagIndex Index, int Value) : StateChange;  // F395 uses FlagIndex
public record ExpChange(ExpIndex Index, int Value) : StateChange;  // Value = additive delta in growth context
public record MarkHistoryChange(MarkIndex Index, int Value) : StateChange;  // F395 naming
// Note: SourceChange, DownbaseChange, NowExChange, ExChange - deferred to follow-up
```

**AC#2**: Update TrainingResult.cs. **Breaking change**: TrainingResult.Changes changes from `List<struct StateChange>` (string-based) to `List<abstract record StateChange>` (type-safe hierarchy).

**Implementation Note**: TrainingResult remains a class (not converted to record). Only the `Changes` property type changes. Success/ErrorMessage properties are preserved.

**AddChange Method Change**:
```csharp
// Before (F393)
public TrainingResult AddChange(string variable, int delta, string? reason = null);

// After (F401) - old string-based overload removed
public TrainingResult AddChange(StateChange change);
public TrainingResult AddChanges(IEnumerable<StateChange> changes);
```

**Scope Note**: EquipmentResult.cs and OrgasmResult.cs also use `List<StateChange>` from the F393 string-based type. These are explicitly **deferred** to follow-up feature because:
- They require new StateChange subtypes (SourceChange, DownbaseChange, NowExChange, ExChange)
- Their AddChange() helper methods need conversion to construct typed StateChange
- Migrating them in this feature would significantly expand scope

**Compatibility Solution**: The old string-based StateChange is renamed to `LegacyStateChange` and kept in TrainingResult.cs temporarily so that EquipmentResult.cs and OrgasmResult.cs can continue to compile. Follow-up feature will remove LegacyStateChange after migrating those files.

```csharp
// Before (F393)
public readonly record struct StateChange { string Variable; int Delta; string? Reason; }

// After (F401)
// TrainingResult class (unchanged structure, just type reference update)
public class TrainingResult {
    public List<StateChange> Changes { get; init; }  // Uses abstract StateChange
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    // AddChange now takes typed StateChange
}
// LegacyStateChange kept temporarily for Equipment/OrgasmResult
public readonly record struct LegacyStateChange { ... }
```

**AC#3**: Rename old string-based StateChange struct to LegacyStateChange in TrainingResult.cs (kept for Equipment/OrgasmResult compatibility).

**AC#4**: AbilityGrowthProcessor returns GrowthResult (with AbilityGrowth/TalentGrowth types). TrainingProcessor converts these to typed StateChange subtypes (AbilityChange, TalentChange, ExpChange) instead of string-based StateChange.

**Scope Boundary**: AC#4 only covers GrowthResult conversion. TrainingProcessor also has TIME (line 56) and CFLAG:好感度 (line 106) string-based changes which are explicitly deferred to 残課題 because:
- TIME requires TimeChange subtype (not yet defined)
- CFLAG:好感度 requires CharacterFlagIndex mapping for favor (needs design work)

**AC#5**: Update IMarkSystem.cs to use unified StateChange (rename MarkStateChange → StateChange, update MarkAcquisitionResult.SideEffects type).

**AC#6-8**: Verify no regression in existing functionality.
- AC#6: `dotnet build Era.Core`
- AC#7: `dotnet test Era.Core.Tests --filter Category=Training`
- AC#8: `dotnet test Era.Core.Tests --filter Category=Mark`

**Note**: EquipmentProcessor and OrgasmProcessor use mixed change types:
- ExpChange: Already in hierarchy (OrgasmProcessor EXP changes could theoretically use ExpChange)
- SOURCE, DOWNBASE, NOWEX, EX: Require new StateChange subtypes not yet defined

Both processors are deferred to follow-up feature to avoid partial migration and ensure clean separation.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Move MarkStateChange hierarchy from IMarkSystem.cs to StateChange.cs, rename to StateChange, add TalentChange | [x] |
| 2 | 5 | Update IMarkSystem.cs and MarkSystem.cs to use StateChange (update MarkAcquisitionResult.SideEffects type) | [x] |
| 3 | 3 | Rename StateChange struct to LegacyStateChange in TrainingResult.cs | [x] |
| 4 | 3 | Update EquipmentResult.cs and OrgasmResult.cs to use LegacyStateChange | [x] |
| 5 | 2 | Update TrainingResult.Changes property to use abstract StateChange and modify AddChange helper | [x] |
| 6 | 4 | Update TrainingProcessor to convert GrowthResult to typed StateChange subtypes (AbilityChange, TalentChange, ExpChange) | [x] |
| 7 | 6-8 | Verify build and tests pass (Training + Mark categories) | [x] |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F393 | Source of current string-based StateChange |
| Predecessor | F395 | Provides abstract StateChange hierarchy design |

---

## Links

- [feature-393.md](feature-393.md) - F393 Training Processing (has string-based StateChange)
- [feature-395.md](feature-395.md) - F395 Mark System (defines abstract hierarchy)
- [feature-402.md](feature-402.md) - F402 StateChange Equipment/Orgasm Migration (follow-up)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 6 definition

---

## 残課題

| Issue | Description | Resolution |
|-------|-------------|------------|
| New StateChange subtypes | SourceChange, DownbaseChange, NowExChange, ExChange needed for Equipment/OrgasmProcessor | → Tracked in F402 |
| EquipmentProcessor migration | Uses SOURCE/DOWNBASE string patterns | → Tracked in F402 |
| OrgasmProcessor migration | Uses SOURCE/DOWNBASE/NOWEX/EX string patterns | → Tracked in F402 |
| TrainingProcessor TIME change | Uses string "TIME" for time modifier changes | → Tracked in F402 |
| TrainingProcessor CFLAG change | Uses string "CFLAG:好感度" for favor changes | → Tracked in F402 |
| LegacyStateChange removal | Temporary compatibility struct for Equipment/OrgasmResult | → Tracked in F402 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-08 | create | opus | Follow-up from F395 FL review (StateChange pattern unification) | PROPOSED |
| 2026-01-08 12:40 | START | implementer | Tasks 1-7 | - |
| 2026-01-08 12:40 | END | implementer | Tasks 1-7 | SUCCESS |
