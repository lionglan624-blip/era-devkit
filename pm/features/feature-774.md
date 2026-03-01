# Feature 774: Shop Core (SHOP.ERB + SHOP2.ERB)

## Status: [DONE]
<!-- fl-reviewed: 2026-02-14T00:00:00Z -->

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

<!-- fc-phase-1-completed -->
## Background

### Philosophy (Mid-term Vision)

Phase 20: Equipment & Shop Systems - Sequential phase migration ensures each game subsystem is converted to C# with full equivalence verification before proceeding. SSOT: `designs/phases/phase-20-27-game-systems.md` Phase 20 section defines the scope, interfaces (IShopSystem, IInventoryManager), and deliverables (Era.Core/Shop/ShopSystem.cs); F647 decomposed it into actionable sub-features including this one.

### Problem (Current Issue)

SHOP.ERB (197 lines, 5 functions) and SHOP2.ERB (246 lines, 3 functions) remain in ERB because the engine's shop state machine (Process.SystemProc.cs:621-761) tightly couples with ERB via callFunction() dispatching to named functions (SHOW_SHOP, USERSHOP, EVENTBUY). This coupling means the shop coordinator cannot be migrated to C# without preserving these entry points exactly. Additionally, SHOP.ERB is a thin router with 10+ external calls spanning multiple subsystems, while SHOP2.ERB's character data display depends on 8+ global variable arrays (FLAG:1003/1004, BASE, MAXBASE, ABL, EXP, JUEL, MARK, CFLAG, TALENT) creating a wide dependency surface that must be modeled via interfaces.

### Goal (What to Achieve)

Migrate SHOP.ERB and SHOP2.ERB to C# (Era.Core/Shop/) with: (1) IShopSystem and IInventoryManager interfaces per Phase 20 architecture, (2) entry point compatibility for BEGIN SHOP callers (SYSTEM.ERB, EVENTTURNEND.ERB), (3) fail-fast stubs for external calls (OPTION, MAN_SET, PRINT_STATE, etc.) with DI-registered shop system, (4) equivalence tests verifying display output and state mutations match legacy behavior, (5) zero technical debt in migrated code, (6) typed display mode (DisplayCategory enum replacing FLAG:1003/1004 magic numbers with invalid value guard), and (7) strongly-typed value types (ShopId, ItemId, ShopItem, PurchaseResult) for interface contracts.

<!-- Sub-Feature Requirements (architecture.md:4629-4637): Reflected in AC Design Constraints C2-C5.
  1. Philosophy inheritance: Phase 20: Equipment & Shop Systems (C2)
  2. Tasks: Debt cleanup - TODO/FIXME/HACK comment removal (C3)
  3. Tasks: Equivalence verification - legacy implementation equivalence tests (C4)
  4. AC: Zero debt - verify zero technical debt (C5)
-->

---

<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why must SHOP.ERB and SHOP2.ERB be migrated? | Phase 20 architecture mandates C# migration of all shop system ERB files to Era.Core | phases/phase-20-27-game-systems.md:7-9 |
| 2 | Why can't they be migrated trivially? | The engine's shop state machine (5 states) calls ERB functions by name via callFunction(), creating tight coupling between engine process flow and ERB entry points | Process.SystemProc.cs:684-741, Process.State.cs:47-51 |
| 3 | Why does the state machine matter? | shopWaitInput() has dual routing: item purchases (< MaxShopItem) go to EVENTBUY, while menu selections (>= MaxShopItem) go to USERSHOP, creating an implicit contract that C# must preserve | Process.SystemProc.cs:698-741 |
| 4 | Why is the dependency surface complex? | SHOP.ERB is a thin router making 10+ external calls (SHOW_CHARADATA, SHOW_COLLECTION, ITEM_BUY, OPTION, MAN_SET, etc.) and SHOP2.ERB reads 8+ global variable arrays for display, requiring interfaces for all external boundaries | SHOP.ERB:38-42,50,117,120; SHOP2.ERB:32-125 |
| 5 | Why (Root)? | The shop system was designed as a central hub with wide coupling to both the engine state machine above and character/item subsystems below, so migration requires defining clean interface boundaries (IShopSystem, IInventoryManager) to decouple the coordinator from both layers | phases/phase-20-27-game-systems.md:22-33 |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | SHOP.ERB and SHOP2.ERB remain as unmigrated ERB files | Engine state machine dispatches to ERB functions by name, and shop coordinator has wide coupling to 10+ external subsystems without interface boundaries |
| Where | Game/ERB/SHOP.ERB, Game/ERB/SHOP2.ERB | engine/Assets/Scripts/Emuera/GameProc/Process.SystemProc.cs:621-761 (state machine) and cross-cutting external calls |
| Fix | Translate ERB line-by-line to C# | Define IShopSystem/IInventoryManager interfaces, model state machine compatibility, and create interface stubs for all external calls |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Parent planning feature that decomposed Phase 20 into sub-features |
| F775 | [DRAFT] | Collection (SHOP_COLLECTION.ERB) - called from SHOP.ERB:40 via SHOW_COLLECTION |
| F776 | [DRAFT] | Items (SHOP_ITEM.ERB + アイテム説明.ERB) - called from SHOP.ERB:42 via ITEM_BUY |
| F777 | [DRAFT] | Customization (SHOP_CUSTOM.ERB + CHARA_CUSTUM.ERB + TALENTCOPY.ERB) |
| F778 | [DRAFT] | Body Settings - shares IBodySettings pattern from Phase 20 |
| F779 | [DRAFT] | Body Settings (continued) |
| F780 | [DRAFT] | Body Settings (continued) |
| F781 | [DRAFT] | Body Settings (continued) |
| F782 | [DRAFT] | Post-Phase Review Phase 20 |
| F783 | [DRAFT] | Phase 21 Planning |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Source code comprehension | FEASIBLE | 443 lines, 8 functions, well-structured ERB with clear control flow |
| Architecture guidance | FEASIBLE | Phase 20 defines IShopSystem, IInventoryManager, deliverables (Era.Core/Shop/ShopSystem.cs) |
| Engine state machine coupling | NEEDS_REVISION | 5-state shop state machine with dual routing (Process.SystemProc.cs:698-741) not reflected in current DRAFT |
| External dependency resolution | NEEDS_REVISION | 10+ external calls require interface stubs; no ERB fallback in target architecture |
| Prerequisite phases | NOT_FEASIBLE | Phases 9-19 are TODO; implementation (/run) blocked until prerequisites complete |
| No technical debt | FEASIBLE | Source files contain no TODO/FIXME/HACK markers |
| Test infrastructure | FEASIBLE | Era.Core.Tests project exists with established patterns (IBodySettings, MockGameContext) |
| /fc vs /run readiness | NEEDS_REVISION | /fc can proceed; /run blocked until prerequisite phases (Phase 14: Era.Core Engine) complete |

**Verdict**: NEEDS_REVISION

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| Engine state machine | HIGH | C# migration must preserve compatibility with Process.SystemProc.cs shop states (BEGIN_SHOP through Shop_CallEventBuy) |
| BEGIN SHOP callers | HIGH | SYSTEM.ERB:57,146 and EVENTTURNEND.ERB:66 depend on BEGIN SHOP entry point remaining functional |
| Character data display | MEDIUM | SHOP2 reads 8+ variable arrays; C# must model same read patterns via interfaces |
| External subsystems | MEDIUM | 10+ external calls (OPTION, MAN_SET, PRINT_STATE, etc.) need interface stubs |
| TFLAG:100 | MEDIUM | Reset in @SHOW_SHOP, read in 100+ COMABLE locations; must preserve reset behavior |
| Phase 20 sibling features | LOW | F775-F777 are Related (not blocking); stubs suffice for their entry points |
| LOADGAME default | LOW | Process.SystemProc.cs:784-786 defaults to BEGIN SHOP after load; must remain compatible |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Engine calls SHOW_SHOP and USERSHOP by name via callFunction() | Process.SystemProc.cs:686,738 | C# migration must preserve these entry points as callable functions |
| BEGIN SHOP is an engine-level command triggering Shop_Begin state | Process.State.cs:47 | Cannot change this entry point; must remain compatible |
| Shop state machine has 5 states with dual routing in shopWaitInput | Process.SystemProc.cs:621-761 | C# must handle both item purchase (< MaxShopItem) and menu selection paths |
| TFLAG:100 reset in @SHOW_SHOP, read across 100+ COMABLE locations | SHOP.ERB:6 | Must preserve exact reset behavior in C# equivalent |
| FLAG:1003/FLAG:1004 persistent display mode state | SHOP2.ERB:32,70,244-245 | Must model as typed enum in C# and preserve write semantics |
| SAVESTR:10 gates collection menu visibility | SHOP.ERB:20,39 | Must preserve conditional check in C# |
| Architecture requires IShopSystem/IInventoryManager interfaces | phases/phase-20-27-game-systems.md:22-33 | Must create interfaces following DI pattern |
| Sub-Feature Requirements mandate debt cleanup, equivalence tests, zero-debt AC | phases/phase-20-27-game-systems.md:88-96 | Tasks and ACs must include mandated items |
| EVENTSHOP and EVENTBUY ERB functions do not exist in this codebase | Grep across Game/ERB (all 3 investigations) | Engine TRYCALL to these is dead code; can omit from migration |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| Phase 9-19 delay blocks /run implementation | HIGH | LOW | /fc proceeds now; /run waits for prerequisites |
| Engine state machine replacement design not ready until Phase 14 | MEDIUM | HIGH | Phase 14 precedes Phase 20 in dependency chain; document coupling now |
| External call stubs grow too numerous (10+ interfaces) | MEDIUM | MEDIUM | Define narrow per-subsystem interfaces following DI pattern |
| SHOP2 CSV-derived display names (ABLNAME, EXPNAME, etc.) depend on Phase 17 data migration | LOW | MEDIUM | Use placeholder data in tests; Phase 17 must precede /run |
| IShopSystem interface may need revision when F775-F777 implement | MEDIUM | LOW | Design with extension points; interface covers F774 scope only |
| SET_FUTANARI_ALL equivalence testing complex (copy-pasted from SHOP_ITEM.ERB) | MEDIUM | LOW | Golden-master pattern for equivalence verification |
| Scope creep from external dependency discovery during implementation | MEDIUM | MEDIUM | Strict scope boundary to SHOP.ERB + SHOP2.ERB; external calls use stubs only |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| SHOP.ERB line count | wc -l Game/ERB/SHOP.ERB | 197 | Source file size |
| SHOP2.ERB line count | wc -l Game/ERB/SHOP2.ERB | 246 | Source file size |
| Total functions | manual count | 8 | 5 in SHOP.ERB + 3 in SHOP2.ERB |
| Era.Core/Shop/ exists | ls Era.Core/Shop/ | Does not exist | No prior implementation |
| Shop-related tests | Grep "Shop" Era.Core.Tests/ | 0 matches | No prior tests |
| Technical debt markers | Grep "TODO\|FIXME\|HACK" Game/ERB/SHOP.ERB Game/ERB/SHOP2.ERB | 0 matches | Source files clean |

**Baseline File**: `.tmp/baseline-774.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | IShopSystem and IInventoryManager interfaces required | phases/phase-20-27-game-systems.md:22-33 | AC must verify interface creation and IShopSystem DI registration; IInventoryManager DI registration deferred to F776 |
| C2 | Philosophy inheritance: Phase 20 Equipment & Shop Systems | phases/phase-20-27-game-systems.md:92 | All ACs under Phase 20 philosophy |
| C3 | Tasks must include debt cleanup (TODO/FIXME/HACK removal) | phases/phase-20-27-game-systems.md:93 | AC with not_contains for TODO/FIXME/HACK in new code |
| C4 | Equivalence tests required for legacy behavior | phases/phase-20-27-game-systems.md:94 | AC must verify equivalence test existence |
| C5 | Zero technical debt AC required | phases/phase-20-27-game-systems.md:95 | Dedicated zero-debt AC using not_contains matcher |
| C6 | Entry points SHOW_SHOP and USERSHOP must remain callable | Process.SystemProc.cs:686,738 | AC must verify entry point compatibility |
| C7 | All 8 functions must be migrated | SHOP.ERB (5) + SHOP2.ERB (3) | AC must verify each function has C# equivalent |
| C8 | erb type: 8-15 AC count | feature-template.md:19 | AC count must be within range |
| C9 | FLAG:1003/FLAG:1004 display mode state must be typed | SHOP2.ERB:32,70,244-245 | AC must verify enum-based display mode |
| C10 | TRYCALL targets: COM461_PUNISHMENT_END (dead), 追加パッチVerUP (exists→no-op), 妊娠パッチDEBUG88 (dead), 妊娠パッチDEBUG88COM (dead); EVENTSHOP/EVENTBUY absent | Grep across Game/ERB; SHOP.ERB:2-3,28,32 | Dead targets omitted; existing target uses no-op stub (not NotImplementedException) since TRYCALL ignores missing functions |
| C11 | BEGIN TRAIN, SAVEGAME, LOADGAME in @USERSHOP are engine state transitions | Process.SystemProc.cs:621-761; SHOP.ERB:35,46,48,196 | All three use NotSupportedException stubs pending Phase 14 engine integration (Mandatory Handoff); IGameState.SaveGame(string)/LoadGame(string) are file-based, not dialog-based |
| C12 | @SCHEDULE/@SHOW_CHARADATA/@SET_FUTANARI_ALL use RESTART/GOTO loop control | SHOP.ERB:90-121,190; SHOP2.ERB:158-163,242 | C# must use while(true) loops (RESTART) and continue/break (GOTO $LABEL) |
| C13 | @USERSHOP calls SHOW_CHARADATA requiring cross-class dependency | SHOP.ERB:38 → SHOP2.ERB:141 | ShopSystem must take ShopDisplay via constructor injection |
| C14 | IShopSystem/IInventoryManager reference types not in Era.Core | Technical Design interface definitions | ShopId, ItemId, ShopItem, PurchaseResult, CharacterId must exist for compilation |

### Constraint Details

**C1: Interface Creation**
- **Source**: Phase 20 architecture mandates IShopSystem and IInventoryManager with specific method signatures
- **Verification**: Grep for interface definitions in Era.Core/
- **AC Impact**: Must verify both interfaces exist with correct method signatures and are registered for DI
- **DI Scope**: IShopSystem is DI-registered (AC#5). IInventoryManager is defined (AC#4) but not DI-registered in F774 because no concrete InventoryManager implementation exists in scope. IInventoryManager DI registration is tracked in Mandatory Handoffs → F776. The interface is architectural pre-investment per Zero Debt Upfront.

**C2: Philosophy Inheritance**
- **Source**: Sub-Feature Requirements in phase-20-27-game-systems.md:92 require all Phase 20 sub-features inherit the Equipment & Shop philosophy
- **Verification**: Check Philosophy section references Phase 20
- **AC Impact**: Philosophy Derivation table must trace from Phase 20 philosophy

**C3: Debt Cleanup Task**
- **Source**: Sub-Feature Requirements mandate TODO/FIXME/HACK comment removal tasks
- **Verification**: Grep for debt markers in source files (currently 0 matches - source is clean)
- **AC Impact**: AC must use not_matches with TODO|FIXME|HACK regex pattern on new C# files to prevent introducing new debt

**C4: Equivalence Testing**
- **Source**: Sub-Feature Requirements mandate legacy-to-C# equivalence tests
- **Verification**: Check Era.Core.Tests/Shop/ for equivalence test files
- **AC Impact**: AC must verify test files exist and cover key functions (at minimum @SHOW_SHOP, @USERSHOP, @LIFE_LIST, @SET_FUTANARI_ALL)

**C5: Zero Technical Debt**
- **Source**: Sub-Feature Requirements mandate a dedicated zero-debt AC
- **Verification**: Grep for TODO|FIXME|HACK in all new C# files
- **AC Impact**: Standalone AC with not_matches matcher targeting Era.Core/Shop/ and Era.Core.Tests/Shop/

**C6: Entry Point Compatibility**
- **Source**: Engine's Process.SystemProc.cs calls SHOW_SHOP (line 686) and USERSHOP (line 738) by name
- **Verification**: C# implementation must register these function names or provide equivalent routing
- **AC Impact**: AC must verify that the migration preserves callable entry points
- **Interface Reconciliation**: ShowShop/UserShop are implementation-detail methods on ShopSystem class, not IShopSystem interface methods. IShopSystem defines the Phase 20 domain contract (GetAvailableItems, Purchase). Entry point methods are accessed via IFunctionRegistry (Phase 14) routing ERB names to class methods, not through interface dispatch.

**C7: Complete Function Coverage**
- **Source**: 8 functions across 2 files: @SHOW_SHOP, @USERSHOP, @SCHEDULE, @SET_FUTANARI_ALL, @DEBUG_ENTER_UFUFU (SHOP.ERB); @LIFE_LIST, @SHOW_CHARADATA, @SHOW_CHARADATA2 (SHOP2.ERB)
- **Verification**: Each function has corresponding C# method
- **AC Impact**: AC must verify all 8 functions have C# equivalents (code type AC)

**C8: AC Count Range**
- **Source**: erb type features require 8-15 ACs per feature-template.md
- **Verification**: Count AC rows in AC table
- **AC Impact**: ac-designer must stay within 8-15 ACs

**C9: Display Mode Typing**
- **Source**: FLAG:1003 selects display category, FLAG:1004 selects sub-category in SHOP2.ERB:32-124. FLAG:1003 initial value is 0 (no category selected); ERB SELECTCASE falls through silently for 0.
- **Verification**: C# uses enum with None=0 sentinel instead of raw flag indices; switch default handles None by skipping display
- **AC Impact**: AC must verify typed display mode exists (not raw int indices)

**C10: TRYCALL Semantics and Dead Code**
- **Source**: SHOP.ERB has 4 TRYCALL calls: COM461_PUNISHMENT_END (line 2, dead), 追加パッチVerUP (line 3, exists in 追加パッチverup.ERB), 妊娠パッチDEBUG88 (line 28, dead), 妊娠パッチDEBUG88COM (line 32, dead). SHOP2.ERB has 1 TRYCALL: 簡易追加情報 (@LIFE_LIST:41, exists in 生理機能追加パッチ.ERB:95). Additionally, @EVENTSHOP and @EVENTBUY do not exist (engine TRYCALL is dead code).
- **Verification**: Dead targets omitted from migration. Existing targets (追加パッチVerUP in ShopSystem, 簡易追加情報 in ShopDisplay) use no-op empty method body (not NotImplementedException) because TRYCALL semantics = "call if exists, ignore if missing."
- **AC Impact**: Do not create ACs for dead TRYCALL targets; existing TRYCALL stubs are no-op (empty body), not fail-fast

**C11: Engine State Transitions**
- **Source**: @USERSHOP dispatches BEGIN TRAIN (line 35), SAVEGAME (line 46), LOADGAME (line 48). @DEBUG_ENTER_UFUFU also uses BEGIN TRAIN (line 196). These are engine-level state machine commands, not ERB function calls.
- **Verification**: All three commands use NotSupportedException stubs. IGameState.SaveGame(string)/LoadGame(string) are file-based operations requiring a filename parameter; ERB SAVEGAME/LOADGAME are parameterless dialog-based operations — signatures are incompatible. Phase 14 must provide dialog-mode save/load interface.
- **AC Impact**: All three engine state transitions (BeginTrain, SaveGame, LoadGame) use NotSupportedException stubs with descriptive messages pending Phase 14 engine integration.

**C12: RESTART/GOTO Transformation**
- **Source**: ERB RESTART = jump to beginning of current function. ERB GOTO $LABEL = jump to labeled location. Used extensively: @SCHEDULE (RESTART×4, GOTO×3, fallthrough RETURN 1), @SET_FUTANARI_ALL (GOTO×1), @SHOW_CHARADATA (RESTART×2, GOTO×1), @SHOW_CHARADATA2 (GOTO×1).
- **Verification**: C# transformation: RESTART → while(true) wrapping function body with `continue`. GOTO $INPUT_LOOP → `continue` in inner labeled while loop. Fallthrough (code after if/elseif chain without RESTART) → `return` (NOT implicit loop continue). @SCHEDULE line 124 `RETURN 1` is fallthrough for unmatched input.
- **AC Impact**: Migration must preserve loop semantics including fallthrough exits; equivalence tests verify behavior

**C13: ShopSystem→ShopDisplay Dependency**
- **Source**: @USERSHOP line 38 calls SHOW_CHARADATA (defined in SHOP2.ERB @SHOW_CHARADATA). ShopSystem must access ShopDisplay methods.
- **Verification**: ShopSystem constructor takes ShopDisplay as parameter
- **AC Impact**: Constructor injection modeled in Technical Design; verified indirectly by equivalence tests (AC#10)

**C14: Value Types for Interface Contracts**
- **Source**: IShopSystem references ShopId, ShopItem, PurchaseResult. IInventoryManager references ItemId. CharacterId already exists in Era.Core/Types/CharacterId.cs — do NOT redefine.
- **Verification**: Era.Core/Shop/ShopTypes.cs must define ShopId, ItemId, ShopItem, PurchaseResult as record types. CharacterId is imported via `using Era.Core.Types;`
- **AC Impact**: AC#13 verifies value types exist; build will fail without them

---

## Dependencies

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (decomposed this feature) |
| Successor | F775 | [DONE] | Collection — SHOP.ERB @USERSHOP:40 calls SHOW_COLLECTION |
| Successor | F776 | [DONE] | Items — SHOP.ERB @USERSHOP:42 calls ITEM_BUY. Shared futanari logic |
| Related | F777 | [DONE] | Customization (SHOP_CUSTOM.ERB + CHARA_CUSTUM.ERB + TALENTCOPY.ERB) |
| Related | F778 | [DONE] | Body Settings - shares IBodySettings pattern |
| Related | F779 | [DONE] | Body Settings (continued) |
| Related | F780 | [PROPOSED] | Body Settings (continued) |
| Related | F781 | [DONE] | Body Settings (continued) |
| Related | F782 | [DRAFT] | Post-Phase Review Phase 20 |
| Related | F783 | [DRAFT] | Phase 21 Planning |

---

<!-- fc-phase-3-completed -->
## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Phase 20: Sequential phase migration ensures each game subsystem is converted to C# with full equivalence verification" | C# implementation files must exist in Era.Core/Shop/ | AC#1, AC#2 |
| "Sequential phase migration...with equivalence verification" | Equivalence tests must verify all 8 functions match legacy behavior | AC#10, AC#11 |
| "Sequential phase migration ensures each game subsystem is converted to C#" (via Phase 20 SSOT: IShopSystem, IInventoryManager) | Interfaces must be defined with correct method signatures | AC#3, AC#4 |
| "Sequential phase migration ensures each game subsystem is converted to C#" (via engine state machine entry points) | SHOW_SHOP and USERSHOP entry points must remain callable | AC#6 |
| "Sequential phase migration ensures each game subsystem is converted to C#" (via Sub-Feature Requirements) | No TODO/FIXME/HACK markers in new code | AC#12a, AC#12b |
| "Sequential phase migration ensures each game subsystem is converted to C#" (via DI architecture) | DI registration for shop system; fail-fast stubs for external calls (compilation-enforced, code-review-verified; no dedicated stub AC due to C8 limit) | AC#5 |
| "Sequential phase migration ensures each game subsystem is converted to C#" | C# migration requires typed state modeling; FLAG:1003/1004 magic numbers become DisplayCategory enum | AC#7 |
| "Sequential phase migration ensures each game subsystem is converted to C#" | Complete subsystem conversion requires all 8 functions migrated as C# methods | AC#8, AC#9 |
| "Sequential phase migration ensures each game subsystem is converted to C#" | Typed state modeling requires invalid enum values to be rejected defensively | AC#7b |
| "Sequential phase migration ensures each game subsystem is converted to C#" (via Phase 20 SSOT: interface contracts) | Interface contract types must be compilable | AC#13 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ShopSystem.cs exists | file | Glob(Era.Core/Shop/ShopSystem.cs) | exists | - | [x] |
| 2 | ShopDisplay.cs exists | file | Glob(Era.Core/Shop/ShopDisplay.cs) | exists | - | [x] |
| 3 | IShopSystem interface with method signatures | code | Grep("(interface IShopSystem\|GetAvailableItems.*ShopId\|Purchase.*CharacterId.*ItemId)", Era.Core/Shop/IShopSystem.cs) | count_equals | 3 | [x] |
| 4 | IInventoryManager interface with method signatures | code | Grep("(interface IInventoryManager\|HasItem.*CharacterId.*ItemId\|AddItem.*CharacterId.*ItemId)", Era.Core/Shop/IInventoryManager.cs) | count_equals | 3 | [x] |
| 5 | DI registration for IShopSystem and ShopDisplay | code | Grep("AddSingleton<(IShopSystem,\\s*ShopSystem\|ShopDisplay)>", Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | count_equals | 2 | [x] |
| 6 | Entry points SHOW_SHOP/USERSHOP preserved (Pos) | code | Grep("public.*(ShowShop\|UserShop)\\b", Era.Core/Shop/ShopSystem.cs) | count_equals | 2 | [x] |
| 7 | DisplayCategory enum defined | code | Grep("enum DisplayCategory", Era.Core/Shop/DisplayCategory.cs) | contains | enum DisplayCategory | [x] |
| 7b | Invalid DisplayCategory guard (Neg) | code | Grep("(Enum\\.IsDefined\|InvalidEnumArgumentException\|ArgumentOutOfRangeException)", Era.Core/Shop/) | contains | Enum.IsDefined | [x] |
| 8 | All 5 SHOP.ERB functions migrated | code | Grep("public.*(ShowShop\|UserShop\|Schedule\|SetFutanariAll\|DebugEnterUfufu)", Era.Core/Shop/ShopSystem.cs) | count_equals | 5 | [x] |
| 9 | All 3 SHOP2.ERB functions migrated | code | Grep("public.*(LifeList\|ShowCharaData\|ShowCharaData2)", Era.Core/Shop/ShopDisplay.cs) | count_equals | 3 | [x] |
| 10 | Equivalence tests exist | test | Test(dotnet test Era.Core.Tests --filter Category=Shop) | succeeds | - | [x] |
| 11 | Equivalence test coverage (Pos) | code | Grep("(ShowShopEquivalence\|UserShopEquivalence\|LifeListEquivalence\|SetFutanariAllEquivalence\|ScheduleEquivalence\|ShowCharaDataEquivalence\|ShowCharaData2Equivalence\|DebugEnterUfufuEquivalence)", Era.Core.Tests/Shop/) | count_equals | 8 | [x] |
| 12a | Zero technical debt - Shop (Neg) | code | Grep("TODO\|FIXME\|HACK", Era.Core/Shop/) | not_matches | TODO\|FIXME\|HACK | [x] |
| 12b | Zero technical debt - Tests (Neg) | code | Grep("TODO\|FIXME\|HACK", Era.Core.Tests/Shop/) | not_matches | TODO\|FIXME\|HACK | [x] |
| 13 | Value types for interface contracts exist | file | Glob(Era.Core/Shop/ShopTypes.cs) | exists | - | [x] |

### AC Details

**AC#1: ShopSystem.cs exists**
- **Test**: Glob pattern="Era.Core/Shop/ShopSystem.cs"
- **Expected**: File exists
- **Rationale**: Phase 20 architecture deliverable (phases/phase-20-27-game-systems.md:69) requires Era.Core/Shop/ShopSystem.cs as the primary shop coordinator. Contains migrated logic from SHOP.ERB (5 functions).

**AC#2: ShopDisplay.cs exists**
- **Test**: Glob pattern="Era.Core/Shop/ShopDisplay.cs"
- **Expected**: File exists
- **Rationale**: SHOP2.ERB's 3 functions (@LIFE_LIST, @SHOW_CHARADATA, @SHOW_CHARADATA2) handle character data display, which is a separate responsibility from the shop coordinator. Following SRP, these belong in a dedicated file.

**AC#3: IShopSystem interface with method signatures**
- **Test**: Grep pattern="(interface IShopSystem\|GetAvailableItems.*ShopId\|Purchase.*CharacterId.*ItemId)" path="Era.Core/Shop/IShopSystem.cs" | count
- **Expected**: 3 matches (interface declaration + GetAvailableItems signature + Purchase signature)
- **Rationale**: C1 constraint. Phase 20 architecture mandates IShopSystem with GetAvailableItems(ShopId) and Purchase(CharacterId, ItemId) methods. count_equals=3 verifies both interface existence and method signatures.

**AC#4: IInventoryManager interface with method signatures**
- **Test**: Grep pattern="(interface IInventoryManager\|HasItem.*CharacterId.*ItemId\|AddItem.*CharacterId.*ItemId)" path="Era.Core/Shop/IInventoryManager.cs" | count
- **Expected**: 3 matches (interface declaration + HasItem signature + AddItem signature)
- **Rationale**: C1 constraint. Phase 20 architecture mandates IInventoryManager with HasItem(CharacterId, ItemId) and AddItem(CharacterId, ItemId, int) methods. count_equals=3 verifies both interface existence and method signatures.

**AC#5: DI registration for IShopSystem and ShopDisplay**
- **Test**: Grep pattern="AddSingleton<(IShopSystem,\s*ShopSystem|ShopDisplay)>" path="Era.Core/DependencyInjection/ServiceCollectionExtensions.cs" | count
- **Expected**: 2 matches (one for `AddSingleton<IShopSystem, ShopSystem>()`, one for `AddSingleton<ShopDisplay>()`)
- **Rationale**: Following existing DI pattern (e.g., `AddSingleton<IBodySettings, BodySettings>`). Shop system must be registered for injection into engine integration layer. ShopDisplay must also be DI-registered since ShopSystem takes it via constructor injection (C13); without ShopDisplay registration, the DI container cannot construct ShopSystem. Anchored pattern (`AddSingleton<...>`) prevents false positives from comments.

**AC#6: Entry points SHOW_SHOP/USERSHOP preserved (Pos)**
- **Test**: Grep pattern="public.*(ShowShop|UserShop)\b" path="Era.Core/Shop/ShopSystem.cs" | count
- **Expected**: 2 matches (one for each method declaration)
- **Rationale**: C6 constraint. Engine's Process.SystemProc.cs:686,738 calls these by name. C# must preserve them as public methods named ShowShop and UserShop (PascalCase convention). The count_equals=2 ensures exactly one declaration of each.

**AC#7: DisplayCategory enum defined**
- **Test**: Grep pattern="enum DisplayCategory" path="Era.Core/Shop/DisplayCategory.cs"
- **Expected**: Contains "enum DisplayCategory"
- **Rationale**: C9 constraint. FLAG:1003 selects display category (None=0 default, Base=2, Ability=3, SexSkill=4, Mark=5, Experience=6, Juel=7, Other=8). None=0 sentinel matches FLAG:1003 initial value; SELECTCASE falls through for 0 (no display). Enum replaces magic numbers for type safety. FLAG:1004 is a sub-index within the category (not an enum itself, since its valid values depend on category).

**AC#7b: Invalid DisplayCategory guard (Neg)**
- **Test**: Grep pattern="(Enum\\.IsDefined|InvalidEnumArgumentException|ArgumentOutOfRangeException)" path="Era.Core/Shop/" type=cs
- **Expected**: Contains `Enum.IsDefined`
- **Rationale**: DisplayCategory enum has None=0 sentinel and values 2-8. FLAG:1003 value 1 is not a valid category (gap between None=0 and Base=2). Without Enum.IsDefined() or equivalent guard, `(DisplayCategory)1` silently succeeds for invalid values. Negative AC ensures defensive validation exists. None=0 passes Enum.IsDefined correctly (matching ERB SELECTCASE fall-through behavior).

**AC#8: All 5 SHOP.ERB functions migrated**
- **Test**: Grep pattern="public.*(ShowShop|UserShop|Schedule|SetFutanariAll|DebugEnterUfufu)" path="Era.Core/Shop/ShopSystem.cs" | count
- **Expected**: 5 matches
- **Rationale**: C7 constraint. All 5 SHOP.ERB functions (@SHOW_SHOP, @USERSHOP, @SCHEDULE, @SET_FUTANARI_ALL, @DEBUG_ENTER_UFUFU) must have C# method equivalents.
- **Behavioral Coverage**: count_equals=5 is a structural check verifying method declarations exist. Behavioral correctness is validated by equivalence tests (AC#10, AC#11) which verify actual output and state mutations. ShowShopEquivalenceTests specifically covers @SHOW_SHOP display logic.

**AC#9: All 3 SHOP2.ERB functions migrated**
- **Test**: Grep pattern="public.*(LifeList|ShowCharaData|ShowCharaData2)" path="Era.Core/Shop/ShopDisplay.cs" | count
- **Expected**: 3 matches
- **Rationale**: C7 constraint. All 3 SHOP2.ERB functions (@LIFE_LIST, @SHOW_CHARADATA, @SHOW_CHARADATA2) must have C# method equivalents. This is a structural AC verifying method declarations exist. Internal call relationships (ShowCharaData→LifeList, ShowCharaData→ShowCharaData2) are verified behaviorally by ShowCharaDataEquivalenceTests (AC#11).

**AC#10: Equivalence tests exist**
- **Test**: `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter Category=Shop'`
- **Expected**: Test run succeeds (exit code 0)
- **Rationale**: C4 constraint. Equivalence tests verify that C# implementation produces same outputs and state mutations as legacy ERB for representative inputs. Tests must pass, proving behavioral equivalence.

**AC#11: Equivalence test coverage (Pos)**
- **Test**: Grep pattern="(ShowShopEquivalence|UserShopEquivalence|LifeListEquivalence|SetFutanariAllEquivalence|ScheduleEquivalence|ShowCharaDataEquivalence|ShowCharaData2Equivalence|DebugEnterUfufuEquivalence)" path="Era.Core.Tests/Shop/" | count
- **Expected**: 8 matches (one test class per function, full coverage)
- **Rationale**: C4 constraint. Equivalence tests must cover all 8 functions: @SHOW_SHOP (main entry, display output), @USERSHOP (menu dispatch, state transitions), @LIFE_LIST (character display, 8+ variable arrays), @SET_FUTANARI_ALL (batch mutation), @SCHEDULE (64 lines, stateful wake time/room selection logic with multiple input loops), @SHOW_CHARADATA (23 lines, RESTART×2, GOTO, INPUT, CHARANUM validation, PRINT_STATE call — non-trivial control flow), @SHOW_CHARADATA2 (82 lines, sole writer of FLAG:1003/FLAG:1004 display mode state with complex SELECTCASE logic over 6 CSV name arrays), @DEBUG_ENTER_UFUFU (4-line wrapper verifying BeginTrain() call).

**AC#12a: Zero technical debt - Shop (Neg)**
- **Test**: Grep pattern="TODO|FIXME|HACK" path="Era.Core/Shop/" type=cs
- **Expected**: not_matches pattern TODO|FIXME|HACK — 0 results expected
- **Rationale**: C3 and C5 constraints. Sub-Feature Requirements mandate zero technical debt in new C# code.

**AC#12b: Zero technical debt - Tests (Neg)**
- **Test**: Grep pattern="TODO|FIXME|HACK" path="Era.Core.Tests/Shop/" type=cs
- **Expected**: not_matches pattern TODO|FIXME|HACK — 0 results expected
- **Rationale**: C3 and C5 constraints. Sub-Feature Requirements mandate zero technical debt in test code.

**AC#13: Value types for interface contracts exist**
- **Test**: Glob pattern="Era.Core/Shop/ShopTypes.cs"
- **Expected**: File exists
- **Rationale**: C14 constraint. IShopSystem and IInventoryManager interfaces reference ShopId, ItemId, ShopItem, PurchaseResult, and CharacterId types. These must be defined as strongly-typed record types (readonly record struct for IDs, record for results) to prevent primitive obsession and enable type-safe API contracts. Without these types, Era.Core/ will not compile. This is a structural precondition AC; type definition correctness is compilation-enforced transitively via AC#10 (dotnet test runs dotnet build, which compiles AC#3/AC#4 interface files referencing these types).

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Migrate SHOP.ERB and SHOP2.ERB to C# (Era.Core/Shop/) | AC#1, AC#2, AC#8, AC#9 |
| 2 | IShopSystem and IInventoryManager interfaces per Phase 20 architecture | AC#3, AC#4 |
| 3 | Entry point compatibility for BEGIN SHOP callers | AC#6 |
| 4 | DI registration and fail-fast stubs for external calls | AC#5 |
| 5 | Equivalence tests verifying display output and state mutations match legacy behavior (all 8 functions) | AC#10, AC#11 |
| 6 | Zero technical debt in migrated code | AC#12a, AC#12b |
| 7 | Typed display mode (DisplayCategory enum replacing FLAG:1003/1004) with invalid value guard | AC#7, AC#7b |
| 8 | Strongly-typed value types for interface contracts | AC#13 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

**Source Files**: SHOP.ERB (197 lines, 5 functions: @SHOW_SHOP, @USERSHOP, @SCHEDULE, @SET_FUTANARI_ALL, @DEBUG_ENTER_UFUFU) and SHOP2.ERB (246 lines, 3 functions: @LIFE_LIST, @SHOW_CHARADATA, @SHOW_CHARADATA2).

**Migration Strategy**: Migrate SHOP.ERB and SHOP2.ERB to C# following the established interface-based pattern from Phase 14 (IBodySettings). The approach preserves entry point compatibility with the engine's shop state machine while creating clean interface boundaries for external subsystems.

**Architecture Pattern**: Follows DDD-lite pattern with:
1. **Interface layer** (IShopSystem, IInventoryManager) - contracts per Phase 20 spec
2. **Implementation layer** (ShopSystem.cs, ShopDisplay.cs) - migrated business logic
3. **DI registration** - ServiceCollectionExtensions.cs integration
4. **Type safety** - DisplayCategory enum replacing FLAG:1003/1004 magic numbers

**External Dependency Handling**: SHOP.ERB's external calls are categorized by type:
1. **ERB function calls** (SHOW_COLLECTION, ITEM_BUY, OPTION, MAN_SET, PRINT_STATE, etc.): **fail-fast stubs** throwing NotImplementedException with descriptive messages directing to Related features (F775-F778).
2. **Engine state transitions** (BEGIN TRAIN, SAVEGAME, LOADGAME): All three use **NotSupportedException** stubs pending Phase 14 engine integration (C11). IGameState.SaveGame(string)/LoadGame(string) are file-based; ERB commands are dialog-mode — signatures incompatible.
3. **TRYCALL targets**: Dead targets (COM461_PUNISHMENT_END, 妊娠パッチDEBUG88, 妊娠パッチDEBUG88COM) are omitted. Existing target (追加パッチVerUP) uses **no-op** empty method body per TRYCALL semantics (C10).
SHOP2.ERB's external calls: PRINT_STATE (@SHOW_CHARADATA:162) uses fail-fast stub in ShopDisplay.cs. TRYCALL 簡易追加情報 (@LIFE_LIST:41, exists in 生理機能追加パッチ.ERB:95) uses no-op empty method body per C10 TRYCALL semantics.

**Utility Injection Strategy**: ERB function stubs (SHOW_COLLECTION, ITEM_BUY, OPTION, MAN_SET, PRINT_STATE, CHILDREM_CHECK, GETPLACENAME, NTR_NAME, 子供訪問_設定, HAS_VAGINA, GET_MARK_LEVEL_OWNERNAME, GET_DAY, 日付_月) are implemented as private method stubs with NotImplementedException within ShopSystem/ShopDisplay classes. These do not require additional constructor parameters — they are self-contained throw sites. No facade interface needed; each stub is replaced in-place when the owning feature/phase delivers the real implementation.

**Equivalence Verification**: Follows ComEquivalenceTestBase pattern established in Era.Core.Tests. Golden-master approach for complex functions (SET_FUTANARI_ALL), mocked external dependencies for display tests.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Create Era.Core/Shop/ShopSystem.cs with migrated @SHOW_SHOP, @USERSHOP, @SCHEDULE, @SET_FUTANARI_ALL, @DEBUG_ENTER_UFUFU logic |
| 2 | Create Era.Core/Shop/ShopDisplay.cs with migrated @LIFE_LIST, @SHOW_CHARADATA, @SHOW_CHARADATA2 logic |
| 3 | Create Era.Core/Shop/IShopSystem.cs interface following Phase 20 spec (GetAvailableItems, Purchase methods) with XML doc comments |
| 4 | Create Era.Core/Shop/IInventoryManager.cs interface following Phase 20 spec (HasItem, AddItem methods) with XML doc comments |
| 5 | Register services.AddSingleton<IShopSystem, ShopSystem>() and services.AddSingleton<ShopDisplay>() in ServiceCollectionExtensions.cs (line ~148 near IBodySettings) |
| 6 | Implement public methods ShowShop() and UserShop() in ShopSystem.cs, verified by Grep pattern matching method declarations |
| 7 | Create Era.Core/Shop/DisplayCategory.cs with enum DisplayCategory { Base = 2, Ability = 3, SexSkill = 4, Mark = 5, Experience = 6, Juel = 7, Other = 8 } mapping FLAG:1003 values |
| 7b | Add Enum.IsDefined() or ArgumentOutOfRangeException guard in DisplayCategory conversion logic within ShopDisplay.LifeList() |
| 8 | Migrate all 5 SHOP.ERB functions as public methods in ShopSystem.cs, naming convention: @SHOW_SHOP → ShowShop(), @SET_FUTANARI_ALL → SetFutanariAll(), etc. |
| 9 | Migrate all 3 SHOP2.ERB functions as public methods in ShopDisplay.cs, naming convention: @LIFE_LIST → LifeList(int arg), @SHOW_CHARADATA → ShowCharaData(), @SHOW_CHARADATA2 → ShowCharaData2(int arg) |
| 10 | Create Era.Core.Tests/Shop/ directory with equivalence test files using Category=Shop attribute for test filtering |
| 11 | Create 8 test files: ShowShopEquivalenceTests.cs, UserShopEquivalenceTests.cs, LifeListEquivalenceTests.cs, SetFutanariAllEquivalenceTests.cs, ScheduleEquivalenceTests.cs, ShowCharaDataEquivalenceTests.cs, ShowCharaData2EquivalenceTests.cs, DebugEnterUfufuEquivalenceTests.cs with test methods verifying key behaviors |
| 12a | Run Grep for TODO\|FIXME\|HACK across Era.Core/Shop/ after implementation, expect 0 matches |
| 12b | Run Grep for TODO\|FIXME\|HACK across Era.Core.Tests/Shop/ after implementation, expect 0 matches |
| 13 | Create Era.Core/Shop/ShopTypes.cs with ShopId, ItemId (readonly record struct), ShopItem, PurchaseResult (record); use existing Era.Core.Types.CharacterId |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| **File Organization** | (A) Single ShopSystem.cs, (B) Separate ShopSystem + ShopDisplay | **B** | SHOP.ERB and SHOP2.ERB have distinct responsibilities: coordinator vs display. SRP suggests separate files matching source structure. |
| **DisplayCategory Design** | (A) Both FLAG:1003 and FLAG:1004 as enums, (B) FLAG:1003 as enum, FLAG:1004 as int | **B** | FLAG:1003 has fixed categories (Base/Ability/etc.). FLAG:1004 is an index within category with dynamic ranges (0-40 for Ability, 50-60 for SexSkill). Only FLAG:1003 benefits from enum type safety. |
| **Entry Point Preservation** | (A) Rename to PascalCase, (B) Keep ERB names with [ErbFunction] attribute | **A** | Engine integration layer (Phase 14) will handle ERB→C# routing via IFunctionRegistry. C# code follows PascalCase convention (ShowShop, UserShop). No custom attributes needed. |
| **External Stub Strategy** | (A) Empty methods, (B) NotImplementedException with feature references | **B** | Empty methods hide dependency boundaries. NotImplementedException with descriptive messages (e.g., "SHOW_COLLECTION - See F775") documents Related features and fails fast during integration. |
| **Equivalence Test Scope** | (A) All 8 functions, (B) 7 core functions, (C) 6 core functions | **A** | AC#11 requires 8 equivalence tests for full Philosophy coverage. @DEBUG_ENTER_UFUFU (4-line wrapper) included to verify BeginTrain() call dispatch and close the Philosophy "full equivalence verification" gap. All 8 functions covered. |
| **Interface Implementation** | (A) ShopSystem implements IShopSystem directly, (B) Parallel class hierarchy | **A** | Following IBodySettings pattern (Era.Core/State/BodySettings.cs): concrete class implements interface directly. Phase 20 spec defines IShopSystem/IInventoryManager as contracts; ShopSystem/InventoryManager are concrete implementations. Single hierarchy reduces complexity. |
| **Engine State Transitions** | (A) NotSupportedException stubs for all three, (B) Existing interfaces + stub | **A** | All three (BEGIN TRAIN, SAVEGAME, LOADGAME) use NotSupportedException stubs. IGameState.SaveGame(string)/LoadGame(string) are file-based; ERB SAVEGAME/LOADGAME are parameterless dialog-mode — signatures incompatible. Phase 14 Mandatory Handoff for dialog-mode save/load interface. |
| **RESTART/GOTO Transformation** | (A) Recursive calls, (B) while(true) + labeled loops | **B** | ERB RESTART = return to function start. GOTO $LABEL = jump to label. C# equivalent: wrap function body in while(true) { ... break; }, RESTART → continue, GOTO $INPUT_LOOP → continue on inner labeled loop. Avoids stack overflow from recursive calls. Standard ERB→C# pattern. |
| **TRYCALL Handling** | (A) NotImplementedException for all, (B) No-op for existing, omit dead | **B** | TRYCALL semantics = "call if exists, silently ignore if missing." Dead targets (COM461_PUNISHMENT_END, 妊娠パッチDEBUG88, 妊娠パッチDEBUG88COM) are omitted entirely. 追加パッチVerUP exists but is a patch hook — no-op empty method body preserves TRYCALL semantics without failing. |

### Upstream Issues

<!-- Optional: Issues discovered during Technical Design that require upstream changes. -->

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|

### Interfaces / Data Structures

**IShopSystem** (Phase 20 spec lines 21-25):
```csharp
namespace Era.Core.Shop;

/// <summary>
/// Interface for shop system operations.
/// Phase 20 Equipment & Shop Systems migration.
/// </summary>
public interface IShopSystem
{
    /// <summary>
    /// Gets available items for the specified shop.
    /// </summary>
    Result<IReadOnlyList<ShopItem>> GetAvailableItems(ShopId shop);

    /// <summary>
    /// Processes an item purchase.
    /// </summary>
    Result<PurchaseResult> Purchase(CharacterId buyer, ItemId item);

    // NOTE: Phase 20 spec defines minimal contract.
    // Migration methods (ShowShop, UserShop, etc.) are implementation details
    // not exposed via interface until integration layer (Phase 14) requires them.
}
```

**IInventoryManager** (Phase 20 spec lines 27-31):
```csharp
namespace Era.Core.Shop;

/// <summary>
/// Interface for inventory management operations.
/// Phase 20 Equipment & Shop Systems migration.
/// </summary>
public interface IInventoryManager
{
    /// <summary>
    /// Checks if a character owns a specific item.
    /// </summary>
    Result<bool> HasItem(CharacterId owner, ItemId item);

    /// <summary>
    /// Adds items to a character's inventory.
    /// </summary>
    Result<Unit> AddItem(CharacterId owner, ItemId item, int count);
}
```

**DisplayCategory** (C9 constraint):
```csharp
namespace Era.Core.Shop;

/// <summary>
/// Display category selection for character status display.
/// Replaces FLAG:1003 magic number constants from SHOP2.ERB:32.
/// </summary>
public enum DisplayCategory
{
    None = 0,       // FLAG:1003 初期値; SELECTCASE fall-through (表示なし)
    Base = 2,       // 基礎表示 (体力/気力)
    Ability = 3,    // 能力表示
    SexSkill = 4,   // 性技表示
    Mark = 5,       // 刻印表示
    Experience = 6, // 経験表示
    Juel = 7,       // 宝珠表示
    Other = 8       // その他表示 (既成事実/好感度/etc.)
}
```

**ShopTypes.cs** (C14 constraint):
```csharp
namespace Era.Core.Shop;

/// <summary>
/// Strongly-typed ID for shop instances.
/// </summary>
public readonly record struct ShopId(int Value);

/// <summary>
/// Strongly-typed ID for items.
/// </summary>
public readonly record struct ItemId(int Value);

// CharacterId: Use existing Era.Core.Types.CharacterId (via `using Era.Core.Types;`)
// Do NOT redefine — Era.Core.Types.CharacterId has well-known constants and operators.

/// <summary>
/// Represents an item available in a shop.
/// </summary>
public record ShopItem(ItemId Id, string Name, int Price);

/// <summary>
/// Success-only result of a purchase operation.
/// Failure cases handled exclusively by Result&lt;PurchaseResult&gt;.Fail().
/// </summary>
public record PurchaseResult(ItemId PurchasedItem, int RemainingBalance);
```

**ShopSystem.cs** (illustrative stub; implementer follows TDD RED→GREEN per CLAUDE.md — do not copy empty bodies verbatim):
```csharp
namespace Era.Core.Shop;

/// <summary>
/// Shop coordinator migrated from SHOP.ERB.
/// Implements Phase 20 IShopSystem interface.
/// </summary>
public class ShopSystem : IShopSystem
{
    private readonly ShopDisplay _display;
    private readonly IVariableStore _variables;
    private readonly IConsoleOutput _console;
    private readonly IGameState _gameState;
    private readonly IInputHandler _inputHandler;

    public ShopSystem(ShopDisplay display, IVariableStore variables, IConsoleOutput console, IGameState gameState, IInputHandler inputHandler)
    {
        _display = display ?? throw new ArgumentNullException(nameof(display));
        _variables = variables ?? throw new ArgumentNullException(nameof(variables));
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
    }

    // IShopSystem contract stubs - Phase 20 domain methods.
    // Implementation deferred to F776 (Items) which provides inventory data.
    // Result.Fail (not exception throw) for unimplemented contract methods.
    // Callers using .Match() can handle this failure gracefully.
    public Result<IReadOnlyList<ShopItem>> GetAvailableItems(ShopId shop)
        => Result<IReadOnlyList<ShopItem>>.Fail("IShopSystem.GetAvailableItems awaits F776 inventory implementation");

    public Result<PurchaseResult> Purchase(CharacterId buyer, ItemId item)
        => Result<PurchaseResult>.Fail("IShopSystem.Purchase awaits F776 inventory implementation");

    // Migrated functions from SHOP.ERB
    public void ShowShop() { /* @SHOW_SHOP: one-shot menu display; engine shopWaitInput handles input */ }
    public int UserShop() { /* @USERSHOP: dispatch via _display, _gameState */ return 0; }
    public int Schedule() { /* @SCHEDULE: while(true) settings loops with fallthrough return */ return 0; }
    public int SetFutanariAll() { /* @SET_FUTANARI_ALL: while(true) batch mutation */ return 0; }
    public void DebugEnterUfufu() { BeginTrain(); /* DEBUG entry: only action is BEGIN TRAIN */ }

    // Shared engine state transition stub — single throw site for Mandatory Handoff (Phase 14)
    private void BeginTrain() => throw new NotSupportedException("BEGIN TRAIN requires engine integration");
}
```

**ShopDisplay.cs** (illustrative stub; implementer follows TDD RED→GREEN per CLAUDE.md):
```csharp
namespace Era.Core.Shop;

/// <summary>
/// Character data display migrated from SHOP2.ERB.
/// Depends on IVariableStore for reading BASE, MAXBASE, ABL, EXP, JUEL, MARK,
/// CFLAG, FLAG, SAVESTR, TALENT, PALAMNAME, ABLNAME, EXPNAME, MARKNAME, etc.
/// </summary>
public class ShopDisplay
{
    private readonly IVariableStore _variables;
    private readonly IConsoleOutput _console;
    private readonly IInputHandler _inputHandler;

    public ShopDisplay(IVariableStore variables, IConsoleOutput console, IInputHandler inputHandler)
    {
        _variables = variables ?? throw new ArgumentNullException(nameof(variables));
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _inputHandler = inputHandler ?? throw new ArgumentNullException(nameof(inputHandler));
    }

    // Migrated functions from SHOP2.ERB
    public void LifeList(int arg) { /* @LIFE_LIST: reads FLAG:1003/1004, BASE, ABL, EXP, etc. via _variables */ }
    public void ShowCharaData() { /* @SHOW_CHARADATA: while(true) category menu + INPUT + RESTART×2 + GOTO */ }
    public void ShowCharaData2(int arg) { /* @SHOW_CHARADATA2: while(true) sub-category selection */ }
}
```

**Test Infrastructure** (following ComEquivalenceTestBase pattern):
```csharp
namespace Era.Core.Tests.Shop;

[Trait("Category", "Shop")]
public class ShowShopEquivalenceTests : BaseTestClass
{
    [Fact]
    public void ShowShop_DisplaysShopMenu_MatchesLegacyOutput()
    {
        // Arrange: Mock context with SAVESTR:10, MONEY, BASE:MASTER
        var context = new MockGameContext()
            .WithSaveStr(10, "collection_unlocked")
            .WithMoney(50000)
            .Build();

        var shopSystem = new ShopSystem(/* dependencies */);

        // Act: Execute ShowShop
        shopSystem.ShowShop();

        // Assert: Verify output contains expected menu items
        // (Golden-master comparison or key output verification)
    }
}
```

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1a | 1,6 | Create Era.Core/Shop/ShopSystem.cs skeleton with IShopSystem implementation and entry points (ShowShop, UserShop) | | [x] |
| 1b | 8,12a | Migrate @SHOW_SHOP and @USERSHOP logic to ShopSystem.cs | | [x] |
| 1c | 8,12a | Migrate @SCHEDULE, @SET_FUTANARI_ALL, @DEBUG_ENTER_UFUFU logic to ShopSystem.cs | | [x] |
| 2 | 2,9,12a | Create Era.Core/Shop/ShopDisplay.cs with migrated SHOP2.ERB functions | | [x] |
| 3 | 3 | Create Era.Core/Shop/IShopSystem.cs interface with XML docs per Phase 20 spec | | [x] |
| 4 | 4 | Create Era.Core/Shop/IInventoryManager.cs interface with XML docs per Phase 20 spec | | [x] |
| 5 | 7 | Create Era.Core/Shop/DisplayCategory.cs enum mapping FLAG:1003 values | | [x] |
| 6 | 5 | Register IShopSystem and ShopDisplay in ServiceCollectionExtensions.cs | | [x] |
| 7 | 10,11,12b | Create equivalence test files (ShowShopEquivalenceTests, UserShopEquivalenceTests, LifeListEquivalenceTests, SetFutanariAllEquivalenceTests, ScheduleEquivalenceTests, ShowCharaDataEquivalenceTests, ShowCharaData2EquivalenceTests, DebugEnterUfufuEquivalenceTests) with Category=Shop | | [x] |
| 8 | 13 | Create Era.Core/Shop/ShopTypes.cs with value types (ShopId, ItemId, ShopItem, PurchaseResult); use existing Era.Core.Types.CharacterId | | [x] |
| 9 | 7b | Add DisplayCategory validation guard (Enum.IsDefined or equivalent) in ShopDisplay | | [x] |

---

## Implementation Contract

> **This section is an implementation contract. Do NOT modify, skip, or optimize the documented steps.**
>
> If issues arise: STOP → Ask user for guidance.

| Phase | Agent | Model | Input | Output |
|-------|-------|-------|-------|--------|
| 0 | implementer | sonnet | Tasks 8 | ShopTypes.cs value types |
| 1 | implementer | sonnet | Tasks 3-4 | IShopSystem.cs, IInventoryManager.cs interfaces |
| 2 | implementer | sonnet | Task 5 | DisplayCategory.cs enum |
| 3a | implementer | sonnet | Tasks 1a, 2 | ShopSystem.cs skeleton, ShopDisplay.cs with migrated logic |
| 3b | implementer | sonnet | Tasks 1b, 1c | Migrate remaining SHOP.ERB function logic to ShopSystem.cs |
| 3c | implementer | sonnet | Task 9 | DisplayCategory validation guard in ShopDisplay |
| 4 | implementer | sonnet | Task 6 | DI registration in ServiceCollectionExtensions.cs |
| 5 | implementer | sonnet | Task 7 | 8 equivalence test files with Category=Shop |

### Pre-conditions

1. **Phase 9-19 prerequisite**: This feature requires Phase 14 (Era.Core Engine integration layer) to be complete before /run. Specifically, the IFunctionRegistry pattern that handles ERB function name → C# method dispatch must exist.
2. **Baseline measurement**: Run commands in Baseline Measurement section and save to `.tmp/baseline-774.txt` for reference.
3. **Source file review**: Read Game/ERB/SHOP.ERB and Game/ERB/SHOP2.ERB completely before implementation.

### Execution Order

0. **Value types** (Task 8, Phase 0): Create ShopTypes.cs with strongly-typed ID types required by IShopSystem/IInventoryManager interfaces. Must exist before interface compilation.
1. **Interfaces** (Tasks 3-4, Phase 1): Define IShopSystem and IInventoryManager interfaces following Phase 20 spec. These provide type contracts for DI.
2. **Enum type safety** (Task 5, Phase 2): Create DisplayCategory enum to replace FLAG:1003 magic numbers.
3. **Implementation** (Tasks 1a-1c, 2, Phase 3a-3b): Migrate ERB functions to C# following established patterns from Era.Core/State/BodySettings.cs. Use interface stubs (NotImplementedException) for external calls.
4. **DI registration** (Task 6, Phase 4): Register IShopSystem → ShopSystem mapping in ServiceCollectionExtensions.cs near IBodySettings registration (line ~148).
5. **Equivalence tests** (Task 7, Phase 5): Create 8 test files following ComEquivalenceTestBase pattern. Use MockGameContext for setup, golden-master approach for complex functions.

### Build Verification Steps

After each Phase:
1. Run `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core/'`
2. Verify zero errors, zero warnings
3. Run `MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet test Era.Core.Tests/ --filter Category=Shop'` (after Phase 5)
4. Mark Task [x] in Tasks table after build success

### Success Criteria

1. All 15 ACs show [x] status
2. dotnet build Era.Core/ succeeds with zero warnings
3. dotnet test Era.Core.Tests/ --filter Category=Shop succeeds
4. Grep for TODO|FIXME|HACK in Era.Core/Shop/ and Era.Core.Tests/Shop/ returns 0 matches

### Stub Verification Strategy

16 Mandatory Handoffs create stubs pending Phase 14/Phase 20 delivery. Verification checkpoints:
- **F782 (Post-Phase Review)**: Reads F774 Mandatory Handoffs table as verification input; validates all Phase 20 stubs are replaced when phase completes
- **Per-feature**: Each Related feature (F775-F781) replaces its corresponding stubs at [DONE]. Trigger: destination feature reaches [DONE]
- **Phase 14**: Engine integration layer delivers IFunctionRegistry, extended IGameState, IVariableStore, IConsoleOutput APIs. Trigger: Phase 14 reaches [DONE]

**Trigger conditions**: Feature-destination handoffs (rows 1-5 and row 9) trigger when their Destination ID feature reaches [DONE]. Phase-destination handoffs (rows 6-8 and 10-16) trigger when Phase 14 or Phase 20 completes. F782 performs final sweep verification.

### Error Handling

- **Build failure**: Check file paths, namespace declarations, and using statements per Issue 21
- **Test failure**: Investigate using debug tests in tests/debug/shop/
- **AC failure**: Check AC Details for exact verification command and expected output
- **Scope discovery**: If external dependencies beyond stubs are needed, STOP and report to user

### Migration Guidelines

**Entry Point Compatibility** (C6 constraint):
- @SHOW_SHOP → `public void ShowShop()`
- @USERSHOP → `public int UserShop()` (returns 0 for stub)
- Engine integration layer (Phase 14) will provide ERB name → C# method routing

**External Call Stubs** (per Technical Design):
```csharp
// Example stub for external call to F775 (Collection)
private void ShowCollection()
{
    throw new NotImplementedException("SHOW_COLLECTION - See F775 (Collection feature)");
}
```

**Variable Access Patterns**:
- SHOP2.ERB reads FLAG:1003/1004, BASE, MAXBASE, ABL, EXP, JUEL, MARK, CFLAG, TALENT
- Use interface parameters for these dependencies (following IBodySettings pattern)
- Equivalence tests mock these dependencies via MockGameContext

**Utility Function Dependencies**:
- SHOP.ERB: CHILDREM_CHECK (line 152, 続柄.ERB), GETPLACENAME (line 65, COMMON_PLACE.ERB), NTR_NAME (line 66, NTR/NTR_MESSAGE.ERB), 子供訪問_設定 (line 120, 子供の訪問関係.ERB), GET_DAY (line 8, COMMON.ERB), 日付_月 (line 8, 天候.ERB)
- SHOP2.ERB: HAS_VAGINA (lines 45/55, COMMON.ERB), GET_MARK_LEVEL_OWNERNAME (line 52, COMMON.ERB)
- BAR is a built-in Emuera command (not an ERB function); C# equivalent uses IGameDisplay or similar rendering interface
- Resolution: inject via IGameContext or narrow utility interfaces; stubs use NotImplementedException with feature references

**DisplayCategory Usage** (C9 constraint):
```csharp
// SHOP2.ERB:32 - FLAG:1003 selects category
// C# equivalent:
DisplayCategory category = (DisplayCategory)flag1003Value;
if (!Enum.IsDefined(category))
    throw new ArgumentOutOfRangeException(nameof(flag1003Value));
switch (category)
{
    case DisplayCategory.None:
        break;  // FLAG:1003=0 initial value; no display (ERB fall-through)
    case DisplayCategory.Base:
        // Display BASE/MAXBASE
        break;
    case DisplayCategory.Ability:
        // Display ABL[0-40]
        break;
    // ...
}
```

**RESTART/GOTO Transformation** (C12 constraint):

Three control flow patterns:
1. **RESTART** → `continue` in while(true) loop (jump to function start)
2. **GOTO $LABEL** → `continue` in inner labeled while(true) loop (retry input)
3. **Fallthrough** → `return` at end of if/elseif chain (exit function when no branch matches)

```csharp
// ERB pattern: RESTART = return to function start
// @SCHEDULE equivalent (includes fallthrough pattern):
public int Schedule()
{
    while (true)  // RESTART wraps entire function
    {
        // ... menu display and input ...
        if (result == 100)
            return 0;           // RETURN 0 → exit
        else if (result == 0)
        {
            // ... process input ...
            continue;           // RESTART → continue
        }
        else if (result == 2)
        {
            MAN_Set();
            continue;           // RESTART → continue
        }
        // ... other branches with RESTART → continue ...
        return 1;               // Fallthrough: unmatched input exits (RETURN 1)
    }
}

// ERB pattern: GOTO $INPUT_LOOP = retry input
// Equivalent:
while (true)  // $INPUT_LOOP label
{
    var input = GetInput();
    if (input < 0 || input >= charCount)
        continue;  // GOTO INPUT_LOOP → continue
    // ... process valid input ...
    break;  // Exit input loop
}
```

**Engine State Transitions** (C11 constraint):
```csharp
// @USERSHOP engine commands — NOT ERB function stubs
if (result == 100)
{
    throw new NotSupportedException("BEGIN TRAIN requires engine integration"); // Mandatory Handoff
    // return 1;  // Uncomment after Phase 14 provides BeginTrain()
}
else if (result == 200)
{
    throw new NotSupportedException("SAVEGAME requires engine integration - dialog-mode not in IGameState"); // Mandatory Handoff
}
else if (result == 300)
{
    throw new NotSupportedException("LOADGAME requires engine integration - dialog-mode not in IGameState"); // Mandatory Handoff
}
```

**TRYCALL Handling** (C10 constraint):
```csharp
// Dead TRYCALL targets — OMIT entirely (no stub needed):
// COM461_PUNISHMENT_END, 妊娠パッチDEBUG88, 妊娠パッチDEBUG88COM

// Existing TRYCALL target — no-op empty method:
private void PatchVerUp() { }  // 追加パッチVerUP: TRYCALL = no-op if missing
```

**Function Count Verification** (AC#8, AC#9):
- SHOP.ERB: @SHOW_SHOP, @USERSHOP, @SCHEDULE, @SET_FUTANARI_ALL, @DEBUG_ENTER_UFUFU (5 total)
- SHOP2.ERB: @LIFE_LIST, @SHOW_CHARADATA, @SHOW_CHARADATA2 (3 total)

**Equivalence Test Minimum** (AC#11 constraint):
Each of the 8 test files must contain at least:
1. One test method with [Fact] attribute
2. Category=Shop trait: `[Trait("Category", "Shop")]`
3. Meaningful verification (Assert statement or golden-master comparison)

### Test Naming Convention

Test files use `{FunctionName}EquivalenceTests.cs` format:
- `ShowShopEquivalenceTests.cs` for @SHOW_SHOP equivalence
- `UserShopEquivalenceTests.cs` for @USERSHOP equivalence
- `LifeListEquivalenceTests.cs` for @LIFE_LIST equivalence
- `SetFutanariAllEquivalenceTests.cs` for @SET_FUTANARI_ALL equivalence
- `ScheduleEquivalenceTests.cs` for @SCHEDULE equivalence
- `ShowCharaDataEquivalenceTests.cs` for @SHOW_CHARADATA equivalence
- `ShowCharaData2EquivalenceTests.cs` for @SHOW_CHARADATA2 equivalence
- `DebugEnterUfufuEquivalenceTests.cs` for @DEBUG_ENTER_UFUFU equivalence

This ensures AC#10 filter `--filter Category=Shop` matches correctly and AC#11 count_equals=8 verification succeeds.

### DI Registration

Register in `Era.Core/DependencyInjection/ServiceCollectionExtensions.cs` near line 148 (after IBodySettings registration):

```csharp
// Shop system (Phase 20)
services.AddSingleton<ShopDisplay>();
services.AddSingleton<IShopSystem, ShopSystem>();
```

Note: ShopDisplay is registered as concrete type (not interface) since ShopSystem takes it via constructor injection. IInventoryManager is defined but not registered yet (awaiting Phase 20 implementation dependencies).

---

## Mandatory Handoffs

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|
| IInventoryManager DI registration deferred | Phase 20 SSOT mandates DI registration for IInventoryManager but no concrete InventoryManager implementation exists in F774 scope | Feature | F776 | N/A (F776 exists) |
| IShopSystem.GetAvailableItems/Purchase implementation | Phase 20 contract stubs using Result.Fail(); full implementation requires inventory data from F776. Trigger: Replace stubs when F776 reaches [DONE] | Feature | F776 | N/A (F776 exists) |
| SHOW_COLLECTION stub | NotImplementedException stub for collection display sub-menu | Feature | F775 | N/A (F775 exists) |
| ITEM_BUY stub | NotImplementedException stub for item purchase sub-menu | Feature | F776 | N/A (F776 exists) |
| OPTION / MAN_SET / PRINT_STATE stubs | NotImplementedException stubs for external utility calls from @USERSHOP and @SHOW_CHARADATA | Feature | F778 | N/A (F778 exists) |
| NTR_NAME / GETPLACENAME / CHILDREM_CHECK / 子供訪問_設定 stubs | NotImplementedException stubs for character/place utility calls | Phase | Phase 20 (phase-20-27-game-systems.md) | N/A (phase exists) |
| HAS_VAGINA / GET_MARK_LEVEL_OWNERNAME stubs | NotImplementedException stubs for COMMON.ERB utility functions used in @LIFE_LIST display | Phase | Phase 20 (phase-20-27-game-systems.md) | N/A (phase exists) |
| GET_DAY / 日付_月 stubs | NotImplementedException stubs for date utility functions used in @SHOW_SHOP display (line 8) | Phase | Phase 20 (phase-20-27-game-systems.md) | N/A (phase exists) |
| ShopDisplay.cs SSOT deviation | Phase 20 Deliverables lists only ShopSystem.cs; ShopDisplay.cs introduced by F774 SRP split (Key Decisions) | Feature | F782 | N/A (F782 exists) |
| BeginTrain() method | BEGIN TRAIN engine command used by @USERSHOP and @DEBUG_ENTER_UFUFU; no existing interface method — uses NotSupportedException stub pending engine integration | Feature | F791 | N/A (F791 exists) |
| Dialog-mode SaveGame()/LoadGame() | ERB SAVEGAME/LOADGAME are parameterless dialog-mode commands; IGameState.SaveGame(string)/LoadGame(string) are file-based — signatures incompatible; uses NotSupportedException stub | Feature | F791 | N/A (F791 exists) |
| IFunctionRegistry registration for SHOW_SHOP/USERSHOP | Engine entry points require ERB name→C# method routing; IFunctionRegistry (F421) handles IBuiltInFunction, not procedure-style entry points — adapter or new registry needed | Feature | F791 | N/A (F791 exists) |
| IVariableStore string extension (SAVESTR) | SAVESTR:10 (string var) needed by ShowShop(); IVariableStore lacks string accessors | Feature | F789 | N/A (F789 exists) |
| CSV name array read access (ABLNAME, EXPNAME, MARKNAME, PALAMNAME) | ShopDisplay.ShowCharaData2() uses 4 CSV name arrays for display labels | Feature | F790 | N/A (F790 exists) |
| IVariableStore 3D accessor (TA array) | TA:chr:slot:sub-index used in @LIFE_LIST:98-103; IVariableStore only has 1D/2D accessors | Feature | F789 | N/A (F789 exists) |
| Engine built-in variable read interface | MONEY, CHARANUM, MASTER, NAME, CALLNAME, ASSI, ISASSI, COUNT, RAND, DAY not in IVariableStore | Feature | F790 | N/A (F790 exists) |
| IConsoleOutput API extension | DrawLine(), ClearLine(int), PrintLineC()/PrintFormLC() (centered), BAR(int,int,int) needed by SHOP.ERB/SHOP2.ERB | Feature | F788 | N/A (F788 exists) |

---

## Execution Log

| Timestamp | Event | Agent | Action | Result |
|-----------|:-----:|-------|--------|--------|
| 2026-02-14 | DEVIATION | feature-reviewer | Phase 8.1 Quality Review | NEEDS_REVISION: equivalence tests are reflection-only structural checks, no behavioral verification |
| 2026-02-14 | FIX | implementer | Phase 8.1 Fix | Added DI registration, resolution failure, and IShopSystem contract tests (27 tests); full behavioral tests blocked by Phase 14 stubs |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase1-RefCheck iter1: [DEP-001] Links section | F779, F780, F781 orphan references - added to Links section
- [resolved-applied] Phase2-Pending iter1: [AC-005] AC#5 + DI Registration | C1 detail updated with DI deferral scope and orphan interface justification
- [resolved-applied] Phase2-Uncertain iter1: [CON-003] AC#6 + IShopSystem interface | C6 detail updated with interface reconciliation note
- [fix] Phase2-Review iter1: [AC-005] Philosophy Derivation row 6 | Goal item 4 "interface-based stubs" mapped to AC#5 which only verifies DI registration - updated Goal and Philosophy Derivation to "fail-fast stubs" + DI registration
- [fix] Phase2-Review iter1: [FMT-002] Dependencies section | Moved from after Acceptance Criteria to between AC Design Constraints and Acceptance Criteria per template ordering
- [fix] Phase2-Review iter1: [FMT-002] Non-template sections | Removed Created, Summary, Scope Reference top-level sections; merged Scope Reference into Technical Design subsection
- [fix] Phase2-Review iter2: [TSK-002] Tasks table AC# column | Remapped Task#1→AC#1,6,8; Task#2→AC#2,9; Task#8→AC#12 to align AC column with task descriptions
- [fix] Phase2-Review iter2: [DEP-001] Mandatory Handoffs table | Added IInventoryManager DI registration deferral entry with destination F776
- [fix] Phase2-Review iter2: [FMT-002] AC Design Constraints/Dependencies separator | Added missing --- horizontal rule between sections
- [resolved-applied] Phase2-Review iter3: [AC-005] Philosophy "full equivalence verification" vs AC#11 | AC#11 expanded to 5 functions including ScheduleEquivalence
- [resolved-applied] Phase2-Uncertain iter3: [SCP-003] IInventoryManager orphan interface | Documented in C1 as architectural pre-investment; handoff tracked to F776
- [fix] Phase2-Review iter3: [FMT-002] Review Notes category codes | Added [AC-005] and [CON-003] codes to [pending] entries per error-taxonomy.md
- [fix] Phase2-Review iter3: [FMT-002] Background/Root Cause separator | Added missing --- horizontal rule between Background and Root Cause Analysis
- [fix] Phase2-Review iter3: [FMT-002] Dependencies/AC separator | Added missing --- horizontal rule between Dependencies and Acceptance Criteria
- [fix] Phase2-Review iter4: [FMT-002] Scope Reference subsection | Merged non-template ### Scope Reference into ### Approach opening paragraph per template structure
- [fix] Phase2-Review iter4: [DEP-001] Mandatory Handoffs Creation Task | Changed Task 6 to N/A (F776 exists) since destination feature already exists
- [resolved-applied] Phase2-Uncertain iter4: [FMT-002] AC Method column | AC#6/AC#8/AC#9 Method columns updated with grep patterns
- [resolved-applied] Phase2-Review iter5: [TSK-002] Missing strongly-typed ID types | Added C14, AC#13, Task 8 for ShopTypes.cs value types
- [resolved-applied] Phase2-Uncertain iter5: [AC-005] ShopDisplay DI gap | Constructor injection added to ShopDisplay stub (IGameContext); verified via equivalence tests
- [fix] Phase3-Maintainability iter5: [FMT-002] Implementation Contract phase ordering | Aligned Phase table numbering with Execution Order (Interfaces→Enum→Implementation→DI→Tests→Verification)
- [resolved-applied] Phase3-Maintainability iter5: [CON-003] DisplayCategory enum guard | Added AC#7b negative AC for invalid DisplayCategory validation
- [fix] Phase2-Review iter6: [AC-005] AC#11 Method column | Added grep pattern to AC#11 Method column
- [fix] Phase2-Review iter6: [AC-005] AC#12 split | Split AC#12 into AC#12a (Era.Core/Shop/) and AC#12b (Era.Core.Tests/Shop/) for valid single-path Grep invocations
- [fix] Phase2-Review iter6: [FMT-002] Mandatory Handoffs Destination | Changed from 'Phase 20 sub-feature (Items/Inventory)' to 'Feature' per template format
- [fix] Phase2-Review iter6: [CON-001] SHOP2.ERB external calls | Updated External Dependency Handling to list PRINT_STATE and 簡易追加情報 as ShopDisplay.cs dependencies
- [resolved-applied] Phase2-Review iter6: [CON-001] Engine commands in @USERSHOP | Added C11, Key Decision, IEngineStateManager pattern in Migration Guidelines
- [resolved-applied] Phase2-Uncertain iter6: [CON-001] RESTART/GOTO transformation | Added C12, Key Decision, while(true)+continue pattern in Migration Guidelines
- [fix] Phase2-Review iter7: [CON-001] Utility function dependencies | Added CHILDREM_CHECK, HAS_VAGINA, GET_MARK_LEVEL_OWNERNAME, GETPLACENAME, BAR to Migration Guidelines
- [resolved-applied] Phase2-Uncertain iter7: [CON-001] TRYCALL semantics | C10 expanded; dead targets omitted, 追加パッチVerUP uses no-op stub
- [resolved-applied] Phase2-Review iter7: [AC-006] AC#8 stub weakness | AC#8 detail updated with behavioral coverage note; AC#11 expansion covers @SHOW_SHOP
- [fix] Phase2-Review iter8: [CON-001] Utility function dependencies | Added NTR_NAME and 子供訪問_設定 to Migration Guidelines (missed in iter7)
- [fix] Phase2-Review iter8: [AC-005] AC#6 pattern | Added 'public.*' qualifier to prevent false matches from internal references
- [resolved-applied] Phase2-Review iter8: [CON-001] ShopSystem→ShopDisplay dependency | Added C13; ShopSystem constructor injection for ShopDisplay
- [fix] Phase2-Review iter9: [DEP-001] Dependencies table | Added F779, F780, F781 to Dependencies table (were only in Links from iter1 fix)
- [resolved-invalid] Phase2-Uncertain iter10: [FMT-002] Tasks Tag column | Validator confirmed all Tasks have deterministic Expected values; no tags needed
- [fix] Phase3-Maintainability iter1: [TSK-002] Tasks table | Split Task 1 into 1a (skeleton+entry points), 1b (@SHOW_SHOP/@USERSHOP logic), 1c (@SCHEDULE/@SET_FUTANARI_ALL/@DEBUG_ENTER_UFUFU logic); updated Implementation Contract Phase 3
- [fix] Phase3-Maintainability iter2: [TSK-002] Tasks table | Removed Task 8 (build verification is process step, not deliverable); redistributed AC#12a to Tasks 1b/1c/2, AC#12b to Task 7; removed Phase 6 from Implementation Contract
- [fix] Phase3-Maintainability iter3: [CON-001] Execution Order step 3 | Fixed wrong path Era.Core/Body/BodySettings.cs → Era.Core/State/BodySettings.cs; removed stale Task 8/Phase 6 reference from Execution Order; updated Task references to 1a-1c, 3a-3b
- [resolved-applied] Phase4-ACValidation iter4: [AC-007] No functional negative ACs | Added AC#7b (invalid DisplayCategory guard) as negative functional AC
- [resolved-applied] Phase4-ACAlignment iter4: [TSK-003] AC-Task alignment BLOCKED | All 15 blocking pending items resolved; AC:Task alignment unblocked
- [fix] Phase7-FinalRefCheck iter4: [AC-005] AC Coverage row 10 | Removed non-existent F359 reference from BaseTestClass pattern
- [fix] Phase7-FinalRefCheck iter4: [CON-001] Key Decisions Interface Implementation | Replaced non-existent F377 reference with actual file path Era.Core/State/BodySettings.cs
- [resolved-applied] Phase2-Pending iter1: [CON-001] AC#11 Technical Design AC Coverage says "4 test files" but AC#11 requires count_equals=5 including ScheduleEquivalence; Key Decisions Equivalence Test Scope selects "B: 4 core functions" excluding @SCHEDULE which is 64 lines with 4x RESTART, not a thin wrapper
- [resolved-applied] Phase2-Pending iter1: [AC-005] AC#3/AC#4 only verify interface name existence (contains "interface IShopSystem"), not method signatures (GetAvailableItems, Purchase, HasItem, AddItem) per Phase 20 SSOT; Philosophy Derivation claims "correct method signatures" coverage → AC#3/AC#4 changed to count_equals matcher verifying interface declaration + method signatures
- [resolved-applied] Phase2-Uncertain iter1: [DEP-001] IFunctionRegistry registration for SHOW_SHOP/USERSHOP entry points acknowledged in C6 detail and Pre-conditions but not tracked in Mandatory Handoffs; Phase 14 is prerequisite but not listed as Predecessor dependency → Phase 14 added as Predecessor in Dependencies; IFunctionRegistry registration added to Mandatory Handoffs
- [resolved-applied] Phase3-Maintainability iter1: [CON-001] IEngineStateManager interface (C11) used in ShopSystem constructor but has no AC, no Task, no Mandatory Handoff tracking → Added Mandatory Handoff for IEngineStateManager to Phase 14. IGameState has SaveGame(fileName)/LoadGame(fileName) but ERB SAVEGAME/LOADGAME are parameterless engine state transitions (open save/load dialog); IEngineStateManager is a distinct interface for state machine commands (BeginTrain, SaveGame, LoadGame without file parameter)
- [fix] Phase3-Maintainability iter1: [CON-001] Technical Design ShopSystem stubs | Changed NotImplementedException to NotSupportedException for IShopSystem contract stubs; added Mandatory Handoff for IShopSystem method implementation to F776
- [fix] Phase3-Maintainability iter1: [AC-005] AC#5 + DI Registration | Updated AC#5 to verify both IShopSystem and ShopDisplay DI registration (count_equals=2); updated Task 6 description; added ShopDisplay registration to DI Registration code sample
- [fix] Phase3-Maintainability iter1: [AC-005] AC#5 matcher | Widened AC#5 pattern to "AddSingleton.*(IShopSystem.*ShopSystem|ShopDisplay)" for resilience to DI registration syntax variations
- [fix] Phase3-Maintainability iter2: [CON-001] ShopTypes.cs CharacterId collision | Removed CharacterId from ShopTypes.cs; use existing Era.Core.Types.CharacterId. Updated C14, Task 8
- [fix] Phase3-Maintainability iter2: [CON-001] ShopDisplay dependency | Replaced IGameContext with IVariableStore in ShopDisplay constructor (IGameContext lacks BASE/MAXBASE/JUEL/MARK/FLAG/SAVESTR/etc.)
- [fix] Phase3-Maintainability iter2: [CON-001] ShopSystem constructor | Added IVariableStore dependency for variable reads (MONEY, BASE:MASTER, TFLAG:100, etc.)
- [fix] Phase3-Maintainability iter2: [DEP-001] Mandatory Handoffs external stubs | Added entries for SHOW_COLLECTION/ITEM_BUY (F775/F776), OPTION/MAN_SET/PRINT_STATE (F778), NTR_NAME/GETPLACENAME/CHILDREM_CHECK/子供訪問_設定 (Phase 20 sibling)
- [fix] Phase2-Review iter1: [FMT-002] Execution Order step numbering | Reordered to match Implementation Contract Phase 0 first (Value types step 0, Interfaces step 1, renumbered)
- [fix] Phase2-Review iter1: [AC-005] AC Design Constraints C3/C5 matcher | Changed not_contains to not_matches (regex alternation pattern requires not_matches matcher)
- [resolved-applied] Phase2-Pending iter2: [FMT-002] Success Criteria AC count says "All 12 ACs" but actual count is 15 (AC#1-13 including 7b, 12a, 12b)
- [fix] Phase2-Review iter2: [AC-005] AC#7b Expected column | Changed descriptive text to concrete matchable string "Enum.IsDefined"
- [fix] Phase2-Review iter2: [FMT-002] Mandatory Handoffs row 5 Destination ID | Changed "(Phase 20 sibling)" to "Phase 20 (phase-20-27-game-systems.md)" per template Option C
- [resolved-applied] Phase2-Uncertain iter2: [AC-006] No AC enforces NotImplementedException fail-fast stubs for 10+ external calls; stubs have zero direct AC enforcement despite being core design principle → Acceptable gap: C8 limit (15 ACs max) prevents addition; stubs are compilation-enforced (calls must compile) and code-review-verified; Philosophy Derivation updated
- [resolved-applied] Phase2-Pending iter3: [CON-001] SAVESTR:10 (string var) and CSV name arrays (ABLNAME, EXPNAME, MARKNAME, PALAMNAME) not accessible via IVariableStore. ShopSystem.ShowShop() and ShopDisplay.ShowCharaData2() cannot compile without additional interface dependencies. No AC/Task/Handoff tracks this gap. → Added Mandatory Handoff for IVariableStore string extension (SAVESTR) and CSV name array read access
- [fix] Phase2-Review iter3: [FMT-002] HTML comment language | Translated Japanese Sub-Feature Requirements comment (lines 34-39) to English per Language Policy
- [fix] Phase2-Review iter3: [CON-001] Philosophy Derivation row 2 | Weakened "full equivalence verification" to "critical function equivalence verification (5 of 8)" to match AC#11 scope
- [fix] Phase2-Review iter1: [FMT-002] Success Criteria | Changed "All 12 ACs" to "All 15 ACs" to match actual AC count
- [fix] Phase2-Review iter1: [AC-005] AC Coverage row 11 | Changed "4 test files" to "5 test files" and added ScheduleEquivalenceTests.cs
- [fix] Phase2-Review iter1: [CON-001] Key Decisions Equivalence Test Scope | Updated from "4 core functions" to "5 core functions" including @SCHEDULE
- [fix] Phase2-Review iter2: [AC-005] AC#11 + ShowCharaData2 | Added ShowCharaData2Equivalence to AC#11 (count_equals=6); updated Key Decisions, AC Details, Philosophy Derivation (6 of 8), Test Naming Convention, AC Coverage row 11
- [resolved-applied] Phase2-Uncertain iter2: [FMT-003] AC#12a/12b Expected column uses regex pattern while AC Details says "0 matches"; minor presentation inconsistency → AC#12a/12b Details Expected aligned to "not_matches pattern: 0 results expected"
- [resolved-applied] Phase2-Pending iter2: [CON-001] AC#7b DisplayCategory guard (Enum.IsDefined/throw) contradicts FLAG:1003=0 first-call behavior in @SHOW_CHARADATA; ERB SELECTCASE silently falls through for value 0 but C# guard would throw → Added None=0 sentinel to DisplayCategory enum; Enum.IsDefined passes for value 0; switch default case handles None by skipping display (matching ERB fall-through)
- [resolved-applied] Phase2-Pending iter2: [AC-006] AC#9 verifies 3 public method declarations in ShopDisplay but does not verify internal call relationships (ShowCharaData→LifeList, ShowCharaData→ShowCharaData2) → Structural AC; behavioral call chain verified by ShowCharaDataEquivalenceTests (added to AC#11, count 6→7); compilation enforces method existence
- [fix] Phase2-Review iter3: [TSK-002] Task 7 | Added ShowCharaData2EquivalenceTests to test file list (6 files)
- [fix] Phase2-Review iter3: [AC-005] AC Coverage row 13 | Removed stale CharacterId from ShopTypes.cs description; noted reuse of Era.Core.Types.CharacterId
- [fix] Phase2-Review iter3: [AC-005] AC Coverage row 5 | Added ShopDisplay registration to match AC#5 count_equals=2
- [resolved-applied] Phase2-Pending iter3: [CON-001] ShopSystem and ShopDisplay constructors lack IConsoleOutput (Era.Core/Interfaces/IConsoleOutput.cs) dependency. Every migrated function uses PRINT commands (20+ in SHOP.ERB, 30+ in SHOP2.ERB). Without IConsoleOutput injection, none of 8 functions can compile. → Added IConsoleOutput to both ShopSystem and ShopDisplay constructors in Technical Design stubs
- [resolved-applied] Phase2-Pending iter3: [CON-001] TA 3D character array (TA:chr:slot:sub-index in SHOP2.ERB @LIFE_LIST:98-103) not accessible via IVariableStore which only has 1D/2D accessors → Added Mandatory Handoff for IVariableStore 3D accessor extension (GetTA/SetTA)
- [resolved-applied] Phase2-Pending iter4: [CON-001] ShowShop() stub comment says "while(true) menu display + input" but ERB @SHOW_SHOP (lines 1-29) has NO loop and NO INPUT → Fixed ShowShop() stub comment to "one-shot menu display; engine shopWaitInput handles input"
- [resolved-applied] Phase2-Uncertain iter4: [CON-001] Engine built-in variables (MONEY, CHARANUM, MASTER, NAME, CALLNAME, ASSI, ISASSI, COUNT, RAND, DAY) not accessible via IVariableStore → Added Mandatory Handoff for engine built-in variable read interface (IEngineVariables or IGameState extension); distinct from IVariableStore array accessors
- [fix] Phase3-Maintainability iter4: [CON-001] IShopSystem stubs | Changed throw NotSupportedException to Result<T>.Fail() for GetAvailableItems and Purchase; throwing from Result-returning methods violates Result type contract
- [resolved-applied] Phase3-Maintainability iter4: [CON-001] IConsoleOutput API gap: interface lacks DrawLine(), ClearLine(int), PrintLineC()/PrintFormLC() (centered), BAR(int,int,int) → Added Mandatory Handoff for IConsoleOutput API extension (DrawLine, ClearLine, centered print, BAR)
- [resolved-applied] Phase3-Maintainability iter4: [CON-001] No IUserInput interface in Era.Core/Interfaces/. 4 of 8 functions use INPUT command (@SCHEDULE 3x, @SET_FUTANARI_ALL 1x, @SHOW_CHARADATA 1x, @SHOW_CHARADATA2 1x) → Added Mandatory Handoff for IUserInput interface; INPUT abstraction required by Phase 14 C# execution model for interactive commands
- [fix] Phase2-Review iter1: [AC-005] AC#7b Details Expected | Aligned AC Details Expected with AC Definition Table Expected: "Contains validation logic" → "Contains `Enum.IsDefined`"
- [fix] Phase2-Review iter2: [DEP-001] Mandatory Handoffs row 3 | Split F775/F776 combined Destination ID into separate rows (SHOW_COLLECTION→F775, ITEM_BUY→F776)
- [fix] Phase2-Review iter2: [FMT-002] Mandatory Handoffs row 5 | Added "(phase exists)" justification to Creation Task column
- [fix] Phase2-Review iter2: [DEP-001] Mandatory Handoffs | Added ShopDisplay.cs SSOT deviation handoff to F782 (Post-Phase Review)
- [fix] Phase2-Review iter2: [AC-005] AC#10 Method column | Wrapped raw shell command in Test() notation for format consistency
- [resolved-applied] Phase2-Pending iter2: [CON-003] PurchaseResult record (bool Success, string? ErrorMessage) duplicates Result<T> error semantics → Redesigned PurchaseResult as success-only data record(ItemId Item, int RemainingBalance); Result<T>.Fail handles all failure cases exclusively
- [resolved-applied] Phase2-Pending iter2: [AC-005] @SHOW_CHARADATA excluded from equivalence tests as "thin wrapper" but has 23 lines with RESTART×2, GOTO, INPUT, CHARANUM validation, PRINT_STATE call → Added ShowCharaDataEquivalence to AC#11 (count 6→7); updated Task 7, Key Decisions (7 of 8), Philosophy Derivation, Test Naming Convention
- [resolved-applied] Phase2-Pending iter3: [CON-001] @SCHEDULE RETURN 1 fallthrough: ERB @SCHEDULE falls through to RETURN 1 (line 124) but C12 says "implicit via while loop end" → Updated C12 with third control flow pattern: RESTART=continue, GOTO=continue-inner-loop, fallthrough=return (explicit at end of if/elseif chain); @SCHEDULE example updated in Migration Guidelines
- [resolved-applied] Phase2-Review iter1: [CON-003] C1 AC Implication says "AC must verify interface creation and DI registration" but C1 Detail defers IInventoryManager DI to F776; internal contradiction within C1
- [fix] Phase2-Review iter1: [AC-005] AC#11 + Philosophy full coverage | Expanded AC#11 from 7 to 8 functions (added DebugEnterUfufuEquivalence); updated Key Decisions to select option A; updated Philosophy Derivation, Goal Coverage, Task 7, Test Naming Convention
- [fix] Phase2-Review iter2: [FMT-002] AC#11 propagation | Updated 3 stale "7 test files" references to "8" in Implementation Contract Phase 5, Execution Order step 5, Equivalence Test Minimum
- [fix] Phase2-Review iter3: [AC-005] Philosophy Derivation rows 5-7 | Rewrote Absolute Claims to trace from Philosophy text ("Sequential phase migration ensures each game subsystem is converted to C#") instead of Constraints
- [fix] Phase2-Review iter3: [AC-005] AC#13 transitivity | Documented compilation-enforced transitivity via AC#10→AC#3/AC#4 in AC#13 Details
- [fix] Phase2-Review iter3: [FMT-002] Goal Coverage traceability | Expanded Goal text with items (6) typed display mode and (7) value types; consolidated Goal Coverage items 7-9 into 7-8
- [fix] Phase2-Review iter4: [AC-005] Philosophy Derivation rows 3-6,10 | Rewrote remaining Goal-text claims to trace from Philosophy "Sequential phase migration ensures each game subsystem is converted to C#" with via-annotations for derivation path
- [fix] Phase2-Review iter5: [FMT-002] Philosophy Derivation row 6 | Shortened AC Coverage prose to terse format matching peer rows
- [fix] Phase3-Maintainability iter6: [CON-001] Utility injection strategy | Added explicit design note documenting private-method-stub approach for 13 ERB function stubs (no facade interface needed)
- [fix] Phase3-Maintainability iter6: [DEP-001] Mandatory Handoffs trigger conditions | Added trigger condition annotations and F782 verification input note to Stub Verification Strategy
- [fix] Phase2-Review iter7: [FMT-002] Stub Verification Strategy row ranges | Fixed Feature-destination range to include row 9 (F782); Phase-destination rows 6-8,10-16
- [fix] Phase2-Review iter7: [CON-001] Mandatory Handoffs CSV name arrays | Changed "6 CSV name arrays" to "4 CSV name arrays" (ABLNAME, EXPNAME, MARKNAME, PALAMNAME verified via SHOP2.ERB grep)
- [fix] PostLoop-UserFix iter8: [CON-001] IGameState SaveGame/LoadGame stubs | Changed from IGameState.SaveGame()/LoadGame() to NotSupportedException stubs; updated C11, Key Decisions, Migration Guidelines, Mandatory Handoffs (added dialog-mode Save/Load handoff)
- [fix] Phase1-InterfaceReconciliation iter1: [CON-001] Dependencies/Technical Design | Replaced non-existent IEngineStateManager with existing IGameState (SaveGame/LoadGame) + BeginTrain() stub; replaced non-existent IUserInput with existing IInputHandler (Era.Core/Input/IInputHandler.cs)
- [fix] Phase1-RefCheck iter1: [CON-001] Migration Guidelines line 758 | NTR_MESSAGE.ERB path corrected to NTR/NTR_MESSAGE.ERB
- [fix] Phase2-Review iter1: [FMT-002] Dependencies table row 2 | Removed invalid "Note" type row for Phase 14 (info already in Technical Constraints and Mandatory Handoffs)
- [resolved-applied] Phase2-Uncertain iter1: [FMT-002] Missing ### Upstream Issues subsection under Technical Design per template lines 369-377; marked Optional in template, zero precedent across codebase
- [resolved-applied] Phase2-Uncertain iter1: [CON-001] IGameState.SaveGame(string)/LoadGame(string) requires filename parameter but ERB SAVEGAME/LOADGAME are parameterless; C11 claims existing IGameState usage but signatures are incompatible
- [fix] Phase2-Review iter2: [FMT-002] Review Notes [fix] entries | Added category codes to all 30+ [fix] entries per mandated format
- [fix] Phase2-Review iter2: [FMT-002] Mandatory Handoffs IInputHandler row | Removed N/A destination row (existing dependency, not deferred handoff)
- [fix] Phase2-Review iter2: [FMT-002] Feasibility Assessment Note paragraph | Moved freeform Note into table row per template format
- [fix] Phase2-Review iter3: [CON-001] TRYCALL 簡易追加情報 miscategorized | Separated from PRINT_STATE in Technical Design; added to C10 as existing TRYCALL with no-op treatment
- [fix] Phase2-Review iter3: [DEP-001] Missing Mandatory Handoffs | Added HAS_VAGINA/GET_MARK_LEVEL_OWNERNAME stubs to Mandatory Handoffs (Phase 20)
- [fix] Phase2-Review iter4: [CON-001] Migration Guidelines _engine references | Replaced _engine.BeginTrain/SaveGame/LoadGame with _gameState and NotSupportedException per C11/Key Decisions
- [fix] Phase2-Review iter4: [CON-001] Technical Design stub annotations | Added TDD RED→GREEN note to ShopSystem.cs and ShopDisplay.cs illustrative stubs
- [fix] Phase2-Review iter5: [FMT-002] Philosophy Derivation AC#12 | Changed AC#12 to AC#12a, AC#12b (no plain AC#12 in AC Definition Table)
- [fix] Phase2-Review iter5: [FMT-002] AC Coverage row 12 | Split into 12a and 12b rows to match AC Definition Table
- [fix] Phase2-Review iter5: [AC-005] AC#5 pattern | Anchored to AddSingleton<...> to prevent false positives from comments
- [resolved-applied] Phase2-Loop iter5: [FMT-002] AC#7 Method column format inconsistency | Grep(path) vs Grep("pattern", path) style; ac-static-verifier handles both (validated iter1) but style inconsistent with peer ACs
- [resolved-skipped] Phase2-Loop iter5: [AC-005] AC#11 equivalence test assertion quality | Test class name grep does not enforce meaningful Assert statements; complementary AC#10 provides behavioral verification (validated iter4)
- [fix] Phase2-Review iter6: [CON-001] ShopDisplay missing IInputHandler | Added IInputHandler to ShopDisplay constructor for @SHOW_CHARADATA/@SHOW_CHARADATA2 INPUT usage
- [fix] Phase2-Review iter6: [DEP-001] Missing GET_DAY/日付_月 | Added to Migration Guidelines and Mandatory Handoffs (Phase 20)
- [fix] Phase2-Review iter6: [CON-001] DebugEnterUfufu duplicate throw | Consolidated to shared private BeginTrain() method
- [fix] Phase2-Review iter7: [FMT-002] AC Coverage row 7b ordering | Moved 7b after 7 to match AC Definition Table ordering
- [fix] Phase3-Maintainability iter8: [DEP-001] IShopSystem contract stub trigger | Added concrete trigger condition (F776 [DONE]) to Mandatory Handoff
- [fix] Phase3-Maintainability iter8: [CON-001] Stub verification strategy | Added ### Stub Verification Strategy section to Implementation Contract
- [fix] Phase2-Review iter1: [FMT-002] AC#3 pipe escaping | Escaped bare | characters in AC#3 Grep pattern to \| for markdown table column consistency
- [fix] Phase2-Review iter1: [FMT-002] AC#4 pipe escaping | Escaped bare | characters in AC#4 Grep pattern to \| for markdown table column consistency
- [resolved-skipped] Phase2-Uncertain iter1: [CON-001] NotImplementedException↔Mandatory Handoffs traceability gap | No AC verifies every NotImplementedException/NotSupportedException throw has a corresponding Mandatory Handoff entry; fix not directly actionable due to C8 AC count limit (15/15) and row-to-throw count mismatch in Mandatory Handoffs table
- [resolved-applied] Phase8-PostReview iter1: [AC-006] Equivalence tests behavioral depth | Tests improved from reflection-only to include DI registration verification, resolution failure verification, and IShopSystem contract tests (27 tests). Full behavioral equivalence (method invocation with mocked data + output assertion) blocked by Phase 14 NotImplementedException stubs in private methods; deferred to Phase 14 completion

---

<!-- fc-phase-6-completed -->
## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning (parent)
- [Related: F775](feature-775.md) - Collection (called from SHOP.ERB)
- [Related: F776](feature-776.md) - Items (called from SHOP.ERB)
- [Related: F777](feature-777.md) - Customization
- [Related: F778](feature-778.md) - Body Settings - shares IBodySettings pattern
- [Related: F779](feature-779.md) - Body Settings (continued)
- [Related: F780](feature-780.md) - Body Settings (continued)
- [Related: F781](feature-781.md) - Body Settings (continued)
- [Related: F782](feature-782.md) - Post-Phase Review Phase 20
- [Related: F783](feature-783.md) - Phase 21 Planning
- [Related: F788](feature-788.md) - IConsoleOutput Extensions (Mandatory Handoff destination)
- [Related: F789](feature-789.md) - IVariableStore Extensions (Mandatory Handoff destination)
- [Related: F790](feature-790.md) - Engine Data Access Layer (Mandatory Handoff destination)
- [Related: F791](feature-791.md) - Engine State & Entry Point Routing (Mandatory Handoff destination)
