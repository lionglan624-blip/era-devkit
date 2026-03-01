# Feature 422: Type Conversion & Casting

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

Implement type conversion and casting system for expression evaluation with Result type error handling.

Part of Phase 8: Expression & Function System implementation.

---

## Background

### Philosophy (Mid-term Vision)

**Expression & Function System**: Migrate ERB expression evaluation to C# with strong typing and error handling. Type Conversion ensures safe and explicit conversion between Int/String/Bool per ERB semantics, avoiding runtime crashes and maintaining legacy behavior equivalence.

### Problem (Current Issue)

Expression evaluation requires type conversion between Int, String, and Bool types:
- Implicit conversions in ERB must be made explicit in C#
- Type mismatches need error handling with Result type
- Conversion failures must provide clear error messages
- Legacy ERB behavior must be preserved exactly

### Goal (What to Achieve)

Implement type conversion system that:
1. Converts between Int/String/Bool per ERB semantics
2. Uses Result type for internal errors (null inputs); type mismatches follow ERB defaults
3. Provides ITypeConverter interface with DI registration
4. Maintains legacy behavior equivalence
5. Zero technical debt (no TODO/FIXME/HACK comments)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ITypeConverter interface exists | file | Glob | exists | Era.Core/Types/ITypeConverter.cs | [x] |
| 2 | TypeConverter implementation exists | file | Glob | exists | Era.Core/Types/TypeConverter.cs | [x] |
| 3 | Int to String conversion works | test | unit | succeeds | Era.Core.Tests | [x] |
| 4 | String to Int conversion works | test | unit | succeeds | Era.Core.Tests | [x] |
| 5 | Bool to Int conversion works | test | unit | succeeds | Era.Core.Tests | [x] |
| 6 | Int to Bool conversion works | test | unit | succeeds | Era.Core.Tests | [x] |
| 7 | String to Bool conversion works | test | unit | succeeds | Era.Core.Tests | [x] |
| 8 | Conversion edge cases and error handling | test | unit | succeeds | Era.Core.Tests | [x] |
| 9 | DI registration configured | file | Grep | contains | "ITypeConverter" | [x] |
| 10 | ERB edge cases verified | test | unit | succeeds | Era.Core.Tests | [x] |
| 11 | Build succeeds | build | dotnet build | succeeds | - | [x] |
| 12 | Unit tests pass | test | dotnet test | succeeds | - | [x] |
| 13 | Bool to String conversion works | test | unit | succeeds | Era.Core.Tests | [x] |
| 14 | 負債ゼロ (no TODO/FIXME) | file | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |

### AC Details

**AC#1**: Glob for `Era.Core/Types/ITypeConverter.cs` file existence

**AC#2**: Glob for `Era.Core/Types/TypeConverter.cs` file existence

**AC#3**: Unit test verifies Int to String conversion (e.g., 123 → "123")
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TypeConverter.IntToString"`

**AC#4**: Unit test verifies String to Int conversion per legacy TOINT semantics
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TypeConverter.StringToInt"`
- ERB TOINT semantics (reference: Creator.Method.cs lines 2353-2383):
  - `null` or `""` → 0 (not Result.Fail)
  - Full-width characters → 0
  - Non-digit start (except +/-) → 0
  - Strings with trailing non-digits after the number → 0 (e.g., "123abc" → 0)
  - Decimal numbers accepted (e.g., "123.45" → 123)
  - Valid integer strings → parsed value

**AC#5**: Unit test verifies Bool to Int conversion (e.g., true → 1, false → 0)
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TypeConverter.BoolToInt"`

**AC#6**: Unit test verifies Int to Bool conversion (e.g., 0 → false, non-zero → true)
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TypeConverter.IntToBool"`

**AC#7**: Unit test verifies String to Bool conversion (e.g., "0" → false, "" → false, "1" → true, "abc" → true)
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TypeConverter.StringToBool"`

**AC#8**: Unit test verifies conversion edge cases and internal error handling
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TypeConverter.EdgeCases"`
- Note: Per legacy ERB semantics, type mismatches return default values (0 for int, "" for string) rather than Result.Fail
- Result.Fail is reserved for internal errors (e.g., null input to methods that require non-null)

**AC#9**: Grep for "ITypeConverter" in DI registration
- File: `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`
- Pattern: `services.AddSingleton<ITypeConverter, TypeConverter>`
- Location: Add registration inside `AddEraCore()` method

**AC#10**: Unit test verifies ERB conversion edge cases beyond individual AC#3-7 tests
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TypeConverter.ErbSemantics"`
- Focus: Cross-type conversion chains (e.g., Bool → Int → String roundtrip) and ERB-specific edge cases not covered by individual tests
- ERB edge cases (reference: Creator.Method.cs TOSTR/TOINT functions):
  - Overflow handling for large values
  - Leading/trailing whitespace → 0 (non-digit start)
  - String to Bool: any non-empty, non-"0" string is truthy (case irrelevant)

**AC#11**: C# build succeeds
- Command: `dotnet build Era.Core/Era.Core.csproj`

**AC#12**: All unit tests pass
- Command: `dotnet test Era.Core.Tests/Era.Core.Tests.csproj`

**AC#13**: Unit test verifies Bool to String conversion (e.g., true → "1", false → "0")
- Test: `dotnet test Era.Core.Tests --filter "FullyQualifiedName~TypeConverter.BoolToString"`
- Used by F417 string concat operator

**AC#14**: Grep for TODO/FIXME/HACK in Era.Core/Types/ files
- Pattern: `TODO|FIXME|HACK`
- Expected: No matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Define ITypeConverter interface with conversion method signatures | [x] |
| 2 | 2 | Implement TypeConverter with Int/String/Bool conversion methods | [x] |
| 3 | 3 | Write unit test for Int to String conversion | [x] |
| 4 | 4 | Write unit test for String to Int conversion | [x] |
| 5 | 5 | Write unit test for Bool to Int conversion | [x] |
| 6 | 6 | Write unit test for Int to Bool conversion | [x] |
| 7 | 7 | Write unit test for String to Bool conversion | [x] |
| 8 | 8 | Write unit test for edge cases and error handling | [x] |
| 9 | 9 | Register ITypeConverter in DI container | [x] |
| 10 | 10 | Write ERB edge case tests (roundtrips, overflow, whitespace) | [x] |
| 11 | 11 | Verify build succeeds | [x] |
| 12 | 12 | Verify all unit tests pass | [x] |
| 13 | 13 | Write unit test for Bool to String conversion | [x] |
| 14 | 14 | Verify no TODO/FIXME/HACK comments in implementation | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Implementation Steps

**Step 1: Interface Definition**
- Create `Era.Core/Types/ITypeConverter.cs`
- Define conversion methods returning Result<T>:
  - `Result<string> ToStringValue(object value)`
  - `Result<long> ToIntValue(object value)`
  - `Result<bool> ToBoolValue(object value)`

**Step 2: Implementation**
- Create `Era.Core/Types/TypeConverter.cs`
- Null handling: null input returns Result.Fail (internal error).
- Empty string ("") handling: Valid input, returns 0 for ToInt (not Result.Fail) per ERB TOINT semantics.
- Implement ERB conversion semantics:
  - Int to String: Standard .ToString()
  - String to Int: Legacy TOINT semantics (see AC#4 Details)
  - Bool to Int: true → 1, false → 0
  - Int to Bool: 0 → false, non-zero → true
  - String to Bool: "0" or empty → false, otherwise true
  - Bool to String: true → "1", false → "0" (included for string concat operator in F417)

**Step 3: Unit Tests**
- Create `Era.Core.Tests/Types/TypeConverterTests.cs`
- Test all conversion paths (6 combinations)
- Test failure cases with Result.Fail verification
- Test ERB semantics per Creator.Method.cs TOSTR/TOINT patterns

**Step 4: DI Registration**
- Register in ServiceCollectionExtensions.cs or equivalent
- Verify injection works in expression evaluator

**Step 5: Technical Debt Cleanup**
- Remove all TODO/FIXME/HACK comments
- Ensure all code is production-ready

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F409 | Phase 8 Planning feature (creates this feature) |
| Related | F377 | Phase 4 Design Principles (Result type, DI patterns) |
| Related | F416 | ExpressionParser - both are Phase 8 Expression System features |
| Successor | F417 | Operator Implementation - depends on type conversion |
| Successor | F423 | Phase 8 Post-Phase Review |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 8 definition (line 2002)
- [feature-409.md](feature-409.md) - Phase 8 Planning (creates F422)
- [feature-377.md](feature-377.md) - Phase 4 Design Principles (Result type)
- [feature-416.md](feature-416.md) - ExpressionParser (related)
- [feature-417.md](feature-417.md) - Operator Implementation (related)
- [feature-423.md](feature-423.md) - Phase 8 Post-Phase Review (successor)

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD FL iter{N}**: [{status}] {location} - {issue summary} -->
- **2026-01-09 FL iter1**: [resolved] AC#9 - Clarified DI registration location in AC#9 Details (iter2)
- **2026-01-09 FL iter1**: [resolved] Interface Definition - F377 does not mandate type-specific overloads; object type is acceptable for converter utility (validated iter2)
- **2026-01-09 FL iter4**: [resolved] Task#12 - Keep separate per AC:Task 1:1 rule; Task#12 correctly maps to AC#13
- **2026-01-09 FL iter7**: [resolved] Task#11 - Split into Task#11 (AC#11) and Task#12 (AC#12) per 1:1 rule (iter8)
- **2026-01-09 FL iter8**: [resolved] Bool to String - Added AC#13 and Task#13 per user decision

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 | create | implementer | Created as Phase 8 sub-feature per F409 Task 8 | PROPOSED |
| 2026-01-09 18:17 | START | implementer | Task 3-8, 10, 13 - TDD Test Creation (RED state) | - |
| 2026-01-09 18:17 | END | implementer | Task 3-8, 10, 13 - Created TypeConverterTests.cs | SUCCESS |
| 2026-01-09 18:22 | START | implementer | Task 1, 2, 9 - Type conversion implementation | - |
| 2026-01-09 18:22 | END | implementer | Task 1, 2, 9 - Created ITypeConverter, TypeConverter, DI registration | SUCCESS |
| 2026-01-09 18:23 | DEVIATION | debugger | StringToInt semantics bug (trailing non-digit, whitespace) | FIXED |
| 2026-01-09 18:25 | END | ac-tester | All 8 TypeConverter tests pass | SUCCESS |
| 2026-01-09 18:26 | END | feature-reviewer | Post-review mode: doc-check required SSOT update | NEEDS_REVISION |
| 2026-01-09 18:27 | END | opus | Updated engine-dev SKILL.md line 89 with TypeConverter | SUCCESS |
