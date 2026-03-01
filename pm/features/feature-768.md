# Feature 768: Cross-Parser Refactoring (TalentConditionParser/VariableConditionParser Deduplication)

## Status: [CANCELLED]

> **Cancelled**: 2026-02-11 — 591/591 PASS (F706) で両パーサーの正常動作が証明済み。純粋な技術負債削減であり、機能的ブロッカーなし。

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

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)
Establish a Single Source of Truth (SSOT) for condition-parsing regex template and disambiguation logic across TalentConditionParser and VariableConditionParser. Consolidating shared patterns into a common utility eliminates manual synchronization obligations, reduces sync-drift risk, and ensures consistent behavior when future condition types (e.g., new variable types) are added. Inherited from F760/F758 trajectory toward full equivalence testing infrastructure.

### Problem (Current Issue)
TalentConditionParser was the first condition parser built (pre-F750), before F758 established the generic VariableConditionParser<TRef> pattern. Because TalentRef implements ICondition directly rather than extending VariableRef (due to non-nullable vs nullable Target/Name defaults), TalentConditionParser's regex and disambiguation logic were hand-rolled independently. When F760 added disambiguation refinements, it deliberately chose to keep TalentRef independent ("Option B - Independent Index property") and deferred cross-parser refactoring to F768 (Maintenance Note #1, feature-760.md:548). The result is ~25-35 lines of structurally identical code across TalentConditionParser.cs:21-24 (regex) and :78-96 (disambiguation) that must be manually synchronized with VariableConditionParser.cs:18-21 and :51-60 on any grammar change.

### Goal (What to Achieve)
1. Extract the shared regex pattern into a single constant
2. Provide a shared disambiguation utility that accommodates both the 3-step TALENT path (keyword allowlist, int.TryParse, Name fallback) and the 2-step variable path (int.TryParse, Name fallback)
3. Eliminate the duplication between TalentConditionParser and VariableConditionParser
4. Preserve their distinct type semantics (TalentRef vs VariableRef)
5. Preserve external consumer contracts

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is TalentConditionParser.TalentPattern identical to VariableConditionParser._pattern? | Both parsers independently construct the same regex for the same two-part/three-part condition grammar | TalentConditionParser.cs:21-24 vs VariableConditionParser.cs:18-21 |
| 2 | Why do both parsers independently construct the same regex? | TalentConditionParser was built standalone because TalentRef implements ICondition directly, not VariableRef | TalentRef.cs:15 (`TalentRef : ICondition`) |
| 3 | Why does TalentRef not extend VariableRef? | TalentRef uses non-nullable `string Target/Name` (default `string.Empty`) while VariableRef uses nullable `string? Target/Name` | TalentRef.cs:18,21 vs VariableRef.cs:12,15 |
| 4 | Why was TalentRef not migrated to VariableRef when F758 established the generic pattern? | F760 explicitly chose "Option B - Independent Index property" to keep TalentRef independent, avoiding nullability migration risk | feature-760.md Key Decisions |
| 5 | Why (Root)? | No refactoring pass was performed after F758 established the generic pattern. F760 knowingly created bounded duplication and deferred cross-parser consolidation to F768 as a tracked maintenance obligation | feature-760.md:548 Maintenance Note #1 |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Regex and disambiguation logic duplicated between TalentConditionParser and VariableConditionParser (~25-35 lines) | TalentConditionParser was built before the VariableConditionParser<TRef> generic pattern existed (pre-F758), and F760 deliberately deferred migration to F768 |
| Where | TalentConditionParser.cs:21-24,78-96 and VariableConditionParser.cs:18-21,51-60 | Architectural decision in F760 Key Decisions + TalentRef.cs:15 type hierarchy split |
| Fix | Copy-paste regex changes to both files on each grammar update | Extract shared regex constant + disambiguation utility; keep TalentRef/VariableRef type hierarchies separate |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F760 | [DONE] | Parent: established current duplication pattern; Key Decision deferred refactoring to F768 |
| F758 | [DONE] | Foundation: created VariableRef/VariableConditionParser<TRef> generic pattern |
| F769 | [DRAFT] | Sibling: Target-only TALENT evaluation (depends on F760, shares TargetKeywords surface) |
| F759 | [DONE] | Related: compound bitwise uses ResolveTalentKey which depends on TalentRef |
| F765 | [DONE] | Sibling: SELECTCASE ARG pattern (ArgConditionParser is separate concern, NOT in scope) |
| F761 | [DONE] | Related: LOCAL variable tracking (LocalConditionParser is separate concern, NOT in scope) |
| F706 | [WIP] | Consumer: full equivalence testing |
| F751 | [DRAFT] | Downstream: TALENT semantic mapping |
| F767 | [DONE] | Related: dialogue-schema documentation |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Regex extraction to shared constant | FEASIBLE | TalentPattern and _pattern are structurally identical; trivially extractable |
| Disambiguation sharing via utility | FEASIBLE | 3-step (TALENT) vs 2-step (variable) can be unified with optional keyword allowlist callback |
| TalentRef type hierarchy change | NOT RECOMMENDED | Non-nullable vs nullable Target/Name creates high-risk impedance mismatch; approach 1 avoids this |
| Test coverage adequacy | FEASIBLE | 159 ErbParser tests + 126 KojoComparer tests provide regression safety |
| Cross-project impact | FEASIBLE | Only 2 external references (ConditionSerializer.cs:139, StateConverter.cs:29) — bounded |
| Build with TreatWarningsAsErrors | FEASIBLE | Standard compliance; no new warning sources expected |

**Verdict**: FEASIBLE

Three viable approaches identified:
1. **Shared constant + utility** (RECOMMENDED): Extract regex to shared constant, create disambiguation utility with optional keyword allowlist hook. Keeps TalentRef/VariableRef separate.
2. **Common interface**: Introduce IConditionParser interface above both parsers. Moderate complexity, limited benefit.
3. **TalentRef extends VariableRef**: Full type hierarchy unification. High risk due to nullability mismatch, JSON discriminator, and dispatch ordering.

All three investigators recommend Approach 1.

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| tools/ErbParser | HIGH | TalentConditionParser and VariableConditionParser both refactored; new shared constant/utility created |
| tools/ErbParser (LogicalOperatorParser) | MEDIUM | Parser registration may need update if TalentConditionParser interface changes |
| tools/ErbToYaml (DatalistConverter) | NONE | No TalentRef/TargetKeywords references; unaffected by refactoring |
| tools/ErbToYaml (ConditionSerializer) | LOW | TargetKeywords consumer (line 139); ResolveTalentKey extracted here by F765 |
| tools/KojoComparer (StateConverter) | LOW | TargetKeywords cross-project reference must remain accessible |
| 13 variable wrapper parsers | LOW | No change expected if VariableConditionParser public API preserved |
| TalentRef/VariableRef types | NONE (approach 1) | Type hierarchy unchanged in recommended approach |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| TalentRef non-nullable `string Target/Name` vs VariableRef nullable `string? Target/Name` | TalentRef.cs:18,21 vs VariableRef.cs:12,15 | Prevents simple TalentRef inheritance from VariableRef; approach 1 avoids |
| TargetKeywords cross-project usage | ConditionSerializer.cs:139, StateConverter.cs:29 | Must remain accessible from ErbParser project (2 external consumers) |
| Generic constraint `where TRef : VariableRef, new()` | VariableConditionParser.cs:9 | Blocks TalentRef as TRef type parameter |
| TALENT parsed before variable parsers in LogicalOperatorParser | LogicalOperatorParser.cs:220-224 | Dispatch ordering must be preserved |
| TalentRef "talent" JSON discriminator | ICondition.cs:9 | JSON serialization schema must be preserved |
| Keyword allowlist is TALENT-specific | TalentConditionParser.cs:27-30 | Only TALENT needs 3-step disambiguation (keyword check before int.TryParse) |
| TreatWarningsAsErrors | Directory.Build.props | Zero warnings required for build |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Utility adoption friction if future parsers have incompatible patterns (e.g., ARG two-pattern system) | MEDIUM | LOW | Factory method accepts prefix parameter; incompatible parsers bypass utility by design (C10) |
| Nullability migration breaks JSON serialization | HIGH (if approach 3) | HIGH | Use approach 1 (shared utility) to avoid type hierarchy changes entirely |
| DatalistConverter dispatch ordering breaks | LOW | HIGH | If TalentRef ever extends VariableRef, reorder `case TalentRef` before `case VariableRef`; approach 1 avoids |
| LogicalOperatorParser ordering regression | LOW | MEDIUM | Preserve TALENT-first registration; add regression test |
| TargetKeywords relocation breaks cross-project consumers | LOW | MEDIUM | Keep in ErbParser namespace; 2 external refs produce compile-time errors if broken |
| F759 ResolveInnerBitwiseRef regression | LOW | MEDIUM | Existing BitwiseConversionTests provide coverage |
| Existing tests reference TalentConditionParser constructor directly | MEDIUM | MEDIUM | Preserve public API; constructor signature unchanged in approach 1 |

---

## Baseline Measurement

<!-- Generated by tech-investigator Phase 4.5. -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ErbParser tests | `dotnet test tools/ErbParser.Tests` | All pass (159 tests) | Includes TalentDisambiguationTests (12 tests from F760) |
| KojoComparer tests | `dotnet test tools/KojoComparer.Tests` | All pass (126 tests) | Includes TalentKeyParser tests |
| ErbToYaml tests | `dotnet test tools/ErbToYaml.Tests` | All pass | Includes BitwiseConversionTests |
| Build zero warnings | `dotnet build` | 0 warnings | TreatWarningsAsErrors enabled |

**Baseline File**: `.tmp/baseline-768.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Regex must exist in exactly one location (SSOT) | F760 Maintenance Note #1 | AC#1: Verify no duplicate regex definitions across both parsers |
| C2 | TargetKeywords must remain accessible from ErbToYaml and KojoComparer | ConditionSerializer.cs:139, StateConverter.cs:29 | AC#13, AC#14: Verify cross-project compilation succeeds |
| C3 | ParseTalentCondition public API signature preserved | 12+ test files in ErbParser.Tests | AC#8: Behavioral equivalence: all existing TALENT tests pass unchanged |
| C4 | VariableConditionParser.Parse public API preserved | 13 wrapper parsers + LogicalOperatorParser | AC#8: All variable parser consumers pass unchanged |
| C5 | Both disambiguation paths produce identical results to current behavior | TalentConditionParser.cs:78-96 (3-step) vs VariableConditionParser.cs:51-60 (2-step) | AC#8: ErbParser test suite verifies behavioral equivalence (12 TalentDisambiguationTests + variable parser tests) |
| C6 | Build with TreatWarningsAsErrors succeeds | Directory.Build.props | AC#12, AC#17: Build verification |
| C7 | All parser test suites pass | ErbParser.Tests, KojoComparer.Tests, ErbToYaml.Tests | AC#8, AC#9, AC#10: Full regression suite execution |
| C8 | LogicalOperatorParser TALENT-before-variable ordering preserved | LogicalOperatorParser.cs:220-224 | AC#15: TALENT parsed before variable parsers |
| C10 | ARG/LOCAL parsers not modified | ArgConditionParser.cs, LocalConditionParser.cs | AC#16: Different regex patterns; genuinely separate concern |

### Constraint Details

**C1: Regex SSOT**
- **Source**: F760 Maintenance Note #1 — "Regex sync obligation tracked in F768"
- **Verification**: Grep for regex pattern definition; must appear in exactly one location
- **AC Impact**: Primary AC must verify single-definition constraint

**C2: TargetKeywords Cross-Project Access**
- **Source**: ConditionSerializer.cs:139 and StateConverter.cs:29 reference TalentConditionParser.TargetKeywords
- **Verification**: `dotnet build` across tools/ErbToYaml and tools/KojoComparer
- **AC Impact**: Build AC must cover all three projects (ErbParser, ErbToYaml, KojoComparer)

**C5: Disambiguation Behavioral Equivalence**
- **Source**: 3-step TALENT (keyword allowlist -> int.TryParse -> Name) vs 2-step variable (int.TryParse -> Name)
- **Verification**: All 12 TalentDisambiguationTests + variable parser tests pass
- **AC Impact**: Test suite ACs must include specific disambiguation test classes

**C3: ParseTalentCondition Public API Preserved**
- **Source**: 12+ test files in ErbParser.Tests reference ParseTalentCondition
- **Verification**: All TalentDisambiguationTests (12 tests) pass unchanged
- **AC Impact**: AC#8 must run full ErbParser test suite including TalentDisambiguationTests

**C4: VariableConditionParser.Parse Public API Preserved**
- **Source**: 13 wrapper parsers and LogicalOperatorParser reference Parse()
- **Verification**: All variable parser consumers compile and tests pass
- **AC Impact**: AC#8 covers via ErbParser.Tests; wrapper parsers indirectly tested

**C6: Build with TreatWarningsAsErrors Succeeds**
- **Source**: Directory.Build.props TreatWarningsAsErrors=true
- **Verification**: `dotnet build` succeeds with 0 warnings
- **AC Impact**: AC#12 verifies build; AC#17 verifies no debt markers in modified files

**C7: All Parser Test Suites Pass**
- **Source**: ErbParser.Tests (159+), KojoComparer.Tests (126+), ErbToYaml.Tests
- **Verification**: `dotnet test` across all three projects
- **AC Impact**: AC#8, AC#9, AC#10 each verify one project's test suite

**C8: LogicalOperatorParser TALENT-Before-Variable Ordering**
- **Source**: LogicalOperatorParser.cs:220-224 calls TALENT parser before variable loop
- **Verification**: Grep for `_talentParser.*ParseTalentCondition` in LogicalOperatorParser.cs
- **AC Impact**: AC#15 verifies TALENT parser call exists; AC#8 provides behavioral ordering coverage

**C10: ARG/LOCAL Parsers Not Modified**
- **Source**: ArgConditionParser.cs uses `^ARG(?::(\d+))?...` pattern; LocalConditionParser.cs uses two-pattern indexed+bare system
- **Verification**: No changes to these files; no debt markers introduced
- **AC Impact**: AC#16 verifies no TODO/FIXME/HACK markers in ARG/LOCAL parsers

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F760 | [DONE] | Established current duplication pattern; Key Decision deferred refactoring to F768 |
| Related | F758 | [DONE] | Foundation: created VariableRef/VariableConditionParser<TRef> generic pattern |
| Related | F759 | [DONE] | Compound bitwise uses ResolveTalentKey depending on TalentRef |
| Related | F769 | [CANCELLED] | Sibling: Target-only TALENT evaluation shares TargetKeywords surface |
| Related | F765 | [DONE] | Sibling: SELECTCASE ARG (ArgConditionParser separate concern, NOT in scope) |
| Related | F761 | [DONE] | LOCAL variable tracking (LocalConditionParser separate concern, NOT in scope) |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Single Source of Truth (SSOT) for condition-parsing regex template" | Regex factory method must exist in exactly one location | AC#1 |
| "Consolidating shared patterns into a common utility" | Disambiguation utility must be shared between both parsers | AC#4, AC#5 |
| "eliminates manual synchronization obligations" | TalentConditionParser must no longer define its own regex | AC#6 |
| "reduces sync-drift risk" | VariableConditionParser must use the shared constant, not a local definition | AC#7 |
| "ensures consistent behavior when future condition types are added" | All existing parser consumers must continue to work (regression) | AC#8, AC#9, AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Shared regex factory method exists in exactly one location | code | Grep(src/tools/dotnet/ErbParser/) | count_equals | 1 | [ ] |
| 2 | TalentConditionParser uses shared regex (no inline Regex construction) | code | Grep(src/tools/dotnet/ErbParser/TalentConditionParser.cs) | not_matches | `new Regex\(` | [ ] |
| 3 | VariableConditionParser uses shared regex (no inline Regex construction) | code | Grep(src/tools/dotnet/ErbParser/VariableConditionParser.cs) | not_matches | `new Regex\(` | [ ] |
| 4 | TalentConditionParser calls shared disambiguation utility | code | Grep(src/tools/dotnet/ErbParser/TalentConditionParser.cs) | contains | "DisambiguateNameOrIndex" | [ ] |
| 5 | VariableConditionParser calls shared disambiguation utility | code | Grep(src/tools/dotnet/ErbParser/VariableConditionParser.cs) | contains | "DisambiguateNameOrIndex" | [ ] |
| 6 | TalentConditionParser uses shared regex factory | code | Grep(src/tools/dotnet/ErbParser/TalentConditionParser.cs) | contains | "CreateConditionPattern" | [ ] |
| 7 | VariableConditionParser uses shared regex factory | code | Grep(src/tools/dotnet/ErbParser/VariableConditionParser.cs) | contains | "CreateConditionPattern" | [ ] |
| 8 | ErbParser tests pass | test | dotnet test tools/ErbParser.Tests | succeeds | - | [ ] |
| 9 | KojoComparer tests pass | test | dotnet test tools/KojoComparer.Tests | succeeds | - | [ ] |
| 10 | ErbToYaml tests pass | test | dotnet test tools/ErbToYaml.Tests | succeeds | - | [ ] |
| 12 | Build zero warnings | build | dotnet build | succeeds | - | [ ] |
| 13 | TargetKeywords accessible from ConditionSerializer | code | Grep(src/tools/dotnet/ErbToYaml/ConditionSerializer.cs) | contains | "TargetKeywords" | [ ] |
| 14 | TargetKeywords accessible from StateConverter | code | Grep(src/tools/dotnet/KojoComparer/StateConverter.cs) | contains | "TargetKeywords" | [ ] |
| 15 | TALENT parsed before variable parsers in LogicalOperatorParser | code | Grep(src/tools/dotnet/ErbParser/LogicalOperatorParser.cs) | matches | `_talentParser.*ParseTalentCondition` | [ ] |
| 16 | No technical debt introduced in ARG/LOCAL parsers | code | Grep(src/tools/dotnet/ErbParser/ArgConditionParser.cs), Grep(src/tools/dotnet/ErbParser/LocalConditionParser.cs) | not_matches | `TODO\|FIXME\|HACK` | [ ] |
| 17 | Zero technical debt in modified files | code | Grep(src/tools/dotnet/ErbParser/TalentConditionParser.cs), Grep(src/tools/dotnet/ErbParser/VariableConditionParser.cs), Grep(src/tools/dotnet/ErbParser/ConditionPatternUtility.cs) | not_matches | `TODO\|FIXME\|HACK` | [ ] |

### AC Details

**AC#1: Shared regex factory method exists in exactly one location**
- **Test**: Grep pattern=`public static Regex CreateConditionPattern` path=src/tools/dotnet/ErbParser/ type=cs | count
- **Expected**: Exactly 1 match (the factory method definition in ConditionPatternUtility)
- **Rationale**: C1 constraint requires SSOT for regex template. The factory method `CreateConditionPattern(prefix)` encapsulates the single regex pattern string. Only one definition must exist across the entire ErbParser project.
- **Constraint**: C1

**AC#2: TalentConditionParser uses shared regex (no inline Regex construction)**
- **Test**: Grep pattern=`new Regex\(` path=src/tools/dotnet/ErbParser/TalentConditionParser.cs type=cs
- **Expected**: 0 matches — TalentConditionParser must not construct its own Regex
- **Rationale**: After refactoring, TalentConditionParser should consume the shared regex constant rather than defining its own. This verifies the deduplication is complete.
- **Constraint**: C1, C3

**AC#3: VariableConditionParser uses shared regex (no inline Regex construction)**
- **Test**: Grep pattern=`new Regex\(` path=src/tools/dotnet/ErbParser/VariableConditionParser.cs type=cs
- **Expected**: 0 matches — VariableConditionParser must not construct its own Regex inline
- **Rationale**: After refactoring, VariableConditionParser should use the shared constant or construct from it. No local `new Regex(...)` should remain.
- **Constraint**: C1, C4

**AC#4: TalentConditionParser calls shared disambiguation utility**
- **Test**: Grep pattern=`DisambiguateNameOrIndex` path=src/tools/dotnet/ErbParser/TalentConditionParser.cs type=cs
- **Expected**: At least 1 match — TalentConditionParser must call the shared disambiguation utility
- **Rationale**: Philosophy claim "Consolidating shared patterns into a common utility" requires disambiguation logic to be centralized. This AC verifies TalentConditionParser delegates to the shared utility instead of implementing its own 3-step disambiguation.
- **Constraint**: C1, C5

**AC#5: VariableConditionParser calls shared disambiguation utility**
- **Test**: Grep pattern=`DisambiguateNameOrIndex` path=src/tools/dotnet/ErbParser/VariableConditionParser.cs type=cs
- **Expected**: At least 1 match — VariableConditionParser must call the shared disambiguation utility
- **Rationale**: Same as AC#4. Verifies VariableConditionParser delegates to the shared utility instead of implementing its own 2-step disambiguation.
- **Constraint**: C1, C5

**AC#6: TalentConditionParser uses shared regex factory**
- **Test**: Grep pattern="CreateConditionPattern" path=src/tools/dotnet/ErbParser/TalentConditionParser.cs
- **Expected**: At least 1 match — TalentConditionParser must call the shared factory method
- **Rationale**: Verifies TalentConditionParser delegates regex construction to ConditionPatternUtility.CreateConditionPattern("TALENT") instead of defining its own regex string.
- **Constraint**: C1

**AC#7: VariableConditionParser uses shared regex factory**
- **Test**: Grep pattern="CreateConditionPattern" path=src/tools/dotnet/ErbParser/VariableConditionParser.cs
- **Expected**: At least 1 match — VariableConditionParser must call the shared factory method
- **Rationale**: Verifies VariableConditionParser delegates regex construction to ConditionPatternUtility.CreateConditionPattern(prefix) instead of defining its own regex string.
- **Constraint**: C1

**AC#8: ErbParser tests pass**
- **Test**: `dotnet test tools/ErbParser.Tests`
- **Expected**: All 159+ tests pass (includes TalentDisambiguationTests 12 tests from F760)
- **Rationale**: Full regression verification. Behavioral equivalence with pre-refactoring state. Covers C3 (ParseTalentCondition API preserved), C5 (both disambiguation paths identical), C8 (LogicalOperatorParser ordering). Includes both positive and negative test cases (e.g., TalentDisambiguationTests cover invalid/boundary inputs), satisfying engine-type positive/negative coverage requirement.
- **Constraint**: C3, C5, C7, C8

**AC#9: KojoComparer tests pass**
- **Test**: `dotnet test tools/KojoComparer.Tests`
- **Expected**: All 126+ tests pass
- **Rationale**: Regression verification for cross-project consumer. StateConverter references TargetKeywords; all TalentKeyParser tests must pass.
- **Constraint**: C2, C7

**AC#10: ErbToYaml tests pass**
- **Test**: `dotnet test tools/ErbToYaml.Tests`
- **Expected**: All tests pass (includes BitwiseConversionTests)
- **Rationale**: Regression verification for ConditionSerializer which references TargetKeywords. DatalistConverter dispatch ordering preserved.
- **Constraint**: C2, C7

**AC#12: Build zero warnings**
- **Test**: `dotnet build`
- **Expected**: Build succeeds with 0 warnings (TreatWarningsAsErrors enabled)
- **Rationale**: C6 constraint. Any unused imports, missing references, or nullability warnings will fail the build.
- **Constraint**: C6

**AC#13: TargetKeywords accessible from ConditionSerializer**
- **Test**: Grep pattern="TargetKeywords" path=src/tools/dotnet/ErbToYaml/ConditionSerializer.cs
- **Expected**: At least 1 match — ConditionSerializer.cs must continue referencing TargetKeywords (currently line 139)
- **Rationale**: C2 constraint. If TargetKeywords is relocated (e.g., to a shared utility class), the reference path may change but access must remain. This AC verifies the consumer still compiles and references the keyword set.
- **Constraint**: C2

**AC#14: TargetKeywords accessible from StateConverter**
- **Test**: Grep pattern="TargetKeywords" path=src/tools/dotnet/KojoComparer/StateConverter.cs
- **Expected**: At least 1 match — StateConverter.cs must continue referencing TargetKeywords (currently line 29)
- **Rationale**: C2 constraint. Cross-project access must be preserved regardless of where TargetKeywords is defined.
- **Constraint**: C2

**AC#15: TALENT parsed before variable parsers in LogicalOperatorParser**
- **Test**: Grep pattern=`_talentParser.*ParseTalentCondition` path=src/tools/dotnet/ErbParser/LogicalOperatorParser.cs
- **Expected**: At least 1 match — TALENT parser invocation must appear before variable parser loop
- **Rationale**: C8 constraint. LogicalOperatorParser.ParseAtomicCondition currently calls `_talentParser.ParseTalentCondition(condition)` at line 220, before the variable parser loop at line 227. This ordering must be preserved.
- **Constraint**: C8

**AC#16: No technical debt introduced in ARG/LOCAL parsers**
- **Test**: Grep pattern=`TODO|FIXME|HACK` path=src/tools/dotnet/ErbParser/ArgConditionParser.cs,src/tools/dotnet/ErbParser/LocalConditionParser.cs
- **Expected**: 0 matches — no debt markers introduced
- **Rationale**: C10 constraint. ARG and LOCAL parsers have genuinely different regex patterns (ARG: `^ARG(?::(\d+))?...`, LOCAL: two-pattern indexed+bare) and are explicitly out of scope. This AC verifies no technical debt was inadvertently introduced.
- **Constraint**: C10

**AC#17: Zero technical debt in modified files**
- **Test**: Grep pattern=`TODO|FIXME|HACK` path=src/tools/dotnet/ErbParser/TalentConditionParser.cs,src/tools/dotnet/ErbParser/VariableConditionParser.cs,src/tools/dotnet/ErbParser/ConditionPatternUtility.cs
- **Expected**: 0 matches
- **Rationale**: All files modified by this feature must be free of technical debt markers. Includes the shared utility file if created as a new file.
- **Constraint**: C6

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Extract shared regex pattern into a factory method | AC#1, AC#6, AC#7 |
| 2 | Provide shared disambiguation utility (3-step TALENT + 2-step variable) | AC#4, AC#5 |
| 3 | Eliminate duplication between TalentConditionParser and VariableConditionParser | AC#2, AC#3, AC#6, AC#7, AC#4, AC#5 |
| 4 | Preserve distinct type semantics (TalentRef vs VariableRef) | AC#8, AC#9, AC#10 |
| 5 | Preserve external consumer contracts | AC#8, AC#9, AC#10, AC#13, AC#14, AC#15 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

**Selected: Approach 1 (Shared Constant + Utility)**

Create a shared static class `ConditionPatternUtility` in src/tools/dotnet/ErbParser/ with:
1. **Shared regex constant**: `ConditionPattern` - extracted from both TalentPattern and _pattern
2. **Disambiguation utility**: `DisambiguateNameOrIndex()` - handles both 3-step (TALENT) and 2-step (variable) logic

**Key characteristics**:
- **Zero type hierarchy changes**: TalentRef remains `ICondition`, VariableRef remains abstract base
- **Minimal API surface**: Single static class with two members (constant + method)
- **Backward compatibility**: All existing parser public APIs preserved

**How this satisfies ACs**:
- AC#1-7: Single regex factory and disambiguation utility eliminates duplication
- AC#8-10: No behavioral changes, all tests pass
- AC#12-17: Build succeeds, cross-project refs preserved, no debt introduced

**Rationale for Approach 1 over Approach 2/3**:
- **Approach 2 (Common Interface)**: Adds abstraction layer without solving duplication
- **Approach 3 (Type Hierarchy Unification)**: High risk due to nullability mismatch (non-nullable `string Target/Name` in TalentRef vs nullable `string? Target/Name` in VariableRef), JSON discriminator changes, and DatalistConverter dispatch ordering

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `ConditionPatternUtility.CreateConditionPattern()` as single factory method; Grep verifies exactly 1 definition |
| 2 | TalentConditionParser does not construct inline Regex; Grep verifies no `new Regex(` |
| 3 | VariableConditionParser does not construct inline Regex; Grep verifies no `new Regex(` |
| 4 | TalentConditionParser calls `ConditionPatternUtility.DisambiguateNameOrIndex()`; Grep verifies `DisambiguateNameOrIndex` present |
| 5 | VariableConditionParser calls `ConditionPatternUtility.DisambiguateNameOrIndex()`; Grep verifies `DisambiguateNameOrIndex` present |
| 6 | TalentConditionParser calls `ConditionPatternUtility.CreateConditionPattern("TALENT")`; Grep verifies `CreateConditionPattern` present |
| 7 | VariableConditionParser calls `ConditionPatternUtility.CreateConditionPattern(prefix)`; Grep verifies `CreateConditionPattern` present |
| 8 | Behavioral equivalence preserved - no changes to disambiguation logic behavior; dotnet test ErbParser.Tests passes all 159+ tests |
| 9 | StateConverter.cs:29 continues referencing TargetKeywords (now via ConditionPatternUtility or TalentConditionParser); dotnet test KojoComparer.Tests passes |
| 10 | ConditionSerializer.cs:139 continues referencing TargetKeywords; dotnet test ErbToYaml.Tests passes |
| 12 | TreatWarningsAsErrors=true enforced; dotnet build succeeds with 0 warnings |
| 13 | TargetKeywords remains accessible (either in ConditionPatternUtility or TalentConditionParser); Grep verifies ConditionSerializer.cs contains "TargetKeywords" |
| 14 | TargetKeywords remains accessible; Grep verifies StateConverter.cs contains "TargetKeywords" |
| 15 | LogicalOperatorParser.cs:220 unchanged - TALENT parser invoked before variable parser loop; Grep verifies `_talentParser.*ParseTalentCondition` pattern |
| 16 | ArgConditionParser and LocalConditionParser not modified; Grep verifies no TODO/FIXME/HACK markers |
| 17 | All modified files clean; Grep verifies no TODO/FIXME/HACK in TalentConditionParser, VariableConditionParser, ConditionPatternUtility |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **Where to extract regex** | A) New ConditionPatternUtility class<br>B) Static member in TalentConditionParser<br>C) Static member in VariableConditionParser | **A** | Neutral location avoids circular dependency; both parsers equally depend on utility |
| **Disambiguation utility signature** | A) Static method with optional callback<br>B) Instance method in base class<br>C) Interface with two implementations | **A** | Minimal - single static method handles both paths via optional `keywordAllowlist` parameter |
| **TargetKeywords location** | A) Move to ConditionPatternUtility<br>B) Keep in TalentConditionParser<br>C) Duplicate in both | **B** | TALENT-specific concern; 2 external consumers already reference TalentConditionParser.TargetKeywords - no need to change |
| **TalentRef type hierarchy** | A) Keep separate from VariableRef<br>B) Extend VariableRef<br>C) Introduce common interface | **A** | Avoid nullability migration risk; non-nullable `string Target/Name` (TalentRef) vs nullable `string? Target/Name` (VariableRef) creates impedance mismatch |
| **VariableConditionParser API** | A) Preserve constructor + Parse() signature<br>B) Add static factory method<br>C) Change to use shared pattern directly | **A** | Behavioral equivalence - all 13 wrapper parsers continue using `new("PREFIX")` constructor |

### Interfaces / Data Structures

**New Class: ConditionPatternUtility**

```csharp
namespace ErbParser;

/// <summary>
/// Shared constants and utilities for condition parsing
/// </summary>
public static class ConditionPatternUtility
{
    /// <summary>
    /// Shared regex pattern for variable conditions with optional operator/value
    /// Supports: PREFIX:name, PREFIX:target:name, PREFIX:target:name != 0
    /// Name group uses [^:\s&]+ to stop at & even without spaces
    /// </summary>
    /// <param name="prefix">Condition prefix (e.g., "TALENT", "CFLAG", "STAIN")</param>
    /// <returns>Compiled regex pattern</returns>
    public static Regex CreateConditionPattern(string prefix)
    {
        return new Regex(
            $@"^{prefix}:(?:([^:]+):)?([^:\s&]+)(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$",
            RegexOptions.Compiled
        );
    }

    public record DisambiguationResult(string? Name, int? Index, bool IsKeywordTarget);

    /// <summary>
    /// Disambiguate nameOrIndex string into Name (string) or Index (int?)
    /// </summary>
    /// <param name="nameOrIndex">The string to disambiguate</param>
    /// <param name="target">The target string (may be empty or null)</param>
    /// <param name="keywordAllowlist">Optional: If provided and target is empty/null and nameOrIndex matches keyword, treat as target-only pattern</param>
    /// <returns>DisambiguationResult if successful, null if nameOrIndex is invalid</returns>
    public static DisambiguationResult? DisambiguateNameOrIndex(
        string nameOrIndex,
        string? target,
        HashSet<string>? keywordAllowlist)
    {
        if (string.IsNullOrWhiteSpace(nameOrIndex))
        {
            return null;
        }

        // Step 1: Keyword allowlist check (TALENT-specific 3-step path)
        if (keywordAllowlist != null &&
            string.IsNullOrEmpty(target) &&
            keywordAllowlist.Contains(nameOrIndex))
        {
            // Two-part pattern with keyword: TALENT:PLAYER
            // This is actually a target reference, not a name
            return new DisambiguationResult(null, null, true);
        }

        // Step 2: int.TryParse
        if (int.TryParse(nameOrIndex, out int parsedIndex))
        {
            return new DisambiguationResult(null, parsedIndex, false);
        }

        // Step 3: Fallback to Name
        return new DisambiguationResult(nameOrIndex, null, false);
    }
}
```

**Modified: TalentConditionParser**

```csharp
namespace ErbParser;

public class TalentConditionParser
{
    // Use shared pattern utility
    private static readonly Regex _pattern =
        ConditionPatternUtility.CreateConditionPattern("TALENT");

    // ERA system variable keywords that are treated as target references
    public static readonly HashSet<string> TargetKeywords = new HashSet<string>
    {
        "PLAYER", "MASTER", "TARGET", "ASSI"
    };

    public TalentRef? ParseTalentCondition(string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return null;

        condition = condition.Trim();
        var match = _pattern.Match(condition);

        if (!match.Success)
            return null;

        var target = match.Groups[1].Success ? match.Groups[1].Value : string.Empty;
        var nameOrIndex = match.Groups[2].Value;

        if (string.IsNullOrWhiteSpace(nameOrIndex))
            return null;

        var operatorValue = match.Groups[3].Success ? match.Groups[3].Value : null;
        var value = match.Groups[4].Success ? match.Groups[4].Value.Trim() : null;

        var result = new TalentRef
        {
            Target = target,
            Operator = operatorValue,
            Value = value
        };

        // Use shared disambiguation utility with TALENT-specific keyword allowlist
        var disambiguation = ConditionPatternUtility.DisambiguateNameOrIndex(
            nameOrIndex,
            target,
            TargetKeywords);

        if (disambiguation == null)
            return null;

        if (disambiguation.IsKeywordTarget)
        {
            result.Target = nameOrIndex;
            result.Name = string.Empty;
            result.Index = null;
        }
        else
        {
            result.Name = disambiguation.Name ?? string.Empty;
            result.Index = disambiguation.Index;
        }

        return result;
    }
}
```

**Modified: VariableConditionParser**

```csharp
namespace ErbParser;

public class VariableConditionParser<TRef> where TRef : VariableRef, new()
{
    private readonly string _prefix;
    private readonly Regex _pattern;

    public VariableConditionParser(string prefix)
    {
        _prefix = prefix;
        _pattern = ConditionPatternUtility.CreateConditionPattern(prefix);
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

        // Use shared disambiguation utility (no keyword allowlist for generic variables)
        var disambiguation = ConditionPatternUtility.DisambiguateNameOrIndex(
            nameOrIndex,
            target,
            null); // No keyword allowlist for generic variables

        if (disambiguation == null)
            return null;

        result.Name = disambiguation.Name;
        result.Index = disambiguation.Index;

        return result;
    }
}
```

**Modified: StainConditionParser (Composition Pattern)**

```csharp
namespace ErbParser;

public class StainConditionParser
{
    private readonly VariableConditionParser<StainRef> _parser = new("STAIN");

    public StainRef? Parse(string condition) => _parser.Parse(condition);
}
```

**Implementation Notes**:

1. **CreateConditionPattern vs Static Constant**: Using a factory method instead of a single static constant allows each parser to create its own compiled Regex instance with the correct prefix, avoiding the need to store prefix as a parameter in every Match() call.

2. **DisambiguateNameOrIndex return semantics**: Returns `DisambiguationResult` record with `IsKeywordTarget=true` when keyword allowlist match occurs - caller must check this property and assign to Target instead of Name/Index.

3. **TalentRef nullability preserved**: Result properties use `string.Empty` instead of `null` for Name/Target, maintaining existing non-nullable semantics.

4. **TargetKeywords location**: Remains in TalentConditionParser because it's TALENT-specific and already referenced by 2 external consumers (ConditionSerializer.cs:139, StateConverter.cs:29).

5. **Regex instance multiplicity**: The factory method provides pattern-template SSOT (single regex string definition), not instance SSOT. Each parser creates its own Regex instance with a different prefix, which is required by the prefix parameterization design. With 13 wrapper parsers this creates multiple instances, but all derive from the single `CreateConditionPattern` template.

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1 | Create ConditionPatternUtility.cs with shared regex factory and disambiguation method | | [ ] |
| 2 | 2,4,6,8 | Refactor TalentConditionParser to use ConditionPatternUtility | | [ ] |
| 3 | 3,5,7,8 | Refactor VariableConditionParser to use ConditionPatternUtility | | [ ] |
| 6 | 8,9,10 | Run full test suite (ErbParser, KojoComparer, ErbToYaml) | | [ ] |
| 7 | 12,13,14 | Verify build with zero warnings and cross-project accessibility | | [ ] |
| 8 | 15,16,17 | Verify parser ordering, ARG/LOCAL unchanged, zero tech debt | | [ ] |

### Task Tags

No `[I]` tags used. All AC Expected values are deterministic.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Execution Order

| Phase | Task# | Description | Pre-condition | Success Criteria |
|:-----:|:-----:|-------------|---------------|------------------|
| 1 | 1 | Create shared utility | None | ConditionPatternUtility.cs exists with CreateConditionPattern() and DisambiguateNameOrIndex() |
| 2 | 2,3 | Refactor parsers | Task 1 complete | Both parsers use shared utility |
| 3 | 6 | Verify integration | Tasks 2-3 complete | All tests pass |
| 4 | 7,8 | Final verification | Task 6 complete | Build succeeds, no ordering/debt issues |

### Constraints

| Constraint | Verification | Failure Action |
|------------|--------------|----------------|
| TalentRef type hierarchy unchanged | No changes to TalentRef.cs or VariableRef.cs | STOP - type changes out of scope |
| TargetKeywords location unchanged | Remains in TalentConditionParser | STOP - cross-project consumers must not break |
| LogicalOperatorParser ordering preserved | TALENT parsed before variable parsers | STOP - dispatch ordering regression |
| ARG/LOCAL parsers not modified | No changes to ArgConditionParser.cs or LocalConditionParser.cs | STOP - out of scope |

### Pre-conditions

- F760 completed (current duplication pattern established)
- All baseline tests passing (159 ErbParser, 126 KojoComparer, ErbToYaml tests)
- TreatWarningsAsErrors=true enforced in Directory.Build.props

### Success Criteria

- Single regex definition (AC#1: exactly 1 match for ConditionPattern)
- No local regex construction (AC#2-3, AC#6-7: no `new Regex(` or local pattern fields)
- Disambiguation consolidated (AC#4-5: DisambiguateNameOrIndex called)
- All 285+ tests pass (AC#8-10)
- Build with 0 warnings (AC#12)
- Cross-project refs work (AC#13-14)
- Parser ordering preserved (AC#15)
- ARG/LOCAL unchanged (AC#16)
- Zero tech debt (AC#17)

### Error Handling

| Error | Cause | Resolution |
|-------|-------|------------|
| Cross-project compile failure | TargetKeywords not accessible | Verify TalentConditionParser.TargetKeywords remains public static |
| Test failures in TalentDisambiguationTests | Disambiguation logic changed | Verify DisambiguateNameOrIndex() preserves 3-step TALENT path |
| Test failures in variable parser tests | Disambiguation logic changed | Verify DisambiguateNameOrIndex() preserves 2-step variable path |
| Build warnings | Unused imports, nullability issues | Clean up imports, verify nullable annotations |

### Implementation Notes

**Task 1: ConditionPatternUtility Structure**

Create `src/tools/dotnet/ErbParser/ConditionPatternUtility.cs`:

```csharp
namespace ErbParser;

/// <summary>
/// Shared constants and utilities for condition parsing
/// </summary>
public static class ConditionPatternUtility
{
    /// <summary>
    /// Create regex pattern for variable conditions with optional operator/value
    /// Supports: PREFIX:name, PREFIX:target:name, PREFIX:target:name != 0
    /// Name group uses [^:\s&]+ to stop at & even without spaces
    /// </summary>
    /// <param name="prefix">Condition prefix (e.g., "TALENT", "CFLAG", "STAIN")</param>
    /// <returns>Compiled regex pattern</returns>
    public static Regex CreateConditionPattern(string prefix)
    {
        return new Regex(
            $@"^{prefix}:(?:([^:]+):)?([^:\s&]+)(?:\s*(!=|==|>=|<=|>|<|&)\s*(.+))?$",
            RegexOptions.Compiled
        );
    }

    public record DisambiguationResult(string? Name, int? Index, bool IsKeywordTarget);

    /// <summary>
    /// Disambiguate nameOrIndex string into Name (string) or Index (int?)
    /// </summary>
    /// <param name="nameOrIndex">The string to disambiguate</param>
    /// <param name="target">The target string (may be empty or null)</param>
    /// <param name="keywordAllowlist">Optional: If provided and target is empty/null and nameOrIndex matches keyword, treat as target-only pattern</param>
    /// <returns>DisambiguationResult if successful, null if nameOrIndex is invalid</returns>
    public static DisambiguationResult? DisambiguateNameOrIndex(
        string nameOrIndex,
        string? target,
        HashSet<string>? keywordAllowlist)
    {
        if (string.IsNullOrWhiteSpace(nameOrIndex))
        {
            return null;
        }

        // Step 1: Keyword allowlist check (TALENT-specific 3-step path)
        if (keywordAllowlist != null &&
            string.IsNullOrEmpty(target) &&
            keywordAllowlist.Contains(nameOrIndex))
        {
            // Two-part pattern with keyword: TALENT:PLAYER
            // This is actually a target reference, not a name
            return new DisambiguationResult(null, null, true);
        }

        // Step 2: int.TryParse
        if (int.TryParse(nameOrIndex, out int parsedIndex))
        {
            return new DisambiguationResult(null, parsedIndex, false);
        }

        // Step 3: Fallback to Name
        return new DisambiguationResult(nameOrIndex, null, false);
    }
}
```

**Task 2: TalentConditionParser Refactoring**

Modify `src/tools/dotnet/ErbParser/TalentConditionParser.cs`:
- Replace `TalentPattern` field with `ConditionPatternUtility.CreateConditionPattern("TALENT")`
- Replace disambiguation logic (:78-96) with `ConditionPatternUtility.DisambiguateNameOrIndex()` call
- Keep `TargetKeywords` in place (no relocation)

**Task 3: VariableConditionParser Refactoring**

Modify `src/tools/dotnet/ErbParser/VariableConditionParser.cs`:
- Replace `_pattern` field with `ConditionPatternUtility.CreateConditionPattern(_prefix)` in constructor
- Replace disambiguation logic (:51-60) with `ConditionPatternUtility.DisambiguateNameOrIndex()` call (no keyword allowlist)

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
- [resolved] Phase2-Pending iter1: [AC-INC] AC#1 — Changed to verify factory method definition (CreateConditionPattern) count_equals 1
- [resolved] Phase2-Pending iter1: [AC-INC] AC#6/AC#7 — Changed to positive verification: contains "CreateConditionPattern"
- [resolved] Phase2-Uncertain iter1: [FMT-TBL] Implementation Contract table format — Accepted. Engine-type features may use adapted format
- [fix] Phase2-Review iter1: [FMT-TBL] Mandatory Handoffs | Changed from free text "None." to proper table format
- [fix] Phase2-Review iter1: [AC-DSC] AC#2/AC#3 Description | Corrected descriptions to reflect regex-focused matchers; added AC#4/AC#5 for disambiguation utility verification
- [fix] Phase2-Review iter1: [PHI-COV] Philosophy Derivation row 2 | Updated AC Coverage to AC#4,AC#5 for disambiguation utility consolidation
- [resolved] Phase2-Pending iter2: [PHI-INC] Philosophy — Added "template" qualifier, narrowed scope to TalentConditionParser and VariableConditionParser
- [fix] Phase2-Review iter2: [DES-API] DisambiguateNameOrIndex | Added `out bool isKeywordTarget` parameter to eliminate caller-side keyword re-checking
- [fix] Phase2-Review iter2: [FMT-CAT] Review Notes | Added category codes to all entries per template format
- [fix] Phase2-Review iter3: [FMT-NUM] AC Definition Table | Renumbered ACs sequentially 1-17; updated all cross-references
- [fix] Phase2-Review iter3: [AC-DSC] AC#2/AC#3 Details headings | Aligned with AC Definition Table descriptions
- [fix] Phase2-Review iter3: [FMT-DUP] Reference (from previous session) | Deleted duplicate section
- [resolved] Phase2-Pending iter3: [DES-DEAD] AC#11, Task 4 — Removed from scope. StainConditionParser has zero consumers (dead code)
- [resolved] Phase2-Uncertain iter3: [AC-ORD] AC#15 — Accepted. AC#15 (existence) + AC#8 (behavioral test suite) provides sufficient ordering coverage
- [fix] Phase2-Review iter4: [FMT-REF] Success Criteria + How this satisfies ACs | Updated stale AC# cross-references to match renumbered 1-17 table
- [fix] Phase2-Review iter4: [CON-DEL] AC Design Constraints C6 | Removed phantom DatalistConverter dispatch ordering constraint (file has no TalentRef dispatch)
- [fix] Phase2-Review iter4: [FMT-GOAL] Goal section | Enumerated 5 goal items to match Goal Coverage Verification table
- [resolved] Phase2-Pending iter4: [REF-FAB] DatalistConverter.cs:367 — Fabricated reference removed. Corrected to 2 external consumers throughout
- [fix] Phase2-Review iter5: [CON-REF] AC Details Constraint cross-refs | Updated stale IDs: AC#8→C3,C5,C7,C8; AC#9→C2,C7; AC#10→C2,C7; AC#11→C9; AC#12→C6; AC#15→C8; AC#16→C10; AC#17→C6
- [fix] Phase2-Review iter5: [FMT-LOG] Execution Log | Removed placeholder row
- [fix] Phase2-Review iter5: [AC-MAP] Tasks table | Moved AC#6 to Task#2, AC#7 to Task#3; Task#1 now AC#1 only
- [fix] Phase2-Review iter5: [AC-DSC] AC#16 Description | Changed from 'ARG/LOCAL parsers not modified' to 'No technical debt introduced in ARG/LOCAL parsers' to match Matcher
- [fix] Phase2-Review iter6: [AC-PATH] AC#17 | Added ConditionPatternUtility.cs to Grep path for debt verification
- [fix] Phase2-Review iter6: [AC-FMT] AC#16, AC#17 | Separated comma-delimited Grep paths into individual Grep() calls
- [resolved] Phase2-Pending iter6: [PHI-SCOPE] — Narrowed to "TalentConditionParser and VariableConditionParser". ARG/LOCAL excluded per C10
- [fix] Phase3-Maintainability iter1: Impact Analysis row 3 | Changed 'contains ResolveTalentKey duplicate from F765' to 'ResolveTalentKey extracted here by F765'
- [fix] Phase3-Maintainability iter1: Implementation Notes | Added item 6 documenting regex instance multiplicity design trade-off
- [fix] Phase3-Maintainability iter1: DisambiguateNameOrIndex signature | Replaced out parameters with DisambiguationResult record; updated TalentConditionParser and VariableConditionParser code blocks
- [fix] Phase3-Maintainability iter1: TalentConditionParser code block | Renamed TalentPattern to _pattern for naming consistency
- [fix] Phase3-Maintainability iter1: Risks table row 1 | Reframed 'Over-engineering' to 'Utility adoption friction' per Zero Debt Upfront principle
- [resolved] Phase3-Maintainability iter1: [REF-FAB-PROP] — Corrected to 2 consumers throughout all sections
- [resolved] Phase3-Maintainability iter1: [TASK-ORPHAN] Task#5 — Merged into Task#7 (AC#13/AC#14 verified by build)
- [fix] Phase2-Review iter2: [CON-DET] AC Design Constraints | Added Constraint Details for C3, C4, C6, C7, C8, C10
- [fix] Phase2-Review iter2: [FMT-TAG] Tasks section | Added Task Tags subsection (no [I] tags)
- [resolved] Phase2-Uncertain iter2: [DES-NAME] Technical Design 'Implementation Notes' — Accepted as reasonable extension
- [resolved] Phase2-Pending iter2: [AC-GAP] — Not needed. AC#2/AC#3 + AC#4/AC#5 + AC#6/AC#7 sufficiently prove old code replaced
- [resolved] Phase2-Uncertain iter2: [DES-ASYM] DisambiguateNameOrIndex — Accepted. Asymmetry is by design (keyword allowlist opt-in)
- [fix] Phase2-Review iter3: [CON-C5] AC Design Constraints C5 AC Implication | Changed from AC#4,AC#5 to AC#8 for behavioral equivalence verification
- [resolved] Phase2-Uncertain iter3: [AC-METHOD] AC#16/AC#17 — Accepted. No SSOT rule against multi-Grep in Method column
- [resolved] Phase2-Pending iter3: [TEST-COV] ConditionPatternUtility — Transitive coverage sufficient for pure extraction refactoring. 159+ existing tests cover all paths
- [fix] Phase4-ACValidation iter4: [AC-NEG] AC#8 Details | Added positive/negative coverage note documenting engine-type coverage requirement satisfied by existing TalentDisambiguationTests
- [fix] Phase2-Review iter5: [AC-PATH] AC#17 Details | Added ConditionPatternUtility.cs to Test field path (propagating iter6 fix from Definition Table to Details)
- [fix] Phase2-Review iter6: [DEP-STAT] F765 status | Updated from [WIP] to [DONE] in Related Features and Dependencies tables

---

## Links
- [feature-760.md](feature-760.md) - Parent: TALENT target/numeric index support
- [feature-758.md](feature-758.md) - Foundation: VariableRef/VariableConditionParser generic pattern
- [feature-759.md](feature-759.md) - Related: compound bitwise ResolveTalentKey
- [feature-769.md](feature-769.md) - Sibling: Target-only TALENT evaluation
- [feature-765.md](feature-765.md) - Sibling: SELECTCASE ARG pattern
- [feature-761.md](feature-761.md) - Related: LOCAL variable tracking
- [feature-706.md](feature-706.md) - Consumer: full equivalence testing
- [feature-751.md](feature-751.md) - Downstream: TALENT semantic mapping
- [feature-767.md](feature-767.md) - Related: dialogue-schema documentation

