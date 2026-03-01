# NTR Core System Design

**Role**: This document serves as an INDEX and OVERVIEW only. Detailed specifications are in child documents.

v2.x-v5.x NTR System Reform specifications.

---

## Design Philosophy Change (Plan B Adoption)

- **Old**: "Same map = NTR progression" (automatic, monotonous)
- **New**: "Same map = Opportunity (OPP)" -> "Action choice" -> "Parameter change on success" -> "Threshold + Hub event = Phase transition"

---

## Parameter Definitions

See [phase-system.md](../designs/phase-system.md) for detailed parameter definitions.

---

## Phase Definitions (NTR Progression Stages)

See [phase-system.md](../designs/phase-system.md) for detailed phase definitions.

---

## Route Definitions (Reason for Phase 4 Point of No Return)

See [phase-system.md](../designs/phase-system.md) for detailed route definitions.

---

## Body Contact Zones

| Zone | Body Parts | Meaning |
|------|------------|---------|
| Public | Hands, Shoulders (surface) | Socially acceptable |
| Semi-Private | Hair, Back, Waist | Acceptable between close people |
| Private | Face, Neck, Thighs | Acceptable in intimate relationships |
| Forbidden | Chest, Genitals, Buttocks | Lovers/Spouses only |

---

## Exposure (Discovery) Stages

| Stage | Phase | Type |
|-------|:-----:|------|
| Minor Exposure | 2-3 | Small discomfort, seeds of suspicion |
| Medium Exposure | 4-5 | Near-conclusive evidence, not definitive |
| Major Exposure | 5-6 | Caught in act, confession, complete exposure |

**Discovery Types**:
- Peeping success (MC obtains evidence)
- Third party witness (mansion, village, etc.)
- Visitor hints (domination, isolation)
- Heroine breaks down and confesses

---

## Reconstruction Phase (RB Axis) - v7.x

| RB | Name | Content | Required Parameters |
|:--:|------|---------|---------------------|
| 0 | Severance | Complete relationship breakdown | - |
| 1 | Contact Permission | No blaming / No probing / Support | TRUST up |
| 2 | Conversation Habit | Daily conversation revival | TRUST threshold |
| 3 | Time Together Revival | Constrained dates | TRUST + AGENCY |
| 4 | Relationship Redefinition | Secret/Open, etc. | TRUST + AGENCY + AFF |
| 5 | Salvation Endings | Various endings | Complex conditions |

**RB3 Unlock Condition**: Requires **TRUST_MC + AGENCY** not AFF_MC (salvation credibility)

---

## Old System Mapping Table

### Parameters

| Old Parameter | New Parameter | Migration Policy |
|---------------|---------------|-----------------|
| Submission level | LEV + FAM + DEP composite | Gradual separation |
| Coercion level | LEV (Leverage/Evidence) | Rename and expand |
| Affection | LOVE_MC | Redefine as hard-to-overwrite axis |
| Weakness to visitor | Part of LEV | Integration |
| NTR pleasure mark | SEXB + Mark | Separate body habit and mark |
| Peeping discovery count | EXPOSE | Integrate into exposure system |

### Features

| Old Feature | New Feature | Migration Policy |
|-------------|-------------|-----------------|
| Same map -> Submission up | Same map -> OPP up | Auto-progress -> Opportunity accumulation |
| Submission threshold events | Phase threshold + Hub events | Gradual transition |
| Linear NTR | R1-R6 route branches | Branch at Phase 4 |
| Peeping/Witnessing | Staged exposure system | Expand to Minor/Medium/Major exposure |

---

## Dependency Graph

```
v1.6 (Kojo complete)
  |
  v
v2.x (Parameter reform) --- S1 introduction
  |
  v
v3.x (Phase management) --- S2 introduction
  |
  v
v4.x (Route branching) --- S3 introduction
  |
  v
v5.x (NTR completion)
  |
  +------------------+
  v                  v
v6.x (Netorase) -- S4   v7.x (Reconciliation) -- S5
  |                  |
  +--------+---------+
           v
v8.x (MC growth) -------- S6 introduction
  |
  v
v9.x (Pregnancy) -------- S7 introduction
  |
  v
v10.x (Incident war) ---- S8 introduction
  |
  v
v11.x (Media) ----------- S9 introduction
```

**Parallel Development**:
- v6.x (Netorase) and v7.x (Reconciliation) can be developed in parallel after v5.x completion
- Kojo (Content Track) can be developed ahead of System Track

---

## Links

- [content-roadmap.md](../content-roadmap.md) - Master roadmap
- [netorase-system.md](../designs/netorase-system.md) - Netorase system design
- [reconciliation-system.md](../designs/reconciliation-system.md) - Reconciliation system design
