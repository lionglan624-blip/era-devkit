# Feature 861: Visitor/Event Core

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

<!-- Architecture Task: Visitor System Migration -->
<!-- Architecture Task: Event System Migration -->
<!-- Architecture Task: Turn-end Processing -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity — Phase 25: AI & Visitor Systems — Visitor/Event Core. Each completed phase triggers planning for the next phase, producing sub-feature DRAFTs that are the SSOT for the next phase's scope.

### Problem (Current Issue)

INTRUDER.ERB, EVENT_KOJO.ERB, EVENT_MESSAGE.ERB, EVENT_MESSAGE_ORGASM.ERB, EVENTCOMEND.ERB, EVENTTURNEND.ERB, 情事中に踏み込み.ERB implement the visitor/event system. The architecture prescribes IVisitorSystem interface, VisitorId + NtrEventId strongly typed IDs, and SRP-split classes. F829 OB-02 (SANTA cosplay text output) requires engine-layer UI primitives now available.

### Goal (What to Achieve)

Migrate INTRUDER.ERB (323 lines), EVENT_KOJO.ERB (392 lines), EVENT_MESSAGE.ERB (258 lines), EVENT_MESSAGE_ORGASM.ERB (305 lines), EVENTCOMEND.ERB (658 lines), EVENTTURNEND.ERB (700 lines), 情事中に踏み込み.ERB (246 lines) to C#. Produce VisitorAI.cs, ScheduleManager.cs, EventDispatcher.cs, EventHandlers.cs. Define IVisitorSystem interface. Assign VisitorId + NtrEventId strongly typed IDs. Resolve OB-02 (SANTA cosplay — engine-layer UI primitives now available).

### Predecessor Obligations (for Mandatory Handoffs)

| Source Feature | Obligation | Category | Status |
|:--------------:|------------|----------|:------:|
| F829 | OB-02: SANTA cosplay text output | deferred | pending |

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F860 | [DRAFT] | NTR Message Generator must complete first (visitor/event consumes NTR message infrastructure) |
| Successor | F862 | [DRAFT] | EVENT_MESSAGE_COM depends on EventDispatcher interface |
| Successor | F863 | [DRAFT] | Location Extensions depends on IVisitorSystem |
| Successor | F864 | [DRAFT] | AFFAIR_DISCLOSURE depends on IEventContext |

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Debt cleanup: no TODO/FIXME/HACK in migrated code | file | Grep(src/Era.Core/Visitor/, pattern="TODO\|FIXME\|HACK") | not_matches | - | [ ] |
| 2 | Legacy ERB vs C# equivalence tests pass | test | dotnet test --filter "Category=Equivalence" | pass | - | [ ] |
| 3 | Zero-debt: no TODO/FIXME/HACK in test code | file | Grep(src/Era.Core.Tests/Visitor/, pattern="TODO\|FIXME\|HACK") | not_matches | - | [ ] |

## Tasks

_To be completed by /fc_

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

## Links

[Predecessor: F860](feature-860.md) - NTR Message Generator
[Related: F829](feature-829.md) - Source of OB-02
