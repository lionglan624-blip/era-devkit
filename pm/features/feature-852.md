# Feature 852: INtrCalculator Domain Service

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

Phase 24: NTR Bounded Context — Domain Layer (Services). Pipeline Continuity — the INtrCalculator Domain Service defines the calculation interface that operates on the NtrProgression Aggregate. It is an abstraction (interface only in Phase 24) that encapsulates NTR evaluation logic previously scattered across ERB utility functions.

### Problem (Current Issue)

NTR calculation logic in ERB source is spread across `NTR_UTIL.ERB` functions (`@NTR_CHK_FAVORABLY`, `@CHK_NTR_SATISFACTORY`) with loose parameter passing and implicit state dependencies. Phase 24 DDD design requires a formal Domain Service interface that declares the calculation contract, enabling Phase 25 to provide concrete implementations.

### Goal (What to Achieve)

Design and implement `INtrCalculator` interface in `src/Era.Core/NTR/Domain/Services/`:
- `INtrCalculator.cs` — Domain Service interface with at minimum: `CanAdvance()`, `CanChangeRoute()` methods
- Interface operates on NtrProgression Aggregate and NtrParameters Value Object

**DDD Input Reference**: `pm/reference/ntr-ddd-input.md` — NtrParameters Grounded status (required parameters for calculator TBD during design). The exact parameter set depends on which ERB parameters are mandatory vs optional, as evidenced by cross-character CHK_NTR_SATISFACTORY variance (0 for U_汎用 to 38 for K10).

### Architecture Task Coverage

<!-- Architecture Task 7: INtrCalculator Domain Service設計 -->

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F851 | [DRAFT] | NtrProgression Aggregate (INtrCalculator operates on Aggregate) |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | INtrCalculator interface exists | file | Glob(src/Era.Core/NTR/Domain/Services/INtrCalculator.cs) | gte | 1 | [ ] |
| 2 | No TODO/FIXME/HACK debt in deliverables | code | Grep(src/Era.Core/NTR/Domain/Services/, pattern="TODO\|FIXME\|HACK") | not_matches | TODO\|FIXME\|HACK | [ ] |

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
