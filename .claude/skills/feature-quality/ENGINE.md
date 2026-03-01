# Engine Type Quality Guide

Issues specific to `Type: engine` features (C# engine/Era.Core).

---

## Granularity

- **1 feature** per functional unit
- Volume limit: ~300 lines
- AC count: 8-15
- Positive AND Negative tests required

---

## Common Issues

### Issue 1: Vague Interface Responsibilities

**Symptom**: Interface defined but purpose unclear.

**Example (Bad)**:
```csharp
public interface IEvaluationContext
{
    // Handles evaluation context
}
```

**Example (Good)**:
```csharp
public interface IEvaluationContext
{
    /// <summary>Get typed argument at specified index</summary>
    T GetArg<T>(int index);

    /// <summary>Get argument count</summary>
    int ArgCount { get; }
}
```

**Fix**: Add XML doc comments for every member + Design rationale section.

---

### Issue 32: AC Glob Patterns in Grep Method

**Symptom**: Grep method uses glob-like patterns (*) which may not work correctly.

**Example (Bad)**:
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 14 | No TODO markers | code | Grep(engine/Assets/Scripts/Services/*Dashboard*) | not_contains | "TODO" | [ ] |

**Example (Good)**:
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 14 | No TODO markers | code | Grep(engine/Assets/Scripts/Services/DashboardService.cs,engine/Assets/Scripts/Services/IDashboardService.cs) | not_contains | "TODO" | [ ] |

**Fix**: Use explicit file paths separated by commas, not glob patterns.

---

### Issue 33: AC Method Column Too Generic

**Symptom**: AC Method column shows "Grep" or "Bash" without specific targets.

**Example (Bad)**:
```
| AC# | Method | Matcher | Expected |
| 1 | Grep | not_contains | "pattern" |
| 2 | Bash | succeeds | - |
```

**Example (Good)**:
```
| AC# | Method | Matcher | Expected |
| 1 | Grep(engine/Assets/Scripts/Path/File.cs) | not_contains | "pattern" |
| 2 | dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s | succeeds | - |
```

**Fix**: Specify file path for Grep and exact command for test/build ACs.

---

### Issue 34: Feature Reference Without Verification

**Symptom**: References feature by ID without checking if it exists or matches claimed scope.

**Example (Bad)**:
```markdown
F582 provides complete YAML loaders for remaining CSV types
```
(where F582 is actually workflow guidance, not CSV elimination)

**Example (Good)**:
```markdown
Complete CSV elimination will be addressed in future features
```
(or verify F582 actually provides the claimed functionality)

**Fix**: Verify referenced feature scope matches claims or use generic future reference.

---

### Issue 35: Partial Test Coverage

**Symptom**: Tests both VariableSize and GameBase failure paths but only verifies one.

**Example (Bad)**:
```
| AC# | Description |
| 6 | VariableSize YAML failure shows error message |
(GameBase failure message untested)
```

**Example (Good)**:
```
| AC# | Description |
| 6 | VariableSize YAML failure shows error message |
| 7 | GameBase YAML failure shows error message |
```

**Fix**: Ensure symmetric test coverage for related functionality.

---

### Issue 2: Result<T> vs Exception Unclear

**Symptom**: No guidance on error handling approach.

**Standard Pattern**:

| Scenario | Use |
|----------|-----|
| Invalid input (recoverable) | `Result<T>.Fail()` |
| Programmer error (null arg) | `ArgumentNullException` |
| Overflow/underflow | `Result<T>.Fail()` |
| Infallible operation | Still use Result<T> for API consistency |

**Result<T> Value Extraction** (F468 pattern):
- Result<T> is a discriminated union with `Success(T Value)` and `Failure(string Error)` subtypes
- NO `.IsSuccess` or `.Value` property on base Result<T> type
- Use pattern matching: `result is Result<int>.Success s ? s.Value : defaultValue`

**Example (Bad)**:
```csharp
var result = store.GetMaxBase(id, MaxBaseIndex.Mood);
var value = result.IsSuccess ? result.Value : 0;  // INVALID - no IsSuccess/Value on Result<T>
```

**Example (Good)**:
```csharp
var result = store.GetMaxBase(id, MaxBaseIndex.Mood);
var value = result is Result<int>.Success s ? s.Value : 0;  // Pattern matching
```

**Example AC**:
```markdown
| 7 | SQRT negative handling | unit | dotnet test | succeeds | - | [ ] |
```

With AC Details:
```markdown
**AC#7**: SQRT(-1) returns Result.Fail("Negative value specified for SQRT function argument")
```

---

### Issue 3: Missing DI Registration

**Symptom**: Interface exists but DI registration not verified.

**Example (Good)**:
```markdown
| 4 | DI registration | file | Grep | contains | "AddSingleton.*IFunctionRegistry.*FunctionRegistry" | [ ] |
```

With Implementation Contract:
```csharp
// In ServiceCollectionExtensions.cs
services.AddSingleton<IFunctionRegistry, FunctionRegistry>();
```

---

### Issue 4: Migration Source Not Referenced

**Symptom**: Feature migrates legacy code but source not documented.

**Example (Good)**:
```markdown
### Source Migration Reference

**Legacy Location**: `engine/Assets/Scripts/Emuera/GameData/Function/Creator.Method.cs`

| Function | Line Range | Notes |
|----------|------------|-------|
| RandMethod | 923-975 | RAND with min/max variants |
| AbsMethod | 1029-1042 | Simple absolute value |
```

---

### Issue 5: Thread Safety Not Specified

**Symptom**: Shared resource without concurrency guidance.

**Example (Good)**:
```markdown
### Implementation Requirements

| Requirement | Verification |
|-------------|--------------|
| Thread safety | Use ConcurrentDictionary or read-write lock |

| 13 | Thread safety test | file | Grep | contains | "Concurrent" | [ ] |
```

---

### Issue 6: Equivalence Criteria Vague

**Symptom**: "matches legacy behavior" without specifics.

**Example (Bad)**:
```markdown
| 11 | Legacy equivalence | test | dotnet test | succeeds | - | [ ] |
```

**Example (Good)**:
```markdown
| 11 | Dispatch equivalence | file | Grep | contains | "DispatchEquivalence" | [ ] |
```

With AC Details:
```markdown
**AC#11**: Dispatch behavior equivalence verified
- Registered function produces same output as legacy for same inputs
- Unregistered function returns equivalent error
- **Minimum**: 3 Assert statements in test
```

---

### Issue 7: Batch Task Waiver Missing

**Symptom**: Multiple ACs in one task without waiver.

**Example (Good)**:
```markdown
| 6 | 6,7,8 | Verify interface signatures | [x] |

<!-- **Batch verification waiver (Task 6)**: Following F384 precedent for related interface signatures. -->
```

---

### Issue 8: AC Matcher Too Broad (F427)

**Symptom**: Pattern matches more than intended target.

**Example (Bad)**:
```markdown
| 4 | No duplicate HashSet | code | Grep | not_contains | private static readonly HashSet | [ ] |
```

**Example (Good)**:
```markdown
| 4 | No duplicate HalfwidthChars in StringFunctions | code | Grep | not_contains | HalfwidthChars | [ ] |
| 5 | No duplicate HalfwidthChars in ArrayFunctions | code | Grep | not_contains | HalfwidthChars | [ ] |
```

**Fix**: Use specific identifiers. Split per-file if checking multiple locations.

---

### Issue 9: AC Table Missing Method Column (F427)

**Symptom**: AC table lacks Method column showing verification tool.

**Example (Bad)**:
```markdown
| AC# | Description | Type | Matcher | Expected | Status |
```

**Example (Good)**:
```markdown
| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | File exists | file | Glob | exists | src/Era.Core/Foo.cs | [ ] |
| 2 | Uses helper | code | Grep | contains | Helper.Method | [ ] |
```

---

### Issue 10: Helper Method Visibility Unclear (F427)

**Symptom**: AC Details lists method but no AC verifies accessibility.

**Example (Bad)**:
```markdown
**AC#1**: Contains methods: `GetByteCount()`, `IsHalfwidth()`
```
(But only GetByteCount verified in AC table)

**Example (Good)**:
```markdown
| 7 | IsHalfwidth is public | code | Grep | contains | public static bool IsHalfwidth | [ ] |
| 8 | ByteIndexToCharIndex uses helper | code | Grep | contains | ShiftJisHelper.IsHalfwidth | [ ] |
```

**Fix**: If AC Details promises a method, add AC to verify it.

---

### Issue 11: Refactor Tasks Should Be Atomic (F427)

**Symptom**: "Refactor to use X" and "Remove duplicate Y" as separate tasks.

**Example (Bad)**:
```markdown
| 2 | 2,3 | Refactor to use ShiftJisHelper | [ ] |
| 3 | 4,5 | Remove duplicate code | [ ] |
```

**Example (Good)**:
```markdown
| 2 | 2,3,4,5 | Refactor to use ShiftJisHelper and remove duplicates | [ ] |
```

**Fix**: Refactoring inherently removes duplicates. Merge into single atomic task.

---

### Issue 12: Philosophy Too Narrow (F427)

**Symptom**: Philosophy states only principle name without SSOT/maintainability context.

**Example (Bad)**:
```markdown
### Philosophy
DRY - Eliminate duplicates.
```

**Example (Good)**:
```markdown
### Philosophy
DRY (Don't Repeat Yourself) - Establish ShiftJisHelper as the single source of truth for all Shift-JIS byte counting operations across Era.Core, ensuring consistent encoding behavior and simplified future maintenance.
```

**Fix**: Include SSOT claim, scope, and long-term maintenance benefit.

---

### Issue 13: Function Behavior Mismatch (F426)

**Symptom**: Feature title/Summary describes functions differently from actual legacy behavior.

**Example (Bad)**:
```markdown
# Feature 426: Built-in Functions Array Extended

## Summary
Migrate 3 extended array expression functions...
- GROUPMATCH (match any value from group in array)
- NOSAMES (verify no duplicate elements in range)
```

**Example (Good)**:
```markdown
# Feature 426: Built-in Functions Value Comparison

## Summary
Migrate 3 variadic value comparison functions...
- GROUPMATCH: Count how many of the comparison values equal the first value
- NOSAMES: Return 1 if first value differs from all others, 0 if any match
```

**Fix**: Read legacy implementation (Creator.Method.cs) BEFORE writing Summary. Verify function behavior matches description.

---

### Issue 14: Missing File Existence AC (F426)

**Symptom**: Summary specifies Output file but no AC verifies its creation.

**Example (Bad)**:
```markdown
**Output**: Methods in `src/Era.Core/Functions/ValueComparisonFunctions.cs`

## Acceptance Criteria
| 1 | GROUPMATCH test | test | Bash | succeeds | ... |  <-- No file existence AC
```

**Example (Good)**:
```markdown
| 1 | ValueComparisonFunctions.cs exists | file | Glob | exists | "src/Era.Core/Functions/ValueComparisonFunctions.cs" | [ ] |
| 2 | GROUPMATCH test | test | Bash | succeeds | ... |
```

**Fix**: If Summary has **Output** section, first AC must verify file existence.

---

### Issue 15: AC Expected Column Format (F426)

**Symptom**: File path embedded in Expected column instead of AC Details.

**Example (Bad)**:
```markdown
| 12 | Zero technical debt | file | Grep | not_contains | "TODO|FIXME|HACK" in src/Era.Core/Functions/Foo.cs | [ ] |
```

**Example (Good)**:
```markdown
| 12 | Zero technical debt | file | Grep | not_contains | "TODO|FIXME|HACK" | [ ] |
```

With AC Details:
```markdown
**AC#12**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="src/Era.Core/Functions/Foo.cs"
- Expected: 0 matches
```

**Fix**: Expected column contains only matcher value. File path goes in AC Details.

---

### Issue 16: Cross-Reference Naming Mismatch (F426)

**Symptom**: Related feature references this feature with outdated/incorrect name.

**Example (Bad)**:
```markdown
# F419 Review Notes
- [resolved] Array functions out-of-scope → deferred to F426 (Array Functions Extended)

# F426 Title
Feature 426: Built-in Functions Value Comparison  <-- Name mismatch!
```

**Example (Good)**:
```markdown
# F419 Review Notes
- [resolved] Value comparison functions out-of-scope → deferred to F426 (Value Comparison Functions)

# F426 Title
Feature 426: Built-in Functions Value Comparison  <-- Names match
```

**Fix**: When renaming feature, update all cross-references in related features (Links, Review Notes).

---

### Issue 17: AC Details Tool Mismatch (F426)

**Symptom**: AC table Method column says one tool, AC Details uses different command.

**Example (Bad)**:
```markdown
| 1 | File exists | file | Glob | exists | src/Era.Core/Foo.cs | [ ] |

**AC#1**: File existence verification
- Test: `dir Era.Core\Functions\Foo.cs`  <-- Uses dir, not Glob!
```

**Example (Good)**:
```markdown
| 1 | File exists | file | Glob | exists | src/Era.Core/Foo.cs | [ ] |

**AC#1**: File existence verification
- Test: Glob pattern="src/Era.Core/Functions/Foo.cs"
- Expected: File exists
```

**Fix**: AC Details must use the same tool as AC table Method column.

---

### Issue 18: Test Naming Convention Missing (F426)

**Symptom**: Feature has type-variant tests but naming convention not documented.

**Example (Bad)**:
```markdown
| 2 | GROUPMATCH Int64 test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TestGroupmatchInt64" | [ ] |
```
(Implementer may use different naming: TestGroupMatchInt64, Groupmatch_Int64_Test, etc.)

**Example (Good)**:
```markdown
### Implementation Contract

**Test Naming Convention**: Test methods follow `Test{FunctionName}{Type}` format (e.g., `TestGroupmatchInt64`, `TestGroupmatchString`). This ensures AC filter patterns match correctly.
```

**Fix**: Document test naming convention in Implementation Contract when AC uses FullyQualifiedName filter.

---

### Issue 19: DI Registration Snippet Missing (F426)

**Symptom**: DI registration AC exists but Implementation Contract lacks code snippet.

**Example (Bad)**:
```markdown
| 11 | DI registration | file | Grep | contains | "AddSingleton.*IFoo" | [ ] |

### Implementation Contract
(No DI code snippet shown)
```

**Example (Good)**:
```markdown
| 11 | DI registration | file | Grep | contains | "AddSingleton.*IFoo" | [ ] |

### Implementation Contract

### DI Registration

Register in `src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
services.AddSingleton<IFoo, Foo>();
```
```

**Fix**: If AC verifies DI registration, Implementation Contract MUST include the exact code snippet.

---

### Issue 20: Error Message Format Missing (F426)

**Symptom**: Feature returns Result.Fail but error message format not specified.

**Example (Bad)**:
```markdown
**Argument Requirements**: Return `Result.Fail` if comparisons array is empty.
```

**Example (Good)**:
```markdown
**Argument Requirements**: Return `Result.Fail` if comparisons array is empty.

**Error Message Format**: `"{FunctionName} function requires at least 2 arguments"` (e.g., `"GROUPMATCH function requires at least 2 arguments"`).
```

**Fix**: Specify exact error message format in Implementation Contract, including Japanese text if user-facing.

---

### Issue 21: Interface Snippet Missing Namespace (F425)

**Symptom**: Implementation Contract shows interface code but lacks namespace and using statements.

**Example (Bad)**:
```csharp
// src/Era.Core/Functions/IFoo.cs
public interface IFoo
{
    Result<string> Bar(string input);
}
```

**Example (Good)**:
```csharp
// src/Era.Core/Functions/IFoo.cs
using Era.Core.Types;

namespace Era.Core.Functions;

public interface IFoo
{
    Result<string> Bar(string input);
}
```

**Fix**: Interface code snippets in Implementation Contract MUST include `using` statements and `namespace` declaration. This prevents implementer confusion about file structure.

---

### Issue 22: Function Argument Special Interpretation Undocumented (F425)

**Symptom**: Function accepts string argument that has special interpretation (regex, format string, etc.) but this is not documented.

**Example (Bad)**:
```markdown
| STRCOUNT | Count substring occurrences | long |
```

**Example (Good)**:
```markdown
| STRCOUNT | Count regex pattern matches in string | long |

**Notes**:
[2] STRCOUNT uses regex - second argument is a regex pattern, not a literal string.
```

**Fix**: If function argument has special interpretation (regex, format string, path pattern, etc.), document explicitly in Notes section and use accurate description in Function table.

---

### Issue 23: Non-Sequential AC Numbering (F435)

**Symptom**: AC IDs use non-standard numbering (2a, 2b, 7a) breaking AC:Task 1:1 mapping pattern.

**Example (Bad)**:
```markdown
| AC# | Description |
| 2 | Interface exists |
| 2a | Stub exists |
| 2b | DI registration |
```

**Example (Good)**:
```markdown
| AC# | Description |
| 2 | Interface exists |
| 3 | Stub exists |
| 4 | DI registration |
```

**Fix**: Use sequential numbering. If adding ACs after initial creation, renumber all subsequent ACs.

---

### Issue 24: Technical Debt Path Scope Incomplete (F435)

**Symptom**: Feature creates files in multiple directories but tech debt AC only checks one path.

**Example (Bad)**:
```markdown
### File Structure
| `src/Era.Core/Training/IFoo.cs` | Interface |
| `src/Era.Core/Training/Foo.cs` | Implementation |
| `src/Era.Core/Commands/Bar.cs` | Command |

| 10 | Zero tech debt | code | Grep | not_contains | TODO|FIXME|HACK | [ ] |

**AC#10**: Tech debt check
- Path: src/Era.Core/Commands/Bar.cs  <-- Missing Training/ files!
```

**Example (Good)**:
```markdown
| 10 | Zero tech debt | code | Grep | not_contains | TODO|FIXME|HACK | [ ] |

**AC#10**: Tech debt check
- Paths: src/Era.Core/Training/, src/Era.Core/Commands/Bar.cs
- Expected: 0 matches across all feature files
```

**Fix**: Tech debt AC must cover ALL directories where feature creates files.

---

### Issue 25: Service Interface Without Implementation (F434)

**Symptom**: Feature defines interface and handlers depending on it, but implementation class not specified.

**Example (Bad)**:
```markdown
### Interface
```csharp
public interface ICharacterManager { ... }
```

### DI Registration
```csharp
services.AddSingleton<ICharacterManager, CharacterManager>();  // CharacterManager not defined!
```
```

**Example (Good)**:
```markdown
### Interface
```csharp
public interface ICharacterManager { ... }
```

### Service Implementation (Stub)
```csharp
public class CharacterManager : ICharacterManager
{
    public Result<Unit> AddChara(CharacterId id)
        => Result<Unit>.Fail("Not implemented - awaiting Phase N");
}
```
```

**Fix**: If DI registration references implementation class, Implementation Contract MUST include implementation code or stub. Specify which Phase provides actual implementation if stub.

---

### Issue 26: Assumed Accessor Methods Not Exist (F434)

**Symptom**: Feature assumes GlobalStatic/legacy accessor methods exist for migration, but they don't.

**Example (Bad)**:
```markdown
### Task 8.5: GlobalStatic Accessor Migration
Replace TODO with GlobalStatic.GetFlag(flagIndex);  // Method doesn't exist!
```

**Example (Good)**:
```markdown
### Task 8.5: GlobalStatic Accessor Migration
**DEFERRED**: GlobalStatic.GetFlag() does not exist.
Deferred to Phase 11 (State Management) which defines accessor pattern.

## Deferred Tasks
| Task 8.5 | GlobalStatic accessor migration | Phase 11 |
```

**Fix**: Before referencing accessor methods, Grep to verify they exist. If not, defer task to appropriate Phase and document in Deferred Tasks.

---

### Issue 27: Volume Waiver Not Documented (F452)

**Symptom**: Feature exceeds ~300 line limit but waiver not explicitly documented.

**Example (Bad)**:
```markdown
## Summary
**Total Volume**: ~50 COM implementations, estimated 5,000-7,000 lines
<!-- No mention of why this exceeds limit -->
```

**Example (Good)**:
```markdown
## Summary
**Total Volume**: ~50 COM implementations, estimated 5,000-7,000 lines

## Review Notes
- **YYYY-MM-DD FL iterN**: [resolved] Volume waiver granted: COM architecture + initial migration requires atomicity as designed in F450 Phase 12 Planning.
```

**Fix**: When volume exceeds ~300 lines, document waiver in Review Notes with justification (atomicity requirement, precedent, planning feature reference).

---

### Issue 28: Contains Matcher for Multiple Classes (F452)

**Symptom**: Using `contains` matcher to verify multiple class definitions when `count_equals` is needed.

**Example (Bad)**:
```markdown
| 10 | Equipment handlers (42-48) | code | Grep | contains | "class Com4[2-8].*EquipmentComBase" | [ ] |
```
(This would PASS if only 1 of 7 classes exists)

**Example (Good)**:
```markdown
| 10 | Equipment handlers (42-48) | code | Grep | count_equals | "class Com4[2-8].*EquipmentComBase" (7) | [ ] |
```

**Fix**: When regex matches multiple expected instances, use `count_equals` with expected count to verify all instances exist.

---

### Issue 29: Era.Core Cannot Reference Engine Layer (F461)

**Symptom**: Feature assumes Era.Core code can call GlobalStatic or engine APIs directly.

**Example (Bad)**:
```markdown
### Implementation Contract
```csharp
// src/Era.Core/Commands/System/CharacterManager.cs
public Result<Unit> AddChara(CharacterId id)
{
    GlobalStatic.VEvaluator.AddCharacter(id.Value);  // IMPOSSIBLE: Era.Core cannot reference engine
    return Result<Unit>.Ok(Unit.Value);
}
```
```

**Example (Good)**:
```markdown
### Architecture Note
Era.Core cannot reference GlobalStatic (engine layer). Integration uses DI:
- Era.Core: Defines interfaces + stub implementations
- engine: Creates implementations calling GlobalStatic
- DI: Register engine implementations via GlobalStatic properties

### Implementation Contract
```csharp
// engine/Assets/Scripts/Emuera/Services/CharacterManagerImpl.cs
public class CharacterManagerImpl : ICharacterManager
{
    public Result<Unit> AddChara(CharacterId id)
    {
        GlobalStatic.VEvaluator.AddCharacter(id.Value);
        return Result<Unit>.Ok(Unit.Value);
    }
}
```
```

**Fix**: Engine integrations must create implementations in `engine/` project, registered via GlobalStatic static properties. Era.Core keeps stub implementations as fallback.

---

### Issue 30: Test Project Cannot Reference Engine (F461)

**Symptom**: Feature expects Era.Core.Tests to use engine implementations.

**Example (Bad)**:
```markdown
| 6 | Tests use engine impl | test | Grep | contains | "CharacterManagerImpl" in SystemCommandTests.cs | [ ] |
```
(Era.Core.Tests cannot reference engine project)

**Example (Good)**:
```markdown
| 6 | Tests use Era.Core stubs | test | Grep | contains | "CharacterManager" in SystemCommandTests.cs | [ ] |
| 7 | Tests verify stub wiring | test | Bash | succeeds | "dotnet test --filter SystemCommandTests" | [ ] |

**AC#7 Details**: Tests verify stub wiring, not engine implementation. Engine correctness verified via headless mode in Post-Phase Review.
```

**Fix**: Era.Core.Tests uses Era.Core stubs. Engine implementation correctness verified separately (integration tests in engine.Tests or headless mode).

---

### Issue 31: DI Dictionary Key Naming Convention Undefined (F501)

**Symptom**: TDD tests use different Dictionary keys than implementation, causing test failures after implementation.

**Root Cause**: When registering Dictionary<string, T> in DI with type-variant keys, the key naming convention is not documented. TDD tests (Phase 3) assume one format, implementation (Phase 4) uses another.

**Example (Bad)**:
```markdown
### Implementation Contract

### DI Registration
```csharp
services.AddSingleton<Dictionary<string, IBinaryOperator>>(sp =>
    new Dictionary<string, IBinaryOperator>
    {
        ["+"] = new AddOperator(),  // Simple key - but what about string "+"?
    });
```
```

TDD Test assumes:
```csharp
var addOp = Services.GetRequiredService<Dictionary<string, IBinaryOperator>>()["+"];
```

Implementation uses:
```csharp
["+(int)"] = new AddOperator(),      // Type-suffixed key
["+(string)"] = new StringConcatOperator(),
```

**Example (Good)**:
```markdown
### Implementation Contract

### DI Registration

**Key Naming Convention**: Dictionary keys use `{symbol}` for type-agnostic operators and `{symbol}({type})` for type-specific variants.

| Operator | Key(s) | Notes |
|----------|--------|-------|
| `+` (Int64) | `"+"`, `"+(int)"` | Both keys registered |
| `+` (String) | `"+(string)"` | Type-specific only |
| `&&` | `"&&"` | No type variants |

```csharp
services.AddSingleton<Dictionary<string, IBinaryOperator>>(sp =>
    new Dictionary<string, IBinaryOperator>
    {
        // Arithmetic - register both simple and typed keys for compatibility
        ["+"] = new AddOperator(),
        ["+(int)"] = new AddOperator(),
        ["+(string)"] = new StringConcatOperator(),
        // Logical - no type variants, simple key only
        ["&&"] = new AndOperator(),
    });
```
```

**Fix**: When using Dictionary<string, T> in DI:
1. Document key naming convention in Implementation Contract
2. Specify ALL keys that will be registered (both simple and type-suffixed if applicable)
3. TDD tests must use documented key format
4. If type dispatch exists, document whether fallback keys are registered

---

### Issue 41: ReportError Interface Assumption (F596)

**Symptom**: Feature adds ReportError method to IErrorDialogService implementation when ReportError is not part of the interface contract.

**Example (Bad)**:
```markdown
| 6 | IErrorDialogService complete implementation | code | Grep | contains | "ReportError" | [ ] |

**AC#6**: ConsoleErrorHandler implements all IErrorDialogService interface methods including ReportError
```
(But IErrorDialogService interface does not include ReportError method)

**Example (Good)**:
```markdown
| 6 | No technical debt in ConsoleErrorHandler | code | Grep | not_contains | "TODO|FIXME|HACK" | [ ] |
```

**Fix**: Verify interface contract before adding method implementation ACs. Only verify methods that are actually part of the interface definition.

---

### Issue 46: Model Change Without Consumer Impact Analysis (F760)

**Symptom**: Feature changes a model class (adds/removes/modifies properties) but doesn't verify all consumers handle the change.

**Example (Bad)**:
```markdown
## Impact Analysis
| ErbParser | Major | TalentRef gains Index property |
| ErbToYaml | Major | ConvertTalentRef branches into three conversion paths |
```
(But ResolveInnerBitwiseRef in same file also uses TalentRef.Name for CSV lookup — breaks when Name becomes empty for numeric patterns)

**Example (Good)**:
```markdown
## Impact Analysis
| ErbParser | Major | TalentRef gains Index property |
| ErbToYaml | Major | ConvertTalentRef + ResolveInnerBitwiseRef must handle TalentRef.Index/Target |
```

**Fix**: When adding/modifying model properties that change existing field semantics (e.g., Name becomes empty when Index is set), Grep all consumers of the model class across all projects. Add each consumer to Impact Analysis and create Task/AC for required updates.

---

### Issue 47: Technical Design References Non-Existent Method (F761)

**Symptom**: Technical Design pseudocode or integration description references a method name that doesn't exist in the actual codebase.

**Example (Bad)**:
```markdown
### Usage in FileConverter
```csharp
var ast = _parser.ParseFile(erbPath);
ast = LocalGateResolver.Resolve(ast); // Integration in FileConverter.ProcessFile()
```
```
(`ProcessFile()` does not exist — actual entry point is `ConvertAsync()`)

**Example (Good)**:
```markdown
### Usage in FileConverter
```csharp
// In ConvertAsync():
var ast = _parser.ParseFile(erbPath);
ast = _localGateResolver.Resolve(ast); // Integration in FileConverter.ConvertAsync()
```
```

**Fix**: Before referencing integration points in Technical Design, Grep the actual codebase to verify method names exist. Use actual method names, not assumed/designed ones.

---

### Issue 63: Interface Extension Without Backward Compatibility AC (F791)

**Symptom**: Feature extends interface with new methods but no AC verifies existing methods remain unchanged. An implementation could accidentally remove/modify existing methods and all ACs still pass.

**Example (Bad)**:
```markdown
| 1 | IGameState declares BeginTrain | code | Grep | matches | `Result<Unit>\s+BeginTrain\(\)` | [ ] |
| 2 | IGameState declares SaveGameDialog | code | Grep | matches | `Result<Unit>\s+SaveGameDialog\(\)` | [ ] |
```
(Only new methods verified; existing 6 methods could be removed silently)

**Example (Good)**:
```markdown
| 1 | IGameState declares BeginTrain | code | Grep | matches | `Result<Unit>\s+BeginTrain\(\)` | [ ] |
| 2 | IGameState declares SaveGameDialog | code | Grep | matches | `Result<Unit>\s+SaveGameDialog\(\)` | [ ] |
| 19 | IGameState existing methods preserved | code | Grep | count_equals | `Result<Unit>\s+(SaveGame\|LoadGame\|Quit\|Restart\|ResetData\|SetVariable)\(` = 6 | [ ] |
```

**Fix**: When extending an existing interface, add a `count_equals` AC verifying all pre-existing method signatures remain intact. Pattern: list existing method names in alternation group, count = number of existing methods.

**See also**: F789 AC#7 (`IVariableStore.cs is unchanged`), F791 AC#20 (`IFunctionRegistry unmodified`).

---

### Issue 64: Handoff Destination Self-Referencing Phase (F791)

**Symptom**: Mandatory Handoff destination references the same Phase the feature belongs to. Phase tasks are already consumed into features — no one picks up new items added to a WIP Phase.

**Example (Bad)**:
```markdown
## Mandatory Handoffs
| GameStateImpl delegation | Phase 20 engine integration feature | - | - |
```
(Feature IS in Phase 20. Adding to Phase 20 tasks = orphan.)

**Example (Good)**:
```markdown
## Mandatory Handoffs
| GameStateImpl delegation | Feature | F793 | FL POST-LOOP Option A |
```
(New DRAFT feature created with explicit executor.)

**Fix**: Before choosing Option C (Phase reference), verify: (1) Phase Status is not WIP/Done, (2) Handoff Phase ≠ feature's own Phase. If either fails, use Option A (new Feature). See `deferred-task-protocol.md` Option C Guards.

---

### Issue 65: Missing Setter ACs When Getter ACs Exist (F789)

**Symptom**: AC table defines getter method verification (GetSaveStr, GetTa) but omits corresponding setter methods (SetSaveStr, SetTa, SetTb). FL adds setter ACs in later iterations.

**Example (Bad)**:
```markdown
| 3 | IStringVariables declares GetSaveStr returning string | code | Grep | matches | `string GetSaveStr\(SaveStrIndex` | [ ] |
| 4 | I3DArrayVariables declares GetTa returning Result<int> | code | Grep | matches | `Result<int> GetTa\(int` | [ ] |
```
(No ACs for SetSaveStr, SetTa, SetTb)

**Example (Good)**:
```markdown
| 3 | IStringVariables declares GetSaveStr | code | Grep | matches | `string GetSaveStr\(SaveStrIndex` | [ ] |
| 3b | IStringVariables declares SetSaveStr | code | Grep | matches | `void SetSaveStr\(SaveStrIndex.*string` | [ ] |
| 4 | I3DArrayVariables declares GetTa | code | Grep | matches | `Result<int> GetTa\(int.*int.*int` | [ ] |
| 4c | I3DArrayVariables declares SetTa | code | Grep | matches | `void SetTa\(int.*int.*int.*int` | [ ] |
```

**Fix**: For every getter AC, check whether a corresponding setter should exist per the interface design. If Goal says "Get/Set", both need ACs.

---

### Issue 66: Stub Replacement Without Actual Call Verification AC (F789)

**Symptom**: AC verifies NotImplementedException is removed (negative assertion) and interface is injected, but does not verify the replacement actually calls the interface method. A hardcoded `return true` would pass both ACs.

**Example (Bad)**:
```markdown
| 10 | HasCollectionUnlocked stub replaced | code | Grep | not_matches | `NotImplementedException.*SAVESTR` | [ ] |
| 12 | ShopSystem injects IStringVariables | code | Grep | matches | `IStringVariables` | [ ] |
```
(Hardcoded `return true` passes both)

**Example (Good)**:
```markdown
| 10 | HasCollectionUnlocked stub replaced | code | Grep | not_matches | `NotImplementedException.*SAVESTR` | [ ] |
| 12 | ShopSystem injects IStringVariables | code | Grep | matches | `IStringVariables` | [ ] |
| 19a | HasCollectionUnlocked calls GetSaveStr | code | Grep | matches | `GetSaveStr` | [ ] |
```

**Fix**: For each stub replacement, add a positive AC verifying the replacement actually calls the interface method (e.g., Grep for `GetSaveStr` in the consuming file).

---

### Issue 67: Cross-Line Grep Matcher That Can Never Match (F789)

**Symptom**: AC grep pattern assumes method name and exception are on the same line, but they're on different lines. Pattern never matches, making not_matches vacuously true.

**Example (Bad)**:
```markdown
| 10 | Stub replaced | code | Grep | not_matches | `HasCollectionUnlocked.*NotImplementedException` | [ ] |
```
(Method signature on line 408, throw on line 409 — single-line grep never matches either way)

**Example (Good)**:
```markdown
| 10 | Stub replaced | code | Grep | not_matches | `NotImplementedException.*SAVESTR` | [ ] |
```
(Matches the throw line content directly)

**Fix**: Grep patterns must match content within a single line. When method name and target content (exception, return value) are on different lines, match the most specific single-line content (e.g., the throw message). Never assume two code constructs are on the same line.

---

### Issue 68: Implementation Contract Stale Constructor Signatures (F790)

**Symptom**: Implementation Contract shows constructor parameters that don't match actual source code. Causes FL to correct signatures during review.

**Example (Bad)**:
```markdown
### ShopSystem constructor change
// Before (F774):
public ShopSystem(IConsoleOutput console, IVariableStore variables, IGameState gameState)
```
(Actual: `ShopSystem(ShopDisplay display, IVariableStore variables, IConsoleOutput console, IGameState gameState, IInputHandler inputHandler)`)

**Example (Good)**:
```markdown
### ShopSystem constructor change
// Before (F774) — verified against src/Era.Core/Shop/ShopSystem.cs:
public ShopSystem(ShopDisplay display, IVariableStore variables, IConsoleOutput console, IGameState gameState, IInputHandler inputHandler)
```

**Fix**: Before writing constructor signatures in Implementation Contract, re-read the actual source file and copy the exact current signature. Add verification comment showing source file path.

---

### Issue 69: AC Cross-Reference Count Inconsistency After AC Addition (F789, F791)

**Symptom**: When ACs are added during FC (e.g., AC#17a/b/c split, AC#18a/b boundary tests), cross-reference tables (Goal Coverage, Tasks AC# column, Philosophy Derivation, Technical Design AC Coverage, Success Criteria) become stale. FL spends multiple iterations fixing counts.

**Example (Bad)**:
```markdown
## Technical Design
### Approach
This approach satisfies all 16 ACs...

## Success Criteria
- All 16 ACs pass
```
(But AC table now has 31 ACs after additions)

**Example (Good)**:
After every AC addition/split, update ALL of:
1. Tasks AC# column (Task → AC mapping)
2. Goal Coverage Verification table
3. Philosophy Derivation AC Coverage column
4. Technical Design Approach (total count)
5. Success Criteria (total count)

**Fix**: After AC design is complete, run a cross-reference consistency check: count AC rows in Definition Table, verify same count appears in Success Criteria and Technical Design. Verify every AC# appears in at least one Task. Verify every Goal maps to at least one AC.

---

### Issue 70: Missing Behavioral Contract ACs for Boundary and Default Values (F789)

**Symptom**: ACs verify interface signatures and implementation existence but not behavioral contracts (what happens at boundaries, what uninitialized values return). FL adds these in later iterations.

**Example (Bad)**:
```markdown
| 3 | GetSaveStr returns string | code | Grep | matches | `string GetSaveStr\(SaveStrIndex` | [ ] |
| 14 | Unit tests pass | test | dotnet test | succeeds | - | [ ] |
```
(No AC verifies GetSaveStr returns empty string for out-of-bounds, or GetTa returns Fail for invalid indices)

**Example (Good)**:
```markdown
| 3 | GetSaveStr returns string | code | Grep | matches | `string GetSaveStr\(SaveStrIndex` | [ ] |
| 14 | Unit tests pass | test | dotnet test | succeeds | - | [ ] |
| 18a | GetSaveStr returns empty for out-of-bounds | test | Grep(src/Era.Core.Tests/) | matches | `GetSaveStr.*string\.Empty` | [ ] |
| 18b | GetTa returns failure for out-of-bounds | test | Grep(src/Era.Core.Tests/) | matches | `GetTa.*Fail` | [ ] |
| 18c | Unit test exercises max valid TA index (304) | test | Grep(src/Era.Core.Tests/) | matches | `GetTa.*304` | [ ] |
```

**Fix**: For every interface method with bounds checking or default behavior documented in Technical Design, add a test existence AC that verifies a unit test exercises the boundary/default case. Pattern: Grep test directory for method name + expected boundary value.

---

## Checklist

- [ ] Interface responsibilities explicit with XML docs
- [ ] Error handling approach specified (Result<T> vs Exception)
- [ ] DI registration AC included
- [ ] Migration source with file paths and line ranges
- [ ] Thread safety requirements for shared resources
- [ ] Equivalence criteria with specific input/output pairs
- [ ] Task batch waivers documented
- [ ] ~300 lines volume limit respected
- [ ] AC matchers use specific identifiers (not broad patterns)
- [ ] Grep methods use explicit file paths (not glob patterns)
- [ ] AC table includes Method column (Glob/Grep/dotnet test)
- [ ] AC Details methods all have corresponding ACs
- [ ] Refactor+Remove tasks merged into atomic task
- [ ] Philosophy includes SSOT claim and scope
- [ ] Function descriptions match legacy implementation behavior
- [ ] Output file has existence AC as first AC
- [ ] AC Expected column has matcher value only (paths in AC Details)
- [ ] Cross-references in related features use current name
- [ ] AC Details uses same tool as AC table Method column
- [ ] Test naming convention documented when using FullyQualifiedName filter
- [ ] DI registration code snippet in Implementation Contract
- [ ] Error message format specified for Result.Fail cases
- [ ] Interface snippet includes namespace and using statements
- [ ] Function arguments with special interpretation (regex, etc.) documented in Notes
- [ ] AC numbering is sequential (no 2a, 2b, 7a - renumber instead)
- [ ] Tech debt AC path covers all directories where feature creates files
- [ ] Service implementations included when DI registration references them
- [ ] Accessor methods verified to exist before assuming in migration tasks
- [ ] Interface extension methods distinguish resolution contexts (e.g., $LABEL vs @FUNCTION)
- [ ] Handler implementations follow established patterns (e.g., resolve then push scope)
- [ ] Volume waiver documented in Review Notes when exceeding ~300 lines (Issue 27)
- [ ] Multi-instance regex uses count_equals instead of contains (Issue 28)
- [ ] Era.Core integration creates implementations in engine/, not Era.Core (Issue 29)
- [ ] Test project uses Era.Core stubs, not engine implementations (Issue 30)
- [ ] DI Dictionary key naming convention documented (Issue 31)
- [ ] TDD tests and implementation use consistent DI keys (Issue 31)
- [ ] AC Grep path doesn't include pre-existing matches from other features (Issue 49)
- [ ] AC Expected column optimized for test commands vs matcher values (Issue 52)
- [ ] Task restructuring consistently handles AC numbering (Issue 53)
- [ ] Test setup instructions use accessible constructors/factory methods (Issue 54)
- [ ] DI registration patterns use exact literals instead of broad regex (Issue 55)
- [ ] Technical debt ACs use comprehensive TODO|FIXME|HACK pattern consistently (Issue 56)
- [ ] Test execution commands specify explicit project paths (Issue 57)
- [ ] Interface method implementation ACs verify only actual interface contract methods (Issue 41)
- [ ] Internal implementation relationship with public upgrade feature clearly documented (Issue 58)
- [ ] Static helper class vs extension methods choice properly justified (Issue 59)
- [ ] API breaking changes include backward compatibility overloads and documentation (Issue 60)
- [ ] AC removal includes systematic renumbering of all cross-references (Issue 61)
- [ ] Model property changes include consumer impact analysis across all projects (Issue 46)
- [ ] Technical Design integration points verified against actual codebase method names (Issue 47)
- [ ] Equivalence tests included within the same Feature as implementation (not separated into a later test-only Feature) (Issue 48)
- [ ] InternalsVisibleTo added to csproj when AC tests internal classes (Issue 62)
- [ ] AC grep alternation uses `|` not `\|` for ripgrep compatibility (Issue 56 note)
- [ ] Every getter AC has a corresponding setter AC when interface defines both (Issue 65)
- [ ] Stub replacement ACs include positive call verification, not just exception removal (Issue 66)
- [ ] Grep patterns match within a single line; no cross-line assumptions (Issue 67)
- [ ] Implementation Contract constructor signatures verified against actual source (Issue 68)
- [ ] AC count consistent across Success Criteria, Technical Design, Tasks, Goal Coverage (Issue 69)
- [ ] Behavioral contract ACs exist for boundary values and defaults (Issue 70)
- [ ] AC alternation matchers split when verifying independent conditions that must hold simultaneously (Issue 71)
- [ ] Test-rewrite Tasks have ground-truth pinning AC to break circular verification (Issue 72)

---

### Issue 48: Test-Implementation Feature Separation (Phase 19 Lesson)

**Symptom**: Migration features split into "creation-only" features and separate "test-only" features, causing massive rework when tests finally run.

**Root Cause**: Phase 19 (Kojo Conversion) created 11 implementation features (F633-F643) before running equivalence tests (F644, F706). Initial pass rate was 12.2% (79/650), requiring 24 additional fix features (F649-F702).

**Example (Bad)**:
```markdown
# Phase 20 Sub-Features
F774: Shop Core - implement ShopSystem.cs
F775: Collection - implement CollectionTracker.cs
...
F782: Equivalence Tests - test all Phase 20 implementations  ← Separated!
```

**Example (Good)**:
```markdown
# Phase 20 Sub-Features
F774: Shop Core - implement ShopSystem.cs + equivalence tests
F775: Collection - implement CollectionTracker.cs + equivalence tests
...
# No separate test-only feature needed
```

**Fix**: Each migration sub-feature MUST include its own equivalence tests as Tasks/ACs within the same feature. Never defer testing to a later "batch test" feature.

---

### Issue 49: AC Grep Path Includes Pre-existing Matches (F517)

**Symptom**: AC pattern count_equals X, but directory contains pre-existing matches from other features.

**Example (Bad)**:
```markdown
| 5 | Common/ has null validation | code | Grep | count_equals | 6 | [ ] |

**AC#5**: Common/ null validation
- Test: Grep pattern=`ArgumentNullException` path=`src/Era.Core/Common/` type=cs | count
- Expected: 6 matches
```
(Common/ actually has 53 pre-existing ArgumentNullException matches in other files)

**Example (Good)**:
```markdown
| 5 | Common/ has null validation | code | Grep | count_equals | 6 | [ ] |

**AC#5**: Common/ null validation (target files only)
- Test: Grep pattern=`ArgumentNullException` paths=[src/Era.Core/Common/GameInitialization.cs, src/Era.Core/Common/InfoPrint.cs, src/Era.Core/Common/CharacterSetup.cs] type=cs | count
- Expected: 6 matches (GameInitialization 4 + InfoPrint 1 + CharacterSetup 1)
- Note: Directory has 53 pre-existing ArgumentNullException in other files; this AC targets only F517 scope files
```

**Fix**: When using count_equals with Grep, verify directory doesn't contain pre-existing matches. If it does, either:
- Use explicit file paths instead of directory
- Document pre-existing count and use total (pre-existing + new)
- Add Note explaining scope restriction

---

### Issue 50: Type Mapping Missing for Service Bridge (F558)

**Symptom**: Feature creates service that bridges Era.Core types to engine types but property mapping not documented.

**Example (Bad)**:
```markdown
### GameBaseService
GameBaseService populates GameBase properties from YAML.
```

**Example (Good)**:
```markdown
### GameBaseService Property Mapping

GameBaseService maps GameBaseConfig (Era.Core strings) to GameBase (engine types):
- `GameBaseConfig.Code` (string) → `GameBase.ScriptUniqueCode` (Int64) - Convert.ToInt64() or 0 if parse fails
- `GameBaseConfig.Version` (string) → `GameBase.ScriptVersion` (Int64) - Convert.ToInt64() or 0 if parse fails
- `GameBaseConfig.Title` (string) → `GameBase.ScriptTitle` (string) - Direct assignment
- Error handling: Parse failures for numeric conversions should log warning and use fallback value
```

**Fix**: When service bridges between type systems (Era.Core → engine), document complete property mapping table with type conversions and error handling.

---

### Issue 51: GlobalStatic Reset Incomplete Coverage (F558)

**Symptom**: Service registered in GlobalStatic but only one service field verified in Reset() method AC.

**Example (Bad)**:
```markdown
| 21 | GlobalStatic.Reset() sets services to null | code | Grep | contains | "_variableSizeService = null" | [ ] |
```
(But feature also creates _gameBaseService which also needs reset verification)

**Example (Good)**:
```markdown
| 21 | GlobalStatic.Reset() sets VariableSizeService to null | code | Grep | contains | "_variableSizeService = null" | [ ] |
| 22 | GlobalStatic.Reset() sets GameBaseService to null | code | Grep | contains | "_gameBaseService = null" | [ ] |
```

**Fix**: When feature adds multiple service fields to GlobalStatic, add separate AC for each field's Reset() verification.

---

### Issue 52: AC Expected Column Contains Test Commands (F558)

**Symptom**: AC Expected column contains full test commands instead of matcher values, bloating table width.

**Example (Bad)**:
```markdown
| 11 | Service test | test | Bash | succeeds | dotnet test --filter FullyQualifiedName~GlobalStaticIntegration_VariableSizeService | [ ] |
```

**Example (Good)**:
```markdown
| 11 | Service test | test | Bash | succeeds | dotnet test --filter FullyQualifiedName~GlobalStaticIntegration_VariableSizeService | [ ] |
```

With AC Details:
```markdown
**AC#11**: Service integration test
- Test: dotnet test --filter FullyQualifiedName~GlobalStaticIntegration_VariableSizeService
- Expected: Test passes with YAML values correctly populating ConstantData arrays
```

**Fix**: Test/build ACs can include full command in Expected column for clarity, but verify AC Details provide additional context about what success means.

---

### Issue 53: Task Restructuring Without AC Renumbering (F558)

**Symptom**: Feature restructures tasks for AC:Task 1:1 compliance but doesn't renumber AC table to match task split.

**Example (Bad)**:
```markdown
## Tasks (After restructuring for AC:Task 1:1)
| 1a | 1,15 | Create IVariableSizeService interface file | [ ] |
| 1b | 3,5,19 | Create VariableSizeService implementation | [ ] |

## Acceptance Criteria (Original numbering)
| 1 | VariableSizeService.cs exists | file | Glob | exists | ... |
| 3 | VariableSizeService class defined | code | Grep | contains | ... |
| 15 | IVariableSizeService.cs exists | file | Glob | exists | ... |
```

**Example (Good)**:
Either keep original AC numbering with clear AC:Task mapping (shown above), or renumber ACs to match new task structure and update all AC Details references consistently.

**Fix**: When restructuring tasks, decide whether to renumber ACs or keep original numbering. If keeping original, ensure AC:Task mapping table is clear. If renumbering, update all AC Details references.

---

### Issue 54: AC Details Test Setup References Nonexistent Constructor (F558)

**Symptom**: Test setup instructions reference creating objects with constructors that don't exist or aren't accessible.

**Example (Bad)**:
```markdown
**AC#11**: Test setup requirements
- (1) Create ConstantData with default constructor
```
(ConstantData is internal engine class, tests cannot create directly)

**Example (Good)**:
```markdown
**AC#11**: Test setup requirements
- (1) Call GlobalStatic.Reset(), (2) Create ConstantData via ProcessInitializer.LoadConstantData() or equivalent test harness
```

**Fix**: Verify object creation patterns in test setup instructions. Use factory methods, test harnesses, or builder patterns instead of assuming direct constructor access.

---

### Issue 55: DI Regex Pattern Too Broad

**Symptom**: AC DI registration uses `.*` regex pattern when exact literal pattern is sufficient.

**Example (Bad)**:
```markdown
| 13 | DI registration exists | code | Grep | contains | AddSingleton.*ICharacterLoader.*YamlCharacterLoader | [ ] |
```

**Example (Good)**:
```markdown
| 13 | DI registration exists | code | Grep | contains | AddSingleton<ICharacterLoader, YamlCharacterLoader> | [ ] |
```

**Fix**: Use exact literal patterns for DI registration when format is known and consistent.

---

### Issue 56: Technical Debt Pattern Inconsistency Across Features

**Symptom**: Features use different technical debt patterns (`TODO` only vs `TODO|FIXME|HACK`) causing inconsistency.

**Example (Bad)**:
```markdown
| 23 | Zero technical debt | code | Grep | not_contains | TODO | [ ] |
```
(F583 uses `TODO|FIXME|HACK`)

**Example (Good)**:
```markdown
| 23 | Zero technical debt | code | Grep | not_matches | TODO|FIXME|HACK | [ ] |
```

**Fix**: Use comprehensive `TODO|FIXME|HACK` pattern with `not_matches` matcher for consistency across all engine features. Note: Use `|` (plain pipe), not `\|` (backslash-pipe) — ripgrep treats `\|` as literal, not alternation.

---

### Issue 57: Test Filter Without Project Path Specification

**Symptom**: Test execution AC uses solution-level filter without explicit project path.

**Example (Bad)**:
```markdown
| 22 | Unit tests pass | exit_code | dotnet test --filter FullyQualifiedName~CharacterLoaderTests | succeeds | 0 | [ ] |
```

**Example (Good)**:
```markdown
| 22 | Unit tests pass | exit_code | dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s --filter FullyQualifiedName~CharacterLoaderTests | succeeds | 0 | [ ] |
```

**Fix**: Specify explicit project path in test commands for clarity and environment independence.

---

### Issue 58: Internal Stub Relationship Documentation Unclear

**Symptom**: Feature creates internal implementations for testing but relationship with public upgrade feature needs clarification.

**Example (Bad)**:
```markdown
**Note**: These are minimal stub implementations for F546 compatibility.
Full implementation in F548 (Composite Specifications).
```

**Example (Good)**:
```markdown
**Note**: These are functional implementations with `internal` visibility to enable F546 testing and compilation. F548 (Composite Specifications) will upgrade these to `public` classes inheriting from `SpecificationBase<T>` with enhanced features.
```

**Fix**: When creating internal implementations that will be upgraded by successor features, clearly document visibility difference and inheritance pattern changes rather than calling them "stubs".

---

### Issue 59: Static Helper Class vs Extension Methods Design Choice

**Symptom**: Creating new static helper classes when extension methods would be more appropriate for type-related operations.

**Example (Bad)**:
```markdown
**DisplayMode Extension Methods** (added to DisplayModeCapture.cs - same file pattern for types operating on OutputLine):

```csharp
public static class DisplayModeHelper
{
    public static string GetModeCounts(List<OutputLine> structuredOutput) { ... }
    public static bool DisplayModeEquals(string actual, string expected) { ... }
}
```

**Example (Good)**:
```markdown
**DisplayMode Extension Methods** (added to DisplayModeCapture.cs):

```csharp
public static class OutputLineExtensions
{
    public static string GetDisplayModeSummary(this List<OutputLine> structuredOutput) { ... }
    public static bool DisplayModeEquals(this OutputLine line, string expectedMode) { ... }
}
```

**Fix**: Use extension methods when operations are conceptually methods on the type being operated upon. This follows SOLID principles by keeping related functionality together.

---

### Issue 60: API Breaking Change Without Backward Compatibility Documentation

**Symptom**: Adding required parameters to existing method signatures without documenting backward compatibility strategy.

**Example (Bad)**:
```csharp
// New signature without overload documentation
public static List<ExpectCheckResult> Validate(
    KojoTestExpect expect,
    string output,
    string stderr,
    List<string> errors,
    List<string> warnings,
    Func<string, long?> getVariable,
    List<OutputLine> structuredOutput)  // NEW parameter
```

**Example (Good)**:
```markdown
**Backward Compatibility**:

The existing 6-parameter overload will delegate to the new 7-parameter signature with structuredOutput=null:

```csharp
public static List<ExpectCheckResult> Validate(
    KojoTestExpected expect,
    string output,
    string stderr,
    List<string> errors,
    List<string> warnings,
    Func<string, long?> getVariable)
{
    return Validate(expect, output, stderr, errors, warnings, getVariable, null);
}
```

**Fix**: When extending method signatures, always provide backward compatibility overloads and document the delegation pattern in Technical Design.

---

### Issue 62: Internal Class Testing Without InternalsVisibleTo (F790)

**Symptom**: AC requires unit tests that directly instantiate `internal sealed class` but project lacks `InternalsVisibleTo` attribute for test project.

**Example (Bad)**:
```markdown
### Implementation Contract
```csharp
internal sealed class EngineVariables : IEngineVariables { ... }
```

| 5 | Delegation test | test | dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s --filter ~EngineVariables | succeeds | - | [ ] |
```
(Era.Core.csproj has no InternalsVisibleTo for Era.Core.Tests — test cannot access internal class)

**Example (Good)**:
```markdown
### Implementation Contract

Add to Era.Core.csproj:
```xml
<ItemGroup>
  <InternalsVisibleTo Include="Era.Core.Tests" />
</ItemGroup>
```

| Task# | AC# | Description |
| 6 | 5 | Add InternalsVisibleTo to Era.Core.csproj |
| 7 | 5 | Create delegation tests for internal EngineVariables |
```

**Fix**: When AC requires testing `internal` classes, add a prerequisite Task for `<InternalsVisibleTo>` in the source project's csproj. Verify the attribute doesn't already exist before adding.

---

### Issue 61: AC Definition Removal Without Renumbering Consistency

**Symptom**: Removing ACs mid-sequence and renumbering without updating all cross-references consistently.

**Example (Bad)**:
```markdown
| AC# | Description | AC# | Description |
| 3 | JSON output exists | → | 3 | Console output |
| 4 | Console output |     | 4 | Validator method |
| 5 | Validator method |   | (AC #3 removed, others renumbered)
```
(But Tasks table still references old AC#4, AC#5)

**Example (Good)**:
```markdown
When removing AC#3:
1. Update AC Definition Table (renumber 4→3, 5→4, etc.)
2. Update Tasks table AC# column to match new numbering
3. Update AC Details headers (AC#4: → AC#3:)
4. Update Success Criteria AC count
5. Update Implementation Contract AC references
```

**Fix**: AC removal requires systematic renumbering of ALL references (Tasks, AC Details, Success Criteria, Implementation Contract) to maintain consistency.

---

### Issue 71: AC Alternation Matcher Allows Partial Verification (F796)

**Symptom**: A single AC uses regex alternation (`A|B`) to check two independent conditions. One match of either satisfies the AC, hiding that the other condition wasn't met.

**Example (Bad)**:
```markdown
| 16 | GetCFlag/SetCFlag helpers use _variables directly | code | Grep(BodySettings.cs) | matches | `_variables\.GetCharacterFlag|_variables\.SetCharacterFlag` |
```
(One match of either GetCharacterFlag OR SetCharacterFlag passes — SetCFlag could be broken while AC passes from GetCFlag match alone)

**Example (Good)**:
```markdown
| 16a | GetCFlag helper uses _variables.GetCharacterFlag | code | Grep(BodySettings.cs) | matches | `_variables\.GetCharacterFlag` |
| 16b | SetCFlag helper uses _variables.SetCharacterFlag | code | Grep(BodySettings.cs) | matches | `_variables\.SetCharacterFlag` |
```

**Fix**: Split alternation ACs into separate ACs when each alternative verifies an independent condition that must hold simultaneously. Alternation is fine when checking for ANY match (e.g., `TODO|FIXME|HACK` in `not_matches`).

---

### Issue 72: Circular Test-AC Verification Without Ground Truth (F796)

**Symptom**: A Task rewrites test expected values and an AC checks those rewritten tests pass. The AC cannot distinguish correct from incorrect expected values since the tests define their own pass criteria.

**Example (Bad)**:
```markdown
Task#4: Rewrite BodyDetailInit tests to use MockVariableStore
AC#13: dotnet test engine.Tests --filter BodyDetailInit | succeeds
```
(Task#4 rewrites tests → AC#13 checks those same tests pass → circular: wrong expected values still pass)

**Example (Good)**:
```markdown
Task#4: Rewrite BodyDetailInit tests to use MockVariableStore
AC#13: dotnet test engine.Tests --filter BodyDetailInit | succeeds
AC#23: Grep(test files) | matches | `Gender.*0.*HairLength.*100`
```
(AC#23 pins a concrete ground-truth value from source code — Character0Male.HairLength=100 — breaking the circularity)

**Fix**: When a Task rewrites tests, add a ground-truth pinning AC that statically verifies at least one concrete expected value from source code appears in the test assertion. The pinned value must be invariant across the migration (only the access mechanism changes, not the data). When a feature migrates multiple methods/classes, ensure ground-truth pinning ACs exist for EACH migrated method, not just the primary one (F797: UterusVolumeInit had AC#13/AC#16 but TemperatureResistance initially lacked pinning ACs).
