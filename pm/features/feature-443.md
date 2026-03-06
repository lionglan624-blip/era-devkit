# Feature 443: REPEAT COUNT Variable Usage Analysis

## Status: [DONE]

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

## Type: infra

## Created: 2026-01-10

---

## Summary

COUNT Variable Usage Analysis for REPEAT/REND Design Decision

Analyze ERB codebase for COUNT:0 system variable usage to determine optimal REPEAT/REND implementation strategy in Era.Core.

**Design Decision Context**:
- **Legacy**: REPEAT uses COUNT:0 system variable with countup semantics (0→N)
- **F441 Current Spec**: Uses Frame.State with countdown semantics (N→0), no COUNT integration
- **Technical Debt Consideration**: Migrating legacy design patterns vs. improving architecture

**Output**:
- Design decision documentation
- Updated F441 Implementation Contract (or new implementation feature if major redesign needed)

---

## Background

### Philosophy (Mid-term Vision)

**Technical Debt Zero Principle**: Migration should not blindly copy legacy patterns. When legacy uses suboptimal design (global state like COUNT variable), evaluate whether:
1. Compatibility requires preserving the pattern
2. Improved design is possible without breaking compatibility

**Scope**: This Feature establishes the design decision for COUNT variable handling as SSOT for F441 implementation.

### Problem (Current Issue)

F441 FL review revealed fundamental mismatch between spec and legacy:

| Aspect | Legacy | F441 Spec |
|--------|--------|-----------|
| Counter direction | Countup (0→N) | Countdown (N→0) |
| State storage | COUNT:0 system variable (global) | Frame.State (local) |
| Loop check | `LoopEnd > counter` | `remaining <= 0` |

**Key Question**: Is COUNT:0 actually used by ERB scripts, or is it internal-only?

### Goal (What to Achieve)

1. **Determine COUNT:0 usage** in Game/ERB/
2. **Make design decision** based on findings
3. **Update F441** with correct implementation approach

### Impact Analysis

| Affected | Change Type | Description |
|----------|-------------|-------------|
| F441 | Design update | Implementation Contract updated with counter semantics |
| count-usage-analysis.md | New SSOT | Analysis results documented as reference |
| full-csharp-architecture.md | Potential | Phase 9 update if F441 rejected and replaced with new design feature (F444) |

---

## Acceptance Criteria

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | COUNT usage analysis complete | file | Glob | exists | Game/agents/reference/count-usage-analysis.md | [x] |
| 2 | Analysis categorizes usage patterns | file | Grep | contains | "## Internal" | [x] |
| 3 | Design decision documented in F441 | file | Grep | contains | "Design Decision:" | [x] |
| 4 | F441 Implementation Contract updated with counter semantics | file | Grep | contains | "Counter direction:" | [x] |

### AC Details

**AC#1**: COUNT usage analysis file exists
- Search Game/ERB/ for `COUNT:0`, `COUNT:`, `COUNT` references
- Document in `Game/agents/reference/count-usage-analysis.md`

**AC#2**: Analysis categorizes usage patterns
- Target: `Game/agents/reference/count-usage-analysis.md`
- Document patterns under categories: "Internal", "User Access", "User Modify"
- Each category should have findings or explicit "None found"

**AC#3**: Design decision documented
- Target: `Game/agents/feature-441.md`
- Add "Design Decision:" section to F441 Review Notes
- Options: (A) Legacy-compliant, (B) Improved design, (C) Hybrid
- Include rationale based on AC#1/AC#2 findings

**AC#4**: F441 Implementation Contract update
- Target: `Game/agents/feature-441.md`
- Update F441 Implementation Contract with "Counter direction:" specification
- Document correct semantics based on design decision (or create successor feature if major redesign)

---

## Tasks

| Task# | AC# | Description | Status |
|:-----:|:---:|-------------|:------:|
| 1 | 1 | Search ERB for COUNT variable usage patterns and create analysis file | [x] |
| 2 | 2 | Categorize findings into Internal/User Access/User Modify | [x] |
| 3 | 3 | Analyze findings and document design decision in F441 | [x] |
| 4 | 4 | Update F441 Implementation Contract with counter semantics | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Analysis Method

1. **Search for COUNT usage patterns**:
   ```bash
   grep -r "COUNT:0" Game/ERB/
   grep -r "COUNT:" Game/ERB/ | grep -v "PRINTCOUNT\|LINECOUNT"
   ```

2. **Categorize findings**:
   - **Internal**: Only in REPEAT loop context (legacy artifact)
   - **User Access**: Script reads COUNT for iteration number
   - **User Modify**: Script writes to COUNT (rare, problematic)

3. **Decision Matrix**:

   | Finding | Recommended Design |
   |---------|-------------------|
   | No COUNT access in scripts | F441 current spec (Frame.State, no COUNT) |
   | Read-only COUNT access | Hybrid: Frame.State + COUNT update for compatibility |
   | COUNT modification in scripts | Full legacy compliance required |

4. **Update F441 or create successor**:
   - Minor fix → Edit F441 Implementation Contract
   - Major redesign → Create F444 as F441 successor

---

## Dependencies

| Type | ID/Name | Reason |
|------|---------|--------|
| Related | F441 | Target feature for design decision |
| Reference | F432 | IExecutionStack interface (context) |

---

## Links

- [feature-441.md](feature-441.md) - REPEAT/REND (target of this analysis)
- [feature-432.md](feature-432.md) - Flow Control Commands (context)
- [index-features.md](index-features.md)

---

## Review Notes

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-01-10 | create | opus | Created for F441 design decision | PROPOSED |
| 2026-01-10 20:47 | START | implementer | Task 1 | - |
| 2026-01-10 20:47 | END | implementer | Task 1 | SUCCESS |
| 2026-01-10 20:47 | START | implementer | Task 2 | - |
| 2026-01-10 20:47 | END | implementer | Task 2 | SUCCESS |
| 2026-01-10 20:47 | START | implementer | Task 3 | - |
| 2026-01-10 20:47 | END | implementer | Task 3 | SUCCESS |
| 2026-01-10 20:47 | START | implementer | Task 4 | - |
| 2026-01-10 20:47 | END | implementer | Task 4 | SUCCESS |
| 2026-01-10 20:55 | END | feature-reviewer | Post-review complete | SUCCESS |
| 2026-01-10 21:00 | END | finalizer | Committed (d5eb040) | SUCCESS |
