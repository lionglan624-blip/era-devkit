# Feature 765: SELECTCASE ARG Parsing

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

## Review Context

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F762 |
| Discovery Point | Philosophy Gate (POST-LOOP Step 6.3) |
| Timestamp | 2026-02-08 |

### Identified Gap
7 occurrences of SELECTCASE ARG in WC系口上 files use ARG in a non-IF-condition context (SELECTCASE blocks). F762's ArgConditionParser handles IF conditions only. SELECTCASE requires a separate parsing pipeline that processes case/branch structures rather than boolean conditions.

### Review Evidence
| Field | Value |
|-------|-------|
| Gap Source | philosophy-deriver |
| Derived Task | "Create SELECTCASE ARG parsing pipeline for case/branch structures" |
| Comparison Result | "ArgConditionParser handles IF conditions; SELECTCASE uses different syntax (CASE value, not IF condition)" |
| DEFER Reason | "Requires separate SELECTCASE parsing pipeline, not extension of IF condition parser" |

### Files Involved
| File | Relevance |
|------|-----------|
| Game/ERB/口上/{character}/WC系口上.ERB | Contains 7 SELECTCASE ARG occurrences (one per character: 1_美鈴 through 7_子悪魔) |
| src/tools/dotnet/ErbParser/ArgConditionParser.cs | IF condition parser from F762, cannot handle SELECTCASE |

### Parent Review Observations
SELECTCASE ARG uses a fundamentally different syntax from IF ARG conditions. SELECTCASE branches on explicit case values (CASE 0, CASE 1, etc.) rather than boolean expressions. This requires a dedicated parsing pipeline for SELECTCASE blocks, separate from the condition parsing infrastructure built in F762.

---

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)
(Inherited from F758/F762) Continue toward full equivalence testing by resolving SELECTCASE condition categories that the current parsing pipeline cannot handle. SSOT: `src/tools/dotnet/ErbParser/` (AST + parser); scope: 7 SELECTCASE ARG occurrences in WC系口上 files.

### Problem (Current Issue)
The ErbParser was purpose-built incrementally for DATALIST and PRINTDATA conversion features, adding only constructs needed for those specific flows. SELECTCASE, a multi-way branch control flow construct, was architecturally excluded because all prior conversion targets used IF/ELSEIF/ELSE for conditional branching. As a result, the parser's main loop (`ErbParser.cs`) falls through to a catch-all "Skip other lines" comment for all unrecognized statements including SELECTCASE, and the AST type hierarchy (`src/tools/dotnet/ErbParser/Ast/`, 9 types) has no SelectCaseNode, CaseBranch, or CaseElseBranch representations. This makes 7 SELECTCASE ARG blocks across all character WC系口上 files completely invisible to the parsing and conversion pipeline.

Additionally, even if parsing were added, the downstream condition infrastructure (`ICondition` with 17 boolean types, `ArgConditionParser`, `DatalistConverter`, `KojoBranchesParser`) operates on boolean expressions (`IF ARG == 13`), not value-matching semantics (`CASE 13,25`). SELECTCASE-to-IF transformation is mechanically feasible but requires an explicit conversion step.

### Goal (What to Achieve)
Add SELECTCASE AST node types and parser logic to ErbParser so that SELECTCASE blocks produce structured AST output. Transform CASE branches to IF-equivalent conditions (e.g., `CASE 13,25` becomes `ARG == 13 || ARG == 25`) for downstream compatibility. Scope limited to 7 bare `SELECTCASE ARG` instances in WC系口上 files; broader SELECTCASE support (CASE IS, expression subjects, non-ARG) is deferred.

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why? | 7 `SELECTCASE ARG` blocks in WC系口上 files produce no AST output and are invisible to the conversion pipeline | ErbParser.ParseString() main loop |
| 2 | Why? | `ErbParser.ParseString()` has explicit handlers for DATALIST, PRINTDATA, STRDATA, IF, PRINTFORM, DATAFORM but no handler for SELECTCASE | ErbParser.cs main loop |
| 3 | Why? | The AST node hierarchy (`src/tools/dotnet/ErbParser/Ast/`) contains only 9 types (IfNode, DatalistNode, PrintDataNode, DataformNode, PrintformNode, ElseIfBranch, ElseBranch, AstNode, AssignmentNode) -- none for SELECTCASE structures | src/tools/dotnet/ErbParser/Ast/ (9 files) |
| 4 | Why? | The parser was designed incrementally for DATALIST-centric content extraction, adding only constructs needed for those specific conversion flows | Prior DATALIST/PRINTDATA features |
| 5 | Why (Root)? | SELECTCASE is a fundamentally different control flow construct (multi-way branch on an expression with value-matching semantics) that was never prioritized because all prior conversion targets used IF/ELSEIF/ELSE for conditional branching | ERA language architecture |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 7 SELECTCASE ARG blocks in WC系口上 are invisible to the parsing pipeline | ErbParser was architecturally built for DATALIST/PRINTDATA/IF only; SELECTCASE as a control flow construct was excluded from the incremental design |
| Where | ErbParser.ParseString() main loop 'Skip other lines' catch-all | AST type hierarchy (src/tools/dotnet/ErbParser/Ast/) lacks SelectCaseNode; parser design scoped to DATALIST-centric constructs |
| Fix | Add SELECTCASE string check to skip-list exceptions | Create SelectCaseNode/CaseBranch AST types and full ParseSelectCaseBlock() method with CASE/CASEELSE/ENDSELECT handling |

---

## Related Features

| Feature | Status | Relationship |
|---------|--------|--------------|
| F762 | [DONE] | Parent -- created ArgConditionParser, ArgRef ICondition type |
| F758 | [DONE] | Grandparent -- prefix-based variable type expansion, generic dispatch pattern |
| F764 | [PROPOSED] | Sibling -- EVENT function conversion pipeline (shares "no real-world caller" constraint) |
| F761 | [DONE] | Related -- LOCAL variable parsing |
| F759 | [DONE] | Related -- compound bitwise conditions (CASEELSE nested IF uses EQUIP compounds) |
| F706 | [BLOCKED] | Successor -- full equivalence testing |

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| AST SelectCaseNode creation | FEASIBLE | Follows existing IfNode/ElseIfBranch pattern in src/tools/dotnet/ErbParser/Ast/ |
| ErbParser SELECTCASE recognition | FEASIBLE | SELECTCASE/CASE/CASEELSE/ENDSELECT is well-defined block structure |
| CASE value parsing | FEASIBLE | Simple comma-separated integers (e.g., CASE 13,25) |
| Transformation to IF-equivalent conditions | FEASIBLE | CASE 13,25 -> ARG == 13 \|\| ARG == 25; mechanically straightforward |
| Nested IF inside CASEELSE | FEASIBLE | Reuse existing ParseIfBlock() infrastructure |
| YAML representation | FEASIBLE | Transform to IF-equivalent branches-with-conditions format |
| CASE IS comparison operators | DEFERRED | 130+ entries in corpus; not in F765 scope |
| Expression SELECTCASE subjects | DEFERRED | e.g., `ARG % 1000`; not in F765 scope |
| Non-ARG SELECTCASE | DEFERRED | ~160 occurrences in kojo; not in F765 scope |

**Verdict**: FEASIBLE for scoped 7 SELECTCASE ARG instances. All instances follow identical structure. Parser design should accommodate future non-ARG SELECTCASE (expression string storage in SelectCaseNode.Subject) but only bare ARG parsing is in scope.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F762 | [DONE] | ArgRef and ArgConditionParser infrastructure |
| Related | F764 | [DONE] | EVENT function conversion pipeline (shares same FileConverter gap) |
| Related | F758 | [DONE] | Generic variable parser dispatch pattern |
| Related | F759 | [DONE] | Compound bitwise conditions |
| Related | F761 | [DONE] | LOCAL variable parsing |
| Successor | F706 | [DONE] | Full equivalence testing (will consume parsing output) |

---

## Impact Analysis

| Area | Impact | Description |
|------|--------|-------------|
| src/tools/dotnet/ErbParser/Ast/ | NEW files | 2 new AST types: SelectCaseNode, CaseBranch |
| src/tools/dotnet/ErbParser/ErbParser.cs | MODIFY | Add SELECTCASE/CASE/CASEELSE/ENDSELECT parsing in main loop |
| src/tools/dotnet/ErbToYaml/IConditionSerializer.cs | NEW file | Interface for shared condition serialization |
| src/tools/dotnet/ErbToYaml/ConditionSerializer.cs | NEW file | Extracted condition serialization (ConvertConditionToYaml, ConvertArgRef, ConvertLogicalOp, MapErbOperatorToYaml) from DatalistConverter, implements IConditionSerializer |
| src/tools/dotnet/ErbToYaml/ISelectCaseConverter.cs | NEW file | Interface for SelectCaseNode conversion |
| src/tools/dotnet/ErbToYaml/SelectCaseConverter.cs | NEW file | SelectCaseNode-to-YAML conversion (IF-equivalent transform), implements ISelectCaseConverter, uses IConditionSerializer and ConditionExtractor |
| src/tools/dotnet/ErbToYaml/DatalistConverter.cs | MODIFY | Replace private condition methods with IConditionSerializer delegation |
| src/tools/dotnet/ErbToYaml/FileConverter.cs | MODIFY | Register SelectCaseNode as convertible node type; add ISelectCaseConverter constructor parameter |
| src/tools/dotnet/KojoComparer/KojoBranchesParser.cs | MODIFY | Add SELECTCASE-derived condition evaluation |
| src/tools/dotnet/ErbToYaml/BranchesToEntriesConverter.cs | MODIFY | Generate condition IDs for ARG case conditions |
| Broader SELECTCASE corpus (202+) | DEFERRED | Parser design enables future extension but implementation deferred |

---

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| No SELECTCASE AST node exists | src/tools/dotnet/ErbParser/Ast/ (9 types) | Must create new AST types from scratch |
| CASE value-matching != IF boolean | ERA language syntax | Requires transformation to IF-equivalent for downstream compatibility |
| CASEELSE can contain nested IF with compound EQUIP conditions | WC系口上.ERB:59-66 | Parser must handle arbitrary statements inside CASE/CASEELSE bodies |
| FileConverter only handles DatalistNode/PrintDataNode/IfNode | FileConverter top-level node filtering | Needs explicit registration of SelectCaseNode |
| All 7 occurrences are in imperative functions (non-DATALIST) | Corpus analysis | Shares F764's EVENT conversion limitation; synthetic tests required |
| CASE syntax has 3 forms in ERA: value list, CASE IS comparison, CASEELSE | ERA language | Parser must handle all 3 forms; F765 scope is value list + CASEELSE only |
| All 7 occurrences have identical structure | Cross-file comparison | Simplifies testing; single representative pattern sufficient |
| ARG values come from TCVAR:20 (;;ARG=TCVAR:20 comment) | WC系口上.ERB:52 | Integer state values as branch keys |
| TreatWarningsAsErrors=true | Directory.Build.props | All new code must be warning-free |
| SelectCaseNode.Subject should store expression string | Future-proofing for non-ARG SELECTCASE | Enables 160+ non-ARG SELECTCASE without AST redesign |
| All 7 SELECTCASE ARG are top-level statements in EVENT functions | Corpus analysis (C4) | SelectCaseNode is a directly-convertible top-level type (like DatalistNode/PrintDataNode), not routed through ContainsConvertibleContent |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Scope creep to general SELECTCASE (202+ occurrences) | HIGH | MEDIUM | Strict scope: 7 SELECTCASE ARG in WC系口上 only; parser generalizable but implementation scoped |
| F764 not complete -- no real-world conversion caller | HIGH | MEDIUM | Parser-only scope with synthetic tests (same approach as F762) |
| YAML representation design friction | MEDIUM | MEDIUM | Transform to IF-equivalent conditions; no new schema needed |
| CASEELSE nested IF parsing complexity | MEDIUM | LOW | Reuse existing ParseIfBlock() infrastructure |
| CASE IS operator form not in scope | MEDIUM | LOW | Parser should recognize but defer; clear scope boundary |
| F764/F765 ordering dependency | MEDIUM | LOW | F765 parser-only scope avoids F764 dependency for implementation |

---

## AC Design Constraints

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | 7 identical SELECTCASE ARG structures across all characters | Corpus analysis | Tests can use single representative pattern |
| C2 | No SelectCaseNode AST type exists | src/tools/dotnet/ErbParser/Ast/ (9 types) | Must verify new AST types are created and well-formed |
| C3 | CASEELSE contains nested IF with compound EQUIP conditions | WC系口上.ERB:59-66 | Must test CASEELSE body containing nested IF blocks |
| C4 | All 7 SELECTCASE ARG are in EVENT functions (not DATALIST) | Corpus analysis | Synthetic tests required (no real-world DATALIST caller) |
| C5 | CASE supports comma-separated values | CASE 13,25 | Parser must produce multiple match values per branch; multi-value test needed |
| C6 | Existing ArgRef infrastructure can be reused | F762 output | Transform CASE values to ArgRef-based conditions |
| C7 | ErbParser main loop skips SELECTCASE at 'Skip other lines' catch-all | ErbParser.cs | Must add explicit handling before skip catch-all |
| C8 | Build must succeed with TreatWarningsAsErrors=true | Directory.Build.props | AC must include build verification |
| C9 | Parser design must accommodate future non-ARG SELECTCASE | 160+ non-ARG SELECTCASE in kojo | SelectCaseNode.Subject should store expression string, not assume ARG |
| C10 | FileConverter needs to handle SelectCaseNode | FileConverter top-level node filtering | Registration required for conversion pipeline; may coordinate with F764 |

### Constraint Details

**C1**: 7 identical structures → single representative test pattern sufficient. **Verification**: Manual inspection of all 7 WC系口上 files confirms identical SELECTCASE ARG block structure (CASE 13,25 / CASE 21 / CASEELSE with nested IF EQUIP / ENDSELECT). Only dialogue text differs. Files: 1_美鈴:54-67, 2_小悪魔:49-62, 3_パチュリー:50-63, 4_咲夜:60-73, 5_レミリア:53-66, 6_フラン:55-68, 7_子悪魔:51-64.
**C2**: No SelectCaseNode exists → must create. **Verification**: Glob src/tools/dotnet/ErbParser/Ast/SelectCaseNode.cs.
**C3**: CASEELSE nests IF with EQUIP conditions. **Verification**: Read WC系口上.ERB:59-66. **AC Impact**: Test must include nested IF in CASEELSE body.
**C4**: All 7 in EVENT functions → synthetic tests required. **Verification**: Check function context around SELECTCASE ARG occurrences.
**C5**: CASE supports comma-separated values. **Verification**: Read CASE 13,25 in corpus. **AC Impact**: Multi-value parsing must produce OR condition.
**C6**: F762 ArgRef infrastructure reusable. **Verification**: Grep ArgRef in tools/ErbParser.
**C7**: Main loop skip at 'Skip other lines' catch-all comment. **Verification**: Read ErbParser.cs 'Skip other lines' catch-all comment. **AC Impact**: Handler must precede catch-all.
**C8**: TreatWarningsAsErrors=true. **Verification**: Read Directory.Build.props. **AC Impact**: Build AC required.
**C9**: Subject stores expression string for future-proofing. **Verification**: Property type is string, not enum.
**C10**: FileConverter dispatch needed. **Verification**: Read FileConverter top-level node filtering. **AC Impact**: SelectCaseNode registration required.

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "full equivalence testing" | SELECTCASE blocks must produce structured AST and transform to IF-equivalent conditions for complete pipeline coverage | AC#1, AC#2, AC#3, AC#4, AC#5, AC#6, AC#12 |
| "SSOT: src/tools/dotnet/ErbParser/" | All new AST types and parsing logic in ErbParser project; parser must handle SELECTCASE keyword and error cases | AC#1, AC#2, AC#3, AC#13, AC#14 |
| "scope: 7 SELECTCASE ARG" | Parser handles the exact structure found in WC系口上 (CASE value-list, CASEELSE with nested IF, ENDSELECT) | AC#2, AC#3, AC#4 |
| "Transform CASE branches to IF-equivalent conditions" | CASE 13,25 becomes ARG == 13 \|\| ARG == 25; downstream pipeline processes these as standard conditions; existing pipeline integrity preserved. Note: AC#7 covers entries-format ID generation and passthrough only (no evaluation); AC#8 covers branches-format evaluation only. Full entries-format evaluation deferred to F764 | AC#5, AC#6, AC#7, AC#8, AC#15 |
| "SelectCaseNode.Subject should store expression string" | Future-proofing: Subject property stores "ARG" (or any expression) as string, not hardcoded | AC#9 |
| "TreatWarningsAsErrors=true" | All new code must compile warning-free and contain no technical debt markers | AC#10, AC#11 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | SelectCaseNode.cs exists | file | Glob(src/tools/dotnet/ErbParser/Ast/SelectCaseNode.cs) | exists | src/tools/dotnet/ErbParser/Ast/SelectCaseNode.cs | [x] |
| 2 | CaseBranch.cs exists | file | Glob(src/tools/dotnet/ErbParser/Ast/CaseBranch.cs) | exists | src/tools/dotnet/ErbParser/Ast/CaseBranch.cs | [x] |
| 3 | ErbParser parses SELECTCASE to SelectCaseNode | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~SelectCase | succeeds | - | [x] |
| 4 | CASEELSE with nested IF parsed correctly | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~SelectCase | succeeds | - | [x] |
| 5 | Multi-value CASE produces OR condition | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~SelectCase | succeeds | - | [x] |
| 6 | SelectCaseConverter transforms SelectCaseNode to YAML | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~SelectCase | succeeds | - | [x] |
| 7 | BranchesToEntriesConverter generates ARG condition IDs | test | dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~SelectCase | succeeds | - | [x] |
| 8 | KojoBranchesParser evaluates ARG OR conditions (branches-format capability) | test | dotnet test tools/KojoComparer.Tests --filter FullyQualifiedName~SelectCase | succeeds | - | [x] |
| 9 | SelectCaseNode has Subject string property | code | Grep(src/tools/dotnet/ErbParser/Ast/SelectCaseNode.cs) | matches | "public string Subject" | [x] |
| 10 | All projects build warning-free | build | dotnet build (3 projects, see AC Details) | succeeds | - | [x] |
| 11 | No technical debt markers in new files | code | Grep(src/tools/dotnet/ErbParser/Ast/{SelectCaseNode,CaseBranch}.cs, src/tools/dotnet/ErbToYaml/{SelectCaseConverter,ConditionSerializer,ISelectCaseConverter,IConditionSerializer}.cs) | not_matches | "TODO\|FIXME\|HACK" | [x] |
| 12 | FileConverter recognizes SelectCaseNode as convertible | code | Grep(src/tools/dotnet/ErbToYaml/FileConverter.cs) | contains | "SelectCaseNode" | [x] |
| 13 | ErbParser handles SELECTCASE keyword | code | Grep(src/tools/dotnet/ErbParser/ErbParser.cs) | contains | "SELECTCASE" | [x] |
| 14 | Unclosed SELECTCASE throws ParseException | test | dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~SelectCase | succeeds | - | [x] |
| 15 | Existing DatalistConverter tests pass after ConditionSerializer extraction | test | dotnet test tools/ErbToYaml.Tests | succeeds | 128 tests pass | [x] |

### AC Details

**AC#1: SelectCaseNode.cs exists**
- Test: Glob pattern="src/tools/dotnet/ErbParser/Ast/SelectCaseNode.cs"
- Expected: File exists
- Rationale: New AST type for SELECTCASE blocks, following IfNode pattern (C2)
- SelectCaseNode inherits AstNode, contains Subject (string), Branches (List\<CaseBranch\>), and optional CaseElseBranch

**AC#2: CaseBranch.cs exists**
- Test: Glob pattern="src/tools/dotnet/ErbParser/Ast/CaseBranch.cs"
- Expected: File exists
- Rationale: Represents individual CASE branch with value list and body, analogous to ElseIfBranch (C2)
- CaseBranch contains Values (List\<string\> for comma-separated values like "13,25") and Body (List\<AstNode\>)

**AC#3: ErbParser parses SELECTCASE to SelectCaseNode**
- Test: dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~SelectCase
- Synthetic test input: SELECTCASE ARG / CASE 13,25 / PRINTFORML text1 / CASE 21 / PRINTFORML text2 / CASEELSE / PRINTFORML text3 / ENDSELECT
- Expected: Parser produces SelectCaseNode with Subject="ARG", 2 CaseBranch entries, and a CaseElse body (C7)
- Covers constraint C1 (single representative pattern sufficient for all 7 identical structures)

**AC#4: CASEELSE with nested IF parsed correctly**
- Test: dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~SelectCase
- Synthetic test input mirrors WC系口上 structure: CASEELSE body containing IF/ELSE/ENDIF block (C3)
- Expected: CaseElse body contains an IfNode with correct Condition, Body, and ElseBranch
- Verifies that existing ParseIfBlock() infrastructure is reused inside CASE/CASEELSE bodies

**AC#5: Multi-value CASE produces OR condition**
- Test: dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~SelectCase
- Input: CASE 13,25 with subject ARG
- Expected: Transformation produces LogicalOp with Operator="||", Left=ArgRef(Index=0, Operator="==", Value="13"), Right=ArgRef(Index=0, Operator="==", Value="25") (C5, C6)
- Single-value CASE (e.g., CASE 21) produces single ArgRef(Index=0, Operator="==", Value="21")

**AC#6: SelectCaseConverter transforms SelectCaseNode to YAML**
- Test: dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~SelectCase
- Input: SelectCaseNode with 2 CASE branches + CASEELSE (CASEELSE body contains nested IF/ELSE with conditions)
- Expected: YAML output has entries with OR-condition for multi-value CASE, single-condition for single-value CASE. Entry content contains PRINTFORML dialogue text from each CASE branch. CASEELSE with nested IfNode produces sub-branches with conditions (recursive ConvertConditionalBranches pattern), not a flat fallback. CASEELSE sub-branch content contains respective PRINTFORML text
- Verifies the full AST-to-YAML pipeline for SELECTCASE blocks including nested IF sub-branch conversion
- Implementation: New SelectCaseConverter class (separate from DatalistConverter to preserve single responsibility; DatalistConverter handles DatalistNode only)

**AC#7: BranchesToEntriesConverter generates ARG condition IDs and transforms ARG conditions**
- Test: dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~SelectCase
- Input: Branches with ARG conditions (both OR compound and single)
- Expected: IDs follow pattern "arg_0_{index}" for single ARG conditions, "or_compound_{index}" for multi-value OR conditions, "fallback" for CASEELSE
- Verifies BranchesToEntriesConverter.GenerateId handles ARG condition type
- Also verifies TransformCondition produces non-null condition for single-value ARG (ARG handler returns legacy dict as passthrough — no DialogueCondition transformation since Era.Core.Dialogue has no ArgEvaluator; entries-format evaluation deferred to F764)

**AC#8: KojoBranchesParser evaluates ARG OR conditions in branches-format YAML**
- Test: dotnet test tools/KojoComparer.Tests --filter FullyQualifiedName~SelectCase
- Input: Branches-format YAML with OR condition containing two ARG eq checks
- State: {"ARG:0": 25} (matches second value in OR)
- Expected: Branch with OR(ARG==13, ARG==25) evaluates to true when ARG:0 is 25
- Capability test: verifies KojoBranchesParser can evaluate ARG OR conditions (branches format). Does NOT test the entries-format round-trip through BranchesToEntriesConverter (entries evaluation via Era.Core.Dialogue is deferred to F764 EVENT function pipeline which enables real-world file conversion)

**AC#9: SelectCaseNode has Subject string property**
- Test: Grep pattern="public string Subject" path="src/tools/dotnet/ErbParser/Ast/SelectCaseNode.cs"
- Expected: Match found
- Rationale: Future-proofing for non-ARG SELECTCASE (C9). Subject stores the expression string ("ARG", "ARG:1", or future expressions like "ARG % 1000") rather than assuming bare ARG

**AC#10: All projects build warning-free**
- Test: dotnet build tools/ErbParser && dotnet build tools/ErbToYaml && dotnet build tools/KojoComparer
- Expected: Exit code 0 with no warnings (C8, TreatWarningsAsErrors=true)
- Covers all modified projects

**AC#11: No technical debt markers in new files**
- Test: Grep pattern="TODO|FIXME|HACK" paths=[src/tools/dotnet/ErbParser/Ast/SelectCaseNode.cs, src/tools/dotnet/ErbParser/Ast/CaseBranch.cs, src/tools/dotnet/ErbToYaml/SelectCaseConverter.cs, src/tools/dotnet/ErbToYaml/ConditionSerializer.cs, src/tools/dotnet/ErbToYaml/ISelectCaseConverter.cs, src/tools/dotnet/ErbToYaml/IConditionSerializer.cs]
- Expected: 0 matches
- Standard zero-debt verification for all 6 new files (AST types + interfaces + converter + serializer)

**AC#12: FileConverter recognizes SelectCaseNode as convertible**
- Test: Grep pattern="SelectCaseNode" path="src/tools/dotnet/ErbToYaml/FileConverter.cs"
- Expected: Match found
- Rationale: FileConverter.cs currently handles DatalistNode, PrintDataNode, IfNode (C10). SelectCaseNode must be added to the convertible node type check

**AC#13: ErbParser handles SELECTCASE keyword**
- Test: Grep pattern="SELECTCASE" path="src/tools/dotnet/ErbParser/ErbParser.cs"
- Expected: Match found
- Rationale: Confirms SELECTCASE handler is added to the main parsing loop before the catch-all skip (C7)

**AC#14: Unclosed SELECTCASE throws ParseException**
- Test: dotnet test tools/ErbParser.Tests --filter FullyQualifiedName~SelectCase
- Input: SELECTCASE ARG / CASE 1 / PRINTFORML text (missing ENDSELECT)
- Expected: ParseException thrown with message indicating unclosed SELECTCASE
- Negative test: error handling parity with existing unclosed DATALIST/IF/PRINTDATA patterns

**AC#15: Existing DatalistConverter tests pass after ConditionSerializer extraction**
- Test: dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~DatalistConverter
- Expected: All existing DatalistConverter tests pass (exit code 0)
- Regression test: verifies ConditionSerializer extraction from DatalistConverter (Task 3) does not break existing DATALIST-to-YAML conversion pipeline

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,2,9 | Create SelectCaseNode and CaseBranch AST types with Subject property | | [x] |
| 2 | 3,4,13,14 | Implement ParseSelectCaseBlock in ErbParser with CASE/CASEELSE/ENDSELECT handling | | [x] |
| 3 | 5,6,7,12,15 | Extract ConditionSerializer from DatalistConverter, create SelectCaseConverter using it, add BranchesToEntriesConverter ARG handlers, integrate with FileConverter | | [x] |
| 4 | 8 | Add KojoBranchesParser ARG OR condition evaluation tests | | [x] |
| 5 | 10,11 | Build verification and zero technical debt check | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1: AST design from Technical Design | SelectCaseNode.cs, CaseBranch.cs |
| 2 | implementer | sonnet | T2: Parser pattern from Technical Design | ParseSelectCaseBlock method in ErbParser.cs |
| 3 | implementer | sonnet | T3: Transformation logic from Technical Design | ConditionSerializer.cs (extracted) + SelectCaseConverter.cs + FileConverter modifications |
| 4 | implementer | sonnet | T4: KojoBranchesParser test from Technical Design | SelectCaseEvaluationTests class in KojoComparer.Tests |
| 5 | ac-tester | haiku | T5: AC#10-11 verification commands | Build + tech debt verification |

**Constraints** (from Technical Design):
1. AST representation: Store raw CASE values as strings, not pre-transformed ICondition
2. Multi-value OR tree: Build left-associative tree matching LogicalOperatorParser pattern
3. CASEELSE representation: Use `List<AstNode>?` in SelectCaseNode (not separate type)
4. Nested IF handling: Reuse existing ParseIfBlock() for CASEELSE bodies
5. Subject property: Store as string ("ARG") to enable future non-ARG SELECTCASE
6. Transformation semantics: CASE 13,25 → LogicalOp(||, ArgRef(==13), ArgRef(==25))
7. TreatWarningsAsErrors=true compliance required
8. ConditionSerializer extraction: `DatalistConverter.ParseCondition()` remains as public facade on IDatalistConverter (used by FileConverter.ProcessConditionalBranch). Only private condition conversion methods (`ConvertConditionToYaml`, `ConvertArgRef`, etc.) are extracted to IConditionSerializer. ParseCondition delegates internally to IConditionSerializer

**Pre-conditions**:
- F762 ArgRef and ArgConditionParser infrastructure exists ([DONE] verified)
- src/tools/dotnet/ErbParser/Ast/ directory contains 9 existing AST types
- ErbParser.cs main loop has insertion point before 'Skip other lines' comment
- DatalistConverter.cs has ConvertConditionalBranches pattern to reference (shared helpers may be extracted)
- KojoBranchesParser.EvaluateCondition() handles LogicalOp and ArgRef via ICondition dispatch

**Success Criteria**:
- All 15 ACs pass verification
- No warnings in `dotnet build tools/ErbParser && dotnet build tools/ErbToYaml && dotnet build tools/KojoComparer`
- Parser produces structured AST for SELECTCASE ARG blocks (verified by AC#3)
- Transformation to IF-equivalent conditions works for single-value and multi-value CASE (verified by AC#5)
- Downstream YAML conversion and evaluation complete round-trip (verified by AC#6-8)

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

---

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ErbParser AST types | Glob(src/tools/dotnet/ErbParser/Ast/*.cs) count | 9 | After: 11 (+SelectCaseNode, +CaseBranch) |
| ErbToYaml converter classes | Glob(src/tools/dotnet/ErbToYaml/*Converter*.cs) + Glob(src/tools/dotnet/ErbToYaml/*Serializer*.cs) | 5 (BatchConverter, BranchesToEntriesConverter, DatalistConverter, FileConverter, PrintDataConverter) | After: 8 (+SelectCaseConverter, +ConditionSerializer, +IConditionSerializer, +ISelectCaseConverter) |
| SELECTCASE ARG coverage | Grep SELECTCASE in ErbParser.cs | 0/7 (invisible to parser) | After: 7/7 (parsed to SelectCaseNode) |
| ErbParser.Tests test classes | dotnet test tools/ErbParser.Tests --list-tests | N (existing) | After: N+1 (+SelectCaseParserTests) |
| ErbToYaml.Tests test classes | dotnet test tools/ErbToYaml.Tests --list-tests | N (existing) | After: N+1 (+SelectCaseConverterTests) |
| KojoComparer.Tests test classes | dotnet test tools/KojoComparer.Tests --list-tests | N (existing) | After: N+1 (+SelectCaseEvaluationTests) |

**Baseline File**: `.tmp/baseline-765.txt`

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY. [fix] = applied fix history (immutable). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [resolved-applied] Phase2 iter1: [FMT-001] Missing Baseline Measurement section added for engine type feature with AST type count, converter class count, SELECTCASE coverage, and test class metrics.
- [resolved-applied] Phase2-Uncertain iter1: [AC-006] AC#11 scope expanded to include SelectCaseConverter.cs (new file). Modified files remain excluded per "new files" scope.
- [resolved-applied] Phase2 iter1: [AC-REG] Added AC#15 for DatalistConverter regression testing after ConditionSerializer extraction. Mapped to Task 3.
- [resolved-applied] Phase2 iter1: [AC-008] AC#8 description reframed from "round-trip" to "branches-format capability test". Entries-format evaluation deferred to F764.
- [resolved-skipped] Phase2-Uncertain iter1: [FMT-002] Phase completion markers out of order (fc-phase-5 before fc-phase-4). /fc pipeline generates in this order by design. User decision: 現状維持。
- [resolved-skipped] Phase2-Uncertain iter1: [FMT-003] AC Design Constraints placed before Acceptance Criteria section. /fc pipeline input order. User decision: 現状維持。
- [resolved-applied] Phase2-Uncertain iter1: [TD-001] ARG TransformCondition: passthrough方式を採用。レガシーdict形式のまま返す。Era.Core.DialogueにArgEvaluator不在のため、entries形式ARG評価はF764に延期。TD AC#7記述を更新。
- [resolved-applied] Phase2 iter2: [FMT-BM] Baseline Measurement table columns updated to match template format (Metric/Command/Baseline Value/Note). Added Baseline File reference.
- [resolved-applied] Phase2 iter2: [AC-COUNT] Updated AC count from "14 ACs" to "15 ACs" in Implementation Contract and Technical Design.
- [resolved-applied] Phase2 iter2: [PHIL-AC15] Added AC#15 to Philosophy Derivation table ("Transform CASE branches" row) and Goal Coverage Verification.
- [resolved-applied] Phase2 iter3: [TD-AC15] Added AC#15 row to Technical Design AC Coverage table.
- [resolved-applied] Phase2 iter3: [FMT-ORDER] Moved Review Notes section before Mandatory Handoffs to match template order.
- [resolved-applied] Phase2-Uncertain iter3: [FMT-004] Goal Coverage Verification heading changed from ### to ## for consistency with surrounding sections.
- [resolved-applied] Phase2-Uncertain iter3: [TD-002] ConditionSerializer class signature added to TD Interfaces section with constructor parameters (TalentCsvLoader, IDimConstResolver?, variableTypePrefixes) and integration notes.
- [resolved-applied] Phase3 iter4: [LEAK] Added 3 deferred SELECTCASE categories to Mandatory Handoffs (CASE IS, expression subjects, non-ARG) with F706 as destination.
- [resolved-applied] Phase3 iter4: [KD-BODY] Added Key Decision for CASE body parsing: duplicate pattern (A) selected, refactoring tracked as handoff.
- [resolved-applied] Phase3 iter4: [EDGE-NESTED] Corrected nested SELECTCASE edge case claim from "handled" to "out of scope" with future implementation note.
- [resolved-applied] Phase2 iter5: [DEP-TYPE] Changed F706 dependency type from "Consumer" to "Successor" per template SSOT (3 valid types: Predecessor/Successor/Related).
- [fix] Phase2 iter1: [FMT-AST] Background Problem, 5 Whys, Technical Constraints, C2, Pre-conditions, Baseline Measurement | AST type count 8→9 (AssignmentNode.cs exists)
- [fix] Phase2 iter1: [TD-012] Technical Design AC#12 | ContainsConvertibleContent→top-level node filtering; SelectCaseNode is directly-convertible type not routed through ContainsConvertibleContent
- [fix] Phase2 iter1: [CON-C4] Technical Constraints | Added explicit assumption: all 7 SELECTCASE ARG are top-level statements, SelectCaseNode is directly-convertible type
- [resolved-skipped] Phase2-Uncertain iter2→3: [FMT-PHIL] Philosophy Derivation subsection inside Acceptance Criteria is not in template. /fc pipeline standard; consistent with FMT-002/FMT-003 precedent.
- [resolved-skipped] Phase2-Uncertain iter2→3: [FMT-FC] Extra /fc sections (Root Cause Analysis, Related Features, etc.) not in template. /fc pipeline standard; reviewer confirmed no action needed.
- [resolved-applied] Phase2-Pending iter2→7: [LEAK-BODY] Added ParseBodyStatements duplication to Mandatory Handoffs with F706 destination.
- [resolved-skipped] Phase2-Uncertain iter2→3: [FMT-CS] ConditionSerializer extraction implicitly covered by Philosophy Derivation row 4 ("Transform CASE branches") via AC#6 (SelectCaseConverter uses ConditionSerializer) and AC#15 (regression safety). No separate row needed.
- [resolved-skipped] Phase2-Uncertain iter3→post: [AC-011B] AC#11 multi-path Grep — AC Detailsで十分明確。Method列は既に更新済み。
- [resolved-applied] Phase2-Pending iter3→post: [AC5-METHOD] AC#5 Method→ErbToYaml.Tests、Task 2→Task 3に移動。
- [fix] Phase2 iter3: [FMT-RN] Review Notes | Resolved 3 [pending] items (Philosophy Derivation placement, extra /fc sections, ConditionSerializer) as resolved-skipped
- [fix] Phase2 iter3: [CON-IC4] Implementation Contract Phase 4 | Changed ac-tester→implementer (sonnet) since new SelectCaseEvaluationTests class must be authored
- [fix] Phase2 iter3: [PHIL-AC] Philosophy Derivation | Added AC#12 to "full equivalence testing" row, AC#13+AC#14 to "SSOT: src/tools/dotnet/ErbParser/" row
- [fix] Phase2 iter2: [PHIL-004] Philosophy Derivation Transform row | Added clarifying note: AC#7 entries-format passthrough only, AC#8 branches-format only, full entries-format evaluation deferred to F764
- [resolved-applied] Phase2-Pending iter4→post: [LINE-209] 全箇所をコメントベース参照に変更（行番号削除）。
- [resolved-applied] Phase2-Uncertain iter4→post: [FMT-BM2] Baseline Measurement converter count 4→5 (BatchConverter追加)、After: 7→8（インターフェース含む）。
- [fix] Phase2 iter4: [AC-010] AC#10 Method | Changed invalid multi-project dotnet build to reference AC Details
- [fix] Phase2 iter4: [FMT-MH] Mandatory Handoffs Destination | Changed "Existing Feature" to "Feature" per template
- [fix] Phase2 iter4: [TD-GID] TD GenerateId pseudocode | Corrected comment from "CFLAG/TCVAR/ABL handlers" to "ABL handler, then fallback"
- [fix] Phase2 iter5: [TD-008] TD Approach Layer 5 + TD AC#8 | Corrected KojoBranchesParser mechanism from "ICondition dispatch" to "Dictionary<string,object> with VariablePrefixes array and dict-key matching"
- [fix] Phase2 iter5: [TD-006] TD AC#6 pseudocode | Added YAML serialization step (dialogueData dict → SerializerBuilder → Serialize)
- [fix] Phase2 iter5: [PHIL-011] Philosophy Derivation | Added AC#11 to "TreatWarningsAsErrors" row
- [fix] Phase2 iter1(FL2): [FMT-MHC] Mandatory Handoffs Creation Task | Replaced prose descriptions with concise Option B format ("Scope item for F706...")
- [resolved-applied] Phase2-Loop iter1(FL2): [AC-011C] AC#11 Method column is prose redirect ("Grep(6 new files, see AC Details)"). Previously resolved-skipped (iter3→post). Re-raised by structural reviewer. Updated to list all 6 file paths directly.
- [resolved-applied] Phase2-Critical iter2(FL2): [TD-ICS] IConditionSerializer.MapErbOperatorToYaml interface signature (string → string) mismatches DatalistConverter.MapErbOperatorToYaml actual signature (string?, string? → Dictionary<string, object>). Interface must match actual extraction or document intentional simplification.
- [fix] PostLoop iter7(FL2): [TD-ICS] Updated IConditionSerializer.MapErbOperatorToYaml signature to Dictionary<string, object> MapErbOperatorToYaml(string?, string?) matching actual DatalistConverter. Also updated ConditionSerializer class signature.
- [fix] Phase2 iter2(FL2): [CON-C1] AC Design Constraint C1 | Added manual verification evidence with specific line ranges for all 7 WC系口上 files confirming identical SELECTCASE ARG structure
- [fix] Phase2 iter3(FL2): [TD-SER] TD pseudocode SerializeCondition(condition) → _conditionSerializer.ConvertConditionToYaml(condition) to match IConditionSerializer interface
- [fix] Phase2 iter3(FL2): [TD-CEX] TD Integration note updated: SelectCaseConverter receives ConditionExtractor for CASEELSE nested IF condition string parsing
- [fix] Phase2 iter3(FL2): [FMT-FIX] Added [{category-code}] brackets to all 21 [fix] entries per template format requirement
- [resolved-applied] Phase2-Uncertain iter3(FL2): [TASK-003] Task 3 description omits BranchesToEntriesConverter modification (AC#7 requires ARG handlers in GenerateId/TransformCondition). TD AC#7 is explicit but Task description incomplete. Added BranchesToEntriesConverter ARG handlers to Task 3.
- [fix] Phase2 iter4(FL2): [FMT-RN2] Added [{category-code}] brackets to 8 non-[fix] Review Notes entries (4 resolved-skipped, 3 pending, 1 resolved-applied)
- [fix] Phase2 iter4(FL2): [AC-006B] AC#6 Expected: Added PRINTFORML content preservation assertion for CASE branches and CASEELSE sub-branches
- [resolved-applied] Phase2-Uncertain iter4(FL2): [TD-PFN] TD ConvertConditionalBranches pattern reuse for CASEELSE: DatalistConverter uses DataformNode.Arguments but SELECTCASE has PrintformNode.Content. Added explicit PrintformNode note to TD AC#6.
- [resolved-applied] Phase2-Uncertain iter4(FL2): [IA-CEX] Impact Analysis SelectCaseConverter row updated: 'uses IConditionSerializer and ConditionExtractor' to match TD Integration note.
- [fix] Phase2 iter5(FL2): [FMT-RNC] Added 3 missing Review Notes template comment lines (Format, Tag rules, Category codes)
- [resolved-applied] Phase2-Loop iter5→post(FL2): [LINE-TD] Converted all residual hardcoded line number references in TD sections to method-name/comment-based references (~10 instances). Includes ParseIfBlock, GenerateId, FileConverter dispatch, LogicalOperatorParser, DATALIST error handling pattern, 5 Whys ErbParser, Technical Constraints FileConverter/ErbParser.
- [fix] Phase2 iter6(FL2): [TD-YAML] TD pseudocode SerializerBuilder missing CamelCaseNamingConvention. Added .WithNamingConvention(CamelCaseNamingConvention.Instance) to match existing DatalistConverter/FileConverter/PrintDataConverter convention
- [resolved-applied] Phase3-Maintainability iter6→post: [API-PRESERVE] Implementation Contract Constraint 8に追加。ParseConditionはDatalistConverterにファサードとして残存。
- [fix] Phase3 iter6: [TD-DI] Impact Analysis + TD Interfaces | Added IConditionSerializer, ISelectCaseConverter interfaces following established DI pattern (IBatchConverter, IDatalistConverter, etc.)
- [fix] Phase3 iter6: [TD-012B] TD AC#12 | Added ISelectCaseConverter constructor DI to FileConverter
- [fix] Phase3 iter6: [TD-CS] TD ConditionSerializer | Updated class to implement IConditionSerializer interface
- [fix] Phase3 iter7: [LEAK-CCB] Mandatory Handoffs | Added ConvertConditionalBranches converter-layer duplication to handoffs with F706 destination
- [fix] Phase3 iter7: [LEAK-PBS] Mandatory Handoffs | Added ParseBodyStatements parser-layer duplication (resolved LEAK-BODY pending)
- [fix] Phase2 iter8: [AC-011] AC#11 | Added IConditionSerializer.cs and ISelectCaseConverter.cs to tech debt scan (6 new files total)
- [fix] Phase2 iter1(FL3): [FMT-5W] 5 Whys | Numbered list format→template table format (Level/Question/Answer/Evidence)
- [fix] Phase2 iter1(FL3): [FMT-SRC] Symptom vs Root Cause | 2-column format→template 3-column format (Aspect/Symptom/Root Cause with What/Where/Fix rows)
- [resolved-applied] Phase2-Uncertain iter1(FL3): [FMT-GCV] Goal Coverage Verification uses 2-column format with free-text Goal Items instead of template 3-column format (Goal Item/Description/Covering AC(s)) with numbered sequential items.
- [fix] PostLoop iter2(FL3): [FMT-GCV] Goal Coverage Verification | 2-column free-text format→template 3-column format with numbered Goal Items
- [resolved-applied] Phase2-Uncertain iter1(FL3): [FMT-MHT] Mandatory Handoffs Creation Task column contains prose ("Scope item for F706...") instead of Task#{N} references. SSOT ambiguity: CLAUDE.md requires "Task to add content OR direct Edit" for Option B, but template validation only checks "Referenced Feature exists."
- [fix] PostLoop iter2(FL3): [FMT-MHT] Mandatory Handoffs Creation Task | Prose descriptions→"Direct Edit" (CLAUDE.md Option B mechanism). Task#6 approach reverted due to AC Coverage Rule conflict (every Task requires AC)
- [fix] Phase2 iter1(FL3-R): [AC-TASK6] Tasks table | Removed Task#6 (handoff-only, no AC). Mandatory Handoffs Creation Task uses "Direct Edit" per CLAUDE.md Deferred Task Protocol Option B
- [fix] Phase7 iter2(FL3-R): [REF-HIST] Background/5 Whys/AC#15 | Removed non-existent F349/F634/F755 references (historical features without feature-{ID}.md files)
- [fix] Phase7 iter3(FL3-R): [REF-PATH] Files Involved | Corrected WC系口上 path from non-existent directory (Game/ERB/口上/WC系口上/) to actual per-character path (Game/ERB/口上/{character}/WC系口上.ERB)

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| CASE IS comparison operators (~130 entries) | Requires CASE IS parsing pipeline (comparison operators not value lists) | Feature | F706 | Direct Edit |
| Expression SELECTCASE subjects (e.g., ARG % 1000) | Requires expression evaluation in SELECTCASE subject | Feature | F706 | Direct Edit |
| Non-ARG SELECTCASE (~160 occurrences) | Different variable subjects require generalized SELECTCASE handling | Feature | F706 | Direct Edit |
| ParseBodyStatements duplication (ParseIfBlock/ParseSelectCaseBlock body-parsing loops) | Parser-level body-parsing duplication; refactoring across block parsers has high regression risk | Feature | F706 | Direct Edit |
| ConvertConditionalBranches duplication (3 copies: DatalistConverter, FileConverter, SelectCaseConverter) | Converter-layer branch-walking logic duplicated; extraction to shared utility risky across 3 converters in F765 scope | Feature | F706 | Direct Edit |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Add SELECTCASE AST node types | AC#1, AC#2, AC#9 |
| 2 | Parser logic for SELECTCASE blocks | AC#3, AC#4, AC#13, AC#14 |
| 3 | Transform CASE branches to IF-equivalent conditions | AC#5, AC#6 |
| 4 | Multi-value CASE (CASE 13,25) → OR condition | AC#5, AC#7 |
| 5 | CASEELSE handling | AC#3, AC#4 |
| 6 | Downstream YAML conversion | AC#6, AC#7, AC#12 |
| 7 | KojoBranchesParser evaluation | AC#8 |
| 8 | TreatWarningsAsErrors compliance | AC#10 |
| 9 | Zero technical debt | AC#11 |
| 10 | Scope limited to 7 bare SELECTCASE ARG | AC#3 |
| 11 | Existing pipeline regression safety | AC#15 |

All Goal items are covered. No gaps found.

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Add SELECTCASE parsing via a 5-layer transformation pipeline matching the existing IF parsing architecture:

1. **AST Layer**: Create `SelectCaseNode` (Subject, Branches, CaseElse) and `CaseBranch` (Values, Body) following the `IfNode`/`ElseIfBranch` pattern
2. **Parser Layer**: Add `ParseSelectCaseBlock()` method in `ErbParser.cs` paralleling `ParseIfBlock()` (ParseIfBlock method), reusing existing `ParseIfBlock()` for nested IF in CASEELSE
3. **Transformation Layer**: Convert CASE value lists to IF-equivalent conditions using existing `ArgRef` and `LogicalOp` infrastructure from F762
4. **Converter Layer**: Create `SelectCaseConverter` class (separate from `DatalistConverter` to preserve single responsibility) to transform `SelectCaseNode` to branch-with-condition format (referencing `ConvertConditionalBranches()` pattern)
5. **Evaluation Layer**: Leverage existing `KojoBranchesParser.EvaluateCondition()` which handles YAML `Dictionary<string,object>` conditions via `VariablePrefixes` array dispatch (including "ARG") and `EvaluateCompoundCondition` for OR/AND/NOT dict-key matching

**Transformation Semantics**:
- `CASE 13,25` with `Subject="ARG"` → `LogicalOp(Operator="||", Left=ArgRef(Index=0, Operator="==", Value="13"), Right=ArgRef(Index=0, Operator="==", Value="25"))`
- `CASE 21` → `ArgRef(Index=0, Operator="==", Value="21")`
- `CASEELSE` with nested IF → recursive sub-branches with conditions (EQUIP conditions preserved)
- `CASEELSE` without nested IF → no condition (empty dict in YAML, fallback priority)

This approach satisfies all 15 ACs by building on proven F762 infrastructure while containing scope to 7 identical SELECTCASE ARG structures.

---

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `src/tools/dotnet/ErbParser/Ast/SelectCaseNode.cs` with `Subject` (string), `Branches` (List<CaseBranch>), `CaseElse` (List<AstNode>?) following `IfNode.cs` pattern |
| 2 | Create `src/tools/dotnet/ErbParser/Ast/CaseBranch.cs` with `Values` (List<string>) and `Body` (List<AstNode>) following `ElseIfBranch.cs` pattern |
| 3 | Add `ParseSelectCaseBlock()` in `ErbParser.cs` (main loop before 'Skip other lines' comment). Parse SELECTCASE/CASE/CASEELSE/ENDSELECT with nested statement support. Test with synthetic input matching WC系口上 structure |
| 4 | In `ParseSelectCaseBlock()`, when encountering CASEELSE, reuse existing `ParseIfBlock()` (ParseIfBlock method) to handle nested IF/ELSE/ENDIF blocks inside CASEELSE body |
| 5 | In `SelectCaseConverter`, convert `CaseBranch.Values` list to `ArgRef`-based `LogicalOp` tree. For multi-value CASE: fold values into left-associative OR tree via `new LogicalOp { Left = accumulator, Operator = "||", Right = new ArgRef { Index = 0, Operator = "==", Value = val } }` |
| 6 | Create new `SelectCaseConverter` class with `Convert(SelectCaseNode, string, string)` method (separate from `DatalistConverter` to preserve single responsibility). Transform each `CaseBranch` to branch dict with OR-condition (multi-value) or single ArgRef (single-value). CASEELSE body: if contains nested IfNode, adapt ConvertConditionalBranches pattern with PrintformNode body extraction (not DataformNode.Arguments as in DatalistConverter); otherwise, produce fallback entry with no condition. `FileConverter` injects `SelectCaseConverter` and dispatches `SelectCaseNode` to it |
| 7 | In `BranchesToEntriesConverter.GenerateId()` (GenerateId method), add `if (condition.ContainsKey("ARG"))` handler returning `$"arg_{argId}_{index}"` for single ARG, existing OR handler covers multi-value. In `TransformCondition()`, add ARG handler that returns the legacy dict as passthrough (no DialogueCondition transformation). Rationale: Era.Core.Dialogue has no ArgEvaluator; entries-format ARG evaluation deferred to F764. ARG condition passthrough format: `{ "ARG": { "0": { "eq": "13" } } }` |
| 8 | `KojoBranchesParser.EvaluateCondition()` dispatches YAML `Dictionary<string,object>` conditions via `VariablePrefixes` array (includes "ARG" → `EvaluateVariableCondition`) and `EvaluateCompoundCondition` for OR/AND/NOT dict keys. No ICondition types involved. ARG evaluation already supported. No changes needed; verify with test using YAML dict `{"OR": [{"ARG": {"0": {"eq": "13"}}}, {"ARG": {"0": {"eq": "25"}}}]}` |
| 9 | `SelectCaseNode` property: `public string Subject { get; set; } = string.Empty;` stores "ARG" for current scope, enables future "ARG:1" or "ARG % 1000" without AST redesign |
| 10 | Build verification: `dotnet build tools/ErbParser && dotnet build tools/ErbToYaml && dotnet build tools/KojoComparer` with exit code 0 |
| 11 | Code review in new AST files: no TODO/FIXME/HACK comments |
| 12 | In `FileConverter.cs` top-level node filtering, add `SelectCaseNode` as a directly-convertible top-level type alongside DatalistNode/PrintDataNode. In conversion dispatch block, add `else if (node is SelectCaseNode)` branch delegating to `ISelectCaseConverter`. Add `ISelectCaseConverter` to FileConverter constructor DI (alongside existing IPathAnalyzer, IPrintDataConverter, IDatalistConverter). Not routed through `ContainsConvertibleContent` (which is IfNode-specific) |
| 13 | In `ErbParser.cs` main loop, add `if (line.StartsWith("SELECTCASE", StringComparison.OrdinalIgnoreCase))` before 'Skip other lines' comment |
| 14 | Add test: `ParseSelectCaseBlock()` throws `ParseException("SELECTCASE without matching ENDSELECT", fileName, lineNumber)` when EOF reached before ENDSELECT, paralleling DATALIST/PRINTDATA error handling pattern (DATALIST/PRINTDATA error handling pattern) |
| 15 | Regression test: `dotnet test tools/ErbToYaml.Tests --filter FullyQualifiedName~DatalistConverter` passes after ConditionSerializer extraction from DatalistConverter. Verifies refactoring in T3 does not break existing DATALIST pipeline |

---

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **AST representation** | A) Store raw CASE values as strings; B) Pre-transform to ICondition in parser | **A** | Separation of concerns: Parser builds AST (syntax), Converter performs transformation (semantics). Matches existing pattern where `IfNode.Condition` stores raw string, `LogicalOperatorParser` parses later |
| **CASE value storage** | A) `List<string> Values`; B) `List<int> Values` | **A** | Future-proofs for CASE "value" (string literals in non-ARG SELECTCASE). Parser preserves raw text; Converter validates/transforms |
| **Multi-value OR tree structure** | A) Build balanced tree; B) Build left-associative tree | **B** | Consistency with `LogicalOperatorParser.ParseOrExpression()` (ParseOrExpression method) which builds `((a \|\| b) \|\| c)`. Same evaluation result, consistent codebase pattern |
| **CASEELSE representation** | A) Add `CaseElseBranch` type; B) Use `List<AstNode>?` in `SelectCaseNode` | **B** | CASEELSE is unique (exactly 0 or 1), unlike ELSEIF (0-N). Reusing IfNode's `ElseBranch?` pattern would need new type; simpler to store as nullable list in parent node |
| **Nested IF in CASEELSE** | A) Inline parse logic; B) Reuse `ParseIfBlock()` | **B** | Code reuse. `ParseIfBlock()` already handles IF/ELSEIF/ELSE/ENDIF recursion (ParseIfBlock method). CASEELSE body parsing loops over statements, delegates IF to existing infrastructure |
| **FileConverter integration** | A) Wait for F764; B) Implement now | **B** | F765 and F764 are independent sibling features with no execution dependency. `FileConverter.cs` has simple node-type dispatch; adding `SelectCaseNode` branch is 10-line change with no F764 coupling |
| **SELECTCASE converter class** | A) Overload DatalistConverter.Convert(); B) Create separate SelectCaseConverter | **B** | DatalistConverter implements IDatalistConverter scoped to DATALIST blocks. Adding SelectCaseNode overload violates single responsibility. FileConverter injects SelectCaseConverter separately. Condition serialization (ConvertConditionToYaml, ConvertArgRef, ConvertLogicalOp, MapErbOperatorToYaml) MUST be extracted from DatalistConverter into shared ConditionSerializer utility as part of T3 |
| **Condition serialization format** | A) Nested dict `{ ARG: { 0: { eq: "13" } } }`; B) Flat `{ type: "ArgRef", index: 0, value: "13" }` | **A** | Consistency with existing variable condition format (CFLAG, TCVAR, EQUIP all use nested dict). BranchesToEntriesConverter.GenerateId() expects this format (GenerateId method) |
| **CASE body parsing** | A) Duplicate existing body-parsing pattern from ParseIfBlock; B) Extract shared ParseBodyStatements helper and refactor all block parsers | **A** | F765 scope is 7 SELECTCASE ARG instances; body-parsing refactoring across ParseIfBlock/ParseElseIfBranch/ParseElseBranch is a separate concern with high regression risk. Duplication is acceptable for F765; refactoring tracked as handoff to F706 sub-features |

---

### Interfaces / Data Structures

#### New AST Types

**SelectCaseNode.cs**
```csharp
namespace ErbParser.Ast;

/// <summary>
/// Represents a SELECTCASE...ENDSELECT block
/// Supports SELECTCASE Subject with CASE values, CASEELSE, and nested statements
/// </summary>
public class SelectCaseNode : AstNode
{
    /// <summary>
    /// The expression being switched on (e.g., "ARG", "ARG:1", "PALAM:欲情")
    /// Stored as string to support future non-ARG SELECTCASE without AST redesign
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// List of CASE branches with value matchers and bodies
    /// </summary>
    public List<CaseBranch> Branches { get; } = new();

    /// <summary>
    /// Optional CASEELSE body (fallback when no CASE matches)
    /// Nullable: CASEELSE is optional in ERA
    /// </summary>
    public List<AstNode>? CaseElse { get; set; }
}
```

**CaseBranch.cs**
```csharp
namespace ErbParser.Ast;

/// <summary>
/// Represents a single CASE branch in a SELECTCASE block
/// CASE 13,25 → Values = ["13", "25"]
/// </summary>
public class CaseBranch
{
    /// <summary>
    /// Comma-separated values from CASE statement (e.g., ["13", "25"])
    /// Stored as strings to preserve original format and support future string matching
    /// </summary>
    public List<string> Values { get; } = new();

    /// <summary>
    /// Statements inside this CASE branch (PRINTFORML, nested IF, etc.)
    /// </summary>
    public List<AstNode> Body { get; } = new();
}
```

---

#### IConditionSerializer and ISelectCaseConverter Interfaces

```csharp
namespace ErbToYaml;

public interface IConditionSerializer
{
    Dictionary<string, object>? ConvertConditionToYaml(ICondition condition);
    Dictionary<string, object> MapErbOperatorToYaml(string? erbOperator, string? value);
}

public interface ISelectCaseConverter
{
    string Convert(SelectCaseNode selectCase, string character, string situation);
}
```

#### ConditionSerializer (Extracted from DatalistConverter)

```csharp
namespace ErbToYaml;

/// <summary>
/// Shared condition serialization utility extracted from DatalistConverter.
/// Handles ICondition → YAML dict conversion for all variable types.
/// Both DatalistConverter and SelectCaseConverter depend on IConditionSerializer.
/// </summary>
public class ConditionSerializer : IConditionSerializer
{
    private readonly TalentCsvLoader _talentLoader;
    private readonly IDimConstResolver? _dimConstResolver;
    private readonly Dictionary<Type, string> _variableTypePrefixes;

    public ConditionSerializer(
        TalentCsvLoader talentLoader,
        IDimConstResolver? dimConstResolver,
        Dictionary<Type, string> variableTypePrefixes)
    {
        _talentLoader = talentLoader;
        _dimConstResolver = dimConstResolver;
        _variableTypePrefixes = variableTypePrefixes;
    }

    /// <summary>Converts ICondition to YAML dict format (e.g., { "TALENT": { "3": { "ne": "0" } } })</summary>
    public Dictionary<string, object>? ConvertConditionToYaml(ICondition condition) { ... }

    /// <summary>Maps ERB operator and value to YAML condition dict (e.g., { "eq": "13" })</summary>
    public Dictionary<string, object> MapErbOperatorToYaml(string? erbOperator, string? value) { ... }
}
```

**Integration**: DatalistConverter receives ConditionSerializer via constructor injection and delegates all condition serialization calls. SelectCaseConverter receives both ConditionSerializer (for CASE→ArgRef condition serialization) and ConditionExtractor (for CASEELSE nested IF condition string parsing: raw string → ICondition → YAML dict).

---

#### Parser Method Signature

```csharp
// In ErbParser.cs
private SelectCaseNode ParseSelectCaseBlock(string[] lines, ref int currentIndex, string fileName)
{
    // 1. Extract subject from "SELECTCASE ARG"
    // 2. Loop: parse CASE branches until CASEELSE or ENDSELECT
    //    - CASE: parse comma-separated values, collect body statements (reuse ParseIfBlock for nested IF)
    //    - CASEELSE: collect body statements until ENDSELECT
    //    - ENDSELECT: return node
    // 3. Throw ParseException if EOF before ENDSELECT
}
```

---

#### Transformation Logic (Pseudocode)

```csharp
// In SelectCaseConverter.cs : ISelectCaseConverter - Separate converter (DatalistConverter handles DatalistNode only)
public string Convert(SelectCaseNode selectCase, string character, string situation)
{
    var branches = new List<object>();

    // Transform CASE branches
    foreach (var caseBranch in selectCase.Branches)
    {
        // Build OR condition from values
        ICondition? condition = null;
        foreach (var value in caseBranch.Values)
        {
            var argRef = new ArgRef { Index = 0, Operator = "==", Value = value };
            if (condition == null)
                condition = argRef;
            else
                condition = new LogicalOp { Left = condition, Operator = "||", Right = argRef };
        }

        // Extract lines from body (PRINTFORML nodes)
        var lines = ExtractLinesFromBody(caseBranch.Body);

        // Create branch dict
        var branch = new Dictionary<string, object> { { "lines", lines } };
        if (condition != null)
        {
            branch["condition"] = _conditionSerializer.ConvertConditionToYaml(condition); // { ARG: { 0: { eq: "13" } } }
        }
        branches.Add(branch);
    }

    // Transform CASEELSE (if present)
    // CASEELSE body may contain nested IfNode (e.g., IF EQUIP conditions)
    // Must recursively convert nested IF to sub-branches, not flatten to single fallback
    if (selectCase.CaseElse != null)
    {
        var nestedIfs = selectCase.CaseElse.OfType<IfNode>().ToList();
        if (nestedIfs.Any())
        {
            // Recursive sub-branch conversion (reuse ConvertConditionalBranches pattern)
            var subBranches = ConvertConditionalBranches(nestedIfs);
            branches.AddRange(subBranches);
        }
        else
        {
            var lines = ExtractLinesFromBody(selectCase.CaseElse);
            var elseBranch = new Dictionary<string, object> { { "lines", lines } }; // No condition
            branches.Add(elseBranch);
        }
    }

    // Convert to entries format
    var entries = BranchesToEntriesConverter.Convert(branches);

    // Serialize to YAML (same pattern as FileConverter.ConvertConditionalNode)
    var dialogueData = new Dictionary<string, object>
    {
        { "character", character },
        { "situation", situation },
        { "entries", entries }
    };
    var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
    return serializer.Serialize(dialogueData);
}
```

---

#### Condition ID Generation Enhancement

```csharp
// In BranchesToEntriesConverter.GenerateId()
private static string GenerateId(Dictionary<string, object>? condition, int index)
{
    if (condition == null || condition.Count == 0)
        return "fallback";

    // Handle compound conditions
    if (condition.ContainsKey("AND"))
        return $"and_compound_{index}";
    if (condition.ContainsKey("OR"))
        return $"or_compound_{index}";
    if (condition.ContainsKey("NOT"))
        return $"not_compound_{index}";

    // Extract condition type and generate semantic ID with branch index to prevent collisions
    if (condition.ContainsKey("TALENT"))
    {
        var talentDict = (Dictionary<string, object>)condition["TALENT"];
        var talentId = talentDict.Keys.First();
        return $"talent_{talentId}_{index}";
    }
    // ... existing ABL handler, then fallback ...

    // NEW: Handle ARG conditions
    if (condition.ContainsKey("ARG"))
    {
        var argDict = (Dictionary<string, object>)condition["ARG"];
        var argIndex = argDict.Keys.First(); // "0" for ARG, "1" for ARG:1
        return $"arg_{argIndex}_{index}";
    }

    return $"condition_{index}";
}
```

---

#### Edge Cases and Validation

| Case | Handling |
|------|----------|
| Empty CASE values (CASE with no value list) | ParseException: "CASE without values" |
| CASE after CASEELSE | ParseException: "CASE after CASEELSE" (ERA syntax violation) |
| Multiple CASEELSE | ParseException: "Multiple CASEELSE branches" (ERA syntax violation) |
| SELECTCASE with no CASE branches | Valid (like IF without ELSE); CASEELSE becomes only branch |
| Nested SELECTCASE | Out of scope — no nested SELECTCASE in WC系口上 corpus. Future support requires explicit recursive dispatch in body-parsing loop (ParseSelectCaseBlock call inside CASE/CASEELSE body parser) |
| CASE value with non-integer | Stored as string; transformation to ArgRef(Value="value") preserves original; runtime evaluation may fail if subject is non-numeric (out of scope for F765 - all 7 instances use integer ARG) |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-09 21:09 | START | implementer | Phase 3 TDD - Create tests only | - |
| 2026-02-09 21:09 | END | implementer | Phase 3 TDD - Create tests only | SUCCESS |
| 2026-02-09 21:13 | START | implementer | Task 1 - AST types already exist from Phase 3 | - |
| 2026-02-09 21:13 | END | implementer | Task 1 - Mark complete | SUCCESS |
| 2026-02-09 21:13 | START | implementer | Task 2 - Implement ParseSelectCaseBlock | - |
| 2026-02-09 21:13 | END | implementer | Task 2 - Build passed, tests passed (3/3) | SUCCESS |
| 2026-02-09 21:22 | START | implementer | Task 3 - Extract ConditionSerializer, create SelectCaseConverter, ARG handlers | - |
| 2026-02-09 21:22 | END | implementer | Task 3 - All tests passed (128/128 ErbToYaml.Tests) | SUCCESS |
| 2026-02-09 21:30 | START | orchestrator | Task 4 - KojoComparer tests (created Phase 3, already GREEN) | - |
| 2026-02-09 21:30 | END | orchestrator | Task 4 - 3/3 tests pass (capability verification) | SUCCESS |
| 2026-02-09 21:30 | START | ac-tester | Task 5 - Build verification + tech debt check | - |
| 2026-02-09 21:30 | END | ac-tester | Task 5 - 0 warnings, 0 debt markers | SUCCESS |

---

## Links
- [feature-762.md](feature-762.md) - Parent feature (ARG parser pipeline)
- [feature-758.md](feature-758.md) - Grandparent feature (prefix-based variable type expansion)
- [feature-764.md](feature-764.md) - Sibling feature (EVENT function conversion pipeline)
- [feature-761.md](feature-761.md) - Related feature (LOCAL variable parsing)
- [feature-759.md](feature-759.md) - Related feature (compound bitwise conditions)
- [feature-706.md](feature-706.md) - Successor feature (full equivalence testing)
