---
name: csharp-14
description: C# 14 features reference. Use when implementing engine features with .NET 10, writing DDD domain models, or creating COM implementations that leverage modern C# patterns.
---

# C# 14 Features Reference

> **Purpose**: Document C# 14 features relevant to ERA game development
> **Runtime**: .NET 10+
> **Last Updated**: 2026-02-12

## When to Use This Skill

- Implementing engine features (Era.Core, uEmuera.Headless)
- Writing DDD domain models (Phase 12+)
- Creating COM implementations
- Refactoring legacy code to modern patterns

---

## Primary Constructors

**Feature**: Define class constructor parameters directly in the class declaration, eliminating boilerplate field declarations.

### Syntax

```csharp
// C# 14 Primary Constructor
public class TrainingProcessor(
    IVariableStore variableStore,
    ITrainingVariables trainingVariables,
    ICharacterStateTracker stateTracker) : ITrainingProcessor
{
    // Parameters become private fields automatically
    public Result<TrainingResult> Process(CharacterId target, CommandId command)
    {
        var ability = variableStore.GetAbility(target, AbilityIndex.Obedient);
        var mood = trainingVariables.GetBase(target, BaseIndex.Mood);
        // Use injected dependencies directly
        return stateTracker.ProcessStateChanges(target);
    }
}

// C# 12 equivalent (verbose)
public class TrainingProcessor : ITrainingProcessor
{
    private readonly IVariableStore _variableStore;
    private readonly ITrainingVariables _trainingVariables;
    private readonly ICharacterStateTracker _stateTracker;

    public TrainingProcessor(
        IVariableStore variableStore,
        ITrainingVariables trainingVariables,
        ICharacterStateTracker stateTracker)
    {
        _variableStore = variableStore;
        _trainingVariables = trainingVariables;
        _stateTracker = stateTracker;
    }
}
```

### ERA-Specific Usage

**Pattern 1: DI with Result<T> Return Types**

```csharp
// Primary Constructor with DI - Command Handler pattern
public class MarkSystemProcessor(
    IVariableStore variables,
    ICharacterStateVariables stateVariables,
    ITrainingVariables trainingVariables) : IMarkSystem
{
    public Result<MarkChange> CalculateMarkGrowth(CharacterId target, MarkIndex markType)
    {
        // Use injected dependencies directly
        var resistTalent = variables.GetTalent(target, TalentIndex.Resistant);
        var tcvar = trainingVariables.GetTCVar(target, TCVarIndex.PleasureIntensity);

        return resistTalent.Bind(resist =>
            tcvar.Bind(intensity =>
                Result<MarkChange>.Success(new MarkChange(markType, intensity - resist))));
    }
}
```

**Pattern 2: Multiple Interface Dependencies**

```csharp
// Complex DI scenario - Multiple interfaces from same implementation
public class AbilityGrowthProcessor(
    IVariableStore coreVariables,
    ITrainingVariables trainingVariables,
    IJuelVariables juelVariables) : IAbilityGrowthProcessor
{
    // All interfaces are separate views of the same VariableStore instance
    public Result<List<AbilityChange>> CalculateGrowth(CharacterId target)
    {
        var abl = coreVariables.GetAbility(target, AbilityIndex.Obedient);
        var base_ = trainingVariables.GetBase(target, BaseIndex.Mood);
        var juel = juelVariables.GetJuel(target, 10);

        // ... growth calculation
        return Result<List<AbilityChange>>.Success(new List<AbilityChange>());
    }
}
```

### Benefits

- **Reduced boilerplate**: Eliminates field declarations and assignment code
- **Improved readability**: Dependencies are visible at class declaration
- **Immutability**: Parameters become readonly fields by default
- **DI compatibility**: Works seamlessly with ASP.NET Core and generic host DI

### Constraints

- Parameters become private readonly fields (cannot be made public)
- Cannot mix primary constructor with explicit field initializers for same parameters
- Base class constructor calls must use primary constructor syntax

---

## Extension Members

**Feature**: Extend existing types with new methods without modifying the original type or using static utility classes.

### Syntax

```csharp
// C# 14 Extension Members
public static class ResultExtensions
{
    // Extension method for Result<T>
    public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> mapper)
    {
        return result.Match(
            onSuccess: value => Result<U>.Success(mapper(value)),
            onFailure: error => Result<U>.Fail(error)
        );
    }

    // Extension method for IEnumerable<StateChange>
    public static TrainingResult ToTrainingResult(this IEnumerable<StateChange> changes, CharacterId target)
    {
        var result = new TrainingResult(target);
        foreach (var change in changes)
        {
            result.AddChange(change);
        }
        return result;
    }
}

// Usage
Result<int> value = GetValue();
Result<string> text = value.Map(v => v.ToString());

var changes = new List<StateChange> { new AbilityChange(AbilityIndex.Obedient, 10) };
TrainingResult result = changes.ToTrainingResult(CharacterId.Meiling);
```

### ERA-Specific Usage

**Pattern 1: Result<T> Composition**

```csharp
// src/Era.Core/Types/ResultExtensions.cs
public static class ResultExtensions
{
    // Bind for monadic composition
    public static Result<U> Bind<T, U>(this Result<T> result, Func<T, Result<U>> binder)
    {
        return result.Match(
            onSuccess: binder,
            onFailure: error => Result<U>.Fail(error)
        );
    }

    // Flatten nested Result<Result<T>>
    public static Result<T> Flatten<T>(this Result<Result<T>> nestedResult)
    {
        return nestedResult.Match(
            onSuccess: inner => inner,
            onFailure: error => Result<T>.Fail(error)
        );
    }
}

// Usage in training processor
public Result<TrainingResult> ProcessTraining(CharacterId target, CommandId command)
{
    return variableStore.GetAbility(target, AbilityIndex.Obedient)
        .Bind(ability => trainingVariables.GetBase(target, BaseIndex.Mood)
        .Bind(mood => CalculateResult(ability, mood)));
}
```

**Pattern 2: Strongly Typed ID Extensions**

```csharp
// src/Era.Core/Types/CharacterIdExtensions.cs
public static class CharacterIdExtensions
{
    // Add domain-specific behavior to CharacterId
    public static bool IsMaster(this CharacterId id)
    {
        return id == CharacterId.Master;
    }

    public static bool IsMainCharacter(this CharacterId id)
    {
        return id == CharacterId.Meiling || id == CharacterId.Sakuya;
    }
}

// Usage
if (target.IsMaster())
{
    // Apply stamina recovery for master
}
```

**Pattern 3: Collection Filtering Extensions**

```csharp
// src/Era.Core/Training/StateChangeExtensions.cs
public static class StateChangeExtensions
{
    // Filter StateChange list by type
    public static IEnumerable<AbilityChange> GetAbilityChanges(this IEnumerable<StateChange> changes)
    {
        return changes.OfType<AbilityChange>();
    }

    // Sum delta values for specific ability
    public static int SumAbilityDelta(this IEnumerable<StateChange> changes, AbilityIndex index)
    {
        return changes.OfType<AbilityChange>()
            .Where(c => c.Index == index)
            .Sum(c => c.Delta);
    }
}

// Usage
var result = trainingProcessor.Process(target, command);
var obedienceGain = result.Value.Changes.SumAbilityDelta(AbilityIndex.Obedient);
```

### Benefits

- **Fluent APIs**: Enable method chaining for Result<T> composition
- **Domain-specific operations**: Add game-specific methods to types
- **Discoverability**: Methods appear in IntelliSense with type instances
- **Testability**: Extension methods are easy to unit test

### Constraints

- Cannot access private members of extended type
- Must be defined in static class
- First parameter must use `this` modifier

---

## Collection Expressions

**Feature**: Concise syntax for creating and initializing collections using `[..]` syntax.

### Syntax

```csharp
// C# 14 Collection Expressions
int[] numbers = [1, 2, 3, 4, 5];
List<string> names = ["Alice", "Bob", "Charlie"];

// Spread operator
int[] first = [1, 2, 3];
int[] second = [4, 5, 6];
int[] combined = [..first, ..second];  // [1, 2, 3, 4, 5, 6]

// Empty collections
int[] empty = [];

// C# 12 equivalent (verbose)
int[] numbers = new int[] { 1, 2, 3, 4, 5 };
List<string> names = new List<string> { "Alice", "Bob", "Charlie" };
int[] combined = first.Concat(second).ToArray();
```

### ERA-Specific Usage

**Pattern 1: StateChange Collection Initialization**

```csharp
// Training result with multiple state changes
public TrainingResult ProcessVirginity(CharacterId target)
{
    var changes = new List<StateChange>
    {
        new TalentChange(TalentIndex.Virginity, -1),
        new ExpChange(ExpIndex.VExp, 100),
        new TCVarChange(TCVarIndex.Defloration, 1),
        new MarkHistoryChange(MarkIndex.ResistanceHistory, 1)
    };

    return new TrainingResult(target)
        .AddChange(changes[0])
        .AddChange(changes[1])
        .AddChange(changes[2])
        .AddChange(changes[3]);
}

// C# 14 Collection Expression (concise)
public TrainingResult ProcessVirginity(CharacterId target)
{
    List<StateChange> changes = [
        new TalentChange(TalentIndex.Virginity, -1),
        new ExpChange(ExpIndex.VExp, 100),
        new TCVarChange(TCVarIndex.Defloration, 1),
        new MarkHistoryChange(MarkIndex.ResistanceHistory, 1)
    ];

    return changes.ToTrainingResult(target);
}
```

**Pattern 2: Character ID Collections**

```csharp
// Define character groups
CharacterId[] mainCharacters = [
    CharacterId.Meiling,
    CharacterId.Sakuya,
    CharacterId.Patchouli,
    CharacterId.Remilia
];

// Combine multiple groups
CharacterId[] allCharacters = [
    ..mainCharacters,
    CharacterId.Flandre,
    CharacterId.Koakuma
];
```

**Pattern 3: Command Handler Registration**

```csharp
// Register command handlers in DI
public static IServiceCollection AddTrainingCommands(this IServiceCollection services)
{
    Type[] handlerTypes = [
        typeof(AbilityGrowthHandler),
        typeof(MarkSystemHandler),
        typeof(VirginityHandler),
        typeof(OrgasmHandler)
    ];

    foreach (var type in handlerTypes)
    {
        services.AddSingleton(type);
    }

    return services;
}
```

**Pattern 4: Test Data Setup**

```csharp
// Test scenario data
public class TrainingTestData
{
    public static IEnumerable<object[]> GetTestCases()
    {
        return [
            [CharacterId.Meiling, CommandId.Caress, 10],
            [CharacterId.Sakuya, CommandId.Kiss, 5],
            [CharacterId.Patchouli, CommandId.Service, 15]
        ];
    }
}
```

### Benefits

- **Concise syntax**: Less boilerplate for collection initialization
- **Type inference**: Compiler infers collection type from usage
- **Spread operator**: Easy collection concatenation
- **Uniform syntax**: Same syntax for arrays, lists, and other collections

### Constraints

- Requires .NET 10+ runtime
- Target type must be constructible and support Add() method (for List<T>) or be array-compatible

---

## field Keyword

**Feature**: Direct access to auto-property backing fields using the `field` keyword, enabling custom logic without explicit field declaration.

### Syntax

```csharp
// C# 14 field keyword
public class Character
{
    public string Name
    {
        get => field;
        set => field = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
    }

    public int Health
    {
        get => field;
        set => field = Math.Clamp(value, 0, MaxHealth);
    }

    public int MaxHealth { get; set; } = 100;
}

// C# 13 equivalent (verbose)
public class Character
{
    private string _name = null!;
    public string Name
    {
        get => _name;
        set => _name = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
    }

    private int _health;
    public int Health
    {
        get => _health;
        set => _health = Math.Clamp(value, 0, MaxHealth);
    }
}
```

### ERA-Specific Usage

**Pattern: Validated Properties**

```csharp
// Character state with validation
public class CharacterState
{
    // Mood clamped to valid range
    public int Mood
    {
        get => field;
        set => field = Math.Clamp(value, 0, 1000);
    }

    // Experience cannot be negative
    public int Experience
    {
        get => field;
        set => field = Math.Max(0, value);
    }

    // Name with normalization
    public string DisplayName
    {
        get => field;
        set => field = string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();
    }
}
```

### Benefits

- **Reduced boilerplate**: No explicit backing field declaration needed
- **Cleaner code**: Validation logic stays with property definition
- **Refactoring friendly**: Easy to add validation to existing auto-properties

### Constraints

- `field` is contextual keyword (only in property accessors)
- Cannot use `field` if explicit backing field exists with same name
- Nullable analysis applies to `field` based on property type

---

## Null-Conditional Assignment

**Feature**: Assign values conditionally when the target is not null using `?.=` syntax.

### Syntax

```csharp
// C# 14 null-conditional assignment
character?.Name = "Updated";     // Only assigns if character is not null
list?.Add(item);                 // Only calls Add if list is not null

// C# 13 equivalent (verbose)
if (character != null)
{
    character.Name = "Updated";
}
if (list != null)
{
    list.Add(item);
}
```

### ERA-Specific Usage

**Pattern 1: Optional State Updates**

```csharp
public class TrainingSession
{
    private CharacterState? _currentTarget;

    public void UpdateTargetMood(int delta)
    {
        // Only update if target exists
        _currentTarget?.Mood += delta;
    }

    public void SetTargetName(string name)
    {
        // Only assign if target exists
        _currentTarget?.DisplayName = name;
    }
}
```

**Pattern 2: Callback Invocation**

```csharp
public class EventDispatcher
{
    private Action<TrainingResult>? _onTrainingComplete;

    public void NotifyComplete(TrainingResult result)
    {
        // Safe invocation without explicit null check
        _onTrainingComplete?.Invoke(result);
    }
}
```

### Benefits

- **Concise null handling**: Single line instead of if-block
- **Reduced nesting**: Cleaner code in null-heavy scenarios
- **Consistent with `?.` operator**: Familiar pattern for C# developers

### Constraints

- Cannot use with `++` or `--` operators
- Left-hand side must be nullable reference or value type
- Does not short-circuit complex expressions

---

## Type Design Guidelines Integration

See [full-csharp-architecture.md](../../../docs/architecture/migration/full-csharp-architecture.md) for integration with existing patterns.

**Key Integration Points**:
1. **Primary Constructors with Result<T>**: Use primary constructors for DI in command handlers and processors
2. **Extension Members for Result<T>**: Add Bind/Map extensions for monadic composition
3. **Collection Expressions for StateChange**: Use `[..]` syntax for initializing StateChange lists

---

# Era.Core Project Patterns

> **Purpose**: Project-specific patterns and decisions derived from Era.Core development experience.
> **Scope**: These patterns are Era.Core conventions, not official C# 14 features.

---

## Primary Constructor with Null Validation

**Decision**: Phase 16 (2026-01-16) - All DI-injected dependencies in Era.Core use explicit null validation with field initializers.

**Rationale**:
- **Fail-Fast**: ArgumentNullException at construction time with clear parameter name
- **Defensive Programming**: Don't rely solely on DI container guarantees
- **Debugging**: Clear error messages vs NullReferenceException at random usage point
- **Testability**: Unit tests can manually instantiate without DI and still get proper validation

### Pattern

```csharp
// Era.Core Standard: Primary Constructor with Explicit Null Validation
public class TrainingProcessor(
    IVariableStore variableStore,
    ITrainingVariables trainingVariables,
    ICharacterStateTracker stateTracker) : ITrainingProcessor
{
    // Field declarations with inline null validation (Fail-Fast at construction)
    private readonly IVariableStore _variableStore =
        variableStore ?? throw new ArgumentNullException(nameof(variableStore));
    private readonly ITrainingVariables _trainingVariables =
        trainingVariables ?? throw new ArgumentNullException(nameof(trainingVariables));
    private readonly ICharacterStateTracker _stateTracker =
        stateTracker ?? throw new ArgumentNullException(nameof(stateTracker));

    // Methods use _fieldName (not camelCase parameter)
    public Result<TrainingResult> Process(CharacterId target, CommandId command)
    {
        var ability = _variableStore.GetAbility(target, AbilityIndex.Obedient);
        var mood = _trainingVariables.GetBase(target, BaseIndex.Mood);
        return _stateTracker.ProcessStateChanges(target);
    }
}
```

### Key Differences from Simple Primary Constructor

| Aspect | Simple Primary Constructor | Era.Core Pattern |
|--------|---------------------------|------------------|
| Field declarations | None (use parameter directly) | Explicit `private readonly` |
| Null validation | None (rely on DI/NRT) | `?? throw new ArgumentNullException` |
| Member access | `parameterName` | `_fieldName` |
| Failure point | First use (NullReferenceException) | Construction (ArgumentNullException) |

### When to Use

| Scenario | Pattern |
|----------|---------|
| DI-injected service | **Era.Core Pattern** (null validation) |
| Non-DI constructor parameter | Case-by-case evaluation |
| Struct/record with value types | Simple Primary Constructor |

### Migration Note

F509, F511, F512, F513 were migrated without null validation. F517 adds null validation to align with this pattern. F510+ use this pattern from initial implementation.

---

## Links

- [engine-dev SKILL](../engine-dev/SKILL.md) - Engine development reference
- [full-csharp-architecture.md](../../../docs/architecture/migration/full-csharp-architecture.md) - Architecture design
