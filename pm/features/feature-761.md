# Feature 761: LOCAL Variable Condition Tracking

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

## Review Context (F758 POST-LOOP)
Deferred from F758 (Prefix-Based Variable Type Expansion). LOCAL variables (355 conditions in kojo) are stateful — their values depend on runtime assignment tracking within the current function scope. This requires a fundamentally different approach from prefix-based condition parsing.

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)
(Inherited from F758) The ErbParser/ErbToYaml/KojoComparer tool pipeline is the SSOT for ERB-to-YAML condition conversion. All condition types used in kojo must be parseable, convertible, and verifiable within this pipeline to achieve full equivalence testing (F706 goal). Scope: LOCAL variable conditions in `Game/ERB/口上/` kojo files.

### Problem (Current Issue)
LOCAL variable conditions in kojo (783 IF/ELSEIF references across 24 files — not 355 as F758 estimated) are completely unhandled by the tool pipeline. This is due to two compounding gaps:

1. **Parser gap**: LOCAL uses bare-identifier syntax (`LOCAL`, `LOCAL:1`) with no colon-prefix pattern. `VariableConditionParser<T>` requires a `{prefix}:` regex (`VariableConditionParser.cs:18`), so `LogicalOperatorParser.ParseAtomicCondition` (`LogicalOperatorParser.cs:162-208`) returns null for any LOCAL reference. No LOCAL parser is registered in the `_variableParsers` list (`LogicalOperatorParser.cs:30-45`).

2. **Structural gap**: LOCAL conditions exist exclusively in imperative function bodies as flow-control guards wrapping content blocks. They NEVER appear inside DATALIST/PRINTDATA blocks (zero matches for `PRINTDATA.*LOCAL|DATALIST.*LOCAL` across all kojo files). The ErbParser/ErbToYaml pipeline processes only DATALIST-internal conditions, so LOCAL-guarded content is architecturally invisible. Additionally, `FileConverter.ContainsConvertibleContent` (`FileConverter.cs:191-212`) checks only direct children of IfNode.Body, not nested structures — LOCAL-guarded functions containing nested PRINTDATA are silently skipped.

F758 deferred LOCAL as "stateful, requires runtime assignment tracking." However, investigation reveals that ~94% of LOCAL assignments in kojo are literal constants (`0`, `1`, or `-1`) — a deliberate content-authoring convention (`;記入チェック（=0, 非表示、1, 表示）` = "write check: 0=hidden, 1=displayed"). These are compile-time deterministic display gates, not runtime-stateful values. Only ~6% (~50 occurrences across 15 files) use non-literal assignments — function-call results (`GET_ABL_BRANCH`, `GET_EXP_BRANCH`, `MASTER_POSE`, `RAND`, ~35 occurrences) and variable references (`LOCAL:2 = TARGET`, 15 occurrences across 9 files) — that genuinely require runtime tracking, and several of those are currently dead code (commented as "将来: 別Feature"). Additionally, `ErbParser.cs:209` silently skips LOCAL assignment statements entirely — the AST has zero representation of assignments like `LOCAL = 0`, meaning the pipeline would need assignment recognition to resolve LOCAL values.

### Goal (What to Achieve)
Enable the ErbParser/ErbToYaml/KojoComparer pipeline to recognize, convert, and verify LOCAL variable conditions in kojo files. Specifically:
- Parse bare LOCAL and LOCAL:N identifiers as conditions
- Statically resolve constant (0/1) LOCAL gates: exclude LOCAL=0 dead-code sections from YAML output; strip LOCAL=1 gates and convert inner content
- Handle compound conditions containing LOCAL (e.g., `IF LOCAL:1 && TALENT:恋人`)
- Scope dynamic LOCAL (function-result assignments) as out-of-scope for this feature, tracked separately

#### 5 Whys

1. **Why**: 783 LOCAL references in kojo across 24 files are completely unhandled by the parsing pipeline.
2. **Why**: `LogicalOperatorParser.ParseAtomicCondition` (`LogicalOperatorParser.cs:162-208`) tries TALENT parsers, then variable parsers (CFLAG/TCVAR/etc), then function calls. `LOCAL` matches none because it has no prefix pattern.
3. **Why**: All variable parsers use `VariableConditionParser<T>` which requires a `{prefix}:` regex pattern (`VariableConditionParser.cs:18`). LOCAL is a bare identifier — no colon-separated prefix exists.
4. **Why**: LOCAL conditions operate at a different structural level than other conditions. They exist in imperative function bodies wrapping content blocks, not inside DATALIST/PRINTDATA. The pipeline was designed to process DATALIST-internal conditions only.
5. **Why (Root Cause)**: F758's scope analysis conflated two distinct usage patterns: (a) kojo-specific constant display gates (97%, statically analyzable) and (b) general ERB LOCAL usage (3%, truly runtime). The tools lack any handling for bare LOCAL identifiers regardless of static or dynamic nature, and the pipeline architecture does not reach the structural level where LOCAL conditions operate.

#### Symptom vs Root Cause

| | Description |
|---|---|
| **Symptom** | 783 LOCAL conditions in kojo are unhandled; F758 deferred them as "355 stateful conditions" |
| **Root Cause** | Bare-identifier parsing gap (no prefix pattern) combined with structural pipeline mismatch (function-body level vs DATALIST level). The "stateful" characterization is misleading — ~94% are static constants. Additionally, `ErbParser.cs:209` silently skips LOCAL assignments, so the AST has no assignment representation |

| Feature | Status | Relationship | Description |
|---------|--------|--------------|-------------|
| F758 | [DONE] | Predecessor | Established VariableConditionParser pattern; explicitly deferred LOCAL |
| F757 | [DONE] | Predecessor | FunctionCall passthrough — needed for GET_ABL_BRANCH/RAND LOCAL assignments |
| F762 | [DONE] | Sibling | ARG bare variables share identical bare-identifier parsing gap and structural challenge |
| F764 | [PROPOSED] | Related | EVENT Function Conversion Pipeline — shares AssignmentNode AST type. Property naming coordinated: both use `Target` |
| F706 | [BLOCKED] | Downstream | Full equivalence verification — needs all condition types resolved |

| Criterion | Assessment | Evidence |
|-----------|:----------:|---------|
| LOCAL parser implementation | FEASIBLE | Bare-identifier pattern is simpler than prefix patterns; no complex regex needed. New `LocalConditionParser` + `LocalRef` type |
| Static gate resolution (97%) | FEASIBLE | Constant 0/1 assignment detection is straightforward static analysis. LOCAL=0 → exclude section, LOCAL=1 → strip gate |
| FileConverter recursive depth | FEASIBLE | `ContainsConvertibleContent` (`FileConverter.cs:191-212`) needs recursive traversal — well-defined fix |
| Compound condition decomposition | FEASIBLE with design decisions | `IF LOCAL:1 && TALENT:恋人` requires splitting LOCAL from parseable condition. Currently all compound LOCAL:1 conditions are dead code (LOCAL:1=0 assigned before) |
| Dynamic LOCAL (function-result) | OUT OF SCOPE | ~30-50 occurrences across 14 files (GET_ABL_BRANCH/GET_EXP_BRANCH/MASTER_POSE/RAND). Several are explicitly dead code. Requires runtime tracking — separate feature |
| YAML representation | NEEDS DESIGN | LOCAL=0 sections should be excluded (dead code). LOCAL=1 sections need gate stripping. Pre-processing filter rather than YAML condition type |
| KojoComparer evaluation | FEASIBLE | State initialization for LOCAL constants; new state key format needed |

**Verdict**: NEEDS_REVISION — The feature is feasible for the static ~94% (constant LOCAL gates). The scope must be revised to clearly separate static gate resolution (this feature) from dynamic LOCAL tracking (future feature). The "stateful" framing from F758 should be corrected.

### Impact Analysis

| Component | Impact | Description |
|-----------|--------|-------------|
| `src/tools/dotnet/ErbParser/` | HIGH | New LocalRef type, LocalConditionParser, AssignmentNode, LogicalOperatorParser registration, ErbParser multi-method assignment recognition, ICondition.cs JsonDerivedType |
| `src/tools/dotnet/ErbToYaml/` | HIGH | FileConverter recursive depth fix, LocalGateResolver dead-code elimination and gate stripping |
| `src/tools/dotnet/KojoComparer/` | MEDIUM | KojoBranchesParser LOCAL evaluation (VariablePrefixes addition), test scenario state initialization |
| `Game/ERB/口上/` | READ-ONLY | 24 kojo files with LOCAL conditions — source data, not modified |

### Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| LOCAL has no prefix:colon syntax | ERB language spec; `VariableConditionParser.cs:18` regex | Cannot use existing VariableConditionParser; needs dedicated bare-identifier parser |
| LOCAL conditions are in imperative function bodies, never inside DATALIST/PRINTDATA | Grep: zero DATALIST/PRINTDATA matches; `ErbParser.cs:209` skips function-body assignments | Must handle LOCAL at a higher structural level than current pipeline |
| LOCAL is a 1D array (LOCAL, LOCAL:0, LOCAL:1, ... LOCAL:999) | `VariableCode.cs` `__ARRAY_1D__` flag; `VariableScope.cs:36-38` | Parser must handle both bare `LOCAL` (implicit index 0) and indexed `LOCAL:N` |
| LOCAL scope is function-local | `VariableScope.cs:36-38` | Values do not persist across function calls; assignment-before-use is always local |
| AST has no function boundary markers (@-declarations discarded) | `ErbParser.cs:209` skips @-lines | LocalGateResolver single-pass walk does not reset between functions; safe for current kojo patterns where each function resets LOCAL at start |
| Bare `LOCAL` in IF means truthiness (`LOCAL != 0`) | ERB semantics | Parser must handle implicit truthiness check, not just comparison operators |
| `ContainsConvertibleContent` is non-recursive | `FileConverter.cs:191-212` | LOCAL-guarded PRINTDATA sections are silently skipped — data loss bug |
| All 360 LOCAL:1 assignments in kojo are constants (0 or 1) | Grep evidence | LOCAL:1 can be fully resolved statically |
| KojoComparer has pre-existing 1 failure (F758 C3) | F758 completion state | AC matchers for KojoComparer should use `gte` not `equals` |
| ErbParser silently skips LOCAL assignment statements | `ErbParser.cs:209` ("Skip other lines") | AST has zero assignment representation; pipeline needs assignment recognition to resolve LOCAL values |
| LOCAL-guarded IfNodes have no ELSEIF/ELSE branches | Verified: zero cases in kojo | LocalGateResolver preserves IfNode unchanged if ElseIfBranches or ElseBranch exist (safe fallback) |

### Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Scope creep from dynamic LOCAL (function-result assignments) | HIGH | HIGH | Explicitly scope out dynamic LOCAL (3%); define clear boundary in ACs |
| F762 (ARG) shares identical bare-identifier gap — duplicated effort | HIGH | MEDIUM | F762 review determined independent parsers are correct (different semantics: LocalRef.Index=int? with null, ArgRef.Index=int with default 0; LOCAL requires gate resolution). Standalone LocalConditionParser is the correct design |
| Count discrepancy (F758's 355 vs actual 783) propagating to ACs | MEDIUM | MEDIUM | Use verified count (783) in all documentation and ACs |
| Compound LOCAL+condition decomposition complexity | MEDIUM | MEDIUM | All compound LOCAL:1 conditions are currently dead code (LOCAL:1=0 before use); phase if needed |
| Silent data loss from non-recursive ContainsConvertibleContent | HIGH | HIGH | Fix as part of this feature; verify with regression tests |
| Over-engineering for patterns that are effectively dead code | MEDIUM | LOW | Focus on LOCAL=0 exclusion and LOCAL=1 gate stripping; skip complex cases |

<!-- fc-phase-2-completed -->

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "the SSOT for ERB-to-YAML condition conversion" | Tool pipeline must handle LOCAL conditions end-to-end (parse → convert → verify) | AC#1-AC#13 |
| "All condition types used in kojo must be parseable" | Static LOCAL and LOCAL:N (~94%) must be parseable as conditions by ErbParser. Dynamic LOCAL (~6%, function-result assignments) deferred to F763 | AC#1, AC#2, AC#3, AC#4 |
| "All condition types must be ... convertible" | LOCAL gates must be converted: LOCAL=0 excluded, LOCAL=1 stripped, content preserved | AC#6, AC#7, AC#8 |
| "All condition types must be ... verifiable" | Static LOCAL verifiability achieved at LocalGateResolver level (dead-code exclusion, gate stripping, compound decomposition) | AC#8, AC#9, AC#17 |
| "full equivalence testing" | Pipeline must handle compound LOCAL conditions and recursive structures | AC#5, AC#9 |
| Gap: dynamic LOCAL (~6%) | Function-result LOCAL assignments (GET_ABL_BRANCH, RAND, etc.) require runtime tracking | Deferred to F763 |
| Gap: KojoComparer LOCAL evaluation | Required for F763 dynamic LOCAL where gates cannot be statically resolved. F761 pre-strips all static LOCAL gates before YAML, so KojoComparer LOCAL evaluation is not triggered by F761 pipeline | AC#10 (F763-prep infrastructure) |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | LocalRef type exists (Pos) | file | Glob(src/tools/dotnet/ErbParser/LocalRef.cs) | exists | - | [x] |
| 2 | LocalConditionParser parses bare LOCAL (Pos) | test | dotnet test src/tools/dotnet/ErbParser.Tests/ --filter DisplayName~BareLocal | succeeds | - | [x] |
| 3 | LocalConditionParser parses LOCAL:N indexed form (Pos) | test | dotnet test src/tools/dotnet/ErbParser.Tests/ --filter DisplayName~IndexedLocal | succeeds | - | [x] |
| 4 | LocalConditionParser rejects non-LOCAL input (Neg) | test | dotnet test src/tools/dotnet/ErbParser.Tests/ --filter DisplayName~RejectsNonLocal | succeeds | - | [x] |
| 5 | LogicalOperatorParser handles LOCAL in compound conditions (Pos) | test | dotnet test src/tools/dotnet/ErbParser.Tests/ --filter DisplayName~CompoundLocal | succeeds | - | [x] |
| 6 | ErbParser recognizes LOCAL assignment statements (Pos) | test | dotnet test src/tools/dotnet/ErbParser.Tests/ --filter FullyQualifiedName~LocalAssignment | succeeds | - | [x] |
| 7 | FileConverter recursive ContainsConvertibleContent (Pos) | test | dotnet test src/tools/dotnet/ErbToYaml.Tests/ --filter FullyQualifiedName~RecursiveConvertible | succeeds | - | [x] |
| 8 | LOCAL=0 dead-code sections excluded from YAML output (Pos) | test | dotnet test src/tools/dotnet/ErbToYaml.Tests/ --filter DisplayName~LocalGateDeadCode | succeeds | - | [x] |
| 9 | LOCAL=1 gate stripped, inner content preserved in YAML (Pos) | test | dotnet test src/tools/dotnet/ErbToYaml.Tests/ --filter DisplayName~LocalGateStrip | succeeds | - | [x] |
| 10 | KojoComparer evaluates LOCAL conditions (Pos) | test | dotnet test src/tools/dotnet/KojoComparer.Tests/ --filter DisplayName~Local | succeeds | - | [x] |
| 11 | All ErbParser tests pass (regression) | test | dotnet test src/tools/dotnet/ErbParser.Tests/ | gte | 140 | [x] |
| 12 | All ErbToYaml tests pass (regression) | test | dotnet test src/tools/dotnet/ErbToYaml.Tests/ | gte | 100 | [x] |
| 13 | Zero technical debt in new files | code | Grep(src/tools/dotnet/ErbParser/LocalRef.cs,src/tools/dotnet/ErbParser/LocalConditionParser.cs,src/tools/dotnet/ErbParser/Ast/AssignmentNode.cs,src/tools/dotnet/ErbToYaml/LocalGateResolver.cs) | not_matches | TODO\|FIXME\|HACK | [x] |
| 14 | F763 DRAFT file exists | file | Glob(pm/features/feature-763.md) | exists | - | [x] |
| 15 | F763 registered in index-features.md | file | Grep(pm/index-features.md) | contains | "763" | [x] |
| 16 | ICondition.cs has JsonDerivedType for LocalRef | code | Grep(src/tools/dotnet/ErbParser/ICondition.cs) | contains | "JsonDerivedType(typeof(LocalRef)" | [x] |
| 17 | Compound LOCAL:1 gate decomposition preserves remaining condition (Pos) | test | dotnet test src/tools/dotnet/ErbToYaml.Tests/ --filter DisplayName~CompoundLocalGate | succeeds | - | [x] |
| 18 | All KojoComparer tests pass (regression) | test | dotnet test src/tools/dotnet/KojoComparer.Tests/ | gte | 109 | [x] |

### AC Details

**AC#1: LocalRef type exists**
- Test: Glob pattern=`src/tools/dotnet/ErbParser/LocalRef.cs`
- Expected: File exists
- Rationale: LOCAL requires a dedicated AST type because it cannot reuse VariableRef (no Target/Name semantics — LOCAL is a bare identifier with optional array index). LocalRef implements ICondition with an Index property (null = implicit index 0) and optional Operator/Value for comparison forms.
- Constraint: C3 (LOCAL:N is array-indexed; Index property must support this)

**AC#2: LocalConditionParser parses bare LOCAL**
- Test: `dotnet test src/tools/dotnet/ErbParser.Tests/ --filter DisplayName~BareLocal`
- Expected: Test verifies parsing of `LOCAL`, `LOCAL == 1`, `LOCAL != 0` patterns. Test method names must contain `BareLocal` (e.g., `ParsesBareLocal`, `ParsesBareLocalWithOperator`).
- Rationale: Bare `LOCAL` is the most common form (~423 occurrences). Must return LocalRef with Index=null (implicit 0). Bare `LOCAL` in IF means truthiness check (`LOCAL != 0`).
- Constraint: C7 (bare LOCAL means truthiness)

**AC#3: LocalConditionParser parses LOCAL:N indexed form**
- Test: `dotnet test src/tools/dotnet/ErbParser.Tests/ --filter DisplayName~IndexedLocal`
- Expected: Test verifies parsing of `LOCAL:1`, `LOCAL:1 == 0`, `LOCAL:2 != 1` patterns. Test method names must contain `IndexedLocal` (e.g., `ParsesIndexedLocal`, `ParsesIndexedLocalWithComparison`).
- Rationale: LOCAL:N is the array-indexed form. Parser must extract the index integer after the colon. All 360 LOCAL:1 assignments are constants.
- Constraint: C3 (LOCAL:N is array index, not separate variable)

**AC#4: LocalConditionParser rejects non-LOCAL input**
- Test: `dotnet test src/tools/dotnet/ErbParser.Tests/ --filter DisplayName~RejectsNonLocal`
- Expected: Test verifies null return for inputs like `CFLAG:0:100`, `TALENT:恋人`, `LOCALS`, empty string, null. Test method names must contain `RejectsNonLocal`.
- Rationale: Negative test — parser must not false-positive on unrelated inputs. `LOCALS` is a different variable type and must not be captured.
- Engine type requires both positive and negative tests.

**AC#5: LogicalOperatorParser handles LOCAL in compound conditions**
- Test: `dotnet test src/tools/dotnet/ErbParser.Tests/ --filter DisplayName~CompoundLocal`
- Expected: Test verifies parsing of `LOCAL:1 && TALENT:恋人` returns LogicalAnd with LocalRef and TalentRef. Test method names must contain `CompoundLocal` (e.g., `ParsesCompoundLocalWithTalent`, `ParsesCompoundLocalWithOperator`).
- Rationale: 349 compound LOCAL conditions exist in kojo (C8). Active compound LOCAL:1 conditions exist (C10). Targeted DisplayName filter ensures compound LOCAL tests actually exist and pass.
- Constraint: C8, C10

**AC#6: ErbParser recognizes LOCAL assignment statements**
- Test: `dotnet test src/tools/dotnet/ErbParser.Tests/ --filter FullyQualifiedName~LocalAssignment`
- Expected: Test verifies `LOCAL = 0`, `LOCAL = 1`, `LOCAL:1 = 0` produce assignment nodes in AST
- Rationale: ErbParser.cs:209 currently silently skips LOCAL assignments. Without assignment recognition, the pipeline cannot determine LOCAL values for static gate resolution. This is the prerequisite for dead-code elimination (AC#8) and gate stripping (AC#9). Must also cover assignment recognition in ParseIfBlock, ParseElseIfBranch, and ParseElseBranch, all of which have separate 'skip other statements' paths that currently silently skip LOCAL assignments.
- Constraint: C2 (~94% are literal constants that must be detected)

**AC#7: FileConverter recursive ContainsConvertibleContent**
- Test: `dotnet test src/tools/dotnet/ErbToYaml.Tests/ --filter FullyQualifiedName~RecursiveConvertible`
- Expected: Test verifies that nested IfNode structures containing PRINTDATA/DATALIST at depth > 1 are detected
- Rationale: Current implementation checks only direct children (`ifNode.Body.Any(n => n is PrintDataNode || n is DatalistNode)`). LOCAL-guarded PRINTDATA (IF LOCAL ... IF TALENT ... PRINTDATA) is at depth 2+ and silently skipped — a data loss bug.
- Constraint: C6 (non-recursive detection causes silent data loss)

**AC#8: LOCAL=0 dead-code sections excluded from YAML output**
- Test: `dotnet test src/tools/dotnet/ErbToYaml.Tests/ --filter DisplayName~LocalGateDeadCode`
- Expected: Test verifies that content guarded by `LOCAL = 0` followed by `IF LOCAL` is excluded from YAML conversion output. Must include: (1) simple LOCAL:0 exclusion, (2) compound dead-code exclusion where `LOCAL:1 = 0` followed by `IF LOCAL:1 && TALENT:恋人` excludes entire IfNode, (3) IF LOCAL before any assignment → IfNode preserved unchanged (unresolved = keep). Test method names must contain `LocalGateDeadCode` (e.g., `LocalGateDeadCode_ExcludesSection`, `LocalGateDeadCode_CompoundCondition`, `LocalGateDeadCode_UnresolvedPreserved`).
- Rationale: LOCAL=0 sections are intentionally disabled content (C4: `;記入チェック（=0, 非表示、1, 表示）`). Including them in YAML would produce incorrect output. Compound dead-code and unresolved edge cases must also be covered.
- Constraint: C4 (LOCAL=0 = dead code by convention), C2 (compound dead-code symmetry with AC#17)

**AC#9: LOCAL=1 gate stripped, inner content preserved in YAML**
- Test: `dotnet test src/tools/dotnet/ErbToYaml.Tests/ --filter DisplayName~LocalGateStrip`
- Expected: Test verifies that `LOCAL = 1` followed by `IF LOCAL` produces YAML containing the inner PRINTDATA content without the LOCAL condition wrapper. Also verifies `LOCAL = -1` gate stripped (non-zero = truthy). Test method names must contain `LocalGateStrip` (e.g., `LocalGateStrip_PreservesContent`, `LocalGateStrip_UnwrapsGate`, `LocalGateStrip_NegativeOneGate`).
- Rationale: LOCAL=1 gates are always-true conditions. The YAML output should contain the inner content directly, as if the LOCAL gate did not exist. The gate itself carries no semantic meaning for YAML.
- Constraint: C2, C4

**AC#10: KojoComparer evaluates LOCAL conditions**
- Test: `dotnet test src/tools/dotnet/KojoComparer.Tests/ --filter DisplayName~Local`
- Expected: LOCAL-specific tests pass (test method names must contain "Local")
- Rationale: KojoComparer must be able to evaluate LOCAL conditions during ERB/YAML equivalence comparison. A targeted filter ensures LOCAL evaluation tests actually exist and pass, rather than relying on a baseline count that could pass without implementation. T14 must create test scenarios with "Local" in the DisplayName.
- Constraint: C9 (pre-existing failures tracked separately by AC#10's targeted filter)

**AC#11: All ErbParser tests pass (regression)**
- Test: `dotnet test src/tools/dotnet/ErbParser.Tests/`
- Expected: >= 140 tests pass (baseline 140, post-F762)
- Rationale: New LOCAL parsing functionality must not break existing condition parsers. Regression gate ensures all pre-existing tests continue to pass.

**AC#12: All ErbToYaml tests pass (regression)**
- Test: `dotnet test src/tools/dotnet/ErbToYaml.Tests/`
- Expected: >= 100 tests pass (baseline 100, post-F762)
- Rationale: New LOCAL gate resolution and recursive depth fix must not break existing conversion logic. Regression gate ensures all pre-existing tests continue to pass.

**AC#13: Zero technical debt in new files**
- Test: Grep pattern=`TODO|FIXME|HACK` paths=`src/tools/dotnet/ErbParser/LocalRef.cs,src/tools/dotnet/ErbParser/LocalConditionParser.cs,src/tools/dotnet/ErbParser/Ast/AssignmentNode.cs,src/tools/dotnet/ErbToYaml/LocalGateResolver.cs`
- Expected: 0 matches
- Rationale: All new production files must not introduce technical debt markers. Covers all 4 new production files created by this feature.

**AC#14: F763 DRAFT file exists**
- Test: Glob pattern=`pm/features/feature-763.md`
- Expected: File exists
- Rationale: Mandatory Handoffs requires F763 DRAFT creation for dynamic LOCAL tracking. File existence verifies the handoff was executed.

**AC#15: F763 registered in index-features.md**
- Test: Grep pattern=`763` path=`pm/index-features.md`
- Expected: Contains "763"
- Rationale: Per DRAFT Creation Checklist, both file existence AND index registration must be verified.

**AC#16: ICondition.cs has JsonDerivedType for LocalRef**
- Test: Grep pattern=`JsonDerivedType(typeof(LocalRef)` path=`src/tools/dotnet/ErbParser/ICondition.cs`
- Expected: Contains match
- Rationale: F758 precedent — all ICondition implementors require `[JsonDerivedType(typeof(T), typeDiscriminator: "...")]` attribute on ICondition.cs for polymorphic JSON serialization. Missing registration causes silent type information loss during round-trip.

**AC#17: Compound LOCAL:1 gate decomposition**
- Test: `dotnet test src/tools/dotnet/ErbToYaml.Tests/ --filter DisplayName~CompoundLocalGate`
- Expected: Test verifies that `LOCAL:1 = 1` followed by `IF LOCAL:1 && FIRSTTIME` produces YAML with condition `FIRSTTIME` only (LOCAL:1 stripped from compound). Also verifies `LOCAL:1 = 1` followed by `IF LOCAL:1 && ARG == 2` produces condition `ARG == 2` only. Implementation Note: Compound decomposition uses string-level splitting on `&&`/`||` operators to preserve original condition text. No ICondition-to-string serializer needed.
- Rationale: Key Decision selects "Full decomposition". C10 confirms active compound LOCAL:1 conditions exist in KOJO_K9_口挿入.ERB and KOJO_K9_EVENT.ERB. Implementation Contract Constraint #7 requires it. Must verify decomposition produces correct remaining condition.
- Constraint: C10 (active compound LOCAL:1 conditions)

**AC#18: All KojoComparer tests pass (regression)**
- Test: `dotnet test src/tools/dotnet/KojoComparer.Tests/`
- Expected: >= 109 tests pass (baseline 109, post-F762, pre-existing 1 failure)
- Rationale: T14/T15 modify KojoBranchesParser (add LOCAL to VariablePrefixes, extend EvaluateCondition). Changes could break existing condition evaluation. Regression gate ensures all pre-existing tests continue to pass.

### Goal Coverage Verification

| Goal Item | AC Coverage |
|-----------|-------------|
| Parse bare LOCAL and LOCAL:N identifiers as conditions | AC#1, AC#2, AC#3, AC#4, AC#5, AC#16 |
| Statically resolve constant LOCAL gates: exclude LOCAL=0 | AC#6, AC#8 |
| Statically resolve constant LOCAL gates: strip LOCAL=1 and convert inner content | AC#6, AC#9 |
| Handle compound conditions containing LOCAL | AC#5, AC#17 |
| Scope dynamic LOCAL as out-of-scope, tracked separately | AC#14, AC#15 |
| FileConverter recursive depth fix (from Problem) | AC#7 |
| KojoComparer LOCAL evaluation (F763-prep infrastructure) | AC#10 |
| Regression safety | AC#11, AC#12, AC#18 |

<!-- fc-phase-4-completed -->
<!-- fc-phase-5-completed -->

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1 | Create LocalRef AST type with Index, Operator, Value properties | | [x] |
| 2 | 2,3,4 | Implement LocalConditionParser with bare and indexed form parsing | | [x] |
| 3 | 5 | Register LocalConditionParser in LogicalOperatorParser._variableParsers | | [x] |
| 4 | 2,3,4,5 | Write LocalConditionParser unit tests (positive, negative, compound) | | [x] |
| 5 | 6 | Create AssignmentNode AST type with Target and Value properties | | [x] |
| 6 | 6 | Extend ErbParser.ParseLines, ParseIfBlock, ParseElseIfBranch, ParseElseBranch to recognize LOCAL assignment statements | | [x] |
| 7 | 6 | Write LOCAL assignment recognition unit tests | | [x] |
| 8 | 7 | Rewrite FileConverter.ContainsConvertibleContent as recursive method | | [x] |
| 9 | 7 | Write recursive depth detection unit tests | | [x] |
| 10 | 8,9 | Implement LocalGateResolver core preprocessing (dead-code exclusion and simple gate stripping) | | [x] |
| 11 | 17 | Implement LocalGateResolver compound condition decomposition (strip LocalRef from LogicalOp, preserve remaining condition) | | [x] |
| 12 | 8,9 | Integrate LocalGateResolver into FileConverter.ConvertAsync | | [x] |
| 13 | 8,9 | Write LocalGateResolver unit tests (dead-code and gate stripping) | | [x] |
| 14 | 10 | Extend KojoBranchesParser.EvaluateCondition to handle LocalRef | | [x] |
| 15 | 10 | Add LOCAL state initialization to KojoComparer test scenarios | | [x] |
| 16 | 11,12 | Run all regression tests (ErbParser and ErbToYaml) | | [x] |
| 17 | 13 | Verify zero technical debt in new files | | [x] |
| 18 | 14,15 | Create F763 DRAFT for dynamic LOCAL tracking and register in index-features.md | | [x] |
| 19 | 16 | Add JsonDerivedType(typeof(LocalRef)) to ICondition.cs | | [x] |
| 20 | 17 | Write compound LocalGateResolver decomposition unit tests | | [x] |
| 21 | 18 | Run KojoComparer regression tests | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 (T1-T4,T19) | implementer | sonnet | LocalRef/LocalConditionParser specs from Technical Design | LocalRef.cs, LocalConditionParser.cs, registration, JsonDerivedType, unit tests |
| 2 (T5-T7) | implementer | sonnet | AssignmentNode specs from Technical Design | AssignmentNode.cs, ErbParser changes, unit tests |
| 3 (T8-T13,T20) | implementer | sonnet | FileConverter/LocalGateResolver specs from Technical Design | Recursive depth fix, LocalGateResolver core + compound decomposition, unit tests |
| 4 (T14-T15) | implementer | sonnet | KojoComparer specs from Technical Design | LocalRef evaluation, state initialization |
| 5 (T16-T18,T21) | ac-tester | haiku | AC commands from AC table | Regression test results (ErbParser, ErbToYaml, KojoComparer) and debt verification |

**Constraints** (from Technical Design):
1. LOCAL cannot reuse VariableRef — bare identifier with no Target/Name semantics requires dedicated LocalRef type
2. LocalRef.Index is int? (null = implicit index 0 for bare LOCAL)
3. Bare LOCAL in IF means truthiness check (LOCAL != 0), not literal boolean
4. LocalGateResolver only resolves literal constant assignments (0, 1, -1) — function-result assignments are out of scope
5. FileConverter.ContainsConvertibleContent must use recursive traversal to detect nested PRINTDATA (silent data loss bug fix)
6. KojoComparer state key format: "LOCAL:{index}" (e.g., "LOCAL:0", "LOCAL:1")
7. Active compound LOCAL:1 conditions exist — LocalGateResolver must decompose compound conditions when stripping LOCAL=1 gates (strip LocalRef, preserve remaining condition)

**Pre-conditions**:
- F758 (Prefix-Based Variable Type Expansion) is [DONE]
- F757 (FunctionCall Passthrough) is [DONE]
- Baseline measurements recorded: ErbParser (140 passed), ErbToYaml (100 passed), KojoComparer (109 passed, 1 failed)

**Success Criteria**:
- All ACs pass (AC#1-AC#18)
- LocalRef type exists and parses bare/indexed forms correctly
- LocalGateResolver excludes LOCAL=0 sections and strips LOCAL=1 gates
- FileConverter recursive depth fix detects nested PRINTDATA
- KojoComparer evaluates LOCAL conditions in equivalence tests
- All regression tests pass (gte baseline counts)
- Zero technical debt in new files

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

---

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ErbParser tests | `dotnet test src/tools/dotnet/ErbParser.Tests/` | 140 passed, 0 failed | Post-F762 baseline; new LOCAL tests will increase count |
| ErbToYaml tests | `dotnet test src/tools/dotnet/ErbToYaml.Tests/` | 100 passed, 0 failed | Post-F762 baseline; new LOCAL conversion tests will increase count |
| KojoComparer tests | `dotnet test src/tools/dotnet/KojoComparer.Tests/` | 109 passed, 1 failed, 3 skipped | Post-F762 baseline; pre-existing 1 failure (C9); use gte matcher |

**Baseline File**: `.tmp/baseline-761.txt`

---

## AC Design Constraints

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | LOCAL is NEVER inside DATALIST/PRINTDATA | All 3 investigations (zero grep matches) | ACs cannot test LOCAL as a DATALIST condition — must test imperative IF gate recognition |
| C2 | ~94% of LOCAL assignments are literal constants (0, 1, -1) | Grep: ~786 constant assignments out of ~837 total (bare + indexed) | Primary ACs should target static gate detection and resolution |
| C3 | LOCAL:N is an array index, not a separate variable | Engine `VariableCode.cs` `__ARRAY_1D__` flag | Parser must handle bare LOCAL, LOCAL:0, LOCAL:1, LOCAL:N |
| C4 | LOCAL=0 sections are dead code by author convention | `;記入チェック（=0, 非表示、1, 表示）` comment pattern (~341 occurrences) | AC should verify dead-code sections excluded from YAML output |
| C5 | Non-literal LOCAL assignments need separate feature | ~50 occurrences across 15 files: function calls (GET_ABL_BRANCH/GET_EXP_BRANCH/MASTER_POSE/RAND, ~35) and variable references (LOCAL:2=TARGET, 15 across 9 files) | ACs for dynamic LOCAL must be explicitly out of scope |
| C6 | ContainsConvertibleContent is non-recursive | `FileConverter.cs:191-212` | AC should verify recursive detection of LOCAL-guarded PRINTDATA |
| C7 | Bare `LOCAL` means truthiness (`LOCAL != 0`) | ERB semantics | AC must test implicit truthiness conversion |
| C8 | Compound conditions: `IF LOCAL:1 && other_condition` | 349 occurrences across 16 files | AC should address compound condition decomposition (or explicitly defer) |
| C9 | KojoComparer pre-existing 1 failure | Post-F762 state (Baseline: 109 passed, 1 failed) | Use `gte` matcher for KojoComparer pass counts |
| C10 | Most compound LOCAL:1 conditions have active paths (LOCAL:1=1 before compound IF) | KOJO_K9_口挿入.ERB (line 1694: LOCAL:1=1, line 1696: IF LOCAL:1 && FIRSTTIME), KOJO_K9_EVENT.ERB (line 28: LOCAL:1=1, line 30: IF LOCAL:1 && ARG==2) | AC must verify compound condition decomposition when LOCAL gate is stripped (e.g., IF LOCAL:1 && FIRSTTIME → IF FIRSTTIME) |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F758 (Prefix-Based Variable Types) | [DONE] | Provides VariableConditionParser pattern and tool pipeline architecture |
| Predecessor | F757 (FunctionCall Passthrough) | [DONE] | FunctionCall handling needed for GET_ABL_BRANCH/RAND detection |
| Related | F762 (ARG Bare Variables) | [DONE] | Shares bare-identifier parsing gap; design should consider shared solution |
| Related | F764 (EVENT Function Conversion Pipeline) | [PROPOSED] | Shares AssignmentNode AST type; property naming coordinated (both use `Target`) |
| Related | F706 (Full Equivalence Verification) | [BLOCKED] | Benefits from LOCAL condition resolution |

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
- [resolved-applied] Phase2-Uncertain iter2: [FMT-001] Section ordering deviates from template (Root Cause Analysis, Related Features, etc. inserted between Background and AC). These are /fc Phase 1-2 investigation outputs — may be standard workflow. Template-defined sections (Dependencies, Baseline) are also reordered.
- [resolved-applied] Phase2-Uncertain iter2: [AC-005] Philosophy absolute claim "All condition types must be parseable" vs Goal excluding dynamic LOCAL (~6%). Philosophy Derivation table maps absolute claim to AC#1-13 without acknowledging gap. → Qualified derivation row and added gap row for F763.
- [resolved-applied] Phase2-Uncertain iter4: [AC-006] AC#8 and AC#9 share identical Method filter (FullyQualifiedName~LocalGate). Not independently verifiable at AC table level. → Split to DisplayName~LocalGateDeadCode (AC#8) and DisplayName~LocalGateStrip (AC#9).
- [resolved-skipped] Phase2-Uncertain iter5: [CON-003] LocalGateResolver builds global LOCAL value map but LOCAL scope is function-local. Already documented in Technical Constraints + Known Limitation. All 24 kojo files verified to reset LOCAL at function start. Accepted as constraint.
- [resolved-applied] Phase2-Uncertain iter6: [CON-004] C10 is factually incorrect — compound LOCAL:1 conditions are NOT all dead code. KOJO_K9_口挿入.ERB (line 1694: LOCAL:1=1, line 1696: IF LOCAL:1 && FIRSTTIME), KOJO_K9_EVENT.ERB (line 28: LOCAL:1=1, line 30: IF LOCAL:1 && ARG==2) have active compound conditions. Also K4/K8/K10 character files. This invalidates Key Decision "structural parsing only" and Implementation Contract Constraint #7. LocalGateResolver design needs compound condition decomposition (strip LOCAL:1 from LogicalOp when value=1).
- [resolved-applied] Phase2-Valid iter7: [CON-001] Task#6 extends ParseLines (line 209) for LOCAL assignments, but LOCAL:1 assignments in kojo occur INSIDE IF blocks. ParseIfBlock (line 516-517), ParseElseIfBranch (line 706-707), ParseElseBranch (line 884-885) all have separate "skip other statements" paths. Task#6 and AC#6 must also cover these branch parsing methods.
- [resolved-applied] Phase2-Valid iter8: [CON-003] LocalGateResolver sequential reassignment — within single functions, LOCAL is reassigned 20+ times (0→1→0→...). Two-pass design (scan all → build map → walk AST) captures only last value. Must use single-pass sequential walk processing assignments and IF-checks in AST order.
- [resolved-applied] Phase2-Valid iter8: [INV-003] Technical Design uses `Node` as base class but actual codebase uses `AstNode`. AssignmentNode : Node should be : AstNode, List<AstNode> should be List<AstNode>. Following design literally produces compilation failures.
- [resolved-applied] Phase2-Valid iter8: [INV-003] IfNode.Condition is `string` (raw text), not `ICondition`/`LocalRef`. LocalGateResolver design assumes parsed condition objects. Resolver must call LogicalOperatorParser.ParseLogicalExpression(ifNode.Condition) before evaluating.
- [resolved-applied] Phase2-Valid iter9: [INV-004] KojoComparer Technical Design assumes ICondition type dispatch (case LocalRef local:) but actual KojoBranchesParser uses dictionary-based evaluation. EvaluateCondition takes dictionaries, not ICondition. Also ValidateConditionScope rejects unknown prefixes — need to add 'LOCAL' to VariablePrefixes/allowedKeys. T13-T14 and AC#10 design needs rewrite.
- [resolved-applied] Phase3-Maintainability iter9: Risk mitigation "Design shared bare-identifier parsing approach upfront; coordinate with F762" is not implemented. Technical Design creates standalone LocalConditionParser with no shared abstraction. F762 ARG needs identical pattern. Consider shared BareIdentifierConditionParser<TRef> base or generic. → F762 review determined independent parsers correct (different semantics). Risk mitigation text updated.
- [resolved-applied] Phase3-Maintainability iter9: LocalGateResolver has no DI injection point. → Added ILocalGateResolver interface + constructor injection in FileConverter. F763 can swap in dynamic resolver.
- [resolved-skipped] Phase2-Uncertain iter3: [FMT-002] Review Context free-text paragraph. Optional section, non-Step-6.3 origin. Accepted as minimal provenance note.
- [resolved-applied] Phase2-Uncertain iter3: [TST-001] LocalGateResolver design handles absent LOCAL keys correctly (Dictionary<string, int?> returns null → keep unchanged), but no explicit unit test covers "IF LOCAL before any assignment" edge case. → Added to AC#8 Expected: "IF LOCAL before any assignment → IfNode preserved unchanged (unresolved = keep)". Test name: LocalGateDeadCode_UnresolvedPreserved.
- [resolved-applied] Phase2-Uncertain iter6: [AC-007] AC#5 filter was broader than compound-LOCAL-only. → Changed to DisplayName~CompoundLocal targeted filter. Test methods must contain CompoundLocal in name.
- [resolved-skipped] Phase2-Uncertain iter9: [FMT-003] AC Design Constraints missing ### Constraint Details. Table inline info sufficient. F764 precedent (resolved-skipped).
- [resolved-applied] Phase2-Uncertain iter9: [TST-002] AC#8 tests simple LOCAL:0 exclusion but not compound LOCAL:0 exclusion (IF LOCAL:1 && TALENT where LOCAL:1=0 → entire IfNode excluded). → Added to AC#8 Expected: "compound dead-code exclusion where LOCAL:1=0 followed by IF LOCAL:1 && TALENT:恋人 excludes entire IfNode". Test name: LocalGateDeadCode_CompoundCondition.
- [resolved-applied] Phase2-Uncertain iter2(FL2): [DES-001] AC#10/T13/T14 design coherence: LocalGateResolver resolves all static LOCAL gates before YAML generation, so KojoComparer LOCAL evaluation is never triggered by real pipeline output within F761 scope. → Reclassified: Philosophy Derivation "verifiable" now maps to AC#8/AC#9/AC#17 (LocalGateResolver). AC#10 moved to new gap row as F763-prep infrastructure. Goal Coverage updated.
- [resolved-skipped] Phase3-Maintainability iter3(FL2): [DES-002] AssignmentNode.Target raw string. F764 shared design prioritized. String generality justified. Re-parse cost negligible (string.Split).
- [resolved-applied] Phase2-Uncertain iter7(FL2): [DES-003] F761 designated as canonical AssignmentNode.cs creator. F764 T1 updated with skip-if-exists note.
- [fix] Phase3-Maintainability iter1(FL3): [DES-004] Technical Design > LocalGateResolver > Resolve() — compound decomposition strategy undefined. Added: string-level splitting on &&/|| operators.
- [fix] Phase3-Maintainability iter1(FL3): [CON-005] Technical Constraints table — ELSEIF/ELSE behavior undefined. Added constraint: resolver preserves unchanged if they exist.
- [fix] Phase3-Maintainability iter1(FL3): [TST-003] AC#9 detail — LOCAL=-1 gate stripping not tested. Added test case and LocalGateStrip_NegativeOneGate name.
- [fix] Phase2-Review iter2(FL3): [INV-005] AC#18 detail + Technical Design AC Coverage — stale T13/T14 → T14/T15.
- [resolved-applied] Phase2-Uncertain iter2(FL3): [OPR-001] LocalGateResolver dead-code/decomposition constrained to && only. || compounds kept unchanged (zero kojo occurrences). Design updated to differentiate &&/|| in pseudocode.
- [fix] Phase2-Review iter3(FL3): [INV-006] Impact Analysis table — SSOT inconsistency. ErbToYaml/ErbParser/KojoComparer rows corrected.
- [fix] Phase2-Review iter3(FL3): [FMT-004] Technical Design AC Coverage table — missing AC#14/15/16. Added 3 rows.
- [fix] Phase2-Review iter3(FL3): [DES-005] LocalGateResolver Resolve() pseudocode — key construction ambiguity. Added explicit mapping step.
- [resolved-skipped] Phase2-Uncertain iter3(FL3): [CNT-001] C8 count discrepancy (349 vs ~344) is minor; no implementation impact. Bare LOCAL compounds are dead-code pattern, handled by gate exclusion.
- [fix] Phase2-Review iter4(FL3): [FMT-005] [fix] Review Notes entries reformatted with [{category-code}] per template format.
- [resolved-skipped] Phase2-Uncertain iter4(FL3): [FMT-006] Background sub-tables missing ### headers. /fc investigation output — formal improvement only, low value.
- [fix] Phase2-Review iter5(FL3): [INV-007] FileConverter.ProcessFile() → FileConverter.ConvertAsync() in Tasks T12, AC Coverage AC#8, Resolved Design Decisions. ProcessFile does not exist; actual entry point is ConvertAsync.
- [fix] Phase3-Maintainability iter6(FL3): [DES-006] LocalGateResolver Resolve() must recurse into IfNode.Body. Kojo pattern: LOCAL=1 → IF LOCAL (strip) → body has LOCAL:1=0 + IF LOCAL:1 (nested gate). After stripping outer gate, nested gates are unprocessed. Added recursive Resolve() calls on body contents for all IfNode branches.

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Dynamic LOCAL (function-result assignments, ~6%) | Requires runtime assignment tracking beyond static analysis | New Feature | F763 | Task#18 |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-09 12:02 | START | implementer | Phase 1 TDD RED (T1-T4, T19) | - |
| 2026-02-09 12:02 | END | implementer | Phase 1 TDD RED (T1-T4, T19) | SUCCESS |

---

## Technical Design

### Approach

This feature adds LOCAL variable condition handling through a **dedicated bare-identifier parser** and **static gate resolution** preprocessing. The implementation is split into three major components:

1. **ErbParser Enhancement**: New `LocalRef` AST node type and `LocalConditionParser` to handle bare `LOCAL` and indexed `LOCAL:N` patterns, plus assignment statement recognition
2. **ErbToYaml Preprocessing**: Static analysis to resolve constant LOCAL gates (LOCAL=0 → exclude section, LOCAL=1 → strip gate) and recursive depth fix for `ContainsConvertibleContent`
3. **KojoComparer State Management**: LOCAL state initialization and evaluation in the existing state conversion system

**Key Design Decision**: LOCAL cannot reuse `VariableRef` or `VariableConditionParser<T>` because it has fundamentally different syntax (bare identifier, no prefix:colon pattern). A dedicated `LocalRef` type implementing `ICondition` with optional `Index` property (null = implicit index 0) is required.

**Rationale**: This approach satisfies ACs by enabling end-to-end LOCAL handling (parse → convert → verify) while maintaining clean separation between:
- Parsing (structure recognition)
- Preprocessing (static gate resolution)
- Evaluation (runtime state management)

The design explicitly scopes out dynamic LOCAL (function-result assignments, ~3% of cases) as tracked in F763.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `src/tools/dotnet/ErbParser/LocalRef.cs` implementing `ICondition` with `Index` property (int?, null = implicit 0), optional `Operator`/`Value` for comparison forms |
| 2 | Implement `LocalConditionParser.Parse()` to handle bare `LOCAL` patterns: match regex `^LOCAL(?:\s*(!=\|==\|>=\|<=\|>\|<)\s*(.+))?$`, return `LocalRef` with `Index=null`. Bare LOCAL means truthiness (`LOCAL != 0`). Add unit tests to `src/tools/dotnet/ErbParser.Tests/LocalConditionParserTests.cs` |
| 3 | Extend `LocalConditionParser.Parse()` to handle indexed `LOCAL:N` patterns: match regex `^LOCAL:(\d+)(?:\s*(!=\|==\|>=\|<=\|>\|<)\s*(.+))?$`, extract index into `Index` property. Add unit tests covering `LOCAL:1`, `LOCAL:2`, comparison operators |
| 4 | Add negative test cases to `LocalConditionParserTests.cs`: verify null return for `CFLAG:0:100`, `TALENT:恋人`, `LOCALS`, empty string, null input |
| 5 | Register `LocalConditionParser` in `LogicalOperatorParser._variableParsers` list (after line 45). Add test cases with `CompoundLocal` in DisplayName (e.g., `ParsesCompoundLocalWithTalent`) verifying `LOCAL:1 && TALENT:恋人` returns `LogicalOp` with `LocalRef` and `TalentRef` children. Note: LocalGateResolver will need to decompose compound conditions when stripping LOCAL=1 gates |
| 6 | Extend `ErbParser.ParseLines()` (line 209), `ParseIfBlock()`, `ParseElseIfBranch()`, and `ParseElseBranch()` to recognize LOCAL assignment statements in all four parsing paths: add `AssignmentNode` type with `Target` (string) and `Value` (string, raw RHS), match pattern `^LOCAL(?::\d+)?\s*=\s*(.+)$`. Add unit tests to `ErbParser.Tests/LocalAssignmentTests.cs` |
| 7 | Rewrite `FileConverter.ContainsConvertibleContent()` (lines 191-212) to recursively check nested `IfNode` structures: change `ifNode.Body.Any(n => n is PrintDataNode \|\| n is DatalistNode)` to recursive traversal using helper method `HasConvertibleContentRecursive()`. Add unit test to `ErbToYaml.Tests/RecursiveConvertibleTests.cs` with 3-level nested structure |
| 8 | Add `LocalGateResolver` preprocessing step in `FileConverter.ConvertAsync()`: before converting DATALIST, scan AST for LOCAL assignments, build value map, exclude IfNode sections where LOCAL=0. Add unit tests with `LocalGateDeadCode` in DisplayName to `ErbToYaml.Tests/LocalGateResolverTests.cs` verifying dead-code exclusion |
| 9 | Extend `LocalGateResolver` to strip LOCAL=1 gates: when LOCAL value is 1, unwrap IfNode and promote Body content to parent level. Add unit tests with `LocalGateStrip` in DisplayName to `ErbToYaml.Tests/LocalGateResolverTests.cs` verifying inner PRINTDATA preserved without wrapper |
| 10 | Add LOCAL evaluation to `KojoBranchesParser.EvaluateCondition()`: add "LOCAL" to `VariablePrefixes` array (line ~18-23). The existing variable prefix loop enables dictionary-based evaluation automatically. YAML condition format: `{"LOCAL": {"0": {"==": 1}}}`. Add LOCAL state initialization in test scenarios. Unit tests will increase pass count from baseline 109 |
| 11 | All existing ErbParser tests must continue passing (baseline 140). No changes to existing parsers. Run `dotnet test src/tools/dotnet/ErbParser.Tests/` to verify regression safety |
| 12 | All existing ErbToYaml tests must continue passing (baseline 100). Recursive depth fix must not break existing conversion logic. Run `dotnet test src/tools/dotnet/ErbToYaml.Tests/` to verify regression safety |
| 13 | Code review step: run `Grep(pattern="TODO\|FIXME\|HACK", paths=src/tools/dotnet/ErbParser/LocalRef.cs,src/tools/dotnet/ErbParser/LocalConditionParser.cs,src/tools/dotnet/ErbParser/Ast/AssignmentNode.cs,src/tools/dotnet/ErbToYaml/LocalGateResolver.cs)` and verify 0 matches |
| 14 | Create F763 DRAFT file via Task#18 |
| 15 | Register F763 in index-features.md via Task#18 |
| 16 | Add `[JsonDerivedType(typeof(LocalRef), typeDiscriminator: "local")]` attribute to ICondition.cs via Task#19 |
| 17 | Add compound decomposition tests to `ErbToYaml.Tests/CompoundLocalGateTests.cs`: test `LOCAL:1=1 → IF LOCAL:1 && FIRSTTIME` resolves to `IF FIRSTTIME`; test `LOCAL:1=1 → IF LOCAL:1 && ARG==2` resolves to `IF ARG==2` |
| 18 | Run `dotnet test src/tools/dotnet/KojoComparer.Tests/` to verify >= 109 tests pass (regression gate for T14/T15 KojoBranchesParser changes) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **LocalRef type design** | A) Reuse VariableRef base class, B) Create standalone ICondition, C) Extend TalentRef | **B) Standalone ICondition** | LOCAL has no Target/Name semantics (bare identifier with optional Index). VariableRef requires Target:Name:Index pattern. Standalone `LocalRef` with only `Index` (int?, null = implicit 0), `Operator`, `Value` is the minimal, semantically correct design |
| **Parser registration** | A) New dedicated parser list, B) Extend VariableConditionParser with fallback, C) Add to LogicalOperatorParser._variableParsers | **C) Add to _variableParsers** | Minimal change. `_variableParsers` is already a list of (prefix, parser) tuples. Adding `("LOCAL", localParser.Parse)` enables compound condition support (AC#5) without architectural changes |
| **Static gate resolution** | A) During parsing (ErbParser), B) During conversion (ErbToYaml preprocessing), C) During evaluation (KojoComparer runtime) | **B) During conversion** | Parsing should produce faithful AST. Static resolution is a conversion optimization. Placing it in `FileConverter` preprocessing keeps parser pure and enables dead-code elimination before YAML generation |
| **Recursive depth fix** | A) Rewrite ContainsConvertibleContent as recursive, B) Add separate HasNestedConvertible method, C) Flatten AST during parsing | **A) Rewrite as recursive** | Current implementation is a bug (silent data loss). Recursive traversal is the correct fix. Helper method `HasConvertibleContentRecursive(IEnumerable<Node>)` called on Body/ElseIfBranches/ElseBranch ensures all nested structures are checked |
| **Assignment recognition** | A) Parse as generic statement, B) Create AssignmentNode type with LHS/RHS, C) Track assignments in separate pass | **B) AssignmentNode type** | Assignments are first-class AST nodes. `AssignmentNode` with `Target` (string, e.g., "LOCAL:1") and `Value` (string, raw RHS) enables static analysis in ErbToYaml. Value is string (not IExpression) because only literal integers are resolved — no expression AST needed. Generic statement loses semantic information |
| **Compound LOCAL handling** | A) Full decomposition (separate LOCAL from other conditions), B) Structural parsing only (no special handling), C) Defer compound conditions | **A) Full decomposition** | Active compound LOCAL:1 conditions exist (KOJO_K9_口挿入.ERB, KOJO_K9_EVENT.ERB). When LocalGateResolver strips LOCAL=1 gate from compound condition, remaining condition must be preserved (e.g., IF LOCAL:1 && FIRSTTIME → IF FIRSTTIME). Structural parsing alone would incorrectly remove or preserve the entire compound condition. |
| **KojoComparer state format** | A) `"LOCAL:{index}"`, B) `"LOCAL::{index}"` (double colon), C) `"LOCAL"` with nested dict | **A) Single colon** | Matches existing state key format (e.g., `"TALENT:TARGET:16"`). Single colon `"LOCAL:0"`, `"LOCAL:1"` is consistent with other array variables. No nested dict needed since LOCAL has no Target dimension |
| **Dynamic LOCAL scope** | A) Handle in this feature, B) Defer to F762, C) Create new feature | **C) Create new feature (F763)** | ~3% of LOCAL assignments use function results (GET_ABL_BRANCH, RAND). Requires runtime assignment tracking beyond static analysis. Tracked as F763 in Mandatory Handoffs |

### Interfaces / Data Structures

#### New AST Node: LocalRef

```csharp
namespace ErbParser;

/// <summary>
/// Represents a reference to a LOCAL variable condition
/// Pattern: LOCAL( op value)? | LOCAL:N( op value)?
/// Note: Bare LOCAL in IF means truthiness check (LOCAL != 0)
/// </summary>
public class LocalRef : ICondition
{
    /// <summary>
    /// Array index for LOCAL:N form. Null means bare LOCAL (implicit index 0).
    /// </summary>
    [JsonPropertyName("index")]
    public int? Index { get; set; }

    /// <summary>
    /// Comparison operator (==, !=, >, <, >=, <=). Null means truthiness check.
    /// </summary>
    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    /// <summary>
    /// Right-hand side value for comparison. Null if no operator.
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
```

**Design Notes**:
- **No Target/Name properties**: LOCAL is a bare identifier, not a hierarchical variable (unlike CFLAG:TARGET:NAME)
- **Index is int?**: Null = implicit index 0 (bare `LOCAL`), non-null = explicit index (e.g., `LOCAL:1`)
- **Truthiness semantics**: When Operator/Value are null, the condition means `LOCAL != 0` (ERB truthiness)

#### New AST Node: AssignmentNode

```csharp
namespace ErbParser.Ast;

/// <summary>
/// Represents an assignment statement (e.g., LOCAL = 0, LOCAL:1 = 1)
/// Enables static analysis of LOCAL values in ErbToYaml preprocessing
/// </summary>
public class AssignmentNode : AstNode
{
    /// <summary>
    /// Left-hand side variable identifier (e.g., "LOCAL", "LOCAL:1")
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Right-hand side value as raw string (e.g., "0", "1", "GET_ABL_BRANCH()")
    /// Static analysis only resolves literal integers; function calls are left unresolved
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
```

**Design Notes**:
- **Target as string**: Stores raw LHS identifier (`"LOCAL"`, `"LOCAL:1"`) for flexibility. Parser extracts index if needed. Property name aligned with F764's AssignmentNode design.
- **Value as string**: Raw RHS text. Static analysis checks `int.TryParse(Value)` — only literal integers (0, 1, -1) are resolved. Function calls (e.g., `GET_ABL_BRANCH()`) and variable references (e.g., `TARGET`) remain as unresolved strings, which LocalGateResolver skips.

#### LocalConditionParser API

```csharp
public class LocalConditionParser
{
    /// <summary>
    /// Parses LOCAL or LOCAL:N condition patterns
    /// Returns LocalRef on success, null on non-match
    /// </summary>
    /// <param name="condition">Condition string (trimmed)</param>
    /// <returns>LocalRef or null</returns>
    public LocalRef? Parse(string condition)
    {
        // Implementation:
        // 1. Try indexed form: ^LOCAL:(\d+)(?:\s*(!=|==|>=|<=|>|<)\s*(.+))?$
        // 2. Try bare form: ^LOCAL(?:\s*(!=|==|>=|<=|>|<)\s*(.+))?$
        // 3. Return null if no match
    }
}
```

**Integration Point**: Add to `LogicalOperatorParser._variableParsers` after line 45:
```csharp
var localParser = new LocalConditionParser();
_variableParsers.Add(("LOCAL", localParser.Parse));
```

#### LocalGateResolver (ErbToYaml Preprocessing)

```csharp
namespace ErbToYaml;

/// <summary>
/// Interface for LOCAL gate resolution. Enables DI injection and F763 dynamic resolver extension.
/// </summary>
public interface ILocalGateResolver
{
    List<AstNode> Resolve(List<AstNode> ast);
}

/// <summary>
/// Resolves static LOCAL gates (LOCAL=0 → exclude, LOCAL=1 → strip)
/// Applied before DATALIST-to-YAML conversion
/// </summary>
public class LocalGateResolver : ILocalGateResolver
{
    /// <summary>
    /// Single-pass sequential walk: processes assignments and IF-checks in AST order to correctly handle LOCAL reassignment patterns (0→1→0→...)
    /// </summary>
    /// <param name="ast">Parsed ERB file AST</param>
    /// <returns>Filtered AST with dead-code removed and LOCAL=1 gates stripped</returns>
    public List<AstNode> Resolve(List<AstNode> ast)
    {
        // Single-pass sequential walk (LOCAL is reassigned 20+ times within functions):
        // 1. Initialize empty value map: Dictionary<string, int?>
        // 2. Walk AST nodes in sequential order:
        //    a. AssignmentNode where Target starts with "LOCAL":
        //       - If Value is literal integer → map[Variable] = parsed int
        //       - Else (function call) → map[Variable] = null (unresolved, skip gate resolution)
        //    b. IfNode where condition contains LocalRef:
        //       - Parse condition string via LogicalOperatorParser.ParseLogicalExpression()
        //       - Construct lookup key: Index == null → "LOCAL", Index == N → "LOCAL:{N}"
        //       - Lookup key in value map
        //       - If value == 0 and simple condition → exclude IfNode (dead code)
        //       - If value == 0 and compound && condition → exclude IfNode (dead code, all && operands must be true)
        //       - If value == 0 and compound || condition → keep IfNode unchanged (|| not supported, zero kojo occurrences)
        //       - If value != 0 && value != null and simple condition → unwrap:
        //         Recursively call Resolve() on IfNode.Body to process nested LOCAL gates,
        //         then promote resolved body contents to parent level
        //       - If value != 0 && value != null and compound && condition → decompose (strip LocalRef, preserve remaining):
        //         Parse condition via LogicalOperatorParser to identify LOCAL component,
        //         then use original condition string substring extraction (split on "&&",
        //         remove LOCAL-matching segment, rejoin). This avoids needing ICondition-to-string
        //         serialization while preserving original condition text exactly.
        //         Then recursively call Resolve() on IfNode.Body for nested LOCAL gates.
        //       - If compound || condition → keep IfNode unchanged (|| not supported)
        //       - If value == null (unresolved) → keep IfNode unchanged
        //    c. Non-LOCAL IfNode → recursively call Resolve() on Body/ElseIf/Else for nested LOCAL gates
        //    d. Other nodes → pass through unchanged
        // 3. Return filtered AST
    }
}
```

**Known Limitation**: LOCAL scope is function-local, but ErbParser discards @-function declarations (line 209). The AST has no function boundary markers. The single-pass walk does not reset LOCAL values between functions. This is safe for current kojo patterns because each @-function resets LOCAL with an explicit assignment at function start (verified: all 24 kojo files follow this pattern). If a function ends with LOCAL=1 and the next function's first IF checks LOCAL before resetting it, incorrect resolution would occur. Tracked in Technical Constraints.

**Usage in FileConverter** (DI injection):
```csharp
// FileConverter constructor: inject ILocalGateResolver
public FileConverter(IErbParser parser, ILocalGateResolver localGateResolver, ...)
{
    _localGateResolver = localGateResolver;
}

// In ConvertAsync:
var ast = _parser.ParseFile(erbPath);
ast = _localGateResolver.Resolve(ast); // Preprocessing step via DI
// Then proceed with DATALIST conversion
```

#### KojoComparer State Key Format

**State Dict Entry**: `"LOCAL:{index}"` → `{value}`

**Example**:
```json
{
  "TALENT:TARGET:16": 1,
  "CFLAG:0:100": 50,
  "LOCAL:0": 1,
  "LOCAL:1": 0
}
```

**StateConverter Change**: Already handles arbitrary colon-separated keys. No change needed — existing logic will work:
```csharp
var parts = kvp.Key.Split(':');  // ["LOCAL", "1"]
var type = parts[0];              // "LOCAL"
var id = parts[parts.Length - 1]; // "1"
context[type][id] = kvp.Value;    // context["LOCAL"]["1"] = 0
```

**KojoBranchesParser EvaluateCondition Extension**:
```csharp
// In KojoBranchesParser.cs:

// 1. Add "LOCAL" to VariablePrefixes array (line ~18-23):
private static readonly string[] VariablePrefixes = [
    "CFLAG", "TCVAR", "EQUIP", "ITEM", "STAIN", "MARK", "EXP", "NOWEX",
    "ABL", "FLAG", "TFLAG", "TEQUIP", "PALAM", "ARG", "LOCAL"
];

// 2. In EvaluateCondition, the existing variable prefix loop (line ~150) already
// iterates VariablePrefixes. Adding "LOCAL" to the array enables evaluation
// of LOCAL conditions in YAML without code changes to the evaluation logic.
// The YAML condition format for LOCAL will be: {"LOCAL": {"0": {"==": 1}}}

// 3. In ValidateConditionScope, "LOCAL" is automatically included in allowedKeys
// because allowedKeys is built from VariablePrefixes + TALENT/FUNCTION/AND/OR/NOT.
```

### Implementation Order (Task Derivation Guidance for wbs-generator)

1. **Phase 1: Parser Infrastructure** (AC#1, AC#2, AC#3, AC#4)
   - Create LocalRef.cs (AC#1)
   - Implement LocalConditionParser with bare and indexed form support (AC#2, AC#3)
   - Add negative tests (AC#4)
   - Register in LogicalOperatorParser (AC#5)

2. **Phase 2: Assignment Recognition** (AC#6)
   - Create AssignmentNode AST type
   - Extend ErbParser.ParseLines() to recognize LOCAL assignments
   - Add unit tests

3. **Phase 3: ErbToYaml Preprocessing** (AC#7, AC#8, AC#9)
   - Fix ContainsConvertibleContent recursive depth (AC#7)
   - Implement LocalGateResolver (AC#8, AC#9)
   - Integrate into FileConverter
   - Add unit tests for dead-code exclusion and gate stripping

4. **Phase 4: KojoComparer Evaluation** (AC#10)
   - Extend KojoBranchesParser.EvaluateCondition for LocalRef
   - Add LOCAL state initialization in test scenarios
   - Verify increased pass count

5. **Phase 5: Regression & Debt Check** (AC#11, AC#12, AC#13)
   - Run all ErbParser tests (AC#11)
   - Run all ErbToYaml tests (AC#12)
   - Grep for technical debt markers (AC#13)

### Edge Cases and Constraints Addressed

| Constraint | Design Solution |
|------------|-----------------|
| **C1: LOCAL never inside DATALIST/PRINTDATA** | LocalGateResolver operates at function-body level (preprocessing), not inside DATALIST conversion. Parsing handles imperative IF gates correctly |
| **C2: ~94% constant assignments** | LocalGateResolver only resolves literal constants. Function-result assignments are left as-is (no gate stripping) |
| **C3: LOCAL:N is array-indexed** | LocalRef.Index property (int?) handles both bare (null) and indexed (non-null) forms |
| **C4: LOCAL=0 = dead code** | LocalGateResolver excludes IfNode sections where LOCAL value is 0 |
| **C5: Dynamic LOCAL out of scope** | Assignment recognition (AC#6) detects function-call RHS but does not attempt evaluation. LocalGateResolver skips non-literal assignments |
| **C6: Non-recursive ContainsConvertibleContent** | Rewrite as recursive helper method (AC#7) |
| **C7: Bare LOCAL = truthiness** | LocalConditionParser sets Operator=null, Value=null for bare LOCAL. KojoBranchesParser interprets this as `!= 0` check |
| **C8: Compound conditions exist** | LogicalOperatorParser registration (AC#5) enables structural parsing. LocalGateResolver decomposes compound conditions when stripping LOCAL=1 gates |
| **C9: KojoComparer pre-existing failures** | Use `gte` matcher (baseline 109 + new tests) |
| **C10: Compound LOCAL:1 are active** | LocalGateResolver decomposes compound conditions: strip LOCAL=1 from LogicalOp, preserve remaining condition |

### Resolved Design Decisions

1. **LocalGateResolver integration point**: FileConverter.ConvertAsync(), before DATALIST conversion. This is consistent with the "Usage in FileConverter" section and minimizes AST walks.
2. **Regression test strategy**: Unit tests with synthetic input for correctness verification. Smoke tests with actual kojo file content are desirable but optional (tracked as implementation-time decision).

---

## Links
- [feature-758.md](feature-758.md) - Parent feature (prefix-based variable type expansion)
- [feature-757.md](archive/feature-757.md) - Predecessor (FunctionCall passthrough)
- [feature-762.md](feature-762.md) - Sibling feature (ARG bare variables, shared parsing gap)
- [feature-764.md](feature-764.md) - Related feature (EVENT Function Conversion Pipeline, shared AssignmentNode)
- [feature-763.md](feature-763.md) - Handoff target (dynamic LOCAL tracking, created by Task#18)
- [feature-706.md](feature-706.md) - Downstream feature (full equivalence verification)
