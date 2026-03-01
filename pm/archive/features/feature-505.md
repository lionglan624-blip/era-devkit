# Feature 505: OperatorRegistry.EvaluateBinary() OCP Violation Investigation

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

## Type: research

## Created: 2026-01-14

---

## Summary

**Investigation-only research feature** (analysis report, not sub-feature creation): OperatorRegistry.EvaluateBinary() OCP Violation Investigation

Investigate TD-P14-001 (OperatorRegistry.EvaluateBinary() OCP violation) and provide decision options for user approval. The acceptance decision was documented by F493 but user was not explicitly consulted.

This is an investigation-only research feature that produces an analysis report in the Execution Log, not sub-features.

**Expected Output**:
- Analysis report of current state (cost/benefit/impact)
- Analysis report enabling informed decision on whether F501 should include TD-P14-001 in its refactoring scope, or whether this debt should be accepted with explicit user confirmation
- Recommendation with justification

**Deliverables in Execution Log**:
1. Current state analysis (operator count, change frequency)
2. Refactoring cost analysis (code changes, test impact)
3. Benefit analysis (extensibility, maintainability)
4. Recommendation with rationale

---

## Background

### Philosophy (Mid-term Vision)

**Phase 15: Architecture Review** - Validate Phase 1-14 implementations against Phase 4 design principles before large-scale parallel implementation phases (Phase 19-21). Ensures all technical debt is either resolved or consciously accepted by user with documented rationale.

### Problem (Current Issue)

TD-P14-001 identifies OperatorRegistry.EvaluateBinary() in Era.Core/Expressions/Operators.cs as having 29 if branches (OCP violation):
- Adding new operators requires modifying the method
- F493 architecture review marked this as "accepted" technical debt
- Acceptance decision was made by the review agent and documented, but user was not explicitly consulted to confirm acceptance of this technical debt before closing F493

### Goal (What to Achieve)

Provide user with actionable decision options:
1. **Option A: Proceed with F501 refactoring** - Strategy pattern with Dictionary<string, IOperator>
2. **Option B: Accept as intentional technical debt** - Document rationale with user approval
3. **Option C: Partial refactoring** - Strategy pattern for frequently-changed operators only, leave stable operators in existing method

**Output**: Analysis report enabling informed decision on whether to fix or accept OCP violation.

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Current state analysis documented | file | Grep | contains | "Current State Analysis:" | [x] |
| 2 | Refactoring cost analysis documented | file | Grep | contains | "Refactoring Cost Analysis:" | [x] |
| 3 | Benefit analysis documented | file | Grep | contains | "Benefit Analysis:" | [x] |
| 4 | Decision options presented | file | Grep | contains | "Decision Options:" | [x] |
| 5 | Recommendation documented | file | Grep | contains | "Recommendation:" | [x] |
| 6 | Recommendation actionable | file | Grep | matches | "Recommendation:.*Option [ABC]" | [x] |

### AC Details

**NOTE**: AC verification uses Type: file + Method: Grep which is not yet supported by ac-static-verifier. Manual verification is acceptable and no blocking tool dependency exists. F504 will provide tool support when implemented.

**Verification Target**: All ACs verify content in the Execution Log section of this same file (self-documenting pattern for research features).

**AC#1**: Current state analysis
- Test: Grep pattern="Current State Analysis:" in feature-505.md Execution Log
- Expected: Analysis of operator count, branch complexity, change frequency

**AC#2**: Refactoring cost analysis
- Test: Grep pattern="Refactoring Cost Analysis:" in feature-505.md Execution Log
- Expected: Estimates code changes, test impact, effort (hours/days)

**AC#3**: Benefit analysis
- Test: Grep pattern="Benefit Analysis:" in feature-505.md Execution Log
- Expected: Evaluates extensibility gain, maintainability improvement, risk reduction

**AC#4**: Decision options presented
- Test: Grep pattern="Decision Options:" in feature-505.md Execution Log
- Expected: Lists Option A (fix), B (accept), C (alternative) with pros/cons

**AC#5**: Recommendation documented
- Test: Grep pattern="Recommendation:" in feature-505.md Execution Log
- Expected: States recommended option with justification

**AC#6**: Recommendation actionable
- Test: Grep pattern="Recommendation:.*Option [ABC]" in feature-505.md Execution Log
- Expected: Recommendation explicitly selects Option A, B, or C

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Analyze current state of OperatorRegistry.EvaluateBinary() | [x] |
| 2 | 2 | Estimate refactoring cost (code, tests, effort) | [x] |
| 3 | 3 | Evaluate benefits (extensibility, maintainability, risk) | [x] |
| 4 | 4 | Present decision options with pros/cons | [x] |
| 5 | 5,6 | Document recommendation with justification | [x] |

<!-- AC:Task 1:1 Rule: 6 ACs = 5 Tasks (AC#5,6 combined in Task#5 - both verify recommendation) -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Investigation Method

1. **Read source file**:
   - `Era.Core/Expressions/Operators.cs` - Count branches in EvaluateBinary()

2. **Assess current state**:
   - Total operator count
   - Branch structure complexity
   - Change frequency: Run `git log -p --since="2025-01-01" Era.Core/Expressions/Operators.cs` to count operator additions/modifications (analyzes commits from 2025-01-01 to present)

3. **Estimate refactoring cost**:
   - Lines of code to change
   - New files to create (IOperator interface, operator implementations)
   - Test modifications required
   - Estimated effort (hours/days)

4. **Evaluate benefits**:
   - Extensibility: Adding new operators without modifying existing code
   - Maintainability: Smaller, focused operator classes vs. large method
   - Risk: Test coverage impact, regression risk

5. **Compare with F501 plan**:
   - Read architecture-review-15.md Section "負債の意図的受け入れ: TD-P14-001"
   - Verify F501 Strategy pattern approach feasibility

### Analysis Report Format

Add to feature-505.md Execution Log:

```markdown
## Current State Analysis:

- **Operator Count**: {count} binary operators
- **Branch Count**: {count} if/else branches in EvaluateBinary()
- **Method Length**: {lines} lines
- **Change Frequency**: {count} changes in last {period} (git log analysis)

## Refactoring Cost Analysis:

- **Code Changes**:
  - Existing interfaces: IBinaryOperator, IUnaryOperator (defined but unused per architecture-review-15.md)
  - New files: {operator_count} operator implementation classes (interface definitions already exist)
  - Modified files: OperatorRegistry.cs, ServiceCollectionExtensions.cs
  - Total LOC estimate: +{new_lines}, -{removed_lines}
- **Test Impact**:
  - New tests: {count} operator unit tests
  - Modified tests: {count} integration tests
- **Effort Estimate**: {hours/days}

## Benefit Analysis:

- **Extensibility**: New operators can be added as separate classes without modifying OperatorRegistry
- **Maintainability**: {small_classes_count} focused operator classes vs. 1 large method
- **Risk Reduction**: {percentage}% test coverage per operator vs. combined method coverage

## Decision Options:

**Option A: Proceed with F501 refactoring**
- Pros: {list}
- Cons: {list}
- Estimated effort: {hours/days}

**Option B: Accept as technical debt**
- Pros: {list}
- Cons: {list}
- Conditions for acceptance: {criteria}

**Option C: Alternative approach** (if discovered)
- Description: {approach}
- Pros: {list}
- Cons: {list}

## Recommendation:

{Selected option} because {justification based on analysis}.
```

### Decision Criteria

**Change Frequency Classification**:
- Run `git log -p --since="2025-01-01" Era.Core/Expressions/Operators.cs` to count operator-related commits (analyzes commits from 2025-01-01 to present)
- If < 5 operator-related commits → classify as "stable"
- If > 10 operator-related commits → classify as "frequently changing"

**Recommend ACCEPT if**:
- Operator set is stable (< 5 changes per year per git log)
- Refactoring cost > benefit (effort > 1 day for minimal gain)
- Tests provide sufficient coverage (no risk)

**Recommend FIX if**:
- Operator set changes frequently (> 10 changes per year per git log)
- Extensibility is critical for Phase 16+ (new operator types planned)
- Maintainability issues observed (method too complex)

**Recommend ALTERNATIVE if**:
- Simpler refactoring approach discovered (e.g., partial Strategy pattern)
- Hybrid approach provides better cost/benefit ratio

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F493 | Code Review Phase 1-4 identified TD-P14-001 |
| Related | F501 | Architecture Refactoring (becomes Successor if Option A recommended) |
| Related | F504 | AC verification tool enhancement (non-blocking, manual verification acceptable) |
| Reference | architecture-review-15.md | TD-P14-001 documentation |

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-14 FL iter0**: [resolved] Phase0-RefCheck - Background.Problem: Claim "decision was made WITHOUT user confirmation" is misleading - architecture-review-15.md has documented Acceptance Rationale, the issue is whether user was consulted before F493 made the decision (Fixed in iter1)
- **2026-01-14 FL iter3**: [skipped] Phase2-Validate - Summary line 27: Reviewer suggests reframing investigation from "user not consulted" to "validate acceptance rationale soundness" - User chose to keep current framing
- **2026-01-14 FL iter3**: [skipped] Phase2-Validate - Goal lines 59-64: Reviewer suggests revising Goal to "should TD-P14-001 remain in F501 scope?" since F501 already exists - User chose to keep current framing

---

## Links

- [feature-486.md](feature-486.md) - Phase 15 Planning (parent feature)
- [feature-493.md](feature-493.md) - Code Review Phase 1-4 (identified TD-P14-001)
- [feature-501.md](feature-501.md) - Architecture Refactoring (scheduled fix)
- [architecture-review-15.md](designs/architecture-review-15.md) - TD-P14-001 documentation
- [Era.Core/Expressions/Operators.cs](../../Era.Core/Expressions/Operators.cs) - Source file

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-14 | create | feature-builder | Created from user request | PROPOSED |
| 2026-01-15 | START | opus | Investigation and analysis | - |
| 2026-01-15 | END | opus | Investigation Report complete | SUCCESS |
| 2026-01-15 | END | opus | AC verification (manual Grep) | PASS:6/6 |
| 2026-01-15 | END | feature-reviewer | maintainability + doc-check | READY |
| 2026-01-15 | DECISION | user | Option A selected (Zero Debt Upfront) | - |

---

## Investigation Report

### Current State Analysis:

**Source File**: `Era.Core/Expressions/Operators.cs` (311 lines total)

- **Operator Count**: 27 binary operators (counting type variants):
  - Arithmetic (Int64): +, -, *, /, % (5 operators)
  - Comparison (Int64): ==, !=, <, >, <=, >= (6 operators)
  - Comparison (String): ==, !=, <, >, <=, >= (6 operators)
  - Logical (Int64): &&, ||, ^^, !&, !| (5 operators)
  - Bitwise (Int64): &, |, ^, <<, >> (5 operators)
  - String: + (concat), * (repeat with Int64) (2 operators, but only 1 in binary dispatch)
- **Branch Count**: 35 if statements in EvaluateBinary() (lines 17-208)
- **Method Length**: 192 lines (EvaluateBinary method body)
- **Change Frequency**: **1 commit** since 2025-01-01 (classified as **stable**)
  - Single commit: `76f0b7a feat(Era.Core): Add Phase 8 operator system (F417)` (2026-01-09)
  - No subsequent modifications to operators

**Interface Availability**:
- `IBinaryOperator` interface exists (IOperatorRegistry.cs:76-85) but is **not used**
- `IUnaryOperator` interface exists (IOperatorRegistry.cs:90-98) but is **not used**
- Both interfaces have proper Result<object> Evaluate() signatures

### Refactoring Cost Analysis:

**Code Changes Required** (if Option A chosen):
- **Existing interfaces**: IBinaryOperator, IUnaryOperator already defined - no new interface definitions needed
- **New files**: ~27 operator implementation classes
  - Example: `AddOperator.cs`, `SubtractOperator.cs`, etc.
  - Each ~15-30 lines (simple operators) to ~40 lines (complex like division with error handling)
- **Modified files**:
  - `OperatorRegistry.cs`: Replace if/else chain with Dictionary<string, IBinaryOperator> lookup
  - `ServiceCollectionExtensions.cs`: Add 27 DI registrations
- **Total LOC estimate**: +800-1000 lines (new operators), -180 lines (removed from OperatorRegistry)

**Test Impact**:
- **Existing tests**: 60 tests for operators (F417) - would need refactoring to test individual classes
- **New tests**: 27 unit test classes (1 per operator)
- **Test migration effort**: Move existing assertions to new test classes

**Effort Estimate**:
- Code implementation: 4-6 hours
- Test migration: 2-3 hours
- **Total: ~1 day (6-9 hours)**

### Benefit Analysis:

**Extensibility**:
- **Current**: Adding new operator requires modifying EvaluateBinary() (OCP violation)
- **After refactoring**: New operators added as separate classes + DI registration (OCP compliant)
- **Value**: Low - operator set is stable (1 change in 1 year), no new operators planned in Phase 16+

**Maintainability**:
- **Current**: 1 large method (192 lines) with 35 branches
- **After refactoring**: 27 focused operator classes (~25 lines average)
- **Value**: Moderate - code is simpler to understand per-operator, but overall complexity increases (27 files vs 1 file)

**Risk Reduction**:
- **Current**: All operators in single method - any change risks affecting others
- **After refactoring**: Isolated classes - change to one operator cannot affect others
- **Value**: Low - operators are pure functions with no shared state, risk is minimal

**Test Coverage Impact**:
- **Current**: Combined coverage across all operator branches
- **After refactoring**: Per-operator coverage, clearer failure isolation
- **Value**: Moderate improvement in test clarity

### Decision Options:

**Option A: Proceed with F501 refactoring**
- **Pros**:
  - Full OCP compliance
  - Individual operator unit testing
  - Clear separation of concerns
  - Interfaces already defined (IBinaryOperator, IUnaryOperator)
- **Cons**:
  - ~1 day effort for minimal practical benefit
  - Increases file count by 27
  - Operator set is stable (no extensions planned)
  - Complexity shifts from "one large method" to "many small files"
- **Estimated effort**: 6-9 hours

**Option B: Accept as technical debt**
- **Pros**:
  - Zero implementation cost
  - Operator set is stable (1 change/year)
  - Current code works correctly
  - No runtime behavior change
  - Existing 60 tests provide coverage
- **Cons**:
  - OCP violation remains documented
  - Future operator additions require method modification
  - Large method reduces readability
- **Conditions for acceptance**:
  - Operator set remains stable
  - No new operator types planned for Phase 16+
  - Existing test coverage maintained

**Option C: Partial refactoring (Dictionary lookup without separate classes)**
- **Description**: Refactor to Dictionary<(string, Type, Type), Func<object, object, Result<object>>> lookup table instead of if/else chain. Keeps all operators in single file but removes branching.
- **Pros**:
  - Eliminates if/else chain (addresses OCP technically)
  - Single file maintained (no explosion of operator classes)
  - Easier to add new operators (just add dictionary entry)
  - Moderate effort (~2-3 hours)
- **Cons**:
  - Not true OCP (lambda expressions in dictionary, not separate classes)
  - Less testable than Option A (cannot test operators in isolation)
  - Dictionary initialization becomes complex

### Recommendation:

**Recommendation: Option A - Proceed with F501 refactoring**

**Initial Analysis** (Option B recommended based on cost/benefit):
- Change frequency is low (1 commit/year)
- Refactoring cost (~1 day) > practical benefit for stable code
- 60 tests provide coverage

**User Challenge**: Project philosophy prioritizes Zero Debt Upfront over short-term cost optimization.

**Re-evaluation applying project philosophy**:

1. **Zero Debt Upfront principle**: "Pay large costs now to eliminate future technical debt. No YAGNI."
   - 1 day cost is acceptable within Phase 1-35 scope
   - "Stable code" is not justification for OCP violation

2. **Interface completion**: IBinaryOperator/IUnaryOperator are already defined but unused
   - Using these interfaces completes the Phase 8 design
   - Leaving them unused = incomplete implementation

3. **Consistency**: Phase 4 established OCP as design principle
   - Phase 8 should follow Phase 4 principles, not violate them

4. **Future maintainers**: No need to explain "why OCP is violated here"
   - Clean codebase with consistent patterns

5. **Testability improvement**: 27 small operator classes > 1 large 192-line method

**F501 Impact**: TD-P14-001 refactoring IS included in F501 scope. F501 should implement Strategy pattern with Dictionary<string, IBinaryOperator>.

**User Decision**: Option A selected (2026-01-15)
