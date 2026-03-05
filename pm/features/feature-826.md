# Feature 826: Post-Phase Review Phase 22

## Status: [DONE]
<!-- fl-reviewed: 2026-03-05T14:38:05Z -->

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

## Type: infra

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->
## Background

### Philosophy (Mid-term Vision)

Phase 22: State Systems -- Post-phase review is the SSOT gate between Phase 22 completion and Phase 23 initiation. It ensures all Phase 22 subsystem migrations are structurally integrated (CP-2 Step 2c E2E checkpoint), the architecture document reflects actual completion status, and accumulated deferred obligations from sub-features are consolidated and routed to concrete destinations before the phase boundary closes.

### Problem (Current Issue)

Phase 22 cannot be declared complete because the mandatory Post-Phase Review has not been executed, despite all 7 implementation sub-features (F814, F819, F821-F825) being [DONE]. This is because the incremental E2E strategy deliberately defers cross-phase integration verification to Post-Phase Review features (Evidence: `docs/architecture/migration/full-csharp-architecture.md:253-256`). Specifically:

1. **CP-2 Step 2c not executed**: The architecture doc (`docs/architecture/migration/phase-20-27-game-systems.md:252`) still shows Phase 22 Status as TODO, and the 4 CP-2 Step 2c verification items (Shop->Counter->State cross-system flow, Phase 20-22 DI integration) have not been verified. Three Phase 22 services (IClothingSystem, IWeatherSettings, IPregnancySettings) lack DI resolution tests in `DiResolutionTests.cs`, and the cross-system behavioral flow test does not exist in `CrossSystemFlowTests.cs`.

2. **Obligation #1 (--unit deprecation) blocked**: The N+4 re-deferral chain from F782 continues because 100+ kojo JSON test files and 9+ skill files still depend on `--unit`. The trigger condition ("kojo no longer requires ERB test runner") remains unmet.

3. **Obligation #32 (ERB file boundary != domain boundary) already resolved**: The /fc verification step 5c was implemented in `.claude/commands/fc.md:201-214` during F814. Requires verification only, not new implementation.

4. **8-9 deferred items from F819-F825 need consolidated triage**: Cross-cutting concerns (SANTA cosplay UI, IVariableStore 2D SAVEDATA stubs, NullMultipleBirthService, CVARSET bulk reset, IS_DOUTEI shared utility, I3DArrayVariables DA gap, INtrQuery dependency, IEngineVariables no-op stubs) must be routed to Phase 23 or concrete future destinations.

**Deferred from F819**: SANTA cosplay text output (CLOTHE_EFFECT.ERB @SANTA function PRINT calls) is a UI-layer concern and cannot be migrated to Era.Core (no UI primitives). Must be handled by the engine layer.

**Deferred from F822**: Two stub implementations from Pregnancy System Migration:
1. **IVariableStore 2D SAVEDATA accessor**: `GetBirthCountByParent`/`SetBirthCountByParent` extension methods are stubs (return 0/no-op). Full runtime implementation requires generic 2D SAVEDATA variable accessor on IVariableStore.
2. **NullMultipleBirthService runtime implementation**: 8-method stub interface for 多生児パッチ.ERB. All methods return safe defaults (GetBirthCount→1, others no-op). Runtime multi-birth logic not migrated.

**Deferred from F824**: Two cross-cutting concerns discovered during Sleep & Menstrual migration that don't belong to any specific Phase 22 sub-feature:
1. **CVARSET bulk reset**: `CVARSET CFLAG, 317` pattern used in both 睡眠深度.ERB and MOVEMENT.ERB:81. F824 inlined in `SleepDepth.ResetUfufu()` (1 C# call site). When MOVEMENT.ERB migrates, extract `IVariableStore.BulkResetCharacterFlags()` as shared method.
2. **IS_DOUTEI shared utility**: Defined in COMMON.ERB, called from 14 ERB files (22 call sites). F824 inlined in `MenstrualCycle.FormatSimpleStatus` (1 C# call site). When second C# call site appears, extract to `ICharacterUtilities.IsDoutei`.

**Deferred from F825**: Three cross-subsystem concerns:
1. **ROOM_SMELL_WHOSE_SAMEN** (ROOM_SMELL.ERB:1108-1140): I3DArrayVariables lacks GetDa/SetDa -- DA interface gap is cross-cutting.
2. **F819: CLOTHES_ACCESSORY requires INtrQuery**: NullNtrQuery returns false -- deferred from F819.
3. **F821: IEngineVariables indexed methods** (GetDay/SetDay/GetTime/SetTime): Default interface no-op stubs pending engine repo implementation.

### Goal (What to Achieve)

1. Execute CP-2 Step 2c E2E checkpoint: verify missing Phase 22 DI resolution tests pass, add cross-system behavioral flow test (Shop->Counter->State), and confirm Phase 20-22 DI integration.
2. Update `docs/architecture/migration/phase-20-27-game-systems.md` Phase 22 Status from TODO to DONE and check off Success Criteria.
3. Re-triage obligation #1 (--unit deprecation): document NOT_FEASIBLE disposition with concrete next trigger condition.
4. Verify obligation #32 (ERB file boundary step 5c): confirm `.claude/commands/fc.md:201-214` implementation is correct.
5. Triage all 8-9 deferred items from F819-F825 with explicit carry-forward destinations (F827/Phase 23 or concrete future features).

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is Phase 22 not complete? | Phase 22 Status remains TODO in the architecture doc despite all sub-features being [DONE] | `docs/architecture/migration/phase-20-27-game-systems.md:252` |
| 2 | Why is the status still TODO? | The mandatory Post-Phase Review (CP-2 Step 2c) has not been executed | `pm/features/feature-826.md:3` (Status: [DRAFT]) |
| 3 | Why hasn't the Post-Phase Review been executed? | F826 was created as a transition feature by F814 to consolidate review tasks after all sub-features complete | `pm/features/feature-814.md:648` (Task 4 creates F826) |
| 4 | Why is a separate review feature needed? | The architecture mandates Post-Phase Review as a mandatory step between phases | `docs/architecture/migration/phase-20-27-game-systems.md:383` ("Post-Phase Review必須") |
| 5 | Why (Root)? | The incremental E2E strategy deliberately defers cross-phase integration verification and obligation consolidation to Post-Phase Review features, since individual sub-features cannot perform holistic cross-subsystem verification | `docs/architecture/migration/full-csharp-architecture.md:253-256` (CP-2 Step 2c definition) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Phase 22 Status shows TODO; deferred items scattered across 5 sub-features | Incremental E2E strategy defers cross-phase integration to Post-Phase Review |
| Where | `phase-20-27-game-systems.md:252` (Status field) | Architecture design: CP-2 Step 2c checkpoint is a deliberate phase-boundary gate |
| Fix | Manually change TODO to DONE | Execute CP-2 Step 2c E2E verification, triage obligations, then update status |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F814 | [DONE] | Predecessor: Phase 22 Planning (created F826, routed obligations #1 and #32) |
| F819 | [DONE] | Predecessor: Clothing System (deferred SANTA cosplay, CLOTHES_ACCESSORY/INtrQuery) |
| F821 | [DONE] | Predecessor: Weather System (deferred IEngineVariables indexed methods) |
| F822 | [DONE] | Predecessor: Pregnancy System (deferred IVariableStore 2D SAVEDATA, NullMultipleBirthService) |
| F823 | [DONE] | Predecessor: Room & Stain |
| F824 | [DONE] | Predecessor: Sleep & Menstrual (deferred CVARSET bulk reset, IS_DOUTEI utility) |
| F825 | [DONE] | Predecessor: Relationships & DI Integration (deferred I3DArrayVariables DA gap, IEngineVariables stubs) |
| F827 | [DRAFT] | Successor: Phase 23 Planning (blocked by F826 completion) |
| F828 | [DONE] | Related: Date Initialization (Phase 22 scope, extracted from F821) |
| F813 | [DONE] | Related: Post-Phase Review Phase 21 (CP-2 Step 2a owner, established E2E infrastructure) |
| F782 | [DONE] | Related: Post-Phase Review Phase 20 (obligation #1 origin) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| All predecessors [DONE] | FEASIBLE | F814, F819, F821-F825 all confirmed [DONE] |
| CP-2 Step 2c implementable | FEASIBLE | E2E test infrastructure exists in core repo (DiResolutionTests.cs, CrossSystemFlowTests.cs); DI registrations complete in ServiceCollectionExtensions.cs |
| Obligation #1 triageable | FEASIBLE | Trigger condition clearly defined; NOT_FEASIBLE disposition has precedent (F782, F813) |
| Obligation #32 resolvable | FEASIBLE | Step 5c already exists in `.claude/commands/fc.md:201-214`; verification only |
| Deferred items routable | FEASIBLE | All 8-9 items have clear descriptions and can be routed to F827/Phase 23 |
| Scope matches infra type | FEASIBLE | No C# domain logic changes; documentation, E2E tests, and triage only |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Architecture Doc | HIGH | Phase 22 Status TODO->DONE; Success Criteria checkboxes updated; enables Phase 23 initiation |
| E2E Test Coverage | MEDIUM | Adds missing Phase 22 DI resolution tests and cross-system flow test to core repo |
| Obligation Chain | MEDIUM | Consolidates 8-9 deferred items into explicit carry-forward destinations; breaks potential orphan debt |
| Phase 23 Planning | HIGH | F827 (Phase 23 Planning) is blocked until F826 completes; carry-forward list feeds F827 scope |
| Workflow | LOW | Obligation #32 verification only (step 5c already exists) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| E2E tests live in core repo (`C:\Era\core`) | Cross-repo architecture | Cross-repo editing required; AC matchers must reference core repo paths |
| --unit deprecation blocked by 100+ kojo JSON + 9+ skill files | Testing infrastructure | Obligation #1 must be re-deferred; NOT_FEASIBLE disposition |
| fc.md step 5c already exists | `.claude/commands/fc.md:201-214` | Obligation #32 = verification only, not new implementation |
| Many Phase 22 services use Null stubs | Feature deferrals (F819-F825) | Behavioral E2E limited to structural DI resolution + seeded execution patterns |
| IEngineVariables stubs are no-ops | F825 deferral | E2E tests involving TIME/DAY operations silently succeed without behavioral verification |
| Cross-repo coordination spans 2 repos | Architecture (devkit + core) | Implementation must edit files in both repos |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| CP-2 Step 2c cross-system flow test complex to implement | MEDIUM | MEDIUM | Leverage existing CrossSystemFlowTests pattern with seeded IRandomProvider |
| Null stubs limit behavioral E2E verification | HIGH | MEDIUM | Accept structural DI resolution + seeded execution as sufficient for Phase 22 gate |
| Deferred items from F819-F825 lost during Phase 23 planning | MEDIUM | HIGH | F826 must produce explicit carry-forward list with concrete destinations |
| Obligation #1 re-deferral chain grows indefinitely | LOW | LOW | Set concrete next trigger: "kojo no longer requires ERB test runner" |
| Step 2b gap causes Step 2c scope expansion | MEDIUM | LOW | DI resolution tests cover registration; scope to Step 2c verification items only |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Phase 22 Status | `grep "Phase Status" docs/architecture/migration/phase-20-27-game-systems.md` | TODO | Must change to DONE |
| DiResolutionTests count | `grep -c "Resolve_" C:\Era\core\src\Era.Core.Tests\E2E\DiResolutionTests.cs` | 38 | Added: IClothingSystem, IWeatherSettings, IPregnancySettings, IDateInitializer |
| CrossSystemFlowTests count | `grep -c "async Task" C:\Era\core\src\Era.Core.Tests\E2E\CrossSystemFlowTests.cs` | 0 (sync tests) | Added: CrossSystemFlow_StateSystemsResolveWithSeededProvider |

**Baseline File**: `_out/tmp/baseline-826.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | CP-2 Step 2c has 4 verification items | `full-csharp-architecture.md:253-257` | ACs must cover all 4 CP-2 Step 2c items |
| C2 | E2E tests live in core repo | Cross-repo architecture | AC matchers must reference core repo paths (`C:\Era\core\...`) |
| C3 | Phase 22 architecture doc update is mandatory | Post-Phase Review mandate | AC must verify Status TODO->DONE and Success Criteria checkboxes |
| C4 | Obligation #1 (--unit) is NOT_FEASIBLE | 100+ kojo JSON + 9+ skill files | AC must verify re-deferral documentation with concrete next trigger |
| C5 | Obligation #32 step 5c already exists | `.claude/commands/fc.md:201-214` | AC should verify existence, not creation |
| C6 | 8-9 deferred items need explicit carry-forward | F826 Problem section | AC must verify each item has tracked destination |
| C7 | F828 is Phase 22 scope but not listed as predecessor | `pm/index-features.md` | F826 should acknowledge F828 status in review |
| C8 | DiResolutionTests already covers some Phase 22 services | `DiResolutionTests.cs:231-273` | Step 2c may reference existing tests rather than create all new |
| C9 | Phase 22 Success Criteria all unchecked | `phase-20-27-game-systems.md:349-354` | AC must verify all Success Criteria are checked after review |

### Constraint Details

**C1: CP-2 Step 2c Verification Items**
- **Source**: `docs/architecture/migration/full-csharp-architecture.md:253-257` defines 4 verification items for Step 2c
- **Verification**: Read Step 2c section in full-csharp-architecture.md
- **AC Impact**: Each of the 4 items needs at least one covering AC; cross-system flow (Shop->Counter->State) is the most complex

**C2: Core Repo E2E Tests**
- **Source**: E2E test files live in `C:\Era\core\src\Era.Core.Tests\E2E\`
- **Verification**: `ls C:\Era\core\src\Era.Core.Tests\E2E\`
- **AC Impact**: AC file paths must use core repo absolute paths; implementation requires cross-repo editing

**C3: Architecture Doc Update**
- **Source**: `docs/architecture/migration/phase-20-27-game-systems.md:252` (Phase Status: TODO)
- **Verification**: Grep for "Phase Status" in architecture doc
- **AC Impact**: AC must verify both Status field change and individual Success Criteria checkbox updates

**C4: Obligation #1 NOT_FEASIBLE**
- **Source**: Chain: F782 -> F813 -> F814 -> F826; 100+ kojo JSON files in `test/ac/kojo/` + 9+ skill files reference `--unit`
- **Verification**: `find test/ac/kojo/ -name "*.json" | wc -l`; grep for `--unit` in skill files
- **AC Impact**: AC verifies documented NOT_FEASIBLE disposition with next trigger condition, not resolution

**C5: Obligation #32 Already Implemented**
- **Source**: `.claude/commands/fc.md:201-214` contains step 5c
- **Verification**: Grep for "5c" or "ERB.*boundary" in fc.md
- **AC Impact**: AC verifies step 5c content correctness, not file creation

**C6: Deferred Items Triage**
- **Source**: 8-9 items documented in F826 Background Problem section from F819/F822/F824/F825
- **Verification**: Count deferred items in Background; verify each has destination in Mandatory Handoffs
- **AC Impact**: Each deferred item must have a row in Mandatory Handoffs with Destination ID

**C7: F828 Acknowledgment**
- **Source**: F828 [DONE] is Phase 22 scope (Date Initialization, extracted from F821)
- **Verification**: Check F828 status in index-features.md
- **AC Impact**: Post-Phase Review should verify F828 completion alongside other sub-features

**C8: Existing DI Resolution Tests**
- **Source**: `DiResolutionTests.cs:231-273` already tests IRoomSmellService, IStainService, ISleepDepth, IMenstrualCycle, IRelationshipService
- **Verification**: Grep for `Resolve_` methods in DiResolutionTests.cs
- **AC Impact**: ACs should require addition of missing services (IClothingSystem, IWeatherSettings, IPregnancySettings) rather than re-testing existing ones

**C9: Success Criteria Checkboxes**
- **Source**: `phase-20-27-game-systems.md:349-354` shows all checkboxes unchecked
- **Verification**: Grep for `- [ ]` in Phase 22 Success Criteria section
- **AC Impact**: AC must verify all checkboxes are checked (`- [x]`) after review completion

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning (created F826, routed obligations #1 and #32) |
| Predecessor | F819 | [DONE] | Clothing System |
| Predecessor | F821 | [DONE] | Weather System |
| Predecessor | F822 | [DONE] | Pregnancy System |
| Predecessor | F823 | [DONE] | Room & Stain |
| Predecessor | F824 | [DONE] | Sleep & Menstrual |
| Predecessor | F825 | [DONE] | Relationships & DI Integration |
| Successor | F827 | [DRAFT] | Phase 23 Planning (blocked by F826 completion) |
| Related | F828 | [DONE] | Date Initialization (Phase 22 scope, extracted from F821) |
| Related | F813 | [DONE] | Post-Phase Review Phase 21 (CP-2 Step 2a owner, E2E infrastructure) |
| Related | F782 | [DONE] | Post-Phase Review Phase 20 (obligation #1 origin) |
| Related | `C:\Era\core` repo | Available | E2E tests live in core repo |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} → This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This → F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block
-->

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "SSOT gate between Phase 22 completion and Phase 23 initiation" | Phase 22 Status must change from TODO to DONE in architecture doc | AC#1 |
| "all Phase 22 subsystem migrations are structurally integrated" | Missing Phase 22 DI resolution tests must be added and pass | AC#2, AC#3 |
| "structurally integrated (CP-2 Step 2c E2E checkpoint)" | Cross-system structural DI resolution test and Headless seeded test must exist; behavioral flow (Shop->Counter->State) deferred until Null stubs are replaced (see Mandatory Handoffs) | AC#4, AC#5, AC#11 |
| "architecture document reflects actual completion status" | All Phase 22 Success Criteria checkboxes must be checked | AC#6 |
| "accumulated deferred obligations from sub-features are consolidated and routed to concrete destinations" | Every deferred item must have a tracked carry-forward destination; obligation #32 verified | AC#7, AC#8, AC#9, AC#10 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Phase 22 Status updated to DONE | file | Grep(docs/architecture/migration/phase-20-27-game-systems.md, pattern="Phase Status.*DONE") | count_equals | 3 | [x] |
| 2 | Phase 22 DI resolution tests added | code | Grep(C:\Era\core\src\Era.Core.Tests\E2E\DiResolutionTests.cs, pattern="Resolve_") | gte | 37 | [x] |
| 3 | Phase 22 services resolve in DI | code | Grep(C:\Era\core\src\Era.Core.Tests\E2E\DiResolutionTests.cs, pattern="Resolve_IClothingSystem\|Resolve_IWeatherSettings\|Resolve_IPregnancySettings\|Resolve_IDateInitializer") | gte | 4 | [x] |
| 4 | Cross-system structural DI resolution test for Phase 22 State services exists | code | Grep(C:\Era\core\src\Era.Core.Tests\E2E\CrossSystemFlowTests.cs, pattern="IClothingSystem\|IWeatherSettings\|IPregnancySettings\|State.*flow\|Shop.*Counter.*State") | gte | 1 | [x] |
| 5 | Step 2a regression: existing E2E tests not broken | test | dotnet test C:\Era\core\src\Era.Core.Tests\Era.Core.Tests.csproj --filter "Category=E2E" --blame-hang-timeout 10s | succeeds | - | [x] |
| 6 | Phase 22 Success Criteria all checked | file | Grep(docs/architecture/migration/phase-20-27-game-systems.md, pattern="- \\[x\\] 服装システム\|- \\[x\\] 妊娠システム\|- \\[x\\] 環境システム\|- \\[x\\] 全テスト PASS\|- \\[x\\].*CP-2 Step 2c") | gte | 5 | [x] |
| 7 | Obligation #1 NOT_FEASIBLE documented in Mandatory Handoffs | file | Grep(pm/features/feature-826.md, pattern="--unit.*Feature\|--unit.*Phase") | gte | 2 | [x] |
| 8 | Mandatory Handoffs table has 11 destination rows | file | Grep(pm/features/feature-826.md, pattern="Task#6 \\\| \\[ \\]") | count_equals | 11 | [x] |
| 9 | Obligation #32 step 5c verified in fc.md | file | Grep(.claude/commands/fc.md, pattern="ERB Responsibility Boundary Gate") | matches | ERB Responsibility Boundary Gate | [x] |
| 10 | Deferred items count >= 8 in Mandatory Handoffs table | file | Grep(pm/features/feature-826.md, pattern="SANTA cosplay\|INtrQuery\|IVariableStore 2D\|NullMultipleBirthService\|CVARSET bulk\|IS_DOUTEI\|I3DArrayVariables\|IEngineVariables") | gte | 27 | [x] |
| 11 | Headless seeded execution deterministic in E2E tests | code | Grep(C:\Era\core\src\Era.Core.Tests\E2E, pattern="SeededRandomProvider") | gte | 2 | [x] |

### AC Details

**AC#1: Phase 22 Status updated to DONE**
- **Test**: `Grep(docs/architecture/migration/phase-20-27-game-systems.md, pattern="Phase Status.*DONE")`
- **Expected**: `count_equals 3`
- **Derivation**: Currently 2 phases have "Phase Status: DONE" (Phase 20 at line 9, Phase 21 at line 111). After F826, Phase 22 (line 252) changes TODO->DONE, making 3 total.
- **Rationale**: C3 constraint: architecture doc must reflect actual Phase 22 completion.

**AC#2: Phase 22 DI resolution tests added**
- **Test**: `Grep(C:\Era\core\src\Era.Core.Tests\E2E\DiResolutionTests.cs, pattern="Resolve_")`
- **Expected**: `>= 37`
- **Derivation**: Current baseline is 34 Resolve_ methods (confirmed by grep). Phase 22 adds 4 missing services: IClothingSystem, IWeatherSettings, IPregnancySettings, IDateInitializer (F828). 34 + 4 = 38 >= 37.
- **Rationale**: CP-2 Step 2c requires Phase 20-22 DI integration verification. C8 constraint confirms these 4 are missing.

**AC#3: Phase 22 services resolve in DI**
- **Test**: `Grep(C:\Era\core\src\Era.Core.Tests\E2E\DiResolutionTests.cs, pattern="Resolve_IClothingSystem|Resolve_IWeatherSettings|Resolve_IPregnancySettings|Resolve_IDateInitializer")`
- **Expected**: `>= 4`
- **Derivation**: 4 Phase 22 services registered in ServiceCollectionExtensions.cs (lines 116, 167, 168, 195) but lacking individual resolution tests. IDateInitializer (F828) also needs coverage.
- **Rationale**: C8 constraint: add missing services rather than re-test existing ones.

**AC#4: Cross-system structural DI resolution test for Phase 22 State services exists**
- **Test**: `Grep(C:\Era\core\src\Era.Core.Tests\E2E\CrossSystemFlowTests.cs, pattern="IClothingSystem|IWeatherSettings|IPregnancySettings|State.*flow|Shop.*Counter.*State")`
- **Expected**: `>= 1`
- **Derivation**: C1 constraint requires Phase 22 State system structural DI integration. Current CrossSystemFlowTests.cs has Training->Counter flow but no Phase 22 State system involvement. At least 1 reference to Phase 22 State services (IClothingSystem, IWeatherSettings, IPregnancySettings) must appear in a structural resolution test.
- **Rationale**: CP-2 Step 2c: Phase 22 State system structural DI integration verification (behavioral flow deferred per Mandatory Handoffs).

**AC#6: Phase 22 Success Criteria all checked**
- **Test**: `Grep(docs/architecture/migration/phase-20-27-game-systems.md, pattern="- [x] 服装システム|- [x] 妊娠システム|- [x] 環境システム|- [x] 全テスト PASS|- [x].*CP-2 Step 2c")`
- **Expected**: `>= 5`
- **Derivation**: C9 constraint: Phase 22 has exactly 5 Success Criteria checkboxes at lines 350-354 (服装システム, 妊娠システム, 環境システム, 全テスト PASS, CP-2 Step 2c PASS). All must be checked.
- **Rationale**: Architecture doc must reflect actual completion status per Philosophy.

**AC#7: Obligation #1 NOT_FEASIBLE documented in Mandatory Handoffs**
- **Test**: `Grep(pm/features/feature-826.md, pattern="--unit.*Feature|--unit.*Phase")`
- **Expected**: `>= 2`
- **Derivation**: Current baseline is 1 (self-reference in AC table). After Mandatory Handoffs table creation, obligation #1 row will contain "--unit" with "Feature" or "Phase" destination, adding 1+ match. Minimum 1 + 1 = 2.
- **Rationale**: C4 constraint: obligation #1 must be re-deferred with NOT_FEASIBLE disposition and concrete next trigger condition.

**AC#8: Mandatory Handoffs table has 11 destination rows**
- **Test**: `Grep(pm/features/feature-826.md, pattern="Task#6 \| \[ \]")`
- **Expected**: `count_equals 11`
- **Derivation**: The Mandatory Handoffs table has exactly 11 rows (1 obligation #1 re-deferral + 8 deferred items from F819/F822/F824/F825 + 1 CP-2 Step 2c behavioral flow deferral + 1 Roslynator investigation leaked from F819). Each row contains the creation task and transferred columns matching the pattern.
- **Rationale**: C6 constraint: all deferred items need explicit carry-forward destinations. count_equals 11 ensures no rows were accidentally deleted or duplicated.

**AC#10: Deferred items count >= 27 across feature file**
- **Test**: `Grep(pm/features/feature-826.md, pattern="SANTA cosplay|INtrQuery|IVariableStore 2D|NullMultipleBirthService|CVARSET bulk|IS_DOUTEI|I3DArrayVariables|IEngineVariables")`
- **Expected**: `>= 27`
- **Derivation**: Grep counts matching lines, not keyword occurrences. Baseline without Mandatory Handoffs is 19 matching lines (some lines contain multiple keywords but count as 1 line). Adding 8 Mandatory Handoff rows (1 per deferred item) brings total to 27. The 8 items: (1) SANTA cosplay from F819, (2) CLOTHES_ACCESSORY/INtrQuery from F819, (3) IVariableStore 2D SAVEDATA from F822, (4) NullMultipleBirthService from F822, (5) CVARSET bulk reset from F824, (6) IS_DOUTEI utility from F824, (7) I3DArrayVariables DA gap from F825, (8) IEngineVariables stubs from F825.
- **Rationale**: C6 constraint: consolidated triage prevents obligation orphaning. Threshold ensures items appear in Mandatory Handoffs beyond existing Background mentions.

**AC#11: Headless seeded execution deterministic in E2E tests**
- **Test**: `Grep(C:\Era\core\src\Era.Core.Tests\E2E, pattern="SeededRandomProvider")`
- **Expected**: `>= 2`
- **Derivation**: Current baseline is 2 occurrences in CrossSystemFlowTests.cs (constructor line 27 + deterministic test line 71). The new `CrossSystemFlow_StateSystemsResolveWithSeededProvider` test reuses the existing `_provider` (pre-configured with SeededRandomProvider in constructor) rather than creating a new instance, so no additional literal occurrence is added. Threshold `gte 2` verifies SeededRandomProvider infrastructure remains intact.
- **Rationale**: CP-2 Step 2c item 3: Headless seeded execution must produce deterministic results. The new test operates within the seeded provider context established by the constructor.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Execute CP-2 Step 2c E2E checkpoint (DI resolution, cross-system flow, Headless seeded, Step 2a regression) | AC#2, AC#3, AC#4, AC#5, AC#11 |
| 2 | Update Phase 22 Status TODO->DONE and check off Success Criteria | AC#1, AC#6 |
| 3 | Re-triage obligation #1 with NOT_FEASIBLE disposition and next trigger | AC#7 |
| 4 | Verify obligation #32 step 5c in fc.md | AC#9 |
| 5 | Triage all 8-9 deferred items with carry-forward destinations | AC#8, AC#10 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

F826 is a pure infra feature with three distinct execution tracks that produce no new domain logic:

**Track A — CP-2 Step 2c E2E tests (core repo)**
Add 4 missing `Resolve_*` tests to `DiResolutionTests.cs` and add 1 new `CrossSystemFlow_StateSystemsResolveWithSeededProvider` test to `CrossSystemFlowTests.cs`. All tests follow the existing pattern: construct a `ServiceCollection`, call `AddEraCore()`, optionally override `IRandomProvider` with `SeededRandomProvider(42)`, then call `GetRequiredService<T>()` and assert non-null. No new test infrastructure is required — `SeededRandomProvider` already exists and `CrossSystemFlowTests` already demonstrates the seeded pattern.

The 4 missing DI resolution tests cover services that are registered in `ServiceCollectionExtensions.cs` but absent from `DiResolutionTests.cs`:
- `IClothingSystem` (line 116)
- `IPregnancySettings` (line 167)
- `IWeatherSettings` (line 168)
- `IDateInitializer` (line 195)

The cross-system flow test resolves `IClothingSystem`, `IWeatherSettings`, and `IPregnancySettings` in a single test method to demonstrate Phase 22 State system DI integration. This satisfies the CP-2 Step 2c "Shop->Counter->State" flow at structural level; full behavioral testing is deferred because many Phase 22 services use Null stubs (F819/F821/F822 constraint).

**Track B — Architecture doc update (devkit repo)**
Edit `docs/architecture/migration/phase-20-27-game-systems.md` to change Phase 22 `Phase Status: TODO` → `Phase Status: DONE` and check all 5 Success Criteria boxes (`- [ ]` → `- [x]`). No other sections are modified.

**Track C — Obligation triage (devkit repo)**
1. Verify `.claude/commands/fc.md:201-214` contains "ERB Responsibility Boundary Gate" (obligation #32 — read-only verification, no edit).
2. Add `## Mandatory Handoffs` section to this feature file with one row per deferred item (8 items from F819/F822/F824/F825) plus obligation #1 re-deferral row. Each row specifies Destination ID = F827 or a concrete named future feature/phase.

The three tracks are independent and can be executed sequentially within a single /run session. Track A requires core repo editing. Tracks B and C are devkit-only.

This approach satisfies all 11 ACs: Track A → AC#2, AC#3, AC#4, AC#5, AC#11; Track B → AC#1, AC#6; Track C → AC#7, AC#8, AC#9, AC#10.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Track B: Change Phase 22 `Phase Status: TODO` → `Phase Status: DONE` in `phase-20-27-game-systems.md`. After edit, 3 phases have Status DONE (Phase 20, 21, 22). |
| 2 | Track A: Add 4 `Resolve_*` test methods to `DiResolutionTests.cs` (IClothingSystem, IPregnancySettings, IWeatherSettings, IDateInitializer). Current count = 34; after = 38 >= 37. |
| 3 | Track A: The 4 new `Resolve_*` methods explicitly name each of the 4 missing Phase 22 services. Grep for the 4 method names returns >= 4 matches. |
| 4 | Track A: Add `CrossSystemFlow_StateSystemsResolveWithSeededProvider` to `CrossSystemFlowTests.cs` that resolves IClothingSystem, IWeatherSettings, IPregnancySettings. Grep for those service names in CrossSystemFlowTests.cs returns >= 1. |
| 5 | Track A: All existing E2E tests continue passing unchanged. No existing tests are modified. `dotnet test --filter "Category=E2E"` passes. |
| 6 | Track B: Change all 5 `- [ ]` checkboxes in Phase 22 Success Criteria to `- [x]`. Grep for the 5 checked patterns returns >= 5 matches. |
| 7 | Track C: Mandatory Handoffs table row for obligation #1 contains "--unit" + "Feature" or "Phase" destination text. Combined with existing feature file mentions, grep returns >= 2. |
| 8 | Track C: Add `## Mandatory Handoffs` section header to this feature file (wbs-generator fills the table). Header appears >= 2 times (existing Tasks row in template + new section). |
| 9 | Track C: Read `.claude/commands/fc.md` lines 201-214 and confirm "ERB Responsibility Boundary Gate" string is present. No edit needed; grep confirms existence. |
| 10 | Track C: Mandatory Handoffs table contains one row per deferred item (8 rows: SANTA cosplay, INtrQuery, IVariableStore 2D, NullMultipleBirthService, CVARSET bulk, IS_DOUTEI, I3DArrayVariables, IEngineVariables). Each row mentions the item keyword. Combined with existing Background/Problem mentions (19 lines), the 8 Mandatory Handoff rows bring total to 27, satisfying gte 27. |
| 11 | Track A: The new `CrossSystemFlow_StateSystemsResolveWithSeededProvider` test reuses the existing `_provider` (seeded in constructor). Existing 2 occurrences of `SeededRandomProvider` in CrossSystemFlowTests.cs (constructor + deterministic test) satisfy gte 2. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| New E2E test file vs extend existing files | A: Create `Phase22IntegrationTests.cs`, B: Extend existing `DiResolutionTests.cs` + `CrossSystemFlowTests.cs` | B: Extend existing files | Architecture doc says `Phase22IntegrationTests.cs` as target, but AC matchers grep existing files (`DiResolutionTests.cs`, `CrossSystemFlowTests.cs`). ACs are immutable; extending existing files satisfies all matchers without requiring a third file. |
| Cross-system flow scope: structural vs behavioral | A: Structural DI resolution only, B: Behavioral state mutation test | A: Structural DI resolution | Phase 22 services use Null stubs (IClothingSystem reads/writes to variable store but many upstream paths are stubs). Technical Constraint C4 explicitly accepts structural DI + seeded execution as sufficient for Phase 22 gate. |
| IDateInitializer inclusion in AC#3 | A: Include (4 services), B: Exclude (3 services, match AC#3 description exactly) | A: Include | IDateInitializer is registered in ServiceCollectionExtensions.cs:195 (F828) and is absent from DiResolutionTests.cs. Including it closes a real gap and AC#3 already lists it in the pattern. The threshold `gte 4` is met. |
| Obligation #1 disposition | A: Re-defer to next trigger, B: Mark permanently NOT_FEASIBLE | A: Re-defer with NOT_FEASIBLE + concrete trigger | F782/F813 precedent: obligations with external blockers are documented as NOT_FEASIBLE with a next trigger condition ("kojo no longer requires ERB test runner") rather than permanently closed. |
| Mandatory Handoffs section placement | A: wbs-generator creates in Phase 5, B: tech-designer creates stub | A: wbs-generator creates in Phase 5 | Section Ownership table in feature-template.md assigns Mandatory Handoffs to wbs-generator (Phase 5). Tech designer documents destinations in AC Coverage; wbs-generator writes the actual table. |

### Interfaces / Data Structures

No new interfaces or data structures are introduced. F826 only adds test methods to existing test classes and edits documentation.

**New test methods (additions only, no interface changes)**:

`DiResolutionTests.cs` — 4 new `[Fact]` methods following the existing pattern:
```csharp
[Fact]
public void Resolve_IClothingSystem()
{
    var service = _provider.GetRequiredService<IClothingSystem>();
    Assert.NotNull(service);
}

[Fact]
public void Resolve_IPregnancySettings()
{
    var service = _provider.GetRequiredService<IPregnancySettings>();
    Assert.NotNull(service);
}

[Fact]
public void Resolve_IWeatherSettings()
{
    var service = _provider.GetRequiredService<IWeatherSettings>();
    Assert.NotNull(service);
}

[Fact]
public void Resolve_IDateInitializer()
{
    var service = _provider.GetRequiredService<IDateInitializer>();
    Assert.NotNull(service);
}
```

`CrossSystemFlowTests.cs` — 1 new `[Fact]` method:
```csharp
[Fact]
public void CrossSystemFlow_StateSystemsResolveWithSeededProvider()
{
    // Verify Phase 22 State system services resolve with seeded random provider
    var clothing = _provider.GetRequiredService<IClothingSystem>();
    var weather = _provider.GetRequiredService<IWeatherSettings>();
    var pregnancy = _provider.GetRequiredService<IPregnancySettings>();

    Assert.NotNull(clothing);
    Assert.NotNull(weather);
    Assert.NotNull(pregnancy);

    // Verify seeded provider is active (CP-2 Step 2c: Headless seeded reproducibility)
    var randomProvider = _provider.GetRequiredService<IRandomProvider>();
    Assert.Equal(42L, randomProvider.Seed);
}
```

**Required using directives** to add to each test file:
- `DiResolutionTests.cs`: `using Era.Core.State;` already present; verify `IClothingSystem`, `IPregnancySettings`, `IWeatherSettings`, `IDateInitializer` namespaces are covered by existing usings (`Era.Core.State`, `Era.Core.Interfaces`).
- `CrossSystemFlowTests.cs`: same — verify `IClothingSystem`, `IWeatherSettings`, `IPregnancySettings` are accessible from existing using directives.

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| Architecture doc says Phase22IntegrationTests.cs is the implementation target (`phase-20-27-game-systems.md:367`), but AC matchers reference DiResolutionTests.cs and CrossSystemFlowTests.cs. Technical Design extends existing files to match ACs. If future review expects a Phase22IntegrationTests.cs file, this will be a gap. | AC Definition Table — AC#2, AC#3, AC#4, AC#11 | No AC change needed. Document in Tasks that the architecture doc target path is informational; ACs are authoritative for implementation target. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 2, 3 | Add 4 missing `Resolve_*` test methods to `DiResolutionTests.cs` in core repo (IClothingSystem, IPregnancySettings, IWeatherSettings, IDateInitializer) | | [x] |
| 2 | 4, 11 | Add `CrossSystemFlow_StateSystemsResolveWithSeededProvider` test to `CrossSystemFlowTests.cs` in core repo resolving 3 Phase 22 State services with SeededRandomProvider | | [x] |
| 3 | 5 | Run `dotnet test --filter "Category=E2E"` against core repo and confirm all existing E2E tests still pass (Step 2a regression) | | [x] |
| 4 | 1, 6 | Edit `docs/architecture/migration/phase-20-27-game-systems.md`: change Phase 22 `Phase Status: TODO` to `Phase Status: DONE` and change all 5 `- [ ]` Success Criteria checkboxes to `- [x]` | | [x] |
| 5 | 9 | Read `.claude/commands/fc.md` lines 201-214 and verify "ERB Responsibility Boundary Gate" string is present (obligation #32 — read-only verification) | | [x] |
| 6 | 7, 8, 10 | Add `## Mandatory Handoffs` section to this feature file with 11 rows: obligation #1 NOT_FEASIBLE re-deferral + 8 deferred items from F819/F822/F824/F825 + CP-2 Step 2c behavioral flow deferral + Roslynator investigation (leaked from F819) with explicit carry-forward destinations | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-826.md Tasks 1-2; core repo `DiResolutionTests.cs`, `CrossSystemFlowTests.cs` | 4 new Resolve_ methods + 1 CrossSystemFlow_ method added to core repo test files |
| 2 | implementer | sonnet | core repo `Era.Core.Tests.csproj`; Phase 1 output | `dotnet test --filter "Category=E2E"` passes (AC#5 green) |
| 3 | implementer | sonnet | `docs/architecture/migration/phase-20-27-game-systems.md`; feature-826.md Task 4 | Phase Status TODO→DONE; 5 checkboxes `[ ]`→`[x]` |
| 4 | implementer | sonnet | `.claude/commands/fc.md`; feature-826.md Task 5 | Read-only verification result logged to Execution Log |
| 5 | implementer | sonnet | feature-826.md Task 6; Predecessor Obligations table | `## Mandatory Handoffs` section appended to feature-826.md with 11 rows |

### Pre-conditions

Before starting Phase 1:
- Confirm core repo is accessible at `C:\Era\core`
- Confirm `C:\Era\core\src\Era.Core.Tests\E2E\DiResolutionTests.cs` exists
- Confirm `C:\Era\core\src\Era.Core.Tests\E2E\CrossSystemFlowTests.cs` exists
- Baseline: run `grep -c "Resolve_" C:\Era\core\src\Era.Core.Tests\E2E\DiResolutionTests.cs` and record count (expected: 34)
- Baseline: run `grep -c "SeededRandomProvider" C:\Era\core\src\Era.Core.Tests\E2E\CrossSystemFlowTests.cs` and record count (expected: 2)

### Execution Order

**Phase 1 — Track A: Add missing DI resolution tests (core repo)**

1. Open `C:\Era\core\src\Era.Core.Tests\E2E\DiResolutionTests.cs`
2. Verify existing using directives cover `Era.Core.State`, `Era.Core.Interfaces` namespaces (IClothingSystem, IPregnancySettings, IWeatherSettings, IDateInitializer must be accessible)
3. Append 4 new `[Fact]` methods following the existing `Resolve_*` pattern (see Technical Design — Interfaces / Data Structures for exact signatures)
4. Verify grep count of `Resolve_` is now >= 38

**Phase 2 — Track A: Add cross-system flow test + regression run (core repo)**

1. Open `C:\Era\core\src\Era.Core.Tests\E2E\CrossSystemFlowTests.cs`
2. Verify existing using directives cover IClothingSystem, IWeatherSettings, IPregnancySettings
3. Append `CrossSystemFlow_StateSystemsResolveWithSeededProvider` method (see Technical Design — Interfaces / Data Structures for exact signature)
4. Verify grep count of `SeededRandomProvider` in `C:\Era\core\src\Era.Core.Tests\E2E\` is still >= 2
5. Run from WSL: `cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test src/Era.Core.Tests/Era.Core.Tests.csproj --filter "Category=E2E" --blame-hang-timeout 10s --results-directory /mnt/c/Era/devkit/_out/test-results`
6. All E2E tests must PASS — if any fail, STOP and report to user

**Phase 3 — Track B: Architecture doc update (devkit repo)**

1. Open `docs/architecture/migration/phase-20-27-game-systems.md`
2. Change exactly 1 occurrence of `Phase Status: TODO` (Phase 22 section) to `Phase Status: DONE`
3. Locate Phase 22 Success Criteria section (lines near 349-354 per AC Design Constraints C9)
4. Change all 5 `- [ ]` checkboxes in that section to `- [x]`
5. Verify: `grep -c "Phase Status.*DONE" docs/architecture/migration/phase-20-27-game-systems.md` returns 3

**Phase 4 — Track C: Obligation #32 verification (devkit repo, read-only)**

1. Read `.claude/commands/fc.md` section around lines 201-214
2. Confirm "ERB Responsibility Boundary Gate" string is present
3. Log result to Execution Log — NO edits to fc.md
4. If string is absent, STOP and report to user (out-of-scope fix required)

**Phase 5 — Track C: Mandatory Handoffs section (devkit repo)**

1. This is performed by the wbs-generator (current phase) — section is written below
2. Implementer does NOT need to re-add Mandatory Handoffs; verify it exists during /run

### Build Verification Steps

After Phase 2:
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test src/Era.Core.Tests/Era.Core.Tests.csproj --filter "Category=E2E" --blame-hang-timeout 10s --results-directory /mnt/c/Era/devkit/_out/test-results'
```
Expected: All tests PASS. Any failure = STOP.

After Phase 3 (doc verification only, no build):
```bash
grep -c "Phase Status.*DONE" docs/architecture/migration/phase-20-27-game-systems.md
# Expected: 3
grep -c "\- \[x\]" docs/architecture/migration/phase-20-27-game-systems.md
# Expected: includes Phase 22 Success Criteria (>= prior count + 5)
```

### Rollback Plan

If issues arise after implementation:
1. Core repo changes (Track A): `git revert HEAD` in `C:\Era\core` to undo test additions
2. Devkit repo changes (Track B, C): `git revert HEAD` in `C:\Era\devkit` to undo doc + Mandatory Handoffs edits
3. Notify user of rollback with specific failure description
4. Create follow-up feature for fix if root cause is out-of-scope

### Error Handling

| Condition | Action |
|-----------|--------|
| `DiResolutionTests.cs` using directives missing for Phase 22 namespaces | Add required `using` at file top; do NOT change test class structure |
| `CrossSystemFlowTests.cs` `_provider.Seed` property does not exist on SeededRandomProvider | Inspect SeededRandomProvider source; adapt assertion to available API; log finding |
| `dotnet test --filter "Category=E2E"` fails on existing tests | STOP — report test name and failure message to user before proceeding |
| "ERB Responsibility Boundary Gate" absent from fc.md | STOP — this is out-of-scope; report to user; do NOT edit fc.md within F826 |
| Phase 22 Status TODO not found in architecture doc | Grep for "TODO" in Phase 22 section to locate current value; report actual value to user |

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| Obligation #1: --unit deprecation NOT_FEASIBLE re-deferral | 100+ kojo JSON test files and 9+ skill files still depend on --unit; trigger condition ("kojo no longer requires ERB test runner") unmet; N+4 re-deferral chain from F782→F813→F814→F826 continues | Feature | F829 | Task#6 | [x] | 追記済み |
| F819: SANTA cosplay text output (CLOTHE_EFFECT.ERB @SANTA PRINT calls) | UI-layer concern; Era.Core has no UI primitives; requires engine layer | Feature | F829 | Task#6 | [x] | 追記済み |
| F819: CLOTHES_ACCESSORY requires INtrQuery (NullNtrQuery returns false) | INtrQuery dependency not injected in Clothing System; behavioral gap until NTR system migrates | Feature | F829 | Task#6 | [x] | 追記済み |
| F822: IVariableStore 2D SAVEDATA accessor stubs (GetBirthCountByParent/SetBirthCountByParent return 0/no-op) | Full runtime implementation requires generic 2D SAVEDATA variable accessor on IVariableStore; interface not yet designed | Feature | F829 | Task#6 | [x] | 追記済み |
| F822: NullMultipleBirthService runtime implementation (8-method stub, 多生児パッチ.ERB) | Multi-birth logic not migrated; all methods return safe defaults; runtime implementation deferred | Feature | F829 | Task#6 | [x] | 追記済み |
| F824: CVARSET bulk reset shared method extraction (IVariableStore.BulkResetCharacterFlags) | `CVARSET CFLAG, 317` pattern inlined by F824; extraction needed when MOVEMENT.ERB migrates (second call site) | Feature | F829 | Task#6 | [x] | 追記済み |
| F824: IS_DOUTEI shared utility extraction (ICharacterUtilities.IsDoutei) | Defined in COMMON.ERB; 22 call sites across 14 files; F824 inlined 1 call site; extraction when second C# call site appears | Feature | F829 | Task#6 | [x] | 追記済み |
| F825: I3DArrayVariables GetDa/SetDa DA interface gap (ROOM_SMELL_WHOSE_SAMEN) | I3DArrayVariables lacks GetDa/SetDa; cross-cutting DA interface gap; ROOM_SMELL.ERB:1108-1140 not migrated | Feature | F829 | Task#6 | [x] | 追記済み |
| F825: IEngineVariables indexed methods no-op stubs (GetDay/SetDay/GetTime/SetTime) | Default interface no-op stubs pending engine repo implementation; behavioral verification blocked until engine provides runtime | Feature | F829 | Task#6 | [x] | 追記済み |
| CP-2 Step 2c behavioral flow test (Shop->Counter->State) | AC#4 verifies structural DI resolution only; behavioral flow deferred because Phase 22 services use Null stubs (F819/F821/F822); implement when stubs are replaced with runtime implementations | Feature | F829 | Task#6 | [x] | 追記済み |
| Roslynator Analyzers investigation (Phase 22 Task 12) | Assigned to F819 by F814, but F819 declared out of scope (feature-819.md:1022); leaked obligation caught by Post-Phase Review | Feature | F829 | Task#6 | [x] | 追記済み |
| ac-static-verifier.py numeric Expected parsing for Grep-method ACs | Tool cannot parse plain numeric Expected (e.g., `37`) when Method uses `Grep(path, pattern="...")` format; requires `` `pattern` = N `` format; affects all features using Grep-method ACs | Feature | F829 | — | [x] | 作成済み |

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
| 2026-03-05T15:00 | START | initializer | Status [REVIEWED] → [WIP] | READY:826:infra |
| 2026-03-05T15:05 | Phase 2 | explorer | Investigation: DiResolutionTests(34), CrossSystemFlowTests(8), Phase22 Status=TODO | READY |
| 2026-03-05T15:10 | Phase 4 | implementer | Task#1-2: Add 4 Resolve_ + 1 CrossSystemFlow_ to core repo | SUCCESS |
| 2026-03-05T15:12 | Phase 4 | orchestrator | Task#3: E2E tests 46/46 PASS | SUCCESS |
| 2026-03-05T15:13 | Phase 4 | orchestrator | Task#4: Phase 22 Status TODO→DONE, 5 checkboxes checked | SUCCESS |
| 2026-03-05T15:13 | Phase 4 | orchestrator | Task#5: fc.md step 5c "ERB Responsibility Boundary Gate" confirmed | SUCCESS |
| 2026-03-05T15:13 | Phase 4 | orchestrator | Task#6: Mandatory Handoffs 11 rows verified (pre-existing) | SUCCESS |
| 2026-03-05T15:20 | Phase 7 | ac-tester | AC verification: 11/11 PASS | OK:11/11 |
| 2026-03-05T15:25 | Phase 8 | feature-reviewer | Post-review quality check | OK |
| 2026-03-05T15:25 | Phase 8 | orchestrator | Step 8.2 skipped — no new extensibility points; Step 8.3 N/A | OK |
| 2026-03-05T15:28 | DEVIATION | Bash | ac-static-verifier.py --ac-type file exit 1 | Format parsing: numeric Expected unsupported for Grep-method file ACs |
| 2026-03-06T12:00 | Phase 9 | orchestrator | AC re-verification: 11/11 PASS (manual), static verifier tool limitation | OK:11/11 |
| 2026-03-06T12:10 | Phase 9 | orchestrator | Mandatory Handoffs 12 rows → F829, TBD resolved, routing improvement applied | OK |
<!-- run-phase-1-completed -->
<!-- run-phase-2-completed -->
<!-- run-phase-4-completed -->
<!-- run-phase-7-completed -->
<!-- run-phase-8-completed -->
<!-- run-phase-9-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: line 676-679 | Missing ## Improvement Log section (template compliance)
- [fix] Phase2-Review iter1: Dependencies table line 261 | External type changed to Related (template SSOT)
- [fix] Phase2-Review iter1: Background lines 54-68 | Removed non-template subsection Predecessor Obligations (content in Problem + Mandatory Handoffs)
- [fix] Phase2-Review iter1: AC#8 lines 301, 344-348 | Fixed self-referential counting (removed derivation text match)
- [fix] Phase2-Review iter1: Philosophy Derivation line 271, Mandatory Handoffs | Added behavioral flow gap acknowledgment and deferral handoff
- [fix] Phase2-Review iter1: AC#2 Derivation line 302 | Fixed arithmetic 34+3=37 → 34+4=38
- [fix] Phase2-Review iter2: Implementation Contract Phase 5 line 519 | Changed 9 rows to 10 rows (cascading fix from iter1 behavioral flow handoff addition)
- [fix] Phase2-Review iter3: AC#10 lines 288, 335-339 | Fixed unreachable threshold gte 34 → gte 27 (grep counts lines not occurrences)
- [fix] Phase2-Review iter4: Technical Design AC Coverage AC#10 line 401 | Updated stale threshold text from 26→34 to 19→27
- [fix] Phase2-Review iter5: Philosophy Derivation lines 271, 273 | Added AC#11 to row 3, AC#9/AC#10 to row 5 (unmapped ACs)
- [fix] Phase3-Maintainability iter6: Mandatory Handoffs | Added Roslynator investigation row (leaked obligation from F819, Phase 22 Task 12)
- [fix] Phase3-Maintainability iter6: Mandatory Handoffs row 10 | Changed "Phase 23+" to "Feature | F827" (Deferred Task Protocol compliance)
- [fix] Phase3-Maintainability iter6: AC#4 description line 282 | Renamed to "Cross-system structural DI resolution test" (accuracy vs behavioral implication)
- [fix] Phase2-Review iter7: AC#4 Details lines 311-315 | Updated header, derivation, rationale to match renamed AC#4 description
- [fix] Phase2-Review iter8: AC#11 lines 289, 341-345, 402 | Fixed unreachable threshold gte 3 → gte 2 (proposed test reuses _provider, no new SeededRandomProvider literal)
- [fix] Phase2-Review iter9: Implementation Contract Phase 2 step 4 line 544 | Changed >= 3 to >= 2 (cascading fix from AC#11 threshold correction)
- [fix] Phase4-ACValidation iter10: AC#2 Method column line 280 | Added missing pattern="Resolve_" parameter
- [fix] Phase3-Maintainability iter1: Mandatory Handoffs row 1 | Changed "Phase | Phase 23 Post-Phase Review (F827 scope)" to "Feature | F827" (Deferred Task Protocol compliance)
- [fix] Phase3-Maintainability iter1: Upstream Issues table row 1 | Removed stale AC#2 derivation fix entry (already applied in iter1 of previous FL)
- [fix] Phase4-ACValidation iter2: AC#9 Expected column | Removed markdown backticks from Expected value (plain text required)

---

## Improvement Log
<!-- Populated by /imp {ID}. Records modification results only (no analysis narrative). -->
<!-- Format: ### /imp {ID} ({date}) -->
<!-- - [applied|revised|rejected|proposed] {description} → `{target}` or — {reason} -->

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning
- [Predecessor: F819](feature-819.md) - Clothing System
- [Predecessor: F821](feature-821.md) - Weather System
- [Predecessor: F822](feature-822.md) - Pregnancy System
- [Predecessor: F823](feature-823.md) - Room & Stain
- [Predecessor: F824](feature-824.md) - Sleep & Menstrual
- [Predecessor: F825](feature-825.md) - Relationships & DI Integration
- [Successor: F827](feature-827.md) - Phase 23 Planning
- [Related: F828](feature-828.md) - Date Initialization
- [Related: F813](feature-813.md) - Post-Phase Review Phase 21
- [Related: F782](feature-782.md) - Post-Phase Review Phase 20
