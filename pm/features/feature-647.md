# Feature 647: Phase 20 Planning

## Status: [DONE]
<!-- fl-reviewed: 2026-02-11T00:00:00Z -->

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

## Type: research

## Created: 2026-01-27

---

## Summary

Feature to create Features: Phase 20 Planning (Equipment & Shop Systems). Create sub-features for Phase 20 implementation following F555 pattern:
- F774: Shop Core (SHOP.ERB + SHOP2.ERB)
- F775: Collection (SHOP_COLLECTION.ERB)
- F776: Items (SHOP_ITEM.ERB + アイテム説明.ERB)
- F777: Customization (SHOP_CUSTOM.ERB + CHARA_CUSTUM.ERB + TALENTCOPY.ERB)
- F778-F781: Body Settings (4 sub-features for 体設定.ERB decomposition)
- F782: Post-Phase Review Phase 20
- F783: Phase 21 Planning

---

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. SSOT: `designs/full-csharp-architecture.md` Phase 20 section (line 4549+) defines the scope; this feature decomposes it into actionable sub-features.

### Problem (Current Issue)

Phase 20 (Equipment & Shop Systems) encompasses 9 ERB files totaling 4,173 lines, but naive per-file decomposition is unreliable because:

1. **Extreme complexity variance**: File sizes range from 28 lines (CHARA_CUSTUM.ERB, a thin wrapper calling SHOP_CUSTOM.ERB:CUSTOM_TERMINAL) to 1,976 lines (体設定.ERB with 25+ functions across 5 distinct functional areas), making uniform per-file features inappropriate.

2. **Tightly coupled subsystem clusters**: Files form functional groups with internal cross-calls — SHOP.ERB:42 calls ITEM_BUY (SHOP_ITEM.ERB), SHOP.ERB:40 calls SHOW_COLLECTION (SHOP_COLLECTION.ERB), SHOP_CUSTOM.ERB:107 calls COPY_CUSTOM (TALENTCOPY.ERB). Splitting coupled files into separate features would create artificial boundaries.

3. **Architecture.md estimates are significantly inaccurate**: SHOP.ERB is listed as ~800 lines but actual is 197 (-75%), CHARA_CUSTUM.ERB as ~300 but actual is 28 (-91%), SHOP_ITEM.ERB as ~200 but actual is 559 (+180%) (full-csharp-architecture.md:4594-4602). Additionally, line 4633 contains a stale "Phase 19" reference that should read "Phase 20" (full-csharp-architecture.md:4549 heading confirms the renumbering).

4. **体設定.ERB warrants multiple sub-features**: At 1,976 lines with 5 distinct functional areas — per-character initialization (lines 6-348), interactive settings UI (lines 350-943), genetic inheritance with mutation/multi-birth logic (lines 944-1341), child growth (lines 1342-1426), and visitor settings (lines 1431-end) — a single feature cannot adequately cover all subsystems.

### Goal (What to Achieve)

Decompose Phase 20 into ~10-11 sub-features grouped by functional subsystem cohesion (not per-file), following the F555 planning pattern:

1. **Subsystem grouping**: Create sub-features for Shop Core (SHOP+SHOP2), Collection (SHOP_COLLECTION), Items (SHOP_ITEM+アイテム説明), Character Customization (SHOP_CUSTOM+CHARA_CUSTUM+TALENTCOPY), and 4 体設定 sub-features (initialization, interactive UI, genetics, visitor)
2. **Transition features**: Create Post-Phase Review Phase 20 and Phase 21 Planning features
3. **Architecture correction**: Fix stale Phase 19 reference (line 4633) and inaccurate line counts in full-csharp-architecture.md
4. **Dependency mapping**: Document cross-file call interfaces and external callers (PREGNACY_S.ERB, OPTION.ERB, MANSETTTING.ERB) that must be preserved
5. **Update index-features.md** with all Phase 20 sub-features

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does Phase 20 need decomposition? | 9 ERB files (4,173 lines total) need sub-features before implementation can begin | full-csharp-architecture.md:4549-4643 |
| 2 | Why can't we decompose per-file? | Extreme complexity variance (28-1,976 lines) and tightly coupled subsystem clusters make naive per-file decomposition unreliable | SHOP.ERB:197 lines vs 体設定.ERB:1,976 lines |
| 3 | Why can't we rely on architecture.md estimates? | Line count estimates are inaccurate (up to -91% and +180% variance) and contain stale Phase 19 references | full-csharp-architecture.md:4594-4602 (estimated), wc -l (actual) |
| 4 | Why are the estimates wrong? | Architecture.md Phase 20 section was written before Phase 19→20 renumbering; line counts were estimated without empirical verification | full-csharp-architecture.md:4587,4588,4633,4638 (stale refs) |
| 5 | Why weren't estimates verified? | No automated mechanism exists to validate architecture.md against codebase; each Planning Feature must independently verify and correct | F555 pattern: same manual verification step |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Architecture.md line counts are inaccurate (-91% to +180%); stale "Phase 19" references exist; per-file decomposition produces unbalanced features | Estimates written without `wc -l` verification; phase renumbering did not propagate; files form 4 coupled subsystem clusters |
| Where | full-csharp-architecture.md:4594-4602 (line counts), lines 4587,4588,4633,4638 (stale refs); 9 Phase 20 ERB files | Architecture.md Phase 20 section (authored pre-renumbering); cross-file call graph (SHOP→SHOP_ITEM, SHOP_CUSTOM→TALENTCOPY) |
| Fix | Correct architecture.md (empirical line counts + phase references); decompose by subsystem cohesion (not per-file) with 体設定 split into 4 sub-features | Verify estimates against codebase; group by call-graph coupling; split large files at function boundaries |

## Related Features

| Feature | Status | Relationship |
|---------|--------|--------------|
| F555 | [DONE] | Pattern precedent — Phase 19 Planning created 15 sub-features (F633-F647). F647 follows same decomposition pattern |
| F703 | [CANCELLED] | Predecessor (Redux) — Cancelled, F646 [DONE] sufficient |
| F646 | [DONE] | Related (original review) — Post-Phase Review Phase 19 |
| F541 | [DONE] | Pattern precedent — Phase 18 Planning, pipeline continuity chain |
| F706 | [DONE] | Unrelated — KojoComparer Full Equivalence, not Phase 20 scope |

<!-- Pattern: 6th Planning Feature in pipeline chain (F471→F486→F503→F516→F541→F555→F647). F555 created 15 sub-features; F647 targets ~10 based on smaller Phase 20 scope (9 files, 4,173 lines). -->

## Feasibility Assessment

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Problem is solvable | FEASIBLE | All 9 ERB files exist, line counts verified, call graph mapped. Decomposition is straightforward |
| Scope is realistic | FEASIBLE | F555 precedent created 15 sub-features. F647 targets ~10 (fewer source files). Estimated 1-2 sessions |
| No blocking constraints | FEASIBLE | F703 (Predecessor) [CANCELLED] — predecessor dissolved. F646 [DONE] sufficient. No remaining blockers |

**Verdict**: FEASIBLE

The decomposition is straightforward given verified file sizes and call-graph data. F703 predecessor was cancelled (F646 [DONE] sufficient), removing the only blocking constraint.

## Impact Analysis

| Area | Impact | Description |
|------|:------:|-------------|
| docs/architecture/migration/full-csharp-architecture.md | LOW | Fix stale Phase 19/17/18 references (lines 4587, 4588, 4633, 4638), correct line count estimates (lines 4594-4602) |
| pm/features/feature-774.md through feature-783.md | MEDIUM | Create ~10 new sub-feature DRAFT files |
| pm/index-features.md | LOW | Add Phase 20 section with all sub-features, increment Next Feature number |
| pm/features/feature-647.md | LOW | Execution Log with decomposition results |

## Technical Constraints

| Constraint | Source | Impact |
|------------|--------|--------|
| Next Feature number is 774 (index-features.md line 174) | index-features.md | LOW - Sub-features will start at F774 |
| F703 is [CANCELLED] (Predecessor dissolved) | feature-703.md | LOW - No longer a blocker. F646 [DONE] sufficient |
| Architecture.md has 4 stale phase references | full-csharp-architecture.md lines 4587, 4588, 4633, 4638 | LOW - Must be corrected as part of F647 Task |
| Architecture.md Tasks 8-9 reference Phase 17/18 (should be Phase 20/21) | full-csharp-architecture.md lines 4587-4588 | LOW - Additional stale references from original (pre-renumbering) content |
| 体設定.ERB requires 4+ sub-features due to 5 functional areas across 1,976 lines | Codebase analysis | MEDIUM - Largest single file. Sub-feature boundaries must align with function boundaries |
| Cross-file calls create hard coupling within subsystem clusters | Codebase call-graph analysis | MEDIUM - Features must group coupled files together, not split them |

## Risks

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| ~~F703 blocking risk~~ | ~~N/A~~ | ~~N/A~~ | F703 [CANCELLED] — risk eliminated. Predecessor dissolved |
| Sub-feature count exceeds 15 (complexity creep) | LOW | LOW | F555 precedent shows 15 is manageable. Target 10-13 based on smaller scope |
| External callers (PREGNACY_S, SYSTEM, EVENTTURNEND) create hidden dependencies for sub-features | MEDIUM | MEDIUM | Call-graph documented in Dependencies:Consumers. Each sub-feature must list external callers |
| architecture.md correction creates merge conflicts with other features | LOW | LOW | Corrections are limited to Phase 20 section (lines 4549-4643). No other active feature edits this section |

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Next Feature number is 774 | index-features.md:174 | AC for sub-feature creation must use F774+ IDs |
| C2 | F555 created 15 sub-features as precedent | feature-555.md | AC for feature count should target ~10-13, not exceed 15 without justification |
| C3 | Architecture.md has 4 stale phase references + inaccurate line counts | full-csharp-architecture.md:4587,4588,4633,4638,4594-4602 | AC must verify architecture.md corrections are included as a Task |
| C4 | 体設定.ERB has 5 distinct functional areas across 1,976 lines | Codebase: 体設定.ERB function analysis | AC for 体設定 decomposition must verify 4+ sub-features (not 1 monolithic feature) |
| C5 | Research type: 3-5 AC count guideline | RESEARCH.md checklist | AC count should be 3-5 (flexible with justification per F424/F437 precedent) |
| C6 | F703 is [CANCELLED] Predecessor dissolved | feature-647.md Dependencies | No longer a blocker. F646 [DONE] sufficient |
| C7 | Transition features required | architecture.md:4640-4642, F555 pattern | Must create Post-Phase Review Phase 20 + Phase 21 Planning features |

### Constraint Details

**C1: Next Feature Number**
- **Source**: `Grep("Next Feature number", "pm/index-features.md")` → line 174: "774"
- **Verification**: Check index-features.md before feature creation
- **AC Impact**: Sub-features must use F774-F783 range (approximately). AC Expected for feature count must use this starting ID

**C2: F555 Precedent (15 sub-features)**
- **Source**: F555 created F633-F647 (15 features) for Phase 19 (117 files)
- **Verification**: `Grep("F633-F647", "pm/features/feature-555.md")`
- **AC Impact**: F647 targets fewer sub-features (~10-13) because Phase 20 has only 9 files (vs 117). Count must be justified in Implementation Contract

**C3: Stale Architecture.md References**
- **Source**: Investigation found 4 stale references:
  - Line 4587: "Phase 17" should be "Phase 20"
  - Line 4588: "Phase 18" should be "Phase 21"
  - Line 4633: "Phase 19" should be "Phase 20"
  - Line 4638: "Phase 20" should be "Phase 21"
  - Lines 4594-4602: Line count estimates significantly inaccurate
- **Verification**: Read architecture.md lines 4587-4638
- **AC Impact**: Must include architecture.md correction Task. AC should verify corrections applied

**C4: 体設定.ERB Decomposition**
- **Source**: Function analysis shows 5 areas:
  1. Per-character initialization (lines 6-348, 14 functions)
  2. Interactive settings UI (lines 350-743, 2 functions)
  3. Body detail arrangement/options (lines 743-943, 2 functions)
  4. Daily change + genetics (lines 932-1426, 4 functions)
  5. Visitor settings (lines 1431-1976, 3 functions)
- **Verification**: `Grep("^@", "Game/ERB/体設定.ERB")`
- **AC Impact**: 体設定 must be split into at minimum 3 sub-features (initialization+UI, genetics+growth, visitor). 4 sub-features preferred for proper granularity

**C5: Research Type AC Count**
- **Source**: RESEARCH.md checklist: "3-5 AC count guideline (flexible with justification)"
- **Verification**: Read RESEARCH.md
- **AC Impact**: Keep AC count at 3-5. If more needed, document justification in Review Notes

**C7: Transition Features**
- **Source**: architecture.md line 4640: "Mandatory Transition Features: Review (infra) + Planning (research)"
- **Verification**: F555 created F646 (Post-Phase Review Phase 19) and F647 (Phase 20 Planning) as transition features
- **AC Impact**: Must include creation of Post-Phase Review Phase 20 (type: infra) and Phase 21 Planning (type: research) as part of sub-feature allocation

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|:------:|-------------|
| Predecessor | F703 | [CANCELLED] | Post-Phase Review Phase 19 Redux — Predecessor解消。F646 [DONE]で十分 |
| Related | F646 | [DONE] | Post-Phase Review Phase 19 (Original) |
| Related | F555 | [DONE] | Phase 19 Planning (pattern/precedent for decomposition) |
| Successor | F774 | [DONE] | Shop Core (SHOP.ERB + SHOP2.ERB) |
| Successor | F775 | [DONE] | Collection (SHOP_COLLECTION.ERB) |
| Successor | F776 | [DONE] | Items (SHOP_ITEM.ERB + アイテム説明.ERB) |
| Successor | F777 | [DONE] | Customization (SHOP_CUSTOM.ERB + CHARA_CUSTUM.ERB + TALENTCOPY.ERB) |
| Successor | F778 | [DONE] | Body Initialization (体設定.ERB lines 6-348) |
| Successor | F779 | [DONE] | Body Settings UI (体設定.ERB lines 350-943) |
| Successor | F780 | [PROPOSED] | Genetics & Growth (体設定.ERB lines 944-1426) |
| Successor | F781 | [DONE] | Visitor Settings (体設定.ERB lines 1431-1976) |
| Successor | F782 | [DRAFT] | Post-Phase Review Phase 20 |
| Successor | F783 | [DRAFT] | Phase 21 Planning |

<!-- Redux Pattern: F646 had 残課題 → F649-F702 fixes → F703 Redux required before Planning -->

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| full-csharp-architecture.md (Phase 20 section, lines 4549-4643) | Documentation | Low | SSOT for Phase 20 scope. Contains stale references needing correction |
| Era.Core DI interfaces (IShopSystem, IInventoryManager, IBodySettings) | Design | Low | Architecture.md defines required interfaces at lines 4562-4577 |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| SYSTEM.ERB | HIGH | Entry points: `BEGIN SHOP` (line 57, 146), `CALL CHARA_CUSTUM` (line 122), `TRYCALL 体詳細初期設定` (line 232), `CALL CUSTOM_CHARAMAKE` (line 69) |
| EVENTTURNEND.ERB | HIGH | `BEGIN SHOP` (line 66), `CALL 体変化_１日経過` (line 12) |
| PREGNACY_S.ERB | HIGH | `CALL 体設定_遺伝` (line 376), `CALL 体設定_子供髪変更` (line 706), `CALL 体設定_子供Ｐ成長` (line 707) |
| OPTION.ERB | MEDIUM | `CALL 体詳細設定１` (line 43) |
| MANSETTTING.ERB | MEDIUM | `CALL 体詳細設定訪問者` (line 294) |
| 追加パッチverup.ERB | MEDIUM | `CALL 体詳細初期設定`, `CALL 体詳細設定１`, `CALL 体設定_遺伝`, `CALL 体設定_子供髪変更` (lines 109-122) |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "continuous development pipeline" | Phase 20 must be fully decomposed into sub-features so implementation can proceed without gaps | AC#1, AC#2, AC#6 |
| "clear phase boundaries" | Sub-features must be grouped by functional subsystem cohesion with documented cross-file dependencies | AC#4 |
| "documented transition points" | Post-Phase Review Phase 20 and Phase 21 Planning features must be created | AC#3 |
| SSOT: "full-csharp-architecture.md Phase 20 section" | Architecture.md stale references and inaccurate line counts must be corrected | AC#5 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Implementation sub-features with scope references | file | Grep("## Scope Reference", pm/features/feature-77[4-9].md + feature-78[01].md) | count_equals | 8 | [x] |
| 2 | Index updated with Phase 20 sub-features | file | Grep("F77[4-9]\|F78[0-3]", pm/index-features.md) | count_equals | 10 | [x] |
| 3 | Transition features created | file | Glob(pm/features/feature-78[23].md) + Grep(pm/index-features.md) | count_equals + contains | 2 AND "Post-Phase Review Phase 20" AND "Phase 21 Planning" | [x] |
| 4 | Dependency mapping documented | file | Grep(pm/features/feature-647.md) | contains | "Cross-File Call Interfaces:" | [x] |
| 5 | Architecture.md corrections applied | file | Grep(pm/features/feature-647.md) | contains | "Architecture Corrections Applied:" | [x] |
| 6 | Next Feature number incremented | file | Grep("Next Feature number", pm/index-features.md) | contains | "784" | [x] |

### AC Details

**AC#1: Implementation sub-features with scope references**
- **Test**: `Grep("## Scope Reference", "pm/features/feature-77[4-9].md")` + `Grep("## Scope Reference", "pm/features/feature-78[01].md")` → count matching files
- **Expected**: All 8 implementation sub-features (F774-F781) contain a `## Scope Reference` section documenting source files and function ranges
- **Rationale**: Satisfies C1 (F774+ IDs), C2 (~10 sub-features), C4 (体設定.ERB split into 4 sub-features: F778-F781). Verifies subsystem grouping quality by confirming each implementation feature has documented scope, not just file existence. Transition features (F782-F783) verified by AC#3. Total 10 DRAFT files verified by AC#2 (index registration)
- **Note**: Both Grep results are summed; combined count must equal 8 for AC to PASS

**AC#2: Index updated with Phase 20 sub-features**
- **Test**: `Grep("F77[4-9]|F78[0-3]", "pm/index-features.md")` → count matching lines
- **Expected**: 10 entries matching F774-F783 in index-features.md, confirming all Phase 20 sub-features are registered
- **Rationale**: Satisfies C1 (F774+ IDs), C2 (~10 entries). count_equals 10 verifies all sub-features (F774-F783) are individually registered, not just the last one. Previous AC checked only F783 presence which didn't guarantee F774-F782 registration. Next Feature number increment verified by AC#6

**AC#3: Transition features created**
- **Test**: `Glob("pm/features/feature-78[23].md")` count_equals 2 + `Grep("Post-Phase Review Phase 20", "pm/index-features.md")` + `Grep("Phase 21 Planning", "pm/index-features.md")`
- **Expected**: (1) Both transition feature DRAFT files exist on disk (feature-782.md, feature-783.md), AND (2) Both registered in index: Post-Phase Review Phase 20 (type: infra) and Phase 21 Planning (type: research)
- **Rationale**: Satisfies C7 (mandatory transition features per architecture.md and F555 pattern) AND feature-template.md DRAFT Creation Checklist (file existence + index registration). DRAFT file existence was previously unverified (AC-COV gap); Glob check closes this gap
- **Note**: All three checks must pass (AND conjunction): Glob count_equals 2 for file existence, both Grep results for index registration

**AC#4: Dependency mapping documented**
- **Test**: `Grep("Cross-File Call Interfaces:", "pm/features/feature-647.md")`
- **Expected**: Execution Log contains a "Cross-File Call Interfaces:" section documenting inter-file calls within Phase 20 ERB files and external callers (SYSTEM.ERB, EVENTTURNEND.ERB, PREGNACY_S.ERB, OPTION.ERB, MANSETTTING.ERB, 追加パッチverup.ERB)
- **Rationale**: Each sub-feature must list which external callers invoke its functions, so implementers know which interfaces to preserve. The mapping must cover all 6 consumers identified in the Dependencies:Consumers table

**AC#5: Architecture.md corrections applied**
- **Test**: `Grep("Architecture Corrections Applied:", "pm/features/feature-647.md")`
- **Expected**: Execution Log contains an "Architecture Corrections Applied:" section documenting all corrections made to full-csharp-architecture.md
- **Rationale**: Satisfies C3 (4 stale phase references at lines 4587, 4588, 4633, 4638 + inaccurate line counts at lines 4594-4602). Must document corrections for all 4 stale references (Phase 17→20, Phase 18→21, Phase 19→20, Phase 20→21) and updated line counts based on empirical `wc -l` measurements. The actual edits to architecture.md are performed as a Task; this AC verifies the corrections are documented in the Execution Log

**AC#6: Next Feature number incremented**
- **Test**: `Grep("Next Feature number", "pm/index-features.md")` → check line contains "784"
- **Expected**: index-features.md "Next Feature number" line contains "784", confirming the number was incremented past all allocated sub-features (F774-F783)
- **Rationale**: Prevents conflicting IDs on subsequent `/next` invocations. Task#5 specifies "update Next Feature number to 784" but without AC verification, the increment could be missed, causing duplicate F774 allocation

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Subsystem grouping (Shop Core, Collection, Items, Customization, 4x 体設定) | AC#1 |
| 2 | Transition features (Post-Phase Review Phase 20, Phase 21 Planning) | AC#3 |
| 3 | Architecture correction (stale refs + line counts) | AC#5 |
| 4 | Dependency mapping (cross-file calls + external callers) | AC#4 |
| 5 | Update index-features.md | AC#2, AC#6 |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

The implementation follows a four-phase process: **Codebase Analysis → Subsystem Grouping → Feature Creation → Index/Architecture Updates**. This approach differs from naive per-file decomposition by grouping files based on functional cohesion and call-graph coupling.

**Phase 1: Verify File Metrics**
- Run `wc -l Game/ERB/*.ERB` for all 9 Phase 20 files to obtain empirical line counts
- Compare with architecture.md estimates (lines 4594-4602) to identify correction deltas
- Document findings in `.tmp/phase20-metrics.txt` for reproducibility

**Phase 2: Analyze Call Graph**
- Use `Grep("^@", "Game/ERB/{file}.ERB")` to map all function definitions
- Use `Grep("CALL|TRYCALL|CALLFORM", "Game/ERB/{file}.ERB")` to map internal cross-file calls
- Use `Grep("CALL.*体設定|CALL.*SHOP|CALL.*CUSTOM", "Game/ERB/SYSTEM.ERB Game/ERB/PREGNACY_S.ERB Game/ERB/OPTION.ERB Game/ERB/MANSETTTING.ERB Game/ERB/EVENTTURNEND.ERB Game/ERB/追加パッチverup.ERB")` to identify external callers
- Output: Dependency matrix showing which files call which functions

**Phase 3: Subsystem Grouping**
- **Shop Core**: SHOP.ERB + SHOP2.ERB (tightly coupled main loop and helpers)
- **Collection**: SHOP_COLLECTION.ERB (collection gallery UI)
- **Items**: SHOP_ITEM.ERB + アイテム説明.ERB (item purchase + description display)
- **Customization**: SHOP_CUSTOM.ERB + CHARA_CUSTUM.ERB + TALENTCOPY.ERB (character appearance/talent customization with CUSTOM_TERMINAL → COPY_CUSTOM call chain)
- **体設定 Sub-Features** (4 features):
  1. **Body Initialization**: Per-character initialization functions (lines 6-348, 14 functions)
  2. **Body Settings UI**: Interactive settings UI + body detail arrangement (lines 350-943, 4 functions)
  3. **Genetics & Growth**: Daily change + genetics with multi-birth/mutation logic + child growth (lines 944-1426, 4 functions)
  4. **Visitor Settings**: Visitor body settings (lines 1431-1976, 3 functions)
- **Transition Features**: Post-Phase Review Phase 20 (type: infra), Phase 21 Planning (type: research)

**Phase 4: Create DRAFT Files**
- Generate feature-774.md through feature-783.md (approximately 10 files)
- Each DRAFT includes:
  - Type (erb for implementation features, infra/research for transitions)
  - Background with inherited Phase 20 philosophy
  - Scope Reference listing source ERB files and function ranges
  - Dependencies:Consumers table listing external callers (SYSTEM, PREGNACY_S, etc.) that must be preserved
  - Stub for Investigation phase (to be completed during `/fc {ID}`)
- Register all features in index-features.md Phase 20 section
- Update "Next Feature number" in index-features.md to F784

**Phase 5: Correct Architecture.md**
- Edit full-csharp-architecture.md lines 4587, 4588, 4633, 4638 to fix stale phase references
- Edit lines 4594-4602 to replace estimated line counts with empirical values from Phase 1
- Document all changes in Execution Log "Architecture Corrections Applied:" section

This approach satisfies the Philosophy of "clear phase boundaries" by grouping tightly coupled files together (SHOP+SHOP2, SHOP_CUSTOM+CHARA_CUSTUM+TALENTCOPY) while isolating independent subsystems (Collection, Items) into separate features. It satisfies "continuous development pipeline" by creating actionable sub-features with documented external dependencies that implementers can execute sequentially.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Phase 3 subsystem grouping produces 8 implementation sub-features (Shop Core, Collection, Items, Customization, 4x Body) + 2 transition features = 10 total. Each implementation DRAFT (F774-F781) must include `## Scope Reference` section with Source Files table documenting files and function ranges. Document allocation in Execution Log "Phase 20 Feature Allocation:" section |
| 2 | Phase 4 registers all allocated features in index-features.md Phase 20 section. Verify with Grep("Phase 20", "index-features.md") |
| 3 | Phase 3 includes Post-Phase Review Phase 20 (infra) and Phase 21 Planning (research) as the final two features in allocation. Verify with Grep("Post-Phase Review Phase 20", "index-features.md") |
| 4 | Phase 2 call-graph analysis produces dependency matrix showing internal calls (e.g., SHOP → SHOP_ITEM, SHOP_CUSTOM → TALENTCOPY) and external callers. Document in Execution Log "Cross-File Call Interfaces:" section |
| 5 | Phase 5 applies all architecture.md corrections and documents in Execution Log "Architecture Corrections Applied:" section with before/after table for 4 stale references and 9 line count corrections |
| 6 | Phase 4 updates "Next Feature number" in index-features.md to 784 after registering all sub-features (F774-F783) |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Grouping Strategy | (A) Per-file (9 features), (B) Per-subsystem (4-6 features), (C) Hybrid subsystem with 体設定 split (9-11 features) | C - Hybrid | Option A creates unbalanced features (28-line vs 1,976-line). Option B lumps 体設定's 5 functional areas into one feature (violates granularity guide). Option C groups coupled files (SHOP+SHOP2) while splitting 体設定 into 4 features for proper granularity |
| 体設定 Decomposition | (A) 1 feature (all 1,976 lines), (B) 3 features (init+UI, genetics, visitor), (C) 4 features (init, UI, genetics+growth, visitor), (D) 5 features (init, UI, genetics, growth, visitor) | C - 4 features | Option A violates 500-line erb guideline. Option B merges UI + detail arrangement (lines 350-943) which could be 2 features. Option C aligns with function boundaries and keeps genetics+growth together (related logic). Option D over-splits genetics and growth (both modify FLAG:体型 state) |
| Transition Feature Count | (A) 1 (Post-Phase Review only), (B) 2 (Review + Planning) | B - 2 features | F555 pattern includes both transitions. Architecture.md line 4640 mandates "Review (infra) + Planning (research)" |
| Architecture.md Corrections | (A) Defer to separate feature, (B) Include in F647 | B - Include | Corrections are Phase 20-specific (stale references, line counts). Deferring would leave architecture.md inaccurate during Phase 20 implementation |
| Feature ID Range | (A) F774-F782 (~9 features), (B) F774-F783 (~10 features) | B - F774-F783 | Allows buffer for final grouping decisions during implementation. Actual count determined by call-graph analysis in Phase 2 |

### Interfaces / Data Structures

**Call Dependency Matrix Format** (`.tmp/phase20-call-graph.txt`):

```
Source File | Function | Calls → Target File:Function | External Callers
------------|----------|------------------------------|------------------
SHOP.ERB | @SHOP | SHOP_ITEM.ERB:ITEM_BUY | SYSTEM.ERB:57, EVENTTURNEND.ERB:66
SHOP_CUSTOM.ERB | @CUSTOM_TERMINAL | TALENTCOPY.ERB:COPY_CUSTOM | SYSTEM.ERB:69
体設定.ERB | @体詳細設定１ | (none) | OPTION.ERB:43, 追加パッチverup.ERB:111
```

**Feature Allocation Table Format** (in Execution Log):

```
| Feature ID | Subsystem | Source Files | Line Count | Key Functions | External Callers |
|------------|-----------|--------------|------------|---------------|------------------|
| F774 | Shop Core | SHOP.ERB, SHOP2.ERB | 197+{SHOP2} | @SHOP, @SHOP2 | SYSTEM, EVENTTURNEND |
| F775 | Collection | SHOP_COLLECTION.ERB | {count} | @SHOW_COLLECTION | SHOP.ERB |
| F776 | Items | SHOP_ITEM.ERB, アイテム説明.ERB | 559+{desc} | @ITEM_BUY | SHOP.ERB |
| F777 | Customization | SHOP_CUSTOM.ERB, CHARA_CUSTUM.ERB, TALENTCOPY.ERB | {counts} | @CUSTOM_TERMINAL, @COPY_CUSTOM | SYSTEM |
| F778 | Body Init | 体設定.ERB (6-348) | 342 | @体詳細初期設定, etc. | SYSTEM, 追加パッチverup |
| F779 | Body UI | 体設定.ERB (350-943) | 593 | @体詳細設定１, @体設定_並び替え | OPTION, 追加パッチverup |
| F780 | Genetics+Growth | 体設定.ERB (944-1426) | 482 | @体設定_遺伝, @体設定_子供髪変更, @体設定_子供Ｐ成長 | PREGNACY_S, 追加パッチverup |
| F781 | Visitor Settings | 体設定.ERB (1431-1976) | 545 | @体詳細設定訪問者 | MANSETTTING |
| F782 | Post-Phase Review Phase 20 | (infra) | - | - | - |
| F783 | Phase 21 Planning | (research) | - | - | - |
```

**Architecture Corrections Table Format** (in Execution Log):

```
| Line | Before | After | Type |
|------|--------|-------|------|
| 4587 | Phase 17 | Phase 20 | Stale reference |
| 4588 | Phase 18 | Phase 21 | Stale reference |
| 4633 | Phase 19 | Phase 20 | Stale reference |
| 4638 | Phase 20 | Phase 21 | Stale reference |
| 4594 | ~800 lines | 197 lines | Line count (SHOP.ERB) |
| 4595 | ~300 lines | 28 lines | Line count (CHARA_CUSTUM.ERB) |
| 4596 | ~200 lines | 559 lines | Line count (SHOP_ITEM.ERB) |
| ... | ... | ... | ... |
```

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1,4 | Verify Phase 20 file metrics (wc -l), document in .tmp/phase20-metrics.txt | | [x] |
| 2 | 4 | Analyze call graph (function definitions, cross-file calls, external callers), document in .tmp/phase20-call-graph.txt | | [x] |
| 3 | 1,4 | Create subsystem grouping allocation table (Shop Core, Collection, Items, Customization, 4x Body, 2x Transition) | | [x] |
| 4 | 1 | Create F774-F783 DRAFT files with Type, Background, Scope Reference, Dependencies:Consumers | | [x] |
| 5 | 2,3,6 | Register all Phase 20 sub-features in index-features.md, update Next Feature number to 784 | | [x] |
| 6 | 5 | Correct architecture.md stale references (lines 4587,4588,4633,4638) and line counts (lines 4594-4602) | | [x] |
| 7 | 4,5 | Document Phase 20 Feature Allocation, Cross-File Call Interfaces, Architecture Corrections in Execution Log | | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

### Execution Sequence

<!-- Research type: Agent/Model columns use implementer(sonnet) for all phases (single-agent analysis workflow, not multi-agent orchestration) -->

| Phase | Agent | Model | Input | Output |
|:-----:|-------|:-----:|-------|--------|
| 1 | implementer | sonnet | Phase 20 ERB files | `.tmp/phase20-metrics.txt` (empirical line counts) |
| 2 | implementer | sonnet | Phase 20 ERB files | `.tmp/phase20-call-graph.txt` (dependency matrix) |
| 3 | implementer | sonnet | Call graph + metrics | Allocation table (10 sub-features) |
| 4 | implementer | sonnet | Allocation table | feature-774.md through feature-783.md |
| 5 | implementer | sonnet | DRAFT files | index-features.md Phase 20 section |
| 6 | implementer | sonnet | Metrics from Phase 1 | full-csharp-architecture.md corrections |
| 7 | implementer | sonnet | All phases | Execution Log sections |

### DRAFT File Template Structure

Each feature-{ID}.md DRAFT must include:

```markdown
# Feature {ID}: {Title}

## Status: [DRAFT]

## Scope Discipline
{Copy verbatim from feature-template.md}

## Type: {erb | infra | research}

## Created: 2026-02-11

---

## Summary

{1-2 sentence description of the sub-feature scope}

---

## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. SSOT: `designs/full-csharp-architecture.md` Phase 20 section (line 4549+) defines the scope; F647 decomposed it into actionable sub-features.

### Problem (Current Issue)

{Specific problem this sub-feature addresses - inherited from F647 decomposition}

### Goal (What to Achieve)

{Specific goal for this sub-feature}

<!-- Sub-Feature Requirements (architecture.md:4629-4637): /fc時に以下を反映すること
  1. Philosophy継承: Phase 20: Equipment & Shop Systems
  2. Tasks: 負債解消 (TODO/FIXME/HACKコメント削除タスクを含む)
  3. Tasks: 等価性検証 (legacy実装との等価性テストを含む)
  4. AC: 負債ゼロ (技術負債ゼロを検証するACを含む)
-->

---

## Scope Reference

### Source Files

| File | Lines | Functions | Note |
|------|------:|-----------|------|
| {file.ERB} | {count} | {function list} | {note} |

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [WIP] | Phase 20 Planning (decomposed this feature) |
| Related | F{sibling} | [DRAFT] | {Related Phase 20 sub-feature} |

### External Dependencies

| Component | Type | Risk | Note |
|-----------|------|------|------|
| {external caller file} | Interface Preservation | HIGH/MEDIUM/LOW | {caller usage description} |

### Consumers (Reverse Dependencies)

| Consumer | Impact Level | Usage |
|----------|--------------|-------|
| {calling file} | HIGH/MEDIUM/LOW | {function call description} |

---

## Links

- [feature-647.md](feature-647.md) - Phase 20 Planning (parent)
```

### Expected Feature ID Allocation

**Decomposition Rationale**: architecture.md lists Phase 20 as 9 ERB files, but allocation shows 10-13 features because:
- Shop Core groups SHOP.ERB + SHOP2.ERB (tightly coupled)
- Customization groups SHOP_CUSTOM.ERB + CHARA_CUSTUM.ERB + TALENTCOPY.ERB (call-chain coupling)
- 体設定.ERB (1,976 lines) splits into 4 features for proper granularity (initialization, UI, genetics+growth, visitor)
- 2 transition features (Post-Phase Review Phase 20, Phase 21 Planning) required by architecture.md and F555 pattern

| Subsystem | Feature IDs | Count | Rationale |
|-----------|-------------|:-----:|-----------|
| Shop Core | F774 | 1 | SHOP.ERB + SHOP2.ERB (coupled main loop) |
| Collection | F775 | 1 | SHOP_COLLECTION.ERB (independent gallery UI) |
| Items | F776 | 1 | SHOP_ITEM.ERB + アイテム説明.ERB (item purchase + description) |
| Customization | F777 | 1 | SHOP_CUSTOM.ERB + CHARA_CUSTUM.ERB + TALENTCOPY.ERB (call-chain coupling) |
| Body Initialization | F778 | 1 | 体設定.ERB lines 6-348 (14 functions) |
| Body Settings UI | F779 | 1 | 体設定.ERB lines 350-943 (4 functions) |
| Genetics & Growth | F780 | 1 | 体設定.ERB lines 944-1426 (4 functions) |
| Visitor Settings | F781 | 1 | 体設定.ERB lines 1431-1976 (3 functions) |
| Post-Phase Review Phase 20 | F782 | 1 | Transition feature (type: infra) |
| Phase 21 Planning | F783 | 1 | Transition feature (type: research) |
| **Total** | **F774-F783** | **10** | All Phase 20 components covered |

### Architecture.md Correction Details

**Stale Phase References** (4 corrections):

| Line | Context | Before | After |
|------|---------|--------|-------|
| 4587 | Task 8 description | "Phase 17" | "Phase 20" |
| 4588 | Task 9 description | "Phase 18" | "Phase 21" |
| 4633 | Implementation Note | "Phase 19" | "Phase 20" |
| 4638 | Transition Features | "Phase 20" | "Phase 21" |

**Line Count Corrections** (9 corrections based on empirical wc -l; {actual}/{delta} placeholders populated by Task#1 during execution):

| Line | File | Estimated | Actual | Delta |
|------|------|----------:|-------:|------:|
| 4594 | SHOP.ERB | ~800 | 197 | -603 (-75%) |
| 4595 | SHOP2.ERB | ~400 | 246 | -154 (-39%) |
| 4596 | SHOP_COLLECTION.ERB | ~300 | 353 | +53 (+18%) |
| 4597 | SHOP_CUSTOM.ERB | ~250 | 472 | +222 (+89%) |
| 4598 | SHOP_ITEM.ERB | ~200 | 559 | +359 (+180%) |
| 4599 | CHARA_CUSTUM.ERB | ~300 | 28 | -272 (-91%) |
| 4600 | TALENTCOPY.ERB | ~100 | 110 | +10 (+10%) |
| 4601 | 体設定.ERB | 1,974 | 1,976 | +2 (+0.1%) |
| 4602 | アイテム説明.ERB | ~200 | 232 | +32 (+16%) |

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-11 | init | wbs-generator | Phase 5 completion | Tasks/Contract/Handoffs/Log/Links generated |
| 2026-02-11 20:34 | END | implementer | Task 5 | SUCCESS |
| 2026-02-11 20:45 | END | implementer | Task 7 | SUCCESS |

Phase 20 Feature Allocation:

| Feature ID | Subsystem | Source Files | Line Count | Key Functions | External Callers |
|------------|-----------|--------------|------------|---------------|------------------|
| F774 | Shop Core | SHOP.ERB, SHOP2.ERB | 443 | @SHOW_SHOP, @USERSHOP, @SCHEDULE, @SET_FUTANARI_ALL, @DEBUG_ENTER_UFUFU, @LIFE_LIST, @SHOW_CHARADATA, @SHOW_CHARADATA2 | SYSTEM.ERB (BEGIN SHOP), EVENTTURNEND.ERB (BEGIN SHOP) |
| F775 | Collection | SHOP_COLLECTION.ERB | 353 | @SHOW_COLLECTION, @ShowCollection_Panties, @ShowCollection_PhotoSingle, @ShowCollection_PhotoCheating, @ShowCollection_PhotoProstitute, @ShowCollection_PhotoDouble, @SellCollection_RiskCheck | SHOP.ERB |
| F776 | Items | SHOP_ITEM.ERB, アイテム説明.ERB | 791 | @ITEM_BUY, @SHOW_ITEM, @ITEM_SALES, @アイテム説明_{N} (37 functions) | SHOP.ERB |
| F777 | Customization | SHOP_CUSTOM.ERB, CHARA_CUSTUM.ERB, TALENTCOPY.ERB | 610 | @CUSTOM_CHARAMAKE, @CUSTOM_TERMINAL, @NAME_CUSTOM, @BASE_CUSTOM, @TALENT_CUSTOM, @ABL_CUSTOM, @EXP_CUSTOM, @CLOTHES_CUSTOM, @VIRGIN_CUSTOM, @CHARA_CUSTUM, @REVERSEMODE_1, @COPY_CUSTOM | SYSTEM.ERB (4 entry points) |
| F778 | Body Init | 体設定.ERB (6-348) | 342 | @体詳細初期設定, @体詳細初期設定0-13 (15 functions) | SYSTEM.ERB, CHARA_SET.ERB, 追加パッチverup.ERB |
| F779 | Body UI | 体設定.ERB (350-943) | 593 | @体詳細設定１, @体詳細設定２, @体詳細整頓, @体詳細オプション設定 | OPTION.ERB, 追加パッチverup.ERB |
| F780 | Genetics+Growth | 体設定.ERB (944-1426) | 482 | @体変化_１日経過, @体設定_遺伝, @体設定_子供Ｐ成長, @体設定_子供髪変更 | EVENTTURNEND.ERB, PREGNACY_S.ERB, 追加パッチverup.ERB |
| F781 | Visitor Settings | 体設定.ERB (1431-1976) | 545 | @体詳細設定訪問者, @体詳細整頓訪問者, @体詳細オプション設定訪問者 | MANSETTTING.ERB |
| F782 | Post-Phase Review Phase 20 | (infra) | - | - | - |
| F783 | Phase 21 Planning | (research) | - | - | - |

Cross-File Call Interfaces:

Internal Calls (within Phase 20):
- SHOP.ERB → SHOP2.ERB:SHOW_CHARADATA, SHOP_COLLECTION.ERB:SHOW_COLLECTION, SHOP_ITEM.ERB:ITEM_BUY
- SHOP_CUSTOM.ERB → TALENTCOPY.ERB:COPY_CUSTOM
- CHARA_CUSTUM.ERB → SHOP_CUSTOM.ERB:CUSTOM_TERMINAL, SHOP_CUSTOM.ERB:CLOTHES_CUSTOM
- SHOP_ITEM.ERB → アイテム説明.ERB:アイテム説明_{N} (CALLFORM dynamic dispatch)
- 体設定.ERB → internal hierarchy (14 initialization sub-functions, settings → arrangement chain)

External Callers (6 consumers):
- SYSTEM.ERB: BEGIN SHOP (SHOP.ERB), CUSTOM_CHARAMAKE (SHOP_CUSTOM.ERB:69), CHARA_CUSTUM (CHARA_CUSTUM.ERB:122), VIRGIN_CUSTOM (SHOP_CUSTOM.ERB:127), REVERSEMODE_1 (TALENTCOPY.ERB:86), TRYCALL 体詳細初期設定 (体設定.ERB:232)
- EVENTTURNEND.ERB: BEGIN SHOP (SHOP.ERB), 体変化_１日経過 (体設定.ERB:12)
- PREGNACY_S.ERB: 体設定_遺伝 (体設定.ERB:376)
- OPTION.ERB: 体詳細設定１ (体設定.ERB:43)
- MANSETTTING.ERB: 体詳細設定訪問者 (体設定.ERB:294)
- 追加パッチverup.ERB: 体詳細初期設定 (体設定.ERB:109), 体設定_遺伝 (体設定.ERB:117/119)

Additional External Caller (discovered):
- CHARA_SET.ERB: TRYCALLFORM 体詳細初期設定{LOCAL} (体設定.ERB:138/181)

Architecture Corrections Applied:

Stale Phase References (4 corrections in full-csharp-architecture.md):
| Line | Before | After | Type |
|------|--------|-------|------|
| 4587 | Phase 17 | Phase 20 | Stale reference |
| 4588 | Phase 18 | Phase 21 | Stale reference |
| 4633 | Phase 19 | Phase 20 | Stale reference |
| 4638 | Phase 20 | Phase 21 | Stale reference |

Line Count Corrections (9 corrections in full-csharp-architecture.md):
| Line | File | Estimated | Actual | Delta |
|------|------|----------:|-------:|------:|
| 4594 | SHOP.ERB | ~800 | 197 | -603 (-75%) |
| 4595 | SHOP2.ERB | ~400 | 246 | -154 (-39%) |
| 4596 | SHOP_COLLECTION.ERB | ~300 | 353 | +53 (+18%) |
| 4597 | SHOP_CUSTOM.ERB | ~250 | 472 | +222 (+89%) |
| 4598 | SHOP_ITEM.ERB | ~200 | 559 | +359 (+180%) |
| 4599 | CHARA_CUSTUM.ERB | ~300 | 28 | -272 (-91%) |
| 4600 | TALENTCOPY.ERB | ~100 | 110 | +10 (+10%) |
| 4601 | 体設定.ERB | 1,974 | 1,976 | +2 (+0.1%) |
| 4602 | アイテム説明.ERB | ~200 | 232 | +32 (+16%) |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase1-RefCheck iter1: Links section | Added F471, F486, F503, F516 pipeline continuity chain links
- [fix] Phase1-RefCheck iter1: Links section | Fixed archived feature paths (feature-555, 646, 541, 516, 503, 486, 471 → archive/ prefix)
- [fix] Phase2-Review iter1: Root Cause Analysis > 5 Whys | Reformatted numbered list to template-mandated table (Level, Question, Answer, Evidence)
- [fix] Phase2-Review iter1: Root Cause Analysis > Symptom vs Root Cause | Restructured to template format (Aspect, Symptom, Root Cause) with What/Where/Fix rows
- [fix] Phase2-Review iter1: Acceptance Criteria | Added missing Goal Coverage Verification subsection
- [fix] Phase2-Review iter1: Technical Constraints, AC Design Constraints, Implementation Contract, Expected Feature ID Allocation | Updated stale F758-F770 range to F774-F783 (Next Feature number is 774)
- [fix] Phase2-Review iter1: Feasibility Assessment, Technical Constraints, Risks, AC Design Constraints C6 | Updated stale F703 [DRAFT] references to [CANCELLED]
- [fix] Phase2-Review iter2: Dependencies table | Reordered columns to template format (Type, Feature, Status, Description) and added 10 Successor entries for F774-F783
- [fix] Phase2-Review iter2: Summary | Expanded to list expected sub-features F774-F783 per RESEARCH.md checklist
- [fix] Phase2-Review iter2: Impact Analysis | Changed columns to template format (Area, Impact, Description) with impact levels
- [fix] Phase2-Review iter2: Implementation Contract Execution Sequence | Changed columns to template format (Phase, Agent, Model, Input, Output)
- [fix] Phase2-Review iter2: DRAFT File Template | Changed Predecessor F647 status from [DRAFT] to [WIP]
- [fix] Phase2-Review iter3: Related Features table | Removed extra Note column, merged into Relationship
- [fix] Phase2-Review iter3: Root Cause Analysis | Removed extra Conclusion subsection (not in template)
- [fix] Phase2-Review iter3: Related Features | Removed extra Pattern Analysis subsection (moved to comment)
- [fix] Phase2-Review iter3: AC#1 | Changed Method from Grep(feature-647.md) contains to Glob(feature-{774..783}.md) count_equals 10 per RESEARCH.md checklist
- [fix] Phase2-Review iter4: Section order | Moved Dependencies section from after Background to after AC Design Constraints per template
- [fix] Phase2-Review iter4: Links | Removed duplicate first Links section (kept end-of-file Links only)
- [fix] Phase2-Review iter4: Review Notes | Added [AC-MTH] category code to pending item
- [fix] Phase2-Review iter5: AC Design Constraints | Moved orphaned Constraint Details subsection back under AC Design Constraints per template
- [fix] Phase2-Review iter5: Mandatory Handoffs | Changed from free text to proper empty table format per template
- [resolved-skipped] Phase2-Uncertain iter3: [AC-MTH] AC#4 Details mismatch — User decision: 現状維持。Execution Logマーカー検証で十分。sub-feature側のConsumers表は各feature /fc時に検証される
- [resolved-skipped] Phase2-Loop iter5: [AC-MTH] AC#5 self-referential — User decision: 現状維持。Execution Log文書化で十分。architecture.md修正はTask#6で実施・検証される
- [resolved-applied] Phase2-Loop iter5: [AC-MTH] AC#1 cannot verify subsystem grouping quality — User decision: AC修正。Glob count_equals → Grep("## Scope Reference") count_equals 8に変更。各implementation sub-feature(F774-F781)のScope Reference存在を検証
- [fix] Phase2-Review iter6: Risks/AC Design Constraints | Added missing --- separator, center-aligned Likelihood/Impact, uppercase values
- [fix] Phase2-Review iter6: Feasibility Assessment | Changed YES to FEASIBLE per template vocabulary
- [fix] Phase2-Review iter6: Line Count Corrections | Fixed line-to-file mapping for 7/9 entries to match architecture.md actual order (4595=SHOP2, 4596=SHOP_COLLECTION, etc.)
- [fix] Phase2-Review iter6: Implementation Contract Phase 3 + AC Coverage | Changed "10-13" to "10" to match AC#1 count_equals and Expected Allocation
- [fix] Phase2-Review iter7: AC Details | Renamed Method→**Test**, Expected→**Expected**, merged Constraint references+Verification+Note into **Rationale** per template
- [fix] Phase2-Review iter1: Tasks table > Task#7 AC# column | Removed spurious AC#1 mapping (AC#1 tests sub-feature files, Task#7 writes Execution Log). Changed '1,4,5' to '4,5'
- [fix] Phase2-Review iter1: AC Details > AC#1 | Added conjunction logic Note: 'Both Grep results are summed; combined count must equal 8 for AC to PASS' per RESEARCH.md Issue 10
- [fix] Phase2-Review iter1: Tasks table > Task#1 AC# column | Removed spurious AC#5 mapping (Task#1 gathers metrics, Task#6/7 apply/document corrections). Changed '1,4,5' to '1,4'
- [resolved-skipped] Phase2-Uncertain iter2: [FMT-LNK] Links section format uses extended format '[Related: F555](archive/feature-555.md) - description' vs template placeholder '[Related](feature-XXX.md)'. User decision: 現状維持
- [resolved-skipped] Phase3-Maintainability iter2: [CON-STA] DRAFT template Predecessor F647 status hardcoded as [WIP] but F647 is currently [PROPOSED]. User decision: 現状維持。/run実行時に[WIP]は正しい値
- [fix] Phase3-Maintainability iter2: Architecture.md Correction Details | Added clarification that {actual}/{delta} placeholders are populated by Task#1 during execution
- [fix] Phase2-Review iter3: AC#3 Matcher/Expected | Added 'Phase 21 Planning' verification to AC#3 (both transition features now verified via AND conjunction)
- [fix] Phase2-Review iter4: Review Notes | Added missing category codes [FMT-LNK] and [CON-STA] to two [pending] entries
- [fix] Phase2-Review iter5: AC#2 Expected | Changed from 'Phase 20' (pre-existing, RESEARCH.md Issue 6) to '| F783 |' (unique marker for last sub-feature)
- [fix] Phase2-Review iter5: AC#2 Details | Updated Test/Expected/Rationale to match new '| F783 |' marker
- [fix] Phase2-Review iter5: AC#3 Rationale | Removed overclaimed 'Both must have corresponding DRAFT files' — AC#3 verifies index registration, DRAFT file creation handled by Task#4
- [resolved-applied] Phase2-Uncertain iter6: [CON-ARC] Architecture.md Sub-Feature Requirements (lines 4629-4637) mandate Planning Feature propagation. User decision: DRAFTテンプレートBackground内にHTML commentで参照ノート追加。/fc時に負債解消・等価性検証・負債ゼロ要件を反映
- [fix] PostLoop-UserFix iter7: DRAFT File Template > Background | Added architecture.md:4629-4637 Sub-Feature Requirements reference note as HTML comment
- [fix] Phase2-Review iter6: AC#3 Matcher | Changed from 'matches' (regex) to 'contains' (literal) per RESEARCH.md Issue 12
- [fix] Phase2-Review iter6: Philosophy line reference | Changed 'line 4509+' to 'line 4549+' in Background and DRAFT template (4509 was testing SKILL notes, 4549 is Phase 20 heading)
- [fix] Phase2-Review iter1: AC#2 | Changed from contains '| F783 |' (last-only) to count_equals 10 with Grep("F77[4-9]|F78[0-3]") to verify all 10 sub-features registered
- [fix] Phase2-Review iter1: AC#6 | Added new AC#6 (Next Feature number incremented to 784) to close gap in pipeline continuity verification. Updated Task#5, Philosophy Derivation, Goal Coverage, AC Coverage
- [resolved-skipped] Phase2-Uncertain iter1: [FMT-SYN] AC#1 Method uses non-standard '+' concatenation syntax. User decision: 現状維持。AC Detailsで結合ロジック文書化済み
- [resolved-skipped] Phase2-Uncertain iter1: [AC-MTH] AC#1 Grep("## Scope Reference") verifies heading existence but not content quality. User decision: 現状維持。DRAFTテンプレートがSource Filesテーブルを強制、存在確認で十分
- [fix] Phase2-Review iter2: Philosophy Derivation | Removed AC#1 from 'clear phase boundaries' row (AC#1 counts headings, doesn't verify boundary clarity). AC#4 remains as sole coverage for this claim
- [fix] Phase2-Review iter2: Review Notes | Added AC count justification: 6 ACs (exceeds 3-5 guideline) justified by AC#6 addition to close pipeline continuity gap — 5 Goal items require distinct verification, AC#6 prevents conflicting ID allocation
- [resolved-applied] Phase2-Review iter2: [FMT-TMP] Review Notes unresolved [pending] items (FMT-SYN, AC-MTH) resolved by user decisions in POST-LOOP
- [fix] Phase2-Review iter3: Tasks table > Task#4 AC# column | Changed '1,2' to '1' (Task#4 creates DRAFT files, AC#2 verifies index registration which is Task#5's responsibility)
- [resolved-applied] Phase2-Uncertain iter4: [AC-COV] Transition feature DRAFT files (F782/F783) have no file existence verification. User decision: AC#3にGlob検証追加。ファイル存在(Glob count_equals 2) + インデックス登録(Grep contains)の両方を検証
- [fix] PostLoop-UserFix iter4: AC#3 | Extended AC#3 with Glob("pm/features/feature-78[23].md") count_equals 2 to verify transition feature DRAFT file existence alongside index registration
- [resolved-applied] Phase2-Uncertain iter5: [DOC-INC] AC#4 Details lists 5 external callers but Dependencies:Consumers lists 6. User decision: 追加パッチverup.ERBをAC#4 Detailsに追加
- [fix] PostLoop-UserFix iter5: AC#4 Details | Added 追加パッチverup.ERB to external callers list (5→6, matching Dependencies:Consumers table)

---

## Links

[Related: F555](archive/feature-555.md) - Phase 19 Planning (pattern precedent)
[Predecessor: F703](feature-703.md) - Post-Phase Review Phase 19 Redux (CANCELLED)
[Related: F646](archive/feature-646.md) - Post-Phase Review Phase 19 (Original)
[Related: F541](archive/feature-541.md) - Phase 18 Planning (pattern precedent)
[Related: F516](archive/feature-516.md) - Phase 17 Planning (pipeline continuity chain)
[Related: F503](archive/feature-503.md) - Phase 16 Planning (pipeline continuity chain)
[Related: F486](archive/feature-486.md) - Phase 15 Planning (pipeline continuity chain)
[Related: F471](archive/feature-471.md) - Phase 14 Planning (pipeline continuity chain)
[Unrelated: F706](feature-706.md) - KojoComparer Full Equivalence (Phase 19 quality)
