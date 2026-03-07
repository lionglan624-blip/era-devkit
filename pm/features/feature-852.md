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

**F850 Handoff**: NtrParameters satisfaction-state parameter expansion (CHK_NTR_SATISFACTORY optional params). Current F850 design has 2 fields (SlaveLevel, FavLevel); ntr-ddd-input.md suggests satisfaction-state tracking may be needed. INtrCalculator design should evaluate whether NtrParameters needs additional fields for its method signatures.

**F851 Handoff (3 items)**:
1. **NtrParameters/Susceptibility mutation methods + events**: NtrProgression aggregate has no methods to update Parameters or CurrentSusceptibility post-creation. Add mutation methods (e.g., `UpdateParameters(NtrParameters)`, `UpdateSusceptibility(Susceptibility)`) with corresponding domain events if runtime state changes are needed.
2. **ExposureDegree Value Object**: NtrExposureLevelChanged event uses raw `int` for exposure level (C3 constraint: no ExposureDegree VO in F850). Evaluate whether a typed `ExposureDegree` VO should replace the raw int for stronger domain modeling.
3. **NtrProgressionCreated domain event**: NtrProgression.Create() factory does not raise a creation event (no state transition at initialization). Add NtrProgressionCreated event if event-driven consumers need aggregate creation visibility.

### Architecture Task Coverage

<!-- Architecture Task 7: INtrCalculator Domain Service設計 -->

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F851 | [DONE] | NtrProgression Aggregate (INtrCalculator operates on Aggregate) |

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
