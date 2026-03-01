# Feature 517: Phase 16 Null Validation Fix

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

Add explicit null validation to all Primary Constructor migrations from Phase 16 (F509, F511, F512, F513). This ensures Fail-Fast behavior with clear ArgumentNullException messages at construction time.

**Scope**: 4 directories, 46 files, 69 classes, 93 DI-injected fields

| Source Feature | Directory | Files | Classes | Fields |
|----------------|-----------|:-----:|:-------:|:------:|
| F509 | Era.Core/Training/ | 10 | 14 | 29 |
| F511 | Era.Core/Commands/Flow/ | 11 | 18 | 24 |
| F512 | Era.Core/Commands/Special/ | 16 | 16 | 16 |
| F513 | Era.Core/Commands/System/ + Other | 9 | 21 | 24 |

**Output**: Updated `.cs` files with field initializer null validation pattern.

**Volume Waiver**: This feature exceeds ~300 lines due to mechanical transformation across 46 files. All changes follow identical pattern (add field declaration with null validation). Atomicity required for consistency with F510 pattern decision.

---

## Background

### Philosophy (Mid-term Vision)

Phase 16: C# 14 Style Migration with defensive programming. Primary Constructor should maintain explicit null validation for Fail-Fast behavior and clear debugging. This establishes a consistent pattern across all Era.Core DI classes: explicit null checks at construction time, not deferred to first use.

**SSOT**: Field initializer null validation (`private readonly IFoo _foo = foo ?? throw new ArgumentNullException(nameof(foo));`) is the single recognized pattern for Primary Constructor with DI dependencies. This pattern is documented in csharp-14 SKILL.

### Problem (Current Issue)

F509, F511, F512, F513 migrated to Primary Constructor but removed null validation, relying on NRT and DI container. This creates:
1. Silent null propagation until first use
2. NullReferenceException instead of ArgumentNullException
3. Inconsistency with F510 pattern that retains null validation
4. Harder debugging (stack trace points to usage, not injection)

### Goal (What to Achieve)

1. Add field initializer null validation to all 93 DI-injected fields
2. Maintain Primary Constructor syntax
3. Ensure Fail-Fast behavior at construction time
4. Verify all tests pass (no functional changes)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Training/ has null validation | code | Grep | count_equals | 29 | [x] |
| 2 | Commands/Flow/ has null validation | code | Grep | count_equals | 24 | [x] |
| 3 | Commands/Special/ has null validation | code | Grep | count_equals | 16 | [x] |
| 4 | Commands/System/ has null validation | code | Grep | count_equals | 15 | [x] |
| 5 | Common/ has null validation | code | Grep | count_equals | 6 | [x] |
| 6 | Variables/ has null validation | code | Grep | count_equals | 1 | [x] |
| 7 | Functions/ has null validation | code | Grep | count_equals | 2 | [x] |
| 8 | Primary Constructor syntax preserved | code | Grep | count_equals | 69 | [x] |
| 9 | Tech debt baseline maintained (4 pre-existing) | code | Grep | count_equals | 4 | [x] |
| 10 | All tests PASS | test | Bash | succeeds | dotnet test | [x] |

### AC Details

**AC#1**: Training/ null validation (F509 scope)
- Test: Grep pattern=`ArgumentNullException` path=`Era.Core/Training/` type=cs | count
- Expected: 29 matches (one per DI-injected field)

**AC#2**: Commands/Flow/ null validation (F511 scope)
- Test: Grep pattern=`ArgumentNullException` path=`Era.Core/Commands/Flow/` type=cs | count
- Expected: 24 matches

**AC#3**: Commands/Special/ null validation (F512 scope)
- Test: Grep pattern=`ArgumentNullException` path=`Era.Core/Commands/Special/` type=cs | count
- Expected: 16 matches

**AC#4**: Commands/System/ null validation
- Test: Grep pattern=`ArgumentNullException` path=`Era.Core/Commands/System/` type=cs | count
- Expected: 15 matches (5 handlers × 3 files: SetColorHandler.cs, SaveGameHandler.cs, AddCharaHandler.cs)

**AC#5**: Common/ null validation (target files only)
- Test: Grep pattern=`ArgumentNullException` paths=[Era.Core/Common/GameInitialization.cs, Era.Core/Common/InfoPrint.cs, Era.Core/Common/CharacterSetup.cs] type=cs | count
- Expected: 6 matches (GameInitialization 4 + InfoPrint 1 + CharacterSetup 1)
- Note: Directory has 53 pre-existing ArgumentNullException in other files; this AC targets only F517 scope files

**AC#6**: Variables/ null validation
- Test: Grep pattern=`ArgumentNullException` path=`Era.Core/Variables/VariableResolver.cs` type=cs | count
- Expected: 1 match

**AC#7**: Functions/ null validation (target files only)
- Test: Grep pattern=`ArgumentNullException` paths=[Era.Core/Functions/CharacterFunctions.cs, Era.Core/Functions/RandomFunctions.cs] type=cs | count
- Expected: 2 matches (CharacterFunctions 1 + RandomFunctions 1)
- Note: FunctionRegistry.cs has 1 pre-existing ArgumentNullException; this AC targets only F517 scope files

**AC#8**: Primary Constructor syntax preserved
- Test: Grep pattern=`public (?:sealed )?class \w+\(` paths=[Era.Core/Training/, Era.Core/Commands/Flow/, Era.Core/Commands/Special/, Era.Core/Commands/System/, Era.Core/Common/, Era.Core/Variables/, Era.Core/Functions/] type=cs | count
- Expected: 69 matches (all migrated classes in F517 scope retain Primary Constructor)
- Note: Pattern matches both `public class` and `public sealed class` declarations. Path restricted to F517 scope (excludes Character/ which is F510)

**AC#9**: Tech debt baseline maintained (pre-existing 4 TODOs)
- Pattern: `TODO|FIXME|HACK`
- Paths: All directories in scope
- Expected: 4 matches total
  - OrgasmProcessor.cs: 1 pre-existing TODO
  - GameInitialization.cs: 3 pre-existing TODOs (Phase 22 State Systems)
- Verification: Count must remain exactly 4. If > 4, new debt added.

**AC#10**: All tests PASS
- Test: `dotnet test`
- Expected: All tests pass, no functional changes

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Add null validation to Training/ (10 files, 29 fields) | [x] |
| 2 | 2 | Add null validation to Commands/Flow/ (11 files, 24 fields) | [x] |
| 3 | 3 | Add null validation to Commands/Special/ (16 files, 16 fields) | [x] |
| 4 | 4 | Add null validation to Commands/System/ (3 files, 15 fields) | [x] |
| 5 | 5 | Add null validation to Common/ (3 files, 6 fields) | [x] |
| 6 | 6 | Add null validation to Variables/ (1 file, 1 field) | [x] |
| 7 | 7 | Add null validation to Functions/ (2 files, 2 fields) | [x] |
| 8 | 8 | Verify Primary Constructor syntax preserved (69 classes) | [x] |
| 9 | 9,10 | Verify zero NEW tech debt and all tests PASS | [x] |

<!-- AC:Task 1:1 Rule: 10 ACs = 9 Tasks. Task#9 batches AC#9+10 per F384 precedent (verification tasks). -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Migration Pattern

Add field declarations with null validation to existing Primary Constructor classes:

**Before (Current - F509/F511/F512/F513 pattern)**:
```csharp
public class BasicChecksProcessor(IVariableStore variableStore) : IBasicChecksProcessor
{
    // No field declaration, uses parameter directly
    public Result Check(CharacterId target) => variableStore.Get(target);
}
```

**After (With Null Validation)**:
```csharp
public class BasicChecksProcessor(IVariableStore variableStore) : IBasicChecksProcessor
{
    private readonly IVariableStore _variableStore =
        variableStore ?? throw new ArgumentNullException(nameof(variableStore));

    public Result Check(CharacterId target) => _variableStore.Get(target);
}
```

### Method Body Updates

Methods must be updated to use `_fieldName` instead of `parameterName`:
- `variableStore` → `_variableStore`
- `trainingVariables` → `_trainingVariables`
- `scopeManager` → `_scopeManager`
- etc.

### Field Reference Table

| Directory | Parameter Pattern | Field Pattern |
|-----------|------------------|---------------|
| Training/ | `variableStore` | `_variableStore` |
| Training/ | `trainingVariables` | `_trainingVariables` |
| Commands/Flow/ | `scopeManager` | `_scopeManager` |
| Commands/Flow/ | `labelResolver` | `_labelResolver` |
| Commands/Flow/ | `executionStack` | `_executionStack` |
| Commands/Special/ | `specialTraining` | `_specialTraining` |
| Commands/System/ | `characterManager` | `_characterManager` |
| Commands/System/ | `styleManager` | `_styleManager` |
| Commands/System/ | `gameState` | `_gameState` |

---

## 引継ぎ先指定 (Mandatory Handoffs)

No deferred tasks. This is a fix feature for F509, F511, F512, F513.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Fixes | F509 | Adds null validation to Training/ |
| Fixes | F511 | Adds null validation to Commands/Flow/ |
| Fixes | F512 | Adds null validation to Commands/Special/ |
| Fixes | F513 | Adds null validation to Commands/System + Other |
| Related | F510 | Same null validation pattern (reference implementation) |
| Successor | F515 | Phase 16 Post-Phase Review |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-16**: Volume waiver granted - 93 fields across 46 files requires atomicity for pattern consistency.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-16 | create | orchestrator | Created to fix F509 null validation | PROPOSED |
| 2026-01-16 | expand | orchestrator | Expanded to consolidate F509/F511/F512/F513 fixes (B案) | PROPOSED |
| 2026-01-16 18:06 | END | implementer | Task 2 | SUCCESS |
| 2026-01-16 18:10 | END | implementer | Task 3 | SUCCESS |
| 2026-01-16 18:13 | END | implementer | Task 4 | SUCCESS |
| 2026-01-16 18:16 | END | implementer | Task 5 | SUCCESS |
| 2026-01-16 18:17 | END | implementer | Task 6 | SUCCESS |
| 2026-01-16 18:20 | END | implementer | Task 7 | SUCCESS |
| 2026-01-16 18:25 | DEVIATION | orchestrator | AC#10 dotnet test | retry (skill未参照) |
| 2026-01-16 18:25 | END | orchestrator | Phase 6 AC Verification | 10/10 PASS |

---

## Links

- [feature-384.md](feature-384.md) - AC:Task 1:1 precedent (verification task batching)
- [feature-509.md](feature-509.md) - F509 Training/ migration (being fixed)
- [feature-510.md](feature-510.md) - F510 Commands/Print migration (first feature using null validation pattern)
- [feature-511.md](feature-511.md) - F511 Commands/Flow/ migration (being fixed)
- [feature-512.md](feature-512.md) - F512 Commands/Special/ migration (being fixed)
- [feature-513.md](feature-513.md) - F513 Commands/System + Other migration (being fixed)
- [feature-515.md](feature-515.md) - Phase 16 Post-Phase Review
- [csharp-14 SKILL](../../.claude/skills/csharp-14/SKILL.md) - Primary Constructor reference
