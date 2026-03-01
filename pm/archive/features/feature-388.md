# Feature 388: Phase 5 Variable Resolution & CSV Loading

## Status: [DONE]

## Type: engine

## Created: 2026-01-07

---

## Summary

Implement IVariableResolver and IVariableDefinitionLoader. Migrate IdentifierDictionary and CSV loading logic from engine to Era.Core.

**Context**: Phase 5 Tasks 4 & 5 - Variable name resolution and CSV definition loading. Combined as they share common infrastructure.

---

## Background

### Philosophy (Mid-term Vision)

**Dynamic Variable Resolution**: Enable runtime variable name lookup:
- ERB code references variables by name ("FLAG:123", "CFLAG:TARGET:好感度")
- Resolver maps names to typed references
- CSV files define custom variable names

### Problem (Current Issue)

```csharp
// Current: engine/Assets/Scripts/Emuera/GameData/IdentifierDictionary.cs
// String-based lookup without type safety
public bool TryGetValue(string name, out VariableToken token) { ... }
```

Variable resolution is string-based and tightly coupled to parser.

### Goal (What to Achieve)

1. **Implement IVariableResolver** for name-to-reference mapping
2. **Implement IVariableDefinitionLoader** for CSV parsing
3. **VariableReference** DTO for resolved variables
4. **VariableDefinitions** DTO for loaded definitions
5. **Result<T>** for resolution failures

---

## Source Analysis

### Engine Files

| File | Location | Lines | Purpose |
|------|----------|:-----:|---------|
| IdentifierDictionary.cs | engine/Assets/Scripts/Emuera/GameData | ~690 | Name resolution |

Note: VariableEvaluator.cs handles runtime evaluation (not extracted in this feature).

**Scope Clarification**: This feature implements CSV-defined variable name resolution only (name→index mapping from CFLAG.csv, TFLAG.csv, etc.). It does NOT replace full IdentifierDictionary functionality which includes function lookups, macro resolution, and reserved word checking.

### Resolution Patterns

Note: "Resolution" column shows conceptual logic, not actual VariableReference output. See AC#5, AC#6 for expected VariableReference format.

| Pattern | Example | Resolution |
|---------|---------|------------|
| Simple | `FLAG:123` | FlagIndex(123) |
| Named | `TFLAG:実行値` | FlagIndex from CSV lookup |
| Character | `CFLAG:TARGET:0` | CharacterFlagIndex(0) + TARGET context |
| Character Named | `CFLAG:0:好感度` | CharacterId(0) + CharacterFlagIndex from CSV |

Note: Character name resolution (`CFLAG:美鈴:好感度`) requires ICharacterRegistry which is out of scope for this feature. Use numeric CharacterId only.

**Resolution Types**: (1) Numeric index (`FLAG:123`) bypasses CSV lookup - returns index directly. (2) Named index (`TFLAG:実行値`, `CFLAG:0:好感度`) requires CSV lookup to resolve name→index.

### CSV Format

```csv
; Game/CSV/CFLAG.csv
1,既成事実
2,好感度
3,異常経験
...
```

Note: LoadFromCsv loads a single CSV file per call. The caller must invoke it for each variable type (CFLAG.csv, TFLAG.csv, etc.).

**CSV Format Handling**: Parser must support: (1) comment lines starting with `;` or `;;`, (2) inline comments after value (e.g., `39,娼婦紋押下回数,;comment`), (3) blank lines, (4) UTF-8 BOM (`\uFEFF`) on first line. Implementation note: Use `StreamReader` with `detectEncodingFromByteOrderMarks: true` or explicitly strip `\uFEFF` from first line.

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | VariableResolver.cs exists | file | - | exists | Era.Core/Variables/VariableResolver.cs | [x] |
| 2 | VariableDefinitionLoader.cs exists | file | - | exists | Era.Core/Variables/VariableDefinitionLoader.cs | [x] |
| 3 | Implements IVariableResolver | code | - | contains | : IVariableResolver | [x] |
| 4 | Implements IVariableDefinitionLoader | code | - | contains | : IVariableDefinitionLoader | [x] |
| 5 | Simple pattern resolves | test | unit | succeeds | ResolveSimplePattern_Success | [x] |
| 6 | Named pattern resolves | test | unit | succeeds | CFLAG:0:好感度 → VariableReference(Character, 2, 0, CFLAG) | [x] |
| 7 | CSV loading works | test | unit | succeeds | Loads "好感度"→2 mapping from CFLAG.csv | [x] |
| 8 | Invalid name returns Failure | test | unit | succeeds | Result.Failure | [x] |
| 9 | IVariableResolver DI registered | code | - | contains | services.AddSingleton<IVariableResolver | [x] |
| 10 | IVariableDefinitionLoader DI registered | code | - | contains | services.AddSingleton<IVariableDefinitionLoader | [x] |
| 11 | C# build succeeds | build | - | succeeds | - | [x] |
| 12 | All tests pass | test | unit | succeeds | - | [x] |

### AC Details

**AC#6 Test Setup**: Test uses mock CsvVariableDefinitions with `{"好感度": 2}` mapping, or loads actual `Game/CSV/CFLAG.csv` which has line `2,好感度`.

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,3 | Implement VariableResolver | [x] |
| 2 | 2,4 | Implement VariableDefinitionLoader | [x] |
| 3 | 5,6,7,8 | Create unit tests for resolution and loading | [x] |
| 4 | 9,10 | Register interfaces in DI (Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | [x] |
| 5 | 11,12 | Verify build and all tests | [x] |

**Batch task waiver (Task 3)**: Creates unit tests for resolution patterns - atomic success/failure. Following F386/F387 precedent for batch test creation.

---

## Deliverables

| File | Purpose |
|------|---------|
| `Era.Core/Variables/VariableResolver.cs` | IVariableResolver implementation |
| `Era.Core/Variables/VariableDefinitionLoader.cs` | IVariableDefinitionLoader implementation |
| `Era.Core.Tests/VariableResolverTests.cs` | Unit tests for resolution patterns |
| `Era.Core.Tests/VariableDefinitionLoaderTests.cs` | Unit tests for CSV loading |

Note: `VariableReference.cs` and `CsvVariableDefinitions.cs` are defined in F384 (foundation types).

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F384 | IVariableResolver, IVariableDefinitionLoader interfaces |
| Predecessor | F385 | VariableCode enum (used internally for variable type identification during parsing) |
| Predecessor | F391 | VariableReference with VariableCode field |
| Parallel | F386, F387 | Independent subsystem |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 5 Tasks 4 & 5
- [feature-377.md](feature-377.md) - F377 Phase 5 planning origin
- [feature-384.md](feature-384.md) - F384 Foundation interfaces
- [feature-385.md](feature-385.md) - F385 VariableCode enum
- [feature-391.md](feature-391.md) - F391 VariableReference amendment
- [feature-386.md](feature-386.md) - F386 VariableStore (parallel)
- [feature-387.md](feature-387.md) - F387 VariableScope (parallel)
- `engine/Assets/Scripts/Emuera/GameData/IdentifierDictionary.cs` - Source file
- `Game/CSV/CFLAG.csv` - Example CSV file

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-07 | create | opus | Created as Phase 5 feature per F377 next-phase planning | PROPOSED |
| 2026-01-07 16:15 | START | initializer | Initialize | - |
| 2026-01-07 16:15 | END | initializer | Initialize | WIP |
| 2026-01-07 16:17 | START | implementer | Task 3 (TDD Tests) | - |
| 2026-01-07 16:17 | END | implementer | Task 3 (TDD Tests) | SUCCESS |
| 2026-01-07 16:20 | START | implementer | Task 1 (VariableResolver) | - |
| 2026-01-07 16:20 | END | implementer | Task 1 (VariableResolver) | SUCCESS |
| 2026-01-07 16:23 | START | implementer | Task 2 (VariableDefinitionLoader) | - |
| 2026-01-07 16:23 | END | implementer | Task 2 (VariableDefinitionLoader) | SUCCESS |
| 2026-01-07 16:27 | START | implementer | Task 4 (DI Registration) | - |
| 2026-01-07 16:27 | END | implementer | Task 4 (DI Registration) | SUCCESS |
| 2026-01-07 16:30 | START | ac-tester | Task 5 (Verification) | - |
| 2026-01-07 16:30 | END | ac-tester | Task 5 (AC 1-12 verified) | SUCCESS |
| 2026-01-07 16:35 | START | feature-reviewer | Post Review | - |
| 2026-01-07 16:35 | END | feature-reviewer | Post Review | NEEDS_REVISION |
| 2026-01-07 16:36 | END | opus | Documentation fix (Deliverables) | SUCCESS |
| 2026-01-07 16:38 | START | feature-reviewer | Doc Check | - |
| 2026-01-07 16:38 | END | feature-reviewer | Doc Check | NEEDS_REVISION |
| 2026-01-07 16:40 | END | opus | Doc fix (engine-dev SKILL, architecture) | SUCCESS |
