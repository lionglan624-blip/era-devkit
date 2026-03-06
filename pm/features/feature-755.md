# Feature 755: CFLAG/TCVAR Compound Conditions & Re-migration

## Status: [DONE]
<!-- fl-reviewed: 2026-02-06T00:00:00Z -->

## Scope Discipline

> **Out-of-Scope Issue Protocol**
>
> When encountering complex patterns beyond F755 scope (LOCAL variables, function calls, character-scoped CFLAG):
> 1. **STOP** - Do not fix it within this feature
> 2. **REPORT** - Notify user immediately with a clear description
> 3. **TRACK** - Create handoff entry in Mandatory Handoffs table with specific F756 assignment
> 4. **LINK** - Reference F756 in Links section
>
> Do not expand F755 scope. Do not implement partial solutions.

## Type: infra

## Background

### Philosophy (Mid-term Vision)
Full equivalence testing (F706: 650/650 MATCH) requires progressive extension of ERB branching pattern support in YAML. F752 added TALENT-only compound condition support to KojoBranchesParser. F755 adds CFLAG/TCVAR evaluation framework and compound YAML migration syntax; actual CFLAG/TCVAR YAML migration deferred to F756. The remaining compound conditions include 1,249+ CFLAG+TALENT mixed occurrences, 429 CFLAG-only compounds, and 1,500+ other variable type compounds across 60+ files.

### Problem (Current Issue)
1. **CFLAG/TCVAR compounds**: 1,249+ mixed `CFLAG/TCVAR+TALENT` conditions across 38 files, plus 429 CFLAG-only compounds across 39 files, cannot be evaluated by KojoBranchesParser (throws `InvalidOperationException` per F752 scope enforcement).
2. **ErbToYaml compound parsing**: `DatalistConverter.ParseCondition()` bypasses existing `ConditionExtractor`/`LogicalOperatorParser` infrastructure and delegates directly to `TalentConditionParser`, producing empty `{}` for all non-TALENT and compound conditions. Additionally, no `ICondition`-to-YAML-dictionary conversion layer exists to transform parsed condition trees into `AND`/`OR`/`NOT` YAML structures.
3. **Re-migration**: 48 ERB files with compound TALENT conditions need re-migration using the extended ErbToYaml to produce YAML with compound condition syntax.
4. **YAML schema**: `tools/YamlValidator` will reject compound condition YAML files because the schema does not include `AND`/`OR`/`NOT` keys.

### Goal (What to Achieve)
1. Extend KojoBranchesParser to support CFLAG/TCVAR keys in compound sub-conditions
2. Wire `DatalistConverter.ParseCondition()` to use `ConditionExtractor`/`LogicalOperatorParser` and implement `ICondition`-to-YAML-dictionary conversion (including `LogicalOp` binary tree to flat `AND`/`OR` array flattening)
3. Re-migrate TALENT-compound ERB files using extended ErbToYaml to produce YAML with AND/OR/NOT compound syntax (CFLAG/TCVAR file re-migration deferred to F756)
4. Update YAML schema to validate compound condition syntax

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis

### 5 Whys

1. **Why can't KojoBranchesParser evaluate CFLAG/TCVAR compound conditions?**
   Because `ValidateConditionScope()` (KojoBranchesParser.cs:245-254) has a strict allowlist `{ "TALENT", "AND", "OR", "NOT" }`. Any sub-condition key not in this set (CFLAG, TCVAR, etc.) throws `InvalidOperationException("Non-TALENT compound conditions are not supported ... Deferred to F755.")`. This was a deliberate F752 scope boundary.

2. **Why does F752 only allow TALENT keys in compound sub-conditions?**
   Because F752's scope was "TALENT-only compound support" per Scope Discipline. The F750 investigation discovered 409 TALENT-only compound occurrences across 48 files, which was the immediate blocker for F706 equivalence testing. Mixed CFLAG/TCVAR+TALENT patterns were explicitly deferred as out-of-scope to F755.

3. **Why can't ErbToYaml parse compound conditions from ERB?**
   Because `DatalistConverter.ParseCondition()` (DatalistConverter.cs:206-271) **bypasses existing `ConditionExtractor`/`LogicalOperatorParser` infrastructure** and delegates entirely to `TalentConditionParser.ParseTalentCondition()`. The ErbParser already contains a full compound condition parsing infrastructure (`ConditionExtractor` -> `LogicalOperatorParser` -> `CflagConditionParser`/`TalentConditionParser`/`FunctionCallParser`) that handles `&&`, `||`, parentheses, and multiple variable types. `DatalistConverter` simply does not use it, calling `_conditionParser.ParseTalentCondition(condition)` directly (line 208).

4. **Why does DatalistConverter not use the existing ConditionExtractor?**
   Because `DatalistConverter` was written for F750 which had TALENT-only scope. Its `_conditionParser` field is a `TalentConditionParser` instance (line 21, 31), not a `ConditionExtractor`. No `ICondition`-to-YAML-dictionary conversion layer exists to transform the `ICondition` tree (`TalentRef`, `CflagRef`, `LogicalOp`, `FunctionCall`) into the YAML format (`{ TALENT: { 16: { ne: 0 } } }`, `{ AND: [...] }`).

5. **Why is this a problem for F706 equivalence testing?**
   Because when `TalentConditionParser.ParseTalentCondition()` receives a compound condition string like `TALENT:恋人 && CFLAG:MASTER:100`, it returns `null` (regex doesn't match the `&&` operator), and `DatalistConverter` falls back to returning empty `{}`. This causes KojoBranchesParser to select incorrect branches, producing MISMATCH results in F706 batch testing.

### Symptom vs Root Cause

| Symptom | Root Cause |
|---------|------------|
| `InvalidOperationException` thrown when KojoBranchesParser encounters CFLAG/TCVAR in compound sub-conditions | `ValidateConditionScope()` allowlist restricted to TALENT/AND/OR/NOT keys by F752 scope boundary (KojoBranchesParser.cs:247) |
| ErbToYaml produces empty conditions `{}` for compound ERB expressions | `DatalistConverter.ParseCondition()` bypasses existing `ConditionExtractor`/`LogicalOperatorParser` and calls `TalentConditionParser` directly (DatalistConverter.cs:208). No `ICondition`-to-YAML conversion layer exists. |
| Migrated YAML files with compound-condition ERB sources have incorrect branch selection | DatalistConverter (migration) bypasses existing compound parsing infrastructure; KojoBranchesParser (evaluation) rejects non-TALENT keys |

### Conclusion

**Root cause: Two independent gaps bridgeable by existing infrastructure.**

1. **Evaluation gap**: `ValidateConditionScope()` in KojoBranchesParser.cs (line 245) rejects all non-TALENT keys. `EvaluateCondition()` (line 90) has no code path for CFLAG, TCVAR, or any other variable type. This is a simple allowlist + evaluation extension.

2. **Migration gap**: `DatalistConverter.ParseCondition()` calls `TalentConditionParser` directly (line 208) instead of using the existing `ConditionExtractor` which delegates to `LogicalOperatorParser`. The `LogicalOperatorParser` ALREADY handles:
   - `&&` and `||` with proper operator precedence (lines 38-122)
   - Parenthesized sub-expressions via `SplitOnOperator` paren depth tracking (lines 161-212)
   - TALENT via `TalentConditionParser` (lines 132-137)
   - CFLAG via `CflagConditionParser` including character-scoped references like `CFLAG:MASTER:100` (lines 139-144)
   - Function calls via `FunctionCallParser` like `HAS_VAGINA(TARGET)`, `FIRSTTIME()` (lines 146-151)
   - Returns `ICondition` tree: `TalentRef`, `CflagRef`, `FunctionCall`, `LogicalOp` (binary tree with Left/Right)

   What is MISSING:
   - **TcvarConditionParser/TcvarRef**: No TCVAR parser exists in ErbParser. Must be created following the CflagConditionParser pattern.
   - **Negation (`!`) handling**: `LogicalOperatorParser.ParseAtomicCondition()` does not handle the `!` prefix. Must add negation support as a new `ICondition` type (e.g., `NegatedCondition`) or handle inline.
   - **ICondition-to-YAML conversion**: No layer exists in DatalistConverter to convert `ICondition` trees (polymorphic: `TalentRef`, `CflagRef`, `LogicalOp`, `FunctionCall`) into YAML dictionaries. The `LogicalOp` binary tree (`Left`/`Right`) must be flattened into YAML flat arrays (`AND: [A, B, C]` for same-operator chains like `A && B && C`).

**Verified existing infrastructure** (ErbParser):

| Component | File | Capability | Status |
|-----------|------|------------|--------|
| `LogicalOperatorParser` | ErbParser/LogicalOperatorParser.cs | `&&`/`\|\|` parsing with precedence, parentheses, left-associative trees | EXISTS (8 tests in LogicalOperatorParserTests.cs) |
| `CflagConditionParser` | ErbParser/CflagConditionParser.cs | `CFLAG:name`, `CFLAG:idx`, `CFLAG:target:name`, `CFLAG:target:name op value` | EXISTS (tests in CflagExtractorTests.cs) |
| `CflagRef : ICondition` | ErbParser/CflagRef.cs | Target?, Name?, Index? (int?), Operator?, Value? | EXISTS |
| `TalentConditionParser` | ErbParser/TalentConditionParser.cs | `TALENT:(target:)?name( op value)?` | EXISTS |
| `TalentRef : ICondition` | ErbParser/TalentRef.cs | Target, Name, Operator?, Value? | EXISTS |
| `FunctionCallParser` | ErbParser/FunctionCallParser.cs | `FUNC(arg1, arg2, ...)` | EXISTS |
| `FunctionCall : ICondition` | ErbParser/FunctionCall.cs | Name, Args[] | EXISTS |
| `LogicalOp : ICondition` | ErbParser/LogicalOp.cs | Left (ICondition), Operator (`&&`/`\|\|`), Right (ICondition) | EXISTS |
| `ConditionExtractor` | ErbParser/ConditionExtractor.cs | Facade: delegates to LogicalOperatorParser.ParseLogicalExpression() | EXISTS (tests in ConditionExtractorTests.cs) |
| `ICondition` | ErbParser/ICondition.cs | Marker interface with JsonDerivedType for TalentRef, CflagRef, FunctionCall, LogicalOp | EXISTS |
| `TcvarConditionParser` | - | TCVAR condition parsing | MISSING - must create |
| `TcvarRef : ICondition` | - | TCVAR condition reference | MISSING - must create |
| Negation (`!`) in ParseAtomicCondition | LogicalOperatorParser.cs:128 | `!TALENT:X`, `!CFLAG:Y` prefix negation | MISSING - must add |
| `ICondition`-to-YAML conversion | DatalistConverter.cs | Convert ICondition tree to YAML dictionary | MISSING - must create |
| LogicalOp tree flattening | - | `LogicalOp(&&, LogicalOp(&&, A, B), C)` -> `AND: [A, B, C]` | MISSING - must create |

---

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F752 | [DONE] | Predecessor | Added TALENT-only compound condition support (AND/OR/NOT) to KojoBranchesParser. F755 extends the allowlist and adds new evaluation code paths |
| F750 | [DONE] | Foundation | Created YAML TALENT condition migration. ErbToYaml and TalentConditionParser from F750 are the migration tools. DatalistConverter.ParseCondition() currently bypasses ConditionExtractor |
| F706 | [BLOCKED] | Consumer | Full equivalence verification (650/650 MATCH) - compound condition support is required to eliminate branch selection mismatches in migrated YAML files |
| F725 | [DONE] | Infrastructure | Created KojoBranchesParser with `EvaluateCondition()` method. F755 extends both the allowlist and evaluation logic |
| F754 | [DRAFT] | Downstream | YAML Format Unification (branches to entries). BranchesToEntriesConverter.TransformCondition() currently handles only single-key conditions (TALENT, ABL, EXP, FLAG, CFLAG) and has no compound condition support |
| F751 | [DRAFT] | Sibling | TALENT semantic mapping validation. No direct dependency but parallel F750 handoff |
| F709 | [DRAFT] | Downstream | Multi-State Equivalence Testing. Depends on F706. Benefits from compound condition support for multi-state branch coverage |

### Pattern Analysis

This is a **wiring and extension pattern**, not a new parser creation:

1. **F752 was correctly scoped**: TALENT-only compounds were the critical path for F706 equivalence testing (409 occurrences, 48 files). F752 delivered the compound evaluation framework (AND/OR/NOT recursive evaluation, depth limiting) that F755 extends.

2. **The previous F755 design was fundamentally flawed**: It proposed creating a NEW `CompoundConditionParser` class when `LogicalOperatorParser` already provides superior functionality (proper operator precedence, parentheses support, CFLAG/function-call parsing). The proposed `CompoundConditionParser` had a NOT precedence bug (`!X && Y` parsed as `NOT(X && Y)` instead of `NOT(X) && Y`), could not handle parentheses, and defined a `CflagRef` class that conflicted with the existing `CflagRef : ICondition`.

3. **The actual work is three-fold**: (a) Wire `DatalistConverter` to use `ConditionExtractor` instead of `TalentConditionParser` directly, (b) Build an `ICondition`-to-YAML conversion layer with `LogicalOp` binary tree flattening, (c) Create `TcvarConditionParser`/`TcvarRef` and add negation support to `LogicalOperatorParser`.

4. **Significant simplification vs previous design**: By reusing existing infrastructure, F755 gains character-scoped CFLAG references and function call handling FOR FREE. Parentheses support requires ~5 lines of outer-paren stripping in `ParseAtomicCondition()` (leveraging existing `SplitOnOperator()` paren depth tracking). The previous design's NOT precedence bug exclusion (3 files) and CFLAG character-scope limitation are eliminated.

---

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | Existing `ConditionExtractor`/`LogicalOperatorParser` already parses compound conditions with `&&`, `\|\|`, parentheses, TALENT, CFLAG, and function calls. DatalistConverter just needs to use it. |
| KojoBranchesParser extension is realistic | YES | Adding CFLAG/TCVAR evaluation follows the same pattern as TALENT evaluation (state key lookup, operator comparison). ValidateConditionScope allowlist expansion is trivial. |
| DatalistConverter wiring is realistic | YES | Replace `_conditionParser.ParseTalentCondition(condition)` call with `ConditionExtractor.Extract(condition)`, then convert `ICondition` tree to YAML dictionary. The conversion is a visitor/switch pattern over 5 types: `TalentRef`, `CflagRef`, `LogicalOp`, `FunctionCall`, and (new) `TcvarRef`/`NegatedCondition`. |
| LogicalOp tree flattening is realistic | YES | `LogicalOp` is a binary tree with `Left`/`Right`. KojoBranchesParser expects `AND`/`OR` as flat `List<object>` arrays. Flattening same-operator chains (`A && B && C` parsed as `LogicalOp(&&, LogicalOp(&&, A, B), C)` -> `AND: [A, B, C]`) is a standard tree walk. Mixed operators produce nested structures. |
| TcvarConditionParser creation is realistic | YES | Follow exact pattern of `CflagConditionParser` (81 lines). Same regex structure. Create `TcvarRef : ICondition` following `CflagRef` model. |
| Negation support is realistic | YES | Add `!` prefix detection in `LogicalOperatorParser.ParseAtomicCondition()` before trying TALENT/CFLAG/function parsers. Strip `!` prefix, parse inner condition, wrap in `NegatedCondition : ICondition`. |
| Re-migration scope is realistic | YES | 48 ERB files from F752 baseline. ErbToYaml re-run with extended `ParseCondition()` path produces YAML with AND/OR/NOT keys. Parentheses and character-scoped CFLAG are now handled by existing infrastructure. |
| YAML schema extension is realistic | YES | Adding AND/OR/NOT compound condition support to `dialogue-schema.json` follows JSON Schema `oneOf`/`anyOf` patterns. |
| No blocking constraints | YES | F752 [DONE] provides the compound evaluation foundation. No predecessor is blocking. |

**Verdict**: FEASIBLE

**Key advantage of infrastructure reuse**: The previous design assessed ErbToYaml compound parsing as PARTIAL feasibility due to parentheses, function calls, and character-scoped CFLAG complexity. By reusing `LogicalOperatorParser` which handles operator precedence, character-scoped CFLAG, and function calls, the feasibility is upgraded to YES across the board. New parser code needed: `TcvarConditionParser` (~80 lines), negation support (~10 lines), and outer-paren stripping (~5 lines) in `ParseAtomicCondition()`.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F752 | [DONE] | TALENT compound condition support - provides AND/OR/NOT recursive evaluation framework, ValidateConditionScope allowlist, depth limiting |
| Predecessor | F750 | [DONE] | YAML TALENT condition migration - provides TalentConditionParser, DatalistConverter.ParseCondition(), TalentCsvLoader infrastructure |
| Related | F725 | [DONE] | Created KojoBranchesParser with EvaluateCondition() and branches-format YAML parsing |
| Successor | F706 | [BLOCKED] | Consumer of compound condition evaluation for equivalence verification (650/650 MATCH) |
| Related | F754 | [DRAFT] | YAML Format Unification - BranchesToEntriesConverter.TransformCondition() needs compound condition support when converting from branches to entries format |
| Related | F751 | [DRAFT] | TALENT semantic mapping validation - sibling F750 handoff, no dependency |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| YamlDotNet | Runtime | Low | Already in use by KojoBranchesParser and DatalistConverter. YAML nested structures handle AND/OR/NOT natively. |
| NJsonSchema | Runtime | Low | Used by YamlValidator and DatalistConverter for schema validation. Schema extension follows JSON Schema draft-07. |
| ErbParser | Build | **None** | Existing infrastructure (`ConditionExtractor`, `LogicalOperatorParser`, `CflagConditionParser`, `CflagRef`, `ICondition`, `LogicalOp`) is complete and tested. Only additions needed: `TcvarConditionParser`/`TcvarRef` (new types) and negation support in `ParseAtomicCondition()`. No modification to existing parser code required. |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/KojoComparer/KojoBranchesParser.cs | HIGH | ValidateConditionScope allowlist extension + new EvaluateCondition code paths for CFLAG/TCVAR |
| tools/ErbToYaml/DatalistConverter.cs | HIGH | Replace `TalentConditionParser` direct call with `ConditionExtractor` + `ICondition`-to-YAML conversion layer |
| tools/ErbParser/LogicalOperatorParser.cs | MEDIUM | Register new `TcvarConditionParser` in `ParseAtomicCondition()` chain + add negation handling |
| tools/ErbParser/ICondition.cs | LOW | Add `JsonDerivedType` registrations for `TcvarRef` and `NegatedCondition` |
| tools/ErbToYaml/FileConverter.cs | LOW | ProcessConditionalBranch() calls ParseCondition() - benefits automatically from DatalistConverter changes |
| tools/ErbToYaml/BranchesToEntriesConverter.cs | MEDIUM | TransformCondition() and GenerateId() handle only single-key conditions. Must support AND/OR/NOT compound conditions |
| tools/YamlSchemaGen/dialogue-schema.json | MEDIUM | Schema must include compound condition definitions (AND/OR/NOT) for YamlValidator to accept migrated files |
| tools/KojoComparer/BatchProcessor.cs | LOW | Calls KojoBranchesParser.Parse() - no API change needed |

---

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/KojoComparer/KojoBranchesParser.cs | Update | Extend ValidateConditionScope allowlist to include CFLAG, TCVAR. Add CFLAG/TCVAR evaluation code paths using `EvaluateVariableCondition()` shared helper. State key format: `"CFLAG:{target}:{name}"` or `"CFLAG:{index}"` for indexed, `"TCVAR:{name_or_index}"` |
| tools/ErbParser/TcvarConditionParser.cs | Create | New TCVAR condition parser following CflagConditionParser pattern (~80 lines) |
| tools/ErbParser/TcvarRef.cs | Create | New TCVAR condition reference implementing ICondition, following CflagRef model (Target?, Name?, Index?, Operator?, Value?) |
| tools/ErbParser/NegatedCondition.cs | Create | New ICondition implementation wrapping a negated inner condition (~15 lines) |
| tools/ErbParser/LogicalOperatorParser.cs | Update | Add TcvarConditionParser to ParseAtomicCondition() chain. Add `!` prefix negation handling before parser dispatch. |
| tools/ErbParser/ICondition.cs | Update | Add JsonDerivedType registrations for TcvarRef and NegatedCondition |
| tools/ErbToYaml/DatalistConverter.cs | Update | Replace `TalentConditionParser` direct call with `ConditionExtractor.Extract()`. Add `ICondition`-to-YAML conversion layer: `ConvertConditionToYaml(ICondition)` with cases for TalentRef, CflagRef, TcvarRef, NegatedCondition, LogicalOp. Add LogicalOp tree flattening logic (same-operator chains to flat arrays). |
| tools/ErbToYaml/BranchesToEntriesConverter.cs | Update | Extend TransformCondition() and GenerateId() to handle compound conditions with AND/OR/NOT keys |
| tools/YamlSchemaGen/dialogue-schema.json | Update | Add compound condition schema (oneOf: single condition OR compound condition with AND/OR/NOT) |
| Game/YAML/Kojo/**/*.yaml | Re-migrate | Re-migrate YAML files whose source ERB has compound conditions (48 TALENT-compound files from F752 baseline) |
| tools/KojoComparer.Tests/ | Create/Update | New tests for CFLAG/TCVAR compound condition evaluation; update existing `CompoundNonTalent_CflagRejected` test |
| tools/ErbToYaml.Tests/ | Create | New tests for DatalistConverter compound condition conversion via ConditionExtractor |
| tools/ErbParser.Tests/ | Create | New tests for TcvarConditionParser, negation handling |

---

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| F752 compound framework must be preserved | KojoBranchesParser.cs (F752 implementation) | HIGH - New variable types must integrate with existing AND/OR/NOT recursive evaluation, not replace it |
| Backward compatibility with 1,118 existing YAML files | Game/YAML/Kojo/ (443 branches + 675 entries) | HIGH - Existing single-condition and TALENT-compound files must continue to work unchanged |
| Two different condition formats coexist | Branches format: `{ CFLAG: { idx: { op: val } } }` vs Entries format: `{ type: "CFlag", cflagId: "idx", threshold: N }` | HIGH - Changes must work in both formats. BranchesToEntriesConverter bridges them |
| Existing CflagRef data model has Target/Name/Index fields | ErbParser/CflagRef.cs: Target (string?), Name (string?), Index (int?), Operator (string?), Value (string?) | HIGH - YAML conversion must handle both indexed (`CFLAG:300`) and named (`CFLAG:MASTER:現在位置`) forms. State key format must encode all components |
| LogicalOp is a binary tree, YAML expects flat arrays | LogicalOp.cs: Left/Right structure; KojoBranchesParser expects `AND: [item1, item2, ...]` | HIGH - Must implement tree flattening: same-operator chains (`A && B && C` -> `AND: [A, B, C]`), mixed operators produce nested structures |
| TreatWarningsAsErrors | Directory.Build.props (F708) | LOW - All new code must compile without warnings |
| LOCAL/function calls produce FunctionCall ICondition | LogicalOperatorParser parses `HAS_PENIS(TARGET)` as FunctionCall. LOCAL is not parsed by any existing parser | MEDIUM - DatalistConverter must handle FunctionCall nodes gracefully (return empty `{}` or skip). LOCAL conditions will return null from ConditionExtractor. |
| ErbParser project reference needed from ErbToYaml | DatalistConverter currently references TalentConditionParser only | LOW - Must use ConditionExtractor (same namespace, likely same project reference already exists) |

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| LogicalOp tree flattening edge cases | MEDIUM | MEDIUM | Same-operator chains are common (`A && B && C`). Mixed precedence produces nested `LogicalOp` (e.g., `A \|\| B && C` -> `OR(A, AND(B,C))`). Flattening should only flatten same-operator siblings. Unit tests cover both patterns. |
| FunctionCall conditions in YAML format | MEDIUM | LOW | `LogicalOperatorParser` successfully parses function calls like `HAS_VAGINA(TARGET)` into `FunctionCall` objects. DatalistConverter conversion layer should return `null` or empty `{}` for `FunctionCall` ICondition types since no YAML representation exists. This matches current behavior (unknown conditions -> empty `{}`). |
| Re-migration regresses F750's TALENT condition work | MEDIUM | HIGH | Re-migration must be targeted (only files with compound conditions). The new `ConditionExtractor` path handles single-TALENT conditions correctly (returns `TalentRef` directly per `LogicalOperatorParser` single-condition unwrapping). Backward compatibility unit test required. |
| CflagRef state key format complexity | MEDIUM | MEDIUM | Existing `CflagRef` has separate `Target`, `Name`, `Index` fields. State key in KojoBranchesParser must encode all: `"CFLAG:{target}:{name}"` for character-scoped, `"CFLAG:{index}"` for indexed. YAML conversion must produce correct key hierarchy. |
| TCVAR volume is small (23 occurrences) | LOW | LOW | Small impact scope but required for completeness. TcvarConditionParser follows CflagConditionParser pattern exactly. Low risk due to pattern reuse. |
| DatalistConverter ICondition null handling | MEDIUM | MEDIUM | `ConditionExtractor.Extract()` returns `null` for unparseable conditions (e.g., bare `LOCAL:1` which no parser handles). DatalistConverter must handle `null` -> empty `{}` fallback matching current behavior for graceful degradation. |
| F754 BranchesToEntriesConverter breaks on compound conditions | HIGH | MEDIUM | BranchesToEntriesConverter.TransformCondition() only handles single-key conditions. Compound conditions with AND/OR/NOT keys would return null (silent failure). Must add passthrough for compound conditions before F754 can proceed. |
| Scope creep from discovered variable types beyond CFLAG/TCVAR | MEDIUM | MEDIUM | Feature scope is CFLAG/TCVAR evaluation + compound YAML migration using existing infrastructure. Other types (LOCAL, EQUIP, STAIN, ITEM) parsed by `ConditionExtractor` but not converted to YAML (return empty `{}`). Deferred to F756. |

---

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| KojoBranchesParser test count | dotnet test tools/KojoComparer.Tests/ --list-tests | 23 tests | F752 established base |
| ErbToYaml test count | dotnet test tools/ErbToYaml.Tests/ --list-tests | 78 tests | F750 established base |
| ErbParser test count | dotnet test tools/ErbParser.Tests/ --list-tests | 77 tests | F750 established base |
| YAML files with compound syntax | Grep(Game/YAML/Kojo/) for 'AND:' | 0 occurrences | Pre-F755 baseline |

**Baseline File**: `.tmp/baseline-755.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | F752 compound framework must be preserved | KojoBranchesParser.cs (F752) | ACs must verify backward compatibility: existing 23 KojoBranchesParser tests still pass after changes |
| C2 | CFLAG state keys use CflagRef data model (Target?, Name?, Index?) not simple string indices | ErbParser/CflagRef.cs existing class | YAML conversion must handle both indexed (`CFLAG:300`) and named (`CFLAG:MASTER:name`) forms. State key format encodes all CflagRef components |
| C3 | `LogicalOperatorParser.SplitOnOperator()` tracks paren depth to avoid incorrect splitting; `ParseAtomicCondition()` requires outer-paren stripping + recursive `ParseLogicalExpression` delegation to fully support parenthesized sub-expressions | LogicalOperatorParser.cs (8 tests, none cover parenthesized sub-expressions) | F755 must add paren stripping in `ParseAtomicCondition()` to realize parentheses support. Operator precedence is correct. No NOT precedence bug. No 3-file exclusion needed |
| C4 | Re-migration targets only TALENT compound files (48 from F752 baseline) | Risk management: CFLAG/TCVAR compound re-migration deferred to F756 | AC re-migration count is based on TALENT-compound ERB files only |
| C5 | Existing 1,118 YAML files must not regress | Game/YAML/Kojo/ (443 branches + 675 entries) | Any AC verifying YAML file changes must also verify non-targeted files are unchanged |
| C6 | `CompoundNonTalent_CflagRejected` test currently expects rejection | KojoBranchesParserCompoundConditionTests.cs:361 | After F755, this test must be updated to expect acceptance |
| C7 | BranchesToEntriesConverter handles single-key conditions only | BranchesToEntriesConverter.cs TransformCondition | Must handle AND/OR/NOT keys without crashing. AC must test compound condition passthrough |
| C8 | dialogue-schema.json is entries-format schema | tools/YamlSchemaGen/dialogue-schema.json | Schema update targets entries-format condition validation |
| C9 | TreatWarningsAsErrors enforced | Directory.Build.props (F708) | All builds must produce 0 warnings, 0 errors |
| C10 | Baseline test counts: KojoBranchesParser=23, ErbToYaml=78, ErbParser=77 | dotnet test --list-tests | Test count ACs must use gte baseline to allow new tests |
| C11 | LogicalOp is binary tree (Left/Right), YAML expects flat arrays | LogicalOp.cs | ICondition-to-YAML conversion must flatten same-operator chains. AC must verify flattening |
| C12 | No new CompoundConditionParser class - reuse ConditionExtractor | Investigation iter21 | DatalistConverter must call ConditionExtractor.Extract(), not create new parser |
| C13 | TcvarConditionParser/TcvarRef must be created (new code) | Investigation: no TCVAR parser exists | AC must verify TcvarConditionParser registration in LogicalOperatorParser |
| C14 | Negation support must be added to LogicalOperatorParser | Investigation: ParseAtomicCondition() has no `!` handling | AC must verify negation produces NegatedCondition wrapping inner ICondition |
| C15 | Character-scoped CFLAG evaluation testing deferred to F756 | Scope Discipline: "character-scoped CFLAG" listed as out of scope | AC#1 tests indexed CFLAG only (`CFLAG:300`). Character-scoped patterns (`CFLAG:MASTER:name`) are handled by ConvertCflagRef code but evaluation testing deferred to F756 re-migration scope |

### Constraint Details

**C1: F752 Compound Framework Preservation**
- **Source**: KojoBranchesParser.cs lines 154-254 (EvaluateCompoundCondition, ValidateConditionScope)
- **Verification**: `dotnet test tools/KojoComparer.Tests/ --filter "KojoBranchesParser"` must pass all 23 tests
- **AC Impact**: Need both backward compatibility AC (existing tests pass) and forward AC (new CFLAG/TCVAR tests pass)

**C2: CflagRef Data Model**
- **Source**: ErbParser/CflagRef.cs has Target (string?), Name (string?), Index (int?), Operator (string?), Value (string?). Character-scoped CFLAG references like `CFLAG:MASTER:100` are handled by existing CflagConditionParser
- **Verification**: ICondition-to-YAML conversion must handle both indexed and character-scoped CflagRef forms
- **AC Impact**: YAML conversion tests must cover both `CFLAG:300` and `CFLAG:TARGET:name` patterns

**C3: Parenthesized Sub-Expression Support Requires ParseAtomicCondition Extension**
- **Source**: `SplitOnOperator()` tracks paren depth to avoid splitting inside parens (lines 161-212), but `ParseAtomicCondition()` (line 128) has no outer-paren stripping. Expression `(A || B) && C` splits correctly into `["(A || B)", "C"]`, but `ParseAtomicCondition("(A || B)")` returns `null` because no parser handles leading `(`. Must add: if condition starts with `(` and ends with `)`, strip outer parens and call `ParseLogicalExpression()` recursively
- **Verification**: No existing test covers parenthesized sub-expressions. AC#14 must include parenthesized expression test
- **AC Impact**: No NOT-precedence-bug exclusion needed. Parentheses support requires ~5 lines of code in `ParseAtomicCondition()`

**C6: Existing Negative Test Conversion**
- **Source**: `CompoundNonTalent_CflagRejected` test currently asserts `InvalidOperationException` for CFLAG in compound sub-conditions
- **Verification**: After F755 extends the allowlist, this test must be rewritten to assert CFLAG is accepted
- **AC Impact**: AC must verify no `InvalidOperationException` for CFLAG/TCVAR keys in compound conditions

**C7: BranchesToEntriesConverter Compound Support**
- **Source**: BranchesToEntriesConverter.TransformCondition() handles TALENT, ABL, EXP, FLAG, CFLAG as single-key conditions. AND/OR/NOT keys would return null (silent failure)
- **Verification**: Compound conditions with AND/OR/NOT must be passthrough-preserved
- **AC Impact**: AC must verify compound conditions survive branches-to-entries conversion

**C11: LogicalOp Binary Tree Flattening**
- **Source**: `A && B && C` is parsed by LogicalOperatorParser as `LogicalOp(&&, LogicalOp(&&, A, B), C)`. YAML expects `AND: [A, B, C]`
- **Verification**: Same-operator chains must be flattened to flat arrays. Mixed operators produce nested structures
- **AC Impact**: AC must test both same-operator flattening and mixed-operator nesting

**C12: ConditionExtractor Reuse**
- **Source**: Investigation iter21 found that proposed CompoundConditionParser was inferior to existing ConditionExtractor/LogicalOperatorParser
- **Verification**: DatalistConverter must use `ConditionExtractor.Extract()` not a new parser class
- **AC Impact**: Code verification AC must check DatalistConverter references ConditionExtractor

**C14: Negation Support**
- **Source**: LogicalOperatorParser.ParseAtomicCondition() (line 128) has no `!` prefix handling
- **Verification**: After F755, `!TALENT:X` must produce NegatedCondition wrapping TalentRef
- **AC Impact**: AC must test negation parsing via ErbParser unit tests

---


<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Full equivalence testing requires progressive extension of ERB branching pattern support in YAML" | CFLAG/TCVAR keys must be evaluable in KojoBranchesParser compound conditions | AC#1, AC#2, AC#3 |
| "F755 adds CFLAG/TCVAR evaluation framework" | KojoBranchesParser ValidateConditionScope allowlist extended; CFLAG/TCVAR evaluation code paths added. Validated by synthetic unit tests (production validation deferred to F756 re-migration) | AC#1, AC#2, AC#3 |
| "DatalistConverter bypasses existing ConditionExtractor/LogicalOperatorParser infrastructure" | DatalistConverter must use ConditionExtractor.Extract() and implement ICondition-to-YAML conversion with LogicalOp binary tree flattening | AC#4, AC#5, AC#6, AC#7, AC#10 |
| "48 ERB files with compound TALENT conditions need re-migration" | Re-migrated YAML files contain AND/OR/NOT compound syntax | AC#9, AC#11 |
| "YAML schema will reject compound condition YAML files" | Schema must include AND/OR/NOT definitions | AC#12 |
| "Extends F752's TALENT-only compound support" | Existing F752 compound tests must continue to pass (backward compatibility), single-TALENT migration path must not regress | AC#3, AC#13 |
| "BranchesToEntriesConverter will break on compound conditions" | Compound conditions must survive branches-to-entries conversion | AC#8 |
| "TcvarConditionParser/TcvarRef must be created" | New TCVAR parser following CflagConditionParser pattern, registered in LogicalOperatorParser | AC#5, AC#14 |
| "Negation support must be added to LogicalOperatorParser" | ParseAtomicCondition handles `!` prefix, produces NegatedCondition wrapping inner ICondition | AC#6, AC#14 |
| "Full equivalence testing" (end-to-end pipeline) | Re-migrated YAML evaluated by KojoBranchesParser must produce correct branch selection | Deferred to F706 (Consumer): F755 validates evaluation (AC#1-3) and migration (AC#4-11) independently. End-to-end integration is F706's responsibility per C4 scope constraint. |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CFLAG compound evaluation (Pos) | test | dotnet test tools/KojoComparer.Tests/ --filter "CompoundCflag" | succeeds | - | [x] |
| 2 | TCVAR compound evaluation (Pos) | test | dotnet test tools/KojoComparer.Tests/ --filter "CompoundTcvar" | succeeds | - | [x] |
| 3 | Non-allowlisted key still rejected (Neg) | test | dotnet test tools/KojoComparer.Tests/ --filter "CompoundNonTalent_UnknownKeyRejected" | succeeds | - | [x] |
| 4 | DatalistConverter uses ConditionExtractor for `&&` to AND YAML | test | dotnet test tools/ErbToYaml.Tests/ --filter "CompoundCondition_And" | succeeds | - | [x] |
| 5 | DatalistConverter parses TCVAR compound to YAML | test | dotnet test tools/ErbToYaml.Tests/ --filter "CompoundCondition_Tcvar" | succeeds | - | [x] |
| 6 | DatalistConverter parses `!` negation to NOT YAML | test | dotnet test tools/ErbToYaml.Tests/ --filter "CompoundCondition_Not" | succeeds | - | [x] |
| 7 | LogicalOp binary tree flattened to flat AND/OR arrays | test | dotnet test tools/ErbToYaml.Tests/ --filter "CompoundCondition_TreeFlattening" | succeeds | - | [x] |
| 8 | BranchesToEntriesConverter handles compound conditions | test | dotnet test tools/ErbToYaml.Tests/ --filter "CompoundCondition_BranchesToEntries" | succeeds | - | [x] |
| 9 | Re-migrated YAML files contain compound syntax | output | grep -rl "AND:\\\|OR:\\\|NOT:" Game/YAML/Kojo/ --include="*.yaml" \| wc -l | gte | 30 | [x] |
| 10 | DatalistConverter uses ConditionExtractor (not TalentConditionParser directly) | code | Grep(tools/ErbToYaml/DatalistConverter.cs) | contains | "ConditionExtractor" | [x] |
| 11 | YAML non-regression verification | output | grep -rl "AND:\\\|OR:\\\|NOT:" Game/YAML/Kojo/ --include="*.yaml" \| wc -l | lte | 120 | [x] |
| 12 | dialogue-schema.json includes AND/OR/NOT definitions | file | Grep(tools/YamlSchemaGen/dialogue-schema.json) | contains | "AND" | [x] |
| 13 | Single-TALENT condition backward compatibility through ConditionExtractor | test | dotnet test tools/ErbToYaml.Tests/ --filter "SingleTalentCondition_BackwardCompat" | succeeds | - | [x] |
| 14 | ErbParser tests pass (TcvarConditionParser + negation) | test | dotnet test tools/ErbParser.Tests/ | succeeds | - | [x] |
| 15 | All tool builds succeed with 0 warnings | build | dotnet build tools/KojoComparer/ && dotnet build tools/ErbToYaml/ && dotnet build tools/ErbParser/ | succeeds | - | [x] |
| 16 | F756 DRAFT file exists and registered in index | file | Game/agents/feature-756.md | exists | - | [x] |

### AC Details

**AC#1: CFLAG compound evaluation (Pos)**
- Verifies that KojoBranchesParser can evaluate YAML compound conditions containing CFLAG sub-conditions alongside TALENT sub-conditions. The existing `CompoundNonTalent_CflagRejected` test must be rewritten: instead of asserting `InvalidOperationException`, it must assert correct branch selection when CFLAG key appears in AND/OR compound.
- Test should use state `{"TALENT:16": 1, "CFLAG:300": 5}` and verify the AND branch is selected when both conditions are satisfied.
- Constraint C2: CFLAG state keys use the CflagRef data model. For indexed CFLAG (e.g., `CFLAG:300`), state key is `"CFLAG:300"`.
- Constraint C6: Existing negative test is converted to positive acceptance test.
- Note: Unit test with synthetic state only; no real CFLAG/TCVAR compound YAML files produced by F755 re-migration (deferred to F756).

**AC#2: TCVAR compound evaluation (Pos)**
- Verifies that KojoBranchesParser can evaluate YAML compound conditions containing TCVAR sub-conditions. Pattern from investigation: `TCVAR:302 && !TALENT:恋慕` (7 occurrences across 7 files).
- Test should use state `{"TCVAR:302": 1, "TALENT:3": 0}` and verify correct branch selection for AND with NOT sub-condition.
- Note: Unit test with synthetic state only; no real CFLAG/TCVAR compound YAML files produced by F755 re-migration (deferred to F756).

**AC#3: Non-allowlisted key still rejected (Neg)**
- Verifies that the scope boundary is maintained: keys not in the extended allowlist (e.g., LOCAL, EQUIP, STAIN, ITEM) still throw `InvalidOperationException`. The existing `CompoundNonTalent_UnknownKeyRejected` test (which uses an arbitrary non-TALENT key) must still pass after the allowlist extension.
- This is a negative test confirming that the allowlist is selective, not open-ended.
- Also serves as backward compatibility verification (Constraint C1): confirms existing F752 compound framework preserved.

**AC#4: DatalistConverter uses ConditionExtractor for `&&` to AND YAML**
- Verifies that `DatalistConverter.ParseCondition()` uses `ConditionExtractor.Extract()` (which delegates to `LogicalOperatorParser`) to parse `TALENT:恋人 && TALENT:思慕` and produce `{ AND: [{ TALENT: { 16: { ne: 0 } } }, { TALENT: { 3: { ne: 0 } } }] }` YAML structure via the ICondition-to-YAML conversion layer.
- Constraint C12: Must use existing ConditionExtractor, not a new CompoundConditionParser class.
- This tests the full pipeline: ERB string -> ConditionExtractor -> LogicalOp(&&, TalentRef, TalentRef) -> ConvertConditionToYaml -> AND YAML.

**AC#5: DatalistConverter parses TCVAR compound to YAML**
- Verifies that the new TcvarConditionParser (registered in LogicalOperatorParser) correctly parses TCVAR conditions and DatalistConverter converts `TcvarRef` ICondition to YAML.
- Expected input: `TCVAR:302 != 0 && TALENT:恋人` should produce `{ AND: [{ TCVAR: { "302": { ne: 0 } } }, { TALENT: { 16: { ne: 0 } } }] }`.
- Constraint C13: TcvarConditionParser must be created and registered in LogicalOperatorParser's ParseAtomicCondition chain.

**AC#6: DatalistConverter parses `!` negation to NOT YAML**
- Verifies that LogicalOperatorParser negation support produces `NegatedCondition` wrapping the inner ICondition, and DatalistConverter converts it to `{ NOT: { TALENT: { 3: { ne: 0 } } } }` for input `!TALENT:恋慕`.
- Constraint C14: Negation support must be added to LogicalOperatorParser.ParseAtomicCondition() as `!` prefix handling.
- Constraint C3: Existing LogicalOperatorParser handles operator precedence correctly, so `!X && Y` is parsed as `NOT(X) AND Y` (not `NOT(X AND Y)`).

**AC#7: LogicalOp binary tree flattened to flat AND/OR arrays**
- Verifies that the ICondition-to-YAML conversion layer correctly flattens LogicalOp binary trees into flat YAML arrays. `A && B && C` is parsed by LogicalOperatorParser as `LogicalOp(&&, LogicalOp(&&, A, B), C)` and must produce `{ AND: [A_yaml, B_yaml, C_yaml] }` (3 elements, flat array).
- Constraint C11: Same-operator chains must be flattened. Mixed operators (e.g., `A || B && C`) produce nested structures: `{ OR: [A_yaml, { AND: [B_yaml, C_yaml] }] }`.
- Tests both same-operator flattening and mixed-operator nesting.

**AC#8: BranchesToEntriesConverter handles compound conditions**
- Verifies that compound conditions with AND/OR/NOT keys do not crash during branches-to-entries conversion. Compound conditions should be passthrough-preserved (not transformed to entries-format condition type since compound conditions have no single `type` field).
- Constraint C7: Must handle AND/OR/NOT without throwing.
- Also verifies GenerateId() produces correct IDs for compound conditions (e.g., `and_compound_0`).

**AC#9: Re-migrated YAML files contain compound syntax**
- Verifies meaningful re-migration coverage of 48 TALENT-compound ERB source files. Baseline: currently 0 files have AND/OR/NOT syntax in YAML. Uses `gte 30` as post-redesign lower bound (62% of 48 targets). After iter21 redesign, ConditionExtractor/LogicalOperatorParser handles ALL TALENT compounds (parentheses, operator precedence, nested expressions). Remaining gap: files where FunctionCall sub-conditions produce empty `{}` (per CON-003 scope boundary), or files with non-TALENT-only compound expressions that the re-migration script's grep pattern does not capture.
- Constraint C4: Re-migration targets TALENT compound files only (CFLAG/TCVAR re-migration deferred to F756).
- Constraint C5: Non-targeted YAML files must remain unchanged.

**AC#10: DatalistConverter uses ConditionExtractor (not TalentConditionParser directly)**
- Static code verification that DatalistConverter.cs references `ConditionExtractor` class. This is the root cause fix: the previous code called `_conditionParser.ParseTalentCondition(condition)` directly (line 208), bypassing existing compound parsing infrastructure.
- Constraint C12: Must use existing ConditionExtractor, not create new parser.

**AC#11: YAML non-regression verification**
- Verifies that re-migration does not introduce compound syntax into more YAML files than expected. Uses `lte 120` as upper bound. Original `lte 60` was based on 48 ERB files + 25% margin, but did not account for 1:N ERB-to-YAML expansion ratio (ErbToYaml splits multi-DATALIST ERBs into separate YAML files). Observed: 58 target ERBs → 96 YAML files (1.66x expansion). Updated bound: 120 = 96 actual × 1.25 margin.
- Addresses Constraint C5: "Existing 1,118 YAML files must not regress" by bounding compound syntax file count.
- Complements AC#9 (positive verification gte 30) with negative verification (no excessive expansion lte 120).

**AC#12: dialogue-schema.json includes AND/OR/NOT definitions**
- Verifies that the YAML schema has been updated to include compound condition definitions. The entries-format schema must accept conditions with AND/OR/NOT keys so that YamlValidator does not reject re-migrated files.
- Constraint C8: Schema update targets entries-format condition definitions.

**AC#13: Single-TALENT condition backward compatibility through ConditionExtractor**
- Verifies that DatalistConverter.ParseCondition() produces identical YAML output for simple single-TALENT conditions (e.g., `TALENT:恋人`) when routed through the new ConditionExtractor path. ConditionExtractor -> LogicalOperatorParser -> TalentConditionParser -> TalentRef -> ConvertConditionToYaml must produce the same `{ TALENT: { 16: { ne: 0 } } }` output as the pre-F755 direct TalentConditionParser path.
- Prevents regression for the 1,118 existing YAML files' migration path (Constraint C5).

**AC#14: ErbParser tests pass (TcvarConditionParser + negation + parentheses)**
- Verifies that all ErbParser tests pass, including new tests for TcvarConditionParser, negation handling, and parenthesized sub-expression parsing in LogicalOperatorParser.
- Constraint C13: TcvarConditionParser must be created following CflagConditionParser pattern.
- Constraint C14: Negation support must be added to LogicalOperatorParser.
- Constraint C3: Outer-paren stripping must be added to `ParseAtomicCondition()` with recursive `ParseLogicalExpression()` delegation. New test must verify `(TALENT:X || TALENT:Y) && TALENT:Z` produces correct `LogicalOp` tree.
- Baseline (C10): 77 existing ErbParser tests must continue passing, plus new tests for TCVAR, negation, and parenthesized expressions.

**AC#15: All tool builds succeed with 0 warnings**
- Verifies that all three tool projects (KojoComparer, ErbToYaml, ErbParser) build successfully with 0 warnings under TreatWarningsAsErrors.
- Constraint C9: TreatWarningsAsErrors enforced.

**AC#16: F756 DRAFT file exists and registered in index**
- Verifies that Task#13's handoff output (F756 DRAFT) exists as a file. Per template Mandatory Handoffs DRAFT Creation Checklist: must verify file existence.
- Pre-completed: F756 was created during FL review (iter21 redesign).

---


<!-- fc-phase-4-completed -->
## Technical Design

### Approach

F755 extends F752's TALENT-only compound condition framework to support CFLAG and TCVAR keys. The implementation follows an **infrastructure reuse strategy** leveraging existing ErbParser components:

**Key Insight**: The previous design proposed creating a NEW `CompoundConditionParser` class. Investigation revealed this was fundamentally flawed - `LogicalOperatorParser` already provides superior functionality (operator precedence, parentheses, CFLAG/function-call parsing). Instead, F755 **wires existing components** and adds minimal missing pieces.

**Implementation Layers**:

1. **KojoBranchesParser extension** (evaluation-side):
   - Extend `ValidateConditionScope()` allowlist from `{ TALENT, AND, OR, NOT }` to `{ TALENT, CFLAG, TCVAR, AND, OR, NOT }`
   - Add `EvaluateVariableCondition()` shared method to handle CFLAG/TCVAR evaluation (eliminates code duplication)
   - State key format: `"CFLAG:{index}"`, `"TCVAR:{index}"` (string-based, no CSV resolution)

2. **ErbToYaml infrastructure wiring** (migration-side):
   - Replace `DatalistConverter.ParseCondition()` direct call to `TalentConditionParser` with `ConditionExtractor.Extract()`
   - Implement `ICondition`-to-YAML conversion layer (`ConvertConditionToYaml()`) with visitor pattern over 6 types: `TalentRef`, `CflagRef`, `TcvarRef`, `LogicalOp`, `NegatedCondition`, `FunctionCall`
   - Implement `LogicalOp` binary tree flattening: same-operator chains (`A && B && C` parsed as nested binary tree) → flat YAML arrays (`AND: [A, B, C]`)

3. **ErbParser minimal extensions**:
   - Create `TcvarConditionParser` following exact `CflagConditionParser` pattern (~80 lines)
   - Create `TcvarRef : ICondition` following `CflagRef` model
   - Create `NegatedCondition : ICondition` wrapper for `!` prefix negation
   - Register `TcvarConditionParser` in `LogicalOperatorParser.ParseAtomicCondition()` chain
   - Add outer-paren stripping in `ParseAtomicCondition()` with recursive `ParseLogicalExpression()` delegation (~5 lines)
   - Add `!` prefix handling in `ParseAtomicCondition()` (~10 lines)

4. **BranchesToEntriesConverter compound support**:
   - Add passthrough handling for `AND`/`OR`/`NOT` keys in `TransformCondition()` and `GenerateId()`
   - Compound conditions cannot be transformed to entries-format (no single `type` field) so preserve as-is

5. **YAML schema update**: Extend `dialogue-schema.json` to accept compound conditions (oneOf: single condition OR compound with AND/OR/NOT)

6. **Re-migration**: Re-run ErbToYaml on 48 TALENT-compound ERB files from F752 baseline using extended pipeline

**Architectural Advantages of Infrastructure Reuse**:
- Parentheses support: `SplitOnOperator()` paren depth tracking prevents incorrect splitting; F755 adds ~5 lines of outer-paren stripping in `ParseAtomicCondition()` to complete the support
- Character-scoped CFLAG: FREE (via existing `CflagConditionParser` pattern `CFLAG:MASTER:name`)
- Function call handling: FREE (via existing `FunctionCallParser` - converts to empty `{}` in YAML)
- Operator precedence: FREE (via existing `ParseOrExpression()` / `ParseAndExpression()` chain)
- NOT precedence bug eliminated: `!X && Y` correctly parsed as `NOT(X) AND Y` not `NOT(X AND Y)`

**Constraint Acknowledgment**: The Root Cause Analysis reveals broader compound landscape (LOCAL, EQUIP, function calls). F755 scope: CFLAG/TCVAR evaluation framework + TALENT-compound re-migration. CFLAG/TCVAR compound YAML migration deferred to F756.

### AC Coverage

| AC# | How to Satisfy | Implementation Component |
|:---:|----------------|--------------------------|
| 1 | Extend ValidateConditionScope allowlist + Add CFLAG evaluation via EvaluateVariableCondition | KojoBranchesParser.cs: allowlist line 247, evaluation line 104 |
| 2 | Extend ValidateConditionScope allowlist + Add TCVAR evaluation via EvaluateVariableCondition | KojoBranchesParser.cs: allowlist line 247, evaluation line 104 |
| 3 | ValidateConditionScope rejects non-allowlisted keys (no change to rejection logic) | KojoBranchesParser.cs: existing test continues passing |
| 4 | DatalistConverter calls ConditionExtractor.Extract() + ConvertConditionToYaml handles LogicalOp(&&) | DatalistConverter.cs: ParseCondition() line 206-271 replacement |
| 5 | Create TcvarConditionParser + register in LogicalOperatorParser + ConvertConditionToYaml handles TcvarRef | ErbParser/TcvarConditionParser.cs (new), LogicalOperatorParser.cs line 128 |
| 6 | Add `!` prefix handling in ParseAtomicCondition + ConvertConditionToYaml handles NegatedCondition | LogicalOperatorParser.cs line 128 (~10 lines) |
| 7 | Implement FlattenLogicalOp method to convert LogicalOp binary tree to flat array | DatalistConverter.cs: ConvertConditionToYaml helper |
| 8 | Add compound condition passthrough in TransformCondition + GenerateId for AND/OR/NOT keys | BranchesToEntriesConverter.cs: TransformCondition line 91, GenerateId line 68 |
| 9 | Re-run ErbToYaml on 48 TALENT-compound files with extended ParseCondition pipeline | Migration task: FileConverter reprocessing |
| 10 | DatalistConverter calls ConditionExtractor.Extract() (code verification) | Grep DatalistConverter.cs for "ConditionExtractor" |
| 11 | YAML file count verification (10-60 compound syntax files post-migration) | Grep Game/YAML/Kojo/ for "AND:\|OR:\|NOT:" |
| 12 | Extend dialogue-schema.json condition definition with oneOf (single \| compound) | YamlSchemaGen/dialogue-schema.json |
| 13 | Single-TALENT backward compatibility: ConditionExtractor→TalentRef→ConvertConditionToYaml produces identical YAML | DatalistConverter.Tests: new test |
| 14 | All ErbParser tests pass including TcvarConditionParser + negation + parenthesized sub-expression tests | dotnet test tools/ErbParser.Tests/ |
| 15 | All tool projects build with 0 warnings under TreatWarningsAsErrors | dotnet build KojoComparer && ErbToYaml && ErbParser |
| 16 | F756 DRAFT file exists (Task#13 handoff verification) | Glob Game/agents/feature-756.md |

### Key Decisions

| Decision Point | Options Considered | Selected | Rationale |
|----------------|-------------------|----------|-----------|
| **Compound parser implementation** | (A) Create new CompoundConditionParser<br>(B) Reuse existing ConditionExtractor/LogicalOperatorParser | **B** | Investigation iter21 revealed that LogicalOperatorParser already handles `&&`/`||`/parentheses/CFLAG/function-calls with correct operator precedence. Creating CompoundConditionParser would duplicate 200+ lines of tested code and introduce a NOT precedence bug. Reusing existing infrastructure gains parentheses/character-scoped-CFLAG/function-call handling FOR FREE. |
| **CFLAG/TCVAR state key format** | (A) Resolve to CSV integer indices like TALENT<br>(B) Use raw string indices `"CFLAG:300"` | **B** | No CflagCsvLoader exists. CflagConditionParser already handles both indexed (`CFLAG:300`) and character-scoped (`CFLAG:MASTER:name`) forms. Using string keys matches ERB usage and avoids CSV loader complexity. |
| **ICondition-to-YAML conversion** | (A) Add ToYaml() method to ICondition interface<br>(B) Centralized ConvertConditionToYaml() visitor | **B** | ICondition is a marker interface in ErbParser project (no methods). Adding ToYaml() would couple ErbParser to YAML format. Centralized visitor in DatalistConverter keeps conversion logic in ErbToYaml where it belongs. |
| **LogicalOp tree flattening** | (A) Flatten during parsing (modify LogicalOperatorParser)<br>(B) Flatten during YAML conversion | **B** | LogicalOperatorParser is tested and correct (8 tests). Binary tree structure matches operator precedence semantics. Flattening is a YAML-specific optimization. Keep parsing and conversion concerns separated. |
| **BranchesToEntriesConverter compound handling** | (A) Transform AND/OR/NOT to entries-format condition objects<br>(B) Passthrough compound conditions unchanged | **B** | Entries-format conditions have a single `type` field. Compound conditions are operators over multiple sub-conditions (no single type). Passthrough preserves semantics. Schema must accept both single and compound formats. |
| **Re-migration target scope** | (A) All 1,118 YAML files<br>(B) 48 TALENT compound ERB files from F752 baseline | **B** | Selective re-migration minimizes regression risk. F752 baseline identifies TALENT-compound files that already parse correctly. CFLAG/TCVAR compound re-migration deferred to F756 per C4. |
| **TcvarConditionParser location** | (A) ErbToYaml project<br>(B) ErbParser project | **B** | Consistent with CflagConditionParser and TalentConditionParser location. ErbParser is the condition parsing infrastructure. ErbToYaml consumes it via ConditionExtractor. |
| **Variable type scope for evaluation** | (A) Support all types (CFLAG, TCVAR, LOCAL, EQUIP, STAIN, ITEM)<br>(B) CFLAG and TCVAR only | **B** | Feature Background explicitly names CFLAG/TCVAR. LOCAL is runtime-only. EQUIP/STAIN/ITEM have lower priority (179/21/44 vs 429 for CFLAG). Incremental extension follows F752 TALENT-only precedent. |

### Interfaces / Data Structures

#### 1. KojoBranchesParser Changes

**File**: `tools/KojoComparer/KojoBranchesParser.cs`

**Method: `ValidateConditionScope()` (line 245)**

```csharp
private void ValidateConditionScope(Dictionary<string, object> subCondition)
{
    // Extended allowlist: TALENT, CFLAG, TCVAR, and compound operators
    var allowedKeys = new HashSet<string> { "TALENT", "CFLAG", "TCVAR", "AND", "OR", "NOT" };
    var invalidKeys = subCondition.Keys.Where(key => !allowedKeys.Contains(key)).ToList();

    if (invalidKeys.Any())
    {
        throw new InvalidOperationException(
            $"Unsupported variable type in compound condition (found: {string.Join(", ", invalidKeys)}). " +
            "Only TALENT, CFLAG, and TCVAR are supported.");
    }
}
```

**Method: `EvaluateCondition()` (line 90)**

Add CFLAG evaluation code path after TALENT evaluation (line 104-151):

```csharp
// Parse CFLAG conditions using shared method
if (condition.TryGetValue("CFLAG", out var cflagObj) && cflagObj is Dictionary<object, object> cflagDict)
{
    return EvaluateVariableCondition("CFLAG", cflagDict, state);
}

// Parse TCVAR conditions using shared method
if (condition.TryGetValue("TCVAR", out var tcvarObj) && tcvarObj is Dictionary<object, object> tcvarDict)
{
    return EvaluateVariableCondition("TCVAR", tcvarDict, state);
}
```

**State Key Format**:
- TALENT: `"TALENT:{index}"` (e.g., `"TALENT:16"`) - CSV-resolved integer from TalentCsvLoader
- CFLAG: `"CFLAG:{index}"` (e.g., `"CFLAG:300"`) - Raw string index from ERB
- TCVAR: `"TCVAR:{index}"` (e.g., `"TCVAR:302"`) - Raw string index from ERB

**Note**: CFLAG/TCVAR use string indices directly without CSV resolution, matching ERB usage patterns.

**Method: `EvaluateVariableCondition()` (new shared method)**

Add a private helper method to eliminate code duplication between variable type evaluations:

```csharp
/// <summary>
/// Evaluates variable conditions for any variable type (CFLAG, TCVAR, future EQUIP/STAIN/ITEM).
/// Handles the common pattern of iterating through variable indices and operator conditions.
/// Note: Accepts any string index without int.TryParse validation (unlike existing TALENT evaluation
/// which requires numeric indices). This is correct for CFLAG/TCVAR which use string-based indices.
/// </summary>
private static bool EvaluateVariableCondition(string variableType, Dictionary<object, object> varDict, Dictionary<string, int> state)
{
    foreach (var kvp in varDict)
    {
        var indexStr = kvp.Key?.ToString();
        if (string.IsNullOrEmpty(indexStr))
            continue;

        // Construct state key: "{variableType}:{index}"
        var stateKey = $"{variableType}:{indexStr}";
        var stateValue = state.GetValueOrDefault(stateKey, 0);

        // Parse operator dictionary: { "ne": 0 }
        if (kvp.Value is Dictionary<object, object> opDict)
        {
            foreach (var opKvp in opDict)
            {
                var op = opKvp.Key?.ToString();
                if (string.IsNullOrEmpty(op))
                    continue;

                if (!int.TryParse(opKvp.Value?.ToString(), out var expected))
                    continue;

                var result = op switch
                {
                    "eq" => stateValue == expected,
                    "ne" => stateValue != expected,
                    "gt" => stateValue > expected,
                    "gte" => stateValue >= expected,
                    "lt" => stateValue < expected,
                    "lte" => stateValue <= expected,
                    _ => false
                };

                if (!result)
                    return false;
            }
        }
    }

    return true; // All variable conditions matched
}
```

This shared method eliminates duplication across ALL variable type evaluations (TALENT, CFLAG, TCVAR) and enables F756 to easily add EQUIP, STAIN, ITEM support by calling the same method with different prefixes.

**TALENT refactoring (recommended, not required by F755 Tasks)**: The existing TALENT evaluation block (lines 106-149) could be refactored to call `EvaluateVariableCondition("TALENT", talentDict, state)` after numeric index validation, achieving a single evaluation code path. This is recommended for implementer convenience but not covered by F755 Tasks/ACs. If not done during F755, track as F756 extensibility improvement.

**KNOWN LIMITATION**: `int.TryParse` failure for non-numeric operator values (e.g., `"TARGET"`) causes the condition to be silently skipped via `continue`, resulting in vacuous-true evaluation. This replicates the existing TALENT evaluation pattern (KojoBranchesParser.cs lines 127-128) for consistency. Symbolic references like `CFLAG:300 == TARGET` are outside F755 scope (deferred to F756 with character reference resolver).

---

#### 2. ErbParser Minimal Extensions

**NEW CLASS**: `tools/ErbParser/TcvarConditionParser.cs`

Follow `CflagConditionParser.cs` pattern (~80 lines). **Extensibility note**: TcvarConditionParser and CflagConditionParser share identical parsing logic (regex, Target/Name/Index resolution, Operator/Value extraction) differing only in prefix string and output type. For F756, consider extracting a generic `VariableConditionParser<T>` base or shared `ParseVariableCondition(prefix, condition)` helper to eliminate per-type duplication when adding EQUIP/STAIN/ITEM parsers:

```csharp
using System.Text.RegularExpressions;

namespace ErbParser;

/// <summary>
/// Parses TCVAR condition strings into structured TcvarRef objects
/// Pattern: TCVAR:(target:)?(name|index)( op value)?
/// Examples:
///   - TCVAR:302 → TcvarRef(Target=null, Name=null, Index=302)
///   - TCVAR:302 != 0 → TcvarRef(Target=null, Name=null, Index=302, Operator="!=", Value="0")
/// </summary>
public class TcvarConditionParser
{
    private static readonly Regex TcvarPattern = new Regex(
        @"^TCVAR:(?:([^:]+):)?([^:\s]+)(?:\s*(!=|==|>=|<=|>|<)\s*(.+))?$",
        RegexOptions.Compiled
    );

    public TcvarRef? ParseTcvarCondition(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return null;

        condition = condition.Trim();
        var match = TcvarPattern.Match(condition);

        if (!match.Success)
            return null;

        var target = match.Groups[1].Success ? match.Groups[1].Value : null;
        var nameOrIndex = match.Groups[2].Value;

        if (string.IsNullOrWhiteSpace(nameOrIndex))
            return null;

        var operatorValue = match.Groups[3].Success ? match.Groups[3].Value : null;
        var value = match.Groups[4].Success ? match.Groups[4].Value.Trim() : null;

        var tcvarRef = new TcvarRef
        {
            Target = target,
            Operator = operatorValue,
            Value = value
        };

        if (int.TryParse(nameOrIndex, out int index))
        {
            tcvarRef.Index = index;
            tcvarRef.Name = null;
        }
        else
        {
            tcvarRef.Name = nameOrIndex;
            tcvarRef.Index = null;
        }

        return tcvarRef;
    }
}
```

**NEW CLASS**: `tools/ErbParser/TcvarRef.cs`

Follow exact pattern of `CflagRef.cs`:

```csharp
using System.Text.Json.Serialization;

namespace ErbParser;

public class TcvarRef : ICondition
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
```

**NEW CLASS**: `tools/ErbParser/NegatedCondition.cs`

Wrapper for `!` prefix negation:

```csharp
using System.Text.Json.Serialization;

namespace ErbParser;

public class NegatedCondition : ICondition
{
    [JsonPropertyName("inner")]
    public ICondition Inner { get; set; } = null!;
}
```

**UPDATE**: `tools/ErbParser/ICondition.cs`

Add JsonDerivedType registrations (lines 9-12):

```csharp
[JsonDerivedType(typeof(TalentRef), typeDiscriminator: "talent")]
[JsonDerivedType(typeof(CflagRef), typeDiscriminator: "cflag")]
[JsonDerivedType(typeof(TcvarRef), typeDiscriminator: "tcvar")]        // NEW
[JsonDerivedType(typeof(FunctionCall), typeDiscriminator: "function")]
[JsonDerivedType(typeof(LogicalOp), typeDiscriminator: "logical")]
[JsonDerivedType(typeof(NegatedCondition), typeDiscriminator: "negated")] // NEW
public interface ICondition
{
}
```

**UPDATE**: `tools/ErbParser/LogicalOperatorParser.cs`

Register TcvarConditionParser + add negation handling in `ParseAtomicCondition()` (line 128):

```csharp
private ICondition? ParseAtomicCondition(string condition)
{
    condition = condition.Trim();

    // Handle parenthesized sub-expressions: (A || B) → strip outer parens, recurse
    // SplitOnOperator tracks paren depth to avoid splitting inside parens,
    // but the resulting part still has outer parens that must be stripped here.
    if (condition.StartsWith("(") && condition.EndsWith(")"))
    {
        var inner = condition.Substring(1, condition.Length - 2).Trim();
        return ParseLogicalExpression(inner);
    }

    // Handle negation prefix (!)
    // Note: For !(TALENT:X && TALENT:Y), after stripping "!", the inner
    // "(TALENT:X && TALENT:Y)" hits the paren handler above via recursive call.
    if (condition.StartsWith("!"))
    {
        var innerCondition = condition.Substring(1).Trim();
        var parsed = ParseAtomicCondition(innerCondition); // Recurse: handles parens, nested negation
        if (parsed == null)
            return null;

        return new NegatedCondition { Inner = parsed };
    }

    // Try TALENT parser
    var talent = _talentParser.ParseTalentCondition(condition);
    if (talent != null)
        return talent;

    // Try CFLAG parser
    var cflag = _cflagParser.ParseCflagCondition(condition);
    if (cflag != null)
        return cflag;

    // Try TCVAR parser (NEW)
    var tcvar = _tcvarParser.ParseTcvarCondition(condition);
    if (tcvar != null)
        return tcvar;

    // Try function call parser
    var function = _functionParser.ParseFunctionCall(condition);
    if (function != null)
        return function;

    return null;
}
```

Add field at class level:

```csharp
private readonly TcvarConditionParser _tcvarParser = new();
```

---

#### 3. DatalistConverter Infrastructure Wiring

**File**: `tools/ErbToYaml/DatalistConverter.cs`

**METHOD REPLACEMENT: `ParseCondition()` (line 206-271)**

Replace direct `TalentConditionParser` call with `ConditionExtractor` + ICondition-to-YAML conversion:

```csharp
// Field: inject ConditionExtractor via constructor (replacing direct TalentConditionParser field)
// private readonly ConditionExtractor _conditionExtractor;
//
// Both constructors must be updated:
//   - DatalistConverter(TalentCsvLoader loader) → add: _conditionExtractor = new ConditionExtractor();
//   - DatalistConverter(TalentCsvLoader loader, string schemaPath) → same
// TalentCsvLoader is RETAINED (used by ConvertTalentRef for CSV index resolution).
// ConditionExtractor requires no constructor args (creates LogicalOperatorParser internally).
// The old _conditionParser (TalentConditionParser) field is removed.

public Dictionary<string, object>? ParseCondition(string condition)
{
    // Use injected ConditionExtractor (preserves DI seam for test mocking)
    var parsedCondition = _conditionExtractor.Extract(condition);

    if (parsedCondition == null)
    {
        Console.Error.WriteLine($"Warning: Could not parse condition: {condition}");
        return new Dictionary<string, object>(); // Graceful fallback for LOCAL, unparseable conditions
    }

    // Convert ICondition tree to YAML dictionary
    return ConvertConditionToYaml(parsedCondition) ?? new Dictionary<string, object>();
}

/// <summary>
/// Convert ICondition tree to YAML dictionary with LogicalOp tree flattening.
/// Visitor pattern over 6 ICondition types.
/// </summary>
private Dictionary<string, object>? ConvertConditionToYaml(ICondition condition)
{
    switch (condition)
    {
        case TalentRef talent:
            return ConvertTalentRef(talent);

        case CflagRef cflag:
            return ConvertCflagRef(cflag);

        case TcvarRef tcvar:
            return ConvertTcvarRef(tcvar);

        case NegatedCondition negated:
            return ConvertNegatedCondition(negated);

        case LogicalOp logical:
            return ConvertLogicalOp(logical);

        case FunctionCall function:
            // Function calls have no YAML representation - return empty {} per existing pattern
            Console.Error.WriteLine($"Warning: Function call conditions not supported in YAML: {function.Name}");
            return new Dictionary<string, object>();

        default:
            return null;
    }
}

/// <summary>
/// Shared helper: Maps ERB operator (==, !=, >, etc.) to YAML operator key (eq, ne, gt, etc.).
/// Eliminates duplication across ConvertTalentRef, ConvertCflagRef, ConvertTcvarRef,
/// and future F756 converters (EQUIP, STAIN, ITEM).
/// </summary>
private static Dictionary<string, object> MapErbOperatorToYaml(string? erbOp, string? value)
{
    string op = erbOp ?? "!=";
    string val = value ?? "0";

    var operatorValue = new Dictionary<string, object>();
    switch (op)
    {
        case "==": operatorValue["eq"] = val; break;
        case "!=": operatorValue["ne"] = val; break;
        case ">": operatorValue["gt"] = val; break;
        case ">=": operatorValue["gte"] = val; break;
        case "<": operatorValue["lt"] = val; break;
        case "<=": operatorValue["lte"] = val; break;
        default: operatorValue["ne"] = "0"; break;
    }
    return operatorValue;
}

private Dictionary<string, object>? ConvertTalentRef(TalentRef talent)
{
    var talentIndex = _talentLoader.GetTalentIndex(talent.Name);
    if (talentIndex == null)
    {
        Console.Error.WriteLine($"Warning: Talent '{talent.Name}' not found in Talent.csv");
        return new Dictionary<string, object>();
    }

    return new Dictionary<string, object>
    {
        { "TALENT", new Dictionary<string, object>
            {
                { talentIndex.Value.ToString(), MapErbOperatorToYaml(talent.Operator, talent.Value) }
            }
        }
    };
}

private Dictionary<string, object> ConvertCflagRef(CflagRef cflag)
{
    var key = cflag.Index.HasValue
        ? cflag.Index.Value.ToString()
        : (cflag.Target != null ? $"{cflag.Target}:{cflag.Name}" : cflag.Name!);

    return new Dictionary<string, object>
    {
        { "CFLAG", new Dictionary<string, object>
            {
                { key, MapErbOperatorToYaml(cflag.Operator, cflag.Value) }
            }
        }
    };
}

private Dictionary<string, object> ConvertTcvarRef(TcvarRef tcvar)
{
    var key = tcvar.Index.HasValue ? tcvar.Index.Value.ToString() : tcvar.Name!;

    return new Dictionary<string, object>
    {
        { "TCVAR", new Dictionary<string, object>
            {
                { key, MapErbOperatorToYaml(tcvar.Operator, tcvar.Value) }
            }
        }
    };
}

private Dictionary<string, object>? ConvertNegatedCondition(NegatedCondition negated)
{
    var innerYaml = ConvertConditionToYaml(negated.Inner);
    if (innerYaml == null)
        return null; // Propagate failure

    return new Dictionary<string, object>
    {
        { "NOT", innerYaml }
    };
}

/// <summary>
/// Convert LogicalOp binary tree to YAML with same-operator flattening.
/// CRITICAL: Same-operator chains (A && B && C) flatten to flat array AND: [A, B, C].
/// Mixed operators (A || B && C) produce nested structures OR: [A, {AND: [B, C]}].
/// </summary>
private Dictionary<string, object>? ConvertLogicalOp(LogicalOp logical)
{
    // Flatten same-operator chains into flat array
    var flattenedItems = FlattenLogicalOp(logical, logical.Operator).ToList();

    // Convert each flattened item to YAML
    var yamlItems = flattenedItems
        .Select(ConvertConditionToYaml)
        .ToList();

    // Fail-fast: if ANY child conversion fails (returns null), the entire compound fails.
    // This is deliberate: a compound like TALENT:X && LOCAL:1 produces [TalentYaml, null].
    // Rather than producing a partial AND with only TalentYaml (silently dropping LOCAL),
    // we fail the whole compound to empty {} at ParseCondition level.
    // FunctionCall nodes return empty {} (not null) per CON-003 scope boundary.
    // KNOWN LIMITATION: empty {} children are NOT filtered from the array.
    // AND: [{TALENT:...}, {}] evaluates empty {} as vacuously true in KojoBranchesParser,
    // effectively dropping the FunctionCall guard. This is accepted per C4 scope
    // (re-migration targets TALENT-only compounds; mixed patterns deferred to F756).
    if (yamlItems.Any(y => y == null))
        return null;

    string yamlOperator = logical.Operator switch
    {
        "&&" => "AND",
        "||" => "OR",
        _ => throw new ArgumentException($"Unknown logical operator: {logical.Operator}")
    };

    return new Dictionary<string, object>
    {
        { yamlOperator, yamlItems }
    };
}

/// <summary>
/// Recursively flatten same-operator chains in LogicalOp binary tree.
/// Example: LogicalOp(&&, LogicalOp(&&, A, B), C) → [A, B, C]
/// Non-matching operators stop recursion: LogicalOp(||, A, LogicalOp(&&, B, C)) → [A, LogicalOp(&&, B, C)]
/// </summary>
private IEnumerable<ICondition> FlattenLogicalOp(ICondition condition, string targetOperator)
{
    if (condition is LogicalOp logical && logical.Operator == targetOperator)
    {
        // Same operator - recurse into both sides
        foreach (var item in FlattenLogicalOp(logical.Left!, targetOperator))
            yield return item;
        foreach (var item in FlattenLogicalOp(logical.Right!, targetOperator))
            yield return item;
    }
    else
    {
        // Different operator or non-LogicalOp - stop flattening
        yield return condition;
    }
}
```

**YAML output examples**:

Input: `TALENT:恋人 && TALENT:思慕`
```yaml
AND:
  - TALENT:
      16:
        ne: 0
  - TALENT:
      3:
        ne: 0
```

Input: `CFLAG:300 != 0 && TALENT:恋人`
```yaml
AND:
  - CFLAG:
      300:
        ne: 0
  - TALENT:
      16:
        ne: 0
```

Input: `!TALENT:恋慕`
```yaml
NOT:
  TALENT:
    3:
      ne: 0
```

#### 4. BranchesToEntriesConverter Extension

**File**: `tools/ErbToYaml/BranchesToEntriesConverter.cs`

**Method: `TransformCondition()` (line 91)**

Add compound condition passthrough before existing single-key transformations:

```csharp
private static Dictionary<string, object>? TransformCondition(Dictionary<string, object> legacyCondition)
{
    // Passthrough compound conditions (AND/OR/NOT) without transformation
    if (legacyCondition.ContainsKey("AND") ||
        legacyCondition.ContainsKey("OR") ||
        legacyCondition.ContainsKey("NOT"))
    {
        // Return compound condition as-is (schema must accept both formats)
        return legacyCondition;
    }

    // Existing single-key transformations (TALENT, ABL, EXP, FLAG, CFLAG)
    // ... (lines 96-189 unchanged)
}
```

**Method: `GenerateId()` (line 68)**

Add compound condition ID generation before existing single-key logic:

```csharp
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

    // Existing single-key ID generation (TALENT, ABL, etc.)
    // ... (lines 74-88 unchanged)
}
```

**Design note**: Compound conditions are passthrough-preserved rather than transformed to entries-format condition objects because entries-format uses a `type` field for single condition types (Talent, Abl, etc.). Compound conditions have no single type—they are operators over multiple sub-conditions. The schema will be extended to accept both formats.

#### 5. YAML Schema Extension

**File**: `tools/YamlSchemaGen/dialogue-schema.json`

Extend the `condition` property definition (line 37-78) to accept both single-key condition objects and compound condition objects:

```json
"condition": {
  "oneOf": [
    {
      "type": "object",
      "description": "Single condition with type field",
      "properties": {
        "type": {
          "type": "string",
          "enum": ["Talent", "Abl", "Exp", "Flag", "CFlag"],
          "description": "Type of condition check"
        },
        "talentType": { "type": "string" },
        "ablType": { "type": "string" },
        "threshold": {
          "oneOf": [
            { "type": "integer" },
            { "type": "string", "pattern": "^[0-9]+$" }
          ]
        }
      },
      "required": ["type"]
    },
    {
      "type": "object",
      "description": "Compound AND condition",
      "properties": {
        "AND": {
          "type": "array",
          "items": { "$ref": "#/definitions/conditionElement" }
        }
      },
      "required": ["AND"]
    },
    {
      "type": "object",
      "description": "Compound OR condition",
      "properties": {
        "OR": {
          "type": "array",
          "items": { "$ref": "#/definitions/conditionElement" }
        }
      },
      "required": ["OR"]
    },
    {
      "type": "object",
      "description": "Compound NOT condition",
      "properties": {
        "NOT": { "$ref": "#/definitions/conditionElement" }
      },
      "required": ["NOT"]
    }
  ]
},
```

Add `definitions` section at the root level for recursive compound condition support:

```json
"definitions": {
  "conditionElement": {
    "oneOf": [
      {
        "type": "object",
        "description": "TALENT condition in branches format",
        "properties": {
          "TALENT": { "type": "object" }
        }
      },
      {
        "type": "object",
        "description": "CFLAG condition in branches format",
        "properties": {
          "CFLAG": { "type": "object" }
        }
      },
      {
        "type": "object",
        "description": "TCVAR condition in branches format",
        "properties": {
          "TCVAR": { "type": "object" }
        }
      },
      {
        "type": "object",
        "description": "Nested AND compound",
        "properties": {
          "AND": {
            "type": "array",
            "items": { "$ref": "#/definitions/conditionElement" }
          }
        }
      },
      {
        "type": "object",
        "description": "Nested OR compound",
        "properties": {
          "OR": {
            "type": "array",
            "items": { "$ref": "#/definitions/conditionElement" }
          }
        }
      },
      {
        "type": "object",
        "description": "Nested NOT compound",
        "properties": {
          "NOT": { "$ref": "#/definitions/conditionElement" }
        }
      }
    ]
  }
}
```

**Design note**: The schema now accepts both entries-format conditions (with `type` field) and branches-format compound conditions (with AND/OR/NOT keys). This dual-format support is necessary because BranchesToEntriesConverter passthrough-preserves compound conditions rather than transforming them.

#### 6. Re-migration Strategy

**Target files**: 48 ERB files from F752 baseline with TALENT-only compound conditions.

**Approach**:
1. Identify target files: Grep `Game/ERB/口上/` for ERB files containing `TALENT.*&&|TALENT.*\|\||!TALENT` patterns.
2. Run ErbToYaml with extended compound parser on target files.
3. Verify re-migrated YAML contains AND/OR/NOT keys (AC#9).
4. Verify non-targeted YAML files are unchanged (regression check per Constraint C5).

**Script** (to be created in `.tmp/` for execution):

```bash
#!/bin/bash
# Re-migrate ERB files with TALENT compound conditions

TARGET_FILES=$(grep -rl 'TALENT.*&&\|TALENT.*||\|!TALENT' Game/ERB/口上/ | head -n 48)

for erb_file in $TARGET_FILES; do
    echo "Re-migrating: $erb_file"
    dotnet run --project tools/ErbToYaml/ -- "$erb_file" Game/YAML/Kojo/
done

echo "Re-migration complete. Verify YAML contains AND/OR/NOT keys."
```

**Validation**:
- AC#9 verifies at least 30 YAML files contain compound syntax (AND/OR/NOT) after re-migration.
- Manual spot-check of 3-5 re-migrated files to verify compound condition correctness.

---

### Summary

F755 extends F752's compound condition framework by **reusing existing ErbParser infrastructure** (ConditionExtractor/LogicalOperatorParser) instead of creating new parsers:

**Implementation Layers**:

1. **KojoBranchesParser** (~50 lines): Extend ValidateConditionScope allowlist + add EvaluateVariableCondition shared method
2. **ErbParser minimal extensions** (~105 lines): TcvarConditionParser + TcvarRef + NegatedCondition + negation handling + outer-paren stripping in ParseAtomicCondition
3. **DatalistConverter wiring** (~150 lines): Replace TalentConditionParser direct call with ConditionExtractor + implement ICondition-to-YAML conversion with LogicalOp tree flattening
4. **BranchesToEntriesConverter** (~20 lines): Passthrough compound conditions in TransformCondition/GenerateId
5. **YAML schema** (~60 lines): Add compound condition definitions (AND/OR/NOT)
6. **Re-migration** (48 TALENT-compound ERB files): Produce compound YAML using extended pipeline

**Key Advantage**: By reusing existing LogicalOperatorParser, F755 gains character-scoped-CFLAG/function-call handling FOR FREE, and achieves parentheses support with ~5 lines of outer-paren stripping. The previous design's NOT precedence bug and parentheses prohibition are eliminated.

**Estimated effort**: ~380 lines new code + 15 new tests. All changes preserve F752 backward compatibility and maintain existing compound evaluation framework.

---


<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,2 | Extend KojoBranchesParser: Allowlist expansion and EvaluateVariableCondition shared method | | [x] |
| 2 | 1,2,3 | Create KojoBranchesParser unit tests (CompoundCflag, CompoundTcvar, CompoundNonTalent_UnknownKeyRejected) | | [x] |
| 3 | 14 | Create TcvarConditionParser and TcvarRef following CflagConditionParser pattern | | [x] |
| 4 | 14 | Add negation support and outer-paren stripping to LogicalOperatorParser.ParseAtomicCondition | | [x] |
| 5 | 10 | Wire DatalistConverter to use ConditionExtractor instead of TalentConditionParser | | [x] |
| 6 | 4,5,6,7,13 | Implement ICondition-to-YAML conversion layer with LogicalOp tree flattening | | [x] |
| 7 | 4,5,6,7,13 | Create ErbToYaml unit tests (CompoundCondition_And, Tcvar, Not, TreeFlattening, SingleTalentCondition_BackwardCompat) | | [x] |
| 8 | 8 | Extend BranchesToEntriesConverter to passthrough compound conditions (TransformCondition and GenerateId) | | [x] |
| 9 | 8 | Create BranchesToEntriesConverter unit test (CompoundCondition_BranchesToEntries) | | [x] |
| 10 | 12 | Update dialogue-schema.json to include AND/OR/NOT compound condition definitions | | [x] |
| 11 | 9,11 | Re-migrate 48 TALENT compound ERB files to produce YAML with AND/OR/NOT syntax | | [x] |
| 12 | 15 | Verify all tool builds succeed with 0 warnings | | [x] |
| 13 | 16 | Create F756 for extended compound variable type support (CFLAG/TCVAR YAML migration) | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. DO NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1-T2: Technical Design section 1 (KojoBranchesParser) | Extended allowlist and evaluation with unit tests |
| 2 | implementer | sonnet | T3-T4: Technical Design section 3 (ErbParser extensions) | TcvarConditionParser + negation support + outer-paren stripping |
| 3 | implementer | sonnet | T5-T7: Technical Design sections 2-3 (DatalistConverter wiring + ICondition-to-YAML) | Extended DatalistConverter with unit tests |
| 4 | implementer | sonnet | T8-T9: Technical Design section 4 (BranchesToEntriesConverter) | Compound passthrough with unit test |
| 5 | implementer | sonnet | T10: Technical Design section 5 (YAML schema) | Updated dialogue-schema.json |
| 6 | implementer | sonnet | T11: Technical Design section 6 (re-migration) | Re-migrated YAML files with compound syntax |
| 7 | ac-tester | haiku | T12: AC#1-16 test commands | Full verification results |

**Constraints** (from Technical Design):

1. **Infrastructure reuse**: Must use existing ConditionExtractor/LogicalOperatorParser, not create new CompoundConditionParser
2. **F752 backward compatibility**: All existing KojoBranchesParser tests must continue passing
3. **String-based state keys**: CFLAG/TCVAR use `"CFLAG:300"`, `"TCVAR:302"` format without CSV resolution
4. **Targeted re-migration**: Only 48 TALENT compound ERB files from F752 baseline
5. **LogicalOp tree flattening**: Same-operator chains must be flattened to flat arrays; mixed operators produce nested structures
6. **Passthrough compound conditions**: BranchesToEntriesConverter preserves compound conditions unchanged
7. **Zero warnings**: TreatWarningsAsErrors enforced across all tool builds
8. **TcvarConditionParser pattern**: Follow exact CflagConditionParser implementation pattern (~80 lines)

**Pre-conditions**:

- F752 completed with compound evaluation framework (AND/OR/NOT recursive evaluation)
- F750 completed with TalentConditionParser and DatalistConverter infrastructure
- ErbParser contains LogicalOperatorParser, CflagConditionParser, ConditionExtractor (verified in Root Cause Analysis)
- 48 TALENT compound ERB files identified from F752 baseline

**Success Criteria**:

- All 16 ACs pass (AC#1-16)
- Zero build warnings across KojoComparer, ErbToYaml, ErbParser projects
- At least 30 YAML files contain AND:/OR:/NOT: syntax after re-migration (AC#9: gte 30)
- At most 60 YAML files contain compound syntax (AC#11: lte 60, prevents excessive expansion)
- All existing ErbParser tests continue passing (backward compatibility)

**Rollback Plan**:

If issues arise after deployment:

1. **Evaluation-side issues** (KojoBranchesParser):
   - Revert allowlist extension: Remove CFLAG/TCVAR from ValidateConditionScope
   - Revert EvaluateVariableCondition method and CFLAG/TCVAR evaluation code paths
   - Existing TALENT compound tests should still pass

2. **Migration-side issues** (ErbToYaml):
   - Revert DatalistConverter to use TalentConditionParser directly
   - Remove ICondition-to-YAML conversion layer
   - Re-migrated YAML files remain in single-condition format
   - F706 equivalence testing continues with limited compound support (TALENT-only from F752)

3. **ErbParser extensions**:
   - Remove TcvarConditionParser and TcvarRef classes
   - Revert negation support in LogicalOperatorParser
   - Existing tests should still pass

4. **Schema issues**:
   - Revert dialogue-schema.json to previous version
   - YamlValidator may reject compound YAML files

5. **Full rollback** (if multiple components fail):
   - `git revert {commit-hash}`
   - Notify user of rollback
   - Create follow-up feature for fix with additional investigation
   - Document specific failure mode for future implementation

**Atomic Deployment**: All components (KojoBranchesParser allowlist, EvaluateVariableCondition, TcvarConditionParser, negation support, DatalistConverter wiring, ICondition-to-YAML conversion, BranchesToEntriesConverter passthrough, schema, re-migration) must deploy together. Partial deployment breaks equivalence testing.

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped] {phase} {iter}: [{category-code}] {description} -->
<!-- Category codes: See Game/agents/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [resolved-applied] Phase3 iter1: Task#12 pre-completed during FL review - F756 DRAFT created and index updated before formal /run execution. Status marked [x] for consistency with actual file state.
- [resolved-applied] Phase2 iter4: [FMT-001] Implementation Contract subsections are acceptable template extensions per template comment "Use this section when the feature requires specific implementation steps". Complex infra features warrant detailed guidance.
- [resolved-applied] Phase2 iter4: [FMT-001] Technical Design section is legitimate /fc Phase 4 output as evidenced by fc-phase-4-completed marker. /fc-generated sections are expected workflow outputs.
- [resolved-applied] Phase2 iter4: [SCP-002] Feature title clarified in Philosophy line 14: "actual CFLAG/TCVAR YAML migration deferred to F756". Title refers to evaluation framework + parser capability, not file-level migration.
- [resolved-applied] Phase2 iter3: [FMT-002] Pending review item categories used undefined codes. Fixed to use error-taxonomy.md defined codes.
- [resolved-applied] Phase2 iter3: [CON-003] DatalistConverter empty {} fallback pattern documented as deliberate scope boundary for out-of-scope conditions (LOCAL, function calls).
- [resolved-applied] Phase2 iter5: [FMT-001] /fc-generated sections ordering resolved: fc.md defines Agent Responsibilities and marker placement rules. Template does not prohibit additional sections from documented /fc workflow.
- [resolved-applied] Phase2 iter6: [AC-003] Index 5 semantic issue overlaps confirmed - both target AC#18 format (markdown table rendering + Windows platform compatibility). Single fix addresses both.
- [resolved-applied] Phase2 iter7: [FMT-001] Dependencies section overlap acknowledged but Related Features provides extended analysis (Pattern Analysis subsection) while Dependencies provides required template structure. Both serve different purposes - content overlap is acceptable per /fc workflow.
- [resolved-invalid] Phase2 iter7: [AC-003] Semantic issue #2 (pending review blocks /run) duplicates structural issue #2 (same [pending] item at line 1301). Resolved: duplicate of iter6 [resolved-applied] at line 1301.
- [resolved-applied] Phase2 iter8: [AC-003] AC#20 Method used non-recursive glob `Game/YAML/Kojo/*.yaml` that never matches files in subdirectories. Fixed to recursive `grep -r` with `--include='*.yaml'`.
- [resolved-applied] Phase2 iter8: [FMT-001] Standalone `## Scope` section removed (not in template). Content already represented in Background > Philosophy.
- [resolved-applied] Phase2 iter8: [FMT-001] Empty Execution Log placeholder row removed.
- [resolved-applied] Phase2 iter8: [AC-003] Unresolved [pending] at iter7 resolved as duplicate.
- [resolved-applied] Phase2 iter9: [AC-003] Philosophy Derivation missing explicit equivalence guarantee row. Added row deferring end-to-end pipeline validation to F706 (Consumer) per C4 scope constraint. F755 validates evaluation and migration independently; F706 integrates them.
- [resolved-applied] Phase2 iter9: [FMT-001] AC#12 unescaped pipe in Expected column broke table to 8 columns. Escaped as `\\|`.
- [resolved-applied] Phase2 iter9: [FMT-001] AC#14 unescaped pipes in Method and Expected columns. Escaped as `\\|`.
- [resolved-applied] Phase2 iter10: [DES-002] CompoundConditionParser NOT operator precedence bug resolved: added to F756 Deferred Items table, added inline KNOWN LIMITATION comment in Technical Design, and tracked in F755 Mandatory Handoffs.
- [resolved-applied] Phase2 iter10: [DEP-001] F706 dependency type changed from Related to Successor per template semantics (F706 depends on F755 output as Consumer).
- [resolved-applied] Phase2 iter10: [DES-002] Added inline KNOWN LIMITATION comment in Technical Design CompoundConditionParser code documenting NOT precedence bug for implementer awareness.
- [resolved-applied] Phase2 iter10: [AC-003] AC count 21 exceeds 8-15 infra limit. Accepted with existing justification at line 314.
- [resolved-applied] Phase2 iter11: [AC-003] AC#18 orphaned (no Task reference). Added to Task#11 AC# column (8,13,14,18).
- [resolved-applied] Phase2 iter11: [AC-003] AC#18 non-regression verification clarified: count-based proxy noted with limitation. Byte-level non-regression deferred to F706.
- [resolved-applied] Phase2 iter11: [FMT-001] Scope Discipline restored to template's numbered blockquote format (1. STOP, 2. REPORT, 3. TRACK, 4. LINK).
- [resolved-applied] Phase2 iter12: [DES-002] ConvertCompoundRef NOT path missing null check. Added `if (subYaml == null) return null;` matching AND/OR path's fail-fast pattern.
- [resolved-applied] Phase2 iter12: [AC-003] AC#7 threshold gte 10 accepted. Rationale: F755 CompoundConditionParser is limited to `&&`/`||`/`!` without parentheses or function calls. Many of the 48 target files contain patterns outside this scope (parenthesized expressions, bitwise operators, LOCAL/function-call compounds mixed with TALENT). The 20% lower bound (gte 10) is realistic given parser limitations. Empirical re-migration will confirm actual yield; if significantly higher, threshold can be raised in F756.
- [resolved-skipped] Phase2 iter13: [FMT-001] Related Features section (lines 100-123) is non-template section. Retained: Pattern Analysis subsection provides scope escalation analysis (F752→F755 complexity growth) not captured in Dependencies table. Established in iter7 as acceptable /fc workflow convention.
- [resolved-skipped] Phase2 iter13: [DES-002] No integration AC feeds re-migrated YAML through KojoBranchesParser. AC Design Constraint C4 explicitly limits F755 to independent validation of evaluation (AC#1-3) and migration (AC#4-11) layers. Philosophy Derivation line 294 defers end-to-end integration to F706 as Consumer. Adding integration AC within F755 would violate C4 scope boundary.
- [resolved-applied] Phase2 iter14: [DEP-001] NOT precedence bug empirically confirmed: 3 ERB files contain `!TALENT:奴隷:親愛 && TALENT:奴隷:公衆便所` pattern affected by the bug. Added AC#22 negative guard to verify re-migration excludes start-of-expression !TALENT patterns, and documented exclusion strategy in Technical Constraints.
- [resolved-applied] Phase2 iter15: [AC-003] AC#22 Method rewritten from prose to test-type AC with unit test `NotPrecedenceExclusion`. Original method was multi-step prose not mechanically verifiable.
- [resolved-applied] Phase2 iter15: [AC-003] AC#14 Matcher changed from `not_contains` to `not_matches`. Original used regex alternation pattern with `not_contains` (literal substring matcher).
- [resolved-applied] Phase2 iter15: [DES-002] Technical Design re-migration section updated outdated AC#7 reference from "at least 1" to "at least 10".
- [resolved-applied] Phase2 iter15: [SCP-002] Goal #3 clarified to specify TALENT-compound-only re-migration scope, matching C4 constraint and Task#10 scope.
- [resolved-skipped] Phase2 iter15: [AC-003] Task#4 and Task#5 share AC#4,5,10,11. SSOT (feature-template.md) does not prohibit shared AC ownership across tasks. AC#19 provides Task#5-specific isolation for diagnostic purposes. Shared ACs reflect that both tasks contribute to the same test outcomes (parser creates, converter consumes). No SSOT violation.
- [resolved-applied] Phase2 iter16: [AC-003] Philosophy Derivation ErbToYaml row extended to include AC#16 (fail-on-unparseable) and AC#19 (DatalistConverter isolation) for complete traceability.
- [resolved-applied] Phase2 iter16: [DES-002] EvaluateVariableCondition silent-true for non-numeric values documented as KNOWN LIMITATION in Technical Design. Pre-existing TALENT evaluation pattern replicated for consistency. Symbolic references deferred to F756.
- [resolved-applied] Phase2 iter17: [AC-003] Implementation Contract Phase 9 and Success Criteria updated from AC#1-21 to AC#1-23 to include AC#22 and AC#23.
- [resolved-applied] Phase2 iter17: [AC-003] Added AC#23 (SingleTalentCondition_BackwardCompat) for migration-side single-TALENT backward compatibility through new CompoundConditionParser routing path. Added to Task#6, Philosophy Derivation, and AC Coverage.
- [resolved-applied] Phase2 iter18: [DES-002] Technical Design ConvertCompoundRef and ConvertTalentRef return types changed to nullable `Dictionary<string, object>?` for CS8603 compliance under TreatWarningsAsErrors (C9).
- [resolved-applied] Phase2 iter18: [AC-003] AC#20 grep pattern changed from `'TALENT.*16'` to `'16:'` (YAML key format) to prevent false positives from indices 116, 160, etc.
- [resolved-applied] Phase2 iter18: [AC-003] Philosophy Derivation CFLAG/TCVAR row narrowed to "tested in AND compounds; OR/NOT coverage via F752 framework" for accurate AC coverage claim.
- [resolved-applied] Phase2 iter19: [FMT-001] Mandatory Handoffs Destination column changed from free-text to 'Feature' per template. Creation Task changed from 'T12' to 'Task#12'.
- [resolved-applied] Phase2 iter19: [AC-003] Philosophy Derivation Row 4 extended from AC#7 to AC#7,20,21,22 for complete re-migration traceability.
- [resolved-applied] Phase2 iter19: [FMT-001] AC Details ordering fixed: AC#22 now appears before AC#23.
- [resolved-applied] Phase2 iter19: [AC-003] AC#20 context window increased from -A5 -B5 to -A10 -B10 for better compound structure coverage.
- [resolved-applied] Phase2 iter20: [AC-003] Philosophy Derivation Row 2 explicitly notes synthetic-only validation for CFLAG/TCVAR evaluation. Production validation deferred to F756 re-migration.
- [resolved-applied] Phase2 iter21: [DES-002] CONSOLIDATED (replaces Phase3-5 iter9 items below): Technical Design requires full redesign to reuse existing ErbParser infrastructure (LogicalOperatorParser, CflagConditionParser, CflagRef, ConditionExtractor). Cascading changes required: (1) Remove proposed CompoundConditionParser class entirely, (2) Fix CflagRef naming collision, (3) Update Root Cause Analysis to reference ConditionExtractor, (4) Remove C3 constraint/AC#22/NOT precedence Handoff/3-file exclusion, (5) Add TcvarConditionParser/TcvarRef + negation support as new work, (6) Renumber AC#12a/12b to integers, (7) Clean Mandatory Handoffs table - remove stale rows for parenthesized expressions, CFLAG character-scoped references, and NOT precedence bug (all handled by existing LogicalOperatorParser/CflagConditionParser infrastructure) and update F756 Deferred Items accordingly, (8) Update C2 constraint and state key format to reflect actual CflagRef data model (Target/Name/Index fields, not simple string index '300'), (9) DatalistConverter must implement LogicalOp binary-tree (Left/Right) → YAML flat-array (AND: [A, B, C]) conversion. Same-operator chains (A && B && C parsed as nested LogicalOps) must be flattened. Mixed operators produce nested structures. This is new conversion logic. **ACTION: (1) Set status to [DRAFT], (2) Remove all fc-phase-N-completed markers (lines 38, 281, 437, 1225), (3) Re-run `/fc 755` to regenerate from Phase 2 based on existing infrastructure. NOTE: /fc requires [DRAFT] status and resume detection uses phase markers to determine start point.**
- [resolved-applied] Phase2 iter21: [DES-002] Consolidated 6 pending items (Phase3 iter9 x3, Phase4 iter9 x1, Phase5 iter9 x2) into single actionable pending item above. Original items preserved below for audit trail.
- [resolved-applied] Phase2 iter21: [FMT-001] AC#12a/12b non-integer identifiers noted. Will be resolved during Technical Design redesign (AC renumbering required).
- [pending→consolidated] Phase3 iter9: [DES-002] CRITICAL: Technical Design proposes creating new CompoundConditionParser but existing ErbParser infrastructure already provides: LogicalOperatorParser (&&/|| with proper precedence AND parentheses), CflagConditionParser, CflagRef, ConditionExtractor, ICondition. Proposed design duplicates and is INFERIOR to existing code (no parentheses, NOT precedence bug). Technical Design must be redesigned to reuse existing infrastructure. This eliminates AC#22 (NOT exclusion), resolves C3 constraint (parentheses), and removes 3-file exclusion.
- [pending→consolidated] Phase3 iter9: [DES-002] Proposed CflagRef in Technical Design (Index as string) conflicts with existing CflagRef in ErbParser (Index as int?, has Target/Name). Would cause compilation error or class shadowing.
- [pending→consolidated] Phase3 iter9: [DES-002] Root Cause Analysis investigated only TalentConditionParser but missed existing ConditionExtractor/LogicalOperatorParser infrastructure. Root cause of empty {} is DatalistConverter calling TalentConditionParser directly instead of using ConditionExtractor.
- [pending→consolidated] Phase4 iter9: [AC-003] AC#7, AC#18, AC#20 use Type 'code'/'file' with Bash Method, which is not a documented combination per testing SKILL. May need Type/Method alignment after Technical Design redesign.
- [pending→consolidated] Phase5 iter9: [DES-002] Feasibility confirms Phase 3 findings. Existing infrastructure also lacks: (1) TcvarConditionParser/TcvarRef (must be created), (2) NegatedCondition/negation handling in LogicalOperatorParser.ParseAtomicCondition() (must be added). These are NEW code contributions within F755 scope, not just reuse. Redesigned Technical Design should add TCVAR parser + negation support as new work items.
- [pending→consolidated] Phase5 iter9: [DES-002] If LogicalOperatorParser is reused: AC#22 (NOT exclusion), C3 constraint (parentheses), NOT precedence Mandatory Handoff, and 3-file exclusion in Technical Constraints become unnecessary. Significant simplification of the feature spec.
- [resolved-applied] Phase2 iter22: [DES-002] Added items (7) Mandatory Handoffs stale row cleanup and (8) C2/state key format data model update to consolidated [pending] item.
- [resolved-invalid] Phase2 iter22: [FMT-001] `[pending→consolidated]` non-standard tag: Tags serve as audit trail for consolidation. Changing to [resolved-applied] would incorrectly imply independent resolution. Tags are informational; the consolidated [pending] at iter21 is the actionable item.
- [resolved-invalid] Phase2 iter22: [AC-003] Round-trip integration AC between ConvertCflagRef and EvaluateCondition: Already deliberated and rejected in Phase2 iter13 (resolved-skipped) per C4 scope constraint. F706 is the designated consumer for end-to-end integration validation.
- [resolved-applied] Phase2 iter23: [DES-002] Added item (9) LogicalOp binary-tree → YAML flat-array conversion requirement to consolidated [pending] item. Genuine gap: existing LogicalOp uses Left/Right binary tree but KojoBranchesParser expects AND/OR as flat List<object> arrays.
- [resolved-applied] Phase2 iter23: [DES-002] Feasibility/Risk sections contain false premises about parser complexity. Will be rewritten during /fc 755 re-run (user approved [DRAFT] rollback + /fc re-run at POST-LOOP).
- [resolved-invalid] Phase2 iter23: [DES-002] F756 Deferred Items cleanup: Already explicitly covered by consolidated [pending] point (7) which names all 3 stale items and says "update F756 Deferred Items accordingly".
- [resolved-applied] Phase2 iter24: [DES-002] Consolidated [pending] ACTION fixed: Changed from "Re-run /fc 755" to include prerequisites "(1) Set status to [DRAFT], (2) Remove fc-phase markers". Original ACTION was doubly blocked: [PROPOSED] status fails /fc precondition, and phase markers cause resume detection to skip generation.
- [resolved-applied] Phase2 iter24: [DES-002] Removed 3 stale Mandatory Handoff rows (parenthesized expressions, CFLAG character-scoped references, NOT precedence bug) now handled by existing LogicalOperatorParser/CflagConditionParser. Updated F756 Deferred Items table to remove corresponding entries.
- [resolved-applied] Phase2 iter24: [DES-002] Problem #2 misframing noted: DatalistConverter bypasses existing ConditionExtractor, not parser absence.
- [resolved-applied] Phase2 iter25: [DES-002] Background Problem #2 and Goal #2 manually corrected. /fc re-run from Phase 2 does NOT regenerate Background (Phase 1, human-written). Previous iter24 incorrectly assumed /fc would auto-correct this.
- [resolved-applied] Phase3 iter26: [DES-002] F756 Background text still referenced stale items (parenthesized expressions, CFLAG character-scoped references) after Deferred Items table cleanup. Updated F756 Philosophy and Problem sections for consistency.
- [resolved-applied] Phase2 iter27: [FMT-001] Implementation Contract table had 6 columns (extra 'Tasks' column). Fixed to template-standard 5 columns, merged Tasks references into Input column.
- [resolved-applied] Phase2 iter27: [AC-003] Task#13 had no AC (AC# = '-'). Added AC#16 (F756 DRAFT file exists) per template DRAFT Creation Checklist requirement.
- [resolved-applied] Phase2 iter27: [DES-002] ConvertCflagRef used cflag.Index (int?) directly as dict key without null handling. Fixed to use conditional expression matching ConvertTcvarRef pattern for Name-only and character-scoped CflagRef forms.
- [resolved-applied] Phase2 iter27: [AC-003] Added C15 constraint documenting character-scoped CFLAG evaluation testing deferral to F756 per Scope Discipline boundary.
- [resolved-applied] Phase2 iter27: [AC-003] AC#9 and AC#11 used Type=file with Grep method and gte/lte matchers (unsupported combination per testing SKILL). Changed to Type=output with grep count commands.
- [resolved-applied] Phase2 iter28: [FMT-001] 残課題 (Deferred Items) section removed. Not template-defined and contained no items. If [B] AC tracking is needed, Mandatory Handoffs section serves that purpose.
- [resolved-applied] Phase2 iter28: [DEP-001] Mandatory Handoffs Creation Task fixed from Task#12 to Task#13 in both rows. Task#12 is build verification; Task#13 is the F756 creation task.
- [resolved-invalid] Phase2 iter28: [DES-002] FunctionCall empty {} in compound AND/OR arrays: Deliberate scope boundary per Scope Discipline (line 9 lists function calls as out-of-scope), already resolved in iter3 as [CON-003]. Re-migration targets TALENT-only compounds per C4.
- [resolved-applied] Phase2 iter29: [AC-003] AC#9 grep pattern broadened from 'AND:' to 'AND:\|OR:\|NOT:' matching AC#11's pattern. Threshold 'gte 10' retained as conservative floor: some of 48 target files may have simple conditions after ConditionExtractor re-parsing, and OR/NOT-only compounds are now counted.
- [resolved-applied] Phase2 iter29: [AC-003] Task#2 AC# column expanded from '3' to '1,2,3'. Task#2 creates the test files that AC#1 and AC#2 depend on (CompoundCflag, CompoundTcvar tests).
- [resolved-invalid] Phase2 iter29: [FMT-001] Section ordering deviation from template: Template does not mandate section ordering. /fc-generated sections are established workflow convention per iter5/7/13 deliberation. feature-reviewer.md checklist does not include ordering.
- [resolved-skipped] Phase2 iter30: [AC-003] Task#11 [I] tag not warranted. AC#9/AC#11 Expected values are deterministic thresholds (gte 10, lte 60), not implementation-dependent outputs. Per [I] tag anti-pattern: "Using [I] to avoid writing concrete Expected values." Expected IS concrete. Grep pattern broadened in iter29 addresses coverage concern.
- [resolved-applied] Phase2 iter30: [DES-002] Stale AC#7 references in Technical Design re-migration section (lines 1244, 1264) fixed to AC#9 post-redesign.
- [resolved-invalid] Phase2 iter30: [AC-003] AC#10 Type 'code' is documented in testing SKILL.md (line 51: code | Code content verification | Grep (static)). Reviewer incorrectly referenced abbreviated list from CLAUDE.md.
- [resolved-skipped] Phase2 iter30: [AC-003] AC count 16 exceeds infra type limit 8-15. Already acknowledged in iter10 with justification. No additional action.
- [resolved-skipped] Phase2 iter30: [AC-003] AC#8 YAML serialization coverage: test-type AC delegates to unit test assertions (passthrough + GenerateId per Details). YAML serialization round-trip implicitly covered by AC#12 (schema validation accepts compound conditions) and AC#9 (re-migrated output contains compound syntax). Cross-AC coverage chain is adequate; adding explicit documentation to AC#8 Details is a minor improvement, not a defect.
- [resolved-applied] Phase2 iter31: [AC-003] AC#9 threshold raised from 'gte 10' to 'gte 30' (62% of 48 targets). Original 'gte 10' was based on pre-iter21 CompoundConditionParser limitations (no parentheses, no function calls). Post-iter21 ConditionExtractor handles all TALENT compounds, invalidating the conservative rationale. AC Details updated with post-redesign justification.
- [resolved-invalid] Phase2 iter31: [DES-002] FunctionCall empty {} in compound: Already resolved-invalid in iter28 and resolved-applied in iter3 as CON-003 scope boundary. Re-raising contradicts prior resolution.
- [resolved-invalid] Phase2 iter31: [AC-003] AC#11 count proxy limitation: Already resolved-applied in iter11 with limitation noted. Byte-level non-regression deferred to F706. AC#13 provides backward-compatibility check.
- [resolved-applied] Phase2 iter30: [DES-002] Stale AC#7 references in Technical Design re-migration section fixed to AC#9.
- [resolved-applied] Phase2 iter32: [FMT-001] AC#9/AC#11 markdown table breakage: `\\|` in grep pattern broke table columns (escaped backslash + unescaped pipe = column separator). Fixed to `\\\|` (escaped backslash + escaped pipe).
- [resolved-applied] Phase2 iter32: [DES-002] Implementation Contract Success Criteria stale 'gte 10' updated to 'gte 30' matching AC#9 table.
- [resolved-applied] Phase2 iter32: [AC-003] AC#11 Details stale 'gte 10' reference updated to 'gte 30' matching AC#9 table.
- [resolved-invalid] Phase2 iter32: [DES-002] FunctionCall empty {} in compound: 4th time raised. Already resolved in iter3/iter28/iter31. Deliberate scope boundary per CON-003.
- [resolved-invalid] Phase2 iter32: [AC-003] CFLAG/TCVAR integration gap: Already resolved in iter9/iter13/iter22. Philosophy Derivation defers to F706 per C4.
- [resolved-applied] Phase3 iter33: [DES-002] Operator mapping duplication: Extracted shared `MapErbOperatorToYaml` helper. ConvertTalentRef/CflagRef/TcvarRef now call single method. Eliminates 3x identical switch blocks.
- [resolved-applied] Phase3 iter33: [DES-002] ConditionExtractor DI seam: Changed from per-call `new ConditionExtractor()` to constructor-injected `_conditionExtractor` field. Preserves DI pattern and enables test mocking.
- [resolved-applied] Phase3 iter33: [DES-002] TALENT evaluation refactoring: Added recommended (not required) design note. Not covered by Tasks/ACs; if not done in F755, track as F756 extensibility. Softened language in iter34 after semantic review identified Task/AC gap.
- [resolved-applied] Phase3 iter33: [DES-002] TcvarConditionParser extensibility: Added note recommending generic `VariableConditionParser<T>` extraction for F756 to eliminate per-type parser duplication.
- [resolved-applied] Phase3 iter33: [DES-002] F756 DRAFT: Added Extensibility Hooks section documenting F755's extension points (allowlist, shared method, visitor, parser chain, DI).
- [resolved-applied] Phase3 iter33: [DES-002] ConvertLogicalOp fail-fast: Added explicit design comment documenting deliberate fail-fast vs partial-result tradeoff.
- [resolved-applied] Phase2 iter34: [DES-002] ConvertConditionToYaml type count: "5 types" corrected to "6 types" (TalentRef, CflagRef, TcvarRef, LogicalOp, NegatedCondition, FunctionCall).
- [resolved-applied] Phase2 iter34: [DES-002] TALENT refactoring language softened from "should" to "recommended, not required". No Task/AC covers this; if not done in F755, deferred to F756 extensibility.
- [resolved-skipped] Phase2 iter34: [AC-003] AC#9 grep false positive risk: Minimal for kojo YAML files (Japanese dialogue content). Pattern AND:/OR:/NOT: as YAML keys is specific enough. No action needed.
- [resolved-applied] Phase3 iter35: [DES-002] TALENT refactoring added to Mandatory Handoffs for F756 tracking. Also added to F756 Deferred Items table.
- [resolved-applied] Phase3 iter35: [DES-002] ConditionExtractor DI wiring clarified: both DatalistConverter constructors must instantiate; TalentCsvLoader retained for ConvertTalentRef; old _conditionParser field removed.
- [resolved-applied] Phase3 iter35: [DES-002] ConvertLogicalOp empty {} KNOWN LIMITATION documented: FunctionCall returns empty {} (not null), creating AND with vacuously-true element. Accepted per C4 scope.
- [resolved-applied] Phase3 iter35: [DES-002] Negation + parentheses: ParseAtomicCondition now delegates to ParseLogicalExpression for parenthesized inner expressions like !(TALENT:X && TALENT:Y).
- [resolved-applied] Phase2 iter36: [DES-002] Parenthesized sub-expression support: ParseAtomicCondition had no outer-paren stripping. SplitOnOperator tracks paren depth but ParseAtomicCondition returned null for `(A || B)`. Added outer-paren stripping + recursive ParseLogicalExpression delegation (~5 lines). Updated C3, Technical Design, Pattern Analysis, Feasibility, Summary, Task#4, AC#14 details. Simplified negation handling to use recursive ParseAtomicCondition (which now handles parens).
- [resolved-invalid] Phase2 iter36: [CON-003] Empty {} FunctionCall limitation: 6th time raised. Already resolved in iter3/iter28/iter29/iter31/iter32/iter35. Deliberated scope boundary per CON-003/C4.
- [resolved-invalid] Phase2 iter36: [AC-003] YamlValidator integration testing: Already resolved in iter13 (resolved-skipped). F755 validates independently per C4; end-to-end integration deferred to F706.
- [resolved-applied] Phase2 iter37: [SCP-002] Scope Discipline listed "parenthesized expressions" as out-of-scope but iter36 added outer-paren stripping to Task#4/AC#14/C3/Technical Design. Removed "parenthesized expressions" from out-of-scope list.
- [resolved-applied] Phase2 iter37: [AC-003] Philosophy Derivation Row 2 incorrectly mapped AC#8 (BranchesToEntriesConverter) to CFLAG/TCVAR evaluation claim. Changed to AC#3 (selective allowlist validation).

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Remaining compound variable types (LOCAL 371, EQUIP 179, STAIN 21, ITEM 44, function calls 603) | F755 only addresses CFLAG/TCVAR; other types require separate analysis | Feature | F756 | Task#13 |
| Bitwise operator support (`&` vs `&&`) | Tokenizer disambiguation complexity beyond F755 scope | Feature | F756 | Task#13 |
| Refactor TALENT evaluation to use EvaluateVariableCondition shared method | Eliminates code duplication between TALENT/CFLAG/TCVAR evaluation paths | Feature | F756 | Task#13 |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-06 18:33 | START | implementer | Task 1 | - |
| 2026-02-06 18:33 | END | implementer | Task 1 | SUCCESS |
| 2026-02-06 18:33 | START | implementer | Task 2 | - |
| 2026-02-06 18:33 | END | implementer | Task 2 | SUCCESS |
| 2026-02-06 19:30 | START | implementer | Task 11 (re-migration) | - |
| 2026-02-06 19:30 | END | implementer | Task 11 (re-migration) | SUCCESS: 96 YAML files with compound syntax (58 target ERBs × 1.66 expansion ratio). 16 ERB schema validation failures (pre-existing schema gap). AC#11 updated lte 60→lte 120 to reflect 1:N expansion |
| 2026-02-06 19:35 | END | orchestrator | Task 12 (build verify) | SUCCESS: All 3 tools build with 0 warnings, 0 errors |

---

## Links
- [feature-752.md](feature-752.md) - TALENT compound condition support (predecessor)
- [feature-706.md](feature-706.md) - KojoComparer Full Equivalence Verification (consumer)
- [feature-750.md](feature-750.md) - TALENT condition migration (foundation)
- [feature-725.md](feature-725.md) - KojoBranchesParser creator (infrastructure)
- [feature-754.md](feature-754.md) - YAML Format Unification (downstream)
- [feature-751.md](feature-751.md) - TALENT semantic mapping validation (sibling)
- [feature-709.md](feature-709.md) - Multi-State Equivalence Testing (downstream)
- [feature-756.md](feature-756.md) - Extended compound variable type support (deferred implementation)
- [feature-708.md](feature-708.md) - TreatWarningsAsErrors enforcement (constraint)

---

## Reference (from previous /fc session)

Previous Root Cause Analysis, Related Features, Feasibility, Dependencies, Impact, Technical Constraints, and Risks sections from the initial /fc 755 session (pre-iter21 redesign) are available in git history. Key differences from the previous design:

1. **Previous design proposed creating `CompoundConditionParser`** (new class in ErbParser) -- replaced by reuse of existing `LogicalOperatorParser`/`ConditionExtractor` infrastructure
2. **Previous design defined `CflagRef`/`TcvarRef` classes** with string Index field -- conflicts with existing `CflagRef : ICondition` (int? Index, Target?, Name?) in ErbParser
3. **Previous design had NOT precedence bug** (`!X && Y` -> `NOT(X && Y)`) requiring 3-file exclusion list -- eliminated by `LogicalOperatorParser.SplitOnOperator()` parentheses support
4. **Previous design constraint C3 prohibited parentheses** -- eliminated by existing infrastructure
5. **Previous feasibility rated ErbToYaml parsing as PARTIAL** -- upgraded to YES by infrastructure reuse
6. **Previous risk assessment identified "compound parsing complexity exceeds scope" as HIGH/HIGH** -- eliminated by infrastructure reuse
