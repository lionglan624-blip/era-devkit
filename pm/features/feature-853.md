# Feature 853: NTR Application Services + Anti-Corruption Layer

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

Phase 24: NTR Bounded Context — Application + Infrastructure Layer. Pipeline Continuity — Application Services and the Anti-Corruption Layer are the consumer layers that bridge the NTR Domain Model with existing game systems. They invoke the Domain Service (F852) and translate between the new DDD model and legacy ERB-era interfaces.

### Problem (Current Issue)

The NTR system currently lacks a formal application layer — game commands directly manipulate ERB variables, and existing system interfaces (e.g., INtrQuery from OB-03/F829) have no typed implementation bridging old and new models. Phase 24 DDD design requires Application Commands/Queries for explicit use cases and an Anti-Corruption Layer to protect the new domain model from legacy system assumptions.

### Goal (What to Achieve)

Implement Application Services and Anti-Corruption Layer in `src/Era.Core/NTR/Application/` and `src/Era.Core/NTR/Infrastructure/`:
- `AdvanceNtrPhaseCommand.cs` — Command to advance NTR phase
- `ChangeNtrRouteCommand.cs` — Command to change NTR route
- `GetNtrStatusQuery.cs` — Query for current NTR status
- `GetSusceptibilityQuery.cs` — Query for susceptibility state
- EventHandlers for domain events
- `NtrProgressionRepository.cs` — Repository interface for Aggregate persistence
- Anti-Corruption Layer classes for bridging existing systems

**DDD Input Reference**: `pm/reference/ntr-ddd-input.md` — OB-03 INtrQuery routes to Phase 24 definition (F829 deferred obligation). The Anti-Corruption Layer must bridge INtrQuery consumers with the new NtrProgression Aggregate.

### Architecture Task Coverage

<!-- Architecture Task 8: NTR Application Services設計 -->
<!-- Architecture Task 9: Anti-Corruption Layer設計（既存システムとの橋渡し） -->

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F852 | [DONE] | INtrCalculator Domain Service (Application Services invoke Domain Service) |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Application Command classes exist | file | Glob(src/Era.Core/NTR/Application/Commands/*.cs) | gte | 2 | [ ] |
| 2 | Application Query classes exist | file | Glob(src/Era.Core/NTR/Application/Queries/*.cs) | gte | 2 | [ ] |
| 3 | Anti-Corruption Layer exists | file | Glob(src/Era.Core/NTR/Infrastructure/AntiCorruption/*.cs) | gte | 1 | [ ] |
| 4 | No TODO/FIXME/HACK debt in deliverables | code | Grep(src/Era.Core/NTR/, pattern="TODO\|FIXME\|HACK") | not_matches | TODO\|FIXME\|HACK | [ ] |

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
