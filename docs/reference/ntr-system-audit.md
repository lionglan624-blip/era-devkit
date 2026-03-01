# NTR System Documentation Audit

**Date**: 2026-01-04
**Status**: ✅ All critical issues resolved

---

## Critical Issues

### Issue 1: Duplicate Phase/Route Definitions (SSOT Violation)

**Locations**:
- `designs/phase-system.md` - Phase 0-7, Route R1-R6, Parameters
- `reference/ntr-core-overview.md` - Same tables duplicated

**Fix**: Remove detailed tables from `ntr-core-overview.md`, keep only links to `phase-system.md`

### Issue 2: Duplicate MC Interaction Content

**Locations**:
- `designs/phase-system.md` lines 305-451 "MC Interaction During NTR" section
- `reference/kojo-phases.md` Phase 8m section (lines 135-327)

**Fix**: Remove from `phase-system.md`, add reference to `kojo-phases.md` Phase 8m

### Issue 3: Duplicate Satisfaction Calculation Code

**Locations**:
- `designs/phase-system.md` @CALC_MC_SEX_SATISFACTION
- `designs/ntr-flavor-stats.md` @CALC_CHINPO_RANK + @CALC_ORGASM_MODIFIER
- `designs/reconciliation-system.md` @CALC_CARNAL_SATISFACTION

**Fix**: Designate `ntr-flavor-stats.md` as SSOT for all stat calculations, others reference it

### Issue 4: Version Number Inconsistency

**Problem**:
- `reconciliation-system.md` says "v2.5 (S2+)"
- `ntr-core-overview.md` dependency graph shows v7.x for reconciliation

**Fix**: Update `reconciliation-system.md` to "v7.x (S5)" to match dependency graph

### Issue 5: Netorase Kojo Partial Duplication

**Locations**:
- `designs/netorase-system.md` "Kojo Branching Design" section
- `reference/kojo-phases.md` Phase 8n section

**Fix**: `netorase-system.md` should reference `kojo-phases.md` Phase 8n, not duplicate content

---

## Medium Issues

### Issue 6: Parent-Child Relationship Unclear

**Problem**: `phase-system.md` declares `ntr-core-overview.md` as parent, but phase-system has more detail

**Fix**: Clarify in `ntr-core-overview.md` that it's an INDEX/OVERVIEW only, detailed specs are in child docs

### Issue 7: CFLAG ID Range Conflicts - Resolved (Feature 340)

**Status**: ✅ Resolved (Feature 340)

**Resolution**: Unified CFLAG allocation table created in [`reference/cflag-ntr-allocation.md`](cflag-ntr-allocation.md)

**New ranges (conflict-free)**:
| Range | Document | Content |
|:-----:|----------|---------|
| 90-108 | ntr-flavor-stats | Chinpo stats, development |
| 109-121 | netorase-system | Netorase permits, fuzoku |
| 122-137 | reconciliation-system | Two-axis affection, trust |

---

## Fix Plan

### Fix A: ntr-core-overview.md Simplification
- Remove detailed Phase/Route/Parameter tables
- Keep only overview text and links to child documents
- Clarify role as INDEX only

### Fix B: phase-system.md Deduplication
- Remove "MC Interaction During NTR" section (lines 305-451)
- Add reference: "See [kojo-phases.md](../reference/kojo-phases.md) Phase 8m"
- Remove duplicated satisfaction calculation, reference ntr-flavor-stats.md

### Fix C: netorase-system.md Deduplication
- Simplify "Kojo Branching Design" section
- Reference kojo-phases.md Phase 8n for detailed line counts

### Fix D: reconciliation-system.md Version Fix
- Change "v2.5 (S2+)" to "v7.x (S5)"
- Add dependency on Phase System completion

---

## Execution Order

1. Fix D (version number) - Simple edit
2. Fix A (ntr-core-overview simplification) - Medium complexity
3. Fix B (phase-system deduplication) - Medium complexity
4. Fix C (netorase-system deduplication) - Low complexity

---

## Links

- [phase-system.md](../designs/phase-system.md)
- [ntr-core-overview.md](ntr-core-overview.md)
- [netorase-system.md](../designs/netorase-system.md)
- [reconciliation-system.md](../designs/reconciliation-system.md)
- [ntr-flavor-stats.md](../designs/ntr-flavor-stats.md)
- [kojo-phases.md](kojo-phases.md)
