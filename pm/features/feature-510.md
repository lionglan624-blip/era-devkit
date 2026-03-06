# Feature 510: Primary Constructor Migration - Character Directory

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

Migrate 4 files in `Era.Core/Character/` directory from traditional constructor pattern to C# 14 Primary Constructor pattern with explicit null validation, eliminating ~15-20 lines of boilerplate code while maintaining runtime safety.

**Target Files**:
- `ExperienceGrowthCalculator.cs` (5 private readonly fields)
- `CharacterStateTracker.cs` (3 private readonly fields)
- `VirginityManager.cs` (2 private readonly fields)
- `PainStateChecker.cs` (1 private readonly field)

**Output**: Updated `.cs` files with Primary Constructor syntax following C# 14 pattern from csharp-14 SKILL.

**Volume**: ~15-20 lines reduced (4 constructor signatures + 4 constructor bodies removed; 11 field declarations retained with inline null validation), refactoring only, no functional changes.

---

## Background

### Philosophy (Mid-term Vision)

Phase 16: C# 14 Style Migration - Apply C# 14 patterns to existing code for simplification. Primary Constructor and Collection Expression reduce ~400 lines of boilerplate across Era.Core, improving maintainability and readability while preserving all functionality.

### Problem (Current Issue)

Character directory classes use verbose traditional constructor pattern with explicit field declarations and assignments:
- ExperienceGrowthCalculator: 5 readonly fields + constructor assignments
- CharacterStateTracker: 3 readonly fields + constructor assignments
- VirginityManager: 2 readonly fields + constructor assignments
- PainStateChecker: 1 readonly field + constructor assignment

This creates unnecessary boilerplate that Primary Constructor eliminates.

### Goal (What to Achieve)

1. Migrate 4 Character directory files to Primary Constructor pattern
2. Eliminate constructor body assignments (retain field declarations with inline null validation)
3. Preserve all dependency injection patterns with explicit runtime null safety
4. Verify test suite still passes (no functional changes)
5. Track technical debt (no TODO/FIXME/HACK added)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ExperienceGrowthCalculator uses primary constructor | code | Grep | contains | `public class ExperienceGrowthCalculator\(` | [x] |
| 2 | CharacterStateTracker uses primary constructor | code | Grep | contains | `public class CharacterStateTracker\(` | [x] |
| 3 | VirginityManager uses primary constructor | code | Grep | contains | `public class VirginityManager\(` | [x] |
| 4 | PainStateChecker uses primary constructor | code | Grep | contains | `public class PainStateChecker\(` | [x] |
| 5 | All fields have null validation | code | Grep | count_equals | `ArgumentNullException` = 12 | [x] |
| 6 | No constructor body assignments | code | Grep | not_contains | `_[a-z].*=.*\?\? throw` | [x] |
| 7 | Zero technical debt | code | Grep | not_contains | `TODO\|FIXME\|HACK` | [x] |
| 8 | All tests PASS after migration | test | Bash | succeeds | `dotnet test` | [x] |

### AC Details

**AC#1-4**: Primary Constructor pattern verification
- Test: Grep pattern=`public class {ClassName}\(` path=`Era.Core/Character/{ClassName}.cs`
- Expected: Class declaration includes constructor parameters in class header
- Pattern: `public class Foo(IBar bar) : IFoo` instead of separate constructor

**AC#5**: All fields have null validation
- Test: Grep pattern=`ArgumentNullException` path=`Era.Core/Character/` type=cs | count
- Expected: 12 matches (11 field initializers + 1 pre-existing method parameter check in PainStateChecker.CheckPain)
- Pattern: `private readonly IFoo _foo = foo ?? throw new ArgumentNullException(nameof(foo));`

**AC#6**: No constructor body assignments
- Test: Grep pattern=`_[a-z].*=.*\?\? throw` path=`Era.Core/Character/` type=cs
- Expected: 0 matches in constructor bodies (null checks moved to field initializers)
- Note: This pattern matches traditional `_field = param ?? throw` in constructor bodies

**AC#7**: Zero technical debt
- Test: Grep pattern=`TODO|FIXME|HACK` path=`Era.Core/Character/` type=cs
- Expected: 0 matches (no debt markers added during migration)

**AC#8**: All tests PASS after migration
- Test: `dotnet test` (full suite as specified in AC table)
- Expected: All tests pass, no functional changes introduced

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,5,6 | Migrate ExperienceGrowthCalculator to primary constructor with null validation | [x] |
| 2 | 2,5,6 | Migrate CharacterStateTracker to primary constructor with null validation | [x] |
| 3 | 3,5,6 | Migrate VirginityManager to primary constructor with null validation | [x] |
| 4 | 4,5,6 | Migrate PainStateChecker to primary constructor with null validation | [x] |
| 5 | 7 | Verify no technical debt markers added | [x] |
| 6 | 8 | Verify all tests pass | [x] |

<!-- AC:Task 1:1 Rule: 8 ACs = 6 Tasks (AC#5,6 shared by Task 1-4 - each migration adds null validation and removes constructor body) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Migration Pattern

Each file migration follows this pattern (Primary Constructor with Explicit Null Validation):

**Before (Traditional Constructor)**:
```csharp
public class ExampleClass : IExample
{
    private readonly IVariableStore _variableStore;
    private readonly ITrainingVariables _trainingVariables;

    public ExampleClass(
        IVariableStore variableStore,
        ITrainingVariables trainingVariables)
    {
        _variableStore = variableStore ?? throw new ArgumentNullException(nameof(variableStore));
        _trainingVariables = trainingVariables ?? throw new ArgumentNullException(nameof(trainingVariables));
    }
}
```

**After (Primary Constructor with Null Validation)**:
```csharp
public class ExampleClass(
    IVariableStore variableStore,
    ITrainingVariables trainingVariables) : IExample
{
    // Field declarations with inline null validation (Fail-Fast at construction)
    private readonly IVariableStore _variableStore =
        variableStore ?? throw new ArgumentNullException(nameof(variableStore));
    private readonly ITrainingVariables _trainingVariables =
        trainingVariables ?? throw new ArgumentNullException(nameof(trainingVariables));

    // Methods use _fieldName (not camelCase parameter)
}
```

**Key Differences from Simple Primary Constructor**:
1. **Field declarations retained** - explicit `private readonly` with null validation
2. **Constructor body eliminated** - null checks move to field initializers
3. **Field names preserved** - methods continue using `_fieldName` (no rename needed)
4. **Fail-Fast guaranteed** - ArgumentNullException at construction time, not first use

### Field Access Changes

**NO CHANGES REQUIRED**: With explicit field declarations, methods continue using `_fieldName`:

**ExperienceGrowthCalculator.cs** (5 fields): `_variableStore`, `_trainingVariables`, `_getCup`, `_getJuel`, `_juelVariables` - no rename

**CharacterStateTracker.cs** (3 fields): `_virginityManager`, `_experienceCalculator`, `_painChecker` - no rename

**VirginityManager.cs** (2 fields): `_variableStore`, `_tequipVariables` - no rename

**PainStateChecker.cs** (1 field): `_variableStore` - no rename

**Note**: Static nested classes (e.g., `Indices` in PainStateChecker) and static readonly arrays (e.g., `PALAMLV`, `EXPLV`) are NOT affected by primary constructor migration.

### ArgumentNullException Handling

**DECISION**: Keep explicit null validation in field initializers.

**Rationale**:
1. **Fail-Fast**: ArgumentNullException at construction time with clear parameter name
2. **Defensive Programming**: Don't rely solely on DI container guarantees
3. **Debugging**: Clear error messages vs NullReferenceException at random usage point
4. **Testability**: Unit tests can manually instantiate without DI and still get proper validation
5. **Consistency**: All Phase 16 migrations (F509-F513) use this pattern

---

## 引継ぎ先指定 (Mandatory Handoffs)

No deferred tasks identified at PROPOSED stage. All migration work is within scope.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F503 | Phase 16 Planning feature |
| Related | F509 | Primary Constructor Migration - Training directory (same Phase 16 pattern) |
| Related | F511 | Primary Constructor Migration - Commands/Flow directory (same Phase 16 pattern) |
| Successor | F515 | Phase 16 Post-Phase Review (verification) |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-16 FL iter3**: [resolved] ArgumentNullException Handling clarified: Keep explicit null validation in field initializers per user decision. Implementation Contract updated with explicit pattern and rationale.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-16 | create | feature-builder | Created from F503 Phase 16 Planning | PROPOSED |
| 2026-01-16 16:55 | START | implementer | Task 1-6 | - |
| 2026-01-16 16:55 | END | implementer | Task 1-6 | SUCCESS |
| 2026-01-16 | END | ac-tester | AC verification | OK:8/8 |
| 2026-01-16 | DEVIATION | feature-reviewer | Mode: post (wrong for engine type) | IRRELEVANT |
| 2026-01-16 | END | feature-reviewer | Mode: doc-check | READY |
| 2026-01-16 | DEVIATION | opus | AC#5 Expected correction | 11→12 (method param validation) |

---

## Links

- [feature-503.md](feature-503.md) - Phase 16 Planning (parent feature)
- [feature-509.md](feature-509.md) - Primary Constructor Migration - Training directory (related pattern)
- [feature-511.md](feature-511.md) - Primary Constructor Migration - Commands/Flow directory (related pattern)
- [feature-512.md](feature-512.md) - Primary Constructor Migration - Commands/Special directory (related pattern)
- [feature-513.md](feature-513.md) - Primary Constructor Migration - Commands/System + Other (related pattern)
- [feature-515.md](feature-515.md) - Post-Phase Review Phase 16 (successor)
- [csharp-14 SKILL](../../.claude/skills/csharp-14/SKILL.md) - Primary Constructor reference
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 16 definition
