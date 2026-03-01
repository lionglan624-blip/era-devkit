# Feature 759: Compound Bitwise Condition Parsing

## Status: [DONE]
<!-- fl-reviewed: 2026-02-08T00:00:00Z -->

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

## Review Context (F758 POST-LOOP)

### Origin

| Field | Value |
|-------|-------|
| Parent Feature | F758 (Prefix-Based Variable Type Expansion) |
| Discovery Point | Philosophy Gate (POST-LOOP Step 6.3) |
| Timestamp | F758 completion |

### Identified Gap

Compound bitwise-comparison `(VAR & mask) == value` (1 occurrence in kojo) requires a two-stage parser that first evaluates the bitwise expression, then compares the result. This is fundamentally different from the prefix-based pattern handled by F758.

### Review Evidence

| Field | Value |
|-------|-------|
| Gap Source | F758 Scope Analysis |
| Derived Task | Parse compound bitwise-comparison |
| Comparison Result | Not parseable by prefix-based approach |
| DEFER Reason | Two-stage evaluation requires different parser architecture |

### Files Involved

| File | Relevance |
|------|-----------|
| src/tools/dotnet/ErbParser/LogicalOperatorParser.cs | Detection point (ParseAtomicCondition) |
| Game/ERB/口上/U_汎用/KOJO_KU_愛撫.ERB:63 | Single kojo occurrence |

### Parent Review Observations

F758 correctly identified this pattern as out-of-scope during its implementation. The compound bitwise-comparison requires expression-level operator precedence, which is a fundamentally different parsing concept from the prefix-based variable type dispatch that F758 implemented.

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)
(Inherited from F758) Continue toward full equivalence testing by resolving the compound bitwise-comparison category beyond prefix-based types.

### Problem (Current Issue)
The ErbParser's `ParseAtomicCondition` (LogicalOperatorParser.cs:164-210) cannot parse compound bitwise-comparison expressions of the form `(VAR & mask) == value` because the parser architecture has no concept of expression-level operator precedence. The paren-stripping guard at line 169 (`StartsWith("(") && EndsWith(")")`) correctly does NOT trigger since the string ends with the comparison value, not `)`. All subsequent prefix-anchored parsers (TalentConditionParser, VariableConditionParser, etc.) fail on the `(`-prefixed string. Furthermore, even if the inner expression were extracted, the existing `bitwise_and` YAML operator only evaluates truthiness (`(val & mask) != 0`), not equality comparison (`(val & mask) == expected`). This dual gap -- no detection and no evaluation semantics -- means `(TALENT:2 & 3) == 3` in KOJO_KU_愛撫.ERB:63 returns null, blocking full kojo equivalence coverage.

### Goal (What to Achieve)
Add compound bitwise-comparison support across the ErbParser/ErbToYaml/KojoComparer pipeline: (1) detect `(expr) op value` patterns in ParseAtomicCondition, (2) introduce a new ICondition type to represent the two-stage evaluation, (3) convert to a new YAML operator that captures both mask and comparison semantics, and (4) evaluate the compound expression in KojoBranchesParser -- without regressing the 26+ existing truthiness-only bitwise patterns.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

**Symptom**: `(TALENT:2 & 3) == 3` in `KOJO_KU_愛撫.ERB:63` returns null from ConditionExtractor.

**5 Whys**:
1. `ParseAtomicCondition` receives `(TALENT:2 & 3) == 3` as an atomic unit after `&&` splitting, but no existing branch handles this form (LogicalOperatorParser.cs:164-210)
2. The paren-stripping guard at line 169 (`StartsWith("(") && EndsWith(")")`) correctly does NOT strip parens because the string ends with `3`, not `)` (LogicalOperatorParser.cs:169)
3. All subsequent parsers (TalentConditionParser, VariableConditionParser, etc.) fail because their regexes are anchored to `^PREFIX:` which doesn't match `(`-prefixed strings (TalentConditionParser.cs:18-19, VariableConditionParser.cs:18-19)
4. Even if paren-stripping worked, `TALENT:2 & 3` would produce a truthiness check (`bitwise_and` = `(val & mask) != 0`), losing the `== 3` equality comparison semantics (KojoBranchesParser.cs:288)
5. **Root Cause**: The parser architecture was designed for flat condition patterns and logical composition only. It has no concept of expression-level operator precedence where bitwise operations produce intermediate values that are then compared. No ICondition type exists for this two-stage evaluation.

| Symptom | Root Cause |
|---------|------------|
| `(TALENT:2 & 3) == 3` returns null | Parser lacks expression evaluation layer between logical splitting and atomic condition matching; no ICondition type for compound bitwise-comparison |

## Related Features

| Feature | Status | Relationship |
|---------|--------|--------------|
| F758 | [DONE] | Parent (deferred compound bitwise to F759) |
| F757 | [DONE] | Foundation (bitwise `&` operator support) |
| F760 | [PROPOSED] | TALENT:2 numeric index overlap |
| F761 | [PROPOSED] | LOCAL:1 in same condition |
| F762 | [DONE] | Sibling (no overlap) |
| F706 | [BLOCKED] | Downstream consumer |

## Feasibility Assessment

**Verdict**: FEASIBLE
- All predecessor dependencies are [DONE] (F758, F757)
- Surgically scoped: pattern detection + 1 new AST type + conversion/evaluation
- F760 is soft dependency (can test with named variables)

## Dependencies

| Type | Feature | Status | Description |
|------|-------------------|--------|-------------|
| Predecessor | F758 | [DONE] | All 13 variable types registered |
| Predecessor | F757 | [DONE] | Simple bitwise `&` support |
| Related | F760 | [DONE] | TALENT:2 numeric index (can work around with Name="2") |
| Related | F761 | [DONE] | LOCAL:1 parsing (separate condition, no blocker) |

---

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| `src/tools/dotnet/ErbParser/LogicalOperatorParser.cs` | MODIFY | Add `(expr) op value` detection in ParseAtomicCondition |
| `src/tools/dotnet/ErbParser/ICondition.cs` | MODIFY | Add JsonDerivedType for new condition type |
| `src/tools/dotnet/ErbParser/BitwiseComparisonCondition.cs` | NEW | New ICondition type with Inner (ICondition), ComparisonOp, ComparisonValue |
| `src/tools/dotnet/ErbToYaml/DatalistConverter.cs` | MODIFY | Add conversion case for compound bitwise |
| `src/tools/dotnet/KojoComparer/KojoBranchesParser.cs` | MODIFY | Add two-stage evaluation for compound bitwise |
| Test files (3 new) | NEW | Parser, conversion, evaluation tests |

## Technical Constraints

1. Paren-stripping logic must not regress for existing `(VAR & mask)` truthiness patterns (26+ kojo occurrences work correctly)
2. Inner variable can be any prefix type (TALENT, TFLAG, TCVAR, etc.) -- parser must be generic
3. YAML schema needs design for compound bitwise representation (e.g., `bitwise_and_cmp`)
4. TALENT:2 is parsed as Name="2" (F760 scope) -- tests should use named variables to avoid dependency
5. `bitwise_and` evaluator currently hardcodes non-zero check (truthiness only) -- new operator needed for equality comparison
6. JsonDerivedType registration required in ICondition.cs
7. ValidateConditionScope does NOT need updating — `bitwise_and_cmp` is an operator key inside opDict (nested within TALENT/variable conditions), not a top-level condition key
8. TreatWarningsAsErrors is enforced (Directory.Build.props)
9. dialogue-schema.json uses permissive `type: object` for conditionElement which already accepts `bitwise_and_cmp` nested dictionary structure — no schema file change needed for F759 scope (schema documentation update is desirable but non-blocking)
10. Paren-stripping guard ordering: The existing paren-stripping guard at LogicalOperatorParser.cs:169 uses naive `StartsWith("(") && EndsWith(")")` check. Compound bitwise detection is placed AFTER this guard (C5). This is safe for all known patterns because no compound bitwise expression ends with `)` (they end with an integer value like `3`). If a future pattern like `(VAR & mask) == (OTHER)` were introduced, the paren-stripping guard would incorrectly trigger first. Guard refinement (using `FindMatchingClosingParen`) is out of F759 scope but tracked as a known limitation.

## Risks

| Risk | Mitigation |
|------|------------|
| Over-engineering for 1 kojo occurrence | Scope to minimal viable: detect, parse, convert, evaluate. Design generic but implement narrow |
| F760 dependency for end-to-end kojo test | Test with named variables; F760 is soft dependency |
| YAML schema design complexity | Choose simplest representation that captures mask + comparison semantics |
| Regression in existing paren-wrapped bitwise parsing | Regression test suite for 26+ existing patterns |

---

## AC Design Constraints

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Only 1 kojo compound bitwise-comparison occurrence | Grep | Primary AC targets this specific pattern |
| C2 | 26+ `(VAR & mask)` truthiness patterns already parse correctly | Multiple kojo files | Regression test (must not break) |
| C3 | TALENT:2 parsed as Name="2" (F760 scope) | TalentConditionParser.cs | Test with named variables to avoid F760 dependency |
| C4 | `bitwise_and` = truthiness only (`!= 0`) | KojoBranchesParser.cs:288 | New YAML operator needed for equality comparison |
| C5 | Detection must come after paren-stripping check in ParseAtomicCondition | LogicalOperatorParser.cs:169 | Order of checks matters for correctness |
| C6 | KojoComparer has pre-existing failure (FileDiscoveryTests) | F758 C3 | Filter out FileDiscoveryTests from AC#6 KojoComparer.Tests run |
| C7 | Inner expression reuses existing variable parsers | VariableConditionParser generic | Parser implementation is generic across prefix types |
| C8 | YAML must be evaluable by KojoBranchesParser | EvaluateOperator switch | New case in evaluator |
| C9 | Non-kojo patterns exist but are out of scope | COMABLE_400.ERB, COMMON.ERB | Design generalizable, test kojo only |
| C10 | The condition is dead code (LOCAL:1=0 on line 61) | All 3 investigations | Impact is parser completeness, not runtime behavior |

### Constraint Details

**C1: Single Kojo Occurrence**
- **Source**: Grep across Game/ERB/口上/ for `(.*&.*)==`
- **Verification**: `KOJO_KU_愛撫.ERB:63` is the only compound bitwise-comparison in kojo
- **AC Impact**: AC#7 targets this pattern directly; no need for multi-occurrence testing

**C2: 26+ Existing Truthiness Patterns**
- **Source**: Grep across kojo for `(VAR & mask)` without comparison operator
- **Verification**: Existing patterns parse to VariableRef with Operator="&" and `bitwise_and` YAML
- **AC Impact**: AC#6 and AC#10 verify no regression in these patterns

**C3: TALENT:2 Numeric Index (F760 Scope)**
- **Source**: TalentConditionParser.cs:18-19 regex anchored to named talent
- **Verification**: TALENT:2 → TalentRef with Name="2", not Index=2
- **AC Impact**: AC#7 tests use named variables (性別嗜好) to avoid F760 dependency

**C4: bitwise_and = Truthiness Only**
- **Source**: KojoBranchesParser.cs:288 `(stateValue & expected) != 0`
- **Verification**: `bitwise_and` evaluates truthiness, not equality
- **AC Impact**: AC#5 introduces distinct `bitwise_and_cmp` operator with mask/op/value structure (general comparison, not equality-only)

**C5: Detection Order in ParseAtomicCondition**
- **Source**: LogicalOperatorParser.cs:169 paren-stripping guard
- **Verification**: Compound detection placed AFTER paren-stripping, BEFORE prefix parsers
- **AC Impact**: AC#3 verifies detection code exists in LogicalOperatorParser.cs

**C6: Pre-existing KojoComparer Failures**
- **Source**: F758 AC Design Constraint C3 (baseline: 109 pass, 1 fail, 3 skip)
- **Verification**: `FileDiscoveryTests.DiscoverTestCases_WithRealFiles_ReturnsTestCases` fails due to ErbRunner build issue (unrelated to F759)
- **AC Impact**: AC#6 filters out `FileDiscoveryTests` from KojoComparer.Tests to avoid pre-existing failure. ErbParser.Tests and ErbToYaml.Tests run unfiltered with `succeeds` matcher

**C7: Inner Expression Reuses Existing Parsers**
- **Source**: VariableConditionParser generic dispatch (F758)
- **Verification**: Recursive ParseAtomicCondition call handles inner `TALENT:x & 3`
- **AC Impact**: AC#7 positive tests verify inner parsing produces correct ICondition types

**C8: YAML Must Be Evaluable**
- **Source**: KojoBranchesParser.cs EvaluateOperator switch + TALENT inline block
- **Verification**: `bitwise_and_cmp` dictionary parsed and two-stage evaluated
- **AC Impact**: AC#9 verifies correct evaluation with TALENT-keyed state

**C9: Non-Kojo Patterns Out of Scope**
- **Source**: COMABLE_400.ERB, COMMON.ERB contain compound bitwise patterns
- **Verification**: Design is generic (any variable type), but testing scoped to kojo
- **AC Impact**: AC#7 tests with TALENT and CFLAG (generic across types)

**C10: Dead Code Condition**
- **Source**: KOJO_KU_愛撫.ERB:61 sets LOCAL:1=0, line 63 checks LOCAL:1 && (TALENT:2 & 3)==3
- **Verification**: Condition is always false at runtime
- **AC Impact**: Feature targets parser completeness (AST coverage), not runtime behavior

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "full equivalence testing" | Every parseable condition pattern must produce a valid AST node, not null (AST-level completeness including dead-code paths per C10); existing patterns must not regress | AC#1, AC#2, AC#3, AC#6, AC#10 |
| "resolving the compound bitwise-comparison category" | The compound bitwise-comparison pattern `(VAR & mask) == value` must be parsed, converted to YAML, and evaluated | AC#1, AC#4, AC#5, AC#7, AC#8 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | BitwiseComparisonCondition.cs exists | file | Glob(src/tools/dotnet/ErbParser/BitwiseComparisonCondition.cs) | exists | - | [x] |
| 2 | ICondition registers BitwiseComparisonCondition | code | Grep(src/tools/dotnet/ErbParser/ICondition.cs) | matches | "JsonDerivedType.*BitwiseComparisonCondition" | [x] |
| 3 | ParseAtomicCondition detects compound bitwise pattern | code | Grep(src/tools/dotnet/ErbParser/LogicalOperatorParser.cs) | contains | "BitwiseComparisonCondition" | [x] |
| 4 | DatalistConverter handles BitwiseComparisonCondition | code | Grep(src/tools/dotnet/ErbToYaml/DatalistConverter.cs) | contains | "BitwiseComparisonCondition" | [x] |
| 5 | KojoBranchesParser evaluates compound bitwise operator | code | Grep(src/tools/dotnet/KojoComparer/KojoBranchesParser.cs) | contains | "bitwise_and_cmp" | [x] |
| 6 | Existing tool tests pass (no regression) | test | dotnet test tools/ErbParser.Tests && dotnet test tools/ErbToYaml.Tests && dotnet test tools/KojoComparer.Tests --filter "FullyQualifiedName!~FileDiscoveryTests" | succeeds | - | [x] |
| 7 | Parser unit tests pass | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~BitwiseComparison | succeeds | - | [x] |
| 8 | Conversion unit tests pass | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~BitwiseComparison | succeeds | - | [x] |
| 9 | Evaluation unit tests pass | test | dotnet test tools/KojoComparer.Tests --filter FullyQualifiedName~BitwiseComparison | succeeds | - | [x] |
| 10 | Existing bitwise truthiness tests pass | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~BitwiseOperator | succeeds | - | [x] |
| 11 | Tools build with zero warnings | build | dotnet build tools/ErbParser && dotnet build tools/ErbToYaml && dotnet build tools/KojoComparer | succeeds | - | [x] |
| 12 | No technical debt markers in BitwiseComparisonCondition.cs | code | Grep(src/tools/dotnet/ErbParser/BitwiseComparisonCondition.cs) | not_matches | "TODO|FIXME|HACK" | [x] |
| 13 | DRAFT feature-766.md exists | file | Glob(pm/features/feature-766.md) | exists | - | [x] |
| 14 | DRAFT feature-767.md exists | file | Glob(pm/features/feature-767.md) | exists | - | [x] |
| 15 | F766 registered in index-features.md | code | Grep(pm/index-features.md) | contains | "766" | [x] |
| 16 | F767 registered in index-features.md | code | Grep(pm/index-features.md) | contains | "767" | [x] |

### AC Details

**AC#1: BitwiseComparisonCondition.cs exists**
- New ICondition type file representing the compound bitwise-comparison pattern `(VAR & mask) op value`
- Properties: Inner (ICondition for the bitwise expression, e.g. TalentRef with Operator="&" and Value="3" where Value is the mask), ComparisonOp (string, e.g. "=="), ComparisonValue (string)
- Must implement ICondition interface
- Constraint: C1, C7 (inner expression reuses existing variable parsers via generic ICondition)

**AC#2: ICondition registers BitwiseComparisonCondition**
- Test: Grep pattern=`JsonDerivedType.*BitwiseComparisonCondition` path=src/tools/dotnet/ErbParser/ICondition.cs
- A new JsonDerivedType attribute must be added to enable polymorphic JSON serialization
- Constraint: C1 (Technical Constraint #6 in feature)

**AC#3: ParseAtomicCondition detects compound bitwise pattern**
- Test: Grep pattern=`BitwiseComparisonCondition` path=src/tools/dotnet/ErbParser/LogicalOperatorParser.cs
- Detection logic must be placed AFTER the paren-stripping guard at line 169 and BEFORE the prefix-anchored parsers
- Pattern detection: input starts with `(`, find matching `)`, then look for comparison operator (`==`, `!=`, `>`, `>=`, `<`, `<=`) followed by a value
- Inner expression parsing: extract content between parens, split on `&`, parse left side with existing variable parsers
- Constraint: C5 (order matters), C7 (reuses existing parsers)

**AC#4: DatalistConverter handles BitwiseComparisonCondition**
- Test: Grep pattern=`BitwiseComparisonCondition` path=src/tools/dotnet/ErbToYaml/DatalistConverter.cs
- New case in ConvertConditionToYaml switch for BitwiseComparisonCondition
- YAML output format: compound operator key (e.g., `bitwise_and_cmp`) capturing both mask and comparison semantics
- Example: `(TALENT:性別嗜好 & 1) == 1` -> `{ TALENT: { index: { bitwise_and_cmp: { mask: "1", op: "eq", value: "1" } } } }`
- Constraint: C4 (new operator needed, `bitwise_and` is truthiness only)

**AC#5: KojoBranchesParser evaluates compound bitwise operator**
- Test: Grep pattern=`bitwise_and_cmp` path=src/tools/dotnet/KojoComparer/KojoBranchesParser.cs
- Shared `EvaluateBitwiseAndCmp` helper method handles two-stage evaluation: (1) compute `stateValue & mask`, (2) compare result with expected value using the comparison operator
- **CRITICAL**: `bitwise_and_cmp` handling must be added to BOTH the TALENT inline evaluation block (lines 127-143) AND `EvaluateVariableCondition` (lines 315-330). TALENT conditions route through the inline block, NOT `EvaluateVariableCondition`.
- Example: state TALENT:16 = 7, mask = 3, op = eq, value = 3 -> (7 & 3) = 3 == 3 -> true
- Constraint: C4, C8 (YAML must be evaluable)

**AC#6: Existing tool tests pass (no regression)**
- Verifies no regression across all three tool test suites
- ErbParser.Tests and ErbToYaml.Tests: `succeeds` (all tests pass cleanly)
- KojoComparer.Tests: filter excludes `FileDiscoveryTests` (pre-existing ErbRunner build failure unrelated to F759, documented in F758 C3 baseline: 109 pass, 1 fail, 3 skip)
- Constraint: C2 (26+ existing truthiness patterns must not break), C6 (pre-existing KojoComparer failure)

**AC#7: Parser unit tests pass (Pos+Neg)**
- Positive: `(TALENT:性別嗜好 & 3) == 3` -> BitwiseComparisonCondition with correct properties
- Positive: `(CFLAG:奴隷:フラグ & 1) != 0` -> BitwiseComparisonCondition with `!=` comparison
- Positive: `(TALENT:2 & 3) == 3` -> BitwiseComparisonCondition with Inner=TalentRef(Name="2", Operator="&", Value="3"), ComparisonOp="==", ComparisonValue="3" (actual kojo pattern from KOJO_KU_愛撫.ERB:63; TALENT:2 parses as Name="2", no F760 dependency)
- Negative: `(TALENT:性別嗜好 & 3)` (no comparison) -> should NOT match compound pattern (falls through to existing truthiness handling)
- Negative: malformed `(TALENT:性別嗜好 & ) == 3` -> returns null
- Negative: `(TALENT:性別嗜好 == 3) == 5` -> returns null (inner expression has non-bitwise operator, rejected by HasBitwiseOperator parse-time validation)
- Constraint: C3 (named variables for primary tests; TALENT:2 added for actual pattern verification without F760 dependency)

**AC#8: Conversion unit tests pass (Pos+Neg)**
- Positive: BitwiseComparisonCondition converts to YAML with `bitwise_and_cmp` operator
- Positive: Verify YAML structure contains mask and comparison value
- Negative: BitwiseComparisonCondition with unsupported inner condition type (non-& operator) returns null from ConvertBitwiseComparisonCondition
- Constraint: C4 (new YAML operator)

**AC#9: Evaluation unit tests pass (Pos+Neg)**
- **CRITICAL**: TALENT state keys use `TALENT:{index}` format (e.g., `TALENT:16`) to exercise the TALENT inline evaluation code path. Non-TALENT keys (e.g., `CFLAG:0`) exercise the EvaluateVariableCondition code path. Both paths MUST be tested.
- Positive (TALENT path): state `TALENT:16`=7, mask=3, compare eq 3 -> `(7 & 3) = 3 == 3` -> true
- Positive (TALENT path): state `TALENT:16`=5, mask=3, compare eq 1 -> `(5 & 3) = 1 == 1` -> true
- Negative (TALENT path): state `TALENT:16`=6, mask=3, compare eq 3 -> `(6 & 3) = 2 != 3` -> false
- Negative (TALENT path): state `TALENT:16`=0, mask=3, compare eq 3 -> `(0 & 3) = 0 != 3` -> false
- Positive (ne): state `TALENT:16`=7, mask=3, compare ne 1 -> `(7 & 3) = 3 != 1` -> true
- Positive (EvaluateVariableCondition path): state `CFLAG:0`=7, mask=3, compare eq 3 -> `(7 & 3) = 3 == 3` -> true
- Negative (EvaluateVariableCondition path): state `CFLAG:0`=6, mask=3, compare eq 3 -> `(6 & 3) = 2 != 3` -> false
- Constraint: C8 (YAML must be evaluable)

**AC#10: Existing bitwise truthiness tests pass**
- F757 BitwiseOperatorTests must continue to pass unchanged
- Verifies no regression in simple `VAR & mask` truthiness patterns
- Constraint: C2

**AC#11: Tools build with zero warnings**
- TreatWarningsAsErrors is enforced (Directory.Build.props)
- All three tool projects must compile cleanly
- Constraint: Technical Constraint #8

**AC#12: No technical debt markers in BitwiseComparisonCondition.cs**
- Test: Grep pattern=`TODO|FIXME|HACK` (Python regex alternation) path=src/tools/dotnet/ErbParser/BitwiseComparisonCondition.cs
- Expected: 0 matches
- Scope: Only the new file (BitwiseComparisonCondition.cs). Modified files (LogicalOperatorParser.cs, DatalistConverter.cs, KojoBranchesParser.cs) may have pre-existing TODO comments; checking those would produce false positives.

**AC#13: DRAFT feature-766.md exists**
- Test: Glob pattern=pm/features/feature-766.md
- Expected: file exists
- Verifies handoff task T9 created the DRAFT feature for paren-stripping guard refinement

**AC#14: DRAFT feature-767.md exists**
- Test: Glob pattern=pm/features/feature-767.md
- Expected: file exists
- Verifies handoff task T10 created the DRAFT feature for dialogue-schema.json documentation update

**AC#15: F766 registered in index-features.md**
- Test: Grep pattern=`766` path=pm/index-features.md
- Expected: contains (F766 row in Active Features table)
- Verifies handoff task T9 registered the DRAFT feature in the index

**AC#16: F767 registered in index-features.md**
- Test: Grep pattern=`767` path=pm/index-features.md
- Expected: contains (F767 row in Active Features table)
- Verifies handoff task T10 registered the DRAFT feature in the index

### Goal Coverage Verification

| Goal Item | AC Coverage |
|-----------|-------------|
| (1) detect `(expr) op value` patterns in ParseAtomicCondition | AC#3, AC#7 |
| (2) introduce a new ICondition type | AC#1, AC#2 |
| (3) convert to a new YAML operator | AC#4, AC#8 |
| (4) evaluate the compound expression in KojoBranchesParser | AC#5, AC#9 |
| without regressing the 26+ existing truthiness-only bitwise patterns | AC#6, AC#10 |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Implement compound bitwise-comparison parsing as a three-layer enhancement to the existing parser pipeline:

1. **Detection Layer** (LogicalOperatorParser.cs): Add pattern detection in `ParseAtomicCondition` AFTER the paren-stripping guard (line 169) and BEFORE prefix-anchored parsers. Detect the pattern `(inner_expr) comparison_op value` by:
   - Check if input starts with `(`
   - Find matching closing `)`
   - Check if remainder matches comparison operator pattern (`==`, `!=`, `>`, `>=`, `<`, `<=`) followed by whitespace and a value
   - Extract three parts: inner expression (between parens), comparison operator, and comparison value

2. **AST Representation Layer** (BitwiseComparisonCondition.cs): Create new ICondition type that represents the two-stage evaluation semantics:
   - `Inner` (ICondition): The bitwise expression (e.g., `TALENT:性別嗜好 & 1`), parsed by recursively calling `ParseAtomicCondition` after stripping outer parens
   - `ComparisonOp` (string): The comparison operator (`==`, `!=`, etc.)
   - `ComparisonValue` (string): The expected value after bitwise operation

3. **YAML Conversion Layer** (DatalistConverter.cs): Introduce new YAML operator `bitwise_and_cmp` that embeds both mask and comparison semantics. The inner ICondition must be a VariableRef or TalentRef with `Operator="&"`. Extract the mask from `Inner.Value` and the comparison from `ComparisonOp/ComparisonValue`. YAML structure:
   ```yaml
   TALENT:
     "16":
       bitwise_and_cmp:
         mask: "3"
         op: "eq"  # Normalized from "=="
         value: "3"
   ```

4. **Evaluation Layer** (KojoBranchesParser.cs): Add two-stage evaluation in operator switch:
   - Parse the `bitwise_and_cmp` operator dictionary to extract `mask`, `op`, and `value`
   - Stage 1: Compute `stateValue & mask`
   - Stage 2: Apply comparison operator to intermediate result and expected value
   - Reuse existing `EvaluateOperator` for comparison (map `op` to existing operator strings)

**Rationale**: This approach satisfies the Goal's four requirements while maintaining backward compatibility. The detection happens at the correct point in the parsing flow (after paren-stripping, before prefix parsers), the AST captures the two-stage semantics explicitly, the YAML representation is distinct from truthiness-only `bitwise_and`, and the evaluation is a clean two-stage computation.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `BitwiseComparisonCondition.cs` with properties: Inner (ICondition), ComparisonOp (string), ComparisonValue (string). Implements ICondition interface |
| 2 | Add `[JsonDerivedType(typeof(BitwiseComparisonCondition), typeDiscriminator: "bitwise_comparison")]` to ICondition.cs attribute list |
| 3 | Add detection logic in `ParseAtomicCondition` (after line 173): Check for `(` prefix, find closing `)`, match comparison operator regex, parse inner expression recursively after stripping parens, construct BitwiseComparisonCondition |
| 4 | Add `case BitwiseComparisonCondition bitwiseComp:` in ConvertConditionToYaml switch (around line 266). Extract mask from `bitwiseComp.Inner.Value`, normalize comparison operator, build nested YAML dictionary |
| 5 | Add shared `EvaluateBitwiseAndCmp` helper method. Add `bitwise_and_cmp` handling in BOTH the TALENT inline evaluation block (lines 127-143) AND `EvaluateVariableCondition` (lines 315-330), calling the shared helper before the existing `int.TryParse` in each loop |
| 6 | No changes to existing test logic required (backward compatibility by design). Regression verification via CI |
| 7 | Create `BitwiseComparisonTests.cs` in ErbParser.Tests with 6 test cases: positive (named TALENT with `==`), positive (CFLAG with `!=`), positive (actual kojo pattern TALENT:2 with `==`), negative (no comparison), negative (malformed inner expression), negative (inner without & operator) |
| 8 | Create `BitwiseComparisonConversionTests.cs` in ErbToYaml.Tests with 3 test cases: verify YAML structure contains `bitwise_and_cmp` key, verify mask/op/value are correctly extracted, negative (unsupported inner type returns null) |
| 9 | Create `BitwiseComparisonEvaluationTests.cs` in KojoComparer.Tests with 7 test cases: TALENT path true (7&3==3, 5&3==1), TALENT path false (6&3==3, 0&3==3), ne operator (7&3 ne 1), EvaluateVariableCondition path true (CFLAG 7&3==3), EvaluateVariableCondition path false (CFLAG 6&3==3) |
| 10 | No changes to BitwiseOperatorTests.cs required. Run existing test suite unchanged |
| 11 | Follow existing codebase patterns (no warnings). Use nullable annotations where appropriate |
| 12 | Complete implementation in first pass. No TODO/FIXME/HACK markers |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| AST representation | A) Extend VariableRef with additional comparison fields, B) Create new ICondition type, C) Nest conditions in LogicalOp | B | Separation of concerns. VariableRef represents atomic variable access with single operator. Compound bitwise-comparison is a distinct expression type with two-stage semantics. LogicalOp is for boolean composition only |
| Detection placement | A) Before paren-stripping guard, B) After paren-stripping guard but before prefix parsers, C) After prefix parsers | B | Constraint C5: Must come after paren-stripping guard (line 167) to avoid incorrect stripping of `(TALENT:x & 3) == 3`. Must come before prefix parsers because `(`-prefixed strings don't match prefix-anchored regexes |
| YAML operator naming | A) Reuse `bitwise_and` with additional field, B) New operator `bitwise_and_eq`, C) New operator `bitwise_and_cmp` | C | Constraint C4: `bitwise_and` = truthiness only. Distinct operator avoids semantic overload. `_cmp` suffix indicates general comparison (supports eq/ne/gt/gte/lt/lte via `op` field). Single operator with `op` field is the extensibility model — no need for separate `bitwise_and_ne`, `bitwise_and_gt`, etc. |
| Inner expression parsing | A) Write custom parser for bitwise expressions, B) Recursively call ParseAtomicCondition after stripping parens | B | Constraint C7: Reuse existing variable parsers. After stripping outer parens, `TALENT:x & 3` is a valid atomic condition already handled by TalentConditionParser with `&` operator support (F757). Reduces code duplication and ensures consistency |
| Comparison operator normalization | A) Store ERB operator as-is (`==`), B) Normalize to YAML operator format (`eq`) in AST, C) Normalize during YAML conversion | C | Keep AST close to source representation (principle: AST preserves original syntax). Normalization is a conversion concern, not a parsing concern. Extract shared `NormalizeErbOperator` helper from existing `MapErbOperatorToYaml` mapping to avoid duplication |
| Evaluation strategy | A) New dedicated method EvaluateBitwiseComparison, B) Extend EvaluateOperator switch with nested logic, C) Two-stage: compute intermediate value, then call existing EvaluateOperator | C | Reuses existing comparison logic. Clean separation: bitwise computation is new, comparison is existing. Reduces duplication and maintains consistency with other operator evaluations |
| Property pattern | A) `{ get; set; } = null!` (match NegatedCondition), B) `required init` properties | B | NegatedCondition uses `null!` (pre-existing pattern, not ideal). BitwiseComparisonCondition uses `required init` to enforce compile-time initialization safety. Divergence from NegatedCondition is intentional: new types should use the safer pattern. NegatedCondition alignment is pre-existing debt, not F759 scope |

### Interfaces / Data Structures

#### BitwiseComparisonCondition (new file)

```csharp
using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a compound bitwise-comparison condition: (VAR & mask) op value
/// Example: (TALENT:性別嗜好 & 3) == 3
/// F759: Two-stage evaluation: (1) compute VAR & mask, (2) compare result with value
/// </summary>
public class BitwiseComparisonCondition : ICondition
{
    /// <summary>
    /// Inner bitwise expression (must have Operator="&")
    /// Example: TALENT:性別嗜好 & 3 → TalentRef with Name="性別嗜好", Operator="&", Value="3"
    /// </summary>
    [JsonPropertyName("inner")]
    public required ICondition Inner { get; init; }

    /// <summary>
    /// Comparison operator applied to bitwise result
    /// Valid values: "==", "!=", ">", ">=", "<", "<="
    /// </summary>
    [JsonPropertyName("comparison_op")]
    public required string ComparisonOp { get; init; }

    /// <summary>
    /// Expected value after bitwise operation
    /// Example: For (TALENT:性別嗜好 & 3) == 3, this is "3"
    /// </summary>
    [JsonPropertyName("comparison_value")]
    public required string ComparisonValue { get; init; }
}
```

#### YAML Schema Extension

**Existing** (`bitwise_and` for truthiness):
```yaml
TALENT:
  "16":
    bitwise_and: "3"  # (TALENT:16 & 3) != 0
```

**New** (`bitwise_and_cmp` for compound comparison):
```yaml
TALENT:
  "16":
    bitwise_and_cmp:
      mask: "3"       # Bitwise mask
      op: "eq"        # Comparison operator (normalized from "==")
      value: "3"      # Expected value after bitwise operation
```

**Design rationale**: Separate operator avoids overloading `bitwise_and` semantics. `_cmp` suffix indicates general comparison (not equality-only). Nested dictionary structure captures all three parameters explicitly. The `op` field supports eq/ne/gt/gte/lt/lte, providing extensibility within a single operator key.

#### LogicalOperatorParser Changes (pseudocode)

Insert detection logic after line 173 (after paren-stripping guard, before prefix parsers):

```csharp
// After line 173 (end of paren-stripping guard)

// Check for compound bitwise-comparison pattern: (expr) op value
if (condition.StartsWith("("))
{
    var closingParenIndex = FindMatchingClosingParen(condition, 0);
    if (closingParenIndex > 0 && closingParenIndex < condition.Length - 1)
    {
        var remainder = condition.Substring(closingParenIndex + 1).Trim();
        // Match comparison operators: ==, !=, >=, <=, >, <
        // Order matters: check >= before >, <= before < to avoid partial matches
        var comparisonMatch = Regex.Match(remainder, @"^(==|!=|>=|<=|>|<)\s+(.+)$");
        if (comparisonMatch.Success)
        {
            var innerExpr = condition.Substring(1, closingParenIndex - 1).Trim();
            var comparisonOp = comparisonMatch.Groups[1].Value;
            var comparisonValue = comparisonMatch.Groups[2].Value.Trim();

            // Parse inner expression recursively (handles TALENT:x & mask)
            var innerCondition = ParseAtomicCondition(innerExpr);
            // Parse-time validation: inner must be a variable ref with & operator
            // This prevents constructing BitwiseComparisonCondition with non-bitwise inner
            // (e.g., "(TALENT:x == 3) == 5" would parse inner as TalentRef with Operator="==", not "&")
            if (innerCondition != null && HasBitwiseOperator(innerCondition))
            {
                return new BitwiseComparisonCondition
                {
                    Inner = innerCondition,
                    ComparisonOp = comparisonOp,
                    ComparisonValue = comparisonValue
                };
            }
        }
    }
}

// Continue to line 173+ (Try TALENT parser, etc.)
```

**HasBitwiseOperator helper**:
```csharp
/// Returns true if condition is a variable reference with Operator="&"
private static bool HasBitwiseOperator(ICondition condition) => condition switch
{
    TalentRef t => t.Operator == "&",
    VariableRef v => v.Operator == "&",
    _ => false
};
```

**FindMatchingClosingParen helper**:
```csharp
/// <summary>
/// Finds the matching closing parenthesis for the opening paren at startIndex.
/// Uses depth counter consistent with SplitOnOperator's paren-tracking approach.
/// Returns -1 if no matching paren found.
/// </summary>
private static int FindMatchingClosingParen(string input, int startIndex)
{
    var depth = 0;
    for (var i = startIndex; i < input.Length; i++)
    {
        if (input[i] == '(') depth++;
        else if (input[i] == ')') depth--;
        if (depth == 0) return i;
    }
    return -1;
}
```

**Edge case handling**:
- FindMatchingClosingParen handles nested parens via paren-depth counter
- If inner expression fails to parse (returns null) or lacks & operator, fall through to return null at end of method
- Regex uses `^` anchor to ensure comparison operator is at start of remainder (not somewhere in middle)
- Groups[2] captures the value including any trailing content (trimmed)
- Non-bitwise inner expressions (e.g., `(TALENT:x == 3) == 5`) are rejected at parse time, not deferred to DatalistConverter

#### DatalistConverter Changes (pseudocode)

Add case in ConvertConditionToYaml switch (around line 266):

```csharp
case BitwiseComparisonCondition bitwiseComp:
    return ConvertBitwiseComparisonCondition(bitwiseComp);
```

New helper method:

```csharp
/// <summary>
/// Convert BitwiseComparisonCondition to YAML format with bitwise_and_cmp operator.
/// Delegates variable key resolution to existing ConvertConditionToYaml infrastructure,
/// then wraps the result with bitwise_and_cmp operator dictionary.
/// F759: Inner must have Operator="&" (validated at parse time by HasBitwiseOperator)
/// </summary>
private Dictionary<string, object>? ConvertBitwiseComparisonCondition(BitwiseComparisonCondition bitwiseComp)
{
    // Step 1: Resolve variable type and key using existing conversion infrastructure
    // The inner condition (e.g., TalentRef with &) already knows how to produce its YAML key
    var (variableType, variableKey, mask) = ResolveInnerBitwiseRef(bitwiseComp.Inner);
    if (variableType == null)
        return null;

    // Step 2: Normalize comparison operator (== → eq, != → ne, etc.)
    // Uses shared NormalizeErbOperator helper (also used by refactored MapErbOperatorToYaml)
    var normalizedOp = NormalizeErbOperator(bitwiseComp.ComparisonOp);

    // Step 3: Apply DIM CONST resolution to both mask and value
    var resolvedMask = _dimConstResolver?.ResolveToString(mask) ?? mask;
    var resolvedValue = _dimConstResolver?.ResolveToString(bitwiseComp.ComparisonValue)
                        ?? bitwiseComp.ComparisonValue;

    // Step 4: Build YAML dictionary (single construction, no duplication)
    return new Dictionary<string, object>
    {
        { variableType, new Dictionary<string, object>
            {
                { variableKey, new Dictionary<string, object>
                    {
                        { "bitwise_and_cmp", new Dictionary<string, object>
                            {
                                { "mask", resolvedMask },
                                { "op", normalizedOp },
                                { "value", resolvedValue }
                            }
                        }
                    }
                }
            }
        }
    };
}

/// <summary>
/// Extract variable type, key, and mask from inner bitwise condition.
/// Reuses existing key resolution (TalentCsvLoader, BuildVariableKey).
/// </summary>
private (string? variableType, string variableKey, string mask) ResolveInnerBitwiseRef(ICondition inner)
{
    switch (inner)
    {
        case TalentRef talent when talent.Operator == "&":
            var talentIndex = _talentLoader.GetTalentIndex(talent.Name);
            if (talentIndex == null)
            {
                Console.Error.WriteLine($"Warning: Talent '{talent.Name}' not found in Talent.csv");
                return (null, "", "");
            }
            return ("TALENT", talentIndex.Value.ToString(), talent.Value ?? "0");

        case VariableRef varRef when varRef.Operator == "&"
                                  && _variableTypePrefixes.ContainsKey(varRef.GetType()):
            return (_variableTypePrefixes[varRef.GetType()],
                    BuildVariableKey(varRef),
                    varRef.Value ?? "0");

        default:
            Console.Error.WriteLine($"Warning: Unsupported inner condition type for compound bitwise: {inner.GetType().Name}");
            return (null, "", "");
    }
}

/// <summary>
/// Normalize ERB comparison operator to YAML format.
/// Extracted as shared helper to avoid duplication with MapErbOperatorToYaml (which contains
/// identical switch mapping). MapErbOperatorToYaml should be refactored to call this helper
/// internally, wrapping the result in Dictionary. Both ConvertBitwiseComparisonCondition and
/// MapErbOperatorToYaml then share a single source of truth for operator normalization.
/// </summary>
private static string NormalizeErbOperator(string erbOp) => erbOp switch
{
    "==" => "eq",
    "!=" => "ne",
    ">" => "gt",
    ">=" => "gte",
    "<" => "lt",
    "<=" => "lte",
    _ => throw new ArgumentException($"Unknown ERB operator: {erbOp}")
};

// MapErbOperatorToYaml refactored to use shared NormalizeErbOperator:
// private Dictionary<string, object> MapErbOperatorToYaml(string? erbOp, string? value)
// {
//     string op = erbOp ?? "!=";
//     string val = _dimConstResolver?.ResolveToString(value ?? "0") ?? (value ?? "0");
//     return new Dictionary<string, object> { { NormalizeErbOperator(op), val } };
// }
```

#### KojoBranchesParser Changes (pseudocode)

**CRITICAL**: TALENT conditions route through a separate inline evaluation block (lines 114-146) that does NOT call `EvaluateVariableCondition`. Since the primary target `(TALENT:性別嗜好 & 3) == 3` produces YAML under the `TALENT` key, `bitwise_and_cmp` handling MUST be added to the TALENT inline block. The same handling is also needed in `EvaluateVariableCondition` for non-TALENT variable types (CFLAG, TCVAR, etc.).

**Shared helper methods** (new, to eliminate loop duplication across TALENT inline and EvaluateVariableCondition):

```csharp
/// <summary>
/// Evaluates a bitwise_and_cmp operator: (stateValue & mask) op expectedValue
/// Returns true if condition matches, false otherwise.
/// Returns null if the operator value is not a valid bitwise_and_cmp dictionary.
/// </summary>
private static bool? EvaluateBitwiseAndCmp(int stateValue, object? opValue)
{
    if (opValue is not Dictionary<object, object> bitwiseDict)
        return null;

    var maskStr = bitwiseDict.GetValueOrDefault("mask", "0")?.ToString() ?? "0";
    var compOp = bitwiseDict.GetValueOrDefault("op", "eq")?.ToString() ?? "eq";
    var valueStr = bitwiseDict.GetValueOrDefault("value", "0")?.ToString() ?? "0";

    if (!int.TryParse(maskStr, out var mask) || !int.TryParse(valueStr, out var expectedValue))
        return null;

    // Two-stage evaluation
    var bitwiseResult = stateValue & mask;
    return EvaluateOperator(bitwiseResult, compOp, expectedValue);
}

/// <summary>
/// Evaluates all operators in an opDict against a state value.
/// Shared between TALENT inline block and EvaluateVariableCondition to eliminate loop duplication.
/// Handles both simple operators (eq/ne/gt/etc.) and compound bitwise_and_cmp.
/// Returns false if any operator condition fails (AND logic).
/// </summary>
private static bool EvaluateOpDict(int stateValue, Dictionary<object, object> opDict)
{
    foreach (var opKvp in opDict)
    {
        var op = opKvp.Key?.ToString();
        if (string.IsNullOrEmpty(op))
            continue;

        // Handle compound bitwise comparison (F759)
        if (op == "bitwise_and_cmp")
        {
            var bitwiseResult = EvaluateBitwiseAndCmp(stateValue, opKvp.Value);
            if (bitwiseResult != true)
                return false; // Reject both false (condition failed) and null (malformed dictionary)
            continue; // Skip int.TryParse for dictionary values
        }

        // Simple operator handling (includes existing bitwise_and truthiness operator:
        // bitwise_and value is a plain int string e.g. "3", so int.TryParse succeeds,
        // and EvaluateOperator handles "bitwise_and" at line 289 with (stateValue & expected) != 0.
        // AC#6/AC#10 regression tests verify this flow is preserved.)
        if (!int.TryParse(opKvp.Value?.ToString(), out var expected))
            continue;
        if (!EvaluateOperator(stateValue, op, expected))
            return false;
    }
    return true;
}
```

**TALENT inline block change** (lines 127-143, replace loop body with shared helper):

```csharp
// Inside TALENT evaluation block
if (kvp.Value is Dictionary<object, object> opDict)
{
    if (!EvaluateOpDict(stateValue, opDict))
        return false;
}
```

**EvaluateVariableCondition change** (lines 315-330, replace loop body with shared helper):

```csharp
// Inside EvaluateVariableCondition
if (kvp.Value is Dictionary<object, object> opDict)
{
    if (!EvaluateOpDict(stateValue, opDict))
        return false;
}
```

**Rationale**: `EvaluateOpDict` encapsulates the entire operator evaluation loop (bitwise_and_cmp + simple operators), eliminating duplication at both the loop level and the bitwise evaluation level. Both TALENT inline and EvaluateVariableCondition call the same single method, ensuring consistency and reducing maintenance surface.

**Behavioral Equivalence Analysis**: The `EvaluateOpDict` refactoring preserves existing behavior for ALL non-`bitwise_and_cmp` operators. Trace: (1) For existing operators like `eq`, `ne`, `bitwise_and`, etc., the `op == "bitwise_and_cmp"` check at line 697 evaluates to `false`, falling through to the existing `int.TryParse` → `EvaluateOperator` path (lines 709-712), which is identical to the current inline loop code. (2) The existing `bitwise_and` truthiness operator has a plain integer string value (e.g., `"3"`), so `int.TryParse` succeeds and `EvaluateOperator` handles it at line 289 with `(stateValue & expected) != 0`. (3) The only behavioral difference is the NEW `bitwise_and_cmp` key: in the old code, its Dictionary value would fail `int.TryParse` and be silently skipped via `continue`; in the new code, it is explicitly handled by `EvaluateBitwiseAndCmp`. Since `bitwise_and_cmp` keys do not exist in any current YAML data, this difference has zero impact on existing behavior. Note the two distinct failure modes: non-`bitwise_and_cmp` keys with unparseable values (e.g., a Dictionary) fail `int.TryParse` and are silently skipped via `continue` (preserving existing behavior), while `bitwise_and_cmp` keys with malformed dictionaries cause `EvaluateBitwiseAndCmp` to return `null`, which `EvaluateOpDict` treats as `return false` (fail-closed for new operator only). AC#6 and AC#10 regression tests verify this equivalence end-to-end.

---

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ErbParser.Tests | `dotnet test tools/ErbParser.Tests` | 140 pass, 0 fail, 0 skip | Includes F757/F758 bitwise tests |
| ErbToYaml.Tests | `dotnet test tools/ErbToYaml.Tests` | 100 pass, 0 fail, 0 skip | Includes F758 conversion tests |
| KojoComparer.Tests | `dotnet test tools/KojoComparer.Tests --filter "FullyQualifiedName!~FileDiscoveryTests"` | 109 pass, 0 fail, 3 skip | FileDiscoveryTests excluded (pre-existing ErbRunner build failure) |
| Tool build | `dotnet build tools/ErbParser && dotnet build tools/ErbToYaml && dotnet build tools/KojoComparer` | 0 warnings | TreatWarningsAsErrors enforced |

**Baseline File**: `.tmp/baseline-759.txt`

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
- [resolved-applied] Phase2-Uncertain iter1: [CON-TYPE] BitwiseComparisonCondition.Inner typed as ICondition allows non-bitwise inner. Fix: Added parse-time validation in ParseAtomicCondition pseudocode (check inner has & operator before constructing BitwiseComparisonCondition) and added AC#7 negative test case for inner without & operator.
- [resolved-applied] Phase2-Uncertain iter2: [FMT-SEC] Missing Baseline Measurement section. Added with current baseline values (ErbParser 140, ErbToYaml 100, KojoComparer 109 pass).
- [resolved-applied] Phase2-Uncertain iter2: [COV-AC] Added `(TALENT:2 & 3) == 3` positive test case to AC#7. Verifies actual kojo pattern produces BitwiseComparisonCondition with Inner=TalentRef(Name="2"). No F760 dependency.
- [resolved-applied] Phase2-Uncertain iter4: [DOC-DESIGN] EvaluateOpDict design doesn't explicitly document that existing `bitwise_and` (truthiness) operator flow is preserved via the simple operator path. Fix: Added dedicated Behavioral Equivalence Analysis paragraph after EvaluateOpDict pseudocode with full trace analysis.
- [resolved-applied] Phase2-Uncertain iter5: [TDD-ORDER] Implementation Contract restructured to TDD RED→GREEN per pipeline stage. Phase 1: T1 AST type (no test needed), Phase 2-3: T5 RED → T2 GREEN (parser), Phase 4-5: T6 RED → T3 GREEN (conversion), Phase 6-7: T7 RED → T4 GREEN (evaluation).
- [resolved-applied] Phase2-Uncertain iter6: [REFACTOR-SCOPE] Covered by DOC-DESIGN fix: Behavioral Equivalence Analysis paragraph added after EvaluateOpDict pseudocode with full trace analysis of existing operator flow preservation.
- [resolved-skipped] Phase2-Uncertain iter7: [SEC-ORDER] Feature section ordering diverges from template (/fc workflow artifact). User decision: skip — low practical impact, reordering conflicts with fc-phase markers.
- [resolved-applied] Phase2-Valid iter8: [AC-COV] T9/T10 handoff tasks lacked ACs. Added AC#13 (feature-766.md exists) and AC#14 (feature-767.md exists) per DRAFT Creation Checklist. Updated T9 AC# to 13, T10 AC# to 14.
- [resolved-applied] Phase2-Valid iter8: [FMT-TBL] Implementation Contract table had extra TDD column (6 vs 5). Removed TDD column per template specification.
- [resolved-applied] Phase2-Uncertain iter8: [LINE-REF] Feature line number references updated to match actual source: guard 167→169, ParseAtomicCondition 162-208→164-210, TALENT inline 127-141→127-143, EvaluateVariableCondition 317-327→315-330, constraint #1 167→169.
- [resolved-applied] Phase2-Uncertain iter8: [PHIL-DEAD] Added AST-level completeness qualification to Philosophy Derivation row 1: 'including dead-code paths per C10'.
- [resolved-applied] Phase2-Valid iter9: [AC-IDX] AC#13/14 only verified DRAFT file existence but not index-features.md registration. Added AC#15 (index contains 766) and AC#16 (index contains 767) per DRAFT Creation Checklist. Updated T9/T10 AC# refs.
- [resolved-applied] Phase2-Valid iter9: [COUNT-AC] Success Criteria said '12 ACs' but feature has 16 ACs. Updated to '16 ACs'.
- [resolved-applied] Phase3-Valid iter10: [T8-NAME] T8 description 'Run full tool test suite and verify no regressions' didn't cover AC#12 (code quality check). Renamed to include 'validate code quality'.
- [resolved-applied] Phase3-Valid iter10: [DEBT-TRACK] NegatedCondition null! vs BitwiseComparisonCondition required init divergence. Added to Mandatory Handoffs under F767.
- [resolved-applied] Phase3-Valid iter10: [NEXT-NUM] No AC verified index Next Feature number update to 768. Added constraint #12 to Implementation Contract.
- [resolved-applied] Phase3-Valid iter10: [TDD-EXEMPT] Phase 1 (T1) POCO exemption from TDD RED phase undocumented. Added constraint #11.
- [resolved-applied] Phase3-Uncertain iter10: [DOC-EQUIV] Added explicit failure mode distinction to Behavioral Equivalence Analysis: continue for non-bitwise_and_cmp TryParse failure vs return false for bitwise_and_cmp malformed dictionary.
- [resolved-applied] Phase2-Valid iter1: [T10-DESC] T10 description omitted NegatedCondition scope from Mandatory Handoffs. Updated to include both dialogue-schema.json documentation and NegatedCondition property pattern alignment.
- [resolved-invalid] Phase2-Invalid iter1: [NORM-DUP] NormalizeErbOperator vs MapErbOperatorToYaml duplication. Invalid: different return types (string vs Dictionary), different scope (6 comparison ops vs 7+ including & and default), Technical Design documents optional refactoring as commented-out code.
- [resolved-skipped] Phase2-Uncertain iter1: [AC12-SCOPE] AC#12 checks TODO/FIXME/HACK only in BitwiseComparisonCondition.cs. New code in modified files (LogicalOperatorParser.cs, DatalistConverter.cs, KojoBranchesParser.cs) could contain debt markers undetected. AC Details justifies narrow scope (pre-existing markers cause false positives). User decision: skip — AC#11 (TreatWarningsAsErrors) and Implementation Contract provide multi-layer defense.

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,2 | Create BitwiseComparisonCondition.cs ICondition type and register in ICondition.cs | | [x] |
| 2 | 3,7 | Add compound bitwise pattern detection in LogicalOperatorParser.ParseAtomicCondition | | [x] |
| 3 | 4,8 | Add BitwiseComparisonCondition conversion case in DatalistConverter | | [x] |
| 4 | 5,9 | Add bitwise_and_cmp evaluation in KojoBranchesParser | | [x] |
| 5 | 7 | Create parser unit tests (BitwiseComparisonTests.cs) | | [x] |
| 6 | 8 | Create conversion unit tests (BitwiseComparisonConversionTests.cs) | | [x] |
| 7 | 9 | Create evaluation unit tests (BitwiseComparisonEvaluationTests.cs) | | [x] |
| 8 | 6,10,11,12 | Run full tool test suite, verify no regressions, and validate code quality | | [x] |
| 9 | 13,15 | Create DRAFT feature-766.md for paren-stripping guard refinement (FindMatchingClosingParen at LogicalOperatorParser.cs:169) | handoff | [x] |
| 10 | 14,16 | Create DRAFT feature-767.md for dialogue-schema.json `bitwise_and_cmp` documentation update and NegatedCondition property pattern alignment | handoff | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | AC#1 Details + Technical Design (T1) | BitwiseComparisonCondition.cs, ICondition.cs registration |
| 2 | implementer | sonnet | AC#7 Details (T5) | BitwiseComparisonTests.cs (RED: parser tests) |
| 3 | implementer | sonnet | Technical Design LogicalOperatorParser (T2) | LogicalOperatorParser.cs detection logic |
| 4 | implementer | sonnet | AC#8 Details (T6) | BitwiseComparisonConversionTests.cs (RED: conversion tests) |
| 5 | implementer | sonnet | Technical Design DatalistConverter (T3) | DatalistConverter.cs conversion case |
| 6 | implementer | sonnet | AC#9 Details (T7) | BitwiseComparisonEvaluationTests.cs (RED: evaluation tests) |
| 7 | implementer | sonnet | Technical Design KojoBranchesParser (T4) | KojoBranchesParser.cs evaluation logic |
| 8 | ac-tester | haiku | AC#6,10,11,12 commands (T8) | Test results, build verification |
| 9 | finalizer | haiku | Mandatory Handoffs table (T9-T10) | DRAFT feature-766.md, DRAFT feature-767.md (AC#13-16) |

**Constraints** (from Technical Design):

1. Detection logic must be placed AFTER paren-stripping guard (line 169 in LogicalOperatorParser.cs) and BEFORE prefix-anchored parsers to avoid incorrect parsing
2. Inner expression parsing must recursively call ParseAtomicCondition to reuse existing variable parsers (Constraint C7)
3. JsonDerivedType registration required in ICondition.cs for polymorphic JSON serialization
4. YAML operator `bitwise_and_cmp` must be distinct from truthiness-only `bitwise_and` (Constraint C4)
5. Inner condition must have Operator="&" (validated at parse time by HasBitwiseOperator in ParseAtomicCondition; DatalistConverter also validates defensively)
6. Comparison operator normalization happens during YAML conversion, not in AST (Decision: Comparison operator normalization)
7. Two-stage evaluation: (1) compute `stateValue & mask`, (2) compare result with expected value using existing EvaluateOperator
8. TreatWarningsAsErrors=true enforced (Directory.Build.props) - all code must compile cleanly
9. Test with named variables (e.g., 性別嗜好) to avoid F760 dependency on TALENT:2 numeric index parsing (Constraint C3)
10. FindMatchingClosingParen must handle nested parens correctly (edge case handling)
11. Phase 1 (T1) creates BitwiseComparisonCondition.cs as a data-only class (POCO). No TDD RED phase needed — AC#1 (file exists) and AC#2 (registration) provide sufficient verification
12. Phase 9 (T9-T10) must update `Next Feature number` in index-features.md to 768 after creating F766 and F767 DRAFTs

**Pre-conditions**:

- F758 (prefix-based variable types) is [DONE] - all 13 variable types registered
- F757 (simple bitwise `&` operator) is [DONE] - truthiness evaluation working
- tools/ErbParser, tools/ErbToYaml, tools/KojoComparer projects build successfully
- All existing tool tests pass (baseline for regression detection)

**Success Criteria**:

1. All 16 ACs pass (file existence, code patterns, tests, build, no tech debt, handoff DRAFTs)
2. BitwiseComparisonCondition successfully parses `(TALENT:性別嗜好 & 3) == 3` pattern
3. YAML conversion produces `bitwise_and_cmp` operator with mask/op/value structure
4. Two-stage evaluation correctly computes `(stateValue & mask) op expectedValue`
5. Zero regressions in existing tool tests (26+ truthiness patterns continue to work)
6. `dotnet build tools/ErbParser tools/ErbToYaml tools/KojoComparer` succeeds with zero warnings

**Rollback Plan**:

If issues arise after implementation:

1. Revert commit with `git revert <commit-hash>`
2. Notify user of rollback with description of issue
3. Create follow-up feature for fix with additional investigation into:
   - Edge cases not covered by initial test suite
   - Interaction with other condition types
   - YAML schema ambiguities
4. Document discovered issues in new feature's Background section

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Paren-stripping guard uses naive StartsWith/EndsWith, vulnerable to `(VAR & mask) == (OTHER)` patterns | Out of scope: guard refinement requires FindMatchingClosingParen integration at line 169 | Feature | F766 | T9 |
| dialogue-schema.json documentation update for `bitwise_and_cmp` operator | Non-blocking: schema uses permissive `type: object` but documentation should reflect new operator | Feature | F767 | T10 |
| NegatedCondition uses `{ get; set; } = null!` pattern while new BitwiseComparisonCondition uses `required init` | Pre-existing debt: NegatedCondition should be aligned to safer `required init` pattern | Feature | F767 | T10 |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|-------|-------|--------|--------|
| 2026-02-08 13:24 | START | implementer | Task 1 | - |
| 2026-02-08 13:24 | END | implementer | Task 1 | SUCCESS |
| 2026-02-08 13:26 | START | implementer | Task 5 | - |
| 2026-02-08 13:26 | END | implementer | Task 5 | SUCCESS (TDD RED: 3 fail, 3 pass) |
| 2026-02-08 13:28 | START | implementer | Task 2 | - |
| 2026-02-08 13:28 | END | implementer | Task 2 | SUCCESS |
| 2026-02-08 13:31 | START | implementer | Task 6 | - |
| 2026-02-08 13:31 | END | implementer | Task 6 | SUCCESS (TDD RED: 2 fail, 1 pass) |
| 2026-02-08 13:32 | START | implementer | Task 3 | - |
| 2026-02-08 13:32 | END | implementer | Task 3 | SUCCESS (TDD GREEN: all conversion tests PASS, zero warnings) |
| 2026-02-08 13:34 | START | implementer | Task 7 | - |
| 2026-02-08 13:34 | END | implementer | Task 7 | SUCCESS (TDD RED: 3 fail, 4 pass) |
| 2026-02-08 13:38 | START | implementer | Task 4 | - |
| 2026-02-08 13:38 | END | implementer | Task 4 | SUCCESS (TDD GREEN: all 7 evaluation tests PASS, zero warnings) |
| 2026-02-08 13:42 | START | orchestrator | Task 8 | - |
| 2026-02-08 13:42 | END | orchestrator | Task 8 | SUCCESS (ErbParser 146 pass, ErbToYaml 103 pass, KojoComparer 111 pass/3 skip, 0 warnings, 0 debt markers) |
| 2026-02-08 13:44 | START | orchestrator | Task 9 | - |
| 2026-02-08 13:44 | END | orchestrator | Task 9 | SUCCESS (feature-766.md [DRAFT] created, index updated) |
| 2026-02-08 13:44 | START | orchestrator | Task 10 | - |
| 2026-02-08 13:44 | END | orchestrator | Task 10 | SUCCESS (feature-767.md [DRAFT] created, index updated, Next Feature number → 768) |

---

## Links
- [feature-758.md](feature-758.md) - Parent feature (prefix-based variable type expansion)
- [feature-757.md](archive/feature-757.md) - Foundation (bitwise & operator support)
- [feature-760.md](feature-760.md) - TALENT:2 numeric index overlap
- [feature-761.md](feature-761.md) - LOCAL:1 in same condition
- [feature-762.md](feature-762.md) - Sibling (no overlap)
- [feature-706.md](feature-706.md) - Downstream consumer
