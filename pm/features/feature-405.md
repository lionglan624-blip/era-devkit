# Feature 405: Callback DI Formalization

## Status: [DONE]

## Phase: 7 (Technical Debt Consolidation)

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

## Created: 2026-01-08

---

## Summary

Formalize ad-hoc callback injection patterns into proper DI factory registration. Currently, callbacks like `Func<CupIndex, int>` and TEQUIP accessors are passed directly as method parameters. This feature establishes `CallbackFactories.cs` to register these callbacks as singletons in the DI container.

---

## Background

### Philosophy (Mid-term Vision)

**Phase 7: Technical Debt Consolidation**: Phase 5-6で蓄積した技術負債の解消 + Phase 8以降の基盤確立。

Callback injection パターンがDI未登録のまま使用されている。本Featureは DI正式化により技術負債ゼロ原則に貢献する。

### Problem (Current Issue)

Current callback patterns are scattered across method signatures:

| Component | Current Pattern | Issue |
|-----------|-----------------|-------|
| CharacterStateTracker.ProcessExperienceGrowth | `Func<CupIndex, int> getCup, Action<CupIndex, int> setCup` parameters | Manual callback passing |
| VirginityManager.CheckLostVirginity | TODO comment: "Determine from TEQUIP via callback" | Missing TEQUIP accessor |
| EquipmentProcessor | Comment: "Check each TEQUIP flag" | No DI-registered accessor |
| OrgasmProcessor | Comment: "requires TEQUIP and TSTR access" | No DI-registered accessor |

These ad-hoc callbacks:
- Clutter method signatures (3+ parameters for simple accessors)
- Require manual wiring at every call site
- Lack centralized definition and documentation
- Cannot be easily mocked for testing

### Goal (What to Achieve)

1. Create `Era.Core/DependencyInjection/CallbackFactories.cs` with `AddTrainingCallbacks()` extension
2. Register CUP accessor factory: `Func<CharacterId, CupIndex, int>` (for ExperienceGrowthCalculator)
3. Register TEQUIP accessor factory: `Func<CharacterId, int, bool>` (equipment flag checker)
4. Update `ServiceCollectionExtensions.AddEraCore()` to call `AddTrainingCallbacks()`
5. Document callback factory patterns for future extensions

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CallbackFactories.cs exists | file | Glob | exists | Era.Core/DependencyInjection/CallbackFactories.cs | [x] |
| 2 | AddTrainingCallbacks extension exists | code | Grep | contains | public static IServiceCollection AddTrainingCallbacks | [x] |
| 3 | CUP accessor factory registered | code | Grep CallbackFactories.cs | contains | Func<CharacterId, CupIndex, int> | [x] |
| 4 | CUP factory uses ITrainingVariables | code | Grep CallbackFactories.cs | contains | ITrainingVariables | [x] |
| 5 | TEQUIP accessor factory registered | code | Grep CallbackFactories.cs | contains | Func<CharacterId, int, bool> | [x] |
| 6 | TEQUIP factory uses ITEquipVariables | code | Grep CallbackFactories.cs | contains | ITEquipVariables | [x] |
| 7 | AddTrainingCallbacks called in AddEraCore | code | Grep ServiceCollectionExtensions.cs | contains | AddTrainingCallbacks | [x] |
| 8 | XML documentation for CUP factory | code | Grep CallbackFactories.cs | contains | CUP array accessor for ExperienceGrowthCalculator | [x] |
| 9 | XML documentation for TEQUIP factory | code | Grep CallbackFactories.cs | contains | TEQUIP accessor for VirginityManager | [x] |
| 10 | C# build succeeds | build | dotnet | succeeds | - | [x] |
| 11 | All Era.Core.Tests pass | test | dotnet | succeeds | Era.Core.Tests | [x] |
| 12 | getCup callback removed from public interfaces | code | Grep Era.Core/Character/I*.cs | not_contains | Func<CupIndex, int> getCup | [x] |
| 13 | setCup callback removed from public interfaces | code | Grep Era.Core/Character/I*.cs | not_contains | Action<CupIndex, int> setCup | [x] |

### AC Details

**AC#1**: Verify file exists at `C:\Era\era紅魔館protoNTR\Era.Core\DependencyInjection\CallbackFactories.cs`

**AC#2**: Extension method signature:
```csharp
public static IServiceCollection AddTrainingCallbacks(this IServiceCollection services)
```

**AC#3-4**: CUP accessor factory registration (for ExperienceGrowthCalculator):
```csharp
// Register CUP array accessor for ExperienceGrowthCalculator
services.AddSingleton<Func<CharacterId, CupIndex, int>>(sp =>
{
    var vars = sp.GetRequiredService<ITrainingVariables>();
    return (character, cupIndex) => vars.GetCup(character, cupIndex) switch
    {
        { IsSuccess: true } r => r.Value,
        _ => 0
    };
});
```

**AC#5-6**: TEQUIP accessor factory registration (equipment flag checker):
```csharp
// Register TEQUIP accessor for VirginityManager
services.AddSingleton<Func<CharacterId, int, bool>>(sp =>
{
    var vars = sp.GetRequiredService<ITEquipVariables>();
    return (character, index) => vars.GetTEquip(character, index) switch
    {
        { IsSuccess: true } r => r.Value != 0,
        _ => false
    };
});
```

**AC#7**: ServiceCollectionExtensions.cs update:
```csharp
public static IServiceCollection AddEraCore(this IServiceCollection services)
{
    // ... existing registrations ...

    // Callback Factories - Feature 405
    services.AddTrainingCallbacks();

    return services;
}
```

**AC#8-9**: XML documentation for each factory explaining purpose and usage pattern.

**AC#10**: `dotnet build Era.Core/Era.Core.csproj --configuration Debug`

**AC#11**: `dotnet test Era.Core.Tests/Era.Core.Tests.csproj --no-build`

**AC#12-13**: Remove ad-hoc callback parameters from CharacterStateTracker and ExperienceGrowthCalculator. Inject CUP accessor via constructor.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Create CallbackFactories.cs with AddTrainingCallbacks extension | [x] |
| 2 | 3,4,8 | Implement CUP accessor factory with ITrainingVariables dependency | [x] |
| 3 | 5,6,9 | Implement TEQUIP accessor factory with ITEquipVariables dependency | [x] |
| 4 | 7 | Update ServiceCollectionExtensions to call AddTrainingCallbacks | [x] |
| 5 | 12 | Update ICharacterStateTracker.ProcessExperienceGrowth signature (remove callback params) | [x] |
| 6 | 12 | Update IExperienceGrowthCalculator.ProcessExperienceGain signature (remove callback params) | [x] |
| 7 | 12 | Add Func<CharacterId, CupIndex, int> getCup to CharacterStateTracker constructor | [x] |
| 8 | 12 | Add Func<CharacterId, CupIndex, int> getCup to ExperienceGrowthCalculator constructor | [x] |
| 9 | 12,13 | Update ExperienceGrowthCalculator internal methods to use _getCup(target, cupIndex) and _trainingVariables.SetCup() | [x] |
| 10 | 10 | Verify C# build succeeds | [x] |
| 11 | 11 | Verify all Era.Core.Tests pass | [x] |

<!-- AC:Task 1:1 Rule: Tasks 5-9 implement AC#12-13 consumer migration -->
<!-- Note: T1-4 create factories, T5-9 migrate consumers, T10-11 verify -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F404 | Requires ITrainingVariables interface segregation |
| Related | F402 | StateChange hierarchy provides type safety foundation |
| Related | F396 | CharacterStateTracker will consume TEQUIP accessor |
| Related | F412 | ITEquipVariables interface (TEQUIP accessor) |
| Successor | F406 | Equipment/OrgasmProcessor will use registered callbacks |

**Dependencies satisfied**: F404 (ITrainingVariables) and F412 (ITEquipVariables) are both complete. All predecessor interfaces are available.

---

## Implementation Notes

### Callback Factory Pattern

**Design Principle**: Factories return delegates that encapsulate variable access with error handling.

**Design Decision**: CUP factory is maintained even though ITrainingVariables.GetCup exists. This ensures consistent DI pattern across all callback types (CUP, TEQUIP). Components inject factory only (not ITrainingVariables directly), using `_getCup(target, cupIndex)` internally.

**Usage Pattern**: CharacterId is obtained from the calling context (target parameter passed to ProcessExperienceGrowth/ProcessExperienceGain). The factory is injected via constructor; internal calls use `_getCup(target, cupIndex)` instead of callback parameters.

**SetCup Handling**: Only GetCup requires a factory wrapper (for Result<T> unwrapping to int). SetCup uses ITrainingVariables.SetCup directly since it returns void and doesn't need Result handling. ExperienceGrowthCalculator will inject both the getCup factory and ITrainingVariables for SetCup access.

**Example Pattern**:
```csharp
services.AddSingleton<Func<TParam, TResult>>(sp =>
{
    var dependency = sp.GetRequiredService<IDependency>();
    return (param) => dependency.Method(param) switch
    {
        { IsSuccess: true } r => r.Value,
        _ => defaultValue
    };
});
```

### GetTEquip Implementation Note

**Interface**: `ITEquipVariables.GetTEquip(CharacterId character, int equipmentIndex)` (provided by F412, now complete).

The TEQUIP accessor uses the segregated ITEquipVariables interface which requires both character ID and equipment index parameters.

### Future Extensions

This pattern can be extended for other callback types:
- `Func<CharacterId, PalamIndex, int>` - PALAM accessor
- `Func<CharacterId, SourceIndex, int>` - SOURCE accessor
- `Func<CharacterId, MarkIndex, int>` - MARK accessor

These are deferred until needed by specific processors.

---

## Review Notes

- **2026-01-09 FL iter1-2**: [resolved] TEQUIP signature Func<int,bool>→Func<CharacterId,int,bool> corrected
- **2026-01-09 FL iter1-2**: [resolved] AC#6 IVariableStore→ITEquipVariables corrected
- **2026-01-09 FL iter1-2**: [resolved] AC#9 CharacterStateTracker→VirginityManager corrected
- **2026-01-09 FL iter1-2**: [resolved] F412 dependency added to Links
- **2026-01-09 FL iter1-2**: [resolved] Task 3 description IVariableStore→ITEquipVariables corrected
- **2026-01-09 FL iter1-2**: [resolved] AC#3,5 HTML entity→actual angle brackets corrected
- **2026-01-09 FL**: [skipped] CUP factory duplication - user chose to maintain for consistent pattern
- **2026-01-09 FL**: [resolved] AC#12 (namespace import) removed as redundant
- **2026-01-09 FL**: [resolved] Tasks 5-9 expanded for interface signature changes and constructor injection

---

## 残課題

| Issue | Description | Priority | Target |
|-------|-------------|----------|--------|
| Result<T> error handling | Factory が 0 を返すエラーハンドリングから Result<T> への変更調査 | Low | → Tracked in F415 |
| Design doc sync | full-csharp-architecture.md line 1888 の TEQUIP signature 更新 | Low | ✅ Fixed |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-08 | create | opus | Created as Task 3 of F398 Phase 7 Planning | PROPOSED |
| 2026-01-09 07:22 | START | implementer | Task 1 | - |
| 2026-01-09 07:22 | END | implementer | Task 1 (includes T2-T3) | SUCCESS |
| 2026-01-09 07:24 | START | implementer | Task 4 | - |
| 2026-01-09 07:25 | END | implementer | Task 4 | SUCCESS |
| 2026-01-09 07:26 | START | implementer | Task 5 | - |
| 2026-01-09 07:27 | END | implementer | Task 5 | SUCCESS |
| 2026-01-09 07:30 | START | implementer | Tasks 7-9 combined | - |
| 2026-01-09 07:33 | END | implementer | Tasks 7-9 (includes T6,T10-T11) | SUCCESS |
| 2026-01-09 | END | finalizer | Feature 405 completed | DONE |

---

## Links

- [feature-398.md](feature-398.md) - Phase 7 Planning (parent feature)
- [feature-404.md](feature-404.md) - IVariableStore ISP Segregation (dependency)
- [feature-402.md](feature-402.md) - StateChange Equipment/Orgasm Migration (related)
- [feature-396.md](feature-396.md) - CharacterStateTracker will consume TEQUIP accessor (related)
- [feature-412.md](feature-412.md) - ITEquipVariables interface (related)
- [feature-406.md](feature-406.md) - Equipment/OrgasmProcessor Completion (successor)
- [feature-415.md](feature-415.md) - Result<T> Error Handling Investigation (残課題 follow-up)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 7 architecture (lines 1868-1901)
- [ServiceCollectionExtensions.cs](../../Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) - Current DI registration
