# Feature 571: Kojo Rendering Integration

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

## Background

### Philosophy (Mid-term Vision)
YAML COM system should integrate seamlessly with kojo rendering for character dialogue and interaction display. Kojo rendering integration enables dynamic character responses based on COM effects while maintaining proper text rendering and localization support. **YamlComExecutor.RenderKojo should be the single integration point for kojo rendering, ensuring consistent dialogue retrieval across all COM execution paths (SSOT).** Context adapters are responsibility-scoped: EffectContext (F565) for effect execution, ComEvaluationContext for kojo condition evaluation. Both are internal implementation details of YamlComExecutor, not public SSOT interfaces. Note: IComContext.EvalContext (Dictionary<string,object>) is for legacy loose-typed value passing; ComEvaluationContext (IEvaluationContext) provides typed interface required by IKojoEngine.GetDialogue.

### Problem (Current Issue)
F565 implements YAML COM runtime but RenderKojo method returns placeholder - full integration needed for character dialogue system. Current implementation lacks connection between COM effects and kojo text generation.

### Goal (What to Achieve)
Integrate YAML COM runtime with kojo rendering system to enable dynamic character dialogue based on COM effect results.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Placeholder comment removed | code | Grep(Era.Core/Commands/Com/YamlComExecutor.cs) | not_contains | "NOTE.*Implement kojo rendering" | [ ] |
| 2 | KojoEngine GetDialogue integration | code | Grep(Era.Core/Commands/Com/YamlComExecutor.cs) | contains | "context.Kojo.GetDialogue" | [ ] |
| 3 | DialogueResult handling | code | Grep(Era.Core/Commands/Com/YamlComExecutor.cs) | contains | "DialogueResult" | [ ] |
| 4 | ComEvaluationContext adapter | file | Glob(Era.Core/Commands/Com/) | exists | "ComEvaluationContext.cs" | [ ] |
| 5 | RenderKojo test coverage | test | dotnet test --filter | succeeds | "FullyQualifiedName~RenderKojo" | [ ] |
| 6 | Missing kojo file handling | test | dotnet test --filter | succeeds | "FullyQualifiedName~RenderKojo_MissingFile" | [ ] |
| 7 | Invalid dialogue handling | test | dotnet test --filter | succeeds | "FullyQualifiedName~RenderKojo_NoMatchingCondition" | [ ] |
| 8 | Build succeeds | build | dotnet build | succeeds | - | [ ] |
| 9 | All tests pass | test | dotnet test | succeeds | - | [ ] |
| 10 | ComEvaluationContext unit tests | test | dotnet test --filter | succeeds | "FullyQualifiedName~ComEvaluationContext" | [ ] |
| 11 | ComEvaluationContext implements IEvaluationContext | code | Grep(Era.Core/Commands/Com/ComEvaluationContext.cs) | contains | "IEvaluationContext" | [ ] |

### AC Details

**AC#1**: Era.Core/Commands/Com/YamlComExecutor.cs
- Test: `Grep "NOTE.*Implement kojo rendering" Era.Core/Commands/Com/YamlComExecutor.cs`
- Verifies placeholder comment is removed after implementation

**AC#2**: Era.Core/Commands/Com/YamlComExecutor.cs
- Test: `Grep "context.Kojo.GetDialogue" Era.Core/Commands/Com/YamlComExecutor.cs`
- Verifies RenderKojo calls IKojoEngine.GetDialogue method

**AC#3**: Era.Core/Commands/Com/YamlComExecutor.cs
- Test: `Grep "DialogueResult" Era.Core/Commands/Com/YamlComExecutor.cs`
- Verifies DialogueResult type is used for dialogue retrieval

**AC#4**: Era.Core/Commands/Com/ComEvaluationContext.cs
- Test: `Glob "Era.Core/Commands/Com/ComEvaluationContext.cs"`
- Verifies ComEvaluationContext adapter class file exists

**AC#5-7**: Era.Core.Tests/Commands/Com/YamlComExecutorTests.cs
- Positive test (AC#5): Verifies RenderKojo returns dialogue from KojoEngine
- Negative test (AC#6): Verifies graceful handling when kojo file missing
- Negative test (AC#7): Verifies graceful handling when no condition matches

**AC#10**: Era.Core.Tests/Commands/Com/ComEvaluationContextTests.cs
- Unit tests for ComEvaluationContext class
- Verifies CurrentCharacter maps from IComContext.Target
- Verifies Variables access through injected IVariableStore

**AC#11**: Era.Core/Commands/Com/ComEvaluationContext.cs
- Test: `Grep "IEvaluationContext" Era.Core/Commands/Com/ComEvaluationContext.cs`
- Verifies ComEvaluationContext implements IEvaluationContext interface

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Remove placeholder implementation from RenderKojo | [x] |
| 2 | 2 | Integrate KojoEngine.GetDialogue call in RenderKojo | [x] |
| 3 | 3 | Handle DialogueResult from GetDialogue response | [x] |
| 4 | 4,11 | Create ComEvaluationContext class (IEvaluationContext impl) | [x] |
| 5 | 5 | Add RenderKojo positive test | [x] |
| 6 | 6 | Add RenderKojo_MissingFile negative test | [x] |
| 7 | 7 | Add RenderKojo_NoMatchingCondition negative test | [x] |
| 8 | 8 | Verify build succeeds | [x] |
| 9 | 9 | Verify all tests pass | [x] |
| 10 | 10 | Add ComEvaluationContext unit tests | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### RenderKojo Integration Steps

1. **Create ComEvaluationContext adapter class**
   - Create `Era.Core/Commands/Com/ComEvaluationContext.cs`
   - Implements `Era.Core.Functions.IEvaluationContext` interface
   - Constructor takes `IComContext context` and `IVariableStore variables` parameters
   - Member implementations:
     - `GetArg<T>()` / `GetArgs<T>()` / `ArgCount` → return default/empty (not used in kojo context)
     - `CurrentCharacter` → `context.Target` (CharacterId → CharacterId? implicit conversion)
     - `Variables` → use injected `IVariableStore` (passed from `YamlComExecutor._variables` field)

2. **Integrate KojoEngine.GetDialogue call**
   ```csharp
   // Helper method in YamlComExecutor:
   private IEvaluationContext CreateEvaluationContext(IComContext context)
       => new ComEvaluationContext(context, _variables);

   // In RenderKojo method:
   var characterId = context.Target;  // IComContext.Target is already CharacterId
   var comId = new ComId(com.Id);
   var evalContext = CreateEvaluationContext(context);
   var result = context.Kojo.GetDialogue(characterId, comId, evalContext);
   ```

3. **Handle DialogueResult**
   ```csharp
   return result.Match(
       onSuccess: dialogue => string.Join("\n", dialogue.Lines),
       onFailure: error => $"[COM {com.Id}: {com.Name}]"  // Fallback on error
   );
   ```

4. **Error handling strategy**
   - Missing kojo file → Return fallback message (graceful degradation)
   - No matching condition → Return fallback message
   - Do not fail COM execution on errors

### Test Implementation Steps

5. **Create mock IKojoEngine for testing**
   - Mock returns Success with DialogueResult for positive test
   - Mock returns Fail for negative tests (missing file, no match)

6. **Add test methods to YamlComExecutorTests.cs**
   - `RenderKojo_WithValidDialogue_ReturnsDialogueText` (AC#5)
   - `RenderKojo_MissingFile_ReturnsFallback` (AC#6)
   - `RenderKojo_NoMatchingCondition_ReturnsFallback` (AC#7)

---

## Dependencies

| Type | Feature | Relationship | Notes |
|------|---------|--------------|-------|
| Predecessor | F565 | Successor | COM YAML Runtime Integration |

---

## Links

- [index-features.md](index-features.md)
- [feature-565.md](feature-565.md) - COM YAML Runtime Integration

---

## Review Notes

- [resolved] Phase1 iter1: Philosophy SSOT claim incorporated into Philosophy section.
- [resolved] Phase1 iter3: Namespace 'Era.Core.Functions.IEvaluationContext' added to Implementation Contract Step 1.
- [resolved] Phase2 iter5: LSP concern - Design is intentional: KojoEngine only uses CurrentCharacter/Variables, GetArg/ArgCount not called.
- [resolved] Phase2 iter5: IComContext.EvalContext vs ComEvaluationContext relationship - clarified in Philosophy: EvalContext for legacy loose-typed, ComEvaluationContext for typed IKojoEngine interface.
- [resolved] Phase2 iter5: CurrentCharacter mapping verification - covered by AC#10 unit tests (see AC#10 Details).
- [resolved] Phase1 iter8: CreateEvaluationContext helper method is covered by Task#2 (Implementation Contract Step 2 shows the code pattern).

---

## Mandatory Handoffs

| Issue | Reason | Tracking Destination | Destination ID |
|-------|--------|---------------------|----------------|
| (No mandatory handoffs) | All work within scope | - | - |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-21 08:48 | START | implementer | Tasks 1-10 | - |
| 2026-01-21 08:48 | END | implementer | Tasks 1-10 | SUCCESS |

---