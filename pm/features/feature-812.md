# Feature 812: SOURCE1 Extended

## Status: [DONE]
<!-- fl-reviewed: 2026-02-24T01:12:26Z -->

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

Phase 21: Counter System -- all ERB counter logic migrated to C# with zero technical debt, equivalence-tested against legacy behavior. SOURCE1 Extended is the SSOT for 35 calculation functions (pleasure modifiers, source-to-CUP converters, parameter adjusters, and the SOURCE_EXTRA mega-function) that compute cumulative parameter deltas during counter events, with documented cross-index side effects (see C13). These functions are consumed by the SOURCE Entry System orchestrator (F811) and NTR_UTIL.ERB, requiring a clean ISourceCalculator interface boundary.

### Problem (Current Issue)

F812's DRAFT spec declares only F783 as predecessor and contains 2 generic stub tasks with 1 AC, because F783's Phase 21 decomposition focused on file-prefix grouping rather than content/dependency analysis (feature-783.md:407-422), deferring all implementation-specific analysis to /fc. SOURCE1.ERB contains 35 functions across 1,959 lines with heavy use of TIMES float-to-int truncation (~200+ sites), SELECTCASE dispatch tables, RELATION 2D array access (SOURCE1.ERB:435-443, 1899-1911), EXPLV/PALAMLV threshold lookups, and 8+ external utility dependencies. The C# type system is incomplete: SourceIndex lacks 12+ well-known constants (GivePleasureC/V/A/B at indices 40-43, Seduction/Humiliation/Provocation/Service/Coercion/Sadism at 50-55, Liquid at 9, SexualActivity at 11), CupIndex lacks 6 constants (Goodwill, Superiority, Learning, Lubrication, Shame, Depression), and 3 critical interface gaps exist: no RELATION accessor in any Era.Core interface, no ASSIPLAY accessor in IEngineVariables, and no C# equivalent of the 締り具合名称 (tightness name) function from PRINT_STATE.ERB:724. Additionally, the SOURCE_恭順 function (SOURCE1.ERB:856-886) contains a bug where all 11 SELECTCASE branches use CASE 0, making only the first branch reachable (always producing a 0.50 multiplier).

### Goal (What to Achieve)

Migrate all 35 functions from SOURCE1.ERB (1,959 lines) to C# with: (1) ISourceCalculator interface exposing all 35 functions for F811 and NTR_UTIL consumption, (2) SourceIndex additions for 12+ missing constants, (3) CupIndex additions for 6 missing constants, (4) RELATION accessor (GetRelation) added to an appropriate interface, (5) ASSIPLAY accessor (GetAssiPlay) added to IEngineVariables, (6) 締り具合名称 abstraction (inline threshold mapping or interface method), (7) EXPLV thresholds via hardcoded approach (following PainStateChecker precedent), (8) faithful reproduction of SOURCE_恭順 CASE-0 bug behavior, (9) TIMES truncation fidelity using (int)(value * multiplier) semantics, (10) equivalence tests verifying C# matches ERB for all 35 functions, and (11) zero technical debt.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does F812 DRAFT have only 2 tasks and 1 AC for a 1,959-line ERB file with 35 functions? | Because F783 created all Phase 21 DRAFTs from a minimal template, deferring AC/Task generation to the /fc stage | feature-812.md:48-62, feature-783.md:22-23 |
| 2 | Why did F783 not analyze SOURCE1.ERB's internal complexity? | Because F783 focused on file-prefix decomposition (grouping SOURCE_* files), not function-level or dependency analysis | feature-811.md:49-53 |
| 3 | Why does file-prefix grouping miss the complexity? | Because SOURCE1.ERB has 35 functions with complex ERA-specific patterns (TIMES truncation, SELECTCASE cascading, RELATION array access, EXPLV thresholds, HAS_VAGINA calls) requiring careful C# mapping | SOURCE1.ERB:28-53, 418-526, 1574-1959 |
| 4 | Why can these patterns not be trivially mapped? | Because the C# type system is incomplete: SourceIndex lacks 12+ constants, CupIndex lacks 6, and 3 interface gaps (RELATION, ASSIPLAY, 締り具合名称) block implementation | SourceIndex.cs:31-49, CupIndex.cs:31-41, IVariableStore.cs (no GetRelation) |
| 5 | Why (Root)? | Because the minimal template approach correctly defers hard analysis to /fc, but F812 cannot proceed without comprehensive type-system completion (typed constants) and interface gap resolution (RELATION, ASSIPLAY, 締り具合名称) that were never catalogued during F783's decomposition | SourceIndex.cs, CupIndex.cs, IEngineVariables.cs |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | F812 has 2 generic tasks and 1 AC for a 1,959-line ERB file with 35 functions | F783 decomposition used file-prefix grouping, not content analysis; C# type system incomplete (missing constants and interface gaps) |
| Where | feature-812.md Tasks/AC tables | SourceIndex.cs (12+ missing constants), CupIndex.cs (6 missing), IVariableStore/IEngineVariables (no RELATION/ASSIPLAY accessors) |
| Fix | Add more generic tasks | Enumerate all 35 functions, add typed constants, resolve 3 interface gaps, create ISourceCalculator interface, write per-function equivalence tests |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F783 | [DONE] | Predecessor -- Phase 21 Planning (parent decomposition) |
| F801 | [DONE] | Related -- Established DI pattern and Counter infrastructure (ActionValidator.cs) |
| F803 | [PROPOSED] | Related -- Overlapping SourceIndex constants (F803 plans 12 of same constants) |
| F811 | [BLOCKED] | Successor -- SOURCE.ERB:127-335 calls 30+ F812 functions via CALL; needs ISourceCalculator |
| F813 | [DRAFT] | Successor -- Post-Phase Review Phase 21 |
| F815 | [DONE] | Related -- StubVariableStore test infrastructure |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Source ERB file accessible and analyzed | FEASIBLE | SOURCE1.ERB: 1,959 lines, 35 functions confirmed |
| Functions are pure calculations (no outgoing CALL/GOTO) | FEASIBLE | Zero CALL/GOTO to external ERB; all functions are self-contained |
| Core IVariableStore methods exist | FEASIBLE | GetSource/SetSource, GetCup/SetCup, GetPalamLv, GetPalam, GetExp, GetTalent, GetAbility, GetBase/SetBase, GetDownbase/SetDownbase, GetMark, GetCharacterFlag, GetFlag all exist |
| ICommonFunctions has SourceRevision1/2, GetRevision, HasVagina | FEASIBLE | ICommonFunctions.cs:16-18 |
| ITEquipVariables and IEngineVariables cover remaining needs | FEASIBLE | GetTEquip, GetSelectCom, GetCharaNum, GetCharacterNo, GetPlayer, GetMaster all exist |
| RELATION accessor exists | NEEDS_REVISION | No GetRelation in any Era.Core interface; needed at SOURCE1.ERB:435-443, 1899-1911 |
| ASSIPLAY accessor exists | NEEDS_REVISION | No GetAssiPlay in IEngineVariables; needed at SOURCE1.ERB:1605, 1613 |
| 締り具合名称 function exists in C# | NEEDS_REVISION | No C# equivalent of PRINT_STATE.ERB:724; needed at 8 sites in SOURCE_PleasureV/A |
| EXPLV thresholds accessible | FEASIBLE | Hardcoded approach viable (PainStateChecker precedent) |
| SourceIndex constants complete | NEEDS_REVISION | Missing 12+ constants (GivePleasureC-B, Seduction-Sadism, Liquid, SexualActivity) |
| CupIndex constants complete | NEEDS_REVISION | Missing 6 constants (Goodwill, Superiority, Learning, Lubrication, Shame, Depression) |
| Code volume manageable | FEASIBLE | 35 functions are highly repetitive; ~20 follow identical structure |
| 人物_客 and 人物_訪問者 constants exist | FEASIBLE | Constants.cs:36-37 |
| ERB bug faithful reproduction | FEASIBLE | SOURCE_恭順 bug documented (SOURCE1.ERB:856-886) |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| F811 (SOURCE Entry System) | HIGH | F811 is blocked on F812; needs ISourceCalculator interface for 30+ CALL sites at SOURCE.ERB:127-335 |
| SourceIndex.cs | MEDIUM | 12+ new well-known constants added; overlaps with F803 planned additions |
| CupIndex.cs | MEDIUM | 6 new well-known constants added |
| IEngineVariables / new interface | MEDIUM | ASSIPLAY accessor and RELATION accessor must be added |
| NTR subsystem (future) | LOW | NTR_UTIL.ERB:1242-1266 calls 25 SOURCE1 functions; ISourceCalculator must support multiple callers |
| Era.Core.Tests | MEDIUM | 35 functions require equivalence tests; large test surface but parameterizable |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| TIMES float-to-int truncation semantics | ~200+ TIMES occurrences in SOURCE1.ERB | Must use (int)(value * multiplier) to match ERA behavior |
| SELECTCASE all-CASE-0 bug in SOURCE_恭順 | SOURCE1.ERB:856-886 | Must faithfully reproduce; only first branch (0.50 multiplier) ever fires |
| RELATION is a 2D array indexed by (character, NO:other_character) | SOURCE1.ERB:435-443, 1899-1911 | Requires new interface method (GetRelation) |
| ASSIPLAY is a 1D global array | SOURCE1.ERB:1605, 1613 | Requires new accessor (GetAssiPlay) on IEngineVariables |
| 締り具合名称 returns string for tightness comparison | SOURCE1.ERB:107-114, 179-187 | No C# equivalent; inline threshold mapping or interface method required |
| きゅうきゅう dead branch in SOURCE1.ERB:109 | SOURCE1.ERB:109, 181 | Bug: 締り具合名称 never returns "きゅうきゅう"; branch is always dead. Faithfully reproduce as unreachable code (similar to SOURCE_恭順 bug) |
| EXPLV thresholds (levels 1-5) | SOURCE1.ERB:67-78, 138-150 | Use hardcoded approach (PainStateChecker precedent) |
| Dual-character writes (CUP:master) | SOURCE1.ERB:256, 1062, 1147, 1226-1311 | 6 master-affecting functions write to master character's CUP; both slave/master params needed |
| CHARANUM bounds check pattern | SOURCE1.ERB:435, 751, 1061, etc. | IF master < CHARANUM guard before RELATION/CUP access |
| Cross-function side effects | SOURCE1.ERB:197-198 | Some functions mutate SOURCE arrays of other indices |
| 人物_客 and 人物_訪問者 special character constants | SOURCE1.ERB:753-756 | Constants.cs:36-37 already has these |
| TreatWarningsAsErrors enabled | Directory.Build.props | All C# must compile without warnings |
| Large function count (35) | SOURCE1.ERB | Consider grouping by category (pleasure, source-to-CUP, parameter, extra) |
| SourceIndex constant overlap with F803 | F803 plans 12 of same constants | Whoever runs first adds them; additive-only, no conflict |
| EXP_UP external function | SOURCE1.ERB:20, 22 | ICounterUtilities or direct implementation needed |
| SOURCE1 functions have zero outgoing CALL/GOTO | Full grep confirmed by all 3 investigators | Clean migration scope; no cascading external dependencies |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| TIMES truncation mismatch | HIGH | HIGH | Unit test each TIMES pattern with known inputs; boundary value testing |
| RELATION interface gap blocks implementation | MEDIUM | HIGH | F812 must create GetRelation accessor first (cannot wait for F811, which is blocked) |
| Missing SourceIndex/CupIndex constants cause compile errors | HIGH | MEDIUM | Enumerate all unique indices from ERB before implementation |
| 締り具合名称 no C# equivalent | HIGH | MEDIUM | Inline name-to-multiplier threshold mapping instead of string comparison |
| SOURCE_恭順 CASE-0 bug produces confusing test expectations | MEDIUM | LOW | Document bug explicitly; test that multiplier always equals 0.50 |
| SourceIndex constant conflict with F803 | MEDIUM | LOW | Additive-only changes; whoever runs first adds them |
| ASSIPLAY accessor missing delays implementation | MEDIUM | MEDIUM | Add GetAssiPlay to IEngineVariables within F812 scope |
| EXPLV hardcoded tables diverge from runtime values | LOW | MEDIUM | Use same hardcoded approach as PainStateChecker |
| 35 functions create large test surface | LOW | MEDIUM | Parameterized tests; functions follow repeating patterns |
| NTR_UTIL.ERB second caller requires interface design | LOW | LOW | ISourceCalculator interface naturally supports multiple callers |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| SOURCE1.ERB function count | grep -c "^@SOURCE_" Game/ERB/SOURCE1.ERB | 35 | All 35 must have C# equivalents |
| SOURCE1.ERB line count | wc -l Game/ERB/SOURCE1.ERB | 1959 | Full file scope |
| SourceIndex well-known constants | grep -c "public static readonly SourceIndex" Era.Core/Types/SourceIndex.cs | 19 | Pre-migration baseline |
| CupIndex well-known constants | grep -c "public static readonly CupIndex" Era.Core/Types/CupIndex.cs | 11 | Pre-migration baseline |
| RELATION accessor count | grep -rc "GetRelation" Era.Core/Interfaces/ | 0 | Gap to be resolved |

**Baseline File**: `.tmp/baseline-812.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | All 35 functions must have C# equivalents | SOURCE1.ERB:7-1959 | AC must verify each @SOURCE_* has corresponding C# method |
| C2 | TIMES truncation must match ERA integer semantics | ~200+ TIMES usages in SOURCE1.ERB | AC must test boundary values for (int)(value * multiplier) |
| C3 | SourceIndex must gain 12+ new well-known constants | SourceIndex.cs:31-49 vs SOURCE1.ERB usage | AC must verify all SourceIndex constants referenced in SOURCE1.ERB exist |
| C4 | CupIndex must gain 6 new well-known constants | CupIndex.cs:31-41 vs SOURCE1.ERB usage | AC must verify all CupIndex constants referenced in SOURCE1.ERB exist |
| C5 | ISourceCalculator interface must expose all 35 functions | F811 dependency at SOURCE.ERB:127-335 | AC must verify interface completeness (method count) |
| C6 | RELATION accessor must exist | SOURCE1.ERB:435-443, 1899-1911 | AC must verify GetRelation method added |
| C7 | ASSIPLAY accessor must exist | SOURCE1.ERB:1605, 1613 | AC must verify GetAssiPlay method added |
| C8 | 締り具合名称 abstraction must exist | SOURCE1.ERB:107-114, 179-187 (8 call sites) | AC must verify tightness threshold logic is callable |
| C9 | SOURCE_恭順 bug faithfully reproduced | SOURCE1.ERB:856-886 (all CASE 0, no CASEELSE) | AC must verify: ABL:従順==0 → 0.50 multiplier; ABL:従順>=1 → no CASE matches, no TIMES applied (effective 1.0) |
| C10 | Zero technical debt | Phase 21 requirements | AC must grep TODO/FIXME/HACK |
| C11 | Equivalence tests for all 35 functions | Phase 21 requirements | C# output must match ERB for representative inputs |
| C12 | F801 DI pattern followed | ActionValidator.cs constructor DI | C# classes must use constructor injection |
| C13 | Cross-function side effects preserved | SOURCE1.ERB:197-198 | AC must test cross-index SOURCE mutations |
| C14 | CHARANUM guard pattern correct | SOURCE1.ERB:435, 751, 1061 | Must check if master is valid character before RELATION/CUP access |
| C15 | Interface Dependency Scan: existing interfaces sufficient | IVariableStore, ICommonFunctions, ITEquipVariables, IEngineVariables | AC must verify all existing interface methods used correctly |
| C16 | CharacterFlagIndex and ExpIndex must have typed constants for all SOURCE1.ERB usages | SOURCE1.ERB:20-22 (CFLAG:ローター挿入/ローターA挿入), SOURCE1.ERB:1073,1158 (CFLAG:310), SOURCE1.ERB:12-19 (EXP indices) | AC must verify CharacterFlagIndex and ExpIndex completeness |
| C17 | きゅうきゅう dead branch must be documented | SOURCE1.ERB:109, 181 vs PRINT_STATE.ERB:724 | AC must note dead branch in equivalence test comments |

### Constraint Details

**C1: All 35 Functions Migrated**
- **Source**: Full enumeration of @SOURCE_* functions in SOURCE1.ERB
- **Verification**: grep "^@SOURCE_" SOURCE1.ERB | wc -l == method count in ISourceCalculator
- **AC Impact**: AC must count methods on ISourceCalculator interface

**C2: TIMES Truncation Fidelity**
- **Source**: ERA TIMES operator performs integer multiplication with truncation
- **Verification**: Test with known fractional multipliers (e.g., TIMES 0.50, TIMES 1.20)
- **AC Impact**: Boundary value tests with specific numeric inputs and expected truncated outputs

**C6: RELATION Accessor**
- **Source**: SOURCE1.ERB:435 accesses RELATION:slave:(NO:master); no GetRelation in any Era.Core interface
- **Verification**: grep "GetRelation" Era.Core/Interfaces/ must find method
- **AC Impact**: New interface method must be added and tested

**C8: 締り具合名称 Abstraction**
- **Source**: PRINT_STATE.ERB:724 defines tightness name function; SOURCE1.ERB:107-114 compares string result
- **Verification**: C# must produce equivalent boolean for threshold comparisons
- **AC Impact**: Inline threshold mapping recommended (avoid string comparison fragility)

**C9: SOURCE_恭順 Bug Preservation**
- **Source**: SOURCE1.ERB:856-886; all 11 SELECTCASE branches use CASE 0 and no CASEELSE exists
- **Verification**: ABL:従順==0 → first CASE 0 fires, TIMES 0.50 applied. ABL:従順>=1 → no CASE matches (no CASEELSE), no TIMES applied, effective multiplier 1.0
- **AC Impact**: Explicit test documenting ERB bug; two test cases required (ABL==0 and ABL>=1)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| Successor | F811 | [BLOCKED] | SOURCE.ERB:127-335 CALLs 30+ F812 functions; needs ISourceCalculator interface |
| Successor | F813 | [DRAFT] | Post-Phase Review Phase 21 |
| Related | F801 | [DONE] | Counter infrastructure; established DI pattern (ActionValidator.cs) |
| Related | F803 | [PROPOSED] | Overlapping SourceIndex constants (F803 plans 12 of same); data dependency only |
| Related | F815 | [DONE] | StubVariableStore test infrastructure |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block

Example:
| Predecessor | F540 | [DONE] | Era.Core setup required |
| Successor | F567 | [BLOCKED] | F567 depends on this feature |
| Related | F100 | [DONE] | Reference implementation |
-->

---

<!-- fc-phase-2-completed -->
<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "all ERB counter logic migrated to C# with zero technical debt" | All 35 SOURCE1.ERB functions must have C# equivalents; no TODO/FIXME/HACK; no duplicated data tables | AC#1, AC#2, AC#14, AC#24, AC#25, AC#26 |
| "SOURCE1 Extended is the SSOT for 35 pure-calculation functions" | ISourceCalculator interface must expose all 35 functions | AC#1, AC#2 |
| "equivalence-tested against legacy behavior" | Equivalence tests must exist for all 35 functions, including cross-index side effects (C13) | AC#15, AC#19, AC#28 |
| "requiring a clean ISourceCalculator interface boundary" | ISourceCalculator interface file must exist with proper DI pattern and DI registration | AC#1, AC#3, AC#21 |
| "12+ well-known constants" for SourceIndex | SourceIndex must gain at least 12 new constants (total >= 31) | AC#4 |
| "6 constants" for CupIndex | CupIndex must gain at least 6 new constants (total >= 17) | AC#5 |
| "no RELATION accessor in any Era.Core interface" | GetRelation method must be added with stub and DI registration | AC#6, AC#17, AC#18 |
| "no ASSIPLAY accessor in IEngineVariables" | GetAssiPlay method must be added; backward compatibility preserved | AC#7, AC#16 |
| "no C# equivalent of the tightness name function" | Tightness threshold logic must be callable in C# | AC#8, AC#27 |
| "faithful reproduction of SOURCE_Obedience CASE-0 bug behavior" | SOURCE_Obedience must always apply 0.50 multiplier when ABL==0; no TIMES when ABL>=1 | AC#9, AC#29 |
| "TIMES truncation fidelity" | TIMES patterns must use (int) cast for truncation | AC#10 |
| "zero technical debt" | No TODO/FIXME/HACK in new files | AC#14 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ISourceCalculator interface file exists | file | Glob(Era.Core/Counter/ISourceCalculator.cs) | exists | - | [x] |
| 2 | ISourceCalculator declares 35 methods | code | Grep(Era.Core/Counter/ISourceCalculator.cs, "void Source") | count_equals | 35 | [x] |
| 3 | SourceCalculator uses constructor DI | code | Grep(Era.Core/Counter/SourceCalculator.cs) | matches | `public sealed class SourceCalculator\(` | [x] |
| 4 | SourceIndex has >= 31 well-known constants | code | Grep(Era.Core/Types/SourceIndex.cs, "public static readonly SourceIndex") | gte | 31 | [x] |
| 5 | CupIndex has >= 17 well-known constants | code | Grep(Era.Core/Types/CupIndex.cs, "public static readonly CupIndex") | gte | 17 | [x] |
| 6 | GetRelation accessor in Era.Core interface | code | Grep(Era.Core/Interfaces/) | matches | `GetRelation\(` | [x] |
| 7 | GetAssiPlay accessor in IEngineVariables | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | `GetAssiPlay\(` | [x] |
| 8 | Tightness threshold logic in SourceCalculator | code | Grep(Era.Core/Counter/SourceCalculator.cs) | matches | `Tightness|tightness` | [x] |
| 9 | SOURCE_Obedience bug test passes | test | dotnet test --filter "FullyQualifiedName~SourceCalculatorTests&FullyQualifiedName~SourceObedience" | succeeds | - | [x] |
| 10 | TIMES truncation uses int cast | code | Grep(Era.Core/Counter/SourceCalculator.cs) | matches | `\(int\)\(` | [x] |
| 11 | CHARANUM guard before RELATION access | code | Grep(Era.Core/Counter/SourceCalculator.cs, "master\\.Value < engine\\.GetCharaNum\\(\\)") | gte | 6 | [x] |
| 12 | Dual-character CUP writes for master | code | Grep(Era.Core/Counter/SourceCalculator.cs, "AddCup\\(master") | gte | 6 | [x] |
| 13 | Era.Core builds without errors | build | dotnet build Era.Core/ | succeeds | - | [x] |
| 14 | No technical debt markers in F812 files | code | Grep(Era.Core/Counter/, Era.Core/Types/SourceIndex.cs, Era.Core/Types/CupIndex.cs, Era.Core/Types/CharacterFlagIndex.cs, Era.Core/Types/ExpIndex.cs, Era.Core/Interfaces/IRelationVariables.cs, Era.Core/Interfaces/NullRelationVariables.cs, Era.Core/Interfaces/IEngineVariables.cs, Era.Core/Interfaces/NullEngineVariables.cs, Era.Core/Character/PainStateChecker.cs) | not_matches | `TODO|FIXME|HACK` | [x] |
| 15 | All equivalence tests pass | test | dotnet test Era.Core.Tests/ --filter FullyQualifiedName~SourceCalculatorTests | succeeds | - | [x] |
| 16 | IEngineVariables pre-existing methods preserved | code | Grep(Era.Core/Interfaces/IEngineVariables.cs, "(Get|Set)\\w+\\(") | gte | 25 | [x] |
| 17 | NullRelationVariables stub file exists | file | Glob(Era.Core/Interfaces/NullRelationVariables.cs) | exists | - | [x] |
| 18 | IRelationVariables DI registration exists | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `IRelationVariables` | [x] |
| 19 | Equivalence tests exist for all 35 functions | code | Grep(Era.Core.Tests/Counter/SourceCalculatorTests.cs, "public.*void.*Source") | gte | 35 | [x] |
| 20 | EXPLV threshold array exists in shared ExpLvTable | code | Grep(Era.Core/Counter/ExpLvTable.cs) | matches | `Thresholds` | [x] |
| 21 | ISourceCalculator DI registration exists | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `ISourceCalculator` | [x] |
| 22 | engine-dev SKILL.md documents ISourceCalculator | code | Grep(.claude/skills/engine-dev/SKILL.md) | matches | `ISourceCalculator` | [x] |
| 23 | engine-dev SKILL.md documents IRelationVariables | code | Grep(.claude/skills/engine-dev/SKILL.md) | matches | `IRelationVariables` | [x] |
| 24 | Shared ExpLvTable class exists | file | Glob(Era.Core/Counter/ExpLvTable.cs) | exists | - | [x] |
| 25 | CharacterFlagIndex has SOURCE1-required constants | code | Grep(Era.Core/Types/CharacterFlagIndex.cs, "RotatorVInsertion\|RotatorAInsertion\|MansionRank") | gte | 3 | [x] |
| 26 | ExpIndex has SOURCE1-required constants | code | Grep(Era.Core/Types/ExpIndex.cs, "public static readonly ExpIndex") | gte | 4 | [x] |
| 27 | Tightness boundary tests pass | test | dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~SourceCalculatorTests&FullyQualifiedName~Tightness" | succeeds | - | [x] |
| 28 | Cross-index SOURCE mutation test exists | code | Grep(Era.Core.Tests/Counter/SourceCalculatorTests.cs, "SourcePleasureA.*CrossIndex\|SourcePleasureA.*Submission\|CrossIndex.*SourcePleasureA") | gte | 1 | [x] |
| 29 | SOURCE_Obedience both branches tested | code | Grep(Era.Core.Tests/Counter/SourceCalculatorTests.cs, "SourceObedience_Abl") | gte | 2 | [x] |
| 30 | SourceCalculator references shared ExpLvTable | code | Grep(Era.Core/Counter/SourceCalculator.cs, "ExpLvTable\\.Thresholds") | matches | - | [x] |
| 31 | RELATION boundary test with non-neutral value | code | Grep(Era.Core.Tests/Counter/SourceCalculatorTests.cs, "Relation.*[^1]0\|GetRelation.*(?!100)\\d") | gte | 1 | [x] |
| 32 | No private EXPLV copy in SourceCalculator after Task#9 | code | Grep(Era.Core/Counter/SourceCalculator.cs, "ExpLvThresholds\|private.*int.*\\[\\].*Exp") | not_matches | - | [x] |

### AC Details

**AC#1: ISourceCalculator interface file exists**
- **Test**: `Glob("Era.Core/Counter/ISourceCalculator.cs")`
- **Expected**: File exists
- **Rationale**: C5 constraint requires ISourceCalculator interface for F811 consumption. Interface file must be a separate file following Era.Core interface conventions. [C5]

**AC#2: ISourceCalculator declares 35 methods**
- **Test**: `Grep(path="Era.Core/Counter/ISourceCalculator.cs", pattern="void Source")` count method declarations
- **Expected**: count_equals 35 (one method per ERB function: SOURCE_EXP through SOURCE_DOWNBASE)
- **Rationale**: C1 and C5 constraints require 1:1 mapping. Each @SOURCE_ function in SOURCE1.ERB must have a corresponding method named `Source*`. The complete method list is defined in Technical Design (ISourceCalculator interface spec, lines 526-561): SourceExp, SourcePleasureC, SourcePleasureV, SourcePleasureA, SourcePleasureB, SourceGivePleasureC, SourceGivePleasureV, SourceGivePleasureA, SourceGivePleasureB, SourceCvabExtra, SourceAffection, SourceSexualActivity, SourceAchievement, SourcePain, SourceFear, SourceLiquid, SourceArousal, SourceObedience, SourceExposure, SourceSubmission, SourcePleasureEnjoyment, SourceConquest, SourcePassive, SourceSeduction, SourceHumiliation, SourceProvocation, SourceService, SourceCoercion, SourceSadism, SourceFilth, SourceDepression, SourceDeviation, SourceAntipathy, SourceExtra, SourceDownbase. Implementation MUST match this exact set. [C1, C5]

**AC#3: SourceCalculator uses constructor DI**
- **Test**: `Grep(path="Era.Core/Counter/SourceCalculator.cs", pattern="public sealed class SourceCalculator\(")`
- **Expected**: matches (primary constructor DI pattern per F801 ActionValidator precedent)
- **Rationale**: C12 constraint requires F801 DI pattern. ActionValidator uses primary constructor syntax. [C12]

**AC#4: SourceIndex has >= 31 well-known constants**
- **Test**: `Grep(path="Era.Core/Types/SourceIndex.cs", pattern="public static readonly SourceIndex")` count occurrences
- **Expected**: gte 31 (baseline 19 + at least 12 new: GivePleasureC/V/A/B, Seduction, Humiliation, Provocation, Service, Coercion, Sadism, Liquid, SexualActivity)
- **Rationale**: C3 constraint. SOURCE1.ERB uses 12+ indices not in current SourceIndex. Baseline confirmed 19. [C3]

**AC#5: CupIndex has >= 17 well-known constants**
- **Test**: `Grep(path="Era.Core/Types/CupIndex.cs", pattern="public static readonly CupIndex")` count occurrences
- **Expected**: gte 17 (baseline 11 + at least 6 new: Goodwill, Superiority, Learning, Lubrication, Shame, Depression)
- **Rationale**: C4 constraint. SOURCE1.ERB writes CUP indices with no CupIndex constants. Baseline confirmed 11. [C4]

**AC#6: GetRelation accessor in Era.Core interface**
- **Test**: `Grep(path="Era.Core/Interfaces/", pattern="GetRelation\(")`
- **Expected**: matches at least 1 file
- **Rationale**: C6 constraint. SOURCE1.ERB:435-443 and 1899-1911 access RELATION 2D array. No GetRelation in any Era.Core interface (confirmed). [C6]

**AC#7: GetAssiPlay accessor in IEngineVariables**
- **Test**: `Grep(path="Era.Core/Interfaces/IEngineVariables.cs", pattern="GetAssiPlay\(")`
- **Expected**: matches
- **Rationale**: C7 constraint. SOURCE1.ERB:1605, 1613 read ASSIPLAY global. No GetAssiPlay in IEngineVariables (confirmed). [C7]

**AC#8: Tightness threshold logic in SourceCalculator**
- **Test**: `Grep(path="Era.Core/Counter/SourceCalculator.cs", pattern="Tightness|tightness")`
- **Expected**: matches
- **Rationale**: C8 constraint. SOURCE1.ERB:107-114, 179-187 compare tightness name strings. C# must implement equivalent inline threshold mapping without string comparison. [C8]
- **Note**: Task#5 MUST include tightness boundary test cases per PRINT_STATE.ERB:724: vLooseness=99→Normal (no multiplier), 100→Tight (0.85), 249→Tight, 250→Normal (ゆるゆる, no multiplier), 449→Normal, 450→Loose (0.55), 699→Loose, 700→VeryLoose (0.40). The 'きゅうきゅう' dead branch (SOURCE1.ERB:109) must be documented but produces no observable behavior change.

**AC#9: SOURCE_Obedience bug test passes**
- **Test**: `dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~SourceCalculatorTests&FullyQualifiedName~SourceObedience"`
- **Expected**: succeeds (test verifies: ABL==0 produces 0.50 multiplier; ABL>=1 produces effective 1.0 due to no CASE match)
- **Rationale**: C9 constraint. SOURCE1.ERB:856-886 has all 11 SELECTCASE branches using CASE 0 and no CASEELSE. Only first CASE 0 fires (0.50). When ABL>=1, no TIMES applied. [C9]

**AC#10: TIMES truncation uses int cast**
- **Test**: `Grep(path="Era.Core/Counter/SourceCalculator.cs", pattern="\(int\)\(")`
- **Expected**: matches (ERA TIMES operator performs integer truncation)
- **Rationale**: C2 constraint. ~200+ TIMES usages require (int)(value * multiplier) semantics. [C2]

**AC#11: CHARANUM guard before RELATION access**
- **Test**: `Grep(path="Era.Core/Counter/SourceCalculator.cs", pattern="master\\.Value < engine\\.GetCharaNum\\(\\)")`
- **Expected**: gte 6 (6 master-affecting functions + RELATION access sites each require a CHARANUM guard)
- **Rationale**: C14 constraint. SOURCE1.ERB:435, 751, 1061 use IF master < CHARANUM guard before RELATION/CUP access. C# must reproduce bounds check for all 6 dual-character functions plus RELATION access sites. Count gte 6 ensures the guard is not merely present once but structurally covers every master-write function. [C14]

**AC#12: Dual-character CUP writes for master**
- **Test**: `Grep(path="Era.Core/Counter/SourceCalculator.cs", pattern="AddCup\(master")`
- **Expected**: gte 6 (6 master-affecting functions each write to master CUP via AddCup helper)
- **Rationale**: Technical Constraint: SOURCE1.ERB:256, 1062, 1147, 1226-1311 write CUP for master character. The 6 master-affecting functions are SOURCE_誘惑, SOURCE_辱め, SOURCE_挑発, SOURCE_奉仕, SOURCE_強要, SOURCE_加虐. SOURCE_恭順 (line 856-886) is excluded because it only writes CUP:奴隷:恭順 (slave CUP, line 886), not master CUP. [C14]

**AC#13: Era.Core builds without errors**
- **Test**: `dotnet build Era.Core/`
- **Expected**: succeeds (TreatWarningsAsErrors enabled per Directory.Build.props)
- **Rationale**: Gate for all code changes. New C# must compile without warnings.

**AC#14: No technical debt markers in F812 files**
- **Test**: `Grep(path="Era.Core/Counter/", pattern="TODO|FIXME|HACK")` + `Grep(path="Era.Core/Types/SourceIndex.cs", pattern="TODO|FIXME|HACK")` + `Grep(path="Era.Core/Types/CupIndex.cs", pattern="TODO|FIXME|HACK")` + `Grep(path="Era.Core/Types/CharacterFlagIndex.cs", pattern="TODO|FIXME|HACK")` + `Grep(path="Era.Core/Types/ExpIndex.cs", pattern="TODO|FIXME|HACK")` + `Grep(path="Era.Core/Interfaces/IRelationVariables.cs", pattern="TODO|FIXME|HACK")` + `Grep(path="Era.Core/Interfaces/NullRelationVariables.cs", pattern="TODO|FIXME|HACK")` + `Grep(path="Era.Core/Interfaces/IEngineVariables.cs", pattern="TODO|FIXME|HACK")` + `Grep(path="Era.Core/Interfaces/NullEngineVariables.cs", pattern="TODO|FIXME|HACK")` + `Grep(path="Era.Core/Character/PainStateChecker.cs", pattern="TODO|FIXME|HACK")`
- **Expected**: not_matches across all 10 F812-touched files (Era.Core/Counter/ dir + SourceIndex.cs + CupIndex.cs + CharacterFlagIndex.cs + ExpIndex.cs + IRelationVariables.cs + NullRelationVariables.cs + IEngineVariables.cs + NullEngineVariables.cs + PainStateChecker.cs)
- **Rationale**: C10 and Philosophy require zero technical debt. Covers all 10 F812-touched files including CharacterFlagIndex.cs and ExpIndex.cs (modified by Task#10) and PainStateChecker.cs (modified by Task#9 ExpLvTable extraction). [C10]

**AC#15: All equivalence tests pass**
- **Test**: `dotnet test Era.Core.Tests/ --filter FullyQualifiedName~SourceCalculatorTests`
- **Expected**: succeeds (equivalence tests for all 35 functions)
- **Rationale**: C11 constraint. Each function requires equivalence testing with representative inputs and TIMES truncation boundary values. [C11]

**AC#16: IEngineVariables pre-existing methods preserved**
- **Test**: `Grep(path="Era.Core/Interfaces/IEngineVariables.cs", pattern="(Get|Set)\\w+\\(")` count occurrences
- **Expected**: count_gte 25 (24 existing + 1 new GetAssiPlay = 25 after F812)
- **Rationale**: Backward compatibility: extending IEngineVariables with GetAssiPlay must not remove existing 24 methods. [C15]

**AC#17: NullRelationVariables stub file exists**
- **Test**: `Glob("Era.Core/Interfaces/NullRelationVariables.cs")`
- **Expected**: File exists
- **Rationale**: V2 Sub-Deliverables: Task 3 creates NullRelationVariables.cs as a stub returning Success(100). Must verify file was actually created. [C6]

**AC#18: IRelationVariables DI registration exists**
- **Test**: `Grep(path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs", pattern="IRelationVariables")`
- **Expected**: matches
- **Rationale**: IRelationVariables must be registered in DI for SourceCalculator constructor injection to work. [C6]

**AC#19: Equivalence tests exist for all 35 functions**
- **Test**: `Grep(path="Era.Core.Tests/Counter/SourceCalculatorTests.cs", pattern="public.*void.*Source")` count unique test methods containing "Source"
- **Expected**: gte 35 (at least one test method per SOURCE_ function; each test method name must contain the C# function name)
- **Naming Convention**: Each test method MUST start with the corresponding C# method name (e.g., `SourceExp_...`, `SourcePleasureC_...`, `SourceDownbase_...`). Helper methods must NOT use `Source` prefix. This ensures `public.*void.*Source` counts only per-function tests.
- **Rationale**: AC#15 verifies tests pass but not that tests exist for all 35 functions. C11 requires "Each function requires equivalence testing." Counting test method signatures (not attributes) ensures [Theory] tests are not double-counted and each function has a dedicated test. [C11]

**AC#20: EXPLV threshold array exists in shared ExpLvTable**
- **Test**: `Grep(path="Era.Core/Counter/ExpLvTable.cs", pattern="Thresholds")`
- **Expected**: matches (shared static threshold array declaration exists)
- **Rationale**: Goal item 7 requires "EXPLV thresholds via hardcoded approach (following PainStateChecker precedent)." Extracted to shared ExpLvTable.cs per DEBT-001 resolution. AC#24 verifies file existence; this AC verifies the Thresholds field is declared. [Goal item 7]

**AC#21: ISourceCalculator DI registration exists**
- **Test**: `Grep(path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs", pattern="ISourceCalculator")`
- **Expected**: matches
- **Rationale**: Task#7 registers `services.AddSingleton<ISourceCalculator, SourceCalculator>()`. AC#18 only verifies IRelationVariables registration. This AC verifies the primary interface (ISourceCalculator) is also registered for F811 consumption via DI. [C5, C12]

**AC#22: engine-dev SKILL.md documents ISourceCalculator**
- **Test**: `Grep(path=".claude/skills/engine-dev/SKILL.md", pattern="ISourceCalculator")`
- **Expected**: matches
- **Rationale**: SSOT update rules require new interfaces to be documented in engine-dev SKILL.md. ISourceCalculator is a new Counter interface created by F812.

**AC#23: engine-dev SKILL.md documents IRelationVariables**
- **Test**: `Grep(path=".claude/skills/engine-dev/SKILL.md", pattern="IRelationVariables")`
- **Expected**: matches
- **Rationale**: SSOT update rules require new interfaces to be documented in engine-dev SKILL.md. IRelationVariables is a new Era.Core interface created by F812.

**AC#24: Shared ExpLvTable class exists**
- **Test**: `Glob("Era.Core/Counter/ExpLvTable.cs")`
- **Expected**: File exists
- **Rationale**: DEBT-001 resolution. PainStateChecker and SourceCalculator both use the same 19-element EXPLV threshold array. Extracting to a shared static class eliminates 2-way duplication and prevents future copies. [C10]

**AC#25: CharacterFlagIndex has SOURCE1-required constants**
- **Test**: `Grep(path="Era.Core/Types/CharacterFlagIndex.cs", pattern="RotatorVInsertion|RotatorAInsertion|MansionRank")` count occurrences
- **Expected**: gte 3 (RotatorVInsertion(15), RotatorAInsertion(16), and MansionRank(310))
- **Rationale**: C16 constraint. SOURCE1.ERB:20-22 uses CFLAG:ローター挿入(15) and CFLAG:ローターA挿入(16). SOURCE1.ERB:1073,1158 uses CFLAG:310. Zero debt requires typed constants. [C16]

**AC#26: ExpIndex has SOURCE1-required constants**
- **Test**: `Grep(path="Era.Core/Types/ExpIndex.cs", pattern="public static readonly ExpIndex")` count occurrences
- **Expected**: gte 4 (baseline 1 AExp + at least 3 new: CExp(0), VExp(1), BExp(3))
- **Rationale**: C16 constraint. SOURCE1.ERB:12-19 uses Ｃ経験(0), Ｖ経験(1), Ａ経験(2=existing AExp), Ｂ経験(3). Zero debt requires typed constants. [C16]

**AC#27: Tightness boundary tests pass**
- **Test**: `dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~SourceCalculatorTests&FullyQualifiedName~Tightness"`
- **Expected**: succeeds (boundary tests: vLooseness=99→Normal/no-multiplier, 100→Tight/0.85, 250→Normal/no-multiplier, 450→Loose/0.55, 700→VeryLoose/0.40)
- **Rationale**: AC#8 only verifies keyword presence of `Tightness` in SourceCalculator.cs. This AC verifies the actual tightness boundary threshold values (from PRINT_STATE.ERB:724) are correct by running dedicated boundary tests. [C8]

**AC#28: Cross-index SOURCE mutation test exists**
- **Test**: `Grep(path="Era.Core.Tests/Counter/SourceCalculatorTests.cs", pattern="SourcePleasureA.*CrossIndex|SourcePleasureA.*Submission|CrossIndex.*SourcePleasureA")` count occurrences
- **Expected**: gte 1 (at least one test method verifying SourcePleasureA's cross-index SOURCE:屈従/=3 mutation)
- **Rationale**: C13 constraint requires faithful reproduction of cross-function side effects (SOURCE1.ERB:197-198). AC#15 is generic (all tests pass); this AC verifies the specific cross-mutation test exists in the test file. [C13]

**AC#29: SOURCE_Obedience both branches tested**
- **Test**: `Grep(path="Era.Core.Tests/Counter/SourceCalculatorTests.cs", pattern="SourceObedience_Abl")` count occurrences
- **Expected**: gte 2 (SourceObedience_AblZero_... and SourceObedience_AblOne_... test methods)
- **Rationale**: C9 constraint requires verifying both ABL==0 (0.50 multiplier) and ABL>=1 (no TIMES, effective 1.0) behavioral branches. AC#9 only verifies a test named SourceObedience passes; this AC ensures both branches have dedicated test methods. [C9]

**AC#30: SourceCalculator references shared ExpLvTable**
- **Test**: `Grep(path="Era.Core/Counter/SourceCalculator.cs", pattern="ExpLvTable\\.Thresholds")`
- **Expected**: matches (at least one reference to shared ExpLvTable.Thresholds)
- **Rationale**: DEBT-001 resolution requires SourceCalculator.cs to reference the shared ExpLvTable.Thresholds instead of maintaining a private EXPLV copy. AC#20 and AC#24 verify ExpLvTable.cs exists with Thresholds field, but do not verify SourceCalculator actually uses it. This AC closes the gap, ensuring the 3-way duplication is eliminated. [C10, DEBT-001]

**AC#31: RELATION boundary test with non-neutral value**
- **Test**: `Grep(path="Era.Core.Tests/Counter/SourceCalculatorTests.cs", pattern="Relation.*[^1]0|GetRelation.*(?!100)\\d")` count occurrences
- **Expected**: gte 1 (at least one test exercises a non-neutral RELATION value)
- **Rationale**: SOURCE1.ERB:435-443, 1899-1911 use RELATION as a multiplier divisor in SourcePleasureC/V/A/B. NullRelationVariables returns 100 (neutral=1.0x). Without a non-neutral RELATION test (e.g., 50 or 150), the multiplication logic cannot be verified for correctness. Analogous to AC#29 (obedience branches) and AC#27 (tightness boundaries). [C6]

**AC#32: No private EXPLV copy in SourceCalculator after Task#9**
- **Test**: `Grep(path="Era.Core/Counter/SourceCalculator.cs", pattern="ExpLvThresholds|private.*int.*\\[\\].*Exp")`
- **Expected**: not_matches (private EXPLV copy must be removed after Task#9 extraction)
- **Rationale**: Task#6 initially creates a private ExpLvThresholds array. Task#9 extracts to shared ExpLvTable.cs. AC#30 verifies SourceCalculator references ExpLvTable.Thresholds, but does not verify the private copy was deleted. This AC closes the DEBT-001 gap: without it, both private array and shared reference could coexist, maintaining duplication. [C10, DEBT-001]

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | ISourceCalculator interface exposing all 35 functions | AC#1, AC#2, AC#3, AC#21 |
| 2 | SourceIndex additions for 12+ missing constants | AC#4 |
| 3 | CupIndex additions for 6 missing constants | AC#5 |
| 4 | RELATION accessor (GetRelation) added to appropriate interface | AC#6, AC#17, AC#18, AC#31 |
| 5 | ASSIPLAY accessor (GetAssiPlay) added to IEngineVariables | AC#7, AC#16 |
| 6 | Tightness abstraction (inline threshold mapping) | AC#8, AC#15, AC#27 |
| 7 | EXPLV thresholds via hardcoded approach | AC#15, AC#20, AC#30 |
| 8 | Faithful reproduction of SOURCE_Obedience CASE-0 bug | AC#9, AC#29 |
| 9 | TIMES truncation fidelity | AC#10 |
| 10 | Equivalence tests for all 35 functions | AC#15, AC#19 |
| 11 | Zero technical debt | AC#13, AC#14 |
| C13 | Cross-function side effects preserved (cross-index SOURCE mutations) | AC#15, AC#28 |
| C14 | CHARANUM guard and dual-character CUP writes (Technical Constraint) | AC#11, AC#12 |
| SSOT | engine-dev SKILL.md documentation updates | AC#22, AC#23 |
| DEBT-001 | Shared ExpLvTable eliminates EXPLV duplication (Zero Debt Upfront) | AC#24, AC#30, AC#32 |
| CONST-001 | CharacterFlagIndex/ExpIndex typed constants (Zero Debt Upfront) | AC#25, AC#26 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Migrate all 35 @SOURCE_* functions from SOURCE1.ERB (1,959 lines) into a single `SourceCalculator` class in `Era.Core/Counter/`, exposed via a new `ISourceCalculator` interface. The class follows the F801 primary constructor DI pattern (ActionValidator precedent). Before implementing any SOURCE_ function, three prerequisite infrastructure tasks are performed within this feature's scope: (1) add 12 missing SourceIndex well-known constants, (2) add 6 missing CupIndex well-known constants, and (3) add `GetRelation(CharacterId, int)` to a new `IRelationVariables` interface (ISP segregation), and `GetAssiPlay()` to `IEngineVariables`.

The tightness string comparison in SOURCE1.ERB (締り具合名称 at lines 107-114 and 179-187) is replaced by an inline integer threshold comparison on `BASE:奴隷:Ｖ緩さ`, using the same threshold table defined in PRINT_STATE.ERB:724. This eliminates fragile string comparison and avoids adding a new interface method. The thresholds mirror PRINT_STATE.ERB:724: <100=Normal (ぎちぎち, no multiplier), 100-249=Tight (きゅっきゅっ, 0.85), 250-449=Normal (ゆるゆる, not compared in SOURCE1.ERB), 450-699=Loose (がばがば, 0.55), >=700=VeryLoose (ぽっかり, 0.40). SOURCE1.ERB:109 compares against 'きゅうきゅう' which is never returned by 締り具合名称 — this is a dead-code bug (similar to SOURCE_恭順 CASE-0 bug) that is faithfully reproduced as an unreachable branch.

EXPLV thresholds follow the PainStateChecker precedent: hardcoded private constants array `private static readonly int[] ExpLvThresholds = { 0, 1, 4, 20, 50, 200, 500, 1000, 2000, 3000, 5000, 8000, 12000, 18000, 25000, 35000, 50000, 70000, 100000 }` (19 elements, from OPTION_2.ERB lines 43-54; SOURCE1.ERB uses indices 1-5 only). These thresholds are extracted into a shared `ExpLvTable` class (resolving DEBT-001 duplication with PainStateChecker). These are used in SOURCE_快Ｖ, SOURCE_快Ａ, SOURCE_快Ｂ, and related functions.

TIMES float-to-int truncation is reproduced with `(int)(value * multiplier)` cast pattern throughout. All ~200+ TIMES sites in SOURCE1.ERB are rendered with this pattern.

The SOURCE_恭順 all-CASE-0 bug is faithfully reproduced: only the first CASE 0 branch (0.50 multiplier) is reachable. The C# implementation uses a direct `if (abl == 0)` condition for the first branch only, with no else, exactly matching the ERB's observable behavior: ABL==0 fires the first CASE 0 (TIMES 0.50); ABL>=1 matches no CASE, no TIMES applied (effective multiplier 1.0). A `// ERB bug: all CASE 0 — only first CASE 0 fires` comment documents this intentional fidelity.

The 6 dual-character (master-affecting) functions (SOURCE_誘惑, SOURCE_辱め, SOURCE_挑発, SOURCE_奉仕, SOURCE_強要, SOURCE_加虐) write CUP to the master character in addition to the slave. These functions accept both `CharacterId slave` and `CharacterId master` parameters. The CHARANUM bounds check (`if (master.Value < engine.GetCharaNum())`) guards all RELATION access and master-CUP writes.

`EXP_UP` (SOURCE1.ERB:20, 22) is an external function defined in COMMON.ERB:136-138 with `#FUNCTION` directive. It computes `EXP:(ARG:1):ARG - TCVAR:(ARG:1):(400 + ARG)` (experience minus training counter variable). Since it is called only twice in SOURCE_EXP (lines 20, 22) and its logic is a simple subtraction, it is inlined in SourceExp rather than adding an interface dependency.

DI registration adds `services.AddSingleton<ISourceCalculator, SourceCalculator>()` in ServiceCollectionExtensions.cs.

This approach satisfies all 32 ACs: the ISourceCalculator file (AC#1), 35 method count (AC#2), DI constructor pattern (AC#3), SourceIndex count >= 31 (AC#4), CupIndex count >= 17 (AC#5), GetRelation in interface (AC#6), GetAssiPlay in IEngineVariables (AC#7), tightness inline logic (AC#8), Obedience bug test (AC#9), (int) cast pattern (AC#10), GetCharaNum guard (AC#11), SetCup master writes (AC#12), build success (AC#13), no debt markers (AC#14), equivalence tests pass (AC#15), IEngineVariables backward compatibility (AC#16), NullRelationVariables file exists (AC#17), IRelationVariables DI registration (AC#18), equivalence test count (AC#19), EXPLV threshold array (AC#20), ISourceCalculator DI registration (AC#21), engine-dev ISourceCalculator documentation (AC#22), engine-dev IRelationVariables documentation (AC#23), shared ExpLvTable (AC#24), CharacterFlagIndex typed constants (AC#25), ExpIndex typed constants (AC#26), tightness boundary tests (AC#27), cross-index mutation test (AC#28), Obedience both branches tested (AC#29), SourceCalculator references ExpLvTable (AC#30), RELATION boundary test (AC#31), no private EXPLV copy (AC#32).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core/Counter/ISourceCalculator.cs` — new interface file |
| 2 | Declare all 35 methods on ISourceCalculator: `void SourceExp(CharacterId slave, CharacterId master)` through `void SourceDownbase(CharacterId slave, CharacterId master)` |
| 3 | `public sealed class SourceCalculator(IVariableStore variables, IEngineVariables engine, ITEquipVariables tequip, ICommonFunctions common, IRelationVariables relation)` — primary constructor DI |
| 4 | Add 12 constants to SourceIndex.cs: GivePleasureC (40), GivePleasureV (41), GivePleasureA (42), GivePleasureB (43), Seduction (50), Humiliation (51), Provocation (52), Service (53), Coercion (54), Sadism (55), Liquid (9), SexualActivity (11) — total will be 19 + 12 = 31 |
| 5 | Add 6 constants to CupIndex.cs: Goodwill (5), Superiority (6), Learning (7), Lubrication (9), Shame (13), Depression (32) — total will be 11 + 6 = 17 |
| 6 | Create `Era.Core/Interfaces/IRelationVariables.cs` with `Result<int> GetRelation(CharacterId character, int otherCharacterNo)` method; inject into SourceCalculator |
| 7 | Add `int GetAssiPlay()` to `IEngineVariables.cs`; implement in `NullEngineVariables.cs` returning 0 |
| 8 | Inline `GetTightness(int vLooseness)` private method in SourceCalculator returning `TightnessLevel` enum (Normal/Tight/Loose/VeryLoose); used in SOURCE_快Ｖ and SOURCE_快Ａ. Thresholds per PRINT_STATE.ERB:724: 100/250/450/700 |
| 9 | Test `SourceObedienceTests`: `SourceObedience_AblZero_AppliesHalfMultiplier` and `SourceObedience_AblOne_AppliesNoMultiplier` — verifies CUP:恭順 values |
| 10 | All TIMES operations rendered as `(int)(value * X)` where X is the double multiplier (e.g., 0.50, 1.20) — matches ~200+ TIMES sites |
| 11 | CHARANUM guard `if (master.Value < engine.GetCharaNum())` before every RELATION access and master CUP write site |
| 12 | Dual-character functions call `variables.SetCup(masterChar, CupIndex.X, ...)` — 6 functions write master CUP |
| 13 | All new C# must pass `dotnet build Era.Core/` with TreatWarningsAsErrors |
| 14 | No TODO/FIXME/HACK in `Era.Core/Counter/SourceCalculator.cs` |
| 15 | Equivalence tests in `Era.Core.Tests/Counter/SourceCalculatorTests.cs` for all 35 functions with representative inputs; TDD RED→GREEN |
| 16 | After adding GetAssiPlay(), IEngineVariables must retain all 24 existing methods (total >= 25) |
| 17 | Create `Era.Core/Interfaces/NullRelationVariables.cs` implementing IRelationVariables with `GetRelation` returning `Success(100)` |
| 18 | Register `services.AddSingleton<IRelationVariables, NullRelationVariables>()` in `ServiceCollectionExtensions.cs` alongside ISourceCalculator registration |
| 19 | Write at least 35 test methods (one per SOURCE_ function) in `Era.Core.Tests/Counter/SourceCalculatorTests.cs`, each named with the corresponding C# method name (e.g., `SourceExp_...`, `SourcePleasureC_...`) |
| 20 | Create `Era.Core/Counter/ExpLvTable.cs` with `public static readonly int[] Thresholds = { 0, 1, 4, 20, 50, 200, 500, 1000, 2000, 3000, 5000, 8000, 12000, 18000, 25000, 35000, 50000, 70000, 100000 }` (19 elements from OPTION_2.ERB). SourceCalculator references `ExpLvTable.Thresholds` instead of private copy. |
| 21 | Register `services.AddSingleton<ISourceCalculator, SourceCalculator>()` in `ServiceCollectionExtensions.cs` |
| 22 | Update `.claude/skills/engine-dev/SKILL.md`: add ISourceCalculator to Counter Interfaces section |
| 23 | Update `.claude/skills/engine-dev/SKILL.md`: add IRelationVariables to Core Interfaces section |
| 24 | Create `Era.Core/Counter/ExpLvTable.cs` with `public static readonly int[] Thresholds = { 0, 1, 4, 20, 50, 200, 500, 1000, 2000, 3000, 5000, 8000, 12000, 18000, 25000, 35000, 50000, 70000, 100000 }` (19 elements from OPTION_2.ERB). Update PainStateChecker to reference `ExpLvTable.Thresholds`. SourceCalculator uses `ExpLvTable.Thresholds` instead of private copy. |
| 25 | Add well-known constants to `Era.Core/Types/CharacterFlagIndex.cs`: RotatorVInsertion(15), RotatorAInsertion(16), and a named constant for index 310 (determine semantic name from SOURCE1.ERB context) |
| 26 | Add well-known constants to `Era.Core/Types/ExpIndex.cs`: CExp(0), VExp(1), BExp(3) — AExp(2) already exists |
| 27 | Tightness boundary tests in SourceCalculatorTests with `Tightness` in method name; each boundary value (99/100/250/450/700) tested with expected multiplier |
| 28 | At least one test method in SourceCalculatorTests with `SourcePleasureA` AND (`CrossIndex` or `Submission`) in name, verifying SOURCE:屈従/=3 side effect |
| 29 | At least two test methods with `SourceObedience_Abl` prefix: `SourceObedience_AblZero_...` and `SourceObedience_AblOne_...` |
| 30 | SourceCalculator.cs references `ExpLvTable.Thresholds` (not private copy). Verifies DEBT-001 elimination: no 3-way duplication of EXPLV thresholds. |
| 31 | At least one test exercises non-neutral RELATION value (e.g., 50 or 150) in SourcePleasureC/V/A/B tests, verifying RELATION multiplier correctness (SOURCE1.ERB:435-443). |
| 32 | After Task#9, SourceCalculator.cs must NOT contain a private ExpLvThresholds array — verified by not_matches grep. Ensures DEBT-001 extraction was complete. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Where to add GetRelation | A: IVariableStore (extend existing), B: new IRelationVariables interface (ISP), C: IEngineVariables | B: new IRelationVariables | RELATION is a distinct 2D array separate from typical variable store arrays; ISP precedent established by ITEquipVariables for TEQUIP. IVariableStore already has 20+ methods; growing further violates ISP. IEngineVariables is for scalar engine variables not character arrays. |
| Tightness name abstraction | A: Add ITightnessChecker interface method, B: Inline threshold mapping as private helper, C: Expose string comparison | B: Inline threshold mapping | No other consumer needs tightness names in C#. PRINT_STATE.ERB thresholds are well-defined constants. Inline eliminates interface complexity and string fragility. SSOT is the ERB threshold table, which we replicate as constants. |
| EXP_UP abstraction | A: Add GetExpUp to ICounterUtilities, B: Inline the subtraction logic in SourceExp | B: Inline in SourceExp | EXP_UP is a #FUNCTION in COMMON.ERB:136-138 computing `EXP:char:index - TCVAR:char:(400+index)`. Only called twice in SOURCE_EXP (lines 20, 22). Simple subtraction; inlining avoids interface pollution for trivial logic. |
| EXPLV thresholds | A: Read from CSV/YAML at runtime, B: Hardcode constants following PainStateChecker precedent | B: Hardcode | PainStateChecker already hardcodes EXPLV thresholds in `_expLvThresholds`. Consistent approach; runtime CSV/YAML reading for EXPLV would require new loader infrastructure. |
| SOURCE_恭順 bug fidelity | A: Fix the bug (correct SELECTCASE), B: Faithfully reproduce (CASE-0 only fires) | B: Faithful reproduction | Philosophy requires "faithful reproduction of SOURCE_恭順 CASE-0 bug behavior." Equivalence tests must match ERB output. The C9 AC explicitly tests the bug behavior. |
| Method signatures (int vs CharacterId) | A: CharacterId for all character params, B: int for slave/master, C: CharacterId slave + int master | A: CharacterId for both | Consistent with F801 ActionValidator pattern. Type safety prevents integer mixing bugs. master param nullable pattern: use `CharacterId master = default` where ERB uses `主人=0`. |
| File placement | A: Era.Core/Counter/ (with existing Counter classes), B: Era.Core/Source/ (new directory) | A: Era.Core/Counter/ | F812 is part of Phase 21 Counter System migration. Counter/ already has ActionValidator, ActionSelector, ICounterSystem. Consistent grouping. |
| Uniform method signatures | A: All 35 methods use (CharacterId slave, CharacterId master), B: Per-function signatures (some omit master) | A: Uniform signatures | F811 SOURCE.ERB dispatch loop always provides both slave and master; uniform signatures simplify dispatcher code. 29 non-dual-character functions ignore master but accept it for API uniformity. ISP trade-off: single monolithic interface preferred over per-function overloads for 35 methods. Phase 25 NTR_UTIL may segregate if needed. |

### Interfaces / Data Structures

**New file: `Era.Core/Interfaces/IRelationVariables.cs`**

```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces;

/// <summary>
/// Interface for accessing RELATION 2D array (character affinity/compatibility values).
/// RELATION:slave:(NO:master) is indexed by slave character and master's CSV registration number (NO).
/// Segregated from IVariableStore per ISP (same pattern as ITEquipVariables for TEQUIP).
/// Feature 812 - SOURCE1 Extended Migration
/// </summary>
public interface IRelationVariables
{
    /// <summary>
    /// Get RELATION value for a character vs another character identified by their NO (CSV number).
    /// Maps to RELATION:character:(characterNo) in ERB.
    /// </summary>
    /// <param name="character">The character whose relation is read</param>
    /// <param name="otherCharacterNo">The CSV registration number (NO) of the other character</param>
    /// <returns>Result containing relation value (typically 1-200, 100=neutral) or error</returns>
    Result<int> GetRelation(CharacterId character, int otherCharacterNo);
}
```

**`GetAssiPlay()` addition to IEngineVariables.cs:**

```csharp
/// <summary>Get ASSIPLAY value (assistant play mode flag, stored in ASSIPLAY:0)</summary>
/// Feature 812 - Required for SOURCE_EXTRA恋慕 branch
int GetAssiPlay();
```

**New file: `Era.Core/Counter/ISourceCalculator.cs`** — 35 methods, all `void`, all accepting `(CharacterId slave, CharacterId master)`:

```csharp
// Era.Core/Counter/ISourceCalculator.cs
using Era.Core.Types;

namespace Era.Core.Counter;

public interface ISourceCalculator
{
    void SourceExp(CharacterId slave, CharacterId master);
    void SourcePleasureC(CharacterId slave, CharacterId master);
    void SourcePleasureV(CharacterId slave, CharacterId master);
    void SourcePleasureA(CharacterId slave, CharacterId master);
    void SourcePleasureB(CharacterId slave, CharacterId master);
    void SourceGivePleasureC(CharacterId slave, CharacterId master);
    void SourceGivePleasureV(CharacterId slave, CharacterId master);
    void SourceGivePleasureA(CharacterId slave, CharacterId master);
    void SourceGivePleasureB(CharacterId slave, CharacterId master);
    void SourceCvabExtra(CharacterId slave, CharacterId master);
    void SourceAffection(CharacterId slave, CharacterId master);
    void SourceSexualActivity(CharacterId slave, CharacterId master);
    void SourceAchievement(CharacterId slave, CharacterId master);
    void SourcePain(CharacterId slave, CharacterId master);
    void SourceFear(CharacterId slave, CharacterId master);
    void SourceLiquid(CharacterId slave, CharacterId master);
    void SourceArousal(CharacterId slave, CharacterId master);
    void SourceObedience(CharacterId slave, CharacterId master);
    void SourceExposure(CharacterId slave, CharacterId master);
    void SourceSubmission(CharacterId slave, CharacterId master);
    void SourcePleasureEnjoyment(CharacterId slave, CharacterId master);
    void SourceConquest(CharacterId slave, CharacterId master);
    void SourcePassive(CharacterId slave, CharacterId master);
    void SourceSeduction(CharacterId slave, CharacterId master);
    void SourceHumiliation(CharacterId slave, CharacterId master);
    void SourceProvocation(CharacterId slave, CharacterId master);
    void SourceService(CharacterId slave, CharacterId master);
    void SourceCoercion(CharacterId slave, CharacterId master);
    void SourceSadism(CharacterId slave, CharacterId master);
    void SourceFilth(CharacterId slave, CharacterId master);
    void SourceDepression(CharacterId slave, CharacterId master);
    void SourceDeviation(CharacterId slave, CharacterId master);
    void SourceAntipathy(CharacterId slave, CharacterId master);
    void SourceExtra(CharacterId slave, CharacterId master);
    void SourceDownbase(CharacterId slave, CharacterId master);
}
```

**`SourceCalculator` constructor (primary DI pattern):**

```csharp
public sealed class SourceCalculator(
    IVariableStore variables,
    IEngineVariables engine,
    ITEquipVariables tequip,
    ICommonFunctions common,
    IRelationVariables relation) : ISourceCalculator
```

**Tightness private helper:**

```csharp
// Mirrors PRINT_STATE.ERB:724 threshold table (inline, no interface method needed)
// NOTE: SOURCE1.ERB:109 compares against "きゅうきゅう" but 締り具合名称 never returns it — dead branch bug.
private enum TightnessLevel { Normal, Tight, Loose, VeryLoose }
private static TightnessLevel GetTightness(int vLooseness)
{
    if (vLooseness < 100) return TightnessLevel.Normal;    // ぎちぎち → no multiplier
    if (vLooseness < 250) return TightnessLevel.Tight;     // きゅっきゅっ → 0.85
    // NOTE: 250-449 = "ゆるゆる" — SOURCE1.ERB does not compare this string, so no multiplier applied
    if (vLooseness < 450) return TightnessLevel.Normal;    // ゆるゆる → no multiplier (not compared in SOURCE1)
    if (vLooseness < 700) return TightnessLevel.Loose;     // がばがば → 0.55
    return TightnessLevel.VeryLoose;                       // ぽっかり → 0.40
}
```

**EXPLV shared threshold table (via ExpLvTable.cs — Task#9):**

```csharp
// SourceCalculator references shared table (no private copy):
// ExpLvTable.Thresholds[1] means below EXPLV:1
// Task#6 initially uses private copy; Task#9 extracts to ExpLvTable.cs and removes private copy
```

See `ExpLvTable.cs` (created by Task#9) for the actual 19-element array from OPTION_2.ERB.

**SOURCE_恭順 faithful bug reproduction:**

```csharp
// ERB bug: all 11 SELECTCASE branches use CASE 0, so only the first fires.
// ABL:従順 == 0 → TIMES 0.50 applied; ABL:従順 >= 1 → no CASE matches, no TIMES (effective 1.0)
private static int ApplyObedienceMultiplier(int source, int abl)
{
    if (abl == 0)
        return (int)(source * 0.50); // ERB bug: only first CASE 0 ever fires
    return source; // no CASE match when abl >= 1
}
```

**DI registration in ServiceCollectionExtensions.cs:**

```csharp
// Counter System (Phase 21) - Feature 812
services.AddSingleton<ISourceCalculator, SourceCalculator>();
```

**NullRelationVariables stub** (for test isolation / Era.Core default):

```csharp
// Era.Core/Interfaces/NullRelationVariables.cs
public sealed class NullRelationVariables : IRelationVariables
{
    public Result<int> GetRelation(CharacterId character, int otherCharacterNo)
        => new Result<int>.Success(100); // 100 = neutral RELATION value (no modification)
}
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| F812 places SourceCalculator in Era.Core/Counter/ but architecture doc specifies Era.Core/Source/ | phase-20-27-game-systems.md:216-217 Deliverables | Update Deliverables path from Era.Core/Source/ to Era.Core/Counter/ |

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 4 | Add 12 missing well-known constants to `Era.Core/Types/SourceIndex.cs`: GivePleasureC(40), GivePleasureV(41), GivePleasureA(42), GivePleasureB(43), Seduction(50), Humiliation(51), Provocation(52), Service(53), Coercion(54), Sadism(55), Liquid(9), SexualActivity(11) | | [x] |
| 2 | 5 | Add 6 missing well-known constants to `Era.Core/Types/CupIndex.cs`: Goodwill(5), Superiority(6), Learning(7), Lubrication(9), Shame(13), Depression(32) | | [x] |
| 3 | 6, 7, 16, 17 | Create `Era.Core/Interfaces/IRelationVariables.cs` with `Result<int> GetRelation(CharacterId character, int otherCharacterNo)` and `Era.Core/Interfaces/NullRelationVariables.cs` stub returning Success(100); add `int GetAssiPlay()` to `IEngineVariables.cs` with `NullEngineVariables.cs` returning 0; after adding GetAssiPlay(), verify IEngineVariables method count >= 25 (baseline 24 + 1 = 25) for backward compatibility (AC#16) | | [x] |
| 4 | 1, 2 | Create `Era.Core/Counter/ISourceCalculator.cs` declaring all 35 `void Source*` methods (`SourceExp` through `SourceDownbase`), each accepting `(CharacterId slave, CharacterId master)` | | [x] |
| 5 | 9, 15, 19, 27, 28, 29, 31 | Write RED equivalence tests in `Era.Core.Tests/Counter/SourceCalculatorTests.cs` for all 35 SOURCE_ functions with representative inputs and TIMES truncation boundary values; include `SourceObedience_AblZero_AppliesHalfMultiplier` and `SourceObedience_AblOne_AppliesNoMultiplier` as two separate [Fact] methods (NOT [Theory] — AC#29 gte 2 requires distinct method names); include tightness boundary tests per PRINT_STATE.ERB:724 (vLooseness=99→Normal, 100→Tight, 250→Normal, 450→Loose, 700→VeryLoose) with multiplier verification in SourcePleasureV/A tests (Tight→0.85, Loose→0.55, VeryLoose→0.40 per SOURCE1.ERB:107-114; きゅうきゅう dead branch produces no multiplier); include cross-index SOURCE mutation test for SourcePleasureA verifying SOURCE:屈従 /= 3 side effect (C13, SOURCE1.ERB:197-198); all test expected values MUST cite SOURCE1.ERB line numbers in InlineData/Fact comments (e.g., `// SOURCE1.ERB:67-78`) to ensure ERB traceability; each test method name MUST start with the corresponding C# method name prefix (SourceExp_, SourcePleasureC_, ..., SourceDownbase_) — helpers MUST NOT use Source prefix; include SourceExtra ASSIPLAY branch tests with GetAssiPlay()==0 and GetAssiPlay()==1 cases (SOURCE1.ERB:1605, 1613); include CHARANUM guard boundary test for dual-character functions: master.Value == GetCharaNum() (out of range) → verify SetCup NOT called for master (C14); include RELATION boundary test for SourcePleasureC/V/A/B with non-neutral RELATION value (e.g., RELATION=50 or 150) verifying multiplier effect per SOURCE1.ERB:435-443 (C6) | | [x] |
| 6 | 3, 8, 10, 11, 12 | Implement `Era.Core/Counter/SourceCalculator.cs` with primary constructor DI (`IVariableStore`, `IEngineVariables`, `ITEquipVariables`, `ICommonFunctions`, `IRelationVariables`); private `GetTightness(int vLooseness) → TightnessLevel` inline helper; hardcoded `ExpLvThresholds` array; all TIMES as `(int)(value * Xm)`; CHARANUM guard before RELATION/master-CUP access; dual-character CUP writes for the 6 master-affecting functions; SOURCE_Obedience CASE-0 bug faithful reproduction with comment | | [x] |
| 7 | 13, 14, 18, 21 | Register `services.AddSingleton<ISourceCalculator, SourceCalculator>()` and `services.AddSingleton<IRelationVariables, NullRelationVariables>()` in `ServiceCollectionExtensions.cs`; verify `dotnet build Era.Core/` succeeds with TreatWarningsAsErrors; verify no TODO/FIXME/HACK in all 10 F812-touched files: Era.Core/Counter/, Era.Core/Types/SourceIndex.cs, Era.Core/Types/CupIndex.cs, Era.Core/Types/CharacterFlagIndex.cs, Era.Core/Types/ExpIndex.cs, Era.Core/Interfaces/IRelationVariables.cs, Era.Core/Interfaces/NullRelationVariables.cs, Era.Core/Interfaces/IEngineVariables.cs, Era.Core/Interfaces/NullEngineVariables.cs, Era.Core/Character/PainStateChecker.cs (matching AC#14 scope) | | [x] |
| 8 | 22, 23 | Update `.claude/skills/engine-dev/SKILL.md` SSOT documentation: add ISourceCalculator and IRelationVariables to Core Interfaces section, add SourceIndex new constants (12) and CupIndex new constants (6) to Types section, add GetAssiPlay() to IEngineVariables section | | [x] |
| 9 | 14, 20, 24, 30, 32 | Extract shared `Era.Core/Counter/ExpLvTable.cs` with `public static readonly int[] Thresholds` (19 elements from OPTION_2.ERB); refactor `PainStateChecker` to use `ExpLvTable.Thresholds` instead of private `_expLvThresholds`; `SourceCalculator` references `ExpLvTable.Thresholds` instead of private copy — remove private `ExpLvThresholds` array from SourceCalculator.cs (AC#32 not_matches verification); after refactor, verify no TODO/FIXME/HACK in Era.Core/Character/PainStateChecker.cs (AC#14 re-check — PainStateChecker.cs is modified by this task after Task#7's initial debt verification) | | [x] |
| 10 | 25, 26 | Add typed constants to `Era.Core/Types/CharacterFlagIndex.cs`: RotatorVInsertion(15), RotatorAInsertion(16), and named constant for index 310; add typed constants to `Era.Core/Types/ExpIndex.cs`: CExp(0), VExp(1), BExp(3) | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

**When to use `[I]`**:
- AC Expected value cannot be determined before implementation
- Task output depends on investigation or runtime discovery
- Downstream tasks depend on this task's concrete output

**When NOT to use `[I]`**:
- Build/compile tasks (Expected = "succeeds" is always deterministic)
- Tasks with spec-defined outputs (API contracts, schema validation)
- Tasks that verify file existence (deterministic yes/no)
- Tasks where Expected can be calculated from requirements (counts from data, paths from config)
- Standard patterns with known outputs (error messages, status codes)

**Anti-pattern**: Using `[I]` to avoid writing concrete Expected values. `[I]` is for genuine uncertainty, not convenience.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Tasks 1-2, 10: SourceIndex.cs, CupIndex.cs, CharacterFlagIndex.cs, ExpIndex.cs + AC#4, AC#5, AC#25, AC#26 | 12 new SourceIndex constants, 6 new CupIndex constants, 3 new CharacterFlagIndex constants, 3 new ExpIndex constants |
| 2 | implementer | sonnet | Task 3: IEngineVariables.cs, NullEngineVariables.cs + AC#6, AC#7 | IRelationVariables.cs, NullRelationVariables.cs, GetAssiPlay in IEngineVariables |
| 3 | implementer | sonnet | Task 4: ISourceCalculator design + AC#1, AC#2 | ISourceCalculator.cs with 35 method declarations |
| 4 | tester | sonnet | Task 5: SOURCE1.ERB, SourceCalculatorTests AC#9, AC#15, AC#19, AC#27, AC#28, AC#29, AC#31 | RED tests (all fail — SourceCalculator does not exist yet) |
| 5 | implementer | sonnet | Task 6: SOURCE1.ERB, SourceCalculator design + AC#3, AC#8, AC#10, AC#11, AC#12 | SourceCalculator.cs implementation |
| 6 | implementer | sonnet | Task 7: ServiceCollectionExtensions.cs + AC#13, AC#14 | DI registration; dotnet build passes; no debt markers |
| 7 | tester | sonnet | Era.Core.Tests/ — run all SourceCalculatorTests (AC#9, AC#15) | GREEN — all equivalence tests pass |
| 8 | implementer | sonnet | Task 8: engine-dev SKILL.md + AC#22, AC#23 | SSOT documentation updated with ISourceCalculator, IRelationVariables, new constants |
| 9 | implementer | sonnet | Task 9: ExpLvTable.cs extraction + PainStateChecker refactor + AC#20, AC#24, AC#30, AC#32 | ExpLvTable.cs shared class; PainStateChecker updated; private EXPLV copy removed |

### Pre-conditions

- F783 [DONE]: Phase 21 Planning complete, SOURCE1.ERB scope confirmed
- F815 [DONE]: StubVariableStore available in Era.Core.Tests for test isolation
- F801 [DONE]: DI pattern established (ActionValidator.cs primary constructor pattern as reference)

### Execution Order

1. **Phase 1 (T1, T2)**: Add typed constants first — all subsequent C# compilation depends on SourceIndex and CupIndex completeness. These are purely additive changes with no risk of breaking existing code.
2. **Phase 2 (T3)**: Create IRelationVariables interface, NullRelationVariables stub, and add GetAssiPlay to IEngineVariables. Add NullRelationVariables stub before SourceCalculator constructor references it. Verify `dotnet build Era.Core/` passes after each change.
3. **Phase 3 (T4)**: Create ISourceCalculator interface — all 35 methods must be declared before tests can reference them.
4. **Phase 4 (T5)**: Write RED tests using StubVariableStore from F815. Tests MUST fail at this point (SourceCalculator does not exist). Confirm RED before proceeding.
5. **Phase 5 (T6)**: Implement SourceCalculator.cs. Follow SOURCE1.ERB function order. For each function group:
   - Pleasure functions (SOURCE_快C, SOURCE_快V, SOURCE_快A, SOURCE_快B): use GetTightness helper for V/A variants
   - Source-to-CUP converters (SOURCE_誘惑 through SOURCE_加虐): apply CHARANUM guard, dual-CUP writes for master-affecting 6
   - Parameter functions (SOURCE_EXP, SOURCE_恭順, etc.): faithfully reproduce SOURCE_恭順 CASE-0 bug
   - SOURCE_EXTRA mega-function: handle ASSIPLAY branch via GetAssiPlay()
   - SOURCE_DOWNBASE: SetBase/SetDownbase writes
6. **Phase 6 (T7)**: Register DI, build verification, debt check.
7. **Phase 7**: Run `dotnet test Era.Core.Tests/ --filter Source` to confirm GREEN. All 35 function equivalence tests must pass.

### Build Verification Steps

```bash
# After Phase 1 (T1, T2): constants compile
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'

# After Phase 2 (T3): interfaces compile
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'

# After Phase 3 (T4): ISourceCalculator compiles
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'

# After Phase 4 (T5): RED tests (MUST FAIL before T6)
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter FullyQualifiedName~SourceCalculatorTests'

# After Phase 5 (T6): full build
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'

# After Phase 7: GREEN test confirmation
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter FullyQualifiedName~SourceCalculatorTests'

# After Phase 8 (T8): SSOT documentation updated
grep -c 'ISourceCalculator' .claude/skills/engine-dev/SKILL.md

# After Phase 9 (T9): ExpLvTable extraction + PainStateChecker refactor
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter FullyQualifiedName~SourceCalculatorTests'
```

### Success Criteria

- All 32 ACs verify PASS
- `dotnet build Era.Core/` exits 0 with TreatWarningsAsErrors
- `dotnet test Era.Core.Tests/ --filter FullyQualifiedName~SourceCalculatorTests` exits 0
- `dotnet test Era.Core.Tests/ --filter "FullyQualifiedName~SourceCalculatorTests&FullyQualifiedName~SourceObedience"` exits 0
- Grep `TODO|FIXME|HACK` in `Era.Core/Counter/SourceCalculator.cs` returns 0 matches

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// Counter System (Phase 21) - Feature 812
services.AddSingleton<ISourceCalculator, SourceCalculator>();
```

### Error Handling

- If `dotnet build` fails after T1/T2: verify numeric indices in SourceIndex.cs and CupIndex.cs do not duplicate existing constants
- If `dotnet build` fails after T3: verify IRelationVariables namespace is `Era.Core.Interfaces` and `NullRelationVariables` implements the interface correctly
- If RED tests fail to compile after T5: verify ISourceCalculator.cs is in `Era.Core.Counter` namespace and Era.Core.Tests project references Era.Core
- If GREEN tests fail after T6: cross-reference SOURCE1.ERB line-by-line for the failing function; TIMES truncation and SELECTCASE logic are the most likely divergence points
- STOP after 3 consecutive build/test failures → report to user

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| SourceIndex constant overlap with F803 (F803 plans 12 of same constants) | Additive-only — whoever runs first adds them; F803 must check existing count and skip already-added constants | Feature | F803 | - |
| IRelationVariables engine-layer implementation (NullRelationVariables is stub only) | Engine implementation calling RELATION array via GlobalStatic belongs in engine/ project per Issue 29 pattern; out of scope for F812 | Feature | F813 | - |

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK
- Missing Task for Option A → FL FAIL
-->

<!-- DRAFT Creation Checklist (Option A):
When a Task creates a new feature-{ID}.md [DRAFT], it MUST complete ALL of:
1. Create feature-{ID}.md file
2. Register in index-features.md (add row to Active Features table)
3. Update "Next Feature number" in index-features.md
AC for DRAFT creation MUST verify BOTH file existence AND index registration.
-->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-24 | DEVIATION | ac-tester | AC#12 verification | FAIL: pattern `SetCup\(.*master` matches 0 — implementation uses `AddCup(master,...)` helper delegating to SetCup. Fix: update AC pattern to `AddCup\(master` |
| 2026-02-24 | DEVIATION | ac-tester | AC#25 verification | FAIL: pattern `CharacterFlagIndex\(310\)` matches 0 — target-typed `new(310)` syntax used. Fix: add `MansionRank` to pattern |
| 2026-02-24 | DEVIATION | feature-reviewer | Phase 8.1 quality review | NEEDS_REVISION: PainStateChecker still has private EXPLV array (Task#9 DEBT-001 incomplete). Fix: refactor to use ExpLvTable.Thresholds |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [resolved-applied] fc-validator 1: [VOL-001] Volume waiver: 1,959 lines SOURCE1.ERB (35 functions) exceeds engine ~300 line limit. Waiver granted — 35 pure-calculation functions form an atomic unit consumed by F811 ISourceCalculator; splitting would create interface fragmentation and circular dependencies.
- [fix] fc-validator 1: [AC-001] AC#16 Expected corrected from count_gte 24 to count_gte 25 (baseline is 24 existing methods, not 23).
- [fix] fc-validator 1: [META-001] Type corrected from erb to engine (all primary artifacts are .cs files in Era.Core).
- [fix] Phase2-Review iter1: [AC-002] AC#15 only verifies tests pass, not count. Added AC#19 (count_gte 35 on test attributes) to enforce equivalence test coverage for all 35 functions.
- [fix] Phase2-Review iter2: [AC-003] Goal item 7 (EXPLV thresholds) had no direct structural AC. Added AC#20 (Grep ExpLvThresholds in SourceCalculator.cs) to verify hardcoded threshold array exists.
- [fix] Phase2-Review iter3: [GOAL-001] Goal#1 incorrectly included AC#11 (CHARANUM guard) and AC#12 (dual-character CUP writes) which verify implementation behavior, not interface boundary. Removed from Goal#1 mapping.
- [fix] Phase2-Review iter4: [META-002] Success Criteria AC count updated (now 26 ACs after subsequent additions).
- [fix] Phase2-Review iter5: [META-003] Upstream Issues section contained 2 stale rows: AC#2 pattern already corrected to 'void Source', AC#5 already uses 'Learning' not 'Suppression'. Cleared stale rows.
- [fix] Phase2-Review iter6: [AC-004] AC#14 scope expanded from SourceCalculator.cs-only to all 4 F812 new files (Era.Core/Counter/ + IRelationVariables.cs + NullRelationVariables.cs) per C10 zero technical debt requirement.
- [fix] Phase2-Review iter7: [AC-005] AC#16 and AC#19 used invalid matcher 'count_gte' — corrected to 'gte' per testing/SKILL.md matcher list.
- [fix] Phase2-Review iter7: [AC-006] AC#19 pattern changed from '[Fact]|[Theory]' attribute count to 'public.*void.*Source' method signature count — ensures per-function coverage (prevents [Theory] multi-function double-counting).
- [fix] Phase2-Review iter8: [AC-007] AC#8 detail expanded with tightness boundary test requirement (vLooseness=99/100/200/300/400). Task#5 updated to include tightness boundary tests.
- [fix] Phase2-Review iter8: [TASK-001] Task#3 description expanded with backward compat verification step (method count >= 25 after GetAssiPlay addition).
- [fix] Phase2-Review iter9: [AC-008] Added AC#21 (ISourceCalculator DI registration in ServiceCollectionExtensions.cs). Updated Task#7, Goal Coverage, Philosophy Derivation, Approach, and Success Criteria for 21 ACs.
- [fix] Phase2-Review iter10: [GOAL-002] Goal#6 coverage table added AC#15 (equivalence tests verify tightness threshold correctness via boundary test cases in Task#5).
- [fix] Phase2-Review iter11: [AC-009] ApplyObedienceMultiplier changed from decimal 0.50m to double 0.50 to match C2 TIMES float-to-int truncation semantics. AC Coverage row 10 clarified Xm→X (double).
- [resolved-applied] Phase2-Review iter11: [AC-010] AC#19 naming convention added: test methods must start with corresponding C# method name; helpers must not use Source prefix. Combined with gte 35 count, ensures per-function coverage.
- [resolved-applied] Phase2-Review iter12: [AC-011] Task#5 updated: all test expected values must cite SOURCE1.ERB line numbers in comments for ERB traceability.
- [fix] Phase2-Review iter1: [AC-012] AC#14 scope expanded from 4 new files to all 7 F812-touched files (added SourceIndex.cs, CupIndex.cs, IEngineVariables.cs, NullEngineVariables.cs) per zero-technical-debt Philosophy claim.
- [fix] Phase2-Review iter1: [AC-013] AC#15 filter narrowed from '--filter Source' to '--filter FullyQualifiedName~SourceCalculatorTests' to scope equivalence verification to exactly the migration test class.
- [fix] Phase2-Review iter1: [TASK-002] Task#5 naming constraint added: test methods must start with corresponding C# method name prefix; helpers must not use Source prefix.
- [fix] Phase2-Review iter2: [AC-014] ISourceCalculator interface spec expanded from partial placeholder to complete enumeration of all 35 C# method names mapped from ERB @SOURCE_* functions.
- [fix] Phase2-Review iter2: [AC-015] AC#12 matcher changed from 'matches' (at least one) to 'gte 6' to verify all 6 master-affecting functions write master CUP.
- [fix] Phase2-Review iter3: [AC-016] AC#11 matcher changed from 'matches' to 'gte 6' and Expected updated to require CHARANUM guard for all 6 dual-character functions, not just existence check.
- [resolved-skipped] Phase2-Review iter4: [AC-017] AC#19 gte 35 doesn't enforce distinct prefix coverage — 36+ tests for 34 functions satisfies gte 35. count_equals 35 would enforce 1:1 but prevents multiple tests per function (e.g., boundary tests). User decision: keep gte 35 (naming convention + AC#15 provides sufficient coverage).
- [fix] Phase2-Review iter1: [FMT-001] Upstream Issues section used prose instead of empty table. Replaced with proper table header per feature-template.md format.
- [fix] Phase2-Review iter1: [AC-018] AC#12 rationale expanded: added explicit exclusion of SOURCE_恭順 (writes slave CUP only, not master) and enumerated the 6 master-affecting functions by name.
- [fix] Phase2-Review iter2: [AC-019] AC#2 rationale expanded: added complete enumeration of all 35 method names as ground truth (Technical Design reference). Closes gap where arbitrary 'void Source*' names could satisfy count without matching ERB functions.
- [fix] Phase3-Maintainability iter3: [DATA-001] EXPLV hardcoded values corrected from fabricated { 0, 50, 150, 300, 600, 1000 } to actual OPTION_2.ERB values { 0, 1, 4, 20, 50, 200, 500, 1000, 2000, 3000, 5000, 8000, 12000, 18000, 25000, 35000, 50000, 70000, 100000 } (19 elements). Updated both Approach and code snippet.
- [fix] Phase3-Maintainability iter3: [DOC-001] EXP_UP attribution corrected: was incorrectly described as 'local helper', actually defined in COMMON.ERB:136-138 as #FUNCTION computing EXP - TCVAR subtraction.
- [fix] Phase3-Maintainability iter3: [SSOT-001] Added Task#8 (engine-dev SKILL.md updates) + AC#22/AC#23 for SSOT documentation. Total ACs: 23. Implementation Contract Phase 8 added.
- [resolved-applied] Phase3-Maintainability iter3: [DEBT-001] EXPLV table duplication: PainStateChecker (19 elements) and AbilityGrowthProcessor (21 elements, different values after index 9) already have private EXPLV copies. Adding a 3rd copy in SourceCalculator creates 3-way duplication. Consider extracting shared ExpLvTable class. Zero Debt Upfront concern.
- [fix] Phase2-Review iter4: [AC-020] AC#9 test filter tightened from '--filter SourceObedience' to '--filter "FullyQualifiedName~SourceCalculatorTests&FullyQualifiedName~SourceObedience"' for proper scoping per AC#15 pattern.
- [fix] Phase4-ACLint iter5: [GOAL-003] Goal Coverage table missing AC#11, AC#12, AC#22, AC#23. Added C14 constraint row (AC#11, AC#12) and SSOT row (AC#22, AC#23).
- [fix] Phase2-Review iter6: [CON-001] C13 (cross-index SOURCE mutations) had no explicit test requirement. Task#5 expanded: added cross-index mutation test for SourcePleasureA verifying SOURCE:屈従 /= 3 (SOURCE1.ERB:197-198).
- [fix] Phase2-Review iter7: [TASK-003] Task#7 description expanded to all AC#14 target files to match AC#14 scope (now 8 files).
- [fix] Phase2-Review iter1: [DOC-002] Approach section AC enumeration expanded to include all ACs (now 26 total).
- [fix] Phase2-Review iter2: [AC-021] Task#5 expanded: added SourceExtra ASSIPLAY branch test requirement (GetAssiPlay()==0 and ==1) per SOURCE1.ERB:1605, 1613.
- [fix] Phase2-Review iter2: [AC-022] AC#11 pattern tightened from 'GetCharaNum\(\)' to 'master\.Value < engine\.GetCharaNum\(\)' to match the structural guard pattern (avoids false positives from non-guard usages).
- [resolved-skipped] Phase2-Review iter3: [AC-023] AC#2 count_equals 35 cannot verify exact method name correctness (loop of [AC-019]). AC#2 rationale already enumerates all 35 names as ground truth; count_equals is the only feasible Grep-based matcher. Name correctness is enforced by Implementation Contract Phase 3 using the explicit interface spec. User decision required: accept count_equals with implementation-contract enforcement, or add alternative verification.
- [fix] Phase2-Uncertain iter3: [AC-024] Task#5 tightness boundary tests expanded: added explicit multiplier verification requirement (Tight→0.85, VeryTight→0.70, Loose→0.55, VeryLoose→0.40 per SOURCE1.ERB:107-114) in SourcePleasureV/A tests.
- [fix] Phase2-Review iter4: [AC-025] Task#5 expanded: added CHARANUM guard boundary test for dual-character functions (master out of range → no SetCup for master). Closes behavioral gap where AC#11 only verifies structural presence.
- [resolved-applied] Phase2-Review iter5: [CONST-001] CharacterFlagIndex/ExpIndex typed constant gaps: SourceExp uses CFLAG:ローター挿入(15), CFLAG:ローターA挿入(16), EXP indices Ｃ経験(0)/Ｖ経験(1)/Ｂ経験(3) — none in CharacterFlagIndex.cs/ExpIndex.cs. CFLAG:310 used in SOURCE_征服/SOURCE_受動 (SOURCE1.ERB:1073,1158) — no CharacterFlagIndex constant. TCVAR(400+index) computed indices used by inlined EXP_UP. Zero debt concern: SourceIndex/CupIndex have typed constants (C3/C4) but CFLAG/EXP gaps are unaddressed. User decision: (a) add Tasks/ACs to F812 scope for these typed constants, (b) create handoff to F813/new feature, or (c) accept raw integers with documented exception.
- [fix] PostLoop iter5: [DEBT-001] Resolved: added Task#9 (shared ExpLvTable.cs extraction) + AC#24. PainStateChecker refactored to share.
- [fix] PostLoop iter5: [CONST-001] Resolved: added Task#10 (CharacterFlagIndex/ExpIndex typed constants) + AC#25, AC#26. Folded into Implementation Contract Phase 1.
- [fix] Phase2-Review iter6: [AC-026] AC#20 rationale corrected: cited [C2] (TIMES truncation) but should reference [Goal item 7] (EXPLV thresholds). C2 is already covered by AC#10.
- [fix] Phase2-Review iter7: [AC-027] AC#20 target changed from SourceCalculator.cs/ExpLvThresholds to ExpLvTable.cs/Thresholds to align with Task#9 shared extraction (AC#24).
- [fix] Phase2-Review iter7: [AC-028] AC#14 scope expanded from 7 to 8 files: added PainStateChecker.cs (modified by Task#9 ExpLvTable extraction).
- [fix] Phase2-Review iter8: [AC-029] AC#20 task mapping corrected: moved from Task#6 to Task#9 (ExpLvTable.cs, not SourceCalculator.cs). AC Coverage row 20 updated.
- [fix] Phase2-Review iter8: [GOAL-004] Added C13 row to Goal Coverage Verification table (cross-index SOURCE mutations → AC#15).
- [fix] Phase2-Review iter8: [TASK-004] Task#7 scope updated from 7 to 8 files: added PainStateChecker.cs per AC#14 [AC-028] expansion.
- [fix] Phase2-Review iter9: [DATA-002] Tightness thresholds corrected from fabricated 100/200/300/400 to actual PRINT_STATE.ERB:724 values 100/250/450/700. Removed VeryTight (きゅうきゅう) from TightnessLevel enum — dead branch bug documented. Updated AC#8 Note, Task#5, AC Coverage row 8, Technical Constraints, Approach section.
- [fix] Phase2-Review iter10: [DOC-003] Technical Design ExpLvThresholds code snippet updated to show post-Task#9 form (shared ExpLvTable reference). Removed the private array snippet that conflicted with Task#9/AC#24 extraction design. Added sequencing comment (Task#6 initially uses private copy → Task#9 extracts).
- [fix] Phase3-Maintainability iter10: [SSOT-002] Upstream Issues populated: architecture doc (phase-20-27-game-systems.md:216-217) specifies Era.Core/Source/ but F812 uses Era.Core/Counter/.
- [fix] Phase3-Maintainability iter10: [KEY-001] Key Decision added: Uniform method signatures (all 35 use slave+master) for F811 dispatcher simplicity. ISP trade-off documented. Phase 25 NTR_UTIL segregation noted.
- [fix] Phase4-ACLint iter10: [AC-030] AC count references in Review Notes [fix] entries normalized (stale 18/21/7/23 → current count).
- [fix] Phase4-ACValidation iter10: [AC-031] AC#4, AC#5, AC#11, AC#12 Method columns updated: added missing search patterns required by gte matcher for ac-static-verifier.
- [fix] Phase2-Review iter1: [FMT-002] Removed non-template ## Summary section (lines 19-22). Content already in Background/Philosophy.
- [fix] Phase2-Review iter1: [AC-032] AC#25 Method column: added missing search pattern "RotatorVInsertion|RotatorAInsertion|CharacterFlagIndex(310)" to match AC Details section.
- [fix] Phase2-Review iter1: [AC-033] Added AC#27 (tightness boundary tests pass via dotnet test filter). AC#8 keyword-only matcher insufficient for threshold correctness verification.
- [fix] Phase2-Review iter1: [AC-034] Added AC#28 (cross-index SOURCE mutation test exists via Grep). C13 was only covered by generic AC#15.
- [fix] Phase2-Review iter1: [AC-035] Added AC#29 (SOURCE_Obedience both branches tested via Grep count gte 2). AC#9 succeeds matcher doesn't enforce both ABL==0/ABL>=1 test cases exist.
- [fix] Phase2-Uncertain iter1: [DOC-004] Philosophy 'pure-calculation functions' changed to 'calculation functions' with C13 cross-index side effects clarification. Aligns Philosophy with Technical Constraints C13.
- [fix] Phase2-Review iter1: [DOC-005] Build Verification Steps expanded: added Phase 8 (SKILL.md grep) and Phase 9 (dotnet build + test after ExpLvTable extraction) commands.
- [fix] Phase2-Review iter2: [AC-036] Added AC#30 (SourceCalculator references shared ExpLvTable.Thresholds). Closes DEBT-001 verification gap: AC#20/AC#24 verify ExpLvTable exists but no AC confirmed SourceCalculator uses it. Task#9 updated, Goal#7 updated, Success Criteria count updated to 30.
- [fix] Phase2-Review iter3: [AC-037] Added AC#31 (RELATION boundary test with non-neutral value). Closes C6 behavioral test gap: no AC enforced non-neutral RELATION value testing in SourcePleasureC/V/A/B. Task#5 expanded, Goal#4 updated.
- [fix] Phase2-Review iter3: [AC-038] Added AC#32 (no private EXPLV copy in SourceCalculator after Task#9). Closes DEBT-001 removal verification gap: AC#30 verifies reference to shared table but not removal of private copy. Task#9 updated.
- [fix] Phase2-Review iter4: [META-004] Approach section AC count updated from 29 to 32. Added AC#30, AC#31, AC#32 to parenthetical list.
- [fix] Phase2-Review iter4: [IMPL-001] Implementation Contract phase table updated: Phase 4 added AC#31, Phase 9 added AC#30/AC#32.
- [fix] Phase2-Review iter4: [PHIL-001] Philosophy Derivation table: 'equivalence-tested against legacy behavior' row expanded to include cross-index side effects (C13) with AC#28.
- [fix] Phase2-Review iter5: [AC-039] AC#14 scope expanded from 8 to 10 files: added CharacterFlagIndex.cs and ExpIndex.cs (modified by Task#10). Task#7 description updated.
- [fix] Phase2-Uncertain iter5: [TASK-005] Task#5 SourceObedience tests: added explicit [Fact] constraint (not [Theory]) to align with AC#29 gte 2 Grep-based method count verification.

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning
- [Related: F801](feature-801.md) - Counter infrastructure (DI pattern)
- [Related: F803](feature-803.md) - Overlapping SourceIndex constants
- [Successor: F811](feature-811.md) - SOURCE Entry System (needs ISourceCalculator)
- [Successor: F813](feature-813.md) - Post-Phase Review Phase 21
- [Related: F815](feature-815.md) - StubVariableStore test infrastructure
