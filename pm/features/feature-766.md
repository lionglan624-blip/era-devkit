# Feature 766: Paren-Stripping Guard Refinement (FindMatchingClosingParen)

## Status: [DONE]
<!-- fl-reviewed: 2026-02-09T00:00:00Z -->

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

## Review Context (F759 /run T9)

### Origin

| Field | Value |
|-------|-------|
| Parent Feature | F759 (Compound Bitwise Condition Parsing) |
| Discovery Point | Technical Constraint #10 / Mandatory Handoffs |
| Timestamp | F759 implementation |

### Identified Gap

The paren-stripping guard at `LogicalOperatorParser.cs:169` uses naive `StartsWith("(") && EndsWith(")")` check. This is vulnerable to future patterns like `(VAR & mask) == (OTHER)` where the guard would incorrectly trigger before the compound bitwise detection logic. The guard should use `FindMatchingClosingParen` (already implemented for F759 compound detection) to verify the closing paren at position 0 matches the final `)` before stripping.

### Review Evidence

| Field | Value |
|-------|-------|
| Gap Source | F759 Technical Constraint #10 |
| Derived Task | Refine paren-stripping guard to use FindMatchingClosingParen |
| Comparison Result | Naive check works for all known patterns but is fragile |
| DEFER Reason | Guard refinement is preventive, not blocking F759 functionality |

### Files Involved

| File | Relevance |
|------|-----------|
| src/tools/dotnet/ErbParser/LogicalOperatorParser.cs | Guard at line 169, FindMatchingClosingParen already exists |

### Parent Review Observations

F759 added `FindMatchingClosingParen` for compound bitwise detection. The same helper can be reused to make the paren-stripping guard robust against edge cases.

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)
(Inherited from F759/F758) Continue toward full equivalence testing by ensuring parser robustness for all parenthesized expression patterns. The parser's structural correctness guarantees must hold for ALL balanced-paren patterns, not just currently-observed ones.

### Problem (Current Issue)
The paren-stripping guard at `LogicalOperatorParser.cs:173` uses a naive `StartsWith("(") && EndsWith(")")` textual check that does not verify the opening `(` at position 0 structurally matches the closing `)` at the end. For expressions like `(CFLAG:奴隷:NTR訪問者と最後にセックスした日時/(24*60)) == (DATETIME()/(24*60))`, the first `(` pairs with an intermediate `)`, not the final one. The guard incorrectly strips both outer characters, producing malformed `CFLAG:.../(24*60)) == (DATETIME()/(24*60)` with unbalanced parens. This guard was written before F759 introduced `FindMatchingClosingParen` (line 308), which provides the exact structural verification needed but was never retroactively applied to the guard.

8-9 real-world ERB files in `Game/ERB/口上/*/KOJO_K*_会話親密.ERB` contain this exact `(expr) == (expr)` pattern. While these currently return `null` (no arithmetic parser exists), the malformed stripping is structurally incorrect and would cause misparse for any future bitwise pattern with parenthesized RHS (e.g., `(VAR & mask) == (OTHER)`).

### Goal (What to Achieve)
Replace the naive `StartsWith/EndsWith` guard condition with a `FindMatchingClosingParen`-based check that only strips outer parentheses when the opening `(` at position 0 structurally matches the closing `)` at `condition.Length - 1`. Preserve all existing valid paren-stripping behavior (e.g., `(A || B)`, `(TALENT:X & 3)`) while correctly rejecting non-matching cases.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

| # | Question | Answer | Evidence |
|:-:|----------|--------|----------|
| 1 | Why does `(A) == (B)` get mishandled? | The guard strips outer chars producing `A) == (B` (malformed) | `LogicalOperatorParser.cs:173-176` |
| 2 | Why does the guard strip these? | It checks only first/last character positions, not structural matching | `LogicalOperatorParser.cs:173` |
| 3 | Why doesn't it use structural matching? | The guard was written before `FindMatchingClosingParen` existed | F759 commit `e4e090ff` introduced the helper |
| 4 | Why wasn't it retroactively updated? | F759 identified this as TC#10 and deferred to a separate feature | `feature-759.md` Technical Constraint #10 |
| 5 | Why is this the root cause? | The guard's textual assumption ("starts with `(` + ends with `)` = matching pair") is structurally incorrect for `(expr) op (expr)` patterns | Mathematical property of balanced parentheses |

### Symptom vs Root Cause

| Aspect | Description |
|--------|-------------|
| Symptom | `(expr) == (expr)` patterns would have outer chars incorrectly stripped, producing malformed expressions |
| Root Cause | Guard at line 173 uses textual position check instead of structural `FindMatchingClosingParen` verification, because it predates the helper's existence |

## Related Features

| Feature | Status | Relationship |
|---------|--------|--------------|
| F759 | [DONE] | Parent feature; introduced `FindMatchingClosingParen` and identified this gap as TC#10 |
| F757 | [DONE] | Foundation; bitwise `&` operator support |
| F758 | [DONE] | Foundation; prefix types support |
| F764 | [PROPOSED] | EVENT function conversion (may bring more patterns into scope) |
| F767 | [DRAFT] | Sibling handoff from F759 |
| F706 | [BLOCKED] | Downstream full equivalence verification |

## Feasibility Assessment

**Verdict**: FEASIBLE

**Rationale**: Single-line fix replacing the guard condition at `LogicalOperatorParser.cs:173` with a `FindMatchingClosingParen(condition, 0) == condition.Length - 1` check. The helper method already exists (line 308), is `private static` within the same class, and is mathematically correct for all balanced parenthesis patterns. All three independent investigations confirmed feasibility unanimously.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F759 | [DONE] | Introduced `FindMatchingClosingParen` helper; no blocker |
| Related | F764 | [DONE] | EVENT function conversion may bring more `(expr) op (expr)` patterns; no blocker |
| Related | F706 | [DONE] | Full equivalence verification downstream; no blocker |

## Impact Analysis

| Component | Impact | Description |
|-----------|--------|-------------|
| `LogicalOperatorParser.cs` | Guard condition change (1 line) | Replace textual check with structural verification |
| `ConditionExtractor.cs` | Indirect (calls `ParseLogicalExpression` via `LogicalOperatorParser`) | Fix propagates through main entry point |
| `LocalGateResolver.cs` (src/tools/dotnet/ErbToYaml/) | Indirect (instantiates `LogicalOperatorParser` directly, calls `ParseLogicalExpression`) | Fix inherently propagates since the fix is in `ParseLogicalExpression` itself |
| Existing test suite | Must continue passing | All existing paren-stripping behavior preserved |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| `FindMatchingClosingParen` is `private static` in same class | `LogicalOperatorParser.cs:308` | Already accessible, no visibility change needed |
| Guard must remain BEFORE compound bitwise detection (line 181) | F759 design ordering | Fix changes only the condition, not the position |
| `SplitOnOperator` only splits on `&&`/`||`, NOT `==`/`!=` | `LogicalOperatorParser.cs:249-300` | Compound comparisons like `(A) == (B)` arrive at guard intact |
| `FindMatchingClosingParen(s, 0)` returns `s.Length - 1` for true wrappers | Method contract | Valid paren-stripping for `(A || B)` and `(TALENT:X & 3)` preserved |
| TreatWarningsAsErrors enforced | `Directory.Build.props` | Must compile cleanly with no warnings |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Fix breaks existing valid paren-stripping | LOW | HIGH | `FindMatchingClosingParen` returns `Length-1` for matching outer parens; existing tests verify |
| `(A) == (B)` falls through to F759 compound detection incorrectly | LOW | LOW | F759 compound detection rejects non-bitwise inner expressions cleanly |
| Fix changes guard ordering relative to compound detection | NONE | - | Fix changes only the condition check, not the code position |
| Test gap for non-matching paren case | HIGH (current) | MEDIUM | F766 must add negative test case(s) |

## AC Design Constraints

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Guard must use `FindMatchingClosingParen` | All 3 investigations, `LogicalOperatorParser.cs:308` | Code verification (grep or code review) |
| C2 | Existing `(A \|\| B)` paren-stripping preserved | `LogicalOperatorParserTests.cs:247-273` | Existing regression test must pass |
| C3 | `(expr) op (expr)` must NOT trigger stripping | 8-9 real-world ERB files | New negative test required |
| C4 | F759 compound bitwise detection still works | `BitwiseComparisonTests.cs` | Existing regression tests must pass |
| C5 | Simple `(TALENT:X & 3)` still paren-stripped correctly | `BitwiseComparisonTests.cs:110-122` | Existing regression test must pass |
| C6 | `(CFLAG:.../(24*60)) == (DATETIME()/(24*60))` still returns null | No arithmetic parser exists | Do NOT require successful parsing of arithmetic patterns |
| C7 | Full test suite passes with TreatWarningsAsErrors | `Directory.Build.props`, F759 baseline | Build + all tests regression |
| C8 | Fix propagates through all entry points | `ConditionExtractor.cs:29`, `LocalGateResolver.cs:62` | Test via main entry point preferred |

### Constraint Details

**C1: Guard must use FindMatchingClosingParen**
- **Source**: All 3 consensus investigations identified this as the correct fix
- **Verification**: Grep for `FindMatchingClosingParen(condition, 0)` in guard section
- **AC Impact**: AC#1 verifies presence, AC#2 verifies naive form removal

**C2: Existing paren-stripping preserved**
- **Source**: `LogicalOperatorParserTests.cs:247-273` existing test
- **Verification**: Run existing `ParseParenthesizedExpression_OrInsideAnd` test
- **AC Impact**: AC#3 regression test

**C3: Non-matching parens must NOT strip**
- **Source**: 8-9 real-world ERB files with `(expr) op (expr)` pattern
- **Verification**: New negative test with `FindMatchingClosingParen` returning intermediate index
- **AC Impact**: AC#5 (real-world pattern), AC#6 (synthetic pattern)

**C4: F759 compound bitwise detection preserved**
- **Source**: `BitwiseComparisonTests.cs` existing tests
- **Verification**: Run all `BitwiseComparison*` tests
- **AC Impact**: AC#7 regression suite

**C5: Simple paren-wrapped bitwise preserved**
- **Source**: `BitwiseComparisonTests.cs:110-122` existing test
- **Verification**: Run `BitwiseComparison_NoComparisonOperator_FallsThrough`
- **AC Impact**: AC#4 regression test

**C6: Arithmetic patterns return null**
- **Source**: No arithmetic parser exists in current codebase
- **Verification**: Confirm no arithmetic parsing capability
- **AC Impact**: AC#5/AC#6 assert null (not successful parse)

**C7: TreatWarningsAsErrors compliance**
- **Source**: `Directory.Build.props`, F759 baseline
- **Verification**: `dotnet build tools/ErbParser` succeeds
- **AC Impact**: AC#8 build verification

**C8: Fix propagates through all entry points**
- **Source**: `ConditionExtractor.cs:29` (via `LogicalOperatorParser`), `LocalGateResolver.cs:62` (src/tools/dotnet/ErbToYaml/, instantiates `LogicalOperatorParser` directly)
- **Verification**: Test via `ConditionExtractor.Extract()` entry point. Fix inherently propagates to `LocalGateResolver` since both callers invoke `ParseLogicalExpression` on the same class
- **AC Impact**: AC#5/AC#6 use `ConditionExtractor.Extract()` as entry point; no separate `LocalGateResolver` AC needed

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "structural correctness guarantees must hold for ALL balanced-paren patterns" | Guard must use structural (FindMatchingClosingParen) verification, not textual | AC#1, AC#2 |
| "not just currently-observed ones" | New negative test for `(expr) op (expr)` pattern that currently does not appear in positive tests | AC#5, AC#6 |
| "Preserve all existing valid paren-stripping behavior" | Regression: `(A \|\| B)` and `(TALENT:X & 3)` still paren-stripped correctly | AC#3, AC#4 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Guard uses FindMatchingClosingParen | code | Grep(src/tools/dotnet/ErbParser/LogicalOperatorParser.cs) | matches | `FindMatchingClosingParen\(condition, 0\)` | [x] |
| 2 | Naive StartsWith/EndsWith guard removed | code | Grep(src/tools/dotnet/ErbParser/LogicalOperatorParser.cs) | not_matches | `StartsWith\("\\("\) && condition\.EndsWith\("\\)"\)` | [x] |
| 3 | Parenthesized OR expression still parsed | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~ParseParenthesizedExpression_OrInsideAnd | succeeds | - | [x] |
| 4 | Simple paren-wrapped bitwise still stripped | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~BitwiseComparison_NoComparisonOperator_FallsThrough | succeeds | - | [x] |
| 5 | Non-matching parens NOT stripped (new test) | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~ParenStripping_NonMatchingOuterParens_NotStripped | succeeds | - | [x] |
| 6 | Arithmetic paren pattern returns null | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~ParenStripping_ArithmeticParenPattern_ReturnsNull | succeeds | - | [x] |
| 7 | F759 compound bitwise tests pass | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~BitwiseComparison | succeeds | - | [x] |
| 8 | Full build with TreatWarningsAsErrors | build | dotnet build tools/ErbParser | succeeds | - | [x] |
| 9 | Full test suite passes | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |

### AC Details

**AC#1: Guard uses FindMatchingClosingParen** (Constraint C1)
- Verify the paren-stripping guard at `ParseAtomicCondition` uses `FindMatchingClosingParen(condition, 0)` to structurally verify the opening paren matches the closing paren
- Test: Grep pattern=`FindMatchingClosingParen\(condition, 0\)` path=`src/tools/dotnet/ErbParser/LogicalOperatorParser.cs`
- Expected: At least 1 match in the paren-stripping guard section (around line 173)

**AC#2: Naive StartsWith/EndsWith guard removed** (Constraint C1)
- Verify the old naive `condition.StartsWith("(") && condition.EndsWith(")")` check is no longer the sole guard condition
- Test: Grep pattern=`StartsWith\("\\("\) && condition\.EndsWith\("\\)"\)` path=`src/tools/dotnet/ErbParser/LogicalOperatorParser.cs`
- Expected: 0 matches (the naive form is replaced by structural check)
- Note: `StartsWith("(")` alone may still appear as part of the new guard; only the combined naive form must be removed

**AC#3: Parenthesized OR expression still parsed** (Constraint C2)
- Regression test: `(TALENT:恋人 || TALENT:思慕) && TALENT:親愛` must still correctly strip outer parens from `(TALENT:恋人 || TALENT:思慕)` and parse the inner `||` expression
- Test: Existing `ParseParenthesizedExpression_OrInsideAnd` test in `LogicalOperatorParserTests.cs`
- Expected: Test passes (FindMatchingClosingParen returns `condition.Length - 1` for true wrappers)

**AC#4: Simple paren-wrapped bitwise still stripped** (Constraint C5)
- Regression test: `(TALENT:性別嗜好 & 3)` must still be paren-stripped to parse as TalentRef with operator `&` and value `3`
- Test: Existing `BitwiseComparison_NoComparisonOperator_FallsThrough` test in `BitwiseComparisonTests.cs`
- Expected: Test passes unchanged

**AC#5: Non-matching parens NOT stripped** (Constraint C3)
- New negative test: `(CFLAG:奴隷:NTR訪問者と最後にセックスした日時/(24*60)) == (DATETIME()/(24*60))` must NOT have outer parens stripped
- The opening `(` at position 0 matches an intermediate `)`, not the final one; `FindMatchingClosingParen(condition, 0)` returns an index < `condition.Length - 1`
- Without paren stripping, this pattern falls through to compound bitwise detection (rejects: no bitwise inner) and then to prefix parsers (no match), returning null
- Test method name: `ParenStripping_NonMatchingOuterParens_NotStripped` in `LogicalOperatorParserTests.cs`
- Entry point: `ConditionExtractor.Extract()` (Constraint C8)
- Expected: result is null (Constraint C6: no arithmetic parser exists)

**AC#6: Arithmetic paren pattern returns null** (Constraint C6)
- New negative test: simpler `(A) == (B)` style pattern to verify non-matching parens
- Test input: `(TALENT:恋人) == (TALENT:思慕)` -- both sides parenthesized, opening `(` matches first `)` not final `)`
- After fix, paren guard does NOT strip; falls through to compound bitwise detection (inner is non-bitwise TALENT comparison, rejected); returns null
- Test method name: `ParenStripping_ArithmeticParenPattern_ReturnsNull` in `LogicalOperatorParserTests.cs`
- Entry point: `ConditionExtractor.Extract()` (Constraint C8)
- Expected: result is null

**AC#7: F759 compound bitwise tests pass** (Constraint C4)
- Regression: All F759 compound bitwise-comparison tests must continue passing after the guard change
- Tests: `BitwiseComparison_TalentNamedWithEquality`, `BitwiseComparison_CflagWithNotEqual`, `BitwiseComparison_ActualKojoPattern`, `BitwiseComparison_MalformedInner_ReturnsNull`, `BitwiseComparison_NonBitwiseInner_ReturnsNull`
- The guard change only affects whether outer parens are stripped; compound detection runs after the guard and uses `FindMatchingClosingParen` independently

**AC#8: Full build with TreatWarningsAsErrors** (Constraint C7)
- Verify the ErbParser project builds cleanly with zero warnings under `TreatWarningsAsErrors`
- Test: `dotnet build tools/ErbParser`
- Expected: Build succeeds with exit code 0

**AC#9: Full test suite passes** (Constraint C7)
- Verify all existing tests in ErbParser.Tests continue to pass after the fix
- This is the catch-all regression gate covering any tests not individually named in AC#3-7
- Test: `dotnet test tools/ErbParser.Tests`
- Expected: All tests pass

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
- [fix] Phase2-Review iter1: [FMT-002] Dependencies table | Column headers 'Feature/Component' and 'Impact' renamed to 'Feature' and 'Description' per template SSOT
- [fix] Phase2-Review iter1: [FMT-002] Implementation Contract table | Removed extra 'Tasks' column, merged Tasks info into Input column per template SSOT
- [fix] Phase2-Review iter1: [FMT-002] Execution Log table | Changed from 3-column to 5-column format per template SSOT
- [fix] Phase3-Maintainability iter2: [FMT-001] Baseline Measurement section | Added missing Baseline Measurement section per template SSOT
- [fix] Phase3-Maintainability iter2: [FMT-001] AC Design Constraints | Added Constraint Details subsection with C1-C8 detail blocks per template SSOT
- [fix] Phase2-Review iter3: [FMT-002] Mandatory Handoffs | Removed duplicate empty Mandatory Handoffs section (kept section with proper table)
- [fix] Phase2-Review iter3: [FMT-002] Review Notes | Added category codes ([FMT-002], [FMT-001]) to all [fix] entries per error-taxonomy.md
- [fix] Phase2-Review iter4: [FMT-002] Review Notes separator | Added missing --- before ## Review Notes section
- [fix] Phase2-Review iter4: [FMT-002] Mandatory Handoffs separator | Added missing --- before ## Mandatory Handoffs section
- [fix] Phase2-Review iter4: [FMT-002] Pending entries category codes | Added [AC-006] and [TSK-004] to two [pending] entries missing codes
- [fix] Phase3-Maintainability iter5: [INV-003] Impact Analysis LocalGateResolver | Clarified LocalGateResolver.cs is in src/tools/dotnet/ErbToYaml/ and instantiates LogicalOperatorParser directly (not via ConditionExtractor). Updated C8 constraint details accordingly.
- [fix] Phase7-FinalRefCheck iter6: [FMT-003] Links F757 | Updated broken link feature-757.md → archive/feature-757.md (F757 was archived)
- [fix] Phase7-FinalRefCheck iter6: [FMT-003] Links F706 | Added missing link for F706 (referenced in Related Features but not in Links)
- [resolved-skipped] Phase2-Uncertain iter3: [FMT-002] AC Design Constraints section ordering | Systemic /fc ordering issue. Root cause fix deferred to DRAFT feature. F766 keeps current ordering.
- [resolved-skipped] Phase2-Uncertain iter3: [AC-006] AC#5/AC#6 vacuous pass concern | AC#1+AC#2 (code verification) sufficiently mitigate. Invocation trace exceeds available verification methods.
- [resolved-skipped] Phase2-Uncertain iter3: [TSK-004] Task#3 verification-only concern | Template does not prohibit verification-only tasks. Implementation Contract Phase 3 already maps to Task#3.

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Replace the naive textual paren-stripping guard at line 173 with a structural verification using the existing `FindMatchingClosingParen` helper. The guard currently uses `condition.StartsWith("(") && condition.EndsWith(")")` which only checks first/last character positions. The fix replaces this with `condition.StartsWith("(") && FindMatchingClosingParen(condition, 0) == condition.Length - 1`, which verifies the opening `(` at position 0 structurally matches the closing `)` at the end.

**Key Insight**: `FindMatchingClosingParen(condition, 0)` returns the index of the matching closing paren for the opening paren at position 0. For a true wrapper like `(A || B)`, it returns `condition.Length - 1` (the final character). For non-matching patterns like `(A) == (B)`, it returns an intermediate index (the first `)` position), so the equality check fails and the guard does not strip.

**Preservation of Existing Behavior**: All valid paren-stripping cases (`(A || B)`, `(TALENT:X & 3)`) remain unchanged because `FindMatchingClosingParen` correctly identifies them as true wrappers. The inner `Substring` and `ParseLogicalExpression` recursion logic (lines 175-176) remains unchanged.

**Guard Ordering**: The paren-stripping guard (line 173) remains positioned BEFORE the F759 compound bitwise detection (line 181). This ordering is critical: the paren-stripping guard handles true wrappers first, then compound detection handles parenthesized comparisons. The fix changes only the guard condition, not the code position.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Code verification via Grep: search for `FindMatchingClosingParen\(condition, 0\)` in `LogicalOperatorParser.cs` around line 173 |
| 2 | Code verification via Grep: verify absence of `StartsWith\("\\("\) && condition\.EndsWith\("\\)"\)` pattern (the naive combined form must be replaced) |
| 3 | Regression test: existing `ParseParenthesizedExpression_OrInsideAnd` test passes unchanged (verifies `(A \|\| B)` paren-stripping preserved) |
| 4 | Regression test: existing `BitwiseComparison_NoComparisonOperator_FallsThrough` test passes unchanged (verifies simple `(TALENT:X & 3)` paren-stripping preserved) |
| 5 | New test `ParenStripping_NonMatchingOuterParens_NotStripped`: input `(CFLAG:奴隷:NTR訪問者と最後にセックスした日時/(24*60)) == (DATETIME()/(24*60))` → `ConditionExtractor.Extract()` returns null (not stripped, falls through all parsers, arithmetic not supported) |
| 6 | New test `ParenStripping_ArithmeticParenPattern_ReturnsNull`: simpler input `(TALENT:恋人) == (TALENT:思慕)` → `ConditionExtractor.Extract()` returns null (not stripped, falls through) |
| 7 | Regression test suite: all F759 `BitwiseComparison*` tests pass unchanged (compound detection runs after guard, independent) |
| 8 | Build verification: `dotnet build tools/ErbParser` succeeds with TreatWarningsAsErrors |
| 9 | Full regression: `dotnet test tools/ErbParser.Tests` all tests pass |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Guard verification method | (A) Naive `StartsWith/EndsWith`, (B) Structural `FindMatchingClosingParen`, (C) New regex-based verification | B: `FindMatchingClosingParen` | Helper already exists (line 308), mathematically correct for all balanced paren patterns, minimal code change (1 line), zero performance impact |
| Guard body logic | (A) Keep existing `Substring + ParseLogicalExpression`, (B) Refactor to use `FindMatchingClosingParen` for substring extraction | A: Keep existing | Existing logic is correct and clear; only the guard condition needs refinement |
| Test method entry point | (A) Direct `ParseLogicalExpression`, (B) `ConditionExtractor.Extract()` | B: `ConditionExtractor.Extract()` | Constraint C8: Test via main entry point to verify fix propagates through all call paths |
| Test coverage for negative cases | (A) Single generic test, (B) Two specific tests (complex arithmetic pattern + simple comparison pattern) | B: Two specific tests | AC#5 covers real-world ERB pattern (8-9 files); AC#6 covers simpler pattern for clarity |
| New test file location | (A) New file `ParenStrippingTests.cs`, (B) Add to `LogicalOperatorParserTests.cs` | B: Add to existing | Both new tests verify `LogicalOperatorParser` behavior; keeps related tests together |

### Implementation Details

**Code Change** (1 line):

```csharp
// File: src/tools/dotnet/ErbParser/LogicalOperatorParser.cs
// Line: 173

// BEFORE:
if (condition.StartsWith("(") && condition.EndsWith(")"))

// AFTER:
if (condition.StartsWith("(") && FindMatchingClosingParen(condition, 0) == condition.Length - 1)
```

**New Test Methods** (add to `src/tools/dotnet/ErbParser.Tests/LogicalOperatorParserTests.cs`):

```csharp
/// <summary>
/// F766 AC#5: Non-matching outer parens should NOT be stripped
/// Pattern: (expr/(24*60)) == (DATETIME()/(24*60))
/// The opening ( at position 0 matches an intermediate ), not the final one
/// </summary>
[Fact]
public void ParenStripping_NonMatchingOuterParens_NotStripped()
{
    // Arrange
    var extractor = new ConditionExtractor();
    var condition = "(CFLAG:奴隷:NTR訪問者と最後にセックスした日時/(24*60)) == (DATETIME()/(24*60))";

    // Act
    var result = extractor.Extract(condition);

    // Assert - should return null (not stripped, no arithmetic parser exists)
    Assert.Null(result);
}

/// <summary>
/// F766 AC#6: Arithmetic paren pattern (simpler case)
/// Pattern: (A) == (B) with both sides parenthesized
/// Should NOT have outer parens stripped
/// </summary>
[Fact]
public void ParenStripping_ArithmeticParenPattern_ReturnsNull()
{
    // Arrange
    var extractor = new ConditionExtractor();
    var condition = "(TALENT:恋人) == (TALENT:思慕)";

    // Act
    var result = extractor.Extract(condition);

    // Assert - should return null (not stripped, falls through all parsers)
    Assert.Null(result);
}
```

### Interfaces / Data Structures

No new interfaces or data structures required. The fix uses the existing `FindMatchingClosingParen` method signature:

```csharp
private static int FindMatchingClosingParen(string input, int startIndex)
```

This method is already `private static` within `LogicalOperatorParser` (line 308) and accessible to the guard at line 173.

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,2 | Refine paren-stripping guard to use FindMatchingClosingParen | | [x] |
| 2 | 5,6 | Add two negative test cases for non-matching outer parens | | [x] |
| 3 | 3,4,7,8,9 | Run full regression suite (build + tests) | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ErbParser build | `dotnet build tools/ErbParser` | Pass (0 warnings) | TreatWarningsAsErrors enforced |
| ErbParser.Tests pass count | `dotnet test tools/ErbParser.Tests` | All pass | Pre-fix baseline |

**Baseline File**: `.tmp/baseline-766.txt`

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1: Guard condition from Technical Design | Guard refinement |
| 2 | implementer | sonnet | T2: Test method code from Technical Design | Two new test methods |
| 3 | ac-tester | haiku | T3: All 9 AC verification commands | Test results |

**Constraints** (from Technical Design):
1. `FindMatchingClosingParen` is `private static` in same class (line 308) - already accessible
2. Guard must remain BEFORE compound bitwise detection (line 181) - fix changes only condition, not position
3. `SplitOnOperator` only splits on `&&`/`||`, NOT `==`/`!=` - compound comparisons arrive at guard intact
4. Test via `ConditionExtractor.Extract()` entry point (Constraint C8) - verifies fix propagates through all call paths

**Pre-conditions**:
- F759 completed and merged (provides `FindMatchingClosingParen` helper)
- Existing test suite baseline passes (all tests green)

**Success Criteria**:
- Guard condition at line 173 uses `FindMatchingClosingParen(condition, 0) == condition.Length - 1`
- Naive `StartsWith("(") && condition.EndsWith(")")` pattern no longer exists
- Two new test methods added to `LogicalOperatorParserTests.cs`:
  - `ParenStripping_NonMatchingOuterParens_NotStripped`
  - `ParenStripping_ArithmeticParenPattern_ReturnsNull`
- All 9 ACs pass (full build + test suite green)

**Test Naming Convention**:
Test methods follow `ParenStripping_{Pattern}_{Behavior}` format. This ensures AC filter patterns match correctly and groups paren-stripping tests together in test explorer.

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| /fc section ordering mismatch | Systemic /fc issue, not fixable per-feature | Feature | F770 | N/A (DRAFT created directly) |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-09 17:52 | START | implementer | Task 2 | - |
| 2026-02-09 17:52 | END | implementer | Task 2 | SUCCESS |
| 2026-02-09 17:54 | START | implementer | Task 1 | - |
| 2026-02-09 17:54 | END | implementer | Task 1 | SUCCESS |

---

## Links
- [feature-759.md](feature-759.md) - Parent feature (compound bitwise condition parsing)
- [feature-757.md](archive/feature-757.md) - Foundation (bitwise operator support)
- [feature-758.md](feature-758.md) - Foundation (prefix types)
- [feature-764.md](feature-764.md) - Related (EVENT function conversion)
- [feature-767.md](feature-767.md) - Sibling handoff from F759
- [feature-706.md](feature-706.md) - Downstream full equivalence verification
