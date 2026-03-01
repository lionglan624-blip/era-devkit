# Feature 383: Phase 4 Test Migration

## Status: [DONE]

**Unblocks**: F377 (Phase 4 Architecture Refactoring Planning)

## Type: engine

## Created: 2026-01-07

---

## Summary

Fix 614 compilation errors in engine.Tests caused by F377's static-to-instance class conversion. Tests still use static method calls (e.g., `ClothingSystem.IsDressed(...)`) but Era.Core classes are now instance classes implementing interfaces.

---

## Background

### Problem

F377 converted 19 static classes to instance classes with DI support, but did not update engine.Tests. Result:

```
614× CS0120: 静的でないフィールド、メソッド、またはプロパティ 'X' で、オブジェクト参照が必要です
```

### Affected Test Files

| File | Error Count | Classes Used |
|------|:-----------:|--------------|
| GameInitializationTests.cs | 126 | GameInitialization |
| ClothingSystemTests.cs | 86 | ClothingSystem |
| HeadlessIntegrationTests.cs | 82 | Various |
| CommonFunctionsTests.cs | 74 | CommonFunctions |
| CharacterSetupTests.cs | 64 | CharacterSetup, TalentManager |
| StateSettingsTests.cs | 50 | WeatherSettings, BodySettings, PregnancySettings |
| LocationSystemTests.cs | 46 | LocationSystem |
| SuccessRateCalculatorTests.cs | 42 | SuccessRateCalculator |
| KojoCommonTests.cs | 34 | KojoCommon |
| NtrInitializationTests.cs | 10 | NtrInitialization |

**Total**: 614 errors

### Root Cause

```csharp
// Before F377 (static)
public static class ClothingSystem
{
    public static bool IsDressed(...) { ... }
}

// After F377 (instance)
public class ClothingSystem : IClothingSystem
{
    public bool IsDressed(...) { ... }  // No longer static
}

// Tests still call:
ClothingSystem.IsDressed(...)  // CS0120 error
```

---

## Design

### Approach: Direct Instantiation (Not Full DI)

For unit tests, use direct instantiation with mock dependencies rather than full DI container setup.

**Pattern**:
```csharp
// Before
[Fact]
public void Test_IsDressed()
{
    var result = ClothingSystem.IsDressed(characterId, getCflag);
    Assert.True(result);
}

// After
[Fact]
public void Test_IsDressed()
{
    var sut = new ClothingSystem();  // Or with mock dependencies
    var result = sut.IsDressed(characterId, getCflag);
    Assert.True(result);
}
```

### Classes Requiring Constructor Dependencies

Some classes have constructor dependencies that must be provided:

| Class | Dependencies |
|-------|--------------|
| GameInitialization | IBodySettings, IPregnancySettings, IWeatherSettings, INtrInitializer |
| CharacterSetup | ICommonFunctions |
| InfoPrint | ICommonFunctions |

**For tests**: Create minimal mock implementations or use the actual implementations where appropriate.

**Dependency Resolution Order**:
1. Create parameterless classes first: `new CommonFunctions()`, `new BodySettings()`, etc.
2. Create dependent classes: `new CharacterSetup(new CommonFunctions())`, `new InfoPrint(new CommonFunctions())`
3. Create GameInitialization: `new GameInitialization(new BodySettings(), new PregnancySettings(), new WeatherSettings(), new NtrInitialization())`

### HeadlessIntegrationTests Strategy

HeadlessIntegrationTests calls methods on GameInitialization in two categories:

**In IGameInitializer interface** (currently called statically, need conversion to instance):
- SetGameModeFlag, GetGameModeFlag, SetNTRPatchFlag, GetNTRPatchFlag, SetNTRAllOnModeFlags
- EventFirst, EventLoad, QuickStartSetup

**Stub methods NOT in IGameInitializer** (currently called statically, need conversion to instance):
- DefaultOption, CustomCharaMake, CharaCustum, VirginCustom, ReverseMode1, ShortCutMode
- VersionUp, ClothesSetting, BodyDetailInit, UterusVolumeInit, TemperatureToleranceInit
- NTRSetStayoutMaximum

**Approach**: Use concrete `GameInitialization` type in tests (not interface) since tests call stub methods not in the interface.

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | engine.Tests builds without errors | build | dotnet build | succeeds | 0 errors | [x] |
| 2 | All engine.Tests pass | test | dotnet test | succeeds | All green | [x] |
| 3 | Era.Core.Tests still pass | test | dotnet test | succeeds | All green | [x] |
| 4 | No static method calls to converted classes | code | Grep | count_equals | 0 | [x] |

### AC Details

**AC1**: `dotnet build engine.Tests/uEmuera.Tests.csproj` → 0 errors
**AC2**: `dotnet test engine.Tests/uEmuera.Tests.csproj` → All pass
**AC3**: `dotnet test Era.Core.Tests/` → All pass (regression check)
**AC4**: Verify no static calls to converted classes in engine.Tests. For each class, run:
```
Grep pattern="ClassName\." path="engine.Tests" glob="*.cs" output_mode="content"
```
Each match line should contain `new ClassName`, `_className.`, `sut.`, or similar instance access pattern. If any line shows `ClassName.MethodName(` without a preceding `new` or instance variable, that is a static call error.

Classes (in engine.Tests): ClothingSystem, GameInitialization, CommonFunctions, CharacterSetup, LocationSystem, SuccessRateCalculator, TalentManager, BodySettings, PregnancySettings, WeatherSettings, NtrInitialization, KojoCommon

Note: GameOptions, InfoState, InfoPrint, InfoEquip, InfoEvent, InfoTrainModeDisplay, StatusOrchestrator are only tested in Era.Core.Tests (covered by AC3)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,4 | Fix simple instantiation tests - no constructor params (ClothingSystem, BodySettings, PregnancySettings, WeatherSettings, LocationSystem, SuccessRateCalculator, KojoCommon). Note: TalentManager calls are in CharacterSetupTests.cs | [x] |
| 2 | 1,4 | Fix ICommonFunctions-dependent tests (CharacterSetup, InfoPrint) | [x] |
| 3 | 1,4 | Fix CommonFunctions tests (simple instantiation) | [x] |
| 4 | 1,4 | Fix GameInitialization tests (4-dependency injection) | [x] |
| 5 | 1,4 | Fix HeadlessIntegrationTests (use concrete GameInitialization type for stub methods) | [x] |
| 6 | 1,4 | Fix NtrInitialization tests | [x] |
| 7 | 2,3 | Run all tests and verify pass | [x] |
| 8 | 4 | Final verification: no static calls remain | [x] |

---

## Implementation Notes

### Test Helper Pattern (Optional)

If many tests need the same setup, consider a helper:

```csharp
public static class TestFactory
{
    public static ClothingSystem CreateClothingSystem() => new ClothingSystem();

    public static GameInitialization CreateGameInitialization() => new GameInitialization(
        new BodySettings(),
        new PregnancySettings(),
        new WeatherSettings(),
        new NtrInitialization()
    );
}
```

### Preserving Test Intent

When converting tests:
1. Keep the same test logic
2. Only change how the SUT (System Under Test) is obtained
3. Do not modify assertions or test data

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F377 | Caused this breakage |
| Successor | None | Unblocks F377 completion |

---

## Links

- [feature-377.md](feature-377.md) - Phase 4 Planning (blocked by this)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Architecture reference

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-07 | create | opus | Created to fix F377 test breakage | PROPOSED |
| 2026-01-07 07:52 | END | implementer | Task 6 | SUCCESS |
| 2026-01-07 07:53 | START | implementer | Task 5 | - |
| 2026-01-07 07:54 | START | implementer | Task 4 | - |
| 2026-01-07 07:54 | END | implementer | Task 4 | SUCCESS |
| 2026-01-07 07:53 | END | implementer | Task 5 | SUCCESS |
| 2026-01-07 07:54 | START | implementer | Task 3 | - |
| 2026-01-07 07:54 | END | implementer | Task 3 | SUCCESS |
| 2026-01-07 07:54 | START | implementer | Task 2 | - |
| 2026-01-07 07:54 | END | implementer | Task 2 | SUCCESS |
| 2026-01-07 07:56 | START | implementer | Task 1 | - |
| 2026-01-07 07:57 | END | implementer | Task 1 | SUCCESS |
| 2026-01-07 08:00 | - | ac-tester | AC 1-4 verification | PASS:4/4 |
| 2026-01-07 08:00 | - | feature-reviewer | post-review | READY |
| 2026-01-07 08:00 | - | feature-reviewer | doc-check | READY |
