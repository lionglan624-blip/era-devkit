# Feature 769: Target-Only TALENT Evaluation with Runtime Target Resolution

## Status: [CANCELLED]

> **Cancellation Reason**: 等価性パイプライン完成(2364/2364 PASS)により実害なし。TALENT:PLAYERの12パターンはERB/YAML双方で同じELSE分岐に落ちるため等価性は保証済み。ランタイム精度向上は将来必要時に再検討。

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

Deferred from F760 (TALENT Target/Numeric Index Pattern Support). 12 target-only TALENT patterns (TALENT:PLAYER & N) parse correctly after F760 but cannot be evaluated at parse time. PLAYER resolves to a character index at runtime, and no static state producer currently emits TALENT:PLAYER state keys. F760 added code comments noting this limitation (AC#8 detail).

### Philosophy (Mid-term Vision)
(Inherited from F760/F758) TALENT is the SSOT for character attributes and must parse correctly across all ERA pattern forms. Runtime target resolution is required for complete evaluation coverage of all 12 TALENT:PLAYER patterns.

### Problem (Current Issue)
1. Target-only patterns (e.g., TALENT:PLAYER & 2) produce YAML key "PLAYER" and stateKey `TALENT:PLAYER`, but no state producer emits `TALENT:PLAYER` keys
2. KojoBranchesParser evaluates these patterns to default-0 (always fails ne: 0 check), causing incorrect branch selection
3. Runtime state injection is needed: PLAYER must resolve to a character index at runtime, then the corresponding TALENT:{charIndex}:{talentIndex} state must be looked up

### Goal (What to Achieve)
Implement runtime target resolution so that target-only TALENT patterns evaluate correctly. When PLAYER appears as a target, resolve it to the current player character index at runtime and look up the appropriate TALENT state for that character.

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F760 | [DONE] | Established target-aware parsing and state key format |

## Links
- [feature-760.md](feature-760.md) - Parent: TALENT target/numeric index support
- [feature-758.md](feature-758.md) - Foundation: VariableRef/VariableConditionParser generic pattern
