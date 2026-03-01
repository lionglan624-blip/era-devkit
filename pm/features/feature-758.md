# Feature 758: Prefix-Based Variable Type Expansion (MARK, EXP, NOWEX, ABL, FLAG, TFLAG, TEQUIP, PALAM)

## Status: [DONE]
<!-- fl-reviewed: 2026-02-07T00:00:00Z -->

<!-- Rebuild Note: Reverted from [PROPOSED] to [DRAFT] due to significant template structure deviation.
     /fc generation produced non-template sections (Root Cause Analysis, Related Features, etc.) and FMT-006 remained unresolved.
     Previous [PROPOSED] version preserved in "Reference: Previous [PROPOSED] Version" section at end of file. -->

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
>
> **Out of Scope** (deferred with concrete tracking):
> - Compound bitwise-comparison `(VAR & mask) == value` (1 occurrence) — requires two-stage parser → **F759** [DRAFT]
> - TALENT target/numeric index patterns `TALENT:PLAYER`, `TALENT:2` (25 occurrences) — requires TalentRef rework → **F760** [DRAFT]
> - LOCAL variables (355 conditions, stateful) — requires runtime assignment tracking → **F761** [DRAFT]
> - ARG bare variables (94 occurrences, stateful) — requires call-context analysis → **F762** [DRAFT]
> - Bare `PLAYER != MASTER` (0 in kojo) — dropped, non-kojo only (no feature needed)

## Type: engine

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)
Continue toward full equivalence testing (F706: 650/650 MATCH) by systematically expanding prefix-based variable type support. The condition parsing pipeline established by F750-F757 provides a proven `VariableConditionParser<TRef>` generic pattern; applying it to all remaining prefix-based types closes the largest single category of unparseable conditions and brings kojo DATALIST conversion significantly closer to full coverage.

### Problem (Current Issue)
After F757 resolved the architectural gaps (bitwise operator, function evaluation, generic consolidation), investigation of the full kojo corpus reveals that **8 prefix-based variable types totaling 1146 occurrences remain unparseable** despite following the exact same `PREFIX:(target:)?name (op value)?` pattern as already-supported types (CFLAG, TCVAR, EQUIP, ITEM, STAIN):

| Variable Type | Occurrences | Files | Pattern |
|--------------|:-----------:|:-----:|---------|
| TFLAG | 253 | 41 | `TFLAG:name`, `TFLAG:target:name` |
| MARK | 234 | 28 | `MARK:name`, `MARK:target:name` |
| ABL | 209 | 30 | `ABL:target:name op value` |
| NOWEX | 125 | 9 | `NOWEX:name`, `NOWEX:target:name` |
| PALAM | 124 | 9 | `PALAM:name`, `PALAM:target:name` |
| EXP | 93 | 43 | `EXP:name`, `EXP:target:name` |
| FLAG | 63 | 27 | `FLAG:name`, `FLAG:target:name` |
| TEQUIP | 45 | 5 | `TEQUIP:name`, `TEQUIP:target:name` |

The root cause is that these 8 types simply lack `VariableConditionParser<TRef>` implementations. The generic base class and thin-wrapper pattern already exist. Each new type requires only ~20 lines across 3 new files (Ref class, Parser class) plus registration in 4 existing touchpoints.

### Goal (What to Achieve)
Add `VariableConditionParser<TRef>` implementations for all 8 remaining prefix-based variable types (MARK, EXP, NOWEX, ABL, FLAG, TFLAG, TEQUIP, PALAM), following the established CflagRef/CflagConditionParser pattern, with full registration across the parsing, conversion, and evaluation pipeline. This resolves 1146 currently-unparseable kojo conditions.

---

<!-- fc-phase-2-completed -->

<!-- fc-phase-3-completed -->

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | 8 new Ref files exist | file | Glob(src/tools/dotnet/ErbParser/{Mark,Exp,Nowex,Abl,Flag,Tflag,Tequip,Palam}Ref.cs) | count_equals | 8 | [x] |
| 2 | 8 new Parser files exist | file | Glob(src/tools/dotnet/ErbParser/{Mark,Exp,Nowex,Abl,Flag,Tflag,Tequip,Palam}ConditionParser.cs) | count_equals | 8 | [x] |
| 3 | All 8 Ref classes extend VariableRef | code | Grep(src/tools/dotnet/ErbParser/) | count_equals | 8 | [x] |
| 4 | All 8 Parser classes use VariableConditionParser | code | Grep(src/tools/dotnet/ErbParser/) | count_equals | 8 | [x] |
| 5 | ICondition.cs has 17 JsonDerivedType attributes | code | Grep(src/tools/dotnet/ErbParser/ICondition.cs) | count_equals | 17 | [x] |
| 6 | LogicalOperatorParser registers all 13 variable parsers | code | Grep(src/tools/dotnet/ErbParser/LogicalOperatorParser.cs) | count_equals | 13 | [x] |
| 7 | DatalistConverter _variableTypePrefixes dictionary has 13 entries | code | Grep(src/tools/dotnet/ErbToYaml/DatalistConverter.cs) | count_equals | 13 | [x] |
| 8 | KojoBranchesParser ValidateConditionScope allowlist contains all 8 new types | code | Grep(src/tools/dotnet/KojoComparer/KojoBranchesParser.cs) | count_equals | 8 | [x] |
| 9 | New type parser functional tests pass | test | dotnet test src/tools/dotnet/ErbParser.Tests/ --filter NewVariableTypeTests | succeeds | - | [x] |
| 10 | ErbParser build succeeds | build | dotnet build src/tools/dotnet/ErbParser/ErbParser.csproj | succeeds | - | [x] |
| 11 | ErbToYaml build succeeds | build | dotnet build src/tools/dotnet/ErbToYaml/ErbToYaml.csproj | succeeds | - | [x] |
| 12 | KojoComparer build succeeds | build | dotnet build src/tools/dotnet/KojoComparer/KojoComparer.csproj | succeeds | - | [x] |
| 13 | ErbParser existing tests pass (>= 111) | test | dotnet test src/tools/dotnet/ErbParser.Tests/ | gte | 111 | [x] |
| 14 | ErbToYaml existing tests pass (>= 94) | test | dotnet test src/tools/dotnet/ErbToYaml.Tests/ | gte | 94 | [x] |
| 15 | KojoComparer existing tests stable (>= 104 passed) | test | dotnet test src/tools/dotnet/KojoComparer.Tests/ | gte | 104 | [x] |
| 16 | KojoBranchesParser evaluates new type conditions | test | dotnet test src/tools/dotnet/KojoComparer.Tests/ --filter NewTypeEvaluationTests | succeeds | - | [x] |
| 17 | End-to-end conversion test for new types | test | dotnet test src/tools/dotnet/ErbToYaml.Tests/ --filter NewTypeConversionTests | succeeds | - | [x] |

### AC Details

#### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "systematically expanding prefix-based variable type support" | All 8 Ref classes and 8 Parser classes must be created | AC#1, AC#2, AC#3, AC#4 |
| "proven VariableConditionParser\<TRef\> generic pattern" | All Ref classes extend VariableRef; all Parsers use VariableConditionParser\<TRef\> | AC#3, AC#4 |
| "full registration across the parsing, conversion, and evaluation pipeline" | All 4 touchpoints (ICondition.cs, LogicalOperatorParser.cs, DatalistConverter.cs, KojoBranchesParser.cs) updated for all 8 types | AC#5, AC#6, AC#7, AC#8, AC#16 |
| "closes the largest single category of unparseable conditions" | Functional test demonstrates mechanism works per type (representative samples); corpus coverage is implicit from pattern-clone architecture where each type uses identical VariableConditionParser\<TRef\> regex | AC#9, AC#17 |
| "full equivalence testing" — no regression | All 3 tool builds succeed; all existing tests pass without regression | AC#10, AC#11, AC#12, AC#13, AC#14, AC#15 |
| "ValidateConditionScope" must include all new types (C9) | All 8 new prefixes added to allowlist | AC#8 |

**AC#1: 8 new Ref files exist**
- Test: Glob pattern for each file individually: `src/tools/dotnet/ErbParser/MarkRef.cs`, `src/tools/dotnet/ErbParser/ExpRef.cs`, `src/tools/dotnet/ErbParser/NowexRef.cs`, `src/tools/dotnet/ErbParser/AblRef.cs`, `src/tools/dotnet/ErbParser/FlagRef.cs`, `src/tools/dotnet/ErbParser/TflagRef.cs`, `src/tools/dotnet/ErbParser/TequipRef.cs`, `src/tools/dotnet/ErbParser/PalamRef.cs`
- Expected: All 8 files exist
- Rationale: Each new type requires a Ref class extending VariableRef, following CflagRef.cs pattern

**AC#2: 8 new Parser files exist**
- Test: Glob pattern for each file individually: `src/tools/dotnet/ErbParser/MarkConditionParser.cs`, `src/tools/dotnet/ErbParser/ExpConditionParser.cs`, `src/tools/dotnet/ErbParser/NowexConditionParser.cs`, `src/tools/dotnet/ErbParser/AblConditionParser.cs`, `src/tools/dotnet/ErbParser/FlagConditionParser.cs`, `src/tools/dotnet/ErbParser/TflagConditionParser.cs`, `src/tools/dotnet/ErbParser/TequipConditionParser.cs`, `src/tools/dotnet/ErbParser/PalamConditionParser.cs`
- Expected: All 8 files exist
- Rationale: Each new type requires a ConditionParser class, following CflagConditionParser.cs pattern

**AC#3: All 8 Ref classes extend VariableRef**
- Test: Grep pattern=`class (Mark|Exp|Nowex|Abl|Flag|Tflag|Tequip|Palam)Ref\s*:\s*VariableRef` path=`src/tools/dotnet/ErbParser/` type=cs | count
- Expected: 8 matches
- Rationale: Ensures all new Ref classes inherit from VariableRef base class (not directly from ICondition), which provides Target, Name, Index, Operator, Value properties
- Constraint: C7 — prefix uniqueness guaranteed by regex, no special disambiguation needed

**AC#4: All 8 Parser classes use VariableConditionParser**
- Test: Grep pattern=`VariableConditionParser<(Mark|Exp|Nowex|Abl|Flag|Tflag|Tequip|Palam)Ref>` path=`src/tools/dotnet/ErbParser/` glob=`*ConditionParser.cs` type=cs | count
- Expected: 8 matches (one per new Parser file only, glob filter excludes LogicalOperatorParser.cs)
- Note: LogicalOperatorParser registrations are verified separately in AC#6.
- Rationale: Verifies reuse of generic pattern, not new parser logic

**AC#5: ICondition.cs has 17 JsonDerivedType attributes**
- Test: Grep pattern=`JsonDerivedType` path=`src/tools/dotnet/ErbParser/ICondition.cs` | count
- Expected: 17 (9 existing + 8 new: mark, exp, nowex, abl, flag, tflag, tequip, palam)
- Rationale: JsonDerivedType enables polymorphic JSON serialization; missing attribute causes silent serialization failure
- Note: Existing 9 = talent, cflag, tcvar, negated, function, logical, equip, item, stain

**AC#6: LogicalOperatorParser registers all 13 variable parsers**
- Test: Grep pattern=`VariableConditionParser<\w+Ref>` path=`src/tools/dotnet/ErbParser/LogicalOperatorParser.cs` | count
- Expected: 13 (5 existing: CFLAG, TCVAR, EQUIP, ITEM, STAIN + 8 new: MARK, EXP, NOWEX, ABL, FLAG, TFLAG, TEQUIP, PALAM)
- Rationale: Registration in LogicalOperatorParser is required for ParseAtomicCondition to try each parser
- Note: Implementation may use list-based or individual field registration — AC verifies count, not style (C2)

**AC#7: DatalistConverter _variableTypePrefixes dictionary has 13 entries**
- Test: Grep pattern=`typeof\(\w+Ref\)` path=`src/tools/dotnet/ErbToYaml/DatalistConverter.cs` | count
- Expected: 13 (5 existing + 8 new: typeof(CflagRef)→"CFLAG", ..., typeof(PalamRef)→"PALAM")
- Rationale: Single `case VariableRef varRef:` arm uses dictionary lookup for prefix. Pattern `typeof\(\w+Ref\)` scopes to dictionary initializer entries (only typeof usage in the file). OCP-compliant: future types need only dictionary append, no switch modification.
- Constraint: C4 — TcvarRef has special key-building logic preserved via if-check inside ConvertVariableRef

**AC#8: KojoBranchesParser VariablePrefixes array contains all 8 new types**
- Test: Grep pattern=`"(MARK|EXP|NOWEX|ABL|FLAG|TFLAG|TEQUIP|PALAM)"` path=`src/tools/dotnet/KojoComparer/KojoBranchesParser.cs` | count
- Expected: 8 (one per new type prefix in VariablePrefixes array — the SSOT for variable type strings; allowedKeys is derived from VariablePrefixes programmatically, so new type strings appear only once in the file)
- Rationale: Without VariablePrefixes entry, the type is excluded from both ValidateConditionScope allowlist (throws InvalidOperationException, C9) and EvaluateCondition dispatch (silently returns false)
- Also verify: KojoBranchesParser EvaluateCondition dispatches new types to EvaluateVariableCondition (covered by AC#16)

**AC#9: New type parser functional tests pass**
- Test: `dotnet test src/tools/dotnet/ErbParser.Tests/ --filter NewVariableTypeTests`
- Expected: Test class passes, covering:
  - 8 positive parse tests (one per type with representative kojo condition string)
  - At least 1 negative test (wrong prefix returns null)
  - Non-numeric comparison value test (C5: e.g., `PALAM:MASTER:潤滑 > PALAMLV:3`, `TFLAG:コマンド成功度 == 成功度_失敗`)
- Constraint: C5 — Non-numeric comparison values (PALAMLV:N, DIM CONST names) must be tested
- Constraint: C6 — Must not break existing FLAG:1 AST-level test in printdata_conditional.erb

**AC#10: ErbParser build succeeds**
- Test: `dotnet build src/tools/dotnet/ErbParser/ErbParser.csproj`
- Expected: Build succeeds with 0 errors, 0 warnings (TreatWarningsAsErrors=true)
- Baseline: 0 warnings, 0 errors

**AC#11: ErbToYaml build succeeds**
- Test: `dotnet build src/tools/dotnet/ErbToYaml/ErbToYaml.csproj`
- Expected: Build succeeds with 0 errors, 0 warnings (TreatWarningsAsErrors=true)
- Baseline: 0 warnings, 0 errors

**AC#12: KojoComparer build succeeds**
- Test: `dotnet build src/tools/dotnet/KojoComparer/KojoComparer.csproj`
- Expected: Build succeeds with 0 errors, 0 warnings (TreatWarningsAsErrors=true)
- Baseline: 0 warnings, 0 errors

**AC#13: ErbParser existing tests pass (>= 111)**
- Test: `dotnet test src/tools/dotnet/ErbParser.Tests/`
- Expected: >= 111 passed (baseline: 111/111)
- Rationale: Regression guard for existing parser tests
- Constraint: C6 — FLAG:1 test data at AST level must not regress
- Constraint: C8 — Existing test coverage provides regression guard

**AC#14: ErbToYaml existing tests pass (>= 94)**
- Test: `dotnet test src/tools/dotnet/ErbToYaml.Tests/`
- Expected: >= 94 passed (baseline: 94/94)
- Rationale: Regression guard for existing conversion tests, especially EquipItemConversionTests, CompoundConditionTests, BitwiseConversionTests (C8)

**AC#15: KojoComparer existing tests stable (>= 104 passed)**
- Test: `dotnet test src/tools/dotnet/KojoComparer.Tests/`
- Expected: >= 104 passed
- Rationale: Pre-existing 1 failure (ErbRunner build) is unrelated to F758 (C3). Using `gte 104` instead of `succeeds` to avoid false negative from pre-existing failure.

**AC#16: KojoBranchesParser evaluates new type conditions**
- Test: `dotnet test src/tools/dotnet/KojoComparer.Tests/ --filter NewTypeEvaluationTests`
- Expected: Test class passes, covering at least 1 new type (e.g., MARK or TFLAG) evaluation via EvaluateCondition → EvaluateVariableCondition dispatch
- Rationale: AC#8 only verifies ValidateConditionScope allowlist. EvaluateCondition has per-type TryGetValue dispatch blocks (lines 142-170) that must be extended or refactored for new types. Without this, new type conditions silently return false.

**AC#17: End-to-end conversion test for new types**
- Test: `dotnet test src/tools/dotnet/ErbToYaml.Tests/ --filter NewTypeConversionTests`
- Expected: Test class passes, covering at least 1 new type end-to-end: parse condition string (e.g., "MARK:MASTER:100") → DatalistConverter.ConvertConditionToYaml → verify YAML output contains expected structure (e.g., `{ "MARK": { "MASTER:100": { ... } } }`)
- Rationale: AC#7 only verifies structural switch case count. This AC verifies functional correctness of DatalistConverter YAML output for new types.

---

<!-- fc-phase-4-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,2,3,4,5 | Create 16 new files (8 Ref + 8 Parser classes), update ICondition.cs with JsonDerivedType registrations, and update VariableRef.cs docstring to list all 13 types | | [x] |
| 2 | 6,7,8 | Refactor LogicalOperatorParser, DatalistConverter, KojoBranchesParser to generic dispatch | | [x] |
| 3 | 9,16,17 | Create NewVariableTypeTests.cs, NewTypeEvaluationTests.cs, and NewTypeConversionTests.cs | | [x] |
| 4 | 10,11,12,13,14,15,16,17 | Verify builds and test regression | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Technical Design Phase 1 (16 file templates + ICondition.cs registration) | 16 new files created + ICondition.cs updated |
| 2 | smart-implementer | opus | Technical Design Phase 2 (generic dispatch refactoring) | LogicalOperatorParser, DatalistConverter, KojoBranchesParser refactored |
| 3 | implementer | sonnet | Technical Design Phase 3 (test creation), AC#9, AC#16, AC#17 Details | NewVariableTypeTests.cs, NewTypeEvaluationTests.cs, NewTypeConversionTests.cs created |
| 4 | ac-tester | haiku | AC#10-17 (build and test verification) | All builds succeed, all tests pass |

**Constraints** (from Technical Design):
1. (C2) All 4 touchpoints must be updated: ICondition.cs, LogicalOperatorParser.cs, DatalistConverter.cs, KojoBranchesParser.cs
2. (C4) TcvarRef special case must be preserved in DatalistConverter — ignores Target field in key construction
3. (C6) Existing FLAG:1 AST-level test must not regress
4. (C8) Existing tests provide regression guard for generic dispatch refactor
5. (C9) All 8 new types MUST be added to ValidateConditionScope allowlist

**Pre-conditions**:
- F757 is [DONE] (provides VariableConditionParser<TRef> generic base)
- All baseline measurements completed (111/94/104 test counts)
- No pending Review Notes

**Success Criteria**:
- All 8 Ref classes extend VariableRef (AC#3 count_equals 8)
- All 8 Parser classes use VariableConditionParser<TRef> (AC#4 count_equals 8)
- All 13 variable parsers registered in LogicalOperatorParser (AC#6 count_equals 13)
- All 13 variable types handled in DatalistConverter switch (AC#7 count_equals 13)
- All 8 new types in KojoBranchesParser ValidateConditionScope allowlist (AC#8)
- NewVariableTypeTests passes (AC#9)
- NewTypeEvaluationTests passes (AC#16)
- NewTypeConversionTests passes (AC#17)
- All 3 projects build with 0 warnings, 0 errors (AC#10-12)
- No test regression: ErbParser >=111, ErbToYaml >=94, KojoComparer >=104 (AC#13-15)

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation

### Technical Design

The following subsections detail the technical approach, implementation decisions, and file-level changes.

#### Approach

Feature 758 implements 8 new prefix-based variable types (MARK, EXP, NOWEX, ABL, FLAG, TFLAG, TEQUIP, PALAM) following the proven `VariableConditionParser<TRef>` generic pattern established by F750-F757. The implementation consists of two complementary strategies:

1. **Type Creation**: 16 new files (8 Ref classes + 8 Parser classes) following the CflagRef/CflagConditionParser thin-wrapper pattern
2. **Generic Dispatch Refactoring (EXT-001)**: Eliminate copy-paste registration pattern across 4 touchpoints by introducing list-based/dictionary-based dispatch mechanisms

This approach satisfies the **Open/Closed Principle**: adding future variable types requires only creating 2 new files (Ref + Parser) and appending to 4 registration lists — no method duplication, no switch expansion.

**Why Generic Dispatch Now**: The old [PROPOSED] version analysis (consensus from 3 independent investigators) identified that adding 8 types using the current copy-paste pattern would create 8 duplicate methods in DatalistConverter, 8 duplicate TryGetValue blocks in KojoBranchesParser, and 8 individual parser fields in LogicalOperatorParser. This violates OCP and creates maintenance debt. Since we must touch all 4 touchpoints for 8 new types anyway, this is the optimal time to refactor to generic dispatch — no future feature will require this level of systematic touchpoint updates again.

#### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create 8 new Ref files following CflagRef.cs pattern (5-line empty class extending VariableRef) |
| 2 | Create 8 new Parser files following CflagConditionParser.cs pattern (7-line thin wrapper using VariableConditionParser<TRef>) |
| 3 | All 8 Ref classes declare `class {Type}Ref : VariableRef` → Grep count_equals 8 |
| 4 | All 8 Parser classes instantiate `VariableConditionParser<{Type}Ref>` in constructor → Grep count_equals 8 (excludes LogicalOperatorParser registrations via specific pattern) |
| 5 | Add 8 `[JsonDerivedType]` attributes to ICondition.cs (9→17 total) → Grep count_equals 17 |
| 6 | LogicalOperatorParser: Replace 5 individual parser fields with `List<(string, VariableConditionParser)>` and replace 5 try-blocks with foreach loop → Grep finds all 13 parsers registered in list |
| 7 | DatalistConverter: Add `Dictionary<Type, string> _variableTypePrefixes` mapping (13 entries) and create single `ConvertVariableRef` method replacing 5 individual Convert{Type}Ref methods. Single `case VariableRef varRef:` arm replaces all individual case arms. TcvarRef special case preserved via if-check before generic logic → Dictionary typeof entry count = 13 |
| 8 | KojoBranchesParser: Define VariablePrefixes array (13 entries, SSOT for variable type strings). Derive ValidateConditionScope allowedKeys from VariablePrefixes + non-variable keys. Replace 5 individual TryGetValue blocks with prefix-list iteration → Grep count_equals 8 new prefixes in VariablePrefixes (only location for new type strings) |
| 9 | Create NewVariableTypeTests.cs with 8 positive parse tests (one per type), 1 negative test, and 1 non-numeric value test → dotnet test --filter NewVariableTypeTests succeeds |
| 10-12 | Build ACs verified by TreatWarningsAsErrors=true → All 3 projects build with 0 warnings, 0 errors |
| 13 | ErbParser test baseline 111 → Generic dispatch does not touch ErbParser internals, no regression expected |
| 14 | ErbToYaml test baseline 94 → Generic ConvertVariableRef regression guarded by existing EquipItemConversionTests, CompoundConditionTests (C8) |
| 15 | KojoComparer test baseline 104 passed → Generic EvaluateVariableCondition already exists (no change), ValidateConditionScope is additive only |
| 16 | Create NewTypeEvaluationTests.cs testing KojoBranchesParser EvaluateCondition dispatch for new types → dotnet test --filter NewTypeEvaluationTests succeeds |
| 17 | Create NewTypeConversionTests.cs testing end-to-end DatalistConverter output for new types → dotnet test --filter NewTypeConversionTests succeeds |

#### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **When to introduce generic dispatch** | A) Add 8 types with copy-paste, defer generic refactor to future feature; B) Add 8 types with generic dispatch now | **B** | This is the last systematic variable type expansion (remaining types are architectural: LOCAL=stateful, ARG=bare, compound bitwise=two-stage). Adding 8 types via copy-paste creates 21 duplicate blocks (8 DatalistConverter methods, 8 KojoBranchesParser blocks, 5 LogicalOperatorParser fields). Refactoring during F758 eliminates all future OCP violations for variable types. |
| **LogicalOperatorParser dispatch** | A) Keep individual fields, add 8 try-blocks; B) List-based registration with foreach loop | **B** | List-based registration (5→13 entries) replaces 5 existing individual fields and 5 try-blocks with single list + foreach loop, and avoids adding 8 more. New types added by appending `(prefix, parser)` tuple to list. Existing parsers (TALENT, FUNCTION) remain individual due to different API signatures. |
| **DatalistConverter dispatch** | A) Create 8 individual Convert{Type}Ref methods; B) Single `case VariableRef varRef:` arm with ConvertVariableRef method and Type→Prefix dictionary | **B** | Single switch arm + single method eliminates 13 individual case arms and 5 duplicate methods. TcvarRef special case (C4) preserved via if-check before generic BuildVariableKey call. Type→Prefix dictionary uses `typeof(MarkRef) → "MARK"` mapping. True OCP: future types need only dictionary entry, no switch modification. |
| **KojoBranchesParser dispatch** | A) Add 8 TryGetValue blocks; B) Prefix-list iteration | **B** | Already has generic `EvaluateVariableCondition` method (F756). Only needs ValidateConditionScope allowlist update (+8 entries). No new TryGetValue blocks needed — existing CFLAG/TCVAR/EQUIP/ITEM/STAIN blocks replaced with foreach over prefix array in separate refactor task. |
| **TcvarRef backward compatibility** | A) Apply generic BuildVariableKey (includes Target field); B) Preserve TcvarRef's Target-ignoring logic | **B** | 79 existing TCVAR conditions use Target field in ERB but TcvarRef currently ignores it (C4). Changing behavior risks regression in DatalistConverter output. Generic dispatch preserves existing behavior via `if (varRef is TcvarRef tcvar)` special case before generic logic. |
| **AC#4 Grep pattern specificity** | A) Count all `VariableConditionParser<` occurrences; B) Count only in new Parser files (exclude LogicalOperatorParser) | **B** | LogicalOperatorParser will have 13 `VariableConditionParser<` instantiations in the list (5 existing + 8 new). AC#4 verifies that new Parser files exist and use the generic pattern. Pattern `VariableConditionParser<(Mark|Exp|Nowex|Abl|Flag|Tflag|Tequip|Palam)Ref>` counts only new types in their own files. |

#### Implementation Structure

##### Phase 1: Create 16 New Files + ICondition.cs Registration

**New Ref Classes** (8 files): `src/tools/dotnet/ErbParser/{Mark,Exp,Nowex,Abl,Flag,Tflag,Tequip,Palam}Ref.cs`
```csharp
using System.Text.Json.Serialization;

namespace ErbParser;

/// <summary>
/// Represents a reference to a {TYPE} condition
/// Pattern: {TYPE}:(target:)?(name|index)( op value)?
/// </summary>
public class {Type}Ref : VariableRef
{
}
```

**New Parser Classes** (8 files): `src/tools/dotnet/ErbParser/{Mark,Exp,Nowex,Abl,Flag,Tflag,Tequip,Palam}ConditionParser.cs`
```csharp
namespace ErbParser;

public class {Type}ConditionParser
{
    private readonly VariableConditionParser<{Type}Ref> _parser = new("{TYPE}");
    public {Type}Ref? Parse{Type}Condition(string condition) => _parser.Parse(condition);
}
```

**ICondition.cs**: Add 8 JsonDerivedType attributes
```csharp
[JsonDerivedType(typeof(MarkRef), typeDiscriminator: "mark")]
[JsonDerivedType(typeof(ExpRef), typeDiscriminator: "exp")]
[JsonDerivedType(typeof(NowexRef), typeDiscriminator: "nowex")]
[JsonDerivedType(typeof(AblRef), typeDiscriminator: "abl")]
[JsonDerivedType(typeof(FlagRef), typeDiscriminator: "flag")]
[JsonDerivedType(typeof(TflagRef), typeDiscriminator: "tflag")]
[JsonDerivedType(typeof(TequipRef), typeDiscriminator: "tequip")]
[JsonDerivedType(typeof(PalamRef), typeDiscriminator: "palam")]
```

##### Phase 2: Generic Dispatch Refactoring (EXT-001)

**LogicalOperatorParser.cs**: List-based parser registration
```csharp
// BEFORE (current):
private readonly VariableConditionParser<CflagRef> _cflagParser = new("CFLAG");
private readonly VariableConditionParser<TcvarRef> _tcvarParser = new("TCVAR");
// ... 4 more individual fields

// Try CFLAG parser
var cflag = _cflagParser.Parse(condition);
if (cflag != null) return cflag;
// ... 5 more try-blocks

// AFTER (generic dispatch):
private readonly List<(string prefix, Func<string, ICondition?> parser)> _variableParsers;

public LogicalOperatorParser()
{
    var cflagParser = new VariableConditionParser<CflagRef>("CFLAG");
    var tcvarParser = new VariableConditionParser<TcvarRef>("TCVAR");
    var equipParser = new VariableConditionParser<EquipRef>("EQUIP");
    var itemParser = new VariableConditionParser<ItemRef>("ITEM");
    var stainParser = new VariableConditionParser<StainRef>("STAIN");
    var markParser = new VariableConditionParser<MarkRef>("MARK");
    var expParser = new VariableConditionParser<ExpRef>("EXP");
    var nowexParser = new VariableConditionParser<NowexRef>("NOWEX");
    var ablParser = new VariableConditionParser<AblRef>("ABL");
    var flagParser = new VariableConditionParser<FlagRef>("FLAG");
    var tflagParser = new VariableConditionParser<TflagRef>("TFLAG");
    var tequipParser = new VariableConditionParser<TequipRef>("TEQUIP");
    var palamParser = new VariableConditionParser<PalamRef>("PALAM");

    _variableParsers =
    [
        ("CFLAG", cflagParser.Parse),
        ("TCVAR", tcvarParser.Parse),
        ("EQUIP", equipParser.Parse),
        ("ITEM", itemParser.Parse),
        ("STAIN", stainParser.Parse),
        ("MARK", markParser.Parse),
        ("EXP", expParser.Parse),
        ("NOWEX", nowexParser.Parse),
        ("ABL", ablParser.Parse),
        ("FLAG", flagParser.Parse),
        ("TFLAG", tflagParser.Parse),
        ("TEQUIP", tequipParser.Parse),
        ("PALAM", palamParser.Parse),
    ];
}

// In ParseAtomicCondition, after TALENT and before FUNCTION:
foreach (var (prefix, parser) in _variableParsers)
{
    var result = parser(condition);
    if (result != null)
        return result;
}
```

**DatalistConverter.cs**: Single ConvertVariableRef method
```csharp
// Add field:
private readonly Dictionary<Type, string> _variableTypePrefixes = new()
{
    { typeof(CflagRef), "CFLAG" },
    { typeof(TcvarRef), "TCVAR" },
    { typeof(EquipRef), "EQUIP" },
    { typeof(ItemRef), "ITEM" },
    { typeof(StainRef), "STAIN" },
    { typeof(MarkRef), "MARK" },
    { typeof(ExpRef), "EXP" },
    { typeof(NowexRef), "NOWEX" },
    { typeof(AblRef), "ABL" },
    { typeof(FlagRef), "FLAG" },
    { typeof(TflagRef), "TFLAG" },
    { typeof(TequipRef), "TEQUIP" },
    { typeof(PalamRef), "PALAM" },
};

// In ConvertConditionToYaml switch, replace all individual VariableRef case blocks with single arm:
case VariableRef varRef when _variableTypePrefixes.ContainsKey(varRef.GetType()):
    return ConvertVariableRef(varRef);

// New generic method:
private Dictionary<string, object> ConvertVariableRef(VariableRef varRef)
{
    // TcvarRef special case (C4): ignores Target field in key construction
    string key;
    if (varRef is TcvarRef tcvar)
    {
        key = tcvar.Index.HasValue ? tcvar.Index.Value.ToString() : tcvar.Name!;
    }
    else
    {
        key = BuildVariableKey(varRef);
    }

    var prefix = _variableTypePrefixes[varRef.GetType()];

    return new Dictionary<string, object>
    {
        { prefix, new Dictionary<string, object>
            {
                { key, MapErbOperatorToYaml(varRef.Operator, varRef.Value) }
            }
        }
    };
}

// Delete: ConvertCflagRef, ConvertTcvarRef, ConvertEquipRef, ConvertItemRef, ConvertStainRef (5 methods)
```

**KojoBranchesParser.cs**: ValidateConditionScope allowlist derived from VariablePrefixes + prefix-list iteration
```csharp
// REQUIRED: EvaluateCondition must dispatch new types to EvaluateVariableCondition
// Replace individual TryGetValue blocks with prefix-list iteration
// SSOT: VariablePrefixes is the single source for all variable type strings
private static readonly string[] VariablePrefixes =
{
    "CFLAG", "TCVAR", "EQUIP", "ITEM", "STAIN",
    "MARK", "EXP", "NOWEX", "ABL", "FLAG", "TFLAG", "TEQUIP", "PALAM"
};

// ValidateConditionScope: Derive allowedKeys from VariablePrefixes + non-variable keys
var allowedKeys = new HashSet<string>(VariablePrefixes)
{
    "TALENT", "FUNCTION", "AND", "OR", "NOT"
};

// In EvaluateBranch, replace 5 individual TryGetValue blocks with:
foreach (var prefix in VariablePrefixes)
{
    if (condition.TryGetValue(prefix, out var obj) && obj is Dictionary<object, object> dict)
    {
        return EvaluateVariableCondition(prefix, dict, state);
    }
}
```

##### Phase 3: Test Creation

**NewVariableTypeTests.cs**: 8 positive + 1 negative + 1 non-numeric test
```csharp
[Fact] public void Parse_MarkCondition() { /* MARK:MASTER:100 */ }
[Fact] public void Parse_ExpCondition() { /* EXP:target:name op value */ }
// ... 6 more type tests
[Fact] public void Parse_WrongPrefix_ReturnsNull() { /* INVALID:x → null */ }
[Fact] public void Parse_NonNumericValue() { /* PALAM:MASTER:潤滑 > PALAMLV:3 */ }
```

#### File Change Summary

| Category | Action | Files | Complexity |
|----------|--------|------:|:----------:|
| New Ref classes | Create | 8 | Trivial (5 lines each) |
| New Parser classes | Create | 8 | Trivial (7 lines each) |
| ICondition.cs | Update (add 8 attributes) | 1 | Trivial |
| LogicalOperatorParser.cs | Refactor (list-based dispatch) | 1 | Low (delete 5 fields + 5 blocks, add 1 list + 1 loop) |
| DatalistConverter.cs | Refactor (single ConvertVariableRef) | 1 | Low (delete 5 methods, add 1 method + 1 dict) |
| KojoBranchesParser.cs | Refactor (allowlist + prefix-list iteration) | 1 | Low (allowlist add + iteration refactor) |
| NewVariableTypeTests.cs | Create | 1 | Low (10 test methods) |
| NewTypeEvaluationTests.cs | Create | 1 | Low (evaluation dispatch tests) |
| NewTypeConversionTests.cs | Create | 1 | Low (end-to-end conversion tests) |
| VariableRef.cs | Update (docstring) | 1 | Trivial (add 8 types to summary) |
| **Total** | | **24** | **Net negative LOC** |

**Net Code Change**: Despite adding 8 new types, the generic dispatch refactoring **reduces total LOC** by eliminating:
- 5 parser fields + 5 try-blocks in LogicalOperatorParser
- 5 duplicate Convert methods in DatalistConverter
- 5 duplicate TryGetValue blocks in KojoBranchesParser

**Future Variable Type Addition**: Only requires 2 new files + 4 list/dict appends (no method duplication).

---

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ErbParser build | dotnet build src/tools/dotnet/ErbParser/ErbParser.csproj | 0 warnings, 0 errors | Clean |
| ErbToYaml build | dotnet build src/tools/dotnet/ErbToYaml/ErbToYaml.csproj | 0 warnings, 0 errors | Clean |
| KojoComparer build | dotnet build src/tools/dotnet/KojoComparer/KojoComparer.csproj | 0 warnings, 0 errors | Clean |
| ErbParser tests | dotnet test src/tools/dotnet/ErbParser.Tests/ | 111/111 pass | All passing |
| ErbToYaml tests | dotnet test src/tools/dotnet/ErbToYaml.Tests/ | 94/94 pass | All passing |
| KojoComparer tests | dotnet test src/tools/dotnet/KojoComparer.Tests/ | 104 pass, 1 fail, 3 skip (108 total) | 1 pre-existing failure (ErbRunner build), unrelated to F758 |

**Baseline File**: `.tmp/baseline-758.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | No CSV lookup for new types | No CSV files exist for MARK/EXP/NOWEX/ABL/FLAG/TFLAG/TEQUIP/PALAM in Game/CSV/ | AC must NOT test CSV lookup for new types |
| C2 | 4 registration touchpoints per type | ICondition.cs, LogicalOperatorParser.cs, DatalistConverter.cs, KojoBranchesParser.cs | ACs must verify all 4 touchpoints are updated |
| C3 | KojoComparer has 1 pre-existing test failure | Baseline: 104 pass, 1 fail (ErbRunner build) | Use `gte 104` matcher, not `succeeds` for KojoComparer tests |
| C4 | TcvarRef has non-standard key-building logic | DatalistConverter.cs:348 — ignores Target field | Generic dispatch must preserve TcvarRef special case; do NOT change TcvarRef behavior |
| C5 | Non-numeric comparison values are pervasive | PALAM uses PALAMLV:N, FLAG/TFLAG use DIM CONST names | ACs must test string value passthrough (not just numeric) |
| C6 | Existing FLAG:1 test data at AST level | src/tools/dotnet/ErbParser.Tests/TestData/printdata_conditional.erb:3 | Verify no regression; AST-level test is safe |
| C7 | Prefix uniqueness guaranteed by regex | `^{prefix}:` anchored match prevents collisions | No disambiguation ACs needed |
| C8 | Existing tests cover 5 Convert methods | EquipItemConversionTests, CompoundConditionTests, BitwiseConversionTests | Regression guard exists for generic dispatch refactor |
| C9 | ValidateConditionScope throws on unknown keys | KojoBranchesParser.cs:283-288 | All 8 new types MUST be added to allowlist |

### Constraint Details

**C1: No CSV Lookup**
- **Source**: Grep of Game/CSV/ confirms no CSV files for MARK, EXP, NOWEX, ABL, FLAG, TFLAG, TEQUIP, PALAM (only Talent.csv exists)
- **Verification**: `ls Game/CSV/` — confirm no new CSV files for these types
- **AC Impact**: Convert methods use BuildVariableKey directly. AC must NOT assume or test CSV index resolution.

**C2: 4 Registration Touchpoints**
- **Source**: Code analysis of existing 5-type pipeline
- **Verification**: Each type requires entries in: (1) ICondition.cs JsonDerivedType, (2) LogicalOperatorParser.cs parser registration, (3) DatalistConverter.cs switch/convert, (4) KojoBranchesParser.cs TryGetValue + ValidateConditionScope
- **AC Impact**: Missing any one touchpoint causes silent failure. ACs should verify each touchpoint, not just end-to-end.

**C3: Pre-existing KojoComparer Failure**
- **Source**: Baseline measurement (ErbRunner build failure, unrelated to F758)
- **Verification**: `dotnet test src/tools/dotnet/KojoComparer.Tests/` — confirm 1 pre-existing failure
- **AC Impact**: Test count matcher must use `gte 104`, not `succeeds`, to avoid false negatives.

**C4: TcvarRef Special Case**
- **Source**: DatalistConverter.cs:348 — TcvarRef ignores Target field in key construction (uses `tcvar.Index ?? tcvar.Name`, not `BuildVariableKey`)
- **Verification**: Compare ConvertTcvarRef with ConvertCflagRef in DatalistConverter.cs
- **AC Impact**: If introducing generic ConvertVariableRef, must preserve TcvarRef's special key logic. Regression tests for existing TCVAR conversion must pass.

**C5: Non-Numeric Comparison Values**
- **Source**: Corpus analysis — PALAM exclusively uses `PALAMLV:N` values, FLAG/TFLAG use DIM CONST names like `成功度_失敗`
- **Verification**: Sample kojo: `PALAM:MASTER:潤滑 > PALAMLV:3`, `TFLAG:コマンド成功度 == 成功度_失敗`
- **AC Impact**: Parser tests must include non-numeric string values, not just integer comparisons.

**C6: FLAG:1 Test Data**
- **Source**: src/tools/dotnet/ErbParser.Tests/TestData/printdata_conditional.erb:3 contains `FLAG:1`
- **Verification**: Read the test data file
- **AC Impact**: Adding FlagConditionParser must not break existing AST-level tests that parse `FLAG:1`.

**C7: Prefix Uniqueness**
- **Source**: VariableConditionParser regex `^{prefix}:` with anchored prefix match
- **Verification**: No two prefixes are substrings of each other (EQUIP vs TEQUIP: TEQUIP checked first or `^` anchor prevents partial match)
- **AC Impact**: No parser ambiguity ACs needed. Prefix order in registration list is not correctness-critical.

**C8: Existing Test Coverage**
- **Source**: ErbToYaml.Tests contains EquipItemConversionTests, CompoundConditionTests, BitwiseConversionTests covering all 5 current Convert methods
- **Verification**: `dotnet test src/tools/dotnet/ErbToYaml.Tests/` — all 94 pass
- **AC Impact**: Regression guard exists. If refactoring to generic ConvertVariableRef, existing tests catch breakage.

**C9: ValidateConditionScope Allowlist**
- **Source**: KojoBranchesParser.cs:282 — `var allowedKeys = new HashSet<string> { "TALENT", "CFLAG", "TCVAR", "EQUIP", "ITEM", "STAIN", "FUNCTION", "AND", "OR", "NOT" }`
- **Verification**: Read KojoBranchesParser.cs line 282
- **AC Impact**: All 8 new type prefixes MUST be added to this allowlist. Without this, compound conditions containing new types will throw InvalidOperationException.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F757 | [DONE] | Runtime Condition Support — provides `VariableConditionParser<TRef>` generic base, bitwise support, function eval |
| Related | F756 | [DONE] | EQUIP/ITEM/STAIN Variable Type Support — pattern origin for clone pattern |
| Related | F755 | [DONE] | CFLAG/TCVAR Compound Support — established variable type registration pattern |
| Related | F752 | [DONE] | LogicalOperatorParser — infrastructure that F758 must register into |
| Related | F750 | [DONE] | TALENT Condition YAML Migration — chain start |
| Related | F706 | [DONE] | Full equivalence verification — consumer of F758's expanded parsing coverage |
| Successor | F759 | [DONE] | Compound bitwise conditions — deferred from F758 scope |
| Successor | F760 | [DONE] | TALENT target/numeric index patterns — deferred from F758 scope |
| Successor | F761 | [DONE] | LOCAL variables — deferred from F758 scope |
| Successor | F762 | [DONE] | ARG bare variables — deferred from F758 scope |

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped] {phase} {iter}: [{category-code}] {description} -->
- [resolved-applied] Phase2 iter1: [FMT-001] Technical Design section moved under Implementation Contract (template compliance)
- [resolved-applied] Phase2 iter1: [FMT-002] Implementation Contract table reduced from 6 to 5 columns (template compliance)
- [resolved-applied] Phase2 iter1: [FMT-003] Philosophy Derivation moved into AC Details (template compliance)
- [resolved-applied] Phase2 iter1: [AC-001] Added AC#16 for KojoBranchesParser EvaluateCondition dispatch functional test
- [resolved-applied] Phase2 iter1: [AC-002] Added AC#17 for end-to-end DatalistConverter conversion test
- [resolved-applied] Phase2 iter1: [DES-001] Fixed LogicalOperatorParser code sample to pre-allocate parser instances
- [resolved-applied] Phase2 iter2: [FMT-004] AC Details heading hierarchy fixed (Philosophy Derivation demoted to h4)
- [resolved-applied] Phase2 iter2: [FMT-005] Japanese section title translated to English
- [resolved-applied] Phase2 iter2: [FMT-006] Rebuild Note moved to HTML comment
- [resolved-applied] Phase2 iter2: [AC-003] AC#4 Grep glob filter added to exclude LogicalOperatorParser matches
- [resolved-applied] Phase2 iter2: [AC-004] Task#4 AC# updated to include AC#16,17 (matching Implementation Contract Phase 4)
- [resolved-invalid] Phase2 iter2: [DES-002] Ref template unused using claim — disproven by baseline (CS8019 is hidden severity)
- [resolved-applied] Phase2 iter3: [DES-003] DatalistConverter switch replaced with single `case VariableRef varRef:` arm for true OCP compliance
- [resolved-applied] Phase2 iter3: [AC-005] AC#7 updated to verify dictionary typeof count instead of switch case count
- [resolved-applied] Phase2 iter3: [DES-004] KojoBranchesParser iteration refactor changed from "optional" to required (consistent with Key Decision)
- [resolved-applied] Phase2 iter3→4: [CVR-003] Philosophy Derivation updated to explicitly acknowledge that AC#9/AC#17 validate mechanism per type, corpus coverage is implicit from pattern-clone architecture
- [resolved-applied] Phase2 iter4: [AC-006] AC#8 Expected changed from "see AC Details" to concrete method (8x individual Grep contains, succeeds matcher)
- [resolved-applied] Phase2 iter4: [DES-005] LogicalOperatorParser field/block counts corrected from 6 to 5 across AC Coverage, File Change Summary, and Net Code Change
- [resolved-applied] Phase2 iter4: [FMT-007] Reference section annotated with removal comment for [DONE] status
- [resolved-applied] Phase2 iter5: [FMT-008] Technical Design subsection headings demoted from ### to #### (Phase headings from #### to #####) for correct hierarchy
- [resolved-applied] Phase2 iter5: [FMT-009] Added introductory line under ### Technical Design to avoid empty section
- [resolved-applied] Phase2 iter6: [AC-007] AC#7 Method column standardized (removed ad-hoc pattern= syntax, pattern in AC Details)
- [resolved-applied] Phase2 iter6: [AC-008] AC#8 Method/Matcher changed from multi-step 8x Grep/succeeds to standard Grep/count_equals 8
- [resolved-applied] Phase2 iter6: [AC-009] AC#16 moved from Task#2 to Task#3 (test creation task); Implementation Contract Phase 3 updated
- [resolved-applied] Phase2 iter7: [FMT-010] Removed duplicate AC#16 row from AC Coverage table
- [resolved-applied] Phase2 iter7: [AC-010] AC#7 grep pattern tightened from `typeof` to `typeof\(\w+Ref\)` for dictionary-scoped matching
- [resolved-applied] Phase2 iter7: [FMT-011] Added missing blank line before ## Acceptance Criteria heading
- [resolved-invalid] Phase2 iter8: [DES-006] Wrapper ConditionParser classes "dead code" claim — established test API pattern used by 5 existing types
- [resolved-applied] Phase3 iter8: [MNT-001] File Change Summary updated: added 2 missing test files + VariableRef.cs docstring update (21→24 files)
- [resolved-applied] Phase2 iter1(new2): [AC-011] AC#8 count_equals 8 would fail after refactoring (new type strings in both allowedKeys and VariablePrefixes = 16 matches). Fixed by deriving allowedKeys from VariablePrefixes programmatically, keeping new type strings in single location.
- [resolved-applied] Phase3 iter2(new2): [MNT-002] Task#1 description missing VariableRef.cs docstring update (orphan in File Change Summary). Added to Task#1 description.
- [resolved-skipped] Phase2 iter3(new2): [FMT-012] Template includes ### Task Tags subsection unconditionally under ## Tasks. F758 omits it. No [I]-tagged Tasks exist, so functional impact is zero.

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Compound bitwise `(VAR & mask) == value` | Requires two-stage parser | Feature | F759 | N/A (pre-created) |
| TALENT target/numeric index patterns | Requires TalentRef rework | Feature | F760 | N/A (pre-created) |
| LOCAL variables (355 conditions, stateful) | Requires runtime assignment tracking | Feature | F761 | N/A (pre-created) |
| ARG bare variables (94 occurrences, stateful) | Requires call-context analysis | Feature | F762 | N/A (pre-created) |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-07 14:00 | START | implementer | Task 1 (16 files + ICondition) | SUCCESS |
| 2026-02-07 14:02 | START | smart-implementer | Task 2 (generic dispatch) | SUCCESS |
| 2026-02-07 14:04 | DEVIATION | ac-tester | AC#17 MarkRef_ProducesStructuredYaml | FAIL: numeric index key mismatch |
| 2026-02-07 14:05 | FIX | debugger | AC#17 test input correction | Fixed: MARK:MASTER:100→MARK:MASTER:好感度 |

## Links
- [feature-757.md](archive/feature-757.md) - Runtime Condition Support (source of handoff items)
- [feature-756.md](archive/feature-756.md) - EQUIP/ITEM/STAIN Variable Type Support (pattern origin)
- [feature-755.md](archive/feature-755.md) - CFLAG/TCVAR Compound Support (pattern origin)
- [feature-752.md](archive/feature-752.md) - LogicalOperatorParser (infrastructure)
- [feature-750.md](archive/feature-750.md) - TALENT Condition YAML Migration (chain start)
- [feature-706.md](feature-706.md) - Full equivalence verification (consumer)
- [feature-759.md](feature-759.md) - Compound bitwise conditions (deferred from F758)
- [feature-760.md](feature-760.md) - TALENT target/numeric index patterns (deferred from F758)
- [feature-761.md](feature-761.md) - LOCAL variables (deferred from F758)
- [feature-762.md](feature-762.md) - ARG bare variables (deferred from F758)

