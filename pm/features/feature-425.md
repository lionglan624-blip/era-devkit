# Feature 425: Built-in Functions String Extended

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

Migrate 5 stateless extended string functions from legacy Creator.Method.cs to Era.Core. These functions were deferred from F419 to maintain feature scope boundaries.

**Output**: `Era.Core/Functions/IStringFunctionsExtended.cs`, `Era.Core/Functions/StringFunctionsExtended.cs` (~120 lines)

**Excluded (game state dependent, moved to F428)**:
- LINEISEMPTY (GlobalStatic.Console.EmptyLine)
- GETLINESTR (exm.Console.getStBar)
- STRFORM (LexicalAnalyzer, ExpressionMediator)
- STRJOIN (VariableTerm, VEvaluator)

---

## Background

### Philosophy (Mid-term Vision)

**Expression & Function System**: Complete the string function migration started in F419. These extended functions operate on string data without game state access, maintaining the "stateless data manipulation" principle.

### Problem (Current Issue)

F419 covers 10 core string functions. The remaining stateless string functions in Creator.cs need migration:
- STRCOUNT, CHARATU - String manipulation utilities
- TOHALF, TOFULL - Character width conversion
- ENCODETOUNI - Unicode encoding

### Goal (What to Achieve)

1. **Implement 5 stateless extended string functions**:
   - STRCOUNT (count regex pattern matches)
   - CHARATU (get character at UTF-16 code unit index)
   - TOHALF (convert to half-width)
   - TOFULL (convert to full-width)
   - ENCODETOUNI (encode to Unicode)

2. **Follow F419 patterns**:
   - Result type for error handling
   - DI registration in FunctionRegistry
   - Interface-based design

3. **Zero technical debt**

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 0 | StringFunctionsExtended.cs exists | file | Glob | exists | Era.Core/Functions/StringFunctionsExtended.cs | [x] |
| 1 | STRCOUNT implemented | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestStrcount" | [x] |
| 2 | CHARATU implemented | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestCharatu" | [x] |
| 3 | TOHALF/TOFULL implemented | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestWidthConversion" | [x] |
| 4 | ENCODETOUNI implemented | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestEncodetouni" | [x] |
| 5 | C# build succeeds | build | Bash | succeeds | "dotnet build Era.Core" | [x] |
| 6 | Zero technical debt | file | Grep | not_contains | TODO|FIXME|HACK | [x] |
| 7 | DI registration | file | Grep | contains | AddSingleton.*IStringFunctionsExtended.*StringFunctionsExtended | [x] |
| 8 | No VB dependency | file | Grep | not_contains | Microsoft.VisualBasic | [x] |

### AC Details

**AC#0**: `Glob "Era.Core/Functions/StringFunctionsExtended.cs"`

**AC#1-4**: C# unit tests in `Era.Core.Tests/Functions/StringFunctionsExtendedTests.cs`
- AC#1: `dotnet test --filter FullyQualifiedName~TestStrcount`
  - Positive: Valid regex returns match count
  - Negative: Invalid regex returns Result.Fail
- AC#2: `dotnet test --filter FullyQualifiedName~TestCharatu`
  - Positive: Valid index returns character
  - Negative: Negative index and >= length both return empty string
- AC#3: `dotnet test --filter FullyQualifiedName~TestWidthConversion` (TOHALF + TOFULL)
- AC#4: `dotnet test --filter FullyQualifiedName~TestEncodetouni`
  - Positive: Valid position returns code point
  - Negative: Empty string returns -1, negative/overflow returns Result.Fail

**AC#5**: `dotnet build Era.Core`

**AC#6**: `Grep pattern="TODO|FIXME|HACK" path="Era.Core/Functions/StringFunctionsExtended.cs"` → no matches

**AC#7**: `Grep pattern="AddSingleton.*IStringFunctionsExtended" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs"` → match found

**AC#8**: `Grep pattern="Microsoft.VisualBasic" path="Era.Core/Functions/StringFunctionsExtended.cs"` → no matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 0 | 0 | Create StringFunctionsExtended.cs file | [x] |
| 1 | 1 | Implement STRCOUNT function | [x] |
| 2 | 2 | Implement CHARATU function | [x] |
| 3 | 3 | Implement TOHALF and TOFULL functions | [x] |
| 4 | 4 | Implement ENCODETOUNI function | [x] |
| 5 | 5 | Verify C# build succeeds | [x] |
| 6 | 6 | Verify no TODO/FIXME/HACK comments | [x] |
| 7 | 7 | Verify DI registration in FunctionRegistry | [x] |
| 8 | 8 | Verify no Microsoft.VisualBasic dependency | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->
<!-- **Batch verification waiver (AC#3/Task#3)**: TOHALF/TOFULL share same implementation method (StrChangeStyleMethod) and dictionary. Following F384 precedent. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Functions (5 total)

**Registration**: `Creator.cs` lines 121, 126-127, 135-136
**Implementation**: `Creator.Method.cs` (see table below)

| Function | Purpose | Return Type | Registration | Implementation |
|----------|---------|-------------|:------------:|:--------------:|
| STRCOUNT | Count regex pattern matches in string | long | line 121 | StrCountMethod (2278-2299) [2] |
| CHARATU | Get character at string index (UTF-16 code unit) | string | line 136 | CharAtMethod (2631-2647) [3] |
| TOHALF | Convert to half-width characters | string | line 126 | StrChangeStyleMethod (2397-2432) [1] |
| TOFULL | Convert to full-width characters | string | line 127 | StrChangeStyleMethod (2397-2432) [1] |
| ENCODETOUNI | Get Unicode code point at position | long | line 135 | EncodeToUniMethod (2594-2629) |

**Notes**:
[1] Legacy uses custom shim at `engine/Assets/Scripts/uEmuera/VisualBasic.cs` (not Microsoft.VisualBasic). **Decision**: Era.Core WILL port the dictionary-based approach from VisualBasic.cs (ToNarrow/ToWide dictionaries) to maintain stateless architecture and avoid engine dependency.
[2] STRCOUNT uses regex - second argument is a regex pattern, not a literal string.
[3] CHARATU returns empty string for out-of-range index (negative or >= string.Length), no error thrown.

### Error Handling

| Function | Scenario | Result |
|----------|----------|--------|
| STRCOUNT | Invalid regex pattern | `Result.Fail("第2引数が正規表現として不正です：{message}")` |
| CHARATU | Out-of-range index | `Result.Ok("")` (empty string, not error) |
| ENCODETOUNI | Empty string | `Result.Ok(-1)` |
| ENCODETOUNI | Negative position | `Result.Fail("ENCODETOUNI関数の第2引数({pos})が負の値です")` |
| ENCODETOUNI | Position >= length | `Result.Fail("ENCODETOUNI関数の第2引数({pos})が文字列の長さを超えています")` |

### Interface Strategy

**Decision**: Create separate `IStringFunctionsExtended` interface.

**Rationale**: Separate interface avoids breaking existing `IStringFunctions` consumers and follows Interface Segregation Principle. F419's `IStringFunctions` remains stable.

### Interface Extension

```csharp
// Era.Core/Functions/IStringFunctionsExtended.cs
using Era.Core.Types;

namespace Era.Core.Functions;

public interface IStringFunctionsExtended
{
    Result<long> Strcount(string str, string search);
    Result<string> Charatu(string str, long index);
    Result<string> Tohalf(string str);
    Result<string> Tofull(string str);
    Result<long> Encodetouni(string str, long position = 0);
}
```

### DI Registration

```csharp
// In Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
services.AddSingleton<IStringFunctionsExtended, StringFunctionsExtended>();
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F419 | Core string functions must be implemented first |
| Related | F421 | Function infrastructure pattern reference |
| Related | F418 | Functions Core pattern reference |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-10 FL iter1**: [resolved] Scope reduced to 5 stateless functions. 4 game-state dependent functions (LINEISEMPTY, GETLINESTR, STRFORM, STRJOIN) moved to F428.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 | create | orchestrator | Created as F419 follow-up per FL review | PROPOSED |
| 2026-01-10 06:04 | START | implementer | Task 0-8 | - |
| 2026-01-10 06:04 | END | implementer | Task 0-8 | SUCCESS |
| 2026-01-10 | END | regression-tester | Era.Core.Tests | OK:603/603 |
| 2026-01-10 | END | ac-tester | AC 0-8 | OK:9/9 |
| 2026-01-10 | END | feature-reviewer | post + doc-check | READY |

---

## Links

- [feature-419.md](feature-419.md) - Built-in Functions Data (parent feature)
- [feature-421.md](feature-421.md) - Function Call Mechanism
- [feature-418.md](feature-418.md) - Functions Core (pattern reference)
- [feature-428.md](feature-428.md) - Game-state dependent string functions (LINEISEMPTY, GETLINESTR, STRFORM, STRJOIN)
