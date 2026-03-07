# Feature 851: NtrProgression Aggregate + Domain Events

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

## Type: engine

## Background

### Philosophy (Mid-term Vision)

Phase 24: NTR Bounded Context — Domain Layer (Aggregate). Pipeline Continuity — the NtrProgression Aggregate is the central domain object that encapsulates NTR state transitions and raises domain events. It depends on Value Objects defined in F850 and is consumed by the Domain Service in F852.

### Problem (Current Issue)

The NTR progression system currently manages state transitions through imperative ERB scripts with no formal Aggregate boundary. FAV level changes, route transitions, and corruption state updates happen through direct variable mutation across multiple ERB files. Phase 24 DDD design requires a proper AggregateRoot that encapsulates state invariants and publishes domain events for state changes.

### Goal (What to Achieve)

Implement NtrProgression as AggregateRoot and domain event classes in `src/Era.Core/NTR/Domain/`:
- `NtrProgression.cs` — AggregateRoot encapsulating NTR state (FAV level, route, phase, corruption state)
- `NtrPhaseAdvanced.cs` — Domain event raised when NTR phase advances
- `NtrRouteChanged.cs` — Domain event raised when NTR route changes
- `NtrExposureIncreased.cs` — Domain event raised when exposure level increases
- `NtrCorrupted.cs` — Domain event raised when character enters corrupted state

**DDD Input Reference**: `pm/reference/ntr-ddd-input.md` — NtrProgression Aggregate section (Validated: universal pattern across all 9 characters + U_汎用, all sharing FAV/TALENT branching model).

### Architecture Task Coverage

<!-- Architecture Task 2: NtrProgression Aggregate設計・実装 -->
<!-- Architecture Task 6: NTR Domain Events定義（PhaseAdvanced, RouteChanged等） -->

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F850 | [DONE] | NTR UL + Value Objects (required before Aggregate) |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | NtrProgression AggregateRoot exists | file | Glob(src/Era.Core/NTR/Domain/NtrProgression.cs) | gte | 1 | [ ] |
| 2 | Domain event classes exist | file | Glob(src/Era.Core/NTR/Domain/Events/*.cs) | gte | 4 | [ ] |
| 3 | No TODO/FIXME/HACK debt in deliverables | code | Grep(src/Era.Core/NTR/Domain/, pattern="TODO\|FIXME\|HACK") | not_matches | TODO\|FIXME\|HACK | [ ] |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

---

## Review Notes
