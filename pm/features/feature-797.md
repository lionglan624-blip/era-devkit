# Feature 797: UterusVolumeInit/TemperatureToleranceInit delegate-to-IVariableStore Migration

## Status: [DONE]
<!-- fl-reviewed: 2026-02-21T03:23:56Z -->

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

---

## Summary

Migrate UterusVolumeInit (IPregnancySettings) and TemperatureToleranceInit (IWeatherSettings) from Func/Action delegate parameters to IVariableStore-based variable access, following the pattern established by F796's BodyDetailInit migration.

---

## Review Context
<!-- Written by FL POST-LOOP Step 6.3. Review findings only. -->

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F796 |
| Discovery Point | Mandatory Handoffs (F796 /run) |
| Timestamp | 2026-02-18 |

### Identified Gap
GameInitialization.cs has two remaining methods with delegate-based dual-pattern identical to BodyDetailInit (pre-F796): UterusVolumeInit (lines 333-341) creates dummy lambdas for IPregnancySettings, and TemperatureToleranceInit (lines 352-361) creates dummy lambdas for IWeatherSettings. Both use `Func<int,int,int>` / `Action<int,int,int>` parameters instead of IVariableStore.

### Review Evidence
| Field | Value |
|-------|-------|
| Gap Source | F796 Mandatory Handoffs (Leak Prevention) |
| Derived Task | "Migrate UterusVolumeInit and TemperatureToleranceInit delegate parameters to IVariableStore-based variable access" |
| Comparison Result | "Identical dual access pattern: delegates in GameInitialization.cs lines 335-338, 356-358" |
| DEFER Reason | "Out of F796 scope (F796 only migrated BodyDetailInit)" |

### Files Involved
| File | Relevance |
|------|-----------|
| Era.Core/Common/GameInitialization.cs | Contains UterusVolumeInit and TemperatureToleranceInit with dummy lambdas |
| Era.Core/Interfaces/IPregnancySettings.cs | Interface likely defining UterusVolumeInit with delegate params |
| Era.Core/Interfaces/IWeatherSettings.cs | Interface likely defining TemperatureToleranceInit with delegate params |
| Era.Core/State/PregnancySettings.cs | Implementation class for UterusVolumeInit |
| Era.Core/State/WeatherSettings.cs | Implementation class for TemperatureToleranceInit |

### Parent Review Observations
F796 identified UterusVolumeInit and TemperatureToleranceInit as having the identical delegate-based dual-pattern during its maintainability review. These were tracked as Mandatory Handoffs with Option A (new DRAFT) to ensure the debt is not forgotten.

---

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. SSOT: `docs/architecture/migration/full-csharp-architecture.md` Phase 20 section.

### Problem (Current Issue)

UterusVolumeInit (`IPregnancySettings`) and TemperatureToleranceInit (`IWeatherSettings`) use `Func<int,int,int>` / `Action<int,int,int>` delegate parameters for variable access because they were migrated in Phase 3 (F370) before `IVariableStore` supported character-scoped operations (`Era.Core/Interfaces/IPregnancySettings.cs:11-15`, `Era.Core/Interfaces/IWeatherSettings.cs:11-15`). This forces `GameInitialization.cs` to create dummy lambdas returning 0 / no-op at call sites (`Era.Core/Common/GameInitialization.cs:328-356`), producing incorrect runtime results. Additionally, `PregnancySettings.cs:56` contains a latent TALENT index bug -- it reads raw index 2 (Gender/性別) instead of the correct index 100 (BodyType/体型), since raw integer delegates provide no type safety to catch the mismatch (`Game/CSV/Talent.csv:5` index 2=性別, `Game/CSV/Talent.csv:117` index 100=体型).

### Goal (What to Achieve)

1. Replace delegate parameters in `IPregnancySettings.UterusVolumeInit` and `IWeatherSettings.TemperatureResistance` with `IVariableStore`-based variable access, following the F796 pattern (`Era.Core/State/BodySettings.cs:29-34`)
2. Fix the TALENT index bug in `PregnancySettings.UterusVolumeInit` (index 2 to 100) using typed `TalentIndex.BodyType`
3. Add missing typed index constants: 5 `CharacterFlagIndex` entries (UterusVolume=350, HeatResistance=370, ColdResistance=371, Father=73, Mother=74) and 1 `TalentIndex` entry (BodyType=100)
4. Remove dummy lambda creation in `GameInitialization.cs` for both methods
5. Rewrite affected tests to use `MockVariableStore` instead of delegate lambdas

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why do UterusVolumeInit and TemperatureToleranceInit produce incorrect results? | GameInitialization.cs creates dummy lambdas returning 0 / no-op instead of accessing real variable data | `Era.Core/Common/GameInitialization.cs:331-335,350-355` |
| 2 | Why does GameInitialization.cs create dummy lambdas? | IPregnancySettings and IWeatherSettings interfaces require Func/Action delegate parameters for variable access | `Era.Core/Interfaces/IPregnancySettings.cs:11-15`, `Era.Core/Interfaces/IWeatherSettings.cs:11-15` |
| 3 | Why do these interfaces use delegate parameters? | PregnancySettings and WeatherSettings were migrated in Phase 3 (F370) before IVariableStore had character-scoped methods | `Era.Core/State/PregnancySettings.cs:14` (Phase 3 Task 5), `Era.Core/State/WeatherSettings.cs:10-11` (Phase 3) |
| 4 | Why were these not updated when IVariableStore gained character-scoped methods? | F796 only migrated BodyDetailInit, explicitly deferring UterusVolumeInit and TemperatureToleranceInit as out-of-scope | F796 Mandatory Handoffs |
| 5 | Why (Root)? | The incremental migration strategy adds IVariableStore methods phase-by-phase without mandatory back-porting of older methods to the new pattern, accumulating delegate-based access pattern debt | Systemic: Phase 3 to Phase 20 gap |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Dummy lambdas returning 0 / no-op in GameInitialization.cs for UterusVolumeInit and TemperatureToleranceInit | Phase 3 migration used Func/Action delegates before IVariableStore existed for character-scoped operations |
| Where | `Era.Core/Common/GameInitialization.cs:328-356` (call sites) | `Era.Core/Interfaces/IPregnancySettings.cs`, `Era.Core/Interfaces/IWeatherSettings.cs` (interface signatures) |
| Fix | Pass real accessor lambdas instead of dummy ones | Replace delegate parameters with IVariableStore constructor injection in PregnancySettings and WeatherSettings |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F796 | [DONE] | Predecessor -- established the IVariableStore migration pattern for BodyDetailInit; source of F797 handoff |
| F779 | [DONE] | Related -- introduced IVariableStore to BodySettings (Phase 20 origin of the pattern) |
| F789 | [DONE] | Related -- extended IVariableStore with Phase 20 methods; provides needed interface methods |
| F794 | [DONE] | Related -- Shared Body Option Validation Abstraction (Phase 20 sibling); separate scope |
| F780 | [PROPOSED] | Related -- Genetics & Growth (Phase 20 sibling); also needs Father(73) CharacterFlagIndex constant |
| F778 | [DONE] | Related -- test patterns for MockVariableStore |
| F782 | [DRAFT] | Successor -- Post-Phase Review depends on cleanup of remaining dual patterns |
| F783 | [DRAFT] | Successor -- indirect dependency via F782 |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| IVariableStore has GetCharacterFlag/SetCharacterFlag/GetTalent | FEASIBLE | `Era.Core/Interfaces/IVariableStore.cs:28-35` |
| F796 pattern established and proven in BodySettings | FEASIBLE | `Era.Core/State/BodySettings.cs:29-34,489-496` |
| DI already resolves IVariableStore | FEASIBLE | `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs:88` |
| Typed index mechanism exists (readonly record struct) | FEASIBLE | `Era.Core/Types/CharacterFlagIndex.cs`, `Era.Core/Types/TalentIndex.cs` |
| CharacterFlagIndex needs 5 new constants | FEASIBLE | Mechanical addition to existing type |
| TalentIndex needs BodyType(100) | FEASIBLE | Mechanical addition to existing type |
| characterCount sourcing for TemperatureResistance | FEASIBLE | Keep as parameter (delegate-to-IVariableStore scope only) |
| Test rewrite scope bounded (3 test files) | FEASIBLE | `StateSettingsTests.cs`, `HeadlessIntegrationTests.cs`, `GameInitializationTests.cs` |
| Result<int> error handling via .Match() pattern | FEASIBLE | Established pattern from F796 |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| IPregnancySettings interface | HIGH | Breaking change: delegate parameters removed from UterusVolumeInit signature |
| IWeatherSettings interface | HIGH | Breaking change: delegate parameters removed from TemperatureResistance signature |
| PregnancySettings implementation | HIGH | Constructor gains IVariableStore; internal variable access rewritten; TALENT index bug fixed |
| WeatherSettings implementation | HIGH | Constructor gains IVariableStore; internal variable access rewritten |
| GameInitialization.cs | MEDIUM | Dummy lambda creation removed for both methods; simplified call sites |
| CharacterFlagIndex type | LOW | 5 new named constants added |
| TalentIndex type | LOW | 1 new named constant added (BodyType=100) |
| DI registration | LOW | ServiceCollectionExtensions.cs auto-resolves IVariableStore for new constructors |
| engine.Tests | MEDIUM | 15+ tests rewritten from delegate lambdas to MockVariableStore |
| Runtime behavior | MEDIUM | TALENT index fix changes UterusVolumeInit body type lookup from Gender(2) to BodyType(100) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Public interface breaking change | IPregnancySettings/IWeatherSettings | All callers must be updated atomically |
| TreatWarningsAsErrors | Directory.Build.props | Unused parameters/imports cause build failure |
| TALENT index 2 is incorrect (should be 100) | PregnancySettings.cs:56 vs Talent.csv:117 | Must fix as part of migration (bug fix changes runtime behavior) |
| characterCount for TemperatureResistance | GameInitialization.cs:350 | Keep as parameter; IVariableStore lacks CHARANUM; separate concern |
| Result<int> error handling | IVariableStore.cs | Must use pattern matching (.Match() or is Success s) for value extraction |
| No-arg constructors currently | PregnancySettings/WeatherSettings | Adding IVariableStore requires updating all test constructors + DI registration |
| WeatherSettings uses System.Random | IWeatherSettings.cs:15 | Keep as constructor-injected or method parameter for testability |
| Existing tests encode wrong TALENT index | StateSettingsTests.cs | Tests must be corrected alongside implementation |
| NtrInitialization also has delegate pattern | GameInitialization.cs:369-376 | Out of scope; track separately |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| TALENT bug fix changes runtime behavior | HIGH | MEDIUM | Document as intentional correction; all characters previously got default 5000 volume due to wrong index |
| characterCount design decision delays | MEDIUM | LOW | Keep hardcoded 10 as parameter (delegate-to-IVariableStore scope only; characterCount resolution is separate concern) |
| System.Random handling decision | LOW | LOW | Keep as constructor-injected or method parameter; not a variable store concern |
| Test rewrite introduces regressions | LOW | MEDIUM | Reuse MockVariableStore pattern from F796; pin ground-truth values |
| CharacterFlagIndex conflict with F780 | LOW | LOW | F797 adds constants first; F780 is only [PROPOSED] |
| Hardcoded characterCount=10 masks issues | MEDIUM | LOW | Out of scope; separate concern from delegate-to-IVariableStore migration |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| Delegate params in IPregnancySettings | Grep for `Func<int,int,int>` in IPregnancySettings.cs | Present | To be removed |
| Delegate params in IWeatherSettings | Grep for `Func<int,int,int>` in IWeatherSettings.cs | Present | To be removed |
| Dummy lambdas in GameInitialization.cs | Grep for `=> 0` in GameInitialization.cs | 6+ matches (UterusVolumeInit + TemperatureToleranceInit) | To be removed |
| TALENT index in PregnancySettings.cs | Grep for `getTalent(characterId, 2)` in PregnancySettings.cs | Present (incorrect) | To be fixed to index 100 |
| Raw int indices in WeatherSettings.cs | Grep for raw indices 370, 371, 73, 74 | Present | To be replaced with typed constants |
| Unit test count | dotnet test engine.Tests --filter "UterusVolume\|TemperatureResistance" | 15+ tests using delegates | To be rewritten with MockVariableStore |

**Baseline File**: `.tmp/baseline-797.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | IPregnancySettings must NOT contain Func/Action delegate parameters | Feature scope | Verify interface signature has no delegate params |
| C2 | IWeatherSettings must NOT contain Func/Action delegate parameters | Feature scope | Verify interface signature has no delegate params |
| C3 | PregnancySettings must accept IVariableStore via constructor | F796 pattern | Verify constructor injection |
| C4 | WeatherSettings must accept IVariableStore via constructor | F796 pattern | Verify constructor injection |
| C5 | TALENT must use index 100 (BodyType), not 2 (Gender) | Bug fix: PregnancySettings.cs:56 vs Talent.csv:117 | Verify TalentIndex.BodyType usage |
| C6 | All raw int CFLAG indices must use typed CharacterFlagIndex constants | Type safety | Verify no raw int indices remain in PregnancySettings/WeatherSettings |
| C7 | GameInitialization must not create dummy lambdas for these methods | TODO cleanup | Verify removal of dummy lambda creation |
| C8 | Business logic values must be preserved (except TALENT index fix) | Equivalence | Verify same CFLAG mappings and thresholds |
| C9 | characterCount kept as parameter for TemperatureResistance | Design decision | AC must not require IVariableStore-based character count |
| C10 | System.Random testability preserved | Testing | RNG must remain injectable/deterministic in tests |
| C11 | ArchitectureTests must pass | DI validation | DI registration compatible with IVariableStore injection |
| C12 | CharacterFlagIndex must include 5 new constants (73, 74, 350, 370, 371) | Interface Dependency Scan | Verify constants exist with correct values |
| C13 | TalentIndex must include BodyType(100) | Interface Dependency Scan | Verify constant exists with correct value |

### Constraint Details

**C1: No Delegate Parameters in IPregnancySettings**
- **Source**: Investigation of `Era.Core/Interfaces/IPregnancySettings.cs:11-15` showing `Func<int,int,int>` and `Action<int,int,int>` parameters
- **Verification**: Grep IPregnancySettings.cs for `Func<` and `Action<` -- should return 0 matches after migration
- **AC Impact**: AC must verify interface signature uses only `int characterId` parameter (no delegates)

**C2: No Delegate Parameters in IWeatherSettings**
- **Source**: Investigation of `Era.Core/Interfaces/IWeatherSettings.cs:11-15` showing delegate + characterCount + rng parameters
- **Verification**: Grep IWeatherSettings.cs for `Func<` and `Action<` -- should return 0 matches after migration
- **AC Impact**: AC must verify interface signature; characterCount and Random may remain as parameters

**C3: IVariableStore Constructor in PregnancySettings**
- **Source**: F796 pattern in `Era.Core/State/BodySettings.cs:29-34`
- **Verification**: Grep PregnancySettings.cs for `IVariableStore` field declaration
- **AC Impact**: AC must verify `_variables` field and constructor parameter

**C4: IVariableStore Constructor in WeatherSettings**
- **Source**: F796 pattern in `Era.Core/State/BodySettings.cs:29-34`
- **Verification**: Grep WeatherSettings.cs for `IVariableStore` field declaration
- **AC Impact**: AC must verify `_variables` field and constructor parameter

**C5: TALENT Index Fix (2 to 100)**
- **Source**: `Era.Core/State/PregnancySettings.cs:56` uses `getTalent(characterId, 2)` but `Game/CSV/Talent.csv:117` defines 体型 at index 100, not 2 (which is 性別/Gender per Talent.csv:5)
- **Verification**: Grep PregnancySettings.cs for `TalentIndex.BodyType` -- should match; grep for raw `2` as talent index -- should not match
- **AC Impact**: AC must verify typed TalentIndex.BodyType usage and absence of raw index 2

**C6: Typed CharacterFlagIndex Usage**
- **Source**: `Era.Core/State/WeatherSettings.cs:70-71` uses raw indices 370, 371; lines 77-78 use 73, 74. PregnancySettings uses raw 350
- **Verification**: Grep for `CharacterFlagIndex.` in both implementation files
- **AC Impact**: AC must verify no raw integer CFLAG indices remain in PregnancySettings and WeatherSettings

**C7: No Dummy Lambdas in GameInitialization**
- **Source**: `Era.Core/Common/GameInitialization.cs:331-335,350-355` creates dummy lambdas
- **Verification**: Grep GameInitialization.cs for UterusVolumeInit/TemperatureToleranceInit call sites -- no lambda expressions
- **AC Impact**: AC must verify simplified call sites without delegate creation

**C8: Business Logic Equivalence**
- **Source**: All 3 investigations confirmed same CFLAG/TALENT mappings
- **Verification**: Test values match ERB source thresholds
- **AC Impact**: Rewritten tests must verify same threshold values and logic branches (except corrected TALENT index)

**C9: characterCount as Parameter**
- **Source**: Design decision from consensus (3/3 explorers recommend keeping as param)
- **Verification**: IWeatherSettings.TemperatureResistance signature includes characterCount parameter
- **AC Impact**: AC must not require IVariableStore-based character count resolution

**C10: System.Random Testability**
- **Source**: `Era.Core/State/WeatherSettings.cs:59` uses System.Random
- **Verification**: Tests can inject deterministic Random(seed) for reproducible results
- **AC Impact**: RNG must remain injectable; no IRandomProvider required

**C11: DI Compatibility**
- **Source**: `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs:150-151` current no-arg registration
- **Verification**: `dotnet test` with ArchitectureTests passes
- **AC Impact**: DI registration must resolve IVariableStore for both constructors

**C12: CharacterFlagIndex Constants**
- **Source**: Interface Dependency Scan across all 3 investigations
- **Verification**: Grep CharacterFlagIndex.cs for each constant name and value
- **AC Impact**: Verify 5 named constants exist: Father(73), Mother(74), UterusVolume(350), HeatResistance(370), ColdResistance(371)

**C13: TalentIndex BodyType Constant**
- **Source**: Interface Dependency Scan across all 3 investigations
- **Verification**: Grep TalentIndex.cs for `BodyType` with value 100
- **AC Impact**: Verify named constant exists: BodyType(100)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F796 | [DONE] | Established the IVariableStore migration pattern for BodyDetailInit; source of F797 handoff |
| Related | F779 | [DONE] | Introduced IVariableStore to BodySettings (Phase 20 origin) |
| Related | F789 | [DONE] | Extended IVariableStore with Phase 20 methods |
| Related | F780 | [PROPOSED] | Genetics & Growth; also needs Father(73) CharacterFlagIndex constant |
| Successor | F782 | [DRAFT] | Post-Phase Review depends on cleanup of remaining dual patterns |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} → This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This → F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
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
| "ensures continuous development pipeline" | Delegate-to-IVariableStore migration must complete for both PregnancySettings and WeatherSettings, eliminating Phase 3 debt | AC#1, AC#2, AC#3, AC#4, AC#7 |
| "clear phase boundaries" | Each migrated method must use typed indices (CharacterFlagIndex, TalentIndex) matching the Phase 20 pattern, not raw integers — typed constants define the boundary between Phase 3 raw-integer access and Phase 20 type-safe access | AC#5, AC#6, AC#8, AC#9, AC#11, AC#12, AC#17, AC#18 |
| "documented transition points" | Migration correctness must be verified through ground-truth test pinning, documenting the corrected behavior at phase transition boundary | AC#13, AC#16, AC#19, AC#22, AC#23 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IPregnancySettings has no Func/Action delegate parameters | code | Grep(Era.Core/Interfaces/IPregnancySettings.cs) | not_matches | `Func<|Action<` | [x] |
| 2 | IWeatherSettings has no Func/Action delegate parameters | code | Grep(Era.Core/Interfaces/IWeatherSettings.cs) | not_matches | `Func<|Action<` | [x] |
| 3 | PregnancySettings accepts IVariableStore via constructor | code | Grep(Era.Core/State/PregnancySettings.cs) | matches | `private readonly IVariableStore _variables` | [x] |
| 4 | WeatherSettings accepts IVariableStore via constructor | code | Grep(Era.Core/State/WeatherSettings.cs) | matches | `private readonly IVariableStore _variables` | [x] |
| 5 | PregnancySettings uses TalentIndex.BodyType (not raw index 2) | code | Grep(Era.Core/State/PregnancySettings.cs) | matches | `TalentIndex\.BodyType` | [x] |
| 6 | PregnancySettings uses CharacterFlagIndex.UterusVolume (not raw 350) | code | Grep(Era.Core/State/PregnancySettings.cs) | matches | `CharacterFlagIndex\.UterusVolume` | [x] |
| 7 | GameInitialization.cs has no dummy lambdas for UterusVolumeInit/TemperatureToleranceInit | code | Grep(Era.Core/Common/GameInitialization.cs) | not_matches | `getCflag = \(|setCflag = \(|getTalent = \(` | [x] |
| 8 | CharacterFlagIndex has 5 new constants with correct values | code | Grep(Era.Core/Types/CharacterFlagIndex.cs) | count_equals | `Father = new\(73\)|Mother = new\(74\)|UterusVolume = new\(350\)|HeatResistance = new\(370\)|ColdResistance = new\(371\)` = 5 | [x] |
| 9 | TalentIndex has BodyType constant with value 100 | code | Grep(Era.Core/Types/TalentIndex.cs) | matches | `BodyType = new\(100\)` | [x] |
| 10 | WeatherSettings.TemperatureResistance keeps characterCount as parameter | code | Grep(Era.Core/State/WeatherSettings.cs) | matches | `TemperatureResistance\(.*int characterCount` | [x] |
| 11 | WeatherSettings uses typed CharacterFlagIndex.HeatResistance | code | Grep(Era.Core/State/WeatherSettings.cs) | matches | `CharacterFlagIndex\.HeatResistance` | [x] |
| 12 | WeatherSettings uses typed CharacterFlagIndex.Father | code | Grep(Era.Core/State/WeatherSettings.cs) | matches | `CharacterFlagIndex\.Father` | [x] |
| 13 | Unit tests pin ground-truth uterus volume 5000 for body type 0 | test | Grep(engine.Tests/Tests/StateSettingsTests.cs) | matches | `Assert.*5000.*UterusVolume\|Assert.*UterusVolume.*5000` | [x] |
| 14 | All unit tests pass | test | dotnet test Era.Core.Tests/ engine.Tests/ | succeeds | - | [x] |
| 15 | No raw integer CFLAG/TALENT indices remain in PregnancySettings or WeatherSettings | code | Grep(Era.Core/State/PregnancySettings.cs,Era.Core/State/WeatherSettings.cs) | not_matches | `, 2\)|, 350[,\)]|, 73\)|, 74\)|, 370[,\)]|, 371[,\)]` | [x] |
| 16 | Unit tests pin ground-truth uterus volume 1500 for body type -3 (shota) | test | Grep(engine.Tests/Tests/StateSettingsTests.cs) | matches | `Assert.*1500.*UterusVolume\|Assert.*UterusVolume.*1500` | [x] |
| 17 | WeatherSettings uses typed CharacterFlagIndex.Mother | code | Grep(Era.Core/State/WeatherSettings.cs) | matches | `CharacterFlagIndex\.Mother` | [x] |
| 18 | WeatherSettings uses typed CharacterFlagIndex.ColdResistance | code | Grep(Era.Core/State/WeatherSettings.cs) | matches | `CharacterFlagIndex\.ColdResistance` | [x] |
| 19 | Tests use TalentIndex.BodyType for UterusVolumeInit test seeding | test | Grep(engine.Tests/Tests/StateSettingsTests.cs) | matches | `TalentIndex\.BodyType` | [x] |
| 20 | Tests construct PregnancySettings with MockVariableStore | test | Grep(engine.Tests/Tests/StateSettingsTests.cs) | matches | `new PregnancySettings\(_mockStore\)` | [x] |
| 21 | Tests construct WeatherSettings with MockVariableStore | test | Grep(engine.Tests/Tests/StateSettingsTests.cs) | matches | `new WeatherSettings\(_mockStore\)` | [x] |
| 22 | Unit tests pin ground-truth HeatResistance value 30 for character 0 | test | Grep(engine.Tests/Tests/StateSettingsTests.cs) | matches | `Assert.*30.*HeatResistance\|Assert.*HeatResistance.*30` | [x] |
| 23 | Unit tests pin ground-truth ColdResistance value 10 for character 0 | test | Grep(engine.Tests/Tests/StateSettingsTests.cs) | matches | `Assert.*10.*ColdResistance\|Assert.*ColdResistance.*10` | [x] |

### AC Details

**AC#1: IPregnancySettings has no Func/Action delegate parameters**
- **Test**: Grep pattern=`Func<|Action<` path=`Era.Core/Interfaces/IPregnancySettings.cs`
- **Expected**: 0 matches (all delegate parameters removed)
- **Rationale**: Constraint C1. The interface must use only `int characterId` parameter after migration, matching F796 IBodySettings pattern. Verifies delegate removal is complete.

**AC#2: IWeatherSettings has no Func/Action delegate parameters**
- **Test**: Grep pattern=`Func<|Action<` path=`Era.Core/Interfaces/IWeatherSettings.cs`
- **Expected**: 0 matches (all delegate parameters removed)
- **Rationale**: Constraint C2. The interface must use `int characterCount` and `System.Random rng` parameters only (no delegate parameters). Constraint C9 allows characterCount and C10 allows Random to remain.

**AC#3: PregnancySettings accepts IVariableStore via constructor**
- **Test**: Grep pattern=`private readonly IVariableStore _variables` path=`Era.Core/State/PregnancySettings.cs`
- **Expected**: 1 match
- **Rationale**: Constraint C3. Following F796 BodySettings pattern (`Era.Core/State/BodySettings.cs:29`), PregnancySettings must store IVariableStore as a private readonly field injected via constructor. DI auto-resolves IVariableStore (C11).
- **Verification Chain**: AC#3 verifies field existence. AC#5 (TalentIndex.BodyType) and AC#6 (CharacterFlagIndex.UterusVolume) verify typed constant usage in the private helpers (GetTalent/SetCFlag), which call `_variables` methods. Together, AC#3+AC#5+AC#6 verify field exists AND is actively used.

**AC#4: WeatherSettings accepts IVariableStore via constructor**
- **Test**: Grep pattern=`private readonly IVariableStore _variables` path=`Era.Core/State/WeatherSettings.cs`
- **Expected**: 1 match
- **Rationale**: Constraint C4. Same F796 pattern as AC#3. WeatherSettings must store IVariableStore for GetCharacterFlag/SetCharacterFlag access.
- **Verification Chain**: AC#4 verifies field existence. AC#11 (HeatResistance), AC#12 (Father), AC#17 (Mother), AC#18 (ColdResistance) verify typed constant usage in the private helpers (GetCFlag/SetCFlag), which call `_variables` methods. Together, AC#4+AC#11+AC#12+AC#17+AC#18 verify field exists AND is actively used.

**AC#5: PregnancySettings uses TalentIndex.BodyType (not raw index 2)**
- **Test**: Grep pattern=`TalentIndex\.BodyType` path=`Era.Core/State/PregnancySettings.cs`
- **Expected**: 1+ matches
- **Rationale**: Constraint C5. Fixes the TALENT index bug: `PregnancySettings.cs:56` currently reads index 2 (Gender/性別) instead of 100 (BodyType/体型). The typed constant provides compile-time safety against future index mismatches.

**AC#6: PregnancySettings uses CharacterFlagIndex.UterusVolume (not raw 350)**
- **Test**: Grep pattern=`CharacterFlagIndex\.UterusVolume` path=`Era.Core/State/PregnancySettings.cs`
- **Expected**: 1+ matches
- **Rationale**: Constraint C6. Raw integer 350 must be replaced with typed CharacterFlagIndex.UterusVolume for type safety.

**AC#7: GameInitialization.cs has no dummy lambdas for UterusVolumeInit/TemperatureToleranceInit**
- **Test**: Grep pattern=`getCflag = \(|setCflag = \(|getTalent = \(` path=`Era.Core/Common/GameInitialization.cs`
- **Expected**: 0 matches (currently 6+ matches across lines 331-355 covering getCflag, setCflag, and getTalent dummy lambda assignments in both UterusVolumeInit and TemperatureToleranceInit -- all removed after migration)
- **Rationale**: Constraint C7. After migration, GameInitialization calls UterusVolumeInit and TemperatureToleranceInit without creating dummy lambdas. The pattern covers all three delegate types: `getCflag = (id, idx) => 0`, `setCflag = (id, idx, val) => { }`, and `getTalent = (id, idx) => 0`. NtrSetStayoutMaximum at line 369+ receives delegates as parameters (not via local variable assignment), so it does not match this pattern.

**AC#8: CharacterFlagIndex has 5 new constants with correct values**
- **Test**: Grep pattern=`Father = new\(73\)|Mother = new\(74\)|UterusVolume = new\(350\)|HeatResistance = new\(370\)|ColdResistance = new\(371\)` path=`Era.Core/Types/CharacterFlagIndex.cs` | count
- **Expected**: 5 matches (one for each constant with its verified value)
- **Rationale**: Constraint C12. Value-binding pattern verifies both constant name AND correct CFLAG index value. Prevents false positives from comments or wrong values.

**AC#9: TalentIndex has BodyType constant with value 100**
- **Test**: Grep pattern=`BodyType = new\(100\)` path=`Era.Core/Types/TalentIndex.cs`
- **Expected**: 1 match
- **Rationale**: Constraint C13. TalentIndex.BodyType(100) is required by PregnancySettings for the corrected TALENT lookup (was raw index 2, should be 100).

**AC#10: WeatherSettings.TemperatureResistance keeps characterCount as parameter**
- **Test**: Grep pattern=`TemperatureResistance\(.*int characterCount` path=`Era.Core/State/WeatherSettings.cs`
- **Expected**: 1 match
- **Rationale**: Constraint C9. characterCount must remain as a method parameter (not resolved via IVariableStore). IVariableStore lacks CHARANUM; this is a separate concern from delegate-to-IVariableStore migration.

**AC#11: WeatherSettings uses typed CharacterFlagIndex.HeatResistance**
- **Test**: Grep pattern=`CharacterFlagIndex\.HeatResistance` path=`Era.Core/State/WeatherSettings.cs`
- **Expected**: 1+ matches
- **Rationale**: Constraint C6. Raw integer 370 must be replaced with typed CharacterFlagIndex.HeatResistance. Split from AC#12 per Issue 71 (alternation allows partial verification).

**AC#12: WeatherSettings uses typed CharacterFlagIndex.Father**
- **Test**: Grep pattern=`CharacterFlagIndex\.Father` path=`Era.Core/State/WeatherSettings.cs`
- **Expected**: 1+ matches
- **Rationale**: Constraint C6. Raw integer 73 must be replaced with typed CharacterFlagIndex.Father. Split from AC#11 per Issue 71 to verify father/mother indices independently from heat/cold.

**AC#13: Unit tests pin ground-truth uterus volume 5000 for body type 0**
- **Test**: Grep pattern=`Assert.*5000.*UterusVolume|Assert.*UterusVolume.*5000` path=`engine.Tests/Tests/StateSettingsTests.cs`
- **Expected**: 1+ matches — an assertion line (anchored by `Assert`) where both `5000` and `UterusVolume` (CharacterFlagIndex) co-occur, verifying the assertion targets the correct flag with the correct value
- **Rationale**: Constraint C8 (business logic equivalence). Pattern requires `Assert` prefix plus both the expected value (5000) and the CharacterFlagIndex name (UterusVolume) on the same line, targeting assertion lines like `Assert.Equal(5000, _mockStore.GetCFlag(0, CharacterFlagIndex.UterusVolume))`. The `Assert` anchor eliminates false positives from comments or variable declarations. Combined with AC#19 (TalentIndex.BodyType seeding) and AC#14 (all tests pass), this ensures: (1) an assertion for body type 0 → 5000 exists with correct flag reference, (2) it seeds the correct TALENT index, (3) it actually passes at runtime.
- **Verification Unit**: AC#13 is a necessary but not sufficient condition. The ground-truth verification unit is AC#13 + AC#19 + AC#14: AC#13 verifies an assertion line with value+flag exists, AC#19 verifies correct index seeding, AC#14 verifies runtime correctness.

**AC#14: All unit tests pass**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ && /home/siihe/.dotnet/dotnet test engine.Tests/'`
- **Expected**: Exit code 0 (all tests pass)
- **Rationale**: Constraint C11 (ArchitectureTests must pass) + Constraint C8 (business logic equivalence). This covers DI registration compatibility, rewritten MockVariableStore tests, and all existing tests still passing. Includes System.Random testability (C10) via deterministic Random(seed) in tests.

**AC#15: No raw integer CFLAG/TALENT indices remain in PregnancySettings or WeatherSettings**
- **Test**: Grep pattern=`, 2\)|, 350[,\)]|, 73\)|, 74\)|, 370[,\)]|, 371[,\)]` path=`Era.Core/State/PregnancySettings.cs,Era.Core/State/WeatherSettings.cs`
- **Expected**: 0 matches (currently 12 matches total: 2 in PregnancySettings, 10 in WeatherSettings -- all replaced with typed constants after migration)
- **Rationale**: Constraint C6. Comprehensive negative check targeting raw integer indices in method argument position (comma-prefixed): 2(Gender->BodyType), 350(UterusVolume), 73(Father), 74(Mother), 370(HeatResistance), 371(ColdResistance). Variable-name-independent: matches `, {raw_int})` or `, {raw_int},` regardless of the caller variable name. Complements the positive checks in AC#5, AC#6, AC#11, AC#12, AC#17, AC#18 which verify specific typed constants are used.
- **Scope Note**: The `, 2\)` sub-pattern targets PregnancySettings.cs (TALENT index 2 → BodyType). WeatherSettings.cs does not use TALENT access, and per Technical Design, its post-migration code has no `, 2)` patterns (RNG uses `rng.Next(2)` which is `(2)` not `, 2)`).

**AC#16: Unit tests pin ground-truth uterus volume 1500 for body type -3 (shota)**
- **Test**: Grep pattern=`Assert.*1500.*UterusVolume|Assert.*UterusVolume.*1500` path=`engine.Tests/Tests/StateSettingsTests.cs`
- **Expected**: 1+ matches — an assertion line (anchored by `Assert`) where both `1500` and `UterusVolume` (CharacterFlagIndex) co-occur, verifying the assertion targets the correct flag with the correct value
- **Rationale**: Constraint C8 (business logic equivalence). Pins a non-default branch (body type -3 → 1500). Pattern requires `Assert` prefix plus both the expected value (1500) and the CharacterFlagIndex name (UterusVolume) on the same line, targeting assertion lines. The `Assert` anchor eliminates false positives from comments. Combined with AC#19 (TalentIndex.BodyType seeding), this verifies the corrected index path routes through the switch-case correctly. A test seeding wrong index 2 would produce default 5000, failing the 1500 assertion at runtime (caught by AC#14).

**AC#17: WeatherSettings uses typed CharacterFlagIndex.Mother**
- **Test**: Grep pattern=`CharacterFlagIndex\.Mother` path=`Era.Core/State/WeatherSettings.cs`
- **Expected**: 1+ matches
- **Rationale**: Constraint C6. Raw integer 74 must be replaced with typed CharacterFlagIndex.Mother. Complements AC#12 (Father) for positive verification of both parent indices in WeatherSettings.

**AC#18: WeatherSettings uses typed CharacterFlagIndex.ColdResistance**
- **Test**: Grep pattern=`CharacterFlagIndex\.ColdResistance` path=`Era.Core/State/WeatherSettings.cs`
- **Expected**: 1+ matches
- **Rationale**: Constraint C6. Raw integer 371 must be replaced with typed CharacterFlagIndex.ColdResistance. Split from AC#11 per Issue 71 rationale (alternation allows partial verification). Complements AC#11 (HeatResistance) for positive verification of both temperature resistance indices.

**AC#19: Tests use TalentIndex.BodyType for UterusVolumeInit test seeding**
- **Test**: Grep pattern=`TalentIndex\.BodyType` path=`engine.Tests/Tests/StateSettingsTests.cs`
- **Expected**: 1+ matches
- **Rationale**: Verifies tests seed MockVariableStore with the corrected TalentIndex.BodyType (index 100), not the old raw index 2. Complements AC#13/AC#16 ground-truth pinning by confirming the test input path uses the corrected index. Without this, AC#13's `UterusVolume.*5000` would pass even with wrong index seeding (body type defaults to 0 → 5000).

**AC#20: Tests construct PregnancySettings with MockVariableStore**
- **Test**: Grep pattern=`new PregnancySettings\(_mockStore\)` path=`engine.Tests/Tests/StateSettingsTests.cs`
- **Expected**: 1+ matches
- **Rationale**: Verifies that test construction uses `new PregnancySettings(_mockStore)` instead of the old no-arg constructor `new PregnancySettings()`. Complements AC#3 (production constructor injection) by confirming the test infrastructure also uses MockVariableStore-based construction, ensuring Goal 5 is fully covered.

**AC#21: Tests construct WeatherSettings with MockVariableStore**
- **Test**: Grep pattern=`new WeatherSettings\(_mockStore\)` path=`engine.Tests/Tests/StateSettingsTests.cs`
- **Expected**: 1+ matches
- **Rationale**: Symmetric with AC#20 for WeatherSettings. Verifies test construction uses `new WeatherSettings(_mockStore)` instead of the old no-arg constructor `new WeatherSettings()`. Ensures Goal 5 test rewrite coverage is balanced between PregnancySettings and WeatherSettings.

**AC#22: Unit tests pin ground-truth HeatResistance value 30 for character 0**
- **Test**: Grep pattern=`Assert.*30.*HeatResistance|Assert.*HeatResistance.*30` path=`engine.Tests/Tests/StateSettingsTests.cs`
- **Expected**: 1+ matches — an assertion line (anchored by `Assert`) where both `30` and `HeatResistance` (CharacterFlagIndex) co-occur
- **Rationale**: Constraint C8 (business logic equivalence) applied to WeatherSettings. Pins HeatResistanceBase[0] = 30 ground-truth value through MockVariableStore assertion. Symmetric with AC#13 (UterusVolumeInit ground-truth pinning). Combined with AC#14 (all tests pass), verifies TemperatureResistance correctly writes base heat resistance values via IVariableStore after migration.

**AC#23: Unit tests pin ground-truth ColdResistance value 10 for character 0**
- **Test**: Grep pattern=`Assert.*10.*ColdResistance|Assert.*ColdResistance.*10` path=`engine.Tests/Tests/StateSettingsTests.cs`
- **Expected**: 1+ matches — an assertion line (anchored by `Assert`) where both `10` and `ColdResistance` (CharacterFlagIndex) co-occur
- **Rationale**: Constraint C8 (business logic equivalence) applied to WeatherSettings. Pins ColdResistanceBase[0] = 10 ground-truth value. Symmetric with AC#16 (body type -3 pinning) as a second ground-truth data point for WeatherSettings. Combined with AC#22, provides two-dimensional coverage of TemperatureResistance base value initialization.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Replace delegate parameters in IPregnancySettings.UterusVolumeInit and IWeatherSettings.TemperatureResistance with IVariableStore-based access | AC#1, AC#2, AC#3, AC#4, AC#10, AC#22, AC#23 |
| 2 | Fix TALENT index bug in PregnancySettings (index 2 to 100) using typed TalentIndex.BodyType | AC#5, AC#9, AC#13, AC#16, AC#19 |
| 3 | Add missing typed index constants: 5 CharacterFlagIndex + 1 TalentIndex | AC#6, AC#8, AC#9, AC#11, AC#12, AC#15, AC#17, AC#18 |
| 4 | Remove dummy lambda creation in GameInitialization.cs for both methods | AC#7 |
| 5 | Rewrite affected tests to use MockVariableStore instead of delegate lambdas | AC#13, AC#14, AC#16, AC#19, AC#20, AC#21, AC#22, AC#23 |

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Apply the F796/BodySettings pattern verbatim to both `PregnancySettings` and `WeatherSettings`. Each class receives `IVariableStore` via constructor injection and stores it as `private readonly IVariableStore _variables`. All delegate parameters are removed from the interface methods and from the implementation signatures. Internal variable access is routed through three private helper methods (`GetCFlag`, `SetCFlag`, `GetTalent`) that call the typed `IVariableStore` methods and unwrap `Result<int>` via `.Match(v => v, _ => 0)`. The TALENT index bug is fixed atomically during the migration by replacing the raw literal `2` with `TalentIndex.BodyType`. Five `CharacterFlagIndex` constants and one `TalentIndex` constant are added to their respective type files. `GameInitialization.cs` call sites for both methods are simplified to remove the dummy lambda blocks; `UterusVolumeInit(characterId)` and `TemperatureToleranceInit()` wrappers remain but now call the no-delegate signatures. `ServiceCollectionExtensions.cs` requires no change because DI already resolves `IVariableStore` as a singleton and constructor injection is automatic. Tests in `StateSettingsTests.cs` are rewritten to construct `PregnancySettings(_mockStore)` / `WeatherSettings(_mockStore)` and interact via `_mockStore` helpers instead of inline lambdas. `GameInitializationTests.cs` `CreateSut()` is updated to pass `new PregnancySettings(_mockStore)` / `new WeatherSettings(_mockStore)`. `HeadlessIntegrationTests.cs` stub tests (`StubInterface_UterusVolumeInit_Exists`, `StubInterface_TemperatureToleranceInit_Exists`) remain valid because the `GameInitialization` wrapper signatures are unchanged.

This approach satisfies all 23 ACs: AC#1–AC#2 verify interface signature cleanup; AC#3–AC#4 verify constructor injection; AC#5–AC#6, AC#11–AC#12, AC#15, AC#17–AC#18 verify typed index usage and absence of raw integers; AC#7 verifies dummy lambda removal in GameInitialization; AC#8–AC#9 verify constant additions; AC#10 verifies `characterCount` stays as a parameter; AC#13 and AC#16 pin ground-truth uterus volume values for two distinct body types; AC#14 confirms all tests pass; AC#19 verifies correct TalentIndex.BodyType test seeding; AC#20–AC#21 verify MockVariableStore-based test construction for both PregnancySettings and WeatherSettings. AC#22–AC#23 pin WeatherSettings ground-truth values (HeatResistance 30 for character 0, ColdResistance 10 for character 0).

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Remove `Func<int,int,int>` and `Action<int,int,int>` parameters from `IPregnancySettings.UterusVolumeInit`; keep only `int characterId` |
| 2 | Remove `Func<int,int,int>` and `Action<int,int,int>` parameters from `IWeatherSettings.TemperatureResistance`; keep `int characterCount` and `System.Random rng` |
| 3 | Add `private readonly IVariableStore _variables` field and `(IVariableStore variables)` constructor to `PregnancySettings` |
| 4 | Add `private readonly IVariableStore _variables` field and `(IVariableStore variables)` constructor to `WeatherSettings` |
| 5 | In `PregnancySettings.UterusVolumeInit`, replace `getTalent(characterId, 2)` with `GetTalent(characterId, TalentIndex.BodyType)` using the private helper |
| 6 | In `PregnancySettings.UterusVolumeInit`, replace `setCflag(characterId, 350, uterusVolume)` with `SetCFlag(characterId, CharacterFlagIndex.UterusVolume, uterusVolume)` |
| 7 | In `GameInitialization.UterusVolumeInit` and `TemperatureToleranceInit`, remove the local dummy lambda variables (`getCflag = (id,idx) => 0` etc.) and call the implementation directly |
| 8 | Add `Father = new(73)`, `Mother = new(74)`, `UterusVolume = new(350)`, `HeatResistance = new(370)`, `ColdResistance = new(371)` to `CharacterFlagIndex.cs` |
| 9 | Add `BodyType = new(100)` to `TalentIndex.cs` |
| 10 | Keep `int characterCount` as first parameter in `IWeatherSettings.TemperatureResistance` and `WeatherSettings.TemperatureResistance`; do not resolve it via `IVariableStore` |
| 11 | In `WeatherSettings.TemperatureResistance`, replace raw literals `370` with `CharacterFlagIndex.HeatResistance` in all `SetCFlag`/`GetCFlag` calls |
| 12 | In `WeatherSettings.TemperatureResistance`, replace raw literals `73` with `CharacterFlagIndex.Father` in `GetCFlag` calls |
| 13 | In `StateSettingsTests.cs`, rewrite UterusVolumeInit body type 0 test to use `MockVariableStore` and assert `_mockStore.GetCFlag(charId, CharacterFlagIndex.UterusVolume) == 5000` — assertion line must co-locate value and flag name |
| 14 | After all changes, `dotnet test Era.Core.Tests/ && dotnet test engine.Tests/` exits with code 0 |
| 15 | After migration, no raw integer CFLAG/TALENT index calls remain in `PregnancySettings.cs` or `WeatherSettings.cs` (confirmed by AC#5, AC#6, AC#11, AC#12, AC#17, AC#18 positive checks and this comprehensive negative grep) |
| 16 | In `StateSettingsTests.cs`, add test `UterusVolumeInit_BodyTypeMinus3_Sets1500` using `MockVariableStore` to pin body type -3 → 1500 ground-truth |
| 17 | In `WeatherSettings.TemperatureResistance`, replace raw literals `74` with `CharacterFlagIndex.Mother` in `GetCFlag` calls |
| 18 | In `WeatherSettings.TemperatureResistance`, replace raw literals `371` with `CharacterFlagIndex.ColdResistance` in `SetCFlag`/`GetCFlag` calls |
| 19 | In `StateSettingsTests.cs`, use `_mockStore.SetTALENT(charId, TalentIndex.BodyType, value)` (not raw index 2) for all UterusVolumeInit test seeding |
| 20 | In `StateSettingsTests.cs`, construct PregnancySettings with `new PregnancySettings(_mockStore)` instead of no-arg constructor |
| 21 | In `StateSettingsTests.cs`, construct WeatherSettings with `new WeatherSettings(_mockStore)` instead of no-arg constructor |
| 22 | In `StateSettingsTests.cs`, rewrite TemperatureResistance character 0 test to assert `_mockStore.GetCFlag(0, CharacterFlagIndex.HeatResistance) == 30` — assertion line must co-locate value and flag name |
| 23 | In `StateSettingsTests.cs`, add assertion for `_mockStore.GetCFlag(0, CharacterFlagIndex.ColdResistance) == 10` — cold resistance base value ground-truth pinning |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Private helper pattern (GetCFlag/SetCFlag/GetTalent) | A: Inline `_variables.GetCharacterFlag(new CharacterId(id), flag).Match(...)` at each call site, B: Private helpers matching F796 BodySettings pattern | B | F796 established the helper pattern; reduces duplication, ensures consistency, and keeps call sites readable |
| TALENT index fix scope | A: Defer to a separate bug-fix feature, B: Fix atomically in this migration | B | The migration changes the delegate signature anyway, making raw index 2 unreachable without a refactor. Fixing atomically is lower risk than a two-step change. Documented as intentional correction in tests. |
| characterCount sourcing | A: Resolve via IVariableStore (add CHARANUM accessor), B: Keep as method parameter | B | IVariableStore has no CHARANUM method. Adding it is a separate concern with broader impact. Design decision confirmed by 3/3 investigation explorers and Constraint C9. |
| System.Random handling | A: Wrap in IRandomProvider interface, B: Keep as method parameter | B | Constraint C10 requires testability without a new interface. `new System.Random(seed)` in tests provides determinism. No need to introduce IRandomProvider for this scope. |
| GameInitialization wrapper signatures | A: Remove the `UterusVolumeInit(int characterId)` / `TemperatureToleranceInit()` wrappers, B: Keep wrappers, remove dummy lambdas inside | B | ERB compatibility layer depends on these parameterless wrapper signatures in `GameInitialization`. Removing them is a separate scope. Only the internal dummy lambda creation is removed. |
| DI registration | A: Manually add IVariableStore to `AddSingleton<IPregnancySettings, PregnancySettings>()`, B: Rely on automatic DI constructor injection | B | `ServiceCollectionExtensions.cs` already registers `IVariableStore` as a singleton. DI auto-resolves constructor parameters; no manual change required (verified via ArchitectureTests AC#14). |

### Interfaces / Data Structures

**Modified Interface: `IPregnancySettings` (`Era.Core/Interfaces/IPregnancySettings.cs`)**

```csharp
public interface IPregnancySettings
{
    void UterusVolumeInit(int characterId);
    // Delegate parameters removed: Func<int,int,int> getCflag, Action<int,int,int> setCflag, Func<int,int,int> getTalent
}
```

**Modified Interface: `IWeatherSettings` (`Era.Core/Interfaces/IWeatherSettings.cs`)**

```csharp
public interface IWeatherSettings
{
    void TemperatureResistance(int characterCount, System.Random rng);
    // Delegate parameters removed: Func<int,int,int> getCflag, Action<int,int,int> setCflag
}
```

**Modified Class: `PregnancySettings` (`Era.Core/State/PregnancySettings.cs`)**

```csharp
public class PregnancySettings : IPregnancySettings
{
    private readonly IVariableStore _variables;

    public PregnancySettings(IVariableStore variables)
    {
        _variables = variables ?? throw new ArgumentNullException(nameof(variables));
    }

    public void UterusVolumeInit(int characterId)
    {
        int bodyType = GetTalent(characterId, TalentIndex.BodyType);  // was: getTalent(characterId, 2)
        int uterusVolume = bodyType switch
        {
            -3 => 1500,
            -2 => 3000,
            -1 => 4000,
            1  => 6000,
            2  => 8000,
            3  => 12000,
            _  => 5000  // default: 普通 (0)
        };
        SetCFlag(characterId, CharacterFlagIndex.UterusVolume, uterusVolume);  // was: setCflag(characterId, 350, ...)
    }

    private int GetCFlag(int characterId, CharacterFlagIndex flag)
        => _variables.GetCharacterFlag(new CharacterId(characterId), flag).Match(v => v, _ => 0);

    private void SetCFlag(int characterId, CharacterFlagIndex flag, int value)
        => _variables.SetCharacterFlag(new CharacterId(characterId), flag, value);

    private int GetTalent(int characterId, TalentIndex talent)
        => _variables.GetTalent(new CharacterId(characterId), talent).Match(v => v, _ => 0);
}
```

**Modified Class: `WeatherSettings` (`Era.Core/State/WeatherSettings.cs`)**

```csharp
public class WeatherSettings : IWeatherSettings
{
    private readonly IVariableStore _variables;

    public WeatherSettings(IVariableStore variables)
    {
        _variables = variables ?? throw new ArgumentNullException(nameof(variables));
    }

    public void TemperatureResistance(int characterCount, System.Random rng)
    {
        // rng null check preserved
        if (rng == null) throw new ArgumentNullException(nameof(rng));

        for (int i = 0; i < 10 && i < characterCount; i++)
        {
            SetCFlag(i, CharacterFlagIndex.HeatResistance, HeatResistanceBase[i]);  // was: setCflag(i, 370, ...)
            SetCFlag(i, CharacterFlagIndex.ColdResistance, ColdResistanceBase[i]);  // was: setCflag(i, 371, ...)
        }
        for (int childId = 10; childId < characterCount; childId++)
        {
            int fatherId = GetCFlag(childId, CharacterFlagIndex.Father);  // was: getCflag(childId, 73)
            int motherId = GetCFlag(childId, CharacterFlagIndex.Mother);  // was: getCflag(childId, 74)
            // ... same inheritance logic ...
        }
    }

    private int GetCFlag(int characterId, CharacterFlagIndex flag)
        => _variables.GetCharacterFlag(new CharacterId(characterId), flag).Match(v => v, _ => 0);

    private void SetCFlag(int characterId, CharacterFlagIndex flag, int value)
        => _variables.SetCharacterFlag(new CharacterId(characterId), flag, value);
}
```

**New Constants in `CharacterFlagIndex.cs`:**

```csharp
// Pregnancy / Family (CFLAG indices verified from CFLAG.csv)
public static readonly CharacterFlagIndex Father = new(73);           // 父親
public static readonly CharacterFlagIndex Mother = new(74);           // 母親
public static readonly CharacterFlagIndex UterusVolume = new(350);    // 子宮内体積

// Temperature Resistance (CFLAG indices verified from 天候.ERB)
public static readonly CharacterFlagIndex HeatResistance = new(370);  // 暑さ耐性
public static readonly CharacterFlagIndex ColdResistance = new(371);  // 寒さ耐性
```

**New Constant in `TalentIndex.cs`:**

```csharp
public static readonly TalentIndex BodyType = new(100);  // 体型 - F797
```

**Modified: `GameInitialization.cs` — `UterusVolumeInit` wrapper**

```csharp
public void UterusVolumeInit(int characterId)
{
    _pregnancySettings.UterusVolumeInit(characterId);
    // Removed: Func<int,int,int> getCflag, Action<int,int,int> setCflag, Func<int,int,int> getTalent dummy lambdas
}
```

**Modified: `GameInitialization.cs` — `TemperatureToleranceInit` wrapper**

```csharp
public void TemperatureToleranceInit()
{
    int characterCount = 10; // Default CHARANUM placeholder (out of scope per C9)
    _weatherSettings.TemperatureResistance(characterCount, new System.Random());
    // Removed: Func<int,int,int> getCflag, Action<int,int,int> setCflag dummy lambdas
}
```

**Modified: `StateSettingsTests.cs` constructor**

```csharp
public StateSettingsTests()
{
    _mockStore = new MockVariableStore();
    _bodySettings = new BodySettings(_mockStore);
    _pregnancySettings = new PregnancySettings(_mockStore);   // was: new PregnancySettings()
    _weatherSettings = new WeatherSettings(_mockStore);        // was: new WeatherSettings()
}
```

Test bodies rewritten to use `_mockStore.SetTALENT(charId, TalentIndex.BodyType, value)` and `_mockStore.GetCFlag(charId, CharacterFlagIndex.UterusVolume)` instead of inline lambda dictionaries. The TALENT index seed changes from raw `(charId, 2)` to `TalentIndex.BodyType`. Null accessor tests (`UterusVolumeInit_NullAccessor_ThrowsArgumentNullException`) are replaced with a null constructor argument test.

**Modified: `GameInitializationTests.cs` `CreateSut()`**

```csharp
private static GameInitialization CreateSut()
{
    var store = new MockVariableStore();
    return new GameInitialization(
        new BodySettings(store),
        new PregnancySettings(store),   // was: new PregnancySettings()
        new WeatherSettings(store),      // was: new WeatherSettings()
        new NtrInitialization()
    );
}
```

### Upstream Issues

<!-- No upstream issues found during Technical Design. All IVariableStore methods required
     (GetCharacterFlag, SetCharacterFlag, GetTalent) are verified present in IVariableStore.cs:28-35.
     All typed index types (CharacterFlagIndex, TalentIndex, CharacterId) exist.
     AC table is consistent with constraints and implementation plan. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| None | - | - |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 8 | Add 5 new constants to `Era.Core/Types/CharacterFlagIndex.cs`: `Father = new(73)`, `Mother = new(74)`, `UterusVolume = new(350)`, `HeatResistance = new(370)`, `ColdResistance = new(371)` | | [x] |
| 2 | 9 | Add 1 new constant to `Era.Core/Types/TalentIndex.cs`: `BodyType = new(100)` | | [x] |
| 3 | 1,3,5,6,15 | Update `Era.Core/Interfaces/IPregnancySettings.cs`: replace the multi-delegate `UterusVolumeInit` declaration with `void UterusVolumeInit(int characterId)`; rewrite `Era.Core/State/PregnancySettings.cs`: add `IVariableStore` constructor injection (`private readonly IVariableStore _variables`), remove delegate parameters, add `GetCFlag`/`SetCFlag`/`GetTalent` private helpers (F796 pattern), replace `getTalent(characterId, 2)` with `GetTalent(characterId, TalentIndex.BodyType)`, replace `setCflag(characterId, 350, ...)` with `SetCFlag(characterId, CharacterFlagIndex.UterusVolume, ...)` | | [x] |
| 4 | 2,4,10,11,12,15,17,18 | Update `Era.Core/Interfaces/IWeatherSettings.cs`: replace delegate parameters in `TemperatureResistance` with `void TemperatureResistance(int characterCount, System.Random rng)`; rewrite `Era.Core/State/WeatherSettings.cs`: add `IVariableStore` constructor injection, remove delegate parameters, add `GetCFlag`/`SetCFlag` private helpers, replace all raw integer indices (370→`CharacterFlagIndex.HeatResistance`, 371→`CharacterFlagIndex.ColdResistance`, 73→`CharacterFlagIndex.Father`, 74→`CharacterFlagIndex.Mother`), keep `characterCount` as method parameter | | [x] |
| 5 | 7 | Update `Era.Core/Common/GameInitialization.cs`: remove dummy lambda creation blocks (`getCflag = (id, idx) => 0`, etc.) AND associated TODO comments (`// TODO: Replace with GlobalStatic accessors → Phase 22`) from `UterusVolumeInit` and `TemperatureToleranceInit` wrapper methods; call implementation directly without delegate construction | | [x] |
| 6 | 13,14,16,19,20,21,22,23 | Rewrite affected tests in `engine.Tests/Tests/StateSettingsTests.cs`: replace delegate lambda constructors with `new PregnancySettings(_mockStore)` / `new WeatherSettings(_mockStore)`; rewrite `UterusVolumeInit` test methods to use `_mockStore.SetTALENT(charId, TalentIndex.BodyType, value)` and assert `_mockStore.GetCFlag(charId, CharacterFlagIndex.UterusVolume) == 5000` for body type 0 (ground-truth pinning); add TemperatureResistance character 0 assertions for `_mockStore.GetCFlag(0, CharacterFlagIndex.HeatResistance) == 30` and `_mockStore.GetCFlag(0, CharacterFlagIndex.ColdResistance) == 10`; update `engine.Tests/Tests/GameInitializationTests.cs` `CreateSut()` to pass `new PregnancySettings(store)` / `new WeatherSettings(store)`; verify `engine.Tests/Tests/HeadlessIntegrationTests.cs` stub tests remain valid | | [x] |
| 7 | 14 | Run `dotnet build Era.Core` (via WSL) and verify zero errors and zero warnings; run `dotnet test Era.Core.Tests` and `dotnet test engine.Tests` and verify all tests pass | | [x] |

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
| 1 | implementer | sonnet | feature-797.md (Tasks T1–T2), `Era.Core/Types/CharacterFlagIndex.cs`, `Era.Core/Types/TalentIndex.cs` | Updated CharacterFlagIndex.cs (5 new constants), TalentIndex.cs (1 new constant) |
| 2 | implementer | sonnet | feature-797.md (Tasks T3–T4), `Era.Core/Interfaces/IPregnancySettings.cs`, `Era.Core/State/PregnancySettings.cs`, `Era.Core/Interfaces/IWeatherSettings.cs`, `Era.Core/State/WeatherSettings.cs` | Updated interface and implementation files with IVariableStore injection and typed index usage |
| 3 | implementer | sonnet | feature-797.md (Task T5), `Era.Core/Common/GameInitialization.cs` | Updated GameInitialization.cs with dummy lambda removal |
| 4 | implementer | sonnet | feature-797.md (Task T6), `engine.Tests/Tests/StateSettingsTests.cs`, `engine.Tests/Tests/GameInitializationTests.cs`, `engine.Tests/Tests/HeadlessIntegrationTests.cs` | Updated test files using MockVariableStore with PregnancySettings/WeatherSettings |
| 5 | tester | sonnet | feature-797.md (Task T7), WSL dotnet environment | Build pass confirmation, all-test pass confirmation |

### Pre-conditions

- F796 is [DONE]: IVariableStore migration pattern confirmed in `Era.Core/State/BodySettings.cs:29-34`
- F789 is [DONE]: `IVariableStore.GetCharacterFlag`, `SetCharacterFlag`, `GetTalent` confirmed present in `Era.Core/Interfaces/IVariableStore.cs:28-35`
- F779 is [DONE]: `CharacterId` typed wrapper exists
- Typed index infrastructure (`CharacterFlagIndex`, `TalentIndex`) exists in `Era.Core/Types/`
- Baseline captured in `.tmp/baseline-797.txt`

### Execution Order

Tasks must execute in sequence: T1 → T2 (typed constants first — required by T3/T4 implementations) → T3 → T4 (interface + implementation atomic group) → T5 (call site cleanup) → T6 (test rewrite) → T7 (build + test verification).

**T1+T2 before T3+T4**: Typed constants (CharacterFlagIndex, TalentIndex) must exist before implementation files reference them. Build will fail if constants are referenced before declaration.

**T3+T4+T5 atomic group**: These three changes form a breaking change set on IPregnancySettings and IWeatherSettings. All must be applied before running build (T7); partial application causes CS7036 compilation errors at GameInitialization.cs call sites.

### New Private Helpers (F796 Pattern)

`PregnancySettings.cs` adds three private helper methods and `WeatherSettings.cs` adds two (GetCFlag, SetCFlag only — no TALENT access required), following `Era.Core/State/BodySettings.cs:502-510`:

```csharp
private int GetCFlag(int characterId, CharacterFlagIndex flag)
    => _variables.GetCharacterFlag(new CharacterId(characterId), flag).Match(v => v, _ => 0);

private void SetCFlag(int characterId, CharacterFlagIndex flag, int value)
    => _variables.SetCharacterFlag(new CharacterId(characterId), flag, value);

private int GetTalent(int characterId, TalentIndex talent)
    => _variables.GetTalent(new CharacterId(characterId), talent).Match(v => v, _ => 0);
```

`WeatherSettings.cs` adds only `GetCFlag` and `SetCFlag` (no TALENT access required).

### Build Verification (T7)

Run via WSL:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core'
```

Expected: `Build succeeded. 0 Error(s). 0 Warning(s).`

### Test Verification (T7)

Run Era.Core.Tests via WSL:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests'
```

Run engine.Tests via WSL:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test engine.Tests'
```

Expected: All tests pass. UterusVolumeInit and TemperatureResistance test methods must all pass with MockVariableStore-based construction.

### Success Criteria

All 23 ACs pass (AC#1–AC#23):
- AC#1–2: Interface signature verified (no delegate parameters in IPregnancySettings and IWeatherSettings)
- AC#3–4: Constructor injection verified (`private readonly IVariableStore _variables` in both implementation files)
- AC#5–6, 11–12, 15, 17–18: Typed index usage verified (TalentIndex.BodyType, CharacterFlagIndex.UterusVolume, HeatResistance, ColdResistance, Father, Mother); no raw integer CFLAG/TALENT indices remain
- AC#7: GameInitialization.cs dummy lambda removal verified (0 matches for getCflag/setCflag/getTalent lambda assignments)
- AC#8–9: New typed constants verified (5 in CharacterFlagIndex.cs, 1 in TalentIndex.cs)
- AC#10: characterCount kept as method parameter in WeatherSettings.TemperatureResistance
- AC#13, 16: Ground-truth pinning verified (5000 for body type 0, 1500 for body type -3)
- AC#14: All unit tests pass (Era.Core.Tests + engine.Tests, exit code 0)
- AC#22–23: WeatherSettings ground-truth pinning verified (HeatResistance 30 for character 0, ColdResistance 10 for character 0)

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| NtrInitialization in GameInitialization.cs (lines 369-376) also uses delegate-based dual access pattern | Same pattern as UterusVolumeInit/TemperatureToleranceInit but explicitly out of scope per Technical Constraint (C9 note); discovered during Technical Design review of GameInitialization.cs call sites | Feature | F782 | - |
| GetCFlag/SetCFlag/GetTalent private helpers duplicated across BodySettings, PregnancySettings, and WeatherSettings | 3-way identical code duplication; any future Result<int> error handling change must be replicated in all 3 files. F797 correctly follows established F796 pattern but extraction into shared base class or IVariableStore extension methods should be tracked | Feature | F782 | - |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-21 06:15 | START | implementer | Task 6 TDD RED | - |
| 2026-02-21 06:20 | END | implementer | Task 6 TDD RED | RED confirmed (compile errors as expected) |
| 2026-02-21 06:25 | START | implementer | Tasks T1-T2 (typed constants) | - |
| 2026-02-21 06:25 | END | implementer | Tasks T1-T2 | SUCCESS (build 0 errors 0 warnings) |
| 2026-02-21 06:28 | START | implementer | Tasks T3-T4 (interface+impl migration) | - |
| 2026-02-21 06:30 | END | implementer | Tasks T3-T4 | SUCCESS (Era.Core build fails at GameInit call sites - expected) |
| 2026-02-21 06:32 | START | implementer | Task T5 (GameInitialization cleanup) | - |
| 2026-02-21 06:33 | END | implementer | Task T5 | SUCCESS (build 0 errors 0 warnings) |
| 2026-02-21 06:34 | DEVIATION | orchestrator | dotnet test engine.Tests/ | BUILD_FAIL: HeadlessIntegrationTests.cs:74-75 CS7036 no-arg PregnancySettings/WeatherSettings |
| 2026-02-21 06:35 | START | implementer | Task T6 supplement (HeadlessIntegrationTests fix) | - |
| 2026-02-21 06:37 | END | implementer | Task T6 supplement | SUCCESS (build 0 errors 0 warnings) |
| 2026-02-21 06:38 | END | orchestrator | T7 test verification | GREEN: Era.Core.Tests 2277/2277, engine.Tests 586/586 |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: Philosophy section | designs/full-csharp-architecture.md path changed to docs/architecture/migration/full-csharp-architecture.md
- [fix] Phase2-Review iter1: AC#7 Definition/Detail | Expanded not_matches pattern from getCflag-only to getCflag/setCflag/getTalent to cover all dummy lambda types
- [fix] Phase2-Review iter1: AC#13 Definition/Detail | Strengthened matcher from bare `5000` to `UterusVolume.*5000|5000.*UterusVolume` for context-specific matching
- [fix] Phase2-Review iter1: Philosophy Derivation | Revised "documented transition points" row to derive AC#13 from ground-truth pinning; removed duplicate AC#5 (already in "clear phase boundaries")
- [fix] Phase2-Review iter2: AC#16 added | New AC pinning body type -3 → 1500 ground-truth to break circular AC#14 dependency for non-default branch
- [fix] Phase2-Review iter2: AC#12/AC#17 split | AC#12 retained for Father, AC#17 added for Mother positive verification in WeatherSettings
- [fix] Phase2-Uncertain iter3: Goal Coverage | Fixed uncovered ACs: moved AC#17 from Goal 1 to Goal 3, added AC#6/AC#10/AC#11/AC#12/AC#15 to appropriate Goals
- [fix] Phase2-Review iter3: Task 3 AC# | Added AC#15 to Task 3 (PregnancySettings raw index removal is part of AC#15's scope)
- [fix] Phase3-Maintainability iter4: AC#11/AC#18 | AC#11 title fixed (was "heat/cold resistance", now "HeatResistance" only); AC#18 added for ColdResistance positive verification
- [fix] Phase3-Maintainability iter4: Mandatory Handoffs | Added GetCFlag/SetCFlag/GetTalent helper duplication tracking → F782
- [fix] Phase2-Review iter5: AC#8 | Strengthened from name-only pattern to value-binding pattern (e.g., `Father = new\(73\)`) preventing wrong-value false positives
- [fix] Phase2-Review iter5: AC#19 added | Verifies tests use TalentIndex.BodyType (not raw index 2) for UterusVolumeInit seeding
- [fix] Phase2-Review iter6: AC#13/AC#16 | Strengthened: narrowed path to StateSettingsTests.cs, changed pattern to match test method names (Sets5000/Sets1500) instead of bare token co-occurrence
- [fix] Phase2-Review iter7: AC#19 | Narrowed path from broad test directories to engine.Tests/Tests/StateSettingsTests.cs to match stated rationale
- [fix] Phase2-Review iter8: AC#13 Detail | Added Verification Unit note documenting AC#13+AC#19+AC#14 triple as ground-truth verification unit
- [fix] Phase2-Review iter9: Implementation Contract | Fixed inconsistency: "Both add same three helpers" → "PregnancySettings adds three, WeatherSettings adds two (no GetTalent)"
- [fix] Phase3-Maintainability iter10: Mandatory Handoffs Leak Prevention | Added F797 deferred obligations to F782 Review Context (NtrInitialization migration + helper duplication extraction)
- [fix] Phase2-Review iter11: Upstream Issues table | Added placeholder row `| None | - | - |` for explicit empty state
- [fix] Phase2-Review iter11: AC#13/AC#16 matchers | Strengthened from method-name-matching (`Sets5000`/`Sets1500`) to assertion-line-matching (`5000.*UterusVolume`/`1500.*UterusVolume`) requiring value+flag co-occurrence
- [fix] Phase2-Review iter12: Philosophy Derivation | Added AC#16, AC#19 to "documented transition points" row to match AC#13 Verification Unit (AC#13+AC#16+AC#19)
- [fix] Phase2-Review iter13: AC#20 added | New AC verifying MockVariableStore-based test construction `new PregnancySettings(_mockStore)` in StateSettingsTests.cs (Goal 5 coverage)
- [fix] Phase2-Review iter13: Philosophy Derivation | Added AC#11, AC#12, AC#17, AC#18 to "clear phase boundaries" row for WeatherSettings typed index coverage
- [resolved-applied] Phase2-Uncertain iter14: Philosophy Derivation "clear phase boundaries" semantics — typed index ACs may be better anchored under a separate "type safety" Philosophy claim rather than "clear phase boundaries"
- [fix] PostLoop-UserFix post-loop: Philosophy Derivation | Added Phase 3→20 boundary rationale to "clear phase boundaries" Derived Requirement
- [fix] Phase2-Review iter14: AC#15 pattern | Removed variable-name coupling (characterId/childId) — now uses comma-prefixed raw integer patterns `, 2\)|, 350[,)]` etc. for variable-name-independent matching
- [fix] Phase2-Review iter15: AC#21 added | Symmetric WeatherSettings MockVariableStore construction verification `new WeatherSettings(_mockStore)` in StateSettingsTests.cs (Goal 5 coverage)
- [fix] Phase2-Review iter16: Goal Coverage | Added AC#13, AC#16, AC#19 to Goal 2 (TALENT index fix behavioral verification)
- [fix] Phase2-Review iter17: AC#13/AC#16 matchers | Added `Assert` anchor prefix to eliminate comment/variable false positives (`Assert.*5000.*UterusVolume`)
- [fix] Phase2-Review iter18: AC#3/AC#4 Detail | Added Verification Chain notes documenting that AC#5/AC#6 (PregnancySettings) and AC#11/AC#12/AC#17/AC#18 (WeatherSettings) imply _variables usage through helper chain
- [fix] Phase2-Review iter18: AC#15 Detail | Added Scope Note documenting `, 2\)` targets PregnancySettings TALENT index only; WeatherSettings has no `, 2)` patterns per Technical Design
- [fix] Phase3-Maintainability iter19: Approach section | Updated stale AC count from "19 ACs" to "21 ACs" and added AC#19-AC#21 to coverage summary
- [fix] Phase3-Maintainability iter20: Leak Prevention | Added F797 as Predecessor to F782 Dependencies table (bidirectional consistency)
- [fix] Phase3-Maintainability iter20: Task T5 | Added TODO comment removal note to Task T5 description (GameInitialization.cs lines 330, 349)
- [fix] Phase2-Review iter1: AC#22/AC#23 added | WeatherSettings ground-truth pinning for HeatResistance(30) and ColdResistance(10) via MockVariableStore assertions; updated Philosophy Derivation, Goal Coverage, AC Coverage, Tasks, Approach, Success Criteria
- [problem-fix] Step9.5: BUILD_FAIL:HeadlessIntegrationTests.cs CS7036 — PregnancySettings/WeatherSettings constructor requires IVariableStore but HeadlessIntegrationTests used no-arg. Fixed by updating CreateGameInitialization() to pass shared MockVariableStore (Action D).

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F796](feature-796.md) - BodyDetailInit delegate-to-IVariableStore migration (pattern source)
- [Related: F779](feature-779.md) - IVariableStore introduction to BodySettings
- [Related: F789](feature-789.md) - IVariableStore Phase 20 extensions
- [Related: F780](feature-780.md) - Genetics & Growth (Phase 20 sibling; also needs Father(73) CharacterFlagIndex constant)
- [Related: F794](feature-794.md) - Shared Body Option Validation Abstraction (Phase 20 sibling)
- [Related: F778](feature-778.md) - MockVariableStore test patterns
- [Successor: F782](feature-782.md) - Post-Phase Review (depends on cleanup of remaining dual patterns)
- [Successor: F783](feature-783.md) - Indirect successor via F782
