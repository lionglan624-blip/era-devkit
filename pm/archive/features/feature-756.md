# Feature 756: Extended Compound Variable Type Support

## Status: [DONE]
<!-- fl-reviewed: 2026-02-06T00:00:00Z -->

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

## Background

### Philosophy (Mid-term Vision)
Extend compound condition support to statically parseable ERB variable types with comparison operators, advancing toward full equivalence testing (F706: 650/650 MATCH). F755 establishes CFLAG/TCVAR compound support and wires DatalistConverter to use existing ConditionExtractor/LogicalOperatorParser. F756 extends to EQUIP (277 occurrences in kojo) and ITEM (44), plus FunctionCall YAML passthrough (412 occurrences, data preservation in converter — evaluator enforces fail-fast InvalidOperationException boundary for FUNCTION conditions, runtime evaluation deferred to F757). STAIN is fully deferred to F757 (all 33 occurrences use bitwise `&` which requires tokenizer changes).

### Problem (Current Issue)
F755 deferred compound variable types beyond CFLAG/TCVAR. Investigation reveals two categories:

**Statically parseable (F756 scope)**:
1. EQUIP variables (277 occurrences in kojo) - same `PREFIX:target:name` pattern as CFLAG. No CSV loader needed.
2. ITEM variables (44 occurrences) - uses numeric index (`ITEM:2`)
3. FunctionCall YAML passthrough - already parsed by FunctionCallParser but ConvertConditionToYaml returns empty {}

**Deferred to F757** (requires bitwise `&` tokenizer changes):
4. STAIN variables (33 occurrences) - all use bitwise `&` operator, zero use comparison operators

**Runtime-only (deferred to separate feature)**:
5. LOCAL variables (787 occurrences) - runtime assignment (`LOCAL:1 = 0` then `IF LOCAL:1`), not a condition prefix
6. Bitwise `&` disambiguation from `&&` - requires tokenizer changes
7. Function call runtime evaluation (GETBIT, HAS_VAGINA, etc.) - cannot be statically resolved

### Goal (What to Achieve)
1. Create EQUIP/ITEM parsers following CflagConditionParser pattern
2. Add EQUIP/ITEM to ValidateConditionScope allowlist and EvaluateCondition dispatch
3. Implement ICondition→YAML conversion for new types in DatalistConverter
4. Implement FunctionCall→YAML opaque passthrough format
5. Register new parsers in LogicalOperatorParser.ParseAtomicCondition chain
6. Verify KojoBranchesParser can evaluate EQUIP/ITEM YAML in the format produced by DatalistConverter (contract test)

## Scope

Deferred from F755 analysis. Extends F755's CFLAG/TCVAR support to full compound condition landscape.

---

## Extensibility Hooks from F755

F755 creates the following extension points for F756:

| Hook | Location | How to Extend |
|------|----------|---------------|
| `ValidateConditionScope` allowlist | KojoBranchesParser.cs | Add new variable type keys (EQUIP, ITEM) to the HashSet |
| `EvaluateVariableCondition` shared method | KojoBranchesParser.cs | Call with new prefixes: `EvaluateVariableCondition("EQUIP", ...)` |
| `ConvertConditionToYaml` visitor | DatalistConverter.cs | Add new `case EquipRef:` / `case StainRef:` branches |
| `MapErbOperatorToYaml` shared helper | DatalistConverter.cs | Reuse for all new variable type converters |
| `ParseAtomicCondition` chain | LogicalOperatorParser.cs | Register new parsers (EquipConditionParser, etc.) |
| `ICondition` JsonDerivedType | ICondition.cs | Add new type registrations |
| Generic parser pattern | TcvarConditionParser/CflagConditionParser | Consider extracting `VariableConditionParser<T>` base to eliminate per-type duplication |

---

## Deferred Items from F755 (Corrected by Investigation)

**In F756 scope** (statically parseable with comparison operators):

| Item | Kojo Occurrences | Complexity |
|------|:----------------:|-----------|
| EQUIP parser + evaluator + converter | 277 across 30+ files | Low - same regex as CFLAG, no CSV needed |
| ITEM parser + evaluator + converter | 44 across 9 files | Low - numeric index pattern |
| FunctionCall YAML passthrough | 412 across 30+ files | Low - already parsed, needs YAML format |

**Deferred to F757** (requires bitwise `&` tokenizer changes):

| Item | Kojo Occurrences | Complexity |
|------|:----------------:|-----------|
| STAIN parser + evaluator + converter | 33 across 13 files | Medium - ALL occurrences use bitwise `&`, zero use comparison operators |

**Deferred to separate feature** (runtime-only):

| Item | Kojo Occurrences | Complexity |
|------|:----------------:|-----------|
| LOCAL variable support | 787 across 16 files | HIGH - runtime assignment, not declarative condition |
| Bitwise `&` disambiguation | 78 across 21 files | HIGH - tokenizer change affects all parsers |
| Function call runtime evaluation (GETBIT, HAS_VAGINA, etc.) | 412 across 30+ files | HIGH - requires runtime game state hooks |

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. **Why can't the system handle LOCAL/EQUIP/STAIN/ITEM variable types in conditions?**
   Because LogicalOperatorParser's `ParseAtomicCondition` chain only recognizes TALENT, CFLAG, TCVAR, and function call patterns. No parser exists for EQUIP, STAIN, ITEM, or LOCAL prefixes - they fall through to `return null`.

2. **Why are there no parsers for these variable types?**
   Because F755 (CFLAG/TCVAR) explicitly deferred them to F756. F755's scope was restricted to the two most impactful types (CFLAG: 1,249+ mixed occurrences; TCVAR: similar). EQUIP/STAIN/ITEM have fewer occurrences in DATALIST context.

3. **Why does the bitwise `&` operator cause problems?**
   Because `SplitOnOperator` in LogicalOperatorParser treats `&&` as a delimiter but has no handling for single `&` (bitwise AND). In ERB, `STAIN:奴隷:Ｖ & 汚れ_精液` is a bitwise mask check, fundamentally different from `TALENT:X && CFLAG:Y` (logical AND). The current tokenizer cannot disambiguate them.

4. **Why do function calls need special handling?**
   Because `FunctionCallParser` already exists and returns `FunctionCall` ICondition objects, but `DatalistConverter.ConvertConditionToYaml` currently just prints a warning and returns an empty dictionary for FunctionCall type. KojoBranchesParser has no code to evaluate function-type conditions. Functions like `HAS_VAGINA(TARGET)`, `GETBIT(...)`, `MASTER_POSE(...)` require runtime game state that cannot be statically resolved.

5. **Why is this a problem for equivalence testing (F706)?**
   Because any unrecognized condition type produces empty `{}` or null, causing incorrect branch selection in both migration (DatalistConverter) and evaluation (KojoBranchesParser). This leads to MISMATCH results in the 650/650 equivalence target.

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| EQUIP/STAIN/ITEM conditions produce empty {} in migrated YAML | No parser exists for these variable types in LogicalOperatorParser's ParseAtomicCondition chain |
| Bitwise `&` in STAIN conditions breaks tokenization | SplitOnOperator only handles `&&` (logical) - no disambiguation for single `&` (bitwise) |
| Function calls in conditions produce warning, empty output | ConvertConditionToYaml has FunctionCall case but returns empty dict; KojoBranchesParser cannot evaluate function conditions |
| LOCAL variable conditions cannot be migrated | LOCAL is a runtime assignment variable (LOCAL:1 = 0, then IF LOCAL:1), not a declarative condition - requires fundamentally different handling |
| Occurrence counts in original spec are inaccurate | Counts were estimated from F755 analysis, not verified by Grep against actual kojo directory |

### Conclusion

**Root cause: Missing parser implementations for variable types beyond CFLAG/TCVAR, plus fundamental architectural gaps for runtime-only constructs.**

The investigation reveals two distinct categories:

1. **Statically parseable types** (EQUIP, STAIN, ITEM): Follow the same `PREFIX:target:name` pattern as CFLAG. Can be handled by creating parsers following the CflagConditionParser pattern. No CSV loaders needed - these use string names or numeric indices directly (unlike TALENT which requires CSV index lookup). EQUIP uses `EQUIP:MASTER:下半身上着１`, STAIN uses `STAIN:奴隷:Ｖ`, ITEM uses `ITEM:2`.

2. **Runtime-only constructs** (LOCAL variables, function calls, bitwise operators): These CANNOT be statically migrated to YAML conditions. LOCAL:1 is assigned at runtime then tested. Function calls like `HAS_VAGINA(TARGET)`, `GETBIT(CFLAG:X, N)` require runtime evaluation. Bitwise `&` requires runtime value resolution. These need a different approach (opaque condition passthrough, runtime evaluation hooks, or scope exclusion).

**Critical correction to original spec**: The original Problem statement claims EQUIP/STAIN/ITEM "require separate CSV loaders." This is **incorrect**. No EQUIP/STAIN/ITEM CSV files exist in `Game/CSV/` (only `Talent.csv` exists). These types use string-based names or numeric indices directly, not CSV-mapped indices.

**Critical correction to occurrence counts**: The original counts (LOCAL=371, EQUIP=179, STAIN=21, ITEM=44, function=603) are inaccurate. Verified counts in `Game/ERB/口上/`:
- LOCAL: **787** occurrences across 16 files
- EQUIP: **277** occurrences across 30+ files
- STAIN: **33** occurrences across 13 files
- ITEM: **44** occurrences across 9 files
- Function calls (HAS_PENIS/FIRSTTIME/GETBIT/HAS_VAGINA): **412** across 30+ files
- GETBIT specifically: **193** across 13 files
- Bitwise `&` (single ampersand): **78** across 21 files

**Critical DATALIST context finding**: Most EQUIP/STAIN/ITEM/LOCAL conditions appear OUTSIDE DATALIST blocks (in WC系口上.ERB, SexHara*.ERB, EVENT files using PRINTDATA/PRINTDATAL). The DatalistConverter only processes DATALIST blocks. This means:
- EQUIP conditions: Primarily in WC系口上 and SexHara files (NOT DATALIST)
- STAIN conditions: In 会話親密 and NTR口上_お持ち帰り files, mostly OUTSIDE DATALIST
- ITEM conditions: Primarily in WC系口上 files (NOT DATALIST)
- LOCAL conditions: Used as runtime assignments in EVENT files (NOT DATALIST)

This significantly affects scope: the **parser** changes (ErbParser + LogicalOperatorParser) are needed for any future non-DATALIST conversion pipeline, while the **converter** changes (DatalistConverter) are only needed for the subset of these conditions that appear inside DATALIST blocks. The **evaluator** changes (KojoBranchesParser) are needed regardless, since any YAML file with these conditions must be evaluable.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F755 | [DONE] | Predecessor | CFLAG/TCVAR compound support - provides all extension points |
| F752 | [DONE] | Predecessor (indirect) | TALENT compound support - original foundation |
| F750 | [DONE] | Foundation | YAML TALENT condition migration - established migration pipeline |
| F706 | [PROPOSED] | Related | Full equivalence verification (650/650 target) - benefits from F756 output but does not list F756 as formal dependency |
| F754 | [DRAFT] | Related | YAML format unification (branches→entries) - may be affected by new condition types |

### Pattern Analysis

F750→F752→F755→F756 forms a progressive extension chain. Each feature extends the previous one's scope:
- F750: Single TALENT conditions
- F752: Compound TALENT conditions (AND/OR/NOT)
- F755: + CFLAG/TCVAR evaluation and migration
- F756: + EQUIP/STAIN/ITEM + function calls + bitwise operators

The pattern shows incremental scope expansion with explicit handoff via Deferred Items. This is the recommended approach: do not attempt all variable types in one feature.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | PARTIAL | EQUIP/STAIN/ITEM parsers: straightforward. Function calls/LOCAL/bitwise: fundamentally different approach needed |
| Scope is realistic | NO | Original scope includes runtime-only constructs (LOCAL, function calls, bitwise) that cannot be statically migrated using the same pattern. Scope must be split. |
| No blocking constraints | PARTIAL | No CSV loaders needed (correction). But bitwise `&` disambiguation requires tokenizer changes in SplitOnOperator, which affects all existing parsers. |

**Verdict**: NEEDS_REVISION

**Recommended scope revision**: Split F756 into two features:
1. **F756**: Statically parseable types only (EQUIP, ITEM) + FunctionCall YAML passthrough + ValidateConditionScope/EvaluateVariableCondition extensions. STAIN deferred to F757.
2. **New feature**: Runtime-only constructs (LOCAL, bitwise `&`, function call evaluation, GETBIT) requiring fundamentally different architecture

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F755 | [DONE] | CFLAG/TCVAR compound support - provides extension points |
| Related | F752 | [DONE] | TALENT compound support - foundation |
| Related | F706 | [PROPOSED] | Full equivalence verification - benefits from F756 output |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| Talent.csv | Data | Low | Only CSV file; EQUIP/STAIN/ITEM have no CSV files |
| ErbParser NuGet deps | Build | Low | System.Text.Json for JsonDerivedType |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/KojoComparer/KojoBranchesParser.cs | HIGH | Evaluates YAML conditions - must support new variable types |
| tools/ErbToYaml/DatalistConverter.cs | MEDIUM | Converts ERB conditions to YAML - needs new ICondition→YAML converters |
| tools/ErbParser/LogicalOperatorParser.cs | HIGH | ParseAtomicCondition chain - must register new parsers |
| tools/ErbParser/ICondition.cs | MEDIUM | JsonDerivedType registrations for new condition types |
| tools/YamlSchemaGen/ | LOW | YAML schema may need updates for new condition keys |

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbParser/EquipConditionParser.cs | Create | Parser for EQUIP:target:name pattern |
| tools/ErbParser/ItemConditionParser.cs | Create | Parser for ITEM:index pattern |
| tools/ErbParser/EquipRef.cs | Create | ICondition type for EQUIP references |
| tools/ErbParser/ItemRef.cs | Create | ICondition type for ITEM references |
| tools/ErbParser/ICondition.cs | Update | Add JsonDerivedType for EquipRef, ItemRef |
| tools/ErbParser/LogicalOperatorParser.cs | Update | Register new parsers in ParseAtomicCondition chain |
| tools/ErbToYaml/DatalistConverter.cs | Update | Add ConvertEquipRef/ConvertItemRef + FunctionCall passthrough |
| tools/KojoComparer/KojoBranchesParser.cs | Update | Add EQUIP/ITEM to ValidateConditionScope allowlist + EvaluateCondition dispatch |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| No CSV files for EQUIP/ITEM | Game/CSV/ only contains Talent.csv | HIGH - Original spec assumption of CSV loaders is wrong. String-based name resolution instead. |
| EQUIP/ITEM follow `PREFIX:target:name` pattern identical to CFLAG | Grep analysis of kojo ERB files | MEDIUM - Can reuse CflagConditionParser regex pattern with different prefix |
| STAIN fully deferred to F757 | All 33 STAIN occurrences use bitwise `&`, zero use comparison operators | HIGH - STAIN is inseparable from bitwise tokenizer changes |
| LOCAL is runtime assignment, not declarative | `LOCAL:1 = 0` then `IF LOCAL:1` | HIGH - Cannot be statically migrated. Fundamentally different from CFLAG/EQUIP/STAIN/ITEM patterns. |
| Function calls already parsed but not converted | FunctionCallParser exists, returns FunctionCall ICondition | MEDIUM - ConvertConditionToYaml needs FunctionCall→YAML conversion, but runtime evaluation remains unsupported |
| GETBIT takes CFLAG reference as argument | `GETBIT(CFLAG:TARGET:WC_道具既知フラグ,2)` | HIGH - Nested variable reference inside function call argument |
| EQUIP/STAIN conditions mostly outside DATALIST | Grep analysis: WC系口上, SexHara, 会話親密 | MEDIUM - DatalistConverter changes have limited impact; parser/evaluator changes have broader impact |
| KojoComparer pre-existing test failures | 2 pilot equiv + 1 file discovery (unrelated) | LOW - Do not count these as regressions |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Bitwise `&` disambiguation breaks existing `&&` parsing | High | High | Exclude bitwise from F756 scope; handle in separate feature |
| EQUIP condition patterns differ from CFLAG more than expected | Low | Medium | Regex already verified: same `PREFIX:target:name( op value)?` pattern |
| New parsers cause regression in LogicalOperatorParser | Medium | High | Existing 89 ErbParser tests provide regression safety net |
| Scope creep from trying to handle runtime constructs | High | High | Strict scope boundary: EQUIP/ITEM only, defer STAIN/LOCAL/bitwise/runtime |

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ErbParser build | dotnet build tools/ErbParser/ErbParser.csproj | 0 warnings, 0 errors | Clean build |
| ErbToYaml build | dotnet build tools/ErbToYaml/ErbToYaml.csproj | 0 warnings, 0 errors | Clean build |
| KojoComparer build | dotnet build tools/KojoComparer/KojoComparer.csproj | 0 warnings, 0 errors | Clean build |
| ErbParser tests | dotnet test tools/ErbParser.Tests | 89/89 pass | All passing |
| ErbToYaml tests | dotnet test tools/ErbToYaml.Tests | 86/86 pass | All passing |
| KojoComparer tests | dotnet test tools/KojoComparer.Tests | 89 pass, 2 fail, 3 skip = 94 total | Pre-existing failures (pilot equiv + file discovery) |
| LOCAL in kojo | Grep "LOCAL:|LOCAL\[" Game/ERB/口上 | 787 across 16 files | Runtime assignment pattern |
| EQUIP in kojo | Grep "EQUIP:|EQUIP\[" Game/ERB/口上 | 277 across 30+ files | Mostly WC系/SexHara (non-DATALIST) |
| STAIN in kojo | Grep "STAIN:|STAIN\[" Game/ERB/口上 | 33 across 13 files | Uses bitwise & operator |
| ITEM in kojo | Grep "ITEM:|ITEM\[" Game/ERB/口上 | 44 across 9 files | Mostly WC系 (non-DATALIST) |
| GETBIT in kojo | Grep "GETBIT" Game/ERB/口上 | 193 across 13 files | Nested CFLAG argument |
| Bitwise & in kojo | Grep "[^&]&[^&]" Game/ERB/口上 | 78 across 21 files | Tokenizer issue |
| Function calls in kojo | Grep "HAS_PENIS\|FIRSTTIME\|GETBIT\|HAS_VAGINA" Game/ERB/口上 | 412 across 30+ files | Runtime evaluation |
| ICondition types | Grep "JsonDerivedType" tools/ErbParser/ICondition.cs | 6 types (talent, cflag, tcvar, negated, function, logical) | Base for extension |
| ValidateConditionScope allowlist | KojoBranchesParser.cs | { TALENT, CFLAG, TCVAR, AND, OR, NOT } | Extend with new types |

**Baseline File**: `.tmp/baseline-756.txt`

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | No CSV files exist for EQUIP/STAIN/ITEM | Game/CSV/ directory (only Talent.csv) | ACs must NOT reference CSV loaders. Parsers use string-based name resolution. |
| C2 | STAIN always uses bitwise `&` operator | Grep analysis: `STAIN:奴隷:Ｖ & 汚れ_精液` | ACs for STAIN parsing must account for bitwise `&` or explicitly exclude it from scope |
| C3 | LOCAL is runtime assignment, not condition prefix | `LOCAL:1 = 0` then `IF LOCAL:1` pattern | ACs must NOT include LOCAL parsing - it requires fundamentally different architecture |
| C4 | EQUIP/STAIN/ITEM conditions mostly outside DATALIST | Grep shows WC系/SexHara context | ACs for DatalistConverter have limited real-world impact; parser/evaluator ACs have broader impact |
| C5 | ErbParser.Tests baseline is 89 tests | dotnet test output | New tests add to this baseline; regression = any of 89 failing |
| C6 | ErbToYaml.Tests baseline is 86 tests | dotnet test output | New tests add to this baseline |
| C7 | KojoComparer.Tests has 2 pre-existing failures | Pilot equiv + file discovery | Do not count as regressions |
| C8 | EQUIP follows CFLAG two-colon regex pattern; ITEM uses single-colon numeric index | EQUIP: `PREFIX:(?:([^:]+):)?([^:\s]+)(?:\s*...)`, ITEM: `ITEM:(\d+)(?:\s*...)` | EQUIP reuses CflagConditionParser regex. ITEM primarily uses `ITEM:N` (numeric index only, no target:name). Parser supports optional target for consistency but real data is numeric-only. |
| C9 | FunctionCall already parsed but not converted to YAML | ConvertConditionToYaml prints warning for FunctionCall case | AC can require FunctionCall→YAML passthrough (preserving function name/args) |
| C10 | Type should be engine, not infra | Tools are C# projects (ErbParser, ErbToYaml, KojoComparer) | ACs should follow engine type guidelines (positive+negative tests, ~300 line limit) |

### Constraint Details

**C1: No EQUIP/STAIN/ITEM CSV Loaders Needed**
- **Source**: `ls Game/CSV/` returns only `Talent.csv`, `_default.config`, `_fixed.config`
- **Verification**: `ls Game/CSV/` - no EQUIP/STAIN/ITEM CSV files
- **AC Impact**: Feature Background must be corrected. Parsers are simpler than originally estimated - no CSV loader dependency.

**C2: STAIN Uses Bitwise & Operator**
- **Source**: Grep `STAIN:.*&` in Game/ERB/口上: `STAIN:奴隷:Ｖ & 汚れ_精液` pattern
- **Verification**: All 33 STAIN occurrences in kojo use `&` not `==`/`!=`
- **AC Impact**: Either (a) exclude STAIN bitwise from F756 scope, or (b) implement bitwise operator parsing in SplitOnOperator. Option (a) is recommended for scope control.

**C3: LOCAL Cannot Be Statically Parsed as Condition**
- **Source**: Grep `LOCAL:` in kojo shows `LOCAL:1 = 0` (assignment) then `IF LOCAL:1` (check)
- **Verification**: LOCAL is ALWAYS used as runtime assignment in kojo EVENT files
- **AC Impact**: LOCAL must be excluded from F756 scope entirely. Requires fundamentally different architecture.

**C8: EQUIP/STAIN/ITEM Parser Pattern Reuse**
- **Source**: Grep analysis of actual ERB patterns: `EQUIP:MASTER:下半身上着１ != 0`, `STAIN:口`, `ITEM:2`
- **Verification**: Pattern matches CflagConditionParser regex with different prefix
- **AC Impact**: Consider extracting `VariableConditionParser<TRef>` base class to eliminate per-type parser duplication (CFLAG + TCVAR + EQUIP + STAIN + ITEM = 5 near-identical parsers)

**C9: FunctionCall YAML Passthrough**
- **Source**: DatalistConverter.cs line 238-240: `case FunctionCall function: Console.Error.WriteLine(...); return new Dictionary<string, object>();`
- **Verification**: Read DatalistConverter.cs ConvertConditionToYaml method
- **AC Impact**: Can implement opaque FunctionCall→YAML format (e.g., `{ "FUNCTION": { "name": "HAS_VAGINA", "args": ["TARGET"] } }`) without runtime evaluation

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Extend compound condition support to statically parseable ERB variable types" | Parsers for EQUIP, ITEM must exist and produce correct ICondition objects | AC#1, AC#2, AC#3, AC#4, AC#14, AC#15 |
| "extends to remaining statically parseable variable types" | New parsers registered in LogicalOperatorParser chain | AC#5 |
| "full equivalence testing" (serialization prerequisite) | New ICondition types registered with JsonDerivedType for polymorphic JSON serialization | AC#6 |
| "FunctionCall YAML passthrough" | FunctionCall conditions converted to YAML instead of empty {}; fail-fast throw for both compound and simple FUNCTION conditions | AC#7, AC#16, AC#19, AC#20 |
| "ICondition→YAML conversion for new types" | DatalistConverter handles EquipRef, ItemRef | AC#8, AC#21 |
| "ValidateConditionScope allowlist and EvaluateCondition dispatch" | KojoBranchesParser supports new variable types | AC#9, AC#17 |
| "Round-trip correctness for new condition types" | DatalistConverter YAML output consumable by KojoBranchesParser | AC#18 |
| "No regression" | All existing tests remain passing | AC#10, AC#11, AC#12 |
| "Zero technical debt" | No TODO/FIXME/HACK markers in new code | AC#13 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | EquipConditionParser.cs exists | file | Glob(tools/ErbParser/EquipConditionParser.cs) | exists | - | [x] |
| 2 | ItemConditionParser.cs exists | file | Glob(tools/ErbParser/ItemConditionParser.cs) | exists | - | [x] |
| 3 | EquipRef.cs exists | file | Glob(tools/ErbParser/EquipRef.cs) | exists | - | [x] |
| 4 | ItemRef.cs exists | file | Glob(tools/ErbParser/ItemRef.cs) | exists | - | [x] |
| 5 | New parsers registered in ParseAtomicCondition | code | Grep(tools/ErbParser/LogicalOperatorParser.cs) | matches | _equipParser\|_itemParser | [x] |
| 6 | JsonDerivedType registrations for new types | code | Grep(tools/ErbParser/ICondition.cs) | count_equals | 8 | [x] |
| 7 | FunctionCall YAML passthrough produces structured output (Pos) | code | Grep(tools/ErbToYaml/DatalistConverter.cs) | contains | "FUNCTION" | [x] |
| 8 | DatalistConverter handles new types (Pos) | code | Grep(tools/ErbToYaml/DatalistConverter.cs) | matches | case EquipRef\|case ItemRef | [x] |
| 9 | ValidateConditionScope allowlist includes new types | code | Grep(tools/KojoComparer/KojoBranchesParser.cs) | matches | "EQUIP".*"ITEM"\|"ITEM".*"EQUIP" | [x] |
| 17 | EvaluateCondition dispatches EQUIP/ITEM to EvaluateVariableCondition | code | Grep(tools/KojoComparer/KojoBranchesParser.cs) | matches | EvaluateVariableCondition.*EQUIP\|EvaluateVariableCondition.*ITEM | [x] |
| 18 | EQUIP/ITEM YAML evaluation test (contract verification) | test | dotnet test tools/KojoComparer.Tests --filter EquipItemYaml | succeeds | - | [x] |
| 10 | ErbParser tests pass (no regression) | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 11 | ErbToYaml tests pass (no regression) | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 12 | KojoComparer tests no new failures (no regression) | output | dotnet test tools/KojoComparer.Tests | not_matches | (Failed\|失敗):\\s+[3-9]\|(Failed\|失敗):\\s+\\d{2,} | [x] |
| 13 | Zero technical debt in new files | code | Grep(tools/ErbParser/EquipConditionParser.cs,tools/ErbParser/ItemConditionParser.cs,tools/ErbParser/EquipRef.cs,tools/ErbParser/ItemRef.cs) | not_matches | TODO\|FIXME\|HACK | [x] |
| 14 | EquipConditionParser behavioral test (Pos+Neg) | test | dotnet test tools/ErbParser.Tests --filter EquipConditionParser | succeeds | - | [x] |
| 15 | ItemConditionParser behavioral test (Pos+Neg) | test | dotnet test tools/ErbParser.Tests --filter ItemConditionParser | succeeds | - | [x] |
| 16 | FunctionCall YAML passthrough behavioral test | test | dotnet test tools/ErbToYaml.Tests --filter FunctionCall | succeeds | - | [x] |
| 19 | FunctionCall in compound condition throws InvalidOperationException (Neg) | test | dotnet test tools/KojoComparer.Tests --filter FunctionCallCompound | succeeds | - | [x] |
| 20 | Simple FunctionCall condition throws InvalidOperationException (Neg) | test | dotnet test tools/KojoComparer.Tests --filter FunctionCallSimple | succeeds | - | [x] |
| 21 | EQUIP/ITEM YAML conversion behavioral test | test | dotnet test tools/ErbToYaml.Tests --filter EquipItemConversion | succeeds | - | [x] |

### AC Details

**AC#1: EquipConditionParser.cs exists**
- New parser file following CflagConditionParser pattern
- Regex: `^EQUIP:(?:([^:]+):)?([^:\s]+)(?:\s*(!=|==|>=|<=|>|<)\s*(.+))?$` (C8: same as CFLAG with EQUIP prefix)
- No CSV loader needed (C1: string-based name resolution)
- Test: Glob pattern=tools/ErbParser/EquipConditionParser.cs
- Expected: File exists

**AC#2: ItemConditionParser.cs exists**
- New parser file following CflagConditionParser pattern
- Regex: `^ITEM:(?:([^:]+):)?([^:\s]+)(?:\s*(!=|==|>=|<=|>|<)\s*(.+))?$` (C8)
- ITEM uses numeric index primarily (`ITEM:2`) but pattern supports both name and index
- Test: Glob pattern=tools/ErbParser/ItemConditionParser.cs
- Expected: File exists

**AC#3: EquipRef.cs exists**
- ICondition implementation with Target, Name, Index, Operator, Value properties (same structure as CflagRef)
- JsonPropertyName attributes on all properties
- Test: Glob pattern=tools/ErbParser/EquipRef.cs
- Expected: File exists

**AC#4: ItemRef.cs exists**
- ICondition implementation with Target, Name, Index, Operator, Value properties
- Test: Glob pattern=tools/ErbParser/ItemRef.cs
- Expected: File exists

**AC#5: New parsers registered in ParseAtomicCondition**
- LogicalOperatorParser must declare `_equipParser`, `_itemParser` fields and call them in the ParseAtomicCondition chain
- Registration order: after TCVAR parser, before function call parser (most specific to least specific)
- Test: Grep pattern=`_equipParser|_itemParser` path=tools/ErbParser/LogicalOperatorParser.cs
- Expected: Both parser fields present (regex matches at least 2 lines: field declarations + parse calls)

**AC#6: JsonDerivedType registrations for new types**
- ICondition.cs must have 8 total JsonDerivedType attributes (6 existing + 2 new: equip, item)
- Existing: talent, cflag, tcvar, negated, function, logical
- New: equip, item
- Constraint C5 baseline: Currently 6 types
- Test: Grep pattern=`JsonDerivedType` path=tools/ErbParser/ICondition.cs | count
- Expected: 8 matches

**AC#7: FunctionCall YAML passthrough produces structured output (Pos)**
- ConvertConditionToYaml must convert FunctionCall to opaque YAML format instead of empty {}
- Format: `{ "FUNCTION": { "name": "...", "args": [...] } }` (C9)
- The warning log can be retained or removed; the key change is producing meaningful YAML output
- Negative: Current behavior (empty {} + warning) is replaced by structured passthrough
- Test: Grep pattern=`"FUNCTION"` in DatalistConverter.cs confirms the structured passthrough key exists (the current code does NOT contain "FUNCTION" string, so this verifies new behavior)
- Verification of correct behavior covered by AC#11 (ErbToYaml regression tests should include new passthrough tests)

**AC#8: DatalistConverter handles new types (Pos)**
- ConvertConditionToYaml switch must include case branches for EquipRef, ItemRef
- Each case calls a ConvertXxxRef method following ConvertCflagRef pattern
- YAML format: `{ "EQUIP": { "MASTER:下半身上着１": { "ne": "0" } } }` (same nesting as CFLAG)
- Test: Grep pattern=`case EquipRef|case ItemRef` path=tools/ErbToYaml/DatalistConverter.cs
- Expected: Both case branches present

**AC#9: ValidateConditionScope allowlist includes new types**
- KojoBranchesParser.ValidateConditionScope HashSet must include "EQUIP", "ITEM"
- Current allowlist: { "TALENT", "CFLAG", "TCVAR", "AND", "OR", "NOT" }
- New allowlist: { "TALENT", "CFLAG", "TCVAR", "EQUIP", "ITEM", "AND", "OR", "NOT" }
- Test: Grep pattern=`"EQUIP".*"ITEM"|"ITEM".*"EQUIP"` path=tools/KojoComparer/KojoBranchesParser.cs
- Expected: Both strings present in the allowlist (order-independent)

**AC#17: EvaluateCondition dispatches EQUIP/ITEM to EvaluateVariableCondition**
- EvaluateCondition must have explicit dispatch branches for EQUIP and ITEM types
- Without dispatch, conditions pass scope validation but silently return false (line 163 fallthrough)
- Pattern: `if (condition.TryGetValue("EQUIP", ...)) return EvaluateVariableCondition("EQUIP", ...)`
- Test: Grep pattern=`EvaluateVariableCondition.*EQUIP|EvaluateVariableCondition.*ITEM` path=tools/KojoComparer/KojoBranchesParser.cs
- Expected: Both EQUIP and ITEM dispatch calls present

**AC#18: EQUIP/ITEM YAML evaluation test (contract verification)**
- Verifies KojoBranchesParser.EvaluateCondition correctly evaluates EQUIP/ITEM conditions in the YAML format produced by DatalistConverter
- Uses manually constructed YAML matching DatalistConverter's documented output format (e.g., `{ "EQUIP": { "MASTER:下半身上着１": { "ne": "0" } } }`)
- Does NOT call DatalistConverter directly (KojoComparer.Tests has no ProjectReference to ErbToYaml)
- YAML format contract: `{ "EQUIP": { "TARGET:NAME": { "op": "val" } } }` and `{ "ITEM": { "INDEX": { "op": "val" } } }` — AC#21 is the format authority (tests actual DatalistConverter output)
- Tests both positive (condition matches state) and negative (condition does not match state) cases
- Test: dotnet test tools/KojoComparer.Tests --filter EquipItemYaml
- Expected: exit code 0 (tests exist and pass)

**AC#10: ErbParser tests pass (no regression)**
- Baseline: 89/89 pass (C5)
- New parser tests should be added, increasing total test count above 89
- All existing tests must continue to pass
- Test: dotnet test tools/ErbParser.Tests
- Expected: exit code 0

**AC#11: ErbToYaml tests pass (no regression)**
- Baseline: 86/86 pass (C6)
- New DatalistConverter tests for EQUIP/ITEM/FunctionCall conversion should be added
- All existing tests must continue to pass
- Test: dotnet test tools/ErbToYaml.Tests
- Expected: exit code 0

**AC#12: KojoComparer tests pass (no regression)**
- Baseline: 89 pass, 2 fail, 3 skip = 94 total (C7)
- Pre-existing failures (pilot equiv + file discovery) are NOT regressions
- New KojoBranchesParser tests for EQUIP/ITEM evaluation should be added
- Test: dotnet test tools/KojoComparer.Tests
- Expected: Failure count ≤ 2 (pre-existing). Test runner exits non-zero due to pre-existing failures; verify no NEW failures introduced by comparing against baseline (89 pass, 2 fail, 3 skip).

**AC#13: Zero technical debt in new files**
- All 4 new files (2 parsers + 2 refs) must have no TODO/FIXME/HACK markers
- DatalistConverter.cs and KojoBranchesParser.cs changes also checked (but existing markers are pre-existing)
- Test: Grep pattern=`TODO|FIXME|HACK` paths=tools/ErbParser/EquipConditionParser.cs, ItemConditionParser.cs, EquipRef.cs, ItemRef.cs
- Expected: 0 matches

**AC#14: EquipConditionParser behavioral test (Pos+Neg)**
- Unit tests verifying EquipConditionParser correctly parses EQUIP conditions
- Positive: `EQUIP:MASTER:下半身上着１ != 0` → EquipRef with Target=MASTER, Name=下半身上着１, Operator=!=, Value=0
- Negative: Invalid inputs return null (e.g., empty string, non-EQUIP prefix)
- Test: dotnet test tools/ErbParser.Tests --filter EquipConditionParser
- Expected: exit code 0 (tests exist and pass)

**AC#15: ItemConditionParser behavioral test (Pos+Neg)**
- Unit tests verifying ItemConditionParser correctly parses ITEM conditions
- Positive: `ITEM:2` → ItemRef with Name=2 (numeric index)
- Negative: Invalid inputs return null
- Test: dotnet test tools/ErbParser.Tests --filter ItemConditionParser
- Expected: exit code 0 (tests exist and pass)

**AC#16: FunctionCall YAML passthrough behavioral test**
- Unit tests verifying DatalistConverter converts FunctionCall to structured YAML
- Positive: FunctionCall("HAS_VAGINA", ["TARGET"]) → { "FUNCTION": { "name": "HAS_VAGINA", "args": ["TARGET"] } }
- Negative: Empty function name handling (if applicable)
- Test: dotnet test tools/ErbToYaml.Tests --filter FunctionCall
- Expected: exit code 0 (tests exist and pass)

**AC#19: FunctionCall in compound condition throws InvalidOperationException (Neg)**
- Negative test documenting intentional fail-fast boundary for FunctionCall conditions
- After F756, DatalistConverter produces `{ "FUNCTION": { ... } }` instead of empty `{}`
- In compound conditions (AND/OR/NOT), FUNCTION key triggers ValidateConditionScope throw
- This is intentional per Key Decision (Option B: keep InvalidOperationException)
- Test verifies the throw behavior is stable and documented
- Test: dotnet test tools/KojoComparer.Tests --filter FunctionCallCompound
- Expected: exit code 0 (test exists and passes, verifying throw behavior)

**AC#20: Simple FunctionCall condition throws InvalidOperationException (Neg)**
- Negative test for simple (non-compound, top-level) FUNCTION conditions
- An explicit `condition.TryGetValue("FUNCTION", ...)` check is added to EvaluateCondition
- Without this check, simple FUNCTION conditions silently return false via line 163 fallthrough (inconsistent with compound behavior)
- Test verifies fail-fast consistency: both simple and compound FUNCTION conditions throw
- Test: dotnet test tools/KojoComparer.Tests --filter FunctionCallSimple
- Expected: exit code 0 (test exists and passes, verifying throw behavior)

**AC#21: EQUIP/ITEM YAML conversion behavioral test**
- Unit tests verifying DatalistConverter correctly converts EquipRef and ItemRef to structured YAML
- Positive: EquipRef(Target=MASTER, Name=下半身上着１, Operator=!=, Value=0) → `{ "EQUIP": { "MASTER:下半身上着１": { "ne": "0" } } }`
- Positive: ItemRef(Name=2, Operator=!=, Value=0) → `{ "ITEM": { "2": { "ne": "0" } } }`
- Parallels AC#16 (FunctionCall behavioral test) for converter-side coverage
- Test: dotnet test tools/ErbToYaml.Tests --filter EquipItemConversion
- Expected: exit code 0 (tests exist and pass)

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|----------------|
| 1 | Create EQUIP/ITEM parsers following CflagConditionParser pattern | AC#1, AC#2, AC#3, AC#4, AC#14, AC#15 |
| 2 | Add EQUIP/ITEM to ValidateConditionScope allowlist and EvaluateCondition dispatch | AC#9, AC#17 |
| 3 | Implement ICondition→YAML conversion for new types in DatalistConverter | AC#8, AC#21 |
| 4 | Implement FunctionCall→YAML opaque passthrough format | AC#7, AC#16, AC#19, AC#20 |
| 5 | Register new parsers in LogicalOperatorParser.ParseAtomicCondition chain | AC#5, AC#6 |
| 6 | KojoBranchesParser evaluates EQUIP/ITEM YAML in converter format (contract test) | AC#18 |

All Goal items are covered.

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

**Pattern Replication Strategy**: F756 extends the F755 CFLAG/TCVAR parser pattern to two new variable types (EQUIP, ITEM) plus FunctionCall YAML passthrough. STAIN is fully deferred to F757 (all 33 occurrences use bitwise `&`, zero use comparison operators). The implementation follows these proven patterns:

1. **Parser Creation** (AC#1-2): Create `EquipConditionParser`, `ItemConditionParser` by cloning `CflagConditionParser` and changing only the prefix regex (`^EQUIP:`, `^ITEM:`)
2. **ICondition Types** (AC#3-4): Create `EquipRef`, `ItemRef` by cloning `CflagRef` structure (Target, Name, Index, Operator, Value properties)
3. **Parser Registration** (AC#5): Add parser instances to `LogicalOperatorParser._equipParser`, `_itemParser` fields and call them in `ParseAtomicCondition` chain after TCVAR, before FunctionCall
4. **JsonDerivedType Registration** (AC#6): Add two new `[JsonDerivedType]` attributes to `ICondition.cs` for equip/item discriminators
5. **YAML Conversion** (AC#7-8): Add `ConvertEquipRef`, `ConvertItemRef` methods to `DatalistConverter` by cloning `ConvertCflagRef` pattern; implement FunctionCall passthrough with opaque format
6. **Evaluation Support** (AC#9, AC#17): Add "EQUIP", "ITEM" to `ValidateConditionScope` allowlist; add `EvaluateVariableCondition` calls for these types in `EvaluateCondition`
7. **Integration Verification** (AC#18): Round-trip test verifying DatalistConverter YAML output is consumable by KojoBranchesParser

**Key Decision**: No base class extraction for this feature. While `VariableConditionParser<TRef>` would eliminate duplication across parser types, this refactoring is deferred to technical debt cleanup. F756 maintains the established per-type parser pattern for consistency with F755.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Clone CflagConditionParser.cs → EquipConditionParser.cs, change regex to `^EQUIP:(?:([^:]+):)?([^:\s]+)(?:\s*(!=\|==\|>=\|<=\|>\|<)\s*(.+))?$` |
| 2 | Clone CflagConditionParser.cs → ItemConditionParser.cs, change regex to `^ITEM:(?:([^:]+):)?([^:\s]+)(?:\s*(!=\|==\|>=\|<=\|>\|<)\s*(.+))?$` |
| 3 | Clone CflagRef.cs → EquipRef.cs, change class name and JsonDerivedType discriminator to "equip" |
| 4 | Clone CflagRef.cs → ItemRef.cs, change class name and JsonDerivedType discriminator to "item" |
| 5 | Add fields to LogicalOperatorParser: `private readonly EquipConditionParser _equipParser = new();` (+ item). Add parser calls in ParseAtomicCondition after TCVAR check, before function call check |
| 6 | Add 2 `[JsonDerivedType(typeof(EquipRef), typeDiscriminator: "equip")]` attributes to ICondition.cs (+ item) |
| 7 | Add `case FunctionCall function:` branch in ConvertConditionToYaml returning `{ "FUNCTION": { "name": function.Name, "args": function.Args } }` |
| 8 | Add `case EquipRef equip: return ConvertEquipRef(equip);` branches (+ item) in ConvertConditionToYaml. Clone ConvertCflagRef method 2 times with variable type name changes |
| 21 | Write EQUIP/ITEM YAML conversion behavioral tests (Pos: EquipRef → `{"EQUIP": {...}}`, ItemRef → `{"ITEM": {...}}`). Run `dotnet test --filter EquipItemConversion` |
| 9 | Change ValidateConditionScope allowedKeys HashSet to include "EQUIP", "ITEM" |
| 17 | Add `if (condition.TryGetValue("EQUIP", ...)) return EvaluateVariableCondition("EQUIP", ...)` branches (+ ITEM) in EvaluateCondition. Verified separately from allowlist to prevent silent false fallthrough. |
| 18 | Write contract test using manually constructed YAML matching DatalistConverter output format (e.g., `{ "EQUIP": { "MASTER:下半身上着１": { "ne": "0" } } }`). Evaluate via KojoBranchesParser.EvaluateCondition. KojoComparer.Tests has no ProjectReference to ErbToYaml — do NOT call DatalistConverter directly. |
| 10 | Run `dotnet test tools/ErbParser.Tests` - expect exit code 0 with test count > 89 (new tests added) |
| 11 | Run `dotnet test tools/ErbToYaml.Tests` - expect exit code 0 with test count > 86 (new tests added) |
| 12 | Run `dotnet test tools/KojoComparer.Tests` - expect exit code 0 (or same failure count as baseline if xUnit returns non-zero for expected failures) |
| 13 | Grep `TODO\|FIXME\|HACK` in all 4 new parser/ref files - expect 0 matches |
| 14 | Write EquipConditionParser unit tests (Pos: EQUIP:MASTER:下半身上着１ != 0, Neg: invalid prefix). Run `dotnet test --filter EquipConditionParser` |
| 15 | Write ItemConditionParser unit tests (Pos: ITEM:2, Neg: invalid prefix). Run `dotnet test --filter ItemConditionParser` |
| 16 | Write FunctionCall YAML passthrough unit test (Pos: FunctionCall → {"FUNCTION":{...}}). Run `dotnet test --filter FunctionCall` |
| 19 | Write negative test: FunctionCall inside compound condition (AND/OR) throws InvalidOperationException in KojoBranchesParser.EvaluateCondition. Documents intentional fail-fast boundary. Run `dotnet test --filter FunctionCallCompound` |
| 20 | Add explicit FUNCTION key check in EvaluateCondition (throws InvalidOperationException for simple FUNCTION conditions). Write negative test verifying throw. Run `dotnet test --filter FunctionCallSimple` |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Parser pattern reuse | (A) Clone CflagConditionParser per type, (B) Extract VariableConditionParser<TRef> base class, (C) Create unified VariableConditionParser with type parameter | A | Consistency with F755 pattern. Option B adds complexity without immediate ROI. Defer refactoring to tech debt cleanup. |
| STAIN scope | (A) Include STAIN parser for comparison operators, (B) Defer STAIN entirely to F757 | B | All 33 STAIN occurrences use bitwise `&`, zero use comparison operators. Parser would be dead code. F757 handles STAIN alongside bitwise tokenizer changes. |
| FunctionCall YAML format | (A) Empty dict {}, (B) Warning string { "error": "..." }, (C) Opaque passthrough { "FUNCTION": { "name": "...", "args": [...] } } | C | Preserves function signature for future runtime evaluation. Enables round-trip migration (ERB→YAML→ERB). Opaque format avoids false positive "equivalence" when functions cannot be statically resolved. |
| FunctionCall evaluation | (A) Stub return false, (B) Keep InvalidOperationException + add explicit simple FUNCTION throw, (C) Add to allowlist but no evaluation (fall through) | B | Adding FUNCTION to allowlist without evaluation converts loud failure into silent wrong result (returns false). Keeping InvalidOperationException for compound (via ValidateConditionScope) AND adding explicit `condition.TryGetValue("FUNCTION", ...)` throw in EvaluateCondition for simple conditions ensures consistent fail-fast behavior. Without the explicit check, simple FUNCTION conditions silently return false via line 163 fallthrough. |
| EQUIP/ITEM CSV loaders | (A) Create CSV loaders, (B) String-based name resolution | B | No EQUIP.csv/ITEM.csv files exist in Game/CSV/. These variables use string names or numeric indices directly. |
| JsonDerivedType registration order | (A) Append new types at end, (B) Insert alphabetically | A | Minimizes diff churn. JsonDerivedType order does not affect runtime behavior. |

### Interfaces / Data Structures

**New ICondition Types** (following CflagRef pattern):

```csharp
public class EquipRef : ICondition
{
    [JsonPropertyName("target")]
    public string? Target { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("index")]
    public int? Index { get; set; }

    [JsonPropertyName("operator")]
    public string? Operator { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

// ItemRef: identical structure, different type name
```

**FunctionCall YAML Format** (opaque passthrough):

```yaml
condition:
  FUNCTION:
    name: "HAS_VAGINA"
    args: ["TARGET"]
```

**Parser Registration Chain** (LogicalOperatorParser.ParseAtomicCondition):

```csharp
// Existing: TALENT → CFLAG → TCVAR → FunctionCall
// New order: TALENT → CFLAG → TCVAR → EQUIP → ITEM → FunctionCall
```

**ValidateConditionScope Allowlist** (KojoBranchesParser):

```csharp
// Before: { "TALENT", "CFLAG", "TCVAR", "AND", "OR", "NOT" }
// After:  { "TALENT", "CFLAG", "TCVAR", "EQUIP", "ITEM", "AND", "OR", "NOT" }
```

**Edge Cases**:

1. **Empty nameOrIndex**: Both parsers (CFLAG, new types) return null for empty `nameOrIndex` after regex match (line 48-51 of CflagConditionParser)
2. **ITEM numeric-only pattern**: ITEM primarily uses numeric indices (`ITEM:2`) but parser supports both name and index forms for consistency
3. **FunctionCall evaluation**: FunctionCall is NOT added to ValidateConditionScope allowlist. YAML passthrough preserves data in DatalistConverter. For evaluation: (a) In compound conditions (AND/OR/NOT), FUNCTION key triggers InvalidOperationException via ValidateConditionScope. (b) For simple (top-level) FUNCTION conditions, an explicit `condition.TryGetValue("FUNCTION", ...)` check is added to EvaluateCondition that throws InvalidOperationException for fail-fast consistency. Without this check, simple FUNCTION conditions would silently return false via line 163 fallthrough. AC#19 verifies compound throw; AC#20 verifies simple throw.

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 3,1,14 | Create EquipRef.cs and EquipConditionParser.cs with unit tests (Pos+Neg) | | [x] |
| 2 | 4,2,15 | Create ItemRef.cs and ItemConditionParser.cs with unit tests (Pos+Neg) | | [x] |
| 3 | 6 | Register new ICondition types in ICondition.cs JsonDerivedType attributes (equip, item) | | [x] |
| 4 | 5 | Register new parsers in LogicalOperatorParser.ParseAtomicCondition chain | | [x] |
| 5 | 8,21 | Add ConvertEquipRef/ConvertItemRef methods to DatalistConverter with behavioral test | | [x] |
| 6 | 7,16 | Implement FunctionCall YAML passthrough in DatalistConverter with unit test | | [x] |
| 7 | 9,17,19,20 | Add EQUIP/ITEM to ValidateConditionScope allowlist, EvaluateCondition dispatch, and FUNCTION throw in KojoBranchesParser with throw tests | | [x] |
| 12 | 18 | Write EQUIP/ITEM YAML evaluation contract test (hand-crafted YAML matching DatalistConverter output format, evaluated by KojoBranchesParser) | | [x] |
| 8 | 10,14,15 | Run ErbParser.Tests and verify no regression + behavioral tests pass | | [x] |
| 9 | 11,16 | Run ErbToYaml.Tests and verify no regression + FunctionCall passthrough test | | [x] |
| 10 | 12 | Run KojoComparer.Tests and verify no regression (same baseline failures only) | | [x] |
| 11 | 13 | Verify zero technical debt markers in all 4 new files | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Technical Design pattern replication specs (T1-T7, T12) | 4 new files + 4 modified files + integration test |
| 2 | ac-tester | haiku | Test commands from ACs (T8-T11) | Test results verification |

**Constraints** (from Technical Design):
1. No CSV loaders needed - EQUIP/ITEM use string-based name resolution (C1)
2. STAIN fully deferred to F757 - all occurrences use bitwise `&` (C2)
3. Parser pattern follows CflagConditionParser regex with prefix changes only (C8)
4. FunctionCall uses opaque passthrough format - no runtime evaluation (C9)
5. JsonDerivedType registration order: append new types at end to minimize diff churn
6. New parsers inserted in chain after TCVAR, before FunctionCall (most specific to least specific)

**Pre-conditions**:
- F755 [DONE] - provides all extension points (ValidateConditionScope, EvaluateVariableCondition, ConvertConditionToYaml patterns)
- Baseline test counts established: ErbParser.Tests=89, ErbToYaml.Tests=86, KojoComparer.Tests=90/94 (2 pre-existing failures)
- Clean build environment: `dotnet build` succeeds for all 3 tool projects

**Success Criteria**:
- All 21 ACs marked `[x]` PASS
- All test suites pass with increased test counts (no regression to baseline)
- All 4 new files (2 parsers + 2 ICondition types) exist and have zero debt markers
- 4 modified files (LogicalOperatorParser, ICondition, DatalistConverter, KojoBranchesParser) updated with new type support

**Rollback Plan**:

If issues arise after deployment:
1. Revert commit with `git revert`
2. Notify user of rollback with specific failure description
3. Create follow-up feature for fix with additional investigation into root cause

---

## Review Notes
- [resolved-applied] Phase2-Uncertain iter1: [AC-COV] Behavioral test ACs added (AC#14-16) to complement file-existence ACs (AC#1-4) and test-pass ACs (AC#10-12). Parser correctness now verified via dotnet test --filter.
- [resolved-applied] Phase2-Uncertain iter2: [EVAL-SAFE] FunctionCall in ValidateConditionScope converts loud failure (InvalidOperationException) into silent false. Key Decision updated to Option B (keep InvalidOperationException).
- [resolved-applied] Phase3-Maintainability iter7: [DEDUP] Code duplication across parser/converter/evaluator chain. Decision: Option A (clone per type) selected per Key Decisions table. Deduplication tracked in Mandatory Handoffs → F757.
- [resolved-applied] Phase2-Uncertain iter1: [AC-TYPE] AC#12 uses Type=output with Method=dotnet test. Intentional deviation: Type=test only supports succeeds/fails matchers, which cannot express baseline failure count check (pre-existing 2 failures cause non-zero exit). Type=output with not_matches is the correct approach for baseline-aware regression detection.
- [resolved-applied] Phase2-Uncertain iter2: [AC-ORDER] AC# numbering is non-sequential (1-9, 17, 18, 10-16). Accepted: template does not mandate sequential ordering. Renumbering 10+ cross-references creates high churn risk with no functional benefit. AC#s are stable identifiers, not position indicators.
- [resolved-applied] Phase2-Uncertain iter3: [FUNC-COMPOUND] FunctionCall compound condition behavioral change accepted as intentional fail-fast per Key Decision (Option B). Added AC#19 as negative test documenting that FUNCTION key in compound conditions throws InvalidOperationException. This converts the behavioral change from "undocumented regression" to "verified boundary."
- [resolved-applied] Phase2-Uncertain iter3: [FUNC-ROUNDTRIP] FunctionCall is deliberately one-way passthrough (converter only, evaluator throws). AC#16 covers converter-side behavior. AC#19 covers evaluator-side compound throw. AC#20 covers simple FUNCTION throw. Full round-trip is not applicable since evaluator intentionally rejects FUNCTION key by design.
- [resolved-applied] Phase2-Valid iter5: [AC18-XPROJ] AC#18 redesigned to avoid cross-project dependency. KojoComparer.Tests has no ProjectReference to ErbToYaml. Test now uses manually constructed YAML matching converter output format instead of calling DatalistConverter directly.
- [resolved-applied] Phase2-Valid iter5: [FUNC-SIMPLE] Simple FUNCTION conditions previously fell through to return false (line 163). Added explicit FUNCTION key check in EvaluateCondition that throws InvalidOperationException for fail-fast consistency. AC#20 verifies this behavior.
- [resolved-applied] Phase2-Uncertain iter5: [TALENT-HANDOFF] TALENT evaluation refactoring handoff to F757 accepted. Semantic scope mismatch tolerated: F757 Problem item #5 already includes this, and both items modify EvaluateCondition method. Splitting to separate feature adds overhead disproportionate to the change size.
- [resolved-applied] Phase2-Valid iter6: [TASK-AC] AC#19/20 moved from Task#6 (DatalistConverter) to Task#7 (KojoBranchesParser). AC#19/20 verify KojoBranchesParser throw behavior which is Task#7's implementation scope.
- [resolved-applied] Phase2-Valid iter6: [GOAL-6] Added Goal item 6 (round-trip correctness) to Goal section. Previously existed in Goal Coverage table but had no corresponding Goal definition.
- [resolved-applied] Phase2-Valid iter6: [PHILOSOPHY] Clarified FunctionCall passthrough in Philosophy: "data preservation only — evaluation deferred to F757". Prevents misleading implication that 412 function call occurrences achieve equivalence.
- [resolved-applied] Phase2-Valid iter6: [AC-DETAIL-FMT] AC Details format deviation accepted. All 20 AC entries contain Test and Expected information in list format. Reformatting to bold format would affect 20 entries with high churn and zero functional benefit. AC Definition Table provides the structured matcher data for automated verification.
- [resolved-applied] Phase2-Valid iter7: [GOAL-6-WORDING] Goal#6 reworded from "Round-trip correctness: DatalistConverter YAML output..." to "Verify KojoBranchesParser can evaluate EQUIP/ITEM YAML in the format produced by DatalistConverter (contract test)". Accurately reflects AC#18's hand-crafted YAML approach.
- [resolved-applied] Phase2-Valid iter7: [C8-ITEM] Constraint C8 updated to distinguish EQUIP (CFLAG two-colon pattern) from ITEM (single-colon numeric index). ITEM primarily uses `ITEM:N` format, not target:name.
- [resolved-applied] Phase2-Valid iter8: [TD-AC18] TD AC Coverage AC#18 updated to match AC Details: use hand-crafted YAML, no cross-project DatalistConverter call.
- [resolved-applied] Phase2-Uncertain iter8: [PHILO-EQUIV] Philosophy Derivation "full equivalence testing" → AC#6 clarified with "(serialization prerequisite)" annotation. JsonDerivedType enables serialization which is a prerequisite for equivalence testing, not direct equivalence verification.
- [resolved-applied] Phase3-Maintainability iter10: [FUNC-ARGS] TD AC#7 referenced `function.Arguments.ToArray()` but FunctionCall.cs uses `Args` property (string[]). Fixed to `function.Args`.
- [resolved-applied] Phase3-Maintainability iter10: [AC9-MATCHER] AC#9 matcher changed from `contains "EQUIP", "ITEM"` (fragile, assumes adjacent placement) to `matches "EQUIP".*"ITEM"|"ITEM".*"EQUIP"` (order-independent).
- [resolved-invalid] Phase2-Structural iter1: [TASK-ORDER] Task# numbering non-sequential (1-7, 12, 8-11). Template does not mandate sequential Task# ordering. Same rationale as accepted [AC-ORDER]. Renumbering requires updating Implementation Contract T-references with high churn risk.
- [resolved-invalid] Phase3-Maintainability iter2: [DEDUP-LOOP] Parser duplication (VariableConditionParser<TRef>) re-raised. Previously resolved in [DEDUP] iter7 as Option A. Key Decision documents rationale. Deduplication tracked in F757 Problem #6.
- [resolved-applied] Phase3-Maintainability iter2: [DEDUP-LEAK] F757 Problem section did not explicitly track parser/Ref/converter deduplication. Added item #6 to F757 Problem section for VariableConditionParser<TRef> extraction.
- [resolved-invalid] Phase3-Maintainability iter2: [TALENT-LOOP] TALENT evaluation duplication re-raised. Previously resolved in [TALENT-HANDOFF] iter5. F757 Problem #5 tracks this.
- [resolved-invalid] Phase3-Maintainability iter2: [OCP-SWITCH] DatalistConverter switch extensibility concern. Variant of [DEDUP] already resolved. Switch-based dispatch on closed JsonDerivedType hierarchy is idiomatic C#. Key Decision documents rationale.
- [resolved-applied] Phase2-Structural iter2: [REVIEW-NOTES-ORDER] Review Notes section moved before Mandatory Handoffs to match template section ordering.
- [resolved-applied] Phase2-Valid iter3: [AC21-CONVERTER-TEST] Added AC#21 for EQUIP/ITEM YAML conversion behavioral test in ErbToYaml.Tests. Parallels AC#16 (FunctionCall). AC#8 alone was structural (code existence via Grep), not behavioral. Goal#3 and Philosophy Derivation updated to include AC#21.
- [resolved-applied] Phase2-Valid iter4: [PHILO-EVALUATOR] Philosophy updated: FunctionCall description changed from "data preservation only" to include evaluator fail-fast boundary. Accurately reflects AC#19/20 InvalidOperationException throws.
- [resolved-applied] Phase2-Valid iter4: [F706-STATUS] F706 status corrected from [BLOCKED] to [PROPOSED] per index-features.md SSOT. Relationship changed from Successor/Consumer to Related (F706 does not list F756 as formal dependency).
- [resolved-applied] Phase2-Valid iter4: [AC18-FORMAT-CONTRACT] AC#18 Details updated with YAML format contract and AC#21 reference as format authority. Improves traceability between evaluator contract test and converter behavioral test.
- [resolved-applied] Phase3-Maintainability iter5: [F757-DEP-STATUS] F757 Dependencies table updated: F756 status from [DRAFT] to [PROPOSED].

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| STAIN parser + evaluator + converter | All 33 occurrences use bitwise `&`, requires tokenizer changes | Feature | F757 | - |
| FUNCTION key in ValidateConditionScope + evaluation | F756 creates FUNCTION YAML passthrough but evaluator throws InvalidOperationException for unknown keys | Feature | F757 | - |
| Refactor TALENT evaluation to use EvaluateVariableCondition | Code deduplication, not on critical path for F756 goals | Feature | F757 | - |

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK
- Missing Task for Option A → FL FAIL
-->

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-06 | creation | - | Created from F755 残課題 handoff | PROPOSED |
| 2026-02-06 | investigation | tech-investigator | Type corrected infra→engine. Occurrence counts corrected. CSV loader assumption corrected. Scope revision: split runtime constructs to separate feature. | COMPLETE |
| 2026-02-06 | ac-design | ac-designer | 16 ACs designed covering 5 Goals. Constraints C1-C10 respected. | COMPLETE |
| 2026-02-06 | technical-design | tech-designer | Pattern replication strategy. STAIN bitwise excluded. FunctionCall passthrough designed. | COMPLETE |
| 2026-02-06 | wbs | wbs-generator | 11 Tasks with AC:Task alignment. Implementation Contract defined. | COMPLETE |
| 2026-02-06 22:26 | tdd-red | implementer | Created 6 test files (EquipConditionParserTests, ItemConditionParserTests, FunctionCallConversionTests, EquipItemConversionTests, KojoBranchesParserEquipItemTests, KojoBranchesParserFunctionCallTests). Compilation fails as expected - EquipConditionParser/ItemConditionParser/EquipRef/ItemRef types do not exist yet. | COMPLETE |

---

## Links
- [feature-755.md](feature-755.md) - CFLAG/TCVAR compound support (predecessor)
- [feature-752.md](feature-752.md) - TALENT compound support (indirect predecessor)
- [feature-706.md](feature-706.md) - Full equivalence verification (consumer)
- [feature-750.md](feature-750.md) - YAML TALENT condition migration (foundation)
- [feature-754.md](feature-754.md) - YAML format unification (related)
- [feature-757.md](feature-757.md) - Bitwise & operator support (successor, deferred from F756)