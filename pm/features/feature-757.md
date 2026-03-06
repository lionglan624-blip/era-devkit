# Feature 757: Runtime Condition Support (Bitwise, Function Evaluation, Generic Consolidation)

## Status: [DONE]
<!-- fl-reviewed: 2026-02-07T00:00:00Z -->

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
> **Out of Scope**:
> - LOCAL variables (787 occurrences) — uses runtime assignment pattern, not condition prefix (Constraint C4); tracked as handoff to F758
> - MARK/EXP (327 occurrences) and bare variable comparisons (87 occurrences) (Constraint C9); tracked as handoff to F758
> - Compound bitwise-comparison expressions `(VAR & mask) == value` (1 occurrence in KOJO_KU_愛撫.ERB:63) — requires two-stage parser; tracked as handoff to F758
> - TALENT bitwise with target/numeric index patterns `TALENT:PLAYER & 2`, `TALENT:2 & 2` (25 of 26 occurrences) — ConvertTalentRef uses GetTalentIndex(Name) which returns null for "PLAYER" and numeric indices. Pre-existing TalentRef limitation; tracked as handoff to F758
>
> Runtime-only condition constructs deferred from F755→F756. Requires fundamentally different architecture from static condition parsing.

## Type: engine

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)
Advance toward full equivalence testing (F706: 650/650 MATCH) by resolving the four remaining parser architectural gaps: bitwise `&` operator support (STAIN, CFLAG, TALENT), nested function call parsing, FUNCTION runtime evaluation, and parser/Ref code duplication consolidation. F756 handles EQUIP/ITEM (comparison-operator patterns). F757 addresses bitwise, function-call, and STAIN variable type support, plus generic base class extraction for the 5x accumulated parser clones. Mid-term: complete compound condition support for all ERB variable types spans F755→F756→F757→F758 (LOCAL, MARK/EXP, bare variables tracked as handoff to F758).

### Problem (Current Issue)
The condition parsing pipeline has four architectural gaps that collectively prevent correct parsing of ~500+ kojo conditions:

1. **Bitwise `&` tokenizer gap** (78+ occurrences across STAIN/CFLAG/TALENT) — `SplitOnOperator` in `LogicalOperatorParser.cs:226-228` correctly handles `&&` (logical AND) but passes single `&` through unsplit to atomic parsers. Because no parser's regex includes `&` in its operator list (e.g., `TalentConditionParser.cs:17-18` only accepts `!=|==|>=|<=|>|<`), the `& value` suffix is **silently dropped**. This causes data corruption: `TALENT:性別嗜好 & 1` is parsed as `TalentRef(Name="性別嗜好")` with the bitwise mask `& 1` lost entirely. Root cause: the parser architecture has no concept of bitwise operators — no `BitwiseOp` ICondition type exists (`ICondition.cs:9-16`), `DatalistConverter.cs:257-278` maps only comparison operators, and all 33 STAIN conditions (which exclusively use `&`) are completely unparseable.

2. **Nested function call rejection** (221 FIRSTTIME(TOSTR(...)) patterns) — `FunctionCallParser.cs:18-20` uses regex `^([A-Z_][A-Z0-9_]*)\(([^()]*)\)$` where `[^()]*` explicitly rejects parentheses inside arguments. This means the most common function call pattern — `FIRSTTIME(TOSTR(350), 1)` — returns null. Root cause: ERB function calls have recursive structure (functions as arguments), but the parser uses a flat regex that cannot handle recursion.

3. **FUNCTION key blocked at runtime boundary** (412 occurrences) — F756 correctly generates `{"FUNCTION": {...}}` YAML in `DatalistConverter.cs:243-251`, but `KojoBranchesParser.cs:278` allowlist `{"TALENT","CFLAG","TCVAR","EQUIP","ITEM","AND","OR","NOT"}` omits `"FUNCTION"`, and `KojoBranchesParser.cs:176-179` explicitly throws `InvalidOperationException`. These are deliberate fail-fast boundaries from F756 that must be resolved for runtime evaluation.

4. **5x parser/Ref/converter code duplication** — After F756, CflagConditionParser, TcvarConditionParser, EquipConditionParser, ItemConditionParser (and forthcoming StainConditionParser) are near-identical clones differing only in regex prefix and class name. The 4 Ref types (`CflagRef.cs`, `TcvarRef.cs`, `EquipRef.cs`, `ItemRef.cs`) are structurally identical (all have `Target?, Name?, Index?, Operator?, Value?`). F755/F756 chose "Option A: clone per type" (`feature-756.md` Key Decisions) explicitly deferring extraction of `VariableConditionParser<TRef>` generic base class. Adding STAIN will create a 6th clone.

Additionally discovered: MARK/EXP (327 occurrences across 55 files) and bare variable comparisons like `PLAYER != MASTER` (87 occurrences across 7 files) are also unparsed, but are out of F757 scope.

### Goal (What to Achieve)
Resolve the four architectural gaps to enable correct parsing of bitwise `&` conditions, nested function calls, and FUNCTION runtime evaluation, while eliminating the accumulated code duplication:

1. **Bitwise `&` operator support**: Add `BitwiseOp` ICondition type and integrate `&` recognition into the tokenizer/parser pipeline so `STAIN:奴隷:Ｖ & 汚れ_精液`, `CFLAG:奴隷:前回売春フラグ & 前回売春_初売春`, and `TALENT:性別嗜好 & 1` are correctly parsed, converted to YAML, and evaluated.
2. **Nested function call parsing**: Replace the flat regex in FunctionCallParser with balanced-parenthesis matching to support `FIRSTTIME(TOSTR(...))` and similar nested patterns (221+ occurrences).
3. **FUNCTION runtime evaluation**: Add `"FUNCTION"` to ValidateConditionScope allowlist and implement evaluation logic in KojoBranchesParser for function conditions against game state.
4. **STAIN parser**: Add StainConditionParser and StainRef, leveraging the new bitwise `&` support (all 33 STAIN conditions use bitwise AND exclusively).
5. **Generic base class extraction**: Extract `VariableConditionParser<TRef>` and `VariableRef` base types to consolidate the 5+ parser/Ref clones into a single generic implementation. Note: DatalistConverter's `Convert*Ref` methods (10-15 lines each) are excluded from consolidation. However, 4 of 6 converters (CFLAG, EQUIP, ITEM, STAIN) share identical key construction logic: `Index.HasValue ? index : (target != null ? target:name : name)`. Only ConvertTalentRef (GetTalentIndex for numeric index lookup) and ConvertTcvarRef (no target prefix) differ. This 4-way duplication is deferred to F758 (see Mandatory Handoffs).

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

1. **Why** can ~500+ kojo conditions not be correctly parsed/evaluated?
   Because the condition parsing pipeline has 4 architectural gaps: no bitwise `&` support, flat regex for function calls, FUNCTION key blocked at runtime boundary, and 5x duplicated parser code that makes adding new types error-prone.

2. **Why** does the pipeline have no bitwise `&` support?
   Because `SplitOnOperator` (LogicalOperatorParser.cs:226-228) was designed only for `&&` (2-char) and `||` (2-char) logical operators. When it encounters a single `&`, the substring match `condition.Substring(i, 2) == "&&"` fails, so `&` passes through unsplit into the atomic condition string. The atomic parsers' regex operator group `(!=|==|>=|<=|>|<)` has no `&` entry, so `& value` is silently dropped.

3. **Why** was single `&` not handled when `&&` was implemented?
   Because the original LogicalOperatorParser (F752) was designed for logical operators only. Bitwise operators are a fundamentally different semantic category (value masking vs boolean logic). The architecture had no `BitwiseOp` ICondition type and no concept of operator precedence between bitwise `&` and logical `&&`. Adding `&` as a split operator would break `&&` splitting unless handled as a distinct precedence level.

4. **Why** does adding bitwise `&` require architectural changes rather than just regex extension?
   Because `&` appears WITHIN the right-hand side of variable conditions (e.g., `STAIN:口 & 汚れ_精液`), not BETWEEN two independent conditions like `&&`. The current parser pipeline assumes: `SplitOnOperator` → atomic parser → regex extracts operator. But bitwise `&` is an OPERATOR in the variable condition, not a SEPARATOR between conditions. This requires a new ICondition type (`BitwiseCondition`) and extension of the variable parser regex to recognize `&` alongside `!=|==|>=|<=|>|<`.

5. **Why** was the clone-per-type strategy chosen despite creating duplication?
   Because F755/F756 prioritized delivery speed over architectural cleanliness. Each new variable type (CFLAG, TCVAR, EQUIP, ITEM) was cloned from the previous, with explicit deferral of generic extraction to F757 (feature-756.md Key Decisions: "Option A: clone per type"). This was a conscious debt-accumulation decision that now requires paydown before adding STAIN (which would create a 6th clone).

### Symptom vs Root Cause

| Symptom (現象) | Root Cause (根本原因) |
|----------------|----------------------|
| STAIN conditions unparseable (32 in kojo) | No `&` operator in parser regex; no BitwiseOp ICondition type |
| CFLAG/TALENT bitwise conditions lose `& value` (62 in kojo) | SplitOnOperator passes single `&` through unsplit to atomic parsers that silently drop it |
| FIRSTTIME(TOSTR(...)) returns null (221 in kojo) | FunctionCallParser regex `[^()]*` explicitly rejects parentheses in arguments |
| FUNCTION YAML causes InvalidOperationException at runtime | F756 deliberately added fail-fast boundary (KojoBranchesParser.cs:176-179) for deferred F757 work |
| 5 near-identical parser files (390+ duplicated lines) | F755/F756 chose clone-per-type over generic base class extraction |

### Conclusion

The root cause is a **multi-layered architectural gap** in the condition parsing pipeline:
1. The tokenizer (`SplitOnOperator`) has no concept of bitwise operators as a separate precedence level between logical `&&`/`||` and comparison operators
2. The atomic parsers have no mechanism to handle operators that differ per variable type (comparison for TALENT/CFLAG, bitwise for STAIN)
3. The function call parser uses a flat regex when ERB function calls are inherently recursive
4. The runtime evaluator has deliberate fail-fast boundaries that must be replaced with actual evaluation logic

These are not independent bugs but consequences of the parser being designed for the simple case (single-variable comparison with logical connectors) and now needing to handle the full ERB expression grammar.

## Related Features

| Feature | Status | Relationship | Note |
|---------|--------|--------------|------|
| F755 | [DONE] | Predecessor | CFLAG/TCVAR compound support. Chose "clone per type" strategy. Source of duplication. |
| F756 | [DONE] | Predecessor | EQUIP/ITEM parsers + FunctionCall YAML passthrough. Added fail-fast FUNCTION boundary. |
| F706 | [PROPOSED] | Consumer | Full equivalence verification (650/650 MATCH). Depends on F757 for complete condition parsing. |
| F752 | [DONE] | Foundation | Compound TALENT condition support. Created LogicalOperatorParser with SplitOnOperator. |
| F750 | [DONE] | Foundation | YAML TALENT condition migration. Established the TalentRef/parser/converter pattern. |
| F543 | [DONE] | Related | IConditionEvaluator interface extraction. May provide patterns for evaluation. |
| F754 | [DRAFT] | Related | YAML format unification (branches to entries). May affect YAML output format. |

### Pattern Analysis

**Recurring Pattern**: F750 → F752 → F755 → F756 → F757 represents an incremental widening of condition type support. Each feature adds 1-2 variable types by cloning from the previous. The duplication was explicitly deferred each time. F757 is the designated "paydown" feature where the technical debt must be resolved before the pattern continues to MARK/EXP/etc.

**Risk**: If F757 does NOT extract the generic base class, future variable type additions (MARK: 327 occurrences, EXP: unknown) will continue cloning and the debt will compound further.

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | YES | All 4 gaps have well-understood solutions: regex extension for `&`, balanced-paren matching for nested calls, allowlist update for FUNCTION, generics for dedup |
| Scope is realistic | PARTIAL | 4 sub-problems in one feature is ambitious (~300 line engine limit). May need scope reduction. |
| No blocking constraints | YES | Both predecessors (F755, F756) are [DONE]. No external dependencies. |

**Verdict**: FEASIBLE (with scope notes)

**Scope Notes**:
1. **LOCAL variables should be OUT OF SCOPE**: LOCAL variables (787 occurrences) use a fundamentally different pattern: runtime assignment (`LOCAL:1 = 1`) then conditional check (`IF LOCAL:1`). They are NOT a condition prefix type like TALENT/CFLAG/STAIN. Parsing LOCAL requires tracking assignments in preceding code, which is a stateful analysis problem completely different from the stateless condition parsing done by the current pipeline. LOCAL should be handled as opaque passthrough or deferred to a separate feature.

2. **Feature title suggests LOCAL is in scope but investigation shows it should not be**: The title says "Runtime Condition Support (LOCAL, Bitwise, Function Evaluation)" but LOCAL requires fundamentally different architecture. Recommendation: either remove LOCAL from the title or explicitly declare it out of scope in the Background.

3. **Volume concern**: 4 sub-problems (bitwise, nested function, FUNCTION eval, generic dedup) could each be ~100-200 lines of implementation. The ENGINE type guide recommends ~300 lines per feature. Consider whether generic dedup (sub-problem 4) should be split to a separate feature. However, the dedup is needed BEFORE adding StainConditionParser to avoid creating a 6th clone, so it has ordering dependency.

## Impact Analysis

| File | Change Type | Description |
|------|-------------|-------------|
| tools/ErbParser/ICondition.cs | Update | Add BitwiseCondition (or extend existing Ref types) JsonDerivedType |
| tools/ErbParser/LogicalOperatorParser.cs | Update | Handle `&` in SplitOnOperator or ParseAtomicCondition; register new parsers |
| tools/ErbParser/FunctionCallParser.cs | Rewrite | Replace flat regex with balanced-parenthesis matching |
| tools/ErbParser/CflagConditionParser.cs | Rewrite/Delete | Extract to generic VariableConditionParser<TRef> or extend regex for `&` |
| tools/ErbParser/TcvarConditionParser.cs | Rewrite/Delete | Consolidated into generic base class |
| tools/ErbParser/EquipConditionParser.cs | Rewrite/Delete | Consolidated into generic base class |
| tools/ErbParser/ItemConditionParser.cs | Rewrite/Delete | Consolidated into generic base class |
| tools/ErbParser/TalentConditionParser.cs | Update | May be consolidated (differs: Target=string.Empty not null, no Index) |
| tools/ErbParser/CflagRef.cs | Update/Delete | Consolidated into VariableRef base or kept with inheritance |
| tools/ErbParser/TcvarRef.cs | Update/Delete | Consolidated into VariableRef base |
| tools/ErbParser/EquipRef.cs | Update/Delete | Consolidated into VariableRef base |
| tools/ErbParser/ItemRef.cs | Update/Delete | Consolidated into VariableRef base |
| tools/ErbParser/StainRef.cs | Create | New (unless generic covers STAIN) |
| tools/ErbToYaml/DatalistConverter.cs | Update | Handle new ICondition types in ConvertConditionToYaml, MapErbOperatorToYaml for `&` |
| tools/KojoComparer/KojoBranchesParser.cs | Update | Add FUNCTION/STAIN to ValidateConditionScope allowlist, implement FUNCTION evaluation, bitwise state evaluation |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| `&&` vs `&` disambiguation in tokenizer | LogicalOperatorParser.cs SplitOnOperator | HIGH - Must not break existing `&&` splitting when adding `&` handling |
| TalentRef has different structure (non-nullable Target/Name, no Index) | TalentRef.cs vs CflagRef.cs | MEDIUM - Generic VariableRef base must accommodate TalentRef's structural differences |
| FunctionCall args may contain nested parentheses AND commas | ERB: FIRSTTIME(TOSTR(TFLAG:50 + 500)) | HIGH - Balanced-paren matching must also handle comma splitting correctly within nested calls |
| FUNCTION evaluation requires game state beyond current Dict<string, int> | KojoBranchesParser.cs:31 state parameter | HIGH - Functions like FIRSTTIME need access to persistent state, HAS_VAGINA needs body data |
| Existing 103 ErbParser tests + 89 ErbToYaml tests must not regress | Baseline | HIGH - Generic extraction must preserve all existing behavior |
| KojoComparer.Tests has 2 pre-existing failures (ErbRunner state injection) | Baseline | LOW - These are unrelated to F757 scope |
| LOCAL variables use runtime assignment pattern, not condition prefix | ERB: LOCAL:1 = 1 then IF LOCAL:1 | HIGH - LOCAL is fundamentally different from TALENT/CFLAG/STAIN patterns |
| Bitwise `&` appears WITHIN variable conditions, not between them | ERB: STAIN:口 & 汚れ_精液 | HIGH - `&` is an operator within the variable expression, not a separator between two conditions |
| KojoBranchesParser existing parameterless constructor is used via `new()` (no reflection) | KojoBranchesParser.cs callers, test files | LOW - Verify call sites before adding optional parameter; standard `new()` calls unaffected |
| ERB bitwise `&` patterns require surrounding spaces for correct parsing | Parser regex design: `[^:\s]+` name group stops at whitespace | MEDIUM - Implementation must verify ERB corpus has no spaceless `&` patterns; if found, name group needs `[^:\s&]+` |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Generic extraction breaks existing serialization (JsonDerivedType discriminators) | Medium | High | Maintain backward-compatible discriminator strings; test JSON round-trip |
| `&&` vs `&` disambiguation introduces subtle tokenizer bugs | Medium | High | Extensive test coverage for edge cases: `A && B & C`, `A & B && C & D` |
| FUNCTION evaluation scope too broad (412 ERB occurrences, many function types) | High | Medium | Limit to opaque passthrough + configurable evaluator (not implement all functions) |
| Volume exceeds ~300 line limit for engine features | High | Low | Split generic dedup to separate feature if needed; document volume waiver |
| TalentRef structural differences make generic extraction complex | Medium | Medium | Accept TalentRef as special case with adapter or separate subclass |

## Baseline Measurement

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ErbParser build | dotnet build tools/ErbParser/ErbParser.csproj | 0 warnings, 0 errors | Clean build |
| ErbToYaml build | dotnet build tools/ErbToYaml/ErbToYaml.csproj | 0 warnings, 0 errors | Clean build |
| KojoComparer build | dotnet build tools/KojoComparer/KojoComparer.csproj | 0 warnings, 0 errors | Clean build |
| ErbParser tests | dotnet test tools/ErbParser.Tests | 103/103 passed | All passing |
| ErbToYaml tests | dotnet test tools/ErbToYaml.Tests | 89/89 passed | All passing |
| KojoComparer tests | dotnet test tools/KojoComparer.Tests | 95 passed, 1 failed, 3 skipped | 1 failure pre-existing (FileDiscoveryTests) |
| STAIN & in kojo | grep STAIN:.*& Game/ERB/口上 | 32 occurrences | All use bitwise & |
| CFLAG single & in kojo | grep CFLAG:.*[^&]&[^&] Game/ERB/口上 | 36 occurrences | Bitwise, not logical && |
| TALENT single & in kojo | grep TALENT:.*[^&]&[^&] Game/ERB/口上 | 26 occurrences | Bitwise, not logical && |
| FIRSTTIME(TOSTR) in kojo | grep FIRSTTIME(TOSTR Game/ERB/口上 | 221 occurrences | Nested function calls |
| FUNCTION in kojo ERBs | grep FUNCTION Game/ERB/口上 *.ERB | 26 occurrences | Runtime function refs |
| Parser clones | ls tools/ErbParser/*ConditionParser.cs | 5 files (Talent, Cflag, Tcvar, Equip, Item) | Near-identical |
| Ref clones | ls tools/ErbParser/*Ref.cs | 5 files (TalentRef, CflagRef, TcvarRef, EquipRef, ItemRef) | 4 identical, TalentRef differs |
| ICondition types | grep JsonDerivedType tools/ErbParser/ICondition.cs | 8 registered types | Current polymorphic types |
| ValidateConditionScope keys | grep allowedKeys tools/KojoComparer/KojoBranchesParser.cs | 8 keys | TALENT,CFLAG,TCVAR,EQUIP,ITEM,AND,OR,NOT |

**Baseline File**: `.tmp/baseline-757.txt`

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | `&&` vs `&` disambiguation must not break existing tests | LogicalOperatorParser.cs:226-228, 103 ErbParser tests | ACs must include regression test for existing `&&` behavior AND new `&` behavior |
| C2 | TalentRef has non-nullable Target/Name and no Index property | TalentRef.cs vs CflagRef.cs structural difference | Generic VariableRef base class AC must account for TalentRef differences or exclude TalentRef from consolidation |
| C3 | FUNCTION evaluation cannot be fully implemented (too many function types) | 412 ERB occurrences, functions like FIRSTTIME need persistent state | AC for FUNCTION should test passthrough/configurable evaluation, not all function implementations |
| C4 | LOCAL variables are OUT OF SCOPE | LOCAL uses assignment pattern (LOCAL:1 = 1), not condition prefix | No AC should reference LOCAL variable parsing or evaluation |
| C5 | Baseline test counts must not regress | 103 ErbParser + 89 ErbToYaml + 95 KojoComparer passing | AC must verify test counts >= baseline after changes |
| C6 | Bitwise `&` is within-variable operator, not between-condition separator | ERB: STAIN:口 & 汚れ_精液 | AC must verify `&` is parsed as part of the variable condition, not as a split point |
| C7 | FunctionCallParser nested args contain both parentheses AND commas | ERB: FIRSTTIME(TOSTR(TFLAG:50 + 500)) | AC for nested function must test argument extraction with inner parens and commas |
| C8 | 1 pre-existing KojoComparer test failure (FileDiscoveryTests) | Baseline: 1 failed, 3 skipped | AC#16 uses `matches "Failed:\\s+1\\b"` regex matcher (not `succeeds`) due to non-zero exit code from pre-existing failure. Word boundary `\\b` prevents false match on `Failed: 10`+ |
| C9 | MARK/EXP (327 occurrences) and bare variables (87 occurrences) are out of scope | Investigation finding | No AC should reference MARK, EXP, or bare variable parsing |
| C10 | YAML output format uses "entries" (not "branches") per BranchesToEntriesConverter | DatalistConverter.cs:96-97 | YAML condition output ACs must use entries format |
| C11 | Bitwise `&` values use DIM CONST names (non-numeric) that int.TryParse cannot resolve | DIM.ERH:293 汚れ_精液=4, DIM.ERH:471 前回売春_初売春=1 | YAML output must store resolved numeric values, not raw constant names. AC must test with DIM CONST-sourced values |
| C12 | DIM.ERH format uses `#DIM CONST name = value` (no `#DIM CONST INT` variant found in current ERB corpus) | Game/ERB/DIM.ERH format analysis | DimConstResolver.LoadFromFile handles `#DIM CONST` format. If `#DIM CONST INT` variant is discovered during implementation, handle as format extension. Integration test AC#32 verifies against real file |
| C13 | Bitwise `&` in ERB patterns always has surrounding spaces | ERB corpus analysis needed | Parser regex `\s*&\s*` relies on spaces around `&`. Regex name group changed from `[^:\s]+` to `[^:\s&]+` to stop at `&` even without surrounding spaces (robustness measure). This handles edge cases where spaceless patterns might exist (`CFLAG:test&value` would correctly parse as name=test, operator=&, value=value) |

### Constraint Details

**C1: && vs & Disambiguation**
- **Source**: LogicalOperatorParser.cs:226-228 `SplitOnOperator` uses `condition.Substring(i, op.Length) == op` where op="&&" (length 2). Single `&` does not match.
- **Verification**: Run existing ErbParser.Tests after changes. Must still pass 103/103.
- **AC Impact**: ACs MUST include both (a) regression test that `A && B` still splits correctly and (b) new test that `STAIN:口 & 汚れ_精液` is parsed with bitwise operator preserved.

**C2: TalentRef Structural Difference**
- **Source**: TalentRef.cs has `Target = string.Empty` (non-nullable), `Name = string.Empty` (non-nullable), and no `Index` property. CflagRef/TcvarRef/EquipRef/ItemRef all have `Target?`, `Name?`, `Index?` (nullable).
- **Verification**: Compare property signatures across all 5 Ref types.
- **AC Impact**: Generic base class design must either (a) make TalentRef a special case, (b) use nullable properties in base with TalentRef adapter, or (c) exclude TalentRef from consolidation. AC should verify the chosen approach works for all 5 types.

**C3: FUNCTION Evaluation Scope**
- **Source**: Functions like FIRSTTIME need persistent state tracking, HAS_VAGINA needs body data, GETBIT needs bitfield operations. Each function type requires different game state access.
- **Verification**: List function types found in kojo ERBs (FIRSTTIME, TOSTR, HAS_VAGINA, MASTER_POSE, etc.).
- **AC Impact**: AC should test that FUNCTION conditions are (a) accepted by ValidateConditionScope, (b) evaluable via configurable callback/delegate, NOT that all function implementations are complete. The evaluator should accept a `Func<string, string[], bool>` or similar delegate.

**C6: Bitwise & Within Variable Condition**
- **Source**: ERB patterns like `STAIN:口 & 汚れ_精液` and `CFLAG:前回売春フラグ & 前回売春_初売春`. The `&` appears after the variable name, operating on the variable's value.
- **Verification**: Parse these patterns and verify the bitwise operator and mask value are preserved in the ICondition tree and YAML output.
- **AC Impact**: Bitwise `&` must be treated as a variable-level operator (like `!=`, `==`), not as a condition-level separator (like `&&`). The existing regex operator group `(!=|==|>=|<=|>|<)` needs `&` added, OR a separate parsing path for bitwise conditions.

**C7: Nested Function Arguments**
- **Source**: Pattern `FIRSTTIME(TOSTR(TFLAG:50 + 500))` has inner function call `TOSTR(TFLAG:50 + 500)` as argument to outer `FIRSTTIME`. Current regex `[^()]*` rejects this.
- **Verification**: Parse `FIRSTTIME(TOSTR(350), 1)` and verify args=[`TOSTR(350)`, `1`].
- **AC Impact**: Balanced-parenthesis matching must correctly identify top-level commas for argument splitting while preserving inner parenthesized expressions as single arguments.

**C11: DIM CONST Names in Bitwise Values**
- **Source**: ERB bitwise conditions use DIM CONST names as mask values: `STAIN:口 & 汚れ_精液` where `汚れ_精液` = 4 (DIM.ERH:293), `CFLAG:前回売春フラグ & 前回売春_初売春` where `前回売春_初売春` = 1 (DIM.ERH:471). TALENT bitwise conditions use numeric values (`& 2`, `& 3`) and are NOT affected.
- **Verification**: Parse `STAIN:口 & 汚れ_精液`, verify Value is resolved to numeric `4` in YAML output (not raw string `汚れ_精液`).
- **AC Impact**: DatalistConverter.MapErbOperatorToYaml must resolve DIM CONST names to numeric values before writing to YAML. The evaluator's `int.TryParse` assumes numeric values. Without resolution, 33 STAIN + 36 CFLAG bitwise conditions silently skip evaluation (conditions vacuously return true). Resolution approach: load DIM.ERH constants during conversion, map constant names to integer values.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F756 | [DONE] | Static variable type support - provides EQUIP/ITEM parsers and FunctionCall YAML passthrough |
| Predecessor | F755 | [DONE] | CFLAG/TCVAR compound support - provides foundation parser pattern |
| Successor | F706 | [BLOCKED] | Full equivalence verification - blocked until F757 enables complete condition parsing |
| Related | F752 | [DONE] | Compound TALENT conditions - created LogicalOperatorParser/SplitOnOperator |
| Related | F750 | [DONE] | YAML TALENT condition migration - established TalentRef/parser pattern |
| Related | F754 | [DRAFT] | YAML format unification - may affect output format |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| None | - | - | All dependencies are internal tool projects |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| tools/ErbToYaml/DatalistConverter.cs | HIGH | ConvertConditionToYaml dispatches on ICondition types. New BitwiseCondition/StainRef must be handled. |
| tools/KojoComparer/KojoBranchesParser.cs | HIGH | EvaluateCondition/ValidateConditionScope must accept new condition types. FUNCTION evaluation logic added here. |
| tools/ErbParser/LogicalOperatorParser.cs | HIGH | ParseAtomicCondition chain and SplitOnOperator must handle bitwise `&`. |
| tools/ErbParser/ICondition.cs | MEDIUM | JsonDerivedType registrations for new types (BitwiseCondition, StainRef if not consolidated). |
| tools/ErbParser/ConditionExtractor.cs | LOW | Delegates to LogicalOperatorParser. No changes needed unless API changes. |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Bitwise `&` operator support (STAIN, CFLAG, TALENT)" | All three in-scope variable types must have bitwise `&` parsing, YAML conversion (with DIM CONST resolution), and runtime evaluation | AC#1, AC#2, AC#3, AC#4, AC#20, AC#23, AC#27, AC#28, AC#29, AC#31, AC#32, AC#33 |
| "Advance toward full equivalence testing (F706: 650/650 MATCH)" | Parser changes must not regress existing test suites | AC#14, AC#15, AC#16 |
| "bitwise `&` operator support across all variable types" | Tokenizer must disambiguate `&&` (logical) from `&` (bitwise) without breaking existing parsing. TALENT limited to name-based patterns per Mandatory Handoffs | AC#1, AC#2, AC#3 |
| "nested function call parsing" | FunctionCallParser must handle balanced parentheses for recursive function calls | AC#5, AC#6 |
| "FUNCTION runtime evaluation" | FUNCTION key must be accepted by ValidateConditionScope and evaluable via configurable delegate | AC#7, AC#8, AC#9 |
| "eliminating the accumulated code duplication" | 5+ parser/Ref clones must be consolidated into generic base class(es) | AC#10, AC#11, AC#12 |
| "Adding STAIN will create a 6th clone" (implicit: must not clone) | StainConditionParser must use the generic base class, not be a new clone | AC#13 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Bitwise `&` parsed in STAIN condition | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 2 | Bitwise `&` parsed in CFLAG condition | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 3 | Bitwise `&` parsed in TALENT condition | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 4 | `&&` logical AND still splits correctly (regression) | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 5 | Nested function call parsed (balanced parens) | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 6 | Nested function args with inner parens and commas extracted | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 7 | FUNCTION key accepted in ValidateConditionScope | code | Grep(tools/KojoComparer/KojoBranchesParser.cs) | matches | "FUNCTION" | [x] |
| 8 | FUNCTION evaluation uses configurable delegate (Pos) | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [x] |
| 9 | FUNCTION evaluation with no delegate returns default (Neg) | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [x] |
| 10 | VariableConditionParser generic base class exists | code | Grep(tools/ErbParser/) | matches | "class VariableConditionParser<" | [x] |
| 11 | VariableRef base type exists | code | Grep(tools/ErbParser/) | matches | "class VariableRef" | [x] |
| 12 | CflagConditionParser delegates to generic base | code | Grep(path="tools/ErbParser/CflagConditionParser.cs") | matches | "VariableConditionParser<CflagRef>" | [x] |
| 13 | StainConditionParser uses generic base (not standalone clone) | code | Grep(tools/ErbParser/) | matches | "StainConditionParser.*VariableConditionParser" | [x] |
| 14 | ErbParser tests pass (baseline regression) | test | dotnet test tools/ErbParser.Tests | succeeds | - | [x] |
| 15 | ErbToYaml tests pass (baseline regression) | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 16 | KojoComparer tests no regression (baseline: 95 pass, 1 fail pre-existing) | output | dotnet test tools/KojoComparer.Tests | matches | "Failed:\\s+1\\b" | [x] |
| 17 | BitwiseCondition YAML output uses correct format | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 18 | STAIN key accepted in ValidateConditionScope | code | Grep(tools/KojoComparer/KojoBranchesParser.cs) | matches | "STAIN" | [x] |
| 19 | No new TODO/FIXME/HACK in F757-modified files | code | git diff + Grep | not_matches | "TODO|FIXME|HACK" | [x] |
| 20 | Bitwise `&` runtime evaluation in EvaluateVariableCondition | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [x] |
| 21 | F758 DRAFT created and registered in index | file | Glob(Game/agents/feature-758.md) | exists | - | [x] |
| 22 | Bitwise `&` runtime evaluation in TALENT-specific evaluator | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [x] |
| 23 | DIM CONST names resolved to numeric values in YAML output | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 24 | TcvarConditionParser delegates to generic base | code | Grep(path="tools/ErbParser/TcvarConditionParser.cs") | matches | "VariableConditionParser<TcvarRef>" | [x] |
| 25 | EquipConditionParser delegates to generic base | code | Grep(path="tools/ErbParser/EquipConditionParser.cs") | matches | "VariableConditionParser<EquipRef>" | [x] |
| 26 | ItemConditionParser delegates to generic base | code | Grep(path="tools/ErbParser/ItemConditionParser.cs") | matches | "VariableConditionParser<ItemRef>" | [x] |
| 27 | DimConstResolver wired in ErbToYaml CLI entry point | code | Grep(path="tools/ErbToYaml/Program.cs") | matches | "DimConstResolver.*LoadFromFile" | [x] |
| 28 | TALENT bitwise YAML output through ConvertTalentRef path | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 29 | CFLAG bitwise YAML output through ConvertCflagRef path | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 30 | STAIN dispatch exists in EvaluateCondition | code | Grep(path="tools/KojoComparer/KojoBranchesParser.cs") | matches | "TryGetValue.*STAIN" | [x] |
| 31 | Shared EvaluateOperator method exists in KojoBranchesParser | code | Grep(path="tools/KojoComparer/KojoBranchesParser.cs") | matches | "EvaluateOperator" | [x] |
| 32 | DimConstResolver loads real DIM.ERH constants correctly | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [x] |
| 33 | IDimConstResolver interface exists | code | Grep(path="tools/ErbToYaml/") | matches | "interface IDimConstResolver" | [x] |
| 34 | FUNCTION InvalidOperationException throw removed | code | Grep(path="tools/KojoComparer/KojoBranchesParser.cs") | not_matches | "InvalidOperationException.*FUNCTION" | [x] |
| 35 | BuildVariableKey shared helper exists in DatalistConverter | code | Grep(path="tools/ErbToYaml/DatalistConverter.cs") | matches | "BuildVariableKey" | [x] |

### AC Details

**AC#1: Bitwise `&` parsed in STAIN condition**
- Test: New unit test parsing `STAIN:口 & 汚れ_精液` verifies the bitwise operator and mask value are preserved in the ICondition tree.
- The `&` must be treated as a variable-level operator within the STAIN condition, not as a condition separator.
- Constraint: C6 (bitwise `&` is within-variable operator).

**AC#2: Bitwise `&` parsed in CFLAG condition**
- Test: New unit test parsing `CFLAG:奴隷:前回売春フラグ & 前回売春_初売春` verifies bitwise operator preserved.
- Tests that CFLAG conditions with `&` no longer silently drop the `& value` suffix.
- Constraint: C1 (must not break existing `&&` behavior).

**AC#3: Bitwise `&` parsed in TALENT condition**
- Test: New unit test parsing `TALENT:性別嗜好 & 1` (name-based lookup) verifies bitwise operator preserved.
- Note: Only name-based TALENT patterns are in scope. Target/numeric index patterns (`TALENT:PLAYER & 2`, `TALENT:2 & 2`) are out of scope due to pre-existing ConvertTalentRef limitation (GetTalentIndex returns null for "PLAYER" and numeric indices). See Mandatory Handoffs → F758.
- Constraint: C2 (TalentRef structural differences must be accommodated).

**AC#4: `&&` logical AND still splits correctly (regression)**
- Test: Existing and new tests verify that `A && B` still correctly splits into two conditions.
- Edge case: `A && B & C` must split on `&&` first (logical), then parse `B & C` as a bitwise condition.
- Constraint: C1 (disambiguation must not break existing tests).

**AC#5: Nested function call parsed (balanced parens)**
- Test: New unit test parsing `FIRSTTIME(TOSTR(350), 1)` returns a FunctionCall with name=FIRSTTIME and correctly extracted arguments.
- Balanced-parenthesis matching replaces the flat regex `[^()]*`.
- Constraint: C7 (nested args contain both parentheses AND commas).

**AC#6: Nested function args with inner parens and commas extracted**
- Test: Parsing `FIRSTTIME(TOSTR(TFLAG:50 + 500))` correctly preserves inner parenthesized expression as a single argument.
- Verifies that top-level commas split arguments while inner commas/parens are preserved.
- Constraint: C7.
- Known limitation: The balanced-paren parser does not handle string literals containing parentheses or commas (e.g., hypothetical `FUNC("text(with)parens")`). This is acceptable for F757 scope because ERB function arguments in the current corpus are identifiers, numbers, or nested function calls — none contain string literals with embedded parens.

**AC#7: FUNCTION key accepted in ValidateConditionScope**
- Verification: Grep for "FUNCTION" in the allowedKeys set in KojoBranchesParser.cs.
- Removes the deliberate fail-fast boundary added by F756 (KojoBranchesParser.cs:176-179).
- CRITICAL: The fail-fast `throw new InvalidOperationException` at EvaluateCondition line 175-179 must ALSO be replaced with actual FUNCTION evaluation logic. ValidateConditionScope (line 278) is only called within EvaluateCompoundCondition (for AND/OR/NOT sub-conditions). A standalone FUNCTION condition goes through EvaluateCondition directly, which hits the throw before scope validation.
- Constraint: C3 (FUNCTION evaluation scope limited to configurable delegate).

**AC#8: FUNCTION evaluation uses configurable delegate (Pos)**
- Test: New unit test constructing KojoBranchesParser with a `Func<string, string[], bool>` delegate that returns true for a known function.
- MUST test a **standalone** FUNCTION condition (top-level, NOT wrapped in AND/OR/NOT) to verify the fail-fast throw at EvaluateCondition line 175-179 is replaced with delegate evaluation.
- Delegate is passed via constructor: `new KojoBranchesParser(functionEvaluator: (name, args) => ...)`.
- Verifies the delegate is called with function name and arguments, and the condition evaluates to true.
- Constraint: C3 (configurable evaluation, not all function implementations).

**AC#9: FUNCTION evaluation with no delegate returns default (Neg)**
- Test: New unit test constructing KojoBranchesParser with no delegate (default constructor), verifying FUNCTION condition returns a safe default (false or passthrough).
- MUST test standalone to verify no InvalidOperationException is thrown.
- Ensures the evaluator does not throw when no delegate is provided (null default parameter).
- Constraint: C3.

**AC#10: VariableConditionParser generic base class exists**
- Verification: Grep for `class VariableConditionParser<` in tools/ErbParser/ directory.
- The generic base class should accept a TRef type parameter and consolidate the shared parsing logic from CflagConditionParser, TcvarConditionParser, EquipConditionParser, ItemConditionParser.
- Constraint: C2 (TalentRef may need special handling due to structural differences).

**AC#11: VariableRef base type exists**
- Verification: Grep for `class VariableRef` in tools/ErbParser/ directory.
- The base type consolidates the shared properties (Target?, Name?, Index?, Operator?, Value?) from CflagRef, TcvarRef, EquipRef, ItemRef.
- Constraint: C2 (TalentRef has non-nullable Target/Name and no Index — may remain a separate type or use adapter).

**AC#12: CflagConditionParser delegates to generic base**
- Verification: Grep for `VariableConditionParser<CflagRef>` in CflagConditionParser.cs confirms the class delegates to or extends the generic base.
- This is a positive test — the old standalone regex must be replaced by generic parser delegation.
- CflagConditionParser may be retained as a thin wrapper or deleted entirely; either way it must reference VariableConditionParser<CflagRef>.

**AC#13: StainConditionParser uses generic base (not standalone clone)**
- Verification: Grep for `StainConditionParser.*VariableConditionParser` confirms the new STAIN parser inherits from the generic base class.
- This prevents creating a 6th clone for STAIN.
- Constraint: C6 (bitwise `&` is within-variable operator, so the generic base must support bitwise operator parsing).

**AC#14: ErbParser tests pass (baseline regression)**
- Test: `dotnet test tools/ErbParser.Tests` must pass. Baseline: 103/103.
- New tests for bitwise, nested function, and generic base class are added on top of baseline.
- Constraint: C5 (baseline test counts must not regress).
- CRITICAL: Must verify JSON round-trip serialization works correctly with the new VariableRef intermediate class in the inheritance chain (ICondition → VariableRef → CflagRef). Existing ErbParser.Tests JSON round-trip tests implicitly cover this. If any round-trip test fails after migration, the root cause is likely JsonDerivedType discriminator interaction with the abstract intermediate class.

**AC#15: ErbToYaml tests pass (baseline regression)**
- Test: `dotnet test tools/ErbToYaml.Tests` must pass. Baseline: 89/89.
- New tests for BitwiseCondition YAML conversion added on top of baseline.
- Constraint: C5.

**AC#16: KojoComparer tests no regression**
- Test: `dotnet test tools/KojoComparer.Tests` output must match regex `Failed:\s+1\b` (exactly 1 failure, pre-existing from FileDiscoveryTests.DiscoverTestCases_WithRealFiles_ReturnsTestCases, not F757 scope).
- The `succeeds` matcher cannot be used because pre-existing failures cause non-zero exit code.
- Uses `matches` with word boundary `\b` to prevent false positives: `Failed: 10` does NOT match `Failed:\s+1\b`, ensuring exactly 1 failure is detected.
- Baseline: 95 passed, 1 failed, 3 skipped. No NEW failures allowed beyond the pre-existing 1.
- Contingency: If the pre-existing FileDiscoveryTests failure is fixed before F757 implementation, update AC#16 to use `succeeds` matcher instead of `matches` regex. The matcher choice is contingent on baseline state at implementation time.
- Constraint: C5, C8.

**AC#17: BitwiseCondition YAML output uses correct format (STAIN path)**
- Test: New ErbToYaml test verifying that `STAIN:口 & 汚れ_精液` produces correct YAML output with `bitwise_and` operator through the ConvertStainRef code path.
- Tests the STAIN-specific converter path. TALENT and CFLAG converter paths are tested separately by AC#28 and AC#29.
- Constraint: C10 (YAML output uses entries format, not branches).

**AC#18: STAIN key accepted in ValidateConditionScope**
- Verification: Grep for "STAIN" in the allowedKeys set in KojoBranchesParser.cs.
- STAIN must be added alongside TALENT, CFLAG, TCVAR, EQUIP, ITEM, AND, OR, NOT, FUNCTION.

**AC#19: No new TODO/FIXME/HACK in F757-modified files**
- Verification: Use `git diff --name-only` to identify files modified by F757, then Grep each modified file for `TODO|FIXME|HACK`. Only new/modified files are checked, not entire directories.
- Pre-existing TODO markers in unmodified files are out of F757 scope and will not cause false failures.
- Alternative: If `git diff` is unavailable during AC verification, check only the specific files listed in Impact Analysis.

**AC#20: Bitwise `&` runtime evaluation in EvaluateVariableCondition**
- Test: New KojoComparer unit test verifying that `EvaluateVariableCondition` correctly handles the `bitwise_and` YAML operator (stateValue & expected != 0).
- The evaluator switch in KojoBranchesParser.cs must include a `bitwise_and` case alongside eq/ne/gt/gte/lt/lte.
- Without this, all 33 STAIN conditions (which exclusively use bitwise `&`) will silently evaluate to false at runtime.
- Constraint: C6 (bitwise `&` is within-variable operator), C1 (must not break existing evaluation).

**AC#21: F758 DRAFT created and registered in index**
- Verification: `Glob(Game/agents/feature-758.md)` confirms file exists.
- F758 covers compound bitwise-comparison expression `(VAR & mask) == value` pattern (1 occurrence in KOJO_KU_愛撫.ERB:63).
- DRAFT file must be created and registered in index-features.md.
- Per Mandatory Handoffs table: compound bitwise-comparison requires two-stage parser beyond F757 scope.

**AC#22: Bitwise `&` runtime evaluation in TALENT-specific evaluator**
- Test: New KojoComparer unit test verifying that the TALENT-specific evaluation path (KojoBranchesParser.cs:106-149) correctly handles `bitwise_and` operator.
- The TALENT evaluator has a separate code path from EvaluateVariableCondition. Both must include `bitwise_and` case.
- Without this, 1 in-scope TALENT bitwise condition (`TALENT:性別嗜好 & 1`, name-based lookup) would silently evaluate to false at runtime. The remaining 25 TALENT bitwise occurrences (target/numeric index patterns) are out of scope per ConvertTalentRef limitation.

**AC#23: DIM CONST names resolved to numeric values in YAML output**
- Test: New ErbToYaml test verifying that `STAIN:口 & 汚れ_精液` produces YAML with `bitwise_and: 4` (numeric), not `bitwise_and: '汚れ_精液'` (raw string).
- DIM CONST resolution must occur during ErbToYaml conversion (DatalistConverter) so YAML is self-contained.
- Without resolution, EvaluateVariableCondition's `int.TryParse` fails on constant names, causing conditions to silently skip.
- Constraint: C11 (DIM CONST names are non-numeric).

**AC#24: TcvarConditionParser delegates to generic base**
- Verification: Grep for `VariableConditionParser<TcvarRef>` in TcvarConditionParser.cs.
- Same consolidation pattern as AC#12.

**AC#25: EquipConditionParser delegates to generic base**
- Verification: Grep for `VariableConditionParser<EquipRef>` in EquipConditionParser.cs.
- Same consolidation pattern as AC#12.

**AC#26: ItemConditionParser delegates to generic base**
- Verification: Grep for `VariableConditionParser<ItemRef>` in ItemConditionParser.cs.
- Same consolidation pattern as AC#12.

**AC#27: DimConstResolver wired in ErbToYaml CLI entry point**
- Verification: Grep for `DimConstResolver.*LoadFromFile` in Program.cs confirms the resolver is instantiated, loaded with DIM.ERH data, and passed to DatalistConverter.
- The `LoadFromFile` check ensures DimConstResolver is not just instantiated (empty) but actually loaded with DIM.ERH constants.
- DIM.ERH path is auto-discovered relative to input directory (same pattern as Talent.csv path resolution).
- Without CLI wiring, the DimConstResolver defaults to null (optional parameter) and all DIM CONST resolution is silently skipped in production.
- This is an integration check complementing AC#23's unit test.

**AC#28: TALENT bitwise YAML output through ConvertTalentRef path**
- Test: New ErbToYaml test verifying that `TALENT:性別嗜好 & 1` produces correct YAML output through the ConvertTalentRef code path.
- ConvertTalentRef (DatalistConverter.cs:284-301) calls `GetTalentIndex(talent.Name)` and then `MapErbOperatorToYaml(talent.Operator, talent.Value)`. This test verifies the path handles `Operator="&"` correctly, producing `bitwise_and: "1"` in YAML.
- Requires test fixture with `性別嗜好` talent in Talent.csv to ensure `GetTalentIndex` does not return null.
- Independent from AC#17 (STAIN path) because ConvertTalentRef has additional GetTalentIndex lookup logic.

**AC#29: CFLAG bitwise YAML output through ConvertCflagRef path**
- Test: New ErbToYaml test verifying that `CFLAG:奴隷:前回売春フラグ & 前回売春_初売春` produces correct YAML output through the ConvertCflagRef code path.
- ConvertCflagRef (DatalistConverter.cs:308-322) passes operator/value to `MapErbOperatorToYaml`. This test verifies the CFLAG-specific conversion handles `Operator="&"` correctly.
- With DIM CONST resolution (C11), `前回売春_初売春` should resolve to `1` in YAML output.

**AC#30: STAIN dispatch exists in EvaluateCondition**
- Verification: Grep for `TryGetValue.*STAIN` in KojoBranchesParser.cs confirms that `EvaluateCondition` has an explicit STAIN dispatch block.
- Without this dispatch, standalone STAIN conditions (not wrapped in AND/OR/NOT) would fall through to `return false` at EvaluateCondition line 181, silently failing all 33 STAIN conditions at runtime.
- AC#18 (ValidateConditionScope) only covers compound sub-conditions. AC#30 covers standalone STAIN dispatch.
- The dispatch block follows the existing pattern for CFLAG/TCVAR/EQUIP/ITEM: `condition.TryGetValue("STAIN", out var stainObj) && stainObj is Dictionary<object, object> stainDict → EvaluateVariableCondition("STAIN", stainDict, state)`.

**AC#31: Shared EvaluateOperator method exists in KojoBranchesParser**
- Verification: Grep for `EvaluateOperator` in KojoBranchesParser.cs confirms a shared static method for operator evaluation.
- Both TALENT-specific evaluator and EvaluateVariableCondition must call this shared method instead of duplicating the operator switch.
- Eliminates 7-case operator switch duplication (eq, ne, gt, gte, lt, lte, bitwise_and) across 2 code paths.

**AC#32: DimConstResolver loads real DIM.ERH constants correctly**
- Test: Integration test that loads `Game/ERB/DIM.ERH` (or a representative subset) and verifies known constants resolve: `汚れ_精液` → 4 (DIM.ERH:293), `前回売春_初売春` → 1 (DIM.ERH:471).
- Catches DIM.ERH format mismatches (e.g., `#DIM CONST INT` variant vs expected `#DIM CONST`) before runtime.
- If `#DIM CONST INT` format is discovered during implementation, DimConstResolver.LoadFromFile must be updated to handle it.

**AC#33: IDimConstResolver interface exists**
- Verification: Grep for `interface IDimConstResolver` in tools/ErbToYaml/ confirms the interface is defined.
- DatalistConverter depends on `IDimConstResolver?` (not concrete `DimConstResolver?`) for testability.
- Follows existing DI pattern (IDatalistConverter) in the codebase.

**AC#34: FUNCTION InvalidOperationException throw removed**
- Verification: Grep for `InvalidOperationException.*FUNCTION` with `not_matches` confirms the fail-fast throw at EvaluateCondition line 175-179 has been replaced with actual FUNCTION evaluation logic. Pattern matches `throw new InvalidOperationException("FUNCTION conditions...")` which is the actual code structure (InvalidOperationException appears before FUNCTION in the string).
- Complements AC#7 (allowlist) and AC#8/AC#9 (delegate tests) by directly verifying the throw removal.
- Without this check, a new FUNCTION block added above the throw would leave the throw as dead code.

**AC#35: BuildVariableKey shared helper exists in DatalistConverter**
- Verification: Grep for `BuildVariableKey` in DatalistConverter.cs confirms the shared key construction helper exists.
- All 4 identical converter paths (ConvertCflagRef, ConvertEquipRef, ConvertItemRef, ConvertStainRef) must use this helper instead of duplicating the key construction logic.
- ConvertTalentRef and ConvertTcvarRef are excluded (different key construction patterns).
- Prevents the 5th clone of the same key construction logic.

### Goal Coverage Verification

| Goal# | Goal Item | Covering AC(s) | Verification |
|:-----:|-----------|-----------------|--------------|
| 1 | Bitwise `&` operator support (parser + converter + evaluator for STAIN/CFLAG/TALENT) | AC#1, AC#2, AC#3, AC#4, AC#17, AC#20, AC#22, AC#23, AC#27, AC#28, AC#29, AC#30, AC#31, AC#32, AC#33, AC#35 | Parser tests (AC#1-3), regression (AC#4), YAML output STAIN (AC#17), TALENT converter (AC#28), CFLAG converter (AC#29), runtime evaluation (AC#20), TALENT evaluator (AC#22), DIM CONST resolution (AC#23), CLI wiring (AC#27), STAIN dispatch (AC#30), shared operator evaluation (AC#31), DIM.ERH integration (AC#32), interface abstraction (AC#33), key construction helper (AC#35) |
| 2 | Nested function call parsing (balanced-parenthesis matching) | AC#5, AC#6 | Positive test with nested args (AC#5), edge case with inner parens+commas (AC#6) |
| 3 | FUNCTION runtime evaluation (allowlist + delegate evaluator) | AC#7, AC#8, AC#9, AC#34 | Allowlist (AC#7), positive delegate (AC#8), negative no-delegate (AC#9), throw removal (AC#34) |
| 4 | STAIN parser (StainConditionParser + StainRef) | AC#1, AC#13, AC#18, AC#30 | STAIN parsing (AC#1), generic base (AC#13), scope allowlist (AC#18), dispatch (AC#30) |
| 5 | Generic base class extraction (VariableConditionParser<TRef>, VariableRef) — parser/Ref only; converter deferred | AC#10, AC#11, AC#12, AC#13, AC#24, AC#25, AC#26 | Base class exists (AC#10, AC#11), all 4 clones delegate to generic (AC#12, AC#24-26), STAIN uses base (AC#13) |

**All 5 Goal items are covered. No gaps detected.**

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature addresses four interrelated architectural gaps in the condition parsing pipeline. The design follows a layered approach, where each layer builds upon the previous one:

**Layer 1: Generic Base Class Extraction** (Foundation)
Extract `VariableConditionParser<TRef>` generic base class and `VariableRef` base type to consolidate the duplicated parser/Ref code from F755/F756. This must be done FIRST, before adding STAIN support, to avoid creating a 6th clone. TalentRef remains separate due to structural differences (non-nullable Target/Name, no Index property).

**Layer 2: Bitwise `&` Operator Support** (Parser Extension)
Extend the generic parser's regex operator group from `(!=|==|>=|<=|>|<)` to `(!=|==|>=|<=|>|<|&)`. Additionally, extend TalentConditionParser's standalone regex operator group from `(!=|==|>=|<=|>|<)` to `(!=|==|>=|<=|>|<|&)` since TalentRef is excluded from the generic base class (Key Decision C). The bitwise `&` is treated as a variable-level operator (like comparison operators), not as a condition-level separator. The tokenizer (SplitOnOperator) does NOT need changes because `&` appears WITHIN variable conditions, not BETWEEN conditions. Also add `bitwise_and` case to both `EvaluateVariableCondition` (CFLAG/TCVAR/EQUIP/ITEM/STAIN path) and the TALENT-specific evaluation path in KojoBranchesParser.

**Layer 2b: DIM CONST Resolution** (Converter Extension)
Resolve DIM CONST names (e.g., `汚れ_精液` → `4`, `前回売春_初売春` → `1`) to numeric values during YAML conversion in DatalistConverter.MapErbOperatorToYaml. This is required because EvaluateVariableCondition uses `int.TryParse` for bitwise mask values, which fails on non-numeric strings. STAIN (33 conditions, all use DIM CONST names) and CFLAG (36 conditions) are affected. TALENT uses numeric values and is unaffected. Approach: load DIM.ERH `#DIM CONST` definitions at converter initialization, resolve constant names to integers in MapErbOperatorToYaml before writing to YAML. This makes YAML output self-contained (no runtime constant lookup needed).

**Layer 3: Nested Function Call Parsing** (Recursive Structure)
Replace FunctionCallParser's flat regex `[^()]*` with balanced-parenthesis matching algorithm. This enables parsing of recursive function calls like `FIRSTTIME(TOSTR(350), 1)`. The algorithm must handle both nested parentheses AND top-level comma splitting for argument extraction.

**Layer 4: FUNCTION Runtime Evaluation** (Evaluator Extension)
Add "FUNCTION" to ValidateConditionScope allowlist and implement configurable delegate-based evaluation in KojoBranchesParser. The evaluator accepts `Func<string, string[], bool>` delegate to enable game-specific function evaluation without implementing all 412 function occurrences in F757 scope.

**Layer 5: STAIN Parser** (Leverages Layer 1+2)
Implement StainConditionParser using the generic base class with prefix "STAIN" and bitwise `&` operator support. Add StainRef type (or use base VariableRef with discriminator). All 33 STAIN conditions in kojo use bitwise AND exclusively.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Add test case `STAIN:口 & 汚れ_精液` to ErbParser.Tests, verify StainRef with Operator="&" and Value="汚れ_精液" |
| 2 | Add test case `CFLAG:奴隷:前回売春フラグ & 前回売春_初売春` to ErbParser.Tests, verify CflagRef preserves bitwise operator |
| 3 | Add test case `TALENT:性別嗜好 & 1` to ErbParser.Tests, verify TalentRef preserves bitwise operator |
| 4 | Existing tests for `A && B` patterns remain passing (regression verification) |
| 5 | Add test case `FIRSTTIME(TOSTR(350), 1)` to ErbParser.Tests, verify FunctionCall with Name="FIRSTTIME" and Args=["TOSTR(350)", "1"] |
| 6 | Add test case with complex nested args to ErbParser.Tests, verify inner parentheses and commas preserved in single argument |
| 7 | Add "FUNCTION" to allowedKeys HashSet in KojoBranchesParser.cs:278, verify with Grep |
| 8 | Add test case to KojoComparer.Tests with constructor-injected delegate returning true, verify condition evaluates to true |
| 9 | Add test case to KojoComparer.Tests with default constructor (no delegate), verify condition returns false (safe default) |
| 10 | Create VariableConditionParser<TRef> generic class in tools/ErbParser/, verify with Grep for class signature |
| 11 | Create VariableRef base class with Target?, Name?, Index?, Operator?, Value? properties, verify with Grep |
| 12 | Convert CflagConditionParser to delegate to VariableConditionParser<CflagRef>, verify with Grep |
| 24 | Convert TcvarConditionParser to delegate to VariableConditionParser<TcvarRef>, verify with Grep |
| 25 | Convert EquipConditionParser to delegate to VariableConditionParser<EquipRef>, verify with Grep |
| 26 | Convert ItemConditionParser to delegate to VariableConditionParser<ItemRef>, verify with Grep |
| 13 | Create StainConditionParser extending VariableConditionParser<StainRef>, verify with Grep for inheritance |
| 14 | Run `dotnet test tools/ErbParser.Tests`, verify all tests pass (baseline: 103) |
| 15 | Run `dotnet test tools/ErbToYaml.Tests`, verify all tests pass (baseline: 89) |
| 16 | Run `dotnet test tools/KojoComparer.Tests`, verify 95 passing (1 pre-existing failure from FileDiscoveryTests) |
| 17 | Add test case to ErbToYaml.Tests verifying bitwise condition converts to YAML with `&` operator preserved in output |
| 18 | Add "STAIN" to allowedKeys HashSet in KojoBranchesParser.cs:278, verify with Grep |
| 19 | Use `git diff --name-only` to find F757-modified files, then Grep each for `TODO|FIXME|HACK` (not_matches) |
| 20 | Add `bitwise_and` case to `EvaluateVariableCondition` switch in KojoBranchesParser.cs, add unit test verifying stateValue & expected != 0 |
| 21 | Create feature-758.md [DRAFT] file and register in index-features.md |
| 22 | Add `bitwise_and` case to TALENT-specific evaluation path in KojoBranchesParser.cs, add unit test |
| 23 | Add DIM CONST resolution to DatalistConverter.MapErbOperatorToYaml, test that `汚れ_精液` → `4` in YAML output |
| 27 | Verify DimConstResolver is instantiated and passed to DatalistConverter in Program.cs CLI entry point |
| 28 | Add test to ErbToYaml.Tests verifying TALENT bitwise condition `TALENT:性別嗜好 & 1` produces correct YAML through ConvertTalentRef |
| 29 | Add test to ErbToYaml.Tests verifying CFLAG bitwise condition `CFLAG:奴隷:前回売春フラグ & 前回売春_初売春` produces correct YAML through ConvertCflagRef |
| 30 | Add STAIN dispatch block to EvaluateCondition: `condition.TryGetValue("STAIN", ...) → EvaluateVariableCondition("STAIN", ...)`, verify with Grep |
| 31 | Extract shared `EvaluateOperator(int, string, int) => bool` method in KojoBranchesParser. Both TALENT evaluator and EvaluateVariableCondition call this instead of inline switch |
| 32 | Integration test loading real DIM.ERH, verify `汚れ_精液` → 4 and `前回売春_初売春` → 1 |
| 33 | Create IDimConstResolver interface in tools/ErbToYaml/, verify DatalistConverter constructor accepts `IDimConstResolver?` (not `DimConstResolver?`) |
| 34 | Verify FUNCTION InvalidOperationException throw removed via `not_matches "InvalidOperationException.*FUNCTION"` grep |
| 35 | Verify BuildVariableKey shared helper exists in DatalistConverter, called by CFLAG/EQUIP/ITEM/STAIN converters |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **Bitwise `&` tokenizer handling** | A) Add `&` as third precedence level in SplitOnOperator<br>B) Add `&` to parser regex operator group<br>C) Create separate BitwiseCondition ICondition type | **B) Add to parser regex** | `&` appears WITHIN variable conditions (e.g., `STAIN:口 & 汚れ_精液`), not BETWEEN conditions. It's a variable-level operator like comparison operators, not a separator like `&&`. Option A would break `&&` splitting. Option C adds unnecessary complexity—bitwise `&` can be represented in existing Ref types via Operator="&" and Value. |
| **Generic base class vs TalentRef consolidation** | A) Include TalentRef in generic base (use nullable properties)<br>B) Keep TalentRef separate with adapter pattern<br>C) Exclude TalentRef from consolidation entirely | **C) Exclude TalentRef** | TalentRef has structural differences (non-nullable Target/Name, no Index property). Forcing it into the generic base would require either (1) nullable properties in base contradicting TalentRef semantics or (2) adapter overhead. Since only CflagRef, TcvarRef, EquipRef, ItemRef, and StainRef share identical structure, the consolidation benefit is achieved with these 5 types. TalentRef remains standalone. |
| **Nested function call parsing** | A) Recursive regex with balancing groups<br>B) Manual balanced-parenthesis matching<br>C) Stack-based parser | **B) Balanced-paren matching** | Regex balancing groups (.NET-specific) are fragile and hard to maintain. Stack-based parser is overengineered for the simple case of function arguments. Balanced-paren matching is straightforward, testable, and handles both nested parens and top-level comma splitting. |
| **FUNCTION evaluation scope** | A) Implement all 412 function occurrences<br>B) Opaque passthrough (no evaluation)<br>C) Configurable delegate evaluator | **C) Configurable delegate** | Option A is out of F757 scope (functions like FIRSTTIME need persistent state tracking, HAS_VAGINA needs body data). Option B defeats the purpose of runtime evaluation. Option C allows game-specific evaluation via `Func<string, string[], bool>` delegate while keeping F757 focused on the parsing/conversion pipeline. |
| **ICondition representation for bitwise** | A) Create BitwiseCondition ICondition type<br>B) Reuse existing Ref types with Operator="&" | **B) Reuse existing Ref types** | Bitwise `&` is syntactically identical to comparison operators—it appears after the variable name and operates on the variable's value. The existing Ref types (CflagRef, StainRef, TalentRef) already have Operator and Value properties. Creating a separate BitwiseCondition type would duplicate all the variable parsing logic (Target, Name, Index) for no semantic benefit. |
| **STAIN Ref type** | A) Reuse generic VariableRef with discriminator<br>B) Create StainRef class | **B) Create StainRef class** | Maintains consistency with existing pattern (CflagRef, TcvarRef, EquipRef, ItemRef). Each variable type has its own Ref class for type safety and JsonDerivedType discriminator clarity. StainRef extends VariableRef base class to leverage consolidation. |
| **FunctionCallParser function name regex** | A) Uppercase only `[A-Z_][A-Z0-9_]*`<br>B) Mixed case `[A-Za-z_][A-Za-z0-9_]*` | **A) Uppercase only** | ERB function names are always uppercase by Emuera convention (FIRSTTIME, TOSTR, HAS_VAGINA, GETBIT, etc.). All 221+ function call occurrences in kojo use uppercase. Preserving the existing regex restriction prevents false matches on non-function ERB keywords. |
| **JsonDerivedType with inheritance** | A) Keep flat (each Ref implements ICondition directly)<br>B) Use inheritance chain (Ref : VariableRef : ICondition) | **B) Inheritance chain** | System.Text.Json JsonDerivedType supports multi-level inheritance (interface → abstract class → concrete class). Existing ErbParser.Tests include JSON round-trip tests that will verify the inheritance chain works correctly with existing discriminator strings. AC#14 (ErbParser tests pass) implicitly covers this. |

### Interfaces / Data Structures

#### 1. Generic Base Classes

```csharp
// tools/ErbParser/VariableRef.cs
namespace ErbParser;

/// <summary>
/// Base class for variable reference conditions (CFLAG, TCVAR, EQUIP, ITEM, STAIN)
/// Common pattern: PREFIX:(target:)?(name|index)( op value)?
/// Note: TalentRef is NOT included due to structural differences (non-nullable Target/Name, no Index)
/// Note: Value stores the raw ERB string (may be a DIM CONST name like 汚れ_精液).
/// DIM CONST resolution to numeric values happens in DatalistConverter (Layer 2b), not here.
/// </summary>
public abstract class VariableRef : ICondition
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

// tools/ErbParser/VariableConditionParser.cs
namespace ErbParser;

/// <summary>
/// Generic parser for variable condition types (CFLAG, TCVAR, EQUIP, ITEM, STAIN)
/// Pattern: PREFIX:(target:)?(name|index)( op value)?
/// Supports comparison operators (!=, ==, >=, <=, >, <) and bitwise operator (&)
/// </summary>
public class VariableConditionParser<TRef> where TRef : VariableRef, new()
{
    private readonly string _prefix;
    private readonly Regex _pattern;

    /// <summary>
    /// Constructor accepting variable type prefix (e.g., "CFLAG", "STAIN")
    /// </summary>
    public VariableConditionParser(string prefix)
    {
        _prefix = prefix;
        // Regex pattern with bitwise & support in operator group
        // Name group uses [^:\s&]+ to stop at & even without surrounding spaces (robustness per C13)
        _pattern = new Regex(
            $@"^{prefix}:(?:([^:]+):)?([^:\s&]+)(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$",
            RegexOptions.Compiled
        );
    }

    public TRef? Parse(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return null;

        condition = condition.Trim();
        var match = _pattern.Match(condition);

        if (!match.Success)
            return null;

        var target = match.Groups[1].Success ? match.Groups[1].Value : null;
        var nameOrIndex = match.Groups[2].Value;

        if (string.IsNullOrWhiteSpace(nameOrIndex))
            return null;

        var operatorValue = match.Groups[3].Success ? match.Groups[3].Value : null;
        var value = match.Groups[4].Success ? match.Groups[4].Value.Trim() : null;

        var result = new TRef
        {
            Target = target,
            Operator = operatorValue,
            Value = value
        };

        if (int.TryParse(nameOrIndex, out int index))
        {
            result.Index = index;
            result.Name = null;
        }
        else
        {
            result.Name = nameOrIndex;
            result.Index = null;
        }

        return result;
    }
}
```

#### 2. StainRef Type

```csharp
// tools/ErbParser/StainRef.cs
namespace ErbParser;

/// <summary>
/// Represents a reference to a STAIN condition
/// Pattern: STAIN:(target:)?(name|index)( op value)?
/// Note: All 33 STAIN conditions in kojo use bitwise & operator exclusively
/// Example: STAIN:口 & 汚れ_精液
/// </summary>
public class StainRef : VariableRef
{
}
```

#### 3. Updated ICondition Registration

```csharp
// tools/ErbParser/ICondition.cs
[JsonDerivedType(typeof(TalentRef), typeDiscriminator: "talent")]
[JsonDerivedType(typeof(CflagRef), typeDiscriminator: "cflag")]
[JsonDerivedType(typeof(TcvarRef), typeDiscriminator: "tcvar")]
[JsonDerivedType(typeof(EquipRef), typeDiscriminator: "equip")]
[JsonDerivedType(typeof(ItemRef), typeDiscriminator: "item")]
[JsonDerivedType(typeof(StainRef), typeDiscriminator: "stain")] // NEW
[JsonDerivedType(typeof(NegatedCondition), typeDiscriminator: "negated")]
[JsonDerivedType(typeof(FunctionCall), typeDiscriminator: "function")]
[JsonDerivedType(typeof(LogicalOp), typeDiscriminator: "logical")]
public interface ICondition
{
}
```

#### 4. Balanced-Paren Function Parser

```csharp
// tools/ErbParser/FunctionCallParser.cs (rewrite)
namespace ErbParser;

public class FunctionCallParser
{
    private static readonly Regex FunctionNamePattern = new Regex(
        @"^([A-Z_][A-Z0-9_]*)\(",
        RegexOptions.Compiled
    );

    public FunctionCall? ParseFunctionCall(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return null;

        condition = condition.Trim();

        // Extract function name
        var match = FunctionNamePattern.Match(condition);
        if (!match.Success)
            return null;

        var functionName = match.Groups[1].Value;

        // Verify closing parenthesis at end
        if (!condition.EndsWith(")"))
            return null;

        // Extract arguments string (between first '(' and matching ')')
        int startIdx = match.Index + match.Length; // Position after "FNAME("
        int endIdx = condition.Length - 1; // Position of final ')'

        // Balanced-paren validation: verify first '(' matches last ')'
        int depth = 1; // Start with opening paren from function name
        int i = startIdx;
        while (i < condition.Length && depth > 0)
        {
            if (condition[i] == '(') depth++;
            else if (condition[i] == ')') depth--;
            i++;
        }

        // If depth != 0 or unmatched position, malformed
        if (depth != 0 || i - 1 != endIdx)
            return null;

        var argsString = condition.Substring(startIdx, endIdx - startIdx);

        // Split arguments by top-level commas (respecting nested parens)
        var args = SplitArguments(argsString);

        return new FunctionCall
        {
            Name = functionName,
            Args = args
        };
    }

    /// <summary>
    /// Split arguments by top-level commas, preserving nested parenthesized expressions
    /// Example: "TOSTR(350), 1" → ["TOSTR(350)", "1"]
    /// </summary>
    private string[] SplitArguments(string argsString)
    {
        if (string.IsNullOrWhiteSpace(argsString))
            return Array.Empty<string>();

        var args = new List<string>();
        var currentArg = new StringBuilder();
        int depth = 0;

        foreach (char c in argsString)
        {
            if (c == '(')
            {
                depth++;
                currentArg.Append(c);
            }
            else if (c == ')')
            {
                depth--;
                currentArg.Append(c);
            }
            else if (c == ',' && depth == 0)
            {
                // Top-level comma - split here
                args.Add(currentArg.ToString().Trim());
                currentArg.Clear();
            }
            else
            {
                currentArg.Append(c);
            }
        }

        // Add final argument
        if (currentArg.Length > 0)
            args.Add(currentArg.ToString().Trim());

        return args.ToArray();
    }
}
```

#### 5. FUNCTION Evaluation Extension

```csharp
// tools/KojoComparer/KojoBranchesParser.cs (additions)

// Add delegate as constructor parameter to KojoBranchesParser class
private readonly Func<string, string[], bool>? _functionEvaluator;

/// <summary>
/// Constructor accepts optional delegate for evaluating FUNCTION conditions.
/// Delegate signature: (functionName, args) => bool
/// Example: new KojoBranchesParser((name, args) => name == "FIRSTTIME" ? CheckFirstTime(args) : false)
/// IMPORTANT: This constructor REPLACES the existing parameterless constructor (lines 17-23).
/// The default parameter `functionEvaluator = null` ensures existing `new KojoBranchesParser()`
/// calls are unaffected. Do NOT create two constructors (avoids _deserializer duplication).
/// </summary>
public KojoBranchesParser(Func<string, string[], bool>? functionEvaluator = null)
{
    _functionEvaluator = functionEvaluator;
    _deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();
}

// Update ValidateConditionScope (line 278)
private void ValidateConditionScope(Dictionary<string, object> subCondition)
{
    var allowedKeys = new HashSet<string> { "TALENT", "CFLAG", "TCVAR", "EQUIP", "ITEM", "STAIN", "FUNCTION", "AND", "OR", "NOT" };
    // ... rest unchanged
}

// Update EvaluateCondition: add STAIN dispatch (after ITEM, before FUNCTION)
// Follows existing pattern for CFLAG/TCVAR/EQUIP/ITEM dispatch blocks
if (condition.TryGetValue("STAIN", out var stainObj) && stainObj is Dictionary<object, object> stainDict)
{
    return EvaluateVariableCondition("STAIN", stainDict, state);
}

// NOTE: EvaluateCondition is already a `private` instance method (KojoBranchesParser.cs:90).
// EvaluateVariableCondition is currently `private static` (line 297).
// FUNCTION evaluation accesses instance field `_functionEvaluator` through EvaluateCondition,
// which is already an instance method — no static-to-instance change needed for EvaluateCondition.
// EvaluateVariableCondition can remain static (bitwise_and uses only method params, no instance state).
// The FUNCTION dispatch is in EvaluateCondition (instance) → _functionEvaluator (instance field).
// The STAIN/CFLAG dispatch is in EvaluateCondition (instance) → EvaluateVariableCondition (static) — no conflict.

// Update EvaluateCondition (add FUNCTION case before fail-fast at line 175)
// Parse FUNCTION conditions using configurable delegate
if (condition.TryGetValue("FUNCTION", out var functionObj) && functionObj is Dictionary<object, object> functionDict)
{
    if (functionDict.TryGetValue("name", out var nameObj) && nameObj is string name)
    {
        var args = functionDict.TryGetValue("args", out var argsObj) && argsObj is List<object> argsList
            ? argsList.Select(a => a?.ToString() ?? string.Empty).ToArray()
            : Array.Empty<string>();

        // Use delegate if configured, otherwise return false (safe default)
        // Delegate callers are responsible for exception safety. Exceptions propagate to surface bugs.
        return _functionEvaluator?.Invoke(name, args) ?? false;
    }
    return false;
}

// Update EvaluateVariableCondition switch (add bitwise_and case)
// Existing switch handles: eq, ne, gt, gte, lt, lte → _ => false
// Add bitwise_and case:
case "bitwise_and":
    if (int.TryParse(operatorValue, out int mask))
        return (stateValue & mask) != 0;
    return false;

// CRITICAL: Extract shared operator evaluation to eliminate duplication between
// TALENT-specific evaluator (lines 106-149) and EvaluateVariableCondition (lines 297-339).
// See Task 3c below.

/// <summary>
/// Shared operator evaluation logic used by both TALENT and generic variable evaluators.
/// Eliminates 7-case operator switch duplication.
/// </summary>
private static bool EvaluateOperator(int stateValue, string op, int expected) => op switch
{
    "eq" => stateValue == expected,
    "ne" => stateValue != expected,
    "gt" => stateValue > expected,
    "gte" => stateValue >= expected,
    "lt" => stateValue < expected,
    "lte" => stateValue <= expected,
    "bitwise_and" => (stateValue & expected) != 0,
    _ => false
};
```

#### 5b. DIM CONST Resolver

```csharp
// tools/ErbToYaml/IDimConstResolver.cs (new file)
namespace ErbToYaml;

/// <summary>
/// Interface for DIM CONST name resolution, enabling test doubles.
/// </summary>
public interface IDimConstResolver
{
    int? Resolve(string name);
    string ResolveToString(string value);
}

// tools/ErbToYaml/DimConstResolver.cs (new file)
namespace ErbToYaml;

/// <summary>
/// Resolves DIM CONST names (e.g., 汚れ_精液) to numeric values from DIM.ERH definitions.
/// Used by DatalistConverter to write resolved numeric values to YAML.
/// </summary>
public class DimConstResolver : IDimConstResolver
{
    private readonly Dictionary<string, int> _constants = new();

    /// <summary>
    /// Load #DIM CONST definitions from a DIM.ERH file.
    /// Format: #DIM CONST name = value
    /// </summary>
    public void LoadFromFile(string dimErhPath)
    {
        foreach (var line in File.ReadAllLines(dimErhPath))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("#DIM CONST "))
            {
                // Parse: #DIM CONST name = value
                var parts = trimmed.Substring("#DIM CONST ".Length).Split('=', 2);
                if (parts.Length == 2
                    && int.TryParse(parts[1].Trim(), out var value))
                {
                    _constants[parts[0].Trim()] = value;
                }
            }
        }
    }

    /// <summary>
    /// Resolve a constant name to its numeric value.
    /// Returns null if not found (value may already be numeric).
    /// </summary>
    public int? Resolve(string name) =>
        _constants.TryGetValue(name, out var value) ? value : null;

    /// <summary>
    /// Resolve a value string: if it's NOT already numeric AND is a known constant name,
    /// return numeric string. Otherwise return the original value.
    /// Skips resolution for already-numeric values to prevent collisions
    /// (e.g., a DIM CONST named "0" would not override the literal "0").
    /// </summary>
    public string ResolveToString(string value)
    {
        if (int.TryParse(value, out _))
            return value; // Already numeric, no resolution needed
        return _constants.TryGetValue(value, out var numericValue)
            ? numericValue.ToString()
            : value;
    }
}

// DatalistConverter constructor injection:
// Add optional IDimConstResolver parameter to BOTH constructor overloads
// Overload 1 (single-param):
public DatalistConverter(string talentCsvPath, IDimConstResolver? dimConstResolver = null)
{
    _dimConstResolver = dimConstResolver;
    // ... existing initialization
}

// Overload 2 (with schema validation) - used by Program.cs CLI:
public DatalistConverter(string talentCsvPath, string schemaPath, IDimConstResolver? dimConstResolver = null)
{
    _dimConstResolver = dimConstResolver;
    // ... existing initialization with schema loading
}

// CRITICAL: Program.cs uses Overload 2 (talentCsvPath, schemaPath) at lines 99 and 144.
// DimConstResolver MUST be injected into Overload 2 for production DIM CONST resolution.

// NOTE: IDatalistConverter interface does NOT need updating. IDimConstResolver is an optional
// constructor parameter (implementation detail), not an interface method. Consumers depending
// on IDatalistConverter are unaffected by this addition.

// DESIGN NOTE: DatalistConverter currently uses internal construction for _talentLoader,
// _conditionExtractor, _yamlSerializer (not DI). Adding IDimConstResolver as constructor
// parameter introduces a transitional DI pattern. This inconsistency is acknowledged and
// accepted: IDimConstResolver is the first step toward full DI. Full DI migration tracked
// as future work (separate from F758 scope — DatalistConverter DI is an internal quality concern).

// Program.cs DIM.ERH path resolution:
// Auto-discover DIM.ERH relative to input directory (same approach as Talent.csv).
// Program.cs already resolves talentCsvPath relative to input. Apply same pattern:
//   var dimErhPath = Path.Combine(inputDir, "..", "ERB", "DIM.ERH");
//   DimConstResolver? dimConstResolver = null;
//   if (File.Exists(dimErhPath))
//   {
//       dimConstResolver = new DimConstResolver();
//       dimConstResolver.LoadFromFile(dimErhPath);
//   }
// Pass dimConstResolver to DatalistConverter constructor.
// If DIM.ERH not found, dimConstResolver remains null → DIM CONST names pass through unresolved.
```

#### 6. DatalistConverter YAML Output

```csharp
// tools/ErbToYaml/DatalistConverter.cs (additions)

// Update ConvertConditionToYaml switch statement (add StainRef case)
case StainRef stain:
    return ConvertStainRef(stain);

// Shared key construction for variable types with identical patterns
// (CFLAG, EQUIP, ITEM, STAIN). ConvertTalentRef and ConvertTcvarRef are excluded
// due to different key construction logic.
private static string BuildVariableKey(VariableRef varRef)
{
    if (varRef.Index.HasValue)
        return varRef.Index.Value.ToString();
    return varRef.Target != null ? $"{varRef.Target}:{varRef.Name}" : varRef.Name!;
}

// Add ConvertStainRef method
private Dictionary<string, object> ConvertStainRef(StainRef stain)
{
    var key = BuildVariableKey(stain);
    return new Dictionary<string, object>
    {
        { "STAIN", new Dictionary<string, object>
            {
                { key, MapErbOperatorToYaml(stain.Operator, stain.Value) }
            }
        }
    };
}

// Update MapErbOperatorToYaml to handle bitwise & operator with DIM CONST resolution
// NOTE: Existing method is `private static` (DatalistConverter.cs:261). Must change to `private` (instance)
// to access _dimConstResolver field. All existing callers already use instance-method call syntax, so no call-site changes needed.
private Dictionary<string, object> MapErbOperatorToYaml(string? erbOp, string? value)
{
    string op = erbOp ?? "!=";
    string val = value ?? "0";

    // Resolve DIM CONST names to numeric values (C11)
    // DESIGN INVARIANT: Universal application of DimConstResolver is safe because
    // ResolveToString is a no-op for already-numeric values (int.TryParse guard).
    // TALENT/TCVAR/EQUIP/ITEM values are numeric and pass through unchanged.
    // Only STAIN/CFLAG with DIM CONST name values (汚れ_精液, 前回売春_初売春) are resolved.
    if (_dimConstResolver != null)
        val = _dimConstResolver.ResolveToString(val);

    var operatorValue = new Dictionary<string, object>();
    switch (op)
    {
        case "==": operatorValue["eq"] = val; break;
        case "!=": operatorValue["ne"] = val; break;
        case ">": operatorValue["gt"] = val; break;
        case ">=": operatorValue["gte"] = val; break;
        case "<": operatorValue["lt"] = val; break;
        case "<=": operatorValue["lte"] = val; break;
        case "&": operatorValue["bitwise_and"] = val; break; // NEW
        default: operatorValue["ne"] = "0"; break;
    }
    return operatorValue;
}
```

#### 7. LogicalOperatorParser Registration

```csharp
// tools/ErbParser/LogicalOperatorParser.cs (field updates after migration)

// After Task 2 migration, parser fields become:
private readonly VariableConditionParser<CflagRef> _cflagParser = new("CFLAG");
private readonly VariableConditionParser<TcvarRef> _tcvarParser = new("TCVAR");
private readonly VariableConditionParser<EquipRef> _equipParser = new("EQUIP");
private readonly VariableConditionParser<ItemRef> _itemParser = new("ITEM");
private readonly VariableConditionParser<StainRef> _stainParser = new("STAIN"); // NEW

// TalentConditionParser remains standalone (excluded from generic base per Key Decision C)

// Add to ParseAtomicCondition chain (before function parser)
// Try STAIN parser
var stain = _stainParser.Parse(condition);
if (stain != null)
{
    return stain;
}
```

### Migration Path for Existing Parsers

1. **Create VariableRef base class** (new file)
2. **Create VariableConditionParser<TRef> generic** (new file)
3. **Update CflagRef/TcvarRef/EquipRef/ItemRef** to extend VariableRef (change `class CflagRef : ICondition` to `class CflagRef : VariableRef` — remove explicit `: ICondition` since VariableRef inherits it. JsonDerivedType discriminators on ICondition remain unchanged.)
4. **Replace CflagConditionParser** implementation with `VariableConditionParser<CflagRef>` instantiation
5. **Replace TcvarConditionParser** implementation with `VariableConditionParser<TcvarRef>` instantiation
6. **Replace EquipConditionParser** implementation with `VariableConditionParser<EquipRef>` instantiation
7. **Replace ItemConditionParser** implementation with `VariableConditionParser<ItemRef>` instantiation
8. **Update TalentConditionParser regex** to include `&` in operator group: `(!=|==|>=|<=|>|<|&)`. Also update name group to `[^:\s&]+` to stop at `&` even without surrounding spaces (robustness per C13). (TalentRef excluded from generic base, so its parser must be independently updated)
9. **Create StainRef** extending VariableRef (new file)
10. **Create StainConditionParser** using `VariableConditionParser<StainRef>` (new file)

**File Deletion**: After migration, the old standalone parser files (CflagConditionParser.cs, TcvarConditionParser.cs, EquipConditionParser.cs, ItemConditionParser.cs) should be deleted if they are replaced entirely by the generic instantiation. However, they may be retained as thin wrappers if backward compatibility is needed:

```csharp
// Backward-compatible wrapper (optional)
public class CflagConditionParser
{
    private readonly VariableConditionParser<CflagRef> _parser = new("CFLAG");
    public CflagRef? ParseCflagCondition(string condition) => _parser.Parse(condition);
}
```

**Recommendation**: Use thin wrappers during migration, then delete wrappers after all call sites are updated to use the generic parser directly.

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|:---:|-------------|:---:|:------:|
| 1 | 10,11 | Create VariableConditionParser<TRef> generic base class and VariableRef base type | | [x] |
| 2 | 12,13,18,24,25,26 | Migrate existing parsers to generic base and create StainConditionParser | | [x] |
| 3 | 1,2,3,4,17,20,22,23,27,28,29,30,31,32,33,35 | Add bitwise `&` operator support to variable condition parsers, evaluator, and DIM CONST resolution | [I] | [x] |
| 4 | 5,6 | Rewrite FunctionCallParser with balanced-parenthesis matching | | [x] |
| 5 | 7,8,9,34 | Add FUNCTION runtime evaluation with configurable delegate | [I] | [x] |
| 6 | 14,15,16,19 | Verify test suite regression and technical debt | | [x] |
| 7 | 21 | Create F758 [DRAFT] for remaining condition parsing gaps (9 handoff items; may need splitting during /fc into coherent sub-features) | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | T1: Technical Design Layer 1 | VariableConditionParser<TRef> generic class and VariableRef base type |
| 2 | implementer | sonnet | T2: Technical Design Layer 2+5 | StainConditionParser, migration of existing parsers |
| 3 | implementer | sonnet | T3: Technical Design Layer 2 + evaluator | Bitwise `&` operator support in parser regex, converter, EvaluateVariableCondition, and EvaluateOperator extraction |
| 4 | implementer | sonnet | T4: Technical Design Layer 3 | FunctionCallParser with balanced-paren matching |
| 5 | implementer | sonnet | T5: Technical Design Layer 4 | FUNCTION evaluation in KojoBranchesParser |
| 6 | ac-tester | haiku | T6: AC commands | Test suite regression and tech debt verification |
| 7 | implementer | sonnet | T7: Mandatory Handoffs | F758 [DRAFT] creation and index-features.md registration |

**Constraints** (from Technical Design):

1. **C1: && vs & Disambiguation** - Bitwise `&` must be added to parser regex operator group `(!=|==|>=|<=|>|<|&)` WITHOUT breaking existing `&&` splitting in LogicalOperatorParser.SplitOnOperator. All 103 ErbParser tests must pass after changes.

2. **C2: TalentRef Exclusion** - TalentRef remains separate from generic VariableConditionParser<TRef> due to structural differences (non-nullable Target/Name, no Index property). Only CflagRef, TcvarRef, EquipRef, ItemRef, and StainRef use the generic base.

3. **C3: FUNCTION Evaluation Scope** - FUNCTION evaluation uses configurable `Func<string, string[], bool>` delegate. Do NOT implement all 412 function occurrences. Evaluator accepts delegate for game-specific functions and returns false (safe default) when no delegate is configured.

4. **C6: Bitwise & Within Variable** - Bitwise `&` is a variable-level operator (like comparison operators), not a condition-level separator (like `&&`). It appears WITHIN variable conditions (e.g., `STAIN:口 & 汚れ_精液`), not BETWEEN conditions.

5. **C7: Nested Function Arguments** - Balanced-paren matching must handle both nested parentheses AND top-level comma splitting. Pattern `FIRSTTIME(TOSTR(TFLAG:50 + 500))` must preserve inner function as single argument.

6. **Migration Path** - Follow the 9-step migration path documented in Technical Design to avoid breaking existing code:
   - Create VariableRef base class
   - Create VariableConditionParser<TRef> generic
   - Update CflagRef/TcvarRef/EquipRef/ItemRef to extend VariableRef
   - Replace parser implementations with generic instantiations
   - Create StainRef extending VariableRef
   - Create StainConditionParser using generic base
   - Register StainRef in ICondition JsonDerivedType
   - Add StainConditionParser to LogicalOperatorParser chain
   - Update DatalistConverter and KojoBranchesParser for STAIN/FUNCTION support

**Pre-conditions**:

- F755 (CFLAG/TCVAR compound support) is [DONE] - provides foundation parser pattern
- F756 (EQUIP/ITEM parsers) is [DONE] - provides 4 cloned parsers to consolidate
- Baseline test counts: 103 ErbParser, 89 ErbToYaml, 95 KojoComparer passing (1 pre-existing failure, 3 skipped)
- All 3 tool projects build with 0 warnings, 0 errors

**Success Criteria**:

- All 35 ACs pass verification
- Test counts >= baseline (103 ErbParser, 89 ErbToYaml, 95 KojoComparer passing)
- 5 parser clones consolidated into single generic implementation
- Bitwise `&` parsed correctly for STAIN/CFLAG/TALENT without breaking `&&`
- Nested function calls parsed with balanced-parenthesis matching
- FUNCTION conditions accepted and evaluable via delegate
- Zero technical debt in modified files
- Zero build warnings or errors

**Rollback Plan**:

If issues arise after deployment:

1. Revert commit with `git revert`
2. Notify user of rollback
3. Create follow-up feature for fix with additional investigation into:
   - JsonDerivedType discriminator compatibility after generic extraction
   - Regex operator group precedence between `&` and `&&`
   - FUNCTION evaluation edge cases not covered by initial delegate design

**Test Naming Convention**:

Test methods follow standard naming convention for new tests:
- Parser tests: `Test{VarType}{Feature}` (e.g., `TestStainBitwiseOperator`, `TestCflagBitwiseOperator`)
- Function tests: `TestNestedFunctionCall`, `TestBalancedParentheses`
- Evaluation tests: `TestFunctionEvaluatorDelegate`, `TestFunctionEvaluatorNullDelegate`

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped] {phase} {iter}: [{category-code}] {description} -->
<!-- Category codes: See Game/agents/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [resolved-applied] Phase2 iter4: [CON-001] DIM CONST names in bitwise `&` values → Added C11 constraint, Layer 2b design, AC#23 for DIM CONST resolution in DatalistConverter. Resolution: resolve constant names to numeric values during YAML conversion.
- [resolved-invalid] Phase2 iter4: [FMT-001] T3/T5 [I] tags with deterministic ACs — ac-validator.md (SSOT) explicitly permits `[I]` with concrete Expected values (line 91: "[I] Task without placeholder: OK").
- [resolved-invalid] Phase1-RefCheck iter1: [REF-001] F758 file does not exist — F758 creation is AC#21/T7 (implementation task during /run). Link added proactively. File will be created during implementation.
- [resolved-applied] Phase2 iter1: [AC-002] AC#17 detail only mentions STAIN bitwise YAML test case. ConvertTalentRef and ConvertCflagRef are separate code paths. → Added AC#28 (TALENT) and AC#29 (CFLAG) for independent converter path coverage.
- [resolved-applied] Phase2 iter1: [AC-003] AC#16 `contains "Failed: 1"` matcher weakness — Changed to `matches "Failed:\\s+1\\b"` regex with word boundary to prevent false match on `Failed: 10`+. Prior fix (`contains "Passed: 95"`) was invalid (new tests raise count), but regex approach correctly targets exactly 1 failure.
- [resolved-applied] Phase2 iter1: [DOC-001] MapErbOperatorToYaml static→instance change not documented in Technical Design — Added explicit note in Section 6 that existing `private static` must change to `private` (instance) for DimConstResolver access.
- [resolved-skipped] Phase2 iter4: [STR-001] Task 3 maps to 12 ACs spanning 5 concerns (parser regex, YAML conversion, DIM CONST resolution, runtime evaluation, STAIN dispatch). Rationale for skipping split: (1) template AC Coverage Rule explicitly allows "Multiple ACs per Task", no upper bound; (2) all 5 concerns share "bitwise `&` operator" as unifying theme; (3) the Implementation Contract Phase 3 already scopes Task 3 as a single implementer dispatch; (4) ACs have independent verification methods regardless of Task grouping.
- [resolved-skipped] Phase2 iter5: [PHI-001] Philosophy Derivation row 2 maps aspirational claim "Advance toward full equivalence testing" to regression-only ACs (AC#14-16). Rationale: (1) Philosophy already narrowed from "achieve" to "Advance toward" in iter5 fix; (2) Derived Requirement correctly narrows to "must not regress"; (3) F706 is the designated consumer feature that measures 650/650 MATCH delta, not F757; (4) forward-progress measurement belongs in F706 ACs, not F757.
- [resolved-skipped] Phase2 iter8: [PHI-002] Philosophy lists TALENT alongside STAIN/CFLAG without qualifying 25/26 exclusion. Rationale: (1) Philosophy Derivation row 3 already qualifies "TALENT limited to name-based patterns per Mandatory Handoffs"; (2) AC#3 detail states "Only name-based TALENT patterns are in scope"; (3) Out of Scope line 23 lists 25/26 exclusion; (4) limitation is surfaced in 3 locations within the feature file. Adding qualification to Philosophy text itself would be redundant with existing qualifications.
- [resolved-skipped] Phase2 iter8: [AC-004] AC#16 only checks Failed count, not Passed count. Rationale: (1) C8 documents design choice (pre-existing failure forces non-zero exit); (2) dotnet test does not silently skip previously-passing tests under normal circumstances; (3) AC#14/AC#15 with `succeeds` matchers catch test regressions in ErbParser/ErbToYaml; (4) KojoComparer test additions visible in code review. Theoretical risk accepted.
- [resolved-applied] Phase3 iter1: [MNT-001] TALENT evaluator operator switch duplication — extracted shared EvaluateOperator method, added AC#31
- [resolved-applied] Phase3 iter1: [MNT-002] DimConstResolver format fragility — added integration AC#32 and C12 constraint
- [resolved-applied] Phase3 iter1: [MNT-003] Constructor API surface — added Technical Constraint note
- [resolved-applied] Phase3 iter1: [MNT-004] Regex `&` without spaces edge case — added C13 constraint and Technical Constraint
- [resolved-applied] Phase3 iter1: [MNT-005] AC#19 pre-existing TODOs false positive — changed to F757-modified files only
- [resolved-applied] Phase3 iter1: [MNT-006] FUNCTION try-catch silent swallow — removed try-catch, delegate exceptions propagate
- [resolved-applied] Phase3 iter4: [MNT-007] KojoBranchesParser god-class boundary — added Mandatory Handoff tracking ConditionEvaluator extraction
- [resolved-applied] Phase3 iter4: [MNT-008] DimConstResolver interface — added IDimConstResolver interface, AC#33, updated constructor DI
- [resolved-applied] Phase3 iter4: [MNT-009] DimConstResolver numeric collision — added int.TryParse guard in ResolveToString
- [resolved-applied] Phase3 iter4: [MNT-010] JSON round-trip multi-level inheritance — added verification note to AC#14
- [resolved-applied] Phase3 iter4: [MNT-011] LogicalOperatorParser field declarations — added complete field list after migration
- [resolved-skipped] Phase3 iter4: [STR-001] Task 3 → 14 ACs (loop) — same issue resolved-skipped in Phase2 iter4 and Phase3 iter1
- [resolved-skipped] Phase3 iter4: [STR-002] F758 bundling (loop) — same issue validated as invalid by ac-validator in iter1
- [resolved-applied] Phase2 iter5: [FMT-005] Execution Log "32 ACs" → "33 ACs" stale count
- [resolved-applied] Phase2 iter5: [FMT-006] Task 7 "6 handoff items" → "7 handoff items" count stale after MNT-007
- [resolved-applied] Phase2 iter5: [DI-001] Overload 2 constructor used concrete DimConstResolver? → IDimConstResolver?
- [resolved-applied] Phase2 iter5: [CON-002] C12 phantom constraint downgraded: `#DIM CONST INT` not found in ERB corpus
- [resolved-applied] Phase2 iter5: [PHI-003] Philosophy Derivation row 1 missing DIM CONST ACs → added AC#23,27,32,33
- [resolved-applied] Phase2 iter6: [FMT-007] Orphaned free text line between Scope Discipline and Type → moved into blockquote
- [resolved-skipped] Phase3 iter1: [STR-001] Task 3 maps to 12+ ACs — previously resolved-skipped in Phase2 iter4 with valid rationale
- [resolved-applied] Phase2 iter2: [FMT-001] Free text between Scope Discipline and Type — moved Out of Scope bullets into Scope Discipline blockquote
- [resolved-applied] Phase2 iter2: [FMT-002] Success Criteria "30 ACs" → "32 ACs" count stale after Phase3 AC additions
- [resolved-skipped] Phase2 iter2: [FMT-003] Section ordering deviates from template (Baseline/AC Design Constraints before Dependencies/AC). Rationale: /fc-generated features (F755, F756, F757) routinely insert investigation sections (Root Cause Analysis, Feasibility Assessment, etc.) between Background and AC. The template ordering applies to initial creation; /fc Phase 1 investigation naturally produces this ordering. Reordering 1200+ lines risks introducing errors with no semantic benefit.
- [resolved-applied] Phase2 iter2: [AC-005] AC#22 detail overstates TALENT impact: "25 conditions" → "1 in-scope condition" per Out of Scope exclusions
- [resolved-applied] Phase2 iter3: [FMT-004] Execution Log "30 ACs" stale count → updated to reflect FL expansions
- [resolved-skipped] Phase2 iter3: [PHI-001] Philosophy advancement claim (loop) — same as iter1/iter5, already resolved-skipped with valid rationale
- [resolved-skipped] Phase2 iter3: [STR-002] F758 bundling (loop) — same as iter1, already validated as invalid by ac-validator
- [resolved-applied] Phase3 iter7: [MNT-012] IDatalistConverter interface unaffected by IDimConstResolver — added explicit note
- [resolved-applied] Phase3 iter7: [MNT-013] Universal DIM CONST resolution safety — added design invariant note
- [resolved-applied] Phase3 iter7: [MNT-014] FUNCTION throw removal verification gap — added AC#34
- [resolved-applied] Phase3 iter7: [MNT-015] Regex name group robustness — changed [^:\s]+ to [^:\s&]+ per C13
- [resolved-applied] Phase3 iter7: [MNT-016] Converter consolidation contradiction — acknowledged 4 identical converters, added Mandatory Handoff
- [resolved-skipped] Phase3 iter7: [STR-001] Task 3 → 15 ACs (loop x4) — same issue resolved-skipped in Phase2 iter4, Phase3 iter1, Phase3 iter4
- [resolved-skipped] Phase3 iter7: [AC-004] AC#16 pass count (loop x3) — same issue resolved-skipped in Phase2 iter1, Phase2 iter3
- [resolved-applied] Phase3 iter8: [MNT-017] DatalistConverter mixed DI pattern — acknowledged transitional inconsistency
- [resolved-applied] Phase3 iter8: [FMT-008] AC#19 method column "dotnet diff" → "git diff"
- [resolved-applied] Phase3 iter8: [MNT-018] FunctionCallParser string literal limitation — added known limitation note to AC#6
- [resolved-applied] Phase3 iter8: [MNT-019] Converter key construction helper — added BuildVariableKey, AC#35
- [resolved-skipped] Phase3 iter8: [STR-001] Task 3 → 15+ ACs (loop x5) — same issue resolved-skipped in 4 prior iterations
- [resolved-skipped] Phase3 iter8: [MNT-007] KojoBranchesParser god-class (loop x2) — already addressed iter4 with Mandatory Handoff
- [resolved-skipped] Phase3 iter8: [AC-004] AC#16 pass count (loop x4) — same issue resolved-skipped in 3 prior iterations
- [resolved-applied] Phase3 iter9: [TBD-001] DatalistConverter DI migration missing from Mandatory Handoffs — added concrete entry with F758 destination
- [resolved-applied] Phase3 iter9: [DOC-002] KojoBranchesParser constructor replacement clarity — documented REPLACES existing, no dual constructors
- [resolved-applied] Phase3 iter9: [FMT-009] Task 7 handoff count 8→9, Execution Log 33→35 ACs
- [resolved-skipped] Phase3 iter9: [STR-001] Task 3 → 16 ACs (loop x6)
- [resolved-skipped] Phase3 iter9: [MNT-013] Universal DIM CONST collision (loop x2) — design invariant already added
- [resolved-applied] Phase3 iter10-FO: [DOC-003] Migration Path step 3 inheritance clarity — added explicit ICondition removal note
- [resolved-applied] Phase3 iter10-FO: [AC-006] AC#16 fragile coupling — added contingency note for pre-existing failure fix
- [resolved-skipped] Phase3 iter10-FO: [STR-001] Task 3 → 16 ACs (loop x7)
- [resolved-skipped] Phase3 iter10-FO: [STR-002] F758 bundling (loop x3)
- [resolved-applied] Phase4 iter10-FO: [AC-007] AC#34 pattern reversed — changed FUNCTION.*InvalidOperationException to InvalidOperationException.*FUNCTION to match actual code

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| Compound bitwise-comparison `(VAR & mask) == value` (1 occurrence: KOJO_KU_愛撫.ERB:63) | Requires two-stage parser (bitwise then comparison); single-operator regex cannot capture both operators | Feature | F758 | T7 (DRAFT creation) |
| TALENT bitwise with target/numeric index `TALENT:PLAYER & 2`, `TALENT:2 & 2` (25 occurrences) | ConvertTalentRef.GetTalentIndex fails for "PLAYER" and numeric indices; pre-existing TalentRef limitation | Feature | F758 | T7 (DRAFT creation) |
| MARK/EXP conditions (327 occurrences across 55 files) | Different variable type requiring new parser/Ref; out of F757 scope | Feature | F758 | T7 (DRAFT creation) |
| Bare variable comparisons `PLAYER != MASTER` (87 occurrences across 7 files) | No prefix-based pattern; requires fundamentally different parsing approach | Feature | F758 | T7 (DRAFT creation) |
| LOCAL variables (787 occurrences) | Uses runtime assignment pattern (LOCAL:1 = 1), not condition prefix; requires stateful analysis | Feature | F758 | T7 (DRAFT creation) |
| TALENT evaluator duplication (KojoBranchesParser lines 106-149 vs EvaluateVariableCondition) | Pre-existing from F752. F757 extracts shared EvaluateOperator for operator switch dedup. Remaining duplication (key construction: GetTalentIndex vs string key) deferred to F758. | Feature | F758 | T7 (DRAFT creation) |
| KojoBranchesParser evaluation logic approaching extraction threshold (7+ variable types, compound recursion, function delegation, operator evaluation) | SRP violation accumulating across F752→F755→F756→F757. F757 adds STAIN dispatch, FUNCTION delegate, shared EvaluateOperator — all increasing responsibility count | Feature | F758 | T7 (DRAFT creation) |
| DatalistConverter Convert{Cflag,Equip,Item,Stain}Ref method duplication (4 identical converters) | 4 of 6 converters use identical Convert{Type}Ref structure. F757 extracts BuildVariableKey shared helper for key construction, eliminating the first layer of duplication. Remaining duplication: full method bodies (dictionary nesting, MapErbOperatorToYaml calls). Further consolidation requires generic method or visitor pattern | Feature | F758 | T7 (DRAFT creation) |
| DatalistConverter transitional DI pattern (IDimConstResolver injected, other deps internally constructed) | F757 introduces IDimConstResolver as constructor-injected dependency while _talentLoader, _conditionExtractor, _yamlSerializer remain internally constructed. Transitional inconsistency per [MNT-017] | Feature | F758 | T7 (DRAFT creation) |

<!-- TBD Prohibition: All deferred items MUST have concrete destination (F{ID}, T{N}, or Phase N) -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-06 | creation | - | Created from F756 investigation | runtime constructs split out |
| 2026-02-07 | fc | tech-investigator | Root cause analysis, feasibility, baseline measurement | FC Phase 2 complete |
| 2026-02-07 11:41 | START | implementer | Task 1 | - |
| 2026-02-07 11:41 | END | implementer | Task 1 | SUCCESS |
| 2026-02-07 | fc | ac-designer | AC design: 30 ACs, 5 Goal items, constraints C1-C11 (later expanded to 35 ACs, C1-C13 by FL) | FC Phase 3 complete |
| 2026-02-07 | fc | wbs-generator | 7 tasks, AC:Task 1:N mapping, Implementation Contract | FC Phase 5 complete |
| 2026-02-07 | fc | feature-validator | Validation PASS | [DRAFT] → [PROPOSED] |
| 2026-02-07 | run | implementer | Tasks 1-5 implementation | SUCCESS (all 7 tasks complete) |
| 2026-02-07 | run | ac-tester | AC verification | 35/35 PASS |
| 2026-02-07 | DEVIATION | Bash | ac-static-verifier.py --ac-type code | exit 1: AC#19 (tool can't parse Method), AC#27 (multi-line pattern). Both manually verified PASS. |
| 2026-02-07 | run | feature-reviewer | Post-review quality check | READY (no issues) |

---

## Links
- [feature-756.md](feature-756.md) - Static variable type support (predecessor, source of deferred items)
- [feature-755.md](feature-755.md) - CFLAG/TCVAR compound support (foundation)
- [feature-706.md](feature-706.md) - Full equivalence verification (consumer)
- [feature-752.md](feature-752.md) - Compound TALENT condition support (created LogicalOperatorParser)
- [feature-750.md](feature-750.md) - YAML TALENT condition migration (established pattern)
- [feature-543.md](feature-543.md) - IConditionEvaluator interface extraction (Related)
- [feature-754.md](feature-754.md) - YAML format unification (Related)
- [feature-758.md](feature-758.md) - Compound bitwise-comparison expression support (Handoff from F757)
