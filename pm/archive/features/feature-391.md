# Feature 391: VariableReference VariableCode Addition

## Status: [DONE]

## Type: engine

## Created: 2026-01-07

---

## Summary

Add VariableCode field to VariableReference record. Amendment to F384 foundation types.

**Context**: F388 review identified that VariableReference(Scope, Index, CharacterId?) cannot distinguish between character-scoped variables (CFLAG vs ABL vs TALENT). Adding VariableCode enables callers to identify variable type without re-parsing identifiers.

---

## Background

### Philosophy (Mid-term Vision)

**Complete Variable Resolution**: Variable resolution should provide all necessary information for callers to access the correct variable storage without additional parsing or lookups.

### Problem (Current Issue)

```csharp
// Current: Era.Core/Types/VariableReference.cs
public record VariableReference(VariableScopeType Scope, int Index, int? CharacterId = null);

// Problem: Cannot distinguish CFLAG from ABL
var ref1 = resolver.Resolve("CFLAG:0:好感度"); // VariableScopeType.Character, Index=2
var ref2 = resolver.Resolve("ABL:0:体力");     // VariableScopeType.Character, Index=0
// Both have Scope=Character - caller cannot know which 2D array to access
```

### Goal (What to Achieve)

1. **Add VariableCode** to VariableReference record
2. **Enable type discrimination** for same-scope variables
3. **Zero breaking changes** - use optional parameter with default

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | VariableCode field added with null default | code | Grep(Era.Core/Types/VariableReference.cs) | contains | VariableCode? Code = null | [x] |
| 2 | Existing code compiles | build | - | succeeds | - | [x] |
| 3 | All tests pass | test | unit | succeeds | - | [x] |
| 4 | Namespace import added | code | Grep(Era.Core/Types/VariableReference.cs) | contains | using Era.Core.Variables | [x] |

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,4 | Add `VariableCode? Code = null` after CharacterId parameter (with using directive) | [x] |
| 2 | 2,3 | Verify build and all tests | [x] |

---

## Deliverables

| File | Purpose |
|------|---------|
| `Era.Core/Types/VariableReference.cs` | Updated record with VariableCode field |

**Note**: No code changes required for existing callers due to optional parameter with default.

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F384 | Original VariableReference definition |
| Predecessor | F385 | VariableCode enum (Era.Core.Variables namespace) |
| Successor | F388 | Will use VariableCode in resolution |

---

## Links

- [feature-384.md](feature-384.md) - F384 Foundation (original definition)
- [feature-385.md](feature-385.md) - F385 VariableCode enum
- [feature-388.md](feature-388.md) - F388 Variable Resolution (successor)

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-07 | create | opus | Created as F384 amendment per F388 FL review | PROPOSED |
| 2026-01-07 | FL | opus | Review-fix loop completed | REVIEWED |
| 2026-01-07 15:30 | START | implementer | Task 1 | - |
| 2026-01-07 15:30 | END | implementer | Task 1 | SUCCESS |
