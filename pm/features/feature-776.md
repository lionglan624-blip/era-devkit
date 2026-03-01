# Feature 776: Items (SHOP_ITEM.ERB + アイテム説明.ERB)

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

## Background

### Philosophy (Mid-term Vision)

Phase 20: Equipment & Shop Systems -- Complete C# migration of all shop subsystems, establishing Era.Core as the single source of truth for item purchase logic, description dispatch, and inventory management. Each sub-feature (F774-F780) migrates one ERB module with its own equivalence tests, ensuring no deferred test debt. SSOT: `designs/phases/phase-20-27-game-systems.md` Phase 20 section; F647 decomposed scope into actionable sub-features.

### Problem (Current Issue)

The `ItemBuy()` method in `ShopSystem.cs` (line 349-350) throws `NotImplementedException`, because the item purchase logic in `SHOP_ITEM.ERB` (559 lines, 3 functions) and item descriptions in `アイテム説明.ERB` (232 lines, 47 functions) have no C# equivalent. This ERB code depends on 6 engine variable arrays (ITEM, ITEMSALES, ITEMPRICE, ITEMNAME, NO:chr) and requires MONEY write access. While SetMoney was added by F775 and is now available on IEngineVariables, the item arrays still have no Era.Core interface abstraction. Additionally, the ERB uses COMMON.ERB utility functions (ITEMSTOCK, CHOICE, PRINT_BASE, HAS_VAGINA), a PRINT_ITEM engine built-in, dynamic CALLFORM dispatch to 47 description handlers, and SETCOLOR/RESETCOLOR for disabled-item coloring -- all requiring C# equivalents.

### Goal (What to Achieve)

1. Replace the `ItemBuy()` stub in `ShopSystem.cs` with a complete C# implementation of `SHOP_ITEM.ERB` logic
2. Implement all 47 item description handlers (`アイテム説明_0` through `アイテム説明_96`) as a dictionary-dispatched pattern
3. Add missing item variable access methods to new IItemVariables interface (`GetItem`/`SetItem`, `GetItemSales`/`SetItemSales`, `GetItemPrice`, `GetItemName`) and add `GetCharacterNo` to IEngineVariables (ISP: NO:(chr) is a general character property); verify SetMoney exists on IEngineVariables (added by F775)
4. Inline COMMON.ERB utility functions (ITEMSTOCK, CHOICE, PRINT_BASE) as private C# methods; use existing ICommonFunctions.HasVagina for HAS_VAGINA
5. Replicate PRINT_ITEM engine built-in behavior, SHOW_ITEM grid display with SETCOLOR/RESETCOLOR, and ITEM_SALES conditional availability
6. Include equivalence tests and zero technical debt (no TODO/FIXME/HACK)
7. Implement [200] item reset handler that resets all ITEMSALES:0-100 to 0 after CHOICE confirmation dialog

<!-- Sub-Feature Requirements (architecture.md:4629-4637): /fc時に以下を反映すること
  1. Philosophy継承: Phase 20: Equipment & Shop Systems
  2. Tasks: 負債解消 (TODO/FIXME/HACKコメント削除タスクを含む)
  3. Tasks: 等価性検証 (legacy実装との等価性テストを含む)
  4. AC: 負債ゼロ (技術負債ゼロを検証するACを含む)
-->

<!-- F788 Mandatory Handoffs (受け入れ側):
  - F788 Task 10: F776 Dependencies に F788 を Predecessor として追加すること
-->

### Source Files

> **Note**: Non-template subsection retained under Background for source traceability.

| File | Lines | Functions | Note |
|------|------:|-----------|------|
| Game/ERB/SHOP_ITEM.ERB | 559 | 3 | @ITEM_BUY, @SHOW_ITEM, @ITEM_SALES |
| Game/ERB/アイテム説明.ERB | 232 | 47 | All @アイテム説明_{N} pattern |

---

<!-- fc-phase-1-completed -->
<!-- fc-phase-2-completed -->
## Root Cause Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

### 5 Whys

| Level | Question | Answer | Evidence |
|:-----:|----------|--------|----------|
| 1 | Why does ItemBuy() throw NotImplementedException? | Because F774 (Shop Core) created a stub for ITEM_BUY, deferring full implementation to F776 | `Era.Core/Shop/ShopSystem.cs:349-350` |
| 2 | Why can't the stub simply be replaced with ERB-equivalent C# logic? | Because SHOP_ITEM.ERB reads/writes 5 engine item arrays (ITEM, ITEMSALES, ITEMPRICE, ITEMNAME, MONEY) and uses NO:(chr), none of which have Era.Core interface abstractions | `Game/ERB/SHOP_ITEM.ERB:6,9,240,271` + `Era.Core/Interfaces/IEngineVariables.cs` (no item methods) |
| 3 | Why are item variable interfaces missing from Era.Core? | Because F790 (Engine Data Access Layer) designed IEngineVariables with read-only scalar access only (GetMoney, GetMaster, etc.), following ISP -- item arrays were out of F790's scope. SetMoney was added by F775 as an extension. | `Era.Core/Interfaces/IEngineVariables.cs:17-20` (GetMoney + SetMoney as extension) |
| 4 | Why didn't F790 include item arrays or write access? | Because no Phase 20 consumer at F790's design time required item variable mutation; F790 targeted scalar engine variables used by SHOP.ERB (the coordinator), not SHOP_ITEM.ERB | F790 feature scope: IEngineVariables base |
| 5 | Why (Root)? | The Phase 20 decomposition (F647) created sub-features incrementally, with each sub-feature extending interfaces as needed. F776 is the first consumer requiring item array access and MONEY write, exposing gaps that are by-design deferred to consuming features | `Era.Core/Variables/VariableCode.cs:62,122,172,285` (ITEM/ITEMSALES/ITEMPRICE/ITEMNAME exist as VariableCode entries but have no Era.Core interface) |

### Symptom vs Root Cause

| Aspect | Symptom | Root Cause |
|--------|---------|------------|
| What | `ItemBuy()` throws NotImplementedException when called from ShopSystem | Era.Core lacks item variable interfaces (ITEM, ITEMSALES, ITEMPRICE, ITEMNAME) while SetMoney was added by F775; IEngineVariables was originally designed read-only per ISP |
| Where | `ShopSystem.cs:349-350` (stub method) | `IEngineVariables.cs` (missing SetMoney), no `IItemVariables` interface exists anywhere in Era.Core |
| Fix | Remove stub, inline hardcoded logic | Add SetMoney to IEngineVariables, create new item variable interface(s), implement ItemBuy with full ERB-equivalent logic using DI-injected interfaces |

## Related Features
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Feature | Status | Relationship |
|---------|--------|--------------|
| F647 | [DONE] | Phase 20 Planning -- decomposed this feature from architecture.md |
| F774 | [DONE] | Shop Core -- created ShopSystem with ItemBuy() stub that F776 replaces |
| F775 | [DONE] | Collection (SHOP_COLLECTION.ERB) -- sibling; already added SetMoney to IEngineVariables; zero cross-calls with F776 |
| F777 | [PROPOSED] | Customization (SHOP_CUSTOM.ERB) -- sibling; zero cross-calls with F776 |
| F788 | [DONE] | IConsoleOutput Extensions -- provides DrawLine, PrintWait, Bar used by item display |
| F789 | [DONE] | IStringVariables -- provides string variable access (SAVESTR etc.) |
| F790 | [DONE] | IEngineVariables base -- provides GetMoney, GetMaster, GetCharaNum, GetRandom; needs SetMoney extension |
| F791 | [DONE] | IGameState mode transitions -- provides game state management |
| F793 | [DONE] | GameStateImpl delegation -- engine-side implementation of IGameState |

## Feasibility Assessment
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Criterion | Assessment | Evidence |
|-----------|:----------:|----------|
| Interface availability | FEASIBLE | IEngineVariables now has SetMoney (added by F775); only item variable interfaces need to be created. New IItemVariables interface fills remaining gap. |
| Existing infrastructure | FEASIBLE | ShopSystem.cs, ShopDisplay.cs, IConsoleOutput, IVariableStore, IStyleManager, ICommonFunctions all exist and are injected via DI |
| ERB complexity | FEASIBLE | SHOP_ITEM.ERB is procedural with clear control flow; 47 description handlers are simple PRINTL statements; COMMON.ERB utilities (ITEMSTOCK 15 lines, CHOICE 16 lines) are small enough to inline |
| Volume | NEEDS_REVISION | Source ERB totals 791 lines; C# output will likely exceed 300-line engine type limit. Volume waiver required with justification (atomicity: item purchase + descriptions are tightly coupled) |
| Dynamic dispatch | FEASIBLE | CALLFORM アイテム説明_{ITEM_NO} maps to Dictionary<int, Action> or switch pattern; 47 handlers are enumerable |
| Engine built-in (PRINT_ITEM) | FEASIBLE | PrintItemCommand.cs exists in engine; C# equivalent can call IConsoleOutput or replicate display logic |

**Verdict**: NEEDS_REVISION -- Interface gaps (SetMoney, item arrays, NO:chr) are resolvable within scope. Volume waiver needed for 791-line source migration requiring atomicity.

## Impact Analysis
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Area | Impact | Description |
|------|:------:|-------------|
| ShopSystem.cs | HIGH | ItemBuy() stub replaced with full implementation; constructor may gain new dependencies (item variable interface) |
| IEngineVariables | MEDIUM | Adding SetMoney(int) method; all implementors (EngineVariables.cs, NullEngineVariables.cs, test mocks) must be updated |
| New item interface | HIGH | New interface(s) for ITEM/ITEMSALES/ITEMPRICE/ITEMNAME/NO access; new stub implementations; DI registration |
| Era.Core/Shop/ | HIGH | New file(s) for item purchase logic, item descriptions, ITEMSTOCK utility |
| engine/ integration | MEDIUM | Engine-side implementations for new interfaces (delegating to GlobalStatic variable access) |
| F775 (sibling) | LOW | F775 also plans SetMoney; whichever feature is implemented first adds it. Zero cross-calls between F775 and F776 |

## Technical Constraints
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Constraint | Source | Impact |
|------------|--------|--------|
| Era.Core cannot reference engine layer | Architecture rule (ENGINE.md Issue 29) | Item variable implementations must be created in engine/ project with DI registration; Era.Core keeps stubs |
| IEngineVariables ISP design | F790 design decision | Adding SetMoney changes read-only contract; may require documenting mixed read/write contract or ISP split |
| PRINT_ITEM is engine built-in | PrintItemCommand.cs in engine | Cannot directly call engine command from Era.Core; must replicate display behavior via IConsoleOutput |
| CALLFORM dynamic dispatch | SHOP_ITEM.ERB:43 | Must map to C# dictionary or switch dispatch; 47 handlers require enumeration |
| ERB RESTART semantics | SHOP_ITEM.ERB multiple RESTART calls | Maps to while(true) loop with continue in C# |
| SIF scope (items 91-92) | SHOP_ITEM.ERB:246-248, 279-281 | SIF only affects the immediately next statement (RESTART). Lines 248/281 ("誰に使いますか？") are NOT dead code — they execute on the confirm path (RESULT=0). C# migration MUST include these PRINTL statements. |
| GETBIT engine function | SHOP_ITEM.ERB:254,262 | Need C# equivalent for bitwise talent checking |
| TRYCALL 排卵誘発剤追加処理 | SHOP_ITEM.ERB:273 | Optional external call to 生理機能追加パッチ.ERB; use C10 no-op stub pattern |
| TreatWarningsAsErrors | Directory.Build.props | All new C# code must compile without warnings |
| Debug print exclusion | SHOP_ITEM.ERB:6 `PRINTFORML {ITEMSALES:62}` | Debug/diagnostic output printing raw ITEMSALES value for item 62; not part of user-facing shop logic. Excluded from C# migration. |

## Risks
<!-- Written by: consensus-synthesizer (Phase 1) -->

| Risk | Likelihood | Impact | Mitigation |
|------|:----------:|:------:|------------|
| SetMoney contention with F775 | RESOLVED | LOW | F775 has already added SetMoney to IEngineVariables. F776 will use this existing implementation. Zero conflict. |
| Volume exceeds 300-line limit | HIGH | MEDIUM | Document volume waiver in Review Notes; justify atomicity (item purchase + descriptions are tightly coupled, splitting would create artificial boundaries) |
| Interface proliferation | MEDIUM | MEDIUM | Consider extending IEngineVariables vs creating new IItemVariables; follow ISP but avoid over-fragmentation |
| 47 description handlers correctness | MEDIUM | LOW | Each handler is a simple PRINTL; dictionary dispatch verified by equivalence tests |
| Per-item special logic (items 60-96) | MEDIUM | HIGH | Items 60-96 have complex talent modifications, character selection loops, and conditional logic; thorough testing required |
| GETBIT function unavailability | LOW | MEDIUM | Verify GETBIT equivalent exists in Era.Core or implement bitwise helper |
| Engine-side implementation complexity | MEDIUM | MEDIUM | New interfaces require engine-side GlobalStatic delegation; follow F790/F793 patterns |

---

## Baseline Measurement

<!-- Generated by consensus-synthesizer (Phase 1). -->

| Metric | Command | Baseline Value | Note |
|--------|---------|----------------|------|
| ItemBuy stub | Grep "NotImplementedException.*ITEM_BUY" Era.Core/Shop/ShopSystem.cs | 1 match | Stub to be replaced |
| IEngineVariables method count | Grep "^\s+(int\|string)\s+Get" Era.Core/Interfaces/IEngineVariables.cs | 11 methods | Existing read-only methods to preserve |
| Item description functions | Grep "@アイテム説明_" Game/ERB/アイテム説明.ERB | 47 functions | All must be migrated |
| SHOP_ITEM functions | Grep "^@" Game/ERB/SHOP_ITEM.ERB | 3 functions | @ITEM_BUY, @SHOW_ITEM, @ITEM_SALES |

**Baseline File**: `.tmp/baseline-776.txt`

---

## AC Design Constraints

<!-- MANDATORY: ac-designer MUST read this section before designing ACs -->

| ID | Constraint | Source | AC Implication |
|:--:|------------|--------|----------------|
| C1 | ItemBuy() stub replaced | ShopSystem.cs:349-350 | AC must verify not_matches NotImplementedException for ITEM_BUY |
| C2 | MONEY deduction via SetMoney | SHOP_ITEM.ERB lines 9,240,271 (13+ MONEY -= occurrences) | AC must verify SetMoney exists on IEngineVariables and is called in item purchase logic |
| C3 | Item array interfaces accessible | SHOP_ITEM.ERB + COMMON.ERB (ITEM, ITEMSALES, ITEMPRICE, ITEMNAME) | AC must verify interface methods exist for all 4 item arrays (get+set where applicable) |
| C4 | CALLFORM dispatch to 47 descriptions | SHOP_ITEM.ERB:43 `CALLFORM アイテム説明_{ITEM_NO}` | AC must verify dispatch mechanism covers all item IDs (0-96 range, 47 handlers) |
| C5 | ITEMSTOCK logic preserved | COMMON.ERB:12-26 (return values 0-5) | AC must verify all 6 return paths (0=available, 1=nonexistent, 2=sold out, 3=insufficient money, 4=condition unmet, 5=max stock) |
| C6 | CHOICE UI replicated | SHOP_ITEM.ERB (16 CALL CHOICE instances) | AC must verify choice/confirmation dialog method exists and is callable |
| C7 | Per-item talent modifications (items 41, 60-96) | SHOP_ITEM.ERB:48-420 | AC must verify individual item handlers for direct purchase (41), skill learning (60-62), ABL modification (63-64), character-targeted items (91-96) |
| C8 | Bulk purchase logic (money cap + mutually exclusive stock/unlimited branches) | SHOP_ITEM.ERB:422-458 | AC must verify: (1) money cap always applied first (RESULT * ITEMPRICE > MONEY), (2) IF ITEMSALES nonzero (limited stock): cap RESULT by ITEMSALES, adjust ITEMSALES, do NOT add to ITEM; (3) ELSE (unlimited): add RESULT to ITEM, cap at 999 |
| C9 | 3-column grid + disabled coloring | SHOP_ITEM.ERB:472-499 | AC must verify layout rendering and SETCOLOR/RESETCOLOR for unavailable items |
| C10 | ITEM_SALES conditional availability | SHOP_ITEM.ERB:502-559 | AC must verify talent-gated item availability logic |
| C11 | Zero technical debt | Phase 20 sub-feature requirement | AC must verify not_matches TODO/FIXME/HACK across all new files |
| C12 | Equivalence tests | Phase 20 sub-feature requirement | AC must verify test class existence for item purchase logic |
| C13 | PRINT_ITEM equivalent | SHOP_ITEM.ERB:8 | AC must verify C# method replicates engine PRINT_ITEM built-in behavior |
| C14 | Empty description handling | アイテム説明_45 (empty function for condom) | AC must verify graceful no-op for items with empty descriptions |
| C15 | RESTART/GOTO as while(true) | SHOP_ITEM.ERB control flow | AC must verify loop structure correctness (no infinite loop bugs, proper break/continue) |
| C16 | NO:(chr) character variable | SHOP_ITEM.ERB:250,283 | AC must verify GetCharacterNo on IEngineVariables (moved from IItemVariables per ISP) |
| C17 | SETCOLOR/RESETCOLOR | SHOP_ITEM.ERB:488-494 | AC must verify IStyleManager injection and color state management |
| C18 | Existing IEngineVariables methods preserved | IEngineVariables.cs (11 methods) | AC must verify count_equals for existing methods after SetMoney addition (backward compatibility) |
| C19 | [200] item reset handler | SHOP_ITEM.ERB:18-26 | AC must verify item reset method exists that resets ITEMSALES:0-100 to 0 |
| C20 | Single-item purchase path (CASE 0 TO 39) | SHOP_ITEM.ERB:459-470 | AC must verify single-item purchase logic: ITEM++, ITEMSALES--, MONEY deduction for items 0-39 |

### Constraint Details

**C1: ItemBuy Stub Replacement**
- **Source**: Investigation of ShopSystem.cs:349-350 found `throw new NotImplementedException("ITEM_BUY - See F776")`
- **Verification**: Grep for NotImplementedException with ITEM_BUY in ShopSystem.cs
- **AC Impact**: Negative AC (not_matches) to verify stub removal, plus positive AC to verify replacement calls item purchase logic

**C2: SetMoney Interface Extension**
- **Source**: Interface dependency scan -- SetMoney was added by F775 to IEngineVariables.cs; SHOP_ITEM.ERB performs `MONEY -= ITEMPRICE:ITEM_NO` in 13+ locations. F776 must verify and use SetMoney for item purchase money deductions.
- **Verification**: Grep for SetMoney call in Era.Core/Shop/ (positive verification that F776 uses it, not just that it exists)
- **AC Impact**: AC#12 verifies SetMoney is called in item purchase implementation. AC#3 (pre-satisfied) confirms SetMoney exists on interface (added by F775)

**C3: Item Variable Interfaces**
- **Source**: Interface dependency scan -- VariableCode.cs defines ITEM(line 62), ITEMSALES(122), ITEMPRICE(172), ITEMNAME(285) as engine variables, but no Era.Core interface exists
- **Verification**: Grep for item variable methods in Era.Core/Interfaces/
- **AC Impact**: Must verify both getter and setter (where applicable: ITEM and ITEMSALES are read/write; ITEMPRICE and ITEMNAME are read-only per `__UNCHANGEABLE__` flag)

**C4: Description Dispatch**
- **Source**: SHOP_ITEM.ERB:43 uses `CALLFORM アイテム説明_{ITEM_NO}` to dynamically call 47 functions in アイテム説明.ERB
- **Verification**: Count description handler registrations in C# dispatch dictionary/switch
- **AC Impact**: AC should use count_equals to verify all 47 handlers are registered

**C5: ITEMSTOCK Logic**
- **Source**: COMMON.ERB:12-26 defines ITEMSTOCK function with 7 code paths returning 6 distinct values. Two paths return 1 (nonexistent): (a) ARG >= 100, (b) !STRLENS(ITEMNAME:ARG) (item name is empty string). Other thresholds: ITEMSALES==-2 → 4 (condition unmet), ITEMSALES==-1 → 2 (sold out), MONEY<ITEMPRICE → 3 (insufficient), ITEM:ARG >= 99 → 5 (max stock), else → 0 (available). Note: max stock threshold is **99** (not 999; 999 is the separate bulk purchase hard cap at SHOP_ITEM.ERB:451).
- **Verification**: Unit test covering all 6 return values with both paths to return 1 tested separately (ARG>=100 and empty ITEMNAME)
- **AC Impact**: Each return path (0-5) should be exercised in unit tests. Must test both nonexistent paths: ARG >= 100 AND ITEMNAME empty. Must use 99 for max stock check, not 999.

**C6: CHOICE UI replicated**
- **Source**: SHOP_ITEM.ERB uses 16 CALL CHOICE instances for purchase confirmation and quantity selection
- **Verification**: Grep for Choice method in Era.Core/Shop/ItemPurchase.cs
- **AC Impact**: AC#24 verifies Choice() method exists in ItemPurchase.cs

**C7: Per-Item Special Logic**
- **Source**: SHOP_ITEM.ERB:48-413 contains item-specific handlers with talent checks, character loops, and MONEY deduction
- **Verification**: Unit tests for representative items from each category
- **AC Impact**: At minimum, test skill items (60-62), futanari items (63-64), and character-targeted items (90-96)

**C8: Bulk Purchase Logic (Mutually Exclusive Branches)**
- **Source**: SHOP_ITEM.ERB:439-455. Money cap applied unconditionally first (line 439-440). Then IF/ELSE branch: (A) ITEMSALES nonzero (limited stock, line 442-448): cap RESULT by ITEMSALES, adjust/exhaust ITEMSALES, do NOT add to ITEM array; (B) ITEMSALES zero (unlimited, line 449-454): add RESULT to ITEM, cap ITEM at 999, adjust RESULT retroactively.
- **Verification**: Unit tests with boundary values covering both branches separately
- **AC Impact**: Must test (A) limited-stock branch where ITEM is NOT modified, and (B) unlimited branch where ITEM is incremented and 999-capped. Money cap applies to both branches.

**C9: 3-column grid + disabled coloring**
- **Source**: SHOP_ITEM.ERB:472-499 implements @SHOW_ITEM with 3-column layout, 28-char padding, and SETCOLOR/RESETCOLOR
- **Verification**: Grep for DisplayItemGrid and SetColor/ResetColor in Era.Core/Shop/
- **AC Impact**: AC#23 verifies DisplayItemGrid method exists; AC#15 verifies SetColor/ResetColor usage

**C10: ITEM_SALES conditional availability**
- **Source**: SHOP_ITEM.ERB:502-559 implements @ITEM_SALES with talent-gated item availability logic
- **Verification**: Grep for UpdateItemAvailability in Era.Core/Shop/
- **AC Impact**: AC#16 verifies UpdateItemAvailability method exists

**C11: Zero technical debt**
- **Source**: Phase 20 sub-feature requirement mandates no deferred technical debt
- **Verification**: Grep for TODO/FIXME/HACK across all new files
- **AC Impact**: AC#14 verifies not_matches TODO|FIXME|HACK in Era.Core/Shop/ and Era.Core/Interfaces/IItemVariables.cs

**C12: Equivalence tests**
- **Source**: Phase 20 sub-feature requirement mandates each module has its own equivalence tests
- **Verification**: Glob for test file existence + dotnet test execution
- **AC Impact**: AC#18 verifies ItemPurchaseTests.cs exists; AC#13 verifies all tests pass

**C13: PRINT_ITEM equivalent**
- **Source**: SHOP_ITEM.ERB:8 calls PRINT_ITEM engine built-in; engine implements via GetHavingItemsString() in VariableEvaluator.cs:989-1011
- **Verification**: Grep for PrintItemCommand in Era.Core/Shop/
- **AC Impact**: AC#19 verifies PrintItemCommand method exists

**C14: Empty description handling**
- **Source**: アイテム説明.ERB contains empty @アイテム説明_45 function (item 45 = condom, no description)
- **Verification**: Verify dictionary entry for item 45 exists as no-op lambda
- **AC Impact**: Covered by AC#5 (47 handler count includes item 45) and AC#13 (unit test for empty description)

**C15: RESTART/GOTO as while(true)**
- **Source**: SHOP_ITEM.ERB uses multiple RESTART calls that map to loop continue, and GOTO for input validation loops
- **Verification**: Code review of while(true) loop structure in ItemPurchase.Execute()
- **AC Impact**: Covered by AC#13 (unit tests verify loop correctness including break/continue behavior)

**C16: Character NO Access**
- **Source**: SHOP_ITEM.ERB:250,283 uses `NO:(LOOP_CHR)` to get character CSV number by runtime index
- **Verification**: Interface method exists for GetCharacterNo(int characterIndex)
- **AC Impact**: New interface method or extension to existing interface

**C17: SETCOLOR/RESETCOLOR**
- **Source**: SHOP_ITEM.ERB:488-494 uses SETCOLOR for unavailable items (sold out, max stock) and RESETCOLOR after
- **Verification**: Grep for SetColor/ResetColor in Era.Core/Shop/
- **AC Impact**: AC#15 verifies SetColor|ResetColor usage in Era.Core/Shop/

**C18: IEngineVariables Backward Compatibility**
- **Source**: ENGINE.md Issue 63 -- extending interface without verifying existing methods remain unchanged
- **Verification**: count_equals for existing Get* method signatures after SetMoney addition
- **AC Impact**: Prevents accidental removal/modification of existing 11 methods

**C19: [200] Item Reset Handler**
- **Source**: SHOP_ITEM.ERB:18-26: ELSEIF RESULT == 200 → CHOICE confirmation → FOR loop resetting ITEMSALES:0-100 = 0 → RESTART
- **Verification**: Grep for reset method in Era.Core/Shop/
- **AC Impact**: AC must verify method exists for resetting all item availability flags

**C20: Single-Item Purchase Path (CASE 0 TO 39)**
- **Source**: SHOP_ITEM.ERB:459-470: CASE 0 TO 39 → CHOICE confirmation → ITEM:ITEM_NO++, ITEMSALES:ITEM_NO--, MONEY -= ITEMPRICE:ITEM_NO → RESTART
- **Verification**: Unit tests covering single-item purchase behavior (ITEM increment, ITEMSALES decrement, MONEY deduction)
- **AC Impact**: Covered by AC#13 unit tests (Task 11 test case 6)

---

## Dependencies

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

| Type | Feature | Status | Description |
|------|---------|--------|-------------|
| Predecessor | F647 | [DONE] | Phase 20 Planning (decomposed this feature) |
| Predecessor | F774 | [DONE] | Shop Core -- SHOP.ERB @USERSHOP calls ITEM_BUY (this feature's entry point); ShopSystem.cs provides DI infrastructure |
| Predecessor | F788 | [DONE] | IConsoleOutput Extensions (DrawLine, PrintWait, Bar etc.) |
| Predecessor | F790 | [DONE] | IEngineVariables base (GetMoney, GetMaster, GetCharaNum, GetRandom); SetMoney added by F775 |
| Related | F775 | [DONE] | Collection (SHOP_COLLECTION.ERB) -- sibling; already added SetMoney to IEngineVariables; zero cross-calls |
| Related | F777 | [DONE] | Customization (SHOP_CUSTOM.ERB) -- sibling; zero cross-calls |
| Related | F789 | [DONE] | IStringVariables (string variable access) |
| Related | F791 | [DONE] | IGameState mode transitions |
| Related | F793 | [DONE] | GameStateImpl engine-side delegation pattern (reference for engine-side implementations) |
| Related | F780 | [PROPOSED] | Genetics & Growth (Phase 20 sibling) |

<!-- fc-phase-3-completed -->

---

## Acceptance Criteria
<!-- Written by: ac-designer (Phase 3) -->

### Philosophy Derivation

| Absolute Claim | Derived Requirement | AC Coverage |
|----------------|---------------------|-------------|
| "Complete C# migration of **all** shop subsystems" | ItemBuy stub replaced with full implementation; all 47 description handlers migrated; all purchase paths (single-item, bulk, per-item special), item reset, display grid, and availability logic migrated; DI infrastructure for variable access; inlined utilities | AC#1, AC#2, AC#5, AC#12, AC#13, AC#15, AC#16, AC#17, AC#19, AC#20, AC#21, AC#23, AC#24, AC#27, AC#28, AC#45 |
| "Era.Core as the **single source of truth** for item purchase logic, description dispatch, and inventory management" | New interfaces for item variables + SetMoney on IEngineVariables + ITEM_SALES availability logic + description dispatch dictionary + inventory read/write (GetItem/SetItem) + inventory write verification (SetItem/SetItemSales called) + character NO access + price/name reads (GetItemPrice/GetItemName) + behavioral delegation verification (GetItem called in InventoryManager) + AddItem delegation verification (SetItem called in InventoryManager) | AC#3, AC#4, AC#5, AC#6, AC#7, AC#8, AC#9, AC#10, AC#11, AC#16, AC#22, AC#25, AC#26, AC#29, AC#30, AC#31, AC#43, AC#44 |
| "each sub-feature migrates one ERB module with its own equivalence tests" | Equivalence test class verifying item purchase logic | AC#13, AC#18 |
| "ensuring **no** deferred test debt" | Zero TODO/FIXME/HACK across all new files | AC#14 |

### AC Definition Table

| AC# | Description | Type | Method | Matcher | Expected | Status |
|:---:|-------------|------|--------|---------|----------|:------:|
| 1 | ItemBuy stub removed (Neg) | code | Grep(Era.Core/Shop/ShopSystem.cs) | not_matches | `NotImplementedException.*ITEM_BUY` | [x] |
| 2 | ItemBuy delegates to ItemPurchase (Pos) | code | Grep(Era.Core/Shop/ShopSystem.cs) | matches | `_itemPurchase\.Execute\(` | [x] |
| 3 | SetMoney declared on IEngineVariables (pre-satisfied: added by F775) | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | `void SetMoney\(int` | [x] |
| 4 | IEngineVariables Get methods total 12 (11 existing + GetCharacterNo added) | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | count_equals | `(int\|string)\s+Get\w+\(` = 12 | [x] |
| 5 | 47 description handlers registered | code | Grep(Era.Core/Shop/ItemDescriptions.cs) | count_equals | `\[\d+\]\s*=\s*\(` = 47 | [x] |
| 6 | IItemVariables interface file exists | file | Glob(Era.Core/Interfaces/IItemVariables.cs) | exists | Era.Core/Interfaces/IItemVariables.cs | [x] |
| 7 | GetItem declared on IItemVariables | code | Grep(Era.Core/Interfaces/IItemVariables.cs) | matches | `int GetItem\(int` | [x] |
| 8 | SetItem declared on IItemVariables | code | Grep(Era.Core/Interfaces/IItemVariables.cs) | matches | `void SetItem\(int` | [x] |
| 9 | GetItemSales and SetItemSales declared | code | Grep(Era.Core/Interfaces/IItemVariables.cs) | count_equals | `(Get|Set)ItemSales\(` = 2 | [x] |
| 10 | GetItemPrice and GetItemName declared (read-only) | code | Grep(Era.Core/Interfaces/IItemVariables.cs) | count_equals | `Get(ItemPrice|ItemName)\(` = 2 | [x] |
| 11 | GetCharacterNo declared on IEngineVariables | code | Grep(Era.Core/Interfaces/IEngineVariables.cs) | matches | `int GetCharacterNo\(int` | [x] |
| 12 | SetMoney called in item purchase logic | code | Grep(Era.Core/Shop/) | matches | `SetMoney\(` | [x] |
| 13 | Unit tests pass | test | dotnet test Era.Core.Tests | succeeds | - | [x] |
| 14 | Zero technical debt in new files | code | Grep(Era.Core/Shop/,Era.Core/Interfaces/) | not_matches | `TODO\|FIXME\|HACK` | [x] |
| 15 | IStyleManager used for disabled-item coloring | code | Grep(Era.Core/Shop/) | matches | `SetColor\|ResetColor` | [x] |
| 16 | UpdateItemAvailability method exists (@ITEM_SALES equivalent) | code | Grep(Era.Core/Shop/) | matches | `UpdateItemAvailability` | [x] |
| 17 | HasVagina called in item purchase logic | code | Grep(Era.Core/Shop/) | matches | `HasVagina\(` | [x] |
| 18 | ItemPurchaseTests.cs exists | file | Glob(Era.Core.Tests/Shop/ItemPurchaseTests.cs) | exists | Era.Core.Tests/Shop/ItemPurchaseTests.cs | [x] |
| 19 | PrintItemCommand method exists (PRINT_ITEM equivalent) | code | Grep(Era.Core/Shop/) | matches | `PrintItemCommand` | [x] |
| 20 | ResetAllItemSales method exists ([200] handler) | code | Grep(Era.Core/Shop/) | matches | `ResetAllItemSales` | [x] |
| 21 | IVariableStore injected into ItemPurchase | code | Grep(Era.Core/Shop/ItemPurchase.cs) | matches | `IVariableStore` | [x] |
| 22 | IEngineVariables injected into ItemDescriptions | code | Grep(Era.Core/Shop/ItemDescriptions.cs) | matches | `IEngineVariables` | [x] |
| 23 | DisplayItemGrid method exists (@SHOW_ITEM equivalent) | code | Grep(Era.Core/Shop/) | matches | `DisplayItemGrid` | [x] |
| 24 | Choice confirmation method exists (CHOICE utility equivalent) | code | Grep(Era.Core/Shop/ItemPurchase.cs) | matches | `Choice\(` | [x] |
| 25 | SetItem called in item purchase logic (ITEM writes via interface) | code | Grep(Era.Core/Shop/) | matches | `SetItem\(` | [x] |
| 26 | SetItemSales called in item purchase logic (ITEMSALES writes via interface) | code | Grep(Era.Core/Shop/) | matches | `SetItemSales\(` | [x] |
| 27 | ItemStock method exists (ITEMSTOCK utility equivalent) | code | Grep(Era.Core/Shop/ItemPurchase.cs) | matches | `ItemStock\(` | [x] |
| 28 | IInputHandler injected into ItemPurchase | code | Grep(Era.Core/Shop/ItemPurchase.cs) | matches | `IInputHandler` | [x] |
| 29 | GetItem called in item purchase logic (ITEM reads via interface) | code | Grep(Era.Core/Shop/) | matches | `GetItem\(` | [x] |
| 30 | GetItemPrice called in purchase logic (ITEMPRICE reads via interface) | code | Grep(Era.Core/Shop/) | matches | `GetItemPrice\(` | [x] |
| 31 | GetItemName called in purchase logic (ITEMNAME reads via interface) | code | Grep(Era.Core/Shop/) | matches | `GetItemName\(` | [x] |
| 32 | PrintBase method exists (PRINT_BASE utility equivalent) | code | Grep(Era.Core/Shop/ItemPurchase.cs) | matches | `PrintBase\(` | [x] |
| 33 | GetAvailableItems stub removed | code | Grep(Era.Core/Shop/ShopSystem.cs) | not_matches | `GetAvailableItems awaits F776` | [x] |
| 34 | Purchase stub removed | code | Grep(Era.Core/Shop/ShopSystem.cs) | not_matches | `Purchase awaits F776` | [x] |
| 35 | InventoryManager implementation exists | file | Glob(Era.Core/Shop/InventoryManager.cs) | exists | Era.Core/Shop/InventoryManager.cs | [x] |
| 36 | InventoryManager implements IInventoryManager | code | Grep(Era.Core/Shop/InventoryManager.cs) | matches | `class InventoryManager.*IInventoryManager` | [x] |
| 37 | IInventoryManager DI registered | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `IInventoryManager` | [x] |
| 38 | IItemVariables DI registered | code | Grep(Era.Core/DependencyInjection/ServiceCollectionExtensions.cs) | matches | `IItemVariables` | [x] |
| 39 | EngineItemVariables implements IItemVariables | code | Grep(Era.Core/,engine/) | matches | `class EngineItemVariables.*IItemVariables` | [x] |
| 40 | ShopSystem constructor accepts IItemVariables | code | Grep(Era.Core/Shop/ShopSystem.cs) | matches | `IItemVariables` | [x] |
| 41 | GetAvailableItems calls ItemStock (Pos) | code | Grep(Era.Core/Shop/ShopSystem.cs) | matches | `ItemStock\(` | [x] |
| 42 | Purchase delegates to ItemPurchase (Pos) | code | Grep(Era.Core/Shop/ShopSystem.cs) | matches | `ItemPurchase` | [x] |
| 43 | InventoryManager delegates to GetItem (behavioral verification) | code | Grep(Era.Core/Shop/InventoryManager.cs) | matches | `GetItem\(` | [x] |
| 44 | InventoryManager delegates to SetItem (AddItem behavioral verification) | code | Grep(Era.Core/Shop/InventoryManager.cs) | matches | `SetItem\(` | [x] |
| 45 | ICommonFunctions injected into ItemPurchase (Pos) | code | Grep(Era.Core/Shop/ItemPurchase.cs) | matches | `ICommonFunctions` | [x] |

### AC Details

**AC#1: ItemBuy stub removed (Neg)**
- **Test**: Grep pattern=`NotImplementedException.*ITEM_BUY` path=Era.Core/Shop/ShopSystem.cs
- **Expected**: 0 matches (stub replaced with real implementation)
- **Rationale**: Verifies C1 -- the NotImplementedException stub at ShopSystem.cs:349-350 is replaced. This is a single-line pattern (throw and ITEM_BUY are on the same line). Currently matches 1 occurrence, confirming non-vacuous test.

**AC#2: ItemBuy delegates to injected ItemPurchase (Pos)**
- **Test**: Grep pattern=`_itemPurchase\.Execute\(` path=Era.Core/Shop/ShopSystem.cs
- **Expected**: At least 1 match -- the ItemBuy() method body delegates to ItemPurchase via injected field
- **Rationale**: Verifies C1 positive side and DI injection pattern. The ItemBuy() method receives ItemPurchase via constructor injection (field `_itemPurchase`) and delegates to it via Execute(). This pattern verifies the stub body was replaced with actual field-based delegation, not manual instantiation via `new`.

**AC#3: SetMoney declared on IEngineVariables (pre-satisfied: added by F775)**
- **Test**: Grep pattern=`void SetMoney\(int` path=Era.Core/Interfaces/IEngineVariables.cs
- **Expected**: 1 match
- **Rationale**: Verifies C2 prerequisite -- SetMoney(int) exists on IEngineVariables (added by F775, confirmed at Era.Core/Interfaces/IEngineVariables.cs:20). SHOP_ITEM.ERB performs `MONEY -= ITEMPRICE:ITEM_NO` in 13+ locations requiring this write capability. This AC is pre-satisfied; F776 will use SetMoney for item purchase money deductions.

**AC#4: IEngineVariables existing methods preserved**
- **Test**: Grep pattern=`(int|string)\s+Get\w+\(` path=Era.Core/Interfaces/IEngineVariables.cs | count
- **Expected**: 12 matches (GetResult, GetMoney, GetDay, GetMaster, GetAssi, GetCount, GetCharaNum, GetRandom, GetName, GetCallName, GetIsAssi, GetCharacterNo)
- **Rationale**: Verifies C18 -- backward compatibility. After adding SetMoney and GetCharacterNo (moved from IItemVariables per ISP), all 12 Get methods must remain unchanged. Prevents accidental removal/modification per ENGINE.md Issue 63.

**AC#5: 47 description handlers registered**
- **Test**: Grep pattern=`\[\d+\]\s*=\s*\(` path=Era.Core/Shop/ItemDescriptions.cs type=cs | count
- **Expected**: 47 matches (dictionary entries mapping item IDs to lambda/delegate description handlers)
- **Rationale**: Verifies C4 -- CALLFORM dispatch to 47 descriptions. The dictionary-dispatch pattern must register exactly 47 entries corresponding to item IDs: 0-17, 19, 20, 23, 24, 27, 35, 36, 40-42, 44-46, 60-64, 70-72, 74, 77, 91-96. Pattern `[\d+] = (` targets dictionary initializer with lambda. Path narrowed to ItemDescriptions.cs (from Era.Core/Shop/) to eliminate false positives from ShopDisplay.cs array index assignments. Uses count_equals to ensure none are missing (per Issue 28).

**AC#6: IItemVariables interface file exists**
- **Test**: Glob pattern=Era.Core/Interfaces/IItemVariables.cs
- **Expected**: File exists
- **Rationale**: Verifies C3 -- new item variable interface created. This interface provides access to ITEM, ITEMSALES, ITEMPRICE, ITEMNAME arrays and character NO, which have no existing Era.Core abstraction.

**AC#7: GetItem declared on IItemVariables**
- **Test**: Grep pattern=`int GetItem\(int` path=Era.Core/Interfaces/IItemVariables.cs
- **Expected**: 1 match
- **Rationale**: Verifies C3 getter for ITEM array (read: `ITEM:ITEM_NO` in ERB). ITEM array is read/write per ERB usage (ITEM:ITEM_NO ++).

**AC#8: SetItem declared on IItemVariables**
- **Test**: Grep pattern=`void SetItem\(int` path=Era.Core/Interfaces/IItemVariables.cs
- **Expected**: 1 match
- **Rationale**: Verifies C3 setter for ITEM array (write: `ITEM:ITEM_NO ++` in ERB). Per Issue 65, every getter must have a corresponding setter AC when the interface defines both.

**AC#9: GetItemSales and SetItemSales declared**
- **Test**: Grep pattern=`(Get|Set)ItemSales\(` path=Era.Core/Interfaces/IItemVariables.cs | count
- **Expected**: 2 matches (one getter, one setter)
- **Rationale**: Verifies C3 -- ITEMSALES is read/write (ERB: `ITEMSALES:ITEM_NO = -1`, `ITEMSALES:ITEM_NO -= RESULT`). Both get and set required.

**AC#10: GetItemPrice and GetItemName declared (read-only)**
- **Test**: Grep pattern=`Get(ItemPrice|ItemName)\(` path=Era.Core/Interfaces/IItemVariables.cs | count
- **Expected**: 2 matches
- **Rationale**: Verifies C3 -- ITEMPRICE and ITEMNAME are read-only per `__UNCHANGEABLE__` flag in CSV. Only getters needed (no SetItemPrice/SetItemName).

**AC#11: GetCharacterNo declared on IEngineVariables**
- **Test**: Grep pattern=`int GetCharacterNo\(int` path=Era.Core/Interfaces/IEngineVariables.cs
- **Expected**: 1 match
- **Rationale**: Verifies C16 -- SHOP_ITEM.ERB uses `NO:(LOOP_CHR)` at lines 250 and 283 to get character CSV number by runtime index. GetCharacterNo moved from IItemVariables to IEngineVariables for ISP compliance: NO:(chr) is a general character-identity property used in 20+ ERB files (not just shop context), and IEngineVariables already has 3 character-index-scoped methods (GetName, GetCallName, GetIsAssi).

**AC#12: SetMoney called in item purchase logic**
- **Test**: Grep pattern=`SetMoney\(` path=Era.Core/Shop/ type=cs
- **Expected**: At least 1 match
- **Rationale**: Verifies C2 positive side -- SetMoney is not just declared but actually called in the item purchase implementation. SHOP_ITEM.ERB performs MONEY deduction in 13+ locations; C# equivalent must call SetMoney. Prevents Issue 66 (stub replacement without actual call).

**AC#13: Unit tests pass**
- **Test**: `dotnet test Era.Core.Tests`
- **Expected**: All tests pass (exit code 0)
- **Rationale**: Verifies C5 (ITEMSTOCK logic, 6 return paths), C7 (per-item talent modifications), C8 (bulk purchase caps), C12 (equivalence tests), C14 (empty description handling), C15 (loop structure). Unit tests must cover item purchase logic, description dispatch, ITEMSTOCK return values, bulk purchase caps, and character-targeted items.

**AC#14: Zero technical debt in new files**
- **Test**: Grep pattern=`TODO|FIXME|HACK` paths=Era.Core/Shop/, Era.Core/Interfaces/
- **Expected**: 0 matches
- **Rationale**: Verifies C11 -- Phase 20 sub-feature requirement mandates zero deferred technical debt. All new and modified files must be free of TODO/FIXME/HACK markers. Uses comprehensive pattern per ENGINE.md Issue 56. Path expanded from file-specific (IItemVariables.cs) to directory-scoped (Era.Core/Interfaces/) to cover all F776-modified interface files: IItemVariables.cs, NullItemVariables.cs, IEngineVariables.cs, NullEngineVariables.cs.

**AC#15: IStyleManager used for disabled-item coloring**
- **Test**: Grep pattern=`SetColor|ResetColor` path=Era.Core/Shop/ type=cs
- **Expected**: At least 1 match
- **Rationale**: Verifies C9 and C17 -- SHOW_ITEM grid display uses SETCOLOR/RESETCOLOR for unavailable items (sold out, max stock). IStyleManager is already available via DI; the implementation must call SetColor and ResetColor for disabled-item visual feedback.

**AC#16: UpdateItemAvailability method exists (@ITEM_SALES equivalent)**
- **Test**: Grep pattern=`UpdateItemAvailability` path=Era.Core/Shop/ type=cs
- **Expected**: At least 1 match
- **Rationale**: Verifies C10 -- the @ITEM_SALES function (SHOP_ITEM.ERB:502-559) implements talent-gated and skill-locked item availability logic. The C# equivalent `UpdateItemAvailability()` method must exist in the Shop directory. This ensures the conditional availability logic (ABL conditions, ITEMSALES flag management) is migrated as a discrete method, not inlined or omitted.

**AC#17: HasVagina called in item purchase logic**
- **Test**: Grep pattern=`HasVagina\(` path=Era.Core/Shop/ type=cs
- **Expected**: At least 1 match
- **Rationale**: Verifies Goal 4 -- SHOP_ITEM.ERB:198,205 uses HAS_VAGINA for item 74 (virginity restoration). The C# implementation must call existing `ICommonFunctions.HasVagina(int)` rather than hardcoding gender checks. Static verification ensures the interface dependency is real, not bypassed.

**AC#18: ItemPurchaseTests.cs exists**
- **Test**: Glob pattern=Era.Core.Tests/Shop/ItemPurchaseTests.cs
- **Expected**: File exists
- **Rationale**: Verifies C12 -- Philosophy claim "each sub-feature migrates one ERB module with its own equivalence tests" requires a dedicated test file. Without this AC, `dotnet test` (AC#13) could vacuously pass with zero tests. File existence ensures the test infrastructure is created, while AC#13 verifies the tests within it pass.

**AC#19: PrintItemCommand method exists (PRINT_ITEM equivalent)**
- **Test**: Grep pattern=`PrintItemCommand` path=Era.Core/Shop/ type=cs
- **Expected**: At least 1 match
- **Rationale**: Verifies C13 -- AC Design Constraint C13 mandates "AC must verify C# method replicates engine PRINT_ITEM built-in behavior." SHOP_ITEM.ERB:8 calls PRINT_ITEM engine built-in to display owned item counts. The C# equivalent `PrintItemCommand()` is a private method in ItemPurchase.cs that replicates this display via IConsoleOutput. Static verification ensures the method exists in the codebase.

**AC#20: ResetAllItemSales method exists ([200] handler)**
- **Test**: Grep pattern=`ResetAllItemSales` path=Era.Core/Shop/ type=cs
- **Expected**: At least 1 match
- **Rationale**: Verifies C19 -- SHOP_ITEM.ERB:18-26 implements [200] option that resets ITEMSALES:0-100 to 0 after CHOICE confirmation. The C# equivalent `ResetAllItemSales()` method must exist. Without this AC, the item reset feature would be silently omitted, violating Philosophy claim "Complete C# migration of all shop subsystems".

**AC#21: IVariableStore injected into ItemPurchase**
- **Test**: Grep pattern=`IVariableStore` path=Era.Core/Shop/ItemPurchase.cs
- **Expected**: At least 1 match
- **Rationale**: Per-item handlers (items 60-96) perform 77+ TALENT/ABL/BASE/MAXBASE/CFLAG/FLAG operations through IVariableStore. This is the primary variable access mechanism for talent modifications (item 60-62 skill learning), ABL changes (item 63-64 技巧), BASE modifications (item 70 HP recovery), and character state operations (items 91-96). Without this AC, IVariableStore could be accidentally omitted from the constructor, causing all per-item variable operations to fail. AC#13 would catch this at runtime, but static verification prevents late discovery.

**AC#22: IEngineVariables injected into ItemDescriptions**
- **Test**: Grep pattern=`IEngineVariables` path=Era.Core/Shop/ItemDescriptions.cs
- **Expected**: At least 1 match
- **Rationale**: Items 60-64 description handlers use `_engine.GetCallName(_engine.GetMaster())` for CALLNAME:MASTER interpolation (PRINTFORML %CALLNAME:MASTER% in ERB). AC#5 only verifies dictionary entry count (47), not that IEngineVariables is injected. Without this AC, IEngineVariables could be omitted from the constructor while lambda entries still exist (null reference at runtime for items 60-64). Follows AC#21 pattern for DI verification.

**AC#23: DisplayItemGrid method exists (@SHOW_ITEM equivalent)**
- **Test**: Grep pattern=`DisplayItemGrid` path=Era.Core/Shop/ type=cs
- **Expected**: At least 1 match
- **Rationale**: Verifies the @SHOW_ITEM function (SHOP_ITEM.ERB:472-499) has a C# equivalent. @SHOW_ITEM renders the 3-column item grid with 28-char padding, ITEMSTOCK filtering (skip nonexistent and condition-unmet items), and SETCOLOR/RESETCOLOR for disabled items. AC#15 verifies the coloring aspect but not the grid display method itself. Without this AC, DisplayItemGrid could be omitted while SetColor/ResetColor exist elsewhere, leaving the grid display unimplemented. Source Files list 3 ERB functions (@ITEM_BUY, @SHOW_ITEM, @ITEM_SALES); each needs method-existence verification.

**AC#24: Choice confirmation method exists (CHOICE utility equivalent)**
- **Test**: Grep pattern=`Choice\(` path=Era.Core/Shop/ItemPurchase.cs
- **Expected**: At least 1 match
- **Rationale**: Verifies C6 -- SHOP_ITEM.ERB uses 16 CALL CHOICE instances for purchase confirmation and quantity selection dialogs. COMMON.ERB CHOICE function (16 lines) provides 2-4 option UI. The C# equivalent `Choice()` private method must exist in ItemPurchase.cs. Without this AC, the CHOICE utility could be omitted or renamed without detection, leaving all confirmation dialogs non-functional.

**AC#25: SetItem called in item purchase logic (ITEM writes via interface)**
- **Test**: Grep pattern=`SetItem\(` path=Era.Core/Shop/ type=cs
- **Expected**: At least 1 match
- **Rationale**: Verifies Philosophy claim "Era.Core as the single source of truth for inventory management." SHOP_ITEM.ERB performs `ITEM:ITEM_NO ++` and `ITEM:ITEM_NO += RESULT` for owned item counts. The C# equivalent must call `IItemVariables.SetItem()` for all ITEM array writes, not hardcode values. Parallels AC#12 (SetMoney call verification). Without this AC, ITEM writes could bypass IItemVariables, violating SSOT.

**AC#26: SetItemSales called in item purchase logic (ITEMSALES writes via interface)**
- **Test**: Grep pattern=`SetItemSales\(` path=Era.Core/Shop/ type=cs
- **Expected**: At least 1 match
- **Rationale**: Verifies Philosophy claim "Era.Core as the single source of truth for inventory management." SHOP_ITEM.ERB performs `ITEMSALES:ITEM_NO -= RESULT`, `ITEMSALES:ITEM_NO = -1`, and `ITEMSALES:N = 0` for availability flag management. The C# equivalent must call `IItemVariables.SetItemSales()` for all ITEMSALES writes. Parallels AC#12 (SetMoney) and AC#25 (SetItem). Without this AC, ITEMSALES writes could bypass IItemVariables.

**AC#27: ItemStock method exists (ITEMSTOCK utility equivalent)**
- **Test**: Grep pattern=`ItemStock\(` path=Era.Core/Shop/ItemPurchase.cs
- **Expected**: At least 1 match
- **Rationale**: Verifies C5 -- COMMON.ERB ITEMSTOCK function (15 lines, 6 return paths) is the most critical inlined utility. Called 6 times in @ITEM_BUY for pre-purchase validation and 4 times in @SHOW_ITEM for display filtering. Pattern-consistent with AC#19 (PrintItemCommand), AC#20 (ResetAllItemSales), AC#23 (DisplayItemGrid), AC#16 (UpdateItemAvailability), AC#24 (Choice). Without this AC, ItemStock could be omitted or renamed without detection, breaking all item availability checks.

**AC#28: IInputHandler injected into ItemPurchase**
- **Test**: Grep pattern=`IInputHandler` path=Era.Core/Shop/ItemPurchase.cs
- **Expected**: At least 1 match
- **Rationale**: ItemPurchase.cs depends on IInputHandler for 14+ INPUT operations including character selection loops (items 70-96), quantity selection (bulk purchase), and confirmation dialogs. Parallels AC#21 (IVariableStore) and AC#22 (IEngineVariables in ItemDescriptions) for DI verification. Without this AC, IInputHandler could be accidentally omitted from the constructor while Choice() and character selection methods still exist (null reference at runtime).

**AC#29: GetItem called in item purchase logic (ITEM reads via interface)**
- **Test**: Grep pattern=`GetItem\(` path=Era.Core/Shop/ type=cs
- **Expected**: At least 1 match
- **Rationale**: Verifies SSOT for inventory reads. AC#25 verifies SetItem writes, but without AC#29, the implementation could read item counts through a non-interface mechanism (bypassing IItemVariables.GetItem). SHOP_ITEM.ERB reads ITEM:ITEM_NO in bulk purchase logic (line 451: `ITEM:ITEM_NO + RESULT > 999`), single-item handlers, and ITEMSTOCK function. All reads must go through IItemVariables.GetItem() per SSOT principle.

**AC#30: GetItemPrice called in purchase logic (ITEMPRICE reads via interface)**
- **Test**: Grep pattern=`GetItemPrice\(` path=Era.Core/Shop/ type=cs
- **Expected**: At least 1 match
- **Rationale**: Verifies SSOT principle for ITEMPRICE reads. SHOP_ITEM.ERB reads ITEMPRICE:ITEM_NO in ITEMSTOCK checks (MONEY < ITEMPRICE:ARG), money deduction (13+ locations), bulk purchase cap (RESULT * ITEMPRICE > MONEY), and display formatting. The C# implementation must call IItemVariables.GetItemPrice() for all price reads. Parallels AC#12 (SetMoney), AC#25 (SetItem), AC#29 (GetItem).

**AC#31: GetItemName called in purchase logic (ITEMNAME reads via interface)**
- **Test**: Grep pattern=`GetItemName\(` path=Era.Core/Shop/ type=cs
- **Expected**: At least 1 match
- **Rationale**: Verifies SSOT principle for ITEMNAME reads. SHOP_ITEM.ERB reads ITEMNAME:ITEM_NO in ITEMSTOCK empty-name check (!STRLENS(ITEMNAME:ARG)), display formatting (PRINTFORML %ITEMNAME:ITEM_NO%), and description headers. The C# implementation must call IItemVariables.GetItemName() for all name reads. Parallels AC#30 (GetItemPrice).

**AC#32: PrintBase method exists (PRINT_BASE utility equivalent)**
- **Test**: Grep pattern=`PrintBase\(` path=Era.Core/Shop/ItemPurchase.cs
- **Expected**: At least 1 match
- **Rationale**: Goal 4 explicitly lists PRINT_BASE as one of three COMMON.ERB utilities to inline. ITEMSTOCK has AC#27, CHOICE has AC#24, but PRINT_BASE had no dedicated AC. PRINT_BASE (3 lines: print name, BAR call, print formatted values) is called by item 70 handler for HP display. Pattern-consistent with AC#27 (ItemStock) and AC#24 (Choice).

**AC#33: GetAvailableItems stub removed**
- **Test**: Grep pattern=`GetAvailableItems awaits F776` path=Era.Core/Shop/ShopSystem.cs
- **Expected**: 0 matches (stub replaced with real implementation)
- **Rationale**: F774 Mandatory Handoff (feature-774.md:919) defers IShopSystem.GetAvailableItems implementation to F776. ShopSystem.cs:35-36 returns Result.Fail with F776 reference. Philosophy "Complete C# migration" requires this stub replacement.

**AC#34: Purchase stub removed**
- **Test**: Grep pattern=`Purchase awaits F776` path=Era.Core/Shop/ShopSystem.cs
- **Expected**: 0 matches (stub replaced with real implementation)
- **Rationale**: F774 Mandatory Handoff (feature-774.md:919) defers IShopSystem.Purchase implementation to F776. ShopSystem.cs:38-39 returns Result.Fail with F776 reference. Philosophy "Complete C# migration" requires this stub replacement.

**AC#35: InventoryManager implementation exists**
- **Test**: Glob pattern=Era.Core/Shop/InventoryManager.cs
- **Expected**: File exists
- **Rationale**: F774 Mandatory Handoff defers IInventoryManager DI registration to F776. InventoryManager implements IInventoryManager using IItemVariables as backing store, bridging the low-level engine variable layer to the high-level domain interface. Note: IInventoryManager is not consumed within F776's own shop logic (ItemPurchase uses IItemVariables directly for performance); it fulfills F774's handoff obligation for downstream features that require high-level domain-typed inventory access.

**AC#36: InventoryManager implements IInventoryManager**
- **Test**: Grep pattern=`class InventoryManager.*IInventoryManager` path=Era.Core/Shop/InventoryManager.cs
- **Expected**: 1 match
- **Rationale**: Verifies the concrete class properly implements the interface contract defined by F774 (HasItem, AddItem with strong types). Note: ERA's ITEM array is globally scoped (ITEM:itemId, no character dimension). InventoryManager bridges the IInventoryManager strong-typed contract to IItemVariables' global array — HasItem/AddItem should delegate to GetItem/SetItem respectively, with CharacterId as a no-op parameter (ERA item ownership is implicit to the player).

**AC#37: IInventoryManager DI registered**
- **Test**: Grep pattern=`IInventoryManager` path=Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- **Expected**: At least 1 match
- **Rationale**: F774 Mandatory Handoff explicitly states "IInventoryManager DI registration deferred" (feature-774.md:919). This AC verifies the registration is completed in F776.

**AC#38: IItemVariables DI registered**
- **Test**: Grep pattern=`IItemVariables` path=Era.Core/DependencyInjection/ServiceCollectionExtensions.cs
- **Expected**: At least 1 match
- **Rationale**: Task 4 registers IItemVariables → NullItemVariables in Era.Core DI. Without a static AC, this registration could be accidentally omitted and only caught indirectly by AC#13 (dotnet test). Parallels AC#37 (IInventoryManager DI verification). Uses full path (Era.Core/DependencyInjection/) to avoid false positives from engine-side registration.

**AC#39: EngineItemVariables implements IItemVariables**
- **Test**: Grep pattern=`class EngineItemVariables.*IItemVariables` paths=Era.Core/, engine/
- **Expected**: 1 match
- **Rationale**: Task 5 creates the engine-side implementation of IItemVariables with GlobalStatic variable delegation. Without this AC, the implementation file could be missing and only caught by AC#13 (runtime test failure). Parallels existing pattern: EngineVariables implements IEngineVariables (Era.Core/Interfaces/EngineVariables.cs).

**AC#40: ShopSystem constructor accepts IItemVariables**
- **Test**: Grep pattern=`IItemVariables` path=Era.Core/Shop/ShopSystem.cs
- **Expected**: At least 1 match
- **Rationale**: Task 8 adds IItemVariables to ShopSystem constructor for pass-through to ItemPurchase. Without this AC, the dependency could be omitted from ShopSystem while ItemPurchase expects it. Static verification catches constructor signature issues before runtime.

**AC#41: GetAvailableItems calls ItemStock (Pos)**
- **Test**: Grep pattern=`ItemStock\(` path=Era.Core/Shop/ShopSystem.cs
- **Expected**: At least 1 match
- **Rationale**: Completes stub replacement triple AC for GetAvailableItems (AC#33 = negative stub removal, AC#41 = positive implementation call). GetAvailableItems must iterate items and call ItemStock() to determine availability. Without this positive AC, GetAvailableItems could be replaced with `Result.Ok(emptyList)` and still pass AC#33.

**AC#42: Purchase delegates to ItemPurchase (Pos)**
- **Test**: Grep pattern=`ItemPurchase _itemPurchase` path=Era.Core/Shop/ShopSystem.cs
- **Expected**: At least 1 match
- **Rationale**: Verifies Purchase method receives ItemPurchase via constructor injection. Completes stub replacement triple AC for Purchase (AC#34 = negative stub removal, AC#42 = positive delegation verification). ShopSystem receives ItemPurchase parameter in constructor and stores it as field `_itemPurchase` for delegation. Without this positive AC, Purchase could return `Result.Ok(default)` and still pass AC#34.

**AC#43: InventoryManager delegates to GetItem (behavioral verification)**
- **Test**: Grep pattern=`GetItem\(` path=Era.Core/Shop/InventoryManager.cs
- **Expected**: At least 1 match
- **Rationale**: AC#35-37 verify InventoryManager exists, implements IInventoryManager, and is DI-registered, but none verify behavioral correctness. AC#36 rationale describes HasItem→GetItem delegation, but a hardcoded `return new Result(true)` would pass all 3 ACs. This AC verifies GetItem is actually called in InventoryManager.cs, ensuring HasItem delegates to IItemVariables rather than returning hardcoded values. Completes stub replacement triple AC pattern (existence → declaration → behavioral call).

**AC#44: InventoryManager delegates to SetItem (AddItem behavioral verification)**
- **Test**: Grep pattern=`SetItem\(` path=Era.Core/Shop/InventoryManager.cs
- **Expected**: At least 1 match
- **Rationale**: Completes behavioral delegation verification for InventoryManager. AC#43 verifies HasItem→GetItem, but AddItem→SetItem has no verification. A hardcoded `return Result.Ok(Unit.Default)` in AddItem would pass AC#35-37 and AC#43 without calling IItemVariables.SetItem. Parallels AC#43 pattern for write-path delegation.

**AC#45: ICommonFunctions injected into ItemPurchase (Pos)**
- **Test**: Grep pattern=`ICommonFunctions` path=Era.Core/Shop/ItemPurchase.cs
- **Expected**: At least 1 match (constructor parameter or field declaration)
- **Rationale**: Ensures HasVagina (AC#17) is satisfied via proper DI injection, not hardcoded. ItemPurchase calls `_common.HasVagina(gender)` for item 74 gender check (SHOP_ITEM.ERB:198,205). This AC verifies the dependency injection is present. Parallels AC#21 (IVariableStore), AC#28 (IInputHandler), AC#22 (IEngineVariables on ItemDescriptions).

### Goal Coverage Verification

| Goal Item | Description | Covering AC(s) |
|:---------:|-------------|:---------------:|
| 1 | Replace ItemBuy() stub with complete C# implementation | AC#1, AC#2, AC#20, AC#25, AC#26, AC#28, AC#29, AC#30, AC#31, AC#33, AC#34, AC#40, AC#41, AC#42 |
| 2 | Implement all 47 item description handlers as dictionary-dispatched pattern | AC#5, AC#22 |
| 3 | Add missing item variable access methods (new IItemVariables); verify SetMoney on IEngineVariables (added by F775) | AC#3, AC#4, AC#6, AC#7, AC#8, AC#9, AC#10, AC#11, AC#38, AC#39, AC#43, AC#44 |
| 4 | Inline COMMON.ERB utilities (ITEMSTOCK, CHOICE, PRINT_BASE); use existing HasVagina via ICommonFunctions DI | AC#13, AC#17, AC#24, AC#27, AC#32, AC#45 |
| 5 | Replicate PRINT_ITEM, SHOW_ITEM grid display with SETCOLOR/RESETCOLOR, ITEM_SALES | AC#15, AC#16, AC#19, AC#23, AC#13 |
| 6 | Include equivalence tests and zero technical debt | AC#13, AC#14, AC#18 |
| 7 | Implement [200] item reset handler | AC#20, AC#26 |

---

<!-- fc-phase-4-completed -->
## Technical Design
<!-- Written by: tech-designer (Phase 4) -->

### Approach

The implementation follows a **three-layer segregated interface design** to satisfy Interface Segregation Principle (ISP) while preserving ERB-equivalent behavior:

1. **IEngineVariables Extension (SetMoney)**: Verify `void SetMoney(int)` exists on IEngineVariables (added by F775). This provides MONEY write operations support for 13+ `MONEY -= ITEMPRICE:ITEM_NO` occurrences in SHOP_ITEM.ERB. F776 will call this existing method for item purchase money deductions.

2. **New IItemVariables Interface**: Create a dedicated interface for item array access with 6 methods:
   - `int GetItem(int itemId)` — Read ITEM:ITEM_NO (current item count)
   - `void SetItem(int itemId, int value)` — Write ITEM:ITEM_NO (for `ITEM:ITEM_NO ++`)
   - `int GetItemSales(int itemId)` — Read ITEMSALES:ITEM_NO (availability flags: 0=available, -1=sold out, -2=locked)
   - `void SetItemSales(int itemId, int value)` — Write ITEMSALES:ITEM_NO (for conditional availability logic in @ITEM_SALES)
   - `int GetItemPrice(int itemId)` — Read-only ITEMPRICE:ITEM_NO (from CSV `__UNCHANGEABLE__`)
   - `string GetItemName(int itemId)` — Read-only ITEMNAME:ITEM_NO (from CSV `__UNCHANGEABLE__`)
   - Note: `GetCharacterNo(int)` is on IEngineVariables (moved per ISP — see IEngineVariables Extension block and Key Decision)

3. **ShopSystem.ItemBuy() Replacement**: Replace the NotImplementedException stub (ShopSystem.cs:349-350) with full C# implementation:
   - **File structure**: Create `Era.Core/Shop/ItemPurchase.cs` for main purchase logic, `Era.Core/Shop/ItemDescriptions.cs` for 47 description handlers (dictionary-dispatch pattern)
   - **CALLFORM dispatch**: Map `CALLFORM アイテム説明_{ITEM_NO}` to `Dictionary<int, Action>` with 47 entries (item IDs: 0-17, 19, 20, 23, 24, 27, 35, 36, 40-42, 44-46, 60-64, 70-72, 74, 77, 91-96)
   - **COMMON.ERB inlining**: Inline ITEMSTOCK (15 lines, 6 return paths), CHOICE (16 lines, 2-4 choice UI), PRINT_BASE (3 lines, BAR + PRINTFORML) as private C# methods in ItemPurchase.cs
   - **HAS_VAGINA reuse**: ERB `HAS_VAGINA(chr)` takes character index and internally reads `TALENT:chr:性別`. C# `ICommonFunctions.HasVagina(int genderValue)` takes gender value directly. Caller must bridge: `_common.HasVagina(_variables.GetTalent(characterId, TalentIndex.Gender))` (SHOP_ITEM.ERB:198,205)
   - **RESTART/GOTO mapping**: Use `while(true)` loop at @ITEM_BUY function level with `continue` for RESTART, `break` for return 0, matching C12 constraint and existing ShopSystem.Schedule() pattern (ShopSystem.cs:141-256)
   - **[200] Item Reset handler**: User selects 200 → CHOICE confirmation dialog ("アイテム個数をリセットしますか？") → if confirmed, FOR loop resets ITEMSALES:0-100 = 0, then RESTART. Implement as `ResetAllItemSales()` private method in ItemPurchase.cs
   - **PRINT_ITEM equivalent**: Replicate engine built-in `GetHavingItemsString()` (VariableEvaluator.cs:989-1011) format: "所持アイテム：" prefix, then for each ITEM with count>0 append "{ITEMNAME}({count}) ", or "なし" if no items owned. Single concatenated string output via `IConsoleOutput.PrintLine()` + `NewLine()`
   - **SETCOLOR/RESETCOLOR**: Use injected `IStyleManager.SetColor()` / `ResetColor()` for disabled-item visual feedback (sold out, max stock, insufficient money)

4. **Per-Item Special Logic (items 41, 60-96)**: Implement as switch/if chain with 16 cases:
   - Item 41: Direct purchase (媚薬) -- intercepted BEFORE SELECTCASE (SHOP_ITEM.ERB:415-420). Simple: deduct money, increment ITEM, print message. No quantity selection, no confirmation dialog.
   - Items 60-62: Skill learning (talent assignment + ITEMSALES -1 for one-time purchase)
   - Items 63-64: ABL modification (技巧 +/-1, repeatable)
   - Item 70: HP recovery (character selection loop + BASE:0 += 300)
   - Items 71-72: Gender transformation (futanari add/remove with MAXBASE modifications)
   - Item 73: Talent grant (母乳体質)
   - Item 74: Virginity restoration (TALENT:処女 = 2, requires HAS_VAGINA check)
   - Item 77: Multi-pregnancy level (CFLAG:355 cycle 0-6)
   - Items 91-92: Fertility medicine (排卵誘発剤/ピル) — CHOICE confirmation → SIF RESULT → RESTART (decline path); on confirm path, PRINTL "誰に使いますか？" then character selection loop (starts at 0) with 3 filters: (1) NO:(chr)==149 && CFLAG:75<3 && !TALENT:MASTER:禁断の知識 → CONTINUE, (2) TALENT:妊娠 → CONTINUE, (3) GETBIT(TALENT:2,0) && CFLAG:81!=3 → eligible. Item 91 sets CFLAG:81=2 + TRYCALL 排卵誘発剤追加処理 (no-op per C10). Item 92 sets CFLAG:81=3. Back option uses [999] not [-1]. Note: Lines 248/281 ("誰に使いますか？") are NOT dead code — SIF only affects the immediately next statement (RESTART); PRINTL executes on the confirm path (RESULT=0).
   - Item 93: 恋慕 grant (character selection loop) + FLAG cleanup: when LOCAL:0 == FLAG:貞操帯鍵購入フラグ (LOCAL defaults to 0, so checks FLAG==0), resets FLAG:貞操帯鍵購入フラグ and FLAG:貞操帯鍵有効カウンタ to 0. Also clears NTR/公衆便所/浮気公認 talents.
   - Item 94: NTR conversion (character selection loop, grants NTR talent, clears 恋慕 and 人妻 talents)
   - Item 95: Weakness removal (character selection loop for characters with MASTERの弱味, clears CFLAG:MASTERの弱味)
   - Item 96: Marriage ring (人妻 status grant) — character selection loop starts at index **1** (excludes MASTER). Eligibility: `(TALENT:親愛 || (TALENT:恋慕 && CFLAG:好感度>5000)) && TALENT:人妻==0`. Validation at INPUT_LOOP_96 uses TALENT:RESULT:6 (NTR talent check). NO:(chr) access for character identification.

5. **Bulk Purchase (effective runtime range: items 40, 42-59)**: Implement quantity selection with money cap + mutually exclusive branches. Note: ERB SELECTCASE declares `CASE 40 TO 60, 91, 92,` (trailing comma is syntactic noise per Emuera parser — no additional items matched) but items 41, 60-64, 70-74, 77, 91-96 are all intercepted by prior IF blocks that RESTART before reaching SELECTCASE. The C# implementation should only handle the effective runtime range (40, 42-59).
   - **Item 50 special display**: When ITEM_NO==50, show remaining stock count: `(あと{ITEMSALES:ITEM_NO}個)` (SHOP_ITEM.ERB:425-426)
   - Money cap (always first): `if (result * price > money) result = money / price`
   - **Branch A (limited stock, ITEMSALES nonzero)**: Cap RESULT by ITEMSALES; if RESULT >= ITEMSALES, exhaust stock (ITEMSALES = -1), else deduct from ITEMSALES. Do NOT add to ITEM array (limited-stock items track availability via ITEMSALES only).
   - **Branch B (unlimited, ITEMSALES zero)**: Add RESULT to ITEM; if ITEM > 999, retroactively adjust RESULT = 999 - previousCount and cap ITEM = 999.

6. **@ITEM_SALES Conditional Availability**: Implement as separate `UpdateItemAvailability()` method called before item display:
   - ABL-gated item: Item 41 (媚薬) requires `ABL:MASTER:教養 >= 1`; if already owned (ITEM:41 > 0), set ITEMSALES:41 = -1 (sold out); if 教養 < 1, set ITEMSALES:41 = -2 (locked); otherwise available
   - Talent-gated items: Items 60-62 (sold out after purchase), Item 63 (技巧UP): available (ITEMSALES=0) if ABL:MASTER:技巧 < 5, locked (ITEMSALES=-2) otherwise; Item 64 (技巧DOWN): available if ABL:MASTER:技巧 > 0 (nonzero), locked otherwise (asymmetric thresholds)
   - Skill-locked items (two distinct talent gates per SHOP_ITEM.ERB:544-556):
     - **禁断の知識 gate**: Items where `(id > 70 && id < 89) || id == 90 || id == 95` → require MASTER or ASSI to have 禁断の知識
     - **調合知識 gate**: Items `70, 91, 92, 93, 94` → require MASTER or ASSI to have 調合知識
     - Check pattern: `TALENT:MASTER:{talent} || (GetAssi() > 0 && TALENT:ASSI:{talent})` (SHOP_ITEM.ERB:551,555). Use `IEngineVariables.GetAssi()` for assistant index.
     - Note: Item 89 has no talent gate in the loop (stays ITEMSALES=-2 unless set elsewhere). Item 96 handled separately (line 530, unconditional).
   - Item 96 (marriage ring): Unconditionally available per SHOP_ITEM.ERB:530

7. **@SHOW_ITEM Grid Display**: Implement as `DisplayItemGrid()` in ItemPurchase.cs (co-located with ItemStock and Execute loop that call it):
   - 3-column layout with 28-character padding (PRINTFORM %LOCALS,28,LEFT%)
   - Disabled state coloring via IStyleManager for ITEMSTOCK return values 2 (sold out) and 5 (max stock)
   - Skip items with ITEMSTOCK return values 1 (nonexistent) or 4 (condition unmet)

8. **Equivalence Testing Strategy**:
   - **Unit tests (Era.Core.Tests/Shop/)**: ItemPurchaseTests.cs, ItemDescriptionsTests.cs, ItemStockTests.cs
   - **ITEMSTOCK 6-path coverage**: Test return values 0-5 for available, nonexistent, sold out, insufficient money, condition unmet, max stock
   - **Bulk purchase caps**: Boundary tests for money/stock/999 limits
   - **Per-item handlers**: Smoke tests for representative items (60-62, 71-72, 90-96) verifying talent/ABL modifications
   - **Empty description**: Test item 45 (コンドーム) which has empty @アイテム説明_45 function
   - **Character selection loops**: Test items 70-96 with mock character rosters

9. **Interface Implementation Pattern** (following F790/F793):
   - **Era.Core**: IItemVariables interface + NullItemVariables stub (safe defaults: 0 for int, empty string for string)
   - **engine/**: EngineItemVariables.cs with GlobalStatic variable delegation, registered in DI container
   - **DI registration**: Add IItemVariables parameter to ShopSystem constructor, inject into ItemPurchase instance

10. **Technical Debt Prevention** (C11):
   - Zero TODO/FIXME/HACK markers in all new files
   - All methods have XML doc comments
   - All magic numbers replaced with named constants (e.g., `const int ITEMSALES_SOLD_OUT = -1`)
   - SIF scope for items 91-92 (lines 246-248, 279-281): SIF only affects the immediately next statement (RESTART). Lines 248/281 ("誰に使いますか？") execute on the confirm path and MUST be included in C# migration

11. **Volume Waiver Justification**:
   - Source: 791 ERB lines (559 SHOP_ITEM.ERB + 232 アイテム説明.ERB)
   - Expected C# output: ~600-700 lines across 3 files (ItemPurchase.cs ~400, ItemDescriptions.cs ~200, tests ~200+)
   - Atomicity requirement: Item purchase logic and descriptions are tightly coupled via CALLFORM dispatch. Splitting into separate features would create artificial boundaries and increase integration risk.
   - Single-feature migration enables unified equivalence testing.

This approach satisfies all ACs via static code verification (AC#1-12, 14-22) and unit test execution (AC#13), with zero deferred technical debt.

### AC Coverage

| AC# | How to Satisfy |
|:---:|----------------|
| 1 | Replace `throw new NotImplementedException("ITEM_BUY...")` stub at ShopSystem.cs:349-350 with call to new ItemPurchase.Execute() method. Grep verifies not_matches NotImplementedException. |
| 2 | ItemBuy() delegates to `_itemPurchase.Execute()` via DI-injected field. Grep matches `_itemPurchase\.Execute\(` confirms stub body replaced with field-based delegation. |
| 3 | Pre-satisfied: `void SetMoney(int value)` already exists on IEngineVariables (added by F775). Grep confirms signature match. No implementation action needed. |
| 4 | Verify all 12 Get* methods (GetResult, GetMoney, GetDay, GetMaster, GetAssi, GetCount, GetCharaNum, GetRandom, GetName, GetCallName, GetIsAssi, GetCharacterNo) remain after SetMoney addition and GetCharacterNo ISP move. Grep count_equals verifies 12 Get methods. |
| 5 | Create Dictionary<int, Action> in ItemDescriptions.cs with 47 entries mapping item IDs to description lambdas. Pattern `\[\d+\] = \(` matches dictionary initializer syntax in ItemDescriptions.cs specifically. Grep count_equals = 47. |
| 6 | Create Era.Core/Interfaces/IItemVariables.cs file. Glob verifies file existence. |
| 7 | Add `int GetItem(int itemId)` to IItemVariables.cs. Grep matches signature. |
| 8 | Add `void SetItem(int itemId, int value)` to IItemVariables.cs. Grep matches signature. |
| 9 | Add `int GetItemSales(int itemId)` and `void SetItemSales(int itemId, int value)` to IItemVariables.cs. Grep count_equals = 2 for (Get\|Set)ItemSales pattern. |
| 10 | Add `int GetItemPrice(int itemId)` and `string GetItemName(int itemId)` to IItemVariables.cs (read-only per CSV `__UNCHANGEABLE__`). Grep count_equals = 2. |
| 11 | Add `int GetCharacterNo(int characterIndex)` to IEngineVariables.cs for NO:(chr) access (moved from IItemVariables per ISP). Grep matches signature in IEngineVariables.cs. |
| 12 | ItemPurchase.cs calls `_engineVars.SetMoney(currentMoney - price)` at purchase points (13+ locations in ERB). Grep matches SetMoney call in Shop/ directory. |
| 13 | Create Era.Core.Tests/Shop/ItemPurchaseTests.cs and ItemDescriptionsTests.cs with unit tests covering: (1) ITEMSTOCK 7 code paths (6 distinct return values, both return-1 paths tested separately: ARG>=100 and empty ITEMNAME), (2) bulk purchase caps, (3) per-item talent modifications, (4) empty description handling, (5) character selection loops. ITEMSTOCK tests reside in ItemPurchaseTests.cs (per Task 12 scope). `dotnet test Era.Core.Tests` verifies all tests pass. |
| 14 | Code review: Grep pattern `TODO\|FIXME\|HACK` across Era.Core/Shop/, Era.Core/Interfaces/ expects 0 matches. All new code follows TreatWarningsAsErrors + XML doc comments + named constants. |
| 15 | ItemPurchase.cs or ShopDisplay.DisplayItemGrid() calls `_styleManager.SetColor(COLOR_DISABLED)` and `_styleManager.ResetColor()` for unavailable items. Grep matches SetColor or ResetColor in Shop/. |
| 16 | ItemPurchase.cs contains `UpdateItemAvailability()` method implementing @ITEM_SALES conditional availability logic. Grep matches method name in Shop/. |
| 17 | ItemPurchase.cs calls `ICommonFunctions.HasVagina()` for item 74 gender check (SHOP_ITEM.ERB:198,205). Grep matches HasVagina call in Shop/. |
| 18 | Era.Core.Tests/Shop/ItemPurchaseTests.cs file exists. Glob verifies file presence to prevent vacuous AC#13 pass. |
| 19 | ItemPurchase.cs contains `PrintItemCommand()` private method replicating engine PRINT_ITEM built-in. Grep matches method name in Shop/. |
| 20 | ItemPurchase.cs contains `ResetAllItemSales()` method implementing [200] option that resets ITEMSALES:0-100 to 0 after CHOICE confirmation. Grep matches method name in Shop/. |
| 21 | ItemPurchase.cs declares IVariableStore dependency for per-item TALENT/ABL/BASE/MAXBASE/CFLAG/FLAG operations (77+ calls across items 60-96). Grep matches IVariableStore in ItemPurchase.cs. |
| 22 | ItemDescriptions.cs declares IEngineVariables dependency for items 60-64 CALLNAME:MASTER interpolation. Grep matches IEngineVariables in ItemDescriptions.cs. |
| 23 | ItemPurchase.cs or ShopDisplay.cs contains `DisplayItemGrid()` method implementing @SHOW_ITEM (3-column grid, ITEMSTOCK filtering, disabled-item coloring). Grep matches DisplayItemGrid in Shop/. |
| 24 | ItemPurchase.cs contains `Choice()` private method implementing COMMON.ERB CHOICE utility (2-4 option confirmation dialog, 16 call sites). Grep matches `Choice\(` in ItemPurchase.cs. |
| 25 | ItemPurchase.cs calls `_items.SetItem(itemId, value)` for all ITEM array writes (owned item count mutations). Grep matches SetItem call in Shop/. Parallels AC#12 (SetMoney verification). |
| 26 | ItemPurchase.cs calls `_items.SetItemSales(itemId, value)` for all ITEMSALES flag writes (availability mutations). Grep matches SetItemSales call in Shop/. Parallels AC#12 and AC#25. |
| 27 | ItemPurchase.cs contains `ItemStock()` internal method implementing COMMON.ERB ITEMSTOCK utility (6 return paths, called 6 times in @ITEM_BUY + 4 times in @SHOW_ITEM). Grep matches `ItemStock\(` in ItemPurchase.cs. |
| 28 | ItemPurchase.cs declares IInputHandler dependency for character selection loops (14+ INPUT calls), quantity selection, and confirmation dialogs. Grep matches IInputHandler in ItemPurchase.cs. Parallels AC#21/AC#22 DI verification. |
| 29 | ItemPurchase.cs calls `_items.GetItem(itemId)` for all ITEM array reads (owned item count checks). Grep matches GetItem call in Shop/. Parallels AC#25 (SetItem write verification) completing SSOT read/write symmetry. |
| 30 | ItemPurchase.cs calls `_items.GetItemPrice(itemId)` for all ITEMPRICE reads (price checks in ITEMSTOCK, money deduction in purchase logic, bulk purchase cap verification). Grep matches GetItemPrice call in Shop/. Parallels AC#12 (SetMoney), AC#25 (SetItem), AC#29 (GetItem). |
| 31 | ItemPurchase.cs calls `_items.GetItemName(itemId)` for all ITEMNAME reads (ITEMSTOCK empty-name check, display formatting, description headers). Grep matches GetItemName call in Shop/. Parallels AC#30 (GetItemPrice). |
| 32 | ItemPurchase.cs contains `PrintBase()` private method implementing COMMON.ERB PRINT_BASE utility (3 lines: name display, BAR call, formatted values). Grep matches `PrintBase\(` in ItemPurchase.cs. Completes Goal 4 utility coverage: ITEMSTOCK(AC#27), CHOICE(AC#24), PRINT_BASE(AC#32). |
| 33 | ShopSystem.GetAvailableItems() no longer returns Result.Fail with F776 reference. Replaced with implementation using IItemVariables + ItemStock to build available item list. Grep not_matches confirms stub removal. |
| 34 | ShopSystem.Purchase() no longer returns Result.Fail with F776 reference. Replaced with implementation delegating to ItemPurchase for purchase execution. Grep not_matches confirms stub removal. |
| 35 | Era.Core/Shop/InventoryManager.cs created implementing IInventoryManager using IItemVariables as backing store. Glob verifies file existence. |
| 36 | InventoryManager class implements IInventoryManager interface (HasItem delegates to GetItem > 0, AddItem delegates to SetItem). CharacterId is no-op (ERA ITEM array is global). Grep verifies class declaration. |
| 37 | IInventoryManager registered in ServiceCollectionExtensions.cs DI container, fulfilling F774 Mandatory Handoff obligation. Grep matches IInventoryManager in DI file. |
| 38 | IItemVariables registered in ServiceCollectionExtensions.cs DI container (Task 4: NullItemVariables in Era.Core). Grep matches IItemVariables in DI file. Parallels AC#37 (IInventoryManager DI verification). |
| 39 | EngineItemVariables implements IItemVariables (Task 5: engine-side implementation). Grep verifies class declaration. |
| 40 | ShopSystem constructor accepts IItemVariables dependency (Task 8: pass-through for ItemPurchase construction). Grep verifies parameter presence. |
| 41 | GetAvailableItems calls ItemStock in ShopSystem.cs (positive stub replacement verification). Completes triple AC with AC#33 (negative). |
| 42 | Purchase references ItemPurchase in ShopSystem.cs (positive stub replacement verification). Completes triple AC with AC#34 (negative). |
| 43 | InventoryManager.HasItem delegates to IItemVariables.GetItem. Grep verifies GetItem call presence in InventoryManager.cs. |
| 44 | InventoryManager.AddItem delegates to IItemVariables.SetItem. Grep verifies SetItem call presence in InventoryManager.cs. |

### Key Decisions

| Decision | Options Considered | Selected | Rationale |
|----------|-------------------|----------|-----------|
| Interface segregation for item variables | (A) Extend IEngineVariables with 6 item methods + SetMoney, (B) Create separate IItemVariables interface for item-specific methods + extend IEngineVariables with SetMoney only | **B** | ISP compliance: Item variables are consumed only by shop subsystem (F776-F777), not general engine consumers. Segregation minimizes interface surface area for non-shop code. F790 designed IEngineVariables as scalar-only; extending with array access would violate original intent. |
| SetMoney placement | (A) Add to IItemVariables, (B) Add to IEngineVariables, (C) Create new IMoneyManager | **B** | MONEY is a scalar engine variable (MONEY:0) already returned by IEngineVariables.GetMoney(). Write access naturally extends existing read contract. Consistency with F790 design (scalar variables on IEngineVariables). Option A would fragment money access (read from IEngineVariables, write from IItemVariables). Option C creates unnecessary interface proliferation for single method. |
| CALLFORM dispatch pattern | (A) Dictionary<int, Action>, (B) Switch statement, (C) Reflection-based dynamic invoke | **A** | Dictionary provides O(1) lookup, explicit 47-entry registration (verifiable by AC#5 count_equals), and natural extensibility. Switch requires 47 case labels but performs identically. Reflection (option C) adds runtime overhead and loses compile-time type safety. Dictionary pattern matches existing C# idioms in codebase. |
| COMMON.ERB utilities | (A) Inline as private methods in ItemPurchase.cs, (B) Create shared utility class CommonFunctions, (C) Replicate in each call site | **A** | ITEMSTOCK (15 lines), CHOICE (16 lines), PRINT_BASE (3 lines) are small and SHOP_ITEM.ERB-specific. Option B would add cross-feature coupling (F775/F777 don't use these utilities). Option C (duplication) violates DRY. Inlining keeps ItemPurchase.cs self-contained while preserving readability. |
| Bulk purchase implementation | (A) Single method with money cap + IF/ELSE branches matching ERB, (B) Separate cap methods composed via min(), (C) Builder pattern for purchase limits | **A** | ERB implements money cap first (line 439-440), then mutually exclusive branches: ITEMSALES nonzero (limited stock, no ITEM modification) vs zero (unlimited, ITEM increment + 999 cap). Option A preserves ERB control flow for equivalence. Option B incorrectly assumes 3 sequential min() operations. Option C over-engineers a 15-line procedural block. |
| Item availability logic (@ITEM_SALES) | (A) Execute on every @ITEM_BUY entry, (B) Cache in constructor, (C) Lazy evaluation on first access | **A** | ERB calls ITEM_SALES at function entry (SHOP_ITEM.ERB:7). Availability depends on runtime state (ABL, TALENT, ITEM counts). Caching (B) would require invalidation logic. Lazy evaluation (C) adds complexity. Entry-point execution matches ERB semantics and ensures fresh state. |
| File organization | (A) Single ItemPurchase.cs file (~600 lines), (B) Split into ItemPurchase.cs + ItemDescriptions.cs, (C) Further split per-item handlers into ItemHandlers.cs | **B** | ItemDescriptions.cs (47 dictionary entries, ~200 lines) is cohesive and independently testable. Option A creates monolithic file. Option C fragments per-item logic across files without cohesion gain. B balances file size (~300 lines each) with logical separation (purchase flow vs description text). |
| Character NO access | (A) Add GetCharacterNo to IEngineVariables, (B) Add to IItemVariables, (C) Add to ICharacterDataAccess | **A** | NO:(chr) is a general character-identity property used in 20+ ERB files beyond shop context (多生児パッチ.ERB, INFO.ERB, etc.). IEngineVariables already has 3 character-index-scoped methods (GetName, GetCallName, GetIsAssi). Option A maintains ISP: non-shop consumers access GetCharacterNo without depending on IItemVariables. Option B forces ISP violation for future non-shop consumers. Option C requires new interface without clear scope. |
| Empty description handling (item 45) | (A) No-op lambda `[45] = () => {}`, (B) Omit from dictionary, check ContainsKey before invoke, (C) Null dictionary entry with null-check | **A** | ERB defines empty @アイテム説明_45 function. No-op lambda preserves 1:1 function correspondence, simplifies dispatch (no conditional checks), and makes empty descriptions explicit. Option B adds runtime branching. Option C uses null as magic value. |
| GETBIT equivalent | (A) Inline bitwise AND where needed, (B) Create GetBit helper, (C) Use existing ICommonFunctions if available | **A** | SHOP_ITEM.ERB uses GETBIT at lines 254, 262 for talent bit checking. C# equivalent is `(value & (1 << bit)) != 0`. Inline expression is idiomatic C# and avoids helper proliferation for 2 call sites. Option B over-abstracts. Option C requires verifying GETBIT exists in ICommonFunctions (not found in C:\Era\erakoumakanNTR\Era.Core\Interfaces\ICommonFunctions.cs). |
| TRYCALL 排卵誘発剤追加処理 handling | (A) No-op (C10 pattern), (B) Extensibility hook via optional delegate, (C) NotImplementedException stub | **A** | SHOP_ITEM.ERB:273 calls TRYCALL 排卵誘発剤追加処理 (optional patch). C10 constraint mandates no-op for dead TRYCALL functions. Option B adds unused extensibility. Option C would fail if called. Following ShopSystem.PatchVerUp() pattern (ShopSystem.cs:342). |
| IInventoryManager/IItemVariables layering | (A) IItemVariables = low-level engine bridge, IInventoryManager = high-level domain facade using IItemVariables, (B) Consolidate into single interface, (C) Leave as independent parallel interfaces | **A** | IItemVariables provides raw engine variable access (int-based, mirroring ERA arrays). IInventoryManager provides domain-typed operations (CharacterId, ItemId, Result<T>). InventoryManager implementation delegates to IItemVariables for actual variable access. Two-layer design follows ISP: low-level consumers (engine bridge) use IItemVariables; high-level consumers (game logic) use IInventoryManager. Option B loses ISP benefits. Option C creates responsibility ambiguity. |
| IShopSystem parameter mapping | (A) Validate and enforce all strongly-typed parameters, (B) Ignore ShopId and CharacterId (ERA-native semantics), map ItemId.Value to int, (C) Validate ItemId only | **B** | ERA has a single item shop (SHOP_ITEM.ERB has no shop parameter) and a global ITEM array (no per-character inventory). ShopId is ignored (ERA concept: all items in one shop). CharacterId is ignored (same rationale as AC#36 InventoryManager no-op). ItemId.Value maps directly to int itemId for IItemVariables calls. This is consistent with InventoryManager.CharacterId handling (AC#36 rationale). |

### Interfaces / Data Structures

**New Interface: IItemVariables** (Era.Core/Interfaces/IItemVariables.cs)

```csharp
namespace Era.Core.Interfaces;

/// <summary>
/// Interface for accessing ERA item-related variables.
/// Provides typed access to item arrays (ITEM, ITEMSALES, ITEMPRICE, ITEMNAME)
/// and character NO property.
/// Feature 776 - Items (SHOP_ITEM.ERB + アイテム説明.ERB)
/// </summary>
public interface IItemVariables
{
    /// <summary>Get ITEM value (owned item count) by item ID</summary>
    int GetItem(int itemId);

    /// <summary>Set ITEM value (owned item count) by item ID</summary>
    void SetItem(int itemId, int value);

    /// <summary>
    /// Get ITEMSALES value (availability flag) by item ID.
    /// Values: 0=available, -1=sold out, -2=locked (condition unmet), >0=limited stock count
    /// </summary>
    int GetItemSales(int itemId);

    /// <summary>Set ITEMSALES value (availability flag) by item ID</summary>
    void SetItemSales(int itemId, int value);

    /// <summary>Get ITEMPRICE value (item price) by item ID (read-only CSV data)</summary>
    int GetItemPrice(int itemId);

    /// <summary>Get ITEMNAME value (item display name) by item ID (read-only CSV data)</summary>
    string GetItemName(int itemId);
}
```

**IEngineVariables Extension**

Add to existing Era.Core/Interfaces/IEngineVariables.cs (after GetMoney):

```csharp
/// <summary>Set MONEY value (player money, stored in MONEY:0)</summary>
void SetMoney(int value);

/// <summary>
/// Get character NO value (CSV number) by 0-based character index.
/// Maps runtime character index to CSV registration number.
/// Moved from IItemVariables per ISP: NO:(chr) is used in 20+ ERB files beyond shop context.
/// </summary>
int GetCharacterNo(int characterIndex);
```

**Null Stub: NullItemVariables** (Era.Core/Interfaces/NullItemVariables.cs)

```csharp
namespace Era.Core.Interfaces;

/// <summary>
/// Null implementation of IItemVariables for testing/stub scenarios.
/// Returns safe default values (0 for int, -2/LOCKED for ItemSales, empty string for string).
/// Feature 776 - Items (SHOP_ITEM.ERB + アイテム説明.ERB)
/// </summary>
internal sealed class NullItemVariables : IItemVariables
{
    public int GetItem(int itemId) => 0;
    public void SetItem(int itemId, int value) { }
    public int GetItemSales(int itemId) => -2; // ITEMSALES_LOCKED: tests must explicitly set up availability
    public void SetItemSales(int itemId, int value) { }
    public int GetItemPrice(int itemId) => 0;
    public string GetItemName(int itemId) => string.Empty;
}
```

**ShopSystem Constructor Update**

Add IItemVariables parameter:

```csharp
public ShopSystem(
    ShopDisplay display,
    IVariableStore variables,
    IConsoleOutput console,
    IGameState gameState,
    IInputHandler inputHandler,
    IEngineVariables engineVars,
    IStringVariables stringVariables,
    IStyleManager styleManager,
    ICommonFunctions commonFunctions,
    IItemVariables itemVariables,        // NEW
    ItemDescriptions itemDescriptions)   // NEW (DI-injected for ItemPurchase)
{
    // ... existing null checks ...
    _itemVariables = itemVariables ?? throw new ArgumentNullException(nameof(itemVariables));
    _itemDescriptions = itemDescriptions ?? throw new ArgumentNullException(nameof(itemDescriptions));
}
```

**ItemPurchase.cs Structure** (Era.Core/Shop/ItemPurchase.cs)

```csharp
namespace Era.Core.Shop;

public class ItemPurchase
{
    private readonly IItemVariables _items;
    private readonly IEngineVariables _engine;
    private readonly IConsoleOutput _console;
    private readonly IInputHandler _input;
    private readonly IVariableStore _variables;
    private readonly IStyleManager _style;
    private readonly ICommonFunctions _common;
    private readonly ItemDescriptions _descriptions;

    // Constructor with DI
    public ItemPurchase(
        IItemVariables items,
        IEngineVariables engine,
        IConsoleOutput console,
        IInputHandler input,
        IVariableStore variables,
        IStyleManager style,
        ICommonFunctions common,
        ItemDescriptions descriptions)  // DI-injected (not new'd internally)
    {
        _items = items;
        _engine = engine;
        _console = console;
        _input = input;
        _variables = variables;
        _style = style;
        _common = common;
        _descriptions = descriptions;
    }

    /// <summary>
    /// @ITEM_BUY: Main item purchase loop.
    /// Migrated from SHOP_ITEM.ERB lines 1-470.
    /// </summary>
    public void Execute()
    {
        while (true) // RESTART loop
        {
            UpdateItemAvailability(); // CALL ITEM_SALES
            PrintItemCommand();       // PRINT_ITEM
            // ... purchase logic ...
        }
    }

    // Private methods: ItemStock, Choice, PrintBase, UpdateItemAvailability, DisplayItemGrid, per-item handlers
}
```

**ItemDescriptions.cs Structure** (Era.Core/Shop/ItemDescriptions.cs)

```csharp
namespace Era.Core.Shop;

public class ItemDescriptions
{
    private readonly IConsoleOutput _console;
    private readonly IEngineVariables _engine;
    private readonly Dictionary<int, Action> _descriptions;

    public ItemDescriptions(IConsoleOutput console, IEngineVariables engine)
    {
        _console = console;
        _engine = engine;
        _descriptions = new Dictionary<int, Action>
        {
            [0] = () => _console.PrintLine("コマンド[ローター]を使用できるようになる"),
            [1] = () => _console.PrintLine("コマンド[Ｅマッサージャ]を使用できるようになる"),
            // ... items 2-46 (static descriptions) ...
            [60] = () => _console.PrintLine($"{_engine.GetCallName(_engine.GetMaster())}に素質【汚れ無視】を取得させる"),
            [61] = () => { _console.PrintLine($"{_engine.GetCallName(_engine.GetMaster())}に素質【調合知識】を取得させる"); _console.PrintLine(""); _console.PrintLine("この素質がないと買えないアイテムがある。"); },
            [62] = () => { _console.PrintLine($"{_engine.GetCallName(_engine.GetMaster())}に素質【禁断の知識】を取得させる"); _console.PrintLine(""); _console.PrintLine("この素質がないと買えないアイテムがある。"); },
            [63] = () => _console.PrintLine($"{_engine.GetCallName(_engine.GetMaster())}の技巧を上げる"),
            [64] = () => _console.PrintLine($"{_engine.GetCallName(_engine.GetMaster())}の技巧を下げる"),
            // ... items 70-96 (static descriptions) ...
            [96] = () => _console.PrintLine("対象に使用すると人妻状態にできる"),
        };
    }

    public void Display(int itemId)
    {
        if (_descriptions.TryGetValue(itemId, out var description))
            description();
        // else: no-op for undefined items (defensive)
    }
}
```

**Constants** (Era.Core/Shop/ItemPurchase.cs or separate ShopConstants.cs)

```csharp
// ITEMSALES flags
private const int ITEMSALES_AVAILABLE = 0;
private const int ITEMSALES_SOLD_OUT = -1;
private const int ITEMSALES_LOCKED = -2;

// ITEMSTOCK return codes
private const int ITEMSTOCK_AVAILABLE = 0;
private const int ITEMSTOCK_NONEXISTENT = 1;
private const int ITEMSTOCK_SOLD_OUT = 2;
private const int ITEMSTOCK_INSUFFICIENT_MONEY = 3;
private const int ITEMSTOCK_CONDITION_UNMET = 4;
private const int ITEMSTOCK_MAX_STOCK = 5;

// ITEMSTOCK thresholds (COMMON.ERB:14,24)
private const int ITEMSTOCK_ITEM_ID_MAX = 100;    // ARG >= 100 → nonexistent
private const int ITEMSTOCK_MAX_COUNT = 99;        // ITEM >= 99 → max stock (NOT 999; 999 is bulk purchase cap)

// Item IDs
private const int ITEM_SKILL_STENCH_RESISTANCE = 60;
private const int ITEM_SKILL_ALCHEMY = 61;
private const int ITEM_SKILL_FORBIDDEN = 62;
// ... etc ...
```

### Upstream Issues

| Issue | Upstream Section | Suggested Fix |
|-------|-----------------|---------------|
| Volume exceeds 300-line engine type guideline | Feasibility Assessment | Document volume waiver: 791 ERB lines → ~600-700 C# lines across 3 files. Atomicity justification: Item purchase + descriptions are tightly coupled via CALLFORM dispatch. Splitting would create artificial feature boundaries and increase integration risk. Single-feature migration enables unified equivalence testing. |
| AC count (44) exceeds erb type guideline (8-15) | AC Definition Table | Volume waiver extends to AC count: 20 AC Design Constraints from 791-line ERB source require individual verification. ISP enforcement adds per-method ACs (AC#7-11). Inlined utility pattern-consistency adds AC#16,19,20,23,24,27. SSOT read/write verification adds AC#12,25,26,29,30,31. DI injection/registration verification adds AC#21,22,28,38,39,40. F774 handoff stubs add AC#33-34 with positive ACs AC#41-42. IInventoryManager implementation adds AC#35-37,43,44 for behavioral delegation verification (HasItem→GetItem and AddItem→SetItem). Reducing AC count would sacrifice traceability or leave verification gaps. |

---

<!-- fc-phase-5-completed -->
## Tasks

| Task# | AC# | Description | Tag | Status |
|:-----:|:---:|-------------|:---:|:------:|
| 1 | 4,6,7,8,9,10,11 | Create IItemVariables interface in Era.Core/Interfaces/ with 6 methods (GetItem, SetItem, GetItemSales, SetItemSales, GetItemPrice, GetItemName). Also add GetCharacterNo(int) to IEngineVariables.cs (AC#11, moved per ISP; changes AC#4 count to 12). Update existing implementors: NullEngineVariables.cs (return 0) and EngineVariables.cs (scaffolding stub returning 0) to implement GetCharacterNo. | | [x] |
| 2 | 3,4 | Verify SetMoney(int) already exists on IEngineVariables (added by F775) - no code change needed | | [x] |
| 3 | 6 | Create NullItemVariables stub implementation in Era.Core/Interfaces/ (safe defaults: 0 for int, empty string for string) | | [x] |
| 4 | 13,38 | Register IItemVariables → NullItemVariables in Era.Core DI (ServiceCollectionExtensions.cs AddEraCore method, following NullEngineVariables pattern) | | [x] |
| 5 | 13,39 | Create EngineItemVariables.cs in engine/ with GlobalStatic variable delegation for all 6 IItemVariables methods (GetItem, SetItem, GetItemSales, SetItemSales, GetItemPrice, GetItemName). ITEM/ITEMSALES/ITEMPRICE/ITEMNAME use named arrays (DataIntegerArray). GetCharacterNo is on IEngineVariables (separate). | | [x] |
| 6 | 13 | Verify EngineVariables.cs already implements SetMoney (added by F775) - no code change needed | | [x] |
| 7 | 13 | Register IItemVariables → EngineItemVariables in engine DI container | | [x] |
| 8 | 13,40 | Add ItemPurchase parameter to ShopSystem constructor (DI-injected). Remove pass-through of IStyleManager, ICommonFunctions, IItemVariables, ItemDescriptions — these are injected directly into ItemPurchase via DI. | | [x] |
| 9 | 5,22 | Create ItemDescriptions.cs in Era.Core/Shop/ with Dictionary<int, Action> containing all 47 description handlers (item IDs: 0-17, 19, 20, 23, 24, 27, 35, 36, 40-42, 44-46, 60-64, 70-72, 74, 77, 91-96) | | [x] |
| 10a | 21,24,27,28,32,45 | Create ItemPurchase.cs scaffold with constructor (DI: IItemVariables, IEngineVariables, IConsoleOutput, IInputHandler, IVariableStore, IStyleManager, ICommonFunctions, ItemDescriptions) and inlined utilities: ItemStock (6 return paths), Choice (2-4 option confirmation), PrintBase (name+BAR+values) | | [x] |
| 10b | 19,20 | Add PrintItemCommand (PRINT_ITEM equivalent: "所持アイテム" display) and ResetAllItemSales ([200] handler: ITEMSALES:0-100 reset to 0) to ItemPurchase.cs | | [x] |
| 10c | 15,16,23 | Add DisplayItemGrid (@SHOW_ITEM: 3-column layout, 28-char padding, SETCOLOR/RESETCOLOR for disabled items) and UpdateItemAvailability (@ITEM_SALES: ABL-gated, talent-gated, skill-locked items) to ItemPurchase.cs | | [x] |
| 10d | 12,17,25,26,29,30,31 | Add per-item handlers to ItemPurchase.cs: simple items (41, 60-64), HP recovery (70), gender transform (71-72), talent grants (73, 74 with HasVagina), multi-pregnancy (77), fertility medicine (91-92 with GETBIT), 恋慕/NTR/weakness/marriage (93-96) | | [x] |
| 10e | 1,2 | Add Execute() main @ITEM_BUY loop to ItemPurchase.cs: while(true) loop with UpdateItemAvailability, PrintItemCommand, DisplayItemGrid, input dispatch (SELECTCASE equivalent), single-item purchase (0-39), bulk purchase (40,42-59), per-item special handlers | | [x] |
| 11 | 1,2,33,34,41,42 | Replace ItemBuy() NotImplementedException stub in ShopSystem.cs with call to `_itemPurchase.Execute()` (field-based delegation). Implement IShopSystem.GetAvailableItems (using IItemVariables + ItemStock) and IShopSystem.Purchase (delegating to ItemPurchase) — replacing F774 deferred stubs. | | [x] |
| 12 | 13,18 | Create Era.Core.Tests/Shop/ItemPurchaseTests.cs with unit tests covering: (1) ITEMSTOCK 7 code paths (both return-1 paths: ARG>=100 and ITEMNAME empty), (2) bulk purchase branches (money cap + limited stock vs unlimited + branch A invariant: ITEM NOT modified for limited stock), (3) per-item handlers (41, 60-62, 63-64, 70, 71-72, 73, 74, 77, 91-96), (4) UpdateItemAvailability logic, (5) while(true) loop correctness, (6) single-item purchase (items 0-39: assert ITEM incremented by 1, assert ITEMSALES decremented by 1, assert MONEY reduced by ITEMPRICE — three separate verifiable outcomes), (7) [200] item reset (ITEMSALES:0-100 reset to 0), (8) GETBIT equivalence (verify bitwise gender check for items 91-92 eligibility: TALENT:2 bit 0 via `(value & (1 << bit)) != 0`), (9) item 89 stays locked after UpdateItemAvailability (ITEMSALES=-2 remains unchanged even when MASTER has both 禁断の知識 and 調合知識 talents; prevents off-by-one in range condition), (10) ShopSystem.GetAvailableItems returns correct ShopItem list (filters unavailable items via ItemStock, constructs ShopItem with correct ItemId/Name/Price), (11) ShopSystem.Purchase returns PurchaseResult with correct remaining balance after deducting item price, (12) UpdateItemAvailability asymmetric thresholds for items 63-64 (技巧=0: 63 available/64 locked; 技巧=5: 63 locked/64 available; 技巧=3: both available) | | [x] |
| 13 | 13 | Create Era.Core.Tests/Shop/ItemDescriptionsTests.cs with unit tests verifying: (1) all 47 handlers registered, (2) Display(itemId) invokes correct handler, (3) empty description no-op (item 45), (4) undefined item defensive handling | | [x] |
| 14 | 13 | Run dotnet test Era.Core.Tests and verify all tests pass | | [x] |
| 15 | 14 | Remove all TODO/FIXME/HACK comments from new and modified files in Era.Core/Shop/, Era.Core/Interfaces/ | | [x] |
| 16 | 1,2,3,4,5,6,7,8,9,10,11,12,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45 | Run all static verification ACs (AC#1-12, 14-45) via ac-static-verifier and verify PASS | | [x] |
| 17 | 35,36,37,43,44 | Create InventoryManager.cs implementing IInventoryManager (HasItem, AddItem) using IItemVariables as backing store; register IInventoryManager → InventoryManager in DI (ServiceCollectionExtensions.cs). Fulfills F774 Mandatory Handoff obligation. | | [x] |

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
| 1 | implementer | sonnet | Tasks 1-4: IItemVariables interface creation + NullItemVariables stub + Era.Core DI registration (SetMoney already exists via F775) | Interface files, ServiceCollectionExtensions.cs updated, Build SUCCESS |
| 2 | implementer | sonnet | Tasks 5-7: Engine-side implementations + DI registration | EngineItemVariables.cs, EngineVariables.cs updated, Build SUCCESS |
| 3 | implementer | sonnet | Task 8: ShopSystem constructor update | ShopSystem.cs updated, Build SUCCESS |
| 4 | implementer | sonnet | Task 9: ItemDescriptions.cs with 47 handlers | ItemDescriptions.cs, Build SUCCESS |
| 5 | implementer | sonnet | Tasks 10a-10e: ItemPurchase.cs (scaffold+utilities, PrintItemCommand+ResetAllItemSales, DisplayItemGrid+UpdateItemAvailability, per-item handlers, Execute main loop) | ItemPurchase.cs (~400 lines), Build SUCCESS |
| 6 | implementer | sonnet | Task 11: ItemBuy stub replacement | ShopSystem.cs updated, Build SUCCESS |
| 7 | implementer | sonnet | Tasks 12-13: Unit tests | ItemPurchaseTests.cs, ItemDescriptionsTests.cs, Build SUCCESS |
| 8 | implementer | sonnet | Task 14: Test execution | dotnet test Era.Core.Tests → All tests PASS |
| 9 | implementer | sonnet | Task 15: Debt cleanup | All TODO/FIXME/HACK removed |
| 10 | implementer | sonnet | Task 17: InventoryManager + IInventoryManager DI | InventoryManager.cs, ServiceCollectionExtensions.cs updated, Build SUCCESS |
| 11 | ac-static-verifier | - | Task 16: Static AC verification | All ACs PASS |

### Execution Order

Tasks 1-4 → Tasks 5-7 → Task 8 → Tasks 9, 10a-10e → Task 11 → Tasks 12-13 → Task 14 → Task 15 → Task 17 → Task 16

**Critical Dependencies**:
- Tasks 5-7 depend on Task 1 (IItemVariables interface must exist)
- Task 6 depends on Task 2 (SetMoney interface must exist)
- Task 8 depends on Tasks 1-7 (all interfaces and implementations must exist)
- Tasks 9, 10a-10e depend on Task 1 (interfaces must exist for DI injection)
- Task 11 depends on Tasks 9, 10a-10e (ItemPurchase class must exist)
- Tasks 12-13 depend on Tasks 9-11 (implementation must exist to test)
- Task 17 depends on Tasks 1, 4 (IItemVariables interface and DI registration pattern must exist)
- Task 16 depends on Task 17 (InventoryManager must exist for AC#35-37,43,44 verification)

### Build Verification Steps

After each Phase:
1. Run `dotnet build Era.Core` via WSL (MSYS_NO_PATHCONV=1 wsl -- bash -c 'cd /mnt/c/Era/erakoumakanNTR && /home/siihe/.dotnet/dotnet build Era.Core')
2. Verify exit code 0 (no warnings/errors per TreatWarningsAsErrors)
3. After Phase 8, run `dotnet test Era.Core.Tests` via WSL
4. Verify all tests pass before proceeding to Phase 9

### Success Criteria

- All ACs in PASS state
- Zero TODO/FIXME/HACK comments in new files
- All unit tests pass
- Build produces zero warnings

### Error Handling

| Error | Action |
|-------|--------|
| Interface compilation error | STOP → Report to user with file:line |
| Engine-side implementation missing GlobalStatic method | STOP → Investigate engine variable access pattern |
| Test failure in ItemPurchase | Debug via Era.Core.Tests, fix implementation |
| AC FAIL in static verification | Review AC Details rationale, fix implementation or update AC constraint |

---

## Mandatory Handoffs

<!-- CRITICAL: Handoff without actionable Task = TBD violation -->
<!-- Option A (new Feature): MUST add creation Task in Tasks table -->
<!-- Option B (existing Feature): Referenced Feature must exist -->
<!-- Option C (Phase): Phase must exist in architecture.md -->

| Issue | Reason | Destination | Destination ID | Creation Task |
|-------|--------|-------------|----------------|---------------|

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
| 2026-02-18 09:00 | START | initializer | Phase 1 Init | READY:776:erb |
| 2026-02-18 09:05 | START | implementer | Tasks 1,3,4 interfaces | SUCCESS |
| 2026-02-18 09:10 | START | implementer | Tasks 12,13 TDD RED | 26 FAIL (RED) |
| 2026-02-18 09:20 | START | implementer | Tasks 5,7 engine stubs | SUCCESS |
| 2026-02-18 09:25 | START | implementer | Task 8 ShopSystem ctor | SUCCESS |
| 2026-02-18 09:30 | START | implementer | Task 9 ItemDescriptions | SUCCESS |
| 2026-02-18 09:35 | START | implementer | Tasks 10a-10e ItemPurchase | SUCCESS |
| 2026-02-18 09:40 | START | implementer | Task 11 stub replacement | SUCCESS |
| 2026-02-18 09:45 | DEVIATION | dotnet test | Phase 4 GREEN check | 3 FAIL: BulkPurchase_LimitedStock, Skill64, AsymmetricThresholds |
| 2026-02-18 09:46 | START | debugger | Fix 3 test failures | SUCCESS: test assertion fixes (ability index mismatch, sold-out value) |
| 2026-02-18 09:50 | END | dotnet test | All tests | 2120 PASS, 0 FAIL |
| 2026-02-18 10:00 | DEVIATION | Bash | ac-static-verifier --ac-type code | exit code 1 (binary file warnings; actual: 41/41 PASS) |
| 2026-02-18 10:01 | END | ac-static-verifier | code+file ACs | 44/44 PASS (verify-logs OK:44/44) |
| 2026-02-18 10:10 | DEVIATION | feature-reviewer | Phase 8.1 quality review | NEEDS_REVISION: item 77 talent index wrong (TALENT_性別==2 vs TALENT_童貞!=0) |
| 2026-02-18 10:11 | END | debugger | Fix item 77 talent check | SUCCESS: added TALENT_童貞=1, fixed check to !=0 |
| 2026-02-18 10:15 | DEVIATION | feature-reviewer | Phase 8.2 doc-check | NEEDS_REVISION: engine-dev SKILL.md missing IItemVariables + GetCharacterNo |
| 2026-02-18 10:20 | END | implementer | Fix 8.2 DEVIATION | SUCCESS: engine-dev SKILL.md updated (IItemVariables section, GetCharacterNo, interface count 37→38) |
| 2026-02-18 10:25 | START | orchestrator | Phase 8.3 SSOT check | N/A (covered by 8.2 fix) |
| 2026-02-18 10:30 | END | ac-tester | Phase 7 AC verification | 45/45 PASS |
| 2026-02-18 10:30 | END | dotnet test | Phase 7 test verification | 2120 PASS, 0 FAIL |
| 2026-02-18 10:35 | END | ac-static-verifier | Phase 9.2.1 code+file re-verify | 44/44 PASS (verify-logs OK:44/44) |
| 2026-02-18 10:36 | END | orchestrator | Phase 9.8 DEVIATION analysis | 4 DEVIATIONs, all 修正済み |

---

## Review Notes

<!-- Mandatory: All [pending] items must be resolved before /run. -->
<!-- Format: - [pending|resolved-applied|resolved-invalid|resolved-skipped|fix|problem-fix] {phase} {iter}: [{category-code}] {description} -->
<!-- Tag rules: [pending] = awaiting user decision (POST-LOOP). [resolved-applied] = fix applied. [resolved-invalid] = validation rejected. [resolved-skipped] = user explicitly chose skip in POST-LOOP ONLY (orchestrator MUST NOT use autonomously). [fix] = applied fix history (immutable, used by is_loop() for A→B→A detection). -->
<!-- Category codes: See pm/reference/error-taxonomy.md (AC-XXX, CON-XXX, DEP-XXX, etc.) -->

- [fix] Phase2-Review iter1: [AC-002] AC#2 (AC Definition Table) | AC#2 matcher `ItemBuy\(\)` was vacuous (matched stub method signature identically); changed to `new ItemPurchase\(` to verify actual delegation
- [resolved-applied] Phase2-Pending iter1: [CON-002] GetCharacterNo(int characterIndex) may be misplaced on IItemVariables. NO:(chr) is a character-identity property used in non-shop ERBs (多生児パッチ.ERB, INFO.ERB), not exclusive to item context. IEngineVariables already has 3 character-index-scoped methods (GetName, GetCallName, GetIsAssi). Moving to IEngineVariables would fix ISP violation. Requires: AC#4 count 11→12, AC#11 path change, Task 1 update, NullItemVariables update, Technical Design update.
- [fix] Phase2-Review iter1: [AC-005] AC coverage gap for @ITEM_SALES | Added AC#16 (UpdateItemAvailability method exists) to verify C10 conditional availability logic migration
- [fix] Phase2-Review iter2: [FMT-001] feature-776.md:64 | Added missing fc-phase-1-completed marker
- [fix] Phase2-Review iter2: [FMT-001] feature-776.md:277 | Added missing --- separator between Dependencies and AC sections
- [fix] Phase2-Review iter2: [AC-005] AC coverage gap for HasVagina (Goal 4) | Added AC#17 (HasVagina call verification) to prevent hardcoded bypass
- [fix] Phase2-Review iter2: [AC-005] AC#13 catch-all without test file verification | Added AC#18 (ItemPurchaseTests.cs exists) to prevent vacuous test pass
- [fix] Phase2-Review iter3: [FMT-001] feature-776.md:282,422 | Added missing section ownership markers for AC and Technical Design
- [fix] Phase2-Review iter3: [INV-003] C8 + Technical Design section 5 | Corrected bulk purchase design from "3 sequential caps via min()" to mutually exclusive IF/ELSE branches matching ERB SHOP_ITEM.ERB:442-454
- [fix] Phase2-Review iter3: [AC-002] AC#5 path | Narrowed from Era.Core/Shop/ to Era.Core/Shop/ItemDescriptions.cs per Upstream Issues recommendation; removed resolved Upstream Issue
- [fix] Phase2-Review iter4: [CON-002] Technical Design section 4-5 | Added item 41 (媚薬) as pre-SELECTCASE special handler; excluded from bulk purchase range (40,42-60 instead of 40-60)
- [fix] Phase2-Review iter4: [CON-002] Technical Design section 3 | Documented HAS_VAGINA character-to-gender indirection: ERB takes char index, C# HasVagina(genderValue) needs bridging via GetTalent
- [fix] Phase2-Review iter4: [AC-005] C7 + Task 11 | Fixed item 90→91 range for character-targeted items; expanded Task 11 test scope to include 41, 63-64, 70, 73, 74, 77
- [fix] Phase2-Review iter5: [CON-002] Technical Design section 4 | Added item 93 detail: FLAG:貞操帯鍵購入フラグ/有効カウンタ reset logic, LOCAL:0 default semantics, NTR/公衆便所/浮気公認 talent clearing
- [fix] Phase2-Uncertain iter5: [AC-005] Philosophy Derivation | Extended row 2 to include 'description dispatch' (AC#5) and 'inventory management' (AC#7, AC#8) per Philosophy text
- [fix] Phase2-Uncertain iter6: [AC-005] AC Design Constraints C13 | Added AC#19 (PrintItemCommand method exists) to verify PRINT_ITEM equivalent per C13 mandate
- [fix] Phase2-Review iter6: [INV-003] Technical Design section 4 | Corrected item 94 (NTR conversion, not loyalty) and item 95 (MASTERの弱味 removal, not 浮気公認)
- [fix] Phase2-Review iter7: [CON-002] Technical Design section 6 | Added ASSI dual-character talent check for items 70-95 (TALENT:MASTER || TALENT:ASSI fallback) per SHOP_ITEM.ERB:551,555
- [fix] Phase2-Review iter7: [CON-002] Technical Design section 6 | Added item 41 ABL:MASTER:教養 availability gate per SHOP_ITEM.ERB:533-541
- [fix] Phase2-Review iter7: [CON-002] C5 + Constants | Documented ITEMSTOCK max stock threshold as 99 (not 999); added ITEMSTOCK_MAX_COUNT and ITEMSTOCK_ITEM_ID_MAX constants
- [fix] Phase2-Review iter8: [AC-005] Goal + AC Constraints + ACs + Technical Design + Tasks | Added [200] アイテムリセット handler (SHOP_ITEM.ERB:18-26): C19 constraint, AC#20 (ResetAllItemSales), Goal item 7, Technical Design [200] handler section, Task 9/11/15 updates
- [fix] Phase2-Review iter8: [AC-005] AC Constraints + Task 11 | Added C20 single-item purchase path (CASE 0 TO 39: ITEM++, ITEMSALES--, MONEY deduction); Task 11 test case (6)
- [fix] Phase2-Review iter8: [AC-005] Philosophy Derivation row 1 | Extended AC Coverage from AC#1,2,5 to include AC#12,13,15,16,19,20 for complete functional path traceability
- [fix] Phase2-Review iter9: [CON-002] ItemDescriptions class | Added IEngineVariables as constructor parameter; items 60-64 now use _engine.GetCallName(_engine.GetMaster()) for CALLNAME:MASTER interpolation
- [fix] Phase2-Review iter9: [AC-005] Goal Coverage + AC Coverage tables | Added Goal 7 row (AC#20) and AC#20 row (ResetAllItemSales) to close traceability gaps
- [fix] Phase2-Review iter10: [CON-002] Technical Design section 6 | Split ITEM_SALES skill-locked items into two distinct talent gates: 禁断の知識 (items 71-88,90,95) and 調合知識 (items 70,91-94) per SHOP_ITEM.ERB:544-556
- [fix] Phase2-Review iter10: [AC-005] AC Definition Table + AC Details + AC Coverage + Tasks | Added AC#21 (IVariableStore injected into ItemPurchase) for 77+ TALENT/ABL/BASE/MAXBASE/CFLAG/FLAG operations verification
- [fix] Phase2-Review iter11: [INV-003] Technical Design section 4 items 91-92 | Corrected from 'sleep depth (熟睡 toggle)' to fertility medicine (排卵誘発剤/ピル): CFLAG:81 state, NO:149 filter, TALENT:妊娠 exclusion, GETBIT gender check, dead code at lines 248/281
- [fix] Phase2-Review iter11: [CON-002] Technical Design section 4 item 96 | Added loop-start-at-1 (excludes MASTER), eligibility condition (親愛 || (恋慕 && 好感度>5000)) && 人妻==0, TALENT:6 NTR check
- [fix] Phase2-Review iter11: [FMT-001] Review Notes | Added [{category-code}] to all [fix] entries per template format specification
- [fix] Phase2-Review iter12: [FMT-001] Scope Reference section | Demoted ## Scope Reference to ### Source Files under Background (removed non-template H2 section + extra --- separator)
- [fix] Phase2-Review iter12: [CON-002] C5 ITEMSTOCK detail | Added second return-1 path (!STRLENS(ITEMNAME:ARG)) to C5 constraint; updated verification to cover both nonexistent paths
- [fix] Phase2-Review iter12: [CON-002] Task 11 test case (2) | Added bulk purchase branch A invariant (ITEM NOT modified for limited stock) to explicit test requirements
- [fix] Phase2-Review iter13: [AC-005] Tasks table | Added Task 4 (NullItemVariables DI registration in ServiceCollectionExtensions.cs AddEraCore, following NullEngineVariables pattern)
- [fix] Phase2-Review iter13: [AC-005] AC Definition Table + AC Details + AC Coverage + Tasks | Added AC#22 (IEngineVariables injected into ItemDescriptions) for items 60-64 CALLNAME:MASTER interpolation verification
- [fix] Phase2-Review iter14: [INV-003] Implementation Contract + Technical Design | Corrected stale 'All 15 ACs' to 'All ACs' (now 23 ACs after iterative additions)
- [fix] Phase2-Review iter14: [AC-005] AC Definition Table + AC Details + AC Coverage + Goal 5 + Task 9/15 | Added AC#23 (DisplayItemGrid method exists) for @SHOW_ITEM grid display verification
- [fix] Phase2-Review iter15: [INV-003] Background + Goal + RCA + AC#3 + Tasks 2,5 + Technical Design + Dependencies + Feasibility + Risks | SetMoney already exists on IEngineVariables (added by F775); updated stale references throughout, marked AC#3 and Tasks 2,5 as pre-satisfied
- [fix] Phase2-Review iter15: [CON-002] Task 11 test case (6) | Strengthened single-item purchase test description to require three separate assertions (ITEM +1, ITEMSALES -1, MONEY -ITEMPRICE)
- [fix] Phase1-RefCheck iter1: [REF-001] Links section | Added F780 (Phase 20 sibling) to Links section per reference-checker
- [fix] Phase2-Review iter1: [FMT-001] Tasks table | Renumbered Task# 3.5 to integer sequence (3.5→4, 4→5, ..., 15→16); updated Implementation Contract, Execution Order, Critical Dependencies
- [fix] Phase2-Review iter1: [FMT-001] Background Source Files | Added non-template subsection note for Source Files under Background
- [fix] Phase2-Review iter1: [INV-003] AC Coverage AC#3 | Updated How to Satisfy text to reflect pre-satisfied status (was implying implementation action)
- [fix] Phase2-Uncertain iter1: [AC-005] Task 12 test case (8) | Added GETBIT equivalence test case for items 91-92 bitwise gender check (TALENT:2 bit 0)
- [fix] Phase2-Uncertain iter2: [AC-005] Philosophy Derivation + AC Definition Table + AC Details + AC Coverage + Goal Coverage + Tasks 10,16 | Added AC#25 (SetItem called) and AC#26 (SetItemSales called) to verify ITEM/ITEMSALES writes go through IItemVariables interface (SSOT enforcement paralleling AC#12 SetMoney)
- [fix] Phase2-Review iter2: [INV-003] Technical Design section 4 + Technical Constraints + section 10 | Corrected SIF dead code mischaracterization: lines 248/281 (誰に使いますか？) are NOT dead code — SIF only affects next statement (RESTART); PRINTL executes on confirm path (RESULT=0). Updated items 91-92 description, Technical Constraints, and debt prevention section.
- [fix] Phase2-Uncertain iter2: [INV-003] Technical Design section 5 | Corrected bulk purchase effective runtime range from (40, 42-60, 91-92) to (40, 42-59); items 41, 60-64, 70-74, 77, 91-96 all intercepted by prior IF blocks. Added item 50 special display note (remaining stock count).
- [fix] Phase2-Uncertain iter3: [INV-003] Technical Design section 5 | Added trailing comma to SELECTCASE quote (`CASE 40 TO 60, 91, 92,`) matching actual ERB; documented as syntactic noise per Emuera parser
- [fix] Phase2-Review iter4: [DEP-001] Related Features + Dependencies | Updated F775 status from [WIP] to [DONE] (confirmed via feature-775.md and index-features.md); updated description to reflect SetMoney already added
- [fix] Phase2-Review iter3: [FMT-001] Links section F647 | Changed from [Related: F647] to [Predecessor: F647] to match Dependencies table type
- [fix] Phase2-Review iter3: [AC-005] Philosophy Derivation | Added 8 missing ACs (AC#9,10,11,17,21,22,23,24) to Philosophy Derivation rows for complete traceability
- [fix] Phase2-Uncertain iter3: [AC-005] AC#27 ItemStock | Added AC#27 (ItemStock method exists) for pattern consistency with AC#19,20,23,16,24; updated Goal 4 coverage, Philosophy row 1, AC Coverage, Tasks 10,16
- [fix] Phase2-Review iter3: [INV-003] Technical Design section 3 PRINT_ITEM | Corrected format spec to match engine GetHavingItemsString(): "所持アイテム：{name}({count}) " prefix format, not bracket-ID format
- [fix] Phase2-Review iter5: [INV-003] AC Coverage #13 | Removed ItemStockTests.cs from AC#13 How to Satisfy (no Task creates it; ITEMSTOCK tests reside in ItemPurchaseTests.cs per Task 12)
- [fix] Phase2-Uncertain iter5: [AC-005] AC#28 IInputHandler DI | Added AC#28 (IInputHandler injected into ItemPurchase) for 14+ INPUT operations; updated Goal 1 coverage, AC Coverage, Tasks 10,16
- [fix] Phase2-Review iter4: [FMT-001] AC Design Constraints | Added 9 missing Constraint Details blocks (C6, C9, C10, C11, C12, C13, C14, C15, C17) for template completeness
- [fix] Phase2-Review iter4: [FMT-001] Upstream Issues | Added AC count waiver (27 ACs vs 8-15 guideline) with justification: 20 constraints, ISP per-method ACs, utility pattern-consistency, SSOT write verification
- [fix] Phase2-Review iter6: [AC-005] AC#29 GetItem read | Added AC#29 (GetItem called in purchase logic) completing SSOT read/write symmetry; updated Philosophy Derivation, Goal Coverage, AC Coverage, Tasks 10,16
- [fix] Phase2-Review iter7: [AC-005] AC#32 PrintBase | Added AC#32 (PrintBase method exists) completing Goal 4 COMMON.ERB utility AC coverage: ITEMSTOCK(AC#27), CHOICE(AC#24), PRINT_BASE(AC#32)
- [fix] Phase2-Review iter5: [FMT-001] Upstream Issues | Updated stale AC count from (27) to (29→31 after AC#30,31 additions)
- [fix] Phase2-Uncertain iter5: [AC-005] AC#30,31 GetItemPrice/GetItemName | Added SSOT read-call ACs for ITEMPRICE and ITEMNAME reads, completing IItemVariables call-verification symmetry
- [fix] Phase2-Review iter5: [INV-003] AC#13 How to Satisfy | Fixed ITEMSTOCK from "6 return paths" to "7 code paths (6 distinct return values, both return-1 paths)" matching C5 and Task 12
- [fix] Phase2-Review iter8: [CON-002] Technical Constraints | Documented SHOP_ITEM.ERB line 6 `PRINTFORML {ITEMSALES:62}` as excluded from migration (debug/diagnostic output, not user-facing shop logic)
- [resolved-applied] Phase3-Maintainability iter6: [CON-002] IShopSystem stubs (GetAvailableItems, Purchase) at ShopSystem.cs:36,39 explicitly reference F776 but are not covered by F776 Tasks/ACs/Handoffs. Need decision: (A) add to F776 scope, or (B) create Mandatory Handoff to new/existing feature.
- [resolved-applied] Phase3-Maintainability iter6: [CON-002] ShopSystem constructor pass-through: Technical Design proposes adding IStyleManager, ICommonFunctions, IItemVariables to ShopSystem constructor as pass-through to ItemPurchase. Consider DI-resolved ItemPurchase (factory/direct injection) vs pass-through pattern. Also ItemDescriptions hardcoded instantiation inside ItemPurchase breaks DI/testability.
- [resolved-applied] Phase3-Maintainability iter6: [CON-002] Deliverable path SSOT: phase-20-27-game-systems.md:74 says Era.Core/Items/ItemDescriptions.cs but F776 Technical Design says Era.Core/Shop/ItemDescriptions.cs. Need decision: (A) update Phase 20 doc to Era.Core/Shop/ (recommended: tightly coupled to shop), or (B) move to Era.Core/Items/ namespace.
- [fix] Phase3-Maintainability iter6: [CON-002] Task 5 | Added NO variable access pattern note (CharacterData.dataInteger via VariableCode.NO differs from named item arrays)
- [fix] Phase1-RefCheck iter1: [REF-001] Dependencies table | Added F780 as Related (was in Links but missing from Dependencies)
- [resolved-applied] Phase3-Maintainability iter1: [CON-002] F774 Mandatory Handoff defers IInventoryManager DI registration to F776 (feature-774.md:919), but F776 has zero mention of IInventoryManager in Tasks, ACs, Technical Design, or Mandatory Handoffs. Leaked handoff obligation.
- [resolved-applied] Phase3-Maintainability iter1: [CON-002] IInventoryManager (HasItem, AddItem with strong types) and IItemVariables (GetItem, SetItem with raw int) provide overlapping item inventory access at different abstraction levels. F776 must document the layering relationship or consolidate.
- [resolved-applied] Phase3-Maintainability iter1: [CON-002] Task 10 covers 19 ACs and ~400 lines of implementation (Execute, ItemStock, Choice, PrintBase, UpdateItemAvailability, DisplayItemGrid, PrintItemCommand, ResetAllItemSales, per-item handlers). Consider splitting into smaller tasks by functional area.
- [fix] Phase3-Maintainability iter1: [CON-002] NullItemVariables.GetItemSales | Changed default from 0 (available) to -2 (ITEMSALES_LOCKED) to prevent tests from passing without explicit availability setup
- [fix] PostLoop-UserFix iter2: [CON-002] GetCharacterNo ISP | Moved GetCharacterNo from IItemVariables to IEngineVariables: AC#4 count 11→12, AC#11 path changed, Task 1 updated (7→6 methods), NullItemVariables/interface code blocks updated, IEngineVariables Extension block updated
- [fix] PostLoop-UserFix iter2: [CON-002] IShopSystem stubs | Added AC#33 (GetAvailableItems stub removed), AC#34 (Purchase stub removed), updated Task 11 to include IShopSystem implementation, updated Task 16 AC list
- [fix] PostLoop-UserFix iter2: [CON-002] ItemDescriptions DI | Changed from `new ItemDescriptions(console, engine)` to constructor injection; ItemPurchase now receives ItemDescriptions as DI parameter
- [fix] PostLoop-UserFix iter2: [CON-002] Deliverable path SSOT | Updated phase-20-27-game-systems.md:74 from Era.Core/Items/ItemDescriptions.cs to Era.Core/Shop/ItemDescriptions.cs
- [fix] PostLoop-UserFix iter2: [CON-002] IInventoryManager leaked handoff | Added AC#35-37 (InventoryManager file, implements interface, DI registered), Task 17 (create InventoryManager using IItemVariables), fulfilling F774 Mandatory Handoff
- [fix] PostLoop-UserFix iter2: [CON-002] IInventoryManager/IItemVariables layering | Added Key Decision documenting two-layer design: IItemVariables=low-level engine bridge, IInventoryManager=high-level domain facade
- [fix] PostLoop-UserFix iter2: [CON-002] Task 10 split | Split monolithic Task 10 into 10a (scaffold+utilities), 10b (PrintItemCommand+ResetAllItemSales), 10c (DisplayItemGrid+UpdateItemAvailability), 10d (per-item handlers), 10e (Execute main loop)
- [fix] Phase2-Review iter1: [INV-003] AC Coverage #4 | Updated from '11 Get methods' to '12 Get methods' matching AC Definition Table count_equals=12
- [fix] Phase2-Review iter1: [FMT-001] Tasks table | Swapped Task#16 and Task#17 for sequential ordering
- [fix] Phase2-Review iter1: [INV-003] Upstream Issues AC count | Updated from (31) to (37) reflecting current AC Definition Table
- [fix] Phase2-Review iter1: [AC-005] Task 1 AC#11 | Added AC#11 to Task 1 AC# column (GetCharacterNo implementation on IEngineVariables was orphaned)
- [fix] Phase2-Review iter1: [INV-003] Key Decision Character NO | Updated Selected from B (IItemVariables) to A (IEngineVariables) with ISP rationale
- [fix] Phase2-Review iter1: [INV-003] Goal 3 | Updated GetCharacterNo from IItemVariables to IEngineVariables per ISP decision
- [fix] Phase2-Review iter2: [INV-003] Technical Design section 2 | Removed GetCharacterNo from IItemVariables method list (already on IEngineVariables)
- [fix] Phase2-Review iter2: [AC-005] Task 1 AC#4 | Added AC#4 to Task 1 AC# column (GetCharacterNo changes Get method count affecting AC#4)
- [fix] Phase2-Review iter2: [INV-003] AC#36 InventoryManager | Added CharacterId semantics note (ERA ITEM array is global, CharacterId is no-op parameter)
- [fix] Phase2-Review iter3: [AC-005] AC Coverage table | Added AC#33-37 How to Satisfy rows for GetAvailableItems/Purchase stubs, InventoryManager, DI registration
- [fix] Phase2-Review iter3: [FMT-001] Implementation Contract | Added Phase 11 for Task 17 (InventoryManager), updated Execution Order and Critical Dependencies
- [fix] Phase2-Review iter3: [INV-003] Implementation Contract Phase 5 | Updated from 'Task 10' to 'Tasks 10a-10e' matching split sub-tasks
- [fix] Phase2-Review iter3: [INV-003] ShopSystem Constructor | Added ItemDescriptions parameter to constructor code block per DI injection decision
- [fix] Phase3-Maintainability iter1: [CON-002] Task 8 scope | Updated Task 8 description to include IStyleManager, ICommonFunctions, ItemDescriptions as pass-through dependencies for ItemPurchase construction
- [fix] Phase3-Maintainability iter1: [AC-005] AC#38 IItemVariables DI | Added AC#38 (IItemVariables DI registered) paralleling AC#37 (IInventoryManager DI); updated Task 4 AC#, Task 16 AC list, Goal Coverage, AC Coverage, Upstream Issues
- [fix] Phase2-Review iter2: [INV-003] AC#37 path | Corrected AC#37 Method path from Era.Core/ServiceCollectionExtensions.cs to Era.Core/DependencyInjection/ServiceCollectionExtensions.cs (actual file location); updated AC Details Test line
- [resolved-applied] Phase3-Maintainability iter3: [CON-002] ItemPurchase DI vs new — ItemPurchase is instantiated via DI injection into ShopSystem constructor. ShopSystem receives ItemPurchase via DI and delegates to it via field `_itemPurchase`. AC#2 changed to verify `_itemPurchase.Execute()` (field-based delegation). AC#42 changed to verify `ItemPurchase _itemPurchase` parameter (constructor injection). Pass-through dependencies removed per user decision to use DI for ItemPurchase.
- [resolved-applied] Phase3-Maintainability iter3: [CON-002] ItemPurchase.cs ~400 lines — User chose single-class (Option A): ERB atomicity preserved, 16 handlers average 15 lines sharing same DI deps, splitting creates artificial boundaries. Volume waiver documented in Upstream Issues.
- [fix] Phase3-Maintainability iter3: [INV-003] Phase 20 deliverable path | Updated phase-20-27-game-systems.md from Era.Core/Items/ItemManager.cs to Era.Core/Shop/InventoryManager.cs matching F776 design
- [fix] Phase3-Maintainability iter3: [AC-005] AC#39,40 | Added AC#39 (EngineItemVariables implements IItemVariables) and AC#40 (ShopSystem constructor accepts IItemVariables) for Tasks 5,8 static verification; updated Task AC# columns, Task 16 list, Goal Coverage, AC Coverage, Upstream Issues
- [fix] Phase2-Review iter4: [FMT-001] Constraint Details ordering | Reordered from C1-C5,C7,C8,C16,C18-C20,C6,C9-C15,C17 to sequential C1-C20
- [fix] Phase2-Review iter4: [AC-005] AC#41,42 stub triple AC | Added positive ACs for GetAvailableItems (ItemStock call) and Purchase (ItemPurchase delegation) completing stub replacement triple AC pattern for AC#33,34; updated Task 11 AC#, Task 16 list, Goal Coverage, AC Coverage, Upstream Issues
- [fix] Phase2-Uncertain iter4: [INV-003] AC#4 description | Updated from 'existing methods preserved (incl. GetCharacterNo)' to 'Get methods total 12 (11 existing + GetCharacterNo added)' for accuracy
- [fix] Phase2-Review iter5: [AC-005] Philosophy Derivation row 1 | Added AC#28 (IInputHandler) to row 1 AC Coverage (missing from complete migration traceability)
- [fix] Phase2-Review iter1: [AC-005] AC#43 InventoryManager behavioral | Added AC#43 (GetItem called in InventoryManager.cs) for behavioral delegation verification; updated Task 17, Task 16, Goal Coverage, AC Coverage, Upstream Issues, Philosophy Derivation
- [fix] Phase2-Uncertain iter1: [CON-002] AC#35 rationale | Added note clarifying IInventoryManager is not consumed within F776 scope but fulfills F774 handoff for downstream features
- [fix] Phase3-Maintainability iter2: [CON-002] DisplayItemGrid location | Resolved ambiguity in Technical Design section 7: committed to ItemPurchase.cs (matching Task 10c definition)
- [resolved-applied] Phase2-Review iter3: [CON-002] ItemStock accessibility — User chose Option A: ItemStock made internal on ItemPurchase, ShopSystem.GetAvailableItems calls _itemPurchase.ItemStock(id).
- [fix] Phase2-Review iter3: [AC-005] Task 12 item 89 test | Added test case (9) for item 89 stays locked after UpdateItemAvailability (negative verification preventing off-by-one in talent gate range)
- [fix] Phase2-Review iter3: [AC-005] AC#42 vacuousness | Annotated DI pending item with AC#2/AC#42 dependency note (both patterns need updating after DI resolution)
- [fix] Phase2-Review iter4: [INV-003] Phase 20 deliverables | Added ItemPurchase.cs and IItemVariables.cs to phase-20-27-game-systems.md Deliverables table
- [fix] Phase2-Review iter4: [INV-003] Task 1 implementors | Extended Task 1 to include updating NullEngineVariables.cs and EngineVariables.cs for GetCharacterNo
- [fix] Phase2-Review iter5: [AC-005] AC#44 AddItem behavioral | Added AC#44 (SetItem called in InventoryManager.cs) completing AddItem→SetItem delegation verification; updated Task 17, Task 16, Goal Coverage, AC Coverage, Upstream Issues, Philosophy Derivation
- [fix] Phase2-Uncertain iter5: [AC-005] Task 12 ShopSystem tests | Added test cases (10-11) for ShopSystem.GetAvailableItems and Purchase functional correctness
- [fix] Phase2-Review iter5: [INV-003] Items 63-64 asymmetric | Expanded Technical Design section 6 with specific ABL thresholds; added Task 12 test case (12) for asymmetric availability conditions
- [fix] Phase3-Maintainability iter6: [CON-002] IShopSystem parameter mapping | Added Key Decision documenting ShopId/CharacterId ignored, ItemId.Value maps to int (ERA single shop, global ITEM array)
- [fix] Phase3-Maintainability iter6: [AC-005] AC#14 scan path | Expanded from file-specific IItemVariables.cs to directory-scoped Era.Core/Interfaces/ covering all F776-modified interface files
- [fix] Phase3-Maintainability iter1: [INV-003] Implementation Contract Task ordering | Moved Task 17 (InventoryManager) before Task 16 (static AC verification) — Task 16 verifies AC#35-37,43,44 which require InventoryManager to exist; updated Execution Order, Implementation Contract phases, Critical Dependencies
- [resolved-applied] Phase2-Review iter2: [FMT-001] Task# non-integer sub-numbering (10a-10e) — User chose Option B: Accept sub-letter pattern as documented exception. Logical grouping (5 sub-tasks composing single ItemPurchase.cs) justified; template shows integer examples but does not prohibit sub-letters.
- [fix] Phase2-Review iter2: [FMT-001] Extra --- separator | Removed extra horizontal rule after ## Type section (template has no separator between Type and Background)
- [fix] PostLoop-UserFix iter3: [CON-002] ItemPurchase DI | Changed from `new ItemPurchase(...)` to DI-injected field `_itemPurchase`; updated AC#2 matcher to `_itemPurchase\.Execute\(`, AC#42 to `ItemPurchase _itemPurchase`, Task 8 (remove pass-through deps), Task 11 (field delegation)
- [fix] PostLoop-UserFix iter3: [CON-002] ItemPurchase SRP | User confirmed single-class (~400 lines) with ERB atomicity justification; volume waiver documented
- [fix] PostLoop-UserFix iter3: [CON-002] ItemStock accessibility | Changed ItemStock from private to internal on ItemPurchase; ShopSystem.GetAvailableItems calls _itemPurchase.ItemStock(id)
- [fix] Phase2-Review iter1: [INV-003] AC#2 Definition Table + AC Coverage stale | Updated AC#2 Expected from `new ItemPurchase\(` to `_itemPurchase\.Execute\(` and AC Coverage How to Satisfy to match DI decision
- [fix] Phase2-Review iter1: [AC-005] AC#45 ICommonFunctions DI | Added AC#45 (ICommonFunctions injected into ItemPurchase) completing DI injection verification pattern; updated Task 10a, Task 16, Philosophy Derivation, Goal Coverage, AC Coverage

---

<!-- fc-phase-6-completed -->

## Links

- [Predecessor: F647](feature-647.md) - Phase 20 Planning (parent)
- [Predecessor: F774](feature-774.md) - Shop Core (ItemBuy stub origin)
- [Related: F775](feature-775.md) - Collection (sibling, SetMoney overlap)
- [Related: F777](feature-777.md) - Customization (sibling)
- [Predecessor: F788](feature-788.md) - IConsoleOutput Extensions
- [Related: F789](feature-789.md) - IStringVariables
- [Predecessor: F790](feature-790.md) - IEngineVariables base
- [Related: F791](feature-791.md) - IGameState mode transitions
- [Related: F780](feature-780.md) - Genetics & Growth (Phase 20 sibling)
- [Related: F793](feature-793.md) - GameStateImpl delegation pattern
