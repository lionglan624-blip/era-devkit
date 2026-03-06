# Feature 543: IConditionEvaluator Interface Extraction

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

## Background

### Philosophy (Mid-term Vision)

Phase 18: KojoEngine SRP分割 - Splitting KojoEngine monolith into single-responsibility components following SOLID principles. This establishes IConditionEvaluator as the single source of truth for all dialogue condition evaluation operations, ensuring consistent evaluation behavior and simplified future maintenance.

### Problem (Current Issue)

KojoEngine (391-line monolith) violates SRP by combining loading, evaluation, rendering, and selection responsibilities. Evaluation responsibility (condition checking for TALENT/ABL/EXP branching) needs extraction into dedicated interface for:
- Testability: Mock evaluation during unit tests
- Extensibility: Support complex specifications via Specification Pattern (F546-F548)
- Maintainability: Isolate condition logic from I/O and rendering

### Goal (What to Achieve)

Extract IConditionEvaluator interface defining evaluation contract:
- Evaluate DialogueCondition against IEvaluationContext
- Boolean return type for simple true/false logic
- Establish foundation for ConditionEvaluator implementation (F550)

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | DialogueCondition.cs exists | file | Glob | exists | "Era.Core/Dialogue/Conditions/DialogueCondition.cs" | [x] |
| 2 | IConditionEvaluator.cs exists | file | Glob | exists | "Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs" | [x] |
| 3 | Interface is public | code | Grep(Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs) | contains | "public interface IConditionEvaluator" | [x] |
| 4 | Evaluate method signature | code | Grep(Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs) | contains | "bool Evaluate(DialogueCondition condition, IEvaluationContext context)" | [x] |
| 5 | XML documentation on interface | code | Grep(Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs) | contains | "/// <summary>" | [x] |
| 6 | XML documentation on Evaluate | code | Grep(Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs) | contains | "<param name=\"condition\">" | [x] |
| 7 | IConditionEvaluator namespace | code | Grep(Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs) | contains | "namespace Era.Core.Dialogue.Evaluation" | [x] |
| 8 | DialogueCondition namespace | code | Grep(Era.Core/Dialogue/Conditions/DialogueCondition.cs) | contains | "namespace Era.Core.Dialogue.Conditions" | [x] |
| 9 | Using statements present | code | Grep(Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs) | contains | "using Era.Core" | [x] |
| 10 | Zero technical debt | code | Grep(Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs) | not_contains | "TODO" | [x] |
| 11 | Tests PASS | test | Bash | succeeds | "dotnet test" | [x] |

### AC Details

**AC#1**: File existence verification
- Test: `Glob("Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs")`
- Expected: File exists

**AC#2**: Interface accessibility
- Test: `Grep("public interface IConditionEvaluator", "Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs")`
- Expected: Interface is public for external use

**AC#3**: Evaluate method signature
- Test: `Grep("bool Evaluate\\(DialogueCondition condition, IEvaluationContext context\\)", "Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs")` [regex match]
- Expected: Method returns bool with DialogueCondition and IEvaluationContext parameters

**AC#4**: XML documentation on interface
- Test: `Grep("/// <summary>", "Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs")` [literal match]
- Expected: Interface has XML summary documentation

**AC#5**: XML documentation on Evaluate
- Test: `Grep("<param name=\"condition\">", "Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs")` [literal match]
- Expected: Evaluate method has parameter documentation for condition

**AC#6**: Namespace declaration
- Test: `Grep("namespace Era.Core.Dialogue.Evaluation", "Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs")`
- Expected: Namespace matches directory structure per ENGINE.md Issue 21

**AC#7**: Using statements present
- Test: `Grep("using Era.Core", "Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs")`
- Expected: DialogueCondition and IEvaluationContext types require Era.Core namespace (pattern matches subnamespaces)

**AC#8**: Zero technical debt
- Test: `Grep("TODO|FIXME|HACK", "Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs")` [not_matches regex alternation]
- Expected: 0 matches (no technical debt in new interface)

**AC#9**: Tests PASS
- Test: `dotnet test`
- Expected: All existing tests continue to pass (no regression)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create Era.Core/Dialogue/Conditions/ directory and DialogueCondition record | [x] |
| 2 | 2 | Create Era.Core/Dialogue/Evaluation/ directory structure | [x] |
| 3 | 3,4,7,9 | Define IConditionEvaluator interface with Evaluate method | [x] |
| 4 | 5,6 | Add XML documentation to interface and method | [x] |
| 5 | 8 | Verify DialogueCondition namespace declaration | [x] |
| 6 | 10 | Verify zero technical debt markers in implementation | [x] |
| 7 | 11 | Verify all tests PASS | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->
<!-- Batch Waiver: T2 (AC#2,3,6,7) = related interface creation elements, T3 (AC#4,5) = documentation pair -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Interface Specification

Per architecture.md lines 3927-3931:

```csharp
// Era.Core/Dialogue/Conditions/DialogueCondition.cs
using Era.Core.Functions;

namespace Era.Core.Dialogue.Conditions;

public record DialogueCondition(
    string Type,
    string? TalentType = null,
    string? AblType = null,
    int? Threshold = null,
    string? Operand = null
);

// Era.Core/Dialogue/Evaluation/IConditionEvaluator.cs
using Era.Core.Dialogue.Conditions;
using Era.Core.Functions;

namespace Era.Core.Dialogue.Evaluation;

/// <summary>
/// Condition evaluation interface following SRP principle.
/// Responsible only for evaluating dialogue conditions, not loading or rendering.
/// </summary>
public interface IConditionEvaluator
{
    /// <summary>
    /// Evaluate a dialogue condition against the provided context.
    /// </summary>
    /// <param name="condition">Condition to evaluate (TALENT/ABL/EXP branching)</param>
    /// <param name="context">Evaluation context containing character state</param>
    /// <returns>True if condition is satisfied, false otherwise</returns>
    bool Evaluate(DialogueCondition condition, IEvaluationContext context);
}
```

### Design Rationale

**Why Boolean Return?**
- Conditions are binary: satisfied or not satisfied
- No partial satisfaction or error states in condition evaluation
- Callers use result directly in if/switch statements

**Why IEvaluationContext Parameter?**
- Encapsulates character state (TALENT, ABL, EXP values)
- Prevents exposing GlobalStatic or internal engine state
- Supports multiple evaluation contexts (player, NPC, historical state)

**Why Not Result&lt;bool&gt;?**
- Condition evaluation is infallible given valid context
- Invalid conditions should be caught during YAML parsing (F549), not runtime
- Result&lt;T&gt; adds overhead without error handling benefit

### Integration with Specification Pattern

IConditionEvaluator will delegate to ISpecification&lt;IEvaluationContext&gt; (F546-F548) for complex conditions:

```csharp
// Example usage in ConditionEvaluator (F550)
public bool Evaluate(DialogueCondition condition, IEvaluationContext context)
{
    var spec = _specificationFactory.Create(condition);
    return spec.IsSatisfiedBy(context);
}
```

This design allows:
- Composable conditions via And/Or/Not (Specification Pattern)
- Reusable condition logic across dialogue system
- Testable condition specifications in isolation

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- [resolved-applied] Phase1 iter5: Critical architectural issue - DialogueCondition type does not exist and IEvaluationContext needs extension. Circular dependency with T6/T7 prevents implementation.
- [resolved-applied] Phase1 iter5: DialogueCondition type does not exist - Grep found 0 matches in Era.Core. Interface cannot compile without this type.
- [resolved-applied] Phase1 iter5: IEvaluationContext extension tracked to T7 but existing Era.Core/Functions/IEvaluationContext.cs lacks TALENT/ABL/EXP access. New interface may be needed.
- [resolved-applied] Phase1 iter6: DialogueCondition type does not exist in Era.Core. Interface cannot compile without this type definition.
- [resolved-applied] Phase1 iter7: Tasks T6 and T7 are marked AC# '-' but are BLOCKING prerequisites for implementation. Circular dependency prevents implementation.
- [resolved-applied] Phase1 iter7: AC#8 Expected value 'TODO\\|FIXME\\|HACK' with not_contains matcher may not work correctly due to backslash escaping.
- [resolved-applied] Phase1 iter8: AC#3 expects Evaluate method with DialogueCondition parameter but DialogueCondition type does not exist in Era.Core.
- [resolved-applied] Phase1 iter8: Implementation Contract shows 'using Era.Core.Dialogue.Conditions' for DialogueCondition but this namespace does not exist.
- [resolved-applied] Phase1 iter9: Critical architectural issues preventing implementation: (1) DialogueCondition type does not exist, (2) Era.Core.Dialogue namespace does not exist, (3) T6/T7 circular dependency, (4) Missing predecessor dependencies, (5) AC#4/AC#5 cannot distinguish XML documentation levels.
- [resolved-applied] Phase2 iter9: Maintainability review confirms all critical architectural issues from Phase 1. Implementation impossible without resolving type dependencies.
- [resolved-applied] Phase3 iter9: AC validation confirms same critical issues: DialogueCondition type missing, Era.Core.Dialogue namespace missing, T6/T7 circular dependency, AC#4/AC#5 pattern conflicts.
- [resolved-applied] Phase1 iter10: Final iteration reached MAX_ITERATIONS (10/10). All critical architectural issues remain unresolved: DialogueCondition type missing, Era.Core.Dialogue namespace missing, T6/T7 circular dependency preventing implementation.
- [resolved-applied] Phase4 iter10: Feasibility check confirms implementation is BLOCKED by critical architectural issues. DialogueCondition type does not exist, T6/T7 circular dependency prevents implementation.

**Resolution Summary (Post-Loop)**:
- Added DialogueCondition record type definition to Implementation Contract
- Updated AC table to include DialogueCondition file creation (AC#1, AC#8)
- Restructured Tasks to include DialogueCondition creation (T1)
- Removed circular dependency: T6/T7 no longer reference internal tasks
- Updated namespace structure to support Era.Core.Dialogue.Conditions

---

## 引継ぎ先指定 (Mandatory Handoffs)
<!-- 課題あり → 全行に追跡先必須。空欄・TBD・未定 禁止。FL が FAIL する -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| IEvaluationContext extension | Interface parameter requires TALENT/ABL/EXP access (not in current Era.Core.Functions.IEvaluationContext) | Feature | F550 | - |
| ConditionEvaluator implementation | Interface extraction does not include implementation | Feature | F550 | - |
| Specification Pattern integration | Complex condition logic requires Specification Pattern | Feature | F546-F548 | - |
| DI registration | Registering IConditionEvaluator with container is out of scope | Feature | F553 | - |

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-18 | create | feature-creator | feature creation | PENDING |
| 2026-01-26 06:44 | START | implementer | Phase 3 TDD Test Creation | - |
| 2026-01-26 06:44 | END | implementer | Phase 3 TDD Test Creation | SUCCESS |
| 2026-01-26 06:47 | START | implementer | Phase 4 Implementation T1-T7 | - |
| 2026-01-26 06:47 | END | implementer | Phase 4 Implementation T1-T7 | SUCCESS |
| 2026-01-26 | DEVIATION | feature-reviewer | Phase 7 Quality Review | NEEDS_REVISION: TBD in Handoffs |
| 2026-01-26 | DEVIATION | feature-reviewer | Phase 7 Doc-Check | NEEDS_REVISION: link description mismatch |

---

## Dependencies

| Type | ID | Description | Status |
|------|----|-------------|:------:|
| Predecessor | F625 | Post-Phase Review Phase 17 (Data Migration) | [DONE] |
| Successor | F550 | ConditionEvaluator Implementation | - |
| Related | F546-F548 | Specification Pattern Infrastructure | - |

---

## Links

- [index-features.md](index-features.md)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md#phase-18-kojoengine-srp分割) - Phase 18 specification (lines 3890-4064)
- [feature-625.md](feature-625.md) - Post-Phase Review Phase 17 (Data Migration)
- [feature-541.md](feature-541.md) - Phase 18 Planning
- [feature-546.md](feature-546.md) - Basic Condition Specifications
- [feature-547.md](feature-547.md) - Complex Condition Specifications
- [feature-548.md](feature-548.md) - Composite Condition Specifications
- [feature-549.md](feature-549.md) - YamlDialogueLoader Implementation
- [feature-550.md](feature-550.md) - ConditionEvaluator Implementation
- [feature-553.md](feature-553.md) - KojoEngine Facade Refactoring
- [feature-template.md](reference/feature-template.md)
