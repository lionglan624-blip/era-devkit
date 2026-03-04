# Feature 824: Sleep & Menstrual System Migration

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

Phase 22: State Systems -- Sleep depth and menstrual cycle subsystem migration covering sleep state tracking and menstrual cycle management (睡眠深度.ERB, 生理機能追加パッチ.ERB).

### Problem (Current Issue)

Two ERB files totalling 407 lines implement sleep and menstrual subsystems: 睡眠深度.ERB (244 lines) and 生理機能追加パッチ.ERB (163 lines). These subsystems are independent of other Phase 22 subsystems and depend only on F814.

### Goal (What to Achieve)

Migrate 睡眠深度.ERB and 生理機能追加パッチ.ERB to C# implementing the relevant sleep and menstrual interfaces. Achieve zero-debt implementation with equivalence tests against ERB baseline.

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
| 2 | No TODO/FIXME/HACK comments remain in Sleep & Menstrual implementation (zero debt) | file | Grep | matches | No matches | [ ] |
| 3 | CP-2 E2E checkpoint: Sleep & Menstrual integration verified | test | dotnet test | pass | All pass | [ ] |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | - | Remove TODO/FIXME/HACK comments from migrated code | | [ ] |
| 2 | - | Write equivalence tests against ERB baseline | | [ ] |

---

## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning
