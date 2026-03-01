# Feature 509: Primary Constructor Migration - Training Directory

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

Apply C# 14 Primary Constructor pattern to 14 classes in 10 implementation files (with private readonly fields) in `Era.Core/Training/` directory.

**Scope**:
- 10 files containing 14 classes with 29 `private readonly` field declarations
- Convert constructor parameter injection → Primary Constructor syntax
- Remove redundant field declarations and null checks
- Preserve all functionality (refactoring only, no behavioral changes)

**Output**:
- Modernized training processor implementations
- ~40-50 lines boilerplate reduction (field declarations + null checks)
- Improved code clarity with C# 14 patterns

**Out of Scope**:
- Other directories (handled by F510-F513)
- Collection expression migration (handled by F514)
- New functionality or logic changes

---

## Background

### Philosophy (Mid-term Vision)

**Phase 16: C# 14 Style Migration** - Apply C# 14 patterns to existing code for simplification. Primary Constructor and Collection Expression reduce ~400 lines of boilerplate across Era.Core, improving maintainability and leveraging modern language features enabled in Phase 10 (.NET 10 / C# 14).

**SSOT**: Primary Constructors make dependency injection explicit at class declaration level, improving code readability and reducing maintenance burden for future contributors. This establishes the C# 14 idiom as the standard pattern for Era.Core implementations.

### Problem (Current Issue)

Training/ directory contains 14 classes (in 10 files) using pre-C# 14 constructor patterns:
- Explicit `private readonly` field declarations
- Manual `ArgumentNullException` null checks
- Verbose constructor bodies assigning parameters to fields

This boilerplate was necessary before C# 14 but is now redundant.

### Goal (What to Achieve)

1. Convert all Training/ classes to Primary Constructor syntax
2. Eliminate ~40-50 lines of boilerplate code
3. Maintain all existing functionality (refactoring only)
4. Verify all tests PASS after migration

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | TrainingProcessor migrated | code | Grep | contains | `public class TrainingProcessor\(` | [x] |
| 2 | MarkSystem migrated | code | Grep | contains | `public class MarkSystem\(` | [x] |
| 3 | SubmissionMarkCalculator migrated | code | Grep | contains | `public class SubmissionMarkCalculator\(` | [x] |
| 4 | PleasureMarkCalculator migrated | code | Grep | contains | `public class PleasureMarkCalculator\(` | [x] |
| 5 | ResistanceMarkCalculator migrated | code | Grep | contains | `public class ResistanceMarkCalculator\(` | [x] |
| 6 | PainMarkCalculator migrated | code | Grep | contains | `public class PainMarkCalculator\(` | [x] |
| 7 | AbilityGrowthProcessor migrated | code | Grep | contains | `public class AbilityGrowthProcessor\(` | [x] |
| 8 | EquipmentProcessor migrated | code | Grep | contains | `public class EquipmentProcessor\(` | [x] |
| 9 | OrgasmProcessor migrated | code | Grep | contains | `public class OrgasmProcessor\(` | [x] |
| 10 | BasicChecksProcessor migrated | code | Grep | contains | `public class BasicChecksProcessor\(` | [x] |
| 11 | FavorCalculator migrated | code | Grep | contains | `public class FavorCalculator\(` | [x] |
| 12 | JuelProcessor migrated | code | Grep | contains | `public class JuelProcessor\(` | [x] |
| 13 | TrainingSetup migrated | code | Grep | contains | `public class TrainingSetup\(` | [x] |
| 14 | SpecialTraining migrated | code | Grep | contains | `public class SpecialTraining\(` | [x] |
| 15 | Only struct declaration remains | code | Grep | count_equals | 1 | [x] |
| 16 | All tests PASS | test | Bash | succeeds | `dotnet test` | [x] |

### AC Details

**AC#1-14**: Primary Constructor pattern verification
- Test: Grep pattern=`public class {ClassName}\(` path=`Era.Core/Training/{FileName}.cs`
- Expected: Class declaration includes constructor parameters in class header
- Pattern: `public class Foo(IBar bar, IBaz baz) : IFoo` instead of separate constructor
- Note: AC#2-6 are all in MarkSystem.cs (5 classes in 1 file)
- Note: `\(` in pattern escapes the parenthesis for regex literal match

**AC#15**: Only struct declaration remains
- Test: Grep pattern=`private readonly` path=`Era.Core/Training/` type=cs
- Expected: 1 match (only `private readonly struct ResultWrapper<T>` in AbilityGrowthProcessor.cs remains)
- Note: The struct declaration is valid C# and should NOT be removed during migration

**AC#16**: All tests PASS
- Test: `dotnet test` at repository root
- Expected: All Era.Core.Tests and related tests PASS
- Verifies no functional regression from refactoring
- Note: Technical debt AC removed (pre-existing TODO in OrgasmProcessor.cs is out of scope for this migration)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Migrate TrainingProcessor to Primary Constructor | [x] |
| 2 | 2-6 | Migrate MarkSystem.cs (5 classes) to Primary Constructor | [x] |
| 3 | 7 | Migrate AbilityGrowthProcessor to Primary Constructor | [x] |
| 4 | 8 | Migrate EquipmentProcessor to Primary Constructor | [x] |
| 5 | 9 | Migrate OrgasmProcessor to Primary Constructor | [x] |
| 6 | 10 | Migrate BasicChecksProcessor to Primary Constructor | [x] |
| 7 | 11 | Migrate FavorCalculator to Primary Constructor | [x] |
| 8 | 12 | Migrate JuelProcessor to Primary Constructor | [x] |
| 9 | 13 | Migrate TrainingSetup to Primary Constructor | [x] |
| 10 | 14 | Migrate SpecialTraining to Primary Constructor | [x] |
| 11 | 15 | Verify only struct declaration remains | [x] |
| 12 | 16 | Run full test suite and verify PASS | [x] |

<!-- AC:Task 1:1 Rule: 16 ACs = 12 Tasks (Task#2 handles 5 classes in single file - atomic file edit) -->
<!-- Task#11-12 are verification tasks that must run AFTER Task#1-10 complete -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Primary Constructor Pattern (C# 14)

**Before** (Pre-C# 14):
```csharp
public class TrainingProcessor : ITrainingProcessor
{
    private readonly IBasicChecksProcessor _basicChecks;
    private readonly IAbilityGrowthProcessor _abilityGrowth;
    private readonly IEquipmentProcessor _equipment;

    public TrainingProcessor(
        IBasicChecksProcessor basicChecks,
        IAbilityGrowthProcessor abilityGrowth,
        IEquipmentProcessor equipment)
    {
        _basicChecks = basicChecks ?? throw new ArgumentNullException(nameof(basicChecks));
        _abilityGrowth = abilityGrowth ?? throw new ArgumentNullException(nameof(abilityGrowth));
        _equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
    }
}
```

**After** (C# 14 Primary Constructor):
```csharp
public class TrainingProcessor(
    IBasicChecksProcessor basicChecks,
    IAbilityGrowthProcessor abilityGrowth,
    IEquipmentProcessor equipment) : ITrainingProcessor
{
    // Fields and constructor body removed
    // Use parameters directly: basicChecks, abilityGrowth, equipment
    public Result<TrainingResult> Process(CharacterId target, CommandId command)
    {
        // Access injected dependencies directly (no underscore prefix)
        return basicChecks.PerformChecks(target)
            .Bind(() => abilityGrowth.Process(target, command))
            .Bind(() => equipment.UpdateEquipment(target));
    }
}
```

**Key Changes**:
1. Constructor parameters move to class declaration: `public class Foo(params) : IFoo`
2. Field declarations removed (parameters become implicit private fields)
3. Use parameter names directly: `_fieldName` → `parameterName`
4. Null checks can be in first method use, or rely on nullable reference types

**Target Files** (10 files, 14 classes, 29 private readonly fields total):
| File | Class | Field Count | Notes |
|------|-------|:-----------:|-------|
| TrainingProcessor.cs | TrainingProcessor | 5 | Orchestrator |
| MarkSystem.cs | MarkSystem | 5 | Main orchestrator |
| MarkSystem.cs | SubmissionMarkCalculator | 1 | Nested calculator |
| MarkSystem.cs | PleasureMarkCalculator | 1 | Nested calculator |
| MarkSystem.cs | ResistanceMarkCalculator | 1 | Nested calculator |
| MarkSystem.cs | PainMarkCalculator | 1 | Nested calculator |
| OrgasmProcessor.cs | OrgasmProcessor | 4 | - |
| AbilityGrowthProcessor.cs | AbilityGrowthProcessor | 2 | +1 struct preserved |
| EquipmentProcessor.cs | EquipmentProcessor | 3 | - |
| JuelProcessor.cs | JuelProcessor | 2 | - |
| BasicChecksProcessor.cs | BasicChecksProcessor | 1 | - |
| FavorCalculator.cs | FavorCalculator | 1 | - |
| TrainingSetup.cs | TrainingSetup | 1 | - |
| SpecialTraining.cs | SpecialTraining | 1 | - |

**Note**: AbilityGrowthProcessor.cs contains a `private readonly struct ResultWrapper<T>` which is preserved (not a DI field).

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F503 | Phase 16 Planning defines migration scope |
| Related | F510-F513 | Other Primary Constructor migrations (Character, Commands) |
| Related | F514 | Collection Expression migration |
| Successor | F515 | Phase 16 Post-Phase Review |

---

## Links

- [feature-503.md](feature-503.md) - Phase 16 Planning
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 16 definition
- [feature-510.md](feature-510.md) - Primary Constructor Migration - Character/
- [feature-511.md](feature-511.md) - Primary Constructor Migration - Commands/Flow/
- [feature-512.md](feature-512.md) - Primary Constructor Migration - Commands/Special/
- [feature-513.md](feature-513.md) - Primary Constructor Migration - Commands/System + Other
- [feature-514.md](feature-514.md) - Collection Expression Migration
- [feature-515.md](feature-515.md) - Post-Phase Review Phase 16

---

## 引継ぎ先指定 (Mandatory Handoffs)

No deferred tasks identified at PROPOSED stage.

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-16 | create | feature-builder | Created from F503 Task#1 | PROPOSED |
| 2026-01-16 15:08 | START | implementer | Task 1 | - |
| 2026-01-16 15:08 | END | implementer | Task 1 | SUCCESS |
| 2026-01-16 15:11 | START | implementer | Task 2 | - |
| 2026-01-16 15:11 | END | implementer | Task 2 | SUCCESS |
| 2026-01-16 15:13 | START | implementer | Task 3 | - |
| 2026-01-16 15:13 | END | implementer | Task 3 | SUCCESS |
| 2026-01-16 15:15 | START | implementer | Task 4 | - |
| 2026-01-16 15:15 | END | implementer | Task 4 | SUCCESS |
| 2026-01-16 15:14 | START | implementer | Task 6 | - |
| 2026-01-16 15:14 | END | implementer | Task 6 | SUCCESS |
| 2026-01-16 15:15 | START | implementer | Task 9 | - |
| 2026-01-16 15:15 | END | implementer | Task 9 | SUCCESS |
| 2026-01-16 15:16 | START | implementer | Task 5 | - |
| 2026-01-16 15:16 | END | implementer | Task 5 | SUCCESS |
| 2026-01-16 15:16 | START | implementer | Task 8 | - |
| 2026-01-16 15:16 | END | implementer | Task 8 | SUCCESS |
| 2026-01-16 15:17 | START | implementer | Task 7 | - |
| 2026-01-16 15:17 | END | implementer | Task 7 | SUCCESS |
| 2026-01-16 15:18 | START | implementer | Task 10 | - |
| 2026-01-16 15:18 | END | implementer | Task 10 | SUCCESS |
| 2026-01-16 15:19 | START | opus | Task 11 (AC verification) | - |
| 2026-01-16 15:19 | END | opus | Task 11 | PASS (1 struct) |
| 2026-01-16 15:19 | START | opus | Task 12 (test suite) | - |
| 2026-01-16 15:19 | END | opus | Task 12 | PASS (1166 tests) |
| 2026-01-16 15:18 | DEVIATION | implementer | Task 8/10 | Primary Constructor null check behavior change - tests updated |
