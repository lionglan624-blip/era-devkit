# Feature 501: Architecture Refactoring

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

## Created: 2026-01-14

---

## Summary

**Conditional feature**: Implement architecture refactorings only if Phase 4 deviations found in F493-F500 reviews.

**Refactoring Scope** (if needed):
- Phase 4 principle violations (SRP/OCP/DIP)
- Folder structure changes (per F496 recommendations)
- Naming convention fixes (per F497 recommendations)
- Testability improvements (per F498 recommendations)
- **TD-P14-001**: OperatorRegistry.EvaluateBinary() OCP violation (per F505 decision)
- JuelProcessor System.Random → IRandomProvider migration (per F499 design, addressing F498 testability finding)

**Output**: Refactored code with all tests PASS, or SKIPPED status if no deviations found.

**CRITICAL**: This feature is created for completeness but may be SKIPPED if F493-F500 reviews find no actionable deviations. Skip decision made after F500 completion.

---

## Background

### Philosophy (Mid-term Vision)

**Phase 15: Architecture Review** - Validate Phase 1-14 implementations against Phase 4 design principles (SRP, DIP, Strongly Typed IDs, Result type, DI registration) before large-scale parallel implementation phases (Phase 19-21). Ensures architectural consistency and identifies technical debt requiring resolution.

### Problem (Current Issue)

Architecture review (F493-F500) may identify deviations requiring refactoring:
- Phase 4 principle violations
- Structural inconsistencies
- Naming convention issues
- Testability problems

However, refactoring scope unknown until reviews complete.

### Goal (What to Achieve)

**IF deviations found**:
1. **Implement refactorings** per F493-F500 recommendations
2. **Verify all tests PASS** after refactoring
3. **Document changes** in architecture-review-15.md
4. **Update design docs** if patterns change

**IF no deviations found**:
1. **Mark feature SKIPPED** with rationale
2. **Document skip decision** in Execution Log

---

## Acceptance Criteria

**Note**: ACs conditional on skip decision. Mark [S] (SKIPPED) if no refactoring needed.

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Skip decision documented | file | Grep(Game/agents/designs/architecture-review-15.md) | contains | "Refactoring Decision:" | [x] |
| 2a | Operators build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 2b | Random build succeeds | build | dotnet build Era.Core | succeeds | - | [x] |
| 3a | Operators tests PASS | test | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 3b | Random tests PASS | test | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 4 | Changes documented | file | Grep(Game/agents/designs/architecture-review-15.md) | contains | "Refactorings Applied:" | [x] |
| 5 | Phase 4 compliance verified | file | Grep(Game/agents/designs/architecture-review-15.md) | contains | "Post-Refactoring Compliance:" | [x] |
| 6 | 負債ゼロ Random層 | file | Grep(Era.Core/Random/) | not_contains | "TODO\\|FIXME\\|HACK" | [x] |
| 7 | 負債ゼロ Operators層 | file | Grep(Era.Core/Expressions/Operators/) | not_contains | "TODO\\|FIXME\\|HACK" | [x] |

### AC Details

**AC#1**: Skip decision documented
- Test: Grep pattern="Refactoring Decision:" in architecture-review-15.md
- Expected: Section states SKIP (no deviations) or REFACTOR (deviations found)

**AC#2a**: Operators build succeeds (TD-P14-001 refactoring)
- Test: `dotnet build Era.Core` after Task#3 (OperatorRegistry Strategy pattern)
- Expected: Exit code 0
- **SKIP**: If skip decision in AC#1
- **Note**: Era.Core/Expressions/Operators/ directory created by Task#3

**AC#2b**: Random build succeeds (IRandomProvider migration)
- Test: `dotnet build Era.Core` after Task#4 (JuelProcessor migration)
- Expected: Exit code 0
- **SKIP**: If skip decision in AC#1
- **Note**: Era.Core/Random/ directory created by Task#4

**AC#3a**: Operators tests PASS (TD-P14-001 refactoring)
- Test: `dotnet test Era.Core.Tests` after Task#3
- Expected: 100% PASS
- **SKIP**: If skip decision in AC#1

**AC#3b**: Random tests PASS (IRandomProvider migration)
- Test: `dotnet test Era.Core.Tests` after Task#4
- Expected: 100% PASS
- **SKIP**: If skip decision in AC#1

**AC#4**: Changes documented (if refactored)
- Test: Grep pattern="Refactorings Applied:" in architecture-review-15.md
- Expected: Lists all refactorings with locations
- **SKIP**: If no refactoring needed

**AC#5**: Compliance verified
- Test: Grep pattern="Post-Refactoring Compliance:" in architecture-review-15.md
- Expected: Confirms Phase 4 compliance after refactoring (or original compliance if skipped)

**AC#6**: Zero technical debt in Random layer (IRandomProvider implementations)
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Random/" (if refactored)
- Expected: 0 technical debt in provider implementation files
- **SKIP**: If no refactoring needed

**AC#7**: Zero technical debt in Operators layer (IBinaryOperator implementations)
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Expressions/Operators/" (if refactored)
- Expected: 0 technical debt in operator implementation files
- **Note**: Pre-existing debt in other Era.Core files (e.g., GameInitialization.cs) is tracked for Phase 20-22 and not in F501 scope
- **SKIP**: If no refactoring needed

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Review F493-F500 findings and decide skip/refactor | [x] |
| 2 | 2a,3a,7 | TD-P14-001: Refactor OperatorRegistry.EvaluateBinary() to Strategy pattern with Dictionary<string, IBinaryOperator> | [x] |
| 3 | 2b,3b,6 | Migrate JuelProcessor System.Random to IRandomProvider (per F499 design, addressing F498 testability finding) | [x] |
| 4 | 4,5 | Document refactorings and verify compliance (負債解消) | [x] |
| 5 | 6,7 | Verify zero new technical debt in new code layers | [x] |

<!-- AC:Task 1:1 Rule: 9 ACs = 5 Tasks (AC#2,3 split to 2a,2b,3a,3b for Operators/Random traceability) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Skip Decision Criteria

**SKIP if**:
- F493-F500 reviews find 0 major deviations
- All minor deviations accepted as intentional technical debt
- Refactoring cost > benefit

**REFACTOR if**:
- Major Phase 4 principle violations found
- Structural inconsistencies affecting maintainability
- Testability issues blocking Phase 16+ implementation

### Refactoring Scope Limits

Per architecture.md Phase 15:

| Permitted | Prohibited |
|-----------|------------|
| ✅ Folder structure changes | ❌ Full rewrites of working code |
| ✅ Interface additions | ❌ New feature additions |
| ✅ Naming renames | ❌ Speculative abstractions |
| ✅ Test additions | ❌ Phase 16+ scope creep |

### Refactoring Documentation

Add to `Game/agents/designs/architecture-review-15.md`:

```markdown
## Refactoring Decision:

**Status**: [SKIP | REFACTOR]

**Rationale**: (explain decision)

## Refactorings Applied:
(If REFACTOR)

| ID | Location | Change | Rationale | Test Impact |
|----|----------|--------|-----------|-------------|
| R15-001 | Era.Core/Foo.cs | Rename FooImpl → Foo | Naming consistency | None |
| R15-002 | Era.Core/Com/ | Move to feature folders | Maintainability | None |
| ... | ... | ... | ... | ... |

## Post-Refactoring Compliance:

**SRP**: [✓ Compliant]
**OCP**: [✓ Compliant]
**DIP**: [✓ Compliant]
**Result Type**: [✓ Consistently used]
**Strongly Typed IDs**: [✓ Consistently used]

**Tests**: [✓ All PASS (N/N)]

## 負債の意図的受け入れ:
(Document any refactoring debt accepted with justification)
```

### TD-P14-001: OperatorRegistry Strategy Pattern Refactoring

Per F505 Investigation Report (Option A confirmed):

1. **Use existing IBinaryOperator interface**:
   - Location: `Era.Core/Expressions/IOperatorRegistry.cs` (lines 76-85)
   - Signature: `Result<object> Evaluate(object left, object right)` with `Result<T>` wrapper
   - Inherits from `IOperator` which provides `Symbol` property
   - **Do NOT create new interface** - use existing one

2. **Implement operator classes** (27 operators per F505):
   - **Location**: Create `Era.Core/Expressions/Operators/` directory for all operator implementations
   - Arithmetic: Add, Subtract, Multiply, Divide, Modulo (→ AddOperator.cs, SubtractOperator.cs, etc.)
   - Comparison: Equal, NotEqual, LessThan, GreaterThan, LessOrEqual, GreaterOrEqual
   - Logical: And, Or
   - String: Concat, etc.
   - All implementations must return `Result<object>` per existing interface

3. **Register operators in DI**:
   ```csharp
   services.AddSingleton<Dictionary<string, IBinaryOperator>>(sp =>
       new Dictionary<string, IBinaryOperator>
       {
           ["+"] = new AddOperator(),
           ["-"] = new SubtractOperator(),
           // ...
       });
   ```

4. **Refactor EvaluateBinary()**: Replace if/else chain with Dictionary lookup

### IRandomProvider Migration

Per test-strategy.md Section 7 and F499:

**Migration Strategy**: Create NEW IRandomProvider interface in Era.Core/Random/ namespace.
- F418's IRandom (Era.Core/Functions/IRandom.cs) remains unchanged; it is NOT deprecated
- IRandomProvider does NOT extend IRandom; they are parallel interfaces with similar signatures
- IRandomProvider adds NextFromArray<T>() and Seed property beyond IRandom's Next() methods
- Create NEW Era.Core/Random/SystemRandomProvider.cs implementing IRandomProvider (SEPARATE from existing Era.Core/Functions/SystemRandom.cs which implements IRandom)
- Future unification of IRandom/IRandomProvider is explicitly out of F501 scope

**F501 Scope**: JuelProcessor.cs migration only.
- SystemRandomProvider.cs: NEW file in Era.Core/Random/ implementing IRandomProvider (production implementation for F501)
- Existing SystemRandom.cs: Remains in Era.Core/Functions/ implementing IRandom (unchanged, for F418 compatibility)
- GameInitialization.cs: Tracked as separate technical debt for Phase 20-22 (not F501 scope)

1. **Create IRandomProvider interface** (per test-strategy.md Section 7):
   ```csharp
   // Era.Core/Random/IRandomProvider.cs
   /// Uses long types to match ERA's 64-bit integer system.
   public interface IRandomProvider
   {
       long Next(long max);
       long Next(long min, long max);
       T NextFromArray<T>(T[] array);
       long Seed { get; }
   }
   ```

2. **Implement providers**:
   - `SystemRandomProvider`: Uses System.Random for production
     - `Seed` property returns `RandomConstants.NoSeed` (-1) indicating non-deterministic behavior
   - `SeededRandomProvider`: Uses seed for deterministic testing
     - `Seed` property returns the configured seed value for reproducibility verification
   - **Seed constant**: Define `public static class RandomConstants { public const long NoSeed = -1; }` in Era.Core/Random/ (C# interfaces cannot contain const fields)

3. **Refactor JuelProcessor.cs**:
   - Replace direct `new Random()` instantiation (line 19) with constructor-injected `IRandomProvider`
   - Change `private readonly Random _random` (line 14) to `private readonly IRandomProvider _random`
   - Note: JuelProcessor currently uses `System.Random` directly, NOT the F418 `IRandom` interface

4. **Update DI registration**:
   - Production: `services.AddSingleton<IRandomProvider, SystemRandomProvider>()`
   - Test: Inject `SeededRandomProvider` with configurable seed

### Verification

After refactoring (if applied):
```bash
dotnet build Era.Core
dotnet test Era.Core.Tests
# Expected: 100% PASS, no new warnings
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F499 | IRandomProvider interface design (input for migration) |
| Predecessor | F500 | Test Strategy Design must complete first (all reviews done) |
| Reference | F505 | TD-P14-001 investigation (confirmed Option A: refactor) - already completed |
| Successor | F502 | Post-Phase Review Phase 15 (verifies refactoring completion) |
| Related | F493-F498 | Architecture reviews provide refactoring scope |

---

## Links

- [feature-486.md](feature-486.md) - Phase 15 Planning (parent feature)
- [feature-499.md](feature-499.md) - Test Strategy Design: IRandomProvider (interface design, migration scope)
- [feature-500.md](feature-500.md) - Test Strategy Design: E2E and /do Integration (test execution workflow)
- [feature-502.md](feature-502.md) - Post-Phase Review Phase 15 (successor feature)
- [test-strategy.md Section 7](designs/test-strategy.md#7-irandomprovider) - IRandomProvider migration details (JuelProcessor refactoring steps)
- [feature-493.md](feature-493.md) - Code Review Phase 1-4
- [feature-494.md](feature-494.md) - Code Review Phase 5-8
- [feature-495.md](feature-495.md) - Code Review Phase 9-12
- [feature-496.md](feature-496.md) - Folder Structure Validation
- [feature-497.md](feature-497.md) - Naming Convention Audit
- [feature-498.md](feature-498.md) - Testability Assessment
- [testability-assessment-15.md](designs/testability-assessment-15.md) - F498 output: Testability improvements recommendations
- [feature-505.md](feature-505.md) - TD-P14-001 OCP violation investigation (Option A confirmed)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 15 refactoring scope limits

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-15 FL iter1**: [resolved] Phase2-Validate - AC#2,3 conditionality: Removed "(if needed)" notation from AC#2,3,4 in iter5. Skip conditions remain in AC Details.
- **2026-01-15 FL iter1**: [pending] Phase2-Validate - Summary Output: Output statement exists but if migration creates new files (IRandomProvider.cs), file existence ACs may be needed.
- **2026-01-15 FL iter1**: [resolved] Phase2-Validate - Dependencies F499: F499 is covered under 'Related F493-F500' range but not explicitly listed as Predecessor. → Fixed in iter2
- **2026-01-15 FL iter2**: [resolved] Phase2-Validate - AC:Task 1:1: Task 2/2a/2b numbering → renumbered to sequential (1-6) in iter3.
- **2026-01-15 FL iter2**: [pending] Phase2-Validate - File existence ACs: IBinaryOperator already exists. IRandomProvider.cs is new; AC#2 (build) implicitly verifies. Explicit file AC optional.
- **2026-01-15 FL iter3**: [pending] Phase2-Validate - File existence ACs (same as iter2): Whether explicit file ACs are mandatory vs optional is debatable. IBinaryOperator exists; IRandomProvider.cs implicit via build.
- **2026-01-15 FL iter4**: [resolved] Phase2-Validate - AC#6 scope: Scoped to Era.Core/Random/ and Era.Core/Expressions/Operators/ in iter5.
- **2026-01-15 FL iter4**: [pending] Phase2-Validate - Task#3/4 independence: Both map to AC#2,3 but are independent efforts. Waiver comment exists; splitting ACs would improve traceability.
- **2026-01-15 FL iter4**: [pending] Phase2-Validate - IUnaryOperator scope: F505 analyzed EvaluateBinary only. IUnaryOperator refactoring not included; defer to follow-up if needed.
- **2026-01-15 FL iter5**: [pending] Phase2-Validate - Task#4 scope clarity: Task explicitly names JuelProcessor; GameInitialization.cs new Random() not in scope per F498 findings (JuelProcessor only High severity).
- **2026-01-15 FL iter5**: [pending] Phase2-Validate - IUnaryOperator (same as iter4): EvaluateUnary() only 66 lines/8 branches vs 192/35 for EvaluateBinary(). Include in scope for consistency or track as separate follow-up.
- **2026-01-15 FL iter7**: [pending] Phase2-Validate - Pattern escaping: Markdown double-backslash vs ripgrep regex. Current 'TODO|FIXME|HACK' with single backslash is correct for ripgrep.
- **2026-01-15 FL iter7**: [pending] Phase2-Validate - pending Review Notes: 5+ items marked [pending] is normal for FL iterations; resolved before REVIEWED status.
- **2026-01-15 FL iter7**: [pending] Phase2-Validate - F505 Reference vs Predecessor: Semantically F505 provides input decision; Reference is functionally acceptable since F505 is DONE.
- **2026-01-15 FL iter9**: [resolved] Phase2-Validate - IRandom/IRandomProvider migration strategy: Added explicit migration strategy in Implementation Contract (new interface, IRandom unchanged, future unification out of scope).
- **2026-01-15 FL iter9**: [pending] Phase2-Validate - AC:Task 1:1 violation: Task#3,4 batch AC#2,3 with waiver. Consider adding dedicated ACs for 1:1 or accept waiver as documented.
- **2026-01-15 FL iter9**: [pending] Phase2-Validate - JuelProcessor clarification: Statement is factually correct; suggested stylistic improvement optional.
- **2026-01-15 FL iter10**: [pending] Phase2-Validate - AC Method column format: Grep(directory/) format is valid per SKILL.md but searching entire directory vs explicit file list is stylistic choice. Keep as-is or list explicit files.
- **2026-01-15 FL iter10**: [pending] Phase2-Validate - AC:Task 1:1 mapping: Task#3,4 map to same AC#2,3 with waiver documented. Splitting ACs optional but improves traceability.
- **2026-01-15 FL iter11**: [resolved] Phase2-Validate - TBD Prohibition: Changed 残課題 target from "F508 (TBD→create during F502)" to "F502 Task#4 (deferred tasks tracking)" per CLAUDE.md.
- **2026-01-15 FL iter11**: [resolved] Phase2-Validate - SystemRandomProvider clarification: Clarified Era.Core/Random/SystemRandomProvider.cs is NEW file separate from Era.Core/Functions/SystemRandom.cs.
- **2026-01-15 FL iter11**: [pending] Phase2-Validate - AC#6 directory note: Era.Core/Random/ doesn't exist yet; created by Task#4. Adding note is stylistic improvement.
- **2026-01-15 FL iter11**: [pending] Phase2-Validate - AC#2,3 splitting: Splitting into AC#2a/2b and AC#3a/3b would improve traceability but waiver is documented.
- **2026-01-15 FL iter12**: [resolved] Phase3-Maintainability - IRandom/IRandomProvider relationship: Clarified "IRandomProvider does NOT extend IRandom; they are parallel interfaces".
- **2026-01-15 FL iter13**: [resolved] Phase2-Validate - Operators directory location: Added "Create Era.Core/Expressions/Operators/ directory" to Implementation Contract step 2, aligning AC#7 path with implementation.
- **2026-01-15 FL iter13**: [resolved] Phase2-Validate - SystemRandomProvider.Seed return value: Already documented at line 262 (RandomConstants.NoSeed = -1). No additional fix needed.
- **2026-01-15 FL iter14**: [resolved] Phase2-Validate - Section name: "残課題 (Deferred Tasks)" → "引継ぎ先指定 (Mandatory Handoffs)" per template. Fixed in iter17.
- **2026-01-15 FL iter14**: [pending] Phase2-Validate - AC#6,7 directory notes: Adding "directory created by Task#N" is stylistic improvement, not SSOT requirement.
- **2026-01-15 FL iter14**: [pending] Phase2-Validate - Review Notes pending items: Whether all [pending] must be terminal before REVIEWED is unclear from fl.md. 15 items accumulated during 13 iterations.
- **2026-01-15 FL iter15**: [resolved] Phase3-Maintainability - test-strategy.md SSOT: Updated line 294 from "Extends existing IRandom" to "parallel interface" matching F501 design.
- **2026-01-15 FL iter16**: [resolved] Phase4-ACValidation - Grep pattern escaping: Changed AC#6,7 Expected from "TODO|FIXME|HACK" to "TODO\\|FIXME\\|HACK" for proper ripgrep literal matching.
- **2026-01-15 FL iter17**: [resolved] Phase2-Validate - Section name consistency: Renamed "残課題 (Deferred Tasks)" to "引継ぎ先指定 (Mandatory Handoffs)" per feature-template.md.
- **2026-01-15 FL iter18**: [pending] Phase2-Validate - JuelProcessor refactoring clarity: Existing _random.Next() calls auto-migrate after field type change. Explicit note is stylistic improvement.
- **2026-01-15 FL iter19**: [resolved] Phase7-FinalRefCheck - Missing link: Added F502 (successor) to Links section per Dependencies reference.
- **2026-01-15 FL iter20**: [resolved] Post-loop - AC:Task 1:1 split: Per Zero Debt Upfront principle, split AC#2,3 into AC#2a,2b,3a,3b for Operators/Random traceability. Added directory creation notes to AC Details.
- **2026-01-15 FL iter20**: [accepted] All remaining [pending] items marked accepted - stylistic improvements that don't block implementation.

---

## 引継ぎ先指定 (Mandatory Handoffs)

| Task | Reason | Target Phase | Target Feature |
|------|--------|:------------:|:--------------:|
| IUnaryOperator refactoring | F505 analyzed EvaluateBinary only (192 lines/35 branches); EvaluateUnary (66 lines/8 branches) deferred for consistency cleanup | Phase 16+ | F502 Task#4 (deferred tasks tracking) |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-14 | create | spec-writer | Created from F486 Phase 15 Planning | PROPOSED |
| 2026-01-15 | START | opus | /do 501 initialization | - |
| 2026-01-15 | END | initializer | Phase 1: Initialize Feature 501 | READY:501:engine |
| 2026-01-15 | END | explorer | Phase 2: Investigation | READY |
| 2026-01-15 | END | implementer | Phase 3: TDD tests created | RED (expected) |
| 2026-01-15 | END | implementer | Task 2: TD-P14-001 Strategy pattern (27 operators) | SUCCESS |
| 2026-01-15 | END | implementer | Task 3: IRandomProvider migration | SUCCESS |
| 2026-01-15 | DEVIATION | debugger | 20 test failures after implementation | FIXED |
| 2026-01-15 | END | implementer | Task 4: Documentation + compliance verification | SUCCESS |
| 2026-01-15 | END | ac-tester | Phase 6: All ACs verified | PASS:9/9 |
| 2026-01-15 | END | feature-reviewer | Phase 7: post + doc-check | READY |
