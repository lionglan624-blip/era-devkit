# Feature 821: Weather System Migration

## Status: [DONE]
<!-- fl-reviewed: 2026-03-05T02:38:41Z -->

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

Phase 22: State Systems -- All ERB state subsystems must be migrated to C# with ISP-compliant interfaces, equivalence-tested against ERB baselines, achieving zero-debt implementation. Weather subsystem (天候.ERB, 839 lines) is the SSOT for weather state management, climate data, and temperature resistance calculation within this phase.

### Problem (Current Issue)

The `IWeatherSettings` interface in Era.Core exposes only 1 of 15 functions from 天候.ERB, because it was extracted as a minimal stub during F377 Phase 4 solely to unblock the SYSTEM.ERB `@気温耐性取得` initialization call (`IWeatherSettings.cs:9`, `WeatherSettings.cs:14-16`). The remaining 14 functions -- spanning calendar management (`@日付変更`, `@日付_月`), climate lookup tables (4 probability/temperature functions, 384 lines), weather simulation state machine (`@天候状態`, `@日間気温設定`, `@現在気温設定`, `@異常気象`), and character status effects (`@天候によるステータス増減処理`) -- have no C# implementation. This blocks F822 (Pregnancy System) which requires `@子供気温耐性取得` (`PREGNACY_S.ERB:426`), and leaves runtime weather callers (`BEFORETRAIN.ERB:47-50`, `EVENTCOMEND.ERB:16-18`, `INFO.ERB:822-823`) without a migration path. Additionally, the 15 functions span 5 distinct domains with different dependency profiles; migrating 1:1 ERB-to-class would violate SRP (F808 lesson).

### Goal (What to Achieve)

Migrate the 13 in-scope functions from 天候.ERB to C# (14 total minus @日付初期設定 which is excluded due to GOTO+INPUT interactive loop; track @日付初期設定 as deferred obligation per Scope Discipline). Implement ISP-compliant interface split, equivalence tests against ERB baseline with deterministic/seeded random verification. Expose `@子供気温耐性取得` as a separately callable method to unblock F822.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is the weather subsystem not available in C#? | Only 1 of 15 functions (TemperatureResistance) exists in Era.Core | `WeatherSettings.cs:14-16` -- "In-scope: 気温耐性取得 (lines 777-819)" |
| 2 | Why was only 1 function migrated? | It was extracted as a minimal stub for F377 Phase 4 to unblock SYSTEM.ERB migration | `WeatherSettings.cs:12` -- "Phase 3 external dependency for F365" |
| 3 | Why was the full weather migration not done during F377? | Full weather migration was deferred to Phase 22 as a state system | `feature-814.md:26` -- Phase 22 planning scope |
| 4 | Why does it matter now? | F822 (Pregnancy) has a hard CALL dependency on `@子供気温耐性取得` which is not yet in C# | `PREGNACY_S.ERB:426` -- `CALL 子供気温耐性取得, 子供` |
| 5 | Why (Root)? | The migration architecture correctly partitioned work by phase; IWeatherSettings was designed for initialization-time use only, not runtime weather simulation, and now requires ISP-compliant expansion across 5 distinct domains | `IWeatherSettings.cs:9` -- single method interface |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | 天候.ERB has 14 unmigrated functions blocking F822 | IWeatherSettings was extracted as minimal single-method stub for F377, not as a complete weather system interface |
| Where | `天候.ERB:1-839` (ERB source) | `IWeatherSettings.cs:9` (single-method interface in Era.Core) |
| Fix | Add methods one-by-one to IWeatherSettings | ISP-compliant interface split across 5 domains with class decomposition per F808 lesson |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F814 | [DONE] | Predecessor -- Phase 22 Planning |
| F797 | [DONE] | Predecessor -- IVariableStore migration |
| F782 | [DONE] | Predecessor -- IVariableStoreExtensions |
| F796 | [DONE] | Pattern source -- BodyDetailInit migration pattern |
| F808 | [DONE] | Lesson source -- ERB boundary != domain boundary |
| F822 | [DRAFT] | Successor -- Pregnancy System depends on @子供気温耐性取得 |
| F825 | [DRAFT] | Successor -- DI Integration depends on F821 |
| F826 | [DRAFT] | Successor -- Post-Phase Review |
| F819 | [DRAFT] | Sibling -- no call-chain dependency |
| F823 | [WIP] | Sibling -- no call-chain dependency |
| F824 | [DRAFT] | Sibling -- no call-chain dependency |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| ERB source availability | FEASIBLE | 天候.ERB: 839 lines, all 15 functions readable |
| Existing partial migration | FEASIBLE | WeatherSettings.TemperatureResistance() as foundation |
| IVariableStore infrastructure | FEASIBLE | GetFlag/SetFlag/GetCFlag/SetCFlag extensions exist |
| ILocationService dependency | FEASIBLE | IsOpenPlace(int) exists at `ILocationService.cs:14` |
| IRandomProvider dependency | FEASIBLE | Next(max) and Next(min, max) at `IRandomProvider.cs:8-38` |
| ICounterUtilities dependency | FEASIBLE | TimeProgress(int) exists at `ICounterUtilities.cs:21` |
| IConsoleOutput dependency | FEASIBLE | Print/PrintLine available at `IConsoleOutput.cs:9-78` |
| IEngineVariables dependency | FEASIBLE | GetTime()/GetDay()/GetCharaNum() exist at `IEngineVariables.cs:108-114` |
| Variable definitions | FEASIBLE | 天候値, 暦法月, 暦法日 defined at `VariableDefinitions.cs:139-160` |
| Interactive UI function (@日付初期設定) | NOT_FEASIBLE | GOTO + INPUT + PRINTBUTTON loop; excluded from scope |
| IEngineVariables indexed access (DAY:1, DAY:2, TIME:1) | NEEDS_REVISION | Current IEngineVariables has scalar GetTime()/GetDay(); indexed access for TIME:1, DAY:1, DAY:2 needs verification |
| Equivalence testing | FEASIBLE | Deterministic data tables + seeded random for state machine |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core interfaces | HIGH | IWeatherSettings expansion or split into multiple ISP-compliant interfaces |
| Era.Core NuGet | HIGH | Interface changes require NuGet version bump and cross-repo coordination |
| F822 (Pregnancy) | HIGH | Unblocked by @子供気温耐性取得 migration |
| F825 (DI Integration) | MEDIUM | New interfaces require DI registration |
| Weather string consumers | MEDIUM | 7 files / 18 occurrences use 天候(天候値) string lookup; must expose GetWeatherName() |
| FlagIndex constants | LOW | 6 new well-known indices needed (MaxTemperature=81, MinTemperature=82, etc.) |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| IWeatherSettings is in Era.Core (NuGet package) | `IWeatherSettings.cs` | Interface changes require NuGet version bump |
| @日付初期設定 uses GOTO + INPUT + PRINTBUTTON loop | `天候.ERB:6-49` | Must be excluded from migration scope; single caller (追加パッチverup.ERB:81) |
| #DIM DYNAMIC local variables (天候TIME, 天候変更) persist across calls | `天候.ERB:614-615` | Must be modeled as instance fields or IVariableStore state |
| GOTO construct in @天候状態 | `天候.ERB:655` | GOTO SKIP_天候 requires refactoring to structured control flow |
| ERB RAND(min,max) inclusive both ends vs C# exclusive upper bound | `天候.ERB:518,521,530-531` | Must use Next(min, max+1) for equivalence |
| ERB integer division semantics | `天候.ERB:579-600` | Must preserve truncation behavior in C# |
| Weather enum 0-12 with Japanese string names | `天候.ERB:473-503` | C# should use proper enum with string mapping |
| 天候(ARG) string function called from 7 external ERB files | 18 occurrences across COMF404, EVENTCOMEND, INFO, etc. | Must be externally callable |
| IEngineVariables indexed DAY/TIME access gap | `IEngineVariables.cs:108-114` | TIME:1, DAY:1, DAY:2 may require GetTime(int)/GetDay(int) or IVariableStore raw access |
| WeatherSettings registered as Singleton | `ServiceCollectionExtensions.cs:165` | Mutable state via IVariableStore only; no instance-level mutable fields |
| TreatWarningsAsErrors | `Directory.Build.props` | Must be warning-free |
| 5 internal-only functions should be private | `天候.ERB` analysis | @異常気象, @年間基礎雷発生確率, @年間基礎降水確率, @年間基礎最高気温, @年間基礎最低気温 |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| ISP violation if all methods on single IWeatherSettings | HIGH | MEDIUM | Design interface split during tech design (CalendarService, ClimateData, WeatherSimulation, WeatherEffects) |
| Scope explosion: 839 lines is large for single feature | HIGH | HIGH | Partition into sub-tasks; 384 lines are pure data tables (trivially migratable) |
| RAND semantics mismatch causing off-by-one | MEDIUM | HIGH | Use Next(min, max+1) consistently; equivalence tests verify |
| IEngineVariables indexed access gap blocks calendar/weather | MEDIUM | MEDIUM | Verify during tech design; fall back to IVariableStore raw array access if needed |
| NuGet version coordination with core repo | MEDIUM | LOW | Standard cross-repo process |
| #DIM DYNAMIC state persistence modeled incorrectly | MEDIUM | MEDIUM | Use IVariableStore for persistence; document state lifetime |
| Weather state machine complexity (probabilistic transitions) | MEDIUM | MEDIUM | Thorough equivalence tests with seeded RNG |
| @日付初期設定 accidentally included in scope | LOW | LOW | Explicitly excluded in Goal; tracked for future migration |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ERB function count | grep -c "^@" 天候.ERB | 15 | Total functions in source file |
| Migrated function count | grep -c method IWeatherSettings.cs | 1 | Only TemperatureResistance() |
| ERB line count | wc -l 天候.ERB | 839 | Total lines |
| External callers | grep -r "天候" *.ERB | 7 files, 18 occurrences | Weather string function consumers |
| Existing test count | find WeatherSettingsTests -name "*.cs" | 0 | No dedicated test file exists |

**Baseline File**: `_out/tmp/baseline-821.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | @子供気温耐性取得 must be separately callable from Pregnancy subsystem | `PREGNACY_S.ERB:426` | AC must verify as distinct method with single-child parameter |
| C2 | Weather enum (0-12) must return correct Japanese strings for all 13 values | `天候.ERB:473-503` | AC must verify all 13 mappings |
| C3 | Temperature tables: 6 months x 30 days of lookup data (max/min temp, precipitation, thunder) | `天候.ERB:185-470` | ACs must cover boundary values with parametric tests |
| C4 | Weather state machine has probabilistic transitions dependent on TIME_PROGRESS | `天候.ERB:613-724` | Need deterministic RNG seeding for equivalence tests |
| C5 | Status effects modify BASE:体力 and BASE:気力 with OPENPLACE filter | `天候.ERB:759-764` | AC must verify reduction formulas and location filtering |
| C6 | @日間気温設定 RAND range: ERB RAND(min,max) inclusive both ends | `天候.ERB:518-521` | AC#7 verifies Next(min, max+1) syntax via code grep; AC#26 verifies behavioral equivalence with seeded RNG |
| C7 | @現在気温設定 integer division across 9 time bands | `天候.ERB:571-610` | AC must verify truncation behavior for each time band |
| C8 | Existing TemperatureResistance() must remain backward-compatible | `GameInitialization.cs:345` | Cannot break existing contract |
| C9 | @日付初期設定 explicitly excluded from migration scope | `天候.ERB:6-49` | Must track as deferred obligation, not create AC |
| C10 | Zero-debt (no TODO/FIXME/HACK) | Feature template | AC for grep verification |
| C11 | Engine type: xUnit tests verify C# implementation against ERB-derived expected values | CLAUDE.md (engine = xUnit直接実行) | ACs use dotnet test with deterministic inputs matching ERB logic |
| C12 | 6 new FlagIndex constants required | `FLAG.yaml:81-89,468-473` | Interface Dependency Scan |
| C13 | @異常気象 must be private (not exposed on interface); 4 climate lookup functions exposed on IClimateDataService for cross-class access per ISP split | ERB call analysis + Technical Design ISP decision | AC#9 verifies @異常気象 private; 4 climate functions are public on IClimateDataService because WeatherSimulation depends on them |

### Constraint Details

**C1: Child Temperature Resistance Callable**
- **Source**: `PREGNACY_S.ERB:426` -- `CALL 子供気温耐性取得, 子供`
- **Verification**: grep for CALL to 子供気温耐性取得 across all ERB files
- **AC Impact**: Must have dedicated AC verifying single-child temperature resistance method exists and is callable with child ID parameter

**C2: Weather Enum Completeness**
- **Source**: `天候.ERB:477-503` -- SELECTCASE with 13 values (0=快晴 through 12=桃霧)
- **Verification**: Count CASE statements in @天候 function
- **AC Impact**: Parametric test covering all 13 enum-to-string mappings

**C3: Temperature Table Accuracy**
- **Source**: `天候.ERB:185-470` -- 4 lookup functions with SELECTCASE over 6 months, each with 30 day ranges
- **Verification**: Compare C# output against ERB CASE values
- **AC Impact**: Boundary value tests for month transitions and day range edges

**C4: Deterministic State Machine Testing**
- **Source**: `天候.ERB:613-724` -- uses RAND and TIME_PROGRESS for state transitions
- **Verification**: Seed IRandomProvider, verify output sequence matches ERB
- **AC Impact**: Must inject IRandomProvider for deterministic testing; test state transition sequences

**C5: Status Effect Formulas**
- **Source**: `天候.ERB:728-772` -- iterates CHARANUM, checks OPENPLACE, modifies BASE
- **Verification**: Compare reduction values against ERB formulas
- **AC Impact**: Must mock ILocationService.IsOpenPlace() and verify BASE modifications

**C6: RAND Range Semantics**
- **Source**: ERB RAND(min,max) inclusive both ends; IRandomProvider.Next(min,max) exclusive upper
- **Verification**: AC#7 verifies code syntax pattern (grep for `.Next(*, *+1)`); AC#26 verifies behavioral output equivalence with seeded RNG
- **AC Impact**: Two-layer verification: static code check (AC#7) + behavioral equivalence (AC#26)

**C7: Integer Division Preservation**
- **Source**: `天候.ERB:579-600` -- temperature interpolation uses integer arithmetic
- **Verification**: Compare C# integer division output against ERB for each time band
- **AC Impact**: All 9 time bands must produce identical truncated results

**C8: Existing TemperatureResistance() Backward Compatibility**
- **Source**: `GameInitialization.cs:345` -- calls `_weatherSettings.TemperatureResistance()`
- **Verification**: Grep IWeatherSettings.cs for unchanged method signature
- **AC Impact**: AC#10 verifies signature preserved with count_equals 1

**C9: @日付初期設定 Excluded from Scope**
- **Source**: `天候.ERB:6-49` -- uses GOTO + INPUT + PRINTBUTTON interactive loop; single caller (`追加パッチverup.ERB:81`)
- **Verification**: No AC created for this function; tracked as deferred obligation in Mandatory Handoffs
- **AC Impact**: No AC; Task#14 creates deferred feature

**C10: Zero-Debt**
- **Source**: Feature template -- no TODO/FIXME/HACK in new weather code
- **Verification**: Grep across 3 new implementation files
- **AC Impact**: AC#12 verifies no debt markers in WeatherSimulation.cs, CalendarService.cs, ClimateDataService.cs

**C11: Engine Type xUnit Tests**
- **Source**: CLAUDE.md engine type definition -- engine features use xUnit直接実行 (dotnet test). Feature Type changed from erb to engine because implementation targets Era.Core library code (not game runtime), all ACs use dotnet test, and no headless game execution is needed.
- **Verification**: dotnet test with Weather/Calendar/ClimateData/CurrentTemperature filter passes
- **AC Impact**: AC#11 runs full test suite; AC#1-AC#7, AC#22 verify C# behavior against ERB-derived expected values

**C12: FlagIndex Constants**
- **Source**: `FLAG.yaml:81-89,468-473` -- MaxTemperature=81, MinTemperature=82, PrecipitationProbability=83, AbnormalWeather=89, CurrentTemperature=6422, AbnormalWeatherDelay=6424
- **Verification**: Verify FLAG.yaml indices match constants
- **AC Impact**: AC must verify FlagIndex contains all 6 new constants

**C13: Private Functions (Updated per ISP Split)**
- **Source**: ERB call analysis -- @異常気象 called only by @日間気温設定 (internal to WeatherSimulation); @年間基礎雷発生確率, @年間基礎降水確率, @年間基礎最高気温, @年間基礎最低気温 called only internally within 天候.ERB but exposed on IClimateDataService per ISP split decision because WeatherSimulation depends on cross-class access
- **Verification**: grep confirms @異常気象 is private on WeatherSimulation; 4 climate functions are public on IClimateDataService (intentional)
- **AC Impact**: AC#9 verifies @異常気象 is private. No AC needed for 4 climate functions since they are intentionally public per Technical Design Key Decision (3-class ISP split)

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning |
| Successor | F822 | [DRAFT] | Pregnancy System depends on @子供気温耐性取得 (PREGNACY_S.ERB:426 CALL) |
| Successor | F825 | [DRAFT] | DI Integration depends on new interface registrations |
| Successor | F826 | [DRAFT] | Post-Phase Review |
| Related | F377 | [DONE] | Origin -- original IWeatherSettings extraction |
| Related | F797 | [DONE] | IVariableStore migration (prerequisite infrastructure) |
| Related | F782 | [DONE] | IVariableStoreExtensions (GetCFlag/SetCFlag) |
| Related | F796 | [DONE] | Pattern source -- BodyDetailInit migration pattern |
| Related | F808 | [DONE] | Lesson source -- ERB boundary != domain boundary |
| Related | F819 | [PROPOSED] | Sibling -- no call-chain dependency |
| Related | F823 | [WIP] | Sibling -- no call-chain dependency |
| Related | F824 | [WIP] | Sibling -- no call-chain dependency |

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

<!-- fc-phase-2-completed -->

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "All ERB state subsystems must be migrated to C#" | All 13 in-scope functions from 天候.ERB have C# implementations (@日付初期設定 deferred per C9/AC#17/AC#25) | AC#1, AC#2, AC#3, AC#4, AC#5, AC#6, AC#7, AC#22, AC#26 |
| "ISP-compliant interfaces" | Weather functionality split across 4 interfaces; @異常気象 private to WeatherSimulation (4 climate functions intentionally public on IClimateDataService per ISP cross-class access) | AC#8, AC#9 |
| "equivalence-tested against ERB baselines" | xUnit tests verify C# output matches ERB-derived expected values with deterministic/seeded random inputs (existing TemperatureResistance equivalence is F377's responsibility — F821 scope is 13 newly-migrated functions only, AC#10 preserves existing contract) | AC#1, AC#2, AC#3, AC#4, AC#6, AC#11 (integration gate), AC#22, AC#26 |
| "zero-debt implementation" | No TODO/FIXME/HACK in new weather code | AC#12 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Weather enum maps all 13 values (0-12) to correct Japanese strings | test | dotnet test --filter "FullyQualifiedName~WeatherEnum" | gte | 13 | [x] |
| 2 | Climate lookup tables return correct boundary values for month/day transitions | test | dotnet test --filter "FullyQualifiedName~ClimateData" | gte | 16 | [x] |
| 3 | Weather state machine transitions produce deterministic results with seeded RNG | test | dotnet test --filter "FullyQualifiedName~WeatherState" | gte | 10 | [x] |
| 4 | Status effects apply correct BASE reductions with OPENPLACE filtering | test | dotnet test --filter "FullyQualifiedName~WeatherEffect" | gte | 3 | [x] |
| 5 | ChildTemperatureResistance is separately callable with single child ID parameter | code | Grep(src/Era.Core/Interfaces/ICalendarService.cs, pattern="void ChildTemperatureResistance\\(int") | count_equals | 1 | [x] |
| 6 | CurrentTemperature calculation preserves integer division across 9 time bands | test | dotnet test --filter "FullyQualifiedName~CurrentTemperature" | gte | 9 | [x] |
| 7 | Weather/Calendar RAND calls use Next(min, max+1) for ERB RAND equivalence | code | Grep(src/Era.Core/State/, pattern="\.Next\(.*,\s*.*\+\s*1\)") | gte | 7 | [x] |
| 8 | ISP split: multiple weather-domain interfaces exist (not monolithic IWeatherSettings) | code | Grep(src/Era.Core/Interfaces/, pattern="interface IWeatherSettings|interface ICalendarService|interface IClimateDataService|interface IWeatherSimulation") | gte | 4 | [x] |
| 9 | @異常気象 is private implementation detail (not exposed on any interface) | code | Grep(src/Era.Core/State/WeatherSimulation.cs, pattern="private.*AbnormalWeather|private.*CheckAbnormalWeather|private.*異常気象") | gte | 1 | [x] |
| 10 | Existing TemperatureResistance() backward compatible (signature unchanged on IWeatherSettings) | code | Grep(src/Era.Core/Interfaces/IWeatherSettings.cs, pattern="void TemperatureResistance.*int characterCount.*Random rng") | count_equals | 1 | [x] |
| 11 | Equivalence test suite passes with all weather, calendar, climate, and temperature tests green | test | dotnet test --filter "FullyQualifiedName~Weather\|FullyQualifiedName~CalendarService\|FullyQualifiedName~ClimateData\|FullyQualifiedName~CurrentTemperature\|FullyQualifiedName~DailyTemperature" --blame-hang-timeout 10s | succeeds | exit code 0 | [x] |
| 12 | No TODO/FIXME/HACK/??? in weather implementation and interface files | code | Grep(src/Era.Core/State/WeatherSimulation.cs src/Era.Core/State/CalendarService.cs src/Era.Core/State/ClimateDataService.cs src/Era.Core/Interfaces/ICalendarService.cs src/Era.Core/Interfaces/IClimateDataService.cs src/Era.Core/Interfaces/IWeatherSimulation.cs, pattern="TODO|FIXME|HACK|\\?\\?\\?") | not_matches | `TODO|FIXME|HACK|\\?\\?\\?` | [x] |
| 13 | FlagIndex contains all 6 new weather constants | code | Grep(src/Era.Core/Types/FlagIndex.cs, pattern="MaxTemperature|MinTemperature|PrecipitationProbability|AbnormalWeather|CurrentTemperature|AbnormalWeatherDelay") | gte | 6 | [x] |
| 14 | New weather-domain interfaces registered in DI container | code | Grep(src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs, pattern="AddSingleton.*IWeatherSettings|AddSingleton.*ICalendarService|AddSingleton.*IClimateDataService|AddSingleton.*IWeatherSimulation") | gte | 4 | [x] |
| 15 | Solution builds without warnings | build | dotnet build --no-restore | succeeds | exit code 0 | [x] |
| 16 | SSOT update: engine-dev INTERFACES.md lists new weather interfaces | code | Grep(.claude/skills/engine-dev/INTERFACES.md, pattern="ICalendarService|IClimateDataService|IWeatherSimulation") | gte | 3 | [x] |
| 17 | Deferred @日付初期設定 feature DRAFT file created | code | Grep(pm/features/, pattern="日付初期設定") | gte | 1 | [x] |
| 18 | WeatherSimulation has correct constructor DI injection (7 deps) | code | Grep(src/Era.Core/State/WeatherSimulation.cs, pattern="IVariableStore\|IEngineVariables\|IRandomProvider\|ICounterUtilities\|ILocationService\|IClimateDataService\|IConsoleOutput") | gte | 7 | [x] |
| 19 | WeatherTime and WeatherChangeInterval FlagIndex constants exist | code | Grep(src/Era.Core/Types/FlagIndex.cs, pattern="WeatherTime\|WeatherChangeInterval") | gte | 2 | [x] |
| 20 | BaseIndex.Stamina and BaseIndex.Vitality constants exist | code | Grep(src/Era.Core/Types/BaseIndex.cs, pattern="Stamina\|Vitality") | gte | 2 | [x] |
| 21 | CharacterFlagIndex.CurrentLocation constant exists | code | Grep(src/Era.Core/Types/CharacterFlagIndex.cs, pattern="CurrentLocation") | gte | 1 | [x] |
| 22 | CalendarService equivalence tests pass (AdvanceDate wrapping, GetMonthName mappings, ChildTemperatureResistance) | test | dotnet test --filter "FullyQualifiedName~CalendarService" --blame-hang-timeout 10s | gte | 13 | [x] |
| 23 | CalendarService has correct constructor DI injection (3 deps) | code | Grep(src/Era.Core/State/CalendarService.cs, pattern="IVariableStore\|IEngineVariables\|IRandomProvider") | gte | 3 | [x] |
| 24 | IEngineVariables indexed DAY/TIME access methods exist | code | Grep(src/Era.Core/Interfaces/IEngineVariables.cs, pattern="GetDay.*int index\|SetDay.*int index\|GetTime.*int index\|SetTime.*int index") | gte | 4 | [x] |
| 25 | Deferred @日付初期設定 registered in index-features.md | code | Grep(pm/index-features.md, pattern="日付初期設定") | gte | 1 | [x] |
| 26 | SetDailyTemperature produces correct FLAG:MaxTemperature and FLAG:MinTemperature with seeded RNG | test | dotnet test --filter "FullyQualifiedName~DailyTemperature" --blame-hang-timeout 10s | gte | 3 | [x] |
| 27 | Negative/boundary test cases exist for weather domain methods | code | Grep(src/Era.Core.Tests/State/, pattern="Invalid|OutOfRange|Throw|ArgumentException|boundary") | gte | 1 | [x] |
| 28 | SSOT update: engine-dev PATTERNS.md lists new weather Type constants and IEngineVariables indexed methods | code | Grep(.claude/skills/engine-dev/PATTERNS.md, pattern="MaxTemperature|Stamina|CurrentLocation|GetDay.*int index") | gte | 4 | [x] |

### AC Details

**AC#1: Weather enum maps all 13 values (0-12) to correct Japanese strings**
- **Test**: `dotnet test --filter "FullyQualifiedName~WeatherEnum" --blame-hang-timeout 10s`
- **Expected**: >= 13 passing tests (one per enum value: 0=快晴, 1=晴れ, 2=曇り, 3=雨, 4=雪, 5=雷雨, 6=大雨, 7=大雪, 8=嵐, 9=吹雪, 10=霧, 11=暗闇, 12=桃霧; verified from 天候.ERB:477-503)
- **Derivation**: 13 SELECTCASE values in 天候.ERB:473-503 (0 through 12), 1:1 mapping required per C2
- **Rationale**: C2 requires all 13 enum-to-string mappings verified; parametric test coverage prevents missing/swapped values

**AC#2: Climate lookup tables return correct boundary values for month/day transitions**
- **Test**: `dotnet test --filter "FullyQualifiedName~ClimateData" --blame-hang-timeout 10s`
- **Expected**: >= 16 passing tests (4 functions x 4 boundary cases: 2 month boundaries + 2 intra-month day-range edges)
- **Derivation**: 4 climate lookup functions (年間基礎最高気温, 年間基礎最低気温, 年間基礎降水確率, 年間基礎雷発生確率) each with 6-month SELECTCASE containing intra-month day-range sub-boundaries (e.g., month 1: days 1-3, 4-5, 6-8, ...). C3 requires "boundary values for month transitions AND day range edges" — both types must be covered.
- **Rationale**: C3 requires both month-boundary AND intra-month day-range edge coverage; 4 functions x (2 month boundaries + 2 day-range edges) = 16 minimum

**AC#3: Weather state machine transitions produce deterministic results with seeded RNG**
- **Test**: `dotnet test --filter "FullyQualifiedName~WeatherState" --blame-hang-timeout 10s`
- **Expected**: >= 10 passing tests covering 10 distinct behavioral paths: (1) storm onset cold→吹雪, (2) storm onset warm→嵐, (3) storm decay (timer crosses 0), (4) no-change (time check fails), (5) abnormal weather GOTO SKIP, (6) rain clearing, (7) cloudy→rain, (8) cloudy→snow (cold), (9) cloudy→thunderstorm, (10) fair weather transitions (clear↔sunny↔cloudy) + heavy rain/snow upgrade
- **Derivation**: @天候状態 (天候.ERB:613-724, 111 lines) has nested IF/ELSEIF structure with 10 distinct behavioral paths per ERB source analysis. Cloudy→precipitation splits into 3 sub-branches (rain/snow/thunderstorm, lines 665-681). Philosophy requires equivalence-tested against ERB baselines; each behavioral path requires at least one test with seeded RNG per C4
- **Rationale**: C4 requires deterministic RNG seeding for equivalence tests; sub-branch coverage ensures precipitation path equivalence

**AC#4: Status effects apply correct BASE reductions with OPENPLACE filtering**
- **Test**: `dotnet test --filter "FullyQualifiedName~WeatherEffect" --blame-hang-timeout 10s`
- **Expected**: >= 3 passing tests (outdoor reduction, indoor no-effect, formula verification)
- **Derivation**: @天候によるステータス増減処理 (天候.ERB:728-772) has 3 key paths: character in open place gets reduction, character indoors is unaffected, and reduction formula matches ERB per C5
- **Rationale**: C5 requires verification of reduction formulas and OPENPLACE location filtering

**AC#6: CurrentTemperature calculation preserves integer division across 9 time bands**
- **Test**: `dotnet test --filter "FullyQualifiedName~CurrentTemperature" --blame-hang-timeout 10s`
- **Expected**: >= 9 passing tests (one per time band)
- **Derivation**: @現在気温設定 (天候.ERB:571-610) has 9 SELECTCASE time bands, each with different integer division formula per C7; all 9 must produce identical truncated results
- **Rationale**: C7 requires verification of truncation behavior for each time band; integer division mismatch causes silent weather inaccuracy

**AC#7: Weather/Calendar RAND calls use Next(min, max+1) for ERB RAND equivalence**
- **Test**: `Grep(src/Era.Core/State/, pattern="\.Next\(.*,\s*.*\+\s*1\)")`
- **Expected**: >= 7 matches (one per ERB RAND call)
- **Derivation**: 天候.ERB contains 7 RAND(min,max) calls: line 518 (最高気温 RAND(-3,3)), 520 (最低気温 RAND(-3,3)), 532 (異常気象 最高気温 RAND(3,5)), 534 (異常気象 LOCAL RAND(3,5)), 542 (異常気象 最高気温 RAND(3,5)), 627 (異常気象発生ディレイ RAND(240,720)), 722 (天候変更 RAND(10,30)). Each ERB RAND(min,max) maps to one `_rng.Next(min, max+1)` call in C#.
- **Rationale**: C6 requires all ERB RAND calls to use inclusive-inclusive semantics via Next(min, max+1); count verification ensures no RAND call is missed or silently uses wrong bounds

**AC#5: ChildTemperatureResistance is separately callable with single child ID parameter**
- **Test**: `Grep(src/Era.Core/Interfaces/ICalendarService.cs, pattern="void ChildTemperatureResistance\\(int")`
- **Expected**: count_equals 1 (exactly one matching method declaration on ICalendarService)
- **Derivation**: C1 requires @子供気温耐性取得 as distinct method with single-child parameter. `PREGNACY_S.ERB:426` calls `CALL 子供気温耐性取得, 子供` — F822 depends on this method being separately callable. Exactly 1 match confirms the method exists on the correct interface without duplication.
- **Rationale**: C1 interface existence check; count_equals prevents duplication; parameter-name-agnostic pattern avoids brittle matching

**AC#8: ISP split: multiple weather-domain interfaces exist (not monolithic IWeatherSettings)**
- **Test**: `Grep(src/Era.Core/Interfaces/, pattern="interface IWeatherSettings|interface ICalendarService|interface IClimateDataService|interface IWeatherSimulation")`
- **Expected**: >= 4 (IWeatherSettings + ICalendarService + IClimateDataService + IWeatherSimulation)
- **Derivation**: 13 in-scope functions span 4 distinct domains per Technical Design: temperature resistance (existing IWeatherSettings), calendar management (ICalendarService), climate lookup (IClimateDataService), weather simulation+effects (IWeatherSimulation). 4 interfaces total.
- **Rationale**: Philosophy requires ISP-compliant interfaces; monolithic expansion of IWeatherSettings would violate SRP per F808 lesson

**AC#9: @異常気象 is private implementation detail**
- **Test**: `Grep(src/Era.Core/State/WeatherSimulation.cs, pattern="private.*AbnormalWeather|private.*CheckAbnormalWeather|private.*異常気象")`
- **Expected**: >= 1 (at least one private method for the @異常気象 function)
- **Derivation**: ERB call analysis confirms @異常気象 is called only by @日間気温設定 (internal). Per C13, it must not appear on any public interface. Verified as private method on WeatherSimulation.
- **Rationale**: C13 requires internal-only functions not exposed on interfaces; private visibility enforces this

**AC#10: Existing TemperatureResistance() backward compatible**
- **Test**: `Grep(src/Era.Core/Interfaces/IWeatherSettings.cs, pattern="void TemperatureResistance.*int characterCount.*Random rng")`
- **Expected**: count_equals 1 (exactly one matching method signature — the existing one)
- **Derivation**: C8 requires the existing contract at `GameInitialization.cs:345` remains unchanged. Exactly 1 match confirms the signature is preserved and not duplicated.
- **Rationale**: Regression guard — any signature change would break GameInitialization callers

**AC#13: FlagIndex contains all 6 new weather constants**
- **Test**: `Grep(src/Era.Core/Types/FlagIndex.cs, pattern="MaxTemperature|MinTemperature|PrecipitationProbability|AbnormalWeather|CurrentTemperature|AbnormalWeatherDelay")`
- **Expected**: >= 6 matches (MaxTemperature=81, MinTemperature=82, PrecipitationProbability=83, AbnormalWeather=89, CurrentTemperature=6422, AbnormalWeatherDelay=6424)
- **Derivation**: 6 FLAG indices identified in C12 from FLAG.yaml:81-89,468-473; each requires a FlagIndex constant for type-safe access
- **Rationale**: C12 requires all 6 constants exist; without them, raw integer indices would bypass type safety

**AC#14: New weather-domain interfaces registered in DI container**
- **Test**: `Grep(src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs, pattern="AddSingleton.*IWeatherSettings|AddSingleton.*ICalendarService|AddSingleton.*IClimateDataService|AddSingleton.*IWeatherSimulation")`
- **Expected**: >= 4 registrations (IWeatherSettings + ICalendarService + IClimateDataService + IWeatherSimulation)
- **Derivation**: Each ISP-split interface requires DI registration; 4 interfaces per AC#8 = 4 registrations
- **Rationale**: New interfaces without DI registration would fail at runtime resolution; must match AC#8 interface count

**AC#16: SSOT update: engine-dev INTERFACES.md lists new weather interfaces**
- **Test**: `Grep(.claude/skills/engine-dev/INTERFACES.md, pattern="ICalendarService|IClimateDataService|IWeatherSimulation")`
- **Expected**: >= 3 (one entry per new interface)
- **Derivation**: 3 new interfaces created per AC#8. Per ssot-update-rules.md, new interfaces in `src/Era.Core/Interfaces/` require INTERFACES.md documentation.
- **Rationale**: SSOT compliance prevents undocumented interfaces from being invisible to engine-dev skill consumers

**AC#17: Deferred @日付初期設定 feature DRAFT file created**
- **Test**: `Grep(pm/features/, pattern="日付初期設定")`
- **Expected**: >= 1 (a feature file in pm/features/ contains 日付初期設定)
- **Derivation**: C9 requires @日付初期設定 exclusion to be tracked. Per Deferred Task Protocol and Mandatory Handoffs DRAFT Creation Checklist, a new feature-{ID}.md must be created.
- **Rationale**: Track What You Skip principle — defer is OK, forget is not

**AC#25: Deferred @日付初期設定 registered in index-features.md**
- **Test**: `Grep(pm/index-features.md, pattern="日付初期設定")`
- **Expected**: >= 1 (index-features.md contains 日付初期設定 entry)
- **Derivation**: Per Deferred Task Protocol, deferred feature must be registered in index-features.md for visibility.
- **Rationale**: Registration ensures the deferred feature is discoverable and not forgotten

**AC#26: SetDailyTemperature equivalence test with seeded RNG**
- **Test**: `dotnet test --filter "FullyQualifiedName~DailyTemperature" --blame-hang-timeout 10s`
- **Expected**: >= 3 passing tests (normal temperature set from climate tables + RAND, AbnormalWeather branch temperature modification, equivalence with ERB-derived expected values)
- **Derivation**: @日間気温設定 (天候.ERB:505-568) sets FLAG:最高気温 and FLAG:最低気温 from climate lookup tables with RAND variation, and invokes @異常気象 for abnormal weather modification. Philosophy requires equivalence-tested against ERB baselines for all migrated functions.
- **Rationale**: Without AC#26, SetDailyTemperature is one of 13 in-scope functions with zero behavioral equivalence coverage — only AC#7 (RAND syntax grep) existed, which does not verify output values

**AC#18: WeatherSimulation constructor DI injection verified**
- **Test**: `Grep(src/Era.Core/State/WeatherSimulation.cs, pattern="IVariableStore|IEngineVariables|IRandomProvider|ICounterUtilities|ILocationService|IClimateDataService|IConsoleOutput")`
- **Expected**: >= 7 (one per constructor dependency)
- **Derivation**: WeatherSimulation requires 7 dependencies per Technical Design class layout. Each must appear as constructor parameter or field for DI injection.
- **Rationale**: V2 constructor injection rule — every constructor parameter must be verifiable

**AC#22: CalendarService equivalence tests pass**
- **Test**: `dotnet test --filter "FullyQualifiedName~CalendarService" --blame-hang-timeout 10s`
- **Expected**: >= 13 passing tests (AdvanceDate: day wrap, month wrap, day+month wrap, normal advance = 4; GetMonthName: 6 month-name mappings; ChildTemperatureResistance: normal child init, boundary child ID, ERB equivalence verification = 3)
- **Derivation**: Philosophy requires "equivalence-tested against ERB baselines" for all migrated functions. @日付変更 has 4 key paths (day overflow, month overflow, combined day+month overflow, normal advance) = 4, @日付_月 has 6 month values, @子供気温耐性取得 has 3 behavioral paths (normal, boundary, ERB equivalence). Total >= 13 minimum.
- **Rationale**: Without CalendarService tests, 3 of 13 migrated functions would have zero behavioral coverage, violating the Philosophy's equivalence-testing requirement

**AC#23: CalendarService has correct constructor DI injection**
- **Test**: `Grep(src/Era.Core/State/CalendarService.cs, pattern="IVariableStore|IEngineVariables|IRandomProvider")`
- **Expected**: >= 3 (one per constructor dependency)
- **Derivation**: CalendarService requires 3 dependencies per Technical Design final class layout: IVariableStore (flag access), IEngineVariables (DAY/TIME access), IRandomProvider (ChildTemperatureResistance RAND)
- **Rationale**: Constructor injection verification ensures all dependencies are present; missing dep would cause runtime DI resolution failure

**AC#24: IEngineVariables indexed DAY/TIME access methods exist**
- **Test**: `Grep(src/Era.Core/Interfaces/IEngineVariables.cs, pattern="GetDay.*int index|SetDay.*int index|GetTime.*int index|SetTime.*int index")`
- **Expected**: >= 4 (GetDay, SetDay, GetTime, SetTime)
- **Derivation**: Key Decision chose option A (add indexed methods to IEngineVariables). CalendarService and WeatherSimulation need DAY/TIME array access via `GetDay(int index)`/`SetDay(int index, int value)`/`GetTime(int index)`/`SetTime(int index, int value)`. Without these methods, Task#3 has no direct AC verification.
- **Rationale**: Task#3 adds these methods; without a direct AC, the Task's output is unverifiable — downstream ACs (AC#3, AC#6, AC#7) could pass with raw IVariableStore workarounds

**AC#19: WeatherTime and WeatherChangeInterval FlagIndex constants exist**
- **Test**: `Grep(src/Era.Core/Types/FlagIndex.cs, pattern="WeatherTime|WeatherChangeInterval")`
- **Expected**: >= 2 (one constant per #DIM DYNAMIC variable)
- **Derivation**: @天候状態 (天候.ERB:613-724) uses #DIM DYNAMIC 天候TIME and 天候変更 as persistent state. These require FlagIndex constants for IVariableStore access. Task#2 [I] determines concrete values.
- **Rationale**: Without these constants, WeatherSimulation.UpdateWeatherState() cannot compile; Task#2 output must be verifiable

**AC#20: BaseIndex.Stamina and BaseIndex.Vitality constants exist**
- **Test**: `Grep(src/Era.Core/Types/BaseIndex.cs, pattern="Stamina|Vitality")`
- **Expected**: >= 2 (体力=Stamina, 気力=Vitality)
- **Derivation**: @天候によるステータス増減処理 (天候.ERB:759-764) modifies BASE:LOCAL:体力 and BASE:LOCAL:気力. Task#4 [I] determines concrete index values from BASE.csv.
- **Rationale**: Without these constants, WeatherSimulation.ApplyWeatherEffects() cannot reference the correct BASE indices

**AC#21: CharacterFlagIndex.CurrentLocation constant exists**
- **Test**: `Grep(src/Era.Core/Types/CharacterFlagIndex.cs, pattern="CurrentLocation")`
- **Expected**: >= 1
- **Derivation**: @天候によるステータス増減処理 uses CFLAG:LOCAL:現在位置 for OPENPLACE check. Task#4 [I] determines concrete index from CFLAG.csv.
- **Rationale**: Without this constant, WeatherSimulation.ApplyWeatherEffects() cannot filter outdoor characters

**AC#27: Negative/boundary test cases exist for weather domain methods**
- **Test**: `Grep(src/Era.Core.Tests/State/, pattern="Invalid|OutOfRange|Throw|ArgumentException|boundary")`
- **Expected**: >= 1
- **Derivation**: Engine type requires both positive and negative test coverage per testing SKILL. Weather domain switch expressions (GetWeatherName 0-12, climate lookup tables) have implicit boundary conditions. At least 1 negative test verifying error behavior for out-of-range inputs.
- **Rationale**: Ensures migration from ERB (no error handling) to C# (explicit error paths) has verified boundary behavior

**AC#28: SSOT update: engine-dev PATTERNS.md lists new weather Type constants and IEngineVariables indexed methods**
- **Test**: `Grep(.claude/skills/engine-dev/PATTERNS.md, pattern="MaxTemperature|Stamina|CurrentLocation|GetDay.*int index")`
- **Expected**: >= 4
- **Derivation**: ssot-update-rules.md requires PATTERNS.md "Strongly Typed Variable Indices" update for new `src/Era.Core/Types/*.cs` entries. Task#13 adds FlagIndex weather constants, BaseIndex.Stamina/Vitality, CharacterFlagIndex.CurrentLocation, and IEngineVariables indexed access methods.
- **Rationale**: Without PATTERNS.md update, future features lack discoverability for new well-known indices and methods

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Migrate 13 unmigrated functions (excl. @日付初期設定) from 天候.ERB to C# (@異常気象 equivalence verified indirectly via AC#26 SetDailyTemperature AbnormalWeather path) | AC#1, AC#2, AC#3, AC#4, AC#5, AC#6, AC#7, AC#11, AC#13, AC#19, AC#20, AC#21, AC#22, AC#26 |
| 2 | ISP-compliant interface split | AC#8, AC#9, AC#10, AC#14, AC#16, AC#18, AC#23, AC#24 |
| 3 | Equivalence tests against ERB baseline with deterministic/seeded random | AC#1, AC#2, AC#3, AC#4, AC#6, AC#11, AC#22, AC#26 |
| 4 | Expose @子供気温耐性取得 as separately callable method to unblock F822 | AC#5, AC#22 |
| 5 | Zero-debt implementation | AC#12, AC#15 |
| 6 | Track excluded @日付初期設定 as deferred obligation | AC#17, AC#25 |
| 7 | Negative/boundary test coverage for engine type | AC#27 |
| 8 | SSOT compliance (INTERFACES.md + PATTERNS.md) | AC#16, AC#28 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

Decompose the 13 in-scope functions from 天候.ERB across 4 ISP-split C# classes/interfaces following F808 lesson (ERB file boundary != domain boundary). The 5 internal-only functions become private methods on their implementation class. The existing `WeatherSettings.TemperatureResistance()` contract is preserved as-is.

**Interface split (4 interfaces, satisfying AC#8 >= 4):**

| Interface | Domain | Methods | ERB Source |
|-----------|--------|---------|------------|
| `IWeatherSettings` | Temperature Resistance (existing) | `TemperatureResistance(int, Random)` | @気温耐性取得 |
| `ICalendarService` | Calendar management | `AdvanceDate()`, `GetMonthName(int)`, `ChildTemperatureResistance(int)` | @日付変更, @日付_月, @子供気温耐性取得 |
| `IClimateDataService` | Climate lookup tables | `GetWeatherName(int)`, `GetMaxTemperatureBase(int, int)`, `GetMinTemperatureBase(int, int)`, `GetPrecipitationProbability(int, int)`, `GetThunderProbability(int, int)` | @天候, @年間基礎最高気温, @年間基礎最低気温, @年間基礎降水確率, @年間基礎雷発生確率 |
| `IWeatherSimulation` | Weather state machine + effects | `SetDailyTemperature()`, `SetCurrentTemperature()`, `UpdateWeatherState()`, `ApplyWeatherEffects()` | @日間気温設定, @現在気温設定, @天候状態, @天候によるステータス増減処理 |

**#DIM DYNAMIC variable handling:**
- `天候TIME` and `天候変更` in @天候状態 are function-scoped persistent state (ERB #DIM DYNAMIC). As WeatherSettings is a Singleton, these become new `FlagIndex` constants stored via `IVariableStore.GetFlag()/SetFlag()`.
- `暦法月` = `DAY:1`, `暦法日` = `DAY:2` (from DIM.ERH): require `IEngineVariables.GetDay(int index)` / `SetDay(int index, int value)`.
- `天候値` = `TIME:1` (from DIM.ERH): requires `IEngineVariables.GetTime(int index)` / `SetTime(int index, int value)`.

**IEngineVariables gap:** The current interface has only scalar `GetTime()`/`SetTime(int)` and `GetDay()`. Calendar and weather value access requires indexed overloads `GetDay(int index)`, `SetDay(int index, int value)`, `GetTime(int index)`, `SetTime(int index, int value)`. These are documented in Upstream Issues.

**RAND semantics:** ERB `RAND(min, max)` is inclusive both ends. `IRandomProvider.Next(min, max)` is exclusive upper. All RAND calls in the ERB use `Next(min, max + 1)`.

**Integer division:** C# integer division truncates like ERB. Formulas in @現在気温設定 (e.g., `FLAG:最低気温 + ((FLAG:最高気温 - FLAG:最低気温) / 5)`) are preserved using `int` arithmetic — no cast to double.

**New FlagIndex constants (6 required by AC#13 + 2 for #DIM DYNAMIC state):**

| Constant | Value | Source |
|----------|-------|--------|
| `MaxTemperature` | 81 | FLAG.yaml:81 |
| `MinTemperature` | 82 | FLAG.yaml:82 |
| `PrecipitationProbability` | 83 | FLAG.yaml:83 |
| `AbnormalWeather` | 89 | FLAG.yaml:89 |
| `CurrentTemperature` | 6422 | FLAG.yaml:6422 |
| `AbnormalWeatherDelay` | 6424 | FLAG.yaml:6424 |
| `WeatherTime` | 6425 | #DIM DYNAMIC 天候TIME |
| `WeatherChangeInterval` | 6426 | #DIM DYNAMIC 天候変更 |

Note: The AC#13 grep pattern matches exactly 6 constants. `WeatherTime` and `WeatherChangeInterval` are additional constants needed for the #DIM DYNAMIC state; they must be in FLAG.yaml or require a new entry.

**Implementation classes (SUPERSEDED — see Final class layout below):**

> Initial candidate merged CalendarService into WeatherSettings. Rejected per Key Decision: "SRP: calendar management is a distinct domain from temperature resistance."

**Final class layout:**

| Class | Interfaces | File | Deps |
|-------|-----------|------|------|
| `WeatherSettings` (existing) | `IWeatherSettings` | `State/WeatherSettings.cs` | `IVariableStore` |
| `CalendarService` | `ICalendarService` | `State/CalendarService.cs` | `IVariableStore`, `IEngineVariables`, `IRandomProvider` |
| `ClimateDataService` | `IClimateDataService` | `State/ClimateDataService.cs` | (none) |
| `WeatherSimulation` | `IWeatherSimulation` | `State/WeatherSimulation.cs` | `IVariableStore`, `IEngineVariables`, `IRandomProvider`, `ICounterUtilities`, `ILocationService`, `IClimateDataService`, `IConsoleOutput` |

**Test classes:**

| Test Class | File | Tests |
|------------|------|-------|
| `WeatherEnumTests` | `Era.Core.Tests/State/WeatherEnumTests.cs` | 13 parametric tests (0-12 → Japanese strings) |
| `ClimateDataTests` | `Era.Core.Tests/State/ClimateDataTests.cs` | Boundary tests for 4 lookup functions |
| `WeatherStateTests` | `Era.Core.Tests/State/WeatherStateTests.cs` | State machine with seeded IRandomProvider |
| `WeatherEffectTests` | `Era.Core.Tests/State/WeatherEffectTests.cs` | Status effect tests with mock ILocationService |
| `CurrentTemperatureTests` | `Era.Core.Tests/State/CurrentTemperatureTests.cs` | 9 time-band integer division tests |
| `CalendarServiceTests` | `Era.Core.Tests/State/CalendarServiceTests.cs` | AdvanceDate wrapping, GetMonthName mappings, ChildTemperatureResistance |
| `DailyTemperatureTests` | `Era.Core.Tests/State/DailyTemperatureTests.cs` | Normal temp set, AbnormalWeather branch, ERB equivalence |

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | `ClimateDataService.GetWeatherName(int)` maps 0-12 to Japanese strings. `WeatherEnumTests` has 13 parametric `[Theory]` test cases covering each value. |
| 2 | `ClimateDataService` lookup methods (`GetMaxTemperatureBase`, `GetMinTemperatureBase`, `GetPrecipitationProbability`, `GetThunderProbability`) return correct values at month-boundary transitions AND intra-month day-range edges (SELECTCASE sub-ranges within each month). `ClimateDataTests` covers >= 16 boundary cases (4 functions x 4 boundary types). |
| 3 | `WeatherSimulation.UpdateWeatherState()` uses injected `IRandomProvider`; `WeatherStateTests` seeds a deterministic `IRandomProvider` (e.g., `SeededRandomProvider(42)`) and verifies state transitions produce the expected sequence. |
| 4 | `WeatherSimulation.ApplyWeatherEffects()` calls `ILocationService.IsOpenPlace()` to filter characters; `WeatherEffectTests` mocks `IsOpenPlace` to return 2 (outdoor) vs other values and verifies BASE reductions. |
| 5 | `ICalendarService` interface in `src/Era.Core/Interfaces/ICalendarService.cs` declares `void ChildTemperatureResistance(int)`. Grep on `ICalendarService.cs` for `void ChildTemperatureResistance\(int` returns count_equals 1 (exact method with int parameter, no brittle parameter-name dependency). |
| 6 | `WeatherSimulation.SetCurrentTemperature()` uses `IEngineVariables.GetTime()` and integer arithmetic for 9 SELECTCASE time bands. `CurrentTemperatureTests` tests each band with known max/min temp values and verifies truncated output. |
| 7 | `WeatherSimulation.SetDailyTemperature()` and `UpdateWeatherState()` use `_rng.Next(min, max + 1)` for all 7 ERB RAND(min, max) calls (天候.ERB:518,520,532,534,542,627,722). `CalendarService.ChildTemperatureResistance()` uses `_rng.Next(2)` for RAND:2 (single-arg, not matched by this pattern). Grep on `src/Era.Core/State/` for `\.Next\(.*,\s*.*\+\s*1\)` returns >= 7 matches (one per ERB RAND call). |
| 8 | `ICalendarService`, `IClimateDataService`, `IWeatherSimulation` are new interfaces added to `src/Era.Core/Interfaces/` alongside existing `IWeatherSettings`. Grep for explicit interface names returns >= 4 matches. |
| 9 | `@異常気象` is implemented as private method on `WeatherSimulation`. Grep on `src/Era.Core/State/WeatherSimulation.cs` for private AbnormalWeather/CheckAbnormalWeather method returns >= 1 match. |
| 10 | `IWeatherSettings.cs` signature `void TemperatureResistance(int characterCount, Random rng)` is unchanged. The existing `WeatherSettings.cs` implementation is not modified. |
| 11 | All `WeatherEnumTests`, `ClimateDataTests`, `WeatherStateTests`, `WeatherEffectTests`, `CurrentTemperatureTests`, `CalendarServiceTests`, `DailyTemperatureTests` pass. `dotnet test --filter "FullyQualifiedName~Weather|FullyQualifiedName~CalendarService|FullyQualifiedName~ClimateData|FullyQualifiedName~CurrentTemperature|FullyQualifiedName~DailyTemperature" --blame-hang-timeout 10s` exits 0. |
| 12 | Implementation follows zero-debt rule: no TODO/FIXME/HACK/??? in `WeatherSimulation.cs`, `CalendarService.cs`, `ClimateDataService.cs`. Grep for `TODO|FIXME|HACK|???` returns 0 matches. Covers `???` placeholder literals (FlagIndex.cs snippet uses `???` in design — AC ensures they don't leak to implementation). |
| 13 | `FlagIndex.cs` gains 6 new constants: `MaxTemperature=81`, `MinTemperature=82`, `PrecipitationProbability=83`, `AbnormalWeather=89`, `CurrentTemperature=6422`, `AbnormalWeatherDelay=6424`. Grep returns >= 6 matches. |
| 14 | `ServiceCollectionExtensions.cs` adds `AddSingleton<ICalendarService, CalendarService>()`, `AddSingleton<IClimateDataService, ClimateDataService>()`, `AddSingleton<IWeatherSimulation, WeatherSimulation>()`. Grep for explicit interface names returns >= 4 registrations. |
| 15 | All new C# files compile warning-free. `TreatWarningsAsErrors=true` enforced. |
| 16 | `.claude/skills/engine-dev/INTERFACES.md` updated with entries for `ICalendarService`, `IClimateDataService`, `IWeatherSimulation` per ssot-update-rules.md. |
| 17 | A feature file in `pm/features/` contains 日付初期設定 in its content (verified by Grep). |
| 25 | index-features.md contains 日付初期設定 entry (verified by Grep). |
| 26 | WeatherSimulation.SetDailyTemperature() tested with seeded IRandomProvider. DailyTemperatureTests verifies FLAG:MaxTemperature and FLAG:MinTemperature values match ERB @日間気温設定 expected output for normal and AbnormalWeather branches. |
| 18 | `WeatherSimulation.cs` constructor has 7 DI-injected dependencies: `IVariableStore`, `IEngineVariables`, `IRandomProvider`, `ICounterUtilities`, `ILocationService`, `IClimateDataService`, `IConsoleOutput`. Grep returns >= 7 matches. |
| 19 | `FlagIndex.cs` contains `WeatherTime` and `WeatherChangeInterval` constants for #DIM DYNAMIC state. Task#2 determines values from FLAG.yaml. |
| 20 | `BaseIndex.cs` contains `Stamina` (体力) and `Vitality` (気力) constants. Task#4 determines values from BASE.csv. |
| 21 | `CharacterFlagIndex.cs` contains `CurrentLocation` (現在位置) constant. Task#4 determines value from CFLAG.csv. |
| 22 | `CalendarServiceTests` covers @日付変更 (AdvanceDate: day wrap, month wrap, day+month wrap, normal = 4), @日付_月 (GetMonthName: 6 month-name mappings), @子供気温耐性取得 (ChildTemperatureResistance: normal, boundary, ERB equivalence = 3). `dotnet test --filter "FullyQualifiedName~CalendarService"` passes with >= 13 tests. |
| 23 | `CalendarService.cs` constructor has 3 DI-injected dependencies: `IVariableStore`, `IEngineVariables`, `IRandomProvider`. Grep returns >= 3 matches. |
| 24 | `IEngineVariables.cs` declares 4 indexed access methods: `GetDay(int index)`, `SetDay(int index, int value)`, `GetTime(int index)`, `SetTime(int index, int value)`. Grep returns >= 4 matches. |
| 27 | At least 1 negative/boundary test method exists in `src/Era.Core.Tests/State/` verifying error behavior for out-of-range weather domain inputs (e.g., invalid weather enum value, out-of-range month/day). |
| 28 | `.claude/skills/engine-dev/PATTERNS.md` updated with FlagIndex weather constants (MaxTemperature, etc.), BaseIndex.Stamina/Vitality, CharacterFlagIndex.CurrentLocation, and IEngineVariables indexed access methods (GetDay/SetDay/GetTime/SetTime). Grep returns >= 4 matches. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Class decomposition | A: Extend WeatherSettings monolithically. B: 3-class split (Calendar, Climate, Simulation). C: 4-class split (WeatherSettings + 3 new) | C: 4-class split | WeatherSettings stays minimal (backward compat); CalendarService groups Calendar+ChildResistance; ClimateDataService is pure lookup; WeatherSimulation handles stateful weather. Total: 4 classes (1 existing + 3 new) |
| #DIM DYNAMIC storage | A: Instance fields (violates Singleton mutable state constraint). B: IVariableStore FlagIndex | B: IVariableStore FlagIndex | Singleton constraint documented in Technical Constraints; all state must go through IVariableStore for persistence across calls |
| RAND semantics | A: Next(min, max) — wrong inclusive/exclusive mismatch. B: Next(min, max+1) | B: Next(min, max+1) | ERB RAND(min,max) is inclusive-inclusive; IRandomProvider.Next(min,max) is inclusive-exclusive; +1 corrects the range |
| Weather enum | A: C# enum type with [Description] attributes. B: Switch expression returning string. C: readonly Dictionary lookup | B: Switch expression | Most readable, matches ERB SELECTCASE pattern exactly, no reflection overhead |
| IEngineVariables indexed access | A: Add GetDay(int)/SetDay(int,int)/GetTime(int)/SetTime(int,int) to IEngineVariables. B: Use raw IVariableStore for DAY/TIME arrays. C: Accept that 暦法月 and 暦法日 values come from parameters | A: Add to IEngineVariables | Clean abstraction; IEngineVariables already has indexed GetTarget(int); consistent pattern; raw IVariableStore bypasses abstraction |
| Calendar service placement | A: Merge into WeatherSettings. B: Separate CalendarService class | B: Separate CalendarService | SRP: calendar management is a distinct domain from temperature resistance; CalendarService is callable by @日付変更 caller sites independently |
| 5 internal functions | A: Expose on interface as internal. B: Private methods on implementation class | B: Private methods | AC#9 explicitly requires not on public interface; ERB call analysis confirms no external callers |

### Interfaces / Data Structures

**New file: `src/Era.Core/Interfaces/ICalendarService.cs`**
```csharp
namespace Era.Core.Interfaces;

/// <summary>
/// Calendar management and child temperature resistance.
/// Migrated from 天候.ERB @日付変更, @日付_月, @子供気温耐性取得 (F821).
/// </summary>
public interface ICalendarService
{
    /// <summary>Advance calendar by one day (wraps month 6 → 1, day 30 → 1). Source: @日付変更</summary>
    void AdvanceDate();

    /// <summary>Returns Japanese season name for calendar month 1-6. Source: @日付_月</summary>
    string GetMonthName(int month);

    /// <summary>Initialize temperature resistance for a single child character. Source: @子供気温耐性取得</summary>
    void ChildTemperatureResistance(int childId);
}
```

**New file: `src/Era.Core/Interfaces/IClimateDataService.cs`**
```csharp
namespace Era.Core.Interfaces;

/// <summary>
/// Climate lookup tables for weather system.
/// Migrated from 天候.ERB @天候, @年間基礎最高気温, @年間基礎最低気温, @年間基礎降水確率, @年間基礎雷発生確率 (F821).
/// The 4 base lookup functions (@年間基礎XX) are exposed publicly because WeatherSimulation requires cross-class access (ISP split decision, C13).
/// </summary>
public interface IClimateDataService
{
    /// <summary>Returns Japanese weather name for weather value 0-12. Source: @天候</summary>
    string GetWeatherName(int weatherValue);

    /// <summary>Returns base maximum temperature for given month (1-6) and day (1-30). Source: @年間基礎最高気温 (public on IClimateDataService per ISP split, C13)</summary>
    int GetMaxTemperatureBase(int month, int day);

    /// <summary>Returns base minimum temperature for given month (1-6) and day (1-30). Source: @年間基礎最低気温 (public on IClimateDataService per ISP split, C13)</summary>
    int GetMinTemperatureBase(int month, int day);

    /// <summary>Returns precipitation probability for given month (1-6) and day (1-30). Source: @年間基礎降水確率 (public on IClimateDataService per ISP split, C13)</summary>
    int GetPrecipitationProbabilityBase(int month, int day);

    /// <summary>Returns thunder probability for given month (1-6) and day (1-30). Source: @年間基礎雷発生確率 (public on IClimateDataService per ISP split, C13)</summary>
    int GetThunderProbabilityBase(int month, int day);
}
```

**New file: `src/Era.Core/Interfaces/IWeatherSimulation.cs`**
```csharp
namespace Era.Core.Interfaces;

/// <summary>
/// Weather state machine and character status effects.
/// Migrated from 天候.ERB @日間気温設定, @現在気温設定, @天候状態, @天候によるステータス増減処理 (F821).
/// Internal function @異常気象 is private to WeatherSimulation (canonical C# name: AbnormalWeather).
/// </summary>
public interface IWeatherSimulation
{
    /// <summary>Set daily temperature bounds from climate tables + RAND. Source: @日間気温設定</summary>
    void SetDailyTemperature();

    /// <summary>Set current temperature based on time-of-day and weather. Source: @現在気温設定</summary>
    void SetCurrentTemperature();

    /// <summary>Update weather state machine (storm onset/decay, weather transitions). Source: @天候状態</summary>
    void UpdateWeatherState();

    /// <summary>Apply weather-based BASE stat reductions to outdoor characters. Source: @天候によるステータス増減処理</summary>
    void ApplyWeatherEffects();
}
```

**FlagIndex additions in `src/Era.Core/Types/FlagIndex.cs`:**
```csharp
// Weather system - Feature 821
public static readonly FlagIndex MaxTemperature = new(81);          // 最高気温
public static readonly FlagIndex MinTemperature = new(82);          // 最低気温
public static readonly FlagIndex PrecipitationProbability = new(83); // 降水確率
public static readonly FlagIndex AbnormalWeather = new(89);         // 異常気象
public static readonly FlagIndex CurrentTemperature = new(6422);    // 現在気温
public static readonly FlagIndex AbnormalWeatherDelay = new(6424);  // 異常気象発生ディレイ
// #DIM DYNAMIC state for @天候状態 (indices to verify from FLAG.yaml)
public static readonly FlagIndex WeatherTime = new(???);            // 天候TIME
public static readonly FlagIndex WeatherChangeInterval = new(???);  // 天候変更
```

Note: WeatherTime and WeatherChangeInterval index values must be verified from FLAG.yaml. These are not included in AC#13's grep pattern but are required for correct @天候状態 implementation.

Note: The `???` values are design-time placeholders. Task#2 [I] and Task#4 [I] MUST resolve all placeholder values before Task#9 (WeatherSimulation implementation). AC#12 zero-debt grep covers TODO/FIXME/HACK/??? in the 6 weather files. FlagIndex.cs `???` design-time placeholders are outside AC#12 scope — resolved values are verified by AC#13 and AC#19 which grep for specific constant names.

**IEngineVariables extensions required (Upstream Issue):**
```csharp
// Add to IEngineVariables.cs (indexed DAY/TIME array access)
int GetDay(int index);               // DAY:index — 暦法月=DAY:1, 暦法日=DAY:2
void SetDay(int index, int value);
int GetTime(int index);              // TIME:index — 天候値=TIME:1
void SetTime(int index, int value);
```

**BaseIndex additions required for weather effects (Upstream Issue):**
```csharp
// BASE:体力 and BASE:気力 used in @天候によるステータス増減処理
// Verify existing BaseIndex constants or add:
public static readonly BaseIndex Stamina = new(??);   // 体力 — verify index from BASE.csv
public static readonly BaseIndex Vitality = new(??);  // 気力 — verify index from BASE.csv
```

**CharacterFlagIndex for current location (Upstream Issue):**
```csharp
// CFLAG:LOCAL:現在位置 used in @天候によるステータス増減処理
public static readonly CharacterFlagIndex CurrentLocation = new(??); // 現在位置 — verify from CFLAG.csv
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| ~~AC#8 grep pattern~~ **RESOLVED**: AC#8 pattern updated to explicitly list all 4 interface names. Expected changed to `gte 4`. | AC#8 | Fixed by orchestrator: AC#8/AC#14 patterns now list explicit interface names instead of `IWeather*` prefix. |
| ~~AC#14 grep pattern~~ **RESOLVED**: AC#14 pattern updated to explicitly list all 4 DI registrations. Expected changed to `gte 4`. | AC#14 | Fixed by orchestrator: same fix as AC#8. |
| `IEngineVariables` lacks indexed DAY/TIME access: `暦法月` = `DAY:1`, `暦法日` = `DAY:2`, `天候値` = `TIME:1` (verified in `DIM.ERH:43-44` and `DIM.ERH:42`). Current interface has only `GetDay()` scalar and `GetTime()`/`SetTime(int value)` scalar. `CalendarService.AdvanceDate()` and `WeatherSimulation.SetCurrentTemperature()` require indexed access. | Technical Constraints | Add `GetDay(int index)`, `SetDay(int index, int value)`, `GetTime(int index)`, `SetTime(int index, int value)` to `IEngineVariables`. Also add to `NullEngineVariables` with default implementations. |
| `BaseIndex` constants for 体力 (stamina) and 気力 (vitality) are not defined in `BaseIndex.cs`. The `@天候によるステータス増減処理` function modifies `BASE:LOCAL:体力` and `BASE:LOCAL:気力`. | Technical Constraints | Add `BaseIndex.Stamina` and `BaseIndex.Vitality` with verified index values from `BASE.csv`/`BASE.yaml`. |
| `CharacterFlagIndex.CurrentLocation` constant is not defined. The `@天候によるステータス増減処理` calls `OPENPLACE(CFLAG:LOCAL:現在位置)`. | Technical Constraints | Add `CharacterFlagIndex.CurrentLocation` with verified index from `CFLAG.csv`. |
| `WeatherTime` and `WeatherChangeInterval` FlagIndex values for `#DIM DYNAMIC 天候TIME` and `#DIM DYNAMIC 天候変更` are not in `FLAG.yaml` (grep returned no match). These need new FLAG.yaml entries or a different storage strategy. | Technical Constraints | Verify if these variables have FLAG.yaml entries (possible non-standard names). If absent, add entries to `FLAG.yaml` with unused indices and corresponding `FlagIndex` constants. **Index selection**: scan FLAG.yaml for gaps above index 6424 (highest known weather-related index); use first two consecutive unused values. Validate chosen indices don't conflict with existing entries via `grep {index} FLAG.yaml`. |

<!-- fc-phase-5-completed -->

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 13 | Add 6 FlagIndex weather constants (MaxTemperature=81, MinTemperature=82, PrecipitationProbability=83, AbnormalWeather=89, CurrentTemperature=6422, AbnormalWeatherDelay=6424) to `src/Era.Core/Types/FlagIndex.cs` | | [x] |
| 2 | 13,19 | Investigate `FLAG.yaml` to find indices for `#DIM DYNAMIC 天候TIME` and `#DIM DYNAMIC 天候変更`; add `WeatherTime` and `WeatherChangeInterval` FlagIndex constants | [I] | [x] |
| 3 | 3,6,7,24 | Add indexed DAY/TIME access to `IEngineVariables` (`GetDay(int index)`, `SetDay(int index, int value)`, `GetTime(int index)`, `SetTime(int index, int value)`) and stub implementations in `NullEngineVariables` | | [x] |
| 4 | 4,20,21 | Investigate `BASE.csv`/`BASE.yaml` and `CFLAG.csv` to verify indices for `体力` (Stamina), `気力` (Vitality), and `現在位置` (CurrentLocation); add `BaseIndex.Stamina`, `BaseIndex.Vitality`, `CharacterFlagIndex.CurrentLocation` constants | [I] | [x] |
| 5 | 8 | Create `src/Era.Core/Interfaces/ICalendarService.cs`, `IClimateDataService.cs`, `IWeatherSimulation.cs` with methods as specified in Technical Design | | [x] |
| 6 | 1,2,12 | Implement `src/Era.Core/State/ClimateDataService.cs` (pure lookup: `GetWeatherName`, `GetMaxTemperatureBase`, `GetMinTemperatureBase`, `GetPrecipitationProbabilityBase`, `GetThunderProbabilityBase`; all 5 methods public on IClimateDataService per ISP split decision) | | [x] |
| 7 | 1,2 | Implement `src/Era.Core.Tests/State/WeatherEnumTests.cs` (13 parametric tests, 0-12 → Japanese strings) and `ClimateDataTests.cs` (≥16 boundary tests for 4 lookup functions) | | [x] |
| 8 | 5,7,12,22,23 | Implement `src/Era.Core/State/CalendarService.cs` (`AdvanceDate`, `GetMonthName`, `ChildTemperatureResistance(int childId)`) with deps `IVariableStore`, `IEngineVariables`, `IRandomProvider` | | [x] |
| 9 | 3,4,6,7,9,12,18,26 | Implement `src/Era.Core/State/WeatherSimulation.cs` (`SetDailyTemperature` with `Next(min, max+1)`, `SetCurrentTemperature` with 9 time-band integer division, `UpdateWeatherState` with seeded RNG, `ApplyWeatherEffects` with OPENPLACE filter; `AbnormalWeather` private) | | [x] |
| 10 | 3,4,6,11,27 | Implement `src/Era.Core.Tests/State/WeatherStateTests.cs` (≥10 state machine tests with seeded IRandomProvider covering 10 behavioral paths), `WeatherEffectTests.cs` (≥3 status effect tests with mock ILocationService), `CurrentTemperatureTests.cs` (9 time-band integer division tests), plus ≥1 negative/boundary test | | [x] |
| 11 | 14 | Register `CalendarService`, `ClimateDataService`, `WeatherSimulation` in `src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` as `AddSingleton` for their respective interfaces | | [x] |
| 12 | 10,11,15 | Run `dotnet build` (warning-free, TreatWarningsAsErrors) and `dotnet test --filter "FullyQualifiedName~Weather|FullyQualifiedName~CalendarService|FullyQualifiedName~ClimateData|FullyQualifiedName~CurrentTemperature|FullyQualifiedName~DailyTemperature" --blame-hang-timeout 10s`; verify AC#10 (IWeatherSettings signature unchanged), then all tests pass | | [x] |
| 13 | 16,28 | Update `.claude/skills/engine-dev/INTERFACES.md` to list `ICalendarService`, `IClimateDataService`, `IWeatherSimulation` with brief descriptions; update `.claude/skills/engine-dev/PATTERNS.md` with new FlagIndex weather constants, BaseIndex.Stamina/Vitality, CharacterFlagIndex.CurrentLocation, and IEngineVariables indexed access methods | | [x] |
| 14 | 17,25 | Create `pm/features/feature-{NEXT}.md` [DRAFT] for deferred @日付初期設定 migration (天候.ERB:6-49) and register in `pm/index-features.md` | | [x] |
| 15 | 22 | Implement `src/Era.Core.Tests/State/CalendarServiceTests.cs` (AdvanceDate: day wrap/month wrap/normal ≥4 tests; GetMonthName: 6 month-name mappings; ChildTemperatureResistance: normal/boundary/ERB-equivalence ≥3 tests; total ≥13) | | [x] |
| 16 | 26 | Implement `src/Era.Core.Tests/State/DailyTemperatureTests.cs` (normal temperature set, AbnormalWeather branch, ERB equivalence; ≥3 tests with seeded IRandomProvider) | | [x] |

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

**Example**:
```markdown
| 1 | 1 | Add API endpoint | | [x] |        ← KNOWN: Expected response format is specified
| 2 | 2 | Calculate aggregates | [I] | [x] | ← UNCERTAIN: Actual totals unknown until implemented
| 3 | 3 | Format report | | [x] |           ← KNOWN: Uses Task 2's output (determined after Task 2)
```

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
| 1 | implementer | sonnet | feature-821.md Task#1 | FlagIndex.cs with 6 weather constants |
| 2 | implementer | sonnet | feature-821.md Task#2, FLAG.yaml | WeatherTime + WeatherChangeInterval FlagIndex constants |
| 3 | implementer | sonnet | feature-821.md Task#3, IEngineVariables.cs, NullEngineVariables.cs | Indexed DAY/TIME overloads on IEngineVariables + NullEngineVariables stubs |
| 4 | implementer | sonnet | feature-821.md Task#4, BASE.csv/BASE.yaml, CFLAG.csv | BaseIndex.Stamina/Vitality + CharacterFlagIndex.CurrentLocation constants |
| 5 | implementer | sonnet | feature-821.md Task#5, Technical Design interfaces | ICalendarService.cs, IClimateDataService.cs, IWeatherSimulation.cs |
| 6 | implementer | sonnet | feature-821.md Task#7, 天候.ERB:185-503 (ERB source for expected values) | WeatherEnumTests.cs (13 tests), ClimateDataTests.cs (≥16 boundary tests) [TDD RED] |
| 7 | implementer | sonnet | feature-821.md Task#6, 天候.ERB:185-503 | ClimateDataService.cs (pure lookup, no deps) [TDD GREEN] |
| 8 | implementer | sonnet | feature-821.md Task#15, 天候.ERB:@日付変更/@日付_月/@子供気温耐性取得 (ERB source for expected values) | CalendarServiceTests.cs (≥13 tests) [TDD RED] |
| 9 | implementer | sonnet | feature-821.md Task#8, 天候.ERB:@日付変更/@日付_月/@子供気温耐性取得 | CalendarService.cs [TDD GREEN] |
| 10 | implementer | sonnet | feature-821.md Task#10, 天候.ERB:518-724 (ERB source for expected values) | WeatherStateTests.cs, WeatherEffectTests.cs, CurrentTemperatureTests.cs [TDD RED] |
| 11 | implementer | sonnet | feature-821.md Task#16, 天候.ERB:505-568 (ERB source for expected values) | DailyTemperatureTests.cs (≥3 tests) [TDD RED] |
| 12 | implementer | sonnet | feature-821.md Task#9, 天候.ERB:518-724 | WeatherSimulation.cs [TDD GREEN] |
| 13 | implementer | sonnet | feature-821.md Task#11, ServiceCollectionExtensions.cs | DI registrations for 3 new services |
| 14 | implementer | sonnet | feature-821.md Task#12, devkit.sln | Build + test verification |
| 15 | implementer | sonnet | feature-821.md Task#13, INTERFACES.md | SSOT update for 3 new interfaces |
| 16 | implementer | sonnet | feature-821.md Task#14, feature-template | Deferred @日付初期設定 feature [DRAFT] creation |

### Pre-conditions

- F814 [DONE] (Phase 22 Planning complete)
- F797 [DONE] (IVariableStore infrastructure available)
- F782 [DONE] (IVariableStoreExtensions: GetCFlag/SetCFlag available)
- Era.Core NuGet version bump required after interface changes (cross-repo coordination with core repo)
- FLAG.yaml entries for 天候TIME/天候変更 confirmed or new entries added to game repo FLAG.yaml (cross-repo: C:\Era\game, same coordination as NuGet version bump)

### Execution Order

Task dependency chain:
1. Task#1 (FlagIndex 6 constants) → independent
2. Task#2 [I] (WeatherTime/WeatherChangeInterval indices) → must complete before Task#9
3. Task#3 (IEngineVariables indexed access) → must complete before Task#8, Task#9
4. Task#4 [I] (BaseIndex/CharacterFlagIndex) → must complete before Task#9
5. Task#5 (3 interface files) → must complete before Task#6, Task#8, Task#9
6. Task#7 (WeatherEnum + ClimateData tests) → TDD RED; requires Task#5 interfaces
7. Task#6 (ClimateDataService) → TDD GREEN; makes Task#7 tests pass
8. Task#15 (CalendarServiceTests) → TDD RED; requires Task#3, Task#5 interfaces
9. Task#8 (CalendarService) → TDD GREEN; makes Task#15 tests pass; requires Task#3, Task#5
10. Task#10 (WeatherState/Effect/CurrentTemperature tests) → TDD RED; requires Task#5 interfaces
11. Task#16 (DailyTemperature tests) → TDD RED; requires Task#5 interfaces
12. Task#9 (WeatherSimulation) → TDD GREEN; makes Task#10, Task#16 tests pass; requires Task#2, Task#3, Task#4, Task#5, Task#6
13. Task#11 (DI registration) → requires Task#6, Task#8, Task#9
14. Task#12 (build + test gate) → final verification
15. Task#13 (SSOT INTERFACES.md update) → after Task#5
16. Task#14 (deferred @日付初期設定 DRAFT creation) → independent

### Build Verification Steps

- Each task: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build src/Era.Core/'`
- Final gate (Task#12): `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s --filter "FullyQualifiedName~Weather|FullyQualifiedName~CalendarService|FullyQualifiedName~ClimateData|FullyQualifiedName~CurrentTemperature|FullyQualifiedName~DailyTemperature"'`
- Warning check: `TreatWarningsAsErrors=true` enforced by `Directory.Build.props`
- Note: Era.Core lives in core repo (`C:\Era\core`), not devkit. All `src/Era.Core/` paths are core-repo-relative.

### Error Handling

- If IEngineVariables indexed access conflicts with existing engine implementation: STOP → report to user
- If FLAG.yaml has no entries for 天候TIME/天候変更: add new entries with unused indices (document chosen values in Task#2 output)
- If BaseIndex/CharacterFlagIndex indices conflict: STOP → report to user
- NuGet version coordination: after interface changes, bump Era.Core version and update PackageReference in devkit

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|
| @日付初期設定 migration (天候.ERB:6-49) | Uses GOTO + INPUT + PRINTBUTTON interactive loop; not feasible without UI abstraction | Feature | F828 | Task#14 | [x] | 作成済み |
| PATTERNS.md update (weather FlagIndex/BaseIndex/CharacterFlagIndex constants) | New well-known variable indices added to existing Type files | SSOT | engine-dev/PATTERNS.md | Task#13 | [x] | 確認済み |
| IEngineVariables indexed methods (GetDay/SetDay/GetTime/SetTime) | 4 new methods on IEngineVariables require engine repo implementation update after NuGet bump | Feature | F825 (DI Integration) | — | [x] | 追記済み |

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

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
| 2026-03-05T03:00:00Z | PHASE_START | orchestrator | Phase 1: Initialize | F814 [DONE], build OK, status → [WIP] |
<!-- run-phase-1-completed -->
| 2026-03-05T03:05:00Z | PHASE_START | orchestrator | Phase 2: Investigation | Explorer: all deps found, CharacterFlagIndex.CurrentLocation exists, BaseIndex.Stamina/Vitality missing, IEngineVariables needs indexed overloads |
<!-- run-phase-2-completed -->
| 2026-03-05T03:10:00Z | PHASE_START | orchestrator | Phase 3: Test Creation | [I] tasks: #2, #4 deferred. Tests need interfaces first (Task#5); following Implementation Contract order |
<!-- run-phase-3-completed -->
| 2026-03-05T03:15:00Z | PHASE_START | orchestrator | Phase 4: Implementation | Tasks 1-5 infra, Tasks 7+6 ClimateData TDD, Tasks 15+8 Calendar TDD, Tasks 10+16+9 Weather TDD, Tasks 11+12 DI+gate, Tasks 13+14 SSOT+deferred |
| 2026-03-05T03:50:00Z | PHASE_END | orchestrator | Phase 4: Implementation | All 16 tasks [x], 115 weather tests pass, build clean, F828 DRAFT created |
<!-- run-phase-4-completed -->
| 2026-03-05T03:55:00Z | Phase 5 | orchestrator | Refactoring review | SKIP (no refactoring needed — code is ERB-equivalent lookup tables + state machine; no duplication or complexity to reduce) |
<!-- run-phase-5-completed -->
| 2026-03-05T04:00:00Z | PHASE_START | orchestrator | Phase 7: Verification | AC lint OK, 27/28 PASS, AC#7 FAIL (RAND +1 pattern not in code — pre-computed literals) |
| 2026-03-05T04:00:00Z | DEVIATION | ac-tester | AC#7 verification | FAIL: grep for .Next(*, *+1) returned 0 matches; impl uses pre-computed values (e.g., Next(-3, 4) instead of Next(-3, 3+1)) |
| 2026-03-05T04:05:00Z | DEBUG_FIX | debugger | AC#7 fix | Changed 7 pre-computed RAND literals to explicit +1 form; 115 tests still pass |
| 2026-03-05T04:05:00Z | PHASE_END | orchestrator | Phase 7: Verification | 28/28 ACs PASS after debug fix |
<!-- run-phase-7-completed -->
| 2026-03-05T04:10:00Z | PHASE_START | orchestrator | Phase 8: Post-Review | Step 8.1 quality review |
| 2026-03-05T04:10:00Z | DEVIATION | feature-reviewer | Step 8.1 quality review | NEEDS_REVISION: 6 critical ERB equivalence issues in WeatherSimulation.cs (storm onset/decay logic, time check inversion, rain clearing probability, heavy rain/snow position, unreachable branches, missing zero-precip guard) |
| 2026-03-05T04:20:00Z | DEBUG_FIX | smart-implementer | Fix 6 ERB equivalence issues | Rewrote UpdateWeatherState() + tests to match ERB exactly; 124 weather tests pass |
| 2026-03-05T04:25:00Z | PHASE_END | orchestrator | Phase 8: Post-Review | Steps 8.2+8.3 OK (SSOT already updated by Task#13) |
<!-- run-phase-8-completed -->
| 2026-03-05T04:30:00Z | PHASE_START | orchestrator | Phase 9: Report | 28/28 AC PASS, 2 DEVIATION (both D=修正済み), handoffs verified |
| 2026-03-05T04:35:00Z | PHASE_END | orchestrator | Phase 10: Finalize | core:3e64df3 (3179 tests), devkit:630dbfa (806 tests) |
| 2026-03-05T04:35:00Z | CodeRabbit | Skip (cross-repo engine) | C# code in core repo, devkit has PM/doc only |
<!-- run-phase-9-completed -->
<!-- run-phase-10-completed -->

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: AC Details section | AC#8 block out of order (after AC#10); reordered to sequential AC# order
- [fix] Phase2-Review iter1: C13/Philosophy Derivation | Updated C13 to reflect ISP split: only @異常気象 is private; 4 climate functions intentionally public on IClimateDataService
- [fix] Phase2-Review iter1: AC Definition Table | Added AC#19 (WeatherTime/WeatherChangeInterval FlagIndex), AC#20 (BaseIndex.Stamina/Vitality), AC#21 (CharacterFlagIndex.CurrentLocation) for Task#2 and Task#4 coverage
- [fix] Phase2-Review iter1: Goal Coverage Row 1 | Removed AC#15/AC#18 (non-migration-specific); added AC#19
- [fix] Phase2-Review iter1: Task#2/Task#4 AC references | Updated Task#2 to AC#13,19; Task#4 to AC#4,20,21
- [fix] Phase2-Review iter2: Tasks/AC tables | Added AC#22 (CalendarService equivalence tests ≥8), Task#15 (CalendarServiceTests), CalendarServiceTests in Test Classes table
- [fix] Phase2-Review iter2: AC#11/Task#12 filter | Updated test filter to include CalendarService tests alongside Weather tests
- [fix] Phase2-Review iter2: Goal Coverage | Added AC#22 to Goal Items 1 and 3 for @日付変更/@日付_月/@子供気温耐性取得 behavioral coverage
- [fix] Phase2-Review iter3: Task#6 AC# column | Removed AC#9 (belongs to Task#9); corrected description to reflect all 5 methods public on IClimateDataService
- [fix] Phase2-Review iter3: Task#9 AC# column | Added AC#9 (AbnormalWeather private method)
- [fix] Phase2-Review iter3: Goal section | Corrected "14 unmigrated functions" to "13 in-scope functions" (14 minus excluded @日付初期設定)
- [fix] Phase2-Uncertain iter3: AC#22 | Increased ChildTemperatureResistance minimum from ≥1 to ≥3 tests; total from ≥8 to ≥10
- [fix] Phase2-Uncertain iter3: AC#7 | Extended grep scope from WeatherSimulation.cs to src/Era.Core/State/ directory (CalendarService also uses RAND via IRandomProvider)
- [fix] Phase2-Review iter3: Philosophy Derivation | Removed AC#10 from equivalence row (it's backward-compat, not equivalence); added AC#22
- [fix] Phase2-Review iter4: Task#9 AC# column | Added AC#4 (ApplyWeatherEffects implementation)
- [fix] Phase2-Review iter4: Implementation Contract Phase#15 | Updated ≥8 to ≥10 to match AC#22
- [fix] Phase2-Review iter4: IClimateDataService comment | Corrected "private to ClimateDataService" to "exposed publicly per ISP split decision"
- [fix] Phase2-Uncertain iter4: Technical Design | Marked initial class layout as SUPERSEDED; removed table to eliminate ambiguity
- [fix] Phase2-Review iter5: AC#1 Detail | Corrected weather enum mappings from ERB source (天候.ERB:477-503): 0=快晴 through 12=桃霧. Multiple values were wrong (e.g., 8=嵐 not 吹雪, 12=桃霧 not 吹雪)
- [fix] Phase2-Review iter5: Task#9 AC# column | Removed test-type ACs (AC#3,4,6) that belong to Task#10; kept code-type ACs (AC#7,9,12,18)
- [fix] Phase2-Review iter5: AC#18/AC#23 | Split AC#18 title to WeatherSimulation only; added AC#23 for CalendarService constructor DI (3 deps)
- [fix] Phase2-Review iter6: Task#7 AC# column | Removed AC#9 (belongs to Task#9); corrected to 1,2,11
- [fix] Phase2-Review iter6: Goal Coverage | Removed standalone "Build verification" row 7; AC#15 added to Goal Item 1 (migration requires compilable code)
- [fix] Phase2-Review iter7: Task#8 AC# column | Added AC#7 (CalendarService RAND usage per iter3 scope extension)
- [fix] Phase2-Review iter7: AC#11/Task#12 filter | Added ClimateData to test filter (ClimateDataTests was excluded by Weather|CalendarService pattern)
- [fix] Phase2-Review iter8: AC#11/Task#12/Build Verification | Added CurrentTemperature to test filter (CurrentTemperatureTests excluded by prior pattern); aligned AC Coverage section with AC Definition Table filter
- [fix] Phase2-Review iter9: Type | Changed erb → engine (all ACs use dotnet test/xUnit, implementation is Era.Core library code, no headless game execution needed)
- [fix] Phase2-Review iter9: C11 | Updated from "ERB headless requirement" to "engine type xUnit tests"; C11 details updated to reflect engine type rationale
- [fix] Phase2-Review iter9: Philosophy Derivation | Updated "equivalence-tested" row to include AC#1-AC#7; clarified wording to "ERB-derived expected values"
- [fix] Phase2-Review iter9: AC#7 | Tightened grep pattern from `Next.*\+ 1\)` to `\.Next\(.*,\s*.*\+\s*1\)` (two-argument Next with +1 upper bound) to reduce false positives
- [fix] Phase1-RefCheck iter10: Links section | Added F377 (legacy, no feature file) to Links section — referenced in Problem, Related Features, Dependencies, AC#10 Detail but absent from Links
- [fix] Phase2-Review iter10: AC#22 AC Definition Table + AC Details | Changed gte from 10 to 13 to match enumerated test cases (4+6+3=13); fixed AdvanceDate derivation text inconsistency
- [fix] Phase2-Review iter10: Goal Coverage Goal Item 1 | Removed AC#15 (build success ≠ migration coverage)
- [fix] Phase2-Review iter10: Tasks table Task#10 AC# column | Removed AC#7 (production code grep belongs to Task#9, not Task#10 test task)
- [fix] Phase2-Review iter10: AC#22 Details AdvanceDate | Corrected "3 key paths + 1 normal" to explicit 4 paths (day overflow, month overflow, combined overflow, normal advance)
- [fix] Phase3-Maintainability iter10: AC Coverage AC#22 | Updated ≥10 to ≥13 to match AC Definition Table
- [fix] Phase3-Maintainability iter10: AC Definition Table + Task#3 | Added AC#24 (IEngineVariables indexed DAY/TIME methods ≥4) to close Task#3 verification gap; updated Task#3 AC# column
- [fix] Phase3-Maintainability iter10: Goal Coverage Goal Item 2 | Added AC#24 to ISP-compliant interface split coverage
- [fix] Phase3-Maintainability iter10: Upstream Issues | Added FlagIndex selection criteria for WeatherTime/WeatherChangeInterval (scan above 6424, validate no conflicts)
- [fix] Phase4-ACValidation iter10: AC#17 | Changed from Glob+content to Grep(pm/features/, pattern) for tooling compatibility
- [fix] Phase4-ACValidation iter10: AC#17b → AC#25 | Renumbered non-standard 17b to integer AC#25; added AC Details, AC Coverage, Goal Coverage entries
- [fix] Phase4-ACValidation iter10: AC#12 | Changed from space-separated file paths to directory Grep(src/Era.Core/State/) for multi-file coverage
- [fix] Phase2-Review iter1: AC Definition Table | Reordered rows to ascending AC# order (18,19,20,21,22,23,24,25)
- [fix] Phase2-Review iter1: Task#15 description | Updated ≥10 to ≥13 to match AC#22 gte 13
- [fix] Phase2-Uncertain iter1: Goal Coverage Goal Item 4 | Added AC#22 to Covering AC(s)
- [fix] Phase2-Uncertain iter1: Philosophy Derivation row 1 | Added @日付初期設定 deferral acknowledgment per C9/AC#17/AC#25
- [fix] Phase2-Review iter2: AC Definition Table + Task#10 + Goal Coverage + Philosophy Derivation + Test classes + Implementation Contract | Added AC#26 (SetDailyTemperature equivalence test ≥3) to close behavioral gap for @日間気温設定
- [fix] Phase2-Review iter2: Key Decisions Class decomposition | Corrected Selected from "B: 3-class split" to "C: 4-class split" matching actual 4-class implementation
- [fix] Phase2-Review iter2: Philosophy Derivation equivalence-tested row | Removed AC#7 (code grep, not behavioral equivalence test)
- [fix] Phase2-Review iter3: Tasks table + Implementation Contract | Added Task#16 for DailyTemperatureTests.cs; removed AC#26 from Task#10; updated Phase 16 to reference Task#16
- [fix] Phase2-Review iter3: C6 constraint + C6 Details | Updated AC Implication to clarify two-layer verification (AC#7 syntax + AC#26 behavioral)
- [fix] Phase2-Review iter4: Goal Coverage Goal Item 3 | Removed AC#10 (backward compat, not equivalence test)
- [fix] Phase2-Review iter4: IWeatherSimulation interface comment | Specified canonical C# name 'AbnormalWeather' for @異常気象 private method
- [fix] Phase2-Review iter4: Philosophy Derivation equivalence row | Annotated AC#11 as (integration gate)
- [fix] Phase2-Review iter5: Implementation Contract + Execution Order | Reordered to TDD RED→GREEN (test tasks before implementation tasks)
- [fix] Phase2-Review iter5: Goal Coverage Goal Item 1 | Added note that @異常気象 equivalence verified indirectly via AC#26 AbnormalWeather path
- [fix] Phase2-Review iter6: AC#2 + C3 compliance | Increased minimum from ≥8 to ≥16 (4 functions x (2 month boundaries + 2 day-range edges)) per C3 intra-month requirement
- [fix] Phase2-Review iter7: Task#5 AC# column | Removed AC#9 (private method on WeatherSimulation.cs, not verifiable by interface creation)
- [fix] Phase2-Uncertain iter7: AC#3 minimum | Increased from ≥4 to ≥8 per ERB source analysis (8 distinct behavioral paths in @天候状態)
- [fix] Phase2-Review iter8: Task#9 AC# column | Added AC#3,4,6,26 (behavioral ACs) per RED/GREEN AC-sharing convention (Task#6/Task#7 precedent)
- [fix] Phase2-Review iter9: Task#7 AC# column | Removed AC#11 (meta-AC/integration gate belongs to Task#12 only, not TDD RED tasks)
- [fix] Phase2-Review iter9: AC#3 minimum + Details | Increased from ≥8 to ≥10; split path 7 into 3 sub-branches (rain/snow/thunderstorm)
- [fix] Phase2-Review iter9: Goal Coverage Goal Item 1 | Added AC#5 for @子供気温耐性取得 interface migration proof
- [fix] Phase2-Review iter10: AC#5 | Changed grep from fragile parameter-name pattern to robust method+type pattern (count_equals 1 on ICalendarService.cs)
- [fix] Phase3-Maintainability iter10: Pre-conditions | Added cross-repo FLAG.yaml coordination for WeatherTime/WeatherChangeInterval
- [fix] Phase3-Maintainability iter10: IClimateDataService comments | Removed misleading "(private)" annotation; replaced with ISP split intent per C13
- [fix] Phase3-Maintainability iter10: Technical Design FlagIndex | Added note about ??? placeholder resolution dependency on Task#2/Task#4
- [fix] Phase3-Maintainability iter10: Task#12 description | Added AC#10 as explicit verification step before test execution
- [fix] Phase2-Uncertain iter11: AC#7 | Changed matcher from `matches` to `gte 7`; 7 RAND calls in 天候.ERB (lines 518,520,532,534,542,627,722) each map to one `.Next(min, max+1)` call
- [fix] Phase3-Maintainability iter11: AC#12 | Extended zero-debt grep pattern to include `???` literals (prevents FlagIndex.cs snippet placeholder leak)
- [fix] Phase3-Maintainability iter11: Philosophy Derivation equivalence-tested row | Added note that TemperatureResistance equivalence is F377's responsibility; F821 scope is 13 newly-migrated functions
- [fix] Phase3-Maintainability iter1: AC#12 | Expanded zero-debt grep scope from src/Era.Core/State/ to src/Era.Core/ to cover Interfaces/, Types/, DependencyInjection/ files
- [fix] Phase2-Review iter2: Philosophy Derivation row 1 | Added AC#22, AC#26 to "All ERB state subsystems must be migrated to C#" row (CalendarService and SetDailyTemperature coverage)
- [fix] Phase2-Review iter2: AC#12 | Narrowed grep scope from src/Era.Core/ to 6 specific new files (3 implementations + 3 interfaces) to avoid false positives from existing code
- [fix] Phase2-Review iter3: AC#7 detail | Clarified ChildTemperatureResistance uses Next(2) (RAND:2 single-arg), not Next(min, max+1) pattern
- [fix] Phase2-Review iter3: Goal text | Added explicit deferred tracking language for @日付初期設定 to match Goal Coverage item 6
- [fix] Phase4-ACValidation iter4: AC#17 | Changed Type from `file` to `code` (Grep on directory is code type, not file type)
- [fix] Phase4-ACValidation iter4: AC#27 | Added negative/boundary test AC per engine type testing requirement (positive + negative coverage)
- [fix] Phase4-ACValidation iter5: AC#12 | Changed `\|` to `|` in grep pattern (ripgrep uses unescaped `|` for alternation); updated design note about AC#12 coverage
- [fix] Phase3-Maintainability iter6: Task#13 + Mandatory Handoffs | Added PATTERNS.md update to Task#13 scope; added PATTERNS.md and IEngineVariables cross-repo Mandatory Handoff entries
- [fix] Phase2-Review iter7: Build Verification Steps | Fixed cross-repo build commands from devkit.sln to core repo paths (cd /mnt/c/Era/core, dotnet build src/Era.Core/); added repo context note
- [fix] Phase2-Review iter8: AC#28 + Task#13 | Added PATTERNS.md verification AC (Grep for new constants/methods); updated Task#13 AC column, Goal Coverage, AC Coverage, AC Details

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning
- [Successor: F822](feature-822.md) - Pregnancy System (depends on @子供気温耐性取得)
- [Successor: F825](feature-825.md) - DI Integration
- [Successor: F826](feature-826.md) - Post-Phase Review
- [Related: F797](feature-797.md) - IVariableStore migration
- [Related: F782](feature-782.md) - IVariableStoreExtensions (GetCFlag/SetCFlag)
- [Related: F796](feature-796.md) - BodyDetailInit migration pattern
- [Related: F808](feature-808.md) - ERB boundary != domain boundary lesson
- [Related: F819](feature-819.md) - Sibling (no call-chain dependency)
- [Related: F823](feature-823.md) - Sibling (no call-chain dependency)
- [Related: F824](feature-824.md) - Sibling (no call-chain dependency)
- [Related: F377] - Origin -- original IWeatherSettings extraction (legacy, no feature file)
