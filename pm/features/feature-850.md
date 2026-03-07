# Feature 850: NTR Ubiquitous Language + Value Objects

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

Phase 24: NTR Bounded Context — Domain Layer (Value Objects). Pipeline Continuity — this sub-feature implements the foundational domain primitives that all subsequent Phase 24 sub-features depend on. The Ubiquitous Language glossary and Value Object definitions establish the shared vocabulary for the NTR Bounded Context.

### Problem (Current Issue)

Phase 24 requires typed C# Value Objects to replace loose ERB variable passing. The NTR system currently uses untyped FAV level integers, string-based route identifiers, and ad-hoc parameter bundles. Phase 24 DDD design introduces typed Value Objects: NtrRoute (R0-R6), NtrPhase (0-7), NtrParameters, and Susceptibility. The Ubiquitous Language glossary defines the shared terminology across the entire NTR Bounded Context.

### Goal (What to Achieve)

Implement NTR Ubiquitous Language definition (glossary) and four Value Objects in `src/Era.Core/NTR/Domain/ValueObjects/`:
- `NtrRoute.cs` — Route classification (R0-R6) for NTR progression paths
- `NtrPhase.cs` — Phase tracking (0-7) within NTR progression
- `NtrParameters.cs` — Typed container for NTR evaluation parameters
- `Susceptibility.cs` — Susceptibility state (屈服度/好感度 balance)
- Ubiquitous Language glossary (inline doc comments or separate markdown)

**DDD Input Reference**: `pm/reference/ntr-ddd-input.md` — distinguishes Validated candidates (FavLevel, AffairPermission, CorruptionState — directly observable in ERB) from Grounded candidates (NtrRoute, NtrPhase, NtrParameters — new concepts requiring Phase 24 design decisions to formalize as typed C# constructs).

### Architecture Task Coverage

<!-- Architecture Task 1: NTR Ubiquitous Language定義（用語辞書） -->
<!-- Architecture Task 3: NtrRoute Value Object実装（R0-R6） -->
<!-- Architecture Task 4: NtrPhase Value Object実装（Phase 0-7） -->
<!-- Architecture Task 5: NtrParameters Value Object実装（FAV_*, 露出度等） -->

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F849 | [DONE] | Phase 24 Planning (parent) |

---

## Acceptance Criteria

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | NtrRoute Value Object exists | file | Glob(src/Era.Core/NTR/Domain/ValueObjects/NtrRoute.cs) | gte | 1 | [ ] |
| 2 | NtrPhase Value Object exists | file | Glob(src/Era.Core/NTR/Domain/ValueObjects/NtrPhase.cs) | gte | 1 | [ ] |
| 3 | NtrParameters Value Object exists | file | Glob(src/Era.Core/NTR/Domain/ValueObjects/NtrParameters.cs) | gte | 1 | [ ] |
| 4 | Susceptibility Value Object exists | file | Glob(src/Era.Core/NTR/Domain/ValueObjects/Susceptibility.cs) | gte | 1 | [ ] |
| 5 | No TODO/FIXME/HACK debt in deliverables | code | Grep(src/Era.Core/NTR/Domain/ValueObjects/, pattern="TODO\|FIXME\|HACK") | not_matches | TODO\|FIXME\|HACK | [ ] |

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
