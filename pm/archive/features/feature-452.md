# Feature 452: COM Analysis and Basic/Caressing Actions (Com0xx/Com1xx)

## Status: [DONE]

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

## Created: 2026-01-11

---

## Summary

Establish COM implementation foundation and migrate basic actions (Com0xx) and caressing commands (Com1xx).

**Scope**:
- COM architecture design (ComBase, IComRegistry, IComContext)
- Com0xx category (~20 files: conversation, movement, basic actions)
- Com1xx category (~30 files: kiss, touch, caressing)
- Equipment handler integration (EQUIP_COM42-48, EQUIP_COM104-106, EQUIP_COM146-148, EQUIP_COM183-189)

**Output**:
- `Era.Core/Commands/Com/ComBase.cs` - COM base class
- `Era.Core/Commands/Com/IComRegistry.cs` - COM registry interface
- `Era.Core/Commands/Com/ComRegistry.cs` - COM registry implementation
- `Era.Core/Commands/Com/IComContext.cs` - COM execution context interface
- `Era.Core/Commands/Com/ComContext.cs` - COM context implementation
- `Era.Core/Commands/Com/ComResult.cs` - COM result type
- `Era.Core/Commands/Com/Com0xx/*.cs` - Basic action implementations (~20 files)
- `Era.Core/Commands/Com/Com1xx/*.cs` - Caressing implementations (~30 files)

**Total Volume**: ~50 COM implementations, estimated 5,000-7,000 lines

---

## Background

### Philosophy (Mid-term Vision)

**Phase 12: COM Implementation** - Migrate all 150+ COMF*.ERB training command implementations to C# using Phase 4 design patterns (Strongly Typed IDs, DI, SRP), ensuring legacy equivalence and establishing single source of truth for game training logic.

### Problem (Current Issue)

Phase 11 completion enables Phase 12 COM migration:
- 150+ COMF*.ERB files need migration to C# (largest migration effort)
- COM architecture foundation needed (ComBase, IComRegistry, IComContext)
- Equipment handlers (F406 deferred) embedded in COMF files require extraction
- Com0xx/Com1xx categories represent ~50 files (~30% of total COM scope)

### Goal (What to Achieve)

1. **Design COM architecture** (ComBase, IComRegistry, IComContext)
2. **Migrate Com0xx** basic actions (~20 files)
3. **Migrate Com1xx** caressing commands (~30 files)
4. **Extract equipment handlers** (19 handlers: EQUIP_COM42-48 (7), 104-106 (3), 146-148 (3), 183-189 (6, no EQUIP_COM185))
5. **Verify legacy equivalence** for all migrated COMs
6. **Zero technical debt** in migrated code

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ComBase.cs exists | file | Glob | exists | Era.Core/Commands/Com/ComBase.cs | [x] |
| 2 | IComRegistry interface | code | Grep | contains | "public interface IComRegistry" | [x] |
| 3 | ComRegistry implementation | code | Grep | contains | "public class ComRegistry : IComRegistry" | [x] |
| 4 | IComContext interface | code | Grep | contains | "public interface IComContext" | [x] |
| 5 | DI registration | code | Grep | contains | "AddSingleton.*IComRegistry.*ComRegistry" | [x] |
| 6 | Com0xx directory exists | file | Glob | count_gte | Era.Core/Commands/Com/Com0xx/*.cs (1) | [x] |
| 7 | Com1xx directory exists | file | Glob | count_gte | Era.Core/Commands/Com/Com1xx/*.cs (1) | [x] |
| 8 | Com0xx implementation count | file | Glob | count_gte | 20 | [x] |
| 9 | Com1xx implementation count | file | Glob | count_gte | 30 | [x] |
| 10 | Equipment handler integration (Com0xx 42-48) | code | Grep | count_equals | "class Com4[2-8]\\s*:.*EquipmentComBase" (7) | [x] |
| 11 | Equipment handler integration (Com1xx 104-106) | code | Grep | count_equals | "class Com10[456]\\s*:.*EquipmentComBase" (3) | [x] |
| 12 | Equipment handler integration (Com1xx 146-148) | code | Grep | count_equals | "class Com14[678]\\s*:.*EquipmentComBase" (3) | [x] |
| 13 | Equipment handler integration (Com1xx 183-189) | code | Grep | count_equals | "class Com18[3-9]\\s*:.*EquipmentComBase" (6) | [x] |
| 14 | COM unit tests pass | test | Bash | succeeds | "dotnet test --filter \"Category=Com\"" | [x] |
| 15 | Zero technical debt (Com0xx) | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 16 | Zero technical debt (Com1xx) | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |

### AC Details

**AC#1**: ComBase.cs file existence
- Test: Glob pattern="Era.Core/Commands/Com/ComBase.cs"
- Base class for all COM implementations

**AC#2**: IComRegistry interface definition
- Test: Grep pattern="public interface IComRegistry" path="Era.Core/Commands/Com/"
- Registry interface for COM lookup

**AC#3**: ComRegistry implementation
- Test: Grep pattern="public class ComRegistry : IComRegistry" path="Era.Core/Commands/Com/"
- Concrete registry implementation with 150+ COM registration

**AC#4**: IComContext interface definition
- Test: Grep pattern="public interface IComContext" path="Era.Core/Commands/Com/"
- Execution context interface (Target, Actor, Abilities, Kojo)

**AC#5**: DI registration
- Test: Grep pattern="AddSingleton.*IComRegistry.*ComRegistry" path="Era.Core/DependencyInjection/"
- Verifies COM services registered in DI container

**AC#6**: Com0xx directory exists
- Test: Glob pattern="Era.Core/Commands/Com/Com0xx/*.cs", count >= 1
- Category subdirectory for basic actions (verified by file existence)

**AC#7**: Com1xx directory exists
- Test: Glob pattern="Era.Core/Commands/Com/Com1xx/*.cs", count >= 1
- Category subdirectory for caressing commands (verified by file existence)

**AC#8**: Com0xx implementation count
- Test: Glob pattern="Era.Core/Commands/Com/Com0xx/*.cs", count >= 20
- Verifies at least 20 Com0xx implementations (matching Summary ~20)

**AC#9**: Com1xx implementation count
- Test: Glob pattern="Era.Core/Commands/Com/Com1xx/*.cs", count >= 30
- Verifies at least 30 Com1xx implementations (matching Summary ~30)

**AC#10**: Equipment handler integration (Com0xx 42-48)
- Test: Grep pattern="class Com4[2-8]\s*:.*EquipmentComBase" path="Era.Core/Commands/Com/Com0xx/", count_equals 7
- Verifies all 7 equipment handlers (Com42-48) inherit from EquipmentComBase

**AC#11**: Equipment handler integration (Com1xx 104-106)
- Test: Grep pattern="class Com10[456]\s*:.*EquipmentComBase" path="Era.Core/Commands/Com/Com1xx/", count_equals 3
- Verifies all 3 equipment handlers (Com104-106) inherit from EquipmentComBase

**AC#12**: Equipment handler integration (Com1xx 146-148)
- Test: Grep pattern="class Com14[678]\s*:.*EquipmentComBase" path="Era.Core/Commands/Com/Com1xx/", count_equals 3
- Verifies all 3 equipment handlers (Com146-148) inherit from EquipmentComBase

**AC#13**: Equipment handler integration (Com1xx 183-189)
- Test: Grep pattern="class Com18[34689]\s*:.*EquipmentComBase" path="Era.Core/Commands/Com/Com1xx/", count_equals 6
- Verifies all 6 equipment handlers (Com183, 184, 186-189) inherit from EquipmentComBase (no EQUIP_COM185)

**AC#14**: COM unit tests pass
- Test: Bash command=`dotnet test --filter "Category=Com"`
- Tests use `[Trait("Category", "Com")]` attribute (xUnit v3 syntax)
- All COM implementations match legacy behavior

**AC#15**: Zero technical debt (Com0xx)
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Com/Com0xx/"
- Expected: 0 matches

**AC#16**: Zero technical debt (Com1xx)
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Com/Com1xx/"
- Expected: 0 matches

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4 | Design and implement COM architecture (ComBase, EquipmentComBase, IComRegistry, ComRegistry, IComContext, IEquipmentCom, IKojoEngine) | [x] |
| 2 | 5 | Register COM services in DI container | [x] |
| 3 | 6,8,10 | Migrate Com0xx basic actions (~20 files) with equipment handler integration | [x] |
| 4 | 7,9,11,12,13 | Migrate Com1xx caressing commands (~30 files) with equipment handler integration | [x] |
| 5 | 14 | Implement COM unit tests and verify legacy equivalence | [x] |
| 6 | 15,16 | Verify zero technical debt | [x] |

**Batch verification waiver (Task 1)**: Architecture interfaces are interdependent and must be created atomically.
**Batch verification waiver (Task 3, 4)**: Migration tasks grouped by COM category for practical implementation.
<!-- AC:Task 1:1 Rule: 16 ACs = 6 Tasks (grouped: architecture → DI → migration → tests → verification) -->

---

## Windows Reserved Name Note (Historical)

> Windows `COM1`-`COM9` 予約デバイス名により Git 追加不可。
> **対応**: `Com01.cs`-`Com09.cs` にゼロパディング (クラス名 `Com01`-`Com09`、ComId値は 1-9)。

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Source Migration Reference

**Legacy Location**: `Game/ERB/COMF*.ERB`

**Equipment Handler Locations** (from architecture.md Phase 12):

| ERB Function | Location | Purpose | Target Class |
|--------------|----------|---------|--------------|
| EQUIP_COM42 | COMF42.ERB:55 | クリキャップ効果 | Com42.ExecuteEquipmentEffect |
| EQUIP_COM43 | COMF43.ERB:60 | オナホール効果 | Com43.ExecuteEquipmentEffect |
| EQUIP_COM44 | COMF44.ERB:89 | バイブ効果 | Com44.ExecuteEquipmentEffect |
| EQUIP_COM45 | COMF45.ERB | アナルバイブ効果 | Com45.ExecuteEquipmentEffect |
| EQUIP_COM46 | COMF46.ERB | アナルビーズ効果 | Com46.ExecuteEquipmentEffect |
| EQUIP_COM47 | COMF47.ERB | ニプルキャップ効果 | Com47.ExecuteEquipmentEffect |
| EQUIP_COM48 | COMF48.ERB | 搾乳機効果 | Com48.ExecuteEquipmentEffect |
| EQUIP_COM104 | COMF104.ERB | アイマスク効果 | Com104.ExecuteEquipmentEffect |
| EQUIP_COM105 | COMF105.ERB | 縄緊縛効果 | Com105.ExecuteEquipmentEffect |
| EQUIP_COM106 | COMF106.ERB | ボールギャグ効果 | Com106.ExecuteEquipmentEffect |
| EQUIP_COM146 | COMF146.ERB | 浣腸効果 | Com146.ExecuteEquipmentEffect |
| EQUIP_COM147 | COMF147.ERB | 拡張バルーン効果 | Com147.ExecuteEquipmentEffect |
| EQUIP_COM148 | COMF148.ERB | アナル電極効果 | Com148.ExecuteEquipmentEffect |
| EQUIP_COM183 | COMF183.ERB | 氷結拘束効果 | Com183.ExecuteEquipmentEffect |
| EQUIP_COM184 | COMF184.ERB | 淫紋刻印効果 | Com184.ExecuteEquipmentEffect |
| EQUIP_COM186 | COMF186.ERB:90 | 特殊道具効果 | Com186.ExecuteEquipmentEffect |
| EQUIP_COM187 | COMF187.ERB | 媚薬効果 | Com187.ExecuteEquipmentEffect |
| EQUIP_COM188 | COMF188.ERB | 催眠効果 | Com188.ExecuteEquipmentEffect |
| EQUIP_COM189 | COMF189.ERB | 感度増幅効果 | Com189.ExecuteEquipmentEffect |

<!-- Note: EQUIP_COM185 does not exist (COMF185.ERB is "助手を犯す" special command) -->

**Integration Pattern**:
```csharp
// Era.Core/Training/EquipmentProcessor.cs (F406で実装済み)
// ProcessEquipment calls ComXX.ExecuteEquipmentEffect(target, result)
private void ProcessClitCap(CharacterId target, EquipmentResult result)
{
    // Phase 12: Delegate to Com42
    var com = _comRegistry.Get(new ComId(42));
    com.ExecuteEquipmentEffect(target, result);
}
```

### COM Architecture Design (Phase 4 Requirements)

**Strongly Typed IDs**:
```csharp
// Era.Core/Types/ComId.cs (to be created in Task 1)
public readonly record struct ComId(int Value);
```

**Interface Definition**:
```csharp
// Era.Core/Commands/Com/ICom.cs
using Era.Core.Types;

namespace Era.Core.Commands.Com;

public interface ICom
{
    ComId Id { get; }
    string Name { get; }
    Result<ComResult> Execute(IComContext context);
}

// Era.Core/Commands/Com/IEquipmentCom.cs
// Interface Segregation: Only equipment-enabled COMs implement this
public interface IEquipmentCom : ICom
{
    void ExecuteEquipmentEffect(CharacterId target, EquipmentResult result);
}
```

**Context Interface**:
```csharp
// Era.Core/Commands/Com/IComContext.cs
using Era.Core.Types;

namespace Era.Core.Commands.Com;

public interface IComContext
{
    CharacterId Target { get; }
    CharacterId Actor { get; }
    IAbilitySystem Abilities { get; }
    IKojoEngine Kojo { get; }  // Note: IKojoEngine interface to be created in Task 1
    Dictionary<string, object> EvalContext { get; }  // For KojoEngine.EvaluateConditions
    Dictionary<string, string> Placeholders { get; }  // For KojoEngine.Render
}

// Era.Core/IKojoEngine.cs (to be created in Task 1)
// Extracted interface from existing KojoEngine class (Era.Core/KojoEngine.cs)
namespace Era.Core;

public interface IKojoEngine
{
    object LoadYaml(string filePath);
    object EvaluateConditions(object dialogueData, Dictionary<string, object> context);
    string Render(object branch, Dictionary<string, string> placeholders);
}
```

**Base Class**:
```csharp
// Era.Core/Commands/Com/ComBase.cs
using Era.Core.Types;

namespace Era.Core.Commands.Com;

public abstract class ComBase : ICom
{
    public abstract ComId Id { get; }
    public abstract string Name { get; }

    public abstract Result<ComResult> Execute(IComContext context);

    protected int CalculatePleasure(IComContext ctx)
    {
        // Common pleasure calculation logic
        return 0; // Placeholder - actual implementation calculates from ctx
    }
}

// Era.Core/Commands/Com/EquipmentComBase.cs
// Base class for equipment-enabled COMs (ISP compliant)
public abstract class EquipmentComBase : ComBase, IEquipmentCom
{
    public abstract void ExecuteEquipmentEffect(CharacterId target, EquipmentResult result);
}
```

**Registry Interface**:
```csharp
// Era.Core/Commands/Com/IComRegistry.cs
using Era.Core.Types;

namespace Era.Core.Commands.Com;

public interface IComRegistry
{
    ICom Get(ComId id);
    bool TryGet(ComId id, out ICom com);
    IEnumerable<ICom> GetAll();
}
```

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs`:

```csharp
// COM Services - ComRegistry handles COM lookup via auto-discovery
services.AddSingleton<IComRegistry, ComRegistry>();
services.AddSingleton<IComContext, ComContext>();
services.AddSingleton<IKojoEngine, KojoEngine>();

// ComRegistry auto-discovers ICom implementations via reflection
// No individual COM registrations needed - ComRegistry.GetAll() scans assembly
```

### Implementation Pattern

Example COM implementation:
```csharp
// Era.Core/Commands/Com/Com1xx/Com100.cs
using Era.Core.Types;
using Era.Core.Ability;

namespace Era.Core.Commands.Com.Com1xx;

// Note: Class naming convention is ComXX (no suffix) to match AC regex patterns
public class Com100 : ComBase
{
    public override ComId Id => new ComId(100);
    public override string Name => "キス";

    public override Result<ComResult> Execute(IComContext ctx)
    {
        // Parameter calculation
        var pleasure = CalculatePleasure(ctx);

        // State modification via ApplyGrowth (IAbilitySystem interface)
        // Uses GrowthResult fluent API (Era.Core/Ability/AbilityGrowth.cs)
        // Note: ExpIndex/PalamIndex use explicit constructors (no well-known constants per F406)
        var growth = new GrowthResult()
            .AddExperience(new ExpIndex(100), pleasure);  // ExpIndex for Kiss
        // Palam changes handled via RESULT array in training flow (separate from GrowthResult)

        ctx.Abilities.ApplyGrowth(ctx.Target, growth);

        // Message generation via KojoEngine
        var dialogue = ctx.Kojo.LoadYaml($"口上/Com100/{ctx.Target}.yaml");
        var branch = ctx.Kojo.EvaluateConditions(dialogue, ctx.EvalContext);
        var message = ctx.Kojo.Render(branch, ctx.Placeholders);

        return Result<ComResult>.Ok(new ComResult
        {
            Success = true,
            Message = message
        });
    }
}
```

### Test Naming Convention

Test methods follow `Test{Category}Com{Number}` format:
- `TestBasicCom0()` - Basic conversation
- `TestBasicCom1()` - Basic movement
- `TestCaressingCom100()` - Kiss
- `TestCaressingCom101()` - Touch

This ensures AC#12 filter pattern matches correctly.

### Error Handling Pattern

Per Issue 2 in ENGINE.md:
- Invalid input (recoverable) → `Result<T>.Fail()` with descriptive Japanese message
- Programmer error (null arg) → `ArgumentNullException`
- COM not found → `Result<T>.Fail("COM_{id} は存在しません")`

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F450 | Phase 12 Planning (defines COM decomposition) |
| Predecessor | F406 | EquipmentProcessor (requires COM integration) |
| Reference | F405 | Callback implementation pattern |
| Reference | F392 | IAbilitySystem interface (used in IComContext) |
| Successor | F453 | Special Actions Migration (Com2xx) |

---

## Links

- [feature-450.md](feature-450.md) - Phase 12 Planning
- [feature-406.md](feature-406.md) - EquipmentProcessor (equipment handler deferred items)
- [feature-405.md](feature-405.md) - Callback implementation pattern
- [feature-392.md](feature-392.md) - IAbilitySystem interface (used in IComContext)
- [feature-453.md](feature-453.md) - Special Actions Migration (Com2xx)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 12 definition

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
<!-- Format: - **YYYY-MM-DD**: {reviewer feedback} -->
- **2026-01-11 FL iter1**: [resolved] Phase2-Validate - Volume: Total Volume (5,000-7,000 lines) exceeds engine type limit (~300 lines). **Volume waiver granted**: COM architecture + initial migration requires atomicity as designed in F450 Phase 12 Planning.
- **2026-01-11 FL iter4**: [resolved] Phase2-Validate - Task 1 Scope: Task 1 covers 7+ files. **Batch waiver granted**: Architecture interfaces are interdependent (documented in Tasks section).

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-11 20:50 | create | feature-builder | Created from F450 Phase 12 Planning | PROPOSED |
| 2026-01-11 22:32 | START | implementer | Task 1 | - |
| 2026-01-11 22:32 | END | implementer | Task 1 | SUCCESS |
| 2026-01-11 22:39 | START | implementer | Task 3 | - |
| 2026-01-11 23:15 | END | implementer | Task 3 | SUCCESS |
| 2026-01-11 22:43 | START | implementer | Task 4 | - |
| 2026-01-11 22:43 | END | implementer | Task 4 | SUCCESS |
| 2026-01-11 22:47 | START | implementer | Task 5 | - |
| 2026-01-11 22:47 | END | implementer | Task 5 | SUCCESS |
| 2026-01-11 22:50 | START | opus | Task 6 Zero tech debt verification | - |
| 2026-01-11 22:50 | END | opus | Task 6 Zero tech debt verification | SUCCESS |
