# Feature 779: Body Settings UI (体設定.ERB lines 350-943)

## Status: [DONE]
<!-- fl-reviewed: 2026-02-17T00:00:00Z -->

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

<!-- Created: 2026-02-11 -->

---

<!-- fc-phase-1-completed -->

## Background

Migrate interactive body settings UI (color customization, body type selection) from 体設定.ERB (lines 350-943) to C#, preserving color utility integration via PRINT_STATE.ERB display functions.

### Philosophy (Mid-term Vision)

Pipeline Continuity - Each phase completion triggers next phase planning. This ensures continuous development pipeline, clear phase boundaries, and documented transition points. SSOT: `designs/full-csharp-architecture.md` Phase 20 section (line 4549+) defines the scope; F647 decomposed it into actionable sub-features.

### Problem (Current Issue)

The 4 body settings UI functions (@体詳細設定１, @体詳細設定２, @体詳細整頓, @体詳細オプション設定) in 体設定.ERB (lines 350-943, ~594 lines) cannot be migrated to C# because the business logic (deduplication, slot compaction, mutual exclusion validation, V/A tightness mapping) is interleaved with UI display formatting and input handling loops. No C# abstraction layer exists to separate the testable business logic from the ERB-bound display and input loops. The existing BodySettings.cs (lines 6-348) only covers initialization; IBodySettings.cs exposes only BodyDetailInit. Display format functions (汎用色SET, 肌色SET) are defined only in PRINT_STATE.ERB as #FUNCTION calls with no C# equivalents, and no prior Phase 20 feature established a pattern for separating pure business logic from the display layer. F781 is now creating this pattern for visitor settings, which F779 should follow.

### Goal (What to Achieve)

Migrate business logic from 3 of the 4 body settings UI functions (@体詳細設定２, @体詳細整頓, @体詳細オプション設定) to C# by: (1) extending IBodySettings with deduplication, slot compaction, and mutual exclusion validation methods, (2) implementing the extractable business logic subset of the 23 menu items (deduplication, slot compaction, mutual exclusion validation, V/A tightness non-linear BASE mapping, P size offset) while direct CFLAG assignments, display formatting, color utilities, input handling, and ERB RESTART loops remain in ERB, (3) adding CharacterFlagIndex and BaseIndex constants for body parameters, and (4) ensuring @体詳細整頓 is exposed via the interface for F780 cross-call compatibility. @体詳細設定１ contains no extractable business logic (character selection menu only) and remains entirely in ERB. The C# implementation must produce equivalent behavior to the ERB original when called from OPTION.ERB:43 and 追加パッチverup.ERB:111.

<!-- Sub-Feature Requirements (architecture.md:4629-4637): /fc時に以下を反映すること
  1. Philosophy継承: Phase 20: Equipment & Shop Systems
  2. Tasks: 負債解消 (TODO/FIXME/HACKコメント削除タスクを含む)
  3. Tasks: 等価性検証 (legacy実装との等価性テストを含む)
  4. AC: 負債ゼロ (技術負債ゼロを検証するACを含む)
-->

### Scope Reference

| File | Lines | Functions | Note |
|------|------:|-----------|------|
| Game/ERB/体設定.ERB | 350-943 | 4 | @体詳細設定１, @体詳細設定２, @体詳細整頓, @体詳細オプション設定 |

---

<!-- fc-phase-2-completed -->

## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why can't the body settings UI be migrated? | The 4 functions (lines 350-943) mix business logic with display formatting and input loops | 体設定.ERB:379-382, 575-733 |
| 2 | Why is the logic interleaved with display? | Display format functions (汎用色SET, 肌色SET) are defined only as ERB #FUNCTIONs with no C# equivalents | PRINT_STATE.ERB:1073, :844, :968, :1358 |
| 3 | Why are there no C# equivalents for display? | No prior Phase 20 feature separated pure business logic from the display layer | Era.Core/State/BodySettings.cs (only lines 6-348) |
| 4 | Why wasn't separation established earlier? | F647 created the scope boundary but did not extract abstractions; F781 is now creating the pattern | feature-781.md, IVisitorSettings.cs |
| 5 | Why (Root)? | The C# interface layer (IBodySettings.cs) only exposes BodyDetailInit and lacks methods for dedup, compaction, validation, and option setting logic | IBodySettings.cs:11-15, BodySettings.cs:24 |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Body settings UI (4 functions, ~594 lines) remains entirely in ERB | Business logic (dedup, compaction, validation, V/A mapping) is interleaved with display/input, and IBodySettings lacks abstraction methods |
| Where | 体設定.ERB lines 350-943 | IBodySettings.cs (incomplete interface) + BodySettings.cs (init only) + missing CharacterFlagIndex/BaseIndex constants |
| Fix | Keep all logic in ERB | Extract testable business logic to C# via IBodySettings extension, leave display in ERB, follow F781 pattern |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Parent — Phase 20 Planning (decomposed this feature) |
| F778 | [DONE] | Sibling — Body Initialization (体設定.ERB lines 6-348), no cross-calls |
| F780 | [DRAFT] | Successor — Genetics & Growth, @体設定_遺伝 (line 1336) calls @体詳細整頓 (line 743) |
| F781 | [DONE] | Parallel — Visitor Settings (体設定.ERB lines 1431-1976), establishes logic/UI separation pattern |
| F794 | [DRAFT] | Successor — Shared Body Option Validation Abstraction (dedup/compaction/validation shared with F781) |
| F788 | [DONE] | Infrastructure — IStringVariables Phase 20 Extensions |
| F789 | [DONE] | Infrastructure — IStringVariables + I3DArrayVariables Phase 20 Extensions |
| F790 | [DONE] | Infrastructure — IEngineVariables + ICsvNameResolver Engine Data Access Layer |
| F791 | [DONE] | Infrastructure — IGameState mode transitions + IEntryPointRegistry |
| F792 | [DONE] | Infrastructure — ac-static-verifier count_equals matcher |
| F793 | [DONE] | Infrastructure — GameStateImpl engine-side delegation |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Interface infrastructure | FEASIBLE | IVariableStore (GetCharacterFlag/SetCharacterFlag, GetBase/SetBase), IEngineVariables (GetCharaNum), IConsoleOutput, IInputHandler all exist |
| Pattern precedent | FEASIBLE | F781 [WIP] establishes logic-only-in-C# pattern for same file |
| CFLAG index alignment | FEASIBLE | F778 corrected BodyOption indices (515-518); body params use 500-512, 515-518 |
| Display function availability | FEASIBLE | Display functions (汎用色SET, 肌色SET) stay in ERB; C# handles logic only |
| Scope clarity | NEEDS_REVISION | @体変化_１日経過 (lines 932-940) falls within line range but belongs to F780; must confirm 4-function boundary |

**Verdict**: FEASIBLE (conditional — Task 1 gates CFLAG index verification; 4-function boundary confirmed in Technical Design)

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Era.Core/Interfaces/IBodySettings.cs | HIGH | Extend with dedup, compaction, validation, option setting methods |
| Era.Core/State/BodySettings.cs | HIGH | Add business logic implementation (~300 lines) |
| Era.Core/Types/CharacterFlagIndex.cs | MEDIUM | Add ~17 body parameter index constants |
| Era.Core/Types/BaseIndex.cs | LOW | Add Ｖ緩さ (27) and Ａ緩さ (26) constants |
| Game/ERB/体設定.ERB | HIGH | Delegate logic to C# calls, retain display/input loops |
| Era.Core/DependencyInjection/ServiceCollectionExtensions.cs | LOW | DI registration for extended BodySettings |
| Era.Core.Tests/ | MEDIUM | New unit tests for business logic |
| F780 (downstream) | MEDIUM | @体詳細整頓 interface exposure enables F780 cross-call |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Character-scoped CFLAG (not global SAVEDATA) | 体設定.ERB:379+ | Must use IVariableStore.GetCharacterFlag/SetCharacterFlag |
| 23 menu items (0-22) including V/A tightness | 体設定.ERB:437 | All 23 selections must be handled in C# logic |
| @体詳細整頓 does NOT sync derived values | 体設定.ERB:743-834 | Critical behavioral difference from F781's visitor version |
| Derived value sync happens at selection time | 体設定.ERB:652-653 (hair), :677-678 (skin) | Hair color = 髪原色, skin color = 肌原色 synced inline in @体詳細設定２ |
| @体詳細オプション設定 takes 3 arguments including CharacterId | 体設定.ERB:836 | C# method signature must accept characterId parameter |
| V/A tightness non-linear BASE mapping | 体設定.ERB:692-730 | Selection value does not map linearly to BASE value |
| P size uses -2 offset | 体設定.ERB:712 | P = RESULT - 2 for size calculation |
| RESTART for UI loops | 体設定.ERB:575+ | ERB-side RESTART must be preserved; C# handles logic only |
| Display format functions are ERB #FUNCTIONs | PRINT_STATE.ERB:968, :1358 | 汎用色SET, 肌色SET have no C# equivalents; stay in ERB |
| 5 range groups for body options | 体設定.ERB:852-907 | Each group has different valid ranges for mutual exclusion |
| Deduplication is order-sensitive | 体設定.ERB:745-770 | Lower-numbered slot preserved when duplicates found |
| Slot compaction direction: highest to lowest (4->3->2->1) | 体設定.ERB:795-833 | Gap elimination proceeds from top slot downward |
| Menu includes 99 (complete) and 999 (cancel) | 体設定.ERB:437+ | Beyond 0-22, special exit codes |
| CFLAG index alignment verified | BodySettings.cs (F778) | Body params: 500-512, 515-518; V/P params still need CFLAG.yaml verification |
| IStyleManager.SetColor/ResetColor are stubs | IStyleManager.cs:11,20 | Return Fail("Not implemented"); display must stay in ERB |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Display functions have no C# equivalents | HIGH | HIGH | Follow F781 pattern: logic only in C#, display stays in ERB |
| V/P CFLAG index verification still needed | MEDIUM | MEDIUM | Body params verified by F778 (500-512, 515-518); V/P params (VPosition, PSize etc.) still need CFLAG.yaml verification |
| Divergence from F781 complicates F794 shared abstraction | MEDIUM | HIGH | Coordinate with F781 pattern; ensure compatible interface design |
| @体詳細整頓 behavioral difference (no derived sync vs F781's sync) | MEDIUM | MEDIUM | Document difference explicitly; F794 must handle both variants |
| V/A tightness non-linear mapping complexity | MEDIUM | MEDIUM | Implement as lookup table or switch; unit test each mapping |
| Ｖ位置 and 目つき possibly missing from CFLAG.yaml | MEDIUM | HIGH | Verify against original CSV before implementation |
| Scope overlap: @体変化_１日経過 (lines 932-940) within line range | MEDIUM | MEDIUM | Confirm 4-function scope boundary; @体変化_１日経過 belongs to F780 |
| F781 pattern changes during implementation | LOW | MEDIUM | Monitor F781 progress; adapt if pattern evolves |
| RESTART requires ERB-side loop control | LOW | LOW | Well-understood ERB pattern; C# handles logic only |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| BodySettings.cs line count | `wc -l Era.Core/State/BodySettings.cs` | ~492 lines | Post-F778 implementation |
| IBodySettings.cs method count | `grep -c 'void\|int' Era.Core/Interfaces/IBodySettings.cs` | 1 method | Only BodyDetailInit exposed |
| CharacterFlagIndex constant count | `grep -c 'public const' Era.Core/Types/CharacterFlagIndex.cs` | TBD | Needs baseline measurement |
| BaseIndex constant count | `grep -c 'public const' Era.Core/Types/BaseIndex.cs` | TBD | Needs baseline measurement |
| Unit test count | `dotnet test --filter BodySettings --list-tests` | TBD | Needs baseline measurement |

**Baseline File**: `.tmp/baseline-779.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | Character-scoped CFLAG + BASE for tightness | 体設定.ERB:379+ | Verify correct CFLAG indices via IVariableStore |
| C2 | 23 menu items (0-22) | 体設定.ERB:437 | Verify all 23 selections produce correct state changes |
| C3 | NO derived value sync in @体詳細整頓 | 体設定.ERB:743-834 | Verify NO SyncDerivedValues call in Tidy method |
| C4 | 5 range groups for body options | 体設定.ERB:852-907 | Test each boundary for mutual exclusion validation |
| C5 | Deduplication is order-sensitive (lower-numbered preserved) | 体設定.ERB:745-770 | Test duplicate removal preserves lower slot |
| C6 | Slot compaction direction (4->3->2->1) | 体設定.ERB:795-833 | Verify compaction eliminates gaps correctly |
| C7 | V/A tightness non-linear BASE mapping | 体設定.ERB:692-730 | Verify each BASE value for all tightness selections |
| C8 | P size -2 offset | 体設定.ERB:712 | Verify P = RESULT - 2 |
| C9 | @体詳細オプション設定 needs CharacterId parameter | 体設定.ERB:836 | Verify characterId param in method signature |
| C10 | P option range 1-2 mutual exclusion | 体設定.ERB:914-925 | Verify P exclusivity validation |
| C11 | F780 cross-calls @体詳細整頓 | 体設定.ERB:1336 | Verify interface exposure for external callers |
| C12 | Display functions stay in ERB | PRINT_STATE.ERB | AC must NOT require C# display implementation |
| C13 | Hair and skin color sync at selection time, not Tidy | 体設定.ERB:652-653, :677-678 | Verify sync in selection handler, absent from Tidy |
| C14 | CFLAG index alignment required | BodySettings.cs vs CFLAG.yaml | AC must verify indices match actual game data |
| C15 | Interface methods must exist for all extracted logic | Interface Dependency Scan | IBodySettings must expose Tidy, ValidateOption, and selection handlers |

### Constraint Details

**C1: Character-Scoped CFLAG Access**
- **Source**: 体設定.ERB:379+ uses CFLAG:TARGET for all body parameters
- **Verification**: Confirm IVariableStore.GetCharacterFlag/SetCharacterFlag work with body param indices
- **AC Impact**: ACs must use character-scoped variable access, not global SAVEDATA

**C2: Full 23-Item Menu Coverage**
- **Source**: 体設定.ERB:437 defines menu items 0 through 22
- **Verification**: Count distinct CASE branches in @体詳細設定２
- **AC Impact**: At minimum, boundary items (0, 22) and V/A tightness (15, 22) need explicit ACs

**C3: No Derived Sync in Tidy**
- **Source**: 体設定.ERB:743-834 (@体詳細整頓) performs dedup+compact but NO sync
- **Verification**: Compare with F781's @体詳細整頓訪問者 which DOES sync
- **AC Impact**: AC must explicitly verify Tidy does NOT call SyncDerivedValues

**C4: Five Range Groups**
- **Source**: 体設定.ERB:852-907 defines 5 groups with different valid ranges
- **Verification**: Read each IF/ELSEIF branch boundary
- **AC Impact**: Each group boundary needs a test case

**C5: Order-Sensitive Deduplication**
- **Source**: 体設定.ERB:745-770 cascading IF checks
- **Verification**: Trace execution with duplicate in slots 1 and 3
- **AC Impact**: Test that slot 1 survives, slot 3 cleared

**C6: Slot Compaction**
- **Source**: 体設定.ERB:795-833 compacts from highest to lowest
- **Verification**: Trace with gap in middle slot
- **AC Impact**: Test gap elimination with various gap patterns

**C7: V/A Tightness Non-Linear Mapping**
- **Source**: 体設定.ERB:692-730 uses BASE for V/A values
- **Verification**: Read each CASE branch for exact BASE values
- **AC Impact**: Each tightness level needs exact expected BASE value

**C8: P Size Offset**
- **Source**: 体設定.ERB:712 applies RESULT-2
- **Verification**: Read the P size assignment line
- **AC Impact**: Verify P = selection - 2

**C9: CharacterId Parameter**
- **Source**: 体設定.ERB:836 @体詳細オプション設定 receives ARG as character ID
- **Verification**: Check ARG usage in function
- **AC Impact**: C# method must accept and propagate characterId

**C10: P Option Mutual Exclusion**
- **Source**: 体設定.ERB:914-925 P options 1 and 2 cannot coexist
- **Verification**: Read the P-specific exclusion logic
- **AC Impact**: Test that setting P option 1 clears P option 2 and vice versa

**C11: F780 Cross-Call Compatibility**
- **Source**: 体設定.ERB:1336 where F780's @体設定_遺伝 calls @体詳細整頓
- **Verification**: Confirm CALL target matches interface method
- **AC Impact**: IBodySettings.Tidy must be public and callable from ERB delegation

**C12: Display Stays in ERB**
- **Source**: PRINT_STATE.ERB:968 (肌色SET), :1358 (汎用色SET) are ERB #FUNCTIONs
- **Verification**: Confirm no C# display function creation in tasks
- **AC Impact**: ACs must NOT test C# color display; only logic verification

**C13: Hair and Skin Color Sync Timing**
- **Source**: 体設定.ERB:652-653 syncs 髪色=髪原色 at selection time; :677-678 syncs 肌色=肌原色 at selection time (same pattern)
- **Verification**: Confirm both syncs are in @体詳細設定２, not @体詳細整頓
- **AC Impact**: Derived sync remains in ERB (direct CFLAG assignment per Goal); no C# AC required. Tidy AC (AC#10) must verify NO sync in C# implementation.

**C14: CFLAG Index Alignment**
- **Source**: BodySettings.cs uses indices 500-512, 515-518 (verified by F778); CFLAG.yaml mappings for V/P params still need verification
- **Verification**: Read both files and compare index values
- **AC Impact**: ACs must verify that C# constants match the actual game CSV/YAML indices

**C15: Interface Method Coverage**
- **Source**: Interface Dependency Scan — IBodySettings.cs currently only exposes BodyDetailInit
- **Verification**: Check IBodySettings.cs method count after implementation
- **AC Impact**: ACs must verify all extracted logic methods exist in IBodySettings

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (decomposed this feature) |
| Related | F778 | [DONE] | Sibling — Body Initialization (体設定.ERB lines 6-348), no cross-calls |
| Successor | F780 | [PROPOSED] | Genetics & Growth — @体設定_遺伝 (line 1336) CALL @体詳細整頓 (体設定.ERB:743) |
| Related | F781 | [DONE] | Parallel — Visitor Settings, establishes logic/UI separation pattern |
| Successor | F794 | [DONE] | Shared Body Option Validation Abstraction (dedup/compaction/validation) |

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

<!-- fc-phase-3-completed -->

---

## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Pipeline Continuity - Each phase completion triggers next phase planning" | Business logic extraction must cover the 3 functions with extractable logic (@体詳細設定２, @体詳細整頓, @体詳細オプション設定) to unblock F780 (next phase); @体詳細設定１ has no extractable logic per Goal | AC#1, AC#2, AC#3, AC#4, AC#5, AC#6, AC#7, AC#8, AC#9, AC#10, AC#11, AC#12, AC#13, AC#14, AC#15, AC#16, AC#17, AC#18, AC#22, AC#25, AC#26 |
| "SSOT: designs/full-csharp-architecture.md Phase 20 section defines the scope" | Implementation must follow Phase 20 interface patterns established by F781/F789/F790 | AC#1, AC#2, AC#3, AC#19, AC#20, AC#21, AC#23, AC#24, AC#27 |
| "continuous development pipeline, clear phase boundaries, and documented transition points" | Zero technical debt in new code; no TODO/FIXME/HACK markers | AC#17 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | IBodySettings extended with new methods | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | count_equals | `(void|int) \w+\(` = 7 | [x] |
| 2 | IBodySettings existing BodyDetailInit preserved | code | Grep(Era.Core/Interfaces/IBodySettings.cs) | matches | `void BodyDetailInit\(` | [x] |
| 3 | BodySettings implements Tidy method | code | Grep(Era.Core/State/BodySettings.cs) | matches | `public void Tidy\(` | [x] |
| 4 | BodySettings implements ValidateBodyOption method | code | Grep(Era.Core/State/BodySettings.cs) | matches | `public int ValidateBodyOption\(` | [x] |
| 5 | BodySettings implements ValidatePenisOption method | code | Grep(Era.Core/State/BodySettings.cs) | matches | `public int ValidatePenisOption\(` | [x] |
| 6 | BodySettings implements ValidateSimpleDuplicate method | code | Grep(Era.Core/State/BodySettings.cs) | matches | `public int ValidateSimpleDuplicate\(` | [x] |
| 7 | BodySettings implements CalculatePenisSize method | code | Grep(Era.Core/State/BodySettings.cs) | matches | `public int CalculatePenisSize\(` | [x] |
| 8 | Dedup preserves lower-numbered slot and clears higher duplicate | test | dotnet test Era.Core.Tests --filter BodySettings | succeeds | - | [x] |
| 9 | Slot compaction eliminates gaps (4->3->2->1 direction) | test | dotnet test Era.Core.Tests --filter BodySettings | succeeds | - | [x] |
| 10 | Tidy does NOT call SyncDerivedValues | code | Grep(Era.Core/State/BodySettings.cs) | not_matches | `SyncDerivedValues` | [x] |
| 11 | V tightness non-linear BASE mapping (5 levels) | test | dotnet test Era.Core.Tests --filter BodySettings | succeeds | - | [x] |
| 12 | A tightness non-linear BASE mapping (5 levels) | test | dotnet test Era.Core.Tests --filter BodySettings | succeeds | - | [x] |
| 13 | P size offset stores RESULT-2 | test | dotnet test Era.Core.Tests --filter BodySettings | succeeds | - | [x] |
| 14 | Mutual exclusion rejects same range group in body options | test | dotnet test Era.Core.Tests --filter BodySettings | succeeds | - | [x] |
| 15 | Mutual exclusion accepts different range groups | test | dotnet test Era.Core.Tests --filter BodySettings | succeeds | - | [x] |
| 16 | P option range 1-2 mutual exclusion | test | dotnet test Era.Core.Tests --filter BodySettings | succeeds | - | [x] |
| 17 | Zero technical debt in new/modified files | code | Grep(Era.Core/State/BodySettings.cs,Era.Core/Interfaces/IBodySettings.cs) | not_matches | `TODO|FIXME|HACK` | [x] |
| 18 | CharacterFlagIndex body parameter constants added with correct values | code | Grep(Era.Core/Types/CharacterFlagIndex.cs) | matches | `HairLength.*new\(500\)` | [x] |
| 19 | BaseIndex V/A tightness constants added with correct values | code | Grep(Era.Core/Types/BaseIndex.cs) | matches | `VLooseness.*new\(27\)` | [x] |
| 20 | C# build succeeds with zero warnings | build | dotnet build Era.Core | succeeds | - | [x] |
| 21 | Unit tests pass (all BodySettings tests) | test | dotnet test Era.Core.Tests --filter BodySettings | succeeds | - | [x] |
| 22 | Simple duplicate validation for hair/eye/V option pairs | test | dotnet test Era.Core.Tests --filter BodySettings | succeeds | - | [x] |
| 23 | engine-dev SKILL.md updated with IBodySettings extension | code | Grep(.claude/skills/engine-dev/SKILL.md) | matches | `IBodySettings` | [x] |
| 24 | BodySettings constructor accepts IVariableStore dependency | code | Grep(Era.Core/State/BodySettings.cs) | matches | `BodySettings\(IVariableStore` | [x] |
| 25 | Tidy preserves anchor slots (slot1, POption1) during mutual exclusion validation | test | dotnet test Era.Core.Tests --filter BodySettings | succeeds | - | [x] |
| 26 | Tidy Phase 2 cascading: clearing higher slot changes lower slot validation outcome | test | dotnet test Era.Core.Tests --filter BodySettings | succeeds | - | [x] |
| 27 | engine.Tests build succeeds after BodySettings constructor change | build | dotnet build engine.Tests | succeeds | - | [x] |

### AC Details

**AC#1: IBodySettings extended with new methods**
- **Test**: Grep pattern=`(void|int) \w+\(` path=`Era.Core/Interfaces/IBodySettings.cs` | count
- **Expected**: 7 method declarations total (1 existing BodyDetailInit + 6 new: Tidy, ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize, GetTightnessBaseValue). Pattern matches method signatures like `void Tidy(` and `int ValidateBodyOption(`.
- **Rationale**: C15 requires IBodySettings to expose all extracted logic methods. F781 established the IVisitorSettings pattern with 11 methods. F779 needs 7 because display/input stays in ERB (C12) and visitor-specific methods (SyncDerivedValues, ValidateMenuSelection, ResetOption) are not needed for the body version (C3, C12). GetTightnessBaseValue is exposed publicly for testability (AC#11/AC#12) and F794 shared abstraction reuse. The count_equals matcher ensures no accidental method removal. (C15)

**AC#2: IBodySettings existing BodyDetailInit preserved**
- **Test**: Grep pattern=`void BodyDetailInit\(` path=`Era.Core/Interfaces/IBodySettings.cs`
- **Expected**: 1 match (existing method signature preserved)
- **Rationale**: Interface extension must not break the existing BodyDetailInit method. This backward compatibility AC ensures the existing IBodySettings contract remains intact. Per ENGINE.md Issue 63, interface extensions must verify existing methods remain unchanged. (C15)

**AC#3: BodySettings implements Tidy method**
- **Test**: Grep pattern=`public void Tidy\(` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 1 match (Tidy method with characterId parameter for CFLAG access)
- **Rationale**: @体詳細整頓 (ERB lines 743-834) must be extracted as a public Tidy method. C11 requires interface exposure for F780 cross-call (体設定.ERB:1336 where @体設定_遺伝 CALLs @体詳細整頓). The method performs dedup + compaction but NO derived value sync (C3). (C3, C11, C15)

**AC#4: BodySettings implements ValidateBodyOption method**
- **Test**: Grep pattern=`public int ValidateBodyOption\(` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 1 match
- **Rationale**: @体詳細オプション設定 body option cases (ERB lines 852-907) implement 5-range-group mutual exclusion validation for 体1/体2/体3/体4 slots. Returns 1 (accept) or 0 (reject). C4 requires boundary testing for all 5 range groups. C9 requires characterId parameter. (C4, C9)

**AC#5: BodySettings implements ValidatePenisOption method**
- **Test**: Grep pattern=`public int ValidatePenisOption\(` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 1 match
- **Rationale**: @体詳細オプション設定 P option cases (ERB lines 914-925) implement P-specific mutual exclusion with range 1-2 special handling beyond simple duplicate check. Returns 1 (accept) or 0 (reject). C10 requires testing P exclusivity. (C9, C10)

**AC#6: BodySettings implements ValidateSimpleDuplicate method**
- **Test**: Grep pattern=`public int ValidateSimpleDuplicate\(` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 1 match
- **Rationale**: @体詳細オプション設定 simple duplicate cases (ERB lines 840-851, 908-913) implement pairwise duplicate checks for hair, eye, and V option pairs. Logic: if candidate != other slot value, return 1 (accept); else return 0 (reject). Following F781 IVisitorSettings pattern which has identical ValidateSimpleDuplicate method. (C9)

**AC#7: BodySettings implements CalculatePenisSize method**
- **Test**: Grep pattern=`public int CalculatePenisSize\(` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 1 match
- **Rationale**: ERB line 712 applies RESULT-2 offset for P size. C8 requires verification of the -2 offset arithmetic. Following F781 IVisitorSettings pattern with identical CalculatePenisSize method. (C8)

**AC#8: Dedup preserves lower-numbered slot and clears higher duplicate**
- **Test**: `dotnet test Era.Core.Tests --filter BodySettings`
- **Expected**: Test passes verifying: (1) Hair: when HairOption1==HairOption2 and HairOption1!=0, HairOption2 is cleared to 0 while HairOption1 is preserved. (2) Eye: when EyeOption1==EyeOption2 and EyeOption1!=0, EyeOption2 is cleared. (3) Body cascading: option1 vs option2/3/4, option2 vs option3/4, option3 vs option4 all cascade correctly with != 0 guard on each level. (4) Body zero-guard: when BodyOption1=0 and BodyOption2=0, no dedup occurs (guard condition prevents clearing). (5) V/P exclusion: when VOption1==VOption2 and VOption1!=0, VOption2 is NOT cleared by dedup phase (V/P options are not subject to dedup per ERB lines 745-770, only hair/eye/body). ERB lines 745-770.
- **Rationale**: C5 specifies order-sensitive deduplication where the lower-numbered slot survives. This must be verified with specific test cases showing slot preservation order. Negative test (5) verifies V/P options are excluded from dedup phase per ERB scope. (C5)

**AC#9: Slot compaction eliminates gaps (4->3->2->1 direction)**
- **Test**: `dotnet test Era.Core.Tests --filter BodySettings`
- **Expected**: Test passes verifying: (1) Body options compact from slot4 downward (slot4 fills first empty among 1,2,3; slot3 fills among 1,2; slot2 fills slot1). (2) Hair/eye/V/P option pair compaction (slot2 moves to slot1 when slot1 empty). (3) Guard conditions: compaction uses > 0 (populated) and <= 0 (empty), NOT != 0 — negative slot value (-1) treated as empty, allowing higher slot to fill it. After compaction, no gaps remain. ERB lines 783-833.
- **Rationale**: C6 specifies highest-to-lowest compaction direction. A value in slot4 with slots 1-3 empty must move to slot1. Compaction guard (> 0 / <= 0) differs from dedup guard (!= 0): negative values are empty for compaction but populated for dedup. (C6)

**AC#10: Tidy does NOT call SyncDerivedValues**
- **Test**: Grep pattern=`SyncDerivedValues` path=`Era.Core/State/BodySettings.cs`
- **Expected**: 0 matches (no SyncDerivedValues method or call exists)
- **Rationale**: CRITICAL behavioral difference from F781. @体詳細整頓 (body version, ERB lines 743-834) performs dedup + compact but does NOT sync derived values. F781's @体詳細整頓訪問者 DOES sync (ERB lines 1878-1880). C3 explicitly documents this difference. C13 confirms derived sync happens at selection time in @体詳細設定２ (lines 652-653 for hair, lines 677-678 for skin), not in Tidy. If SyncDerivedValues appears, it indicates incorrect behavior ported from F781. **Implementation note**: The string "SyncDerivedValues" must not appear anywhere in BodySettings.cs including comments — the behavioral difference is documented in this feature file, not in code comments. (C3, C13)

**AC#11: V tightness non-linear BASE mapping (5 levels)**
- **Test**: `dotnet test Era.Core.Tests --filter BodySettings`
- **Expected**: Test passes verifying all 5 V tightness BASE mappings from ERB lines 692-702: selection index 0 ('ぎちぎち') -> BASE=0, index 1 ('きゅっきゅっ') -> BASE=100, index 2 ('ゆるゆる') -> BASE=250, index 3 ('がばがば') -> BASE=450, index 4 ('ぽっかり') -> BASE=700. Method accepts selection index (0-4) and returns corresponding BASE value.
- **Rationale**: C7 requires verification of each BASE value for all tightness selections. The mapping is non-linear (0, 100, 250, 450, 700) and must be exact. ERB uses string comparison against 締り具合名称() but C# can use index-based lookup. (C7)

**AC#12: A tightness non-linear BASE mapping (5 levels)**
- **Test**: `dotnet test Era.Core.Tests --filter BodySettings`
- **Expected**: Test passes verifying all 5 A tightness BASE mappings from ERB lines 720-730: selection index 0 ('ぎちぎち') -> BASE=0, index 1 ('きゅっきゅっ') -> BASE=100, index 2 ('ゆるゆる') -> BASE=250, index 3 ('がばがば') -> BASE=450, index 4 ('ぽっかり') -> BASE=700. Same mapping as V but targeting BASE:ARG:Ａ緩さ instead of BASE:ARG:Ｖ緩さ.
- **Rationale**: V and A share the same 5-level non-linear mapping but target different BASE indices. Both must be independently tested to ensure no copy-paste errors. (C7)

**AC#13: P size offset stores RESULT-2**
- **Test**: `dotnet test Era.Core.Tests --filter BodySettings`
- **Expected**: Test passes verifying: CalculatePenisSize(5) returns 3 (5-2), CalculatePenisSize(2) returns 0 (2-2), CalculatePenisSize(0) returns -2 (0-2). ERB line 712: `CFLAG:ARG:Ｐ大きさ = RESULT-2`.
- **Rationale**: C8 requires verification of the P = RESULT - 2 offset. Edge cases at boundaries (0, negative result) must be tested. (C8)

**AC#14: Mutual exclusion rejects same range group in body options**
- **Test**: `dotnet test Era.Core.Tests --filter BodySettings`
- **Expected**: Test passes verifying: for each of the 5 range groups (1-9, 10-29, 30-49, 50-54, 55-59), when ANY of the other 3 body slots has a value in the same range as the candidate, ValidateBodyOption returns 0 (reject). Tests verify OR-logic across all other slots from multiple slot perspectives (slot1 conflict in slot2/3/4, slot2 conflict in slot1/3/4, etc.). Boundary values tested: 1 and 9 (group 1), 10 and 29 (group 2), 30 and 49 (group 3), 50 and 54 (group 4), 55 and 59 (group 5). ERB lines 852-907.
- **Rationale**: C4 requires testing each boundary for all 5 range groups. The validation logic differs per slot (体1/体2/体3/体4 each check a different combination of "other" slots). (C4)

**AC#15: Mutual exclusion accepts different range groups**
- **Test**: `dotnet test Era.Core.Tests --filter BodySettings`
- **Expected**: Test passes verifying: when body option slots have values in different range groups (e.g., slot1=5 in range 1-9, candidate=15 in range 10-29), ValidateBodyOption returns 1 (accept). Tests boundary crossings: 9 vs 10 (adjacent groups), 29 vs 30, 49 vs 50, 54 vs 55.
- **Rationale**: Negative test for C4. Ensures mutual exclusion does not falsely reject values in different range groups. Adjacent boundary values are different groups and must be accepted simultaneously. (C4)

**AC#16: P option range 1-2 mutual exclusion**
- **Test**: `dotnet test Era.Core.Tests --filter BodySettings`
- **Expected**: Test passes verifying: **Slot P1 (candidate for POption1, existing POption2)**: (1) POption2=1, candidate=2 returns 0 (reject, both in range 1-2); (2) POption2=3, candidate=1 returns 1 (accept, POption2 outside range 1-2); (3) POption2=4, candidate=4 returns 0 (reject, duplicate). **Slot P2 (candidate for POption2, existing POption1)**: (4) POption1=1, candidate=2 returns 0 (reject, both in range 1-2); (5) POption1=3, candidate=1 returns 1 (accept, POption1 outside range 1-2); (6) POption1=3, candidate=3 returns 0 (reject, duplicate via fallthrough to RETURNF 0). ERB lines 914-925.
- **Rationale**: C10 specifies P options 1-2 have special mutual exclusion beyond simple duplicate. Combined logic: IF both in range [1,2] -> reject; ELSEIF candidate != other -> accept; ELSE fallthrough to RETURNF 0 (reject duplicate). Both slot directions must be tested. (C10)

**AC#17: Zero technical debt in new/modified files**
- **Test**: Grep pattern=`TODO|FIXME|HACK` paths=`Era.Core/State/BodySettings.cs, Era.Core/Interfaces/IBodySettings.cs`
- **Expected**: 0 matches across all target files
- **Rationale**: Sub-Feature Requirements (line 48) mandate debt-free code. Philosophy requires "documented transition points" with no untracked debt. (C12)

**AC#18: CharacterFlagIndex body parameter constants added with correct values**
- **Test**: Grep pattern=`HairLength.*new\(500\)` path=`Era.Core/Types/CharacterFlagIndex.cs`
- **Expected**: At least 1 match verifying HairLength constant with correct CFLAG index 500 (from CFLAG.csv, matching IVariableStore raw array index space). Pattern verifies both name AND value. Additional constants expected for all body parameters with verified indices from CFLAG.csv: HairLength(500), HairLengthCategory(501), HairOption1(502), HairOption2(503), HairBaseColor(504), HairColor(505), EyeColorRight(506), EyeColorLeft(507), EyeExpression(508), EyeOption1(509), EyeOption2(510), SkinBaseColor(511), SkinColor(512), BodyOption1(515), BodyOption2(516), BodyOption3(517), BodyOption4(518). Plus: VPosition(402), VOption1(403), VOption2(404), PSize(406), POption1(407), POption2(408). IVariableStore uses raw array indices (`_cflags[index.Value]`), so CharacterFlagIndex must use CFLAG.csv values, not CFLAG.yaml name-resolution values.
- **Rationale**: Previous matcher (`matches HairLength`) only verified name existence but not index value correctness. IVariableStore.GetCharacterFlag accesses `_cflags[index.Value]` as raw array index. Existing BodySettings.cs (F778) uses CFLAG.csv index 500. CFLAG.yaml value 1175 is for ERB runtime name resolution, a different index space. Wrong index values cause silent runtime data corruption. The value-inclusive matcher prevents this. (C1, C14)

**AC#19: BaseIndex V/A tightness constants added with correct values**
- **Test**: Grep pattern=`VLooseness.*new\(27\)` path=`Era.Core/Types/BaseIndex.cs`
- **Expected**: At least 1 match verifying VLooseness constant with correct BASE index 27 (from BASE.yaml:48-49). Both VLooseness(27) and ALooseness(26) constants expected for Ｖ緩さ and Ａ緩さ.
- **Rationale**: Previous matcher (`matches VLooseness`) only verified name existence. Value-inclusive matcher ensures BASE indices match YAML source of truth. (C1)

**AC#20: C# build succeeds with zero warnings**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'`
- **Expected**: Build succeeds (exit code 0). TreatWarningsAsErrors=true ensures zero warnings.
- **Rationale**: Directory.Build.props enforces TreatWarningsAsErrors. New interface methods and implementation must compile cleanly.

**AC#21: Unit tests pass (all BodySettings tests)**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter BodySettings'`
- **Expected**: All tests pass (exit code 0)
- **Rationale**: Umbrella AC ensuring all unit tests for the BodySettings module pass. Covers dedup (AC#8), compaction (AC#9), V/A tightness mapping (AC#11, AC#12), P size (AC#13), mutual exclusion (AC#14, AC#15, AC#16), and simple duplicate (AC#22). Includes equivalence testing per Sub-Feature Requirements (line 49).

**AC#22: Simple duplicate validation for hair/eye/V option pairs**
- **Test**: `dotnet test Era.Core.Tests --filter BodySettings`
- **Expected**: Test passes verifying: (1) ValidateSimpleDuplicate(5, 5) returns 0 (reject, duplicate); (2) ValidateSimpleDuplicate(5, 3) returns 1 (accept, different); (3) ValidateSimpleDuplicate(0, 5) returns 1 (accept, candidate 0 with non-zero other slot); (4) ValidateSimpleDuplicate(5, 0) returns 1 (accept, other slot empty); (5) ValidateSimpleDuplicate(0, 0) returns 0 (reject, matching ERB semantics). Tests cover all 6 CASE paths from ERB: hair1/hair2 (lines 840-845), eye1/eye2 (lines 846-851), V1/V2 (lines 908-913).
- **Rationale**: ERB @体詳細オプション設定 implements simple duplicate checks for paired options (hair, eye, V). Following F781 IVisitorSettings pattern which has identical ValidateSimpleDuplicate method. All 6 CASE branches share the same logic: IF candidate != other_slot THEN RETURNF 1 ELSE RETURNF 0. Note: ValidateSimpleDuplicate(0,0)=0 is the pure function behavior, but Tidy's dedup phase guards against zero before invoking dedup (so this case is unreachable in Tidy context; however the function correctly handles it per ERB semantics). (C9)

**AC#23: engine-dev SKILL.md updated with IBodySettings extension**
- **Test**: Grep pattern=`IBodySettings` path=`.claude/skills/engine-dev/SKILL.md`
- **Expected**: At least 1 match (IBodySettings documented with new methods in Core Interfaces or Phase 20 section)
- **Rationale**: Per ssot-update-rules.md row 4, new methods on `Era.Core/Interfaces/*.cs` require engine-dev SKILL.md update. IBodySettings extension must be discoverable by future agents/developers. (C15)

**AC#24: BodySettings constructor accepts IVariableStore dependency**
- **Test**: Grep pattern=`BodySettings\(IVariableStore` path=`Era.Core/State/BodySettings.cs`
- **Expected**: At least 1 match confirming BodySettings constructor accepts IVariableStore parameter. DI container (ServiceCollectionExtensions.cs:149) already registers IBodySettings→BodySettings; adding IVariableStore constructor parameter requires the DI container to resolve IVariableStore first.
- **Rationale**: The previous matcher (`matches IBodySettings` in ServiceCollectionExtensions.cs) already passed on the current codebase without any F779 changes, making it unable to detect regressions. The constructor signature in BodySettings.cs is the actual artifact created by F779. AC#27 (engine.Tests build) verifies the downstream impact of this constructor change.

**AC#25: Tidy preserves anchor slots during mutual exclusion validation**
- **Test**: `dotnet test Era.Core.Tests --filter BodySettings`
- **Expected**: Test passes verifying: (1) Body: Tidy validates only slots 4, 3, 2 (NOT slot1). When BodyOption1 has a range-group conflict against BodyOption2, BodyOption1 is NOT cleared by Tidy (slot1 is the anchor, preserved). (2) P: Tidy validates only POption2 (NOT POption1). When POption1 has a range 1-2 conflict against POption2, POption1 is NOT cleared by Tidy (POption1 is the anchor). (3) V: VOption1 and VOption2 are NOT validated/cleared during Tidy Phase 2 (mutual exclusion). V options only participate in Phase 3 compaction (slot2→slot1 when empty), not in mutual exclusion validation. ERB lines 772-780.
- **Rationale**: ERB @体詳細整頓 lines 772-780 validates only 体オプション４/３/２ and Ｐオプション２. Slot1 (body) and POption1 are anchor slots never cleared during Tidy's mutual exclusion phase. V options (VOption1/VOption2) are completely absent from Phase 2 — they use simple duplicate validation only via @体詳細オプション設定 but Tidy never invokes this for V. This is a critical behavioral difference: if implementation validates V options in Phase 2, it would produce non-equivalent behavior (Goal item 5). This AC verifies both anchor slot preservation and V option exclusion boundaries.

**AC#26: Tidy Phase 2 cascading: clearing higher slot changes lower slot validation outcome**
- **Test**: `dotnet test Era.Core.Tests --filter BodySettings`
- **Expected**: Test passes verifying cascading mutual exclusion in Tidy Phase 2: Setup: BodyOption1=15 (range 10-29), BodyOption2=0, BodyOption3=7 (range 1-9), BodyOption4=5 (range 1-9). Phase 2 execution: (1) validate slot4=5: slot3=7 is in same range 1-9 → reject, clear slot4 to 0. (2) validate slot3=7: slot1=15 (range 10-29, different), slot2=0, slot4=0 (just cleared) → accept, slot3 survives. (3) validate slot2=0: skip (0 is not validated). After Phase 2 + Phase 3 compaction: BodyOption1=15, BodyOption2=7 (compacted from slot3), BodyOption3=0, BodyOption4=0. Without cascading (if slot4 not cleared first), slot3 would also be cleared.
- **Rationale**: The sequential order of Phase 2 validation (slot4→slot3→slot2) with real-time CFLAG state updates creates cascading behavior critical for ERB equivalence (Goal item 5). Clearing a higher slot changes the outcome for subsequent lower slot checks. ERB lines 773-778 execute sequentially with CFLAG side effects visible to subsequent checks. This AC verifies that the C# implementation preserves this sequential dependency. (C4, C6)

**AC#27: engine.Tests build succeeds after BodySettings constructor change**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build engine.Tests/'`
- **Expected**: Build succeeds (exit code 0). Adding IVariableStore constructor to BodySettings breaks 3 existing call sites: GameInitializationTests.cs:21, HeadlessIntegrationTests.cs:23, StateSettingsTests.cs:21 (all use parameterless `new BodySettings()`). These must be updated to pass a mock IVariableStore.
- **Rationale**: Constructor parameter addition is a breaking change that affects engine.Tests/ callers. AC#20 only builds Era.Core/ and AC#21 only tests Era.Core.Tests/, leaving engine.Tests/ breakage undetected. Pre-commit hook catches this but the spec must provide explicit guidance.

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Extending IBodySettings with deduplication, slot compaction, and mutual exclusion validation methods | AC#1, AC#2, AC#3, AC#4, AC#5, AC#6, AC#7, AC#23 |
| 2 | Implementing BodySettings business logic (extractable subset of 23 menu items: dedup, compaction, mutual exclusion, V/A tightness mapping, P size offset — direct CFLAG assignments remain in ERB per C12) | AC#3, AC#4, AC#5, AC#6, AC#7, AC#8, AC#9, AC#10, AC#11, AC#12, AC#13, AC#14, AC#15, AC#16, AC#22, AC#26 |
| 3 | Adding CharacterFlagIndex and BaseIndex constants for body parameters | AC#18, AC#19 |
| 4 | Ensuring @体詳細整頓 exposed via interface for F780 cross-call compatibility | AC#1, AC#3, AC#10 |
| 5 | C# implementation must produce equivalent behavior to ERB original | AC#8, AC#9, AC#10, AC#11, AC#12, AC#13, AC#14, AC#15, AC#16, AC#21, AC#22, AC#25, AC#26 |
| 6 | Zero technical debt (Sub-Feature Requirement) | AC#17 |
| 7 | Build and test success | AC#20, AC#21, AC#24, AC#27 |

---

<!-- fc-phase-4-completed -->

## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

Migrate body settings business logic to C# by extending IBodySettings with 6 methods matching F781's IVisitorSettings pattern. The existing BodySettings class will be extended with business logic implementations while display formatting (汎用色SET, 肌色SET), input handling (INPUT loops), and RESTART flow remain in ERB. The approach follows these principles:

1. **Interface Extension**: Add 6 methods to IBodySettings (Tidy, ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize, GetTightnessBaseValue). This is fewer than F781's 11 methods because body settings does NOT sync derived values (C3), does NOT need menu validation (menu bounds are ERB-only), and does NOT need ResetOption (direct CFLAG assignment in ERB). GetTightnessBaseValue is exposed publicly for testability (AC#11/AC#12) and F794 reuse.

2. **Typed Constants**: Add CharacterFlagIndex constants for all body parameters (HairLength through BodyOption4, VPosition through POption2) and BaseIndex constants for V/A tightness (VLooseness=27, ALooseness=26). This adds typed CharacterFlagIndex constants for use by new methods; existing BodyDetailInit hardcoded indices (500-512, 515-518) remain until F796 migration

3. **IVariableStore Integration**: All CFLAG and BASE access goes through IVariableStore.GetCharacterFlag/SetCharacterFlag and GetBase/SetBase. The characterId parameter flows through all methods to enable character-scoped variable access.

4. **V/A Tightness Non-Linear Mapping**: Implement as a private static method accepting selection index (0-4) and returning BASE value using a switch expression with explicit mappings: 0→0 (ぎちぎち), 1→100 (きゅっきゅっ), 2→250 (ゆるゆる), 3→450 (がばがば), 4→700 (ぽっかり). Both V and A use identical mapping but different BaseIndex targets.

5. **Mutual Exclusion Validation**: ValidateBodyOption implements 5-range-group logic (1-9, 10-29, 30-49, 50-54, 55-59) checking candidate against all other 3 slots. ValidatePenisOption implements special 1-2 range exclusion with fallthrough to duplicate check. ValidateSimpleDuplicate implements pairwise equality check.

6. **Slot Compaction**: Tidy method performs 3 phases: (1) Deduplication (lines 745-770) - clear higher slot when duplicate found, (2) Mutual exclusion validation (lines 772-781) - call validation methods from highest to lowest slot, (3) Compaction (lines 782-833) - move values from higher slots to lower empty slots. CRITICAL: Tidy does NOT call SyncDerivedValues (C3 behavioral difference from F781).

7. **DI and Construction**: BodySettings constructor accepts IVariableStore via DI. All methods are instance methods requiring IVariableStore for CFLAG/BASE access.

8. **No ERB Modification**: ERB @体詳細整頓, @体詳細設定２, and @体詳細オプション設定 remain as-is in ERB (not modified in F779). No ERB-to-C# delegation mechanism exists in the engine (ERB CALL can only invoke ERB @functions). F781 follows the same pattern: C# IVisitorSettings exists but ERB @体詳細整頓訪問者 remains pure ERB. The C# validation methods (ValidateBodyOption, ValidatePenisOption, etc.) are used internally by Tidy and exposed on IBodySettings for F794 shared abstraction and future ERB delegation once engine infrastructure is established.

This approach satisfies all 27 ACs by providing testable business logic implementations while preserving ERB's display/input responsibilities.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Extend IBodySettings with 6 new method signatures (Tidy, ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize, GetTightnessBaseValue). Grep count_equals `(void\|int) \w+\(` will match 7 total (1 existing + 6 new). |
| 2 | Preserve existing `void BodyDetailInit(` signature in IBodySettings. No modifications to line 11-15 of IBodySettings.cs. |
| 3 | Implement `public void Tidy(int characterId)` in BodySettings.cs. Method performs dedup → validate → compact in 3 phases following ERB lines 743-834. |
| 4 | Implement `public int ValidateBodyOption(int characterId, int candidateValue, int slot)` with 5-range-group logic checking candidate against other 3 slots. Returns 1 (accept) or 0 (reject). |
| 5 | Implement `public int ValidatePenisOption(int characterId, int candidateValue, int slot)` with special 1-2 range logic. Returns 1 (accept) or 0 (reject). |
| 6 | Implement `public int ValidateSimpleDuplicate(int candidateValue, int otherSlotValue)` with equality check. Returns 1 (accept if different) or 0 (reject if same). |
| 7 | Implement `public int CalculatePenisSize(int rawInput)` returning `rawInput - 2`. |
| 8 | Unit test verifies deduplication logic preserves lower slot. Test cases: HairOption1==HairOption2 (preserve 1, clear 2), BodyOption1==BodyOption3 (preserve 1, clear 3), cascading dedup for all 4 body option pairs. |
| 9 | Unit test verifies slot compaction from highest to lowest. Test cases: BodyOption4 moves to slot1 when slot1 empty, HairOption2 moves to slot1, gap elimination. |
| 10 | Grep `SyncDerivedValues` in BodySettings.cs expects 0 matches. Implementation must NOT include sync call in Tidy method. |
| 11 | Unit test verifies V tightness mapping. Test all 5 levels: GetTightnessBaseValue(0)=0, GetTightnessBaseValue(1)=100, GetTightnessBaseValue(2)=250, GetTightnessBaseValue(3)=450, GetTightnessBaseValue(4)=700. |
| 12 | Unit test verifies A tightness mapping. Same mappings as V but targeting BaseIndex.ALooseness instead of VLooseness. |
| 13 | Unit test verifies P size calculation. Test boundary: CalculatePenisSize(5)=3, CalculatePenisSize(2)=0, CalculatePenisSize(0)=-2. |
| 14 | Unit test verifies mutual exclusion rejection. Test all 5 range groups with conflicts: candidate=5, other_slot=7 (both in range 1-9) returns 0. |
| 15 | Unit test verifies mutual exclusion acceptance. Test adjacent boundaries: candidate=9, other_slot=10 (different groups) returns 1. |
| 16 | Unit test verifies P 1-2 exclusion. Test both slot directions: POption1=1, candidate=2 returns 0; POption2=3, candidate=1 returns 1. |
| 17 | No TODO/FIXME/HACK comments in new code. Grep pattern `TODO\|FIXME\|HACK` expects 0 matches in IBodySettings.cs and BodySettings.cs. |
| 18 | Add CharacterFlagIndex constants with verified CFLAG.csv indices (HairLength=500, etc.). Grep `HairLength.*new\(500\)` expects at least 1 match. AC#18 verifies both name AND index value. |
| 19 | Add 2 BaseIndex constants: VLooseness=27, ALooseness=26. Grep `VLooseness` expects at least 1 match. |
| 20 | Build with WSL dotnet build. TreatWarningsAsErrors=true enforces zero warnings. |
| 21 | All unit tests pass covering ACs 8-16, 22, 25, 26. WSL dotnet test with filter BodySettings. |
| 22 | Unit test verifies simple duplicate logic. ValidateSimpleDuplicate(5,5)=0 (reject), ValidateSimpleDuplicate(5,3)=1 (accept), ValidateSimpleDuplicate(0,5)=1 (accept), boundary cases. |
| 23 | Update engine-dev SKILL.md with IBodySettings section documenting the 6 new methods. Grep `IBodySettings` expects at least 1 match. |
| 24 | Verify BodySettings constructor accepts IVariableStore. Grep `BodySettings\(IVariableStore` in BodySettings.cs expects at least 1 match. |
| 25 | Unit test verifies Tidy does not validate/clear slot1 (body) or POption1 (P) during mutual exclusion phase. |
| 26 | Unit test verifies Tidy Phase 2 cascading: clearing slot4 (range conflict with slot3) enables slot3 to pass subsequent validation. Setup: BodyOption1=15, BodyOption3=7, BodyOption4=5. After Tidy: BodyOption1=15, BodyOption2=7, BodyOption3=0, BodyOption4=0. |
| 27 | Update engine.Tests/ call sites to pass mock IVariableStore to BodySettings constructor. Build engine.Tests/ succeeds. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Number of interface methods | A: 11 methods (match F781), B: 7 methods (1 existing + 6 new), C: 8 methods (add SetBodyParameter helpers) | B: 7 methods | Body settings does NOT sync derived values (C3), does NOT need menu validation (menu bounds are ERB-only), and does NOT need ResetOption (direct CFLAG assignment in ERB). GetTightnessBaseValue exposed for testability (AC#11/AC#12) and F794 reuse. |
| V/A tightness mapping implementation | A: Dictionary lookup, B: Switch expression, C: Array index | B: Switch expression | Switch expression provides compile-time exhaustiveness checking and clearer intent than array indexing. Dictionary adds allocation overhead for 5 fixed mappings. Switch matches ERB's cascading IF structure (lines 692-730). |
| CharacterFlagIndex constants scope | A: Add to existing CharacterFlagIndex.cs, B: Create BodyParameterIndex.cs, C: Keep hardcoded integers | A: Add to CharacterFlagIndex.cs | Consolidates all character flag constants in single SSOT file. Existing pattern has CharacterFlagIndex.Favor; body parameters are same CFLAG array scope. Avoids proliferation of index types. |
| Tidy method phases | A: Single pass (dedup+validate+compact), B: 3 separate phases, C: 2 phases (dedup+compact, skip validate) | B: 3 separate phases | ERB structure (lines 743-834) uses 3 distinct phases. Phase separation improves testability (can verify each phase independently) and matches ERB semantics exactly. Phase order matters: dedup before validate (prevent false conflicts), validate before compact (ensure no invalid values propagate). |
| Mutual exclusion validation return type | A: bool, B: int (0/1), C: Result<bool> | B: int (0/1) | Matches ERB RETURNF convention (lines 854-927) where 0=reject, 1=accept. Direct int return enables zero-cost interop with ERB delegation. Result wrapper adds unnecessary overhead for simple validation. |
| BASE index values source | A: Infer from ERB comments, B: Read CFLAG.yaml, C: Verify against CSV at implementation time, then define as compile-time constants | C: Verify against CSV then hardcode | CFLAG index discrepancy (C14) requires verification against actual game data. Task 1/2 verifies values from CSV/YAML, then defines compile-time BaseIndex constants (VLooseness=27, ALooseness=26). This prevents index mismatch bugs. |
| Derived value sync timing | A: Sync in Tidy, B: Sync in selection handlers (ERB), C: No sync | B: Sync in selection handlers | C13 specifies sync happens at selection time (lines 652-653 for hair, 677-678 for skin) NOT in Tidy. This is CRITICAL behavioral difference from F781. Tidy performs dedup+compact only; derived sync is ERB responsibility. |
| DI pattern for new methods | A: Use IVariableStore in constructor (alongside existing delegate BodyDetailInit), B: Pass delegates to all methods, C: Refactor BodyDetailInit to use IVariableStore | A: IVariableStore in constructor | New methods need persistent variable access. BodyDetailInit's delegate pattern is legacy (F778 scope). Dual-pattern is intentional and temporary: Mandatory Handoff tracks migration to F778. |
| ValidateBodyOption signature pattern | A: Use characterId+IVariableStore (current), B: Use typed interface, C: Pass all slot values as parameters | C: Pass slot values as parameters (F794 target) | F794 reconciliation guidance. F779 keeps characterId+IVariableStore in its interface; F794 will extract pure-function validation accepting slot values as parameters. Compatible with both F779 (IVariableStore) and F781 (IVisitorVariables) callers. |
| characterId parameter type | A: Use CharacterId (strongly typed), B: Keep int (match existing pattern), C: Future Phase 20 migration | B: Keep int | Consistent with F781/IVisitorSettings/BodyDetailInit existing pattern. Codebase-wide int→CharacterId migration is Phase 20+ scope, not F779-specific. Internal cast in each method is acceptable. |

### Interfaces / Data Structures

```csharp
// Era.Core/Interfaces/IBodySettings.cs
public interface IBodySettings
{
    // Existing method (preserved)
    void BodyDetailInit(
        int characterId,
        Func<int, int, int> getCflag,
        Action<int, int, int> setCflag,
        Func<int, int, int> getTalent);

    // New methods (F779)
    void Tidy(int characterId);
    int ValidateBodyOption(int characterId, int candidateValue, int slot);
    int ValidatePenisOption(int characterId, int candidateValue, int slot);
    int ValidateSimpleDuplicate(int candidateValue, int otherSlotValue);
    int CalculatePenisSize(int rawInput);
    int GetTightnessBaseValue(int selectionIndex);
}
```

**Key differences from F781 IVisitorSettings**:
- NO `SyncDerivedValues()` method (C3 - body version does not sync)
- NO `ValidateMenuSelection(int selection)` (menu bounds checked in ERB)
- NO `ResetOption(int selection)` (ERB uses direct CFLAG assignment)
- NO `Deduplicate()`, `ValidateAndClearExclusiveOptions()`, `Compact()` as separate public methods (all unified in single Tidy() following ERB @体詳細整頓 structure)

```csharp
// Era.Core/State/BodySettings.cs - Constructor
private readonly IVariableStore _variables;

public BodySettings(IVariableStore variables)
{
    _variables = variables ?? throw new ArgumentNullException(nameof(variables));
}
```

**New CharacterFlagIndex constants** (Era.Core/Types/CharacterFlagIndex.cs):
```csharp
// Body parameters (CFLAG indices 500-512, 515-518 from BodySettings.cs)
public static readonly CharacterFlagIndex HairLength = new(500);          // 髪の長さ
public static readonly CharacterFlagIndex HairLengthCategory = new(501); // 髪の長さ指定
public static readonly CharacterFlagIndex HairOption1 = new(502);         // 髪オプション１
public static readonly CharacterFlagIndex HairOption2 = new(503);         // 髪オプション２
public static readonly CharacterFlagIndex HairBaseColor = new(504);       // 髪原色
public static readonly CharacterFlagIndex HairColor = new(505);           // 髪色
public static readonly CharacterFlagIndex EyeColorRight = new(506);       // 目色右
public static readonly CharacterFlagIndex EyeColorLeft = new(507);        // 目色左
public static readonly CharacterFlagIndex EyeExpression = new(508);       // 目つき
public static readonly CharacterFlagIndex EyeOption1 = new(509);          // 瞳オプション１
public static readonly CharacterFlagIndex EyeOption2 = new(510);          // 瞳オプション２
public static readonly CharacterFlagIndex SkinBaseColor = new(511);       // 肌原色
public static readonly CharacterFlagIndex SkinColor = new(512);           // 肌色
public static readonly CharacterFlagIndex BodyOption1 = new(515);         // 体オプション１
public static readonly CharacterFlagIndex BodyOption2 = new(516);         // 体オプション２
public static readonly CharacterFlagIndex BodyOption3 = new(517);         // 体オプション３
public static readonly CharacterFlagIndex BodyOption4 = new(518);         // 体オプション４

// V/P parameters (verified from CFLAG.csv: archive/original-source/originalSource(era紅魔館protoNTR/CSV/CFLAG.csv)
public static readonly CharacterFlagIndex VPosition = new(402);      // Ｖ位置
public static readonly CharacterFlagIndex VOption1 = new(403);       // Ｖオプション１
public static readonly CharacterFlagIndex VOption2 = new(404);       // Ｖオプション２
public static readonly CharacterFlagIndex PSize = new(406);          // Ｐ大きさ
public static readonly CharacterFlagIndex POption1 = new(407);       // Ｐオプション１
public static readonly CharacterFlagIndex POption2 = new(408);       // Ｐオプション２
```

**New BaseIndex constants** (Era.Core/Types/BaseIndex.cs):
```csharp
public static readonly BaseIndex VLooseness = new(27);  // Ｖ緩さ (verify via CSV)
public static readonly BaseIndex ALooseness = new(26);  // Ａ緩さ (verify via CSV)
```

**V/A Tightness Mapping Implementation**:
```csharp
// Era.Core/State/BodySettings.cs - Public method (IBodySettings)
public int GetTightnessBaseValue(int selectionIndex)
{
    return selectionIndex switch
    {
        0 => 0,    // ぎちぎち
        1 => 100,  // きゅっきゅっ
        2 => 250,  // ゆるゆる
        3 => 450,  // がばがば
        4 => 700,  // ぽっかり
        _ => throw new ArgumentOutOfRangeException(nameof(selectionIndex), "Tightness selection must be 0-4")
    };
}
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| CFLAG index verification required for V/P parameters | AC Design Constraints C14 | Task must verify actual CFLAG indices for VPosition, VOption1/2, PSize, POption1/2 from CFLAG.yaml or CSV before defining CharacterFlagIndex constants. Body params verified by F778 (500-512, 515-518); V/P ranges still need verification. Risk: Index mismatch causes runtime data corruption. |
| F794 shared abstraction needs behavioral variant handling | Dependencies | F794 must account for body version (no sync) vs visitor version (with sync) when creating shared validation abstraction. Cannot naively extract common logic without preserving behavioral differences. |
| F779 Tidy unified vs F781 3 separate public methods | Technical Design (line 591) | F779 exposes single Tidy() with internal dedup/validate/compact phases. F781 exposes Deduplicate(), ValidateAndClearExclusiveOptions(), Compact() as separate public methods. F794 must either: (1) extract only leaf algorithms as shared, or (2) require F779 to also expose 3 phases publicly. |
| F779/F781 ValidateBodyOption signature reconciliation | Dependencies | Decision: Option C (pass slot values as parameters). F779 and F781 validation methods will accept all slot values as parameters instead of reading IVariableStore/IVisitorVariables internally. This makes validation logic pure functions, enabling F794 shared extraction. F779 caller reads CFLAG via IVariableStore and passes values; F781 caller reads via IVisitorVariables and passes values. |

<!-- fc-phase-5-completed -->

---

## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 18 | Add CharacterFlagIndex constants for all body parameters (HairLength through BodyOption4, VPosition through POption2) using verified CFLAG indices | | [x] |
| 2 | 19 | Add BaseIndex constants (VLooseness=27, ALooseness=26) | | [x] |
| 3 | 1,2,3,4,5,6,7 | Extend IBodySettings with 6 new methods (Tidy, ValidateBodyOption, ValidatePenisOption, ValidateSimpleDuplicate, CalculatePenisSize, GetTightnessBaseValue) preserving existing BodyDetailInit | | [x] |
| 4 | 3,8,9,10,25,26 | Implement BodySettings.Tidy performing deduplication (preserving lower slot), mutual exclusion validation, and slot compaction (4→3→2→1) without SyncDerivedValues call | | [x] |
| 5 | 4,14,15 | Implement BodySettings.ValidateBodyOption with 5-range-group mutual exclusion logic (1-9, 10-29, 30-49, 50-54, 55-59) | | [x] |
| 6 | 5,16 | Implement BodySettings.ValidatePenisOption with P-specific range 1-2 mutual exclusion and fallthrough duplicate check | | [x] |
| 7 | 6,22 | Implement BodySettings.ValidateSimpleDuplicate with pairwise equality check returning 1 (accept if different) or 0 (reject if same) | | [x] |
| 8 | 7,13 | Implement BodySettings.CalculatePenisSize returning rawInput - 2 | | [x] |
| 9 | 11,12 | Implement GetTightnessBaseValue public IBodySettings method with non-linear mapping (0→0, 1→100, 2→250, 3→450, 4→700) used by V and A tightness logic | | [x] |
| 10 | 8,9,10,11,12,13,14,15,16,21,22,25,26 | Create unit tests for all BodySettings business logic methods covering dedup, compaction, tightness mapping, mutual exclusion, P size offset, simple duplicate validation, anchor slot preservation, and cascading Phase 2 | | [x] |
| 11 | 17 | Verify zero technical debt (no TODO/FIXME/HACK comments) in IBodySettings.cs and BodySettings.cs | | [x] |
| 12 | 20,21 | Build Era.Core and run all BodySettings unit tests via WSL dotnet | | [x] |
| 13 | 23 | Update engine-dev SKILL.md with IBodySettings extension documentation | | [x] |
| 14 | 24 | Verify/update DI registration in ServiceCollectionExtensions.cs for BodySettings with IVariableStore constructor dependency | | [x] |
| 15 | 27 | Update engine.Tests/ call sites (GameInitializationTests, HeadlessIntegrationTests, StateSettingsTests) to pass mock IVariableStore to BodySettings constructor | | [x] |

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

**Example**:
```markdown
| 1 | 1 | Add API endpoint | | [ ] |        ← KNOWN: Expected response format is specified
| 2 | 2 | Calculate aggregates | [I] | [ ] | ← UNCERTAIN: Actual totals unknown until implemented
| 3 | 3 | Format report | | [ ] |           ← KNOWN: Uses Task 2's output (determined after Task 2)
```

**When NOT to use `[I]`**:
- Build/compile tasks (Expected = "succeeds" is always deterministic)
- Tasks with spec-defined outputs (API contracts, schema validation)
- Tasks that verify file existence (deterministic yes/no)
- Tasks where Expected can be calculated from requirements (counts from data, paths from config)
- Standard patterns with known outputs (error messages, status codes)

**Anti-pattern**: Using `[I]` to avoid writing concrete Expected values. `[I]` is for genuine uncertainty, not convenience.

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 1 | implementer | sonnet | Tasks 1-2 (Constants) | CharacterFlagIndex and BaseIndex constants added |
| 2 | implementer | sonnet | Task 3 (Interface) | IBodySettings.cs extended with 6 new method signatures |
| 3 | implementer | sonnet | Task 10 (Tests - RED) | Era.Core.Tests/ unit tests for all BodySettings methods (failing) |
| 4 | implementer | sonnet | Tasks 4-9 (Implementation - GREEN) | BodySettings.cs business logic implementation (tests pass) |
| 5 | implementer | sonnet | Tasks 11-15 (Verification) | Build/test success, debt-free code, SKILL.md update, DI registration verified, engine.Tests/ updated |

### Pre-conditions

- F781 (Visitor Settings) establishes logic-only-in-C# pattern
- IVariableStore provides GetCharacterFlag/SetCharacterFlag and GetBase/SetBase
- BodySettings.cs constructor to accept IVariableStore via DI (to be added by Task 3 or Task 4; current class has no constructor)

### Execution Order

**CRITICAL**: Tasks 1-2 MUST complete before Task 3. CharacterFlagIndex and BaseIndex constants must exist before IBodySettings method signatures can reference them.

**TDD Workflow**: Task 10 (test creation) executes BEFORE Tasks 4-9 (RED→GREEN). All expected values are deterministic from ERB source (no `[I]` tags), so tests can be written first. Phase 3 = RED (write failing tests with known expected values), Phase 4 = GREEN (implement business logic to pass tests).

### CFLAG Index Verification (Task 1)

**Before defining CharacterFlagIndex constants**, verify actual indices against game data:

1. Read `Game/data/CFLAG.yaml` for body parameter indices
2. Compare with BodySettings.cs indices 500-512, 515-518 (verified by F778) and ERB comments
3. If mismatch found, use CSV/YAML values (SSOT)
4. Document any discrepancies in Task 1 completion note

Fallback: If CFLAG.yaml is incomplete, verify against Game/archive/original-source/originalSource(era紅魔館protoNTR/CSV/CFLAG.csv (VPosition=402, EyeExpression=508 confirmed).

**Known indices to verify** (from ERB 体設定.ERB):
- 髪の長さ, 髪の長さ指定, 髪オプション１/２, 髪原色, 髪色 (Hair group)
- 目色右, 目色左, 目つき, 瞳オプション１/２ (Eye group)
- 肌原色, 肌色 (Skin group)
- 体オプション１/２/３/４ (Body group)
- Ｖ位置, Ｖオプション１/２ (V group)
- Ｐ大きさ, Ｐオプション１/２ (P group)

**If Ｖ位置 or 目つき missing from CFLAG.yaml**: Document in Mandatory Handoffs for upstream fix.

### BASE Index Verification (Task 2)

Verify BASE indices 26 (Ａ緩さ) and 27 (Ｖ緩さ) from CSV or ERB comments (lines 692-730).

### Interface Extension (Task 3)

Extend `Era.Core/Interfaces/IBodySettings.cs` with 6 new method signatures:

```csharp
// Era.Core/Interfaces/IBodySettings.cs
using Era.Core.Types;

namespace Era.Core.Interfaces;

public interface IBodySettings
{
    // Existing method (preserved - AC#2)
    void BodyDetailInit(
        int characterId,
        Func<int, int, int> getCflag,
        Action<int, int, int> setCflag,
        Func<int, int, int> getTalent);

    // New methods (AC#1, AC#3-7)
    void Tidy(int characterId);
    int ValidateBodyOption(int characterId, int candidateValue, int slot);
    int ValidatePenisOption(int characterId, int candidateValue, int slot);
    int ValidateSimpleDuplicate(int candidateValue, int otherSlotValue);
    int CalculatePenisSize(int rawInput);
    int GetTightnessBaseValue(int selectionIndex);
}
```

### BodySettings Implementation Structure (Tasks 4-9)

Implement in `Era.Core/State/BodySettings.cs`:

**Constructor** (to be added in Task 3 or Task 4):
```csharp
private readonly IVariableStore _variables;

public BodySettings(IVariableStore variables)
{
    _variables = variables ?? throw new ArgumentNullException(nameof(variables));
}
```

**Public Method** (Task 9 - IBodySettings):
```csharp
public int GetTightnessBaseValue(int selectionIndex)
{
    return selectionIndex switch
    {
        0 => 0,    // ぎちぎち
        1 => 100,  // きゅっきゅっ
        2 => 250,  // ゆるゆる
        3 => 450,  // がばがば
        4 => 700,  // ぽっかり
        _ => throw new ArgumentOutOfRangeException(nameof(selectionIndex), "Tightness selection must be 0-4")
    };
}
```

**Tidy Method** (Task 4 - ERB lines 743-834):

3-phase implementation following ERB structure:

1. **Deduplication (lines 745-770)**: Cascading checks preserving lower-numbered slot
   - Hair: if HairOption1 == HairOption2 && HairOption1 != 0 → clear HairOption2
   - Eye: if EyeOption1 == EyeOption2 && EyeOption1 != 0 → clear EyeOption2
   - Body: cascading (option1 vs 2/3/4, option2 vs 3/4, option3 vs 4) with != 0 guard on each level
   - NOTE: V/P options are NOT deduplicated in Phase 1 (ERB lines 745-770 only cover hair, eye, body)

2. **Mutual Exclusion Validation (lines 772-781)**: Call validation methods from highest to lowest slot
   - Body: validate slot4 first, then slot3, then slot2 (NOT slot1 — slot1 is the anchor slot, never cleared by mutual exclusion during Tidy)
   - P: validate POption2 only (NOT POption1 — POption1 is the anchor slot)
   - V: NO validation (V options only participate in Phase 3 compaction, not in mutual exclusion validation per ERB lines 772-780)

3. **Compaction (lines 782-833)**: Move values from higher slots to lower empty slots. **Guard conditions**: populated = `> 0`, empty = `<= 0` (NOT `!= 0`). This differs from dedup which uses `!= 0`. Negative values are treated as empty during compaction.
   - Body: 4→1, 3→1, 2→1 (first empty), then 4→2, 3→2 (second empty), then 4→3 (third empty)
   - Hair/Eye/V/P pairs: slot2→slot1 when slot1 empty

**CRITICAL**: Tidy does NOT call SyncDerivedValues (C3). Derived value sync happens at selection time in ERB, not during Tidy.

**ValidateBodyOption** (Task 5 - ERB lines 852-907):

5 range groups with mutual exclusion:
- Range 1-9: Check if ANY of other 3 slots in range [1,9]
- Range 10-29: Check if ANY of other 3 slots in range [10,29]
- Range 30-49: Check if ANY of other 3 slots in range [30,49]
- Range 50-54: Check if ANY of other 3 slots in range [50,54]
- Range 55-59: Check if ANY of other 3 slots in range [55,59]

Return 0 (reject) if conflict found, 1 (accept) otherwise.

**ValidatePenisOption** (Task 6 - ERB lines 914-925):

P-specific logic:
```
IF (candidate in [1,2] AND otherSlot in [1,2])
    RETURN 0  // Reject: both in range 1-2
ELSEIF (candidate != otherSlot)
    RETURN 1  // Accept: different values
ELSE
    RETURN 0  // Reject: duplicate (fallthrough)
```

**ValidateSimpleDuplicate** (Task 7 - ERB lines 840-851, 908-913):

Simple equality check:
```
IF (candidateValue != otherSlotValue)
    RETURN 1  // Accept
ELSE
    RETURN 0  // Reject
```

**CalculatePenisSize** (Task 8 - ERB line 712):
```csharp
public int CalculatePenisSize(int rawInput) => rawInput - 2;
```

### Unit Test Coverage (Task 10)

Create tests in `Era.Core.Tests/State/BodySettingsTests.cs`:

**Deduplication tests** (AC#8):
- Hair: HairOption1=5, HairOption2=5 → HairOption2 cleared to 0
- Eye: EyeOption1=3, EyeOption2=3 → EyeOption2 cleared
- Body cascading: BodyOption1=10, BodyOption3=10 → BodyOption3 cleared (preserving lower slot)
- V/P exclusion: VOption1=5, VOption2=5 → VOption2 NOT cleared (V/P excluded from dedup phase)

**Compaction tests** (AC#9):
- Body: BodyOption4=50, slots 1-3 empty → BodyOption4 moves to slot1
- Hair: HairOption2=10, HairOption1=0 → HairOption2 moves to slot1
- Gap elimination: BodyOption1=10, BodyOption2=0, BodyOption3=30, BodyOption4=0 → no gaps after compaction
- Negative guard: BodyOption1=-1, BodyOption2=0, BodyOption3=0, BodyOption4=50 → BodyOption4 moves to slot1 (replacing -1 since <= 0 is empty for compaction), BodyOption4=0. Verifies > 0 / <= 0 guard distinction from dedup != 0.

**No sync verification** (AC#10): Grep pattern `SyncDerivedValues` expects 0 matches in BodySettings.cs

**V/A tightness mapping** (AC#11, AC#12):
- Test all 5 levels for V: 0→0, 1→100, 2→250, 3→450, 4→700
- Test all 5 levels for A: same mapping, different BaseIndex target

**P size offset** (AC#13):
- CalculatePenisSize(5) == 3
- CalculatePenisSize(2) == 0
- CalculatePenisSize(0) == -2

**Mutual exclusion** (AC#14, AC#15):
- Reject: candidate=5, otherSlot=7 (both in range 1-9) → return 0
- Accept: candidate=9, otherSlot=10 (different groups) → return 1
- Boundary crossings: 9 vs 10, 29 vs 30, 49 vs 50, 54 vs 55

**P exclusion** (AC#16):
- POption2=1, candidate=2 → return 0 (both in range 1-2)
- POption2=3, candidate=1 → return 1 (POption2 outside range 1-2)
- POption1=3, candidate=3 → return 0 (duplicate)

**Simple duplicate** (AC#22):
- ValidateSimpleDuplicate(5, 5) → 0 (reject)
- ValidateSimpleDuplicate(5, 3) → 1 (accept)
- ValidateSimpleDuplicate(0, 5) → 1 (accept)
- ValidateSimpleDuplicate(5, 0) → 1 (accept)

### Build Verification Steps (Task 12)

**Build command** (AC#20):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'
```

**Test command** (AC#21):
```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter BodySettings'
```

### SKILL.md Update (Task 13)

Add to `.claude/skills/engine-dev/SKILL.md` in the Core Interfaces section or Phase 20 subsection:

```markdown
**IBodySettings** (`Era.Core/Interfaces/IBodySettings.cs`):
- `void Tidy(int characterId)`: Deduplication + mutual exclusion validation + slot compaction for body options (4 slots), hair/eye/V/P option pairs (2 slots each). Does NOT sync derived values (behavioral difference from IVisitorSettings).
- `int ValidateBodyOption(int characterId, int candidateValue, int slot)`: 5-range-group mutual exclusion (1-9, 10-29, 30-49, 50-54, 55-59). Returns 1 (accept) or 0 (reject).
- `int ValidatePenisOption(int characterId, int candidateValue, int slot)`: P-specific range 1-2 mutual exclusion + duplicate check. Returns 1 (accept) or 0 (reject).
- `int ValidateSimpleDuplicate(int candidateValue, int otherSlotValue)`: Pairwise equality check for hair/eye/V option pairs. Returns 1 (different=accept) or 0 (same=reject).
- `int CalculatePenisSize(int rawInput)`: Returns rawInput - 2 (ERB line 712 offset).
- `int GetTightnessBaseValue(int selectionIndex)`: Non-linear V/A tightness BASE mapping (0→0, 1→100, 2→250, 3→450, 4→700).
```

### Success Criteria

- All ACs pass
- Zero technical debt (no TODO/FIXME/HACK)
- Build succeeds with zero warnings (TreatWarningsAsErrors=true)
- All unit tests pass

### Error Handling

**CFLAG index mismatch** (C14): If verification reveals indices don't match BodySettings.cs verified 500-512, 515-518:
1. STOP implementation
2. Document actual indices from CSV/YAML
3. Ask user: Use CSV values or verify ERB assumptions?
4. Update CharacterFlagIndex constants per user decision

**Missing CFLAG entries** (Ｖ位置, 目つき possibly missing from CFLAG.yaml):
1. Grep CFLAG.yaml for "Ｖ位置" and "目つき"
2. If missing: Add to Mandatory Handoffs with destination TBD
3. Proceed with implementation using verified indices only

**F781 pattern divergence**: If F781 implementation changes during F779 execution:
1. STOP and report divergence to user
2. User decides: Adapt F779 or keep original pattern
3. Document decision in Review Notes

---

## Mandatory Handoffs
<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| BodyDetailInit delegate-to-IVariableStore migration | Dual access pattern (Func/Action delegates + IVariableStore) in same class is technical debt | Feature | F796 | N/A |
| ERB @体詳細整頓 delegation to C# BodySettings.Tidy | No engine mechanism exists for ERB function → C# method override. F781 also has no ERB delegation. Requires engine-side ERB function override infrastructure first | Phase | Phase 20 (body settings ERB wiring) | N/A — requires engine infrastructure |

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
| 2026-02-17 | Phase 1 | initializer | Status [REVIEWED]→[WIP] | SUCCESS |
| 2026-02-17 | Phase 2 | explorer | Codebase investigation | SUCCESS |
| 2026-02-17 | Phase 3 | implementer | Tasks 1-3 (constants+interface+stubs) | SUCCESS |
| 2026-02-17 | Phase 3 | implementer | Task 10 (TDD RED tests, 48 tests) | SUCCESS |
| 2026-02-17 | Phase 4 | implementer | Tasks 4-9 (implementation) | 47/48 PASS |
| 2026-02-17 | DEVIATION | orchestrator | Test fix: dedup test values 10,20→5,35 | Test used same-range values triggering Phase 2 exclusion |
| 2026-02-17 | Phase 4 | orchestrator | Re-run tests after fix | 48/48 PASS |
| 2026-02-17 | Phase 7 | ac-static-verifier | Code AC#10 FAIL: comment contained SyncDerivedValues | Removed comment, re-verified 13/13 PASS |
| 2026-02-17 | Phase 7 | verify-logs | Log verification | OK:15/15 |
| 2026-02-17 | DEVIATION | feature-reviewer | NEEDS_REVISION: null-forgiving _variables! operator | Added RequireVariables() guard method |
| 2026-02-17 | Phase 8 | orchestrator | Fix verified: build 0 errors, 48/48 tests PASS | SUCCESS |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase1-RefCheck iter1: Links section | Added F788-F793 infrastructure features to Links section (orphan reference fix)
- [fix] Phase2-Uncertain iter1: line 312 | Added missing --- separator before ## Acceptance Criteria (template compliance)
- [fix] Phase2-Review iter1: Implementation Contract Tidy phase 2 | Corrected slot validation to exclude slot1 (body) and POption1 (P) per ERB lines 772-780
- [fix] Phase2-Review iter1: Pre-conditions line 716 | Corrected false claim about BodySettings constructor already existing
- [resolved-applied] Phase2-Pending iter1: [AC-002] V/A tightness mapping (GetTightnessBaseValue) is private with no public interface method. AC#11/AC#12 expect testable tightness mapping but no IBodySettings method exposes it. Decision needed: (A) Add public GetTightnessBaseValue to IBodySettings (update AC#1 count 6→7), or (B) Add separate SetVTightness/SetATightness methods (count 6→8), or (C) Remove AC#11/AC#12/Task 9 and keep V/A tightness entirely in ERB.
- [fix] Phase2-Review iter2: line 648-649 | Added missing --- separator before ## Tasks (template compliance)
- [fix] Phase2-Review iter2: line 313-314 | Added blank line between --- and ## Acceptance Criteria (markdown parsing safety)
- [fix] Phase2-Review iter2: fc-phase comments | Added blank lines after fc-phase-N-completed comments before headings (3 locations)
- [fix] Phase2-Review iter2: Tidy Phase 1 dedup | Removed V/P option dedup from Phase 1 (not in ERB lines 745-770, only hair/eye/body)
- [fix] Phase2-Review iter2: AC#8 Expected | Added zero-guard test case for body dedup (BodyOption1=0, BodyOption2=0 → no dedup)
- [resolved-applied] Phase3-Maintainability iter3: [CON-003] F779 ValidateBodyOption uses characterId+IVariableStore while F781 uses parameterless IVisitorVariables. Decision: Option C (pass slot values as parameters). Key Decisions + Upstream Issues updated.
- [fix] Phase3-Maintainability iter3: Mandatory Handoffs | Added BodyDetailInit delegate-to-IVariableStore migration handoff to F778
- [fix] Phase3-Maintainability iter3: Key Decisions | Added DI pattern decision documenting dual-pattern as intentional/temporary
- [fix] Phase3-Maintainability iter3: AC#24 + Task 14 | Added DI registration verification AC and Task
- [fix] Phase3-Maintainability iter3: AC#22 Rationale | Added clarification about ValidateSimpleDuplicate(0,0) context in Tidy
- [fix] Phase2-Review iter4: AC#11,AC#12,Technical Design,Implementation Contract,Task 9,AC Coverage,Unit Tests | Corrected inverted V/A tightness mapping (was 0→700...4→0, correct is 0→0...4→700) verified against ERB 締り具合名称 function
- [fix] Phase2-Review iter5: lines 518,947 | Updated AC count from 23 to 24 (AC#24 was added in iter3)
- [fix] Phase2-Uncertain iter5: AC#25 + Task 4 AC coverage | Added Tidy anchor slot preservation AC verifying slot1/POption1 not validated during mutual exclusion
- [fix] Phase2-Review iter6: AC Coverage table | Added AC#24 and AC#25 rows to Technical Design AC Coverage section
- [fix] Phase2-Review iter6: Implementation Contract | Added Task 14 to Phase 5 (Verification)
- [fix] Phase2-Review iter6: Approach line 524 | Updated AC count from 24 to 25
- [fix] Phase2-Review iter6: Task 10 AC# column | Added AC#25 to test creation task coverage
- [fix] Phase5-Feasibility iter7: Scope Reference | Corrected file path from Game/ERB/体関係/体設定.ERB to Game/ERB/体設定.ERB (all occurrences)
- [fix] Phase2-Uncertain iter8: Goal Coverage table | Clarified Goal item 2 description to specify extractable business logic subset (not all 23 menu items), noting direct CFLAG assignments remain in ERB per C12
- [fix] Phase2-Uncertain iter9: Mandatory Handoffs | Added template comments (CRITICAL, Option A/B/C, Validation, DRAFT Creation Checklist) per feature-template.md
- [fix] Phase2-Review iter9: ## Scope Reference | Demoted from ## to ### (non-template top-level section, following F775/F781 pattern)
- [fix] Phase2-Review iter10: Implementation Contract + AC#9 | Added explicit compaction guard conditions (> 0 / <= 0 vs dedup != 0) and negative value test case per ERB lines 783-833
- [resolved-applied] Phase3-Maintainability iter11: [CON-002] IBodySettings methods use int characterId. Decision: Option B (keep int, consistent with F781/existing). Key Decisions updated.
- [fix] Phase3-Maintainability iter11: Upstream Issues | Added F779 Tidy unified vs F781 3 separate methods design constraint for F794
- [fix] Phase3-Maintainability iter11: Feasibility Verdict | Updated from NEEDS_REVISION to FEASIBLE (conditional) with Task 1 gate and confirmed 4-function boundary
- [resolved-applied] Phase2-Review iter12: [AC-003] AC#18/AC#19 verify only EXISTENCE of CharacterFlagIndex/BaseIndex constants but not their INDEX VALUES. Decision: (A/B) AC#18 matcher changed to `HairLength.*new\(1175\)`, AC#19 to `VLooseness.*new\(27\)`. Values verified from CFLAG.yaml:785 and BASE.yaml:48-49.
- [resolved-applied] Phase2-Review iter12: [AC-004] VPosition (Ｖ位置) and EyeExpression (目つき) missing from CFLAG.yaml. Task 1 Error Handling says to add to Mandatory Handoffs, but no AC verifies this fallback. Decision: Option A — Task 1 uses CFLAG.csv as fallback. VPosition=402, EyeExpression=508 confirmed.
- [fix] Phase2-Review iter12: Goal text line 43 | Clarified from '23 menu items' to 'extractable business logic subset of 23 menu items' with explicit list, matching Goal Coverage table
- [resolved-applied] Phase2-Review iter13: [TASK-001] No Task or AC covers ERB file delegation. Technical Design states ERB delegates to C# (line 518) and Impact Analysis marks 体設定.ERB as HIGH, but all 14 Tasks only modify C# files. Decision needed: (A) Add ERB delegation Tasks (modify @体詳細整頓 and @体詳細設定２ to call C# methods, with ACs), or (B) Redefine Goal as 'extract business logic to C# for testing' and add Mandatory Handoff for ERB wiring to a successor feature. Decision: Option A — AC#28 + Task 16 added for @体詳細整頓 delegation.
- [fix] Phase2-Review iter13: Implementation Contract | Reordered Phase 3→4: Phase 3 = Task 10 (RED: tests), Phase 4 = Tasks 4-9 (GREEN: implementation) per TDD mandate
- [fix] Phase2-Uncertain iter13: AC#10 Rationale | Added implementation note prohibiting string 'SyncDerivedValues' in comments within BodySettings.cs
- [fix] Phase1-RefCheck iter1: Implementation Contract line 749 | Corrected CFLAG.yaml path from Game/CSV/CFLAGname.csv or Game/CSV/YAML/CFLAG.yaml to Game/data/CFLAG.yaml
- [info] Phase1-DriftChecked: F781 (Related)
- [info] Phase1-DriftChecked: F778 (Sibling)
- [fix] Phase1-DriftCheck iter1: Feasibility/Constraints/Risks/Baseline/Technical Design | F778 drift: corrected BodyOption indices (513-516→515-518), updated baseline (348→492 lines, 5→1 methods), updated CFLAG alignment to FEASIBLE, downgraded risk
- [fix] Phase3-Maintainability iter1: AC#8 Expected + Unit Test Coverage | Added V/P dedup exclusion negative test case (VOption1==VOption2 NOT cleared by dedup phase per ERB lines 745-770)
- [fix] Phase3-Maintainability iter2: Implementation Contract line 800 | Corrected constructor comment from '(already exists)' to '(to be added in Task 3 or Task 4)' matching Pre-conditions line 737
- [fix] Phase3-Maintainability iter2: Key Decision BASE index source | Clarified Option C from 'Verify at runtime via ICsvNameResolver' to 'Verify against CSV at implementation time, then define as compile-time constants' matching actual approach
- [fix] Phase2-Review iter3: AC#25 Expected + Implementation Contract Tidy Phase 2 | Added V option exclusion from mutual exclusion validation (V options only in Phase 3 compaction per ERB lines 772-780)
- [fix] Phase2-Uncertain iter3: C13 constraint | Updated title and details from 'Hair color sync' to 'Hair and skin color sync' adding ERB:677-678 skin sync reference
- [fix] Phase2-Review iter4: AC#26 + Task 10 + Goal Coverage + AC Coverage | Added Tidy Phase 2 cascading mutual exclusion AC verifying sequential slot4→slot3→slot2 dependency with CFLAG side effects
- [fix] Phase2-Review iter4: Goal line 43 | Clarified from 'Migrate the 4 functions' to 'Migrate business logic from 3 of 4 functions' since @体詳細設定１ has no extractable logic
- [fix] Phase2-Review iter5: C13 AC Impact | Updated from 'Selection handler AC must verify sync' to 'Derived sync remains in ERB; no C# AC required' since direct CFLAG assignments stay in ERB per Goal
- [fix] Phase2-Review iter5: Unit Test Coverage Compaction | Added negative guard test case (BodyOption1=-1 treated as empty by compaction's <= 0 guard, verifying distinction from dedup's != 0)
- [fix] Phase2-Review iter5: Philosophy Derivation row 1 | Corrected from 'all 4 functions' to '3 functions with extractable logic' matching updated Goal text
- [fix] Phase2-Review iter6: AC#27 + Task 15 + Goal Coverage + Implementation Contract | Added engine.Tests/ build AC and Task for BodySettings constructor breaking change (3 call sites: GameInitializationTests, HeadlessIntegrationTests, StateSettingsTests)
- [fix] Phase1-RefCheck iter1: Summary | Replaced nonexistent '色関係.ERB' reference with 'PRINT_STATE.ERB display functions' (file does not exist in codebase)
- [resolved-applied] Phase1-CodebaseDrift iter1: [DOC-001] F778 [DONE] resolved CFLAG index discrepancy (500-516 → 500-512, 515-518). Multiple sections reference stale '500-516': Feasibility Assessment (NEEDS_REVISION row), Technical Constraints (CFLAG index discrepancy row), Risks (CFLAG index mismatch row), AC Design Constraints C14, AC#18 Details, Baseline Measurement rows 1-2, Feasibility Verdict (conditional → unconditional). All are doc fixes verifiable against BodySettings.cs source.
- [info] Phase1-DriftChecked: F778 (Related)
- [resolved-applied] Phase3-Maintainability iter1: [DEP-001] Mandatory Handoff destination F778 is [DONE]. Decision: Option A — Create new feature [DRAFT] for BodyDetailInit delegate→IVariableStore migration. Deferred to Step 6.3.
- [fix] Phase3-Maintainability iter1: AC#24 | Changed matcher from `matches IBodySettings` in ServiceCollectionExtensions.cs (already passes without changes) to `matches BodySettings\(IVariableStore` in BodySettings.cs (verifies actual constructor change)
- [fix] Phase5-Feasibility iter1: C14/AC#18/Approach/Technical Design/Implementation Contract/Error Handling | Updated remaining stale '500-516' references to '500-512, 515-518' (6 locations) per F778 drift correction
- [fix] PostLoop-UserFix iter1: [AC-002] | Option A applied: GetTightnessBaseValue promoted from private to public IBodySettings method. AC#1 count 6→7, interface/approach/design/implementation contract updated (12 locations)
- [fix] PostLoop-UserFix iter2: [CON-003] | Option C applied: pass slot values as parameters. Key Decisions + Upstream Issues updated for F794 reconciliation.
- [fix] PostLoop-UserFix iter2: [CON-002] | Option B applied: keep int characterId. Key Decisions updated.
- [fix] PostLoop-UserFix iter2: [DEP-001] | Option A applied: Created F796 [DRAFT] for BodyDetailInit migration. Mandatory Handoffs updated from F778→F796.
- [fix] PostLoop-UserFix iter3: [AC-003] | Option A applied: AC#18 matcher changed to `HairLength.*new\(1175\)`, AC#19 to `VLooseness.*new\(27\)`. Values verified from CFLAG.yaml and BASE.yaml.
- [fix] PostLoop-UserFix iter4: [AC-004] | Option A applied: Task 1 uses CFLAG.csv as fallback. VPosition=402, EyeExpression=508 confirmed. Updated Technical Design TBD values (VOption1=403, VOption2=404, PSize=406, POption1=407, POption2=408).
- [fix] PostLoop-UserFix iter2: [TASK-001] | Option A applied: AC#28 + Task 16 added for @体詳細整頓 ERB delegation to C# BodySettings.Tidy.
- [fix] PostLoop-UserFix iter2: [DEP-001] | Option A applied: Created F796 [DRAFT] for BodyDetailInit migration. Mandatory Handoffs updated from F778→F796.
- [resolved-applied] Phase2-Review iter1r: [AC-005] AC#18 expects HairLength.*new(1175) from CFLAG.yaml but Technical Design says HairLength=new(500) from CFLAG.csv. Decision: Option A — Use CFLAG.csv values (500). IVariableStore uses raw array indices (`_cflags[index.Value]`), matching CFLAG.csv. AC#18 matcher updated to `HairLength.*new\(500\)`. AC#18 Details and AC Coverage updated.
- [fix] Phase2-Review iter1r: Key Decision + Approach | Clarified ValidateBodyOption signature as F794 target (not F779 change); clarified Approach item 2 about typed constants scope
- [fix] Phase2-Review iter2r: Task 9 | Changed 'private helper' to 'public IBodySettings method' per resolved AC-002
- [fix] Phase2-Review iter2r: Approach item 8 | Clarified ERB delegation terminology: @体詳細整頓=full delegation (AC#28), @体詳細設定２/@体詳細オプション設定=inline CALLFORM
- [fix] Phase2-Review iter3r: Approach item 8 | Further clarified: @体詳細設定２/@体詳細オプション設定 remain as-is in ERB (not modified in F779). C# methods for Tidy internal use + F794 reuse.
- [fix] Phase2-Review iter3r: Implementation Contract Phase 2 | Corrected '5 new method signatures' to '6' matching AC#1 count=7 and Task 3 description
- [fix] Phase3-Maintainability iter4r: Implementation Contract | Added Phase 6 for Task 16 (ERB Delegation)
- [fix] Phase3-Maintainability iter4r: Task 4 AC# | Added AC#26 (Tidy Phase 2 cascading)
- [fix] Phase3-Maintainability iter4r: AC Coverage AC#23 | Corrected '5 new methods' to '6'
- [fix] Phase2-Uncertain iter1: Philosophy Derivation | Added missing AC#25,AC#26,AC#28 to row 1 and AC#23,AC#24,AC#27 to row 2 (6 ACs not traced to Philosophy claims)
- [fix] Phase2-Review iter1: ## Created | Converted to HTML comment (non-template H2 section per feature-template.md)
- [fix] Phase2-Review iter1: ## Summary | Merged into Background opening paragraph (non-template H2 section per feature-template.md)
- [fix] Phase2-Review iter2: AC Coverage AC#18 | Updated row to match AC#18 expected pattern (HairLength=1175 per CFLAG.yaml, not stale 500)
- [resolved-applied] Phase2-Review iter2: [AC-006] AC#28 grep pattern `CALL IBodySettings\.Tidy|CALLFORM.*Tidy|BodySettings.*Tidy` uses ERB CALL syntax but ERB CALL can only invoke ERB @functions, not C# methods. Decision: Option A — AC#28 and Task#16 removed. ERB delegation deferred to Mandatory Handoffs (Phase 20). Investigation confirmed: no engine ERB function override mechanism exists; F781 also has no ERB delegation.
- [fix] Phase1-RefCheck iter1: Links section | Added F796 to Links section (orphan reference from Mandatory Handoffs)
- [fix] Phase2-Review iter1: Interfaces/Data Structures code block | Added missing GetTightnessBaseValue method to first IBodySettings code block (lines 587-603) matching authoritative block at lines 797-819
- [fix] PostLoop-UserFix iter2: [AC-005] | Option A applied: CFLAG.csv values (500) for CharacterFlagIndex. AC#18 matcher `HairLength.*new\(1175\)` → `HairLength.*new\(500\)`. AC#18 Details and AC Coverage updated. IVariableStore uses raw array indices.
- [fix] PostLoop-UserFix iter2: [AC-006] | Option A applied: AC#28 and Task#16 removed (ERB delegation). ERB @体詳細整頓 delegation deferred to Mandatory Handoffs (Phase 20). No engine ERB function override mechanism exists.
- [fix] Phase2-Review iter1: Approach line 528 | Corrected stale 'all 28 ACs' to 'all 27 ACs' (AC#28 removed per AC-006)
- [fix] Phase2-Review iter1: Mandatory Handoffs F796 | Changed Creation Task from '(created by FL POST-LOOP)' to 'N/A' (Option B: F796 exists)

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning (parent)
- [Related: F778](feature-778.md) - Body Initialization (sibling)
- [Successor: F780](feature-780.md) - Genetics & Growth (successor calling @体詳細整頓)
- [Related: F781](feature-781.md) - Visitor Settings (parallel pattern establishing logic/UI separation)
- [Successor: F794](feature-794.md) - Shared Body Option Validation Abstraction (successor)
- [Related: F788](feature-788.md) - IStringVariables Phase 20 Extensions (infrastructure)
- [Related: F789](feature-789.md) - IStringVariables + I3DArrayVariables Phase 20 Extensions (infrastructure)
- [Related: F790](feature-790.md) - IEngineVariables + ICsvNameResolver (infrastructure)
- [Related: F791](feature-791.md) - IGameState mode transitions + IEntryPointRegistry (infrastructure)
- [Related: F792](feature-792.md) - ac-static-verifier count_equals matcher (infrastructure)
- [Related: F793](feature-793.md) - GameStateImpl engine-side delegation (infrastructure)
- [Successor: F796](feature-796.md) - BodyDetailInit delegate→IVariableStore migration (Mandatory Handoff)
