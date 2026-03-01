# Feature 777: Customization (SHOP_CUSTOM.ERB + CHARA_CUSTUM.ERB + TALENTCOPY.ERB)

## Status: [DONE]
<!-- finalizer: 2026-02-18 -->

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

---

<!-- fc-phase-1-completed -->

## Background

### Philosophy (Mid-term Vision)

Behavior Preservation - C# migration must preserve all ERB behavioral invariants to ensure game-logic correctness. This ensures behavioral equivalence for core logic (display delegation explicitly excluded), interface completeness for required and forward-looking operations, and zero technical debt in migrated code. SSOT: `designs/phases/phase-20-27-game-systems.md` Phase 20 section defines the scope; F647 decomposed it into actionable sub-features.

### Problem (Current Issue)

The character customization subsystem (SHOP_CUSTOM.ERB, CHARA_CUSTUM.ERB, TALENTCOPY.ERB) cannot be migrated to C# because Phase 20 interfaces were designed for read-only consumers and lack the write accessors and input handling methods that the customization subsystem requires. Specifically, IEngineVariables has no NAME/CALLNAME setters or NO variable access (`Era.Core/Interfaces/IEngineVariables.cs:11-48`), and IInputHandler has no ONEINPUT equivalent (`Era.Core/Input/IInputHandler.cs:1-28`). Additionally, the 202-line GET_TALENTNAME function (`PRINT_STATE.ERB:316-518`) is a value-dependent name resolver with no C# equivalent that must be extracted.

### Goal (What to Achieve)

Migrate SHOP_CUSTOM.ERB, CHARA_CUSTUM.ERB, and TALENTCOPY.ERB to C# as `Era.Core/Character/CharacterCustomizer.cs`, `Era.Core/Character/TalentCopier.cs`, and `Era.Core/Character/TalentNameResolver.cs` (extracting GET_TALENTNAME from PRINT_STATE.ERB:316-518) (per `phase-20-27-game-systems.md:75-76`), extending IEngineVariables with setters and missing accessors (TARGET, PLAYER getter/setter pairs, SetCharacterNo), ICsvNameResolver with GetTalentName (CSV array accessor for CASEELSE fallback), and IInputHandler with ONEINPUT support, while injecting TalentNameResolver into CharacterCustomizer for value-dependent talent name resolution and preserving the 4-entry-point architecture callable from SYSTEM.ERB.

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why can't F777 be migrated? | Interface methods are missing across 2 interfaces (IEngineVariables: 8 setters/accessors for NAME, CALLNAME, MASTER, TARGET, PLAYER, SetCharacterNo (GetCharacterNo already exists from F776); IInputHandler: ONEINPUT) | `Era.Core/Interfaces/IEngineVariables.cs:11-59`, `Era.Core/Input/IInputHandler.cs:1-28` |
| 2 | Why are these methods missing? | IEngineVariables was designed read-only (getters only), IInputHandler has no single-character input mode | `IEngineVariables.cs:40-44` (GetName/GetCallName only), `IInputHandler.cs:1-28` |
| 3 | Why were they designed read-only/incomplete? | F790 designed these interfaces for known consumers at the time (shop display, state queries) which only needed read access | `phase-20-27-game-systems.md:75-76` (deliverables specified but interface scope not) |
| 4 | Why were F777 requirements not anticipated? | F777 was decomposed separately from the interface features (F788-F791); no cross-feature dependency analysis identified write requirements | `feature-647.md` (Phase 20 decomposition) |
| 5 | Why (Root)? | Incremental interface design in Phase 20 sub-features (F788-F791) did not perform forward-looking dependency analysis for sibling features (F774-F782), resulting in interfaces that serve existing consumers but block future ones | Architecture pattern: interfaces designed per-consumer rather than per-domain |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | Customization system remains in ERB, cannot be migrated | Phase 20 interfaces lack write accessors and name resolution methods needed by customization subsystem |
| Where | SHOP_CUSTOM.ERB, CHARA_CUSTUM.ERB, TALENTCOPY.ERB | IEngineVariables, ICsvNameResolver, IInputHandler interface definitions |
| Fix | Leave in ERB or use workarounds | Extend 3 interfaces with missing methods; extract GET_TALENTNAME as TalentNameResolver |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Phase 20 Planning (parent, decomposed this feature) |
| F774 | [DONE] | Shop Core (SHOP.ERB + SHOP2.ERB) - same subsystem |
| F775 | [DONE] | Collection (SHOP_COLLECTION.ERB) - sibling Phase 20 |
| F776 | [DONE] | Items (SHOP_ITEM.ERB + アイテム説明.ERB) - sibling Phase 20 |
| F788 | [DONE] | IConsoleOutput Extensions (DRAWLINE, BAR, PrintColumnLeft) |
| F789 | [DONE] | IStringVariables + I3DArrayVariables |
| F790 | [DONE] | IEngineVariables + ICsvNameResolver (needs setters extension) |
| F791 | [DONE] | IGameState mode transitions |
| F368 | [DONE] | Phase 3 partial migration (CharacterSetup.cs, TalentManager.cs - DTO pattern) |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Interface Extensions | FEASIBLE | IEngineVariables requires 8 additive methods (SetName/SetCallName/SetMaster/Get+SetTarget/Get+SetPlayer/SetCharacterNo); GetCharacterNo already exists from F776; IInputHandler requires RequestOneInput. All additive-only; existing 13 methods preserved (AC#13). Extension pattern proven by F788/F790. Existing ICharacterManager, IStyleManager, IVariableStore already provide needed methods |
| GET_TALENTNAME Extraction | FEASIBLE | 202-line SELECTCASE (`PRINT_STATE.ERB:316-518`) maps cleanly to C# switch expression with CASEELSE fallback to TALENTNAME CSV via ICsvNameResolver.GetTalentName |
| F368 Integration | FEASIBLE | Pre-existing TalentManager.CopyTalents (`Era.Core/Common/TalentManager.cs:20`) and CharacterSetup.cs use DTOs; can coexist or be replaced within F777 scope |
| Call Chain Preservation | FEASIBLE | 4 entry points in SYSTEM.ERB are direct function calls; C# equivalents can maintain same signatures |
| ICommonFunctions Bridge | FEASIBLE | HasPenis/HasVagina/IsFemale take genderValue (`ICommonFunctions.cs:10-12`); two-step pattern (get gender, pass to function) is straightforward |

**Verdict**: FEASIBLE

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| IEngineVariables consumers | HIGH | Adding setter methods; existing 13 methods unaffected (additive change) |
| ICsvNameResolver consumers | LOW | Adding 1 new method (GetTalentName - CSV array accessor); existing 4 methods unaffected (additive change) |
| IInputHandler consumers | MEDIUM | Adding ONEINPUT support; existing RequestNumericInput/RequestStringInput unaffected |
| SYSTEM.ERB | HIGH | 4 entry points must be redirected to C# implementations |
| PRINT_STATE.ERB | MEDIUM | GET_TALENTNAME extracted; PRINT_STATE_ABL/TALENT/EXP called but can be stubbed |
| ICharacterManager consumers | LOW | AddChara/DelChara already exist; used as-is (no interface changes needed) |
| IStyleManager consumers | LOW | SetColor/ResetColor already exist; used as-is (no interface changes needed) |
| IVariableStore consumers | LOW | All needed accessors (TALENT, ABL, EXP, BASE, MAXBASE, CFLAG) already exist |
| F368 CharacterSetup.cs / TalentManager.cs | MEDIUM | Pre-existing DTO-based code must be integrated or superseded |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| TalentNameResolver injection into CharacterCustomizer required | `PRINT_STATE.ERB:316-518` GET_TALENTNAME | Provides value-dependent talent name resolution; CharacterCustomizer must inject TalentNameResolver for CUSTOM_TERMINAL display |
| IEngineVariables lacks setters (NAME, CALLNAME, MASTER) and entirely lacks TARGET/PLAYER (no getters or setters) | `Era.Core/Interfaces/IEngineVariables.cs:11-59` | Blocks NAME_CUSTOM writes and REVERSEMODE_1 reordering |
| IEngineVariables has GetCharacterNo (from F776) but lacks SetCharacterNo setter | `Era.Core/Interfaces/IEngineVariables.cs:58` | Blocks SHOP_CUSTOM NO writes (character identification reads work via GetCharacterNo) |
| IInputHandler lacks ONEINPUT equivalent | `Era.Core/Input/IInputHandler.cs:1-28` | Blocks CHARA_CUSTUM single-character input |
| GET_TALENTNAME is 202-line SELECTCASE | `PRINT_STATE.ERB:316-518` | Must be extracted as TalentNameResolver; not a simple CSV lookup |
| ICommonFunctions takes genderValue not character index | `Era.Core/Interfaces/ICommonFunctions.cs:10-12` | Requires two-step call pattern (get gender, then call) |
| IConsoleOutput lacks PRINTFORML | `Era.Core/Interfaces/IConsoleOutput.cs:1-71` | Workaround: PrintLine + string interpolation |
| 開始時人数 = 14 compile-time constant | `DIM.ERH:56` | Must be named constant in C# |
| Architecture deliverable names | `phase-20-27-game-systems.md:75-76` | CharacterCustomizer.cs, TalentCopier.cs (not CustomizationSystem.cs) |
| REVERSEMODE_1 requires ADDCHARA/DELCHARA | `TALENTCOPY.ERB:30,36,45,48,53` via `Era.Core/Interfaces/ICharacterManager.cs:11-14` | ICharacterManager.AddChara/DelChara already exist; must be injected as dependency |
| CFLAG:311 sentinel value > InitialCharacterCount (C19) | `TALENTCOPY.ERB:41` | REVERSEMODE_1 sets CFLAG:MASTER:311=15 (> 開始時人数=14) as sentinel during reorder. Must use InitialCharacterCount+1 or documented constant |
| SHOP_CUSTOM requires SETCOLOR/RESETCOLOR | `SHOP_CUSTOM.ERB:271-391` (19 call sites) via `Era.Core/Interfaces/IStyleManager.cs:10-20` | IStyleManager already exists; must be injected as dependency |
| IVariableStore provides TALENT/ABL/EXP/BASE/MAXBASE/CFLAG access | `Era.Core/Interfaces/IVariableStore.cs:15-103` | Primary data interface for character variable reads/writes; existing dependency |
| TARGET and PLAYER variables absent from IEngineVariables | `Era.Core/Interfaces/IEngineVariables.cs:11-59` (no getter or setter) | Both SetTarget and SetPlayer needed (TALENTCOPY.ERB:29,39); also need GetTarget if read |
| F368 DTO-interface impedance | `Era.Core/Common/CharacterSetup.cs`, `Era.Core/Common/TalentManager.cs` | Pre-Phase 20 dictionary-based DTOs disconnected from IVariableStore |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Interface extension scope creep | HIGH | MEDIUM | Scope extensions strictly within F777 requirements; add only methods needed |
| GET_TALENTNAME shared concern | MEDIUM | MEDIUM | Extract as standalone TalentNameResolver.cs usable by future features |
| REVERSEMODE_1 complexity (character deletion/reorder) | MEDIUM | HIGH | Equivalence testing against ERB implementation with boundary cases |
| F368 integration conflict | MEDIUM | MEDIUM | Architectural decision: integrate TalentManager.CopyTalents or replace with IVariableStore-based implementation |
| ONEINPUT behavioral difference | LOW | LOW | Approximate with RequestStringInput + single-char validation |
| Backward compatibility of extended interfaces | LOW | MEDIUM | Additive-only changes; verify existing consumers unaffected |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ERB source line count | `wc -l Game/ERB/SHOP_CUSTOM.ERB Game/ERB/CHARA_CUSTUM.ERB Game/ERB/TALENTCOPY.ERB` | ~610 lines total | 472 + 28 + 110 |
| Existing unit tests | `dotnet test Era.Core.Tests/ --filter "CharacterSetup\|TalentManager"` | 0 tests (no pre-existing tests for CharacterCustomizer/TalentCopier) | F368 pre-existing tests |
| IEngineVariables method count | `grep -c ";" Era.Core/Interfaces/IEngineVariables.cs` | 13 methods | Baseline for backward compat AC (includes SetMoney and GetCharacterNo from F776) |
| ICsvNameResolver method count | `grep -c ";" Era.Core/Interfaces/ICsvNameResolver.cs` | 4 existing methods | Baseline for backward compat AC (AC#16 counts 4 existing) |

**Baseline File**: `.tmp/baseline-777.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | 4 entry points callable from SYSTEM.ERB | `SYSTEM.ERB:69,86,122,127` | Verify all 4 entry points (CUSTOM_CHARAMAKE, CHARA_CUSTUM, VIRGIN_CUSTOM, REVERSEMODE_1) |
| C2 | CUSTOM_TERMINAL call chain integrity | `CHARA_CUSTUM.ERB:14` calls CUSTOM_TERMINAL | Verify CHARA_CUSTUM delegates to CUSTOM_TERMINAL |
| C3 | Gender-based value normalization | `SHOP_CUSTOM.ERB:19-67` | Test 3 gender paths (male, female, both) for value normalization (zeroing incompatible ABL/EXP/TALENT based on gender, 処女 state handling), mutual exclusion invariants (肉便器/公衆便所, 肉便器/NTR) |
| C4 | TALENT_CUSTOM 4-page pagination | `SHOP_CUSTOM.ERB:302-361` | Verify pagination produces 4 pages of talent display |
| C5 | COPY_CUSTOM talent range 0-190 (191 talents) | `TALENTCOPY.ERB:94` (`FOR LOCAL,0,191` = exclusive upper bound, copies indices 0-190) | Verify 191-talent copy range; note pre-existing discrepancy with F368 TalentManager.CopyTalents (`TalentManager.cs:32` copies 192 elements via `Array.Copy`) |
| C6 | REVERSEMODE_1 character reorder via CFLAG:311 | `TALENTCOPY.ERB:36-50` | Verify CFLAG:311-based reordering logic |
| C7 | NAME_CUSTOM empty input for player (ARG==0) keeps current name | `SHOP_CUSTOM.ERB:244-248` | Test both empty and non-empty input paths for player character |
| C8 | Zero technical debt | `designs/full-csharp-architecture.md` | AC must verify no TODO/FIXME/HACK in new files |
| C9 | Equivalence tests required | `designs/full-csharp-architecture.md` | AC must include equivalence verification against ERB behavior |
| C10 | Color constants for disabled items | `ColorSettings.erh:27,31` | Verify SetColor uses correct hex values (0x999999 for disabled) |
| C11 | CUSTOM_TERMINAL excluded talent indices | `SHOP_CUSTOM.ERB:146,313` | Talents 3-5, 9, 34, 38, 72-76, 84, 85, 153-155 must be excluded correctly |
| C12 | VIRGIN_CUSTOM gender restriction | `SHOP_CUSTOM.ERB` | Only affects IS_FEMALE characters |
| C13 | CLOTHES_CUSTOM player-only restriction | `SHOP_CUSTOM.ERB` | Only applies when NO:ARG==0 (player character) |
| C14 | IEngineVariables backward compatibility | Interface Dependency Scan | Existing 13 methods (including GetCharacterNo from F776) must remain unchanged after extension |
| C15 | ICsvNameResolver backward compatibility | Interface Dependency Scan | Existing 4 methods must remain unchanged after extension |
| C16 | IInputHandler backward compatibility | Interface Dependency Scan | Existing methods must remain unchanged after extension |
| C17 | 開始時人数 = 14 as named constant | `DIM.ERH:56` | Must not be magic number; use named constant |
| C18 | COPY_CUSTOM source character bound | `TALENTCOPY.ERB:86` | Source must be initial character (1 ≤ source < 開始時人数); verify bound check |
| C19 | CFLAG:311 sentinel value > InitialCharacterCount | `TALENTCOPY.ERB:41` | Verify TalentCopier uses named sentinel constant, not magic number 15 |
| C20 | Gender cycling (talent 2) and 性別嗜好 cycling (talent 81) special cases | `SHOP_CUSTOM.ERB:149-161` | Two distinct cycling branches in CUSTOM_TERMINAL: gender (1→2→3→1), 性別嗜好 (-1→0→1→2→3→-1). Must not be collapsed into generic toggle or quantitative cycling |

### Constraint Details

**C1: 4 Entry Points Callable**
- **Source**: SYSTEM.ERB lines 69, 86, 122, 127 call CUSTOM_CHARAMAKE, REVERSEMODE_1, CHARA_CUSTUM, VIRGIN_CUSTOM respectively
- **Verification**: Grep SYSTEM.ERB for all CALL/JUMP to customization functions
- **AC Impact**: Each entry point needs at least one AC verifying it can be invoked

**C2: CUSTOM_TERMINAL Call Chain**
- **Source**: CHARA_CUSTUM.ERB:14 (`CALL CUSTOM_TERMINAL(RESULT)`) is the delegation call; line 20 is ONEINPUT (separate concern)
- **Verification**: Read CHARA_CUSTUM.ERB to confirm delegation pattern
- **AC Impact**: AC must verify that CharaCustom method delegates to CustomTerminal logic

**C3: Gender-Based Value Normalization**
- **Source**: SHOP_CUSTOM.ERB:19-67 normalizes character values based on gender (zeroes incompatible ABL/EXP/TALENT, handles 処女 state, enforces mutual exclusions)
- **Verification**: Read SHOP_CUSTOM.ERB gender branching logic
- **AC Impact**: Need 3 test paths (male, female, both-gender characters) for value normalization verification

**C5: COPY_CUSTOM Talent Range**
- **Source**: TALENTCOPY.ERB:94 `FOR LOCAL,0,191` uses ERA exclusive upper bound, copying indices 0-190 (191 talents). Pre-existing discrepancy: F368 `TalentManager.CopyTalents` (`TalentManager.cs:32`) uses `Array.Copy(source, target, 192)` copying 192 elements (indices 0-191).
- **Verification**: Read TALENTCOPY.ERB FOR loop bounds; compare with TalentManager.cs Array.Copy count
- **AC Impact**: AC must verify 191-talent copy (matching ERB behavior). Separately, the F368 discrepancy (192 vs 191) should be flagged as out-of-scope issue for tracking

**C6: REVERSEMODE_1 Reorder**
- **Source**: TALENTCOPY.ERB:36-50 uses CFLAG:311 to determine character reordering during deletion
- **Verification**: Read TALENTCOPY.ERB REVERSEMODE_1 function
- **AC Impact**: Most complex operation; needs equivalence test with boundary cases (first char, last char, middle char)

**C8: Zero Technical Debt**
- **Source**: Sub-Feature Requirements from architecture.md:4629-4637
- **Verification**: Grep new files for TODO|FIXME|HACK
- **AC Impact**: Mandatory AC using not_matches pattern

**C14: IEngineVariables Backward Compatibility**
- **Source**: Interface Dependency Scan confirmed 13 existing methods (`IEngineVariables.cs:14-59`, includes SetMoney and GetCharacterNo from F776)
- **Verification**: Count method signatures in IEngineVariables.cs
- **AC Impact**: count_equals AC to verify existing 13 methods preserved

**C15: ICsvNameResolver Backward Compatibility**
- **Source**: Interface Dependency Scan confirmed 4 existing methods
- **Verification**: Count method signatures in ICsvNameResolver.cs
- **AC Impact**: count_equals or similar AC to verify existing methods preserved

**C17: Named Constant for 開始時人数**
- **Source**: DIM.ERH:56 defines 開始時人数 = 14 as compile-time constant
- **Verification**: Read DIM.ERH line 56
- **AC Impact**: New C# code must use named constant, not literal 14

**C4: TALENT_CUSTOM 4-Page Pagination**
- **Source**: SHOP_CUSTOM.ERB:302-361 implements 4-page talent display with page navigation
- **Verification**: Read SHOP_CUSTOM.ERB pagination logic
- **AC Impact**: AC must verify pagination produces 4 pages of talent display

**C7: NAME_CUSTOM Empty Input Handling**
- **Source**: SHOP_CUSTOM.ERB:244-248 checks if input is empty for player character (ARG==0) and keeps current name
- **Verification**: Read SHOP_CUSTOM.ERB NAME_CUSTOM function
- **AC Impact**: Test both empty and non-empty input paths for player character

**C9: Equivalence Tests Required**
- **Source**: designs/full-csharp-architecture.md mandates equivalence testing for all migrations
- **Verification**: Check test project for equivalence test classes
- **AC Impact**: AC must include equivalence verification comparing C# output against ERB behavior

**C10: Color Constants for Disabled Items**
- **Source**: ColorSettings.erh:27,31 defines color constants (0x999999 for disabled items)
- **Verification**: Read ColorSettings.erh for hex values
- **AC Impact**: Verify SetColor uses correct hex values matching ERB color constants

**C11: CUSTOM_TERMINAL Excluded Talent Indices**
- **Source**: SHOP_CUSTOM.ERB:146,313 excludes talents 3-5, 9, 34, 38, 72-76, 84, 85, 153-155 from display
- **Verification**: Read SHOP_CUSTOM.ERB exclusion conditions
- **AC Impact**: AC must verify excluded talent list matches ERB implementation exactly

**C12: VIRGIN_CUSTOM Gender Restriction**
- **Source**: SHOP_CUSTOM.ERB restricts VIRGIN_CUSTOM to IS_FEMALE characters only
- **Verification**: Read SHOP_CUSTOM.ERB VIRGIN_CUSTOM function
- **AC Impact**: AC must verify gender restriction is enforced

**C13: CLOTHES_CUSTOM Player-Only Restriction**
- **Source**: SHOP_CUSTOM.ERB restricts CLOTHES_CUSTOM to player character only (NO:ARG==0)
- **Verification**: Read SHOP_CUSTOM.ERB CLOTHES_CUSTOM function
- **AC Impact**: AC must verify player-only restriction is enforced

**C16: IInputHandler Backward Compatibility**
- **Source**: Interface Dependency Scan for existing IInputHandler methods
- **Verification**: Count method signatures in IInputHandler.cs
- **AC Impact**: AC must verify existing methods remain unchanged after ONEINPUT extension

**C18: COPY_CUSTOM Source Character Bound**
- **Source**: TALENTCOPY.ERB:86 validates source character is initial character (1 ≤ source < 開始時人数)
- **Verification**: Read TALENTCOPY.ERB COPY_CUSTOM function bound check
- **AC Impact**: AC must verify bound check prevents invalid source character selection

**C19: CFLAG:311 Sentinel Value**
- **Source**: TALENTCOPY.ERB:41 sets CFLAG:311 sentinel value > InitialCharacterCount (15 > 14)
- **Verification**: Read TALENTCOPY.ERB REVERSEMODE_1 sentinel usage
- **AC Impact**: AC must verify named sentinel constant is used, not magic number 15

**C20: Gender and 性別嗜好 Cycling Special Cases**
- **Source**: SHOP_CUSTOM.ERB:149-161 implements two distinct cycling branches: gender (talent 2: 1→2→3→1) and 性別嗜好 (talent 81: -1→0→1→2→3→-1)
- **Verification**: Read SHOP_CUSTOM.ERB CUSTOM_TERMINAL cycling logic
- **AC Impact**: AC must verify both cycling patterns are implemented as distinct branches, not collapsed into generic toggle

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (decomposed this feature) |
| Predecessor | F788 | [DONE] | IConsoleOutput Extensions (DRAWLINE, BAR, PrintColumnLeft) |
| Predecessor | F790 | [DONE] | IEngineVariables + ICsvNameResolver (needs extension for setters only) |
| Related | F774 | [DONE] | Shop Core (SHOP.ERB + SHOP2.ERB) - same subsystem |
| Related | F775 | [DONE] | Collection (SHOP_COLLECTION.ERB) - sibling Phase 20 |
| Related | F776 | [DONE] | Items (SHOP_ITEM.ERB + アイテム説明.ERB) - sibling Phase 20 |
| Related | F789 | [DONE] | IStringVariables + I3DArrayVariables |
| Related | F791 | [DONE] | IGameState mode transitions |
| Related | F368 | [DONE] | Phase 3 partial migration (CharacterSetup.cs, TalentManager.cs) |
| Successor | F782 | [DRAFT] | Post-Phase Review Phase 20 (192 vs 191 talent discrepancy, PRINT_STATE migration) |

<!-- Dependency Types (SSOT):
| Type | Direction | Effect | Usage |
|------|-----------|--------|-------|
| Predecessor | F{ID} -> This | BLOCKING | This feature cannot start until F{ID} is [DONE]. FL Phase 0 enforces. |
| Successor | This -> F{ID} | Informational | F{ID} depends on this feature. No effect on this feature's status. |
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
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "behavioral equivalence for core logic (display delegation explicitly excluded)" | All ERB core-logic behavioral invariants preserved: gender normalization (3 paths), mutual exclusion (肉便器/公衆便所, 肉便器/NTR), cycling logic (gender 1→2→3→1, 性別嗜好 -1→0→1→2→3→-1), 191-talent copy range, character reorder via CFLAG:311, 処女 state handling; equivalence tests verify migrated behavior. Exclusion: PrintStateAbl/Talent/Exp are display-only no-op stubs (no game-state side effects) — full display equivalence tracked via F782 Successor. AC#45 verifies stubs declared; AC#82 verifies loop non-crash | AC#1, AC#2, AC#14, AC#15, AC#38, AC#45, AC#56, AC#57, AC#58, AC#61, AC#62, AC#64, AC#65, AC#66, AC#75, AC#77, AC#78, AC#79, AC#82, AC#84 |
| "interface completeness for required and forward-looking operations" | Phase 20 interfaces complete for customization subsystem; IEngineVariables extended with setters and TARGET/PLAYER accessors (GetPlayer: Zero Debt Upfront — write-only in F777 scope, read needed by future Phase 20 consumers); ICsvNameResolver extended with GetTalentName; IInputHandler extended with RequestOneInput (approximates ONEINPUT via single-char validation — functionally equivalent for CHARA_CUSTUM's [999]-only confirmation flow; full keypress semantics deferred to engine integration); no cross-feature interface gaps remain; 4 entry points preserve callable architecture; tracked cross-feature architectural debt (F782 Successor dependency covers TalentManager 192 vs TalentCopier 191 discrepancy) | AC#3, AC#4, AC#5, AC#6, AC#7, AC#8, AC#9, AC#10, AC#13, AC#18, AC#19, AC#30, AC#55 |
| "zero technical debt in migrated code" | No TODO/FIXME/HACK in new files; backward compatibility preserved for all 3 extended interfaces; named constants for magic numbers; DI registration complete. Note: RequestOneInput approximation is scoped under interface completeness (Claim 2), not technical debt — the approximation is functionally sufficient for F777's use case | AC#13, AC#16, AC#17, AC#20, AC#27, AC#37 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | CharacterCustomizer.cs exists | file | Glob(Era.Core/Character/CharacterCustomizer.cs) | exists | - | [ ] |
| 2 | TalentCopier.cs exists | file | Glob(Era.Core/Character/TalentCopier.cs) | exists | - | [ ] |
| 3 | IEngineVariables declares SetName | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | `void SetName\(int characterIndex, string` | [ ] |
| 4 | IEngineVariables declares SetCallName | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | `void SetCallName\(int characterIndex, string` | [ ] |
| 5 | IEngineVariables declares SetMaster | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | `void SetMaster\(int` | [ ] |
| 6 | IEngineVariables declares GetTarget and SetTarget | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | count_equals | `(Get|Set)Target\(` = 2 | [ ] |
| 7 | IEngineVariables declares GetPlayer and SetPlayer | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | count_equals | `(Get|Set)Player\(` = 2 | [ ] |
| 8 | IEngineVariables declares SetCharacterNo | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | `void SetCharacterNo\(int characterIndex, int value\)` | [ ] |
| 9 | Inject ICsvNameResolver into TalentNameResolver constructor | code | Grep(Era.Core/Character/TalentNameResolver.cs) | matches | `TalentNameResolver\(ICsvNameResolver` | [ ] |
| 10 | IInputHandler declares RequestOneInput | code | Grep(Era.Core/Input/IInputHandler.cs) | matches | `Result<Unit> RequestOneInput\(` | [ ] |
| 11 | TalentNameResolver.cs exists | file | Glob(Era.Core/Character/TalentNameResolver.cs) | exists | - | [ ] |
| 12 | TalentNameResolver covers talent case 130 (last case) | code | Grep(Era.Core/Character/TalentNameResolver.cs) | matches | `130.*回復` | [ ] |
| 13 | IEngineVariables backward compat (13 existing methods) | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | count_equals | `(GetResult|GetMoney|SetMoney|GetDay|GetMaster|GetAssi|GetCount|GetCharaNum|GetRandom|GetName|GetCallName|GetIsAssi|GetCharacterNo)\(` = 13 | [ ] |
| 14 | Unit tests pass | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~CharacterCustomizer | succeeds | - | [ ] |
| 15 | Equivalence tests exist for COPY_CUSTOM 191-talent range | code | Grep(Era.Core.Tests/) | matches | `Assert.*191\|Copies.*191\|count.*191\|TalentCount.*191` | [ ] |
| 16 | ICsvNameResolver backward compat (4 existing methods) | code | Grep(Era.Core/Interfaces/ICsvNameResolver.cs) | count_equals | `(GetAblName|GetExpName|GetMarkName|GetPalamName)\(` = 4 | [ ] |
| 17 | IInputHandler backward compat (existing members) | code | Grep(Era.Core/Input/IInputHandler.cs) | count_equals | `(RequestNumericInput|RequestStringInput|ProvideInput|IsWaitingForInput)\b` = 4 | [ ] |
| 18 | CharacterCustomizer has 3 public SYSTEM.ERB entry points | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | count_equals | `public.*Result.*(CustomCharamake|CharaCustum|VirginCustom)\(` = 3 | [ ] |
| 19 | TalentCopier has ReverseMode1 and CopyCustom methods | code | Grep(Era.Core/Character/TalentCopier.cs) | count_equals | `public.*Result.*(ReverseMode1|CopyCustom)\(` = 2 | [ ] |
| 20 | Zero technical debt in new/modified files | code | Grep(Era.Core/Character/CharacterCustomizer.cs,Era.Core/Character/TalentCopier.cs,Era.Core/Character/TalentNameResolver.cs,Era.Core/Constants/GameConstants.cs,Era.Core/Types/CharacterFlagIndex.cs,Era.Core/Types/TalentIndex.cs) | not_matches | `TODO|FIXME|HACK` | [ ] |
| 21 | TalentCopier copies exactly 191 talents (0-190) | code | Grep(Era.Core/Character/TalentCopier.cs) | matches | `i\s*<\s*191\|i\s*<=\s*190\|Range\(0,\s*191\)` | [ ] |
| 22 | CharacterCustomizer injects IEngineVariables | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `IEngineVariables` | [ ] |
| 23 | CharacterCustomizer injects IStyleManager | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `IStyleManager` | [ ] |
| 25 | CharacterCustomizer injects IInputHandler | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `IInputHandler` | [ ] |
| 26 | CharacterCustomizer injects ICsvNameResolver | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `ICsvNameResolver` | [ ] |
| 27 | Named constants for 開始時人数 and ReorderSentinelValue (derived) | code | Grep(Era.Core/Constants/GameConstants.cs) | count_equals | `InitialCharacterCount\s*=\s*14\|ReorderSentinelValue\s*=.*InitialCharacterCount` = 2 | [ ] |
| 28 | TalentCopier injects ICharacterManager | code | Grep(Era.Core/Character/TalentCopier.cs) | matches | `ICharacterManager` | [ ] |
| 29 | TalentCopier injects IEngineVariables | code | Grep(Era.Core/Character/TalentCopier.cs) | matches | `IEngineVariables` | [ ] |
| 30 | Build succeeds | build | dotnet build Era.Core | succeeds | - | [ ] |
| 31 | CharacterCustomizer includes ClothesCustom logic | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `ClothesCustom` | [ ] |
| 32 | TalentCopier injects IVariableStore | code | Grep(Era.Core/Character/TalentCopier.cs) | matches | `IVariableStore` | [ ] |
| 33 | CharacterCustomizer injects IVariableStore | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `IVariableStore` | [ ] |
| 34 | CharacterCustomizer injects ICommonFunctions | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `ICommonFunctions` | [ ] |
| 35 | CharacterCustomizer injects IConsoleOutput | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `IConsoleOutput` | [ ] |
| 36 | CharacterCustomizer injects TalentCopier | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `TalentCopier` | [ ] |
| 37 | DI registration for CharacterCustomizer, TalentNameResolver, and TalentCopier | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | count_equals | `(CharacterCustomizer|TalentCopier|TalentNameResolver)` = 3 | [ ] |
| 38 | TalentCopier unit tests pass | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~TalentCopier | succeeds | - | [ ] |
| 39 | CharacterCustomizer excluded talent indices with correct values | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `(ExcludedTalent\|excludedTalent\|_excludedIndices)[\s\S]{0,300}(3[\s\S]{0,200}72[\s\S]{0,200}153)` | [ ] |
| 40 | VirginCustom gender restriction | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `IsFemale` | [ ] |
| 41 | CopyCustom source character bound check | code | Grep(Era.Core/Character/TalentCopier.cs) | matches | `source.*<.*InitialCharacterCount|source.*>=.*1|<\s*1.*source` | [ ] |
| 42 | TalentNameResolver covers mid-range case 70 (快感) | code | Grep(Era.Core/Character/TalentNameResolver.cs) | matches | `70.*快感|快感.*70` | [ ] |
| 43 | TalentCopier injects IConsoleOutput | code | Grep(Era.Core/Character/TalentCopier.cs) | matches | `IConsoleOutput` | [ ] |
| 44 | TalentCopier injects IInputHandler | code | Grep(Era.Core/Character/TalentCopier.cs) | matches | `IInputHandler` | [ ] |
| 45 | CharacterCustomizer has PRINT_STATE no-op stub methods | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | count_equals | `void PrintState(Abl|Talent|Exp)\(` = 3 | [ ] |
| 46 | CharacterCustomizer implements pagination logic | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `TalentsPerPage\s*=\s*40\|talentsPerPage\s*=\s*40\|PageSize\s*=\s*40` | [ ] |
| 47 | NAME_CUSTOM empty input preservation unit test (NAME and CALLNAME) | code | Grep(Era.Core.Tests/) | matches | `EmptyInput.*(Name\|CallName)\|empty.*(name\|callname)\|preserve.*(Name\|CallName)` | [ ] |
| 48 | CharacterCustomizer implements talent type-dependent modification | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `GetTalentName\|talentName\|toggleTalent\|OnOff\|onOff` | [ ] |
| 49 | CharacterCustomizer CustomCharamake sets initial MAXBASE and BASE | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `MaxBase\|maxBase\|SetBase\|base.*maxBase\|2000` | [ ] |
| 50 | ClothesCustom verifies player-only NO guard | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `GetCharacterNo\|ClothesCustom.*no\|playerOnly` | [ ] |
| 51 | CustomCharamake sets initial gender (TALENT:2) | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `Gender\|gender\|TalentIndex\.Gender\|TALENT.*2` | [ ] |
| 52 | TalentNameResolver covers multi-value case 61 (汚臭耐性) | code | Grep(Era.Core/Character/TalentNameResolver.cs) | matches | `61.*潔癖\|潔癖.*61` | [ ] |
| 53 | CustomCharamake initializes NO to 0 | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `SetCharacterNo\|engineVars.*No` | [ ] |
| 54 | ClothesCustom uses HasVagina for clothing pattern | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `HasVagina.*[Cc]lothes\|[Cc]lothes.*HasVagina\|hasVagina.*cloth` | [ ] |
| 55 | ICsvNameResolver declares GetTalentName (CSV array) | code | Grep(Era.Core/Interfaces/ICsvNameResolver.cs) | matches | `string GetTalentName\(int` | [ ] |
| 56 | CustomTerminal implements mutual exclusion normalization | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `MutualExclu\|肉便器\|PublicToilet\|mutualExclu` | [ ] |
| 57 | TalentNameResolver unit tests pass | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~TalentNameResolver | succeeds | - | [ ] |
| 58 | ABL_CUSTOM filters gender-incompatible abilities | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `!.*HasVagina.*[Aa]bl\|HasVagina.*[Aa]bl\|[Aa]bl.*[Hh]as[Vv]agina` | [ ] |
| 59 | TalentCopier uses named sentinel for CFLAG:311 reorder | code | Grep(Era.Core/Character/TalentCopier.cs) | matches | `Sentinel\|InitialCharacterCount.*\\+\|sentinel` | [ ] |
| 60 | CharacterCustomizer injects TalentNameResolver | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `CharacterCustomizer\([\s\S]{0,500}TalentNameResolver` | [ ] |
| 61 | VirginCustom mutation unit test | test | dotnet test Era.Core.Tests --filter FullyQualifiedName~VirginCustom | succeeds | - | [ ] |
| 62 | EXP_CUSTOM filters gender-incompatible experiences | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `!.*HasVagina.*[Ee]xp\|HasVagina.*[Ee]xp\|[Ee]xp.*[Hh]as[Vv]agina` | [ ] |
| 63 | TalentCustom page count bound (4 pages) | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `MaxPage\|TotalPages\s*=\s*4\|pageCount\s*=\s*4\|Pages\s*=\s*4\|page.*<\s*4` | [ ] |
| 64 | VirginCustom skips player character (index 0) | code | Grep(Era.Core.Tests/) | matches | `[Pp]layer.*[Nn]ot.*[Mm]utat\|[Ss]kip.*[Pp]layer\|startIndex.*1\|index.*0.*[Nn]ot` | [ ] |
| 65 | CUSTOM_TERMINAL uses HasPenis for 精力/童貞 normalization | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `[Hh]as[Pp]enis.*[Ss]tamina\|[Hh]as[Pp]enis.*精力\|[Hh]as[Pp]enis.*[Mm]axBase` | [ ] |
| 66 | CUSTOM_TERMINAL implements 処女 state handling | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `[Vv]irgin.*[Ss]ensation\|処女.*[Vv]感覚\|[Vv]irginState\|[Vv]irgin.*[Ee]xp` | [ ] |
| 67 | ABL_CUSTOM excludes addiction abilities (index >= 30) | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `[Aa]ddiction\|>= 30\|AblThreshold\|ablIndex.*30` | [ ] |
| 68 | EXP_CUSTOM caps input at 50 | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `[Mm]in.*50\|[Mm]ax[Ee]xp.*50\|[Ee]xp[Cc]ap.*50\|Math\.Min.*50` | [ ] |
| 69 | BASE_CUSTOM uses named constants for modification bounds (1000-3000, step 100) | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `[Mm]in.*[Bb]ase.*1000\|[Mm]ax.*[Bb]ase.*3000\|[Bb]ase[Ss]tep.*100\|1000.*3000` | [ ] |
| 70 | ABL_CUSTOM enforces 0-2 value range | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `[Aa]bl.*>= 2\|[Aa]bl.*<= 0\|[Aa]blMax.*2\|[Mm]ax[Aa]bl.*2` | [ ] |
| 71 | CharaCustum calls RequestOneInput for confirmation | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `RequestOneInput` | [ ] |
| 72 | ClothesCustom implements maid uniform secondary prompt (female only) | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `[Mm]aid\|メイド\|[Cc]lothing[Pp]attern.*\\+\|服装パターン.*\\+` | [ ] |
| 73 | TalentCopier uses named CFLAG index for 311 | code | Grep(Era.Core/Character/TalentCopier.cs) | matches | `CharacterFlagIndex\|CflagIndex\|ReorderSentinel\|cflagIndex` | [ ] |
| 74 | CharacterCustomizer calls RequestStringInput for name input | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `RequestStringInput` | [ ] |
| 75 | CustomTerminal implements HAS_VAGINA gender normalization | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `[Hh]as[Vv]agina.*(ホモ\|[Gg]ay\|[Hh]omo)\|[Ll]esbian\|レズ.*[Hh]as[Vv]agina` | [ ] |
| 76 | TalentCustom handles 童貞 special case toggle | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `index.*==.*1\|talentIndex.*==.*1\|童貞.*[Tt]oggle\|[Vv]irginity[Mm]ale` | [ ] |
| 77 | CustomTerminal implements gender cycling (talent 2: 1→2→3→1) | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `TalentIndex\.Gender\|[Gg]ender.*[Cc]ycl\|[Gg]ender.*3.*1` | [ ] |
| 78 | CustomTerminal implements 性別嗜好 cycling (talent 81: -1→0→1→2→3→-1) | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `TalentIndex\.SexualOrientation\|[Oo]rientation.*[Cc]ycl\|== 3.*-1` | [ ] |
| 79 | Equivalence test exists for REVERSEMODE_1 character reorder | code | Grep(Era.Core.Tests/) | matches | `ReverseMode1.*[Rr]eorder\|[Rr]eorder.*ReverseMode1\|CFLAG.*311\|[Ss]entinel.*[Rr]eorder` | [ ] |
| 80 | CharacterCustomizer uses disabled-item color constants (setting + text) | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `DisabledSettingColor\|DisabledTextColor\|0x999999` | [ ] |
| 81 | Test exists for ClothesCustom player-only restriction | code | Grep(Era.Core.Tests/) | matches | `ClothesCustom.*(Player\|NonPlayer\|Guard\|playerOnly\|Skip)` | [ ] |
| 82 | Test exists for CustomTerminal loop with no-op PRINT_STATE stubs | code | Grep(Era.Core.Tests/) | matches | `CustomTerminal.*(Stub\|NoOp\|PrintState\|Loop\|Complete\|Finish)\|PrintState.*(Stub\|NoOp\|Complete)` | [ ] |
| 83 | Test exists for 処女 state Branch 2 (non-virgin vagina V経験==0 → set 1) | code | Grep(Era.Core.Tests/) | matches | `VirginSensation\|V経験.*NonVirgin\|virginState.*[Ss]et\|Branch2.*Virgin` | [ ] |
| 84 | CharacterCustomizer uses two-step gender pattern (GetTalent→.Value→HasVagina/HasPenis) | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `GetTalent\(.*TalentIndex\.Gender[\s\S]{0,120}\.Value[\s\S]{0,40}Has(Vagina\|Penis)` | [ ] |
| 85 | TalentCopier calls GetTarget for COPY_CUSTOM target reading | code | Grep(Era.Core/Character/TalentCopier.cs) | matches | `GetTarget\(` | [ ] |
| 86 | TalentIndex declares well-known indices with correct values | code | Grep(Era.Core/Types/TalentIndex.cs) | count_equals | `Gender\s*=\s*2\|MaleVirginity\s*=\s*1\|SexualOrientation\s*=\s*81\|Lewdness\s*=\s*4` = 4 | [ ] |
| 87 | TalentNameResolver CASEELSE delegates to ICsvNameResolver.GetTalentName | code | Grep(Era.Core/Character/TalentNameResolver.cs) | matches | `_nameResolver\.GetTalentName\(` | [ ] |
| 88 | VirginCustom uses two-step gender pattern for IsFemale | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `GetTalent\(.*TalentIndex\.Gender[\s\S]{0,120}\.Value[\s\S]{0,40}IsFemale` | [ ] |
| 89 | CharacterCustomizer calls TalentNameResolver for talent name resolution | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `_talentNameResolver\.GetTalentName\(` | [ ] |
| 90 | CharaCustum delegates to CustomTerminal | code | Grep(Era.Core/Character/CharacterCustomizer.cs) | matches | `CharaCustum[\s\S]{0,300}CustomTerminal\(` | [ ] |

### AC Details

**AC#1: CharacterCustomizer.cs exists**
- **Test**: Glob pattern="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: File exists
- **Rationale**: Architecture deliverable per phase-20-27-game-systems.md:75. Migrates SHOP_CUSTOM.ERB + CHARA_CUSTUM.ERB logic. [C1]

**AC#2: TalentCopier.cs exists**
- **Test**: Glob pattern="Era.Core/Character/TalentCopier.cs"
- **Expected**: File exists
- **Rationale**: Architecture deliverable per phase-20-27-game-systems.md:76. Migrates TALENTCOPY.ERB logic. [C1]

**AC#3: IEngineVariables declares SetName**
- **Test**: Grep pattern=`void SetName\(int characterIndex, string` path="Era.Core/Interfaces/IEngineVariables.cs"
- **Expected**: 1 match
- **Rationale**: NAME_CUSTOM writes NAME:ARG (SHOP_CUSTOM.ERB:247). Requires setter on IEngineVariables. [C7]

**AC#4: IEngineVariables declares SetCallName**
- **Test**: Grep pattern=`void SetCallName\(int characterIndex, string` path="Era.Core/Interfaces/IEngineVariables.cs"
- **Expected**: 1 match
- **Rationale**: NAME_CUSTOM writes CALLNAME:ARG (SHOP_CUSTOM.ERB:258-261). Requires setter on IEngineVariables.

**AC#5: IEngineVariables declares SetMaster**
- **Test**: Grep pattern=`void SetMaster\(int` path="Era.Core/Interfaces/IEngineVariables.cs"
- **Expected**: 1 match
- **Rationale**: REVERSEMODE_1 writes MASTER=0 (TALENTCOPY.ERB:37). Existing GetMaster is read-only.

**AC#6: IEngineVariables declares GetTarget and SetTarget**
- **Test**: Grep pattern=`(Get|Set)Target\(` path="Era.Core/Interfaces/IEngineVariables.cs" | count
- **Expected**: 2 matches (1 getter + 1 setter)
- **Rationale**: REVERSEMODE_1 and COPY_CUSTOM both write TARGET (TALENTCOPY.ERB:29,87). TARGET is entirely absent from current interface. [C14]

**AC#7: IEngineVariables declares GetPlayer and SetPlayer**
- **Test**: Grep pattern=`(Get|Set)Player\(` path="Era.Core/Interfaces/IEngineVariables.cs" | count
- **Expected**: 2 matches (1 getter + 1 setter)
- **Rationale**: REVERSEMODE_1 writes PLAYER=MASTER (TALENTCOPY.ERB:39). SetPlayer needed for F777. GetPlayer added per Zero Debt Upfront: PLAYER is read by other SYSTEM.ERB consumers in future Phase 20 migrations; adding both now completes interface extension in one pass. PLAYER is entirely absent from current interface. [C14]

**AC#8: IEngineVariables declares SetCharacterNo**
- **Test**: Grep pattern=`void SetCharacterNo\(int characterIndex, int value\)` path="Era.Core/Interfaces/IEngineVariables.cs"
- **Expected**: 1 match (setter only — GetCharacterNo already exists from F776)
- **Rationale**: SHOP_CUSTOM.ERB:3 writes NO:ARG=0. GetCharacterNo already exists from F776 for reads (TALENTCOPY.ERB:44). Only SetCharacterNo needed for writes. [C13, C14]

**AC#9: Inject ICsvNameResolver into TalentNameResolver constructor**
- **Test**: Grep pattern=`TalentNameResolver\(ICsvNameResolver` path="Era.Core/Character/TalentNameResolver.cs"
- **Expected**: 1 match (constructor signature with ICsvNameResolver parameter — verifies DI, not just interface reference)
- **Rationale**: TalentNameResolver (non-static class) requires ICsvNameResolver for CASEELSE fallback (PRINT_STATE.ERB:515-517 does CSV TALENTNAME lookup for ~164 talent indices outside the 26 switch cases). Without ICsvNameResolver injection, CASEELSE returns wrong names. [C9]

**AC#10: IInputHandler declares RequestOneInput**
- **Test**: Grep pattern=`Result<Unit> RequestOneInput\(` path="Era.Core/Input/IInputHandler.cs"
- **Expected**: 1 match
- **Rationale**: CHARA_CUSTUM.ERB:20 uses ONEINPUT. IInputHandler has no equivalent. [C16]

**AC#11: TalentNameResolver.cs exists**
- **Test**: Glob pattern="Era.Core/Character/TalentNameResolver.cs"
- **Expected**: File exists
- **Rationale**: GET_TALENTNAME (PRINT_STATE.ERB:316-518) is a 202-line SELECTCASE that must be extracted as standalone class for reuse.

**AC#12: TalentNameResolver covers talent case 130 (last case)**
- **Test**: Grep pattern=`130.*回復` path="Era.Core/Character/TalentNameResolver.cs"
- **Expected**: 1 match (case 130 maps to 回復遅い/回復早い)
- **Rationale**: Case 130 (回復速度) is the last SELECTCASE entry in GET_TALENTNAME (PRINT_STATE.ERB:509-514). If case 130 exists, all preceding 25 cases (0-105) must also exist since no implementer would skip middle cases and only add the last one. This single-line grep avoids cross-line matching issues.

**AC#13: IEngineVariables backward compatibility**
- **Test**: Grep pattern=`(GetResult|GetMoney|SetMoney|GetDay|GetMaster|GetAssi|GetCount|GetCharaNum|GetRandom|GetName|GetCallName|GetIsAssi|GetCharacterNo)\(` path="Era.Core/Interfaces/IEngineVariables.cs" | count
- **Expected**: 13 (all pre-existing methods preserved, includes GetCharacterNo from F776)
- **Rationale**: Additive-only extension; existing consumers must not break. Baseline: 13 methods (including SetMoney and GetCharacterNo from F776). [C14]

**AC#14: Unit tests pass for CharacterCustomizer**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter FullyQualifiedName~CharacterCustomizer'`
- **Expected**: Exit code 0, all tests pass
- **Rationale**: TDD requirement. Tests cover CUSTOM_TERMINAL normalization logic, gender exclusion paths, and entry point delegation. [C3, C9]

**AC#15: Equivalence tests exist for COPY_CUSTOM 191-talent range**
- **Test**: Grep pattern=`Assert.*191|Copies.*191|count.*191|TalentCount.*191` path="Era.Core.Tests/" type=cs
- **Expected**: At least 1 match confirming equivalence test asserts 191-talent range
- **Rationale**: TALENTCOPY.ERB:94 uses FOR LOCAL,0,191 (exclusive upper bound = 191 iterations copying indices 0-190). Equivalence test must verify exactly 191 talents copied, NOT 192 (F368 TalentManager.CopyTalents discrepancy). [C5, C9]

**AC#16: ICsvNameResolver backward compatibility**
- **Test**: Grep pattern=`(GetAblName|GetExpName|GetMarkName|GetPalamName)\(` path="Era.Core/Interfaces/ICsvNameResolver.cs" | count
- **Expected**: 4 (all pre-existing methods preserved)
- **Rationale**: Additive-only extension; existing consumers must not break. Baseline: 4 methods. [C15]

**AC#17: IInputHandler backward compatibility**
- **Test**: Grep pattern=`(RequestNumericInput|RequestStringInput|ProvideInput|IsWaitingForInput)\b` path="Era.Core/Input/IInputHandler.cs" | count
- **Expected**: 4 (all pre-existing members preserved)
- **Rationale**: Additive-only extension; existing consumers must not break. Baseline: 3 methods + 1 property = 4 members. [C16]

**AC#18: CharacterCustomizer has 3 public SYSTEM.ERB entry points**
- **Test**: Grep pattern=`public.*Result.*(CustomCharamake|CharaCustum|VirginCustom)\(` path="Era.Core/Character/CharacterCustomizer.cs" | count
- **Expected**: 3 methods
- **Rationale**: SYSTEM.ERB calls 3 CharacterCustomizer functions: CUSTOM_CHARAMAKE:69, CHARA_CUSTUM:122, VIRGIN_CUSTOM:127. CustomTerminal is an internal delegation target (CHARA_CUSTUM.ERB:14 calls CUSTOM_TERMINAL) — internal access modifier, testable via InternalsVisibleTo. Combined with AC#19 (TalentCopier: ReverseMode1), this covers all 4 SYSTEM.ERB entry points. [C1]

**AC#19: TalentCopier has ReverseMode1 and CopyCustom methods**
- **Test**: Grep pattern=`public.*Result.*(ReverseMode1|CopyCustom)\(` path="Era.Core/Character/TalentCopier.cs" | count
- **Expected**: 2 methods
- **Rationale**: SYSTEM.ERB:86 calls REVERSEMODE_1; SHOP_CUSTOM.ERB:107 calls COPY_CUSTOM. Both must be in TalentCopier. ERB signatures: CopyCustom(int targetId) — source character selected interactively via INPUT loop inside the method; ReverseMode1() — character selection is interactive inside (parameterless). Core logic (actual talent copy, character reorder) extractable as private methods for unit testing via InternalsVisibleTo. [C1, C6]

**AC#20: Zero technical debt in new files**
- **Test**: Grep pattern=`TODO|FIXME|HACK` paths=[Era.Core/Character/CharacterCustomizer.cs, Era.Core/Character/TalentCopier.cs, Era.Core/Character/TalentNameResolver.cs, Era.Core/Constants/GameConstants.cs, Era.Core/Types/CharacterFlagIndex.cs, Era.Core/Types/TalentIndex.cs]
- **Expected**: 0 matches
- **Rationale**: Sub-Feature Requirements mandate zero technical debt. All new/modified files checked: 3 Character/ files (Task 5/4/6) + GameConstants.cs (Task 1) + CharacterFlagIndex.cs (Task 6) + TalentIndex.cs (Task 1). [C8]

**AC#21: TalentCopier copies exactly 191 talents (0-190)**
- **Test**: Grep pattern=`i\s*<\s*191|i\s*<=\s*190|Range\(0,\s*191\)` path="Era.Core/Character/TalentCopier.cs"
- **Expected**: At least 1 match confirming 191-talent loop bound in implementation
- **Rationale**: TALENTCOPY.ERB:94 FOR LOCAL,0,191 = exclusive upper bound. Must copy indices 0-190 (191 talents), NOT 192. [C5]

**AC#22: CharacterCustomizer injects IEngineVariables**
- **Test**: Grep pattern=`IEngineVariables` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (constructor injection or field)
- **Rationale**: NAME_CUSTOM requires SetName/SetCallName; CUSTOM_TERMINAL reads NAME:ARG. [C1]

**AC#23: CharacterCustomizer injects IStyleManager**
- **Test**: Grep pattern=`IStyleManager` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: BASE_CUSTOM, TALENT_CUSTOM, ABL_CUSTOM all use SETCOLOR/RESETCOLOR (19 call sites in SHOP_CUSTOM.ERB). [C10]

**AC#25: CharacterCustomizer injects IInputHandler**
- **Test**: Grep pattern=`IInputHandler` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: CHARA_CUSTUM.ERB:20 uses ONEINPUT; other functions use INPUT/INPUTS. IInputHandler required. [C1]

**AC#26: CharacterCustomizer injects ICsvNameResolver**
- **Test**: Grep pattern=`ICsvNameResolver` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: ABL_CUSTOM, EXP_CUSTOM use ABLNAME/EXPNAME for menu display. ICsvNameResolver already provides these (GetAblName, GetExpName). [C9]

**AC#27: Named constants for 開始時人数 and ReorderSentinelValue (derived)**
- **Test**: Grep pattern=`InitialCharacterCount\s*=\s*14|ReorderSentinelValue\s*=.*InitialCharacterCount` path="Era.Core/Constants/GameConstants.cs" | count
- **Expected**: 2 matches (InitialCharacterCount=14 definition + ReorderSentinelValue derived from InitialCharacterCount)
- **Rationale**: DIM.ERH:56 defines 開始時人数=14 as constant. ReorderSentinelValue=InitialCharacterCount+1 is the CFLAG:311 sentinel (TALENTCOPY.ERB:41). Both must be in shared GameConstants.cs (not private const). [C17, C19]

**AC#28: TalentCopier injects ICharacterManager**
- **Test**: Grep pattern=`ICharacterManager` path="Era.Core/Character/TalentCopier.cs"
- **Expected**: At least 1 match
- **Rationale**: REVERSEMODE_1 uses ADDCHARA/DELCHARA (TALENTCOPY.ERB:30,36,45,48,53). ICharacterManager.AddChara/DelChara required.

**AC#29: TalentCopier injects IEngineVariables**
- **Test**: Grep pattern=`IEngineVariables` path="Era.Core/Character/TalentCopier.cs"
- **Expected**: At least 1 match
- **Rationale**: REVERSEMODE_1 writes TARGET, MASTER, PLAYER (TALENTCOPY.ERB:29,37-39). COPY_CUSTOM reads/writes TARGET. IEngineVariables required.

**AC#30: Build succeeds**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'`
- **Expected**: Exit code 0
- **Rationale**: Interface extensions must compile cleanly. TreatWarningsAsErrors=true ensures no warnings.

**AC#31: CharacterCustomizer includes ClothesCustom logic**
- **Test**: Grep pattern=`ClothesCustom` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: CHARA_CUSTUM.ERB:22 calls CLOTHES_CUSTOM without arguments (implicit ARG=0 in ERA = always player character). CLOTHES_CUSTOM (SHOP_CUSTOM.ERB:416-445) handles clothing pattern selection. ClothesCustom always operates on characterId=0 (player), matching ERB's implicit default. CharacterCustomizer must include this logic as part of the CharaCustum flow. [C13]

**AC#32: TalentCopier injects IVariableStore**
- **Test**: Grep pattern=`IVariableStore` path="Era.Core/Character/TalentCopier.cs"
- **Expected**: At least 1 match (constructor injection or field)
- **Rationale**: CopyCustom copies TALENT arrays via IVariableStore.GetTalent/SetTalent (TALENTCOPY.ERB:94-96). IVariableStore is the primary data dependency for talent copying.

**AC#33: CharacterCustomizer injects IVariableStore**
- **Test**: Grep pattern=`IVariableStore` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (constructor injection or field)
- **Rationale**: CUSTOM_TERMINAL reads/writes TALENT, ABL, EXP, BASE, MAXBASE, CFLAG arrays (SHOP_CUSTOM.ERB:6-67, 122-232, 302-414). IVariableStore is the primary data interface. [C1]

**AC#34: CharacterCustomizer injects ICommonFunctions**
- **Test**: Grep pattern=`ICommonFunctions` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (constructor injection or field)
- **Rationale**: CUSTOM_TERMINAL uses HAS_VAGINA/HAS_PENIS for gender-based menu filtering (SHOP_CUSTOM.ERB:21,25,42,55). VIRGIN_CUSTOM uses IS_FEMALE. [C3, C12]

**AC#35: CharacterCustomizer injects IConsoleOutput**
- **Test**: Grep pattern=`IConsoleOutput` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (constructor injection or field)
- **Rationale**: CUSTOM_TERMINAL uses DRAWLINE, BAR, PRINTSL/PRINTL/PRINTFORM throughout (SHOP_CUSTOM.ERB:69,72,75,84,110). F788 predecessor provides these methods. [C10]

**AC#36: CharacterCustomizer injects TalentCopier**
- **Test**: Grep pattern=`TalentCopier` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (constructor injection or field)
- **Rationale**: CUSTOM_TERMINAL delegates to COPY_CUSTOM(ARG) when LOCAL:100==1006 (SHOP_CUSTOM.ERB:106-107). CopyCustom lives in TalentCopier.cs. CharacterCustomizer must inject TalentCopier to call CopyCustom.

**AC#37: DI registration for CharacterCustomizer, TalentNameResolver, and TalentCopier**
- **Test**: Grep pattern=`(CharacterCustomizer|TalentCopier|TalentNameResolver)` path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs" | count
- **Expected**: 3 (all three classes registered)
- **Rationale**: Per F774 ShopSystem precedent (ServiceCollectionExtensions.cs:157), new service classes must be registered in DI container. TalentNameResolver is now a non-static class with ICsvNameResolver dependency and must be registered so CharacterCustomizer can receive it as an injected dependency at runtime.

**AC#38: TalentCopier unit tests pass**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter FullyQualifiedName~TalentCopier'`
- **Expected**: Exit code 0, all tests pass
- **Rationale**: TDD requirement. Tests cover CFLAG:311 reordering (C6), 191-talent range equivalence (C5), and source character bound (C18). Complements AC#14 which covers CharacterCustomizer only. [C6, C9]

**AC#39: CharacterCustomizer excluded talent indices with correct values [C11]**
- **Test**: Grep pattern=`(ExcludedTalent|excludedTalent|_excludedIndices)[\s\S]{0,300}(3[\s\S]{0,200}72[\s\S]{0,200}153)` path="Era.Core/Character/CharacterCustomizer.cs" multiline=true
- **Expected**: At least 1 match (named exclusion collection containing representative values 3, 72, 153)
- **Rationale**: CUSTOM_TERMINAL (SHOP_CUSTOM.ERB:146,313) excludes talent indices 3-5, 9, 34, 38, 72-76, 84, 85, 153-155 from customization menu. Implementation must use a named collection AND contain correct values. Multiline matcher spot-checks 3 representative values from distinct ranges (3=low, 72=mid, 153=high) to verify content correctness, not just collection existence. [C11]

**AC#40: VirginCustom gender restriction**
- **Test**: Grep pattern=`IsFemale` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: VIRGIN_CUSTOM only affects IS_FEMALE characters (SHOP_CUSTOM.ERB). Must use IsFemale (checks TALENT:2 gender identity = 1), NOT HasVagina (includes futanari with gender values 1 and 3). [C12]

**AC#41: CopyCustom source character bound check**
- **Test**: Grep pattern=`source.*<.*InitialCharacterCount|source.*>=.*1|<\s*1.*source` path="Era.Core/Character/TalentCopier.cs"
- **Expected**: At least 1 match (bound check expression present)
- **Rationale**: COPY_CUSTOM source must be initial character (1 ≤ source < 開始時人数) per TALENTCOPY.ERB:86. TalentCopier must validate this bound. Matcher requires actual bound-check expression, not just the constant name (which is covered by AC#27). [C18]

**AC#42: TalentNameResolver covers mid-range case 70 (快感)**
- **Test**: Grep pattern=`70.*快感|快感.*70` path="Era.Core/Character/TalentNameResolver.cs"
- **Expected**: At least 1 match (case 70 maps to 快感の否定/快感に素直)
- **Rationale**: Case 70 is a mid-range entry in GET_TALENTNAME (PRINT_STATE.ERB:437-442). Combined with AC#12 (case 130, last entry), verifying a mid-range case provides stronger coverage that non-sequential case indices are not skipped.

**AC#43: TalentCopier injects IConsoleOutput**
- **Test**: Grep pattern=`IConsoleOutput` path="Era.Core/Character/TalentCopier.cs"
- **Expected**: At least 1 match (constructor injection or field)
- **Rationale**: REVERSEMODE_1 uses PRINTL/PRINTFORML (TALENTCOPY.ERB:9-24,31-33) and COPY_CUSTOM uses PRINTL/PRINTFORML (TALENTCOPY.ERB:73,80,83,89,97). IConsoleOutput required for output operations.

**AC#44: TalentCopier injects IInputHandler**
- **Test**: Grep pattern=`IInputHandler` path="Era.Core/Character/TalentCopier.cs"
- **Expected**: At least 1 match (constructor injection or field)
- **Rationale**: REVERSEMODE_1 uses INPUT (TALENTCOPY.ERB:26,34) and COPY_CUSTOM uses INPUT (TALENTCOPY.ERB:85,92). IInputHandler required for user input operations.

**AC#45: CharacterCustomizer has PRINT_STATE no-op stub methods**
- **Test**: Grep pattern=`PrintState(Abl|Talent|Exp)` path="Era.Core/Character/CharacterCustomizer.cs" | count
- **Expected**: 3 (one method for each: PrintStateAbl, PrintStateTalent, PrintStateExp)
- **Rationale**: CUSTOM_TERMINAL calls PRINT_STATE_ABL/TALENT/EXP unconditionally every loop iteration (SHOP_CUSTOM.ERB:78-82). These must exist as no-op stub methods (not NotImplementedException). Code inspection verifies stub existence; AC#14 verifies they don't throw at runtime. [C1]

**AC#46: CharacterCustomizer implements pagination logic**
- **Test**: Grep pattern=`TalentsPerPage\s*=\s*40|talentsPerPage\s*=\s*40|PageSize\s*=\s*40` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (named constant assignment with pagination size 40 in TalentCustom method)
- **Rationale**: TALENT_CUSTOM uses 4-page pagination (SHOP_CUSTOM.ERB:302-361, ARG:1 ranges 0-3, each page shows 40 talents). Implementation must verify the concrete pagination size of 40, not just generic page naming. [C4]

**AC#47: NAME_CUSTOM empty input preservation unit test (NAME and CALLNAME)**
- **Test**: Grep pattern=`EmptyInput.*(Name|CallName)|empty.*(name|callname)|preserve.*(Name|CallName)` path="Era.Core.Tests/" type=cs
- **Expected**: At least 1 match (test verifying empty input preserves current NAME and CALLNAME for player character)
- **Rationale**: NAME_CUSTOM (SHOP_CUSTOM.ERB:244-248) preserves NAME and (SHOP_CUSTOM.ERB:257-261) preserves CALLNAME when player (ARG==0) submits empty input. Both follow the same pattern: `IF ARG == 0 && RESULTS == "" → keep current value`. Unit test must verify both branches. [C7]

**AC#48: CharacterCustomizer implements talent type-dependent modification**
- **Test**: Grep pattern=`GetTalentName|talentName|toggleTalent|OnOff|onOff` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (talent type determination using TalentNameResolver/name comparison)
- **Rationale**: CUSTOM_TERMINAL (SHOP_CUSTOM.ERB:136-180) uses GET_TALENTNAME for business logic: line 163 compares TALENTNAME:RESULT == GET_TALENTNAME(RESULT,1) to determine on/off vs quantitative talent type, line 175 checks next-level name to determine cycling direction. This is core talent modification behavior, not just display.

**AC#49: CharacterCustomizer CustomCharamake sets initial MAXBASE and BASE**
- **Test**: Grep pattern=`MaxBase|maxBase|SetBase|base.*maxBase|2000` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (initialization of HP/SP default values)
- **Rationale**: CUSTOM_CHARAMAKE (SHOP_CUSTOM.ERB:10-13) sets MAXBASE:ARG:0/1=2000 then BASE:ARG:0/1=MAXBASE:ARG:0/1. Both MAXBASE and BASE must be initialized — without BASE assignment, characters start with 0 current HP/SP despite having 2000 max.

**AC#50: ClothesCustom verifies player-only NO guard**
- **Test**: Grep pattern=`GetCharacterNo|ClothesCustom.*no|playerOnly` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (NO-based guard check in ClothesCustom method)
- **Rationale**: CLOTHES_CUSTOM (SHOP_CUSTOM.ERB:418) uses `SIF NO:ARG` to return immediately for non-player characters. Only player character (NO==0) can access clothing customization. [C13]

**AC#51: CustomCharamake sets initial gender (TALENT:2)**
- **Test**: Grep pattern=`Gender|gender|TalentIndex\.Gender|TALENT.*2` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (gender initialization in CustomCharamake method)
- **Rationale**: CUSTOM_CHARAMAKE (SHOP_CUSTOM.ERB:6-8) sets TALENT:ARG:2=1 (female default for non-player) and TALENT:ARG:2=2 (male for player ARG==0). This conditional gender assignment determines how CUSTOM_TERMINAL normalization processes the character.

**AC#52: TalentNameResolver covers multi-value case 61 (汚臭耐性)**
- **Test**: Grep pattern=`61.*潔癖|潔癖.*61` path="Era.Core/Character/TalentNameResolver.cs"
- **Expected**: At least 1 match (case 61 = 汚臭耐性 with 4 values: -2=潔癖症, -1=汚臭敏感, 1=汚臭鈍感, 2=汚れ無視)
- **Rationale**: Case 61 (PRINT_STATE.ERB:426-435) uses 4-value pattern unlike simple -1/1 cases. Combined with AC#12 (case 130, last entry) and AC#42 (case 70, simple -1/1), verifying a multi-value case ensures the implementer handles all pattern types.

**AC#53: CustomCharamake initializes NO to 0**
- **Test**: Grep pattern=`SetCharacterNo|engineVars.*No` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (NO initialization in CustomCharamake method)
- **Rationale**: CUSTOM_CHARAMAKE (SHOP_CUSTOM.ERB:3) sets NO:ARG=0. This initialization is prerequisite for CLOTHES_CUSTOM's NO-based guard (AC#50): without it, newly created characters may have uninitialized NO values.

**AC#54: ClothesCustom uses HasVagina for clothing pattern**
- **Test**: Grep pattern=`HasVagina.*[Cc]lothes|[Cc]lothes.*HasVagina|hasVagina.*cloth` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (HasVagina check in ClothesCustom method)
- **Rationale**: CLOTHES_CUSTOM (SHOP_CUSTOM.ERB:420) uses HAS_VAGINA(ARG), NOT IS_FEMALE. Futanari characters (gender=3, HasVagina=true) must receive female clothing pattern (pattern=2). Using IsFemale instead would incorrectly exclude futanari from female clothing.

**AC#55: ICsvNameResolver declares GetTalentName (CSV array accessor)**
- **Test**: Grep pattern=`string GetTalentName\(int` path="Era.Core/Interfaces/ICsvNameResolver.cs"
- **Expected**: 1 match
- **Rationale**: CASEELSE in TalentNameResolver.GetTalentName falls back to TALENTNAME CSV lookup when talent value is non-zero but unhandled (PRINT_STATE.ERB:515-517 CASEELSE → TALENTNAME:LOCAL). TalentNameResolver (non-static, injected into CharacterCustomizer) uses ICsvNameResolver.GetTalentName for this fallback. Simple CSV array accessor, same pattern as GetAblName/GetExpName/GetMarkName/GetPalamName.

**AC#56: CustomTerminal implements mutual exclusion normalization**
- **Test**: Grep pattern=`MutualExclu|肉便器|PublicToilet|mutualExclu` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: CUSTOM_TERMINAL (SHOP_CUSTOM.ERB:58-67) enforces mutual exclusion invariants: 肉便器↔公衆便所 and 肉便器↔NTR. These are core business rules that prevent contradictory talent states. [C3]

**AC#57: TalentNameResolver unit tests pass**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter FullyQualifiedName~TalentNameResolver'`
- **Expected**: Exit code 0, all tests pass
- **Rationale**: TalentNameResolver extracts 202-line SELECTCASE (PRINT_STATE.ERB:316-518) with 26 case mappings, multi-value patterns (cases 61, 81, 100, 105), and CASEELSE ICsvNameResolver fallback. Dedicated unit tests verify: (1) known cases return correct names, (2) CASEELSE delegates to ICsvNameResolver.GetTalentName, (3) talentValue==0 returns empty string.

**AC#58: ABL_CUSTOM filters gender-incompatible abilities**
- **Test**: Grep pattern=`!.*HasVagina.*[Aa]bl|HasVagina.*[Aa]bl|[Aa]bl.*[Hh]as[Vv]agina` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: ABL_CUSTOM (SHOP_CUSTOM.ERB:370-373) uses `!(HAS_VAGINA(ARG))` for indices 17/32 (同性愛関係: レズっ気/レズ中毒) and `HAS_VAGINA(ARG)` for indices 18/33 (ホモっ気/ゲイ中毒). MUST use HasVagina (NOT HasPenis) because futanari (gender=3) has HasVagina=true AND HasPenis=true; using HasPenis would incorrectly filter for futanari. [C3]

**AC#59: TalentCopier uses named sentinel for CFLAG:311 reorder**
- **Test**: Grep pattern=`Sentinel|InitialCharacterCount.*\+|sentinel` path="Era.Core/Character/TalentCopier.cs"
- **Expected**: At least 1 match (named constant for sentinel value, not magic number 15)
- **Rationale**: REVERSEMODE_1 (TALENTCOPY.ERB:41) sets CFLAG:MASTER:311=15 as sentinel during character reorder. Value must be > InitialCharacterCount(14) to place player's old slot outside normal range. Using named constant prevents breakage if InitialCharacterCount changes. [C19]

**AC#60: CharacterCustomizer injects TalentNameResolver**
- **Test**: Grep pattern=`CharacterCustomizer\([\s\S]{0,500}TalentNameResolver` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (constructor injection or field)
- **Rationale**: CharacterCustomizer uses TalentNameResolver for talent type-dependent modification logic (CUSTOM_TERMINAL GET_TALENTNAME calls). Every other CharacterCustomizer dependency has a dedicated injection AC (AC#22-36); TalentNameResolver must also have one to verify DI completeness. The matcher pattern verifies TalentNameResolver appears as a CharacterCustomizer constructor parameter (DI injection), not just any string reference.

**AC#61: VirginCustom mutation unit test**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter FullyQualifiedName~VirginCustom'`
- **Expected**: Exit code 0, all tests pass
- **Rationale**: VIRGIN_CUSTOM (SHOP_CUSTOM.ERB:448-472) performs 6 variable mutations for IS_FEMALE characters: TALENT:処女=1, EXP:V経験=0, EXP:V性交経験=0, EXP:A性交経験=0, ABL:V感覚=0, ABL:A感覚=0. AC#40 only verifies IsFemale reference exists but doesn't verify mutation correctness. Dedicated unit test ensures all 6 mutations are applied correctly. [C12]

**AC#62: EXP_CUSTOM filters gender-incompatible experiences**
- **Test**: Grep pattern=`!.*HasVagina.*[Ee]xp|HasVagina.*[Ee]xp|[Ee]xp.*[Hh]as[Vv]agina` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: EXP_CUSTOM (SHOP_CUSTOM.ERB:403-408) uses `!(HAS_VAGINA(ARG))` for index 51 and `HAS_VAGINA(ARG)` for index 52. Same HasVagina requirement as AC#58 for futanari correctness. Independent filtering path from ABL_CUSTOM; verifying both separately ensures neither is accidentally omitted. [C3]

**AC#63: TalentCustom page count bound (4 pages)**
- **Test**: Grep pattern=`MaxPage|TotalPages\s*=\s*4|pageCount\s*=\s*4|Pages\s*=\s*4|page.*<\s*4` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (named constant or bound check for 4 pages)
- **Rationale**: TALENT_CUSTOM (SHOP_CUSTOM.ERB:302-361) uses 4-page pagination (ARG:1 range 0-3, SHOP_CUSTOM.ERB:355 `IF ARG:1 != 3`). Combined with AC#46 (TalentsPerPage=40), this ensures complete pagination verification. [C4]

**AC#64: VirginCustom skips player character (index 0)**
- **Test**: Grep pattern=`[Pp]layer.*[Nn]ot.*[Mm]utat|[Ss]kip.*[Pp]layer|startIndex.*1|index.*0.*[Nn]ot` path="Era.Core.Tests/" type=cs
- **Expected**: At least 1 match (unit test verifying player character is not mutated)
- **Rationale**: VIRGIN_CUSTOM (SHOP_CUSTOM.ERB:457) iterates `FOR LOOP_CHR,1,CHARANUM` starting from index 1, deliberately skipping player character (index 0). An implementation starting from 0 would incorrectly mutate the player. AC#61 tests mutations on IS_FEMALE characters but doesn't verify player exclusion. [C12]

**AC#65: CUSTOM_TERMINAL uses HasPenis for 精力/童貞 normalization**
- **Test**: Grep pattern=`[Hh]as[Pp]enis.*[Ss]tamina|[Hh]as[Pp]enis.*精力|[Hh]as[Pp]enis.*[Mm]axBase` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (HasPenis check for 精力 MAXBASE/BASE initialization)
- **Rationale**: CUSTOM_TERMINAL (SHOP_CUSTOM.ERB:42-49) uses `HAS_PENIS(ARG)` to set 精力 MAXBASE/BASE=1000 for characters with penis, and zero 童貞+精力 for characters without. MUST use HasPenis (NOT !HasVagina) because futanari (gender=3) has HasVagina=true AND HasPenis=true. Using !HasVagina would incorrectly zero futanari's 精力. [C3]

**AC#66: CUSTOM_TERMINAL implements 処女 state handling**
- **Test**: Grep pattern=`[Vv]irgin.*[Ss]ensation|処女.*[Vv]感覚|[Vv]irginState|[Vv]irgin.*[Ee]xp` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (処女-dependent ability initialization)
- **Rationale**: CUSTOM_TERMINAL (SHOP_CUSTOM.ERB:50-57) has 3-branch conditional: (1) 処女==1 → zero V感覚/V経験/V性交経験/V拡張経験, (2) HAS_VAGINA and V経験==0 → set V経験=1 (non-virgin with vagina gets minimum V experience), (3) else → no-op. Branch 2 is subtle but critical for character state consistency. [C3]

**AC#67: ABL_CUSTOM excludes addiction abilities (index >= 30)**
- **Test**: Grep pattern=`[Aa]ddiction|>= 30|AblThreshold|ablIndex.*30` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (bound check excluding ABL indices >= 30)
- **Rationale**: ABL_CUSTOM (SHOP_CUSTOM.ERB:195) uses `SIF LOCAL >= 30 / RESTART` to exclude 中毒 (addiction) abilities from customization. Without this, players could directly modify addiction values. [C3]

**AC#68: EXP_CUSTOM caps input at 50**
- **Test**: Grep pattern=`[Mm]in.*50|[Mm]ax[Ee]xp.*50|[Ee]xp[Cc]ap.*50|Math\.Min.*50` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (MIN cap on EXP value input)
- **Rationale**: EXP_CUSTOM (SHOP_CUSTOM.ERB:229) uses `MIN(RESULT,50)` to cap user input at 50. Without this, players could set arbitrarily high experience values. [C3]

**AC#69: BASE_CUSTOM uses named constants for modification bounds**
- **Test**: Grep pattern=`[Mm]in.*[Bb]ase.*1000|[Mm]ax.*[Bb]ase.*3000|[Bb]ase[Ss]tep.*100|1000.*3000` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: CUSTOM_TERMINAL (SHOP_CUSTOM.ERB:122-134) uses HP/SP modification bounds: 1000 min, 3000 max, 100 step. These magic numbers must be named constants. [C8]

**AC#70: ABL_CUSTOM enforces 0-2 value range**
- **Test**: Grep pattern=`[Aa]bl.*>= 2|[Aa]bl.*<= 0|[Aa]blMax.*2|[Mm]ax[Aa]bl.*2` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: ABL_CUSTOM (SHOP_CUSTOM.ERB:198-210) caps ABL values at 0 minimum and 2 maximum. Increment at 2 or decrement at 0 results in RESTART (rejected). [C3]

**AC#71: CharaCustum calls RequestOneInput for confirmation**
- **Test**: Grep pattern=`RequestOneInput` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: CHARA_CUSTUM.ERB:20 uses ONEINPUT for [999] confirmation flow. AC#10 verifies the interface declaration and AC#25 verifies IInputHandler injection, but neither verifies CharaCustum actually calls RequestOneInput. Without this AC, an implementer could use RequestNumericInput everywhere and still pass all existing ACs.

**AC#72: ClothesCustom implements maid uniform secondary prompt (female clothing pattern only)**
- **Test**: Grep pattern=`[Mm]aid|メイド|[Cc]lothing[Pp]attern.*\+|服装パターン.*\+` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: SHOP_CUSTOM.ERB:438-444 secondary prompt triggers only when 服装パターン==2 (female clothing). The ternary expression `(CFLAG:服装パターン == 1) ? 執事 # メイド` always evaluates to メイド inside the ==2 guard (執事 is unreachable dead text in ERB). Implementation must match ERB behavior exactly: secondary prompt for female clothing only, always offering メイド option with CFLAG += RESULT*2. [C13]

**AC#73: TalentCopier uses named CFLAG index for 311**
- **Test**: Grep pattern=`CharacterFlagIndex|CflagIndex|ReorderSentinel|cflagIndex` path="Era.Core/Character/TalentCopier.cs"
- **Expected**: At least 1 match
- **Rationale**: CFLAG index 311 is a magic number in TALENTCOPY.ERB:40-46. AC#59 verifies the sentinel VALUE is named (InitialCharacterCount+1), but the CFLAG INDEX itself (311) should also be a named constant via CharacterFlagIndex.cs. Zero-debt principle (C8).

**AC#74: CharacterCustomizer calls RequestStringInput for name input**
- **Test**: Grep pattern=`RequestStringInput` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: NAME_CUSTOM (SHOP_CUSTOM.ERB:243,256) uses INPUTS (string input). Without this AC, an implementer could use RequestNumericInput, breaking text name input.

**AC#75: CustomTerminal implements HAS_VAGINA gender normalization**
- **Test**: Grep pattern=`[Hh]as[Vv]agina.*(ホモ|[Gg]ay|[Hh]omo)|[Ll]esbian|レズ.*[Hh]as[Vv]agina` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: CUSTOM_TERMINAL (SHOP_CUSTOM.ERB:21-41) zeros gender-incompatible ABL/EXP/TALENT based on HasVagina. AC#65 covers HasPenis (lines 42-49), AC#66 covers virgin state (lines 50-57), AC#56 covers mutual exclusion (lines 58-67), but lines 21-41 (the main gender normalization) had no code-level AC — only unit test coverage (AC#14).

**AC#76: TalentCustom handles 童貞 special case toggle**
- **Test**: Grep pattern=`index.*==.*1|talentIndex.*==.*1|童貞.*[Tt]oggle|[Vv]irginity[Mm]ale` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match
- **Rationale**: SHOP_CUSTOM.ERB:163 uses `TALENTNAME:RESULT == GET_TALENTNAME(RESULT,1) || RESULT == 1`. The `|| RESULT == 1` is required because GET_TALENTNAME(1,1) returns empty (SIF !1 = false). Without this special case, 童貞 would be treated as quantitative instead of toggle.

**AC#77: CustomTerminal implements gender cycling (talent 2: 1→2→3→1)**
- **Test**: Grep pattern=`TalentIndex\.Gender|[Gg]ender.*[Cc]ycl|[Gg]ender.*3.*1` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (requires TalentIndex.Gender named constant reference, not magic literal 2)
- **Rationale**: SHOP_CUSTOM.ERB:149-154 has a special-case branch for talent index 2 (性別/gender) that cycles through 3 values (1→2→3→1) instead of using the on/off toggle or quantitative cycling. This is distinct from the generic talent modification logic covered by AC#48. Matcher requires TalentIndex.Gender named constant to enforce AC#86 consumption (prevents magic literal 2). [C20, C3]

**AC#78: CustomTerminal implements 性別嗜好 cycling (talent 81: -1→0→1→2→3→-1)**
- **Test**: Grep pattern=`TalentIndex\.SexualOrientation|[Oo]rientation.*[Cc]ycl|== 3.*-1` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (requires TalentIndex.SexualOrientation named constant reference, not magic literal 81)
- **Rationale**: SHOP_CUSTOM.ERB:156-161 has a special-case branch for talent index 81 (性別嗜好/sexual orientation) that cycles through 5 values (-1→0→1→2→3→-1). Matcher requires TalentIndex.SexualOrientation named constant to enforce AC#86 consumption (prevents magic literal 81). [C20, C3]

**AC#79: Equivalence test exists for REVERSEMODE_1 character reorder**
- **Test**: Grep pattern=`ReverseMode1.*[Rr]eorder|[Rr]eorder.*ReverseMode1|CFLAG.*311|[Ss]entinel.*[Rr]eorder` path="Era.Core.Tests/" type=cs
- **Expected**: At least 1 match (test referencing REVERSEMODE_1 reorder or CFLAG:311 sentinel)
- **Rationale**: REVERSEMODE_1 (TALENTCOPY.ERB:1-55) is the most complex operation in F777: character deletion/addition via CFLAG:311 sentinel reorder. AC#38 runs TalentCopier tests but doesn't verify an equivalence test specifically for the reorder exists. AC#15 follows this pattern for COPY_CUSTOM (verifies 191-talent equivalence test exists); REVERSEMODE_1 needs equivalent verification. [C6, C19, C9]

**AC#80: CharacterCustomizer uses disabled-item color constants (setting + text)**
- **Test**: Grep pattern=`DisabledSettingColor|DisabledTextColor|0x999999` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (disabled-item color constants DisabledSettingColor or DisabledTextColor)
- **Rationale**: SHOP_CUSTOM.ERB:271-391 uses SETCOLOR(0x999999) for disabled items in CUSTOM_TERMINAL menu display (19 call sites). ERB uses two distinct color constants (色設定_設定_無効化 for setting items, 色設定_テキスト_無効 for text items), both 0x999999. Split into DisabledSettingColor and DisabledTextColor per Zero Debt Upfront. [C10]

**AC#81: ClothesCustom Player-Only Unit Test [C13]**
- **Test**: Grep pattern=`ClothesCustom.*(Player|NonPlayer|Guard|playerOnly|Skip)` path="Era.Core.Tests/" type=cs
- **Expected**: At least 1 match (test exercising player-only restriction)
- **Rationale**: C13 requires player-only restriction enforcement. AC#50 verifies code-level guard existence, but no unit test AC verifies the guard works correctly at runtime. Parallel to AC#47 (NAME_CUSTOM test) and AC#61 (VirginCustom test) which have dedicated test ACs for their constraints. [C13]

**AC#82: CustomTerminal Stub Loop Completion Test [C2]**
- **Test**: Grep pattern=`CustomTerminal.*(Stub|NoOp|PrintState|Loop|Complete|Finish)|PrintState.*(Stub|NoOp|Complete)` path="Era.Core.Tests/" type=cs
- **Expected**: At least 1 match (test exercising CustomTerminal loop with no-op stubs)
- **Rationale**: AC#45 verifies 3 PRINT_STATE stub method declarations exist via code grep, but cannot verify they are no-ops (not throwing). CustomTerminal loop calls all 3 stubs unconditionally every iteration — a throwing stub would break the entire loop. This AC verifies a dedicated test exists that exercises CustomTerminal through a complete loop iteration including stub invocations. [C2]

**AC#83: 処女 State Branch 2 Unit Test [C3]**
- **Test**: Grep pattern=`VirginSensation|V経験.*NonVirgin|virginState.*[Ss]et|Branch2.*Virgin` path="Era.Core.Tests/" type=cs
- **Expected**: At least 1 match (test exercising Branch 2: non-virgin vagina V経験==0 → set V経験=1)
- **Rationale**: AC#66 verifies 処女 state handling code exists via Grep (3-branch conditional), but Branch 2 (non-virgin character with vagina and V経験==0 → set V経験=1) is identified as "subtle but critical" with no dedicated unit test. This AC ensures a test exists specifically exercising this edge case where the character is non-virgin but has zero V experience, triggering the V経験 correction. [C3]

**AC#84: ICommonFunctions Two-Step Gender Pattern [C3]**
- **Test**: Grep pattern=`GetTalent\(.*TalentIndex\.Gender[\s\S]{0,120}\.Value[\s\S]{0,40}Has(Vagina|Penis)` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (two-step pattern: GetTalent→.Value→HasVagina/HasPenis)
- **Rationale**: ICommonFunctions.HasVagina/HasPenis take genderValue (talent value), NOT characterId. The correct pattern is: (1) `var gender = GetTalent(characterId, TalentIndex.Gender)`, (2) `HasVagina(gender.Value)`. AC#58/62/65/75 verify HasVagina/HasPenis usage but cannot detect parameter inversion. This AC uses multiline matching to verify: GetTalent with TalentIndex.Gender → `.Value` access → HasVagina/HasPenis call. The `.Value` requirement ensures the returned talent value is actually forwarded, preventing the characterId-passing bug. [C3]

**AC#85: TalentCopier GetTarget Usage [C1]**
- **Test**: Grep pattern=`GetTarget\(` path="Era.Core/Character/TalentCopier.cs"
- **Expected**: At least 1 match (call site for TARGET reading in CopyCustom)
- **Rationale**: TALENTCOPY.ERB:87 reads TARGET variable inside COPY_CUSTOM (`TARGET = LOCAL:RESULT`). AC#6 verifies IEngineVariables declares GetTarget/SetTarget, AC#29 verifies TalentCopier injects IEngineVariables, but neither verifies TalentCopier actually calls GetTarget. Without this AC, an implementer could inject IEngineVariables and only call SetTarget while hardcoding the target character index, breaking the COPY_CUSTOM flow. [C1]

**AC#86: TalentIndex Well-Known Indices [C3, C20]**
- **Test**: Grep pattern=`Gender\s*=\s*2|MaleVirginity\s*=\s*1|SexualOrientation\s*=\s*81|Lewdness\s*=\s*4` path="Era.Core/Types/TalentIndex.cs" | count
- **Expected**: 4 matches (Gender=2, MaleVirginity=1, SexualOrientation=81, Lewdness=4)
- **Rationale**: CharacterCustomizer cycling logic (AC#77 gender talent 2, AC#78 性別嗜好 talent 81) and special-case logic (AC#76 童貞 talent 1, AC#48 淫乱 talent 4) require well-known TalentIndex constants with correct ERB-matching values. Value verification prevents wrong-value declarations from silently passing AC#86 while breaking runtime cycling logic. [C3, C20]

**AC#87: TalentNameResolver CASEELSE delegates to ICsvNameResolver.GetTalentName**
- **Test**: Grep pattern=`_nameResolver\.GetTalentName\(` path="Era.Core/Character/TalentNameResolver.cs"
- **Expected**: At least 1 match (call site in CASEELSE branch)
- **Rationale**: PRINT_STATE.ERB:515-517 CASEELSE does CSV TALENTNAME lookup for ~164 talent indices outside the 26 switch cases. AC#9 verifies constructor injection of ICsvNameResolver, AC#55 verifies GetTalentName is declared on the interface, but neither verifies the actual call site exists. Without AC#87, a hardcoded empty-string return in CASEELSE would pass both AC#9 and AC#55. Completes the stub-replacement triple-AC pattern: (1) AC#55 interface declaration, (2) AC#9 constructor injection, (3) AC#87 actual call. [C9]

**AC#88: VirginCustom uses two-step gender pattern for IsFemale**
- **Test**: Grep pattern=`GetTalent\(.*TalentIndex\.Gender[\s\S]{0,120}\.Value[\s\S]{0,40}IsFemale` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (two-step pattern: GetTalent→.Value→IsFemale)
- **Rationale**: ICommonFunctions.IsFemale(int genderValue) takes genderValue, NOT characterId (ICommonFunctions.cs:12). Same parameter-inversion bug class as HasVagina/HasPenis (AC#84). VirginCustom must use the two-step pattern: (1) get gender talent, (2) extract .Value, (3) pass to IsFemale. Without AC#88, an implementer could call IsFemale(characterId) and pass AC#40 (which only checks IsFemale string existence). [C3, C12]

**AC#89: CharacterCustomizer calls TalentNameResolver for talent name resolution**
- **Test**: Grep pattern=`_talentNameResolver\.GetTalentName\(` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (call site in CustomTerminal talent type-dependent modification)
- **Rationale**: AC#60 verifies TalentNameResolver is injected into CharacterCustomizer constructor. AC#48 verifies talent type-dependent modification logic exists but its matcher (`GetTalentName|talentName|...`) does not enforce the call goes to the injected TalentNameResolver instance. Without AC#89, a hardcoded string lookup would pass AC#48 and AC#60. Completes the stub-replacement triple-AC pattern for CharacterCustomizer→TalentNameResolver: (1) AC#60 constructor injection, (2) AC#89 actual call site. Parallel to AC#87 (TalentNameResolver→ICsvNameResolver triple-AC).

**AC#90: CharaCustum delegates to CustomTerminal [C2]**
- **Test**: Grep pattern=`CharaCustum[\s\S]{0,300}CustomTerminal\(` path="Era.Core/Character/CharacterCustomizer.cs"
- **Expected**: At least 1 match (delegation call within CharaCustum method body)
- **Rationale**: C2 requires "CHARA_CUSTUM delegates to CUSTOM_TERMINAL" (CHARA_CUSTUM.ERB:14 `CALL CUSTOM_TERMINAL(RESULT)`). AC#18 verifies CharaCustum exists as a public method but not that it delegates. AC#82 verifies a test for CustomTerminal loop completion exists but not that the test exercises the CharaCustum→CustomTerminal call chain. Without AC#90, an implementer could inline all loop logic in CharaCustum without creating a CustomTerminal method, passing AC#18 and AC#82 while violating C2. [C2]

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Migrate SHOP_CUSTOM.ERB + CHARA_CUSTUM.ERB to CharacterCustomizer.cs | AC#1, AC#18, AC#22, AC#23, AC#25, AC#26, AC#31, AC#33, AC#34, AC#35, AC#36, AC#39, AC#40, AC#45, AC#46, AC#47, AC#48, AC#49, AC#50, AC#51, AC#53, AC#54, AC#56, AC#58, AC#60, AC#61, AC#62, AC#63, AC#65, AC#66, AC#67, AC#68, AC#69, AC#70, AC#71, AC#72, AC#74, AC#75, AC#76, AC#77, AC#78, AC#80, AC#81, AC#82, AC#83, AC#84, AC#86, AC#88, AC#89, AC#90 |
| 2 | Migrate TALENTCOPY.ERB to TalentCopier.cs | AC#2, AC#19, AC#21, AC#27, AC#28, AC#29, AC#32, AC#38, AC#41, AC#43, AC#44, AC#59, AC#73, AC#85 |
| 3 | Extend IEngineVariables with setters (SetName, SetCallName, SetMaster) | AC#3, AC#4, AC#5 |
| 4 | Extend IEngineVariables with TARGET getter/setter | AC#6 |
| 5 | Extend IEngineVariables with PLAYER getter/setter | AC#7 |
| 6 | Extend IEngineVariables with SetCharacterNo (getter exists from F776) | AC#8 |
| 7 | Extend ICsvNameResolver with GetTalentName, inject TalentNameResolver (non-static with ICsvNameResolver) into CharacterCustomizer | AC#9, AC#55, AC#60, AC#87 |
| 8 | Extend IInputHandler with ONEINPUT support | AC#10 |
| 9 | Extract GET_TALENTNAME as TalentNameResolver | AC#11, AC#12, AC#42, AC#52, AC#57 |
| 10 | Preserve 4 SYSTEM.ERB entry points (3 CharacterCustomizer + 1 TalentCopier) plus internal delegation targets | AC#18 (3 entry points + CustomTerminal), AC#19 (ReverseMode1 + CopyCustom) |
| 11 | Backward compatibility of extended interfaces | AC#13, AC#16, AC#17 |
| 12 | Zero technical debt | AC#20 |
| 13 | Equivalence tests | AC#14, AC#15, AC#38, AC#64, AC#79 |
| 14 | Build succeeds | AC#30 |
| 15 | DI registration | AC#37 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

This feature migrates SHOP_CUSTOM.ERB, CHARA_CUSTUM.ERB, and TALENTCOPY.ERB to C# by creating three new classes with interface extensions to satisfy missing method dependencies.

**Core Strategy**:
1. **CharacterCustomizer.cs**: Consolidates SHOP_CUSTOM.ERB (9 functions) + CHARA_CUSTUM.ERB (1 wrapper) into a single class with 4 public entry points
2. **TalentCopier.cs**: Migrates TALENTCOPY.ERB (2 functions) for character reordering and talent copying
3. **TalentNameResolver.cs**: Extracts GET_TALENTNAME (202-line SELECTCASE) as standalone utility
4. **Interface Extensions**: Add 10 missing methods across IEngineVariables (8: SetName, SetCallName, SetMaster, Get/SetTarget, Get/SetPlayer, SetCharacterNo), ICsvNameResolver (1 - GetTalentName for CSV array lookup), IInputHandler (1)

**Dependency Count Rationale**: CharacterCustomizer has 9 injected dependencies (IVariableStore, IConsoleOutput, IEngineVariables, IInputHandler, ICsvNameResolver, IStyleManager, ICommonFunctions, TalentNameResolver, TalentCopier) because it consolidates 9 ERB functions (CUSTOM_CHARAMAKE, CUSTOM_TERMINAL, NAME_CUSTOM, BASE_CUSTOM, TALENT_CUSTOM, ABL_CUSTOM, EXP_CUSTOM, CLOTHES_CUSTOM, VIRGIN_CUSTOM) that share a common CUSTOM_TERMINAL dispatcher loop and character context. ICharacterManager was removed as a phantom dependency — SHOP_CUSTOM.ERB and CHARA_CUSTUM.ERB never use ADDCHARA/DELCHARA; only TALENTCOPY.ERB does (handled by TalentCopier). Extracting helper classes would break the RESTART/GOTO loop pattern that ties these functions together. The dependency count reflects the subsystem's cross-cutting concerns (display, input, variables, character state), not an SRP violation.

**Architecture Pattern** (following F774 ShopSystem precedent):
- Constructor injection for all dependencies (IVariableStore, IConsoleOutput, IEngineVariables, IInputHandler, ICsvNameResolver, IStyleManager, ICommonFunctions, TalentNameResolver, TalentCopier)
- Result<T> for all operations (no exceptions for business logic failures)
- RESTART/GOTO patterns → while(true) with continue/break (proven C12 pattern from F774)
- PRINT_STATE_ABL/TALENT/EXP → no-op stub delegation (deferred to future features; no-op required because CUSTOM_TERMINAL calls these unconditionally every loop iteration)

**F368 Integration Strategy**:
- **Coexist**: Keep existing TalentManager.CopyTalents (192-element copy) for backward compatibility
- **Add**: New TalentCopier.CopyCustom (191-element copy matching ERB TALENTCOPY.ERB:94 FOR LOCAL,0,191)
- **Rationale**: F368 pre-existing code uses DTO pattern; F777 uses IVariableStore pattern. Consumer boundary: TalentManager.CopyTalents is called by CharacterSetup.cs (F368 DTO-based character creation); TalentCopier.CopyCustom is called by CharacterCustomizer.CustomTerminal (F777 IVariableStore-based customization flow). Both coexist until F782 (Post-Phase Review) consolidates the discrepancy

**Key Technical Decisions**:
- **191 vs 192 Talent Count**: TALENTCOPY.ERB:94 uses `FOR LOCAL,0,191` (exclusive upper bound = 191 iterations, indices 0-190). F368 TalentManager.CopyTalents uses `Array.Copy(source, target, 192)`. This feature matches ERB behavior exactly (191 talents). The F368 discrepancy is tracked as out-of-scope issue per Scope Discipline protocol
- **Gender Checks**: ICommonFunctions.HasVagina/HasPenis take genderValue parameter, not character index. Pattern: `var gender = _variables.GetTalent(characterId, TalentIndex.Gender); _commonFunctions.HasVagina(gender.Value)`
- **NO Variable**: GetCharacterNo exists from F776. Only SetCharacterNo needed for writes
- **TARGET/PLAYER Variables**: Entirely absent from IEngineVariables. Requires both getters and setters
- **Named Constants**: DIM.ERH:56 defines 開始時人数=14. C# uses `Constants.InitialCharacterCount` (not magic number)

This approach satisfies all 89 ACs (AC#1-AC#90, excluding deleted AC#24) through systematic interface extension, proven migration patterns, and exact ERB equivalence.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create Era.Core/Character/CharacterCustomizer.cs with namespace Era.Core.Character |
| 2 | Create Era.Core/Character/TalentCopier.cs with namespace Era.Core.Character |
| 3 | Add `void SetName(int characterIndex, string name)` to IEngineVariables interface |
| 4 | Add `void SetCallName(int characterIndex, string callName)` to IEngineVariables interface |
| 5 | Add `void SetMaster(int value)` to IEngineVariables interface |
| 6 | Add `int GetTarget()` and `void SetTarget(int value)` to IEngineVariables interface |
| 7 | Add `int GetPlayer()` and `void SetPlayer(int value)` to IEngineVariables interface |
| 8 | Add `void SetCharacterNo(int characterIndex, int value)` to IEngineVariables interface (GetCharacterNo already exists from F776) |
| 9 | Inject ICsvNameResolver into TalentNameResolver constructor for CASEELSE CSV fallback |
| 10 | Add `Result<Unit> RequestOneInput(string prompt)` to IInputHandler interface |
| 11 | Create Era.Core/Character/TalentNameResolver.cs as non-static class with ICsvNameResolver injection |
| 12 | Include case 130 (回復速度 → 回復遅い/回復早い) in TalentNameResolver switch expression |
| 13 | Verify all 13 pre-existing IEngineVariables methods remain unchanged in interface definition |
| 14 | Write unit tests in Era.Core.Tests/Character/CharacterCustomizerTests.cs exercising CUSTOM_TERMINAL normalization |
| 15 | Write equivalence test verifying 191-talent copy range (0-190 indices) in Era.Core.Tests/Character/TalentCopierTests.cs |
| 16 | Verify all 4 pre-existing ICsvNameResolver methods remain unchanged |
| 17 | Verify all 4 pre-existing IInputHandler members remain unchanged |
| 18 | Implement CustomCharamake, CharaCustum, VirginCustom as public methods and CustomTerminal as internal in CharacterCustomizer |
| 19 | Implement ReverseMode1() and CopyCustom(int targetId) public methods in TalentCopier (matching ERB signatures; source/character selection is interactive inside each method) |
| 20 | Ensure no TODO/FIXME/HACK in all new/modified files (6 files incl. TalentIndex.cs) |
| 21 | Use `for (int i = 0; i <= 190; i++)` or `for (int i = 0; i < 191; i++)` loop in TalentCopier.CopyCustom |
| 22 | Inject IEngineVariables via CharacterCustomizer constructor parameter |
| 23 | Inject IStyleManager via CharacterCustomizer constructor parameter |
| 25 | Inject IInputHandler via CharacterCustomizer constructor parameter |
| 26 | Inject ICsvNameResolver via CharacterCustomizer constructor parameter |
| 27 | Define `public const int InitialCharacterCount = 14;` in Era.Core/Constants/GameConstants.cs as shared constant |
| 28 | Inject ICharacterManager via TalentCopier constructor parameter |
| 29 | Inject IEngineVariables via TalentCopier constructor parameter |
| 30 | Compile Era.Core project cleanly with dotnet build |
| 31 | Include ClothesCustom method/logic in CharacterCustomizer for CHARA_CUSTUM→CLOTHES_CUSTOM call chain |
| 32 | Inject IVariableStore via TalentCopier constructor parameter |
| 33 | Inject IVariableStore via CharacterCustomizer constructor parameter |
| 34 | Inject ICommonFunctions via CharacterCustomizer constructor parameter |
| 35 | Inject IConsoleOutput via CharacterCustomizer constructor parameter |
| 36 | Inject TalentCopier via CharacterCustomizer constructor parameter (for CUSTOM_TERMINAL→COPY_CUSTOM delegation) |
| 37 | Register CharacterCustomizer, TalentCopier, and TalentNameResolver in ServiceCollectionExtensions.cs (per F774 ShopSystem precedent) |
| 38 | Write unit tests in Era.Core.Tests/Character/TalentCopierTests.cs exercising CFLAG:311 reordering, 191-talent equivalence, and source bound check |
| 39 | Include excluded talent indices (3-5, 9, 34, 38, 72-76, 84, 85, 153-155) as named collection in CharacterCustomizer |
| 40 | Include IsFemale gender restriction in VirginCustom method (NOT HasVagina — different semantics) |
| 41 | Include source character bound check (1 ≤ source < InitialCharacterCount) in CopyCustom |
| 42 | Include case 70 (快感の否定/快感に素直) in TalentNameResolver switch expression (mid-range verification) |
| 43 | Inject IConsoleOutput via TalentCopier constructor parameter (REVERSEMODE_1/COPY_CUSTOM use PRINTL/PRINTFORML) |
| 44 | Inject IInputHandler via TalentCopier constructor parameter (REVERSEMODE_1/COPY_CUSTOM use INPUT) |
| 45 | Include PrintStateAbl, PrintStateTalent, PrintStateExp as no-op stub methods in CharacterCustomizer |
| 46 | Include pagination logic (TalentsPerPage, page navigation) in CharacterCustomizer.TalentCustom method |
| 47 | Write unit test verifying empty input preservation for player character NAME_CUSTOM |
| 48 | Include talent type-dependent modification logic (on/off toggle vs quantitative cycling) using TalentNameResolver in CharacterCustomizer.TalentCustom |
| 49 | Include MAXBASE initialization (2000) in CharacterCustomizer.CustomCharamake |
| 50 | Include NO-based player guard in CharacterCustomizer.ClothesCustom |
| 51 | Include conditional gender initialization (TALENT:2=1 non-player, =2 player) in CharacterCustomizer.CustomCharamake |
| 52 | Include multi-value case 61 (潔癖症: -2/-1/1/2) in TalentNameResolver switch expression |
| 53 | Include NO initialization (SetCharacterNo=0) in CharacterCustomizer.CustomCharamake |
| 54 | Include HasVagina (NOT IsFemale) gender check in CharacterCustomizer.ClothesCustom for clothing pattern |
| 55 | Add `string GetTalentName(int index)` to ICsvNameResolver interface (simple CSV array accessor) |
| 56 | Implement mutual exclusion invariants (肉便器/公衆便所, 肉便器/NTR) in CharacterCustomizer.CustomTerminal |
| 57 | Write TalentNameResolver unit tests covering known cases, multi-value patterns, CASEELSE CSV fallback, and value==0 |
| 58 | Implement gender-based ability filtering (HasVagina) in ABL_CUSTOM for 同性愛関係 abilities (indices 17/32, 18/33) |
| 59 | Use named sentinel constant (InitialCharacterCount + 1) for CFLAG:311 reorder value in TalentCopier.ReverseMode1 |
| 60 | Inject TalentNameResolver via CharacterCustomizer constructor parameter (for talent type-dependent modification in CUSTOM_TERMINAL) |
| 61 | Write VirginCustom unit test verifying all 6 mutations (TALENT:処女=1, EXP:V経験/V性交経験/A性交経験=0, ABL:V感覚/A感覚=0) for IS_FEMALE characters |
| 62 | Implement gender-based experience filtering (HasVagina) in EXP_CUSTOM for indices 51/52 |
| 63 | Include page count bound (4 pages) in CharacterCustomizer.TalentCustom |
| 64 | Write unit test verifying VirginCustom skips player character (index 0) |
| 65 | Implement HasPenis check in CUSTOM_TERMINAL for 精力/童貞 normalization (NOT !HasVagina — futanari distinction) |
| 66 | Implement 処女 state handling in CUSTOM_TERMINAL (3-branch: virgin→zero abilities, non-virgin with vagina and V経験==0→set V経験=1, else→no-op) |
| 67 | Implement ABL index >= 30 exclusion (中毒 abilities) in ABL_CUSTOM |
| 68 | Implement MIN(input, 50) cap in EXP_CUSTOM |
| 69 | Include named constants for BASE_CUSTOM modification bounds (BaseMin=1000, BaseMax=3000, BaseStep=100) in CharacterCustomizer.BaseCustom |
| 70 | Include ABL value range enforcement (0 minimum, 2 maximum) in CharacterCustomizer.AblCustom |
| 71 | Ensure CharaCustum method calls RequestOneInput for [999] confirmation flow |
| 72 | Implement ClothesCustom secondary maid uniform prompt (female clothing only, 執事 is unreachable dead text in ERB) with CFLAG += RESULT*2 calculation in CharacterCustomizer |
| 73 | Use named constant for CFLAG index 311 in TalentCopier.ReverseMode1 (CharacterFlagIndex.ReorderSentinel or CflagIndex.ReorderSentinel) |
| 74 | Call RequestStringInput for text name input in CharacterCustomizer.NameCustom (NOT RequestNumericInput) |
| 75 | Implement HAS_VAGINA (NOT HAS_PENIS) gender-based ability/experience filtering in CUSTOM_TERMINAL (SHOP_CUSTOM.ERB:21-41) for 同性愛関係 indices |
| 76 | Implement 童貞 special case toggle handling in TalentCustom (SHOP_CUSTOM.ERB:163 `TALENTNAME:RESULT == GET_TALENTNAME(RESULT,1) || RESULT == 1` pattern) |
| 77 | Implement gender (talent 2) special-case cycling (1→2→3→1) in CustomTerminal modification logic, distinct from on/off toggle |
| 78 | Implement 性別嗜好 (talent 81) special-case cycling (-1→0→1→2→3→-1) in CustomTerminal modification logic, distinct from gender cycling |
| 79 | Write equivalence test for REVERSEMODE_1 character reorder verifying CFLAG:311 sentinel logic |
| 80 | Include disabled-item color constant (0x999999) in CharacterCustomizer for SETCOLOR calls [C10] |
| 81 | Write unit test verifying ClothesCustom rejects non-player character (player-only restriction) |
| 82 | Write unit test verifying CustomTerminal loop completes with no-op PRINT_STATE stubs |
| 83 | Write unit test verifying 処女 state Branch 2 (non-virgin vagina V経験==0 → set V経験=1) |
| 84 | Implement two-step gender pattern (GetTalent→.Value→HasVagina/HasPenis) in CharacterCustomizer |
| 85 | Call GetTarget in TalentCopier.CopyCustom for target character reading |
| 86 | Declare Gender=2, MaleVirginity=1, SexualOrientation=81, Lewdness=4 in TalentIndex |
| 87 | Call _nameResolver.GetTalentName in TalentNameResolver CASEELSE fallback path |
| 88 | Implement two-step gender pattern (GetTalent→.Value→IsFemale) in VirginCustom |
| 89 | Call _talentNameResolver.GetTalentName in CharacterCustomizer.CustomTerminal for talent type determination |
| 90 | Implement CharaCustum→CustomTerminal delegation call (CHARA_CUSTUM.ERB:14 CALL CUSTOM_TERMINAL pattern) [C2] |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| 191 vs 192 talent count | A) Use 192 (match F368), B) Use 191 (match ERB), C) Investigate discrepancy | B (191) | TALENTCOPY.ERB:94 `FOR LOCAL,0,191` is exclusive upper bound (0-190 = 191 iterations). F777 must match ERB exactly. F368 discrepancy tracked as out-of-scope issue per Scope Discipline |
| F368 integration | A) Replace TalentManager, B) Coexist with new classes, C) Merge into TalentCopier | B (Coexist) | F368 uses DTO pattern (dictionary-based), F777 uses IVariableStore pattern. Both serve different consumers. Clean separation until Phase 20 consolidation |
| GET_TALENTNAME extraction | A) Inline in CharacterCustomizer, B) Static utility class, C) Instance service with DI | C (Instance service) | 202-line SELECTCASE includes CASEELSE fallback requiring TALENTNAME CSV lookup. TalentNameResolver (non-static) receives ICsvNameResolver via constructor for CSV fallback (converts talent value→name for unhandled cases). Instance service enables dependency injection while maintaining single responsibility. |
| PRINT_STATE_* delegation | A) Implement fully, B) Stub with no-op, C) Defer feature | B (No-op stub) | PRINT_STATE_ABL/TALENT/EXP are display-only, not core logic. CUSTOM_TERMINAL calls these unconditionally every loop iteration (SHOP_CUSTOM.ERB:78-82); NotImplementedException would crash the loop. No-op stubs preserve call sites for future implementation while keeping entry points functional. Tracked in Mandatory Handoffs → F782 |
| Gender check pattern | A) Add HasVagina(CharacterId) to ICommonFunctions, B) Use two-step pattern (get gender, call HasVagina(int)) | B (Two-step) | ICommonFunctions.HasVagina already exists with genderValue parameter. Two-step pattern: `var g = _variables.GetTalent(id, TalentIndex.Gender); _commonFunctions.HasVagina(g.Value)` avoids API change |
| IEngineVariables TARGET/PLAYER scope | A) Add only setters, B) Add both getters and setters | B (Getters+Setters) | REVERSEMODE_1 writes TARGET (TALENTCOPY.ERB:29) and PLAYER (TALENTCOPY.ERB:39). GetTarget needed for COPY_CUSTOM (TALENTCOPY.ERB:87 reads TARGET). GetPlayer: PLAYER is write-only in F777 scope; added per Zero Debt Upfront (PLAYER read by other SYSTEM.ERB consumers in future migrations) |
| NO variable access scope | A) Add SetCharacterNo only (getter exists), B) Add both Get+Set | A (Setter only) | SHOP_CUSTOM.ERB:3 writes NO:ARG (needs SetCharacterNo), TALENTCOPY.ERB:44 reads NO:ARG (uses existing GetCharacterNo from F776). Only setter needed. |
| Named constant location | A) TalentCopier private const, B) GameConstants.InitialCharacterCount, C) GameConstants static class | B (Shared constant) | 開始時人数=14 is used in 5 active ERB files (CHARA_SET, CLOTHES, TALENTCOPY, NTR_UTIL, 子供の訪問関係) + DIM.ERH definition. Future C# migrations will all need this constant. Shared constant avoids duplication. Location: Era.Core/Constants/GameConstants.cs |
| ICsvNameResolver.GetTalentName purpose | A) Return value-dependent name (like TalentNameResolver does), B) Return simple CSV array value, C) No method needed | B (CSV array) | TalentNameResolver already handles value-dependent mapping. ICsvNameResolver.GetTalentName should be a simple CSV array lookup (pattern matches GetAblName/GetExpName/GetMarkName/GetPalamName). CASEELSE fallback: when talentValue!=0 but unhandled, use CSV name directly (no value substitution). |
| TalentCopier method signatures | A) Match ERB (CopyCustom(targetId), ReverseMode1()), B) Extract params for testability | A (Match ERB) | ERB equivalence is priority. Interactive selection logic (INPUT loop, character list display) is integral to COPY_CUSTOM/REVERSEMODE_1. Core logic extractable as private methods for InternalsVisibleTo unit testing. Public methods match ERB call signatures exactly (CopyCustom(int targetId) with source selected inside; ReverseMode1() with character selected inside). |
| SHOP_CUSTOM.ERB:168 dead code | A) Preserve in C#, B) Omit (unreachable) | B (Omit) | Line 168 IF LOCAL == 2 in quantitative talent ELSE branch is dead code: RESULT==2 handled at line 149; LOCAL >= 40 from TALENT_CUSTOM FOR loop. Omitting dead code for C# migration; equivalence tests verify behavior matches |

### Interfaces / Data Structures

**IEngineVariables Extensions** (Era.Core/Interfaces/IEngineVariables.cs):

```csharp
// Existing interface (13 methods preserved - AC#13, includes GetCharacterNo from F776)
// Add 8 new methods:

/// <summary>Set character NAME (runtime value) by 0-based index</summary>
void SetName(int characterIndex, string name);

/// <summary>Set character CALLNAME (runtime value) by 0-based index</summary>
void SetCallName(int characterIndex, string callName);

/// <summary>Set MASTER value (master character index, stored in MASTER:0)</summary>
void SetMaster(int value);

/// <summary>Get TARGET value (target character index, stored in TARGET:0)</summary>
int GetTarget();

/// <summary>Set TARGET value (target character index, stored in TARGET:0)</summary>
void SetTarget(int value);

/// <summary>Get PLAYER value (player character index, stored in PLAYER:0)</summary>
int GetPlayer();

/// <summary>Set PLAYER value (player character index, stored in PLAYER:0)</summary>
void SetPlayer(int value);

/// <summary>Set character NO value (character number) by 0-based index. GetCharacterNo already exists from F776.</summary>
void SetCharacterNo(int characterIndex, int value);
```

**ICsvNameResolver Extension** (Era.Core/Interfaces/ICsvNameResolver.cs):

```csharp
// Existing interface (4 methods preserved - AC#16)
// Add 1 new method:

/// <summary>Get talent name by index (TALENTNAME CSV array)</summary>
string GetTalentName(int index);
```

**IInputHandler Extension** (Era.Core/Input/IInputHandler.cs):

```csharp
// Existing interface (4 members preserved - AC#17)
// Add 1 new method:

/// <summary>Request single character input (ONEINPUT equivalent)</summary>
/// <param name="prompt">User prompt message</param>
/// <remarks>
/// Approximates ONEINPUT behavior via RequestStringInput with single-character validation.
/// Full ONEINPUT semantics (immediate keypress capture) deferred to engine integration.
/// </remarks>
Result<Unit> RequestOneInput(string prompt);
```

**CharacterCustomizer Constructor Signature**:

```csharp
public CharacterCustomizer(
    IVariableStore variables,
    IConsoleOutput console,
    IEngineVariables engineVars,
    IInputHandler inputHandler,
    ICsvNameResolver nameResolver,
    IStyleManager styleManager,
    ICommonFunctions commonFunctions,
    TalentNameResolver talentNameResolver,
    TalentCopier talentCopier)
```

**Note**: TalentNameResolver is now a regular (non-static) class injected via constructor. It receives ICsvNameResolver via its own constructor for CASEELSE CSV fallback. 9 injected dependencies total. ICharacterManager was removed as a phantom dependency — it is not used by SHOP_CUSTOM.ERB or CHARA_CUSTUM.ERB.

**TalentCopier Constructor Signature and Methods**:

```csharp
public TalentCopier(
    IVariableStore variables,
    IEngineVariables engineVars,
    ICharacterManager characterManager,
    IConsoleOutput console,
    IInputHandler inputHandler)

// Public methods matching ERB signatures:
public Result<Unit> ReverseMode1()
{
    // Character selection via interactive INPUT loop inside this method
    // Core reordering logic extractable as private method for testing via InternalsVisibleTo
}

public Result<Unit> CopyCustom(int targetId)
{
    // Source character selected interactively via INPUT loop inside this method
    // Core talent copy logic extractable as private method for testing via InternalsVisibleTo
    // Copies exactly 191 talents (indices 0-190, matching TALENTCOPY.ERB:94 FOR LOCAL,0,191)
}
```

**TalentNameResolver Structure** (non-static with ICsvNameResolver injection):

```csharp
namespace Era.Core.Character;

/// <summary>
/// Extracts GET_TALENTNAME logic (PRINT_STATE.ERB:316-518).
/// Maps talent indices to value-dependent display names.
/// For value==0 and unknown talents, falls back to TALENTNAME CSV via ICsvNameResolver.
/// </summary>
public class TalentNameResolver
{
    private readonly ICsvNameResolver _nameResolver;

    public TalentNameResolver(ICsvNameResolver nameResolver)
    {
        _nameResolver = nameResolver ?? throw new ArgumentNullException(nameof(nameResolver));
    }

    public string GetTalentName(int talentIndex, int talentValue)
    {
        return talentIndex switch
        {
            0 => talentValue != 0 ? "処女" : "",           // 処女 (SIF ARG:1)
            1 => talentValue == 0 ? "童貞" : "",            // 童貞 (SIF !ARG:1)
            2 => talentValue switch { 3 => "ふたなり", 2 => "オトコ", _ => "" },  // 性別
            4 => talentValue switch { 1 => "淫乱", 2 => "娼婦", _ => "" },        // 淫乱
            // ... 22 more cases (10-130): most use -1/1 pattern; cases 61, 81, 100, 105 use multi-value patterns ...
            // Most cases: -1 => negative form, 1 => positive form
            130 => talentValue switch { -1 => "回復遅い", 1 => "回復早い", _ => "" }, // 回復速度
            _ => talentValue != 0 ? _nameResolver.GetTalentName(talentIndex) : ""  // CASEELSE: CSV fallback
        };
    }
}
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| F368 192 vs 191 talent discrepancy | Out-of-Scope | Track as separate issue: TalentManager.CopyTalents copies 192 elements (Array.Copy count parameter), ERB TALENTCOPY.ERB:94 copies 191 (FOR LOCAL,0,191 exclusive upper). Investigate root cause: Is ERB bug or CSV definition change? Defer to Phase 20 consolidation feature |
| PRINT_STATE_ABL/TALENT/EXP not migrated | AC Design Constraints | Add Note in AC#14 Details: "PRINT_STATE_* stub delegation acceptable; full implementation deferred to future display feature" |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 3,4,5,6,7,8,27,86 | Extend IEngineVariables with 8 new methods (SetName, SetCallName, SetMaster, Get/SetTarget, Get/SetPlayer, SetCharacterNo) + update NullEngineVariables + create Era.Core/Constants/GameConstants.cs (including InitialCharacterCount=14 and ReorderSentinelValue=InitialCharacterCount+1); extend TalentIndex.cs with well-known indices (Gender=2, MaleVirginity=1, SexualOrientation=81, Lewdness=4) | | [x] |
| 2 | 55 | Add GetTalentName(int index) to ICsvNameResolver (CSV array accessor) + update NullCsvNameResolver | | [x] |
| 3 | 10 | Extend IInputHandler with RequestOneInput method + update InputHandler.cs concrete implementation + update test stubs (StubInputHandler, TestInputHandler); concrete InputHandler.RequestOneInput tested via CharaCustum integration tests (AC#14) | | [x] |
| 4 | 9,11,12,42,52,57,87 | Create TalentNameResolver.cs with all 26 talent cases (0-130), including multi-value patterns for cases 61, 81, 100, 105; includes TalentNameResolver unit tests (mini-TDD within Phase 2) | | [x] |
| 5 | 1,18,22,23,25,26,31,33,34,35,36,39,40,45,46,48,49,50,51,53,54,56,58,60,62,63,65,66,67,68,69,70,71,72,74,75,76,77,78,80,84,88,89,90 | Create CharacterCustomizer.cs with 4 entry points, ClothesCustom logic (HasVagina + maid secondary prompt), excluded talent indices, VirginCustom gender restriction (IsFemale), pagination logic (4 pages, 40 talents/page), PRINT_STATE no-op stubs, talent type-dependent modification including gender cycling (talent 2: 1→2→3→1) and 性別嗜好 cycling (talent 81: -1→0→1→2→3→-1), CustomCharamake initialization (gender + MAXBASE + BASE + NO), NO-based player guard, mutual exclusion normalization, ABL/EXP gender-based filtering (HasVagina), HasPenis 精力/童貞 normalization, 処女 state handling, ABL index >= 30 exclusion (中毒), EXP MIN(input,50) cap, BASE_CUSTOM named bounds constants (1000/3000/100), ABL_CUSTOM 0-2 range enforcement, CharaCustum RequestOneInput confirmation call, RequestStringInput for name input, HAS_VAGINA gender normalization, 童貞 special case toggle handling, two-step gender pattern (GetTalent→HasVagina/HasPenis), and 9 injected dependencies including TalentNameResolver | | [x] |
| 6 | 2,19,28,29,32,41,43,44,59,73,85 | Create TalentCopier.cs with 2 methods (ReverseMode1(), CopyCustom(int targetId) — matching ERB signatures), source character bound check, interactive character/source selection inside methods, named CFLAG index 311 constant, and 5 injected dependencies; add `ReorderSentinel = new(311)` to CharacterFlagIndex.cs (shared type, not private const) | | [x] |
| 7 | 14,15,38,47,61,64,79,81,82,83 | Write unit tests for CharacterCustomizer (gender exclusion, CUSTOM_TERMINAL normalization, entry point delegation, empty input name preservation, VirginCustom mutations, ClothesCustom player-only restriction, CustomTerminal stub loop completion, 処女 state Branch 2) and TalentCopier (CFLAG:311 reorder equivalence, 191-talent range equivalence, source bound) | | [x] |
| 8 | 13,16,17 | [Verification] Verify interface backward compatibility (13 existing IEngineVariables methods, 4 existing ICsvNameResolver methods, 4 existing IInputHandler members preserved) | | [x] |
| 9 | 20 | [Verification] Verify zero technical debt in all new/modified files (6 files) | | [x] |
| 10 | 21 | [Verification] Verify TalentCopier implementation details (191-talent range) | | [x] |
| 11 | 30 | [Verification] Build verification (Era.Core compiles cleanly) | | [x] |
| 12 | 37 | Register CharacterCustomizer, TalentNameResolver, and TalentCopier in ServiceCollectionExtensions.cs | | [x] |

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
| 1 | implementer | sonnet | Task 1,2,3: Interface extensions | Extended IEngineVariables, ICsvNameResolver, IInputHandler |
| 2 | implementer | sonnet | Task 4: TalentNameResolver extraction | TalentNameResolver.cs (non-static with ICsvNameResolver injection) with 26 switch cases; TalentNameResolver unit tests follow mini-TDD within Phase 2 (implement + test in same phase, Task 4 includes both AC#9 and AC#57). **Mini-TDD constraint**: tests MUST cover all documented cases (0,1,2,4,61,70,81,100,105,130 + CASEELSE + value==0) to compensate for missing independent RED phase. |
| 3 | tester | sonnet | Task 7: Unit test creation (RED) | CharacterCustomizerTests.cs, TalentCopierTests.cs (failing tests) |
| 4 | implementer | sonnet | Task 5: CharacterCustomizer implementation (includes TalentNameResolver injection) | CharacterCustomizer.cs with 4 entry points |
| 5 | implementer | sonnet | Task 6: TalentCopier implementation | TalentCopier.cs with ReverseMode1, CopyCustom |
| 6 | tester | sonnet | Task 7: Run tests (GREEN) | All unit tests pass |
| 7 | ac-tester | haiku | Task 8-12: Verify all ACs | AC verification report |

### Pre-conditions

1. F788 [DONE]: IConsoleOutput.DrawLine, Bar, PrintColumnLeft available
2. F790 [DONE]: IEngineVariables (13 methods including GetCharacterNo from F776), ICsvNameResolver (4 methods) baseline established
3. F791 [DONE]: IGameState mode transitions available

### Execution Order

**CRITICAL: Follow TDD RED→GREEN cycle**

1. **Phase 1**: Interface extensions (Task 1, 2, 3) MUST complete before Phase 2-6
2. **Phase 2**: TalentNameResolver (Task 4) depends on Task 2 (ICsvNameResolver.GetTalentName must exist before implementation)
3. **Phase 3**: Unit tests MUST be written BEFORE implementation (RED state)
4. **Phase 4-5**: Implementation creates CharacterCustomizer and TalentCopier (GREEN state)
5. **Phase 6**: Re-run tests to verify GREEN state
6. **Phase 7**: AC verification runs after all implementation complete

### Build Verification

After each phase, verify build succeeds:

```bash
MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'
```

### Success Criteria

- All 89 ACs pass (AC#1-AC#90, excluding deleted AC#24)
- TreatWarningsAsErrors=true (no compiler warnings)
- Unit tests achieve equivalence with ERB behavior
- F368 TalentManager.CopyTalents coexists (no breaking changes)

### Error Handling

- **Interface extension conflicts**: STOP → Report to user (may indicate concurrent F788-F791 changes)
- **Test failures in Phase 6**: Debug via implementer → ac-tester loop (max 3 iterations, then STOP)
- **Build failures**: STOP → Report syntax/dependency errors to user

### Implementation Details

#### Task 1: IEngineVariables Extension

Add to `Era.Core/Interfaces/IEngineVariables.cs` (after existing 13 methods, including GetCharacterNo from F776):

```csharp
// Character name setters
void SetName(int characterIndex, string name);
void SetCallName(int characterIndex, string callName);

// Game state variables
void SetMaster(int value);

// TARGET variable (entirely new)
int GetTarget();
void SetTarget(int value);

// PLAYER variable (entirely new)
int GetPlayer();
void SetPlayer(int value);

// NO variable setter (GetCharacterNo already exists from F776)
void SetCharacterNo(int characterIndex, int value);
```

**Implementation**: Delegate to GlobalStatic in engine layer (Era.Core defines interface only, engine provides implementation).

#### Task 3: IInputHandler Extension

Add to `Era.Core/Input/IInputHandler.cs` (after existing 4 members):

```csharp
/// <summary>Request single character input (ONEINPUT equivalent)</summary>
/// <param name="prompt">User prompt message</param>
/// <remarks>
/// Approximates ONEINPUT behavior via RequestStringInput with single-character validation.
/// Full ONEINPUT semantics (immediate keypress capture) deferred to engine integration.
/// </remarks>
Result<Unit> RequestOneInput(string prompt);
```

**Implementation**: Call RequestStringInput, validate length == 1, return Result<Unit>.

#### Task 4: TalentNameResolver Structure

Create `Era.Core/Character/TalentNameResolver.cs`:

```csharp
using System;

namespace Era.Core.Character;

/// <summary>
/// Extracts GET_TALENTNAME logic (PRINT_STATE.ERB:316-518).
/// Maps talent indices to value-dependent display names.
/// </summary>
public class TalentNameResolver
{
    private readonly ICsvNameResolver _nameResolver;

    public TalentNameResolver(ICsvNameResolver nameResolver)
    {
        _nameResolver = nameResolver;
    }

    public string GetTalentName(int talentIndex, int talentValue)
    {
        return (talentIndex, talentValue) switch
        {
            (0, var v) when v != 0 => "処女",
            (1, 0) => "童貞",
            // ... 22 more cases (10-130) ...
            (130, -1) => "回復遅い",
            (130, 1) => "回復早い",
            _ when talentValue != 0 => _nameResolver.GetTalentName(talentIndex),
            _ => string.Empty
        };
    }
}
```

**Source mapping**: PRINT_STATE.ERB:316-518 SELECTCASE structure → C# switch expression.

#### Task 5: CharacterCustomizer Constructor

```csharp
using Era.Core.Interfaces;
using Era.Core.Input;
using Era.Core.Types;

namespace Era.Core.Character;

public class CharacterCustomizer
{
    private readonly IVariableStore _variables;
    private readonly IConsoleOutput _console;
    private readonly IEngineVariables _engineVars;
    private readonly IInputHandler _inputHandler;
    private readonly ICsvNameResolver _nameResolver;
    private readonly IStyleManager _styleManager;
    private readonly ICommonFunctions _commonFunctions;
    private readonly TalentNameResolver _talentNameResolver;
    private readonly TalentCopier _talentCopier;

    public CharacterCustomizer(
        IVariableStore variables,
        IConsoleOutput console,
        IEngineVariables engineVars,
        IInputHandler inputHandler,
        ICsvNameResolver nameResolver,
        IStyleManager styleManager,
        ICommonFunctions commonFunctions,
        TalentNameResolver talentNameResolver,
        TalentCopier talentCopier)
    {
        _variables = variables ?? throw new ArgumentNullException(nameof(variables));
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _engineVars = engineVars ?? throw new ArgumentNullException(nameof(engineVars));
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
        _nameResolver = nameResolver ?? throw new ArgumentNullException(nameof(nameResolver));
        _styleManager = styleManager ?? throw new ArgumentNullException(nameof(styleManager));
        _commonFunctions = commonFunctions ?? throw new ArgumentNullException(nameof(commonFunctions));
        _talentNameResolver = talentNameResolver ?? throw new ArgumentNullException(nameof(talentNameResolver));
        _talentCopier = talentCopier ?? throw new ArgumentNullException(nameof(talentCopier));
    }

    // Entry points: CustomCharamake, CustomTerminal, CharaCustum, VirginCustom
    public Result<Unit> CustomCharamake(int characterId) { /* SHOP_CUSTOM.ERB logic */ }
    public Result<Unit> CustomTerminal(int characterId) { /* SHOP_CUSTOM.ERB:CUSTOM_TERMINAL logic */ }
    public Result<Unit> CharaCustum() { /* CHARA_CUSTUM.ERB logic - menu-driven flow, delegates to CustomTerminal */ }
    public Result<Unit> VirginCustom() { /* SHOP_CUSTOM.ERB:VIRGIN_CUSTOM logic - iterates all characters internally */ }
}
```

**Pattern**: while(true) loops for RESTART/GOTO equivalence (proven in F774 ShopSystem.cs).

#### Task 6: TalentCopier Implementation

```csharp
using Era.Core.Interfaces;
using Era.Core.Types;

namespace Era.Core.Character;

public class TalentCopier
{
    private readonly IVariableStore _variables;
    private readonly IEngineVariables _engineVars;
    private readonly ICharacterManager _characterManager;
    private readonly IConsoleOutput _console;
    private readonly IInputHandler _inputHandler;

    // Uses GameConstants.InitialCharacterCount (= 14, from DIM.ERH:56 開始時人数)
    // Uses GameConstants.ReorderSentinelValue (= InitialCharacterCount + 1 = 15) for CFLAG:311 sentinel during ReverseMode1

    public TalentCopier(
        IVariableStore variables,
        IEngineVariables engineVars,
        ICharacterManager characterManager,
        IConsoleOutput console,
        IInputHandler inputHandler)
    {
        _variables = variables ?? throw new ArgumentNullException(nameof(variables));
        _engineVars = engineVars ?? throw new ArgumentNullException(nameof(engineVars));
        _characterManager = characterManager ?? throw new ArgumentNullException(nameof(characterManager));
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
    }

    // Methods (parameterless, character selection is interactive inside each method)
    public Result<Unit> ReverseMode1() { /* TALENTCOPY.ERB:REVERSEMODE_1 logic — character selected via INPUT inside */ }
    public Result<Unit> CopyCustom(int targetId) { /* TALENTCOPY.ERB:COPY_CUSTOM logic — source selected via INPUT inside */ }
}
```

**CRITICAL**: CopyCustom MUST use `for (int i = 0; i < 191; i++)` or `for (int i = 0; i <= 190; i++)` to copy exactly 191 talents (matching TALENTCOPY.ERB:94 `FOR LOCAL,0,191` exclusive upper bound).

#### Task 7: Unit Test Structure

Create `Era.Core.Tests/Character/CharacterCustomizerTests.cs`:

```csharp
[TestClass]
public class CharacterCustomizerTests
{
    [TestMethod]
    public void CustomTerminal_GenderExclusion_MaleOnly() { /* Test C3 */ }

    [TestMethod]
    public void CustomTerminal_GenderExclusion_FemaleOnly() { /* Test C3 */ }

    [TestMethod]
    public void CustomTerminal_GenderExclusion_Both() { /* Test C3 */ }

    [TestMethod]
    public void CharaCustum_DelegatesToCustomTerminal() { /* Test C2 */ }
}
```

Create `Era.Core.Tests/Character/TalentCopierTests.cs`:

```csharp
[TestClass]
public class TalentCopierTests
{
    [TestMethod]
    public void CopyCustom_Copies191Talents() { /* Test C5: Verify indices 0-190 copied, index 191 NOT touched */ }

    [TestMethod]
    public void ReverseMode1_ReordersCharacters() { /* Test C6: CFLAG:311 reordering logic */ }
}
```

**Equivalence test pattern**: Mock IVariableStore, set up source character with known talent values, call CopyCustom, verify exactly 191 GetTalent/SetTalent calls.

### Named Constants

DIM.ERH:56 defines `開始時人数 = 14`. C# implementation:

```csharp
// Era.Core/Constants/GameConstants.cs (shared — used by 5+ ERB migrations)
public const int InitialCharacterCount = 14;
```

Do NOT use magic number `14` in code. Use `GameConstants.InitialCharacterCount`.

### PRINT_STATE_* Stub Delegation

CUSTOM_TERMINAL calls PRINT_STATE_ABL/TALENT/EXP unconditionally every loop iteration (SHOP_CUSTOM.ERB:78-82). Stub these as no-op:

```csharp
private void PrintStateAbl(int characterId)
{
    // No-op: PRINT_STATE_ABL display deferred to F782. Must not throw — called every loop iteration.
}
```

AC#45 verifies CustomTerminal loop completes without exception with these no-op stubs.

### F368 Integration Note

F368 `TalentManager.CopyTalents` (192-element copy) coexists with F777 `TalentCopier.CopyCustom` (191-element copy). Both remain callable. No breaking changes to F368 code.

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| F368 TalentManager 192 vs 191 talent discrepancy | Out-of-scope investigation: TALENTCOPY.ERB:94 copies 191 (FOR LOCAL,0,191), TalentManager.CopyTalents copies 192 (Array.Copy count). Root cause unclear (ERB bug vs CSV definition change). F782 must: (1) Check Game/CSV/ talent definitions for index 191 existence, (2) Determine if ERB 191 is bug or intentional, (3) Reconcile TalentManager and TalentCopier counts. | Feature | F782 | - |
| PRINT_STATE_ABL/TALENT/EXP display migration | Stub delegation in CharacterCustomizer. Display functions from PRINT_STATE.ERB not in F777 scope. | Feature | F782 | - |
| IConsoleOutput lacks PrintFormLine (PRINTFORML equivalent) | SHOP_CUSTOM.ERB uses PRINTFORML extensively; current workaround is PrintLine + string interpolation. Should be added as proper interface method to eliminate per-migration workaround. | Feature | F782 | - |

<!-- Validation (FL PHASE-7):
- Option A: Creation Task exists → OK (file created during /run)
- Option B: Referenced Feature exists → OK
- Option C: Phase exists in architecture.md → OK

DRAFT Creation Rules (FL POST-LOOP Step 6.3):
- Destination = Feature + Destination ID = "-" → Create DRAFT with explicit executor
- Always include: Title, Type, Summary, Why Now, Priority, Executor
- Use exact template from POST-LOOP.md Step 6.3
-->

---

## Execution Log
| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-18 | DEVIATION | ac-static-verifier | code AC verification | 70/81 passed, 11 FAIL |
| 2026-02-18 | DEVIATION | ac-static-verifier | code AC verification (resume) | 70/81 passed, 11 FAIL (exit 1) |
| 2026-02-18 | FIX | debugger | fix 11 AC pattern mismatches | IInputHandler comment, TalentIndex annotations, CharacterCustomizer code/comments |
| 2026-02-18 | PASS | ac-static-verifier | code AC re-verification | 81/81 passed |
| 2026-02-18 | PASS | ac-static-verifier | file AC verification | 3/3 passed |
| 2026-02-18 | PASS | ac-static-verifier | build AC verification | 1/1 passed |
| 2026-02-18 | PASS | ac-tester | test AC verification | 4/4 passed (AC#14:20, AC#38:13, AC#57:87, AC#61:5) |
| 2026-02-18 | COMPLETE | orchestrator | All 89 ACs verified | 89/89 PASS |
| 2026-02-18 | DEVIATION | feature-reviewer | post-review NEEDS_REVISION | 4 critical, 2 major issues |
| 2026-02-18 | FIX | debugger | fix 3 real behavioral bugs | Gender cycling, ExpMilkExperience, CopyCustom bound check |
| 2026-02-18 | PASS | ac-static-verifier | code AC re-verification post-fix | 81/81 passed |
| 2026-02-18 | PASS | dotnet test | test re-verification post-fix | 2240/2240 passed |
| 2026-02-18 | UPDATE | implementer | SSOT update engine-dev SKILL.md | IEngineVariables+8, ICsvNameResolver+1, IInputHandler+1 |

---

## Review Notes
<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A->B->A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->
- [fix] Phase1-RefCheck iter1: [DEP-001] Links section | F782 referenced in Mandatory Handoffs but missing from Links section
- [resolved-applied] Phase2-Pending iter1: [AC-006] ICsvNameResolver.GetTalentName 1-param vs 2-param mismatch. → Applied: Option B (Remove GetTalentName from ICsvNameResolver, inject TalentNameResolver directly) selected in FL POST-LOOP.
- [fix] PostLoop-UserFix iter4: [AC-006] ICsvNameResolver GetTalentName removal | Removed GetTalentName from ICsvNameResolver scope; AC#9 repurposed to verify TalentNameResolver injection; Task 2 removed; Technical Design/Problem/Goal/5Whys updated
- [fix] PostLoop-UserFix iter4: [INV-003] TalentNameResolver non-static | Changed from static to instance class with ICsvNameResolver DI; added AC#55 for ICsvNameResolver.GetTalentName CSV accessor; re-added Task 2; updated CASEELSE to use injected CSV lookup
- [fix] PostLoop-UserFix iter4: [AC-006] AC#24 phantom dependency removal | Removed ICharacterManager from CharacterCustomizer; AC#24 deleted; constructor 9→8 deps
- [fix] PostLoop-UserFix iter4: [INV-003] TalentCopier ERB signature matching | CopyCustom(sourceId,targetId)→CopyCustom(targetId); ReverseMode1(characterId)→ReverseMode1(); internal INPUT loops; InternalsVisibleTo for test
- [fix] Phase2-Review iter1: [INV-003] CharacterCustomizer constructor | Added TalentNameResolver as 9th dependency (non-static instance injection); updated dep count 8→9
- [fix] Phase2-Review iter1: [INV-003] Task 4 code snippet | Changed from static to non-static with ICsvNameResolver constructor injection and _nameResolver CASEELSE fallback
- [fix] Phase2-Review iter1: [AC-005] AC#37 DI registration | Updated to include TalentNameResolver (count 2→3); Task 12 updated
- [fix] Phase2-Review iter2: [AC-005] AC#56 mutual exclusion | Added AC for CustomTerminal mutual exclusion normalization (肉便器/公衆便所, 肉便器/NTR); updated C3; AC count→55
- [fix] Phase2-Review iter2: [INV-003] Philosophy Derivation row 3 | Changed from TalentNameResolver extraction to Mandatory Handoffs + entry points (AC#18,19,45)
- [fix] Phase2-Review iter3: [FMT-002] AC#37 table/detail inconsistency | Table pattern updated to include TalentNameResolver, count 2→3
- [fix] Phase2-Review iter3: [INV-003] CustomTerminal parameter | Renamed from 'mode' to 'characterId' (ERB passes character index)
- [fix] Phase2-Review iter3: [AC-005] AC#57 TalentNameResolver unit tests | Added test execution AC; updated Goal Coverage, Task 7, AC count→56
- [fix] Phase2-Review iter4: [AC-002] AC#46 | Strengthened pagination matcher to verify concrete page size 40
- [fix] Phase2-Review iter4: [CON-001] C19 CFLAG:311 sentinel | Added constraint documenting sentinel > InitialCharacterCount; Task 6 implementation note
- [fix] Phase2-Review iter4: [AC-005] AC#58 gender filtering | Added AC for ABL_CUSTOM/EXP_CUSTOM gender-incompatible ability exclusion; AC count→57
- [fix] Phase2-Review iter5: [AC-005] AC#56,58 orphan fix | Added AC#56,58 to Task 5 AC list
- [fix] Phase2-Review iter5: [INV-003] AC#9 table/detail consistency | Aligned Details to match table (Grep TalentNameResolver.cs for ICsvNameResolver)
- [fix] Phase2-Review iter5: [AC-005] AC#59 sentinel | Added AC for CFLAG:311 named sentinel constant [C19]; AC count→58
- [fix] Phase2-Review iter1: [INV-003] Technical Design CharaCustum signature | CharaCustum(int characterId) corrected to CharaCustum() matching CHARA_CUSTUM.ERB parameterless entry
- [fix] Phase2-Review iter1: [AC-005] AC Coverage | Added AC#31 for CLOTHES_CUSTOM call chain from CHARA_CUSTUM.ERB:22
- [fix] Phase2-Review iter2: [INV-003] Task 1 + Technical Design + 5 Whys | IEngineVariables method count corrected from 7 to 9 (total interface methods from 9 to 11)
- [fix] Phase2-Review iter2: [FMT-002] Technical Design + Success Criteria | AC count corrected from 30 to 31
- [fix] Phase2-Review iter3: [FMT-002] Review Notes | Added missing category codes to all entries
- [fix] Phase2-Review iter3: [INV-003] Technical Design TalentNameResolver | Case mappings corrected to match PRINT_STATE.ERB:316-518 (case 0=処女, case 1=童貞, value conditions use -1/1 not 0/1)
- [fix] Phase2-Review iter3: [INV-003] Technical Design VirginCustom signature | VirginCustom(int characterId) corrected to VirginCustom() matching SHOP_CUSTOM.ERB parameterless entry
- [fix] Phase2-Review iter4: [AC-005] AC Coverage | Added AC#32 for TalentCopier IVariableStore injection
- [fix] Phase2-Review iter5: [FMT-002] AC Details ordering | Swapped AC#31 and AC#32 to match sequential order
- [fix] Phase2-Review iter5: [AC-005] AC Coverage | Added AC#33 (CharacterCustomizer IVariableStore), AC#34 (ICommonFunctions), AC#35 (IConsoleOutput)
- [resolved-applied] Phase2-Pending iter6: [INV-003] TalentNameResolver CASEELSE CSV lookup issue. → Applied: Option A (Non-static with ICsvNameResolver injection) selected in FL POST-LOOP. Added AC#55, re-added Task 2, updated Technical Design.
- [fix] Phase2-Uncertain iter6: [CON-001] AC Design Constraints | Added C18 for COPY_CUSTOM source character bound (TALENTCOPY.ERB:86)
- [fix] Phase2-Review iter7: [FMT-001] Acceptance Criteria + Technical Design | Added missing ownership comments
- [fix] Phase2-Review iter7: [AC-005] AC Coverage | Added AC#36 for CharacterCustomizer TalentCopier injection (CUSTOM_TERMINAL→COPY_CUSTOM cross-class call)
- [fix] Phase3-Maintainability iter8: [SCP-004] Mandatory Handoffs | Added PRINT_STATE_ABL/TALENT/EXP display migration handoff
- [fix] Phase3-Maintainability iter8: [AC-005] AC Coverage | Added AC#37 for DI registration + Task 12
- [fix] Phase3-Maintainability iter8: [TSK-001] Task 1/2 descriptions | Added NullEngineVariables/NullCsvNameResolver update requirement
- [fix] Phase3-Maintainability iter8: [INV-003] F368 Integration | Documented consumer boundary (TalentManager=DTO, TalentCopier=IVariableStore)
- [fix] Phase3-Maintainability iter8: [AC-002] AC#27 | Replaced unreliable not_matches with positive matches assertion
- [fix] Phase2-Review iter1: [FMT-002] AC Details ordering | Swapped AC#36 and AC#37 to correct sequential order
- [fix] Phase2-Review iter1: [AC-005] AC Coverage | Added AC#38 (TalentCopier unit tests), AC#39 (excluded talent indices C11), AC#40 (VirginCustom gender C12), AC#41 (CopyCustom source bound C18)
- [fix] Phase2-Review iter2: [SCP-004] Mandatory Handoffs | PRINT_STATE handoff assigned to F782 (was Destination ID = '-')
- [fix] Phase2-Review iter2: [FMT-002] Execution Log | Cleared template placeholder data (2025-12-23 entries)
- [resolved-applied] Phase2-Pending iter2: [DES-001] PRINT_STATE_* stubs use NotImplementedException which throws at runtime. Design decision needed: (A) Replace with no-op Result.Ok stubs (safe but silent), or (B) Keep NotImplementedException (explicit failure for unimplemented paths). → Applied: Option A (no-op stubs) selected in FL iter1 Phase 2.
- [fix] Phase2-Review iter3: [FMT-001] Background section | Removed consumed fc-time HTML instruction comments
- [fix] Phase2-Review iter3: [AC-002] AC#39 | Improved matcher from overly broad digit pattern to named collection identifier
- [fix] Phase2-Review iter3: [AC-002] AC#40 | Corrected matcher from IsFemale|HasVagina to IsFemale only (matching ERB IS_FEMALE semantics, not HAS_VAGINA)
- [fix] Phase2-Review iter4: [AC-005] AC Coverage | Added AC#42 (TalentNameResolver mid-range case 60 verification)
- [fix] Phase2-Review iter4: [CON-001] AC Design Constraint C7 | Corrected description from 'MASTER uses default' to 'player (ARG==0) keeps current name'
- [fix] Phase2-Review iter5: [FMT-002] Success Criteria | AC count corrected from 41 to 44
- [fix] Phase2-Review iter5: [AC-002] AC#42 | Corrected from non-existent case 60 (従順) to valid case 70 (快感)
- [fix] Phase2-Review iter5: [AC-005] AC Coverage | Added AC#43 (TalentCopier IConsoleOutput), AC#44 (TalentCopier IInputHandler) + updated constructor to 5 deps
- [fix] Phase2-Review iter6: [FMT-002] Success Criteria | AC count corrected from 42 to 44 (missed case-sensitive occurrence)
- [fix] Phase2-Review iter7: [AC-005] Goal Coverage | Added AC#27 to Goal Item 2 (TalentCopier migration)
- [fix] Phase2-Review iter7: [INV-003] AC#13 + Baseline + C14 | IEngineVariables method count corrected from 11 to 12 (SetMoney was missing from baseline and regex)
- [fix] Phase2-Review iter8: [FMT-002] Pre-conditions + Implementation Details | Remaining '11 methods' references corrected to 12
- [fix] Phase2-Review iter8: [FMT-002] IEngineVariables Extensions comment | 'Add 7 new methods' corrected to 'Add 9 new methods'
- [fix] Phase2-Review iter9: [AC-002] AC#41 | Removed 'InitialCharacterCount' from regex alternatives (trivially satisfied by AC#27 constant definition)
- [fix] Phase2-Review iter1: [DES-001] Technical Design PRINT_STATE stub | Changed from NotImplementedException to no-op stub (CUSTOM_TERMINAL calls unconditionally every loop iteration)
- [fix] Phase2-Review iter1: [AC-005] AC Coverage | Added AC#45 (CustomTerminal no-op stub loop completion test), AC#46 (pagination logic C4), AC#47 (empty input name preservation C7)
- [fix] Phase2-Review iter2: [FMT-002] Success Criteria | AC count corrected from 44 to 47
- [fix] Phase2-Review iter2: [INV-003] Implementation Details PRINT_STATE stub | Updated code snippet from NotImplementedException to no-op stub (matching resolved decision)
- [resolved-applied] Phase2-Uncertain iter3: [AC-006] AC#24 CharacterCustomizer ICharacterManager phantom dependency. → Applied: Option A (Remove AC#24, remove ICharacterManager from CharacterCustomizer constructor, 9→8 deps) selected in FL POST-LOOP.
- [fix] Phase3-Maintainability iter3: [DES-001] Named constant location | Changed from private const in TalentCopier to shared GameConstants.InitialCharacterCount (5+ ERB files use 開始時人数)
- [resolved-applied] Phase3-Maintainability iter3: [INV-003] TalentCopier method signatures → Applied: Option A (Match ERB signatures) selected. CopyCustom(int targetId) with source selected via interactive INPUT inside; ReverseMode1() parameterless with character selected via interactive INPUT inside. Core logic (talent copy, character reorder) extractable as private methods for InternalsVisibleTo unit testing.
- [fix] Phase3-Maintainability iter3: [DES-001] Dependency count rationale | Added documentation explaining why 8-9 dependencies is justified (shared CUSTOM_TERMINAL dispatcher loop)
- [fix] Phase3-Maintainability iter3: [SCP-004] F368 handoff specificity | Added concrete investigation steps to F782 handoff (check CSV talent 191, determine ERB bug vs intentional)
- [fix] Phase3-Maintainability iter3: [AC-002] AC#45 | Changed from duplicate test AC to code inspection AC (Grep for PrintStateAbl/Talent/Exp no-op stubs)
- [fix] Phase3-Maintainability iter3: [DEP-001] F775 status | Updated from [WIP] to [DONE] (drift candidate: completed since F777 design)
- [fix] Phase2-Review iter4: [AC-005] AC Coverage | Added AC#48 (talent type-dependent modification C4), AC#49 (CustomCharamake MAXBASE init), AC#50 (ClothesCustom NO guard C13)
- [fix] Phase2-Review iter5: [FMT-002] Success Criteria | AC count corrected from 47 to 51
- [fix] Phase2-Review iter5: [AC-005] AC Coverage | Added AC#51 (CustomCharamake gender initialization)
- [fix] Phase2-Review iter5: [AC-002] AC#47 | Updated to cover both NAME and CALLNAME empty input preservation
- [fix] Phase2-Review iter5: [INV-003] C3 description | Changed from 'menu filtering' to 'value normalization' (matching SHOP_CUSTOM.ERB:19-67 behavior)
- [fix] Phase2-Review iter6: [FMT-002] Success Criteria + Technical Design | AC count corrected to 53
- [fix] Phase2-Review iter6: [INV-003] TalentCopier code snippet | Replaced private const with GameConstants.InitialCharacterCount reference
- [fix] Phase2-Review iter6: [INV-003] TalentNameResolver comment | Corrected from 'using -1/1 pattern' to note multi-value cases 61,81,100,105
- [fix] Phase2-Review iter6: [AC-005] AC Coverage | Added AC#52 (TalentNameResolver multi-value case 61), AC#53 (CustomCharamake NO init)
- [fix] Phase2-Review iter6: [AC-002] AC#49 | Updated matcher to cover both MAXBASE and BASE initialization
- [fix] Phase2-Review iter7: [FMT-002] Success Criteria + Technical Design | AC count corrected to 54
- [fix] Phase2-Review iter7: [INV-003] Named Constants snippet | Updated from private const to shared GameConstants
- [fix] Phase2-Review iter7: [INV-003] AC#52 | Corrected case 61 value descriptions to match PRINT_STATE.ERB:426-435
- [fix] Phase2-Review iter7: [INV-003] AC#7 rationale | Corrected: PLAYER is write-only in F777 scope, GetPlayer for domain completeness
- [fix] Phase2-Review iter7: [AC-005] AC Coverage | Added AC#54 (ClothesCustom HasVagina, NOT IsFemale — futanari clothing pattern)
- [fix] Phase2-Review iter1: [FMT-002] AC Details ordering | Moved AC#51 to correct sequential position between AC#50 and AC#52
- [fix] Phase4-ACValidation iter2: [AC-002] AC#45 | Fixed escaped pipe `\|` to unescaped `|` for regex alternation in AC Definition Table (AC Details already correct)
- [fix] Phase2-Review iter3: [INV-003] AC#18 | Corrected description and rationale: CustomTerminal is internal delegation target, not SYSTEM.ERB entry point. 3 SYSTEM.ERB entry points on CharacterCustomizer + AC#19 covers ReverseMode1
- [fix] Phase2-Review iter1: [AC-005] AC Coverage | Added AC#60 (CharacterCustomizer TalentNameResolver injection); updated Goal Coverage Item 1 and 7, Task 5 AC list
- [fix] Phase2-Review iter1: [TSK-001] Task AC alignment | Moved AC#9 from Task 5 to Task 4 (AC#9 tests TalentNameResolver.cs, belongs in Task 4)
- [fix] Phase2-Review iter2: [FMT-002] AC Coverage numbering | Renumbered AC Coverage rows 24-54 to 25-55 (matching AC Definition Table after AC#24 deletion)
- [fix] Phase2-Review iter2: [AC-005] AC Coverage missing entries | Added AC#56-59 to AC Coverage table
- [fix] Phase2-Review iter2: [FMT-002] AC count | Corrected 55→60 in Technical Design and Success Criteria
- [fix] Phase2-Review iter2: [AC-005] VirginCustom mutation | Added AC#61 (VirginCustom unit test for 6 mutations); updated Goal Coverage, Task 7, AC Coverage
- [fix] Phase2-Uncertain iter1: [TSK-001] Task 3 scope | Updated Task 3 description to include InputHandler.cs concrete implementation + test stub updates (StubInputHandler, TestInputHandler)
- [fix] Phase2-Review iter1: [AC-002] AC#46 matcher | Removed bare '40' alternative; replaced with named constant assignment pattern (TalentsPerPage\s*=\s*40)
- [fix] Phase2-Uncertain iter1: [FMT-002] Goal wording | Updated Goal to mention both setters and missing accessors (TARGET, PLAYER, NO getter/setter pairs)
- [fix] Phase2-Review iter2: [FMT-002] C19 column mismatch | Merged C19 prefix into Constraint column (4→3 columns)
- [fix] Phase2-Uncertain iter2: [AC-002] AC#58 split | Split ABL_CUSTOM/EXP_CUSTOM into AC#58 (ABL) + AC#62 (EXP) for independent gender filtering verification
- [fix] Phase2-Review iter2: [AC-005] AC#63 | Added page count bound AC (4 pages) for C4 completeness
- [fix] Phase2-Uncertain iter2: [INV-003] Philosophy Derivation row 3 | Revised 'documented transition points' mapping to match AC#18/19/45 actual scope (callable architecture + stub boundaries)
- [fix] Phase2-Review iter2: [FMT-002] AC count | Corrected 60→63 in Technical Design and Success Criteria
- [fix] Phase2-Review iter3: [FMT-002] AC ordering | Reordered AC#58-63 to sequential order in AC Definition Table
- [fix] Phase2-Review iter3: [AC-002] AC#58,62 HasPenis→HasVagina | Corrected matchers to use HasVagina (not HasPenis) for futanari correctness; ERB uses !(HAS_VAGINA), not HAS_PENIS
- [fix] Phase2-Review iter3: [AC-005] AC#64 | Added VirginCustom player exclusion AC (FOR LOOP_CHR,1,CHARANUM skips index 0); updated Task 7, Goal Coverage
- [fix] Phase2-Uncertain iter3: [INV-003] Philosophy Derivation row 2 | Revised 'clear phase boundaries' to 'interfaces complete for customization subsystem'; added AC#30
- [fix] Phase2-Review iter3: [FMT-002] AC count | Corrected 63→64 in Technical Design and Success Criteria
- [fix] Phase2-Review iter4: [FMT-002] AC Coverage duplicates | Removed duplicate AC#62/63 rows; reordered AC#58-66 sequentially
- [fix] Phase2-Review iter4: [INV-003] AC#58 rationale | Corrected ABL indices 17/32 label from V感覚 to 同性愛関係 (レズっ気/レズ中毒)
- [fix] Phase2-Uncertain iter4: [AC-005] AC#65 | Added HasPenis 精力/童貞 normalization AC (HAS_PENIS != !HAS_VAGINA for futanari)
- [fix] Phase2-Uncertain iter4: [AC-005] AC#66 | Added 処女 state handling AC (3-branch: virgin→zero, non-virgin vagina V経験==0→set 1, else→no-op)
- [fix] Phase2-Review iter4: [FMT-002] AC count | Corrected 64→66 in Technical Design and Success Criteria
- [fix] Phase2-Review iter5: [FMT-002] AC count | Corrected 66→65 (actual count: AC#1-66 minus deleted AC#24)
- [fix] Phase2-Review iter5: [FMT-002] AC Details ordering | Reordered AC Details to sequential AC#58-66
- [fix] Phase2-Review iter6: [AC-005] AC#67 | Added ABL_CUSTOM addiction exclusion AC (SIF LOCAL >= 30 / RESTART excludes 中毒 abilities); updated Task 5 AC list, Goal Coverage Item 1, AC Coverage table
- [fix] Phase2-Review iter6: [AC-005] AC#68 | Added EXP_CUSTOM MIN(RESULT,50) cap AC (prevents arbitrarily high experience values); updated Task 5 AC list, Goal Coverage Item 1, AC Coverage table
- [fix] Phase2-Review iter6: [FMT-002] AC count | Corrected 65→67 in Technical Design and Success Criteria
- [fix] Phase2-Review iter6: [TSK-001] Implementation Contract Phase 2 | Added mini-TDD note for TalentNameResolver (implement + test within Phase 2)
- [fix] Phase2-Review iter6: [FMT-002] Success Criteria | Corrected remaining '63 ACs' to '65 ACs' (then to '67 ACs')
- [fix] Phase2-Review iter6: [FMT-002] AC#52 | Corrected description from '潔癖症' to '汚臭耐性' (matching AC Details)
- [fix] PostLoop-UserFix iter7: [CON-001] C19 dangling reference | Added C19 row to AC Design Constraints table (CFLAG:311 sentinel > InitialCharacterCount; TALENTCOPY.ERB:41)
- [fix] PostLoop-UserFix iter7: [FMT-002] Baseline TBD | Replaced 'TBD (run at /fc time)' with concrete '0 tests (no pre-existing tests for CharacterCustomizer/TalentCopier)'
- [fix] PostLoop-UserFix iter7: [FMT-002] AC#36 Coverage row | Removed duplicate TalentNameResolver reference (AC#60 covers separately); description now mentions TalentCopier injection only
- [fix] PostLoop-UserFix iter7: [AC-005] AC#69 | Added BASE_CUSTOM named constants AC for modification bounds (1000 min, 3000 max, 100 step) [C8]; updated Goal Coverage Item 1, Task 5, AC Coverage, AC count 67→69
- [fix] PostLoop-UserFix iter7: [AC-005] AC#70 | Added ABL_CUSTOM 0-2 value range enforcement AC [C3]; updated Goal Coverage Item 1, Task 5, AC Coverage
- [fix] PostLoop-UserFix iter7: [DES-001] SHOP_CUSTOM.ERB:168 dead code | Added Key Technical Decisions entry for line 168 dead code (B: Omit — unreachable IF LOCAL==2 branch)
- [fix] Phase2-Review iter8: [FMT-002] Baseline row 5 | Removed post-implementation metric (ICsvNameResolver 5 methods) from Baseline; already verified by AC#55+AC#16
- [fix] Phase2-Review iter8: [INV-003] Feasibility Assessment | Changed Interface Extensions from NEEDS_REVISION to FEASIBLE; updated verdict to FEASIBLE (feature purpose IS to add extensions; additive-only pattern proven by F788/F790)
- [fix] Phase1-RefCheck iter1: [DEP-001] F368 link path | Updated Links section F368 link from feature-368.md to archive/feature-368.md (F368 is archived)
- [fix] Phase2-Review iter1: [AC-005] AC#71 | Added RequestOneInput usage AC for CharaCustum confirmation flow (CHARA_CUSTUM.ERB:20 ONEINPUT); updated Goal Coverage, Task 5, AC Coverage, AC count 69→70
- [fix] Phase3-Maintainability iter2: [AC-005] AC#72 | Added ClothesCustom maid/butler secondary prompt AC (SHOP_CUSTOM.ERB:438-444); updated Task 5, AC Coverage, AC count 70→73
- [fix] Phase3-Maintainability iter2: [AC-005] AC#73 | Added CFLAG:311 index named constant AC (CharacterFlagIndex); updated Task 6, AC Coverage
- [fix] Phase3-Maintainability iter2: [AC-005] AC#74 | Added RequestStringInput AC for NAME_CUSTOM (SHOP_CUSTOM.ERB:243 INPUTS); updated Task 5, AC Coverage
- [resolved-skipped] Phase2-Review iter3: [FMT-002] AC#24 numbering gap | AC#24 deleted but gap not renumbered (AC#23→AC#25). User decision: gap accepted (AC numbers are identifiers, not sequential indices).
- [fix] Phase2-Review iter3: [AC-005] AC#75 | Added CUSTOM_TERMINAL HAS_VAGINA gender normalization code AC (SHOP_CUSTOM.ERB:21-41); updated Task 5, AC Coverage, AC count 73→75
- [fix] Phase2-Review iter1: [ALN-001] Task 4 AC#57 alignment | Moved AC#57 from Task 7 to Task 4 to align Tasks table with Implementation Contract Phase 2 (mini-TDD intent)
- [fix] Phase2-Review iter1: [DOC-001] Dependency count 8→9 | Updated Approach paragraph and Architecture Pattern to include TalentNameResolver in CharacterCustomizer's 9 injected dependencies
- [fix] Phase2-Review iter2: [GOAL-001] Goal ICsvNameResolver omission | Amended Goal to include ICsvNameResolver extension with GetTalentName (CSV array accessor for CASEELSE fallback)
- [fix] Phase2-Review iter2: [GOAL-002] Goal TalentNameResolver deliverable | Amended Goal to list TalentNameResolver.cs as explicit deliverable (extracting GET_TALENTNAME from PRINT_STATE.ERB:316-518)
- [fix] Phase2-Review iter3: [AC-005] AC#76 | Added 童貞 special case toggle AC (SHOP_CUSTOM.ERB:163 `|| RESULT == 1`); updated Task 5, AC Coverage
- [fix] Phase2-Review iter1: [AC-005] AC#77,AC#78 | Added gender cycling (talent 2: 1→2→3→1) and 性別嗜好 cycling (talent 81: -1→0→1→2→3→-1) ACs for CUSTOM_TERMINAL special cases (SHOP_CUSTOM.ERB:149-161); added C20 constraint; updated Task 5, AC Coverage, Goal Coverage Item 1, AC count 76→78
- [fix] Phase2-Review iter1: [AC-005] AC#72 correction | Revised AC#72 from maid/butler to maid-only (執事 is unreachable dead text in ERB); updated matcher to remove butler/執事 alternatives
- [fix] Phase2-Review iter2: [FMT-002] AC count | Updated AC count from 75 to 77 in Approach and Success Criteria sections
- [fix] Phase2-Review iter2: [AC-005] AC#77,AC#78 constraint tags | Changed constraint references from [C3] to [C20, C3] (C20 is primary constraint)
- [fix] Phase2-Review iter2: [INV-003] Philosophy Derivation row 2 | Added AC#20 to 'clear phase boundaries' AC Coverage (row text mentions 'zero technical debt' but AC#20 was omitted)
- [fix] Phase2-Review iter2: [INV-003] Philosophy Derivation row 1 | Added AC#38,57,61,64,79 to 'continuous development pipeline' AC Coverage (equivalence tests)
- [fix] Phase2-Review iter2: [FMT-002] AC Coverage AC#37 | Added TalentNameResolver to How-to-Satisfy entry
- [fix] Phase2-Review iter3: [FMT-002] IEngineVariables 12→13 remnants | Updated Impact Analysis, C14 Details, Task 8, Pre-conditions (4 locations still said 12)
- [fix] Phase2-Review iter3: [AC-002] AC#9 matcher | Strengthened from ICsvNameResolver|_nameResolver to TalentNameResolver\(ICsvNameResolver (verifies constructor DI, not just reference)
- [fix] Phase2-Review iter3: [INV-003] Key Decisions TARGET/PLAYER | Corrected 'reads them' to be specific: GetTarget needed for COPY_CUSTOM read; GetPlayer pending user decision
- [fix] Phase2-Review iter4: [AC-005] Goal Coverage Goal#1 | Added AC#75, AC#76 to Goal#1 Covering ACs (HAS_VAGINA gender normalization, 童貞 toggle)
- [fix] Phase2-Uncertain iter4: [INV-003] Philosophy Derivation row 1 | Expanded derived requirement to include domain logic preservation; added AC#56,58,62,65,66,75,77,78 to Philosophy coverage
- [resolved-skipped] Phase2-Pending iter5: [AC-005] SYSTEM.ERB call redirection. User decision: AC不要。ERBファイルが薄いラッパーとして残り、SYSTEM.ERBは既存ERB関数名を呼び続ける（F774先例）。SYSTEM.ERB変更はF777スコープ外。
- [resolved-applied] Phase2-Pending iter1: [DES-001] AC#7 GetPlayer scope. User decision: Option A (GetPlayer+SetPlayer both maintained). Rationale: Zero Debt Upfront — PLAYER is read by other SYSTEM.ERB consumers in future Phase 20 migrations; adding GetPlayer now completes interface extension in one pass.
- [fix] PostLoop-UserFix iter5: [DES-001] AC#7 rationale | Updated from 'domain completeness' to 'Zero Debt Upfront' justification; updated Key Decisions entry
- [fix] Phase2-Review iter1: [TSK-001] GameConstants.cs creation | Moved AC#27 from Task 10 to Task 1; Task 1 now creates GameConstants.cs; Task 10 verifies only AC#21
- [fix] Phase2-Review iter2: [AC-005] AC#80 | Added disabled-item color constant AC (0x999999 [C10]); updated Goal Coverage, Task 5, AC Coverage, AC count 78→79
- [fix] Phase2-Review iter3: [INV-003] Philosophy Derivation row 3 | Revised 'stubs mark explicit transition boundaries to F782' to 'F782 handoff documented in Mandatory Handoffs section' (matches actual AC#45 verification scope)
- [fix] Phase2-Review iter3: [AC-002] AC#45 matcher | Changed from `PrintState(Abl|Talent|Exp)` to `void PrintState(Abl|Talent|Exp)\(` to scope to method declarations only
- [fix] Phase2-Review iter1: [AC-005] AC#79 | Added REVERSEMODE_1 equivalence test existence AC (mirrors AC#15 for COPY_CUSTOM); updated Goal Coverage, Task 7, AC Coverage, AC count 77→78
- [info] Phase1-DriftChecked: F776 (Related)
- [fix] Phase1-DriftFix iter1: [DEP-001] F776 status sync | Updated F776 status from [WIP] to [DONE] in Dependencies and Related Features
- [fix] Phase1-DriftFix iter1: [INV-003] IEngineVariables GetCharacterNo drift | F776 added GetCharacterNo; updated AC#8 (SetCharacterNo only), AC#13 (12→13 methods), AC#50/53 matchers (GetNo/SetNo→GetCharacterNo/SetCharacterNo), C14 (12→13), Goal, 5Whys, Technical Design Interfaces (9→8 new methods), Key Decisions (NO variable scope)
- [fix] Phase2-Uncertain iter1: [FMT-002] Constraint Details | Added missing detail blocks for C4, C7, C9, C10, C11, C12, C13, C16, C18, C19, C20 (11 of 20 constraints)
- [resolved-applied] Phase2-Uncertain iter1: [AC-005] AC#45 stub no-op verification gap. → Applied: Option B (AC#82追加) selected in FL POST-LOOP. CustomTerminal stub loop completion test ACを追加。
- [resolved-applied] Phase2-Review iter1: [AC-005] AC#66 Branch 2 coverage gap. → Applied: Option A (AC#83追加) selected in FL POST-LOOP. Branch 2専用ユニットテスト存在検証ACを追加。
- [fix] Phase2-Review iter2: [INV-003] Philosophy Derivation row 3 | Revised derived requirement to remove 'F782 handoff documented in Mandatory Handoffs section' claim (AC#45 only verifies stub declarations, not documentation)
- [fix] Phase2-Review iter2: [AC-005] AC#81 | Added ClothesCustom player-only restriction unit test AC [C13]; updated Task 7, Goal Coverage, AC Coverage, AC count 79→80
- [resolved-applied] Phase2-Review iter3: [AC-002] ICommonFunctions two-step pattern. → Applied: Option A (AC#84追加) selected in FL POST-LOOP. 2ステップgenderパターン検証ACを追加。
- [fix] PostLoop-UserFix iter3: [AC-005] AC#82 | Added CustomTerminal stub loop completion test AC; updated Goal Coverage (documented transition points), Task 7, AC Coverage, AC count 80→81
- [fix] PostLoop-UserFix iter3: [AC-005] AC#83 | Added 処女 state Branch 2 unit test AC [C3]; updated Task 7, Goal Coverage, AC Coverage, AC count 81→82
- [fix] PostLoop-UserFix iter3: [AC-002] AC#84 | Added ICommonFunctions two-step gender pattern AC [C3]; updated Task 5, Goal Coverage, AC Coverage, AC count 82→83
- [fix] Phase2-Review iter4: [FMT-002] AC#81,82,83 type mismatch | Changed type from 'test' to 'code' (Grep-based test existence checks are code type per testing SSOT)
- [fix] Phase2-Review iter4: [AC-002] AC#84 matcher | Changed to matches with proximity constraint (GetTalent→HasVagina/HasPenis spans adjacent lines)
- [fix] Phase2-Review iter5: [AC-002] AC#84 matcher | Added .Value requirement to multiline pattern (prevents characterId-passing bug)
- [fix] Phase2-Review iter5: [AC-002] AC#39 matcher | Strengthened from name-only to matches with value spot-check (3, 72, 153 from C11 exclusion list)
- [fix] Phase2-Review iter6: [FMT-002] AC Coverage table | Added missing AC#84 row (two-step gender pattern)
- [fix] Phase2-Review iter6: [INV-003] Philosophy Derivation row 1 | Added AC#84 to 'continuous development pipeline' coverage (gender normalization correctness)
- [fix] Phase4-ACValidation iter7: [AC-002] AC#39,AC#84 matcher | Changed matches_multiline to matches (re.search on full file content already supports cross-line [\s\S] patterns; matches_multiline is not a valid matcher)
- [fix] Phase2-Review iter8: [AC-005] AC#85 | Added TalentCopier GetTarget usage AC [C1]; updated Task 6, Goal Coverage Item 2, AC Coverage, AC count 83→84
- [fix] Phase2-Review iter9: [TSK-001] Implementation Contract Phase 2 | Strengthened mini-TDD constraint: tests MUST cover all documented cases to compensate for missing independent RED phase
- [fix] Phase2-Review iter10: [TSK-001] Tasks 8-11 | Added [Verification] labels to clarify pure-verification nature of Tasks 8-11 (no implementation deliverables, ac-tester responsibilities)
- [fix] Phase3-Maintainability iter10: [TSK-001] Task 6 | Added CharacterFlagIndex.cs ReorderSentinel instruction (shared type, not private const)
- [fix] Phase3-Maintainability iter10: [TSK-001] Task 1 | Added ReorderSentinelValue to GameConstants.cs deliverable
- [fix] Phase3-Maintainability iter10: [TSK-001] Task 3 | Added RequestOneInput concrete testing note (via CharaCustum AC#14)
- [resolved-applied] Phase3-Maintainability iter10: [DES-001] TalentIndex.cs well-known indices. → Applied: Option A (Task 1に追加) selected in FL POST-LOOP. TalentIndex.csにGender, MaleVirginity, SexualOrientation, Lewdness定数追加。AC#86追加。
- [fix] PostLoop-UserFix iter10: [DES-001] AC#86 | Added TalentIndex well-known indices AC [C3, C20]; updated Task 1, AC Coverage, AC count 84→85
- [fix] Phase2-Review iter1: [FMT-002] AC count | Corrected 86→85 in Technical Design Approach and Success Criteria (actual: AC#1-86 minus deleted AC#24 = 85)
- [fix] Phase2-Review iter1: [FMT-002] Created section | Removed non-template '## Created: 2026-02-11' heading
- [fix] Phase2-Uncertain iter1: [PHI-001] Philosophy Derivation | Added AC#55 to 'clear phase boundaries' row (ICsvNameResolver GetTalentName completes Phase 20 interface coverage)
- [fix] Phase2-Review iter2: [AC-002] AC#20 file coverage | Expanded from 3 to 5 files (added GameConstants.cs, CharacterFlagIndex.cs); updated AC Details, AC Coverage, Task 9
- [fix] Phase2-Review iter3: [AC-002] AC#86 matcher | Strengthened from name-only count to value-inclusive (Gender=2, MaleVirginity=1, SexualOrientation=81, Lewdness=4); prevents wrong-value silent pass
- [fix] Phase2-Review iter4: [AC-002] AC#77,AC#78 matchers | Changed from magic literals (talentIndex==2/81) to require TalentIndex.Gender/SexualOrientation named constants; enforces AC#86 consumption
- [fix] Phase2-Review iter4: [DOC-001] AC#86 rationale | Corrected from 'AC#65 童貞, AC#66 淫乱' to 'AC#76 童貞, AC#48 淫乱' (accurate consumer references)
- [fix] Phase2-Review iter5: [DOC-001] Goal Coverage item 10 | Clarified 4 SYSTEM.ERB entry points (3 CharacterCustomizer + 1 TalentCopier) vs internal delegation targets (CustomTerminal, CopyCustom)
- [fix] Phase2-Review iter6: [AC-005] AC#87 | Added TalentNameResolver CASEELSE call-through AC (stub-replacement triple-AC: AC#55 interface + AC#9 injection + AC#87 call site); updated Task 4, Goal Coverage item 7, AC Coverage, AC count 85→86
- [fix] Phase2-Review iter7: [AC-002] AC#20 file coverage | Added TalentIndex.cs to zero-debt check (6 files total); updated AC Details, AC Coverage, Task 9
- [fix] Phase2-Review iter7: [AC-005] AC#88 | Added VirginCustom IsFemale two-step gender pattern AC (parallel to AC#84); updated Task 5, Goal Coverage item 1, AC Coverage, AC count 86→87
- [fix] Phase2-Review iter7: [DOC-001] Goal Coverage row 13 | Removed AC#61 (mutation test, not equivalence) from Equivalence tests row
- [resolved-applied] Phase3-Maintainability iter8: [DES-001] DisabledColor constant split: ERB uses 色設定_設定_無効化 (C10:27) and 色設定_テキスト_無効 (C10:31), both 0x999999 but different semantic purposes. Zero Debt Upfront says split into 2 named constants; pragmatic says merge since values are identical. Design decision applied: Split into DisabledSettingColor and DisabledTextColor; updated AC#80 matcher and description.
- [fix] Phase3-Maintainability iter8: [DOC-001] AC#31 rationale | Documented CLOTHES_CUSTOM implicit ARG=0 (always player character) from CHARA_CUSTUM.ERB:22 parameterless call
- [fix] Phase3-Maintainability iter8: [AC-002] AC#27 | Expanded to verify both InitialCharacterCount and ReorderSentinelValue in GameConstants.cs (count_equals=2); updated AC Details
- [fix] Phase3-Maintainability iter8: [AC-002] AC#80 | Removed bare '999999' alternative from matcher (false-positive risk); kept 0x999999 and DisabledColor
- [fix] Phase2-Review iter1: [FMT-001] ## Summary section | Removed non-template section (content already in Goal)
- [fix] Phase2-Review iter1: [FMT-002] ## Scope Reference section | Removed non-template section (content covered in Impact Analysis and Technical Constraints)
- [fix] Phase2-Review iter2: [DOC-001] Philosophy Derivation 'clear phase boundaries' | Distinguished zero in-scope debt (AC#20) from tracked cross-feature debt (F782 Successor); added F782 as Successor dependency
- [fix] Phase2-Review iter2: [AC-002] AC#60 matcher | Strengthened from bare `TalentNameResolver` to `CharacterCustomizer\([\s\S]{0,500}TalentNameResolver` (verifies constructor DI injection)
- [fix] Phase3-Maintainability iter3: [AC-002] AC#80 Details | Synced AC Details pattern with Definition Table (removed bare '999999' alternative)
- [fix] PostLoop-UserFix iter4: [DES-001] AC#80 DisabledColor split | Split into DisabledSettingColor + DisabledTextColor per Zero Debt Upfront; updated AC#80 matcher and description
- [fix] Phase3-Maintainability iter1: [SCP-004] Mandatory Handoffs | Added PRINTFORML workaround tracking to F782 (IConsoleOutput lacks PrintFormLine)
- [resolved-applied] Phase3-Maintainability iter1: [DES-002] CustomTerminal public access: AC#18 counts CustomTerminal as public (count_equals=4) but since CharaCustum and CustomTerminal are on the same class, CustomTerminal could be private/internal. Design decision: keep public (4 methods) or make internal (3 public methods)? → Applied: Option A (internal に変更) selected in FL POST-LOOP. AC#18 count_equals=4→3、CustomTerminal を internal に。InternalsVisibleTo でテスト可能。
- [fix] Phase2-Review iter2: [INV-003] Task 6 sentinel constant | Changed TalentCopierSentinel to ReorderSentinelValue (matches AC#27 and Task 1)
- [fix] Phase2-Review iter1: [FMT-002] AC Coverage rows 81-88 | Reformatted from 3-column to 2-column format (matching all other rows)
- [fix] Phase2-Uncertain iter1: [AC-002] AC#21 matcher | Strengthened from `191|0.*190` to `i\s*<\s*191|i\s*<=\s*190|Range\(0,\s*191\)` (requires loop structure context, prevents comment false positives)
- [fix] Phase2-Uncertain iter1: [AC-002] AC#15 matcher | Strengthened from `191.*talent|talent.*191|0.*190` to `Assert.*191|Copies.*191|count.*191|TalentCount.*191` (requires test assertion context)
- [fix] Phase2-Review iter1: [PHI-001] Philosophy Derivation row 1 | Revised 'All 3 ERB files migrated' to accurately reflect partial migration with no-op stubs tracked via F782; added AC#45,AC#82
- [fix] Phase2-Review iter2: [AC-002] AC#82 matcher | Broadened test naming patterns to accept Loop/Complete/Finish alternatives (prevents false negatives from valid test names)
- [fix] PostLoop-UserFix iter3: [DES-002] CustomTerminal internal | Changed CustomTerminal from public to internal; AC#18 count_equals=4→3; AC#18 description/matcher updated to 3 public SYSTEM.ERB entry points; AC Coverage row 18 updated
- [fix] Phase2-Review iter1: [FMT-002] AC#81-86 Details format | Renamed **Verification** to **Test** and added **Expected** field for AC#81-86 (AC#87-88 already standard format)
- [fix] Phase2-Review iter1: [FMT-002] AC count clarification | Changed '87 ACs' to '88 ACs (AC#1-AC#89, excluding deleted AC#24)' in Approach and Success Criteria
- [fix] Phase2-Review iter1: [AC-005] AC#89 | Added CharacterCustomizer→TalentNameResolver call-site AC (triple-AC pattern: AC#60 injection + AC#89 call site); updated Task 1, AC Coverage, AC count 87→88
- [resolved-applied] Phase2-Uncertain iter1: [PHI-001] Pipeline Continuity is meta-process principle not domain principle. The Philosophy→AC derivation is arguably circular (re-states Goal). Consider replacing with domain-specific design principles (interface completeness, behavior preservation, zero debt).
- [fix] Phase2-Review iter2: [FMT-002] Success Criteria AC count | Corrected 87→88 (AC#1-89 minus deleted AC#24 = 88)
- [fix] Phase2-Review iter2: [TSK-001] Task 5 AC#89 | Added AC#89 to Task 5 AC column (CharacterCustomizer→TalentNameResolver call-site)
- [fix] Phase2-Review iter3: [GOAL-001] Goal Coverage Item 12 | Moved AC#86 from 'Zero technical debt' to Goal Item 1 (AC#86 verifies TalentIndex constant correctness [C3, C20], not zero debt [C8])
- [fix] Phase2-Review iter4: [AC-005] AC#90 | Added CharaCustum→CustomTerminal delegation AC [C2]; updated Task 5, Goal Coverage Item 1, AC Coverage, AC count 88→89
- [fix] Phase2-Uncertain iter4: [AC-005] AC#15/AC#79 [Skip] bypass | Note: uncertain issue about [Skip/Ignore] annotated tests bypassing test existence ACs — deferred as TDD RED→GREEN mandate prevents this in practice. No AC change applied.
- [fix] Phase2-Review iter5: [FMT-002] Success Criteria AC count | Corrected 88→89 (AC#1-90 minus deleted AC#24 = 89); range corrected to AC#1-AC#90
- [fix] Phase2-Review iter1: [FMT-002] AC#39 Details format | Renamed **Verification** to **Test** and added **Expected** field for AC#39
- [fix] Phase2-Review iter2: [AC-002] AC#27 matcher | Strengthened ReorderSentinelValue matcher to enforce derivation from InitialCharacterCount (prevents hardcoded value passing AC)
- [fix] PostLoop-UserFix iter3: [PHI-001] Philosophy replacement | Changed Philosophy from "Pipeline Continuity" (meta-process) to "Behavior Preservation" (domain principle); rederived Philosophy Derivation table with 3 domain claims: behavioral equivalence, interface completeness, zero technical debt
- [fix] Phase2-Review iter4: [PHI-002] Philosophy Derivation refinement | Amended Claim 1 to scope display exclusion ("core logic, display delegation excluded"); Claim 2 to include "forward-looking operations" and document GetPlayer/RequestOneInput rationale; Claim 3 note clarifying RequestOneInput is scoped under Claim 2

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning (decomposed this feature)
- [Predecessor: F788](feature-788.md) - IConsoleOutput Extensions (DRAWLINE, BAR, PrintColumnLeft)
- [Predecessor: F790](feature-790.md) - IEngineVariables + ICsvNameResolver (needs extension for setters only)
- [Related: F774](feature-774.md) - Shop Core (SHOP.ERB + SHOP2.ERB) - same subsystem
- [Related: F775](feature-775.md) - Collection (SHOP_COLLECTION.ERB) - sibling Phase 20
- [Related: F776](feature-776.md) - Items (SHOP_ITEM.ERB + アイテム説明.ERB) - sibling Phase 20
- [Related: F789](feature-789.md) - IStringVariables + I3DArrayVariables
- [Related: F791](feature-791.md) - IGameState mode transitions
- [Related: F368](archive/feature-368.md) - Phase 3 partial migration (CharacterSetup.cs, TalentManager.cs)
- [Handoff: F782](feature-782.md) - Post-Phase Review Phase 20 (192 vs 191 talent discrepancy)
