# Feature 551: TemplateDialogueRenderer Implementation

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

## Created: 2026-01-18

---

## Summary

Implement `TemplateDialogueRenderer` concrete class for IDialogueRenderer interface to enable template variable replacement in dialogue text. This implementation provides the Rendering responsibility extracted from KojoEngine monolith.

**Output**: `Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs`

**Scope**: Single file (~150 lines) implementing IDialogueRenderer with template string variable substitution using context values.

---

## Background

### Philosophy (Mid-term Vision)

Phase 18: KojoEngine SRP分割 - Concrete implementations of extracted interfaces enable independent testing, DI injection, and future extensibility. TemplateDialogueRenderer establishes extensible foundation for dialogue template variable substitution with initial support for CALLNAME and TALENT variables, designed for future expansion to all ERA variable types.

### Problem (Current Issue)

IDialogueRenderer interface (F544) requires concrete implementation for dialogue template rendering. Current KojoEngine has template rendering logic tightly coupled with loading and evaluation, preventing independent testing and reuse.

### Goal (What to Achieve)

Create TemplateDialogueRenderer class with:
- Render(string template, IEvaluationContext context) method returning Result<string>
- Variable substitution (e.g., `{CharacterName}`, `{AblValue}`) using context
- Error handling for undefined variables with descriptive Result.Fail messages
- Zero technical debt (no TODO/FIXME/HACK markers)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | TemplateDialogueRenderer.cs exists | file | Glob | exists | "Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs" | [x] |
| 2 | Implements IDialogueRenderer | code | Grep | contains | "class TemplateDialogueRenderer.*IDialogueRenderer" | [x] |
| 3 | Render method signature | code | Grep | contains | "Result<string> Render\\(string template.*IEvaluationContext" | [x] |
| 4 | Variable substitution pattern | code | Grep | contains | "\\{[^}]+\\}" | [x] |
| 5 | Context variable access | code | Grep | contains | "context\\.Variables\\." | [x] |
| 6 | Undefined variable handling | code | Grep | contains | "Result<string>.Fail" | [x] |
| 7 | Null validation | code | Grep | contains | "ArgumentNullException" | [x] |
| 8 | Unit tests exist | file | Glob | exists | "Era.Core.Tests/Dialogue/Rendering/TemplateDialogueRendererTests.cs" | [x] |
| 9 | Simple substitution test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TemplateDialogueRendererTests.TestRenderCallName" | [x] |
| 10 | Multiple variables test | test | Bash | succeeds | "dotnet test --filter FullyQualifiedName~TemplateDialogueRendererTests.TestRenderTalentValue" | [x] |
| 11 | Zero technical debt | code | Grep | not_contains | "TODO\\|FIXME\\|HACK" | [x] |
| 12 | ITemplateVariableResolver.cs exists | file | Glob | exists | "Era.Core/Dialogue/Rendering/ITemplateVariableResolver.cs" | [x] |

### AC Details

**AC#1**: File existence verification
- Test: Glob pattern="Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs"
- Expected: File exists

**AC#2**: Interface implementation
- Test: Grep pattern="class TemplateDialogueRenderer.*IDialogueRenderer" path="Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs" type=cs
- Expected: Class declaration implements IDialogueRenderer

**AC#3**: Render method signature
- Test: Grep pattern="Result<string> Render\\(string template.*IEvaluationContext" path="Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs" type=cs
- Expected: Matches IDialogueRenderer.Render signature

**AC#4**: Variable substitution pattern
- Test: Grep pattern="\\{[^}]+\\}" path="Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs" type=cs
- Expected: Uses regex pattern to find variables in template (e.g., `{CALLNAME}`, `{TALENT:Affection}`)

**AC#5**: Context variable access
- Test: Grep pattern="context\\.Variables\\." path="Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs" type=cs
- Expected: Accesses variables from IEvaluationContext via Variables property

**AC#6**: Undefined variable handling
- Test: Grep pattern="Result<string>.Fail" path="Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs" type=cs
- Expected: Uses Result pattern for error handling (undefined variable)

**AC#7**: Null validation
- Test: Grep pattern="ArgumentNullException" path="Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs" type=cs
- Expected: Validates template/context parameters

**AC#8**: Unit tests exist
- Test: Glob pattern="Era.Core.Tests/Dialogue/Rendering/TemplateDialogueRendererTests.cs"
- Expected: Test file exists

**AC#9**: Simple substitution test
- Test: `dotnet test --filter FullyQualifiedName~TemplateDialogueRendererTests.TestRenderCallName`
- Expected: Test verifies CALLNAME variable substitution (template="{CALLNAME}" → placeholder "Character{Id}")

**AC#10**: Multiple variables test
- Test: `dotnet test --filter FullyQualifiedName~TemplateDialogueRendererTests.TestRenderTalentValue`
- Expected: Test verifies TALENT variable substitution (template="好感度: {TALENT:Affection}" → "好感度: 5")

**AC#11**: Zero technical debt
- Test: Grep pattern="TODO\\|FIXME\\|HACK" path="Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs" type=cs
- Expected: 0 matches (no debt markers introduced during implementation)

**AC#12**: ITemplateVariableResolver.cs exists
- Test: Glob pattern="Era.Core/Dialogue/Rendering/ITemplateVariableResolver.cs"
- Expected: Interface file exists for extensible variable resolution


---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create TemplateDialogueRenderer.cs file in Era.Core/Dialogue/Rendering/ | [x] |
| 2 | 2 | Implement class TemplateDialogueRenderer implementing IDialogueRenderer interface | [x] |
| 3 | 3 | Implement Render(string template, IEvaluationContext context) method signature | [x] |
| 4 | 4 | Implement variable substitution pattern matching for {VARIABLE} and {VARIABLE:param} | [x] |
| 5 | 5 | Implement context variable access via context.Variables property | [x] |
| 6 | 6 | Implement undefined variable error handling with Result<string>.Fail | [x] |
| 7 | 7 | Implement null parameter validation with ArgumentNullException | [x] |
| 8 | 8 | Create unit test file TemplateDialogueRendererTests.cs | [x] |
| 9 | 9 | Implement TestRenderCallName test for CALLNAME variable substitution | [x] |
| 10 | 10 | Implement TestRenderTalentValue test for TALENT variable substitution | [x] |
| 11 | 11 | Verify zero technical debt (no TODO/FIXME/HACK comments) | [x] |
| 12 | 12 | Create ITemplateVariableResolver.cs interface file in Era.Core/Dialogue/Rendering/ | [x] |
| 13 | - | Create feature-628.md for character data service to resolve CALLNAME placeholder tracking | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Investigation

### ERA Variable Types Analysis

From ERA system analysis and eraTW codebase review, template variables include:

| Variable Type | Example | Purpose | Future Priority |
|---------------|---------|---------|-----------------|
| CALLNAME | {CALLNAME} | Character display name | ✅ F551 |
| TALENT | {TALENT:Affection} | Character talents (indexed) | ✅ F551 |
| ABL | {ABL:体力} | Character abilities | 🔄 F628+ |
| PALAM | {PALAM:快C} | Character parameters | 🔄 F628+ |
| EXP | {EXP:Ｃ経験} | Experience values | 🔄 F628+ |
| FLAG | {FLAG:告白済み} | Global flags | 🔄 F628+ |
| CFLAG | {CFLAG:興奮度} | Character flags | 🔄 F628+ |
| JUEL | {JUEL:Ｖ珠} | Character jewels/accessories | 🔄 F628+ |
| BASE | {BASE:体力} | Base stats before modifiers | 🔄 F628+ |

### Design Extension Strategy

The ITemplateVariableResolver pattern enables incremental addition of variable types without modifying core TemplateDialogueRenderer logic. Each variable type can be implemented as separate resolver class with specific IVariableStore access patterns.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Interface Reference

From F544 (IDialogueRenderer Interface Extraction):

```csharp
// Era.Core/Dialogue/Rendering/IDialogueRenderer.cs
using Era.Core.Types;

namespace Era.Core.Dialogue.Rendering;

public interface IDialogueRenderer
{
    /// <summary>Render dialogue template with context variables</summary>
    Result<string> Render(string template, IEvaluationContext context);
}

// Era.Core/Dialogue/Rendering/ITemplateVariableResolver.cs
using Era.Core.Types;
using Era.Core.Functions;

namespace Era.Core.Dialogue.Rendering;

public interface ITemplateVariableResolver
{
    /// <summary>Resolve specific variable type with optional parameter</summary>
    Result<string> ResolveVariable(IEvaluationContext context, string parameter);
}
```

### Implementation Template

```csharp
// Era.Core/Dialogue/Rendering/TemplateDialogueRenderer.cs
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Era.Core.Types;
using Era.Core.Functions;

namespace Era.Core.Dialogue.Rendering;

public class TemplateDialogueRenderer : IDialogueRenderer
{
    private static readonly Regex VariablePattern = new Regex(@"\{([^}:]+)(?::([^}]+))?\}", RegexOptions.Compiled);
    private readonly ConcurrentDictionary<string, ITemplateVariableResolver> _resolvers;

    public TemplateDialogueRenderer()
    {
        _resolvers = new ConcurrentDictionary<string, ITemplateVariableResolver>(StringComparer.OrdinalIgnoreCase);

        // Register default resolvers
        RegisterResolver("CALLNAME", new CallNameResolver());
        RegisterResolver("TALENT", new TalentResolver());
    }

    public void RegisterResolver(string variableType, ITemplateVariableResolver resolver)
    {
        _resolvers[variableType] = resolver;
    }

    public Result<string> Render(string template, IEvaluationContext context)
    {
        if (template == null)
            throw new ArgumentNullException(nameof(template));
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var result = template;
        var matches = VariablePattern.Matches(template);

        foreach (Match match in matches)
        {
            var variableType = match.Groups[1].Value;
            var variableParam = match.Groups[2].Value;

            if (!_resolvers.TryGetValue(variableType, out var resolver))
            {
                return Result<string>.Fail($"Undefined variable in template: {variableType}");
            }

            var resolveResult = resolver.ResolveVariable(context, variableParam);
            if (resolveResult is Result<string>.Failure f)
            {
                return Result<string>.Fail($"Error resolving variable {variableType}: {f.Error}");
            }

            var value = ((Result<string>.Success)resolveResult).Value;
            result = result.Replace(match.Value, value);
        }

        return Result<string>.Ok(result);
    }
}

internal class CallNameResolver : ITemplateVariableResolver
{
    public Result<string> ResolveVariable(IEvaluationContext context, string parameter)
    {
        var characterId = context.CurrentCharacter;
        if (characterId == null)
            return Result<string>.Fail("No current character available");

        return Result<string>.Ok($"Character{characterId}");
    }
}

internal class TalentResolver : ITemplateVariableResolver
{
    public Result<string> ResolveVariable(IEvaluationContext context, string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            return Result<string>.Fail("TALENT variable requires parameter");

        var characterId = context.CurrentCharacter;
        if (characterId == null)
            return Result<string>.Fail("No current character available");

        var talentIndex = GetTalentIndexByName(parameter);
        if (talentIndex == null)
            return Result<string>.Fail($"Invalid talent name: {parameter}");

        var talentResult = context.Variables.GetTalent(characterId.Value, talentIndex.Value);
        return talentResult is Result<int>.Success ts
            ? Result<string>.Ok(ts.Value.ToString())
            : Result<string>.Fail($"Failed to get talent {parameter}");
    }

    private static TalentIndex? GetTalentIndexByName(string talentName)
    {
        var field = typeof(TalentIndex).GetField(talentName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        return field?.GetValue(null) as TalentIndex?;
    }
}
```


### Error Message Format

| Error Type | Format |
|------------|--------|
| Undefined variable | `"Undefined variable in template: {variableName}"` |

### Test Cases (Minimum)

| Test Name | Scenario | Expected Result |
|-----------|----------|-----------------|
| TestRenderCallName | Template="{CALLNAME}", context with character | Result.Success with "Character{Id}" |
| TestRenderTalentValue | Template="好感度: {TALENT:Affection}", context with character talent | Result.Success with "好感度: 5" |
| TestRenderNoVariables | Template="こんにちは" (no variables) | Result.Success with original string |
| TestRenderUndefinedVariable | Template="{Unknown}", unknown variable type | Result.Fail with "Undefined variable" |
| TestRenderNullTemplate | Null template argument | ArgumentNullException |
| TestRenderNullContext | Null context argument | ArgumentNullException |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->

- [resolved-applied] Phase1 iter1: Implementation Contract template used invalid context.GetVariable() method. Revised to use actual IEvaluationContext.Variables access pattern with type-specific variable resolution.
- [resolved-applied] Phase1 iter1: AC#3 pattern updated to reflect correct implementation approach.
- [resolved-applied] Phase1 iter1: AC#5 pattern changed from context.GetVariable to context.Variables. to match actual API.
- [resolved-applied] Phase1 iter1: Test cases revised to use CALLNAME and TALENT variable types instead of arbitrary Name property.
- [resolved-applied] Phase1 iter1: AC#4 pattern updated to handle variable format with optional parameters (e.g., {TALENT:恋慕}).
- [resolved-applied] Phase1 iter2: AC:Task 1:1 violation fixed. Split 5 tasks into 12 tasks for 1:1 correspondence with ACs.
- [resolved-applied] Phase1 iter2: Enum.TryParse<TalentIndex> fixed. Replaced with reflection-based lookup for TalentIndex static readonly fields.
- [resolved-applied] Phase1 iter2: Result<int> handling for GetTalent() fixed. Added proper Result unwrapping with error handling.
- [resolved-applied] Phase1 iter2: TODO comment removed to satisfy AC#11 zero technical debt. CALLNAME placeholder implementation tracked to F553.
- [resolved-applied] Phase1 iter3: TalentIndex reflection lookup fixed. Test template changed from {TALENT:恋慕} to {TALENT:Affection} to match English field names.
- [resolved-applied] Phase1 iter3: AC#12 DI registration removed due to conflict with F544 handoff. DI registration deferred to F553 per F544's explicit handoff.
- [resolved-applied] Phase1 iter3: Ambiguous handoff F553#T{N} fixed. Changed tracking destination to new Feature F570 for character data service implementation.
- [resolved-applied] Phase1 iter4: Invalid handoff destination F570 fixed. Changed to proper next Feature ID F628 for character data service.
- [resolved-applied] Phase1 iter4: Invalid using Era.Core.Enums; removed from Implementation Template. TalentIndex is in Era.Core.Types namespace.
- [resolved-applied] Phase1 iter4: AC#4 pattern updated from \\{[^}:]+\\} to \\{[^}]+\\} to match optional parameter format like {TALENT:Affection}.
- [resolved-applied] Phase1 iter5: Invalid C# switch expression syntax fixed. Changed switch expression with return statement to switch statement with early returns.
- [resolved-applied] Phase1 iter6: Result<T> API usage fixed. Replaced invalid .Value and .Error access with proper pattern matching (is Result<int>.Success ts).
- [resolved-applied] Phase1 iter6: F628 handoff tracking fixed. Added Task#12 to create feature-628.md file per Deferred Task Protocol requirement.
- [resolved-applied] Phase1 iter7: AC#9 test description fixed. Updated to expect placeholder "Character{Id}" instead of actual character name to match implementation behavior.
- [resolved-invalid] Phase1 iter7: Task#12 has AC# as '-' violating AC:Task 1:1 principle. Invalid per F545 precedent: Creation tasks for Deferred Task Protocol are exempt from AC:Task 1:1 rule.
- [resolved-applied] Phase2 iter8: OCP violation fixed. Replaced hard-coded switch with ITemplateVariableResolver strategy pattern for extensible variable type registration.
- [resolved-applied] Phase2 iter8: Philosophy scope narrowed from "all dialogue template rendering" to "extensible foundation with initial CALLNAME/TALENT support" to match implementation.
- [resolved-applied] Phase2 iter8: Added Investigation section documenting ERA variable types analysis and extension strategy for Zero Debt Upfront compliance.
- [resolved-applied] Phase1 iter9: IVariableResolver naming collision fixed. Renamed to ITemplateVariableResolver to distinguish from existing Era.Core.Interfaces.IVariableResolver.
- [resolved-applied] Phase1 iter9: Interface Reference inconsistency fixed. Moved ITemplateVariableResolver to separate file per Interface Reference section.
- [resolved-applied] Phase1 iter9: Missing using Era.Core.Functions; added for IEvaluationContext namespace access.
- [resolved-applied] Phase1 iter10: ITemplateVariableResolver interface creation task added. Added AC#12 and Task#12 for interface file existence verification.
- [resolved-invalid] Phase1 iter10: AC#5 context.Variables. pattern concern invalid - TalentResolver is internal class in same file, pattern will match.
- [resolved-invalid] Phase1 iter10: Missing using System.Collections.Concurrent; concern invalid - using statement already present in Implementation Template.

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

| 課題 | 理由 | 追跡先 | 追跡先ID |
|------|------|--------|----------|
| CALLNAME variable returns placeholder "Character{Id}" instead of actual character names | Requires character data access service not available in F551 scope | Create new Feature for character data service | F628 |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | feature creation | PENDING |
| 2026-01-26 08:10 | START | implementer | Task 1-12 | - |
| 2026-01-26 08:10 | END | implementer | Task 1-12 | SUCCESS |
| 2026-01-26 08:15 | END | orchestrator | Task 13 (F628 handoff) | SUCCESS |
| 2026-01-26 08:20 | DEVIATION | feature-reviewer | doc-check | NEEDS_REVISION: Status/Dependencies |

## Dependencies

| Type | ID | Description | Status |
|------|----|-------------|:------:|
| Predecessor | F544 | IDialogueRenderer Interface Extraction | [DONE] |
| Related | F549 | YamlDialogueLoader Implementation | - |
| Related | F550 | ConditionEvaluator Implementation | - |
| Related | F552 | PriorityDialogueSelector Implementation | - |
| Successor | F553 | KojoEngine Facade Refactoring | - |

## Links

- [index-features.md](index-features.md)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 18 section lines 3917-4037
- [F541: Phase 18 Planning](feature-541.md)
- [F544: IDialogueRenderer Interface Extraction](feature-544.md)
- [F553: KojoEngine Facade Refactoring](feature-553.md)
- [feature-template.md](reference/feature-template.md)
