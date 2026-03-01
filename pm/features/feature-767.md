# Feature 767: dialogue-schema.json Documentation Update and NegatedCondition Property Pattern Alignment

## Status: [DONE]
<!-- fl-reviewed: 2026-02-10T00:00:00Z -->

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

## Type: infra

## Review Context

### Origin

| Field | Value |
|-------|-------|
| Parent Feature | F759 (Compound Bitwise Condition Parsing) |
| Discovery Point | Technical Constraint #9 / Mandatory Handoffs |
| Timestamp | 2026-02-08 |

### Identified Gap

Two items bundled as they are both non-blocking documentation/alignment work from F759:

1. **dialogue-schema.json documentation**: The schema uses permissive `type: object` for conditionElement which already accepts `bitwise_and_cmp` nested dictionary structure. However, the documentation should explicitly describe the new operator for developer reference.

2. **NegatedCondition property pattern alignment**: NegatedCondition uses `{ get; set; } = null!` pattern while the new BitwiseComparisonCondition uses the safer `required init` pattern. NegatedCondition should be aligned to `required init` for consistency and compile-time safety.

### Review Evidence

| Field | Value |
|-------|-------|
| Gap Source | F759 Technical Constraints #9 and Key Decisions (Property pattern) |
| Derived Task | Update schema docs + align NegatedCondition property pattern |
| Comparison Result | Non-blocking but improves documentation and code consistency |
| DEFER Reason | Neither item affects runtime behavior or test results |

### Files Involved

| File | Relevance |
|------|-----------|
| src/tools/dotnet/YamlSchemaGen/dialogue-schema.json | Schema documentation update for bitwise_and_cmp |
| src/tools/dotnet/ErbParser/NegatedCondition.cs | Property pattern alignment to required init |

### Parent Review Observations

F759 intentionally chose `required init` over `null!` for BitwiseComparisonCondition as the safer pattern. The divergence with NegatedCondition is pre-existing technical debt.

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)
Eliminate property pattern inconsistency and documentation gaps introduced during iterative parser evolution (F757-F762), ensuring compile-time safety and developer discoverability.

### Problem (Current Issue)
The ErbParser codebase evolved iteratively through F757-F762, with each feature addressing immediate parsing needs without retroactively aligning pre-existing types to newer patterns. Because F759 intentionally chose `required init` as the safer property pattern for BitwiseComparisonCondition (`BitwiseComparisonCondition.cs:17`) but correctly scoped retroactive alignment out of its implementation, NegatedCondition (`NegatedCondition.cs:8`) retains the `{ get; set; } = null!` anti-pattern -- the only ICondition type using `null!`, since all other types use either nullable properties (`?`) or value-type defaults (`string.Empty`). The `null!` pattern suppresses nullable reference warnings at declaration time but provides no compile-time enforcement that `Inner` is actually set during construction.

Separately, dialogue-schema.json has diverged from its generator (`src/tools/dotnet/YamlSchemaGen/Program.cs`) into a hand-maintained artifact. The committed schema uses `entries` with compound conditions (AND/OR/NOT) while Program.cs generates `branches` with flat conditions. Due to this divergence and the schema's permissive `type: object` for conditionElement values, the `bitwise_and_cmp` operator introduced in F759 is implicitly accepted but has no explicit documentation for developers.

### Goal (What to Achieve)
1. Align NegatedCondition's `Inner` property from `{ get; set; } = null!` to `public required ICondition Inner { get; init; }`, matching the BitwiseComparisonCondition pattern for compile-time safety.
2. Add explicit `bitwise_and_cmp` operator documentation to the `conditionElement` definitions in `src/tools/dotnet/YamlSchemaGen/dialogue-schema.json`.
3. Add explicit `bitwise_and` (truthiness) operator documentation to the `conditionElement` definitions in `src/tools/dotnet/YamlSchemaGen/dialogue-schema.json`.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does NegatedCondition use `null!` while BitwiseComparisonCondition uses `required init`? | F759 intentionally chose `required init` for new types but did not retroactively update NegatedCondition | NegatedCondition.cs:8, BitwiseComparisonCondition.cs:17 |
| 2 | Why wasn't NegatedCondition updated in F759? | NegatedCondition is the only pre-existing ICondition type with a non-nullable reference property requiring initialization | Investigation consensus (3/3) |
| 3 | Why does NegatedCondition use `null!` specifically? | The `null!` pattern suppresses nullable warnings at declaration but provides no compile-time enforcement | NegatedCondition.cs:8 |
| 4 | Why was compile-time enforcement not used originally? | The ErbParser codebase evolved iteratively through F757-F762, each feature addressing immediate parsing needs | feature-757.md through feature-762.md |
| 5 | Why (Root) | No retroactive pattern alignment was applied across existing types during iterative evolution | F759 Key Decisions, F759 Mandatory Handoffs |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | NegatedCondition uses `null!`; schema lacks `bitwise_and_cmp` docs | Iterative evolution without retroactive alignment; hand-maintained schema with permissive `type: object` |
| Where | NegatedCondition.cs:8; dialogue-schema.json conditionElement | ErbParser codebase F757-F762 evolution; schema-generator divergence |
| Fix | Change property pattern; add schema entry | Align to `required init` pattern; document operator in schema |

## Related Features

| Feature | Status | Relationship |
|---------|--------|--------------|
| F759 | [DONE] | Parent: compound bitwise condition parsing, created F767 as handoff |
| F758 | [DONE] | Grandparent: prefix-based variable type expansion |
| F757 | [DONE] | Foundation: bitwise `&` operator support |
| F768 | [PROPOSED] | Sibling: cross-parser refactoring (also deferred from F759/F760 line) |
| F706 | [WIP] | Downstream: full equivalence verification |
| F754 | [DRAFT] | Related: YAML format unification (branches vs entries overlap) |

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Property pattern change scope | FEASIBLE | Single property declaration in NegatedCondition.cs:8 |
| Consumer code compatibility | FEASIBLE | Object initializer syntax works with both `set` and `init` (C# spec) |
| Precedent exists | FEASIBLE | BitwiseComparisonCondition.cs:17 uses identical `required init` pattern |
| Schema edit scope | FEASIBLE | One JSON documentation addition; permissive `type: object` unchanged |
| Test impact | FEASIBLE | No test changes needed; existing tests compatible |

**Verdict**: FEASIBLE

## Impact Analysis

| Area | Impact | Description |
|------|:------:|-------------|
| `src/tools/dotnet/ErbParser/NegatedCondition.cs` | HIGH | Property pattern change from `{ get; set; } = null!` to `required init` |
| `src/tools/dotnet/YamlSchemaGen/dialogue-schema.json` | HIGH | Add `bitwise_and_cmp` documentation to conditionElement |
| `src/tools/dotnet/ErbParser/LogicalOperatorParser.cs:216` | LOW | Object initializer syntax compatible with `init` |
| `src/tools/dotnet/ErbParser.Tests/LogicalOperatorParserTests.cs:330-332` | LOW | Object initializer syntax compatible with `init` |
| `src/tools/dotnet/ErbToYaml/ConditionSerializer.cs:55-56` | LOW | BitwiseComparisonCondition dispatch |
| `src/tools/dotnet/ErbToYaml/ConditionSerializer.cs:84` | LOW | YAML output reference for `bitwise_and` truthiness operator (schema documentation source) |
| `src/tools/dotnet/ErbToYaml/ConditionSerializer.cs:300-304` | LOW | YAML output reference for `bitwise_and_cmp` operator with `mask`/`op`/`value` fields (schema documentation source) |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| C1: `required init` supports object initializer syntax | C# language spec | No consumer code changes needed |
| C2: JSON deserialization proven with `required init` | BitwiseComparisonCondition precedent | Low risk |
| C3: dialogue-schema.json is hand-maintained, NOT generator output | Investigation consensus (3/3) | Direct JSON edit only, do NOT regenerate |
| C4: Program.cs `additionalProperties: false` would break current schema | Investigation evidence | Do NOT run generator |
| C5: TreatWarningsAsErrors enforced | Directory.Build.props | Build must succeed with zero warnings |
| C6: NegatedCondition is the ONLY `null!` ICondition type | Investigation consensus (3/3) | Scope alignment to NegatedCondition only |
| C7: Schema uses permissive `type: object` for variable type values | Schema evidence | Adding `bitwise_and_cmp` is informational documentation |
| C8: Schema missing 10+ variable types (EQUIP, ITEM, STAIN, etc.) | Out-of-scope | Do not address in F767 |
| C9: Schema validation tests reference committed schema | SchemaValidationTests.cs | Must pass after edit |
| C10: `bitwise_and` (truthiness) also undocumented | Investigation 2, 3 | Consider documenting both operators |

### Constraint Details

**C1: `required init` supports object initializer syntax**
- **Source**: C# language spec
- **Verification**: Confirm object initializer syntax `new NegatedCondition { Inner = ... }` compiles with `init` accessor
- **AC Impact**: No consumer code changes needed

**C2: JSON deserialization proven with `required init`**
- **Source**: BitwiseComparisonCondition precedent
- **Verification**: BitwiseComparisonCondition.cs uses `required init` and deserialization works in existing tests
- **AC Impact**: Low risk

**C3: dialogue-schema.json is hand-maintained, NOT generator output**
- **Source**: Investigation consensus (3/3)
- **Verification**: Verify dialogue-schema.json is NOT generated by running Program.cs; check git history for manual commits
- **AC Impact**: Direct JSON edit only, do NOT regenerate

**C4: Program.cs `additionalProperties: false` would break current schema**
- **Source**: Investigation evidence
- **Verification**: Check Program.cs output against committed schema; verify divergence exists
- **AC Impact**: Do NOT run generator

**C5: TreatWarningsAsErrors enforced**
- **Source**: Directory.Build.props
- **Verification**: Confirm Directory.Build.props contains `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
- **AC Impact**: Build must succeed with zero warnings

**C6: NegatedCondition is the ONLY `null!` ICondition type**
- **Source**: Investigation consensus (3/3)
- **Verification**: Grep all ICondition implementations for `null!` pattern; verify only NegatedCondition uses it
- **AC Impact**: Scope alignment to NegatedCondition only

**C7: Schema uses permissive `type: object` for variable type values**
- **Source**: Schema evidence
- **Verification**: Inspect dialogue-schema.json lines 148, 158, 168, 177, 188, 198 for `type: object`
- **AC Impact**: Adding `bitwise_and_cmp` is informational documentation

**C8: Schema missing 10+ variable types (EQUIP, ITEM, STAIN, etc.)**
- **Source**: Out-of-scope
- **Verification**: Note items not documented; confirm F754 tracks unification
- **AC Impact**: Do not address in F767

**C9: Schema validation tests reference committed schema**
- **Source**: SchemaValidationTests.cs
- **Verification**: Confirm SchemaValidationTests.cs loads `dialogue-schema.json` and validates against it
- **AC Impact**: Must pass after edit

**C10: `bitwise_and` (truthiness) also undocumented**
- **Source**: Investigation 2, 3
- **Verification**: Confirm `bitwise_and` operator exists in parser but has no schema documentation
- **AC Impact**: Consider documenting both operators

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| `required init` breaks JSON deserialization | LOW | HIGH | BitwiseComparisonCondition proves pattern works |
| Schema edit introduces JSON syntax error | LOW | HIGH | Validate with schema validation tests |
| Updating schema without Program.cs creates further divergence | MEDIUM | LOW | Known divergence; F754 tracks unification |
| Scope creep to all model classes | MEDIUM | MEDIUM | Feature scope is NegatedCondition only per F759 handoff |

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| `null!` count in src/tools/dotnet/ErbParser/ | `Grep("null!", src/tools/dotnet/ErbParser/)` | 1 (NegatedCondition.cs:8) | Target: 0 |
| Bitwise operators documented in schema | `Grep("bitwise", dialogue-schema.json)` | 0 | Target: 2 (bitwise_and_cmp, bitwise_and) |

**Baseline File**: N/A (infra feature — no runtime behavior, metrics are code-level counts)

---

## AC Design Constraints

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | NegatedCondition `Inner` must use `required init` | F759 handoff | Verify via code inspection |
| C2 | Must NOT have `null!` pattern in NegatedCondition | Current debt | `not_contains "null!"` AC |
| C3 | Object initializer at construction sites must compile | LogicalOperatorParser.cs:216 | Build AC mandatory |
| C4 | JSON serialization roundtrip must work | ICondition JsonDerivedType | Indirect via AC#8 (ErbParser.Tests) + BitwiseComparisonCondition precedent; no dedicated NegatedCondition serialization test exists |
| C5 | Schema must contain `bitwise_and_cmp` documentation | F759 TC#9 | `contains` AC on schema file |
| C6 | Schema change must not break validation | SchemaValidationTests.cs | Test AC |
| C7 | TreatWarningsAsErrors enforced | Directory.Build.props | Build with zero warnings |
| C8 | Scope limited to NegatedCondition only | F759 handoff | Other ICondition types unchanged |
| C9 | Schema is hand-maintained | Investigation consensus | Direct JSON edit, NOT regenerate |

## Dependencies

| Type | Feature | Status | Description |
|------|-------------------|--------|-------------|
| Predecessor | F759 | [DONE] | Parent feature; created `bitwise_and_cmp` operator and BitwiseComparisonCondition |
| Related | F754 | [CANCELLED] | YAML Format Unification (branches vs entries) -- overlaps schema divergence |
| Related | F768 | [CANCELLED] | Cross-parser refactoring -- sibling debt from same parser evolution |
| Related | F706 | [DONE] | Downstream equivalence verification |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Eliminate property pattern inconsistency" | NegatedCondition must use same property pattern as BitwiseComparisonCondition (`required init`) | AC#1, AC#2, AC#3, AC#4 |
| "compile-time safety" | `null!` suppression must be removed; `required init` enforces initialization at construction | AC#1, AC#2, AC#5 |
| "documentation gaps" / "developer discoverability" | `bitwise_and_cmp` and `bitwise_and` operators must be explicitly documented in schema | AC#6, AC#7, AC#13, AC#15 |
| "matching the BitwiseComparisonCondition pattern" (Goal 1) | Property declaration must match exact pattern used by BitwiseComparisonCondition | AC#1, AC#3 |
| "explicit documentation for developer reference" (Goals 2, 3) | Schema conditionElement definitions must describe bitwise_and_cmp and bitwise_and | AC#6, AC#7, AC#13, AC#15 |
| "zero warnings" (C5/C7) | Build must succeed with TreatWarningsAsErrors | AC#5 |
| "existing tests must pass" (C9) | ErbParser.Tests and YamlSchemaGen.Tests must pass without modification | AC#8, AC#9 |
| "scope alignment to NegatedCondition only" (C6) | Other ICondition types must remain unchanged | AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | NegatedCondition Inner uses required init pattern | code | Grep(src/tools/dotnet/ErbParser/NegatedCondition.cs) | matches | `public required ICondition Inner \{ get; init; \}` | [x] |
| 2 | NegatedCondition has no null! suppression | code | Grep(src/tools/dotnet/ErbParser/NegatedCondition.cs) | not_contains | `null!` | [x] |
| 3 | NegatedCondition Inner has JsonPropertyName attribute | code | Grep(src/tools/dotnet/ErbParser/NegatedCondition.cs) | contains | `[JsonPropertyName("inner")]` | [x] |
| 4 | Object initializer syntax preserved at construction site | code | Grep(src/tools/dotnet/ErbParser/LogicalOperatorParser.cs) | contains | `new NegatedCondition { Inner = parsed }` | [x] |
| 5 | ErbParser project builds with zero warnings | build | dotnet build src/tools/dotnet/ErbParser/ | succeeds | - | [x] |
| 6 | Schema contains bitwise_and_cmp condition documentation | code | Grep(src/tools/dotnet/YamlSchemaGen/dialogue-schema.json) | contains | `bitwise_and_cmp` | [x] |
| 7 | Schema bitwise_and_cmp has description text | code | Grep(src/tools/dotnet/YamlSchemaGen/dialogue-schema.json) | matches | `description.*bitwise.*comparison` | [x] |
| 8 | ErbParser.Tests pass | test | dotnet test src/tools/dotnet/ErbParser.Tests/ | succeeds | - | [x] |
| 9 | YamlSchemaGen.Tests pass (existing tests + JSON validity) | test | dotnet test src/tools/dotnet/YamlSchemaGen.Tests/ | succeeds | - | [x] |
| 10 | Other ICondition types unchanged (no null! elsewhere) | code | Grep(src/tools/dotnet/ErbParser/) | not_contains | `null!` | [x] |
| 11 | NegatedCondition has no set accessor | code | Grep(src/tools/dotnet/ErbParser/NegatedCondition.cs) | not_contains | `{ get; set; }` | [x] |
| 12 | ErbToYaml project builds with zero warnings | build | dotnet build src/tools/dotnet/ErbToYaml/ | succeeds | - | [x] |
| 13 | Schema contains bitwise_and truthiness documentation | code | Grep(src/tools/dotnet/YamlSchemaGen/dialogue-schema.json) | matches | `"bitwise_and"\s*:` | [x] |
| 14 | Schema bitwise_and_cmp uses correct field names | code | Grep(src/tools/dotnet/YamlSchemaGen/dialogue-schema.json) | contains | `"mask"` | [x] |
| 15 | Schema bitwise_and has description text | code | Grep(src/tools/dotnet/YamlSchemaGen/dialogue-schema.json) | matches | `description.*bitwise.*truthiness` | [x] |

**Note**: 15 ACs within infra range (8-16). AC#9 covers both existing test regression and JSON validity (merged from former AC#9/AC#11). AC#12 verifies downstream consumer ErbToYaml builds after NegatedCondition property change.

### AC Details

**AC#1: NegatedCondition Inner uses required init pattern**
- **Test**: `Grep(src/tools/dotnet/ErbParser/NegatedCondition.cs)`
- **Expected**: matches `public required ICondition Inner { get; init; }`
- **Rationale**: Verifies Goal 1 alignment to required init pattern matching BitwiseComparisonCondition

**AC#2: NegatedCondition has no null! suppression**
- **Test**: `Grep(src/tools/dotnet/ErbParser/NegatedCondition.cs)`
- **Expected**: not_contains `null!`
- **Rationale**: Verifies null! anti-pattern completely removed

**AC#3: NegatedCondition Inner has JsonPropertyName attribute**
- **Test**: `Grep(src/tools/dotnet/ErbParser/NegatedCondition.cs)`
- **Expected**: contains `[JsonPropertyName("inner")]`
- **Rationale**: Ensures JSON serialization attribute preserved after property change

**AC#4: Object initializer syntax preserved at construction site**
- **Test**: `Grep(src/tools/dotnet/ErbParser/LogicalOperatorParser.cs)`
- **Expected**: contains `new NegatedCondition { Inner = parsed }`
- **Rationale**: Regression guard confirming construction site is not modified

**AC#5: ErbParser project builds with zero warnings**
- **Test**: `dotnet build src/tools/dotnet/ErbParser/`
- **Expected**: succeeds (zero warnings)
- **Rationale**: TreatWarningsAsErrors enforced; required init must not introduce warnings

**AC#6: Schema contains bitwise_and_cmp condition documentation**
- **Test**: `Grep(src/tools/dotnet/YamlSchemaGen/dialogue-schema.json)`
- **Expected**: contains `bitwise_and_cmp`
- **Rationale**: Verifies operator documentation exists in schema for developer reference

**AC#7: Schema bitwise_and_cmp has description text**
- **Test**: `Grep(src/tools/dotnet/YamlSchemaGen/dialogue-schema.json)`
- **Expected**: matches `description.*bitwise.*comparison`
- **Rationale**: Verifies description text explains the operator purpose

**AC#8: ErbParser.Tests pass**
- **Test**: `dotnet test src/tools/dotnet/ErbParser.Tests/`
- **Expected**: succeeds
- **Rationale**: Ensures NegatedCondition property change causes no test regression

**AC#9: YamlSchemaGen.Tests pass (existing tests + JSON validity)**
- **Test**: `dotnet test src/tools/dotnet/YamlSchemaGen.Tests/`
- **Expected**: succeeds
- **Rationale**: Schema validation tests must pass after schema edits; covers JSON validity

**AC#10: Other ICondition types unchanged (no null! elsewhere)**
- **Test**: `Grep(src/tools/dotnet/ErbParser/)`
- **Expected**: not_contains `null!`
- **Rationale**: Scope guard verifying NegatedCondition was the only null! user

**AC#11: NegatedCondition has no set accessor**
- **Test**: `Grep(src/tools/dotnet/ErbParser/NegatedCondition.cs)`
- **Expected**: not_contains `{ get; set; }`
- **Rationale**: Negative verification ensuring old pattern completely removed

**AC#12: ErbToYaml project builds with zero warnings**
- **Test**: `dotnet build src/tools/dotnet/ErbToYaml/`
- **Expected**: succeeds (zero warnings)
- **Rationale**: Verifies downstream consumer compiles after NegatedCondition property change

**AC#13: Schema contains bitwise_and truthiness documentation**
- **Test**: `Grep(src/tools/dotnet/YamlSchemaGen/dialogue-schema.json)`
- **Expected**: matches `"bitwise_and"\s*:`
- **Rationale**: Verifies bitwise_and truthiness operator documented distinctly from bitwise_and_cmp; actual YAML output is flat operator-value (ConditionSerializer.cs:84)

**AC#14: Schema bitwise_and_cmp uses correct field names**
- **Test**: `Grep(src/tools/dotnet/YamlSchemaGen/dialogue-schema.json)`
- **Expected**: contains `"mask"`
- **Rationale**: Verifies schema uses correct field name from actual YAML output (ConditionSerializer.cs:302), preventing documentation of wrong field names

**AC#15: Schema bitwise_and has description text**
- **Test**: `Grep(src/tools/dotnet/YamlSchemaGen/dialogue-schema.json)`
- **Expected**: matches `description.*bitwise.*truthiness`
- **Rationale**: Verifies bitwise_and truthiness operator has description text, symmetric with AC#7 for bitwise_and_cmp

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:--------------:|
| 1 | Align NegatedCondition Inner from `{ get; set; } = null!` to `required init` | AC#1, AC#2, AC#3, AC#4, AC#11 |
| 2 | Add explicit `bitwise_and_cmp` documentation to dialogue-schema.json | AC#6, AC#7, AC#14 |
| 3 | Add explicit `bitwise_and` (truthiness) documentation to dialogue-schema.json | AC#13, AC#15 |
| (Implicit) | Build succeeds with zero warnings | AC#5, AC#12 |
| (Implicit) | Existing tests pass | AC#8, AC#9 |
| (Implicit) | Scope limited to NegatedCondition | AC#10 |

All Goal items are covered.

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature consists of two independent, low-risk changes with proven precedent:

**1. NegatedCondition Property Pattern Alignment**

Replace the `{ get; set; } = null!` pattern with `public required ICondition Inner { get; init; }` to match BitwiseComparisonCondition's safer compile-time enforcement pattern. The `null!` null-forgiving operator suppresses nullable reference warnings but provides no compile-time guarantee that `Inner` is initialized. The `required init` pattern enforces initialization at construction time via object initializer syntax, which is already used at all NegatedCondition construction sites.

**Implementation**: Direct property declaration edit in `NegatedCondition.cs` line 8. The existing `[JsonPropertyName("inner")]` attribute (line 7) remains unchanged. Object initializer syntax `new NegatedCondition { Inner = parsed }` at LogicalOperatorParser.cs:216 is compatible with both `set` and `init` accessors, requiring no consumer code changes.

**2. Schema Documentation Addition**

Add explicit `bitwise_and_cmp` operator documentation to the `conditionElement` definitions in `dialogue-schema.json`. The schema currently uses permissive `type: object` for all condition variable types (TALENT, ABL, EXP, etc.) per lines 148, 158, 168, 177, 188, 198. This permissive structure already accepts the nested dictionary format output by BitwiseComparisonCondition, but developers have no documentation indicating that `bitwise_and_cmp` is a valid operator.

**Implementation**: Add a `definitions.bitwiseOperators` section documenting the bitwise operator formats. The bitwise operators are value-level constructs nested inside variable type entries, not top-level condition types. Per Technical Constraint C10 investigation recommendation, the design should also document the truthiness operator `bitwise_and` (bitwise result without comparison) if space permits.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Grep verification: Edit NegatedCondition.cs line 8 to `public required ICondition Inner { get; init; }` matching BitwiseComparisonCondition.cs:17 pattern |
| 2 | Negative verification: After edit, `null!` string is completely removed from NegatedCondition.cs (line 8 currently contains it) |
| 3 | Preserve existing `[JsonPropertyName("inner")]` attribute at line 7; Grep verifies attribute remains adjacent to property |
| 4 | No change needed at LogicalOperatorParser.cs:216; object initializer syntax `{ Inner = parsed }` compiles with `init` accessor; Grep verifies existing pattern |
| 5 | Build verification: `dotnet build src/tools/dotnet/ErbParser/` with TreatWarningsAsErrors enforced via Directory.Build.props; `required init` pattern proven by BitwiseComparisonCondition |
| 6 | Add `bitwise_and_cmp` documentation to dialogue-schema.json definitions.bitwiseOperators section with operator structure description |
| 7 | Add `description` field to bitwise_and_cmp operator definition: "Bitwise AND with comparison condition: (VAR & mask) op value" |
| 8 | No test changes needed; LogicalOperatorParserTests.cs:330-332 uses object initializer compatible with `init`; run `dotnet test src/tools/dotnet/ErbParser.Tests/` |
| 9 | No schema validation test changes needed; permissive `type: object` structure unchanged; run `dotnet test src/tools/dotnet/YamlSchemaGen.Tests/` |
| 10 | Grep verification across `src/tools/dotnet/ErbParser/` directory: NegatedCondition.cs is the only file with `null!` per investigation consensus C6; after fix, zero occurrences remain |
| 11 | Negative verification: Grep `not_contains "{ get; set; }"` in NegatedCondition.cs ensures old pattern completely removed (complements AC#1 positive match) |
| 12 | Build verification: `dotnet build src/tools/dotnet/ErbToYaml/` verifies downstream consumer compiles after NegatedCondition property change; ErbToYaml.csproj references ErbParser |
| 13 | Add bitwise_and documentation to dialogue-schema.json definitions.bitwiseOperators section; Grep matches `"bitwise_and"\s*:` to verify the key is present distinctly from `bitwise_and_cmp`; actual YAML output is flat operator-value (ConditionSerializer.cs:84) |
| 14 | Grep verification: dialogue-schema.json contains `"mask"` field name from actual bitwise_and_cmp YAML output (ConditionSerializer.cs:302); prevents documenting wrong field names |
| 15 | Add description field to bitwise_and operator definition in definitions.bitwiseOperators section: "Bitwise AND truthiness condition: (VAR & mask) evaluates non-zero"; Grep matches `description.*bitwise.*truthiness` |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Property pattern for NegatedCondition | (A) Keep `{ get; set; } = null!`<br>(B) Change to `required init`<br>(C) Change to nullable `ICondition?` | B | Matches F759 BitwiseComparisonCondition pattern; compile-time enforcement superior to runtime null-forgiving; object initializer syntax compatible; no consumer changes needed |
| Schema edit scope | (A) Add `bitwise_and_cmp` only<br>(B) Regenerate entire schema from Program.cs<br>(C) Add all missing operators (10+ variable types per C8) | A | Schema is hand-maintained per C3; Program.cs divergence tracked in F754; scope limited to F759 handoff; missing types are separate debt per C8 |
| Schema structure for bitwise_and_cmp | (A) Add to top-level condition.oneOf<br>(B) Add to definitions.conditionElement.oneOf<br>(C) Add as definitions reference from variable type descriptions | C | Bitwise operators are value-level constructs nested inside variable type entries (ConditionSerializer.cs:84,300-304), not top-level condition types. Standalone definitions section documents the correct structure for developer reference. |
| Document bitwise_and (truthiness) | (A) Yes, add alongside bitwise_and_cmp<br>(B) No, defer to separate feature | A | C10 investigation recommendation; truthiness pattern `(VAR & mask)` without comparison is valid; minimal additional documentation cost; improves developer reference completeness |
| Consumer code updates | (A) Update LogicalOperatorParser.cs:216<br>(B) No changes, rely on compatibility | B | Object initializer `{ Inner = parsed }` compiles with both `set` and `init`; C# language spec guarantees compatibility; no functional change needed |

### Interfaces / Data Structures

**No new types.** Both changes modify existing declarations:

**NegatedCondition.cs (before)**:
```csharp
public class NegatedCondition : ICondition
{
    [JsonPropertyName("inner")]
    public ICondition Inner { get; set; } = null!;
}
```

**NegatedCondition.cs (after)**:
```csharp
public class NegatedCondition : ICondition
{
    [JsonPropertyName("inner")]
    public required ICondition Inner { get; init; }
}
```

**dialogue-schema.json additions** (new definitions section):

New `definitions.bitwiseOperators` section:
```json
"bitwiseOperators": {
  "description": "Bitwise operators that appear as value-level constructs within variable type conditions (e.g., TALENT > key > operator)",
  "oneOf": [
    {
      "type": "object",
      "description": "Bitwise AND with comparison condition: (VAR & mask) op value",
      "properties": {
        "bitwise_and_cmp": {
          "type": "object",
          "properties": {
            "mask": { "type": "string", "description": "Bitmask value for bitwise AND" },
            "op": { "type": "string", "enum": ["eq", "ne", "gt", "gte", "lt", "lte"], "description": "Normalized comparison operator" },
            "value": { "type": "string", "description": "Comparison target value" }
          },
          "required": ["mask", "op", "value"]
        }
      },
      "required": ["bitwise_and_cmp"]
    },
    {
      "type": "object",
      "description": "Bitwise AND truthiness condition: (VAR & mask) evaluates non-zero",
      "properties": {
        "bitwise_and": {
          "type": "string",
          "description": "Mask value for bitwise AND truthiness check"
        }
      },
      "required": ["bitwise_and"]
    }
  ]
}
```

**Notes**:
- Bitwise operators are documented as a standalone definitions section for developer reference
- `bitwise_and_cmp` uses `mask`/`op`/`value` fields matching actual YAML output (ConditionSerializer.cs:300-304, BitwiseComparisonCondition `[JsonPropertyName]` attributes)
- `bitwise_and` uses flat string value (mask) matching actual YAML output (ConditionSerializer.cs:84)
- Permissive `type: object` for variable types (TALENT/ABL/etc.) remains unchanged per C7

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,2,3,4,11 | Edit NegatedCondition.cs property to `public required ICondition Inner { get; init; }` | | [x] |
| 2 | 5,12 | Build ErbParser and ErbToYaml projects with zero warnings | | [x] |
| 3 | 6,7,13,14,15 | Add bitwise_and_cmp and bitwise_and entries to dialogue-schema.json conditionElement definitions | | [x] |
| 4 | 8 | Run ErbParser.Tests to verify no regression | | [x] |
| 5 | 9 | Run YamlSchemaGen.Tests to verify schema validation and JSON validity | | [x] |
| 6 | 10 | Verify no null! pattern elsewhere in src/tools/dotnet/ErbParser/ (scope guard) | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1: NegatedCondition.cs line 8 pattern from Technical Design | Property declaration edit |
| 2 | ac-tester | haiku | Verify T1 output + Execute T2,T4: Verify NegatedCondition pattern + Build ErbParser/ErbToYaml + run ErbParser.Tests (ACs #1-5,8,11,12) | Verification results (property pattern + build + tests) |
| 3 | implementer | sonnet | T3: dialogue-schema.json structure from Technical Design | Schema documentation addition |
| 4 | ac-tester | haiku | T5,T6: Run YamlSchemaGen.Tests + verify scope (ACs #6,7,9,10,13,14,15) | Verification results (schema + tests + scope guard) |

**Constraints** (from Technical Design):

1. NegatedCondition.cs line 8 must change from `{ get; set; } = null!` to `public required ICondition Inner { get; init; }`
2. Preserve `[JsonPropertyName("inner")]` attribute at line 7
3. No changes to LogicalOperatorParser.cs:216 - object initializer syntax compatible with both `set` and `init`
4. Schema edit: Add new definitions.bitwiseOperators section for developer reference documentation
5. Schema structure: Bitwise operators are value-level constructs, documented in separate definitions section (not conditionElement.oneOf)
6. Schema must include both `bitwise_and_cmp` (with comparison) and `bitwise_and` (truthiness only) per C10
7. TreatWarningsAsErrors enforced - build must succeed with zero warnings
8. Scope limited to NegatedCondition only - no other ICondition types modified

**Pre-conditions**:

- F759 completed (BitwiseComparisonCondition exists as precedent for `required init` pattern)
- `src/tools/dotnet/ErbParser/NegatedCondition.cs` exists at line 8 with `{ get; set; } = null!` pattern
- `src/tools/dotnet/YamlSchemaGen/dialogue-schema.json` exists with conditionElement definitions (lines 143-233)
- All existing tests pass (ErbParser.Tests, YamlSchemaGen.Tests)
- Directory.Build.props enforces TreatWarningsAsErrors=true

**Success Criteria**:

- All 15 ACs pass verification
- `dotnet build src/tools/dotnet/ErbParser/` succeeds with zero warnings
- `dotnet test src/tools/dotnet/ErbParser.Tests/` all pass
- `dotnet test src/tools/dotnet/YamlSchemaGen.Tests/` all pass
- Grep verification: zero occurrences of `null!` in `src/tools/dotnet/ErbParser/` directory
- Grep verification: zero occurrences of `{ get; set; }` in NegatedCondition.cs
- Schema contains `bitwise_and_cmp` with description field

**Rollback Plan**:

If issues arise after deployment:

1. Revert commit with `git revert <commit-hash>`
2. Notify user of rollback with specific failure details
3. Create follow-up feature for investigation and fix with:
   - Root cause analysis of why `required init` pattern failed
   - Investigation of JSON serialization edge cases if deserialization issues occurred
   - Investigation of schema validation test failures if SchemaValidationTests failed

**Notes**:

- Phase 1 and Phase 3 are implementation phases (direct file edits)
- Phase 2 and Phase 4 are verification phases (test execution and pattern checking)
- 6 Tasks cover 15 ACs using N:1 AC:Task mapping (Multiple ACs per Task allowed per template)
- T1 covers AC#1,2,3,4,11; T2 covers AC#5,12; T3 covers AC#6,7,13,14,15; T4 covers AC#8; T5 covers AC#9; T6 covers AC#10
- T2 (build), T4 (ErbParser.Tests), T5 (YamlSchemaGen.Tests), T6 (scope guard) are verification tasks

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| (none identified) | - | - | - | - |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-10 10:51 | START | implementer | Task 3 | - |
| 2026-02-10 10:51 | END | implementer | Task 3 | SUCCESS |
| 2026-02-10 11:42 | START | ac-tester | IC Phase 4: Verify ACs #6,7,9,10,13,14,15 | - |
| 2026-02-10 11:42 | END | ac-tester | IC Phase 4: Verify schema + tests + scope | SUCCESS (7/7 ACs PASS) |
| 2026-02-10 12:00 | START | ac-tester | Full AC Verification: All 15 ACs | - |
| 2026-02-10 12:00 | END | ac-tester | Full AC Verification | 14 PASS, 1 FAIL (AC#7 case-sensitivity) |
| 2026-02-10 12:01 | DEVIATION | ac-tester | AC#7 verification | FAIL: regex case mismatch (Bitwise vs bitwise) |
| 2026-02-10 12:01 | FIX | orchestrator | Fix description case | Changed "Bitwise" to "bitwise" in schema descriptions |
| 2026-02-10 12:01 | VERIFY | orchestrator | Re-verify AC#7,15,9 | ALL PASS |

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->

- [fix] AC Verification: [AC-007] AC#7 regex pattern case-sensitive mismatch | Fixed: Changed "Bitwise" to "bitwise" in schema descriptions (lines 254, 270). AC#7 now passes.
- [fix] Phase1-RefCheck iter1: [FMT-003] Links section | Missing link for F757
- [fix] Phase1-RefCheck iter1: [FMT-003] Links section | Missing link for F758
- [fix] Phase1-RefCheck iter1: [FMT-003] Links section | Missing link for F754
- [fix] Phase1-RefCheck iter1: [FMT-003] Links section | Missing link for F768
- [fix] Phase1-RefCheck iter1: [FMT-003] Links section | Missing link for F706
- [fix] Phase1-RefCheck iter1: [FMT-003] Links section | F757 link path corrected to archive/
- [resolved-applied] Phase2-Pending iter1: [FMT-002] Tasks table missing mandatory Tag column (resolved via Task consolidation fix)
- [resolved-applied] Phase2-Pending iter1: [AC-005] No AC verifies bitwise_and (truthiness) schema documentation despite Key Decision selecting Option A → Added AC#13, Goal 3, updated T3
- [fix] Phase2-Review iter1: [FMT-002] Execution Log table | Wrong columns (3 cols → 5 cols per template)
- [fix] Phase2-Review iter1: [FMT-002] Implementation Contract table | Extra Tasks column removed (6 cols → 5 cols per template)
- [fix] Phase2-Review iter1: [AC-005] AC#9/AC#11 | Merged redundant AC#11 into AC#9
- [fix] Phase2-Review iter1: [TSK-001] Tasks table | Consolidated 12 verification-mirror Tasks to 6 implementation-centric Tasks with N:1 AC mapping
- [resolved-applied] Phase2-Pending iter2: [INV-003] Technical Design schema structure for bitwise_and_cmp is factually incorrect (wrong placement, field names, nesting) → Fixed field names to mask/op/value per ConditionSerializer.cs
- [fix] Phase2-Review iter2: [FMT-002] AC Coverage table | Removed duplicate AC#11 entry (stale from AC merge)
- [fix] Phase2-Review iter2: [AC-005] Philosophy Derivation | Fixed compile-time safety mapping from AC#2,AC#4 to AC#2,AC#5
- [fix] Phase2-Review iter2: [FMT-001] Added Baseline Measurement section (empty, with deletion comment for infra type)
- [fix] Phase2-Review iter2: [FMT-002] Review Notes | Added error-taxonomy category codes to all entries
- [fix] Phase2-Review iter3: [FMT-001] AC Design Constraints | Added ### Constraint Details subsection with C1-C10 detail blocks
- [fix] Phase2-Review iter3: [FMT-002] Tasks table header | Changed T# to Task# per template
- [fix] Phase2-Review iter3: [FMT-002] Dependencies table header | Changed Feature/Component to Feature per template
- [fix] Phase2-Review iter3: [FMT-002] Implementation Contract Phase table | Fixed AC#10 misplacement (Phase 2→Phase 4) and added T1 to Phase 2 Input
- [resolved-applied] Phase2-Uncertain iter3: [FMT-002] Implementation Contract Phase 2 Input column semantics ambiguous (T1 reference) → Clarified as 'Verify T1 output + Execute T2,T4'
- [resolved-applied] Phase2-Pending iter3: [AC-005] Goal section missing bitwise_and truthiness (orphaned Key Decision Option A) → Added Goal 3
- [fix] Phase2-Review iter4: [FMT-002] 5 Whys | Reformatted from numbered list to 4-column table (Level|Question|Answer|Evidence)
- [fix] Phase2-Review iter4: [FMT-002] Feasibility Assessment | Reformatted from prose to 3-column table (Criterion|Assessment|Evidence)
- [fix] Phase2-Review iter4: [FMT-002] Review Context Origin Timestamp | Changed from 'F759 implementation' to YYYY-MM-DD format
- [fix] Phase2-Review iter4: [AC-005] AC Design Constraint #4 | Downgraded to note indirect coverage (no dedicated serialization AC)
- [resolved-applied] Phase2-Pending iter4: [INV-003] Tasks T3 description propagates wrong schema placement from Technical Design → T3 updated with bitwise_and scope
- [resolved-applied] Phase2-Uncertain iter4: [CON-003] IC constraint #6 'optionally' ambiguous vs Key Decision Option A '(if space permits)' → Removed 'optionally', changed Key Decision to firm 'A'
- [fix] Phase2-Review iter5: [FMT-002] Symptom vs Root Cause table | Restructured to template format (Aspect|Symptom|Root Cause with What/Where/Fix rows)
- [fix] Phase2-Review iter5: [FMT-002] Technical Constraints table | Removed extra ID column, merged ID into Constraint column
- [fix] Phase2-Review iter5: [FMT-002] Impact Analysis table | Renamed 'Component' to 'Area' per template
- [fix] Phase2-Review iter5: [AC-005] Philosophy Derivation | Added AC#1 to compile-time safety row
- [fix] Phase2-Review iter5: [AC-005] AC#4 Description | Changed 'compiles' to 'preserved' to reflect Grep method
- [resolved-applied] Phase2-Pending iter5: [INV-003] Technical Design bitwise_and (truthiness) schema also factually incorrect (flat string, not nested object) → Fixed to flat string per ConditionSerializer.cs:84
- [fix] Phase2-Review iter6: [FMT-002] Impact Analysis table | Changed Impact values from Direct/None to HIGH/LOW per template
- [resolved-applied] Phase2-Pending iter6: [FMT-002] AC Details blocks missing mandatory Test/Expected/Rationale subfields (all 11 ACs) → Rewritten all 13 AC Details to template format
- [resolved-applied] Phase2-Pending iter6: [INV-003] Philosophy inheritance claim fabricated — F759/F758 have different philosophies → Rewritten as F767's own Philosophy
- [resolved-applied] Phase2-Uncertain iter6: [FMT-001] Baseline Measurement section comment-only (infra type not in DELETE list) → Replaced with visible N/A text and before/after metrics
- [resolved-applied] Phase2-Uncertain iter6: [AC-005] ACs #6/#7 don't verify schema field structure (downstream of INV-003) → Added AC#14 for field name verification
- [resolved-applied] Phase2-Uncertain iter6: [FMT-002] Impact Analysis omits ConditionSerializer.cs:84,300-304 YAML output references → Added both as LOW impact entries
- [fix] Phase2-Review iter7: [FMT-002] Section ordering | Reordered to match template (Impact Analysis→TC→Risks→Baseline→AC Design Constraints→Dependencies)
- [fix] Phase2-Review iter7: [FMT-002] Goal Coverage Verification | Column header 'Covering ACs' → 'Covering AC(s)'
- [fix] Phase2-Review iter7: [FMT-002] Review Context header | Removed non-template parenthetical annotation
- [fix] Phase2-Review iter8: [AC-005] Added AC#12 (ErbToYaml build verification) for downstream consumer coverage
- [fix] Phase4-ACValidation iter1: [AC-005] AC#10 | Changed matcher from not_matches to not_contains for semantic correctness (null! is literal string, not regex)
- [fix] PostLoop-UserFix post-loop: [AC-005] Added Goal 3, AC#13, AC#14, updated T3/IC for bitwise_and scope inclusion
- [fix] PostLoop-UserFix post-loop: [INV-003] Fixed bitwise_and_cmp schema fields to mask/op/value per ConditionSerializer.cs
- [fix] PostLoop-UserFix post-loop: [INV-003] Fixed bitwise_and schema to flat string per ConditionSerializer.cs:84
- [fix] PostLoop-UserFix post-loop: [FMT-002] IC Phase 2 Input clarified as 'Verify T1 output + Execute T2,T4'
- [fix] PostLoop-UserFix post-loop: [FMT-002] AC Details rewritten to Test/Expected/Rationale format (all 13 blocks)
- [fix] PostLoop-UserFix post-loop: [INV-003] Philosophy rewritten as F767's own (removed fabricated F759/F758 inheritance)
- [fix] PostLoop-UserFix post-loop: [FMT-001] Baseline Measurement changed from comment to visible N/A with metrics
- [fix] PostLoop-UserFix post-loop: [AC-005] Added AC#14 for schema field name verification (mask)
- [fix] PostLoop-UserFix post-loop: [FMT-002] Added ConditionSerializer.cs:84,300-304 to Impact Analysis
- [fix] Phase2-Review iter1: [FMT-002] IC Success Criteria | Changed 'All 12 ACs' to 'All 14 ACs'
- [fix] Phase2-Review iter1: [FMT-002] IC Notes | Changed '11 ACs' to '14 ACs'
- [fix] Phase2-Review iter1: [FMT-002] IC Notes | Updated Task-AC mapping enumeration to cover all 14 ACs
- [resolved-applied] Phase2-Review iter1: [FMT-001] Baseline Measurement uses free-text N/A instead of template table format → Changed to template table format with null!/schema metrics per user decision
- [fix] Phase2-Review iter2: [INV-003] Impact Analysis, AC Details, Technical Design | Replaced all DatalistConverter.cs references with ConditionSerializer.cs (correct file for bitwise operators)
- [fix] Phase2-Review iter3: [FMT-002] AC Design Constraints ID column | Changed bare numbers (1-9) to C-prefix format (C1-C9) per template
- [resolved-applied] Phase2-Uncertain iter3: [FMT-002] Goal Coverage Goal Item column uses text labels ('Goal 1', '(Implicit)') instead of numeric indices → Changed explicit Goals to numeric (1,2,3), kept (Implicit) as-is per user decision
- [resolved-skipped] Phase2-Uncertain iter4: [FMT-002] Review Context Discovery Point — template shows 'Philosophy Gate (POST-LOOP Step 6.3)' but F767 was created from TC#9/Mandatory Handoffs, not Philosophy Gate → Current value is factually accurate per user decision
- [resolved-applied] Phase2-Review iter4: [INV-003] Technical Design schema structure places bitwise_and_cmp and bitwise_and as top-level conditionElement.oneOf peers, but actual YAML output nests them inside variable type objects (e.g., TALENT > key > bitwise_and_cmp). Schema documentation would misrepresent actual structure. → Redesigned to definitions.bitwiseOperators section per user decision
- [resolved-applied] Phase2-Review iter4: [AC-005] Goal 3 (bitwise_and) has only key presence AC#13 but no description text AC, while Goal 2 has both AC#6 (key) and AC#7 (description). Asymmetric coverage → Added AC#15 per user decision
- [resolved-applied] Phase3-Maintainability iter4: [AC-005] AC#6,7,13,14 Expected values assume top-level conditionElement placement — blocked on schema structure decision → Unblocked: ACs use string-presence matchers (contains/matches) which work in definitions.bitwiseOperators placement
- [fix] PostLoop-UserFix post-loop: [INV-003] Technical Design schema structure redesigned from conditionElement.oneOf to definitions.bitwiseOperators (value-level nesting)
- [fix] PostLoop-UserFix post-loop: [AC-005] Added AC#15 for bitwise_and description text verification (symmetric with AC#7)
- [fix] PostLoop-UserFix post-loop: [FMT-001] Baseline Measurement changed to template table format with null!/schema metrics
- [fix] PostLoop-UserFix post-loop: [FMT-002] Goal Coverage Goal Item column changed to numeric (1,2,3) for explicit Goals
- [fix] Phase2-Review iter1: [AC-005] TD AC Coverage table | Added AC#15 row
- [fix] Phase2-Review iter1: [FMT-002] IC Phase 4 | Added AC#15 to Phase 4 AC list
- [resolved-applied] Phase2-Uncertain iter1: [FMT-002] Impact Analysis table Impact column uses left alignment instead of center alignment per template → Fixed to center alignment per user decision
- [fix] Phase2-Review iter2: [FMT-002] Missing --- separators | Added between Baseline/AC Design Constraints and Dependencies/AC sections
- [fix] Phase2-Review iter2: [FMT-002] Risks table | Center-aligned Likelihood and Impact columns
- [fix] Phase2-Review iter2: [FMT-002] Goal Coverage table | Center-aligned Goal Item and Covering AC(s) columns
- [fix] Phase2-Review iter3: [CON-003] TD Approach, Key Decision, Interfaces, Notes, IC Constraint | Removed 'referenced by variable type descriptions' claims to match standalone definitions approach
- [fix] PostLoop-UserFix post-loop: [FMT-002] Impact Analysis Impact column changed to center alignment per template

---

## Links
- [feature-759.md](feature-759.md) - Parent feature (compound bitwise condition parsing)
- [feature-757.md](archive/feature-757.md) - Foundation: bitwise `&` operator support
- [feature-758.md](feature-758.md) - Grandparent: prefix-based variable type expansion
- [feature-754.md](feature-754.md) - Related: YAML format unification
- [feature-768.md](feature-768.md) - Sibling: cross-parser refactoring
- [feature-706.md](feature-706.md) - Downstream: full equivalence verification
