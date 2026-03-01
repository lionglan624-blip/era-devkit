# Feature 762: ARG Bare Variable Condition Parsing

## Status: [DONE]
<!-- wip-started: 2026-02-08T00:00:00Z -->

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

## Review Context

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F758 |
| Discovery Point | Philosophy Gate (POST-LOOP Step 6.3) |
| Timestamp | 2026-02-05 |

### Identified Gap
ARG bare variables (94 occurrences in kojo EVENT files) cannot be parsed because VariableConditionParser requires PREFIX: colon-separated format. ARG uses a bare identifier with no prefix delimiter. F758 POST-LOOP philosophy-deriver identified this as a missing variable type parser.

### Review Evidence
| Field | Value |
|-------|-------|
| Gap Source | philosophy-deriver |
| Derived Task | "Create dedicated ARG condition parser for bare variable format" |
| Comparison Result | "No ARG parser exists in LogicalOperatorParser dispatch (13 prefix-based parsers, 0 bare variable parsers)" |
| DEFER Reason | "ARG requires dedicated regex (not PREFIX: format); cannot reuse VariableConditionParser<TRef>" |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/dotnet/ErbParser/VariableConditionParser.cs | Existing parser requires PREFIX: format, cannot handle ARG |
| src/tools/dotnet/ErbParser/LogicalOperatorParser.cs | Dispatch chain needs ARG parser registration |
| Game/ERB/口上/1_美鈴/KOJO_K1_EVENT.ERB | Example of ARG conditions in kojo EVENT files |

### Parent Review Observations
F758 initially characterized ARG as "stateful" requiring call-context analysis. Investigation revealed ARG values are well-documented integer enumerations (0-5), making them fully parseable as static conditions. The "stateful" characterization was overstated.

<!-- fc-phase-1-completed -->

---

## Background

### Philosophy (Mid-term Vision)
(Inherited from F758) Continue toward full equivalence testing by resolving condition categories that the current parsing pipeline cannot handle. Each variable type must have a dedicated parser, ICondition representation, YAML serialization, and KojoComparer evaluation path.

### Problem (Current Issue)
94 ARG conditions in kojo EVENT files (`IF ARG == 2`, `IF LOCAL:1 && ARG`) cannot be parsed because `VariableConditionParser<TRef>` requires a `PREFIX:` colon-separated format (`src/tools/dotnet/ErbParser/VariableConditionParser.cs:19`), but ARG is a bare identifier with no prefix delimiter. The `LogicalOperatorParser` dispatch chain (`src/tools/dotnet/ErbParser/LogicalOperatorParser.cs:162-207`) tries TALENT, 13 variable parsers, and function parsers — none match bare ARG, so `ParseAtomicCondition()` returns null. Additionally, all 94 occurrences exist exclusively in EVENT functions that use imperative IF/PRINT flow (not DATALIST/PRINTDATA blocks), which `FileConverter` (`src/tools/dotnet/ErbToYaml/FileConverter.cs:69-80`) does not convert conditions for. The F758 characterization of ARG as "stateful" is overstated: ARG values are well-documented integer enumerations (0-5), making them fully parseable as static conditions.

### Goal (What to Achieve)
Create a dedicated ARG condition parser (`ArgConditionParser`), `ArgRef` ICondition type, YAML serialization, and `KojoBranchesParser` evaluation support so that all 4 ARG condition patterns are parseable, serializable, and evaluable. Of these, 2 patterns (bare `ARG`, `ARG == N`) cover 10 real corpus occurrences as standalone conditions; 2 patterns (`ARG:N`, `ARG:N == M`) are forward-compatible support with 0 current corpus matches (Technical Constraints: No indexed ARG in conditions). The remaining 84/94 are compound conditions with LOCAL (requiring F761). Scope is parser-pipeline only; EVENT function conversion to YAML is deferred to F764.

<!-- fc-phase-2-completed -->

---

## Root Cause Analysis

### 5 Whys

1. **Symptom**: 94 ARG conditions in kojo return null from `ConditionExtractor.Extract()` (`src/tools/dotnet/ErbParser/ConditionExtractor.cs:29`)
2. **Why**: `ParseAtomicCondition()` tries TALENT parser, 13 variable parsers, and function parser — none match bare ARG (`src/tools/dotnet/ErbParser/LogicalOperatorParser.cs:162-207`)
3. **Why**: All variable parsers use regex `^{prefix}:(?:([^:]+):)?([^:\s&]+)` requiring a `PREFIX:` colon format (`src/tools/dotnet/ErbParser/VariableConditionParser.cs:19`)
4. **Why**: ARG has no colon prefix — its colon (ARG:N) is an index separator, not a prefix delimiter (`Game/ERB/口上/1_美鈴/KOJO_K1_EVENT.ERB:21`)
5. **Why**: The parsing pipeline was designed for PREFIX:target:name game-state variables; function parameters like ARG were architecturally excluded and deferred from F758 scope (`pm/features/feature-758.md:26`)

### Symptom vs Root Cause

| Layer | Description |
|-------|-------------|
| **Symptom** | 94 ARG conditions unparseable |
| **Proximate Cause** | No parser registered for bare ARG format in LogicalOperatorParser dispatch |
| **Root Cause** | VariableConditionParser regex requires PREFIX: colon format; ARG uses bare identifier without prefix; pipeline dispatch has no fallback for non-prefixed variables |
| **Structural Factor** | All 94 conditions exist in EVENT functions (non-DATALIST), outside current FileConverter scope |

---

## Related Features

| Feature | Status | Relationship |
|---------|--------|--------------|
| F758 | [DONE] | Parent — generic VariableConditionParser pattern, deferred ARG |
| F757 | [DONE] | VariableConditionParser generic base implementation |
| F761 | [PROPOSED] | Co-dependent — LOCAL parsing required for 84/94 compound conditions |
| F706 | [BLOCKED] | Consumer — full equivalence testing |

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| ArgConditionParser creation | FEASIBLE | 4 simple regex patterns, follows F758 precedent |
| ArgRef ICondition integration | FEASIBLE | Standard JsonDerivedType addition |
| YAML representation | FEASIBLE | Mirrors existing variable condition format |
| KojoBranchesParser evaluation | FEASIBLE | Standard evaluation pattern with state dict |
| EVENT function conversion | NEEDS_REVISION | FileConverter skips EVENT IF blocks; requires pipeline extension |
| Call-context test injection | FEASIBLE | Test scenarios inject ARG values into state dict |

**Verdict**: FEASIBLE for parser + YAML + evaluation pipeline. EVENT function conversion is NEEDS_REVISION and deferred.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F758 | [DONE] | Provides generic parser pattern to follow |
| Related | F761 | [DONE] | LOCAL parsing — 84/94 (89.4%) compound conditions include LOCAL |
| Related | F706 | [DONE] | Consumer of parsed conditions for equivalence testing |

---

## Impact Analysis

| Component | Impact |
|-----------|--------|
| `src/tools/dotnet/ErbParser/` | New ArgRef type + ArgConditionParser + dispatch registration |
| `src/tools/dotnet/ErbToYaml/` | DatalistConverter ARG handling in ConvertConditionToYaml |
| `src/tools/dotnet/KojoComparer/` | KojoBranchesParser ARG evaluation |
| `tools/ErbParserTests/` | Unit tests for parser |
| `tools/ErbToYamlTests/` | Unit tests for converter |
| `tools/KojoComparerTests/` | Unit tests for evaluator |

---

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| No prefix colon in ARG | ERA language syntax | Cannot reuse VariableConditionParser regex; dedicated parser required |
| ARG:N colon is index separator | ERA syntax (`ARG:0`, `ARG:1`) | Different semantics from `PREFIX:name` — index vs prefix |
| All 94 in EVENT (non-DATALIST) | Corpus analysis (grep confirmed 0 DATALIST in EVENT files) | Pipeline never encounters these conditions in current conversion |
| 84/94 compound with LOCAL | Corpus (`IF LOCAL:1 && ARG == N`) | F761 required for 89.4% of conditions; standalone coverage is 10/94 |
| ArgRef fields differ from VariableRef | VariableRef has Target/Name/Index; ARG needs only Index/Operator/Value | Design decision: new class vs VariableRef with null Target/Name |
| Integer-only values | Corpus: ARG values are 0-5 | Operator/Value always integer comparison |
| No indexed ARG in conditions | Grep `IF.*ARG:\d+` returned 0 | `ARG:N` form never appears in IF conditions (only in function signatures) |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| 84/94 conditions unparseable without F761 | HIGH | HIGH | Document limitation; implement F761 first or accept 10/94 standalone coverage |
| EVENT conversion requires FileConverter rework | HIGH | HIGH | Scope to parser-only; EVENT conversion deferred to separate feature |
| ArgRef vs VariableRef hierarchy mismatch | MEDIUM | LOW | Create standalone ArgRef (simpler) or extend VariableRef with nullable fields |
| Bare `IF ARG` boolean ambiguity | LOW | LOW | `IF ARG` means `ARG != 0`; handled by existing null-operator truthy logic |
| SELECTCASE ARG out of scope | MEDIUM | LOW | Tracked in Mandatory Handoffs; 7 occurrences in WC系口上, DRAFT created via POST-LOOP |

---

## AC Design Constraints

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | 4 syntactic patterns | Corpus analysis | Must test: bare `ARG`, `ARG == N`, `ARG:N`, `ARG:N == M` |
| C2 | Cannot reuse VariableConditionParser | Regex requires PREFIX: | Dedicated ArgConditionParser class |
| C3 | 84/94 include LOCAL:1 | Grep analysis | Compound condition ACs depend on F761 |
| C4 | Not in DATALIST blocks | Corpus (0 DATALIST in EVENT files) | Unit tests use synthetic condition strings, not file conversion |
| C5 | Integer values only | Corpus: 0-5 | Expected values are integers |
| C6 | State key format convention | Existing convention | `"ARG:0"` for bare ARG (index 0), `"ARG:1"` for indexed ARG:1 |
| C7 | ArgRef serialization | ICondition JsonDerivedType | Must verify JSON round-trip |
| C8 | KojoBranchesParser VariablePrefixes | `KojoBranchesParser.cs:18-21` | Must add "ARG" to allowlist |

<!-- fc-phase-3-completed -->

---

## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Each variable type **must** have a dedicated parser" | ArgConditionParser class must exist with Parse method | AC#1, AC#2 |
| "Each variable type **must** have ... ICondition representation" | ArgRef class implementing ICondition with JsonDerivedType | AC#3, AC#4 |
| "Each variable type **must** have ... YAML serialization" | DatalistConverter must handle ArgRef in ConvertConditionToYaml via dedicated case branch | AC#5 |
| "Each variable type **must** have ... KojoComparer evaluation path" | KojoBranchesParser must include "ARG" in VariablePrefixes and ValidateConditionScope | AC#6, AC#7 |
| "**all** 4 ARG condition patterns ... are parseable" | Parser must handle bare ARG, ARG == N, ARG:N, ARG:N == M | AC#2 |
| "Continue toward **full** equivalence testing" | LogicalOperatorParser dispatch must include ARG parser (infrastructure-only; actual equivalence testing execution blocked until EVENT conversion feature F764 is created) | AC#8 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ArgConditionParser.cs exists | file | Glob(src/tools/dotnet/ErbParser/ArgConditionParser.cs) | exists | - | [x] |
| 2 | ArgConditionParser parses all 4 patterns | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~ArgConditionParserTests | succeeds | - | [x] |
| 3 | ArgRef.cs exists | file | Glob(src/tools/dotnet/ErbParser/ArgRef.cs) | exists | - | [x] |
| 4 | ArgRef registered as JsonDerivedType on ICondition | code | Grep(src/tools/dotnet/ErbParser/ICondition.cs) | contains | "JsonDerivedType(typeof(ArgRef), typeDiscriminator: \"arg\")" | [x] |
| 5 | DatalistConverter ConvertConditionToYaml handles ArgRef | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~ArgConditionConversionTests | succeeds | - | [x] |
| 6 | KojoBranchesParser VariablePrefixes includes ARG | code | Grep(src/tools/dotnet/KojoComparer/KojoBranchesParser.cs) | contains | "\"ARG\"" | [x] |
| 7 | KojoBranchesParser evaluates ARG conditions | test | dotnet test tools/KojoComparer.Tests --filter FullyQualifiedName~ArgConditionEvaluationTests | succeeds | - | [x] |
| 8 | LogicalOperatorParser dispatch includes ARG parser | code | Grep(src/tools/dotnet/ErbParser/LogicalOperatorParser.cs) | matches | "\"ARG\".*argParser" | [x] |
| 9 | ArgRef JSON round-trip serialization | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~ArgRefSerializationTests | succeeds | - | [x] |
| 10 | Negative: parser rejects non-ARG input | code | Grep(src/tools/dotnet/ErbParser.Tests/ArgConditionParserTests.cs) | matches | "Parse_NullInput_ReturnsNull" | [x] |
| 11 | Build succeeds with zero warnings | build | dotnet build tools/ErbParser && dotnet build tools/ErbToYaml && dotnet build tools/KojoComparer | succeeds | - | [x] |
| 12 | Zero technical debt in modified files | code | Grep(src/tools/dotnet/ErbParser/ArgRef.cs,src/tools/dotnet/ErbParser/ArgConditionParser.cs,src/tools/dotnet/ErbToYaml/DatalistConverter.cs,src/tools/dotnet/KojoComparer/KojoBranchesParser.cs) | not_matches | "TODO\|FIXME\|HACK" | [x] |

### AC Details

**AC#1: ArgConditionParser.cs exists**
- Dedicated parser class (cannot reuse VariableConditionParser<TRef> per C2)
- File must be in src/tools/dotnet/ErbParser/ alongside other condition parsers
- Test: Glob pattern="src/tools/dotnet/ErbParser/ArgConditionParser.cs"

**AC#2: ArgConditionParser parses all 4 patterns (C1)**
- Tests must cover all 4 syntactic patterns per C1:
  - Bare `ARG` (truthy check, no operator) -> Index=0, Operator=null, Value=null
  - `ARG == N` (equality comparison) -> Index=0, Operator="==", Value="N"
  - `ARG:N` (indexed truthy) -> Index=N, Operator=null, Value=null
  - `ARG:N == M` (indexed comparison) -> Index=N, Operator="==", Value="M"
- Per C5: only integer values (0-5 in corpus)
- Per C6: bare ARG defaults to index 0
- Negative tests (AC#10) included in same test class: null input, empty string, non-ARG prefix (e.g., "CFLAG:1") return null

**AC#3: ArgRef.cs exists**
- New standalone class (not extending VariableRef, per C5 constraint: ArgRef only needs Index/Operator/Value, not Target/Name)
- Implements ICondition interface
- Properties: Index (int), Operator (string?), Value (string?)
- Test: Glob pattern="src/tools/dotnet/ErbParser/ArgRef.cs"

**AC#4: ArgRef registered as JsonDerivedType on ICondition (C7)**
- Must add `[JsonDerivedType(typeof(ArgRef), typeDiscriminator: "arg")]` to ICondition interface
- Enables polymorphic JSON serialization/deserialization
- Test: Grep pattern in src/tools/dotnet/ErbParser/ICondition.cs

**AC#5: DatalistConverter ConvertConditionToYaml handles ArgRef**
- ArgRef cannot use the existing `case VariableRef varRef` branch (ArgRef does not extend VariableRef)
- Must add dedicated `case ArgRef argRef:` branch in ConvertConditionToYaml switch
- Test class: ArgConditionConversionTests in tools/ErbToYaml.Tests
- Tests use synthetic ArgRef conditions (not file conversion, per C4)
- Verifies YAML output format: `{ "ARG": { "0": { "eq": "2" } } }`
- Note: No _variableTypePrefixes registration needed (ArgRef is standalone, not VariableRef subclass)

**AC#6: KojoBranchesParser VariablePrefixes includes ARG (C8)**
- Must add "ARG" to the VariablePrefixes string array
- This enables EvaluateCondition to dispatch ARG conditions to EvaluateVariableCondition
- Also enables ValidateConditionScope to accept ARG as a valid key
- Test: Grep for `"ARG"` in KojoBranchesParser.cs (contains check)

**AC#7: KojoBranchesParser evaluates ARG conditions**
- Test class: ArgConditionEvaluationTests in tools/KojoComparer.Tests
- Must verify branch selection with ARG conditions and state dictionary
- Per C6: state key format is "ARG:0" for bare ARG, "ARG:1" for indexed ARG:1
- Tests use synthetic YAML with ARG conditions (not converted from ERB, per C4)

**AC#8: LogicalOperatorParser dispatch includes ARG parser**
- Must register ArgConditionParser in _variableParsers list
- Placement: after existing 13 prefix-based parsers, before function parser fallback
- Test: Grep for `"ARG".*argParser` in LogicalOperatorParser.cs (matches check on registration line)

**AC#9: ArgRef JSON round-trip serialization (C7)**
- Test class: ArgRefSerializationTests in tools/ErbParser.Tests
- Must verify: serialize ArgRef to JSON via ICondition polymorphism, deserialize back, verify all properties preserved
- Verifies typeDiscriminator "arg" appears in serialized output
- Constraint C7: JSON round-trip is critical for YAML pipeline

**AC#10: Negative tests for parser**
- Verified by grep checking `Parse_NullInput_ReturnsNull` method exists in ArgConditionParserTests.cs
- Implementation must also include: Parse_EmptyString_ReturnsNull, Parse_NonArgPrefix_ReturnsNull (verified via test execution in AC#2)
- Ensures parser does not accidentally match other variable types

**AC#11: Build succeeds with zero warnings**
- All three affected projects must build cleanly (chained via `&&`)
- TreatWarningsAsErrors=true (Directory.Build.props) ensures zero-warning compliance
- Test: `dotnet build tools/ErbParser && dotnet build tools/ErbToYaml && dotnet build tools/KojoComparer`

**AC#12: Zero technical debt in modified files**
- All modified files must have no TODO/FIXME/HACK markers
- Scope: ArgRef.cs, ArgConditionParser.cs (new), DatalistConverter.cs, KojoBranchesParser.cs (modified)
- Verifies new files and prevents regression in existing modified files

### Goal Coverage Verification

| Goal Item | Covering AC(s) |
|-----------|----------------|
| Create ArgConditionParser | AC#1, AC#2, AC#10 |
| Create ArgRef ICondition type | AC#3, AC#4, AC#9 |
| YAML serialization (DatalistConverter) | AC#5 |
| KojoBranchesParser evaluation support | AC#6, AC#7 |
| All 4 ARG patterns parseable | AC#2 |
| LogicalOperatorParser dispatch integration | AC#8 |
| Build/quality | AC#11, AC#12 |

<!-- fc-phase-4-completed -->
<!-- fc-phase-5-completed -->

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 3 | Create ArgRef.cs with ICondition interface and JSON serialization | | [x] |
| 2 | 4 | Add JsonDerivedType attribute for ArgRef to ICondition.cs | | [x] |
| 3 | 1 | Create ArgConditionParser.cs with regex pattern for 4 ARG forms | | [x] |
| 4 | 8 | Register ArgConditionParser in LogicalOperatorParser dispatch chain | | [x] |
| 5 | 5 | Add ArgRef case branch to DatalistConverter ConvertConditionToYaml | | [x] |
| 6 | 6 | Add "ARG" to KojoBranchesParser VariablePrefixes array | | [x] |
| 7 | 2,9,10 | Write ArgConditionParserTests and ArgRefSerializationTests (4 patterns, JSON round-trip, negative cases) | | [x] |
| 8 | 5 | Write ArgConditionConversionTests (DatalistConverter YAML output) | | [x] |
| 9 | 7 | Write ArgConditionEvaluationTests (KojoBranchesParser evaluation) | | [x] |
| 10 | 11,12 | Build verification and technical debt check | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T6: Technical Design interfaces and implementation order | ArgRef, ArgConditionParser, integration into 3 pipeline components |
| 2 | implementer | sonnet | T7-T9: AC Details test specifications | Unit tests for parser, converter, evaluator |
| 3 | ac-tester | haiku | T10: Build commands and Grep patterns from ACs | Build verification + debt check results |

**Constraints** (from Technical Design):

1. **ArgRef is standalone** (not extending VariableRef) - Only needs Index/Operator/Value properties, not Target/Name
2. **Regex pattern order** - ArgConditionParser regex must handle all 4 forms: bare `ARG`, `ARG == N`, `ARG:N`, `ARG:N == M`
3. **Dispatch placement** - ArgConditionParser registers AFTER existing 13 variable parsers, BEFORE function parser
4. **State key format** - Bare `ARG` defaults to index 0 → state key `"ARG:0"`, indexed `ARG:1` → state key `"ARG:1"`
5. **Test strategy** - All tests use synthetic conditions (not file conversion) per AC Design Constraint C4
6. **DatalistConverter case branch** - ArgRef needs dedicated case (cannot reuse VariableRef branch if not extending it)

**Pre-conditions**:
- F758 completed (generic VariableConditionParser pattern established)
- src/tools/dotnet/ErbParser/ICondition.cs exists with 17 existing JsonDerivedType attributes
- src/tools/dotnet/ErbParser/LogicalOperatorParser.cs exists with 13 variable parsers registered
- src/tools/dotnet/ErbToYaml/DatalistConverter.cs exists with _variableTypePrefixes dictionary and ConvertConditionToYaml switch
- src/tools/dotnet/KojoComparer/KojoBranchesParser.cs exists with VariablePrefixes array (13 prefixes)

**Success Criteria**:
- All 12 ACs pass verification
- Zero build warnings (TreatWarningsAsErrors=true from Directory.Build.props)
- Zero technical debt markers (TODO/FIXME/HACK) in new files
- ArgConditionParser.Parse() handles all 4 patterns and rejects non-ARG input
- JSON round-trip serialization preserves ArgRef properties via ICondition polymorphism
- DatalistConverter produces correct YAML format: `{ "ARG": { "0": { "eq": "2" } } }`
- KojoBranchesParser evaluates ARG conditions using state dict keys "ARG:0", "ARG:1", etc.

**Implementation Order** (from Technical Design):

1. **T1**: Create `src/tools/dotnet/ErbParser/ArgRef.cs` with properties Index (int), Operator (string?), Value (string?), implementing ICondition
2. **T2**: Add `[JsonDerivedType(typeof(ArgRef), typeDiscriminator: "arg")]` to ICondition.cs
3. **T3**: Create `src/tools/dotnet/ErbParser/ArgConditionParser.cs` with regex `@"^ARG(?::(\d+))?(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$"`
4. **T4**: Register ArgConditionParser in LogicalOperatorParser._variableParsers list with entry `("ARG", argParser.Parse)`
5. **T5**: Add dedicated `case ArgRef argRef:` branch and `ConvertArgRef()` method in DatalistConverter.ConvertConditionToYaml switch
6. **T6**: Add `"ARG"` to KojoBranchesParser.VariablePrefixes array at line 17-21
7. **T7**: Write ArgConditionParserTests and ArgRefSerializationTests in tools/ErbParser.Tests
8. **T8**: Write ArgConditionConversionTests in tools/ErbToYaml.Tests
9. **T9**: Write ArgConditionEvaluationTests in tools/KojoComparer.Tests
10. **T10**: Run build verification and technical debt check

**Test Naming Convention**:
- Parser tests: `ArgConditionParserTests.Parse_{Pattern}_{ExpectedResult}()` format
- Serialization tests: `ArgRefSerializationTests.{Operation}_{Scenario}()` format
- Conversion tests: `ArgConditionConversionTests.ConvertArgRef_{Pattern}_ProducesCorrectYaml()` format
- Evaluation tests: `ArgConditionEvaluationTests.Evaluate_{Scenario}()` format

**Error Handling**:
- ArgConditionParser.Parse() returns `null` for null/empty input or non-matching patterns
- No exceptions thrown by parser (null return indicates non-match)
- DatalistConverter follows existing error handling pattern (ConvertConditionToYaml returns null for unhandled types)

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

---

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ARG conditions in kojo | `grep -rn "IF.*ARG" Game/ERB/口上/` | 94 occurrences | All in EVENT functions |
| Registered variable parsers | `grep -c "variableParsers" src/tools/dotnet/ErbParser/LogicalOperatorParser.cs` | 13 parsers | No ARG parser |
| JsonDerivedType on ICondition | `grep -c "JsonDerivedType" src/tools/dotnet/ErbParser/ICondition.cs` | 17 attributes | No ArgRef |
| VariablePrefixes count | `grep -c '"' src/tools/dotnet/KojoComparer/KojoBranchesParser.cs` (VariablePrefixes) | 13 prefixes | No ARG |

**Baseline File**: `.tmp/baseline-762.txt`

---

## Technical Design

### Approach

ARG is a bare variable (no `PREFIX:` format) representing function parameters, requiring a dedicated parser since `VariableConditionParser<TRef>` requires colon-prefixed input. We'll create:

1. **ArgRef** as a standalone ICondition type (not extending VariableRef) with properties: Index (int), Operator (string?), Value (string?)
2. **ArgConditionParser** with custom regex to handle 4 patterns: bare `ARG`, `ARG == N`, `ARG:N`, `ARG:N == M`
3. **Integration** into LogicalOperatorParser dispatch, DatalistConverter YAML serialization, and KojoBranchesParser evaluation

This approach follows F758 precedent for variable type expansion while accommodating ARG's unique syntax.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `src/tools/dotnet/ErbParser/ArgConditionParser.cs` with Parse method following F758 pattern |
| 2 | Write comprehensive unit tests covering all 4 patterns (bare ARG, ARG == N, ARG:N, ARG:N == M) plus negative cases in `ArgConditionParserTests` |
| 3 | Create `src/tools/dotnet/ErbParser/ArgRef.cs` implementing ICondition with Index/Operator/Value properties and JSON serialization attributes |
| 4 | Add `[JsonDerivedType(typeof(ArgRef), typeDiscriminator: "arg")]` to ICondition.cs interface |
| 5 | Add dedicated `case ArgRef argRef:` branch and `ConvertArgRef()` in DatalistConverter.ConvertConditionToYaml; test with `ArgConditionConversionTests` |
| 6 | Add "ARG" to VariablePrefixes string array in KojoBranchesParser.cs line 17-21 |
| 7 | Write `ArgConditionEvaluationTests` in KojoComparer.Tests using synthetic YAML with ARG conditions and state dict keys like "ARG:0", "ARG:1" |
| 8 | Add `("ARG", argParser.Parse)` tuple to `_variableParsers` list in LogicalOperatorParser constructor |
| 9 | Write `ArgRefSerializationTests` testing JSON round-trip via ICondition polymorphism with typeDiscriminator "arg" |
| 10 | Include negative tests in `ArgConditionParserTests`: null input, empty string, non-ARG prefix ("CFLAG:1", "LOCAL:1") all return null |
| 11 | Build all three projects (ErbParser, ErbToYaml, KojoComparer) with TreatWarningsAsErrors=true |
| 12 | Keep ArgConditionParser.cs and ArgRef.cs free of TODO/FIXME/HACK markers |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| ArgRef class hierarchy | A) Extend VariableRef with nullable Target/Name<br/>B) Standalone class with only Index/Operator/Value | B | VariableRef has Target/Name/Index fields designed for `PREFIX:target:name` format; ARG only needs Index. Extending VariableRef would leave Target/Name always null, violating Liskov Substitution Principle and adding confusion. Standalone class is cleaner. |
| Parser regex strategy | A) Reuse VariableConditionParser<ArgRef><br/>B) Dedicated ArgConditionParser with custom regex | B | VariableConditionParser requires `PREFIX:` format (`^{prefix}:...`); ARG has no prefix colon (AC Design Constraint C2). Custom parser is mandatory. |
| DatalistConverter integration | A) Extend VariableRef case branch<br/>B) Dedicated case branch for ArgRef | B | If ArgRef doesn't extend VariableRef, the existing `case VariableRef varRef` won't match. Need dedicated case with format: `case ArgRef argRef: return ConvertArgRef(argRef);` |
| State key format | A) `"ARG"` (no index)<br/>B) `"ARG:0"`, `"ARG:1"` (with index) | B | Follows existing convention ("TALENT:16", "CFLAG:美鈴_思慕獲得"). Bare `ARG` defaults to index 0 → state key `"ARG:0"`. Indexed `ARG:1` → state key `"ARG:1"`. |
| LogicalOperatorParser placement | A) Before existing variable parsers<br/>B) After existing 13 variable parsers, before function parser | B | Maintains existing dispatch order: TALENT → 13 prefix-based variables → ARG → functions. ARG is last variable parser since it's least specific (no prefix). |
| Test strategy | A) File conversion tests from EVENT files<br/>B) Synthetic condition tests | B | Per AC Design Constraint C4: all 94 ARG conditions are in EVENT functions (non-DATALIST), which FileConverter doesn't process. Synthetic tests (create ArgRef instances, test parser/converter/evaluator directly) are required. |

### Interfaces / Data Structures

#### ArgRef.cs
```csharp
using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to an ARG (function parameter) condition.
/// Pattern: ARG(:index)?( op value)?
/// Examples: ARG, ARG == 2, ARG:1, ARG:1 == 3
/// </summary>
public class ArgRef : ICondition
{
    /// <summary>
    /// Parameter index (default 0 for bare ARG).
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// Comparison operator: ==, !=, >, >=, <, <=, &amp;
    /// Null for truthy check (ARG or ARG:N without operator).
    /// </summary>
    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    /// <summary>
    /// Value to compare against (integer string).
    /// Null for truthy check.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
```

#### ArgConditionParser.cs
```csharp
using System.Text.RegularExpressions;

namespace ErbParser;

/// <summary>
/// Parser for ARG (function parameter) conditions.
/// Supports 4 patterns:
/// - ARG (truthy check, index 0)
/// - ARG == N (comparison, index 0)
/// - ARG:N (truthy check, explicit index)
/// - ARG:N == M (comparison, explicit index)
/// </summary>
public class ArgConditionParser
{
    private static readonly Regex Pattern = new(
        @"^ARG(?::(\d+))?(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$",
        RegexOptions.Compiled
    );

    /// <summary>
    /// Parse ARG condition string.
    /// Returns null if input is null, empty, or doesn't match ARG pattern.
    /// </summary>
    public ArgRef? Parse(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return null;

        condition = condition.Trim();
        var match = Pattern.Match(condition);

        if (!match.Success)
            return null;

        // Group 1: optional index (default 0)
        var index = match.Groups[1].Success && int.TryParse(match.Groups[1].Value, out var idx)
            ? idx
            : 0;

        // Group 2: operator (optional)
        var operatorValue = match.Groups[2].Success ? match.Groups[2].Value : null;

        // Group 3: value (optional)
        var value = match.Groups[3].Success ? match.Groups[3].Value.Trim() : null;

        return new ArgRef
        {
            Index = index,
            Operator = operatorValue,
            Value = value
        };
    }
}
```

#### LogicalOperatorParser.cs (constructor addition)
```csharp
public LogicalOperatorParser()
{
    var cflagParser = new VariableConditionParser<CflagRef>("CFLAG");
    // ... (existing 13 parsers)
    var palamParser = new VariableConditionParser<PalamRef>("PALAM");
    var argParser = new ArgConditionParser(); // NEW

    _variableParsers =
    [
        ("CFLAG", cflagParser.Parse),
        // ... (existing 13 entries)
        ("PALAM", palamParser.Parse),
        ("ARG", argParser.Parse), // NEW: after prefix-based parsers, before function parser
    ];
}
```

#### ICondition.cs (add JsonDerivedType)
```csharp
[JsonDerivedType(typeof(TalentRef), typeDiscriminator: "talent")]
// ... (existing 17 types)
[JsonDerivedType(typeof(PalamRef), typeDiscriminator: "palam")]
[JsonDerivedType(typeof(ArgRef), typeDiscriminator: "arg")] // NEW
public interface ICondition
{
}
```

#### DatalistConverter.cs (ConvertConditionToYaml case addition)
Note: No _variableTypePrefixes entry needed — ArgRef is standalone (not VariableRef subclass), so the existing `case VariableRef varRef when` branch cannot match it.
```csharp
private Dictionary<string, object>? ConvertConditionToYaml(ICondition condition)
{
    switch (condition)
    {
        case TalentRef talent:
            return ConvertTalentRef(talent);
        case VariableRef varRef when _variableTypePrefixes.ContainsKey(varRef.GetType()):
            return ConvertVariableRef(varRef);
        case ArgRef argRef: // NEW: dedicated case since ArgRef doesn't extend VariableRef
            return ConvertArgRef(argRef);
        case NegatedCondition negated:
            return ConvertNegatedCondition(negated);
        // ... (rest of cases)
    }
}

/// <summary>
/// Convert ArgRef to YAML format.
/// Pattern: { "ARG": { "0": { "eq": "2" } } }
/// </summary>
private Dictionary<string, object> ConvertArgRef(ArgRef argRef)
{
    var key = argRef.Index.ToString();
    return new Dictionary<string, object>
    {
        { "ARG", new Dictionary<string, object>
            {
                { key, MapErbOperatorToYaml(argRef.Operator, argRef.Value) }
            }
        }
    };
}
```

#### KojoBranchesParser.cs (VariablePrefixes addition)
```csharp
private static readonly string[] VariablePrefixes =
[
    "CFLAG", "TCVAR", "EQUIP", "ITEM", "STAIN",
    "MARK", "EXP", "NOWEX", "ABL", "FLAG", "TFLAG", "TEQUIP", "PALAM",
    "ARG" // NEW
];
```

### Test Structure

#### ErbParser.Tests/ArgConditionParserTests.cs
- `Parse_BareArg_ReturnsIndex0NoOperator()` - `"ARG"` → Index=0, Operator=null, Value=null
- `Parse_ArgWithComparison_ReturnsIndex0WithOperatorAndValue()` - `"ARG == 2"` → Index=0, Operator="==", Value="2"
- `Parse_IndexedArg_ReturnsIndexNoOperator()` - `"ARG:1"` → Index=1, Operator=null, Value=null
- `Parse_IndexedArgWithComparison_ReturnsIndexWithOperatorAndValue()` - `"ARG:1 == 3"` → Index=1, Operator="==", Value="3"
- `Parse_AllOperators_ReturnsCorrectOperator()` - Test ==, !=, >, >=, <, <=, &
- `Parse_NullInput_ReturnsNull()` (AC#10)
- `Parse_EmptyString_ReturnsNull()` (AC#10)
- `Parse_NonArgPrefix_ReturnsNull()` - "CFLAG:1", "LOCAL:1" return null (AC#10)

#### ErbParser.Tests/ArgRefSerializationTests.cs (AC#9)
- `Serialize_ArgRef_ContainsTypeDiscriminator()` - JSON contains `"$type": "arg"`
- `Deserialize_ArgRefJson_ReturnsCorrectProperties()` - Round-trip preserves Index/Operator/Value
- `Polymorphic_SerializeAsICondition_WorksCorrectly()` - Serialize via ICondition interface

#### ErbToYaml.Tests/ArgConditionConversionTests.cs (AC#5)
- `ConvertArgRef_BareArg_ProducesCorrectYaml()` - `{ "ARG": { "0": { "ne": "0" } } }`
- `ConvertArgRef_ArgWithComparison_ProducesCorrectYaml()` - `{ "ARG": { "0": { "eq": "2" } } }`
- `ConvertArgRef_IndexedArg_ProducesCorrectYaml()` - `{ "ARG": { "1": { "ne": "0" } } }`
- `ConvertArgRef_IndexedArgWithComparison_ProducesCorrectYaml()` - `{ "ARG": { "1": { "eq": "3" } } }`

#### KojoComparer.Tests/ArgConditionEvaluationTests.cs (AC#7)
- `Evaluate_ArgCondition_WithMatchingState_ReturnsTrue()` - State `{"ARG:0": 2}`, YAML condition `ARG == 2` → selects branch
- `Evaluate_ArgCondition_WithNonMatchingState_ReturnsFalse()` - State `{"ARG:0": 1}`, YAML condition `ARG == 2` → skips branch
- `Evaluate_IndexedArgCondition_WithMatchingState_ReturnsTrue()` - State `{"ARG:1": 3}`, YAML condition `ARG:1 == 3` → selects branch

### Implementation Order

1. **ArgRef.cs** (AC#3) - Standalone ICondition class
2. **ICondition.cs** (AC#4) - Add JsonDerivedType attribute
3. **ArgConditionParser.cs** (AC#1) - Parser with regex
4. **LogicalOperatorParser.cs** (AC#8) - Register parser in dispatch
5. **DatalistConverter.cs** (AC#5) - Add ConvertArgRef case branch
6. **KojoBranchesParser.cs** (AC#6) - Add "ARG" to VariablePrefixes
7. **Unit tests** (AC#2, AC#5, AC#7, AC#9, AC#10) - All test classes
8. **Build verification** (AC#11, AC#12) - Zero warnings, zero debt

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
- [resolved-applied] Phase2 iter1: [SCP-004] AC#8 test case 'Evaluate_CompoundCondition_LocalAndArg_ReturnsCorrectly' removed — requires F761 [DRAFT], belongs to F761 scope
- [resolved-applied] Phase2 iter1: [AC-005] Goal revised to distinguish 2 real corpus patterns from 2 forward-compatible patterns (0 corpus) and note 84/94 F761 dependency
- [resolved-applied] Phase2 iter2: [FMT-002] Implementation Contract table corrected from 6 columns to 5 columns (Tasks merged into Input)
- [resolved-applied] Phase2 iter1(FL2): [SCP-004] EVENT function conversion added to Mandatory Handoffs as F764 with Creation Task#12; SELECTCASE ARG assigned F765 with Creation Task#11
- [resolved-applied] Phase2 iter1(FL2): [FMT-003] Review Context restructured from single paragraph to template-mandated subsections (Origin, Identified Gap, Review Evidence, Files Involved, Parent Review Observations)
- [resolved-applied] Phase2 iter1(FL2): [SEM-001] Philosophy Derivation "full equivalence testing" clarified as infrastructure-only, blocked until EVENT conversion F764
- [resolved-applied] Phase2 iter2: [AC-006] Added AC#13-16 for DRAFT creation verification (file existence + index registration) per template DRAFT Creation Checklist
- [resolved-applied] Phase2 iter2: [FMT-005] Removed undefined [H] tag from Task#11/#12, left Tag column empty (KNOWN)
- [resolved-applied] Phase2 iter2: [FMT-006] Added --- separators between all major sections per user decision in POST-LOOP
- [resolved-applied] Phase2 iter3: [FMT-007] Success Criteria stale AC count (12→16)
- [resolved-applied] Phase2 iter3: [REF-001] Goal cross-reference corrected from "(Technical Constraint C7)" to "(Technical Constraints: No indexed ARG in conditions)"
- [resolved-applied] Phase2 iter3: [SCP-005] User chose Option B — removed T11/T12+AC#13-16 from feature. DRAFTs (F764, F765) will be created in FL POST-LOOP Step 6.3
- [resolved-applied] Phase3 iter4: [AC-007] AC#12 scope extended from src/tools/dotnet/ErbParser/ to all modified files (ArgRef.cs, ArgConditionParser.cs, DatalistConverter.cs, KojoBranchesParser.cs)
- [resolved-applied] Phase3 iter5: [ID-001] F763 ID conflict with F761 — reassigned SELECTCASE ARG handoff from F763 to F765 (F761 claims F763 for Dynamic LOCAL tracking)
- [resolved-skipped] Phase4 iter6: [AC-008] AC#12 regex `\|` — user confirmed skip. Pattern works correctly in Python re.search (`\|` = `|` alternation)
- [resolved-invalid] Phase3 iter9: Shared BareVariableConditionParser abstraction with F761 — ac-validator determined LOCAL and ARG have fundamentally different semantics (LocalRef.Index=int? with null, ArgRef.Index=int with default 0; LOCAL requires gate resolution pipeline). F761 itself chose standalone LocalRef despite identifying the shared gap. Independent parsers are the correct design.

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| SELECTCASE ARG (7 occurrences in WC系口上) | Out of scope — non-IF-condition context, requires separate SELECTCASE parsing pipeline | New Feature [DRAFT] | F765 | FL POST-LOOP Step 6.3 |
| EVENT function conversion pipeline | All 94 ARG conditions are in EVENT functions (non-DATALIST); parser pipeline has zero real-world callers until EVENT conversion exists | New Feature [DRAFT] | F764 | FL POST-LOOP Step 6.3 |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

---

## Links
- [feature-758.md](feature-758.md) - Parent feature (prefix-based variable type expansion)
- [feature-757.md](archive/feature-757.md) - VariableConditionParser generic base implementation
- [feature-761.md](feature-761.md) - LOCAL parsing (co-dependent, 84/94 compound conditions)
- [feature-706.md](feature-706.md) - Full equivalence testing (consumer)
- [feature-708.md](feature-708.md) - Compiler settings (TreatWarningsAsErrors)
