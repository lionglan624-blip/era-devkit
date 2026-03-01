# Feature 763: Dynamic LOCAL Variable Tracking

## Status: [CANCELLED]

> **Cancelled**: 2026-02-11 — Categories 1-3 (94%) は既に対応済み（LocalGateResolver null-preservation）。Category 4 はEVENT限定 (~22件) で微小スコープ。591/591 PASS で空state検証は完了。

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
| Parent Feature | F761 |
| Discovery Point | Mandatory Handoff (F761 Scope Discipline) |
| Timestamp | 2026-02-05 |

### Identified Gap
F761 handles static LOCAL gates (~94% of cases: constant 0/1 assignments). Dynamic LOCAL (~6%) was deferred: function-result assignments (GET_ABL_BRANCH, GET_EXP_BRANCH, MASTER_POSE, RAND ~35 occurrences) and variable references (LOCAL:2 = TARGET, 15 occurrences across 9 files). Requires runtime assignment tracking beyond static analysis.

### Review Evidence
| Field | Value |
|-------|-------|
| Gap Source | F761 Implementation (feature-761.md Mandatory Handoffs) |
| Derived Task | "Handle dynamic LOCAL variable assignments that F761 deferred" |
| Comparison Result | "~6% of LOCAL patterns are dynamic — not covered by static gate resolution" |
| DEFER Reason | "Dynamic patterns require runtime context unavailable in static analysis pipeline" |

### Files Involved
| File | Relevance |
|------|-----------|
| src/tools/dotnet/ErbToYaml/LocalGateResolver.cs | Stores null for non-integer RHS (line 25-28) |
| src/tools/dotnet/ErbToYaml/ConditionSerializer.cs | No case LocalRef handler in ConvertConditionToYaml |
| pm/features/feature-761.md | Parent feature that created this handoff |

### Parent Review Observations
F761 deliberately scoped to static LOCAL gates for tractability. The ~6% dynamic estimate conflates 4 distinct pattern categories with different conversion semantics, discovered during F763 investigation.

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)
(Inherited from F761/F758) The ErbParser/ErbToYaml/KojoComparer tool pipeline is the SSOT for ERB-to-YAML condition conversion. All condition types used in kojo must be parseable, convertible, and verifiable within this pipeline to achieve full equivalence testing (F706 goal). Scope: Dynamic LOCAL variable conditions with function-result or variable-reference assignments.

### Problem (Current Issue)
The LocalGateResolver (F761) stores `null` for all non-integer RHS values (`LocalGateResolver.cs:25-28`: `int.TryParse` fails, sets `localValues[key] = null`), causing downstream IfNodes with dynamic LOCAL conditions to be preserved unchanged without conversion. F761 estimated this as "~6% dynamic LOCAL, ~50 occurrences across 15 files" and deferred them as a single feature (F763). However, these ~50 occurrences decompose into 4 semantically distinct categories with fundamentally different conversion needs:

1. **Dead assignments** (GET_ABL_BRANCH/GET_EXP_BRANCH, ~11 occurrences): Assigned but never used in any subsequent IF condition. All have "将来: 別Feature" comments. Already correctly handled by null-preservation -- no pipeline change needed.
2. **Imperative control flow** (MASTER_POSE + TARGET swap, ~15-21 occurrences): Used for dual-character kojo calling mechanics (`TARGET = LOCAL`, `TRYCALLFORM`, `TARGET = LOCAL:2`). Not content conditions -- outside YAML conversion scope entirely.
3. **Storage references** (LOCAL:101 = MASTER_POSE, ~7 occurrences): Values stored for inter-function communication in SCOM blocks. Not used in IF conditions within kojo scope.
4. **Genuine conditional usage** (~12 EVENT compound conditions + ~10 RAND ELSEIF): The only patterns where dynamic LOCAL values drive condition evaluation. But EVENT patterns exist exclusively in EVENT files blocked by F764, and RAND patterns use PRINTFORM (not DATALIST/PRINTDATA), making them unreachable by the current FileConverter.

Because the occurrence count conflates 4 distinct pattern classes -- most of which are already handled or out-of-scope -- the feature's actionable scope is near-zero without F764 (EVENT Function Conversion Pipeline). The genuine gap is not "LocalGateResolver cannot resolve dynamic LOCAL" but rather "the pipeline has no mechanism to convert dynamic LOCAL conditions to YAML, and the only patterns that would benefit from such conversion are blocked by the lack of EVENT function support (F764)."

Additionally, `ConditionSerializer.ConvertConditionToYaml` (ConditionSerializer.cs:33-59) has no `case LocalRef` handler, silently returning null for any LocalRef that survives LocalGateResolver. This is a latent gap that would cause incorrect YAML output if a code path ever produces an IfNode with an unresolved LOCAL condition reaching conversion.

### Goal (What to Achieve)
Classify all dynamic LOCAL patterns by category, ensure each category has appropriate pipeline handling (no-op recognition, explicit skip, or conversion), close the ConditionSerializer LocalRef gap, and establish clear dependency boundaries with F764 for EVENT-scoped patterns. Specifically:

1. **Classify and document** all ~50 dynamic LOCAL occurrences into the 4 categories with explicit handling strategy per category.
2. **Add defensive LocalRef handling** in ConditionSerializer to prevent silent null output for unresolved LOCAL conditions.
3. **Verify no-regression** on F761's static LOCAL gate resolution (~94% of cases).
4. **Establish F764 dependency** for EVENT-scoped dynamic LOCAL patterns (compound `LOCAL:1 && MASTER_POSE()` and RAND-based branching).

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why are ~50 dynamic LOCAL occurrences unconverted? | LocalGateResolver stores `null` for non-integer RHS values and preserves IfNodes unchanged when value is null | `LocalGateResolver.cs:106-107` |
| 2 | Why does LocalGateResolver treat all non-integer RHS as opaque? | F761 was deliberately scoped to static LOCAL gates (~94%) and deferred dynamic patterns as a conscious design decision | `feature-761.md:44` |
| 3 | Why can't these dynamic patterns simply be converted like static ones? | They decompose into 4 semantically distinct categories (dead code, imperative flow, storage, conditional) — each requiring a fundamentally different resolution strategy | Pattern analysis across ~50 occurrences |
| 4 | Why are the genuinely conditional patterns (RAND, EVENT compounds) not convertible even with a dynamic resolver? | RAND patterns use PRINTFORM (not DATALIST/PRINTDATA) and EVENT compound patterns exist in EVENT function bodies — both unreachable by current FileConverter without F764 | `KOJO_K4_会話親密.ERB:24-48`, EVENT files |
| 5 | Why was the scope estimated as a single "~6% dynamic" gap? | F761 counted occurrences by assignment type (function-result vs. constant) without categorizing by downstream conversion impact, leading to an inflated scope estimate | `feature-761.md` Review Context |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | ~50 dynamic LOCAL assignments are unresolved by the tool pipeline, deferred from F761 | The "~6% dynamic" framing conflates 4 independent pattern classes with different conversion semantics. Categories 1-3 (~33-39 occurrences) need no pipeline changes. Category 4 (~22 occurrences) is blocked by F764 |
| Where | `LocalGateResolver.cs:25-28` (`int.TryParse` fails for non-integer RHS) | Feature-level scope estimation in F761 counted by assignment type without categorizing by downstream conversion impact |
| Fix | Resolve all ~50 dynamic LOCAL assignments | Classify patterns by conversion impact; close ConditionSerializer LocalRef gap; establish F764 dependency for EVENT-scoped patterns |

## Related Features

| Feature | Status | Relationship |
|---------|--------|--------------|
| F761 (LOCAL Variable Condition Tracking) | [DONE] | Direct predecessor. Provides all current LOCAL infrastructure (LocalRef, LocalConditionParser, AssignmentNode, LocalGateResolver, ILocalGateResolver DI interface). Created F763 as mandatory handoff |
| F764 (EVENT Function Conversion Pipeline) | [DONE] | Reference for Category 4 scope boundary. EVENT files contain compound `IF LOCAL:1 && MASTER_POSE(6,1,1)` patterns and RAND-based LOCAL branching |
| F765 (SELECTCASE ARG Parsing) | [DONE] | RAND:6 -> IF LOCAL == N pattern is semantically equivalent to SELECTCASE RAND. Potential overlap in randomization handling |
| F706 (KojoComparer Full Equivalence) | [WIP] | Downstream consumer. Benefits from complete LOCAL condition handling |
| F757 (Runtime Condition Support) | [DONE] | FunctionCall ICondition passthrough for MASTER_POSE in conditions already works |
| F762 (ARG Bare Variable Condition) | [DONE] | Sibling bare-identifier pattern. Shares compound patterns with ARG in EVENT files |

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|---------|
| GET_ABL_BRANCH/GET_EXP_BRANCH dead-code recognition | FEASIBLE (NO-OP) | All 11 occurrences have "将来: 別Feature" comments; LOCAL never checked in subsequent conditions. Already correctly handled by null-preservation. No code change needed |
| MASTER_POSE + TARGET swap pattern | FEASIBLE (OUT-OF-SCOPE) | 15-21 occurrences of imperative control flow. Not content conditions. Already correctly handled by null-preservation |
| LOCAL:101 storage references | FEASIBLE (NO-OP) | ~7 occurrences. Stored values never used in IF conditions within kojo scope |
| ConditionSerializer LocalRef handler | FEASIBLE | Adding `case LocalRef` following existing ArgRef pattern is straightforward (ConditionSerializer.cs:33-59) |
| RAND-based LOCAL branching (K4_会話親密.ERB) | FEASIBLE (POST-F764) | F764 is [DONE]. Infrastructure exists but K4_会話親密.ERB uses PRINTFORM — deferred to follow-up feature |
| Compound LOCAL && MASTER_POSE in EVENT | FEASIBLE (POST-F764) | F764 is [DONE]. EVENT function infrastructure exists. ~12 occurrences deferred to follow-up feature for non-K1 EVENT files |
| ILocalGateResolver DI extension | FEASIBLE | Interface exists (`ILocalGateResolver.cs`). Nullable DI injection in `FileConverter.cs:17`. F761 designed for F763 extensibility |
| KojoComparer dynamic LOCAL evaluation | FEASIBLE | `KojoBranchesParser.cs:22` already has "LOCAL" in VariablePrefixes. Infrastructure in place for state injection |
| LOCAL:2 && STRCOUNT dead code patterns | ALREADY HANDLED | All 6 occurrences have `LOCAL:2 = 0` before compound check. F761's static gate resolver already handles as dead code (LOCAL:2=0 makes AND false) |

**Verdict**: FEASIBLE (scoped). Categories 1-3 are already handled. Category 4 is technically feasible post-F764 but deferred to follow-up feature. Current scope: defensive ConditionSerializer LocalRef handler + classification documentation.

## Impact Analysis

| Area | Impact | Description |
|------|--------|---------|
| LocalGateResolver | LOW | Current null-preservation behavior is correct for Categories 1-3. Category 4 requires F764 first |
| ConditionSerializer | MEDIUM | Missing `case LocalRef` is a latent gap. Defensive handling needed to prevent silent null output |
| FileConverter | LOW | No change needed until F764 enables EVENT function conversion |
| ErbParser | LOW | AssignmentNode captures raw string Value. Adequate for current scope. Expression-typed Value deferred |
| KojoComparer | LOW | LOCAL infrastructure exists. Dynamic evaluation deferred until converted YAML contains LOCAL conditions |
| Test coverage | MEDIUM | Need tests verifying null-preservation behavior and ConditionSerializer defensive handling |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| LocalGateResolver is static analysis (no runtime context) | F761 design | Cannot evaluate function calls (GET_ABL_BRANCH, MASTER_POSE) at conversion time |
| AssignmentNode.Value is raw string, not parsed expression | `src/tools/dotnet/ErbParser/Ast/AssignmentNode.cs:17` | Cannot evaluate function-result assignments without adding expression parsing |
| No function boundary markers in AST | `src/tools/dotnet/ErbParser/ErbParser.cs:209` (skips @-lines) | LOCAL values can leak across function boundaries. VARSET LOCAL resets partially mitigate |
| ConditionSerializer has no LocalRef conversion handler | `src/tools/dotnet/ErbToYaml/ConditionSerializer.cs:33-59` | Dynamic LOCAL conditions that survive LocalGateResolver produce null YAML output |
| RAND:N is non-deterministic | ERB language semantics | Cannot be statically resolved; multi-branch YAML representation needed |
| MASTER_POSE depends on TEQUIP runtime state | `Game/ERB/SOURCE_POSE.ERB:334-358` | Returns TARGET:LOCAL based on equipment checks -- cannot be statically resolved |
| EVENT files unreachable by FileConverter | `src/tools/dotnet/ErbToYaml/FileConverter.cs:69-80` | Dynamic LOCAL patterns in EVENT files invisible until F764 |
| GET_ABL_BRANCH/GET_EXP_BRANCH results are dead code | "将来: 別Feature" comments at all 11 occurrences | Never influence IF conditions; no conversion impact |
| TARGET swap is imperative, not conditional | Pattern: `LOCAL=MASTER_POSE, LOCAL:2=TARGET, TARGET=LOCAL, TRYCALLFORM, TARGET=LOCAL:2` | Cannot be represented as YAML conditions; needs skip/passthrough |
| MASTER_POSE/TARGET swap LOCAL in SCOM functions may bleed across boundaries | No function boundary reset without VARSET LOCAL | VARSET LOCAL resets (6 occurrences in SCOM patterns) partially mitigate |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Scope inflation from treating all ~50 occurrences as in-scope | HIGH | HIGH | Categorize by conversion impact; recognize Categories 1-3 as handled/out-of-scope |
| Over-engineering dynamic resolution for patterns that need no intervention | HIGH | HIGH | Focus on actual conversion impact, not occurrence count |
| F764 dependency creating blocking chain for genuine dynamic LOCAL conversion | HIGH | HIGH | Explicitly establish F764 as predecessor for Category 4; scope F763 to what is achievable now |
| RAND YAML representation requiring disproportionate infrastructure for 1 file | MEDIUM | MEDIUM | Defer to F765 (SELECTCASE) or post-F764 feature |
| ConditionSerializer silent null for LocalRef causing incorrect YAML in edge cases | MEDIUM | MEDIUM | Add defensive `case LocalRef` handler with explicit error/skip behavior |
| Function boundary leakage in LocalGateResolver | MEDIUM | LOW | Current kojo files reset LOCAL at function start. Risk only if future files break convention |
| Conflating F763 scope with F764 scope (EVENT compound conditions) | MEDIUM | HIGH | Clear boundary: F763 = non-EVENT + defensive handling; F764 = EVENT function conversion |

---

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Dynamic LOCAL assignments (function-result) | grep -r "LOCAL.*=.*(" Game/ERB/口上/ | ~30 | GET_ABL_BRANCH(11), MASTER_POSE(12), RAND(1), others |
| Dynamic LOCAL assignments (variable-ref) | grep -r "LOCAL:\d\+ = TARGET" Game/ERB/口上/ | ~15 | TARGET swap storage across 9 files |
| VARSET LOCAL occurrences | grep -r "VARSET LOCAL" Game/ERB/口上/ | 6 | Function-level LOCAL reset in SCOM functions |
| ConditionSerializer condition types handled | case count in ConvertConditionToYaml | 7 | TalentRef, VariableRef, ArgRef, NegatedCondition, LogicalOp, FunctionCall, BitwiseComparisonCondition (NO LocalRef) |
| LocalGateResolver test count | dotnet test --filter LocalGateResolver | F761 baseline | Existing tests for static gate resolution |

**Baseline File**: `.tmp/baseline-763.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | GET_ABL_BRANCH/GET_EXP_BRANCH LOCAL assignments are dead code (never checked in conditions) | "将来: 別Feature" comments at all 11 occurrences | ACs must NOT require resolving these; verify they are correctly handled as no-op by null-preservation |
| C2 | MASTER_POSE + TARGET swap is imperative control flow, not content conditions | Pattern analysis: `KOJO_KU_愛撫.ERB:1406-1412` | ACs must NOT attempt YAML conversion of TARGET swap patterns; verify skip/passthrough |
| C3 | ConditionSerializer.ConvertConditionToYaml has no `case LocalRef` handler | `ConditionSerializer.cs:33-59` | AC must verify either all LOCAL is pre-stripped OR a defensive LocalRef handler exists |
| C4 | Genuine dynamic LOCAL conditions exist exclusively in EVENT files | Grep evidence: compound patterns only in *_EVENT.ERB | ACs requiring dynamic LOCAL conversion must acknowledge F764 dependency |
| C5 | `LOCAL:2 = 0; IF LOCAL:2 && ...` patterns are already handled by F761 static gate | `KOJO_K10_EVENT.ERB:324-327` | ACs should verify no regression, not re-implement |
| C6 | ILocalGateResolver DI interface exists for resolver extension | `ILocalGateResolver.cs`, nullable injection in `FileConverter.cs:17` | ACs can verify DI-based extension without modifying existing resolver |
| C7 | RAND:6 in K4_会話親密.ERB uses PRINTFORM, not DATALIST | `KOJO_K4_会話親密.ERB:24-48` | RAND LOCAL branching is unreachable by current FileConverter; AC cannot verify YAML conversion without F764 |
| C8 | Only 1 file uses RAND-based LOCAL branching | K4_会話親密.ERB only | Scope any RAND-specific infrastructure to avoid over-engineering for 1 file |
| C9 | KojoComparer already includes LOCAL in VariablePrefixes | `KojoBranchesParser.cs:22` | Build on existing LOCAL evaluation infrastructure |
| C10 | AssignmentNode.Value is raw string | `AssignmentNode.cs:17` | Cannot evaluate function-call RHS without expression parsing; ACs should not require runtime evaluation |

### Constraint Details

**C1: Dead Code Recognition**
- **Source**: All 11 GET_ABL_BRANCH/GET_EXP_BRANCH assignments have "将来: 別Feature" comments and LOCAL is never checked in subsequent IF conditions
- **Verification**: Grep for GET_ABL_BRANCH in kojo files and verify no IF LOCAL follows
- **AC Impact**: ACs must verify these are recognized as unused rather than requiring conversion

**C3: ConditionSerializer LocalRef Gap**
- **Source**: Switch statement in ConvertConditionToYaml handles 7 ICondition types but not LocalRef
- **Verification**: Read `ConditionSerializer.cs:33-59` and confirm no `case LocalRef`
- **AC Impact**: AC must verify defensive handling (error, warning, or explicit skip) rather than silent null

**C4: EVENT File Exclusivity**
- **Source**: All compound `LOCAL:1 && MASTER_POSE()` patterns are in *_EVENT.ERB files
- **Verification**: Grep for `LOCAL.*&&.*MASTER_POSE` in kojo files
- **AC Impact**: Any AC requiring conversion of these patterns is blocked until F764 is [DONE]

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F761 (LOCAL Variable Condition Tracking) | [DONE] | Provides LocalRef, LocalConditionParser, AssignmentNode, LocalGateResolver infrastructure |
| Related | F764 (EVENT Function Conversion Pipeline) | [DONE] | Reference for Category 4 scope boundary. EVENT function conversion enables future dynamic LOCAL conversion for compound LOCAL && MASTER_POSE and RAND-based branching patterns |
| Related | F765 (SELECTCASE ARG Parsing) | [DONE] | RAND:6 -> IF LOCAL == N cascade is semantically equivalent to SELECTCASE RAND. May share randomization implementation |
| Related | F757 (Runtime Condition Support) | [DONE] | FunctionCall ICondition passthrough for MASTER_POSE already works in conditions |
| Related | F762 (ARG Bare Variable Condition) | [DONE] | Sibling bare-identifier pattern; shares compound patterns with ARG in EVENT files |
| Successor | F706 (KojoComparer Full Equivalence) | [WIP] | Downstream consumer. Benefits from complete LOCAL condition handling |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "SSOT for ERB-to-YAML condition conversion" | All dynamic LOCAL occurrences must be classified with explicit pipeline handling strategy | AC#1, AC#2, AC#3 |
| "All condition types used in kojo must be parseable, convertible, and verifiable" | ConditionSerializer must not silently return null for LocalRef conditions. Note: UnresolvedLocal marker provides explicit pipeline handling (parseable, verifiable) but not full conversion. Full conversion requires post-F764 follow-up | AC#4, AC#5, AC#8, AC#9 |
| "Full equivalence testing (F706 goal)" | F761 static LOCAL gate resolution must not regress; defensive LocalRef handler must produce structured output detectable by downstream equivalence tooling | AC#4, AC#5, AC#6, AC#7 |
| "Dynamic LOCAL variable conditions with function-result or variable-reference assignments" | Patterns with F764 scope boundary must be explicitly documented with dependency | AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Classification section exists | file | Grep(pm/features/feature-763.md, "^## Dynamic LOCAL Classification") | count_equals | 1 | [ ] |
| 2 | All 4 categories documented with handling strategy | file | Grep(pm/features/feature-763.md, "### Category [1-4]") | gte | 4 | [ ] |
| 3 | Each category has occurrence count and explicit handling | file | Grep(pm/features/feature-763.md, "^\\*\\*Handling\\*\\*: No pipeline change") | gte | 2 | [ ] |
| 4 | ConditionSerializer has LocalRef case handler | code | Grep(src/tools/dotnet/ErbToYaml/ConditionSerializer.cs) | contains | "case LocalRef" | [ ] |
| 5 | ConditionSerializer LocalRef does not return null silently | test | dotnet test tools/ErbToYaml.Tests --filter "LocalRef" | succeeds | - | [ ] |
| 6 | Existing LocalGateResolver tests pass | test | dotnet test tools/ErbToYaml.Tests --filter "LocalGateResolver" | succeeds | - | [ ] |
| 7 | Existing CompoundLocalGate tests pass | test | dotnet test tools/ErbToYaml.Tests --filter "CompoundLocalGate" | succeeds | - | [ ] |
| 8 | ConditionSerializer LocalRef unit test file exists | file | Glob(src/tools/dotnet/ErbToYaml.Tests/*LocalRef*) | exists | - | [ ] |
| 9 | ConditionSerializer LocalRef unit test passes | test | dotnet test tools/ErbToYaml.Tests --filter "LocalRef" | succeeds | - | [ ] |
| 10 | F764 dependency documented for EVENT-scoped patterns | file | Grep(pm/features/feature-763.md) | contains | "BLOCKED on F764" | [ ] |
| 11 | Build succeeds | build | dotnet build tools/ErbToYaml | succeeds | - | [ ] |
| 12 | All ErbToYaml tests pass | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [ ] |
| 13 | Zero technical debt in new code | code | Grep(src/tools/dotnet/ErbToYaml/ConditionSerializer.cs) | not_matches | "TODO\|FIXME\|HACK" | [ ] |
| 14 | dialogue-schema.json has UnresolvedLocal variant | code | Grep(src/tools/dotnet/YamlSchemaGen/dialogue-schema.json) | contains | "UnresolvedLocal" | [ ] |
| 15 | ConditionSerializer default case logs warning for unrecognized types | code | Grep(src/tools/dotnet/ErbToYaml/ConditionSerializer.cs) | contains | "Unrecognized condition type" | [ ] |

### AC Details

**AC#1: Classification section exists** (Goal 1, Constraint C1/C2/C4/C7)
- Verifies the feature file contains the "## Dynamic LOCAL Classification" section header as a top-level section (not inside a fenced code block)
- **Test**: Grep pattern="^## Dynamic LOCAL Classification" path="pm/features/feature-763.md"
- **Expected**: Exactly 1 match (count_equals)
- **Rationale**: Verifies the classification section was created as a deliverable, not just present in Technical Design template

**AC#2: All 4 categories documented with handling strategy** (Goal 1, Constraint C1/C2/C4/C7)
- Each category must be explicitly documented: (1) Dead assignments, (2) Imperative control flow, (3) Storage references, (4) Genuine conditional usage
- **Test**: Grep pattern="### Category [1-4]" path="pm/features/feature-763.md" | count
- **Expected**: >= 4 matches (gte matcher; Technical Design template also contains category headings)
- **Rationale**: Prevents conflation of pattern classes that drove the original scope inflation (Risk #1)

**AC#3: Each category has occurrence count and explicit handling** (Goal 1, Constraint C1/C2)
- At minimum Category 1 (dead assignments) and Category 2 (imperative flow) must state "No pipeline change" since they are correctly handled by null-preservation (C1) or out-of-scope (C2)
- **Test**: Grep pattern="^\*\*Handling\*\*: No pipeline change" path="pm/features/feature-763.md" (anchored to deliverable section's bold `**Handling**:` prefix, excluding Technical Design fenced code block matches)
- **Expected**: >= 2 matches (Categories 1 and 2)
- **Rationale**: Verifies Categories 1-2 explicitly document no-op handling in the deliverable classification section (Constraint C1/C2). Anchored pattern prevents trivial satisfaction by Technical Design template content

**AC#4: ConditionSerializer has LocalRef case handler** (Goal 2, Constraint C3)
- The switch statement in `ConvertConditionToYaml` (ConditionSerializer.cs:33-59) currently has 7 ICondition type cases but no `case LocalRef`
- A defensive handler must be added to prevent silent null return when a LocalRef survives LocalGateResolver
- **Test**: Grep pattern="case LocalRef" path="src/tools/dotnet/ErbToYaml/ConditionSerializer.cs"
- **Expected**: Match found
- The handler should produce a meaningful skip/warning, not YAML conversion (C10: AssignmentNode.Value is raw string, cannot evaluate)
- **Rationale**: Prevents silent null output for unresolved LOCAL conditions (Constraint C3)

**AC#5: ConditionSerializer LocalRef does not return null silently** (Goal 2, Constraint C3)
- The defensive handler must NOT simply fall through to the default `return null` case
- It should return a structured UnresolvedLocal marker (non-null)
- **Test**: dotnet test tools/ErbToYaml.Tests --filter "LocalRef" (same as AC#9 — the unit test asserts non-null output with UnresolvedLocal key)
- **Expected**: At least 1 test passes
- **Rationale**: Defensive handler must produce meaningful output, not silent drop. Verified by the LocalRef unit test which asserts `Assert.NotNull(result)` and `Assert.True(result.ContainsKey("UnresolvedLocal"))`

**AC#6: Existing LocalGateResolver tests pass** (Goal 3, Constraint C5)
- 7 existing tests verify F761's static LOCAL gate resolution (dead code exclusion, gate stripping, compound conditions, sequential reassignment)
- These tests MUST continue to pass unchanged, confirming no regression in the ~94% static case handling
- **Test**: dotnet test tools/ErbToYaml.Tests --filter "LocalGateResolver"
- **Expected**: 7 tests pass
- **Constraint** C5: `LOCAL:2 = 0; IF LOCAL:2 && ...` patterns are already handled by F761 static gate
- **Rationale**: Ensures no regression in ~94% static case handling (Constraint C5)

**AC#7: Existing CompoundLocalGate tests pass** (Goal 3)
- 3 existing tests verify compound LOCAL && non-LOCAL condition handling
- These tests MUST continue to pass unchanged
- **Test**: dotnet test tools/ErbToYaml.Tests --filter "CompoundLocalGate"
- **Expected**: 3 tests pass
- **Rationale**: Ensures compound LOCAL && non-LOCAL handling is preserved

**AC#8: ConditionSerializer LocalRef unit test file exists** (Goal 2, Constraint C3)
- A new test file must exist verifying the defensive handler behavior
- **Test**: Glob pattern="src/tools/dotnet/ErbToYaml.Tests/*LocalRef*"
- **Expected**: At least 1 file exists
- **Rationale**: Ensures test file is created (dotnet test --filter with 0 matching tests returns exit code 0, so file existence is verified separately)

**AC#9: ConditionSerializer LocalRef unit test passes** (Goal 2, Constraint C3)
- The test must verify that when a LocalRef condition reaches ConvertConditionToYaml, the defensive handler behaves correctly (not silent null)
- **Test**: dotnet test tools/ErbToYaml.Tests --filter "LocalRef"
- **Expected**: At least 1 test passes
- **Rationale**: Validates defensive handler behavior through automated testing (Constraint C3)

**AC#10: F764 dependency documented for EVENT-scoped patterns** (Goal 4, Constraint C4/C7/C8)
- The classification document must explicitly state that Category 4 (genuine dynamic LOCAL conditions) is blocked by F764 (EVENT Function Conversion Pipeline)
- This is already established in the Dependencies table (F764 as Related with [DONE] status)
- **Test**: Grep pattern="BLOCKED on F764" path="pm/features/feature-763.md"
- **Expected**: Match found in classification section (verifies Category 4 explicitly documents F764 blocking)
- **Constraint** C4: genuine dynamic LOCAL conditions exist exclusively in EVENT files
- **Constraint** C7: RAND LOCAL branching uses PRINTFORM, unreachable by current FileConverter
- **Constraint** C8: Only 1 file uses RAND-based LOCAL branching (K4_会話親密.ERB)
- **Rationale**: Category 4 patterns must explicitly document F764 blocking (Constraints C4/C7/C8)

**AC#11: Build succeeds** (Standard gate)
- Full build of ErbToYaml project with new LocalRef handler must succeed
- **Test**: dotnet build tools/ErbToYaml
- **Expected**: Build succeeds with zero errors
- TreatWarningsAsErrors applies (Directory.Build.props)
- **Rationale**: Standard build gate (TreatWarningsAsErrors applies)

**AC#12: All ErbToYaml tests pass** (Regression gate)
- All existing ErbToYaml tests must continue passing after adding the LocalRef handler
- This is broader than AC#6/AC#7 which target only LocalGateResolver tests
- **Test**: dotnet test tools/ErbToYaml.Tests
- **Expected**: All tests pass
- **Rationale**: Broader regression gate beyond targeted LocalGateResolver tests

**AC#13: Zero technical debt in new code** (Standard gate)
- No TODO/FIXME/HACK markers in ConditionSerializer.cs after implementation
- **Test**: Grep pattern="TODO|FIXME|HACK" path="src/tools/dotnet/ErbToYaml/ConditionSerializer.cs"
- **Expected**: 0 matches
- **Note**: Pre-existing matches in ConditionSerializer.cs must be checked at baseline. If any exist, this AC verifies no NEW markers are added.
- **Rationale**: Prevents accumulation of deferred work markers in new code

**AC#14: dialogue-schema.json has UnresolvedLocal variant** (Goal 2)
- The dialogue-schema.json conditionElement oneOf must include an UnresolvedLocal variant to prevent schema validation failures when LocalRef conditions produce structured YAML output
- **Test**: Grep pattern="UnresolvedLocal" path="src/tools/dotnet/YamlSchemaGen/dialogue-schema.json"
- **Expected**: Match found
- **Rationale**: UnresolvedLocal YAML output must pass schema validation. Without schema registration, the defensive handler creates YAML that fails the project's own validation gate
- **Note**: dialogue-schema.json is de facto hand-maintained (YamlSchemaGen/Program.cs generates a structurally different schema with `branches` format; the current file uses `entries` format with additional condition types added by subsequent features). Manual edits are safe — the generator output is stale and not re-run

**AC#15: ConditionSerializer default case logs warning for unrecognized types** (Goal 2)
- The default case in ConvertConditionToYaml should log a warning with the unrecognized type name instead of silently returning null
- **Test**: Grep pattern="Unrecognized condition type" path="src/tools/dotnet/ErbToYaml/ConditionSerializer.cs"
- **Expected**: Match found
- **Rationale**: Prevents future ICondition additions from silently failing. One-line change alongside LocalRef handler

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|-----------------|
| 1 | Classify and document all ~50 dynamic LOCAL occurrences into 4 categories | AC#1, AC#2, AC#3 |
| 2 | Add defensive LocalRef handling in ConditionSerializer | AC#4, AC#5, AC#8, AC#9, AC#14, AC#15 |
| 3 | Verify no-regression on F761's static LOCAL gate resolution | AC#6, AC#7, AC#11, AC#12 |
| 4 | Establish F764 dependency for EVENT-scoped patterns | AC#10 |

<!-- fc-phase-4-completed -->

---

## Technical Design

### Approach

This feature is a **documentation + defensive gap closure** feature, NOT a full dynamic LOCAL resolution implementation. The approach has three components:

1. **Classification Document**: Add a new "## Dynamic LOCAL Classification" section to this feature file documenting all ~50 dynamic LOCAL occurrences grouped into 4 categories with explicit handling strategy per category.

2. **ConditionSerializer LocalRef Handler**: Add a defensive `case LocalRef` to the `ConvertConditionToYaml` switch statement (ConditionSerializer.cs ConvertConditionToYaml switch, after BitwiseComparisonCondition case, before default) that produces a meaningful YAML skip marker instead of silently returning null.

3. **Unit Test Coverage**: Add a new test in `ConditionSerializerLocalRefTests.cs` (new file) verifying the defensive handler behavior when a LocalRef reaches ConvertConditionToYaml.

**LogicalOp interaction**: When a LocalRef appears inside a compound condition (LogicalOp), the non-null UnresolvedLocal return enables the LogicalOp to produce YAML output instead of returning null. Previously, `ConvertLogicalOp` (line 227: `if (yamlItems.Any(y => y == null)) return null`) would drop entire compound conditions when any child hit the default null case. After this fix, compound conditions containing unresolved LOCAL will produce structured output (e.g., `{AND: [{UnresolvedLocal: {...}}, {TALENT: {...}}]}`) rather than being silently dropped. This improvement only applies when ALL children in the compound condition are recognized types — if any sibling condition hits the default case (returning null), the entire LogicalOp still returns null. AC#15 (default case warning) mitigates this by making such cases visible. The unit test should include a compound-condition-with-LocalRef scenario.

**Why this is NOT a full resolver**: The genuine dynamic LOCAL patterns (Category 4) exist exclusively in EVENT files which are blocked by F764 (EVENT Function Conversion Pipeline). Categories 1-3 are already correctly handled by F761's null-preservation behavior. The actionable work here is to prevent silent failures and document the boundaries.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add "## Dynamic LOCAL Classification" section to this feature file with subsections for Category 1-4 |
| 2 | Grep verification: all 4 category subsections exist (pattern: `### Category [1-4]`) |
| 3 | Each category subsection includes occurrence count and handling strategy. Categories 1-2 state "No pipeline change" |
| 4 | Add `case LocalRef localRef:` to ConditionSerializer.ConvertConditionToYaml switch (after BitwiseComparisonCondition case, before default). Also update IConditionSerializer.cs docstring to reflect that non-null returns include structured markers (UnresolvedLocal) for recognized but unconvertible conditions |
| 5 | The LocalRef handler returns a YAML object with "UnresolvedLocal" type marker, NOT null |
| 6 | Existing LocalGateResolver tests continue passing (7 tests) - no code change to LocalGateResolver |
| 7 | Existing CompoundLocalGate tests continue passing (3 tests) - no code change to LocalGateResolver |
| 8 | Add new test `ConditionSerializerLocalRefTests.cs` with at least 1 test verifying LocalRef handler YAML output |
| 9 | ConditionSerializer LocalRef unit test passes |
| 10 | Dependencies table includes F764 with [DONE] status. Classification section must reference F764 scope boundary for Category 4 |
| 11 | Standard dotnet build gate |
| 12 | Standard test suite regression gate |
| 13 | No TODO/FIXME/HACK in ConditionSerializer.cs after implementation (baseline: 0 markers) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| LocalRef handler behavior | (A) Throw exception, (B) Return null, (C) Return skip marker YAML | C | Silent null (B) causes incorrect YAML. Exception (A) is too harsh for a deferred pattern. Skip marker (C) preserves round-trip ability and enables future tooling to detect unresolved patterns |
| Skip marker format | (A) `{"error": "UnresolvedLocal"}`, (B) `{"UnresolvedLocal": {"index": N, "operator": op, "value": val}}`, (C) YAML comment | B | Structured object (B) is parseable and preserves LocalRef.Index. Comment (C) breaks schema validation. Error (A) is semantically wrong (this is intentional deferral, not an error) |
| Classification location | (A) New file in `pm/reference/`, (B) Inline in feature-763.md, (C) CODE comments | B | Classification is feature-specific analysis, not reusable reference. Inline (B) satisfies AC#1/10 grep patterns without creating orphaned docs |
| Category 4 handling | (A) Implement dynamic resolver now, (B) Document F764 dependency and defer, (C) Skip Category 4 | B | F764 is now [DONE] but scoped to K1_EVENT only. Category 4 patterns span multiple EVENT files. Defensive handler + classification provides immediate value while follow-up feature handles conversion |

### Interfaces / Data Structures

**ConditionSerializer LocalRef Handler** (new code)

```csharp
// Add to ConditionSerializer.cs ConvertConditionToYaml switch (after BitwiseComparisonCondition case, before default)
case LocalRef localRef:
    return new Dictionary<string, object>
    {
        { "UnresolvedLocal", new Dictionary<string, object>
            {
                { "index", localRef.Index ?? 0 }, // null (bare LOCAL) collapses to 0 — intentional lossy mapping, bare LOCAL is semantically LOCAL:0
                { "operator", localRef.Operator ?? "!=" },
                { "value", localRef.Value ?? "0" }
            }
        }
    };
```

**Rationale**: This preserves all LocalRef metadata in YAML without attempting conversion. The YAML output is valid (parseable) but semantically distinct from other condition types, making it detectable by downstream tooling (e.g., KojoComparer can warn on UnresolvedLocal presence).

**Classification Document Structure** (new section in this file)

```markdown
## Dynamic LOCAL Classification

### Category 1: Dead Assignments (GET_ABL_BRANCH/GET_EXP_BRANCH)

**Occurrences**: ~11
**Handling**: No pipeline change needed
**Rationale**: All 11 occurrences have "将来: 別Feature" comments. LOCAL is assigned but never checked in subsequent IF conditions. LocalGateResolver's null-preservation (line 28) already handles these correctly.

### Category 2: Imperative Control Flow (MASTER_POSE + TARGET Swap)

**Occurrences**: ~15-21
**Handling**: No pipeline change needed (out-of-scope)
**Rationale**: Pattern `LOCAL=MASTER_POSE, LOCAL:2=TARGET, TARGET=LOCAL, TRYCALLFORM, TARGET=LOCAL:2` is imperative control flow for dual-character kojo calling, not content conditions. YAML conversion is not applicable.

### Category 3: Storage References (LOCAL:101 = MASTER_POSE)

**Occurrences**: ~7
**Handling**: No pipeline change needed
**Rationale**: Values stored for inter-function communication in SCOM blocks. Not used in IF conditions within kojo scope. No conversion impact.

### Category 4: Genuine Conditional Usage (EVENT Compound + RAND)

**Occurrences**: ~22 (12 EVENT compound + 10 RAND ELSEIF)
**Handling**: BLOCKED on F764 (EVENT Function Conversion Pipeline)
**Rationale**: Compound `IF LOCAL:1 && MASTER_POSE(6,1,1)` patterns exist exclusively in EVENT files. RAND patterns use PRINTFORM (not DATALIST/PRINTDATA). Both are unreachable by current FileConverter without F764's function boundary recognition.
```

**Unit Test Structure** (new file: `src/tools/dotnet/ErbToYaml.Tests/ConditionSerializerLocalRefTests.cs`)

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using ErbParser;
using ErbToYaml;

namespace ErbToYaml.Tests;

public class ConditionSerializerLocalRefTests
{
    [Fact]
    public void LocalRefReachingConverter_ProducesUnresolvedMarker()
    {
        // Arrange: ConditionSerializer instance with required dependencies
        var talentCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "Game", "CSV", "Talent.csv");
        var talentLoader = new TalentCsvLoader(talentCsvPath);
        var variableTypePrefixes = new Dictionary<Type, string>();
        var serializer = new ConditionSerializer(talentLoader, null, variableTypePrefixes);

        // Act: ConvertConditionToYaml with a LocalRef that survived LocalGateResolver
        // (In real pipeline, LocalGateResolver sets value to null for function-results,
        // preserving the IfNode with LocalRef condition)
        var localRef = new LocalRef { Index = 1, Operator = "==", Value = "1" };
        var result = serializer.ConvertConditionToYaml(localRef);

        // Assert: Defensive handler produces UnresolvedLocal marker
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("UnresolvedLocal"));

        var unresolved = result["UnresolvedLocal"] as Dictionary<string, object>;
        Assert.NotNull(unresolved);
        Assert.Equal(1, unresolved["index"]);
        Assert.Equal("==", unresolved["operator"]);
        Assert.Equal("1", unresolved["value"]);
    }

    [Fact]
    public void BareLocalRef_DefaultsIndexToZero()
    {
        // Arrange
        var talentCsvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "Game", "CSV", "Talent.csv");
        var talentLoader = new TalentCsvLoader(talentCsvPath);
        var variableTypePrefixes = new Dictionary<Type, string>();
        var serializer = new ConditionSerializer(talentLoader, null, variableTypePrefixes);

        // Act: Bare LOCAL (null Index) — e.g., "IF LOCAL != 0"
        var localRef = new LocalRef { Operator = "!=", Value = "0" };
        var result = serializer.ConvertConditionToYaml(localRef);

        // Assert: Index defaults to 0 for bare LOCAL
        Assert.NotNull(result);
        var unresolved = result["UnresolvedLocal"] as Dictionary<string, object>;
        Assert.NotNull(unresolved);
        Assert.Equal(0, unresolved["index"]);
    }
}
```

<!-- fc-phase-5-completed -->

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,2,3,10 | Document dynamic LOCAL pattern classification in feature file | | [ ] |
| 2 | 4,5,15 | Add defensive LocalRef handler to ConditionSerializer + default case warning | | [ ] |
| 3 | 8,9 | Add ConditionSerializer LocalRef unit test | | [ ] |
| 4 | 6,7,11,12,13 | Verify no regression and zero tech debt | | [ ] |
| 5 | 14 | Add UnresolvedLocal variant to dialogue-schema.json conditionElement oneOf | | [ ] |

### Task Tags

No `[I]` (Investigation) tags used. All ACs have concrete Expected values determined during investigation.

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1: Classification structure from Technical Design | Classification section in feature-763.md |
| 2 | implementer | sonnet | T2,T3: LocalRef handler design from Technical Design | ConditionSerializer.cs + ConditionSerializerLocalRefTests.cs |
| 3 | implementer | sonnet | T5: UnresolvedLocal schema variant from Technical Design | Updated dialogue-schema.json |
| 4 | ac-tester | haiku | T4: Test commands from ACs | Test results |

**Constraints** (from Technical Design):

1. **Classification scope**: 4 categories only (Dead assignments, Imperative control flow, Storage references, Genuine conditional usage)
2. **LocalRef handler behavior**: Must return structured YAML with UnresolvedLocal marker, NOT null or exception
3. **No LocalGateResolver changes**: All regression tests (AC#6, AC#7) must pass without code changes to LocalGateResolver
4. **F764 dependency**: Category 4 patterns explicitly documented as blocked on EVENT Function Conversion Pipeline
5. **Default case behavior**: The default case in ConvertConditionToYaml must log a warning with the unrecognized condition type name instead of silently returning null (AC#15)

**Pre-conditions**:
- F761 static LOCAL gate resolution infrastructure is complete ([DONE])
- ConditionSerializer.cs:33-59 switch statement exists with 7 ICondition type cases
- LocalGateResolver tests exist in tools/ErbToYaml.Tests (7 tests for LocalGateResolver, 3 for CompoundLocalGate)

**Success Criteria**:
- All 15 ACs pass
- No regression in F761 static LOCAL handling (AC#6, AC#7)
- ConditionSerializer LocalRef gap closed (AC#4, AC#5, AC#8, AC#9)
- All ~50 dynamic LOCAL occurrences categorized with explicit handling strategy (AC#1, AC#2, AC#3)
- F764 dependency established for EVENT-scoped patterns (AC#10)
- Zero technical debt (AC#13)

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Category 4 dynamic LOCAL conversion (~22 occurrences) | F764 [DONE] but scoped to K1 only; Category 4 spans multiple EVENT files | Feature | F772 | POST-LOOP Step 6.3 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

---

## Review Notes
- [fix] Phase1-RefCheck iter1: [FMT-003] Links section | F757 link broken (archived), updated to archive/feature-757.md
- [resolved-applied] Phase1-CodebaseDrift iter1: [INV-004] F765 completed ConditionSerializer extraction — AC#4/AC#5/AC#12 reference DatalistConverter.cs but ConvertConditionToYaml switch now resides in ConditionSerializer.cs. Technical Design, Impact Analysis, Baseline Measurement also stale.
- [fix] Phase1-DriftCheck iter1: [INV-004] Drift-checked F765 (Related) — no drift issues found
- [fix] Phase2-Review iter1: [FMT-002] Root Cause Analysis > 5 Whys | Reformatted from numbered list to template-required 4-column table
- [fix] Phase2-Review iter1: [FMT-002] Root Cause Analysis > Symptom vs Root Cause | Reformatted from 2-column to template-required 3-column table (What/Where/Fix)
- [resolved-applied] Phase2-Review iter1: [FMT-001] Acceptance Criteria | Missing '### Goal Coverage Verification' subsection required by template
- [resolved-applied] Phase2-Review iter1: [INV-004] AC#4/AC#5/AC#12/Technical Design/Impact Analysis/Baseline/Constraints/Contract | Codebase drift from F765: ConvertConditionToYaml extracted to ConditionSerializer.cs. All DatalistConverter.cs references stale.
- [resolved-applied] Phase2-Uncertain iter1: [AC-005] Philosophy Derivation row 2 | Claim 'convertible' mapped to UnresolvedLocal skip marker — conflates handling with conversion
- [fix] Phase2-Review iter2: [FMT-002] Implementation Contract table | Removed extra 'Tasks' column, integrated Task refs into Input column
- [fix] Phase2-Review iter2: [FMT-002] Impact Analysis table | Renamed 'Details' column to 'Description' per template
- [fix] Phase2-Review iter2: [AC-002] AC#2 | Changed matcher from count_equals:4 to gte:4 to avoid double-counting with Technical Design template
- [resolved-applied] Phase2-Uncertain iter2: [INV-004] Feasibility/Scope | F764 is [DONE] but feature scoped as 'documentation + defensive gap closure' because Category 4 was blocked. Scope rationale may be invalidated — needs verification of F764 deliverables
- [fix] Phase2-Review iter3: [FMT-001] Section order | Reordered to template: Mandatory Handoffs > Execution Log > Review Notes > Links
- [fix] Phase2-Review iter3: [FMT-002] Review Context | Restructured from bare paragraph to template format (Origin/Identified Gap/Review Evidence/Files Involved/Parent Review Observations)
- [fix] Phase2-Review iter3: [FMT-002] Feasibility Assessment | Changed **Overall Verdict** to **Verdict** per template
- [fix] Phase2-Review iter3: [AC-004] AC#9 | Changed matcher from contains "F764" to contains "BLOCKED on F764" (was trivially satisfied)
- [fix] Phase2-Review iter3: [INV-003] Key Decisions row 2 | Aligned Option B description with actual code shape
- [resolved-applied] Phase2-Review iter3: [AC-005] Technical Design > Skip marker format | UnresolvedLocal output breaks dialogue-schema.json — conditionElement oneOf has no UnresolvedLocal variant. Added AC#14 + Task#5 for schema update
- [fix] Phase2-Review iter4: [FMT-002] AC Details | Added bold formatting (**Test**:, **Expected**:, **Rationale**:) and missing Rationale fields across all 12 AC entries
- [fix] Phase2-Review iter4: [FMT-001] Section separators | Added missing `---` before ## Technical Design and ## Tasks
- [fix] Phase2-Review iter4: [AC-006] AC#8 | Split into AC#8 (file exists) + AC#8b (test passes) to prevent trivial satisfaction with zero matching tests
- [fix] Phase2-Review iter5: [FMT-002] Review Context header | Removed parenthetical annotation '(F761 Mandatory Handoff)' per template
- [fix] Phase2-Review iter5: [DEP-002] Dependencies > F764 | Changed from Predecessor to Related (F763 deliverables don't consume F764 output)
- [fix] Phase2-Review iter5: [AC-005] Philosophy Derivation Row 3 | Broadened derived requirement to include equivalence-advancing LocalRef handler; added AC#4/AC#5 coverage
- [fix] Phase2-Review iter5: [FMT-002] AC numbering | Renumbered AC#8b to AC#9 and shifted AC#9-12 to AC#10-13 for sequential integers
- [resolved-applied] Phase2-Uncertain iter5: [AC-004] AC#1/AC#2/AC#3 | Trivially satisfied by Technical Design fenced code block content — AC#1 changed to section header existence check with count_equals:1
- [fix] Phase2-Review iter6: [FMT-002] Success Criteria/Key Decisions/AC#5 note/AC#10 details | Propagated AC renumbering to all downstream cross-references (13 ACs, AC#10 for F764, AC#13 for debt, AC#9 in AC#5 note, F764 Related in AC#10)
- [fix] Phase2-Review iter7: [DEP-002] Related Features table | Updated F764 [PROPOSED]→[DONE] and F765 [PROPOSED]→[DONE]; updated AC Coverage row 10 F764 status reference
- [resolved-applied] Phase3-Maintainability iter8: [SCP-004] Mandatory Handoffs | Category 4 (~22 occurrences) has no tracked destination. Created F772 [DRAFT] and added to Mandatory Handoffs
- [resolved-applied] Phase3-Maintainability iter8: [AC-005] Extensibility | ConditionSerializer default case returns null silently for unrecognized ICondition types. Added AC#15 + Task#2 scope expansion for warning log
- [pending] Phase2-Uncertain iter9: [FMT-002] Mandatory Handoffs > Creation Task column | 'POST-LOOP Step 6.3' is not Task#{N} format per template. F772 already exists as [DRAFT] and is registered in index-features.md, so the file creation has happened — consider changing to 'N/A (created during FL POST-LOOP)'
- [pending] Phase2-Uncertain iter9: [INV-004] AC#10 + Category 4 | 'BLOCKED on F764' is semantically contradictory since F764 is [DONE]. The actual blocker is non-K1 EVENT file conversion (tracked as F772). Consider changing to 'Deferred to F772 (non-K1 EVENT conversion)' while preserving F764 historical context
- [fix] Phase2-Review iter9: [TSK-002] Implementation Contract > Phase table | Task#5 not assigned to any phase. Added Phase 3 for T5 (schema update)
- [fix] Phase2-Review iter9: [INV-003] Technical Design > Unit Test Structure | ConditionSerializer requires 3 constructor params but test used parameterless constructor. Fixed to match existing test pattern (TalentCsvLoader, null, empty dict)
- [fix] Phase2-Review iter9: [INV-003] AC#14 Details | Clarified dialogue-schema.json is de facto hand-maintained (YamlSchemaGen output structurally different from current file)
- [pending] Phase2-Review iter10: [FMT-002] Technical Design > AC Coverage table | AC#14 and AC#15 missing from AC Coverage table (only AC#1-13 present). Needs rows for AC#14 (schema update) and AC#15 (default case warning)
- [fix] Phase2-Review iter10: [FMT-002] Section separators | Removed 5 extra --- separators between Root Cause Analysis through Risks (template groups these sections without separators)
- [pending] Phase2-Uncertain iter11: [INV-004] Background > Problem, Root Cause Analysis | Stale F764-blocked narrative broader than existing [pending] AC#10 item. Problem says 'blocked by lack of EVENT function support (F764)', Root Cause says 'blocked by F764' — but F764 is [DONE]. Actionable sections (Key Decisions, Feasibility) already updated. Problem/Root Cause are historical analysis sections
- [fix] Phase2-Review iter11: [AC-006] AC#5 | Malformed not_contains with embedded newline. Changed to meta type verified via AC#4+AC#9
- [fix] Phase2-Review iter11: [FMT-001] Tasks section | Added missing ### Task Tags subsection per template
- [fix] Phase2-Review iter11: [AC-005] Philosophy Derivation Row 4 | AC#8/AC#9 misattributed. Moved to Row 2 (parseable/verifiable); Row 4 now covers AC#10 only
- [fix] Phase2-Review iter11: [INV-003] Technical Design > Approach | Added LogicalOp interaction analysis. UnresolvedLocal non-null return changes compound condition behavior from null-drop to structured YAML output
- [pending] Phase2-Review iter12: [AC-005] AC Definition Table | No AC verifies compound-condition-with-LocalRef (LogicalOp interaction). Technical Design states 'unit test should include compound-condition-with-LocalRef scenario' but AC#8/AC#9 only verify basic LocalRef. Need AC#16 for compound scenario
- [pending] Phase2-Uncertain iter12: [AC-005] Philosophy Derivation Row 2 | 'verifiable' coverage limited to schema validation (AC#14) but AC#14 not listed in Row 2 AC Coverage. Equivalence verification (KojoComparer) deferred to F772. Existing Note partially addresses but doesn't name AC#14/F772 specifically
- [fix] Phase3-Maintainability iter13: [DEP-002] index-features.md | F764 listed in F763 Depends On column but F764 is Related (not Predecessor). Changed to F761 only
- [fix] Phase3-Maintainability iter13: [AC-005] Technical Design > Unit Test Structure | Added bare LOCAL (null Index) edge case test. Verifies index defaults to 0
- [fix] Phase3-Maintainability iter13: [INV-003] Technical Design > ConditionSerializer LocalRef Handler | Added comment documenting intentional null-to-0 collapse for bare LOCAL
- [fix] Phase3-Maintainability iter13: [INV-003] Technical Design > LogicalOp interaction | Clarified improvement only applies when ALL children recognized. Unrecognized siblings still cause null-drop
- [fix] Phase2-Review iter14: [AC-002] AC#5 Type | Changed from invalid 'meta' to 'test' type with dotnet test command matching AC#9
- [fix] Phase2-Review iter14: [FMT-004] Review Notes [info] tag | Changed to [fix] tag per template-allowed tag list
- [fix] Phase2-Review iter14: [AC-004] AC#3 | Anchored pattern to deliverable section (bold Handling prefix) to prevent trivial satisfaction by Technical Design fenced code block
- [fix] Phase2-Review iter14: [CON-002] Implementation Contract | Added Constraint #5 for default case warning behavior (AC#15) — implementer guidance was missing
- [pending] Phase2-Uncertain iter14: [AC-005] Goal Coverage Verification | AC#13 (Zero tech debt) unmapped to any Goal item. Could fit Goal 2 or Goal 3 — unclear assignment
- [pending] Phase2-Uncertain iter14: [AC-005] Philosophy Derivation Row 3 | Row 3 claims 'detectable by downstream equivalence tooling' but AC#4/AC#5 verify handler existence/non-null, not downstream tool compatibility. AC#14 partially relevant but not listed in Row 3
- [pending] Phase2-Review iter15: [AC-006] AC#5 vs AC#9 | AC#5 and AC#9 are verbatim duplicates (same Type/Method/Matcher/Expected) after iter14 type change. Both mapped to different Tasks but verify identically. Need differentiation or merge
- [pending] Phase2-Review iter15: [INV-004] AC Design Constraints C4/C7 | C4 says 'blocked until F764 is [DONE]' and C7 says 'without F764' — both stale since F764 is [DONE]. Actual blocker is non-K1 EVENT scope (F772)
- [fix] Phase3-Maintainability iter16: [INV-003] Technical Design > AC Coverage #4 | IConditionSerializer docstring says 'Returns null if cannot be converted' but UnresolvedLocal returns non-null for unconvertible condition. Added docstring update to Task#2 scope
- [pending] Phase2-Uncertain iter17: [AC-001] Baseline Measurement row 5 | 'F761 baseline' is symbolic, not concrete. Should be '7' (LocalGateResolver tests only)
- [pending] Phase2-Review iter17: [AC-001] AC#1 count_equals:1 | Will FAIL on correct implementation: fenced code block (1 match) + deliverable section (1 match) = 2 matches, but expects exactly 1
- [pending] Phase2-Review iter17: [AC-004] AC#3 gte:2 loop | Iter14 fix insufficient: ^\*\*Handling\*\*: No pipeline change still matches 3 fenced code block lines, trivially satisfying gte:2
- [fix] Phase2-Review iter17: [DEP-002] Related Features > F706 | Updated [BLOCKED] → [WIP] to match actual feature-706.md status
- [fix] Phase1-RefCheck iter1: Links section | Added F758 and F772 to Links (referenced in Background and Mandatory Handoffs respectively)
- [pending] Phase4-ACValidation iter1: [AC-005] AC#15 | No negative test AC verifying default case fires for unrecognized ICondition type. AC#15 only checks string presence in source, not runtime behavior

---

## Links
- [feature-761.md](feature-761.md) - Parent feature (static LOCAL gate resolution)
- [feature-764.md](feature-764.md) - EVENT Function Conversion Pipeline (de-facto prerequisite for Category 4)
- [feature-765.md](feature-765.md) - SELECTCASE ARG Parsing (RAND overlap)
- [feature-757.md](archive/feature-757.md) - Runtime Condition Support (FunctionCall infrastructure)
- [feature-762.md](feature-762.md) - ARG Bare Variable Condition (sibling pattern)
- [feature-706.md](feature-706.md) - Full Equivalence Verification (downstream consumer)
- [feature-758.md](feature-758.md) - ErbToYaml Pipeline (Philosophy origin)
- [feature-772.md](feature-772.md) - Category 4 Dynamic LOCAL Conversion (Mandatory Handoff)
