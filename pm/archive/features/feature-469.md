# Feature 469: SCOMF Full Implementation

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

## Created: 2026-01-12

---

## Summary

**Phase 13: DDD Foundation - SCOMF Variable Infrastructure**

Add STAIN and DOWNBASE variable support to IVariableStore, completing the variable infrastructure required for SCOMF1-16 scenarios. This feature focuses on infrastructure only; actual SCOMF scenario implementation is deferred to follow-up features.

**Note**: FL review determined that full SCOMF1-16 implementation (~3000+ lines) is too large for a single feature. This feature provides the infrastructure, and F473 will implement the scenarios.

**Output**:
- `Era.Core/Types/StainIndex.cs` - Strongly-typed STAIN index
- `Era.Core/Interfaces/IVariableStore.cs` - Add GetStain/SetStain and GetDownbase/SetDownbase methods
- `Era.Core/Variables/VariableStore.cs` - Implement GetStain/SetStain and GetDownbase/SetDownbase
- `Era.Core/Variables/CharacterVariables.cs` - Add _stain array field
- Unit tests in `Era.Core.Tests/Variables/StainVariableTests.cs`

**Volume**: ~200 lines (type definitions + interface + implementation + tests)

---

## Background

### Philosophy (Mid-term Vision)

**Phase 13: DDD Foundation** - Establish Domain-Driven Design foundation through Aggregate Root, Repository, and UnitOfWork patterns. SCOMF completion applies these patterns to special training scenarios, enabling proper variable manipulation through IVariableStore abstraction.

### Problem (Current Issue)

SCOMF1-16 scenarios require STAIN and DOWNBASE variable access, but IVariableStore is missing:
- GetStain/SetStain methods (needed for STAIN array: 754 ERB occurrences)
- GetDownbase/SetDownbase methods (needed for DOWNBASE array: 532 ERB occurrences)

CharacterVariables.cs also lacks the `_stain` array field.

Without this infrastructure, SCOMF scenario implementation cannot proceed.

### Goal (What to Achieve)

1. **Add STAIN support** to IVariableStore (GetStain/SetStain with StainIndex)
2. **Add DOWNBASE support** to IVariableStore (GetDownbase/SetDownbase with DownbaseIndex)
3. **Add STAIN array infrastructure** to CharacterVariables.cs
4. **Test variable access** through unit tests
5. **Eliminate technical debt** (no TODO/FIXME/HACK comments)

**Deferred to F473**: SpecialTraining full implementation for SCOMF1-16 scenarios

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | StainIndex type exists | file | Glob | exists | "Era.Core/Types/StainIndex.cs" | [x] |
| 2 | IVariableStore has GetStain | code | Grep | contains | "GetStain.*CharacterId.*StainIndex" | [x] |
| 3 | IVariableStore has SetStain | code | Grep | contains | "SetStain.*CharacterId.*StainIndex" | [x] |
| 4 | IVariableStore has GetDownbase | code | Grep | contains | "GetDownbase.*CharacterId.*DownbaseIndex" | [x] |
| 5 | IVariableStore has SetDownbase | code | Grep | contains | "SetDownbase.*CharacterId.*DownbaseIndex" | [x] |
| 6 | CharacterVariables has _stain array | code | Grep | contains | "_stain" | [x] |
| 7 | Variable tests pass | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~StainVariableTests" | [x] |
| 8 | Build succeeds | build | Bash | succeeds | "dotnet build Era.Core" | [x] |
| 9 | Zero technical debt | code | Grep | not_contains | "TODO|FIXME|HACK" | [x] |

### AC Details

**AC#1**: StainIndex strongly-typed ID
- Test: Glob pattern="Era.Core/Types/StainIndex.cs"
- Expected: File exists with readonly struct StainIndex
- Verifies: Type-safe STAIN index (consistent with DownbaseIndex pattern)

**AC#2**: IVariableStore GetStain method
- Test: Grep pattern="GetStain.*CharacterId.*StainIndex" path="Era.Core/Interfaces/IVariableStore.cs"
- Expected: Contains `Result<int> GetStain(CharacterId character, StainIndex index);`
- Verifies: STAIN variable access via IVariableStore

**AC#3**: IVariableStore SetStain method
- Test: Grep pattern="SetStain.*CharacterId.*StainIndex" path="Era.Core/Interfaces/IVariableStore.cs"
- Expected: Contains `void SetStain(CharacterId character, StainIndex index, int value);`
- Verifies: STAIN variable write via IVariableStore

**AC#4**: IVariableStore GetDownbase method
- Test: Grep pattern="GetDownbase.*CharacterId.*DownbaseIndex" path="Era.Core/Interfaces/IVariableStore.cs"
- Expected: Contains `Result<int> GetDownbase(CharacterId character, DownbaseIndex index);`
- Verifies: DOWNBASE variable access via IVariableStore (DownbaseIndex already exists)

**AC#5**: IVariableStore SetDownbase method
- Test: Grep pattern="SetDownbase.*CharacterId.*DownbaseIndex" path="Era.Core/Interfaces/IVariableStore.cs"
- Expected: Contains `void SetDownbase(CharacterId character, DownbaseIndex index, int value);`
- Verifies: DOWNBASE variable write via IVariableStore

**AC#6**: CharacterVariables STAIN array
- Test: Grep pattern="_stain" path="Era.Core/Variables/CharacterVariables.cs"
- Expected: Contains `_stain` array field declaration
- Verifies: STAIN array infrastructure exists

**AC#7**: Variable unit tests pass
- Test: `dotnet test --filter FullyQualifiedName~StainVariableTests`
- Expected: PASS
- Verifies: STAIN/DOWNBASE variable access works correctly

**AC#8**: Build succeeds
- Test: `dotnet build Era.Core`
- Expected: Build succeeded
- Verifies: No compile errors from changes

**AC#9**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Interfaces/IVariableStore.cs Era.Core/Variables/VariableStore.cs Era.Core/Variables/CharacterVariables.cs Era.Core/Types/StainIndex.cs"
- Expected: 0 matches
- Verifies: Clean implementation in all modified files

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create StainIndex strongly-typed ID in Era.Core/Types/ | [x] |
| 2 | 2,3 | Add GetStain/SetStain methods to IVariableStore interface | [x] |
| 3 | 4,5 | Add GetDownbase/SetDownbase methods to IVariableStore interface | [x] |
| 4 | 6 | Add _stain array field to CharacterVariables.cs | [x] |
| 5 | 2,3 | Implement GetStain/SetStain in VariableStore delegating to CharacterVariables | [x] |
| 6 | 4,5 | Implement GetDownbase/SetDownbase in VariableStore delegating to CharacterVariables | [x] |
| 7 | 7 | Write unit tests for STAIN/DOWNBASE variable access | [x] |
| 8 | 8 | Verify build succeeds with all changes | [x] |
| 9 | 9 | Remove all TODO/FIXME/HACK comments from modified files | [x] |

<!-- AC:Task 1:1 Rule: 9 ACs = 9 Tasks (Tasks 2,3,5,6 batch related get/set methods) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### StainIndex Type Definition

```csharp
// Era.Core/Types/StainIndex.cs
namespace Era.Core.Types
{
    /// <summary>
    /// Strongly typed index for STAIN arrays.
    /// Prevents mixing stain indices with other integer types.
    /// STAIN: character stain/dirt status during scenes.
    /// Feature 469 - Required for SCOMF special training scenarios.
    /// </summary>
    public readonly record struct StainIndex
    {
        public int Value { get; }

        public StainIndex(int value)
        {
            Value = value;
        }

        /// <summary>
        /// Implicit conversion to int for backward compatibility with existing code.
        /// </summary>
        public static implicit operator int(StainIndex id) => id.Value;

        /// <summary>
        /// Explicit conversion from int to StainIndex.
        /// Explicit to encourage type safety and prevent accidental conversions.
        /// </summary>
        public static explicit operator StainIndex(int value) => new(value);

        // Well-known indices (add as needed based on ERB usage)
    }
}
```

### IVariableStore Extension

```csharp
// Add to Era.Core/Interfaces/IVariableStore.cs

// STAIN: character stain/dirt status
// Feature 469 - Required for SCOMF special training scenarios
Result<int> GetStain(CharacterId character, StainIndex index);
void SetStain(CharacterId character, StainIndex index, int value);

// DOWNBASE: temporary status decreases (stamina, willpower)
// Feature 469 - Required for SCOMF special training scenarios
// Note: DownbaseIndex already exists (Era.Core/Types/DownbaseIndex.cs)
Result<int> GetDownbase(CharacterId character, DownbaseIndex index);
void SetDownbase(CharacterId character, DownbaseIndex index, int value);
```

### CharacterVariables Extension

```csharp
// Add to Era.Core/Variables/CharacterVariables.cs

private const int STAIN_SIZE = 100;  // Verify from CSV/ERB usage
private readonly int[] _stain;

// In constructor:
_stain = new int[STAIN_SIZE];

// Add methods:
public int GetStain(StainIndex index) => _stain[index.Value];
public void SetStain(StainIndex index, int value) => _stain[index.Value] = value;
```

### VariableStore Extension

```csharp
// Add to Era.Core/Variables/VariableStore.cs

public Result<int> GetStain(CharacterId character, StainIndex index)
{
    if (!_characters.TryGetValue(character, out var charVars))
        return Result<int>.Fail($"Character {character.Value} not found");
    return Result<int>.Ok(charVars.GetStain(index));
}

public void SetStain(CharacterId character, StainIndex index, int value)
{
    if (_characters.TryGetValue(character, out var charVars))
        charVars.SetStain(index, value);
}

// Similar for GetDownbase/SetDownbase
```

### Test Requirements

Tests must verify:
1. StainIndex type works correctly (implicit/explicit conversion)
2. GetStain/SetStain work with valid CharacterId
3. GetDownbase/SetDownbase work with valid CharacterId
4. Invalid CharacterId returns Result.Fail
5. Array bounds checking (if applicable)

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F468 | Legacy Bridge + DI Integration - provides IVariableStore access patterns |
| Predecessor | F463 | Phase 13 Planning - defines DDD Foundation scope |
| Successor | F473 | SCOMF Full Implementation - uses this feature's infrastructure |

---

## Links

- [feature-463.md](feature-463.md) - Phase 13 Planning (parent feature)
- [feature-435.md](feature-435.md) - SCOMF stub implementation (Phase 9)
- [feature-465.md](feature-465.md) - DDD Foundation: Aggregate Root + Character Aggregate
- [feature-466.md](feature-466.md) - DDD Foundation: Repository Pattern
- [feature-467.md](feature-467.md) - DDD Foundation: UnitOfWork Pattern
- [feature-468.md](feature-468.md) - Legacy Bridge + DI Integration (related pattern)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 13 Task 10

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-13 FL iter1**: [resolved] Fundamental rewrite - original scope was incorrect (confused F435 SCOMF1-16 with non-existent SOURCE/STAIN/EXP/TCVAR handlers)
- **2026-01-13 FL iter1**: [resolved] User decision: STAIN も含める - IVariableStore に GetStain/SetStain 追加が必要
- **2026-01-13 FL iter2**: [resolved] User decision: インフラ整備のみ、SCOMF実装は別Feature (F473) へ分離
- **2026-01-13 FL iter2**: [applied] Scope reduction: infrastructure only (STAIN/DOWNBASE support in IVariableStore)
- **2026-01-13 FL iter2**: [applied] Added DOWNBASE to scope (required for SCOMF scenarios)
- **2026-01-13 FL iter2**: [applied] Updated ACs and Tasks to focus on variable infrastructure

## 残課題

| Issue | Tracking | Notes |
|-------|----------|-------|
| SpecialTraining full implementation | F473 | SCOMF1-16 scenarios using F469 infrastructure |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-12 | create | feature-builder | Created from F463 Phase 13 Planning | PROPOSED |
| 2026-01-13 | rewrite | opus | FL iter1: Fundamental scope correction | PROPOSED |
| 2026-01-13 12:53 | START | implementer | Task 1-9 | - |
| 2026-01-13 13:00 | END | implementer | Task 1-9 | SUCCESS |
| 2026-01-13 13:05 | END | ac-tester | AC 1-9 | PASS (9/9) |
| 2026-01-13 13:10 | END | feature-reviewer | Post-review | READY (SSOT updated) |
