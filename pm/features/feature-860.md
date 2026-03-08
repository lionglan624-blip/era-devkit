# Feature 860: NTR Message Generator

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

Pipeline Continuity — Phase 25: AI & Visitor Systems — NTR Message Generator.

### Problem (Current Issue)

NTR_MESSAGE.ERB is a self-contained 2,337-line file with 100+ functions that has no outbound CALL to other NTR files. NtrMessageGenerator.cs is prescribed by Phase 25 SRP split mandate. Only depends on NTR_UTIL functions from F856.

### Goal (What to Achieve)

Migrate NTR_MESSAGE.ERB (2,337 lines, 100+ functions) to C#. Produce NtrMessageGenerator.cs per SRP split mandate. Self-contained — no outbound NTR CALL dependencies.

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F856 | [DRAFT] | NTR Core/Util Foundation must complete first (NTR_MESSAGE uses NTR_UTIL functions) |
| Successor | F861 | [DRAFT] | Visitor/Event Core depends on NTR Message infrastructure |

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
