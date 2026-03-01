# Feature 511: Primary Constructor Migration - Commands/Flow Directory

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

## Created: 2026-01-16

---

## Summary

Migrate 11 command handler files in `Era.Core/Commands/Flow/` directory from explicit constructor initialization to C# 14 Primary Constructor pattern.

**Scope**: Era.Core/Commands/Flow/ directory (11 handler files, 24 private readonly fields total)

**Target Files** (11 files, 18 handler classes, 24 fields total):
- CallHandler.cs: CallHandler, CallFormHandler (2+2 fields)
- IfHandler.cs: IfHandler, ElseIfHandler, ElseHandler, EndIfHandler (1+1+1+1 fields)
- ForHandler.cs: ForHandler, NextHandler (1+1 fields)
- GotoHandler.cs: GotoHandler (1 field)
- JumpHandler.cs: JumpHandler (2 fields)
- RendHandler.cs: RendHandler (2 fields)
- RepeatHandler.cs: RepeatHandler (2 fields)
- ReturnHandler.cs: ReturnHandler, ReturnFormHandler (1+1 fields)
- TryGotoHandler.cs: TryGotoHandler (1 field)
- TryJumpHandler.cs: TryJumpHandler (2 fields)
- WhileHandler.cs: WhileHandler, WendHandler (1+1 fields)

**Expected Line Reduction**: ~90-110 lines (field declarations + constructor assignments + null checks eliminated)

**Pattern Application**: Primary Constructor with DI dependencies (csharp-14 SKILL Pattern 1)

---

## Background

### Philosophy (Mid-term Vision)

Phase 16: C# 14 Style Migration - Apply C# 14 patterns to existing code for simplification. Primary Constructor and Collection Expression reduce ~400 lines of boilerplate across the codebase. This feature targets the Commands/Flow directory as a focused refactoring unit.

### Problem (Current Issue)

Commands/Flow handlers use C# 12 explicit constructor pattern with:
- Explicit `private readonly` field declarations
- Constructor parameter assignment boilerplate
- Repetitive null checks

Example (CallHandler.cs current state):
```csharp
public class CallHandler : ICommandHandler<CallCommand, Unit>
{
    private readonly IScopeManager _scopeManager;
    private readonly ILabelResolver _labelResolver;

    public CallHandler(IScopeManager scopeManager, ILabelResolver labelResolver)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _labelResolver = labelResolver ?? throw new ArgumentNullException(nameof(labelResolver));
    }
}
```

This pattern repeats across 11 files with 24 total field declarations.

### Goal (What to Achieve)

1. **Apply Primary Constructor pattern** to all 11 Flow handler files
2. **Eliminate field declaration boilerplate** (24 field declarations removed)
3. **Eliminate constructor assignment code** (~24 assignment lines removed)
4. **Remove null check boilerplate** (DI framework handles service instantiation)
5. **Verify migration completeness** (no remaining explicit constructors in scope)
6. **Ensure test continuity** (all existing tests PASS after migration)

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CallHandler migrated | code | Grep | contains | "public class CallHandler(" | [x] |
| 2 | CallFormHandler migrated | code | Grep | contains | "public class CallFormHandler(" | [x] |
| 3 | IfHandler migrated | code | Grep | contains | "public class IfHandler(" | [x] |
| 4 | ElseIfHandler migrated | code | Grep | contains | "public class ElseIfHandler(" | [x] |
| 5 | ElseHandler migrated | code | Grep | contains | "public class ElseHandler(" | [x] |
| 6 | EndIfHandler migrated | code | Grep | contains | "public class EndIfHandler(" | [x] |
| 7 | ForHandler migrated | code | Grep | contains | "public class ForHandler(" | [x] |
| 8 | NextHandler migrated | code | Grep | contains | "public class NextHandler(" | [x] |
| 9 | GotoHandler migrated | code | Grep | contains | "public class GotoHandler(" | [x] |
| 10 | JumpHandler migrated | code | Grep | contains | "public class JumpHandler(" | [x] |
| 11 | RendHandler migrated | code | Grep | contains | "public class RendHandler(" | [x] |
| 12 | RepeatHandler migrated | code | Grep | contains | "public class RepeatHandler(" | [x] |
| 13 | ReturnHandler migrated | code | Grep | contains | "public class ReturnHandler(" | [x] |
| 14 | ReturnFormHandler migrated | code | Grep | contains | "public class ReturnFormHandler(" | [x] |
| 15 | TryGotoHandler migrated | code | Grep | contains | "public class TryGotoHandler(" | [x] |
| 16 | TryJumpHandler migrated | code | Grep | contains | "public class TryJumpHandler(" | [x] |
| 17 | WhileHandler migrated | code | Grep | contains | "public class WhileHandler(" | [x] |
| 18 | WendHandler migrated | code | Grep | contains | "public class WendHandler(" | [x] |
| 19 | No explicit fields remain | code | Grep | not_contains | "private readonly" | [x] |
| 20 | Zero technical debt | code | Grep | not_contains | "TODO\|FIXME\|HACK" | [x] |
| 21 | All tests PASS | test | Bash | succeeds | "dotnet test" | [x] |

### AC Details

**AC#1-18 (18 handlers)**: Primary Constructor migration verification
- Test: Grep pattern="public class {ClassName}(" in Era.Core/Commands/Flow/{File}.cs
- Expected: Class declaration has opening parenthesis immediately after class name (Primary Constructor syntax)
- Pattern: `public class CallHandler(` indicates Primary Constructor (vs. `public class CallHandler :` for traditional)
- Combined with AC#19 (no private readonly fields), ensures complete migration

**AC#19**: No explicit field declarations remain
- Test: Grep pattern="private readonly" path="Era.Core/Commands/Flow"
- Expected: 0 matches (all fields converted to primary constructor parameters)

**AC#20**: Zero technical debt
- Test: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Commands/Flow"
- Expected: 0 matches across all migrated files

**AC#21**: All tests PASS
- Test: `dotnet test` from repository root
- Expected: All Era.Core.Tests and related tests pass
- Verification: Migration is refactoring-only, no functional changes

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1,2 | Migrate CallHandler.cs (CallHandler, CallFormHandler) | [x] |
| 2 | 3,4,5,6 | Migrate IfHandler.cs (IfHandler, ElseIfHandler, ElseHandler, EndIfHandler) | [x] |
| 3 | 7,8 | Migrate ForHandler.cs (ForHandler, NextHandler) | [x] |
| 4 | 9 | Migrate GotoHandler.cs (GotoHandler) | [x] |
| 5 | 10 | Migrate JumpHandler.cs (JumpHandler) | [x] |
| 6 | 11 | Migrate RendHandler.cs (RendHandler) | [x] |
| 7 | 12 | Migrate RepeatHandler.cs (RepeatHandler) | [x] |
| 8 | 13,14 | Migrate ReturnHandler.cs (ReturnHandler, ReturnFormHandler) | [x] |
| 9 | 15 | Migrate TryGotoHandler.cs (TryGotoHandler) | [x] |
| 10 | 16 | Migrate TryJumpHandler.cs (TryJumpHandler) | [x] |
| 11 | 17,18 | Migrate WhileHandler.cs (WhileHandler, WendHandler) | [x] |
| 12 | 19,20,21 | Verify migration completeness and test PASS | [x] |

<!-- AC verification completed 2026-01-16 -->

<!-- AC:Task 1:1 Rule: 21 ACs = 12 Tasks (file-based grouping + batch verification waiver) -->
<!-- File-based grouping: Multiple handlers per file are migrated atomically in one task (same edit operation). Tasks 1-11 group handlers by file, not by class. -->
<!-- Batch verification waiver (Task 12): Following F384 precedent - verification tasks checking no-field-remains + no-debt + test-PASS are related verification steps executed atomically. -->

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Migration Pattern

**Example: CallHandler (2 fields)**

**Before (C# 12)**:
```csharp
public class CallHandler : ICommandHandler<CallCommand, Unit>
{
    private readonly IScopeManager _scopeManager;
    private readonly ILabelResolver _labelResolver;

    public CallHandler(IScopeManager scopeManager, ILabelResolver labelResolver)
    {
        _scopeManager = scopeManager ?? throw new ArgumentNullException(nameof(scopeManager));
        _labelResolver = labelResolver ?? throw new ArgumentNullException(nameof(labelResolver));
    }

    public Task<Result<Unit>> Handle(CallCommand command, CancellationToken ct)
    {
        var labelResult = _labelResolver.ResolveLabel(command.FunctionName);
        // ... use _scopeManager, _labelResolver
    }
}
```

**After (C# 14 Primary Constructor)**:
```csharp
public class CallHandler(
    IScopeManager scopeManager,
    ILabelResolver labelResolver) : ICommandHandler<CallCommand, Unit>
{
    public Task<Result<Unit>> Handle(CallCommand command, CancellationToken ct)
    {
        // Null checks removed - DI framework guarantees non-null dependencies
        var labelResult = labelResolver.ResolveLabel(command.FunctionName);
        // ... use scopeManager, labelResolver directly
    }
}
```

### Implementation Steps

| Step | Action | Verification |
|:----:|--------|--------------|
| 1 | Identify all `private readonly` fields | Count = 24 (verified) |
| 2 | For each file, move constructor parameters to class declaration | Primary constructor syntax |
| 3 | Remove field declarations | Grep "private readonly" returns 0 |
| 4 | Remove constructor body | Parameters become implicit fields |
| 5 | Update field references (_field → field) | Use parameter name directly |
| 6 | Remove ArgumentNullException checks | DI framework validates |
| 7 | Verify compilation | `dotnet build` succeeds |
| 8 | Run tests | `dotnet test` passes |

### Null Check Handling

**Rationale**: DI framework (Microsoft.Extensions.DependencyInjection) guarantees registered services are instantiated (not null) when using standard registration patterns (`AddSingleton<TService, TImpl>`, `AddScoped`, `AddTransient`). All handlers in scope use standard DI registration. Manual factories returning null are programmer error, not a scenario to defend against in every constructor.

**Action**: Remove null checks during migration to eliminate boilerplate.

**Trade-off**: Removes defense-in-depth but simplifies code. Any null injection will manifest as NullReferenceException at call site rather than ArgumentNullException at constructor.

**Reference**: csharp-14 SKILL Primary Constructor Pattern 1 (DI with Result<T>).

---

## Source Migration Reference

**Location**: `Era.Core/Commands/Flow/*.cs` (11 files, 18 handler classes)

| File | Handler Classes | Fields/Class | Dependencies |
|------|-----------------|:------------:|--------------|
| CallHandler.cs | CallHandler, CallFormHandler | 2, 2 | IScopeManager, ILabelResolver |
| IfHandler.cs | IfHandler, ElseIfHandler, ElseHandler, EndIfHandler | 1, 1, 1, 1 | IExecutionStack |
| ForHandler.cs | ForHandler, NextHandler | 1, 1 | IExecutionStack |
| GotoHandler.cs | GotoHandler | 1 | ILabelResolver |
| JumpHandler.cs | JumpHandler | 2 | ILabelResolver, IScopeManager |
| RendHandler.cs | RendHandler | 2 | IExecutionStack, IGameState |
| RepeatHandler.cs | RepeatHandler | 2 | IExecutionStack, IGameState |
| ReturnHandler.cs | ReturnHandler, ReturnFormHandler | 1, 1 | IScopeManager |
| TryGotoHandler.cs | TryGotoHandler | 1 | ILabelResolver |
| TryJumpHandler.cs | TryJumpHandler | 2 | ILabelResolver, IScopeManager |
| WhileHandler.cs | WhileHandler, WendHandler | 1, 1 | IExecutionStack |

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Predecessor | F503 | Phase 16 Planning Feature must complete first |
| Interface | IScopeManager | Used by Call/Return/Jump/TryJump handlers for scope management |
| Interface | ILabelResolver | Used by Goto/Jump/TryGoto/TryJump handlers for label resolution |
| Interface | IExecutionStack | Used by If/For/While/Repeat/Rend handlers for control flow frames |
| Interface | IGameState | Used by Repeat/Rend handlers for COUNT:0 system variable |

---

## Links

- [feature-503.md](feature-503.md) - Phase 16 Planning (parent feature)
- [full-csharp-architecture.md](designs/full-csharp-architecture.md) - Phase 16 C# 14 Style Migration
- [csharp-14 SKILL](../../.claude/skills/csharp-14/SKILL.md) - Primary Constructor reference

---

## 引継ぎ先指定 (Mandatory Handoffs)

No deferred tasks identified at PROPOSED stage.

---

## Review Notes
<!-- Optional: Add review feedback here. /next command shows this section. -->
- **2026-01-16 FL iter2**: [resolved] Phase2-Validate - Null Check Handling: DI framework claim revised. Updated rationale to clarify standard registration guarantees instantiation, added Trade-off section documenting defense-in-depth removal.

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-16 | create | feature-builder | Created from F503 Phase 16 Planning | PROPOSED |
| 2026-01-16 15:09 | START | implementer | Task 1 | - |
| 2026-01-16 15:09 | END | implementer | Task 1 | SUCCESS |
| 2026-01-16 15:11 | START | implementer | Task 2 | - |
| 2026-01-16 15:11 | END | implementer | Task 2 | SUCCESS |
| 2026-01-16 15:13 | START | implementer | Task 3 | - |
| 2026-01-16 15:13 | END | implementer | Task 3 | SUCCESS |
| 2026-01-16 15:14 | START | implementer | Task 4 | - |
| 2026-01-16 15:14 | END | implementer | Task 4 | SUCCESS |
| 2026-01-16 15:16 | START | implementer | Task 5 | - |
| 2026-01-16 15:16 | END | implementer | Task 5 | SUCCESS |
| 2026-01-16 15:17 | START | implementer | Task 9 | - |
| 2026-01-16 15:17 | END | implementer | Task 9 | SUCCESS |
| 2026-01-16 15:18 | START | implementer | Task 11 | - |
| 2026-01-16 15:18 | END | implementer | Task 11 | SUCCESS |
| 2026-01-16 15:17 | START | implementer | Task 10 | - |
| 2026-01-16 15:18 | END | implementer | Task 10 | SUCCESS |
| 2026-01-16 15:18 | START | implementer | Task 8 | - |
| 2026-01-16 15:18 | END | implementer | Task 8 | SUCCESS |
| 2026-01-16 15:18 | START | implementer | Task 6 | - |
| 2026-01-16 15:18 | END | implementer | Task 6 | SUCCESS |
| 2026-01-16 15:18 | START | implementer | Task 7 | - |
| 2026-01-16 15:18 | END | implementer | Task 7 | SUCCESS |
| 2026-01-16 15:20 | START | implementer | Task 12 | - |
| 2026-01-16 15:20 | END | implementer | Task 12 | SUCCESS |
