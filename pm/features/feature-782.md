# Feature 782: Post-Phase Review Phase 20

## Status: [DONE]
<!-- fl-reviewed: 2026-02-22T05:32:36Z -->

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

## Review Context

### Origin
| Field | Value |
|-------|-------|
| Parent Feature | F786, F780, F797 |
| Discovery Point | Post-Phase Review (Phase 20 completion gate) |
| Timestamp | 2026-02-11 |

### Identified Gap
Phase 20 sub-features completed by ERB module boundary, deferring 4 cross-cutting concerns: (1) GetCFlag/SetCFlag/GetTalent helper duplication across 3 State classes (BodySettings, PregnancySettings, WeatherSettings), (2) NtrSetStayoutMaximum delegate-based pattern (legacy Func/Action params instead of IVariableStore), (3) BodySettings SRP violation (11 methods/6 concerns), (4) N+4 --unit/--flow deprecation obligation.

### Review Evidence
| Gap# | Gap Source | Derived Task | Comparison Result | DEFER Reason |
|:----:|-----------|--------------|-------------------|--------------|
| 1 | F786 Review Context | "Remove --unit and --flow flags from testing infrastructure" | "--unit blocked by kojo pipeline; --flow removable from active files" | "--unit requires C# migration complete" |
| 2 | F780 Review Context | "Extract GeneticsService from BodySettings" | "BodySettings 11 methods / 6 concerns violates SRP" | "DRAFT creation only in F782; full extraction requires separate feature" |
| 3 | F797 Review Context | "Migrate NtrSetStayoutMaximum delegates to IVariableStore" | "Identical pattern to F797 UterusVolumeInit" | "N/A — actionable in F782" |
| 4 | F797 Review Context | "Extract shared GetCFlag/SetCFlag/GetTalent helpers" | "3-way copy (GetCFlag/SetCFlag) + 2-way copy (GetTalent)" | "N/A — actionable in F782" |

### Files Involved
| File | Relevance |
|------|-----------|
| Era.Core/State/BodySettings.cs | GetCFlag/SetCFlag/GetTalent private helpers (lines 507-514); SRP violation (11 methods) |
| Era.Core/State/PregnancySettings.cs | GetCFlag/SetCFlag/GetTalent private helpers (lines 101-108) |
| Era.Core/State/WeatherSettings.cs | GetCFlag/SetCFlag private helpers only (lines 110-114); no GetTalent |
| Era.Core/State/NtrInitialization.cs | Delegate-based SetStayoutMaximum (lines 47-51) |
| Era.Core/Interfaces/INtrInitializer.cs | Func/Action delegate parameters (lines 14-15) |
| Era.Core/Common/GameInitialization.cs | NTRSetStayoutMaximum delegate passthrough caller |
| .claude/skills/testing/SKILL.md | --flow reference in frontmatter and AC Types table |
| .claude/skills/engine-dev/SKILL.md | --flow reference in CLI Options |
| .claude/skills/run-workflow/SKILL.md | --flow reference in erb row |
| .claude/skills/erb-syntax/SKILL.md | --flow reference in step 5 |
| .claude/commands/kojo-init.md | --flow reference in AC row |

### Parent Review Observations
Phase 20 sub-features (F774-F781, F788-F797) organized by ERB module boundary. Cross-cutting utility patterns deferred to post-phase review checkpoint (F782) per architecture design. N+4 --unit removal blocked by kojo testing pipeline dependency (9+ active skill files, 100+ kojo test JSON). All 19 predecessor features confirmed [DONE].

---

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning, with zero technical debt carried forward. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. SSOT: `designs/full-csharp-architecture.md` Phase 20 section defines the scope; `designs/phases/phase-20-27-game-systems.md` specifies Phase 4 Design Requirements including debt-zero AC and equivalence test obligations. F647 decomposed Phase 20 into actionable sub-features (F774-F781).

### Problem (Current Issue)

Phase 20 (Equipment & Shop Systems) is functionally complete -- all 19 predecessor features (F647, F774-F781, F786, F788-F791, F793-F797) are [DONE] -- but the migration strategy organized sub-features by ERB module rather than by utility function dependency. Because of this, cross-cutting concerns were deferred to post-phase review: (1) GetCFlag/SetCFlag private helpers are duplicated identically across BodySettings.cs (lines 507-511), PregnancySettings.cs (lines 101-105), and WeatherSettings.cs (lines 110-114); additionally GetTalent is duplicated across BodySettings.cs (line 513) and PregnancySettings.cs (line 107) -- WeatherSettings does NOT have GetTalent; (2) BodySettings.cs at 1149 lines / 11 methods / 6 concerns violates SRP due to F780 migrating genetics, P growth, and hair change into the existing class; (3) NtrInitialization.cs (line 47-51) and INtrInitializer.cs (line 14-15) use legacy Func/Action delegate parameters instead of IVariableStore; (4) the N+4 obligation from F786 to remove --unit/--flow flags is blocked because --unit is foundational to kojo testing infrastructure (9+ active skill files, 100+ test JSON files). Additionally, Phase 20 Status in architecture.md remains WIP, and OrgasmProcessor.cs (Era.Core/Training/OrgasmProcessor.cs:197) TODO references "Phase 20-21" but the architecture assigns it to Phase 25-26.

### Goal (What to Achieve)

1. Resolve actionable Phase 20 deferred obligations: extract duplicated GetCFlag/SetCFlag/GetTalent helpers into shared IVariableStore extension methods, migrate NtrSetStayoutMaximum from delegate parameters to IVariableStore injection, and fix the OrgasmProcessor TODO phase reference.
2. Assess and document N+4 --unit deprecation as NOT_FEASIBLE with a concrete new trigger condition; remove --flow references from active skill files where already archived.
3. Create a [DRAFT] feature for BodySettings GeneticsService extraction (SRP remediation) and Shop-layer IVariableStore helper deduplication.
4. Update Phase 20 Status from WIP to DONE in architecture.md.
5. Verify zero Phase 20-scoped technical debt (TODO/FIXME/HACK) remains in Era.Core/Shop and Era.Core/State (both already return 0; OrgasmProcessor.cs TODO is in Era.Core/Training/ and is Phase 25-26 scope, excluded).

<!-- Sub-Feature Requirements (architecture.md:4629-4637): /fc時に以下を反映すること
  1. Philosophy継承: Phase 20: Equipment & Shop Systems
  2. Tasks: 負債解消 (TODO/FIXME/HACKコメント削除タスクを含む)
  3. Tasks: 等価性検証 (legacy実装との等価性テストを含む)
  4. AC: 負債ゼロ (技術負債ゼロを検証するACを含む)
-->

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does Phase 20 carry 4 categories of deferred technical debt? | Because cross-cutting utility functions (helpers, delegates, SRP) were deferred to post-phase review instead of being addressed during feature implementation | pm/features/feature-782.md Review Context sections |
| 2 | Why were these cross-cutting concerns deferred? | Because Phase 20 sub-features were decomposed by ERB module boundary (Shop, Collection, Items, Body Settings), not by utility function dependency | docs/architecture/phases/phase-20-27-game-systems.md:40-47 |
| 3 | Why were sub-features organized by ERB module? | Because the migration strategy mirrors ERB file structure to maintain traceability between source and migrated code | docs/architecture/phases/phase-20-27-game-systems.md:92-105 |
| 4 | Why does mirroring ERB structure cause utility duplication? | Because utility patterns like GetCFlag/SetCFlag cross multiple ERB modules but are private to each migrated class, and refactoring was out of scope for migration features | Era.Core/State/BodySettings.cs:507-514 (GetCFlag+SetCFlag+GetTalent), PregnancySettings.cs:101-108 (GetCFlag+SetCFlag+GetTalent), WeatherSettings.cs:110-114 (GetCFlag+SetCFlag only) |
| 5 | Why (Root)? | The architecture design chose "migration first, refactor later" as the standard pattern, creating an intentional post-phase review checkpoint (F782) to consolidate cross-cutting debt | docs/architecture/phases/phase-20-27-game-systems.md:99 (debt-zero AC requirement) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Phase 20 Status remains WIP despite all sub-features being [DONE] | Post-phase review (F782) has not yet executed to consolidate deferred obligations and close the phase |
| Where | feature-782.md Review Context lists 4 deferred obligations | Phase decomposition by ERB module boundary deferred cross-cutting refactoring to post-phase checkpoint |
| Fix | Manually update Phase Status to DONE | Execute F782 to resolve actionable debt, create DRAFT features for larger refactoring, and formally close Phase 20 |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Phase 20 Planning (parent; decomposed into F774-F781) |
| F774 | [DONE] | Shop Core (originated 11 NotImplementedException stubs tracked in Mandatory Handoffs) |
| F780 | [DONE] | Genetics & Growth (originated BodySettings SRP deferred obligation) |
| F786 | [DONE] | Test Infrastructure (originated N+4 --unit/--flow deprecation deferral) |
| F795 | [DONE] | External ERB Function Migration (resolved 3 CollectionTracker stubs) |
| F797 | [DONE] | IVariableStore Migration (originated delegate migration + helper duplication deferrals) |
| F783 | [DRAFT] | Phase 21 Planning (successor, blocked by F782) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| All predecessors [DONE] | FEASIBLE | F774-F781, F788-F797 all verified [DONE]; F780 status was stale in feature file (fixed) |
| Helper duplication extraction | FEASIBLE | GetCFlag/SetCFlag: 3-way copy (6 methods); GetTalent: 2-way copy (BodySettings+PregnancySettings only; WeatherSettings has no GetTalent); mechanical extraction |
| NtrSetStayoutMaximum delegate migration | FEASIBLE | Same proven pattern as F797 (UterusVolumeInit/TemperatureToleranceInit); single caller |
| BodySettings SRP extraction | FEASIBLE | Creates DRAFT feature only; does not execute extraction in F782 |
| N+4 --unit complete removal | NOT_FEASIBLE | 9+ active skill files, 100+ kojo test JSON files, 3 engine.Tests files depend on --unit |
| N+4 --flow doc removal from active files | FEASIBLE | --flow already archived in testing/_archived/FLOW.md; active references are doc-only |
| Phase 20 Status update | FEASIBLE | Simple documentation update in phase-20-27-game-systems.md |
| OrgasmProcessor TODO fix | FEASIBLE | Comment text correction only ("Phase 20-21" to "Phase 25-26") |
| NotImplementedException stub resolution | NOT_FEASIBLE | 11 stubs reference Phase 14/21/28-34 functions; cross-phase boundaries |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core/State/ | MEDIUM | Helper extraction changes 3 files (BodySettings, PregnancySettings, WeatherSettings); removes 8 private helpers total (6 GetCFlag/SetCFlag + 2 GetTalent); adds shared extension methods |
| Era.Core/State/NtrInitialization.cs | LOW | Delegate-to-IVariableStore migration; contained to single file + interface |
| Era.Core/Interfaces/INtrInitializer.cs | LOW | Interface signature change (remove Func/Action params); single implementation |
| Era.Core/Common/GameInitialization.cs | LOW | Remove delegate passthrough in NTRSetStayoutMaximum; single caller |
| .claude/skills/ (5+ files) | LOW | Remove --flow references from active documentation |
| docs/architecture/phases/ | LOW | Phase 20 Status update from WIP to DONE |
| pm/ (new DRAFT) | LOW | New feature file for BodySettings GeneticsService extraction |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| --unit removal blocked by kojo testing pipeline | .claude/skills/testing/KOJO.md active usage; 100+ test/ac/kojo/ JSON files | N+4 --unit removal must be further deferred; assess and document new trigger condition |
| NtrInitialization delegate migration requires INtrInitializer interface change | Era.Core/Interfaces/INtrInitializer.cs:14-15 | Breaking change to interface; must update all consumers |
| NotImplementedException stubs are cross-phase boundaries | Era.Core/Shop/ShopSystem.cs:381-439, ShopDisplay.cs:413-437 | Cannot resolve within F782; stubs tracked with destination phases |
| OrgasmProcessor TODO is Phase 25-26 scope | phase-20-27-game-systems.md Phase 25-26 section | Can only fix comment text, not implement the feature |
| BodySettings SRP creates DRAFT only | feature-782.md Review Context (F780 deferral) | F782 does NOT execute the extraction; creates tracking feature |
| TreatWarningsAsErrors enabled | Directory.Build.props | All refactoring must compile cleanly without warnings |
| NtrInitialization delegate migration requires updating existing tests | engine.Tests/Tests/NtrInitializationTests.cs: 5 test methods (SetStayoutMaximum_SetsHighNibble_PreservesLowNibble, SetStayoutMaximum_MaxDays7_SetsCorrectBits, SetStayoutMaximum_DaysOver15_MasksTo4Bits, SetStayoutMaximum_NullGetCflag_ThrowsArgumentNullException, SetStayoutMaximum_NullSetCflag_ThrowsArgumentNullException) all use old Func/Action delegate API | Delegate migration must update all 5 test methods to IVariableStore-based API; test files are immutable-protected (pre-commit hook applies) |
| NtrInitialization constructor change breaks GameInitializationTests | engine.Tests/Tests/GameInitializationTests.cs: CreateGameInitialization() (line ~76) calls `new NtrInitialization()` which will fail after T9 adds IVariableStore constructor parameter | T12 must update GameInitializationTests.CreateGameInitialization() to pass mock IVariableStore to NtrInitialization constructor |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| N+4 deferral chain grows indefinitely | MEDIUM | LOW | Document concrete trigger condition: "C# migration functionally complete (kojo no longer requires ERB test runner)" |
| GetCFlag helper extraction changes error semantics | LOW | LOW | Pattern is uniform across all 3 files; extraction preserves identical behavior |
| NtrSetStayoutMaximum migration introduces regression | LOW | LOW | Single caller (GameInitialization.cs:365); existing test coverage for NtrInitialization |
| AC scopes cross-phase stubs as Phase 20 debt | HIGH | HIGH | Distinguish internal Phase 20 debt from cross-phase boundary stubs in AC design; stubs have known destinations |
| BodySettings grows further in Phase 22+ without DRAFT | LOW | MEDIUM | Create DRAFT extraction feature in F782 to track SRP remediation |
| Stale feature-782.md dependency status causes FL gate rejection | HIGH | LOW | Fixed: F780 status corrected from [PROPOSED] to [DONE] |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| NotImplementedException count in Era.Core/Shop/ | `rg "NotImplementedException" Era.Core/Shop/ -c` | 12 | Cross-phase stubs; NOT Phase 20 debt (ShopSystem 9 + ShopDisplay 3) |
| TODO/FIXME/HACK in Era.Core/State/ | `rg "TODO\|FIXME\|HACK" Era.Core/State/ -c` | 0 | Era.Core/State/ is already clean; no TODOs present |
| TODO/FIXME/HACK in Era.Core/Training/ | `rg "TODO\|FIXME\|HACK" Era.Core/Training/ -c` | 1 | OrgasmProcessor.cs:197 (Phase 25-26 scope; excluded from C1 debt-zero scan) |
| Private GetCFlag/SetCFlag duplicates | `rg "private.*GetCFlag\|private.*SetCFlag" Era.Core/State/ -c` | 6 | 3 files, 6 helper methods (GetCFlag+SetCFlag only; GetTalent counted separately) |
| Private GetTalent duplicates | `rg "private.*GetTalent" Era.Core/State/ -c` | 2 | BodySettings.cs:513 + PregnancySettings.cs:107; WeatherSettings does NOT have GetTalent |
| Private GetCFlag/SetCFlag/GetTalent in Era.Core/Shop/ | `rg "private.*(GetCFlag\|SetCFlag\|GetTalent)" Era.Core/Shop/ -c` | 5 | Shop-layer helpers excluded from F782 scope (C5); must remain unchanged |
| NotImplementedException co-located annotation count in Era.Core/Shop/ | `rg "NotImplementedException.*(Phase [0-9]+\|See F[0-9]+)" Era.Core/Shop/ -c` | 11 | ShopSystem 8 + ShopDisplay 3; validates AC#4b expected value |

**Baseline File**: `.tmp/baseline-782.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | architecture.md requires debt-zero AC | phase-20-27-game-systems.md Phase 4 Design Requirements | Must include AC verifying no TODO/FIXME/HACK in Era.Core/Shop/ and Era.Core/State/; Era.Core/State/ already returns 0 (clean); OrgasmProcessor TODO is in Era.Core/Training/ and is Phase 25-26 scope, excluded from scan |
| C2 | architecture.md requires equivalence test verification | phase-20-27-game-systems.md Phase 4 Design Requirements | Must verify equivalence tests exist for Phase 20 deliverables |
| C3 | N+4 --unit is conditional on kojo migration | phase-5-19-content-migration.md:2647 | AC should verify assessment and deferral documentation, NOT --unit removal |
| C4 | --flow already archived | testing/_archived/FLOW.md | AC can require --flow removal from active skill files |
| C5 | Helper duplication is asymmetric: GetCFlag/SetCFlag are 3-way copy (all 3 State files); GetTalent is 2-way copy (BodySettings + PregnancySettings only; WeatherSettings does NOT have GetTalent). ItemPurchase.cs uses raw int index params (GetTalent(int characterIndex, int talentIndex), GetCFlag(int characterIndex, int cflagIndex), SetCFlag(int characterIndex, int cflagIndex, int value)) and is excluded; ShopSystem.cs and ShopDisplay.cs use strongly-typed params (CharacterId, TalentIndex, CharacterFlagIndex) matching IVariableStoreExtensions signatures and are deferred to separate feature | BodySettings.cs:507-514, PregnancySettings.cs:101-108, WeatherSettings.cs:110-114 (State layer extraction scope); ItemPurchase.cs:300,348,354 (raw int params, excluded); ShopSystem.cs:443,449 & ShopDisplay.cs:464,466 (strongly-typed params, deferred to future Shop-layer deduplication feature) | AC must verify 6 GetCFlag/SetCFlag private copies (3 files × 2) and 2 GetTalent private copies (2 files) replaced by shared implementation in State layer only; WeatherSettings scope is GetCFlag+SetCFlag only |
| C6 | NtrSetStayoutMaximum has single caller | GameInitialization.cs:359-366 | AC must verify Func/Action removed from INtrInitializer interface |
| C7 | 11 NotImplementedException stubs are cross-phase | ShopSystem.cs:381-439, ShopDisplay.cs:413-437 | AC must NOT require stub count = 0; stubs are tracked with destination phases |
| C8 | BodySettings SRP creates DRAFT only | feature-782.md Review Context (F780 deferral) | AC verifies DRAFT file creation + index registration, NOT extraction execution |
| C9 | OrgasmProcessor TODO phase reference is wrong | OrgasmProcessor.cs:197 ("Phase 20-21" vs architecture Phase 25-26) | AC must verify corrected comment text |
| C10 | Phase 20 Status must update to DONE | phase-20-27-game-systems.md:9 | AC must verify Phase Status changed from WIP to DONE |

### Constraint Details

**C1: Debt-Zero AC**
- **Source**: phase-20-27-game-systems.md Phase 4 Design Requirements (architecture.md:4629-4637)
- **Verification**: Grep for TODO/FIXME/HACK in Era.Core/Shop/ and Era.Core/State/; both directories must return 0. Era.Core/State/ is already clean (0 results). OrgasmProcessor.cs TODO is in Era.Core/Training/ (not Era.Core/State/) and is Phase 25-26 scope — excluded from debt-zero scan
- **AC Impact**: AC must scope the debt scan to Era.Core/Shop/ and Era.Core/State/ only; Era.Core/Training/ is out of Phase 20 scope

**C2: Equivalence Test Verification**
- **Source**: phase-20-27-game-systems.md Phase 4 Design Requirements
- **Verification**: C2 enforced by AC#12b (NtrInitializationTests maintain >= 5 test methods after rewrite, preserving equivalence) and AC#13 (IVariableStoreExtensions tests cover all methods including error paths)
- **AC Impact**: AC#12b ensures NtrInitialization delegation migration maintains test count and behavioral edge case coverage; AC#13 ensures extracted helper tests verify identical behavior

**C3: N+4 --unit Assessment**
- **Source**: phase-5-19-content-migration.md:2647 (DEFERRED to F782)
- **Verification**: Check that assessment is documented with concrete trigger condition for future removal
- **AC Impact**: AC verifies assessment documentation exists with new trigger condition; does NOT require --unit removal

**C4: --flow Active File Removal**
- **Source**: testing/_archived/FLOW.md (--flow already archived); active references in 5+ skill files
- **Verification**: Grep for --flow in non-archived .claude/skills/ and .claude/commands/ files
- **AC Impact**: AC can require zero --flow references in active (non-archived) skill/command files

**C5: Helper Deduplication**
- **Source**: BodySettings.cs:507-514, PregnancySettings.cs:101-108, WeatherSettings.cs:110-114; ItemPurchase.cs:300,348,354 (excluded — int index params differ from strongly-typed enum params used in State layer)
- **Verification**: Grep for `private.*GetCFlag|private.*SetCFlag` in Era.Core/State/ (must return 0 after extraction — 6 copies removed); grep for `private.*GetTalent` in Era.Core/State/ (must return 0 after extraction — 2 copies removed). WeatherSettings scope is GetCFlag+SetCFlag ONLY (no GetTalent to remove there)
- **AC Impact**: AC must verify 6 private GetCFlag/SetCFlag copies and 2 private GetTalent copies all replaced by shared implementation; scope excludes Shop-layer helpers due to signature mismatch

**C6: Delegate Migration**
- **Source**: INtrInitializer.cs:14-15 (Func/Action parameters); NtrInitialization.cs:47-51 (implementation)
- **Verification**: Grep for Func<int.*int.*int> in INtrInitializer.cs
- **AC Impact**: AC must verify Func/Action removed from interface and implementation

**C7: Cross-Phase Stubs**
- **Source**: F774 Mandatory Handoffs; ShopSystem.cs:381-439, ShopDisplay.cs:413-437
- **Verification**: Count NotImplementedException in Era.Core/Shop/; verify count is tracked, not zero
- **AC Impact**: AC tracks stub count for baseline; does NOT require zero

**C8: BodySettings DRAFT Feature**
- **Source**: F780 deferred obligation (Review Context)
- **Verification**: Glob for new feature file; Grep index-features.md for registration
- **AC Impact**: AC verifies both file creation AND index-features.md registration per DRAFT Creation Checklist

**C9: OrgasmProcessor TODO Fix**
- **Source**: OrgasmProcessor.cs:197 says "Phase 20-21" but architecture says Phase 25-26
- **Verification**: Grep for "Phase 20-21" in OrgasmProcessor.cs (should be zero after fix)
- **AC Impact**: AC verifies corrected phase reference

**C10: Phase Status Update**
- **Source**: phase-20-27-game-systems.md:9 (currently "WIP")
- **Verification**: Grep for Phase Status in Phase 20 section
- **AC Impact**: AC verifies Phase 20 Status reads "DONE"

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (decomposed this feature) |
| Predecessor | F774 | [DONE] | Shop Core (originated 11 NotImplementedException cross-phase stubs) |
| Predecessor | F775 | [DONE] | Collection |
| Predecessor | F776 | [DONE] | Items |
| Predecessor | F777 | [DONE] | Customization |
| Predecessor | F778 | [DONE] | Body Initialization |
| Predecessor | F779 | [DONE] | Body Settings UI |
| Predecessor | F780 | [DONE] | Genetics & Growth (originated BodySettings SRP deferral) |
| Predecessor | F781 | [DONE] | Visitor Settings |
| Predecessor | F786 | [DONE] | Test Infrastructure (originated N+4 --unit/--flow deferral) |
| Predecessor | F788 | [DONE] | IConsoleOutput Phase 20 Extensions |
| Predecessor | F789 | [DONE] | IVariableStore Phase 20 Extensions |
| Predecessor | F790 | [DONE] | Engine Data Access Layer |
| Predecessor | F791 | [DONE] | Engine State Transitions & Entry Point Routing |
| Predecessor | F793 | [DONE] | GameStateImpl Engine-Side Delegation (Mandatory Handoff origin) |
| Predecessor | F794 | [DONE] | Shared Body Option Validation Abstraction |
| Predecessor | F795 | [DONE] | External ERB Function Migration (resolved 3 CollectionTracker stubs) |
| Predecessor | F796 | [DONE] | BodyDetailInit IVariableStore Migration |
| Predecessor | F797 | [DONE] | UterusVolumeInit/TemperatureToleranceInit IVariableStore Migration (originated delegate + duplication deferrals) |
| Successor | F783 | [DRAFT] | Phase 21 Planning |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} → This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This → F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
| Related | - | None | Reference only. No blocking, no status effect. |

Blocking Logic (FL Phase 0):
- Only Type=Predecessor triggers [BLOCKED] status
- Successor/Related do NOT block

Example:
| Predecessor | F540 | [DONE] | Era.Core setup required |
| Successor | F567 | [BLOCKED] | F567 depends on this feature |
| Related | F100 | [DONE] | Reference implementation |
-->

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "zero technical debt carried forward" | Verify no technical debt remains in Phase 20 scope: no TODO/FIXME/HACK markers (Era.Core/Shop/, Era.Core/State/); stale --flow references removed from active documentation; shared extension class exists at designed path; call sites adopted extension method syntax | AC#1, AC#2, AC#3, AC#3b, AC#3c, AC#3d, AC#3e, AC#8, AC#9, AC#10, AC#14, AC#20, AC#21, AC#22 |
| "continuous development pipeline" | Phase 20 Status updated to DONE so Phase 21 can proceed | AC#7 |
| "clear phase boundaries" | Cross-phase stubs tracked (not falsely zeroed); stubs have destination phase annotations; stale stub handoff verified in F783; OrgasmProcessor TODO corrected to proper phase | AC#4, AC#4b, AC#4c, AC#6, AC#6b |
| "documented transition points" | BodySettings DRAFT created and registered; N+4 obligation tracked in F783 with concrete trigger condition; Shop-layer dedup tracked in F800 | AC#5a, AC#5b, AC#15, AC#15b, AC#16 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | Zero TODO/FIXME/HACK in Era.Core/Shop/ and Era.Core/State/ | code | Grep(Era.Core/Shop/ Era.Core/State/) | count_equals | "TODO|FIXME|HACK" = 0 | [x] |
| 2 | Private GetCFlag/SetCFlag eliminated from Era.Core/State/ | code | Grep(Era.Core/State/) | count_equals | "private.*GetCFlag|private.*SetCFlag" = 0 | [x] |
| 3 | --flow removed from active testing SKILL.md (non-comment lines) | code | Grep(.claude/skills/testing/SKILL.md) | not_matches | "^[^<].*--flow" | [x] |
| 3b | --flow removed from active engine-dev SKILL.md | code | Grep(.claude/skills/engine-dev/SKILL.md) | not_contains | "--flow" | [x] |
| 3c | --flow removed from active run-workflow SKILL.md | code | Grep(.claude/skills/run-workflow/SKILL.md) | not_contains | "--flow" | [x] |
| 3d | --flow removed from active erb-syntax SKILL.md | code | Grep(.claude/skills/erb-syntax/SKILL.md) | not_contains | "--flow" | [x] |
| 3e | --flow removed from active kojo-init.md | code | Grep(.claude/commands/kojo-init.md) | not_contains | "--flow" | [x] |
| 4 | Cross-phase NotImplementedException stubs tracked (not zero) | code | Grep(Era.Core/Shop/) | gte | "NotImplementedException" >= 1 | [x] |
| 4b | Cross-phase stubs have destination phase annotations | code | Grep(Era.Core/Shop/) | gte | "NotImplementedException.*(Phase [0-9]+\|See F[0-9]+)" >= 11 | [x] |
| 4c | F783 contains stale stub annotation update obligation | file | Grep(pm/features/feature-783.md) | matches | "Stale Stub Annotations.*See Phase 20" | [x] |
| 5a | BodySettings DRAFT feature file created | file | Grep(pm/features/feature-800.md) | matches | "BodySettings.*GeneticsService\|GeneticsService.*extraction" | [x] |
| 5b | BodySettings DRAFT registered in index-features.md | file | Grep(pm/index-features.md) | matches | "GeneticsService|BodySettings.*SRP|BodySettings.*extraction" | [x] |
| 6 | OrgasmProcessor TODO phase reference corrected (old text removed) | code | Grep(Era.Core/Training/OrgasmProcessor.cs) | not_contains | "Phase 20-21" | [x] |
| 6b | OrgasmProcessor TODO phase reference corrected (new text present) | code | Grep(Era.Core/Training/OrgasmProcessor.cs) | matches | "Phase 25-26" | [x] |
| 7 | Phase 20 Status updated to DONE | file | Grep(docs/architecture/phases/phase-20-27-game-systems.md) | matches | "Phase Status.*DONE" | [x] |
| 8 | Private GetTalent eliminated from Era.Core/State/ | code | Grep(Era.Core/State/) | count_equals | "private.*GetTalent" = 0 | [x] |
| 9 | Func/Action delegates removed from INtrInitializer interface | code | Grep(Era.Core/Interfaces/INtrInitializer.cs) | not_matches | "Func<.*int.*int.*int>|Action<.*int.*int.*int>" | [x] |
| 10 | NtrInitialization uses IVariableStore (not delegate params) | code | Grep(Era.Core/State/NtrInitialization.cs) | matches | "private readonly IVariableStore" | [x] |
| 10b | NtrInitialization SetStayoutMaximum routes through IVariableStore extension methods | code | Grep(Era.Core/State/NtrInitialization.cs) | matches | "_variables\\.GetCFlag\|_variables\\.SetCFlag" | [x] |
| 11 | Build succeeds after all changes | build | dotnet build | succeeds | - | [x] |
| 12 | All existing tests pass after refactoring | test | dotnet test | succeeds | - | [x] |
| 12b | NtrInitializationTests maintain >= 5 test methods after rewrite | code | Grep(engine.Tests/Tests/NtrInitializationTests.cs) | gte | "\\[Test\\]\|\\[Fact\\]\|\\[TestCase\\]" >= 5 | [x] |
| 12c | HeadlessIntegrationTests StubInterface_NTRSetStayoutMaximum_Exists method survives rewrite | code | Grep(engine.Tests/Tests/HeadlessIntegrationTests.cs) | matches | "StubInterface_NTRSetStayoutMaximum_Exists" | [x] |
| 12d | NtrInitializationTests preserve bit-packing behavioral assertions | code | Grep(engine.Tests/Tests/NtrInitializationTests.cs) | matches | "(0b11110000\|0b00001111\|shiftedDays\|HighNibble\|LowNibble\|MaxDays\|Masks)" | [x] |
| 12e | HeadlessIntegrationTests StubInterface method references new API | code | Grep(engine.Tests/Tests/HeadlessIntegrationTests.cs) | matches | "new NtrInitialization\\([^)]" | [x] |
| 12f | NtrInitializationTests preserve null-guard ArgumentNullException test | code | Grep(engine.Tests/Tests/NtrInitializationTests.cs) | matches | "Throws.*ArgumentNullException" | [x] |
| 12g | NtrInitializationTests verify null-guard paramName "variables" | code | Grep(engine.Tests/Tests/NtrInitializationTests.cs) | matches | "\"variables\"" | [x] |
| 13 | IVariableStoreExtensions unit tests cover all methods | code | Grep(Era.Core.Tests/Interfaces/IVariableStoreExtensionsTests.cs) | gte | "\\[Test\\]\|\\[Fact\\]\|\\[TestCase\\]" >= 5 | [x] |
| 13b | IVariableStoreExtensions tests include error/default-0 path | code | Grep(Era.Core.Tests/Interfaces/IVariableStoreExtensionsTests.cs) | matches | "default.*0\|returns.*0\|_ => 0\|Error\|error" | [x] |
| 13c | IVariableStoreExtensions tests verify GetTalent overload | code | Grep(Era.Core.Tests/Interfaces/IVariableStoreExtensionsTests.cs) | matches | "GetTalent" | [x] |
| 14 | GameInitialization delegate params removed from NTRSetStayoutMaximum | code | Grep(Era.Core/Common/GameInitialization.cs) | not_matches | "NTRSetStayoutMaximum.*Func\|NTRSetStayoutMaximum.*Action" | [x] |
| 15 | F783 contains N+4 --unit deprecation obligation with trigger condition | file | Grep(pm/features/feature-783.md) | matches | "N\\+4.*--unit\|NOT_FEASIBLE.*--unit" | [x] |
| 15b | F783 trigger condition specifies concrete kojo dependency | file | Grep(pm/features/feature-783.md) | matches | "(trigger.*condition.*(kojo\|ERB)\|kojo.*ERB\|kojo.*test runner)" | [x] |
| 16 | F800 DRAFT includes Shop-layer IVariableStore dedup scope | file | Grep(pm/features/feature-800.md) | matches | "ShopSystem\|ShopDisplay\|Shop.*IVariableStore\|IVariableStore.*dedup" | [x] |
| 17 | CharacterFlagIndex.StayoutMaximumDays enum value exists with value 22 | code | Grep(Era.Core/Types/CharacterFlagIndex.cs) | matches | "StayoutMaximumDays\\s*=\\s*22" | [x] |
| 18 | Shop-layer private helpers unchanged (C5 exclusion respected) | code | Grep(Era.Core/Shop/) | count_equals | "private.*(GetCFlag\|SetCFlag\|GetTalent)" = 5 | [x] |
| 19 | engine-dev SKILL.md documents IVariableStoreExtensions | file | Grep(.claude/skills/engine-dev/SKILL.md) | matches | "IVariableStoreExtensions" | [x] |
| 20 | IVariableStoreExtensions.cs exists at designed path | code | Grep(Era.Core/Interfaces/IVariableStoreExtensions.cs) | matches | "public static class IVariableStoreExtensions" | [x] |
| 21 | Extension method call-site adoption verified in Era.Core/State/ | code | Grep(Era.Core/State/) | gte | "_variables\\.GetCFlag\|_variables\\.SetCFlag" >= 3 | [x] |
| 22 | GetTalent extension method call-site adoption verified in Era.Core/State/ | code | Grep(Era.Core/State/) | gte | "_variables\\.GetTalent" >= 2 | [x] |

### AC Details

**AC#1: Zero TODO/FIXME/HACK in Era.Core/Shop/ and Era.Core/State/**
- **Test**: `Grep("TODO|FIXME|HACK", path="Era.Core/Shop/")` and `Grep("TODO|FIXME|HACK", path="Era.Core/State/")`
- **Expected**: 0 matches in both directories combined
- **Rationale**: Architecture design requirement (phase-20-27-game-systems.md Phase 4 Design Requirements) mandates debt-zero AC for Phase 20 scope. Era.Core/State/ baseline is already 0. Era.Core/Shop/ baseline is also 0 (NotImplementException stubs are not TODO markers). OrgasmProcessor.cs is in Era.Core/Training/ (Phase 25-26 scope, excluded per C1). [Constraint: C1]

**AC#2: Private GetCFlag/SetCFlag eliminated from Era.Core/State/**
- **Test**: `Grep("private.*GetCFlag|private.*SetCFlag", path="Era.Core/State/")`
- **Expected**: 0 matches (baseline: 6 copies across 3 files)
- **Rationale**: Resolves 3-way code duplication (BodySettings, PregnancySettings, WeatherSettings) by extracting to shared IVariableStore extension methods. [Constraint: C5]

**AC#3: --flow removed from active testing SKILL.md (non-comment lines)**
- **Test**: `Grep("^[^<].*--flow", path=".claude/skills/testing/SKILL.md")`
- **Expected**: No matches on non-comment lines (HTML comment lines with `<!--` are excluded; the archived comment at line 511 is acceptable)
- **Rationale**: --flow is already archived in testing/_archived/FLOW.md. Active references in SKILL.md frontmatter description and AC Types table should be removed. [Constraint: C4]

**AC#3b: --flow removed from active engine-dev SKILL.md**
- **Test**: `Grep("--flow", path=".claude/skills/engine-dev/SKILL.md")`
- **Expected**: 0 matches
- **Rationale**: --flow reference in CLI Options line should be removed since --flow is archived. [Constraint: C4]

**AC#3c: --flow removed from active run-workflow SKILL.md**
- **Test**: `Grep("--flow", path=".claude/skills/run-workflow/SKILL.md")`
- **Expected**: 0 matches
- **Rationale**: --flow reference in erb row should be removed. [Constraint: C4]

**AC#3d: --flow removed from active erb-syntax SKILL.md**
- **Test**: `Grep("--flow", path=".claude/skills/erb-syntax/SKILL.md")`
- **Expected**: 0 matches
- **Rationale**: --flow reference in step 5 should be removed. [Constraint: C4]

**AC#3e: --flow removed from active kojo-init.md**
- **Test**: `Grep("--flow", path=".claude/commands/kojo-init.md")`
- **Expected**: 0 matches
- **Rationale**: --flow reference in AC row should be removed. [Constraint: C4]

**AC#4: Cross-phase NotImplementException stubs tracked (not zero)**
- **Test**: `Grep("NotImplementedException", path="Era.Core/Shop/")`
- **Expected**: Count >= 1 (baseline: 12 across ShopSystem.cs and ShopDisplay.cs)
- **Rationale**: These 11+ stubs reference Phase 14/21/28-34 functions and are cross-phase boundaries. AC verifies they are tracked, NOT that they are zero. [Constraint: C7]

**AC#4b: Cross-phase stubs have destination phase annotations**
- **Test**: `Grep("NotImplementedException.*(Phase [0-9]+|See F[0-9]+)", path="Era.Core/Shop/")`
- **Expected**: Count >= 11 (11 of 12 NotImplementedException occurrences have co-located destination annotations; the 12th is a section comment header at ShopSystem.cs:372 without annotation)
- **Rationale**: Philosophy "clear phase boundaries" requires stubs to have tracked destinations. Pattern co-locates NotImplementedException with destination annotations on the same line, excluding non-stub phase references in comments/interfaces. Stubs use mixed styles: "See Phase 20 sibling feature", "See F778 (Body Settings)", or "(Phase 14/Phase 20)". T1 baseline verification runs this exact grep to confirm actual same-line co-location count before implementation. If count < 11, T1 reports the gap and documents the missing annotation count in Review Notes as a discovered issue; annotation remediation for missing annotations is tracked via F783 (Mandatory Handoffs row 4) as part of the broader stub annotation update obligation. **Note**: 6 of 11 throw stubs say "See Phase 20 sibling feature" which becomes stale after Phase 20→DONE; correct destination requires Phase 21 decomposition (deferred to F783 per Mandatory Handoffs row 4). AC#4b verifies annotations exist (tracked), not that destinations are fully resolved. [Constraint: C7]

**AC#4c: F783 contains stale stub annotation update obligation**
- **Test**: `Grep("Stale Stub Annotations.*See Phase 20", path="pm/features/feature-783.md")`
- **Expected**: Match found on F783 section header (single line: "Stale Stub Annotations: 'See Phase 20 sibling feature' × 6")
- **Rationale**: Mandatory Handoff row 4 defers 6 stale "See Phase 20 sibling feature" annotations to F783. Matcher targets the section header line which co-locates "Stale Stub Annotations" with "See Phase 20" on a single line, avoiding multi-line matching issues. F783 already contains the stale stub annotation section header (written during F782 FL Phase 3 iter8); AC#4c is read-only verification of this specific content. Note: The N+4 --unit trigger condition (verified by AC#15/AC#15b) is a separate obligation written by T24 during /run — T24 is NOT a no-op. [Constraint: C7]

**AC#5a: BodySettings DRAFT feature file created**
- **Test**: `Grep("BodySettings.*GeneticsService|GeneticsService.*extraction", path="pm/features/feature-800.md")`
- **Expected**: Match found confirming feature-800.md contains GeneticsService extraction description
- **Rationale**: F780 deferred obligation requires creating a DRAFT feature for SRP remediation (extracting genetics, P growth, and hair change methods from BodySettings). Matcher targets feature-800.md specifically (not glob) because F780 also contains "GeneticsService" text, which would false-positive a glob-based AC. [Constraint: C8]

**AC#5b: BodySettings DRAFT registered in index-features.md**
- **Test**: `Grep("GeneticsService|BodySettings.*SRP|BodySettings.*extraction", path="pm/index-features.md")`
- **Expected**: Match found confirming registration in Active Features table
- **Rationale**: Per DRAFT Creation Checklist, new feature files must be registered in index-features.md. [Constraint: C8]

**AC#6: OrgasmProcessor TODO phase reference corrected**
- **Test**: `Grep("Phase 20-21", path="Era.Core/Training/OrgasmProcessor.cs")`
- **Expected**: 0 matches (baseline: 1 match at line 197)
- **Rationale**: Architecture assigns this TODO to Phase 25-26, not Phase 20-21. Comment text must be corrected. [Constraint: C9]

**AC#6b: OrgasmProcessor TODO phase reference corrected (new text present)**
- **Test**: `Grep("Phase 25-26", path="Era.Core/Training/OrgasmProcessor.cs")`
- **Expected**: Match found confirming the TODO comment now references Phase 25-26
- **Rationale**: AC#6 verifies the old text "Phase 20-21" is removed but cannot distinguish correction from deletion. Deleting the TODO entirely would pass AC#6 while destroying Phase 25-26 tracking. This AC ensures the corrected text was substituted, not deleted. [Constraint: C9]

**AC#7: Phase 20 Status updated to DONE**
- **Test**: `Grep("Phase Status.*DONE", path="docs/architecture/phases/phase-20-27-game-systems.md")`
- **Expected**: Match found confirming Phase 20 Status is DONE
- **Rationale**: All Phase 20 sub-features are [DONE]; formal status update closes the phase and unblocks Phase 21. [Constraint: C10]

**AC#8: Private GetTalent eliminated from Era.Core/State/**
- **Test**: `Grep("private.*GetTalent", path="Era.Core/State/")`
- **Expected**: 0 matches (baseline: 2 copies in BodySettings.cs and PregnancySettings.cs; WeatherSettings does NOT have GetTalent)
- **Rationale**: Resolves 2-way GetTalent duplication (asymmetric with 3-way GetCFlag/SetCFlag per C5). WeatherSettings is excluded from GetTalent scope. [Constraint: C5]

**AC#9: Func/Action delegates removed from INtrInitializer interface**
- **Test**: `Grep("Func<.*int.*int.*int>|Action<.*int.*int.*int>", path="Era.Core/Interfaces/INtrInitializer.cs")`
- **Expected**: 0 matches (baseline: 2 matches at lines 14-15)
- **Rationale**: NtrSetStayoutMaximum must migrate from legacy Func/Action delegate parameters to IVariableStore-based access. Interface signature becomes `void SetStayoutMaximum(int characterId, int days)` with constructor-injected `_variables`. [Constraint: C6]

**AC#10: NtrInitialization uses IVariableStore (not delegate params)**
- **Test**: `Grep("private readonly IVariableStore", path="Era.Core/State/NtrInitialization.cs")`
- **Expected**: Match found confirming IVariableStore field declaration (constructor injection)
- **Rationale**: Validates that the delegate-to-IVariableStore migration pattern (proven in F797) is applied to NtrSetStayoutMaximum. Strengthened matcher ("private readonly IVariableStore") confirms field declaration, not just a using directive. [Constraint: C6]

**AC#10b: NtrInitialization SetStayoutMaximum routes through IVariableStore extension methods**
- **Test**: `Grep("_variables\\.GetCFlag|_variables\\.SetCFlag", path="Era.Core/State/NtrInitialization.cs")`
- **Expected**: Match found confirming SetStayoutMaximum uses `_variables.GetCFlag(...)` and `_variables.SetCFlag(...)` extension methods
- **Rationale**: AC#10 verifies IVariableStore field declaration but not usage. AC#2 ensures private helpers are removed from State classes. However, a bypass implementation calling `_variables.GetCharacterFlag(...)` directly (skipping the extracted extension methods) would pass AC#10 and AC#2. This AC verifies the delegate migration routes through the IVariableStoreExtensions methods, confirming the extracted helpers are actually consumed by their primary consumer. [Constraint: C6]

**AC#11: Build succeeds after all changes**
- **Test**: `dotnet build` (via WSL)
- **Expected**: Build succeeds with exit code 0 (TreatWarningsAsErrors enabled)
- **Rationale**: All refactoring must compile cleanly. Interface changes to INtrInitializer and helper extraction affect multiple files.

**AC#12: All existing tests pass after refactoring**
- **Test**: `dotnet test` (via WSL)
- **Expected**: All tests pass
- **Rationale**: Ensures helper extraction and delegate migration do not introduce regressions. NtrInitializationTests must be updated to match new IVariableStore API.

**AC#12b: NtrInitializationTests maintain >= 5 test methods after rewrite**
- **Test**: `Grep("[Test]|[Fact]|[TestCase]", path="engine.Tests/Tests/NtrInitializationTests.cs")`
- **Expected**: Count >= 5 (same as pre-rewrite count of 5 test methods)
- **Rationale**: T11 rewrites 5 NtrInitializationTests from Func/Action delegate API to IVariableStore-based API. This AC ensures no test methods are dropped during rewrite. The 3 behavioral tests (high nibble isolation, max days 7, 4-bit masking) must be preserved as equivalence tests for the delegate migration. The 2 null-guard tests change from null getCflag/setCflag to null IVariableStore. C2 (equivalence test mandate) is enforced by maintaining the same test count and behavioral coverage. [Constraint: C2]

**AC#12c: HeadlessIntegrationTests StubInterface_NTRSetStayoutMaximum_Exists method survives rewrite**
- **Test**: `Grep("StubInterface_NTRSetStayoutMaximum_Exists", path="engine.Tests/Tests/HeadlessIntegrationTests.cs")`
- **Expected**: Match found confirming the integration test method exists after T12 rewrite
- **Rationale**: T12 updates HeadlessIntegrationTests for the new delegate-free API. This AC prevents silent deletion of the integration test — AC#12 (dotnet test succeeds) cannot catch a deleted test method. Philosophy "zero technical debt carried forward" requires preserving test coverage.

**AC#12d: NtrInitializationTests preserve bit-packing behavioral assertions**
- **Test**: `Grep("(0b11110000|0b00001111|shiftedDays|HighNibble|LowNibble|MaxDays|Masks)", path="engine.Tests/Tests/NtrInitializationTests.cs")`
- **Expected**: Match found confirming bit-packing behavioral assertions exist in the rewritten tests
- **Rationale**: AC#12b counts test methods (>= 5) but cannot verify behavioral content. The 3 critical behavioral edge cases (high nibble isolation, max days 7, 4-bit masking) must be preserved as equivalence tests per C2. This AC verifies at least one bit-packing assertion pattern exists in the rewritten tests, preventing 5 trivial tests from satisfying AC#12b. [Constraint: C2]

**AC#12e: HeadlessIntegrationTests StubInterface method references new API**
- **Test**: `Grep("new NtrInitialization\\([^)]", path="engine.Tests/Tests/HeadlessIntegrationTests.cs")`
- **Expected**: Match found confirming a NtrInitialization constructor call with arguments (the new IVariableStore parameter)
- **Rationale**: AC#12c verifies the method name survives rewrite but cannot verify the method body tests the new API. A gutted method with `Assert.Pass()` would satisfy AC#12c while providing zero coverage. The original matcher `(SetStayoutMaximum|IVariableStore)` was too weak since both pre-existed in the file (`SetStayoutMaximum` in the method name, `IVariableStore` in `MockVariableStore : IVariableStore` at line 23). Strengthened to match `new NtrInitialization\([^)]` — this regex matches a constructor call with at least one argument. Before migration, the parameterless constructor `new NtrInitialization()` does NOT match this pattern. After migration, the mandatory IVariableStore constructor parameter forces `new NtrInitialization(mockStore)` which DOES match. A gutted test body (Assert.Pass()) would fail this AC since it wouldn't construct NtrInitialization at all.

**AC#12f: NtrInitializationTests preserve null-guard ArgumentNullException test**
- **Test**: `Grep("Throws.*ArgumentNullException", path="engine.Tests/Tests/NtrInitializationTests.cs")`
- **Expected**: Match found confirming at least one test uses Assert.Throws<ArgumentNullException> or equivalent NUnit exception assertion
- **Rationale**: Strengthened from name-matching to assertion-matching. Previous regex could match test names or comments without behavioral assertion. New regex requires "Throws" and "ArgumentNullException" co-occurring on the same line, which is characteristic of actual NUnit exception assertion code (Assert.Throws<ArgumentNullException> or Throws.TypeOf<ArgumentNullException>). The NtrInitialization constructor includes `_variables = variables ?? throw new ArgumentNullException(nameof(variables))`. The original 2 null-argument tests (null getCflag, null setCflag) must be replaced with 2 null-IVariableStore guard tests: (1) null IVariableStore throws ArgumentNullException, (2) exception has correct parameter name "variables". This maintains the 5-test count (3 behavioral + 2 null-guard) required by AC#12b. Without AC#12f, 5 tests concentrating on bit-packing while omitting the null-guard would satisfy AC#12b+AC#12d. The null constructor guard is the behavioral equivalent of the old null getCflag/setCflag tests — its absence is an equivalence gap violating C2. [Constraint: C2]

**AC#12g: NtrInitializationTests verify null-guard paramName "variables"**
- **Test**: `Grep("\"variables\"", path="engine.Tests/Tests/NtrInitializationTests.cs")`
- **Expected**: Match found confirming the string literal "variables" appears in the rewritten tests
- **Rationale**: AC#12f verifies ArgumentNullException is thrown but cannot verify the exception's ParamName property. Key Decisions explicitly requires test "(2) exception has correct parameter name 'variables'". The paramName string "variables" must appear in the test file (e.g., `Assert.That(ex.ParamName, Is.EqualTo("variables"))` or `e.ParamName == "variables"`). Without this AC, `throw new ArgumentNullException("wrongParam")` passes AC#12f. [Constraint: C2]

**AC#13: IVariableStoreExtensions unit tests cover all methods**
- **Test**: `Grep("[Test]|[Fact]|[TestCase]", path="Era.Core.Tests/Interfaces/IVariableStoreExtensionsTests.cs")`
- **Expected**: Count >= 5 (one per method: GetCFlag-CharacterId, GetCFlag-int, SetCFlag-CharacterId, SetCFlag-int, GetTalent-int)
- **Rationale**: F782 extracts shared GetCFlag/SetCFlag/GetTalent from 3 private copies into IVariableStoreExtensions.cs, including CharacterId overloads to support strongly-typed Shop-layer code. Unit tests verify extension methods behave identically to the replaced private helpers, including the `.Match(v => v, _ => 0)` default-0 error path for getters. File existence alone cannot verify test adequacy; count >= 5 ensures all 5 public methods have at least one test. T23 must include at least one test per getter verifying the default-0 on error behavior. [Constraint: C5]

**AC#13b: IVariableStoreExtensions tests include error/default-0 path**
- **Test**: `Grep("default.*0|returns.*0|_ => 0|Error|error", path="Era.Core.Tests/Interfaces/IVariableStoreExtensionsTests.cs")`
- **Expected**: Match found confirming at least one test exercises the default-0 error path
- **Rationale**: The extracted GetCFlag and GetTalent extension methods use `.Match(v => v, _ => 0)` returning 0 on error. AC#13 counts test methods but cannot verify error-path coverage. This AC ensures the distinctive error-handling behavior of the original private helpers is tested. **Note**: Only GetCFlag and GetTalent getters have the `.Match()` error path; SetCFlag has no error return (void). The error-path test must target GetCFlag or GetTalent specifically.

**AC#13c: IVariableStoreExtensions tests verify GetTalent overload**
- **Test**: `Grep("GetTalent", path="Era.Core.Tests/Interfaces/IVariableStoreExtensionsTests.cs")`
- **Expected**: Match found confirming GetTalent is tested
- **Rationale**: AC#13 (>= 5 test methods) cannot verify per-overload coverage — 5 tests concentrated on one overload would satisfy the count. GetTalent is the asymmetric helper (2-way copy in BodySettings+PregnancySettings only, vs 3-way for GetCFlag/SetCFlag) and most likely to be omitted. This AC ensures T23 includes at least one GetTalent test, providing minimum per-method diversity. GetCFlag and SetCFlag are implicitly covered since they constitute 4 of the 5 overloads and AC#13b requires error-path testing of getters. [Constraint: C5]

**AC#14: GameInitialization delegate params removed from NTRSetStayoutMaximum**
- **Test**: `Grep("NTRSetStayoutMaximum.*Func|NTRSetStayoutMaximum.*Action", path="Era.Core/Common/GameInitialization.cs")`
- **Expected**: 0 matches
- **Rationale**: GameInitialization.NTRSetStayoutMaximum is the single caller of INtrInitializer.SetStayoutMaximum. After delegate migration, it must no longer pass Func/Action parameters. AC#11 (build) provides indirect coverage via TreatWarningsAsErrors, but explicit verification ensures the caller update is documented and testable. [Constraint: C6]

**AC#15: F783 contains N+4 --unit deprecation obligation with trigger condition**
- **Test**: `Grep("N\\+4.*--unit|NOT_FEASIBLE.*--unit", path="pm/features/feature-783.md")`
- **Expected**: Match found confirming N+4 --unit deprecation obligation is documented in F783 with both obligation context (N+4) and subject (--unit) co-occurring
- **Rationale**: Mandatory Handoff requires actionable tracking of N+4 --unit deprecation in the destination feature (F783 Phase 21 Planning). Trigger condition: "C# migration functionally complete (kojo no longer requires ERB test runner)."

**AC#15b: F783 trigger condition specifies concrete kojo dependency**
- **Test**: `Grep("(trigger.*condition.*(kojo|ERB)|kojo.*ERB|kojo.*test runner)", path="pm/features/feature-783.md")`
- **Expected**: Match found confirming the trigger condition co-occurs with the concrete kojo/ERB blocker
- **Rationale**: AC#15 verifies N+4/--unit co-occurrence but does not enforce that the trigger condition is substantive. Philosophy "documented transition points" requires concrete documentation, not just labels. This AC ensures the documented trigger condition co-occurs with the actual blocker reference (kojo/ERB), preventing trivial phrases like "trigger condition: TBD" from passing.

**AC#16: F800 DRAFT includes Shop-layer IVariableStore dedup scope**
- **Test**: `Grep("ShopSystem|ShopDisplay|Shop.*IVariableStore|IVariableStore.*dedup", path="pm/features/feature-800.md")`
- **Expected**: Match found confirming Shop-layer deduplication is scoped in F800
- **Rationale**: Mandatory Handoff row 2 defers Shop-layer IVariableStore helper deduplication (ShopSystem.cs, ShopDisplay.cs strongly-typed helpers) to F800. AC#5a only verifies GeneticsService scope. AC#16 ensures the Shop-layer dedup obligation is not silently dropped from the DRAFT. [Constraint: C5]

**AC#17: CharacterFlagIndex.StayoutMaximumDays enum value exists**
- **Test**: `Grep("StayoutMaximumDays\\s*=\\s*22", path="Era.Core/Types/CharacterFlagIndex.cs")`
- **Expected**: Match found confirming enum value StayoutMaximumDays = 22 exists
- **Rationale**: T2 [I] investigation adds StayoutMaximumDays = 22 to CharacterFlagIndex if missing. The Upstream Issues section notes "int→enum cast always compiles in C#" — AC#11 (build) cannot verify the enum value is correct. This AC directly confirms the enum value exists with the proper integer assignment (22 = 長期滞在最大日数), preventing silent bit-packing errors in SetStayoutMaximum.

**AC#18: Shop-layer private helpers unchanged (C5 exclusion respected)**
- **Test**: `Grep("private.*(GetCFlag|SetCFlag|GetTalent)", path="Era.Core/Shop/")`
- **Expected**: Count = 5 (same as baseline — unchanged by F782)
- **Rationale**: C5 explicitly excludes Shop-layer helpers (ItemPurchase.cs raw int params, ShopSystem.cs/ShopDisplay.cs strongly-typed params) from F782's extraction scope. AC#2 and AC#8 verify Era.Core/State/ helpers go to 0, but no AC prevents accidental modification of Era.Core/Shop/. This AC ensures the Shop-layer exclusion is respected, preserving the deferred obligation for F800. [Constraint: C5]

**AC#19: engine-dev SKILL.md documents IVariableStoreExtensions**
- **Test**: `Grep("IVariableStoreExtensions", path=".claude/skills/engine-dev/SKILL.md")`
- **Expected**: Match found confirming IVariableStoreExtensions is documented in engine-dev SKILL.md
- **Rationale**: IVariableStoreExtensions introduces 5 new public extension methods as the canonical helper API for Era.Core consumers. Zero Debt Upfront principle requires documenting new public APIs to prevent re-duplication by future developers unaware of the shared helpers.

**AC#20: IVariableStoreExtensions.cs exists at designed path**
- **Test**: `Grep("public static class IVariableStoreExtensions", path="Era.Core/Interfaces/IVariableStoreExtensions.cs")`
- **Expected**: Match found confirming the extension class exists as a dedicated file at the designed path
- **Rationale**: T4 creates Era.Core/Interfaces/IVariableStoreExtensions.cs as the single shared location for GetCFlag/SetCFlag/GetTalent helpers. AC#2 and AC#8 verify private copies removed (count=0), but without verifying the shared class exists at the designed path, a developer could place helpers in an existing file (e.g., BodySettings.cs) and satisfy removal ACs while violating the architectural design. This AC enforces the Key Decision to use extension methods in a dedicated file. [Constraint: C5]

**AC#21: Extension method call-site adoption verified in Era.Core/State/**
- **Test**: `Grep("_variables\\.GetCFlag|_variables\\.SetCFlag", path="Era.Core/State/")`
- **Expected**: Count >= 3 (at least one GetCFlag or SetCFlag extension method call per State file)
- **Rationale**: AC#2 and AC#8 verify private helper count reaches 0 (deletion) but cannot verify the replacement uses the shared extension methods. An implementer could delete private helpers and inline the logic using `_variables.GetCharacterFlag(...)` / `_variables.SetCharacterFlag(...)` directly, bypassing the extracted IVariableStoreExtensions class. This AC verifies that at least 3 call sites across the 3 State files (BodySettings, PregnancySettings, WeatherSettings) use `_variables.GetCFlag` or `_variables.SetCFlag` — the extension method syntax unique to IVariableStoreExtensions. **Per-file distribution note**: Count >= 3 could theoretically be satisfied by calls in one file only. However, AC#2 (private GetCFlag/SetCFlag = 0) removes the private helpers from all 3 files, and AC#11 (build succeeds with TreatWarningsAsErrors) ensures each file's call sites compile — forcing replacement code in every file. Since the baseline has 2 private helpers per file (6 total), each file must have compilable replacement calls. [Constraint: C5]

**AC#22: GetTalent extension method call-site adoption verified in Era.Core/State/**
- **Test**: `Grep("_variables\\.GetTalent", path="Era.Core/State/")`
- **Expected**: Count >= 2 (at least one GetTalent extension method call per file that had private GetTalent — BodySettings and PregnancySettings)
- **Rationale**: AC#8 verifies private GetTalent count reaches 0 (deletion) but cannot verify the replacement uses the shared extension method. AC#21 covers GetCFlag/SetCFlag adoption but not GetTalent. An implementer could inline `_variables.GetTalent(new CharacterId(id), talent).Match(v => v, _ => 0)` directly, bypassing IVariableStoreExtensions.GetTalent. This AC verifies at least 2 call sites use `_variables.GetTalent` extension syntax (one per file: BodySettings + PregnancySettings; WeatherSettings has no GetTalent). [Constraint: C5]

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Resolve actionable Phase 20 deferred obligations: extract duplicated GetCFlag/SetCFlag/GetTalent helpers, migrate NtrSetStayoutMaximum, fix OrgasmProcessor TODO | AC#2, AC#8, AC#9, AC#10, AC#10b, AC#6, AC#6b, AC#13, AC#14, AC#17, AC#20, AC#21, AC#22 |
| 2 | Assess and document N+4 --unit deprecation as NOT_FEASIBLE; remove --flow references from active skill files | AC#3, AC#3b, AC#3c, AC#3d, AC#3e, AC#15, AC#15b |
| 3 | Create a [DRAFT] feature for BodySettings GeneticsService extraction and Shop-layer IVariableStore helper deduplication | AC#5a, AC#5b, AC#16 |
| 4 | Update Phase 20 Status from WIP to DONE | AC#7 |
| 5 | Verify zero Phase 20-scoped technical debt in Era.Core/Shop and Era.Core/State; verify cross-phase stubs tracked with destination annotations; SSOT documentation for new APIs | AC#1, AC#18, AC#4, AC#4b, AC#4c, AC#19 |
| 6 | Build succeeds, all tests pass, NtrInitialization equivalence maintained | AC#11, AC#12, AC#12b, AC#12c, AC#12d, AC#12e, AC#12f, AC#12g, AC#13b, AC#13c |

---

<!-- fc-phase-4-completed -->
## Technical Design

### Approach

This feature is a pure infrastructure cleanup: no new runtime behavior, only refactoring and documentation updates. All changes are mechanical and verifiable by static grep/glob after the fact.

The implementation is organized into five distinct work areas, each isolated from the others and executable in sequence:

1. **Helper extraction** — Extract the three private helper methods (GetCFlag, SetCFlag, GetTalent) that are duplicated across BodySettings.cs, PregnancySettings.cs, and WeatherSettings.cs into a new `IVariableStoreExtensions` static class in `Era.Core/Interfaces/`. The extracted extension methods carry identical implementations to the current private helpers. All call sites in the three State classes are updated to use the extension methods, and the private helpers are deleted. WeatherSettings receives only GetCFlag/SetCFlag (no GetTalent, per C5).

2. **NtrSetStayoutMaximum delegate migration** — Apply the same F797 pattern (UterusVolumeInit/TemperatureToleranceInit) to NtrInitialization: remove Func/Action parameters from `INtrInitializer.SetStayoutMaximum` and `NtrInitialization.SetStayoutMaximum`, inject `IVariableStore` via constructor, and update `GameInitialization.NTRSetStayoutMaximum` to remove delegate passthrough. The 5 NtrInitializationTests must be rewritten to use a mock `IVariableStore` instead of delegate lambdas. HeadlessIntegrationTests.StubInterface_NTRSetStayoutMaximum_Exists must also be updated since its delegate-based call site changes.

3. **--flow doc removal** — Edit the 5 active skill/command files to remove all `--flow` references from non-archived lines. All removals are documentation-only (no C# changes). The archived comment in testing/SKILL.md line 511 (`<!-- --flow tests/regression/... -->`) is exempt per AC#3.

4. **Single-file fixes** — (a) Fix OrgasmProcessor.cs line 197: change "Phase 20-21" to "Phase 25-26". (b) Update `phase-20-27-game-systems.md` Phase Status from "WIP" to "DONE".

5. **BodySettings GeneticsService + Shop-layer dedup DRAFT** — Create `pm/features/feature-800.md` as a [DRAFT] feature scoping (a) GeneticsService extraction from BodySettings (genetics, P growth, and hair change methods) AND (b) Shop-layer IVariableStore helper deduplication (ShopSystem.cs, ShopDisplay.cs strongly-typed helpers). Both scope items must appear in the DRAFT Background or Identified Gap section to satisfy AC#16. Register in `index-features.md` and increment Next Feature number to 801.

6. **IVariableStoreExtensions unit test file** — Create Era.Core.Tests/Interfaces/IVariableStoreExtensionsTests.cs with unit tests for the three extension methods (GetCFlag, SetCFlag, GetTalent) to verify they produce identical results to the replaced private helpers.

The approach satisfies all ACs. Build and test verification (AC#11, AC#12) are the final gate confirming no regression.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | After helper extraction and delegate migration, grep Era.Core/Shop/ and Era.Core/State/ for TODO/FIXME/HACK returns 0. No new markers introduced; baseline was already 0 for both dirs. |
| 2 | Delete 3 `private int GetCFlag(...)` and 3 `private void SetCFlag(...)` methods from BodySettings.cs, PregnancySettings.cs, WeatherSettings.cs. Replace call sites with `_variables.GetCFlag(...)` / `_variables.SetCFlag(...)` extension method syntax. |
| 3 | Edit `.claude/skills/testing/SKILL.md`: remove `--flow` from frontmatter description line and from the AC Types table cell (`--unit / --flow` → `--unit`). The archived HTML comment at line 511 is exempt (starts with `<!--`). |
| 3b | Edit `.claude/skills/engine-dev/SKILL.md` line 490: remove `--flow <json>,` from the CLI Options list. |
| 3c | Edit `.claude/skills/run-workflow/SKILL.md` line 102: change `--unit or --flow` to `--unit`. |
| 3d | Edit `.claude/skills/erb-syntax/SKILL.md` line 104: change `--unit` or `--flow` to `--unit`. |
| 3e | Edit `.claude/commands/kojo-init.md` line 171: change `--flow tests/regression/` to archived description or remove the AC row reference. |
| 4 | No action required. NotImplementedException stubs in Era.Core/Shop/ are pre-existing cross-phase stubs and are not modified. AC verifies count ≥ 1 (baseline: 12 = 11 throw-stubs + 1 comment header at ShopSystem.cs:372). |
| 4b | No action required. Stubs already have destination annotations co-located in their NotImplementedException messages. Verify with grep for `NotImplementedException.*(Phase [0-9]+\|See F[0-9]+)` which excludes non-stub phase references. AC is read-only verification. |
| 4c | No action required. F783 already documents the stale stub annotation update obligation (written during F782 FL Phase 3 iter8). AC is read-only verification of Mandatory Handoff row 4 content in destination feature. |
| 5a | Create `pm/features/feature-800.md` with [DRAFT] status, Type: engine, Background describing GeneticsService extraction from BodySettings. File name contains a numeric ID and description matching the regex. |
| 5b | Add row for F800 to the Active Features table in `pm/index-features.md`. Pattern `GeneticsService\|BodySettings.*SRP\|BodySettings.*extraction` must appear in the file. |
| 6 | Edit `Era.Core/Training/OrgasmProcessor.cs` line 197: replace "Phase 20-21" with "Phase 25-26". |
| 6b | Complementary verification: OrgasmProcessor.cs contains "Phase 25-26" after T18 correction (ensures comment was substituted, not deleted). |
| 7 | Edit `docs/architecture/phases/phase-20-27-game-systems.md`: change `**Phase Status**: WIP` to `**Phase Status**: DONE` for Phase 20. |
| 8 | Delete 2 `private int GetTalent(...)` methods from BodySettings.cs and PregnancySettings.cs. Replace call sites with `_variables.GetTalent(...)` extension method. WeatherSettings is not touched for GetTalent. |
| 9 | Remove `Func<int, int, int> getCflag` and `Action<int, int, int> setCflag` parameters from `INtrInitializer.SetStayoutMaximum`. Interface after migration has signature `void SetStayoutMaximum(int characterId, int days)` (uses constructor-injected _variables). |
| 10 | NtrInitialization gains `private readonly IVariableStore _variables` field and constructor. The grep for "IVariableStore" in NtrInitialization.cs matches the field type and constructor param. |
| 10b | T9 SetStayoutMaximum implementation uses `_variables.GetCFlag(...)` and `_variables.SetCFlag(...)` IVariableStoreExtensions methods, confirming delegate migration routes through extracted helpers. |
| 11 | Run `dotnet build` via WSL after all changes. TreatWarningsAsErrors means zero warnings. |
| 12 | Run `dotnet test` via WSL. NtrInitializationTests and HeadlessIntegrationTests must be updated before running (see Key Decisions). |
| 12b | T11 rewrites NtrInitializationTests maintaining >= 5 test methods. C2 equivalence mandate enforced by preserving same test count and behavioral edge cases (high nibble isolation, max days 7, 4-bit masking, null guard). |
| 12c | T12 updates HeadlessIntegrationTests preserving StubInterface_NTRSetStayoutMaximum_Exists method. AC prevents silent deletion during delegate-free API rewrite. |
| 12d | T11 rewrites NtrInitializationTests preserving bit-packing behavioral assertions (0b11110000/0b00001111/shiftedDays patterns). C2 equivalence mandate requires bit-packing edge cases preserved. |
| 12e | T12 updates HeadlessIntegrationTests to reference the new delegate-free API. Matcher verifies `new NtrInitialization\([^)]` — constructor call with arguments (parameterless constructor no longer exists after migration). |
| 12f | T11 rewrites NtrInitializationTests preserving null-guard ArgumentNullException test for null IVariableStore constructor parameter. C2 equivalence mandate: replaces old null getCflag/setCflag tests. |
| 12g | T11 rewrites null-guard test to verify paramName "variables" matches constructor parameter name. |
| 13 | Create Era.Core.Tests/Interfaces/IVariableStoreExtensionsTests.cs with unit tests verifying GetCFlag, SetCFlag, GetTalent extension methods (including CharacterId overloads) produce identical results to the replaced private helpers. |
| 13b | T23 includes at least one test exercising the .Match(v => v, _ => 0) default-0 error path for getters. |
| 13c | T23 includes at least one test for GetTalent overload (asymmetric 2-way copy, most likely to be omitted). |
| 14 | GameInitialization.NTRSetStayoutMaximum method signature changes from `void NTRSetStayoutMaximum(int characterId, int days, Func<int, int, int> getCflag, Action<int, int, int> setCflag)` to `void NTRSetStayoutMaximum(int characterId, int days)`, removing delegate parameters completely. Grep for the old parameter pattern returns 0 matches. |
| 15 | T24 documents N+4 --unit obligation in F783; AC verifies the documentation exists with trigger condition pattern. |
| 15b | T24 documents the concrete trigger condition (kojo/ERB dependency) in F783; AC#15b verifies the trigger condition is substantive. |
| 16 | T20 creates feature-800.md with Shop-layer dedup scope in the Identified Gap or Background section, mentioning ShopSystem.cs and/or ShopDisplay.cs helper deduplication. |
| 17 | T2 adds StayoutMaximumDays = 22 to CharacterFlagIndex if missing; grep confirms the enum value exists with correct integer assignment. |
| 18 | No action required. Era.Core/Shop/ private helpers (GetCFlag, SetCFlag, GetTalent) are not modified by F782. T1 baseline verification confirms count = 5. AC is read-only enforcement of C5 exclusion. |
| 19 | T25 updates engine-dev SKILL.md to document IVariableStoreExtensions (Zero Debt Upfront principle). |
| 20 | T4 creates Era.Core/Interfaces/IVariableStoreExtensions.cs as a dedicated extension class. AC verifies the class declaration exists at the designed path. |
| 21 | T5/T6/T7 update State file call sites from private helpers to _variables.GetCFlag/_variables.SetCFlag extension methods. At least 1 call per file = count >= 3. |
| 22 | T5/T6 update GetTalent call sites from private helpers to _variables.GetTalent extension method. At least 1 call per file (BodySettings + PregnancySettings) = count >= 2. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Where to place shared GetCFlag/SetCFlag/GetTalent helpers | A: Base class (VariableStoreBase), B: IVariableStore extension methods in new file, C: Static utility class | B: IVariableStore extension methods in `Era.Core/Interfaces/IVariableStoreExtensions.cs` | Extension methods are the idiomatic C# pattern for adding helpers to an interface without inheritance coupling. No base class is needed. Consistent with the existing pattern (F797 introduced direct `_variables.GetCharacterFlag/GetTalent` calls; extension methods give them the same short-hand syntax the private helpers provided). |
| NtrInitialization constructor injection style | A: IVariableStore injected via constructor, B: Pass IVariableStore as third parameter to SetStayoutMaximum | A: Constructor injection | F797 (UterusVolumeInit/TemperatureToleranceInit) used constructor injection as the proven pattern. All three State classes use constructor-injected `_variables`. Consistent with existing DI strategy. |
| NtrInitializationTests update strategy | A: Rewrite 5 tests using a fake IVariableStore dictionary-backed implementation, B: Add a new test file, keep old tests and make them compile with #pragma | A: Rewrite in-place using fake IVariableStore | Pre-commit hook enforces immutability on AC/Regression tests, not unit tests in engine.Tests. NtrInitializationTests are unit tests (not AC tests), so they can be updated. Rewriting in-place avoids dead code. The null-argument tests (test 4 and 5) that previously checked null getCflag/setCflag must be replaced with 2 null-IVariableStore guard tests: (1) null IVariableStore throws `ArgumentNullException`, (2) exception has correct parameter name `"variables"`. This maintains the 5-test count. |
| GameInitialization.NTRSetStayoutMaximum signature change | A: Remove getCflag/setCflag params (breaking), B: Keep params with default=null (soft deprecation) | A: Remove params | Single caller confirmed (SYSTEM.ERB via headless). ERB-to-C# calls go through GameInitialization, which now resolves IVariableStore internally. Breaking change is acceptable since there is exactly one call site and it can be updated together. |
| --flow removal in testing/SKILL.md frontmatter | A: Remove entire description field, B: Remove only --flow from description string | B: Remove only --flow from description string | Frontmatter description is used for skill lookup. Removing only the --flow token preserves the description and avoids breaking skill invocation. |
| BodySettings GeneticsService DRAFT feature ID | Next available = 800 (per index-features.md "Next Feature number: 800") | F800 | Index-features.md states next ID is 800. |

### Interfaces / Data Structures

**New file: `Era.Core/Interfaces/IVariableStoreExtensions.cs`**

```csharp
using Era.Core.Types;

namespace Era.Core.Interfaces
{
    /// <summary>
    /// Extension methods for IVariableStore providing short-hand helpers
    /// equivalent to the private GetCFlag/SetCFlag/GetTalent helpers formerly
    /// duplicated across BodySettings, PregnancySettings, and WeatherSettings.
    /// Extracted in F782 (Post-Phase Review Phase 20).
    /// Includes CharacterId overloads to support strongly-typed Shop-layer code.
    /// </summary>
    public static class IVariableStoreExtensions
    {
        public static int GetCFlag(this IVariableStore variables, CharacterId character, CharacterFlagIndex flag)
            => variables.GetCharacterFlag(character, flag).Match(v => v, _ => 0);

        public static int GetCFlag(this IVariableStore variables, int characterId, CharacterFlagIndex flag)
            => variables.GetCFlag(new CharacterId(characterId), flag);

        public static void SetCFlag(this IVariableStore variables, CharacterId character, CharacterFlagIndex flag, int value)
            => variables.SetCharacterFlag(character, flag, value);

        public static void SetCFlag(this IVariableStore variables, int characterId, CharacterFlagIndex flag, int value)
            => variables.SetCFlag(new CharacterId(characterId), flag, value);

        // Note: No CharacterId overload for GetTalent — IVariableStore.GetTalent(CharacterId, TalentIndex)
        // is an instance method that shadows any extension with the same name and parameter types.
        // GetCFlag/SetCFlag CharacterId overloads work because the interface uses different names
        // (GetCharacterFlag/SetCharacterFlag). Shop-layer GetTalent dedup deferred to F800.
        public static int GetTalent(this IVariableStore variables, int characterId, TalentIndex talent)
            => variables.GetTalent(new CharacterId(characterId), talent).Match(v => v, _ => 0);
    }
}
```

**Updated `INtrInitializer.SetStayoutMaximum` signature:**

```csharp
// Before (F371/F797 era):
void SetStayoutMaximum(int characterId, int days, Func<int, int, int> getCflag, Action<int, int, int> setCflag);

// After (F782):
void SetStayoutMaximum(int characterId, int days);
```

**Updated `NtrInitialization.SetStayoutMaximum` implementation:**

```csharp
private readonly IVariableStore _variables;

public NtrInitialization(IVariableStore variables)
{
    _variables = variables ?? throw new ArgumentNullException(nameof(variables));
}

public void SetStayoutMaximum(int characterId, int days)
{
    // T2 verifies CharacterFlagIndex has value 22; adds StayoutMaximumDays = 22 if missing
    var flag = CharacterFlagIndex.StayoutMaximumDays;
    int currentValue = _variables.GetCFlag(characterId, flag);
    currentValue &= ~0b11110000;
    int shiftedDays = (days & 0b00001111) << 4;
    int newValue = currentValue | shiftedDays;
    _variables.SetCFlag(characterId, flag, newValue);
}
```

Note: T2 [I] investigation verifies `CharacterFlagIndex.StayoutMaximumDays` (value 22, 長期滞在最大日数) exists in the enum. If missing, T2 adds it. The design uses the named enum value for type safety (Zero Debt Upfront).

**Updated `GameInitialization.NTRSetStayoutMaximum`:**

```csharp
// Before:
public void NTRSetStayoutMaximum(int characterId, int days, Func<int, int, int> getCflag, Action<int, int, int> setCflag)
{
    _ntrInitializer.SetStayoutMaximum(characterId, days, getCflag, setCflag);
}

// After:
public void NTRSetStayoutMaximum(int characterId, int days)
{
    _ntrInitializer.SetStayoutMaximum(characterId, days);
}
```

`GameInitialization` already holds an `IVariableStore` field (`_variables`) — verify this field exists before implementing; if not, it must be injected via the GameInitialization constructor.

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| `CharacterFlagIndex` may not have a value at index 22 (長期滞在最大日数). The current NtrInitialization uses raw int 22 in delegate calls. After migration to IVariableStore extension methods, GetCFlag requires a `CharacterFlagIndex` enum value. If it does not exist, the implementer must add it or use a cast. | Technical Constraints (interface dependency) | Grep `Era.Core/Types/CharacterFlagIndex.cs` for value 22 before implementing. If missing, add `StayoutMaximumDays = 22` to the enum. (Note: int→enum cast always compiles in C#, so AC#13 build alone does not verify correctness. T2 [I] investigation confirms the value exists or adds it; AC#13 then verifies the complete build succeeds.) |
| `GameInitialization` does not visibly hold an `IVariableStore _variables` field in the read excerpt. If it does not, injecting one requires a constructor change. | Impact Analysis (Era.Core/Common/GameInitialization.cs: LOW) | Grep GameInitialization constructor for `IVariableStore`. If absent, add injection and update DI registration. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 1, 4, 4b, 4c, 18 | Verify baseline state: grep Era.Core/Shop/ and Era.Core/State/ for TODO/FIXME/HACK returns 0; grep Era.Core/Shop/ for NotImplementedException returns >= 1; grep Era.Core/Shop/ for "NotImplementedException.*(Phase [0-9]+\|See F[0-9]+)" returns >= 11; verify F783 contains stale stub annotation obligation; verify Era.Core/Shop/ private helper count = 5 (C5 baseline) | | [x] |
| 2 | 17 | [I] Investigate CharacterFlagIndex enum in Era.Core/Types/: verify value 22 exists or add StayoutMaximumDays = 22 if missing | [I] | [x] |
| 3 | 10, 14 | [I] Investigate GameInitialization constructor: verify IVariableStore _variables field exists or plan constructor injection | [I] | [x] |
| 4 | 2, 8, 20 | Create Era.Core/Interfaces/IVariableStoreExtensions.cs with shared GetCFlag, SetCFlag, GetTalent extension methods (namespace Era.Core.Interfaces) | | [x] |
| 5 | 2, 8, 21, 22 | Update BodySettings.cs: remove private GetCFlag, SetCFlag, GetTalent helpers; update all call sites to use extension methods | | [x] |
| 6 | 2, 8, 21, 22 | Update PregnancySettings.cs: remove private GetCFlag, SetCFlag, GetTalent helpers; update all call sites to use extension methods | | [x] |
| 7 | 2, 21 | Update WeatherSettings.cs: remove private GetCFlag, SetCFlag helpers (no GetTalent in this file); update call sites to use extension methods | | [x] |
| 8 | 9 | Update INtrInitializer interface: remove Func/Action delegate parameters from SetStayoutMaximum; new signature uses IVariableStore | | [x] |
| 9 | 10, 10b | Update NtrInitialization.cs: add IVariableStore constructor injection; rewrite SetStayoutMaximum to use _variables.GetCFlag/_variables.SetCFlag extension methods; update XML doc comments to remove getCflag/setCflag param docs and add IVariableStore constructor param doc; SetStayoutMaximum method body must contain _variables.GetCFlag and _variables.SetCFlag calls (AC#10b) | | [x] |
| 10 | 14 | Update GameInitialization.NTRSetStayoutMaximum: remove getCflag/setCflag delegate params; call SetStayoutMaximum(characterId, days) without delegate passthrough | | [x] |
| 11 | 12, 12b, 12d, 12f, 12g | Rewrite 5 NtrInitializationTests to use fake IVariableStore (dictionary-backed) instead of Func/Action lambdas; replace 2 null-delegate tests with 2 null-IVariableStore guard tests (exception throw + paramName verification); maintain >= 5 test methods covering same behavioral edge cases | | [x] |
| 12 | 12, 12c, 12e | Update HeadlessIntegrationTests.StubInterface_NTRSetStayoutMaximum_Exists and GameInitializationTests.CreateGameInitialization() for new delegate-free API (NtrInitialization constructor now requires IVariableStore) | | [x] |
| 13 | 3 | Edit .claude/skills/testing/SKILL.md: remove --flow from frontmatter description and AC Types table (preserve archived HTML comment) | | [x] |
| 14 | 3b | Edit .claude/skills/engine-dev/SKILL.md: remove --flow from CLI Options list | | [x] |
| 15 | 3c | Edit .claude/skills/run-workflow/SKILL.md: change "--unit or --flow" to "--unit" | | [x] |
| 16 | 3d | Edit .claude/skills/erb-syntax/SKILL.md: remove --flow reference | | [x] |
| 17 | 3e | Edit .claude/commands/kojo-init.md: remove --flow reference from AC row | | [x] |
| 18 | 6, 6b | Fix Era.Core/Training/OrgasmProcessor.cs TODO: change "Phase 20-21" to "Phase 25-26" | | [x] |
| 19 | 7 | Update docs/architecture/phases/phase-20-27-game-systems.md: change Phase Status from WIP to DONE for Phase 20 | | [x] |
| 20 | 5a, 5b, 16 | Create pm/features/feature-800.md as [DRAFT] (Type: engine, GeneticsService extraction from BodySettings + Shop-layer IVariableStore helper deduplication scope); register in index-features.md Active Features table; increment Next Feature number to 801 | | [x] |
| 21 | 11 | Run dotnet build via WSL; verify exit code 0 (TreatWarningsAsErrors enabled) | | [x] |
| 22 | 12 | Run dotnet test via WSL; verify all tests pass | | [x] |
| 23 | 13, 13b, 13c | Create Era.Core.Tests/Interfaces/IVariableStoreExtensionsTests.cs: unit tests for all 5 overloads — GetCFlag(CharacterId,flag), GetCFlag(int,flag), SetCFlag(CharacterId,flag,value), SetCFlag(int,flag,value), GetTalent(int,talent); each verifies identical behavior to replaced private helpers (default 0 on error for getters) | | [x] |
| 24 | 15, 15b | Document N+4 --unit deprecation obligation with trigger condition in F783 Review Context section; write trigger condition and kojo/ERB blocker on the same line to satisfy AC#15b single-line matcher | | [x] |
| 25 | 19 | Update .claude/skills/engine-dev/SKILL.md to document IVariableStoreExtensions in Core Interfaces section per ssot-update-rules.md | | [x] |

<!-- AC Coverage Rule: Every Task must be verified by at least one AC. Multiple ACs per Task allowed. -->

### Task Tags

| Tag | Meaning | Phase 3 | Phase 4 |
|:---:|---------|---------|---------|
| (none) | KNOWN - AC Expected value is deterministic | Create test (RED) | Implement (GREEN) |
| `[I]` | UNCERTAIN - AC Expected depends on implementation | **Skip** | Mini-TDD: Implement → Write test → Verify |

**When to use `[I]`**:
- AC Expected value cannot be determined before implementation
- Task output depends on investigation or runtime discovery
- Downstream tasks depend on this task's concrete output

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | ac-tester | sonnet | feature-782.md AC#1, AC#4, AC#4b | Baseline verification pass (read-only; no code changes) |
| 2 | implementer | sonnet | T2: CharacterFlagIndex investigation; T3: GameInitialization investigation | Investigation findings documented; upstream issues resolved |
| 3a | implementer | sonnet | T23: IVariableStoreExtensionsTests.cs creation (RED) | IVariableStoreExtensionsTests.cs created; tests expected to fail (no implementation yet) |
| 3b | implementer | sonnet | T4-T7: IVariableStoreExtensions.cs creation + 3 State file updates (GREEN) | Era.Core/Interfaces/IVariableStoreExtensions.cs created; private helpers removed from BodySettings, PregnancySettings, WeatherSettings; T23 tests now pass |
| 4 | implementer | sonnet | T8-T10: INtrInitializer + NtrInitialization + GameInitialization updates | Delegate-based API replaced with IVariableStore injection across interface, implementation, and caller |
| 5 | implementer | sonnet | T11-T12: NtrInitializationTests + HeadlessIntegrationTests updates | 5 unit tests + 1 integration test rewritten to use new IVariableStore-based API |
| 6 | implementer | sonnet | T13-T17: --flow removal from 5 active skill/command files | --flow references removed from testing/SKILL.md, engine-dev/SKILL.md, run-workflow/SKILL.md, erb-syntax/SKILL.md, kojo-init.md |
| 7 | implementer | sonnet | T18: OrgasmProcessor.cs TODO fix; T19: phase-20-27-game-systems.md Phase Status update | OrgasmProcessor comment corrected to "Phase 25-26"; Phase 20 Status set to DONE |
| 8 | implementer | sonnet | T20: feature-800.md DRAFT creation + index-features.md registration | feature-800.md created with GeneticsService extraction scope; index-features.md updated |
| 9 | implementer | sonnet | T24: Document N+4 obligation in F783; T25: Update engine-dev SKILL.md with IVariableStoreExtensions | N+4 obligation documented in F783; engine-dev SKILL.md updated |
| 10 | implementer | sonnet | T21: dotnet build | Build succeeds; exit code 0 |
| 11 | ac-tester | sonnet | T22: dotnet test | All tests pass |

### Pre-conditions

- All predecessor features F647, F774-F781, F786, F788-F797 are [DONE]
- WSL2 Ubuntu 24.04 available with /home/siihe/.dotnet/dotnet
- Era.Core/State/ baseline for TODO/FIXME/HACK is 0 (pre-verified in Baseline Measurement)

### Execution Order

Tasks must be executed in dependency order:
1. **T1** (baseline verification) — independent, can run first
2. **T2, T3** (investigation) — must complete before T4-T10 (upstream issues gate)
3. **T23** (create test file RED) — must precede T4
4. **T4** (create extension file GREEN) — must precede T5-T7
5. **T5-T7** (update State files) — can run in parallel after T4
6. **T8** (update INtrInitializer interface) — must precede T9-T10
7. **T9** (update NtrInitialization) — must follow T8
8. **T10** (update GameInitialization) — must follow T8 (depends on interface signature)
9. **T11-T12** (update tests) — must follow T9-T10 (tests reference updated signatures)
10. **T13-T17** (--flow removal) — independent; can run in parallel with T4-T12
11. **T18-T19** (single-file fixes) — independent; can run at any point
12. **T20** (DRAFT feature creation) — independent; can run at any point
13. **T24** (N+4 obligation in F783) — independent; can run at any point
14. **T25** (engine-dev SKILL.md update) — independent; can run at any point after T4
15. **T21** (build) — must follow ALL code changes (T4-T12)
16. **T22** (test) — must follow T21

### Build Verification Steps

```bash
# After all code changes, run via WSL:
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build'
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test'
```

Expected: Exit code 0 for both commands.

### Success Criteria

- All ACs pass
- Build succeeds with zero warnings (TreatWarningsAsErrors=true)
- All existing tests pass (including rewritten NtrInitializationTests)
- feature-800.md exists and is registered in index-features.md

### Error Handling

| Error | Action |
|-------|--------|
| CharacterFlagIndex value 22 missing | Add `StayoutMaximumDays = 22` to enum; document in T2 findings |
| GameInitialization lacks IVariableStore field | Add constructor injection; update DI registration; document in T3 findings |
| Build fails with warnings | Stop; identify warning source; fix before proceeding |
| Test failures after rewrite | Stop; report to user (3 consecutive failures = escalate) |

### Rollback Plan

If issues arise after implementation:
1. Revert commit with `git revert HEAD`
2. Notify user of rollback with specific error details
3. Create follow-up feature for targeted fix (do NOT re-attempt same approach without user guidance)

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| BodySettings SRP violation (GeneticsService extraction) | F782 scope is DRAFT creation only; actual extraction (genetics, P growth, hair change methods) requires separate feature with full AC/Task design | Feature | F800 | T20 |
| Shop-layer IVariableStore helper duplication (ShopSystem.cs, ShopDisplay.cs) | Strongly-typed helpers use CharacterId/TalentIndex params matching IVariableStoreExtensions signatures; deduplication deferred to post-F782 | Feature | F800 | T20 |
| N+4 --unit deprecation (testing SKILL removes --unit) | Blocked by kojo testing pipeline: 9+ active skill files, 100+ kojo test JSON files depend on --unit; removal requires C# migration functionally complete (kojo no longer requires ERB test runner). T24 edits existing F783 to document obligation with trigger condition | Feature | F783 | (Option B — F783 exists; T24 edits) |
| Stale stub annotations ("See Phase 20 sibling feature" × 6) | 6 stubs in ShopSystem.cs/ShopDisplay.cs reference "Phase 20 sibling feature" which becomes stale after Phase 20 → DONE; correct destination requires Phase 21 decomposition | Feature | F783 | (Option B — F783 exists) |

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK
- Missing Task for Option A → FL FAIL
-->

<!-- DRAFT Creation Checklist (Option A):
When a Task creates a new feature-{ID}.md [DRAFT], it MUST complete ALL of:
1. Create feature-{ID}.md file
2. Register in index-features.md (add row to Active Features table)
3. Update "Next Feature number" in index-features.md
AC for DRAFT creation MUST verify BOTH file existence AND index registration.
-->

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-22 | DEVIATION | Bash | dotnet build (no project specified) | exit code 1: MSB1011 multiple projects in folder; retried with explicit project paths |
| 2026-02-22 | DEVIATION | feature-reviewer | Phase 8.1 quality review | NEEDS_REVISION: T1,T9,T10 Task status stale ([ ] instead of [x]) |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [resolved-applied] Phase2-Pending iter1: [FMT-004] Review Context section restructured to match feature-template.md format with Origin table, Identified Gap, Review Evidence, Files Involved, Parent Review Observations subsections
- [resolved-applied] Phase2-Pending iter2: [AC-005] AC#5 circular self-reference removed — AC#5 assessed NOT_FEASIBLE and verification deferred to F783 (AC#17); original AC#5 deleted
- [fix] Phase2-Uncertain iter1: line 75 | Removed ## Created: 2026-02-11 heading — not in feature template
- [fix] Phase2-Review iter1: AC Definition Table / AC Details / Goal Coverage / Tasks / Implementation Contract | Added AC#15 (IVariableStoreExtensions unit tests exist) + T23 to cover F782's new deliverable test coverage gap
- [fix] Phase2-Review iter1: AC Definition Table / AC Details / Goal Coverage | Added AC#7b (cross-phase stubs have destination phase annotations) to verify "See Phase [0-9]+" in Era.Core/Shop/
- [fix] Phase2-Review iter2: Technical Design / Interfaces / AC Coverage | Fixed SetStayoutMaximum design inconsistency — removed redundant IVariableStore method parameter, aligned with Key Decision "A: Constructor injection"
- [fix] Phase2-Review iter3: AC#7b Definition / AC Details / AC Coverage | Broadened AC#7b matcher from "See Phase [0-9]+" to "Phase [0-9]+|See F[0-9]+" to cover all stub annotation styles (See Phase, See F, (Phase NN))
- [fix] Phase2-Review iter4: Review Context line 61 | Corrected GetTalent duplication scope — WeatherSettings does NOT have GetTalent (only GetCFlag/SetCFlag); aligned with C5, AC#11, Baseline Measurement
- [fix] Phase2-Uncertain iter5: AC Definition Table / AC Details / Goal Coverage / Tasks | Added AC#16 (GameInitialization delegate params removed from NTRSetStayoutMaximum) for explicit caller verification
- [fix] Phase2-Review iter6: Tasks table T23 | Corrected AC mapping from "16, 17" to "15" — T23 creates IVariableStoreExtensionsTests, unrelated to AC#16 (GameInitialization)
- [fix] Phase2-Uncertain iter6: AC#13 Definition / AC Details | Strengthened matcher from "IVariableStore" to "private readonly IVariableStore" to confirm field declaration
- [fix] Phase2-Review iter6: AC#7b Definition / AC Details | Changed matcher from "matches" to "gte 11" to verify all stubs have destination annotations, not just one
- [fix] Phase2-Review iter7: Philosophy Derivation table | Added AC#7b to "clear phase boundaries" row (was missing after iter1 AC#7b creation)
- [resolved-applied] Phase3-Maintainability iter8: [SCOPE] 7 stubs annotated "See Phase 20 sibling feature" → DEFERRED to F783 (Phase 21 Planning). Correct destination feature IDs require Phase 21 decomposition.
- [fix] Phase3-Maintainability iter8: Tasks table T1 | Added AC#7b to T1 baseline verification (grep Era.Core/Shop/ for destination annotations >= 11)
- [fix] Phase2-Review iter9: AC#7b Definition / AC Details / AC Coverage / T1 | Changed matcher to co-locate NotImplementedException with phase annotations on same line, excluding non-stub inflation (15→11 matches)
- [fix] Phase2-Review iter10: Tasks table T10 | Removed AC#13 from T10 mapping (12,13,17→12,17) — AC#13 greps NtrInitialization.cs which is T9's scope, not T10's
- [fix] Phase2-Review iter1: ## Summary section (lines 77-80) | Removed non-template ## Summary section — feature-template.md has no ## Summary; content was redundant with Background/Goal
- [fix] Phase2-Review iter1: Philosophy Derivation / Goal Coverage | Added AC#2,AC#3 derivation row ("zero technical debt" → C2 equivalence test mandate); split Goal Coverage row 5 into 5 (debt-zero) + 5b (equivalence tests)
- [fix] Phase2-Review iter1: Mandatory Handoffs / AC Definition / AC Details / AC Coverage / Goal Coverage / Tasks / Implementation Contract | Added T24+AC#17 for N+4 --unit obligation tracking in F783; updated Mandatory Handoffs Creation Task from (none) to T24 (after renumbering: T24)
- [fix] Phase3-Maintainability iter2: Tasks T12 / Technical Constraints | Extended T12 to cover GameInitializationTests.CreateGameInitialization() — NtrInitialization constructor change requires IVariableStore mock; added Technical Constraints row
- [fix] Phase3-Maintainability iter2: Upstream Issues CharacterFlagIndex row | Added note: int→enum cast compiles regardless; T2 [I] investigation + AC#13 build verify correctness together
- [fix] Phase2-Review iter3: Success Criteria / Technical Design Approach | Updated "All 18 ACs" → "All 17 ACs" after AC#5 removal in iter2
- [fix] Phase2-Review iter4: AC#3 Definition / AC Details / AC Coverage | Strengthened threshold from gte 4 → gte 6 to match documented baseline of 6 State test files
- [fix] Phase2-Uncertain iter5: AC#7b Details | Added T1 baseline verification note about same-line co-location count confirmation
- [fix] Phase2-Review iter5: AC#18 Definition / AC Details | Tightened matcher from loose OR (N+4|--unit) to co-occurring (N+4.*--unit|NOT_FEASIBLE.*--unit) requiring both obligation context and subject
- [fix] Phase2-Review iter5: Tasks T1 | Fixed threshold reference from >= 4 → >= 6 (missed in iter4 AC#3 propagation)
- [fix] Phase3-Maintainability iter6: Tasks T5/T6 | Merged T6 (GetCFlag/SetCFlag) + T7 (GetTalent) into single T6 for PregnancySettings.cs; AC mapping now 4,11
- [fix] Phase3-Maintainability iter6: AC Design Constraints C5 / Mandatory Handoffs | Corrected C5 exclusion: ItemPurchase.cs (raw int, excluded) vs ShopSystem.cs/ShopDisplay.cs (strongly-typed, deferred); added Mandatory Handoffs row for Shop-layer dedup → F800
- [fix] Phase3-Maintainability iter6: Technical Design Interfaces / AC#15 / AC Coverage | Added CharacterId overloads to IVariableStoreExtensions (Zero Debt Upfront: supports future Shop-layer deduplication without API changes)
- [fix] Phase3-Maintainability iter7: Technical Design Interfaces GetTalent | Removed CharacterId GetTalent overload (dead code: IVariableStore.GetTalent instance method shadows extension with same name+params; int overload restored to call .Match() directly). GetCFlag/SetCFlag CharacterId overloads retained (no name collision with GetCharacterFlag/SetCharacterFlag)
- [fix] Phase2-Review iter1: Tasks table | Renumbered T8→T7 through T25→T24 to close T7 gap (from iter6 T6+T7 merge); updated all downstream references in AC Coverage, Implementation Contract, Execution Order, Mandatory Handoffs, Technical Constraints
- [fix] Phase3-Maintainability iter2: Mandatory Handoffs row 2 / T20 | Shop-layer dedup handoff now references T20 as Creation Task (was "(none)"); T20 description expanded to include Shop-layer scope
- [fix] Phase3-Maintainability iter2: Technical Design Interfaces / T4 / AC#15 / Implementation Contract | Moved IVariableStoreExtensions.cs from Era.Core/Common/ to Era.Core/Interfaces/ (namespace-directory convention); updated test path to Era.Core.Tests/Interfaces/
- [fix] Phase3-Maintainability iter2: Links section | Corrected F800 annotation from "T21" to "T20" (T20 creates feature-800.md)
- [fix] Phase3-Maintainability iter2: Technical Design NtrInitialization code sample | Replaced raw int cast (CharacterFlagIndex)22 with named enum CharacterFlagIndex.StayoutMaximumDays (Zero Debt Upfront; T2 verifies enum value exists)
- [fix] PostLoop-UserFix iter3: Review Context | Restructured to feature-template.md format (Origin, Identified Gap, Review Evidence, Files Involved, Parent Review Observations)
- [resolved-applied] Phase2-Pending iter1: [AC-005] AC#2 and AC#3 removed — read-only existence checks on pre-existing files. C2 satisfied by Baseline Measurement. AC#15 covers F782-specific test verification.
- [fix] PostLoop-UserFix iter3: AC#5 removal | Removed AC#5 (circular self-reference); consolidated N+4 verification into AC#17 (F783); renumbered AC#6a-6e→AC#5-5e, AC#7→AC#6, etc.; removed T21, renumbered T22-T25→T21-T24
- [resolved-skipped] Phase1-RefCheck iter1: [REF-001] F800 referenced in Links section but feature-800.md does not exist yet (forward reference — T20 creates F800 during /run)
- [fix] Phase2-Review iter1: AC Definition Table | Renumbered AC#6→4, 6b→4b, 7a→5a, 7b→5b, 8→6, 9→7, 10→8, 11→9, 12→10, 13→11, 14→12, 15→13, 16→14, 17→15 to match AC Details numbering
- [fix] Phase2-Review iter1: Goal Coverage row 1 | Removed AC#15 from Goal 1 (belongs to Goal 5c only)
- [fix] Phase2-Review iter1: Philosophy Derivation | Added AC#2, AC#8, AC#9, AC#10, AC#14 to "zero technical debt carried forward" row
- [fix] Phase2-Review iter1: Technical Design / Success Criteria | Fixed stale AC#6a→AC#3; removed hardcoded "15" from "All ACs pass"
- [fix] Phase4-ACValidation iter2: AC#5a Definition Table | Changed Method from Glob to Grep, Matcher from contains to matches (Glob pairs with exists, not contains)
- [fix] Phase4-ACValidation iter2: AC#13 Definition Table | Changed Matcher from gte to exists (idiomatic for Glob file existence check)
- [fix] Phase2-Review iter3: AC#13 Definition Table / AC Details | Strengthened from Glob exists to Grep count_equals >= 5 test methods (one per extension method/overload)
- [fix] Phase2-Review iter4: Philosophy Derivation | Added AC#3-3e to "zero technical debt" row; expanded derived requirement to include stale --flow reference removal
- [fix] Phase2-Review iter5: AC Definition Table / AC Details / Goal Coverage / Tasks / AC Coverage | Added AC#16 for Shop-layer IVariableStore dedup scope verification in F800 DRAFT (Mandatory Handoff coverage gap)
- [fix] Phase2-Review iter6: Goal 3 / Goal Coverage row 3 | Expanded description to include "and Shop-layer IVariableStore helper deduplication" to match T20 dual scope
- [fix] Phase2-Review iter6: AC#4b Details | Added note acknowledging 7/11 stubs have stale "See Phase 20 sibling feature" annotations deferred to F783; AC verifies existence, not resolution
- [fix] Phase2-Review iter7: AC#13 Details | Removed false C2 constraint citation (C2 says "no AC required; satisfied by Baseline Measurement"); AC#13 now cites only C5
- [fix] Phase2-Review iter8: T23 description | Expanded to explicitly list all 5 overloads matching AC#13 threshold (was only 3 behavioral groups)
- [fix] Phase2-Review iter9: Mandatory Handoffs row 3 | Changed from "Creation Task: T24" to "Option B — F783 exists; T24 edits" (F783 pre-exists as [DRAFT])
- [fix] Phase3-Maintainability iter10: Links section | Changed F800 from "Unrelated" to "Successor" (F800 is created by T20 as Mandatory Handoff)
- [fix] Phase3-Maintainability iter10: AC#4b Details / Mandatory Handoffs row 4 | Corrected "7 stubs" to "6 stubs" (verified by grep: ShopSystem 4 + ShopDisplay 2 = 6)
- [fix] Phase2-Review iter1: Review Evidence table | Restructured from repeated Field/Value rows to proper multi-column table (Gap#/Gap Source/Derived Task/Comparison Result/DEFER Reason) per feature-template.md single-table format
- [fix] Phase2-Review iter2: F783 Review Context | Corrected stale stub count from "× 7" to "× 6" (ShopSystem 4 + ShopDisplay 2 = 6) to match F782 iter10 correction
- [fix] Phase2-Review iter2: AC Definition / AC Details / Goal Coverage / AC Coverage / Philosophy Derivation | Added AC#15b (trigger condition verifies concrete kojo dependency) to enforce substantive trigger condition in F783
- [fix] Phase2-Review iter3: Goal Coverage row 5 | Removed AC#4/4b (stub existence) from debt-zero Goal 5; added Goal 5b (cross-phase stub tracking) with AC#4, AC#4b, AC#4c
- [fix] Phase2-Review iter3: AC Definition / AC Details / AC Coverage / Philosophy Derivation / T1 | Added AC#4c (F783 stale stub annotation obligation verification) for Mandatory Handoff row 4 coverage
- [fix] Phase2-Review iter4: AC#15b matcher | Strengthened from OR to co-occurrence: "(trigger.*condition.*(kojo|ERB)|kojo.*ERB|kojo.*test runner)" prevents trivial "trigger condition" phrases from passing
- [fix] Phase2-Review iter4: Baseline Measurement | Corrected NotImplementedException count from 11 to 12 (verified: ShopSystem 9 + ShopDisplay 3 = 12); AC#4 Details already had 12
- [fix] Phase2-Review iter5: T24 AC# column | Added AC#15b to T24 mapping (was orphan AC)
- [fix] Phase2-Review iter5: AC#4b threshold | Updated >= 11 to >= 12 (matching corrected baseline of 12 stubs); updated T1 description
- [fix] Phase2-Review iter5: Goal Coverage table | Added AC#13 to Goal 1; added QG row for AC#11/AC#12 (build/test quality gate)
- [fix] Phase2-Review iter6: AC Definition / AC Details / AC Coverage / Goal Coverage / T11 / C2 | Added AC#12b (NtrInitializationTests >= 5 after rewrite) enforcing C2 equivalence mandate
- [fix] Phase2-Review iter6: AC#4c matcher | Strengthened from "See Phase 20 sibling feature|stale.*stub" to "Action Required.*stub|Action Required.*See Phase 20" (distinguishes actionable obligation from problem description)
- [fix] Phase2-Review iter6: AC#13 Details | Added default-0 error path coverage requirement to rationale; T23 must verify .Match(v => v, _ => 0) behavior
- [fix] Phase2-Review iter7: AC#4c matcher | Changed from "Action Required.*stub" to "Stale Stub Annotations.*See Phase 20" (single-line match on F783 section header; avoids multi-line split)
- [fix] Phase2-Review iter7: AC#4b Details | Clarified T1 must add missing destination annotations if count < 12 (was "annotation normalization is needed" with no explicit remediation)
- [fix] Phase2-Review iter8: AC Definition / AC Details / AC Coverage / Goal Coverage / T12 | Added AC#12c (HeadlessIntegrationTests method name survival after rewrite) preventing silent test deletion
- [fix] Phase2-Review iter8: AC Definition / AC Details / AC Coverage / Goal Coverage / T23 | Added AC#13b (error/default-0 path test existence) enforcing .Match(v => v, _ => 0) behavioral verification
- [fix] Phase3-Maintainability iter9: AC#4b threshold / T1 | Reverted >= 12 to >= 11 (co-location pattern matches 11 of 12 NotImplementedException occurrences; 12th is comment header ShopSystem.cs:372)
- [fix] Phase2-Review iter10: AC Coverage row 4 | Aligned baseline from 11 to "12 = 11 throw-stubs + 1 comment header" (matches AC#4 Details baseline: 12)
- [fix] Phase2-Uncertain iter10: AC#13b Details | Added note: only GetCFlag/GetTalent getters have .Match() error path; SetCFlag is void
- [fix] Phase2-Uncertain iter10: Goal Coverage row 5c | Added note: assessment is static in F782 Feasibility Assessment table; ACs verify downstream F783 handoff
- [resolved-skipped] Phase2-Pending iter1: [FMT-004] Review Evidence table format — multi-column vs Field/Value format (loop detected: previously changed from Field/Value to multi-column in iter1)
- [resolved-applied] Phase2-Uncertain iter1: [AC-005] Goal 2 N+4 --unit self-assessment — no AC verifies F782 itself contains trigger condition documentation (only F783 verified via AC#15/15b)
- [fix] PostLoop-UserFix post-loop: AC Definition / AC Details / Goal Coverage / AC Coverage / Philosophy Derivation / T1 | Added AC#19 (F782 Feasibility Assessment documents --unit NOT_FEASIBLE with kojo dependency) for Goal 2 self-verification gap
- [fix] Phase2-Review iter1: AC Definition / AC Details / AC Coverage / Goal Coverage / T2 | Added AC#17 (CharacterFlagIndex.StayoutMaximumDays=22 enum value verification) — T2 [I] adds enum value; int→enum cast always compiles so AC#11 cannot verify correctness
- [fix] Phase2-Review iter2: AC Definition / AC Details / AC Coverage / Goal Coverage / T11 | Added AC#12d (NtrInitializationTests bit-packing behavioral assertions) — AC#12b count cannot verify behavioral content; C2 equivalence mandate requires bit-packing edge cases preserved
- [fix] Phase2-Review iter2: Implementation Contract Phase 3 / Execution Order | Moved T23 before T4 for TDD RED→GREEN compliance — non-[I] tasks require test-first per Task Tags
- [fix] Phase2-Review iter2: AC Definition / AC Details / AC Coverage / Goal Coverage / Baseline Measurement | Added AC#18 (Shop-layer private helper count unchanged) — C5 exclusion enforcement; prevents accidental modification of Era.Core/Shop/
- [fix] Phase2-Review iter3: AC#4b Details | Removed contradictory write instruction from read-only T1 (was "T1 must add" → now "T1 reports gap; annotation remediation out of F782 scope")
- [fix] Phase2-Review iter3: AC Definition / AC Details / AC Coverage / Goal Coverage / T12 | Added AC#12e (HeadlessIntegrationTests references new API) — AC#12c name-existence cannot verify test body exercises delegate-free API
- [fix] Phase2-Review iter4: Tasks T2 AC# / T8 AC# | Corrected T2 mapping from "9, 17" to "17" — AC#9 (Func/Action removal) belongs to T8 (INtrInitializer update), not T2 (CharacterFlagIndex investigation)
- [fix] Phase2-Review iter1: AC Coverage table | Added missing AC#12d row (bit-packing behavioral assertions) — defined in AC Definition Table/Details but absent from AC Coverage
- [fix] Phase2-Review iter1: AC#12e Definition / AC Details / AC Coverage | Strengthened matcher from "(SetStayoutMaximum|IVariableStore)" to "IVariableStore" — SetStayoutMaximum pre-existed before migration; IVariableStore is the new dependency
- [fix] Phase2-Review iter2: AC#12e Definition / AC Details / AC Coverage | Further strengthened matcher from "IVariableStore" to "new NtrInitialization\\([^)]" — IVariableStore pre-exists at line 23 (MockVariableStore : IVariableStore); new pattern matches constructor with arguments (unique to post-migration)
- [resolved-skipped] Phase2-Pending iter3: [AC-001] AC#1 debt-zero scope excludes Era.Core/Interfaces/ — new IVariableStoreExtensions.cs (T4) outside scan; expand AC#1 or not (C1 constraint limits to Shop/State)
- [fix] Phase2-Review iter3: AC Definition / AC Details / AC Coverage / T23 | Added AC#13c (GetTalent overload test verification) — AC#13 count >= 5 cannot verify per-overload coverage; GetTalent asymmetric 2-way copy most likely to be omitted
- [fix] Phase2-Review iter4: Goal Coverage row 2 | Expanded Goal 2 description to include N+4 sub-goal; added AC#15/AC#15b to covering ACs (was split to 5c only)
- [fix] Phase2-Review iter4: Implementation Contract Phase 3 | Split into Phase 3a (T23 RED) and Phase 3b (T4-T7 GREEN) for TDD RED→GREEN boundary enforcement per run-workflow SKILL
- [fix] Phase3-Maintainability iter5: Tasks T1 | Added AC#18 to T1 baseline verification (orphan AC — no Task mapped to AC#18)
- [fix] Phase3-Maintainability iter5: Tasks T10 | Removed AC#9 from T10 mapping (AC#9 is INtrInitializer scope = T8; T10 is GameInitialization scope = AC#14 only)
- [fix] Phase3-Maintainability iter5: Tasks T9 | Added XML doc comment update note to T9 description (remove stale getCflag/setCflag param docs after migration)
- [fix] Phase2-Review iter1: AC Coverage table | Added missing AC#18 row (Shop-layer private helpers unchanged — C5 exclusion enforcement)
- [fix] Phase2-Review iter1: AC Definition / AC Details / AC Coverage / Goal Coverage / T11 / Philosophy Derivation | Added AC#12f (NtrInitializationTests null-guard ArgumentNullException test) — C2 equivalence mandate for null constructor guard
- [fix] Phase2-Uncertain iter1: Philosophy Derivation "documented transition points" row | Added AC#16 to anchor Shop-layer dedup tracking obligation
- [resolved-applied] Phase2-Pending iter2: [AC-005] AC#19 is a tautological self-verification — verifies static content in F782's Feasibility Assessment written at FC time; no implementation Task creates this content. Remove AC#19 or add concrete Task?
- [fix] PostLoop-UserFix post-loop: AC Definition / AC Details / AC Coverage / Goal Coverage / Philosophy Derivation / T1 | Removed AC#19 (tautological self-verification of static FC-phase content); Goal 2 F782-internal documentation completed at FC phase, not an implementation deliverable
- [fix] Phase2-Review iter1: AC Definition / AC Details / AC Coverage / Goal Coverage / T9 | Added AC#10b (NtrInitialization SetStayoutMaximum routes through IVariableStore extension methods) — closes gap between field declaration (AC#10) and actual usage in method body
- [fix] Phase2-Review iter2: Technical Design Work Area 5 | Expanded F800 DRAFT scope from GeneticsService-only to include Shop-layer IVariableStore helper deduplication (aligning with T20 and AC#16)
- [fix] Phase2-Review iter3: AC#5a Definition / AC Details | Changed matcher target from glob (pm/features/feature-*.md) to specific file (pm/features/feature-800.md) — F780 contains "GeneticsService" text causing false-positive with glob
- [fix] Phase3-Maintainability iter4: AC Definition / AC Details / AC Coverage / Goal Coverage / Tasks / Implementation Contract / Execution Order | Added T25+AC#19 (engine-dev SKILL.md documents IVariableStoreExtensions) per ssot-update-rules.md line 16
- [fix] Phase2-Review iter5: AC Definition / AC Details / AC Coverage / Philosophy Derivation / T18 | Added AC#6b (OrgasmProcessor "Phase 25-26" positive assertion) — AC#6 not_contains cannot distinguish correction from deletion
- [fix] Phase3-Maintainability iter6: AC#4b Details | Added discovered-issue tracking path for count < 11 scenario (Leak Prevention: "Track What You Skip")
- [fix] Phase3-Maintainability iter6: AC#4c Details | Clarified that "F783 already contains" refers to stale stub annotation content only; N+4 trigger condition (AC#15/15b) is a separate obligation written by T24
- [fix] Phase2-Review iter7: Goal Coverage row 1 | Added AC#6b to Goal 1 covering ACs (OrgasmProcessor correction verification)
- [fix] Phase2-Review iter1: AC Definition Table / AC Details / AC Coverage / Goal Coverage / Tasks / Philosophy Derivation | Renumbered AC#20 → AC#19 (closing numbering gap after AC#19 removal); added new AC#20 (IVariableStoreExtensions.cs exists at designed path) to T4
- [fix] Phase2-Review iter1: Execution Order | Fixed duplicate step number 15 → renumbered T22 step to 16
- [fix] Phase2-Review iter1: Baseline Measurement | Added co-located annotation count row (value=11) validating AC#4b expected value
- [fix] Phase2-Review iter2: AC#12f Rationale / Key Decisions / T11 | Clarified 2 null-guard tests maintained (exception throw + paramName verification) to match AC#12b >= 5 (3 behavioral + 2 null-guard = 5)
- [resolved-skipped] Phase2-Review iter3: AC#4b requires >= 11 co-located annotations but T1 is read-only; if baseline < 11 (unlikely given measured baseline = 11), no F782 remediation Task exists. User chose: keep current design (baseline = 11, F783 tracks annotation remediation)
- [fix] Phase2-Review iter1: Goal Coverage Verification table | Merged 5b into row 5 (cross-phase stub tracking), removed 5c (already covered by Goal 2), renamed QG→6 for template-compliant integer IDs
- [fix] Phase2-Review iter1: AC#12f Definition / AC Details | Strengthened matcher from name-matching to assertion-matching: (ArgumentNullException|null.*IVariableStore|NtrInitialization.*null) → Throws.*ArgumentNullException
- [fix] Phase2-Review iter1: Tasks T24 | Added single-line format constraint for trigger condition to satisfy AC#15b matcher
- [fix] Phase2-Review iter1: Tasks T9 | Added AC#10b constraint note: SetStayoutMaximum method body must contain _variables.GetCFlag and _variables.SetCFlag calls
- [fix] Phase2-Review iter2: AC Definition / AC Details / AC Coverage / Goal Coverage / T11 | Added AC#12g (NtrInitializationTests paramName "variables" verification) — AC#12f cannot verify paramName; Key Decisions requires it
- [fix] Phase2-Review iter2: AC Definition / AC Details / AC Coverage / Goal Coverage / Philosophy Derivation / T5/T6/T7 | Added AC#21 (extension method call-site adoption >= 3 in Era.Core/State/) — AC#2/AC#8 verify deletion but not adoption
- [fix] Phase2-Review iter3: AC Definition / AC Details / AC Coverage / Goal Coverage / Philosophy Derivation / T5/T6 | Added AC#22 (GetTalent call-site adoption >= 2 in Era.Core/State/) — AC#8 verifies deletion but not adoption; AC#21 only covers GetCFlag/SetCFlag
- [fix] Phase2-Uncertain iter3: AC#19 Details / AC Coverage | Corrected SSOT citation: ssot-update-rules.md line 16 applies to interfaces not static classes; replaced with Zero Debt Upfront principle rationale
- [fix] Phase2-Review iter4: Goal Coverage row 1/5 | Moved AC#19 from Goal 1 (deferred obligation resolution) to Goal 5 (zero technical debt / SSOT documentation) — AC#19 is SSOT doc update, not deferred obligation resolution
- [fix] Phase2-Review iter5: Goal Coverage row 6 | Added orphan AC#13c (GetTalent overload test) to Goal 6 covering ACs
- [fix] Phase2-Uncertain iter5: AC#21 Details | Added per-file distribution note explaining why >= 3 is sufficient (AC#2 + AC#11 TreatWarningsAsErrors enforce per-file replacement)

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning (parent; decomposed into F774-F781)
- [Predecessor: F774](feature-774.md) - Shop Core (originated 11 NotImplementedException cross-phase stubs)
- [Predecessor: F775](feature-775.md) - Collection
- [Predecessor: F776](feature-776.md) - Items
- [Predecessor: F777](feature-777.md) - Customization
- [Predecessor: F778](feature-778.md) - Body Initialization
- [Predecessor: F779](feature-779.md) - Body Settings UI
- [Predecessor: F780](feature-780.md) - Genetics & Growth (originated BodySettings SRP deferral)
- [Predecessor: F781](feature-781.md) - Visitor Settings
- [Predecessor: F786](feature-786.md) - Test Infrastructure (originated N+4 --unit/--flow deferral)
- [Predecessor: F788](feature-788.md) - IConsoleOutput Phase 20 Extensions
- [Predecessor: F789](feature-789.md) - IVariableStore Phase 20 Extensions
- [Predecessor: F790](feature-790.md) - Engine Data Access Layer
- [Predecessor: F791](feature-791.md) - Engine State Transitions & Entry Point Routing
- [Predecessor: F793](feature-793.md) - GameStateImpl Engine-Side Delegation
- [Predecessor: F794](feature-794.md) - Shared Body Option Validation Abstraction
- [Predecessor: F795](feature-795.md) - External ERB Function Migration (resolved 3 CollectionTracker stubs)
- [Predecessor: F796](feature-796.md) - BodyDetailInit IVariableStore Migration
- [Predecessor: F797](feature-797.md) - UterusVolumeInit/TemperatureToleranceInit IVariableStore Migration (originated delegate + duplication deferrals)
- [Successor: F783](feature-783.md) - Phase 21 Planning (blocked by F782)
- [Successor: F800](feature-800.md) - BodySettings GeneticsService Extraction (DRAFT created by T20)
