# Feature 421: Function Call Mechanism

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

Implement function call mechanism for ERB built-in functions: FunctionRegistry, IFunctionRegistry interface, IBuiltInFunction interface, function dispatch, and evaluation context.

---

## Background

### Philosophy (Mid-term Vision)

**Expression & Function System**: Phase 8 establishes the foundation for evaluating ERB expressions and built-in functions. The function call mechanism provides:
- Function registry for 100+ built-in functions
- Type-safe function dispatch through interfaces
- Evaluation context for function execution
- Result-based error handling

This feature contributes to Phase 8 by implementing the infrastructure that F418-F420 built-in function implementations will register with and use.

### Problem (Current Issue)

Phase 8 requires function call infrastructure before implementing 100+ built-in functions (F418-F420):
- No registry mechanism for function lookup
- No interface contract for built-in function implementations
- No evaluation context for function execution
- No type-safe dispatch mechanism

Without this infrastructure, built-in function features cannot proceed.

### Goal (What to Achieve)

1. **IFunctionRegistry interface** - Function lookup and registration contract
2. **FunctionRegistry implementation** - Concrete registry with DI registration
3. **IBuiltInFunction interface** - Function implementation contract
4. **IEvaluationContext interface** - Context for function execution
5. **Function dispatch mechanism** - Type-safe function invocation
6. **Zero technical debt** - No TODO/FIXME/HACK comments remain
7. **Dispatch behavior equivalence** - Function dispatch produces same results as legacy ERB

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IFunctionRegistry interface exists | file | Glob | exists | Era.Core/Functions/IFunctionRegistry.cs | [x] |
| 2 | FunctionRegistry implementation exists | file | Glob | exists | Era.Core/Functions/FunctionRegistry.cs | [x] |
| 3 | IBuiltInFunction interface exists | file | Glob | exists | Era.Core/Functions/IBuiltInFunction.cs | [x] |
| 4 | IEvaluationContext interface exists | file | Glob | exists | Era.Core/Functions/IEvaluationContext.cs | [x] |
| 5 | DI registration includes IFunctionRegistry | file | Grep | contains | "AddSingleton.*IFunctionRegistry.*FunctionRegistry" | [x] |
| 6 | GetFunction returns Result type | file | Grep | contains | "Result<IBuiltInFunction> GetFunction" | [x] |
| 7 | Register method exists | file | Grep | contains | "Register.*string.*IBuiltInFunction" | [x] |
| 8 | IBuiltInFunction has Execute method | file | Grep | contains | "Result<object> Execute" | [x] |
| 9 | Unit tests exist | file | Glob | exists | Era.Core.Tests/Functions/FunctionRegistryTests.cs | [x] |
| 10 | Unit tests pass | build | build | succeeds | - | [x] |
| 11 | Dispatch behavior equivalence verified | file | Grep | contains | "DispatchEquivalence" | [x] |
| 12 | Zero technical debt | file | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 13 | Thread safety test exists | file | Grep | contains | "Concurrent" | [x] |

### AC Details

**AC#1**: IFunctionRegistry.cs exists in Era.Core/Functions/
- Test: `Glob pattern="Era.Core/Functions/IFunctionRegistry.cs"`
- Expected: File exists

**AC#2**: FunctionRegistry.cs exists in Era.Core/Functions/
- Test: `Glob pattern="Era.Core/Functions/FunctionRegistry.cs"`
- Expected: File exists

**AC#3**: IBuiltInFunction.cs exists in Era.Core/Functions/
- Test: `Glob pattern="Era.Core/Functions/IBuiltInFunction.cs"`
- Expected: File exists

**AC#4**: IEvaluationContext.cs exists in Era.Core/Functions/
- Test: `Glob pattern="Era.Core/Functions/IEvaluationContext.cs"`
- Expected: File exists

**AC#5**: DI registration in ServiceCollectionExtensions or equivalent
- Test: `Grep pattern="AddSingleton.*IFunctionRegistry.*FunctionRegistry" path="Era.Core/DependencyInjection/"`
- Expected: DI registration found (flexible pattern for whitespace/formatting)
- Note: If Grep fails on Windows due to path format, use `dir` command or backslash path as workaround

**AC#6**: IFunctionRegistry.GetFunction returns Result<IBuiltInFunction>
- Test: `Grep pattern="Result<IBuiltInFunction> GetFunction" path="Era.Core/Functions/IFunctionRegistry.cs"`
- Expected: Method signature found

**AC#7**: IFunctionRegistry.Register method exists
- Test: `Grep pattern="Register.*string.*IBuiltInFunction" path="Era.Core/Functions/IFunctionRegistry.cs"`
- Expected: Method signature found (flexible pattern for parameter names/whitespace)

**AC#8**: IBuiltInFunction.Execute returns Result<object>
- Test: `Grep pattern="Result<object> Execute" path="Era.Core/Functions/IBuiltInFunction.cs"`
- Expected: Method signature found

**AC#9**: FunctionRegistryTests.cs exists
- Test: `Glob pattern="Era.Core.Tests/Functions/FunctionRegistryTests.cs"`
- Expected: File exists

**AC#10**: All unit tests pass
- Test: `dotnet test Era.Core.Tests --filter "Category=Functions"`
- Expected: 0 failures

**AC#11**: Dispatch behavior equivalence verified in tests
- Test: `Grep pattern="DispatchEquivalence" path="Era.Core.Tests/Functions/FunctionRegistryTests.cs"`
- Expected: Test method with "DispatchEquivalence" in name verifying:
  1. Registered function Execute() produces same output as legacy FunctionMethod.cs for same inputs
  2. Unregistered function lookup returns equivalent error to legacy ERB
  3. Case-insensitive function name resolution (if applicable per legacy behavior)
- **Minimum assertion requirement**: Test must include at least 3 Assert statements covering these criteria. AC#10 (test pass) implicitly verifies test content quality.

**AC#12**: No TODO/FIXME/HACK comments remain
- Test: `Grep pattern="TODO|FIXME|HACK" path="Era.Core/Functions/" type="cs"`
- Expected: 0 matches (not_contains)

**AC#13**: Thread safety test exists
- Test: `Grep pattern="Concurrent" path="Era.Core.Tests/Functions/FunctionRegistryTests.cs"`
- Expected: Concurrent access test exists verifying thread-safe function lookup

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create IFunctionRegistry interface with GetFunction and Register methods | [x] |
| 2 | 2 | Implement FunctionRegistry with dictionary-based storage | [x] |
| 3 | 3 | Create IBuiltInFunction interface with Execute method | [x] |
| 4 | 4 | Create IEvaluationContext interface for function execution context | [x] |
| 5 | 5 | Add DI registration for IFunctionRegistry → FunctionRegistry | [x] |
| 6 | 6,7,8 | Verify interface signatures use Result types and match architecture.md | [x] |
| 7 | 9 | Create FunctionRegistryTests.cs with registration and lookup tests | [x] |
| 8 | 10 | Verify all unit tests pass | [x] |
| 9 | 11 | Add dispatch behavior equivalence test verifying function dispatch matches legacy | [x] |
| 10 | 12 | Remove all TODO/FIXME/HACK comments from implementation | [x] |
| 11 | 13 | Add concurrent access test for thread-safe function lookup | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->
<!-- **Batch verification waiver (Task 6)**: Following F384 precedent for batch verification of related interface signatures. AC:Task 1:1 rule waived. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Interface Definitions (per architecture.md Phase 8)

**IFunctionRegistry**:
```csharp
public interface IFunctionRegistry
{
    Result<IBuiltInFunction> GetFunction(string name);
    void Register(string name, IBuiltInFunction function);
}
```

**IBuiltInFunction**:
```csharp
public interface IBuiltInFunction
{
    Result<object> Execute(IEvaluationContext context, params object[] args);
}
```

**IEvaluationContext**:
```csharp
public interface IEvaluationContext
{
    /// <summary>Get typed argument at specified index</summary>
    T GetArg<T>(int index);

    /// <summary>Get all arguments as typed array</summary>
    T[] GetArgs<T>();

    /// <summary>Get argument count</summary>
    int ArgCount { get; }

    /// <summary>
    /// Current character for character-related functions.
    /// Set only during character-scoped function calls (e.g., GETPALAM during TRAIN mode).
    /// Null for global functions (ABS, RAND, TOSTR etc.).
    /// </summary>
    CharacterId? CurrentCharacter { get; }

    /// <summary>Variable access for GETPALAM, GETEXP etc.</summary>
    IVariableStore Variables { get; }
}
```

**Design rationale**:
- `GetArg<T>/GetArgs<T>`: Used by F418 function registration pattern (e.g., `context.GetArg<long>(0)`)
- `CurrentCharacter`: Optional, only set during character-scoped function calls
- `Variables`: Provides access to game state for functions like GETPALAM, GETEXP

### Implementation Requirements

| Requirement | Verification |
|-------------|--------------|
| **Result type usage** | All methods return Result<T> or use Result for error handling |
| **No static classes** | FunctionRegistry is instance-based, registered via DI |
| **DI registration** | IFunctionRegistry → FunctionRegistry in ServiceCollectionExtensions |
| **Thread safety** | FunctionRegistry must be thread-safe for function lookup |

### File Locations

| File | Path |
|------|------|
| IFunctionRegistry.cs | Era.Core/Functions/IFunctionRegistry.cs |
| FunctionRegistry.cs | Era.Core/Functions/FunctionRegistry.cs |
| IBuiltInFunction.cs | Era.Core/Functions/IBuiltInFunction.cs |
| IEvaluationContext.cs | Era.Core/Functions/IEvaluationContext.cs |
| FunctionRegistryTests.cs | Era.Core.Tests/Functions/FunctionRegistryTests.cs |

### Testing Requirements

1. **Registration tests**: Verify Register method adds functions correctly
2. **Lookup tests**: Verify GetFunction retrieves registered functions
3. **Not found tests**: Verify GetFunction returns Result.Fail for unregistered functions
4. **Duplicate registration tests**: Verify behavior when registering same name twice
5. **Thread safety tests**: Verify concurrent access works correctly
6. **Legacy equivalence**: Verify function call mechanism matches ERB behavior

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F409 | Phase 8 Planning feature creates this feature |
| Related | F377 | Phase 4 Design Principles (Result type, DI, no static classes) |
| Related | F384 | IVariableStore interface used by IEvaluationContext.Variables |
| Successor | F418 | Built-in Functions Core will register with FunctionRegistry |
| Successor | F419 | Built-in Functions Data will register with FunctionRegistry |
| Successor | F420 | Built-in Functions Game will register with FunctionRegistry |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 8 definition
- [feature-409.md](feature-409.md) - Phase 8 Planning (predecessor)
- [feature-377.md](feature-377.md) - Phase 4 Design Principles (Result type, DI)
- [feature-384.md](feature-384.md) - IVariableStore interface
- [feature-418.md](feature-418.md) - Built-in Functions Core (successor)
- [feature-419.md](feature-419.md) - Built-in Functions Data (successor)
- [feature-420.md](feature-420.md) - Built-in Functions Game (successor)

---

## Out-of-Scope Note

| 発見日 | 種類 | 内容 | 対応 |
|--------|------|------|------|
| 2026-01-09 | stale build | Expression tests (60件) が初回実行で FAIL。再ビルド後 PASS。 | 解決済み |

**詳細**: F421 実装中の regression test で Expression tests が FAIL と表示されたが、`dotnet build` 後に再実行したところ 60/60 PASS。ビルドアーティファクトが古かっただけで、F417 (Phase 8: Operator Implementation) は正常に ✅ 完了済み。index-features.md も正しく ✅ 表示。引き継ぎ不要。

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-09 FL iter1**: [resolved] Implementation Contract - IEvaluationContext responsibilities are vague. → Added explicit interface definition.
- **2026-01-09 FL iter2**: [resolved] AC#11 Legacy equivalence - need to specify what "equivalence" means. → Redefined as "Dispatch behavior equivalence" with clear verification criteria.
- **2026-01-09 FL iter3**: [resolved] AC#12 TODO-only pattern. → Updated to TODO|FIXME|HACK. Added F384 dependency. Made AC#5/AC#7 patterns flexible.
- **2026-01-09 FL iter4**: [resolved] Task 6 batch waiver, AC#5 path workaround added.
- **2026-01-09 FL iter4**: [skipped] AC#5 DI pattern - Use simple single-line registration syntax per Feasibility recommendation. No change needed.
- **2026-01-09 FL iter4**: [skipped] IEvaluationContext.Variables - Keep IVariableStore exposure per YAGNI. Create IReadOnlyVariableStore only if needed later.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 | create | implementer | Created F421 per F409 Task 7 | PROPOSED |
| 2026-01-09 18:19 | START | implementer | Task 7 | - |
| 2026-01-09 18:19 | END | implementer | Task 7 | SUCCESS |
| 2026-01-09 18:30 | START | implementer | Tasks 1-6 | - |
| 2026-01-09 18:32 | END | implementer | Tasks 1-6 | SUCCESS |
| 2026-01-09 18:35 | START | ac-tester | Tasks 8-11 verification | - |
| 2026-01-09 18:35 | END | ac-tester | Tasks 8-11 verification | SUCCESS |
| 2026-01-09 18:40 | START | feature-reviewer | post mode | - |
| 2026-01-09 18:40 | DEVIATION | feature-reviewer | SSOT doc gap | NEEDS_REVISION |
| 2026-01-09 18:41 | END | feature-reviewer | post mode (after fix) | READY |
| 2026-01-09 18:42 | START | feature-reviewer | doc-check mode | - |
| 2026-01-09 18:42 | END | feature-reviewer | doc-check mode | READY |
