# Feature 806: WC Counter Message SEX

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

C# migration of WC Counter SEX-category messages: dialogue and message generation for sexual actions during toilet counter events.

---

## Background

### Philosophy (Mid-term Vision)

Phase 21: Counter System — WC Counter Message SEX is the largest single message category (3,518 lines), requiring dedicated feature scope for manageable migration and testing.

### Scope Reference

| Source File | Approx Lines | Description |
|-------------|:------------:|-------------|
| TOILET_COUNTER_MESSAGE_SEX.ERB | 3518 | SEX-category messages for WC counter |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F783 | [DONE] | Phase 21 Planning (parent decomposition) |
| Predecessor | F805 | [DONE] | WC Counter Source + Message Core -- defines message dispatch interface that F806 SEX handlers implement |
| Successor | F813 | [DRAFT] | Post-Phase Review Phase 21 |
| Predecessor | F808 | [DONE] | WC Counter Message ITEM + NTR — TRYCALL EVENT_WC_COUNTER_MESSAGE_bottomCLOTH_OFF (TOILET_COUNTER_MESSAGE_ITEM.ERB:1182) |

---

## Deferred Obligations (from F805)

### F805 Action-ID-to-Variant Dispatch Routing Table

F805 `WcCounterMessage.Dispatch()` maps action IDs to message handlers. F806 (SEX) must implement handlers for the following action IDs (currently returning 0 as stubs):

**Direct dispatch**: 30, 31, 32, 33, 34, 35, 36, 40, 41, 42, 43, 44, 45, 46, 47, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 70, 71, 72, 73, 74, 75, 80, 81, 82, 83, 91, 500, 502, 503, 504, 505, 600, 601

Source: `Era.Core/Counter/WcCounterMessage.cs` Dispatch() method, lines 249-293.

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
