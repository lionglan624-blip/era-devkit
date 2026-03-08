# Feature 857: NTR Behavioral Systems

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

Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Behavioral Systems.

### Problem (Current Issue)

NTR_FRIENDSHIP.ERB, NTR_EXHIBITION.ERB, and NTR_SEX.ERB implement behavioral subsystems that depend on NTR_UTIL (F856). The MARK system state integration for CHK_NTR_SATISFACTORY (F852 obligation) lives in this behavioral layer.

### Goal (What to Achieve)

Migrate NTR_FRIENDSHIP.ERB (918 lines), NTR_EXHIBITION.ERB (469 lines), NTR_SEX.ERB (1,150 lines) to C#. Produce FriendshipSystem.cs, ExhibitionHandler.cs. Integrate MARK system state for CHK_NTR_SATISFACTORY (F852 obligation).

### Predecessor Obligations (for Mandatory Handoffs)

| Source Feature | Obligation | Category | Status |
|:--------------:|------------|----------|:------:|
| F852 | MARK system state integration for CHK_NTR_SATISFACTORY | deferred | pending |

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F856 | [DRAFT] | NTR Core/Util Foundation must complete first |
| Successor | F858 | [DRAFT] | NTR Master Scenes depends on behavioral systems |

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
[Related: F852](feature-852.md) - Source of CHK_NTR_SATISFACTORY obligation
