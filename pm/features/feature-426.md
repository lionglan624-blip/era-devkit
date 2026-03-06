# Feature 426: Built-in Functions Value Comparison

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

## Created: 2026-01-09

---

## Summary

Migrate 3 variadic value comparison functions from legacy Creator.Method.cs to Era.Core. These functions were deferred from F419 to maintain feature scope boundaries.

**Output**: Methods in `Era.Core/Functions/ValueComparisonFunctions.cs`

---

## Background

### Philosophy (Mid-term Vision)

**Expression & Function System**: Complete the function migration started in F419. These variadic comparison functions operate on explicit value arguments without game state access, maintaining the "stateless data manipulation" principle.

### Problem (Current Issue)

F419 covers 12 core array expression functions. The remaining 3 variadic comparison functions in Creator.cs (lines 94-96) need migration:
- GROUPMATCH - Count matches (how many args equal first arg)
- NOSAMES - All-different check (1 if first differs from all, else 0)
- ALLSAMES - All-same check (1 if first equals all, else 0)

### Goal (What to Achieve)

1. **Implement 3 variadic value comparison functions**:
   - GROUPMATCH: Count how many of the comparison values equal the first value
   - NOSAMES: Return 1 if first value differs from all others, 0 if any match
   - ALLSAMES: Return 1 if first value equals all others, 0 if any differ

2. **Follow F419 patterns**:
   - Result type for error handling
   - DI registration in FunctionRegistry
   - Interface-based design

3. **Zero technical debt**

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ValueComparisonFunctions.cs exists | file | Glob | exists | "Era.Core/Functions/ValueComparisonFunctions.cs" | [x] |
| 2 | GROUPMATCH Int64 test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestGroupmatchInt64" | [x] |
| 3 | GROUPMATCH String test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestGroupmatchString" | [x] |
| 4 | NOSAMES Int64 test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestNosamesInt64" | [x] |
| 5 | NOSAMES String test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestNosamesString" | [x] |
| 6 | ALLSAMES Int64 test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestAllsamesInt64" | [x] |
| 7 | ALLSAMES String test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestAllsamesString" | [x] |
| 8 | Insufficient args error | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestInsufficientArgs" | [x] |
| 9 | C# build succeeds | build | Bash | succeeds | "dotnet build Era.Core" | [x] |
| 10 | Full equivalence tests | test | Bash | succeeds | "dotnet test --filter Category=ValueComparisonFunctions" | [x] |
| 11 | DI registration | file | Grep | contains | "AddSingleton.*IValueComparisonFunctions" | [x] |
| 12 | Zero technical debt | file | Grep | not_contains | "TODO|FIXME|HACK" | [x] |

### AC Details

**AC#1**: File existence verification
- Test: Glob pattern="Era.Core/Functions/ValueComparisonFunctions.cs"
- Expected: File exists

**AC#8**: Insufficient arguments error handling
- Test: `dotnet test --filter FullyQualifiedName~TestInsufficientArgs`
- Verifies: All 3 functions return `Result.Fail` when comparisons array is empty

**AC#10**: Full equivalence tests
- Test: `dotnet test --filter "Category=ValueComparisonFunctions"`
- Category includes: GROUPMATCH, NOSAMES, ALLSAMES (Int64 + String variants)
- Expected: ~30+ test cases covering edge cases (empty comparisons, single comparison, many comparisons)

**AC#11**: DI registration
- Test: Grep pattern="AddSingleton.*IValueComparisonFunctions" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"
- Expected: `services.AddSingleton<IValueComparisonFunctions, ValueComparisonFunctions>()`

**AC#12**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Functions/ValueComparisonFunctions.cs"
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create ValueComparisonFunctions.cs with interface | [x] |
| 2 | 2 | Implement GROUPMATCH Int64 variant | [x] |
| 3 | 3 | Implement GROUPMATCH String variant | [x] |
| 4 | 4 | Implement NOSAMES Int64 variant | [x] |
| 5 | 5 | Implement NOSAMES String variant | [x] |
| 6 | 6 | Implement ALLSAMES Int64 variant | [x] |
| 7 | 7 | Implement ALLSAMES String variant | [x] |
| 8 | 8 | Implement insufficient args error handling tests | [x] |
| 9 | 9 | Verify C# build succeeds | [x] |
| 10 | 10 | Add [Category("ValueComparisonFunctions")] to tests | [x] |
| 11 | 11 | Register IValueComparisonFunctions in ServiceCollectionExtensions | [x] |
| 12 | 12 | Verify no TODO/FIXME/HACK comments | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Functions (3 total)

**Source (registration)**: `Creator.cs` lines 94-96
**Source (implementation)**: `Creator.Method.cs` lines 1370-1417 (GroupMatchMethod), 1419-1465 (NosamesMethod), 1467-1513 (AllsamesMethod)

| Function | Purpose | Return Type | Types | Source |
|----------|---------|-------------|:-----:|:------:|
| GROUPMATCH | Count matches: how many args equal first arg | int | Int64, String | line 94 |
| NOSAMES | All-different: 1 if first differs from all, else 0 | int | Int64, String | line 95 |
| ALLSAMES | All-same: 1 if first equals all, else 0 | int | Int64, String | line 96 |

### Interface Extension

```csharp
// Create IValueComparisonFunctions for variadic value comparison
// Each function supports both Int64 and String types via method overloading
Result<long> Groupmatch(long value, params long[] comparisons);
Result<long> Groupmatch(string value, params string[] comparisons);
Result<long> Nosames(long value, params long[] comparisons);
Result<long> Nosames(string value, params string[] comparisons);
Result<long> Allsames(long value, params long[] comparisons);
Result<long> Allsames(string value, params string[] comparisons);
```

**Argument Requirements**: All functions require at least 2 arguments (first value + at least one comparison). Return `Result.Fail` if `comparisons` array is empty.

**Type Handling**: Unlike legacy runtime type detection, F426 uses compile-time method overloading. The caller must use the correct overload for their data type.

**Design Rationale**: Method overloading is used instead of separate names (e.g., GroupmatchInt/GroupmatchStr or Groupmatch/Cgroupmatch) because F426 operates on individual values rather than arrays. Overloading is more natural for value comparisons and provides type safety at compile time. This differs from F419 array functions (Match/Cmatch, Sumarray/Sumcarray) where naming conventions distinguish array element types.

**Test Naming Convention**: Test methods follow `Test{FunctionName}{Type}` format (e.g., `TestGroupmatchInt64`, `TestGroupmatchString`). This ensures AC#2-7 filter patterns `FullyQualifiedName~Test{FunctionName}{Type}` match correctly.

**Error Message Format**: When `comparisons` array is empty, return `Result.Fail` with message format: `"{FunctionName}関数には少なくとも2つの引数が必要です"` (e.g., `"GROUPMATCH関数には少なくとも2つの引数が必要です"`).

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IValueComparisonFunctions, ValueComparisonFunctions>();
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F419 | Core array functions must be implemented first |
| Predecessor | F421 | FunctionRegistry must exist |
| Related | F418 | Functions Core pattern reference |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

- **2026-01-09 FL iter2**: [resolved] Type handling documented - F426 uses compile-time overloads (added to Implementation Contract)
- **2026-01-10 FL iter5**: [resolved] Added test naming convention, error message format, DI registration snippet, AC Details path specifications

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 | create | orchestrator | Created as F419 follow-up per FL review | PROPOSED |
| 2026-01-10 05:49 | START | implementer | Task 1-12 | - |
| 2026-01-10 05:49 | END | implementer | Task 1-12 | SUCCESS |

---

## Links

- [feature-419.md](feature-419.md) - Built-in Functions Data (parent feature)
- [feature-421.md](feature-421.md) - Function Call Mechanism
- [feature-418.md](feature-418.md) - Functions Core (pattern reference)
