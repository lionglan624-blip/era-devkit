# Phase System Design

v3.x Phase Management specifications. Extracted and structured from [ntr-phase.md](ntr-phase.md) design discussion.

**Parent**: [ntr-core-overview.md](../reference/ntr-core-overview.md)

---

## Design Philosophy

**Core Principle**: NTR progression is not about "acts" but about "boundary erosion and self-narrative rewriting".

- **Old**: Same map = NTR progression (automatic, monotonous)
- **New**: Same map = Opportunity (OPP) -> Action choice -> Parameter change -> Threshold + Hub event = Phase transition

**Key Experience**: "Barely controllable" - Player feels they can manage, but control gradually slips away.

---

## Phase Definitions

| Phase | Name | Content | Transition Trigger |
|:-----:|------|---------|-------------------|
| 0 | Foundation | MC relationship visualization (trust/satisfaction) | Initial state |
| 1 | External Pressure | Coercion/Evidence/Debt/Environmental pressure | LEV or DEP threshold + Hub event |
| 2 | Familiarity | Dates, time together, secrets, habituation | FAM threshold + Hub event |
| 3 | Contact | Physical boundary movement (hand -> shoulder -> embrace) | FAM + TEM threshold + Hub event |
| 4 | Point of No Return | Self-narrative rewritten (route branch confirmed) | Complex conditions + Hub event |
| 5 | Sexual Relationship | Repetition, habituation, concealment skill development | After route confirmation |
| 6 | Full Corruption | Value/Priority transformation, agency change | Phase 5 duration + conditions |
| 7A | Selective Spread | Spread to specific partners (preference/situation) | Choice branch |
| 7B | Commercialization | Prostitution, entertainment (institutional) | External pressure + choice |
| 8+ | MC Aftermath | Reclaim / Trade / Complicity / Ruin-Release | Player choice chapter |

### Phase Transition Model

```
Transition = Threshold met + Hub Event occurred
           = (Parameter >= X) AND (OPP consumed by Visitor action)

Player intervention:
- Peeping: Gain information, but increases SUS and progress
- Prevention: Short-term success, but may backfire (rationalization)
- Neglect: Quiet progression (most dangerous)
```

---

## Route Definitions (Phase 4 Branch Reasons)

| Route | Name | Summary | LOVE_MC Tendency | Trigger Condition |
|:-----:|------|---------|:----------------:|------------------|
| R1 | Coercion/Sacrifice | For protection / No escape | Maintains | LEV high |
| R2 | Familiarity/Flow | Boundary collapse / Atmosphere | Maintains | FAM high + TEM mid |
| R3 | Escape/Anesthesia | Stress/Isolation refuge | Maintains | DEP high + SAT_MC low |
| R4 | Trade/Contract | Condition exchange | Maintains (self-esteem cost) | DEP mid + trade conditions |
| R5 | Rebellion/Revenge | Backlash from MC's failures | Falls | TRUST_MC low + MC failure |
| R6 | Real Affair | Romanticization toward visitor | Falls/Reverses | EMO high (strict unlock) |

### Route Philosophy

- **R1-R4**: "Loves MC but body yields" - Core NTR experience
- **R5-R6**: "Genuine affair" - Conditional unlock only

**Design Rule**: R6 (Real Affair) requires both MC failure accumulation AND visitor compatibility.

### Visitor Type → Route Mapping

**Detailed specification**: See [visitor-type-system.md](visitor-type-system.md)

Player selects visitor type at game start, which determines route tendency:

| Visitor Type | Primary Param | Route Tendency |
|--------------|:-------------:|:--------------:|
| 強制型 (Coercion) | LEV | R1 |
| 誘惑型 (Seduction) | FAM | R2 |
| 依存型 (Dependency) | DEP | R3/R4 |
| 情熱型 (Passion) | EMO | R6 |
| ランダム (Random) | Mixed | Emergent |

**Player Agency**:
- NTR Mode: Route direction determined by visitor type × heroine traits (player influences speed only)
- NTRase Mode (v6.x): Player can directly manipulate parameter accumulation via instructions

---

## Parameter Definitions

### Visitor -> Heroine (per pair)

| Parameter | Description | Primary Effect |
|-----------|-------------|----------------|
| OPP | Opportunity (same map accumulation) | Action availability |
| LEV | Leverage/Evidence (coercion basis) | R1 route fuel |
| FAM | Familiarity (boundary dissolution) | R2 route fuel |
| DEP | Dependency/Debt (support/rescue) | R3/R4 route fuel |
| TEM | Temptation fuel (FRUST + BORED + THRILL) x COMPAT | All route accelerator |
| SEXB | Body habit (physical association, not romance) | Post-Phase 4 growth |
| EMO | Romanticization (real affair core) | R6 unlock only |
| SUS | Suspicion (MC awareness level) | Discovery risk |
| EXPOSE | Exposure stage | Discovery event trigger |

### MC <-> Heroine (per Heroine)

| Parameter | Description | Design Note |
|-----------|-------------|-------------|
| LOVE_MC | Romantic affection to MC | Hard to overwrite (core of "loves but yields") |
| TRUST_MC | Trust/Security to MC | Falls with lies/distance |
| SAT_MC | MC satisfaction (conversation/relationship quality) | Affects FRUST |
| RES | Resistance (relationship base + MC satisfaction) | Defense barrier |

### Temptation Fuel Components

| Component | Description | Source |
|-----------|-------------|--------|
| LIBIDO | Base sexual desire | Character trait + situation |
| FRUST | Sexual frustration | Low SAT_MC accumulation |
| BORED | Boredom/Seeking stimulation | Stability accumulation |
| THRILL | Thrill-seeking | Character trait + secrets |

**Composite**: `TEM = (FRUST + BORED + THRILL) × COMPAT × Situation modifier`

---

## Phase × Route Matrix

### Parameter Flow by Phase and Route

| Phase | R1 Coercion | R2 Familiarity | R3 Escape | R4 Trade | R5 Rebellion | R6 Affair |
|:-----:|-------------|----------------|-----------|----------|--------------|-----------|
| **0** | LOVE↑ TRUST↑ SAT↑ | Same | Same | Same | SAT↓ seeds | Same |
| **1** | LEV↑ SUS↑ | FAM↑(small) OPP only | Stress→TEM↑ | LEV/DEP↑ conditions | TRUST↓ SAT↓ | TEM↑ (no EMO yet) |
| **2** | DEP↑(small) | FAM↑ DEP↑ secrets | DEP↑ TEM↑ "relief" | DEP↑ RES↓ costs | TEM↑ "spite" seeds | TEM↑ (EMO conditional) |
| **3** | LEV↑ RES↓ forced | FAM↑↑ RES↓ excuse→silence | TEM↑ DEP↑ escape | Contract contact | TRUST↓ SEXB↑ | TEM↑↑ (EMO small) |
| **4** | Secret community | Refusal language gone | Escape place formed | Sold self by condition | Intentional secrets | Romance starts: EMO↑↑ |
| **5** | SEXB↑ LOVE stays | SEXB↑↑ habit | TEM↑ when suffering | DEP/LEV↑ routine | LOVE↓ TRUST↓ compare | EMO↑↑ love overwrite |
| **6** | Value shift mild | Boundary never returns | Escape habituated | Institutionalized | Hostility stable | Love reversal possible |
| **7** | 7A:LEV extension 7B:external | 7A:stimulus seeking | 7A:escape expansion | 7B natural (trade) | 7A/7B both | 7A (exclusive tendency) |
| **8+** | Reclaim (guilt core) | Reset boundary | Reconnect (escape resolve) | Buy/Contract cancel | Apology/Break/Renegotiate | Difficult repair (alt ED) |

---

## Change Signs by Phase

Observable changes that signal progression:

| Phase | Sign | Kojo Expression |
|:-----:|------|-----------------|
| 2-3 | Flashier clothing/grooming | "Did something good happen?" |
| 2-3 | Time with MC decreases | Frequent outings, busy excuses |
| 3 | Ring off ("forgot it") | **Phase 4 border indicator** |
| 3-4 | Seems bored when together | Conversation decrease, distracted |
| 4-5 | MC sex feels off | Less responsive, going through motions |
| 5 | Schedule conflicts | "Routine" with visitor |
| 5-6 | Body changes | Physical adaptation signs |
| 6 | Priority reversal | MC becomes "convenient" not "important" |

### Ring Event Placement

The "wedding ring removed, just forgot" event is optimal at **Phase 3 late ~ Phase 4 early**.
- Maximum impact when LOVE_MC still remains
- Signals secret community formation
- Works for both R1 (must hide) and R2 (habit) routes

---

## Discovery Events (Exposure System)

### Staged Discovery

| Stage | Phase | Content | Outcome |
|-------|:-----:|---------|---------|
| Minor | 2-3 | Suspicion, discomfort, excuses work | SUS↑, can recover |
| Medium | 4-5 | Near-proof, explanations needed, lies accumulate | Relationship distorts |
| Major | 5-6 | Caught in act, confession, complete exposure | Relationship redefinition |

### Discovery Types

| Type | Trigger | System Effect |
|------|---------|---------------|
| Peeping success | Player action | MC obtains evidence (LEV reversal possible) |
| Third party witness | EXPOSE threshold + location | Reputation/External pressure |
| Visitor hints | Visitor action (domination) | Isolation acceleration |
| Heroine breakdown | GUILT high + trigger | Confession event |

---

## Post-Reveal Routes (Phase 6~8+)

After major discovery, relationship enters redefinition:

| Post-Route | Name | Condition | Content |
|:----------:|------|-----------|---------|
| A | Repair/Reclaim | TRUST recovery actions | MC takes back (NTR reversal) |
| B | Open Coexistence | TRUST low but maintained | Three-way relationship |
| C | Transfer | EMO high, LOVE_MC low | Officially visitor's partner |
| D | Role Fixation | SEXB high, institutionalized | Phase 7B connection |
| E | Secret Reconnection | BOND_MC_SECRET growth | "Don't tell them" events |

### Secret Reconnection Route (Special)

**New Parameter**: `BOND_MC_SECRET` - Secret bond with MC (not romance)

Growth conditions:
- MC doesn't interrogate/probe
- Life support/security provision
- Higher visitor control = higher growth when successful (danger fuels it)

Result: "Fallen, yet also with MC..." - Taboo × Secret experience

---

## Phase 6: Full Corruption Detail

### Corruption Criteria (3 conditions)

1. **Agency Shift**: Passive → Active planning (AGENCY↑)
2. **Permanent Boundary Drop**: RES recovery coefficient↓ (never returns)
3. **Relationship Redefinition**: MC relationship reclassified (ROLE_MC changes)

### Phase 6 Reveal Variations

| Type | Condition | Content |
|------|-----------|---------|
| Victory Declaration | EMO high, LOVE_MC low | Open announcement, can have LOVE_MC=0 |
| Resignation | SEXB high, TRUST low | "Tired of hiding", "Don't care anymore" |
| Cry for Help | LOVE_MC remains, GUILT high | "Stop me", "Help" but can't return |

### LOVE_MC Handling Modes

| Mode | Description | Use Case |
|------|-------------|----------|
| A (Protected) | LOVE_MC minimum guaranteed | "Loves but yields" core |
| B (Conditional Zero) | MC failure accumulation → LOVE_MC=0 possible | R5 route |
| C (Locked Zero) | Full corruption locks LOVE_MC | Recovery difficult endings |

---

## Body Contact Zones

Physical boundary progression detail:

| Zone | Body Parts | Social Meaning | Excuse Type |
|------|------------|----------------|-------------|
| Z0 Public | Fingers, forearm | Accident/Courtesy/Work | 100% excusable |
| Z1 Semi-Private | Shoulder, upper back, hair | Distance invasion | Needs excuse |
| Z2 Private | Waist, thighs, embrace distance | "Just us two" territory | Must hide |
| Z3 Forbidden | Chest, genitals, buttocks | Lovers/Spouses only | Point of no return |

### Contact Event Tags

| Tag | Values | Effect |
|-----|--------|--------|
| Intent | Accident / Work / Comfort / Play / Domination | Changes meaning of same touch |
| Duration | Instant / Seconds / Continuous | Accumulation effect |
| Secrecy | Public / Semi-private / Private / Secret | SUS impact |
| Initiative | Visitor→ / Heroine→ / Mutual | Agency indicator |

### Contact Progression Stages

| Stage | Zone | Characteristics | Effect |
|:-----:|:----:|-----------------|--------|
| A | Z0 | Accident (100% excuse) | FAM+ small |
| B | Z0-Z1 | Pretext required | FAM+ mid, SUS+ small |
| C | Z1 | Refusal = "ruins atmosphere" | DEP+ small, RES- mid |
| D | Z1-Z2 | Private space, zero distance | FAM+ large, THRILL+ mid |
| E | Z2 | Boundary test (reaction observation) | Branch by response |
| F | Z2-Z3 | Point of no return | RES recovery↓, auto-progress |

---

## Gensokyo World Adaptation

Modern concepts translated to Touhou setting:

| Modern | Gensokyo Equivalent |
|--------|---------------------|
| Tea/Cafe | Reception room / Garden pavilion / Library reading circle |
| Dinner | Kitchen tasting / Banquet prep / Staff meals |
| Bar/Drinks | Banquet (strongest Gensokyo content) / Evening gathering |
| Karaoke (private room) | Song gathering / Magic acoustics / Small theater |
| Beach/Pool | Genbu Ravine / Misty Lake / Hot springs / SDM pool |
| Nightclub | Remilia's night party / Magic illumination / Mansion ball |

---

## Implementation Roadmap

### v3.0: Phase Management Foundation

| Feature | Content | Dependencies |
|---------|---------|--------------|
| Phase 0-2 implementation | Foundation/Pressure/Familiarity | S1 params |
| Hub event system | Threshold + event triggers | OPP system |
| Basic transition logic | Parameter-based progression | - |

### v3.1: Phase Management Expansion

| Feature | Content | Dependencies |
|---------|---------|--------------|
| Phase 3-4 implementation | Contact/Point of No Return | v3.0 |
| Route branching at Phase 4 | R1-R6 condition evaluation | v3.0 |
| Change signs integration | Observable progression markers | - |

### v3.2: Body Contact Zone Management

| Feature | Content | Dependencies |
|---------|---------|--------------|
| Zone system (Z0-Z3) | Boundary tracking per pair | v3.1 |
| Contact event tags | Intent/Duration/Secrecy/Initiative | v3.1 |
| Zone progression events | Stage A-F implementation | v3.1 |

### v3.3: Hub Event System

| Feature | Content | Dependencies |
|---------|---------|--------------|
| Discovery events (Minor/Medium/Major) | Staged exposure | v3.1 |
| Post-Reveal routing | Repair/Open/Transfer/Role/Secret | v3.1 |
| Full corruption events | Phase 6 completion | v3.1 |

---

## MC Interaction During NTR

NTR進行中の主人公（MC）とのインタラクション分岐設計。

**Detailed specification**: See [kojo-phases.md](../reference/kojo-phases.md) Phase 8m section.

**Summary**:
- Skinship Light (COM 0, 1): ~130 lines
- Skinship Medium (COM 20, 21): ~160 lines
- Sexual Interaction (COM 3-12, 60-90): ~500 lines
- Exposure Reaction: ~240 lines
- **Total**: ~1030 lines

---

## Links

- [ntr-phase.md](ntr-phase.md) - Original design discussion
- [ntr-core-overview.md](../reference/ntr-core-overview.md) - Parent design (parameters/routes overview)
- [content-roadmap.md](../content-roadmap.md) - Master roadmap
- [kojo-phases.md](../reference/kojo-phases.md) - Kojo content phases
