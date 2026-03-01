# Feature 384: Phase 5 Foundation - Types & Interfaces

## Status: [DONE]

## Type: engine

## Created: 2026-01-07

---

## Summary

Create the foundational types and interfaces for Phase 5 Variable System. This feature establishes Strongly Typed IDs and interface contracts that F385-F388 depend on.

**Context**: This is the prerequisite for all other Phase 5 features. No implementation logic - only type definitions and interface contracts.

---

## Background

### Philosophy (Mid-term Vision)

**Foundation First**: Define contracts before implementation. This enables:
- Parallel development of F385-F387 (F388 requires F385 VariableCode enum)
- Clear API boundaries
- Type-safe compilation from day one

### Problem (Current Issue)

Phase 5 requires multiple subsystems that share common types:
- Variable storage needs typed array indices (FlagIndex, CharacterFlagIndex, etc.)
- Scope management needs LocalVariableIndex
- All subsystems need interface definitions

### Goal (What to Achieve)

1. **7 Strongly Typed IDs** for compile-time safety:
   | Typed ID | ERA Variable | Array Dimension |
   |----------|--------------|-----------------|
   | FlagIndex | FLAG, TFLAG | 1D |
   | CharacterFlagIndex | CFLAG | 2D (character × index) |
   | AbilityIndex | ABL | 2D |
   | TalentIndex | TALENT | 2D |
   | PalamIndex | PALAM | 2D |
   | ExpIndex | EXP | 2D |
   | LocalVariableIndex | LOCAL | 1D (scope-local) |
2. **4 Interfaces** defining contracts for subsystems
3. **Supporting types** (VariableScopeType enum, VariableReference/CsvVariableDefinitions DTOs)

Note: DI registration occurs in F386-F388 when interfaces are implemented (interfaces alone are not registered).

---

## Design Principles

### Phase 4 Pattern Requirements (Mandatory)

| Principle | Application |
|-----------|-------------|
| **Strongly Typed IDs** | `readonly record struct` pattern from F377 |
| **Interface contracts** | Define before implementation |
| **Result<T>** | Used in interface signatures |

### Type Definitions

```csharp
// Strongly Typed IDs (Era.Core/Types/)
// Following F377 CharacterId pattern: readonly record struct with constructor and operator conversions
// Array index types do not define well-known constants as indices are dynamically determined by CSV configuration.

// Array index types - prevent mixing indices from different arrays
// Each type includes:
// - Value property
// - Constructor
// - implicit operator int (backward compatibility)
// - explicit operator (from int, requires explicit cast)

public readonly record struct FlagIndex
{
    public int Value { get; }
    public FlagIndex(int value) { Value = value; }
    public static implicit operator int(FlagIndex id) => id.Value;
    public static explicit operator FlagIndex(int value) => new(value);
}

public readonly record struct CharacterFlagIndex
{
    public int Value { get; }
    public CharacterFlagIndex(int value) { Value = value; }
    public static implicit operator int(CharacterFlagIndex id) => id.Value;
    public static explicit operator CharacterFlagIndex(int value) => new(value);
}

public readonly record struct AbilityIndex
{
    public int Value { get; }
    public AbilityIndex(int value) { Value = value; }
    public static implicit operator int(AbilityIndex id) => id.Value;
    public static explicit operator AbilityIndex(int value) => new(value);
}

public readonly record struct TalentIndex
{
    public int Value { get; }
    public TalentIndex(int value) { Value = value; }
    public static implicit operator int(TalentIndex id) => id.Value;
    public static explicit operator TalentIndex(int value) => new(value);
}

public readonly record struct PalamIndex
{
    public int Value { get; }
    public PalamIndex(int value) { Value = value; }
    public static implicit operator int(PalamIndex id) => id.Value;
    public static explicit operator PalamIndex(int value) => new(value);
}

public readonly record struct ExpIndex
{
    public int Value { get; }
    public ExpIndex(int value) { Value = value; }
    public static implicit operator int(ExpIndex id) => id.Value;
    public static explicit operator ExpIndex(int value) => new(value);
}

public readonly record struct LocalVariableIndex
{
    public int Value { get; }
    public LocalVariableIndex(int value) { Value = value; }
    public static implicit operator int(LocalVariableIndex id) => id.Value;
    public static explicit operator LocalVariableIndex(int value) => new(value);
}

// Supporting enum - used by IVariableScope (F387) for scope stack management
// Note: TFLAG is 1D global storage that resets per turn, shares FlagIndex type with FLAG
public enum VariableScopeType { Global, Local, Character }

// DTOs for IVariableResolver and IVariableDefinitionLoader
// VariableReference: Resolved variable reference from identifier lookup
// Note: Uses raw int Index intentionally. At resolution time, the Scope field determines
// which typed index applies. Consumers can cast based on Scope:
//   Global → FlagIndex, Character → CharacterFlagIndex/AbilityIndex/etc., Local → LocalVariableIndex
public record VariableReference(
    VariableScopeType Scope,
    int Index,
    int? CharacterId = null
);

// CsvVariableDefinitions: Loaded CSV definitions (renamed to avoid conflict with Era.Core.Common.VariableDefinitions)
// Contains mappings from variable names to indices loaded from CSV files
public record CsvVariableDefinitions(
    IReadOnlyDictionary<string, int> NameToIndex,
    IReadOnlyList<string> IndexToName
);
```

**Note**: VariableId was removed as it duplicates VariableCode enum functionality (F385). VariableCode enum is the primary way to reference variable types.

### Interface Contracts

```csharp
// IVariableStore - Storage abstraction (F386 implements)
// Supports 1D arrays (FLAG, TFLAG) and 2D arrays (CFLAG, ABL, TALENT, PALAM, EXP)
// Error handling design:
// - Getters: 1D arrays return int (static bounds), 2D arrays return Result<int> (character validation)
// - Setters: Return void (fire-and-forget). Invalid character silently ignored.
//   Rationale: Setters typically called in tight loops; returning Result adds overhead.
//   F386 implementation may log warnings for invalid characters.
public interface IVariableStore
{
    // 1D arrays - static bounds, cannot fail index lookup
    // FLAG: persistent global flags
    int GetFlag(FlagIndex index);
    void SetFlag(FlagIndex index, int value);

    // TFLAG: turn-temporary flags (same index type, separate storage, resets per turn)
    int GetTFlag(FlagIndex index);
    void SetTFlag(FlagIndex index, int value);

    // 2D character arrays - require character validation
    // Setters return void (fire-and-forget by design - see interface comment)
    Result<int> GetCharacterFlag(CharacterId character, CharacterFlagIndex flag);
    void SetCharacterFlag(CharacterId character, CharacterFlagIndex flag, int value);
    Result<int> GetAbility(CharacterId character, AbilityIndex ability);
    void SetAbility(CharacterId character, AbilityIndex ability, int value);
    Result<int> GetTalent(CharacterId character, TalentIndex talent);
    void SetTalent(CharacterId character, TalentIndex talent, int value);
    Result<int> GetPalam(CharacterId character, PalamIndex index);
    void SetPalam(CharacterId character, PalamIndex index, int value);
    Result<int> GetExp(CharacterId character, ExpIndex index);
    void SetExp(CharacterId character, ExpIndex index, int value);
}

// IVariableScope - Scope management (F387 implements)
// Note: ARG variable handling (function arguments) uses same LocalVariableIndex
// but maintains separate storage. ARG vs LOCAL distinction is F387 implementation detail.
public interface IVariableScope
{
    VariableScopeType CurrentScope { get; }
    void PushLocal();
    void PopLocal();
    Result<int> GetLocal(LocalVariableIndex index);
    void SetLocal(LocalVariableIndex index, int value);
}

// IVariableResolver - Name resolution (F388 implements)
public interface IVariableResolver
{
    Result<VariableReference> Resolve(string identifier);
    bool TryResolve(string identifier, out VariableReference reference);
}

// IVariableDefinitionLoader - CSV loading (F388 implements)
public interface IVariableDefinitionLoader
{
    Result<CsvVariableDefinitions> LoadFromCsv(string csvPath);
}
```

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | FlagIndex exists | file | Glob | exists | Era.Core/Types/FlagIndex.cs | [x] |
| 2 | CharacterFlagIndex exists | file | Glob | exists | Era.Core/Types/CharacterFlagIndex.cs | [x] |
| 3 | AbilityIndex exists | file | Glob | exists | Era.Core/Types/AbilityIndex.cs | [x] |
| 4 | TalentIndex exists | file | Glob | exists | Era.Core/Types/TalentIndex.cs | [x] |
| 5 | PalamIndex exists | file | Glob | exists | Era.Core/Types/PalamIndex.cs | [x] |
| 6 | ExpIndex exists | file | Glob | exists | Era.Core/Types/ExpIndex.cs | [x] |
| 7 | LocalVariableIndex exists | file | Glob | exists | Era.Core/Types/LocalVariableIndex.cs | [x] |
| 8 | IVariableStore exists | file | Glob | exists | Era.Core/Interfaces/IVariableStore.cs | [x] |
| 9 | IVariableScope exists | file | Glob | exists | Era.Core/Interfaces/IVariableScope.cs | [x] |
| 10 | IVariableResolver exists | file | Glob | exists | Era.Core/Interfaces/IVariableResolver.cs | [x] |
| 11 | IVariableDefinitionLoader exists | file | Glob | exists | Era.Core/Interfaces/IVariableDefinitionLoader.cs | [x] |
| 12 | VariableScopeType exists | file | Glob | exists | Era.Core/Types/VariableScopeType.cs | [x] |
| 13 | VariableReference exists | file | Glob | exists | Era.Core/Types/VariableReference.cs | [x] |
| 14 | CsvVariableDefinitions exists | file | Glob | exists | Era.Core/Types/CsvVariableDefinitions.cs | [x] |
| 15 | C# build succeeds | build | dotnet build | succeeds | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create FlagIndex typed ID (1D array access) | [x] |
| 2 | 2-6 | Create character-scoped index types (batch: identical pattern) | [x] |
| 3 | 7 | Create LocalVariableIndex typed ID | [x] |
| 4 | 8 | Create IVariableStore interface | [x] |
| 5 | 9 | Create IVariableScope interface | [x] |
| 6 | 10-11 | Create IVariableResolver and IVariableDefinitionLoader interfaces | [x] |
| 7 | 12-14 | Create supporting types (VariableScopeType, VariableReference, CsvVariableDefinitions) | [x] |
| 8 | 15 | Verify build succeeds | [x] |

**Batch task waiver (Task 2, 6, 7)**: Following F377 precedent for batch creation of similar types. These tasks create multiple files following identical patterns. AC:Task 1:1 rule waived. If any file fails, the entire task fails (atomic success/failure).

---

## Deliverables

### Strongly Typed IDs (Era.Core/Types/)

| File | Purpose |
|------|---------|
| `FlagIndex.cs` | FLAG array index (1D) |
| `CharacterFlagIndex.cs` | CFLAG array index (2D) |
| `AbilityIndex.cs` | ABL array index (2D) |
| `TalentIndex.cs` | TALENT array index (2D) |
| `PalamIndex.cs` | PALAM array index (2D) |
| `ExpIndex.cs` | EXP array index (2D) |
| `LocalVariableIndex.cs` | LOCAL variable index |

### Interfaces (Era.Core/Interfaces/)

Note: Interfaces placed in Era.Core/Interfaces/ per F377 pattern (20+ interfaces already there), rather than Era.Core/Variables/ as simplified in architecture doc. This maintains consistent interface organization.

| File | Purpose |
|------|---------|
| `IVariableStore.cs` | Storage contract |
| `IVariableScope.cs` | Scope management contract |
| `IVariableResolver.cs` | Name resolution contract |
| `IVariableDefinitionLoader.cs` | CSV loading contract |

### Supporting Types (Era.Core/Types/)

| File | Purpose |
|------|---------|
| `VariableScopeType.cs` | Scope type enum |
| `VariableReference.cs` | Resolved variable reference DTO |
| `CsvVariableDefinitions.cs` | Loaded CSV definitions container |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F377 | Phase 4 patterns (Strongly Typed IDs, Result) |
| Successor | F385 | VariableCode enum (uses types) |
| Successor | F386 | VariableStore (implements IVariableStore) |
| Successor | F387 | VariableScope (implements IVariableScope) |
| Successor | F388 | Resolver + Loader (implements interfaces) |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 5 definition
- [feature-377.md](feature-377.md) - Phase 4 patterns

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-07 | create | opus | Created as Phase 5 foundation feature per F377 next-phase planning | PROPOSED |
| 2026-01-07 10:39 | START | implementer | Task 1 | - |
| 2026-01-07 10:39 | END | implementer | Task 1 | SUCCESS |
| 2026-01-07 10:41 | START | implementer | Task 2 | - |
| 2026-01-07 10:41 | END | implementer | Task 2 | SUCCESS |
| 2026-01-07 10:42 | START | implementer | Task 3 | - |
| 2026-01-07 10:42 | END | implementer | Task 3 | SUCCESS |
| 2026-01-07 10:44 | START | implementer | Task 4 | - |
| 2026-01-07 10:44 | END | implementer | Task 4 | SUCCESS |
| 2026-01-07 10:45 | START | implementer | Task 5 | - |
| 2026-01-07 10:45 | END | implementer | Task 5 | SUCCESS |
| 2026-01-07 10:50 | START | implementer | Task 6 | - |
| 2026-01-07 10:50 | END | implementer | Task 6 | SUCCESS |
| 2026-01-07 10:50 | START | implementer | Task 7 | - |
| 2026-01-07 10:50 | END | implementer | Task 7 | SUCCESS |
| 2026-01-07 10:52 | START | implementer | Task 8 | - |
| 2026-01-07 10:52 | END | implementer | Task 8 | SUCCESS |
