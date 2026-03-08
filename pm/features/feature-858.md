# Feature 858: NTR Master Scenes

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

Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Master Scenes.

### Problem (Current Issue)

NTR_MASTER_SEX.ERB and NTR_MASTER_3P_SEX.ERB are self-contained master scene processors. ROOM_SMELL_WHOSE appears 5 times in NTR_MASTER_SEX.ERB, requiring I3DArrayVariables GetDa/SetDa implementation (F829 OB-08).

### Goal (What to Achieve)

Migrate NTR_MASTER_SEX.ERB (1,507 lines), NTR_MASTER_3P_SEX.ERB (2,200 lines) to C#. Produce ThreesomeHandler.cs + master sex scene processor. Implement I3DArrayVariables GetDa/SetDa for ROOM_SMELL_WHOSE (OB-08).

### Predecessor Obligations (for Mandatory Handoffs)

| Source Feature | Obligation | Category | Status |
|:--------------:|------------|----------|:------:|
| F829 | OB-08: I3DArrayVariables GetDa/SetDa DA gap (ROOM_SMELL_WHOSE 5 occurrences in NTR_MASTER_SEX.ERB) | deferred | pending |

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F857 | [DRAFT] | NTR Behavioral Systems must complete first (master scenes reference behavioral state) |

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

[Predecessor: F857](feature-857.md) - NTR Behavioral Systems
[Related: F829](feature-829.md) - Source of OB-08
