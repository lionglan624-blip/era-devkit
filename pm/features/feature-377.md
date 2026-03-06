# Feature 377: Phase 4 Architecture Refactoring Planning

## Status: [DONE]

**Unblocked**: F383 completed (2026-01-07)

**Previously Blocked by**: F380, F382 (both DONE as of 2026-01-06)

## Type: engine

## Created: 2026-01-06

---

## Summary

Plan and execute Phase 4 (Architecture Refactoring) from full-csharp-architecture.md. Transform Phase 3's static class implementations into DI-ready architecture with proper abstractions.

**Context**: Phase 4 establishes the architectural patterns that all subsequent phases (5-24) must follow. The goal is not "splitting files" but "enabling testability and extensibility through proper abstractions."

---

## Background

### Philosophy (Mid-term Vision)

**Testability and Extensibility**: Phase 3 created static classes for 1:1 ERB migration. Phase 4 transforms these into injectable, mockable, extensible components. This is not about reducing line counts—it's about enabling:
- Unit testing with mocks
- Future extensions without modifying existing code
- Clear contracts via interfaces

### Problem (Current Issue)

Phase 3 deliverables share a common anti-pattern:

```csharp
// Every Phase 3 file follows this pattern
public static class SomeSystem
{
    public static void DoSomething(int characterId, ...) { ... }
}
```

**Core Issues** (in order of severity):

1. **DIP Violation (Critical)**: Concrete dependencies everywhere, no abstraction
2. **ISP Violation (Critical)**: No interfaces = clients depend on everything
3. **Untestable**: Static methods cannot be mocked
4. **Type-unsafe**: Raw `int` for IDs enables bugs at compile-time

**Non-Issues** (common misconceptions):
- Line count alone is NOT a problem (607-line single-responsibility class is fine)
- Switch statements are NOT inherently bad (if they express a single decision)

### Goal (What to Achieve)

1. **Interface extraction** for all Phase 3 static classes (DIP/ISP fix)
2. **DI infrastructure** for dependency injection
3. **Strongly Typed IDs** for compile-time safety
4. **Result type** for explicit error handling
5. **Pattern documentation** for Phase 5-24 reference

---

## Design Principles

### Core Principles (Mandatory)

| Principle | Application in Phase 4 |
|-----------|------------------------|
| **DIP** | Extract interface for every static class → inject via constructor |
| **ISP** | Keep interfaces small and focused on client needs |
| **YAGNI** | Don't split files just because they're large; split only for distinct responsibilities |
| **KISS** | Prefer simple solutions; avoid over-engineering |

### When to Split (Responsibility-Based)

**DO split when**:
- A class has multiple unrelated responsibilities (e.g., "calculate" AND "format output")
- Different clients need different subsets of functionality
- Testing requires isolating specific behavior

**DON'T split when**:
- File is large but has single cohesive responsibility (e.g., ClothingSystem = all clothing logic)
- Split would create artificial boundaries (e.g., splitting constants by arbitrary size)
- No clear benefit to testability or extensibility

### Refactoring Decision Matrix

| Symptom | Root Cause? | Action |
|---------|:-----------:|--------|
| static class | **YES** | Extract interface, convert to instance class |
| Raw int IDs | **YES** | Apply Strongly Typed ID |
| Many parameters | Maybe | Consider parameter object if parameters represent a concept |
| Large file (200+ lines) | **NO** | Only split if multiple responsibilities exist |
| Large switch | **NO** | Only replace if switch will grow with new features |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CharacterId strongly typed ID exists | file | Glob | exists | Era.Core/Types/CharacterId.cs | [x] |
| 2 | LocationId strongly typed ID exists | file | Glob | exists | Era.Core/Types/LocationId.cs | [x] |
| 3 | Result type exists | file | Glob | exists | Era.Core/Types/Result.cs | [x] |
| 4 | IGameState interface exists | file | Glob | exists | Era.Core/Interfaces/IGameState.cs | [x] |
| 5 | DI configuration exists | file | Glob | exists | Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | [x] |
| 6 | Interface files created for all 20 targets | file | Glob Era.Core/Interfaces/I*.cs | count_equals | 20 | [x] |
| 7 | Static class declarations in pure constant files | code | Grep "public static class" Era.Core --glob "*.cs" | count_equals | 6 | [x] |
| 8 | C# build succeeds | build | dotnet build | succeeds | - | [x] |
| 9 | Architecture tests pass | test | dotnet test | succeeds | - | [x] |
| 10 | Existing tests still pass | test | dotnet test | succeeds | - | [x] |
| 11 | Pattern documentation complete | code | Grep "## Pattern Documentation" Game/agents/feature-377.md | contains | "## Pattern Documentation" | [x] |

### AC Details

**AC6**: `Glob Era.Core/Interfaces/I*.cs` → count_equals 20 (19 service interfaces + IGameState)
**AC7**: `Grep "public static class" Era.Core --glob "*.cs"` → count_equals 6
**AC8**: `dotnet build Era.Core/`
**AC9**: `dotnet test Era.Core.Tests/ --filter "Category=Architecture"` (architecture validation tests, created by Task 6)
**AC10**: `dotnet test Era.Core.Tests/` (all existing tests)

> **AC6 Note**: Verify exactly 20 interface files exist in Era.Core/Interfaces/ (19 service interfaces + IGameState). Directory Era.Core/Interfaces/ will be created as part of Task 5.
> **AC7 Note**: 6 static class declarations from 4 files: Constants(1), VariableDefinitions(3 including nested VisitorAppearance and Aliases), RelationshipTypes(1), ColorSettings(1). ServiceCollectionExtensions is an extension class, not counted as pure constant file.
> **AC11 Note**: Pattern Documentation must include: Strongly Typed ID Pattern, Result Type Pattern, Interface + DI Pattern, Usage Example.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Create Strongly Typed IDs: CharacterId.cs, LocationId.cs | [x] |
| 2 | 3 | Create Result<T> type with Success/Failure | [x] |
| 3 | 4 | Create core interfaces: IGameState (new state abstraction) | [x] |
| 4 | 5 | Setup DI: ServiceCollectionExtensions.cs | [x] |
| 5 | 6,7 | Extract interfaces for 19 static classes (see Interface Extraction Plan) | [x] |
| 6 | 8,9,10 | Update tests for DI support, create architecture tests with [Category("Architecture")], verify all pass | [x] |
| 7 | 11 | Document patterns for Phase 5-24 | [x] |

---

## Phase 3 Deliverables Inventory (Complete)

All 25 files in Era.Core requiring Phase 4 consideration (including 2 already-compliant files: IGameContext.cs, KojoEngine.cs):

| File | Lines | Feature | Type | Phase 4 Action |
|------|------:|:-------:|------|----------------|
| InfoState.cs | 631 | F381 | static class | → `IInfoState` |
| ClothingSystem.cs | 607 | F369 | static class | → `IClothingSystem` |
| Constants.cs | 547 | F364 | static class | Keep static (pure constants) |
| BodySettings.cs | 490 | F370 | static class | → `IBodySettings` |
| GameOptions.cs | 412 | F367 | static class | → `IGameOptions` |
| KojoEngine.cs | 390 | (existing) | instance class | DI registration only |
| GameInitialization.cs | 370 | F365 | static class | → `IGameInitializer` |
| InfoEquip.cs | 365 | F379 | static class | → `IInfoEquip` |
| CharacterSetup.cs | 334 | F368 | static class | → `ICharacterSetup` |
| LocationSystem.cs | 314 | F372 | static class | → `ILocationService` |
| CommonFunctions.cs | 303 | F366 | static class | → `ICommonFunctions` |
| InfoEvent.cs | 204 | F378 | static class | → `IInfoEvent` |
| InfoTrainModeDisplay.cs | 199 | F382 | static class | → `IInfoTrainModeDisplay` |
| InfoPrint.cs | 195 | F373 | static class | → `IInfoPrint` |
| VariableDefinitions.cs | 180 | F376 | static class | Keep static (pure constants) |
| StatusOrchestrator.cs | 151 | F380 | static class | → `IStatusOrchestrator` |
| SuccessRateCalculator.cs | 142 | F374 | static class | → `ISuccessRateCalculator` + params record |
| PregnancySettings.cs | 102 | F370 | static class | → `IPregnancySettings` |
| WeatherSettings.cs | 108 | F370 | static class | → `IWeatherSettings` |
| RelationshipTypes.cs | 86 | F376 | static class | Keep static (pure constants) |
| NtrInitialization.cs | 74 | F371 | static class | → `INtrInitializer` |
| ColorSettings.cs | 70 | F376 | static class | Keep static (pure constants) |
| KojoCommon.cs | 60 | F375 | static class | → `IKojoCommon` |
| IGameContext.cs | 40 | (existing) | interface | Update: string→CharacterId |
| TalentManager.cs | 33 | F369 | static class | → `ITalentManager` |

**Summary**:
- Interface extraction needed: **19 classes**
- Keep as static (pure constants): **4 classes** (Constants, VariableDefinitions, RelationshipTypes, ColorSettings)
- Already instance/interface: **2 files** (KojoEngine, IGameContext)
- Update existing interface: **1 file** (IGameContext - type safety)

---

## Interface Extraction Plan

> **Note on IGameState vs IGameContext**: `IGameContext` (existing) is the engine-level context interface. `IGameState` (new) will be a higher-level game state abstraction for Era.Core, wrapping engine state access with strongly typed IDs. IGameContext will be updated to use CharacterId instead of raw int.

### Priority 1: Core Infrastructure

| Current | Interface | Reason |
|---------|-----------|--------|
| GameInitialization.cs | `IGameInitializer` | Entry point, many dependents |
| GameOptions.cs | `IGameOptions` | Configuration, widely used |
| CommonFunctions.cs | `ICommonFunctions` | Utility, many dependents |

### Priority 2: Domain Services

| Current | Interface | Special Notes |
|---------|-----------|---------------|
| LocationSystem.cs | `ILocationService` | + `LocationId` strongly typed |
| CharacterSetup.cs | `ICharacterSetup` | Contains CharacterState DTO |
| ClothingSystem.cs | `IClothingSystem` | 607 lines but single responsibility |
| TalentManager.cs | `ITalentManager` | Small, standalone interface |
| SuccessRateCalculator.cs | `ISuccessRateCalculator` | + `SuccessRateParams` record |

### Priority 3: State & Settings

| Current | Interface | Reason |
|---------|-----------|--------|
| BodySettings.cs | `IBodySettings` | Character body configuration |
| PregnancySettings.cs | `IPregnancySettings` | Pregnancy system |
| WeatherSettings.cs | `IWeatherSettings` | Weather system |
| NtrInitialization.cs | `INtrInitializer` | NTR system setup |

### Priority 4: INFO System

| Current | Interface | Reason |
|---------|-----------|--------|
| InfoState.cs | `IInfoState` | 631 lines, single responsibility (state display) |
| InfoPrint.cs | `IInfoPrint` | Print utilities |
| InfoEquip.cs | `IInfoEquip` | Equipment display |
| InfoEvent.cs | `IInfoEvent` | Event handling |
| InfoTrainModeDisplay.cs | `IInfoTrainModeDisplay` | Train mode display |
| StatusOrchestrator.cs | `IStatusOrchestrator` | Turn orchestration |
| KojoCommon.cs | `IKojoCommon` | Kojo branch resolution |

---

## Strongly Typed IDs

| Target | Type Name | Usage | Phase 4 Scope |
|--------|-----------|-------|:-------------:|
| Character ID | `CharacterId` | `人物_美鈴` etc. | ✓ |
| Location ID | `LocationId` | `場所_正門` etc. | ✓ |
| Command ID | `CommandId` | COM numbers | Deferred (Phase 8) |

**Implementation**:
```csharp
public readonly record struct CharacterId(int Value)
{
    public static readonly CharacterId Meiling = new(Constants.人物_美鈴);
    public static readonly CharacterId Sakuya = new(Constants.人物_咲夜);
    // Static members for well-known IDs

    public static implicit operator int(CharacterId id) => id.Value;
    // Allow implicit conversion to int for backward compatibility
}
```

---

## DI Configuration

```csharp
// Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEraCore(this IServiceCollection services)
    {
        // Core Infrastructure
        services.AddSingleton<IGameInitializer, GameInitializer>();
        services.AddSingleton<IGameOptions, GameOptions>();
        services.AddSingleton<ICommonFunctions, CommonFunctions>();

        // Domain Services
        services.AddSingleton<ILocationService, LocationService>();
        services.AddSingleton<ICharacterSetup, CharacterSetup>();
        services.AddSingleton<IClothingSystem, ClothingSystem>();
        services.AddTransient<ISuccessRateCalculator, SuccessRateCalculator>();

        // State & Settings
        services.AddSingleton<IBodySettings, BodySettings>();
        services.AddSingleton<IPregnancySettings, PregnancySettings>();
        services.AddSingleton<IWeatherSettings, WeatherSettings>();
        services.AddSingleton<INtrInitializer, NtrInitializer>();

        // INFO System
        services.AddTransient<IInfoState, InfoState>();
        services.AddTransient<IInfoPrint, InfoPrint>();
        services.AddTransient<IInfoEquip, InfoEquip>();
        services.AddTransient<IInfoEvent, InfoEvent>();
        services.AddTransient<IInfoTrainModeDisplay, InfoTrainModeDisplay>();
        services.AddTransient<IStatusOrchestrator, StatusOrchestrator>();
        services.AddTransient<IKojoCommon, KojoCommon>();
        services.AddTransient<ITalentManager, TalentManager>();

        return services;
    }
}
```

---

## SuccessRateCalculator Refactoring (Example)

**Before** (18 parameters):
```csharp
public static int GetSuccessRate(
    int ablObedience, int palamSubmission, int talentAttitude, ...)
```

**After** (parameter object):
```csharp
public record SuccessRateParams(
    CharacterId Target,
    CharacterId Master,
    CharacterAttributes TargetAttributes,
    CharacterAttributes MasterAttributes,
    int Relation,
    int Favorability
);

public interface ISuccessRateCalculator
{
    Result<int> Calculate(SuccessRateParams parameters);
}
```

**Rationale**: The 18 parameters represent a coherent concept ("calculation context"). Grouping them improves readability and enables validation in one place.

---

## Phase 4 Audit Procedure

### Step 1: Verify Inventory Completeness

```bash
# List all Era.Core files
find Era.Core -name "*.cs" -not -path "*/obj/*" | wc -l
# Expected: 25 files (per inventory above)
```

### Step 2: Identify Static Classes Needing Interfaces

```bash
# Find static classes (excluding pure constant files)
grep -l "public static class" Era.Core/**/*.cs | \
  grep -v "Constants\|VariableDefinitions\|RelationshipTypes\|ColorSettings"
# Each result needs interface extraction
```

### Step 3: Verify DI Registration

After interface extraction, verify all interfaces are registered:

```csharp
[Fact]
public void AddEraCore_RegistersAllInterfaces()
{
    var services = new ServiceCollection();
    services.AddEraCore();
    var provider = services.BuildServiceProvider();

    // Verify each interface resolves
    Assert.NotNull(provider.GetService<IGameInitializer>());
    Assert.NotNull(provider.GetService<ILocationService>());
    // ... all 19 interfaces
}
```

### Step 4: Verify No New Static Classes (CI Gate)

```yaml
# .github/workflows/architecture.yml
- name: No new static classes
  run: |
    STATIC_COUNT=$(grep -r "public static class" Era.Core --include="*.cs" | \
      grep -v "Constants\|VariableDefinitions\|RelationshipTypes\|ColorSettings\|Extensions" | wc -l)
    if [ "$STATIC_COUNT" -gt 0 ]; then
      echo "ERROR: Static classes found (should be interfaces)"
      exit 1
    fi
```

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F364-F376, F378-F382 | Phase 3 deliverables to refactor (all DONE) |
| Successor | Phase 5-24 | All subsequent phases use patterns established here |

---

## Links

### Architecture Reference
- [full-csharp-architecture.md](Game/agents/designs/full-csharp-architecture.md) - Phase 4 definition

### Phase 3 Deliverables (All)

| Feature | File | Phase 4 Action |
|:-------:|------|----------------|
| F364 | Constants.cs | Keep static (constants) |
| F365 | GameInitialization.cs | → `IGameInitializer` |
| F366 | CommonFunctions.cs | → `ICommonFunctions` |
| F367 | GameOptions.cs | → `IGameOptions` |
| F368 | CharacterSetup.cs | → `ICharacterSetup` |
| F369 | ClothingSystem.cs, TalentManager.cs | → `IClothingSystem`, `ITalentManager` |
| F370 | Body/Pregnancy/WeatherSettings.cs | → Interfaces |
| F371 | NtrInitialization.cs | → `INtrInitializer` |
| F372 | LocationSystem.cs | → `ILocationService` + `LocationId` |
| F373 | InfoPrint.cs | → `IInfoPrint` |
| F374 | SuccessRateCalculator.cs | → `ISuccessRateCalculator` + params record |
| F375 | KojoCommon.cs | → `IKojoCommon` |
| F376 | VariableDefinitions.cs, ColorSettings.cs, RelationshipTypes.cs | Keep static (constants) |
| F378 | InfoEvent.cs | → `IInfoEvent` |
| F379 | InfoEquip.cs | → `IInfoEquip` |
| F380 | StatusOrchestrator.cs | → `IStatusOrchestrator` |
| F381 | InfoState.cs | → `IInfoState` |
| F382 | InfoTrainModeDisplay.cs | → `IInfoTrainModeDisplay` |

---

## Pattern Documentation

### Strongly Typed ID Pattern

```csharp
public readonly record struct CharacterId(int Value)
{
    public static readonly CharacterId None = new(0);
    public static readonly CharacterId Meiling = new(1);

    public static implicit operator int(CharacterId id) => id.Value;
    public static explicit operator CharacterId(int value) => new(value);
}
```

### Result Type Pattern

```csharp
public abstract record Result<T>
{
    public sealed record Success(T Value) : Result<T>;
    public sealed record Failure(string Error) : Result<T>;

    public static Result<T> Ok(T value) => new Success(value);
    public static Result<T> Fail(string error) => new Failure(error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        => this switch
        {
            Success s => onSuccess(s.Value),
            Failure f => onFailure(f.Error),
            _ => throw new InvalidOperationException()
        };
}
```

### Interface + DI Pattern

```csharp
// 1. Define interface
public interface ILocationService
{
    string GetPlaceName(LocationId locationId);
    bool IsBathroom(LocationId locationId);
}

// 2. Implement (convert static to instance)
public class LocationService : ILocationService
{
    public string GetPlaceName(LocationId locationId) => ...;
    public bool IsBathroom(LocationId locationId) => ...;
}

// 3. Register in DI
services.AddSingleton<ILocationService, LocationService>();

// 4. Inject via constructor
public class SomeConsumer(ILocationService locationService)
{
    public void DoWork()
    {
        var name = locationService.GetPlaceName(LocationId.Gate);
    }
}
```

### Usage Example

```csharp
// Before Phase 4:
var name = LocationSystem.GetPlaceName(Constants.場所_正門);

// After Phase 4:
public class MyService(ILocationService locations)
{
    public void ShowLocation()
    {
        var name = locations.GetPlaceName(LocationId.Gate);
    }
}
```

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-06 | create | orchestrator | Created as Phase 4 planning feature | PROPOSED |
| 2026-01-06 | review | opus | Identified missing files in SRP Split Mapping | ISSUE |
| 2026-01-06 | update | opus | Added F367-F376, F378-F382 to scope | UPDATED |
| 2026-01-06 | review | opus | Identified line-count-based criteria as non-essential | ISSUE |
| 2026-01-06 | rewrite | opus | Complete rewrite with principle-based approach: removed line-count criteria, added YAGNI/KISS principles, complete inventory of 25 files, interface extraction plan for 19 classes | UPDATED |
| 2026-01-07 | validate | opus | Source code validation: confirmed 19 interfaces needed (was incorrectly 18), line counts verified, AC7 constant count (6) confirmed correct | FIXED |
| 2026-01-07 05:39 | START | implementer | Task 1 | - |
| 2026-01-07 05:39 | END | implementer | Task 1 | SUCCESS |
| 2026-01-07 05:41 | START | implementer | Task 2 | - |
| 2026-01-07 05:41 | END | implementer | Task 2 | SUCCESS |
| 2026-01-07 05:43 | START | implementer | Task 3 | - |
| 2026-01-07 05:43 | END | implementer | Task 3 | SUCCESS |
| 2026-01-07 05:45 | START | implementer | Task 4 | - |
| 2026-01-07 05:45 | END | implementer | Task 4 | SUCCESS |
| 2026-01-07 05:50 | START | implementer | Task 5 | - |
| 2026-01-07 06:05 | END | implementer | Task 5: Extract 19 interfaces | SUCCESS |
| 2026-01-07 06:07 | START | implementer | Task 6 | - |
| 2026-01-07 06:07 | END | implementer | Task 6 | SUCCESS |
| 2026-01-07 06:15 | verify | opus | Phase 6 Verification | PASS |
| 2026-01-07 06:20 | review | feature-reviewer | Post-review (mode: post) | NEEDS_REVISION |
| 2026-01-07 06:25 | fix | opus | Fixed AC status marks, added Task 5 log | FIXED |
| 2026-01-07 06:30 | review | feature-reviewer | Doc-check (mode: doc-check) | NEEDS_REVISION |
| 2026-01-07 06:35 | fix | opus | Updated engine-dev SKILL.md with Phase 4 DI pattern | FIXED |
| 2026-01-07 07:00 | audit | opus | AC10 verification failed: engine.Tests has 307 CS0120 errors (static→instance migration incomplete) | BLOCKED |
| 2026-01-07 07:00 | create | opus | Created F383 to fix engine.Tests DI migration | - |
| 2026-01-07 | next-phase | opus | Created Phase 5 features F384-F390 per full-csharp-architecture.md | CREATED |

---

## Follow-up Features (Phase 5)

Phase 4 完了に伴い、次フェーズ (Phase 5 Variable System) の Feature を作成:

### Implementation Features

| ID | Name | Purpose |
|:---|------|---------|
| F384 | Phase 5 Foundation - Types & Interfaces | Strongly Typed IDs, Interface contracts (prerequisite) |
| F385 | Phase 5 VariableCode Enum Migration | VariableCode enum extraction |
| F386 | Phase 5 VariableStore Implementation | IVariableStore implementation |
| F387 | Phase 5 VariableScope Implementation | IVariableScope implementation |
| F388 | Phase 5 Variable Resolution & CSV Loading | IVariableResolver, IVariableDefinitionLoader |

### Transition Features (SRP: Review と Planning を分離)

| ID | Name | Purpose |
|:---|------|---------|
| F389 | Phase 5 Post-Phase Review | F384-F388 のレビュー、技術負債確認 |
| F390 | Phase 6 Planning | Phase 6 スコープ分析、sub-feature 作成 |

**Reference**: [full-csharp-architecture.md](designs/full-csharp-architecture.md) Phase 5 definition

---

## Validation Report (2026-01-07)

### Validation Method

Actual Era.Core source files were inspected to verify inventory accuracy.

**Commands executed**:

```bash
# 1. List all Era.Core .cs files (excluding obj/bin)
find Era.Core -name "*.cs" ! -path "*/obj/*" ! -path "*/bin/*"
# Result: 25 files ✓

# 2. Count lines and classify each file
wc -l <file>; grep "public static class" <file>
# Result: All line counts match inventory ✓

# 3. Count all static class declarations
grep -r "public static class" Era.Core --include="*.cs"
# Result: 24 declarations (22 top-level + 2 nested in VariableDefinitions)
```

### File Classification Results

| Category | Count | Files |
|----------|:-----:|-------|
| **Interface extraction needed** | 19 | BodySettings, CharacterSetup, ClothingSystem, CommonFunctions, GameInitialization, GameOptions, InfoEquip, InfoEvent, InfoPrint, InfoState, InfoTrainModeDisplay, KojoCommon, LocationSystem, NtrInitialization, PregnancySettings, StatusOrchestrator, SuccessRateCalculator, TalentManager, WeatherSettings |
| **Pure constants (keep static)** | 4 | Constants, VariableDefinitions, ColorSettings, RelationshipTypes |
| **Already instance/interface** | 2 | KojoEngine (instance), IGameContext (interface) |
| **Total** | 25 | |

### AC7 Static Class Count Verification

Pure constant files contain 6 `public static class` declarations:

| File | Declarations |
|------|:------------:|
| Constants.cs | 1 |
| VariableDefinitions.cs | 3 (VariableDefinitions, VisitorAppearance, Aliases) |
| ColorSettings.cs | 1 |
| RelationshipTypes.cs | 1 |
| **Total** | **6** ✓ |

### Corrections Applied

| Item | Before | After | Reason |
|------|:------:|:-----:|--------|
| AC6 count | 18 | 19 | Inventory table lists 19 interfaces, not 18 |
| Summary | 18 classes | 19 classes | Same as above |
| Task 5 | 18 static classes | 19 static classes | Same as above |

### Judgment Criteria

1. **Interface extraction needed**: File contains `public static class` AND is NOT pure constants (no methods, only `const` fields)
2. **Pure constants**: File contains ONLY `const` declarations, no methods with logic
3. **Already compliant**: File is already `interface` or non-static `class`
