# Feature 825: Relationships & DI Integration

## Status: [DONE]
<!-- fl-reviewed: 2026-03-05T12:02:38Z -->

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
<!-- fc-phase-2-completed -->

## Background

### Philosophy (Mid-term Vision)

Phase 22: State Systems is the SSOT for all character state subsystem migrations. F825 owns the relationships subsystem (続柄.ERB) migration to C# and serves as the final Phase 22 sub-feature responsible for DI integration batch registration and cross-subsystem wiring verification. All relationship logic must be encapsulated in a dedicated IRelationshipService, following the same DI injection pattern established by sibling features (F819-F824).

### Problem (Current Issue)

続柄.ERB (379 lines) contains the relationships subsystem as pure ERB script with no C# equivalent. The relationship functions (SONZOKU_CHECK, HIZOKU_CHECK, BOUKEI_CHECK, CHILDREM_CHECK, TUDUKIGARA_OUTPUT_ALL) use recursive algorithms on CFLAG:Father(73)/Mother(74) chains with hard depth limits (6, 8, 1 respectively) and produce relationship bitmasks using bit positions up to 51 (甥姪), requiring `long` rather than `int` for bitmask operations. Because all required data access interfaces (IVariableStore.GetCharacterFlag, IVariableStore.GetTalent, IEngineVariables.GetCharaNum, IEngineVariables.GetCharacterNo) already exist from sibling features, the migration itself is contained and low-risk.

The IComableUtilities/ICounterUtilities consolidation task originally assigned to F825 is architecturally unsound because the signature mismatch (IComableUtilities uses raw `int` parameters at IComableUtilities.cs:16,30,44 while ICounterUtilities uses strongly-typed `CharacterId` at ICounterUtilities.cs:12,21,36) is intentional -- COMABLE subsystem operates on raw character indices while COUNTER subsystem uses type-safe CharacterId. Forcing consolidation would weaken type safety and cascade across 30+ call sites in ComableChecker.

Tasks 5-11 (Null implementation completion) are already done: F813 (Post-Phase Review Phase 21) registered all 8 Null/Stub implementations at ServiceCollectionExtensions.cs:247-272. These tasks should be reclassified as verification-only.

**Deferred from F823**: ROOM_SMELL_WHOSE_SAMEN (ROOM_SMELL.ERB:1108-1140) blocked by I3DArrayVariables lacking GetDa/SetDa. DA interface gap is cross-cutting and out of scope.

**Deferred from F819**: (1) CLOTHES_ACCESSORY requires INtrQuery (NullNtrQuery returns false). (2) CFLAG backup slot indices partially resolved.

**Deferred from F821**: IEngineVariables indexed methods (GetDay/SetDay/GetTime/SetTime) are default interface no-op stubs pending engine repo implementation.

**Deferred from F824**: DI registration of own services (ISleepDepth, IMenstrualCycle, IHeartbreakService) done in F824. F825 owns cross-subsystem verification.

### Goal (What to Achieve)

Migrate 続柄.ERB relationship functions to a C# RelationshipService implementing IRelationshipService, verify all Phase 21 Null implementations are registered (reclassify Tasks 5-11 as verification), document IComableUtilities/ICounterUtilities signature divergence as intentional (not consolidate), and extend CP-2 DiResolutionTests with IRelationshipService DI resolution. CharacterFlagIndex.MultiBirthSiblingFlag (index 1106) must be added for 多生児兄弟フラグ support.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does the relationships subsystem need migration? | Because 続柄.ERB contains relationship logic as ERB script with no C# equivalent, blocking C# unit testing and DI integration | 続柄.ERB:1-379 |
| 2 | Why is there no C# equivalent yet? | Because 続柄.ERB was deferred as the final Phase 22 sub-feature, requiring all sibling subsystems (F819-F824) to complete first for DI wiring | F814 Phase 22 Planning |
| 3 | Why does it depend on sibling completion? | Because F825 owns cross-subsystem DI integration batch -- all interfaces must be registered before final wiring verification | ServiceCollectionExtensions.cs:247-272 |
| 4 | Why are all data access patterns already available? | Because sibling features established IVariableStore.GetCharacterFlag, IVariableStore.GetTalent, IEngineVariables.GetCharaNum/GetCharacterNo | IVariableStore.cs:28,34; IEngineVariables.cs:35,58 |
| 5 | Why (Root)? | The relationships subsystem is the last ERB-only holdout in Phase 22 because its recursive CFLAG chain traversal was less urgent than sibling subsystems, but all infrastructure now exists for a straightforward migration to IRelationshipService | CharacterFlagIndex.cs:40-41 (Father/Mother defined) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 続柄.ERB functions cannot be unit tested or DI-integrated | Relationship logic exists only as ERB script with no C# service abstraction |
| Where | 続柄.ERB (379 lines, 6 functions, 3 recursive) | Missing IRelationshipService/RelationshipService in Era.Core |
| Fix | Leave in ERB, test via headless mode only | Migrate to C# RelationshipService with IVariableStore/IEngineVariables DI injection |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F814 | [DONE] | Phase 22 Planning -- defined scope and obligations |
| F819 | [DONE] | Clothing System -- deferred items (INtrQuery, CFLAG backup slots) |
| F821 | [DONE] | Weather System -- deferred IEngineVariables indexed methods |
| F822 | [DONE] | Pregnancy System -- produces CFLAG Father/Mother data read by F825 |
| F823 | [DONE] | Room & Stain System -- deferred WHOSE_SAMEN DA gap |
| F824 | [DONE] | Sleep & Menstrual System -- DI pattern reference |
| F801 | [DONE] | Counter Utilities -- ICounterUtilities uses CharacterId (intentional divergence ref) |
| F809 | [DONE] | Comable Utilities -- IComableUtilities uses int (intentional divergence ref) |
| F813 | [DONE] | Post-Phase Review Phase 21 -- registered all Null stubs |
| F826 | [DRAFT] | Post-Phase Review Phase 22 -- successor |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| ERB migration to RelationshipService | FEASIBLE | All interfaces exist (IVariableStore, IEngineVariables, CharacterFlagIndex, Constants) |
| DI integration batch verification | FEASIBLE | All Null implementations already registered at ServiceCollectionExtensions.cs:247-272 |
| IComableUtilities/ICounterUtilities | FEASIBLE | Document as intentional divergence (int vs CharacterId is type-safety by design) |
| CharacterFlagIndex.MultiBirthSiblingFlag | FEASIBLE | Trivial addition of index 1106 |
| CP-2 DiResolutionTests extension | FEASIBLE | Established pattern at DiResolutionTests.cs:34-267 |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core State layer | MEDIUM | New IRelationshipService/RelationshipService added |
| DI container | LOW | Single new registration + verification of existing registrations |
| CharacterFlagIndex | LOW | One new constant (MultiBirthSiblingFlag = 1106) |
| External ERB callers | LOW | Only 2 callers (住人同士の交流設定.ERB:306, SHOP.ERB:152) -- remain in ERB |
| Phase 22 sibling features | LOW | No code changes to siblings; verification only |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Relationship bitmask requires `long` (64-bit) -- bit 51 (甥姪) exceeds 32-bit int | 続柄.ERB RelationshipTypes | All bitmask operations must use long, not int |
| IComableUtilities uses `int`; ICounterUtilities uses `CharacterId` for 3 overlapping methods | IComableUtilities.cs:16,30,44; ICounterUtilities.cs:12,21,36 | Cannot merge -- document as intentional divergence |
| 30+ call sites depend on IComableUtilities `int` signatures | ComableChecker Range3x/4x | Breaking changes would cascade; no consolidation |
| CHILDREM_CHECK uses NO:ARG (CSV number 148/149) not character index | 続柄.ERB:377 | Requires IEngineVariables.GetCharacterNo(int) |
| Recursive depth limits (SONZOKU=6, HIZOKU=8, BOUKEI=1) must match ERB | 続柄.ERB:250,288,325 | Hard limits in C# must match ERB defaults |
| Remilia(5)/Flan(6) hardcoded in sibling checks | 続柄.ERB:218-221 | Must use CharacterId.Remilia/CharacterId.Flandre (well-known constants) |
| Two-level CFLAG dereference for grandparent lookup | 続柄.ERB CFLAG chain | CFLAG:(CFLAG:X:Father):Father pattern |
| DA interface gap (I3DArrayVariables lacks GetDa/SetDa) | I3DArrayVariables.cs:10-35 | WHOSE_SAMEN explicitly deferred -- out of scope |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Bitmask overflow if using int instead of long | LOW | HIGH | Use long throughout; bit 51 test in unit tests |
| Tasks 5-11 misunderstood as requiring new code | HIGH | LOW | Reclassify as verification-only in AC design |
| Recursive logic bugs in CFLAG chain traversal | LOW | MEDIUM | TDD with family tree fixtures; test depth limits |
| MultiBirthSiblingFlag index incorrect | LOW | LOW | Verify against CFLAG.yaml index 1106 |
| Scope creep from consolidation attempt | MEDIUM | MEDIUM | Explicitly document consolidation as out of scope (intentional divergence) |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Existing DI registrations | Grep ServiceCollectionExtensions.cs for AddSingleton/AddTransient | 8 Null/Stub implementations registered | Lines 247-272 |
| DiResolutionTests count | Grep DiResolutionTests.cs for \[Fact\] | Existing count | To be extended with IRelationshipService |
| CharacterFlagIndex constants | Grep CharacterFlagIndex.cs for "new(" | Existing count (includes Father=73, Mother=74) | MultiBirthSiblingFlag to be added |

**Baseline File**: `_out/tmp/baseline-825.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Functions are pure computation (no side effects, no PRINT, no state mutation) | 続柄.ERB analysis | Unit tests with mock IVariableStore; no headless mode needed for logic tests |
| C2 | CHILDREM_CHECK uses NO:ARG (CSV number 148/149) | 続柄.ERB:377 | Test fixtures must set up NO values via IEngineVariables.GetCharacterNo |
| C3 | Remilia(5)/Flan(6) special case in sibling checks | 続柄.ERB:218-221 | Must test specific character IDs using Constants |
| C4 | Bitmask requires long (bit 51 = 甥姪) | RelationshipTypes bit positions | Verify bitmask operations use long; test bit 51 specifically |
| C5 | CP-2 via DiResolutionTests pattern | Established pattern DiResolutionTests.cs | Extend with IRelationshipService resolution assertion |
| C6 | All 8 Null stubs already exist and are registered | ServiceCollectionExtensions.cs:247-272 | Tasks 5-11 = verify registration only, not create new code |
| C7 | DA gap (WHOSE_SAMEN) out of scope | I3DArrayVariables.cs:10-35 | No WHOSE_SAMEN AC; explicitly excluded |
| C8 | IComableUtilities/ICounterUtilities divergence is intentional | IComableUtilities.cs:16; ICounterUtilities.cs:12 | No consolidation AC; document divergence instead |
| C9 | Recursive depth limits must match ERB | 続柄.ERB:250,288,325 | SONZOKU=6, HIZOKU=8, BOUKEI=1 in test assertions |
| C10 | Only 2 external ERB callers | 住人同士の交流設定.ERB:306, SHOP.ERB:152 | External callers remain in ERB; no ERB-to-C# delegation AC needed |
| C11 | MultiBirthSiblingFlag CFLAG index 1106 missing | CharacterFlagIndex.cs (absent) | AC must verify addition of index constant |
| C12 | erb type but logic is pure computation | Feature type = erb | Equivalence tests can use unit tests (mock IVariableStore), not headless mode |

### Constraint Details

**C1: Pure Computation Functions**
- **Source**: 続柄.ERB analysis -- all functions return values via RESULT/RESULTS without PRINT or state mutation
- **Verification**: Confirm no PRINT/PRINTL statements in 続柄.ERB functions
- **AC Impact**: Unit tests with mock IVariableStore are sufficient; no headless game execution needed for logic verification

**C4: Long Bitmask Requirement**
- **Source**: RelationshipTypes.cs constants include bit positions up to 51 (甥姪/niece-nephew)
- **Verification**: Confirm bit 51 used in RelationshipTypes.cs
- **AC Impact**: All bitmask parameters and return types must use long; test must verify bit 51 set/read correctly

**C2: CHILDREM_CHECK Uses NO:ARG (CSV Number 148/149)**
- **Source**: 続柄.ERB:377 — `SIF NO:ARG == 148 || NO:ARG == 149`
- **Verification**: Read 続柄.ERB CHILDREM_CHECK function to confirm NO comparison
- **AC Impact**: AC#12 verifies GetCharacterNo + 148/149 references in RelationshipService.cs

**C3: Remilia/Flan Special Case in Sibling Checks**
- **Source**: 続柄.ERB:218-221, 239-242, 361-364 — hardcoded Scarlet sister relationship branches
- **Verification**: Read 続柄.ERB for 人物_レミリア/人物_フラン references
- **AC Impact**: AC#13 verifies Constants.人物_レミリア/人物_フラン usage in RelationshipService.cs

**C5: CP-2 DiResolutionTests Pattern**
- **Source**: Established pattern at DiResolutionTests.cs:34-267 — one [Fact] per interface resolution
- **Verification**: Read DiResolutionTests.cs to confirm pattern
- **AC Impact**: AC#15 verifies IRelationshipService added; AC#21 verifies all E2E tests pass

**C6: Null Implementations Already Complete**
- **Source**: F813 Task 5 registered all missing Phase 21 interfaces; ServiceCollectionExtensions.cs:247-272 confirms
- **Verification**: Grep ServiceCollectionExtensions.cs for all 8 Null class names
- **AC Impact**: No "create Null implementation" ACs; only "verify registration exists" ACs

**C8: Intentional Interface Divergence**
- **Source**: IComableUtilities (F809) uses raw int for COMABLE subsystem; ICounterUtilities (F801) uses CharacterId for COUNTER subsystem. 3 overlapping methods: TimeProgress (identical int signature in both), IsAirMaster (int vs CharacterId), GetTargetNum (no param vs CharacterId). 2 of 3 have divergent parameter types.
- **Verification**: Grep both interfaces for overlapping method names (TimeProgress, IsAirMaster, GetTargetNum)
- **AC Impact**: No consolidation AC; instead add documentation/comment AC noting intentional divergence

**C9: Recursive Depth Limits**
- **Source**: 続柄.ERB default parameters: SONZOKU_CHECK depth=6 (line 250), HIZOKU_CHECK depth=8 (line 288), BOUKEI_CHECK depth=1 (line 325)
- **Verification**: Read 続柄.ERB default parameter values
- **AC Impact**: Test each recursive function at max depth and verify termination

**C10: Only 2 External ERB Callers**
- **Source**: 住人同士の交流設定.ERB:306 (TUDUKIGARA_OUTPUT_ALL), SHOP.ERB:152 (CHILDREM_CHECK)
- **Verification**: Grep game ERB files for TUDUKIGARA/CHILDREM_CHECK calls
- **AC Impact**: No ERB-to-C# delegation AC needed — external callers remain in ERB

**C11: MultiBirthSiblingFlag Gap**
- **Source**: CFLAG.yaml index 646-647 defines 多生児兄弟フラグ at position 1106; CharacterFlagIndex.cs lacks this constant
- **Verification**: Grep CharacterFlagIndex.cs for "1106" or "MultiBirth"
- **AC Impact**: AC must verify CharacterFlagIndex.MultiBirthSiblingFlag exists with correct index value

**C12: erb Type But Logic Is Pure Computation**
- **Source**: 続柄.ERB analysis — all functions return via RETURNF/RESULT with no PRINT/state mutation
- **Verification**: Confirm no PRINT/PRINTL in 続柄.ERB functions
- **AC Impact**: Unit tests with mock IVariableStore are sufficient; no headless game execution needed

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning -- defined scope and obligations |
| Predecessor | F819 | [DONE] | Clothing System -- DI integration requires all subsystem interfaces |
| Predecessor | F821 | [DONE] | Weather System -- DI integration requires all subsystem interfaces |
| Predecessor | F822 | [DONE] | Pregnancy System -- produces CFLAG Father/Mother data (read-only data dependency) |
| Predecessor | F823 | [DONE] | Room & Stain System -- DI integration requires all subsystem interfaces |
| Predecessor | F824 | [DONE] | Sleep & Menstrual System -- DI integration requires all subsystem interfaces |
| Related | F813 | [DONE] | Post-Phase Review Phase 21 -- registered all Null stubs (Tasks 5-11 verification source) |
| Successor | F826 | [DRAFT] | Post-Phase Review Phase 22 -- depends on F825 completion |

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

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "F825 owns the relationships subsystem (続柄.ERB) migration to C#" | IRelationshipService interface and RelationshipService implementation must exist | AC#1, AC#2 |
| "All relationship logic must be encapsulated in a dedicated IRelationshipService" | All 6 ERB functions (TUDUKIGARA_OUTPUT_ALL, TUDUKIGARA_CHECK, SONZOKU_CHECK, HIZOKU_CHECK, BOUKEI_CHECK, CHILDREM_CHECK) must have C# equivalents in RelationshipService | AC#3, AC#4 |
| "following the same DI injection pattern established by sibling features (F819-F824)" | RelationshipService must use constructor DI for IVariableStore, IEngineVariables; must be registered in ServiceCollectionExtensions | AC#5, AC#6 |
| "serves as the final Phase 22 sub-feature responsible for DI integration batch registration and cross-subsystem wiring verification" | All Phase 21 Null stubs verified registered; IRelationshipService DI-registered; DiResolutionTests extended and passing | AC#6, AC#15, AC#16, AC#21 |
| "Phase 22: State Systems is the SSOT for all character state subsystem migrations" | No duplicate IRelationshipService implementations | AC#7 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IRelationshipService interface exists | code | Grep(Era.Core/**/IRelationshipService.cs) | matches | `interface IRelationshipService` | [x] |
| 2 | RelationshipService implements IRelationshipService | code | Grep(Era.Core/**/RelationshipService.cs) | matches | `RelationshipService.*IRelationshipService` | [x] |
| 3 | RelationshipService contains all 6 migrated functions | code | Grep(Era.Core/**/RelationshipService.cs, pattern="CheckRelationship\|OutputAllRelationships\|CheckAncestors\|CheckDescendants\|CheckCollateral\|IsChild") | gte | 6 | [x] |
| 4 | Unit tests exist for relationship functions | code | Grep(Era.Core.Tests/**/RelationshipServiceTests.cs, pattern="\[Fact\]\|\[Theory\]") | gte | 8 | [x] |
| 5 | RelationshipService injects IVariableStore and IEngineVariables | code | Grep(Era.Core/**/RelationshipService.cs, pattern="IVariableStore\|IEngineVariables") | gte | 2 | [x] |
| 6 | IRelationshipService registered in ServiceCollectionExtensions | code | Grep(Era.Core/**/ServiceCollectionExtensions.cs, pattern="AddSingleton.*IRelationshipService.*RelationshipService") | matches | `AddSingleton.*IRelationshipService.*RelationshipService` | [x] |
| 7 | IRelationshipService SSOT enforcement -- single implementation | code | Grep(Era.Core/**/, pattern=": IRelationshipService") | lte | 1 | [x] |
| 8 | Bitmask return type uses long (not int) | code | Grep(Era.Core/**/IRelationshipService.cs, pattern="long") | matches | `long` | [x] |
| 9 | Bit 51 (甥姪) tested in unit tests | code | Grep(Era.Core.Tests/**/RelationshipServiceTests.cs, pattern="51\|甥姪\|NieceNephew") | matches | `51\|甥姪\|NieceNephew` | [x] |
| 10 | SONZOKU depth limit 6 tested | code | Grep(Era.Core.Tests/**/RelationshipServiceTests.cs, pattern="6\|Ancestor.*Depth\|尊属") | matches | `6\|Ancestor.*Depth\|尊属` | [x] |
| 11 | HIZOKU depth limit 8 tested | code | Grep(Era.Core.Tests/**/RelationshipServiceTests.cs, pattern="8\|Descendant.*Depth\|卑属") | matches | `8\|Descendant.*Depth\|卑属` | [x] |
| 12 | CHILDREM_CHECK uses GetCharacterNo with NO values 148/149 | code | Grep(Era.Core/**/RelationshipService.cs, pattern="GetCharacterNo\|148\|149") | gte | 2 | [x] |
| 13 | Remilia/Flan special case uses well-known CharacterId constants | code | Grep(Era.Core/**/RelationshipService.cs, pattern="CharacterId\\.Remilia\|CharacterId\\.Flandre\|人物_レミリア\|人物_フラン") | gte | 2 | [x] |
| 14 | CharacterFlagIndex.MultiBirthSiblingFlag exists with index 1106 | code | Grep(Era.Core/**/CharacterFlagIndex.cs, pattern="MultiBirthSiblingFlag.*new\(1106\)") | matches | `MultiBirthSiblingFlag.*new\(1106\)` | [x] |
| 15 | DiResolutionTests extended with IRelationshipService resolution | code | Grep(Era.Core.Tests/**/DiResolutionTests.cs, pattern="IRelationshipService") | matches | `IRelationshipService` | [x] |
| 16 | All 8 Null/Stub registrations verified present | code | Grep(Era.Core/**/ServiceCollectionExtensions.cs, pattern="NullCounterUtilities\|NullWcSexHaraService\|NullNtrUtilityService\|NullShrinkageSystem\|NullTrainingCheckService\|NullKnickersSystem\|NullEjaculationProcessor\|NullKojoMessageService") | gte | 8 | [x] |
| 17 | IComableUtilities divergence documented | code | Grep(Era.Core/**/IComableUtilities.cs, pattern="intentional\|diverge\|by design\|separate") | matches | `intentional\|diverge\|by design\|separate` | [x] |
| 18 | No TODO/FIXME/HACK in RelationshipService | code | Grep(Era.Core/**/RelationshipService.cs, pattern="TODO\|FIXME\|HACK") | not_matches | `TODO\|FIXME\|HACK` | [x] |
| 19 | No TODO/FIXME/HACK in IRelationshipService | code | Grep(Era.Core/**/IRelationshipService.cs, pattern="TODO\|FIXME\|HACK") | not_matches | `TODO\|FIXME\|HACK` | [x] |
| 20 | No TODO/FIXME/HACK in RelationshipServiceTests | code | Grep(Era.Core.Tests/**/RelationshipServiceTests.cs, pattern="TODO\|FIXME\|HACK") | not_matches | `TODO\|FIXME\|HACK` | [x] |
| 21 | CP-2 E2E: DiResolutionTests all pass | test | dotnet test --filter "Category=E2E" | succeeds | Exit code 0 | [x] |
| 22 | RelationshipService unit tests all pass | test | dotnet test --filter "FullyQualifiedName~RelationshipServiceTests" | succeeds | Exit code 0 | [x] |
| 23 | Era.Core builds without errors | build | dotnet build Era.Core | succeeds | Exit code 0 | [x] |
| 24 | BOUKEI depth limit 1 tested | code | Grep(Era.Core.Tests/**/RelationshipServiceTests.cs, pattern="Collateral.*Depth\|傍系\|maxDepth.*1") | matches | `Collateral.*Depth\|傍系\|maxDepth.*1` | [x] |
| 25 | ICounterUtilities divergence documented | code | Grep(Era.Core/**/ICounterUtilities.cs, pattern="intentional\|diverge\|by design\|separate") | matches | `intentional\|diverge\|by design\|separate` | [x] |
| 26 | RelationshipService uses MultiBirthSiblingFlag | code | Grep(Era.Core/**/RelationshipService.cs, pattern="MultiBirthSiblingFlag") | matches | `MultiBirthSiblingFlag` | [x] |

### AC Details

**AC#3: RelationshipService contains all 6 migrated functions**
- **Test**: `Grep(Era.Core/**/RelationshipService.cs, pattern="CheckRelationship|OutputAllRelationships|CheckAncestors|CheckDescendants|CheckCollateral|IsChild")`
- **Expected**: `gte 6`
- **Rationale**: 続柄.ERB has 6 functions: TUDUKIGARA_OUTPUT_ALL -> OutputAllRelationships, TUDUKIGARA_CHECK -> CheckRelationship, SONZOKU_CHECK -> CheckAncestors, HIZOKU_CHECK -> CheckDescendants, BOUKEI_CHECK -> CheckCollateral, CHILDREM_CHECK -> IsChild. Each must have a C# method. Count = 6 method definitions.
- **Derivation**: 6 functions enumerated from 続柄.ERB source (lines 5, 174, 250, 288, 325, 373)

**AC#4: Unit tests exist for relationship functions**
- **Test**: `Grep(Era.Core.Tests/**/RelationshipServiceTests.cs, pattern="[Fact]|[Theory]")`
- **Expected**: `gte 8`
- **Rationale**: Minimum 8 tests: parent detection, child detection, sibling (elder/younger), grandparent, ancestor depth limit, descendant depth limit, Remilia/Flan special case, bit 51 long bitmask. Pure computation (C1) allows full unit test coverage.
- **Derivation**: 6 functions where OutputAllRelationships delegates to CheckRelationship (1 group) + 4 independent functions = 5 testable groups x 1 test + 3 boundary cases (depth limits, long bitmask, Remilia/Flan) = 8 minimum

**AC#5: RelationshipService injects IVariableStore and IEngineVariables**
- **Test**: `Grep(Era.Core/**/RelationshipService.cs, pattern="IVariableStore|IEngineVariables")`
- **Expected**: `gte 2`
- **Rationale**: RelationshipService needs IVariableStore for CFLAG access (Father/Mother chain traversal) and IEngineVariables for GetCharacterNo (CHILDREM_CHECK) and GetCharaNum (CHARANUM). Following sibling DI pattern (C5).
- **Derivation**: 2 required interfaces: IVariableStore (CFLAG:X:Father, CFLAG:X:Mother, TALENT:X:性別) + IEngineVariables (GetCharacterNo for NO:ARG, GetCharaNum for CHARANUM)

**AC#7: IRelationshipService SSOT enforcement**
- **Test**: `Grep(Era.Core/**/, pattern=": IRelationshipService")`
- **Expected**: `lte 1`
- **Rationale**: Philosophy claims SSOT -- only one class should implement IRelationshipService.
- **Derivation**: SSOT enforcement per ERB Issue 15 anti-pattern prevention

**AC#12: CHILDREM_CHECK uses GetCharacterNo with NO values 148/149**
- **Test**: `Grep(Era.Core/**/RelationshipService.cs, pattern="GetCharacterNo|148|149")`
- **Expected**: `gte 2`
- **Rationale**: CHILDREM_CHECK (続柄.ERB:373-379) uses NO:ARG to check if character number is 148 (son) or 149 (daughter). C# must use IEngineVariables.GetCharacterNo and compare against 148/149 (C2).
- **Derivation**: 1 GetCharacterNo call + at least 1 reference to 148 or 149 = gte 2

**AC#16: All 8 Null/Stub registrations verified present**
- **Test**: `Grep(Era.Core/**/ServiceCollectionExtensions.cs, pattern="NullCounterUtilities|NullWcSexHaraService|NullNtrUtilityService|NullShrinkageSystem|NullTrainingCheckService|NullKnickersSystem|NullEjaculationProcessor|NullKojoMessageService")`
- **Expected**: `gte 8`
- **Rationale**: Tasks 5-11 reclassified as verification-only (C6). All 8 Null implementations registered by F813 at ServiceCollectionExtensions.cs:265-272. Verification confirms no regression.
- **Derivation**: 8 Null/Stub classes: NullCounterUtilities (line 265), NullWcSexHaraService (266), NullNtrUtilityService (267), NullShrinkageSystem (268), NullTrainingCheckService (269), NullKnickersSystem (270), NullEjaculationProcessor (271), NullKojoMessageService (272)

**AC#13: Remilia/Flan special case uses well-known CharacterId constants**
- **Test**: `Grep(Era.Core/**/RelationshipService.cs, pattern="CharacterId\\.Remilia|CharacterId\\.Flandre|人物_レミリア|人物_フラン")`
- **Expected**: `gte 2`
- **Rationale**: 続柄.ERB lines 218-221, 239-242, 361-364 hardcode Remilia(5)/Flan(6) for sibling special cases. C# should use `CharacterId.Remilia` and `CharacterId.Flandre` (well-known constants at CharacterId.cs:25-26). Pattern also accepts `Constants.人物_レミリア/人物_フラン` for backward compatibility.
- **Derivation**: 2 distinct constants referenced: Remilia + Flandre = minimum 2 occurrences

**AC#24: BOUKEI depth limit 1 tested**
- **Test**: `Grep(Era.Core.Tests/**/RelationshipServiceTests.cs, pattern="Collateral.*Depth|傍系|maxDepth.*1")`
- **Expected**: `matches`
- **Rationale**: Constraint C9 requires all three depth limits tested: SONZOKU=6 (AC#10), HIZOKU=8 (AC#11), BOUKEI=1 (this AC). BOUKEI_CHECK (CheckCollateral) uses maxDepth=1 as default, verifying siblings share exactly 1 common parent generation.
- **Derivation**: C9 depth limit parity — all recursive functions must have their depth limit tested

**AC#26: RelationshipService uses MultiBirthSiblingFlag**
- **Test**: `Grep(Era.Core/**/RelationshipService.cs, pattern="MultiBirthSiblingFlag")`
- **Expected**: `matches`
- **Rationale**: 続柄.ERB:43-45 uses CFLAG:多生児兄弟フラグ (index 1106) in TUDUKIGARA_OUTPUT_ALL for multi-birth sibling labeling (双子/三つ子). AC#14 verifies the constant exists; this AC verifies it is consumed by RelationshipService.OutputAllRelationships.
- **Derivation**: C11 MultiBirthSiblingFlag gap — existence (AC#14) + usage (AC#26)

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Migrate 続柄.ERB relationship functions to RelationshipService implementing IRelationshipService | AC#1, AC#2, AC#3, AC#5, AC#8, AC#12, AC#13 |
| 2 | Verify all Phase 21 Null implementations are registered (Tasks 5-11 verification) | AC#16 |
| 3 | Document IComableUtilities/ICounterUtilities signature divergence as intentional | AC#17, AC#25 |
| 4 | Extend CP-2 DiResolutionTests with IRelationshipService DI resolution | AC#6, AC#15, AC#21 |
| 5 | Add CharacterFlagIndex.MultiBirthSiblingFlag (index 1106) and verify usage in RelationshipService | AC#14, AC#26 |
| 6 | Unit tests verify correctness of all relationship functions | AC#4, AC#7, AC#9, AC#10, AC#11, AC#22, AC#24 |
| 7 | Zero debt — no TODO/FIXME/HACK in new code | AC#18, AC#19, AC#20 |
| 8 | Era.Core builds without errors after all changes | AC#23 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

F825 has two distinct work streams: (1) migrating 続柄.ERB's six relationship functions to `RelationshipService : IRelationshipService` in Era.Core's `State/` layer, and (2) completing the Phase 22 DI integration baseline by verifying all 8 Null/Stub registrations, extending DiResolutionTests with IRelationshipService, and documenting the IComableUtilities/ICounterUtilities intentional divergence.

**Migration strategy (stream 1):** Because all six ERB functions are pure computation with no side effects (C1), the entire service is unit-testable via mock IVariableStore and IEngineVariables — no headless game execution required. RelationshipService receives both interfaces via constructor DI, following the sibling pattern from F819-F824. The bitmask return type is `long` throughout (C4, bit 51 = 甥姪). Recursive helpers respect depth limits verbatim from ERB (C9: SONZOKU=6, HIZOKU=8, BOUKEI=1). CHILDREM_CHECK uses `IEngineVariables.GetCharacterNo(int)` to map runtime indices to CSV numbers 148/149 (C2). Remilia/Flan sibling special-case uses `Constants.人物_レミリア = 5` / `Constants.人物_フラン = 6` (C3). Two-level CFLAG dereference (`CFLAG:(CFLAG:X:Father):Father`) is implemented as two sequential calls to `IVariableStore.GetCharacterFlag` using `CharacterFlagIndex.Father` / `CharacterFlagIndex.Mother`.

**DI integration stream (stream 2):** CharacterFlagIndex.MultiBirthSiblingFlag = new(1106) is added to the Clothing System / Family section of CharacterFlagIndex.cs. IRelationshipService is registered as `AddSingleton<IRelationshipService, RelationshipService>()` in ServiceCollectionExtensions. DiResolutionTests gains one new `[Fact]` for `Resolve_IRelationshipService`. Tasks 5-11 are verification-only: grep confirms all 8 Null/Stub class names are present at lines 265-272.

**Interface divergence (stream 3):** IComableUtilities and ICounterUtilities share 3 overlapping methods (`TimeProgress`, `IsAirMaster`, `GetTargetNum`). Of these, `TimeProgress` has identical `int` parameter types in both interfaces; `IsAirMaster` and `GetTargetNum` diverge (raw `int` in IComableUtilities vs strongly-typed `CharacterId` in ICounterUtilities). A comment noting "intentional divergence — COMABLE subsystem uses raw int indices; COUNTER subsystem uses typed CharacterId (2 of 3 overlapping methods diverge)" is added to both interface files. No consolidation is performed (C8).

This approach satisfies all 26 ACs without headless game execution: code structure ACs (1-3, 5-8, 12-14, 16-17) are verified by Grep; unit test ACs (4, 9-11) by dotnet test on RelationshipServiceTests; DI ACs (15, 21) by dotnet test E2E DiResolutionTests; build AC (23) by dotnet build; zero-debt ACs (18-20) by not_matches Grep.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `src/Era.Core/Interfaces/IRelationshipService.cs` with `public interface IRelationshipService` |
| 2 | Create `src/Era.Core/State/RelationshipService.cs` with `: IRelationshipService` declaration |
| 3 | Implement the 6 methods: `OutputAllRelationships`, `CheckRelationship`, `CheckAncestors`, `CheckDescendants`, `CheckCollateral`, `IsChild` in RelationshipService |
| 4 | Create `src/Era.Core.Tests/State/RelationshipServiceTests.cs` with `[Fact]`/`[Theory]` tests covering all 6 functions, depth limits, bitmask, Remilia/Flan |
| 5 | RelationshipService constructor signature: `RelationshipService(IVariableStore variables, IEngineVariables engine)` |
| 6 | Add `services.AddSingleton<IRelationshipService, RelationshipService>()` to ServiceCollectionExtensions |
| 7 | Only one class in Era.Core implements IRelationshipService (RelationshipService); enforced by SSOT design — no NullRelationshipService needed since service is pure computation |
| 8 | IRelationshipService.cs method signatures use `long` for bitmask parameters/return types |
| 9 | RelationshipServiceTests includes a test verifying bit position 51 (甥姪/NieceNephew) in long bitmask |
| 10 | RelationshipServiceTests includes a test that runs SONZOKU (CheckAncestors) at depth 6 and verifies bitmask result |
| 11 | RelationshipServiceTests includes a test that runs HIZOKU (CheckDescendants) at depth 8 and verifies bitmask result |
| 24 | RelationshipServiceTests includes a test that runs BOUKEI (CheckCollateral) at depth 1 and verifies bitmask result |
| 12 | RelationshipService.IsChild calls `_engine.GetCharacterNo(i)` and compares against 148 and 149 |
| 13 | RelationshipService sibling-check branches reference `CharacterId.Remilia` and `CharacterId.Flandre` (or `Constants.人物_レミリア`/`人物_フラン`) |
| 14 | Add `public static readonly CharacterFlagIndex MultiBirthSiblingFlag = new(1106);` to CharacterFlagIndex.cs |
| 15 | Add `Resolve_IRelationshipService` [Fact] to DiResolutionTests.cs |
| 16 | Grep confirms all 8 Null/Stub class names are present in ServiceCollectionExtensions.cs at lines 265-272 — verification only, no new code |
| 17 | Add XML comment with "intentional divergence" / "by design" text to IComableUtilities.cs |
| 25 | Add XML comment with "intentional divergence" / "by design" text to ICounterUtilities.cs |
| 18 | No TODO/FIXME/HACK in RelationshipService.cs — write clean implementation |
| 19 | No TODO/FIXME/HACK in IRelationshipService.cs — write clean interface |
| 20 | No TODO/FIXME/HACK in RelationshipServiceTests.cs — write clean tests |
| 21 | DiResolutionTests all pass — AddSingleton<IRelationshipService, RelationshipService> correctly registered |
| 22 | All RelationshipServiceTests pass — unit test correctness of all 6 functions |
| 23 | Era.Core builds without errors after all above additions |
| 26 | RelationshipService.cs references `MultiBirthSiblingFlag` (used in OutputAllRelationships for 多生児兄弟 labeling per 続柄.ERB:43-45) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Return type for relationship bitmask | `int` (32-bit), `long` (64-bit) | `long` | Bit 51 (甥姪) exceeds int range (C4); `long` prevents silent overflow |
| Null implementation for IRelationshipService | NullRelationshipService returning 0L, no Null class | No Null class | Service is pure computation (no side effects); returning 0L is misleading. Unlike Null stubs for I/O facades, a relationship service that always returns 0 produces incorrect game logic. RegisterService directly to concrete RelationshipService |
| Placement of RelationshipService in file tree | State/, Relationships/ sub-folder, Character/ | State/ (top-level, matching sibling F819-F824) | SleepDepth, MenstrualCycle, HeartbreakService all live in State/ — consistency with Phase 22 sibling layout |
| CHILDREM_CHECK implementation | Direct O(1) check vs O(N) loop | Direct `_engine.GetCharacterNo(targetChar) is 148 or 149` with bounds guard | ERB CHILDREM_CHECK(ARG) takes single character index, checks NO:ARG directly — no loop; `GetCharaNum()` used only for bounds check |
| Two-level CFLAG dereference (grandparent lookup) | Single helper method, inline two-step call | Inline two-step: `GetCharacterFlag(char, Father)` → use result as CharacterId → `GetCharacterFlag(grandparentId, Father)` | Avoids over-abstraction; only used in 2-3 call sites within the service |
| IComableUtilities/ICounterUtilities divergence | Consolidate (breaking), document as intentional, suppress | Document as intentional with XML comment | Consolidation would break 30+ call sites (C8); suppressing divergence hides architectural decision; comment is the minimum viable documentation |
| Test approach for pure-computation service | Headless game, xUnit unit tests with mocks | xUnit unit tests with Mock\<IVariableStore\> and Mock\<IEngineVariables\> | C1 confirms no side effects or PRINT; C12 confirms unit tests are sufficient; no headless mode overhead |

### Interfaces / Data Structures

**IRelationshipService** (new, `Era.Core/Interfaces/IRelationshipService.cs`):

```csharp
public interface IRelationshipService
{
    /// <summary>
    /// Returns all relationship labels between two characters as a formatted string.
    /// Migrates TUDUKIGARA_OUTPUT_ALL from 続柄.ERB (RETURNF LOCALS at line 170).
    /// Pure computation — returns string, no side effects.
    /// </summary>
    string OutputAllRelationships(CharacterId charA, CharacterId charB);

    /// <summary>
    /// Returns a bitmask of relationship types between two characters.
    /// Migrates TUDUKIGARA_CHECK from 続柄.ERB.
    /// Bitmask uses long — bit 51 (甥姪) exceeds int range.
    /// </summary>
    long CheckRelationship(CharacterId charA, CharacterId charB);

    /// <summary>
    /// Checks ancestor (尊属) relationships and returns a bitmask with specific ancestor type bits set.
    /// Migrates SONZOKU_CHECK from 続柄.ERB. Returns long bitmask (SETBIT LOCAL, RETURNF LOCAL).
    /// CheckRelationship combines via LOCAL |= CheckAncestors(...).
    /// Default maxDepth = 6 (matches ERB default parameter).
    /// </summary>
    long CheckAncestors(CharacterId targetChar, CharacterId ancestorChar, int maxDepth = RelationshipTypes.尊属探索_上限);

    /// <summary>
    /// Checks descendant (卑属) relationships and returns a bitmask with specific descendant type bits set.
    /// Migrates HIZOKU_CHECK from 続柄.ERB. Returns long bitmask (SETBIT LOCAL, RETURNF LOCAL).
    /// CheckRelationship combines via LOCAL |= CheckDescendants(...).
    /// Default maxDepth = RelationshipTypes.卑属探索_上限 (8, matches ERB default parameter).
    /// </summary>
    long CheckDescendants(CharacterId targetChar, CharacterId descendantChar, int maxDepth = RelationshipTypes.卑属探索_上限);

    /// <summary>
    /// Checks collateral (傍系) relationships and returns a bitmask with specific collateral type bits set.
    /// Migrates BOUKEI_CHECK from 続柄.ERB. Returns long bitmask (SETBIT LOCAL, RETURNF LOCAL).
    /// CheckRelationship combines via LOCAL |= CheckCollateral(...).
    /// Default maxDepth = RelationshipTypes.傍系探索_上限 (1, matches ERB default parameter — siblings share 1 parent).
    /// </summary>
    long CheckCollateral(CharacterId charA, CharacterId charB, int maxDepth = RelationshipTypes.傍系探索_上限);

    /// <summary>
    /// Checks if targetChar is a child (son=148, daughter=149) by CSV number.
    /// Migrates CHILDREM_CHECK from 続柄.ERB:373-379.
    /// Uses IEngineVariables.GetCharacterNo to map runtime index to CSV number.
    /// </summary>
    bool IsChild(CharacterId targetChar);
}
```

**RelationshipService** (new, `Era.Core/State/RelationshipService.cs`):

Constructor signature:
```csharp
public RelationshipService(IVariableStore variables, IEngineVariables engine)
```

Key implementation notes:
- `_variables.GetCharacterFlag(char, CharacterFlagIndex.Father)` returns `Result<int>`; unwrap with `.Match(v => v, _ => 0)` following established codebase pattern; treat 0 as "no parent" (terminates recursion)
- Two-level grandparent dereference: `int fatherId = _variables.GetCharacterFlag(charId, CharacterFlagIndex.Father).Match(v => v, _ => 0); if (fatherId > 0) { int grandfatherId = _variables.GetCharacterFlag(new CharacterId(fatherId), CharacterFlagIndex.Father).Match(v => v, _ => 0); ... }`
- `IsChild` direct check: `return _engine.GetCharacterNo(targetChar) is 148 or 149;` (with bounds check: `if (targetChar >= _engine.GetCharaNum()) return false;` matching ERB line 375-376). No loop — ERB CHILDREM_CHECK(ARG) is O(1) on a single character index.
- Remilia/Flan sibling special case: compare against `CharacterId.Remilia` and `CharacterId.Flandre` well-known constants (CharacterId.cs:25-26) — these are character indices, not CSV numbers; do NOT use `GetCharacterNo` for this comparison
- Depth-limit recursion: pass depth counter as parameter, return false when depth reaches 0

**CharacterFlagIndex addition** (`Era.Core/Types/CharacterFlagIndex.cs`):
```csharp
// Sibling birth (F825 - Relationships System)
public static readonly CharacterFlagIndex MultiBirthSiblingFlag = new(1106); // 多生児兄弟フラグ
```

**Method Ownership Table** (single interface — MANDATORY check passed: only IRelationshipService is new):

| Method | Owner Interface | Domain Rationale |
|--------|----------------|-----------------|
| OutputAllRelationships | IRelationshipService | Returns formatted relationship label string between two characters — belongs to relationship domain |
| CheckRelationship | IRelationshipService | Bitmask computation between two characters — core relationship query |
| CheckAncestors | IRelationshipService | CFLAG Father/Mother chain traversal upward — returns long bitmask with ancestor type bits |
| CheckDescendants | IRelationshipService | CFLAG Father/Mother chain traversal downward — returns long bitmask with descendant type bits |
| CheckCollateral | IRelationshipService | Common-ancestor detection — returns long bitmask with collateral relationship bits |
| IsChild | IRelationshipService | CSV number check for son/daughter categories — child identity |

No cross-interface dependencies exist: all 6 methods are self-contained within RelationshipService using IVariableStore + IEngineVariables (pre-existing interfaces, not new).

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| [RESOLVED] AC#7 path normalized to `Era.Core/**/` — consistent with AC#1-3 pattern. | AC Definition Table, AC#7 Method | Fixed by orchestrator |
| AC#16 verifies 8 Null registrations with `gte 8` but the actual grep returns the count of matching *lines* not matching *class names*. ServiceCollectionExtensions.cs lines 265-272 each contain exactly one Null class name. Verified: 8 lines × 1 name each = exactly 8 matches. No issue — `gte 8` is satisfied. | Resolved during Technical Design — no upstream action needed | N/A |
| [RESOLVED] `OutputAllRelationships` return type: ERB uses `#FUNCTIONS` + `RETURNF LOCALS` (line 170) = pure string return. Corrected to `string OutputAllRelationships(CharacterId charA, CharacterId charB)`. | Technical Design | Signature corrected in-place by orchestrator |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 14 | Add CharacterFlagIndex.MultiBirthSiblingFlag = new(1106) to CharacterFlagIndex.cs | | [x] |
| 2 | 1, 8, 19 | Create IRelationshipService interface (Era.Core/Interfaces/IRelationshipService.cs) with all 6 method signatures using long bitmask types | | [x] |
| 3 | 2, 3, 5, 12, 13, 18, 26 | Create RelationshipService implementing IRelationshipService (Era.Core/State/RelationshipService.cs) with constructor DI for IVariableStore and IEngineVariables; implement all 6 methods migrated from 続柄.ERB | | [x] |
| 4 | 4, 7, 9, 10, 11, 20, 22, 24 | Create RelationshipServiceTests.cs (Era.Core.Tests/State/) with unit tests covering all 6 functions; include bit-51 long bitmask test, SONZOKU depth-6 test, HIZOKU depth-8 test, BOUKEI depth-1 test, Remilia/Flan special case; minimum 8 [Fact]/[Theory] | | [x] |
| 5 | 6, 15, 21 | Register IRelationshipService in ServiceCollectionExtensions (AddSingleton<IRelationshipService, RelationshipService>) and add Resolve_IRelationshipService [Fact] to DiResolutionTests.cs | | [x] |
| 6 | 16 | Verify all 8 Null/Stub registrations exist in ServiceCollectionExtensions.cs (NullCounterUtilities, NullWcSexHaraService, NullNtrUtilityService, NullShrinkageSystem, NullTrainingCheckService, NullKnickersSystem, NullEjaculationProcessor, NullKojoMessageService) | | [x] |
| 7 | 17, 25 | Add "intentional divergence" XML comment to IComableUtilities.cs and ICounterUtilities.cs documenting that 2 of 3 overlapping methods (IsAirMaster, GetTargetNum) have divergent parameter types while TimeProgress has identical int signatures | | [x] |
| 8 | 23 | Build Era.Core without errors | | [x] |

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

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-825.md Tasks 1-2: CharacterFlagIndex + IRelationshipService | CharacterFlagIndex.cs (1106 added), IRelationshipService.cs (new) |
| 2 | implementer | sonnet | feature-825.md Task 3: RelationshipService implementation | RelationshipService.cs (new, 6 methods) |
| 3 | tester | sonnet | feature-825.md Task 4: RelationshipServiceTests | RelationshipServiceTests.cs (new, ≥8 tests) |
| 4 | implementer | sonnet | feature-825.md Task 5: DI registration + DiResolutionTests extension | ServiceCollectionExtensions.cs (IRelationshipService added), DiResolutionTests.cs (Resolve_IRelationshipService added) |
| 5 | implementer | sonnet | feature-825.md Tasks 6-7: Null stub verification + divergence comments | ServiceCollectionExtensions.cs (verified, no change expected), IComableUtilities.cs + ICounterUtilities.cs (comments added) |
| 6 | tester | sonnet | feature-825.md Task 8: Era.Core build | Build result |

### Pre-conditions

- All predecessors (F814, F819, F821, F822, F823, F824, F813) are [DONE]
- IVariableStore.GetCharacterFlag, IEngineVariables.GetCharacterNo, IEngineVariables.GetCharaNum already exist
- CharacterFlagIndex.Father (73) and CharacterFlagIndex.Mother (74) already exist
- ServiceCollectionExtensions.cs lines 265-272 contain all 8 Null/Stub registrations (verified by F813)

### Execution Order

1. **Task 1 first**: CharacterFlagIndex.MultiBirthSiblingFlag must be added before RelationshipService uses it (if needed)
2. **Task 2**: IRelationshipService interface — defines contract before implementation
3. **Task 3**: RelationshipService — implement against the interface (TDD RED already exists from prior features pattern)
4. **Task 4**: RelationshipServiceTests — write unit tests; all must pass before proceeding
5. **Task 5**: DI wiring — register IRelationshipService and add DiResolutionTests [Fact]
6. **Tasks 6-7**: Verification and documentation — no new logic, grep-verified
7. **Task 8**: Full build verification

### Build Verification

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build'
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test --filter "FullyQualifiedName~RelationshipServiceTests" --blame-hang-timeout 10s'
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test --filter "Category=E2E" --blame-hang-timeout 10s'
```

### Key Implementation Notes

- **Bitmask type**: All bitmask parameters and return types in IRelationshipService and RelationshipService MUST use `long` (64-bit). Bit 51 (甥姪) exceeds `int` range.
- **Recursive depth limits**: SONZOKU (CheckAncestors) default = `RelationshipTypes.尊属探索_上限` (6), HIZOKU (CheckDescendants) = `RelationshipTypes.卑属探索_上限` (8), BOUKEI (CheckCollateral) = `RelationshipTypes.傍系探索_上限` (1). Use constants from `Era.Core/Common/RelationshipTypes.cs:11-13`, not magic numbers.
- **Two-level CFLAG dereference**: `int fatherId = _variables.GetCharacterFlag(charId, CharacterFlagIndex.Father).Match(v => v, _ => 0);` then use as `new CharacterId(fatherId)` for grandparent lookup.
- **CHILDREM_CHECK (IsChild)**: Direct check `_engine.GetCharacterNo(targetChar) is 148 or 149` with bounds guard. No loop — ERB is O(1) on single character index (line 375-379).
- **Remilia/Flan special case**: Compare against `CharacterId.Remilia` and `CharacterId.Flandre` well-known constants (defined in `CharacterId.cs:25-26`) — these are character indices, not CSV numbers. Do NOT use `GetCharacterNo` for this comparison. Do NOT use hardcoded integers. Prefer `CharacterId.Remilia` over `Constants.人物_レミリア`.
- **Task 6 is verification-only**: Do NOT create or modify Null implementations. Only confirm presence via grep.
- **No NullRelationshipService**: RelationshipService is pure computation; no Null stub needed.

### Error Handling

- If `GetCharacterFlag` returns a failure/invalid result for Father/Mother: `.Match(v => v, _ => 0)` yields 0, treated as "no parent" — terminates recursion (return 0L)
- If `GetCharaNum` returns 0: IsChild returns false immediately
- If build fails: STOP and report to user before proceeding

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| ROOM_SMELL_WHOSE_SAMEN (ROOM_SMELL.ERB:1108-1140) | I3DArrayVariables lacks GetDa/SetDa — DA interface gap is cross-cutting | Feature | F826 | N/A (successor exists) | [x] | 追記済み |
| F819: CLOTHES_ACCESSORY requires INtrQuery | NullNtrQuery returns false — deferred from F819 | Feature | F826 | N/A (successor exists) | [x] | 追記済み |
| F821: IEngineVariables indexed methods (GetDay/SetDay/GetTime/SetTime) | Default interface no-op stubs pending engine repo implementation | Feature | F826 | N/A (successor exists) | [x] | 追記済み |

<!-- Transferred + Result columns (F811/F805 Lesson):
- Transferred: [ ] = Not yet written / [x] = Content confirmed in destination (grep verified)
- Result: 作成済み(A), 追記済み(B), 記載済み(C), 確認済み(既存)
- Phase 9.4.1 で転記実行・Result記入。Phase 10.0.2 で検証のみ
- Prevents "Destination filled but content never transferred" gap
-->

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
| 2026-03-05T12:10 | PHASE_START | orchestrator | Phase 1 Initialize | F825 [REVIEWED]->[WIP] |
| 2026-03-05T12:15 | PHASE_DONE | orchestrator | Phase 2 Investigation | All key files verified in Era.Core |
| 2026-03-05T12:35 | PHASE_DONE | orchestrator | Phase 3 TDD RED | 9 FAIL / 4 PASS (RED confirmed) |
| 2026-03-05T13:10 | PHASE_DONE | implementer | Phase 4 TDD GREEN Task 3 | RelationshipService full logic implemented; 13/13 PASS |
| 2026-03-05T13:25 | PHASE_DONE | implementer | Phase 4 Tasks 5,7 | DI registration + divergence comments; 41/41 E2E PASS |
| 2026-03-05T13:25 | VERIFY | orchestrator | Task 6 verification | 8/8 Null/Stub registrations confirmed |
| 2026-03-05T13:30 | Phase 5 | orchestrator | Refactoring review | SKIP (no refactoring needed) |
| 2026-03-05T13:45 | PHASE_DONE | ac-tester | Phase 7 Verification | 26/26 ACs PASS |
| 2026-03-05T14:00 | DEVIATION | feature-reviewer | Phase 8.1 Quality Review | NEEDS_REVISION: GetSiblingLabel multi-birth check bug (aMultiBirth==bMultiBirth should be aMultiBirth==(int)charB) |
| 2026-03-05T14:05 | FIX | orchestrator | Phase 8.1 fix | Fixed multi-birth check + added twin-label test; 14/14 PASS |
| 2026-03-05T14:10 | PHASE_DONE | orchestrator | Phase 8 Post-Review | SSOT updated (INTERFACES.md + PATTERNS.md) |
| 2026-03-05T14:15 | PHASE_DONE | orchestrator | Phase 9 Report & Approval | 26/26 PASS, 1 DEVIATION (D:修正済み), 3 Handoffs→F826 |
| 2026-03-05T14:20 | COMMIT | orchestrator | Phase 10 Commit | core:5bd9588, devkit:c67dd68 |
| 2026-03-05T14:25 | CodeRabbit | 0 findings | - |
<!-- run-phase-1-completed -->
<!-- run-phase-2-completed -->
<!-- run-phase-3-completed -->
<!-- run-phase-4-completed -->
<!-- run-phase-5-completed -->
<!-- run-phase-7-completed -->
<!-- run-phase-8-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: Links section | F801 referenced in body (C8) but missing from Links — added
- [fix] Phase1-RefCheck iter1: Links section | F809 referenced in body (C8) but missing from Links — added
- [fix] Phase1-RefCheck iter1: Related Features table | F801, F809 added for intentional divergence cross-reference
- [fix] Phase2-Review iter1: Improvement Log section | Missing template-required section — added between Review Notes and Links
- [fix] Phase2-Review iter1: Philosophy Derivation row 2 | "All 5 ERB functions" count mismatch — corrected to "All 6 ERB functions"
- [fix] Phase2-Review iter1: AC Definition Table | Missing AC for BOUKEI depth limit 1 (C9 parity) — added AC#24, updated Task 4
- [fix] Phase2-Review iter2: Symptom vs Root Cause | "5 recursive functions" → "6 functions (3 recursive)"
- [fix] Phase2-Review iter2: Technical Design IsChild | O(N) loop misrepresented ERB O(1) semantics — corrected to direct GetCharacterNo check with bounds guard
- [fix] Phase2-Review iter3: Technical Design Remilia/Flan | GetCharacterNo used for index comparison — corrected to direct CharacterId comparison (indices, not CSV numbers)
- [fix] Phase2-Review iter4: Philosophy Derivation | Missing row for "DI integration batch registration and cross-subsystem wiring verification" claim — added with AC#6, AC#15, AC#16, AC#21
- [fix] Phase2-Review iter4: AC#4 Derivation | "5 function groups" unexplained — clarified OutputAllRelationships+CheckRelationship grouping
- [fix] Phase2-Review iter5: IRelationshipService | CheckAncestors/CheckDescendants/CheckCollateral return bool→long — ERB uses SETBIT+RETURNF bitmasks; CheckRelationship needs LOCAL |= results
- [fix] Phase2-Review iter6: Technical Design | Result<T>.IsSuccess/.Value → .Match(v => v, _ => 0) — .Value doesn't exist on Result<T> base class; established codebase pattern
- [fix] Phase2-Review iter7: Key Implementation Notes | fatherResult.Value leftover → .Match(v => v, _ => 0) — consistency with iter6 fix
- [fix] Phase3-Maintainability iter8: IRelationshipService | Magic depth limits 6/8/1 → RelationshipTypes.尊属探索_上限/卑属探索_上限/傍系探索_上限 constants (SSOT)
- [fix] Phase3-Maintainability iter8: AC#13 + design | Constants.人物_レミリア → CharacterId.Remilia/Flandre well-known constants; AC matcher updated
- [fix] Phase3-Maintainability iter8: Mandatory Handoffs | Added WHOSE_SAMEN (F823), CLOTHES_ACCESSORY/INtrQuery (F819), IEngineVariables stubs (F821) → F826
- [fix] Phase4-ACValidation iter9: AC#17 | Pipe-separated paths unsupported — split into AC#17 (IComableUtilities) + AC#25 (ICounterUtilities)
- [fix] Phase2-Review iter1: AC Definition Table | AC#25 out of sequential order (between AC#17 and AC#18) — moved to after AC#24
- [fix] Phase3-Maintainability iter2: IRelationshipService path | Era.Core/State/ → Era.Core/Interfaces/ — sibling convention (F819/F821/F822/F824 all use Interfaces/)
- [fix] Phase3-Maintainability iter2: AC#26 added | MultiBirthSiblingFlag usage in RelationshipService not verified — added AC#26 (Grep for MultiBirthSiblingFlag in RelationshipService.cs), updated Goal Coverage, AC Coverage, Task 3
- [fix] Phase3-Maintainability iter2: C8 divergence scope | 3 methods claimed divergent but TimeProgress has identical int signature — corrected to 2 of 3 divergent (IsAirMaster, GetTargetNum)
- [info] Phase1-DriftChecked: F814 (Predecessor)
- [info] Phase1-DriftChecked: F819 (Predecessor)
- [info] Phase1-DriftChecked: F821 (Predecessor)
- [info] Phase1-DriftChecked: F822 (Predecessor)
- [info] Phase1-DriftChecked: F823 (Predecessor)
- [info] Phase1-DriftChecked: F824 (Predecessor)

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} -> `{target}` or -- {reason} -->

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning
- [Predecessor: F819](feature-819.md) - Clothing System
- [Predecessor: F821](feature-821.md) - Weather System
- [Predecessor: F822](feature-822.md) - Pregnancy System
- [Predecessor: F823](feature-823.md) - Room & Stain System
- [Predecessor: F824](feature-824.md) - Sleep & Menstrual System
- [Related: F813](feature-813.md) - Post-Phase Review Phase 21
- [Related: F801](feature-801.md) - Counter Utilities (ICounterUtilities CharacterId type)
- [Related: F809](feature-809.md) - Comable Utilities (IComableUtilities int type)
- [Successor: F826](feature-826.md) - Post-Phase Review Phase 22
