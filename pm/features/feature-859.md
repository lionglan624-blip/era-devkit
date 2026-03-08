# Feature 859: NTR Extended Systems

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

<!-- Architecture Task: NTR Subsystem Migration -->

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Extended Systems.

### Problem (Current Issue)

NTR_TAKEOUT.ERB, NTR_VISITOR.ERB, NTR_COMF416.ERB, and NTR陥落イベント.ERB are extended support systems separated from the behavioral cluster (NTR_FRIENDSHIP/EXHIBITION/SEX) per CALL graph analysis — no friendship/exhibition coupling. These files reference NTR_UTIL core utilities.

### Goal (What to Achieve)

Migrate NTR_TAKEOUT.ERB (1,794 lines), NTR_VISITOR.ERB (541 lines), NTR_COMF416.ERB (272 lines), NTR陥落イベント.ERB (206 lines) to C#. Produce DateOutSystem.cs, CorruptionEvents.cs, NtrVisitorHandler.cs.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F856 | [DRAFT] | NTR Core/Util Foundation must complete first (extended systems reference core utilities) |

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Debt cleanup: no TODO/FIXME/HACK in migrated code | file | Grep(src/Era.Core/NTR/, pattern="TODO\|FIXME\|HACK") | not_matches | - | [ ] |
| 2 | Legacy ERB vs C# equivalence tests pass | test | dotnet test --filter "Category=Equivalence" | pass | - | [ ] |
| 3 | Zero-debt: no TODO/FIXME/HACK in test code | file | Grep(src/tools/dotnet/**/NTR*, pattern="TODO\|FIXME\|HACK") | not_matches | - | [ ] |

## Tasks

_To be completed by /fc_

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task | Transferred | Result |
|-------|--------|-------------|----------------|---------------|:-----------:|--------|

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|

## Links

[Predecessor: F856](feature-856.md) - NTR Core/Util Foundation
