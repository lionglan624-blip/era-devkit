# Feature 821: Weather System Migration

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

## Background

### Philosophy (Mid-term Vision)

Phase 22: State Systems -- Weather subsystem migration covering weather state management and child temperature resistance calculation (天候.ERB, 839 lines).

### Problem (Current Issue)

天候.ERB (839 lines) implements the weather subsystem including the `子供気温耐性取得` function at line 822, which is called by PREGNACY_S.ERB:426 creating a cross-subsystem dependency. This dependency means Weather must be migrated before Pregnancy can be migrated.

### Goal (What to Achieve)

Migrate 天候.ERB to C# implementing IWeatherSettings and related weather interfaces. Achieve zero-debt implementation with equivalence tests against ERB baseline.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | (stub - to be completed by /fc) | | | | | [ ] |
| 2 | No TODO/FIXME/HACK comments remain in Weather implementation (zero debt) | file | Grep | matches | No matches | [ ] |
| 3 | CP-2 E2E checkpoint: Weather integration verified | test | dotnet test | pass | All pass | [ ] |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | - | Remove TODO/FIXME/HACK comments from migrated code | | [ ] |
| 2 | - | Write equivalence tests against ERB baseline | | [ ] |

---

## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning
