# Feature 415: Callback Factory Result<T> Error Handling Investigation

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

## Type: infra

## Created: 2026-01-09

---

## Summary

Investigate the implications of callback factories returning default values (0 or false) on error instead of propagating Result<T>. Analyze impact on consumers and propose design recommendations for future callback factory implementations.

---

## Background

### Philosophy (Mid-term Vision)

**Phase 7: Technical Debt Consolidation**: Establish patterns and principles for Phase 8+ foundation.

Error handling patterns in callback factories affect system reliability and debuggability. This investigation ensures the chosen pattern is appropriate for the codebase.

### Problem (Current Issue)

F405 Callback DI Formalization introduces factories that return default values on error:

```csharp
// CUP factory - returns 0 on failure
return (character, cupIndex) => vars.GetCup(character, cupIndex) switch
{
    { IsSuccess: true } r => r.Value,
    _ => 0  // Silent failure
};

// TEQUIP factory - returns false on failure
return (character, index) => vars.GetTEquip(character, index) switch
{
    { IsSuccess: true } r => r.Value != 0,
    _ => false  // Silent failure
};
```

This pattern:
- Matches ERB semantics where invalid variable access returns 0
- May hide errors that should be surfaced
- Is inconsistent with Result<T> pattern used elsewhere in Era.Core

### Goal (What to Achieve)

1. Document the rationale for current error handling pattern
2. Analyze which consumers would benefit from Result<T> propagation
3. Determine if pattern change is warranted (scope for future Feature)
4. Establish guidelines for future callback factory implementations

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Analysis document created | file | Glob | exists | Game/agents/designs/callback-error-handling.md | [x] |
| 2 | Current pattern rationale documented | code | Grep callback-error-handling.md | contains | ERB semantics compatibility | [x] |
| 3 | Consumer impact analysis completed | code | Grep callback-error-handling.md | contains | Impact Analysis | [x] |
| 4 | Recommendation section exists | code | Grep callback-error-handling.md | contains | Recommendation | [x] |

### AC Details

**AC#1**: Create analysis document at `Game/agents/designs/callback-error-handling.md`

**AC#2-3**: Document sections:
- Current Pattern: Why factories return 0/false on error (ERB semantics compatibility)
- Impact Analysis: Which consumers would benefit from Result<T> propagation
- Trade-offs: Simplicity vs. explicit error handling

**AC#4**: Recommendation section with:
- Keep current pattern / Change to Result<T> / Hybrid approach
- Guidelines for future callback factory implementations
- Scope estimate if pattern change recommended

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Create callback-error-handling.md document | [x] |
| 2 | 2 | Document current pattern rationale (ERB semantics) | [x] |
| 3 | 3 | Analyze consumer impact (ExperienceGrowthCalculator uses DI factory; MarkSystem uses same pattern locally) | [x] |
| 4 | 4 | Write recommendation section with guidelines | [x] |

<!-- AC:Task 1:1 Rule: 1 AC = 1 Test = 1 Task -->

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F405 | Parent feature (created this as 残課題) |

**Note**: This investigation can proceed independently of F405 implementation. Results may inform future callback factory designs.

---

## Implementation Notes

### Analysis Scope

**Files to analyze**:
- `Era.Core/DependencyInjection/CallbackFactories.cs`
- `Era.Core/Character/ExperienceGrowthCalculator.cs` (CUP consumer)
- `Era.Core/Character/VirginityManager.cs` (TEQUIP consumer)
- `Era.Core/Training/MarkSystem.cs` (uses IVariableStore.GetCup directly via local helpers, not DI factory)

### Questions to Answer

1. Do any consumers need to distinguish between "value is 0" and "error occurred"?
2. Would Result<T> propagation simplify or complicate consumer code?
3. Is there precedent in the codebase for silent-failure patterns?

---

## Review Notes

<!-- /fl command will add review feedback here -->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-09 | create | opus | Follow-up from F405 残課題 | PROPOSED |
| 2026-01-09 09:30 | complete | implementer | Tasks 1-4 | SUCCESS |

---

## Links

- [feature-405.md](feature-405.md) - Parent feature (Callback DI Formalization)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 7 architecture
