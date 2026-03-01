# Feature 513: Primary Constructor Migration - Commands/System + Other Directories

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

## Created: 2026-01-16

---

## Summary

Apply C# 14 Primary Constructor pattern to Era.Core files with DI constructors in Commands/System, Common, Variables, and Functions directories (9 files containing 21 classes with 24 DI fields).

**Scope**: Commands/System/ (3 files) + Common/ (3 files) + Variables/ (1 file) + Functions/ (2 files)

**Target Files** (verified by code inspection - DI classes only):
- Commands/System/: AddCharaHandler.cs, SetColorHandler.cs, SaveGameHandler.cs (3 files)
- Common/: GameInitialization.cs, InfoPrint.cs, CharacterSetup.cs (3 files)
- Variables/: VariableResolver.cs (1 file)
- Functions/: CharacterFunctions.cs, RandomFunctions.cs (2 files)

**Excluded Files** (parameterless constructors, NOT DI):
- Variables/: VariableStore.cs, CharacterVariables.cs, VariableScope.cs, LocalScope.cs (self-initialized data structures)
- Functions/: SystemRandom.cs, FunctionRegistry.cs (self-initialized fields)

**Philosophy**: Primary Constructor simplifies DI-heavy handler classes by eliminating explicit constructor parameter to field assignments.

**Before**:
```csharp
public class AddCharaHandler
{
    private readonly ICharacterManager _characterManager;

    public AddCharaHandler(ICharacterManager characterManager)
    {
        _characterManager = characterManager;
    }
}
```

**After**:
```csharp
public class AddCharaHandler(ICharacterManager characterManager)
{
    // Field declarations removed - primary constructor parameters are automatically available
}
```

**Expected Impact**: 24 field declarations removed, 24 constructor parameter assignments removed (total ~48 lines).

**Note**: Commands/System files each contain 5 handler classes (e.g., AddCharaHandler.cs has AddCharaHandler, DelCharaHandler, PickupCharaHandler, SwapCharaHandler, AddCopyCharaHandler).

---

## Background

### Philosophy (Mid-term Vision)

Phase 16: C# 14 Style Migration - Apply C# 14 patterns to existing code for simplification. Primary Constructor and Collection Expression reduce ~400 lines of boilerplate across all Era.Core directories.

### Problem (Current Issue)

Era.Core Commands/System, Common, Variables, and Functions directories contain classes with `private readonly` fields for dependency injection. These require explicit field declarations and constructor assignments, adding boilerplate.

### Goal (What to Achieve)

Apply Primary Constructor pattern to 9 DI-based files (containing 21 classes with 24 DI fields) in Commands/System + Other directories, eliminating ~48 lines of field declarations and constructor parameter assignments while maintaining identical functionality.

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | AddCharaHandler.cs uses Primary Constructor | code | Grep | count_equals | 5 for "class \w+Handler\(ICharacterManager" | [x] |
| 2 | SetColorHandler.cs uses Primary Constructor | code | Grep | count_equals | 5 for "class \w+Handler\(IStyleManager" | [x] |
| 3 | SaveGameHandler.cs uses Primary Constructor | code | Grep | count_equals | 5 for "class \w+Handler\(IGameState" | [x] |
| 4 | GameInitialization.cs uses Primary Constructor | code | Grep | contains | "class GameInitialization\(" | [x] |
| 5 | InfoPrint.cs uses Primary Constructor | code | Grep | contains | "class InfoPrint\(" | [x] |
| 6 | CharacterSetup.cs uses Primary Constructor | code | Grep | contains | "class CharacterSetup\(" | [x] |
| 7 | VariableResolver.cs uses Primary Constructor | code | Grep | contains | "class VariableResolver\(" | [x] |
| 8 | CharacterFunctions.cs uses Primary Constructor | code | Grep | contains | "class CharacterFunctions\(" | [x] |
| 9 | RandomFunctions.cs uses Primary Constructor | code | Grep | contains | "class RandomFunctions\(" | [x] |
| 10 | No DI field declarations in migrated files | code | Grep | count_equals | 0 | [x] |
| 11 | Zero NEW technical debt (see AC Details for baseline) | code | Grep | count_equals | 3 | [x] |
| 12 | All tests PASS | test | Bash | succeeds | "dotnet test" | [x] |

### AC Details

**AC#1-3**: Multi-class file Primary Constructor Migration (Commands/System)
- Test: Grep pattern `class \w+Handler\(IDependency` matches primary constructor syntax only
- Expected: Each file has exactly 5 classes using primary constructor pattern
- Pattern `class ClassName\(` specifically detects primary constructor (fails on traditional constructor)
- Before migration: 0 matches (traditional constructors use `public ClassName(...)` not `class ClassName(...)`)
- After migration: 5 matches per file

**AC#4-9**: Single-class file Primary Constructor Migration
- Test: Grep pattern `class ClassName\(` matches primary constructor syntax only
- Expected: Pattern found in file (class uses primary constructor)
- Files: GameInitialization.cs, InfoPrint.cs, CharacterSetup.cs, VariableResolver.cs, CharacterFunctions.cs, RandomFunctions.cs

**AC#10**: No DI field declarations in migrated files
- Pattern: `private readonly I`
- Paths (run separately, sum to 0):
  - Era.Core/Commands/System/AddCharaHandler.cs
  - Era.Core/Commands/System/SetColorHandler.cs
  - Era.Core/Commands/System/SaveGameHandler.cs
  - Era.Core/Common/GameInitialization.cs
  - Era.Core/Common/InfoPrint.cs
  - Era.Core/Common/CharacterSetup.cs
  - Era.Core/Variables/VariableResolver.cs
  - Era.Core/Functions/CharacterFunctions.cs
  - Era.Core/Functions/RandomFunctions.cs
- Expected: 0 matches total (all 24 DI field declarations replaced by primary constructor parameters)
- Baseline: Before migration 24 matches (15 in Commands/System, 6 in Common, 1 in Variables, 2 in Functions)

**AC#11**: Zero NEW technical debt (baseline-aware)
- Pattern: `TODO|FIXME|HACK`
- Paths: Same 9 files as AC#10
- Expected: Exactly 3 matches total (all in GameInitialization.cs at lines 319, 339, 358)
- Pre-existing TODOs: GameInitialization.cs contains 3 TODOs for Phase 22 State Systems - these are the documented baseline
- Verification: Count must remain exactly 3. If count > 3, new debt was added. If count < 3, pre-existing TODOs were modified (unexpected).

**AC#12**: All tests PASS
- Test: `dotnet test`
- Expected: Exit code 0, all tests passing
- Verifies refactoring preserves functionality

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Migrate AddCharaHandler.cs to Primary Constructor | [x] |
| 2 | 2 | Migrate SetColorHandler.cs to Primary Constructor | [x] |
| 3 | 3 | Migrate SaveGameHandler.cs to Primary Constructor | [x] |
| 4 | 4 | Migrate GameInitialization.cs to Primary Constructor | [x] |
| 5 | 5 | Migrate InfoPrint.cs to Primary Constructor | [x] |
| 6 | 6 | Migrate CharacterSetup.cs to Primary Constructor | [x] |
| 7 | 7 | Migrate VariableResolver.cs to Primary Constructor | [x] |
| 8 | 8 | Migrate CharacterFunctions.cs to Primary Constructor | [x] |
| 9 | 9 | Migrate RandomFunctions.cs to Primary Constructor | [x] |
| 10 | 10,11 | Verify DI field declarations removed (AC#10=0) and TODO baseline unchanged (AC#11=3) | [x] |
| 11 | 12 | Run all tests and verify PASS | [x] |

<!-- AC:Task alignment: 12 ACs → 11 Tasks (Tasks 1-9 are 1:1, Task 10 combines AC 10+11, Task 11 covers AC 12) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Primary Constructor Pattern

**Before**:
```csharp
public class AddCharaHandler
{
    private readonly ICharacterManager _characterManager;

    public AddCharaHandler(ICharacterManager characterManager)
    {
        _characterManager = characterManager;
    }

    public void Handle()
    {
        _characterManager.DoSomething();
    }
}
```

**After**:
```csharp
public class AddCharaHandler(ICharacterManager characterManager)
{
    public void Handle()
    {
        characterManager.DoSomething();
    }
}
```

**Key Points**:
1. Move constructor parameters to class declaration parentheses
2. Remove field declarations
3. Remove explicit constructor
4. Update member access from `_field` to `parameter` (parameter names use camelCase of interface name without "I" prefix: e.g., ICharacterManager → characterManager)
5. Preserve all method bodies and member implementations

**Null Validation Note**: Primary Constructor migration removes explicit `ArgumentNullException` checks from constructors (both traditional `?? throw` pattern and `ThrowIfNull()` calls like in RandomFunctions.cs). This is intentional - DI container guarantees non-null dependencies at registration time. Defensive null checks in constructors are redundant with proper DI configuration and add unnecessary boilerplate.

### Migration Source Reference

| File | Directory | Class Count | Primary Constructor Parameters |
|------|-----------|:-----------:|-------------------------------|
| AddCharaHandler.cs | Commands/System | 5 | ICharacterManager characterManager |
| SetColorHandler.cs | Commands/System | 5 | IStyleManager styleManager |
| SaveGameHandler.cs | Commands/System | 5 | IGameState gameState |
| GameInitialization.cs | Common | 1 | IBodySettings bodySettings, IPregnancySettings pregnancySettings, IWeatherSettings weatherSettings, INtrInitializer ntrInitializer |
| InfoPrint.cs | Common | 1 | ICommonFunctions commonFunctions |
| CharacterSetup.cs | Common | 1 | ICommonFunctions commonFunctions |
| VariableResolver.cs | Variables | 1 | IVariableDefinitionLoader loader |
| CharacterFunctions.cs | Functions | 1 | ICharacterDataAccess dataAccess |
| RandomFunctions.cs | Functions | 1 | IRandom random |

**Note**: Commands/System files each contain 5 handler classes sharing the same DI dependency:
- AddCharaHandler.cs: AddCharaHandler, DelCharaHandler, PickupCharaHandler, SwapCharaHandler, AddCopyCharaHandler
- SetColorHandler.cs: SetColorHandler, SetBgColorHandler, SetFontHandler, ResetColorHandler, AlignmentHandler
- SaveGameHandler.cs: SaveGameHandler, LoadGameHandler, QuitHandler, RestartHandler, ResetDataHandler

### Verification Steps

1. **Before migration**: Run `dotnet test` to establish baseline
2. **Per file**:
   - Migrate to primary constructor
   - Verify compilation success
   - Check AC pattern match (Grep)
3. **After all migrations**: Run `dotnet test` again and verify identical results

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-16 FL iter1**: [applied] Phase2-Validate - AC#7,8,10,11,12,14: 6 classes removed from scope (parameterless constructors, NOT DI). ACs renumbered 1-12.
- **2026-01-16 FL iter1**: [applied] Phase2-Validate - AC#10 scope: Updated to "private readonly I" pattern on DI-migrated files only.
- **2026-01-16 FL iter1**: [applied] Phase2-Validate - Summary/Goal: Updated file count (15→9) and impact estimate (~80→~54 lines).
- **2026-01-16 FL iter1**: [applied] Phase3-Maintainability - AC#11: Pre-existing TODOs in GameInitialization.cs acknowledged as Phase 22 baseline (documented in AC Details).
- **2026-01-16 FL iter2**: [applied] Summary/Goal: Updated to accurate counts (24 DI fields across 24 classes in 9 files, ~48 lines impact). Added note about multi-class files in Commands/System.
- **2026-01-16 FL iter2**: [applied] AC#10: Changed to count_equals=0 matcher with explanation of before/after counts. Migration Source Reference updated with class counts.
- **2026-01-16 FL iter3**: [applied] AC#1-9: Changed patterns to `class ClassName\(` to specifically detect Primary Constructor syntax (fails on traditional constructor). AC#1-3 use count_equals=5 for multi-class files.
- **2026-01-16 FL iter4**: [applied] Implementation Contract: Added Null Validation Note documenting intentional removal of ArgumentNullException checks (DI container guarantees non-null).
- **2026-01-16 FL iter5**: [applied] AC#10/AC#11: Fixed Expected column format (removed embedded patterns). AC#11 changed to count_equals=3 for baseline-aware TODO checking. AC Details updated with exact file paths.

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

No deferred tasks identified at PROPOSED stage.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F503 | Phase 16 Planning must complete first |
| Related | F509-F512 | Other Primary Constructor migration features (Training, Character, Commands/Flow, Commands/Special) |
| Related | F514 | Collection Expression Migration (Phase 16 scope) |
| Successor | F515 | Phase 16 Post-Phase Review |

---

## Links

- [feature-503.md](feature-503.md) - Phase 16 Planning (parent feature)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 16 definition
- [feature-509.md](feature-509.md) - Primary Constructor Migration - Training/ (related)
- [feature-510.md](feature-510.md) - Primary Constructor Migration - Character/ (related)
- [feature-511.md](feature-511.md) - Primary Constructor Migration - Commands/Flow/ (related)
- [feature-512.md](feature-512.md) - Primary Constructor Migration - Commands/Special/ (related)
- [feature-514.md](feature-514.md) - Collection Expression Migration (related)
- [feature-515.md](feature-515.md) - Phase 16 Post-Phase Review (successor)

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-16 | create | feature-builder | Created from F503 Phase 16 Planning | PROPOSED |
| 2026-01-16 | START | initializer | Initialize Feature 513 | - |
| 2026-01-16 | END | initializer | Status → [WIP] | SUCCESS |
| 2026-01-16 | START | implementer | Task 1-11 | - |
| 2026-01-16 | END | implementer | 9 files migrated, 21 classes | SUCCESS |
| 2026-01-16 | START | ac-tester | AC#1-12 verification | - |
| 2026-01-16 | END | ac-tester | All AC PASS | SUCCESS |
| 2026-01-16 | START | feature-reviewer | Mode: post | - |
| 2026-01-16 | END | feature-reviewer | READY | SUCCESS |
| 2026-01-16 | START | feature-reviewer | Mode: doc-check | - |
| 2026-01-16 | END | feature-reviewer | READY (no SSOT updates needed) | SUCCESS |
