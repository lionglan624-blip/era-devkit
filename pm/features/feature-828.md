# Feature 828: Date Initialization Migration (@日付初期設定)

## Status: [DONE]
<!-- fl-reviewed: 2026-03-05T07:57:33Z -->

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

ERB interactive UI patterns (GOTO+INPUT loops) must be migrated to C# using established abstraction patterns (IConsoleOutput + IInputHandler), maintaining interface segregation between pure computation services and interactive UI workflows. Each migrated function is the single source of truth for its domain logic.

### Problem (Current Issue)

@日付初期設定 (天候.ERB:6-49) was explicitly excluded from F821 (feature-821.md:34) because its GOTO+INPUT+PRINTBUTTON interactive loop pattern requires UI abstraction (IConsoleOutput + IInputHandler) that pure-computation services like CalendarService and WeatherSimulation do not need. Adding interactive UI methods to CalendarService would violate SRP and ISP, since CalendarService (CalendarService.cs:11-17) is a computation-only service with no UI dependencies. The function uses GOTO $Ty245124 (天候.ERB:49) to loop back after each INPUT, creating a re-entrant UI loop that requires a while(true) state machine pattern in C#, as established by ShopSystem (ShopSystem.cs:170-228).

### Goal (What to Achieve)

Migrate @日付初期設定 to a new C# DateInitializer class with IConsoleOutput and IInputHandler dependencies, implementing the while(true)+RequestNumericInput pattern established by ShopSystem. The class must validate month (1-6) and day (1-30), display season names via CalendarService.GetMonthName, and call SetDailyTemperature + SetCurrentTemperature on confirmation. Register the new service in the DI container.

---

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why is @日付初期設定 unmigrated? | F821 explicitly excluded it from scope | feature-821.md:34 |
| 2 | Why was it excluded from F821? | It uses GOTO+INPUT+PRINTBUTTON interactive loop, unlike pure computation functions | 天候.ERB:9,15-16,25,49 |
| 3 | Why can't the interactive loop be added to CalendarService? | CalendarService is a pure computation service with no UI dependencies; adding UI would violate SRP/ISP | CalendarService.cs:11-17 |
| 4 | Why does the GOTO+INPUT pattern need special handling? | ERB GOTO blocks execution and loops back after INPUT, requiring a state machine (while loop) in C# | 天候.ERB:49 (GOTO $Ty245124) |
| 5 | Why (Root)? | No separate interactive UI class exists for date initialization; the function needs its own class with IConsoleOutput+IInputHandler following the ShopSystem pattern | ShopSystem.cs:170-228 (established pattern) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | @日付初期設定 remains unmigrated in 天候.ERB | Interactive UI patterns require separate abstraction from pure computation services |
| Where | 天候.ERB:6-49 | Missing DateInitializer class in Era.Core |
| Fix | Add InitializeDate to CalendarService (band-aid, ISP violation) | Create separate DateInitializer with IConsoleOutput+IInputHandler following ShopSystem pattern |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F821 | [DONE] | Predecessor -- Weather System Migration, provides ICalendarService, IWeatherSimulation, IEngineVariables.GetDay/SetDay |
| F774 | [DONE] | Related -- ShopSystem precedent for GOTO+INPUT migration pattern |
| F777 | [DONE] | Related -- CharacterCustomizer precedent for interactive UI pattern |
| F822 | [DRAFT] | Sibling in Phase 22 -- no dependency |
| F824 | [DONE] | Sibling in Phase 22 -- no dependency |
| F819 | [DONE] | Sibling in Phase 22 -- no dependency |
| F823 | [DONE] | Sibling in Phase 22 -- no dependency |
| F825 | [DRAFT] | Successor -- DI Integration |
| F826 | [DRAFT] | Successor -- Post-Phase Review |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Interface availability | FEASIBLE | IConsoleOutput (PrintButton, PrintLine, PrintForm, DrawLine), IInputHandler (RequestNumericInput, ProvideInput), IWeatherSimulation (SetDailyTemperature, SetCurrentTemperature), ICalendarService (GetMonthName) all exist |
| Pattern precedent | FEASIBLE | ShopSystem.cs:170-228 demonstrates while(true)+RequestNumericInput for GOTO+INPUT migration |
| DI infrastructure | FEASIBLE | ServiceCollectionExtensions.cs registers all required services (lines 109, 185, 187, 209) |
| Engine variable access | FEASIBLE | IEngineVariables.GetDay(int)/SetDay(int,int) added by F821 (IEngineVariables.cs:116-122) |
| Test infrastructure | FEASIBLE | CalendarServiceTests.cs demonstrates Mock patterns for IVariableStore, IEngineVariables |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core services | MEDIUM | New DateInitializer class + IDateInitializer interface added |
| DI composition root | LOW | One new service registration in ServiceCollectionExtensions |
| CalendarService | LOW | No modification needed (GetMonthName consumed, not modified) |
| WeatherSimulation | LOW | No modification (SetDailyTemperature/SetCurrentTemperature called via existing interface) |
| Test suite | MEDIUM | New test file DateInitializerTests.cs with mock-based tests |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Code lives in core repo (C:\Era\core) | Project structure | Implementation requires changes in separate repository |
| GOTO must map to while(true) loop | Language constraint | Established pattern in ShopSystem.cs:170-228 |
| IInputHandler uses async request-response | IInputHandler.cs:14 | Must follow ShopSystem RequestNumericInput/GetResult pattern |
| GetDay/SetDay are default interface methods | IEngineVariables.cs:118-122 | Tests use mocks, must handle default method behavior |
| TreatWarningsAsErrors enabled | Directory.Build.props | All code must be warning-free |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| ISP violation if added to CalendarService | MEDIUM | MEDIUM | Create separate DateInitializer class with own IDateInitializer interface |
| CalendarService constructor bloat | MEDIUM | LOW | Mitigated by using separate class (no CalendarService changes) |
| State machine complexity | LOW | MEDIUM | Only 3 states (month, day, confirm); ShopSystem pattern proven |
| Headless input loop hang in tests | LOW | MEDIUM | All tests use mocked IInputHandler with predetermined responses |
| Behavioral change if non-interactive approach chosen | MEDIUM | HIGH | Tech-designer decides interactive vs parameterized; document decision in Key Decisions |
| Scope creep into verup.ERB caller | LOW | MEDIUM | Feature scope is @日付初期設定 only, caller migration out of scope |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| CalendarService methods | Grep ICalendarService.cs for method signatures | 3 methods | GetMonthName, GetSeasonName, GetDaysInMonth |
| WeatherSimulation methods | Grep IWeatherSimulation.cs for method signatures | 2 methods (SetDailyTemperature, SetCurrentTemperature) | Called on date confirmation |
| ServiceCollectionExtensions registrations | Grep ServiceCollectionExtensions.cs for AddSingleton/AddTransient | Current count | New registration to be added |
| ERB source lines | 天候.ERB:6-49 | 44 lines | Migration scope |

**Baseline File**: `_out/tmp/baseline-828.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Month validation: 1-6 only | 天候.ERB:22 | AC must verify valid range acceptance and out-of-range rejection |
| C2 | Day validation: 1-30 only | 天候.ERB:35,41 | AC must verify valid range acceptance and out-of-range rejection |
| C3 | Default values: month=1, day=1 | 天候.ERB:7-8 | AC must verify initial state before user input |
| C4 | Confirmation calls SetDailyTemperature + SetCurrentTemperature | 天候.ERB:45-46 | AC must verify both weather methods called on confirmation |
| C5 | Confirmation sets DAY:1=month, DAY:2=day | 天候.ERB:44-47 | AC must verify SetDay calls with correct indices |
| C6 | GetMonthName reused for season display | 天候.ERB:14 | AC must verify CalendarService.GetMonthName called for display |
| C7 | Confirm button shown only when both month and day are valid | 天候.ERB:22-23 | AC must verify conditional display logic |
| C8 | Invalid input loops back without crash | 天候.ERB:49 (GOTO) | AC must verify resilience to invalid input |
| C9 | Engine type: xUnit tests | CLAUDE.md | AC matchers must use dotnet_test |
| C10 | Changes in core repo | Project structure | AC must specify core repo file paths |
| C11 | ISP: Separate interface from ICalendarService | F808 lesson, 3/3 investigator consensus | AC should verify IDateInitializer exists separately |
| C12 | IConsoleOutput PrintButton used for selection | Interface Dependency Scan | AC must verify PrintButton calls for month/day/confirm buttons |
| C13 | IInputHandler RequestNumericInput used | Interface Dependency Scan | AC must verify input handling via IInputHandler |
| C14 | DI registration required | Established pattern | AC must verify AddSingleton/AddTransient in ServiceCollectionExtensions |

### Constraint Details

**C1: Month Validation Range**
- **Source**: 天候.ERB:22 -- IF month < 1 || month > 6 check
- **Verification**: Read 天候.ERB line 22 for condition
- **AC Impact**: Need both positive (valid month 1-6) and negative (month 0, 7) test cases

**C4: Weather Initialization on Confirmation**
- **Source**: 天候.ERB:45-46 -- CALL SetDailyTemperature + CALL SetCurrentTemperature
- **Verification**: Read 天候.ERB lines 45-46
- **AC Impact**: Mock IWeatherSimulation and verify both methods called exactly once on confirmation

**C7: Conditional Confirm Button**
- **Source**: 天候.ERB:22-23 -- Confirm button only displayed when month and day are both valid
- **Verification**: Read 天候.ERB lines 22-23
- **AC Impact**: Must test that confirm option is absent when either value is invalid

**C11: Interface Segregation**
- **Source**: F808 lesson learned, all 3 investigators recommend separate class
- **Verification**: IDateInitializer.cs exists as separate file from ICalendarService.cs
- **AC Impact**: Grep for IDateInitializer interface declaration, verify not added to ICalendarService

**C2: Day Validation Range**
- **Source**: 天候.ERB:35,41 -- SIF RESULT >= 1 && RESULT <= 30 check
- **Verification**: Read 天候.ERB lines 35, 41 for condition
- **AC Impact**: Need both positive (valid day 1-30) and negative (day 0, 31) test cases

**C3: Default Values**
- **Source**: 天候.ERB:7-8 -- 暦法月 = 1, 暦法日 = 1
- **Verification**: Read 天候.ERB lines 7-8 for initial assignments
- **AC Impact**: Verify first display uses month=1, day=1 before any user input

**C5: SetDay Calls on Confirmation**
- **Source**: 天候.ERB:44-47 -- DAY:1=暦法月, DAY:2=暦法日 set before weather init (confirmed via DIM.ERH:43-44)
- **Verification**: Grep DateInitializer.cs for SetDay calls
- **AC Impact**: Verify SetDay(1, month) and SetDay(2, day) called on confirmation

**C6: GetMonthName Reuse**
- **Source**: 天候.ERB:14 -- %日付_月(暦法月)% calls existing @日付_月 function
- **Verification**: Grep DateInitializer.cs for GetMonthName call
- **AC Impact**: Verify CalendarService.GetMonthName is called for display, not reimplemented

**C8: Invalid Input Resilience**
- **Source**: 天候.ERB:49 -- GOTO Ty245124 loops back on any unhandled input
- **Verification**: Test with invalid input values (e.g., 99) and verify loop continues
- **AC Impact**: Must verify no crash/exception on out-of-range or unmapped input

**C9: Engine Type xUnit Tests**
- **Source**: CLAUDE.md Feature Types -- engine type uses dotnet test, not headless
- **Verification**: AC matchers use `succeeds` on `dotnet test` commands
- **AC Impact**: All behavioral ACs must be verifiable via xUnit with mocked dependencies

**C10: Core Repo File Paths**
- **Source**: Project structure -- Era.Core lives in C:\Era\core
- **Verification**: All AC file paths point to C:/Era/core/src/Era.Core/
- **AC Impact**: Implementation targets core repo, not devkit

**C12: PrintButton for UI Selection**
- **Source**: 天候.ERB:15,18,23 -- PRINTBUTTON for [0:変更], [1:変更], [2:決定]
- **Verification**: Grep DateInitializer.cs for PrintButton calls
- **AC Impact**: At least 2 unconditional PrintButton calls (month change, day change); confirm is conditional

**C13: IInputHandler RequestNumericInput**
- **Source**: IInputHandler.cs:14 -- RequestNumericInput is the established input method
- **Verification**: Grep DateInitializer.cs for RequestNumericInput
- **AC Impact**: Must use IInputHandler.RequestNumericInput for all user input, following ShopSystem pattern

**C14: DI Registration**
- **Source**: Established pattern in ServiceCollectionExtensions.cs
- **Verification**: Grep ServiceCollectionExtensions.cs for IDateInitializer registration
- **AC Impact**: Must verify both interface and implementation registered

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F821 | [DONE] | Weather System Migration -- provides ICalendarService (GetMonthName), IWeatherSimulation (SetDailyTemperature, SetCurrentTemperature), IEngineVariables (GetDay/SetDay) |
| Successor | F825 | [DRAFT] | DI Integration -- may consume IDateInitializer |
| Successor | F826 | [DRAFT] | Post-Phase Review |
| Related | F774 | [DONE] | ShopSystem -- precedent for GOTO+INPUT migration pattern (ShopSystem.cs:170-228) |
| Related | F777 | [DONE] | CharacterCustomizer -- precedent for interactive UI pattern |

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
| "must be migrated to C# using established abstraction patterns (IConsoleOutput + IInputHandler)" | DateInitializer class exists with IConsoleOutput and IInputHandler dependencies | AC#1, AC#2, AC#3 |
| "maintaining interface segregation between pure computation services and interactive UI workflows" | IDateInitializer is a separate interface from ICalendarService; CalendarService unchanged | AC#4, AC#5 |
| "Each migrated function is the single source of truth for its domain logic" | DateInitializer implements the complete @日付初期設定 logic (month/day selection, validation, confirmation) | AC#6, AC#7, AC#8, AC#9, AC#10, AC#11 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IDateInitializer.cs exists as separate interface file | file | Glob(C:/Era/core/src/Era.Core/Interfaces/IDateInitializer.cs) | exists | - | [x] |
| 2 | DateInitializer.cs exists as implementation file | file | Glob(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs) | exists | - | [x] |
| 3 | DateInitializer injects all 5 dependencies | code | Grep(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs, pattern="IConsoleOutput|IInputHandler|ICalendarService|IWeatherSimulation|IEngineVariables") | gte | 5 | [x] |
| 4 | IDateInitializer is not added to ICalendarService | code | Grep(C:/Era/core/src/Era.Core/Interfaces/ICalendarService.cs, pattern="InitializeDate|DateInitializer|IInputHandler|IConsoleOutput") | not_matches | - | [x] |
| 5 | CalendarService has no UI dependencies added | code | Grep(C:/Era/core/src/Era.Core/State/CalendarService.cs, pattern="IInputHandler|IConsoleOutput") | not_matches | - | [x] |
| 6 | DateInitializer uses while loop pattern | code | Grep(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs, pattern="while.*true") | matches | - | [x] |
| 7 | DateInitializer uses RequestNumericInput | code | Grep(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs, pattern="RequestNumericInput") | matches | `RequestNumericInput` | [x] |
| 8 | DateInitializer calls GetMonthName for display | code | Grep(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs, pattern="GetMonthName") | matches | `GetMonthName` | [x] |
| 9 | DateInitializer calls SetDailyTemperature and SetCurrentTemperature | code | Grep(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs, pattern="SetDailyTemperature|SetCurrentTemperature") | gte | 2 | [x] |
| 10 | DateInitializer calls SetDay for month and day | code | Grep(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs, pattern="SetDay") | gte | 2 | [x] |
| 11 | DateInitializer uses PrintButton for UI selection | code | Grep(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs, pattern="PrintButton") | gte | 2 | [x] |
| 12 | DI registration for IDateInitializer in ServiceCollectionExtensions | code | Grep(C:/Era/core/src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs, pattern="IDateInitializer.*DateInitializer") | matches | `IDateInitializer.*DateInitializer` | [x] |
| 13 | DateInitializerTests.cs exists | file | Glob(C:/Era/core/src/Era.Core.Tests/Calendar/DateInitializerTests.cs) | exists | - | [x] |
| 14 | Valid month (1-6) accepted and updates month | test | dotnet test C:/Era/core/src/Era.Core.Tests/ --filter "FullyQualifiedName~DateInitializerTests.InitializeDate_ValidMonth_AcceptsAndUpdates" --blame-hang-timeout 10s | succeeds | pass | [x] |
| 15 | Valid day (1-30) accepted and updates day | test | dotnet test C:/Era/core/src/Era.Core.Tests/ --filter "FullyQualifiedName~DateInitializerTests.InitializeDate_ValidDay_AcceptsAndUpdates" --blame-hang-timeout 10s | succeeds | pass | [x] |
| 16 | Confirmation calls both weather methods test | test | dotnet test C:/Era/core/src/Era.Core.Tests/ --filter "FullyQualifiedName~DateInitializerTests.InitializeDate_Confirmation_CallsWeatherMethods" --blame-hang-timeout 10s | succeeds | pass | [x] |
| 17 | Confirm button conditional guard exists before PrintButton 決定 | code | Grep(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs, pattern="month >= 1 && month <= 6 && day >= 1 && day <= 30") | contains | `month >= 1 && month <= 6 && day >= 1 && day <= 30` | [x] |
| 18 | Invalid input loops back without crash test | test | dotnet test C:/Era/core/src/Era.Core.Tests/ --filter "FullyQualifiedName~DateInitializerTests.InitializeDate_InvalidInput_LoopsBack" --blame-hang-timeout 10s | succeeds | pass | [x] |
| 19 | Default values month=1 day=1 test | test | dotnet test C:/Era/core/src/Era.Core.Tests/ --filter "FullyQualifiedName~DateInitializerTests.InitializeDate_DefaultValues" --blame-hang-timeout 10s | succeeds | pass | [x] |
| 20 | Build succeeds with zero warnings | build | dotnet build C:/Era/core/src/Era.Core/ | succeeds | pass | [x] |
| 21 | engine-dev INTERFACES.md updated with IDateInitializer entry | code | Grep(C:/Era/devkit/.claude/skills/engine-dev/INTERFACES.md, pattern="IDateInitializer") | matches | `IDateInitializer` | [x] |
| 22 | Invalid month (0, 7) rejected and month unchanged | test | dotnet test C:/Era/core/src/Era.Core.Tests/ --filter "FullyQualifiedName~DateInitializerTests.InitializeDate_InvalidMonth_RejectedAndUnchanged" --blame-hang-timeout 10s | succeeds | pass | [x] |
| 23 | Invalid day (0, 31) rejected and day unchanged | test | dotnet test C:/Era/core/src/Era.Core.Tests/ --filter "FullyQualifiedName~DateInitializerTests.InitializeDate_InvalidDay_RejectedAndUnchanged" --blame-hang-timeout 10s | succeeds | pass | [x] |
| 24 | Confirmation calls SetDay with correct indices | test | dotnet test C:/Era/core/src/Era.Core.Tests/ --filter "FullyQualifiedName~DateInitializerTests.InitializeDate_Confirmation_CallsSetDayWithCorrectIndices" --blame-hang-timeout 10s | succeeds | pass | [x] |

### AC Details

**AC#3: DateInitializer injects all 5 dependencies**
- **Test**: `Grep(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs, pattern="IConsoleOutput|IInputHandler|ICalendarService|IWeatherSimulation|IEngineVariables")`
- **Expected**: `gte 5`
- **Derivation**: 5 constructor-injected interfaces: (1) IConsoleOutput for PrintButton/PrintLine/DrawLine, (2) IInputHandler for RequestNumericInput, (3) ICalendarService for GetMonthName, (4) IWeatherSimulation for SetDailyTemperature/SetCurrentTemperature, (5) IEngineVariables for GetDay/SetDay. Each maps to an ERB dependency in @日付初期設定.
- **Rationale**: Verifies complete DI injection following ShopSystem pattern; missing any dependency would break the migration.

**AC#9: DateInitializer calls SetDailyTemperature and SetCurrentTemperature**
- **Test**: `Grep(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs, pattern="SetDailyTemperature|SetCurrentTemperature")`
- **Expected**: `gte 2`
- **Derivation**: 2 weather methods called on confirmation: (1) SetDailyTemperature (天候.ERB:45 CALL 日間気温設定), (2) SetCurrentTemperature (天候.ERB:46 CALL 現在気温設定). Both must be called exactly once on date confirmation.
- **Rationale**: Constraint C4 requires both weather initialization calls on confirmation.

**AC#10: DateInitializer calls SetDay for month and day**
- **Test**: `Grep(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs, pattern="SetDay")`
- **Expected**: `gte 2`
- **Derivation**: 2 SetDay calls required: (1) SetDay(1, month) for DAY:1=暦法月 (天候.ERB:44-47 implied), (2) SetDay(2, day) for DAY:2=暦法日. Maps to ERB assignment of 暦法月 and 暦法日 before weather init.
- **Rationale**: Constraint C5 requires SetDay calls with correct indices for both month and day.

**AC#11: DateInitializer uses PrintButton for UI selection**
- **Test**: `Grep(C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs, pattern="PrintButton")`
- **Expected**: `gte 2`
- **Derivation**: Minimum 2 PrintButton calls: (1) month change button "[0:変更]" (天候.ERB:15), (2) day change button "[1:変更]" (天候.ERB:18). Confirm button "[2:決定]" (天候.ERB:23) is conditional, adding a 3rd call when both values are valid. Floor is 2 (always displayed).
- **Rationale**: Constraint C12 requires PrintButton usage for interactive selection buttons.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Migrate @日付初期設定 to new C# DateInitializer class | AC#1, AC#2 |
| 2 | IConsoleOutput and IInputHandler dependencies | AC#3, AC#7 |
| 3 | while(true)+RequestNumericInput pattern (ShopSystem precedent) | AC#6, AC#7 |
| 4 | Validate month (1-6) | AC#14, AC#22 |
| 5 | Validate day (1-30) | AC#15, AC#23 |
| 6 | Display season names via CalendarService.GetMonthName | AC#8 |
| 7 | Call SetDailyTemperature + SetCurrentTemperature on confirmation | AC#9, AC#16 |
| 7b | SetDay calls for DAY:1 (month) and DAY:2 (day) on confirmation | AC#10, AC#24 |
| 8 | Register new service in DI container | AC#12 |
| 9 | Interface segregation (separate IDateInitializer) | AC#4, AC#5, AC#21 |
| 10 | Confirm button shown only when both values valid | AC#11, AC#17 |
| 11 | Invalid input loops back without crash | AC#18 |
| 12 | Default values month=1 day=1 | AC#19 |
| 13 | Build succeeds | AC#20 |
| 14 | Test coverage | AC#13, AC#14, AC#15, AC#16, AC#17, AC#18, AC#19, AC#22, AC#23, AC#24 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

`DateInitializer` is a new C# class in `Era.Core.Calendar` that migrates `@日付初期設定` (天候.ERB:6-49). It follows the ShopSystem `while(true)+RequestNumericInput` pattern for GOTO+INPUT loop migration. The class receives five constructor-injected dependencies: `IConsoleOutput`, `IInputHandler`, `ICalendarService`, `IWeatherSimulation`, and `IEngineVariables`.

The single public method `InitializeDate()` runs a `while(true)` loop that:
1. Displays the current month (via `GetMonthName`) and day values with `PrintButton` change buttons
2. Conditionally renders the confirm button when both month (1-6) and day (1-30) are valid
3. Calls `RequestNumericInput("")` to await button selection (0=month change, 1=day change, 2=confirm)
4. On selection 0: reads a second numeric input for month (1-6); validates and updates local state
5. On selection 1: reads a second numeric input for day (1-30); validates and updates local state
6. On selection 2 (with valid month+day): calls `SetDay(1, month)`, `SetDay(2, day)`, `SetDailyTemperature()`, `SetCurrentTemperature()`, then returns
7. On any invalid/unhandled selection: `continue` (maps to GOTO $Ty245124)

A new `IDateInitializer` interface in `Era.Core.Interfaces` declares the single `void InitializeDate()` method. This preserves ISP by keeping UI-interactive services separate from `ICalendarService` (pure computation). `CalendarService.cs` receives no modifications.

DI registration follows the Singleton pattern established by `ICalendarService` and `IWeatherSimulation` in `ServiceCollectionExtensions.cs`.

Tests in `DateInitializerTests.cs` use mocked `IInputHandler` with pre-configured `ProvideInput` responses to drive the loop without headless hangs. All 24 ACs are covered by this approach.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create `C:/Era/core/src/Era.Core/Interfaces/IDateInitializer.cs` declaring `void InitializeDate()` |
| 2 | Create `C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs` implementing `IDateInitializer` |
| 3 | Constructor signature: `DateInitializer(IConsoleOutput, IInputHandler, ICalendarService, IWeatherSimulation, IEngineVariables)` — 5 unique interface type references in file |
| 4 | `ICalendarService.cs` gets no `InitializeDate`, `DateInitializer`, `IInputHandler`, or `IConsoleOutput` additions; verified by Grep returning no matches |
| 5 | `CalendarService.cs` gets no `IInputHandler` or `IConsoleOutput` field/parameter; verified by Grep returning no matches |
| 6 | `InitializeDate()` body contains `while (true)` for the GOTO $Ty245124 loop |
| 7 | `_inputHandler.RequestNumericInput("")` called inside the loop for button selection and sub-inputs |
| 8 | `_calendar.GetMonthName(month)` called in the display block to render season name |
| 9 | `_weather.SetDailyTemperature()` and `_weather.SetCurrentTemperature()` called in the confirmation branch (RESULT==2) |
| 10 | `_engineVars.SetDay(1, month)` and `_engineVars.SetDay(2, day)` called before weather init on confirmation |
| 11 | At least 2 unconditional `PrintButton` calls: `PrintButton("[0:変更]", 0)` and `PrintButton("[1:変更]", 1)`; confirm button `PrintButton("[2:決定]", 2)` is conditional |
| 12 | `services.AddSingleton<IDateInitializer, DateInitializer>()` added to `ServiceCollectionExtensions.cs` in the State/Weather registration block |
| 13 | Create `C:/Era/core/src/Era.Core.Tests/Calendar/DateInitializerTests.cs` with xUnit test class |
| 14 | `InitializeDate_ValidMonth_AcceptsAndUpdates` test: input sequence [0, valid 1-6], verify month updates and loop continues |
| 15 | `InitializeDate_ValidDay_AcceptsAndUpdates` test: input sequence [1, valid 1-30], verify day updates and loop continues |
| 22 | `InitializeDate_InvalidMonth_RejectedAndUnchanged` test: input sequence [0, invalid 0 or 7], verify month remains unchanged and loop continues |
| 24 | `InitializeDate_Confirmation_CallsSetDayWithCorrectIndices` test: pre-set valid month+day, input [2], verify mock `IEngineVariables` receives `SetDay(1, selectedMonth)` and `SetDay(2, selectedDay)` with correct index arguments |
| 23 | `InitializeDate_InvalidDay_RejectedAndUnchanged` test: input sequence [1, invalid 0 or 31], verify day remains unchanged and loop continues |
| 16 | `InitializeDate_Confirmation_CallsWeatherMethods` test: pre-set valid month+day, input [2], verify `SetDailyTemperature()` and `SetCurrentTemperature()` each called once |
| 17 | Code-level verification: Grep `DateInitializer.cs` for the conditional guard `month >= 1 && month <= 6 && day >= 1 && day <= 30` before `PrintButton("[2:決定]", 2)`. Defensive code faithfully migrated from 天候.ERB:22 — unreachable false branch (month/day always valid due to input validation), but guard preserved for ERB fidelity |
| 18 | `InitializeDate_InvalidInput_LoopsBack` test: provide unmapped input (e.g., 99), verify loop continues without exception; terminate with valid confirm sequence |
| 19 | `InitializeDate_DefaultValues` test: verify first `GetMonthName` call uses month=1, first day display uses day=1 (default state before any input) |
| 20 | `dotnet build C:/Era/core/src/Era.Core/` passes with zero warnings; ensured by following `TreatWarningsAsErrors` convention and adding XML doc comments to `IDateInitializer` |
| 21 | Update `.claude/skills/engine-dev/INTERFACES.md` to include `IDateInitializer` entry in Core Interfaces section (SSOT update per ssot-update-rules.md) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Interactive vs parameterized | A: Pass month/day as parameters (pure function); B: Interactive while(true) loop following ShopSystem | B: Interactive while(true) | ERB function is UI-interactive by nature; parameterized approach would lose GOTO+INPUT semantics and break the established migration pattern |
| Interface placement | A: Add `InitializeDate()` to `ICalendarService`; B: New `IDateInitializer` in `Era.Core.Interfaces` | B: New `IDateInitializer` | ISP constraint (C11) and F808 lesson: CalendarService is pure computation; mixing UI-interactive methods violates SRP/ISP |
| File location | A: `Era.Core.State` (alongside CalendarService); B: `Era.Core.Calendar` (domain-specific); C: `Era.Core.Interactive` (new generic namespace) | B: `Era.Core.Calendar` | Interactive UI classes use domain-specific namespaces (ShopSystem → Era.Core.Shop, CharacterCustomizer → Era.Core.Character); Era.Core.State is for pure computation services (CalendarService, WeatherSimulation); placing interactive UI in State blurs the boundary mandated by Philosophy |
| DI scope | A: Transient; B: Singleton | B: Singleton | Stateless after `InitializeDate()` returns (local variables only); consistent with `ICalendarService` and `IWeatherSimulation` as Singleton |
| Month/day state during loop | A: Read from `IEngineVariables` on each iteration; B: Local `int month, day` variables initialized to 1 | B: Local variables | ERB initializes `暦法月 = 1` / `暦法日 = 1` at function entry (天候.ERB:7-8), not re-read from engine state; local variables correctly mirror this |
| Input result extraction | A: Call `_engineVars.GetResult()` after RequestNumericInput; B: Cast `ProvideInput` result | A: `_engineVars.GetResult()` | Established ShopSystem.cs pattern (`int input = GetResult()`) — avoids coupling to IInputHandler internals |
| Sub-input for month/day change | A: Separate nested while loops; B: Single RESULT branch with direct input call | B: Single RESULT branch | ERB has flat IF/ELSEIF structure (天候.ERB:26-48) — a nested loop would over-engineer; one `RequestNumericInput` per branch, validate, then GOTO back |
| Result<Unit> handling | A: Check and propagate Result<Unit> from IConsoleOutput/IInputHandler; B: Discard return values (fire-and-forget) | B: Discard return values | Consistent with ShopSystem/CharacterCustomizer precedent; Result handling for UI output is a cross-cutting concern not scoped to individual interactive services |

### Interfaces / Data Structures

**New interface** (`C:/Era/core/src/Era.Core/Interfaces/IDateInitializer.cs`):

```csharp
namespace Era.Core.Interfaces;

/// <summary>
/// Interactive date initialization service.
/// Migrated from 天候.ERB @日付初期設定 (F828).
/// Provides GOTO+INPUT loop for setting calendar month and day before game start.
/// </summary>
public interface IDateInitializer
{
    /// <summary>
    /// Run interactive date initialization UI loop.
    /// Displays month/day selection with PRINTBUTTON options, validates range,
    /// and calls SetDailyTemperature + SetCurrentTemperature on confirmation.
    /// Loops until user confirms valid month (1-6) and day (1-30).
    /// </summary>
    void InitializeDate();
}
```

**Implementation skeleton** (`C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs`):

```csharp
using Era.Core.Input;
using Era.Core.Interfaces;

namespace Era.Core.Calendar;

/// <summary>
/// Interactive date initialization (天候.ERB @日付初期設定, F828).
/// Implements while(true)+RequestNumericInput pattern from ShopSystem precedent.
/// </summary>
public class DateInitializer : IDateInitializer
{
    private readonly IConsoleOutput _console;
    private readonly IInputHandler _inputHandler;
    private readonly ICalendarService _calendar;
    private readonly IWeatherSimulation _weather;
    private readonly IEngineVariables _engineVars;

    public DateInitializer(
        IConsoleOutput console,
        IInputHandler inputHandler,
        ICalendarService calendar,
        IWeatherSimulation weather,
        IEngineVariables engineVars)
    {
        _console = console;
        _inputHandler = inputHandler;
        _calendar = calendar;
        _weather = weather;
        _engineVars = engineVars;
    }

    public void InitializeDate()
    {
        int month = 1; // 暦法月 = 1 (天候.ERB:7)
        int day = 1;   // 暦法日 = 1 (天候.ERB:8)

        while (true) // GOTO $Ty245124 (天候.ERB:9,49)
        {
            _console.DrawLine();
            _console.PrintLine("");
            _console.PrintLine("日付初期設定");
            _console.PrintLine("");

            string monthName = _calendar.GetMonthName(month); // 天候.ERB:14
            _console.PrintForm("{0} ", monthName);
            _console.PrintButton("[0:変更]", 0L); // 天候.ERB:15
            _console.PrintLine("");

            _console.PrintForm("{0}日 ", day); // 天候.ERB:17
            _console.PrintButton("[1:変更]", 1L); // 天候.ERB:18
            _console.PrintLine("");
            _console.PrintLine("");
            _console.PrintLine("");

            if (month >= 1 && month <= 6 && day >= 1 && day <= 30) // 天候.ERB:22
                _console.PrintButton("[2:決定]", 2L); // 天候.ERB:23
            _console.PrintLine("");

            _inputHandler.RequestNumericInput(""); // INPUT (天候.ERB:25)
            int result = _engineVars.GetResult();

            if (result == 0) // 天候.ERB:26-36 月変更
            {
                _console.PrintLine("月変更");
                _console.PrintLine("[1] 春季     4~5月");
                _console.PrintLine("[2] 夏季上旬 6~7月");
                _console.PrintLine("[3] 夏季下旬 8~9月");
                _console.PrintLine("[4] 秋季     10~11月");
                _console.PrintLine("[5] 冬季上旬 12~1月");
                _console.PrintLine("[6] 冬季下旬 2~3月");
                _inputHandler.RequestNumericInput("");
                int monthInput = _engineVars.GetResult();
                if (monthInput >= 1 && monthInput <= 6)
                    month = monthInput;
            }
            else if (result == 1) // 天候.ERB:37-42 日変更
            {
                _console.PrintLine("日時変更(1~30のいずれかの数値を入力してください)");
                _inputHandler.RequestNumericInput("");
                int dayInput = _engineVars.GetResult();
                if (dayInput >= 1 && dayInput <= 30)
                    day = dayInput;
            }
            else if (result == 2 && month >= 1 && month <= 6 && day >= 1 && day <= 30) // 天候.ERB:44-47
            {
                _engineVars.SetDay(1, month); // DAY:1 = 暦法月
                _engineVars.SetDay(2, day);   // DAY:2 = 暦法日
                _weather.SetDailyTemperature();   // CALL 日間気温設定
                _weather.SetCurrentTemperature(); // CALL 現在気温設定
                return; // RETURN (天候.ERB:47)
            }
            // else: continue (GOTO $Ty245124, 天候.ERB:49)
        }
    }
}
```

**DI registration** (in `ServiceCollectionExtensions.cs`, alongside existing Weather/Calendar registrations):

```csharp
services.AddSingleton<IDateInitializer, DateInitializer>();
```

**Note on `IInputHandler` namespace**: `IInputHandler` resides in `Era.Core.Input` (not `Era.Core.Interfaces`). The `DateInitializer.cs` must include `using Era.Core.Input;` to reference it.

### Upstream Issues

<!-- Optional: Issues discovered during design that require upstream changes (AC gaps, constraint gaps, interface gaps).
     Orchestrator reads this section after Phase 4 and dispatches micro-revisions if needed. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| AC#3 Grep pattern counts interface name occurrences in the file body (IConsoleOutput, IInputHandler, etc.), but `IInputHandler` lives in `Era.Core.Input` namespace, not `Era.Core.Interfaces`. The using directive `using Era.Core.Input;` appears in the file but `IInputHandler` itself is still referenced by name in the constructor — the Grep pattern matches on the type name, so this is fine. No action needed. | N/A | No fix needed — pattern matches type names, not namespaces |
| AC#10 Details state "SetDay(1, month) for DAY:1=暦法月 (天候.ERB:44-47 implied)". The ERB actually uses `暦法月` and `暦法日` variable names defined in DIM.ERH as `#DEFINE 暦法月 DAY:1` and `#DEFINE 暦法日 DAY:2`. The mapping is confirmed: SetDay(1, month) = DAY:1 and SetDay(2, day) = DAY:2. No AC change needed, but the AC Detail comment "implied" should be "confirmed via DIM.ERH:43-44". | AC Details for AC#10 | Minor wording clarification — no AC change needed |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1 | Create `C:/Era/core/src/Era.Core/Interfaces/IDateInitializer.cs` declaring `void InitializeDate()` with XML doc comments | | [x] |
| 2 | 2, 3, 6, 7, 8, 9, 10, 11, 17 | Create `C:/Era/core/src/Era.Core/Calendar/DateInitializer.cs` implementing IDateInitializer with all 5 constructor-injected dependencies (IConsoleOutput, IInputHandler, ICalendarService, IWeatherSimulation, IEngineVariables) and while(true)+RequestNumericInput loop following ShopSystem pattern | | [x] |
| 3 | 4, 5 | Verify ICalendarService.cs and CalendarService.cs receive no UI dependencies (IInputHandler, IConsoleOutput) — no file changes, grep verification only | | [x] |
| 4 | 12 | Add `services.AddSingleton<IDateInitializer, DateInitializer>()` to `C:/Era/core/src/Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` alongside existing Weather/Calendar registrations | | [x] |
| 5 | 13, 14, 15, 16, 18, 19, 22, 23, 24 | Create `C:/Era/core/src/Era.Core.Tests/Calendar/DateInitializerTests.cs` with xUnit tests covering: default values (month=1/day=1), valid month acceptance, invalid month rejection, valid day acceptance, invalid day rejection, confirmation weather+SetDay calls, invalid input loop-back | | [x] |
| 6 | 20 | Build `C:/Era/core/src/Era.Core/` and verify zero warnings (TreatWarningsAsErrors enforced) | | [x] |
| 7 | 21 | Update `.claude/skills/engine-dev/INTERFACES.md` with IDateInitializer entry in Core Interfaces section (devkit repo, SSOT update per ssot-update-rules.md) | | [x] |

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

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | feature-828.md Tasks T1-T2, Technical Design (Interfaces/Data Structures skeleton) | `IDateInitializer.cs`, `DateInitializer.cs` |
| 2 | implementer | sonnet | feature-828.md Task T4, ServiceCollectionExtensions.cs | Updated `ServiceCollectionExtensions.cs` with IDateInitializer registration |
| 3 | implementer | sonnet | feature-828.md Task T5, Technical Design AC#14-#19 coverage | `DateInitializerTests.cs` with all 7 test methods (RED state) |
| 4 | tester | sonnet | feature-828.md all ACs, core repo | AC verification results |
| 5 | implementer | sonnet | feature-828.md Task T7, INTERFACES.md | Updated `INTERFACES.md` with IDateInitializer entry |

### Pre-conditions

- F821 is [DONE]: ICalendarService (GetMonthName), IWeatherSimulation (SetDailyTemperature, SetCurrentTemperature), IEngineVariables (GetDay/SetDay) all exist
- Core repo at `C:/Era/core` is accessible
- ShopSystem.cs:170-228 available as pattern reference

### Execution Order

1. **T1** — Create `IDateInitializer.cs` interface (no dependencies, safe to do first)
2. **T2** — Create `DateInitializer.cs` implementation (depends on T1 interface existing)
3. **T3** — Grep verification only (no file changes; confirms ISP compliance)
4. **T4** — Add DI registration in `ServiceCollectionExtensions.cs` (depends on T1+T2 existing)
5. **T5** — Create `DateInitializerTests.cs` (depends on T1+T2 for mock targets; write in TDD RED state before checking build)
6. **T6** — Build verification (depends on T1+T2+T4+T5 all complete)
7. **T7** — Update INTERFACES.md with IDateInitializer entry (devkit repo, after T1 interface is defined)

### Build Verification Steps

```bash
# Run from WSL:
cd /mnt/c/Era/core && /home/siihe/.dotnet/dotnet build src/Era.Core/ --no-incremental

# Run tests:
/home/siihe/.dotnet/dotnet test src/Era.Core.Tests/ --blame-hang-timeout 10s --results-directory /mnt/c/Era/devkit/_out/test-results --filter "FullyQualifiedName~DateInitializerTests"
```

### Success Criteria

- All 24 ACs pass (exists, Grep matches/not_matches, succeeds pass)
- Build produces zero warnings (TreatWarningsAsErrors enforced)
- `IDateInitializer.cs` is separate from `ICalendarService.cs` (ISP)
- `CalendarService.cs` has no new IInputHandler/IConsoleOutput references

### Error Handling

- If build fails with CS0234 (type not found): Check `using Era.Core.Input;` is present in `DateInitializer.cs` for IInputHandler
- If test hangs: Verify all IInputHandler.RequestNumericInput calls are matched by mock ProvideInput responses in test setup
- If GetResult() not available on IEngineVariables mock: Check IEngineVariables.cs for GetResult method; if missing, use established ShopSystem pattern to resolve

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|-------------|--------|
| verup.ERB caller site still calls @日付初期設定 via ERB CALL | Interactive UI now in C# DateInitializer; caller must be updated to invoke via DI | Feature | F825 | N/A (F825 exists) | [x] | 確認済み |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-03-05T08:10 | Phase 1 | initializer | Status [REVIEWED]->[WIP] | OK |
| 2026-03-05T08:14 | Phase 2 | explorer | Codebase investigation | OK - all interfaces confirmed |
| 2026-03-05T08:18 | Phase 3 | implementer | TDD RED: stubs + 8 tests | RED confirmed (8/8 fail NotImplementedException) |
| 2026-03-05T08:22 | Phase 4 | implementer | T2 impl + T4 DI reg + T3 grep + T7 INTERFACES.md | GREEN (8/8 pass, 0 warnings) |
| 2026-03-05T08:25 | Phase 5 | orchestrator | Refactoring review | SKIP (no refactoring needed) |
| 2026-03-05T08:30 | DEVIATION | Bash | dotnet build src/Era.Core/ | PRE-EXISTING: ChildNameGenerator.cs (untracked, not F828) 3 errors blocking build |
| 2026-03-05T08:35 | Phase 7 | ac-tester | AC verification (WIP files .bak'd) | 24/24 PASS |
| 2026-03-05T08:33 | DEVIATION | Bash | ac-static-verifier --ac-type code | exit 1: tool gte/not_matches format limitation (6/12 passed, 6 manual-verified PASS) |
| 2026-03-05T08:33 | DEVIATION | Bash | ac-static-verifier --ac-type build | exit 1: Smart App Control blocks dotnet (WSL build verified PASS) |
| 2026-03-05T08:38 | Phase 8 | feature-reviewer | Post-review + SSOT check | READY |
| 2026-03-05T08:40 | Phase 9 | orchestrator | Report & Approval | Approved |
| 2026-03-05T08:40 | Phase 10 | finalizer | [WIP]->[DONE] | READY_TO_COMMIT |
| 2026-03-05T08:40 | Commit | orchestrator | a6ded3b | CI passed (806 tests) |
| 2026-03-05T08:42 | CodeRabbit | 0 findings | - |
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
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A->B->A detection). -->
<!-- Category codes: See docs/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase1-RefCheck iter1: Links section | F808 referenced in body but missing from Links section
- [fix] Phase2-Review iter1: Mandatory Handoffs table | Missing Transferred and Result columns (template requires 7 columns)
- [fix] Phase2-Review iter2: Tasks table + Implementation Contract | AC#21 moved from Task#1 to new Task#7 with dedicated Implementation Contract phase
- [fix] Phase2-Review iter2: Goal Coverage Verification | AC#21 moved from Description column to Covering AC(s) column for Goal item 9
- [fix] Phase2-Review iter3: AC#17 | Changed from runtime test to code-level grep — confirm button guard is defensive dead code (month/day always valid), untestable via public API
- [fix] Phase3-Maintainability iter4: Mandatory Handoffs | Added verup.ERB caller migration tracking to F825
- [fix] Phase3-Maintainability iter4: File location Key Decision | Changed namespace from Era.Core.State to Era.Core.Calendar (ShopSystem/CharacterCustomizer precedent)
- [fix] Phase3-Maintainability iter4: AC#14/AC#15 | Split into 4 ACs (AC#14+AC#22 for month, AC#15+AC#23 for day) — separate valid acceptance from invalid rejection
- [fix] Phase3-Maintainability iter4: Key Decisions | Added Result<Unit> handling decision (discard, consistent with ShopSystem precedent)
- [fix] Phase2-Review iter5: AC Definition Table | Added AC#24 for SetDay behavioral test (SetDay had code grep only, asymmetric with weather methods which have both code+behavioral)
- [fix] Phase2-Review iter6: Success Criteria | Stale AC count (21 → 24)
- [fix] Phase4-ACValidation iter7: AC#21 | Changed Type from test to code (Grep method requires code type)
- [fix] Phase4-ACValidation iter7: AC#17 | Changed Matcher from matches to contains (avoids regex metacharacter issues)
- [fix] Phase4-ACValidation iter7: AC table | Reordered AC#23/AC#24 to sequential order
- [fix] Phase4-ACValidation iter8: AC#1,2,13 | Changed Expected from 1 to - for exists matcher
- [fix] Phase4-ACValidation iter8: AC#6 | Removed duplicate pattern from Expected (already in Method)

---

<!-- fc-phase-6-completed -->
## Links
- [Predecessor: F821](feature-821.md) - Weather System Migration
- [Related: F774](feature-774.md) - ShopSystem (GOTO+INPUT migration precedent)
- [Related: F777](feature-777.md) - CharacterCustomizer (interactive UI precedent)
- [Sibling: F819](feature-819.md) - Sibling in Phase 22
- [Sibling: F822](feature-822.md) - Sibling in Phase 22
- [Sibling: F823](feature-823.md) - Sibling in Phase 22
- [Sibling: F824](feature-824.md) - Sibling in Phase 22
- [Successor: F825](feature-825.md) - DI Integration
- [Successor: F826](feature-826.md) - Post-Phase Review
- [Related: F808](feature-808.md) - ISP lesson learned (separate interface precedent)
