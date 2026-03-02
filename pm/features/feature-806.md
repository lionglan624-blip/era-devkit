# Feature 806: WC Counter Message SEX

## Status: [DONE]
<!-- fl-reviewed: 2026-03-02T11:58:37Z -->

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

## Type: erb

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)

Phase 21 Counter System migration converts all ERB counter message logic to C# with full DI and equivalence testing. Each ERB message category is the SSOT for its C# migration target, ensuring no behavioral regression during the transition from interpreted ERB to compiled C#.

### Problem (Current Issue)

WcCounterMessage.cs contains 44 SEX-category action ID stubs (lines 249-293) that return 0 without producing any message output, because F805's architectural boundary was drawn at the dispatch switch statement rather than at the service composition level. The source ERB file TOILET_COUNTER_MESSAGE_SEX.ERB (3,518 lines, 49 functions) includes the largest single function in Phase 21 -- MESSAGE40 at 1,412 lines -- with deeply interleaved external dependencies (IVirginityManager, ICommonFunctions, ILocationService, IShrinkageSystem, IWcCounterMessageItem, IWcCounterMessageNtr, and others). Two interface gaps exist: IEngineVariables lacks GetTime/SetTime for 4 TIME += operations, and MESSAGE43-45 need GetEx/SetEx access which is only available on ICharacterStateVariables (ISP-preferred resolution: inject ICharacterStateVariables rather than extending IVariableStore). The current feature spec has only 1 AC and 2 Tasks, which is grossly insufficient for a 3,518-line migration.

### Goal (What to Achieve)

Implement all 44 SEX-category action ID handlers in a new WcCounterMessageSex class following the F808 extraction pattern (interface + implementation class + DI registration), replacing all stubs in WcCounterMessage.Dispatch() with delegation calls. Resolve the IEngineVariables TIME and IVariableStore EX interface gaps. Ensure behavioral equivalence with the source ERB through comprehensive testing, including MESSAGE40 decomposition into testable sub-methods and WC_VA_FITTING as a public pure function.

| Source File | Approx Lines | Description |
|-------------|:------------:|-------------|
| TOILET_COUNTER_MESSAGE_SEX.ERB | 3518 | SEX-category messages for WC counter |

**Function Classification** (49 ERB functions):
- **44 action-ID handlers** (MESSAGE30-36, 40-47, 50-60, 70-75, 80-83, 91, 500, 502-505, 600-601): Dispatch targets migrated as public HandleMessageN methods on IWcCounterMessageSex
- **1 public pure function** (WC_VA_FITTING): Migrated as VaFitting on IWcCounterMessageSex (AC#5, AC#16, AC#43)
- **4 internal ERB helpers**: Local subroutines called within the 44 handlers; inlined as private methods in WcCounterMessageSex during C# migration. Covered by AC#15 (gte 100 field usages) and AC#56 (6 named sub-methods). These do not require separate dispatch entries.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why are 44 SEX action IDs returning 0? | F805 created WcCounterMessage with stub returns for all SEX action IDs, deferring implementation to F806 | `WcCounterMessage.cs:249-293` |
| 2 | Why was SEX deferred as a single block? | F783 Phase 21 Planning grouped migration scope by ERB file, not by structural complexity | `feature-806.md:29` (Phase 21 scope) |
| 3 | Why is the migration complex? | TOILET_COUNTER_MESSAGE_SEX.ERB has 49 functions including MESSAGE40 (1,412 lines) with 14 external API calls, 28 KOJO hooks, and a multi-variable state machine | `TOILET_COUNTER_MESSAGE_SEX.ERB:68-1479` |
| 4 | Why does WcCounterMessage lack the needed DI dependencies? | The constructor (11 current deps, post-F807) was designed for the dispatch framework, not for SEX handler internals that need ICommonFunctions, IVirginityManager, ILocationService, etc. | `WcCounterMessage.cs:41-53` |
| 5 | Why (Root)? | F805's architectural boundary was placed at the dispatch switch statement rather than at the service composition level, so F806 inherits the full burden of expanding the DI graph and implementing all handler logic without any decomposition guidance | `WcCounterMessage.cs:249-293` (44 stubs) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 44 action IDs return 0 with no message output | Architectural boundary at dispatch level deferred all service composition and handler logic to F806 |
| Where | WcCounterMessage.Dispatch() stubs (lines 249-293) | Missing IWcCounterMessageSex extraction class following F808 pattern |
| Fix | Inline handler code in WcCounterMessage (would cause constructor explosion) | Extract WcCounterMessageSex with own DI graph, delegate from Dispatch() |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| F805 | [DONE] | WC Counter Source + Message Core (defines dispatch stubs) |
| F808 | [DONE] | WC Counter Message ITEM + NTR (provides IWcCounterMessageItem, IWcCounterMessageNtr) |
| F807 | [DONE] | WC Counter Message TEASE (sibling, no cross-calls) |
| F813 | [DRAFT] | Post-Phase Review Phase 21 (successor) |
| F801 | [DONE] | Main Counter Core (ICounterUtilities) |
| F802 | [DONE] | Main Counter Output (ITextFormatting) |
| F811 | [DONE] | IKojoMessageService KojoMessageWcCounter overload |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Predecessor satisfaction | FEASIBLE | F783 [DONE], F805 [DONE], F808 [DONE] |
| Architectural pattern | FEASIBLE | F808 established extraction pattern (interface + class + DI registration) |
| Interface availability | FEASIBLE | 13+ needed interfaces exist (ICommonFunctions, IVirginityManager, ILocationService, IShrinkageSystem, IItemVariables, ICounterUtilities, IWcCounterMessageItem, IWcCounterMessageNtr, ITextFormatting, IKojoMessageService, IVariableStore, ITEquipVariables, IRandomProvider) |
| Interface gap: TIME | NEEDS_REVISION | IEngineVariables lacks GetTime/SetTime; 4 TIME += statements require this |
| Interface gap: EX | NEEDS_REVISION | IVariableStore lacks GetEx/SetEx (exists on ICharacterStateVariables; inject ISP-segregated interface) |
| Scale | FEASIBLE | 3,518 lines is large but F808 (3,479 lines combined) was completed successfully at similar scale |
| Testability | FEASIBLE | All dependencies injectable via Moq; WC_VA_FITTING is a pure function |
| Scope clarity | FEASIBLE | 49 functions clearly identified, all 83 action IDs enumerated in F805 dispatch |

**Verdict**: FEASIBLE

**Note**: ICounterUtilities and IItemVariables are direct constructor dependencies of WcCounterMessageSex (used for item checks and counter utilities across multiple handlers).

**Volume Override**: TOILET_COUNTER_MESSAGE_SEX.ERB (3,518 lines, 66 ACs) exceeds erb template guidelines (~500 lines, 8-15 ACs). Justification: all 83 action IDs belong to a single ERB file forming one functional unit; splitting would fragment the delegation pattern and create cross-feature dependencies within a single dispatch switch. F808 (3,479 lines combined) was completed successfully at comparable scale.

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| WcCounterMessage.cs | HIGH | 44 stub methods replaced with delegation to IWcCounterMessageSex |
| IEngineVariables.cs | MEDIUM | Add GetTime/SetTime methods (interface extension) |
| ICharacterStateVariables (existing) | LOW | Injected into WcCounterMessageSex for GetEx/SetEx access (ISP-preferred over extending IVariableStore) |
| ServiceCollectionExtensions.cs | LOW | Register IWcCounterMessageSex implementation |
| Era.Core Counter namespace | HIGH | New WcCounterMessageSex class (largest handler class in Phase 21) |
| Kojo integration | LOW | 28 TRYCALLFORM dispatch points map to existing IKojoMessageService |
| External consumers | LOW | WC_VA_FITTING must be public (called by 9 kojo ERB files) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| IEngineVariables lacks GetTime/SetTime | `IEngineVariables.cs` (no TIME accessor) | 4 TIME += statements in ERB cannot be migrated without extension |
| IVariableStore lacks GetEx/SetEx | `IVariableStore.cs` (exists on ICharacterStateVariables) | MESSAGE43-45 check EX counters; inject ICharacterStateVariables (ISP-preferred) |
| WC_VA_FITTING is cross-file callable | 9 kojo ERB files call WC_VA_FITTING externally | Must be exposed as public method on IWcCounterMessageSex |
| Delegation pattern mandatory | WcCounterMessage constructor already has 10 deps | Inject IWcCounterMessageSex to avoid constructor explosion (>17 params) |
| MESSAGE40 monolithic structure | 1,412 lines with 3 local vars as persistent state | Must decompose into 6+ private sub-methods for testability (AC#56 verifies 6 named methods) |
| KOJO dispatch requires action type constants | 28 unique TRYCALLFORM dispatch patterns | Need actionType constant definitions for KojoMessageWcCounter calls |
| KOJO 3-param overload possible | Some KOJO patterns pass extra param | IKojoMessageService may need 3-param KojoMessageWcCounter overload |
| State mutations beyond text output | CFLAG, TALENT, BASE, STAIN, DOWNBASE, EQUIP, TCVAR, TEQUIP mutations | Handlers are not pure text generators; they modify game state |
| DOWNBASE additive semantics | 12 DOWNBASE += statements | Requires SetDownbase with additive composition |
| STAIN bitwise OR | 6 STAIN assignments using OR | Requires GetStain + SetStain with bitwise composition |
| SETBIT/GETBIT operations | 30+ SETBIT/GETBIT calls for WC_同席者, WC_あなたエロ写真, etc. | Requires BitfieldUtility (exists) |
| TreatWarningsAsErrors | Directory.Build.props | All new files must compile clean |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| MESSAGE40 translation errors due to 1,413-line length | HIGH | HIGH | Decompose into 6+ private sub-methods (AC#56 verifies 6 named methods); test each independently |
| DI constructor explosion in WcCounterMessage | HIGH | MEDIUM | Use delegation pattern: inject IWcCounterMessageSex (not individual deps) |
| Interface additions (TIME/EX) break existing consumers | LOW | HIGH | Add as default interface methods or verify no other implementors |
| WC_VA_FITTING correctness regression | LOW | HIGH | Pure function; create exhaustive unit test matrix covering all branches |
| sexualDesire location mutation side effects | MEDIUM | HIGH | Carefully model CFLAG:現在位置 mutations with explicit SetCharacterFlag tests |
| KOJO dispatch pattern mismatch | MEDIUM | MEDIUM | Verify all 28 unique TRYCALLFORM patterns map to actionType constants; use existing 2-param KojoMessageWcCounter overload |
| EQUIP/TEQUIP named index constants incomplete | MEDIUM | LOW | Follow WcCounterMessageItem pattern from F808 |
| Missing test equivalence data for complex branches | HIGH | HIGH | Prioritize simple handlers first (MESSAGE50-601); build up to MESSAGE40 |
| State mutation bugs (TALENT/CFLAG direct writes) | MEDIUM | HIGH | Test mutations explicitly with mock verification |
| Cross-repo NuGet dependency from interface additions | LOW | MEDIUM | Interface changes are in Era.Core (same repo as implementation) |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Stub count | Grep for `=> 0` in WcCounterMessage.cs SEX region | 44 | All 44 SEX action IDs return 0 (TEASE action IDs replaced by F807) |
| ERB function count | Count @ functions in TOILET_COUNTER_MESSAGE_SEX.ERB | 49 | Target migration scope |
| ERB line count | wc -l TOILET_COUNTER_MESSAGE_SEX.ERB | 3518 | Source complexity metric |
| Existing test count | dotnet test WcCounterMessage.Tests --list-tests | 0 | No WcCounterMessageSex tests exist before F806 |

**Baseline File**: `_out/tmp/baseline-806.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | 44 action IDs must dispatch to non-zero-returning handlers | `WcCounterMessage.cs:249-293` | AC must verify every stub is replaced with delegation call |
| C2 | IWcCounterMessageItem calls use TRYCALL semantics (null-safe) | `TOILET_COUNTER_MESSAGE_SEX.ERB:407,1540,1815,1903,2037,2578` | AC must verify BottomClothOff/BottomClothOut called correctly across all 6 handler call sites; AC#83 verifies direct invocation syntax in implementation (null-conditional changed to direct per Phase7-Debug iter1; DI non-nullable registration guarantees TRYCALL semantics) |
| C3 | IWcCounterMessageNtr.WithFirstSex called for 13 characters | `TOILET_COUNTER_MESSAGE_SEX.ERB:1492-1516` | AC must verify all 13 character dispatches |
| C4 | WC_VA_FITTING must return same values as ERB | `TOILET_COUNTER_MESSAGE_SEX.ERB:3097-3174` | AC must include equivalence test for pure function |
| C5 | LOST_VIRGIN/LOST_VIRGIN_M/LOST_VIRGIN_A called with correct VirginityType | `TOILET_COUNTER_MESSAGE_SEX.ERB:1079,1468,3087` | AC must verify IVirginityManager called with correct types |
| C6 | DOWNBASE, STAIN, TIME mutations must match ERB values | Various lines in ERB | AC must verify stat changes match ERB formulas |
| C7 | sexualDesire location routing must cover all 26 branches | `TOILET_COUNTER_MESSAGE_SEX.ERB:2580-2835` | AC must test representative location/character combinations |
| C8 | MESSAGE40 photography system (SETBIT WC_あなた処女喪失写真/WC_あなたエロ写真) | Various lines in MESSAGE40 | AC must verify correct bit flags set |
| C9 | Simple handlers (MESSAGE50-601) must produce correct text | `TOILET_COUNTER_MESSAGE_SEX.ERB:3179-3518` | Each short handler needs AC for conditional output |
| C10 | No TODO/FIXME/HACK comments in migrated files | Feature standard | Standard code quality AC |
| C11 | IEngineVariables TIME accessor required | Interface Dependency Scan | AC must verify GetTime/SetTime added to IEngineVariables |
| C12 | EX variable access required (MESSAGE43-45) | Interface Dependency Scan | AC must verify ICharacterStateVariables injected for GetEx/SetEx access |
| C13 | WC_VA_FITTING must be public (cross-file callable) | Interface Dependency Scan: 9 kojo ERB files call externally | AC must verify method is on IWcCounterMessageSex interface |
| C14 | HasPenis/HasVagina branching in 30+ locations | `TOILET_COUNTER_MESSAGE_SEX.ERB` | AC must cover both gender branch paths |
| C15 | WC_応答分類 8 response categories in MESSAGE40 | `TOILET_COUNTER_MESSAGE_SEX.ERB:68-1479` | AC must test representative cases per category |
| C16 | 28 TRYCALLFORM KOJO dispatch patterns with action type constants | `TOILET_COUNTER_MESSAGE_SEX.ERB` (28 unique patterns) | AC must verify representative KojoMessageWcCounter action type mappings |

**Mutation AC Coverage Note**: Technical Constraint lists 8 mutation types. Dedicated behavioral test ACs: CFLAG (AC#59/62), TALENT (AC#60/63), DOWNBASE (AC#18/67), STAIN (AC#19/71/73 — AC#71 verifies SetStain gte 1, AC#73 verifies GetStain read-side for bitwise OR), TIME (AC#20/34/57). BASE/DOWNBASE implementation covered by AC#67 (SetBase|SetDownbase gte 1). EQUIP/TEQUIP covered by AC#52 (injection) + AC#68 (SetEquip|SetTEquip gte 1 impl) + AC#75 (test-side SetEquip|SetTEquip gte 1 in WcCounterMessageSexTests.cs). TCVAR covered by AC#50 (injection) + AC#69 (SetTCVar gte 1 impl) + AC#74 (test-side SetTCVar|TCVAR gte 1 in WcCounterMessageSexTests.cs). EX counter read access (MESSAGE43-45) covered by AC#66 (GetEx gte 3 in implementation) + AC#72 (test-side GetEx|絶頂 gte 3 in WcCounterMessageSexTests.cs). Proxy AC#15 (gte 100 field usages) covers structural completeness. AC#39 (all tests pass) provides runtime correctness guarantee for all mutations.

### Constraint Details

**C1: Dispatch Stub Replacement**
- **Source**: F805 created 44 stubs returning 0 in WcCounterMessage.Dispatch() (lines 249-293)
- **Verification**: Grep WcCounterMessage.cs for `=> 0` in SEX action ID range; count should be 0 after migration
- **AC Impact**: AC must verify delegation to IWcCounterMessageSex for each action ID group

**C4: WC_VA_FITTING Equivalence**
- **Source**: ERB lines 3097-3174 define a #FUNCTION with RETURNF (pure function returning fitting level 0-4)
- **Verification**: Compare C# output with ERB output for all input combinations
- **AC Impact**: Exhaustive unit test matrix for all parameter combinations

**C5: Virginity Manager Integration**
- **Source**: ERB calls LOST_VIRGIN_M (12 times), LOST_VIRGIN (2 times), LOST_VIRGIN_A (1 time)
- **Verification**: Verify IVirginityManager.CheckLostVirginity called with correct VirginityType enum values
- **AC Impact**: Mock verification for each virginity type variant

**C7: Location Routing Coverage**
- **Source**: sexualDesire (ERB lines 2580-2835) has 26 SELECTCASE branches with 12 character-specific room mappings
- **Verification**: Unit tests covering representative location/character combinations
- **AC Impact**: Test matrix for location routing branches

**C11: TIME Interface Extension**
- **Source**: 4 TIME += statements at ERB lines 1742, 2376, 2406, 3064
- **Verification**: Grep IEngineVariables.cs for GetTime/SetTime methods
- **AC Impact**: Interface extension must be backward compatible

**C12: EX Variable Access**
- **Source**: MESSAGE43/44/45 check EX counters (EX:MASTER:Ａ絶頂, Ｃ絶頂, Ｂ絶頂); ICharacterStateVariables already has GetEx/SetEx
- **Verification**: Verify WcCounterMessageSex injects ICharacterStateVariables for EX access
- **AC Impact**: ISP-preferred: inject ICharacterStateVariables (not extend IVariableStore)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| Predecessor | F805 | [DONE] | WC Counter Source + Message Core -- defines dispatch stubs that F806 replaces |
| Predecessor | F808 | [DONE] | WC Counter Message ITEM + NTR -- provides IWcCounterMessageItem and IWcCounterMessageNtr called from SEX ERB (SEX.ERB:407,1540,1815,1903,2037,2578 TRYCALL BottomClothOff/BottomClothOut; SEX.ERB:1492-1516 TRYCALL WithFirstSex x13; SEX.ERB:2499 TRYCALL WithPetting) |
| Related | F807 | [DONE] | WC Counter Message TEASE (sibling, no cross-calls between SEX and TEASE) |
| Successor | F813 | [DRAFT] | Post-Phase Review Phase 21 |
| Related | F801 | [DONE] | Main Counter Core (provides ICounterUtilities) |
| Related | F802 | [DONE] | Main Counter Output (provides ITextFormatting) |

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

<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Each ERB message category is the SSOT for its C# migration target, ensuring no behavioral regression" | IWcCounterMessageSex interface + implementation must exist with 44 action-ID handlers + VaFitting directly verified as non-stub methods; 4 internal ERB helpers covered indirectly via AC#15 field usage proxy (inlined as private methods during migration). WcCounterMessage.Dispatch() must delegate to IWcCounterMessageSex for all 44 SEX-category action IDs. **Coverage rationale**: AC#49 (gte 18 string-equality assertions) provides 2+ assertions per 9 handler groups ensuring no group is entirely uncovered; AC#76 (gte 18 co-located `// ERB:{line}` annotations) enforces ERB-source traceability for each assertion; AC#39 (all tests pass) provides runtime correctness guarantee; AC#44/AC#47/AC#48 (per-group test counts) ensure test existence at the handler level. Grep-based matchers cannot enforce per-assertion value correctness (resolved-skipped iter7 AC#49 matcher breadth limitation); the ERB-derivation mandate in Task#9/Task#10 descriptions and AC#76 co-location requirement provide the human-auditable correctness bridge. **TIME limitation**: AC#39 verifies mock-level correctness only; the 4 TIME += operations depend on engine-level IEngineVariables.GetTime/SetTime override for runtime behavioral equivalence (tracked in Mandatory Handoffs → F813, DIM Backward Compatibility Note) | AC#1-AC#8, AC#14-AC#15, AC#30-AC#31, AC#44-AC#49, AC#56, AC#58, AC#62-AC#65, AC#67-AC#73, AC#76-AC#77, AC#83 |
| "full DI and equivalence testing" | Unit test file must exist with Assert/Verify calls covering dispatch, handlers, state mutations, virginity, location routing, photography, and pure function equivalence | AC#9-AC#13, AC#16-AC#29, AC#40-AC#42, AC#50-AC#55, AC#59-AC#61, AC#74-AC#75, AC#78, AC#81-AC#82 |
| "Resolve the IEngineVariables TIME and IVariableStore EX interface gaps" | IEngineVariables must declare GetTime/SetTime; ICharacterStateVariables (already has GetEx/SetEx) must be injected into WcCounterMessageSex; GetEx() must be called for MESSAGE43-45 | AC#14, AC#32-AC#34, AC#57, AC#66, AC#72 |
| "WC_VA_FITTING as a public pure function" | WC_VA_FITTING must appear on IWcCounterMessageSex interface and have exhaustive unit tests | AC#5, AC#16, AC#43 |
| "comprehensive testing" | Both positive and negative tests required for erb type. Positive test ACs are attributed to rows 1 and 2 above; this row covers the negative/absence test ACs supplementing them (absence of stubs, TODO markers, NotImplementedException) plus the runtime correctness gates (build + all tests pass) | AC#8, AC#30-AC#31, AC#35-AC#39, AC#79, AC#80 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IWcCounterMessageSex interface file exists | file | Glob(Era.Core/Counter/IWcCounterMessageSex.cs) | exists | - | [x] |
| 2 | WcCounterMessageSex implementation class exists | code | Grep(Era.Core/Counter/) | matches | `class WcCounterMessageSex.*IWcCounterMessageSex` | [x] |
| 3 | WcCounterMessageSex unit test file exists | file | Glob(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs) | exists | - | [x] |
| 4 | IWcCounterMessageSex registered in DI | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `AddSingleton.*IWcCounterMessageSex.*WcCounterMessageSex` | [x] |
| 5 | IWcCounterMessageSex declares VaFitting as public method (C13) | code | Grep(Era.Core/Counter/IWcCounterMessageSex.cs) | matches | `VaFitting` | [x] |
| 6 | WcCounterMessage injects IWcCounterMessageSex for delegation | code | Grep(Era.Core/Counter/WcCounterMessage.cs) | matches | `IWcCounterMessageSex` | [x] |
| 7 | All 44 SEX action ID stubs replaced with delegation (Pos) | code | Grep(Era.Core/Counter/WcCounterMessage.cs, pattern="=> _sex\.HandleMessage") | gte | 44 | [x] |
| 8 | No SEX action ID stubs return literal 0 (Neg) | code | Grep(path="Era.Core/Counter/WcCounterMessage.cs", pattern="^\s+(30|31|32|33|34|35|36|40|41|42|43|44|45|46|47|50|51|52|53|54|55|56|57|58|59|60|70|71|72|73|74|75|80|81|82|83|91|500|502|503|504|505|600|601) => 0") | not_matches | - | [x] |
| 9 | WcCounterMessageSexTests contains Assert or Verify calls | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs) | matches | `Assert\.|\.Verify\(` | [x] |
| 10 | WcCounterMessageSex injects IVirginityManager (C5) | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `IVirginityManager` | [x] |
| 11 | WcCounterMessageSex injects ILocationService (C7) | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `ILocationService` | [x] |
| 12 | WcCounterMessageSex injects IWcCounterMessageItem (C2) | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `IWcCounterMessageItem` | [x] |
| 13 | WcCounterMessageSex injects IWcCounterMessageNtr (C3) | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `IWcCounterMessageNtr` | [x] |
| 14 | WcCounterMessageSex injects ICharacterStateVariables for EX access (C12) | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `ICharacterStateVariables` | [x] |
| 15 | WcCounterMessageSex implementation contains non-stub logic (service usage count) | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="_variables\.\|_console\.\|_engine\.\|_virginity\.\|_location\.\|_item\.\|_ntr\.\|_common\.\|_shrinkage\.\|_textFormatting\.\|_kojoMessage\.\|_characterState\.\|_equip\.\|_random\.") | gte | 100 | [x] |
| 16 | VaFitting pure function tested with multiple input combinations (C4) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="VaFitting") | gte | 5 | [x] |
| 17 | Tests verify IVirginityManager.CheckLostVirginity calls (C5) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="CheckLostVirginity\|LostVirginity\|VirginityType") | gte | 3 | [x] |
| 18 | Tests verify BASE/DOWNBASE mutation (C6) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SetBase\|SetDownbase\|Downbase\|DownBase") | gte | 2 | [x] |
| 19 | Tests verify STAIN mutation (C6) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SetStain\|Stain") | gte | 2 | [x] |
| 20 | Tests verify TIME mutation (C6) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SetTime\|GetTime\|TIME") | gte | 2 | [x] |
| 21 | Tests verify sexualDesire location routing (C7) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SexualDesire\|sexualDesire\|Location") | gte | 10 | [x] |
| 22 | Tests verify photography SETBIT operations (C8) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SetBit\|BitfieldUtility") | gte | 2 | [x] |
| 23 | Tests verify simple handlers MESSAGE50-601 output (C9) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="Message5[0-9]\|Message60\|Message7[0-5]\|Message8[0-3]\|Message91\|Message500\|Message502\|Message503\|Message504\|Message505\|Message600\|Message601") | gte | 15 | [x] |
| 24 | Tests verify WC_応答分類 response category branching (C15) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="応答分類\|ResponseCategory\|WcResponseCategory") | gte | 3 | [x] |
| 25 | Tests verify HasPenis/HasVagina gender branching (C14) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="HasPenis\|HasVagina") | gte | 2 | [x] |
| 26 | Tests verify IWcCounterMessageItem.BottomClothOff/BottomClothOut calls (C2, 6 ERB call sites) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="BottomClothOff\|BottomClothOut") | gte | 6 | [x] |
| 27 | WcCounterMessageSexTests includes 13 WithFirstSex character dispatch tests (C3) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="WithFirstSex") | gte | 13 | [x] |
| 28 | Tests verify KOJO dispatch via IKojoMessageService mock.Verify (C16) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="Verify.*KojoMessage\|KojoMessageWcCounter.*Times") | gte | 4 | [x] |
| 29 | WcCounterMessageSex uses BitfieldUtility for SETBIT/GETBIT (C8) | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `BitfieldUtility` | [x] |
| 30 | No NotImplementedException stubs in SEX handler (Neg) | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | not_matches | `NotImplementedException` | [x] |
| 31 | No NotImplementedException stubs in IWcCounterMessageSex (Neg) | code | Grep(Era.Core/Counter/IWcCounterMessageSex.cs) | not_matches | `NotImplementedException` | [x] |
| 32 | IEngineVariables declares GetTime method (C11) | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | `GetTime` | [x] |
| 33 | IEngineVariables declares SetTime method (C11) | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | `SetTime` | [x] |
| 34 | WcCounterMessageSex calls SetTime at all 4 TIME += points (C11) | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="\.SetTime\(") | gte | 4 | [x] |
| 35 | No TODO/FIXME/HACK in WcCounterMessageSex (C10, Neg) | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | not_matches | `TODO\|FIXME\|HACK` | [x] |
| 36 | No TODO/FIXME/HACK in IWcCounterMessageSex (C10, Neg) | code | Grep(Era.Core/Counter/IWcCounterMessageSex.cs) | not_matches | `TODO\|FIXME\|HACK` | [x] |
| 37 | No TODO/FIXME/HACK in WcCounterMessageSexTests (C10, Neg) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs) | not_matches | `TODO\|FIXME\|HACK` | [x] |
| 38 | C# solution builds without errors | build | dotnet build Era.Core/ | succeeds | - | [x] |
| 39 | All unit tests pass | test | dotnet test Era.Core.Tests/ --blame-hang-timeout 10s | succeeds | - | [x] |
| 40 | WcCounterMessageSex injects IKojoMessageService (C16) | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `IKojoMessageService` | [x] |
| 41 | WcCounterMessageSex injects IShrinkageSystem | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `IShrinkageSystem` | [x] |
| 42 | WcCounterMessageSex injects ICommonFunctions | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `ICommonFunctions` | [x] |
| 43 | VaFitting returns ERB-equivalent values for known input/output pairs | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="Assert.*Equal.*VaFitting\|VaFitting.*Returns\|VaFitting.*Expected") | gte | 3 | [x] |
| 44 | Tests cover MESSAGE30-36 handler output variants | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="Message3[0-6]\|HandleMessage3") | gte | 7 | [x] |
| 45 | Dispatch() signature includes CharacterId offender parameter | code | Grep(Era.Core/Counter/WcCounterMessage.cs, pattern="Dispatch.*int action.*CharacterId offender") | matches | - | [x] |
| 46 | SendMessage() passes offender to Dispatch() call site | code | Grep(Era.Core/Counter/WcCounterMessage.cs, pattern="Dispatch.*action.*offender") | matches | - | [x] |
| 47 | Tests cover MESSAGE41-47 handler variants | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="Message4[1-7]\|HandleMessage4[1-7]") | gte | 7 | [x] |
| 48 | Tests cover MESSAGE40 handler entry point | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="Message40\|HandleMessage40") | gte | 3 | [x] |
| 49 | WcCounterMessageSexTests includes 18+ string-equality assertions verifying handler text output per handler group (MESSAGE30-36, 40, 41-47, 50-60, 70-75, 80-83, 91, 500/502-505, 600-601) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="Assert\.Equal.*\"\|Assert\.Contains.*\"") | gte | 18 | [x] |
| 50 | WcCounterMessageSex injects IVariableStore | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `IVariableStore` | [x] |
| 51 | WcCounterMessageSex injects IEngineVariables | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `IEngineVariables` | [x] |
| 52 | WcCounterMessageSex injects ITEquipVariables | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `ITEquipVariables` | [x] |
| 53 | WcCounterMessageSex injects ITextFormatting | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `ITextFormatting` | [x] |
| 54 | WcCounterMessageSex injects IConsoleOutput | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `IConsoleOutput` | [x] |
| 55 | WcCounterMessageSex injects IRandomProvider | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `IRandomProvider` | [x] |
| 56 | MESSAGE40 decomposed into 6 named private sub-methods | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="private void.*ForcedOrgasm\|private void.*SexualDesire\|private void.*WithOther\|private void.*HandleVirginity\|private void.*HandlePhotography\|private void.*HandleResponseCategory") | gte | 6 | [x] |
| 57 | WcCounterMessageSex calls GetTime for TIME read-modify-write at 4 migration points | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="\.GetTime\(\)") | gte | 4 | [x] |
| 58 | All 44 handler methods declared in WcCounterMessageSex.cs | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="HandleMessage(3[0-6]\|4[0-7]\|5[0-9]\|60\|7[0-5]\|8[0-3]\|91\|500\|502\|503\|504\|505\|600\|601)\\b") | gte | 44 | [x] |
| 59 | Tests verify CFLAG mutation calls (sexualDesire location routing) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SetCharacterFlag\|SetCflag\|CFLAG\|現在位置") | gte | 2 | [x] |
| 60 | Tests verify TALENT mutation calls | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SetTalent\|TALENT") | gte | 2 | [x] |
| 61 | Tests verify IConsoleOutput.PrintLine called with correct output text | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="Verify.*PrintLine\|PrintLine.*Times") | gte | 5 | [x] |
| 62 | WcCounterMessageSex.cs calls SetCharacterFlag for CFLAG mutations | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="SetCharacterFlag\|SetCflag") | gte | 1 | [x] |
| 63 | WcCounterMessageSex.cs calls SetTalent for TALENT mutations | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="SetTalent") | gte | 1 | [x] |
| 64 | WcCounterMessageSex.cs calls WithPetting for SEX.ERB:2499 | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="\.WithPetting") | gte | 1 | [x] |
| 65 | Tests verify WithPetting mock call | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="WithPetting") | gte | 1 | [x] |
| 66 | WcCounterMessageSex.cs calls GetEx for MESSAGE43-45 EX counter checks | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="\.GetEx\(") | gte | 3 | [x] |
| 67 | Implementation calls SetBase/SetDownbase | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="SetBase\|SetDownbase") | gte | 1 | [x] |
| 68 | Implementation calls SetEquip/SetTEquip | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="SetEquip\|SetTEquip") | gte | 1 | [x] |
| 69 | Implementation calls SetTCVar | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="SetTCVar") | gte | 1 | [x] |
| 70 | Simple handler methods declared in WcCounterMessageSex | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="HandleMessage(3[0-6]\|5[0-9]\|60\|7[0-5]\|8[0-3]\|91\|500\|502\|503\|504\|505\|600\|601)\\b") | gte | 36 | [x] |
| 71 | Implementation calls SetStain | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="SetStain") | gte | 1 | [x] |
| 72 | Tests verify GetEx counter access for MESSAGE43-45 | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="GetEx\|絶頂") | gte | 3 | [x] |
| 73 | Implementation calls GetStain for bitwise OR read-modify-write | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="\.GetStain\(") | gte | 1 | [x] |
| 74 | Tests verify SetTCVar mutation calls | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SetTCVar\|TCVAR") | gte | 1 | [x] |
| 75 | Tests verify SetEquip/SetTEquip mutation calls | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SetEquip\|SetTEquip") | gte | 1 | [x] |
| 76 | Test assertions include co-located ERB source annotations | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="Assert\\.(?:Equal\|Contains).*// ERB:") | gte | 18 | [x] |
| 77 | Tests reference MESSAGE40 private sub-method names (4+ of 6) | code | Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="ForcedOrgasm\|SexualDesire\|WithOther\|HandleVirginity\|HandlePhotography\|HandleResponseCategory") | gte | 4 | [x] |
| 78 | Tests verify WcCounterMessage delegates to IWcCounterMessageSex with correct offender | code | Grep(Era.Core.Tests/, pattern="HandleMessage.*offender\|_sex.*Verify.*offender\|WcCounterMessage.*Dispatch.*offender") | gte | 1 | [x] |
| 79 | No TODO/FIXME/HACK in modified IEngineVariables.cs | code | Grep(Era.Core/Interfaces/IEngineVariables.cs, pattern="TODO\|FIXME\|HACK") | not_matches | - | [x] |
| 80 | No TODO/FIXME/HACK in modified WcCounterMessage.cs | code | Grep(Era.Core/Counter/WcCounterMessage.cs, pattern="TODO\|FIXME\|HACK") | not_matches | - | [x] |
| 81 | WcCounterMessageSex injects IItemVariables | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `IItemVariables` | [x] |
| 82 | WcCounterMessageSex injects ICounterUtilities | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs) | matches | `ICounterUtilities` | [x] |
| 83 | BottomClothOff/BottomClothOut invocations present for all 6 C2 TRYCALL sites | code | Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="\.BottomClothOff\|\.BottomClothOut") | gte | 6 | [x] |

### AC Details

**AC#1: IWcCounterMessageSex interface file exists**
- **Test**: `Glob(Era.Core/Counter/IWcCounterMessageSex.cs)`
- **Expected**: File exists
- **Rationale**: Following F808 extraction pattern, a dedicated interface is required for the SEX handler. This interface enables DI injection into WcCounterMessage and cross-file callability of WC_VA_FITTING (C13).

**AC#2: WcCounterMessageSex implementation class exists**
- **Test**: `Grep(Era.Core/Counter/)` for `class WcCounterMessageSex.*IWcCounterMessageSex`
- **Expected**: Pattern matches
- **Rationale**: Concrete implementation class must exist and implement the interface. Follows F808 pattern.

**AC#3: WcCounterMessageSex unit test file exists**
- **Test**: `Glob(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs)`
- **Expected**: File exists
- **Rationale**: TDD requirement -- SEX handler must have unit tests.

**AC#4: IWcCounterMessageSex registered in DI**
- **Test**: `Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs)` for `AddSingleton.*IWcCounterMessageSex.*WcCounterMessageSex`
- **Expected**: Pattern matches
- **Rationale**: DI registration required for WcCounterMessage to receive IWcCounterMessageSex via constructor injection.

**AC#5: IWcCounterMessageSex declares VaFitting as public method (C13)**
- **Test**: `Grep(Era.Core/Counter/IWcCounterMessageSex.cs)` for `VaFitting`
- **Expected**: Pattern matches
- **Rationale**: WC_VA_FITTING is called by 9 kojo ERB files externally, so it must be a public interface method (C13).

**AC#6: WcCounterMessage injects IWcCounterMessageSex**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessage.cs)` for `IWcCounterMessageSex`
- **Expected**: Pattern matches
- **Rationale**: Delegation pattern avoids constructor explosion (>17 params). WcCounterMessage delegates to IWcCounterMessageSex.

**AC#7: All 44 SEX action ID stubs replaced with delegation (Pos)**
- **Test**: Count delegation calls to the SEX handler field in WcCounterMessage.cs >= 44
- **Expected**: Each of the 44 action IDs has a delegation call
- **Rationale**: C1 requires all 44 stubs replaced with non-zero-returning handlers.

**AC#8: No SEX action ID stubs return literal 0 (Neg)**
- **Test**: Verify no F806-scope action IDs still have `=> 0` patterns
- **Expected**: Pattern not found
- **Rationale**: Negative complement to AC#7. Ensures no stub was accidentally left behind.

**AC#9: WcCounterMessageSexTests contains Assert or Verify calls**
- **Test**: Count assertion patterns in test file
- **Expected**: Pattern found
- **Rationale**: Tests must contain actual assertions, not just setup code.

**AC#10: WcCounterMessageSex injects IVirginityManager (C5)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `IVirginityManager`
- **Expected**: Pattern matches
- **Rationale**: C5 requires IVirginityManager for CheckLostVirginity calls (LOST_VIRGIN/LOST_VIRGIN_M/LOST_VIRGIN_A). Constructor injection is required for testability.

**AC#11: WcCounterMessageSex injects ILocationService (C7)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `ILocationService`
- **Expected**: Pattern matches
- **Rationale**: C7 requires ILocationService for sexualDesire 26-branch location routing. Constructor injection enables mock substitution in tests.

**AC#12: WcCounterMessageSex injects IWcCounterMessageItem (C2)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `IWcCounterMessageItem`
- **Expected**: Pattern matches
- **Rationale**: C2 requires IWcCounterMessageItem for BottomClothOff/BottomClothOut TRYCALL semantics.

**AC#13: WcCounterMessageSex injects IWcCounterMessageNtr (C3)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `IWcCounterMessageNtr`
- **Expected**: Pattern matches
- **Rationale**: C3 requires IWcCounterMessageNtr for WithFirstSex calls covering 13 characters.

**AC#14: WcCounterMessageSex injects ICharacterStateVariables for EX access (C12)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `ICharacterStateVariables`
- **Expected**: Pattern matches
- **Rationale**: C12 requires ICharacterStateVariables for GetEx/SetEx access in MESSAGE43-45. ISP-preferred over extending IVariableStore.

**AC#15: Implementation depth verification**
- **Test**: Count service field usages >= 100
- **Expected**: Substantial service interaction indicating full implementation
- **Rationale**: 3,518 lines of ERB logic requires extensive service calls. A low count indicates incomplete migration. Threshold of 100 is a structural proxy (not ERB-line-derived); AC#39 (all tests pass) and AC#49 (18+ string-equality assertions) provide the authoritative behavioral completeness gates.

**AC#16: VaFitting pure function equivalence (C4)**
- **Test**: Count VaFitting references in test file >= 5
- **Expected**: Multiple test cases covering fitting levels 0-4
- **Rationale**: WC_VA_FITTING is a pure function (ERB lines 3097-3174) that can be tested exhaustively.

**AC#17: Virginity manager integration (C5)**
- **Test**: Count virginity-related patterns in test file >= 3
- **Expected**: Tests for LOST_VIRGIN, LOST_VIRGIN_M, and LOST_VIRGIN_A
- **Rationale**: IVirginityManager.CheckLostVirginity must be called with correct VirginityType for each variant.

**AC#18: Tests verify BASE/DOWNBASE mutation (C6)**
- **Test**: Count `SetBase|SetDownbase|Downbase|DownBase` patterns in WcCounterMessageSexTests.cs >= 2
- **Expected**: gte 2
- **Derivation**: BASE/DOWNBASE += appears at 12+ ERB locations; gte 2 tests ensures at least 2 mutation scenarios (different handlers or different delta values) are verified. Pattern includes both SetBase and SetDownbase to match AC#67's implementation-side scope (SetBase|SetDownbase gte 1).
- **Rationale**: C6 requires BASE/DOWNBASE mutations match ERB values. Additive semantics must be verified with mock assertions. AC#67 (implementation side) covers both SetBase and SetDownbase; AC#18 (test side) must cover both to maintain the impl+test pair pattern.

**AC#19: Tests verify STAIN mutation (C6)**
- **Test**: Count `SetStain|Stain` patterns in WcCounterMessageSexTests.cs >= 2
- **Expected**: gte 2
- **Derivation**: STAIN appears at 6 ERB assignment locations using bitwise OR; gte 2 tests ensures both the OR-composition semantics and distinct handlers are verified
- **Rationale**: C6 requires STAIN mutations use bitwise OR semantics. Mock verification confirms correct GetStain + SetStain composition.

**AC#20: Tests verify TIME mutation (C6)**
- **Test**: Count `SetTime|GetTime|TIME` patterns in WcCounterMessageSexTests.cs >= 2
- **Expected**: gte 2
- **Derivation**: TIME += appears at 4 ERB lines (1742, 2376, 2406, 3064); gte 2 tests ensures at least 2 of the 4 TIME mutation points are covered by separate test scenarios
- **Rationale**: C6 and C11 require TIME mutations match ERB values. SetTime is the new IEngineVariables method added in this feature.

**AC#21: Location routing (C7)**
- **Test**: Count location-related test patterns >= 10
- **Expected**: Representative location/character combinations (~38% of 26 SELECTCASE branches)
- **Rationale**: sexualDesire has 26 SELECTCASE branches with 12 character-specific room mappings. gte 10 ensures representative coverage of the highest-risk branching path.

**AC#22: Photography system (C8)**
- **Test**: Count photography-related patterns in test file >= 2
- **Expected**: BitfieldUtility.SetBit calls verified for photo flags
- **Rationale**: MESSAGE40 sets WC_あなた処女喪失写真 and WC_あなたエロ写真 bit flags.

**AC#23: Simple handler coverage (C9)**
- **Test**: Count handler test references >= 15
- **Expected**: gte 15
- **Derivation**: MESSAGE50-601 contains 29 distinct handlers. C9 requires "each short handler needs AC for conditional output." gte 15 ensures at least half the handlers have named test references, providing meaningful coverage across all handler sub-groups (MESSAGE50-60, 70-75, 80-83, 91, 500, 502-505, 600-601).
- **Rationale**: Simple handlers produce conditional text output that must be verified. The threshold of 15 (vs. the original 5) ensures representative coverage across all sub-groups rather than concentrating tests on only a few handlers.

**AC#24: Tests verify WC_応答分類 response category branching (C15)**
- **Test**: Count `応答分類|ResponseCategory|WcResponseCategory` patterns in WcCounterMessageSexTests.cs >= 3
- **Expected**: gte 3
- **Derivation**: WC_応答分類 has 8 response categories (ERB lines 68-1479); gte 3 tests ensures at least 3 distinct category values are exercised, covering the main positive, neutral, and negative response paths
- **Rationale**: C15 requires representative cases per category. With 8 categories, 3+ tests provides meaningful coverage of the SELECTCASE dispatch.

**AC#25: Tests verify HasPenis/HasVagina gender branching (C14)**
- **Test**: Count `HasPenis|HasVagina` patterns in WcCounterMessageSexTests.cs >= 2
- **Expected**: gte 2
- **Derivation**: HasPenis/HasVagina branching appears at 30+ ERB locations; gte 2 tests ensures both gender branch paths (one HasPenis=true test and one HasVagina=true test) are verified
- **Rationale**: C14 requires both gender branch paths are covered. These two tests form the minimal positive coverage for the binary gender dispatch.

**AC#26: Tests verify IWcCounterMessageItem.BottomClothOff/BottomClothOut calls (C2, 6 ERB call sites)**
- **Test**: Count `BottomClothOff|BottomClothOut` patterns in WcCounterMessageSexTests.cs >= 6
- **Expected**: gte 6
- **Derivation**: 6 TRYCALL call sites across handlers: 2 BottomClothOff at ERB:407 (MESSAGE40 ForcedOrgasm_CVA) and ERB:2578 (sexualDesire), 4 BottomClothOut at ERB:1540 (MESSAGE41 ForcedOrgasm), ERB:1815 (MESSAGE42), ERB:1903 (MESSAGE43), ERB:2037 (MESSAGE44). gte 6 tests ensures each call site is separately verified with mock.Verify().
- **Rationale**: C2 requires TRYCALL semantics are correct at all call sites, not just representative ones. Each handler's delegation to IWcCounterMessageItem must be independently verified since different handlers may pass different parameters or have different null-safe conditions.

**AC#27: WcCounterMessageSexTests includes 13 WithFirstSex character dispatch tests (C3)**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="WithFirstSex") gte 13`
- **Expected**: gte 13
- **Derivation**: WithFirstSex is called for 13 distinct characters (SEX.ERB:1492-1516); gte 13 requires 13 separate test methods each containing "WithFirstSex" in their name, one per character dispatch path. Previous `InlineData.*WithFirstSex` pattern was broken because xUnit [InlineData] attributes appear on separate lines from the test method name, making single-line Grep unable to match both on the same line.
- **Rationale**: C3 requires all 13 character dispatches are handled. The pattern counts "WithFirstSex" occurrences in the test file; 13 individual [Fact] test methods (rather than 1 [Theory] with 13 [InlineData]) provides clearer per-character failure reporting. Actual distinct-character-ID verification is guaranteed by runtime test execution (AC#39: all tests pass).

**AC#28: Tests verify KOJO dispatch via IKojoMessageService (C16)**
- **Test**: Count `Verify.*KojoMessage|KojoMessageWcCounter.*Times` patterns in WcCounterMessageSexTests.cs >= 4
- **Expected**: gte 4
- **Derivation**: There are 28 unique TRYCALLFORM dispatch patterns for KOJO across multiple handler groups (C16); gte 4 tests ensures at least 4 distinct KojoMessageWcCounter action type constants are verified with mock.Verify(), covering representative action groups (MESSAGE30s, MESSAGE40, MESSAGE50s, MESSAGE500s). Each match must be a mock.Verify() assertion call, not a setup or constructor reference
- **Rationale**: C16 requires verification of KOJO dispatch action type constant mappings from ERB patterns. Previous reference to C15 was incorrect (C15 is WC_応答分類 ResponseCategory, covered by AC#24).

**AC#29: BitfieldUtility usage (C8)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `BitfieldUtility`
- **Expected**: Pattern matches
- **Rationale**: Implementation must use BitfieldUtility for SETBIT/GETBIT operations (30+ operations in ERB).

**AC#30: No NotImplementedException stubs in SEX handler (Neg)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `NotImplementedException`
- **Expected**: Pattern not found
- **Rationale**: All 44 handler methods must be fully implemented. A NotImplementedException would indicate an incomplete migration stub left in production code.

**AC#31: No NotImplementedException stubs in IWcCounterMessageSex (Neg)**
- **Test**: `Grep(Era.Core/Counter/IWcCounterMessageSex.cs)` for `NotImplementedException`
- **Expected**: Pattern not found
- **Rationale**: The interface must have no default throw bodies. All method declarations should have no implementation body (interface members) or default-interface-method bodies that are non-throwing.

**AC#32: IEngineVariables declares GetTime method (C11)**
- **Test**: `Grep(Era.Core/Interfaces/IEngineVariables.cs)` for `GetTime`
- **Expected**: Pattern matches
- **Rationale**: C11 requires GetTime as a default interface method on IEngineVariables. Required for TIME read operations in TIME += migration points.

**AC#33: IEngineVariables declares SetTime method (C11)**
- **Test**: `Grep(Era.Core/Interfaces/IEngineVariables.cs)` for `SetTime`
- **Expected**: Pattern matches
- **Rationale**: C11 requires SetTime as a default interface method on IEngineVariables. Required for the 4 TIME += statements at ERB lines 1742, 2376, 2406, 3064.

**AC#34: WcCounterMessageSex calls SetTime at all 4 TIME += points (C11)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `\.SetTime\(`
- **Expected**: gte 4
- **Derivation**: 4 ERB TIME += statements (lines 1742, 2376, 2406, 3064) require SetTime writes. AC#34 count-gates the write side; AC#57 count-gates the read side (GetTime gte 4).
- **Rationale**: C11 requires WcCounterMessageSex to call SetTime at all 4 TIME += migration points. The gte 4 threshold mirrors AC#57 (GetTime gte 4) to enforce symmetric read-modify-write coverage.

**AC#35: No TODO/FIXME/HACK in WcCounterMessageSex (C10, Neg)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `TODO|FIXME|HACK`
- **Expected**: Pattern not found
- **Rationale**: C10 standard quality gate. No technical debt markers permitted in the implementation class.

**AC#36: No TODO/FIXME/HACK in IWcCounterMessageSex (C10, Neg)**
- **Test**: `Grep(Era.Core/Counter/IWcCounterMessageSex.cs)` for `TODO|FIXME|HACK`
- **Expected**: Pattern not found
- **Rationale**: C10 standard quality gate. No technical debt markers permitted in the interface file.

**AC#37: No TODO/FIXME/HACK in WcCounterMessageSexTests (C10, Neg)**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs)` for `TODO|FIXME|HACK`
- **Expected**: Pattern not found
- **Rationale**: C10 standard quality gate. No technical debt markers permitted in the test file.

**AC#38: C# solution builds without errors**
- **Test**: `dotnet build Era.Core/` with TreatWarningsAsErrors=true
- **Expected**: Build succeeds
- **Rationale**: All new files must compile without errors or warnings. TreatWarningsAsErrors=true is enforced by Directory.Build.props.

**AC#39: All unit tests pass**
- **Test**: `dotnet test Era.Core.Tests/ --blame-hang-timeout 10s`
- **Expected**: All tests pass
- **Rationale**: No regressions are permitted in the test suite. All existing and new tests must pass after the F806 implementation.

**AC#40: WcCounterMessageSex injects IKojoMessageService (C16)**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `IKojoMessageService`
- **Expected**: Pattern matches
- **Rationale**: 28 TRYCALLFORM dispatch patterns require IKojoMessageService for KojoMessageWcCounter calls. Constructor injection is required for testability.

**AC#41: WcCounterMessageSex injects IShrinkageSystem**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `IShrinkageSystem`
- **Expected**: Pattern matches
- **Rationale**: IShrinkageSystem is required for shrinkage-related state checks in SEX handler logic. Constructor injection enables mock substitution in tests.

**AC#42: WcCounterMessageSex injects ICommonFunctions**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `ICommonFunctions`
- **Expected**: Pattern matches
- **Rationale**: ICommonFunctions provides HasPenis/HasVagina queries used in 30+ branching locations (C14). Constructor injection enables mock substitution in tests.

**AC#43: VaFitting behavioral equivalence (C4)**
- **Test**: Count VaFitting return value assertions in test file >= 3
- **Expected**: gte 3
- **Derivation**: WC_VA_FITTING (ERB lines 3097-3174) is a pure function with deterministic outputs; at least 3 tests must verify that VaFitting returns the exact same values as the ERB function for known inputs (e.g., fitting levels 0, 2, 4)
- **Rationale**: Philosophy claims "no behavioral regression" — this AC verifies actual output correctness against ERB-derived expected values, not just mock call counts.

**AC#44: Tests cover MESSAGE30-36 handler output variants**
- **Test**: Count `Message3[0-6]|HandleMessage3` patterns in WcCounterMessageSexTests.cs >= 7
- **Expected**: gte 7
- **Derivation**: MESSAGE30-36 are 7 distinct SEX action handlers at ERB lines before the simple handler section; gte 7 tests ensures all 7 handlers in this group (MESSAGE30 through MESSAGE36) have behavioral correctness verification
- **Rationale**: C9 AC Design Constraint covers MESSAGE50-601 but not MESSAGE30-36. This AC fills the gap, ensuring the first 7 handlers in the SEX category also have test coverage per the Philosophy's "no behavioral regression" claim.

**AC#45: Dispatch() signature includes CharacterId offender parameter**
- **Test**: Grep WcCounterMessage.cs for `Dispatch.*int action.*CharacterId offender`
- **Expected**: Pattern matches
- **Rationale**: The Dispatch() signature change from `Dispatch(int action)` to `Dispatch(int action, CharacterId offender)` is required so that SEX handler delegation calls can pass offender to `_sex.HandleMessageN(offender)`. Without this signature change, offender is not in scope within Dispatch().

**AC#46: SendMessage() passes offender to Dispatch() call site**
- **Test**: Grep WcCounterMessage.cs for `Dispatch.*action.*offender`
- **Expected**: Pattern matches
- **Rationale**: The call site in SendMessage() must be updated from `Dispatch(action)` to `Dispatch(action, offender)` to thread the offender parameter through to the switch expression.

**AC#47: Tests cover MESSAGE41-47 handler variants**
- **Test**: Count `Message4[1-7]|HandleMessage4[1-7]` patterns in WcCounterMessageSexTests.cs >= 7
- **Expected**: gte 7
- **Derivation**: MESSAGE41-47 are 7 handlers covering forced orgasm (41-45 using ICharacterStateVariables GetEx) and sexual desire (46-47 with 26-branch location routing); gte 7 tests ensures all 7 handlers in this group (MESSAGE41 through MESSAGE47) have behavioral correctness verification
- **Rationale**: AC#44 covers MESSAGE30-36, AC#23 covers MESSAGE50-601, but MESSAGE41-47 had no equivalent handler-name test AC. This fills the gap.

**AC#48: Tests cover MESSAGE40 handler entry point**
- **Test**: Count `Message40|HandleMessage40` patterns in WcCounterMessageSexTests.cs >= 3
- **Expected**: gte 3
- **Derivation**: MESSAGE40 is the largest function (1,413 ERB lines) decomposed into 6 private sub-methods. The `Message4[1-7]` regex in AC#47 explicitly excludes index 40. gte 3 tests ensures the HandleMessage40 entry point is explicitly exercised with representative WC_応答分類 category/state combinations.
- **Rationale**: MESSAGE40 rated HIGH/HIGH risk. AC#24, AC#22, AC#25 cover sub-method behaviors but none verifies the entry point itself is tested.

**AC#49: Behavioral text output equivalence verification**
- **Test**: Count `Assert.Equal.*"|Assert.Contains.*"` patterns in WcCounterMessageSexTests.cs >= 18
- **Expected**: gte 18
- **Derivation**: Philosophy requires "no behavioral regression"; structural proxy AC#15 (gte 100 field usages) cannot distinguish correct from incorrect implementations. At least 18 tests must assert actual text output matches ERB-derived expected strings — 2+ per distinct handler group (9 groups): MESSAGE30-36, MESSAGE40, MESSAGE41-47, MESSAGE50-60, MESSAGE70-75, MESSAGE80-83, MESSAGE91, MESSAGE500/502-505, MESSAGE600-601. The matcher requires Assert.Equal/Assert.Contains with string literals (not mock call count proxies), ensuring value-level verification. **Expected string values in these assertions MUST be derived from TOILET_COUNTER_MESSAGE_SEX.ERB source, NOT from C# implementation output** (mirrors Task#9 ERB-derivation mandate).
- **Rationale**: Bridges structural completeness (AC#15) and behavioral correctness. Each handler group must have at least 2 tests that compare output text against ERB-derived expected strings, verifying that the C# migration produces identical visible output to the original ERB logic.

**AC#50: WcCounterMessageSex injects IVariableStore**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `IVariableStore`
- **Expected**: Pattern matches
- **Rationale**: IVariableStore provides DOWNBASE, STAIN, TALENT, BASE, CFLAG mutations used across multiple handlers. Constructor injection required for testability.

**AC#51: WcCounterMessageSex injects IEngineVariables**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `IEngineVariables`
- **Expected**: Pattern matches
- **Rationale**: IEngineVariables provides GetTime/SetTime (C11), GetTarget, GetDay and other engine state. Partially covered by AC#34 (SetTime calls) but explicit injection AC ensures constructor completeness.

**AC#52: WcCounterMessageSex injects ITEquipVariables**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `ITEquipVariables`
- **Expected**: Pattern matches
- **Rationale**: ITEquipVariables provides EQUIP/TEQUIP access for equipment state checks and mutations in SEX handlers.

**AC#53: WcCounterMessageSex injects ITextFormatting**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `ITextFormatting`
- **Expected**: Pattern matches
- **Rationale**: ITextFormatting provides text output formatting functions used across handler text generation.

**AC#54: WcCounterMessageSex injects IConsoleOutput**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `IConsoleOutput`
- **Expected**: Pattern matches
- **Rationale**: IConsoleOutput is the primary text output channel for all handler messages.

**AC#55: WcCounterMessageSex injects IRandomProvider**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `IRandomProvider`
- **Expected**: Pattern matches
- **Rationale**: IRandomProvider provides random number generation for probabilistic handler branches.

**AC#56: MESSAGE40 handler group decomposed into 6 named private sub-methods**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `private void.*ForcedOrgasm|private void.*SexualDesire|private void.*WithOther|private void.*HandleVirginity|private void.*HandlePhotography|private void.*HandleResponseCategory` gte 6
- **Expected**: gte 6
- **Derivation**: Technical Design specifies the MESSAGE40 handler group must be decomposed into 6 private sub-methods: ForcedOrgasm_CVA, SexualDesire, WithOther, HandleVirginity, HandlePhotography, HandleResponseCategory. SexualDesire is a shared private helper called by HandleMessage40 and HandleMessage46-47 (ERB source lines 2580-2835, outside MESSAGE40 range 68-1479). Goal item 7 ("MESSAGE40 decomposition into testable sub-methods") requires structural verification that these methods exist.
- **Rationale**: Private method existence verification ensures the MESSAGE40 handler group is properly decomposed. Each sub-method name is derived from the Technical Design's explicit enumeration. SexualDesire serves both MESSAGE40 (direct routing) and MESSAGE46-47 (26-branch location routing).
- **Note**: Pattern uses `private void` prefix to match method declarations only, excluding constant declarations like `private const int CflagWcForcedOrgasmCount`. Build gate AC#38 provides authoritative completeness check.

**AC#57: WcCounterMessageSex calls GetTime for TIME read-modify-write**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs)` for `\.GetTime\(\)` gte 4
- **Expected**: gte 4
- **Derivation**: 4 ERB TIME += statements at lines 1742, 2376, 2406, 3064 require read-modify-write semantics: `GetTime()` reads current value, delta is added, then `SetTime(result)` writes back. AC#34 (gte 4) verifies all 4 SetTime write calls; AC#57 (gte 4) verifies all 4 GetTime read calls.
- **Rationale**: Without GetTime verification, an implementor could call `SetTime(constant)` — an absolute assignment instead of an additive operation — and pass AC#34 while producing incorrect TIME values when TIME is already non-zero.

**AC#58: All 44 handler method declarations exist in WcCounterMessageSex.cs**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="HandleMessage(3[0-6]|4[0-7]|5[0-9]|60|7[0-5]|8[0-3]|91|500|502|503|504|505|600|601)\\b")` gte 44
- **Expected**: gte 44
- **Derivation**: The AC Definition Table enumerates 44 action-ID handlers across all groups (MESSAGE30-36=7, MESSAGE40-47=8, MESSAGE50-60=11, MESSAGE70-75=6, MESSAGE80-83=4, MESSAGE91=1, MESSAGE500+502-505=5, MESSAGE600-601=2; total=44). **Regex-to-group mapping**: `3[0-6]`→MESSAGE30-36 (7), `4[0-7]`→MESSAGE40-47 (8), `5[0-9]`→MESSAGE50-59 (10) + `60`→MESSAGE60 (1) = MESSAGE50-60 group (11), `7[0-5]`→MESSAGE70-75 (6), `8[0-3]`→MESSAGE80-83 (4), `91`→MESSAGE91 (1), `500|502|503|504|505`→MESSAGE500+502-505 (5), `600|601`→MESSAGE600-601 (2). The `\b` word boundary prevents `HandleMessage500` from also matching `HandleMessage50`. All 44 must appear as method declarations in the implementation file to satisfy C1 (all stubs replaced with non-zero handlers).
- **Rationale**: AC#7 verifies that WcCounterMessage.cs contains 44 delegation calls to `_sex.HandleMessageN`; AC#58 is the complementary implementation-side check that all 44 handler methods actually exist in WcCounterMessageSex.cs. Together they ensure no handler is delegated to but missing from the implementation. Added in Review iter8 to close the implementation-file verification gap noted for Tasks 6 and 7.

**AC#59: Tests verify CFLAG mutation calls (sexualDesire location routing)**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SetCharacterFlag|SetCflag|CFLAG|現在位置")` gte 2
- **Expected**: gte 2
- **Derivation**: The Risks table identifies CFLAG mutations (sexualDesire location routing, C7) as a MEDIUM likelihood / HIGH impact risk: "Carefully model CFLAG:現在位置 mutations with explicit SetCharacterFlag tests." The SexualDesire private sub-method (ERB lines 2580-2835) performs CFLAG writes for 現在位置 (current location) state. C7 documents 26 SELECTCASE branches; gte 2 tests ensures at least 2 distinct CFLAG mutation scenarios are mock-verified.
- **Rationale**: CFLAG mutations are state side effects distinct from text output. Without explicit mock.Verify() tests for SetCharacterFlag, an implementation that omits location state writes would pass all other ACs while silently corrupting game state. This AC was added in Review iter8 alongside AC#60 to address the MEDIUM/HIGH risk entry.

**AC#60: Tests verify TALENT mutation calls**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SetTalent|TALENT")` gte 2
- **Expected**: gte 2
- **Derivation**: The Risks table identifies TALENT/CFLAG direct writes as a MEDIUM likelihood / HIGH impact risk: "State mutation bugs (TALENT/CFLAG direct writes)." The Technical Constraints section notes "CFLAG, TALENT, BASE, STAIN, DOWNBASE, EQUIP, TCVAR, TEQUIP mutations" as non-pure side effects across the 44 handlers. TALENT writes appear in multiple handlers in TOILET_COUNTER_MESSAGE_SEX.ERB. Gte 2 ensures at least 2 distinct TALENT mutation calls are mock-verified.
- **Rationale**: TALENT mutations are character progression writes that affect long-term game state. An implementor that omits TALENT writes would produce silent data loss on character progression. AC#63 (implementation side) and AC#60 (test side) form the pair that guarantees TALENT mutations are both implemented and test-verified. Added in Review iter8.

**AC#61: Tests verify IConsoleOutput.PrintLine mock verification calls**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="Verify.*PrintLine|PrintLine.*Times")` gte 5
- **Expected**: gte 5
- **Derivation**: IConsoleOutput is the primary text output channel for all 44 handler messages (AC#54). AC#49 verifies string-equality assertions for ERB-derived output; AC#61 additionally requires 5+ explicit mock.Verify() calls on PrintLine to confirm that the correct output method is invoked. Five is the minimum count covering representative handler groups (MESSAGE30-36, MESSAGE40, MESSAGE41-47, MESSAGE50-60, and one other group), ensuring the delegation chain through IConsoleOutput is explicitly verified rather than only inferred.
- **Rationale**: AC#49 (Assert.Equal/Assert.Contains with string literals) focuses on output value correctness; AC#61 focuses on output method invocation correctness. An implementation that buffers output and flushes it differently could satisfy AC#49's string checks while never actually calling PrintLine on IConsoleOutput. AC#61 closes this gap by requiring mock.Verify() assertions. Added in Review iter8 to restore IConsoleOutput side-effect verification removed from AC#49 in iter4.

**AC#62: WcCounterMessageSex.cs calls SetCharacterFlag for CFLAG mutations**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="SetCharacterFlag|SetCflag")` gte 1
- **Expected**: gte 1
- **Derivation**: TOILET_COUNTER_MESSAGE_SEX.ERB performs CFLAG mutations including 現在位置 (current location) state writes as part of the sexualDesire routing logic (C7, ERB lines 2580-2835). The IVariableStore interface provides SetCharacterFlag for CFLAG writes. Gte 1 verifies the implementation actually calls this API at least once, confirming CFLAG mutations are not silently omitted.
- **Rationale**: AC#59 verifies the test side (mock.Verify calls for CFLAG); AC#62 is the implementation-side complement verifying SetCharacterFlag actually appears in WcCounterMessageSex.cs. Without this AC, a test using mock.Setup().Returns() but no actual implementation call could satisfy AC#59 while the production code omits the mutation. Added in Review iter9.

**AC#63: WcCounterMessageSex.cs calls SetTalent for TALENT mutations**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="SetTalent")` gte 1
- **Expected**: gte 1
- **Derivation**: TOILET_COUNTER_MESSAGE_SEX.ERB contains TALENT mutation writes across multiple handlers. The IVariableStore interface provides SetTalent for TALENT writes. Gte 1 verifies the implementation invokes this API at least once, confirming TALENT mutations are not silently omitted from the migration.
- **Rationale**: AC#60 verifies the test side (mock.Verify calls for TALENT); AC#63 is the implementation-side complement verifying SetTalent actually appears in WcCounterMessageSex.cs. The MEDIUM/HIGH risk entry in the Risks table ("State mutation bugs (TALENT/CFLAG direct writes)") requires both an implementation check and a test check. Added in Review iter9.

**AC#64: WcCounterMessageSex.cs calls WithPetting for SEX.ERB:2499**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="\.WithPetting")` gte 1
- **Expected**: gte 1
- **Derivation**: TOILET_COUNTER_MESSAGE_SEX.ERB:2499 contains a TRYCALL to WithPetting via IWcCounterMessageNtr (the same TRYCALL semantics pattern used for BottomClothOff/BottomClothOut via IWcCounterMessageItem at C2). The dependency table entry for F808 explicitly lists "SEX.ERB:2499 TRYCALL WithPetting" as one of the IWcCounterMessageNtr interaction points. Gte 1 confirms the implementation delegates this call rather than omitting it.
- **Rationale**: WithPetting is a cross-service delegation call (IWcCounterMessageNtr, injected via AC#13) that must be translated from ERB's TRYCALL WithPetting at line 2499. Without this AC, the single TRYCALL at line 2499 could be silently omitted from the migration and no other AC would detect its absence. AC#65 provides the corresponding test-side verification. Added in Review iter11.

**AC#65: Tests verify WithPetting mock call**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="WithPetting")` gte 1
- **Expected**: gte 1
- **Derivation**: SEX.ERB:2499 calls WithPetting via IWcCounterMessageNtr. Since WithPetting is a void side-effect call (no return value used in the ERB), mock.Verify() is the appropriate assertion mechanism. Gte 1 ensures at least one test explicitly verifies that WithPetting is called on the IWcCounterMessageNtr mock when the relevant handler executes.
- **Rationale**: AC#64 verifies the implementation contains the WithPetting call; AC#65 verifies there is a test asserting it. The pair mirrors the AC#62/AC#59 (CFLAG) and AC#63/AC#60 (TALENT) implementation+test pairs. Without a test, a future refactor could inadvertently delete the WithPetting call and only integration testing would catch it. Added in Review iter11.

**AC#66: WcCounterMessageSex.cs calls GetEx for MESSAGE43-45 EX counter checks**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="\.GetEx\(")` gte 3
- **Expected**: gte 3
- **Derivation**: C12 documents that MESSAGE43, MESSAGE44, and MESSAGE45 each check an EX counter (EX:MASTER:Ａ絶頂, Ｃ絶頂, Ｂ絶頂 respectively). ICharacterStateVariables provides GetEx (injected per AC#14). Each of the three MESSAGE43/44/45 handlers must call GetEx once to read its corresponding EX counter, yielding a minimum of 3 calls. Gte 3 directly mirrors the 3-handler count from the C12 constraint detail: "MESSAGE43/44/45 check EX counters (EX:MASTER:Ａ絶頂, Ｃ絶頂, Ｂ絶頂)."
- **Rationale**: C12 and the Technical Constraints table identify the EX variable access gap as a NEEDS_REVISION feasibility item requiring ICharacterStateVariables injection. AC#14 verifies injection; AC#66 verifies the injected interface is actually used for GetEx calls at the three migration points. Goal 5 ("Resolve IVariableStore EX interface gap") is satisfied by AC#14 (injection) + AC#66 (usage). Added in Review iter11.

**AC#67: Implementation calls SetBase/SetDownbase**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="SetBase\|SetDownbase")` gte 1
- **Expected**: gte 1
- **Derivation**: The Technical Constraints table lists "BASE, DOWNBASE" as mutation types among the non-pure state changes performed by the 44 handlers (CFLAG, TALENT, BASE, STAIN, DOWNBASE, EQUIP, TCVAR, TEQUIP mutations). AC#18 verifies test coverage for DOWNBASE (SetDownbase in test file); AC#67 is the implementation-side complement verifying SetBase or SetDownbase actually appears in WcCounterMessageSex.cs. Gte 1 is the minimum presence check following the AC#62/AC#63 pattern.
- **Rationale**: Without this AC, a test using mock.Setup().Returns() but no actual implementation call could satisfy AC#18 while the production code omits the BASE/DOWNBASE mutations entirely. AC#67 closes the implementation-side gap for BASE/DOWNBASE mutations to complete coverage of the 8 mutation types listed in the Technical Constraints section.

**AC#68: Implementation calls SetEquip/SetTEquip**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="SetEquip\|SetTEquip")` gte 1
- **Expected**: gte 1
- **Derivation**: The Technical Constraints table lists "EQUIP, TEQUIP" as mutation types among the non-pure state changes performed by the 44 handlers. AC#52 verifies ITEquipVariables is injected; AC#68 verifies the injected interface is actually used for SetEquip or SetTEquip calls at migration points. Gte 1 is the minimum presence check following the AC#62/AC#63 pattern.
- **Rationale**: AC#52 (injection) + AC#68 (usage) form the pair for EQUIP/TEQUIP mutations, analogous to AC#14 + AC#66 for EX counters. Without this AC, ITEquipVariables could be injected but never called, silently omitting all equipment state mutations from the migration. Added to complete implementation-side coverage of the 8 mutation types in the Technical Constraints section.

**AC#69: Implementation calls SetTCVar**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="SetTCVar")` gte 1
- **Expected**: gte 1
- **Derivation**: The Technical Constraints table lists "TCVAR" as a mutation type among the non-pure state changes performed by the 44 handlers (CFLAG, TALENT, BASE, STAIN, DOWNBASE, EQUIP, TCVAR, TEQUIP mutations). TCVAR mutations use IVariableStore.SetTCVar (same interface as SetTalent and SetCharacterFlag, injected per AC#50). Gte 1 is the minimum presence check confirming TCVAR mutations are not silently omitted.
- **Rationale**: AC#50 verifies IVariableStore injection; AC#69 verifies the TCVAR-specific mutation method is actually called. Without this AC, TCVAR mutations could be omitted with no other AC detecting the absence. Completes the implementation-side coverage for all 8 mutation types listed in the Technical Constraints section alongside AC#62 (CFLAG), AC#63 (TALENT), AC#67 (BASE/DOWNBASE), AC#68 (EQUIP/TEQUIP).

**AC#70: Simple handler methods declared in WcCounterMessageSex**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="HandleMessage(3[0-6]\|5[0-9]\|60\|7[0-5]\|8[0-3]\|91\|500\|502\|503\|504\|505\|600\|601)\\b")` gte 36
- **Expected**: gte 36
- **Derivation**: Task#5 implements the 36 simple handlers: MESSAGE30-36 (7 handlers) + MESSAGE50-60 (11 handlers) + MESSAGE70-75 (6 handlers) + MESSAGE80-83 (4 handlers) + MESSAGE91 (1 handler) + MESSAGE500+502-505 (5 handlers) + MESSAGE600-601 (2 handlers) = 36 total. AC#58 verifies all 44 handler method declarations using a pattern that includes MESSAGE40-47; AC#70 is the Task#5-scoped complement that verifies only the 36 simple handlers declared by Task#5, using a pattern that explicitly excludes MESSAGE40-47. This allows Task#5 completion to be verified independently of Tasks 6 and 7.
- **Rationale**: AC#58 (gte 44, all handlers) cannot be satisfied by Task#5 alone since Task#5 implements only 36 of 44 handlers. AC#70 provides the Task#5-specific verification gate: after Task#5, the 36 simple handler method declarations must exist in WcCounterMessageSex.cs. The pattern uses the same `\b` word boundary as AC#58 to prevent prefix-match false positives (e.g., HandleMessage500 matching HandleMessage50).

**AC#71: Implementation calls SetStain**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="SetStain")` gte 1
- **Expected**: gte 1
- **Derivation**: The Technical Constraints table lists "STAIN bitwise OR | 6 STAIN assignments using OR | Requires GetStain + SetStain with bitwise composition." Six STAIN assignments appear across multiple handlers in TOILET_COUNTER_MESSAGE_SEX.ERB. Gte 1 is the minimum presence check confirming SetStain is called at least once in the implementation, verifying the STAIN mutations are not silently omitted from the migration. The STAIN bitwise-OR semantics (GetStain + OR + SetStain) are covered by the existing AC#19 test-side verification.
- **Rationale**: AC#19 verifies the test side (SetStain|Stain patterns gte 2 in WcCounterMessageSexTests.cs); AC#71 is the implementation-side complement verifying SetStain actually appears in WcCounterMessageSex.cs. Without this AC, a test mock setup could satisfy AC#19 while the production code silently omits all STAIN mutations. AC#71 completes the implementation+test pair for STAIN mutations, mirroring the AC#62/AC#59 (CFLAG), AC#63/AC#60 (TALENT), and AC#67/AC#18 (BASE/DOWNBASE) patterns. Assigned to Task#6 because MESSAGE40 contains STAIN mutation calls within the message handling scope.

**AC#72: Tests verify GetEx counter access for MESSAGE43-45**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="GetEx\|絶頂")` gte 3
- **Expected**: gte 3
- **Derivation**: C12 documents that MESSAGE43, MESSAGE44, and MESSAGE45 each check an EX counter (EX:MASTER:Ａ絶頂, Ｃ絶頂, Ｂ絶頂 respectively). AC#66 verifies the implementation calls GetEx gte 3 times; AC#72 is the test-side complement verifying that WcCounterMessageSexTests.cs contains at least 3 test references to GetEx or 絶頂 — one per MESSAGE handler. The pattern covers the method name ("GetEx") and the Japanese counter identifier ("絶頂") to allow test methods named by either convention. The bare `EX` token was removed to prevent false-positive matches against common C# keywords like "Exception".
- **Rationale**: AC#66 (implementation side) verifies that GetEx is called 3 times in the production code; AC#72 (test side) verifies that the corresponding test coverage exists. Without AC#72, the MESSAGE43-45 EX counter checks could be implemented but untested — only the all-tests-pass AC#39 would catch regressions at runtime. AC#72 makes the test-side coverage explicit, following the AC#14+AC#66 injection+usage pair with an additional test-side verification. Assigned to Task#9 alongside AC#17 (virginity) and AC#20 (TIME) as the other interface-gap test verification ACs in that task.

**AC#73: Implementation calls GetStain for bitwise OR read-modify-write**
- **Test**: `Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern="\.GetStain\(")` gte 1
- **Expected**: gte 1
- **Derivation**: The Technical Constraints table states "STAIN bitwise OR | 6 STAIN assignments using OR | Requires GetStain + SetStain with bitwise composition." The bitwise OR read-modify-write pattern requires first reading the current STAIN value via GetStain, ORing in the new bits, then writing back via SetStain. AC#71 verifies the write side (SetStain gte 1); AC#73 is the read-side complement verifying GetStain is actually called — ensuring the OR composition is implemented (not just an overwrite). Gte 1 is the minimum presence check for the read side of the pattern.
- **Rationale**: Without AC#73, the implementation could call SetStain with a constant value (no bitwise OR), silently discarding existing STAIN bits while AC#71 passes. AC#73 closes this gap by requiring the GetStain read that precedes the OR composition, completing the full read-modify-write pair. Assigned to Task#6 alongside AC#71 because MESSAGE40 contains the STAIN mutations within the decomposed sub-methods. Part of the STAIN coverage triple: AC#19 (test-side SetStain|Stain gte 2), AC#71 (impl SetStain gte 1), AC#73 (impl GetStain gte 1).

**AC#74: Tests verify SetTCVar mutation calls**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SetTCVar\|TCVAR")` gte 1
- **Expected**: gte 1
- **Derivation**: The Technical Constraints table lists "TCVAR" as a mutation type among the non-pure state changes performed by the 44 handlers. AC#69 verifies the implementation calls SetTCVar (gte 1 in WcCounterMessageSex.cs); AC#74 is the test-side complement verifying that WcCounterMessageSexTests.cs contains at least 1 test reference to SetTCVar or TCVAR. Gte 1 is the minimum presence check confirming TCVAR mutation test coverage exists. The pattern covers both the method name ("SetTCVar") and the variable name ("TCVAR") to allow test methods named by ERB variable name.
- **Rationale**: AC#69 verifies the implementation-side TCVAR mutation; AC#74 adds the test-side complement to form a complete implementation+test pair, following the same pattern as AC#62/AC#59 (CFLAG), AC#63/AC#60 (TALENT), AC#67/AC#18 (DOWNBASE), AC#68/AC#75 (EQUIP/TEQUIP), and AC#71/AC#19 (STAIN). Without AC#74, TCVAR mutations could be implemented but untested — only AC#39 (all tests pass) would catch regressions. Assigned to Task#11 alongside AC#59/AC#60/AC#61 as the state mutation test verification tasks.

**AC#75: Tests verify SetEquip/SetTEquip mutation calls**
- **Test**: `Grep(Era.Core.Tests/Counter/WcCounterMessageSexTests.cs, pattern="SetEquip\|SetTEquip")` gte 1
- **Expected**: gte 1
- **Derivation**: The Technical Constraints table lists "EQUIP, TEQUIP" as mutation types among the non-pure state changes performed by the 44 handlers. AC#68 verifies the implementation calls SetEquip or SetTEquip (gte 1 in WcCounterMessageSex.cs); AC#75 is the test-side complement verifying that WcCounterMessageSexTests.cs contains at least 1 test reference to SetEquip or SetTEquip. Gte 1 is the minimum presence check confirming EQUIP/TEQUIP mutation test coverage exists. The pattern mirrors the AC#68 implementation pattern exactly.
- **Rationale**: AC#52 verifies ITEquipVariables injection; AC#68 verifies the implementation calls SetEquip/SetTEquip; AC#75 adds the test-side complement to form the complete injection+implementation+test triple, following the same three-layer pattern established for other mutation types. Without AC#75, EQUIP/TEQUIP mutations could be implemented but untested — only AC#39 (all tests pass) would catch regressions. Assigned to Task#11 alongside AC#74, AC#59, and AC#60 as the state mutation test verification tasks.

**AC#76: Test assertions include co-located ERB source annotations**
- **Test**: Count `Assert\.(?:Equal|Contains).*// ERB:` patterns in WcCounterMessageSexTests.cs >= 18
- **Expected**: gte 18
- **Derivation**: AC#49 requires 18+ string-equality assertions (Assert.Equal/Assert.Contains) with ERB-derived expected values, but the AC#49 Grep matcher cannot verify provenance. This traceability AC requires each such assertion to include a co-located `// ERB:{line_number}` comment annotation on the same line, identifying the source ERB line. The co-location pattern (`Assert.Equal(...) // ERB:1234`) ensures annotations are bound to assertions, not placed on arbitrary lines.
- **Rationale**: Prevents circular verification where test authors copy expected strings from C# implementation output rather than ERB source. Co-location enforcement (same-line pattern match) closes the gap where independent Grep counts could be satisfied by placing `// ERB:` comments on non-assertion lines. Each annotation creates an auditable link back to TOILET_COUNTER_MESSAGE_SEX.ERB.

**AC#77: Tests reference MESSAGE40 private sub-method names (4+ of 6)**
- **Test**: Count `ForcedOrgasm|SexualDesire|WithOther|HandleVirginity|HandlePhotography|HandleResponseCategory` patterns in WcCounterMessageSexTests.cs >= 4
- **Expected**: gte 4
- **Derivation**: AC#56 verifies 6 private sub-methods exist structurally in the implementation. AC#48 verifies the MESSAGE40 entry point HandleMessage40 is tested 3+ times. Neither AC verifies that sub-method decomposition is individually exercised in tests. This AC requires 4+ of the 6 sub-method names appear in test method names or assertions, ensuring the decomposition provides testable boundaries (not just structural code organization). Threshold 4 (of 6) allows minor sub-methods to be tested indirectly via entry point while ensuring the majority are directly referenced.
- **Rationale**: Goal 7 claims "MESSAGE40 decomposition into testable sub-methods" — "testable" implies tests exercise sub-methods by name. Without this AC, the sub-method decomposition could exist structurally (AC#56) but never be individually targeted by tests, making the "testable" claim unverifiable.

**AC#78: Tests verify WcCounterMessage delegates to IWcCounterMessageSex with correct offender**
- **Test**: Count `HandleMessage.*offender|_sex.*Verify.*offender|WcCounterMessage.*Dispatch.*offender` patterns in Era.Core.Tests/ >= 1
- **Expected**: gte 1
- **Derivation**: Task#4 modifies WcCounterMessage.cs to inject IWcCounterMessageSex and replace 44 stubs with `=> _sex.HandleMessageN(offender)` delegation calls. AC#7 (gte 44 delegation patterns) and AC#45/AC#46 (Dispatch signature, SendMessage call-site) are static Grep checks on production code. None verify that the `offender` CharacterId is actually passed through at runtime. This AC requires at least 1 test-side reference verifying the delegation includes the offender parameter, closing the static-only verification gap for the delegation layer.
- **Rationale**: Without runtime/test-side verification, an implementation passing `CharacterId.Default` instead of the actual offender would satisfy all static ACs (AC#7/AC#45/AC#46) while silently breaking offender-dependent handler behavior.

**AC#79: No TODO/FIXME/HACK in modified IEngineVariables.cs**
- **Test**: Grep(Era.Core/Interfaces/IEngineVariables.cs) for TODO|FIXME|HACK
- **Expected**: not_matches
- **Derivation**: F806 Task#1 adds GetTime/SetTime to IEngineVariables. The existing TODO/FIXME/HACK gate (AC#35-37) only covers the 3 new SEX-specific files. This extends the gate to the modified shared interface file.
- **Rationale**: Prevents technical debt markers from being introduced into shared interface files during F806 modifications.

**AC#80: No TODO/FIXME/HACK in modified WcCounterMessage.cs**
- **Test**: Grep(Era.Core/Counter/WcCounterMessage.cs) for TODO|FIXME|HACK
- **Expected**: not_matches
- **Derivation**: F806 Task#4 replaces 44 stubs and modifies Dispatch/SendMessage signatures. This extends the gate to the modified shared dispatch file.
- **Rationale**: Prevents technical debt markers from being introduced during the 44-stub replacement in the shared dispatch file.

**AC#81: WcCounterMessageSex injects IItemVariables**
- **Test**: Grep(Era.Core/Counter/WcCounterMessageSex.cs) for `IItemVariables`
- **Expected**: Pattern matches
- **Derivation**: IItemVariables is a direct constructor dependency used for item-related checks across multiple handlers.
- **Rationale**: Ensures IItemVariables is constructor-injected for testability via Moq.

**AC#82: WcCounterMessageSex injects ICounterUtilities**
- **Test**: Grep(Era.Core/Counter/WcCounterMessageSex.cs) for `ICounterUtilities`
- **Expected**: Pattern matches
- **Derivation**: ICounterUtilities is a direct constructor dependency used for counter utility operations across multiple handlers.
- **Rationale**: Ensures ICounterUtilities is constructor-injected for testability via Moq.

**AC#83: BottomClothOff/BottomClothOut invocations present for all 6 C2 TRYCALL sites**
- **Test**: Grep(Era.Core/Counter/WcCounterMessageSex.cs, pattern=`\.BottomClothOff|\.BottomClothOut`) gte 6
- **Expected**: At least 6 direct invocations (matching all 6 ERB TRYCALL call sites)
- **Derivation**: C2 constraint requires TRYCALL semantics. ERB TRYCALL invokes a function only if it exists at runtime. In C# with DI, IWcCounterMessageItem is always registered as a non-nullable singleton (Nullable=enable + TreatWarningsAsErrors makes `?.` on non-nullable DI interface a build error CS8073). DI registration guarantees non-null, so direct invocation (`.BottomClothOff()`) is semantically equivalent to ERB TRYCALL when the handler is always present. The 6 call sites (ERB lines 407, 1540, 1815, 1903, 2037, 2578) must all appear as direct invocations.
- **Rationale**: Ensures all 6 ERB TRYCALL call sites are migrated. TRYCALL semantics are satisfied structurally by DI's non-nullable registration guarantee rather than runtime null-conditional checks.
- **Note**: Originally specified as null-conditional (`?.`) invocation; changed to direct (`.`) invocation per Phase7-Debug iter1 fix (CS8073 build error under Nullable=enable).

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Implement all 44 SEX-category action ID handlers in WcCounterMessageSex | AC#1, AC#2, AC#7, AC#8, AC#10, AC#11, AC#12, AC#13, AC#15, AC#30, AC#40, AC#41, AC#42, AC#44, AC#47, AC#58, AC#64, AC#67, AC#68, AC#69, AC#70, AC#83 |
| 2 | Follow F808 extraction pattern (interface + implementation + DI) | AC#1, AC#2, AC#3, AC#4, AC#6, AC#50, AC#51, AC#52, AC#53, AC#54, AC#55, AC#81, AC#82 |
| 3 | Replace all stubs in WcCounterMessage.Dispatch() with delegation | AC#6, AC#7, AC#8, AC#45, AC#46, AC#78 |
| 4 | Resolve IEngineVariables TIME interface gap | AC#32, AC#33, AC#34, AC#57 |
| 5 | Resolve IVariableStore EX interface gap (inject ICharacterStateVariables) | AC#14, AC#66, AC#72 |
| 6 | Behavioral equivalence through comprehensive testing | AC#9, AC#16, AC#17, AC#18, AC#19, AC#20, AC#21, AC#22, AC#23, AC#24, AC#25, AC#26, AC#27, AC#28, AC#43, AC#44, AC#47, AC#48, AC#49, AC#59, AC#60, AC#61, AC#62, AC#63, AC#64, AC#65, AC#66, AC#67, AC#68, AC#69, AC#71, AC#72, AC#73, AC#74, AC#75, AC#76, AC#83 |
| 7 | MESSAGE40 decomposition into testable sub-methods | AC#15, AC#22, AC#24, AC#25, AC#29, AC#34, AC#48, AC#56, AC#57, AC#77 |
| 8 | WC_VA_FITTING as public pure function | AC#5, AC#16, AC#43 |
| 9 | Code quality gates (no stubs, no debt markers, build and tests pass) | AC#30, AC#31, AC#35, AC#36, AC#37, AC#38, AC#39, AC#79, AC#80 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

WcCounterMessageSex follows the F808 extraction pattern: a single `sealed class WcCounterMessageSex` implementing a dedicated `IWcCounterMessageSex` interface, registered with the DI container alongside WcCounterMessageItem and WcCounterMessageNtr. WcCounterMessage receives `IWcCounterMessageSex` via constructor injection and delegates all 44 SEX action IDs to it, replacing every `=> 0` stub in `Dispatch()`.

The class is organized into three layers:
1. **Public dispatch methods** -- one per action ID group (e.g., `HandleMessage30()`, `HandleMessage40()`), called by WcCounterMessage via the field reference.
2. **Private sub-methods** -- decomposing the MESSAGE40 handler group and MESSAGE41-47 using 6+ shared private helpers: `ForcedOrgasm_CVA()`, `SexualDesire()`, `WithOther()`, `HandleVirginity()`, `HandlePhotography()`, `HandleResponseCategory()`, and optional state-mutation helpers. SexualDesire is a shared helper called by HandleMessage40 and HandleMessage46-47 (ERB source lines 2580-2835, outside MESSAGE40 range 68-1479). AC#56 enforces the 6 required methods; additional helpers are permitted. This makes each sub-unit independently unit-testable.
3. **Pure function** -- `VaFitting(int size, int depth)` returns a fitting level 0-4 with no side effects, exposed publicly on the interface (C13).

IEngineVariables is extended with `GetTime()` and `SetTime(int value)` as default interface methods (body: `=> 0` / empty), backward-compatible with all existing implementors. ICharacterStateVariables already provides `GetEx`/`SetEx` and is injected directly (ISP-preferred over extending IVariableStore).

KOJO dispatch uses the existing `IKojoMessageService.KojoMessageWcCounter(int characterIndex, int actionType)` overload (already declared as a default method in F811). Action type integer constants are declared as private `const int KojoAction*` fields in WcCounterMessageSex following the same pattern used in WcCounterMessage.

State mutations (DOWNBASE additive, STAIN bitwise OR, EQUIP, CFLAG/TALENT writes) are performed through the existing IVariableStore, IEngineVariables, and ITEquipVariables APIs. BitfieldUtility handles all SETBIT/GETBIT operations for WC_同席者, WC_あなたエロ写真, and WC_あなた処女喪失写真 flag fields.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `Era.Core/Counter/IWcCounterMessageSex.cs` |
| 2 | Create `Era.Core/Counter/WcCounterMessageSex.cs` declaring `public sealed class WcCounterMessageSex : IWcCounterMessageSex` |
| 3 | Create `Era.Core.Tests/Counter/WcCounterMessageSexTests.cs` |
| 4 | Add `services.AddSingleton<IWcCounterMessageSex, WcCounterMessageSex>()` to ServiceCollectionExtensions.cs |
| 5 | Declare `int VaFitting(int inserter, int target, int holeType)` on IWcCounterMessageSex |
| 6 | Add `IWcCounterMessageSex` constructor parameter to WcCounterMessage; store as `_sex` field |
| 7 | Replace all 44 `=> 0` stubs with `=> _sex.HandleMessageN()` calls in WcCounterMessage.Dispatch() |
| 8 | After stub replacement, no `N => 0` lines remain for F806 action IDs in WcCounterMessage.cs |
| 9 | WcCounterMessageSexTests includes `Assert.*` and `mock.Verify(...)` calls |
| 10 | WcCounterMessageSex constructor receives `IVirginityManager virginityManager` |
| 11 | WcCounterMessageSex constructor receives `ILocationService locationService` |
| 12 | WcCounterMessageSex constructor receives `IWcCounterMessageItem item` |
| 13 | WcCounterMessageSex constructor receives `IWcCounterMessageNtr ntr` |
| 14 | WcCounterMessageSex constructor receives `ICharacterStateVariables characterState` |
| 15 | Full ERB migration produces 100+ field usages across all handler and sub-method bodies |
| 16 | WcCounterMessageSexTests includes 5+ test methods exercising VaFitting with different (inserter, target, holeType) inputs |
| 17 | WcCounterMessageSexTests includes 3+ tests verifying CheckLostVirginity with VirginityType.Normal, VirginityType.Male, VirginityType.Anal |
| 18 | WcCounterMessageSexTests includes 2+ tests verifying SetBase/SetDownBase calls with expected delta values |
| 19 | WcCounterMessageSexTests includes 2+ tests verifying SetStain calls with bitwise OR semantics |
| 20 | WcCounterMessageSexTests includes 2+ tests verifying SetTime calls with ERB-derived delta values for TIME mutation (expected deltas MUST be derived from TOILET_COUNTER_MESSAGE_SEX.ERB lines 1742, 2376, 2406, 3064, NOT from implementation output) |
| 21 | WcCounterMessageSexTests includes 10+ tests covering distinct SexualDesire location branches (C7: 26 SELECTCASE branches) |
| 22 | WcCounterMessageSexTests includes 2+ tests verifying BitfieldUtility.SetBit for photo flag fields |
| 23 | WcCounterMessageSexTests includes 15+ test methods naming Message50-601 handler variants |
| 24 | WcCounterMessageSexTests includes 3+ tests parameterised by WC_応答分類 (ResponseCategory) values |
| 25 | WcCounterMessageSexTests includes 2+ tests covering HasPenis=true and HasVagina=true branches |
| 26 | WcCounterMessageSexTests includes 6+ tests verifying BottomClothOff/BottomClothOut mock calls (2 BottomClothOff at ERB:407,2578 + 4 BottomClothOut at ERB:1540,1815,1903,2037) |
| 27 | WcCounterMessageSexTests includes 13 individual [Fact] WithFirstSex test methods, one per character dispatch path |
| 28 | WcCounterMessageSexTests includes 4+ tests verifying KojoMessageWcCounter mock calls with distinct action type constants |
| 29 | WcCounterMessageSex.cs contains `BitfieldUtility` usage for SETBIT/GETBIT operations |
| 30 | No `NotImplementedException` in WcCounterMessageSex.cs (all handlers fully implemented) |
| 31 | No `NotImplementedException` in IWcCounterMessageSex.cs (interface has no default throw bodies) |
| 32 | Add `int GetTime()` as default interface method on IEngineVariables with `=> 0` body |
| 33 | Add `void SetTime(int value)` as default interface method on IEngineVariables with empty body |
| 34 | WcCounterMessageSex calls `_engine.SetTime(...)` at the 4 TIME += migration points |
| 35 | WcCounterMessageSex.cs contains no TODO/FIXME/HACK comments |
| 36 | IWcCounterMessageSex.cs contains no TODO/FIXME/HACK comments |
| 37 | WcCounterMessageSexTests.cs contains no TODO/FIXME/HACK comments |
| 38 | `dotnet build Era.Core/` succeeds with TreatWarningsAsErrors=true |
| 39 | `dotnet test Era.Core.Tests/ --blame-hang-timeout 10s` passes all tests |
| 40 | WcCounterMessageSex constructor receives `IKojoMessageService kojoMessage` |
| 41 | WcCounterMessageSex constructor receives `IShrinkageSystem shrinkageSystem` |
| 42 | WcCounterMessageSex constructor receives `ICommonFunctions commonFunctions` |
| 43 | VaFitting returns ERB-equivalent values for known input/output pairs |
| 44 | WcCounterMessageSexTests includes 7+ tests covering MESSAGE30-36 handler output variants |
| 45 | Dispatch() signature includes CharacterId offender parameter |
| 46 | SendMessage() passes offender to Dispatch() call site |
| 47 | WcCounterMessageSexTests includes 7+ tests covering MESSAGE41-47 handler variants |
| 48 | WcCounterMessageSexTests includes 3+ tests covering MESSAGE40 handler entry point |
| 49 | Write 18+ string-equality assertions verifying handler text output (2+ per handler group) |
| 50 | Inject IVariableStore in WcCounterMessageSex constructor |
| 51 | Inject IEngineVariables in WcCounterMessageSex constructor |
| 52 | Inject ITEquipVariables in WcCounterMessageSex constructor |
| 53 | Inject ITextFormatting in WcCounterMessageSex constructor |
| 54 | Inject IConsoleOutput in WcCounterMessageSex constructor |
| 55 | Inject IRandomProvider in WcCounterMessageSex constructor |
| 56 | Implement MESSAGE40 as 6 named private sub-methods |
| 57 | Call GetTime() at 4 TIME += read-modify-write migration points |
| 58 | Verify all 44 handler method declarations exist in implementation file |
| 59 | Verify CFLAG mutation test coverage (gte 2) |
| 60 | Verify TALENT mutation test coverage (gte 2) |
| 61 | Verify IConsoleOutput.PrintLine mock verification calls (gte 5) |
| 62 | Verify SetCharacterFlag calls in implementation for CFLAG mutations |
| 63 | Verify SetTalent calls in implementation for TALENT mutations |
| 64 | Verify WithPetting call in implementation (SEX.ERB:2499) |
| 65 | Verify WithPetting test coverage |
| 66 | Verify GetEx calls in implementation for MESSAGE43-45 EX counters |
| 67 | Call SetBase or SetDownbase at BASE/DOWNBASE mutation points in WcCounterMessageSex.cs |
| 68 | Call SetEquip or SetTEquip at EQUIP/TEQUIP mutation points in WcCounterMessageSex.cs |
| 69 | Call SetTCVar at TCVAR mutation points in WcCounterMessageSex.cs |
| 70 | Declare all 36 simple handler methods (MESSAGE30-36 + MESSAGE50-601) in WcCounterMessageSex.cs |
| 71 | Call SetStain at STAIN mutation points in WcCounterMessageSex.cs |
| 72 | WcCounterMessageSexTests includes 3+ tests verifying GetEx access for MESSAGE43-45 EX counters |
| 73 | Call GetStain at STAIN read-side points in WcCounterMessageSex.cs (bitwise OR read-modify-write: GetStain + OR + SetStain) |
| 74 | WcCounterMessageSexTests includes 1+ test verifying SetTCVar mutation call (mock.Verify) |
| 75 | WcCounterMessageSexTests includes 1+ test verifying SetEquip or SetTEquip mutation call (mock.Verify) |
| 76 | WcCounterMessageSexTests includes 18+ co-located ERB source annotations (Assert.Equal/Contains + // ERB:{line} on same line) for traceability |
| 77 | WcCounterMessageSexTests references 4+ of 6 MESSAGE40 private sub-method names (ForcedOrgasm, SexualDesire, WithOther, HandleVirginity, HandlePhotography, HandleResponseCategory) in test method names or assertions, verifying sub-method decomposition is individually tested |
| 78 | Tests verify WcCounterMessage delegates to IWcCounterMessageSex with correct offender CharacterId (runtime/mock verification, not static grep) |
| 79 | No TODO/FIXME/HACK in IEngineVariables.cs (Neg) |
| 80 | No TODO/FIXME/HACK in WcCounterMessage.cs (Neg) |
| 81 | IItemVariables injection presence in WcCounterMessageSex.cs |
| 82 | ICounterUtilities injection presence in WcCounterMessageSex.cs |
| 83 | BottomClothOff/BottomClothOut direct invocations present for all 6 C2 TRYCALL sites (DI guarantees non-null) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Class structure | Single class vs split into sub-classes per action group | Single `WcCounterMessageSex` class | F808 used a single class per ERB file; splitting would add DI complexity without behavioral benefit. Private sub-methods provide decomposition within the class. |
| MESSAGE40 handler group decomposition | Inline translation vs private sub-methods | 6+ private sub-methods (AC#56 verifies `ForcedOrgasm_CVA`, `SexualDesire`, `WithOther`, `HandleVirginity`, `HandlePhotography`, `HandleResponseCategory`); SexualDesire shared by HandleMessage40 and HandleMessage46-47 | Enables isolated unit testing of each logical section; prevents a single method exceeding maintainability limits. |
| DI strategy for WcCounterMessage | Add 7+ new constructor params vs inject single IWcCounterMessageSex | Inject `IWcCounterMessageSex` as one new param | WcCounterMessage constructor is already at 11 params (post-F807). Delegating to IWcCounterMessageSex keeps WcCounterMessage's surface stable (C Constraint: constructor explosion avoidance). |
| IEngineVariables TIME extension | Extend interface with default methods vs add new interface ITimeVariables | Default interface methods (`GetTime() => 0`, `SetTime(int value) {}`) | Default interface methods are backward-compatible; no existing implementors require changes. A new interface would add DI graph entries with no benefit since TIME is a scalar engine variable like DAY/MONEY already on IEngineVariables. |
| EX variable access | Extend IVariableStore with GetEx/SetEx vs inject ICharacterStateVariables | Inject `ICharacterStateVariables` (already has GetEx/SetEx) | ISP-preferred: ICharacterStateVariables already segregates character state tracking. Extending IVariableStore would violate ISP by adding EX methods where only MESSAGE43-45 need them. |
| VaFitting exposure | Private helper vs public interface method | Public method on `IWcCounterMessageSex` | 9 kojo ERB files call WC_VA_FITTING externally (C13). Must be accessible via the injected interface. |
| KOJO dispatch | New 3-param overload vs existing 2-param overload with constants | Use existing `KojoMessageWcCounter(int characterIndex, int actionType)` default overload already declared in IKojoMessageService | The overload already exists (F811). Action type constants follow the WcCounterMessage pattern (KojoActionMessage* consts). |
| Internal helpers | ForcedOrgasm_CVA/SexualDesire/WithOther as public vs private | Private methods | These are implementation details of MESSAGE40 decomposition; only the public handler entry points (one per action ID) need interface exposure. |
| Dispatch offender convention | Explicit parameter vs implicit GetTarget() | Explicit parameter for F806 SEX handlers; implicit GetTarget() retained for ITEM/NTR | SEX handlers need offender for WithFirstSex character dispatch (C3); existing ITEM/NTR handlers pre-date the parameter and function correctly with GetTarget(). F807 [DONE] chose implicit GetTarget() for TEASE. Full convention unification deferred to F813. |

### Interfaces / Data Structures

**IWcCounterMessageSex** (new file: `Era.Core/Counter/IWcCounterMessageSex.cs`):

```csharp
public interface IWcCounterMessageSex
{
    // Public pure function (C13: cross-file callable)
    int VaFitting(int inserter, int target, int holeType);

    // Per-action handler methods (one per dispatch action ID group)
    int HandleMessage30(CharacterId offender);
    int HandleMessage31(CharacterId offender);
    int HandleMessage32(CharacterId offender);
    int HandleMessage33(CharacterId offender);
    int HandleMessage34(CharacterId offender);
    int HandleMessage35(CharacterId offender);
    int HandleMessage36(CharacterId offender);
    int HandleMessage40(CharacterId offender);
    int HandleMessage41(CharacterId offender);
    int HandleMessage42(CharacterId offender);
    int HandleMessage43(CharacterId offender);
    int HandleMessage44(CharacterId offender);
    int HandleMessage45(CharacterId offender);
    int HandleMessage46(CharacterId offender);
    int HandleMessage47(CharacterId offender);
    int HandleMessage50(CharacterId offender);
    // ... 51-60, 70-75, 80-83, 91, 500, 502-505, 600, 601
}
```

**IEngineVariables additions** (default interface methods, backward-compatible):

```csharp
/// <summary>Get TIME value (elapsed time counter)</summary>
/// Feature 806 - Required for TIME += operations in TOILET_COUNTER_MESSAGE_SEX.ERB
int GetTime() => 0;

/// <summary>Set TIME value (elapsed time counter)</summary>
/// Feature 806 - Required for TIME += operations in TOILET_COUNTER_MESSAGE_SEX.ERB
void SetTime(int value) { }
```

**DIM Backward Compatibility Note**: Default bodies (`=> 0` / `{ }`) follow C# default interface method (DIM) conventions for backward-compatible interface evolution. The real IEngineVariables implementor in the engine repo overrides both methods with game-state-backed TIME access. Test mocks override as needed via Moq `.Setup()`. The `=> 0` default ensures existing implementors compile without changes; behavioral correctness is enforced by mock configuration in test scenarios (AC#20, AC#34, AC#57). NuGet version bump tracking is in Mandatory Handoffs row 1.

**WcCounterMessage changes** (constructor + Dispatch):
- Add `IWcCounterMessageSex sex` constructor parameter, stored as `_sex`
- Change `Dispatch(int action)` signature to `Dispatch(int action, CharacterId offender)` and update call site in `SendMessage()` from `Dispatch(action)` to `Dispatch(action, offender)`
- Replace 44 `=> 0` stubs with `=> _sex.HandleMessageN(offender)` calls (offender now in scope via updated Dispatch signature)

**WcCounterMessageSex constructor** (16 dependencies):

**Responsibility Justification**: All 16 dependencies are required by the source ERB file (TOILET_COUNTER_MESSAGE_SEX.ERB calls 16 distinct external service APIs across its 49 functions). Unlike typical SRP violations where dependencies indicate conflated responsibilities, this dependency count reflects the ERB source's inherent cross-system nature — the counter message system produces text output that depends on virginity state, location, equipment, character flags, shrinkage, items, NTR events, and KOJO dialogue simultaneously. Splitting into sub-classes before implementation would be premature (coupling patterns within the 3,518 lines are unknown until implemented). Post-implementation responsibility review is tracked in Mandatory Handoffs (F813: "WcCounterMessageSex responsibility review (16 deps)").

```
IVariableStore, IEngineVariables, ITEquipVariables, ITextFormatting,
IConsoleOutput, IKojoMessageService, IRandomProvider,
IVirginityManager, ILocationService, IWcCounterMessageItem,
IWcCounterMessageNtr, ICharacterStateVariables, IShrinkageSystem, ICommonFunctions,
IItemVariables, ICounterUtilities
```

**Key private sub-methods in WcCounterMessageSex**:

| Method | ERB Source | Purpose |
|--------|-----------|---------|
| `ForcedOrgasm_CVA(CharacterId)` | MESSAGE40 subsection | CVA orgasm forced-output sequence |
| `SexualDesire(CharacterId)` | ERB lines 2580-2835 | 26-branch location/character routing |
| `WithOther(CharacterId)` | ERB lines ~1800-2050 | Cross-character interaction handler |
| `HandleVirginity(CharacterId, VirginityType)` | ERB lines 1079, 1468, 3087 | IVirginityManager.CheckLostVirginity dispatch |
| `HandlePhotography(CharacterId)` | MESSAGE40 SETBIT section | Sets WC_あなたエロ写真 / WC_あなた処女喪失写真 bits |
| `HandleResponseCategory(CharacterId, int)` | ERB lines 68-1479 (8 categories) | WC_応答分類 switch dispatch |

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| IEngineVariables lacks GetTime/SetTime | `Era.Core/Interfaces/IEngineVariables.cs` | Add `int GetTime() => 0;` and `void SetTime(int value) {}` as default interface methods (backward-compatible). No existing implementors require changes. |
| WcCounterMessage must inject IWcCounterMessageSex | `Era.Core/Counter/WcCounterMessage.cs` | Add `IWcCounterMessageSex sex` constructor parameter; store as `_sex`; replace 44 stub arms in Dispatch() switch expression |
| Dispatch() signature needs offender parameter | `Era.Core/Counter/WcCounterMessage.cs` Dispatch(int action) | Change to `Dispatch(int action, CharacterId offender)` and update call site in `SendMessage()`. Existing HandleMessage10-16 remain parameterless (they use `_engine.GetTarget()` internally). Also benefits F807 TEASE handlers. |
| DI registration missing for IWcCounterMessageSex | `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` | Add `services.AddSingleton<IWcCounterMessageSex, WcCounterMessageSex>();` adjacent to existing IWcCounterMessageItem/IWcCounterMessageNtr registrations |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 32,33,79 | Extend IEngineVariables with `int GetTime() => 0` and `void SetTime(int value) {}` as default interface methods | (none) | [x] |
| 2 | 1,5,31,36 | Create `Era.Core/Counter/IWcCounterMessageSex.cs` with VaFitting and all 44 HandleMessageN method signatures; no NotImplementedException, no TODO/FIXME/HACK | (none) | [x] |
| 3 | 2,4,10,11,12,13,14,40,41,42,50,51,52,53,54,55,81,82 | Create `Era.Core/Counter/WcCounterMessageSex.cs` with `sealed class WcCounterMessageSex : IWcCounterMessageSex`; constructor receives all 16 DI dependencies; register `AddSingleton<IWcCounterMessageSex, WcCounterMessageSex>()` in ServiceCollectionExtensions.cs | (none) | [x] |
| 4 | 6,7,8,45,46,80 | Update `WcCounterMessage.cs` to inject `IWcCounterMessageSex sex` (store as `_sex`); change Dispatch(int action) to Dispatch(int action, CharacterId offender); update SendMessage() call site; replace all 44 `=> 0` SEX action ID stubs in Dispatch() with `=> _sex.HandleMessageN(offender)` delegation calls | (none) | [x] |
| 5 | 30,35,68,70 | Implement simple handlers: MESSAGE30-36 (7 handlers) and MESSAGE50-601 (MESSAGE50-60, 70-75, 80-83, 91, 500, 502-505, 600, 601 = 29 handlers); no NotImplementedException; no TODO/FIXME/HACK | (none) | [x] |
| 6 | 29,30,35,56,62,63,67,69,71,73 | Implement MESSAGE40 decomposed into private sub-methods: `HandleResponseCategory`, `ForcedOrgasm_CVA`, `HandleVirginity`, `HandlePhotography`, `WithOther`, `SexualDesire`; include BitfieldUtility SETBIT/GETBIT operations; call `_engine.SetTime(_engine.GetTime() + delta)` at 2 TIME += points in ForcedOrgasm_CVA and sexualDesire sub-methods (read-modify-write); include CFLAG (SetCharacterFlag via SexualDesire sub-method), TALENT (SetTalent), BASE/DOWNBASE, EQUIP/TEQUIP, TCVAR, STAIN mutation calls; no TODO/FIXME/HACK | (none) | [x] |
| 7 | 15,30,34,35,57,58,64,66,83 | Implement MESSAGE41-47: forced orgasm handlers (41-45 use ICharacterStateVariables GetEx), sexual desire handlers (46-47 call SexualDesire private method for 26-branch location routing); includes 2 TIME += points in MESSAGE41 and MESSAGE45 (AC#34/AC#57 cumulative gte 4 with Task#6); no NotImplementedException; no TODO/FIXME/HACK | (none) | [x] |
| 8 | 3,9,37 | Create `Era.Core.Tests/Counter/WcCounterMessageSexTests.cs` with test class skeleton; verify file exists with Assert or Verify calls; no TODO/FIXME/HACK | (none) | [x] |
| 9 | 16,17,18,19,20,43,72 | Write unit tests (expected values for value-equality assertions — VaFitting return values, SetDownbase deltas, SetStain values, SetTime deltas — MUST be derived from TOILET_COUNTER_MESSAGE_SEX.ERB, NOT from implementation output; mock.Verify assertions verify call presence, not ERB-derived values): 5+ VaFitting parameter combinations covering fitting levels 0-4 (C4); 3+ VaFitting return value assertions against ERB-derived expected values (C4 equivalence); 3+ CheckLostVirginity tests for VirginityType.Normal/Male/Anal (C5); 2+ SetBase/SetDownbase tests with delta values (C6); 2+ SetStain tests with bitwise OR semantics (C6); 2+ SetTime tests for TIME mutation (C6, C11); 3+ GetEx tests verifying EX counter access for MESSAGE43-45 (EX:MASTER:Ａ絶頂, Ｃ絶頂, Ｂ絶頂 per C12) (AC#72) | (none) | [x] |
| 10 | 21,22,23,24,25,44,47,48,49,76,77 | Write behavioral output tests (expected string values for Assert.Equal/Assert.Contains assertions MUST be derived from TOILET_COUNTER_MESSAGE_SEX.ERB source, NOT from C# implementation output): 3+ MESSAGE40 entry point tests; 7+ MESSAGE30-36 handler output tests; 7+ MESSAGE41-47 handler tests; 10+ sexualDesire location routing tests (C7); 2+ photography SetBit tests for WC photo flags (C8); 15+ simple handler MESSAGE50-601 tests (C9); 3+ WC_応答分類 ResponseCategory branching tests (C15); 2+ HasPenis/HasVagina gender branch tests (C14); 18+ string-equality assertions verifying handler text output (AC#49); each Assert.Equal/Assert.Contains must include co-located // ERB:{line_number} comment on the same line (AC#76) | (none) | [x] |
| 11 | 59,60,61,74,75 | Write state mutation tests: 2+ SetCharacterFlag/CFLAG mutation mock.Verify tests (C7, AC#59); 2+ SetTalent mutation mock.Verify tests (AC#60); 5+ IConsoleOutput.PrintLine mock.Verify call tests (AC#61); 1+ SetTCVar mutation mock.Verify tests (AC#74); 1+ SetEquip/SetTEquip mutation mock.Verify tests (AC#75) | (none) | [x] |
| 12 | 26,27,28,65,78 | Write cross-service delegation tests: 6+ BottomClothOff/BottomClothOut mock call tests (C2, 6 ERB call sites at 407,1540,1815,1903,2037,2578); 13 parameterized WithFirstSex tests with distinct CharacterId values (C3, one per character in SEX.ERB:1492-1516); 4+ KojoMessageWcCounter mock.Verify call tests; 1+ WithPetting delegation test (AC#65); 1+ WcCounterMessage delegation test verifying offender CharacterId is passed correctly to IWcCounterMessageSex.HandleMessageN (AC#78) | (none) | [x] |
| 13 | 38,39 | Run `dotnet build Era.Core/` with TreatWarningsAsErrors=true (succeeds); run `dotnet test Era.Core.Tests/ --blame-hang-timeout 10s` (all tests pass) | (none) | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Tasks 1-3 (IEngineVariables extension, IWcCounterMessageSex interface, WcCounterMessageSex class skeleton + DI registration) | IEngineVariables.cs updated, IWcCounterMessageSex.cs, WcCounterMessageSex.cs, ServiceCollectionExtensions.cs |
| 2 | implementer | sonnet | Task 4 (WcCounterMessage.cs update: inject IWcCounterMessageSex, change Dispatch signature, replace 44 stubs) | WcCounterMessage.cs updated |
| 3 | tester | sonnet | Task 8 (test skeleton creation, RED) | WcCounterMessageSexTests.cs (skeleton with Assert/Verify calls) |
| 4 | implementer | sonnet | Tasks 5-7 (simple handlers, MESSAGE40 decomposition, MESSAGE41-47, GREEN) | WcCounterMessageSex.cs fully implemented |
| 5 | tester | sonnet | Task 9 + Task 10 (behavioral output: MESSAGE40, MESSAGE30-36, MESSAGE41-47, MESSAGE50-601, sexualDesire, photography, ResponseCategory, gender) | WcCounterMessageSexTests.cs (behavioral output tests) |
| 6 | tester | sonnet | Task 11 (state mutation: CFLAG, TALENT, PrintLine mock.Verify) | WcCounterMessageSexTests.cs (state mutation tests) |
| 7 | tester | sonnet | Task 12 (cross-service delegation: BottomClothOff/Out, WithFirstSex x13, KOJO, WithPetting) | WcCounterMessageSexTests.cs (delegation tests) |
| 8 | implementer | sonnet | Task 13 (build + test verification) | Build PASS, all tests PASS |

**TDD order**: Task 8 creates test skeleton (RED) BEFORE Tasks 5-7 implement handlers (GREEN). Tests 9-12 add full assertions AFTER implementation.

**Rules**:
- File naming: `IWcCounterMessageSex.cs`, `WcCounterMessageSex.cs`, `WcCounterMessageSexTests.cs` (exact names required for AC static verification)
- Test naming: `{Method}_{Scenario}_{ExpectedResult}` pattern
- Build gate: `TreatWarningsAsErrors=true` applies. Zero compiler warnings permitted.
- Default interface methods: GetTime/SetTime on IEngineVariables (`=> 0` / `{ }`) — backward-compatible
- DI registration: `AddSingleton<IWcCounterMessageSex, WcCounterMessageSex>()` adjacent to IWcCounterMessageItem/IWcCounterMessageNtr
- Constructor injection: WcCounterMessageSex takes 16 deps; WcCounterMessage adds `IWcCounterMessageSex sex` as `_sex`
- No NotImplementedException: All 44 handler methods fully implemented; interface has no default throw bodies
- No TODO/FIXME/HACK: Three files checked
- BitfieldUtility: All SETBIT/GETBIT operations use `BitfieldUtility`
- Mock verification: Moq `mock.Verify(...)` for side-effect checks; Assert for return values
- Hang prevention: All `dotnet test` include `--blame-hang-timeout 10s`

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| IEngineVariables interface extension (GetTime/SetTime) requires core repo update if Era.Core is published (packaging concern only; behavioral override is tracked separately below) | Default interface methods added in F806 are source-only; if Era.Core NuGet is re-published after F806, the package version must be bumped | Feature | F813 | - | [x] | 確認済み |
| IEngineVariables GetTime/SetTime default bodies (=> 0 / {}) are behavioral no-ops until engine implementor overrides | Engine override required for TIME access correctness; without override, all 4 TIME += operations silently fail | Feature | F813 | Task#1 | [x] | 確認済み |
| WC_VA_FITTING public interface method must be documented for kojo authors | 9 kojo ERB files call WC_VA_FITTING; after C# migration, external callers must know to use IWcCounterMessageSex.VaFitting | Feature | F813 | - | [x] | 確認済み |
| Dispatch() signature change (int action → int action, CharacterId offender) requires F807 spec update | F806 changes shared Dispatch() method; F807 TEASE handlers also need offender parameter for delegation. F807 spec must update handler signatures and dispatch delegation accordingly | Feature | F807 | - | [x] | F807 chose parameterless handlers; offender convention unification deferred to F813 |
| KOJO 3-param overload investigation | Technical Constraint: "Some KOJO patterns pass extra param." F806 uses 2-param KojoMessageWcCounter overload for all 28 TRYCALLFORM patterns. Verify during /run that no pattern requires a 3rd parameter; if any does, add 3-param overload to IKojoMessageService | Feature | F813 | - | [x] | 確認済み |
| WcCounterMessageSex responsibility review (16 deps) | WcCounterMessageSex has 16 constructor dependencies, exceeding F808's WcCounterMessageNtr (9 deps) which was already flagged for responsibility splitting. Evaluate if handler groups (simple handlers, MESSAGE40 sub-methods, location routing) should be split into separate classes | Feature | F813 | - | [x] | 確認済み |
| Dispatch() dual offender convention: SEX handlers use explicit offender parameter; ITEM/NTR handlers use implicit _engine.GetTarget(). F807 TEASE must choose convention; consider refactoring HandleMessage10-16 for consistency | Mixed convention creates permanent dual-path in Dispatch() | Feature | F807, F813 | Task#4 | [x] | F807 chose implicit GetTarget(); dual convention persists; unification remains F813 scope |
| WcCounterMessageSex duplicate constant names (TalentVirginity2/TalentGender2) | TalentVirginity2 (=0) duplicates TalentVirginity; TalentGender2 (=2) duplicates TalentGender; maintainability risk if one is updated without the other | Feature | F813 | - | [x] | 確認済み |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-02 21:24 | START | implementer | Task 5 | - |
| 2026-03-02 21:24 | END | implementer | Task 5 | SUCCESS |
| 2026-03-02 22:34 | START | implementer | Task 7 | - |
| 2026-03-02 22:34 | END | implementer | Task 7 | SUCCESS |
| 2026-03-02 | DEVIATION | smart-implementer | Task 6 MESSAGE40 | Agent context overflow (work completed before overflow) |
| 2026-03-02 | DEVIATION | implementer | Task 6 MESSAGE40 retry | Agent context overflow (work completed before overflow) |
| 2026-03-02 | DEVIATION | Bash | dotnet build after Task 6 | CS0266: long→int cast (fixed immediately) |
| 2026-03-02 | DEVIATION | implementer | Tasks 9-10 behavioral tests | Agent context overflow (74 tests written before overflow) |
| 2026-03-02 | DEVIATION | feature-reviewer | Phase 8.1 post-review | NEEDS_REVISION: 3 [pending] items unresolved |
| 2026-03-02 | START | orchestrator | Task 13 build+test | - |
| 2026-03-02 | END | orchestrator | Task 13 build | SUCCESS: 0 warnings, 0 errors |
| 2026-03-02 | END | orchestrator | Task 13 devkit tests | SUCCESS: 805 passed, 0 failed |
| 2026-03-02 | END | orchestrator | Task 13 Era.Core unit tests | SUCCESS: 157 passed, 0 failed (110 WcCounterMessageSex) |
| 2026-03-02 | DEVIATION | Bash | Era.Core full test suite | PRE-EXISTING: 28 failures (26 integration file-not-found, 2 F807 WcCounterMessageTease regressions) |
| 2026-03-02 | DEVIATION | ac-tester | AC#83 verification | FAIL: 0 null-conditional ?.BottomClothOff/?.BottomClothOut found; implementation uses direct invocation on non-nullable DI interface |
| 2026-03-02 | DEVIATION | feature-reviewer | Phase 8.1 post-review | NEEDS_REVISION: C2 constraint text still references null-conditional after AC#83 fix |
| 2026-03-02 | CodeRabbit | Skip (erb) | - |

---

<!-- fc-phase-6-completed -->
## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase1-RefCheck iter1: [FMT-003] Links section | F811 referenced in body but missing from Links section; added [Related: F811]
- [fix] Phase1-RefCheck iter1: [DEP-001] Related Features table | F811 referenced in body but missing from Related Features table; added row
- [fix] Phase2-Review iter1: [FMT-002] Task Tags table | 2-column format replaced with 4-column template format (Tag/Meaning/Phase3/Phase4)
- [fix] Phase2-Review iter1: [FMT-002] Implementation Contract heading | Promoted ### to ## per template
- [fix] Phase2-Review iter1: [FMT-002] Mandatory Handoffs heading | Promoted ### to ## per template
- [fix] Phase2-Review iter1: [FMT-002] Implementation Contract TDD order | Reordered: Task 8 (RED) before Tasks 5-7 (GREEN) per CLAUDE.md TDD mandate
- [fix] Phase2-Review iter1: [CON-002] AC#27 threshold | Changed gte 3 to gte 13 per C3 constraint "all 13 character dispatches"
- [fix] Phase2-Review iter1: [AC-005] AC Definition Table | Added AC#43 VaFitting behavioral equivalence per Philosophy "no behavioral regression"
- [fix] Phase2-Review iter1: [FMT-001] Summary section | Removed non-template Summary section (content covered by Background > Goal)
- [fix] Phase2-Review iter2: [FMT-002] AC Coverage table header | Replaced `Criterion` column header with `AC#` per template format
- [fix] Phase2-Review iter2: [FMT-001] Background > Scope Reference | Moved non-template `### Scope Reference` subsection into `### Goal` body
- [fix] Phase2-Review iter2: [AC-001] Baseline Measurement | Replaced `TBD` with `0` (no WcCounterMessageSex tests exist before F806)
- [fix] Phase2-Review iter2: [INV-003] Philosophy Derivation row 2 | Replaced fabricated "enabling TDD" with actual Philosophy text "full DI and equivalence testing"
- [resolved-applied] Phase2-Uncertain iter2: [AC-005] Philosophy Derivation row 1 claims "all 49 ERB functions migrated" but AC coverage only directly verifies 44 action ID handlers + VaFitting. The 4 internal ERB helpers are covered indirectly by AC#15 and AC#56 but not by a dedicated AC verifying their existence as private methods.
- [fix] Phase2-Review iter3: [AC-005] Task#5 AC coverage | Added AC#58 (gte 36 simple handler method declarations in WcCounterMessageSex.cs) to Task#5; closes implementation-file verification gap for MESSAGE30-36 and MESSAGE50-601
- [fix] Phase2-Review iter4: [AC-002] AC#49 matcher | Removed `.Verify.*PrintLine` mock call proxy from AC#49 matcher; contradicted AC Detail "not mock call count proxies"
- [fix] Phase2-Review iter4: [AC-001] AC#58 threshold | Changed gte 37 to gte 36 (actual count: 7+11+6+4+1+5+2=36); fixed Task#5 "30 handlers" to "29 handlers"
- [fix] Phase2-Review iter4: [AC-002] AC#27 matcher | Removed `Theory.*WithFirstSex` from AC#27 matcher; inflated count by 1, allowing 12 InlineData entries to pass gte 13
- [fix] Phase2-Review iter5: [AC-001] AC#28 Detail | Updated AC#28 Detail to match table matcher (Verify.*KojoMessage pattern instead of stale setup-inclusive pattern)
- [fix] Phase2-Review iter5: [AC-005] Goal Coverage row 1 | Added AC#47 (MESSAGE41-47 tests) and AC#58 (gte 36 handler declarations) to Goal 1
- [fix] Phase2-Review iter5: [AC-005] Goal Coverage row 6 | Added AC#43 (VaFitting behavioral equivalence) to Goal 6
- [fix] Phase2-Review iter6: [AC-005] AC#59 | Added AC#59 MESSAGE40 dedicated string-equality verification (gte 3 in MESSAGE40-named tests); updated Task#10, Goal Coverage row 7, AC Coverage
- [fix] Phase2-Review iter6: [AC-001] Task#10 | Fixed "2+ KojoMessageWcCounter" to "4+ KojoMessageWcCounter mock.Verify" to match AC#28 gte 4
- [fix] Phase2-Review iter7: [AC-002] AC#59 | Removed AC#59 (broken single-line matcher; Message40.*Assert.Equal never matches across lines in xUnit); MESSAGE40 behavioral verification covered by AC#48+AC#49
- [fix] Phase2-Review iter7: [AC-002] AC#7 matcher | Changed `_sex\.|_wcCounterMessageSex\.` to `_sex\.HandleMessage` gte 44 to count only HandleMessageN delegation calls
- [resolved-skipped] Phase2-Review iter7: [AC-004] AC#49 matcher breadth | AC#49 pattern `Assert\.Equal.*"|Assert\.Contains.*"` matches any string assertion, not specifically ERB-derived handler output. Multiline scoping not practical with Grep-based matchers. Known limitation; AC#39 (all tests pass) provides runtime guarantee
- [fix] Phase2-Review iter8: [AC-001] AC#58 | Extended regex to include MESSAGE40-47 (4[0-7]), raised threshold from gte 36 to gte 44; now verifies all 44 handler method declarations; updated Task#6, Task#7 to reference AC#58
- [fix] Phase2-Review iter8: [AC-005] AC#59-60 | Added AC#59 (CFLAG mutation tests gte 2) and AC#60 (TALENT mutation tests gte 2) per Risks table MEDIUM/HIGH; updated Task#10, Goal Coverage row 6
- [fix] Phase2-Review iter8: [AC-005] AC#61 | Added AC#61 (Verify.*PrintLine gte 5) restoring IConsoleOutput side-effect verification removed from AC#49; updated Task#10, Goal Coverage row 6
- [fix] Phase2-Review iter9: [TSK-001] Task#5 AC#58 | Removed AC#58 from Task#5 (cumulative gte 44 not satisfiable by Task#5's 36 handlers alone); AC#58 remains in Task#7 (final handler task)
- [fix] Phase2-Review iter9: [AC-005] AC#62-63 | Added implementation-side AC#62 (SetCharacterFlag gte 1) and AC#63 (SetTalent gte 1) in WcCounterMessageSex.cs; updated Task#6, Goal Coverage row 6
- [fix] Phase2-Review iter10: [TSK-002] Task#11 | AC#59 (CFLAG), AC#60 (TALENT), AC#61 (PrintLine) state mutation test obligations assigned to Task#11 (not Task#10; Task#10/11 split occurred in iter4)
- [fix] Phase2-Review iter10: [AC-005] Philosophy Derivation | Added AC#56, AC#58, AC#62, AC#63 to row 1; AC#57 to row 3
- [fix] Phase2-Review iter10: [FMT-001] Mandatory Handoffs | Added KOJO 3-param overload investigation handoff to F813
- [fix] Phase2-Review iter11: [AC-002] AC#58 | Added \\b word boundary to prevent prefix matching (HandleMessage50 in HandleMessage500)
- [fix] Phase2-Review iter11: [AC-005] AC#64-65 | Added WithPetting implementation AC#64 (gte 1) and test AC#65 (gte 1) for SEX.ERB:2499; updated Task#7, Task#10, Goal Coverage row 6
- [fix] Phase2-Review iter11: [AC-005] AC#66 | Added GetEx usage AC#66 (gte 3 for MESSAGE43-45); updated Task#7, Philosophy Derivation row 3, Goal Coverage row 6
- [fix] Phase3-Maintainability iter10: [TSK-001] Task#5 | Removed AC#44 (test-file AC) from Task#5's implementation task AC column
- [fix] Phase3-Maintainability iter10: [CON-002] Task#9 | Added ERB-derivation mandate to Task#9 description (expected values must come from ERB source, not implementation)
- [fix] Phase4-ACValidation iter10: [AC-002] AC#43, AC#49 | Changed invalid type 'equiv' to 'code' per testing SKILL valid types
- [fix] Phase2-Review iter2: [AC-004] AC#27 matcher | Improved to require parameterized test entries with distinct character IDs per C3
- [fix] Phase2-Review iter3: [AC-001] Task#10 description | Updated WithFirstSex from "3+" to "13 parameterized" per AC#27 gte 13 alignment
- [fix] Phase2-Review iter3: [AC-002] AC#27 matcher | Simplified pattern to robust "WithFirstSex" (gte 13) from fragile multi-line InlineData pattern
- [fix] Phase2-Uncertain iter4: [AC-001] AC#27 Details | Added runtime execution guarantee note (AC#39 verifies distinct character dispatch paths)
- [fix] Phase2-Review iter4: [AC-005] AC Definition Table | Added AC#44 MESSAGE30-36 handler test coverage (fills C9 gap)
- [fix] Phase3-Maintainability iter5: [INV-003] Technical Design | Added Dispatch() signature change (int action → int action, CharacterId offender) per source code verification
- [fix] Phase3-Maintainability iter5: [INV-003] Technical Design | Fixed constructor count header (12 → 14 dependencies)
- [fix] Phase3-Maintainability iter5: [FMT-001] Upstream Issues | Added Dispatch() signature change entry
- [fix] Phase3-Maintainability iter5: [INV-003] Feasibility Assessment | Added note re ICounterUtilities/IItemVariables indirect access via IWcCounterMessageItem
- [fix] Phase2-Review iter6: [CON-002] AC#28 + AC#40 constraint ref | Fixed incorrect C15 citation to C16 (KOJO dispatch); added C16 to AC Design Constraints; raised AC#28 threshold from gte 2 to gte 4
- [fix] Phase2-Review iter7: [AC-005] AC Definition Table | Added AC#45 (Dispatch signature) and AC#46 (call-site update) for Dispatch() offender parameter threading
- [fix] Phase2-Review iter8: [AC-005] AC Definition Table | Added AC#47 MESSAGE41-47 handler test coverage (fills gap between AC#44 and AC#23)
- [fix] Phase3-Maintainability iter9: [FMT-001] Mandatory Handoffs | Added Dispatch() signature change handoff to F807
- [fix] Phase1-RefCheck iter1: [FMT-003] Key Decisions table | Removed dangling F412 reference (feature-412.md does not exist); "from F412" removed from EX variable access row
- [fix] Phase2-Review iter1: [FMT-002] Tasks table Tag column | Replaced custom tags (infra/impl/test/build) with template-valid (none) per Task Tags definition
- [fix] Phase2-Review iter1: [FMT-001] Deferred Obligations section | Moved from standalone ## section to ### subsection under Dependencies per template structure
- [fix] Phase2-Review iter1: [FMT-002] Implementation Contract | Replaced 2-column Rule/Description table with template 5-column Phase/Agent/Model/Input/Output table; preserved rules as notes
- [fix] Phase2-Uncertain iter1: [AC-005] AC Definition Table | Added AC#49 behavioral text output verification (ConsoleOutput mock assertions for deterministic handlers); updated Task#10, Goal Coverage, AC Coverage
- [fix] Phase2-Review iter1: [AC-002] AC#27 | Strengthened matcher from simple "WithFirstSex" count to "InlineData.*WithFirstSex" pattern requiring parameterized test structure
- [fix] Phase2-Uncertain iter2: [AC-001] AC#49 | Raised threshold from gte 2 to gte 6 (one per handler group: MESSAGE30-36, 50-60, 70-75, 80-83, 500, 502-505, 600-601)
- [fix] Phase2-Review iter2: [AC-005] AC Definition Table | Added AC#50-55 for 6 uncovered constructor dependencies (IVariableStore, IEngineVariables, ITEquipVariables, ITextFormatting, IConsoleOutput, IRandomProvider); updated Task#3, Goal Coverage, Philosophy Derivation
- [fix] Phase2-Review iter3: [AC-002] AC#49 | Strengthened matcher from ConsoleOutput proxy to string-equality pattern (Assert.Equal.*"|Assert.Contains.*"|.Verify.*PrintLine); changed Type from code to equiv; updated AC Details and AC Coverage
- [fix] Phase2-Review iter4: [AC-001] AC#49 | Raised threshold from gte 6 to gte 9 (expanded handler groups to include MESSAGE40, 41-47, 91 as distinct groups)
- [fix] Phase2-Review iter4: [AC-005] AC Definition Table | Added AC#56 MESSAGE40 private sub-method existence verification (6 named methods); updated Task#6, Goal Coverage row 7, AC Coverage
- [fix] Phase2-Review iter5: [FMT-001] Scope Reference | Added Function Classification documenting 49 ERB functions breakdown (44 action-ID handlers + 1 WC_VA_FITTING + 4 internal helpers)
- [fix] Phase2-Review iter5: [AC-001] AC#23 | Raised threshold from gte 5 to gte 15 (29 handlers require at least half to have named test references per C9)
- [fix] Phase2-Review iter6: [AC-001] Task#10 | Changed MESSAGE50-601 test count from "5+" to "15+" to align with AC#23 gte 15
- [fix] Phase2-Review iter6: [AC-001] Task#6 | Added SexualDesire to sub-method list (6th method required by AC#56); made TIME += read-modify-write semantics explicit
- [fix] Phase2-Review iter6: [AC-005] AC Definition Table | Added AC#57 GetTime verification (gte 4 calls for TIME read-modify-write); updated Task#6, Goal Coverage row 4, AC Coverage
- [fix] Phase2-Review iter7: [AC-002] AC#15 | Removed _counterUtilities from pattern (not a direct field per Feasibility note); added _equip and _random for ITEquipVariables/IRandomProvider
- [fix] Phase2-Review iter7: [AC-002] AC#28 | Strengthened matcher from setup-inclusive pattern to Verify.*KojoMessage assertion-only pattern
- [fix] Phase2-Review iter7: [AC-005] Goal Coverage row 9 | Added AC#30 (no NotImplementedException in implementation class) to code quality gates
- [fix] Phase2-Review iter1: [FMT-001] Section order | Moved ## Review Notes before ## Links per template order
- [fix] Phase2-Review iter1: [FMT-001] Dependencies section | Removed non-template ### Deferred Obligations subsection (content redundant with Background > Goal Function Classification)
- [resolved-applied] Phase2-Uncertain iter1: [AC-001] AC#21 threshold too low for sexualDesire location routing -- raised from gte 3 to gte 10 (~38% coverage of 26 SELECTCASE branches)
- [resolved-applied] Phase2-Uncertain iter1: [AC-001] AC#49 behavioral equivalence breadth -- raised from gte 9 to gte 18 (2+ assertions per handler group minimum)
- [fix] Phase2-Review iter2: [AC-005] Goal Coverage row 5 | Added AC#66 to Goal 5 covering ACs (GetEx verification completes EX interface gap resolution goal)
- [fix] Phase2-Review iter2: [AC-002] AC#27 matcher | Removed Theory.*WithFirstSex from pattern (previously logged as fixed in iter4 but pattern still contained it); ensures gte 13 counts only InlineData entries
- [fix] Phase2-Review iter3: [INV-003] Philosophy Derivation row 1 | Corrected "all 49 ERB functions migrated" to "44 action-ID handlers + VaFitting directly verified; 4 internal helpers covered indirectly via AC#15"; resolved existing [pending] iter2 item
- [fix] Phase2-Review iter3: [AC-002] AC#27 matcher + Detail | Fixed broken InlineData.*WithFirstSex pattern (xUnit InlineData and method name on separate lines) to simple "WithFirstSex" gte 13; updated AC description and rationale
- [fix] Phase3-Maintainability iter4: [FMT-001] AC Details | Added AC Detail sections for AC#58-66 (9 ACs missing Test/Expected/Derivation/Rationale blocks)
- [fix] Phase3-Maintainability iter4: [FMT-001] Feasibility Assessment | Added Volume Override justification note (3,518 lines / 66 ACs exceeds erb template guidelines; justified by single-file functional unit)
- [resolved-applied] Phase3-Maintainability iter4: [TSK-004] Task#10 granularity | Split Task#10 (16 ACs) into Task#10 (9 ACs, behavioral output), Task#12 (3 ACs, state mutation), Task#13 (4 ACs, cross-service delegation)
- [fix] Phase2-Review iter1: [FMT-001] Section order | Moved ## Technical Design after ## Acceptance Criteria per template-mandated order (AC Phase 3 before TD Phase 4)
- [fix] PostLoop-UserFix iter2: [AC-001] AC#21 threshold | Raised from gte 3 to gte 10 (sexualDesire 26 SELECTCASE branches, ~38% coverage)
- [fix] PostLoop-UserFix iter2: [AC-001] AC#49 threshold | Raised from gte 9 to gte 18 (2+ assertions per handler group minimum)
- [fix] PostLoop-UserFix iter2: [TSK-004] Task#10 split | Split into Task#10 (behavioral output, 9 ACs), Task#12 (state mutation, 3 ACs), Task#13 (cross-service delegation, 4 ACs)
- [fix] Phase2-Review iter1: [AC-001] AC#49 Details | Updated threshold from gte 9 to gte 18 and removed stale .Verify.*PrintLine pattern per table SSOT
- [fix] Phase2-Review iter1: [AC-001] AC#21 Details | Updated threshold from >= 3 to >= 10 per table SSOT
- [fix] Phase2-Review iter1: [FMT-002] Task renumbering | Renumbered Task#11→#11(state mutation), Task#12→#12(delegation), Task#13→#13(build gate) for sequential order; updated Implementation Contract
- [fix] Phase2-Review iter2: [INV-003] Philosophy Derivation row 1 | Changed "all 66 action IDs" to "all 44 SEX-category action IDs" (66 conflated F806+F808 scope)
- [fix] Phase2-Review iter2: [AC-001] AC#15 Details | Added explicit structural proxy documentation and behavioral gate cross-references (AC#39, AC#49)
- [fix] Phase3-Maintainability iter3: [FMT-001] AC Design Constraints | Added Mutation AC Coverage Note documenting which mutation types have dedicated vs structural-only ACs
- [fix] Phase2-Review iter4: [AC-001] AC Coverage row 27 | Fixed "parameterized WithFirstSex tests with distinct InlineData character IDs" to "13 individual [Fact] WithFirstSex test methods" per AC#27 Detail
- [fix] Phase3-Maintainability iter5: [TSK-001] Task#6 | Removed AC#48 (test-file AC) from implementation task AC column per iter10 precedent (AC#44 removal from Task#5)
- [fix] Phase3-Maintainability iter5: [TSK-001] Task#7 | Removed AC#47 (test-file AC) from implementation task AC column per same precedent
- [fix] Phase2-Review iter6: [AC-001] TD AC Coverage row 28 | Fixed "2+ tests" to "4+ tests" per AC#28 table SSOT (gte 4)
- [fix] Phase2-Review iter7: Review Notes header | Added mandatory comment header block per template
- [fix] Phase2-Review iter7: Implementation Contract | Changed ASCII arrow '->' to Unicode '→' per template
- [resolved-applied] Phase2-Uncertain iter7: AC#44 and AC#47 thresholds raised from gte 3 to gte 7 — aligns with 'no behavioral regression' Philosophy claim requiring all handlers in each group to have test coverage
- [fix] Phase2-Review iter8: [AC-001] TD/Constraints/Risks/Key Decisions | Changed "6-8 private sub-methods" to "6+ private sub-methods" to align with AC#56 (gte 6 named methods)
- [fix] Phase3-Maintainability iter9: [DEP-001] index-features.md | Added F808 to F806 predecessors (was F783, F805; now F783, F805, F808)
- [fix] Phase3-Maintainability iter9: [FMT-001] Mandatory Handoffs | Added WcCounterMessageSex responsibility review (14 deps) handoff to F813
- [fix] Phase3-Maintainability iter9: [AC-005] AC#67-69 | Added dedicated mutation presence ACs for BASE/DOWNBASE (AC#67), EQUIP/TEQUIP (AC#68), TCVAR (AC#69); updated Mutation Note, Goal Coverage, AC Coverage, Task#6
- [fix] Phase2-Review iter10: [AC-005] Philosophy Derivation row 1 | Added AC#64, AC#65, AC#67-69 to 'no behavioral regression' AC coverage
- [fix] Phase2-Review iter10: [AC-005] Goal Coverage row 1 | Added AC#64 (WithPetting implementation) to Goal 1 handler implementation coverage
- [fix] Phase2-Review iter10: [AC-001] Task#9 description | Clarified ERB-derivation mandate applies to value-equality assertions only; mock.Verify assertions verify call presence
- [fix] Phase2-Review iter11: [AC-005] Philosophy Derivation rows 1+2 | Added AC#44-48 to row 1 ('no behavioral regression'); added AC#40-42, AC#59-61 to row 2 ('full DI')
- [fix] Phase2-Review iter11: [AC-005] AC#70 | Added simple handler structural verification (gte 36 declarations); assigned to Task#5; updated Goal Coverage, AC Coverage, Philosophy Derivation
- [fix] Phase2-Review iter12: [AC-005] AC#71 | Added SetStain impl-side presence check (gte 1); assigned to Task#6; updated Mutation Note, Goal Coverage, Philosophy Derivation
- [fix] Phase2-Review iter12: [AC-005] AC#72 | Added GetEx test-side verification (gte 3 for MESSAGE43-45); assigned to Task#9; updated Goal Coverage, Philosophy Derivation row 3
- [fix] Phase2-Review iter12: [AC-005] Goal Coverage row 7 | Added AC#34 (SetTime) and AC#57 (GetTime) to MESSAGE40 decomposition covering ACs
- [fix] Phase2-Review iter13: [AC-005] AC#73 | Added GetStain read-side impl check (gte 1) for STAIN bitwise OR; assigned to Task#6
- [fix] Phase2-Review iter13: [AC-005] AC#74-75 | Added test-side mutation verification for TCVAR (AC#74) and EQUIP/TEQUIP (AC#75); assigned to Task#11
- [fix] Phase2-Review iter14: [TSK-001] Task#6/Task#7 AC#57 | Moved AC#57 (GetTime gte 4) from Task#6 to Task#7 (2 TIME calls in MESSAGE41/45 + 2 in ForcedOrgasm_CVA/sexualDesire = 4 cumulative); updated Task descriptions
- [fix] Phase2-Review iter14: [AC-005] Goal Coverage row 5 | Added AC#72 (GetEx test-side) to EX interface gap goal
- [fix] Phase2-Review iter15: [AC-001] AC#34 | Changed SetTime from `matches` (presence) to `gte 4` (count) to mirror AC#57 GetTime gte 4; symmetric read-modify-write coverage for all 4 TIME += ERB statements
- [fix] Phase2-Review iter16: [TSK-001] Task#6/Task#7 AC#34 | Moved AC#34 (SetTime gte 4) from Task#6 to Task#7 (same cumulative logic as AC#57 move in iter14)
- [fix] Phase2-Review iter16: [AC-002] AC#7 matcher | Strengthened from `_sex\.HandleMessage` to `=> _sex\.HandleMessage` to prevent false positives from comments/XML doc
- [fix] Phase2-Review iter1: [AC-005] AC#44, AC#47 | Raised thresholds from gte 3 to gte 7 — all handlers in MESSAGE30-36 and MESSAGE41-47 groups now require test coverage per Philosophy 'no behavioral regression'
- [fix] Phase2-Review iter1: [FMT-001] Implementation Contract | Renumbered phases 5b→6, 5c→7, 6→8 for sequential integer compliance
- [fix] Phase2-Review iter2: [AC-002] AC#49 + Task#10 | Added ERB-derivation source constraint to AC#49 Derivation and Task#10 description — prevents circular verification where test strings are copied from C# output instead of ERB source
- [resolved-applied] Phase2-Review iter3: AC#49 matcher enforcement gap resolved — added AC#76 (// ERB: comment annotation gte 18) for ERB source traceability; assigned to Task#10
- [fix] PostLoop-UserFix iter3: [AC-005] AC#76 | Added ERB source traceability AC (// ERB: annotation gte 18) — enforces AC#49 ERB-derivation mandate via auditable source line comments
- [fix] Phase2-Review iter1: [INV-003] Philosophy Derivation row 5 | Clarified rationale: positive test ACs attributed to rows 1 and 2; row 5 covers negative/absence ACs supplementing them
- [fix] Phase2-Review iter1: [INV-003] Review Notes iter10 | Corrected stale [fix] entry: AC#59/60/61 assigned to Task#11 (not Task#10); Task split occurred in iter4
- [fix] Phase2-Review iter4: [AC-002] AC#76 matcher | Strengthened from independent `// ERB:` count to co-located `Assert\.(?:Equal|Contains).*// ERB:` pattern — ensures ERB annotations are bound to assertion lines, not arbitrary comments
- [resolved-skipped] Phase2-Review iter5: TDD ordering — Implementation Contract's post-GREEN enrichment pattern (Tasks 9-12 after Tasks 5-7). Rationale: Pattern executed successfully during /run; test skeleton (Task 8) created before implementation (Tasks 5-7), assertions enriched after implementation (Tasks 9-12). Standard adaptation of TDD for large migration features where expected values depend on full handler logic.
- [fix] Phase2-Review iter1: [TSK-001] Task#5 AC#15 | Removed AC#15 from Task#5 (cumulative gte 100 not satisfiable by Task#5's 36 handlers alone, same logic as AC#58 removal in iter9); AC#70 provides Task#5-specific gate
- [fix] Phase2-Review iter1: [AC-001] AC#20 Detail | Strengthened AC#20 Detail to require ERB-derived delta values for SetTime (TOILET_COUNTER_MESSAGE_SEX.ERB lines 1742, 2376, 2406, 3064); prevents false-passing with SetTime(0)
- [fix] Phase3-Maintainability iter2: [INV-003] Technical Design | Added DIM backward compatibility note for IEngineVariables GetTime/SetTime default bodies (=> 0 / { })
- [fix] Phase3-Maintainability iter2: [INV-003] Technical Design | Added Responsibility Justification for 14-dependency constructor (all 14 required by ERB source cross-system API calls)
- [fix] Phase3-Maintainability iter2: [AC-004] Philosophy Derivation row 1 | Added coverage rationale documenting how AC#49+AC#76+AC#39+AC#44/47/48 provide layered behavioral regression coverage
- [fix] Phase2-Review iter3: [AC-001] AC#58 Derivation | Added explicit regex-to-group mapping documenting how alternation groups cover 44 handler method names (5[0-9]+60 split for MESSAGE50-60)
- [fix] Phase2-Review iter1: [AC-002] AC#72 matcher | Removed bare `EX` token from pattern `GetEx\|絶頂\|EX` → `GetEx\|絶頂` to prevent false-positive matches against 'Exception', 'Execute', etc.
- [fix] Phase2-Review iter1: [AC-002] AC#23 matcher | Added `Message91` to pattern to include MESSAGE91 simple handler in coverage count
- [resolved-invalid] Phase2-Review iter1: [AC-004] AC#49 subsumed by AC#76 — premise incorrect: AC#49 matcher (`Assert\.Equal.*"|Assert\.Contains.*"`) requires string-literal double-quote; AC#76 matcher (`Assert\.(?:Equal|Contains).*// ERB:`) requires ERB annotation. These test different aspects (string literal presence vs ERB source traceability). A line matching AC#76 without string literal (e.g., `Assert.Equal(expectedVar, actual); // ERB:1742`) does NOT match AC#49. Both ACs are needed: AC#49 gates hard-coded value assertions, AC#76 gates ERB source traceability.
- [fix] Phase3-Maintainability iter4: [AC-004] AC#49 vs AC#76 pending | Resolved [pending] as [resolved-invalid]: AC#49 and AC#76 use different matcher patterns (string-literal vs ERB-annotation), making them complementary not redundant
- [fix] Phase2-Review iter2: [AC-002] AC#18 matcher | Extended pattern from `SetDownbase|Downbase|DownBase` to `SetBase|SetDownbase|Downbase|DownBase` to match AC#67's implementation-side scope; renamed to 'Tests verify BASE/DOWNBASE mutation'
- [fix] Phase2-Review iter2: [TSK-001] Task#9 description | Added SetBase to '2+ SetBase/SetDownbase tests with delta values (C6)' for consistency with AC#18+AC#67
- [fix] Phase2-Review iter5: [TSK-001] Task#6 description | Added CFLAG (SetCharacterFlag), TALENT (SetTalent), and STAIN to Task#6 mutation list to align with AC#62, AC#63, AC#71 in AC column
- [fix] Phase2-Uncertain iter5: [AC-005] Philosophy Derivation row 5 | Added AC#8 (stub absence) to 'comprehensive testing' row per stated 'negative ACs verify absence of stubs' criterion
- [fix] Phase2-Review iter3: [INV-003] AC#56 + Technical Design | Corrected SexualDesire framing from 'decomposing MESSAGE40' to 'decomposing the MESSAGE40 handler group'; SexualDesire (ERB 2580-2835) is outside MESSAGE40 (68-1479), shared by HandleMessage40 + HandleMessage46-47
- [fix] Phase2-Review iter3: [AC-001] Line count | Standardized MESSAGE40 from 1,413 to 1,412 lines (1479-68+1=1412) across Technical Constraints, Risks, AC#48, AC#56, Key Decisions
- [resolved-invalid] Phase2-Review iter3: [AC-004] Philosophy Derivation row 1 behavioral verification gap — same concern as resolved-skipped iter7 AC#49 matcher breadth: Grep-based matchers cannot enforce value correctness. Already addressed by coverage rationale in Philosophy Derivation row 1 (added iter2) and ERB-derivation mandate in Task#9/Task#10 descriptions providing human-auditable correctness bridge
- [fix] Phase2-Review iter6: [AC-004] pending resolution | Resolved [pending] iter3 Philosophy Derivation behavioral verification gap as [resolved-invalid]: same concern as resolved-skipped iter7 AC#49, already addressed by coverage rationale in Philosophy Derivation row 1
- [fix] Phase2-Review iter4: [AC-002] AC#23 matcher | Changed `Message70` to `Message7[0-5]` and `Message80` to `Message8[0-3]` to match all handlers in MESSAGE70-75 and MESSAGE80-83 groups (Message503-505 already covered by `Message5[0-9]` substring match)
- [fix] Phase2-Review iter7: [AC-002] AC#23 matcher | Added explicit Message503/Message504/Message505 alternations for self-documentation (already covered by Message5[0-9] substring match, but explicit listing improves pattern auditability)
- [fix] Phase2-Review iter1: [CON-002] C2 source refs | Updated C2 from 2 call sites (407,1540) to all 6 (407,1540,1815,1903,2037,2578); raised AC#26 gte 2 → gte 6; updated AC#26 Detail and Task#12 description
- [fix] Phase2-Review iter1: [AC-005] AC#77 | Added MESSAGE40 sub-method test verification (ForcedOrgasm|SexualDesire|WithOther|HandleVirginity|HandlePhotography|HandleResponseCategory gte 4); updated Task#10, Goal Coverage row 7, Philosophy Derivation row 1
- [fix] Phase2-Review iter1: [INV-003] Problem section | Corrected IVariableStore narrative to reflect ISP-preferred ICharacterStateVariables injection resolution
- [fix] Phase2-Review iter1: [AC-001] AC#26 Detail | Updated Detail gte 2→6 and expanded ERB call site references (407,1540,1815,1903,2037,2578) to match AC Definition Table
- [fix] Phase2-Review iter1: [AC-005] AC#78 | Added WcCounterMessage delegation test AC (HandleMessage.*offender gte 1); closes static-only verification gap for Task#4's Dispatch signature change; assigned to Task#12, Goal Coverage row 3, Philosophy Derivation row 2
- [fix] Phase2-Review iter1: [AC-005] Philosophy Derivation row 2 | Added AC#50-AC#55 (6 injection ACs) to 'full DI' row; consistent with AC#10-AC#13 placement in row 2
- [fix] Phase2-Review iter1: [AC-005] Philosophy Derivation row 3 | Added AC#14 (ICharacterStateVariables injection) to EX interface gap resolution row
- [fix] Phase2-Review iter1: [TSK-002] Task#12 description | Added AC#78 work item (WcCounterMessage delegation offender test) to Task#12 description
- [fix] Phase3-Maintainability iter1: [FMT-004] Review Notes | Corrected [resolved-skipped] tag on TDD ordering entry to [pending] — per protocol, [resolved-skipped] is only valid from POST-LOOP user decision
- [resolved-skipped] Phase3-Maintainability iter1: WcCounterMessageSex 16-dependency constructor — single class retained. Rationale: all 16 dependencies map 1:1 to ERB source APIs (IVirginityManager→LOST_VIRGIN, ILocationService→BEDROOM/CFLAG:現在位置, etc.); splitting before implementation would be premature since coupling patterns are unknown pre-migration. F813 Mandatory Handoff explicitly tracks post-implementation responsibility review with full implementation evidence.
- [fix] Phase2-Review iter1: [INV-003] Risks table | Corrected KOJO dispatch pattern count from 22 to 28 to match C16/Technical Constraints SSOT
- [fix] Phase2-Review iter1: [INV-003] Impact Analysis | Corrected IVariableStore.cs row to ICharacterStateVariables (existing) — ISP-preferred injection, not interface extension, per C12/AC#14/Key Decisions
- [fix] Phase3-Maintainability iter1: [DEP-001] Mandatory Handoffs | Added IEngineVariables GetTime/SetTime engine override tracking (behavioral no-ops until engine implementor overrides); distinguished from NuGet packaging handoff
- [fix] Phase3-Maintainability iter1: [INV-003] Mandatory Handoffs + Key Decisions | Added Dispatch() dual offender convention handoff (F807/F813) and Key Decision documenting explicit vs implicit GetTarget() choice
- [fix] Phase3-Maintainability iter1: [AC-005] AC#79-80 | Added TODO/FIXME/HACK gate for modified shared files IEngineVariables.cs (Task#1) and WcCounterMessage.cs (Task#4); updated Goal Coverage, Philosophy Derivation
- [fix] Phase2-Review iter1: [AC-005] Philosophy Derivation row 5 | Added AC#38 (build) and AC#39 (tests pass) to 'comprehensive testing' row; these runtime correctness gates were cited in row 1 rationale but absent from coverage mapping
- [resolved-skipped] Phase3-Maintainability iter1: Dispatch() dual offender convention — mixed convention retained (explicit for SEX, implicit GetTarget() for ITEM/NTR). Rationale: HandleMessage10-16 belong to F808 [DONE] scope; modifying them in F806 is out-of-scope. F807 [DONE] independently chose implicit GetTarget() convention. Dual convention unification tracked in F813 Mandatory Handoff.
- [info] Phase1-DriftChecked: F807 (Related)
- [fix] Phase1-CodebaseDrift iter1: [INV-003] Root Cause/Key Decisions | WcCounterMessage constructor count 10→11 (post-F807 WcCounterMessageTease injection); line refs updated
- [fix] Phase1-CodebaseDrift iter1: [INV-003] Baseline Measurement | Changed "All 80 action IDs return 0" to "All 44 SEX action IDs return 0 (TEASE replaced by F807)"
- [fix] Phase1-CodebaseDrift iter1: [DEP-001] Mandatory Handoffs | Marked Dispatch() signature + dual offender convention handoffs to F807 as [x] transferred (F807 chose parameterless/implicit GetTarget())
- [fix] Phase1-CodebaseDrift iter1: [INV-003] Key Decisions | Updated offender convention to reflect F807 [DONE] chose implicit GetTarget()
- [resolved-invalid] Phase1-RefCheck-ApplyFailed iter1: _out/tmp/baseline-806.txt referenced in Baseline Measurement does not exist on disk (gitignored _out/tmp/ files have 7-day rotation; transient by design — absence is expected behavior, not a spec defect)
- [fix] Phase2-Review iter1: [FMT-004] Review Notes | Resolved baseline-806.txt [pending] as [resolved-invalid] per _out/tmp/ transient file policy
- [fix] Phase2-Review iter1: [TSK-001] Task#6 AC#15 | Removed AC#15 from Task#6 (cumulative gte 100 not satisfiable by Task#6's MESSAGE40 sub-methods alone; same logic as AC#15 removal from Task#5 in prior iter and AC#58 removal in iter9)
- [fix] Phase2-Uncertain iter2: [AC-004] Philosophy Derivation row 1 | Added TIME limitation cross-reference noting AC#39 mock-only coverage and engine override dependency (tracked in Mandatory Handoffs → F813)
- [fix] Phase2-Review iter3: [INV-003] AC#49 description + Details + AC#23 Details | Changed '500-505' to '500, 502-505' across all occurrences (MESSAGE501 does not exist per Function Classification)
- [fix] Phase2-Review iter4: [CON-002] AC#49 group count | Merged MESSAGE500 and MESSAGE502-505 into single group 'MESSAGE500/502-505' in description and Derivation; consistent with 9-group count × 2 = gte 18 threshold and Philosophy Derivation row 1 '9 handler groups'
- [fix] Phase3-Maintainability iter5: [INV-003] constructor count | Updated 14→16 dependencies across TD, Responsibility Justification, Implementation Contract, Mandatory Handoffs; added IItemVariables + ICounterUtilities to constructor list; corrected Feasibility Note removing incorrect 'not direct constructor dependencies' claim
- [fix] Phase3-Maintainability iter5: [INV-003] VaFitting signature | Updated from (int bustSize, int insertDepth) to (int inserter, int target, int holeType) per actual IWcCounterMessageSex.cs interface; updated TD Coverage and interface spec
- [fix] Phase3-Maintainability iter5: [AC-005] AC#81-82 | Added IItemVariables (AC#81) and ICounterUtilities (AC#82) injection verification ACs; assigned to Task#3; updated Philosophy Derivation row 2, Goal Coverage row 2, AC Coverage
- [resolved-applied] Phase3-Maintainability iter1: Duplicate constant names in WcCounterMessageSex.cs — TalentVirginity2 (=0, same as TalentVirginity) and TalentGender2 (=2, same as TalentGender); deferred to F813 for constant consolidation review. Mandatory Handoffs row added.
- [fix] Phase3-Maintainability iter1: pm/features/feature-806.md:1039 | Task#6 status updated from [ ] to [x] — code inspection confirms all 6 sub-methods exist (HandleResponseCategory, ForcedOrgasm_CVA, HandleVirginity, HandlePhotography, WithOther, SexualDesire) with mutations present
- [fix] Phase2-Uncertain iter2: [TSK-003] duplicate constants tracking | Updated [pending] to [resolved-applied] with F813 destination; added Mandatory Handoffs row for TalentVirginity2/TalentGender2 consolidation review
- [fix] Phase2-Review iter3: [TSK-001] Task#9 description | Added GetEx test obligation for AC#72 (3+ GetEx tests for MESSAGE43-45 EX counter access per C12)
- [fix] Phase2-Review iter4: [AC-005] AC#83 | Added C2 TRYCALL null-conditional verification AC (Grep for ?.BottomClothOff/?.BottomClothOut gte 2); assigned to Task#7; updated C2 AC Implication, Goal Coverage row 6, AC Coverage
- [fix] Phase2-Review iter5: [AC-005] Philosophy Derivation row 1 | Added AC#49 to AC Coverage (was cited in rationale but absent from range); merged AC#44-AC#48 + AC#50-AC#56 into AC#44-AC#56; added AC#83
- [fix] Phase2-Review iter5: [AC-005] Goal Coverage row 1 | Added AC#83 (C2 TRYCALL null-conditional) to Goal 1 covering ACs
- [fix] Phase2-Review iter6: [AC-002] AC#56 matcher | Changed `private.*` to `private void.*` to exclude constant declarations (CflagWcForcedOrgasmCount etc.); added AC Details note re AC#38 build gate
- [fix] Phase2-Review iter6: [AC-005] Philosophy Derivation row 1 | Changed AC#44-AC#56 to AC#44-AC#49, AC#56; removed injection-only AC#50-AC#55 per AC#10-AC#13 row 2 precedent
- [fix] Phase2-Review iter6: [TSK-001] Task#6/Task#5 AC#68 | Moved AC#68 (SetEquip/SetTEquip) from Task#6 to Task#5; EQUIP/TEQUIP writes are in MESSAGE50 (Task#5 scope), not MESSAGE40
- [fix] Phase7-Debug iter1: [AC-002] AC#83 matcher | Changed from null-conditional (?.) to direct invocation (.) pattern; Nullable=enable + TreatWarningsAsErrors makes ?. on non-nullable DI-injected IWcCounterMessageItem a build error (CS8073)
- [fix] Phase2-Review iter7: [AC-002] AC#83 Details + AC Coverage | Updated AC Details section and AC Coverage row to match Phase7-Debug iter1 fix (null-conditional → direct invocation; threshold gte 2 → gte 6)
- [fix] Phase8-PostReview iter1: [CON-002] C2 constraint text | Updated C2 AC Implication from 'null-conditional invocation' to 'direct invocation' per Phase7-Debug iter1 AC#83 fix

## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning
- [Predecessor: F805](feature-805.md) - WC Counter Source + Message Core
- [Predecessor: F808](feature-808.md) - WC Counter Message ITEM + NTR
- [Related: F807](feature-807.md) - WC Counter Message TEASE
- [Successor: F813](feature-813.md) - Post-Phase Review Phase 21
- [Related: F801](feature-801.md) - Main Counter Core
- [Related: F802](feature-802.md) - Main Counter Output
- [Related: F811](feature-811.md) - IKojoMessageService KojoMessageWcCounter overload
