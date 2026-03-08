# Feature 862: EVENT_MESSAGE_COM Migration

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

<!-- Architecture Task: Event System Migration -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity — Phase 25: AI & Visitor Systems — COM Event Message System. Each completed phase triggers planning for the next phase, producing sub-feature DRAFTs that are the SSOT for the next phase's scope.

### Problem (Current Issue)

EVENT_MESSAGE_COM.ERB is a self-contained 4,599-line file. CALL graph analysis shows NTR references are limited to kojo callbacks (TRYCCALLFORM NTR_KOJO_*) and display helpers (NTR_NAME) — no NTR core dependency. Requires EventDispatcher.cs interface from F861.

### Goal (What to Achieve)

Migrate EVENT_MESSAGE_COM.ERB (4,599 lines) to ComEventMessageHandler.cs. NTR references limited to kojo callbacks and display helpers — no NTR core dependency.

### Predecessor Obligations (for Mandatory Handoffs)

| Source Feature | Obligation | Category | Status |
|:--------------:|------------|----------|:------:|

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F861 | [DRAFT] | Visitor/Event Core must complete first (EventDispatcher interface required) |

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Debt cleanup: no TODO/FIXME/HACK in migrated code | file | Grep(src/Era.Core/Events/, pattern="TODO\|FIXME\|HACK") | not_matches | - | [ ] |
| 2 | Legacy ERB vs C# equivalence tests pass | test | dotnet test --filter "Category=Equivalence" | pass | - | [ ] |
| 3 | Zero-debt: no TODO/FIXME/HACK in test code | file | Grep(src/Era.Core.Tests/Events/, pattern="TODO\|FIXME\|HACK") | not_matches | - | [ ] |

## Tasks

_To be completed by /fc_

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

## Links

[Predecessor: F861](feature-861.md) - Visitor/Event Core
