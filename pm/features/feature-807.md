# Feature 807: WC Counter Message TEASE

## Status: [DRAFT]

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

## Type: erb

## Summary

C# migration of WC Counter TEASE-category messages: dialogue and message generation for teasing actions during toilet counter events.

---

## Background

### Philosophy (Mid-term Vision)

Phase 21: Counter System — WC Counter Message TEASE is a high-volume message category (3,483 lines), requiring dedicated feature scope to maintain migration quality.

### Scope Reference

| Source File | Approx Lines | Description |
|-------------|:------------:|-------------|
| TOILET_COUNTER_MESSAGE_TEASE.ERB | 3483 | TEASE-category messages for WC counter |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| Predecessor | F805 | [DONE] | WC Counter Source + Message Core -- defines message dispatch interface that F807 TEASE handlers implement |
| Successor | F813 | [DRAFT] | Post-Phase Review Phase 21 |
| Predecessor | F808 | [DONE] | WC Counter Message ITEM + NTR — TRYCALL EVENT_WC_COUNTER_MESSAGE_NTRrevelation, _setITEMs, _bottomITEM_SHOW |

---

## Deferred Obligations (from F805)

### F805 Action-ID-to-Variant Dispatch Routing Table

F805 `WcCounterMessage.Dispatch()` and `DispatchWithBranch()` map action IDs to message handlers. F807 (TEASE) must implement handlers for the following action IDs (currently returning 0 as stubs):

**Direct dispatch**: 21, 22, 23, 24, 25, 26, 27
**Branch dispatch**: (29, 1), (29, 2), (29, 3)

Source: `Era.Core/Counter/WcCounterMessage.cs` Dispatch() lines 241-248, DispatchWithBranch() lines 306-309.

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | - | Remove TODO/FIXME/HACK comments in migrated C# files | | [ ] |
| 2 | - | Write equivalence tests comparing C# output to legacy ERB behavior | | [ ] |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | No TODO/FIXME/HACK comments remain in migrated files (zero technical debt) | code | Grep | not_matches | TODO\|FIXME\|HACK | [ ] |

---

## Links

- [Predecessor: F783](feature-783.md) - Phase 21 Planning
