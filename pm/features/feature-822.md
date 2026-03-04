# Feature 822: Pregnancy System Migration

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

Phase 22: State Systems -- Pregnancy subsystem migration covering pregnancy state management, child movement events, and pregnancy events (PREGNACY_S.ERB, PREGNACY_S_CHILD_MOVEMENT.ERB, PREGNACY_S_EVENT.ERB, PREGNACY_S_EVENT0.ERB).

### Problem (Current Issue)

Four pregnancy ERB files totalling 2,534 lines require migration: PREGNACY_S.ERB (2372 lines), PREGNACY_S_CHILD_MOVEMENT.ERB (118 lines), PREGNACY_S_EVENT.ERB (28 lines), PREGNACY_S_EVENT0.ERB (16 lines). PREGNACY_S.ERB:426 calls `子供気温耐性取得` defined in 天候.ERB:822, creating a mandatory Predecessor dependency on F821 (Weather). PREGNACY_S.ERB also has external call dependencies to 多生児パッチ.ERB, 体設定.ERB, and 妊娠処理変更パッチ.ERB which require interface stubs.

### Goal (What to Achieve)

Migrate all four pregnancy ERB files to C# implementing IPregnancySettings and related interfaces. Create interface stubs for external dependencies (多生児パッチ.ERB, 体設定.ERB, 妊娠処理変更パッチ.ERB). Achieve zero-debt implementation with equivalence tests against ERB baseline.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F814 | [DONE] | Phase 22 Planning |
| Predecessor | F821 | [DRAFT] | Weather System -- PREGNACY_S.ERB:426 calls 子供気温耐性取得 defined in 天候.ERB:822 |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | (stub - to be completed by /fc) | | | | | [ ] |
| 2 | No TODO/FIXME/HACK comments remain in Pregnancy implementation (zero debt) | file | Grep | matches | No matches | [ ] |
| 3 | CP-2 E2E checkpoint: Pregnancy integration verified | test | dotnet test | pass | All pass | [ ] |

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | - | Remove TODO/FIXME/HACK comments from migrated code | | [ ] |
| 2 | - | Write equivalence tests against ERB baseline | | [ ] |
| 3 | - | Create interface stubs for external dependencies (多生児パッチ.ERB, 体設定.ERB, 妊娠処理変更パッチ.ERB) | | [ ] |

---

## Links

- [Predecessor: F814](feature-814.md) - Phase 22 Planning
- [Predecessor: F821](feature-821.md) - Weather System (cross-subsystem call dependency)
