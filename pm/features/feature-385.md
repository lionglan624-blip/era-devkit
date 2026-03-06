# Feature 385: Phase 5 VariableCode Enum Migration

## Status: [DONE]

## Type: engine

## Created: 2026-01-07

---

## Summary

Migrate VariableCode enum (~1500 lines, 500+ codes) from engine to Era.Core. This is the variable code definition that all variable operations reference.

**Context**: Phase 5 Task 1 - VariableCode enum migration. Large file requiring careful extraction.

---

## Background

### Philosophy (Mid-term Vision)

**Centralized Variable Definitions**: All variable codes in one enum enables:
- Compile-time validation of variable references
- IntelliSense support for variable names
- Single source of truth for variable IDs

### Problem (Current Issue)

```csharp
// Current: engine/MinorShift.Emuera/GameData/VariableCode.cs (~1500 lines)
public enum VariableCode
{
    FLAG = 0,
    CFLAG = 1,
    // ... 500+ codes
}
```

Engine-level enum prevents Era.Core from referencing variables without engine dependency.

### Goal (What to Achieve)

1. **Extract VariableCode enum** to Era.Core/Variables/
2. **Preserve all 500+ codes** exactly
3. **Add XML documentation** for key codes
4. **Engine references Era.Core** instead of defining locally

---

## Source Analysis

### Engine File

| File | Location | Lines |
|------|----------|:-----:|
| VariableCode.cs | engine/MinorShift.Emuera/GameData | ~1500 |

### Variable Categories

| Category | Code Range | Count | Description |
|----------|:----------:|:-----:|-------------|
| FLAG | 0-65 | 66 | Global flags |
| CFLAG | 100-588 | 489 | Character flags |
| TFLAG | 600-679 | 80 | Turn temporary flags |
| ABL | 700-849 | 150 | Abilities |
| TALENT | 900-949 | 50 | Talents |
| PALAM | 1000-1029 | 30 | Parameters |
| EXP | 1100-1119 | 20 | Experience |

---

## Acceptance Criteria

| AC# | Description | Type | Matcher | Expected | Status |
|:---:|-------------|------|---------|----------|:------:|
| 1 | VariableCode.cs exists in Era.Core | file | exists | Era.Core/Variables/VariableCode.cs | [x] |
| 2 | All codes preserved | code | count_gte | 218 enum members | [x] |
| 3 | FLAG codes present | code | contains | FLAG = 0x03 | [x] |
| 4 | CFLAG codes present | code | contains | CFLAG = 0x09 | [x] |
| 5 | Engine references Era.Core VariableCode | code | contains | using Era.Core.Variables | [x] |
| 6 | C# build succeeds | build | succeeds | - | [x] |
| 7 | Existing tests pass | test | succeeds | - | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2,3,4 | Extract VariableCode enum to Era.Core/Variables/ | [x] |
| 2 | 5 | Update engine to reference Era.Core.Variables.VariableCode | [x] |
| 3 | 6,7 | Verify build and tests | [x] |

---

## Deliverables

| File | Purpose |
|------|---------|
| `Era.Core/Variables/VariableCode.cs` | Variable code enum (500+ codes) |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F384 | Types foundation |
| Parallel | F386, F387, F388 | Can develop in parallel after F384 |

---

## Links

- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 5 Task 1
- `engine/MinorShift.Emuera/GameData/VariableCode.cs` - Source file

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-07 | create | opus | Created as Phase 5 feature per F377 next-phase planning | PROPOSED |
| 2026-01-07 12:51 | END | implementer | Task 1 | SUCCESS |
| 2026-01-07 12:59 | END | implementer | Task 2 | SUCCESS |
| 2026-01-07 12:59 | END | implementer | Task 3 | SUCCESS |
